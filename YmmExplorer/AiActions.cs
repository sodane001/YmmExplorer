using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace YmmExplorer;

// ====================================================================== アイテムの番号(AI とのやりとり用)
/// <summary>
/// タイムラインのアイテムに「#12」のような番号を付ける。同じアイテムには、会話の間ずっと同じ番号を使う
/// (AI が「#12 の拡大率を…」と提案してきた時に、どのアイテムか分かるように)。
/// </summary>
public static class AiItemIds
{
    static readonly ConditionalWeakTable<object, string> ids = new();
    static readonly Dictionary<string, WeakReference<object>> byId = new();
    static int next = 1;

    public static string IdOf(object item)
    {
        if (ids.TryGetValue(item, out var id)) return id;
        id = "#" + next++;
        ids.Add(item, id);
        byId[id] = new WeakReference<object>(item);
        return id;
    }

    public static object? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        id = id.Trim();
        if (!id.StartsWith('#')) id = "#" + id;
        return byId.TryGetValue(id, out var w) && w.TryGetTarget(out var o) ? o : null;
    }
}

// ====================================================================== 項目の読み書き
/// <summary>アイテム・エフェクトの「YMM4の画面に出る項目」を読み書きする(AI へ送る説明と、提案の実行に使う)</summary>
public static class AiInspector
{
    static readonly Dictionary<Type, List<(PropertyInfo p, string label)>> cache = new();

    /// <summary>YMM4の画面に出る項目(Display が付いたもの)</summary>
    public static List<(PropertyInfo p, string label)> DisplayProps(Type t)
    {
        if (cache.TryGetValue(t, out var hit)) return hit;
        var list = new List<(PropertyInfo, string)>();
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.GetIndexParameters().Length != 0 || !p.CanRead) continue;
            DisplayAttribute? d;
            try { d = p.GetCustomAttribute<DisplayAttribute>(true); } catch { continue; }
            if (d == null) continue;
            string label;
            try { label = d.GetName() ?? ""; } catch { label = ""; }
            if (string.IsNullOrEmpty(label)) label = YmmText.Resolve(d.Name ?? p.Name, d.ResourceType);
            list.Add((p, label));
        }
        cache[t] = list;
        return list;
    }

    public static bool IsAnimation(object? v)
        => v != null && v is not string && v.GetType().Name.Contains("Animation", StringComparison.Ordinal) && Ymm.Get(v, "Values") is IEnumerable;

    public static List<object> AnimationValues(object anim)
    {
        var list = new List<object>();
        if (Ymm.Get(anim, "Values") is IEnumerable e) foreach (var v in e) if (v != null) list.Add(v);
        return list;
    }

    static string Fmt(double d) => d.ToString("0.##", CultureInfo.InvariantCulture);

    static string EnumText(object e)
    {
        try
        {
            var f = e.GetType().GetField(e.ToString()!, BindingFlags.Public | BindingFlags.Static);
            var d = f?.GetCustomAttribute<DisplayAttribute>();
            if (d != null)
            {
                string n;
                try { n = d.GetName() ?? ""; } catch { n = ""; }
                if (string.IsNullOrEmpty(n)) n = YmmText.Resolve(d.Name ?? "", d.ResourceType);
                if (!string.IsNullOrEmpty(n)) return n;
            }
        }
        catch { }
        return e.ToString() ?? "";
    }

    /// <summary>値を短い文章に(読めない・複雑な値は null)</summary>
    public static string? ValueText(object? v, int maxText = 60)
    {
        switch (v)
        {
            case null: return null;
            case string s:
                s = s.Replace("\r", "").Replace("\n", "⏎");
                return "\"" + (s.Length > maxText ? s.Substring(0, maxText) + "…" : s) + "\"";
            case bool b: return b ? "true" : "false";
            case Enum e: return EnumText(e);
            case Color c: return c.A == 255 ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
            case SolidColorBrush br: return ValueText(br.Color);
            case double d: return Fmt(d);
            case float f: return Fmt(f);
            case decimal m: return Fmt((double)m);
            case int or long or short or byte or uint or ulong or ushort: return Convert.ToString(v, CultureInfo.InvariantCulture);
            case FontFamily ff: return "\"" + ff.Source + "\"";
        }
        if (IsAnimation(v))
        {
            var vals = AnimationValues(v).Select(x => Ymm.Get(x, "Value")).Select(x => x is IConvertible ? Convert.ToDouble(x, CultureInfo.InvariantCulture) : double.NaN)
                                         .Where(x => !double.IsNaN(x)).ToList();
            if (vals.Count == 0) return null;
            if (vals.Count == 1) return Fmt(vals[0]);
            var mode = Ymm.Get(v, "AnimationType") ?? Ymm.Get(v, "Type");
            var modeText = mode is Enum me ? EnumText(me) : "";
            var ex = AnimationExtraText(v);
            return "[" + string.Join(",", vals.Select(Fmt)) + "]" + (modeText.Length > 0 ? "(" + modeText + (ex.Length > 0 ? "・" + ex : "") + ")" : "");
        }
        return null;
    }

    /// <summary>「拡大率[Zoom]=100 不透明度[Opacity]=100 …」の形に。skip の項目は書かない</summary>
    public static string PropsText(object target, int max, ISet<string>? skip = null)
    {
        var parts = new List<string>();
        foreach (var (p, label) in DisplayProps(target.GetType()))
        {
            if (skip != null && skip.Contains(p.Name)) continue;
            object? raw;
            try { raw = Ymm.Get(target, p.Name); } catch { continue; }
            var text = ValueText(raw);
            if (text == null) continue;
            parts.Add(label == p.Name ? $"{p.Name}={text}" : $"{label}[{p.Name}]={text}");
            if (parts.Count >= max) { parts.Add("…"); break; }
        }
        return string.Join(" ", parts);
    }

    public static string EffectLabel(object fx)
    {
        var label = Ymm.Get(fx, "Label") as string;
        if (string.IsNullOrEmpty(label)) label = fx.GetType().Name.Replace("Effect", "");
        return label!;
    }

    public static List<object> Effects(object item, bool video)
    {
        var list = new List<object>();
        if (Ymm.Get(item, Ymm.EffectsProperty(video)) is IEnumerable e) foreach (var x in e) if (x != null) list.Add(x);
        return list;
    }

    // ---------------------------------------------------------------- 書き込み
    /// <summary>今の値を覚えておく(元に戻す用)。アニメーションは、キーごとの数値の一覧</summary>
    public static object? Snapshot(object target, PropertyInfo p)
    {
        var v = Ymm.Get(target, p.Name);
        if (IsAnimation(v)) return new AnimSnapshot(Ymm.Get(v, "AnimationType"), AnimationValues(v!).Select(x => ToD(Ymm.Get(x, "Value"))).ToList());
        return v;
    }

    /// <summary>アニメーションの覚え書き(動き方と、各点の値)</summary>
    public sealed record AnimSnapshot(object? Type, List<double> Values);

    static double ToD(object? v) { try { return v == null ? 0 : Convert.ToDouble(v, CultureInfo.InvariantCulture); } catch { return 0; } }

    /// <summary>覚えておいた値に戻す</summary>
    public static bool Restore(object target, PropertyInfo p, object? snapshot, out string err)
    {
        err = "";
        var cur = Ymm.Get(target, p.Name);
        if (IsAnimation(cur) && snapshot is AnimSnapshot snap)
        {
            if (!SetPointCount(cur!, snap.Values.Count, out err)) return false;
            var keys = AnimationValues(cur!);
            for (int i = 0; i < keys.Count && i < snap.Values.Count; i++)
                if (!SetPointValue(keys[i], snap.Values[i], out err)) return false;
            if (snap.Type != null) Ymm.SetValue(cur!, "AnimationType", snap.Type, out _);
            return true;
        }
        return Ymm.SetValue(target, p.Name, snapshot, out err);
    }

    public static PropertyInfo? FindProp(object target, string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim();
        var props = DisplayProps(target.GetType());
        var hit = props.FirstOrDefault(x => x.p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).p
                  ?? props.FirstOrDefault(x => x.label == name).p;
        if (hit != null) return hit;
        // 画面に出ない基本の項目(位置・長さ・レイヤーなど)
        if (name is "Frame" or "Length" or "Layer" or "IsHidden" or "IsLocked" or "Remark")
            return Ymm.FindProperty(target.GetType(), name);
        // AI がよく使う、ちがう呼び方(Size → FontSize など)
        if (Aliases.TryGetValue(name, out var alts))
            foreach (var alt in alts)
            {
                var h = props.FirstOrDefault(x => x.p.Name.Equals(alt, StringComparison.OrdinalIgnoreCase)).p
                        ?? Ymm.FindProperty(target.GetType(), alt);
                if (h != null) return h;
            }
        // 名前の終わりが同じもの・日本語名に含まれるもの(1つだけ当てはまる時)
        var tail = props.Where(x => x.p.Name.EndsWith(name, StringComparison.OrdinalIgnoreCase)).ToList();
        if (tail.Count == 1) return tail[0].p;
        var part = props.Where(x => x.label.Length > 0 && (x.label.Contains(name) || name.Contains(x.label))).ToList();
        if (part.Count == 1) return part[0].p;
        return null;
    }

    static readonly Dictionary<string, string[]> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Size"] = new[] { "FontSize", "Size" }, ["TextSize"] = new[] { "FontSize" }, ["FontSize"] = new[] { "FontSize", "Size" },
        ["Scale"] = new[] { "Zoom" }, ["ZoomRate"] = new[] { "Zoom" }, ["Alpha"] = new[] { "Opacity" }, ["Transparency"] = new[] { "Opacity" },
        ["Angle"] = new[] { "Rotation" }, ["Rotate"] = new[] { "Rotation" }, ["Color"] = new[] { "FontColor", "Color" }, ["TextColor"] = new[] { "FontColor" },
        ["Font"] = new[] { "Font", "FontFamily" }, ["Content"] = new[] { "Text", "Serif" }, ["Volume"] = new[] { "Volume" }, ["Speed"] = new[] { "PlaybackRate", "Speed" },
    };

    /// <summary>項目が見つからない時に、使える項目の名前を並べる(AI へのヒントにもなる)</summary>
    public static string PropNames(object target)
        => string.Join(" / ", DisplayProps(target.GetType()).Take(40).Select(x => x.label == x.p.Name ? x.p.Name : $"{x.label}[{x.p.Name}]"));

    /// <summary>AI から届いた値(JSON)を、項目に書く</summary>
    public static bool Apply(object target, PropertyInfo p, JsonNode? value, out string err)
    {
        err = "";
        var cur = Ymm.Get(target, p.Name);
        if (IsAnimation(cur)) return ApplyAnimation(cur!, value, out err);
        if (!TryConvert(value, p.PropertyType, out var converted, out err)) return false;
        if (p.GetSetMethod(true) == null) { err = "この項目は書き換えられません"; return false; }
        return Ymm.SetValue(target, p.Name, converted, out err);
    }

    // ---------------------------------------------------------------- アニメーション(直線移動・加減速移動など)
    static Type? animEnum;

    /// <summary>動き方の選択肢(AnimationType)の型。タイムラインのどれかのアニメーションから調べる</summary>
    public static Type? AnimationTypeEnum
    {
        get
        {
            if (animEnum != null) return animEnum;
            try
            {
                var tl = Ymm.Timeline();
                if (tl == null) return null;
                foreach (var it in (Ymm.Get(tl, "Items") as IEnumerable)?.Cast<object>() ?? Enumerable.Empty<object>())
                    foreach (var (p, _) in DisplayProps(it.GetType()))
                    {
                        var v = Ymm.Get(it, p.Name);
                        if (!IsAnimation(v)) continue;
                        var tp = Ymm.FindProperty(v!.GetType(), "AnimationType");
                        if (tp != null && tp.PropertyType.IsEnum) return animEnum = tp.PropertyType;
                    }
            }
            catch { }
            return null;
        }
    }

    /// <summary>AI に見せる「動き方」の名前の一覧</summary>
    public static string AnimationTypeNames()
    {
        var t = AnimationTypeEnum;
        return t == null ? "" : string.Join("、", Enum.GetValues(t).Cast<object>().Select(EnumText));
    }

    static bool SetPointValue(object point, double d, out string err)
    {
        var vp = Ymm.FindProperty(point.GetType(), "Value");
        object boxed = vp != null && vp.PropertyType != typeof(double) ? Convert.ChangeType(d, Nullable.GetUnderlyingType(vp.PropertyType) ?? vp.PropertyType, CultureInfo.InvariantCulture) : d;
        return Ymm.SetValue(point, "Value", boxed, out err);
    }

    static object? NewPoint(Type elementType, double v)
    {
        try
        {
            var o = Activator.CreateInstance(elementType, true);
            if (o != null && Ymm.SetValue(o, "Value", v, out _)) return o;
        }
        catch { }
        try { return Activator.CreateInstance(elementType, v); }
        catch { return null; }
    }

    /// <summary>点の数を count にする(足りなければ最後の値で足す。多ければ、あいだの点を減らす)</summary>
    static bool SetPointCount(object anim, int count, out string err)
    {
        err = "";
        var values = AnimationValues(anim);
        if (values.Count == 0) { err = "アニメーションの値がありません"; return false; }
        count = Math.Max(1, count);
        if (values.Count == count) return true;
        var elementType = values[0].GetType();
        var list = new List<object>(values);
        while (list.Count < count)
        {
            var add = NewPoint(elementType, ToD(Ymm.Get(list[^1], "Value")));
            if (add == null) { err = "アニメーションの点を作れませんでした"; return false; }
            list.Add(add);
        }
        while (list.Count > count) list.RemoveAt(list.Count > 2 ? list.Count - 2 : list.Count - 1);
        var p = Ymm.FindProperty(anim.GetType(), "Values");
        if (p == null) { err = "アニメーションの点の一覧が見つかりません"; return false; }
        var merged = Ymm.MergeEffects(p.PropertyType, null, list, false);
        if (merged == null) { err = Ymm.LastError; return false; }
        return Ymm.SetValue(anim, "Values", merged, out err);
    }

    // ---- 中間点の操作から使う
    public static bool RestoreAnimation(object anim, AnimSnapshot snap, out string err)
    {
        if (!SetPointCount(anim, snap.Values.Count, out err)) return false;
        var keys = AnimationValues(anim);
        for (int i = 0; i < keys.Count && i < snap.Values.Count; i++)
            if (!SetPointValue(keys[i], snap.Values[i], out err)) return false;
        if (snap.Type != null) Ymm.SetValue(anim, "AnimationType", snap.Type, out _);
        return true;
    }

    static bool ReplacePoints(object anim, List<object> list, out string err)
    {
        err = "";
        var p = Ymm.FindProperty(anim.GetType(), "Values");
        if (p == null) { err = "アニメーションの点の一覧が見つかりません"; return false; }
        var merged = Ymm.MergeEffects(p.PropertyType, null, list, false);
        if (merged == null) { err = Ymm.LastError; return false; }
        return Ymm.SetValue(anim, "Values", merged, out err);
    }

    /// <summary>index の位置に、値 v の点を入れる</summary>
    public static bool InsertPoint(object anim, int index, double v, out string err)
    {
        err = "";
        var list = AnimationValues(anim);
        if (list.Count == 0) { err = "アニメーションの値がありません"; return false; }
        var add = NewPoint(list[0].GetType(), v);
        if (add == null) { err = "中間点を作れませんでした"; return false; }
        list.Insert(Math.Clamp(index, 0, list.Count), add);
        return ReplacePoints(anim, list, out err);
    }

    public static bool RemovePoint(object anim, int index, out string err)
    {
        err = "";
        var list = AnimationValues(anim);
        if (index < 0 || index >= list.Count) { err = "その点はありません"; return false; }
        list.RemoveAt(index);
        return ReplacePoints(anim, list, out err);
    }

    public static bool SetPoint(object anim, int index, double v, out string err)
    {
        err = "";
        var list = AnimationValues(anim);
        if (index < 0 || index >= list.Count) { err = "その点はありません"; return false; }
        return SetPointValue(list[index], v, out err);
    }

    /// <summary>点の数を count にして、すべて値 v にする</summary>
    public static bool SetPointCountAll(object anim, int count, double v, out string err)
    {
        if (!SetPointCount(anim, count, out err)) return false;
        foreach (var k in AnimationValues(anim)) if (!SetPointValue(k, v, out err)) return false;
        return true;
    }

    /// <summary>動かない設定なら、直線移動(選択肢の2番目)にする</summary>
    public static void EnsureMoving(object anim)
    {
        var tp = Ymm.FindProperty(anim.GetType(), "AnimationType");
        if (tp == null || !tp.PropertyType.IsEnum) return;
        try
        {
            if (Convert.ToInt32(Ymm.Get(anim, "AnimationType") ?? 0, CultureInfo.InvariantCulture) != 0) return;
            var vals = Enum.GetValues(tp.PropertyType);
            if (vals.Length > 1) Ymm.SetValue(anim, "AnimationType", vals.GetValue(1), out _);
        }
        catch { }
    }

    /// <summary>
    /// アニメーションを書き換える。value は
    /// 数値(動かさずにその値) / 配列 [始点, (中間点…), 終点] / {"type":"直線移動","values":[0,300]} や {"type":"加減速移動","from":0,"to":300,"span":1}
    /// </summary>
    static bool ApplyAnimation(object anim, JsonNode? value, out string err)
    {
        err = "";
        var nums = new List<double>();
        string? typeName = null;
        JsonObject? extra = null;
        switch (value)
        {
            case JsonArray arr:
                foreach (var a in arr) if (TryDouble(a, out var d)) nums.Add(d);
                break;
            case JsonObject o:
                typeName = o["type"] is JsonValue tv && tv.TryGetValue<string>(out var tn) ? tn : o["AnimationType"] is JsonValue av && av.TryGetValue<string>(out var an) ? an : null;
                if (o["values"] is JsonArray va) foreach (var a in va) { if (TryDouble(a, out var d)) nums.Add(d); }
                else
                {
                    if (TryDouble(o["from"] ?? o["start"], out var f)) nums.Add(f);
                    if (o["mid"] is JsonArray mids) foreach (var m in mids) { if (TryDouble(m, out var d)) nums.Add(d); }
                    if (TryDouble(o["to"] ?? o["end"], out var t)) nums.Add(t);
                    if (nums.Count == 0 && TryDouble(o["value"], out var one)) nums.Add(one);
                }
                extra = o;
                break;
            default:
                if (TryDouble(value, out var single)) nums.Add(single);
                break;
        }
        if (nums.Count == 0 && typeName == null) { err = "数値ではありません"; return false; }

        // 動き方(直線移動など)
        var typeProp = Ymm.FindProperty(anim.GetType(), "AnimationType");
        bool moving = nums.Count >= 2;
        if (typeName != null && typeProp != null)
        {
            if (!TryConvert(JsonValue.Create(typeName), typeProp.PropertyType, out var typeVal, out err)) return false;
            if (!Ymm.SetValue(anim, "AnimationType", typeVal, out err)) return false;
            moving = Convert.ToInt32(typeVal, CultureInfo.InvariantCulture) != 0;
        }
        else if (nums.Count == 1 && typeProp != null && typeProp.PropertyType.IsEnum)
        {
            // 1つの値だけ: 動かさない
            Ymm.SetValue(anim, "AnimationType", Enum.ToObject(typeProp.PropertyType, 0), out _);
            moving = false;
        }
        else if (nums.Count >= 2 && typeProp != null && typeProp.PropertyType.IsEnum && Convert.ToInt32(Ymm.Get(anim, "AnimationType") ?? 0, CultureInfo.InvariantCulture) == 0)
        {
            // 値が2つ以上あるのに動かない設定なら、直線移動(選択肢の2番目)にする
            var vals = Enum.GetValues(typeProp.PropertyType);
            if (vals.Length > 1) Ymm.SetValue(anim, "AnimationType", vals.GetValue(1), out _);
        }

        // 点の数と値(中間点を作るのは、YMM4 のアイテムの中間点の位置とそろえる必要があるため、始点と終点だけ増やす)
        if (nums.Count > 0)
        {
            var have = AnimationValues(anim).Count;
            int want = moving ? Math.Max(2, nums.Count) : 1;
            if (want > 2 && have != want) { nums = new List<double> { nums[0], nums[^1] }; want = Math.Max(2, have); }
            if (want != have && (want <= 2 || have < 2) && !SetPointCount(anim, Math.Max(want, moving ? 2 : have), out err)) return false;
            var keys = AnimationValues(anim);
            for (int i = 0; i < keys.Count; i++)
            {
                double d = i == keys.Count - 1 ? nums[^1] : nums[Math.Min(i, nums.Count - 1)];
                if (!SetPointValue(keys[i], d, out err)) return false;
            }
        }

        // そのほかの設定(span・加減速の種類など。名前はアニメーションの項目名)
        if (extra != null)
            foreach (var (k, v) in extra)
            {
                if (k is "type" or "AnimationType" or "values" or "from" or "to" or "start" or "end" or "mid" or "value") continue;
                var prop = Ymm.FindProperty(anim.GetType(), k) ?? anim.GetType().GetProperties().FirstOrDefault(x => x.Name.Equals(k, StringComparison.OrdinalIgnoreCase));
                if (prop == null || prop.GetSetMethod(true) == null) continue;
                if (TryConvert(v, prop.PropertyType, out var cv, out _)) Ymm.SetValue(anim, prop.Name, cv, out _);
            }
        return true;
    }

    /// <summary>アニメーションの、動き方以外の設定(span など)の名前と今の値</summary>
    public static string AnimationExtraText(object anim)
    {
        var parts = new List<string>();
        foreach (var p in anim.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.Name is "Values" or "AnimationType" || p.GetIndexParameters().Length > 0 || p.GetSetMethod() == null) continue;
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            if (!(t.IsEnum || t.IsPrimitive)) continue;
            try { var v = ValueText(p.GetValue(anim)); if (v != null) parts.Add($"{p.Name}={v}"); } catch { }
        }
        return string.Join(",", parts.Take(6));
    }

    static bool TryDouble(JsonNode? n, out double d)
    {
        d = 0;
        if (n is not JsonValue v) return false;
        if (v.TryGetValue<double>(out d)) return true;
        if (v.TryGetValue<string>(out var s)) return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d);
        return false;
    }

    public static bool TryConvert(JsonNode? value, Type type, out object? result, out string err)
    {
        result = null; err = "";
        var t = Nullable.GetUnderlyingType(type) ?? type;
        var v = value as JsonValue;
        string? s = null;
        if (v != null && v.TryGetValue<string>(out var str)) s = str;
        try
        {
            if (t == typeof(string)) { result = s ?? value?.ToJsonString() ?? ""; return true; }
            if (t == typeof(bool))
            {
                if (v != null && v.TryGetValue<bool>(out var b)) { result = b; return true; }
                if (s != null) { result = s is "true" or "on" or "1" or "はい" or "オン"; return true; }
                err = "true / false ではありません"; return false;
            }
            if (t.IsEnum)
            {
                if (s != null)
                {
                    foreach (var name in Enum.GetNames(t))
                    {
                        var e = Enum.Parse(t, name);
                        if (name.Equals(s, StringComparison.OrdinalIgnoreCase) || EnumText(e) == s) { result = e; return true; }
                    }
                    err = $"「{s}」という選択肢はありません(選べるのは {string.Join("・", Enum.GetValues(t).Cast<object>().Select(EnumText))})";
                    return false;
                }
                if (TryDouble(value, out var n)) { result = Enum.ToObject(t, (int)n); return true; }
                err = "選択肢の名前ではありません"; return false;
            }
            if (t == typeof(Color) || t == typeof(SolidColorBrush) || t == typeof(Brush))
            {
                if (s == null) { err = "色は \"#RRGGBB\" で指定してください"; return false; }
                var c = (Color)ColorConverter.ConvertFromString(s);
                result = t == typeof(Color) ? c : new SolidColorBrush(c);
                return true;
            }
            if (t == typeof(FontFamily)) { if (s == null) { err = "フォント名ではありません"; return false; } result = new FontFamily(s); return true; }
            if (t.IsPrimitive || t == typeof(decimal))
            {
                if (!TryDouble(value, out var d)) { err = "数値ではありません"; return false; }
                if (t == typeof(int) || t == typeof(long) || t == typeof(short)) d = Math.Round(d);
                result = Convert.ChangeType(d, t, CultureInfo.InvariantCulture);
                return true;
            }
        }
        catch (Exception ex) { err = ex.Message; return false; }
        err = $"この種類の項目({t.Name})は、AIの提案からは変えられません";
        return false;
    }
}

// ====================================================================== 提案
public sealed class AiProposal : NotifyBase
{
    public string Op { get; init; } = "";
    public string ItemId { get; init; } = "";
    public string? Prop { get; init; }
    public string? Effect { get; init; }
    public bool Video { get; init; } = true;
    public int Index { get; init; } = -1;
    public JsonNode? Value { get; init; }
    public JsonObject? Params { get; init; }
    public string Reason { get; init; } = "";
    /// <summary>新しく作る(create)アイテムの種類: text / shape</summary>
    public string? ItemType { get; init; }
    /// <summary>新しく作るテキストの文字</summary>
    public string? Text { get; init; }
    /// <summary>新しく作る図形の種類(名前)</summary>
    public string? Shape { get; init; }
    /// <summary>新しく作るアイテムに入れる項目の値</summary>
    public JsonObject? Props { get; init; }
    /// <summary>新しく作るアイテムに付けるエフェクト(名前と項目の値)</summary>
    public List<(string name, JsonObject? prm)>? Effects { get; init; }
    /// <summary>中間点の値({"X":100,"fx0.Value":50} の形)</summary>
    public JsonObject? KeyValues { get; init; }
    /// <summary>AI が指定したシーン(1から)</summary>
    public int? Scene { get; init; }
    /// <summary>この提案の対象のシーン(0から。分からなければ -1)と、その表示</summary>
    public int SceneIndex { get; set; } = -1;
    public string SceneText { get; set; } = "";
    public bool HasScene => SceneText.Length > 0;
    /// <summary>「シーンを開く」で移動するフレーム</summary>
    public int JumpFrame { get; set; } = -1;
    /// <summary>配置(place)する素材の番号(M1・F3・P2 など)</summary>
    public string? Source { get; init; }
    public int? Frame { get; init; }
    public int? Layer { get; init; }
    public int? Length { get; init; }
    /// <summary>配置する時間(秒)。Frame が無い時に使う</summary>
    public double? Seconds { get; init; }
    public string Title { get; set; } = "";
    public DateTime Time { get; init; } = DateTime.Now;

    /// <summary>pending / done / rejected / failed</summary>
    string state = "pending";
    public string State { get => state; set { if (Set(ref state, value)) { Raise(nameof(IsPending)); Raise(nameof(CanRun)); Raise(nameof(StateText)); } } }
    public bool IsPending => state == "pending";
    /// <summary>実行できる(確認待ち、または失敗したのでもう一度)</summary>
    public bool CanRun => state is "pending" or "failed";
    string message = "";
    public string Message { get => message; set { if (Set(ref message, value)) Raise(nameof(StateText)); } }
    public string StateText => state switch
    {
        "done" => "✓ 実行しました" + (message.Length > 0 ? " " + message : ""),
        "rejected" => "却下しました",
        "failed" => "⚠ できませんでした: " + message,
        _ => "確認待ち",
    };
    public bool HasReason => Reason.Length > 0;
}

// ====================================================================== 変更ログ
public sealed class AiLogEntry : NotifyBase
{
    public DateTime Time { get; set; } = DateTime.Now;
    public string Title { get; set; } = "";
    public string Reason { get; set; } = "";
    public string Result { get; set; } = "";
    public string Ai { get; set; } = "";
    string undone = "";
    public string Undone { get => undone; set { if (Set(ref undone, value)) { Raise(nameof(CanUndo)); Raise(nameof(HasUndone)); } } }

    public string TimeText => Time.ToString("MM/dd HH:mm:ss");
    public bool HasReason => Reason.Length > 0;
    public bool HasUndone => undone.Length > 0;

    /// <summary>元に戻す処理(このYMM4を起動している間だけ使える。保存はしない)</summary>
    [System.Text.Json.Serialization.JsonIgnore] public Func<Task<string?>>? UndoAction { get; set; }
    [System.Text.Json.Serialization.JsonIgnore] public bool CanUndo => UndoAction != null && undone.Length == 0;
}

public static class AiLog
{
    static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "ai-log.json");

    public static List<AiLogEntry> Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<List<AiLogEntry>>(File.ReadAllText(FilePath)) ?? new();
        }
        catch { }
        return new();
    }

    public static void Save(IEnumerable<AiLogEntry> entries)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(entries.Take(500).ToList(), new JsonSerializerOptions { WriteIndented = true }));
            File.Move(tmp, FilePath, true);
        }
        catch { }
    }

    public static string Folder => Path.GetDirectoryName(FilePath)!;
}

// ====================================================================== 提案の読み取りと実行
public static class AiActions
{
    static readonly Regex Block = new(@"```[ \t]*ymm-actions?[ \t]*\r?\n?(.*?)(?:```|\z)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    /// <summary>AI に伝える「提案の書き方」</summary>
    public static string Instructions(string effectNames)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("## 操作の提案(この機能がオンです)");
        sb.AppendLine("アドバイスのうち、YMM4の項目を変えれば実現できるものは、返事のいちばん最後に、次の形式のブロックで提案してください。");
        sb.AppendLine("ユーザーが1つずつ確認して、OKしたものだけが実行されます。文章のアドバイスは今まで通り書いてください。");
        sb.AppendLine("```ymm-actions");
        sb.AppendLine("[");
        sb.AppendLine("  {\"op\":\"set\",\"item\":\"#12\",\"prop\":\"Zoom\",\"value\":120,\"reason\":\"文字が小さくて読みにくいため\"},");
        sb.AppendLine("  {\"op\":\"set\",\"item\":\"#12\",\"prop\":\"X\",\"value\":[0,300],\"reason\":\"アニメーションはキーごとの値を配列で\"},");
        sb.AppendLine("  {\"op\":\"addEffect\",\"item\":\"#12\",\"kind\":\"video\",\"effect\":\"ぼかし\",\"params\":{\"Blur\":4},\"reason\":\"…\"},");
        sb.AppendLine("  {\"op\":\"setEffect\",\"item\":\"#12\",\"kind\":\"video\",\"index\":0,\"prop\":\"Blur\",\"value\":8,\"reason\":\"…\"},");
        sb.AppendLine("  {\"op\":\"removeEffect\",\"item\":\"#12\",\"kind\":\"audio\",\"index\":1,\"reason\":\"…\"},");
        sb.AppendLine("  {\"op\":\"replaceEffect\",\"item\":\"#12\",\"kind\":\"video\",\"index\":0,\"effect\":\"画面外から登場退場\",\"params\":{},\"reason\":\"エフェクトの種類を変える(同じ名前の項目の値は引き継ぐ)\"},");
        sb.AppendLine("  {\"op\":\"place\",\"scene\":1,\"source\":\"M3\",\"frame\":600,\"layer\":5,\"length\":180,\"reason\":\"素材を配置する(length は省略可)\"},");
        sb.AppendLine("  {\"op\":\"create\",\"scene\":1,\"type\":\"text\",\"text\":\"第1章 はじめに\",\"frame\":0,\"layer\":8,\"length\":180,\"props\":{\"FontSize\":80,\"Y\":-300},\"reason\":\"見出しのテキストを新しく作る\"},");
        sb.AppendLine("  {\"op\":\"create\",\"scene\":2,\"type\":\"shape\",\"shape\":\"四角形\",\"frame\":120,\"layer\":2,\"length\":300,\"props\":{\"Opacity\":60},\"reason\":\"字幕の下に帯を敷く\"}");
        sb.AppendLine("]");
        sb.AppendLine("```");
        sb.AppendLine("きまり:");
        sb.AppendLine("・item はタイムラインの「#番号」。prop は [ ] の中の英語の名前をそのまま書く。開始位置・長さ・レイヤーは Frame・Length・Layer(フレーム単位。Layer はタイムラインに書いてある番号のまま。L03 なら 3)");
        sb.AppendLine("・value は、いまの値と同じ形で書く(数値 / true・false / \"文字\" / 色は \"#RRGGBB\" / 選択肢は表示されている名前)");
        sb.AppendLine("・アニメーション(移動)にできる項目(X・Y・拡大率など)は、value に {\"type\":\"動き方\",\"from\":始点,\"to\":終点} を書くと動かせる。例: {\"op\":\"set\",\"item\":\"#12\",\"prop\":\"X\",\"value\":{\"type\":\"直線移動\",\"from\":-800,\"to\":0}}");
        sb.AppendLine("  動きを止めて1つの値にする時は、数値だけを書く。アイテムの始めから終わりまでかけて、始点から終点へ動く。中間点は、今ある数のまま値だけ変えられる(values に全部の値)");
        sb.AppendLine("  span など、アニメーションのほかの設定も、タイムラインの ( ) の中に出ている英語の名前で書ける(例: \"span\":0.5)");
        sb.AppendLine("・kind は video(映像エフェクト)か audio(音声エフェクト)。index はそのアイテムの fx番号・afx番号");
        sb.AppendLine("・effect は、下の「使えるエフェクト」の名前から選ぶ");
        sb.AppendLine("・place は「配置できる素材」の番号(M=素材データベース、F=テンプレート・許可したフォルダーのファイル、P=プリセット)を source に書く。いま開いているシーンの frame(フレーム)・layer に置かれる。ほかのアイテムと重ならないレイヤーを選ぶ");
        sb.AppendLine("・タイムラインに無いアイテム・項目・素材は使わない。提案は大事なものから最大15個。変えるものが無ければブロックは書かない");
        sb.AppendLine("・中間点(キーフレーム)の操作。frame はアイテムの始まりからのフレーム、index は中間点の番号(0から。タイムラインの「中間点」の並び順)。values には項目名と値(エフェクトの項目は \"fx0.X\" のように番号を付ける)");
        sb.AppendLine("  {\"op\":\"addKeyframe\",\"item\":\"#12\",\"frame\":60,\"values\":{\"X\":200,\"Zoom\":150}}  …中間点を足す(書かなかった項目は今の動きのまま)");
        sb.AppendLine("  {\"op\":\"setKeyframe\",\"item\":\"#12\",\"index\":0,\"values\":{\"Y\":-100}} / {\"op\":\"moveKeyframe\",\"item\":\"#12\",\"index\":0,\"frame\":90} / {\"op\":\"removeKeyframe\",\"item\":\"#12\",\"index\":0}");
        sb.AppendLine("・アイテムを消す {\"op\":\"deleteItem\",\"item\":\"#12\"} / 複製する {\"op\":\"duplicateItem\",\"item\":\"#12\",\"frame\":600,\"layer\":4}(frame・layer は省略すると、すぐ後ろ・同じレイヤー)");
        sb.AppendLine("・create はテキスト(type:text、text に文字)・図形(type:shape、shape に「作れる図形」の名前)・グループ制御などの「作れるアイテム」(type に名前)を新しく作る。props には英語の項目名と値、effects にはエフェクト名の配列(例: [\"ぼかし\",{\"name\":\"縁取り\",\"params\":{\"Size\":5}}])を書ける");
        sb.AppendLine("  例: {\"op\":\"create\",\"type\":\"GroupItem\",\"frame\":0,\"layer\":10,\"length\":600,\"props\":{\"Zoom\":120},\"reason\":\"下のレイヤーをまとめて拡大\"}(グループ制御は、置いたレイヤーより下のアイテムにまとめて効く)");
        sb.AppendLine("・place と create の scene は、タイムラインの「シーン番号」(1から)。省略すると、いま開いているシーン");
        sb.AppendLine("・ブロックの中は JSON だけを書き、```ymm-actions で始めて ``` で閉じる");
        if (effectNames.Length > 0) sb.AppendLine(effectNames);
        try { var shapes = ShapeNamesText(); if (shapes.Length > 0) sb.AppendLine(shapes); } catch { }
        try { var items = CreatableItemsText(); if (items.Length > 0) sb.Append(items); } catch { }
        try { var anims = AiInspector.AnimationTypeNames(); if (anims.Length > 0) sb.AppendLine("動き方(アニメーションの type に書く名前): " + anims); } catch { }
        return sb.ToString();
    }

    /// <summary>返事から提案のブロックを取り出す(画面に出す文章からは消す)</summary>
    /// <summary>```json など、ほかの書き方のブロック(中身に "op" があるものだけ使う)</summary>
    static readonly Regex AnyBlock = new(@"```[ \t]*(?:json|jsonc|ymm|ymm-action|actions)?[ \t]*\r?\n(.*?)```", RegexOptions.Singleline | RegexOptions.IgnoreCase);

    static string Normalize(string op)
    {
        var o = op.Replace("_", "").Replace("-", "").Trim().ToLowerInvariant();
        return o switch
        {
            "set" or "setprop" or "setproperty" or "setvalue" or "change" or "update" or "move" => "set",
            "seteffect" or "seteffectparam" or "seteffectprop" or "seteffectproperty" or "effectset" => "setEffect",
            "addeffect" or "effectadd" or "add" => "addEffect",
            "removeeffect" or "deleteeffect" or "effectremove" or "remove" => "removeEffect",
            "replaceeffect" or "changeeffect" or "swapeffect" or "seteffecttype" or "effectreplace" => "replaceEffect",
            "place" or "placematerial" or "placefile" or "placepreset" or "placetemplate" or "insert" or "addmaterial" => "place",
            "create" or "createitem" or "additem" or "new" or "newitem" or "addtext" or "addshape" or "createtext" or "createshape" => "create",
            "addkeyframe" or "addmidpoint" or "addpoint" or "insertkeyframe" => "addKeyframe",
            "removekeyframe" or "deletekeyframe" or "removemidpoint" or "removepoint" => "removeKeyframe",
            "movekeyframe" or "movemidpoint" or "movepoint" => "moveKeyframe",
            "setkeyframe" or "setmidpoint" or "setpoint" or "editkeyframe" => "setKeyframe",
            "deleteitem" or "removeitem" or "delete" => "deleteItem",
            "duplicateitem" or "copyitem" or "duplicate" or "clone" => "duplicateItem",
            _ => op,
        };
    }

    /// <summary>返事から提案のブロックを取り出す(画面に出す文章からは消す)</summary>
    public static List<AiProposal> Extract(string reply, out string cleaned, out string? error)
    {
        var list = new List<AiProposal>();
        error = null;
        cleaned = reply;
        // 決めた書き方(```ymm-actions)が無い時は、```json などで "op" を含むものを使う
        var matches = Block.Matches(reply).Cast<Match>().ToList();
        var usedRegex = Block;
        if (matches.Count == 0)
        {
            matches = AnyBlock.Matches(reply).Cast<Match>().Where(m => m.Groups[1].Value.Contains("\"op\"")).ToList();
            usedRegex = AnyBlock;
        }
        if (matches.Count == 0) return list;
        foreach (var m in matches)
        {
            try
            {
                var node = JsonNode.Parse(m.Groups[1].Value.Trim(), documentOptions: new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
                var arr = node as JsonArray ?? (node is JsonObject o0 ? (o0["actions"] as JsonArray ?? new JsonArray(o0.DeepClone())) : new JsonArray());
                foreach (var a in arr.OfType<JsonObject>())
                {
                    string Str(params string[] keys)
                    {
                        foreach (var k in keys)
                            if (a[k] is JsonNode n) return n is JsonValue v && v.TryGetValue<string>(out var s) ? s : n.ToJsonString();
                        return "";
                    }
                    bool Has(params string[] keys) => keys.Any(k => a[k] != null);
                    int idx = -1;
                    foreach (var k in new[] { "index", "fx", "effectIndex" })
                        if (a[k] is JsonValue iv)
                        {
                            if (iv.TryGetValue<int>(out var i)) { idx = i; break; }
                            if (iv.TryGetValue<string>(out var si) && int.TryParse(si.TrimStart('f', 'x', 'a'), out i)) { idx = i; break; }
                        }
                    var kind = Str("kind", "type");
                    var item = Str("item", "id", "target").Trim();
                    if (item.Length > 0 && !item.StartsWith('#')) item = "#" + item;
                    var op = Normalize(Str("op", "action"));
                    if (op.Length == 0) continue;
                    list.Add(new AiProposal
                    {
                        Op = op,
                        ItemId = item,
                        Prop = Has("prop", "property") ? Str("prop", "property") : null,
                        Effect = Has("effect", "name") ? Str("effect", "name") : null,
                        Video = !kind.Equals("audio", StringComparison.OrdinalIgnoreCase),
                        Index = idx,
                        Value = (a["value"] ?? a["to"])?.DeepClone(),
                        Params = a["params"]?.DeepClone() as JsonObject,
                        Reason = Str("reason", "why"),
                        Source = Has("source", "material", "file", "preset", "template") ? Str("source", "material", "file", "preset", "template").Trim() : null,
                        Frame = IntOf(a, "frame", "start"),
                        Layer = IntOf(a, "layer"),
                        Length = IntOf(a, "length", "frames"),
                        ItemType = Has("itemType", "type") ? Str("itemType", "type").Trim().ToLowerInvariant() : (op.Contains("text", StringComparison.OrdinalIgnoreCase) || Str("op").Contains("text", StringComparison.OrdinalIgnoreCase) ? "text" : Str("op").Contains("shape", StringComparison.OrdinalIgnoreCase) ? "shape" : null),
                        Text = Has("text") ? Str("text") : null,
                        Shape = Has("shape") ? Str("shape") : null,
                        Props = a["props"]?.DeepClone() as JsonObject ?? a["properties"]?.DeepClone() as JsonObject,
                        Scene = IntOf(a, "scene"),
                        KeyValues = (a["values"] as JsonObject)?.DeepClone() as JsonObject,
                        Effects = a["effects"] is JsonArray fxa ? fxa.Select(x => x is JsonValue xv && xv.TryGetValue<string>(out var xn) ? (xn, (JsonObject?)null)
                                    : x is JsonObject xo ? ((xo["name"] ?? xo["effect"])?.ToString() ?? "", xo["params"]?.DeepClone() as JsonObject) : ("", null)).Where(x => x.Item1.Length > 0).ToList() : null,
                        Seconds = a["time"] is JsonValue tv && tv.TryGetValue<double>(out var sec) ? sec : a["seconds"] is JsonValue sv && sv.TryGetValue<double>(out var sec2) ? sec2 : (double?)null,
                    });
                }
            }
            catch (Exception ex) { error = "提案のブロックを読めませんでした: " + ex.Message; }
        }
        var used = new HashSet<string>(matches.Select(m => m.Value));
        cleaned = usedRegex.Replace(reply, m => used.Contains(m.Value) ? "" : m.Value).TrimEnd();
        List<object> scenes = new();
        try { scenes = ProjectSnapshot.FindScenes(out _); } catch { }
        foreach (var p in list) { FillScene(p, scenes); p.Title = Describe(p, scenes); }
        return list;
    }

    /// <summary>フレームを秒の表示に(「12.5秒」「1分02.5秒」)</summary>
    public static string SecText(double frames, double fps)
    {
        var sec = frames / (fps > 0 ? fps : 60);
        if (sec < 60) return $"{sec:0.##}秒";
        int m = (int)(sec / 60);
        return $"{m}分{sec - m * 60:00.##}秒";
    }

    /// <summary>提案のシーンの fps(分からなければ、いま開いているタイムライン)</summary>
    static double FpsFor(AiProposal p, List<object>? scenes = null)
    {
        try
        {
            scenes ??= ProjectSnapshot.FindScenes(out _);
            if (p.SceneIndex >= 0 && p.SceneIndex < scenes.Count) return ProjectSnapshot.FpsOf(ProjectSnapshot.TimelineOf(scenes[p.SceneIndex]));
            var tl = Ymm.Timeline();
            if (tl != null) return ProjectSnapshot.FpsOf(tl);
        }
        catch { }
        return 60;
    }

    /// <summary>Frame・Length(フレームで数える項目)なら、秒の表示にする</summary>
    static bool IsFrameProp(string? name) => name is "Frame" or "Length";
    static string FrameValueText(object? v, double fps)
    {
        try { return v == null ? "" : SecText(Convert.ToDouble(v is JsonValue jv ? jv.GetValue<double>() : v, CultureInfo.InvariantCulture), fps); }
        catch { return v?.ToString() ?? ""; }
    }

    /// <summary>提案の対象シーンを調べる(アイテムならそれが入っているシーン、配置・作成なら指定のシーンか、いま開いているシーン)</summary>
    public static void FillScene(AiProposal p, List<object> scenes)
    {
        try
        {
            int idx = -1;
            var item = AiItemIds.Find(p.ItemId);
            if (item != null)
            {
                for (int i = 0; i < scenes.Count && idx < 0; i++)
                    if (ProjectSnapshot.ItemsOf(ProjectSnapshot.TimelineOf(scenes[i])).Any(x => ReferenceEquals(x, item))) idx = i;
                p.JumpFrame = Convert.ToInt32(Ymm.Get(item, "Frame") ?? 0);
            }
            else if (p.Op is "place" or "create")
            {
                idx = p.Scene is int sc && sc >= 1 && sc <= scenes.Count ? sc - 1 : scenes.FindIndex(ProjectSnapshot.IsCurrent);
                p.JumpFrame = p.Frame ?? -1;
            }
            if (idx >= 0 && idx < scenes.Count)
            {
                p.SceneIndex = idx;
                var fps = ProjectSnapshot.FpsOf(ProjectSnapshot.TimelineOf(scenes[idx]));
                p.SceneText = $"シーン{idx + 1}「{ProjectSnapshot.SceneName(scenes[idx], idx)}」" + (p.JumpFrame >= 0 ? $" {SecText(p.JumpFrame, fps)}" : "");
            }
        }
        catch { }
    }

    static int? IntOf(JsonObject a, params string[] keys)
    {
        foreach (var k in keys)
            if (a[k] is JsonValue v)
            {
                if (v.TryGetValue<int>(out var i)) return i;
                if (v.TryGetValue<double>(out var d)) return (int)Math.Round(d);
                if (v.TryGetValue<string>(out var s) && int.TryParse(s.TrimStart('L', 'l'), out i)) return i;
            }
        return null;
    }

    static string ItemText(object? item, string id)
    {
        if (item == null) return id + "(見つかりません)";
        var kind = item.GetType().Name.Replace("Item", "");
        var text = (Ymm.Get(item, "Serif") as string) ?? (Ymm.Get(item, "Text") as string);
        if (!string.IsNullOrEmpty(text)) { text = text.Replace("\r", "").Replace("\n", " "); if (text.Length > 14) text = text.Substring(0, 14) + "…"; return $"{id} {kind}「{text}」"; }
        return $"{id} {kind}";
    }

    /// <summary>人が読む説明(「#12 テキスト「…」: 拡大率 100 → 120」)</summary>
    public static string Describe(AiProposal p, List<object>? scenes = null)
    {
        double fpsD = FpsFor(p, scenes);
        if (p.Op is "addKeyframe" or "removeKeyframe" or "moveKeyframe" or "setKeyframe" or "deleteItem" or "duplicateItem")
        {
            var it = AiItemIds.Find(p.ItemId);
            var h = ItemText(it, p.ItemId);
            string Vals() => p.KeyValues is { Count: > 0 } kv ? " " + string.Join(" ", kv.Select(x => $"{x.Key}={x.Value?.ToJsonString()}")) : "";
            return p.Op switch
            {
                "addKeyframe" => $"{h}: {(p.Frame != null ? SecText(p.Frame.Value, fpsD) + "(アイテムの始めから)" : "?")} に中間点を足す" + Vals(),
                "removeKeyframe" => $"{h}: {p.Index + 1} つ目の中間点を消す",
                "moveKeyframe" => $"{h}: {p.Index + 1} つ目の中間点を {(p.Frame != null ? SecText(p.Frame.Value, fpsD) : "?")}(アイテムの始めから)へ動かす",
                "setKeyframe" => $"{h}: {p.Index + 1} つ目の中間点の値を変える" + Vals(),
                "deleteItem" => $"{h}: アイテムを消す",
                _ => $"{h}: アイテムを複製する" + (p.Frame != null ? $"({SecText(p.Frame.Value, fpsD)}・L{p.Layer ?? 0:00} へ)" : "(すぐ後ろへ)"),
            };
        }
        if (p.Op == "create")
        {
            var where = p.Frame != null ? SecText(p.Frame.Value, fpsD) : p.Seconds != null ? SecText(p.Seconds.Value * fpsD, fpsD) : "シークバーの位置";
            var what = p.ItemType == "shape" ? $"図形「{p.Shape ?? "?"}」"
                     : p.ItemType is null or "text" ? $"テキスト「{(p.Text is { Length: > 20 } t ? t[..20] + "…" : p.Text)}」"
                     : $"{(ResolveItemType(p.ItemType) is { } rt ? CreatableItems().FirstOrDefault(x => x.type == rt).label ?? rt : p.ItemType)}";
            if (p.Effects is { Count: > 0 }) what += "(エフェクト: " + string.Join("・", p.Effects.Select(e => e.name)) + ")";
            return $"新しい{what}を L{p.Layer ?? 0:00} の {where} に作る" + (p.Length != null ? $"(長さ {SecText(p.Length.Value, fpsD)})" : "") + (p.Props is { Count: > 0 } ? " " + p.Props.ToJsonString() : "");
        }
        if (p.Op == "place")
        {
            var src = AiLibrary.Find(p.Source);
            var where = p.Frame != null ? SecText(p.Frame.Value, fpsD) : p.Seconds != null ? SecText(p.Seconds.Value * fpsD, fpsD) : "シークバーの位置";
            return $"{(src == null ? $"{p.Source}(見つかりません)" : $"{src.Id} {src.KindText}「{src.Name}」")} を L{p.Layer ?? 0:00} の {where} に配置" + (p.Length != null ? $"(長さ {SecText(p.Length.Value, fpsD)})" : "");
        }
        var item = AiItemIds.Find(p.ItemId);
        var head = ItemText(item, p.ItemId);
        var kind = p.Video ? "映像エフェクト" : "音声エフェクト";
        string ValueOut(JsonNode? n) => n is JsonValue v && v.TryGetValue<string>(out var s) ? s : n?.ToJsonString() ?? "";
        try
        {
            switch (p.Op)
            {
                case "set":
                {
                    var prop = item == null ? null : AiInspector.FindProp(item, p.Prop);
                    var label = prop == null ? p.Prop : AiInspector.DisplayProps(item!.GetType()).FirstOrDefault(x => x.p == prop).label ?? prop.Name;
                    if (prop != null && IsFrameProp(prop.Name))
                        return $"{head}: {(prop.Name == "Frame" ? "開始位置" : "長さ")} {FrameValueText(Ymm.Get(item!, prop.Name), fpsD)} → {FrameValueText(p.Value, fpsD)}";
                    var before = prop == null ? null : AiInspector.ValueText(Ymm.Get(item!, prop.Name));
                    return $"{head}: {label} {before ?? ""} → {ValueOut(p.Value)}";
                }
                case "replaceEffect":
                {
                    var fx = item == null ? null : AiInspector.Effects(item, p.Video).ElementAtOrDefault(p.Index);
                    return $"{head}: {kind}「{(fx == null ? $"{p.Index}番" : AiInspector.EffectLabel(fx))}」を「{p.Effect}」に変える" + (p.Params is { Count: > 0 } ? " " + p.Params.ToJsonString() : "");
                }
                case "setEffect" when IsEffectNameProp(p.Prop) && !HasEffectProp(item, p):
                {
                    var fx = item == null ? null : AiInspector.Effects(item, p.Video).ElementAtOrDefault(p.Index);
                    return $"{head}: {kind}「{(fx == null ? $"{p.Index}番" : AiInspector.EffectLabel(fx))}」を「{ValueOut(p.Value)}」に変える";
                }
                case "setEffect":
                {
                    var fx = item == null ? null : AiInspector.Effects(item, p.Video).ElementAtOrDefault(p.Index);
                    var prop = fx == null ? null : AiInspector.FindProp(fx, p.Prop);
                    var label = prop == null ? p.Prop : AiInspector.DisplayProps(fx!.GetType()).FirstOrDefault(x => x.p == prop).label ?? prop.Name;
                    var before = prop == null ? null : AiInspector.ValueText(Ymm.Get(fx!, prop.Name));
                    return $"{head}: {kind}「{(fx == null ? "?" : AiInspector.EffectLabel(fx))}」の {label} {before ?? ""} → {ValueOut(p.Value)}";
                }
                case "addEffect":
                    return $"{head}: {kind}「{p.Effect}」を足す" + (p.Params is { Count: > 0 } ? " " + p.Params.ToJsonString() : "");
                case "removeEffect":
                {
                    var fx = item == null ? null : AiInspector.Effects(item, p.Video).ElementAtOrDefault(p.Index);
                    return $"{head}: {kind}「{(fx == null ? $"{p.Index}番" : AiInspector.EffectLabel(fx))}」を外す";
                }
            }
        }
        catch { }
        return $"{head}: {p.Op} (よく分からない提案です)";
    }

    static List<BuiltinEffect>? videoFx, audioFx;
    static List<BuiltinEffect> Catalog(bool video)
    {
        if (video) return videoFx ??= SafeFind(true);
        return audioFx ??= SafeFind(false);
        static List<BuiltinEffect> SafeFind(bool v) { try { return BuiltinCatalog.FindEffects(v, out _); } catch { return new(); } }
    }

    /// <summary>AI に見せる「使えるエフェクト」の一覧</summary>
    public static string EffectNamesText()
    {
        var sb = new StringBuilder();
        var v = Catalog(true).Select(e => e.Name).Distinct().ToList();
        var a = Catalog(false).Select(e => e.Name).Distinct().ToList();
        if (v.Count > 0) sb.AppendLine("使えるエフェクト(映像): " + string.Join("、", v.Take(250)));
        if (a.Count > 0) sb.AppendLine("使えるエフェクト(音声): " + string.Join("、", a.Take(120)));
        return sb.ToString();
    }

    static BuiltinEffect? FindEffect(string? name, bool video)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        name = name.Trim();
        var list = Catalog(video);
        return list.FirstOrDefault(e => e.Name == name)
               ?? list.FirstOrDefault(e => e.Type.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || e.Type.Name.Equals(name + "Effect", StringComparison.OrdinalIgnoreCase))
               ?? list.FirstOrDefault(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    static async Task<bool> SetEffectsAsync(object item, bool video, List<object> next)
    {
        var p = Ymm.FindProperty(item.GetType(), Ymm.EffectsProperty(video)) ?? throw new InvalidOperationException("このアイテムにはエフェクトがありません");
        var merged = Ymm.MergeEffects(p.PropertyType, null, next, false) ?? throw new InvalidOperationException(Ymm.LastError);
        string err = "";
        bool ok = false;
        await Ymm.EditAsync(item, () => ok = Ymm.SetValue(item, p.Name, merged, out err));
        if (!ok) throw new InvalidOperationException(err);
        return true;
    }

    static bool IsEffectNameProp(string? prop) => prop != null && prop.Trim().ToLowerInvariant() is "effectname" or "effect" or "name" or "type" or "effecttype" or "kind" or "エフェクト" or "種類";

    static bool HasEffectProp(object? item, AiProposal p)
    {
        if (item == null) return false;
        var fx = AiInspector.Effects(item, p.Video).ElementAtOrDefault(p.Index);
        return fx != null && AiInspector.FindProp(fx, p.Prop) != null;
    }

    /// <summary>index 番のエフェクトを、別のエフェクトに入れ替える(同じ名前の項目の値は引き継ぎ、params で上書き)</summary>
    static async Task<(string result, Func<Task<string?>>? undo)> ReplaceEffectAsync(object item, AiProposal p, string? effectName)
    {
        var list = AiInspector.Effects(item, p.Video);
        if (p.Index < 0 || p.Index >= list.Count) throw new InvalidOperationException($"{p.Index} 番のエフェクトがありません");
        var old = list[p.Index];
        var def = FindEffect(effectName, p.Video) ?? throw new InvalidOperationException($"「{effectName}」というエフェクトが見つかりません");
        var fx = Activator.CreateInstance(def.Type, true) ?? throw new InvalidOperationException("エフェクトを作れませんでした");
        // 同じ名前・同じ種類の項目(X・Y・効果時間など)は、前のエフェクトの値を引き継ぐ
        int carried = 0;
        foreach (var (np, _) in AiInspector.DisplayProps(fx.GetType()))
        {
            var op = AiInspector.DisplayProps(old.GetType()).FirstOrDefault(x => x.p.Name == np.Name).p;
            if (op == null) continue;
            try
            {
                var ov = Ymm.Get(old, op.Name);
                if (AiInspector.IsAnimation(ov))
                {
                    if (AiInspector.Restore(fx, np, AiInspector.Snapshot(old, op), out _)) carried++;
                }
                else if (op.PropertyType == np.PropertyType && np.GetSetMethod(true) != null && (np.PropertyType.IsPrimitive || np.PropertyType.IsEnum || np.PropertyType == typeof(string)))
                {
                    if (Ymm.SetValue(fx, np.Name, ov, out _)) carried++;
                }
            }
            catch { }
        }
        var notes = new List<string>();
        if (p.Params != null)
            foreach (var (k, v) in p.Params)
            {
                var prop = AiInspector.FindProp(fx, k);
                if (prop == null) { notes.Add($"{k}: 項目なし"); continue; }
                if (!AiInspector.Apply(fx, prop, v, out var e)) notes.Add($"{k}: {e}");
            }
        if (Ymm.Get(old, "IsEnabled") is bool en) Ymm.SetValue(fx, "IsEnabled", en, out _);
        list[p.Index] = fx;
        await SetEffectsAsync(item, p.Video, list);
        int at = p.Index;
        var oldLabel = AiInspector.EffectLabel(old);
        return ($"「{oldLabel}」を「{def.Name}」に入れ替えました" + (carried > 0 ? $"(同じ項目 {carried} 個の値を引き継ぎ)" : "") + (notes.Count > 0 ? "(設定できなかった項目: " + string.Join(" / ", notes) + ")" : ""), async () =>
        {
            var now = AiInspector.Effects(item, p.Video);
            var i = now.IndexOf(fx);
            if (i < 0) return "入れ替えたエフェクトが見つかりません";
            now[i] = old;
            await SetEffectsAsync(item, p.Video, now);
            return null;
        });
    }

    /// <summary>提案を実行する。成功したら、元に戻す処理と結果の説明を返す</summary>
    public static async Task<(string result, Func<Task<string?>>? undo)> ExecuteAsync(AiProposal p)
    {
        if (p.Op == "place") return await PlaceAsync(p);
        if (p.Op == "create") return await CreateAsync(p);
        if (p.Op is "addKeyframe" or "removeKeyframe" or "moveKeyframe" or "setKeyframe") return await AiKeyframes.ExecuteAsync(p);
        if (p.Op == "deleteItem") return await DeleteItemAsync(p);
        if (p.Op == "duplicateItem") return await DuplicateItemAsync(p);
        var item = AiItemIds.Find(p.ItemId) ?? throw new InvalidOperationException($"アイテム {p.ItemId} が見つかりません(消されたか、YMM4を開き直しました)");
        switch (p.Op)
        {
            case "set":
            {
                var prop = AiInspector.FindProp(item, p.Prop) ?? throw new InvalidOperationException($"「{p.Prop}」という項目がありません(使える項目: {AiInspector.PropNames(item)})");
                var before = AiInspector.Snapshot(item, prop);
                var beforeText = AiInspector.ValueText(Ymm.Get(item, prop.Name)) ?? "";
                string err = "";
                bool ok = false;
                await Ymm.EditAsync(item, () => ok = AiInspector.Apply(item, prop, p.Value, out err));
                if (!ok) throw new InvalidOperationException(err);
                var after = AiInspector.ValueText(Ymm.Get(item, prop.Name)) ?? "";
                if (IsFrameProp(prop.Name))
                {
                    var fpsE = FpsFor(p);
                    beforeText = FrameValueText(before, fpsE);
                    after = FrameValueText(Ymm.Get(item, prop.Name), fpsE);
                }
                return ($"{beforeText} → {after}", async () =>
                {
                    string e2 = ""; bool ok2 = false;
                    await Ymm.EditAsync(item, () => ok2 = AiInspector.Restore(item, prop, before, out e2));
                    return ok2 ? null : e2;
                });
            }
            case "replaceEffect":
                return await ReplaceEffectAsync(item, p, p.Effect);
            case "setEffect" when IsEffectNameProp(p.Prop) && !HasEffectProp(item, p):
                // 「エフェクトの名前を変える」= 別のエフェクトに入れ替える
                return await ReplaceEffectAsync(item, p, p.Value is JsonValue nv && nv.TryGetValue<string>(out var nn) ? nn : p.Value?.ToJsonString());
            case "setEffect":
            {
                var fx = AiInspector.Effects(item, p.Video).ElementAtOrDefault(p.Index) ?? throw new InvalidOperationException($"{p.Index} 番のエフェクトがありません");
                var prop = AiInspector.FindProp(fx, p.Prop) ?? throw new InvalidOperationException($"エフェクトに「{p.Prop}」という項目がありません(使える項目: {AiInspector.PropNames(fx)})");
                var before = AiInspector.Snapshot(fx, prop);
                var beforeText = AiInspector.ValueText(Ymm.Get(fx, prop.Name)) ?? "";
                string err = "";
                bool ok = false;
                await Ymm.EditAsync(item, () => ok = AiInspector.Apply(fx, prop, p.Value, out err));
                if (!ok) throw new InvalidOperationException(err);
                var after = AiInspector.ValueText(Ymm.Get(fx, prop.Name)) ?? "";
                return ($"{beforeText} → {after}", async () =>
                {
                    string e2 = ""; bool ok2 = false;
                    await Ymm.EditAsync(item, () => ok2 = AiInspector.Restore(fx, prop, before, out e2));
                    return ok2 ? null : e2;
                });
            }
            case "addEffect":
            {
                var def = FindEffect(p.Effect, p.Video) ?? throw new InvalidOperationException($"「{p.Effect}」というエフェクトが見つかりません");
                var fx = Activator.CreateInstance(def.Type, true) ?? throw new InvalidOperationException("エフェクトを作れませんでした");
                var notes = new List<string>();
                if (p.Params != null)
                    foreach (var (k, v) in p.Params)
                    {
                        var prop = AiInspector.FindProp(fx, k);
                        if (prop == null) { notes.Add($"{k}: 項目なし"); continue; }
                        if (!AiInspector.Apply(fx, prop, v, out var e)) notes.Add($"{k}: {e}");
                    }
                var list = AiInspector.Effects(item, p.Video);
                list.Add(fx);
                await SetEffectsAsync(item, p.Video, list);
                return ("「" + def.Name + "」を足しました" + (notes.Count > 0 ? "(設定できなかった項目: " + string.Join(" / ", notes) + ")" : ""), async () =>
                {
                    var now = AiInspector.Effects(item, p.Video);
                    if (!now.Remove(fx)) return "足したエフェクトが見つかりません";
                    await SetEffectsAsync(item, p.Video, now);
                    return null;
                });
            }
            case "removeEffect":
            {
                var list = AiInspector.Effects(item, p.Video);
                if (p.Index < 0 || p.Index >= list.Count) throw new InvalidOperationException($"{p.Index} 番のエフェクトがありません");
                var fx = list[p.Index];
                list.RemoveAt(p.Index);
                await SetEffectsAsync(item, p.Video, list);
                int at = p.Index;
                return ("「" + AiInspector.EffectLabel(fx) + "」を外しました", async () =>
                {
                    var now = AiInspector.Effects(item, p.Video);
                    now.Insert(Math.Min(at, now.Count), fx);
                    await SetEffectsAsync(item, p.Video, now);
                    return null;
                });
            }
        }
        throw new InvalidOperationException($"「{p.Op}」という操作は使えません");
    }

    // ---------------------------------------------------------------- 素材の配置
    static async Task<double> DurationAsync(string path, bool video)
    {
        try
        {
            var f = await global::Windows.Storage.StorageFile.GetFileFromPathAsync(path).AsTask();
            return video ? (await f.Properties.GetVideoPropertiesAsync().AsTask()).Duration.TotalSeconds
                         : (await f.Properties.GetMusicPropertiesAsync().AsTask()).Duration.TotalSeconds;
        }
        catch { return 0; }
    }

    /// <summary>アイテムが入っているタイムライン</summary>
    static object? TimelineOfItem(object item)
    {
        try
        {
            foreach (var sc in ProjectSnapshot.FindScenes(out _))
            {
                var tl = ProjectSnapshot.TimelineOf(sc);
                if (ProjectSnapshot.ItemsOf(tl).Any(x => ReferenceEquals(x, item))) return tl;
            }
        }
        catch { }
        var cur = Ymm.Timeline();
        return cur != null && ProjectSnapshot.ItemsOf(cur).Any(x => ReferenceEquals(x, item)) ? cur : null;
    }

    static async Task<(string result, Func<Task<string?>>? undo)> DeleteItemAsync(AiProposal p)
    {
        var item = AiItemIds.Find(p.ItemId) ?? throw new InvalidOperationException($"アイテム {p.ItemId} が見つかりません");
        await OpenSceneAsync(p);
        var tl = TimelineOfItem(item) ?? throw new InvalidOperationException("アイテムが入っているタイムラインが見つかりません");
        int frame = Convert.ToInt32(Ymm.Get(item, "Frame") ?? 0), layer = Convert.ToInt32(Ymm.Get(item, "Layer") ?? 0);
        // 元に戻す時のために、消す前の姿をテンプレートにしておく
        var keep = Ymm.CreateTemplate("AIが消したアイテム", new[] { item });
        if (!Ymm.RemoveItems(tl, new[] { item })) throw new InvalidOperationException(Ymm.LastError);
        return ("消しました", keep == null ? null : async () =>
        {
            await OpenSceneAsync(p);
            var (ok, msg) = await Ymm.PlaceTemplateAtAsync(keep, frame, layer);
            return ok ? null : msg;
        });
    }

    static async Task<(string result, Func<Task<string?>>? undo)> DuplicateItemAsync(AiProposal p)
    {
        var item = AiItemIds.Find(p.ItemId) ?? throw new InvalidOperationException($"アイテム {p.ItemId} が見つかりません");
        await OpenSceneAsync(p);
        var tl = TimelineOfItem(item) ?? Ymm.Timeline() ?? throw new InvalidOperationException("タイムラインが見つかりません");
        int f0 = Convert.ToInt32(Ymm.Get(item, "Frame") ?? 0), len = Convert.ToInt32(Ymm.Get(item, "Length") ?? 0), l0 = Convert.ToInt32(Ymm.Get(item, "Layer") ?? 0);
        var template = Ymm.CreateTemplate("AIが複製したアイテム", new[] { item }) ?? throw new InvalidOperationException(Ymm.LastError);
        int frame = p.Frame ?? (p.Seconds != null ? (int)Math.Round(p.Seconds.Value * ProjectSnapshot.FpsOf(tl)) : f0 + len);
        return await PlaceTemplateAsync(tl, template, frame, p.Layer ?? l0);
    }

    /// <summary>提案のシーンを YMM4 で開く(すでに開いていれば何もしない)</summary>
    public static async Task<bool> OpenSceneAsync(AiProposal p)
    {
        if (p.SceneIndex < 0) return true;
        var scenes = ProjectSnapshot.FindScenes(out var owner);
        if (p.SceneIndex >= scenes.Count) return false;
        var scene = scenes[p.SceneIndex];
        if (ProjectSnapshot.IsCurrent(scene)) return true;
        if (!ProjectSnapshot.Switch(scene, owner)) return false;
        await Task.Delay(300);
        return true;
    }

    // ---------------------------------------------------------------- 図形・テキスト・グループ制御などを新しく作る
    static List<ShapeInfo> Shapes()
    {
        if (ShapeDiscovery.Found.Count > 0) return ShapeDiscovery.Found.ToList();
        try { return ShapeDiscovery.Scan(); } catch { return new(); }
    }

    /// <summary>図形タブに登録してある図形(配置データつき。自分で登録した図形も)</summary>
    static List<ShapeEntry> StoredShapes()
    {
        var list = new List<ShapeEntry>();
        try
        {
            int n = Math.Clamp(ExplorerSettings.Instance.ShapeTabCount, 1, 100);
            for (int slot = 1; slot <= n; slot++)
                foreach (var e in ShapeStore.Instance.All(slot))
                    if (!string.IsNullOrEmpty(e.TemplateJson) && !list.Any(x => x.Name == e.Name && x.Key == e.Key)) list.Add(e);
        }
        catch { }
        return list;
    }

    /// <summary>AI に見せる「作れる図形」の一覧(YMM4 に入っているすべての図形と、図形タブに登録した図形)</summary>
    public static string ShapeNamesText()
    {
        var names = Shapes().Select(s => s.Name).Concat(StoredShapes().Select(e => e.Name)).Where(n => n.Length > 0).Distinct().ToList();
        return names.Count > 0 ? "作れる図形(create の shape に書く名前): " + string.Join("、", names.Take(200)) : "";
    }

    static readonly Dictionary<string, string> ItemLabels = new()
    {
        ["GroupItem"] = "グループ制御", ["EffectItem"] = "映像エフェクト", ["VideoEffectItem"] = "映像エフェクト", ["TransitionItem"] = "トランジション",
        ["FrameBufferItem"] = "フレームバッファ", ["ScreenSplitItem"] = "画面分割", ["TextItem"] = "テキスト", ["ShapeItem"] = "図形",
    };
    static readonly HashSet<string> NeedsSource = new() { "VoiceItem", "ImageItem", "VideoItem", "AudioItem", "TachieItem", "TachieFaceItem", "SceneItem", "TextItem", "ShapeItem" };
    static List<(string type, string label, string props)>? creatable;

    /// <summary>YMM4の「アイテムの初期値」から分かる、新しく作れるアイテム(グループ制御・映像エフェクトなど。テキスト・図形・ファイルが要るものは除く)</summary>
    static List<(string type, string label, string props)> CreatableItems()
    {
        if (creatable != null) return creatable;
        var list = new List<(string, string, string)>();
        try
        {
            var st = Ymm.FindType("YukkuriMovieMaker.Settings.ItemSettings");
            var defs = Ymm.Get(st?.GetProperty("Default", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null), "DefaultItems");
            if (defs != null)
                foreach (var p in defs.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (NeedsSource.Contains(p.Name) || !p.Name.EndsWith("Item")) continue;
                    object? v;
                    try { v = p.GetValue(defs); } catch { continue; }
                    if (v == null) continue;
                    var label = ItemLabels.TryGetValue(p.Name, out var l) ? l : p.Name.Replace("Item", "");
                    var props = string.Join(" ", AiInspector.DisplayProps(v.GetType()).Take(30).Select(x => x.label == x.p.Name ? x.p.Name : $"{x.label}[{x.p.Name}]"));
                    list.Add((p.Name, label, props));
                }
        }
        catch { }
        return creatable = list;
    }

    /// <summary>AI に見せる「作れるアイテム」の一覧</summary>
    public static string CreatableItemsText()
    {
        var list = CreatableItems();
        if (list.Count == 0) return "";
        var sb = new StringBuilder("作れるアイテム(create の type に書く名前。テキストは text、図形は shape):\n");
        foreach (var (t, l, pr) in list) sb.AppendLine($"- {t}({l}) 項目: {pr}");
        return sb.ToString();
    }

    static string? ResolveItemType(string? want)
    {
        if (string.IsNullOrWhiteSpace(want)) return null;
        var w = want.Trim();
        var list = CreatableItems();
        var alias = w.ToLowerInvariant() switch { "group" or "groupcontrol" => "GroupItem", "effect" or "videoeffect" => "EffectItem", "transition" => "TransitionItem", _ => null };
        return list.FirstOrDefault(x => x.type.Equals(w, StringComparison.OrdinalIgnoreCase) || x.type.Equals(w + "Item", StringComparison.OrdinalIgnoreCase)).type
            ?? list.FirstOrDefault(x => x.label == w || w.Contains(x.label) || x.label.Contains(w)).type
            ?? (alias != null ? list.FirstOrDefault(x => x.type.Contains(alias.Replace("Item", ""), StringComparison.OrdinalIgnoreCase)).type : null);
    }

    /// <summary>作ったアイテムに、エフェクトを付ける(置く前)</summary>
    static void AttachEffects(object item, AiProposal p, List<string> notes)
    {
        if (p.Effects == null || p.Effects.Count == 0) return;
        foreach (var video in new[] { true, false })
        {
            var add = new List<object>();
            foreach (var (fname, prm) in p.Effects)
            {
                var def = FindEffect(fname, video);
                if (def == null) continue;
                var fx = Activator.CreateInstance(def.Type, true);
                if (fx == null) continue;
                if (prm != null)
                    foreach (var (k, v) in prm)
                    {
                        var fp = AiInspector.FindProp(fx, k);
                        if (fp == null || !AiInspector.Apply(fx, fp, v, out var e)) notes.Add($"{fname}.{k}: 設定できません");
                    }
                add.Add(fx);
            }
            if (add.Count == 0) continue;
            var ep = Ymm.FindProperty(item.GetType(), Ymm.EffectsProperty(video));
            if (ep == null) { notes.Add("このアイテムにはエフェクトを付けられません"); continue; }
            var merged = Ymm.MergeEffects(ep.PropertyType, null, AiInspector.Effects(item, video).Concat(add).ToList(), false);
            if (merged == null || !Ymm.SetValue(item, ep.Name, merged, out _)) notes.Add("エフェクトを付けられませんでした");
        }
        foreach (var (fname, _) in p.Effects)
            if (FindEffect(fname, true) == null && FindEffect(fname, false) == null) notes.Add($"「{fname}」というエフェクトが見つかりません");
    }

    static void ApplyProps(object item, JsonObject? props, List<string> notes)
    {
        if (props == null) return;
        foreach (var (k, v) in props)
        {
            var prop = AiInspector.FindProp(item, k);
            if (prop == null) { notes.Add($"{k}: 項目なし"); continue; }
            if (!AiInspector.Apply(item, prop, v, out var e)) notes.Add($"{k}: {e}");
        }
    }

    static async Task<(string result, Func<Task<string?>>? undo)> CreateAsync(AiProposal p)
    {
        await OpenSceneAsync(p);
        var tl = Ymm.Timeline() ?? throw new InvalidOperationException("YMM4のタイムラインが見つかりません");
        double fps = ProjectSnapshot.FpsOf(tl);
        int frame = p.Frame ?? (p.Seconds != null ? (int)Math.Round(p.Seconds.Value * fps) : Ymm.CurrentFrame());
        int layer = Math.Max(0, p.Layer ?? 0);
        int length = p.Length is > 0 ? p.Length.Value : (int)Math.Round(5 * fps);
        var notes = new List<string>();

        if (p.ItemType == "shape")
        {
            var want = (p.Shape ?? "").Trim();
            // 1) 図形タブに配置データがある図形は、それを使う(いちばん確実)
            var stored = StoredShapes();
            var entry = stored.FirstOrDefault(e => e.Name == want) ?? (want.Length > 0 ? stored.FirstOrDefault(e => e.Name.Contains(want, StringComparison.OrdinalIgnoreCase) || (e.Key.Length > 0 && e.Key.EndsWith(want, StringComparison.OrdinalIgnoreCase))) : null);
            var list = Shapes();
            var shape = list.FirstOrDefault(s => s.Name == want)
                        ?? (want.Length > 0 ? list.FirstOrDefault(s => s.Key.EndsWith(want, StringComparison.OrdinalIgnoreCase) || s.Name.Contains(want, StringComparison.OrdinalIgnoreCase)) : list.FirstOrDefault(s => s.IsBuiltIn));
            if (entry == null && shape != null) entry = stored.FirstOrDefault(e => e.Key == shape.Key);
            if (entry == null && shape == null) throw new InvalidOperationException($"「{want}」という図形が見つかりません");

            if (entry != null && Ymm.TemplateFromJson(entry.TemplateJson!) is { } stTemplate)
            {
                // 置いてから、長さ・項目・エフェクトを入れる
                var (text, undo, added) = await PlaceTemplateWithItemsAsync(tl, stTemplate, frame, layer);
                foreach (var it in added)
                    await Ymm.EditAsync(it, () => { Ymm.SetValue(it, "Length", length, out _); ApplyProps(it, p.Props, notes); AttachEffects(it, p, notes); });
                return ($"図形「{entry.Name}」を作りました。" + text + Notes(notes), undo);
            }
            // 2) 配置データが無い図形は、YMM4 の図形プラグインから初期値で作る
            var item = Ymm.CreateShapeItem(shape!.Key, frame, layer, length) ?? throw new InvalidOperationException(Ymm.LastError + "(図形タブで「標準を自動収録」してから試すと、作れることがあります)");
            ApplyProps(item, p.Props, notes);
            AttachEffects(item, p, notes);
            var template = Ymm.CreateTemplate(shape.Name, new[] { item }) ?? throw new InvalidOperationException(Ymm.LastError);
            var (t2, u2) = await PlaceTemplateAsync(tl, template, frame, layer);
            return ($"図形「{shape.Name}」を作りました。" + t2 + Notes(notes), u2);
        }

        object newItem;
        string name;
        if (p.ItemType is null or "text")
        {
            newItem = Ymm.CreateDefaultItem("TextItem", frame, layer, length) ?? throw new InvalidOperationException(Ymm.LastError);
            if (!Ymm.SetValue(newItem, "Text", p.Text ?? "", out var terr)) throw new InvalidOperationException("文字を入れられませんでした: " + terr);
            name = "テキスト";
        }
        else
        {
            // グループ制御・映像エフェクトなど
            var type = ResolveItemType(p.ItemType) ?? throw new InvalidOperationException($"「{p.ItemType}」というアイテムは作れません(作れるもの: {string.Join("・", CreatableItems().Select(x => $"{x.type}({x.label})"))})");
            newItem = Ymm.CreateDefaultItem(type, frame, layer, length) ?? throw new InvalidOperationException(Ymm.LastError);
            name = CreatableItems().FirstOrDefault(x => x.type == type).label ?? type;
        }
        ApplyProps(newItem, p.Props, notes);
        AttachEffects(newItem, p, notes);
        var tpl = Ymm.CreateTemplate(name, new[] { newItem }) ?? throw new InvalidOperationException(Ymm.LastError);
        var (t3, u3) = await PlaceTemplateAsync(tl, tpl, frame, layer);
        return (name + "を作りました。" + t3 + Notes(notes), u3);
    }

    static string Notes(List<string> notes) => notes.Count > 0 ? "(入れられなかった項目: " + string.Join(" / ", notes.Distinct()) + ")" : "";

    /// <summary>テンプレートを置いて、増えたアイテムも返す</summary>
    static async Task<(string text, Func<Task<string?>>? undo, List<object> added)> PlaceTemplateWithItemsAsync(object tl, object template, int frame, int layer)
    {
        var before = new HashSet<object>(ProjectSnapshot.ItemsOf(tl), ReferenceEqualityComparer.Instance);
        var (text, undo) = await PlaceTemplateAsync(tl, template, frame, layer);
        var added = ProjectSnapshot.ItemsOf(tl).Where(i => !before.Contains(i)).ToList();
        return (text, undo, added);
    }

    /// <summary>テンプレートを置いて、増えたアイテムを調べる(元に戻す時に取り除く)</summary>
    static async Task<(string text, Func<Task<string?>>? undo)> PlaceTemplateAsync(object tl, object template, int frame, int layer)
    {
        var before = new HashSet<object>(ProjectSnapshot.ItemsOf(tl), ReferenceEqualityComparer.Instance);
        var (ok, msg) = await Ymm.PlaceTemplateAtAsync(template, frame, layer);
        if (!ok) throw new InvalidOperationException(msg);
        await Task.Delay(150);
        var added = ProjectSnapshot.ItemsOf(tl).Where(i => !before.Contains(i)).ToList();
        foreach (var a in added) AiItemIds.IdOf(a);
        var text = $"L{layer:00} の {SecText(frame, ProjectSnapshot.FpsOf(tl))} に配置しました" + (added.Count > 0 ? $"(新しいアイテム {string.Join("、", added.Select(AiItemIds.IdOf))})" : "");
        Func<Task<string?>>? undo = added.Count == 0 ? null : () =>
            Task.FromResult(Ymm.RemoveItems(tl, added) ? null : Ymm.LastError + "(YMM4 の「元に戻す」(Ctrl+Z)で戻してください)");
        return (text, undo);
    }

    static async Task<(string result, Func<Task<string?>>? undo)> PlaceAsync(AiProposal p)
    {
        var src = AiLibrary.Find(p.Source) ?? throw new InvalidOperationException($"素材 {p.Source} が見つかりません");
        await OpenSceneAsync(p);
        var tl = Ymm.Timeline() ?? throw new InvalidOperationException("YMM4のタイムラインが見つかりません");
        double fps = ProjectSnapshot.FpsOf(tl);
        int frame = p.Frame ?? (p.Seconds != null ? (int)Math.Round(p.Seconds.Value * fps) : Ymm.CurrentFrame());
        int layer = Math.Max(0, p.Layer ?? 0);
        object? template;
        if (src.TemplateJson != null)
        {
            template = Ymm.TemplateFromJson(src.TemplateJson) ?? throw new InvalidOperationException(Ymm.LastError);
        }
        else
        {
            if (src.Path == null || !File.Exists(src.Path)) throw new InvalidOperationException("ファイルが見つかりません: " + src.Path);
            int length = p.Length ?? 0;
            if (length <= 0)
            {
                double sec = src.MediaKind is "video" or "audio" ? await DurationAsync(src.Path, src.MediaKind == "video") : 0;
                length = (int)Math.Round((sec > 0 ? sec : 5) * fps);
            }
            var newItem = Ymm.CreateFileItem(src.Path, src.MediaKind, frame, layer, length) ?? throw new InvalidOperationException(Ymm.LastError);
            template = Ymm.CreateTemplate(src.Name, new[] { newItem }) ?? throw new InvalidOperationException(Ymm.LastError);
        }
        return await PlaceTemplateAsync(tl, template, frame, layer);
    }
}

// ====================================================================== AI に見せる素材(素材データベース・テンプレート・プリセット・許可したフォルダー)
public static class AiLibrary
{
    public sealed record Source(string Id, string Name, string? Path, string? TemplateJson, string MediaKind)
    {
        public string KindText => TemplateJson != null ? "プリセット" : MediaKind switch { "image" => "画像", "video" => "動画", "audio" => "音声", _ => "ファイル" };
    }

    // 同じ素材には、会話の間ずっと同じ番号を使う
    static readonly Dictionary<string, Source> byId = new(StringComparer.OrdinalIgnoreCase);
    static readonly Dictionary<string, string> idByKey = new(StringComparer.OrdinalIgnoreCase);
    static int nextM = 1, nextF = 1, nextP = 1;

    public static Source? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return byId.TryGetValue(id.Trim().TrimStart('#'), out var s) ? s : null;
    }

    static Source Register(string prefix, string key, Func<string, Source> make)
    {
        if (idByKey.TryGetValue(prefix + key, out var id) && byId.TryGetValue(id, out var hit)) return hit;
        id = prefix + (prefix switch { "M" => nextM++, "P" => nextP++, _ => nextF++ });
        var s = make(id);
        idByKey[prefix + key] = id;
        byId[id] = s;
        return s;
    }

    public static string MediaKindOf(string path)
        => AiFiles.IsImage(path) ? "image" : AiMedia.IsVideo(path) ? "video" : AiMedia.IsAudio(path) ? "audio" : "other";

    static string Sec(double s) => s <= 0 ? "" : s >= 60 ? $" {(int)(s / 60)}分{s % 60:0}秒" : $" {s:0.#}秒";

    /// <summary>AI に見せる一覧(番号つき)。合計 maxChars 文字まで</summary>
    public static string Build(IEnumerable<string> allowedFolders, int maxChars = 40000, bool includeStores = true)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## 配置できる素材(op \"place\" の source に、この番号を書く)");
        bool Full() => sb.Length > maxChars;

        // YMM4専用ストレージの「素材」フォルダー(YMM4 マネージャーで登録したもの)は、いつも見せる
        var storageMaterials = StorageInfo.MaterialsDir;
        var storageRecordings = StorageInfo.RecordingsDir;
        var storageDirs = new[] { storageMaterials, storageRecordings }.Where(d => d != null).Select(d => d!).Concat(StorageInfo.OtherDirs).ToList();
        if (storageDirs.Count > 0)
            allowedFolders = storageDirs.Concat(allowedFolders.Where(f => !storageDirs.Any(d => string.Equals(Path.GetFullPath(f).TrimEnd('\\'), d.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))));
        if (!includeStores) goto Folders;

        // 素材データベース
        try
        {
            var mats = MaterialStore.Instance.All().Where(e => !e.IsDeleted && e.Kind is "image" or "video" or "audio").Take(400).ToList();
            if (mats.Count > 0)
            {
                sb.AppendLine("### 素材データベース");
                foreach (var e in mats)
                {
                    if (Full()) { sb.AppendLine("(多いため省略)"); break; }
                    var path = MaterialStore.Instance.FullPath(e);
                    var src = Register("M", e.Id, id => new Source(id, string.IsNullOrEmpty(e.Name) ? System.IO.Path.GetFileName(path) : e.Name, path, null, e.Kind));
                    var info = new List<string>();
                    if (e.Width > 0) info.Add($"{e.Width}×{e.Height}");
                    var d = Sec(e.DurationSec); if (d.Length > 0) info.Add(d.Trim());
                    if (e.Tags.Count > 0) info.Add("タグ: " + string.Join(",", e.Tags.Take(8)));
                    if (!string.IsNullOrEmpty(e.Folder)) info.Add("フォルダー: " + e.Folder);
                    if (e.Rating > 0) info.Add($"評価{e.Rating}");
                    if (!string.IsNullOrEmpty(e.Memo)) info.Add("メモ: " + (e.Memo.Length > 40 ? e.Memo[..40] + "…" : e.Memo));
                    sb.AppendLine($"{src.Id} {src.KindText}「{src.Name}」" + (info.Count > 0 ? " | " + string.Join(" | ", info) : ""));
                }
            }
        }
        catch { }

        // テンプレート(ファイル)
        try
        {
            var count = Math.Max(1, ExplorerSettings.Instance.TemplateTabCount);
            for (int slot = 1; slot <= Math.Min(count, 100) && !Full(); slot++)
            {
                var list = TemplateStore.Instance.All(slot).Where(t => File.Exists(t.Path) && MediaKindOf(t.Path) != "other").ToList();
                if (list.Count == 0) continue;
                sb.AppendLine($"### テンプレート{slot}");
                foreach (var t in list.Take(200))
                {
                    if (Full()) break;
                    var src = Register("F", t.Path, id => new Source(id, string.IsNullOrEmpty(t.Name) ? System.IO.Path.GetFileName(t.Path) : t.Name, t.Path, null, MediaKindOf(t.Path)));
                    sb.AppendLine($"{src.Id} {src.KindText}「{src.Name}」" + (string.IsNullOrEmpty(t.Memo) ? "" : " | メモ: " + t.Memo));
                }
            }
        }
        catch { }

        // プリセット(YMM4のアイテムテンプレートで保存したもの)
        try
        {
            var count = Math.Max(1, ExplorerSettings.Instance.PresetTabCount);
            for (int slot = 1; slot <= Math.Min(count, 100) && !Full(); slot++)
            {
                var list = PresetStore.Instance.All(slot).Where(x => !string.IsNullOrEmpty(x.TemplateJson)).ToList();
                if (list.Count == 0) continue;
                sb.AppendLine($"### プリセット{slot}(アイテムのまとまり。置くと中のアイテムがまとめて入る)");
                foreach (var x in list.Take(200))
                {
                    if (Full()) break;
                    var src = Register("P", x.Id, id => new Source(id, x.Name, null, x.TemplateJson, "preset"));
                    sb.AppendLine($"{src.Id} プリセット「{src.Name}」" + (string.IsNullOrEmpty(x.Memo) ? "" : " | メモ: " + x.Memo));
                }
            }
        }
        catch { }

        Folders:
        // 許可したフォルダー(と、専用ストレージの素材フォルダー)
        foreach (var folder in allowedFolders)
        {
            if (Full()) break;
            if (!Directory.Exists(folder)) { sb.AppendLine($"### 許可したフォルダー {folder}(見つかりません)"); continue; }
            sb.AppendLine(string.Equals(folder, storageMaterials, StringComparison.OrdinalIgnoreCase) ? $"### YMM4専用ストレージの素材フォルダー {folder}"
                        : string.Equals(folder, storageRecordings, StringComparison.OrdinalIgnoreCase) ? $"### YMM4専用ストレージの録画フォルダー(ゲームの録画など) {folder}"
                        : storageDirs.Contains(folder) ? $"### YMM4専用ストレージの {Path.GetFileName(folder)} フォルダー {folder}"
                        : $"### 許可したフォルダー {folder}");
            try
            {
                var files = Directory.EnumerateFiles(folder, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, MaxRecursionDepth = 5 })
                                     .Where(f => MediaKindOf(f) != "other").Take(500).OrderBy(f => f, NaturalComparer.Instance).ToList();
                foreach (var f in files)
                {
                    if (Full()) { sb.AppendLine("(多いため省略)"); break; }
                    var src = Register("F", f, id => new Source(id, System.IO.Path.GetFileName(f), f, null, MediaKindOf(f)));
                    sb.AppendLine($"{src.Id} {src.KindText} {System.IO.Path.GetRelativePath(folder, f)}");
                }
            }
            catch (Exception ex) { sb.AppendLine("(読めませんでした: " + ex.Message + ")"); }
        }
        return sb.ToString();
    }
}

// ====================================================================== YMM4 マネージャーの「YMM4専用ストレージ」
/// <summary>YMM4 マネージャーで登録した専用ストレージ(%AppData%\YMM4Manager\settings.json の StorageRoot)</summary>
public static class StorageInfo
{
    static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YMM4Manager", "settings.json");

    public static string? Root
    {
        get
        {
            try
            {
                if (!File.Exists(SettingsPath)) return null;
                var r = JsonNode.Parse(File.ReadAllText(SettingsPath))?["StorageRoot"]?.GetValue<string>();
                return !string.IsNullOrWhiteSpace(r) && Directory.Exists(r) ? r : null;
            }
            catch { return null; }
        }
    }

    /// <summary>素材・録画のほかの、専用ストレージのフォルダー(YMM4 本体・バックアップ・過去のバージョン・プリセット・AI会話は除く)</summary>
    public static List<string> OtherDirs
    {
        get
        {
            var r = Root;
            if (r == null) return new();
            var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "素材", "録画", "バックアップ", "YMM4の過去のバージョン", "YMM4プリセット", "AI会話", "AI記憶", "プロジェクト" };
            try
            {
                return Directory.GetDirectories(r).Where(d => !skip.Contains(Path.GetFileName(d)) && !File.Exists(Path.Combine(d, "YukkuriMovieMaker.exe")))
                                .Select(Path.GetFullPath).ToList();
            }
            catch { return new(); }
        }
    }

    /// <summary>専用ストレージの「録画」フォルダー(ゲームの録画など。無ければ null)</summary>
    public static string? RecordingsDir
    {
        get
        {
            var r = Root;
            if (r == null) return null;
            var d = Path.Combine(r, "録画");
            return Directory.Exists(d) ? Path.GetFullPath(d) : null;
        }
    }

    /// <summary>専用ストレージの「素材」フォルダー(無ければ null)</summary>
    public static string? MaterialsDir
    {
        get
        {
            var r = Root;
            if (r == null) return null;
            var d = Path.Combine(r, "素材");
            return Directory.Exists(d) ? Path.GetFullPath(d) : null;
        }
    }
}

// ====================================================================== 中間点(キーフレーム)
/// <summary>
/// AI の提案で、アイテムの中間点を足す・消す・動かす・値を変える。
/// YMM4 は中間点の位置をアイテム全体で1組(KeyFrames.Frames)持ち、動いている項目それぞれが「中間点の数 + 2」個の値を持つ、という前提
/// (アイテムタブの中間点の操作と同じ考え方)
/// </summary>
public static class AiKeyframes
{
    public static List<int>? ReadFrames(object item)
    {
        var kf = Ymm.Get(item, "KeyFrames");
        if (kf == null || Ymm.Get(kf, "Frames") is not IEnumerable e) return null;
        var list = new List<int>();
        try { foreach (var f in e) if (f != null) list.Add(Convert.ToInt32(f, CultureInfo.InvariantCulture)); }
        catch { return null; }
        return list;
    }

    static bool WriteFrames(object item, List<int> frames, out string err)
    {
        err = "";
        var kf = Ymm.Get(item, "KeyFrames");
        if (kf == null) { err = "このアイテムには中間点の仕組みが見つかりません"; return false; }
        try
        {
            var p = Ymm.FindProperty(kf.GetType(), "Frames");
            if (p == null) { err = "中間点の位置の一覧が見つかりません"; return false; }
            var now = p.GetValue(kf);
            if (now is IList il && !il.IsReadOnly && !il.IsFixedSize)
            {
                il.Clear();
                foreach (var f in frames) il.Add(f);
            }
            else
            {
                object? value = p.PropertyType.IsArray ? frames.ToArray()
                              : p.PropertyType.IsAssignableFrom(typeof(List<int>)) ? new List<int>(frames)
                              : Ymm.MergeEffects(p.PropertyType, null, frames.Cast<object>(), false);
                if (value == null) { err = "中間点の位置を作れませんでした: " + Ymm.LastError; return false; }
                if (!Ymm.SetValue(kf, "Frames", value, out err)) return false;
            }
            var count = Ymm.FindProperty(kf.GetType(), "Count");
            if (count != null && count.PropertyType == typeof(int) && count.GetSetMethod(true) != null) Ymm.SetValue(kf, "Count", frames.Count, out _);
            return true;
        }
        catch (Exception ex) { err = ex.GetBaseException().Message; return false; }
    }

    /// <summary>アイテムの中の、動かせる項目(Animation)をすべて集める(エフェクトや、図形の設定の中も)</summary>
    static void Collect(object? o, int depth, List<object> result, HashSet<object> seen)
    {
        if (o == null || depth > 4 || !seen.Add(o)) return;
        PropertyInfo[] props;
        try { props = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance); }
        catch { return; }
        foreach (var p in props)
        {
            try
            {
                if (p.GetIndexParameters().Length != 0 || !p.CanRead) continue;
                var pt = p.PropertyType;
                if (pt.IsValueType || pt == typeof(string) || p.Name == "KeyFrames") continue;
                var v = p.GetValue(o);
                if (v == null) continue;
                if (AiInspector.IsAnimation(v)) { if (seen.Add(v)) result.Add(v); continue; }
                if (v is IEnumerable e)
                {
                    if (p.Name is "VideoEffects" or "AudioEffects") foreach (var x in e) Collect(x, depth + 1, result, seen);
                    continue;
                }
                var ns = v.GetType().Namespace ?? "";
                if (ns.StartsWith("System", StringComparison.Ordinal) || ns.StartsWith("Microsoft", StringComparison.Ordinal)) continue;
                if (v.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(x => x.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>(true) != null))
                    Collect(v, depth + 1, result, seen);
            }
            catch { }
        }
    }

    static List<object> AllAnimations(object item)
    {
        var all = new List<object>();
        Collect(item, 0, all, new HashSet<object>(ReferenceEqualityComparer.Instance));
        return all;
    }

    /// <summary>「X」「fx0.Value」「afx1.Volume」の形の名前から、アニメーションを探す</summary>
    static object? AnimationOf(object item, string key, out string err)
    {
        err = "";
        object target = item;
        var name = key.Trim();
        var m = System.Text.RegularExpressions.Regex.Match(name, @"^(a?fx)(\d+)\.(.+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (m.Success)
        {
            var fx = AiInspector.Effects(item, !m.Groups[1].Value.StartsWith("a", StringComparison.OrdinalIgnoreCase)).ElementAtOrDefault(int.Parse(m.Groups[2].Value));
            if (fx == null) { err = $"「{key}」のエフェクトがありません"; return null; }
            target = fx;
            name = m.Groups[3].Value;
        }
        var prop = AiInspector.FindProp(target, name);
        if (prop == null) { err = $"「{key}」という項目がありません"; return null; }
        var anim = Ymm.Get(target, prop.Name);
        if (!AiInspector.IsAnimation(anim)) { err = $"「{key}」は動かせる項目ではありません"; return null; }
        return anim;
    }

    static double ValueAt(object anim, int i) { try { return Convert.ToDouble(Ymm.Get(AiInspector.AnimationValues(anim)[i], "Value"), CultureInfo.InvariantCulture); } catch { return 0; } }

    /// <summary>元に戻すための覚え書き(中間点の位置と、すべての動く項目)</summary>
    sealed record State(List<int>? Frames, List<(object anim, object? snap)> Anims);

    static State Take(object item)
    {
        var anims = new List<(object, object?)>();
        foreach (var a in AllAnimations(item))
            anims.Add((a, new AiInspector.AnimSnapshot(Ymm.Get(a, "AnimationType"), AiInspector.AnimationValues(a).Select((_, i) => ValueAt(a, i)).ToList())));
        return new State(ReadFrames(item), anims);
    }

    static string? Restore(object item, State st)
    {
        if (st.Frames != null && !WriteFrames(item, st.Frames, out var err)) return err;
        foreach (var (anim, snap) in st.Anims)
            if (snap is AiInspector.AnimSnapshot s) AiInspector.RestoreAnimation(anim, s, out _);
        return null;
    }

    public static async Task<(string result, Func<Task<string?>>? undo)> ExecuteAsync(AiProposal p)
    {
        var item = AiItemIds.Find(p.ItemId) ?? throw new InvalidOperationException($"アイテム {p.ItemId} が見つかりません");
        var before = Take(item);
        string result = "";
        string err = "";
        bool ok = false;
        await Ymm.EditAsync(item, () => ok = Run(item, p, out result, out err));
        if (!ok) { await Ymm.EditAsync(item, () => Restore(item, before)); throw new InvalidOperationException(err); }
        return (result, async () =>
        {
            string? e2 = null;
            await Ymm.EditAsync(item, () => e2 = Restore(item, before));
            return e2;
        });
    }

    static bool Run(object item, AiProposal p, out string result, out string err)
    {
        result = ""; err = "";
        var frames = ReadFrames(item);
        if (frames == null) { err = "このアイテムには中間点の仕組みが見つかりません"; return false; }
        int length = Convert.ToInt32(Ymm.Get(item, "Length") ?? 0, CultureInfo.InvariantCulture);
        var all = AllAnimations(item);
        // 中間点の数と値の数が合わない項目があれば、何もしない(壊さないため)
        if (all.Any(a => { var n = AiInspector.AnimationValues(a).Count; return n != 1 && n != frames.Count + 2; }))
        { err = "中間点の数と、項目の値の数が合わないものがあります(想定と違う形のため、変えませんでした)"; return false; }

        switch (p.Op)
        {
            case "addKeyframe":
            {
                if (length < 2) { err = "アイテムが短すぎて、中間点を入れられません"; return false; }
                int frame = Math.Clamp(p.Frame ?? length / 2, 1, length - 1);
                if (frames.Contains(frame)) { p = new AiProposal { Op = "setKeyframe", ItemId = p.ItemId, Index = frames.IndexOf(frame), KeyValues = p.KeyValues }; return Run(item, p, out result, out err); }
                int pos = frames.Count(f => f < frame);
                int b = pos == 0 ? 0 : frames[pos - 1], a = pos == frames.Count ? length : frames[pos];
                double t = a > b ? (frame - b) / (double)(a - b) : 0.5;
                // 値を指定された項目で、まだ動いていないものは、動くようにしておく(今の値のまま)
                var requested = new List<(object anim, double v)>();
                if (p.KeyValues != null)
                    foreach (var (k, v) in p.KeyValues)
                    {
                        var anim = AnimationOf(item, k, out err);
                        if (anim == null) return false;
                        if (!TryNum(v, out var d)) { err = $"「{k}」の値が数値ではありません"; return false; }
                        requested.Add((anim, d));
                    }
                var next = new List<int>(frames);
                next.Insert(pos, frame);
                if (!WriteFrames(item, next, out err)) return false;
                foreach (var anim in all)
                {
                    var n = AiInspector.AnimationValues(anim).Count;
                    bool want = requested.Any(r => ReferenceEquals(r.anim, anim));
                    if (n == frames.Count + 2 && n >= 2)
                    {
                        double v0 = ValueAt(anim, pos), v1 = ValueAt(anim, pos + 1);
                        if (!AiInspector.InsertPoint(anim, pos + 1, v0 + (v1 - v0) * t, out err)) return false;
                    }
                    else if (want)
                    {
                        // 動いていなかった項目: 今の値で、全部の点をそろえて作る
                        double v = ValueAt(anim, 0);
                        if (!AiInspector.SetPointCountAll(anim, next.Count + 2, v, out err)) return false;
                        AiInspector.EnsureMoving(anim);
                    }
                }
                foreach (var (anim, v) in requested)
                    if (!AiInspector.SetPoint(anim, pos + 1, v, out err)) return false;
                result = $"{frame} フレーム目(アイテムの始めから)に中間点を足しました(中間点 {next.Count} 個)";
                return true;
            }
            case "removeKeyframe":
            {
                int i = p.Index;
                if (i < 0 || i >= frames.Count) { err = $"{i + 1} つ目の中間点がありません(中間点は {frames.Count} 個)"; return false; }
                var next = new List<int>(frames);
                next.RemoveAt(i);
                if (!WriteFrames(item, next, out err)) return false;
                foreach (var anim in all)
                    if (AiInspector.AnimationValues(anim).Count == frames.Count + 2 && !AiInspector.RemovePoint(anim, i + 1, out err)) return false;
                result = $"{i + 1} つ目の中間点を消しました";
                return true;
            }
            case "moveKeyframe":
            {
                int i = p.Index;
                if (i < 0 || i >= frames.Count) { err = $"{i + 1} つ目の中間点がありません"; return false; }
                int low = i == 0 ? 1 : frames[i - 1] + 1, high = i == frames.Count - 1 ? length - 1 : frames[i + 1] - 1;
                if (high < low || p.Frame == null) { err = "動かせる場所がありません"; return false; }
                int frame = Math.Clamp(p.Frame.Value, low, high);
                var next = new List<int>(frames) { [i] = frame };
                if (!WriteFrames(item, next, out err)) return false;
                result = $"{i + 1} つ目の中間点を {frame} フレーム目へ動かしました" + (frame != p.Frame ? "(前後の中間点を越えないところまで)" : "");
                return true;
            }
            default: // setKeyframe
            {
                int i = p.Index;
                if (i < 0 || i >= frames.Count) { err = $"{i + 1} つ目の中間点がありません"; return false; }
                if (p.KeyValues == null || p.KeyValues.Count == 0) { err = "変える値がありません"; return false; }
                foreach (var (k, v) in p.KeyValues)
                {
                    var anim = AnimationOf(item, k, out err);
                    if (anim == null) return false;
                    if (!TryNum(v, out var d)) { err = $"「{k}」の値が数値ではありません"; return false; }
                    var n = AiInspector.AnimationValues(anim).Count;
                    if (n != frames.Count + 2)
                    {
                        if (!AiInspector.SetPointCountAll(anim, frames.Count + 2, ValueAt(anim, 0), out err)) return false;
                        AiInspector.EnsureMoving(anim);
                    }
                    if (!AiInspector.SetPoint(anim, i + 1, d, out err)) return false;
                }
                result = $"{i + 1} つ目の中間点の値を変えました";
                return true;
            }
        }
    }

    static bool TryNum(JsonNode? n, out double d)
    {
        d = 0;
        if (n is not JsonValue v) return false;
        if (v.TryGetValue<double>(out d)) return true;
        return v.TryGetValue<string>(out var s) && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out d);
    }
}
