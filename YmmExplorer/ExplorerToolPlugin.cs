using YukkuriMovieMaker.Plugin;

namespace YmmExplorer;

/// <summary>ツールメニューの「YmmExplorer」</summary>
public class ExplorerToolPlugin : IToolPlugin
{
    public ExplorerToolPlugin() => PluginReset.ApplyPending();     // 予約された完全リセット(設定を読む前に)
    public string Name => "YmmExplorer";
    public Type ViewModelType => typeof(ExplorerViewModel);
    public Type ViewType => typeof(ExplorerView);
}

/// <summary>ツールメニューの「YmmExplorer ２」(同時に開けます。中身は同じ)</summary>
public class ExplorerToolPlugin2 : IToolPlugin
{
    public ExplorerToolPlugin2() => PluginReset.ApplyPending();     // 予約された完全リセット(設定を読む前に)
    public string Name => "YmmExplorer ２";
    public Type ViewModelType => typeof(ExplorerViewModel2);
    public Type ViewType => typeof(ExplorerView2);
}

/// <summary>ツールメニューの「YmmExplorer ３」</summary>
public class ExplorerToolPlugin3 : IToolPlugin
{
    public ExplorerToolPlugin3() => PluginReset.ApplyPending();     // 予約された完全リセット(設定を読む前に)
    public string Name => "YmmExplorer ３";
    public Type ViewModelType => typeof(ExplorerViewModel3);
    public Type ViewType => typeof(ExplorerView3);
}

/// <summary>ツールメニューの「YmmExplorer ４」</summary>
public class ExplorerToolPlugin4 : IToolPlugin
{
    public ExplorerToolPlugin4() => PluginReset.ApplyPending();     // 予約された完全リセット(設定を読む前に)
    public string Name => "YmmExplorer ４";
    public Type ViewModelType => typeof(ExplorerViewModel4);
    public Type ViewType => typeof(ExplorerView4);
}

/// <summary>ツールメニューの「YmmExplorer ５」</summary>
public class ExplorerToolPlugin5 : IToolPlugin
{
    public ExplorerToolPlugin5() => PluginReset.ApplyPending();     // 予約された完全リセット(設定を読む前に)
    public string Name => "YmmExplorer ５";
    public Type ViewModelType => typeof(ExplorerViewModel5);
    public Type ViewType => typeof(ExplorerView5);
}

// 2〜5つ目用の型。型を分けることで、それぞれ別のウィンドウ・別の状態(表示フォルダーなど)になり、最大5個まで同時に開けます。
// タグ・テンプレート・プリセット・設定は共有なので、どのウィンドウで変更しても他のウィンドウに反映されます。
public sealed class ExplorerViewModel2 : ExplorerViewModel { protected override int WindowId => 2; }
public sealed class ExplorerViewModel3 : ExplorerViewModel { protected override int WindowId => 3; }
public sealed class ExplorerViewModel4 : ExplorerViewModel { protected override int WindowId => 4; }
public sealed class ExplorerViewModel5 : ExplorerViewModel { protected override int WindowId => 5; }

public class ExplorerView2 : ExplorerView { }
public class ExplorerView3 : ExplorerView { }
public class ExplorerView4 : ExplorerView { }
public class ExplorerView5 : ExplorerView { }
