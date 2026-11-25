using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.Loader;
using Vortice.Direct3D11;
using YukkuriMovieMaker.Player.Video;

namespace YMM4EffectShapes;

/// <summary>
/// 3Dプレビュー(YMM43D、別のプラグイン)が入っている時だけ、演出3Dの図形を、その3D空間の中に立体のまま置けるようにする。
/// YMM43D を参照するのは別ファイルの小さな橋渡し(YmmExplorer.Bridge3D.bin)だけ。YMM43D が無い時は何もしない。
/// YMM43D.dll そのものは同梱しない(参照して使うだけ)
/// </summary>
internal static class Ymm43DBridge
{
    public const string FileName = "YmmExplorer.Bridge3D.bin";
    static MethodInfo? create;
    static bool loadFailed;

    public static string Status { get; private set; } = "";

    public static IShapeSource Wrap(EffectShapeSource source, object parameter, YukkuriMovieMaker.Commons.IGraphicsDevicesAndContext devices)
    {
        var m = Get();
        if (m is null) return source;
        try
        {
            Action<ID3D11Device, ID3D11DeviceContext, Matrix4x4, Matrix4x4, Vector3, float, int, int, int, bool> draw = source.Draw3DExternal;
            Func<int, int, int, (Vector3 Min, Vector3 Max)> bounds = source.Bounds3D;
            Action<bool> hostRenders = v => source.HostRenders = v;
            var wrapped = (IShapeSource?)m.Invoke(null, [source, parameter, devices, draw, bounds, hostRenders]);
            if (wrapped is null) return source;
            source.Bridged = true;
            return wrapped;
        }
        catch (Exception e)
        {
            Status = "3Dプレビューとの連携に失敗: " + e.GetBaseException().Message;
            return source;
        }
    }

    static MethodInfo? Get()
    {
        if (create is not null || loadFailed) return create;
        // YMM43D が読み込まれていなければ、まだ何もしない(後から読み込まれることもあるので、失敗扱いにしない)
        var ymm43d = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name == "YMM43D" && a.GetType("YMM43D.Commons.I3DProvider", false) is not null)
            .OrderByDescending(a => a.GetName().Version)
            .FirstOrDefault();
        if (ymm43d is null) return null;
        try
        {
            var dir = Path.GetDirectoryName(typeof(Ymm43DBridge).Assembly.Location) ?? "";
            var path = Path.Combine(dir, FileName);
            if (!File.Exists(path)) { loadFailed = true; Status = "橋渡しファイルがありません"; return null; }
            // YMM43D と同じ読み込み先に入れて、YMM43D の型をそのまま使えるようにする
            var alc = AssemblyLoadContext.GetLoadContext(ymm43d) ?? AssemblyLoadContext.Default;
            var asm = alc.LoadFromAssemblyPath(path);
            create = asm.GetType("YmmExplorer.Bridge3D.Factory")?.GetMethod("Create");
            if (create is null) { loadFailed = true; Status = "橋渡しの読み込みに失敗"; }
            else Status = "3Dプレビュー(YMM43D " + ymm43d.GetName().Version + ")と連携中";
            return create;
        }
        catch (Exception e)
        {
            loadFailed = true;
            Status = "3Dプレビューとの連携に失敗: " + e.GetBaseException().Message;
            return null;
        }
    }
}
