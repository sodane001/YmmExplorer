using System.Windows;
using System.Windows.Controls;

namespace YmmExplorer;

/// <summary>
/// 幅に合わせて列の数を変え、カードを「いちばん低い列」へ順に積むパネル。
/// 列の幅は表示幅いっぱいに広がるので、右側に余白が残らない(アイテムタブ用)。
/// </summary>
public sealed class StretchColumnsPanel : Panel
{
    public static readonly DependencyProperty MinColumnWidthProperty = DependencyProperty.Register(
        nameof(MinColumnWidth), typeof(double), typeof(StretchColumnsPanel),
        new FrameworkPropertyMetadata(420.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public double MinColumnWidth { get => (double)GetValue(MinColumnWidthProperty); set => SetValue(MinColumnWidthProperty, value); }

    public static readonly DependencyProperty GapProperty = DependencyProperty.Register(
        nameof(Gap), typeof(double), typeof(StretchColumnsPanel),
        new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public double Gap { get => (double)GetValue(GapProperty); set => SetValue(GapProperty, value); }

    int columns = 1;
    double columnWidth = 100;

    List<UIElement> Shown()
    {
        var list = new List<UIElement>();
        foreach (UIElement c in InternalChildren)
            if (c.Visibility != Visibility.Collapsed) list.Add(c);
        return list;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double avail = double.IsInfinity(availableSize.Width) ? 800 : availableSize.Width;
        var shown = Shown();
        foreach (UIElement c in InternalChildren)
            if (c.Visibility == Visibility.Collapsed) c.Measure(new Size(0, 0));

        columns = Math.Max(1, (int)Math.Floor((avail + Gap) / (Math.Max(50, MinColumnWidth) + Gap)));
        columns = Math.Min(columns, Math.Max(1, shown.Count));
        columnWidth = Math.Max(10, (avail - Gap * (columns - 1)) / columns);

        var heights = new double[columns];
        foreach (var c in shown)
        {
            c.Measure(new Size(columnWidth, double.PositiveInfinity));
            int col = 0;
            for (int i = 1; i < columns; i++) if (heights[i] < heights[col] - 0.5) col = i;
            heights[col] += c.DesiredSize.Height;
        }
        return new Size(avail, heights.Max());
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var heights = new double[columns];
        foreach (var c in Shown())
        {
            int col = 0;
            for (int i = 1; i < columns; i++) if (heights[i] < heights[col] - 0.5) col = i;
            c.Arrange(new Rect(col * (columnWidth + Gap), heights[col], columnWidth, c.DesiredSize.Height));
            heights[col] += c.DesiredSize.Height;
        }
        return new Size(finalSize.Width, Math.Max(finalSize.Height, heights.Max()));
    }
}
