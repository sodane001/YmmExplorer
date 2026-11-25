using System.ComponentModel.DataAnnotations;
using System.Globalization;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  演出
// =====================================================================================================

internal sealed class NullKernel : AudioKernel
{
    public override void Reset(int hz) { }
    public override void Params(long frame, long total, int hz) { }
    public override void Process(ref float l, ref float r) { }
}

[AudioEffect("ダッキング（区間だけ音量を下げる）", ["演出：効果"], ["ダッキング", "ducking", "BGMを下げる", "声の時だけ", "音量を下げる", "サイドチェイン", "演出"])]
public sealed class DuckingEffect : FxAudioEffectBase
{
    public override string Label => "ダッキング（区間だけ音量を下げる）";

    [Display(GroupName = "ダッキング", Name = "下げる区間(秒)", Description = "アイテムの先頭からの秒数。「1.5-4, 6-8.2」のように、カンマで区切って書きます。YMM4 のエフェクトは他のアイテムの音を聞けないので、声のある所を手で指定します")]
    [TextEditor(AcceptsReturn = false)]
    public string Ranges { get => ranges; set => Set(ref ranges, value ?? ""); }
    string ranges = "1-4";

    [Display(GroupName = "ダッキング", Name = "下げる量")]
    [AnimationSlider("F0", "dB", 0, 30)]
    public Animation Amount { get; } = new Animation(12, 0, 60);

    [Display(GroupName = "ダッキング", Name = "下げ始め・戻りの時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation Fade { get; } = new Animation(0.3, 0, 10);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, Fade];
    internal override AudioKernel CreateKernel() => new K(this);

    internal static List<(double a, double b)> Parse(string s)
    {
        var list = new List<(double, double)>();
        foreach (var part in (s ?? "").Replace('、', ',').Replace('，', ',').Replace('〜', '-').Replace('～', '-').Replace('ー', '-').Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var ab = part.Split('-', StringSplitOptions.TrimEntries);
            if (ab.Length == 2 && double.TryParse(ab[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var a) && double.TryParse(ab[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var b) && b > a)
                list.Add((a, b));
        }
        return list;
    }

    sealed class K(DuckingEffect e) : AudioKernel
    {
        List<(double a, double b)> list = [];
        string parsed = "\0";
        float target = 1, g = 1; double coef = 0.999; long frame; int hz;
        double fade;
        public override void Reset(int h) { hz = h; }
        public override void Params(long f, long t, int h)
        {
            hz = h; frame = f;
            if (!ReferenceEquals(parsed, e.Ranges)) { parsed = e.Ranges; list = Parse(parsed); }
            fade = Math.Max(0.001, e.Fade.GetValue(f, t, h));
            float down = Db.ToGain(-e.Amount.GetValue(f, t, h));
            double sec = (double)f / h;
            target = 1;
            // 区間の手前からなめらかに下げ、区間の後でなめらかに戻す
            foreach (var (a, b) in list)
            {
                double w;
                if (sec >= a && sec <= b) w = 1;
                else if (sec < a) w = 1 - Math.Clamp((a - sec) / fade, 0, 1);
                else w = 1 - Math.Clamp((sec - b) / fade, 0, 1);
                target = (float)Math.Min(target, 1 - (1 - down) * Smooth(w));
            }
            coef = Math.Exp(-1.0 / (0.01 * h));
        }
        static double Smooth(double x) => x * x * (3 - 2 * x);
        public override void Process(ref float l, ref float r)
        {
            g = (float)(coef * g + (1 - coef) * target);
            l *= g; r *= g;
        }
    }
}

[AudioEffect("だんだん速く・遅く", ["演出：効果"], ["だんだん速く", "だんだん遅く", "加速", "減速", "スピード", "早回し", "スロー", "演出"])]
public sealed class SpeedRampEffect : FxAudioEffectBase, IRateEffect
{
    public override string Label => "だんだん速く・遅く";

    [Display(GroupName = "速さ", Name = "始まりの速さ", Description = "1 で元の速さ。高さも一緒に変わります(レコードの回転のように)")]
    [AnimationSlider("F2", "倍", 0.25, 3)]
    public Animation Start { get; } = new Animation(1, 0.1, 8);

    [Display(GroupName = "速さ", Name = "終わりの速さ")]
    [AnimationSlider("F2", "倍", 0.25, 3)]
    public Animation End { get; } = new Animation(2, 0.1, 8);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Start, End];
    internal override AudioKernel CreateKernel() => new NullKernel();

    public double SourceTime(double t, double dur, long frame, long total, int hz)
    {
        double s0 = Start.GetValue(0, total, hz), s1 = End.GetValue(0, total, hz);
        if (dur <= 0) return t;
        // 速さが直線で変わる時、進んだ量はその積分
        return s0 * t + (s1 - s0) * t * t / (2 * dur);
    }
}

[AudioEffect("逆再生", ["演出：効果"], ["逆再生", "リバース", "reverse", "巻き戻し", "逆回転", "演出"])]
public sealed class ReverseEffect : FxAudioEffectBase, IRateEffect
{
    public override string Label => "逆再生";
    protected override IEnumerable<IAnimatable> GetAnimatables() => [];
    internal override AudioKernel CreateKernel() => new NullKernel();
    public double SourceTime(double t, double dur, long frame, long total, int hz) => Math.Max(0, dur - t - 1.0 / Math.Max(1, hz));
}

public enum TapeMode
{
    [Display(Name = "最後に止まる(テープストップ)")] Stop,
    [Display(Name = "最初に回り始める(テープスタート)")] Start,
    [Display(Name = "両方")] Both,
}

[AudioEffect("テープストップ", ["演出：効果"], ["テープストップ", "tape stop", "レコードが止まる", "電源が切れる", "スローダウン", "停電", "テープスタート", "演出"])]
public sealed class TapeStopEffect : FxAudioEffectBase, IRateEffect
{
    public override string Label => "テープストップ";

    [Display(GroupName = "テープストップ", Name = "種類")]
    [EnumComboBox]
    public TapeMode Mode { get => mode; set => Set(ref mode, value); }
    TapeMode mode = TapeMode.Stop;

    [Display(GroupName = "テープストップ", Name = "止まるまでの時間")]
    [AnimationSlider("F2", "秒", 0.1, 3)]
    public Animation Length { get; } = new Animation(0.8, 0.02, 30);

    [Display(GroupName = "テープストップ", Name = "カーブ", Description = "大きいほど、最後に急に止まります")]
    [AnimationSlider("F1", "", 0.5, 3)]
    public Animation Curve { get; } = new Animation(1.5, 0.2, 6);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Length, Curve];
    internal override AudioKernel CreateKernel() => new NullKernel();

    public double SourceTime(double t, double dur, long frame, long total, int hz)
    {
        double len = Math.Clamp(Length.GetValue(0, total, hz), 0.02, Math.Max(0.02, dur / (Mode == TapeMode.Both ? 2 : 1)));
        double k = Math.Max(0.2, Curve.GetValue(0, total, hz));
        // 速さ v(u) = (1-u)^k (u: 0→1)。進む量は ∫v = (1-(1-u)^(k+1))/(k+1)
        static double Area(double uu, double kk) => (1 - Math.Pow(1 - Math.Clamp(uu, 0, 1), kk + 1)) / (kk + 1);
        double src = t, shift = 0;
        if (Mode != TapeMode.Stop)
        {
            // 回り始め: 速さ 0→1。最初の len 秒で進む量は len*(1-Area 逆)
            if (t < len) return len * (1.0 / (k + 1) - Area(1 - t / len, k)) + 0;
            shift = len - len / (k + 1);   // 回り始めで遅れた分
            src = t - shift;
        }
        if (Mode != TapeMode.Start)
        {
            double s = dur - len;
            if (t > s) return (s - shift) + len * Area((t - s) / len, k);
        }
        return src;
    }
}

[AudioEffect("ビットクラッシャー（ファミコン風）", ["演出：効果"], ["ビットクラッシャー", "bitcrusher", "8bit", "ファミコン", "レトロゲーム", "チップチューン", "ローファイ", "lofi", "演出"])]
public sealed class BitCrusherEffect : FxAudioEffectBase
{
    public override string Label => "ビットクラッシャー（ファミコン風）";

    [Display(GroupName = "ビットクラッシャー", Name = "ビット数", Description = "小さいほどザラザラ(16で元のまま)")]
    [AnimationSlider("F0", "bit", 2, 16)]
    public Animation Bits { get; } = new Animation(6, 1, 24);

    [Display(GroupName = "ビットクラッシャー", Name = "サンプリング周波数", Description = "小さいほどこもって、金属っぽい音に")]
    [AnimationSlider("F0", "Hz", 1000, 44100)]
    public Animation Rate { get; } = new Animation(11025, 200, 192000);

    [Display(GroupName = "ビットクラッシャー", Name = "混ぜる量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Mix { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Bits, Rate, Mix];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(BitCrusherEffect e) : AudioKernel
    {
        double acc, step; float hl, hr, q, mix;
        public override void Reset(int hz) { acc = 1; hl = hr = 0; }
        public override void Params(long f, long t, int hz)
        {
            step = Math.Clamp(e.Rate.GetValue(f, t, hz) / hz, 0.0001, 1);
            q = MathF.Pow(2, (float)Math.Clamp(e.Bits.GetValue(f, t, hz), 1, 24) - 1);
            mix = (float)e.Mix.GetValue(f, t, hz) / 100f;
        }
        public override void Process(ref float l, ref float r)
        {
            acc += step;
            if (acc >= 1) { acc -= 1; hl = MathF.Round(l * q) / q; hr = MathF.Round(r * q) / q; }
            l = l * (1 - mix) + hl * mix; r = r * (1 - mix) + hr * mix;
        }
    }
}

public enum LfoShape
{
    [Display(Name = "なめらか(サイン)")] Sine,
    [Display(Name = "三角")] Triangle,
    [Display(Name = "四角(ON/OFF)")] Square,
}

[AudioEffect("トレモロ（音量のゆれ）", ["演出：効果"], ["トレモロ", "tremolo", "音量のゆれ", "ゆらゆら", "震える声", "オートパン", "演出"])]
public sealed class TremoloEffect : FxAudioEffectBase
{
    public override string Label => "トレモロ（音量のゆれ）";

    [Display(GroupName = "トレモロ", Name = "速さ")]
    [AnimationSlider("F1", "Hz", 0.1, 20)]
    public Animation Rate { get; } = new Animation(5, 0.01, 100);

    [Display(GroupName = "トレモロ", Name = "深さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Depth { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "トレモロ", Name = "形")]
    [EnumComboBox]
    public LfoShape Shape { get => shape; set => Set(ref shape, value); }
    LfoShape shape = LfoShape.Sine;

    [Display(GroupName = "トレモロ", Name = "左右で交互に", Description = "オンで、左右に行ったり来たりします(オートパン)")]
    public bool Pan { get => pan; set => Set(ref pan, value); }
    bool pan;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Rate, Depth];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(TremoloEffect e) : AudioKernel
    {
        double ph, inc; float depth; LfoShape shape; bool pan;
        public override void Reset(int hz) { ph = 0; }
        public override void Params(long f, long t, int hz)
        {
            inc = e.Rate.GetValue(f, t, hz) / hz;
            depth = (float)e.Depth.GetValue(f, t, hz) / 100f; shape = e.Shape; pan = e.Pan;
        }
        float Wave(double p)
        {
            p -= Math.Floor(p);
            return shape switch
            {
                LfoShape.Triangle => (float)(1 - Math.Abs(p * 2 - 1)),
                LfoShape.Square => p < 0.5 ? 1f : 0f,
                _ => (float)(0.5 + 0.5 * Math.Sin(p * Math.PI * 2)),
            };
        }
        public override void Process(ref float l, ref float r)
        {
            ph += inc; if (ph > 1e6) ph -= Math.Floor(ph);
            float a = Wave(ph);
            float gl = 1 - depth * (1 - a);
            float gr = pan ? 1 - depth * a : gl;
            l *= gl; r *= gr;
        }
    }
}

[AudioEffect("心の声・回想", ["演出：効果"], ["心の声", "回想", "モノローグ", "独白", "頭の中", "テレパシー", "夢", "演出"])]
public sealed class InnerVoiceEffect : FxAudioEffectBase
{
    public override string Label => "心の声・回想";

    [Display(GroupName = "心の声", Name = "響き")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Wet { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "心の声", Name = "こだま", Description = "言葉の終わりが少し遅れてくり返されます")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Echo { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "心の声", Name = "夢っぽさ", Description = "高い音と低い音を少し削り、声をやわらかくします")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Soft { get; } = new Animation(40, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Wet, Echo, Soft];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(InnerVoiceEffect e) : AudioKernel
    {
        readonly Reverb rv = new();
        readonly DelayLine dl = new(), dr = new();
        readonly StereoBiquad lp = new(), hp = new();
        float wet, echo; int d;
        public override void Reset(int hz) { rv.Configure(hz, 0.88f, 0.5f, 20, 1.3f); rv.Reset(); dl.Resize(hz + 8); dr.Resize(hz + 8); lp.Reset(); hp.Reset(); d = hz * 280 / 1000; }
        public override void Params(long f, long t, int hz)
        {
            wet = (float)e.Wet.GetValue(f, t, hz) / 100f * 2.5f; echo = (float)e.Echo.GetValue(f, t, hz) / 100f;
            float s = (float)e.Soft.GetValue(f, t, hz) / 100f;
            lp.LowPass(16000 * Math.Pow(0.3, s), hz); hp.HighPass(60 + 200 * s, hz);
        }
        public override void Process(ref float l, ref float r)
        {
            float el = dl.Read(d), er = dr.Read(d + 37);
            dl.Write(l + er * 0.35f * echo); dr.Write(r + el * 0.35f * echo);
            rv.Process(l, r, out var wl, out var wr);
            float ol = l + el * echo * 0.6f + wl * wet, or2 = r + er * echo * 0.6f + wr * wet;
            lp.Run(ref ol, ref or2); hp.Run(ref ol, ref or2);
            l = ol * 0.8f; r = or2 * 0.8f;
        }
    }
}

[AudioEffect("空間の広がり（ステレオ拡張）", ["演出：効果"], ["空間の広がり", "ステレオ拡張", "ワイド", "stereo widener", "広がり", "立体", "モノラル", "演出"])]
public sealed class StereoWidenEffect : FxAudioEffectBase
{
    public override string Label => "空間の広がり（ステレオ拡張）";

    [Display(GroupName = "広がり", Name = "広さ", Description = "100% で元のまま、0% でモノラル、200% で大きく広がります")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Width { get; } = new Animation(150, 0, 300);

    [Display(GroupName = "広がり", Name = "モノラルの音も広げる", Description = "左右が同じ音(声など)にも、少しずらした音を足して広げます")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Haas { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "広がり", Name = "低音はまん中のまま", Description = "低い音まで広げると、こもったり打ち消し合ったりするのを防ぎます")]
    public bool KeepBass { get => keepBass; set => Set(ref keepBass, value); }
    bool keepBass = true;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Width, Haas];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(StereoWidenEffect e) : AudioKernel
    {
        readonly DelayLine dl = new();
        readonly Biquad sideHp = new(), haasHp = new();
        float width, haas; int d; bool keepBass;
        public override void Reset(int hz) { dl.Resize(hz / 20 + 8); d = Math.Max(2, hz * 12 / 1000); sideHp.Reset(); haasHp.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            width = (float)e.Width.GetValue(f, t, hz) / 100f; haas = (float)e.Haas.GetValue(f, t, hz) / 100f; keepBass = e.KeepBass;
            sideHp.HighPass(150, hz); haasHp.HighPass(300, hz);
        }
        public override void Process(ref float l, ref float r)
        {
            float mid = (l + r) * 0.5f, side = (l - r) * 0.5f;
            float s = side * width;
            if (keepBass && width > 1) s = side + sideHp.Run(side) * (width - 1);
            dl.Write(mid);
            float h = haasHp.Run(dl.Read(d)) * haas * 0.5f;
            s += h;
            float g = 1f / (1 + Math.Max(0, width - 1) * 0.25f + haas * 0.15f);
            l = (mid + s) * g; r = (mid - s) * g;
        }
    }
}
