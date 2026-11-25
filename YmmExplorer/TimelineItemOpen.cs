using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace YmmExplorer;

/// <summary>
/// YMM4のタイムラインでアイテムをダブルクリックした時、YMM4標準のアイテム編集の代わりに、
/// YmmExplorer のアイテムタブを開く。
/// 2回目のクリック(ダブルクリック)の入力そのものを、YMM4に届く前に取り消すので、標準の編集画面は開かない。
/// 1回目のクリックはそのまま届くので、アイテムの選択はいつもどおりに行われる。
/// アイテムタブがどのウィンドウにも出ていない時や、設定でオフの時は、何もしない(標準どおりに開く)
/// </summary>
internal static class TimelineItemOpen
{
    static bool attached;
    static long lastDownMs;
    static Point lastDownPos;
    static object? lastDownTarget;

    [DllImport("user32.dll")] static extern uint GetDoubleClickTime();

    public static void Ensure()
    {
        if (attached) return;
        try
        {
            InputManager.Current.PreProcessInput += OnPreProcess;
            attached = true;
        }
        catch { }
    }

    static void OnPreProcess(object sender, PreProcessInputEventArgs e)
    {
        try
        {
            if (e.StagingItem.Input is not MouseButtonEventArgs me) return;
            if (me.RoutedEvent != Mouse.PreviewMouseDownEvent || me.ChangedButton != MouseButton.Left) return;
            if (!ExplorerSettings.Instance.OpenItemTabOnDoubleClick) return;

            // ダブルクリックかどうかを、自分で数える
            // (この時点では、WPF がまだ ClickCount を数えていないことがあるため。2回目のクリックそのものを止めたいので、ここで判断する)
            var src = (me.OriginalSource ?? me.Source ?? Mouse.DirectlyOver) as DependencyObject;
            if (src == null) return;
            var win = Window.GetWindow(src);
            if (win == null) return;
            long now = Environment.TickCount64;
            var pos = me.GetPosition(win);
            bool isDouble = me.ClickCount >= 2
                || (now - lastDownMs <= Math.Max(200, GetDoubleClickTime())
                    && ReferenceEquals(lastDownTarget, win)
                    && Math.Abs(pos.X - lastDownPos.X) <= SystemParameters.MinimumHorizontalDragDistance * 2
                    && Math.Abs(pos.Y - lastDownPos.Y) <= SystemParameters.MinimumVerticalDragDistance * 2);
            lastDownMs = isDouble ? 0 : now;       // 3回目のクリックを、またダブルクリックと数えないように
            lastDownPos = pos;
            lastDownTarget = win;
            if (!isDouble) return;
            if (Keyboard.Modifiers != ModifierKeys.None) return;

            if (IsInsideExplorer(src) || !IsTimelineItem(src)) return;

            var windows = ShortcutHost.OpenWindows();
            var target = windows.FirstOrDefault(w => w.HasItemTab);
            if (target == null) return;        // アイテムタブが無い時は、標準のまま

            e.Cancel();
            var dispatcher = Application.Current!.Dispatcher;
            void Open()
            {
                try { if (target.OpenItemTab()) BringToFront(target); }
                catch { }
            }
            dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(Open));
            // 念のため: YMM4側がそれでも標準のアイテム編集を前に出した時は、少し後でもう一度こちらを前に出す
            dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(Open));
            var again = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            again.Tick += (_, _) => { again.Stop(); BringToFront(target); };
            again.Start();
        }
        catch { }
    }

    static DependencyObject? Parent(DependencyObject d)
        => d is Visual or System.Windows.Media.Media3D.Visual3D
            ? VisualTreeHelper.GetParent(d) ?? LogicalTreeHelper.GetParent(d)
            : LogicalTreeHelper.GetParent(d);

    static bool IsInsideExplorer(DependencyObject d)
    {
        for (var c = d; c != null; c = Parent(c))
            if (c is ExplorerView) return true;
        return false;
    }

    /// <summary>
    /// クリックした場所が、タイムライン上のアイテムか。
    /// 親をたどって、表示元(DataContext)が「いま選ばれているアイテム」かそれを包むものになっている部品があり、
    /// さらにその上にタイムラインらしい部品があれば、アイテムとみなす
    /// </summary>
    static bool IsTimelineItem(DependencyObject src)
    {
        List<object> selected;
        try { selected = Ymm.SelectedItems(); } catch { selected = new(); }
        bool foundItem = false, itemLike = false, timeline = false;
        int steps = 0;
        for (var c = src; c != null && steps < 120; c = Parent(c), steps++)
        {
            var dc = (c as FrameworkElement)?.DataContext ?? (c as FrameworkContentElement)?.DataContext;
            if (!foundItem && dc != null)
            {
                if (selected.Any(s => ReferenceEquals(s, dc))) foundItem = true;
                else
                {
                    foreach (var name in new[] { "Item", "Model", "Source", "TimelineItem", "Value" })
                    {
                        object? inner = null;
                        try { inner = Ymm.Get(dc, name); } catch { }
                        if (inner != null && selected.Any(s => ReferenceEquals(s, inner))) { foundItem = true; break; }
                    }
                }
                // 選択がまだ反映されていない時の予備: 表示元の型の名前が「〜Item〜」(アイテムの表示用)
                var tn = dc.GetType().Name;
                if (tn.Contains("Item", StringComparison.Ordinal) && !tn.Contains("Items", StringComparison.Ordinal)
                    && !tn.Contains("Menu", StringComparison.Ordinal) && !tn.Contains("Layer", StringComparison.Ordinal))
                    itemLike = true;
            }
            var n = c.GetType().Name + "|" + (dc?.GetType().Name ?? "");
            if (n.Contains("Timeline", StringComparison.OrdinalIgnoreCase)) timeline = true;
            if (timeline && (foundItem || itemLike)) return true;
        }
        return false;
    }

    /// <summary>YmmExplorer の画面を前に出す(別ウィンドウなら手前に、タブにまとめてあればそのタブを選ぶ)</summary>
    static void BringToFront(ExplorerViewModel vm)
    {
        ExplorerView? view = null;
        lock (ExplorerView.Instances)
            foreach (var w in ExplorerView.Instances)
                if (w.TryGetTarget(out var v) && ReferenceEquals(v.DataContext, vm)) { view = v; break; }
        if (view == null)
            foreach (Window w in Application.Current.Windows)
            {
                view = Find(w, vm);
                if (view != null) break;
            }
        if (view == null) return;

        // 1) ドッキング(YMM4のツールをタブにまとめている時): レイアウトの中から、この画面を入れている物を探して選ぶ。
        //    裏に隠れているタブの中身は、画面の部品の木につながっていないので、レイアウトの側から探す
        bool selected = false;
        try { selected = SelectInDockLayout(view); } catch { }

        // 2) 画面の部品の木をたどって、タブ・ウィンドウを前に出す(見えている時)
        for (var c = (DependencyObject)view; c != null; c = Parent(c))
        {
            if (c is TabItem ti) ti.IsSelected = true;
            if (c is Window win)
            {
                if (win.WindowState == WindowState.Minimized) win.WindowState = WindowState.Normal;
                if (win != Application.Current.MainWindow) win.Activate();
                break;
            }
        }
        _ = selected;
        try { view.BringIntoView(); } catch { }
    }

    /// <summary>この画面(またはそれを包む物)が、obj かどうか</summary>
    static bool Holds(object? content, ExplorerView view)
    {
        if (content == null) return false;
        if (ReferenceEquals(content, view) || ReferenceEquals(content, view.DataContext)) return true;
        if (content is DependencyObject d)
            for (var c = (DependencyObject?)view; c != null; c = LogicalTreeHelper.GetParent(c) ?? (c is Visual ? VisualTreeHelper.GetParent(c) : null))
                if (ReferenceEquals(c, d)) return true;
        return false;
    }

    /// <summary>
    /// ドッキングの仕組み(DockingManager)のレイアウトをたどり、中身がこの画面の物を「選択・アクティブ」にする
    /// </summary>
    static bool SelectInDockLayout(ExplorerView view)
    {
        bool done = false;
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

        void Walk(object? node, int depth)
        {
            if (node == null || done || depth > 30 || !visited.Add(node)) return;
            var t = node.GetType();
            var contentProp = t.GetProperty("Content");
            if (contentProp != null && t.Name.Contains("Layout", StringComparison.Ordinal))
            {
                object? content = null;
                try { content = contentProp.GetValue(node); } catch { }
                if (Holds(content, view))
                {
                    foreach (var name in new[] { "IsSelected", "IsActive" })
                    {
                        try
                        {
                            var p = t.GetProperty(name);
                            if (p != null && p.PropertyType == typeof(bool) && p.CanWrite) p.SetValue(node, true);
                        }
                        catch { }
                    }
                    done = true;
                    return;
                }
            }
            foreach (var name in new[] { "Children", "RootPanel", "FloatingWindows", "Hidden", "LeftSide", "RightSide", "TopSide", "BottomSide", "RootDocument" })
            {
                object? v = null;
                try { v = t.GetProperty(name)?.GetValue(node); } catch { }
                if (v is System.Collections.IEnumerable list and not string) foreach (var x in list) Walk(x, depth + 1);
                else if (v != null && !v.GetType().IsValueType && v is not string) Walk(v, depth + 1);
            }
        }

        foreach (Window w in Application.Current.Windows)
        {
            foreach (var dm in FindAll(w, d => d.GetType().Name.Contains("DockingManager", StringComparison.Ordinal)))
            {
                object? layout = null;
                try { layout = dm.GetType().GetProperty("Layout")?.GetValue(dm); } catch { }
                Walk(layout, 0);
                if (done) return true;
            }
        }
        return done;
    }

    static IEnumerable<DependencyObject> FindAll(DependencyObject root, Func<DependencyObject, bool> match)
    {
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);
        int visited = 0;
        while (queue.Count > 0 && visited++ < 40000)
        {
            var d = queue.Dequeue();
            if (match(d)) { yield return d; continue; }
            if (d is not Visual and not System.Windows.Media.Media3D.Visual3D) continue;
            int n = VisualTreeHelper.GetChildrenCount(d);
            for (int i = 0; i < n; i++) queue.Enqueue(VisualTreeHelper.GetChild(d, i));
        }
    }

    static ExplorerView? Find(DependencyObject root, ExplorerViewModel vm)
        => FindAll(root, d => d is ExplorerView ev && ReferenceEquals(ev.DataContext, vm)).FirstOrDefault() as ExplorerView;
}
