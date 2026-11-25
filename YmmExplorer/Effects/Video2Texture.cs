using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 85〜100: 質感・重ねる

internal static class Tx
{
    /// <summary>形の上だけに模様を重ねる</summary>
    public static ID2D1Image Atop(FxKit k, ID2D1Image i, in FxFrame f, Action<ID2D1DeviceContext, ID2D1SolidColorBrush> draw)
    {
        var pat = k.Draw(f.B, dc => { using var b = dc.CreateSolidColorBrush(new Color4(1, 1, 1, 1)); draw(dc, b); });
        return k.Comp("atop", i, pat, CompositeMode.SourceAtop);
    }
    public static RawRectF Grow(RawRectF r, float m) => new(r.Left - m, r.Top - m, r.Right + m, r.Bottom + m);
}

[VideoEffect("ドロップシャドウ", ["演出：質感・画風"], ["影", "ドロップシャドウ", "drop shadow", "立体", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class DropShadowEffect : FxVSC
{
    public override string Label => "ドロップシャドウ";
    public DropShadowEffect() { Color = System.Windows.Media.Color.FromRgb(0, 0, 0); }
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color); float u = MathF.Min(f.W, f.H), d = u * (.012f + Z(f) * .07f);
        var sh = k.Blur("b", k.Affine("a", k.Matrix("m", i, FxKit.Fill(c.R, c.G, c.B, Math.Min(P(f), 1f) * .75f)), Mv.Move(d, d)), d * .7f);
        return k.Comp("c", sh, i);
    }
}

[VideoEffect("長い影", ["演出：質感・画風"], ["長い影", "long shadow", "フラットデザイン", "影", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class LongShadowEffect : FxVSC
{
    public override string Label => "長い影";
    public LongShadowEffect() { Color = System.Windows.Media.Color.FromRgb(20, 24, 34); }
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color); float len = MathF.Min(f.W, f.H) * (.04f + Z(f) * .5f), al = Math.Min(P(f), 1f); const int n = 18;
        ID2D1Image sil = k.Matrix("m", i, FxKit.Fill(c.R, c.G, c.B, al)), o = k.Affine("a0", sil, Mv.Move(len, len));
        for (int s = n - 1; s >= 1; s--) { float q = s / (float)n * len; o = k.Comp("c" + s, o, k.Affine("a" + s, sil, Mv.Move(q, q))); }
        return k.Comp("top", o, i);
    }
}

[VideoEffect("縁取り", ["演出：質感・画風"], ["縁取り", "アウトライン", "outline", "ふち", "ステッカー", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class OutlineEffect : FxVSC
{
    public override string Label => "縁取り";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color, Math.Min(P(f), 1f));
        return k.Comp("c", k.Outline("o", i, MathF.Min(f.W, f.H) * (.006f + Z(f) * .05f), c), i);
    }
}

[VideoEffect("二重の縁取り", ["演出：質感・画風"], ["二重", "縁取り", "アウトライン", "double outline", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class DoubleOutlineEffect : FxVCC
{
    public override string Label => "二重の縁取り";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float u = MathF.Min(f.W, f.H), p = Math.Min(P(f), 1f);
        var outer = k.Outline("o2", i, u * .05f, FxKit.Rgb(Color2, p)); var inner = k.Outline("o1", i, u * .024f, FxKit.Rgb(Color, p));
        return k.Comp("c2", k.Comp("c1", outer, inner), i);
    }
}

[VideoEffect("外側の光", ["演出：質感・画風"], ["外側の光", "グロー", "glow", "光る", "オーラ", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class OuterGlowEffect : FxVSC
{
    public override string Label => "外側の光";
    public OuterGlowEffect() { Color = System.Windows.Media.Color.FromRgb(0x6F, 0xE0, 0xFF); }
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color); float r = MathF.Min(f.W, f.H) * (.012f + Z(f) * .09f), a = MathF.Min(P(f), 2f);
        var wide = k.Blur("b1", k.Matrix("m", i, FxKit.Fill(c.R, c.G, c.B, MathF.Min(1f, a))), r); var near = k.Blur("b2", k.Matrix("m2", i, FxKit.Fill(c.R, c.G, c.B, MathF.Min(1f, a))), r * .35f);
        return k.Comp("top", k.Comp("c", wide, near, CompositeMode.Plus), i);
    }
}

[VideoEffect("内側の光", ["演出：質感・画風"], ["内側の光", "inner glow", "インナーグロー", "光る", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class InnerGlowEffect : FxVSC
{
    public override string Label => "内側の光";
    public InnerGlowEffect() { Color = System.Windows.Media.Color.FromRgb(0xFF, 0xF2, 0xB0); }
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = new Color4(FxKit.Rgb(Color).R, FxKit.Rgb(Color).G, FxKit.Rgb(Color).B, Math.Min(P(f), 1f)); var b = f.B;
        var plate = k.Draw(b, dc => { using var br = dc.CreateSolidColorBrush(c); dc.FillRectangle(b, br); });
        var soft = k.Blur("b", i, MathF.Min(f.W, f.H) * (.01f + Z(f) * .07f));
        var ring = k.Comp("ring", plate, soft, CompositeMode.DestinationOut);
        return k.Comp("c", i, ring, CompositeMode.SourceAtop);
    }
}

[VideoEffect("エンボス", ["演出：質感・画風"], ["エンボス", "emboss", "凹凸", "浮き彫り", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class EmbossEffect : FxVS
{
    public override string Label => "エンボス";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float d = MathF.Max(1f, MathF.Min(f.W, f.H) * (.002f + Z(f) * .012f)), p = Math.Min(P(f), 1f);
        var inv = k.Matrix("inv", k.Affine("a2", i, Mv.Move(d, d)), Mx.Out(-.2126f, -.7152f, -.0722f, 1, -.2126f, -.7152f, -.0722f, 1, -.2126f, -.7152f, -.0722f, 1));
        var half = k.Matrix("half", inv, FxKit.Tone(0, 1, 1, .5f));
        var gray = k.Matrix("gray", k.Comp("c", k.Affine("a1", k.Matrix("luma", i, Mx.Out(.2126f, .7152f, .0722f, 0, .2126f, .7152f, .0722f, 0, .2126f, .7152f, .0722f, 0)), Mv.Move(-d, -d)), half), FxKit.Tone(0, 2.6f, 1));
        var cut = k.Comp("cut", i, gray, CompositeMode.SourceAtop);
        return p >= .999f ? cut : k.Comp("mix", i, k.Matrix("mix_a", cut, FxKit.Tone(0, 1, 1, p)));
    }
}

[VideoEffect("シャープ", ["演出：質感・画風"], ["シャープ", "sharpen", "くっきり", "輪郭", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class SharpenEffect : FxVS
{
    public override string Label => "シャープ";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        // 元の画像からぼかしを引いて、輪郭だけを足す
        float p = MathF.Min(P(f), 2f) * 1.1f, s = MathF.Max(.6f, MathF.Min(f.W, f.H) * (.001f + Z(f) * .007f));
        var soft = k.Blur("b", i, s);
        var sharp = k.Builtin("ar", "ArithmeticComposite", i, ("Coefficients", new Vector4(0, 1 + p, -p, 0)), ("ClampOutput", true));
        k.SetInput("ar", 1, soft);
        return k.Comp("in", sharp, i, CompositeMode.DestinationIn);
    }
}

[VideoEffect("走査線", ["演出：質感・画風"], ["走査線", "スキャンライン", "scanline", "モニター", "ブラウン管", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class ScanlinesEffect : FxVA
{
    public override string Label => "走査線";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float a = Math.Min(P(f), 1f) * .42f, step = MathF.Max(2f, f.H * (.006f + Z(f) * .02f)), y0 = f.B.Top + (f.T * Sp(f) * 18f) % step, l = f.B.Left, r = f.B.Right, b0 = f.B.Bottom;
        return Tx.Atop(k, i, f, (dc, b) => { b.Color = new Color4(0, 0, 0, a); for (float y = y0 - step; y < b0; y += step) dc.FillRectangle(new RawRectF(l, y, r, y + step * .45f), b); });
    }
}

[VideoEffect("砂嵐ノイズ", ["演出：質感・画風"], ["砂嵐", "ノイズ", "noise", "ざらざら", "テレビ", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class StaticNoiseEffect : FxVA
{
    public override string Label => "砂嵐ノイズ";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        int seed = (int)(f.T * MathF.Max(1f, MathF.Abs(Sp(f)) * 24f)); float freq = .25f + (1f - MathF.Min(Z(f), 1f)) * .7f;
        var n = k.Turb("t", Vector2.Zero, freq, 2, seed);
        var gray = k.Matrix("g", n, new float[] { 2.6f, 2.6f, 2.6f, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -.8f, -.8f, -.8f, Math.Min(P(f), 1f) * .55f });
        return k.Comp("c", i, gray, CompositeMode.SourceAtop);
    }
}

[VideoEffect("方眼", ["演出：質感・画風"], ["方眼", "グリッド", "grid", "マス目", "ノート", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class GridOverlayEffect : FxVSC
{
    public override string Label => "方眼";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color, Math.Min(P(f), 1f) * .55f); float st = MathF.Max(5f, MathF.Min(f.W, f.H) * (.025f + Z(f) * .14f)), l = f.B.Left, t = f.B.Top, r = f.B.Right, bo = f.B.Bottom, cx = f.C.X, cy = f.C.Y;
        return Tx.Atop(k, i, f, (dc, b) => { b.Color = c; for (float x = cx - MathF.Ceiling((cx - l) / st) * st; x <= r; x += st) dc.DrawLine(new(x, t), new(x, bo), b, 1.4f); for (float y = cy - MathF.Ceiling((cy - t) / st) * st; y <= bo; y += st) dc.DrawLine(new(l, y), new(r, y), b, 1.4f); });
    }
}

[VideoEffect("斜めストライプ", ["演出：質感・画風"], ["ストライプ", "斜線", "stripe", "縞", "注意", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class StripeOverlayEffect : FxVAC
{
    public override string Label => "斜めストライプ";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color, Math.Min(P(f), 1f) * .45f); float st = MathF.Max(6f, MathF.Min(f.W, f.H) * (.03f + Z(f) * .16f)), off = (f.T * Sp(f) * st * 1.2f) % (st * 2f), l = f.B.Left, t = f.B.Top, r = f.B.Right, bo = f.B.Bottom, h = f.H;
        return Tx.Atop(k, i, f, (dc, b) => { b.Color = c; for (float x = l - h - st * 2f + off; x < r; x += st * 2f) YMM4EffectShapes.Gfx.FillPoly(dc, b, new List<Vector2> { new(x + h, t), new(x + h + st, t), new(x + st, bo), new(x, bo) }); });
    }
}

[VideoEffect("水玉模様", ["演出：質感・画風"], ["水玉", "ドット", "polka dot", "模様", "ポップ", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class PolkaDotsEffect : FxVSC
{
    public override string Label => "水玉模様";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color, Math.Min(P(f), 1f) * .6f); float st = MathF.Max(8f, MathF.Min(f.W, f.H) * (.04f + Z(f) * .16f)), l = f.B.Left, t = f.B.Top, r = f.B.Right, bo = f.B.Bottom;
        return Tx.Atop(k, i, f, (dc, b) => { b.Color = c; int row = 0; for (float y = t; y <= bo + st; y += st * .5f, row++) for (float x = l + (row % 2) * st * .5f; x <= r + st; x += st) dc.FillEllipse(new Ellipse(new(x, y), st * .17f, st * .17f), b); });
    }
}

public abstract class FxStrips : FxVA
{
    protected abstract float Shift(int n, float y01, float t, float w);
    protected virtual int Bands(in FxFrame f) => Math.Clamp((int)(f.H / MathF.Max(2f, f.H * (.006f + Z(f) * .03f))), 4, 160);
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        int n = Bands(f); float amp = Math.Min(P(f), 2f), t = f.T * Sp(f), w = f.W, top = f.B.Top, bh = f.H / n, l = f.B.Left, r = f.B.Right;
        var area = Tx.Grow(f.B, w * .3f);
        return k.Draw(area, dc =>
        {
            for (int s = 0; s < n; s++)
            {
                float y0 = top + s * bh, dx = Shift(s, (s + .5f) / n, t, w) * amp;
                dc.PushAxisAlignedClip(new RawRectF(area.Left, y0, area.Right, y0 + bh + .6f), AntialiasMode.Aliased);
                dc.Transform = Matrix3x2.CreateTranslation(dx, 0); dc.DrawImage(i, InterpolationMode.Linear, CompositeMode.SourceOver); dc.Transform = Matrix3x2.Identity;
                dc.PopAxisAlignedClip();
            }
        });
    }
}

[VideoEffect("横波ゆらぎ", ["演出：質感・画風"], ["波", "ゆらぎ", "wave", "水面", "回想", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class HorizontalWaveEffect : FxStrips
{
    public override string Label => "横波ゆらぎ";
    protected override int Bands(in FxFrame f) => Math.Clamp((int)(f.H / 3f), 20, 180);
    protected override float Shift(int n, float y, float t, float w) => MathF.Sin(y * 11f + t * 3.2f) * w * .035f;
}

[VideoEffect("スライスずれ", ["演出：質感・画風"], ["スライス", "ずれ", "slice", "グリッチ", "切れる", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class SliceShiftEffect : FxStrips
{
    public override string Label => "スライスずれ";
    protected override int Bands(in FxFrame f) => Math.Clamp((int)(4f + Z(f) * 34f), 4, 80);
    protected override float Shift(int n, float y, float t, float w) { int step = (int)(t * 7f); return (FxKit.Hash(n * 31 + step * 17) - .5f) * w * .18f; }
}

[VideoEffect("シルエット", ["演出：質感・画風"], ["シルエット", "silhouette", "黒塗り", "正体", "逆光", "質感", "演出"], IsAviUtlSupported = false)]
public sealed class SilhouetteEffect : FxVCC
{
    public override string Label => "シルエット";
    public SilhouetteEffect() { Color = System.Windows.Media.Color.FromRgb(8, 8, 12); Color2 = System.Windows.Media.Color.FromRgb(0xFF, 0xC8, 0x6E); }
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var a = FxKit.Rgb(Color); var b = FxKit.Rgb(Color2); float p = Math.Min(P(f), 1f);
        var body = k.Matrix("m", i, Mx.Mix(Mx.Out(0, 0, 0, a.R, 0, 0, 0, a.G, 0, 0, 0, a.B), p));
        var glow = k.Blur("b", k.Matrix("g", i, FxKit.Fill(b.R, b.G, b.B, p)), MathF.Min(f.W, f.H) * .045f);
        return k.Comp("c", glow, body);
    }
}
