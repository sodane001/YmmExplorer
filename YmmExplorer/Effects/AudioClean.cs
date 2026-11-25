using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  聞きやすく
// =====================================================================================================

[AudioEffect("音量を一定に（自動調整）", ["演出：聞きやすく"], ["音量を一定に", "ラウドネス", "ノーマライズ", "自動調整", "AGC", "音量", "大きさをそろえる", "演出"])]
public sealed class LevelerEffect : FxAudioEffectBase
{
    public override string Label => "音量を一定に（自動調整）";

    [Display(GroupName = "音量を一定に", Name = "目標の大きさ", Description = "この大きさに近づくように、自動で上げ下げします(-16dB くらいが動画向け)")]
    [AnimationSlider("F0", "dB", -30, -6)]
    public Animation Target { get; } = new Animation(-16, -60, 0);

    [Display(GroupName = "音量を一定に", Name = "最大の上げ幅")]
    [AnimationSlider("F0", "dB", 0, 30)]
    public Animation MaxBoost { get; } = new Animation(12, 0, 60);

    [Display(GroupName = "音量を一定に", Name = "最大の下げ幅")]
    [AnimationSlider("F0", "dB", 0, 30)]
    public Animation MaxCut { get; } = new Animation(12, 0, 60);

    [Display(GroupName = "音量を一定に", Name = "反応の速さ", Description = "小さいほど素早く合わせます(速すぎると、音量がゆれて聞こえます)")]
    [AnimationSlider("F0", "ms", 50, 2000)]
    public Animation Speed { get; } = new Animation(400, 10, 10000);

    [Display(GroupName = "音量を一定に", Name = "これより小さい音は上げない", Description = "無音やノイズまで大きくしないための境目")]
    [AnimationSlider("F0", "dB", -70, -20)]
    public Animation Gate { get; } = new Animation(-45, -100, 0);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Target, MaxBoost, MaxCut, Speed, Gate];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(LevelerEffect e) : AudioKernel
    {
        double ms, gain = 1, coef, gcoef;
        float target, boost, cut, gate;
        public override void Reset(int hz) { ms = 0; gain = 1; }
        public override void Params(long f, long t, int hz)
        {
            target = (float)e.Target.GetValue(f, t, hz); boost = (float)e.MaxBoost.GetValue(f, t, hz);
            cut = (float)e.MaxCut.GetValue(f, t, hz); gate = (float)e.Gate.GetValue(f, t, hz);
            double ms0 = Math.Max(10, e.Speed.GetValue(f, t, hz));
            coef = Math.Exp(-1.0 / (ms0 * 0.001 * hz));
            gcoef = Math.Exp(-1.0 / (ms0 * 0.0005 * hz));
        }
        public override void Process(ref float l, ref float r)
        {
            double x = (l * l + r * r) * 0.5;
            ms = coef * ms + (1 - coef) * x;
            float lev = Db.ToDb(Math.Sqrt(ms)) + 3f;
            if (lev > gate)
            {
                float want = Math.Clamp(target - lev, -cut, boost);
                gain = gcoef * gain + (1 - gcoef) * Db.ToGain(want);
            }
            l = Limit((float)(l * gain)); r = Limit((float)(r * gain));
        }
        static float Limit(float v) => MathF.Abs(v) < 0.9f ? v : MathF.Sign(v) * (0.9f + 0.1f * MathF.Tanh((MathF.Abs(v) - 0.9f) * 10f));
    }
}

[AudioEffect("コンプレッサー", ["演出：聞きやすく"], ["コンプレッサー", "compressor", "コンプ", "音圧", "音量差", "ダイナミクス", "演出"])]
public sealed class CompressorEffect : FxAudioEffectBase
{
    public override string Label => "コンプレッサー";

    [Display(GroupName = "コンプレッサー", Name = "しきい値", Description = "これより大きい音を小さくします")]
    [AnimationSlider("F0", "dB", -60, 0)]
    public Animation Threshold { get; } = new Animation(-20, -80, 0);

    [Display(GroupName = "コンプレッサー", Name = "比率", Description = "大きい音をどれだけ縮めるか(4 で 4分の1)")]
    [AnimationSlider("F1", ":1", 1, 20)]
    public Animation Ratio { get; } = new Animation(4, 1, 100);

    [Display(GroupName = "コンプレッサー", Name = "アタック")]
    [AnimationSlider("F1", "ms", 0.1, 100)]
    public Animation Attack { get; } = new Animation(10, 0.01, 1000);

    [Display(GroupName = "コンプレッサー", Name = "リリース")]
    [AnimationSlider("F0", "ms", 10, 1000)]
    public Animation Release { get; } = new Animation(150, 1, 5000);

    [Display(GroupName = "コンプレッサー", Name = "音量の補正", Description = "小さくなった分を、全体で持ち上げます")]
    [AnimationSlider("F1", "dB", 0, 24)]
    public Animation Makeup { get; } = new Animation(6, -24, 48);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Threshold, Ratio, Attack, Release, Makeup];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(CompressorEffect e) : AudioKernel
    {
        readonly Envelope env = new();
        float th, ratio, makeup;
        public override void Reset(int hz) => env.Reset();
        public override void Params(long f, long t, int hz)
        {
            th = (float)e.Threshold.GetValue(f, t, hz); ratio = (float)Math.Max(1, e.Ratio.GetValue(f, t, hz));
            makeup = Db.ToGain(e.Makeup.GetValue(f, t, hz));
            env.Set(e.Attack.GetValue(f, t, hz), e.Release.GetValue(f, t, hz), hz);
        }
        public override void Process(ref float l, ref float r)
        {
            float lev = Db.ToDb(env.Run(MathF.Max(MathF.Abs(l), MathF.Abs(r))));
            float over = lev - th;
            // 境目をなめらかに(ソフトニー 6dB)
            float red = over <= -3 ? 0 : over >= 3 ? over * (1 - 1 / ratio) : (over + 3) * (over + 3) / 12f * (1 - 1 / ratio);
            float g = Db.ToGain(-red) * makeup;
            l *= g; r *= g;
        }
    }
}

[AudioEffect("リミッター（音割れ防止）", ["演出：聞きやすく"], ["リミッター", "limiter", "音割れ", "クリップ", "上限", "音圧", "演出"])]
public sealed class LimiterEffect : FxAudioEffectBase
{
    public override string Label => "リミッター（音割れ防止）";

    [Display(GroupName = "リミッター", Name = "上限", Description = "これを超えないように抑えます")]
    [AnimationSlider("F1", "dB", -12, 0)]
    public Animation Ceiling { get; } = new Animation(-1, -40, 0);

    [Display(GroupName = "リミッター", Name = "入力を持ち上げる")]
    [AnimationSlider("F1", "dB", 0, 20)]
    public Animation Drive { get; } = new Animation(0, -20, 40);

    [Display(GroupName = "リミッター", Name = "リリース")]
    [AnimationSlider("F0", "ms", 10, 500)]
    public Animation Release { get; } = new Animation(80, 1, 5000);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Ceiling, Drive, Release];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(LimiterEffect e) : AudioKernel
    {
        readonly DelayLine dl = new(), dr = new();
        double gain = 1, rel;
        float ceil, drive;
        int look;
        public override int Latency => look;
        public override void Reset(int hz)
        {
            look = Math.Max(1, hz / 500);   // 2ms 先読み
            dl.Resize(look + 4); dr.Resize(look + 4); gain = 1;
        }
        public override void Params(long f, long t, int hz)
        {
            ceil = Db.ToGain(e.Ceiling.GetValue(f, t, hz)); drive = Db.ToGain(e.Drive.GetValue(f, t, hz));
            rel = Math.Exp(-1.0 / Math.Max(1, e.Release.GetValue(f, t, hz) * 0.001 * hz));
        }
        public override void Process(ref float l, ref float r)
        {
            float il = l * drive, ir = r * drive;
            float peak = MathF.Max(MathF.Abs(il), MathF.Abs(ir));
            double want = peak > ceil ? ceil / peak : 1;
            gain = want < gain ? want : rel * gain + (1 - rel) * want;      // すぐ下げて、ゆっくり戻す
            dl.Write(il); dr.Write(ir);
            l = Math.Clamp((float)(dl.Read(look) * gain), -ceil, ceil);
            r = Math.Clamp((float)(dr.Read(look) * gain), -ceil, ceil);
        }
    }
}

[AudioEffect("ノイズ除去", ["演出：聞きやすく"], ["ノイズ除去", "ノイズリダクション", "noise reduction", "サー", "ホワイトノイズ", "雑音", "ブーン", "演出"])]
public sealed class DenoiseEffect : FxAudioEffectBase
{
    public override string Label => "ノイズ除去";

    [Display(GroupName = "ノイズ除去", Name = "強さ", Description = "ずっと鳴っている小さな音(サー・ブーン)を消す強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Amount { get; } = new Animation(70, 0, 200);

    [Display(GroupName = "ノイズ除去", Name = "消しすぎ防止", Description = "大きいほど、声の不自然さ(水っぽさ)が減ります")]
    [AnimationSlider("F0", "%", 0, 50)]
    public Animation Floor { get; } = new Animation(12, 0, 100);

    [Display(GroupName = "ノイズ除去", Name = "低い音もカット", Description = "空調・振動などの低い音を消す周波数(0で使わない)")]
    [AnimationSlider("F0", "Hz", 0, 200)]
    public Animation LowCut { get; } = new Animation(70, 0, 1000);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, Floor, LowCut];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(DenoiseEffect e) : AudioKernel
    {
        readonly SpectralDenoiser dl = new(), dr = new();
        readonly StereoBiquad hp = new();
        float cut;
        public override int Latency => SpectralDenoiser.N - 1;
        public override void Reset(int hz) { dl.Reset(); dr.Reset(); hp.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            float a = (float)e.Amount.GetValue(f, t, hz) / 100f, fl = (float)e.Floor.GetValue(f, t, hz) / 100f;
            dl.Amount = dr.Amount = a; dl.Floor = dr.Floor = fl;
            cut = (float)e.LowCut.GetValue(f, t, hz);
            if (cut > 1) hp.HighPass(cut, hz);
        }
        public override void Process(ref float l, ref float r)
        {
            if (cut > 1) hp.Run(ref l, ref r);
            l = dl.Run(l); r = dr.Run(r);
        }
    }
}

[AudioEffect("ノイズゲート", ["演出：聞きやすく"], ["ノイズゲート", "noise gate", "ゲート", "無音", "息継ぎ", "雑音", "演出"])]
public sealed class NoiseGateEffect : FxAudioEffectBase
{
    public override string Label => "ノイズゲート";

    [Display(GroupName = "ノイズゲート", Name = "しきい値", Description = "これより小さい音を消します")]
    [AnimationSlider("F0", "dB", -80, -10)]
    public Animation Threshold { get; } = new Animation(-48, -100, 0);

    [Display(GroupName = "ノイズゲート", Name = "下げる量")]
    [AnimationSlider("F0", "dB", 0, 80)]
    public Animation Range { get; } = new Animation(40, 0, 120);

    [Display(GroupName = "ノイズゲート", Name = "開く速さ")]
    [AnimationSlider("F1", "ms", 0.1, 50)]
    public Animation Attack { get; } = new Animation(2, 0.01, 500);

    [Display(GroupName = "ノイズゲート", Name = "保持")]
    [AnimationSlider("F0", "ms", 0, 500)]
    public Animation Hold { get; } = new Animation(80, 0, 5000);

    [Display(GroupName = "ノイズゲート", Name = "閉じる速さ")]
    [AnimationSlider("F0", "ms", 10, 1000)]
    public Animation Release { get; } = new Animation(150, 1, 5000);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Threshold, Range, Attack, Hold, Release];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(NoiseGateEffect e) : AudioKernel
    {
        readonly Envelope env = new();
        double g = 1, att, rel; float th, floorG; int hold, held;
        public override void Reset(int hz) { env.Reset(); g = 1; held = 0; }
        public override void Params(long f, long t, int hz)
        {
            env.Set(1, 30, hz);
            th = Db.ToGain(e.Threshold.GetValue(f, t, hz)); floorG = Db.ToGain(-e.Range.GetValue(f, t, hz));
            att = Math.Exp(-1.0 / Math.Max(1, e.Attack.GetValue(f, t, hz) * 0.001 * hz));
            rel = Math.Exp(-1.0 / Math.Max(1, e.Release.GetValue(f, t, hz) * 0.001 * hz));
            hold = (int)(e.Hold.GetValue(f, t, hz) * 0.001 * hz);
        }
        public override void Process(ref float l, ref float r)
        {
            float lev = env.Run(MathF.Max(MathF.Abs(l), MathF.Abs(r)));
            double want;
            if (lev >= th) { want = 1; held = hold; }
            else if (held > 0) { want = 1; held--; }
            else want = floorG;
            g = want > g ? att * g + (1 - att) * want : rel * g + (1 - rel) * want;
            l *= (float)g; r *= (float)g;
        }
    }
}

[AudioEffect("歯擦音を抑える（ディエッサー）", ["演出：聞きやすく"], ["ディエッサー", "de-esser", "サ行", "歯擦音", "シー", "耳に痛い", "演出"])]
public sealed class DeEsserEffect : FxAudioEffectBase
{
    public override string Label => "歯擦音を抑える（ディエッサー）";

    [Display(GroupName = "ディエッサー", Name = "周波数", Description = "これより高い「サ・シ」の音を見張ります")]
    [AnimationSlider("F0", "Hz", 3000, 10000)]
    public Animation Freq { get; } = new Animation(6000, 1000, 20000);

    [Display(GroupName = "ディエッサー", Name = "しきい値")]
    [AnimationSlider("F0", "dB", -50, 0)]
    public Animation Threshold { get; } = new Animation(-28, -80, 0);

    [Display(GroupName = "ディエッサー", Name = "最大で下げる量")]
    [AnimationSlider("F0", "dB", 0, 24)]
    public Animation MaxCut { get; } = new Animation(10, 0, 48);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Freq, Threshold, MaxCut];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(DeEsserEffect e) : AudioKernel
    {
        readonly StereoBiquad hp = new();
        readonly Biquad det = new();
        readonly Envelope env = new();
        float th, maxCut;
        public override void Reset(int hz) { hp.Reset(); det.Reset(); env.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            double fr = e.Freq.GetValue(f, t, hz);
            hp.HighPass(fr, hz); det.HighPass(fr, hz);
            env.Set(0.5, 60, hz);
            th = (float)e.Threshold.GetValue(f, t, hz); maxCut = (float)e.MaxCut.GetValue(f, t, hz);
        }
        public override void Process(ref float l, ref float r)
        {
            float lev = Db.ToDb(env.Run(det.Run((l + r) * 0.5f)));
            float red = Math.Clamp((lev - th) * 0.7f, 0f, maxCut);
            float g = Db.ToGain(-red);
            float hl = l, hr = r;
            hp.Run(ref hl, ref hr);
            l -= hl * (1 - g); r -= hr * (1 - g);
        }
    }
}

public enum CutSlope
{
    [Display(Name = "ゆるやか(12dB/oct)")] Gentle,
    [Display(Name = "急(24dB/oct)")] Steep,
}

[AudioEffect("低音カット", ["演出：聞きやすく"], ["低音カット", "ローカット", "ハイパス", "high pass", "こもり", "ポップノイズ", "風の音", "演出"])]
public sealed class LowCutEffect : FxAudioEffectBase
{
    public override string Label => "低音カット";

    [Display(GroupName = "低音カット", Name = "周波数", Description = "これより低い音を減らします(声なら 80〜120Hz)")]
    [AnimationSlider("F0", "Hz", 20, 500)]
    public Animation Freq { get; } = new Animation(90, 10, 5000);

    [Display(GroupName = "低音カット", Name = "傾き")]
    [EnumComboBox]
    public CutSlope Slope { get => slope; set => Set(ref slope, value); }
    CutSlope slope = CutSlope.Gentle;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Freq];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(LowCutEffect e) : AudioKernel
    {
        readonly StereoBiquad a = new(), b = new();
        bool steep;
        public override void Reset(int hz) { a.Reset(); b.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            double fr = e.Freq.GetValue(f, t, hz);
            steep = e.Slope == CutSlope.Steep;
            a.HighPass(fr, hz, steep ? 0.541 : 0.7071); b.HighPass(fr, hz, 1.307);
        }
        public override void Process(ref float l, ref float r)
        {
            a.Run(ref l, ref r);
            if (steep) b.Run(ref l, ref r);
        }
    }
}
