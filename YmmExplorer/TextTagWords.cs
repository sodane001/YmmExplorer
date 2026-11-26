using System.Text;

namespace YmmExplorer;

/// <summary>テキストのテンプレートのタグで使う言葉(よく使うタグと、検索の時だけ使う言いかえ)</summary>
public static class TextTagWords
{
    /// <summary>検索欄の下に並べる、よく使うタグ</summary>
    public static readonly string[] Quick =
    {
        "字幕", "タイトル", "読みやすい", "かわいい", "かっこいい", "光る", "立体", "二重ふち", "手書き", "筆文字", "英字", "上品", "金", "白", "黄", "赤", "青",
    };

    static readonly Dictionary<string, string> synonyms = new()
    {
        ["字幕"] = "じまく subtitle caption", ["テロップ"] = "telop", ["タイトル"] = "title 題名", ["見出し"] = "みだし heading headline",
        ["ポップ"] = "pop", ["かわいい"] = "可愛い cute kawaii", ["強調"] = "きょうちょう emphasis", ["注意"] = "ちゅうい caution warning",
        ["ネオン"] = "neon", ["光る"] = "ひかる 光 発光 glow shine", ["和風"] = "わふう 和 japanese", ["ゲーム"] = "game", ["バトル"] = "battle",
        ["ホラー"] = "horror 怖い", ["ミステリー"] = "mystery", ["こわい"] = "怖い scary", ["シンプル"] = "simple", ["おしゃれ"] = "お洒落 stylish",
        ["解説"] = "かいせつ ゆっくり", ["会話"] = "かいわ セリフ talk", ["季節"] = "きせつ season", ["イベント"] = "event", ["ニュース"] = "news",
        ["情報"] = "じょうほう info", ["レトロ"] = "retro 昔 なつかしい", ["昭和"] = "しょうわ", ["映画"] = "えいが movie cinema", ["ドラマ"] = "drama",
        ["伝統色"] = "でんとうしょく 日本の色", ["宝石"] = "ほうせき jewel gem", ["メタル"] = "metal 金属", ["高級"] = "こうきゅう luxury premium",
        ["パステル"] = "pastel", ["ゆめかわ"] = "ゆめかわいい", ["料理"] = "りょうり food cooking", ["グルメ"] = "gourmet 食べ物",
        ["自然"] = "しぜん nature", ["旅行"] = "りょこう travel 旅", ["スポーツ"] = "sports", ["熱血"] = "ねっけつ", ["学習"] = "がくしゅう study 勉強",
        ["教育"] = "きょういく education 学校", ["ビジネス"] = "business 仕事", ["プレゼン"] = "presentation 発表", ["配信"] = "はいしん stream 実況",
        ["ゴシック"] = "gothic sans", ["明朝"] = "みんちょう serif", ["丸ゴシック"] = "まるごしっく round", ["まるい"] = "丸い round",
        ["読みやすい"] = "見やすい readable", ["UD"] = "ユニバーサルデザイン", ["教科書体"] = "きょうかしょたい", ["筆文字"] = "ふで 筆 brush 毛筆 書道",
        ["楷書"] = "かいしょ", ["行書"] = "ぎょうしょ", ["ポップ体"] = "pop", ["角ゴシック"] = "かくごしっく", ["ロゴ体"] = "logo",
        ["ドット"] = "dot pixel ドット絵 8bit", ["手書き"] = "てがき handwriting", ["マジック"] = "magic marker", ["英字"] = "英語 アルファベット english alphabet",
        ["セリフ"] = "serif", ["筆記体"] = "ひっきたい script", ["等幅"] = "monospace", ["タイプライター"] = "typewriter", ["装飾"] = "decorative",
        ["細い"] = "ほそい thin light", ["太い"] = "ふとい bold heavy", ["極太"] = "ごくぶと black heavy",
        ["ふち"] = "縁 縁取り ふちどり outline border", ["二重ふち"] = "二重 double", ["三重ふち"] = "三重 triple", ["多重ふち"] = "多重 multiple",
        ["角ふち"] = "かど sharp", ["細ふち"] = "細い縁", ["影"] = "かげ shadow", ["ぼかし影"] = "soft shadow", ["ぼかし"] = "blur",
        ["立体"] = "りったい 3d 立体的", ["ふちなし"] = "縁なし plain", ["字間広め"] = "文字間 spacing", ["斜体"] = "イタリック italic ななめ 斜め",
        ["太字"] = "bold", ["白"] = "しろ white ホワイト", ["黒"] = "くろ black ブラック", ["灰色"] = "はいいろ グレー gray grey",
        ["赤"] = "あか red レッド", ["ピンク"] = "pink 桃色", ["オレンジ"] = "orange 橙 だいだい", ["茶"] = "ちゃいろ 茶色 brown ブラウン",
        ["黄"] = "きいろ 黄色 yellow イエロー", ["黄緑"] = "きみどり", ["緑"] = "みどり green グリーン", ["水色"] = "みずいろ cyan 空色",
        ["青"] = "あお blue ブルー", ["紺"] = "こん navy ネイビー", ["紫"] = "むらさき purple パープル", ["金"] = "きん ゴールド gold",
        ["銀"] = "ぎん シルバー silver", ["派手"] = "はで flashy", ["上品"] = "じょうひん elegant エレガント", ["白い背景向け"] = "白背景 明るい背景",
        ["キラキラ"] = "sparkle", ["グラデーション"] = "gradient グラデ",
    };

    /// <summary>タグの言いかえ(英語や別の書き方)。画面には出さず、検索の時だけ使う</summary>
    public static string Synonyms(IEnumerable<string> tags)
    {
        var sb = new StringBuilder();
        foreach (var t in tags) if (synonyms.TryGetValue(t, out var s)) sb.Append(' ').Append(s);
        return sb.ToString();
    }
}

/// <summary>検索欄の下に並ぶ「よく使うタグ」のボタン1つ</summary>
public sealed class TextTagChip : NotifyBase
{
    public TextTagChip(string name, int count) { Name = name; Count = count; }
    public string Name { get; }
    public int Count { get; }
    public string ToolTip => $"「{Name}」のタグで探します({Count} 件)。もう一度押すと解除。Ctrl を押しながらだと、今の検索に足します";
    bool isActive;
    /// <summary>いまの検索に入っている</summary>
    public bool IsActive { get => isActive; set => Set(ref isActive, value); }
}
