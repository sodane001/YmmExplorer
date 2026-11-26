using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  映像 701番〜の土台: 光(絵の上に光を描いて足す)
// =====================================================================================================

/// <summary>光。色・強さ・大きさ・速さ。Add=true で光を足す(明るくなる)、false でふつうに重ねる</summary>
public abstract class FxLight : FxVAC
{
    private protected virtual bool Add => true;
    private protected abstract void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Color4 c, float l, float t, float r, float bo, float z, float o);
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color, Math.Min(P(f), 1f));
        float z = MathF.Max(.05f, Z(f) * 2f), o = f.T * Sp(f);
        float l = f.B.Left, t = f.B.Top, r = f.B.Right, bo = f.B.Bottom;
        var img = k.Draw(f.B, dc => { using var b = dc.CreateSolidColorBrush(c); Lt(dc, b, c, l, t, r, bo, z, o); });
        return Add ? k.Comp("c", i, img, CompositeMode.Plus) : k.Comp("c", i, img);
    }
    /// <summary>色 c の透明度を a 倍にして筆に入れる</summary>
    private protected static void A(ID2D1SolidColorBrush b, Color4 c, float a) => b.Color = new Color4(c.R, c.G, c.B, Math.Clamp(c.A * a, 0f, 1f));
    private protected static void Rgb(ID2D1SolidColorBrush b, uint rgb, float a) => b.Color = new Color4(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, Math.Clamp(a, 0f, 1f));
    private protected static void Dot(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float x, float y, float rr) => dc.FillEllipse(new Ellipse(new(x, y), rr, rr), b);
    /// <summary>ふちがぼやけた丸(同心円を重ねる)</summary>
    private protected static void Soft(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Color4 c, float x, float y, float rr, float a)
    {
        for (int k = 6; k >= 1; k--) { A(b, c, a * .22f); dc.FillEllipse(new Ellipse(new(x, y), rr * k / 6f, rr * k / 6f), b); }
    }
    private protected static void Poly(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, params float[] xy) { var q = new List<Vector2>(); for (int i = 0; i + 1 < xy.Length; i += 2) q.Add(new(xy[i], xy[i + 1])); YMM4EffectShapes.Gfx.FillPoly(dc, b, q); }
    private protected static float H(int i) { uint x = (uint)i * 2654435761u; x ^= x >> 13; return (x % 1000) / 1000f; }
}
