using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows.Input;
using System.Windows.Threading;

namespace YmmExplorer;

/// <summary>
/// 素材データベースの「ローカルAIで作る」画面。
/// Local AI Studio(このPCの中で動くAI)で画像・音楽(Audio Studio MCP も)を作り、そのまま素材として取り込む。
/// 各モデルの性能・重さの目安も出す。
/// </summary>
public sealed class LocalStudioGenVm : NotifyBase
{
    readonly MaterialLibraryVm library;
    readonly DispatcherTimer clock = new() { Interval = TimeSpan.FromSeconds(1) };
    List<LocalStudioModel> all = new();
    bool isOpen, isBusy, isImage = true, loading;
    string status = "", log = "", prompt = "", negative = "", seed = "", bpm = "", hardware = "";
    int steps = 20, duration = 8, variations = 1, sizeIndex, audioTypeIndex;
    LocalStudioModel? model;
    DateTime startedAt;
    CancellationTokenSource? cts;

    public LocalStudioGenVm(MaterialLibraryVm library)
    {
        this.library = library;
        clock.Tick += (_, _) => Raise(nameof(ElapsedText));
        GenerateCommand = new RelayCommand(async _ => await GenerateAsync(), _ => !isBusy && model is { Installed: true } && prompt.Trim().Length > 0);
        CancelCommand = new RelayCommand(_ => cts?.Cancel(), _ => isBusy);
        RefreshCommand = new RelayCommand(async _ => await LoadAsync(), _ => !loading);
        CloseCommand = new RelayCommand(_ => IsOpen = false);
        OpenAppCommand = new RelayCommand(async _ =>
        {
            try
            {
                if (!await LocalAiStudio.OpenAppAsync()) { Status = "Local AI Studio は起動中です(タスクバーから開けます)"; await LoadAsync(); return; }
                Status = "Local AI Studio を起動しています…";
                // 起動が終わったら、モデルの一覧を読み直す(最大 60 秒待つ)
                for (int i = 0; i < 60 && !await LocalAiStudio.IsRunningAsync(); i++) await Task.Delay(1000);
                await LoadAsync();
            }
            catch (Exception ex) { Status = ex.Message; }
        });
        UninstallCommand = new RelayCommand(async _ => await UninstallAsync(), _ => !isBusy && model is { Installed: true, IsAudioStudio: false });
        OpenOutputCommand = new RelayCommand(_ =>
        {
            var dir = Path.Combine(LocalAiStudio.DataDir, "outputs");
            try { if (Directory.Exists(dir)) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(dir) { UseShellExecute = true }); } catch { }
        });
    }

    public ICommand GenerateCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand OpenAppCommand { get; }
    public ICommand UninstallCommand { get; }

    /// <summary>選んでいるモデルを、Local AI Studio からアンインストールする(このPCからファイルを消す)</summary>
    async Task UninstallAsync()
    {
        if (model is not { } m) return;
        if (System.Windows.MessageBox.Show($"{m.Name} をアンインストールしますか？\nこのPCから、モデルのファイル({m.Size})を削除します。もう一度使う時は、インストールし直します。",
                "Local AI Studio", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes) return;
        IsBusy = true;
        startedAt = DateTime.Now;
        Log = "";
        Status = $"{m.Name} をアンインストールしています…";
        try
        {
            await LocalAiStudio.UninstallAsync(m.Id, new Progress<StudioProgress>(p => Log = p.Text), CancellationToken.None);
            Status = $"{m.Name} をアンインストールしました";
        }
        catch (Exception ex) { Status = "⚠ " + ex.Message; }
        finally { IsBusy = false; Log = ""; }
        await LoadAsync();
    }
    public ICommand OpenOutputCommand { get; }

    // ------------------------------------------------------------------ 参考ファイル(この画像をもとに・このメロディーのように)
    string referenceImage = "", referenceMelody = "";
    double strength = 0.6;
    static readonly string[] ImageExts = { ".png", ".jpg", ".jpeg", ".webp", ".bmp" };
    static readonly string[] AudioExts = { ".wav", ".mp3", ".flac", ".ogg", ".m4a", ".aac", ".wma", ".opus" };

    /// <summary>いま選んでいる参考ファイル(画像のときは参考画像、音楽のときは参考のメロディー)</summary>
    public string Reference => isImage ? referenceImage : referenceMelody;
    public string ReferenceName => Reference.Length > 0 ? Path.GetFileName(Reference) : "選んでいません";
    public bool HasReference => Reference.Length > 0;
    public string ReferenceTitle => isImage ? "参考画像(省略可)― この画像をもとに作ります" : "参考のメロディー(省略可)― この曲のように作ります";
    /// <summary>選んだモデルで参考ファイルを使えるか(使えない時の説明)</summary>
    public string ReferenceNote
    {
        get
        {
            if (model == null) return "";
            if (isImage) return model.Accepts.Contains("image") ? "「変える強さ」が小さいほど、参考画像に近くなります" : "このモデルは参考画像を使えません";
            if (model.Accepts.Contains("melody")) return "WAV が確実です(MP3 なども、ffmpeg が入っていれば自動で変換します)";
            if (model.Accepts.Contains("melody?")) return "Audio Studio MCP が受け付ける時だけ使います";
            return "メロディーを参考にするなら「MusicGen Melody」を選んでください(Local AI Studio でインストール)";
        }
    }
    /// <summary>参考画像から変える強さ(0.05〜1.0。小さいほど参考画像に近い)</summary>
    public double Strength { get => strength; set { if (Set(ref strength, Math.Round(Math.Clamp(value, 0.05, 1.0), 2))) Raise(nameof(StrengthText)); } }
    public string StrengthText => $"{strength:0.00}";

    void SetReference(string path)
    {
        if (isImage) referenceImage = path; else referenceMelody = path;
        foreach (var n in new[] { nameof(Reference), nameof(ReferenceName), nameof(HasReference) }) Raise(n);
    }

    ICommand? pickReferenceCommand, clearReferenceCommand, useSelectedCommand;
    public ICommand PickReferenceCommand => pickReferenceCommand ??= new RelayCommand(_ =>
    {
        var exts = isImage ? ImageExts : AudioExts;
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = isImage ? "参考にする画像を選んでください" : "参考にするメロディー(音声ファイル)を選んでください",
            Filter = (isImage ? "画像" : "音声") + "|" + string.Join(";", exts.Select(e => "*" + e)) + "|すべて|*.*",
        };
        if (dlg.ShowDialog() == true) SetReference(dlg.FileName);
    });
    public ICommand ClearReferenceCommand => clearReferenceCommand ??= new RelayCommand(_ => SetReference(""));
    /// <summary>素材データベースで選んでいる素材を、参考にする</summary>
    public ICommand UseSelectedCommand => useSelectedCommand ??= new RelayCommand(_ =>
    {
        var exts = isImage ? ImageExts : AudioExts;
        var hit = library.SelectedPaths().FirstOrDefault(p => exts.Contains(Path.GetExtension(p).ToLowerInvariant()));
        if (hit == null) { Status = isImage ? "素材データベースで、参考にする画像を選んでから押してください" : "素材データベースで、参考にする音声を選んでから押してください"; return; }
        SetReference(hit);
        Status = $"「{Path.GetFileName(hit)}」を参考にします";
    });

    public bool IsOpen
    {
        get => isOpen;
        set { if (Set(ref isOpen, value) && value && all.Count == 0) _ = LoadAsync(); }
    }

    public bool IsBusy { get => isBusy; private set { if (Set(ref isBusy, value)) { Raise(nameof(IsIdle)); CommandManager.InvalidateRequerySuggested(); } } }
    public bool IsIdle => !isBusy;
    public string Status { get => status; private set => Set(ref status, value); }
    public string Log { get => log; private set => Set(ref log, value); }
    public string Hardware { get => hardware; private set => Set(ref hardware, value); }
    // 進み具合(GPU・残り時間)が届いている間は、その中に経過時間も入っているので、こちらは出さない
    public string ElapsedText => isBusy && log.Length == 0 ? $"{(int)(DateTime.Now - startedAt).TotalMinutes:00}:{(DateTime.Now - startedAt).Seconds:00}" : "";
    double progressValue;
    bool progressUnknown = true;
    /// <summary>進み具合のバー(0〜1)</summary>
    public double ProgressValue { get => progressValue; private set => Set(ref progressValue, value); }
    /// <summary>進み具合が分からない(動き続けるバーにする)</summary>
    public bool ProgressUnknown { get => progressUnknown; private set => Set(ref progressUnknown, value); }

    // ------------------------------------------------------------------ 画像か音楽か
    public bool IsImage { get => isImage; set { if (value && Set(ref isImage, true)) ModeChanged(); } }
    public bool IsMusic { get => !isImage; set { if (value && isImage) { isImage = false; ModeChanged(); } } }

    void ModeChanged()
    {
        Raise(nameof(IsImage)); Raise(nameof(IsMusic));
        FillModels();
        foreach (var n in new[] { nameof(PromptHint), nameof(Reference), nameof(ReferenceName), nameof(HasReference), nameof(ReferenceTitle), nameof(ReferenceNote) }) Raise(n);
    }

    public ObservableCollection<LocalStudioModel> Models { get; } = new();

    public LocalStudioModel? Model
    {
        get => model;
        set
        {
            if (!Set(ref model, value)) return;
            if (value != null)
            {
                if (value.Type == "image")
                {
                    int width = value.DefaultInt("width", 512), height = value.DefaultInt("height", 512);
                    var idx = Array.IndexOf(SizeKeys, $"{width}x{height}");
                    SizeIndex = idx >= 0 ? idx : 0;
                    Steps = value.DefaultInt("steps", 20);
                }
                else
                {
                    Duration = value.DefaultInt("duration", 8);
                }
            }
            foreach (var n in new[] { nameof(ModelNote), nameof(ModelDescription), nameof(IsAudioStudio), nameof(DurationMin), nameof(DurationMax), nameof(ModelState), nameof(ReferenceNote) }) Raise(n);
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public string ModelNote => model?.RatingText ?? "";
    public string ModelDescription => model?.Description ?? "";
    public string ModelState => model == null ? "" : model.Installed ? "✓ 使えます" : "未インストール: Local AI Studio の「モデルライブラリ」でインストールしてください" + (model.Detail.Length > 0 ? $"({model.Detail})" : "");
    public bool IsAudioStudio => model?.IsAudioStudio == true;

    public string Prompt { get => prompt; set { if (Set(ref prompt, value ?? "")) CommandManager.InvalidateRequerySuggested(); } }
    public string PromptHint => isImage
        ? "作りたい画像を、英語で書くと伝わりやすいです(例: a cozy room with a window, anime style, soft light)"
        : "ジャンル・楽器・雰囲気を英語で(例: calm lo-fi hip hop with soft piano and rain)";
    public string NegativePrompt { get => negative; set => Set(ref negative, value ?? ""); }

    // 画像の設定
    static readonly string[] SizeKeys = { "512x512", "768x768", "1024x1024", "768x512", "512x768", "1024x576", "576x1024" };
    public IReadOnlyList<string> Sizes { get; } = new[] { "512 × 512(正方形)", "768 × 768", "1024 × 1024", "768 × 512(横長)", "512 × 768(縦長)", "1024 × 576(16:9)", "576 × 1024(9:16)" };
    public int SizeIndex { get => sizeIndex; set => Set(ref sizeIndex, Math.Clamp(value, 0, SizeKeys.Length - 1)); }
    public int Steps { get => steps; set => Set(ref steps, Math.Clamp(value, 1, 50)); }

    // 音楽の設定
    public int DurationMin => model?.DefaultInt("min_duration", 1) ?? 1;
    public int DurationMax => model?.DefaultInt("max_duration", 30) ?? 30;
    public int Duration { get => duration; set => Set(ref duration, Math.Clamp(value, DurationMin, Math.Max(DurationMin, DurationMax))); }
    public IReadOnlyList<string> AudioTypes { get; } = new[] { "BGM(曲)", "楽器ソロ・ループ", "効果音" };
    static readonly string[] AudioTypeKeys = { "music", "instrument", "sfx" };
    public int AudioTypeIndex { get => audioTypeIndex; set => Set(ref audioTypeIndex, Math.Clamp(value, 0, 2)); }
    public int Variations { get => variations; set => Set(ref variations, Math.Clamp(value, 1, 4)); }
    public string Bpm { get => bpm; set => Set(ref bpm, value ?? ""); }
    public string Seed { get => seed; set => Set(ref seed, value ?? ""); }

    // ------------------------------------------------------------------ 読み込み
    async Task LoadAsync()
    {
        if (loading) return;
        loading = true;
        Status = "Local AI Studio につないでいます…";
        try
        {
            all = await LocalAiStudio.ModelsAsync(null, CancellationToken.None);
            Hardware = "このPC: " + await LocalAiStudio.HardwareAsync(CancellationToken.None);
            FillModels();
            var ready = all.Count(m => m.Type != "chat" && m.Installed);
            Status = ready > 0 ? $"つながりました。使える画像・音楽のモデル: {ready} 個" : "つながりました。画像・音楽のモデルがまだありません。「Local AI Studio を開く」→「モデルライブラリ」でインストールしてください";
        }
        catch (LocalStudioNotRunningException ex) { Status = ex.Message; }
        catch (Exception ex) { Status = "⚠ " + ex.Message; }
        finally { loading = false; CommandManager.InvalidateRequerySuggested(); }
    }

    void FillModels()
    {
        var keep = model?.Id;
        Models.Clear();
        foreach (var m in all.Where(x => x.Type == (isImage ? "image" : "music")).OrderByDescending(x => x.Installed))
            Models.Add(m);
        Model = Models.FirstOrDefault(m => m.Id == keep) ?? Models.FirstOrDefault(m => m.Installed) ?? Models.FirstOrDefault();
    }

    // ------------------------------------------------------------------ 生成
    async Task GenerateAsync()
    {
        var m = model;
        if (m == null || isBusy) return;
        var payload = new JsonObject { ["prompt"] = prompt.Trim() };
        if (seed.Trim().Length > 0)
        {
            if (!long.TryParse(seed.Trim(), out var sv) || sv < 0 || sv > uint.MaxValue) { Status = "シードは 0〜4294967295 の数字で入れてください"; return; }
            payload["seed"] = sv;
        }
        var tags = new List<string> { "AI生成" };
        if (m.Type == "image")
        {
            var parts = SizeKeys[sizeIndex].Split('x');
            payload["width"] = int.Parse(parts[0]);
            payload["height"] = int.Parse(parts[1]);
            payload["steps"] = steps;
            if (negative.Trim().Length > 0) payload["negative_prompt"] = negative.Trim();
            if (referenceImage.Length > 0)
            {
                if (!m.Accepts.Contains("image")) { Status = "このモデルは参考画像を使えません。参考画像を外してください(×)"; return; }
                if (!File.Exists(referenceImage)) { Status = "参考画像が見つかりません: " + referenceImage; return; }
                payload["init_image"] = referenceImage;
                payload["strength"] = strength;
                tags.Add("参考画像あり");
            }
            tags.Add("画像");
        }
        else
        {
            payload["duration"] = duration;
            if (referenceMelody.Length > 0)
            {
                if (!m.Accepts.Any(a => a.StartsWith("melody"))) { Status = "このモデルは参考のメロディーを使えません。「MusicGen Melody」を選ぶか、参考のメロディーを外してください(×)"; return; }
                if (!File.Exists(referenceMelody)) { Status = "参考のメロディーが見つかりません: " + referenceMelody; return; }
                // 24bit・浮動小数などの WAV は、読みやすい 16bit にしてから渡す
                payload["melody"] = AudioCompat.PlayablePath(referenceMelody);
            }
            if (m.IsAudioStudio)
            {
                payload["audio_type"] = AudioTypeKeys[audioTypeIndex];
                payload["variations"] = variations;
                if (bpm.Trim().Length > 0)
                {
                    if (!int.TryParse(bpm.Trim(), out var b) || b < 40 || b > 240) { Status = "テンポ(BPM)は 40〜240 の数字で入れてください"; return; }
                    payload["bpm"] = b;
                }
                tags.Add(AudioTypeKeys[audioTypeIndex] switch { "sfx" => "効果音", "instrument" => "楽器", _ => "BGM" });
            }
            else tags.Add("BGM");
        }

        IsBusy = true;
        startedAt = DateTime.Now;
        clock.Start();
        cts = new CancellationTokenSource();
        Log = "";
        Status = $"{m.Name} で作っています…(初回はモデルの読み込みに時間がかかります)";
        try
        {
            ProgressValue = 0; ProgressUnknown = true;
            var progress = new Progress<StudioProgress>(p =>
            {
                Log = p.Text;
                ProgressUnknown = p.Fraction is null;
                ProgressValue = p.Fraction ?? 0;
            });
            var files = await LocalAiStudio.GenerateAsync(m.Id, payload, progress, cts.Token);
            files = files.Where(File.Exists).ToList();
            if (files.Count == 0) { Status = "できたファイルが見つかりませんでした"; return; }
            var memo = $"指示: {prompt.Trim()}\nLocal AI Studio / {m.Name} / {DateTime.Now:yyyy-MM-dd HH:mm}";
            await library.ImportGeneratedAsync(files, tags, memo);
            Status = $"できました({(int)(DateTime.Now - startedAt).TotalSeconds} 秒)。{files.Count} 個を素材データベースに取り込みました";
        }
        catch (OperationCanceledException) { Status = "待つのをやめました(Local AI Studio 側の処理は、終わるまで続きます。できたファイルは「生成したファイル」から取り込めます)"; }
        catch (Exception ex) { Status = "⚠ " + ex.Message; }
        finally
        {
            IsBusy = false;
            clock.Stop();
            Raise(nameof(ElapsedText));
        }
    }
}
