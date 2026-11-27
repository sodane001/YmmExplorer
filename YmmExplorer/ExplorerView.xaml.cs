using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YmmExplorer;

public partial class ExplorerView : UserControl
{
    readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    bool seeking, playing;
    ExplorerViewModel? hooked;

    // 一括選択(範囲選択・複数選択のままドラッグ)。エクスプローラー・テンプレート・プリセットで共通
    readonly ListSelector fileSelector, templateSelector, presetSelector, shapeSelector, videoFxSelector, audioFxSelector, textSelector;

    // 動画のコマを選ぶ小さなプレーヤー(プリセットのプレビュー動画 / テンプレートの動画ファイル)
    readonly FramePlayer presetFrames, templateFrames, materialFrames, viewerFrames;
    readonly ListSelector materialSelector;

    /// <summary>開いている YmmExplorer の画面(タブの裏に隠れていて、見えていない物も含む)</summary>
    internal static readonly List<WeakReference<ExplorerView>> Instances = new();

    public ExplorerView()
    {
        lock (Instances)
        {
            Instances.RemoveAll(w => !w.TryGetTarget(out _));
            Instances.Add(new WeakReference<ExplorerView>(this));
        }
        InitializeComponent();
        ApplyUiScale(ExplorerSettings.Instance.UiScale, false);
        fileSelector = new ListSelector(FileList, FileRubberRect)
        {
            // YMM4のタイムラインへ、選択したファイルをまとめてドラッグできます
            DragPaths = () => FileList.SelectedItems.Cast<FileItem>().Select(f => f.FullPath).ToArray(),
        };
        templateSelector = new ListSelector(TemplateList, RubberRect, TemplateInsertMark)
        {
            ReorderPayload = () => Vm?.TemplateReorderPayload(),
            DragPaths = () => TemplateList.SelectedItems.Cast<TemplateItem>()
                                          .Where(t => t.Item != null).Select(t => t.Path).ToArray(),
        };
        presetSelector = new ListSelector(PresetList, PresetRubberRect, PresetInsertMark)
        {
            ReorderPayload = () => Vm?.PresetReorderPayload(),
        };
        shapeSelector = new ListSelector(ShapeList, ShapeRubberRect, ShapeInsertMark)
        {
            ReorderPayload = () => Vm?.ShapeReorderPayload(),
        };
        videoFxSelector = new ListSelector(VideoFxList, VideoFxRubberRect, VideoFxInsertMark)
        {
            ReorderPayload = () => Vm?.VideoFx.ReorderPayload(),
        };
        audioFxSelector = new ListSelector(AudioFxList, AudioFxRubberRect, AudioFxInsertMark)
        {
            ReorderPayload = () => Vm?.AudioFx.ReorderPayload(),
        };
        textSelector = new ListSelector(TextLibList, TextLibRubberRect, TextLibInsertMark)
        {
            ReorderPayload = () => Vm?.TextLib.ReorderPayload(),
        };
        presetFrames = new FramePlayer(PresetPlayer, PresetSeek, PresetPlayBtn);
        templateFrames = new FramePlayer(TemplatePlayer, TemplateSeek, TemplatePlayBtn);
        materialFrames = new FramePlayer(MaterialPlayer, MaterialSeek, MaterialPlayBtn);
        viewerFrames = new FramePlayer(ViewerPlayer, ViewerSeek, ViewerPlayBtn) { AutoPlay = true, TimeLabel = ViewerTime };
        materialSelector = new ListSelector(MaterialList, MaterialRubberRect)
        {
            // 一覧から、YMM4のタイムラインへ素材ファイルをドラッグできます
            DragPaths = () => Vm?.Materials.SelectedPaths().ToArray() ?? Array.Empty<string>(),
        };
        timer.Tick += (_, _) => UpdateProgress();
        DataContextChanged += (_, e) =>
        {
            if (hooked != null)
            {
                hooked.PropertyChanged -= OnVmChanged;
                hooked.ReleasePresetPlayer -= ReleasePresetPlayer;
                hooked.TemplatesReordered -= OnTemplatesReordered;
                hooked.PresetsReordered -= OnPresetsReordered;
                hooked.ShapesReordered -= OnShapesReordered;
                hooked.VideoFx.Reordered -= OnVideoFxReordered;
                hooked.AudioFx.Reordered -= OnAudioFxReordered;
                hooked.TextLib.Reordered -= OnTextLibReordered;
                hooked.Materials.MediaRequested -= OnMaterialMedia;
                hooked.Materials.RestoreSelection -= OnMaterialRestoreSelection;
                hooked.Materials.ViewerMediaRequested -= OnViewerMedia;
                hooked.Materials.ViewerChanged -= ResetViewerZoom;
                hooked.FocusSearchRequested -= FocusSearchBox;
                hooked.AiEdit.MessageAdded -= ScrollAiToEnd;
                hooked.UiZoomRequested -= OnUiZoomRequested;
            }
            hooked = e.NewValue as ExplorerViewModel;
            if (hooked != null)
            {
                hooked.PropertyChanged += OnVmChanged;
                hooked.ReleasePresetPlayer += ReleasePresetPlayer;
                hooked.TemplatesReordered += OnTemplatesReordered;
                hooked.PresetsReordered += OnPresetsReordered;
                hooked.ShapesReordered += OnShapesReordered;
                hooked.VideoFx.Reordered += OnVideoFxReordered;
                hooked.AudioFx.Reordered += OnAudioFxReordered;
                hooked.TextLib.Reordered += OnTextLibReordered;
                hooked.Materials.MediaRequested += OnMaterialMedia;
                hooked.Materials.RestoreSelection += OnMaterialRestoreSelection;
                hooked.Materials.ViewerMediaRequested += OnViewerMedia;
                hooked.Materials.ViewerChanged += ResetViewerZoom;
                hooked.FocusSearchRequested += FocusSearchBox;
                hooked.AiEdit.MessageAdded -= ScrollAiToEnd;
                hooked.AiEdit.MessageAdded += ScrollAiToEnd;
                hooked.UiZoomRequested += OnUiZoomRequested;
                hooked.ItemEditor.IsEditing = () => ItemViewRoot.IsKeyboardFocusWithin && Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase;
            }
        };
        Unloaded += (_, _) =>
        {
            Vm?.ClosePreview();
            ShortcutHost.EarlyKeyDown -= OnEarlyKey;
            // 閉じた時は、再生中の動画・音声を止めて、ファイルとタイマーを放す
            foreach (var fp in new[] { presetFrames, templateFrames, materialFrames, viewerFrames }) { try { fp?.Release(); } catch { } }
            try { StopMedia(); } catch { }
        };
        Loaded += (_, _) => { ShortcutHost.EarlyKeyDown -= OnEarlyKey; ShortcutHost.EarlyKeyDown += OnEarlyKey; };
        // ショートカットでタブを移動する時、最後にさわったウィンドウ(YmmExplorer / ２ / ３…)を動かす
        PreviewMouseDown += (_, _) => ShortcutHost.Touch(Vm);
        IsKeyboardFocusWithinChanged += (_, e) => { if (e.NewValue is true) ShortcutHost.Touch(Vm); };
        Loaded += (_, _) => { if (IsVisible) ShortcutHost.Touch(Vm); };
    }

    /// <summary>
    /// アイテムタブのスライダーを離した時: 端に当てていたら、範囲を倍に広げる。
    /// 値の受け渡し(Delay=12)が終わってから調べるので、少し後で呼ぶ
    /// </summary>
    void RangeSlider_Release(object sender, MouseButtonEventArgs e)
    {
        var ctx = (sender as FrameworkElement)?.DataContext;
        if (sender is Slider sl) System.Windows.Data.BindingOperations.GetBindingExpression(sl, System.Windows.Controls.Primitives.RangeBase.ValueProperty)?.UpdateSource();
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (ctx is ItemPropRow row) row.ReleaseSlider();
            else if (ctx is ItemAnimPoint pt) pt.ReleaseSlider();
        }), DispatcherPriority.Background);
    }

    /// <summary>ショートカットキー欄の横の「削除」ボタン: 同じ行のキー欄を空にする(割り当ても解除される)</summary>
    void ClearKeyRecorder_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Parent is not Panel panel) return;
        foreach (var r in panel.Children.OfType<KeyRecorder>()) r.Keys = "";
    }

    // ---------- 操作ショートカット ----------
    void OnUiZoomRequested(int dir)
    {
        if (dir == 0) ApplyUiScale(1, true);
        else ApplyUiScale(UiScale.ScaleX + (dir > 0 ? 0.1 : -0.1), true);
    }

    /// <summary>今見えている検索欄(SearchText などに結び付いた入力欄)へカーソルを移す</summary>
    void FocusSearchBox()
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var box = FindVisibleSearchBox(this);
            if (box == null) return;
            box.Focus();
            Keyboard.Focus(box);
            box.SelectAll();
        }), System.Windows.Threading.DispatcherPriority.Input);
    }

    static TextBox? FindVisibleSearchBox(DependencyObject root)
    {
        int n = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < n; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is UIElement { IsVisible: false }) continue;
            if (child is TextBox tb && tb.IsEnabled)
            {
                // 入力欄の薄い説明(Tag)に「検索」とあるもの、または SearchText に結び付いたもの
                var path = System.Windows.Data.BindingOperations.GetBinding(tb, TextBox.TextProperty)?.Path?.Path ?? "";
                if ((tb.Tag as string)?.Contains("検索") == true || path.EndsWith("SearchText")) return tb;
            }
            var found = FindVisibleSearchBox(child);
            if (found != null) return found;
        }
        return null;
    }

    ExplorerViewModel? Vm => DataContext as ExplorerViewModel;

    // ---------- 幅に応じたレイアウト調整(狭いときの重なり防止) ----------
    void Root_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // 表示倍率を掛ける前の幅で判断する
        var w = e.NewSize.Width / Math.Max(0.1, UiScale.ScaleX);
        bool hideSide = w < 760;
        SideCol.Width = hideSide ? new GridLength(0) : new GridLength(w < 980 ? 160 : 190);
        SidePane.Visibility = hideSide ? Visibility.Collapsed : Visibility.Visible;
        DetailPane.Width = w < 980 ? 220 : 280;
    }

    // ---------- 一覧 ----------
    void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => Vm?.SetSelection(FileList.SelectedItems.Cast<FileItem>());

    ListBoxItem? ItemAt(DependencyObject? source)
        => source == null ? null : ItemsControl.ContainerFromElement(FileList, source) as ListBoxItem;

    // ダブルクリック: フォルダーは移動、写真・音楽・動画はプレビュー、その他は既定のアプリで開く
    void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemAt(e.OriginalSource as DependencyObject)?.DataContext is FileItem f)
            Vm?.Open(f);
    }

    // Enter: 開く / Space: プレビュー / Backspace: 上の階層へ
    void FileList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && FileList.SelectedItem is FileItem f)
        {
            Vm?.Open(f);
            e.Handled = true;
        }
        else if (e.Key == Key.System && e.SystemKey == Key.Enter)
        {
            // Alt + Enter: プロパティ
            var vm = Vm;
            if (vm != null && vm.ShowPropertiesCommand.CanExecute(null)) vm.ShowPropertiesCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Space && FileList.SelectedItem is FileItem { CanPreview: true } p)
        {
            Vm?.ShowPreview(p);
            e.Handled = true;
        }
        else if (e.Key == Key.Back)
        {
            Vm?.GoUp();
            e.Handled = true;
        }
    }

    // ---------- 画面全体の表示倍率(Ctrl + ホイール / Ctrl + 0 で100%) ----------
    void ApplyUiScale(double scale, bool save)
    {
        if (double.IsNaN(scale) || scale <= 0) scale = 1;
        scale = Math.Clamp(Math.Round(scale, 2), 0.6, 2.5);
        UiScale.ScaleX = UiScale.ScaleY = scale;
        if (!save) return;
        ExplorerSettings.Instance.UiScale = scale;
        ExplorerSettings.Instance.Save();
        Vm?.ShowStatus($"表示倍率: {scale * 100:0}%(Ctrl + ホイールで変更、Ctrl + 0 で 100% に戻す)");
    }

    void Root_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control) return;
        ApplyUiScale(UiScale.ScaleX + (e.Delta > 0 ? 0.05 : -0.05), true);
        e.Handled = true;
    }

    // Ctrl + Shift + ホイールで、ファイル一覧の表示サイズ変更(Ctrl + ホイールは、画面全体の表示倍率)
    void FileList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != (ModifierKeys.Control | ModifierKeys.Shift) || Vm == null) return;
        Vm.ViewMode += e.Delta > 0 ? 1 : -1;
        e.Handled = true;
    }

    // 右クリック: 未選択の項目なら、その項目だけを選択してからメニューを出す(複数選択中はそのまま)
    void FileList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var item = ItemAt(e.OriginalSource as DependencyObject);
        if (item == null) { FileList.UnselectAll(); return; }
        if (!item.IsSelected)
        {
            FileList.UnselectAll();
            item.IsSelected = true;
        }
    }

    void FileSelectAll_Click(object sender, RoutedEventArgs e)
    {
        FileList.Focus();
        FileList.SelectAll();
    }

    void FileClearSelection_Click(object sender, RoutedEventArgs e) => FileList.UnselectAll();

    // 並べ替えた後は、移動した項目を選択したままにする
    void OnTemplatesReordered(IReadOnlyList<TemplateItem> items)
    {
        TemplateList.SelectedItems.Clear();
        foreach (var i in items) TemplateList.SelectedItems.Add(i);
    }

    void OnPresetsReordered(IReadOnlyList<PresetItem> items)
    {
        PresetList.SelectedItems.Clear();
        foreach (var i in items) PresetList.SelectedItems.Add(i);
    }

    // ---------- テンプレートタブ ----------
    void TemplateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => Vm?.SetTemplateSelection(TemplateList.SelectedItems.Cast<TemplateItem>());

    ListBoxItem? TemplateItemAt(DependencyObject? source)
        => source == null ? null : ItemsControl.ContainerFromElement(TemplateList, source) as ListBoxItem;

    // ダブルクリック: プレビューできるものはプレビュー、フォルダーは場所を開く、その他は既定のアプリで開く
    void TemplateList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TemplateItemAt(e.OriginalSource as DependencyObject)?.DataContext is TemplateItem t)
            Vm?.OpenTemplate(t);
    }

    void TemplateList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var vm = Vm;
        if (vm == null) return;
        if (e.Key == Key.Delete && vm.RemoveTemplateCommand.CanExecute(null))
        {
            vm.RemoveTemplateCommand.Execute(null);
            e.Handled = true;
        }
        else if ((e.Key == Key.Enter || e.Key == Key.Space) && TemplateList.SelectedItem is TemplateItem t)
        {
            vm.OpenTemplate(t);
            e.Handled = true;
        }
        else if (e.Key == Key.System && (e.SystemKey == Key.Left || e.SystemKey == Key.Right))
        {
            // Alt + ← / → : 選んだ項目を前後へ移動
            vm.MoveTemplatesCommand.Execute(e.SystemKey == Key.Left ? "up" : "down");
            e.Handled = true;
        }
        else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) != 0
                 && vm.PasteTemplateThumbnailCommand.CanExecute(null) && Clipboard.ContainsImage())
        {
            // 画像をコピーしてある時は、選んだテンプレートのサムネイルとして貼り付け
            vm.PasteTemplateThumbnailCommand.Execute(null);
            e.Handled = true;
        }
    }

    void TemplateList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var item = TemplateItemAt(e.OriginalSource as DependencyObject);
        if (item == null) { TemplateList.UnselectAll(); return; }
        if (!item.IsSelected)
        {
            TemplateList.UnselectAll();
            item.IsSelected = true;
        }
    }

    void TemplateThumbFile_Click(object sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedTemplate is not { } t) return;
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "サムネイルにする画像を選択",
            Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp;*.tif;*.tiff|すべてのファイル|*.*",
        };
        if (dlg.ShowDialog() == true) Vm.SetTemplateThumbnailFromFile(t, dlg.FileName);
    }

    // テンプレートの動画の、いま表示しているコマをサムネイルにする
    void TemplateFrameThumb_Click(object sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedTemplate is not { } t) return;
        var (frame, error) = templateFrames.CaptureFrame();
        if (frame == null) { Vm.ShowTemplateMessage(error ?? "コマを取得できませんでした"); return; }
        Vm.UseFrameAsTemplateThumbnail(t, frame);
    }

    void TemplateSelectAll_Click(object sender, RoutedEventArgs e)
    {
        TemplateList.Focus();
        TemplateList.SelectAll();
    }

    void TemplateClearSelection_Click(object sender, RoutedEventArgs e) => TemplateList.UnselectAll();

    // タブが多い時は、マウスホイールで左右にスクロール
    void TabScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        TabScroll.ScrollToHorizontalOffset(TabScroll.HorizontalOffset - e.Delta);
        e.Handled = true;
    }

    // エクスプローラーなどからファイルをドロップして追加
    void TemplateList_DragOver(object sender, DragEventArgs e)
    {
        var reorder = e.Data.GetDataPresent(ListSelector.ReorderFormat);
        e.Effects = reorder || e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        // 並べ替えのドラッグ中は、入れる位置に縦線を出す
        if (reorder) templateSelector.ShowInsertMark(e.GetPosition(TemplateList));
        else templateSelector.HideInsertMark();
        e.Handled = true;
    }

    void TemplateList_DragLeave(object sender, DragEventArgs e) => templateSelector.HideInsertMark();

    void TemplateList_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        templateSelector.HideInsertMark();

        // このタブの中での並べ替え
        if (e.Data.GetData(ListSelector.ReorderFormat) is string payload
            && Vm?.TryReorderTemplates(payload, templateSelector.InsertIndexAt(e.GetPosition(TemplateList))) == true)
            return;

        // それ以外(エクスプローラーなどからのファイル)は、追加
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths) Vm?.AddTemplatePaths(paths);
    }

    void AddTemplateFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog { Multiselect = true, Title = "テンプレートに追加するファイルを選択" };
        if (dlg.ShowDialog() == true) Vm?.AddTemplatePaths(dlg.FileNames);
    }

    void AddTemplateFolder_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFolderDialog { Multiselect = true, Title = "テンプレートに追加するフォルダーを選択" };
        if (dlg.ShowDialog() == true) Vm?.AddTemplatePaths(dlg.FolderNames);
    }

    // ---------- 設定画面 ----------
    // エクスプローラータブの「開始フォルダー」をフォルダー選択で決める
    void ChooseStartFolder_Click(object sender, RoutedEventArgs e)
    {
        var ctx = (sender as FrameworkElement)?.DataContext;
        var tab = ctx as TabEntry ?? (ctx as TabToggle)?.Entry;
        if (tab == null) return;
        var dlg = new Microsoft.Win32.OpenFolderDialog { Title = $"「{tab.DisplayName}」の開始フォルダーを選択" };
        if (System.IO.Directory.Exists(tab.StartPath)) dlg.InitialDirectory = tab.StartPath;
        if (dlg.ShowDialog() == true) tab.StartPath = dlg.FolderName;
    }

    // ---------- 映像エフェクト・音声エフェクトのタブ(2つのタブで共通の処理) ----------
    // 画面側の DataContext が EffectLibraryVm なので、イベントを起こした部品から、どちらのタブかが分かります
    static EffectLibraryVm? Fx(object sender) => (sender as FrameworkElement)?.DataContext as EffectLibraryVm;

    ListBox FxListOf(EffectLibraryVm fx)
        => ReferenceEquals(fx, Vm?.VideoFx) ? VideoFxList : ReferenceEquals(fx, Vm?.AudioFx) ? AudioFxList : TextLibList;

    ListSelector FxSelectorOf(EffectLibraryVm fx)
        => ReferenceEquals(fx, Vm?.VideoFx) ? videoFxSelector : ReferenceEquals(fx, Vm?.AudioFx) ? audioFxSelector : textSelector;

    static void Reselect(ListBox list, IEnumerable<object> items)
    {
        list.SelectedItems.Clear();
        foreach (var i in items) list.SelectedItems.Add(i);
    }

    void OnVideoFxReordered(IReadOnlyList<EffectItem> items) => Reselect(VideoFxList, items);
    void OnAudioFxReordered(IReadOnlyList<EffectItem> items) => Reselect(AudioFxList, items);
    void OnTextLibReordered(IReadOnlyList<EffectItem> items) => Reselect(TextLibList, items);

    void FxList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox list && Fx(sender) is { } fx) fx.SetSelection(list.SelectedItems.Cast<EffectItem>());
    }

    void FxCapture_Click(object sender, RoutedEventArgs e) => Fx(sender)?.Capture(OwnerHwnd());

    // 選択中のアイテムのエフェクトを、1つのまとまりとして読み取る(YMM4へコピーのキー操作を送る)
    async void FxReadSelected_Click(object sender, RoutedEventArgs e)
    {
        if (Fx(sender) is { } fx) await fx.ReadSelectedAsync(this);
    }

    void FxReadClipboard_Click(object sender, RoutedEventArgs e) => Fx(sender)?.ReadFromClipboard();

    // 選んだ書式で、新しいテキストアイテムをシークバーの位置に配置
    async void FxPlaceNewText_Click(object sender, RoutedEventArgs e)
    {
        if (Fx(sender) is { } fx) await fx.PlaceNewTextAsync(fx.FirstSelected);
    }

    void FxSample_Click(object sender, RoutedEventArgs e) => Fx(sender)?.RegisterSample();

    // 選択中のアイテムに適用(YMM4へキー操作を送る)
    async void FxApply_Click(object sender, RoutedEventArgs e)
    {
        if (Fx(sender) is { } fx) await fx.ApplyAsync(fx.FirstSelected, OwnerHwnd(), this);
    }

    /// <summary>
    /// 映像エフェクト・音声エフェクト・テキストの一覧で、選んだ状態で Enter を押した時に適用する。
    /// YMM4より先に受け取る(YMM4が Enter を先に使うと、一覧の PreviewKeyDown まで届かないため)
    /// </summary>
    bool OnEarlyKey(KeyEventArgs e)
    {
        if (e.Key != Key.Enter && e.Key != Key.Return) return false;
        if (Keyboard.Modifiers != ModifierKeys.None || e.IsRepeat) return false;
        foreach (var list in new[] { VideoFxList, AudioFxList, TextLibList })
        {
            if (!list.IsVisible || list.SelectedItem is not EffectItem item || Fx(list) is not { } fx) continue;
            if (!list.IsKeyboardFocusWithin)
            {
                // 一覧を選んだあと、YMM4のタイムラインでアイテムを選んでから Enter を押した時も適用する。
                // ただし、文字の入力中・ボタンや選択欄の上・別の小さな画面(ダイアログ)では、YMM4の Enter を邪魔しない
                var focused = Keyboard.FocusedElement as DependencyObject;
                if (focused is System.Windows.Controls.Primitives.TextBoxBase or PasswordBox or System.Windows.Controls.Primitives.ButtonBase
                    or ComboBox or ComboBoxItem or MenuItem) return false;
                var w = focused != null ? Window.GetWindow(focused) : null;
                if (w != null && w != Application.Current?.MainWindow && w != Window.GetWindow(this)) return false;
                // 同じタブを開いたウィンドウが複数ある時は、最後にさわったウィンドウだけが受け取る
                if (Vm is { } vm && ShortcutHost.ActiveWindow > 0 && ShortcutHost.ActiveWindow != vm.WindowNumber) return false;
            }
            _ = fx.ApplyAsync(item, OwnerHwnd(), this);
            e.Handled = true;
            return true;
        }
        return false;
    }

    void FxList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBox list || Fx(sender) is not { } fx) return;
        if (ItemsControl.ContainerFromElement(list, e.OriginalSource as DependencyObject) is ListBoxItem { DataContext: EffectItem item })
            _ = fx.ApplyAsync(item, OwnerHwnd(), this);
    }

    void FxList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not ListBox list || Fx(sender) is not { } fx) return;
        if (e.Key == Key.Delete && fx.RemoveCommand.CanExecute(null))
        {
            fx.RemoveCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && list.SelectedItem is EffectItem item)
        {
            _ = fx.ApplyAsync(item, OwnerHwnd(), this);
            e.Handled = true;
        }
        else if (e.Key == Key.System && (e.SystemKey == Key.Left || e.SystemKey == Key.Right))
        {
            fx.MoveCommand.Execute(e.SystemKey == Key.Left ? "up" : "down");
            e.Handled = true;
        }
        else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) != 0
                 && fx.PasteThumbnailCommand.CanExecute(null) && Clipboard.ContainsImage())
        {
            fx.PasteThumbnailCommand.Execute(null);
            e.Handled = true;
        }
    }

    void FxSelectAll_Click(object sender, RoutedEventArgs e)
    {
        if (Fx(sender) is not { } fx) return;
        var list = FxListOf(fx);
        list.Focus();
        list.SelectAll();
    }

    void FxClearSelection_Click(object sender, RoutedEventArgs e)
    {
        if (Fx(sender) is { } fx) FxListOf(fx).UnselectAll();
    }

    void FxThumbFile_Click(object sender, RoutedEventArgs e)
    {
        var fx = Fx(sender);
        var item = fx?.SelectedItem;
        if (fx == null || item == null) return;
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "サムネイルにする画像を選択",
            Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp;*.tif;*.tiff|すべてのファイル|*.*",
        };
        if (dlg.ShowDialog() == true) fx.SetThumbnailFromFile(item, dlg.FileName);
    }

    // 並べ替えのドラッグ(挿入位置に縦線)と、画像ファイルのドロップ(サムネイル)
    void FxList_DragOver(object sender, DragEventArgs e)
    {
        if (sender is not ListBox list || Fx(sender) is not { } fx) return;
        var reorder = e.Data.GetDataPresent(ListSelector.ReorderFormat);
        e.Effects = reorder || e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        if (reorder) FxSelectorOf(fx).ShowInsertMark(e.GetPosition(list));
        else FxSelectorOf(fx).HideInsertMark();
        e.Handled = true;
    }

    void FxList_DragLeave(object sender, DragEventArgs e)
    {
        if (Fx(sender) is { } fx) FxSelectorOf(fx).HideInsertMark();
    }

    void FxList_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        if (sender is not ListBox list || Fx(sender) is not { } fx) return;
        var selector = FxSelectorOf(fx);
        selector.HideInsertMark();

        if (e.Data.GetData(ListSelector.ReorderFormat) is string payload)
        {
            fx.TryReorder(payload, selector.InsertIndexAt(e.GetPosition(list)));
            return;
        }

        // 画像ファイルをタイルへドロップ: そのエフェクトのサムネイルにする
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
        var image = files.FirstOrDefault(PresetThumb.IsImageFile);
        if (image == null) return;
        var container = ItemsControl.ContainerFromElement(list, e.OriginalSource as DependencyObject) as ListBoxItem;
        var target = container?.DataContext as EffectItem ?? fx.SelectedItem;
        if (container != null && !container.IsSelected)
        {
            list.UnselectAll();
            container.IsSelected = true;
        }
        fx.SetThumbnailFromFile(target, image);
    }

    // ---------- 図形タブ ----------
    void OnShapesReordered(IReadOnlyList<ShapeItem> items)
    {
        ShapeList.SelectedItems.Clear();
        foreach (var i in items) ShapeList.SelectedItems.Add(i);
    }

    void ShapeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => Vm?.SetShapeSelection(ShapeList.SelectedItems.Cast<ShapeItem>());

    void CaptureShape_Click(object sender, RoutedEventArgs e) => Vm?.CaptureShape(OwnerHwnd());

    void AttachShapeData_Click(object sender, RoutedEventArgs e) => Vm?.AttachShapeData(OwnerHwnd());

    // 図形をタイムライン(YMM4のシークバーの位置)へ配置
    async void PlaceShape_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is { } vm) await vm.PlaceShapeAsync(vm.FirstSelectedShape, OwnerHwnd(), this);
    }

    void ShapeList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement(ShapeList, e.OriginalSource as DependencyObject) is ListBoxItem { DataContext: ShapeItem s })
            _ = Vm?.PlaceShapeAsync(s, OwnerHwnd(), this);
    }

    void ShapeList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var vm = Vm;
        if (vm == null) return;
        if (e.Key == Key.Delete && vm.RemoveShapesCommand.CanExecute(null))
        {
            vm.RemoveShapesCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && ShapeList.SelectedItem is ShapeItem s)
        {
            _ = vm.PlaceShapeAsync(s, OwnerHwnd(), this);
            e.Handled = true;
        }
        else if (e.Key == Key.System && (e.SystemKey == Key.Left || e.SystemKey == Key.Right))
        {
            // Alt + ← / → : 選んだ項目を前後へ移動
            vm.MoveShapesCommand.Execute(e.SystemKey == Key.Left ? "up" : "down");
            e.Handled = true;
        }
        else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) != 0
                 && vm.PasteShapeThumbnailCommand.CanExecute(null) && Clipboard.ContainsImage())
        {
            vm.PasteShapeThumbnailCommand.Execute(null);
            e.Handled = true;
        }
    }

    void ShapeSelectAll_Click(object sender, RoutedEventArgs e)
    {
        ShapeList.Focus();
        ShapeList.SelectAll();
    }

    void ShapeClearSelection_Click(object sender, RoutedEventArgs e) => ShapeList.UnselectAll();

    void ShapeThumbFile_Click(object sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedShape is not { } s) return;
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "サムネイルにする画像を選択",
            Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp;*.tif;*.tiff|すべてのファイル|*.*",
        };
        if (dlg.ShowDialog() == true) Vm.SetShapeThumbnailFromFile(s, dlg.FileName);
    }

    // 並べ替えのドラッグ(挿入位置に縦線を出す)と、画像ファイルのドロップ(サムネイル)
    void ShapeList_DragOver(object sender, DragEventArgs e)
    {
        var reorder = e.Data.GetDataPresent(ListSelector.ReorderFormat);
        e.Effects = reorder || e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        if (reorder) shapeSelector.ShowInsertMark(e.GetPosition(ShapeList));
        else shapeSelector.HideInsertMark();
        e.Handled = true;
    }

    void ShapeList_DragLeave(object sender, DragEventArgs e) => shapeSelector.HideInsertMark();

    void ShapeList_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        shapeSelector.HideInsertMark();

        if (e.Data.GetData(ListSelector.ReorderFormat) is string payload)
        {
            Vm?.TryReorderShapes(payload, shapeSelector.InsertIndexAt(e.GetPosition(ShapeList)));
            return;
        }

        // 画像ファイルをタイルへドロップ: その図形のサムネイルにする
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
        var image = files.FirstOrDefault(PresetThumb.IsImageFile);
        if (image == null) return;
        var container = ItemsControl.ContainerFromElement(ShapeList, e.OriginalSource as DependencyObject) as ListBoxItem;
        var target = container?.DataContext as ShapeItem ?? Vm?.SelectedShape;
        if (container != null && !container.IsSelected)
        {
            ShapeList.UnselectAll();
            container.IsSelected = true;
        }
        Vm?.SetShapeThumbnailFromFile(target, image);
    }

    // ---------- プリセットタブ ----------
    // クリップボードの読み書きには、このビューが載っているウィンドウのハンドルが必要です
    IntPtr OwnerHwnd()
        => (PresentationSource.FromVisual(this) as HwndSource)?.Handle ?? IntPtr.Zero;

    void CapturePreset_Click(object sender, RoutedEventArgs e) => Vm?.CapturePreset(OwnerHwnd());

    void ApplyPreset_Click(object sender, RoutedEventArgs e) => Vm?.ApplyPreset(Vm.FirstSelectedPreset, OwnerHwnd());

    // タイムライン(YMM4のシークバーの位置)へ配置
    async void PlacePreset_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is { } vm) await vm.PlacePresetAsync(vm.FirstSelectedPreset, OwnerHwnd(), this);
    }

    void PresetThumbFile_Click(object sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedPreset is not { } p) return;
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "サムネイルにする画像を選択",
            Filter = "画像ファイル|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.webp;*.tif;*.tiff|すべてのファイル|*.*",
        };
        if (dlg.ShowDialog() == true) Vm.SetThumbnailFromFile(p, dlg.FileName);
    }

    // 画像ファイルをタイルへドロップ → そのプリセットのサムネイルにする
    void PresetList_DragOver(object sender, DragEventArgs e)
    {
        var reorder = e.Data.GetDataPresent(ListSelector.ReorderFormat);
        e.Effects = reorder || e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        if (reorder) presetSelector.ShowInsertMark(e.GetPosition(PresetList));
        else presetSelector.HideInsertMark();
        e.Handled = true;
    }

    void PresetList_DragLeave(object sender, DragEventArgs e) => presetSelector.HideInsertMark();

    void PresetList_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        presetSelector.HideInsertMark();

        // このタブの中での並べ替え
        if (e.Data.GetData(ListSelector.ReorderFormat) is string payload)
        {
            Vm?.TryReorderPresets(payload, presetSelector.InsertIndexAt(e.GetPosition(PresetList)));
            return;
        }

        // 画像ファイルをドロップ: サムネイルにする
        if (e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
        var image = files.FirstOrDefault(PresetThumb.IsImageFile);
        if (image == null) return;      // 画像以外は無視

        // ドロップした位置のタイルを対象にする(タイルの外なら、選択中のプリセット)
        var container = ItemsControl.ContainerFromElement(PresetList, e.OriginalSource as DependencyObject) as ListBoxItem;
        var target = container?.DataContext as PresetItem ?? Vm?.SelectedPreset;
        if (container != null && !container.IsSelected)
        {
            PresetList.UnselectAll();
            container.IsSelected = true;
        }
        Vm?.SetThumbnailFromFile(target, image);
    }

    void PresetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => Vm?.SetPresetSelection(PresetList.SelectedItems.Cast<PresetItem>());

    void PresetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ItemsControl.ContainerFromElement(PresetList, e.OriginalSource as DependencyObject) is ListBoxItem { DataContext: PresetItem p })
            _ = Vm?.PlacePresetAsync(p, OwnerHwnd(), this);
    }

    void PresetList_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        var vm = Vm;
        if (vm == null) return;
        if (e.Key == Key.Delete && vm.RemovePresetCommand.CanExecute(null))
        {
            vm.RemovePresetCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && PresetList.SelectedItem is PresetItem p)
        {
            _ = vm.PlacePresetAsync(p, OwnerHwnd(), this);
            e.Handled = true;
        }
        else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) != 0
                 && vm.PasteThumbnailCommand.CanExecute(null) && Clipboard.ContainsImage())
        {
            // 画像をコピーしてある時は、選んだプリセットのサムネイルとして貼り付け
            vm.PasteThumbnailCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.System && (e.SystemKey == Key.Left || e.SystemKey == Key.Right))
        {
            // Alt + ← / → : 選んだ項目を前後へ移動
            vm.MovePresetsCommand.Execute(e.SystemKey == Key.Left ? "up" : "down");
            e.Handled = true;
        }
    }

    void PresetSelectAll_Click(object sender, RoutedEventArgs e)
    {
        PresetList.Focus();
        PresetList.SelectAll();
    }

    void PresetClearSelection_Click(object sender, RoutedEventArgs e) => PresetList.UnselectAll();

    // ---------- プリセットのプレビュー動画(詳細パネルで再生・コマ選択) ----------
    PresetItem? watchedPreset;

    void WatchPreset(PresetItem? p)
    {
        if (watchedPreset != null) watchedPreset.PropertyChanged -= OnWatchedPresetChanged;
        watchedPreset = p;
        if (p != null) p.PropertyChanged += OnWatchedPresetChanged;
        presetFrames.Load(p?.PreviewPath);
    }

    void OnWatchedPresetChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PresetItem.PreviewPath)) presetFrames.Load(watchedPreset?.PreviewPath);
    }

    /// <summary>再生を止めて、動画ファイルを解放する(上書き・削除の前に呼ばれます)</summary>
    void ReleasePresetPlayer() => presetFrames.Release();

    // いま表示しているコマをサムネイルにする
    void PresetFrameThumb_Click(object sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedPreset is not { } p) return;
        var (frame, error) = presetFrames.CaptureFrame();
        if (frame == null) { Vm.ShowPresetMessage(error ?? "コマを取得できませんでした"); return; }
        Vm.UseFrameAsThumbnail(p, frame);
    }

    void PresetPreviewFile_Click(object sender, RoutedEventArgs e)
    {
        if (Vm?.SelectedPreset is not { } p) return;
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title = "プレビュー動画にするファイルを選択",
            Filter = "動画ファイル|*.mp4;*.mov;*.m4v;*.wmv;*.avi;*.mkv;*.webm|すべてのファイル|*.*",
        };
        if (dlg.ShowDialog() == true) Vm.AttachPreviewFile(p, dlg.FileName);
    }

    // ---------- 素材データベース ----------
    void OnMaterialMedia(string? path) => materialFrames.Load(path);

    void OnMaterialRestoreSelection(IReadOnlyList<MaterialItem> items)
    {
        MaterialList.SelectedItems.Clear();
        foreach (var i in items) MaterialList.SelectedItems.Add(i);
    }

    void MaterialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        => Vm?.Materials.SetSelection(MaterialList.SelectedItems.Cast<MaterialItem>());

    void MaterialList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        // ダブルクリックで、プラグインの中の大きなプレビューを開く(外のアプリには飛ばさない)
        if (ItemsControl.ContainerFromElement(MaterialList, e.OriginalSource as DependencyObject) is ListBoxItem { DataContext: MaterialItem item })
            Vm?.Materials.OpenViewer(item);
    }

    void MaterialList_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (Vm != null) Vm.Materials.ViewportWidth = MaterialList.ActualWidth;
    }

    // ---------- AI音声生成(Audio Studio MCP) ----------
    void StudioChooseExe_Click(object sender, RoutedEventArgs e)
    {
        var studio = Vm?.Materials.Studio;
        if (studio == null) return;
        var dlg = new Microsoft.Win32.OpenFileDialog { Title = "audio-studio-mcp.exe を選択", Filter = "実行ファイル (*.exe)|*.exe|すべてのファイル (*.*)|*.*" };
        if (dlg.ShowDialog() == true) studio.ExePath = dlg.FileName;
    }

    void StudioChooseOutput_Click(object sender, RoutedEventArgs e)
    {
        var studio = Vm?.Materials.Studio;
        if (studio == null) return;
        var dlg = new Microsoft.Win32.OpenFolderDialog { Title = "Audio Studio MCP の出力フォルダーを選択" };
        if (dlg.ShowDialog() == true) studio.OutputDir = dlg.FolderName;
    }

    // ---------- AIエディット ----------
    void AiSaveKey_Click(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        Vm.AiEdit.ApiKey = AiKeyBox.Password;
        AiKeyBox.Password = "";
        Vm.AiEdit.Status = Vm.AiEdit.ApiKey.Length > 0 ? "APIキーを保存しました" : "APIキーを消しました";
    }

    /// <summary>Enter で送信、Shift+Enter で改行(日本語の変換中の Enter は、確定に使う)</summary>
    void AiInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (AiPasteAttachment()) e.Handled = true;
            return;
        }
        if (e.Key != Key.Enter || e.IsRepeat) return;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) return;
        if (e.ImeProcessedKey != Key.None || e.Key == Key.ImeProcessed) return;
        if (sender is TextBox tb) tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        if (Vm?.AiEdit.SendCommand.CanExecute(null) == true) Vm.AiEdit.SendCommand.Execute(null);
        e.Handled = true;
    }

    void AiAddAllowedFolder_Click(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        var dlg = new Microsoft.Win32.OpenFolderDialog { Multiselect = true, Title = "AI に見せて、配置に使ってよい素材フォルダーを選んでください" };
        if (dlg.ShowDialog() == true) Vm.AiEdit.AddAllowedFolders(dlg.FolderNames);
    }

    void AiAttachFile_Click(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        var dlg = new Microsoft.Win32.OpenFileDialog { Multiselect = true, Title = "AI に送るファイルを選んでください(画像・テキスト・字幕など)" };
        if (dlg.ShowDialog() == true) Vm.AiEdit.AddAttachments(dlg.FileNames);
    }

    void AiAttachFolder_Click(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        var dlg = new Microsoft.Win32.OpenFolderDialog { Multiselect = true, Title = "AI に送るフォルダーを選んでください" };
        if (dlg.ShowDialog() == true) Vm.AiEdit.AddAttachments(dlg.FolderNames);
    }

    // 入力欄・会話へのドラッグ&ドロップで添付
    void AiDrop_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    void AiDrop_Drop(object sender, DragEventArgs e)
    {
        if (Vm == null || e.Data.GetData(DataFormats.FileDrop) is not string[] files) return;
        Vm.AiEdit.AddAttachments(files);
        e.Handled = true;
    }

    /// <summary>Ctrl+V: クリップボードの画像・ファイルを添付する(文字なら、ふつうに貼り付け)</summary>
    bool AiPasteAttachment()
    {
        if (Vm == null) return false;
        try
        {
            if (Clipboard.ContainsFileDropList())
            {
                Vm.AiEdit.AddAttachments(Clipboard.GetFileDropList().Cast<string>());
                return true;
            }
            if (Clipboard.ContainsImage() && !Clipboard.ContainsText())
            {
                var img = Clipboard.GetImage();
                if (img == null) return false;
                var dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "YmmExplorer", "ai-paste");
                System.IO.Directory.CreateDirectory(dir);
                var path = System.IO.Path.Combine(dir, $"貼り付け-{DateTime.Now:yyyyMMdd-HHmmss}.png");
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(img));
                using (var fs = System.IO.File.Create(path)) enc.Save(fs);
                Vm.AiEdit.AddAttachments(new[] { path });
                return true;
            }
        }
        catch (Exception ex) { Vm.AiEdit.Status = "貼り付けた画像を添付できませんでした: " + ex.Message; }
        return false;
    }

    void ScrollAiToEnd() => Dispatcher.BeginInvoke(new Action(() => AiScroll.ScrollToEnd()), DispatcherPriority.Background);

    // ---------- アイテムタブ ----------
    /// <summary>
    /// アイテムタブの文字の入力欄: 日本語入力の変換中(確定前)の文字も、プレビューへ送る。
    /// WPF は変換が確定するまで、入力欄の文字を YMM4 側へ渡さないため、変換の途中経過を自分で拾う
    /// </summary>
    void ItemTextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb || !imeHooked.Add(tb)) return;
        TextCompositionManager.AddPreviewTextInputUpdateHandler(tb, (_, ev) => OnItemComposition(tb, ev.TextComposition?.CompositionText ?? ""));
        TextCompositionManager.AddTextInputUpdateHandler(tb, (_, ev) => OnItemComposition(tb, ev.TextComposition?.CompositionText ?? ""));
        tb.PreviewTextInput += (_, _) => EndItemComposition(tb, true);       // 確定
        tb.LostKeyboardFocus += (_, _) => EndItemComposition(tb, false);
    }

    readonly HashSet<TextBox> imeHooked = new();

    void OnItemComposition(TextBox tb, string composition)
    {
        if (tb.DataContext is not ItemPropRow row) return;
        // 入力欄の中身が変換中の文字を反映してから読む
        Dispatcher.BeginInvoke(new Action(() =>
        {
            var text = tb.Text ?? "";
            if (composition.Length > 0 && !text.Contains(composition, StringComparison.Ordinal))
                text = text.Insert(Math.Clamp(tb.CaretIndex, 0, text.Length), composition);
            if (composition.Length == 0) { EndItemComposition(tb, false); return; }     // 変換を取り消した
            row.PreviewComposing(text);
        }), DispatcherPriority.Input);
    }

    void EndItemComposition(TextBox tb, bool committed)
    {
        if (tb.DataContext is not ItemPropRow row || !row.IsComposing) return;
        // 確定した時は、入力欄の文字が変わってから合わせる
        Dispatcher.BeginInvoke(new Action(() => row.EndComposing(tb.Text ?? "")), committed ? DispatcherPriority.Background : DispatcherPriority.Input);
    }

    /// <summary>項目の「…」ボタン: デフォルトに戻す・設定するメニューを開く</summary>
    void ItemRowMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.ContextMenu == null) return;
        b.ContextMenu.PlacementTarget = b;
        b.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        b.ContextMenu.IsOpen = true;
    }

    /// <summary>
    /// 複数行の入力欄(文字・コメント・選択肢など): Enter で必ず改行する。
    /// YMM4やほかのショートカットに Enter を取られて改行できないことがあるため、自分で改行を入れる(日本語の変換中の Enter は確定に使う)
    /// </summary>
    void ItemMultiline_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox box || (e.Key != Key.Return && e.Key != Key.Enter)) return;
        if (e.ImeProcessedKey != Key.None || e.Key == Key.ImeProcessed) return;
        if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != 0) return;
        int at = box.SelectionStart;
        box.SelectedText = Environment.NewLine;
        box.SelectionLength = 0;
        box.CaretIndex = at + Environment.NewLine.Length;
        e.Handled = true;
    }

    /// <summary>入力欄で Enter を押したら、その場で値を反映する</summary>
    void ItemField_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return || sender is not TextBox box) return;
        box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        box.SelectAll();
        e.Handled = true;
    }

    // ---------- 素材データベース: 大きなプレビュー ----------
    void OnViewerMedia(string? path) => viewerFrames.Load(path);

    void ResetViewerZoom()
    {
        ViewerScale.ScaleX = ViewerScale.ScaleY = 1;
        ViewerShift.X = ViewerShift.Y = 0;
    }

    bool viewerPanning;
    Point viewerPanStart;

    void ViewerStage_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Vm?.Materials.ViewerCanZoom != true) return;
        var p = e.GetPosition(ViewerImage);
        double old = ViewerScale.ScaleX;
        double next = Math.Clamp(old * (e.Delta > 0 ? 1.2 : 1 / 1.2), 1, 16);
        if (next <= 1.001) { ResetViewerZoom(); e.Handled = true; return; }
        // マウスの下の点が動かないように、位置をずらす
        ViewerShift.X += p.X * (old - next);
        ViewerShift.Y += p.Y * (old - next);
        ViewerScale.ScaleX = ViewerScale.ScaleY = next;
        e.Handled = true;
    }

    void ViewerStage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Vm?.Materials.ViewerCanZoom != true) return;
        if (e.ClickCount == 2) { ResetViewerZoom(); return; }
        if (ViewerScale.ScaleX <= 1.001) return;
        viewerPanning = true;
        viewerPanStart = e.GetPosition(ViewerStage);
        ViewerStage.CaptureMouse();
    }

    void ViewerStage_MouseMove(object sender, MouseEventArgs e)
    {
        if (!viewerPanning) return;
        var p = e.GetPosition(ViewerStage);
        ViewerShift.X += p.X - viewerPanStart.X;
        ViewerShift.Y += p.Y - viewerPanStart.Y;
        viewerPanStart = p;
    }

    void ViewerStage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!viewerPanning) return;
        viewerPanning = false;
        ViewerStage.ReleaseMouseCapture();
    }

    void MaterialArea_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    async void MaterialArea_Drop(object sender, DragEventArgs e)
    {
        e.Handled = true;
        if (Vm == null || e.Data.GetData(DataFormats.FileDrop) is not string[] paths) return;
        await Vm.Materials.ImportPathsAsync(paths);
    }

    async void MaterialImportFiles_Click(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        var dlg = new Microsoft.Win32.OpenFileDialog { Multiselect = true, Title = "素材データベースに取り込むファイルを選択" };
        if (dlg.ShowDialog() == true) await Vm.Materials.ImportPathsAsync(dlg.FileNames);
    }

    async void MaterialImportFolder_Click(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        var dlg = new Microsoft.Win32.OpenFolderDialog { Multiselect = true, Title = "素材データベースに取り込むフォルダーを選択(中のファイルをすべて取り込みます)" };
        if (dlg.ShowDialog() == true) await Vm.Materials.ImportPathsAsync(dlg.FolderNames);
    }

    async void MaterialMoveLibrary_Click(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        var dlg = new Microsoft.Win32.OpenFolderDialog { Title = "素材の新しい保存先(空のフォルダー)を選択" };
        if (dlg.ShowDialog() == true) await Vm.Materials.MoveLibraryAsync(dlg.FolderName);
    }

    void MaterialOpenLibrary_Click(object sender, RoutedEventArgs e)
    {
        if (Vm == null) return;
        var dlg = new Microsoft.Win32.OpenFolderDialog { Title = "開くライブラリーのフォルダーを選択(materials.json があるフォルダー)" };
        if (dlg.ShowDialog() == true) Vm.Materials.OpenLibrary(dlg.FolderName);
    }

    void MaterialShowLibrary_Click(object sender, RoutedEventArgs e)
    {
        try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(MaterialStore.Instance.Root) { UseShellExecute = true }); }
        catch { }
    }

    // ---------- プレビュー ----------
    void OnVmChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExplorerViewModel.PreviewItem)) ApplyPreview();
        else if (e.PropertyName == nameof(ExplorerViewModel.PdfPageImage)) PdfScroll.ScrollToTop();
        else if (e.PropertyName == nameof(ExplorerViewModel.SelectedPreset)) WatchPreset(Vm?.SelectedPreset);
        else if (e.PropertyName == nameof(ExplorerViewModel.ShowPresetView) && Vm?.ShowPresetView != true) presetFrames.Pause();
        else if (e.PropertyName == nameof(ExplorerViewModel.SelectedTemplate))
            templateFrames.Load(Vm?.SelectedTemplate is { CanPlayFrames: true } t ? t.Path : null);
        else if (e.PropertyName == nameof(ExplorerViewModel.ShowTemplateView) && Vm?.ShowTemplateView != true) templateFrames.Pause();
        else if (e.PropertyName == nameof(ExplorerViewModel.ShowMaterialView) && Vm?.ShowMaterialView != true) { materialFrames.Pause(); viewerFrames.Pause(); }
    }

    void ApplyPreview()
    {
        StopMedia();
        var f = Vm?.PreviewItem;
        if (f == null) return;

        PreviewLayer.Focus();
        if (f.IsAudio || f.IsVideo)
        {
            SeekSlider.Maximum = 1;
            SeekSlider.Value = 0;
            TimeText.Text = "00:00 / 00:00";
            // 24bit などの WAV は、再生できる形の一時ファイルにしてから再生する
            Player.Source = new Uri(f.IsAudio ? AudioCompat.PlayablePath(f.FullPath) : f.FullPath);
            Player.Play();
            SetPlaying(true);
            timer.Start();
        }
    }

    void StopMedia()
    {
        timer.Stop();
        try
        {
            Player.Stop();
            Player.Close();   // ファイルのロックを解放
            Player.Source = null;
        }
        catch { }
        SetPlaying(false);
    }

    void SetPlaying(bool p)
    {
        playing = p;
        PlayGlyph.Text = p ? "\uE769" : "\uE768";   // 一時停止 / 再生
    }

    void TogglePlay()
    {
        if (Player.Source == null) return;
        if (playing) { Player.Pause(); SetPlaying(false); }
        else { Player.Play(); SetPlaying(true); }
    }

    void PlayBtn_Click(object sender, RoutedEventArgs e) => TogglePlay();

    void Player_MediaOpened(object sender, RoutedEventArgs e) => UpdateProgress();

    void Player_MediaEnded(object sender, RoutedEventArgs e)
    {
        Player.Pause();
        Player.Position = TimeSpan.Zero;
        SetPlaying(false);
        UpdateProgress();
    }

    void Player_MediaFailed(object? sender, ExceptionRoutedEventArgs e)
    {
        StopMedia();
        Vm?.ReportError("再生できません(この形式のコーデックが未対応の可能性があります)");
    }

    void UpdateProgress()
    {
        if (!Player.NaturalDuration.HasTimeSpan) return;
        var total = Player.NaturalDuration.TimeSpan;
        if (!seeking)
        {
            SeekSlider.Maximum = Math.Max(total.TotalSeconds, 1);
            SeekSlider.Value = Player.Position.TotalSeconds;
        }
        TimeText.Text = $"{Fmt(Player.Position)} / {Fmt(total)}";
    }

    static string Fmt(TimeSpan t) => t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"mm\:ss");

    void Seek_Down(object sender, MouseButtonEventArgs e) => seeking = true;

    void Seek_Up(object sender, MouseButtonEventArgs e)
    {
        if (Player.Source != null) Player.Position = TimeSpan.FromSeconds(SeekSlider.Value);
        seeking = false;
        UpdateProgress();
    }

    void PreviewBackdrop_MouseDown(object sender, MouseButtonEventArgs e) => Vm?.ClosePreview();

    // プレビュー表示中のキー操作: Esc=閉じる / ←→=前後のファイル / Space=再生・停止 / PageUp・PageDown=PDFのページ送り
    void Root_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.D0 && Keyboard.Modifiers == ModifierKeys.Control) { ApplyUiScale(1, true); e.Handled = true; return; }
        var vm = Vm;
        if (vm != null && vm.TextSplit.IsOpen && e.Key == Key.Escape)
        {
            vm.TextSplit.IsOpen = false;
            e.Handled = true;
            return;
        }
        if (vm != null && vm.ShowItemView && vm.ItemEditor.GraphOpen && e.Key == Key.Escape)
        {
            vm.ItemEditor.CloseGraph();
            e.Handled = true;
            return;
        }
        if (vm != null && vm.ShowMaterialView && vm.Materials.ViewerOpen)
        {
            switch (e.Key)
            {
                case Key.Escape: vm.Materials.CloseViewer(); break;
                case Key.Left: vm.Materials.StepViewer(-1); break;
                case Key.Right: vm.Materials.StepViewer(1); break;
                case Key.Space: if (vm.Materials.ViewerShowMedia) viewerFrames.TogglePlay(); else return; break;
                case Key.PageUp: if (vm.Materials.ViewerShowPdfNav) vm.Materials.StepViewerPage(-1); else return; break;
                case Key.PageDown: if (vm.Materials.ViewerShowPdfNav) vm.Materials.StepViewerPage(1); else return; break;
                default: return;
            }
            e.Handled = true;
            return;
        }
        if (vm == null || !vm.IsPreviewOpen) return;
        switch (e.Key)
        {
            case Key.Escape: vm.ClosePreview(); break;
            case Key.Left: vm.StepPreview(-1); break;
            case Key.Right: vm.StepPreview(1); break;
            case Key.PageUp: vm.StepPdfPage(-1); break;
            case Key.PageDown: vm.StepPdfPage(1); break;
            case Key.Space: TogglePlay(); break;
            default: return;
        }
        e.Handled = true;
    }
}
