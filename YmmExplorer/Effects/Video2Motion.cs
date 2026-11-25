using System.Numerics;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 53〜68: 動き

internal static class Mv
{
    public static Matrix3x2 Scale(Vector2 c, float x, float y) => Matrix3x2.CreateScale(x, y, c);
    public static Matrix3x2 Rot(Vector2 c, float a) => Matrix3x2.CreateRotation(a, c);
    public static Matrix3x2 Move(float x, float y) => Matrix3x2.CreateTranslation(x, y);
    public static Matrix3x2 SkewX(float yPivot, float k) => new(1, 0, k, 1, -k * yPivot, 0);
}

[VideoEffect("ふわふわ浮く", ["演出：動き"], ["浮く", "ふわふわ", "float", "漂う", "幽霊", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class FloatBobEffect : FxVA { public override string Label => "ふわふわ浮く"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float t = f.T * Sp(f), a = P(f) * Z(f) * f.H * .12f; return k.Affine("a", i, Mv.Move(MathF.Sin(t * 1.1f) * a * .35f, MathF.Sin(t * 2f) * a)); } }

[VideoEffect("呼吸（拡大縮小）", ["演出：動き"], ["呼吸", "拡大縮小", "脈打つ", "breath", "pulse", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class BreathEffect : FxVA { public override string Label => "呼吸（拡大縮小）"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = 1f + MathF.Sin(f.T * Sp(f) * 2.4f) * P(f) * Z(f) * .16f; return k.Affine("a", i, Mv.Scale(f.C, s, s)); } }

[VideoEffect("回り続ける", ["演出：動き"], ["回転", "回る", "spin", "rotate", "くるくる", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class SpinEffect : FxVA { public override string Label => "回り続ける"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Affine("a", i, Mv.Rot(f.C, f.T * Sp(f) * P(f) * MathF.Tau * (.1f + Z(f)))); }

[VideoEffect("振り子", ["演出：動き"], ["振り子", "pendulum", "ぶらぶら", "揺れる", "吊るす", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class PendulumEffect : FxVA { public override string Label => "振り子"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Affine("a", i, Mv.Rot(new Vector2(f.C.X, f.B.Top), MathF.Sin(f.T * Sp(f) * 3f) * P(f) * Z(f) * .7f)); }

[VideoEffect("ぴょんぴょん跳ねる", ["演出：動き"], ["跳ねる", "ジャンプ", "ぴょんぴょん", "bounce", "hop", "喜ぶ", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class HopEffect : FxVA
{
    public override string Label => "ぴょんぴょん跳ねる";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float s = MathF.Abs(MathF.Sin(f.T * Sp(f) * 4f)), h = s * P(f) * Z(f) * f.H * .5f, sq = MathF.Pow(1f - s, 5f) * P(f) * .16f;
        return k.Affine("a", i, Mv.Scale(new Vector2(f.C.X, f.B.Bottom), 1f + sq, 1f - sq) * Mv.Move(0, -h));
    }
}

[VideoEffect("3Dフリップ", ["演出：動き"], ["フリップ", "flip", "カード", "横回転", "3D", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Flip3DEffect : FxVA
{
    public override string Label => "3Dフリップ";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float c = MathF.Cos(f.T * Sp(f) * (1f + Z(f) * 3f)), s = 1f + (c - 1f) * Math.Min(P(f), 1f);
        if (MathF.Abs(s) < .012f) s = .012f;
        return k.Matrix("m", k.Affine("a", i, Mv.Scale(f.C, s, 1f)), FxKit.Tone(-(1f - MathF.Abs(c)) * .22f * Math.Min(P(f), 1f)));
    }
}

[VideoEffect("つぶれて伸びる", ["演出：動き"], ["つぶれる", "伸びる", "むにゅ", "squash", "stretch", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class SquashEffect : FxVA { public override string Label => "つぶれて伸びる"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float q = MathF.Sin(f.T * Sp(f) * 4f) * P(f) * Z(f) * .45f; return k.Affine("a", i, Mv.Scale(new Vector2(f.C.X, f.B.Bottom), 1f + q, 1f / (1f + q))); } }

[VideoEffect("地震（横揺れ）", ["演出：動き"], ["地震", "横揺れ", "earthquake", "揺れ", "振動", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class EarthquakeEffect : FxVA { public override string Label => "地震（横揺れ）"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float t = f.T * Sp(f) * 32f, a = P(f) * Z(f) * f.W * .05f; return k.Affine("a", i, Mv.Move(FxKit.Noise(t, 3) * a, FxKit.Noise(t * .8f, 9) * a * .2f)); } }

[VideoEffect("くねくね（斜め揺れ）", ["演出：動き"], ["くねくね", "斜め", "skew", "ダンス", "揺れ", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class WiggleSkewEffect : FxVA { public override string Label => "くねくね（斜め揺れ）"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Affine("a", i, Mv.SkewX(f.B.Bottom, MathF.Sin(f.T * Sp(f) * 4f) * P(f) * Z(f) * .6f)); }

[VideoEffect("ゆっくりズーム", ["演出：動き"], ["ズーム", "ケンバーンズ", "ken burns", "写真を動かす", "zoom", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class SlowZoomEffect : FxVS { public override string Label => "ゆっくりズーム"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float q = f.Dur > 0 ? Math.Clamp(f.T / f.Dur, 0f, 1f) : 0f, s = 1f + P(f) * (.04f + Z(f) * .5f) * q; return k.Affine("a", i, Mv.Scale(f.C, s, s)); } }

[VideoEffect("ゆっくり横移動", ["演出：動き"], ["横移動", "パン", "pan", "写真を動かす", "ケンバーンズ", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class SlowPanEffect : FxVS
{
    public override string Label => "ゆっくり横移動";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float q = f.Dur > 0 ? Math.Clamp(f.T / f.Dur, 0f, 1f) : 0f, d = P(f) * Z(f) * f.W * .3f;
        return k.Affine("a", i, Mv.Scale(f.C, 1f + P(f) * Z(f) * .6f, 1f + P(f) * Z(f) * .6f) * Mv.Move(d * (.5f - q), 0));
    }
}

[VideoEffect("走行の振動", ["演出：動き"], ["走行", "乗り物", "電車", "車", "振動", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class RideShakeEffect : FxVA
{
    public override string Label => "走行の振動";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float t = f.T * Sp(f), a = P(f) * Z(f) * f.H * .025f, bump = MathF.Pow(MathF.Max(0f, MathF.Sin(t * 5.3f)), 16f);
        return k.Affine("a", i, Mv.Move(FxKit.Noise(t * 9f, 5) * a * .35f, MathF.Sin(t * 42f) * a * .55f + bump * a * 2f));
    }
}

[VideoEffect("酔っぱらい", ["演出：動き"], ["酔っぱらい", "ふらふら", "めまい", "drunk", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class DrunkEffect : FxVA
{
    public override string Label => "酔っぱらい";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float t = f.T * Sp(f) * 1.5f, a = P(f) * Z(f);
        var m = Mv.Rot(f.C, FxKit.Noise(t, 11) * a * .36f) * Mv.Scale(f.C, 1f + FxKit.Noise(t * .7f, 12) * a * .12f, 1f + FxKit.Noise(t * .8f, 13) * a * .12f) * Mv.Move(FxKit.Noise(t, 14) * a * f.W * .1f, FxKit.Noise(t, 15) * a * f.H * .08f);
        return k.Comp("c", k.Affine("a", i, m), k.Matrix("g", k.Affine("b", i, Mv.Move(a * f.W * .025f, 0) * m), FxKit.Tone(0, 1, 1, .32f * Math.Min(P(f), 1f))));
    }
}

[VideoEffect("左右反転", ["演出：動き"], ["左右反転", "ミラー", "mirror", "flip", "反転", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class MirrorHEffect : FxVP { public override string Label => "左右反転"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = 1f - 2f * Math.Min(P(f), 1f); if (MathF.Abs(s) < .012f) s = .012f; return k.Affine("a", i, Mv.Scale(f.C, s, 1f)); } }

[VideoEffect("上下反転", ["演出：動き"], ["上下反転", "逆さま", "flip", "反転", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class MirrorVEffect : FxVP { public override string Label => "上下反転"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = 1f - 2f * Math.Min(P(f), 1f); if (MathF.Abs(s) < .012f) s = .012f; return k.Affine("a", i, Mv.Scale(f.C, 1f, s)); } }

[VideoEffect("分身", ["演出：動き"], ["分身", "クローン", "増える", "残像", "clone", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class CloneSideEffect : FxVA
{
    public override string Label => "分身";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float p = Math.Min(P(f), 1f), d = f.W * (.12f + Z(f) * .5f) * (1f + MathF.Sin(f.T * Sp(f) * 2f) * .12f);
        ID2D1Image o = k.Matrix("t2", k.Affine("a2", i, Mv.Move(-d * 2f, 0)), FxKit.Tone(0, 1, 1, .22f * p));
        o = k.Comp("c1", o, k.Matrix("t3", k.Affine("a3", i, Mv.Move(d * 2f, 0)), FxKit.Tone(0, 1, 1, .22f * p)));
        o = k.Comp("c2", o, k.Matrix("t4", k.Affine("a4", i, Mv.Move(-d, 0)), FxKit.Tone(0, 1, 1, .48f * p)));
        o = k.Comp("c3", o, k.Matrix("t5", k.Affine("a5", i, Mv.Move(d, 0)), FxKit.Tone(0, 1, 1, .48f * p)));
        return k.Comp("c4", o, i);
    }
}
