using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 471〜490: 漫符・感情表現

public sealed class SleepyZzzPlugin : QuickShapePlugin { public override string Name => "演出：Zzz（眠い・寝ている）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SleepyZzzParameter(s); }
public sealed class SleepyZzzParameter : Q3Param
{
    public SleepyZzzParameter() : this(null) { }
    public SleepyZzzParameter(SharedDataStore? s) : base(s) { Text = "Z"; Count = 4; Color1 = Hex(0x8FB8FF); }
    static readonly Dictionary<string, ParamGuide> g = G("Zzz", "Text=文字", "Count=文字の数||1|12", "Variant=浮かぶ向き|1=右上　2=左上　3=真上|1|3", "Speed=速さ", "Color1=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 1, 12); float u = U(c), t = c.Time * Spd(c), dx = Variant == 2 ? -1f : Variant == 3 ? 0f : 1f; var o = P0(c);
        for (int i = 0; i < n; i++) { float k = Gfx.Mod(t * 0.35f + i / (float)n, 1f), al = MathF.Min(k * 5f, 1f) * (1f - Sub(k, 0.7f, 0.3f)); var q = o + new Vector2(dx * k * 240f * u + MathF.Sin(k * 9f) * 26f * u, -k * 320f * u); Say(dc, b, Str, (42f + k * 96f) * u, q, C1(a * al), Rgb(0.05f, 0.08f, 0.2f, 0.75f * a * al), 4f * u); }
    }
}

public sealed class StartleLinesPlugin : QuickShapePlugin { public override string Name => "演出：ビクッ（驚きの跳ね線）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new StartleLinesParameter(s); }
public sealed class StartleLinesParameter : Q3Param
{
    public StartleLinesParameter() : this(null) { }
    public StartleLinesParameter(SharedDataStore? s) : base(s) { Text = "ビクッ"; Count = 14; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x222222); }
    protected override double DefAmount => 40;
    static readonly Dictionary<string, ParamGuide> g = G("ビクッ", "Text=文字", "Count=線の数||4|40", "Amount=跳ねる高さ", "Color1=線と文字の色", "Color2=ふちの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 4, 40); float u = U(c), k = Sub(c.Time, 0f, 0.45f), hop = MathF.Sin(k * MathF.PI) * Amt(c) * 120f * u; var o = P0(c) - new Vector2(0, hop); float fade = 1f - Sub(k, 0.65f, 0.35f);
        for (int i = 0; i < n; i++) { float th = i * MathF.Tau / n, r0 = (210f + 46f * DrawKit.EaseOut(k)) * u, r1 = r0 + (54f + 36f * (i % 2)) * u; Ln(dc, b, Pol(o, r0, th), Pol(o, r1, th), C2(a * fade), 12f * u); Ln(dc, b, Pol(o, r0, th), Pol(o, r1, th), C1(a * fade), 6f * u); }
        Say(dc, b, Str, 72f * u, o - new Vector2(0, 340f * u), C1(a), C2(a), 5f * u, scale: DrawKit.EaseOutBack(Sub(c.Time, 0f, 0.2f)));
    }
}

public sealed class ChiinBellPlugin : QuickShapePlugin { public override string Name => "演出：ちーん（鐘と線香）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ChiinBellParameter(s); }
public sealed class ChiinBellParameter : Q3Param
{
    public ChiinBellParameter() : this(null) { }
    public ChiinBellParameter(SharedDataStore? s) : base(s) { Text = "ちーん"; Count = 3; Color1 = Hex(0xC9A227); Color2 = Hex(0xCFD8DC); }
    static readonly Dictionary<string, ParamGuide> g = G("ちーん", "Text=文字", "Count=煙の量||1|8", "Color1=おりんの色", "Color2=煙の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time; var o = P0(c); var bowl = o + new Vector2(-150f * u, 170f * u);
        Pie(dc, b, bowl - new Vector2(0, 58f * u), 118f * u, 0f, MathF.PI, C1(a)); Oval(dc, b, bowl - new Vector2(0, 58f * u), 118f * u, 18f * u, Col(Gfx.Mix(Color1, Hex(0x000000), 0.35f), a)); RBox(dc, b, bowl + new Vector2(0, 74f * u), 200f * u, 26f * u, 8f * u, Rgb(0.45f, 0.1f, 0.12f, a));
        for (int i = 0; i < 3; i++) { float k = Gfx.Mod(t * 0.55f + i / 3f, 1f); Arc(dc, b, bowl - new Vector2(0, 70f * u), (130f + k * 150f) * u, -2.5f, -0.65f, C1(a * (1f - k)), 5f * u); }
        var stick = o + new Vector2(170f * u, 240f * u); Ln(dc, b, stick, stick - new Vector2(0, 190f * u), Rgb(0.35f, 0.5f, 0.3f, a), 7f * u); Dot(dc, b, stick - new Vector2(0, 190f * u), 7f * u, Rgb(1, 0.35f, 0.15f, a));
        for (int s = 0; s < Math.Clamp(Count, 1, 8); s++) { var pts = new List<Vector2>(); for (int i = 0; i <= 30; i++) { float k = i / 30f; pts.Add(stick + new Vector2(MathF.Sin(k * 7f - t * 1.5f + s * 1.8f) * (6f + k * 42f) * u + s * 8f * u, -190f * u - k * 300f * u)); } Path(dc, b, pts, C2(0.42f * a), 5f * u); }
        Say(dc, b, Str, 118f * u, o - new Vector2(70f * u, 130f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.75f * a), 6f * u, scale: DrawKit.EaseOutBack(Sub(t, 0.1f, 0.3f)));
    }
}

public sealed class SmirkShadowPlugin : QuickShapePlugin { public override string Name => "演出：ニヤリの暗い影"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SmirkShadowParameter(s); }
public sealed class SmirkShadowParameter : Q3Param
{
    public SmirkShadowParameter() : this(null) { }
    public SmirkShadowParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x000000); Color2 = Hex(0xFF3030); }
    protected override double DefAmount => 75;
    protected override double DefAmount2 => 34;
    static readonly Dictionary<string, ParamGuide> g = G("ニヤリの影", "Amount=影の濃さ", "Amount2=目の間隔", "Color1=影の色", "Color2=目の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), k = DrawKit.EaseOut(Sub(c.Time, 0f, 0.45f)), gap = Amt2(c) * 250f * u; var o = P0(c); float W = 420f * u, H = 200f * u;
        using (var g2 = Gfx.Linear(dc, o - new Vector2(0, H), o + new Vector2(0, 26f * u), (0f, C1(Amt(c) * a * k)), (0.72f, C1(Amt(c) * a * k)), (1f, C1(0f)))) dc.FillEllipse(new Ellipse(o - new Vector2(0, H * 0.45f), W * 0.86f, H * 0.68f), g2);
        float glow = 0.75f + 0.25f * MathF.Sin(c.Time * 7f), open = Sub(c.Time, 0.35f, 0.14f);
        foreach (int s in new[] { -1, 1 }) { var e = o + new Vector2(s * gap, -58f * u); Oval(dc, b, e, 62f * u, 22f * u * open, C2(0.25f * a * glow)); Poly(dc, b, new List<Vector2> { e + new Vector2(-s * 42f * u, -14f * u * open), e + new Vector2(s * 42f * u, 14f * u * open), e + new Vector2(-s * 24f * u, 15f * u * open) }, C2(a * glow)); }
    }
}

public sealed class StareLinePlugin : QuickShapePlugin { public override string Name => "演出：ジーッ（見つめる視線）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new StareLineParameter(s); }
public sealed class StareLineParameter : Q3Param
{
    public StareLineParameter() : this(null) { }
    public StareLineParameter(SharedDataStore? s) : base(s) { Text = "ジーッ"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x222222); }
    protected override double DefAmount => 82;
    protected override double DefAmount2 => 45;
    static readonly Dictionary<string, ParamGuide> g = G("ジーッ", "Text=文字", "Amount=視線の先 X", "Amount2=視線の先 Y", "Color1=線と文字の色", "Color2=ふちの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), p = DrawKit.EaseOut(Sub(c.Time, 0f, 0.6f)); var o = P0(c); var goal = new Vector2(Amt(c) * c.Width, Amt2(c) * c.Height);
        foreach (int s in new[] { -1, 1 })
        {
            var e = o + new Vector2(s * 46f * u, 0); Dot(dc, b, e, 25f * u, C1(a)); Dot(dc, b, e + Vector2.Normalize(goal - e + new Vector2(0.01f, 0)) * 10f * u, 12f * u, C2(a)); var d = goal + new Vector2(s * 12f * u, 0) - e; float len = d.Length() * p; if (len < 1f) continue; d = Vector2.Normalize(d);
            for (float x = 32f * u + Gfx.Mod(c.Time * 46f, 34f) * u; x < len; x += 34f * u) Ln(dc, b, e + d * x, e + d * MathF.Min(x + 18f * u, len), C1(0.92f * a), 5f * u);
        }
        Say(dc, b, Str, 62f * u, Vector2.Lerp(o, goal, 0.5f) - new Vector2(0, 58f * u), C1(a * p), C2(a * p), 5f * u);
    }
}

public sealed class WhisperBubblePlugin : QuickShapePlugin { public override string Name => "演出：ひそひそ話"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WhisperBubbleParameter(s); }
public sealed class WhisperBubbleParameter : Q3Param
{
    public WhisperBubbleParameter() : this(null) { }
    public WhisperBubbleParameter(SharedDataStore? s) : base(s) { Text = "ひそひそ"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFFD7B5); Color3 = Hex(0x455A64); }
    static readonly Dictionary<string, ParamGuide> g = G("ひそひそ話", "Text=文字", "Variant=向き|1=右へ　2=左へ|1|2", "Color1=吹き出しの色", "Color2=手の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), d = Variant == 2 ? -1f : 1f, p = DrawKit.EaseOutBack(Sub(c.Time, 0.05f, 0.3f)); var o = P0(c); var hand = o + new Vector2(d * 12f * u, 0);
        RBox(dc, b, hand, 56f * u, 104f * u, 24f * u, C2(a)); for (int i = 0; i < 3; i++) RBox(dc, b, hand + new Vector2(d * 34f * u, (i - 1) * 30f * u), 54f * u, 24f * u, 11f * u, C2(a));
        var bub = o + new Vector2(d * 245f * u, -105f * u + MathF.Sin(c.Time * 3f) * 5f * u); Oval(dc, b, bub, 170f * u * p, 82f * u * p, C1(0.96f * a)); Dot(dc, b, o + new Vector2(d * 92f * u, -32f * u), 12f * u * p, C1(0.96f * a)); Dot(dc, b, o + new Vector2(d * 62f * u, -14f * u), 7f * u * p, C1(0.96f * a)); Say(dc, b, Str, 42f * u, bub, C3(a), maxW: 290f * u, scale: MathF.Max(0.01f, p));
    }
}

public sealed class CozyTeaPlugin : QuickShapePlugin { public override string Name => "演出：ほっこり（湯気の立つお茶）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CozyTeaParameter(s); }
public sealed class CozyTeaParameter : Q3Param
{
    public CozyTeaParameter() : this(null) { }
    public CozyTeaParameter(SharedDataStore? s) : base(s) { Count = 3; Color1 = Hex(0x6B8E5A); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xFFC56E); }
    static readonly Dictionary<string, ParamGuide> g = G("ほっこり", "Count=湯気の量||1|8", "Color1=湯のみの色", "Color2=湯気の色", "Color3=光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time; var o = P0(c);
        for (int i = 0; i < 12; i++) { float th = i / 12f * MathF.Tau + t * 0.15f, r = (210f + MathF.Sin(t + i) * 34f) * u; Dot(dc, b, Pol(o, r, th), (24f + 14f * Rnd(i)) * u, C3(0.2f * a * (0.6f + 0.4f * MathF.Sin(t * 1.5f + i)))); }
        Poly(dc, b, new List<Vector2> { o + new Vector2(-112f * u, -55f * u), o + new Vector2(112f * u, -55f * u), o + new Vector2(88f * u, 110f * u), o + new Vector2(-88f * u, 110f * u) }, C1(a)); Oval(dc, b, o - new Vector2(0, 55f * u), 112f * u, 24f * u, Rgb(0.35f, 0.5f, 0.28f, a)); Oval(dc, b, o - new Vector2(0, 53f * u), 96f * u, 16f * u, Rgb(0.6f, 0.75f, 0.35f, a));
        for (int s = 0; s < Math.Clamp(Count, 1, 8); s++) { float x = (s - (Math.Clamp(Count, 1, 8) - 1) / 2f) * 44f * u; var pts = new List<Vector2>(); for (int i = 0; i <= 26; i++) { float k = i / 26f; pts.Add(o + new Vector2(x + MathF.Sin(k * 7f - t * 2f + s) * 22f * u, -80f * u - k * 190f * u)); } Path(dc, b, pts, C2(0.55f * a), 7f * u); }
    }
}

public sealed class GoosebumpsPlugin : QuickShapePlugin { public override string Name => "演出：ひえっ（鳥肌）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GoosebumpsParameter(s); }
public sealed class GoosebumpsParameter : Q3Param
{
    public GoosebumpsParameter() : this(null) { }
    public GoosebumpsParameter(SharedDataStore? s) : base(s) { Text = "ひえっ"; Count = 60; Color1 = Hex(0xBFD8FF); Color2 = Hex(0x304FFE); }
    protected override double DefAmount => 50;
    static readonly Dictionary<string, ParamGuide> g = G("鳥肌", "Text=文字", "Count=ぶつぶつの量||10|200", "Amount=震え", "Color1=ぶつぶつの色", "Color2=線と文字ふちの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 10, 200); float u = U(c), t = c.Time, sp = DrawKit.EaseOut(Sub(t, 0f, 0.6f)), sh = Amt(c) * 9f * u; var o = P0(c);
        for (int i = 0; i < n; i++) { float th = Rnd(i) * MathF.Tau, r = (150f + Rnd(i + 90) * 190f) * u * sp; if (Rnd(i + 40) > sp) continue; var q = Pol(o, r, th) + new Vector2(MathF.Sin(t * 53f + i) * sh, MathF.Cos(t * 47f + i) * sh); Dot(dc, b, q, (3f + Rnd(i + 3) * 4f) * u, C1(0.9f * a)); }
        for (int i = 0; i < 10; i++) { float th = i / 10f * MathF.Tau; var pts = new List<Vector2>(); for (int k = 0; k <= 8; k++) { float e = k / 8f, off = (k % 2 == 0 ? 1f : -1f) * 9f * u * (0.4f + Amt(c)); pts.Add(Pol(o, (280f + e * 70f) * u, th) + new Vector2(-MathF.Sin(th), MathF.Cos(th)) * off); } Path(dc, b, pts, C2(0.85f * a), 4f * u); }
        Say(dc, b, Str, 76f * u, o - new Vector2(0, 385f * u) + new Vector2(MathF.Sin(t * 61f) * sh, 0), Rgb(1, 1, 1, a), C2(a), 5f * u);
    }
}

public sealed class BulbFailPlugin : QuickShapePlugin { public override string Name => "演出：電球が消える（ひらめき失敗）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BulbFailParameter(s); }
public sealed class BulbFailParameter : Q3Param
{
    public BulbFailParameter() : this(null) { }
    public BulbFailParameter(SharedDataStore? s) : base(s) { Count = 4; Color1 = Hex(0xFFE45C); Color2 = Hex(0x78909C); Color3 = Hex(0xB0BEC5); }
    static readonly Dictionary<string, ParamGuide> g = G("電球が消える", "Count=点滅の回数||1|12", "Color1=ついた色", "Color2=消えた色", "Color3=煙の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 1, 12); float u = U(c), t = c.Time, off = 0.5f + n * 0.2f; bool lit = t < 0.5f || (t < off && (int)((t - 0.5f) / 0.1f) % 2 == 1); var o = P0(c);
        if (lit) { Dot(dc, b, o, 170f * u, C1(0.2f * a)); for (int i = 0; i < 10; i++) { float th = i / 10f * MathF.Tau; Ln(dc, b, Pol(o, 135f * u, th), Pol(o, 185f * u, th), C1(a), 8f * u); } }
        var col = lit ? C1(a) : C2(a); Dot(dc, b, o, 104f * u, col); Poly(dc, b, new List<Vector2> { o + new Vector2(-62f * u, 80f * u), o + new Vector2(62f * u, 80f * u), o + new Vector2(42f * u, 142f * u), o + new Vector2(-42f * u, 142f * u) }, col); RBox(dc, b, o + new Vector2(0, 166f * u), 96f * u, 54f * u, 12f * u, Rgb(0.45f, 0.48f, 0.52f, a));
        Path(dc, b, new List<Vector2> { o + new Vector2(-34f * u, 84f * u), o + new Vector2(-18f * u, 12f * u), o + new Vector2(0, 42f * u), o + new Vector2(18f * u, 12f * u), o + new Vector2(34f * u, 84f * u) }, lit ? Rgb(1, 0.65f, 0.1f, a) : Rgb(0.2f, 0.22f, 0.25f, a), 5f * u);
        if (t > off) for (int i = 0; i < 9; i++) { float k = Gfx.Mod((t - off) * 0.55f + i / 9f, 1f), ap = Sub(t, off + i * 0.05f, 0.25f); Dot(dc, b, o + new Vector2(MathF.Sin(k * 8f + i) * (10f + k * 40f) * u, -110f * u - k * 240f * u), (12f + k * 32f) * u, C3(0.4f * a * ap * (1f - k))); }
    }
}

public sealed class BlankEyesPlugin : QuickShapePlugin { public override string Name => "演出：白目（ショック）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BlankEyesParameter(s); }
public sealed class BlankEyesParameter : Q3Param
{
    public BlankEyesParameter() : this(null) { }
    public BlankEyesParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x222222); Color3 = Hex(0xDDEBFF); }
    protected override double DefAmount => 35;
    protected override double DefAmount2 => 45;
    static readonly Dictionary<string, ParamGuide> g = G("白目", "Amount=目の間隔", "Amount2=目の大きさ", "Background=魂を出す", "Color1=白目の色", "Color2=ふちの色", "Color3=魂の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, gap = Amt(c) * 260f * u, R = (28f + Amt2(c) * 110f) * u * DrawKit.EaseOutBack(Sub(t, 0f, 0.2f)); var o = P0(c);
        foreach (int s in new[] { -1, 1 }) { Dot(dc, b, o + new Vector2(s * gap, 0), R, C1(a)); Ring(dc, b, o + new Vector2(s * gap, 0), R, C2(a), 6f * u); }
        if (!Background) return; float k = Sub(t, 0.35f, 1.8f); if (k <= 0f) return; var q = o + new Vector2(MathF.Sin(k * 7f) * 30f * u, -(90f + k * 260f) * u); float al = a * (1f - Sub(k, 0.75f, 0.25f)) * 0.82f; Dot(dc, b, q, 58f * u, C3(al)); Poly(dc, b, new List<Vector2> { q + new Vector2(-54f * u, 18f * u), q + new Vector2(54f * u, 18f * u), q + new Vector2(MathF.Sin(t * 5f) * 24f * u, 118f * u) }, C3(al)); Dot(dc, b, q + new Vector2(-18f * u, -8f * u), 6f * u, C2(al)); Dot(dc, b, q + new Vector2(18f * u, -8f * u), 6f * u, C2(al));
    }
}

public sealed class ShiverColdPlugin : QuickShapePlugin { public override string Name => "演出：ぶるぶる寒い"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ShiverColdParameter(s); }
public sealed class ShiverColdParameter : Q3Param
{
    public ShiverColdParameter() : this(null) { }
    public ShiverColdParameter(SharedDataStore? s) : base(s) { Count = 24; Color1 = Hex(0x74C7FF); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 55;
    static readonly Dictionary<string, ParamGuide> g = G("ぶるぶる寒い", "Amount=振動の大きさ", "Count=雪の量||0|100", "Background=鼻水のつらら", "Color1=振動線の色", "Color2=雪の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, amp = Amt(c) * 24f * u; var o = P0(c);
        foreach (int s in new[] { -1, 1 }) for (int l = 0; l < 3; l++) { var pts = new List<Vector2>(); for (int i = 0; i <= 14; i++) { float k = i / 14f; pts.Add(o + new Vector2(s * (250f + l * 46f) * u + (i % 2 == 0 ? 1f : -1f) * amp * MathF.Sin(t * 40f + l), (k - 0.5f) * (340f - l * 60f) * u)); } Path(dc, b, pts, C1((0.95f - l * 0.25f) * a), 6f * u); }
        for (int i = 0; i < Math.Clamp(Count, 0, 100); i++) { float y = Gfx.Mod(Rnd(i) + t * (0.12f + Rnd(i + 5) * 0.14f), 1f) * 700f * u - 350f * u, x = (Rnd(i + 11) - 0.5f) * 800f * u + MathF.Sin(t + i) * 18f * u; Dot(dc, b, o + new Vector2(x, y), (3f + Rnd(i + 20) * 5f) * u, C2(0.85f * a)); }
        if (Background) { float L = (40f + MathF.Sin(t * 1.4f) * 14f) * u; Poly(dc, b, new List<Vector2> { o + new Vector2(-11f * u, 0), o + new Vector2(11f * u, 0), o + new Vector2(0, L * 2f) }, Rgb(0.72f, 0.92f, 1f, 0.9f * a)); }
    }
}

public sealed class MuscleFlexPlugin : QuickShapePlugin { public override string Name => "演出：ムキムキ（力こぶとオーラ）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MuscleFlexParameter(s); }
public sealed class MuscleFlexParameter : Q3Param
{
    public MuscleFlexParameter() : this(null) { }
    public MuscleFlexParameter(SharedDataStore? s) : base(s) { Text = "ムキィッ"; Color1 = Hex(0xFFC9A3); Color2 = Hex(0xFF6A00); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("ムキムキ", "Text=文字", "Color1=腕の色", "Color2=オーラの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, flex = 0.9f + 0.1f * MathF.Abs(MathF.Sin(t * 5f)); var o = P0(c);
        for (int i = 0; i < 26; i++) { float th = i / 26f * MathF.Tau, r = (235f + MathF.Sin(t * 9f + i * 2.2f) * 34f + (i % 2) * 42f) * u; Poly(dc, b, new List<Vector2> { Pol(o, 170f * u, th - 0.13f), Pol(o, r, th), Pol(o, 170f * u, th + 0.13f) }, C2(0.72f * a)); }
        Path(dc, b, new List<Vector2> { o + new Vector2(-140f * u, 95f * u), o + new Vector2(30f * u, 105f * u), o + new Vector2(95f * u, -60f * u) }, C1(a), 88f * u); Dot(dc, b, o + new Vector2(-25f * u, 42f * u), 88f * u * flex, C1(a)); Dot(dc, b, o + new Vector2(102f * u, -105f * u), 55f * u, C1(a));
        Arc(dc, b, o + new Vector2(-25f * u, 42f * u), 58f * u * flex, -2.6f, -0.9f, Rgb(0.75f, 0.42f, 0.28f, a), 6f * u); Say(dc, b, Str, 84f * u, o - new Vector2(0, 310f * u), C3(a), C2(a), 6f * u, scale: DrawKit.EaseOutBack(Sub(t, 0.05f, 0.3f)));
    }
}

public sealed class HungryGuuPlugin : QuickShapePlugin { public override string Name => "演出：腹ペコ（ぐぅ〜）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HungryGuuParameter(s); }
public sealed class HungryGuuParameter : Q3Param
{
    public HungryGuuParameter() : this(null) { }
    public HungryGuuParameter(SharedDataStore? s) : base(s) { Text = "ぐぅ〜"; Color1 = Hex(0xFFB74D); Color2 = Hex(0x5D4037); }
    static readonly Dictionary<string, ParamGuide> g = G("腹ペコ", "Text=文字", "Color1=文字の色", "Color2=渦とふちの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time; var o = P0(c); var pts = new List<Vector2>(); for (int i = 0; i <= 80; i++) { float k = i / 80f; pts.Add(Pol(o, k * 92f * u, k * 16f + t * 3.5f)); } Path(dc, b, pts, C2(0.9f * a), 7f * u);
        string s = Str; float x = o.X + 125f * u; for (int i = 0; i < s.Length; i++) { string ch = s[i].ToString(); var q = new Vector2(x + 44f * u, o.Y - 100f * u + MathF.Sin(t * 5f + i * 0.9f) * 24f * u); Say(dc, b, ch, 84f * u, q, C1(a * Sub(t, i * 0.09f, 0.2f)), C2(a * Sub(t, i * 0.09f, 0.2f)), 5f * u); x += 82f * u; }
    }
}

public sealed class GigglePlugin : QuickShapePlugin { public override string Name => "演出：くすくす笑い"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GiggleParameter(s); }
public sealed class GiggleParameter : Q3Param
{
    public GiggleParameter() : this(null) { }
    public GiggleParameter(SharedDataStore? s) : base(s) { Text = "くす"; Count = 6; Color1 = Hex(0xFF8FB8); Color2 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("くすくす笑い", "Text=文字", "Count=数||1|20", "Color1=文字の色", "Color2=音符の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 1, 20); float u = U(c), t = c.Time; var o = P0(c);
        for (int i = 0; i < n; i++)
        {
            float k = Gfx.Mod(t * 0.42f + Rnd(i), 1f), side = i % 2 == 0 ? 1f : -1f, al = MathF.Min(k * 6f, 1f) * (1f - Sub(k, 0.75f, 0.25f)), hop = MathF.Abs(MathF.Sin(k * MathF.PI * 4f)) * 34f * u; var q = o + new Vector2(side * (110f + Rnd(i + 8) * 170f + k * 60f) * u, 100f * u - k * 230f * u - hop);
            if (i % 3 == 2) { Oval(dc, b, q, 19f * u, 14f * u, C2(a * al)); Ln(dc, b, q + new Vector2(17f * u, 0), q + new Vector2(17f * u, -56f * u), C2(a * al), 5f * u); Ln(dc, b, q + new Vector2(17f * u, -56f * u), q + new Vector2(38f * u, -40f * u), C2(a * al), 5f * u); }
            else Say(dc, b, Str, 60f * u, q, C1(a * al), Rgb(0.25f, 0.05f, 0.12f, 0.8f * a * al), 2.5f * u);
        }
    }
}

public sealed class SadRaincloudPlugin : QuickShapePlugin { public override string Name => "演出：しょんぼりの雨雲"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SadRaincloudParameter(s); }
public sealed class SadRaincloudParameter : Q3Param
{
    public SadRaincloudParameter() : this(null) { }
    public SadRaincloudParameter(SharedDataStore? s) : base(s) { Count = 24; Color1 = Hex(0x607D8B); Color2 = Hex(0x7FC8FF); Color3 = Hex(0xFFE45C); Background = false; }
    protected override double DefAmount => 50;
    static readonly Dictionary<string, ParamGuide> g = G("しょんぼりの雨雲", "Amount=雲の大きさ", "Count=雨の量||1|100", "Background=雷", "Color1=雲の色", "Color2=雨の色", "Color3=雷の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c) * (0.55f + Amt(c) * 0.9f), t = c.Time; var o = P0(c) + new Vector2(MathF.Sin(t * 0.9f) * 12f * u, -120f * u); int n = Math.Clamp(Count, 1, 100);
        for (int i = 0; i < n; i++) { float k = Gfx.Mod(t * (1.1f + Rnd(i) * 0.5f) + Rnd(i + 7), 1f), x = (Rnd(i + 14) - 0.5f) * 280f * u; var q = o + new Vector2(x, 45f * u + k * 270f * u); Ln(dc, b, q, q + new Vector2(-3f * u, 25f * u), C2(a * (1f - k * 0.5f)), 4f * u); }
        if (Background && Gfx.Mod(t, 2.2f) < 0.18f) Poly(dc, b, new List<Vector2> { o + new Vector2(15f * u, 42f * u), o + new Vector2(-34f * u, 142f * u), o + new Vector2(-5f * u, 136f * u), o + new Vector2(-23f * u, 225f * u), o + new Vector2(45f * u, 110f * u), o + new Vector2(12f * u, 116f * u), o + new Vector2(38f * u, 42f * u) }, C3(a));
        foreach (var (x, y, r) in new[] { (-105f, 10f, 62f), (-35f, -32f, 86f), (58f, -18f, 72f), (125f, 15f, 52f) }) Dot(dc, b, o + new Vector2(x, y) * u, r * u, C1(a)); RBox(dc, b, o + new Vector2(5f * u, 38f * u), 340f * u, 62f * u, 30f * u, C1(a));
    }
}

public sealed class AngryStompPlugin : QuickShapePlugin { public override string Name => "演出：イライラの足踏み"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AngryStompParameter(s); }
public sealed class AngryStompParameter : Q3Param
{
    public AngryStompParameter() : this(null) { }
    public AngryStompParameter(SharedDataStore? s) : base(s) { Text = "トントン"; Color1 = Hex(0xFF5252); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("イライラの足踏み", "Text=文字", "Speed=速さ", "Color1=衝撃線の色", "Color2=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), f = Gfx.Mod(c.Time * Spd(c) * 3f, 1f), e = DrawKit.EaseOut(f); var o = P0(c);
        foreach (int s in new[] { -1, 1 }) for (int i = 0; i < 3; i++) { float th = MathF.PI + (s > 0 ? MathF.PI - (0.28f + i * 0.34f) : 0.28f + i * 0.34f); Ln(dc, b, Pol(o, (65f + e * 50f) * u, th), Pol(o, (120f + e * 70f) * u, th), C1(a * (1f - f)), 9f * u); }
        Ln(dc, b, o - new Vector2(210f * u, 0), o + new Vector2(210f * u, 0), Rgb(1, 1, 1, 0.45f * a), 4f * u); Say(dc, b, Str, 62f * u, o - new Vector2(0, 190f * u + (1f - e) * 16f * u), C2(a), C1(a), 5f * u, scale: 1.14f - 0.14f * e);
    }
}

public sealed class BlushSteamPlugin : QuickShapePlugin { public override string Name => "演出：恥ずかしい（ボンッと湯気）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BlushSteamParameter(s); }
public sealed class BlushSteamParameter : Q3Param
{
    public BlushSteamParameter() : this(null) { }
    public BlushSteamParameter(SharedDataStore? s) : base(s) { Count = 9; Color1 = Hex(0xFF2D55); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("恥ずかしい", "Amount=赤さ", "Count=湯気の量||1|30", "Color1=赤の色", "Color2=湯気の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, k = Sub(t, 0f, 0.3f); var o = P0(c); using (var rg = Gfx.Radial(dc, o, 300f * u, (0f, C1(Amt(c) * 0.85f * a * k)), (0.65f, C1(Amt(c) * 0.5f * a * k)), (1f, C1(0f)))) dc.FillEllipse(new Ellipse(o, 300f * u, 220f * u), rg);
        foreach (int s in new[] { -1, 1 }) for (int i = 0; i < 3; i++) { var q = o + new Vector2(s * (130f + i * 30f) * u, 42f * u); Ln(dc, b, q + new Vector2(8f * u, -18f * u), q + new Vector2(-8f * u, 18f * u), C1(a * k), 5f * u); }
        for (int i = 0; i < Math.Clamp(Count, 1, 30); i++) { float at = 0.25f + Rnd(i) * 0.25f, e = Gfx.Mod(MathF.Max(0f, t - at) * 0.65f + Rnd(i + 5), 1f); if (t < at) continue; float side = (Rnd(i + 11) - 0.5f) * 2f; var q = o + new Vector2(side * (60f + e * 210f) * u, -170f * u - e * 240f * u); Dot(dc, b, q, (22f + e * 42f) * u, C2(0.62f * a * (1f - e))); }
    }
}

public sealed class ThinkingDotsPlugin : QuickShapePlugin { public override string Name => "演出：考え中…（くるくる）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ThinkingDotsParameter(s); }
public sealed class ThinkingDotsParameter : Q3Param
{
    public ThinkingDotsParameter() : this(null) { }
    public ThinkingDotsParameter(SharedDataStore? s) : base(s) { Text = "考え中…"; Count = 8; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x37474F); }
    static readonly Dictionary<string, ParamGuide> g = G("考え中", "Text=文字", "Count=点の数||3|20", "Speed=回る速さ", "Color1=点と文字の色", "Color2=ふちの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 3, 20); float u = U(c), t = c.Time * Spd(c) * 4f; var o = P0(c);
        for (int i = 0; i < n; i++) { float th = i * MathF.Tau / n, f = Gfx.Mod(t / MathF.Tau - i / (float)n, 1f); var q = Pol(o, 72f * u, th); Dot(dc, b, q, 14f * u, C2(0.8f * a)); Dot(dc, b, q, (5f + f * 7f) * u, C1(a * (0.25f + 0.75f * f))); }
        Say(dc, b, Str, 48f * u, o + new Vector2(0, 130f * u), C1(a), C2(a), 4f * u);
    }
}

public sealed class EyeBeamPlugin : QuickShapePlugin { public override string Name => "演出：目からビーム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new EyeBeamParameter(s); }
public sealed class EyeBeamParameter : Q3Param
{
    public EyeBeamParameter() : this(null) { }
    public EyeBeamParameter(SharedDataStore? s) : base(s) { Count = 90; Color1 = Hex(0xFF2020); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 92;
    protected override double DefAmount2 => 62;
    static readonly Dictionary<string, ParamGuide> g = G("目からビーム", "Count=目の間隔|画面の1920基準で|10|400", "Amount=光線の先 X", "Amount2=光線の先 Y", "Color1=光線の色", "Color2=中心の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), p = DrawKit.EaseOut(Sub(c.Time, 0.05f, 0.25f)), w = 1f + 0.18f * MathF.Sin(c.Time * 42f), gap = Math.Clamp(Count, 10, 400) * u / 2f; var o = P0(c); var goal = new Vector2(Amt(c) * c.Width, Amt2(c) * c.Height);
        foreach (int s in new[] { -1, 1 })
        {
            var e = o + new Vector2(s * gap, 0); var hit = Vector2.Lerp(e, goal + new Vector2(s * 12f * u, 0), p); Ln(dc, b, e, hit, C1(0.3f * a), 34f * u * w); Ln(dc, b, e, hit, C1(a), 15f * u * w); Ln(dc, b, e, hit, C2(a), 5f * u * w); Dot(dc, b, e, 25f * u * w, C1(a)); Dot(dc, b, e, 12f * u, C2(a));
            if (p >= 1f) { Dot(dc, b, hit, 42f * u * w, C1(0.45f * a)); Dot(dc, b, hit, 18f * u * w, C2(a)); }
        }
    }
}

public sealed class WordsPiercePlugin : QuickShapePlugin { public override string Name => "演出：言葉が刺さる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WordsPierceParameter(s); }
public sealed class WordsPierceParameter : Q3Param
{
    public WordsPierceParameter() : this(null) { }
    public WordsPierceParameter(SharedDataStore? s) : base(s) { Text = "正論\n図星\n締切"; Color1 = Hex(0x222222); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xFF2D2D); }
    static readonly Dictionary<string, ParamGuide> g = G("言葉が刺さる", "Text=文字|1行に1つ", "Variant=飛んでくる向き|1=右から　2=左から　3=上から|1|3", "Color1=矢の色", "Color2=文字の色", "Color3=衝撃の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return; float u = U(c), t = c.Time; var o = P0(c);
        for (int i = 0; i < r.Count && i < 8; i++)
        {
            float th = (Variant == 3 ? -MathF.PI / 2f : Variant == 2 ? MathF.PI : 0f) + (i - (r.Count - 1) / 2f) * MathF.Min(0.62f, 2.4f / r.Count), start = 0.08f + i * 0.34f, k = Sub(t, start, 0.2f); if (k <= 0f) continue;
            var d = new Vector2(MathF.Cos(th), MathF.Sin(th)); float dist = (1f - k * k) * 1050f * u, shake = k >= 1f ? MathF.Sin((t - start) * 60f) * 7f * u * (1f - Sub(t, start + 0.2f, 0.3f)) : 0f; var tip = o + d * (dist + i * 8f * u) + new Vector2(-d.Y, d.X) * shake; var tail = tip + d * 390f * u; Arrow(dc, b, tail, tip, C1(a), 15f * u); Panel(dc, b, tip + d * 225f * u, 230f * u, 82f * u, 8f * u, C2(a), C1(a), 5f * u); Say(dc, b, r[i], 46f * u, tip + d * 225f * u, C1(a), maxW: 210f * u);
            if (k >= 1f) { float e = Sub(t, start + 0.2f, 0.32f); for (int q = 0; q < 8; q++) { float ang = q / 8f * MathF.Tau; Ln(dc, b, Pol(tip, 22f * u + e * 40f * u, ang), Pol(tip, 42f * u + e * 76f * u, ang), C3(a * (1f - e)), 6f * u); } }
        }
    }
}
