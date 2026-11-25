using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 47〜64: 変調・歪み

/// <summary>1次の全域通過(フェイザー用)</summary>
internal struct Ap1
{
    float x1, y1;
    public void Reset() { x1 = y1 = 0; }
    public float Run(float x, float a) { float y = -a * x + x1 + a * y1; x1 = x; y1 = y; return y; }
}

/// <summary>歪みの共通(入力の増幅→形を変える→音色を整える→音量を戻す)</summary>
internal sealed class ShapeK(FxA2 e, Func<float, float> shape, double minDb, double maxDb, double tone, double outDb) : KB(e)
{
    readonly StereoBiquad lp = new(), hp = new(); float gain = 1, back = 1;
    protected override void Init() { lp.Reset(); hp.Reset(); }
    protected override void Tick() { gain = Db.ToGain(minDb + (maxDb - minDb) * A); back = Db.ToGain(outDb - (maxDb - minDb) * A * .45); lp.LowPass(tone, Hz); hp.HighPass(45, Hz); }
    protected override void Run(ref float l, ref float r) { hp.Run(ref l, ref r); l = shape(l * gain); r = shape(r * gain); lp.Run(ref l, ref r); l *= back; r *= back; }
}

[AudioEffect("コーラス", ["演出：変調"], ["コーラス", "chorus", "厚み", "重ねる", "演出"])]
public sealed class ChorusEffect : FxA3
{
    public override string Label => "コーラス";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new(); Lfo la, lb; double dep, baseD; float wet;
        protected override void Init() { int n = Hz / 10 + 8; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); la.Reset(); lb.Reset(.25); }
        protected override void Tick() { baseD = .018 * Hz; dep = (.001 + A * .007) * Hz; wet = .35f + A * .4f; }
        protected override void Run(ref float l, ref float r)
        {
            double f = .15 + S * 2.4; la.Step(f, Hz); lb.Step(f * 1.13, Hz); a.Write(l); b.Write(r);
            float wl = a.Read(baseD + dep * la.Sin), wr = b.Read(baseD + dep * lb.Sin);
            l = (l + wl * wet) / (1 + wet * .5f); r = (r + wr * wet) / (1 + wet * .5f);
        }
    }
}

[AudioEffect("フランジャー", ["演出：変調"], ["フランジャー", "flanger", "ジェット", "うねり", "演出"])]
public sealed class FlangerEffect : FxA3
{
    public override string Label => "フランジャー";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new(); Lfo lfo; float fbl, fbr;
        protected override void Init() { int n = Hz / 50 + 8; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); lfo.Reset(); fbl = fbr = 0; }
        protected override void Run(ref float l, ref float r)
        {
            lfo.Step(.05 + S * 1.2, Hz); double d = (.0008 + lfo.Uni * .0055) * Hz; float fb = .25f + A * .6f;
            a.Write(l + fbl * fb); b.Write(r + fbr * fb); fbl = a.Read(d); fbr = b.Read(d + .00015 * Hz);
            l = (l + fbl) * .62f; r = (r + fbr) * .62f;
        }
    }
}

[AudioEffect("フェイザー", ["演出：変調"], ["フェイザー", "phaser", "シュワシュワ", "うねり", "演出"])]
public sealed class PhaserEffect : FxA3
{
    public override string Label => "フェイザー";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        readonly Ap1[] al = new Ap1[6], ar = new Ap1[6]; Lfo lfo; float fbl, fbr, co;
        protected override void Init() { for (int i = 0; i < 6; i++) { al[i].Reset(); ar[i].Reset(); } lfo.Reset(); fbl = fbr = 0; }
        protected override void Tick() { lfo.Step((.05 + S * 2) * 128, Hz); double f = Exp(250, 3200, lfo.Uni), t = Math.Tan(Math.PI * f / Hz); co = (float)((t - 1) / (t + 1)); }
        protected override void Run(ref float l, ref float r)
        {
            float fb = A * .7f, x = l + fbl * fb, y = r + fbr * fb;
            for (int i = 0; i < 6; i++) { x = al[i].Run(x, -co); y = ar[i].Run(y, -co); }
            fbl = x; fbr = y; l = (l + x) * .6f; r = (r + y) * .6f;
        }
    }
}

[AudioEffect("ビブラート", ["演出：変調"], ["ビブラート", "vibrato", "音程の揺れ", "震える声", "演出"])]
public sealed class VibratoEffect : FxA3
{
    public override string Label => "ビブラート";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new(); Lfo lfo;
        public override int Latency => (int)(.012 * Hz);
        protected override void Init() { int n = Hz / 20 + 8; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); lfo.Reset(); }
        protected override void Run(ref float l, ref float r) { lfo.Step(2 + S * 9, Hz); double d = .012 * Hz + (.0004 + A * .0055) * Hz * lfo.Sin; a.Write(l); b.Write(r); l = a.Read(d); r = b.Read(d); }
    }
}

[AudioEffect("リング変調", ["演出：変調"], ["リング変調", "ring modulator", "金属", "機械", "ロボット", "演出"])]
public sealed class RingModEffect : FxA2
{
    public override string Label => "リング変調";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e) { Lfo o; protected override void Init() => o.Reset(); protected override void Run(ref float l, ref float r) { o.Step(Exp(30, 1800, A), Hz); float m = (float)o.Sin; l *= m * 1.3f; r *= m * 1.3f; } }
}

[AudioEffect("オーバードライブ", ["演出：歪み"], ["オーバードライブ", "overdrive", "歪み", "ギター", "力強い", "演出"])]
public sealed class OverdriveEffect : FxA2 { public override string Label => "オーバードライブ"; internal override AudioKernel CreateKernel() => new ShapeK(this, MathF.Tanh, 3, 20, 7000, 0); }

[AudioEffect("ディストーション", ["演出：歪み"], ["ディストーション", "distortion", "歪み", "ギター", "ロック", "演出"])]
public sealed class DistortionEffect : FxA2 { public override string Label => "ディストーション"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => Math.Clamp(x * 1.4f, -.7f, .7f), 10, 36, 5000, 2); }

[AudioEffect("ファズ", ["演出：歪み"], ["ファズ", "fuzz", "歪み", "ザラザラ", "潰れた音", "演出"])]
public sealed class FuzzEffect : FxA2 { public override string Label => "ファズ"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Abs(x) < .02f ? 0f : MathF.Sign(x) * (.6f + .15f * MathF.Tanh(MathF.Abs(x))), 12, 34, 3800, 0); }

[AudioEffect("サチュレーター", ["演出：歪み"], ["サチュレーター", "saturator", "密度", "太い音", "アナログ", "演出"])]
public sealed class SaturatorEffect : FxA2 { public override string Label => "サチュレーター"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Abs(x) < 1f ? x - x * x * x / 3f : MathF.Sign(x) * 2f / 3f, 0, 9, 16000, 1); }

internal sealed class TapeK(FxA2 e, bool hiss) : KB(e)
{
    readonly DelayLine a = new(), b = new(); readonly StereoBiquad lp = new(), hp = new(); Lfo wow, flut; WhiteNoise n = new(77); readonly Biquad nf = new();
    public override int Latency => (int)(.008 * Hz);
    protected override void Init() { int s = Hz / 20 + 8; a.Resize(s); b.Resize(s); a.Reset(); b.Reset(); lp.Reset(); hp.Reset(); wow.Reset(); flut.Reset(); nf.Reset(); nf.HighPass(2500, Hz); }
    protected override void Tick() { lp.LowPass(hiss ? 7500 : 11000, Hz); hp.HighPass(hiss ? 90 : 40, Hz); }
    protected override void Run(ref float l, ref float r)
    {
        wow.Step(.7, Hz); flut.Step(7.3, Hz); double d = .008 * Hz + (A * .0028 * wow.Sin + A * .0005 * flut.Sin) * Hz;
        a.Write(MathF.Tanh(l * (1f + A))); b.Write(MathF.Tanh(r * (1f + A))); l = a.Read(d) / (1f + A * .5f); r = b.Read(d) / (1f + A * .5f); lp.Run(ref l, ref r); hp.Run(ref l, ref r);
        if (hiss) { float h = nf.Run(n.Next()) * (.006f + A * .022f); l += h; r += h; }
    }
}

[AudioEffect("テープ風", ["演出：歪み"], ["テープ", "tape", "アナログ", "レトロ", "丸い音", "演出"])]
public sealed class TapeLookEffect : FxA2 { public override string Label => "テープ風"; internal override AudioKernel CreateKernel() => new TapeK(this, false); }

[AudioEffect("カセットテープ", ["演出：歪み"], ["カセット", "cassette", "ノイズ", "ヒスノイズ", "レトロ", "昭和", "演出"])]
public sealed class CassetteEffect : FxA2 { public override string Label => "カセットテープ"; internal override AudioKernel CreateKernel() => new TapeK(this, true); }

[AudioEffect("レコード", ["演出：歪み"], ["レコード", "vinyl", "プチプチ", "蓄音機", "古い音", "演出"])]
public sealed class VinylEffect : FxA2
{
    public override string Label => "レコード";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad lp = new(), hp = new(); WhiteNoise n = new(913); float pop; int sign = 1;
        protected override void Init() { lp.Reset(); hp.Reset(); pop = 0; }
        protected override void Tick() { lp.LowPass(5200, Hz); hp.HighPass(260, Hz); }
        protected override void Run(ref float l, ref float r)
        {
            lp.Run(ref l, ref r); hp.Run(ref l, ref r); float c = (n.Next() + 1f) * .5f;
            if (c > 1f - (.00004f + A * .00045f)) { pop = .08f + A * .45f * c; sign = -sign; }
            float x = pop * sign + n.Next() * (.002f + A * .006f); pop *= .82f; l = l * 1.15f + x; r = r * 1.15f + x * .85f;
        }
    }
}

[AudioEffect("サンプルレート落とし", ["演出：歪み"], ["サンプルレート", "低音質", "ガサガサ", "downsample", "レトロゲーム", "演出"])]
public sealed class DownsampleEffect : FxA2
{
    public override string Label => "サンプルレート落とし";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        float hl, hr; double ph;
        protected override void Init() { hl = hr = 0; ph = 1; }
        protected override void Run(ref float l, ref float r) { ph += Exp(12000, 1200, A) / Hz; if (ph >= 1) { ph -= Math.Floor(ph); hl = l; hr = r; } l = hl; r = hr; }
    }
}

internal sealed class PitchAddK(FxA1 e, double ratio, float wet, double lowPass = 0) : KB(e)
{
    readonly PitchShifter a = new(), b = new(); readonly StereoBiquad lp = new();
    protected override void Init() { a.Configure(Hz, 60); b.Configure(Hz, 60); a.Reset(); b.Reset(); lp.Reset(); if (lowPass > 0) lp.LowPass(lowPass, Hz); }
    protected override void Run(ref float l, ref float r)
    {
        float x = l, y = r; if (lowPass > 0) lp.Run(ref x, ref y);
        float g = (e is FxA2 ? A * 2f : 1f) * wet, wl = a.Run(x, ratio) * g, wr = b.Run(y, ratio) * g;
        l = (l + wl) / (1f + g * .35f); r = (r + wr) / (1f + g * .35f);
    }
}

[AudioEffect("ハーモナイザー", ["演出：変調"], ["ハーモナイザー", "harmonizer", "ハモり", "3度", "合唱", "演出"])]
public sealed class HarmonizerEffect : FxA2 { public override string Label => "ハーモナイザー"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 4 / 12.0), .75f); }

[AudioEffect("オクターブ上を重ねる", ["演出：変調"], ["オクターブ上", "octave up", "高い音を足す", "キラキラ", "演出"])]
public sealed class OctaveUpEffect : FxA2 { public override string Label => "オクターブ上を重ねる"; internal override AudioKernel CreateKernel() => new PitchAddK(this, 2, .7f); }

[AudioEffect("オクターブ下を重ねる", ["演出：変調"], ["オクターブ下", "octave down", "低い音を足す", "太い声", "演出"])]
public sealed class OctaveDownEffect : FxA2 { public override string Label => "オクターブ下を重ねる"; internal override AudioKernel CreateKernel() => new PitchAddK(this, .5, .8f); }

[AudioEffect("サブベース", ["演出：音質"], ["サブベース", "sub bass", "重低音", "重い", "迫力", "演出"])]
public sealed class SubBassEffect : FxA2 { public override string Label => "サブベース"; internal override AudioKernel CreateKernel() => new PitchAddK(this, .5, 1.1f, 220); }

[AudioEffect("エキサイター", ["演出：音質"], ["エキサイター", "exciter", "倍音", "抜け", "きらびやか", "演出"])]
public sealed class ExciterEffect : FxA2
{
    public override string Label => "エキサイター";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad hp = new(), hp2 = new();
        protected override void Init() { hp.Reset(); hp2.Reset(); }
        protected override void Tick() { hp.HighPass(3200, Hz); hp2.HighPass(2600, Hz); }
        protected override void Run(ref float l, ref float r)
        {
            float x = l, y = r; hp.Run(ref x, ref y); x = MathF.Tanh(x * 7f); y = MathF.Tanh(y * 7f); hp2.Run(ref x, ref y);
            float g = A * .32f; l += x * g; r += y * g;
        }
    }
}
