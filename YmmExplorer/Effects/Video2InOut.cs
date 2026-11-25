using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 69〜84: 登場・退場

/// <summary>出入りの土台。q=1 で元の姿、q=0 で消えた状態。「強さ」が 0 なら元のまま</summary>
public abstract class FxInOut : FxVS
{
    protected float Sec(in FxFrame f) => MathF.Max(.02f, Z(f) * 1.2f);
    protected float Prog(in FxFrame f) => f.InOut(Sec(f), Sec(f));
    protected bool Leaving(in FxFrame f) => f.T > f.Dur / 2f;
    protected static float EaseOut(float x) => 1f - (1f - x) * (1f - x) * (1f - x);
    protected static float Back(float x) { float c = 1.9f, y = x - 1f; return 1f + (c + 1f) * y * y * y + c * y * y; }
    protected static float Bounce(float x)
    {
        const float n = 7.5625f, d = 2.75f;
        if (x < 1f / d) return n * x * x;
        if (x < 2f / d) { x -= 1.5f / d; return n * x * x + .75f; }
        if (x < 2.5f / d) { x -= 2.25f / d; return n * x * x + .9375f; }
        x -= 2.625f / d; return n * x * x + .984375f;
    }
    /// <summary>強さ 0 で 1(元のまま)に近づける</summary>
    protected float Q(in FxFrame f, float q) => 1f + (q - 1f) * Math.Min(P(f), 1f);
    internal static float[] Alpha(float a) => FxKit.Tone(0, 1, 1, Math.Clamp(a, 0f, 1f));
}

[VideoEffect("フェードで出入り", ["演出：出入り"], ["フェード", "fade", "ふわっと", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class FadeInOutEffect : FxInOut { public override string Label => "フェードで出入り"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float q = Prog(f); return k.Matrix("m", i, Alpha(Q(f, q * q * (3f - 2f * q)))); } }

[VideoEffect("ズームで出入り", ["演出：出入り"], ["ズーム", "zoom", "拡大して出る", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class ZoomInOutEffect : FxInOut { public override string Label => "ズームで出入り"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float q = EaseOut(Prog(f)), s = MathF.Max(.01f, Q(f, q)); return k.Matrix("m", k.Affine("a", i, Mv.Scale(f.C, s, s)), Alpha(Q(f, MathF.Min(1f, Prog(f) * 3f)))); } }

[VideoEffect("ポップで出る", ["演出：出入り"], ["ポップ", "pop", "ぽんっ", "弾む", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class PopInEffect : FxInOut { public override string Label => "ポップで出る"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = MathF.Max(.01f, Q(f, Back(Prog(f)))); return k.Matrix("m", k.Affine("a", i, Mv.Scale(f.C, s, s)), Alpha(Q(f, MathF.Min(1f, Prog(f) * 5f)))); } }

public abstract class FxSlide : FxInOut
{
    protected abstract Vector2 Dir(in FxFrame f);
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = EaseOut(Prog(f)), d = (1f - q) * Math.Min(P(f), 1f); var v = Dir(f);
        return k.Matrix("m", k.Affine("a", i, Mv.Move(v.X * d * f.W, v.Y * d * f.H)), Alpha(Q(f, MathF.Min(1f, Prog(f) * 3f))));
    }
}

[VideoEffect("左からスライド", ["演出：出入り"], ["スライド", "slide", "左から", "滑り込む", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class SlideLeftEffect : FxSlide { public override string Label => "左からスライド"; protected override Vector2 Dir(in FxFrame f) => new(Leaving(f) ? 1 : -1, 0); }

[VideoEffect("右からスライド", ["演出：出入り"], ["スライド", "slide", "右から", "滑り込む", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class SlideRightEffect : FxSlide { public override string Label => "右からスライド"; protected override Vector2 Dir(in FxFrame f) => new(Leaving(f) ? -1 : 1, 0); }

[VideoEffect("上から落ちる", ["演出：出入り"], ["落ちる", "drop", "バウンド", "弾む", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class DropBounceEffect : FxInOut
{
    public override string Label => "上から落ちる";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float p = Prog(f), q = Leaving(f) ? p * p : Bounce(p), y = -(1f - q) * Math.Min(P(f), 1f) * f.H * 1.15f;
        if (Leaving(f)) y = -y;
        return k.Matrix("m", k.Affine("a", i, Mv.Move(0, y)), Alpha(Q(f, MathF.Min(1f, p * 6f))));
    }
}

[VideoEffect("下からせり上がる", ["演出：出入り"], ["せり上がる", "下から", "rise", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class RiseUpEffect : FxSlide { public override string Label => "下からせり上がる"; protected override Vector2 Dir(in FxFrame f) => new(0, 1); }

[VideoEffect("回転しながら出る", ["演出：出入り"], ["回転", "spin", "くるっと", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class SpinInEffect : FxInOut
{
    public override string Label => "回転しながら出る";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = EaseOut(Prog(f)), s = MathF.Max(.01f, Q(f, q)), a = (1f - q) * MathF.Tau * Math.Min(P(f), 1f) * (Leaving(f) ? 1f : -1f);
        return k.Matrix("m", k.Affine("a", i, Mv.Scale(f.C, s, s) * Mv.Rot(f.C, a)), Alpha(Q(f, MathF.Min(1f, Prog(f) * 3f))));
    }
}

[VideoEffect("カードめくりで出る", ["演出：出入り"], ["カード", "めくる", "flip", "横回転", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class CardFlipInEffect : FxInOut
{
    public override string Label => "カードめくりで出る";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = EaseOut(Prog(f)), s = MathF.Max(.012f, Q(f, MathF.Sin(q * MathF.PI / 2f)));
        return k.Matrix("t", k.Matrix("m", k.Affine("a", i, Mv.Scale(f.C, s, 1f)), FxKit.Tone(-(1f - s) * .35f)), Alpha(Q(f, MathF.Min(1f, Prog(f) * 8f))));
    }
}

[VideoEffect("縦に伸びて出る", ["演出：出入り"], ["縦に伸びる", "開く", "stretch", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class StretchVEffect : FxInOut { public override string Label => "縦に伸びて出る"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = MathF.Max(.012f, Q(f, Back(Prog(f)))); return k.Matrix("m", k.Affine("a", i, Mv.Scale(f.C, 1f, s)), Alpha(Q(f, MathF.Min(1f, Prog(f) * 8f)))); } }

[VideoEffect("横に伸びて出る", ["演出：出入り"], ["横に伸びる", "開く", "stretch", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class StretchHEffect : FxInOut { public override string Label => "横に伸びて出る"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = MathF.Max(.012f, Q(f, Back(Prog(f)))); return k.Matrix("m", k.Affine("a", i, Mv.Scale(f.C, s, 1f)), Alpha(Q(f, MathF.Min(1f, Prog(f) * 8f)))); } }

[VideoEffect("ぼかしから出る", ["演出：出入り"], ["ぼかし", "blur", "ピント", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class BlurInEffect : FxInOut
{
    public override string Label => "ぼかしから出る";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = EaseOut(Prog(f)), s = (1f - q) * Math.Min(P(f), 1f) * MathF.Min(f.W, f.H) * .09f;
        return k.Matrix("m", k.Blur("b", i, s), Alpha(Q(f, MathF.Min(1f, Prog(f) * 2.5f))));
    }
}

[VideoEffect("白く光って出る", ["演出：出入り"], ["白", "光る", "flash", "フラッシュ", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class WhiteFlashInEffect : FxInOut
{
    public override string Label => "白く光って出る";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = Prog(f), w = (1f - EaseOut(q)) * Math.Min(P(f), 1f);
        return k.Matrix("t", k.Matrix("m", i, Mx.Mix(Mx.Out(0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1), w)), Alpha(Q(f, MathF.Min(1f, q * 5f))));
    }
}

[VideoEffect("モザイクから出る", ["演出：出入り"], ["モザイク", "mosaic", "ピクセル", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class MosaicInEffect : FxInOut
{
    public override string Label => "モザイクから出る";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = EaseOut(Prog(f)), cell = (1f - q) * Math.Min(P(f), 1f) * MathF.Min(f.W, f.H) * .16f;
        var fade = Alpha(Q(f, MathF.Min(1f, Prog(f) * 3f)));
        if (cell < 1.5f) return k.Matrix("m", i, fade);
        float s = 1f / cell; var c = f.C;
        var small = k.Affine("s", k.Blur("b", i, cell * .35f), Mv.Scale(c, s, s));
        return k.Matrix("m", k.Affine("l", small, Mv.Scale(c, cell, cell), true), fade);
    }
}

public abstract class FxMaskIn : FxInOut
{
    protected abstract void Mask(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, RawRectF r, float q);
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = Q(f, EaseOut(Prog(f)));
        if (q >= .999f) return i;
        var r = f.B;
        var m = k.Draw(r, dc => { using var b = dc.CreateSolidColorBrush(new Color4(1, 1, 1, 1)); Mask(dc, b, r, q); });
        return k.Comp("c", m, i, CompositeMode.SourceIn);
    }
}

[VideoEffect("円で開く", ["演出：出入り"], ["円", "アイリス", "iris", "丸く開く", "ワイプ", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class IrisInEffect : FxMaskIn
{
    public override string Label => "円で開く";
    protected override void Mask(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, RawRectF r, float q) { float w = r.Right - r.Left, h = r.Bottom - r.Top, R = MathF.Sqrt(w * w + h * h) * .52f * q; dc.FillEllipse(new Ellipse(new Vector2((r.Left + r.Right) / 2f, (r.Top + r.Bottom) / 2f), R, R), b); }
}

[VideoEffect("斜めワイプ", ["演出：出入り"], ["斜め", "ワイプ", "wipe", "切り替え", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class DiagonalWipeEffect : FxMaskIn
{
    public override string Label => "斜めワイプ";
    protected override void Mask(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, RawRectF r, float q)
    {
        float w = r.Right - r.Left, h = r.Bottom - r.Top, x = r.Left - h * .4f + (w + h * .8f) * q;
        YMM4EffectShapes.Gfx.FillPoly(dc, b, new List<Vector2> { new(r.Left - h, r.Top), new(x + h * .4f, r.Top), new(x - h * .4f, r.Bottom), new(r.Left - h, r.Bottom) });
    }
}
