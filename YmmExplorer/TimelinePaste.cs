using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace YmmExplorer;

/// <summary>
/// クリップボードにセットしたプリセットを、YMM4のタイムラインに貼り付ける(Ctrl+Vを送る)。
/// YMM4のタイムラインは、貼り付けるとシークバーの位置に素材が置かれる前提です。
/// </summary>
internal static class TimelinePaste
{
    /// <summary>成功したら null、貼り付けを見送った場合はその理由を返す</summary>
    public static Task<string?> PasteAsync(object? ownView) => SendKeysAsync(ownView, "Ctrl+V");

    /// <summary>
    /// YMM4のタイムラインにフォーカスを移して、keys(例: "Ctrl+V"、"Ctrl+Shift+V")のキー操作を送る。
    /// 成功したら null、見送った場合はその理由を返す。
    /// </summary>
    public static async Task<string?> SendKeysAsync(object? ownView, string keys)
    {
        if (!TryParseKeys(keys, out var modifiers, out var key))
            return $"キーの指定「{keys}」を読み取れませんでした(例: Ctrl+V)。";

        var main = Application.Current?.MainWindow;
        if (main == null) return "YMM4のメインウィンドウが見つかりませんでした。";

        if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
        main.Activate();

        // タイムラインらしいパーツがあれば、そこにキーボードフォーカスを移す
        var timeline = FindTimeline(main);
        if (timeline != null)
        {
            timeline.Focus();
            Keyboard.Focus(timeline);
        }
        await Task.Delay(200);

        // 文字を入力する欄にフォーカスが残っていると、キー操作が欄の中に入ってしまう。
        // まず、欄からフォーカスを外して(YMM4のウィンドウ全体でキー操作を受ける状態にして)から送る。
        var focused = Keyboard.FocusedElement as DependencyObject;
        var cleared = false;
        if (focused != null && IsTextInput(focused))
        {
            Keyboard.ClearFocus();
            cleared = true;
            await Task.Delay(100);
            focused = Keyboard.FocusedElement as DependencyObject;
        }

        // 外せなかった時や、このプラグインの中にフォーカスがある時は、間違った場所に送らないよう見送る
        if (focused == null)
        {
            if (!cleared) return "YMM4側にフォーカスがありませんでした。";
        }
        else if (IsTextInput(focused)) return "文字を入力する欄からフォーカスを外せなかったため、自動のキー操作は見送りました。";
        else if (ownView is DependencyObject own && IsInside(focused, own))
            return "タイムラインにフォーカスを移せませんでした。";

        return SendKeys(modifiers, key) ? null : "キー操作を送れませんでした。";
    }

    static FrameworkElement? FindTimeline(DependencyObject root)
    {
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);
        int visited = 0;
        while (queue.Count > 0 && visited++ < 30000)
        {
            var d = queue.Dequeue();
            if (d is FrameworkElement fe && fe.IsVisible && fe.Focusable
                && fe.GetType().Name.Contains("Timeline", StringComparison.OrdinalIgnoreCase))
                return fe;
            if (d is not Visual and not System.Windows.Media.Media3D.Visual3D) continue;
            int n = VisualTreeHelper.GetChildrenCount(d);
            for (int i = 0; i < n; i++) queue.Enqueue(VisualTreeHelper.GetChild(d, i));
        }
        return null;
    }

    static DependencyObject? Parent(DependencyObject d)
        => d is Visual or System.Windows.Media.Media3D.Visual3D
            ? VisualTreeHelper.GetParent(d) ?? LogicalTreeHelper.GetParent(d)
            : LogicalTreeHelper.GetParent(d);

    static bool IsTextInput(DependencyObject d)
    {
        for (var c = d; c != null; c = Parent(c))
            if (c is TextBoxBase or PasswordBox or ComboBox { IsEditable: true }) return true;
        return false;
    }

    static bool IsInside(DependencyObject d, DependencyObject ancestor)
    {
        for (var c = d; c != null; c = Parent(c))
            if (ReferenceEquals(c, ancestor)) return true;
        return false;
    }

    // ---- キー送信 ----
    /// <summary>"Ctrl+Shift+V" のような指定を、修飾キーと本体のキーに分ける</summary>
    static bool TryParseKeys(string spec, out List<ushort> modifiers, out ushort key)
    {
        modifiers = new List<ushort>();
        key = 0;
        var parts = (spec ?? "").Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;

        foreach (var m in parts[..^1])
        {
            switch (m.ToLowerInvariant())
            {
                case "ctrl": case "control": modifiers.Add(0x11); break;
                case "shift": modifiers.Add(0x10); break;
                case "alt": modifiers.Add(0x12); break;
                case "win": modifiers.Add(0x5B); break;
                default: return false;
            }
        }

        var k = parts[^1];
        if (k.Length == 1 && char.IsAsciiLetterOrDigit(k[0])) { key = char.ToUpperInvariant(k[0]); return true; }
        if ((k[0] is 'F' or 'f') && int.TryParse(k.AsSpan(1), out var f) && f is >= 1 and <= 24) { key = (ushort)(0x70 + f - 1); return true; }
        var named = k.ToLowerInvariant() switch
        {
            "enter" => 0x0D, "delete" or "del" => 0x2E, "insert" or "ins" => 0x2D, "home" => 0x24, "end" => 0x23,
            "pageup" => 0x21, "pagedown" => 0x22, "tab" => 0x09, "space" => 0x20, "esc" or "escape" => 0x1B,
            "up" => 0x26, "down" => 0x28, "left" => 0x25, "right" => 0x27, _ => 0,
        };
        if (named == 0) return false;
        key = (ushort)named;
        return true;
    }

    static bool SendKeys(List<ushort> modifiers, ushort key)
    {
        var inputs = new List<INPUT>();
        foreach (var m in modifiers) inputs.Add(Key(m, false));
        inputs.Add(Key(key, false));
        inputs.Add(Key(key, true));
        for (int i = modifiers.Count - 1; i >= 0; i--) inputs.Add(Key(modifiers[i], true));
        return SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>()) == inputs.Count;
    }

    static INPUT Key(ushort vk, bool up) => new()
    {
        type = 1,   // INPUT_KEYBOARD
        U = new InputUnion { ki = new KEYBDINPUT { wVk = vk, dwFlags = up ? 0x0002u : 0u } },
    };

    [StructLayout(LayoutKind.Sequential)]
    struct INPUT { public uint type; public InputUnion U; }

    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT { public int dx, dy; public uint mouseData, dwFlags, time; public IntPtr dwExtraInfo; }

    [StructLayout(LayoutKind.Sequential)]
    struct KEYBDINPUT { public ushort wVk, wScan; public uint dwFlags, time; public IntPtr dwExtraInfo; }

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}
