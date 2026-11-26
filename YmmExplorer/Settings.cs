using System.IO;
using System.Text.Json;

namespace YmmExplorer;

/// <summary>プラグインの設定。%AppData%\YmmExplorer\settings.json に保存します</summary>
public sealed class ExplorerSettings
{
    static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "settings.json");

    public static ExplorerSettings Instance { get; } = Load();

    /// <summary>詳細パネルに表示する項目のID。nullなら初期設定</summary>
    public List<string>? DetailFields { get; set; }

    /// <summary>詳細パネルにサムネイルを表示する</summary>
    public bool DetailThumbnail { get; set; } = true;

    /// <summary>並び替えの基準(0=名前 1=更新日時 2=作成日時 3=種類 4=サイズ)</summary>
    public int SortIndex { get; set; }

    public bool SortDescending { get; set; }

    public bool FoldersFirst { get; set; } = true;

    /// <summary>テンプレートのタブの数(1〜10)</summary>
    public int TemplateTabCount { get; set; } = 10;

    /// <summary>テンプレート1〜10の名前。空欄なら「テンプレート1」のような初期名</summary>
    public List<string>? TemplateTabNames { get; set; }

    /// <summary>エクスプローラーのタブの数(1〜10)。タブごとに別のフォルダーを開ける</summary>
    public int ExplorerTabCount { get; set; } = 1;

    public List<string>? ExplorerTabNames { get; set; }

    /// <summary>プリセットのタブの数(1〜10)</summary>
    public int PresetTabCount { get; set; } = 5;

    public List<string>? PresetTabNames { get; set; }

    // タブごとの詳細設定(名前・表示/非表示・開始フォルダー)。旧バージョンの *TabNames は読み込み時に引き継ぐ
    public List<TabConfig>? ExplorerTabConfigs { get; set; }
    public List<TabConfig>? TemplateTabConfigs { get; set; }
    public List<TabConfig>? PresetTabConfigs { get; set; }

    /// <summary>クラウドのみ(未ダウンロード)のファイルも、サムネイルや詳細情報を取得する(ダウンロードが発生する)</summary>
    public bool CloudThumbnails { get; set; }

    /// <summary>プリセットを作った時、含まれる素材から自動でプレビュー動画をエンコードして保存する</summary>
    public bool PresetAutoPreview { get; set; } = true;

    /// <summary>図形のタブの数(1〜)。タブごとに、表示する図形・順番・サムネイルを別々に持てる</summary>
    public int ShapeTabCount { get; set; } = 1;

    public List<string>? ShapeTabNames { get; set; }
    public List<TabConfig>? ShapeTabConfigs { get; set; }

    /// <summary>演出図形のタブ(図形の一覧と自動で同じになる読み取り専用のタブ)</summary>
    public int EffectShapeTabCount { get; set; } = 1;
    public List<string>? EffectShapeTabNames { get; set; }
    public List<TabConfig>? EffectShapeTabConfigs { get; set; }

    /// <summary>非表示にした図形も一覧に出す(表示に戻す操作用)</summary>
    public bool ShapeShowHidden { get; set; }

    /// <summary>図形の一覧に出さない図形(図形の型名)。空なら、すべて表示</summary>
    public List<string>? HiddenShapeKeys { get; set; }

    /// <summary>図形の一覧に出すか(設定の「図形の表示」で選ぶ。初期はすべて表示)</summary>
    public bool IsShapeShown(string? key) => key == null || HiddenShapeKeys == null || !HiddenShapeKeys.Contains(YMM4EffectShapes.Solid3D.Solid3DDefaults.Key2D(key));   // 演出3D は2D版の設定に従う

    /// <summary>映像エフェクトのタブの数(1〜)</summary>
    public int VideoEffectTabCount { get; set; } = 1;
    public List<string>? VideoEffectTabNames { get; set; }
    public List<TabConfig>? VideoEffectTabConfigs { get; set; }

    /// <summary>音声エフェクトのタブの数(1〜)</summary>
    public int AudioEffectTabCount { get; set; } = 1;
    public List<string>? AudioEffectTabNames { get; set; }
    public List<TabConfig>? AudioEffectTabConfigs { get; set; }

    /// <summary>テキストのタブの数(1〜)</summary>
    public int TextTabCount { get; set; } = 1;

    /// <summary>素材データベースのタブの数(1〜)</summary>
    public int MaterialTabCount { get; set; } = 1;
    public List<string>? MaterialTabNames { get; set; }
    public List<TabConfig>? MaterialTabConfigs { get; set; }

    /// <summary>素材データベースのライブラリーの保存先(空なら、標準の場所)</summary>
    public string? MaterialLibraryPath { get; set; }

    /// <summary>素材を取り込む時、元のファイルを移動する(オフなら、コピー)</summary>
    public bool MaterialImportMove { get; set; }

    /// <summary>フォルダーごと取り込む時、フォルダー名をタグにする</summary>
    public bool MaterialTagsFromFolders { get; set; } = true;

    /// <summary>
    /// ウィンドウ(YmmExplorer・２〜５)ごとに、タブバーから隠したタブ。キーはウィンドウの番号("1"〜"5")、値は「種類:番号」の一覧。
    /// ウィンドウの番号が無い時は、以前の版の共通の設定(各タブの Visible)から作る
    /// </summary>
    public Dictionary<string, List<string>>? WindowHiddenTabs { get; set; }

    /// <summary>ウィンドウごとに、最後に開いていたタブ(「種類:番号」)。YMM4を起動し直した時に、そのタブから始める</summary>
    public Dictionary<string, string>? WindowLastTab { get; set; }

    /// <summary>AI音声生成(Audio Studio MCP): 実行ファイル・ポート・出力フォルダー・Web画面も出すか・自動で取り込むか・付けるタグ</summary>
    public string? AudioStudioExe { get; set; }
    public int AudioStudioPort { get; set; } = 8765;
    public string? AudioStudioOutput { get; set; }
    public bool AudioStudioWebUi { get; set; }
    /// <summary>できた音を、選ぶ画面を出さずにすべて取り込む(以前の版の初期値は true だったため、名前を変えて初期値を false に)</summary>
    public bool AudioStudioImportAll { get; set; }
    public bool AudioStudioAutoImport { get; set; } = true;
    public string? AudioStudioTags { get; set; }

    /// <summary>画面全体の表示倍率(Ctrl + ホイールで変える。1 = 100%)</summary>
    public double UiScale { get; set; } = 1.0;

    /// <summary>アイテムタブのレイアウト: カード(グループ)の並び順・非表示・よく使う項目・1列の最小の幅</summary>
    public List<string>? ItemGroupOrder { get; set; }
    public List<string>? ItemHiddenGroups { get; set; }
    public List<string>? ItemQuickProps { get; set; }
    /// <summary>「よく使う項目」に、自分で足した項目・外した項目</summary>
    public List<string>? ItemQuickAdded { get; set; }
    public List<string>? ItemQuickRemoved { get; set; }
    /// <summary>最近使ったフォント(フォントを選ぶ画面の上に出す)</summary>
    public List<string>? RecentFonts { get; set; }
    public double ItemColumnWidth { get; set; } = 430;

    /// <summary>ショートカットのタブの数(1〜)</summary>
    /// <summary>エクスプローラーのタブごとの表示サイズ(0=詳細 1=一覧 2〜5=アイコンの大きさ)</summary>
    public Dictionary<string, int>? ExplorerViewModes { get; set; }
    /// <summary>エクスプローラーのタブごとの絞り込み</summary>
    public Dictionary<string, ExplorerFilterState>? ExplorerFilters { get; set; }
    public int AiEditTabCount { get; set; } = 1;
    public List<string>? AiEditTabNames { get; set; }
    public List<TabConfig>? AiEditTabConfigs { get; set; }
    // AIエディット
    public string? AiProvider { get; set; }
    /// <summary>AIごとのAPIキー(Windows の DPAPI で暗号化したもの)</summary>
    public Dictionary<string, string>? AiKeys { get; set; }
    public Dictionary<string, string>? AiModels { get; set; }
    public string? AiBaseUrl { get; set; }
    public bool AiSendTimeline { get; set; } = true;
    public int AiPreviewMode { get; set; } = 1;
    public int AiFramesPerScene { get; set; } = 1;
    public string? AiExtraInstruction { get; set; }
    /// <summary>AI に操作(エフェクトの追加・値の変更など)を提案させる</summary>
    public bool AiProposals { get; set; }
    /// <summary>AI がインターネットを検索・閲覧する(YMM4 マネージャーの AI質問と共通)</summary>
    public bool AiWebAccess { get; set; }
    /// <summary>「〜して」のような指示は、提案がオフでも、その1回は操作を出してもらう</summary>
    public bool AiDetectCommands { get; set; } = true;
    /// <summary>YMM4専用ストレージのすべての中身を AI に見せ、読ませる</summary>
    public bool AiShareStorage { get; set; } = true;
    /// <summary>AI が会話をまたいで覚える(専用ストレージの「AI記憶」)</summary>
    public bool AiUseMemory { get; set; } = true;
    /// <summary>指示された操作は、OK を押さずにすぐ実行する</summary>
    public bool AiAutoRunCommands { get; set; }
    /// <summary>タイムラインの詳しさ: 0 = ふつう / 1 = すべての項目とエフェクトの中身</summary>
    public int AiDetail { get; set; }
    /// <summary>プレビュー画像の大きさ: 0 = 小(768) / 1 = 中(1024) / 2 = 大(1536)</summary>
    public int AiPreviewSize { get; set; } = 1;
    /// <summary>プレビューを撮る場所: 0 = 均等に / 1 = セリフ・テキストごと</summary>
    public int AiPreviewPoints { get; set; }
    /// <summary>とぎれなく撮る時の、1秒あたりのコマ数(0 = 全フレーム)</summary>
    public int AiContinuousFps { get; set; } = 2;
    /// <summary>添付した動画から取り出すコマの数</summary>
    public int AiMediaFrames { get; set; } = 8;
    /// <summary>ChatGPT の時、添付した動画・音声を文字起こしする</summary>
    public bool AiTranscribe { get; set; } = true;
    /// <summary>提案の時に、素材データベース・テンプレート・プリセット・許可したフォルダーを AI に見せる</summary>
    public bool AiShareLibrary { get; set; } = true;
    /// <summary>AI に見せてよい素材フォルダー</summary>
    public List<string>? AiAllowedFolders { get; set; }
    public int ShortcutTabCount { get; set; } = 1;
    public List<string>? ShortcutTabNames { get; set; }
    public List<TabConfig>? ShortcutTabConfigs { get; set; }

    /// <summary>アイテムのタブの数(1〜)</summary>
    public int ItemTabCount { get; set; } = 1;
    public List<string>? ItemTabNames { get; set; }
    public List<TabConfig>? ItemTabConfigs { get; set; }

    /// <summary>YMM4設定のタブの数(1〜)</summary>
    public int YmmSettingsTabCount { get; set; } = 1;
    public List<string>? YmmSettingsTabNames { get; set; }
    public List<TabConfig>? YmmSettingsTabConfigs { get; set; }
    public List<string>? TextTabNames { get; set; }
    public List<TabConfig>? TextTabConfigs { get; set; }

    /// <summary>テキストをダブルクリックした時、クリップボードに戻したあとYMM4へ送るキー操作(テキストアイテムの貼り付け)</summary>
    public string TextApplyKeys { get; set; } = "Ctrl+V";

    /// <summary>テキストの書式を保存する時の「対象パラメータ」で、前回オンにした項目のID(未設定なら、標準の項目)</summary>
    public List<string>? TextStyleGroups { get; set; }
    /// <summary>テキストの書式を保存する時に、書式からサムネイルを自動で作る</summary>
    public bool TextThumbAuto { get; set; } = true;
    public string? TextThumbBg { get; set; }
    public string? TextThumbSample { get; set; }

    /// <summary>YMM4の内部機能(選択中のアイテムの取得・書式やエフェクトの直接適用・テンプレートの配置)を使う。オフならクリップボード方式のみ</summary>
    public bool UseYmmApi { get; set; } = true;

    /// <summary>テキストの書式として「保存しない・適用しない」項目(カンマ区切り)。文字の内容・時間・位置など</summary>
    public string TextExcludeKeys { get; set; } = "Text,Frame,Layer,Length,Group,Remark,ContentOffset,IsLocked,IsHidden,X,Y,Z";

    /// <summary>(旧設定。使用しません)</summary>
    public bool TextReplaceOriginal { get; set; }

    /// <summary>書式・エフェクトを選択中のアイテムに適用する時、元のアイテムを削除して置き換える(オフなら、適用したコピーを追加する)</summary>
    public bool ReplaceOriginalItem { get; set; } = true;

    /// <summary>書き換えられなかった独自形式のデータを、クリップボードから外して貼り付ける(YMM4が元のデータを使ってしまうのを防ぐ)</summary>
    public bool DropUnmodifiedFormats { get; set; } = true;

    /// <summary>(旧設定。使用しません)</summary>
    public bool EffectAppendToExisting { get; set; } = true;

    /// <summary>映像エフェクトのまとまりを、選択中のアイテムのエフェクトに追加する(オフなら、既存のエフェクトをすべて削除してから追加する)</summary>
    public bool VideoEffectAppend { get; set; } = true;
    /// <summary>エフェクトタブで、映像エフェクトのサムネイルに使うサンプル画像(空欄なら同梱の絵)</summary>
    public string FxSampleImage { get; set; } = "";

    /// <summary>テキストタブのお気に入り(テンプレートの ID)</summary>
    public List<string>? TextFavorites { get; set; }
    /// <summary>テキストタブで、選択中のテキストに適用する時に文字の大きさも変える</summary>
    public bool TextTplWithSize { get; set; }
    /// <summary>テキストタブでテンプレートを使う時: 0=新しいテキストを作る 1=選択中のテキストに適用する</summary>
    public int TextTplMode { get; set; }

    /// <summary>音声エフェクトのまとまりを、選択中のアイテムのエフェクトに追加する(オフなら、既存のエフェクトをすべて削除してから追加する)</summary>
    public bool AudioEffectAppend { get; set; } = true;

    /// <summary>「選択中のアイテムから読み取る」時に、YMM4へ送るコピーのキー操作</summary>
    public string ItemCopyKeys { get; set; } = "Ctrl+C";

    /// <summary>タイムラインのアイテムをダブルクリックした時、YMM4標準のアイテム編集ではなく、YmmExplorer のアイテムタブを開く</summary>
    public bool OpenItemTabOnDoubleClick { get; set; } = true;

    /// <summary>エフェクトをダブルクリックした時、クリップボードに戻したあとYMM4へ送るキー操作(例: Ctrl+V)</summary>
    public string VideoEffectApplyKeys { get; set; } = "Ctrl+V";
    public string AudioEffectApplyKeys { get; set; } = "Ctrl+V";

    /// <summary>プリセットをダブルクリックした時、YMM4にCtrl+Vを送ってタイムラインへ貼り付ける</summary>
    public bool PresetAutoPaste { get; set; } = true;

    static ExplorerSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<ExplorerSettings>(File.ReadAllText(FilePath)) ?? new();
        }
        catch { /* 壊れていたら初期設定 */ }
        return new();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}

/// <summary>タブ1つ分の設定(名前・表示するか・エクスプローラーの開始フォルダー)</summary>
public sealed class TabConfig
{
    public string Name { get; set; } = "";
    public bool Visible { get; set; } = true;
    public string StartPath { get; set; } = "";
}

/// <summary>詳細パネルの1行(項目名と値)</summary>
public sealed record DetailRow(string Label, string Value);

/// <summary>設定画面のチェックボックス1つ分</summary>
public sealed class DetailFieldOption : NotifyBase
{
    public string Id { get; }
    public string Label { get; }
    public bool Default { get; }

    bool isChecked;
    public bool IsChecked
    {
        get => isChecked;
        set { if (Set(ref isChecked, value)) Changed?.Invoke(); }
    }

    public event Action? Changed;

    public DetailFieldOption(string id, string label, bool isDefault, bool isChecked)
    {
        Id = id;
        Label = label;
        Default = isDefault;
        this.isChecked = isChecked;
    }
}

/// <summary>エクスプローラーの絞り込み(タブごとに保存)</summary>
public sealed class ExplorerFilterState
{
    public string? Search { get; set; }
    public string? Ext { get; set; }
    public int Date { get; set; }
    public bool Sub { get; set; }
    public string? Tag { get; set; }
}
