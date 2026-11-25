using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 83〜100: 空間・時間

/// <summary>場所の響き(大きさ・吸音・初期の遅れ・音色)。強さは響きの量</summary>
internal sealed class RoomK(FxA2 e, float size, float damp, float preMs, float scale, double hp, double lp, float wet, float dry = 1f) : KB(e)
{
    readonly Reverb rv = new(); readonly StereoBiquad h = new(), o = new();
    protected override void Init() { rv.Configure(Hz, size, damp, preMs, scale); rv.Reset(); h.Reset(); o.Reset(); h.HighPass(hp, Hz); o.LowPass(lp, Hz); }
    protected override void Run(ref float l, ref float r)
    {
        rv.Process(l, r, out var wl, out var wr); h.Run(ref wl, ref wr); o.Run(ref wl, ref wr);
        float w = wet * (.25f + A * 1.5f); l = l * dry + wl * w; r = r * dry + wr * w;
    }
}

[AudioEffect("小さな部屋", ["演出：場所の響き"], ["部屋", "room", "狭い", "リバーブ", "響き", "演出"])]
public sealed class SmallRoomEffect : FxA2 { public override string Label => "小さな部屋"; internal override AudioKernel CreateKernel() => new RoomK(this, .25f, .7f, 3, .45f, 160, 7000, .75f); }

[AudioEffect("録音スタジオ", ["演出：場所の響き"], ["スタジオ", "studio", "控えめ", "リバーブ", "響き", "演出"])]
public sealed class StudioRoomEffect : FxA2 { public override string Label => "録音スタジオ"; internal override AudioKernel CreateKernel() => new RoomK(this, .42f, .8f, 8, .7f, 220, 9000, .5f); }

[AudioEffect("体育館", ["演出：場所の響き"], ["体育館", "gym", "広い", "硬い響き", "リバーブ", "演出"])]
public sealed class GymRoomEffect : FxA2 { public override string Label => "体育館"; internal override AudioKernel CreateKernel() => new RoomK(this, .86f, .18f, 42, 1.5f, 200, 9000, .95f, .85f); }

[AudioEffect("大聖堂", ["演出：場所の響き"], ["大聖堂", "cathedral", "教会", "厳か", "長い響き", "リバーブ", "演出"])]
public sealed class CathedralEffect : FxA2 { public override string Label => "大聖堂"; internal override AudioKernel CreateKernel() => new RoomK(this, .97f, .3f, 70, 2.2f, 180, 6500, 1.1f, .72f); }

[AudioEffect("トンネル", ["演出：場所の響き"], ["トンネル", "tunnel", "筒", "反響", "リバーブ", "演出"])]
public sealed class TunnelEffect : FxA2
{
    public override string Label => "トンネル";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly Reverb rv = new(); readonly DelayLine a = new(), b = new(); readonly StereoBiquad bp = new(); float fl, fr;
        protected override void Init() { rv.Configure(Hz, .8f, .35f, 20, 1.1f); rv.Reset(); int n = Hz / 3; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); bp.Reset(); bp.BandPass(620, Hz, .55); fl = fr = 0; }
        protected override void Run(ref float l, ref float r)
        {
            float fb = .35f + A * .32f; a.Write(l + fl * fb); b.Write(r + fr * fb); fl = a.Read(.083 * Hz); fr = b.Read(.097 * Hz);
            float xl = fl, xr = fr; bp.Run(ref xl, ref xr); rv.Process(l + xl, r + xr, out var wl, out var wr); float w = .35f + A * .9f; l = l * .78f + xl * .75f + wl * w; r = r * .78f + xr * .75f + wr * w;
        }
    }
}

[AudioEffect("宇宙空間", ["演出：場所の響き"], ["宇宙", "space", "幻想的", "無限", "リバーブ", "SF", "演出"])]
public sealed class SpaceVerbEffect : FxA2 { public override string Label => "宇宙空間"; internal override AudioKernel CreateKernel() => new RoomK(this, 1f, .05f, 110, 3f, 320, 5200, 1.35f, .55f); }

[AudioEffect("森の中", ["演出：場所の響き"], ["森", "forest", "自然", "屋外", "反響", "演出"])]
public sealed class ForestVerbEffect : FxA2
{
    public override string Label => "森の中";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new(); readonly StereoBiquad lp = new(); readonly Reverb rv = new();
        static readonly (double t, float g)[] Taps = [(.071, .32f), (.123, .24f), (.187, .18f), (.263, .13f), (.351, .09f)];
        protected override void Init() { int n = Hz / 2; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); lp.Reset(); lp.LowPass(2600, Hz); rv.Configure(Hz, .55f, .9f, 30, 1f); rv.Reset(); }
        protected override void Run(ref float l, ref float r)
        {
            a.Write(l); b.Write(r); float xl = 0, xr = 0; for (int i = 0; i < Taps.Length; i++) { float vl = a.Read(Taps[i].t * Hz) * Taps[i].g, vr = b.Read((Taps[i].t + .009 * (i + 1)) * Hz) * Taps[i].g; xl += i % 2 == 0 ? vl : vl * .4f; xr += i % 2 == 1 ? vr : vr * .4f; }
            lp.Run(ref xl, ref xr); rv.Process(xl, xr, out var wl, out var wr); float w = .5f + A * 1.3f; l = l * .9f + (xl + wl * .5f) * w; r = r * .9f + (xr + wr * .5f) * w;
        }
    }
}

[AudioEffect("車の中", ["演出：場所の響き"], ["車", "車内", "car", "こもる", "ドライブ", "演出"])]
public sealed class CarInteriorEffect : FxA2
{
    public override string Label => "車の中";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly StereoBiquad lp = new(), lo = new(); readonly Reverb rv = new();
        protected override void Init() { lp.Reset(); lo.Reset(); rv.Configure(Hz, .12f, .95f, 1, .3f); rv.Reset(); }
        protected override void Tick() { lp.LowPass(Exp(9000, 2600, A), Hz); lo.Peak(140, Hz, 1.2, 2 + A * 6); }
        protected override void Run(ref float l, ref float r) { lp.Run(ref l, ref r); lo.Run(ref l, ref r); rv.Process(l, r, out var wl, out var wr); l = l * .85f + wl * .45f; r = r * .85f + wr * .45f; }
    }
}

[AudioEffect("ピンポンディレイ", ["演出：エコー"], ["ピンポン", "ping pong", "ディレイ", "左右", "エコー", "演出"])]
public sealed class PingPongDelayEffect : FxA3
{
    public override string Label => "ピンポンディレイ";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new();
        protected override void Init() { int n = Hz + 8; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); }
        protected override void Run(ref float l, ref float r)
        {
            double d = (.08 + (1 - S) * .55) * Hz; float x = a.Read(d), y = b.Read(d), fb = .25f + A * .55f;
            a.Write((l + r) * .5f + y * fb); b.Write(x * fb); l += x * .75f; r += y * .75f;
        }
    }
}

[AudioEffect("スラップバック", ["演出：エコー"], ["スラップバック", "slapback", "短いエコー", "ロカビリー", "演出"])]
public sealed class SlapbackEffect : FxA2
{
    public override string Label => "スラップバック";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new(); readonly StereoBiquad o = new();
        protected override void Init() { int n = Hz / 3; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); o.Reset(); o.BandPass(1800, Hz, .45); }
        protected override void Run(ref float l, ref float r) { a.Write(l); b.Write(r); float x = a.Read((.07 + A * .08) * Hz), y = b.Read((.07 + A * .08) * Hz); o.Run(ref x, ref y); l += x * .85f; r += y * .85f; }
    }
}

[AudioEffect("テープエコー", ["演出：エコー"], ["テープエコー", "tape echo", "ダブ", "揺れるエコー", "ディレイ", "演出"])]
public sealed class TapeEchoEffect : FxA3
{
    public override string Label => "テープエコー";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new(); readonly StereoBiquad lp = new(), hp = new(); Lfo w; float fl, fr;
        protected override void Init() { int n = Hz + 16; a.Resize(n); b.Resize(n); a.Reset(); b.Reset(); lp.Reset(); hp.Reset(); lp.LowPass(2800, Hz); hp.HighPass(180, Hz); w.Reset(); fl = fr = 0; }
        protected override void Run(ref float l, ref float r)
        {
            w.Step(.9, Hz); double d = (.12 + (1 - S) * .5 + .0022 * w.Sin) * Hz; float x = a.Read(d), y = b.Read(d * 1.01); lp.Run(ref x, ref y); hp.Run(ref x, ref y); fl = MathF.Tanh(x * 1.2f); fr = MathF.Tanh(y * 1.2f);
            float fb = .3f + A * .55f; a.Write(l + fl * fb); b.Write(r + fr * fb); l += fl * .8f; r += fr * .8f;
        }
    }
}

[AudioEffect("シマー", ["演出：エコー"], ["シマー", "shimmer", "きらきら", "天国", "幻想的", "リバーブ", "演出"])]
public sealed class ShimmerEffect : FxA2
{
    public override string Label => "シマー";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly Reverb rv = new(); readonly Pitch2 up = new(); readonly StereoBiquad hp = new(); float fl, fr;
        protected override void Init() { rv.Configure(Hz, .9f, .1f, 25, 1.6f); rv.Reset(); up.Init(Hz, 80); hp.Reset(); hp.HighPass(500, Hz); fl = fr = 0; }
        protected override void Run(ref float l, ref float r)
        {
            float xl = fl, xr = fr; up.Run(ref xl, ref xr, 2); hp.Run(ref xl, ref xr); float fb = .12f + A * .3f;
            rv.Process(l + xl * fb, r + xr * fb, out fl, out fr); fl = MathF.Tanh(fl); fr = MathF.Tanh(fr); float w = .45f + A * .8f; l = l * .75f + fl * w; r = r * .75f + fr * w;
        }
    }
}

[AudioEffect("ゲートリバーブ", ["演出：エコー"], ["ゲートリバーブ", "gated reverb", "80年代", "スネア", "切れる響き", "演出"])]
public sealed class GatedReverbEffect : FxA2
{
    public override string Label => "ゲートリバーブ";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly Reverb rv = new(); readonly Envelope env = new(); float g; int hold;
        protected override void Init() { rv.Configure(Hz, .88f, .25f, 4, 1f); rv.Reset(); env.Reset(); env.Set(1, 30, Hz); g = 0; hold = 0; }
        protected override void Run(ref float l, ref float r)
        {
            float v = env.Run(MathF.Max(MathF.Abs(l), MathF.Abs(r))); if (v > .1f - A * .07f) hold = (int)((.05 + A * .25) * Hz); else if (hold > 0) hold--;
            float t = hold > 0 ? 1f : 0f; g += (t - g) * (t > g ? .02f : .004f); rv.Process(l, r, out var wl, out var wr); float w = g * (.6f + A); l = l * .85f + wl * w; r = r * .85f + wr * w;
        }
    }
}

[AudioEffect("スタッター", ["演出：時間"], ["スタッター", "stutter", "ダダダ", "繰り返し", "DJ", "演出"])]
public sealed class StutterEffect : FxA3
{
    public override string Label => "スタッター";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        float[] bl = [], br = []; int cycle, slice, pos;
        protected override void Init() { int n = Hz + 8; if (bl.Length != n) { bl = new float[n]; br = new float[n]; } pos = 0; cycle = 0; slice = 1; }
        protected override void Run(ref float l, ref float r)
        {
            slice = Math.Clamp((int)((.03 + (1 - S) * .17) * Hz), 32, bl.Length - 2); int reps = 2 + (int)(A * 6);
            int round = cycle / slice; int inSlice = cycle % slice;
            if (round == 0) { bl[inSlice] = l; br[inSlice] = r; }
            else { float fade = MathF.Min(1f, MathF.Min(inSlice, slice - 1 - inSlice) / MathF.Max(1f, slice * .08f)); l = bl[inSlice] * fade; r = br[inSlice] * fade; }
            if (++cycle >= slice * reps) cycle = 0;
        }
    }
}

[AudioEffect("トランスゲート", ["演出：時間"], ["トランスゲート", "trance gate", "刻む", "リズム", "ゲート", "演出"])]
public sealed class TranceGateEffect : FxA3
{
    public override string Label => "トランスゲート";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        static readonly bool[] Pattern = [true, false, true, true, false, true, false, true];
        Lfo ph; float g;
        protected override void Init() { ph.Reset(); g = 0; }
        protected override void Run(ref float l, ref float r)
        {
            ph.Step((1 + S * 6) / 8, Hz); double u = ph.Phase * 8; int s = (int)u; float t = Pattern[s % 8] && u - s < .78 ? 1f : 1f - (.25f + A * .75f); g += (t - g) * .012f; l *= g; r *= g;
        }
    }
}

[AudioEffect("グラニュラー", ["演出：時間"], ["グラニュラー", "granular", "粒", "不思議な音", "アンビエント", "演出"])]
public sealed class GranularEffect : FxA3
{
    public override string Label => "グラニュラー";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        const int N = 10;
        readonly DelayLine a = new(), b = new(); readonly double[] start = new double[N], rate = new double[N]; readonly int[] age = new int[N], len = new int[N]; readonly float[] pan = new float[N]; WhiteNoise n = new(4242);
        void Spawn(int i) { len[i] = (int)((.025 + (1 - S) * .16) * Hz * (.6f + (n.Next() + 1f) * .4f)); start[i] = (.04 + (n.Next() + 1f) * .5 * (.1 + A * .7)) * Hz + len[i]; rate[i] = n.Next() > .55f ? 2 : n.Next() < -.7f ? .5 : 1; age[i] = 0; pan[i] = (n.Next() + 1f) * .5f; }
        protected override void Init() { int s = Hz * 2 + 8; a.Resize(s); b.Resize(s); a.Reset(); b.Reset(); for (int i = 0; i < N; i++) { Spawn(i); age[i] = (int)(len[i] * (i / (float)N)); } }
        protected override void Run(ref float l, ref float r)
        {
            a.Write(l); b.Write(r); float ol = 0, or = 0;
            for (int i = 0; i < N; i++)
            {
                if (age[i] >= len[i]) Spawn(i);
                float k = age[i] / (float)Math.Max(1, len[i]), w = MathF.Sin(k * MathF.PI); w *= w; double d = Math.Max(2, start[i] - age[i] * (rate[i] - 1) + 0);
                float v = (a.Read(d) + b.Read(d)) * .5f * w; ol += v * (1 - pan[i]); or += v * pan[i]; age[i]++;
            }
            l = ol * .62f; r = or * .62f;
        }
    }
}

[AudioEffect("途切れる通信", ["演出：時間"], ["途切れる", "通信", "電波が悪い", "ブツブツ", "dropout", "ノイズ", "演出"])]
public sealed class DropoutEffect : FxA3
{
    public override string Label => "途切れる通信";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA3 e) : KB(e)
    {
        WhiteNoise n = new(808); int left; bool on = true; float g = 1; readonly StereoBiquad bp = new();
        protected override void Init() { left = 0; on = true; g = 1; bp.Reset(); bp.BandPass(1500, Hz, .7); }
        protected override void Run(ref float l, ref float r)
        {
            if (--left <= 0) { on = (n.Next() + 1f) * .5f > .15f + A * .55f; left = (int)((.012 + (n.Next() + 1f) * .5f * (.03 + (1 - S) * .16)) * Hz); }
            g += ((on ? 1f : 0f) - g) * .22f; bp.Run(ref l, ref r); float h = (1 - g) * n.Next() * .018f * (A + .2f); l = l * 1.6f * g + h; r = r * 1.6f * g + h;
        }
    }
}

[AudioEffect("エレベーターの中", ["演出：場所の響き"], ["エレベーター", "金属", "箱", "狭い", "響き", "演出"])]
public sealed class ElevatorEffect : FxA2
{
    public override string Label => "エレベーターの中";
    internal override AudioKernel CreateKernel() => new K(this);
    sealed class K(FxA2 e) : KB(e)
    {
        readonly DelayLine a = new(), b = new(), c = new(), d = new(); readonly StereoBiquad bp = new(); float fa, fb, fc, fd;
        protected override void Init() { int n = Hz / 40 + 8; foreach (var x in new[] { a, b, c, d }) { x.Resize(n); x.Reset(); } bp.Reset(); bp.BandPass(1100, Hz, .5); fa = fb = fc = fd = 0; }
        protected override void Run(ref float l, ref float r)
        {
            float g = .45f + A * .42f, ml = (fa + fc) * .5f, mr = (fb + fd) * .5f;
            a.Write(l + ml * g); b.Write(r + mr * g); c.Write(r + mr * g); d.Write(l + ml * g); fa = a.Read(.0037 * Hz); fb = b.Read(.0049 * Hz); fc = c.Read(.0071 * Hz); fd = d.Read(.0093 * Hz);
            float xl = (fa + fd) * .5f, xr = (fb + fc) * .5f; bp.Run(ref xl, ref xr); l = l * .72f + xl * (.45f + A * .7f); r = r * .72f + xr * (.45f + A * .7f);
        }
    }
}
