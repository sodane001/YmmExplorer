using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace YmmExplorer;

/// <summary>ショートカットキー1件の割り当て</summary>
public sealed class ShortcutBinding
{
    /// <summary>種類: preset / shape / video-effect / audio-effect / text / file</summary>
    public string Kind { get; set; } = "";

    /// <summary>割り当て先のID(ファイル・フォルダーは、そのパス)</summary>
    public string TargetId { get; set; } = "";

    public string Name { get; set; } = "";

    /// <summary>キーの組み合わせ(例: Ctrl+Shift+1)</summary>
    public string Keys { get; set; } = "";
}

/// <summary>キーの置き換え1件: From を押したら、YMM4へ To のキー操作を送る(To が空なら、From を無効にする)</summary>
public sealed class KeyRemap
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public bool Enabled { get; set; } = true;
}

public sealed class ShortcutFile
{
    public List<ShortcutBinding> Bindings { get; set; } = new();
    public List<KeyRemap> Remaps { get; set; } = new();
}

/// <summary>ファイル・プリセット・図形・エフェクト・テキストに付けたショートカットキーの保存先(shortcuts.json)</summary>
public sealed class ShortcutStore
{
    static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "shortcuts.json");

    public static ShortcutStore Instance { get; } = new();

    readonly object gate = new();
    ShortcutFile data = new();

    public event Action? Changed;

    ShortcutStore()
    {
        try
        {
            if (File.Exists(FilePath))
                data = JsonSerializer.Deserialize<ShortcutFile>(File.ReadAllText(FilePath)) ?? new();
        }
        catch { data = new(); }
    }

    public static string KindLabel(string kind) => kind switch
    {
        "preset" => "プリセット",
        "shape" => "図形",
        "video-effect" => "映像エフェクト",
        "audio-effect" => "音声エフェクト",
        "text" => "テキスト",
        "file" => "ファイル・フォルダー",
        "action" => "このツールの操作",
        _ => kind,
    };

    public IReadOnlyList<ShortcutBinding> All()
    {
        lock (gate) return data.Bindings.ToList();
    }

    public string KeysFor(string kind, string id)
    {
        lock (gate) return data.Bindings.FirstOrDefault(b => b.Kind == kind && b.TargetId == id)?.Keys ?? "";
    }

    static bool SameKeys(string a, string b) =>
        string.Equals(KeyChord.Normalize(a), KeyChord.Normalize(b), StringComparison.OrdinalIgnoreCase);

    public ShortcutBinding? Find(string keys) => Find(keys, 0);

    /// <summary>
    /// 押されたキーの割り当てを探す。同じキーが、ウィンドウ別(YmmExplorer ２ だけ、など)にも付いている時は、
    /// 最後にさわったウィンドウ(activeWindow)のもの → ウィンドウを決めていないもの → 他のウィンドウのもの、の順
    /// </summary>
    public ShortcutBinding? Find(string keys, int activeWindow)
    {
        if (string.IsNullOrEmpty(keys)) return null;
        List<ShortcutBinding> hits;
        lock (gate) hits = data.Bindings.Where(b => SameKeys(b.Keys, keys)).ToList();
        return hits.FirstOrDefault(b => activeWindow > 0 && ActionShortcuts.WindowOf(b) == activeWindow)
            ?? hits.FirstOrDefault(b => ActionShortcuts.WindowOf(b) == 0)
            ?? hits.FirstOrDefault();
    }

    /// <summary>押したキーの「続き」(さらにキーを足した組み合わせ)が割り当てられているか</summary>
    public bool HasLonger(string keys)
    {
        if (string.IsNullOrEmpty(keys)) return false;
        lock (gate)
            return data.Bindings.Any(b => KeyChord.IsLongerThan(b.Keys, keys))
                || (data.Remaps ??= new()).Any(r => r.Enabled && KeyChord.IsLongerThan(r.From, keys));
    }

    /// <summary>キーを割り当てる(空なら解除)。同じキーが他に割り当てられていたら、そちらを解除して、その名前を返す</summary>
    public string? Set(string kind, string id, string name, string keys)
    {
        string? replaced = null;
        lock (gate)
        {
            data.Bindings.RemoveAll(b => b.Kind == kind && b.TargetId == id);
            if (!string.IsNullOrEmpty(keys))
            {
                // 同じキーでも、実行するウィンドウが違う操作ショートカットどうしは、両方残す
                int window = ActionShortcuts.WindowOf(kind, id);
                var other = data.Bindings.FirstOrDefault(b => SameKeys(b.Keys, keys) && ActionShortcuts.WindowOf(b) == window);
                if (other != null)
                {
                    replaced = other.Name;
                    data.Bindings.Remove(other);
                }
                data.Bindings.Add(new ShortcutBinding { Kind = kind, TargetId = id, Name = name, Keys = keys });
            }
        }
        Save();
        Changed?.Invoke();
        return replaced;
    }

    // ---- キーの置き換え
    public IReadOnlyList<KeyRemap> Remaps()
    {
        lock (gate) return (data.Remaps ??= new()).ToList();
    }

    public KeyRemap? FindRemap(string keys)
    {
        if (string.IsNullOrEmpty(keys)) return null;
        lock (gate) return (data.Remaps ??= new()).FirstOrDefault(r => r.Enabled && SameKeys(r.From, keys));
    }

    public KeyRemap AddRemap()
    {
        var r = new KeyRemap();
        lock (gate) (data.Remaps ??= new()).Add(r);
        Save();
        return r;
    }

    public void RemoveRemap(string id)
    {
        lock (gate) (data.Remaps ??= new()).RemoveAll(r => r.Id == id);
        Save();
    }

    /// <summary>名前が変わった時に、一覧の表示名も合わせる</summary>
    public void Rename(string kind, string id, string name)
    {
        bool changed = false;
        lock (gate)
        {
            var b = data.Bindings.FirstOrDefault(x => x.Kind == kind && x.TargetId == id);
            if (b != null && b.Name != name) { b.Name = name; changed = true; }
        }
        if (changed) { Save(); Changed?.Invoke(); }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            lock (gate)
                File.WriteAllText(FilePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}

/// <summary>
/// キー入力を「Ctrl+Shift+1」「Ctrl+A+S+D」のような文字にする。
/// Ctrl・Alt・Shift・Win に加えて、ふつうのキーも何個でも同時押しで組み合わせられる(並びは押した順ではなく、決まった順にそろえる)。
/// </summary>
public static class KeyChord
{
    // いま押されている(修飾キー以外の)キー
    static readonly List<Key> held = new();

    public static Key RealKey(KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.ImeProcessed) key = e.ImeProcessedKey;
        return key;
    }

    public static bool IsModifierOrNone(Key key) =>
        key is Key.None or Key.DeadCharProcessed or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin or Key.System or Key.ImeProcessed;

    /// <summary>キーが押された(ShortcutHost から呼ぶ)</summary>
    public static void NoteDown(Key key)
    {
        if (IsModifierOrNone(key)) return;
        if (!held.Contains(key)) held.Add(key);
    }

    /// <summary>キーが離された</summary>
    public static void NoteUp(Key key) => held.Remove(key);

    // 他の画面へ移った間に離されたキーは、離したことにする
    static void Prune(Key except)
    {
        held.RemoveAll(k => k != except && !Keyboard.IsKeyDown(k));
    }

    /// <summary>まだ押されたままの(修飾キー以外の)キーの数</summary>
    public static int HeldCount
    {
        get { Prune(Key.None); return held.Count; }
    }

    public static string? FromEvent(KeyEventArgs e)
    {
        var key = RealKey(e);
        if (IsModifierOrNone(key)) return null;
        NoteDown(key);
        Prune(key);
        return Compose(Keyboard.Modifiers, held);
    }

    public static string Compose(ModifierKeys m, IEnumerable<Key> keys)
    {
        var parts = new List<string>();
        if (m.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (m.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (m.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (m.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
        parts.AddRange(keys.Distinct().OrderBy(k => (int)k).Select(KeyName));
        return string.Join("+", parts);
    }

    /// <summary>文字を、決まった並びに直す(古い設定や手入力との比較用)</summary>
    public static string Normalize(string chord) =>
        TryParse(chord, out var m, out var keys) ? Compose(m, keys) : (chord ?? "").Trim();

    /// <summary>「Ctrl+Shift+A+S」のような文字を、修飾キーとキー(1個以上)に分ける</summary>
    public static bool TryParse(string chord, out ModifierKeys modifiers, out List<Key> keys)
    {
        modifiers = ModifierKeys.None;
        keys = new List<Key>();
        if (string.IsNullOrWhiteSpace(chord)) return false;
        var tokens = chord.Trim().Split('+');
        // 「+」キーそのものは、区切りの間が空になる(Ctrl++ → "Ctrl", "", "")ので、空が2つで「+」1個
        int empties = 0;
        foreach (var raw in tokens)
        {
            var t = raw.Trim();
            if (t.Length == 0)
            {
                empties++;
                if (empties == 2) { keys.Add(Key.OemPlus); empties = 0; }
                continue;
            }
            empties = 0;
            switch (t)
            {
                case "Ctrl": modifiers |= ModifierKeys.Control; continue;
                case "Alt": modifiers |= ModifierKeys.Alt; continue;
                case "Shift": modifiers |= ModifierKeys.Shift; continue;
                case "Win": modifiers |= ModifierKeys.Windows; continue;
            }
            var k = ParseKey(t);
            if (k == Key.None) return false;
            if (!keys.Contains(k)) keys.Add(k);
        }
        return keys.Count > 0;
    }

    /// <summary>a のキーが、b のキーをすべて含み、さらに多い(同じ修飾キーで)</summary>
    public static bool IsLongerThan(string a, string b)
    {
        if (!TryParse(a, out var ma, out var ka) || !TryParse(b, out var mb, out var kb)) return false;
        return ma == mb && ka.Count > kb.Count && kb.All(ka.Contains);
    }

    static Key ParseKey(string name)
    {
        if (name.Length == 1 && char.IsDigit(name[0])) return Key.D0 + (name[0] - '0');
        return name switch
        {
            "-" => Key.OemMinus,
            "," => Key.OemComma,
            "." => Key.OemPeriod,
            "Enter" => Key.Return,
            _ => Enum.TryParse<Key>(name, true, out var k) ? k : Key.None,
        };
    }

    static string KeyName(Key k)
    {
        if (k >= Key.D0 && k <= Key.D9) return ((int)k - (int)Key.D0).ToString();
        return k switch
        {
            Key.OemPlus => "+",
            Key.OemMinus => "-",
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.Return => "Enter",
            _ => k.ToString(),
        };
    }
}

/// <summary>
/// 「キーを押して記録」する入力欄。クリックして、実際にキーを押すと、その組み合わせが入る。
/// Esc: 取りやめ / Delete・Backspace: 解除
/// </summary>
public class KeyRecorder : TextBox
{
    public static readonly DependencyProperty KeysProperty = DependencyProperty.Register(
        nameof(Keys), typeof(string), typeof(KeyRecorder),
        new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (d, _) => ((KeyRecorder)d).Refresh()));

    public string Keys
    {
        get => (string)GetValue(KeysProperty);
        set => SetValue(KeysProperty, value ?? "");
    }

    public KeyRecorder()
    {
        IsReadOnly = true;
        IsReadOnlyCaretVisible = false;
        Cursor = Cursors.Hand;
        IsUndoEnabled = false;
        Refresh();
    }

    void Refresh()
    {
        if (!IsKeyboardFocused) Text = string.IsNullOrEmpty(Keys) ? "(未設定: クリックしてキーを押す)" : Keys;
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        Text = "キーを押してください…(何個でも同時押しOK / Esc: 取りやめ / Delete: 解除)";
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        pending = null;
        Refresh();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e) => Record(e);

    string? pending;      // 押している途中の組み合わせ(すべてのキーを離した時に決定する)

    /// <summary>
    /// 押されたキーを記録する。Ctrl+S のような組み合わせは、YMM4が先に受け取ってしまい、この入力欄まで届かないことがあるので、
    /// ShortcutHost(YMM4より先にキーを見る場所)からも呼ばれる。
    /// キーは何個でも同時に押せる。押したキーをすべて離した時に決まる。
    /// </summary>
    public void Record(KeyEventArgs e)
    {
        var key = KeyChord.RealKey(e);
        e.Handled = true;
        bool alone = pending == null && Keyboard.Modifiers == ModifierKeys.None;
        if (key == Key.Escape && alone) { pending = null; Keyboard.ClearFocus(); return; }
        if (key is Key.Delete or Key.Back && alone)
        {
            Keys = "";
            Keyboard.ClearFocus();
            return;
        }
        var chord = KeyChord.FromEvent(e);
        if (chord == null) return;          // Ctrlなど修飾キーだけの間は、次のキーを待つ
        pending = chord;
        Text = chord + "  …(すべてのキーを離すと決定)";
    }

    /// <summary>キーが離された時。押していたキーがすべて離れたら、組み合わせを決定する</summary>
    public void RecordUp(KeyEventArgs e)
    {
        e.Handled = true;
        KeyChord.NoteUp(KeyChord.RealKey(e));
        if (pending == null || KeyChord.HeldCount > 0) return;
        var chord = pending;
        pending = null;
        Keys = chord;
        Keyboard.ClearFocus();
    }

    protected override void OnPreviewKeyUp(KeyEventArgs e) => RecordUp(e);
}

/// <summary>YMM4のどの画面にいても、割り当てたショートカットキーを受け付ける</summary>
public static class ShortcutHost
{
    static readonly List<ExplorerViewModel> Targets = new();
    static bool attached;
    static ExplorerViewModel? lastActive;
    static string? deferred;     // 「続き」があるので、いったん止めているキー(続きが押されずに離されたら、YMM4へ送り直す)

    /// <summary>
    /// YMM4より先にキーを見たい画面(一覧で Enter を押した時の適用など)。true を返すと、そのキーはYMM4へ渡さない。
    /// YMM4がウィンドウ全体で Enter などを先に使ってしまい、一覧まで届かないことがあるため
    /// </summary>
    public static event Func<KeyEventArgs, bool>? EarlyKeyDown;

    /// <summary>最後にさわったウィンドウ。タブの移動などは、このウィンドウで行う</summary>
    public static void Touch(ExplorerViewModel? vm)
    {
        if (vm != null) lastActive = vm;
    }

    /// <summary>最後にさわったウィンドウの番号(1〜5、分からなければ 0)</summary>
    public static int ActiveWindow
    {
        get
        {
            lock (Targets)
                return lastActive != null && Targets.Contains(lastActive) ? lastActive.WindowNumber
                    : Targets.FirstOrDefault()?.WindowNumber ?? 0;
        }
    }

    /// <summary>いま開いている YmmExplorer のウィンドウ(最後にさわったものが先頭)</summary>
    public static List<ExplorerViewModel> OpenWindows()
    {
        lock (Targets)
        {
            var list = Targets.ToList();
            if (lastActive != null && list.Remove(lastActive)) list.Insert(0, lastActive);
            return list;
        }
    }

    public static void Attach(ExplorerViewModel vm)
    {
        lock (Targets) Targets.Add(vm);
        TimelineItemOpen.Ensure();
        if (attached) return;
        InputManager.Current.PreProcessInput += OnPreProcess;
        attached = true;
    }

    public static void Detach(ExplorerViewModel vm)
    {
        lock (Targets)
        {
            Targets.Remove(vm);
            if (lastActive == vm) lastActive = null;
            if (Targets.Count > 0 || !attached) return;
        }
        InputManager.Current.PreProcessInput -= OnPreProcess;
        attached = false;
    }

    static void OnPreProcess(object sender, PreProcessInputEventArgs e)
    {
        if (e.StagingItem.Input is not KeyEventArgs ke) return;
        var focused = Keyboard.FocusedElement;
        if (ke.RoutedEvent == Keyboard.PreviewKeyUpEvent)
        {
            if (focused is KeyRecorder rec)
            {
                rec.RecordUp(ke);
                e.Cancel();
                return;
            }
            KeyChord.NoteUp(KeyChord.RealKey(ke));
            if (deferred != null && KeyChord.HeldCount == 0)
            {
                var d = deferred;
                deferred = null;
                if (!KeyChord.IsModifierOrNone(KeyChord.RealKey(ke))) KeySender.Send(d);
            }
            return;
        }
        if (ke.RoutedEvent != Keyboard.PreviewKeyDownEvent) return;
        if (focused is not KeyRecorder && EarlyKeyDown is { } early)
            foreach (Func<KeyEventArgs, bool> h in early.GetInvocationList())
            {
                bool used = false;
                try { used = h(ke); } catch { }
                if (used) { e.Cancel(); return; }
            }
        if (focused is KeyRecorder recorder)
        {
            // 記録中: YMM4のショートカットより先に受け取って、押された組み合わせを記録する(割り当ては実行しない)
            recorder.Record(ke);
            e.Cancel();
            return;
        }

        var chord = KeyChord.FromEvent(ke);
        if (chord == null) return;
        if (KeySender.IsOwn(chord)) return;            // こちらが送ったキーは、そのままYMM4へ通す

        // 文字を入力している時に、Ctrl・Alt無しのキーを横取りしない
        var mods = Keyboard.Modifiers;
        bool typing = focused is TextBoxBase or PasswordBox && !mods.HasFlag(ModifierKeys.Control) && !mods.HasFlag(ModifierKeys.Alt);

        bool wasDeferred = deferred != null && !ke.IsRepeat;
        if (!ke.IsRepeat) deferred = null;
        int active = ActiveWindow;
        var binding = ShortcutStore.Instance.Find(chord, active);
        if (binding == null)
        {
            // キーの置き換え: 押されたキーの代わりに、YMM4へ別のキー操作を送る(送るキーが空なら、無効にするだけ)
            var remap = ShortcutStore.Instance.FindRemap(chord);
            if (remap == null)
            {
                // A+S+D のような同時押しの途中: YMM4へは渡さずに続きを待つ(続きが無ければ、離した時に送り直す)
                if (!typing && ShortcutStore.Instance.HasLonger(chord))
                {
                    e.Cancel();
                    if (!ke.IsRepeat && !wasDeferred && KeyChord.HeldCount <= 1) deferred = chord;
                }
                return;
            }
            if (typing) return;
            e.Cancel();
            if (!string.IsNullOrEmpty(remap.To)) KeySender.Send(remap.To);
            return;
        }
        if (typing) return;

        e.Cancel();                                    // YMM4側のショートカットには渡さない
        if (ke.IsRepeat) return;                       // 押しっぱなしでは、くり返し実行しない

        // 実行するウィンドウ: 操作ショートカットでウィンドウを決めてあれば、そのウィンドウ。それ以外は最後にさわったウィンドウ
        int want = ActionShortcuts.WindowOf(binding);
        ExplorerViewModel? vm;
        lock (Targets)
        {
            vm = want > 0 ? Targets.LastOrDefault(t => t.WindowNumber == want) : null;
            if (vm == null && want == 0) vm = lastActive != null && Targets.Contains(lastActive) ? lastActive : Targets.FirstOrDefault();
            if (vm == null)
            {
                // 決めたウィンドウが開いていない
                var any = lastActive != null && Targets.Contains(lastActive) ? lastActive : Targets.FirstOrDefault();
                any?.Notify($"[{binding.Keys}] {ActionShortcuts.WindowLabel(want)} が開いていないので、実行できませんでした");
                return;
            }
        }
        _ = vm.ExecuteShortcutAsync(binding);
    }
}

/// <summary>設定画面の「ショートカットキー一覧」の1行</summary>
public sealed class ShortcutRow : NotifyBase
{
    readonly ShortcutBinding binding;
    readonly Action changed;
    public static bool Suppress;

    public ShortcutRow(ShortcutBinding b, Action changed)
    {
        binding = b;
        this.changed = changed;
    }

    public ShortcutBinding Binding => binding;
    public string Name => string.IsNullOrWhiteSpace(binding.Name) ? "(名前なし)" : binding.Name;
    public string KindText => ShortcutStore.KindLabel(binding.Kind)
        + (binding.Kind == "file" ? " / " + binding.TargetId : "");

    public string Keys
    {
        get => binding.Keys;
        set
        {
            if (value == binding.Keys) return;
            Suppress = true;
            try { ShortcutStore.Instance.Set(binding.Kind, binding.TargetId, binding.Name, value ?? ""); }
            finally { Suppress = false; }
            Raise(nameof(Keys));
            changed();        // 同じキーを持っていた他の割り当てが外れるので、一覧を作り直す
        }
    }
}

/// <summary>YMM4へ、キー操作(Ctrl+S など)を送る。キーの置き換えに使う</summary>
public static class KeySender
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    const uint KeyUp = 0x2, Extended = 0x1;
    const byte VkShift = 0x10, VkControl = 0x11, VkAlt = 0x12, VkLWin = 0x5B;

    static string ownChord = "";
    static long ownUntil;

    /// <summary>いま届いたキーが、こちらが送ったものか(置き換えの置き換えにならないように)</summary>
    public static bool IsOwn(string chord) =>
        Environment.TickCount64 < ownUntil
        && (string.Equals(KeyChord.Normalize(chord), ownChord, StringComparison.OrdinalIgnoreCase) || KeyChord.IsLongerThan(ownChord, chord));

    static bool IsExtended(Key k) => k is Key.Left or Key.Right or Key.Up or Key.Down or Key.Home or Key.End or Key.PageUp or Key.PageDown
        or Key.Insert or Key.Delete or Key.NumLock or Key.Divide or Key.RightCtrl or Key.RightAlt;

    static bool Down(byte vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

    public static void Send(string chord)
    {
        if (!KeyChord.TryParse(chord, out var mods, out var keys)) return;
        try
        {
            var vks = keys.Select(k => (key: k, vk: (byte)KeyInterop.VirtualKeyFromKey(k))).Where(x => x.vk != 0).ToList();
            if (vks.Count == 0) return;
            ownChord = KeyChord.Normalize(chord);
            ownUntil = Environment.TickCount64 + 400;

            var want = new (ModifierKeys flag, byte vk)[]
            {
                (ModifierKeys.Control, VkControl), (ModifierKeys.Shift, VkShift), (ModifierKeys.Alt, VkAlt), (ModifierKeys.Windows, VkLWin),
            };
            // いま押している修飾キーのうち、送る組み合わせに無いものは、いったん離したことにする
            foreach (var (flag, m) in want)
                if (!mods.HasFlag(flag) && Down(m)) keybd_event(m, 0, KeyUp, UIntPtr.Zero);
            foreach (var (flag, m) in want)
                if (mods.HasFlag(flag) && !Down(m)) keybd_event(m, 0, 0, UIntPtr.Zero);

            // ふつうのキーは、順に押して、逆の順に離す(同時押し)
            foreach (var (key, vk) in vks) keybd_event(vk, 0, IsExtended(key) ? Extended : 0, UIntPtr.Zero);
            for (int i = vks.Count - 1; i >= 0; i--)
                keybd_event(vks[i].vk, 0, (IsExtended(vks[i].key) ? Extended : 0) | KeyUp, UIntPtr.Zero);

            foreach (var (flag, m) in want)
                if (mods.HasFlag(flag)) keybd_event(m, 0, KeyUp, UIntPtr.Zero);
        }
        catch { }
    }
}

/// <summary>ショートカットタブの「キーの置き換え」の1行</summary>
public sealed class RemapRow : NotifyBase
{
    readonly KeyRemap remap;
    public RemapRow(KeyRemap remap) => this.remap = remap;
    public string Id => remap.Id;

    public string Name { get => remap.Name; set { if (remap.Name != (value ?? "")) { remap.Name = value ?? ""; Save(); Raise(nameof(Name)); } } }
    public string From { get => remap.From; set { if (remap.From != (value ?? "")) { remap.From = value ?? ""; Save(); Raise(nameof(From)); Raise(nameof(StateText)); } } }
    public string To { get => remap.To; set { if (remap.To != (value ?? "")) { remap.To = value ?? ""; Save(); Raise(nameof(To)); Raise(nameof(StateText)); } } }
    public bool Enabled { get => remap.Enabled; set { if (remap.Enabled != value) { remap.Enabled = value; Save(); Raise(nameof(Enabled)); Raise(nameof(StateText)); } } }

    public string StateText =>
        string.IsNullOrEmpty(remap.From) ? "「押すキー」を決めてください"
        : !remap.Enabled ? "オフ(置き換えません)"
        : string.IsNullOrEmpty(remap.To) ? $"{remap.From} を押しても、何も起きないようにします"
        : $"{remap.From} を押すと、YMM4には {remap.To} が届きます";

    static void Save() => ShortcutStore.Instance.Save();
}
