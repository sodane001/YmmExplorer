using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 65〜82: 声の演出

/// <summary>音程を変える左右セット</summary>
internal sealed class Pitch2
{
    readonly PitchShifter a = new(), b = new();
    public void Init(int hz, float ms = 55f) { a.Configure(hz, ms); b.Configure(hz, ms); a.Reset(); b.Reset(); }
    public void Run(ref float l, ref float r, double ratio) { l = a.Run(l, ratio); r = b.Run(r, ratio); }
}

/// <summary>声の高さ + 帯域 + 歪み + 響きの組み合わせ</summary>
internal sealed class VoiceK(FxA2 e, double semi, double hp, double lp, float drive, float reverb, double size = .6, double peakF = 0, double peakDb = 0) : KB(e)
{
    readonly Pitch2 ps = new(); readonly StereoBiquad h = new(), o = new(), pk = new(); readonly Reverb rv = new();
    protected override void Init() { ps.Init(Hz); h.Reset(); o.Reset(); pk.Reset(); rv.Configure(Hz, (float)size, .35f, 12f); rv.Reset(); }
    protected override void Tick() { h.HighPass(hp, Hz); o.LowPass(lp, Hz); pk.Peak(peakF > 0 ? peakF : 1000, Hz, 1.1, peakDb); }
    protected override void Run(ref float l, ref float r)
    {
        if (semi != 0) ps.Run(ref l, ref r, Math.Pow(2, semi * (A * 2) / 12));
        h.Run(ref l, ref r); if (peakDb != 0) pk.Run(ref l, ref r); o.Run(ref l, ref r);
        if (drive > 0) { float g = 1f + drive * A * 2f; l = MathF.Tanh(l * g) / MathF.Sqrt(g); r = MathF.Tanh(r * g) / MathF.Sqrt(g); }
        if (reverb > 0) { rv.Process(l, r, out var wl, out var wr); float w = reverb * (.4f + A * 1.2f); l += wl * w; r += wr * w; }
    }
}

[AudioEffect("ヘリウムの声", ["演出：声のキャラ"], ["ヘリウム", "高い声", "キンキン", "面白い声", "ボイスチェンジャー", "演出"])]
public sealed class HeliumVoiceEffect : FxA2 { public override string Label => "ヘリウムの声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 8, 180, 14000, 0, 0); }

[AudioEffect("巨人の声", ["演出：声のキャラ"], ["巨人", "低い声", "大きい", "ボス", "ボイスチェンジャー", "演出"])]
public sealed class GiantVoiceEffect : FxA2 { public override string Label => "巨人の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -8, 40, 5500, 0, .55f, .8, 160, 4); }

[AudioEffect("悪魔の声", ["演出：声のキャラ"], ["悪魔", "デーモン", "怖い声", "低い声", "ホラー", "演出"])]
public sealed class DemonVoiceEffect : FxA2
{
    public override string Label => "悪魔の声";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly Pitch2 lo = new(), lo2 = new(); readonly StereoBiquad lp = new(); readonly Reverb rv = new();
        protected override void Init() { lo.Init(Hz); lo2.Init(Hz, 70); lp.Reset(); rv.Configure(Hz, .7f, .4f, 10); rv.Reset(); }
        protected override void Tick() => lp.LowPass(4200, Hz);
        protected override void Run(ref float l, ref float r)
        {
            float al = l, ar = r, bl = l, br = r; lo.Run(ref al, ref ar, Math.Pow(2, -(4 + A * 6) / 12)); lo2.Run(ref bl, ref br, Math.Pow(2, -(12 + A * 3) / 12));
            float g = 1.5f + A * 4f; l = MathF.Tanh((al * .7f + bl * .6f) * g) * .55f; r = MathF.Tanh((ar * .7f + br * .6f) * g) * .55f; lp.Run(ref l, ref r);
            rv.Process(l, r, out var wl, out var wr); l += wl * .4f; r += wr * .4f;
        }
    }
}

[AudioEffect("宇宙人の声", ["演出：声のキャラ"], ["宇宙人", "エイリアン", "alien", "金属", "SF", "演出"])]
public sealed class AlienVoiceEffect : FxA3
{
    public override string Label => "宇宙人の声";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        readonly Pitch2 ps = new(); Lfo w, c;
        protected override void Init() { ps.Init(Hz, 40); w.Reset(); c.Reset(); }
        protected override void Run(ref float l, ref float r)
        {
            w.Step(2 + S * 12, Hz); c.Step(220 + A * 700, Hz); float x = l, y = r; ps.Run(ref x, ref y, Math.Pow(2, (3 + w.Sin * (1 + A * 3)) / 12));
            float m = (float)c.Sin, ring = .35f + A * .5f; l = x * (1 - ring) + x * m * ring * 1.4f; r = y * (1 - ring) + y * m * ring * 1.4f;
        }
    }
}

[AudioEffect("幽霊の声", ["演出：声のキャラ"], ["幽霊", "おばけ", "ghost", "ホラー", "響き", "演出"])]
public sealed class GhostVoiceEffect : FxA2
{
    public override string Label => "幽霊の声";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad bp = new(); readonly Reverb rv = new(); readonly DelayLine dl = new(), dr = new(); Lfo lfo;
        protected override void Init() { bp.Reset(); rv.Configure(Hz, .92f, .15f, 45); rv.Reset(); int n = Hz / 2 + 8; dl.Resize(n); dr.Resize(n); dl.Reset(); dr.Reset(); lfo.Reset(); }
        protected override void Tick() => bp.BandPass(1300, Hz, .9);
        protected override void Run(ref float l, ref float r)
        {
            lfo.Step(.35, Hz); bp.Run(ref l, ref r); float g = (float)(.75 + .25 * lfo.Sin); l *= 1.4f * g; r *= 1.4f * g;
            dl.Write(l); dr.Write(r); float el = dl.Read(.21 * Hz) * .35f, er = dr.Read(.27 * Hz) * .35f;
            rv.Process(l + el, r + er, out var wl, out var wr); float w = .5f + A * 1.3f; l = l * .55f + wl * w; r = r * .55f + wr * w;
        }
    }
}

[AudioEffect("ゾンビの声", ["演出：声のキャラ"], ["ゾンビ", "zombie", "ガラガラ", "うめき", "ホラー", "演出"])]
public sealed class ZombieVoiceEffect : FxA2
{
    public override string Label => "ゾンビの声";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly Pitch2 ps = new(); readonly StereoBiquad bp = new(); WhiteNoise n = new(55); float gate = 1;
        protected override void Init() { ps.Init(Hz); bp.Reset(); }
        protected override void Tick() { bp.BandPass(700, Hz, .55); gate = n.Next() > (.2f - A * .9f) ? 1f : .15f; }
        protected override void Run(ref float l, ref float r)
        {
            ps.Run(ref l, ref r, Math.Pow(2, -(3 + A * 4) / 12)); bp.Run(ref l, ref r); float d = 3f + A * 7f;
            l = MathF.Tanh(l * d) * .5f * gate; r = MathF.Tanh(r * d) * .5f * gate;
        }
    }
}

[AudioEffect("黒幕の声", ["演出：声のキャラ"], ["黒幕", "匿名", "正体を隠す", "変声", "インタビュー", "演出"])]
public sealed class AnonymousVoiceEffect : FxA2 { public override string Label => "黒幕の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -5, 110, 3000, .35f, .06f, .35, 500, 4); }

[AudioEffect("機械音声", ["演出：声のキャラ"], ["機械音声", "合成音声", "ボコーダー", "vocoder", "ロボ", "演出"])]
public sealed class VocoderEffect : FxA2
{
    public override string Label => "機械音声";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        const int B = 12;
        readonly Biquad[] an = new Biquad[B], sy = new Biquad[B]; readonly Envelope[] env = new Envelope[B]; double ph;
        protected override void Init()
        {
            for (int i = 0; i < B; i++) { double f = 140 * Math.Pow(5200.0 / 140, i / (B - 1.0)); (an[i] ??= new()).Reset(); an[i].BandPass(f, Hz, 5); (sy[i] ??= new()).Reset(); sy[i].BandPass(f, Hz, 5); (env[i] ??= new()).Reset(); env[i].Set(4, 28, Hz); }
            ph = 0;
        }
        protected override void Run(ref float l, ref float r)
        {
            ph += (70 + A * 190) / Hz; ph -= Math.Floor(ph); float saw = (float)(ph * 2 - 1), x = (l + r) * .5f, o = 0;
            for (int i = 0; i < B; i++) { float g = env[i].Run(an[i].Run(x)); o += sy[i].Run(saw) * g; }
            o *= 11f; o = MathF.Tanh(o); l = o; r = o;
        }
    }
}

[AudioEffect("ダブリング", ["演出：声の重ね"], ["ダブリング", "doubling", "2人", "重ねる", "厚み", "演出"])]
public sealed class DoublingEffect : FxA2
{
    public override string Label => "ダブリング";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new(); Lfo la, lb;
        protected override void Init() { int n = Hz / 8 + 8; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); la.Reset(); lb.Reset(.4); }
        protected override void Run(ref float l, ref float r)
        {
            la.Step(.37, Hz); lb.Step(.51, Hz); a.Write(l); b.Write(r);
            float w = .45f + A * .5f, x = a.Read((.024 + .0016 * la.Sin) * Hz), y = b.Read((.037 + .0018 * lb.Sin) * Hz); float dl = l, dr = r;
            l = (dl + x * w + y * w * .25f) / (1 + w * .55f); r = (dr + y * w + x * w * .25f) / (1 + w * .55f);
        }
    }
}

[AudioEffect("3人の声", ["演出：声の重ね"], ["3人", "三人", "同時に話す", "ハモり", "重ねる", "演出"])]
public sealed class TrioVoiceEffect : FxA2
{
    public override string Label => "3人の声";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly Pitch2 hi = new(), lo = new(); readonly DelayLine dh = new(), dlw = new();
        protected override void Init() { hi.Init(Hz); lo.Init(Hz, 70); int n = Hz / 8; dh.Resize(n); dlw.Resize(n); dh.Reset(); dlw.Reset(); }
        protected override void Run(ref float l, ref float r)
        {
            float hl = l, hr = r, ll = l, lr = r; hi.Run(ref hl, ref hr, Math.Pow(2, (3 + A * 4) / 12)); lo.Run(ref ll, ref lr, Math.Pow(2, -(3 + A * 4) / 12));
            dh.Write((hl + hr) * .5f); dlw.Write((ll + lr) * .5f); float h = dh.Read(.021 * Hz), w = dlw.Read(.034 * Hz);
            l = (l * .75f + h * .75f + w * .3f) * .68f; r = (r * .75f + w * .75f + h * .3f) * .68f;
        }
    }
}

[AudioEffect("合唱", ["演出：声の重ね"], ["合唱", "大勢", "群衆", "choir", "広がる", "演出"])]
public sealed class ChoirEffect : FxA2
{
    public override string Label => "合唱";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        const int N = 8;
        readonly DelayLine d = new(); readonly double[] ph = new double[N]; readonly Reverb rv = new();
        protected override void Init() { d.Resize(Hz / 5 + 8); d.Reset(); for (int i = 0; i < N; i++) ph[i] = i * .137; rv.Configure(Hz, .68f, .4f, 18); rv.Reset(); }
        protected override void Run(ref float l, ref float r)
        {
            float x = (l + r) * .5f, ol = 0, or = 0; d.Write(x);
            for (int i = 0; i < N; i++) { ph[i] += (.18 + i * .043) / Hz; ph[i] -= Math.Floor(ph[i]); float v = d.Read((.018 + i * .0065 + (.0012 + A * .0035) * Math.Sin(ph[i] * Math.Tau)) * Hz); float pan = i / (N - 1f); ol += v * (1 - pan); or += v * pan; }
            ol *= .46f; or *= .46f; rv.Process(ol, or, out var wl, out var wr); l = l * .45f + ol + wl * (.2f + A * .5f); r = r * .45f + or + wr * (.2f + A * .5f);
        }
    }
}

[AudioEffect("ヘルメット越し", ["演出：場所の声"], ["ヘルメット", "宇宙服", "こもった声", "マスク", "演出"])]
public sealed class HelmetVoiceEffect : FxA2
{
    public override string Label => "ヘルメット越し";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad bp = new(), pk = new(); readonly DelayLine dl = new(), dr = new(); float fl, fr;
        protected override void Init() { bp.Reset(); pk.Reset(); int n = Hz / 50 + 8; dl.Resize(n); dr.Resize(n); dl.Reset(); dr.Reset(); fl = fr = 0; }
        protected override void Tick() { bp.BandPass(1000, Hz, .55); pk.Peak(620, Hz, 3, 4 + A * 7); }
        protected override void Run(ref float l, ref float r)
        {
            float fb = .35f + A * .4f; dl.Write(l + fl * fb); dr.Write(r + fr * fb); fl = dl.Read(.0042 * Hz); fr = dr.Read(.0051 * Hz);
            l = l * .6f + fl * .7f; r = r * .6f + fr * .7f; bp.Run(ref l, ref r); pk.Run(ref l, ref r);
        }
    }
}

[AudioEffect("駅のアナウンス", ["演出：場所の声"], ["駅", "アナウンス", "構内放送", "電車", "スピーカー", "演出"])]
public sealed class StationPaEffect : FxA2 { public override string Label => "駅のアナウンス"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 420, 3800, .45f, .75f, .85, 1900, 5); }

[AudioEffect("館内放送", ["演出：場所の声"], ["館内放送", "校内放送", "天井スピーカー", "ピンポンパンポン", "演出"])]
public sealed class IntercomEffect : FxA2 { public override string Label => "館内放送"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 520, 3200, .6f, .32f, .55, 1500, 6); }

[AudioEffect("テレビ越し", ["演出：場所の声"], ["テレビ", "別の部屋", "遠くのテレビ", "生活音", "演出"])]
public sealed class TvNextRoomEffect : FxA2 { public override string Label => "テレビ越し"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 240, 4200, .15f, .32f, .3, 1200, 3); }

[AudioEffect("スマホのスピーカー", ["演出：場所の声"], ["スマホ", "スピーカー", "小さなスピーカー", "phone", "動画の音", "演出"])]
public sealed class PhoneSpeakerEffect : FxA2 { public override string Label => "スマホのスピーカー"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 620, 8500, .3f, 0, .3, 2600, 6); }

[AudioEffect("イヤホンの音漏れ", ["演出：場所の声"], ["イヤホン", "音漏れ", "シャカシャカ", "電車", "演出"])]
public sealed class EarbudLeakEffect : FxA2
{
    public override string Label => "イヤホンの音漏れ";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad hp = new(), hp2 = new(), pk = new();
        protected override void Init() { hp.Reset(); hp2.Reset(); pk.Reset(); }
        protected override void Tick() { hp.HighPass(2600, Hz); hp2.HighPass(2600, Hz); pk.Peak(6000, Hz, .9, 6); }
        protected override void Run(ref float l, ref float r) { hp.Run(ref l, ref r); hp2.Run(ref l, ref r); pk.Run(ref l, ref r); float g = .12f + A * .7f; l = MathF.Tanh(l * 3f) * g; r = MathF.Tanh(r * 3f) * g; }
    }
}

[AudioEffect("ドライブスルー", ["演出：場所の声"], ["ドライブスルー", "インターホン", "小さなマイク", "ノイズ", "演出"])]
public sealed class DriveThruEffect : FxA2
{
    public override string Label => "ドライブスルー";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad bp = new(); readonly Biquad nf = new(); WhiteNoise n = new(321); Lfo c;
        protected override void Init() { bp.Reset(); nf.Reset(); nf.BandPass(2200, Hz, .8); c.Reset(); }
        protected override void Tick() => bp.BandPass(1500, Hz, 1.25);
        protected override void Run(ref float l, ref float r)
        {
            c.Step(1.3, Hz); bp.Run(ref l, ref r); float g = 3f + A * 7f, h = nf.Run(n.Next()) * (.012f + A * .05f) * (float)(.7 + .3 * c.Sin);
            l = MathF.Tanh(l * g) * .6f + h; r = MathF.Tanh(r * g) * .6f + h;
        }
    }
}
