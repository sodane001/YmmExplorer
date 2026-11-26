using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  映像 601番〜の土台: フレーム(枠)。範囲のまわりに枠を描いて、絵の上に重ねる
// =====================================================================================================

/// <summary>枠。色・強さ(濃さ)・大きさ(太さ)・速さ(動く枠の速さ)</summary>
public abstract class FxBorder : FxVAC
{
    private protected abstract void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o);
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color, Math.Min(P(f), 1f));
        float w = MathF.Max(2f, MathF.Min(f.W, f.H) * (.006f + Z(f) * .03f)), o = f.T * Sp(f);
        float l = f.B.Left, t = f.B.Top, r = f.B.Right, bo = f.B.Bottom;
        var img = k.Draw(f.B, dc => { using var b = dc.CreateSolidColorBrush(c); Fr(dc, b, l, t, r, bo, w, o); });
        return k.Comp("c", i, img);
    }
    private protected static void L(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float x0, float y0, float x1, float y1, float w) => dc.DrawLine(new(x0, y0), new(x1, y1), b, w);
    /// <summary>内側に収まる四角の線</summary>
    private protected static void Box(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w) => dc.DrawRectangle(new RawRectF(l + w / 2f, t + w / 2f, r - w / 2f, bo - w / 2f), b, w);
    private protected static void Poly(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, params float[] xy) { var q = new List<Vector2>(); for (int i = 0; i + 1 < xy.Length; i += 2) q.Add(new(xy[i], xy[i + 1])); YMM4EffectShapes.Gfx.FillPoly(dc, b, q); }
    /// <summary>四角の周りの位置(左上から時計回りに d だけ進んだ所)</summary>
    private protected static Vector2 OnEdge(float l, float t, float r, float bo, float d)
    {
        float w = r - l, h = bo - t;
        if (d < w) return new(l + d, t); d -= w;
        if (d < h) return new(r, t + d); d -= h;
        if (d < w) return new(r - d, bo); d -= w;
        return new(l, bo - Math.Min(d, h));
    }
    /// <summary>点線(off だけずらす)</summary>
    private protected static void Dash(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float off)
    {
        float per = 2f * ((r - l) + (bo - t)), seg = w * 3f;
        for (float d = ((off % (seg * 2f)) + seg * 2f) % (seg * 2f); d < per; d += seg * 2f)
        {
            var a = OnEdge(l, t, r, bo, d); var c = OnEdge(l, t, r, bo, MathF.Min(per, d + seg));
            if (MathF.Abs(a.X - c.X) > .5f && MathF.Abs(a.Y - c.Y) > .5f) { var m = OnEdge(l, t, r, bo, d + seg * .5f); dc.DrawLine(a, m, b, w); dc.DrawLine(m, c, b, w); }
            else dc.DrawLine(a, c, b, w);
        }
    }
    /// <summary>波またはジグザグの枠</summary>
    private protected static void Wave(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o, bool zig)
    {
        float amp = w * 1.5f, step = w * 6f; l += amp + w; t += amp + w; r -= amp + w; bo -= amp + w;
        float per = 2f * ((r - l) + (bo - t)); Vector2? prev = null;
        for (float d = 0; d <= per + 1f; d += zig ? step / 2f : step / 8f)
        {
            var p = OnEdge(l, t, r, bo, MathF.Min(d, per)); var c = new Vector2((l + r) / 2f, (t + bo) / 2f);
            float ph = (d + o * 30f) / step * MathF.Tau, k = zig ? ((int)(d / (step / 2f)) % 2 == 0 ? 1f : -1f) : MathF.Sin(ph);
            var n = Vector2.Normalize(p - c); var q = p + n * k * amp;
            if (prev is Vector2 pv) dc.DrawLine(pv, q, b, w);
            prev = q;
        }
    }
    /// <summary>枠に沿って印を並べる</summary>
    private protected static void Along(float l, float t, float r, float bo, float gap, float off, Action<float, float, int> a)
    {
        float m = gap * .4f; l += m; t += m; r -= m; bo -= m;
        float per = 2f * ((r - l) + (bo - t)); int k = 0;
        for (float d = ((off % gap) + gap) % gap; d < per; d += gap, k++) { var p = OnEdge(l, t, r, bo, d); a(p.X, p.Y, k); }
    }
}
