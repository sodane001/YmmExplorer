using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace YmmExplorer;

/// <summary>緩急(イージング)の計算。グラフに、実際の動きに近い曲線を描くために使う</summary>
public static class Easing
{
    static double In(string family, double t)
    {
        switch (family)
        {
            case "Quad": return t * t;
            case "Cubic": return t * t * t;
            case "Quart": return t * t * t * t;
            case "Quint": return t * t * t * t * t;
            case "Sine": return 1 - Math.Cos(t * Math.PI / 2);
            case "Expo": return t <= 0 ? 0 : Math.Pow(2, 10 * (t - 1));
            case "Circ": return 1 - Math.Sqrt(Math.Max(0, 1 - t * t));
            case "Back": return t * t * (2.70158 * t - 1.70158);
            case "Elastic": return t <= 0 ? 0 : t >= 1 ? 1 : -Math.Pow(2, 10 * (t - 1)) * Math.Sin((t - 1 - 0.075) * 2 * Math.PI / 0.3);
            case "Bounce": return 1 - BounceOut(1 - t);
            default: return t;
        }
    }

    static double BounceOut(double t)
    {
        const double n = 7.5625, d = 2.75;
        if (t < 1 / d) return n * t * t;
        if (t < 2 / d) { t -= 1.5 / d; return n * t * t + 0.75; }
        if (t < 2.5 / d) { t -= 2.25 / d; return n * t * t + 0.9375; }
        t -= 2.625 / d;
        return n * t * t + 0.984375;
    }

    /// <summary>「Quad_InOut」のような名前を、種類・向き・整数かどうかに分ける</summary>
    public static bool Parse(string raw, out string family, out string direction, out bool integer)
    {
        family = direction = "";
        integer = false;
        var parts = (raw ?? "").Split('_');
        if (parts.Length < 2) return false;
        if (parts[1] != "In" && parts[1] != "Out" && parts[1] != "InOut" && parts[1] != "OutIn") return false;
        family = parts[0];
        direction = parts[1];
        integer = parts.Length > 2 && parts[2] == "Int";
        return true;
    }

    public static double Apply(string family, string direction, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return direction switch
        {
            "In" => In(family, t),
            "Out" => 1 - In(family, 1 - t),
            "InOut" => t < 0.5 ? In(family, 2 * t) / 2 : 1 - In(family, 2 - 2 * t) / 2,
            "OutIn" => t < 0.5 ? (1 - In(family, 1 - 2 * t)) / 2 : 0.5 + In(family, 2 * t - 1) / 2,
            _ => t,
        };
    }

    /// <summary>動かし方の名前から、0〜1 の進み具合を返す関数を作る(分からない名前は、まっすぐ)</summary>
    public static Func<double, double> For(string raw)
    {
        if (Parse(raw, out var family, out var direction, out _)) return t => Apply(family, direction, t);
        if (raw.Contains("加減速", StringComparison.Ordinal)) return t => Apply("Quad", "InOut", t);
        if (raw.Contains("瞬間", StringComparison.Ordinal)) return t => t >= 1 ? 1 : 0;
        return t => Math.Clamp(t, 0, 1);
    }
}

/// <summary>
/// 移動(キーフレーム)のグラフ。横が時間(左がアイテムの始まり、右が終わり)、縦が値。
/// 点を上下にドラッグすると、その点の値が変わる。
/// </summary>
public sealed class AnimGraph : FrameworkElement
{
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
        nameof(Row), typeof(ItemPropRow), typeof(AnimGraph), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public ItemPropRow? Row { get => (ItemPropRow?)GetValue(RowProperty); set => SetValue(RowProperty, value); }

    /// <summary>値が変わるたびに増える番号(描き直しの合図)</summary>
    public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
        nameof(Version), typeof(int), typeof(AnimGraph), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public int Version { get => (int)GetValue(VersionProperty); set => SetValue(VersionProperty, value); }

    /// <summary>操作モード(0=ふつう 1=手書き 2=中間点を横にも動かす)。変わった時の描き直しの合図</summary>
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
        nameof(Mode), typeof(int), typeof(AnimGraph), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public int Mode { get => (int)GetValue(ModeProperty); set => SetValue(ModeProperty, value); }

    static int CurrentMode => ItemPropRow.CurrentGraphMode;

    const double PadLeft = 52, PadRight = 18, PadTop = 22, PadBottom = 22;
    static readonly Typeface Face = new("Yu Gothic UI");

    int dragIndex = -1, hoverIndex = -1;
    double dragValue;
    long lastApply;
    /// <summary>中間点を足した直後(YMM4側に点が増えるのを待っている間)。増える前の点の数を覚えておく</summary>
    int pendingCount = -1;
    bool hoverCurve;
    Point lastMouse;

    // 手書き: なぞった線(この部品の上の位置。左から順)
    List<Point>? drawn;
    // 中間点を横に動かしている時の、横の位置(0〜1)。動かしていない時は -1
    double dragFrac = -1;
    bool dragHorizontalOnly;

    Brush Res(string key, Color fallback)
    {
        if (TryFindResource(key) is Brush b) return b;
        var s = new SolidColorBrush(fallback);
        s.Freeze();
        return s;
    }

    FormattedText Text(string s, Brush brush, double size = 10) =>
        new(s, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Face, size, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);

    bool Layout(out List<double> values, out List<double> xs, out double min, out double max, out Rect plot)
    {
        values = new(); xs = new(); min = 0; max = 1;
        plot = new Rect(PadLeft, PadTop, Math.Max(10, ActualWidth - PadLeft - PadRight), Math.Max(10, ActualHeight - PadTop - PadBottom));
        var row = Row;
        if (row == null) return false;
        values = row.GraphValues();
        if (values.Count == 0) return false;
        if (pendingCount >= 0 && values.Count > pendingCount) pendingCount = -1;      // 点が増えた
        if (pendingCount < 0 && dragIndex >= 0 && dragIndex < values.Count && !dragHorizontalOnly) values[dragIndex] = dragValue;
        var pos = row.GraphPositions(values.Count);
        if (pendingCount < 0 && dragFrac >= 0 && dragIndex > 0 && dragIndex < pos.Count - 1) pos[dragIndex] = dragFrac;
        row.GraphRange(values, out min, out max);
        foreach (var p in pos) xs.Add(plot.Left + p * plot.Width);
        return true;
    }

    double Y(double v, double min, double max, Rect plot) => plot.Bottom - (v - min) / (max - min) * plot.Height;

    protected override void OnRender(DrawingContext dc)
    {
        var panel = Res("Panel", Color.FromRgb(0x25, 0x27, 0x2B));
        var strokeBrush = Res("Stroke", Color.FromRgb(0x3A, 0x3D, 0x44));
        var sub = Res("Sub", Color.FromRgb(0x9A, 0xA0, 0xA8));
        var text = Res("Text", Colors.White);
        var accent = Res("Accent", Color.FromRgb(0x60, 0xCD, 0xFF));
        dc.DrawRoundedRectangle(panel, null, new Rect(0, 0, ActualWidth, ActualHeight), 6, 6);
        if (!Layout(out var values, out var xs, out var min, out var max, out var plot)) return;
        var row = Row!;

        // 目盛り(上・まん中・下)と、その値
        var gridPen = new Pen(strokeBrush, 1) { DashStyle = new DashStyle(new double[] { 3, 3 }, 0) };
        gridPen.Freeze();
        foreach (var frac in new[] { 0.0, 0.5, 1.0 })
        {
            double v = min + (max - min) * frac, y = Y(v, min, max, plot);
            dc.DrawLine(gridPen, new Point(plot.Left, y), new Point(plot.Right, y));
            var ft = Text(row.FormatValue(v), sub);
            dc.DrawText(ft, new Point(plot.Left - ft.Width - 6, y - ft.Height / 2));
        }

        // 値の動き(曲線)と、その下の塗り
        var ease = row.GraphEase();
        var curve = new List<Point>();
        if (values.Count == 1) { curve.Add(new Point(plot.Left, Y(values[0], min, max, plot))); curve.Add(new Point(plot.Right, Y(values[0], min, max, plot))); }
        else
            for (int i = 0; i < values.Count - 1; i++)
            {
                const int steps = 28;
                for (int k = 0; k <= steps; k++)
                {
                    if (i > 0 && k == 0) continue;
                    double t = k / (double)steps;
                    curve.Add(new Point(xs[i] + (xs[i + 1] - xs[i]) * t, Y(values[i] + (values[i + 1] - values[i]) * ease(t), min, max, plot)));
                }
            }
        dc.PushClip(new RectangleGeometry(new Rect(plot.Left - 1, 1, plot.Width + 2, ActualHeight - 2)));
        var fill = new StreamGeometry();
        using (var g = fill.Open())
        {
            g.BeginFigure(new Point(curve[0].X, plot.Bottom), true, true);
            g.PolyLineTo(curve, true, false);
            g.LineTo(new Point(curve[^1].X, plot.Bottom), true, false);
        }
        fill.Freeze();
        var fillBrush = accent.Clone();
        fillBrush.Opacity = 0.16;
        fillBrush.Freeze();
        dc.DrawGeometry(fillBrush, null, fill);
        var line = new StreamGeometry();
        using (var g = line.Open())
        {
            g.BeginFigure(curve[0], false, false);
            g.PolyLineTo(curve.Skip(1).ToList(), true, true);
        }
        line.Freeze();
        var curvePen = new Pen(accent, 2.2) { LineJoin = PenLineJoin.Round };
        curvePen.Freeze();
        dc.DrawGeometry(null, curvePen, line);
        dc.Pop();

        // 手書き中の線
        if (drawn != null && drawn.Count > 1)
        {
            var orange = new SolidColorBrush(Color.FromRgb(0xFF, 0x9F, 0x43));
            orange.Freeze();
            var sp = new Pen(orange, 2.6) { LineJoin = PenLineJoin.Round, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            sp.Freeze();
            var sg = new StreamGeometry();
            using (var g = sg.Open())
            {
                g.BeginFigure(drawn[0], false, false);
                g.PolyLineTo(drawn.Skip(1).ToList(), true, true);
            }
            sg.Freeze();
            dc.DrawGeometry(null, sp, sg);
        }

        // いまのモード(ふつう以外の時だけ、右上に出す)
        if (CurrentMode != 0)
        {
            var badge = Text(CurrentMode == 1 ? "✎ 手書きモード" : "⇔ 中間点横移動モード", text, 11);
            var br = new Rect(plot.Right - badge.Width - 12, 2, badge.Width + 12, badge.Height + 4);
            var bb = accent.Clone();
            bb.Opacity = 0.35;
            bb.Freeze();
            dc.DrawRoundedRectangle(bb, null, br, 5, 5);
            dc.DrawText(badge, new Point(br.Left + 6, br.Top + 2));
        }

        // 中間点を横に動かしている時: いまの位置(フレーム)
        if (dragFrac >= 0 && dragIndex > 0 && dragIndex < values.Count - 1 && row.ItemLength > 0)
        {
            var ft = Text($"{Math.Round(dragFrac * row.ItemLength)} フレーム目", text, 11);
            double fx = Math.Clamp(xs[dragIndex] - ft.Width / 2, 2, ActualWidth - ft.Width - 2);
            dc.DrawRoundedRectangle(panel, null, new Rect(fx - 4, plot.Bottom - ft.Height - 6, ft.Width + 8, ft.Height + 4), 4, 4);
            dc.DrawText(ft, new Point(fx, plot.Bottom - ft.Height - 4));
        }

        // 点(ドラッグできる)と、名前・値
        var pointPen = new Pen(text, 1.5);
        pointPen.Freeze();
        if (pendingCount >= 0 || (hoverCurve && dragIndex < 0 && CurrentMode == 0))
        {
            // これから入る中間点(線の上にマウスを置いた時・足した直後)
            var ghost = accent.Clone();
            ghost.Opacity = 0.55;
            ghost.Freeze();
            var gp = pendingCount >= 0 ? new Point(lastMouse.X, Math.Clamp(lastMouse.Y, plot.Top, plot.Bottom)) : new Point(lastMouse.X, CurveY(lastMouse.X, values, xs, min, max, plot, ease));
            dc.DrawEllipse(ghost, pointPen, gp, 6.5, 6.5);
            if (pendingCount < 0)
            {
                var hint = Text("ここをドラッグで、中間点を追加", text);
                dc.DrawText(hint, new Point(Math.Clamp(gp.X - hint.Width / 2, 2, ActualWidth - hint.Width - 2), Math.Max(1, gp.Y - hint.Height - 10)));
            }
        }
        for (int i = 0; i < values.Count; i++)
        {
            double x = values.Count == 1 ? plot.Left : xs[i], y = Math.Clamp(Y(values[i], min, max, plot), 2, ActualHeight - 2);
            if (i > 0 && i < values.Count - 1) dc.DrawLine(gridPen, new Point(x, plot.Top), new Point(x, plot.Bottom));
            bool hot = i == dragIndex || i == hoverIndex;
            dc.DrawEllipse(accent, pointPen, new Point(x, y), hot ? 7.5 : 5.5, hot ? 7.5 : 5.5);

            var value = Text(row.FormatValue(values[i]), text, 11);
            double tx = Math.Clamp(x - value.Width / 2, 2, ActualWidth - value.Width - 2);
            double ty = y - value.Height - 9;
            if (ty < 1) ty = y + 10;
            dc.DrawText(value, new Point(tx, ty));

            var name = Text(i == 0 ? "始点" : i == values.Count - 1 ? "終点" : $"中間点{i}", sub);
            dc.DrawText(name, new Point(Math.Clamp(x - name.Width / 2, 2, ActualWidth - name.Width - 2), plot.Bottom + 5));
        }
    }

    /// <summary>横の位置 x での、線の高さ</summary>
    double CurveY(double x, List<double> values, List<double> xs, double min, double max, Rect plot, Func<double, double> ease)
    {
        if (values.Count == 1) return Y(values[0], min, max, plot);
        for (int i = 0; i < values.Count - 1; i++)
        {
            if (x > xs[i + 1] && i < values.Count - 2) continue;
            double w = xs[i + 1] - xs[i];
            double t = w <= 0 ? 0 : Math.Clamp((x - xs[i]) / w, 0, 1);
            return Y(values[i] + (values[i + 1] - values[i]) * ease(t), min, max, plot);
        }
        return Y(values[^1], min, max, plot);
    }

    int HitPoint(Point p)
    {
        if (!Layout(out var values, out var xs, out var min, out var max, out var plot)) return -1;
        int best = -1;
        double bestDistance = 14;
        for (int i = 0; i < values.Count; i++)
        {
            double x = values.Count == 1 ? plot.Left : xs[i], y = Y(values[i], min, max, plot);
            double d = Math.Sqrt((p.X - x) * (p.X - x) + (p.Y - y) * (p.Y - y));
            if (d < bestDistance) { bestDistance = d; best = i; }
        }
        return best;
    }

    /// <summary>線の上(点ではない所)にマウスがあるか。中間点を足せる場所</summary>
    bool OnCurve(Point p)
    {
        var row = Row;
        if (row == null || !row.IsAnimated || CurrentMode != 0) return false;
        if (!Layout(out var values, out var xs, out var min, out var max, out var plot)) return false;
        if (values.Count < 2 || p.X < plot.Left + 10 || p.X > plot.Right - 10) return false;
        return Math.Abs(p.Y - CurveY(p.X, values, xs, min, max, plot, row.GraphEase())) < 12;
    }

    double ValueAt(Point p)
    {
        Layout(out _, out _, out var min, out var max, out var plot);
        double v = max - (p.Y - plot.Top) / plot.Height * (max - min);
        return Math.Clamp(v, min, max);
    }

    double FractionAt(Point p)
    {
        Layout(out _, out _, out _, out _, out var plot);
        return Math.Clamp((p.X - plot.Left) / plot.Width, 0, 1);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        var row = Row;
        if (row == null) return;
        var p = e.GetPosition(this);
        lastMouse = p;

        if (CurrentMode == 1)
        {
            // 手書き: なぞり始め
            if (!Layout(out var v0, out _, out var mn, out var mx, out var pl)) return;
            row.GraphRange(v0, out mn, out mx);
            row.LockGraphRange(mn, mx);
            drawn = new List<Point> { ClampToPlot(p, pl) };
            CaptureMouse();
            InvalidateVisual();
            e.Handled = true;
            return;
        }

        var i = HitPoint(p);
        bool insert = i < 0 && OnCurve(p);
        if (i < 0 && !insert) return;

        // ドラッグの間は、縦の範囲を固定する(動かしている途中で目盛りが変わらないように)
        var before = row.GraphValues();
        row.GraphRange(before, out var min, out var max);
        row.LockGraphRange(min, max);
        if (insert)
        {
            // 線の上をつかんだ: その場所に中間点を足して、そのまま上下に動かせるようにする
            Layout(out _, out var xs, out _, out _, out _);
            dragValue = ValueAt(p);
            dragIndex = xs.Count(x => x < p.X);          // 足した点が入る番号
            pendingCount = before.Count;
            row.InsertPointAt(FractionAt(p), dragValue);
        }
        else
        {
            dragIndex = i;
            dragValue = ValueAt(p);
            dragHorizontalOnly = false;
            // 中間点横移動モード: 中間点は左右にも動かせる
            if (CurrentMode == 2 && i > 0 && i < before.Count - 1 && row.CanMovePoints)
            {
                dragFrac = row.GraphPositions(before.Count)[i];
                dragValue = before[i];
            }
            else dragFrac = -1;
        }
        hoverCurve = false;
        CaptureMouse();
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var p = e.GetPosition(this);
        lastMouse = p;
        if (drawn != null)
        {
            // 手書き: 左へ戻ったら、戻った所から先を描き直す(線はいつも左から右へ1本)
            if (!Layout(out _, out _, out _, out _, out var pl)) return;
            var q = ClampToPlot(p, pl);
            while (drawn.Count > 0 && drawn[^1].X >= q.X) drawn.RemoveAt(drawn.Count - 1);
            drawn.Add(q);
            InvalidateVisual();
            return;
        }
        if (dragIndex < 0)
        {
            int h = HitPoint(p);
            bool curve = h < 0 && OnCurve(p);
            if (h != hoverIndex || curve != hoverCurve || curve)
            {
                hoverIndex = h;
                hoverCurve = curve;
                int n = Row?.GraphValues().Count ?? 0;
                Cursor = CurrentMode == 1 ? Cursors.Pen
                       : h >= 0 ? (CurrentMode == 2 && h > 0 && h < n - 1 ? Cursors.SizeAll : Cursors.SizeNS)
                       : curve ? Cursors.Cross : null;
                InvalidateVisual();
            }
            else if (CurrentMode == 1 && Cursor != Cursors.Pen) Cursor = Cursors.Pen;
            return;
        }
        dragHorizontalOnly = dragFrac >= 0 && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        dragValue = ValueAt(p);
        if (dragFrac >= 0) dragFrac = SlideFraction(p);
        InvalidateVisual();
        if (pendingCount >= 0) return;                   // 点が増えるまでは、書き込まない
        // 書き込みは、少し間隔を空ける(「元に戻す」の履歴が増えすぎないように)
        if (Environment.TickCount64 - lastApply < 70) return;
        lastApply = Environment.TickCount64;
        if (!dragHorizontalOnly) Row?.SetPointFromGraph(dragIndex, dragValue);
        if (dragFrac >= 0) Row?.MovePointTo(dragIndex, dragFrac);
    }

    Point ClampToPlot(Point p, Rect plot) => new(Math.Clamp(p.X, plot.Left, plot.Right), Math.Clamp(p.Y, plot.Top, plot.Bottom));

    /// <summary>中間点を横に動かす時の位置(前後の点は追い越さない)</summary>
    double SlideFraction(Point p)
    {
        var row = Row;
        if (row == null) return -1;
        int n = row.GraphValues().Count;
        var pos = row.GraphPositions(n);
        int len = Math.Max(2, row.ItemLength);
        double gap = 1.0 / len;
        double lo = dragIndex - 1 >= 0 ? pos[dragIndex - 1] + gap : gap;
        double hi = dragIndex + 1 < n ? pos[dragIndex + 1] - gap : 1 - gap;
        if (dragIndex - 1 == 0) lo = gap;
        if (dragIndex + 1 == n - 1) hi = 1 - gap;
        double f = FractionAt(p);
        if (hi < lo) return pos[dragIndex];
        return Math.Round(Math.Clamp(f, lo, hi) * len) / len;   // フレームの区切りに合わせる
    }

    /// <summary>なぞった線を、点の少ない折れ線にする(形が大きく変わらない範囲で点を減らす)</summary>
    static List<Point> Simplify(List<Point> pts, double tolerance)
    {
        if (pts.Count < 3) return new List<Point>(pts);
        var keep = new bool[pts.Count];
        keep[0] = keep[^1] = true;
        var stack = new Stack<(int a, int b)>();
        stack.Push((0, pts.Count - 1));
        while (stack.Count > 0)
        {
            var (a, b) = stack.Pop();
            double best = 0;
            int at = -1;
            var pa = pts[a]; var pb = pts[b];
            double dx = pb.X - pa.X, dy = pb.Y - pa.Y, len = Math.Sqrt(dx * dx + dy * dy);
            for (int i = a + 1; i < b; i++)
            {
                double d = len < 1e-9 ? (pts[i] - pa).Length : Math.Abs(dy * pts[i].X - dx * pts[i].Y + pb.X * pa.Y - pb.Y * pa.X) / len;
                if (d > best) { best = d; at = i; }
            }
            if (at >= 0 && best > tolerance) { keep[at] = true; stack.Push((a, at)); stack.Push((at, b)); }
        }
        var result = new List<Point>();
        for (int i = 0; i < pts.Count; i++) if (keep[i]) result.Add(pts[i]);
        return result;
    }

    void FinishStroke()
    {
        var row = Row;
        var line = drawn;
        drawn = null;
        if (row == null || line == null || line.Count < 2) { row?.LockGraphRange(null, null); return; }
        if (!Layout(out _, out _, out var min, out var max, out var plot)) { row.LockGraphRange(null, null); return; }
        // 点が多すぎると重くなるので、40点以下になるまで、まとめ方を粗くする
        double tol = 2.5;
        var simple = Simplify(line, tol);
        while (simple.Count > 40 && tol < 60) { tol *= 1.5; simple = Simplify(line, tol); }
        var pts = simple.Select(q => ((q.X - plot.Left) / plot.Width, max - (q.Y - plot.Top) / plot.Height * (max - min))).ToList();
        row.ApplyStroke(pts);
        row.LockGraphRange(null, null);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (drawn != null)
        {
            FinishStroke();
            if (IsMouseCaptured) ReleaseMouseCapture();
            InvalidateVisual();
            e.Handled = true;
            return;
        }
        if (dragIndex < 0) return;
        int i = dragIndex;
        double v = dragValue;
        var row = Row;
        dragIndex = -1;
        ReleaseMouseCapture();
        if (row != null)
        {
            int n = row.GraphValues().Count;
            if (pendingCount < 0 || n > pendingCount)
            {
                // ふつうは、ドラッグで変わるのは値(縦)だけ。中間点横移動モードの時は、横の位置(フレーム)も
                if (!dragHorizontalOnly) row.SetPointFromGraph(i, v);
                if (dragFrac >= 0) row.MovePointTo(i, dragFrac);
            }
            row.LockGraphRange(null, null);
        }
        pendingCount = -1;
        dragFrac = -1;
        dragHorizontalOnly = false;
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        if (drawn != null) FinishStroke();
    }

    /// <summary>中間点を右クリックで消す</summary>
    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        var row = Row;
        if (row == null || dragIndex >= 0 || drawn != null) return;
        int i = HitPoint(e.GetPosition(this));
        int n = row.GraphValues().Count;
        if (i <= 0 || i >= n - 1) return;
        row.RemovePointAt(i);
        e.Handled = true;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        if (dragIndex < 0 && (hoverIndex >= 0 || hoverCurve)) { hoverIndex = -1; hoverCurve = false; Cursor = null; InvalidateVisual(); }
    }
}
