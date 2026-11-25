using System.Windows;
using System.Windows.Controls;

namespace YmmExplorer;

/// <summary>
/// タイルを「いちばん低い列」から順に積んでいくパネル(Eagleのウォーターフォール表示)。
/// タイルの高さがそろっていれば、ふつうの格子状になります。
/// </summary>
public sealed class WaterfallPanel : Panel
{
    int columns = 1;
    double columnWidth = 100;

    /// <summary>true なら、幅がばらばらのタイルを、左から右へ並べて折り返す(行そろえ表示)</summary>
    public static readonly DependencyProperty FlowProperty = DependencyProperty.Register(
        nameof(Flow), typeof(bool), typeof(WaterfallPanel),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));
    public bool Flow { get => (bool)GetValue(FlowProperty); set => SetValue(FlowProperty, value); }

    Size FlowLayout(double avail, bool arrange)
    {
        double x = 0, y = 0, rowH = 0, maxW = 0;
        foreach (UIElement c in InternalChildren)
        {
            var d = c.DesiredSize;
            if (x > 0 && x + d.Width > avail + 0.5) { x = 0; y += rowH; rowH = 0; }
            if (arrange) c.Arrange(new Rect(x, y, d.Width, d.Height));
            x += d.Width;
            rowH = Math.Max(rowH, d.Height);
            maxW = Math.Max(maxW, x);
        }
        return new Size(maxW, y + rowH);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double avail = double.IsInfinity(availableSize.Width) ? 800 : availableSize.Width;
        foreach (UIElement c in InternalChildren) c.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        if (Flow) return FlowLayout(avail, false);
        columnWidth = InternalChildren.Count == 0 ? 100 : Math.Max(20, InternalChildren.Cast<UIElement>().Max(c => c.DesiredSize.Width));
        columns = Math.Max(1, (int)Math.Floor(avail / columnWidth));
        var heights = new double[columns];
        foreach (UIElement c in InternalChildren)
        {
            int col = 0;
            for (int i = 1; i < columns; i++) if (heights[i] < heights[col]) col = i;
            heights[col] += c.DesiredSize.Height;
        }
        return new Size(columns * columnWidth, heights.Length == 0 ? 0 : heights.Max());
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Flow)
        {
            var used = FlowLayout(finalSize.Width, true);
            return new Size(Math.Max(finalSize.Width, used.Width), used.Height);
        }
        var heights = new double[columns];
        foreach (UIElement c in InternalChildren)
        {
            int col = 0;
            for (int i = 1; i < columns; i++) if (heights[i] < heights[col]) col = i;
            c.Arrange(new Rect(col * columnWidth, heights[col], columnWidth, c.DesiredSize.Height));
            heights[col] += c.DesiredSize.Height;
        }
        return new Size(Math.Max(finalSize.Width, columns * columnWidth), heights.Length == 0 ? 0 : heights.Max());
    }
}
