using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YmmExplorer;

/// <summary>テキストタブの画面(標準のテンプレートを、演出図形タブと同じ形で並べる)</summary>
public partial class TextBrowserView : UserControl
{
    const double TileWidth = 176;

    public TextBrowserView() => InitializeComponent();

    TextBrowserVm? Vm => DataContext as TextBrowserVm;

    void RowList_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not FrameworkElement list || list.ActualWidth <= 0) return;
        var width = list.ActualWidth - SystemParameters.VerticalScrollBarWidth - 4;
        if (width > 0) Vm?.SetColumns((int)(width / TileWidth));
    }

    void Tile_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (Vm is not { } vm || (sender as FrameworkElement)?.DataContext is not TextTplItem it) return;
        vm.Activate(it, e.ClickCount == 2);
        e.Handled = true;
    }

    void Star_Click(object sender, RoutedEventArgs e)
    {
        if (Vm is { } vm && (sender as FrameworkElement)?.DataContext is TextTplItem it) vm.ToggleFavorite(it);
        e.Handled = true;
    }
}
