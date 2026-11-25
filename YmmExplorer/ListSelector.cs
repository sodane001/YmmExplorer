using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YmmExplorer;

/// <summary>
/// ListBox にエクスプローラーと同じ一括選択の操作を足します(エクスプローラー・テンプレート・プリセットで共通)。
///  ・空きスペースをドラッグ: 範囲選択(Ctrlを押しながらで追加選択)
///  ・複数選択中の項目を押したまま動かすと、選択を保ったままドラッグ(DragPaths を指定した時)
/// Ctrl+クリック / Shift+クリック / Ctrl+A は ListBox 標準の動作です。
/// </summary>
internal sealed class ListSelector
{
    readonly ListBox list;
    readonly Rectangle rubber;
    readonly Canvas canvas;

    Point? start;
    HashSet<ListBoxItem> baseSet = new();
    ListBoxItem? pressedSelected;     // 複数選択中の項目を押した(ドラッグしなければ、離した時にその項目だけを選択)
    Point? dragStart;

    /// <summary>ドラッグで外へ渡すファイルのパス(nullならファイルは渡さない)</summary>
    public Func<string[]>? DragPaths { get; set; }

    /// <summary>並べ替えのドラッグ用データのカスタム形式名</summary>
    public const string ReorderFormat = "YmmExplorer.Reorder";

    /// <summary>
    /// 一覧の中で並べ替えるためのドラッグデータ。1行目に種類とタブ番号(例: T3)、2行目以降に項目の識別子。
    /// (nullなら、この一覧では並べ替えドラッグをしない)
    /// </summary>
    public Func<string?>? ReorderPayload { get; set; }

    readonly Rectangle? insertMark;

    public ListSelector(ListBox list, Rectangle rubber, Rectangle? insertMark = null)
    {
        this.list = list;
        this.rubber = rubber;
        this.insertMark = insertMark;
        canvas = (Canvas)rubber.Parent;
        list.PreviewMouseLeftButtonDown += OnDown;
        list.PreviewMouseMove += OnMove;
        list.PreviewMouseLeftButtonUp += OnUp;
        list.LostMouseCapture += (_, _) => EndRubberBand();
    }

    ListBoxItem? ItemAt(DependencyObject? source)
        => source == null ? null : ItemsControl.ContainerFromElement(list, source) as ListBoxItem;

    static T? FindAncestor<T>(DependencyObject? d) where T : DependencyObject
    {
        while (d != null)
        {
            if (d is T t) return t;
            d = d is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(d)
                : LogicalTreeHelper.GetParent(d);
        }
        return null;
    }

    IEnumerable<ListBoxItem> Containers()
    {
        for (int i = 0; i < list.Items.Count; i++)
            if (list.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem c) yield return c;
    }

    void OnDown(object sender, MouseButtonEventArgs e)
    {
        var src = e.OriginalSource as DependencyObject;
        var item = ItemAt(src);
        dragStart = null;
        pressedSelected = null;

        if (item == null)
        {
            // 空きスペース: 範囲選択を開始(スクロールバー上は除く)
            if (FindAncestor<ScrollBar>(src) != null) return;
            list.Focus();
            var ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            baseSet = ctrl ? Containers().Where(c => c.IsSelected).ToHashSet() : new HashSet<ListBoxItem>();
            if (!ctrl) list.UnselectAll();
            start = e.GetPosition(list);
            list.CaptureMouse();
            e.Handled = true;
            return;
        }

        if (DragPaths == null && ReorderPayload == null) return;
        dragStart = e.GetPosition(null);
        // 複数選択中の項目を押した時は、選択を崩さない(そのままドラッグできるように)
        if (e.ClickCount == 1 && item.IsSelected && list.SelectedItems.Count > 1
            && Keyboard.Modifiers == ModifierKeys.None)
        {
            pressedSelected = item;
            list.Focus();
            e.Handled = true;
        }
    }

    void OnUp(object sender, MouseButtonEventArgs e)
    {
        EndRubberBand();
        if (pressedSelected != null)
        {
            // ドラッグせずに離した = ふつうのクリック: その項目だけを選択
            var it = pressedSelected;
            pressedSelected = null;
            list.UnselectAll();
            it.IsSelected = true;
        }
        dragStart = null;
    }

    void OnMove(object sender, MouseEventArgs e)
    {
        if (start != null)
        {
            if (e.LeftButton == MouseButtonState.Pressed) UpdateRubberBand(e.GetPosition(list));
            else EndRubberBand();
            return;
        }

        if ((DragPaths == null && ReorderPayload == null)
            || e.LeftButton != MouseButtonState.Pressed || dragStart is not { } s) return;
        var p = e.GetPosition(null);
        if (Math.Abs(p.X - s.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(p.Y - s.Y) < SystemParameters.MinimumVerticalDragDistance) return;

        dragStart = null;
        pressedSelected = null;     // ドラッグが始まったので、離した時の単独選択はしない
        var paths = DragPaths?.Invoke() ?? Array.Empty<string>();
        var payload = ReorderPayload?.Invoke();
        if (paths.Length == 0 && payload == null) return;

        var data = new DataObject();
        if (paths.Length > 0) data.SetData(DataFormats.FileDrop, paths);
        if (payload != null) data.SetData(ReorderFormat, payload);
        // 他のアプリへ渡した時にファイルを「移動」されないよう、Move は許可しない
        DragDrop.DoDragDrop(list, data, DragDropEffects.Copy | DragDropEffects.Link);
    }

    // ---- 並べ替えドラッグ: 挿入位置の判定と目印 ----
    /// <summary>ドロップ位置(一覧の座標)から、挿入する番号と目印の位置を求める</summary>
    (int Index, Rect Mark) Locate(Point p)
    {
        ListBoxItem? best = null;
        Rect bestRect = default;
        double bestDist = double.MaxValue;
        foreach (var c in Containers())
        {
            var origin = c.TranslatePoint(new Point(0, 0), list);
            var r = new Rect(origin, new Size(c.ActualWidth, c.ActualHeight));
            var dx = Math.Max(Math.Max(r.Left - p.X, 0), p.X - r.Right);
            var dy = Math.Max(Math.Max(r.Top - p.Y, 0), p.Y - r.Bottom);
            var d = dx * dx + dy * dy;
            if (d < bestDist) { bestDist = d; best = c; bestRect = r; }
        }
        if (best == null) return (0, new Rect(0, 0, 3, 40));

        var index = list.ItemContainerGenerator.IndexFromContainer(best);
        // タイルの左半分なら手前に、右半分なら後ろに入れる
        return p.X < bestRect.Left + bestRect.Width / 2
            ? (index, new Rect(bestRect.Left - 2, bestRect.Top, 3, bestRect.Height))
            : (index + 1, new Rect(bestRect.Right - 1, bestRect.Top, 3, bestRect.Height));
    }

    public int InsertIndexAt(Point p) => Locate(p).Index;

    public void ShowInsertMark(Point p)
    {
        if (insertMark == null) return;
        var mark = Locate(p).Mark;
        var tl = list.TranslatePoint(mark.TopLeft, canvas);
        Canvas.SetLeft(insertMark, tl.X);
        Canvas.SetTop(insertMark, tl.Y);
        insertMark.Width = mark.Width;
        insertMark.Height = mark.Height;
        insertMark.Visibility = Visibility.Visible;
    }

    public void HideInsertMark()
    {
        if (insertMark != null) insertMark.Visibility = Visibility.Collapsed;
    }

    void UpdateRubberBand(Point now)
    {
        if (start is not { } st) return;
        var rect = new Rect(st, now);
        var tl = list.TranslatePoint(rect.TopLeft, canvas);
        Canvas.SetLeft(rubber, tl.X);
        Canvas.SetTop(rubber, tl.Y);
        rubber.Width = rect.Width;
        rubber.Height = rect.Height;
        rubber.Visibility = Visibility.Visible;

        foreach (var c in Containers())
        {
            var origin = c.TranslatePoint(new Point(0, 0), list);
            var bounds = new Rect(origin, new Size(c.ActualWidth, c.ActualHeight));
            c.IsSelected = baseSet.Contains(c) || rect.IntersectsWith(bounds);
        }
    }

    void EndRubberBand()
    {
        if (start == null) return;
        start = null;
        rubber.Visibility = Visibility.Collapsed;
        if (list.IsMouseCaptured) list.ReleaseMouseCapture();
    }
}
