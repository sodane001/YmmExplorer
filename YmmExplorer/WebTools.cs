// AI がインターネットを調べるための道具(検索・ページを読む)。YmmExplorer と YMM4 マネージャーで同じものを使います。
// Claude・Gemini は AI 側の検索を使うので、これは ChatGPT(OpenAI)と OpenAI 互換の AI のときに使います。
// API キーの要らない検索(DuckDuckGo、だめなら Bing)を使い、このPCや LAN の中のページは安全のため読みません。
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace YmmExplorer;

public sealed record WebSource(string Title, string Url);

public static class WebTools
{
    const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0 Safari/537.36 YmmExplorer";
    const int MaxBytes = 3 * 1024 * 1024;

    static readonly HttpClient http = new(new HttpClientHandler { AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.All })
    { Timeout = TimeSpan.FromSeconds(25) };

    // Shift_JIS・EUC-JP のページも読めるように
    static WebTools() { try { Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); } catch { } }

    /// <summary>OpenAI 形式の道具の説明(function calling)</summary>
    public static JsonArray ToolsJson() => new()
    {
        new JsonObject
        {
            ["type"] = "function",
            ["function"] = new JsonObject
            {
                ["name"] = "web_search",
                ["description"] = "インターネットを検索して、ページのタイトル・URL・要約を得る。最新の情報や知らないことを調べる時に使う。",
                ["parameters"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject { ["query"] = new JsonObject { ["type"] = "string", ["description"] = "検索する言葉" } },
                    ["required"] = new JsonArray("query"),
                },
            },
        },
        new JsonObject
        {
            ["type"] = "function",
            ["function"] = new JsonObject
            {
                ["name"] = "fetch_url",
                ["description"] = "URL のページを開いて、本文を読む。検索結果のページを詳しく読む時や、ユーザーが URL を書いた時に使う。",
                ["parameters"] = new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject { ["url"] = new JsonObject { ["type"] = "string", ["description"] = "読むページの URL" } },
                    ["required"] = new JsonArray("url"),
                },
            },
        },
    };

    public static string SystemNote =>
        $"今日は {DateTime.Now:yyyy年M月d日} です。あなたはインターネットを使えます。最新の情報・知らないこと・URL の中身が必要な時は、" +
        "web_search で検索し、fetch_url でページを読んでから答えてください。推測で補った部分はそう書いてください。";

    // ------------------------------------------------------------------ 安全確認
    static async Task CheckHostAsync(Uri uri, CancellationToken ct)
    {
        if (uri.Scheme is not ("http" or "https")) throw new InvalidOperationException("http:// か https:// の URL だけ読めます。");
        var host = uri.Host.ToLowerInvariant();
        if (host == "localhost" || host.EndsWith(".local") || host.EndsWith(".localhost") || host.EndsWith(".internal"))
            throw new InvalidOperationException("このPCやLANの中のページは読めません。");
        IPAddress[] addresses;
        try { addresses = IPAddress.TryParse(host.Trim('[', ']'), out var ip) ? new[] { ip } : await Dns.GetHostAddressesAsync(host, ct); }
        catch (SocketException) { throw new InvalidOperationException($"{host} が見つかりません。"); }
        foreach (var a in addresses)
            if (IsPrivate(a)) throw new InvalidOperationException("このPCやLANの中のページは読めません。");
    }

    static bool IsPrivate(IPAddress a)
    {
        if (a.IsIPv4MappedToIPv6) a = a.MapToIPv4();
        if (IPAddress.IsLoopback(a) || a.Equals(IPAddress.Any) || a.Equals(IPAddress.IPv6Any)) return true;
        if (a.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = a.GetAddressBytes();
            return b[0] == 10 || b[0] == 127 || b[0] == 0 || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) || (b[0] == 192 && b[1] == 168)
                   || (b[0] == 169 && b[1] == 254) || (b[0] == 100 && b[1] >= 64 && b[1] <= 127) || b[0] >= 224;
        }
        if (a.IsIPv6LinkLocal || a.IsIPv6SiteLocal || a.IsIPv6Multicast) return true;
        var v6 = a.GetAddressBytes();
        return (v6[0] & 0xFE) == 0xFC;   // fc00::/7
    }

    static async Task<(string text, string type, string url)> DownloadAsync(string url, HttpContent? post, CancellationToken ct)
    {
        var uri = new Uri(url);
        for (int hop = 0; hop < 6; hop++)
        {
            await CheckHostAsync(uri, ct);
            using var req = new HttpRequestMessage(post != null && hop == 0 ? HttpMethod.Post : HttpMethod.Get, uri);
            if (post != null && hop == 0) req.Content = post;
            req.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            req.Headers.TryAddWithoutValidation("Accept-Language", "ja,en;q=0.8");
            req.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,text/plain,application/json;q=0.9,*/*;q=0.5");
            using var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            if ((int)res.StatusCode is >= 300 and < 400 && res.Headers.Location is { } loc)
            {
                uri = loc.IsAbsoluteUri ? loc : new Uri(uri, loc);
                continue;
            }
            if (!res.IsSuccessStatusCode) throw new InvalidOperationException($"ページを開けませんでした({(int)res.StatusCode})。");
            var type = res.Content.Headers.ContentType?.MediaType?.ToLowerInvariant() ?? "";
            await using var stream = await res.Content.ReadAsStreamAsync(ct);
            using var buffer = new MemoryStream();
            var chunk = new byte[81920];
            int n;
            while ((n = await stream.ReadAsync(chunk, ct)) > 0 && buffer.Length < MaxBytes) buffer.Write(chunk, 0, n);
            var bytes = buffer.ToArray();
            Encoding enc = Encoding.UTF8;
            var charset = res.Content.Headers.ContentType?.CharSet;
            if (string.IsNullOrEmpty(charset))
            {
                var head = Encoding.ASCII.GetString(bytes, 0, Math.Min(bytes.Length, 4096));
                var m = Regex.Match(head, "<meta[^>]+charset=[\"']?([\\w\\-]+)", RegexOptions.IgnoreCase);
                if (m.Success) charset = m.Groups[1].Value;
            }
            try
            {
                if (!string.IsNullOrEmpty(charset)) enc = Encoding.GetEncoding(charset.Trim('"'));
            }
            catch { enc = Encoding.UTF8; }
            return (enc.GetString(bytes), type, uri.ToString());
        }
        throw new InvalidOperationException("転送が多すぎて、ページを開けませんでした。");
    }

    // ------------------------------------------------------------------ HTML → 読める文章
    static readonly Regex SkipBlocks = new(@"<(script|style|noscript|svg|template|iframe|head|nav|footer|form|button)\b[^>]*>.*?</\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    static readonly Regex BlockTags = new(@"</?(p|div|br|li|tr|h[1-6]|section|article|table|ul|ol|dd|dt|pre|blockquote|header|main)\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    static readonly Regex AnyTag = new(@"<[^>]+>", RegexOptions.Compiled);

    public static (string title, string text) HtmlToText(string html)
    {
        var tm = Regex.Match(html, @"<title[^>]*>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var title = tm.Success ? WebUtility.HtmlDecode(AnyTag.Replace(tm.Groups[1].Value, "")).Trim() : "";
        var body = Regex.Replace(html, "<!--.*?-->", "", RegexOptions.Singleline);
        body = SkipBlocks.Replace(body, " ");
        body = BlockTags.Replace(body, "\n");
        body = WebUtility.HtmlDecode(AnyTag.Replace(body, ""));
        var sb = new StringBuilder();
        bool blank = false;
        foreach (var raw in body.Split('\n'))
        {
            var line = Regex.Replace(raw, @"[ \t　 ]+", " ").Trim();
            if (line.Length == 0) { if (!blank && sb.Length > 0) sb.Append('\n'); blank = true; continue; }
            sb.Append(line).Append('\n');
            blank = false;
        }
        return (title, sb.ToString().Trim());
    }

    static string Clean(string fragment) => Regex.Replace(WebUtility.HtmlDecode(AnyTag.Replace(fragment, "")), @"\s+", " ").Trim();

    // ------------------------------------------------------------------ 検索
    public sealed record SearchResult(string Title, string Url, string Snippet);

    static async Task<List<SearchResult>> DuckDuckGoAsync(string query, int limit, CancellationToken ct)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string> { ["q"] = query, ["kl"] = "jp-jp" });
        var (html, _, _) = await DownloadAsync("https://html.duckduckgo.com/html/", form, ct);
        var list = new List<SearchResult>();
        foreach (Match m in Regex.Matches(html, "<a[^>]+class=\"result__a\"[^>]+href=\"([^\"]+)\"[^>]*>(.*?)</a>(.*?)(?=<a[^>]+class=\"result__a\"|$)", RegexOptions.Singleline))
        {
            var href = WebUtility.HtmlDecode(m.Groups[1].Value);
            var ud = Regex.Match(href, "[?&]uddg=([^&]+)");
            if (ud.Success) href = Uri.UnescapeDataString(ud.Groups[1].Value);
            if (href.StartsWith("//")) href = "https:" + href;
            if (href.Contains("duckduckgo.com/y.js") || !href.StartsWith("http")) continue;
            var sn = Regex.Match(m.Groups[3].Value, "class=\"result__snippet\"[^>]*>(.*?)</a>", RegexOptions.Singleline);
            list.Add(new SearchResult(Clean(m.Groups[2].Value), href, sn.Success ? Clean(sn.Groups[1].Value) : ""));
            if (list.Count >= limit) break;
        }
        return list;
    }

    static async Task<List<SearchResult>> BingAsync(string query, int limit, CancellationToken ct)
    {
        var (html, _, _) = await DownloadAsync($"https://www.bing.com/search?q={Uri.EscapeDataString(query)}&setlang=ja&cc=JP", null, ct);
        var list = new List<SearchResult>();
        foreach (Match block in Regex.Matches(html, "<li class=\"b_algo\"(.*?)</li>", RegexOptions.Singleline))
        {
            var link = Regex.Match(block.Groups[1].Value, "<h2[^>]*>\\s*<a[^>]+href=\"([^\"]+)\"[^>]*>(.*?)</a>", RegexOptions.Singleline);
            if (!link.Success) continue;
            var sn = Regex.Match(block.Groups[1].Value, "<p[^>]*>(.*?)</p>", RegexOptions.Singleline);
            list.Add(new SearchResult(Clean(link.Groups[2].Value), WebUtility.HtmlDecode(link.Groups[1].Value), sn.Success ? Clean(sn.Groups[1].Value) : ""));
            if (list.Count >= limit) break;
        }
        return list;
    }

    public static async Task<List<SearchResult>> SearchAsync(string query, int limit, CancellationToken ct)
    {
        query = (query ?? "").Trim();
        if (query.Length == 0) throw new InvalidOperationException("検索する言葉が空です。");
        if (query.Length > 300) query = query.Substring(0, 300);
        string last = "";
        foreach (var engine in new Func<string, int, CancellationToken, Task<List<SearchResult>>>[] { DuckDuckGoAsync, BingAsync })
        {
            try
            {
                var r = await engine(query, limit, ct);
                if (r.Count > 0) return r;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex) { last = ex.Message; }
        }
        throw new InvalidOperationException("検索結果を取得できませんでした。" + last);
    }

    public static async Task<(string title, string url, string text)> FetchAsync(string url, int maxChars, CancellationToken ct)
    {
        url = (url ?? "").Trim();
        if (!Regex.IsMatch(url, "^https?://", RegexOptions.IgnoreCase)) url = "https://" + url;
        if (!Uri.TryCreate(url, UriKind.Absolute, out _)) throw new InvalidOperationException("URL の形が正しくありません。");
        var (body, type, final) = await DownloadAsync(url, null, ct);
        string title = final, text;
        if (type.Contains("html") || body.Substring(0, Math.Min(body.Length, 2000)).Contains("<html", StringComparison.OrdinalIgnoreCase))
        {
            (title, text) = HtmlToText(body);
            if (title.Length == 0) title = final;
        }
        else if (type.StartsWith("text/") || type.Contains("json") || type.Length == 0) text = body;
        else throw new InvalidOperationException($"このページは文章ではないため読めません({type})。");
        if (text.Length > maxChars) text = text.Substring(0, maxChars) + "\n…(長いので、ここまで)";
        return (title, final, text);
    }

    // ------------------------------------------------------------------ AI の道具として呼ぶ
    public static string Describe(string name, string argsJson)
    {
        string arg = "";
        try { var j = JsonNode.Parse(argsJson); arg = j?["query"]?.GetValue<string>() ?? j?["url"]?.GetValue<string>() ?? ""; } catch { }
        return name == "fetch_url" ? $"🌐 ページを読んでいます: {arg}" : $"🔎 検索しています: {arg}";
    }

    public static async Task<string> RunToolAsync(string name, string argsJson, List<WebSource> sources, CancellationToken ct)
    {
        JsonNode? args = null;
        try { args = JsonNode.Parse(string.IsNullOrWhiteSpace(argsJson) ? "{}" : argsJson); } catch { }
        try
        {
            if (name == "web_search")
            {
                var results = await SearchAsync(args?["query"]?.GetValue<string>() ?? argsJson, 6, ct);
                foreach (var r in results.Take(3)) Add(sources, r.Title, r.Url);
                return string.Join("\n\n", results.Select((r, i) => $"[{i + 1}] {r.Title}\n{r.Url}\n{r.Snippet}"));
            }
            if (name == "fetch_url")
            {
                var (title, url, text) = await FetchAsync(args?["url"]?.GetValue<string>() ?? argsJson, 9000, ct);
                Add(sources, title, url);
                return $"# {title}\n{url}\n\n{text}";
            }
            return $"「{name}」という道具はありません。web_search か fetch_url を使ってください。";
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { return "(失敗: " + ex.Message + ")"; }
    }

    static void Add(List<WebSource> sources, string title, string url)
    {
        if (sources.Any(s => s.Url == url)) return;
        sources.Add(new WebSource(title.Length > 120 ? title.Substring(0, 120) : title, url));
    }

    /// <summary>道具を使えない AI 用: 質問で検索して、上位のページを読んだ結果をまとめる</summary>
    public static async Task<string> QuickResearchAsync(string question, List<WebSource> sources, CancellationToken ct)
    {
        var query = Regex.Replace(question ?? "", @"\s+", " ").Trim();
        if (query.Length > 200) query = query.Substring(0, 200);
        List<SearchResult> results;
        try { results = await SearchAsync(query, 5, ct); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) { return "(インターネット検索に失敗しました: " + ex.Message + ")"; }
        var sb = new StringBuilder("# 検索結果: " + query + "\n");
        for (int i = 0; i < results.Count; i++) sb.Append($"\n[{i + 1}] {results[i].Title}\n{results[i].Url}\n{results[i].Snippet}\n");
        foreach (var r in results.Take(2))
        {
            try
            {
                var (title, url, text) = await FetchAsync(r.Url, 3500, ct);
                Add(sources, title, url);
                sb.Append($"\n# ページ: {title}\n{url}\n\n{text}\n");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch { }
        }
        foreach (var r in results) Add(sources, r.Title, r.Url);
        return sb.ToString();
    }

    public static string SourcesBlock(IEnumerable<WebSource> sources)
    {
        var list = sources.Where(s => !string.IsNullOrWhiteSpace(s.Url)).GroupBy(s => s.Url).Select(g => g.First()).Take(8).ToList();
        if (list.Count == 0) return "";
        return "\n\n参考にしたページ:\n" + string.Join("\n", list.Select(s => $"- {(s.Title.Length > 0 ? s.Title : s.Url)}  {s.Url}"));
    }
}
