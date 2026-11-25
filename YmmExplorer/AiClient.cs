// AI(Claude・Gemini・ChatGPT・OpenAI 互換・Local AI Studio)とのやりとり。
// YmmExplorer と YMM4 マネージャーで同じものを使います(YMM4 の内部には触れません)。
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace YmmExplorer;

/// <summary>AI に送る「画像以外のファイル」(PDF・動画・音声)。AI の種類ごとに送り方を変える</summary>
public sealed class AiFilePart
{
    public string Mime { get; init; } = "";
    /// <summary>中身(base64)。Uri と、どちらか一方</summary>
    public string? Base64 { get; init; }
    /// <summary>Gemini にアップロードしたファイルの URI</summary>
    public string? Uri { get; init; }
    public string FileName { get; init; } = "";
    public string Caption { get; init; } = "";
}

// ====================================================================== APIキーの保存(Windows の DPAPI で暗号化。このPC・このユーザーでしか戻せない)
public static class SecretBox
{
    [StructLayout(LayoutKind.Sequential)]
    struct DATA_BLOB { public int cbData; public IntPtr pbData; }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CryptProtectData(ref DATA_BLOB pDataIn, string? desc, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, ref DATA_BLOB pDataOut);
    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool CryptUnprotectData(ref DATA_BLOB pDataIn, IntPtr desc, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, ref DATA_BLOB pDataOut);
    [DllImport("kernel32.dll")]
    static extern IntPtr LocalFree(IntPtr h);

    static byte[]? Run(byte[] input, bool protect)
    {
        var inBlob = new DATA_BLOB { cbData = input.Length, pbData = Marshal.AllocHGlobal(input.Length) };
        var outBlob = new DATA_BLOB();
        try
        {
            Marshal.Copy(input, 0, inBlob.pbData, input.Length);
            bool ok = protect
                ? CryptProtectData(ref inBlob, "YmmExplorer", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 1, ref outBlob)
                : CryptUnprotectData(ref inBlob, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 1, ref outBlob);
            if (!ok) return null;
            var result = new byte[outBlob.cbData];
            Marshal.Copy(outBlob.pbData, result, 0, outBlob.cbData);
            return result;
        }
        catch { return null; }
        finally
        {
            Marshal.FreeHGlobal(inBlob.pbData);
            if (outBlob.pbData != IntPtr.Zero) LocalFree(outBlob.pbData);
        }
    }

    public static string Protect(string plain)
    {
        if (string.IsNullOrEmpty(plain)) return "";
        var b = Run(Encoding.UTF8.GetBytes(plain), true);
        return b == null ? "" : Convert.ToBase64String(b);
    }

    public static string Unprotect(string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return "";
        try
        {
            var b = Run(Convert.FromBase64String(stored), false);
            return b == null ? "" : Encoding.UTF8.GetString(b);
        }
        catch { return ""; }
    }
}

// ====================================================================== AI の種類
public sealed class AiProviderInfo
{
    public AiProviderInfo(string id, string name, string defaultModel, string keyHint, string keyUrl, params string[] knownModels)
    { Id = id; Name = name; DefaultModel = defaultModel; KeyHint = keyHint; KeyUrl = keyUrl; KnownModels = knownModels; }
    /// <summary>一覧をまだ読んでいない時に、▼ に出しておくモデル(読んだら、サーバーの一覧に置き換える)</summary>
    public IReadOnlyList<string> KnownModels { get; }
    public string Id { get; }
    public string Name { get; }
    public string DefaultModel { get; }
    public string KeyHint { get; }
    public string KeyUrl { get; }
    public override string ToString() => Name;
}

/// <summary>1回のやりとりの中身(文章と画像)</summary>
public sealed class AiTurn
{
    public string Role { get; init; } = "user";       // user / assistant
    public string Text { get; init; } = "";
    public List<(string mime, string base64, string caption)> Images { get; init; } = new();
    /// <summary>画像以外のファイル(PDF・動画・音声)。送れる AI にだけ送る</summary>
    public List<AiFilePart> Files { get; init; } = new();
}

/// <summary>Claude・Gemini・ChatGPT(OpenAI)・OpenAI互換(ローカルAIなど)へ送る</summary>
public static class AiClient
{
    public static readonly IReadOnlyList<AiProviderInfo> Providers = new[]
    {
        new AiProviderInfo("claude", "Claude(Anthropic)", "claude-sonnet-5-5", "sk-ant-…", "https://platform.claude.com/settings/keys",
            "claude-sonnet-5-5", "claude-opus-5-5", "claude-haiku-5-5", "claude-fable-5-2"),
        new AiProviderInfo("gemini", "Gemini(Google)", "gemini-3.5-flash-lite", "AIza…", "https://aistudio.google.com/apikey",
            "gemini-3.5-flash-lite"),
        new AiProviderInfo("openai", "ChatGPT(OpenAI)", "gpt-4o", "sk-…", "https://platform.openai.com/api-keys",
            "gpt-4o", "gpt-4o-mini"),
        new AiProviderInfo("compatible", "OpenAI 互換(ローカルAI・その他)", "", "(要らなければ空欄)", ""),
        new AiProviderInfo("localstudio", "Local AI Studio(このPCのAI・無料)", "qwen3-4b", "(APIキーは要りません)", "",
            "qwen3-4b", "qwen3-06b", "gemma3-4b", "llama32-3b", "mistral-7b", "deepseek-r1-7b"),
    };

    /// <summary>APIキーが要る AI か(OpenAI 互換と Local AI Studio は要らない)</summary>
    public static bool NeedsKey(string provider) => provider is not ("compatible" or "localstudio");

    /// <summary>モデルの性能・重さの説明(Local AI Studio のモデルだけ。ほかは空)</summary>
    public static string ModelNote(string provider, string model)
        => provider == "localstudio" ? LocalAiStudio.Describe(model) : "";

    static readonly HttpClient http = new() { Timeout = TimeSpan.FromMinutes(5) };

    /// <summary>
    /// AI に送って、返事を受け取る。web = true で、AI がインターネットを検索・閲覧できる
    /// (Claude・Gemini は AI 側の検索、ChatGPT・OpenAI 互換・Local AI Studio は、このソフト/Local AI Studio が検索して渡す)
    /// </summary>
    public static async Task<string> SendAsync(string provider, string model, string key, string baseUrl, string system,
                                               IReadOnlyList<AiTurn> turns, CancellationToken ct, bool json = false, bool web = false)
    {
        var reply = provider switch
        {
            "localstudio" => await LocalStudioAsync(model, system, turns, ct, web),
            "claude" => await ClaudeAsync(model, key, system, turns, ct, web),
            "gemini" => await GeminiAsync(model, key, system, turns, ct, json, web),
            _ => await OpenAiAsync(provider == "openai" ? "https://api.openai.com/v1" : baseUrl, model, key, system, turns, provider == "openai", ct, web),
        };
        return CleanReply(reply);
    }

    static readonly System.Text.RegularExpressions.Regex ThinkBlock = new(@"<think(?:ing)?>.*?(?:</think(?:ing)?>|\z)",
        System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    /// <summary>
    /// 返事から、画面に出さなくてよいものを取り除く: 考え中の文(&lt;think&gt;…&lt;/think&gt;。小さなモデルが返事に混ぜることがある)
    /// </summary>
    public static string CleanReply(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        // 「…考え…</think>本文」のように、始まりの印が無い時は、終わりの印までを捨てる
        var close = text.LastIndexOf("</think>", StringComparison.OrdinalIgnoreCase);
        if (close >= 0 && text.IndexOf("<think>", StringComparison.OrdinalIgnoreCase) < 0) text = text.Substring(close + 8);
        text = ThinkBlock.Replace(text, "");
        return text.Trim();
    }

    /// <summary>処理中のお知らせ(「🔎 検索しています…」「生成中 45%・残り 約 0:12・GPU 87%」など。画面の下に出す)</summary>
    public static event Action<string>? Progress;
    static void Report(string text) { try { Progress?.Invoke(text); } catch { } }

    /// <summary>
    /// 送る。AI が混んでいる(503・529・502・500)・回数の上限(429)の時は、少し待って自動でやり直す(最大3回)。
    /// エラーは、日本語の説明をつけて返す
    /// </summary>
    static async Task<string> PostAsync(HttpRequestMessage req, CancellationToken ct)
    {
        var bodyText = req.Content != null ? await req.Content.ReadAsStringAsync(ct) : "";
        var mediaType = req.Content?.Headers.ContentType?.MediaType ?? "application/json";
        int[] waits = { 3, 8, 20 };
        for (int attempt = 0; ; attempt++)
        {
            // 同じリクエストは2回送れないので、作り直す
            using var copy = new HttpRequestMessage(req.Method, req.RequestUri) { Content = new StringContent(bodyText, Encoding.UTF8, mediaType) };
            foreach (var h in req.Headers) copy.Headers.TryAddWithoutValidation(h.Key, h.Value);

            using var res = await http.SendAsync(copy, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            if (res.IsSuccessStatusCode) return body;

            int code = (int)res.StatusCode;
            string detail = body;
            try
            {
                var j = JsonNode.Parse(body);
                detail = j?["error"]?["message"]?.GetValue<string>() ?? j?["error"]?.ToString() ?? body;
            }
            catch { }
            if (detail.Length > 400) detail = detail.Substring(0, 400) + "…";

            bool noCredit = detail.Contains("credit", StringComparison.OrdinalIgnoreCase) || detail.Contains("quota", StringComparison.OrdinalIgnoreCase)
                            || detail.Contains("billing", StringComparison.OrdinalIgnoreCase) || detail.Contains("insufficient", StringComparison.OrdinalIgnoreCase);
            bool retryable = code is 500 or 502 or 503 or 504 or 529 || (code == 429 && !noCredit);
            if (retryable && attempt < waits.Length)
            {
                var wait = res.Headers.RetryAfter?.Delta?.TotalSeconds is double ra && ra > 0 && ra < 60 ? ra : waits[attempt];
                Retrying?.Invoke($"AI が混んでいるため、{wait:0} 秒後にやり直します…({attempt + 1}/{waits.Length})");
                await Task.Delay(TimeSpan.FromSeconds(wait), ct);
                continue;
            }
            throw new InvalidOperationException(Explain(code, detail, noCredit) + $"\n\n(AI からの返事 {code}: {detail})");
        }
    }

    /// <summary>
    /// Local AI Studio(このPCのAI)へ送る。画像・ファイルは送れないので、何が付いていたかだけを文で伝える
    /// </summary>
    static async Task<string> LocalStudioAsync(string model, string system, IReadOnlyList<AiTurn> turns, CancellationToken ct, bool web)
    {
        // 画像は、いちばん新しい質問のものだけ送る(このPCのAIには重いので、8枚まで)。
        // 画像を見られないモデルの時は、Local AI Studio が外して、付いていたことを AI に伝える
        int lastUser = -1;
        for (int i = turns.Count - 1; i >= 0; i--) if (turns[i].Role != "assistant") { lastUser = i; break; }
        var messages = turns.Select((t, i) =>
        {
            var text = t.Text;
            if (t.Files.Count > 0) text += $"\n\n(PDF・動画・音声などのファイルが {t.Files.Count} 個付いていましたが、このPCのAIは読めないため省きました)";
            IReadOnlyList<string>? images = null;
            if (i == lastUser && t.Images.Count > 0)
            {
                images = t.Images.Take(8).Select(x => x.base64).ToList();
                var captions = t.Images.Take(8).Select((x, n) => x.caption.Length > 0 ? $"画像{n + 1}: {x.caption}" : "").Where(c => c.Length > 0).ToList();
                if (captions.Count > 0) text += "\n\n(添付の画像)\n" + string.Join("\n", captions);
                if (t.Images.Count > 8) text += $"\n(画像は {t.Images.Count} 枚ありましたが、最初の 8 枚だけ送りました)";
            }
            else if (t.Images.Count > 0) text += $"\n\n(この時は画像が {t.Images.Count} 枚付いていました)";
            return (t.Role, text, images);
        }).ToList();
        if (string.IsNullOrWhiteSpace(model)) model = "qwen3-4b";
        var progress = new Progress<StudioProgress>(p => Report("Local AI Studio: " + p.Text));
        return await LocalAiStudio.ChatAsync(model, system, messages, ct, web, progress);
    }

    /// <summary>そのAIで使えるモデルの一覧を、AI のサーバーから読む</summary>
    public static async Task<List<string>> ListModelsAsync(string provider, string key, string baseUrl, CancellationToken ct)
    {
        if (provider == "localstudio")
        {
            var models = await LocalAiStudio.ModelsAsync("chat", ct);
            var ready = models.Where(m => m.Installed).Select(m => m.Id).ToList();
            if (ready.Count == 0)
                throw new InvalidOperationException("Local AI Studio に、インストール済みのチャットモデルがありません。Local AI Studio の「モデルライブラリ」でインストールしてください。");
            return ready;
        }
        var list = new List<string>();
        HttpRequestMessage req;
        switch (provider)
        {
            case "claude":
                req = new HttpRequestMessage(HttpMethod.Get, "https://api.anthropic.com/v1/models?limit=100");
                req.Headers.Add("x-api-key", key);
                req.Headers.Add("anthropic-version", "2023-06-01");
                break;
            case "gemini":
                req = new HttpRequestMessage(HttpMethod.Get, "https://generativelanguage.googleapis.com/v1beta/models?pageSize=200");
                req.Headers.Add("x-goog-api-key", key);
                break;
            default:
                var b = provider == "openai" ? "https://api.openai.com/v1" : baseUrl;
                req = new HttpRequestMessage(HttpMethod.Get, b.TrimEnd('/') + "/models");
                if (!string.IsNullOrWhiteSpace(key)) req.Headers.Add("Authorization", "Bearer " + key);
                break;
        }
        using (req)
        {
            using var res = await http.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode) throw new InvalidOperationException(Explain((int)res.StatusCode, body, false));
            var j = JsonNode.Parse(body);
            if (provider == "gemini")
            {
                if (j?["models"] is JsonArray ms)
                    foreach (var m in ms)
                    {
                        var methods = m?["supportedGenerationMethods"] as JsonArray;
                        if (methods != null && !methods.Any(x => x?.GetValue<string>() == "generateContent")) continue;
                        var name = m?["name"]?.GetValue<string>() ?? "";
                        if (name.StartsWith("models/")) name = name.Substring(7);
                        if (name.Contains("embedding") || name.Contains("aqa") || name.Contains("imagen") || name.Contains("tts")) continue;
                        list.Add(name);
                    }
            }
            else if (j?["data"] is JsonArray data)
            {
                foreach (var m in data)
                {
                    var id = m?["id"]?.GetValue<string>() ?? "";
                    if (provider == "openai" && !(id.StartsWith("gpt") || id.StartsWith("o") || id.StartsWith("chatgpt"))) continue;
                    if (id.Contains("embedding") || id.Contains("whisper") || id.Contains("tts") || id.Contains("dall-e") || id.Contains("audio") || id.Contains("realtime") || id.Contains("transcribe")) continue;
                    list.Add(id);
                }
            }
        }
        return list.Distinct().OrderByDescending(x => x, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>エラーの中に「代わりにこのモデルを」と書いてあれば、その名前</summary>
    public static string? SuggestedModel(string message)
    {
        var m = System.Text.RegularExpressions.Regex.Match(message, @"use (?:the )?(?:model )?(?:models/)?([A-Za-z0-9][A-Za-z0-9._\-]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!m.Success) return null;
        var name = m.Groups[1].Value.TrimEnd('.');
        return name.Contains('-') || name.Contains('.') ? name : null;
    }

    /// <summary>やり直す時のお知らせ(画面の下に出す)</summary>
    public static event Action<string>? Retrying;

    static string Explain(int code, string detail, bool noCredit)
    {
        if (noCredit) return "APIの残高(クレジット)や、無料で使える範囲がなくなっています。そのAIの請求ページでクレジットを追加するか、別のAI(⚙ 設定)に切り替えてください。";
        return code switch
        {
            400 when detail.Contains("model", StringComparison.OrdinalIgnoreCase) => "モデル名が正しくないか、そのモデルは使えません。⚙ 設定でモデル名を確かめてください。",
            400 => "送った内容が受け付けられませんでした。プレビューの枚数を減らすか、送るものを絞ってもう一度お試しください。",
            401 or 403 => "APIキーが正しくないか、使う権限がありません。⚙ 設定でAPIキーを入れ直してください。",
            404 => "モデルが見つかりません。⚙ 設定でモデル名を確かめてください(新しい名前に変わっていることがあります)。",
            413 => "送る量が多すぎます。プレビューの枚数を減らしてください。",
            429 => "短い時間に送りすぎて、回数の上限に達しました。少し待ってから送ってください。",
            500 or 502 or 503 or 504 or 529 => "AI のサーバーが混んでいるか、一時的に止まっています。3回やり直しましたが、だめでした。少し時間をおくか、⚙ 設定で別のモデル・別のAIに切り替えてください。",
            _ => "AI からエラーが返りました。",
        };
    }

    static async Task<string> ClaudeAsync(string model, string key, string system, IReadOnlyList<AiTurn> turns, CancellationToken ct, bool web = false)
    {
        var messages = new JsonArray();
        foreach (var t in turns)
        {
            var content = new JsonArray();
            foreach (var (mime, b64, caption) in t.Images)
            {
                if (caption.Length > 0) content.Add(new JsonObject { ["type"] = "text", ["text"] = caption });
                content.Add(new JsonObject { ["type"] = "image", ["source"] = new JsonObject { ["type"] = "base64", ["media_type"] = mime, ["data"] = b64 } });
            }
            foreach (var f in t.Files.Where(f => f.Mime == "application/pdf" && f.Base64 != null))
            {
                if (f.Caption.Length > 0) content.Add(new JsonObject { ["type"] = "text", ["text"] = f.Caption });
                content.Add(new JsonObject { ["type"] = "document", ["source"] = new JsonObject { ["type"] = "base64", ["media_type"] = f.Mime, ["data"] = f.Base64 } });
            }
            content.Add(new JsonObject { ["type"] = "text", ["text"] = t.Text });
            messages.Add(new JsonObject { ["role"] = t.Role == "assistant" ? "assistant" : "user", ["content"] = content });
        }
        var body = new JsonObject { ["model"] = model, ["max_tokens"] = 16000, ["system"] = system, ["messages"] = messages };
        if (web)
        {
            // Claude 側の検索(web_search)とページ閲覧(web_fetch)。検索は 1回の質問で最大 5回まで
            body["tools"] = new JsonArray
            {
                new JsonObject { ["type"] = "web_search_20250305", ["name"] = "web_search", ["max_uses"] = 5 },
                new JsonObject { ["type"] = "web_fetch_20250910", ["name"] = "web_fetch", ["max_uses"] = 5 },
            };
            Report("Claude がインターネットを調べながら答えています…");
        }
        var sb = new StringBuilder();
        var sources = new List<WebSource>();
        for (int round = 0; round < 5; round++)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            { Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json") };
            req.Headers.Add("x-api-key", key);
            req.Headers.Add("anthropic-version", "2023-06-01");
            if (web) req.Headers.Add("anthropic-beta", "web-fetch-2025-09-10");
            var res = JsonNode.Parse(await PostAsync(req, ct));
            if (res?["content"] is JsonArray arr)
                foreach (var c in arr)
                {
                    var type = c?["type"]?.GetValue<string>();
                    if (type == "text")
                    {
                        sb.Append(c!["text"]?.GetValue<string>());
                        if (c["citations"] is JsonArray cites)
                            foreach (var ci in cites)
                                if (ci?["url"]?.GetValue<string>() is { Length: > 0 } u) sources.Add(new WebSource(ci["title"]?.GetValue<string>() ?? "", u));
                    }
                    else if (type == "server_tool_use")
                    {
                        var input = c!["input"];
                        Report(input?["url"] != null ? "🌐 Claude がページを読んでいます: " + input["url"] : "🔎 Claude が検索しています: " + input?["query"]);
                    }
                    else if (type == "web_fetch_tool_result" && c!["content"]?["url"]?.GetValue<string>() is { Length: > 0 } fu)
                        sources.Add(new WebSource(c["content"]?["content"]?["title"]?.GetValue<string>() ?? "", fu));
                }
            // 調べるのに時間がかかって途中で止まった時(pause_turn)は、そのまま続けてもらう
            if (res?["stop_reason"]?.GetValue<string>() != "pause_turn" || res["content"] is not JsonArray content) break;
            messages.Add(new JsonObject { ["role"] = "assistant", ["content"] = content.DeepClone() });
        }
        return sb.ToString() + (web ? WebTools.SourcesBlock(sources) : "");
    }

    static async Task<string> GeminiAsync(string model, string key, string system, IReadOnlyList<AiTurn> turns, CancellationToken ct, bool json = false, bool web = false)
    {
        var contents = new JsonArray();
        foreach (var t in turns)
        {
            var parts = new JsonArray();
            foreach (var (mime, b64, caption) in t.Images)
            {
                if (caption.Length > 0) parts.Add(new JsonObject { ["text"] = caption });
                parts.Add(new JsonObject { ["inline_data"] = new JsonObject { ["mime_type"] = mime, ["data"] = b64 } });
            }
            foreach (var f in t.Files)
            {
                if (f.Caption.Length > 0) parts.Add(new JsonObject { ["text"] = f.Caption });
                if (f.Uri != null) parts.Add(new JsonObject { ["file_data"] = new JsonObject { ["mime_type"] = f.Mime, ["file_uri"] = f.Uri } });
                else if (f.Base64 != null) parts.Add(new JsonObject { ["inline_data"] = new JsonObject { ["mime_type"] = f.Mime, ["data"] = f.Base64 } });
            }
            parts.Add(new JsonObject { ["text"] = t.Text });
            contents.Add(new JsonObject { ["role"] = t.Role == "assistant" ? "model" : "user", ["parts"] = parts });
        }
        var body = new JsonObject
        {
            ["system_instruction"] = new JsonObject { ["parts"] = new JsonArray(new JsonObject { ["text"] = system }) },
            ["contents"] = contents,
            ["generationConfig"] = json ? new JsonObject { ["responseMimeType"] = "application/json" } : new JsonObject { ["maxOutputTokens"] = 16384 },
        };
        if (web && !json)
        {
            // Google 検索と、URL のページを読む機能
            body["tools"] = new JsonArray { new JsonObject { ["google_search"] = new JsonObject() }, new JsonObject { ["url_context"] = new JsonObject() } };
            Report("Gemini が Google 検索で調べながら答えています…");
        }
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent";
        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json") };
        req.Headers.Add("x-goog-api-key", key);
        var res = JsonNode.Parse(await PostAsync(req, ct));
        var sb = new StringBuilder();
        if (res?["candidates"]?[0]?["content"]?["parts"] is JsonArray parts2)
            foreach (var p in parts2) if (p?["text"] is JsonNode tx) sb.Append(tx.GetValue<string>());
        if (sb.Length == 0 && res?["promptFeedback"]?["blockReason"] is JsonNode br) return "(AI が答えませんでした: " + br + ")";
        if (web && res?["candidates"]?[0]?["groundingMetadata"]?["groundingChunks"] is JsonArray chunks)
        {
            var sources = new List<WebSource>();
            foreach (var ch in chunks)
                if (ch?["web"]?["uri"]?.GetValue<string>() is { Length: > 0 } u) sources.Add(new WebSource(ch["web"]?["title"]?.GetValue<string>() ?? "", u));
            sb.Append(WebTools.SourcesBlock(sources));
        }
        return sb.ToString();
    }

    static async Task<string> OpenAiAsync(string baseUrl, string model, string key, string system, IReadOnlyList<AiTurn> turns, bool allowFiles, CancellationToken ct, bool web = false)
    {
        if (string.IsNullOrWhiteSpace(baseUrl)) throw new InvalidOperationException("OpenAI 互換の AI の URL を入れてください(例: http://localhost:11434/v1)");
        var messages = new JsonArray { new JsonObject { ["role"] = "system", ["content"] = system } };
        foreach (var t in turns)
        {
            var pdfs = allowFiles ? t.Files.Where(f => f.Mime == "application/pdf" && f.Base64 != null).ToList() : new List<AiFilePart>();
            if ((t.Images.Count == 0 && pdfs.Count == 0) || t.Role == "assistant")
            {
                messages.Add(new JsonObject { ["role"] = t.Role == "assistant" ? "assistant" : "user", ["content"] = t.Text });
                continue;
            }
            var content = new JsonArray();
            foreach (var (mime, b64, caption) in t.Images)
            {
                if (caption.Length > 0) content.Add(new JsonObject { ["type"] = "text", ["text"] = caption });
                content.Add(new JsonObject { ["type"] = "image_url", ["image_url"] = new JsonObject { ["url"] = $"data:{mime};base64,{b64}" } });
            }
            foreach (var f in pdfs)
                content.Add(new JsonObject { ["type"] = "file", ["file"] = new JsonObject { ["filename"] = f.FileName, ["file_data"] = $"data:{f.Mime};base64,{f.Base64}" } });
            content.Add(new JsonObject { ["type"] = "text", ["text"] = t.Text });
            messages.Add(new JsonObject { ["role"] = "user", ["content"] = content });
        }
        var body = new JsonObject { ["model"] = model, ["messages"] = messages };
        if (!web)
        {
            var res = JsonNode.Parse(await PostChatAsync(baseUrl, key, body, ct));
            return res?["choices"]?[0]?["message"]?["content"]?.GetValue<string>() ?? "";
        }

        // インターネットを使う: このソフトが検索・ページ閲覧の道具を用意して、AI に使ってもらう(function calling)
        messages[0] = new JsonObject { ["role"] = "system", ["content"] = system + "\n\n" + WebTools.SystemNote };
        body["tools"] = WebTools.ToolsJson();
        var sources = new List<WebSource>();
        for (int round = 0; round < 7; round++)
        {
            if (round == 6) body.Remove("tools");   // 最後は、調べた内容で答えてもらう
            JsonNode? res;
            try { res = JsonNode.Parse(await PostChatAsync(baseUrl, key, body, ct)); }
            catch (InvalidOperationException ex) when (round == 0 && body.ContainsKey("tools") && ex.Message.Contains("tool", StringComparison.OrdinalIgnoreCase))
            {
                // 道具を使えない AI: 先に質問で検索して、結果を一緒に渡す
                body.Remove("tools");
                Report("🔎 インターネットで調べています…");
                var question = turns.LastOrDefault(t => t.Role != "assistant")?.Text ?? "";
                var research = await WebTools.QuickResearchAsync(question.Length > 600 ? question.Substring(question.Length - 600) : question, sources, ct);
                messages.Insert(1, new JsonObject { ["role"] = "system", ["content"] = "インターネットで調べた結果です。これをもとに答えてください。\n\n" + research });
                res = JsonNode.Parse(await PostChatAsync(baseUrl, key, body, ct));
            }
            var msg = res?["choices"]?[0]?["message"];
            if (msg?["tool_calls"] is not JsonArray calls || calls.Count == 0 || !body.ContainsKey("tools"))
                return (msg?["content"]?.GetValue<string>() ?? "") + WebTools.SourcesBlock(sources);
            messages.Add(msg.DeepClone());
            foreach (var call in calls.Take(4))
            {
                var name = call?["function"]?["name"]?.GetValue<string>() ?? "";
                var args = call?["function"]?["arguments"]?.GetValue<string>() ?? "{}";
                Report(WebTools.Describe(name, args));
                var result = await WebTools.RunToolAsync(name, args, sources, ct);
                messages.Add(new JsonObject { ["role"] = "tool", ["tool_call_id"] = call?["id"]?.GetValue<string>() ?? "", ["content"] = result.Length > 12000 ? result.Substring(0, 12000) : result });
            }
            // 道具を4つより多く頼まれた時は、残りに「やっていない」と返す(返事のない呼び出しがあると、エラーになるため)
            foreach (var call in calls.Skip(4))
                messages.Add(new JsonObject { ["role"] = "tool", ["tool_call_id"] = call?["id"]?.GetValue<string>() ?? "", ["content"] = "(一度に頼める数を超えたため、行いませんでした)" });
        }
        return WebTools.SourcesBlock(sources);
    }

    static async Task<string> PostChatAsync(string baseUrl, string key, JsonObject body, CancellationToken ct)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, baseUrl.TrimEnd('/') + "/chat/completions")
        { Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json") };
        if (!string.IsNullOrWhiteSpace(key)) req.Headers.Add("Authorization", "Bearer " + key);
        return await PostAsync(req, ct);
    }
}


// ====================================================================== AI との会話の記録(YMM4専用ストレージの「AI会話」フォルダー)
// YmmExplorer の AIエディットと、YMM4 マネージャーの AI質問で同じ形を使うので、どちらで話した会話も、どちらでも続きから話せます。
public sealed class AiChatLine
{
    public string Role { get; set; } = "user";
    public string Text { get; set; } = "";
    public string Attached { get; set; } = "";
    public DateTime Time { get; set; } = DateTime.Now;
}

public sealed class AiChatRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "";
    /// <summary>どこで話したか(AIエディット / AI質問)</summary>
    public string Source { get; set; } = "";
    public DateTime Created { get; set; } = DateTime.Now;
    public DateTime Updated { get; set; } = DateTime.Now;
    public string Provider { get; set; } = "";
    public string Model { get; set; } = "";
    /// <summary>話していた時の YMM4 プロジェクト(分かる時だけ)</summary>
    public string Project { get; set; } = "";
    public List<AiChatLine> Messages { get; set; } = new();

    [System.Text.Json.Serialization.JsonIgnore] public string FilePath { get; set; } = "";
    [System.Text.Json.Serialization.JsonIgnore] public string InfoText => $"{Updated:yyyy/MM/dd HH:mm} ・ {Messages.Count(m => m.Role == "user")} 回の質問 ・ {Source}" + (Project.Length > 0 ? $" ・ {Project}" : "") + (Model.Length > 0 ? $" ・ {Model}" : "");
}

public static class AiChatLog
{
    public static string AppDataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "AI会話");

    /// <summary>YMM4 マネージャーで登録した専用ストレージ(無ければ null)</summary>
    static string? StorageRoot
    {
        get
        {
            try
            {
                var s = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YMM4Manager", "settings.json");
                if (!File.Exists(s)) return null;
                var r = JsonNode.Parse(File.ReadAllText(s))?["StorageRoot"]?.GetValue<string>();
                return !string.IsNullOrWhiteSpace(r) && Directory.Exists(r) ? r : null;
            }
            catch { return null; }
        }
    }

    /// <summary>会話を保存する場所(専用ストレージがあれば、その中の「AI会話」)</summary>
    public static string Dir => StorageRoot is { } r ? Path.Combine(r, "AI会話") : AppDataDir;

    static readonly JsonSerializerOptions Opts = new() { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static void Save(AiChatRecord r)
    {
        try
        {
            if (r.Messages.Count == 0) return;
            r.Updated = DateTime.Now;
            if (string.IsNullOrWhiteSpace(r.Title))
            {
                var first = r.Messages.FirstOrDefault(m => m.Role == "user")?.Text ?? "会話";
                first = first.Replace("\r", "").Replace("\n", " ").Trim();
                r.Title = first.Length > 40 ? first.Substring(0, 40) + "…" : first;
            }
            var dir = Dir;
            Directory.CreateDirectory(dir);
            // 専用ストレージを登録する前の会話は、そのまま元の場所に置いておく(移さない)
            if (string.IsNullOrEmpty(r.FilePath) || !File.Exists(r.FilePath))
                r.FilePath = Path.Combine(dir, $"{r.Created:yyyyMMdd_HHmmss}_{r.Id[..8]}.json");
            var tmp = r.FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(r, Opts));
            File.Move(tmp, r.FilePath, true);
        }
        catch { }
    }

    /// <summary>保存してある会話(新しい順)。専用ストレージと、登録前の場所の両方から読む</summary>
    public static List<AiChatRecord> List(int max = 300)
    {
        var list = new List<AiChatRecord>();
        foreach (var dir in new[] { Dir, AppDataDir }.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var f in Directory.GetFiles(dir, "*.json"))
            {
                try
                {
                    var r = JsonSerializer.Deserialize<AiChatRecord>(File.ReadAllText(f));
                    if (r == null) continue;
                    r.FilePath = f;
                    list.Add(r);
                }
                catch { }
            }
        }
        return list.GroupBy(r => r.Id).Select(g => g.OrderByDescending(x => x.Updated).First()).OrderByDescending(r => r.Updated).Take(max).ToList();
    }

    public static void Delete(AiChatRecord r)
    {
        try { if (File.Exists(r.FilePath)) File.Delete(r.FilePath); } catch { }
    }
}

// ====================================================================== YMM4専用ストレージの中身を AI に見せる
/// <summary>
/// 専用ストレージの、すべてのファイルの一覧(AI に送る)と、AI が「読みたい」と言ったファイルの場所の確認。
/// AI は ```ymm-read {"paths":["素材/台本.txt"]} ``` と書くと、そのファイルの中身を読める(専用ストレージの中だけ)
/// </summary>
public static class AiStorage
{
    public static string? Root
    {
        get
        {
            try
            {
                var s = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YMM4Manager", "settings.json");
                if (!File.Exists(s)) return null;
                var r = JsonNode.Parse(File.ReadAllText(s))?["StorageRoot"]?.GetValue<string>();
                return !string.IsNullOrWhiteSpace(r) && Directory.Exists(r) ? Path.GetFullPath(r).TrimEnd('\\') : null;
            }
            catch { return null; }
        }
    }

    static string Size(long b) => b >= 1L << 30 ? $"{b / (double)(1L << 30):0.#}GB" : b >= 1L << 20 ? $"{b / (double)(1L << 20):0.#}MB" : $"{Math.Max(1, b / 1024)}KB";

    /// <summary>YMM4 本体のフォルダー(中身は多いので、まとめて書く)</summary>
    static bool IsYmm4Dir(string dir) => File.Exists(Path.Combine(dir, "YukkuriMovieMaker.exe"));

    /// <summary>専用ストレージの中身の一覧(フォルダーの木の形。多いフォルダーは途中まで)</summary>
    public static string Index(int maxChars = 30000)
    {
        var root = Root;
        if (root == null) return "";
        var sb = new StringBuilder();
        sb.AppendLine($"## YMM4専用ストレージの中身({root})");
        sb.AppendLine("(パスは専用ストレージからの相対。中身を読みたいファイルは ymm-read で指定できる)");
        void Walk(string dir, int depth)
        {
            if (sb.Length > maxChars) return;
            string[] subs, files;
            try { subs = Directory.GetDirectories(dir); files = Directory.GetFiles(dir); }
            catch { return; }
            var indent = new string(' ', depth * 2);
            int shown = 0;
            foreach (var f in files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                if (sb.Length > maxChars) { sb.AppendLine(indent + "…(多いため省略)"); return; }
                if (++shown > 60) { sb.AppendLine($"{indent}…ほか {files.Length - 60} 個のファイル"); break; }
                FileInfo fi;
                try { fi = new FileInfo(f); } catch { continue; }
                sb.AppendLine($"{indent}{fi.Name}({Size(fi.Length)}・{fi.LastWriteTime:yyyy/MM/dd})");
            }
            foreach (var d in subs.OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
            {
                if (sb.Length > maxChars) return;
                var name = Path.GetFileName(d);
                if (IsYmm4Dir(d))
                {
                    // YMM4 本体: プラグインと設定の名前だけ
                    var plugin = Path.Combine(d, "user", "plugin");
                    var plugins = Directory.Exists(plugin) ? Directory.GetFileSystemEntries(plugin).Select(Path.GetFileName).ToList() : new List<string?>();
                    sb.AppendLine($"{indent}{name}/(YMM4 本体。プラグイン {plugins.Count} 個: {string.Join("、", plugins.Take(40))})");
                    continue;
                }
                int count = 0;
                try { count = Directory.EnumerateFiles(d, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true }).Take(10001).Count(); } catch { }
                sb.AppendLine($"{indent}{name}/(ファイル {(count > 10000 ? "10000以上" : count.ToString())} 個)");
                if (depth < 4) Walk(d, depth + 1);
            }
        }
        Walk(root, 0);
        return sb.ToString();
    }

    static readonly System.Text.RegularExpressions.Regex ReadBlock = new(@"```[ \t]*ymm-read[ \t]*\r?\n?(.*?)(?:```|\z)", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    /// <summary>返事から「読みたいファイル」を取り出す(専用ストレージの中にあるものだけ。返事の文章からは消す)</summary>
    public static List<string> ExtractReads(ref string text)
    {
        var list = new List<string>();
        var m = ReadBlock.Match(text);
        if (!m.Success) return list;
        text = ReadBlock.Replace(text, "").TrimEnd();
        var root = Root;
        // 読んでよい場所: 専用ストレージ + 会話の記録・AIの記憶(専用ストレージが無い時は AppData の中)
        var allowed = new[] { root, AiChatLog.Dir, AiChatLog.AppDataDir, AiMemory.Dir }
            .Where(d => d != null).Select(d => Path.GetFullPath(d!).TrimEnd('\\')).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var baseDir = root ?? Path.GetDirectoryName(AiChatLog.Dir)!;
        var body = m.Groups[1].Value.Trim();
        var raw = new List<string>();
        try
        {
            var node = JsonNode.Parse(body, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true });
            var arr = node as JsonArray ?? node?["paths"] as JsonArray ?? (node?["path"] != null ? new JsonArray(node["path"]!.DeepClone()) : null);
            if (arr != null) foreach (var a in arr) if (a?.ToString() is { Length: > 0 } s) raw.Add(s);
        }
        catch { raw.AddRange(body.Split('\n').Select(l => l.Trim().Trim('"', ',')).Where(l => l.Length > 0)); }
        foreach (var r in raw.Take(20))
        {
            try
            {
                var full = Path.GetFullPath(Path.IsPathRooted(r) ? r : Path.Combine(baseDir, r.Replace('/', '\\')));
                if (!allowed.Any(a => (full + "\\").StartsWith(a + "\\", StringComparison.OrdinalIgnoreCase))) continue;   // 専用ストレージの外は読ませない
                if (File.Exists(full) || Directory.Exists(full)) list.Add(full);
            }
            catch { }
        }
        return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>AI への説明(システムの指示に足す)</summary>
    public static string Instructions =>
        "## YMM4専用ストレージ\n" +
        "ユーザーの YMM4専用ストレージ(素材・録画・プロジェクト・YMM4 本体・バックアップなど)の中身の一覧が届きます。中身を読む必要があるファイルは、返事に次のブロックを書くと、アプリがそのファイルを読んで、続けて送ります(1回の返事で20個まで。専用ストレージの中だけ)。\n" +
        "```ymm-read\n{\"paths\":[\"素材/台本.txt\",\"録画/2026-11-01.mp4\"]}\n```\n" +
        "読む必要がない時は書かない。ファイルの中身を読む前に、内容を決めつけない。";
}

// ====================================================================== AI の記憶(会話をまたいで覚える)
/// <summary>
/// AI が、これまでの会話(ほかのプロジェクト・YMM4 マネージャーの AI質問も含む)をまたいで覚えておくもの。
/// 専用ストレージの「AI記憶\記憶.md」に 1 行 1 つで保存する(メモ帳で直接直してもよい)。
/// AI は返事に ```ymm-memory {"add":[...],"remove":[...]} ``` を書いて、記憶を足したり消したりする。
/// あわせて、保存してある過去の会話の一覧も AI に見せ、詳しく知りたい会話は ymm-read で読ませる
/// </summary>
public static class AiMemory
{
    public static string Dir => AiStorage.Root is { } r ? Path.Combine(r, "AI記憶")
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "AI記憶");
    public static string FilePath => Path.Combine(Dir, "記憶.md");
    const int MaxLines = 400;
    static readonly object Gate = new();

    const string Header = "# AI の記憶\n" +
        "# AI が会話をまたいで覚えていることです。「- 」で始まる 1 行が 1 つの記憶です。自由に書き足し・書き換え・削除できます。\n";

    /// <summary>覚えていること(古い順)</summary>
    public static List<string> Load()
    {
        lock (Gate)
        {
            try
            {
                if (!File.Exists(FilePath)) return new();
                return File.ReadAllLines(FilePath).Select(l => l.Trim()).Where(l => l.StartsWith("- ")).Select(l => l.Substring(2).Trim()).Where(l => l.Length > 0).ToList();
            }
            catch { return new(); }
        }
    }

    static void Write(List<string> lines)
    {
        Directory.CreateDirectory(Dir);
        var tmp = FilePath + ".tmp";
        File.WriteAllText(tmp, Header + "\n" + string.Join("\n", lines.Select(l => "- " + l)) + "\n", new UTF8Encoding(false));
        File.Move(tmp, FilePath, true);
    }

    /// <summary>記憶を足す・消す。実際に変わったもの(足したもの・消したもの)を返す</summary>
    public static (List<string> added, List<string> removed) Apply(IEnumerable<string> adds, IEnumerable<string> removes, string project = "")
    {
        var added = new List<string>();
        var removed = new List<string>();
        lock (Gate)
        {
            try
            {
                var lines = Load();
                foreach (var r in removes.Select(x => x.Trim()).Where(x => x.Length >= 2))
                {
                    var hit = lines.Where(l => l.Contains(r, StringComparison.OrdinalIgnoreCase)).ToList();
                    foreach (var h in hit) { lines.Remove(h); removed.Add(h); }
                }
                foreach (var a0 in adds)
                {
                    var a = a0.Replace("\r", "").Replace("\n", " ").Trim().TrimStart('-', ' ');
                    if (a.Length == 0) continue;
                    if (lines.Any(l => l.EndsWith(a, StringComparison.OrdinalIgnoreCase))) continue;   // 同じものは足さない
                    var line = $"{DateTime.Now:yyyy/MM/dd}" + (project.Length > 0 ? $" [{project}]" : "") + " " + a;
                    lines.Add(line);
                    added.Add(a);
                }
                if (lines.Count > MaxLines) lines = lines.Skip(lines.Count - MaxLines).ToList();
                if (added.Count > 0 || removed.Count > 0) Write(lines);
            }
            catch { }
        }
        return (added, removed);
    }

    /// <summary>すべて忘れる(ファイルは残し、中身を空にする)</summary>
    public static void Clear() { lock (Gate) { try { Write(new()); } catch { } } }

    /// <summary>記憶のファイル(無ければ作る)。メモ帳などで開く用</summary>
    public static string EnsureFile()
    {
        lock (Gate) { try { if (!File.Exists(FilePath)) Write(new()); } catch { } }
        return FilePath;
    }

    static readonly System.Text.RegularExpressions.Regex MemoryBlock = new(@"```[ \t]*ymm-memory[ \t]*\r?\n?(.*?)(?:```|\z)", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    // 小さなモデルは「<ymm-memory>{…}」「<ymes記憶>{…}」のような形で書くことがあるので、それも記憶として読む
    static readonly System.Text.RegularExpressions.Regex MemoryTagBlock = new(@"<\s*ym\w*[-_ ]?(?:memory|記憶)[^>]*>\s*(\{.*?\}|\[.*?\])\s*(?:<\s*/\s*ym[^>]*>)?",
        System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    // 中身の無い、残った記憶の印(<ymes記憶の追加> など)
    static readonly System.Text.RegularExpressions.Regex StrayMemoryTag = new(@"^[ \t]*<\s*/?\s*ym\w*[^>\n]*(?:memory|記憶)[^>\n]*>[ \t]*\r?\n?",
        System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    static void Collect(JsonNode? n, List<string> to)
    {
        if (n is JsonArray arr) { foreach (var a in arr) if (a?.ToString() is { Length: > 0 } t) to.Add(t); }
        else if (n is JsonValue v && v.ToString().Length > 0) to.Add(v.ToString());
    }

    /// <summary>返事から ymm-memory のブロックを取り出して(返事からは消す)、記憶に反映する。変わったことの一言(無ければ "")</summary>
    public static string ExtractAndApply(ref string text, string project = "")
    {
        var adds = new List<string>();
        var removes = new List<string>();
        var matches = MemoryBlock.Matches(text).Cast<System.Text.RegularExpressions.Match>().Concat(MemoryTagBlock.Matches(text)).ToList();
        foreach (var m in matches)
        {
            var body = m.Groups[1].Value.Trim();
            try
            {
                var node = JsonNode.Parse(body, documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true });
                if (node is JsonArray arr0) { foreach (var a in arr0) if (a?.ToString() is { Length: > 0 } s0) adds.Add(s0); }
                else
                {
                    foreach (var key in new[] { "add", "remember" }) Collect(node?[key], adds);
                    foreach (var key in new[] { "remove", "forget" }) Collect(node?[key], removes);
                }
            }
            catch { adds.AddRange(body.Split('\n').Select(l => l.Trim().TrimStart('-', ' ').Trim('"', ',')).Where(l => l.Length > 0)); }
        }
        text = StrayMemoryTag.Replace(MemoryTagBlock.Replace(MemoryBlock.Replace(text, ""), ""), "").TrimEnd();
        if (adds.Count == 0 && removes.Count == 0) return "";
        var (added, removed) = Apply(adds, removes, project);
        var parts = new List<string>();
        if (added.Count > 0) parts.Add("🧠 覚えました: " + string.Join(" / ", added.Take(5)) + (added.Count > 5 ? $" ほか {added.Count - 5} 件" : ""));
        if (removed.Count > 0) parts.Add("🧹 忘れました: " + removed.Count + " 件");
        return string.Join("\n", parts);
    }

    /// <summary>過去の会話の一覧(新しい順。いまの会話は除く)</summary>
    static string PastChats(string? currentId, int max)
    {
        var sb = new StringBuilder();
        var root = AiStorage.Root;
        foreach (var r in AiChatLog.List(max + 1).Where(r => r.Id != currentId).Take(max))
        {
            var path = r.FilePath;
            if (root != null && (path + "").StartsWith(root + "\\", StringComparison.OrdinalIgnoreCase)) path = path.Substring(root.Length + 1).Replace('\\', '/');
            var qs = r.Messages.Where(m => m.Role == "user").Select(m => m.Text.Replace("\r", "").Replace("\n", " ").Trim()).Where(t => t.Length > 0)
                      .Take(3).Select(t => t.Length > 50 ? t.Substring(0, 50) + "…" : t);
            sb.AppendLine($"- {r.Updated:yyyy/MM/dd HH:mm} ・ {r.Source}" + (r.Project.Length > 0 ? $" ・ プロジェクト「{r.Project}」" : "") +
                          $" ・ 「{r.Title}」 ・ 質問: {string.Join(" / ", qs)} ・ ファイル: {path}");
        }
        return sb.ToString();
    }

    /// <summary>システムの指示に足す、記憶の説明と中身・過去の会話の一覧</summary>
    public static string Prompt(string? currentChatId, string project = "", int maxChats = 40)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## AI の記憶(会話をまたいで覚えておく)");
        sb.AppendLine("あなたは、これまでの会話(ほかのプロジェクトや、YMM4 マネージャーの AI質問・YmmExplorer の AIエディットでの会話も含む)をまたいで、ユーザーのことを覚えておけます。");
        sb.AppendLine("- 下の「覚えていること」は、前の会話で記録したものです。答えに役立つ時は使ってください(毎回わざわざ話題に出す必要はありません)。");
        sb.AppendLine("- 長く役に立つこと(ユーザーの好みや決めたこと、よく使う素材・キャラクター・設定、作っている動画やシリーズの内容、進め方の約束、「覚えておいて」と頼まれたこと)が新しく分かったら、返事の最後に次のブロックで記録してください。1つの記憶は短い1文で、あとで読んで分かるように具体的に。");
        sb.AppendLine("```ymm-memory\n{\"add\":[\"字幕はいつも源ノ角ゴシック Bold・48px を使う\"],\"remove\":[\"古くなった記憶の一部の文字\"]}\n```");
        sb.AppendLine("- 古くなった・間違っていた記憶は remove で消して、正しいものを add してください。ユーザーが「忘れて」と言ったら remove で消してください。");
        sb.AppendLine("- その場かぎりのこと、APIキー・パスワードなどの秘密、すでに覚えていることは記録しないでください。記録することが無い時はブロックを書かないでください。");
        sb.AppendLine("- 下の「これまでの会話」の中身を詳しく知りたい時は、そのファイルを ymm-read で読めます(例: ```ymm-read\n{\"paths\":[\"AI会話/20261120_101500_abcd1234.json\"]}\n```)。");
        if (project.Length > 0) sb.AppendLine($"- いまのプロジェクトは「{project}」です。");
        var mem = Load();
        sb.AppendLine();
        sb.AppendLine($"### 覚えていること({mem.Count} 件)");
        if (mem.Count == 0) sb.AppendLine("(まだありません)");
        else
        {
            var text = string.Join("\n", mem.Select(l => "- " + l));
            if (text.Length > 20000) text = "(古いものは省略)\n" + text.Substring(text.Length - 20000);
            sb.AppendLine(text);
        }
        var past = PastChats(currentChatId, maxChats);
        if (past.Length > 0)
        {
            sb.AppendLine();
            sb.AppendLine("### これまでの会話(新しい順)");
            sb.Append(past);
        }
        return sb.ToString();
    }
}
