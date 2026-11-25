using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace YmmExplorer;

/// <summary>このプラグイン自体の操作(タブの移動など)に付けるショートカットの1項目</summary>
public sealed class ActionShortcutRow : NotifyBase
{
    readonly Action changed;

    public ActionShortcutRow(string id, string name, string? note, Action changed, string? storedName = null)
    {
        Id = id;
        Name = name;
        Note = note ?? "";
        StoredName = storedName ?? name;
        this.changed = changed;
    }

    /// <summary>割り当て一覧などに出す名前(ウィンドウ名つき)</summary>
    public string StoredName { get; }

    public bool HasKeys => !string.IsNullOrEmpty(Keys);

    /// <summary>ShortcutBinding.TargetId(例: tab:Explorer:1 / tab-next / settings)</summary>
    public string Id { get; }
    public string Name { get; }
    public string Note { get; }
    public bool HasNote => Note.Length > 0;

    /// <summary>他の割り当てでキーが外れた時などに、表示を更新する</summary>
    public void Refresh()
    {
        Raise(nameof(Keys));
        Raise(nameof(HasKeys));
    }

    public string Keys
    {
        get => ShortcutStore.Instance.KeysFor(ActionShortcuts.Kind, Id);
        set
        {
            if (value == Keys) return;
            ShortcutRow.Suppress = true;
            try { ShortcutStore.Instance.Set(ActionShortcuts.Kind, Id, StoredName, value ?? ""); }
            finally { ShortcutRow.Suppress = false; }
            Raise(nameof(Keys));
            Raise(nameof(HasKeys));
            changed();       // 同じキーを持っていた他の割り当てが外れるので、一覧を作り直す
        }
    }
}

/// <summary>操作ショートカットを、どのウィンドウで実行するかの選択肢</summary>
public sealed class ActionScopeChoice : NotifyBase
{
    readonly Func<int> current;
    readonly Action<int> select;

    public ActionScopeChoice(int window, Func<int> current, Action<int> select)
    {
        Window = window;
        this.current = current;
        this.select = select;
    }

    public int Window { get; }
    public string Label => ActionShortcuts.WindowLabel(Window);

    public bool IsSelected
    {
        get => current() == Window;
        set { if (value) select(Window); }
    }

    public void Refresh() => Raise(nameof(IsSelected));
}

/// <summary>操作ショートカットの見出しごとのまとまり</summary>
public sealed class ActionShortcutGroup
{
    public ActionShortcutGroup(string title, IEnumerable<ActionShortcutRow> rows)
    {
        Title = title;
        Rows = new ObservableCollection<ActionShortcutRow>(rows);
    }

    public string Title { get; }
    public ObservableCollection<ActionShortcutRow> Rows { get; }
}

/// <summary>割り当てられる操作の一覧</summary>
public static class ActionShortcuts
{
    public const string Kind = "action";

    public const int WindowCount = 5;

    public static string TabId(TabKind kind, int slot) => $"tab:{kind}:{slot}";

    /// <summary>ウィンドウの名前(0 = 最後にクリックしたウィンドウ)</summary>
    public static string WindowLabel(int window) => window switch
    {
        <= 0 => "最後にクリックしたウィンドウ",
        1 => "YmmExplorer",
        _ => "YmmExplorer " + "０１２３４５６７８９"[Math.Min(window, 9)],
    };

    /// <summary>実行するウィンドウを決めたID: 「w2|tab-next」= YmmExplorer ２ で「次のタブへ」</summary>
    public static string Scoped(int window, string id) => window > 0 ? $"w{window}|{id}" : id;

    /// <summary>ID を、ウィンドウの番号と操作に分ける</summary>
    public static (int window, string action) Split(string? id)
    {
        id ??= "";
        int bar = id.IndexOf('|');
        if (bar > 1 && id[0] == 'w' && int.TryParse(id.Substring(1, bar - 1), out var w)) return (w, id.Substring(bar + 1));
        return (0, id);
    }

    public static int WindowOf(string kind, string id) => kind == Kind ? Split(id).window : 0;
    public static int WindowOf(ShortcutBinding b) => WindowOf(b.Kind, b.TargetId);

    public static List<ActionShortcutGroup> Build(Action changed) => Build(changed, 0);

    public static List<ActionShortcutGroup> Build(Action changed, int window)
    {
        string suffix = window > 0 ? $"({WindowLabel(window)})" : "";
        ActionShortcutRow R(string id, string name, string? note = null) => new(Scoped(window, id), name, note, changed, name + suffix);
        var list = new List<ActionShortcutGroup>
        {
            new("タブの切り替え", new[]
            {
                R("tab-next", "次のタブへ", "右どなりのタブへ(いちばん右なら左端へ)"),
                R("tab-prev", "前のタブへ", "左どなりのタブへ(いちばん左なら右端へ)"),
                R("tab-last", "直前に開いていたタブへ戻る", "2つのタブを行き来する時に便利です"),
            }.Concat(Enumerable.Range(1, 9).Select(n =>
                R($"tab-index:{n}", $"左から {n} 番目のタブへ", "このウィンドウで表示しているタブの並び順で数えます")))),
        };

        // タブを名前で指定(エクスプローラー2 など)。並び順が変わっても、同じタブへ移動する
        foreach (var g in TabGroups.All)
        {
            var rows = g.Tabs.Select(t => R(TabId(t.Kind, t.Slot), $"「{t.DisplayName}」タブを開く",
                window > 0 && !WindowTabs.IsShownIn(window, t.Kind, t.Slot) ? $"{WindowLabel(window)} では非表示のタブです(タブ構成でチェックを入れると使えます)" : null)).ToList();
            if (rows.Count > 0) list.Add(new ActionShortcutGroup($"タブを開く: {g.Title}", rows));
        }

        list.Add(new("エクスプローラー", new[]
        {
            R("back", "戻る"),
            R("forward", "進む"),
            R("up", "1つ上のフォルダーへ"),
            R("refresh", "最新の情報に更新"),
            R("search", "検索欄にカーソルを移す", "開いているタブの検索欄へ"),
        }));
        list.Add(new("画面", new[]
        {
            R("settings", "設定画面を開く / 閉じる"),
            R("zoom-in", "画面を拡大", "Ctrl+マウスホイールと同じです"),
            R("zoom-out", "画面を縮小"),
            R("zoom-reset", "画面の大きさを元に戻す"),
        }));
        list.Add(new("機能を開く", new[]
        {
            R("text-split", "選択中のテキストを分割画面へ", "YMM4で選んでいるテキスト・ボイスアイテムを、テキスト分割の画面に読み込みます(開いている時は読み込み直し)"),
            R("ai-audio", "AI音声生成を開く", "素材データベースタブへ移動して開きます"),
            R("item-tab", "アイテムタブ(選択中のアイテムの編集)へ"),
        }));
        return list;
    }

    /// <summary>一覧(ショートカットキー一覧など)に出す名前</summary>
    public static string NameOf(string id)
    {
        var action = Split(id).action;
        foreach (var g in Build(() => { }))
            foreach (var r in g.Rows)
                if (r.Id == action) return r.Name;
        return action;
    }
}
