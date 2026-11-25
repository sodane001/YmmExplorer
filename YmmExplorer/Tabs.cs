using System.ComponentModel;
using System.Windows;

namespace YmmExplorer;

/// <summary>タブの種類</summary>
public enum TabKind { Explorer, Template, Preset, Shape, VideoEffect, AudioEffect, Text, YmmSettings, Material, Item, Shortcut, AiEdit, EffectShape }

/// <summary>タブ1つ分(番号・名前・表示するか・開始フォルダー)。全ウィンドウで共有されます</summary>
public sealed class TabEntry : NotifyBase
{
    public TabKind Kind { get; }
    public int Slot { get; }
    public string DefaultName { get; }

    string name;
    bool isVisible;
    string startPath;

    /// <summary>名前・開始フォルダーが変わった(false) / 表示状態が変わった(true)</summary>
    internal Action<TabEntry, bool>? Edited;

    /// <summary>ユーザーが付けた名前。空欄なら初期名を使う</summary>
    public string Name
    {
        get => name;
        set
        {
            value ??= "";
            if (!Set(ref name, value)) return;
            Raise(nameof(DisplayName));
            Edited?.Invoke(this, false);
        }
    }

    public string DisplayName => string.IsNullOrWhiteSpace(name) ? DefaultName : name.Trim();

    /// <summary>タブバーに表示するか。非表示にしても、中身は消えません</summary>
    public bool IsVisible
    {
        get => isVisible;
        set
        {
            if (isVisible == value) return;
            // 全部のタブを隠すと操作できなくなるので、最後の1つは隠せない
            if (!value && TabGroups.TotalVisible() <= 1)
            {
                Application.Current?.Dispatcher.BeginInvoke(new Action(() => Raise(nameof(IsVisible))));
                return;
            }
            isVisible = value;
            Raise(nameof(IsVisible));
            Edited?.Invoke(this, true);
        }
    }

    /// <summary>エクスプローラーのタブを開いた時の最初のフォルダー(空欄ならビデオフォルダー)</summary>
    public string StartPath
    {
        get => startPath;
        set
        {
            value ??= "";
            if (!Set(ref startPath, value)) return;
            Edited?.Invoke(this, false);
        }
    }

    internal TabEntry(TabKind kind, int slot, string defaultName, TabConfig cfg)
    {
        Kind = kind;
        Slot = slot;
        DefaultName = defaultName;
        name = cfg.Name ?? "";
        isVisible = cfg.Visible;
        startPath = cfg.StartPath ?? "";
    }

    internal TabConfig ToConfig() => new() { Name = name, Visible = isVisible, StartPath = startPath };

    // まとめて変更する時用(1件ごとに保存・再構築しない)
    internal void ResetNameQuiet()
    {
        name = "";
        Raise(nameof(Name));
        Raise(nameof(DisplayName));
    }

    internal void ShowQuiet()
    {
        if (isVisible) return;
        isVisible = true;
        Raise(nameof(IsVisible));
    }
}

/// <summary>同じ種類のタブの組。数は設定で自由に増減でき、表示するタブも選べます</summary>
public sealed class TabGroup : NotifyBase
{
    /// <summary>タブの数の上限(多すぎると、ツールを開けなくなるほど重くなるため)</summary>
    public const int MaxTabs = 100;

    public TabKind Kind { get; }
    readonly string prefix;
    /// <summary>種類の名前(エクスプローラー・プリセットなど)</summary>
    public string Title => prefix;
    readonly bool plainFirstName;
    readonly ExplorerSettings settings = ExplorerSettings.Instance;
    readonly Func<ExplorerSettings, int> getCount;
    readonly Action<ExplorerSettings, int> setCount;
    readonly Func<ExplorerSettings, List<TabConfig>?> getConfigs;
    readonly Action<ExplorerSettings, List<TabConfig>> setConfigs;
    readonly Func<ExplorerSettings, List<string>?> getOldNames;
    readonly List<TabEntry> entries = new();
    int count;

    /// <summary>すべてのタブ(1〜Count番)。設定画面用</summary>
    public IReadOnlyList<TabEntry> Tabs { get; private set; } = Array.Empty<TabEntry>();

    /// <summary>タブバーに表示するタブ</summary>
    public IReadOnlyList<TabEntry> VisibleTabs { get; private set; } = Array.Empty<TabEntry>();

    /// <summary>タブの数・表示するタブが変わった時</summary>
    public event Action? TabsChanged;

    public TabGroup(TabKind kind, string prefix,
        Func<ExplorerSettings, int> getCount, Action<ExplorerSettings, int> setCount,
        Func<ExplorerSettings, List<TabConfig>?> getConfigs, Action<ExplorerSettings, List<TabConfig>> setConfigs,
        Func<ExplorerSettings, List<string>?> getOldNames, bool plainFirstName = false)
    {
        Kind = kind;
        this.prefix = prefix;
        this.plainFirstName = plainFirstName;
        this.getCount = getCount;
        this.setCount = setCount;
        this.getConfigs = getConfigs;
        this.setConfigs = setConfigs;
        this.getOldNames = getOldNames;

        count = Math.Clamp(getCount(settings), 1, MaxTabs);
        var configs = getConfigs(settings);
        var oldNames = getOldNames(settings);
        // 保存された設定が上限を超えていても(古い版で大きな数にした場合など)、上限までで読み込む
        var n = Math.Min(MaxTabs, Math.Max(count, configs?.Count ?? 0));
        for (int i = 0; i < n; i++)
        {
            TabConfig cfg = configs != null && i < configs.Count
                ? configs[i]
                : new TabConfig { Name = oldNames != null && i < oldNames.Count ? oldNames[i] : "" };
            AddEntry(cfg);
        }
        Rebuild(false);

        // 上限を超えていた設定は、ここで正しい値に書き直す(次回以降、ツールが開けなくなるのを防ぐ)
        if (getCount(settings) > MaxTabs || (configs?.Count ?? 0) > MaxTabs)
        {
            setCount(settings, count);
            Save();
        }
    }

    void AddEntry(TabConfig cfg)
    {
        var slot = entries.Count + 1;
        // 図形のタブは、1つ目を「図形」、2つ目以降を「図形2」「図形3」…にする
        var defaultName = plainFirstName && slot == 1 ? prefix : $"{prefix}{slot}";
        var e = new TabEntry(Kind, slot, defaultName, cfg) { Edited = OnEntryEdited };
        entries.Add(e);
    }

    /// <summary>タブの数。減らしても中身は消えず、増やすと元の中身が戻ります</summary>
    public int Count
    {
        get => count;
        set
        {
            value = Math.Clamp(value, 1, MaxTabs);
            if (value == count) return;
            while (entries.Count < value) AddEntry(new TabConfig());
            // 増えたタブは、増やしたウィンドウにだけ出す(他のウィンドウでは「あるけど表示しない」状態にする)
            if (value > count) WindowTabs.OnTabsAdded(Kind, count + 1, value);
            count = value;
            setCount(settings, value);
            Save();
            Rebuild(true);
            Raise(nameof(Count));
        }
    }

    void OnEntryEdited(TabEntry e, bool visibilityChanged)
    {
        Save();
        if (visibilityChanged) Rebuild(true);
    }

    void Rebuild(bool notify)
    {
        Tabs = entries.Take(count).ToList();
        VisibleTabs = Tabs.Where(t => t.IsVisible).ToList();
        Raise(nameof(Tabs));
        Raise(nameof(VisibleTabs));
        if (notify) TabsChanged?.Invoke();
    }

    void Save()
    {
        setConfigs(settings, entries.Select(e => e.ToConfig()).ToList());
        settings.Save();
    }

    public void ResetNames()
    {
        foreach (var t in entries) t.ResetNameQuiet();
        Save();
    }

    public void ShowAll()
    {
        foreach (var t in entries) t.ShowQuiet();
        Save();
        Rebuild(true);
    }
}

/// <summary>3種類のタブの組(エクスプローラー・テンプレート・プリセット)</summary>
public static class TabGroups
{
    public static TabGroup Explorer { get; } = new(TabKind.Explorer, "エクスプローラー",
        s => s.ExplorerTabCount, (s, v) => s.ExplorerTabCount = v,
        s => s.ExplorerTabConfigs, (s, v) => s.ExplorerTabConfigs = v,
        s => s.ExplorerTabNames);

    public static TabGroup Template { get; } = new(TabKind.Template, "テンプレート",
        s => s.TemplateTabCount, (s, v) => s.TemplateTabCount = v,
        s => s.TemplateTabConfigs, (s, v) => s.TemplateTabConfigs = v,
        s => s.TemplateTabNames);

    public static TabGroup Preset { get; } = new(TabKind.Preset, "プリセット",
        s => s.PresetTabCount, (s, v) => s.PresetTabCount = v,
        s => s.PresetTabConfigs, (s, v) => s.PresetTabConfigs = v,
        s => s.PresetTabNames);

    public static TabGroup Shape { get; } = new(TabKind.Shape, "図形",
        s => s.ShapeTabCount, (s, v) => s.ShapeTabCount = v,
        s => s.ShapeTabConfigs, (s, v) => s.ShapeTabConfigs = v,
        s => s.ShapeTabNames, plainFirstName: true);

    public static TabGroup EffectShape { get; } = new(TabKind.EffectShape, "演出図形",
        s => s.EffectShapeTabCount, (s, v) => s.EffectShapeTabCount = v,
        s => s.EffectShapeTabConfigs, (s, v) => s.EffectShapeTabConfigs = v,
        s => s.EffectShapeTabNames, plainFirstName: true);

    public static TabGroup VideoEffect { get; } = new(TabKind.VideoEffect, "映像エフェクト",
        s => s.VideoEffectTabCount, (s, v) => s.VideoEffectTabCount = v,
        s => s.VideoEffectTabConfigs, (s, v) => s.VideoEffectTabConfigs = v,
        s => s.VideoEffectTabNames, plainFirstName: true);

    public static TabGroup AudioEffect { get; } = new(TabKind.AudioEffect, "音声エフェクト",
        s => s.AudioEffectTabCount, (s, v) => s.AudioEffectTabCount = v,
        s => s.AudioEffectTabConfigs, (s, v) => s.AudioEffectTabConfigs = v,
        s => s.AudioEffectTabNames, plainFirstName: true);

    public static TabGroup Text { get; } = new(TabKind.Text, "テキスト",
        s => s.TextTabCount, (s, v) => s.TextTabCount = v,
        s => s.TextTabConfigs, (s, v) => s.TextTabConfigs = v,
        s => s.TextTabNames, plainFirstName: true);

    public static TabGroup YmmSettings { get; } = new(TabKind.YmmSettings, "YMM4設定",
        s => s.YmmSettingsTabCount, (s, v) => s.YmmSettingsTabCount = v,
        s => s.YmmSettingsTabConfigs, (s, v) => s.YmmSettingsTabConfigs = v,
        s => s.YmmSettingsTabNames);

    public static TabGroup Material { get; } = new(TabKind.Material, "素材データベース",
        s => s.MaterialTabCount, (s, v) => s.MaterialTabCount = v,
        s => s.MaterialTabConfigs, (s, v) => s.MaterialTabConfigs = v,
        s => s.MaterialTabNames);

    public static TabGroup Item { get; } = new(TabKind.Item, "アイテム",
        s => s.ItemTabCount, (s, v) => s.ItemTabCount = v,
        s => s.ItemTabConfigs, (s, v) => s.ItemTabConfigs = v,
        s => s.ItemTabNames);

    public static TabGroup Shortcut { get; } = new(TabKind.Shortcut, "ショートカット",
        s => s.ShortcutTabCount, (s, v) => s.ShortcutTabCount = v,
        s => s.ShortcutTabConfigs, (s, v) => s.ShortcutTabConfigs = v,
        s => s.ShortcutTabNames);

    public static TabGroup AiEdit { get; } = new(TabKind.AiEdit, "AIエディット",
        s => s.AiEditTabCount, (s, v) => s.AiEditTabCount = v,
        s => s.AiEditTabConfigs, (s, v) => s.AiEditTabConfigs = v,
        s => s.AiEditTabNames);

    public static TabGroup Of(TabKind kind) => kind switch
    {
        TabKind.AiEdit => AiEdit,
        TabKind.Shortcut => Shortcut,
        TabKind.Item => Item,
        TabKind.Material => Material,
        TabKind.YmmSettings => YmmSettings,
        TabKind.Text => Text,
        TabKind.Explorer => Explorer,
        TabKind.Template => Template,
        TabKind.Shape => Shape,
        TabKind.EffectShape => EffectShape,
        TabKind.VideoEffect => VideoEffect,
        TabKind.AudioEffect => AudioEffect,
        _ => Preset,
    };

    public static TabGroup[] All => new[] { Explorer, Template, Preset, EffectShape, VideoEffect, AudioEffect, Text, YmmSettings, Material, Item, Shortcut, AiEdit };

    /// <summary>いまタブバーに表示されているタブの合計</summary>
    public static int TotalVisible() => All.Sum(g => g.VisibleTabs.Count);

    /// <summary>タブバーの先頭に来るタブ(エクスプローラー→テンプレート→プリセットの順で最初の表示タブ)</summary>
    public static TabEntry? FirstVisible()
        => All.Select(g => g.VisibleTabs.FirstOrDefault()).FirstOrDefault(t => t != null);
}

/// <summary>
/// ウィンドウ1つ(YmmExplorer・２〜５)の、タブバーに出すタブの選び方。
/// タブの数・名前・中身は全ウィンドウで共通。どのタブを上に出すかだけを、ウィンドウごとに持つ。
/// </summary>
public sealed class WindowTabs
{
    readonly string id;
    readonly HashSet<string> hidden;

    /// <summary>このウィンドウで、出すタブが変わった時</summary>
    public event Action? Changed;

    static string Key(TabKind kind, int slot) => $"{kind}:{slot}";

    // 開いているウィンドウ(番号ごとに、いちばん新しいもの)
    static readonly Dictionary<string, WindowTabs> Live = new();

    // 初めて開くウィンドウ: 以前の版の共通の設定(隠していたタブ)を引き継ぐ
    static HashSet<string> LegacyHidden() =>
        new(TabGroups.All.SelectMany(g => g.Tabs).Where(t => !t.IsVisible).Select(t => Key(t.Kind, t.Slot)));

    public WindowTabs(int windowId)
    {
        id = windowId.ToString();
        var st = ExplorerSettings.Instance;
        st.WindowHiddenTabs ??= new Dictionary<string, List<string>>();
        if (st.WindowHiddenTabs.TryGetValue(id, out var saved)) hidden = new HashSet<string>(saved);
        else
        {
            hidden = LegacyHidden();
            Save();
        }
        lock (Live) Live[id] = this;
    }

    /// <summary>そのウィンドウで、タブを表示する設定か(まだ開いていないウィンドウも、保存された設定で答える)</summary>
    public static bool IsShownIn(int window, TabKind kind, int slot)
    {
        var key = Key(kind, slot);
        lock (Live) if (Live.TryGetValue(window.ToString(), out var w)) return !w.hidden.Contains(key);
        var st = ExplorerSettings.Instance;
        if (st.WindowHiddenTabs != null && st.WindowHiddenTabs.TryGetValue(window.ToString(), out var list)) return !list.Contains(key);
        return !LegacyHidden().Contains(key);
    }

    /// <summary>
    /// タブの数が増えた時。増やしたウィンドウ(最後にさわったウィンドウ)ではそのまま表示し、
    /// それ以外のウィンドウでは、タブはあるけれど表示しない(タブ構成のチェックが外れた)状態にする
    /// </summary>
    public static void OnTabsAdded(TabKind kind, int fromSlot, int toSlot)
    {
        if (toSlot < fromSlot) return;
        int origin = ShortcutHost.ActiveWindow;
        if (origin <= 0) origin = 1;
        var keys = Enumerable.Range(fromSlot, toSlot - fromSlot + 1).Select(s => Key(kind, s)).ToList();
        var st = ExplorerSettings.Instance;
        st.WindowHiddenTabs ??= new Dictionary<string, List<string>>();
        for (int w = 1; w <= ActionShortcuts.WindowCount; w++)
        {
            var wid = w.ToString();
            WindowTabs? live;
            lock (Live) Live.TryGetValue(wid, out live);
            HashSet<string> set = live?.hidden
                ?? (st.WindowHiddenTabs.TryGetValue(wid, out var saved) ? new HashSet<string>(saved) : LegacyHidden());
            foreach (var k in keys)
            {
                if (w == origin) set.Remove(k);
                else set.Add(k);
            }
            st.WindowHiddenTabs[wid] = set.OrderBy(x => x).ToList();
        }
        st.Save();
        // 各ウィンドウのタブバーは、このあと TabGroup の TabsChanged で作り直される
    }

    void Save()
    {
        var st = ExplorerSettings.Instance;
        st.WindowHiddenTabs ??= new Dictionary<string, List<string>>();
        st.WindowHiddenTabs[id] = hidden.OrderBy(x => x).ToList();
        st.Save();
    }

    public bool IsVisible(TabEntry e) => !hidden.Contains(Key(e.Kind, e.Slot));

    public IReadOnlyList<TabEntry> VisibleTabs(TabGroup g) => g.Tabs.Where(IsVisible).ToList();

    public int TotalVisible() => TabGroups.All.Sum(g => g.Tabs.Count(IsVisible));

    public TabEntry? FirstVisible() => TabGroups.All.SelectMany(g => g.Tabs).FirstOrDefault(IsVisible);

    /// <summary>表示・非表示を変える。このウィンドウで最後の1つになるタブは、隠せない(false を返す)</summary>
    public bool SetVisible(TabEntry e, bool visible)
    {
        if (IsVisible(e) == visible) return true;
        if (!visible && TotalVisible() <= 1) return false;
        if (visible) hidden.Remove(Key(e.Kind, e.Slot)); else hidden.Add(Key(e.Kind, e.Slot));
        Save();
        Changed?.Invoke();
        return true;
    }

    public void ShowAll(TabGroup g)
    {
        bool any = false;
        foreach (var t in g.Tabs) any |= hidden.Remove(Key(t.Kind, t.Slot));
        if (!any) return;
        Save();
        Changed?.Invoke();
    }
}

/// <summary>設定画面の「タブ構成」の1行。名前・開始フォルダーは全ウィンドウ共通、表示するかどうかは、このウィンドウだけ</summary>
public sealed class TabToggle : NotifyBase
{
    readonly TabEntry src;
    readonly WindowTabs window;

    public TabToggle(TabEntry src, WindowTabs window)
    {
        this.src = src;
        this.window = window;
        src.PropertyChanged += OnSrcChanged;
    }

    void OnSrcChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TabEntry.Name) or nameof(TabEntry.StartPath) or nameof(TabEntry.DisplayName)) Raise(e.PropertyName);
    }

    public void Detach() => src.PropertyChanged -= OnSrcChanged;

    public TabEntry Entry => src;
    public string DefaultName => src.DefaultName;
    public string Name { get => src.Name; set => src.Name = value; }
    public string StartPath { get => src.StartPath; set => src.StartPath = value; }

    /// <summary>このウィンドウのタブバーに出すか(ほかのウィンドウには影響しない)</summary>
    public bool IsVisible
    {
        get => window.IsVisible(src);
        set
        {
            if (!window.SetVisible(src, value))
                Application.Current?.Dispatcher.BeginInvoke(new Action(() => Raise(nameof(IsVisible))));   // 最後の1つは隠せない
            else Raise(nameof(IsVisible));
        }
    }

    public void RaiseVisible() => Raise(nameof(IsVisible));
}

/// <summary>上部のタブ1つ分の表示用。選択状態はウィンドウごと</summary>
public sealed class TabVm : NotifyBase
{
    readonly ExplorerViewModel owner;
    readonly TabEntry src;

    public TabKind Kind => src.Kind;
    public int Slot => src.Slot;
    public string Name => src.DisplayName;

    /// <summary>種類の切れ目に区切り線を出す</summary>
    public bool IsGroupStart { get; }

    public bool IsSelected
    {
        get => owner.SelectedKind == Kind && owner.SelectedSlot == Slot && !owner.IsSettingsPageOpen;
        set { if (value) owner.SelectTab(Kind, Slot); }
    }

    public TabVm(ExplorerViewModel owner, TabEntry src, bool isGroupStart)
    {
        this.owner = owner;
        this.src = src;
        IsGroupStart = isGroupStart;
        src.PropertyChanged += OnSrcChanged;
    }

    void OnSrcChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TabEntry.DisplayName)) Raise(nameof(Name));
    }

    public void RaiseSelected() => Raise(nameof(IsSelected));

    public void Detach() => src.PropertyChanged -= OnSrcChanged;
}
