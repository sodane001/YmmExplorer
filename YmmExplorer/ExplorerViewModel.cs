using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YmmExplorer;

public sealed class PlaceItem
{
    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public string Glyph { get; init; } = "\uE8B7";
}

public class ExplorerViewModel : NotifyBase, IDisposable
{
    const int MaxResults = 5000;

    readonly TagStore store = TagStore.Instance;
    readonly Stack<string> back = new();
    readonly Stack<string> forward = new();
    CancellationTokenSource? cts;
    CancellationTokenSource? debounce;
    List<FileItem> selected = new();
    static readonly string[] SortKeys = { "Name", "Modified", "Created", "Type", "Size" };
    readonly ExplorerSettings settings = ExplorerSettings.Instance;
    int sortIndex;
    bool sortDesc;
    bool foldersFirst = true;
    string sortKey => SortKeys[sortIndex];

    // ---------- 表示用プロパティ ----------
    IReadOnlyList<FileItem> items = Array.Empty<FileItem>();
    public IReadOnlyList<FileItem> Items { get => items; private set => Set(ref items, value); }

    public ObservableCollection<PlaceItem> Places { get; } = new();

    IReadOnlyList<TagInfo> allTags = Array.Empty<TagInfo>();
    public IReadOnlyList<TagInfo> AllTags { get => allTags; private set => Set(ref allTags, value); }
    public bool HasNoTags => AllTags.Count == 0;

    string currentPath = "";
    public string CurrentPath { get => currentPath; private set { if (Set(ref currentPath, value)) AddressText = value; } }

    string addressText = "";
    public string AddressText { get => addressText; set => Set(ref addressText, value); }

    string statusText = "";
    public string StatusText { get => statusText; private set => Set(ref statusText, value); }

    /// <summary>画面側(表示倍率の変更など)から、下の状態表示に文字を出す</summary>
    public void ShowStatus(string text) => StatusText = text;

    bool isBusy;
    public bool IsBusy { get => isBusy; private set => Set(ref isBusy, value); }

    // ---------- 検索条件 ----------
    string searchText = "";
    public string SearchText { get => searchText; set { if (Set(ref searchText, value)) { ScheduleRefresh(); SaveFilters(); } } }

    string extFilter = "";
    public string ExtFilter { get => extFilter; set { if (Set(ref extFilter, value)) { ScheduleRefresh(); SaveFilters(); } } }

    int datePreset;
    public int DatePreset { get => datePreset; set { if (Set(ref datePreset, value)) { ScheduleRefresh(); SaveFilters(); } } }

    bool includeSub;
    public bool IncludeSub { get => includeSub; set { if (Set(ref includeSub, value)) { ScheduleRefresh(); SaveFilters(); } } }

    string? tagFilter;
    public string? TagFilter
    {
        get => tagFilter;
        set { if (Set(ref tagFilter, value)) { Raise(nameof(HasTagFilter)); _ = RefreshAsync(); SaveFilters(); } }
    }
    public bool HasTagFilter => TagFilter != null;

    // ---------- 絞り込み(検索・拡張子・日付・サブフォルダー・タグ)を、エクスプローラーのタブごとに覚える ----------
    bool restoringFilters;

    void SaveFilters()
    {
        if (restoringFilters) return;
        var st = ExplorerSettings.Instance;
        st.ExplorerFilters ??= new Dictionary<string, ExplorerFilterState>();
        st.ExplorerFilters[activeExplorerSlot.ToString()] = new ExplorerFilterState
        {
            Search = searchText, Ext = extFilter, Date = datePreset, Sub = includeSub, Tag = tagFilter,
        };
        st.Save();
    }

    /// <summary>そのタブで前に使っていた絞り込みに戻す(フォルダーを開く前に呼ぶので、読み込みは1回で済む)</summary>
    void RestoreFilters(int slot)
    {
        var f = ExplorerSettings.Instance.ExplorerFilters is { } all && all.TryGetValue(slot.ToString(), out var x) ? x : new ExplorerFilterState();
        restoringFilters = true;
        try
        {
            searchText = f.Search ?? ""; extFilter = f.Ext ?? ""; datePreset = f.Date; includeSub = f.Sub; tagFilter = f.Tag;
            foreach (var n in new[] { nameof(SearchText), nameof(ExtFilter), nameof(DatePreset), nameof(IncludeSub), nameof(TagFilter), nameof(HasTagFilter) })
                Raise(n);
        }
        finally { restoringFilters = false; }
    }

    // ---------- 詳細ペイン ----------
    public bool HasSelection => selected.Count > 0;
    public bool HasNoSelection => selected.Count == 0;

    // 右側の詳細パネルの表示/非表示(選択の有無とは無関係に切り替え可能)。
    // プラグインを開き直しても直前の状態を引き継ぐ。
    static bool lastDetailOpen = true;
    bool isDetailOpen = lastDetailOpen;
    public bool IsDetailOpen
    {
        get => isDetailOpen;
        set { if (Set(ref isDetailOpen, value)) lastDetailOpen = value; }
    }

    string detailTitle = "";
    public string DetailTitle { get => detailTitle; private set => Set(ref detailTitle, value); }
    // 詳細パネルの項目(設定で選んだものだけが並ぶ)
    public ObservableCollection<DetailRow> DetailRows { get; } = new();

    // 設定で選べる項目。上から順に表示される (ID, 表示名, 初期状態)
    static readonly (string Id, string Label, bool Default)[] FieldDefs =
    {
        ("type", "種類", true),
        ("size", "サイズ", true),
        ("modified", "更新日時", true),
        ("created", "作成日時", false),
        ("accessed", "アクセス日時", false),
        ("ext", "拡張子", false),
        ("location", "場所", true),
        ("path", "フルパス", false),
        ("attrs", "属性", false),
        ("cloud", "クラウド", true),
        ("dimensions", "画像・動画のサイズ(解像度)", true),
        ("duration", "再生時間", true),
        ("framerate", "フレームレート", false),
        ("bitrate", "ビットレート", false),
        ("audio", "音声(サンプルレート・チャンネル)", false),
        ("pages", "PDFのページ数", true),
    };

    public IReadOnlyList<DetailFieldOption> DetailOptions { get; }

    FileItem? detailItem;
    FileMeta? detailMeta;
    string? detailMetaPath;
    int detailToken;

    FileItem? DetailItem
    {
        get => detailItem;
        set
        {
            if (ReferenceEquals(detailItem, value)) return;
            if (detailItem != null) detailItem.PropertyChanged -= OnDetailItemChanged;
            detailItem = value;
            if (detailItem != null) detailItem.PropertyChanged += OnDetailItemChanged;
            RaiseDetailThumb();
            Raise(nameof(HasDetailItem));
            Raise(nameof(DetailShortcutKeys));
        }
    }

    void OnDetailItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FileItem.Thumbnail)) RaiseDetailThumb();
    }

    bool showDetailThumbnail = true;
    public bool ShowDetailThumbnail
    {
        get => showDetailThumbnail;
        set
        {
            if (!Set(ref showDetailThumbnail, value)) return;
            settings.DetailThumbnail = value;
            settings.Save();
            RaiseDetailThumb();
        }
    }

    public ImageSource? DetailThumb => showDetailThumbnail ? detailItem?.Thumbnail : null;
    public bool HasNoDetailThumb => DetailThumb == null;

    void RaiseDetailThumb()
    {
        Raise(nameof(DetailThumb));
        Raise(nameof(HasNoDetailThumb));
    }

    bool isSettingsOpen;
    public bool IsSettingsOpen
    {
        get => isSettingsOpen;
        set { if (Set(ref isSettingsOpen, value)) Raise(nameof(IsSettingsClosed)); }
    }
    public bool IsSettingsClosed => !isSettingsOpen;
    string detailGlyph = "\uE7C3";
    public string DetailGlyph { get => detailGlyph; private set => Set(ref detailGlyph, value); }
    Brush detailBrush = Brushes.Gray;
    public Brush DetailBrush { get => detailBrush; private set => Set(ref detailBrush, value); }

    IReadOnlyList<TagInfo> selectedTags = Array.Empty<TagInfo>();
    public IReadOnlyList<TagInfo> SelectedTags { get => selectedTags; private set => Set(ref selectedTags, value); }

    string newTagText = "";
    public string NewTagText { get => newTagText; set => Set(ref newTagText, value); }

    // ---------- 表示サイズ (0=詳細 1=一覧 2=小 3=中 4=大 5=特大) ----------
    int viewMode;
    public int ViewMode
    {
        get => viewMode;
        set
        {
            value = Math.Clamp(value, 0, 5);
            if (!Set(ref viewMode, value)) return;
            // エクスプローラーのタブごとに覚えておく(ほかのタブ・ツールを触って戻っても、同じ大きさのまま)
            if (!loadingViewMode)
            {
                var st = ExplorerSettings.Instance;
                st.ExplorerViewModes ??= new Dictionary<string, int>();
                st.ExplorerViewModes[activeExplorerSlot.ToString()] = value;
                st.Save();
            }
            foreach (var n in new[] { nameof(IsDetails), nameof(IsList), nameof(IsTiles),
                                      nameof(IconSize), nameof(GlyphSize), nameof(TileWidth) })
                Raise(n);
        }
    }
    bool loadingViewMode;

    /// <summary>そのエクスプローラーのタブで、前に選んでいた表示サイズに戻す</summary>
    void RestoreViewMode(int slot)
    {
        var modes = ExplorerSettings.Instance.ExplorerViewModes;
        if (modes == null || !modes.TryGetValue(slot.ToString(), out var m)) return;
        loadingViewMode = true;
        try { ViewMode = m; }
        finally { loadingViewMode = false; }
    }

    public bool IsDetails => viewMode == 0;
    public bool IsList => viewMode == 1;
    public bool IsTiles => viewMode >= 2;
    public double IconSize => viewMode switch { 2 => 48, 3 => 96, 4 => 160, 5 => 256, _ => 48 };
    public double GlyphSize => IconSize * 0.62;
    public double TileWidth => IconSize + 28;

    // ---------- プレビュー ----------
    FileItem? previewItem;
    public FileItem? PreviewItem
    {
        get => previewItem;
        private set
        {
            if (!Set(ref previewItem, value)) return;
            PreviewMessage = "";
            foreach (var n in new[] { nameof(IsPreviewOpen), nameof(IsPreviewImage), nameof(IsPreviewMedia),
                                      nameof(IsPreviewVideo), nameof(IsPreviewAudio), nameof(IsPreviewPdf),
                                      nameof(PreviewCounter) })
                Raise(n);
            LoadPreviewImage();
            LoadPdf();
        }
    }
    public bool IsPreviewOpen => previewItem != null;
    public bool IsPreviewImage => previewItem?.IsImage == true;
    public bool IsPreviewVideo => previewItem?.IsVideo == true;
    public bool IsPreviewAudio => previewItem?.IsAudio == true;
    public bool IsPreviewMedia => IsPreviewVideo || IsPreviewAudio;
    public bool IsPreviewPdf => previewItem?.IsPdf == true;

    // ---------- PDFプレビュー ----------
    PdfDoc? pdf;
    int pdfPage, pdfPageCount, pdfToken;

    ImageSource? pdfPageImage;
    public ImageSource? PdfPageImage { get => pdfPageImage; private set => Set(ref pdfPageImage, value); }

    public string PdfPageText => pdfPageCount == 0 ? "" : $"{pdfPage + 1} / {pdfPageCount}";

    async void LoadPdf()
    {
        pdfToken++;
        pdf = null;
        pdfPage = 0;
        pdfPageCount = 0;
        PdfPageImage = null;
        Raise(nameof(PdfPageText));

        var f = previewItem;
        if (f == null || !f.IsPdf) return;
        try
        {
            var d = await PdfDoc.OpenAsync(f.FullPath);
            if (previewItem != f) return;
            pdf = d;
            pdfPageCount = d.PageCount;
            await ShowPdfPage(0);
        }
        catch
        {
            if (previewItem == f) PreviewMessage = "このPDFは開けません(パスワード付き、または破損している可能性があります)";
        }
    }

    async Task ShowPdfPage(int index)
    {
        var d = pdf;
        var f = previewItem;
        if (d == null || f == null || pdfPageCount == 0) return;
        index = Math.Clamp(index, 0, pdfPageCount - 1);
        var token = ++pdfToken;
        try
        {
            var img = await d.RenderAsync(index, 1400);
            if (token != pdfToken || previewItem != f) return;
            pdfPage = index;
            PdfPageImage = img;
            Raise(nameof(PdfPageText));
        }
        catch
        {
            if (token == pdfToken && previewItem == f) PreviewMessage = "このページを表示できません";
        }
    }

    public void StepPdfPage(int delta)
    {
        if (pdf == null) return;
        _ = ShowPdfPage(pdfPage + delta);
    }

    ImageSource? previewImage;
    public ImageSource? PreviewImage { get => previewImage; private set => Set(ref previewImage, value); }

    string previewMessage = "";
    public string PreviewMessage { get => previewMessage; private set => Set(ref previewMessage, value); }

    public string PreviewCounter
    {
        get
        {
            if (previewItem == null) return "";
            var l = PreviewList();
            var i = l.IndexOf(previewItem);
            return i < 0 ? "" : $"{i + 1} / {l.Count}";
        }
    }

    List<FileItem> PreviewList()
    {
        var l = Items.Where(i => i.CanPreview).ToList();
        if (previewItem != null && !l.Contains(previewItem))
        {
            var t = Templates.Select(x => x.Item).OfType<FileItem>().Where(i => i.CanPreview).ToList();
            if (t.Contains(previewItem)) return t;
        }
        return l;
    }

    public void ReportError(string message) => PreviewMessage = message;

    async void LoadPreviewImage()
    {
        PreviewImage = null;
        var f = previewItem;
        if (f == null || !f.IsImage) return;

        var img = await Task.Run(() =>
        {
            try
            {
                var uri = new Uri(f.FullPath);
                int w = BitmapDecoder.Create(uri, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None)
                                     .Frames[0].PixelWidth;
                var b = new BitmapImage();
                b.BeginInit();
                b.CacheOption = BitmapCacheOption.OnLoad;
                if (w > 2400) b.DecodePixelWidth = 2400;
                b.UriSource = uri;
                b.EndInit();
                b.Freeze();
                return (ImageSource?)b;
            }
            catch { return null; }
        });

        if (previewItem != f) return;
        if (img == null) PreviewMessage = "この画像はプレビューできません(形式が未対応の可能性があります)";
        else PreviewImage = img;
    }

    public void ShowPreview(FileItem f) => PreviewItem = f;
    public void ClosePreview() => PreviewItem = null;

    public void StepPreview(int delta)
    {
        if (previewItem == null) return;
        var l = PreviewList();
        if (l.Count == 0) return;
        var i = l.IndexOf(previewItem);
        if (i < 0) i = 0;
        PreviewItem = l[(i + delta + l.Count) % l.Count];
    }

    // ---------- 並べ替えヘッダー ----------
    string H(string key, string label) => sortKey == key ? label + (sortDesc ? " ▼" : " ▲") : label;
    public string HeaderName => H("Name", "名前");
    public string HeaderModified => H("Modified", "更新日時");
    public string HeaderType => H("Type", "種類");
    public string HeaderSize => H("Size", "サイズ");

    // 並び替えバー(どの表示モードでも使える)
    public int SortIndex
    {
        get => sortIndex;
        set
        {
            value = Math.Clamp(value, 0, SortKeys.Length - 1);
            if (!Set(ref sortIndex, value)) return;
            settings.SortIndex = value;
            ApplySort();
        }
    }

    public bool SortDescending
    {
        get => sortDesc;
        set
        {
            if (!Set(ref sortDesc, value)) return;
            settings.SortDescending = value;
            ApplySort();
        }
    }

    public bool FoldersFirst
    {
        get => foldersFirst;
        set
        {
            if (!Set(ref foldersFirst, value)) return;
            settings.FoldersFirst = value;
            ApplySort();
        }
    }

    // ---------- コマンド ----------
    public ICommand BackCommand { get; }
    public ICommand ForwardCommand { get; }
    public ICommand UpCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand GoCommand { get; }
    public ICommand NavigateCommand { get; }
    public ICommand SortCommand { get; }
    public ICommand FilterTagCommand { get; }
    public ICommand ClearTagFilterCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand AddTagCommand { get; }
    public ICommand RemoveTagCommand { get; }
    public ICommand ShowInExplorerCommand { get; }
    public ICommand CopyPathCommand { get; }
    public ICommand ClosePreviewCommand { get; }
    public ICommand PrevPreviewCommand { get; }
    public ICommand NextPreviewCommand { get; }
    public ICommand OpenExternalCommand { get; }
    public ICommand PreviewSelectedCommand { get; }
    public ICommand OpenSelectedCommand { get; }
    public ICommand AddToTemplateCommand { get; }
    public ICommand AddToSlotCommand { get; }
    public ICommand ToggleSettingsPageCommand { get; }
    public ICommand IncreaseTabsCommand { get; }
    public ICommand DecreaseTabsCommand { get; }
    public ICommand ResetTabNamesCommand { get; }
    public ICommand RemovePresetCommand { get; }
    public ICommand ShowAllTabsCommand { get; }
    public ICommand OpenCloudCommand { get; }
    public ICommand RefreshCloudCommand { get; }
    public ICommand DownloadCloudCommand { get; }
    public ICommand DiagnoseClipboardCommand { get; }
    public ICommand CopyDiagnosisCommand { get; }
    public ICommand CreatePresetFromSelectionCommand { get; }
    public ICommand PresetPreviewThumbnailCommand { get; }
    public ICommand AddShapeFromSelectionCommand { get; }
    public ICommand AttachShapeTemplateCommand { get; }
    public ICommand ShapePreviewThumbnailCommand { get; }
    public ICommand DiagnoseYmmCommand { get; }
    public ICommand AddToMaterialsCommand { get; }
    public ICommand DeleteShortcutCommand { get; }
    public ICommand RefreshPluginsCommand { get; }
    public ICommand TogglePluginCommand { get; }
    public ICommand DeletePluginCommand { get; }
    public ICommand LoadYmmSettingsCommand { get; }
    public ICommand RevertYmmSettingsCommand { get; }
    public ICommand AutoRegisterCommand { get; }
    public ICommand AutoRegisterShapesCommand { get; }
    public ICommand StartShapeCaptureCommand { get; }
    public ICommand CancelShapeCaptureCommand { get; }
    public ICommand ScanShapesCommand { get; }
    public ICommand HideShapesCommand { get; }
    public ICommand RemoveShapesCommand { get; }
    public ICommand RestoreRemovedShapesCommand { get; }
    public ICommand MoveShapesCommand { get; }
    public ICommand PasteShapeThumbnailCommand { get; }
    public ICommand ClearShapeThumbnailCommand { get; }
    public ICommand MoveTemplatesCommand { get; }
    public ICommand MovePresetsCommand { get; }
    public ICommand PasteTemplateThumbnailCommand { get; }
    public ICommand ClearTemplateThumbnailCommand { get; }
    public ICommand CreatePreviewCommand { get; }
    public ICommand ClearPreviewCommand { get; }
    public ICommand PasteThumbnailCommand { get; }
    public ICommand ClearThumbnailCommand { get; }
    public ICommand UseMediaThumbnailCommand { get; }
    public ICommand RemoveTemplateCommand { get; }
    public ICommand OpenTemplateCommand { get; }
    public ICommand RevealTemplateCommand { get; }
    public ICommand TemplatePropertiesCommand { get; }
    public ICommand ShowPropertiesCommand { get; }
    public ICommand ToggleSettingsCommand { get; }
    public ICommand ResetDetailFieldsCommand { get; }
    public ICommand PrevPageCommand { get; }
    public ICommand NextPageCommand { get; }

    public ExplorerViewModel()
    {
        BackCommand = new RelayCommand(_ => GoBack(), _ => back.Count > 0);
        ForwardCommand = new RelayCommand(_ => GoForward(), _ => forward.Count > 0);
        UpCommand = new RelayCommand(_ => GoUp(), _ => ParentOf(CurrentPath) != null);
        RefreshCommand = new RelayCommand(_ => _ = RefreshAsync());
        GoCommand = new RelayCommand(_ => Navigate(AddressText.Trim().Trim('"')));
        NavigateCommand = new RelayCommand(p => { if (p is string s) Navigate(s); });
        SortCommand = new RelayCommand(p => { if (p is string s) ToggleSort(s); });
        FilterTagCommand = new RelayCommand(p =>
        {
            if (p is not string s) return;
            TagFilter = string.Equals(TagFilter, s, StringComparison.OrdinalIgnoreCase) ? null : s;
        });
        ClearTagFilterCommand = new RelayCommand(_ => TagFilter = null);
        ClearFiltersCommand = new RelayCommand(_ =>
        {
            searchText = ""; extFilter = ""; datePreset = 0; includeSub = false; tagFilter = null;
            SaveFilters();
            Raise(nameof(SearchText)); Raise(nameof(ExtFilter)); Raise(nameof(DatePreset));
            Raise(nameof(IncludeSub)); Raise(nameof(TagFilter)); Raise(nameof(HasTagFilter));
            _ = RefreshAsync();
        });
        AddTagCommand = new RelayCommand(p => AddTag(p as string));
        RemoveTagCommand = new RelayCommand(p => { if (p is string s) RemoveTag(s); });
        ShowInExplorerCommand = new RelayCommand(_ => ShowInExplorer(), _ => selected.Count > 0);
        CopyPathCommand = new RelayCommand(_ => CopyPath(), _ => selected.Count > 0);
        ClosePreviewCommand = new RelayCommand(_ => ClosePreview());
        PrevPreviewCommand = new RelayCommand(_ => StepPreview(-1));
        NextPreviewCommand = new RelayCommand(_ => StepPreview(1));
        OpenExternalCommand = new RelayCommand(_ => OpenExternal(PreviewItem ?? selected.FirstOrDefault()));
        PreviewSelectedCommand = new RelayCommand(_ =>
        {
            var f = selected.FirstOrDefault(x => x.CanPreview);
            if (f != null) ShowPreview(f);
        }, _ => selected.Any(x => x.CanPreview));
        OpenSelectedCommand = new RelayCommand(_ => Open(selected.FirstOrDefault()), _ => selected.Count > 0);
        AddToTemplateCommand = new RelayCommand(
            _ => AddTemplatePaths(selected.Select(x => x.FullPath)),
            _ => selected.Count > 0 && selectedKind == TabKind.Template);
        AddToSlotCommand = new RelayCommand(
            p =>
            {
                if (p is not int slot) return;
                var n = templateStore.Add(slot, selected.Select(x => x.FullPath));
                var name = TabGroups.Template.Tabs.FirstOrDefault(t => t.Slot == slot)?.DisplayName ?? $"テンプレート{slot}";
                StatusText = n > 0 ? $"「{name}」に {n} 件追加しました" : $"「{name}」には登録済みです";
            },
            _ => selected.Count > 0);
        ToggleSettingsPageCommand = new RelayCommand(_ => IsSettingsPageOpen = !IsSettingsPageOpen);
        // 設定画面のタブ数・名前(パラメーターに TabKind の名前を渡す)
        IncreaseTabsCommand = new RelayCommand(
            p =>
            {
                if (GroupOf(p) is not { } g) return;
                int before = g.Count;
                g.Count++;
                // 図形の新しいタブ: 以前の版が自動で入れた図形が残っていたら消して、空の状態から始める
                if (ReferenceEquals(g, TabGroups.Shape))
                    for (int slot = before + 1; slot <= g.Count; slot++) ShapeStore.Instance.PurgeAutoLeftovers(slot);
            },
            p => GroupOf(p) is { } g && g.Count < TabGroup.MaxTabs);
        DecreaseTabsCommand = new RelayCommand(
            p => { if (GroupOf(p) is { } g) g.Count--; },
            p => GroupOf(p) is { } g && g.Count > 1);
        ResetTabNamesCommand = new RelayCommand(p => GroupOf(p)?.ResetNames());
        ShowAllTabsCommand = new RelayCommand(p => { if (GroupOf(p) is { } g) WindowTabs.ShowAll(g); });
        OpenCloudCommand = new RelayCommand(p =>
        {
            if (p is not string path || !Directory.Exists(path)) return;
            var ex = WindowTabs.VisibleTabs(TabGroups.Explorer).FirstOrDefault(t => t.Slot == activeExplorerSlot)
                     ?? WindowTabs.VisibleTabs(TabGroups.Explorer).FirstOrDefault();
            if (ex == null) { StatusText = "エクスプローラーのタブが非表示のため開けません"; return; }
            SelectTab(TabKind.Explorer, ex.Slot);
            Navigate(path);
        });
        RefreshCloudCommand = new RelayCommand(_ => RefreshCloud());
        DownloadCloudCommand = new RelayCommand(_ => _ = DownloadCloudAsync(), _ => HasCloudOnlySelected && !isBusy);
        RemovePresetCommand = new RelayCommand(_ => RemoveSelectedPresets(), _ => selectedPresets.Count > 0);
        DiagnoseClipboardCommand = new RelayCommand(_ => ClipboardDiagText = ClipboardRaw.Describe());
        CopyDiagnosisCommand = new RelayCommand(_ =>
        {
            try { System.Windows.Clipboard.SetText(ClipboardDiagText); }
            catch { ClipboardDiagText += "\n(コピーに失敗しました。文字を選んで Ctrl+C してください)"; }
        }, _ => ClipboardDiagText.Length > 0);
        CreatePresetFromSelectionCommand = new RelayCommand(_ => CreatePresetFromSelection());
        PresetPreviewThumbnailCommand = new RelayCommand(_ => SetPresetThumbnailFromPreview(), _ => selectedPreset != null);
        AddShapeFromSelectionCommand = new RelayCommand(_ => AddShapeFromSelection());
        AttachShapeTemplateCommand = new RelayCommand(_ => AttachShapeTemplateFromSelection(), _ => selectedShape != null);
        ShapePreviewThumbnailCommand = new RelayCommand(_ => SetShapeThumbnailFromPreview(), _ => selectedShape != null);
        DiagnoseYmmCommand = new RelayCommand(_ => YmmDiagText = Ymm.Diagnose());
        AddToMaterialsCommand = new RelayCommand(_ => _ = AddSelectionToMaterialsAsync(), _ => selected.Count > 0);
        DeleteShortcutCommand = new RelayCommand(p =>
        {
            if (p is ShortcutRow row) { ShortcutStore.Instance.Set(row.Binding.Kind, row.Binding.TargetId, row.Binding.Name, ""); ReloadShortcutRows(); }
        });
        RefreshPluginsCommand = new RelayCommand(_ => RefreshPlugins());
        TogglePluginCommand = new RelayCommand(p => { if (p is PluginInfo pi) PluginMessage = PluginManager.Toggle(pi); });
        DeletePluginCommand = new RelayCommand(p => DeletePlugin(p as PluginInfo));
        LoadYmmSettingsCommand = new RelayCommand(_ => LoadYmmSettings());
        RevertYmmSettingsCommand = new RelayCommand(_ => RevertYmmSettings());
        AutoRegisterCommand = new RelayCommand(p => _ = AutoRegisterAsync(p as TabEntry ?? (p as TabToggle)?.Entry));
        AutoRegisterShapesCommand = new RelayCommand(async _ =>
        {
            if (shapeSlot < 1) { ShapeMessage = "⚠ このタブを開いた状態で押してください(タブが選ばれていません)"; return; }
            ShapeMessage = await AutoRegisterShapesAsync(shapeSlot);
        });
        StartShapeCaptureCommand = new RelayCommand(_ => StartShapeCapture(selectedShapes), _ => selectedShapes.Count > 0);
        CancelShapeCaptureCommand = new RelayCommand(_ => EndShapeCapture("初期値の配置データの登録を中止しました"), _ => IsShapeCapturing);
        shapeCaptureTimer.Tick += (_, _) => OnShapeCaptureTick();
        ScanShapesCommand = new RelayCommand(_ => _ = EnsureShapesAsync(true), _ => !isShapeScanning);
        HideShapesCommand = new RelayCommand(p => HideShapes(p as string == "hide"), _ => selectedShapes.Count > 0);
        RemoveShapesCommand = new RelayCommand(_ => RemoveSelectedShapes(), _ => selectedShapes.Count > 0);
        RestoreRemovedShapesCommand = new RelayCommand(async _ =>
        {
            if (shapeSlot < 1) return;
            var n = shapeStore.ForgetRemoved(shapeSlot);
            if (n == 0) { ShapeMessage = "削除した自動収録の図形はありません"; return; }
            ShapeMessage = $"削除した図形 {n} 個を戻しています…";
            ShapeMessage = $"削除した図形 {n} 個を戻しました。" + await AutoRegisterShapesAsync(shapeSlot);
        });
        MoveShapesCommand = new RelayCommand(p => MoveShapes(p as string ?? ""), _ => selectedShapes.Count > 0);
        PasteShapeThumbnailCommand = new RelayCommand(_ => SetShapeThumbnailFromClipboard(), _ => selectedShape != null);
        ClearShapeThumbnailCommand = new RelayCommand(
            _ => SetShapeThumbnail(selectedShape, null, "サムネイルを削除しました"),
            _ => selectedShape != null && selectedShape.HasCustomThumb);
        MoveTemplatesCommand = new RelayCommand(p => MoveTemplates(p as string ?? ""), _ => selectedTemplates.Count > 0);
        MovePresetsCommand = new RelayCommand(p => MovePresets(p as string ?? ""), _ => selectedPresets.Count > 0);
        PasteTemplateThumbnailCommand = new RelayCommand(_ => SetTemplateThumbnailFromClipboard(), _ => selectedTemplate != null);
        ClearTemplateThumbnailCommand = new RelayCommand(
            _ => SetTemplateThumbnail(selectedTemplate, null, "ファイル自身のサムネイルに戻しました"),
            _ => selectedTemplate != null && selectedTemplate.HasCustomThumb);
        CreatePreviewCommand = new RelayCommand(_ => _ = CreatePreviewAsync(selectedPreset, false),
                                                _ => selectedPreset != null && !isPresetEncoding);
        ClearPreviewCommand = new RelayCommand(_ => ClearPreview(selectedPreset),
                                               _ => selectedPreset != null && selectedPreset.HasPreview && !isPresetEncoding);
        PasteThumbnailCommand = new RelayCommand(_ => SetThumbnailFromClipboard(), _ => selectedPreset != null);
        ClearThumbnailCommand = new RelayCommand(_ => SetThumbnail(selectedPreset, null, "サムネイルを削除しました"), _ => selectedPreset != null);
        UseMediaThumbnailCommand = new RelayCommand(p => { if (p is string path) _ = UseMediaAsThumbnailAsync(path); },
                                                   _ => selectedPreset != null);
        RemoveTemplateCommand = new RelayCommand(_ => RemoveSelectedTemplates(), _ => selectedTemplates.Count > 0);
        OpenTemplateCommand = new RelayCommand(_ => OpenTemplate(selectedTemplates.FirstOrDefault()), _ => selectedTemplates.Count > 0);
        RevealTemplateCommand = new RelayCommand(_ => RevealTemplate(selectedTemplates.FirstOrDefault()), _ => selectedTemplates.Count > 0);
        TemplatePropertiesCommand = new RelayCommand(
            _ => { var t = selectedTemplates.FirstOrDefault(); if (t != null && !ShellActions.ShowProperties(t.Path)) TemplateMessage = "プロパティを開けませんでした"; },
            _ => selectedTemplates.Count > 0);
        ShowPropertiesCommand = new RelayCommand(_ => ShowProperties(), _ => !string.IsNullOrEmpty(CurrentPath));
        ToggleSettingsCommand = new RelayCommand(_ => IsSettingsOpen = !IsSettingsOpen);
        ResetDetailFieldsCommand = new RelayCommand(_ =>
        {
            foreach (var o in DetailOptions) o.IsChecked = o.Default;
            ShowDetailThumbnail = true;
        });
        PrevPageCommand = new RelayCommand(_ => StepPdfPage(-1), _ => pdfPage > 0);
        NextPageCommand = new RelayCommand(_ => StepPdfPage(1), _ => pdfPage < pdfPageCount - 1);

        // 保存済みの設定を反映
        sortIndex = Math.Clamp(settings.SortIndex, 0, SortKeys.Length - 1);
        sortDesc = settings.SortDescending;
        foldersFirst = settings.FoldersFirst;
        showDetailThumbnail = settings.DetailThumbnail;
        var enabled = settings.DetailFields != null ? new HashSet<string>(settings.DetailFields) : null;
        var options = new List<DetailFieldOption>();
        foreach (var d in FieldDefs)
        {
            var o = new DetailFieldOption(d.Id, d.Label, d.Default, enabled?.Contains(d.Id) ?? d.Default);
            o.Changed += OnDetailOptionChanged;
            options.Add(o);
        }
        DetailOptions = options;

        foreach (var c in CloudDetector.Detect()) CloudLocations.Add(c);
        BuildPlaces();
        store.Changed += OnStoreChanged;
        templateStore.Changed += OnTemplateStoreChanged;
        presetStore.Changed += OnPresetStoreChanged;
        ShortcutHost.Attach(this);
        ShortcutStore.Instance.Changed += OnShortcutsChanged;
        if (!pendingDeletesDone) { pendingDeletesDone = true; PluginManager.ProcessPendingDeletes(); }
        shapeStore.Changed += OnShapeStoreChanged;
        ShapeVisibilityVm.Changed += OnShapeVisibilityChanged;
        VideoFx = new EffectLibraryVm(this, TabKind.VideoEffect);
        AudioFx = new EffectLibraryVm(this, TabKind.AudioEffect);
        TextLib = new EffectLibraryVm(this, TabKind.Text);
        Materials = new MaterialLibraryVm();
        EffectShapes = new EffectShapeBrowserVm(PlaceShapeByKeyAsync, PlacePvRecipeAsync, settings.IsShapeShown);
        FxBrowser = new FxBrowserVm(settings);
        TextBrowser = new TextBrowserVm(settings);
        _ = FxCatalog.EnsureAsync(settings.FxSampleImage ?? "");     // 初回だけ、サンプル画像にエフェクトをかけたサムネイルを作る
        _ = EffectShapeCatalog.EnsureAsync();       // 初回だけ、全図形のサムネイルを作ってプラグインフォルダーに保存する
        ItemEditor = new ItemEditorVm(() => ShowItemView);
        TextSplit.Done += text => { StatusText = text; ItemEditor.Rebuild(); };
        WindowTabs = new WindowTabs(WindowId);
        WindowTabs.Changed += OnTabsChanged;
        foreach (var g in TabGroups.All) g.TabsChanged += OnTabsChanged;
        BuildTabs();
        RebuildTabToggles();

        // 最初に開くタブ: このウィンドウで表示しているタブのうち、いちばん左のもの
        if (WindowTabs.FirstVisible() is { } first)
        {
            selectedKind = first.Kind;
            selectedSlot = first.Slot;
            activeExplorerSlot = first.Kind == TabKind.Explorer
                ? first.Slot
                : WindowTabs.VisibleTabs(TabGroups.Explorer).FirstOrDefault()?.Slot ?? 1;
        }
        // YMM4で別のツールを開いて戻ると、この画面が作り直されることがある。その時は、前に開いていたタブへ戻す
        saved = SavedWindows.TryGetValue(WindowId, out var prev) ? prev : null;
        if (saved == null && ExplorerSettings.Instance.WindowLastTab?.TryGetValue(WindowId.ToString(), out var last) == true)
        {
            // YMM4を起動し直した時: 最後に開いていたタブだけ戻す
            var parts = last.Split(':');
            if (parts.Length == 2 && Enum.TryParse<TabKind>(parts[0], out var lk) && int.TryParse(parts[1], out var ls))
                saved = new SavedWindow(lk, ls, lk == TabKind.Explorer ? ls : activeExplorerSlot, new Dictionary<int, ExplorerTabState>());
        }
        // 前回「図形」タブを開いていた時は、それに代わる「演出図形」タブを開く
        if (saved?.Kind == TabKind.Shape) saved = saved with { Kind = TabKind.EffectShape, Slot = 1 };
        // 映像・音声エフェクトのタブは「エフェクト」タブ1つにまとめた
        if (saved?.Kind is TabKind.VideoEffect or TabKind.AudioEffect) saved = saved with { Kind = TabKind.VideoEffect, Slot = 1 };
        if (saved != null && WindowTabs.VisibleTabs(TabGroups.Of(saved.Kind)).Any(t => t.Slot == saved.Slot))
        {
            selectedKind = saved.Kind;
            selectedSlot = saved.Slot;
            if (WindowTabs.VisibleTabs(TabGroups.Explorer).Any(t => t.Slot == saved.ExplorerSlot)) activeExplorerSlot = saved.ExplorerSlot;
            foreach (var kv in saved.ExplorerStates) explorerStates[kv.Key] = kv.Value;
        }
        ReloadTemplates();
        ReloadPresets();
        ReloadShapes();
        if (selectedKind == TabKind.Shape) _ = EnsureShapesAsync(false);
        VideoFx.Reload();
        AudioFx.Reload();
        TextLib.Reload();
        ReloadTags();

        // エクスプローラーは、前に開いていたフォルダー(戻る・進むの履歴も)と、アイコンの大きさへ戻す
        RestoreViewMode(activeExplorerSlot);
        RestoreFilters(activeExplorerSlot);
        if (explorerStates.TryGetValue(activeExplorerSlot, out var st) && Directory.Exists(st.Path))
        {
            foreach (var x in st.Back.Reverse()) back.Push(x);
            foreach (var x in st.Forward.Reverse()) forward.Push(x);
            Navigate(st.Path, false);
        }
        else Navigate(StartPathFor(activeExplorerSlot), false);

        // エクスプローラー以外のタブへ戻した時は、そのタブの中身の読み込みを、タブを選んだ時と同じように行う
        if (saved != null && selectedKind != TabKind.Explorer)
        {
            var (k, sl) = (selectedKind, selectedSlot);
            selectedSlot = 0;                       // 「同じタブ」と見なされないように
            SelectTab(k, sl, false);
        }
        restoring = false;
        RememberWindow();
    }

    // ---------- ウィンドウの状態の記憶(別のツールを開いて戻った時に、開いていたタブ・フォルダーへ戻すため) ----------
    sealed record SavedWindow(TabKind Kind, int Slot, int ExplorerSlot, Dictionary<int, ExplorerTabState> ExplorerStates);

    /// <summary>ウィンドウの番号 → 最後の状態(YMM4を閉じるまで覚えておく)</summary>
    static readonly Dictionary<int, SavedWindow> SavedWindows = new();
    SavedWindow? saved;
    bool restoring = true;

    /// <summary>いまの状態(開いているタブ・エクスプローラーのフォルダーと履歴)を覚える</summary>
    void RememberWindow()
    {
        if (restoring) return;
        try
        {
            var states = new Dictionary<int, ExplorerTabState>(explorerStates);
            if (!string.IsNullOrEmpty(CurrentPath))
                states[activeExplorerSlot] = new ExplorerTabState(CurrentPath, back.ToArray(), forward.ToArray());
            SavedWindows[WindowId] = new SavedWindow(selectedKind, selectedSlot, activeExplorerSlot, states);

            var st = ExplorerSettings.Instance;
            st.WindowLastTab ??= new Dictionary<string, string>();
            var key = WindowId.ToString();
            var value = $"{selectedKind}:{selectedSlot}";
            if (!st.WindowLastTab.TryGetValue(key, out var old) || old != value)
            {
                st.WindowLastTab[key] = value;
                st.Save();
            }
        }
        catch { }
    }

    public void Dispose()
    {
        store.Changed -= OnStoreChanged;
        templateStore.Changed -= OnTemplateStoreChanged;
        presetStore.Changed -= OnPresetStoreChanged;
        ShortcutHost.Detach(this);
        ShortcutStore.Instance.Changed -= OnShortcutsChanged;
        ymmSaver.Flush();
        Materials.Dispose();
        ItemEditor.Dispose();
        shapeStore.Changed -= OnShapeStoreChanged;
        ShapeVisibilityVm.Changed -= OnShapeVisibilityChanged;
        shapeCaptureTimer.Stop();
        VideoFx.Dispose();
        AudioFx.Dispose();
        TextLib.Dispose();
        EffectShapes.Dispose();
        FxBrowser.Dispose();
        TextBrowser.Dispose();
        foreach (var g in TabGroups.All) g.TabsChanged -= OnTabsChanged;
        WindowTabs.Changed -= OnTabsChanged;
        foreach (var list in TabToggles.Values) foreach (var t in list) t.Detach();
        foreach (var t in Tabs) t.Detach();
        DetailItem = null;
    }

    // ---------- 場所 ----------
    void BuildPlaces()
    {
        void Add(string name, string? path, string glyph)
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                Places.Add(new PlaceItem { Name = name, Path = path, Glyph = glyph });
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        Add("ホーム", home, "\uE80F");
        Add("デスクトップ", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "\uE7F4");
        Add("ダウンロード", System.IO.Path.Combine(home, "Downloads"), "\uE896");
        Add("ドキュメント", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\uE8A5");
        Add("ピクチャ", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "\uE91B");
        Add("ビデオ", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "\uE714");
        Add("ミュージック", Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "\uE8D6");

        foreach (var d in DriveInfo.GetDrives())
        {
            try
            {
                if (!d.IsReady) continue;
                var label = string.IsNullOrWhiteSpace(d.VolumeLabel) ? "ローカルディスク" : d.VolumeLabel;
                Add($"{label} ({d.Name.TrimEnd('\\')})", d.Name, "\uEDA2");
            }
            catch { }
        }

        // クラウド(公式アプリの同期フォルダー)
        foreach (var c in CloudLocations.Where(c => c.Found)) Add(c.Name, c.Path, "\uE753");
    }

    // ---------- クラウド連携 ----------
    public ObservableCollection<CloudLocation> CloudLocations { get; } = new();

    void RefreshCloud()
    {
        CloudLocations.Clear();
        foreach (var c in CloudDetector.Detect()) CloudLocations.Add(c);
        Places.Clear();
        BuildPlaces();
    }

    /// <summary>クラウドのみのファイルも、サムネイルや詳細情報を取得する(ダウンロードが発生する)</summary>
    public bool CloudThumbnails
    {
        get => settings.CloudThumbnails;
        set
        {
            if (settings.CloudThumbnails == value) return;
            settings.CloudThumbnails = value;
            settings.Save();
            Raise(nameof(CloudThumbnails));
        }
    }

    public bool HasCloudOnlySelected => selected.Any(x => x.IsCloudOnly);

    /// <summary>選んだクラウドのみのファイルを、パソコンにダウンロードする(最後まで読み込むことで、各サービスのアプリに取得させる)</summary>
    async Task DownloadCloudAsync()
    {
        var targets = selected.Where(x => x.IsCloudOnly).ToList();
        if (targets.Count == 0) return;
        IsBusy = true;
        int done = 0, failed = 0;
        try
        {
            foreach (var f in targets)
            {
                StatusText = $"クラウドからダウンロード中… ({done + failed + 1}/{targets.Count}) {f.Name}";
                try
                {
                    await Task.Run(() =>
                    {
                        using var fs = new FileStream(f.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1 << 20, FileOptions.SequentialScan);
                        fs.CopyTo(Stream.Null, 1 << 20);
                    });
                    done++;
                }
                catch { failed++; }
            }
        }
        finally { IsBusy = false; }
        StatusText = failed == 0 ? $"{done} 件をダウンロードしました" : $"{done} 件をダウンロードしました({failed} 件は失敗)";
        await RefreshAsync();
    }

    // ---------- ナビゲーション ----------
    static string? ParentOf(string path)
        => string.IsNullOrEmpty(path) ? null : Directory.GetParent(path)?.FullName;

    void Navigate(string path, bool record = true)
    {
        if (!Directory.Exists(path)) { StatusText = "フォルダーが見つかりません: " + path; return; }
        if (record && !string.IsNullOrEmpty(CurrentPath))
        {
            back.Push(CurrentPath);
            forward.Clear();
        }
        tagFilter = null;
        Raise(nameof(TagFilter));
        Raise(nameof(HasTagFilter));
        CurrentPath = path;
        _ = RefreshAsync();
        RememberWindow();
    }

    void GoBack()
    {
        if (back.Count == 0) return;
        forward.Push(CurrentPath);
        Navigate(back.Pop(), false);
    }

    void GoForward()
    {
        if (forward.Count == 0) return;
        back.Push(CurrentPath);
        Navigate(forward.Pop(), false);
    }

    public void GoUp()
    {
        var p = ParentOf(CurrentPath);
        if (p != null) Navigate(p);
    }

    // ---------- タブ(機能切り替え) ----------
    // 並び: エクスプローラー1〜n / テンプレート1〜n / プリセット1〜n(数は設定で変更)
    public TabGroup ExplorerGroup => TabGroups.Explorer;
    public TabGroup TemplateGroup => TabGroups.Template;
    public TabGroup PresetGroup => TabGroups.Preset;
    static TabGroup? GroupOf(object? p)
        => p is string s && Enum.TryParse<TabKind>(s, out var k) ? TabGroups.Of(k) : null;

    public ObservableCollection<TabVm> Tabs { get; } = new();

    TabKind selectedKind = TabKind.Explorer;
    int selectedSlot = 1;
    public TabKind SelectedKind => selectedKind;
    public int SelectedSlot => selectedSlot;

    bool isSettingsPageOpen;
    public bool IsSettingsPageOpen
    {
        get => isSettingsPageOpen;
        set
        {
            if (!Set(ref isSettingsPageOpen, value)) return;
            ClosePreview();
            RefreshTabState();
        }
    }

    public bool IsExplorerTab => selectedKind == TabKind.Explorer;
    public bool IsTemplateTab => selectedKind == TabKind.Template;
    public bool IsPresetTab => selectedKind == TabKind.Preset;
    public bool IsShapeTab => selectedKind == TabKind.Shape;
    public bool IsVideoEffectTab => selectedKind == TabKind.VideoEffect;
    public bool IsAudioEffectTab => selectedKind == TabKind.AudioEffect;
    public bool ShowVideoEffectView => IsVideoEffectTab && !isSettingsPageOpen;
    public bool ShowAudioEffectView => IsAudioEffectTab && !isSettingsPageOpen;
    /// <summary>素材データベース(Eagle風の素材管理)</summary>
    public MaterialLibraryVm Materials { get; }
    public bool IsMaterialTab => selectedKind == TabKind.Material;
    public bool ShowMaterialView => IsMaterialTab && !isSettingsPageOpen;
    public TabGroup MaterialGroup => TabGroups.Material;
    public ItemEditorVm ItemEditor { get; }
    public TextSplitVm TextSplit { get; } = new();
    public bool IsItemTab => selectedKind == TabKind.Item;
    public bool ShowItemView => IsItemTab && !isSettingsPageOpen;
    public TabGroup ItemGroup => TabGroups.Item;
    public bool IsYmmSettingsTab => selectedKind == TabKind.YmmSettings;
    public bool ShowYmmSettingsView => IsYmmSettingsTab && !isSettingsPageOpen;
    public TabGroup YmmSettingsGroup => TabGroups.YmmSettings;
    public bool IsTextTab => selectedKind == TabKind.Text;
    public bool ShowTextView => IsTextTab && !isSettingsPageOpen;

    /// <summary>テキストプリセットのタブの中身</summary>
    public EffectLibraryVm TextLib { get; }
    public TabGroup TextGroup => TabGroups.Text;

    /// <summary>映像エフェクト・音声エフェクトのタブの中身</summary>
    public EffectLibraryVm VideoFx { get; }
    public EffectLibraryVm AudioFx { get; }
    public TabGroup VideoEffectGroup => TabGroups.VideoEffect;
    public TabGroup AudioEffectGroup => TabGroups.AudioEffect;
    public bool ShowShapeView => IsShapeTab && !isSettingsPageOpen;
    public TabGroup ShapeGroup => TabGroups.Shape;
    /// <summary>演出図形のタブ(分類で絞って配置するだけの、読み取り専用のタブ)</summary>
    public EffectShapeBrowserVm EffectShapes { get; }
    /// <summary>エフェクトタブ(同梱の演出エフェクト)</summary>
    public FxBrowserVm FxBrowser { get; }
    /// <summary>テキストタブ(標準のテンプレート)</summary>
    public TextBrowserVm TextBrowser { get; }
    public bool IsEffectShapeTab => selectedKind == TabKind.EffectShape;
    public bool ShowEffectShapeView => IsEffectShapeTab && !isSettingsPageOpen;
    public bool ShowExplorerView => IsExplorerTab && !isSettingsPageOpen;
    public bool ShowTemplateView => IsTemplateTab && !isSettingsPageOpen;
    public bool ShowPresetView => IsPresetTab && !isSettingsPageOpen;
    public bool ShowSettingsPage => isSettingsPageOpen;

    public void SelectTab(TabKind kind, int slot) => SelectTab(kind, slot, true);

    /// <summary>このウィンドウにアイテムタブが出ているか</summary>
    public bool HasItemTab => WindowTabs.VisibleTabs(TabGroups.Of(TabKind.Item)).Count > 0;

    /// <summary>このウィンドウに出ているアイテムタブを開く(無ければ false)</summary>
    public bool OpenItemTab()
    {
        var tab = WindowTabs.VisibleTabs(TabGroups.Of(TabKind.Item)).FirstOrDefault();
        if (tab == null) return false;
        SelectTab(TabKind.Item, tab.Slot);
        ItemEditor.Rebuild();
        return true;
    }

    // 「直前に開いていたタブへ戻る」用
    TabKind? previousKind;
    int previousSlot;

    void SelectTab(TabKind kind, int slot, bool closeSettings)
    {
        if (kind == TabKind.Shape) { kind = TabKind.EffectShape; slot = 1; }     // 「図形」タブは「演出図形」タブに統合した
        if (kind is TabKind.VideoEffect or TabKind.AudioEffect) { kind = TabKind.VideoEffect; slot = 1; }     // エフェクトのタブは1つにまとめた
        slot = Math.Clamp(slot, 1, TabGroups.Of(kind).Count);
        var wasSettings = isSettingsPageOpen;
        if (closeSettings) isSettingsPageOpen = false;     // タブを選んだら設定画面は閉じる
        if (kind == selectedKind && slot == selectedSlot)
        {
            if (wasSettings) RefreshTabState();
            return;
        }

        previousKind = selectedKind;
        previousSlot = selectedSlot;
        selectedKind = kind;
        selectedSlot = slot;
        ClosePreview();
        Application.Current?.Dispatcher.BeginInvoke(new Action(RememberWindow));
        switch (kind)
        {
            case TabKind.Explorer:
                if (slot != activeExplorerSlot)
                {
                    SaveExplorerState();
                    LoadExplorerState(slot);
                }
                break;
            case TabKind.Template: ReloadTemplates(); break;
            case TabKind.Preset: ReloadPresets(); break;
            case TabKind.Shape:
                ReloadShapes();
                _ = EnsureShapesAsync(false);
                break;
            case TabKind.EffectShape: EffectShapes.Refresh(); break;
            case TabKind.VideoEffect: FxBrowser.Refresh(); break;
            case TabKind.AudioEffect: AudioFx.Reload(); break;
            case TabKind.Text: TextBrowser.Refresh(); break;
            case TabKind.YmmSettings:
                if (YmmSettingGroups.Count == 0) LoadYmmSettings();
                break;
            case TabKind.Material: Materials.Reload(); break;
            case TabKind.Item: ItemEditor.Rebuild(); break;
            case TabKind.Shortcut: ReloadShortcutRows(); ReloadRemapRows(); ReloadActionShortcuts(); break;
        }
        RefreshTabState();
    }

    void RefreshTabState()
    {
        foreach (var t in Tabs) t.RaiseSelected();
        Raise(nameof(SelectedKind));
        Raise(nameof(SelectedSlot));
        Raise(nameof(IsExplorerTab));
        Raise(nameof(IsTemplateTab));
        Raise(nameof(IsPresetTab));
        Raise(nameof(ShowExplorerView));
        Raise(nameof(ShowTemplateView));
        Raise(nameof(ShowPresetView));
        Raise(nameof(IsShapeTab));
        Raise(nameof(ShowShapeView));
        Raise(nameof(IsEffectShapeTab));
        Raise(nameof(ShowEffectShapeView));
        Raise(nameof(IsVideoEffectTab));
        Raise(nameof(IsAudioEffectTab));
        Raise(nameof(ShowVideoEffectView));
        Raise(nameof(ShowAudioEffectView));
        Raise(nameof(IsTextTab));
        Raise(nameof(ShowTextView));
        Raise(nameof(IsYmmSettingsTab));
        Raise(nameof(ShowYmmSettingsView));
        Raise(nameof(IsMaterialTab));
        Raise(nameof(ShowMaterialView));
        Raise(nameof(IsItemTab));
        Raise(nameof(ShowItemView));
        Raise(nameof(IsShortcutTab));
        Raise(nameof(ShowShortcutView));
        Raise(nameof(IsAiEditTab));
        Raise(nameof(ShowAiEditView));
        Raise(nameof(ShowSettingsPage));
    }

    // ---------- このウィンドウのタブ(どのタブを上に出すかは、ウィンドウごと) ----------
    /// <summary>このウィンドウの番号(YmmExplorer = 1、YmmExplorer ２〜５ = 2〜5)</summary>
    protected virtual int WindowId => 1;

    /// <summary>このウィンドウの番号(1〜5)</summary>
    public int WindowNumber => WindowId;

    /// <summary>右下に短く知らせる</summary>
    public void Notify(string text) => Toast(text);

    /// <summary>このウィンドウで、タブバーに出すタブ</summary>
    public WindowTabs WindowTabs { get; }

    /// <summary>設定画面の「タブ構成」の行(種類の名前 → 行の一覧)。表示のチェックは、このウィンドウだけに効く</summary>
    public Dictionary<string, ObservableCollection<TabToggle>> TabToggles { get; } = new();

    void RebuildTabToggles()
    {
        foreach (var g in TabGroups.All)
        {
            var key = g.Kind.ToString();
            if (!TabToggles.TryGetValue(key, out var list)) TabToggles[key] = list = new ObservableCollection<TabToggle>();
            var want = g.Tabs;
            if (list.Count == want.Count && list.Select(t => t.Entry).SequenceEqual(want))
            {
                foreach (var t in list) t.RaiseVisible();
                continue;
            }
            foreach (var t in list) t.Detach();
            list.Clear();
            foreach (var e in want) list.Add(new TabToggle(e, WindowTabs));
        }
    }

    void BuildTabs()
    {
        foreach (var t in Tabs) t.Detach();
        Tabs.Clear();
        bool any = false;
        foreach (var g in TabGroups.All)
        {
            bool first = true;
            var visible = WindowTabs.VisibleTabs(g);
            foreach (var e in visible)
            {
                Tabs.Add(new TabVm(this, e, any && first));     // 種類の切れ目に区切り線
                first = false;
            }
            if (visible.Count > 0) any = true;
        }
    }

    // タブの数(全ウィンドウ共通)や、このウィンドウで出すタブが変わった時
    void OnTabsChanged()
    {
        BuildTabs();
        RebuildTabToggles();
        if (selectedKind == TabKind.Shortcut) ReloadActionShortcuts();     // タブ名・数が変わったら、操作ショートカットの一覧も
        // 開いているタブが消えた/隠れた時は、表示中のいちばん左のタブへ(設定画面は開いたまま)
        var stillThere = WindowTabs.VisibleTabs(TabGroups.Of(selectedKind)).Any(t => t.Slot == selectedSlot);
        if (!stillThere && WindowTabs.FirstVisible() is { } f) SelectTab(f.Kind, f.Slot, false);
        else RefreshTabState();
    }

    // ---------- エクスプローラーのタブごとの状態(開いているフォルダーと、戻る・進む履歴) ----------
    // 検索条件・表示モード・並び順・選択は、すべてのエクスプローラータブで共通です。
    sealed record ExplorerTabState(string Path, string[] Back, string[] Forward);

    readonly Dictionary<int, ExplorerTabState> explorerStates = new();
    int activeExplorerSlot = 1;

    /// <summary>そのエクスプローラータブの開始フォルダー(設定した場所が無ければビデオフォルダー)</summary>
    static string StartPathFor(int slot)
    {
        var p = TabGroups.Explorer.Tabs.FirstOrDefault(t => t.Slot == slot)?.StartPath;
        return !string.IsNullOrWhiteSpace(p) && Directory.Exists(p) ? p : StartPath();
    }

    static string StartPath()
    {
        var start = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        if (!Directory.Exists(start)) start = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return start;
    }

    void SaveExplorerState()
        => explorerStates[activeExplorerSlot] = new ExplorerTabState(CurrentPath, back.ToArray(), forward.ToArray());

    void LoadExplorerState(int slot)
    {
        activeExplorerSlot = slot;
        RestoreViewMode(slot);
        RestoreFilters(slot);
        back.Clear();
        forward.Clear();
        if (explorerStates.TryGetValue(slot, out var st) && Directory.Exists(st.Path))
        {
            // Stack.ToArray() は新しい順なので、古い方から積み直す
            foreach (var x in st.Back.Reverse()) back.Push(x);
            foreach (var x in st.Forward.Reverse()) forward.Push(x);
            Navigate(st.Path, false);
        }
        else Navigate(StartPathFor(slot), false);
    }

    // ---------- テンプレート(選んでいるタブの分だけを表示) ----------
    readonly TemplateStore templateStore = TemplateStore.Instance;
    List<TemplateItem> selectedTemplates = new();

    int templateSlot => selectedKind == TabKind.Template ? selectedSlot : 0;

    public ObservableCollection<TemplateItem> Templates { get; } = new();
    public bool HasNoTemplates => Templates.Count == 0;

    string templateMessage = "";
    /// <summary>テンプレートタブの上部に出す案内・結果メッセージ</summary>
    public string TemplateMessage { get => templateMessage; private set => Set(ref templateMessage, value); }

    public string TemplateSelectionText => $"{selectedTemplates.Count} / {Templates.Count} 件を選択中";

    TemplateItem? selectedTemplate;
    /// <summary>ちょうど1つ選んでいる時だけ、名前やメモを編集できる</summary>
    public TemplateItem? SelectedTemplate
    {
        get => selectedTemplate;
        private set
        {
            if (!Set(ref selectedTemplate, value)) return;
            Raise(nameof(HasSelectedTemplate));
            Raise(nameof(HasNoSelectedTemplate));
        }
    }
    public bool HasSelectedTemplate => selectedTemplate != null;
    public bool HasNoSelectedTemplate => selectedTemplate == null;

    void OnTemplateStoreChanged(int slot)
    {
        if (slot == templateSlot) ReloadTemplates();
    }

    void ReloadTemplates()
    {
        selectedTemplates = new();
        SelectedTemplate = null;
        Templates.Clear();
        TemplateMessage = "";
        if (templateSlot >= 1)
            foreach (var e in templateStore.All(templateSlot)) Templates.Add(new TemplateItem(e, templateStore));
        Raise(nameof(HasNoTemplates));
        Raise(nameof(TemplateSelectionText));
    }

    public void SetTemplateSelection(IEnumerable<TemplateItem> sel)
    {
        selectedTemplates = sel.ToList();
        SelectedTemplate = selectedTemplates.Count == 1 ? selectedTemplates[0] : null;
        Raise(nameof(TemplateSelectionText));
    }

    /// <summary>いま開いているテンプレートタブに追加</summary>
    public void AddTemplatePaths(IEnumerable<string> paths)
    {
        if (templateSlot < 1) return;
        var n = templateStore.Add(templateSlot, paths);
        TemplateMessage = n > 0 ? $"{n} 件追加しました" : "すでに登録されています";
    }

    void RemoveSelectedTemplates()
    {
        if (templateSlot < 1) return;
        var paths = selectedTemplates.Select(t => t.Path).ToList();
        if (paths.Count == 0) return;
        templateStore.Remove(templateSlot, paths);
        TemplateMessage = $"{paths.Count} 件をテンプレートから削除しました(元のファイルは削除されません)";
    }

    /// <summary>プレビューできるものはプレビュー、フォルダーはエクスプローラータブで開く、その他は既定のアプリで開く</summary>
    public void OpenTemplate(TemplateItem? t)
    {
        var f = t?.Item;
        if (f == null) { TemplateMessage = "ファイルが見つかりません"; return; }
        if (f.IsDirectory) { RevealTemplate(t); return; }
        if (f.CanPreview) { ShowPreview(f); return; }
        OpenExternal(f);
    }

    /// <summary>エクスプローラータブで、そのテンプレートがある場所を開く</summary>
    void RevealTemplate(TemplateItem? t)
    {
        if (t == null) return;
        var dir = Directory.Exists(t.Path) ? t.Path : System.IO.Path.GetDirectoryName(t.Path);
        if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) { TemplateMessage = "場所が見つかりません"; return; }
        var ex = WindowTabs.VisibleTabs(TabGroups.Explorer).FirstOrDefault(x => x.Slot == activeExplorerSlot)
                 ?? WindowTabs.VisibleTabs(TabGroups.Explorer).FirstOrDefault();
        if (ex == null) { TemplateMessage = "エクスプローラーのタブが非表示のため開けません(設定で表示してください)"; return; }
        SelectTab(TabKind.Explorer, ex.Slot);
        Navigate(dir);
    }

    // ---------- テンプレートの並べ替え ----------
    /// <summary>並べ替え後に、移動した項目を選び直してほしい時</summary>
    public event Action<IReadOnlyList<TemplateItem>>? TemplatesReordered;

    /// <summary>選んだテンプレートを移動する。how = up / down / top / bottom</summary>
    void MoveTemplates(string how)
    {
        if (templateSlot < 1 || selectedTemplates.Count == 0) return;
        var paths = selectedTemplates.Select(t => t.Path).ToList();
        templateStore.Move(templateSlot, paths, how);
        AfterTemplateReorder(paths);
    }

    /// <summary>ドラッグ&ドロップ用: 並べ替えのデータ(1行目=T+タブ番号、以降=パス)</summary>
    public string? TemplateReorderPayload()
        => templateSlot < 1 || selectedTemplates.Count == 0
            ? null
            : $"T{templateSlot}\n" + string.Join("\n", selectedTemplates.Select(t => t.Path));

    /// <summary>このタブの中での並べ替えなら移動して true。他のタブ・他のウィンドウからのドロップなら false</summary>
    public bool TryReorderTemplates(string payload, int insertIndex)
    {
        var lines = payload.Split('\n');
        if (lines.Length < 2 || lines[0] != $"T{templateSlot}") return false;
        var paths = lines.Skip(1).ToList();
        var known = Templates.Select(t => t.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!paths.All(known.Contains)) return false;
        templateStore.MoveTo(templateSlot, paths, insertIndex);
        AfterTemplateReorder(paths);
        return true;
    }

    void AfterTemplateReorder(List<string> paths)
    {
        var set = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
        TemplatesReordered?.Invoke(Templates.Where(t => set.Contains(t.Path)).ToList());
    }

    // ---------- テンプレートのサムネイル ----------
    public void ShowTemplateMessage(string message) => TemplateMessage = message;

    void SetTemplateThumbnail(TemplateItem? t, BitmapSource? image, string okMessage)
    {
        if (t == null) { TemplateMessage = "テンプレートを選んでください"; return; }
        TemplateMessage = t.SetThumbnail(image) ? okMessage : "画像を変換できませんでした";
    }

    /// <summary>クリップボードの画像(スクリーンショットなど)をサムネイルにする</summary>
    public void SetTemplateThumbnailFromClipboard()
    {
        try
        {
            if (!System.Windows.Clipboard.ContainsImage())
            {
                TemplateMessage = "クリップボードに画像がありません。画像をコピーしてから押してください";
                return;
            }
            SetTemplateThumbnail(selectedTemplate, System.Windows.Clipboard.GetImage(), "クリップボードの画像をサムネイルにしました");
        }
        catch { TemplateMessage = "クリップボードの画像を読み取れませんでした"; }
    }

    /// <summary>画像ファイルをサムネイルにする</summary>
    public void SetTemplateThumbnailFromFile(TemplateItem? t, string path)
    {
        var img = PresetThumb.LoadFile(path);
        if (img == null) { TemplateMessage = "画像を読み込めませんでした"; return; }
        SetTemplateThumbnail(t, img, "画像をサムネイルにしました");
    }

    /// <summary>再生中の動画の、いま表示しているコマをサムネイルにする</summary>
    public void UseFrameAsTemplateThumbnail(TemplateItem? t, BitmapSource frame)
        => SetTemplateThumbnail(t, frame, "選んだコマをサムネイルにしました");

    // ====================================================================
    //  設定のタブ / ショートカットキー / プラグイン一覧 / YMM4設定 / 標準の自動収録
    // ====================================================================
    static bool pendingDeletesDone;

    // 予約された完全リセットは、どの設定を読むよりも先に行う
    static ExplorerViewModel() => PluginReset.ApplyPending();

    string settingsSection = "tabs";

    /// <summary>設定画面の、いま開いているタブ(tabs / behavior / link / shortcuts / ymm / plugins)</summary>
    public string SettingsSection
    {
        get => settingsSection;
        set
        {
            if (!Set(ref settingsSection, value)) return;
            Raise(nameof(ShowSettingsTabs));
            Raise(nameof(ShowSettingsBehavior));
            Raise(nameof(ShowSettingsLink));
            Raise(nameof(ShowSettingsShortcuts));
            Raise(nameof(ShowSettingsPlugins));
            Raise(nameof(ShowSettingsReset));
            Raise(nameof(ShowSettingsShapes));
            if (value == "plugins") RefreshPlugins();
            else if (value == "shapes") _ = ShapeVisibility.EnsureLoadedAsync();
            else if (value == "shortcuts") ReloadShortcutRows();
        }
    }

    public bool ShowSettingsTabs { get => settingsSection == "tabs"; set { if (value) SettingsSection = "tabs"; } }
    public bool ShowSettingsBehavior { get => settingsSection == "behavior"; set { if (value) SettingsSection = "behavior"; } }
    public bool ShowSettingsLink { get => settingsSection == "link"; set { if (value) SettingsSection = "link"; } }
    public bool ShowSettingsShortcuts { get => settingsSection == "shortcuts"; set { if (value) SettingsSection = "shortcuts"; } }
    public bool ShowSettingsPlugins { get => settingsSection == "plugins"; set { if (value) SettingsSection = "plugins"; } }
    public bool ShowSettingsReset { get => settingsSection == "reset"; set { if (value) SettingsSection = "reset"; } }
    public bool ShowSettingsShapes { get => settingsSection == "shapes"; set { if (value) SettingsSection = "shapes"; } }

    /// <summary>設定の「図形の表示」(一覧に出す図形を選ぶ)</summary>
    public ShapeVisibilityVm ShapeVisibility { get; } = new();

    void OnShapeVisibilityChanged()
    {
        if (System.Windows.Application.Current?.Dispatcher is { } d && !d.CheckAccess()) { d.BeginInvoke(OnShapeVisibilityChanged); return; }
        ReloadShapes();
    }

    // ---------- 完全リセット ----------
    readonly PluginReset.Request resetRequest = PluginReset.Pending() ?? new PluginReset.Request();
    public bool ResetSettings { get => resetRequest.Settings; set { resetRequest.Settings = value; Raise(nameof(ResetSettings)); } }
    public bool ResetShortcuts { get => resetRequest.Shortcuts; set { resetRequest.Shortcuts = value; Raise(nameof(ResetShortcuts)); } }
    public bool ResetLibrary { get => resetRequest.Library; set { resetRequest.Library = value; Raise(nameof(ResetLibrary)); } }
    public bool ResetMaterials { get => resetRequest.Materials; set { resetRequest.Materials = value; Raise(nameof(ResetMaterials)); } }
    public bool ResetKeepBackup { get => resetRequest.KeepBackup; set { resetRequest.KeepBackup = value; Raise(nameof(ResetKeepBackup)); } }
    public bool IsResetPending => PluginReset.IsPending;
    public bool IsResetNotPending => !PluginReset.IsPending;
    string resetMessage = PluginReset.LastResult;
    public string ResetMessage { get => resetMessage; private set { Set(ref resetMessage, value); Raise(nameof(HasResetMessage)); } }
    public bool HasResetMessage => resetMessage.Length > 0;

    ICommand? scheduleResetCommand, cancelResetCommand, closeYmmCommand, openResetBackupCommand;
    public ICommand ScheduleResetCommand => scheduleResetCommand ??= new RelayCommand(_ =>
    {
        if (!resetRequest.Settings && !resetRequest.Shortcuts && !resetRequest.Library && !resetRequest.Materials)
        { ResetMessage = "⚠ リセットするものを、1つ以上選んでください"; return; }
        var parts = new List<string>();
        if (resetRequest.Settings) parts.Add("・設定(タブ構成・動作・表示・タグ など)");
        if (resetRequest.Shortcuts) parts.Add("・ショートカットキー・キーの置き換え");
        if (resetRequest.Library) parts.Add("・登録したテンプレート・プリセット・図形・エフェクト・テキストの書式");
        if (resetRequest.Materials) parts.Add("・素材データベース(取り込んだ素材のファイルも)");
        var msg = "次のものを、すべて初期状態に戻します。\n\n" + string.Join("\n", parts)
                  + (resetRequest.KeepBackup ? "\n\n消すものは、バックアップのフォルダーへ移します(あとで戻せます)。" : "\n\n⚠ バックアップを残さないので、元に戻せません。")
                  + "\n\nリセットは、次にYMM4を起動した時に行います。よろしいですか?";
        var owner = System.Windows.Application.Current?.MainWindow;
        var answer = owner != null
            ? System.Windows.MessageBox.Show(owner, msg, "YmmExplorer の完全リセット", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning)
            : System.Windows.MessageBox.Show(msg, "YmmExplorer の完全リセット", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning);
        if (answer != System.Windows.MessageBoxResult.OK) return;
        resetRequest.RequestedAt = DateTime.Now;
        if (!PluginReset.Schedule(resetRequest, out var error)) { ResetMessage = "⚠ 予約できませんでした: " + error; return; }
        ResetMessage = "リセットを予約しました。YMM4を閉じて、もう一度起動するとリセットされます(取りやめる時は「予約を取り消す」)";
        Raise(nameof(IsResetPending)); Raise(nameof(IsResetNotPending));
    });
    public ICommand CancelResetCommand => cancelResetCommand ??= new RelayCommand(_ =>
    {
        PluginReset.Cancel();
        ResetMessage = "リセットの予約を取り消しました";
        Raise(nameof(IsResetPending)); Raise(nameof(IsResetNotPending));
    });
    /// <summary>YMM4を閉じる(いつもの閉じる操作と同じ。保存の確認なども出ます)</summary>
    public ICommand CloseYmmCommand => closeYmmCommand ??= new RelayCommand(_ =>
    {
        try { System.Windows.Application.Current?.MainWindow?.Close(); } catch { }
    });
    public ICommand OpenResetBackupCommand => openResetBackupCommand ??= new RelayCommand(_ =>
    {
        try
        {
            System.IO.Directory.CreateDirectory(PluginReset.BackupRoot);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(PluginReset.BackupRoot) { UseShellExecute = true });
        }
        catch { }
    });

    // ---------- ショートカットキー ----------
    public ObservableCollection<ShortcutRow> ShortcutRows { get; } = new();
    public bool HasNoShortcuts => ShortcutRows.Count == 0;

    void OnShortcutsChanged()
    {
        if (ShortcutRow.Suppress || (settingsSection != "shortcuts" && !ShowShortcutView)) return;
        ReloadShortcutRows();
        foreach (var g in ActionShortcutGroups) foreach (var r in g.Rows) r.Refresh();   // 他の画面でキーを付け替えた時も表示を合わせる
        Raise(nameof(TextSplitShortcutKeys));
    }

    // ---------- ショートカットのタブ(キーの置き換え) ----------
    // ---------- AIエディットのタブ(AIに、タイムラインとプレビューを見せて編集の相談をする) ----------
    public AiEditVm AiEdit { get; } = new();
    public bool IsAiEditTab => selectedKind == TabKind.AiEdit;
    public bool ShowAiEditView => IsAiEditTab && !isSettingsPageOpen;
    public TabGroup AiEditGroup => TabGroups.AiEdit;

    public bool IsShortcutTab => selectedKind == TabKind.Shortcut;
    public bool ShowShortcutView => IsShortcutTab && !isSettingsPageOpen;
    public TabGroup ShortcutGroup => TabGroups.Shortcut;
    public ObservableCollection<RemapRow> RemapRows { get; } = new();
    public bool HasNoRemaps => RemapRows.Count == 0;
    public ICommand AddRemapCommand => addRemapCommand ??= new RelayCommand(_ =>
    {
        RemapRows.Add(new RemapRow(ShortcutStore.Instance.AddRemap()));
        Raise(nameof(HasNoRemaps));
    });
    public ICommand RemoveRemapCommand => removeRemapCommand ??= new RelayCommand(p =>
    {
        if (p is not RemapRow row) return;
        ShortcutStore.Instance.RemoveRemap(row.Id);
        RemapRows.Remove(row);
        Raise(nameof(HasNoRemaps));
    });
    ICommand? addRemapCommand, removeRemapCommand;

    void ReloadRemapRows()
    {
        RemapRows.Clear();
        foreach (var r in ShortcutStore.Instance.Remaps()) RemapRows.Add(new RemapRow(r));
        Raise(nameof(HasNoRemaps));
    }

    /// <summary>このツールの操作(タブの移動など)に付けるショートカット</summary>
    public ObservableCollection<ActionShortcutGroup> ActionShortcutGroups { get; } = new();

    /// <summary>操作ショートカットを、どのウィンドウで実行するか(0 = 最後にクリックしたウィンドウ)。最初は、このウィンドウ</summary>
    int actionScope = -1;
    public int ActionScope
    {
        get => actionScope < 0 ? WindowId : actionScope;
        set
        {
            if (value == ActionScope) return;
            actionScope = value;
            foreach (var c in ActionScopeChoices) c.Refresh();
            Raise(nameof(ActionScope));
            Raise(nameof(ActionScopeText));
            ReloadActionShortcuts();
        }
    }

    public string ActionScopeText => ActionScope == 0
        ? "どのウィンドウで押しても、最後にクリックしたウィンドウ(YmmExplorer・２・３…)で実行します。"
        : $"{ActionShortcuts.WindowLabel(ActionScope)} のタブ・画面だけを動かします。同じキーを別のウィンドウにも割り当てると、最後にクリックしたウィンドウの方が実行されます。";

    ObservableCollection<ActionScopeChoice>? actionScopeChoices;
    public ObservableCollection<ActionScopeChoice> ActionScopeChoices => actionScopeChoices ??= new ObservableCollection<ActionScopeChoice>(
        Enumerable.Range(0, ActionShortcuts.WindowCount + 1).Select(w => new ActionScopeChoice(w, () => ActionScope, v => ActionScope = v)));

    /// <summary>「選択中のテキストを分割画面へ」のショートカット(どのウィンドウで押しても、最後にクリックしたウィンドウで開く)</summary>
    public string TextSplitShortcutKeys
    {
        get => ShortcutStore.Instance.KeysFor(ActionShortcuts.Kind, "text-split");
        set
        {
            ShortcutStore.Instance.Set(ActionShortcuts.Kind, "text-split", "選択中のテキストを分割画面へ", value ?? "");
            Raise(nameof(TextSplitShortcutKeys));
            foreach (var g in ActionShortcutGroups) foreach (var r in g.Rows) r.Refresh();
        }
    }

    public ICommand ClearActionShortcutCommand => clearActionShortcutCommand ??= new RelayCommand(p =>
    {
        if (p is ActionShortcutRow r) r.Keys = "";
    });
    ICommand? clearActionShortcutCommand;

    void ReloadActionShortcuts()
    {
        ActionShortcutGroups.Clear();
        // 作り直すと開いた見出しが閉じてしまうので、キーの表示だけ更新する
        void Again() => System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            foreach (var g in ActionShortcutGroups) foreach (var r in g.Rows) r.Refresh();
            ReloadShortcutRows();
        }));
        foreach (var g in ActionShortcuts.Build(Again, ActionScope)) ActionShortcutGroups.Add(g);
    }

    void ReloadShortcutRows()
    {
        ShortcutRows.Clear();
        foreach (var b in ShortcutStore.Instance.All().Where(b => b.Kind != ActionShortcuts.Kind).OrderBy(b => b.Kind).ThenBy(b => b.Name, StringComparer.CurrentCultureIgnoreCase))
            ShortcutRows.Add(new ShortcutRow(b, () => System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(ReloadShortcutRows))));
        Raise(nameof(HasNoShortcuts));
    }

    /// <summary>詳細パネルで選んでいるファイル・フォルダー(ショートカットキーの割り当て用)</summary>
    public bool HasDetailItem => detailItem != null;

    public string DetailShortcutKeys
    {
        get => detailItem == null ? "" : ShortcutStore.Instance.KeysFor("file", detailItem.FullPath);
        set
        {
            if (detailItem == null) return;
            ShortcutStore.Instance.Set("file", detailItem.FullPath, detailItem.Name, value ?? "");
            Raise(nameof(DetailShortcutKeys));
        }
    }

    string toastText = "";
    public string ToastText { get => toastText; private set => Set(ref toastText, value); }
    bool showToast;
    public bool ShowToast { get => showToast; private set => Set(ref showToast, value); }
    System.Windows.Threading.DispatcherTimer? toastTimer;

    void Toast(string text)
    {
        ToastText = text;
        ShowToast = true;
        toastTimer ??= new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3.5) };
        toastTimer.Tick -= OnToastTick;
        toastTimer.Tick += OnToastTick;
        toastTimer.Stop();
        toastTimer.Start();
    }

    void OnToastTick(object? sender, EventArgs e)
    {
        toastTimer?.Stop();
        ShowToast = false;
    }

    /// <summary>検索欄へカーソルを移す(画面側で、表示中の検索欄を探す)</summary>
    public event Action? FocusSearchRequested;
    /// <summary>画面の拡大(+1)・縮小(-1)・元に戻す(0)</summary>
    public event Action<int>? UiZoomRequested;

    /// <summary>このツールの操作のショートカット。成功した時は何も表示せず、できなかった時だけ知らせる</summary>
    void ExecuteAction(ShortcutBinding b)
    {
        var id = ActionShortcuts.Split(b.TargetId).action;   // 「w2|」のようなウィンドウ指定は、ShortcutHost で済ませてある
        string? fail = null;
        var tabs = Tabs.ToList();                        // このウィンドウのタブバーの並び
        int current = tabs.FindIndex(t => t.Kind == selectedKind && t.Slot == selectedSlot);

        void Go(TabVm t) => SelectTab(t.Kind, t.Slot, true);
        bool Visible(TabKind k, int slot) => tabs.Any(t => t.Kind == k && t.Slot == slot);
        bool OpenKind(TabKind k)
        {
            if (selectedKind == k && !isSettingsPageOpen) return true;
            var t = tabs.FirstOrDefault(x => x.Kind == k);
            if (t == null) return false;
            Go(t);
            return true;
        }

        if (id.StartsWith("tab:"))
        {
            var parts = id.Split(':');
            if (parts.Length == 3 && Enum.TryParse<TabKind>(parts[1], out var kind) && int.TryParse(parts[2], out var slot))
            {
                if (slot > TabGroups.Of(kind).Count) fail = "そのタブはもうありません(タブの数が減っています)";
                else if (!Visible(kind, slot)) fail = "そのタブは、このウィンドウでは表示しない設定になっています";
                else SelectTab(kind, slot, true);
            }
            else fail = "割り当て先のタブが分かりません";
        }
        else if (id.StartsWith("tab-index:"))
        {
            if (int.TryParse(id.Substring("tab-index:".Length), out var n) && n >= 1 && n <= tabs.Count) Go(tabs[n - 1]);
            else fail = "その番号のタブはありません";
        }
        else switch (id)
        {
            case "tab-next":
            case "tab-prev":
                if (tabs.Count == 0) break;
                int step = id == "tab-next" ? 1 : -1;
                int start = current < 0 ? (step > 0 ? -1 : 0) : current;
                if (isSettingsPageOpen && current >= 0) { SelectTab(selectedKind, selectedSlot, true); break; }
                Go(tabs[((start + step) % tabs.Count + tabs.Count) % tabs.Count]);
                break;
            case "tab-last":
                if (previousKind is TabKind pk && Visible(pk, previousSlot)) SelectTab(pk, previousSlot, true);
                else fail = "戻れるタブがありません";
                break;
            case "back":
            case "forward":
            case "up":
            case "refresh":
                if (!OpenKind(TabKind.Explorer) && selectedKind != TabKind.Explorer) { fail = "エクスプローラーのタブがありません"; break; }
                if (id == "back") GoBack();
                else if (id == "forward") GoForward();
                else if (id == "up") GoUp();
                else _ = RefreshAsync();
                break;
            case "search":
                if (isSettingsPageOpen) SelectTab(selectedKind, selectedSlot, true);
                FocusSearchRequested?.Invoke();
                break;
            case "settings":
                IsSettingsPageOpen = !IsSettingsPageOpen;
                break;
            case "zoom-in": UiZoomRequested?.Invoke(1); break;
            case "zoom-out": UiZoomRequested?.Invoke(-1); break;
            case "zoom-reset": UiZoomRequested?.Invoke(0); break;
            case "text-split":
                TextSplit.Open();
                break;
            case "ai-audio":
                if (!OpenKind(TabKind.Material)) { fail = "素材データベースのタブがありません"; break; }
                Materials.Studio.IsOpen = true;
                break;
            case "item-tab":
                if (!OpenKind(TabKind.Item)) fail = "アイテムのタブがありません";
                break;
            default:
                fail = "未対応の操作です";
                break;
        }
        if (fail != null) Toast($"[{b.Keys}] {ActionShortcuts.NameOf(id)}: {fail}");
    }

    /// <summary>ショートカットキーが押された時の実行(どのタブを開いていても動く)</summary>
    public async Task ExecuteShortcutAsync(ShortcutBinding b)
    {
        if (b.Kind == ActionShortcuts.Kind)
        {
            try { ExecuteAction(b); }
            catch (Exception ex) { Toast($"[{b.Keys}] {b.Name}: 実行できませんでした: " + ex.Message); }
            return;
        }
        string message;
        try
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(System.Windows.Application.Current.MainWindow).Handle;
            switch (b.Kind)
            {
                case "preset":
                {
                    var e = presetStore.Find(b.TargetId);
                    if (e == null) { message = "割り当て先のプリセットが見つかりません"; break; }
                    await PlacePresetAsync(new PresetItem(e, presetStore), hwnd, null);
                    message = PresetMessage;
                    break;
                }
                case "shape":
                {
                    var e = shapeStore.Find(b.TargetId);
                    if (e == null) { message = "割り当て先の図形が見つかりません"; break; }
                    await PlaceShapeAsync(new ShapeItem(e, shapeStore, false), hwnd, null);
                    message = ShapeMessage;
                    break;
                }
                case "video-effect":
                case "audio-effect":
                case "text":
                {
                    var tabKind = b.Kind == "video-effect" ? TabKind.VideoEffect : b.Kind == "audio-effect" ? TabKind.AudioEffect : TabKind.Text;
                    var lib = tabKind == TabKind.VideoEffect ? VideoFx : tabKind == TabKind.AudioEffect ? AudioFx : TextLib;
                    var st = EffectStore.For(tabKind);
                    var e = st.Find(b.TargetId);
                    if (e == null) { message = "割り当て先が見つかりません"; break; }
                    await lib.ApplyAsync(new EffectItem(e, st, lib.Placeholder), hwnd, null);
                    message = lib.Message;
                    break;
                }
                case "file":
                {
                    if (!System.IO.File.Exists(b.TargetId) && !System.IO.Directory.Exists(b.TargetId)) { message = "ファイル・フォルダーが見つかりません"; break; }
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(b.TargetId) { UseShellExecute = true });
                    message = "開きました";
                    break;
                }
                default: message = "未対応の種類です"; break;
            }
        }
        catch (Exception ex) { message = "実行できませんでした: " + ex.Message; }
        Toast($"[{b.Keys}] {b.Name}: {message}");
    }

    // ---------- 素材データベースへの追加 ----------
    /// <summary>エクスプローラーで選んでいるファイル・フォルダーを、素材データベースへまとめて取り込む</summary>
    async Task AddSelectionToMaterialsAsync()
    {
        var paths = selected.Select(f => f.FullPath).ToList();
        if (paths.Count == 0) return;
        await Materials.ImportPathsAsync(paths);
        Toast("素材データベース: " + Materials.Message);
    }

    // ---------- プラグイン一覧 ----------
    public ObservableCollection<PluginInfo> Plugins { get; } = new();

    string pluginMessage = "";
    public string PluginMessage { get => pluginMessage; private set => Set(ref pluginMessage, value); }

    void RefreshPlugins()
    {
        Plugins.Clear();
        var found = PluginManager.Scan(out var message);
        foreach (var p in found) Plugins.Add(p);
        PluginMessage = message;
    }

    void DeletePlugin(PluginInfo? p)
    {
        if (p == null) return;
        var answer = System.Windows.MessageBox.Show(
            $"プラグイン「{p.Name}」を削除します。\n\n{p.Path}\n\nこの操作は元に戻せません。削除しますか?",
            "プラグインの削除", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
        if (answer != System.Windows.MessageBoxResult.Yes) return;
        var result = PluginManager.Delete(p);
        RefreshPlugins();
        PluginMessage = result;
    }

    // ---------- YMM4設定(タブ・独自レイアウト) ----------
    readonly YmmSettingsSaver ymmSaver = new();

    /// <summary>YMM4の設定グループ(すべて)</summary>
    public ObservableCollection<YmmSettingGroup> YmmSettingGroups { get; } = new();

    /// <summary>左の一覧に出すグループ(検索に合うものだけ)</summary>
    public ObservableCollection<YmmSettingGroup> VisibleYmmGroups { get; } = new();

    YmmSettingGroup? selectedYmmGroup;
    public YmmSettingGroup? SelectedYmmGroup
    {
        get => selectedYmmGroup;
        set
        {
            if (Set(ref selectedYmmGroup, value)) Raise(nameof(HasSelectedYmmGroup));
        }
    }
    public bool HasSelectedYmmGroup => selectedYmmGroup != null;

    string ymmSettingMessage = "";
    public string YmmSettingMessage { get => ymmSettingMessage; private set => Set(ref ymmSettingMessage, value); }

    string ymmSettingFilter = "";
    public string YmmSettingFilter
    {
        get => ymmSettingFilter;
        set
        {
            if (Set(ref ymmSettingFilter, value ?? "")) ApplyYmmFilter();
        }
    }

    void LoadYmmSettings()
    {
        var previous = selectedYmmGroup?.Title;
        YmmSettingGroups.Clear();
        var loaded = YmmSettingsCatalog.Load(ymmSaver.MarkDirty, out var message);
        foreach (var g in loaded) YmmSettingGroups.Add(g);
        YmmSettingMessage = message;
        SelectedYmmGroup = null;
        ApplyYmmFilter();
        if (previous != null)
            SelectedYmmGroup = VisibleYmmGroups.FirstOrDefault(g => g.Title == previous) ?? SelectedYmmGroup;
    }

    void ApplyYmmFilter()
    {
        var f = ymmSettingFilter.Trim();
        VisibleYmmGroups.Clear();
        foreach (var g in YmmSettingGroups)
        {
            var groupHit = f.Length > 0 && g.Title.Contains(f, StringComparison.CurrentCultureIgnoreCase);
            foreach (var sub in g.SubGroups)
            {
                var subHit = f.Length > 0 && sub.Title.Contains(f, StringComparison.CurrentCultureIgnoreCase);
                foreach (var e in sub.Entries)
                    e.IsVisible = f.Length == 0 || groupHit || subHit
                        || e.Title.Contains(f, StringComparison.CurrentCultureIgnoreCase)
                        || e.PropertyName.Contains(f, StringComparison.CurrentCultureIgnoreCase)
                        || e.Description.Contains(f, StringComparison.CurrentCultureIgnoreCase);
            }
            g.RefreshCounts();
            g.IsVisible = g.VisibleCount > 0;
            if (g.IsVisible) VisibleYmmGroups.Add(g);
        }
        if (selectedYmmGroup == null || !VisibleYmmGroups.Contains(selectedYmmGroup))
            SelectedYmmGroup = VisibleYmmGroups.FirstOrDefault();
    }

    void RevertYmmSettings()
    {
        int n = 0;
        foreach (var g in YmmSettingGroups)
            foreach (var e in g.Entries)
                if (e.IsChanged) { e.Revert(); n++; }
        ymmSaver.Flush();
        YmmSettingMessage = n == 0 ? "変更した項目はありません" : $"{n} 項目を、開いた時の値に戻しました";
    }

    // ---------- 標準の図形・エフェクトの自動収録(タブごとに独立) ----------
    string autoRegisterMessage = "";
    public string AutoRegisterMessage { get => autoRegisterMessage; private set => Set(ref autoRegisterMessage, value); }

    static string SafeRun(Func<string> f)
    {
        try { return f(); }
        catch (Exception ex) { return "⚠ 自動収録に失敗しました: " + ex.GetBaseException().Message; }
    }

    async Task AutoRegisterAsync(TabEntry? tab)
    {
        if (tab == null) return;
        switch (tab.Kind)
        {
            case TabKind.Shape: AutoRegisterMessage = $"「{tab.DisplayName}」: " + await AutoRegisterShapesAsync(tab.Slot); break;
            case TabKind.VideoEffect: AutoRegisterMessage = $"「{tab.DisplayName}」: " + SafeRun(() => VideoFx.AutoRegisterBuiltins(tab.Slot)); break;
            case TabKind.AudioEffect: AutoRegisterMessage = $"「{tab.DisplayName}」: " + SafeRun(() => AudioFx.AutoRegisterBuiltins(tab.Slot)); break;
        }
    }

    async Task<string> AutoRegisterShapesAsync(int slot)
    {
        try
        {
            // 毎回、読み込まれている図形を探し直す(プラグインを入れた直後でも拾えるように)
            await Task.Run(() => ShapeDiscovery.Scan());
        }
        catch (Exception ex) { return "⚠ 図形の検索に失敗しました: " + ex.GetBaseException().Message; }
        var found = ShapeDiscovery.Found.ToList();
        if (found.Count == 0) return "YMM4の図形を見つけられませんでした";
        // 設定の「図形の表示」で外した図形は、収録しない
        var all = found.Where(f => settings.IsShapeShown(f.Key)).ToList();
        if (all.Count == 0) return "収録できる図形がありません(設定の「図形の表示」で、すべて外されています)";
        var added = shapeStore.Sync(slot, all);
        var skippedRemoved = shapeStore.RemovedCount(slot);
        var builtinCount = all.Count(f => f.IsBuiltIn);

        // 配置データが無い図形には、初期値の図形アイテムから配置データ(アイテムテンプレート)を作って登録する
        var made = new Dictionary<string, string>();
        int noData = 0;
        string firstError = "";
        foreach (var e in shapeStore.All(slot))
        {
            if (e.IsCustom || string.IsNullOrEmpty(e.Key) || e.TemplateJson != null || e.Formats is { Count: > 0 }) continue;
            string? json = null;
            string err = "";
            try
            {
                var item = Ymm.CreateShapeItem(e.Key, 0, 0, 150);
                var template = item == null ? null : Ymm.CreateTemplate(e.Name, new[] { item });
                json = template == null ? null : Ymm.TemplateToJson(template);
                if (json == null) err = Ymm.LastError;
            }
            catch (Exception ex) { err = ex.GetBaseException().Message; }
            if (json != null) made[e.Id] = json;
            else { noData++; if (firstError.Length == 0) firstError = $"「{e.Name}」: {err}"; }
        }
        if (made.Count > 0) shapeStore.AttachTemplates(slot, made);

        // サムネイルの無い図形には、初期値の状態の図形を描いた画像を付ける
        // (初期値では描けない図形は、名前だけのサムネイル。前に自動で付けたものも作り直す。自分で設定したものは変えない)
        var thumbs = new Dictionary<string, string>();
        int symbolic = 0;
        using var thumbSession = ShapeThumbRenderer.BeginSession();
        var targets = shapeStore.All(slot).Where(e => !e.IsCustom && !string.IsNullOrEmpty(e.Key) && (e.Thumbnail == null || e.AutoThumbnail)).ToList();
        for (int i = 0; i < targets.Count; i++)
        {
            var e = targets[i];
            if (i % 4 == 0)
            {
                AutoRegisterMessage = $"プレビュー画像を作っています… {i + 1} / {targets.Count}";
                await Dispatcher.Yield(DispatcherPriority.Background);
            }
            try
            {
                BitmapSource? img = null;
                try { img = ShapeThumbRenderer.Render(e.Key, e.Name); } catch { }
                if (img == null || ShapeThumbRenderer.LastError.Length > 0) symbolic++;
                // 描けなかった図形(仮の記号の絵)は、名前だけのサムネイルにする
                var b64 = img != null && ShapeThumbRenderer.LastError.Length == 0 ? NameThumb.EncodePicture(img) : NameThumb.Encode(e.Name, "図形");
                if (b64 != null) thumbs[e.Id] = b64;
            }
            catch { }
        }
        var thumbCount = thumbs.Count > 0 ? shapeStore.AttachAutoThumbnails(slot, thumbs) : 0;

        return $"標準 {builtinCount} 個 / プラグイン {all.Count - builtinCount} 個の図形のうち、{added} 個を自動収録しました"
            + (added < all.Count ? $"({all.Count - added} 個は収録済み、または削除したもの)" : "")
            + (skippedRemoved > 0 ? $"。削除した {skippedRemoved} 個は入れていません(「削除した自動収録の図形を戻す」で戻せます)" : "")
            + (made.Count > 0 ? $"。{made.Count} 個に配置データを付けました" : "")
            + (noData > 0 ? $"。配置データを作れなかったもの {noData} 個(最初の失敗 {firstError})" : "")
            + (thumbCount > 0 ? $"。{thumbCount} 個のサムネイルを、初期状態の図形にしました" + (symbolic > 0 ? $"(うち {symbolic} 個は初期状態では描けないので、名前の画像)" : "") : "");
    }

    /// <summary>標準の図形を、初期値のまま作って、シークバーの位置に配置する(実験的)。失敗したら、理由を返す</summary>
    Task<string?> PlaceBuiltinShapeAsync(ShapeItem s) => PlaceShapeByKeyAsync(s.Entry.Key, s.Name);

    /// <summary>図形を初期値のまま作って、シークバーの位置に配置する。失敗したら、理由を返す</summary>
    async Task<string?> PlaceShapeByKeyAsync(string key, string name)
    {
        if (Ymm.Timeline() == null) return Ymm.LastError;
        var frame = Ymm.CurrentFrame();
        var selected = Ymm.SelectedItems().FirstOrDefault();
        var layer = selected != null ? Convert.ToInt32(Ymm.Get(selected, "Layer") ?? 0) : 0;
        var item = Ymm.CreateShapeItem(key, frame, layer, 150);
        if (item == null) return Ymm.LastError;
        var template = Ymm.CreateTemplate(name, new[] { item });
        if (template == null) return Ymm.LastError;
        var (ok, message) = await Ymm.PlaceTemplateAsync(template);
        return ok ? null : message;
    }

    /// <summary>選んだ設定のPV背景を、シークバーの位置に配置する。失敗したら、理由を返す</summary>
    async Task<string?> PlacePvRecipeAsync(PvRecipe recipe, string name, bool solid3D)
    {
        if (Ymm.Timeline() == null) return Ymm.LastError;
        var frame = Ymm.CurrentFrame();
        var selected = Ymm.SelectedItems().FirstOrDefault();
        var layer = selected != null ? Convert.ToInt32(Ymm.Get(selected, "Layer") ?? 0) : 0;
        var key = solid3D ? YMM4EffectShapes.Solid3D.Solid3DDefaults.Key3D(EffectShapeCatalog.PvShapeKey) : EffectShapeCatalog.PvShapeKey;
        var item = Ymm.CreateShapeItem(key, frame, layer, 150);
        if (item == null) return Ymm.LastError;
        if (Ymm.Get(item, "ShapeParameter") is not YMM4EffectShapes.PvStage3DParameter p) return "PV背景の設定を読み込めませんでした";
        p.Scene = recipe.Scene;
        p.Solid = recipe.Solid;
        p.Style = recipe.Style;
        p.Palette = recipe.Palette;
        var template = Ymm.CreateTemplate(name, new[] { item });
        if (template == null) return Ymm.LastError;
        var (ok, message) = await Ymm.PlaceTemplateAsync(template);
        return ok ? null : message;
    }

    // ---------- YMM4の内部機能(直接方式) ----------
    // 別のテキストプリセットプラグインの解析で分かった、YMM4自身のアイテムテンプレートを使う方式。
    // 選択中のアイテムをテンプレートにして保存し、配置は MainModel.AddTemplateItemAsync で行う(クリップボード不要)。

    /// <summary>YMM4の内部機能(選択中のアイテムの取得・書き換え・テンプレートの配置)を使う</summary>
    public bool UseYmmApi
    {
        get => settings.UseYmmApi;
        set
        {
            if (settings.UseYmmApi == value) return;
            settings.UseYmmApi = value;
            settings.Save();
            Raise(nameof(UseYmmApi));
        }
    }

    string ymmDiagText = "";
    public string YmmDiagText { get => ymmDiagText; private set => Set(ref ymmDiagText, value); }

    /// <summary>選択中のアイテムを、YMM4のアイテムテンプレートのJSONにする</summary>
    static (string? Json, int Count, string Error) SelectionToTemplateJson(string name)
    {
        if (Ymm.Timeline() == null) return (null, 0, "⚠ " + Ymm.LastError);
        var items = Ymm.SelectedItems();
        if (items.Count == 0) return (null, 0, "⚠ YMM4のタイムラインで、保存したいアイテムを選んでください");
        var template = Ymm.CreateTemplate(name, items);
        if (template == null) return (null, 0, "⚠ " + Ymm.LastError);
        var json = Ymm.TemplateToJson(template);
        if (json == null) return (null, 0, "⚠ " + Ymm.LastError);
        return (json, items.Count, "");
    }

    static async Task<string> PlaceTemplateJsonAsync(string json)
    {
        var template = Ymm.TemplateFromJson(json);
        if (template == null) return "⚠ " + Ymm.LastError;
        var (ok, message) = await Ymm.PlaceTemplateAsync(template);
        return ok ? message : "⚠ " + message;
    }

    /// <summary>選択中のアイテムを、プリセットとして保存する(アイテムテンプレート方式)</summary>
    void CreatePresetFromSelection()
    {
        if (presetSlot < 1) return;
        var (json, count, error) = SelectionToTemplateJson("プリセット");
        if (json == null) { PresetMessage = error; return; }

        var fp = ClipboardRaw.FingerprintText(json);
        var same = presetStore.All(presetSlot).FirstOrDefault(e => e.TemplateJson != null && ClipboardRaw.FingerprintText(e.TemplateJson) == fp);
        if (same != null)
        {
            PresetMessage = $"同じ内容のプリセット「{same.Name}」がすでにあります(データID {fp})";
            return;
        }
        var entry = new PresetEntry { Name = $"プリセット {DateTime.Now:MM/dd HH:mm}", TemplateJson = json };
        presetStore.Add(presetSlot, entry);
        PresetMessage = $"選択中の {count} 個のアイテムを、プリセットとして保存しました(ダブルクリックでシークバーの位置に配置できます)";

        // 含まれる素材から、プレビュー動画を自動で作る(設定でオフにできます)
        if (settings.PresetAutoPreview && Presets.FirstOrDefault(x => x.Entry.Id == entry.Id) is { } created)
            _ = CreatePreviewAsync(created, true);
    }

    /// <summary>YMM4のプレビュー画面に映っている画像を、プリセットのサムネイルにする</summary>
    void SetPresetThumbnailFromPreview()
    {
        var bmp = Ymm.CapturePreview();
        if (bmp == null) { PresetMessage = "プレビュー画像を取り込めませんでした。" + Ymm.LastError; return; }
        SetThumbnail(selectedPreset, bmp, "YMM4のプレビュー画面をサムネイルにしました");
    }

    /// <summary>選択中のアイテムを、新しい図形として追加する(アイテムテンプレート方式)</summary>
    void AddShapeFromSelection()
    {
        if (shapeSlot < 1) return;
        var (json, count, error) = SelectionToTemplateJson("図形");
        if (json == null) { ShapeMessage = error; return; }

        var fp = ClipboardRaw.FingerprintText(json);
        var same = shapeStore.All(shapeSlot).FirstOrDefault(e => e.TemplateJson != null && ClipboardRaw.FingerprintText(e.TemplateJson) == fp);
        if (same != null)
        {
            ShapeMessage = $"同じ内容が「{same.Name}」にすでに登録されています(データID {fp})";
            return;
        }
        shapeStore.Add(shapeSlot, new ShapeEntry
        {
            IsCustom = true,
            Name = $"図形 {DateTime.Now:MM/dd HH:mm}",
            Source = "自分で追加",
            TemplateJson = json,
        });
        ShapeMessage = $"選択中の {count} 個のアイテムを、図形として追加しました(ダブルクリックでシークバーの位置に配置できます)";
    }

    /// <summary>選んでいる図形に、選択中のアイテムを「配置データ」として登録する(アイテムテンプレート方式)</summary>
    void AttachShapeTemplateFromSelection()
    {
        var s = selectedShape;
        if (s == null) { ShapeMessage = "図形を1つ選んでください"; return; }
        var (json, count, error) = SelectionToTemplateJson(s.Name);
        if (json == null) { ShapeMessage = error; return; }
        s.Entry.TemplateJson = json;
        shapeStore.Save();
        s.NotifyChanged();
        ShapeMessage = $"「{s.Name}」に、選択中の {count} 個のアイテムを配置データとして登録しました。ダブルクリックで配置できます";
    }

    void SetShapeThumbnailFromPreview()
    {
        var bmp = Ymm.CapturePreview();
        if (bmp == null) { ShapeMessage = "プレビュー画像を取り込めませんでした。" + Ymm.LastError; return; }
        SetShapeThumbnail(selectedShape, bmp, "YMM4のプレビュー画面をサムネイルにしました");
    }

    // ---------- クリップボードの診断(設定画面) ----------
    string clipboardDiagText = "";
    public string ClipboardDiagText { get => clipboardDiagText; private set => Set(ref clipboardDiagText, value); }

    // ---------- 図形(YMM4に入っている図形・プラグインで追加された図形・自分で追加した図形) ----------
    readonly ShapeStore shapeStore = ShapeStore.Instance;
    List<ShapeItem> selectedShapes = new();
    bool isShapeScanning;

    int shapeSlot => selectedKind == TabKind.Shape ? selectedSlot : 0;

    public ObservableCollection<ShapeItem> Shapes { get; } = new();
    public bool HasNoShapes => Shapes.Count == 0;

    string shapeSearch = "";
    /// <summary>図形タブの検索(名前・メモ・内部タグ)。一覧の表示だけを絞る</summary>
    public string ShapeSearch
    {
        get => shapeSearch;
        set
        {
            if (!Set(ref shapeSearch, value ?? "")) return;
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(Shapes);
            var q = shapeSearch.Trim();
            view.Filter = q.Length == 0 ? null : o => o is ShapeItem it && SearchTags.Matches(q, it.Entry.Key, it.Entry.Name, it.Entry.Memo, it.Entry.Source);
        }
    }

    string shapeMessage = "";
    public string ShapeMessage { get => shapeMessage; private set => Set(ref shapeMessage, value); }

    string shapeScanText = "";
    /// <summary>「標準 12 個 / プラグイン 5 個の図形を見つけました」</summary>
    public string ShapeScanText { get => shapeScanText; private set => Set(ref shapeScanText, value); }

    public string ShapeSelectionText => $"{selectedShapes.Count} / {Shapes.Count} 件を選択中";

    ShapeItem? selectedShape;
    public ShapeItem? SelectedShape
    {
        get => selectedShape;
        private set
        {
            if (!Set(ref selectedShape, value)) return;
            Raise(nameof(HasSelectedShape));
            Raise(nameof(HasNoSelectedShape));
        }
    }
    public bool HasSelectedShape => selectedShape != null;
    public bool HasNoSelectedShape => selectedShape == null;

    /// <summary>非表示にした図形も一覧に出す(表示に戻す操作用)</summary>
    public bool ShapeShowHidden
    {
        get => settings.ShapeShowHidden;
        set
        {
            if (settings.ShapeShowHidden == value) return;
            settings.ShapeShowHidden = value;
            settings.Save();
            Raise(nameof(ShapeShowHidden));
            ReloadShapes();
        }
    }

    void OnShapeStoreChanged(int slot)
    {
        if (slot == shapeSlot) ReloadShapes();
    }

    void ReloadShapes()
    {
        selectedShapes = new();
        SelectedShape = null;
        Shapes.Clear();
        ShapeMessage = "";
        if (shapeSlot >= 1)
        {
            var keys = ShapeDiscovery.Done ? ShapeDiscovery.Found.Select(f => f.Key).ToHashSet() : null;
            foreach (var e in shapeStore.All(shapeSlot))
            {
                if (e.Hidden && !settings.ShapeShowHidden) continue;
                if (!e.IsCustom && !settings.IsShapeShown(e.Key)) continue;   // 設定の「図形の表示」で外した図形
                var missing = !e.IsCustom && keys != null && !keys.Contains(e.Key);
                Shapes.Add(new ShapeItem(e, shapeStore, missing));
            }
        }
        Raise(nameof(HasNoShapes));
        Raise(nameof(ShapeSelectionText));
    }

    /// <summary>YMM4に読み込まれている図形を探して(初回・「もう一度探す」)、一覧に足す</summary>
    async Task EnsureShapesAsync(bool force)
    {
        if (shapeSlot < 1 || isShapeScanning) return;
        if (!ShapeDiscovery.Done || force)
        {
            isShapeScanning = true;
            ShapeScanText = "YMM4の図形を探しています…";
            try { await Task.Run(() => ShapeDiscovery.Scan()); }
            catch { }
            finally { isShapeScanning = false; }
        }

        var found = ShapeDiscovery.Found;
        ShapeScanText = found.Count == 0
            ? "YMM4の図形を見つけられませんでした。「クリップボードから図形を追加」で、自分で追加できます"
            : $"標準 {found.Count(f => f.IsBuiltIn)} 個 / プラグイン {found.Count(f => !f.IsBuiltIn)} 個の図形を見つけました";
        // 新しく見つかった図形があれば Changed で一覧が更新される。無ければ、見つからなくなった図形の表示だけ更新
        // 標準の図形は、「標準の図形をすべて自動収録」を押した時だけ収録する(タブごとに独立。収録後は削除・並べ替えができる)
        if (shapeSlot >= 1) ReloadShapes();
    }

    public void SetShapeSelection(IEnumerable<ShapeItem> sel)
    {
        selectedShapes = sel.ToList();
        SelectedShape = selectedShapes.Count == 1 ? selectedShapes[0] : null;
        Raise(nameof(ShapeSelectionText));
        CommandManager.InvalidateRequerySuggested();   // 削除などのボタンを、選んだらすぐ押せるようにする
    }

    public ShapeItem? FirstSelectedShape => selectedShapes.FirstOrDefault();

    /// <summary>いまクリップボードにある内容(YMM4でコピーした図形のアイテム)を、新しい図形として追加する</summary>
    public void CaptureShape(IntPtr hwnd)
    {
        if (shapeSlot < 1) return;
        var formats = ClipboardRaw.Capture(hwnd, out var error);
        if (formats == null) { ShapeMessage = error; return; }
        if (formats.Count == 0)
        {
            ShapeMessage = "クリップボードが空です。YMM4のタイムラインで図形のアイテムを選んで Ctrl+C してから押してください";
            return;
        }
        var sameShape = ClipboardRaw.FindSame(formats, shapeStore.All(shapeSlot).Select(e => (e.Name, e.Formats)));
        if (sameShape != null)
        {
            ShapeMessage = $"同じ内容が「{sameShape}」にすでに登録されています(データID {ClipboardRaw.Fingerprint(formats)})。"
                + "YMM4で別の図形のアイテムをコピーできているか確認してください";
            return;
        }
        shapeStore.Add(shapeSlot, new ShapeEntry
        {
            IsCustom = true,
            Name = $"図形 {DateTime.Now:MM/dd HH:mm}",
            Source = "自分で追加",
            Formats = formats,
        });
        ShapeMessage = "図形を追加しました(数値などはそのまま保存されています)";
    }

    /// <summary>選んでいる図形に、クリップボードの内容を「配置データ」として登録する(これでダブルクリックで配置できる)</summary>
    public void AttachShapeData(IntPtr hwnd)
    {
        var s = selectedShape;
        if (s == null) { ShapeMessage = "図形を1つ選んでください"; return; }
        var formats = ClipboardRaw.Capture(hwnd, out var error);
        if (formats == null) { ShapeMessage = error; return; }
        if (formats.Count == 0)
        {
            ShapeMessage = "クリップボードが空です。YMM4で「" + s.Name + "」のアイテムを選んで Ctrl+C してから押してください";
            return;
        }
        var sameData = ClipboardRaw.FindSame(formats, shapeStore.All(shapeSlot).Where(e => e.Id != s.Entry.Id).Select(e => (e.Name, e.Formats)));
        if (sameData != null)
        {
            ShapeMessage = $"同じ内容が「{sameData}」にすでに登録されています(データID {ClipboardRaw.Fingerprint(formats)})。"
                + $"YMM4で「{s.Name}」のアイテムをコピーし直してください";
            return;
        }
        s.Entry.Formats = formats;
        shapeStore.Save();
        s.NotifyChanged();
        ShapeMessage = $"「{s.Name}」に配置データを登録しました。ダブルクリックでタイムラインに配置できます";
    }

    /// <summary>図形をタイムラインに配置する(配置データをクリップボードに戻して、YMM4へ Ctrl+V を送る)</summary>
    public async Task PlaceShapeAsync(ShapeItem? s, IntPtr hwnd, object? ownView)
    {
        if (s == null) { ShapeMessage = "図形を選んでください"; return; }
        if (s.Entry.TemplateJson != null)
        {
            ShapeMessage = settings.UseYmmApi
                ? $"「{s.Name}」: " + await PlaceTemplateJsonAsync(s.Entry.TemplateJson)
                : "この図形を配置するには、設定で「YMM4の内部機能を使う」をオンにしてください";
            return;
        }
        if (!s.HasSnapshot && !s.IsCustom && settings.UseYmmApi && !string.IsNullOrEmpty(s.Entry.Key))
        {
            var builtinError = await PlaceBuiltinShapeAsync(s);
            if (builtinError == null) { ShapeMessage = $"「{s.Name}」を、シークバーの位置に配置しました"; return; }
            ShapeMessage = $"⚠ {builtinError}。従来の方法で配置します";
        }
        if (!s.HasSnapshot)
        {
            // 初期値のデータがまだ無い図形: YMM4で1回コピーするだけで自動登録する待ち受けを始める
            if (IsShapeCapturing)
                ShapeMessage = "いま別の図形の配置データを待っています。完了するか、中止してからもう一度お試しください";
            else
                StartShapeCapture(new[] { s });
            return;
        }
        if (!ClipboardRaw.RestoreVerified(hwnd, s.Entry.Formats!, out var error, out var dataId)) { ShapeMessage = error; return; }

        if (!settings.PresetAutoPaste)
        {
            ShapeMessage = $"「{s.Name}」をクリップボードにセットしました。タイムラインで Ctrl+V で貼り付けてください";
            return;
        }
        var skipped = await TimelinePaste.PasteAsync(ownView);
        ShapeMessage = skipped == null
            ? $"「{s.Name}」の貼り付け(Ctrl+V)をYMM4に送りました(データID {dataId})"
            : $"クリップボードにセットしました。{skipped}タイムラインをクリックして Ctrl+V で貼り付けてください";
    }

    // ---- 初期値の配置データを、YMM4でのコピーから自動で登録する ----
    // YMM4の図形は、YMM4で追加した直後の「初期値のまま」のアイテムをコピーすれば、そのままデータとして登録できます。
    // 登録待ちの間はクリップボードを見張り、図形のアイテムらしいコピーがあれば自動で登録して、次の図形に進みます。
    readonly DispatcherTimer shapeCaptureTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };
    Queue<ShapeItem> shapeCaptureQueue = new();
    ShapeItem? shapeCaptureTarget;
    uint shapeCaptureSeq, shapePendingSeq;
    DateTime shapeCaptureDeadline, shapePendingSince;
    int shapeCaptureTotal, shapeCaptureDone;

    public bool IsShapeCapturing => shapeCaptureTarget != null;

    string shapeCaptureText = "";
    public string ShapeCaptureText { get => shapeCaptureText; private set => Set(ref shapeCaptureText, value); }

    /// <summary>配置データがまだ無い図形を、順番に登録していく</summary>
    void StartShapeCapture(IEnumerable<ShapeItem> items)
    {
        var targets = items.Where(x => !x.HasSnapshot).ToList();
        if (targets.Count == 0) { ShapeMessage = "選んだ図形には、すべて配置データがあります"; return; }
        shapeCaptureQueue = new Queue<ShapeItem>(targets);
        shapeCaptureTotal = targets.Count;
        shapeCaptureDone = 0;
        NextShapeCapture();
    }

    void NextShapeCapture()
    {
        if (shapeCaptureQueue.Count == 0)
        {
            EndShapeCapture($"{shapeCaptureDone} 件の配置データを登録しました。これからはダブルクリックで、初期値のまま配置できます");
            return;
        }
        var target = shapeCaptureQueue.Dequeue();
        shapeCaptureTarget = target;
        shapeCaptureSeq = ClipboardRaw.SequenceNumber;      // これ以降のコピーだけを対象にする
        shapePendingSeq = 0;
        shapeCaptureDeadline = DateTime.Now.AddMinutes(3);
        ShapeCaptureText = $"({shapeCaptureDone + 1}/{shapeCaptureTotal}) YMM4で「{target.Name}」を追加してください(初期値のまま)。"
            + "追加したアイテムを選んで Ctrl+C を押すと、自動で登録されます。";
        Raise(nameof(IsShapeCapturing));
        shapeCaptureTimer.Start();
    }

    void EndShapeCapture(string message)
    {
        shapeCaptureTimer.Stop();
        shapeCaptureTarget = null;
        shapeCaptureQueue = new Queue<ShapeItem>();
        ShapeCaptureText = "";
        Raise(nameof(IsShapeCapturing));
        ShapeMessage = message;
    }

    void OnShapeCaptureTick()
    {
        var target = shapeCaptureTarget;
        if (target == null) { shapeCaptureTimer.Stop(); return; }
        if (DateTime.Now > shapeCaptureDeadline) { EndShapeCapture("3分たったので、登録を中止しました"); return; }

        var seq = ClipboardRaw.SequenceNumber;
        if (seq == shapeCaptureSeq) return;                 // まだコピーされていない
        // YMM4はコピー時にクリップボードを何度か書き換えることがあるので、変化が止まってから(約0.6秒後に)読み取る
        if (seq != shapePendingSeq) { shapePendingSeq = seq; shapePendingSince = DateTime.Now; return; }
        if (DateTime.Now - shapePendingSince < TimeSpan.FromMilliseconds(600)) return;

        var formats = ClipboardRaw.Capture(IntPtr.Zero, out _);
        if (formats == null) return;                        // 他のアプリが使用中。次の確認でやり直す
        shapeCaptureSeq = seq;
        shapePendingSeq = 0;

        if (!ClipboardRaw.LooksLikeData(formats))
        {
            ShapeMessage = "コピーされた内容が、YMM4のアイテムではないようです。図形のアイテムを選んで、もう一度 Ctrl+C してください";
            return;
        }

        var dup = ClipboardRaw.FindSame(formats, shapeStore.All(shapeSlot).Where(e => e.Id != target.Entry.Id).Select(e => (e.Name, e.Formats)));
        if (dup != null)
        {
            ShapeMessage = $"同じ内容が「{dup}」にすでに登録されています(データID {ClipboardRaw.Fingerprint(formats)})。"
                + $"YMM4で「{target.Name}」を追加して、そのアイテムをコピーしてください";
            return;
        }

        target.Entry.Formats = formats;
        shapeStore.Save();
        target.NotifyChanged();
        shapeCaptureDone++;
        ShapeMessage = $"「{target.Name}」の配置データを登録しました";
        NextShapeCapture();
    }

    void HideShapes(bool hide)
    {
        if (shapeSlot < 1 || selectedShapes.Count == 0) return;
        var n = selectedShapes.Count;
        shapeStore.SetHidden(shapeSlot, selectedShapes.Select(x => x.Entry.Id).ToList(), hide);
        ShapeMessage = hide
            ? $"{n} 件を非表示にしました(「非表示の図形も表示」で戻せます)"
            : $"{n} 件を表示に戻しました";
    }

    void RemoveSelectedShapes()
    {
        if (shapeSlot < 1 || selectedShapes.Count == 0) return;
        var ids = selectedShapes.Select(x => x.Entry.Id).ToList();
        var auto = selectedShapes.Count(x => !x.Entry.IsCustom);
        var removed = shapeStore.Remove(shapeSlot, ids);
        ShapeMessage = $"{removed} 件を削除しました"
            + (auto > 0 ? "(自動収録した図形は、もう一度「自動収録」しても入りません。戻す時は「削除した自動収録の図形を戻す」)" : "");
    }

    // ---- 並べ替え ----
    public event Action<IReadOnlyList<ShapeItem>>? ShapesReordered;

    void MoveShapes(string how)
    {
        if (shapeSlot < 1 || selectedShapes.Count == 0) return;
        var ids = selectedShapes.Select(x => x.Entry.Id).ToList();
        shapeStore.Move(shapeSlot, ids, how, settings.ShapeShowHidden);
        AfterShapeReorder(ids);
    }

    public string? ShapeReorderPayload()
        => shapeSlot < 1 || selectedShapes.Count == 0
            ? null
            : $"S{shapeSlot}\n" + string.Join("\n", selectedShapes.Select(x => x.Entry.Id));

    public bool TryReorderShapes(string payload, int insertIndex)
    {
        var lines = payload.Split('\n');
        if (lines.Length < 2 || lines[0] != $"S{shapeSlot}") return false;
        var ids = lines.Skip(1).ToList();
        var known = Shapes.Select(x => x.Entry.Id).ToHashSet();
        if (!ids.All(known.Contains)) return false;
        shapeStore.MoveTo(shapeSlot, ids, insertIndex, settings.ShapeShowHidden);
        AfterShapeReorder(ids);
        return true;
    }

    void AfterShapeReorder(List<string> ids)
    {
        var set = new HashSet<string>(ids);
        ShapesReordered?.Invoke(Shapes.Where(x => set.Contains(x.Entry.Id)).ToList());
    }

    // ---- サムネイル ----
    void SetShapeThumbnail(ShapeItem? s, BitmapSource? image, string okMessage)
    {
        if (s == null) { ShapeMessage = "図形を選んでください"; return; }
        ShapeMessage = s.SetThumbnail(image) ? okMessage : "画像を変換できませんでした";
    }

    public void SetShapeThumbnailFromClipboard()
    {
        try
        {
            if (!System.Windows.Clipboard.ContainsImage())
            {
                ShapeMessage = "クリップボードに画像がありません。画像をコピーしてから押してください";
                return;
            }
            SetShapeThumbnail(selectedShape, System.Windows.Clipboard.GetImage(), "クリップボードの画像をサムネイルにしました");
        }
        catch { ShapeMessage = "クリップボードの画像を読み取れませんでした"; }
    }

    public void SetShapeThumbnailFromFile(ShapeItem? s, string path)
    {
        var img = PresetThumb.LoadFile(path);
        if (img == null) { ShapeMessage = "画像を読み込めませんでした"; return; }
        SetShapeThumbnail(s, img, "画像をサムネイルにしました");
    }

    // ---------- プリセット(YMM4のタイムライン素材をコピーした内容を、そのまま保存) ----------
    readonly PresetStore presetStore = PresetStore.Instance;
    List<PresetItem> selectedPresets = new();

    int presetSlot => selectedKind == TabKind.Preset ? selectedSlot : 0;

    public ObservableCollection<PresetItem> Presets { get; } = new();
    public bool HasNoPresets => Presets.Count == 0;

    string presetMessage = "";
    public string PresetMessage { get => presetMessage; private set => Set(ref presetMessage, value); }

    public string PresetSelectionText => $"{selectedPresets.Count} / {Presets.Count} 件を選択中";

    PresetItem? selectedPreset;
    public PresetItem? SelectedPreset
    {
        get => selectedPreset;
        private set
        {
            if (!Set(ref selectedPreset, value)) return;
            Raise(nameof(HasSelectedPreset));
            Raise(nameof(HasNoSelectedPreset));
        }
    }
    public bool HasSelectedPreset => selectedPreset != null;
    public bool HasNoSelectedPreset => selectedPreset == null;

    void OnPresetStoreChanged(int slot)
    {
        if (slot == presetSlot) ReloadPresets();
    }

    void ReloadPresets()
    {
        selectedPresets = new();
        SelectedPreset = null;
        Presets.Clear();
        PresetMessage = "";
        if (presetSlot >= 1)
            foreach (var e in presetStore.All(presetSlot)) Presets.Add(new PresetItem(e, presetStore));
        Raise(nameof(HasNoPresets));
        Raise(nameof(PresetSelectionText));
        LoadPresetMedia();
    }

    public void SetPresetSelection(IEnumerable<PresetItem> sel)
    {
        selectedPresets = sel.ToList();
        SelectedPreset = selectedPresets.Count == 1 ? selectedPresets[0] : null;
        Raise(nameof(PresetSelectionText));
        LoadPresetMedia();
    }

    /// <summary>いまクリップボードにある内容(YMM4でコピーした素材)を、プリセットとして保存</summary>
    public void CapturePreset(IntPtr hwnd)
    {
        if (presetSlot < 1) return;
        var formats = ClipboardRaw.Capture(hwnd, out var error);
        if (formats == null) { PresetMessage = error; return; }
        if (formats.Count == 0)
        {
            PresetMessage = "クリップボードが空です。YMM4のタイムラインで素材を選んで Ctrl+C してから押してください";
            return;
        }

        var same = ClipboardRaw.FindSame(formats, presetStore.All(presetSlot).Select(e => (e.Name, (List<ClipFormat>?)e.Formats)));
        if (same != null)
        {
            PresetMessage = $"同じ内容のプリセット「{same}」がすでにあります(データID {ClipboardRaw.Fingerprint(formats)})。"
                + "YMM4で別のアイテムをコピーできているか確認してください";
            return;
        }
        var entry = new PresetEntry
        {
            Name = $"プリセット {DateTime.Now:MM/dd HH:mm}",
            Formats = formats,
        };
        presetStore.Add(presetSlot, entry);
        PresetMessage = formats.Any(f => f.Name != null)
            ? $"プリセットを作成しました({formats.Count} 形式をそのまま保存)"
            : "作成しました。ただしテキストだけです。YMM4の素材をコピーできているか確認してください";

        // その場で、含まれる素材からプレビュー動画をエンコードして保存
        if (settings.PresetAutoPreview && Presets.FirstOrDefault(x => x.Entry.Id == entry.Id) is { } created)
            _ = CreatePreviewAsync(created, true);
    }

    /// <summary>プリセットの内容をクリップボードに戻す(その後、YMM4のタイムラインで Ctrl+V)</summary>
    public void ApplyPreset(PresetItem? p, IntPtr hwnd)
    {
        if (p == null) { PresetMessage = "プリセットを選んでください"; return; }
        PresetMessage = ClipboardRaw.Restore(hwnd, p.Entry.Formats, out var error)
            ? $"「{p.Name}」をクリップボードにセットしました。YMM4のタイムラインで Ctrl+V で貼り付けてください"
            : error;
    }

    public PresetItem? FirstSelectedPreset => selectedPresets.FirstOrDefault();

    /// <summary>ダブルクリックで、YMM4にCtrl+Vを送って自動で貼り付ける</summary>
    public bool PresetAutoPaste
    {
        get => settings.PresetAutoPaste;
        set
        {
            if (settings.PresetAutoPaste == value) return;
            settings.PresetAutoPaste = value;
            settings.Save();
            Raise(nameof(PresetAutoPaste));
        }
    }

    /// <summary>
    /// プリセットをタイムラインに配置する。内容をクリップボードに戻し、YMM4へ Ctrl+V を送ります
    /// (貼り付け位置はYMM4の仕様に従い、シークバーの位置)。
    /// </summary>
    public async Task PlacePresetAsync(PresetItem? p, IntPtr hwnd, object? ownView)
    {
        if (p == null) { PresetMessage = "プリセットを選んでください"; return; }

        // YMM4のアイテムテンプレートで保存したプリセットは、クリップボードを使わず、確実にシークバーの位置へ配置する
        if (p.Entry.TemplateJson != null)
        {
            PresetMessage = settings.UseYmmApi
                ? $"「{p.Name}」: " + await PlaceTemplateJsonAsync(p.Entry.TemplateJson)
                : "このプリセットを配置するには、設定で「YMM4の内部機能を使う」をオンにしてください";
            return;
        }
        if (!ClipboardRaw.RestoreVerified(hwnd, p.Entry.Formats, out var error, out var dataId)) { PresetMessage = error; return; }

        if (!settings.PresetAutoPaste)
        {
            PresetMessage = $"「{p.Name}」をクリップボードにセットしました。タイムラインで Ctrl+V で貼り付けてください";
            return;
        }

        var skipped = await TimelinePaste.PasteAsync(ownView);
        PresetMessage = skipped == null
            ? $"「{p.Name}」の貼り付け(Ctrl+V)をYMM4に送りました(データID {dataId})"
            : $"クリップボードにセットしました。{skipped}タイムラインをクリックして Ctrl+V で貼り付けてください";
    }

    // ---------- プリセットのプレビュー動画(その場でエンコードして保存) ----------
    /// <summary>詳細パネルの再生をやめてファイルを解放してほしい時(上書き・削除の前)</summary>
    public event Action? ReleasePresetPlayer;

    bool isPresetEncoding;
    public bool IsPresetEncoding { get => isPresetEncoding; private set => Set(ref isPresetEncoding, value); }

    string presetEncodeText = "";
    public string PresetEncodeText { get => presetEncodeText; private set => Set(ref presetEncodeText, value); }

    /// <summary>プリセットを作った時に、自動でプレビュー動画を作る</summary>
    public bool PresetAutoPreview
    {
        get => settings.PresetAutoPreview;
        set
        {
            if (settings.PresetAutoPreview == value) return;
            settings.PresetAutoPreview = value;
            settings.Save();
            Raise(nameof(PresetAutoPreview));
        }
    }

    readonly SemaphoreSlim encodeGate = new(1, 1);

    /// <summary>
    /// プリセットに含まれる画像・動画をつなげてエンコードし、プレビュー動画として保存する。
    /// (YMM4側のエフェクトなどは含まれず、素材そのもののプレビューになります)
    /// </summary>
    public async Task CreatePreviewAsync(PresetItem? p, bool auto)
    {
        if (p == null) return;
        var entry = p.Entry;
        await encodeGate.WaitAsync();       // エンコードは1つずつ順番に
        try
        {
            IsPresetEncoding = true;
            PresetEncodeText = "素材を調べています…";
            var media = await Task.Run(() => PresetMediaFinder.Find(entry));
            if (media.Count == 0)
            {
                if (!auto) PresetMessage = "プレビューに使える画像・動画が見つかりませんでした。動画ファイルを登録することもできます";
                return;
            }

            ReleasePresetPlayer?.Invoke();      // 上書きするので、再生中ならファイルを解放
            var fileName = entry.Id + ".mp4";
            var progress = new Progress<double>(v =>
                PresetEncodeText = $"プレビューをエンコード中… {(v <= 1 ? v * 100 : v):0}%");
            var (ok, error) = await PresetPreviewEncoder.EncodeAsync(media, fileName, progress);
            if (!ok) { PresetMessage = error; return; }

            entry.PreviewFile = fileName;
            presetStore.Save();
            Presets.FirstOrDefault(x => x.Entry.Id == entry.Id)?.NotifyPreviewChanged();
            PresetMessage = $"「{entry.Name}」のプレビュー動画を作成して保存しました";
        }
        catch (Exception ex)
        {
            PresetMessage = "プレビューを作成できませんでした: " + ex.Message;
        }
        finally
        {
            IsPresetEncoding = false;
            PresetEncodeText = "";
            encodeGate.Release();
        }
    }

    /// <summary>手持ちの動画(YMM4で書き出したものなど)をプレビュー動画として登録する</summary>
    public void AttachPreviewFile(PresetItem? p, string path)
    {
        if (p == null) { PresetMessage = "プリセットを選んでください"; return; }
        try
        {
            ReleasePresetPlayer?.Invoke();
            Directory.CreateDirectory(PresetPreviewStore.Dir);
            var name = p.Entry.Id + System.IO.Path.GetExtension(path).ToLowerInvariant();
            if (!string.Equals(p.Entry.PreviewFile, name, StringComparison.OrdinalIgnoreCase))
                PresetPreviewStore.DeleteQuietly(p.Entry.PreviewFile);
            File.Copy(path, PresetPreviewStore.PathOf(name), true);
            p.SetPreviewFile(name);
            PresetMessage = "プレビュー動画を登録しました";
        }
        catch (Exception ex)
        {
            PresetMessage = "動画を登録できませんでした: " + ex.Message;
        }
    }

    void ClearPreview(PresetItem? p)
    {
        if (p == null) return;
        ReleasePresetPlayer?.Invoke();
        PresetPreviewStore.DeleteQuietly(p.Entry.PreviewFile);
        p.SetPreviewFile(null);
        PresetMessage = "プレビュー動画を削除しました";
    }

    public void ShowPresetMessage(string message) => PresetMessage = message;

    // ---------- プリセットの並べ替え ----------
    public event Action<IReadOnlyList<PresetItem>>? PresetsReordered;

    /// <summary>選んだプリセットを移動する。how = up / down / top / bottom</summary>
    void MovePresets(string how)
    {
        if (presetSlot < 1 || selectedPresets.Count == 0) return;
        var ids = selectedPresets.Select(p => p.Entry.Id).ToList();
        presetStore.Move(presetSlot, ids, how);
        AfterPresetReorder(ids);
    }

    public string? PresetReorderPayload()
        => presetSlot < 1 || selectedPresets.Count == 0
            ? null
            : $"P{presetSlot}\n" + string.Join("\n", selectedPresets.Select(p => p.Entry.Id));

    public bool TryReorderPresets(string payload, int insertIndex)
    {
        var lines = payload.Split('\n');
        if (lines.Length < 2 || lines[0] != $"P{presetSlot}") return false;
        var ids = lines.Skip(1).ToList();
        var known = Presets.Select(p => p.Entry.Id).ToHashSet();
        if (!ids.All(known.Contains)) return false;
        presetStore.MoveTo(presetSlot, ids, insertIndex);
        AfterPresetReorder(ids);
        return true;
    }

    void AfterPresetReorder(List<string> ids)
    {
        var set = new HashSet<string>(ids);
        PresetsReordered?.Invoke(Presets.Where(p => set.Contains(p.Entry.Id)).ToList());
    }

    /// <summary>再生中のプレビュー動画の、いま表示しているコマをサムネイルにする</summary>
    public void UseFrameAsThumbnail(PresetItem? p, BitmapSource frame)
        => SetThumbnail(p, frame, "選んだコマをサムネイルにしました");

    // ---------- プリセットのサムネイル ----------
    public ObservableCollection<MediaRef> PresetMedia { get; } = new();
    public bool HasNoPresetMedia => PresetMedia.Count == 0;
    int presetMediaToken;

    /// <summary>選んだプリセットの中で使われている画像・動画を探す(サムネイルの候補)</summary>
    async void LoadPresetMedia()
    {
        var token = ++presetMediaToken;
        PresetMedia.Clear();
        Raise(nameof(HasNoPresetMedia));
        var p = selectedPreset;
        if (p == null) return;

        var found = await Task.Run(() => PresetMediaFinder.Find(p.Entry));
        if (token != presetMediaToken) return;
        foreach (var m in found) PresetMedia.Add(m);
        Raise(nameof(HasNoPresetMedia));
    }

    void SetThumbnail(PresetItem? p, BitmapSource? image, string okMessage)
    {
        if (p == null) { PresetMessage = "プリセットを選んでください"; return; }
        PresetMessage = p.SetThumbnail(image) ? okMessage : "画像を変換できませんでした";
    }

    /// <summary>クリップボードの画像(スクリーンショットなど)をサムネイルにする</summary>
    public void SetThumbnailFromClipboard()
    {
        try
        {
            if (!System.Windows.Clipboard.ContainsImage())
            {
                PresetMessage = "クリップボードに画像がありません。画像をコピーしてから押してください";
                return;
            }
            SetThumbnail(selectedPreset, System.Windows.Clipboard.GetImage(), "クリップボードの画像をサムネイルにしました");
        }
        catch { PresetMessage = "クリップボードの画像を読み取れませんでした"; }
    }

    /// <summary>画像ファイルをサムネイルにする(ファイル選択・ドラッグ&ドロップ用)</summary>
    public void SetThumbnailFromFile(PresetItem? p, string path)
    {
        var img = PresetThumb.LoadFile(path);
        if (img == null) { PresetMessage = "画像を読み込めませんでした"; return; }
        SetThumbnail(p, img, "画像をサムネイルにしました");
    }

    /// <summary>プリセットに含まれる素材をサムネイルにする(画像はそのまま、動画はWindowsのサムネイル)</summary>
    async Task UseMediaAsThumbnailAsync(string path)
    {
        var p = selectedPreset;
        if (p == null) return;
        BitmapSource? img = PresetThumb.IsImageFile(path) ? PresetThumb.LoadFile(path) : null;
        img ??= await ShellThumbnail.GetAsync(path, 320);
        if (img == null) { PresetMessage = "この素材からはサムネイルを作れませんでした"; return; }
        SetThumbnail(p, img, $"「{System.IO.Path.GetFileName(path)}」をサムネイルにしました");
    }

    void RemoveSelectedPresets()
    {
        if (presetSlot < 1) return;
        var ids = selectedPresets.Select(x => x.Entry.Id).ToList();
        if (ids.Count == 0) return;
        ReleasePresetPlayer?.Invoke();      // 再生中のプレビュー動画のファイルを解放してから削除
        presetStore.Remove(presetSlot, ids);
        PresetMessage = $"{ids.Count} 件をプリセットから削除しました";
    }

    /// <summary>フォルダーは移動、写真・音楽・動画はプレビュー、その他は既定のアプリで開く</summary>
    public void Open(FileItem? f)
    {
        if (f == null) return;
        if (f.IsDirectory) { Navigate(f.FullPath); return; }
        if (f.CanPreview) { ShowPreview(f); return; }
        OpenExternal(f);
    }

    public void OpenExternal(FileItem? f)
    {
        if (f == null) return;
        try { Process.Start(new ProcessStartInfo(f.FullPath) { UseShellExecute = true }); }
        catch (Exception ex) { StatusText = ex.Message; }
    }

    // ---------- 検索・一覧 ----------
    async void ScheduleRefresh()
    {
        debounce?.Cancel();
        debounce = new CancellationTokenSource();
        var t = debounce.Token;
        try { await Task.Delay(250, t); }
        catch (TaskCanceledException) { return; }
        await RefreshAsync();
    }

    sealed record Query(string Path, string[] Terms, string[] Exts, DateTime? Since, string? Tag, bool Recursive)
    {
        public bool HasFilter => Terms.Length > 0 || Exts.Length > 0 || Since != null;

        public bool Match(FileSystemInfo f)
        {
            foreach (var t in Terms)
                if (f.Name.IndexOf(t, StringComparison.OrdinalIgnoreCase) < 0) return false;
            if (Exts.Length > 0)
            {
                if (f is DirectoryInfo) return false;
                var e = f.Extension.TrimStart('.').ToLowerInvariant();
                if (!Exts.Contains(e)) return false;
            }
            if (Since != null && f.LastWriteTime < Since) return false;
            return true;
        }
    }

    Query BuildQuery()
    {
        var terms = SearchText.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
        var exts = ExtFilter.Split(new[] { ' ', ',', ';', '　', '、' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim().TrimStart('*', '.').ToLowerInvariant())
                            .Where(x => x.Length > 0).ToArray();
        DateTime? since = DatePreset switch
        {
            1 => DateTime.Today,
            2 => DateTime.Today.AddDays(-7),
            3 => DateTime.Today.AddDays(-30),
            4 => DateTime.Today.AddYears(-1),
            _ => null,
        };
        var q = new Query(CurrentPath, terms, exts, since, TagFilter, false);
        return q with { Recursive = IncludeSub && q.HasFilter && TagFilter == null };
    }

    List<FileItem> Collect(Query q, CancellationToken ct)
    {
        var result = new List<FileItem>();
        IEnumerable<FileSystemInfo> source;
        if (q.Tag != null)
        {
            source = store.PathsWithTag(q.Tag).Select(p =>
                Directory.Exists(p) ? (FileSystemInfo)new DirectoryInfo(p) :
                File.Exists(p) ? new FileInfo(p) : null!).Where(x => x != null);
        }
        else
        {
            var opt = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = q.Recursive,
                AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
            };
            source = new DirectoryInfo(q.Path).EnumerateFileSystemInfos("*", opt);
        }

        foreach (var fsi in source)
        {
            ct.ThrowIfCancellationRequested();
            if (!q.Match(fsi)) continue;
            result.Add(FileItem.From(fsi, store));
            if (result.Count >= MaxResults) break;
        }
        return result;
    }

    async Task RefreshAsync()
    {
        if (string.IsNullOrEmpty(CurrentPath)) return;
        cts?.Cancel();
        cts = new CancellationTokenSource();
        var token = cts.Token;
        var q = BuildQuery();
        IsBusy = true;
        try
        {
            var list = await Task.Run(() => Collect(q, token), token);
            if (token.IsCancellationRequested) return;
            Items = Sort(list);
            var head = q.Tag != null ? $"タグ「{q.Tag}」: " : q.HasFilter ? "検索結果: " : "";
            var tail = list.Count >= MaxResults ? $"（上限 {MaxResults} 件で打ち切り）" : "";
            StatusText = $"{head}{list.Count} 個の項目{tail}";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { StatusText = ex.Message; }
        finally { if (!token.IsCancellationRequested) IsBusy = false; }
    }

    // ---------- 並べ替え ----------
    /// <summary>列ヘッダーのクリック: 同じ列なら昇順/降順を反転、別の列ならその列で昇順</summary>
    void ToggleSort(string key)
    {
        var i = Array.IndexOf(SortKeys, key);
        if (i < 0) return;
        if (i == sortIndex) SortDescending = !sortDesc;
        else
        {
            if (sortDesc) { sortDesc = false; settings.SortDescending = false; Raise(nameof(SortDescending)); }
            SortIndex = i;
        }
    }

    void ApplySort()
    {
        settings.Save();
        Raise(nameof(HeaderName)); Raise(nameof(HeaderModified));
        Raise(nameof(HeaderType)); Raise(nameof(HeaderSize));
        Items = Sort(Items.ToList());
    }

    List<FileItem> Sort(List<FileItem> list)
    {
        var o = foldersFirst ? list.OrderBy(i => i.IsDirectory ? 0 : 1) : list.OrderBy(_ => 0);
        o = sortKey switch
        {
            "Modified" => sortDesc ? o.ThenByDescending(i => i.Modified) : o.ThenBy(i => i.Modified),
            "Created" => sortDesc ? o.ThenByDescending(i => i.Created) : o.ThenBy(i => i.Created),
            "Type" => sortDesc ? o.ThenByDescending(i => i.TypeText, NaturalComparer.Instance)
                               : o.ThenBy(i => i.TypeText, NaturalComparer.Instance),
            "Size" => sortDesc ? o.ThenByDescending(i => i.Size) : o.ThenBy(i => i.Size),
            _ => sortDesc ? o.ThenByDescending(i => i.Name, NaturalComparer.Instance)
                          : o.ThenBy(i => i.Name, NaturalComparer.Instance),
        };
        // 同じ値同士は名前順にそろえる
        if (sortKey != "Name") o = o.ThenBy(i => i.Name, NaturalComparer.Instance);
        return o.ToList();
    }

    // ---------- 選択・詳細 ----------
    public void SetSelection(IEnumerable<FileItem> sel)
    {
        selected = sel.ToList();
        UpdateDetail();
    }

    void UpdateDetail()
    {
        Raise(nameof(HasSelection));
        Raise(nameof(HasNoSelection));
        Raise(nameof(HasCloudOnlySelected));
        detailToken++;
        if (selected.Count != 1 || detailMetaPath != selected[0].FullPath)
        {
            detailMeta = null;
            detailMetaPath = null;
        }

        if (selected.Count == 0)
        {
            DetailItem = null;
            DetailTitle = "";
            DetailRows.Clear();
            SelectedTags = Array.Empty<TagInfo>();
            return;
        }

        var f = selected[0];
        DetailGlyph = f.IconGlyph;
        DetailBrush = f.IconBrush;
        DetailItem = selected.Count == 1 ? f : null;
        DetailTitle = selected.Count == 1 ? f.Name : $"{selected.Count} 個の項目を選択中";
        SelectedTags = selected.SelectMany(s => s.Tags)
                               .DistinctBy(t => t.Name, StringComparer.OrdinalIgnoreCase).ToList();
        RebuildDetailRows();
        EnsureDetailMeta();
    }

    bool On(string id) => DetailOptions.FirstOrDefault(o => o.Id == id)?.IsChecked == true;

    void OnDetailOptionChanged()
    {
        settings.DetailFields = DetailOptions.Where(o => o.IsChecked).Select(o => o.Id).ToList();
        settings.Save();
        RebuildDetailRows();
        EnsureDetailMeta();
    }

    static string FmtDuration(TimeSpan t)
        => t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"mm\:ss");

    void RebuildDetailRows()
    {
        DetailRows.Clear();
        if (selected.Count == 0) return;

        if (selected.Count > 1)
        {
            var files = selected.Count(x => !x.IsDirectory);
            DetailRows.Add(new("項目", $"ファイル {files} 個 / フォルダー {selected.Count - files} 個"));
            DetailRows.Add(new("合計サイズ", FileItem.FormatSize(selected.Where(x => !x.IsDirectory).Sum(x => x.Size))));
            return;
        }

        var f = selected[0];
        var m = detailMeta;
        void Add(string id, string label, string? value)
        {
            if (!string.IsNullOrEmpty(value) && On(id)) DetailRows.Add(new(label, value));
        }

        Add("type", "種類", f.TypeText);
        Add("size", "サイズ", f.IsDirectory ? null : f.SizeText);
        Add("modified", "更新日時", f.ModifiedText);
        Add("created", "作成日時", f.CreatedText);
        Add("accessed", "アクセス日時", f.AccessedText);
        Add("ext", "拡張子", f.IsDirectory || f.Extension.Length == 0 ? null : f.Extension.ToUpperInvariant());
        Add("location", "場所", System.IO.Path.GetDirectoryName(f.FullPath));
        Add("path", "フルパス", f.FullPath);
        Add("attrs", "属性", f.AttributesText);
        Add("cloud", "クラウド", f.IsCloudOnly ? "クラウドのみ(開くとダウンロードされます)" : null);
        Add("dimensions", "サイズ(解像度)", m?.Width is int w && m.Height is int h ? $"{w} × {h}" : null);
        Add("duration", "再生時間", m?.Duration is TimeSpan t ? FmtDuration(t) : null);
        Add("framerate", "フレームレート", m?.FrameRate is double fr ? $"{fr:0.##} fps" : null);
        Add("bitrate", "ビットレート", m?.Bitrate is long br ? $"{br / 1000} kbps" : null);
        Add("audio", "音声",
            m?.SampleRate is int sr
                ? $"{sr / 1000.0:0.#} kHz" + (m.Channels is int ch ? $" / {ch}ch" : "")
                : m?.Channels is int ch2 ? $"{ch2}ch" : null);
        Add("pages", "ページ数", m?.PdfPages is int pg ? $"{pg} ページ" : null);
    }

    /// <summary>再生時間・解像度などは読み込みに時間がかかるため、選択後に裏で取得して追記する</summary>
    void EnsureDetailMeta()
    {
        if (selected.Count != 1 || detailMeta != null) return;
        var f = selected[0];
        if (f.IsDirectory || !(f.IsImage || f.IsVideo || f.IsAudio || f.IsPdf)) return;
        if (f.IsCloudOnly && !settings.CloudThumbnails) return;     // 読むとダウンロードが始まるため
        if (!(On("dimensions") || On("duration") || On("framerate") || On("bitrate") || On("audio") || On("pages"))) return;
        _ = LoadMetaAsync(f, detailToken);
    }

    async Task LoadMetaAsync(FileItem f, int token)
    {
        var m = await ShellProps.ReadAsync(f.FullPath) ?? new FileMeta();

        if (f.IsImage && m.Width == null)
        {
            try
            {
                var size = await Task.Run(() =>
                {
                    var fr = System.Windows.Media.Imaging.BitmapDecoder.Create(
                        new Uri(f.FullPath), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None).Frames[0];
                    return (fr.PixelWidth, fr.PixelHeight);
                });
                m.Width = size.PixelWidth;
                m.Height = size.PixelHeight;
            }
            catch { }
        }

        if (f.IsPdf && On("pages"))
        {
            try { m.PdfPages = (await PdfDoc.OpenAsync(f.FullPath)).PageCount; }
            catch { }
        }

        if (token != detailToken || selected.Count != 1 || selected[0] != f) return;
        detailMeta = m;
        detailMetaPath = f.FullPath;
        RebuildDetailRows();
    }

    // ---------- タグ ----------
    void ReloadTags()
    {
        AllTags = store.AllTags();
        Raise(nameof(HasNoTags));
    }

    void OnStoreChanged()
    {
        ReloadTags();
        foreach (var i in Items) i.ReloadTags(store);
        UpdateDetail();
        if (TagFilter != null) _ = RefreshAsync();
    }

    void AddTag(string? name)
    {
        name = (name ?? NewTagText).Trim();
        if (name.Length == 0 || selected.Count == 0) return;
        store.Add(selected.Select(s => s.FullPath), name);
        NewTagText = "";
    }

    void RemoveTag(string name)
    {
        if (selected.Count == 0) return;
        store.Remove(selected.Select(s => s.FullPath), name);
    }

    // ---------- その他 ----------
    void ShowInExplorer()
    {
        if (selected.Count == 0) return;
        try { Process.Start("explorer.exe", $"/select,\"{selected[0].FullPath}\""); }
        catch (Exception ex) { StatusText = ex.Message; }
    }

    /// <summary>Windows標準のプロパティ画面。選択中の項目、無ければ今開いているフォルダー</summary>
    void ShowProperties()
    {
        var path = selected.FirstOrDefault()?.FullPath ?? CurrentPath;
        if (string.IsNullOrEmpty(path)) return;
        if (!ShellActions.ShowProperties(path)) StatusText = "プロパティを開けませんでした";
    }

    void CopyPath()
    {
        if (selected.Count == 0) return;
        try { Clipboard.SetText(string.Join(Environment.NewLine, selected.Select(s => s.FullPath))); }
        catch (Exception ex) { StatusText = ex.Message; }
    }
}
