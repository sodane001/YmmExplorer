using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;
using WpfColor = System.Windows.Media.Color;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  動き
// =====================================================================================================

[VideoEffect("手ぶれ・カメラシェイク", ["演出：動き"], ["手ぶれ", "カメラシェイク", "camera shake", "揺れ", "振動", "地震", "衝撃", "演出"], IsAviUtlSupported = false)]
public sealed class CameraShakeEffect : FxVideoEffectBase
{
    public override string Label => "手ぶれ・カメラシェイク";

    [Display(GroupName = "手ぶれ", Name = "揺れの大きさ")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation Amount { get; } = new Animation(15, 0, 2000);

    [Display(GroupName = "手ぶれ", Name = "回転の揺れ")]
    [AnimationSlider("F1", "°", 0, 10)]
    public Animation Rotate { get; } = new Animation(1, 0, 180);

    [Display(GroupName = "手ぶれ", Name = "揺れの速さ")]
    [AnimationSlider("F1", "", 0.1, 30)]
    public Animation Speed { get; } = new Animation(8, 0, 200);

    [Display(GroupName = "手ぶれ", Name = "だんだん弱く", Description = "0で同じ強さのまま。大きいほど早くおさまります(地震・衝撃向け)")]
    [AnimationSlider("F1", "", 0, 10)]
    public Animation Decay { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "手ぶれ", Name = "拡大して端を隠す", Description = "揺れて画面の端が見えないように、少し拡大します(全画面の素材向け)")]
    [AnimationSlider("F0", "%", 0, 30)]
    public Animation Cover { get; } = new Animation(3, 0, 200);

    [Display(GroupName = "手ぶれ", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, Rotate, Speed, Decay, Cover];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float t = f.T * f.V(Speed), env = MathF.Exp(-f.T * f.V(Decay));
        float amt = f.V(Amount) * env, rot = f.V(Rotate) * env * MathF.PI / 180f, cover = 1f + f.V(Cover) / 100f;
        var off = new Vector2(FxKit.Noise(t, seed * 3 + 1), FxKit.Noise(t, seed * 3 + 2)) * amt;
        var c = f.C;
        var m = Matrix3x2.CreateScale(cover, cover, c) * Matrix3x2.CreateRotation(FxKit.Noise(t * 0.8f, seed * 3 + 3) * rot, c) * Matrix3x2.CreateTranslation(off);
        return k.Affine("shake", input, m);
    }
}

[VideoEffect("ズームパンチ", ["演出：動き"], ["ズームパンチ", "zoom punch", "ドン", "寄る", "拡大", "衝撃", "強調", "ツッコミ", "演出"], IsAviUtlSupported = false)]
public sealed class ZoomPunchEffect : FxVideoEffectBase
{
    public override string Label => "ズームパンチ";

    [Display(GroupName = "ズームパンチ", Name = "開始", Description = "アイテムの先頭から何秒後にドンと寄るか")]
    [AnimationSlider("F2", "秒", 0, 10)]
    public Animation Start { get; } = new Animation(0, 0, 3600);

    [Display(GroupName = "ズームパンチ", Name = "寄る量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Amount { get; } = new Animation(20, -90, 1000);

    [Display(GroupName = "ズームパンチ", Name = "戻る時間")]
    [AnimationSlider("F2", "秒", 0.05, 3)]
    public Animation Back { get; } = new Animation(0.5, 0.01, 60);

    [Display(GroupName = "ズームパンチ", Name = "くり返す間隔", Description = "0で1回だけ。拍に合わせてくり返す時は、その秒数")]
    [AnimationSlider("F2", "秒", 0, 4)]
    public Animation Interval { get; } = new Animation(0, 0, 600);

    [Display(GroupName = "ズームパンチ", Name = "中心の横位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation X { get; } = new Animation(50, -200, 300);

    [Display(GroupName = "ズームパンチ", Name = "中心の縦位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Y { get; } = new Animation(50, -200, 300);

    [Display(GroupName = "ズームパンチ", Name = "回転のひねり")]
    [AnimationSlider("F1", "°", -15, 15)]
    public Animation Twist { get; } = new Animation(0, -180, 180);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Start, Amount, Back, Interval, X, Y, Twist];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float tau = f.T - f.V(Start), iv = f.V(Interval);
        if (iv > 0.01f && tau > 0) tau %= iv;
        float env = 0f;
        const float rise = 0.06f;
        if (tau >= 0)
            env = tau < rise ? tau / rise : MathF.Exp(-(tau - rise) / MathF.Max(0.01f, f.V(Back) / 4f));
        float s = 1f + f.V(Amount) / 100f * env;
        var c = f.At(f.V(X) / 100f, f.V(Y) / 100f);
        var m = Matrix3x2.CreateScale(s, s, c) * Matrix3x2.CreateRotation(f.V(Twist) * env * MathF.PI / 180f, c);
        return k.Affine("punch", input, m);
    }
}

[VideoEffect("モーションブラー", ["演出：動き"], ["モーションブラー", "motion blur", "ブレ", "スピード", "速い", "ぼかし", "流れる", "演出"], IsAviUtlSupported = false)]
public sealed class MotionBlurEffect : FxVideoEffectBase
{
    public override string Label => "モーションブラー";

    [Display(GroupName = "モーションブラー", Name = "強さ")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation Amount { get; } = new Animation(20, 0, 1000);

    [Display(GroupName = "モーションブラー", Name = "向き", Description = "0°で横、90°で縦にぶれます")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "モーションブラー", Name = "元の絵を残す", Description = "ぶれた絵に、くっきりした元の絵を重ねる量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Keep { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, Angle, Keep];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var blur = k.DirBlur("dir", input, f.V(Amount) / 2f, f.V(Angle));
        float keep = f.V(Keep) / 100f;
        if (keep <= 0.001f) return blur;
        return k.Comp("keep", blur, k.Matrix("alpha", input, FxKit.Tone(0, 1, 1, keep)), CompositeMode.SourceOver);
    }
}

[VideoEffect("残像（ゴースト）", ["演出：動き"], ["残像", "ゴースト", "ghost", "分身", "スピード", "高速移動", "ブレ", "演出"], IsAviUtlSupported = false)]
public sealed class AfterimageEffect : FxVideoEffectBase
{
    public override string Label => "残像（ゴースト）";

    [Display(GroupName = "残像", Name = "数")]
    [TextBoxSlider("F0", "個", 1, 12)]
    [Range(1, 30)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 4;

    [Display(GroupName = "残像", Name = "ずれ(横)", Description = "残像1つごとの横のずれ")]
    [AnimationSlider("F0", "px", -100, 100)]
    public Animation DX { get; } = new Animation(-25, -2000, 2000);

    [Display(GroupName = "残像", Name = "ずれ(縦)")]
    [AnimationSlider("F0", "px", -100, 100)]
    public Animation DY { get; } = new Animation(0, -2000, 2000);

    [Display(GroupName = "残像", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(55, 0, 100);

    [Display(GroupName = "残像", Name = "揺れ", Description = "残像が左右にゆれ動く量(0で止まったまま)")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Swing { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "残像", Name = "色を付ける")]
    [ToggleSlider]
    public bool UseTint { get => useTint; set => Set(ref useTint, value); }
    bool useTint = false;

    [Display(GroupName = "残像", Name = "残像の色")]
    [ColorPicker]
    public WpfColor Tint { get => tint; set => Set(ref tint, value); }
    WpfColor tint = WpfColor.FromRgb(90, 180, 255);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [DX, DY, Strength, Swing];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        int n = Math.Clamp(count, 1, 30);
        float dx = f.V(DX), dy = f.V(DY), s = f.V(Strength) / 100f, sw = f.V(Swing) / 100f, t = f.T;
        var b = f.B;
        float ex = MathF.Abs(dx) * n * (1 + sw), ey = MathF.Abs(dy) * n * (1 + sw);
        var big = new RawRectF(b.Left - ex, b.Top - ey, b.Right + ex, b.Bottom + ey);
        var copies = new ID2D1Image[n];
        var tc = FxKit.Rgb(tint);
        for (int i = 0; i < n; i++)
        {
            float a = s * (1f - (float)(i + 1) / (n + 1));
            copies[i] = k.Matrix("g" + i, input, useTint ? FxKit.Fill(tc.R, tc.G, tc.B, a) : FxKit.Tone(0, 1, 1, a));
        }
        return k.Draw(big, dc =>
        {
            for (int i = n - 1; i >= 0; i--)
            {
                float w = 1f + sw * MathF.Sin(t * 6f + i * 0.9f);
                dc.Transform = Matrix3x2.CreateTranslation(dx * (i + 1) * w, dy * (i + 1) * w);
                dc.DrawImage(copies[i], InterpolationMode.Linear, CompositeMode.SourceOver);
            }
            dc.Transform = Matrix3x2.Identity;
            dc.DrawImage(input, InterpolationMode.Linear, CompositeMode.SourceOver);
        }, clip: false);
    }
}

[VideoEffect("陽炎（ゆらゆら）", ["演出：動き"], ["陽炎", "かげろう", "heat haze", "ゆらゆら", "ゆがみ", "暑い", "蜃気楼", "水中", "演出"], IsAviUtlSupported = false)]
public sealed class HeatHazeEffect : FxVideoEffectBase
{
    public override string Label => "陽炎（ゆらゆら）";

    [Display(GroupName = "陽炎", Name = "ゆがみの強さ")]
    [AnimationSlider("F0", "px", 0, 60)]
    public Animation Amount { get; } = new Animation(12, 0, 1000);

    [Display(GroupName = "陽炎", Name = "ゆがみの細かさ")]
    [AnimationSlider("F0", "", 1, 100)]
    public Animation Detail { get; } = new Animation(20, 1, 1000);

    [Display(GroupName = "陽炎", Name = "上へ流れる速さ")]
    [AnimationSlider("F0", "px/秒", 0, 300)]
    public Animation Speed { get; } = new Animation(60, -5000, 5000);

    [Display(GroupName = "陽炎", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, Detail, Speed];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float freq = f.V(Detail) / 2000f;
        var map = k.Turb("turb", new Vector2(0, f.T * f.V(Speed)), MathF.Max(0.0005f, freq), 2, seed);
        return k.Displace("disp", input, map, f.V(Amount));
    }
}

public enum JellyAnchor
{
    [Display(Name = "下(足もと)")] Bottom,
    [Display(Name = "まん中")] Center,
}

[VideoEffect("ぷるん（弾む揺れ）", ["演出：動き"], ["ぷるん", "ぷるぷる", "弾む", "バウンド", "ゼリー", "jelly", "伸び縮み", "登場", "演出"], IsAviUtlSupported = false)]
public sealed class JellyEffect : FxVideoEffectBase
{
    public override string Label => "ぷるん（弾む揺れ）";

    [Display(GroupName = "ぷるん", Name = "開始", Description = "アイテムの先頭から何秒後に揺れ始めるか")]
    [AnimationSlider("F2", "秒", 0, 10)]
    public Animation Start { get; } = new Animation(0, 0, 3600);

    [Display(GroupName = "ぷるん", Name = "揺れの大きさ")]
    [AnimationSlider("F0", "%", 0, 50)]
    public Animation Amount { get; } = new Animation(15, 0, 200);

    [Display(GroupName = "ぷるん", Name = "揺れの速さ")]
    [AnimationSlider("F1", "回/秒", 0.5, 10)]
    public Animation Freq { get; } = new Animation(3.5, 0.1, 60);

    [Display(GroupName = "ぷるん", Name = "おさまる速さ")]
    [AnimationSlider("F1", "", 0, 10)]
    public Animation Decay { get; } = new Animation(3, 0, 100);

    [Display(GroupName = "ぷるん", Name = "くり返す間隔", Description = "0で1回だけ")]
    [AnimationSlider("F2", "秒", 0, 5)]
    public Animation Interval { get; } = new Animation(0, 0, 600);

    [Display(GroupName = "ぷるん", Name = "伸び縮みの中心")]
    [EnumComboBox]
    public JellyAnchor Anchor { get => anchor; set => Set(ref anchor, value); }
    JellyAnchor anchor = JellyAnchor.Bottom;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Start, Amount, Freq, Decay, Interval];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float tau = f.T - f.V(Start), iv = f.V(Interval);
        if (iv > 0.01f && tau > 0) tau %= iv;
        float a = tau < 0 ? 0f : f.V(Amount) / 100f * MathF.Exp(-tau * f.V(Decay)) * MathF.Sin(tau * f.V(Freq) * MathF.PI * 2f);
        var c = anchor == JellyAnchor.Bottom ? new Vector2(f.C.X, f.B.Bottom) : f.C;
        return k.Affine("jelly", input, Matrix3x2.CreateScale(1f - a * 0.7f, 1f + a, c));
    }
}
