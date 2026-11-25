using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  映像 101番〜・音声 101番〜の共通の土台
// =====================================================================================================

/// <summary>色の調整(行列1つで作る色味)</summary>
public abstract class FxGrade : FxVP
{
    private protected abstract float[] M { get; }
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(M, Math.Min(P(f), 1f)));
}

internal static class Gd
{
    /// <summary>彩度・コントラスト・明るさ・赤緑青の強さ</summary>
    public static float[] G(float sat, float con, float br, float r = 1, float g = 1, float b = 1)
    {
        const float lr = .2126f, lg = .7152f, lb = .0722f; float n = 1 - sat, o = .5f - .5f * con + br;
        float[] R(float gain, int d) { float q = con * gain; return [q * (n * lr + (d == 0 ? sat : 0)), q * (n * lg + (d == 1 ? sat : 0)), q * (n * lb + (d == 2 ? sat : 0)), o]; }
        var x = R(r, 0); var y = R(g, 1); var z = R(b, 2);
        return Mx.Out(x[0], x[1], x[2], x[3], y[0], y[1], y[2], y[3], z[0], z[1], z[2], z[3]);
    }
    /// <summary>暗い所を色 a、明るい所を色 b にする</summary>
    public static float[] D(uint a, uint b)
    {
        float C(uint v, int s) => ((v >> s) & 255) / 255f; float ar = C(a, 16), ag = C(a, 8), ab = C(a, 0), dr = C(b, 16) - ar, dg = C(b, 8) - ag, db = C(b, 0) - ab;
        return Mx.Out(dr * .2126f, dr * .7152f, dr * .0722f, ar, dg * .2126f, dg * .7152f, dg * .0722f, ag, db * .2126f, db * .7152f, db * .0722f, ab);
    }
}

/// <summary>動き(変形の行列で作る)。a は 強さ×大きさ で、初期値で 1 に近い</summary>
public abstract class FxMove : FxVA
{
    private protected abstract Matrix3x2 Mat(in FxFrame f, float t, float a);
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Affine("a", i, Mat(f, f.T * Sp(f), P(f) * Z(f) * 2f));
    private protected static float S(float x) => MathF.Sin(x);
    private protected static float C(float x) => MathF.Cos(x);
    private protected static float N(float t, int s) => FxKit.Noise(t, s);
    private protected static Matrix3x2 Mo(in FxFrame f, float x, float y) => Mv.Move(x * f.W, y * f.H);
    private protected static Matrix3x2 Ro(in FxFrame f, float a) => Mv.Rot(f.C, a);
    private protected static Matrix3x2 Sc(in FxFrame f, float x, float y) => Mv.Scale(f.C, MathF.Max(.02f, x), MathF.Max(.02f, y));
    private protected static Matrix3x2 Ft(in FxFrame f, float x, float y) => Mv.Scale(new Vector2(f.C.X, f.B.Bottom), MathF.Max(.02f, x), MathF.Max(.02f, y));
    private protected static Matrix3x2 RoF(in FxFrame f, float a) => Mv.Rot(new Vector2(f.C.X, f.B.Bottom), a);
    private protected static Matrix3x2 Sk(in FxFrame f, float k) => Mv.SkewX(f.B.Bottom, k);
}

/// <summary>登場・退場(d=1 で消えた状態、d=0 で元の姿)</summary>
public abstract class FxEnter : FxInOut
{
    private protected abstract Matrix3x2 Mat(in FxFrame f, float d, float dir);
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = EaseOut(Prog(f)), d = (1f - q) * Math.Min(P(f), 1f);
        return k.Matrix("m", k.Affine("a", i, Mat(f, d, Leaving(f) ? -1f : 1f)), Alpha(Q(f, MathF.Min(1f, Prog(f) * 3f))));
    }
    private protected static Matrix3x2 Mo(in FxFrame f, float x, float y) => Mv.Move(x * f.W, y * f.H);
    private protected static Matrix3x2 Ro(in FxFrame f, float a) => Mv.Rot(f.C, a);
    private protected static Matrix3x2 Sc(in FxFrame f, float x, float y) => Mv.Scale(f.C, MathF.Max(.012f, x), MathF.Max(.012f, y));
    private protected static Matrix3x2 Ft(in FxFrame f, float x, float y) => Mv.Scale(new Vector2(f.C.X, f.B.Bottom), MathF.Max(.012f, x), MathF.Max(.012f, y));
    private protected static Matrix3x2 RoF(in FxFrame f, float a) => Mv.Rot(new Vector2(f.C.X, f.B.Bottom), a);
    private protected static Matrix3x2 Sk(in FxFrame f, float k) => Mv.SkewX(f.B.Bottom, k);
}

/// <summary>形の上に重ねる模様。st はマスの大きさ、c は色</summary>
public abstract class FxPat : FxVAC
{
    private protected abstract void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o);
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color, Math.Min(P(f), 1f) * .55f); float st = MathF.Max(7f, MathF.Min(f.W, f.H) * (.03f + Z(f) * .15f)), o = f.T * Sp(f) * st * .8f, l = f.B.Left, t = f.B.Top, r = f.B.Right, bo = f.B.Bottom;
        return Tx.Atop(k, i, f, (dc, b) => { b.Color = c; Pat(dc, b, l, t, r, bo, st, o); });
    }
    private protected static void Each(float l, float t, float r, float bo, float st, Action<float, float, int> a)
    {
        int row = 0; for (float y = t - st; y <= bo + st; y += st, row++) { int col = 0; for (float x = l - st + (row % 2) * st * .5f; x <= r + st; x += st, col++) a(x, y, row * 31 + col); }
    }
    private protected static void Poly(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, params float[] xy) { var q = new List<Vector2>(); for (int i = 0; i + 1 < xy.Length; i += 2) q.Add(new(xy[i], xy[i + 1])); YMM4EffectShapes.Gfx.FillPoly(dc, b, q); }
    private protected static void Line(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float x0, float y0, float x1, float y1, float w = 2f) => dc.DrawLine(new(x0, y0), new(x1, y1), b, w);
    private protected static void Dot(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float x, float y, float rr) => dc.FillEllipse(new Ellipse(new(x, y), rr, rr), b);
    private protected static void Ring(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float x, float y, float rr, float w = 2f) => dc.DrawEllipse(new Ellipse(new(x, y), rr, rr), b, w);
}

// ----------------------------------------------------------------------------------------------- 音声

/// <summary>反響。tl/tr は秒、fb は返り、cross は左右の入れ替わり</summary>
internal sealed class EchoK(FxA2 e, double tl, double tr, float fb, double hp, double lp, float wet, bool cross = false) : KB(e)
{
    readonly DelayLine a = new(), b = new(); readonly StereoBiquad h = new(), o = new(); float xl, xr;
    protected override void Init() { int n = (int)(Math.Max(tl, tr) * Hz * 1.6) + 16; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); h.Reset(); o.Reset(); h.HighPass(hp, Hz); o.LowPass(lp, Hz); xl = xr = 0; }
    protected override void Run(ref float l, ref float r)
    {
        double k = .7 + A * .6; float x = a.Read(tl * k * Hz), y = b.Read(tr * k * Hz); h.Run(ref x, ref y); o.Run(ref x, ref y); xl = x; xr = y; float g = MathF.Min(.92f, fb * (.5f + A));
        a.Write(l + (cross ? xr : xl) * g); b.Write(r + (cross ? xl : xr) * g); l += x * wet; r += y * wet;
    }
}

/// <summary>音量の周期的な変化。shape: 0=波 1=四角 2=のこぎり 3=鼓動</summary>
internal sealed class GainLfoK(FxA3 e, double lo, double hi, int shape, bool stereo = false) : KB(e)
{
    Lfo p; float gl = 1, gr = 1;
    protected override void Init() { p.Reset(); gl = gr = 1; }
    float W(double u) => shape switch { 1 => u < .5 ? 1f : 0f, 2 => (float)(1 - u), 3 => (float)Math.Pow(Math.Max(0, Math.Sin(u * Math.PI * 2)), 8), _ => (float)(.5 + .5 * Math.Sin(u * Math.PI * 2)) };
    protected override void Run(ref float l, ref float r)
    {
        p.Step(Exp(lo, hi, S), Hz); float d = .2f + A * .8f, tl = 1 - d * (1 - W(p.Phase)), tr = stereo ? 1 - d * (1 - W((p.Phase + .5) % 1)) : tl; gl += (tl - gl) * .02f; gr += (tr - gr) * .02f; l *= gl; r *= gr;
    }
}

/// <summary>揺れる遅れ(コーラス・フランジャーの仲間)。ms は基準、dep は揺れ幅</summary>
internal sealed class ModDelayK(FxA3 e, double ms, double dep, double lo, double hi, float fb, float wet) : KB(e)
{
    readonly DelayLine a = new(), b = new(); Lfo p; float xl, xr;
    protected override void Init() { int n = (int)((ms + dep) * .0025 * Hz) + 32; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); p.Reset(); xl = xr = 0; }
    protected override void Run(ref float l, ref float r)
    {
        p.Step(Exp(lo, hi, S), Hz); double d = (ms + dep * (.3 + A * 1.2) * p.Sin) * .001 * Hz, d2 = (ms - dep * (.3 + A * 1.2) * p.Sin) * .001 * Hz; float g = fb * (.5f + A);
        a.Write(l + xl * g); b.Write(r + xr * g); xl = a.Read(Math.Max(2, d)); xr = b.Read(Math.Max(2, d2)); float k = 1f / (1f + wet * .6f); l = (l + xl * wet) * k; r = (r + xr * wet) * k;
    }
}

/// <summary>周期的に動く帯域フィルター。kind: 0=低域通過 1=帯域 2=高域通過</summary>
internal sealed class FilterLfoK(FxA3 e, double f0, double f1, int kind, double q, double lo, double hi) : KB(e)
{
    readonly StereoBiquad x = new(); Lfo p;
    protected override void Init() { x.Reset(); p.Reset(); }
    protected override void Tick()
    {
        p.Step(Exp(lo, hi, S) * 128, Hz); double c = Exp(f0, f0 + (f1 - f0) * (.25 + A * .75), p.Uni);
        if (kind == 0) x.LowPass(c, Hz, q); else if (kind == 1) x.BandPass(c, Hz, q); else x.HighPass(c, Hz, q);
    }
    protected override void Run(ref float l, ref float r) { x.Run(ref l, ref r); if (kind == 1) { l *= 2f; r *= 2f; } }
}

/// <summary>帯域を切って音量をそろえる(機器・場所の音)</summary>
internal sealed class BandK(FxA2 e, double hp, double lp, double db = 0) : KB(e)
{
    readonly StereoBiquad h = new(), h2 = new(), o = new(), o2 = new(); float g = 1;
    protected override void Init() { foreach (var x in new[] { h, h2, o, o2 }) x.Reset(); }
    protected override void Tick() { double w = .4 + A * 1.2; h.HighPass(hp * w, Hz); h2.HighPass(hp * w, Hz); o.LowPass(Math.Min(20000, lp / w), Hz); o2.LowPass(Math.Min(20000, lp / w), Hz); g = Db.ToGain(db); }
    protected override void Run(ref float l, ref float r) { h.Run(ref l, ref r); h2.Run(ref l, ref r); o.Run(ref l, ref r); o2.Run(ref l, ref r); l *= g; r *= g; }
}
