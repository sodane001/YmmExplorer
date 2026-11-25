using System.Collections.ObjectModel;
using System.Windows.Input;

namespace YmmExplorer;

/// <summary>設定の「図形の表示」: 図形1つ分(☑で一覧に出す)</summary>
public sealed class ShapeVisItem : NotifyBase
{
    readonly ShapeVisibilityVm owner;
    bool isVisible;

    public string Key { get; }
    public string Name { get; }
    public string Source { get; }
    public string Category { get; }
    public int Order { get; }

    public ShapeVisItem(ShapeVisibilityVm owner, string key, string name, string source, string category, int order, bool visible)
    {
        this.owner = owner;
        Key = key; Name = name; Source = source; Category = category; Order = order;
        isVisible = visible;
    }

    public bool IsVisible
    {
        get => isVisible;
        set { if (Set(ref isVisible, value)) owner.OnItemChanged(this); }
    }

    /// <summary>まとめて切り替える時用(保存は呼んだ側でまとめて行う)</summary>
    internal void SetQuietly(bool v) => Set(ref isVisible, v, nameof(IsVisible));
}

/// <summary>設定の「図形の表示」: 分類1つ分</summary>
public sealed class ShapeVisGroup : NotifyBase
{
    readonly ShapeVisibilityVm owner;
    bool isExpanded;

    public string Name { get; }
    public ObservableCollection<ShapeVisItem> Items { get; } = new();
    public ObservableCollection<ShapeVisItem> Shown { get; } = new();
    public ICommand AllOnCommand { get; }
    public ICommand AllOffCommand { get; }

    public ShapeVisGroup(ShapeVisibilityVm owner, string name)
    {
        this.owner = owner;
        Name = name;
        AllOnCommand = new RelayCommand(_ => owner.SetMany(Shown, true));
        AllOffCommand = new RelayCommand(_ => owner.SetMany(Shown, false));
    }

    public bool IsExpanded { get => isExpanded; set => Set(ref isExpanded, value); }
    public string CountText => $"{Items.Count(i => i.IsVisible)} / {Items.Count} 種類を表示";
    public bool HasShown => Shown.Count > 0;
    internal void Refresh() { Raise(nameof(CountText)); Raise(nameof(HasShown)); }
}

/// <summary>
/// 設定の「図形の表示」タブ。YMM4に入っている図形(標準・同梱の演出図形・他のプラグイン)を分類ごとに並べ、
/// 一覧(アイテムタブの図形の選択画面・図形タブ)に出すかどうかを ☑ で選ぶ。初期は、すべて表示
/// </summary>
public sealed class ShapeVisibilityVm : NotifyBase
{
    readonly ExplorerSettings settings = ExplorerSettings.Instance;
    string searchText = "", message = "";
    bool isLoading, loaded;

    /// <summary>表示する図形の設定が変わった(図形タブの一覧を作り直す用)</summary>
    public static event Action? Changed;

    public ObservableCollection<ShapeVisGroup> Groups { get; } = new();
    public ICommand AllOnCommand { get; }
    public ICommand AllOffCommand { get; }
    public ICommand ReloadCommand { get; }
    public ICommand ExpandAllCommand { get; }
    public ICommand CollapseAllCommand { get; }

    public ShapeVisibilityVm()
    {
        AllOnCommand = new RelayCommand(_ => SetMany(Groups.SelectMany(g => g.Shown).ToList(), true));
        AllOffCommand = new RelayCommand(_ => SetMany(Groups.SelectMany(g => g.Shown).ToList(), false));
        ReloadCommand = new RelayCommand(async _ => await LoadAsync(true));
        ExpandAllCommand = new RelayCommand(_ => { foreach (var g in Groups) g.IsExpanded = true; });
        CollapseAllCommand = new RelayCommand(_ => { foreach (var g in Groups) g.IsExpanded = false; });
    }

    public string SearchText { get => searchText; set { if (Set(ref searchText, value ?? "")) Filter(); } }
    public string Message { get => message; private set => Set(ref message, value); }
    public bool IsLoading { get => isLoading; private set => Set(ref isLoading, value); }
    public string SummaryText
    {
        get
        {
            int all = Groups.Sum(g => g.Items.Count), on = Groups.Sum(g => g.Items.Count(i => i.IsVisible));
            return all == 0 ? "" : $"{all} 種類のうち {on} 種類を表示中";
        }
    }

    /// <summary>設定のタブを開いた時に呼ぶ(初回だけ読み込む)</summary>
    public async Task EnsureLoadedAsync()
    {
        if (!loaded) await LoadAsync(false);
    }

    async Task LoadAsync(bool rescan)
    {
        if (IsLoading) return;
        IsLoading = true;
        Message = "YMM4の図形を探しています…";
        try
        {
            IReadOnlyList<ShapeInfo> found = ShapeDiscovery.Found;
            if (rescan || !ShapeDiscovery.Done || found.Count == 0)
                found = await Task.Run(() => ShapeDiscovery.Scan());

            var items = new List<ShapeVisItem>();
            for (int i = 0; i < found.Count; i++)
            {
                var f = found[i];
                if (YMM4EffectShapes.Solid3D.Solid3DDefaults.Is3DKey(f.Key)) continue;   // 演出3D は2D版といっしょに表示・非表示になる
                string cat; int order;
                if (ShapeCategories.TryGet(f.Key, out var c, out var o)) { cat = c; order = 1000 + o; }
                else if (f.IsBuiltIn) { cat = ShapeCategories.Standard; order = i; }
                else { cat = ShapeCategories.OtherPlugins; order = 100000 + i; }
                var name = f.Name.StartsWith("演出：", StringComparison.Ordinal) ? f.Name[3..] : f.Name;
                items.Add(new ShapeVisItem(this, f.Key, name, f.IsBuiltIn ? "標準" : f.Source, cat, order, settings.IsShapeShown(f.Key)));
            }

            Groups.Clear();
            var cats = ShapeCategories.Order.Where(c => items.Any(x => x.Category == c)).ToList();
            foreach (var c in items.Select(x => x.Category).Distinct()) if (!cats.Contains(c)) cats.Add(c);
            foreach (var c in cats)
            {
                var g = new ShapeVisGroup(this, c);
                foreach (var it in items.Where(x => x.Category == c).OrderBy(x => x.Order)) g.Items.Add(it);
                Groups.Add(g);
            }
            loaded = true;
            Filter();
            Message = items.Count == 0 ? "YMM4の図形を見つけられませんでした" : "";
        }
        catch (Exception ex) { Message = "⚠ 図形の一覧を読み込めませんでした: " + ex.GetBaseException().Message; }
        finally { IsLoading = false; }
    }

    void Filter()
    {
        var s = searchText.Trim();
        foreach (var g in Groups)
        {
            g.Shown.Clear();
            bool catHit = s.Length > 0 && g.Name.Contains(s, StringComparison.CurrentCultureIgnoreCase);
            foreach (var it in g.Items)
                if (s.Length == 0 || catHit || SearchTags.Matches(s, it.Key, it.Name, it.Source))
                    g.Shown.Add(it);
            // 検索している時は、見つかった分類を開いておく
            if (s.Length > 0) g.IsExpanded = g.Shown.Count > 0;
            g.Refresh();
        }
        Raise(nameof(SummaryText));
    }

    internal void OnItemChanged(ShapeVisItem item)
    {
        Save();
        foreach (var g in Groups) if (g.Name == item.Category) g.Refresh();
        Raise(nameof(SummaryText));
    }

    internal void SetMany(IEnumerable<ShapeVisItem> items, bool visible)
    {
        foreach (var it in items) it.SetQuietly(visible);
        Save();
        foreach (var g in Groups) g.Refresh();
        Raise(nameof(SummaryText));
    }

    void Save()
    {
        // 一覧に無い(いまは読み込まれていない)図形の設定は、そのまま残す
        var known = Groups.SelectMany(g => g.Items).ToList();
        var keep = (settings.HiddenShapeKeys ?? new List<string>()).Where(k => known.All(i => i.Key != k));
        var hidden = keep.Concat(known.Where(i => !i.IsVisible).Select(i => i.Key)).Distinct().ToList();
        settings.HiddenShapeKeys = hidden.Count == 0 ? null : hidden;
        settings.Save();
        Changed?.Invoke();
    }
}
