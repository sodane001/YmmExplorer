using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace YmmExplorer;

/// <summary>
/// 2Dのカラーパッド。左の四角で「鮮やかさ(横)と明るさ(縦)」を、右の帯で「色あい」を、ドラッグで決める。
/// </summary>
public sealed class ColorPad : FrameworkElement
{
    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor), typeof(Color), typeof(ColorPad),
        new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender,
            (d, e) => ((ColorPad)d).OnColorChanged((Color)e.NewValue)));
    public Color SelectedColor { get => (Color)GetValue(SelectedColorProperty); set => SetValue(SelectedColorProperty, value); }

    const double HueWidth = 24, Gap = 10;
    double hue, sat, val = 1;
    bool internalChange;
    int dragging;       // 0=なし / 1=四角 / 2=色あいの帯

    static readonly LinearGradientBrush HueBrush = MakeHueBrush();
    static readonly LinearGradientBrush BlackBrush = MakeBlackBrush();

    static LinearGradientBrush MakeHueBrush()
    {
        var b = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
        for (int i = 0; i <= 6; i++)
        {
            ColorTools.HsvToRgb(i * 60 % 360, 1, 1, out var r, out var g, out var bl);
            b.GradientStops.Add(new GradientStop(Color.FromRgb(r, g, bl), i / 6.0));
        }
        b.Freeze();
        return b;
    }

    static LinearGradientBrush MakeBlackBrush()
    {
        var b = new LinearGradientBrush(Color.FromArgb(0, 0, 0, 0), Colors.Black, new Point(0, 0), new Point(0, 1));
        b.Freeze();
        return b;
    }

    void OnColorChanged(Color c)
    {
        // ドラッグ中は、外から戻ってくる(少し遅れた)色で、印の位置を動かさない
        if (internalChange || dragging != 0) return;
        ColorTools.RgbToHsv(c.R, c.G, c.B, out var h, out var s, out var v);
        // 灰色・黒の時は色あいが決まらないので、いまの色あいを保つ
        if (s > 0.0001 && v > 0.0001) hue = h;
        sat = s;
        val = v;
    }

    Rect Square => new(0, 0, Math.Max(10, ActualWidth - HueWidth - Gap), Math.Max(10, ActualHeight));
    Rect HueBar => new(Math.Max(10, ActualWidth - HueWidth), 0, HueWidth, Math.Max(10, ActualHeight));

    protected override void OnRender(DrawingContext dc)
    {
        var sq = Square;
        var bar = HueBar;
        ColorTools.HsvToRgb(hue, 1, 1, out var r, out var g, out var b);
        var white = new LinearGradientBrush(Colors.White, Color.FromRgb(r, g, b), new Point(0, 0), new Point(1, 0));
        white.Freeze();
        var clip = new RectangleGeometry(sq, 6, 6);
        dc.PushClip(clip);
        dc.DrawRectangle(white, null, sq);
        dc.DrawRectangle(BlackBrush, null, sq);
        dc.Pop();
        dc.PushClip(new RectangleGeometry(bar, 6, 6));
        dc.DrawRectangle(HueBrush, null, bar);
        dc.Pop();

        // 印(四角の中の丸、帯の上の線)
        var p = new Point(sq.Left + sat * sq.Width, sq.Top + (1 - val) * sq.Height);
        var dark = new Pen(Brushes.Black, 3);
        var light = new Pen(Brushes.White, 1.6);
        dark.Freeze(); light.Freeze();
        dc.DrawEllipse(null, dark, p, 7, 7);
        dc.DrawEllipse(null, light, p, 7, 7);
        double hy = bar.Top + hue / 360.0 * bar.Height;
        var mark = new Rect(bar.Left - 2, hy - 3, bar.Width + 4, 6);
        dc.DrawRoundedRectangle(null, dark, mark, 3, 3);
        dc.DrawRoundedRectangle(null, light, mark, 3, 3);
    }

    void Update(Point p)
    {
        if (dragging == 1)
        {
            var sq = Square;
            sat = Math.Clamp((p.X - sq.Left) / sq.Width, 0, 1);
            val = 1 - Math.Clamp((p.Y - sq.Top) / sq.Height, 0, 1);
        }
        else if (dragging == 2)
        {
            var bar = HueBar;
            hue = Math.Clamp((p.Y - bar.Top) / bar.Height, 0, 0.9999) * 360;
        }
        ColorTools.HsvToRgb(hue, sat, val, out var r, out var g, out var b);
        internalChange = true;
        try { SetCurrentValue(SelectedColorProperty, Color.FromArgb(SelectedColor.A, r, g, b)); }
        finally { internalChange = false; }
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        var p = e.GetPosition(this);
        dragging = p.X >= HueBar.Left - Gap / 2 ? 2 : 1;
        CaptureMouse();
        Update(p);
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (dragging != 0) Update(e.GetPosition(this));
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (dragging == 0) return;
        Update(e.GetPosition(this));
        dragging = 0;
        ReleaseMouseCapture();
        e.Handled = true;
    }
}
