using System.Text.Json.Nodes;

namespace YmmExplorer;

/// <summary>
/// テキストアイテムの「書式(フォント・大きさ・色・縁取りなど)とエフェクト」だけを取り出して保存し、
/// 選択中のテキストアイテムに適用する。文字の内容・時間・位置などは、設定の「適用しない項目」で除きます。
/// YMM4でアイテムをコピーしたデータ(JSON)を使う方式で、データの書式を確認できていないため、JSONとして読める場合だけ動作します。
/// </summary>
internal static class TextStyle
{
    public static HashSet<string> ParseKeys(string? keys)
        => (keys ?? "").Split(new[] { ',', '、', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       .ToHashSet(StringComparer.OrdinalIgnoreCase);

    internal static string? TypeOf(JsonObject o) => o["$type"] is JsonValue v && v.TryGetValue<string>(out var t) ? t : null;

    internal static string ShortType(string? type)
    {
        if (string.IsNullOrEmpty(type)) return "不明";
        var n = type.Split(',')[0];
        return n[(n.LastIndexOf('.') + 1)..];
    }

    /// <summary>アイテムの型か(例: ...Items.TextItem)。エフェクトなどは除く</summary>
    static bool IsItemType(string? type)
    {
        if (string.IsNullOrEmpty(type)) return false;
        var n = type.Split(',')[0];
        return n.EndsWith("Item", StringComparison.Ordinal) && !n.Contains("Effect", StringComparison.OrdinalIgnoreCase);
    }

    static bool IsTextType(string? type) => IsItemType(type) && ShortType(type).Contains("Text", StringComparison.OrdinalIgnoreCase);

    /// <summary>JSONの中から、アイテムのデータを探す(見つけたアイテムの中までは探さない)</summary>
    internal static List<JsonObject> FindItems(JsonNode? node)
    {
        var list = new List<JsonObject>();
        void Walk(JsonNode? n)
        {
            if (n is JsonObject o)
            {
                if (IsItemType(TypeOf(o))) { list.Add(o); return; }
                foreach (var kv in o) Walk(kv.Value);
            }
            else if (n is JsonArray a)
            {
                foreach (var e in a) Walk(e);
            }
        }
        Walk(node);
        return list;
    }

    static string NotTextMessage(JsonObject item)
        => $"選択中のアイテムがテキストではありません(種類: {ShortType(TypeOf(item))})。テキストのアイテムを選んでください";

    // ---------- 読み取り ----------
    /// <summary>
    /// コピーされたアイテムのデータを、項目ごとに読み取る(どの項目を保存するかは、あとで選ぶ)。
    /// テキストではないアイテムの場合は、Item が null で警告が Info に入る。
    /// </summary>
    public static (JsonObject? Item, string Info) ExtractAll(IReadOnlyList<ClipFormat> formats)
    {
        var parsed = EffectBundle.ParseAll(formats);
        if (parsed.Count == 0)
            return (null, "コピーされた内容を、JSONとして読み取れませんでした。「設定」の診断で、中身の形式を確認してください");

        List<JsonObject> items = new();
        foreach (var p in parsed)
        {
            items = FindItems(p.Node);
            if (items.Count > 0) break;
        }
        if (items.Count == 0)
            return (null, "コピーされた内容の中に、アイテムのデータが見つかりませんでした。YMM4でアイテムを選んでコピーしてください");

        var notText = items.FirstOrDefault(i => !IsTextType(TypeOf(i)));
        if (notText != null) return (null, "⚠ " + NotTextMessage(notText));

        return ((JsonObject)items[0].DeepClone(), "");
    }

    // ---------- 適用 ----------
    /// <summary>
    /// コピーした(選択中の)アイテムのデータに、保存した書式を上書きしたデータを作る。
    /// テキストではないアイテムが含まれる場合は、warning に警告を入れて null を返す。
    /// </summary>
    public static ApplyResult? ApplyToCopy(IReadOnlyList<ClipFormat> copied, string styleJson, HashSet<string> exclude, out string warning)
    {
        warning = "";
        if (JsonNode.Parse(styleJson) is not JsonObject style)
        {
            warning = "保存した書式を読み取れませんでした";
            return null;
        }
        var parsed = EffectBundle.ParseAll(copied);
        if (parsed.Count == 0)
        {
            warning = "コピーされた内容を、JSONとして読み取れませんでした。「設定」の診断で、中身の形式を確認してください";
            return null;
        }

        var byFormat = parsed.ToDictionary(p => p.Format);
        var result = new List<ClipFormat>();
        var modified = new List<ClipFormat>();
        var unchanged = new List<ClipFormat>();
        int applied = 0;
        foreach (var f in copied)
        {
            if (!byFormat.TryGetValue(f, out var p)) { result.Add(f); unchanged.Add(f); continue; }
            var items = FindItems(p.Node);
            if (items.Count == 0) { result.Add(f); unchanged.Add(f); continue; }

            foreach (var item in items)
            {
                if (!IsTextType(TypeOf(item))) { warning = NotTextMessage(item); return null; }
                foreach (var kv in style)
                {
                    if (kv.Key == "$type" || exclude.Contains(kv.Key)) continue;
                    item[kv.Key] = kv.Value?.DeepClone();
                }
                applied++;
            }
            var rebuilt = EffectBundle.Rebuild(p);
            result.Add(rebuilt);
            modified.Add(rebuilt);
        }
        if (applied == 0)
        {
            warning = "コピーされた内容の中に、アイテムのデータが見つかりませんでした";
            return null;
        }
        return new ApplyResult(result, modified, unchanged);
    }
}
