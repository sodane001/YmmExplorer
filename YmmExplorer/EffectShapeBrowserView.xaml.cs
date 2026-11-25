using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YmmExplorer;

/// <summary>演出図形タブの画面</summary>
public partial class EffectShapeBrowserView : UserControl
{
    const double TileWidth = 176;

    public EffectShapeBrowserView() => InitializeComponent();

    EffectShapeBrowserVm? Vm => DataContext as EffectShapeBrowserVm;

    void RowList_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not FrameworkElement list || list.ActualWidth <= 0) return;
        var width = list.ActualWidth - SystemParameters.VerticalScrollBarWidth - 4;
        if (width > 0) Vm?.SetColumns((int)(width / TileWidth));
    }

    void Tile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Vm is not { } vm || (sender as FrameworkElement)?.DataContext is not EffectShapeInfo it) return;
        vm.Activate(it, e.ClickCount == 2);
        e.Handled = true;
    }
}
