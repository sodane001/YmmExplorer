using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using WpfColor = System.Windows.Media.Color;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  追加エフェクト(映像 37番〜・音声 29番〜)の共通の土台。使う項目の数ごとに基底を分ける
// =====================================================================================================

/// <summary>映像: 強さ</summary>
public abstract class FxVP : FxVideoEffectBase
{
    [Display(GroupName = "効果", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Power { get; } = new Animation(100, 0, 300);
    protected virtual IEnumerable<IAnimatable> Extra => [];
    protected sealed override IEnumerable<IAnimatable> GetAnimatables() => [Power, .. Extra];
    protected float P(in FxFrame f) => MathF.Max(0f, f.V(Power) / 100f);
}

/// <summary>映像: 強さ・大きさ</summary>
public abstract class FxVS : FxVP
{
    [Display(GroupName = "効果", Name = "大きさ・細かさ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Size { get; } = new Animation(50, 0, 300);
    protected override IEnumerable<IAnimatable> Extra => [Size];
    protected float Z(in FxFrame f) => MathF.Max(0f, f.V(Size) / 100f);
}

/// <summary>映像: 強さ・大きさ・速さ</summary>
public abstract class FxVA : FxVS
{
    [Display(GroupName = "効果", Name = "速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, -1000, 1000);
    protected override IEnumerable<IAnimatable> Extra => [Size, Speed];
    protected float Sp(in FxFrame f) => f.V(Speed) / 100f;
}

/// <summary>映像: 強さ・色</summary>
public abstract class FxVC : FxVP
{
    [Display(GroupName = "効果", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x5C, 0x8A);
}

/// <summary>映像: 強さ・2色</summary>
public abstract class FxVCC : FxVC
{
    [Display(GroupName = "効果", Name = "2つめの色")]
    [ColorPicker]
    public WpfColor Color2 { get => color2; set => Set(ref color2, value); }
    WpfColor color2 = WpfColor.FromRgb(0xFF, 0xE9, 0xA8);
}

/// <summary>映像: 強さ・大きさ・色</summary>
public abstract class FxVSC : FxVS
{
    [Display(GroupName = "効果", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);
}

/// <summary>映像: 強さ・大きさ・速さ・色</summary>
public abstract class FxVAC : FxVA
{
    [Display(GroupName = "効果", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);
}

/// <summary>色の行列を作る道具</summary>
internal static class Mx
{
    public static readonly float[] Id = Out(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);

    /// <summary>出力の赤 = r0*R + r1*G + r2*B + r3 のように書く</summary>
    public static float[] Out(float r0, float r1, float r2, float r3, float g0, float g1, float g2, float g3, float b0, float b1, float b2, float b3)
        => [r0, g0, b0, 0, r1, g1, b1, 0, r2, g2, b2, 0, 0, 0, 0, 1, r3, g3, b3, 0];

    /// <summary>元の色(0)と m(1)の間を混ぜる</summary>
    public static float[] Mix(float[] m, float p)
    {
        var o = new float[20];
        for (int i = 0; i < 20; i++) o[i] = Id[i] + (m[i] - Id[i]) * p;
        return o;
    }

    /// <summary>色相を回す(角度はラジアン)</summary>
    public static float[] Hue(float a)
    {
        float c = MathF.Cos(a), s = MathF.Sin(a);
        return Out(0.213f + c * 0.787f - s * 0.213f, 0.715f - c * 0.715f - s * 0.715f, 0.072f - c * 0.072f + s * 0.928f, 0,
                   0.213f - c * 0.213f + s * 0.143f, 0.715f + c * 0.285f + s * 0.140f, 0.072f - c * 0.072f - s * 0.283f, 0,
                   0.213f - c * 0.213f - s * 0.787f, 0.715f - c * 0.715f + s * 0.715f, 0.072f + c * 0.928f + s * 0.072f, 0);
    }
}

/// <summary>音声: 効果の量</summary>
public abstract class FxA1 : FxAudioEffectBase
{
    [Display(GroupName = "効果", Name = "効果の量", Description = "0%で元の音、100%で効果だけ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Mix { get; } = new Animation(100, 0, 100);
    protected virtual IEnumerable<IAnimatable> Extra => [];
    protected sealed override IEnumerable<IAnimatable> GetAnimatables() => [Mix, .. Extra];
}

/// <summary>音声: 効果の量・強さ</summary>
public abstract class FxA2 : FxA1
{
    [Display(GroupName = "効果", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Amount { get; } = new Animation(50, 0, 100);
    protected override IEnumerable<IAnimatable> Extra => [Amount];
}

/// <summary>音声: 効果の量・強さ・速さ</summary>
public abstract class FxA3 : FxA2
{
    [Display(GroupName = "効果", Name = "速さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Rate { get; } = new Animation(50, 0, 100);
    protected override IEnumerable<IAnimatable> Extra => [Amount, Rate];
}

/// <summary>追加の音声エフェクトの処理の土台。Mx=効果の量、A=強さ(0〜1)、S=速さ(0〜1)</summary>
internal abstract class KB(FxA1 e) : AudioKernel
{
    protected int Hz = 48000;
    protected float Mx, A, S;
    protected long Frame, Total;
    protected float Sec => (float)Frame / Math.Max(1, Hz);
    protected float Dur => (float)Total / Math.Max(1, Hz);

    public override void Reset(int hz) { Hz = hz; Init(); }
    public override void Params(long frame, long total, int hz)
    {
        Hz = hz; Frame = frame; Total = total;
        Mx = Math.Clamp((float)e.Mix.GetValue(frame, total, hz) / 100f, 0f, 1f);
        if (e is FxA2 a) A = Math.Clamp((float)a.Amount.GetValue(frame, total, hz) / 100f, 0f, 1f);
        if (e is FxA3 b) S = Math.Clamp((float)b.Rate.GetValue(frame, total, hz) / 100f, 0f, 1f);
        Tick();
    }
    public sealed override void Process(ref float l, ref float r)
    {
        float dl = l, dr = r;
        Run(ref l, ref r);
        l = dl + (l - dl) * Mx;
        r = dr + (r - dr) * Mx;
    }
    protected virtual void Init() { }
    protected virtual void Tick() { }
    protected abstract void Run(ref float l, ref float r);
    /// <summary>A=0 で lo、A=1 で hi(掛け算の補間。周波数に使う)</summary>
    protected static double Exp(double lo, double hi, double k) => lo * Math.Pow(hi / lo, Math.Clamp(k, 0, 1));
}

/// <summary>ゆっくりした揺れ(0〜1 周期)</summary>
internal struct Lfo
{
    double ph;
    public void Reset(double start = 0) { ph = start; }
    public double Step(double freq, int hz) { ph += freq / Math.Max(1, hz); ph -= Math.Floor(ph); return ph; }
    public readonly double Sin => Math.Sin(ph * Math.PI * 2);
    public readonly double Uni => 0.5 + 0.5 * Math.Sin(ph * Math.PI * 2);
    public readonly double Phase => ph;
}
