using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YMM4EffectShapes;

namespace YmmExplorer;

/// <summary>演出図形タブに並ぶ図形、またはPV背景の設定1つ分(読み取り専用)</summary>
public sealed class EffectShapeInfo : NotifyBase
{
    public string Key { get; }
    public string Name { get; }
    public string Title { get; }
    public string Category { get; }
    public int Order { get; }
    public string ThumbFile { get; }

    WeakReference<BitmapSource>? cache;
    bool loading;

    public EffectShapeInfo(string key, string name, string category, int order, string thumbFile)
    {
        Key = key;
        Name = name;
        Title = name.StartsWith("演出：", StringComparison.Ordinal) ? name[3..] : name.StartsWith("演出3D：", StringComparison.Ordinal) ? name[5..] : name;
        Category = category;
        Order = order;
        ThumbFile = thumbFile;
        HasModel = YMM4EffectShapes.Solid3D.Solid3DDefaults.Is3DKey(key) && YMM4EffectShapes.Models3D.HasModelFor(YMM4EffectShapes.Solid3D.Solid3DDefaults.Key2D(key));
    }

    /// <summary>3D版で、図形に合わせた専用の3Dモデルがあるもの(一覧に「3Dモデル」の印を出す)</summary>
    public bool HasModel { get; }

    /// <summary>画面に出た時だけ、保存してあるサムネイルを読み込む(1400種以上あるので、メモリーに持ち続けない)</summary>
    public ImageSource? Thumb
    {
        get
        {
            if (cache != null && cache.TryGetTarget(out var bmp)) return bmp;
            if (!loading) _ = LoadAsync();
            return null;
        }
    }

    async Task LoadAsync()
    {
        loading = true;
        var path = ThumbFile;
        var bmp = await Task.Run(() => Decode(path));
        loading = false;
        if (bmp == null) return;
        cache = new WeakReference<BitmapSource>(bmp);
        Raise(nameof(Thumb));
    }

    static BitmapSource? Decode(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            using var ms = new MemoryStream(File.ReadAllBytes(path));
            var b = new BitmapImage();
            b.BeginInit();
            b.CacheOption = BitmapCacheOption.OnLoad;
            b.StreamSource = ms;
            b.EndInit();
            b.Freeze();
            return b;
        }
        catch { return null; }
    }

    internal void ThumbReady()
    {
        cache = null;
        Raise(nameof(Thumb));
    }

    bool isSelected;
    public bool IsSelected { get => isSelected; set => Set(ref isSelected, value); }
}

/// <summary>PV背景の設定の組み合わせ</summary>
public readonly record struct PvRecipe(PvScene Scene, PvSolid Solid, PvStyle Style, PvPalette Palette)
{
    public static PvRecipe Default => new(PvScene.Float, PvSolid.Mix, PvStyle.FillLine, PvPalette.Custom);

    public PvStage3DParameter CreateParameter() => new() { Scene = Scene, Solid = Solid, Style = Style, Palette = Palette };
}

/// <summary>
/// 演出図形の一覧とサムネイル。一覧は、読み込まれている図形から毎回作るので、図形が増減すると自動で同じになる。
/// サムネイルは初回にまとめて作り、プラグインフォルダーの ShapeThumbs に保存する(次回からは読むだけ)
/// </summary>
public static class EffectShapeCatalog
{
    /// <summary>図形の絵やサムネイルの作り方を変えた時に上げると、次の起動で全部作り直す</summary>
    const string Revision = "3";
    /// <summary>PV背景のサムネイルだけを作り直す時に上げる</summary>
    const string PvRevision = "1";
    /// <summary>3D版のサムネイルだけを作り直す時に上げる</summary>
    const string Revision3D = "1";
    const string Prefix = "YMM4EffectShapes.";
    public const string PvShapeKey = "YMM4EffectShapes.PvStage3DPlugin";

    public const string Group3D = "3D背景", Group2D = "2D背景", GroupSolid = "立体", GroupStyle = "描き方", GroupPalette = "配色";
    public static readonly string[] PvGroupOrder = [Group3D, Group2D, GroupSolid, GroupStyle, GroupPalette];

    static Task? run;

    public static string Folder => Path.Combine(Path.GetDirectoryName(typeof(EffectShapeCatalog).Assembly.Location) ?? AppContext.BaseDirectory, "ShapeThumbs");

    public static string Folder3D => Folder + "3D";

    public static IReadOnlyList<EffectShapeInfo> Items { get; private set; } = Array.Empty<EffectShapeInfo>();
    /// <summary>同じ図形の3D版(演出3D)。並び・分類は2D版と同じ</summary>
    public static IReadOnlyList<EffectShapeInfo> Items3D { get; private set; } = Array.Empty<EffectShapeInfo>();
    public static IReadOnlyList<EffectShapeInfo> PvOptions { get; } = BuildPvOptions();

    /// <summary>作成中の進み具合など(空なら表示しない)</summary>
    public static string Status { get; private set; } = "一覧を読み込んでいます…";

    public static event Action? ItemsChanged;
    public static event Action? StatusChanged;

    /// <summary>UIスレッドから呼ぶ。何度呼んでも、1回だけ動く</summary>
    public static Task EnsureAsync() => run ??= RunAsync();

    static void SetStatus(string text)
    {
        Status = text;
        StatusChanged?.Invoke();
    }

    public static string DisplayName<T>(T value) where T : struct, Enum =>
        typeof(T).GetField(value.ToString())?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();

    static List<EffectShapeInfo> BuildPvOptions()
    {
        var dir = Folder;
        var list = new List<EffectShapeInfo>();
        void Add<T>(string kind, T v, string group) where T : struct, Enum =>
            list.Add(new EffectShapeInfo($"{kind}:{v}", DisplayName(v), group, list.Count, Path.Combine(dir, $"pv_{kind}_{v}.jpg")));
        foreach (var v in Enum.GetValues<PvScene>()) if (!PvFlatInfo.IsFlat(v)) Add("Scene", v, Group3D);
        foreach (var v in Enum.GetValues<PvScene>()) if (PvFlatInfo.IsFlat(v)) Add("Scene", v, Group2D);
        foreach (var v in Enum.GetValues<PvSolid>()) Add("Solid", v, GroupSolid);
        foreach (var v in Enum.GetValues<PvStyle>()) Add("Style", v, GroupStyle);
        foreach (var v in Enum.GetValues<PvPalette>()) Add("Palette", v, GroupPalette);
        return list;
    }

    /// <summary>その設定だけを変えた、サムネイル用のPV背景</summary>
    static PvStage3DParameter ThumbParameter(string key)
    {
        var p = key.Split(':');
        return p[0] switch
        {
            "Scene" => new PvRecipe(Enum.Parse<PvScene>(p[1]), PvSolid.Mix, PvStyle.FillLine, PvMono.IsMono(Enum.Parse<PvScene>(p[1])) ? PvPalette.Paper : PvPalette.Custom).CreateParameter(),
            "Solid" => new PvRecipe(PvScene.Float, Enum.Parse<PvSolid>(p[1]), PvStyle.FillLine, PvPalette.Custom).CreateParameter(),
            "Style" => new PvRecipe(PvScene.Tunnel, PvSolid.Mix, Enum.Parse<PvStyle>(p[1]), PvPalette.Custom).CreateParameter(),
            _ => new PvRecipe(PvScene.Wave, PvSolid.Mix, PvStyle.FillLine, Enum.Parse<PvPalette>(p[1])).CreateParameter(),
        };
    }

    static async Task RunAsync()
    {
        try
        {
            var found = await Task.Run(ShapeDiscovery.Scan);
            var dir = Folder;
            Items = found.Where(f => f.Key.StartsWith(Prefix, StringComparison.Ordinal) && f.Key != PvShapeKey && !YMM4EffectShapes.Solid3D.Solid3DDefaults.Is3DKey(f.Key))
                .Select(f =>
                {
                    bool known = ShapeCategories.TryGet(f.Key, out var cat, out var order);
                    return new EffectShapeInfo(f.Key, f.Name, known ? cat : "その他", known ? order : int.MaxValue, Path.Combine(dir, f.Key + ".jpg"));
                })
                .OrderBy(i => CategoryIndex(i.Category)).ThenBy(i => i.Order).ThenBy(i => i.Name, StringComparer.Ordinal)
                .ToList();
            var keys3D = found.Where(f => YMM4EffectShapes.Solid3D.Solid3DDefaults.Is3DKey(f.Key)).ToDictionary(f => f.Key, f => f.Name);
            var dir3D = Folder3D;
            Items3D = Items.Select(i => (i, key: YMM4EffectShapes.Solid3D.Solid3DDefaults.Key3D(i.Key)))
                .Where(x => keys3D.ContainsKey(x.key))
                .Select(x => new EffectShapeInfo(x.key, keys3D[x.key], x.i.Category, x.i.Order, Path.Combine(dir3D, x.key + ".jpg")))
                .ToList();
            ItemsChanged?.Invoke();
            await BuildThumbsAsync(dir);
            await Build3DThumbsAsync(dir3D);
        }
        catch (Exception ex) { SetStatus("⚠ サムネイルを作れませんでした: " + ex.GetBaseException().Message); }
    }

    public static int CategoryIndex(string category)
    {
        int i = Array.IndexOf(ShapeCategories.Order, category);
        return i < 0 ? int.MaxValue : i;
    }

    static bool RevisionIs(string file, string value) => File.Exists(file) && File.ReadAllText(file).Trim() == value;

    static async Task BuildThumbsAsync(string dir)
    {
        Directory.CreateDirectory(dir);
        var revFile = Path.Combine(dir, "revision.txt");
        var pvRevFile = Path.Combine(dir, "pv_revision.txt");
        bool same = RevisionIs(revFile, Revision), pvSame = RevisionIs(pvRevFile, PvRevision);
        var pvTodo = PvOptions.Where(i => !pvSame || !File.Exists(i.ThumbFile)).ToList();
        var todo = Items.Where(i => !same || !File.Exists(i.ThumbFile)).ToList();
        int total = pvTodo.Count + todo.Count, done = 0;
        if (total > 0)
        {
            using var session = ShapeThumbRenderer.BeginSession();
            foreach (var it in pvTodo)
            {
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                if (done++ % 5 == 0) SetStatus($"サムネイルを作っています… {done} / {total}(初回だけです)");
                var img = ShapeThumbRenderer.RenderParameter(ThumbParameter(it.Key));
                SaveJpeg(img != null && ShapeThumbRenderer.LastError.Length == 0 ? NameThumb.Picture(img) : NameThumb.Render(it.Title, "PV背景"), it.ThumbFile);
                it.ThumbReady();
            }
            File.WriteAllText(pvRevFile, PvRevision);
            foreach (var it in todo)
            {
                // 画面の操作を優先し、手が空いた時に1つずつ作る
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                if (done++ % 5 == 0) SetStatus($"サムネイルを作っています… {done} / {total}(初回だけです)");
                Save(it);
                it.ThumbReady();
            }
        }
        File.WriteAllText(pvRevFile, PvRevision);
        File.WriteAllText(revFile, Revision);

        // もう無い図形のサムネイルを片付ける(このフォルダーの .jpg だけ)
        var keep = new HashSet<string>(Items.Concat(PvOptions).Select(i => i.ThumbFile), StringComparer.OrdinalIgnoreCase);
        foreach (var f in Directory.GetFiles(dir, "*.jpg"))
            if (!keep.Contains(f)) try { File.Delete(f); } catch { }
        SetStatus("");
    }

    /// <summary>3D版のサムネイル(2D版の後に、手が空いた時に作る)</summary>
    static async Task Build3DThumbsAsync(string dir)
    {
        Directory.CreateDirectory(dir);
        var revFile = Path.Combine(dir, "revision.txt");
        bool same = RevisionIs(revFile, Revision3D);
        var todo = Items3D.Where(i => !same || !File.Exists(i.ThumbFile)).ToList();
        if (todo.Count > 0)
        {
            using var session = ShapeThumbRenderer.BeginSession();
            int done = 0;
            foreach (var it in todo)
            {
                await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
                if (done++ % 5 == 0) SetStatus($"3Dのサムネイルを作っています… {done} / {todo.Count}(初回だけです)");
                Save(it);
                it.ThumbReady();
            }
        }
        File.WriteAllText(revFile, Revision3D);
        var keep = new HashSet<string>(Items3D.Select(i => i.ThumbFile), StringComparer.OrdinalIgnoreCase);
        foreach (var f in Directory.GetFiles(dir, "*.jpg"))
            if (!keep.Contains(f)) try { File.Delete(f); } catch { }
        SetStatus("");
    }

    static void Save(EffectShapeInfo it)
    {
        BitmapSource? img = null;
        try { img = ShapeThumbRenderer.Render(it.Key, it.Name, it.Category == "画面転換"); } catch { }
        bool drawn = img != null && ShapeThumbRenderer.LastError.Length == 0;
        SaveJpeg(drawn ? NameThumb.Picture(img!) : NameThumb.Render(it.Title, YMM4EffectShapes.Solid3D.Solid3DDefaults.Is3DKey(it.Key) ? "演出3D" : "演出図形"), it.ThumbFile);
    }

    static void SaveJpeg(BitmapSource picture, string file)
    {
        var enc = new JpegBitmapEncoder { QualityLevel = 88 };
        enc.Frames.Add(BitmapFrame.Create(picture));
        var tmp = file + ".tmp";
        using (var fs = File.Create(tmp)) enc.Save(fs);
        File.Move(tmp, file, true);
    }
}

public sealed record EffectShapeCategory(string Name, int Count)
{
    public string Label => $"{Name}({Count})";
}

public sealed record EffectShapeRow(IReadOnlyList<EffectShapeInfo> Tiles);

/// <summary>演出図形タブの画面(ウィンドウごと)。図形の追加・削除・並べ替えはできない</summary>
public sealed class EffectShapeBrowserVm : NotifyBase, IDisposable
{
    public const string AllName = "すべて";
    public const string PvName = "PV背景";

    readonly Func<string, string, Task<string?>> place;
    readonly Func<PvRecipe, string, bool, Task<string?>> placePv;
    readonly Func<string, bool> isShown;
    int columns = 4;
    bool updating;

    public EffectShapeBrowserVm(Func<string, string, Task<string?>> place, Func<PvRecipe, string, bool, Task<string?>> placePv, Func<string, bool> isShown)
    {
        this.place = place;
        this.placePv = placePv;
        this.isShown = isShown;
        PlaceCommand = new RelayCommand(_ => _ = PlaceAsync(), _ => selected != null);
        PlacePvCommand = new RelayCommand(_ => _ = PlacePvAsync());
        ResetPvCommand = new RelayCommand(_ => { recipe = PvRecipe.Default; RecipeChanged(); });
        BackToShapesCommand = new RelayCommand(_ => IsPvMode = false);
        PvGroups = EffectShapeCatalog.PvGroupOrder
            .Select(g => new EffectShapeCategory(g, EffectShapeCatalog.PvOptions.Count(o => o.Category == g))).ToList();
        selectedPvGroup = PvGroups[0];
        EffectShapeCatalog.ItemsChanged += OnItemsChanged;
        EffectShapeCatalog.StatusChanged += OnStatusChanged;
        ShapeVisibilityVm.Changed += OnItemsChanged;
        MarkRecipe();
        BuildPvRows();
        Refresh();
    }

    public void Dispose()
    {
        EffectShapeCatalog.ItemsChanged -= OnItemsChanged;
        EffectShapeCatalog.StatusChanged -= OnStatusChanged;
        ShapeVisibilityVm.Changed -= OnItemsChanged;
        previewSession?.Dispose();
        previewSession = null;
    }

    void OnItemsChanged() => System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(Refresh));
    void OnStatusChanged() { Raise(nameof(Status)); Raise(nameof(HasStatus)); }

    public string Status => EffectShapeCatalog.Status;
    public bool HasStatus => Status.Length > 0;
    public RelayCommand PlaceCommand { get; }
    public RelayCommand PlacePvCommand { get; }
    public RelayCommand ResetPvCommand { get; }
    public RelayCommand BackToShapesCommand { get; }

    // ---------------------------------------------------------------- 図形 / PV背景 の切り替え
    bool isPvMode;
    public bool IsPvMode
    {
        get => isPvMode;
        set
        {
            if (!Set(ref isPvMode, value)) return;
            Raise(nameof(IsShapeMode));
            Message = "";
            previewSession?.Dispose();
            previewSession = null;
            if (value)
            {
                previewSession = ShapeThumbRenderer.BeginSession();
                BuildPvRows();
                QueuePreview();
            }
            else BuildRows();
        }
    }
    public bool IsShapeMode { get => !isPvMode; set => IsPvMode = !value; }

    // ---------------------------------------------------------------- 2D / 3D
    bool show3D;
    /// <summary>オン=同じ図形の3D版(演出3D)を並べて、配置する。分類・検索はそのまま</summary>
    public bool Show3D
    {
        get => show3D;
        set
        {
            if (!Set(ref show3D, value)) return;
            Raise(nameof(Show2D));
            Message = "";
            var key2D = selected == null ? null : YMM4EffectShapes.Solid3D.Solid3DDefaults.Key2D(selected.Key);
            Selected = null;
            Refresh();
            if (key2D != null) Selected = Source.FirstOrDefault(i => YMM4EffectShapes.Solid3D.Solid3DDefaults.Key2D(i.Key) == key2D);
            if (isPvMode) QueuePreview();
        }
    }
    public bool Show2D { get => !show3D; set => Show3D = !value; }

    IReadOnlyList<EffectShapeInfo> Source => show3D ? EffectShapeCatalog.Items3D : EffectShapeCatalog.Items;

    // ---------------------------------------------------------------- 図形
    IReadOnlyList<EffectShapeCategory> categories = Array.Empty<EffectShapeCategory>();
    public IReadOnlyList<EffectShapeCategory> Categories { get => categories; private set => Set(ref categories, value); }

    EffectShapeCategory? selectedCategory;
    public EffectShapeCategory? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (value == null && updating) return;
            if (value?.Name == PvName)
            {
                // 「PV背景」は分類ではなく専用の画面を開く。戻った時は、前の分類のままにする
                IsPvMode = true;
                System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() => Raise(nameof(SelectedCategory))));
                return;
            }
            if (!Set(ref selectedCategory, value)) return;
            if (!updating) BuildRows();
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
            BuildRows();
        }
    }
    public bool HasNoSearch => search.Length == 0;

    IReadOnlyList<EffectShapeRow> rows = Array.Empty<EffectShapeRow>();
    public IReadOnlyList<EffectShapeRow> Rows { get => rows; private set => Set(ref rows, value); }

    string countText = "";
    public string CountText { get => countText; private set => Set(ref countText, value); }

    EffectShapeInfo? selected;
    public EffectShapeInfo? Selected
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

    public void SetColumns(int value)
    {
        value = Math.Max(1, value);
        if (value == columns) return;
        columns = value;
        BuildRows();
        BuildPvRows();
    }

    /// <summary>今の図形の一覧に合わせて、分類と表示を作り直す</summary>
    public void Refresh()
    {
        var shown = Source.Where(i => isShown(i.Key)).ToList();
        var want = selectedCategory?.Name ?? AllName;
        var list = new List<EffectShapeCategory>
        {
            new(AllName, shown.Count),
            new(PvName, EffectShapeCatalog.PvOptions.Count(o => o.Category is EffectShapeCatalog.Group3D or EffectShapeCatalog.Group2D)),
        };
        list.AddRange(shown.GroupBy(i => i.Category).OrderBy(g => EffectShapeCatalog.CategoryIndex(g.Key)).Select(g => new EffectShapeCategory(g.Key, g.Count())));
        updating = true;
        Categories = list;
        SelectedCategory = list.FirstOrDefault(c => c.Name == want) ?? list[0];
        updating = false;
        if (selected != null && !shown.Contains(selected)) Selected = null;
        BuildRows();
    }

    void BuildRows()
    {
        var cat = selectedCategory?.Name ?? AllName;
        var q = search.Trim();
        var hit = Source.Where(i => isShown(i.Key)
            && (cat == AllName || i.Category == cat)
            && (q.Length == 0 || SearchTags.Matches(q, i.Key, i.Name, i.Category))).ToList();
        Rows = hit.Chunk(columns).Select(c => new EffectShapeRow(c)).ToList();
        CountText = $"{hit.Count} 種" + (show3D ? "(3D)" : "");
    }

    public async Task PlaceAsync()
    {
        var s = selected;
        if (s == null) return;
        try
        {
            var error = await place(s.Key, s.Title);
            Message = error == null ? $"「{s.Title}」を配置しました" : "⚠ " + error;
        }
        catch (Exception ex) { Message = "⚠ " + ex.GetBaseException().Message; }
    }

    // ---------------------------------------------------------------- PV背景
    public IReadOnlyList<EffectShapeCategory> PvGroups { get; }

    EffectShapeCategory selectedPvGroup;
    public EffectShapeCategory SelectedPvGroup
    {
        get => selectedPvGroup;
        set
        {
            if (value == null || !Set(ref selectedPvGroup, value)) return;
            BuildPvRows();
        }
    }

    IReadOnlyList<EffectShapeRow> pvRows = Array.Empty<EffectShapeRow>();
    public IReadOnlyList<EffectShapeRow> PvRows { get => pvRows; private set => Set(ref pvRows, value); }

    void BuildPvRows() =>
        PvRows = EffectShapeCatalog.PvOptions.Where(o => o.Category == selectedPvGroup.Name).Chunk(columns).Select(c => new EffectShapeRow(c)).ToList();

    PvRecipe recipe = PvRecipe.Default;
    public PvRecipe Recipe => recipe;
    public string PvSceneText => EffectShapeCatalog.DisplayName(recipe.Scene) + (PvFlatInfo.IsFlat(recipe.Scene) ? "(2D)" : "(3D)");
    public string PvSolidText => PvFlatInfo.IsFlat(recipe.Scene) ? "2D背景では使いません" : EffectShapeCatalog.DisplayName(recipe.Solid);
    public string PvStyleText => PvFlatInfo.IsFlat(recipe.Scene) ? "2D背景では使いません" : EffectShapeCatalog.DisplayName(recipe.Style);
    public string PvPaletteText => EffectShapeCatalog.DisplayName(recipe.Palette);

    BitmapSource? pvPreview;
    public BitmapSource? PvPreview { get => pvPreview; private set => Set(ref pvPreview, value); }

    IDisposable? previewSession;
    bool previewQueued;

    void QueuePreview()
    {
        if (previewQueued) return;
        previewQueued = true;
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            previewQueued = false;
            if (!isPvMode) return;
            try
            {
                var prm = recipe.CreateParameter();
                if (show3D) YMM4EffectShapes.Solid3D.Solid3DDefaults.Apply(prm, new PvStage3DPlugin().Name);
                var img = ShapeThumbRenderer.RenderParameter(prm);
                if (img != null) PvPreview = NameThumb.Picture(img);
            }
            catch { }
        }));
    }

    void MarkRecipe()
    {
        var keys = new HashSet<string> { $"Scene:{recipe.Scene}", $"Solid:{recipe.Solid}", $"Style:{recipe.Style}", $"Palette:{recipe.Palette}" };
        foreach (var o in EffectShapeCatalog.PvOptions) o.IsSelected = keys.Contains(o.Key);
    }

    void RecipeChanged()
    {
        MarkRecipe();
        Raise(nameof(PvSceneText));
        Raise(nameof(PvSolidText));
        Raise(nameof(PvStyleText));
        Raise(nameof(PvPaletteText));
        QueuePreview();
    }

    /// <summary>PV背景の設定を1つ選ぶ</summary>
    public void PickPv(EffectShapeInfo option)
    {
        var p = option.Key.Split(':');
        recipe = p[0] switch
        {
            "Scene" => Enum.Parse<PvScene>(p[1]) is var sc && PvMono.IsMono(sc) && recipe.Palette == PvPalette.Custom ? recipe with { Scene = sc, Palette = PvPalette.Paper } : recipe with { Scene = Enum.Parse<PvScene>(p[1]) },
            "Solid" => recipe with { Solid = Enum.Parse<PvSolid>(p[1]) },
            "Style" => recipe with { Style = Enum.Parse<PvStyle>(p[1]) },
            _ => recipe with { Palette = Enum.Parse<PvPalette>(p[1]) },
        };
        RecipeChanged();
    }

    public async Task PlacePvAsync()
    {
        var title = "PV背景 " + EffectShapeCatalog.DisplayName(recipe.Scene);
        try
        {
            if (show3D) title = "3D " + title;
            var error = await placePv(recipe, title, show3D);
            Message = error == null ? $"「{title}」を配置しました" : "⚠ " + error;
        }
        catch (Exception ex) { Message = "⚠ " + ex.GetBaseException().Message; }
    }

    /// <summary>タイルをクリックした</summary>
    public void Activate(EffectShapeInfo it, bool isDouble)
    {
        if (it.Key.Contains(':'))
        {
            PickPv(it);
            if (isDouble) _ = PlacePvAsync();
            return;
        }
        Selected = it;
        if (isDouble) _ = PlaceAsync();
    }
}
