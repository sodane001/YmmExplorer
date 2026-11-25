using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 29〜46: 音質

/// <summary>山のフィルターを並べる(強さで量が変わる)</summary>
internal sealed class EqK(FxA1 e, params (double f, double q, double db)[] bands) : KB(e)
{
    readonly StereoBiquad[] fx = bands.Select(_ => new StereoBiquad()).ToArray();
    protected override void Init() { foreach (var x in fx) x.Reset(); }
    protected override void Tick() { for (int i = 0; i < fx.Length; i++) fx[i].Peak(bands[i].f, Hz, bands[i].q, bands[i].db * A * 2); }
    protected override void Run(ref float l, ref float r) { foreach (var x in fx) x.Run(ref l, ref r); }
}

[AudioEffect("低音を強く", ["演出：音質"], ["低音", "bass", "ベース", "迫力", "EQ", "イコライザー", "演出"])]
public sealed class BassBoostEffect : FxA2 { public override string Label => "低音を強く"; internal override AudioKernel CreateKernel() => new EqK(this, (90, .7, 7), (180, .9, 2)); }

[AudioEffect("高音を強く", ["演出：音質"], ["高音", "treble", "明るい", "EQ", "イコライザー", "演出"])]
public sealed class TrebleBoostEffect : FxA2 { public override string Label => "高音を強く"; internal override AudioKernel CreateKernel() => new EqK(this, (5000, .7, 3), (10000, .7, 6)); }

[AudioEffect("中音を強く", ["演出：音質"], ["中音", "ミッド", "mid", "声の芯", "EQ", "イコライザー", "演出"])]
public sealed class MidBoostEffect : FxA2 { public override string Label => "中音を強く"; internal override AudioKernel CreateKernel() => new EqK(this, (1200, .8, 6)); }

[AudioEffect("こもりを取る", ["演出：音質"], ["こもり", "もやもや", "すっきり", "EQ", "声", "演出"])]
public sealed class DeMudEffect : FxA2 { public override string Label => "こもりを取る"; internal override AudioKernel CreateKernel() => new EqK(this, (320, 1.1, -6), (600, 1.2, -2)); }

[AudioEffect("ドンシャリ", ["演出：音質"], ["ドンシャリ", "低音", "高音", "派手", "EQ", "演出"])]
public sealed class SmileEqEffect : FxA2 { public override string Label => "ドンシャリ"; internal override AudioKernel CreateKernel() => new EqK(this, (80, .8, 6), (900, .6, -4), (9000, .7, 6)); }

[AudioEffect("声をくっきり", ["演出：音質"], ["声", "くっきり", "聞き取りやすい", "明瞭", "ボイス", "EQ", "演出"])]
public sealed class VoiceClarityEffect : FxA2
{
    public override string Label => "声をくっきり";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad hp = new(), a = new(), b = new();
        protected override void Init() { hp.Reset(); a.Reset(); b.Reset(); }
        protected override void Tick() { hp.HighPass(70 + A * 80, Hz); a.Peak(3000, Hz, .9, A * 8); b.Peak(280, Hz, 1, -A * 4); }
        protected override void Run(ref float l, ref float r) { hp.Run(ref l, ref r); b.Run(ref l, ref r); a.Run(ref l, ref r); }
    }
}

[AudioEffect("温かい音", ["演出：音質"], ["温かい", "柔らかい", "丸い音", "warm", "EQ", "演出"])]
public sealed class WarmToneEffect : FxA2
{
    public override string Label => "温かい音";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad lo = new(), hi = new();
        protected override void Init() { lo.Reset(); hi.Reset(); }
        protected override void Tick() { lo.Peak(220, Hz, .8, A * 6); hi.HighShelf(4500, Hz, -A * 12); }
        protected override void Run(ref float l, ref float r) { lo.Run(ref l, ref r); hi.Run(ref l, ref r); }
    }
}

internal sealed class SweepK(FxA2 e, bool open) : KB(e)
{
    readonly StereoBiquad f = new();
    protected override void Init() => f.Reset();
    protected override void Tick()
    {
        double len = Math.Max(0.05, 0.1 + A * 6), k = Math.Clamp((open ? Sec : Dur - Sec) / len, 0, 1);
        f.LowPass(Exp(180, 20000, open ? k : k), Hz, 1.1);
    }
    protected override void Run(ref float l, ref float r) => f.Run(ref l, ref r);
}

[AudioEffect("フィルターが開く", ["演出：音質"], ["フィルター", "開く", "こもった音から", "イントロ", "filter", "演出"])]
public sealed class FilterOpenEffect : FxA2 { public override string Label => "フィルターが開く"; internal override AudioKernel CreateKernel() => new SweepK(this, true); }

[AudioEffect("フィルターが閉じる", ["演出：音質"], ["フィルター", "閉じる", "こもる", "アウトロ", "filter", "演出"])]
public sealed class FilterCloseEffect : FxA2 { public override string Label => "フィルターが閉じる"; internal override AudioKernel CreateKernel() => new SweepK(this, false); }

[AudioEffect("ワウ", ["演出：音質"], ["ワウ", "wah", "ギター", "音色が動く", "演出"])]
public sealed class WahEffect : FxA3
{
    public override string Label => "ワウ";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        readonly StereoBiquad f = new(); Lfo lfo; int n;
        protected override void Init() { f.Reset(); lfo.Reset(); }
        protected override void Tick() { lfo.Step((0.3 + S * 6) * 128, Hz); f.BandPass(Exp(350, 350 + 150 + A * 3000, lfo.Uni), Hz, 2.6); n++; }
        protected override void Run(ref float l, ref float r) { f.Run(ref l, ref r); l *= 2.2f; r *= 2.2f; }
    }
}

[AudioEffect("オートワウ", ["演出：音質"], ["オートワウ", "auto wah", "音に反応", "ファンク", "演出"])]
public sealed class AutoWahEffect : FxA2
{
    public override string Label => "オートワウ";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad f = new(); readonly Envelope env = new(); float last;
        protected override void Init() { f.Reset(); env.Reset(); env.Set(6, 140, Hz); }
        protected override void Tick() { f.BandPass(300 + Math.Min(1f, last * (2 + A * 10)) * (600 + A * 3600), Hz, 2.4); }
        protected override void Run(ref float l, ref float r) { last = env.Run((l + r) * .5f); f.Run(ref l, ref r); l *= 2.2f; r *= 2.2f; }
    }
}

[AudioEffect("帯域だけ残す", ["演出：音質"], ["バンドパス", "bandpass", "帯域", "細い音", "フィルター", "演出"])]
public sealed class BandOnlyEffect : FxA2
{
    public override string Label => "帯域だけ残す";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad f = new();
        protected override void Init() => f.Reset();
        protected override void Tick() => f.BandPass(Exp(120, 8000, A), Hz, 1.4);
        protected override void Run(ref float l, ref float r) { f.Run(ref l, ref r); l *= 1.6f; r *= 1.6f; }
    }
}

internal sealed class FadeK(FxA2 e, bool fadeIn) : KB(e)
{
    float g, target; double coef;
    protected override void Init() { g = -1; coef = Math.Exp(-1.0 / (0.004 * Hz)); }
    protected override void Tick()
    {
        double len = 0.05 + A * 6, k = Math.Clamp((fadeIn ? Sec : Dur - Sec) / len, 0, 1);
        target = (float)(k * k * (3 - 2 * k));
        if (g < 0) g = target;
    }
    protected override void Run(ref float l, ref float r) { g = (float)(coef * g + (1 - coef) * target); l *= g; r *= g; }
}

[AudioEffect("フェードイン", ["演出：音量・定位"], ["フェードイン", "fade in", "だんだん大きく", "音量", "演出"])]
public sealed class AudioFadeInEffect : FxA2 { public override string Label => "フェードイン"; internal override AudioKernel CreateKernel() => new FadeK(this, true); }

[AudioEffect("フェードアウト", ["演出：音量・定位"], ["フェードアウト", "fade out", "だんだん小さく", "音量", "演出"])]
public sealed class AudioFadeOutEffect : FxA2 { public override string Label => "フェードアウト"; internal override AudioKernel CreateKernel() => new FadeK(this, false); }

[AudioEffect("左右の位置（パン）", ["演出：音量・定位"], ["パン", "pan", "左右", "定位", "ステレオ", "演出"])]
public sealed class StaticPanEffect : FxA2
{
    public override string Label => "左右の位置（パン）";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        float gl = 1, gr = 1;
        protected override void Tick() { double a = A * Math.PI / 2; gl = (float)(Math.Cos(a) * Math.Sqrt(2)); gr = (float)(Math.Sin(a) * Math.Sqrt(2)); }
        protected override void Run(ref float l, ref float r) { float m = (l + r) * .5f; l = m * gl; r = m * gr; }
    }
}

[AudioEffect("オートパン", ["演出：音量・定位"], ["オートパン", "auto pan", "左右に動く", "ステレオ", "演出"])]
public sealed class AutoPanEffect : FxA3
{
    public override string Label => "オートパン";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        Lfo lfo; float gl = 1, gr = 1;
        protected override void Init() => lfo.Reset();
        protected override void Tick() { lfo.Step((0.1 + S * 4) * 128, Hz); double a = (0.5 + lfo.Sin * .5 * Math.Max(.05f, A * 2 > 1 ? 1 : A * 2)) * Math.PI / 2; gl = (float)(Math.Cos(a) * Math.Sqrt(2)); gr = (float)(Math.Sin(a) * Math.Sqrt(2)); }
        protected override void Run(ref float l, ref float r) { float m = (l + r) * .5f; l = m * gl; r = m * gr; }
    }
}

[AudioEffect("モノラル化", ["演出：音量・定位"], ["モノラル", "mono", "左右を混ぜる", "片耳", "演出"])]
public sealed class MonoMixEffect : FxA1
{
    public override string Label => "モノラル化";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA1 e) : KB(e) { protected override void Run(ref float l, ref float r) { l = r = (l + r) * .5f; } }
}

[AudioEffect("左右入れ替え", ["演出：音量・定位"], ["左右入れ替え", "swap", "ステレオ", "演出"])]
public sealed class SwapChannelsEffect : FxA1
{
    public override string Label => "左右入れ替え";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA1 e) : KB(e) { protected override void Run(ref float l, ref float r) => (l, r) = (r, l); }
}
