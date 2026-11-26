using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer;

/// <summary>エフェクトタブに並ぶ、同梱の演出エフェクト1つ分(読み取り専用)</summary>
public sealed class FxInfoItem : NotifyBase
{
    public Type Type { get; }
    public bool IsVideo { get; }
    public string Name { get; }
    public string Category { get; }
    public string Description { get; }
    public string Tags { get; }
    public int No { get; }
    public string ThumbFile { get; }
    public string NoText => (IsVideo ? "V" : "A") + No;
    public string Title => Name;

    WeakReference<BitmapSource>? cache;
    bool loading;

    public FxInfoItem(Type type, bool video, string name, string category, string description, string tags, int no, string thumbFile)
    {
        Type = type; IsVideo = video; Name = name; Category = category; Description = description; Tags = tags; No = no; ThumbFile = thumbFile;
    }

    /// <summary>画面に出た時だけ、保存してあるサムネイルを読み込む</summary>
    public ImageSource? Thumb
    {
        get
        {
            if (!IsVideo) return null;
            if (cache != null && cache.TryGetTarget(out var bmp)) return bmp;
            if (!loading) _ = LoadAsync();
            return null;
        }
    }

    async Task LoadAsync()
    {
        loading = true;
        var path = ThumbFile;
        var bmp = await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(path)) return null;
                using var ms = new MemoryStream(File.ReadAllBytes(path));
                var b = new BitmapImage();
                b.BeginInit(); b.CacheOption = BitmapCacheOption.OnLoad; b.StreamSource = ms; b.EndInit(); b.Freeze();
                return (BitmapSource)b;
            }
            catch { return null; }
        });
        loading = false;
        if (bmp == null) return;
        cache = new WeakReference<BitmapSource>(bmp);
        Raise(nameof(Thumb));
    }

    internal void ThumbReady() { cache = null; Raise(nameof(Thumb)); }

    bool isSelected;
    public bool IsSelected { get => isSelected; set => Set(ref isSelected, value); }
}

/// <summary>
/// 同梱の演出エフェクト(映像・音声)の一覧と、映像エフェクトのサムネイル。
/// サムネイルは、設定で決めたサンプル画像にエフェクトをかけて初回に作り、プラグインフォルダーの FxThumbs に保存する。
/// サンプル画像を変えると、全部作り直す。YMM4 標準のエフェクトや、ほかのプラグインのエフェクトは並べない
/// </summary>
public static class FxCatalog
{
    /// <summary>サムネイルの作り方を変えた時に上げる</summary>
    const string Revision = "1";

    public static string Folder => Path.Combine(Path.GetDirectoryName(typeof(FxCatalog).Assembly.Location) ?? AppContext.BaseDirectory, "FxThumbs");

    public static IReadOnlyList<FxInfoItem> Video { get; private set; } = Array.Empty<FxInfoItem>();
    public static IReadOnlyList<FxInfoItem> Audio { get; private set; } = Array.Empty<FxInfoItem>();

    public static string Status { get; private set; } = "";
    public static event Action? ItemsChanged;
    public static event Action? StatusChanged;

    static Task? run;
    static bool rerun;
    /// <summary>サムネイルを作る時に使うサンプル画像(空欄なら同梱の絵)</summary>
    static string samplePath = "";

    static void SetStatus(string s) { Status = s; StatusChanged?.Invoke(); }

    /// <summary>UIスレッドから呼ぶ。何度呼んでも1回だけ動く</summary>
    public static Task EnsureAsync(string sample)
    {
        samplePath = sample ?? "";
        return run ??= RunAsync();
    }

    /// <summary>サンプル画像を変えた時: サムネイルを作り直す</summary>
    public static void SetSample(string sample)
    {
        samplePath = sample ?? "";
        if (run == null || run.IsCompleted) run = RunAsync();
        else rerun = true;
    }

    static async Task RunAsync()
    {
        try
        {
            if (Video.Count == 0 && Audio.Count == 0)
            {
                Video = Scan(true);
                Audio = Scan(false);
                ItemsChanged?.Invoke();
            }
            do
            {
                rerun = false;
                await BuildThumbsAsync();
            } while (rerun);
        }
        catch (Exception ex) { SetStatus("⚠ サムネイルを作れませんでした: " + ex.GetBaseException().Message); }
    }

    /// <summary>このプラグインの中にある演出エフェクト(印 …EffectAttribute の付いたもの)を探す</summary>
    static List<FxInfoItem> Scan(bool video)
    {
        var want = video ? typeof(IVideoEffect) : typeof(IAudioEffect);
        var list = new List<FxInfoItem>();
        var dir = Folder;
        int seq = 0;
        foreach (var t in BuiltinCatalog.TypesOf(typeof(FxCatalog).Assembly))
        {
            if (t.IsAbstract || !want.IsAssignableFrom(t) || t.GetConstructor(Type.EmptyTypes) == null) continue;
            CustomAttributeData? attr = null;
            try { attr = t.GetCustomAttributesData().FirstOrDefault(a => a.AttributeType.Name == (video ? "VideoEffectAttribute" : "AudioEffectAttribute")); } catch { }
            if (attr == null || attr.ConstructorArguments.Count == 0) continue;
            var name = attr.ConstructorArguments[0].Value as string ?? t.Name;
            string Arr(int i) => attr.ConstructorArguments.Count > i && attr.ConstructorArguments[i].Value is IEnumerable<CustomAttributeTypedArgument> v
                ? string.Join(" ", v.Select(x => x.Value as string ?? "")) : "";
            var category = Arr(1).Replace("演出：", "").Trim();
            if (category.Length == 0) category = "その他";
            var tags = Arr(2);
            var info = video ? FxInfo.Video : FxInfo.Audio;
            int no = ++seq + 100000;
            string desc = "";
            if (info.TryGetValue(name, out var d)) { no = d.No; desc = d.Desc; }
            if (desc.Length == 0)
            {
                // 説明の無いものは、タグの2つ目(短い説明が入っていることが多い)を使う
                var parts = (attr.ConstructorArguments.Count > 2 && attr.ConstructorArguments[2].Value is IEnumerable<CustomAttributeTypedArgument> tv ? tv.Select(x => x.Value as string ?? "").ToList() : new List<string>());
                desc = parts.Skip(1).FirstOrDefault(x => x.Length >= 6 && x != name) ?? "";
            }
            list.Add(new FxInfoItem(t, video, name, category, desc, tags, no, Path.Combine(dir, t.Name + ".jpg")));
        }
        return list.OrderBy(x => x.No).ThenBy(x => x.Name, StringComparer.Ordinal).ToList();
    }

    static string SampleKey()
    {
        if (samplePath.Length == 0 || !File.Exists(samplePath)) return "default";
        try { return samplePath + "|" + File.GetLastWriteTimeUtc(samplePath).Ticks + "|" + new FileInfo(samplePath).Length; }
        catch { return samplePath; }
    }

    static async Task BuildThumbsAsync()
    {
        var dir = Folder;
        Directory.CreateDirectory(dir);
        var revFile = Path.Combine(dir, "revision.txt");
        var key = Revision + "|" + SampleKey();
        bool same = File.Exists(revFile) && File.ReadAllText(revFile).Trim() == key;
        var todo = Video.Where(i => !same || !File.Exists(i.ThumbFile)).ToList();
        if (todo.Count > 0)
        {
            var sample = LoadSample();
            using var session = ShapeThumbRenderer.BeginSession();
            int done = 0;
            foreach (var it in todo)
            {
                if (rerun) return;      // 途中でサンプル画像が変わった: 最初から作り直す
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                if (done++ % 5 == 0) SetStatus($"サンプル画像にエフェクトをかけて、サムネイルを作っています… {done} / {todo.Count}");
                BitmapSource? img = null;
                try { img = ShapeThumbRenderer.RenderVideoEffect(it.Type, sample); } catch { }
                SaveJpeg(img ?? NameThumb.Render(it.Name, "映像エフェクト"), it.ThumbFile);
                it.ThumbReady();
            }
        }
        File.WriteAllText(revFile, key);
        var keep = new HashSet<string>(Video.Select(i => i.ThumbFile), StringComparer.OrdinalIgnoreCase);
        foreach (var f in Directory.GetFiles(dir, "*.jpg"))
            if (!keep.Contains(f)) try { File.Delete(f); } catch { }
        SetStatus("");
    }

    static void SaveJpeg(BitmapSource picture, string file)
    {
        var enc = new JpegBitmapEncoder { QualityLevel = 88 };
        enc.Frames.Add(BitmapFrame.Create(picture));
        var tmp = file + ".tmp";
        using (var fs = File.Create(tmp)) enc.Save(fs);
        File.Move(tmp, file, true);
    }

    /// <summary>サムネイル用のサンプル画像(640×360 に収める)。読めない時は同梱の絵</summary>
    public static BitmapSource LoadSample()
    {
        try
        {
            if (samplePath.Length > 0 && File.Exists(samplePath))
            {
                var b = new BitmapImage();
                using (var ms = new MemoryStream(File.ReadAllBytes(samplePath)))
                {
                    b.BeginInit(); b.CacheOption = BitmapCacheOption.OnLoad; b.StreamSource = ms; b.EndInit(); b.Freeze();
                }
                return Fit(b);
            }
        }
        catch { }
        return DefaultSample();
    }

    /// <summary>640×360 の中に収める(縦横比はそのまま。はみ出す分は切る)</summary>
    static BitmapSource Fit(BitmapSource src)
    {
        const int w = 640, h = 360;
        double sc = Math.Max(w / (double)src.PixelWidth, h / (double)src.PixelHeight);
        double pw = src.PixelWidth * sc, ph = src.PixelHeight * sc;
        var dv = new DrawingVisual();
        RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.HighQuality);
        using (var dc = dv.RenderOpen()) dc.DrawImage(src, new Rect((w - pw) / 2, (h - ph) / 2, pw, ph));
        var rt = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
        rt.Render(dv);
        rt.Freeze();
        return rt;
    }

    /// <summary>同梱のサンプル画像(空・山・家・人と、色の帯。エフェクトの違いが分かりやすい絵)</summary>
    public static BitmapSource DefaultSample()
    {
        const int w = 640, h = 360;
        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            dc.DrawRectangle(new LinearGradientBrush(Color.FromRgb(0x4F, 0xA8, 0xF0), Color.FromRgb(0xC8, 0xE8, 0xFF), 90), null, new Rect(0, 0, w, h));
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0xFF, 0xD5, 0x4F)), null, new Point(520, 80), 42, 42);
            var mountain = new StreamGeometry();
            using (var g = mountain.Open())
            {
                g.BeginFigure(new Point(0, 250), true, true);
                g.LineTo(new Point(140, 120), true, false); g.LineTo(new Point(250, 210), true, false); g.LineTo(new Point(380, 100), true, false);
                g.LineTo(new Point(520, 220), true, false); g.LineTo(new Point(640, 160), true, false); g.LineTo(new Point(640, 360), true, false); g.LineTo(new Point(0, 360), true, false);
            }
            dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(0x5B, 0x8F, 0x5E)), null, mountain);
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x7C, 0xC5, 0x5F)), null, new Rect(0, 270, w, 90));
            // 家
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0xF5, 0xEB, 0xD8)), null, new Rect(90, 200, 120, 90));
            var roof = new StreamGeometry();
            using (var g = roof.Open()) { g.BeginFigure(new Point(78, 204), true, true); g.LineTo(new Point(150, 150), true, false); g.LineTo(new Point(222, 204), true, false); }
            dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(0xD8, 0x43, 0x15)), null, roof);
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x6D, 0x4C, 0x41)), null, new Rect(135, 240, 30, 50));
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x81, 0xD4, 0xFA)), null, new Rect(104, 215, 24, 20));
            // 人(丸い頭と体)
            dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0xFF, 0xE0, 0xC2)), null, new Point(380, 215), 26, 26);
            dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(0xE8, 0x48, 0x8A)), null, new Rect(350, 243, 60, 70), 18, 18);
            dc.DrawEllipse(Brushes.Black, null, new Point(371, 211), 3.5, 3.5); dc.DrawEllipse(Brushes.Black, null, new Point(389, 211), 3.5, 3.5);
            // 色の帯
            Color[] cols = [Color.FromRgb(0xE5, 0x39, 0x35), Color.FromRgb(0xFB, 0x8C, 0x00), Color.FromRgb(0xFD, 0xD8, 0x35), Color.FromRgb(0x43, 0xA0, 0x47), Color.FromRgb(0x1E, 0x88, 0xE5), Color.FromRgb(0x8E, 0x24, 0xAA)];
            for (int i = 0; i < cols.Length; i++) dc.DrawRectangle(new SolidColorBrush(cols[i]), null, new Rect(450 + i * 28, 300, 28, 36));
            var ft = new FormattedText("Sample", System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI Semibold"), 34, Brushes.White, 1.0);
            dc.DrawText(ft, new Point(24, 22));
        }
        var rt = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
        rt.Render(dv);
        rt.Freeze();
        return rt;
    }

    /// <summary>エフェクトを、YMM4のタイムラインで選択中のアイテムに付ける。戻り値は画面に出す文</summary>
    public static async Task<string> ApplyAsync(FxInfoItem it, bool append)
    {
        if (Ymm.Timeline() == null) return "⚠ " + Ymm.LastError;
        var targets = Ymm.SelectedItems();
        if (targets.Count == 0) return "⚠ YMM4のタイムラインで、エフェクトを付けたいアイテムを選んでください";
        var prop = Ymm.EffectsProperty(it.IsVideo);
        var kind = it.IsVideo ? "映像" : "音声";
        var bad = targets.FirstOrDefault(t => Ymm.FindProperty(t.GetType(), prop) == null);
        if (bad != null) return $"⚠ 選択中のアイテム(種類: {Ymm.TypeName(bad)})には、{kind}エフェクトを付けられません";
        int ok = 0;
        var errors = new List<string>();
        foreach (var t in targets)
        {
            var p = Ymm.FindProperty(t.GetType(), prop)!;
            string? json;
            try { json = BuiltinCatalog.EffectToJson(it.IsVideo, Activator.CreateInstance(it.Type)!); }
            catch (Exception ex) { errors.Add(ex.GetBaseException().Message); continue; }
            if (json == null) { errors.Add(Ymm.LastError); continue; }
            var loaded = Ymm.JsonToValue(p.PropertyType, json);
            if (loaded == null) { errors.Add(Ymm.LastError); continue; }
            try
            {
                await Ymm.EditAsync(t, () =>
                {
                    var merged = Ymm.MergeEffects(p.PropertyType, Ymm.Get(t, prop), loaded, append);
                    if (merged == null) { errors.Add(Ymm.LastError); return; }
                    if (Ymm.SetValue(t, prop, merged, out var err)) ok++; else errors.Add(err);
                });
            }
            catch (Exception ex) { errors.Add(ex.GetBaseException().Message); }
        }
        return ok > 0
            ? $"{ok} 個のアイテムに「{it.Name}」を付けました" + (append ? "" : "(前のエフェクトは外しました)") + (errors.Count > 0 ? $"(失敗 {errors.Count} 件: {errors[0]})" : "")
            : "⚠ エフェクトを付けられませんでした: " + (errors.FirstOrDefault() ?? "原因不明");
    }
}

public sealed record FxCategory(string Name, int Count)
{
    public string Label => $"{Name}({Count})";
}

public sealed record FxRow(IReadOnlyList<FxInfoItem> Tiles);

/// <summary>エフェクトタブの画面(ウィンドウごと)。映像はサンプル画像のサムネイル、音声は名前と説明の一覧</summary>
public sealed class FxBrowserVm : NotifyBase, IDisposable
{
    public const string AllName = "すべて";
    readonly ExplorerSettings settings;
    int columns = 4;
    bool updating;

    public FxBrowserVm(ExplorerSettings settings)
    {
        this.settings = settings;
        ApplyCommand = new RelayCommand(_ => _ = ApplyAsync(), _ => selected != null);
        ChooseSampleCommand = new RelayCommand(_ => ChooseSample());
        ResetSampleCommand = new RelayCommand(_ => SetSamplePath(""));
        RebuildThumbsCommand = new RelayCommand(_ => { FxCatalog.SetSample(settings.FxSampleImage ?? ""); });
        FxCatalog.ItemsChanged += OnItemsChanged;
        FxCatalog.StatusChanged += OnStatusChanged;
        Refresh();
    }

    public void Dispose()
    {
        FxCatalog.ItemsChanged -= OnItemsChanged;
        FxCatalog.StatusChanged -= OnStatusChanged;
    }

    void OnItemsChanged() => Application.Current?.Dispatcher.BeginInvoke(new Action(Refresh));
    void OnStatusChanged() { Raise(nameof(Status)); Raise(nameof(HasStatus)); }

    public string Status => FxCatalog.Status;
    public bool HasStatus => Status.Length > 0;
    public RelayCommand ApplyCommand { get; }
    public RelayCommand ChooseSampleCommand { get; }
    public RelayCommand ResetSampleCommand { get; }
    public RelayCommand RebuildThumbsCommand { get; }

    // ---------------------------------------------------------------- 映像 / 音声
    bool isAudio;
    public bool IsAudio
    {
        get => isAudio;
        set
        {
            if (!Set(ref isAudio, value)) return;
            Raise(nameof(IsVideo));
            Selected = null;
            Message = "";
            Refresh();
        }
    }
    public bool IsVideo { get => !isAudio; set => IsAudio = !value; }

    IReadOnlyList<FxInfoItem> Source => isAudio ? FxCatalog.Audio : FxCatalog.Video;

    IReadOnlyList<FxCategory> categories = Array.Empty<FxCategory>();
    public IReadOnlyList<FxCategory> Categories { get => categories; private set => Set(ref categories, value); }

    FxCategory? selectedCategory;
    public FxCategory? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (value == null && updating) return;
            if (!Set(ref selectedCategory, value)) return;
            if (!updating) Build();
        }
    }

    string search = "";
    public string Search
    {
        get => search;
        set
        {
            if (!Set(ref search, value ?? "")) return;
            Raise(nameof(HasNoSearch));
            Build();
        }
    }
    public bool HasNoSearch => search.Length == 0;

    IReadOnlyList<FxRow> rows = Array.Empty<FxRow>();
    public IReadOnlyList<FxRow> Rows { get => rows; private set => Set(ref rows, value); }

    IReadOnlyList<FxInfoItem> listItems = Array.Empty<FxInfoItem>();
    /// <summary>音声エフェクトの一覧(名前と説明)</summary>
    public IReadOnlyList<FxInfoItem> ListItems { get => listItems; private set => Set(ref listItems, value); }

    string countText = "";
    public string CountText { get => countText; private set => Set(ref countText, value); }

    FxInfoItem? selected;
    public FxInfoItem? Selected
    {
        get => selected;
        set
        {
            if (ReferenceEquals(selected, value)) return;
            if (selected != null) selected.IsSelected = false;
            selected = value;
            if (selected != null) selected.IsSelected = true;
            Raise(nameof(Selected));
            Raise(nameof(HasSelected));
            Raise(nameof(HasNoSelected));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
    public bool HasSelected => selected != null;
    public bool HasNoSelected => selected == null;

    string message = "";
    public string Message { get => message; private set => Set(ref message, value); }

    /// <summary>オン=前のエフェクトは残して、うしろに追加する</summary>
    public bool Append
    {
        get => isAudio ? settings.AudioEffectAppend : settings.VideoEffectAppend;
        set
        {
            if (isAudio) settings.AudioEffectAppend = value; else settings.VideoEffectAppend = value;
            settings.Save();
            Raise(nameof(Append));
        }
    }

    public string SampleText => string.IsNullOrWhiteSpace(settings.FxSampleImage) ? "同梱のサンプル画像" : Path.GetFileName(settings.FxSampleImage);

    public void SetColumns(int value)
    {
        value = Math.Max(1, value);
        if (value == columns) return;
        columns = value;
        Build();
    }

    public void Refresh()
    {
        var src = Source;
        var want = selectedCategory?.Name ?? AllName;
        var list = new List<FxCategory> { new(AllName, src.Count) };
        var order = new List<string>();
        foreach (var i in src) if (!order.Contains(i.Category)) order.Add(i.Category);
        list.AddRange(order.Select(c => new FxCategory(c, src.Count(i => i.Category == c))));
        updating = true;
        Categories = list;
        SelectedCategory = list.FirstOrDefault(c => c.Name == want) ?? list[0];
        updating = false;
        Raise(nameof(Append));
        Build();
    }

    void Build()
    {
        var cat = selectedCategory?.Name ?? AllName;
        var q = search.Trim();
        var hit = Source.Where(i => (cat == AllName || i.Category == cat)
            && (q.Length == 0 || SearchTags.Matches(q, i.Type.Name, i.Name, i.Category, i.Description, i.Tags))).ToList();
        if (isAudio) { ListItems = hit; Rows = Array.Empty<FxRow>(); }
        else { Rows = hit.Chunk(columns).Select(c => new FxRow(c)).ToList(); ListItems = Array.Empty<FxInfoItem>(); }
        CountText = $"{hit.Count} 種";
    }

    public async Task ApplyAsync()
    {
        var s = selected;
        if (s == null) return;
        try { Message = await FxCatalog.ApplyAsync(s, Append); }
        catch (Exception ex) { Message = "⚠ " + ex.GetBaseException().Message; }
    }

    /// <summary>タイル・行をクリックした</summary>
    public void Activate(FxInfoItem it, bool isDouble)
    {
        Selected = it;
        if (isDouble) _ = ApplyAsync();
    }

    // ---------------------------------------------------------------- サンプル画像
    void ChooseSample()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "サムネイルに使うサンプル画像を選ぶ",
            Filter = "画像|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp|すべてのファイル|*.*",
        };
        if (dlg.ShowDialog() == true) SetSamplePath(dlg.FileName);
    }

    public void SetSamplePath(string path)
    {
        settings.FxSampleImage = path ?? "";
        settings.Save();
        Raise(nameof(SampleText));
        Message = path?.Length > 0 ? "サンプル画像を変えました。映像エフェクトのサムネイルを作り直しています…" : "同梱のサンプル画像に戻しました。サムネイルを作り直しています…";
        FxCatalog.SetSample(settings.FxSampleImage);
    }
}
