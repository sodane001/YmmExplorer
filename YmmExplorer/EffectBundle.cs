using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace YmmExplorer;

/// <summary>JSONの直前にある「長さ」の情報(.NETのデータ形式など)。書き換えて長さが変わった時に、合わせて直す</summary>
internal readonly record struct LengthField(bool Varint, int Offset, int Size);

/// <summary>クリップボードの1形式から読み取ったJSON(元のバイト列の前後も覚えておき、書き戻せるようにする)</summary>
internal sealed record ParsedJson(ClipFormat Format, JsonNode Node, Encoding Encoding, byte[] Prefix, byte[] Suffix, LengthField? Length);

/// <summary>書き換えたデータと、どの形式を書き換えられたか(書き換えられなかった形式が貼り付けに使われていないか確認する用)</summary>
internal sealed record ApplyResult(List<ClipFormat> Formats, List<ClipFormat> Modified, List<ClipFormat> Unchanged)
{
    static string Label(ClipFormat f) => $"{(f.Name ?? $"標準形式{f.Id}")}({FileItem.FormatSize(f.ByteCount)})";

    /// <summary>「書き換えた: A、B / 書き換えられなかった: C」</summary>
    public string Report()
    {
        var text = "書き換えた形式: " + (Modified.Count == 0 ? "なし" : string.Join("、", Modified.Select(Label)));
        if (Unchanged.Count > 0) text += " / 書き換えられなかった形式: " + string.Join("、", Unchanged.Select(Label));
        return text;
    }
}

/// <summary>取り出したエフェクトのまとまり</summary>
internal sealed record ExtractResult(string Json, List<string> Names, int Count);

/// <summary>
/// YMM4でアイテムをコピーしたデータ(JSON)から、そのアイテムのエフェクト一覧(VideoEffects / AudioEffects)だけを取り出し、
/// 1つの「まとまり」にする。適用する時は、YMM4が使うエフェクトの貼り付け形式の「見本」に、そのまとまりをはめ込む。
/// データの書式を確認できていないため、JSONとして読める場合だけ動作します。
/// </summary>
internal static class EffectBundle
{
    static readonly JsonSerializerOptions Out = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    static readonly JsonDocumentOptions DocOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

    internal static string ToJson(JsonNode node) => node.ToJsonString(Out);

    /// <summary>書き換えたJSONを、元の形式のバイト列(前後の部分はそのまま)に戻す</summary>
    internal static ClipFormat Rebuild(ParsedJson p)
    {
        var body = p.Encoding.GetBytes(ToJson(p.Node));
        var prefix = p.Prefix;
        if (p.Length is { } lf)
        {
            // JSONの長さが変わったので、直前の「長さ」の情報も書き直す
            var len = lf.Varint ? EncodeVarint(body.Length) : BitConverter.GetBytes(body.Length);
            prefix = prefix[..lf.Offset].Concat(len).Concat(prefix[(lf.Offset + lf.Size)..]).ToArray();
        }
        var data = new byte[prefix.Length + body.Length + p.Suffix.Length];
        prefix.CopyTo(data, 0);
        body.CopyTo(data, prefix.Length);
        p.Suffix.CopyTo(data, prefix.Length + body.Length);
        return new ClipFormat { Id = p.Format.Id, Name = p.Format.Name, Data = Convert.ToBase64String(data) };
    }

    static byte[] EncodeVarint(int value)
    {
        var bytes = new List<byte>();
        var v = (uint)value;
        while (v >= 0x80) { bytes.Add((byte)(v | 0x80)); v >>= 7; }
        bytes.Add((byte)v);
        return bytes.ToArray();
    }

    /// <summary>JSONの直前に、その長さを表す数(7ビット可変長 または 4バイト整数)があれば見つける</summary>
    static LengthField? FindLengthField(byte[] bytes, int start, int segmentLength)
    {
        for (int k = 1; k <= 5 && start - k >= 0; k++)
        {
            // 7ビット可変長: 最後のバイトだけ最上位ビットが0、それ以外は1
            if (bytes[start - 1] >= 0x80) break;
            bool ok = true;
            long value = 0;
            for (int i = 0; i < k; i++)
            {
                var b = bytes[start - k + i];
                if (i < k - 1 && b < 0x80) { ok = false; break; }
                value |= (long)(b & 0x7F) << (7 * i);
            }
            if (ok && value == segmentLength) return new LengthField(true, start - k, k);
        }
        if (start >= 4 && BitConverter.ToInt32(bytes, start - 4) == segmentLength)
            return new LengthField(false, start - 4, 4);
        return null;
    }

    // ---------- JSONの読み取り ----------
    public static List<ParsedJson> ParseAll(IReadOnlyList<ClipFormat> formats)
    {
        var list = new List<ParsedJson>();
        foreach (var f in formats)
        {
            byte[] bytes;
            try { bytes = Convert.FromBase64String(f.Data); }
            catch { continue; }
            var p = TryParseUtf8(f, bytes) ?? TryParseUtf16(f, bytes);
            if (p != null) list.Add(p);
        }
        return list;
    }

    // UTF-8: 先頭の { か [ から、最後の } か ] までをJSONとして読む(前後にある文字や長さ情報は、そのまま残す)
    static ParsedJson? TryParseUtf8(ClipFormat f, byte[] b)
    {
        int i = Array.FindIndex(b, x => x == 0x7B || x == 0x5B);
        int j = Array.FindLastIndex(b, x => x == 0x7D || x == 0x5D);
        if (i < 0 || j <= i) return null;
        return Parse(f, Encoding.UTF8, b, i, j + 1);
    }

    // UTF-16(Windowsのテキスト形式): 2バイトごとに { か [ を探す
    static ParsedJson? TryParseUtf16(ClipFormat f, byte[] b)
    {
        int i = -1, j = -1;
        for (int k = 0; k + 1 < b.Length; k += 2)
        {
            if (b[k + 1] != 0) continue;
            if (i < 0 && (b[k] == 0x7B || b[k] == 0x5B)) i = k;
            if (b[k] == 0x7D || b[k] == 0x5D) j = k;
        }
        if (i < 0 || j <= i) return null;
        return Parse(f, Encoding.Unicode, b, i, j + 2);
    }

    static ParsedJson? Parse(ClipFormat f, Encoding enc, byte[] bytes, int start, int end)
    {
        try
        {
            var text = enc.GetString(bytes, start, end - start);
            var node = JsonNode.Parse(text, null, DocOptions);
            if (node == null) return null;
            return new ParsedJson(f, node, enc, bytes[..start], bytes[end..], FindLengthField(bytes, start, end - start));
        }
        catch { return null; }
    }

    // ---------- アイテムのコピーから、エフェクトのまとまりを取り出す ----------
    /// <summary>失敗した時は Result が null で、理由が Info に入る</summary>
    public static (ExtractResult? Result, string Info) Extract(IReadOnlyList<ClipFormat> formats, TabKind kind)
    {
        var key = kind == TabKind.VideoEffect ? "VideoEffects" : "AudioEffects";
        var parsed = ParseAll(formats);
        if (parsed.Count == 0)
            return (null, "コピーされた内容を、JSONとして読み取れませんでした。「設定」の診断で、中身の形式を確認してください");

        var seen = new SortedSet<string>(StringComparer.Ordinal);
        JsonArray? best = null;
        foreach (var p in parsed)
        {
            var found = new List<JsonArray>();
            Collect(p.Node, key, found, seen);
            var withItems = found.FirstOrDefault(a => a.Count > 0);
            if (withItems != null) { best = withItems; break; }
            best ??= found.FirstOrDefault();
        }

        if (best == null)
        {
            var others = seen.Count > 0 ? $"(見つかった「…Effects」の項目: {string.Join("、", seen)})" : "";
            return (null, $"コピーされた内容の中に「{key}」が見つかりませんでした{others}");
        }
        return (new ExtractResult(best.ToJsonString(Out), NamesOf(best), best.Count), "");
    }

    static void Collect(JsonNode? node, string key, List<JsonArray> found, SortedSet<string> seen)
    {
        if (node is JsonObject o)
        {
            foreach (var kv in o)
            {
                if (kv.Key.EndsWith("Effects", StringComparison.OrdinalIgnoreCase)) seen.Add(kv.Key);
                if (kv.Value is JsonArray arr && string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
                    found.Add(arr);
                else
                    Collect(kv.Value, key, found, seen);
            }
        }
        else if (node is JsonArray a)
        {
            foreach (var e in a) Collect(e, key, found, seen);
        }
    }

    /// <summary>エフェクトの型名(例: YukkuriMovieMaker.Project.Effects.BlurEffect, YukkuriMovieMaker → BlurEffect)</summary>
    internal static List<string> NamesOf(JsonArray arr)
    {
        var names = new List<string>();
        foreach (var e in arr)
        {
            string name = "(不明なエフェクト)";
            if (e is JsonObject o && o["$type"] is JsonValue v && v.TryGetValue<string>(out var t) && !string.IsNullOrEmpty(t))
            {
                var typeName = t.Split(',')[0];
                name = typeName[(typeName.LastIndexOf('.') + 1)..];
            }
            names.Add(name);
        }
        return names;
    }

    // ---------- 見本にはめ込んで、貼り付け用のデータを作る ----------
    static bool IsEffectObject(JsonObject o)
        => o["$type"] is JsonValue v && v.TryGetValue<string>(out var t) && t.Contains("Effect", StringComparison.OrdinalIgnoreCase);

    static bool IsEffectArray(JsonNode? n)
        => n is JsonArray a && a.Count > 0 && a.All(e => e is JsonObject o && o.ContainsKey("$type"));

    /// <summary>
    /// 見本(YMM4でエフェクトをコピーした時のデータ)の中のエフェクト部分を、まとまりのエフェクトに入れ替えたデータを作る。
    /// 入れ替えられる場所が見本に無ければ null。
    /// </summary>
    public static List<ClipFormat>? BuildFromSample(IReadOnlyList<ClipFormat> sample, string effectsJson, TabKind kind)
    {
        if (JsonNode.Parse(effectsJson) is not JsonArray effects) return null;
        var key = kind == TabKind.VideoEffect ? "VideoEffects" : "AudioEffects";

        var parsed = ParseAll(sample).ToDictionary(p => p.Format);
        var result = new List<ClipFormat>();
        bool replaced = false;
        foreach (var f in sample)
        {
            if (!parsed.TryGetValue(f, out var p)) { result.Add(f); continue; }
            var root = ReplaceEffects(p.Node, effects, key);
            if (root == null) { result.Add(f); continue; }

            var body = p.Encoding.GetBytes(root.ToJsonString(Out));
            var data = new byte[p.Prefix.Length + body.Length + p.Suffix.Length];
            p.Prefix.CopyTo(data, 0);
            body.CopyTo(data, p.Prefix.Length);
            p.Suffix.CopyTo(data, p.Prefix.Length + body.Length);
            result.Add(new ClipFormat { Id = f.Id, Name = f.Name, Data = Convert.ToBase64String(data) });
            replaced = true;
        }
        return replaced ? result : null;
    }

    static JsonNode? ReplaceEffects(JsonNode root, JsonArray effects, string key)
    {
        if (IsEffectArray(root)) return effects.DeepClone();
        if (root is JsonObject ro && IsEffectObject(ro))
            return effects.Count == 1 ? effects[0]?.DeepClone() : effects.DeepClone();
        return ReplaceInside(root, effects, key) ? root : null;
    }

    static bool ReplaceInside(JsonNode node, JsonArray effects, string key)
    {
        if (node is JsonObject o)
        {
            foreach (var kv in o.ToList())
            {
                if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase) && (kv.Value is JsonArray || kv.Value == null))
                { o[kv.Key] = effects.DeepClone(); return true; }
                if (IsEffectArray(kv.Value)) { o[kv.Key] = effects.DeepClone(); return true; }
                if (kv.Value != null && ReplaceInside(kv.Value, effects, key)) return true;
            }
        }
        else if (node is JsonArray a)
        {
            foreach (var e in a)
                if (e != null && ReplaceInside(e, effects, key)) return true;
        }
        return false;
    }

    /// <summary>
    /// コピーした(選択中の)アイテムのデータに、エフェクトのまとまりを足す(または置き換える)。
    /// そのアイテムにエフェクトを付けられない場合は、warning に警告を入れて null を返す。
    /// </summary>
    public static ApplyResult? ApplyToCopy(IReadOnlyList<ClipFormat> copied, string effectsJson, TabKind kind,
                                            bool append, out string warning)
    {
        warning = "";
        if (JsonNode.Parse(effectsJson) is not JsonArray effects)
        {
            warning = "保存したエフェクトのまとまりを読み取れませんでした";
            return null;
        }
        var key = kind == TabKind.VideoEffect ? "VideoEffects" : "AudioEffects";
        var label = kind == TabKind.VideoEffect ? "映像" : "音声";

        var parsed = ParseAll(copied);
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
            var items = TextStyle.FindItems(p.Node);
            if (items.Count == 0) { result.Add(f); unchanged.Add(f); continue; }

            foreach (var item in items)
            {
                var prop = item.Select(kv => kv.Key).FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
                if (prop == null)
                {
                    warning = $"選択中のアイテム(種類: {TextStyle.ShortType(TextStyle.TypeOf(item))})には、{label}エフェクトを付けられません";
                    return null;
                }
                if (append && item[prop] is JsonArray existing)
                    foreach (var e in effects) existing.Add(e?.DeepClone());
                else
                    item[prop] = effects.DeepClone();
                applied++;
            }
            var rebuilt = Rebuild(p);
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

    /// <summary>この内容が、貼り付け形式の見本として使えるか(エフェクトの入れ替え先があるか)</summary>
    public static bool IsUsableSample(IReadOnlyList<ClipFormat> formats, TabKind kind)
        => BuildFromSample(formats, "[{\"$type\":\"Probe\"}]", kind) != null;
}
