using System.Collections.Immutable;
using System.Reflection;

namespace YmmExplorer;

/// <summary>YMM4に読み込まれているエフェクト(標準・プラグイン)1つ分</summary>
public sealed record BuiltinEffect(string Name, Type Type, object Instance, bool IsBuiltIn = true, string AssemblyName = "");

/// <summary>YMM4に読み込まれている映像・音声エフェクトを探す(自動収録用。標準もプラグインも対象)</summary>
public static class BuiltinCatalog
{
    internal static IEnumerable<Assembly> AllAssemblies()
    {
        var seen = new HashSet<Assembly>();
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            if (seen.Add(a)) yield return a;
        List<System.Runtime.Loader.AssemblyLoadContext> contexts;
        try { contexts = System.Runtime.Loader.AssemblyLoadContext.All.ToList(); }
        catch { yield break; }
        foreach (var c in contexts)
        {
            List<Assembly> list;
            try { list = c.Assemblies.ToList(); }
            catch { continue; }
            foreach (var a in list)
                if (seen.Add(a)) yield return a;
        }
    }

    static bool IsBuiltInAssembly(Assembly asm)
    {
        try
        {
            var name = asm.GetName().Name ?? "";
            if (!name.StartsWith("YukkuriMovieMaker", StringComparison.OrdinalIgnoreCase)) return false;
            var location = "";
            try { location = asm.Location ?? ""; } catch { }
            return !location.Replace('/', '\\').Contains(@"\user\plugin\", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    internal static IEnumerable<Type> TypesOf(Assembly asm)
    {
        try { return asm.GetTypes(); }
        catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null).Select(t => t!); }
        catch { return Array.Empty<Type>(); }
    }

    static string InterfaceName(bool video) => video ? "IVideoEffect" : "IAudioEffect";

    /// <summary>調べなくてよいアセンブリ(.NET本体など)。全部調べると時間がかかるため</summary>
    internal static bool IsSystemAssembly(string name) =>
        name.StartsWith("System", StringComparison.Ordinal) || name.StartsWith("Microsoft", StringComparison.Ordinal)
        || name.StartsWith("Windows", StringComparison.Ordinal) || name.StartsWith("Presentation", StringComparison.Ordinal)
        || name.StartsWith("netstandard", StringComparison.Ordinal) || name.StartsWith("mscorlib", StringComparison.Ordinal)
        || name.StartsWith("Newtonsoft", StringComparison.Ordinal) || name.StartsWith("WinRT", StringComparison.Ordinal)
        || name.StartsWith("ReachFramework", StringComparison.Ordinal) || name.StartsWith("UIAutomation", StringComparison.Ordinal)
        || name.StartsWith("DirectWrite", StringComparison.Ordinal) || name.StartsWith("Vortice", StringComparison.Ordinal)
        || name.StartsWith("SharpGen", StringComparison.Ordinal) || name.StartsWith("NAudio", StringComparison.Ordinal);

    /// <summary>エフェクトの型が実装しているインターフェース(IVideoEffect / IAudioEffect)。名前で探す</summary>
    static Type? InterfaceOf(Type effectType, bool video)
    {
        var simple = InterfaceName(video);
        try { return effectType.GetInterfaces().FirstOrDefault(i => i.Name == simple); }
        catch { return null; }
    }

    static object? TryCreate(Type t)
    {
        if (t.GetConstructor(Type.EmptyTypes) != null) return Activator.CreateInstance(t);
        if (t.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null) != null)
            return Activator.CreateInstance(t, true);
        return null;
    }

    /// <summary>
    /// エフェクトを探す。YMM4本体の標準のエフェクトも、プラグインで追加されたエフェクトも見つける。
    /// 型そのものではなく「インターフェースの名前」で探す(図形と同じ方式。YMM4の部品が別々の領域に読み込まれていても見つかる)。
    /// YMM4の一覧に出るエフェクト(「…EffectAttribute」が付いているもの)を優先し、1つも無ければ、付いていないものも対象にする。
    /// </summary>
    public static List<BuiltinEffect> FindEffects(bool video, out string message)
    {
        var kindText = video ? "映像" : "音声";
        var simple = InterfaceName(video);
        var self = typeof(BuiltinCatalog).Assembly;
        var withAttr = new List<BuiltinEffect>();
        var noAttr = new List<(Type type, bool builtIn, string asmName)>();
        int candidates = 0, skipped = 0, asmCount = 0;
        string lastError = "";

        foreach (var asm in AllAssemblies())
        {
            if (asm == self || asm.IsDynamic) continue;
            var asmName = "";
            try { asmName = asm.GetName().Name ?? ""; } catch { }
            if (IsSystemAssembly(asmName)) continue;
            asmCount++;
            var builtIn = IsBuiltInAssembly(asm);
            foreach (var t in TypesOf(asm))
            {
                try
                {
                    if (!t.IsClass || t.IsAbstract || t.ContainsGenericParameters) continue;
                    if (!t.GetInterfaces().Any(i => i.Name == simple)) continue;
                    candidates++;
                    bool marked;
                    try { marked = t.GetCustomAttributesData().Any(a => a.AttributeType.Name.EndsWith("EffectAttribute", StringComparison.Ordinal)); }
                    catch { marked = false; }
                    if (!marked) { noAttr.Add((t, builtIn, asmName)); continue; }

                    var instance = TryCreate(t);
                    if (instance == null) { skipped++; continue; }
                    var label = Ymm.Get(instance, "Label") as string;
                    if (string.IsNullOrWhiteSpace(label)) label = t.Name.Replace("Effect", "");
                    withAttr.Add(new BuiltinEffect(label!, t, instance, builtIn, asmName));
                }
                catch (Exception ex) { skipped++; lastError = ex.GetBaseException().Message; }
            }
        }

        var list = withAttr;
        if (list.Count == 0)
        {
            // 印(Attribute)の付いたものが1つも無い時は、付いていないものも対象にする
            foreach (var (t, builtIn, asmName) in noAttr)
            {
                try
                {
                    var instance = TryCreate(t);
                    if (instance == null) { skipped++; continue; }
                    var label = Ymm.Get(instance, "Label") as string;
                    if (string.IsNullOrWhiteSpace(label)) label = t.Name.Replace("Effect", "");
                    list.Add(new BuiltinEffect(label!, t, instance, builtIn, asmName));
                }
                catch (Exception ex) { skipped++; lastError = ex.GetBaseException().Message; }
            }
        }
        list = list.GroupBy(x => x.Type.FullName ?? x.Type.Name).Select(g => g.First())
                   .OrderByDescending(x => x.IsBuiltIn).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        message = list.Count == 0
            ? $"{kindText}エフェクトを見つけられませんでした(調べたアセンブリ {asmCount} 個 / {simple} を実装した型 {candidates} 個 / 作成できなかったもの {skipped} 個"
              + (lastError.Length > 0 ? $" / 最後のエラー: {lastError}" : "") + ")"
            : $"{kindText}エフェクト 標準 {list.Count(x => x.IsBuiltIn)} 個 / プラグイン {list.Count(x => !x.IsBuiltIn)} 個を見つけました"
              + (skipped > 0 ? $"(作成できなかったもの {skipped} 個)" : "");
        return list;
    }

    /// <summary>エフェクト1つ分を、「エフェクトのまとまり」のJSON(YMM4のJSON)にする</summary>
    public static string? EffectToJson(bool video, object effect)
    {
        // そのエフェクト自身が実装しているインターフェースを使う(同じ名前の型が複数読み込まれていても、食い違わない)
        var iface = InterfaceOf(effect.GetType(), video);
        if (iface == null) { return null; }
        var listType = typeof(ImmutableList<>).MakeGenericType(iface);
        var list = Ymm.MergeEffects(listType, null, new[] { effect }, false);
        return list == null ? null : Ymm.ValueToJson(list);
    }
}

/// <summary>
/// YMM4やプラグインが持っている「表示用の文字」(リソース)から、キー(例: YMM4Key_EffectCategoryAnimationName)に対応する日本語を探す。
/// </summary>
public static class YmmText
{
    static List<System.Resources.ResourceManager>? managers;
    static readonly Dictionary<string, string> cache = new();

    /// <summary>キーらしい文字(英数字と _ だけで、_ を含む)か</summary>
    public static bool LooksLikeKey(string? text)
    {
        if (string.IsNullOrEmpty(text) || !text.Contains('_')) return false;
        foreach (var c in text) if (!(c == '_' || (c < 128 && char.IsLetterOrDigit(c)))) return false;
        return true;
    }

    static List<System.Resources.ResourceManager> Managers()
    {
        if (managers != null) return managers;
        var list = new List<System.Resources.ResourceManager>();
        foreach (var asm in BuiltinCatalog.AllAssemblies())
        {
            try
            {
                if (asm.IsDynamic) continue;
                var name = asm.GetName().Name ?? "";
                if (BuiltinCatalog.IsSystemAssembly(name)) continue;
                foreach (var t in BuiltinCatalog.TypesOf(asm))
                {
                    try
                    {
                        var p = t.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (p != null && typeof(System.Resources.ResourceManager).IsAssignableFrom(p.PropertyType)
                            && p.GetValue(null) is System.Resources.ResourceManager rm)
                            list.Add(rm);
                    }
                    catch { }
                }
            }
            catch { }
        }
        managers = list;
        return list;
    }

    /// <summary>YMM4のエフェクトの分類名(YMM4標準の画面と同じ呼び名)</summary>
    static readonly Dictionary<string, string> CategoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Animation"] = "アニメーション", ["Branch"] = "分岐", ["Camera"] = "カメラ", ["Composition"] = "合成",
        ["Decoration"] = "装飾", ["Drawing"] = "描画", ["Filtering"] = "加工", ["Layout"] = "配置",
        ["Transition"] = "登場退場", ["Text"] = "テキスト", ["3D"] = "3D", ["ThreeD"] = "3D", ["Audio"] = "音声",
        ["Effect"] = "エフェクト", ["Other"] = "その他", ["Others"] = "その他", ["Plugin"] = "プラグイン",
        ["Template"] = "テンプレート", ["Color"] = "色", ["Blur"] = "ぼかし", ["Distortion"] = "ゆがみ",
        ["Volume"] = "音量", ["Delay"] = "ディレイ", ["Equalizer"] = "イコライザー", ["Pitch"] = "音程", ["Noise"] = "ノイズ",
    };

    /// <summary>YMM4の中から見つからなかったキーを、こちらで持っている対応表で日本語にする</summary>
    static string? Known(string key)
    {
        var k = key;
        if (k.StartsWith("YMM4Key_", StringComparison.Ordinal)) k = k.Substring(8);
        foreach (var prefix in new[] { "EffectCategory", "VideoEffectCategory", "AudioEffectCategory", "Category" })
            if (k.StartsWith(prefix, StringComparison.Ordinal) && k.Length > prefix.Length) { k = k.Substring(prefix.Length); break; }
        if (k.EndsWith("Name", StringComparison.Ordinal) && k.Length > 4) k = k.Substring(0, k.Length - 4);
        return CategoryNames.TryGetValue(k, out var jp) ? jp : null;
    }

    /// <summary>キーなら日本語に直す。見つからなければ、そのまま返す</summary>
    public static string Resolve(string text, Type? resourceType = null)
    {
        if (!LooksLikeKey(text)) return text;
        if (cache.TryGetValue(text, out var hit)) return hit;
        string? found = null;
        try
        {
            if (resourceType != null)
                found = resourceType.GetProperty(text, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as string;
        }
        catch { }
        if (string.IsNullOrEmpty(found))
            foreach (var rm in Managers())
            {
                try { found = rm.GetString(text, System.Globalization.CultureInfo.CurrentUICulture); }
                catch { found = null; }
                if (!string.IsNullOrEmpty(found)) break;
            }
        if (string.IsNullOrEmpty(found)) found = Known(text);
        var result = string.IsNullOrEmpty(found) ? text : found!;
        cache[text] = result;
        return result;
    }
}
