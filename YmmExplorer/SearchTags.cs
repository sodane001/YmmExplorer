using System.Text;

namespace YmmExplorer;

/// <summary>
/// このプラグインが追加する図形・エフェクト(同梱の演出図形・同梱プラグイン)の「内部タグ」。
/// 画面には出さず、検索の時だけ使う(例: 「あめ」「rain」で「雨」がヒットする)。
/// データは SearchTags.Data.cs(tools/gen_tags.py で自動生成)
/// </summary>
public static partial class SearchTags
{
    /// <summary>型の名前(名前空間つき、または型の名前だけ)からタグを返す。無ければ ""</summary>
    public static string For(string? typeName)
    {
        if (string.IsNullOrEmpty(typeName)) return "";
        if (data.TryGetValue(typeName, out var t)) return t;
        var dot = typeName.LastIndexOf('.');
        return dot >= 0 && data.TryGetValue(typeName.Substring(dot + 1), out var t2) ? t2 : "";
    }

    /// <summary>
    /// 検索の言葉(スペース区切りはすべてを含むもの)が、名前・分類・内部タグのどれかに入っているか。
    /// ひらがな/カタカナ・全角/半角・大文字/小文字の違いは無視する
    /// </summary>
    public static bool Matches(string query, string typeName, params string?[] texts)
    {
        typeName = YMM4EffectShapes.Solid3D.Solid3DDefaults.Key2D(typeName);   // 演出3D は2D版と同じタグ
        var terms = query.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length == 0) return true;
        var raw = string.Join(" ", texts.Where(x => !string.IsNullOrEmpty(x))) + " " + For(typeName);
        // 同じ図形・エフェクトは何度も比べるので、比べやすい形は覚えておく
        if (!cache.TryGetValue(raw, out var hay)) { hay = Normalize(raw); if (cache.Count < 5000) cache[raw] = hay; }
        foreach (var t in terms)
            if (!hay.Contains(Normalize(t), StringComparison.Ordinal)) return false;
        return true;
    }

    static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> cache = new();

    /// <summary>比べやすい形にする(全角英数→半角、カタカナ→ひらがな、小文字)</summary>
    public static string Normalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        s = s.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
        var sb = new StringBuilder(s.Length);
        foreach (var c in s) sb.Append(c is >= 'ァ' and <= 'ヶ' ? (char)(c - 0x60) : c);
        return sb.ToString();
    }
}
