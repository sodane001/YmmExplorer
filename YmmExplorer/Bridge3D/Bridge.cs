using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Direct3D11;
using YMM43D.Commons;
using YMM43D.Plugin;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace YmmExplorer.Bridge3D;

/// <summary>YmmExplorer から呼ぶ入口(型は YMM4 と .NET の共通の物だけを使う)</summary>
public static class Factory
{
    public static IShapeSource Create(IShapeSource inner, object parameter, IGraphicsDevicesAndContext devices,
        Action<ID3D11Device, ID3D11DeviceContext, Matrix4x4, Matrix4x4, Vector3, float, int, int, int, bool> draw,
        Func<int, int, int, (Vector3 Min, Vector3 Max)> bounds, Action<bool> hostRenders)
    {
        var s = new Source3D(inner, parameter, devices, draw, bounds, hostRenders);
        // 3Dプレビューは、図形アイテムの「図形の設定」をかぎにして、立体の描画元を探す。ここで登録しておく
        Provider3DRegistry.Register(parameter, s, devices);
        return s;
    }
}

/// <summary>
/// 演出3Dの図形の描画元。ふだんの画面には今まで通りの絵を出し、
/// 3Dプレビューからは I3DProvider として呼ばれて、立体を3D空間にそのまま描く
/// </summary>
internal sealed class Source3D : IShapeSource2, I3DProvider, I3DBounds, I3DAlive
{
    readonly IShapeSource inner;
    readonly object parameter;
    readonly IGraphicsDevicesAndContext devices;
    readonly Action<bool> hostRenders;
    // ふだんの画面の絵も、3Dプレビューの部品(シーンのカメラ・光)で描く。3Dプレビューの図形と同じ方法なので、両方の見え方がそろう
    readonly Output3DRenderer output3D = new();
    ID2D1Image? hostImage;
    bool hostFailed;
    readonly Action<ID3D11Device, ID3D11DeviceContext, Matrix4x4, Matrix4x4, Vector3, float, int, int, int, bool> draw;
    readonly Func<int, int, int, (Vector3 Min, Vector3 Max)> bounds;

    public Source3D(IShapeSource inner, object parameter, IGraphicsDevicesAndContext devices,
        Action<ID3D11Device, ID3D11DeviceContext, Matrix4x4, Matrix4x4, Vector3, float, int, int, int, bool> draw,
        Func<int, int, int, (Vector3 Min, Vector3 Max)> bounds, Action<bool> hostRenders)
    {
        this.inner = inner;
        this.parameter = parameter;
        this.devices = devices;
        this.hostRenders = hostRenders;
        hostRenders(true);
        this.draw = draw;
        this.bounds = bounds;
    }

    public ID2D1Image Output => hostImage ?? inner.Output;

    public static string LastError { get; private set; } = "";
    public IEnumerable<VideoController> Controllers => inner is IShapeSource2 s2 ? s2.Controllers : [];
    public void Update(TimelineItemSourceDescription desc)
    {
        inner.Update(desc);
        hostImage = null;
        if (!hostFailed)
        {
            try
            {
                var f = FrameContext.FromItem(desc);

                hostImage = output3D.Render(devices, desc, GetLocalBounds(f), Matrix4x4.Identity, Draw, out _, this, true, null);
            }
            catch (Exception e)
            {
                // 描けなかった時は、自分で描いた絵に戻す
                hostFailed = true;
                LastError = e.ToString();
                hostRenders(false);
                inner.Update(desc);
            }
        }
        SceneRevision.Advance();
    }

    public void Dispose()
    {
        IsAlive = false;
        Provider3DRegistry.Unregister(parameter, this);
        output3D.Dispose();
        inner.Dispose();
    }

    public bool IsAlive { get; private set; } = true;
    public bool RequiresMappedTexture => false;

    public void Draw(in Render3DContext rc, DrawContext3D dc)
    {
        var t = dc.Time;
        draw(rc.Device, rc.Context, dc.World, rc.ViewProjection, rc.GetCameraPosition(), dc.Opacity, t.Frame, t.Length, t.Fps, dc.DepthOnly || rc.IsShadowPass);
    }

    public WorldBounds GetLocalBounds(in FrameContext f)
    {
        var (min, max) = bounds(f.Frame, f.Length, f.Fps);
        return new WorldBounds(min, max);
    }
}
