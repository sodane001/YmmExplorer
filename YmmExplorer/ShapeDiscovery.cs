using System.Reflection;
using System.Runtime.Loader;

namespace YmmExplorer;

/// <summary>YMM4に読み込まれている図形(標準・プラグイン)1つ分</summary>
public sealed class ShapeInfo
{
    /// <summary>図形プラグインの型名(同じ図形かどうかの目印)</summary>
    public string Key { get; init; } = "";
    public string Name { get; init; } = "";
    /// <summary>「標準」または「プラグイン: 名前」</summary>
    public string Source { get; init; } = "";
    public bool IsBuiltIn { get; init; }
}

/// <summary>
/// 実行中のYMM4に読み込まれている「図形プラグイン」(IShapePlugin を実装した型)を探します。
/// YMM4本体の標準の図形も、後から追加したプラグインの図形も、同じ仕組みで見つけます。
/// YMM4のAPIを直接参照せず、型の名前で探しているので、バージョンが変わっても動作しやすい方式です。
/// </summary>
public static class ShapeDiscovery
{
    const string InterfaceName = "IShapePlugin";

    public static IReadOnlyList<ShapeInfo> Found { get; private set; } = Array.Empty<ShapeInfo>();
    public static bool Done { get; private set; }

    static IEnumerable<Assembly> AllAssemblies()
    {
        var seen = new HashSet<Assembly>();
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            if (seen.Add(a)) yield return a;
        // プラグインは別のロード領域に読み込まれていることがある
        IEnumerable<AssemblyLoadContext> contexts;
        try { contexts = AssemblyLoadContext.All.ToList(); }
        catch { yield break; }
        foreach (var c in contexts)
        {
            IEnumerable<Assembly> list;
            try { list = c.Assemblies.ToList(); }
            catch { continue; }
            foreach (var a in list)
                if (seen.Add(a)) yield return a;
        }
    }

    static Type[] TypesOf(Assembly a)
    {
        try { return a.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null).ToArray()!; }
        catch { return Array.Empty<Type>(); }
    }

    /// <summary>時間がかかることがあるので、裏のスレッドで呼んでください</summary>
    public static List<ShapeInfo> Scan()
    {
        var result = new List<ShapeInfo>();
        var self = typeof(ShapeDiscovery).Assembly;
        foreach (var asm in AllAssemblies())
        {
            // 同梱の演出図形は、このプラグイン自身(YmmExplorer.dll)の中にあるので、自分も探す
            if (asm.IsDynamic) continue;
            foreach (var t in TypesOf(asm))
            {
                try
                {
                    if (!t.IsClass || t.IsAbstract || t.ContainsGenericParameters) continue;
                    if (!t.GetInterfaces().Any(i => i.Name == InterfaceName)) continue;
                    if (t.GetConstructor(Type.EmptyTypes) == null) continue;

                    var asmName = asm.GetName().Name ?? "";
                    var location = "";
                    try { location = asm.Location ?? ""; } catch { }
                    var builtIn = asmName.StartsWith("YukkuriMovieMaker", StringComparison.OrdinalIgnoreCase)
                                  && !location.Replace('/', '\\').Contains(@"\user\plugin\", StringComparison.OrdinalIgnoreCase);
                    result.Add(new ShapeInfo
                    {
                        Key = t.FullName ?? t.Name,
                        Name = ReadName(t),
                        IsBuiltIn = builtIn,
                        Source = builtIn ? "標準" : asm == self ? "YmmExplorer 同梱の演出図形" : $"プラグイン: {asmName}",
                    });
                }
                catch { /* 読み込めない型は飛ばす */ }
            }
        }
        // 同じ型が複数回見つかった場合は1つにまとめる
        result = result.GroupBy(r => r.Key).Select(g => g.First())
                       .OrderByDescending(r => r.IsBuiltIn).ThenBy(r => r.Name, NaturalComparer.Instance).ToList();
        Found = result;
        Done = true;
        return result;
    }

    /// <summary>図形プラグインの表示名(Name プロパティ)。取れなければ型の名前</summary>
    static string ReadName(Type t)
    {
        object? instance = null;
        try
        {
            instance = Activator.CreateInstance(t);
            var prop = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            if (prop?.GetValue(instance) is string s && !string.IsNullOrWhiteSpace(s)) return s;
        }
        catch { }
        finally { (instance as IDisposable)?.Dispose(); }
        return t.Name;
    }
}
