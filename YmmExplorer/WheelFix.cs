using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace YmmExplorer;

/// <summary>
/// ホイールで、外側の一覧がスクロールしない時がある問題の対策。
/// 内側の部品(小さなスクロール欄・数値の入力欄・閉じたプルダウンなど)がホイールを受け取ってしまい、
/// 内側がそれ以上動けない時も、外側へ回らないのが原因。
/// 外側の ScrollViewer に WheelFix.Enabled="True" をつけると、
/// 内側が動ける時は内側を、動けない時は外側をスクロールする。閉じたプルダウンの値も、ホイールでは変えない
/// </summary>
public static class WheelFix
{
    public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
        "Enabled", typeof(bool), typeof(WheelFix), new PropertyMetadata(false, OnEnabledChanged));

    public static bool GetEnabled(DependencyObject d) => (bool)d.GetValue(EnabledProperty);
    public static void SetEnabled(DependencyObject d, bool value) => d.SetValue(EnabledProperty, value);

    /// <summary>この部品の上では、ホイールを自分で使う(拡大縮小など)。外側へは回さない</summary>
    public static readonly DependencyProperty OwnsWheelProperty = DependencyProperty.RegisterAttached(
        "OwnsWheel", typeof(bool), typeof(WheelFix), new PropertyMetadata(false));

    public static bool GetOwnsWheel(DependencyObject d) => (bool)d.GetValue(OwnsWheelProperty);
    public static void SetOwnsWheel(DependencyObject d, bool value) => d.SetValue(OwnsWheelProperty, value);

    static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer sv) return;
        sv.PreviewMouseWheel -= OnPreviewMouseWheel;
        if (e.NewValue is true) sv.PreviewMouseWheel += OnPreviewMouseWheel;
    }

    static DependencyObject? Parent(DependencyObject d)
        => d is Visual || d is System.Windows.Media.Media3D.Visual3D ? VisualTreeHelper.GetParent(d) ?? LogicalTreeHelper.GetParent(d) : LogicalTreeHelper.GetParent(d);

    static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Handled || sender is not ScrollViewer outer) return;
        if (Keyboard.Modifiers != ModifierKeys.None) return;          // Ctrl + ホイール(表示倍率)などは、そのまま
        bool up = e.Delta > 0;

        // マウスの下から外側まで、たどる
        for (var d = e.OriginalSource as DependencyObject; d != null && !ReferenceEquals(d, outer); d = Parent(d))
        {
            if (GetOwnsWheel(d)) return;
            if (d is Popup) return;                                   // 開いたプルダウンの中などは、そのまま
            if (d is ComboBox cb)
            {
                if (cb.IsDropDownOpen) return;
                continue;                                             // 閉じたプルダウンは、値を変えずに外側をスクロール
            }
            if (d is ScrollViewer inner && inner.ScrollableHeight > 0.5)
            {
                bool can = up ? inner.VerticalOffset > 0.5 : inner.VerticalOffset < inner.ScrollableHeight - 0.5;
                if (can) return;                                      // 内側が動けるなら、内側に任せる
            }
        }

        // 内側が動けない: 外側をスクロールする
        if (outer.ScrollableHeight <= 0.5) return;
        double step = Math.Max(1, SystemParameters.WheelScrollLines) * 16.0 * Math.Abs(e.Delta) / 120.0;
        outer.ScrollToVerticalOffset(Math.Clamp(outer.VerticalOffset + (up ? -step : step), 0, outer.ScrollableHeight));
        e.Handled = true;
    }
}
