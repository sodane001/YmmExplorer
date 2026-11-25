using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  声を変える
// =====================================================================================================

[AudioEffect("電話の声", ["演出：声を変える"], ["電話", "電話の声", "telephone", "通話", "スマホ", "受話器", "演出"])]
public sealed class TelephoneEffect : FxAudioEffectBase
{
    public override string Label => "電話の声";

    [Display(GroupName = "電話の声", Name = "低い側", Description = "これより低い音を消します")]
    [AnimationSlider("F0", "Hz", 100, 1000)]
    public Animation Low { get; } = new Animation(350, 20, 5000);

    [Display(GroupName = "電話の声", Name = "高い側", Description = "これより高い音を消します")]
    [AnimationSlider("F0", "Hz", 1500, 6000)]
    public Animation High { get; } = new Animation(3200, 500, 20000);

    [Display(GroupName = "電話の声", Name = "ざらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Grit { get; } = new Animation(25, 0, 100);

    [Display(GroupName = "電話の声", Name = "回線のノイズ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation NoiseAmt { get; } = new Animation(10, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Low, High, Grit, NoiseAmt];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(TelephoneEffect e) : AudioKernel
    {
        readonly Biquad h1 = new(), h2 = new(), l1 = new(), l2 = new(), pk = new();
        WhiteNoise rnd = new(77);
        float grit, noise;
        public override void Reset(int hz) { h1.Reset(); h2.Reset(); l1.Reset(); l2.Reset(); pk.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            double lo = e.Low.GetValue(f, t, hz), hi = e.High.GetValue(f, t, hz);
            h1.HighPass(lo, hz); h2.HighPass(lo, hz); l1.LowPass(hi, hz); l2.LowPass(hi, hz);
            pk.Peak(1500, hz, 1.2, 4);
            grit = (float)e.Grit.GetValue(f, t, hz) / 100f; noise = (float)e.NoiseAmt.GetValue(f, t, hz) / 100f;
        }
        public override void Process(ref float l, ref float r)
        {
            float m = (l + r) * 0.5f;                       // 電話は片耳(モノラル)
            m = pk.Run(l2.Run(l1.Run(h2.Run(h1.Run(m)))));
            if (grit > 0) m = m * (1 - grit) + Db.Soft(m * (1 + grit * 8)) / (1 + grit * 2) * grit;
            m += rnd.Next() * 0.01f * noise;
            l = r = m * 1.4f;
        }
    }
}

[AudioEffect("無線・トランシーバー", ["演出：声を変える"], ["無線", "トランシーバー", "walkie talkie", "インカム", "軍隊", "パイロット", "交信", "演出"])]
public sealed class WalkieTalkieEffect : FxAudioEffectBase
{
    public override string Label => "無線・トランシーバー";

    [Display(GroupName = "無線", Name = "ひずみ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Drive { get; } = new Animation(55, 0, 100);

    [Display(GroupName = "無線", Name = "ザー音")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Static { get; } = new Animation(25, 0, 100);

    [Display(GroupName = "無線", Name = "最初と最後に「ピッ」", Description = "送信の始まりと終わりの合図音")]
    public bool Beep { get => beep; set => Set(ref beep, value); }
    bool beep = true;

    [Display(GroupName = "無線", Name = "ザー音を声の時だけ", Description = "声がない時はザー音も止めます(スケルチ)")]
    public bool Squelch { get => squelch; set => Set(ref squelch, value); }
    bool squelch = true;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Drive, Static];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(WalkieTalkieEffect e) : AudioKernel
    {
        readonly Biquad hp = new(), hp2 = new(), lp = new(), lp2 = new(), nb = new();
        readonly Envelope env = new();
        WhiteNoise rnd = new(5);
        float drive, stat; long frame, total; int hz; bool beep, sq; double open;
        public override void Reset(int h) { hp.Reset(); hp2.Reset(); lp.Reset(); lp2.Reset(); nb.Reset(); env.Reset(); open = 0; }
        public override void Params(long f, long t, int h)
        {
            hz = h; frame = f; total = t;
            hp.HighPass(500, h); hp2.HighPass(500, h); lp.LowPass(2600, h); lp2.LowPass(2600, h); nb.BandPass(2000, h, 0.6);
            env.Set(2, 200, h);
            drive = (float)e.Drive.GetValue(f, t, h) / 100f; stat = (float)e.Static.GetValue(f, t, h) / 100f;
            beep = e.Beep; sq = e.Squelch;
        }
        public override void Process(ref float l, ref float r)
        {
            float m = (l + r) * 0.5f;
            float lev = env.Run(m);
            m = lp2.Run(lp.Run(hp2.Run(hp.Run(m))));
            m = Db.Soft(m * (1 + drive * 12)) * (0.6f + 0.2f * (1 - drive));
            double want = !sq || lev > 0.01f ? 1 : 0;
            open = open * 0.999 + want * 0.001;
            m += nb.Run(rnd.Next()) * 0.25f * stat * (float)(sq ? open : 1);
            if (beep && hz > 0)
            {
                long beepLen = hz / 8;
                long f = frame++;
                if (f < beepLen || ((total - f) < beepLen && f < total))
                {
                    double ph = 2 * Math.PI * (f < beepLen ? 1200 : 900) * f / hz;
                    m += (float)Math.Sin(ph) * 0.18f;
                }
            }
            l = r = m;
        }
    }
}

[AudioEffect("拡声器・メガホン", ["演出：声を変える"], ["拡声器", "メガホン", "megaphone", "ハンドマイク", "選挙", "アナウンス", "スピーカー", "演出"])]
public sealed class MegaphoneEffect : FxAudioEffectBase
{
    public override string Label => "拡声器・メガホン";

    [Display(GroupName = "拡声器", Name = "ひずみ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Drive { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "拡声器", Name = "響き", Description = "屋外で鳴っているような短い響き")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Echo { get; } = new Animation(20, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Drive, Echo];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(MegaphoneEffect e) : AudioKernel
    {
        readonly Biquad hp = new(), hp2 = new(), lp = new(), pk = new();
        readonly DelayLine dl = new();
        float drive, echo; int d1, d2;
        public override void Reset(int hz) { hp.Reset(); hp2.Reset(); lp.Reset(); pk.Reset(); dl.Resize(hz / 2 + 4); d1 = hz * 73 / 1000; d2 = hz * 151 / 1000; }
        public override void Params(long f, long t, int hz)
        {
            hp.HighPass(700, hz, 0.9); hp2.HighPass(700, hz, 0.9); lp.LowPass(4500, hz); pk.Peak(2200, hz, 1.5, 8);
            drive = (float)e.Drive.GetValue(f, t, hz) / 100f; echo = (float)e.Echo.GetValue(f, t, hz) / 100f;
        }
        public override void Process(ref float l, ref float r)
        {
            float m = (l + r) * 0.5f;
            m = pk.Run(lp.Run(hp2.Run(hp.Run(m))));
            m = Db.Soft(m * (1.5f + drive * 10)) * 0.55f;
            float o = m + (dl.Read(d1) * 0.35f + dl.Read(d2) * 0.2f) * echo;
            dl.Write(m);
            l = r = o;
        }
    }
}

[AudioEffect("ロボットの声", ["演出：声を変える"], ["ロボット", "robot", "機械", "AI", "アンドロイド", "宇宙人", "メカ", "演出"])]
public sealed class RobotEffect : FxAudioEffectBase
{
    public override string Label => "ロボットの声";

    [Display(GroupName = "ロボット", Name = "金属感の周波数", Description = "声に掛け合わせる音の高さ(低いほどガラガラ)")]
    [AnimationSlider("F0", "Hz", 20, 400)]
    public Animation Freq { get; } = new Animation(90, 1, 2000);

    [Display(GroupName = "ロボット", Name = "金属の響き", Description = "缶の中で鳴っているような短い響き")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Metal { get; } = new Animation(50, 0, 95);

    [Display(GroupName = "ロボット", Name = "混ぜる量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Mix { get; } = new Animation(85, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Freq, Metal, Mix];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(RobotEffect e) : AudioKernel
    {
        readonly DelayLine cl = new(), cr = new();
        double ph, inc; float metal, mix; int d;
        public override void Reset(int hz) { cl.Resize(hz / 50 + 4); cr.Resize(hz / 50 + 4); ph = 0; d = Math.Max(2, hz * 6 / 1000); }
        public override void Params(long f, long t, int hz)
        {
            inc = 2 * Math.PI * e.Freq.GetValue(f, t, hz) / hz;
            metal = (float)e.Metal.GetValue(f, t, hz) / 100f * 0.9f; mix = (float)e.Mix.GetValue(f, t, hz) / 100f;
        }
        public override void Process(ref float l, ref float r)
        {
            ph += inc; if (ph > Math.PI * 2) ph -= Math.PI * 2;
            float c = (float)Math.Sin(ph);
            float wl = l * c, wr = r * c;
            float ol = wl + cl.Read(d) * metal, orr = wr + cr.Read(d + 1) * metal;
            cl.Write(ol); cr.Write(orr);
            ol *= 1 - metal * 0.5f; orr *= 1 - metal * 0.5f;
            l = l * (1 - mix) + ol * mix * 1.3f; r = r * (1 - mix) + orr * mix * 1.3f;
        }
    }
}

[AudioEffect("ボイスチェンジャー（高く・低く）", ["演出：声を変える"], ["ボイスチェンジャー", "ピッチ", "pitch", "高い声", "低い声", "ヘリウム", "子供の声", "悪役", "演出"])]
public sealed class VoiceChangerEffect : FxAudioEffectBase
{
    public override string Label => "ボイスチェンジャー（高く・低く）";

    [Display(GroupName = "ボイスチェンジャー", Name = "高さ", Description = "半音(12で1オクターブ)。速さは変わりません")]
    [AnimationSlider("F1", "半音", -12, 12)]
    public Animation Semitones { get; } = new Animation(5, -24, 24);

    [Display(GroupName = "ボイスチェンジャー", Name = "なめらかさ", Description = "大きいほど低い声がなめらか(小さいほど高い声がくっきり)")]
    [AnimationSlider("F0", "ms", 20, 120)]
    public Animation Window { get; } = new Animation(50, 10, 300);

    [Display(GroupName = "ボイスチェンジャー", Name = "元の声を混ぜる")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Dry { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Semitones, Window, Dry];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(VoiceChangerEffect e) : AudioKernel
    {
        readonly PitchShifter pl = new(), pr = new();
        double ratio = 1; float dry; float winSet = -1;
        public override void Reset(int hz) { winSet = -1; pl.Reset(); pr.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            ratio = Math.Pow(2, e.Semitones.GetValue(f, t, hz) / 12.0);
            dry = (float)e.Dry.GetValue(f, t, hz) / 100f;
            float w = (float)Math.Round(e.Window.GetValue(f, t, hz));
            if (w != winSet) { winSet = w; pl.Configure(hz, w); pr.Configure(hz, w); }
        }
        public override void Process(ref float l, ref float r)
        {
            float a = pl.Run(l, ratio), b = pr.Run(r, ratio);
            l = a * (1 - dry) + l * dry; r = b * (1 - dry) + r * dry;
        }
    }
}

[AudioEffect("ささやき声", ["演出：声を変える"], ["ささやき", "ささやき声", "whisper", "ひそひそ", "内緒話", "息", "ASMR", "演出"])]
public sealed class WhisperEffect : FxAudioEffectBase
{
    public override string Label => "ささやき声";

    [Display(GroupName = "ささやき", Name = "息っぽさ", Description = "声の高さを消して、息の音に置きかえる量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Breath { get; } = new Animation(75, 0, 100);

    [Display(GroupName = "ささやき", Name = "近さ", Description = "大きいほど耳元で話しているように")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Near { get; } = new Animation(50, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Breath, Near];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K : AudioKernel
    {
        // 声を帯域ごとに分け、それぞれの大きさでノイズを鳴らす(簡単なボコーダー)
        static readonly double[] Bands = { 300, 500, 800, 1200, 1800, 2600, 3600, 5000, 7000 };
        readonly Biquad[] an = new Biquad[Bands.Length], sy = new Biquad[Bands.Length];
        readonly Envelope[] en = new Envelope[Bands.Length];
        readonly Biquad hp = new(), shelf = new();
        WhiteNoise rnd = new(9);
        float breath, near;
        readonly WhisperEffect e;
        public K(WhisperEffect e) { this.e = e; for (int i = 0; i < Bands.Length; i++) { an[i] = new(); sy[i] = new(); en[i] = new(); } }
        public override void Reset(int hz)
        {
            for (int i = 0; i < Bands.Length; i++) { an[i].Reset(); sy[i].Reset(); en[i].Reset(); an[i].BandPass(Bands[i], hz, 2.5); sy[i].BandPass(Bands[i], hz, 2.5); en[i].Set(3, 25, hz); }
            hp.Reset(); shelf.Reset();
        }
        public override void Params(long f, long t, int hz)
        {
            breath = (float)e.Breath.GetValue(f, t, hz) / 100f; near = (float)e.Near.GetValue(f, t, hz) / 100f;
            hp.HighPass(250, hz); shelf.HighShelf(4000, hz, 3 + near * 6);
        }
        public override void Process(ref float l, ref float r)
        {
            float m = (l + r) * 0.5f, n = rnd.Next(), w = 0;
            for (int i = 0; i < Bands.Length; i++) w += sy[i].Run(n) * en[i].Run(an[i].Run(m)) * 6f;
            float dryv = hp.Run(m);
            float o = shelf.Run(dryv * (1 - breath) * 0.6f + w * breath);
            float pan = 0.15f * near;
            l = o * (1 + pan); r = o * (1 - pan);
        }
    }
}

[AudioEffect("ラジオ（AM・古いラジオ）", ["演出：声を変える"], ["ラジオ", "AM", "古いラジオ", "radio", "レトロ", "昭和", "蓄音機", "演出"])]
public sealed class OldRadioEffect : FxAudioEffectBase
{
    public override string Label => "ラジオ（AM・古いラジオ）";

    [Display(GroupName = "ラジオ", Name = "こもり具合", Description = "高い側をどこで切るか")]
    [AnimationSlider("F0", "Hz", 1500, 6000)]
    public Animation High { get; } = new Animation(3500, 500, 20000);

    [Display(GroupName = "ラジオ", Name = "ザー音")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Hiss { get; } = new Animation(15, 0, 100);

    [Display(GroupName = "ラジオ", Name = "パチパチ音")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Crackle { get; } = new Animation(20, 0, 100);

    [Display(GroupName = "ラジオ", Name = "電波のゆらぎ", Description = "音量がゆっくりゆれます")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Fade { get; } = new Animation(20, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [High, Hiss, Crackle, Fade];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(OldRadioEffect e) : AudioKernel
    {
        readonly Biquad hp = new(), lp = new(), lp2 = new(), pk = new(), hb = new();
        WhiteNoise rnd = new(31);
        float hiss, crack, fade; double ph, ph2, inc, inc2, pop;
        public override void Reset(int hz) { hp.Reset(); lp.Reset(); lp2.Reset(); pk.Reset(); hb.Reset(); ph = ph2 = 0; pop = 0; }
        public override void Params(long f, long t, int hz)
        {
            double hi = e.High.GetValue(f, t, hz);
            hp.HighPass(250, hz); lp.LowPass(hi, hz); lp2.LowPass(hi, hz); pk.Peak(1000, hz, 0.8, 3); hb.HighPass(2000, hz);
            hiss = (float)e.Hiss.GetValue(f, t, hz) / 100f; crack = (float)e.Crackle.GetValue(f, t, hz) / 100f; fade = (float)e.Fade.GetValue(f, t, hz) / 100f;
            inc = 2 * Math.PI * 0.23 / hz; inc2 = 2 * Math.PI * 0.071 / hz;
        }
        public override void Process(ref float l, ref float r)
        {
            float m = (l + r) * 0.5f;
            m = pk.Run(lp2.Run(lp.Run(hp.Run(m))));
            m = Db.Soft(m * 1.8f) * 0.7f;
            ph += inc; ph2 += inc2;
            float g = 1 - fade * 0.35f * (float)(0.5 + 0.5 * Math.Sin(ph) * Math.Sin(ph2 * 1.7));
            m *= g;
            m += hb.Run(rnd.Next()) * 0.04f * hiss;
            // パチパチ: ときどき短い破裂音
            if (crack > 0 && rnd.Next() > 1 - 0.0006f * crack) pop = (rnd.Next() > 0 ? 1 : -1) * (0.1 + 0.25 * crack);
            m += (float)pop; pop *= 0.82;
            l = r = m;
        }
    }
}
