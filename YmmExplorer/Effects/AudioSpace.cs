using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// =====================================================================================================
//  響き・場所
// =====================================================================================================

public enum RoomKind
{
    [Display(Name = "お風呂")] Bath,
    [Display(Name = "洞窟")] Cave,
    [Display(Name = "トンネル")] Tunnel,
    [Display(Name = "体育館")] Gym,
    [Display(Name = "小さな部屋")] SmallRoom,
}

[AudioEffect("お風呂・洞窟の響き", ["演出：響き・場所"], ["お風呂", "洞窟", "トンネル", "体育館", "エコー", "リバーブ", "reverb", "響き", "反響", "演出"])]
public sealed class RoomReverbEffect : FxAudioEffectBase
{
    public override string Label => "お風呂・洞窟の響き";

    [Display(GroupName = "響き", Name = "場所")]
    [EnumComboBox]
    public RoomKind Kind { get => kind; set => Set(ref kind, value); }
    RoomKind kind = RoomKind.Bath;

    [Display(GroupName = "響き", Name = "響きの量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Wet { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "響き", Name = "元の音")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Dry { get; } = new Animation(90, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Wet, Dry];
    internal override AudioKernel CreateKernel() => new K(this);

    internal static (float room, float damp, float pre, float scale, float hi) Preset(RoomKind k) => k switch
    {
        RoomKind.Bath => (0.75f, 0.15f, 8, 0.55f, 9000),
        RoomKind.Cave => (0.96f, 0.45f, 45, 1.8f, 5000),
        RoomKind.Tunnel => (0.92f, 0.3f, 30, 1.4f, 6500),
        RoomKind.Gym => (0.88f, 0.25f, 25, 1.3f, 8000),
        _ => (0.5f, 0.5f, 3, 0.4f, 7000),
    };

    sealed class K(RoomReverbEffect e) : AudioKernel
    {
        readonly Reverb rv = new();
        readonly StereoBiquad lp = new();
        float wet, dry; RoomKind? last; int hzSet;
        public override void Reset(int hz) { last = null; hzSet = hz; rv.Reset(); lp.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            if (last != e.Kind || hzSet != hz)
            {
                last = e.Kind; hzSet = hz;
                var p = Preset(e.Kind);
                rv.Configure(hz, p.room, p.damp, p.pre, p.scale);
                lp.LowPass(p.hi, hz);
            }
            wet = (float)e.Wet.GetValue(f, t, hz) / 100f * 3f; dry = (float)e.Dry.GetValue(f, t, hz) / 100f;
        }
        public override void Process(ref float l, ref float r)
        {
            rv.Process(l, r, out var wl, out var wr);
            lp.Run(ref wl, ref wr);
            l = l * dry + wl * wet; r = r * dry + wr * wet;
        }
    }
}

[AudioEffect("ホール・教会の響き", ["演出：響き・場所"], ["ホール", "教会", "コンサート", "大聖堂", "リバーブ", "reverb", "響き", "壮大", "演出"])]
public sealed class HallReverbEffect : FxAudioEffectBase
{
    public override string Label => "ホール・教会の響き";

    [Display(GroupName = "ホール", Name = "広さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Size { get; } = new Animation(75, 0, 100);

    [Display(GroupName = "ホール", Name = "高音の吸収", Description = "大きいほど、響きがやわらかく暗くなります")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Damp { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "ホール", Name = "響きが始まるまで")]
    [AnimationSlider("F0", "ms", 0, 150)]
    public Animation PreDelay { get; } = new Animation(30, 0, 400);

    [Display(GroupName = "ホール", Name = "響きの量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Wet { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "ホール", Name = "元の音")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Dry { get; } = new Animation(85, 0, 100);

    [Display(GroupName = "ホール", Name = "左右の広がり")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Width { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Size, Damp, PreDelay, Wet, Dry, Width];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(HallReverbEffect e) : AudioKernel
    {
        readonly Reverb rv = new();
        float wet, dry, width;
        public override void Reset(int hz) { rv.Configure(hz, 0.8f, 0.4f, 30, 1.6f); rv.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            rv.Configure(hz, (float)e.Size.GetValue(f, t, hz) / 100f, (float)e.Damp.GetValue(f, t, hz) / 100f, (float)e.PreDelay.GetValue(f, t, hz), 1.6f);
            wet = (float)e.Wet.GetValue(f, t, hz) / 100f * 3f; dry = (float)e.Dry.GetValue(f, t, hz) / 100f; width = (float)e.Width.GetValue(f, t, hz) / 100f;
        }
        public override void Process(ref float l, ref float r)
        {
            rv.Process(l, r, out var wl, out var wr);
            float mid = (wl + wr) * 0.5f, side = (wl - wr) * 0.5f * width;
            l = l * dry + (mid + side) * wet; r = r * dry + (mid - side) * wet;
        }
    }
}

[AudioEffect("山びこ（やまびこ）", ["演出：響き・場所"], ["山びこ", "やまびこ", "こだま", "エコー", "echo", "ディレイ", "delay", "繰り返し", "演出"])]
public sealed class MountainEchoEffect : FxAudioEffectBase
{
    public override string Label => "山びこ（やまびこ）";

    [Display(GroupName = "山びこ", Name = "返ってくるまで")]
    [AnimationSlider("F0", "ms", 100, 1500)]
    public Animation Time { get; } = new Animation(450, 10, 3000);

    [Display(GroupName = "山びこ", Name = "くり返し", Description = "大きいほど何度も返ってきます")]
    [AnimationSlider("F0", "%", 0, 90)]
    public Animation Feedback { get; } = new Animation(45, 0, 95);

    [Display(GroupName = "山びこ", Name = "返ってくる音の大きさ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Wet { get; } = new Animation(50, 0, 100);

    [Display(GroupName = "山びこ", Name = "左右に跳ね返る")]
    public bool PingPong { get => pingPong; set => Set(ref pingPong, value); }
    bool pingPong = true;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Time, Feedback, Wet];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(MountainEchoEffect e) : AudioKernel
    {
        readonly DelayLine dl = new(), dr = new();
        readonly Biquad fl = new(), fr = new(), hl = new(), hr = new();
        double d; float fb, wet; bool pp;
        public override void Reset(int hz) { dl.Resize(hz * 3 + 8); dr.Resize(hz * 3 + 8); fl.Reset(); fr.Reset(); hl.Reset(); hr.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            d = e.Time.GetValue(f, t, hz) * 0.001 * hz;
            fb = (float)e.Feedback.GetValue(f, t, hz) / 100f; wet = (float)e.Wet.GetValue(f, t, hz) / 100f; pp = e.PingPong;
            fl.LowPass(3500, hz); fr.LowPass(3500, hz); hl.HighPass(200, hz); hr.HighPass(200, hz);
        }
        public override void Process(ref float l, ref float r)
        {
            // 返ってくるたびに、高い音と低い音が少しずつ減る(遠くの山で跳ね返った感じ)
            float el = hl.Run(fl.Run(dl.Read(d))), er = hr.Run(fr.Run(dr.Read(d)));
            if (pp) { dl.Write((l + r) * 0.5f + er * fb); dr.Write(el * fb); }
            else { dl.Write(l + el * fb); dr.Write(r + er * fb); }
            l += el * wet; r += er * wet;
        }
    }
}

[AudioEffect("壁の向こう・隣の部屋", ["演出：響き・場所"], ["壁越し", "壁の向こう", "隣の部屋", "ドア越し", "こもった音", "マンション", "盗み聞き", "演出"])]
public sealed class ThroughWallEffect : FxAudioEffectBase
{
    public override string Label => "壁の向こう・隣の部屋";

    [Display(GroupName = "壁越し", Name = "壁の厚さ", Description = "厚いほどこもって、低い音しか届きません")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Thick { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "壁越し", Name = "部屋の響き")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Room { get; } = new Animation(25, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Thick, Room];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(ThroughWallEffect e) : AudioKernel
    {
        readonly StereoBiquad a = new(), b = new(), pk = new();
        readonly Reverb rv = new();
        float room, gain;
        public override void Reset(int hz) { a.Reset(); b.Reset(); pk.Reset(); rv.Configure(hz, 0.55f, 0.6f, 5, 0.5f); rv.Reset(); }
        public override void Params(long f, long t, int hz)
        {
            float th = (float)e.Thick.GetValue(f, t, hz) / 100f;
            double cut = 1800 * Math.Pow(0.12, th);   // 1800Hz 〜 220Hz
            a.LowPass(cut, hz); b.LowPass(cut, hz); pk.Peak(180, hz, 1, 3 + th * 3);
            gain = Db.ToGain(-4 - th * 10);
            room = (float)e.Room.GetValue(f, t, hz) / 100f * 2.5f;
        }
        public override void Process(ref float l, ref float r)
        {
            rv.Process(l, r, out var wl, out var wr);
            l += wl * room; r += wr * room;
            a.Run(ref l, ref r); b.Run(ref l, ref r); pk.Run(ref l, ref r);
            l *= gain; r *= gain;
        }
    }
}

[AudioEffect("水の中", ["演出：響き・場所"], ["水中", "水の中", "underwater", "プール", "海", "溺れる", "ゴボゴボ", "こもった音", "演出"])]
public sealed class UnderwaterEffect : FxAudioEffectBase
{
    public override string Label => "水の中";

    [Display(GroupName = "水の中", Name = "深さ", Description = "深いほどこもって、ゆらぎが大きくなります")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Depth { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "水の中", Name = "ゆらぎ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Wobble { get; } = new Animation(50, 0, 100);

    [Display(GroupName = "水の中", Name = "泡の音")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Bubbles { get; } = new Animation(20, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Depth, Wobble, Bubbles];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(UnderwaterEffect e) : AudioKernel
    {
        readonly StereoBiquad a = new(), b = new();
        readonly DelayLine dl = new(), dr = new();
        readonly Reverb rv = new();
        readonly Biquad bub = new();
        WhiteNoise rnd = new(3);
        float wob, bubbles; double ph, inc, ms, bph, binc, benv;
        public override void Reset(int hz) { a.Reset(); b.Reset(); dl.Resize(hz / 20 + 8); dr.Resize(hz / 20 + 8); rv.Configure(hz, 0.7f, 0.8f, 10, 0.7f); rv.Reset(); ph = 0; benv = 0; ms = hz / 1000.0; }
        public override void Params(long f, long t, int hz)
        {
            float dep = (float)e.Depth.GetValue(f, t, hz) / 100f;
            double cut = 1200 * Math.Pow(0.25, dep);
            a.LowPass(cut, hz, 1.1); b.LowPass(cut * 1.3, hz);
            wob = (float)e.Wobble.GetValue(f, t, hz) / 100f * (0.5f + dep);
            bubbles = (float)e.Bubbles.GetValue(f, t, hz) / 100f;
            inc = 2 * Math.PI * 0.8 / hz;
        }
        public override void Process(ref float l, ref float r)
        {
            ph += inc;
            dl.Write(l); dr.Write(r);
            double d = ms * (8 + 6 * wob * Math.Sin(ph)), d2 = ms * (8 + 6 * wob * Math.Sin(ph * 1.31 + 1));
            float il = dl.Read(d), ir = dr.Read(d2);
            rv.Process(il, ir, out var wl, out var wr);
            il += wl * 1.5f; ir += wr * 1.5f;
            a.Run(ref il, ref ir); b.Run(ref il, ref ir);
            // 泡: 高さが上がっていく短い音を、ときどき鳴らす
            if (bubbles > 0 && benv < 0.01 && rnd.Next() > 1 - 0.00008f * bubbles) { benv = 0.15 * bubbles + 0.05; bph = 0; binc = 2 * Math.PI * (300 + 500 * (rnd.Next() + 1)) / (ms * 1000); }
            float bs = 0;
            if (benv > 0.001) { bph += binc; binc *= 1.00004; bs = (float)(Math.Sin(bph) * benv); benv *= 0.9993; }
            l = il * 1.3f + bs; r = ir * 1.3f + bs;
        }
    }
}

[AudioEffect("遠くから聞こえる", ["演出：響き・場所"], ["遠く", "遠くから", "距離", "far away", "遠い", "呼び声", "外から", "演出"])]
public sealed class FarAwayEffect : FxAudioEffectBase
{
    public override string Label => "遠くから聞こえる";

    [Display(GroupName = "遠く", Name = "距離")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Distance { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "遠く", Name = "屋外", Description = "オンで響きが少なく、オフで建物の中の響き")]
    public bool Outdoor { get => outdoor; set => Set(ref outdoor, value); }
    bool outdoor = true;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Distance];
    internal override AudioKernel CreateKernel() => new K(this);

    sealed class K(FarAwayEffect e) : AudioKernel
    {
        readonly StereoBiquad lp = new(), hp = new();
        readonly Reverb rv = new();
        float dist, dry, wet; bool? lastOut; int hzSet;
        public override void Reset(int hz) { lp.Reset(); hp.Reset(); rv.Reset(); lastOut = null; hzSet = hz; }
        public override void Params(long f, long t, int hz)
        {
            if (lastOut != e.Outdoor || hzSet != hz)
            {
                lastOut = e.Outdoor; hzSet = hz;
                if (e.Outdoor) rv.Configure(hz, 0.6f, 0.7f, 60, 1.2f); else rv.Configure(hz, 0.85f, 0.45f, 25, 1.1f);
            }
            dist = (float)e.Distance.GetValue(f, t, hz) / 100f;
            lp.LowPass(9000 * Math.Pow(0.18, dist), hz); hp.HighPass(80 + 300 * dist, hz);
            dry = Db.ToGain(-dist * 24); wet = (e.Outdoor ? 0.8f : 1.8f) * (0.3f + dist);
        }
        public override void Process(ref float l, ref float r)
        {
            float m = (l + r) * 0.5f;          // 遠い音は左右の差がほとんどない
            l = l * 0.3f + m * 0.7f; r = r * 0.3f + m * 0.7f;
            rv.Process(l, r, out var wl, out var wr);
            float ol = l * dry + wl * wet * 0.35f, or2 = r * dry + wr * wet * 0.35f;
            lp.Run(ref ol, ref or2); hp.Run(ref ol, ref or2);
            l = ol; r = or2;
        }
    }
}
