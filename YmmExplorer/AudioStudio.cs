using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;

namespace YmmExplorer;

/// <summary>
/// MCPサーバー(Audio Studio MCP など)へ、HTTP(Streamable HTTP)でつなぐ小さなクライアント。
/// JSON-RPC の initialize → tools/list → tools/call だけを扱う。
/// </summary>
public sealed class McpHttpClient : IDisposable
{
    readonly HttpClient http = new() { Timeout = TimeSpan.FromMinutes(30) };   // 長い音の生成は、数分かかることがある
    readonly string endpoint;
    string? sessionId;
    string protocolVersion = "2025-06-18";
    int nextId = 1;

    public bool IsConnected { get; private set; }
    public string ServerName { get; private set; } = "";

    public McpHttpClient(string endpoint) => this.endpoint = endpoint;

    public void Dispose() => http.Dispose();

    async Task<JsonNode?> SendAsync(string method, JsonNode? parameters, bool notification, CancellationToken ct)
    {
        var body = new JsonObject { ["jsonrpc"] = "2.0", ["method"] = method };
        int id = 0;
        if (!notification) { id = nextId++; body["id"] = id; }
        if (parameters != null) body["params"] = parameters;

        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        req.Headers.TryAddWithoutValidation("Accept", "application/json, text/event-stream");
        if (sessionId != null) req.Headers.TryAddWithoutValidation("Mcp-Session-Id", sessionId);
        if (IsConnected) req.Headers.TryAddWithoutValidation("MCP-Protocol-Version", protocolVersion);

        using var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
        if (res.Headers.TryGetValues("Mcp-Session-Id", out var sid)) sessionId = sid.FirstOrDefault() ?? sessionId;
        if (notification) return null;
        if (!res.IsSuccessStatusCode)
        {
            var text = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new InvalidOperationException($"サーバーがエラーを返しました({(int)res.StatusCode}): {Short(text)}");
        }

        var type = res.Content.Headers.ContentType?.MediaType ?? "";
        JsonNode? reply = null;
        if (type.Contains("event-stream", StringComparison.OrdinalIgnoreCase))
        {
            // SSE: 「data: {...}」の行を順に読み、この要求の id の返事を探す(途中の進み具合の通知は飛ばす)
            using var stream = await res.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var data = new StringBuilder();
            string? line;
            while ((line = await reader.ReadLineAsync(ct).ConfigureAwait(false)) != null)
            {
                if (line.StartsWith("data:", StringComparison.Ordinal)) { data.Append(line.AsSpan(5).TrimStart()); continue; }
                if (line.Length != 0 || data.Length == 0) continue;
                var node = TryParse(data.ToString());
                data.Clear();
                if (node?["id"] is JsonNode nid && nid.ToString() == id.ToString()) { reply = node; break; }
            }
            if (reply == null && data.Length > 0) reply = TryParse(data.ToString());
        }
        else reply = TryParse(await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false));

        if (reply == null) throw new InvalidOperationException("サーバーの返事を読めませんでした");
        if (reply["error"] is JsonNode err) throw new InvalidOperationException(err["message"]?.ToString() ?? err.ToJsonString());
        return reply["result"];
    }

    static JsonNode? TryParse(string text)
    {
        try { return JsonNode.Parse(text); }
        catch { return null; }
    }

    static string Short(string s) => s.Length > 300 ? s.Substring(0, 300) + "…" : s;

    public async Task ConnectAsync(CancellationToken ct)
    {
        IsConnected = false;
        sessionId = null;
        var result = await SendAsync("initialize", new JsonObject
        {
            ["protocolVersion"] = protocolVersion,
            ["capabilities"] = new JsonObject(),
            ["clientInfo"] = new JsonObject { ["name"] = "YmmExplorer", ["version"] = "1.2.1" },
        }, false, ct).ConfigureAwait(false);
        protocolVersion = result?["protocolVersion"]?.ToString() ?? protocolVersion;
        ServerName = result?["serverInfo"]?["name"]?.ToString() ?? "";
        IsConnected = true;
        await SendAsync("notifications/initialized", null, true, ct).ConfigureAwait(false);
    }

    public async Task<JsonArray> ListToolsAsync(CancellationToken ct)
    {
        var result = await SendAsync("tools/list", new JsonObject(), false, ct).ConfigureAwait(false);
        return result?["tools"] as JsonArray ?? new JsonArray();
    }

    public async Task<JsonNode?> CallToolAsync(string name, JsonObject arguments, CancellationToken ct)
        => await SendAsync("tools/call", new JsonObject { ["name"] = name, ["arguments"] = arguments }, false, ct).ConfigureAwait(false);
}

/// <summary>
/// ツールの入力欄1つ(MCPサーバーが教えてくれる入力の形 inputSchema から、自動で作る)。
/// 文字・数・はい/いいえ・選択肢 に対応する。
/// </summary>
public sealed class McpField : NotifyBase
{
    string text = "";
    bool flag;
    string? choice;

    public string Name { get; }
    public string Label { get; }
    public string Tip { get; }
    public string Type { get; }
    public bool Required { get; }
    public List<string> Choices { get; } = new();

    public bool IsText => Choices.Count == 0 && Type is "string" or "" or "array" or "object";
    /// <summary>1行の入力欄で出すか(長い指示の欄は、複数行で出す)</summary>
    public bool IsSingleLine => IsText && !IsLongText;
    public bool IsLongText { get; }
    public bool IsNumber => Choices.Count == 0 && Type is "number" or "integer";
    public bool IsBool => Type == "boolean";
    public bool IsChoice => Choices.Count > 0;
    /// <summary>ファイルの場所を入れる欄(「選択中の素材を使う」ボタンを出す)</summary>
    public bool IsPath { get; }

    public string Text { get => text; set => Set(ref text, value ?? ""); }
    public bool Flag { get => flag; set => Set(ref flag, value); }
    public string? Choice { get => choice; set => Set(ref choice, value); }

    static readonly Dictionary<string, string> Japanese = new(StringComparer.OrdinalIgnoreCase)
    {
        ["prompt"] = "指示(どんな音か)", ["text"] = "指示(どんな音か)", ["description"] = "指示(どんな音か)",
        ["negative_prompt"] = "入れたくない音", ["audio_type"] = "種類", ["bpm"] = "テンポ(BPM)", ["variations"] = "候補の数(1〜4)",
        ["seed"] = "シード(-1 でランダム)", ["duration"] = "長さ(秒)", ["seconds"] = "長さ(秒)", ["duration_seconds"] = "長さ(秒)",
        ["steps"] = "ステップ数", ["cfg_scale"] = "指示への忠実さ", ["model"] = "モデル", ["mode"] = "編集のしかた",
        ["input_path"] = "元の音声ファイル", ["path"] = "音声ファイル", ["audio_path"] = "音声ファイル", ["file"] = "音声ファイル",
        ["start"] = "開始(秒)", ["end"] = "終了(秒)", ["format"] = "書き出す形式", ["loudness"] = "ラウドネス(LUFS)",
        ["target_lufs"] = "ラウドネス(LUFS)", ["loop"] = "ループにする", ["fade_in"] = "フェードイン(秒)", ["fade_out"] = "フェードアウト(秒)",
        ["peak_ceiling_db"] = "ピークの上限(dB)", ["sound_source"] = "効果音: 何が", ["sound_action"] = "効果音: どうする",
        ["sound_material"] = "効果音: 材質", ["sound_space"] = "効果音: 場所の響き", ["mono"] = "モノラルにする",
        ["sample_rate"] = "サンプルレート", ["output_name"] = "出力ファイル名",
    };

    public McpField(string name, JsonNode schema, bool required)
    {
        Name = name;
        Required = required;
        Type = schema["type"]?.ToString() ?? "";
        if (Type.StartsWith("[", StringComparison.Ordinal)) Type = (schema["type"] as JsonArray)?.Select(x => x?.ToString()).FirstOrDefault(x => x != "null") ?? "";
        var desc = schema["description"]?.ToString() ?? "";
        var title = schema["title"]?.ToString();
        Label = Japanese.TryGetValue(name, out var jp) ? jp : !string.IsNullOrWhiteSpace(title) ? title! : name;
        Tip = (desc.Length > 0 ? desc : Label) + $"  ({name}{(required ? "・必須" : "")})";
        if (schema["enum"] is JsonArray en) foreach (var v in en) if (v != null) Choices.Add(v.ToString());
        if (Choices.Count > 0 && !required) Choices.Insert(0, "");
        var lname = name.ToLowerInvariant();
        IsPath = Type is "string" or "" && Choices.Count == 0
                 && (lname.Contains("path") || lname.Contains("file") || lname == "input" || lname.EndsWith("_audio") || lname == "audio");
        IsLongText = !IsPath && Type is "string" or "" && (lname.Contains("prompt") || lname == "text" || lname == "description");
        var def = schema["default"];
        if (def != null)
        {
            if (IsBool) flag = def.ToString() == "true" || def.ToString() == "True";
            else if (IsChoice) choice = def.ToString();
            else text = def.ToString();
        }
    }

    /// <summary>入力された値を、サーバーへ送る形にする(空のものは送らない)</summary>
    public bool TryValue(out JsonNode? value, out string error)
    {
        value = null;
        error = "";
        if (IsBool) { value = flag; return true; }
        var raw = IsChoice ? choice ?? "" : text.Trim();
        if (raw.Length == 0)
        {
            if (Required) { error = $"「{Label}」を入れてください"; return false; }
            return true;
        }
        if (Type == "integer")
        {
            if (!long.TryParse(raw, out var n)) { error = $"「{Label}」は整数で入れてください"; return false; }
            value = n; return true;
        }
        if (Type == "number")
        {
            if (!double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d))
            { error = $"「{Label}」は数で入れてください"; return false; }
            value = d; return true;
        }
        if (Type is "array" or "object")
        {
            try { value = JsonNode.Parse(raw); return true; }
            catch { error = $"「{Label}」は JSON の形で入れてください"; return false; }
        }
        value = raw;
        return true;
    }
}

/// <summary>例のボタン1つ(表示名と、入る文字)</summary>
public sealed record StudioExample(string Label, string Prompt);

/// <summary>サーバーのツール1つ(generate_audio など)</summary>
public sealed class McpTool
{
    public string Name { get; }
    public string Label { get; }
    public string Description { get; }
    public List<McpField> Fields { get; } = new();

    static readonly Dictionary<string, string> Japanese = new()
    {
        ["generate_audio"] = "新しく作る(BGM・楽器・効果音)", ["edit_audio"] = "作り直す・延ばす(編集)",
        ["process_audio"] = "仕上げる(ループ・音量・書き出し)", ["unload_models"] = "モデルをメモリから外す",
    };

    public McpTool(JsonNode tool)
    {
        Name = tool["name"]?.ToString() ?? "";
        Label = Japanese.TryGetValue(Name, out var jp) ? $"{jp}  ({Name})" : Name;
        Description = tool["description"]?.ToString() ?? "";
        var schema = tool["inputSchema"];
        var required = new HashSet<string>((schema?["required"] as JsonArray)?.Select(x => x?.ToString() ?? "") ?? Array.Empty<string>());
        if (schema?["properties"] is JsonObject props)
            foreach (var kv in props)
                if (kv.Value != null) Fields.Add(new McpField(kv.Key, kv.Value, required.Contains(kv.Key)));
        // 指示(プロンプト)を先頭に、必須の欄をその次に並べる
        Fields.Sort((a, b) => Rank(a).CompareTo(Rank(b)));
    }

    static int Rank(McpField f) => f.IsLongText ? 0 : f.Required ? 1 : f.IsChoice ? 2 : 3;
}

/// <summary>
/// 素材データベースの「AI音声生成」: Audio Studio MCP を起動しておき、このプラグインの画面から音を作って、そのまま素材として取り込む。
/// サーバーの入力の形は、つないだ時にサーバーから教えてもらって、入力欄を自動で作る(サーバーの版が変わっても合わせられる)。
/// </summary>
/// <summary>ファイルを、ごみ箱へ移す(Windows の SHFileOperation)</summary>
public static class RecycleBin
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public uint wFunc;
        public string pFrom;
        public string? pTo;
        public ushort fFlags;
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string? lpszProgressTitle;
    }

    [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    static extern int SHFileOperation(ref SHFILEOPSTRUCT op);

    const uint FO_DELETE = 3;
    const ushort FOF_SILENT = 0x4, FOF_NOCONFIRMATION = 0x10, FOF_ALLOWUNDO = 0x40, FOF_NOERRORUI = 0x400;

    public static bool Send(string path)
    {
        try
        {
            var op = new SHFILEOPSTRUCT
            {
                wFunc = FO_DELETE,
                pFrom = path + "\0\0",
                fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT | FOF_NOERRORUI,
            };
            return SHFileOperation(ref op) == 0 && !File.Exists(path);
        }
        catch { return false; }
    }
}

/// <summary>生成後の「どれを取り込むか選ぶ」画面の1候補</summary>
public sealed class StudioCandidate : NotifyBase
{
    public StudioCandidate(string path, int number)
    {
        Path = path;
        Number = number;
    }

    public string Path { get; }
    public int Number { get; }
    public string Title => $"候補 {Number}";
    public string FileName => System.IO.Path.GetFileName(Path);

    double duration;
    public string DurationText => duration <= 0 ? "" : TimeSpan.FromSeconds(duration).ToString(duration >= 3600 ? @"h\:mm\:ss" : @"m\:ss");

    System.Windows.Media.PointCollection wave = new();
    /// <summary>波形(幅 300 × 高さ 48 の中の、上下の折れ線)</summary>
    public System.Windows.Media.PointCollection Wave { get => wave; private set => Set(ref wave, value); }

    bool isPlaying, isAdded;
    double progress;
    public bool IsPlaying { get => isPlaying; set { if (Set(ref isPlaying, value)) Raise(nameof(PlayText)); } }
    public string PlayText => isPlaying ? "■ 止める" : "▶ 聴く";
    public bool IsAdded { get => isAdded; set { if (Set(ref isAdded, value)) Raise(nameof(AddText)); } }
    public string AddText => isAdded ? "✓ 取り込み済み" : "＋ これを取り込む";
    /// <summary>再生位置(0〜1)</summary>
    public double Progress { get => progress; set => Set(ref progress, value); }

    public async Task LoadAsync()
    {
        try
        {
            duration = await MaterialStore.ReadDurationAsync(Path, true);
            Raise(nameof(DurationText));
            var peaks = await MaterialStore.ComputePeaksAsync(Path, 150);
            if (peaks == null || peaks.Length == 0) return;
            var pts = new System.Windows.Media.PointCollection();
            double step = 300.0 / peaks.Length;
            for (int i = 0; i < peaks.Length; i++) pts.Add(new System.Windows.Point(i * step, 24 - Math.Clamp(peaks[i], 0, 1) * 23));
            for (int i = peaks.Length - 1; i >= 0; i--) pts.Add(new System.Windows.Point(i * step, 24 + Math.Clamp(peaks[i], 0, 1) * 23));
            pts.Freeze();
            Wave = pts;
        }
        catch { }
    }
}

public sealed class AudioStudioVm : NotifyBase
{
    static readonly string[] AudioExt = { ".wav", ".ogg", ".flac", ".mp3" };
    readonly MaterialLibraryVm library;
    readonly ExplorerSettings settings = ExplorerSettings.Instance;
    readonly DispatcherTimer clock = new() { Interval = TimeSpan.FromSeconds(1) };
    McpHttpClient? client;
    /// <summary>起動したサーバー(全ウィンドウで1つ。YMM4の終了時に止める)</summary>
    static Process? server;
    static bool exitHooked;
    CancellationTokenSource? running;
    DateTime startedAt;

    bool isOpen, isBusy, isConnected;
    string status = "まだつないでいません", log = "", resultText = "";
    McpTool? tool;

    public ObservableCollection<McpTool> Tools { get; } = new();
    public ICommand ConnectCommand { get; }
    public ICommand StartServerCommand { get; }
    public ICommand StopServerCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand UseSelectedCommand { get; }
    public ICommand OpenWebUiCommand { get; }
    public ICommand OpenOutputCommand { get; }

    // ------------------------------------------------------------------ YMM4を閉じる時
    /// <summary>生成中かどうか(全ウィンドウ共通。閉じる時の確認で使う)</summary>
    static int generatingCount;

    /// <summary>
    /// YMM4のウィンドウを閉じる時に、Audio Studio MCP が動いていたら確認を出し、「はい」なら終了させてから閉じる。
    /// 「いいえ」なら、YMM4を閉じるのを取りやめる。
    /// </summary>
    static void HookExit()
    {
        if (exitHooked) return;
        var app = System.Windows.Application.Current;
        var main = app?.MainWindow;
        if (app == null || main == null) return;
        exitHooked = true;
        main.Closing += OnMainClosing;
        app.Exit += (_, _) => KillAll();          // 念のため(確認を通らずに終わった時も止める)
    }

    /// <summary>このプラグインが起動したサーバーと、外で起動された audio-studio-mcp の両方</summary>
    static List<Process> RunningServers()
    {
        var list = new List<Process>();
        try { if (server is { HasExited: false }) list.Add(server); } catch { }
        try
        {
            foreach (var p in Process.GetProcessesByName("audio-studio-mcp"))
                if (!list.Any(x => x.Id == p.Id)) list.Add(p);
        }
        catch { }
        return list;
    }

    static void OnMainClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (e.Cancel) return;                     // YMM4側で閉じるのを取りやめた時(保存の確認で「キャンセル」など)は、何もしない
        var running = RunningServers();
        if (running.Count == 0) return;
        var msg = "AI音声生成(Audio Studio MCP)が、まだ動いています。\n"
                  + (generatingCount > 0 ? "いま音を作っている途中です。閉じると、作りかけの音は失われます。\n" : "")
                  + "\nYMM4を閉じると、Audio Studio MCP も自動で終了します(GPUのメモリも空きます)。\nよろしいですか?";
        var answer = System.Windows.MessageBox.Show(sender as System.Windows.Window, msg, "YmmExplorer",
            System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning);
        if (answer != System.Windows.MessageBoxResult.OK) { e.Cancel = true; return; }
        KillAll();
    }

    static void KillAll()
    {
        foreach (var p in RunningServers())
        {
            try { p.Kill(true); p.WaitForExit(3000); } catch { }
        }
        server = null;
    }

    public AudioStudioVm(MaterialLibraryVm library)
    {
        this.library = library;
        // 自分で起動した Audio Studio MCP につないだ場合も、YMM4を閉じる時に確認できるようにする
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(HookExit));
        ConnectCommand = new RelayCommand(async _ => await ConnectAsync(), _ => !isBusy);
        StartServerCommand = new RelayCommand(async _ => await StartServerAsync(), _ => !isBusy);
        StopServerCommand = new RelayCommand(_ => StopServer(), _ => server is { HasExited: false });
        RunCommand = new RelayCommand(async _ => await RunAsync(), _ => !isBusy && isConnected && tool != null);
        CancelCommand = new RelayCommand(_ => running?.Cancel(), _ => isBusy);
        CloseCommand = new RelayCommand(_ => IsOpen = false);
        UseSelectedCommand = new RelayCommand(p =>
        {
            var path = library.SelectedPaths().FirstOrDefault();
            if (p is McpField f && path != null) f.Text = path;
            else if (path == null) Status = "⚠ 素材データベースで、元にする音声を選んでから押してください";
        });
        OpenWebUiCommand = new RelayCommand(_ => Open($"http://127.0.0.1:{Port}/"));
        OpenOutputCommand = new RelayCommand(_ => { if (Directory.Exists(OutputDir)) Open(OutputDir); else Status = "⚠ 出力フォルダーが見つかりません。設定で指定してください"; });
        clock.Tick += (_, _) => Tick();
    }

    // ------------------------------------------------------------------ 生成後の「どれを取り込むか選ぶ」画面
    public ObservableCollection<StudioCandidate> Candidates { get; } = new();
    bool isPicking;
    public bool IsPicking { get => isPicking; private set => Set(ref isPicking, value); }
    public string PickTitle => Candidates.Count <= 1 ? "できあがりました" : $"{Candidates.Count} 個できました。聴き比べて、取り込むものを選んでください";
    List<string> lastTags = new();
    string lastMemo = "";
    readonly System.Windows.Media.MediaPlayer player = new();
    readonly DispatcherTimer playTimer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    StudioCandidate? playing;

    ICommand? playCandidateCommand, addCandidateCommand, addAllCommand, deleteCandidateCommand, regenerateCommand, closePickCommand, showFileCommand;
    public ICommand PlayCandidateCommand => playCandidateCommand ??= new RelayCommand(p => { if (p is StudioCandidate c) TogglePlay(c); });
    public ICommand AddCandidateCommand => addCandidateCommand ??= new RelayCommand(async p => { if (p is StudioCandidate c && !c.IsAdded) await AddCandidatesAsync(new[] { c }); });
    public ICommand AddAllCandidatesCommand => addAllCommand ??= new RelayCommand(async _ => await AddCandidatesAsync(Candidates.Where(c => !c.IsAdded).ToList()));
    public ICommand DeleteCandidateCommand => deleteCandidateCommand ??= new RelayCommand(p => { if (p is StudioCandidate c) DeleteCandidate(c); });
    public ICommand ShowCandidateFileCommand => showFileCommand ??= new RelayCommand(p =>
    {
        if (p is StudioCandidate c) try { Process.Start("explorer.exe", $"/select,\"{c.Path}\""); } catch { }
    });
    /// <summary>同じ設定で、もう一度作る</summary>
    public ICommand RegenerateCommand => regenerateCommand ??= new RelayCommand(async _ =>
    {
        ClosePick();
        await RunAsync();
    }, _ => !isBusy && isConnected && tool != null);
    public ICommand ClosePickCommand => closePickCommand ??= new RelayCommand(_ => ClosePick());

    void ShowPick(IReadOnlyList<string> files, List<string> tags, string memo)
    {
        StopPlay();
        Candidates.Clear();
        int n = 1;
        foreach (var f in files)
        {
            var c = new StudioCandidate(f, n++);
            Candidates.Add(c);
            _ = c.LoadAsync();
        }
        lastTags = tags;
        lastMemo = memo;
        Raise(nameof(PickTitle));
        IsPicking = Candidates.Count > 0;
    }

    void ClosePick()
    {
        StopPlay();
        IsPicking = false;
    }

    void TogglePlay(StudioCandidate c)
    {
        bool same = playing == c && c.IsPlaying;
        StopPlay();
        if (same) return;
        try
        {
            if (!playTimerHooked)
            {
                playTimerHooked = true;
                playTimer.Tick += (_, _) =>
                {
                    if (playing == null) return;
                    var len = player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan.TotalSeconds : 0;
                    playing.Progress = len > 0 ? Math.Clamp(player.Position.TotalSeconds / len, 0, 1) : 0;
                };
                player.MediaEnded += (_, _) => StopPlay();
                player.MediaFailed += (_, e) => { StopPlay(); Status = "⚠ 再生できませんでした: " + e.ErrorException?.Message; };
            }
            player.Open(new Uri(AudioCompat.PlayablePath(c.Path)));      // 24bit などのWAVは、再生できる形に直してから
            player.Play();
            playing = c;
            c.IsPlaying = true;
            playTimer.Start();
        }
        catch (Exception ex) { Status = "⚠ 再生できませんでした: " + ex.Message; }
    }
    bool playTimerHooked;

    void StopPlay()
    {
        try { player.Stop(); player.Close(); } catch { }
        playTimer.Stop();
        if (playing != null) { playing.IsPlaying = false; playing.Progress = 0; }
        playing = null;
    }

    async Task AddCandidatesAsync(IReadOnlyList<StudioCandidate> list)
    {
        if (list.Count == 0) return;
        await library.ImportGeneratedAsync(list.Select(c => c.Path).ToList(), lastTags, lastMemo);
        foreach (var c in list) c.IsAdded = true;
        Status = $"{list.Count} 個を、素材データベースに取り込みました";
    }

    void DeleteCandidate(StudioCandidate c)
    {
        if (playing == c) StopPlay();
        try
        {
            // ごみ箱へ(元に戻せるように)
            if (!RecycleBin.Send(c.Path)) File.Delete(c.Path);
        }
        catch (Exception ex)
        {
            if (File.Exists(c.Path)) { Status = "⚠ 削除できませんでした: " + ex.Message; return; }
        }
        Candidates.Remove(c);
        Raise(nameof(PickTitle));
        Status = c.IsAdded ? "ファイルを削除しました(素材データベースに取り込んだコピーは残っています)" : "ファイルをごみ箱へ移しました";
        if (Candidates.Count == 0) IsPicking = false;
    }

    static void Open(string target)
    {
        try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); } catch { }
    }

    // ------------------------------------------------------------------ 設定(設定ページの「YMM4連携」から変える)
    public string ExePath { get => settings.AudioStudioExe ?? ""; set { settings.AudioStudioExe = value?.Trim().Trim('"'); settings.Save(); Raise(nameof(ExePath)); } }
    public int Port { get => settings.AudioStudioPort is > 0 and < 65536 ? settings.AudioStudioPort : 8765; set { settings.AudioStudioPort = value; settings.Save(); Raise(nameof(Port)); Raise(nameof(Endpoint)); } }
    public string OutputDir { get => settings.AudioStudioOutput ?? ""; set { settings.AudioStudioOutput = value?.Trim().Trim('"'); settings.Save(); Raise(nameof(OutputDir)); } }
    public bool WithWebUi { get => settings.AudioStudioWebUi; set { settings.AudioStudioWebUi = value; settings.Save(); Raise(nameof(WithWebUi)); } }
    /// <summary>オン: できた音をすべて自動で取り込む / オフ: 聴き比べて選ぶ画面を出す</summary>
    public bool AutoImport { get => settings.AudioStudioImportAll; set { settings.AudioStudioImportAll = value; settings.Save(); Raise(nameof(AutoImport)); } }
    public string ImportTags { get => settings.AudioStudioTags ?? "AI生成"; set { settings.AudioStudioTags = value; settings.Save(); Raise(nameof(ImportTags)); } }
    public string Endpoint => $"http://127.0.0.1:{Port}/mcp";

    // ------------------------------------------------------------------ 画面の状態
    public bool IsOpen
    {
        get => isOpen;
        set
        {
            if (!Set(ref isOpen, value)) return;
            if (value && !isConnected && !isBusy) _ = ConnectAsync(quiet: true);
        }
    }

    public bool IsBusy { get => isBusy; private set { if (Set(ref isBusy, value)) { Raise(nameof(IsIdle)); CommandManager.InvalidateRequerySuggested(); } } }
    public bool IsIdle => !isBusy;
    public bool IsConnected { get => isConnected; private set { if (Set(ref isConnected, value)) { Raise(nameof(IsDisconnected)); CommandManager.InvalidateRequerySuggested(); } } }
    public bool IsDisconnected => !isConnected;
    public string Status { get => status; private set => Set(ref status, value); }
    public string Log { get => log; private set => Set(ref log, value); }
    public string ResultText { get => resultText; private set => Set(ref resultText, value); }
    public string ElapsedText => isBusy ? $"{(int)(DateTime.Now - startedAt).TotalSeconds} 秒" : "";

    // ------------------------------------------------------------------ 生成中の画面(経過時間・GPUの使用率)
    bool isGenerating;
    string generatingTitle = "", generatingDetail = "";
    double gpuUsage, gpuMemUsed, gpuMemTotal;
    string gpuName = "";
    bool hasGpu, gpuReading;
    readonly List<double> gpuHistory = new();

    /// <summary>生成中(専用の画面を出す)</summary>
    public bool IsGenerating { get => isGenerating; private set => Set(ref isGenerating, value); }
    public string GeneratingTitle { get => generatingTitle; private set => Set(ref generatingTitle, value); }
    public string GeneratingDetail { get => generatingDetail; private set => Set(ref generatingDetail, value); }
    public string ElapsedClock
    {
        get
        {
            var t = DateTime.Now - startedAt;
            return isGenerating ? $"{(int)t.TotalMinutes:00}:{t.Seconds:00}" : "00:00";
        }
    }
    public bool HasGpu => hasGpu;
    public bool NoGpu => !hasGpu;
    public double GpuUsage => gpuUsage;
    public string GpuUsageText => hasGpu ? $"{gpuUsage:0} %" : "—";
    public double GpuMemPercent => gpuMemTotal > 0 ? gpuMemUsed / gpuMemTotal * 100 : 0;
    public string GpuMemText => hasGpu ? $"{gpuMemUsed / 1024:0.0} / {gpuMemTotal / 1024:0.0} GB" : "—";
    public string GpuName => hasGpu ? gpuName : "GPUの情報を取れませんでした(NVIDIA の nvidia-smi が見つかりません)";

    /// <summary>使用率の移り変わり(グラフ。幅 120 × 高さ 40)</summary>
    public System.Windows.Media.PointCollection GpuGraph
    {
        get
        {
            var pts = new System.Windows.Media.PointCollection();
            var list = gpuHistory.ToList();
            for (int i = 0; i < list.Count; i++)
                pts.Add(new System.Windows.Point(120.0 * i / Math.Max(1, 59), 40 - list[i] / 100 * 38));
            pts.Freeze();
            return pts;
        }
    }

    /// <summary>1秒ごと: 経過時間を進め、GPUを調べる(調べるのは裏で行い、終わっていなければ次に回す)</summary>
    void Tick()
    {
        Raise(nameof(ElapsedText));
        Raise(nameof(ElapsedClock));
        if (!isGenerating || gpuReading) return;
        gpuReading = true;
        Task.Run(GpuMonitor.Read).ContinueWith(t =>
        {
            gpuReading = false;
            var info = t.Status == TaskStatus.RanToCompletion ? t.Result : null;
            App(() =>
            {
                hasGpu = info != null;
                if (info != null)
                {
                    gpuName = info.Name;
                    gpuUsage = info.Usage;
                    gpuMemUsed = info.MemUsedMb;
                    gpuMemTotal = info.MemTotalMb;
                    gpuHistory.Add(info.Usage);
                    if (gpuHistory.Count > 60) gpuHistory.RemoveAt(0);
                }
                foreach (var n in new[] { nameof(HasGpu), nameof(NoGpu), nameof(GpuUsage), nameof(GpuUsageText), nameof(GpuMemPercent),
                                          nameof(GpuMemText), nameof(GpuName), nameof(GpuGraph) })
                    Raise(n);
            });
        }, TaskScheduler.Default);
    }
    public bool ServerRunning => server is { HasExited: false };

    public McpTool? Tool
    {
        get => tool;
        set { if (Set(ref tool, value)) { Raise(nameof(ToolDescription)); RaiseSimple(); CommandManager.InvalidateRequerySuggested(); } }
    }
    public string ToolDescription => tool?.Description ?? "";

    // ------------------------------------------------------------------ 「新しく作る」のわかりやすい画面
    // 入力欄の名前は、サーバーが教えてくれた中から探す(名前が違っても、それらしいものを使う)
    McpField? Field(params string[] names)
    {
        var fields = tool?.Fields;
        if (fields == null) return null;
        foreach (var n in names)
        {
            var hit = fields.FirstOrDefault(f => string.Equals(f.Name, n, StringComparison.OrdinalIgnoreCase));
            if (hit != null) return hit;
        }
        foreach (var n in names)
        {
            var hit = fields.FirstOrDefault(f => f.Name.Contains(n, StringComparison.OrdinalIgnoreCase));
            if (hit != null) return hit;
        }
        return null;
    }

    McpField? PromptField => tool?.Fields.FirstOrDefault(f => f.IsLongText) ?? Field("prompt", "text", "description");
    McpField? TypeField => Field("audio_type", "type");
    McpField? DurationField => Field("duration", "seconds", "length", "duration_seconds");
    McpField? BpmField => Field("bpm", "tempo");
    McpField? VariationsField => Field("variations", "num_variations", "count");

    /// <summary>「新しく作る」の時は、わかりやすい画面を出す</summary>
    public bool IsSimple => tool != null && (tool.Name == "generate_audio" || (PromptField != null && TypeField != null));
    public bool IsGeneric => !IsSimple;

    string kind = "music";
    public bool KindMusic { get => kind == "music"; set { if (value) SetKind("music"); } }
    public bool KindInstrument { get => kind == "instrument"; set { if (value) SetKind("instrument"); } }
    public bool KindSfx { get => kind == "sfx"; set { if (value) SetKind("sfx"); } }

    void SetKind(string k)
    {
        kind = k;
        foreach (var n in new[] { nameof(KindMusic), nameof(KindInstrument), nameof(KindSfx), nameof(KindHint), nameof(PromptLabel),
                                  nameof(PromptHint), nameof(Examples), nameof(ShowBpm), nameof(ShowSfxFields), nameof(DurationMax),
                                  nameof(DurationPresets) })
            Raise(n);
        if (Duration > DurationMax) Duration = DurationMax;
    }

    public string KindHint => kind switch
    {
        "sfx" => "効果音: 足音・ドア・爆発・魔法など、短い音",
        "instrument" => "楽器ソロ/ループ: 1つの楽器だけの演奏(ほかの曲と重ねる素材に)",
        _ => "BGM: 複数の楽器による1曲",
    };
    public string PromptLabel => kind == "sfx" ? "効果音の説明(英語)" : "音楽の説明(英語)";
    public string PromptHint => kind == "sfx"
        ? "何の音か・どう鳴るか・場所の響きを、短い英語で(例: Heavy wooden door slowly creaking open in a large hall)"
        : "ジャンル・主な楽器・リズム・雰囲気を、1〜2文の英語で。歌(ボーカル・歌詞)は作れません";

    public string Prompt
    {
        get => PromptField?.Text ?? "";
        set { if (PromptField is { } f) { f.Text = value ?? ""; Raise(nameof(Prompt)); } }
    }

    /// <summary>説明の例(押すと、説明の欄に入る)</summary>
    public List<StudioExample> Examples => kind switch
    {
        "sfx" => new()
        {
            new("ドア", "Old wooden door creaking open slowly"), new("足音", "Footsteps on gravel, walking at a steady pace"),
            new("爆発", "Distant explosion with deep rumble and debris"), new("魔法", "Magical sparkle chime spell, bright and shimmering"),
            new("決定音", "Short bright UI confirm sound, clean digital blip"), new("雨", "Steady rain on a window with soft thunder far away"),
        },
        "instrument" => new()
        {
            new("ピアノ", "Gentle solo piano melody, slow and emotional"), new("アコギ", "Fingerpicked acoustic guitar loop, warm and relaxed"),
            new("ドラム", "Punchy hip-hop drum loop, boom bap"), new("シンセ", "Retro synth arpeggio loop, 80s style"),
            new("ベース", "Funky slap bass groove loop"),
        },
        _ => new()
        {
            new("Lo-fi チル", "Lo-fi hip-hop track with mellow electric piano, soft vinyl crackle and relaxed drums"),
            new("8bit 冒険", "Upbeat 8-bit chiptune adventure theme with catchy melody"),
            new("壮大なバトル", "Epic orchestral battle music with powerful drums, brass and choir pads"),
            new("穏やかなアンビエント", "Calm ambient soundscape with soft pads and gentle textures"),
            new("ほのぼの日常", "Cheerful and cute daily life music with ukulele, glockenspiel and light percussion"),
            new("ホラー", "Dark suspenseful horror ambience with eerie strings and low drones"),
        },
    };

    ICommand? exampleCommand;
    public ICommand ExampleCommand => exampleCommand ??= new RelayCommand(p => { if (p is StudioExample e) Prompt = e.Prompt; });

    // 長さ
    bool autoDuration = true;
    double duration = 60;
    public bool HasDuration => DurationField != null;
    public bool AutoDuration { get => autoDuration; set { if (Set(ref autoDuration, value)) { Raise(nameof(ManualDuration)); Raise(nameof(DurationText)); } } }
    public bool ManualDuration => !autoDuration;
    public double DurationMax => kind == "sfx" ? 30 : 380;
    public double Duration { get => duration; set { if (Set(ref duration, Math.Round(Math.Clamp(value, 1, DurationMax)))) Raise(nameof(DurationText)); } }
    public string DurationText => autoDuration ? "おまかせ" : duration >= 60 ? $"{(int)duration / 60}分{(int)duration % 60:00}秒" : $"{duration:0} 秒";
    public List<StudioExample> DurationPresets => kind == "sfx"
        ? new() { new("おまかせ", "auto"), new("1秒", "1"), new("3秒", "3"), new("5秒", "5"), new("10秒", "10") }
        : new() { new("おまかせ", "auto"), new("30秒", "30"), new("1分", "60"), new("2分", "120"), new("3分", "180") };
    ICommand? durationPresetCommand;
    public ICommand DurationPresetCommand => durationPresetCommand ??= new RelayCommand(p =>
    {
        if (p is not StudioExample e) return;
        if (e.Prompt == "auto") { AutoDuration = true; return; }
        AutoDuration = false;
        Duration = double.Parse(e.Prompt, System.Globalization.CultureInfo.InvariantCulture);
    });

    // テンポ
    bool autoBpm = true;
    double bpm = 100;
    public bool ShowBpm => BpmField != null && kind != "sfx";
    public bool AutoBpm { get => autoBpm; set { if (Set(ref autoBpm, value)) { Raise(nameof(ManualBpm)); Raise(nameof(BpmText)); } } }
    public bool ManualBpm => !autoBpm;
    public double Bpm { get => bpm; set { if (Set(ref bpm, Math.Round(Math.Clamp(value, 40, 220)))) Raise(nameof(BpmText)); } }
    public string BpmText => autoBpm ? "自動" : $"{bpm:0} BPM";

    // 候補の数
    double variations = 2;
    public bool HasVariations => VariationsField != null;
    public double Variations { get => variations; set { if (Set(ref variations, Math.Round(Math.Clamp(value, 1, 4)))) { Raise(nameof(VariationsText)); Raise(nameof(RunText)); } } }
    public string VariationsText => $"{variations:0} 個";
    public string RunText => IsSimple ? (HasVariations ? $"▶ {variations:0} 個作って取り込む" : "▶ 作って取り込む") : "▶ 実行して取り込む";

    /// <summary>効果音の細かい指定(何が・どうする・材質・場所の響き)</summary>
    public bool ShowSfxFields => kind == "sfx" && SfxFields.Count > 0;
    public List<McpField> SfxFields => tool?.Fields.Where(f => f.Name.StartsWith("sound_", StringComparison.OrdinalIgnoreCase)).ToList() ?? new();

    /// <summary>詳細設定: わかりやすい画面で扱っていない、残りの入力欄(シードなど)</summary>
    public List<McpField> AdvancedFields
    {
        get
        {
            var handled = new HashSet<McpField?> { PromptField, TypeField, DurationField, BpmField, VariationsField };
            return tool?.Fields.Where(f => !handled.Contains(f) && !f.Name.StartsWith("sound_", StringComparison.OrdinalIgnoreCase)).ToList() ?? new();
        }
    }

    void RaiseSimple()
    {
        if (TypeField is { } tf && !string.IsNullOrEmpty(tf.Choice) && tf.Choice is "music" or "instrument" or "sfx") kind = tf.Choice!;
        foreach (var n in new[] { nameof(IsSimple), nameof(IsGeneric), nameof(Prompt), nameof(HasDuration), nameof(ShowBpm), nameof(HasVariations),
                                  nameof(SfxFields), nameof(AdvancedFields), nameof(RunText) })
            Raise(n);
        SetKind(kind);
    }

    /// <summary>わかりやすい画面の値を、サーバーへ送る入力欄へ書き込む</summary>
    void ApplySimpleForm()
    {
        if (!IsSimple) return;
        if (TypeField is { } tf)
        {
            if (tf.IsChoice) tf.Choice = tf.Choices.Contains(kind) ? kind : tf.Choice;
            else tf.Text = kind;
        }
        if (DurationField is { } df) df.Text = autoDuration ? "" : duration.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (BpmField is { } bf) bf.Text = autoBpm || kind == "sfx" ? "" : bpm.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (VariationsField is { } vf) vf.Text = variations.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    void AddLog(string line)
    {
        var text = $"[{DateTime.Now:HH:mm:ss}] {line}\n" + log;
        Log = text.Length > 6000 ? text.Substring(0, 6000) : text;
    }

    // ------------------------------------------------------------------ サーバーの起動・接続
    /// <summary>Audio Studio MCP を、HTTPのサーバーとして起動する(ウィンドウは出さない)</summary>
    async Task StartServerAsync()
    {
        if (await TryConnectAsync()) { Status = "すでに起動しています。つなぎました"; return; }
        var exe = ExePath;
        if (string.IsNullOrWhiteSpace(exe) || !File.Exists(exe))
        {
            Status = "⚠ 設定の「YMM4連携」→「AI音声生成」で、audio-studio-mcp.exe の場所を指定してください";
            return;
        }
        try
        {
            var psi = new ProcessStartInfo(exe, (WithWebUi ? $"--web {Port} --no-browser" : $"--http {Port}") + " --preload")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(exe) ?? "",
            };
            if (!string.IsNullOrWhiteSpace(OutputDir)) psi.Environment["AUDIO_STUDIO_OUTPUT_DIR"] = OutputDir;
            server = Process.Start(psi);
            if (server == null) { Status = "⚠ 起動できませんでした"; return; }
            HookExit();
            server.OutputDataReceived += (_, e) => { if (e.Data != null) App(() => AddLog(e.Data)); };
            server.ErrorDataReceived += (_, e) => { if (e.Data != null) App(() => AddLog(e.Data)); };
            server.BeginOutputReadLine();
            server.BeginErrorReadLine();
            Raise(nameof(ServerRunning));
            AddLog($"起動: {exe} {psi.Arguments}");
        }
        catch (Exception ex) { Status = "⚠ 起動できませんでした: " + ex.Message; return; }

        // モデルの読み込みに時間がかかるので、つながるまで待つ(最大 3 分)
        IsBusy = true;
        startedAt = DateTime.Now;
        clock.Start();
        Status = "起動中…(モデルを読み込んでいます。初回は数分かかることがあります)";
        try
        {
            for (int i = 0; i < 90; i++)
            {
                if (server == null || server.HasExited) { Status = "⚠ サーバーが終了しました。下の記録を確認してください"; return; }
                if (await TryConnectAsync()) return;
                await Task.Delay(2000);
            }
            Status = "⚠ 3分待ってもつながりませんでした。下の記録を確認してください";
        }
        finally { IsBusy = false; clock.Stop(); Raise(nameof(ServerRunning)); }
    }

    static void App(Action a) => System.Windows.Application.Current?.Dispatcher.BeginInvoke(a);

    void StopServer()
    {
        try { if (server is { HasExited: false }) server.Kill(true); } catch { }
        server = null;
        IsConnected = false;
        Status = "サーバーを止めました";
        Raise(nameof(ServerRunning));
    }

    /// <summary>画面を閉じる時。サーバーは止めない(ほかのウィンドウや、作り直された画面でも使うため。YMM4の終了時に止まる)</summary>
    public void Dispose()
    {
        running?.Cancel();
        clock.Stop();
        client?.Dispose();
    }

    async Task<bool> TryConnectAsync()
    {
        try
        {
            client?.Dispose();
            client = new McpHttpClient(Endpoint);
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
            await client.ConnectAsync(cts.Token);
            var tools = await client.ListToolsAsync(cts.Token);
            var keep = tool?.Name;
            Tools.Clear();
            foreach (var t in tools) if (t != null) Tools.Add(new McpTool(t));
            Tool = Tools.FirstOrDefault(t => t.Name == keep) ?? Tools.FirstOrDefault(t => t.Name == "generate_audio") ?? Tools.FirstOrDefault();
            IsConnected = true;
            Status = $"つながりました({(client.ServerName.Length > 0 ? client.ServerName : "MCPサーバー")} / ツール {Tools.Count} 個)";
            return true;
        }
        catch
        {
            IsConnected = false;
            return false;
        }
    }

    async Task ConnectAsync(bool quiet = false)
    {
        if (isBusy) return;
        IsBusy = true;
        try
        {
            if (!await TryConnectAsync() && !quiet)
                Status = $"⚠ {Endpoint} につながりませんでした。「起動する」を押すか、Audio Studio MCP を「--http {Port}」か「--web {Port}」で起動してください";
            else if (!isConnected) Status = "まだつないでいません(「起動する」を押すと、サーバーを起動してつなぎます)";
        }
        finally { IsBusy = false; }
    }

    // ------------------------------------------------------------------ ツールを実行して、できた音を取り込む
    async Task RunAsync()
    {
        var t = tool;
        if (client == null || t == null) return;
        var args = new JsonObject();
        foreach (var f in t.Fields)
        {
            if (!f.TryValue(out var v, out var error)) { Status = "⚠ " + error; return; }
            if (v != null) args[f.Name] = v;
        }

        IsBusy = true;
        running = new CancellationTokenSource();
        startedAt = DateTime.Now;
        gpuHistory.Clear();
        GeneratingTitle = IsSimple ? (kind switch { "sfx" => "効果音を作っています", "instrument" => "楽器の音を作っています", _ => "BGMを作っています" }) : $"「{t.Label}」を実行しています";
        var promptText = PromptField?.Text.Trim() ?? "";
        GeneratingDetail = (promptText.Length > 0 ? $"「{(promptText.Length > 80 ? promptText.Substring(0, 80) + "…" : promptText)}」" : "")
                           + (IsSimple ? $"  /  長さ {DurationText}  /  候補 {variations:0} 個" : "");
        IsGenerating = true;
        Interlocked.Increment(ref generatingCount);
        clock.Start();
        Tick();
        Status = $"「{t.Label}」を実行中…(生成には、しばらくかかります)";
        ResultText = "";
        var before = Snapshot();
        ApplySimpleForm();
        try
        {
            var result = await client.CallToolAsync(t.Name, args, running.Token);
            var text = string.Join("\n", (result?["content"] as JsonArray)?.Select(c => c?["text"]?.ToString()).Where(x => !string.IsNullOrEmpty(x)) ?? Array.Empty<string?>());
            if (result?["structuredContent"] is JsonNode sc) text += (text.Length > 0 ? "\n" : "") + sc.ToJsonString();
            bool isError = result?["isError"]?.GetValue<bool>() == true;
            ResultText = text.Length > 4000 ? text.Substring(0, 4000) + "…" : text;
            if (isError) { Status = "⚠ サーバーがエラーを返しました(下の結果を確認してください)"; return; }

            // できたファイル: 結果の文字の中の場所と、出力フォルダーに増えたファイルの両方から探す
            var files = PathsIn(text).Concat(NewFiles(before)).Distinct(StringComparer.OrdinalIgnoreCase).Where(File.Exists).ToList();
            // 結果に書かれていなかった時の最後の手: 出力フォルダーで、この実行のあいだに作られた音声ファイル
            if (files.Count == 0)
                foreach (var dir in OutputDirs())
                {
                    try
                    {
                        files.AddRange(Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
                            .Where(f => AudioExt.Contains(Path.GetExtension(f).ToLowerInvariant()) && File.GetLastWriteTime(f) >= startedAt.AddSeconds(-5)));
                    }
                    catch { }
                }
            // 候補を複数作った時に、結果に1つしか書かれていないことがあるので、同じフォルダーでこの実行中にできた音も足す
            if (files.Count > 0 && files.Count < (IsSimple && HasVariations ? (int)variations : 1))
                foreach (var dir in files.Select(Path.GetDirectoryName).Where(d => !string.IsNullOrEmpty(d)).Distinct(StringComparer.OrdinalIgnoreCase).ToList())
                {
                    try
                    {
                        files.AddRange(Directory.EnumerateFiles(dir!, "*", SearchOption.TopDirectoryOnly)
                            .Where(f => AudioExt.Contains(Path.GetExtension(f).ToLowerInvariant()) && File.GetLastWriteTime(f) >= startedAt.AddSeconds(-5)));
                    }
                    catch { }
                }
            files = files.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(f => File.GetLastWriteTime(f)).ToList();
            var secs = (int)(DateTime.Now - startedAt).TotalSeconds;
            if (files.Count == 0)
            {
                var dirs = string.Join(" / ", OutputDirs());
                Status = $"終わりました({secs} 秒)。できたファイルは見つかりませんでした(探した場所: {(dirs.Length > 0 ? dirs : "なし。設定で出力フォルダーを指定してください")})";
                return;
            }

            var prompt = t.Fields.FirstOrDefault(f => f.IsLongText)?.Text.Trim() ?? "";
            var tags = (ImportTags ?? "").Split(new[] { ',', '、', ' ', '　' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var kind = t.Fields.FirstOrDefault(f => f.Name == "audio_type")?.Choice;
            if (!string.IsNullOrEmpty(kind)) tags.Add(kind switch { "music" => "BGM", "sfx" => "効果音", "instrument" => "楽器", _ => kind! });
            var memo = (prompt.Length > 0 ? "指示: " + prompt + "\n" : "") + $"Audio Studio MCP / {t.Name} / {DateTime.Now:yyyy-MM-dd HH:mm}";
            if (AutoImport)
            {
                // 選ぶ画面を出さずに、すべて取り込む
                await library.ImportGeneratedAsync(files, tags, memo);
                Status = $"終わりました({secs} 秒)。{files.Count} 個の音を、素材データベースに取り込みました";
                return;
            }
            // 聴き比べて、取り込むものを選ぶ画面
            ShowPick(files, tags, memo);
            Status = $"終わりました({secs} 秒)。{files.Count} 個できました。聴いて、取り込むものを選んでください";
        }
        catch (OperationCanceledException) { Status = "取りやめました(サーバー側の処理は、終わるまで続く場合があります)"; }
        catch (Exception ex) { Status = "⚠ 実行できませんでした: " + ex.GetBaseException().Message; IsConnected = client.IsConnected; }
        finally
        {
            IsBusy = false;
            if (isGenerating) Interlocked.Decrement(ref generatingCount);
            IsGenerating = false;
            clock.Stop();
            Raise(nameof(ElapsedText));
        }
    }

    /// <summary>
    /// 出力フォルダーの候補。設定で決めていなければ、Audio Studio MCP の標準(リポジトリ直下の outputs)を、
    /// audio-studio-mcp.exe の場所(リポジトリ\.venv\Scripts)からたどって探す
    /// </summary>
    IEnumerable<string> OutputDirs()
    {
        var list = new List<string>();
        if (!string.IsNullOrWhiteSpace(OutputDir)) list.Add(OutputDir);
        try
        {
            var exe = ExePath;
            if (!string.IsNullOrWhiteSpace(exe))
            {
                var dir = Path.GetDirectoryName(exe);
                for (int i = 0; i < 4 && !string.IsNullOrEmpty(dir); i++)
                {
                    var cand = Path.Combine(dir, "outputs");
                    if (Directory.Exists(cand)) list.Add(cand);
                    dir = Path.GetDirectoryName(dir);
                }
            }
        }
        catch { }
        return list.Distinct(StringComparer.OrdinalIgnoreCase).Where(Directory.Exists);
    }

    /// <summary>
    /// 結果の中から、音声ファイルの場所を探す。「C:\…\a.wav」のような完全な場所のほか、「outputs/a.wav」「a.wav」のような
    /// 途中までの場所や名前だけの時も、出力フォルダーの中から探して完全な場所にする。JSON の中の文字(\\ や \u…)も読む
    /// </summary>
    IEnumerable<string> PathsIn(string text)
    {
        var found = new List<string>();
        var texts = new List<string> { text };
        // JSON として読めるところは、文字を取り出しておく(\\ が \ に戻るので、場所が正しく読める)
        foreach (Match m in Regex.Matches(text, @"\{[\s\S]*\}|\[[\s\S]*\]"))
        {
            try { CollectStrings(JsonNode.Parse(m.Value), texts); } catch { }
        }
        foreach (var t in texts)
            foreach (Match m in Regex.Matches(t, @"[^\s""'<>|*?:,\[\]{}()]*(?:[A-Za-z]:[\\/])?[^\s""'<>|*?,\[\]{}()]+?\.(?:wav|ogg|flac|mp3)\b", RegexOptions.IgnoreCase))
            {
                var p = m.Value.Trim().Replace('/', Path.DirectorySeparatorChar);
                if (Path.IsPathRooted(p) && File.Exists(p)) { found.Add(p); continue; }
                foreach (var dir in OutputDirs())
                {
                    var a = Path.Combine(dir, p);
                    if (File.Exists(a)) { found.Add(a); break; }
                    var b = Path.Combine(Path.GetDirectoryName(dir) ?? dir, p);     // 「outputs/…」のように書かれている時
                    if (File.Exists(b)) { found.Add(b); break; }
                    var hit = SafeFind(dir, Path.GetFileName(p));
                    if (hit != null) { found.Add(hit); break; }
                }
            }
        return found;
    }

    static void CollectStrings(JsonNode? node, List<string> into)
    {
        switch (node)
        {
            case JsonObject o: foreach (var kv in o) CollectStrings(kv.Value, into); break;
            case JsonArray a: foreach (var x in a) CollectStrings(x, into); break;
            case JsonValue v when v.TryGetValue<string>(out var s): into.Add(s); break;
        }
    }

    static string? SafeFind(string dir, string name)
    {
        try { return Directory.EnumerateFiles(dir, name, SearchOption.AllDirectories).FirstOrDefault(); }
        catch { return null; }
    }

    Dictionary<string, DateTime> Snapshot()
    {
        var map = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in OutputDirs())
        {
            try
            {
                foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                    if (AudioExt.Contains(Path.GetExtension(f).ToLowerInvariant())) map[f] = File.GetLastWriteTimeUtc(f);
            }
            catch { }
        }
        return map;
    }

    IEnumerable<string> NewFiles(Dictionary<string, DateTime> before)
    {
        foreach (var kv in Snapshot())
            if (!before.TryGetValue(kv.Key, out var old) || old != kv.Value) yield return kv.Key;
    }
}
