using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>
/// YMM4本体の内部(MainViewModel・タイムライン・アイテム)に、リフレクションで直接アクセスする部品。
///
/// 別のテキストプリセットプラグインの解析で分かった、次の正式な手順を使います。
///  ・Application.Current.MainWindow.DataContext(MainViewModel) → フィールド "model"(MainModel) → プロパティ "Timeline"
///  ・Timeline の SelectedItems / SelectedItem / CurrentFrame / ID
///  ・アイテムの編集は BeginEdit() → 値を設定 → EndEditAsync()(元に戻す履歴にも載る)
///  ・エフェクトなどの保存・復元は YMM4自身の JSON(YukkuriMovieMaker.Json.Json の GetJsonText / LoadFromText)
///  ・配置は ItemTemplate を作り、MainModel.AddTemplateItemAsync(frame, layer, template)(テンプレートはNewtonsoftのTypeNameHandling.Autoで保存)
///
/// YMM4のバージョンで内部の名前が変わると動かなくなる可能性があるため、失敗しても例外にせず、理由を LastError に残します。
/// </summary>
internal static class Ymm
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    /// <summary>直近の失敗の理由(画面に出す用)</summary>
    public static string LastError { get; private set; } = "";

    static void Fail(string message) => LastError = message;

    // ------------------------------------------------------------------ 型・メンバーの検索
    public static Type? FindType(string fullName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(fullName, false);
                if (t != null) return t;
            }
            catch { }
        }
        try
        {
            foreach (var ctx in AssemblyLoadContext.All.ToList())
                foreach (var asm in ctx.Assemblies.ToList())
                {
                    try
                    {
                        var t = asm.GetType(fullName, false);
                        if (t != null) return t;
                    }
                    catch { }
                }
        }
        catch { }
        return null;
    }

    public static PropertyInfo? FindProperty(Type type, string name)
    {
        try
        {
            foreach (var p in type.GetProperties(All))
                if (p.Name == name && p.GetIndexParameters().Length == 0) return p;
        }
        catch { }
        return null;
    }

    static FieldInfo? FindField(Type type, string name)
    {
        for (var t = type; t != null; t = t.BaseType)
        {
            try
            {
                var f = t.GetField(name, All | BindingFlags.DeclaredOnly);
                if (f != null) return f;
            }
            catch { }
        }
        return null;
    }

    static MethodInfo? FindMethod(object target, string name, int paramCount)
    {
        var t = target.GetType();
        try
        {
            foreach (var m in t.GetMethods(All))
                if (m.Name == name && m.GetParameters().Length == paramCount) return m;
            foreach (var i in t.GetInterfaces())
                foreach (var m in i.GetMethods())
                    if (m.Name == name && m.GetParameters().Length == paramCount) return m;
        }
        catch { }
        return null;
    }

    /// <summary>プロパティ(なければフィールド)の値を取る。ReactiveProperty のような「Value を持つ入れ物」は中身を返す</summary>
    public static object? Get(object? target, string name)
    {
        if (target == null) return null;
        try
        {
            var t = target.GetType();
            object? v;
            var p = FindProperty(t, name);
            if (p != null && p.CanRead) v = p.GetValue(target);
            else
            {
                var f = FindField(t, name);
                if (f == null) return null;
                v = f.GetValue(target);
            }
            return Unwrap(v);
        }
        catch { return null; }
    }

    static object? Unwrap(object? v)
    {
        if (v == null) return null;
        var t = v.GetType();
        if (t.Name.Contains("ReactiveProperty", StringComparison.Ordinal))
        {
            var p = FindProperty(t, "Value");
            if (p != null) { try { return p.GetValue(v); } catch { } }
        }
        return v;
    }

    static int ToInt(object? v)
    {
        try { return v == null ? 0 : Convert.ToInt32(v); }
        catch { return 0; }
    }

    // ------------------------------------------------------------------ UIスレッドで実行
    static T? OnUi<T>(Func<T?> func)
    {
        var app = Application.Current;
        if (app == null) return default;
        try
        {
            if (app.Dispatcher.CheckAccess()) return func();
            return app.Dispatcher.Invoke(func);
        }
        catch (Exception ex)
        {
            Fail(ex.Message);
            return default;
        }
    }

    // ------------------------------------------------------------------ YMM4本体へのアクセス
    static bool LooksLikeMainViewModel(object? o)
        => o != null && (FindProperty(o.GetType(), "ActiveTimelineViewModel") != null || FindField(o.GetType(), "model") != null
                         && o.GetType().Name.Contains("MainViewModel", StringComparison.Ordinal));

    public static object? MainViewModel()
        => OnUi<object>(() =>
        {
            var app = Application.Current;
            var dc = app.MainWindow?.DataContext;
            if (LooksLikeMainViewModel(dc)) return dc;
            foreach (Window w in app.Windows)
            {
                var d = w.DataContext;
                if (LooksLikeMainViewModel(d)) return d;
            }
            return dc;
        });

    public static object? MainModel() => Get(MainViewModel(), "model");

    /// <summary>いま開いている YMM4 プロジェクトの名前(拡張子なし)。分からなければ ""</summary>
    public static string ProjectName()
    {
        try
        {
            var vm = MainViewModel();
            var model = Get(vm, "model");
            foreach (var o in new[] { model, vm, Get(model, "Project"), Get(vm, "Project") })
            {
                if (o == null) continue;
                foreach (var n in new[] { "ProjectFilePath", "FilePath", "ProjectPath", "Path", "FileName" })
                {
                    var v = Get(o, n);
                    var str = v as string ?? (v != null && v.GetType().Name.Contains("ReactiveProperty") ? Get(v, "Value") as string : null);
                    if (!string.IsNullOrWhiteSpace(str) && str.EndsWith(".ymmp", StringComparison.OrdinalIgnoreCase))
                        return System.IO.Path.GetFileNameWithoutExtension(str);
                }
            }
            // 予備: ウィンドウのタイトル(「プロジェクト名 - YukkuriMovieMaker」のような形)
            var title = OnUi<string>(() => Application.Current?.MainWindow?.Title ?? "") ?? "";
            var cut = title.IndexOf(" - ", StringComparison.Ordinal);
            var name = (cut > 0 ? title[..cut] : "").Trim().TrimEnd('*').Trim();
            if (name.EndsWith(".ymmp", StringComparison.OrdinalIgnoreCase)) name = System.IO.Path.GetFileNameWithoutExtension(name);
            return name.Contains("YukkuriMovieMaker", StringComparison.OrdinalIgnoreCase) ? "" : name;
        }
        catch { return ""; }
    }

    /// <summary>YMM4のタイムライン(モデル)。見つからなければ null(LastError に理由)</summary>
    public static object? Timeline()
    {
        var mainVm = MainViewModel();
        if (mainVm == null) { Fail("YMM4のメイン画面(MainViewModel)が見つかりませんでした"); return null; }
        var model = Get(mainVm, "model");
        var tl = Get(model, "Timeline");
        if (tl == null)
        {
            // 予備: アクティブなタイムラインのViewModelから
            tl = Get(Get(mainVm, "ActiveTimelineViewModel"), "Timeline");
        }
        if (tl == null) Fail("YMM4のタイムラインが見つかりませんでした");
        return tl;
    }

    /// <summary>タイムラインで選択中のアイテム(モデル)</summary>
    public static List<object> SelectedItems()
    {
        var list = new List<object>();
        var tl = Timeline();
        if (tl == null) return list;
        if (Get(tl, "SelectedItems") is IEnumerable e)
            foreach (var i in e) if (i != null) list.Add(i);
        if (list.Count == 0 && Get(tl, "SelectedItem") is { } one) list.Add(one);
        return list;
    }

    public static int CurrentFrame() => ToInt(Get(Timeline(), "CurrentFrame"));

    public static string TypeName(object item) => item.GetType().Name;
    public static bool IsTextItem(object item) => item.GetType().Name == "TextItem";
    public static bool IsVoiceItem(object item) => item.GetType().Name == "VoiceItem";

    // ------------------------------------------------------------------ 値のJSON化(YMM4自身のJSONを使う)
    static MethodInfo? JsonMethod(string name)
    {
        var jt = FindType("YukkuriMovieMaker.Json.Json");
        if (jt == null) return null;
        try { return jt.GetMethods(All).FirstOrDefault(m => m.Name == name && m.IsGenericMethodDefinition); }
        catch { return null; }
    }

    /// <summary>
    /// YMM4のJSON機能(YukkuriMovieMaker.Json.Json)の、name という名前のメソッドを呼ぶ。
    /// YMM4のバージョンによって引数の数が違うことがあるので、引数の少ないものから順に試し、足りない引数は既定値で埋める。
    /// </summary>
    static bool TryYmmJson(string name, Type t, object first, out object? result, out string error)
    {
        result = null;
        error = "";
        var jt = FindType("YukkuriMovieMaker.Json.Json");
        if (jt == null) { error = "YukkuriMovieMaker.Json.Json が見つかりませんでした"; return false; }
        List<MethodInfo> methods;
        try
        {
            methods = jt.GetMethods(All)
                .Where(m => m.Name == name && m.IsStatic && m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1)
                .OrderBy(m => m.GetParameters().Length).ToList();
        }
        catch (Exception ex) { error = ex.Message; return false; }
        if (methods.Count == 0) { error = $"Json.{name} が見つかりませんでした"; return false; }

        foreach (var def in methods)
        {
            try
            {
                var m = def.MakeGenericMethod(t);
                var ps = m.GetParameters();
                if (ps.Length == 0 || !ps[0].ParameterType.IsInstanceOfType(first)) continue;
                var args = new object?[ps.Length];
                args[0] = first;
                for (int i = 1; i < ps.Length; i++)
                    args[i] = ps[i].HasDefaultValue ? ps[i].DefaultValue
                            : ps[i].ParameterType.IsValueType ? Activator.CreateInstance(ps[i].ParameterType) : null;
                result = m.Invoke(null, args);
                if (result != null) return true;
                error = $"Json.{name} が何も返しませんでした";
            }
            catch (Exception ex) { error = ex.InnerException?.Message ?? ex.Message; }
        }
        if (error.Length == 0) error = $"Json.{name} に、合う形のものがありませんでした";
        return false;
    }

    /// <summary>YMM4のJSON機能が使えない時の代わり: Newtonsoft.Json で、型の名前つきのJSONにする(YMM4の保存形式と同じ考え方)</summary>
    static string? NewtonsoftSerialize(object value, Type type)
    {
        try
        {
            var nt = Newtonsoft();
            if (nt == null) return null;
            var m = nt.Value.convert.GetMethods().FirstOrDefault(x =>
                x.Name == "SerializeObject" && x.GetParameters().Length == 3
                && x.GetParameters()[0].ParameterType == typeof(object) && x.GetParameters()[1].ParameterType == typeof(Type)
                && x.GetParameters()[2].ParameterType == nt.Value.settings);
            return m?.Invoke(null, new[] { value, type, NewtonsoftSettings(nt.Value.settings) }) as string;
        }
        catch { return null; }
    }

    static object? NewtonsoftDeserialize(string json, Type type)
    {
        try
        {
            var nt = Newtonsoft();
            if (nt == null) return null;
            var m = nt.Value.convert.GetMethods().FirstOrDefault(x =>
                x.Name == "DeserializeObject" && x.GetParameters().Length == 3
                && x.GetParameters()[0].ParameterType == typeof(string) && x.GetParameters()[1].ParameterType == typeof(Type)
                && x.GetParameters()[2].ParameterType == nt.Value.settings);
            return m?.Invoke(null, new[] { json, type, NewtonsoftSettings(nt.Value.settings) });
        }
        catch { return null; }
    }

    static bool IsSimple(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal);
    }

    /// <summary>値をJSON文字列にする。基本の型(文字・数・真偽・列挙)と色は専用の形、それ以外はYMM4のJSON</summary>
    public static string? ValueToJson(object? value)
    {
        if (value == null) return null;
        try
        {
            var t = value.GetType();
            if (value is Color c) return JsonSerializer.Serialize(c.ToString());      // "#AARRGGBB"
            if (t.IsEnum) return JsonSerializer.Serialize(value.ToString());
            if (IsSimple(t)) return JsonSerializer.Serialize(value, t);

            if (TryYmmJson("GetJsonText", t, value, out var text, out var why) && text is string ok) return ok;
            var alt = NewtonsoftSerialize(value, t);
            if (alt != null) return alt;
            Fail("値をJSONにできませんでした: " + why);
            return null;
        }
        catch (Exception ex)
        {
            Fail("値をJSONにできませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    /// <summary>JSON文字列から、type 型の値を作る</summary>
    public static object? JsonToValue(Type type, string json)
    {
        try
        {
            var real = Nullable.GetUnderlyingType(type) ?? type;
            if (real == typeof(Color))
                return ColorConverter.ConvertFromString(JsonSerializer.Deserialize<string>(json));
            if (real.IsEnum)
            {
                var text = json.Trim();
                if (text.StartsWith('"')) return Enum.Parse(real, JsonSerializer.Deserialize<string>(text)!, true);
                return Enum.ToObject(real, JsonSerializer.Deserialize<long>(text));
            }
            if (IsSimple(real)) return JsonSerializer.Deserialize(json, type);

            if (TryYmmJson("LoadFromText", type, json, out var loaded, out var why) && loaded != null) return loaded;
            var alt = NewtonsoftDeserialize(json, type);
            if (alt != null) return alt;
            Fail("JSONから値を作れませんでした: " + why);
            return null;
        }
        catch (Exception ex)
        {
            Fail("JSONから値を作れませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    // ------------------------------------------------------------------ 値の設定
    /// <summary>
    /// 設定できる項目は、そのまま代入する。読み取り専用の項目(Animationなど)は、中身を CopyFrom か、
    /// 設定できるプロパティの値のコピーで書き換える。
    /// </summary>
    public static bool SetValue(object target, string propertyName, object? value, out string error)
    {
        error = "";
        try
        {
            var p = FindProperty(target.GetType(), propertyName);
            if (p == null) { error = "この項目がありません"; return false; }
            var setter = p.GetSetMethod(true);
            if (setter != null && !setter.IsStatic)
            {
                setter.Invoke(target, new[] { value });
                return true;
            }
            // 読み取り専用: いまの値の中身を書き換える
            var current = p.GetValue(target);
            if (current == null || value == null) { error = "書き込めない項目です"; return false; }
            CopyInto(current, value);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.InnerException?.Message ?? ex.Message;
            return false;
        }
    }

    static void CopyInto(object target, object source)
    {
        // CopyFrom(同じ型) があれば、それを使う
        var cf = target.GetType().GetMethods(All).FirstOrDefault(m =>
            m.Name == "CopyFrom" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.IsInstanceOfType(source));
        if (cf != null) { cf.Invoke(target, new[] { source }); return; }

        foreach (var p in target.GetType().GetProperties(All))
        {
            if (p.GetIndexParameters().Length != 0 || !p.CanRead) continue;
            object? sv;
            try { sv = p.GetValue(source); }
            catch { continue; }

            var setter = p.GetSetMethod(true);
            if (setter != null && !setter.IsStatic)
            {
                try { setter.Invoke(target, new[] { sv }); } catch { }
                continue;
            }
            // 読み取り専用の入れ物(Bezier・KeyFramesなど)は、さらに中身をコピー
            try
            {
                var tv = p.GetValue(target);
                if (tv != null && sv != null) CopyInto(tv, sv);
            }
            catch { }
        }
    }

    // ------------------------------------------------------------------ アイテムの編集(元に戻す履歴つき)
    /// <summary>BeginEdit → apply → EndEditAsync。BeginEdit/EndEditAsync が無いアイテムは、そのまま apply だけ行う</summary>
    public static async Task EditAsync(object item, Action apply)
    {
        var begin = FindMethod(item, "BeginEdit", 0);
        var end = FindMethod(item, "EndEditAsync", 0);
        begin?.Invoke(item, null);
        try { apply(); }
        finally
        {
            if (end != null)
            {
                var r = end.Invoke(item, null);
                if (r is Task t) await t;
            }
        }
    }

    /// <summary>編集の開始(「元に戻す」の区切りの始まり)。スライダーを動かしている間のように、続けて書き換える時に使う</summary>
    public static void BeginEdit(object item)
    {
        try { FindMethod(item, "BeginEdit", 0)?.Invoke(item, null); } catch { }
    }

    /// <summary>編集の終わり(ここまでが、「元に戻す」の1回分になる)</summary>
    public static async Task EndEditAsync(object item)
    {
        try
        {
            var r = FindMethod(item, "EndEditAsync", 0)?.Invoke(item, null);
            if (r is Task t) await t;
        }
        catch { }
    }

    // ------------------------------------------------------------------ テキストの書式(対象パラメータ)
    /// <summary>
    /// 「対象パラメータ」の1項目(画面のスイッチ1つ分)。
    /// Props はアイテムのプロパティ名で、別名は | で区切る(先に見つかったものを使う)。
    /// 名前が確実でない項目は Pattern(名前に含まれる文字)でも探す。
    /// </summary>
    public sealed record StyleGroup(string Id, string Label, string[] Props, bool DefaultOn = false, string? Pattern = null);

    /// <summary>対象パラメータの一覧(画面に出す順番)</summary>
    public static readonly StyleGroup[] StyleGroups =
    {
        new("Position", "描画位置", new[] { "X", "Y", "Z" }),
        new("Opacity", "不透明度", new[] { "Opacity" }),
        new("Zoom", "拡大率", new[] { "Zoom" }),
        new("Rotation", "回転角", new[] { "Rotation" }),
        new("Fade", "フェード", new[] { "FadeIn", "FadeOut" }),
        new("Blend", "合成モード", new[] { "Blend|BlendMode" }),
        new("Invert", "左右反転", new[] { "IsInverted|IsFlipped|IsMirrored" }),
        new("Clipping", "クリッピング", new[] { "IsClippingWithObjectAbove|IsClipping" }),
        new("OnTop", "手前に表示", new[] { "IsAlwaysOnTop|IsFront" }),
        new("ZOrder", "Z値順に表示", new[] { "IsZOrderEnabled|IsZOrder|ZOrderEnabled" }, Pattern: "ZOrder"),
        new("Font", "フォント", new[] { "Font" }, true),
        new("Size", "サイズ", new[] { "FontSize" }, true),
        new("LineHeight", "行の高さ", new[] { "LineHeight2|LineHeight" }, true),
        new("Spacing", "文字間隔", new[] { "LetterSpacing2|LetterSpacing" }, true),
        new("Wrap", "折り返し", new[] { "WordWrap", "MaxWidth" }),
        new("Align", "文字揃え", new[] { "TextAlignment|TextAlign|Alignment|HorizontalAlignment|TextHorizontalAlignment|BasePoint" }, true, "Align|BasePoint"),
        new("Color", "文字色", new[] { "FontColor" }, true),
        new("Decoration", "装飾", new[] { "Style", "StyleColor" }, true),
        new("Bold", "太字", new[] { "Bold" }, true),
        new("Italic", "イタリック", new[] { "Italic" }, true),
        new("TrimEnd", "行末スペース削除", new[] { "TrimEnd|TrimEndSpace|IsTrimEnd|TrimTrailingSpace|RemoveTrailingSpace" }, Pattern: "Trim"),
        new("Split", "文字ごとに分割", new[] { "IsDevidedPerCharacter|IsDividedPerCharacter|DevidedPerCharacter|DividedPerCharacter|SplitCharacters|IsSplitCharacters|SplitByCharacter|IsSplitByCharacter|SplitCharacter" }, Pattern: "PerCharacter|Devided|Divided|Split"),
        new("TextAnim", "テキストアニメーション", new[] { "DisplayInterval", "DisplayDirection", "HideInterval", "HideDirection" }),
        new("Effects", "映像エフェクト", new[] { "VideoEffects" }, true),
    };

    /// <summary>1つの対象パラメータについて、選択中のアイテムから読み取った値(項目名 → JSON)。空なら、このアイテムには無い項目</summary>
    public sealed class StyleGroupData
    {
        public StyleGroup Group { get; }
        public Dictionary<string, string> Values { get; } = new();
        public StyleGroupData(StyleGroup group) { Group = group; }
    }

    /// <summary>
    /// プロパティを探す。ボイスアイテムは、字幕用の「Jimaku」付きの名前を先に探す。
    /// 戻り値の Base は、保存に使う名前(Jimakuを付けない名前)
    /// </summary>
    static (string Base, PropertyInfo Prop)? ResolveProp(object item, string spec)
    {
        var type = item.GetType();
        foreach (var cand in spec.Split('|'))
        {
            if (IsVoiceItem(item))
            {
                var j = FindProperty(type, "Jimaku" + cand);
                if (j != null) return (cand, j);
            }
            var p = FindProperty(type, cand);
            if (p != null) return (cand, p);
        }
        return null;
    }

    /// <summary>名前に pattern を含む、設定できる単純な項目(真偽・列挙・数)を探す</summary>
    static (string Base, PropertyInfo Prop)? FindByPattern(object item, string pattern)
    {
        try
        {
            // pattern は「A|B」のように複数書ける(前にあるものから探す)
            foreach (var pat in pattern.Split('|', StringSplitOptions.RemoveEmptyEntries))
            foreach (var p in item.GetType().GetProperties(All))
            {
                if (p.GetIndexParameters().Length != 0 || !p.CanRead || p.GetSetMethod(true) == null) continue;
                if (p.Name.IndexOf(pat, StringComparison.OrdinalIgnoreCase) < 0) continue;
                var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                if (!(t == typeof(bool) || t.IsEnum || t == typeof(int) || t == typeof(double))) continue;
                var name = p.Name.StartsWith("Jimaku", StringComparison.Ordinal) ? p.Name.Substring(6) : p.Name;
                return (name, p);
            }
        }
        catch { }
        return null;
    }

    /// <summary>保存した項目名の、画面での名前(対象パラメータの名前。なければ項目名のまま)</summary>
    public static string LabelOfProp(string baseName)
    {
        foreach (var g in StyleGroups)
            foreach (var spec in g.Props)
                if (spec.Split('|').Contains(baseName)) return g.Label;
        return baseName;
    }

    /// <summary>テキスト/ボイスアイテムから、対象パラメータごとに値を読み取る(アイテムに無い項目は Values が空)</summary>
    public static List<StyleGroupData> ReadTextStyleGroups(object item)
    {
        var list = new List<StyleGroupData>();
        foreach (var g in StyleGroups)
        {
            var d = new StyleGroupData(g);
            foreach (var spec in g.Props)
            {
                var r = ResolveProp(item, spec);
                if (r == null) continue;
                var json = ValueToJson(Get(item, r.Value.Prop.Name));
                if (json != null) d.Values[r.Value.Base] = json;
            }
            if (d.Values.Count == 0 && g.Pattern != null)
            {
                var r = FindByPattern(item, g.Pattern);
                if (r != null)
                {
                    var json = ValueToJson(Get(item, r.Value.Prop.Name));
                    if (json != null) d.Values[r.Value.Base] = json;
                }
            }
            list.Add(d);
        }
        return list;
    }

    /// <summary>
    /// 保存した書式(項目名 → JSON)を、アイテムに適用する(失敗した項目の理由を返す)。
    /// ボイスアイテムは、字幕を「カスタム」にしてから設定する。
    /// </summary>
    public static async Task<(int Applied, List<string> Failed)> ApplyTextStyleAsync(
        object item, Dictionary<string, string> style, bool useEdit = true)
    {
        var failed = new List<string>();
        int applied = 0;
        void Body()
        {
            if (IsVoiceItem(item))
            {
                var vis = FindProperty(item.GetType(), "JimakuVisibility");
                if (vis != null && vis.GetSetMethod(true) != null && vis.PropertyType.IsEnum)
                {
                    try { vis.SetValue(item, Enum.Parse(vis.PropertyType, "Custom", true)); } catch { }
                }
            }
            foreach (var kv in style)
            {
                var label = LabelOfProp(kv.Key);
                var r = ResolveProp(item, kv.Key);
                if (r == null) { failed.Add($"{label}(この項目が無い)"); continue; }
                var value = JsonToValue(r.Value.Prop.PropertyType, kv.Value);
                if (value == null) { failed.Add($"{label}({LastError})"); continue; }
                if (SetValue(item, r.Value.Prop.Name, value, out var err)) applied++;
                else failed.Add($"{label}({err})");
            }
        }

        // タイムラインにあるアイテムは BeginEdit〜EndEditAsync で編集(元に戻す履歴に載る)。まだ置いていない新規アイテムは、そのまま設定する
        if (useEdit) await EditAsync(item, Body);
        else Body();
        return (applied, failed);
    }

    // ------------------------------------------------------------------ エフェクトのまとまり
    /// <summary>エフェクト一覧の項目名(映像: VideoEffects / 音声: AudioEffects)</summary>
    public static string EffectsProperty(bool video) => video ? "VideoEffects" : "AudioEffects";

    /// <summary>エフェクトの表示名(Label や Name、なければ型の名前)</summary>
    public static List<string> EffectNames(object? effects)
    {
        var names = new List<string>();
        if (effects is not IEnumerable e) return names;
        foreach (var x in e)
        {
            if (x == null) continue;
            var label = Get(x, "Label") as string ?? Get(x, "Name") as string;
            names.Add(string.IsNullOrWhiteSpace(label) ? x.GetType().Name : label);
        }
        return names;
    }

    public static int Count(object? seq)
    {
        int n = 0;
        if (seq is IEnumerable e) foreach (var _ in e) n++;
        return n;
    }

    /// <summary>ImmutableList&lt;T&gt; にして、existing の後ろに added を足す(append=falseなら added だけ)</summary>
    public static object? MergeEffects(Type listType, object? existing, object? added, bool append)
    {
        try
        {
            var elementType = listType.IsGenericType ? listType.GetGenericArguments()[0] : typeof(object);
            var items = new List<object>();
            if (append && existing is IEnumerable ex) foreach (var x in ex) if (x != null) items.Add(x);
            if (added is IEnumerable ad) foreach (var x in ad) if (x != null) items.Add(x);

            var array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < items.Count; i++) array.SetValue(items[i], i);

            var factory = listType.Assembly.GetType("System.Collections.Immutable.ImmutableList")
                          ?? typeof(System.Collections.Immutable.ImmutableList);
            var create = factory?.GetMethods().FirstOrDefault(m =>
                m.Name == "CreateRange" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
            if (create == null) { Fail("ImmutableList.CreateRange が見つかりませんでした"); return null; }
            return create.MakeGenericMethod(elementType).Invoke(null, new object[] { array });
        }
        catch (Exception ex)
        {
            Fail("エフェクトの一覧を作れませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    // ------------------------------------------------------------------ テンプレート(アイテムの保存と配置)
    static (Type convert, Type settings)? Newtonsoft()
    {
        var c = FindType("Newtonsoft.Json.JsonConvert");
        var s = FindType("Newtonsoft.Json.JsonSerializerSettings");
        return c != null && s != null ? (c, s) : null;
    }

    static object? NewtonsoftSettings(Type settingsType)
    {
        var s = Activator.CreateInstance(settingsType);
        var tnh = settingsType.GetProperty("TypeNameHandling");     // Auto = 4
        tnh?.SetValue(s, Enum.ToObject(tnh.PropertyType, 4));
        var fmt = settingsType.GetProperty("Formatting");           // Indented = 1
        fmt?.SetValue(s, Enum.ToObject(fmt.PropertyType, 1));
        return s;
    }

    /// <summary>選んだアイテムを、YMM4のアイテムテンプレート(ItemTemplate)にする</summary>
    public static object? CreateTemplate(string name, IReadOnlyList<object> items)
    {
        try
        {
            var tt = FindType("YukkuriMovieMaker.Settings.ItemTemplate");
            if (tt == null) { Fail("YMM4のアイテムテンプレート(ItemTemplate)が見つかりませんでした"); return null; }
            var tl = Timeline();
            if (tl == null) return null;
            var id = Get(tl, "ID");

            foreach (var ctor in tt.GetConstructors(All))
            {
                var ps = ctor.GetParameters();
                if (ps.Length != 5 || ps[1].ParameterType != typeof(string) || ps[4].ParameterType != typeof(Guid)) continue;
                if (!ps[3].ParameterType.IsInstanceOfType(tl)) continue;

                var group = ps[0].ParameterType.IsEnum ? Enum.ToObject(ps[0].ParameterType, 0) : null;
                var elementType = ps[2].ParameterType.IsGenericType ? ps[2].ParameterType.GetGenericArguments()[0] : typeof(object);
                var array = Array.CreateInstance(elementType, items.Count);
                for (int i = 0; i < items.Count; i++) array.SetValue(items[i], i);
                return ctor.Invoke(new[] { group, name, array, tl, id });
            }
            Fail("ItemTemplate のコンストラクターが見つかりませんでした(YMM4のバージョンが違う可能性があります)");
            return null;
        }
        catch (Exception ex)
        {
            Fail("テンプレートを作れませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    public static string? TemplateToJson(object template)
    {
        try
        {
            var nt = Newtonsoft();
            if (nt == null) { Fail("Newtonsoft.Json が見つかりませんでした"); return null; }
            var m = nt.Value.convert.GetMethods().FirstOrDefault(x =>
                x.Name == "SerializeObject" && x.GetParameters().Length == 2
                && x.GetParameters()[0].ParameterType == typeof(object) && x.GetParameters()[1].ParameterType == nt.Value.settings);
            if (m == null) { Fail("JsonConvert.SerializeObject が見つかりませんでした"); return null; }
            return m.Invoke(null, new[] { template, NewtonsoftSettings(nt.Value.settings) }) as string;
        }
        catch (Exception ex)
        {
            Fail("テンプレートをJSONにできませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    public static object? TemplateFromJson(string json)
    {
        try
        {
            var tt = FindType("YukkuriMovieMaker.Settings.ItemTemplate");
            var nt = Newtonsoft();
            if (tt == null || nt == null) { Fail("ItemTemplate または Newtonsoft.Json が見つかりませんでした"); return null; }
            var m = nt.Value.convert.GetMethods().FirstOrDefault(x =>
                x.Name == "DeserializeObject" && x.GetParameters().Length == 3
                && x.GetParameters()[0].ParameterType == typeof(string) && x.GetParameters()[1].ParameterType == typeof(Type)
                && x.GetParameters()[2].ParameterType == nt.Value.settings);
            if (m == null) { Fail("JsonConvert.DeserializeObject が見つかりませんでした"); return null; }
            return m.Invoke(null, new[] { json, tt, NewtonsoftSettings(nt.Value.settings) });
        }
        catch (Exception ex)
        {
            Fail("テンプレートを読み込めませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    static IDisposable? AcquireAddItemLock(object model)
    {
        try
        {
            var status = Get(model, "AsyncAwaitStatus");
            if (status == null) return null;
            var lockMethod = status.GetType().GetMethods().FirstOrDefault(m =>
                m.Name == "Lock" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(bool));
            return lockMethod?.Invoke(status, new object[] { true }) as IDisposable;
        }
        catch { return null; }
    }

    /// <summary>テンプレートのアイテムを、タイムラインのシークバー(現在のフレーム)の位置に配置する</summary>
    public static async Task<(bool Ok, string Message)> PlaceTemplateAsync(object template)
    {
        var model = MainModel();
        var tl = Timeline();
        if (model == null || tl == null) return (false, string.IsNullOrEmpty(LastError) ? "YMM4のタイムラインを取得できませんでした" : LastError);

        IDisposable? addLock = null;
        try
        {
            var frame = ToInt(Get(tl, "CurrentFrame"));
            var layer = Math.Max(ToInt(Get(template, "Layer")), ToInt(Get(Get(tl, "SelectedItem"), "Layer")));

            var add = model.GetType().GetMethods(All).FirstOrDefault(m =>
            {
                if (m.Name != "AddTemplateItemAsync") return false;
                var ps = m.GetParameters();
                return ps.Length == 3 && ps[0].ParameterType == typeof(int) && ps[1].ParameterType == typeof(int)
                       && ps[2].ParameterType.IsInstanceOfType(template);
            });
            if (add == null) return (false, "YMM4に AddTemplateItemAsync が見つかりませんでした(YMM4のバージョンが違う可能性があります)");

            addLock = AcquireAddItemLock(model);
            var r = add.Invoke(model, new object[] { frame, layer, template });
            if (r is Task task) await task;
            var count = Count(Get(template, "Items"));
            return (true, $"フレーム {frame}・レイヤー {layer} に {count} 個のアイテムを配置しました");
        }
        catch (Exception ex)
        {
            return (false, "配置できませんでした: " + (ex.InnerException?.Message ?? ex.Message));
        }
        finally { addLock?.Dispose(); }
    }

    /// <summary>テンプレートを、指定したフレーム・レイヤーへ配置する(テキスト分割用)</summary>
    public static async Task<(bool Ok, string Message)> PlaceTemplateAtAsync(object template, int frame, int layer)
    {
        var model = MainModel();
        if (model == null) return (false, string.IsNullOrEmpty(LastError) ? "YMM4のタイムラインを取得できませんでした" : LastError);
        IDisposable? addLock = null;
        try
        {
            var add = model.GetType().GetMethods(All).FirstOrDefault(m =>
            {
                if (m.Name != "AddTemplateItemAsync") return false;
                var ps = m.GetParameters();
                return ps.Length == 3 && ps[0].ParameterType == typeof(int) && ps[1].ParameterType == typeof(int)
                       && ps[2].ParameterType.IsInstanceOfType(template);
            });
            if (add == null) return (false, "YMM4に AddTemplateItemAsync が見つかりませんでした(YMM4のバージョンが違う可能性があります)");
            addLock = AcquireAddItemLock(model);
            var r = add.Invoke(model, new object[] { frame, layer, template });
            if (r is Task task) await task;
            return (true, "");
        }
        catch (Exception ex) { return (false, ex.InnerException?.Message ?? ex.Message); }
        finally { addLock?.Dispose(); }
    }

    /// <summary>アイテムを複製する(YMM4の GetClone を使う)</summary>
    public static object? CloneItem(object item)
    {
        try
        {
            var m = item.GetType().GetMethods(All).FirstOrDefault(x => x.Name == "GetClone" && x.GetParameters().Length == 0);
            if (m == null) { Fail("アイテムの複製機能(GetClone)が見つかりませんでした"); return null; }
            return m.Invoke(item, null);
        }
        catch (Exception ex) { Fail(ex.InnerException?.Message ?? ex.Message); return null; }
    }

    static object? ItemSettingsDefault()
    {
        var st = FindType("YukkuriMovieMaker.Settings.ItemSettings");
        return st?.GetProperty("Default", All)?.GetValue(null);
    }

    /// <summary>その種類のアイテムの「デフォルト(既定値)」として、YMM4が持っているアイテム</summary>
    public static object? DefaultItemFor(Type itemType)
    {
        try
        {
            var items = Get(ItemSettingsDefault(), "DefaultItems");
            if (items == null) { Fail("YMM4のデフォルトのアイテム(DefaultItems)が見つかりませんでした"); return null; }
            var props = items.GetType().GetProperties(All);
            var p = props.FirstOrDefault(x => x.PropertyType == itemType) ?? props.FirstOrDefault(x => x.Name == itemType.Name)
                    ?? props.FirstOrDefault(x => x.PropertyType.IsAssignableFrom(itemType) && x.PropertyType != typeof(object));
            if (p == null) { Fail("この種類のアイテムには、デフォルトがありません"); return null; }
            return p.GetValue(items);
        }
        catch (Exception ex) { Fail(ex.InnerException?.Message ?? ex.Message); return null; }
    }

    /// <summary>デフォルトの変更を、YMM4の設定として保存する(保存の機能が見つからなければ、YMM4の終了時に保存される)</summary>
    public static bool SaveItemSettings()
    {
        try
        {
            var def = ItemSettingsDefault();
            var save = def?.GetType().GetMethods(All).FirstOrDefault(m => m.Name == "Save" && m.GetParameters().Length == 0);
            if (def == null || save == null) return false;
            save.Invoke(def, null);
            return true;
        }
        catch { return false; }
    }

    // ------------------------------------------------------------------ 新しいテキストアイテム
    /// <summary>YMM4の「既定のテキストアイテム」を複製して、新しいテキストアイテムを作る</summary>
    public static object? CreateDefaultTextItem(string text, int frame, int layer, int length)
    {
        try
        {
            var st = FindType("YukkuriMovieMaker.Settings.ItemSettings");
            if (st == null) { Fail("YMM4のアイテム設定(ItemSettings)が見つかりませんでした"); return null; }
            var def = st.GetProperty("Default", All)?.GetValue(null);
            var textItem = Get(Get(def, "DefaultItems"), "TextItem");
            var clone = textItem?.GetType().GetMethods(All)
                .FirstOrDefault(m => m.Name == "GetClone" && m.GetParameters().Length == 0)?.Invoke(textItem, null);
            if (clone == null) { Fail("既定のテキストアイテムを複製できませんでした"); return null; }

            SetValue(clone, "Text", text, out _);
            SetValue(clone, "Frame", frame, out _);
            SetValue(clone, "Layer", layer, out _);
            SetValue(clone, "Length", length, out _);
            return clone;
        }
        catch (Exception ex)
        {
            Fail("テキストアイテムを作れませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    // ------------------------------------------------------------------ 診断
    /// <summary>YMM4の内部へのアクセスができるかを調べて、結果を文章にする(うまく動かない時の確認用)</summary>
    public static string Diagnose()
    {
        var lines = new List<string>();
        void Row(bool ok, string text) => lines.Add((ok ? "○ " : "× ") + text);

        var main = MainViewModel();
        Row(main != null, "メイン画面(MainViewModel): " + (main?.GetType().FullName ?? "見つかりません"));
        var model = Get(main, "model");
        Row(model != null, "MainModel(フィールド model): " + (model?.GetType().Name ?? "見つかりません"));
        var tl = Timeline();
        Row(tl != null, "タイムライン: " + (tl?.GetType().FullName ?? LastError));
        if (tl != null)
        {
            var items = SelectedItems();
            Row(true, $"現在のフレーム: {CurrentFrame()} / 選択中のアイテム: {items.Count} 個"
                      + (items.Count > 0 ? "(" + string.Join("、", items.Take(5).Select(TypeName)) + ")" : ""));
            if (items.Count > 0)
            {
                var first = items[0];
                var isText = IsTextItem(first) || IsVoiceItem(first);
                Row(FindMethod(first, "BeginEdit", 0) != null && FindMethod(first, "EndEditAsync", 0) != null,
                    $"編集の開始・終了(BeginEdit / EndEditAsync): 先頭のアイテム {TypeName(first)}");
                if (isText)
                {
                    var groups = ReadTextStyleGroups(first);
                    var found = groups.Count(g => g.Values.Count > 0);
                    Row(found > 0, $"テキストの対象パラメータ: {found} / {groups.Count} 項目が見つかりました"
                                   + (found < groups.Count ? "(無い項目: " + string.Join("、", groups.Where(g => g.Values.Count == 0).Select(g => g.Group.Label)) + ")" : ""));
                }
                Row(FindProperty(first.GetType(), "VideoEffects") != null, "映像エフェクトの項目(VideoEffects)");
                Row(FindProperty(first.GetType(), "AudioEffects") != null, "音声エフェクトの項目(AudioEffects)");
            }
        }
        Row(JsonMethod("GetJsonText") != null && JsonMethod("LoadFromText") != null, "YMM4のJSON機能(Json.GetJsonText / LoadFromText)");
        Row(FindType("YukkuriMovieMaker.Settings.ItemTemplate") != null, "アイテムテンプレート(ItemTemplate)");
        Row(Newtonsoft() != null, "Newtonsoft.Json");
        Row(model != null && model.GetType().GetMethods(All).Any(m => m.Name == "AddTemplateItemAsync"), "テンプレートの配置(AddTemplateItemAsync)");
        Row(FindType("YukkuriMovieMaker.Settings.ItemSettings") != null, "既定のアイテム設定(ItemSettings)");
        var preview = OnUi<object>(PreviewViewModel);
        Row(Player(preview) != null, "プレビュー画面(画像の取り込み): " + (preview?.GetType().Name ?? "見つかりません"));
        return string.Join("\n", lines);
    }

    // ------------------------------------------------------------------ 標準の図形アイテム
    /// <summary>
    /// 図形のプラグイン(IShapePlugin)から、初期値の図形アイテムを作る(実験的)。
    /// 「既定の図形アイテム」を複製(なければ新規作成)して、図形のパラメータを入れる
    /// </summary>
    /// <summary>
    /// ファイル(画像・動画・音声)のアイテムを作る。YMM4の「アイテムの初期値」を複製して、ファイルの場所・位置・長さを入れる。
    /// kind は image / video / audio
    /// </summary>
    public static object? CreateFileItem(string path, string kind, int frame, int layer, int length)
    {
        try
        {
            var typeName = kind switch { "video" => "VideoItem", "audio" => "AudioItem", _ => "ImageItem" };
            object? item = null;
            var settingsType = FindType("YukkuriMovieMaker.Settings.ItemSettings");
            var def = settingsType?.GetProperty("Default", All)?.GetValue(null);
            var defaultItem = Get(Get(def, "DefaultItems"), typeName);
            if (defaultItem != null)
                item = defaultItem.GetType().GetMethods(All).FirstOrDefault(m => m.Name == "GetClone" && m.GetParameters().Length == 0)?.Invoke(defaultItem, null);
            var itemType = item?.GetType() ?? FindType("YukkuriMovieMaker.Project.Items." + typeName);
            if (item == null && itemType != null)
            {
                // ファイルの場所を受け取るコンストラクターがあれば、それを使う
                var withPath = itemType.GetConstructors(All).FirstOrDefault(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(string));
                item = withPath != null ? withPath.Invoke(new object[] { path }) : Activator.CreateInstance(itemType, true);
            }
            if (item == null) { Fail($"{typeName} を作れませんでした"); return null; }
            if (Get(item, "FilePath") as string != path && !SetValue(item, "FilePath", path, out var err))
            { Fail("ファイルの場所を入れられませんでした: " + err); return null; }
            SetValue(item, "Frame", frame, out _);
            SetValue(item, "Layer", layer, out _);
            SetValue(item, "Length", Math.Max(1, length), out _);
            return item;
        }
        catch (Exception ex)
        {
            Fail("アイテムを作れませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    /// <summary>YMM4の「アイテムの初期値」を複製して、新しいアイテムを作る(typeName は TextItem など)</summary>
    public static object? CreateDefaultItem(string typeName, int frame, int layer, int length)
    {
        try
        {
            object? item = null;
            var settingsType = FindType("YukkuriMovieMaker.Settings.ItemSettings");
            var def = settingsType?.GetProperty("Default", All)?.GetValue(null);
            var defaultItem = Get(Get(def, "DefaultItems"), typeName);
            if (defaultItem != null)
                item = defaultItem.GetType().GetMethods(All).FirstOrDefault(m => m.Name == "GetClone" && m.GetParameters().Length == 0)?.Invoke(defaultItem, null);
            if (item == null)
            {
                var t = FindType("YukkuriMovieMaker.Project.Items." + typeName);
                if (t != null) item = Activator.CreateInstance(t, true);
            }
            if (item == null) { Fail($"{typeName} を作れませんでした"); return null; }
            SetValue(item, "Frame", frame, out _);
            SetValue(item, "Layer", layer, out _);
            SetValue(item, "Length", Math.Max(1, length), out _);
            return item;
        }
        catch (Exception ex)
        {
            Fail("アイテムを作れませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    /// <summary>タイムラインからアイテムを取り除く(取り除く仕組みが見つからなければ false)</summary>
    public static bool RemoveItems(object timeline, IReadOnlyList<object> items)
    {
        if (items.Count == 0) return true;
        try
        {
            foreach (var m in timeline.GetType().GetMethods(All).Where(m => m.Name is "RemoveItems" or "DeleteItems" or "RemoveItem" or "DeleteItem"))
            {
                var ps = m.GetParameters();
                if (ps.Length < 1 || ps.Skip(1).Any(p => !p.HasDefaultValue)) continue;
                var rest = ps.Skip(1).Select(p => p.DefaultValue).ToArray();
                var pt = ps[0].ParameterType;
                if (pt.IsInstanceOfType(items[0]))
                {
                    foreach (var it in items) m.Invoke(timeline, new[] { it }.Concat(rest).ToArray());
                    return true;
                }
                var elem = pt.IsArray ? pt.GetElementType() : pt.IsGenericType ? pt.GetGenericArguments()[0] : null;
                if (elem == null || !elem.IsInstanceOfType(items[0])) continue;
                var arr = Array.CreateInstance(elem, items.Count);
                for (int i = 0; i < items.Count; i++) arr.SetValue(items[i], i);
                object arg = arr;
                if (!pt.IsInstanceOfType(arr))
                {
                    var listType = typeof(List<>).MakeGenericType(elem);
                    var list = Activator.CreateInstance(listType, arr)!;
                    if (!pt.IsInstanceOfType(list)) continue;
                    arg = list;
                }
                m.Invoke(timeline, new[] { arg }.Concat(rest).ToArray());
                return true;
            }
            // Items が書き換えられる一覧なら、そこから外す
            if (Get(timeline, "Items") is System.Collections.IList il && !il.IsReadOnly)
            {
                foreach (var it in items) il.Remove(it);
                return true;
            }
        }
        catch (Exception ex) { Fail("アイテムを取り除けませんでした: " + (ex.InnerException?.Message ?? ex.Message)); return false; }
        Fail("アイテムを取り除く仕組みが見つかりませんでした");
        return false;
    }

    public static object? CreateShapeItem(string pluginTypeName, int frame, int layer, int length)
    {
        try
        {
            var pluginType = FindType(pluginTypeName);
            if (pluginType == null) { Fail("図形のプラグインの型が見つかりませんでした"); return null; }
            var plugin = Activator.CreateInstance(pluginType);
            var create = pluginType.GetMethods(All).FirstOrDefault(m => m.Name == "CreateShapeParameter");
            if (create == null) { Fail("CreateShapeParameter が見つかりませんでした"); return null; }

            var args = create.GetParameters()
                .Select(p => p.HasDefaultValue ? p.DefaultValue : (p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null))
                .ToArray();
            var parameter = create.Invoke(plugin, args);
            if (parameter == null) { Fail("図形のパラメータを作れませんでした"); return null; }

            object? item = null;
            var settingsType = FindType("YukkuriMovieMaker.Settings.ItemSettings");
            var def = settingsType?.GetProperty("Default", All)?.GetValue(null);
            var defaultShape = Get(Get(def, "DefaultItems"), "ShapeItem");
            if (defaultShape != null)
                item = defaultShape.GetType().GetMethods(All).FirstOrDefault(m => m.Name == "GetClone" && m.GetParameters().Length == 0)?.Invoke(defaultShape, null);
            if (item == null)
            {
                var itemType = FindType("YukkuriMovieMaker.Project.Items.ShapeItem");
                if (itemType != null) item = Activator.CreateInstance(itemType);
            }
            if (item == null) { Fail("図形アイテム(ShapeItem)を作れませんでした"); return null; }

            // 図形の種類を「型」で持っている項目(ShapeType など)があれば、先に入れておく(後から入れるとパラメータが作り直されるため)
            foreach (var tp in item.GetType().GetProperties(All).Where(p => p.PropertyType == typeof(Type) && p.Name.Contains("Shape", StringComparison.Ordinal) && p.GetSetMethod(true) != null))
                try { tp.GetSetMethod(true)!.Invoke(item, new object[] { pluginType }); } catch { }
            var target = item.GetType().GetProperties(All)
                .Where(p => p.GetSetMethod(true) != null && p.PropertyType != typeof(object) && p.PropertyType.IsInstanceOfType(parameter))
                .OrderByDescending(p => p.Name.Contains("Shape", StringComparison.Ordinal)).FirstOrDefault();
            if (target == null) { Fail("図形アイテムに、図形のパラメータを入れる項目が見つかりませんでした"); return null; }
            target.GetSetMethod(true)!.Invoke(item, new[] { parameter });

            SetValue(item, "Frame", frame, out _);
            SetValue(item, "Layer", layer, out _);
            SetValue(item, "Length", length, out _);
            return item;
        }
        catch (Exception ex)
        {
            Fail("図形を作れませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
    }

    // ------------------------------------------------------------------ プレビュー画像の取り込み
    /// <summary>プレビューの再生部分(画像保存 SaveImage を持つもの)。名前が player でなくても、型で探す</summary>
    static object? Player(object? previewViewModel)
    {
        if (previewViewModel == null) return null;
        try
        {
            if (HasSaveImage(previewViewModel.GetType())) return previewViewModel;
            for (var t = previewViewModel.GetType(); t != null && t != typeof(object); t = t.BaseType)
            {
                foreach (var f in t.GetFields(All | BindingFlags.DeclaredOnly))
                {
                    if (f.FieldType.IsValueType || f.FieldType == typeof(string)) continue;
                    if (!f.Name.Contains("player", StringComparison.OrdinalIgnoreCase) && !HasSaveImage(f.FieldType)) continue;
                    object? v;
                    try { v = f.GetValue(previewViewModel); } catch { continue; }
                    if (v != null && HasSaveImage(v.GetType())) return v;
                }
                foreach (var p in t.GetProperties(All | BindingFlags.DeclaredOnly))
                {
                    if (p.GetIndexParameters().Length != 0 || p.PropertyType.IsValueType || p.PropertyType == typeof(string)) continue;
                    if (!p.Name.Contains("Player", StringComparison.OrdinalIgnoreCase) && !HasSaveImage(p.PropertyType)) continue;
                    object? v;
                    try { v = p.GetValue(previewViewModel); } catch { continue; }
                    if (v != null && HasSaveImage(v.GetType())) return v;
                }
            }
        }
        catch { }
        return null;
    }

    static bool HasSaveImage(Type t)
    {
        try { return t.GetMethods(All).Any(m => m.Name == "SaveImage"); }
        catch { return false; }
    }

    static object? cachedPreviewVm;

    /// <summary>YMM4のプレビュー画面(の ViewModel)。名前で探し、無ければ画面の部品や、メイン画面の中を広く探す</summary>
    static object? PreviewViewModel()
    {
        if (cachedPreviewVm != null && Player(cachedPreviewVm) != null) return cachedPreviewVm;
        var main = MainViewModel();
        if (main == null) return null;
        foreach (var name in new[] { "PreviewViewModel", "Preview", "ActivePreviewViewModel", "ScenePreviewViewModel" })
        {
            var v = Get(main, name);
            if (Player(v) != null) return cachedPreviewVm = v;
        }
        // ドッキングされた画面の一覧から、プレビューを探す
        if (Get(main, "AnchorableAreaViewModels") is IEnumerable areas)
            foreach (var a in areas)
            {
                if (a == null) continue;
                var content = Get(a, "Content") ?? a;
                foreach (var c in new[] { a, content })
                    if (c.GetType().Name.Contains("Preview", StringComparison.Ordinal) && Player(c) != null) return cachedPreviewVm = c;
            }
        // 画面の部品の DataContext から探す(プレビューのパネルの中にある)
        try
        {
            int budget = 30000;
            foreach (Window w in Application.Current.Windows)
            {
                var found = FindInVisualTree(w, ref budget);
                if (found != null) return cachedPreviewVm = found;
            }
        }
        catch { }
        // メイン画面の中を広く探す
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var queue = new Queue<(object o, int d)>();
        queue.Enqueue((main, 0));
        int visited = 0;
        while (queue.Count > 0 && visited < 3000)
        {
            var (o, d) = queue.Dequeue();
            if (!seen.Add(o)) continue;
            visited++;
            if (o.GetType().Name.Contains("Preview", StringComparison.Ordinal) && Player(o) != null) return cachedPreviewVm = o;
            if (d >= 4) continue;
            foreach (var p in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.GetIndexParameters().Length != 0 || p.PropertyType.IsValueType || p.PropertyType == typeof(string)) continue;
                object? v;
                try { v = p.GetValue(o); } catch { continue; }
                v = Unwrap(v);
                if (v == null) continue;
                if (v is IEnumerable e && v is not string)
                {
                    try { foreach (var x in e.Cast<object?>().Take(50)) if (x != null && (x.GetType().Namespace ?? "").StartsWith("YukkuriMovieMaker", StringComparison.Ordinal)) queue.Enqueue((x, d + 1)); }
                    catch { }
                    continue;
                }
                if ((v.GetType().Namespace ?? "").StartsWith("YukkuriMovieMaker", StringComparison.Ordinal)) queue.Enqueue((v, d + 1));
            }
        }
        return null;
    }

    static object? FindInVisualTree(DependencyObject root, ref int budget)
    {
        if (--budget < 0) return null;
        if (root is FrameworkElement fe && fe.DataContext is { } dc && dc.GetType().Name.Contains("Preview", StringComparison.Ordinal) && Player(dc) != null)
            return dc;
        int n;
        try { n = VisualTreeHelper.GetChildrenCount(root); } catch { return null; }
        for (int i = 0; i < n; i++)
        {
            var r = FindInVisualTree(VisualTreeHelper.GetChild(root, i), ref budget);
            if (r != null) return r;
        }
        return null;
    }

    /// <summary>予備: プレビューの画面の部品を、そのまま画像にする(SaveImage が使えない時)</summary>
    static BitmapSource? CapturePreviewVisual()
    {
        FrameworkElement? best = null;
        double bestArea = 0;
        int budget = 30000;
        void Walk(DependencyObject d)
        {
            if (--budget < 0) return;
            if (d is ExplorerView) return;      // このプラグイン自身の画面は除く
            if (d is FrameworkElement fe && fe.IsVisible && fe.ActualWidth > 80 && fe.ActualHeight > 45)
            {
                var name = fe.GetType().Name + "|" + (fe.DataContext?.GetType().Name ?? "") + "|" + fe.Name;
                bool previewish = name.Contains("Preview", StringComparison.Ordinal);
                bool drawsImage = fe is System.Windows.Controls.Image img && img.Source != null;
                if (previewish && drawsImage)
                {
                    var area = fe.ActualWidth * fe.ActualHeight;
                    if (area > bestArea) { bestArea = area; best = fe; }
                }
            }
            int n;
            try { n = VisualTreeHelper.GetChildrenCount(d); } catch { return; }
            for (int i = 0; i < n; i++) Walk(VisualTreeHelper.GetChild(d, i));
        }
        foreach (Window w in Application.Current.Windows) Walk(w);
        if (best == null) return null;
        // 画像の部品なら、元の画像そのもの(D3DImage などは、見えている大きさで描き写す)
        if (best is System.Windows.Controls.Image im && im.Source is BitmapSource bs) { if (!bs.IsFrozen && bs.CanFreeze) bs = bs.Clone(); return bs; }
        var rtb = new RenderTargetBitmap((int)Math.Ceiling(best.ActualWidth), (int)Math.Ceiling(best.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        rtb.Render(best);
        rtb.Freeze();
        return rtb;
    }

    /// <summary>いまYMM4のプレビューに映っている画像を取り込む(サムネイル用)</summary>
    public static BitmapSource? CapturePreview()
    {
        string? path = null;
        try
        {
            var vm = OnUi<object>(PreviewViewModel);
            var player = Player(vm);
            var save = player?.GetType().GetMethods(All).FirstOrDefault(m => m.Name == "SaveImage");
            if (player == null || save == null)
            {
                // 予備: 画面に映っているプレビューを、そのまま画像にする
                var shot = OnUi<BitmapSource>(CapturePreviewVisual);
                if (shot != null) return shot;
                Fail("YMM4のプレビュー画面が見つかりませんでした(プレビューを表示した状態で、もう一度お試しください)");
                return null;
            }

            path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
            var ps = save.GetParameters();
            var args = ps.Length >= 2 ? new object[] { path, false } : new object[] { path };
            OnUi<object>(() => { save.Invoke(player, args); return null; });
            if (!File.Exists(path))
            {
                // 保存が後から行われる版のYMM4: 少し待つ。だめなら、画面に映っているプレビューを画像にする
                for (int i = 0; i < 15 && !File.Exists(path); i++) System.Threading.Thread.Sleep(20);
                if (!File.Exists(path))
                {
                    var shot = OnUi<BitmapSource>(CapturePreviewVisual);
                    if (shot != null) return shot;
                    Fail("プレビュー画像を保存できませんでした");
                    return null;
                }
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch (Exception ex)
        {
            Fail("プレビューを取り込めませんでした: " + (ex.InnerException?.Message ?? ex.Message));
            return null;
        }
        finally
        {
            try { if (path != null && File.Exists(path)) File.Delete(path); } catch { }
        }
    }
}
