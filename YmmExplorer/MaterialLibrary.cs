using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>一覧に出す素材1つ分</summary>
public sealed class MaterialItem : NotifyBase
{
    public MaterialEntry Entry { get; }
    readonly MaterialStore store;
    ImageSource? thumb;
    bool thumbStarted;

    public MaterialItem(MaterialEntry e, MaterialStore s)
    {
        Entry = e;
        store = s;
    }

    public string Id => Entry.Id;
    public string FullPath => store.FullPath(Entry);

    public string Name
    {
        get => Entry.Name;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim() == Entry.Name) return;
            Entry.Name = value.Trim();
            store.Touch();
            Raise(nameof(Name));
        }
    }

    public string Memo
    {
        get => Entry.Memo;
        set
        {
            if (value == Entry.Memo) return;
            Entry.Memo = value ?? "";
            store.Touch();
            Raise(nameof(Memo));
        }
    }

    public string Kind => Entry.Kind;
    public string KindGlyph => Entry.Kind switch { "image" => "\uEB9F", "video" => "\uE714", "audio" => "\uE8D6", _ => "\uE7C3" };
    public string KindText => Entry.Kind switch { "image" => "画像", "video" => "動画", "audio" => "音声", _ => "その他" };
    public string SizeText => FileItem.FormatSize(Entry.Size);
    public string AddedText => Entry.Added.ToString("yyyy/MM/dd HH:mm");
    public string RatingText => Entry.Rating <= 0 ? "" : new string('★', Entry.Rating);
    public string TagsText => string.Join("  ", Entry.Tags.Select(t => "#" + t));
    public bool HasDuration => Entry.DurationSec > 0 && (Entry.Kind == "video" || Entry.Kind == "audio");
    public string DurationText
    {
        get
        {
            var t = TimeSpan.FromSeconds(Math.Round(Entry.DurationSec));
            return t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes}:{t.Seconds:00}";
        }
    }
    public string InfoText => $"{KindText} / {Entry.Ext.ToUpperInvariant()} / {SizeText}"
                              + (DimensionText.Length > 0 ? $" / {DimensionText}" : "")
                              + (HasDuration ? $" / 長さ: {DurationText}" : "") + $" / 追加: {AddedText}";

    public bool HasNoThumb => thumb == null;

    /// <summary>横÷縦(ウォーターフォール表示で、タイルの高さを決めるのに使う)</summary>
    public double Aspect => Entry.Width > 0 && Entry.Height > 0
        ? (double)Entry.Width / Entry.Height
        : Entry.Kind switch { "video" => 16.0 / 9, "audio" => 312.0 / 216, _ => 1.0 };

    double tileW = 150, tileH = 112;
    public double TileWidth => tileW;
    public double TileHeight => tileH;
    public void SetTile(double w, double h)
    {
        tileW = w; tileH = h;
        Raise(nameof(TileWidth)); Raise(nameof(TileHeight));
    }

    public string DimensionText => Entry.Width > 0 && Entry.Height > 0 ? $"{Entry.Width} × {Entry.Height}" : "";

    public ImageSource? Thumb
    {
        get
        {
            if (thumb == null && !thumbStarted)
            {
                thumbStarted = true;
                _ = LoadThumbAsync();
            }
            return thumb;
        }
    }

    async Task LoadThumbAsync()
    {
        var path = store.ThumbPath(Entry);
        if (ThumbCache.TryGet(path, out var cached) && cached != null)
        {
            thumb = cached;
            Raise(nameof(Thumb));
            Raise(nameof(HasNoThumb));
            return;
        }
        if (!File.Exists(path)) return;
        var image = await Task.Run<ImageSource?>(() =>
        {
            try
            {
                var b = new BitmapImage();
                b.BeginInit();
                b.CacheOption = BitmapCacheOption.OnLoad;
                b.DecodePixelWidth = 320;
                b.UriSource = new Uri(path);
                b.EndInit();
                b.Freeze();
                return b;
            }
            catch { return null; }
        });
        if (image == null) return;
        ThumbCache.Put(path, image);
        thumb = image;
        Raise(nameof(Thumb));
        Raise(nameof(HasNoThumb));
    }
}

/// <summary>左の一覧の1行(Value が null なら「すべて」、空なら「未分類」、特別な印なら「タグなし」「ゴミ箱」)</summary>
public sealed class MaterialFolderItem
{
    public const string UntaggedToken = "\u0001untagged";
    public const string TrashToken = "\u0001trash";

    public string Name { get; }
    public string? Value { get; }
    public int Count { get; }
    public string Glyph { get; }
    public bool IsReal => !string.IsNullOrEmpty(Value) && Value != UntaggedToken && Value != TrashToken;

    public MaterialFolderItem(string name, string? value, int count, string glyph = "\uE8B7")
    {
        Name = name;
        Value = value;
        Count = count;
        Glyph = glyph;
    }
}

/// <summary>色の見本1つ(色での検索・パレット表示用)</summary>
public sealed class PaletteSwatch
{
    public string Hex { get; }
    public Brush Brush { get; }
    public string Tip => "#" + Hex + "  クリックで、この色に近い素材を探します";
    public PaletteSwatch(string hex)
    {
        Hex = hex;
        Brush = ColorTools.Brush(hex);
    }
}

/// <summary>左の「タグ」一覧の1つ(押すと、そのタグで絞り込む)</summary>
public sealed class MaterialTagItem : NotifyBase
{
    readonly Action changed;
    bool isActive;

    public string Name { get; }
    public int Count { get; }
    public string Display => $"{Name} ({Count})";

    public bool IsActive
    {
        get => isActive;
        set
        {
            if (Set(ref isActive, value)) changed();
        }
    }

    public MaterialTagItem(string name, int count, bool active, Action changed)
    {
        Name = name;
        Count = count;
        isActive = active;
        this.changed = changed;
    }
}

/// <summary>素材データベース(Eagle風の素材管理)</summary>
public sealed class MaterialLibraryVm : NotifyBase
{
    readonly MaterialStore store = MaterialStore.Instance;
    readonly ExplorerSettings settings = ExplorerSettings.Instance;
    List<MaterialItem> selected = new();
    bool suppressFilter;
    bool isImporting;
    int previewToken;

    public ObservableCollection<MaterialItem> Items { get; } = new();
    public ObservableCollection<MaterialFolderItem> FolderItems { get; } = new();
    public ObservableCollection<MaterialTagItem> TagItems { get; } = new();
    public ObservableCollection<string> DetailTags { get; } = new();
    public ObservableCollection<string> FolderChoices { get; } = new();
    public ObservableCollection<string> ExtChoices { get; } = new();
    public ObservableCollection<PaletteSwatch> DetailPalette { get; } = new();
    public ICollectionView TagView { get; }

    public EnumOption[] SortOptions { get; } =
    {
        new("added", "追加した日"), new("name", "名前"), new("size", "サイズ"), new("rating", "評価"),
        new("ext", "拡張子"), new("random", "ランダム"),
    };

    /// <summary>色で探すための、よく使う色(Eagleの色パレットのように)</summary>
    public PaletteSwatch[] ColorPresets { get; } = new[]
    {
        "FF3B30", "FF9500", "FFCC00", "34C759", "00C7BE", "32ADE6", "007AFF", "5856D6",
        "AF52DE", "FF2D92", "A2845E", "FFFFFF", "8E8E93", "000000",
    }.Select(h => new PaletteSwatch(h)).ToArray();

    public ICommand AddTagCommand { get; }
    public ICommand RemoveTagCommand { get; }
    public ICommand SetRatingCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ClearTagFilterCommand { get; }
    public ICommand AddFolderCommand { get; }
    public ICommand DeleteFolderCommand { get; }
    public ICommand ShowInExplorerCommand { get; }
    public ICommand CopyPathCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand RestoreCommand { get; }
    public ICommand PermanentDeleteCommand { get; }
    public ICommand EmptyTrashCommand { get; }
    public ICommand PickColorCommand { get; }
    public ICommand ClearColorCommand { get; }
    public ICommand ApplyHexCommand { get; }
    public ICommand SimilarColorCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ViewerCloseCommand { get; }
    public ICommand ViewerNextCommand { get; }
    public ICommand ViewerPrevCommand { get; }
    public ICommand ViewerOpenCommand { get; }
    public ICommand ViewerPageNextCommand { get; }
    public ICommand ViewerPagePrevCommand { get; }

    /// <summary>動画・音声のプレビューを読み込む(null なら解放)</summary>
    public event Action<string?>? MediaRequested;

    /// <summary>作り直した一覧の中で、選択を戻したい素材</summary>
    public event Action<IReadOnlyList<MaterialItem>>? RestoreSelection;

    /// <summary>AI音声生成(Audio Studio MCP と接続)</summary>
    public AudioStudioVm Studio { get; }

    /// <summary>ローカルAIで画像・音楽を作る(Local AI Studio と連携)</summary>
    public LocalStudioGenVm LocalGen { get; }

    public MaterialLibraryVm()
    {
        Studio = new AudioStudioVm(this);
        LocalGen = new LocalStudioGenVm(this);
        AddTagCommand = new RelayCommand(_ => AddTags());
        RemoveTagCommand = new RelayCommand(p => { if (p is string t && selected.Count > 0) store.RemoveTag(selected.Select(s => s.Id).ToList(), t); });
        SetRatingCommand = new RelayCommand(p =>
        {
            if (selected.Count == 0 || !int.TryParse(p?.ToString(), out var n)) return;
            var ids = selected.Select(s => s.Id).ToList();
            // いまと同じ星を押したら、評価を外す
            if (selected.Count == 1 && selected[0].Entry.Rating == n) n = 0;
            store.SetRating(ids, n);
        });
        TagView = CollectionViewSource.GetDefaultView(TagItems);
        TagView.Filter = o => o is MaterialTagItem t
            && (tagFilterText.Trim().Length == 0 || t.Name.Contains(tagFilterText.Trim(), StringComparison.CurrentCultureIgnoreCase));
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => selected.Count > 0);
        RestoreCommand = new RelayCommand(_ =>
        {
            var n = store.Restore(selected.Select(s => s.Id).ToList());
            if (n > 0) Message = $"{n} 個の素材をゴミ箱から元に戻しました";
        }, _ => selected.Count > 0 && IsTrashView);
        EmptyTrashCommand = new RelayCommand(_ => EmptyTrash(), _ => IsTrashView && store.Trashed().Count > 0);
        PermanentDeleteCommand = new RelayCommand(_ => DeletePermanently(), _ => selected.Count > 0);
        PickColorCommand = new RelayCommand(p => { if (p is string h) SetColorFilter(h); });
        ClearColorCommand = new RelayCommand(_ => SetColorFilter(null));
        ApplyHexCommand = new RelayCommand(_ =>
        {
            if (ColorTools.TryParse(colorHexText, out var r, out var g, out var b)) SetColorFilter(ColorTools.ToHex(r, g, b));
            else Message = "色は「#RRGGBB」の形で入力してください(例: #FF8800)";
        });
        SimilarColorCommand = new RelayCommand(_ =>
        {
            var first = selected.FirstOrDefault()?.Entry.Palette.FirstOrDefault();
            if (first != null) SetColorFilter(first);
        }, _ => selected.Count > 0 && selected[0].Entry.Palette.Count > 0);
        ClearFiltersCommand = new RelayCommand(_ => ClearAllFilters());
        ViewerCloseCommand = new RelayCommand(_ => CloseViewer());
        ViewerNextCommand = new RelayCommand(_ => StepViewer(1));
        ViewerPrevCommand = new RelayCommand(_ => StepViewer(-1));
        ViewerOpenCommand = new RelayCommand(_ => OpenViewer(null), _ => selected.Count > 0);
        ViewerPageNextCommand = new RelayCommand(_ => StepViewerPage(1));
        ViewerPagePrevCommand = new RelayCommand(_ => StepViewerPage(-1));
        ClearTagFilterCommand = new RelayCommand(_ => ClearTagFilter());
        AddFolderCommand = new RelayCommand(_ => AddFolder());
        DeleteFolderCommand = new RelayCommand(_ => DeleteFolder(), _ => selectedFolderItem is { IsReal: true });
        ShowInExplorerCommand = new RelayCommand(_ =>
        {
            var f = selected.FirstOrDefault();
            if (f != null) try { Process.Start("explorer.exe", $"/select,\"{f.FullPath}\""); } catch { }
        }, _ => selected.Count > 0);
        CopyPathCommand = new RelayCommand(_ =>
        {
            if (selected.Count == 0) return;
            try { Clipboard.SetText(string.Join(Environment.NewLine, selected.Select(s => s.FullPath))); Message = "ファイルのパスをコピーしました"; }
            catch { }
        }, _ => selected.Count > 0);
        OpenCommand = new RelayCommand(_ =>
        {
            var f = selected.FirstOrDefault();
            if (f != null) try { Process.Start(new ProcessStartInfo(f.FullPath) { UseShellExecute = true }); } catch { }
        }, _ => selected.Count > 0);

        store.Changed += OnStoreChanged;
    }

    public void Dispose()
    {
        store.Changed -= OnStoreChanged;
        Studio.Dispose();
    }

    void OnStoreChanged()
    {
        Raise(nameof(LibraryRoot));
        Reload();
    }

    string message = "";
    public string Message { get => message; private set => Set(ref message, value); }

    // ------------------------------------------------------------------ 設定
    public string LibraryRoot => store.Root;

    public bool ImportMove
    {
        get => settings.MaterialImportMove;
        set
        {
            if (settings.MaterialImportMove == value) return;
            settings.MaterialImportMove = value;
            settings.Save();
            Raise(nameof(ImportMove));
            Raise(nameof(ImportCopy));
        }
    }

    public bool ImportCopy { get => !ImportMove; set => ImportMove = !value; }

    public bool TagsFromFolders
    {
        get => settings.MaterialTagsFromFolders;
        set
        {
            if (settings.MaterialTagsFromFolders == value) return;
            settings.MaterialTagsFromFolders = value;
            settings.Save();
            Raise(nameof(TagsFromFolders));
        }
    }

    // ------------------------------------------------------------------ 絞り込み・並べ替え
    string searchText = "";
    public string SearchText { get => searchText; set { if (Set(ref searchText, value ?? "")) Refilter(); } }

    string kindFilter = "all";
    void SetKind(string kind)
    {
        if (kindFilter == kind) return;
        kindFilter = kind;
        Raise(nameof(KindAll)); Raise(nameof(KindImage)); Raise(nameof(KindVideo)); Raise(nameof(KindAudio)); Raise(nameof(KindOther));
        Refilter();
    }
    public bool KindAll { get => kindFilter == "all"; set { if (value) SetKind("all"); } }
    public bool KindImage { get => kindFilter == "image"; set { if (value) SetKind("image"); } }
    public bool KindVideo { get => kindFilter == "video"; set { if (value) SetKind("video"); } }
    public bool KindAudio { get => kindFilter == "audio"; set { if (value) SetKind("audio"); } }
    public bool KindOther { get => kindFilter == "other"; set { if (value) SetKind("other"); } }

    string sortMode = "added";
    public string SortMode { get => sortMode; set { if (Set(ref sortMode, value ?? "added")) Refilter(); } }

    bool sortDescending = true;
    public bool SortDescending { get => sortDescending; set { if (Set(ref sortDescending, value)) Refilter(); } }

    MaterialFolderItem? selectedFolderItem;
    public MaterialFolderItem? SelectedFolderItem
    {
        get => selectedFolderItem;
        set
        {
            if (!Set(ref selectedFolderItem, value)) return;
            Raise(nameof(CanDeleteFolder));
            Raise(nameof(IsTrashView));
            Raise(nameof(IsNotTrashView));
            if (!suppressFilter) Refilter();
        }
    }
    public bool CanDeleteFolder => selectedFolderItem is { IsReal: true };
    public bool IsTrashView => selectedFolderItem?.Value == MaterialFolderItem.TrashToken;
    public bool IsNotTrashView => !IsTrashView;

    // ---- フィルター(評価・拡張子・形・色)
    bool filterOpen;
    public bool FilterOpen { get => filterOpen; set => Set(ref filterOpen, value); }

    int ratingIndex;
    public int RatingIndex { get => ratingIndex; set { if (Set(ref ratingIndex, Math.Clamp(value, 0, 5))) Refilter(); } }

    int shapeIndex;
    public int ShapeIndex { get => shapeIndex; set { if (Set(ref shapeIndex, Math.Clamp(value, 0, 3))) Refilter(); } }

    string? extFilter;
    public string ExtChoice
    {
        get => extFilter ?? "(すべて)";
        set
        {
            if (value == null) return;
            var v = value == "(すべて)" ? null : value;
            if (string.Equals(v, extFilter, StringComparison.OrdinalIgnoreCase)) return;
            extFilter = v;
            Raise(nameof(ExtChoice));
            Refilter();
        }
    }

    string? colorFilter;
    public bool HasColorFilter => colorFilter != null;
    public Brush ColorFilterBrush => colorFilter == null ? Brushes.Transparent : ColorTools.Brush(colorFilter);
    public string ColorFilterText => colorFilter == null ? "色は指定していません。下の色か、素材の色をクリックしてください" : $"#{colorFilter} に近い色の素材を、近い順に表示中";

    double colorTolerance = 28;
    public double ColorTolerance { get => colorTolerance; set { if (Set(ref colorTolerance, value) && colorFilter != null) Refilter(); } }

    string colorHexText = "";
    public string ColorHexText { get => colorHexText; set => Set(ref colorHexText, value ?? ""); }

    void SetColorFilter(string? hex)
    {
        colorFilter = hex;
        if (hex != null) { ColorHexText = "#" + hex; FilterOpen = true; }
        Raise(nameof(HasColorFilter)); Raise(nameof(ColorFilterBrush)); Raise(nameof(ColorFilterText));
        Refilter();
    }

    void ClearAllFilters()
    {
        suppressFilter = true;
        kindFilter = "all"; ratingIndex = 0; shapeIndex = 0; extFilter = null; colorFilter = null; searchText = "";
        foreach (var t in TagItems) t.IsActive = false;
        suppressFilter = false;
        foreach (var n in new[] { nameof(KindAll), nameof(KindImage), nameof(KindVideo), nameof(KindAudio), nameof(KindOther),
                                  nameof(RatingIndex), nameof(ShapeIndex), nameof(ExtChoice), nameof(HasColorFilter),
                                  nameof(ColorFilterBrush), nameof(ColorFilterText), nameof(SearchText) })
            Raise(n);
        Refilter();
    }

    int ActiveFilterCount =>
        (kindFilter != "all" ? 1 : 0) + (ratingIndex > 0 ? 1 : 0) + (shapeIndex > 0 ? 1 : 0)
        + (extFilter != null ? 1 : 0) + (colorFilter != null ? 1 : 0);
    public string FilterButtonText => ActiveFilterCount > 0 ? $"フィルター ({ActiveFilterCount})" : "フィルター";

    string tagFilterText = "";
    public string TagFilterText { get => tagFilterText; set { if (Set(ref tagFilterText, value ?? "")) TagView.Refresh(); } }

    // ---- 表示(タイルの大きさ・並べ方)
    double tileSize = 150;
    public double TileSize { get => tileSize; set { if (Set(ref tileSize, Math.Clamp(value, 80, 360))) ApplyLayout(); } }

    /// <summary>0=ウォーターフォール / 1=グリッド / 2=行そろえ(画像の形に合わせて、タイルの幅が変わる)</summary>
    int layoutMode;
    void SetLayout(int mode)
    {
        if (layoutMode == mode) return;
        layoutMode = mode;
        Raise(nameof(LayoutWaterfall)); Raise(nameof(LayoutGrid)); Raise(nameof(LayoutJustified));
        ApplyLayout();
    }
    public bool LayoutWaterfall { get => layoutMode == 0; set { if (value) SetLayout(0); } }
    public bool LayoutGrid { get => layoutMode == 1; set { if (value) SetLayout(1); } }
    public bool LayoutJustified { get => layoutMode == 2; set { if (value) SetLayout(2); } }

    bool showNames = true;
    public bool ShowNames { get => showNames; set => Set(ref showNames, value); }

    /// <summary>一覧の表示幅(行そろえ表示で、1行に入る数を決めるのに使う)</summary>
    double viewportWidth = 800;
    public double ViewportWidth
    {
        get => viewportWidth;
        set
        {
            if (Math.Abs(value - viewportWidth) < 1) return;
            viewportWidth = value;
            if (layoutMode == 2) ApplyLayout();
        }
    }

    void ApplyTile(MaterialItem i)
    {
        double w = tileSize;
        double h = layoutMode == 0 ? Math.Clamp(w / Math.Max(0.2, i.Aspect), w * 0.45, w * 2.2) : w * 0.75;
        i.SetTile(w, h);
    }

    /// <summary>行そろえ: 1行の高さをそろえ、画像の縦横比に合わせて幅を変え、行の右端までぴったり埋める</summary>
    void Justify()
    {
        const double chrome = 16;                                  // タイル1つの枠・余白の分
        double avail = Math.Max(200, viewportWidth - 16 - 24);       // 一覧の余白とスクロールバーの分を引く
        double baseH = tileSize * 0.8;
        var row = new List<(MaterialItem item, double a)>();
        double sumA = 0;

        void Flush(bool last)
        {
            if (row.Count == 0) return;
            double h = (avail - row.Count * chrome) / sumA;
            if (last || h > baseH * 1.5) h = Math.Min(h, baseH);     // 最後の行は、引き伸ばさない
            h = Math.Max(40, h);
            foreach (var (item, a) in row) item.SetTile(Math.Max(30, Math.Floor(a * h)), Math.Floor(h));
            row.Clear();
            sumA = 0;
        }

        foreach (var i in Items)
        {
            double a = Math.Clamp(i.Aspect, 0.4, 3.5);
            row.Add((i, a));
            sumA += a;
            if (sumA * baseH + row.Count * chrome >= avail) Flush(false);
        }
        Flush(true);
    }

    void ApplyLayout()
    {
        if (layoutMode == 2) { Justify(); return; }
        foreach (var i in Items) ApplyTile(i);
    }

    string newFolderText = "";
    public string NewFolderText { get => newFolderText; set => Set(ref newFolderText, value ?? ""); }

    public string CountText => IsTrashView ? $"ゴミ箱 {Items.Count} 個" : $"{Items.Count} / {store.All().Count} 個";
    public bool HasNoItems => Items.Count == 0;
    public bool IsImporting => isImporting;

    void OnTagFilterChanged() => Refilter();

    void ClearTagFilter()
    {
        suppressFilter = true;
        foreach (var t in TagItems) t.IsActive = false;
        suppressFilter = false;
        Refilter();
    }

    /// <summary>ライブラリーを読み直して、フォルダー・タグの一覧と素材の一覧を作り直す</summary>
    public void Reload()
    {
        var all = store.All();
        var prevFolder = selectedFolderItem?.Value;
        var activeTags = TagItems.Where(t => t.IsActive).Select(t => t.Name).ToHashSet(StringComparer.CurrentCultureIgnoreCase);

        suppressFilter = true;
        FolderItems.Clear();
        FolderItems.Add(new MaterialFolderItem("すべての素材", null, all.Count, "\uE8F1"));
        FolderItems.Add(new MaterialFolderItem("未分類", "", all.Count(e => string.IsNullOrEmpty(e.Folder)), "\uE8B7"));
        FolderItems.Add(new MaterialFolderItem("タグなし", MaterialFolderItem.UntaggedToken, all.Count(e => e.Tags.Count == 0), "\uE8EC"));
        FolderItems.Add(new MaterialFolderItem("ゴミ箱", MaterialFolderItem.TrashToken, store.Trashed().Count, "\uE74D"));
        foreach (var f in store.Folders()) FolderItems.Add(new MaterialFolderItem(f, f, all.Count(e => e.Folder == f), "\uE8B7"));
        SelectedFolderItem = FolderItems.FirstOrDefault(f => f.Value == prevFolder) ?? FolderItems[0];

        TagItems.Clear();
        foreach (var g in all.SelectMany(e => e.Tags).GroupBy(t => t, StringComparer.CurrentCultureIgnoreCase)
                             .OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase))
            TagItems.Add(new MaterialTagItem(g.Key, g.Count(), activeTags.Contains(g.Key), OnTagFilterChanged));

        FolderChoices.Clear();
        FolderChoices.Add("(未分類)");
        foreach (var f in store.Folders()) FolderChoices.Add(f);

        ExtChoices.Clear();
        ExtChoices.Add("(すべて)");
        foreach (var x in all.Select(e => e.Ext).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase)
                             .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            ExtChoices.Add(x);
        if (extFilter != null && !ExtChoices.Contains(extFilter, StringComparer.OrdinalIgnoreCase)) extFilter = null;
        Raise(nameof(ExtChoice));
        suppressFilter = false;

        Refilter();
        _ = store.BackfillAsync();
    }

    void Refilter()
    {
        if (suppressFilter) return;
        var keep = selected.Select(s => s.Id).ToHashSet();
        var ci = StringComparison.CurrentCultureIgnoreCase;

        var folder = selectedFolderItem?.Value;
        bool trashView = folder == MaterialFolderItem.TrashToken;
        IEnumerable<MaterialEntry> q = trashView ? store.Trashed() : store.All();
        if (!trashView && folder != null)
        {
            if (folder == MaterialFolderItem.UntaggedToken) q = q.Where(e => e.Tags.Count == 0);
            else q = q.Where(e => (e.Folder ?? "") == folder);
        }
        if (kindFilter != "all") q = q.Where(e => e.Kind == kindFilter);
        foreach (var tag in TagItems.Where(t => t.IsActive).Select(t => t.Name).ToList())
        {
            var t = tag;
            q = q.Where(e => e.Tags.Contains(t, StringComparer.CurrentCultureIgnoreCase));
        }
        if (ratingIndex > 0) q = q.Where(e => e.Rating >= ratingIndex);
        if (extFilter != null) q = q.Where(e => string.Equals(e.Ext, extFilter, StringComparison.OrdinalIgnoreCase));
        if (shapeIndex > 0)
            q = q.Where(e =>
            {
                if (e.Width <= 0 || e.Height <= 0) return false;
                double r = (double)e.Width / e.Height;
                return shapeIndex switch { 1 => r > 1.1, 2 => r < 0.9, _ => r >= 0.9 && r <= 1.1 };
            });
        var s = searchText.Trim();
        if (s.Length > 0)
            q = q.Where(e => e.Name.Contains(s, ci) || e.Memo.Contains(s, ci) || e.Ext.Contains(s, ci) || e.Tags.Any(t => t.Contains(s, ci)));

        List<MaterialEntry> list;
        if (colorFilter != null)
        {
            // 色で探している時は、近い順に並べる
            var cf = colorFilter;
            list = q.Select(e => (e, d: e.Palette.Count == 0 ? 999.0 : e.Palette.Min(h => ColorTools.Distance(h, cf))))
                    .Where(x => x.d <= colorTolerance).OrderBy(x => x.d).Select(x => x.e).ToList();
        }
        else if (sortMode == "random")
        {
            var rng = new Random();
            list = q.OrderBy(_ => rng.Next()).ToList();
        }
        else
        {
            IOrderedEnumerable<MaterialEntry> ordered = sortMode switch
            {
                "name" => q.OrderBy(e => e.Name, NaturalComparer.Instance),
                "size" => q.OrderBy(e => e.Size),
                "rating" => q.OrderBy(e => e.Rating).ThenBy(e => e.Name, NaturalComparer.Instance),
                "ext" => q.OrderBy(e => e.Ext, StringComparer.OrdinalIgnoreCase).ThenBy(e => e.Name, NaturalComparer.Instance),
                _ => q.OrderBy(e => e.Added),
            };
            list = (sortDescending ? ordered.Reverse() : ordered).ToList();
        }

        Items.Clear();
        foreach (var e in list) Items.Add(new MaterialItem(e, store));
        ApplyLayout();
        Raise(nameof(FilterButtonText));
        if (viewerItem != null)
        {
            // 一覧を作り直した時は、表示中の素材を新しい一覧の中から探し直す(無くなっていたら閉じる)
            var again = Items.FirstOrDefault(i => i.Id == viewerItem.Id);
            if (again == null) CloseViewer(); else viewerItem = again;
        }
        Raise(nameof(CountText));
        Raise(nameof(HasNoItems));

        var restore = Items.Where(i => keep.Contains(i.Id)).ToList();
        if (restore.Count > 0) RestoreSelection?.Invoke(restore);
    }

    // ------------------------------------------------------------------ 選択・詳細
    MaterialItem? First => selected.FirstOrDefault();
    public bool HasSelection => selected.Count > 0;
    public bool HasSingle => selected.Count == 1;
    public bool HasMulti => selected.Count > 1;
    public bool HasNoSelection => selected.Count == 0;
    public string SelectionText => selected.Count > 1 ? $"{selected.Count} 個を選択中(タグ・フォルダー・評価を、まとめて設定できます)" : "";
    public string DetailInfo => HasSingle ? First!.InfoText : "";
    public string DetailDimensions => HasSingle ? First!.DimensionText : "";

    public string DetailUrl
    {
        get => HasSingle ? First!.Entry.Url : "";
        set
        {
            if (!HasSingle) return;
            var v = (value ?? "").Trim();
            if (v == First!.Entry.Url) return;
            First.Entry.Url = v;
            store.SetUrl(First.Id, v);
        }
    }

    string Star(int n) => selected.Count > 0 && selected.All(s => s.Entry.Rating >= n) ? "★" : "☆";
    public string Star1 => Star(1);
    public string Star2 => Star(2);
    public string Star3 => Star(3);
    public string Star4 => Star(4);
    public string Star5 => Star(5);

    public string DetailName
    {
        get => HasSingle ? First!.Name : "";
        set { if (HasSingle) First!.Name = value; }
    }

    public string DetailMemo
    {
        get => HasSingle ? First!.Memo : "";
        set { if (HasSingle) First!.Memo = value; }
    }

    public string DetailRatingText
    {
        get
        {
            if (selected.Count == 0) return "";
            var r = selected[0].Entry.Rating;
            if (selected.Any(s => s.Entry.Rating != r)) return "(評価はそれぞれ違います)";
            return r <= 0 ? "(評価なし)" : new string('★', r) + new string('☆', 5 - r);
        }
    }

    public string DetailFolder
    {
        get
        {
            if (selected.Count == 0) return "";
            var f = selected[0].Entry.Folder ?? "";
            if (selected.Any(s => (s.Entry.Folder ?? "") != f)) return "";
            return f.Length == 0 ? "(未分類)" : f;
        }
        set
        {
            if (selected.Count == 0 || string.IsNullOrEmpty(value)) return;
            var target = value == "(未分類)" ? "" : value;
            if (selected.All(s => (s.Entry.Folder ?? "") == target)) return;
            store.SetFolder(selected.Select(s => s.Id).ToList(), target);
        }
    }

    string newTagText = "";
    public string NewTagText { get => newTagText; set => Set(ref newTagText, value ?? ""); }

    public void SetSelection(IEnumerable<MaterialItem> sel)
    {
        selected = sel.ToList();
        DetailTags.Clear();
        foreach (var t in selected.SelectMany(i => i.Entry.Tags).Distinct(StringComparer.CurrentCultureIgnoreCase)) DetailTags.Add(t);
        foreach (var n in new[]
                 {
                     nameof(HasSelection), nameof(HasSingle), nameof(HasMulti), nameof(HasNoSelection), nameof(SelectionText),
                     nameof(DetailInfo), nameof(DetailName), nameof(DetailMemo), nameof(DetailRatingText), nameof(DetailFolder),
                     nameof(DetailDimensions), nameof(DetailUrl), nameof(Star1), nameof(Star2), nameof(Star3), nameof(Star4), nameof(Star5),
                     nameof(HasPalette),
                 })
            Raise(n);
        DetailPalette.Clear();
        if (selected.Count == 1) foreach (var h in selected[0].Entry.Palette) DetailPalette.Add(new PaletteSwatch(h));
        Raise(nameof(HasPalette));
        UpdatePreview();
        CommandManager.InvalidateRequerySuggested();
    }

    public bool HasPalette => DetailPalette.Count > 0;

    public List<string> SelectedPaths() => selected.Select(s => s.FullPath).Where(File.Exists).ToList();

    void AddTags()
    {
        if (selected.Count == 0) { Message = "タグを付けたい素材を選んでください"; return; }
        var tags = NewTagText.Split(new[] { ',', '、', ' ', '　', '#' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tags.Length == 0) return;
        var ids = selected.Select(s => s.Id).ToList();
        NewTagText = "";
        store.AddTags(ids, tags);
        Message = $"{ids.Count} 個の素材に、タグ「{string.Join("」「", tags)}」を付けました";
    }

    void AddFolder()
    {
        var name = NewFolderText.Trim();
        if (name.Length == 0) return;
        if (!store.AddFolder(name)) { Message = "同じ名前のフォルダーがあります"; return; }
        NewFolderText = "";
        Message = $"フォルダー「{name}」を作りました(素材は、詳細パネルの「フォルダー」から入れられます)";
    }

    void DeleteFolder()
    {
        if (selectedFolderItem is not { IsReal: true } f) return;
        var answer = MessageBox.Show($"フォルダー「{f.Name}」を削除します。\n中の素材は消えず、「未分類」に戻ります。\n\nよろしいですか?",
            "フォルダーの削除", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes) return;
        store.RemoveFolder(f.Value!);
        Message = $"フォルダー「{f.Name}」を削除しました";
    }

    void DeleteSelected()
    {
        if (selected.Count == 0) return;
        var ids = selected.Select(x => x.Id).ToList();
        MediaRequested?.Invoke(null);          // 再生中のファイルを手放す
        if (!IsTrashView)
        {
            var moved = store.Trash(ids);
            Message = $"{moved} 個の素材をゴミ箱へ入れました(左の「ゴミ箱」から元に戻せます)";
            return;
        }
        DeletePermanently();
    }

    /// <summary>ゴミ箱を通さずに、ライブラリー内のファイルごと完全に削除する(確認あり)</summary>
    void DeletePermanently()
    {
        if (selected.Count == 0) return;
        var ids = selected.Select(x => x.Id).ToList();
        var answer = MessageBox.Show(
            $"{ids.Count} 個の素材を、完全に削除します。\nライブラリー内のファイルも消えます(元のファイルを残して取り込んだ場合、元のファイルは消えません)。\n\nよろしいですか?",
            "完全に削除", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;
        MediaRequested?.Invoke(null);          // 再生中のファイルを手放す
        var n = store.Remove(ids);
        Message = $"{n} 個の素材を完全に削除しました";
    }

    void EmptyTrash()
    {
        var count = store.Trashed().Count;
        if (count == 0) return;
        var answer = MessageBox.Show($"ゴミ箱の {count} 個の素材を、すべて完全に削除します。\nライブラリー内のファイルも消えます。\n\nよろしいですか?",
            "ゴミ箱を空にする", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;
        MediaRequested?.Invoke(null);
        Message = $"ゴミ箱を空にしました({store.EmptyTrash()} 個)";
    }

    // ------------------------------------------------------------------ プレビュー
    ImageSource? previewImage;
    public ImageSource? PreviewImage => previewImage;
    public bool ShowImagePreview => previewImage != null;
    public bool ShowMediaPreview => HasSingle && (First!.Kind == "video" || First.Kind == "audio");
    public bool ShowAudioGlyph => HasSingle && First!.Kind == "audio";
    public bool ShowOtherGlyph => HasSingle && previewImage == null && First!.Kind != "video" && First.Kind != "audio";

    void UpdatePreview()
    {
        var token = ++previewToken;
        MediaRequested?.Invoke(null);
        previewImage = null;
        var item = HasSingle ? First : null;
        if (item != null)
        {
            if (item.Kind == "video" || item.Kind == "audio") { if (viewerItem == null) MediaRequested?.Invoke(item.FullPath); }
            else _ = LoadImagePreviewAsync(item, token);
        }
        RaisePreview();
    }

    async Task LoadImagePreviewAsync(MaterialItem item, int token)
    {
        var path = item.FullPath;
        var image = item.Kind == "image"
            ? await Task.Run<ImageSource?>(() =>
            {
                try
                {
                    var b = new BitmapImage();
                    b.BeginInit();
                    b.CacheOption = BitmapCacheOption.OnLoad;
                    b.DecodePixelWidth = 1000;
                    b.UriSource = new Uri(path);
                    b.EndInit();
                    b.Freeze();
                    return b;
                }
                catch { return null; }
            })
            : null;
        image ??= item.Thumb;            // 読み込めない形式は、サムネイルで代用
        if (token != previewToken) return;
        previewImage = image;
        RaisePreview();
    }

    // ------------------------------------------------------------------ 大きなプレビュー(独自のビューアー)
    /// <summary>ビューアーで再生する動画・音声(null なら解放)</summary>
    public event Action<string?>? ViewerMediaRequested;

    /// <summary>ビューアーの表示が変わった(拡大・位置を元に戻す合図)</summary>
    public event Action? ViewerChanged;

    MaterialItem? viewerItem;
    ImageSource? viewerImage;
    bool viewerLoading;
    int viewerToken;
    string viewerMode = "other";
    string viewerText = "";
    PdfDoc? viewerPdf;
    int viewerPage, viewerPageCount;

    public bool ViewerOpen => viewerItem != null;
    public ImageSource? ViewerImage => viewerImage;
    public string ViewerText => viewerText;
    public bool ViewerShowMedia => viewerItem != null && (viewerMode == "video" || viewerMode == "audio");
    public bool ViewerShowImage => viewerItem != null && viewerImage != null && viewerMode != "video" && viewerMode != "text";
    public bool ViewerShowText => viewerItem != null && viewerMode == "text" && !viewerLoading;
    public bool ViewerShowPdfNav => viewerItem != null && viewerMode == "pdf" && viewerPageCount > 0;
    public string ViewerPageText => viewerPageCount > 0 ? $"ページ {viewerPage + 1} / {viewerPageCount}" : "";
    public bool ViewerShowOther => viewerItem != null && viewerMode != "video" && viewerMode != "audio" && viewerMode != "text"
                                   && viewerImage == null && !viewerLoading;
    public bool ViewerCanZoom => viewerItem != null && viewerImage != null && (viewerMode == "image" || viewerMode == "pdf" || viewerMode == "other");
    public string ViewerTitle => viewerItem?.Name ?? "";
    public string ViewerInfo => viewerItem?.InfoText ?? "";
    public string ViewerPositionText => viewerItem == null ? "" : $"{Items.IndexOf(viewerItem) + 1} / {Items.Count}";
    public string ViewerHint => viewerItem == null ? "" : viewerMode switch
    {
        "video" or "audio" => "Space: 再生・一時停止 / ← →: 前後の素材 / Esc: 閉じる",
        "text" => "← →: 前後の素材 / Esc: 閉じる(文字は選んでコピーできます)",
        "pdf" => "PageUp / PageDown: ページ送り / ホイール: 拡大・縮小 / ドラッグ: 移動 / ← →: 前後の素材 / Esc: 閉じる",
        _ => "ホイール: 拡大・縮小 / ドラッグ: 移動 / ダブルクリック: 元の大きさ / ← →: 前後の素材 / Esc: 閉じる",
    };

    void RaiseViewer()
    {
        foreach (var n in new[] { nameof(ViewerOpen), nameof(ViewerImage), nameof(ViewerShowMedia), nameof(ViewerShowImage), nameof(ViewerShowOther),
                                  nameof(ViewerCanZoom), nameof(ViewerTitle), nameof(ViewerInfo), nameof(ViewerPositionText), nameof(ViewerHint),
                                  nameof(ViewerText), nameof(ViewerShowText), nameof(ViewerShowPdfNav), nameof(ViewerPageText) })
            Raise(n);
    }

    /// <summary>素材を、プラグインの中の大きなプレビューで開く(item が null なら、選択中の先頭)</summary>
    public void OpenViewer(MaterialItem? item)
    {
        item ??= selected.FirstOrDefault();
        if (item == null) return;
        MediaRequested?.Invoke(null);              // 右のパネルの小さなプレーヤーは止める
        ShowInViewer(item);
    }

    void ShowInViewer(MaterialItem item)
    {
        var token = ++viewerToken;
        ViewerMediaRequested?.Invoke(null);
        viewerItem = item;
        viewerMode = MaterialStore.ViewModeOf(item.Entry.Ext);
        viewerImage = null;
        viewerText = "";
        viewerPdf = null;
        viewerPage = 0;
        viewerPageCount = 0;
        viewerLoading = false;
        switch (viewerMode)
        {
            case "video":
                ViewerMediaRequested?.Invoke(item.FullPath);
                break;
            case "audio":
                viewerImage = item.Thumb;          // 波形の画像を大きく出す
                ViewerMediaRequested?.Invoke(item.FullPath);
                break;
            case "text":
                viewerLoading = true;
                _ = LoadViewerTextAsync(item, token);
                break;
            case "pdf":
                viewerImage = item.Thumb;
                viewerLoading = true;
                _ = LoadViewerPdfAsync(item, token);
                break;
            default:
                viewerImage = item.Thumb;          // 読み込みが終わるまでは、サムネイルを出しておく
                viewerLoading = true;
                _ = LoadViewerImageAsync(item, token);
                break;
        }
        RaiseViewer();
        ViewerChanged?.Invoke();
    }

    async Task LoadViewerImageAsync(MaterialItem item, int token)
    {
        var path = item.FullPath;
        int srcWidth = item.Entry.Width;
        ImageSource? image = viewerMode != "image" ? null : await Task.Run<ImageSource?>(() =>
        {
            try
            {
                var b = new BitmapImage();
                b.BeginInit();
                b.CacheOption = BitmapCacheOption.OnLoad;
                if (srcWidth <= 0 || srcWidth > 3840) b.DecodePixelWidth = 3840;   // とても大きい画像は、4K幅までに抑える
                b.UriSource = new Uri(path);
                b.EndInit();
                b.Freeze();
                return b;
            }
            catch { return null; }
        });
        // 直接読めない形式(PSD・フォント・文書など)は、Windowsが作る大きなサムネイルで代用する
        if (image == null)
        {
            try { image = await ShellThumbnail.GetAsync(path, 1024); } catch { }
        }
        if (token != viewerToken || viewerItem == null) return;
        viewerLoading = false;
        if (image != null) viewerImage = image;
        RaiseViewer();
    }

    async Task LoadViewerTextAsync(MaterialItem item, int token)
    {
        var path = item.FullPath;
        var text = await Task.Run(() =>
        {
            try
            {
                const int limit = 512 * 1024;       // 大きすぎるファイルは、先頭だけ表示する
                byte[] bytes;
                bool cut = false;
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int len = (int)Math.Min(limit, fs.Length);
                    cut = fs.Length > limit;
                    bytes = new byte[len];
                    int read = 0;
                    while (read < len)
                    {
                        int n = fs.Read(bytes, read, len - read);
                        if (n <= 0) break;
                        read += n;
                    }
                }
                string t;
                if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) t = System.Text.Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);
                else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) t = System.Text.Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
                else if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) t = System.Text.Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                else
                {
                    try { t = new System.Text.UTF8Encoding(false, true).GetString(bytes); }
                    catch
                    {
                        // UTF-8 でなければ、Shift_JIS として読む
                        try
                        {
                            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                            t = System.Text.Encoding.GetEncoding(932).GetString(bytes);
                        }
                        catch { t = System.Text.Encoding.UTF8.GetString(bytes); }
                    }
                }
                if (cut) t += "\n\n…(ファイルが大きいため、先頭の 512KB だけを表示しています)";
                return t;
            }
            catch (Exception ex) { return "(読み込めませんでした: " + ex.Message + ")"; }
        });
        if (token != viewerToken || viewerItem == null) return;
        viewerLoading = false;
        viewerText = text;
        RaiseViewer();
    }

    async Task LoadViewerPdfAsync(MaterialItem item, int token)
    {
        try
        {
            var doc = await PdfDoc.OpenAsync(item.FullPath);
            if (token != viewerToken || viewerItem == null) return;
            viewerPdf = doc;
            viewerPageCount = doc.PageCount;
            viewerPage = 0;
            var page = await doc.RenderAsync(0, 1600);
            if (token != viewerToken || viewerItem == null) return;
            viewerImage = page;
        }
        catch { }
        if (token != viewerToken || viewerItem == null) return;
        viewerLoading = false;
        RaiseViewer();
    }

    /// <summary>PDFのページを送る</summary>
    public async void StepViewerPage(int delta)
    {
        var doc = viewerPdf;
        if (doc == null || viewerPageCount <= 0) return;
        int next = Math.Clamp(viewerPage + delta, 0, viewerPageCount - 1);
        if (next == viewerPage) return;
        viewerPage = next;
        Raise(nameof(ViewerPageText));
        var token = viewerToken;
        try
        {
            var page = await doc.RenderAsync(next, 1600);
            if (token != viewerToken || viewerPage != next) return;
            viewerImage = page;
            RaiseViewer();
            ViewerChanged?.Invoke();
        }
        catch { }
    }

    /// <summary>前後の素材へ移る(端まで行ったら、反対側へ回る)</summary>
    public void StepViewer(int delta)
    {
        if (viewerItem == null || Items.Count == 0) return;
        int i = Items.IndexOf(viewerItem);
        if (i < 0) i = 0;
        var next = Items[((i + delta) % Items.Count + Items.Count) % Items.Count];
        if (ReferenceEquals(next, viewerItem)) return;
        ShowInViewer(next);
        RestoreSelection?.Invoke(new[] { next });  // 一覧の選択も合わせる
    }

    public void CloseViewer()
    {
        if (viewerItem == null) return;
        viewerToken++;
        ViewerMediaRequested?.Invoke(null);
        viewerItem = null;
        viewerImage = null;
        viewerText = "";
        viewerPdf = null;
        viewerPageCount = 0;
        viewerLoading = false;
        RaiseViewer();
        UpdatePreview();                           // 右のパネルのプレビューを元に戻す
    }

    void RaisePreview()
    {
        foreach (var n in new[] { nameof(PreviewImage), nameof(ShowImagePreview), nameof(ShowMediaPreview), nameof(ShowAudioGlyph), nameof(ShowOtherGlyph) })
            Raise(n);
    }

    // ------------------------------------------------------------------ 取り込み・保存先
    /// <summary>ファイル・フォルダーを、まとめて取り込む(エクスプローラーのドロップ・選択・ダイアログのどれからでも)</summary>
    public async Task ImportPathsAsync(IEnumerable<string> paths)
    {
        var list = paths.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        if (list.Count == 0) return;
        if (isImporting) { Message = "別の取り込みの途中です。終わるまでお待ちください"; return; }

        isImporting = true;
        Raise(nameof(IsImporting));
        var options = new ImportOptions
        {
            Mode = settings.MaterialImportMove ? ImportMode.Move : ImportMode.Copy,
            TagsFromFolders = settings.MaterialTagsFromFolders,
            Folder = selectedFolderItem is { IsReal: true } f ? f.Value! : "",
        };
        try
        {
            var result = await store.ImportAsync(list, options, m => Message = m);
            Message = $"{result.Added} 個を取り込みました"
                + (result.Restored > 0 ? $"、{result.Restored} 個をゴミ箱から戻しました(タグなどは初期の状態)" : "")
                + (result.Duplicates > 0 ? $"({result.Duplicates} 個は、すでにあるため取り込みませんでした)" : "")
                + (result.Failed > 0 ? $"(失敗 {result.Failed} 個)" : "");
        }
        catch (Exception ex) { Message = "取り込めませんでした: " + ex.Message; }
        finally
        {
            isImporting = false;
            Raise(nameof(IsImporting));
        }
    }

    /// <summary>AIで作った音を取り込む(元のファイルは残す。タグとメモ(指示の内容)を付ける)</summary>
    public async Task ImportGeneratedAsync(IReadOnlyList<string> files, List<string> tags, string memo)
    {
        if (isImporting) { Message = "別の取り込みの途中です。終わるまでお待ちください"; return; }
        isImporting = true;
        Raise(nameof(IsImporting));
        try
        {
            var options = new ImportOptions
            {
                Mode = ImportMode.Copy,
                TagsFromFolders = false,
                Folder = selectedFolderItem is { IsReal: true } f ? f.Value! : "",
                ExtraTags = tags,
            };
            var result = await store.ImportAsync(files, options, m => Message = m);
            foreach (var e in result.NewEntries) if (string.IsNullOrEmpty(e.Memo)) e.Memo = memo;
            if (result.NewEntries.Count > 0) store.Touch();
            Message = $"AIで作った素材を {result.Added} 個取り込みました" + (result.Duplicates > 0 ? $"(同じもの {result.Duplicates} 個は、取り込み済み)" : "");
            Reload();
        }
        catch (Exception ex) { Message = "取り込めませんでした: " + ex.Message; }
        finally
        {
            isImporting = false;
            Raise(nameof(IsImporting));
        }
    }

    /// <summary>保存先を、新しい場所へ引っ越す(素材も一緒に移る)</summary>
    public async Task MoveLibraryAsync(string newRoot)
    {
        if (isImporting) { Message = "取り込みの途中です。終わるまでお待ちください"; return; }
        MediaRequested?.Invoke(null);
        isImporting = true;
        Raise(nameof(IsImporting));
        try
        {
            var result = await store.MoveLibraryAsync(newRoot, m => Message = m);
            if (string.Equals(store.Root.TrimEnd('\\'), newRoot.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            {
                settings.MaterialLibraryPath = store.Root;
                settings.Save();
            }
            Message = result;
        }
        finally
        {
            isImporting = false;
            Raise(nameof(IsImporting));
            Raise(nameof(LibraryRoot));
        }
    }

    /// <summary>別の保存先(すでにあるライブラリー)に切り替える</summary>
    public void OpenLibrary(string folder)
    {
        MediaRequested?.Invoke(null);
        store.Open(folder);
        settings.MaterialLibraryPath = store.Root;
        settings.Save();
        Message = $"「{store.Root}」のライブラリーを開きました({store.All().Count} 個)";
    }
}
