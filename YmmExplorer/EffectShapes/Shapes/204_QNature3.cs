using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 491〜510: 自然・天気・生き物

public sealed class HailstormPlugin : QuickShapePlugin { public override string Name => "演出：ひょう・あられ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HailstormParameter(s); }
public sealed class HailstormParameter : Q3Param
{
    public HailstormParameter() : this(null) { }
    public HailstormParameter(SharedDataStore? s) : base(s) { Count = 90; Color1 = Hex(0xF2FAFF); Background = false; }
    protected override double DefAmount => 40;
    protected override double DefAmount2 => 55;
    static readonly Dictionary<string, ParamGuide> g = G("ひょう・あられ", "Count=量||5|500", "Amount=粒の大きさ", "Amount2=風|50で無風", "Speed=速さ", "Background=暗い空を塗る", "Color1=粒の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c), wind = (Amt2(c) - 0.5f) * 0.7f, size = (2.5f + Amt(c) * 9f) * u, ground = h * 0.92f; int n = Math.Clamp(Count, 5, 500);
        if (Background) Sky(dc, w, h, Rgb(0.16f, 0.2f, 0.27f, a), Rgb(0.34f, 0.39f, 0.45f, a));
        for (int i = 0; i < n; i++)
        {
            float life = 0.55f + Rnd(i) * 0.25f, ph = Gfx.Mod(t / life + Rnd(i + 3), 1f), x0 = Rnd(i + 7) * w * 1.3f - w * 0.15f, r = size * (0.6f + Rnd(i + 9) * 0.7f);
            if (ph < 0.72f) { float k = ph / 0.72f; var q = new Vector2(x0 + wind * k * h, k * ground); Ln(dc, b, q, q - new Vector2(wind, 1f) * r * 3.2f, C1(0.25f * a), r * 0.7f); Dot(dc, b, q, r, C1(a)); }
            else { float k = (ph - 0.72f) / 0.28f, side = Rnd(i + 11) > 0.5f ? 1f : -1f; var q = new Vector2(x0 + wind * ground + side * k * 42f * u, ground - MathF.Sin(k * MathF.PI) * (28f + Rnd(i + 13) * 34f) * u); Dot(dc, b, q, r * 0.85f, C1(a * (1f - k * 0.7f))); }
        }
    }
}

public sealed class DustHazePlugin : QuickShapePlugin { public override string Name => "演出：黄砂・花粉でかすむ空"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DustHazeParameter(s); }
public sealed class DustHazeParameter : Q3Param
{
    public DustHazeParameter() : this(null) { }
    public DustHazeParameter(SharedDataStore? s) : base(s) { Count = 180; Color1 = Hex(0xD9B65A); Color2 = Hex(0xFFE9A8); }
    protected override double DefAmount => 55;
    static readonly Dictionary<string, ParamGuide> g = G("黄砂・花粉", "Amount=濃さ", "Count=粒の数||0|800", "Speed=流れる速さ", "Color1=もやの色", "Color2=粒の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c), den = Amt(c); Sky(dc, w, h, C1(0.25f * den * a), C1(0.68f * den * a));
        for (int i = 0; i < 7; i++) { float x = Gfx.Mod(Rnd(i) * w * 1.6f + t * (14f + i * 5f) * u, w * 1.6f) - w * 0.3f, y = h * (0.2f + Rnd(i + 8) * 0.7f), rx = (360f + Rnd(i + 2) * 320f) * u; using var br = Gfx.Radial(dc, new(x, y), rx, (0f, C2(0.26f * den * a)), (1f, C2(0f))); dc.FillEllipse(new Ellipse(new(x, y), rx, rx * 0.42f), br); }
        for (int i = 0; i < Math.Clamp(Count, 0, 800); i++) { float x = Gfx.Mod(Rnd(i) * w + t * (22f + Rnd(i + 5) * 50f) * u, w), y = Gfx.Mod(Rnd(i + 9) * h + MathF.Sin(t * 0.7f + i) * 22f * u, h); Dot(dc, b, new(x, y), (1f + Rnd(i + 3) * 2.2f) * u, C2((0.3f + 0.5f * Rnd(i + 4)) * a)); }
    }
}

public sealed class TyphoonSatellitePlugin : QuickShapePlugin { public override string Name => "演出：台風（衛星写真風）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TyphoonSatelliteParameter(s); }
public sealed class TyphoonSatelliteParameter : Q3Param
{
    public TyphoonSatelliteParameter() : this(null) { }
    public TyphoonSatelliteParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xF4F7FA); Color2 = Hex(0x15395E); }
    protected override double DefAmount => 60;
    protected override double DefAmount2 => 0;
    static readonly Dictionary<string, ParamGuide> g = G("台風", "Amount=大きさ", "Amount2=進む量", "Variant=進む向き|1=北東　2=北西　3=北　4=西|1|4", "Speed=回る速さ", "Background=海を塗る", "Color1=雲の色", "Color2=海の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), R = (220f + Amt(c) * 340f) * u; if (Background) Box(dc, b, 0, 0, c.Width, c.Height, C2(a));
        var dir = Variant switch { 2 => new Vector2(-0.7f, -0.7f), 3 => new Vector2(0, -1), 4 => new Vector2(-1, 0), _ => new Vector2(0.7f, -0.7f) }; var o = P0(c) + dir * Amt2(c) * c.Height * 0.55f;
        using (var gl = Gfx.Radial(dc, o, R * 1.08f, (0f, C1(0.75f * a)), (0.6f, C1(0.42f * a)), (1f, C1(0f)))) dc.FillEllipse(new Ellipse(o, R * 1.08f, R * 1.08f), gl);
        for (int arm = 0; arm < 5; arm++) for (int i = 0; i < 52; i++)
        {
            float k = i / 52f, th = arm * MathF.Tau / 5f - t * 0.7f + k * 5.6f, r = R * (0.13f + k * 0.92f), sz = R * (0.14f - k * 0.08f) * (0.75f + 0.25f * MathF.Sin(i * 1.7f + arm)); Oval(dc, b, Pol(o, r, th), sz * 1.25f, sz, C1((0.42f - k * 0.26f) * a));
        }
        float eye = R * 0.105f; using (var ge = Gfx.Radial(dc, o, eye * 2f, (0f, C2(a)), (0.48f, C2(a)), (1f, C2(0f)))) dc.FillEllipse(new Ellipse(o, eye * 2f, eye * 2f), ge);
    }
}

public sealed class MorningDewPlugin : QuickShapePlugin { public override string Name => "演出：朝露のしずく"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MorningDewParameter(s); }
public sealed class MorningDewParameter : Q3Param
{
    public MorningDewParameter() : this(null) { }
    public MorningDewParameter(SharedDataStore? s) : base(s) { Count = 9; Color1 = Hex(0x4C9A45); Color2 = Hex(0xD8F4FF); }
    static readonly Dictionary<string, ParamGuide> g = G("朝露のしずく", "Count=しずくの数||1|40", "Speed=流れる速さ", "Color1=葉の色", "Color2=しずくの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c); var o = P0(c); var leaf = new List<Vector2>(); var A = o + new Vector2(-720f * u, 190f * u); var B = o + new Vector2(720f * u, -190f * u); var d = B - A; var n = Vector2.Normalize(new Vector2(-d.Y, d.X));
        for (int i = 0; i <= 50; i++) { float k = i / 50f; leaf.Add(Vector2.Lerp(A, B, k) + n * MathF.Sin(k * MathF.PI) * 300f * u); } for (int i = 50; i >= 0; i--) { float k = i / 50f; leaf.Add(Vector2.Lerp(A, B, k) - n * MathF.Sin(k * MathF.PI) * 300f * u); }
        Poly(dc, b, leaf, C1(a)); Ln(dc, b, A, B, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.4f), a), 7f * u);
        for (int i = 1; i < 9; i++) { var m = Vector2.Lerp(A, B, i / 9f); float l = MathF.Sin(i / 9f * MathF.PI) * 250f * u; var fw = Vector2.Normalize(d) * 150f * u; Ln(dc, b, m, m + n * l + fw, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.25f), 0.7f * a), 3f * u); Ln(dc, b, m, m - n * l + fw, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.25f), 0.7f * a), 3f * u); }
        for (int i = 0; i < Math.Clamp(Count, 1, 40); i++)
        {
            float k = 0.12f + Rnd(i) * 0.76f, side = (Rnd(i + 4) - 0.5f) * 1.3f, slide = Gfx.Mod(t * 0.035f * (0.5f + Rnd(i + 8)), 0.16f), r = (15f + Rnd(i + 2) * 22f) * u; var q = Vector2.Lerp(A, B, MathF.Max(0.03f, k - slide)) + n * side * MathF.Sin(k * MathF.PI) * 300f * u;
            Oval(dc, b, q + new Vector2(3f, 5f) * u, r, r * 0.92f, Rgb(0.05f, 0.2f, 0.08f, 0.35f * a)); Dot(dc, b, q, r, C2(0.55f * a)); Ring(dc, b, q, r, Rgb(1, 1, 1, 0.45f * a), 2f * u); float tw = 0.65f + 0.35f * MathF.Sin(t * 2.4f + i * 2f); Dot(dc, b, q - new Vector2(r * 0.34f, r * 0.36f), r * 0.26f, Rgb(1, 1, 1, tw * a));
        }
    }
}

public sealed class MossForestFloorPlugin : QuickShapePlugin { public override string Name => "演出：苔と森の地面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MossForestFloorParameter(s); }
public sealed class MossForestFloorParameter : Q3Param
{
    public MossForestFloorParameter() : this(null) { }
    public MossForestFloorParameter(SharedDataStore? s) : base(s) { Count = 40; Color1 = Hex(0x3D7A36); Color2 = Hex(0x5A6158); Color3 = Hex(0xD8FF8A); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("苔と森の地面", "Amount=緑の濃さ", "Count=光の量||0|200", "Seed=シード", "Color1=苔の色", "Color2=岩の色", "Color3=光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, den = Amt(c); Sky(dc, w, h, Rgb(0.04f, 0.1f, 0.07f, a), Rgb(0.08f, 0.17f, 0.09f, a)); Box(dc, b, 0, h * 0.62f, w, h, Col(Gfx.Mix(Hex(0x24301F), Color1, den), a));
        for (int i = 0; i < 7; i++) { float x = Rnd(i + Seed) * w, y = h * (0.66f + Rnd(i + 20 + Seed) * 0.24f), rx = (120f + Rnd(i + 40 + Seed) * 170f) * u, ry = rx * 0.58f; Oval(dc, b, new(x, y), rx, ry, C2(a)); Oval(dc, b, new(x, y - ry * 0.25f), rx * 0.96f, ry * 0.76f, Col(Gfx.Mix(Color2, Color1, 0.45f + den * 0.5f), a)); }
        for (int i = 0; i < 520; i++) { float x = Rnd(i * 3 + Seed) * w, y = h * (0.6f + Rnd(i * 5 + Seed) * 0.4f); Dot(dc, b, new(x, y), (2f + Rnd(i + 7) * 7f) * u, Col(Gfx.Mix(Color1, Hex(0xB8E86A), Rnd(i + 11) * 0.55f), (0.3f + den * 0.5f) * a)); }
        for (int i = 0; i < Math.Clamp(Count, 0, 200); i++) { float x = Rnd(i + 80) * w + MathF.Sin(t * 0.5f + i) * 26f * u, y = h * (0.28f + Rnd(i + 90) * 0.68f) + MathF.Cos(t * 0.6f + i) * 18f * u, gl = 0.35f + 0.65f * MathF.Pow(0.5f + 0.5f * MathF.Sin(t * 2f + i * 1.7f), 2f); Dot(dc, b, new(x, y), 11f * u * gl, C3(0.17f * a * gl)); Dot(dc, b, new(x, y), 3f * u, C3(a * gl)); }
    }
}

public sealed class MushroomForestPlugin : QuickShapePlugin { public override string Name => "演出：キノコの森"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MushroomForestParameter(s); }
public sealed class MushroomForestParameter : Q3Param
{
    public MushroomForestParameter() : this(null) { }
    public MushroomForestParameter(SharedDataStore? s) : base(s) { Count = 12; Color1 = Hex(0xFF5C8A); Color2 = Hex(0x57F2E2); Color3 = Hex(0x0A1630); }
    static readonly Dictionary<string, ParamGuide> g = G("キノコの森", "Count=数||1|40", "Background=光らせる", "Seed=シード", "Color1=かさの色", "Color2=光の色", "Color3=背景の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time; int n = Math.Clamp(Count, 1, 40); Sky(dc, w, h, C3(a), Col(Gfx.Mix(Color3, Hex(0x1B5A55), 0.65f), a)); Box(dc, b, 0, h * 0.82f, w, h, Rgb(0.04f, 0.12f, 0.1f, a));
        for (int i = 0; i < n; i++)
        {
            int id = i + Seed * 31; float depth = (i + 1f) / n, x = Rnd(id) * w, S = (45f + depth * 145f + Rnd(id + 4) * 40f) * u, baseY = h * (0.76f + depth * 0.2f), sway = MathF.Sin(t + i) * 5f * u, pulse = 0.55f + 0.45f * MathF.Sin(t * 1.5f + i * 1.3f); var top = new Vector2(x + sway, baseY - S * 1.3f);
            if (Background) Dot(dc, b, top, S * 1.5f, C2(0.11f * pulse * a)); Poly(dc, b, new List<Vector2> { new(x - S * 0.18f, baseY), new(x + S * 0.18f, baseY), top + new Vector2(S * 0.13f, 0), top - new Vector2(S * 0.13f, 0) }, Rgb(0.92f, 0.87f, 0.75f, a)); Pie(dc, b, top + new Vector2(0, S * 0.08f), S * 0.86f, MathF.PI, MathF.Tau, Background ? Col(Gfx.Mix(Color1, Color2, pulse * 0.28f), a) : C1(a)); Oval(dc, b, top + new Vector2(0, S * 0.08f), S * 0.86f, S * 0.14f, Rgb(0.95f, 0.85f, 0.75f, a));
            for (int k = 0; k < 4; k++) Dot(dc, b, top + new Vector2((k - 1.5f) * S * 0.36f, -S * (0.25f + 0.2f * (k % 2))), S * 0.075f, Rgb(1, 1, 1, 0.9f * a));
        }
    }
}

public sealed class JellyfishDriftPlugin : QuickShapePlugin { public override string Name => "演出：クラゲがただよう"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new JellyfishDriftParameter(s); }
public sealed class JellyfishDriftParameter : Q3Param
{
    public JellyfishDriftParameter() : this(null) { }
    public JellyfishDriftParameter(SharedDataStore? s) : base(s) { Count = 6; Color1 = Hex(0xB48CFF); Color2 = Hex(0x5EE7FF); Color3 = Hex(0x061633); }
    static readonly Dictionary<string, ParamGuide> g = G("クラゲ", "Count=数||1|30", "Amount=光り方", "Speed=速さ", "Background=海を塗る", "Color1=色1", "Color2=色2", "Color3=海の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), t = c.Time * Spd(c), glow = Amt(c); if (Background) Sky(dc, w, h, Col(Gfx.Mix(Color3, Hex(0x155A8A), 0.65f), a), C3(a));
        for (int i = 0; i < Math.Clamp(Count, 1, 30); i++)
        {
            float pulse = MathF.Sin(t * 2.2f + i * 1.7f), S = (70f + Rnd(i) * 90f) * u, x = Rnd(i + 3) * w + MathF.Sin(t * 0.3f + i) * 60f * u, y = Gfx.Mod(Rnd(i + 5) * h - t * (18f + Rnd(i + 8) * 18f) * u, h + S * 4f) - S * 1.5f; var q = new Vector2(x, y); var col = Gfx.Mix(Color1, Color2, Rnd(i + 2));
            Dot(dc, b, q, S * 1.7f, Col(col, 0.1f * glow * a)); float rx = S * (1f + pulse * 0.12f), ry = S * (0.78f - pulse * 0.1f); var bell = new List<Vector2>(); for (int k = 0; k <= 28; k++) { float e = MathF.PI + k / 28f * MathF.PI; bell.Add(q + new Vector2(MathF.Cos(e) * rx, MathF.Sin(e) * ry)); } Poly(dc, b, bell, Col(col, 0.58f * a)); Oval(dc, b, q, rx, S * 0.15f, Col(Gfx.Mix(col, Hex(0xFFFFFF), 0.5f), 0.65f * a));
            for (int l = 0; l < 7; l++) { float lx = (l - 3f) / 3f * rx * 0.86f; var pts = new List<Vector2>(); for (int k = 0; k <= 14; k++) { float e = k / 14f; pts.Add(q + new Vector2(lx + MathF.Sin(e * 6f + t * 2f + l) * (5f + e * 18f) * u, e * S * 2f)); } Path(dc, b, pts, Col(col, 0.5f * a), 3f * u); }
        }
    }
}

public sealed class GoldfishBowlPlugin : QuickShapePlugin { public override string Name => "演出：金魚鉢の金魚"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GoldfishBowlParameter(s); }
public sealed class GoldfishBowlParameter : Q3Param
{
    public GoldfishBowlParameter() : this(null) { }
    public GoldfishBowlParameter(SharedDataStore? s) : base(s) { Count = 3; Color1 = Hex(0xFF5A2B); Color2 = Hex(0x7FD7F7); Color3 = Hex(0x3E9B50); }
    static readonly Dictionary<string, ParamGuide> g = G("金魚鉢", "Count=金魚の数||1|8", "Background=水草", "Speed=泳ぐ速さ", "Color1=金魚の色", "Color2=水の色", "Color3=水草の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), R = 400f * u; var o = P0(c); var water = Gfx.ArcPts(o, R, MathF.PI * -0.12f, MathF.PI * 1.12f, 80); Poly(dc, b, water, C2(0.5f * a)); Oval(dc, b, o + new Vector2(0, R * 0.9f), R * 0.56f, R * 0.13f, Rgb(0.85f, 0.75f, 0.55f, a));
        if (Background) for (int s = 0; s < 4; s++) { var pts = new List<Vector2>(); float x = (s - 1.5f) * 120f * u; for (int i = 0; i <= 18; i++) { float k = i / 18f; pts.Add(o + new Vector2(x + MathF.Sin(k * 5f + t + s) * (5f + k * 24f) * u, R * 0.88f - k * (210f + s * 28f) * u)); } Path(dc, b, pts, C3(a), 13f * u); }
        for (int i = 0; i < Math.Clamp(Count, 1, 8); i++)
        {
            float sp = 0.42f + Rnd(i) * 0.3f, ph = t * sp + i * 2.3f, ax = R * (0.5f + Rnd(i + 2) * 0.22f), ay = R * (0.2f + Rnd(i + 5) * 0.2f), oy = R * (-0.05f + Rnd(i + 8) * 0.42f); var q = o + new Vector2(MathF.Sin(ph) * ax, oy + MathF.Sin(ph * 1.7f) * ay); float dir = MathF.Cos(ph) >= 0f ? 1f : -1f, S = (50f + Rnd(i + 3) * 18f) * u, tail = MathF.Sin(t * 9f + i) * 24f * u;
            Poly(dc, b, new List<Vector2> { q - new Vector2(dir * S * 0.72f, 0), q - new Vector2(dir * S * 1.75f, S * 0.68f + tail), q - new Vector2(dir * S * 1.38f, tail * 0.4f), q - new Vector2(dir * S * 1.75f, -S * 0.68f + tail) }, C1(0.9f * a)); Oval(dc, b, q, S, S * 0.58f, C1(a)); Dot(dc, b, q + new Vector2(dir * S * 0.55f, -S * 0.12f), S * 0.13f, Rgb(1, 1, 1, a)); Dot(dc, b, q + new Vector2(dir * S * 0.58f, -S * 0.12f), S * 0.07f, Rgb(0, 0, 0, a));
        }
        for (int i = 0; i < 10; i++) { float k = Gfx.Mod(t * 0.28f + Rnd(i), 1f); Ring(dc, b, o + new Vector2((Rnd(i + 3) - 0.5f) * R + MathF.Sin(k * 9f + i) * 10f * u, R * 0.75f - k * R * 1.45f), (4f + Rnd(i + 7) * 8f) * u, Rgb(1, 1, 1, 0.65f * a * (1f - k)), 2f * u); }
        var glass = Gfx.ArcPts(o, R, MathF.PI * -0.28f, MathF.PI * 1.28f, 90); Path(dc, b, glass, Rgb(1, 1, 1, 0.75f * a), 9f * u); Arc(dc, b, o, R * 0.86f, MathF.PI * 0.72f, MathF.PI * 1.05f, Rgb(1, 1, 1, 0.42f * a), 16f * u); b.Color = Rgb(1, 1, 1, 0.75f * a); dc.DrawEllipse(new Ellipse(o - new Vector2(0, R * 0.77f), R * 0.637f, R * 0.07f), b, 6f * u);
    }
}

public sealed class WhaleSilhouettePlugin : QuickShapePlugin { public override string Name => "演出：クジラのシルエット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WhaleSilhouetteParameter(s); }
public sealed class WhaleSilhouetteParameter : Q3Param
{
    public WhaleSilhouetteParameter() : this(null) { }
    public WhaleSilhouetteParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x0A2940); Color2 = Hex(0x1E7FB5); Color3 = Hex(0xF4A261); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("クジラ", "Variant=向きと場所|1=海・右へ　2=海・左へ　3=空・右へ　4=空・左へ|1|4", "Amount=速さ", "Background=背景を塗る", "Color1=クジラの色", "Color2=海の色", "Color3=空の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), t = c.Time, dir = Variant is 2 or 4 ? -1f : 1f, S = 520f * u; bool sky = Variant >= 3;
        if (Background) { if (sky) Sky(dc, w, h, Rgb(0.24f, 0.28f, 0.52f, a), C3(a)); else Sky(dc, w, h, C2(a), Rgb(0.02f, 0.1f, 0.2f, a)); }
        float span = w + S * 3.6f, x = Gfx.Mod(t * (40f + Amt(c) * 260f) * u + S * 1.2f, span) - S * 1.8f; if (dir < 0f) x = w - x; var q = new Vector2(x, P0(c).Y + MathF.Sin(t * 0.7f) * 26f * u); float tail = MathF.Sin(t * 1.6f) * 0.2f; Vector2 P(float px, float py) => q + new Vector2(dir * px * S, py * S);
        var body = new List<Vector2>(); float[] xs = [1f, 0.88f, 0.62f, 0.25f, -0.15f, -0.5f, -0.82f, -1.08f], up = [0f, -0.17f, -0.27f, -0.31f, -0.27f, -0.18f, -0.08f, tail * 0.25f - 0.015f], dn = [0.04f, 0.2f, 0.32f, 0.33f, 0.26f, 0.16f, 0.07f, tail * 0.25f + 0.02f];
        for (int i = 0; i < xs.Length; i++) body.Add(P(xs[i], up[i])); for (int i = xs.Length - 1; i >= 0; i--) body.Add(P(xs[i], dn[i])); Poly(dc, b, body, C1(a));
        var root = P(-1.05f, tail * 0.25f); Poly(dc, b, new List<Vector2> { root, P(-1.43f, tail - 0.26f), P(-1.27f, tail), P(-1.43f, tail + 0.26f) }, C1(a)); Poly(dc, b, new List<Vector2> { P(0.28f, 0.27f), P(-0.03f, 0.64f + tail * 0.5f), P(0.03f, 0.28f) }, C1(a)); for (int i = 0; i < 7; i++) Ln(dc, b, P(0.75f - i * 0.075f, 0.13f), P(0.67f - i * 0.075f, 0.28f), Rgb(1, 1, 1, 0.18f * a), 3f * u); Dot(dc, b, P(0.68f, -0.03f), 7f * u, Rgb(1, 1, 1, 0.75f * a));
    }
}

public sealed class OwlNightForestPlugin : QuickShapePlugin { public override string Name => "演出：フクロウの目が光る夜の森"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new OwlNightForestParameter(s); }
public sealed class OwlNightForestParameter : Q3Param
{
    public OwlNightForestParameter() : this(null) { }
    public OwlNightForestParameter(SharedDataStore? s) : base(s) { Count = 3; Color1 = Hex(0x0B1730); Color2 = Hex(0x050A12); Color3 = Hex(0xFFD93B); }
    protected override double DefAmount => 35;
    static readonly Dictionary<string, ParamGuide> g = G("夜の森のフクロウ", "Count=数||1|8", "Amount=まばたきの間隔|0〜100 = 1〜9秒", "Color1=空の色", "Color2=木と鳥の色", "Color3=目の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, iv = 1f + Amt(c) * 8f; int n = Math.Clamp(Count, 1, 8); Sky(dc, w, h, C1(a), Col(Gfx.Mix(Color1, Hex(0x315C72), 0.6f), a)); Dot(dc, b, new(w * 0.78f, h * 0.2f), 78f * u, Rgb(0.96f, 0.94f, 0.8f, 0.9f * a));
        for (int i = 0; i < 9; i++) { float x = i / 8f * w, tw = (85f + Rnd(i) * 55f) * u; Poly(dc, b, new List<Vector2> { new(x - tw, h), new(x - tw * 0.32f, 0), new(x + tw * 0.32f, 0), new(x + tw, h) }, C2((i % 2 == 0 ? 0.65f : 1f) * a)); }
        for (int i = 0; i < n; i++)
        {
            float x = w * (i + 0.5f) / n, y = h * (0.4f + 0.2f * Rnd(i + 5)), S = (60f + Rnd(i) * 22f) * u; Ln(dc, b, new(x - S * 2.8f, y + S * 1.06f), new(x + S * 2.8f, y + S * 0.92f), C2(a), 18f * u); Oval(dc, b, new(x, y), S, S * 1.18f, C2(a)); Poly(dc, b, new List<Vector2> { new(x - S * 0.8f, y - S * 0.7f), new(x - S * 0.82f, y - S * 1.36f), new(x - S * 0.22f, y - S * 1.02f) }, C2(a)); Poly(dc, b, new List<Vector2> { new(x + S * 0.8f, y - S * 0.7f), new(x + S * 0.82f, y - S * 1.36f), new(x + S * 0.22f, y - S * 1.02f) }, C2(a));
            float ph = Gfx.Mod(t + Rnd(i + 9) * iv, iv), open = ph < 0.18f ? MathF.Abs(ph / 0.09f - 1f) : 1f; foreach (int s in new[] { -1, 1 }) { var e = new Vector2(x + s * S * 0.38f, y - S * 0.36f); Oval(dc, b, e, S * 0.34f, S * 0.34f * open, C3(a)); Oval(dc, b, e, S * 0.13f, S * 0.15f * open, Rgb(0, 0, 0, a)); }
        }
    }
}

public sealed class AntTrailPlugin : QuickShapePlugin { public override string Name => "演出：蟻の行列"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AntTrailParameter(s); }
public sealed class AntTrailParameter : Q3Param
{
    public AntTrailParameter() : this(null) { }
    public AntTrailParameter(SharedDataStore? s) : base(s) { Count = 32; Color1 = Hex(0x1D1410); Color2 = Hex(0xC8A877); }
    protected override double DefAmount => 45;
    static readonly Dictionary<string, ParamGuide> g = G("蟻の行列", "Count=数||1|150", "Amount=道の曲がり方", "Speed=進む速さ", "Background=地面を塗る", "Color1=蟻の色", "Color2=地面の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), t = c.Time * Spd(c), bend = Amt(c) * h * 0.32f, cy = P0(c).Y; int n = Math.Clamp(Count, 1, 150); if (Background) { Box(dc, b, 0, 0, w, h, C2(a)); for (int i = 0; i < 180; i++) Dot(dc, b, new(Rnd(i) * w, Rnd(i + 200) * h), (1f + Rnd(i + 400) * 4f) * u, Rgb(0.42f, 0.32f, 0.22f, 0.3f * a)); }
        Vector2 At(float k) { float x = -0.08f + k * 1.16f; return new(x * w, cy + MathF.Sin(k * 9f) * bend + MathF.Sin(k * 3.7f + 1f) * bend * 0.45f); }
        for (int i = 0; i < n; i++)
        {
            float k = Gfx.Mod(i / (float)n + t * 0.035f, 1f); var q = At(k); var f = At(k + 0.004f) - q; float ang = MathF.Atan2(f.Y, f.X); var d = new Vector2(MathF.Cos(ang), MathF.Sin(ang)); var nn = new Vector2(-d.Y, d.X); float S = 8f * u, step = MathF.Sin(t * 22f + i) * 3f * u;
            for (int l = -1; l <= 1; l++) foreach (int s in new[] { -1, 1 }) Ln(dc, b, q + d * l * S * 0.7f, q + d * (l * S * 1.1f + s * l * step) + nn * s * S * 1.65f, C1(a), 1.7f * u); Oval(dc, b, q - d * S * 1.1f, S * 0.95f, S * 0.95f, C1(a)); Dot(dc, b, q, S * 0.55f, C1(a)); Dot(dc, b, q + d * S, S * 0.72f, C1(a)); foreach (int s in new[] { -1, 1 }) Ln(dc, b, q + d * S * 1.5f, q + d * S * 2.5f + nn * s * S, C1(a), 1.4f * u);
        }
    }
}

public sealed class DewySpiderWebPlugin : QuickShapePlugin { public override string Name => "演出：クモの巣（朝露つき）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DewySpiderWebParameter(s); }
public sealed class DewySpiderWebParameter : Q3Param
{
    public DewySpiderWebParameter() : this(null) { }
    public DewySpiderWebParameter(SharedDataStore? s) : base(s) { Count = 40; Color1 = Hex(0xEAF2F8); Color2 = Hex(0xBDEBFF); }
    protected override double DefAmount => 45;
    static readonly Dictionary<string, ParamGuide> g = G("クモの巣", "Amount=すき間", "Count=露の量||0|200", "Color1=糸の色", "Color2=露の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        const int spokes = 14; float u = U(c), R = 470f * u, p = DrawKit.EaseOut(Intro(c, 0f, 1.2f)), gap = (28f + Amt(c) * 58f) * u; var o = P0(c); int rings = (int)(R / gap);
        for (int i = 0; i < spokes; i++) { float th = i * MathF.Tau / spokes + 0.11f; Ln(dc, b, o, Pol(o, R * (0.92f + 0.08f * Rnd(i)) * p, th), C1(0.85f * a), 2.5f * u); }
        for (int r = 1; r <= rings; r++) { if (r * gap > R * p) break; var ring = new List<Vector2>(); for (int i = 0; i <= spokes; i++) { float a0 = i * MathF.Tau / spokes + 0.11f, a1 = (i + 1) * MathF.Tau / spokes + 0.11f, rr = r * gap; ring.Add(Pol(o, rr, a0)); if (i < spokes) ring.Add(Pol(o, rr * 0.92f, (a0 + a1) / 2f)); } Path(dc, b, ring, C1(0.72f * a), 1.8f * u); }
        for (int i = 0; i < Math.Clamp(Count, 0, 200); i++) { int sp = (int)(Rnd(i) * spokes), rg = 1 + (int)(Rnd(i + 3) * rings); if (rg * gap > R * p) continue; var q = Pol(o, rg * gap, sp * MathF.Tau / spokes + 0.11f); float rr = (3f + Rnd(i + 8) * 6f) * u, tw = 0.55f + 0.45f * MathF.Sin(c.Time * 2.4f + i); Dot(dc, b, q, rr, C2(0.82f * a)); Dot(dc, b, q - new Vector2(rr, rr) * 0.3f, rr * 0.34f, Rgb(1, 1, 1, tw * a)); }
    }
}

public sealed class CoralReefPlugin : QuickShapePlugin { public override string Name => "演出：サンゴ礁"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CoralReefParameter(s); }
public sealed class CoralReefParameter : Q3Param
{
    public CoralReefParameter() : this(null) { }
    public CoralReefParameter(SharedDataStore? s) : base(s) { Count = 14; Color1 = Hex(0x18A9C9); Color2 = Hex(0x07395F); Color3 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("サンゴ礁", "Count=魚の数||0|60", "Speed=速さ", "Seed=シード", "Color1=水の色(上)", "Color2=水の色(下)", "Color3=魚の色");
    void Branch(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 p, float ang, float len, int depth, int seed, Color4 col, float u, float t)
    {
        var q = Pol(p, len, ang + MathF.Sin(t + seed) * 0.018f * (5 - depth)); Ln(dc, b, p, q, col, MathF.Max(2f, depth * 4.2f) * u); if (depth <= 0) { Dot(dc, b, q, 5f * u, col); return; }
        Branch(dc, b, q, ang - 0.38f - Rnd(seed) * 0.28f, len * 0.74f, depth - 1, seed * 3 + 1, col, u, t); Branch(dc, b, q, ang + 0.34f + Rnd(seed + 5) * 0.3f, len * 0.7f, depth - 1, seed * 3 + 2, col, u, t);
    }
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c); Sky(dc, w, h, C1(a), C2(a)); for (int i = 0; i < 7; i++) { float x = Gfx.Mod(i / 7f * w + MathF.Sin(t * 0.25f + i) * 60f * u, w), sway = 95f * u; Poly(dc, b, new List<Vector2> { new(x - 34f * u, 0), new(x + 34f * u, 0), new(x + sway + 130f * u, h * 0.75f), new(x + sway - 90f * u, h * 0.75f) }, Rgb(1, 1, 1, 0.055f * a)); }
        Oval(dc, b, new(w / 2f, h * 1.02f), w * 0.72f, h * 0.2f, Rgb(0.85f, 0.75f, 0.55f, a));
        for (int i = 0; i < 11; i++) { var col = Col(Gfx.Hsv(345f + Rnd(i + Seed) * 75f + (i % 3) * 55f, 0.64f, 1f), a); Branch(dc, b, new(Rnd(i + 8 + Seed) * w, h * (0.87f + Rnd(i + 3) * 0.08f)), -MathF.PI / 2f + (Rnd(i + 2) - 0.5f) * 0.4f, (58f + Rnd(i + 5) * 36f) * u, 4, i + 3 + Seed, col, u, t); }
        for (int i = 0; i < Math.Clamp(Count, 0, 60); i++) { float dir = i % 2 == 0 ? 1f : -1f, span = w + 200f * u, x = Gfx.Mod(Rnd(i + 50) * span + dir * t * (32f + Rnd(i + 1) * 60f) * u, span) - 100f * u, y = h * (0.12f + Rnd(i + 70) * 0.56f) + MathF.Sin(t + i) * 16f * u, S = (15f + Rnd(i + 4) * 14f) * u; var col = i % 3 == 0 ? Rgb(1, 0.45f, 0.25f, a) : i % 3 == 1 ? C3(a) : Rgb(0.45f, 0.95f, 0.85f, a); Oval(dc, b, new(x, y), S, S * 0.52f, col); Poly(dc, b, new List<Vector2> { new(x - dir * S * 0.8f, y), new(x - dir * S * 1.6f, y - S * 0.55f), new(x - dir * S * 1.6f, y + S * 0.55f) }, col); Dot(dc, b, new(x + dir * S * 0.55f, y - S * 0.1f), S * 0.1f, Rgb(0, 0, 0, a)); }
    }
}

public sealed class AvalanchePlugin : QuickShapePlugin { public override string Name => "演出：雪崩"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AvalancheParameter(s); }
public sealed class AvalancheParameter : Q3Param
{
    public AvalancheParameter() : this(null) { }
    public AvalancheParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x4C6680); Color3 = Hex(0x9CCBEA); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("雪崩", "Amount=勢い", "Variant=向き|1=右へ　2=左へ|1|2", "Background=空と山を塗る", "Color1=雪の色", "Color2=山の色", "Color3=空の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, pow = 0.45f + Amt(c) * 1.2f, prog = MathF.Min(1.2f, t * 0.28f * pow); bool left = Variant == 2; Vector2 M(float x, float y) => new(left ? w - x * w : x * w, y * h);
        if (Background) { Sky(dc, w, h, Rgb(0.28f, 0.5f, 0.75f, a), C3(a)); Poly(dc, b, new List<Vector2> { M(-0.05f, 0.2f), M(0.12f, 0.06f), M(1.05f, 0.92f), M(1.05f, 1.05f), M(-0.05f, 1.05f) }, C2(a)); Poly(dc, b, new List<Vector2> { M(-0.05f, 0.2f), M(0.12f, 0.06f), M(0.27f, 0.2f), M(0.19f, 0.25f), M(0.12f, 0.2f), M(0.03f, 0.3f) }, C1(a)); }
        for (int i = 0; i < 300; i++)
        {
            float lag = Rnd(i) * 0.62f, k = prog - lag; if (k <= 0f) continue; k = MathF.Min(k, 1.05f); float x = 0.12f + k * 0.85f + (Rnd(i + 3) - 0.5f) * 0.1f, off = (Rnd(i + 5) - 0.72f) * (0.13f + k * 0.14f), bounce = MathF.Abs(MathF.Sin(t * (4f + Rnd(i + 7) * 5f) + i)) * 0.018f, y = 0.09f + (x - 0.12f) * 0.95f + off - bounce, R = (16f + Rnd(i + 9) * 42f) * u * (0.55f + k * 0.8f);
            Dot(dc, b, M(x, y), R, Col(Gfx.Mix(Color1, Color3, Rnd(i + 11) * 0.32f), (0.42f + Rnd(i + 2) * 0.45f) * a));
        }
    }
}

public sealed class GroundCrackPlugin : QuickShapePlugin { public override string Name => "演出：地割れ（地震の亀裂）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GroundCrackParameter(s); }
public sealed class GroundCrackParameter : Q3Param
{
    public GroundCrackParameter() : this(null) { }
    public GroundCrackParameter(SharedDataStore? s) : base(s) { Count = 7; Color1 = Hex(0x120D0A); Color2 = Hex(0x8D7355); Color3 = Hex(0xC6AA82); }
    static readonly Dictionary<string, ParamGuide> g = G("地割れ", "Count=本数||1|24", "Seed=シード", "Background=地面を塗る", "Color1=ひびの色", "Color2=地面の色", "Color3=砂の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), t = c.Time, p = DrawKit.EaseOut(Sub(t, 0.05f, 0.9f)), quake = (1f - Sub(t, 0.3f, 1f)) * 12f * u; var o = P0(c) + new Vector2(MathF.Sin(t * 53f) * quake, MathF.Cos(t * 47f) * quake); if (Background) { Box(dc, b, 0, 0, w, h, C2(a)); for (int i = 0; i < 160; i++) Dot(dc, b, new(Rnd(i) * w, Rnd(i + 200) * h), (2f + Rnd(i + 400) * 8f) * u, Rgb(0.28f, 0.2f, 0.13f, 0.18f * a)); }
        int n = Math.Clamp(Count, 1, 24);
        for (int s = 0; s < n; s++)
        {
            float th = s * MathF.Tau / n + (Rnd(s + Seed) - 0.5f) * 0.55f, L = (420f + Rnd(s + 8 + Seed) * 560f) * u; var pts = new List<Vector2> { o }; var cur = o; for (int i = 1; i <= 15; i++) { float dir = th + (Rnd(s * 19 + i + Seed) - 0.5f) * 1.05f; cur += new Vector2(MathF.Cos(dir), MathF.Sin(dir)) * L / 15f; pts.Add(cur); } var part = BubbleKit.PartByLength(pts, p); Path(dc, b, part, C1(a), (15f - s % 3 * 3f) * u);
            if (part.Count > 5) { var bq = part[part.Count / 2]; var br = Pol(bq, L * 0.2f * p, th + (s % 2 == 0 ? 0.8f : -0.8f)); Ln(dc, b, bq, br, C1(a), 4f * u); }
            for (int i = 0; i < 6; i++) { if (part.Count < 2) break; var q = part[(int)(Rnd(s * 7 + i) * (part.Count - 1))]; float k = Gfx.Mod(t * 0.9f + Rnd(i + s), 1f); Dot(dc, b, q + new Vector2((Rnd(i + 2) - 0.5f) * 12f * u, k * 30f * u), 3f * u, C3(a * (1f - k))); }
        }
    }
}

public sealed class HazyMoonPlugin : QuickShapePlugin { public override string Name => "演出：おぼろ月"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HazyMoonParameter(s); }
public sealed class HazyMoonParameter : Q3Param
{
    public HazyMoonParameter() : this(null) { }
    public HazyMoonParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFF2C2); Color2 = Hex(0x101A35); Color3 = Hex(0xA8B3C7); }
    protected override double DefAmount => 100;
    protected override double DefAmount2 => 55;
    static readonly Dictionary<string, ParamGuide> g = G("おぼろ月", "Amount=月の形|100で満月", "Amount2=雲の濃さ", "Speed=雲の速さ", "Background=夜空を塗る", "Color1=月の色", "Color2=空の色", "Color3=雲の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), t = c.Time * Spd(c), R = 180f * u, phase = Amt(c), cl = Amt2(c); var o = P0(c) - new Vector2(0, 90f * u); if (Background) Sky(dc, w, h, C2(a), Col(Gfx.Mix(Color2, Hex(0x3C4D70), 0.7f), a));
        using (var gl = Gfx.Radial(dc, o, R * 3.1f, (0f, C1(0.5f * a)), (0.35f, C1(0.17f * a)), (1f, C1(0f)))) dc.FillEllipse(new Ellipse(o, R * 3.1f, R * 3.1f), gl);
        var moon = Gfx.ArcPts(o, R, -MathF.PI / 2f, MathF.PI / 2f, 36); float term = (2f * phase - 1f) * R; for (int i = 0; i <= 36; i++) { float e = MathF.PI / 2f - i / 36f * MathF.PI; moon.Add(o + new Vector2(-MathF.Cos(e) * term, MathF.Sin(e) * R)); } Poly(dc, b, moon, C1(a));
        for (int i = 0; i < 12; i++) { float span = w + 900f * u, x = Gfx.Mod(Rnd(i) * span + t * (12f + Rnd(i + 3) * 25f) * u, span) - 450f * u, y = o.Y + (Rnd(i + 5) - 0.5f) * 520f * u, rx = (210f + Rnd(i + 7) * 260f) * u; using var br = Gfx.Radial(dc, new(x, y), rx, (0f, C3((0.3f + cl * 0.42f) * a)), (0.62f, C3(cl * 0.28f * a)), (1f, C3(0f))); dc.FillEllipse(new Ellipse(new(x, y), rx, rx * 0.25f), br); }
    }
}

public sealed class MistyLakePlugin : QuickShapePlugin { public override string Name => "演出：朝霧の湖（水面に映る）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MistyLakeParameter(s); }
public sealed class MistyLakeParameter : Q3Param
{
    public MistyLakeParameter() : this(null) { }
    public MistyLakeParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x47705F); Color2 = Hex(0xE7EEF0); }
    protected override double DefAmount => 55;
    static readonly Dictionary<string, ParamGuide> g = G("朝霧の湖", "Variant=時間帯|1=朝　2=昼　3=夕方　4=夜|1|4", "Amount=霧の濃さ", "Speed=霧の速さ", "Color1=山の色", "Color2=霧の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c), lake = h * 0.56f, mist = Amt(c); var (top, low) = Variant switch { 2 => (Hex(0x5EA8E8), Hex(0xCDE9F7)), 3 => (Hex(0x70528F), Hex(0xFFB56B)), 4 => (Hex(0x0C1630), Hex(0x2E446B)), _ => (Hex(0x9BC4DF), Hex(0xFFE6C9)) };
        Shade(dc, new(0, 0), new(0, lake), Col(top, a), Col(low, a), 0, 0, w, lake); Shade(dc, new(0, lake), new(0, h), Col(low, a), Col(Gfx.Mix(top, Hex(0x06121E), 0.35f), a), 0, lake, w, h);
        for (int l = 0; l < 3; l++) { var up = new List<Vector2> { new(0, lake) }; var dn = new List<Vector2> { new(0, lake) }; for (int i = 0; i <= 48; i++) { float k = i / 48f, H = (MathF.Abs(MathF.Sin(k * (5f + l * 2f) + l * 1.4f)) * (95f - l * 12f) + MathF.Abs(MathF.Sin(k * (13f + l) + l)) * 34f + 58f - l * 8f) * u * (1.65f - l * 0.32f); up.Add(new(k * w, lake - H)); dn.Add(new(k * w + MathF.Sin(t * 1.2f + k * 40f) * 3f * u, lake + H * 0.82f)); } up.Add(new(w, lake)); dn.Add(new(w, lake)); var col = Gfx.Mix(Color1, low, 0.62f - l * 0.26f); if (Variant == 4) col = Gfx.Mix(col, Hex(0x07101F), 0.65f); Poly(dc, b, up, Col(col, a)); Poly(dc, b, dn, Col(col, 0.52f * a)); }
        for (int i = 0; i < 26; i++) { float y = lake + (Rnd(i) * 0.9f + 0.03f) * (h - lake), x = Gfx.Mod(Rnd(i + 5) * w + MathF.Sin(t * 0.7f + i) * 22f * u, w), L = (50f + Rnd(i + 7) * 130f) * u; Ln(dc, b, new(x - L, y), new(x + L, y), Rgb(1, 1, 1, 0.13f * a), 2f * u); }
        for (int i = 0; i < 10; i++) { float span = w + 1000f * u, x = Gfx.Mod(Rnd(i + 20) * span + t * (10f + i * 3f) * u, span) - 500f * u, y = lake + (Rnd(i + 30) - 0.58f) * 150f * u, rx = (360f + Rnd(i + 40) * 280f) * u; using var br = Gfx.Radial(dc, new(x, y), rx, (0f, C2((0.2f + mist * 0.55f) * a)), (1f, C2(0f))); dc.FillEllipse(new Ellipse(new(x, y), rx, rx * 0.2f), br); }
    }
}

public sealed class SunflowerFieldPlugin : QuickShapePlugin { public override string Name => "演出：ひまわり畑"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SunflowerFieldParameter(s); }
public sealed class SunflowerFieldParameter : Q3Param
{
    public SunflowerFieldParameter() : this(null) { }
    public SunflowerFieldParameter(SharedDataStore? s) : base(s) { Count = 34; Color1 = Hex(0xFFCF1F); Color2 = Hex(0x2F8FE6); Color3 = Hex(0x3F8A3A); }
    static readonly Dictionary<string, ParamGuide> g = G("ひまわり畑", "Count=密度||6|120", "Speed=風の速さ", "Color1=花の色", "Color2=空の色", "Color3=葉の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c); int n = Math.Clamp(Count, 6, 120); Sky(dc, w, h * 0.72f, C2(a), Rgb(0.82f, 0.95f, 1f, a)); for (int i = 0; i < 5; i++) { float x = Gfx.Mod(Rnd(i) * w + t * 10f * u, w + 300f * u) - 150f * u, y = h * (0.08f + Rnd(i + 2) * 0.24f); for (int k = 0; k < 4; k++) Dot(dc, b, new(x + k * 48f * u, y + MathF.Sin(k * 2f) * 11f * u), (38f - MathF.Abs(k - 1.5f) * 8f) * u, Rgb(1, 1, 1, 0.92f * a)); }
        Box(dc, b, 0, h * 0.55f, w, h, C3(a));
        for (int i = 0; i < n; i++)
        {
            float d = (i + 1f) / n, S = (24f + d * d * 88f) * u, bx = Rnd(i * 7 + 2) * w, by = h * (0.57f + d * d * 0.45f), wind = MathF.Sin(t * 1.25f + bx * 0.003f) * S * 0.28f; var head = new Vector2(bx + wind, by - S * 1.7f); Ln(dc, b, new(bx, by + S), head, Col(Gfx.Mix(Color3, Hex(0x173B18), 0.28f), a), MathF.Max(2f, S * 0.11f)); Oval(dc, b, new(bx - S * 0.35f, by - S * 0.25f), S * 0.42f, S * 0.18f, C3(a)); Oval(dc, b, new(bx + S * 0.35f, by - S * 0.5f), S * 0.42f, S * 0.18f, C3(a));
            for (int p = 0; p < 16; p++) { float th = p * MathF.Tau / 16f + wind * 0.01f; Poly(dc, b, new List<Vector2> { Pol(head, S * 0.35f, th - 0.22f), Pol(head, S * 0.84f, th), Pol(head, S * 0.35f, th + 0.22f) }, Col(p % 2 == 0 ? Color1 : Gfx.Mix(Color1, Hex(0xFF8F00), 0.32f), a)); } Dot(dc, b, head, S * 0.42f, Rgb(0.27f, 0.15f, 0.07f, a)); Dot(dc, b, head, S * 0.2f, Rgb(0.45f, 0.27f, 0.1f, a));
        }
    }
}

public sealed class RollingAcornsPlugin : QuickShapePlugin { public override string Name => "演出：どんぐりが転がる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RollingAcornsParameter(s); }
public sealed class RollingAcornsParameter : Q3Param
{
    public RollingAcornsParameter() : this(null) { }
    public RollingAcornsParameter(SharedDataStore? s) : base(s) { Count = 6; Color1 = Hex(0xB86F2C); Color2 = Hex(0x6D4427); Color3 = Hex(0x8A5A32); }
    static readonly Dictionary<string, ParamGuide> g = G("どんぐり", "Count=数||1|24", "Variant=転がる向き|1=右へ　2=左へ|1|2", "Color1=実の色", "Color2=帽子の色", "Color3=松ぼっくりの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, dir = Variant == 2 ? -1f : 1f, W = c.Width; var o = P0(c) + new Vector2(0, 170f * u); int n = Math.Clamp(Count, 1, 24); Ln(dc, b, new(0, o.Y + 42f * u), new(W, o.Y + 42f * u), Rgb(0.45f, 0.32f, 0.2f, 0.85f * a), 5f * u);
        for (int i = 0; i < n; i++)
        {
            float k = DrawKit.EaseOut(Sub(t, i * 0.16f, 1.25f + Rnd(i) * 0.4f)), goal = (i - (n - 1) / 2f) * 112f * u + (Rnd(i + 4) - 0.5f) * 30f * u, startX = -dir * (W * 0.72f + i * 45f * u), x = o.X + Lerp(startX, goal, k), hop = MathF.Abs(MathF.Sin(k * MathF.PI * 3f)) * (1f - k) * 76f * u, ang = dir * (1f - k) * -18f + dir * 0.18f; var q = new Vector2(x, o.Y - hop); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(ang + MathF.PI / 2f, q) * prev;
            if (i % 3 == 2) { Oval(dc, b, q, 34f * u, 42f * u, C3(a)); for (int l = -2; l <= 2; l++) { float ly = l * 14f * u; Path(dc, b, new List<Vector2> { q + new Vector2(-30f * u, ly), q + new Vector2(0, ly + 9f * u), q + new Vector2(30f * u, ly) }, Rgb(0.28f, 0.16f, 0.08f, a), 3f * u); } }
            else { Oval(dc, b, q + new Vector2(0, 8f * u), 30f * u, 37f * u, C1(a)); Oval(dc, b, q + new Vector2(-9f * u, 10f * u), 6f * u, 16f * u, Rgb(1, 0.85f, 0.65f, 0.45f * a)); Pie(dc, b, q - new Vector2(0, 8f * u), 34f * u, MathF.PI, MathF.Tau, C2(a)); Ln(dc, b, q - new Vector2(0, 40f * u), q - new Vector2(0, 54f * u), C2(a), 6f * u); }
            dc.Transform = prev;
        }
    }
}

public sealed class SnailOnLeafPlugin : QuickShapePlugin { public override string Name => "演出：雨上がりのカタツムリ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SnailOnLeafParameter(s); }
public sealed class SnailOnLeafParameter : Q3Param
{
    public SnailOnLeafParameter() : this(null) { }
    public SnailOnLeafParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xB9764A); Color2 = Hex(0x3F9550); Color3 = Hex(0x8C8FE8); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("カタツムリ", "Amount=進む速さ", "Variant=向き|1=右へ　2=左へ|1|2", "Color1=殻の色", "Color2=葉の色", "Color3=あじさいの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, dir = Variant == 2 ? -1f : 1f; var o = P0(c); for (int i = 0; i < 70; i++) { float th = Rnd(i) * MathF.Tau, r = MathF.Sqrt(Rnd(i + 3)) * 300f * u; var q = o + new Vector2(MathF.Cos(th) * r * 1.3f, MathF.Sin(th) * r * 0.62f - 240f * u); float pet = (24f + Rnd(i + 5) * 8f) * u; var col = Gfx.Mix(Color3, Hex(0xE7C5FF), Rnd(i + 7) * 0.7f); for (int p = 0; p < 4; p++) Dot(dc, b, Pol(q, pet * 0.62f, p * MathF.PI / 2f + i), pet * 0.62f, Col(col, 0.95f * a)); Dot(dc, b, q, 4f * u, Rgb(1, 1, 0.75f, a)); }
        Oval(dc, b, o + new Vector2(0, 150f * u), 700f * u, 130f * u, C2(a)); Ln(dc, b, o + new Vector2(-690f * u, 150f * u), o + new Vector2(690f * u, 150f * u), Col(Gfx.Mix(Color2, Hex(0xFFFFFF), 0.35f), a), 5f * u); float crawl = Gfx.Mod(t * (2f + Amt(c) * 58f), 900f) - 450f, x = o.X + dir * crawl * u, stretch = 1f + MathF.Sin(t * 2.3f) * 0.07f; var base0 = new Vector2(x, o.Y + 75f * u);
        Ln(dc, b, base0 - new Vector2(dir * 150f * u, -6f * u), base0 - new Vector2(dir * 520f * u, -6f * u), Rgb(1, 1, 1, 0.22f * a), 8f * u); Path(dc, b, new List<Vector2> { base0 + new Vector2(-dir * 120f * u * stretch, 0), base0 + new Vector2(dir * 95f * u * stretch, 0), base0 + new Vector2(dir * 138f * u * stretch, -48f * u) }, Rgb(0.88f, 0.76f, 0.58f, a), 42f * u); var head = base0 + new Vector2(dir * 145f * u * stretch, -60f * u); foreach (int s in new[] { -1, 1 }) { var tip = head + new Vector2(dir * (s * 16f + 22f) * u, -68f * u + MathF.Sin(t * 2f + s) * 6f * u); Ln(dc, b, head, tip, Rgb(0.88f, 0.76f, 0.58f, a), 7f * u); Dot(dc, b, tip, 8f * u, Rgb(0.2f, 0.15f, 0.1f, a)); }
        var shell = base0 + new Vector2(-dir * 20f * u, -80f * u); Dot(dc, b, shell, 92f * u, C1(a)); var spiral = new List<Vector2>(); for (int i = 0; i <= 70; i++) { float k = i / 70f; spiral.Add(Pol(shell, (1f - k) * 80f * u, k * 15f * dir)); } Path(dc, b, spiral, Rgb(0.34f, 0.18f, 0.09f, a), 7f * u);
    }
}
