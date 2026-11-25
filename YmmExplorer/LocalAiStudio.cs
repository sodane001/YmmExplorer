// Local AI Studio(このPCの中で動くAI: チャット・画像生成・音楽生成)との連携。
// YmmExplorer と YMM4 マネージャーで同じものを使います。
// Local AI Studio は起動中、%LOCALAPPDATA%\LocalAIWorkbench\api.json にポートと合言葉を書き出すので、それを読んでつなぐ。
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;

namespace YmmExplorer;

/// <summary>Local AI Studio のモデル1つ分(性能・重さの目安つき)</summary>
public sealed class LocalStudioModel
{
    public string Id { get; init; } = "";
    /// <summary>chat / image / music</summary>
    public string Type { get; init; } = "";
    public string Name { get; init; } = "";
    public string Engine { get; init; } = "";
    public string Size { get; init; } = "";
    public string License { get; init; } = "";
    public string Description { get; init; } = "";
    /// <summary>性能(1〜5)</summary>
    public int Performance { get; init; }
    /// <summary>重さ(1〜5。大きいほど、たくさんのメモリと時間が要る)</summary>
    public int Weight { get; init; }
    public string Memory { get; init; } = "";
    public string Speed { get; init; } = "";
    public string Note { get; init; } = "";
    public bool Installed { get; init; }
    public string Detail { get; init; } = "";
    public JsonObject Defaults { get; init; } = new();
    /// <summary>参考にできるファイル(image = 参考画像 / melody = 参考のメロディー / text)</summary>
    public IReadOnlyList<string> Accepts { get; init; } = Array.Empty<string>();
    /// <summary>画像を見られるチャットモデルか</summary>
    public bool Vision { get; init; }

    public bool IsAudioStudio => Engine == "Audio Studio MCP";
    public static string Stars(int v, char on = '★', char off = '☆') => new string(on, Math.Clamp(v, 0, 5)) + new string(off, 5 - Math.Clamp(v, 0, 5));
    public static string WeightWord(int v) => v switch { 1 => "とても軽い", 2 => "軽い", 3 => "ふつう", 4 => "重い", 5 => "とても重い", _ => "—" };

    /// <summary>▼ の一覧に出す名前</summary>
    public string Display => $"{Name}　性能{Stars(Performance)}　重さ{Stars(Weight, '■', '□')}" + (Installed ? "" : "　(未インストール)");
    /// <summary>性能・重さ・目安の説明(1〜2行)</summary>
    public string RatingText => Performance == 0 ? "" :
        $"性能 {Stars(Performance)}　重さ {Stars(Weight, '■', '□')}({WeightWord(Weight)})　速さ: {Speed}\n必要なメモリの目安: {Memory}" + (Note.Length > 0 ? $"　/　{Note}" : "");
    public override string ToString() => Display;

    public int DefaultInt(string key, int fallback)
    {
        try { return Defaults[key]?.GetValue<int>() ?? fallback; } catch { return fallback; }
    }
}

/// <summary>Local AI Studio で処理中のものの進み具合(GPU の使用率・残り時間つき)</summary>
public sealed class StudioProgress
{
    /// <summary>queued / running</summary>
    public string State { get; init; } = "";
    public string Phase { get; init; } = "";
    /// <summary>0〜1。分からなければ null</summary>
    public double? Fraction { get; init; }
    /// <summary>残りの秒数。分からなければ null</summary>
    public double? EtaSeconds { get; init; }
    /// <summary>残り時間が、過去の実績からの目安か</summary>
    public bool EtaGuess { get; init; }
    public double ElapsedSeconds { get; init; }
    public int Tokens { get; init; }
    public double Rate { get; init; }
    public double? GpuUtil { get; init; }
    public double? VramUsedMb { get; init; }
    public double? VramTotalMb { get; init; }
    public double? TempC { get; init; }
    public string LastLog { get; init; } = "";
    /// <summary>Local AI Studio が作った説明の1行(「生成中 45%・残り 約 0:23・GPU 87%・VRAM 5.1/8.0 GB」など)</summary>
    public string ServerText { get; init; } = "";

    public static string Clock(double seconds)
    {
        var t = TimeSpan.FromSeconds(Math.Max(0, Math.Round(seconds)));
        return t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes}:{t.Seconds:00}";
    }

    public string GpuText
    {
        get
        {
            if (GpuUtil is not double u) return "";
            var s = $"GPU {u:0}%";
            if (VramUsedMb is double used && used > 0) s += $"・VRAM {used / 1024:0.0}" + (VramTotalMb is double tot && tot > 0 ? $"/{tot / 1024:0.0} GB" : " GB");
            if (TempC is double c && c > 0) s += $"・{c:0}℃";
            return s;
        }
    }

    /// <summary>画面に出す1行</summary>
    public string Text
    {
        get
        {
            if (ServerText.Length > 0) return ServerText;
            var parts = new List<string>();
            if (State == "queued") parts.Add("待機中");
            else
            {
                parts.Add((Phase.Length > 0 ? Phase : "処理中") + (Fraction is double f ? $" {f * 100:0}%" : ""));
                if (EtaSeconds is double e) parts.Add("残り 約 " + Clock(e) + (EtaGuess ? "(目安)" : ""));
                parts.Add("経過 " + Clock(ElapsedSeconds));
                if (GpuText.Length > 0) parts.Add(GpuText);
            }
            return string.Join("・", parts);
        }
    }
    public override string ToString() => Text;
}

/// <summary>Local AI Studio が起動していない(自動では起動しない)</summary>
public sealed class LocalStudioNotRunningException : InvalidOperationException
{
    public LocalStudioNotRunningException(string message) : base(message) { }
}

public static class LocalAiStudio
{
    sealed record ApiInfo(int Port, string Token, string Exe);

    static readonly HttpClient http = new(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromMinutes(40) };
    static List<LocalStudioModel> cache = new();

    /// <summary>Local AI Studio のデータフォルダー(api.json・生成したファイル outputs など)</summary>
    public static string DataDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LocalAIWorkbench");

    /// <summary>設定で指定した LocalAIStudio.exe の場所(空なら自動で探す)</summary>
    public static string? ExeOverride { get; set; }

    /// <summary>最後に読んだモデルの一覧(性能・重さの表示用)</summary>
    public static IReadOnlyList<LocalStudioModel> CachedModels => cache;

    /// <summary>モデルの性能・重さの説明。分からなければ空</summary>
    public static string Describe(string modelId) => cache.FirstOrDefault(m => m.Id == modelId)?.RatingText ?? "";

    static ApiInfo? ReadInfo()
    {
        try
        {
            var path = Path.Combine(DataDir, "api.json");
            if (!File.Exists(path)) return null;
            var j = JsonNode.Parse(File.ReadAllText(path));
            int port = j?["port"]?.GetValue<int>() ?? 0;
            var token = j?["token"]?.GetValue<string>() ?? "";
            if (port <= 0 || token.Length == 0) return null;
            return new ApiInfo(port, token, j?["exe"]?.GetValue<string>() ?? "");
        }
        catch { return null; }
    }

    static async Task<bool> PingAsync(ApiInfo info, CancellationToken ct)
    {
        try
        {
            using var c = CancellationTokenSource.CreateLinkedTokenSource(ct);
            c.CancelAfter(TimeSpan.FromSeconds(3));
            using var res = await http.GetAsync($"http://127.0.0.1:{info.Port}/api/ping", c.Token);
            return res.IsSuccessStatusCode;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { return false; }
        catch (HttpRequestException) { return false; }
    }

    /// <summary>LocalAIStudio.exe の場所を探す(設定 → 前回の起動 → インストーラーが書いたレジストリ → よくある場所)</summary>
    public static string? FindExe()
    {
        var list = new List<string?> { ExeOverride, ReadInfoLoose()?.Exe };
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\LocalAIStudio");
            list.Add(key?.GetValue("InstallPath") as string);
        }
        catch { }
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        list.Add(Path.Combine(local, "Programs", "LocalAIStudio", "LocalAIStudio.exe"));
        list.Add(Path.Combine(local, "Programs", "Local AI Studio", "LocalAIStudio.exe"));
        list.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LocalAIStudio", "LocalAIStudio.exe"));
        list.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Local AI Studio", "LocalAIStudio.exe"));
        return list.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p));
    }

    // 前回の起動で api.json に書かれた exe(起動中でなくても読む)
    static ApiInfo? ReadInfoLoose()
    {
        try
        {
            var path = Path.Combine(DataDir, "api.json");
            if (!File.Exists(path)) return null;
            var j = JsonNode.Parse(File.ReadAllText(path));
            return new ApiInfo(0, "", j?["exe"]?.GetValue<string>() ?? "");
        }
        catch { return null; }
    }

    /// <summary>起動しているか(起動はしない)</summary>
    public static async Task<bool> IsRunningAsync(CancellationToken ct = default)
        => ReadInfo() is { } info && await PingAsync(info, ct);

    /// <summary>
    /// つながるか確かめる。Local AI Studio は自分では起動しない(勝手に開かないように)。
    /// 起動していなければ「Local AI Studio を開く」を押してもらうよう伝える
    /// </summary>
    static async Task<ApiInfo> EnsureAsync(CancellationToken ct)
    {
        if (ReadInfo() is { } info && await PingAsync(info, ct)) return info;
        if (FindExe() == null)
            throw new LocalStudioNotRunningException("Local AI Studio が見つかりませんでした。Local AI Studio をインストールして起動してください" +
                "(設定で LocalAIStudio.exe の場所を指定することもできます)。");
        throw new LocalStudioNotRunningException("Local AI Studio が起動していません。「Local AI Studio を開く」を押して起動してから、もう一度お試しください。");
    }

    static async Task<JsonNode?> SendAsync(HttpMethod method, string path, JsonNode? body, CancellationToken ct)
    {
        for (int attempt = 0; ; attempt++)
        {
            var info = await EnsureAsync(ct);
            using var req = new HttpRequestMessage(method, $"http://127.0.0.1:{info.Port}{path}");
            req.Headers.TryAddWithoutValidation("X-Studio-Token", info.Token);
            if (body != null) req.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");
            using var res = await http.SendAsync(req, ct);
            var text = await res.Content.ReadAsStringAsync(ct);
            JsonNode? node = null;
            try { node = JsonNode.Parse(text); } catch { }
            if ((int)res.StatusCode == 401 && attempt == 0) continue;     // 再起動で合言葉が変わった時は、読み直してもう一度
            if (!res.IsSuccessStatusCode)
                throw new InvalidOperationException("Local AI Studio: " + (node?["error"]?.GetValue<string>() ?? $"エラー {(int)res.StatusCode}"));
            return node;
        }
    }

    /// <summary>モデルの一覧(type: chat / image / music。null ならすべて)</summary>
    public static async Task<List<LocalStudioModel>> ModelsAsync(string? type, CancellationToken ct)
    {
        var state = await SendAsync(HttpMethod.Get, "/api/state", null, ct);
        var list = new List<LocalStudioModel>();
        var status = state?["status"] as JsonObject;
        if (state?["models"] is JsonArray ms)
            foreach (var m in ms)
            {
                if (m is not JsonObject o) continue;
                var id = Str(o, "id");
                var st = status?[id] as JsonObject;
                list.Add(new LocalStudioModel
                {
                    Id = id, Type = Str(o, "type"), Name = Str(o, "name"), Engine = Str(o, "engine"), Size = Str(o, "size"),
                    License = Str(o, "license"), Description = Str(o, "description"), Memory = Str(o, "memory"), Speed = Str(o, "speed"),
                    Note = Str(o, "note"), Performance = Int(o, "performance"), Weight = Int(o, "weight"),
                    Installed = st?["installed"]?.GetValue<bool>() ?? false, Detail = st?["detail"]?.GetValue<string>() ?? "",
                    Defaults = o["defaults"] is JsonObject d ? (JsonObject)d.DeepClone() : new JsonObject(),
                    Accepts = o["accepts"] is JsonArray acc ? acc.Select(x => x is JsonValue jv && jv.TryGetValue<string>(out var str) ? str : "").Where(x => x.Length > 0).ToList() : new List<string>(),
                    Vision = o["vision"] is JsonValue vv && vv.TryGetValue<bool>(out var vis) && vis,
                });
            }
        cache = list;
        return type == null ? list : list.Where(m => m.Type == type).ToList();
    }

    /// <summary>このPCの GPU(なければ CPU)の説明</summary>
    public static async Task<string> HardwareAsync(CancellationToken ct)
    {
        var state = await SendAsync(HttpMethod.Get, "/api/state", null, ct);
        var hw = state?["hardware"];
        var name = hw?["name"]?.GetValue<string>() ?? "CPU";
        var vram = hw?["vram_mb"]?.GetValue<int>() ?? 0;
        return vram > 0 ? $"{name}(VRAM {vram / 1024.0:0.#} GB)" : name + "(GPUなし・CPUで動かします)";
    }

    static string Str(JsonObject o, string key) { try { return o[key]?.GetValue<string>() ?? ""; } catch { return o[key]?.ToString() ?? ""; } }
    static int Int(JsonObject o, string key) { try { return o[key]?.GetValue<int>() ?? 0; } catch { return 0; } }

    /// <summary>
    /// チャット。messages は (user/assistant, 文章)。終わるまで待って、答えを返す。
    /// web = true で、インターネット検索・ページ閲覧を使う。progress には進み具合(GPU・残り時間つき)が届く
    /// </summary>
    public static async Task<string> ChatAsync(string modelId, string system, IEnumerable<(string role, string text, IReadOnlyList<string>? images)> messages,
                                               CancellationToken ct, bool web = false, IProgress<StudioProgress>? progress = null)
    {
        var arr = new JsonArray();
        int chars = system.Length;
        string? lastRole = null;
        foreach (var (role, raw, images) in messages)
        {
            var text = raw;
            bool hasImages = images is { Count: > 0 } && role != "assistant";
            if (string.IsNullOrWhiteSpace(text)) { if (!hasImages) continue; text = "(画像を添付しました)"; }
            var r = role == "assistant" ? "assistant" : "user";
            chars += text.Length;
            // 同じ人の発言が続く時は、1つにまとめる
            if (r == lastRole && arr[^1] is JsonObject prev)
            {
                prev["content"] = prev["content"]!.GetValue<string>() + "\n\n" + text;
                if (hasImages) AddImages(prev, images!);
                continue;
            }
            var item = new JsonObject { ["role"] = r, ["content"] = text };
            if (hasImages) AddImages(item, images!);
            arr.Add(item);
            lastRole = r;
        }
        if (arr.Count == 0 || lastRole != "user") arr.Add(new JsonObject { ["role"] = "user", ["content"] = "(続けてください)" });
        while (arr.Count > 100) arr.RemoveAt(0);
        if (arr[0] is JsonObject first && first["role"]!.GetValue<string>() == "assistant") arr.RemoveAt(0);
        // 送る長さに合わせて、読み込める文の長さ(トークン)を広げる(日本語は 1文字 ≒ 1トークン)
        int numCtx = Math.Clamp((int)(chars * 1.2) + 2048, 4096, 32768);
        var payload = new JsonObject { ["messages"] = arr, ["num_ctx"] = numCtx, ["keep_alive"] = "10m" };
        if (!string.IsNullOrWhiteSpace(system)) payload["system"] = system.Length > 200000 ? system.Substring(0, 200000) : system;
        if (web) payload["web"] = true;
        // 処理として頼んで、進み具合を見ながら待つ
        var job = await SendAsync(HttpMethod.Post, "/api/jobs", new JsonObject { ["kind"] = "generate", ["model_id"] = modelId, ["payload"] = payload }, ct);
        var id = job?["id"]?.GetValue<string>() ?? throw new InvalidOperationException("Local AI Studio に頼めませんでした。");
        var detail = await WaitJobAsync(id, progress, ct, 500);
        return detail?["result"]?["text"]?.GetValue<string>() ?? "";
    }

    // 添付の画像(base64)。1回に8枚まで
    static void AddImages(JsonObject message, IReadOnlyList<string> images)
    {
        var list = message["images"] as JsonArray ?? new JsonArray();
        foreach (var b64 in images)
            if (list.Count < 8 && !string.IsNullOrEmpty(b64)) list.Add(b64);
        message["images"] = list;
    }

    static double? Num(JsonNode? n)
    {
        if (n is not JsonValue v) return null;
        try { return v.GetValue<double>(); } catch { }
        try { return v.GetValue<int>(); } catch { return null; }
    }

    static StudioProgress ParseProgress(JsonNode? d)
    {
        var gpu = d?["gpu"];
        string S(string k) { try { return d?[k]?.GetValue<string>() ?? ""; } catch { return ""; } }
        return new StudioProgress
        {
            State = S("status"), Phase = S("phase"), Fraction = Num(d?["progress"]), EtaSeconds = Num(d?["eta"]),
            EtaGuess = d?["eta_guess"] is JsonValue g && g.TryGetValue<bool>(out var b) && b,
            ElapsedSeconds = Num(d?["elapsed"]) ?? 0, Tokens = (int)(Num(d?["tokens"]) ?? 0), Rate = Num(d?["rate"]) ?? 0,
            GpuUtil = Num(gpu?["util"]), VramUsedMb = Num(gpu?["vram_used_mb"]), VramTotalMb = Num(gpu?["vram_total_mb"]), TempC = Num(gpu?["temp_c"]),
            LastLog = S("last_log"), ServerText = S("progress_text"),
        };
    }

    /// <summary>処理が終わるまで待つ。失敗したら、その説明で例外にする</summary>
    static async Task<JsonNode?> WaitJobAsync(string id, IProgress<StudioProgress>? progress, CancellationToken ct, int interval = 700)
    {
        while (true)
        {
            await Task.Delay(interval, ct);
            var detail = await SendAsync(HttpMethod.Get, "/api/jobs/" + id, null, ct);
            var st = detail?["status"]?.GetValue<string>() ?? "";
            if (st == "succeeded") return detail;
            if (st == "failed") throw new InvalidOperationException(detail?["error"]?.GetValue<string>() ?? "Local AI Studio で処理できませんでした。");
            progress?.Report(ParseProgress(detail));
        }
    }

    /// <summary>画像・音楽を作る。終わるまで待って、できたファイルの場所を返す。progress には進み具合(GPU・残り時間つき)が届く</summary>
    public static async Task<List<string>> GenerateAsync(string modelId, JsonObject payload, IProgress<StudioProgress>? progress, CancellationToken ct)
    {
        var job = await SendAsync(HttpMethod.Post, "/api/jobs", new JsonObject { ["kind"] = "generate", ["model_id"] = modelId, ["payload"] = payload }, ct);
        var id = job?["id"]?.GetValue<string>() ?? throw new InvalidOperationException("Local AI Studio に頼めませんでした。");
        var detail = await WaitJobAsync(id, progress, ct);
        var paths = new List<string>();
        if (detail?["result"]?["paths"] is JsonArray ps) foreach (var p in ps) if (p?.GetValue<string>() is { Length: > 0 } s) paths.Add(s);
        if (paths.Count == 0 && detail?["result"]?["path"]?.GetValue<string>() is { Length: > 0 } one) paths.Add(one);
        return paths;
    }

    /// <summary>モデルをアンインストールする(Local AI Studio の中で消す)。終わるまで待つ</summary>
    public static async Task UninstallAsync(string modelId, IProgress<StudioProgress>? progress, CancellationToken ct)
    {
        var job = await SendAsync(HttpMethod.Post, "/api/jobs", new JsonObject { ["kind"] = "uninstall", ["model_id"] = modelId }, ct);
        var id = job?["id"]?.GetValue<string>() ?? throw new InvalidOperationException("Local AI Studio に頼めませんでした。");
        await WaitJobAsync(id, progress, ct);
    }

    /// <summary>Local AI Studio を開く(起動していなければ起動する)。起動中なら false(タスクバーから開いてください)</summary>
    public static async Task<bool> OpenAppAsync()
    {
        if (await IsRunningAsync()) return false;
        var exe = FindExe() ?? throw new InvalidOperationException("Local AI Studio が見つかりませんでした。インストールしてください。");
        Process.Start(new ProcessStartInfo(exe) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(exe) ?? "" });
        return true;
    }
}
