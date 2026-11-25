using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 511〜530: 季節・行事・和風(花札・家紋・歌舞伎などは、実物をまねない簡略な絵柄)

public abstract class Q3WaParam : Q3Param
{
    protected Q3WaParam(SharedDataStore? s) : base(s) { }
    protected static void Crane(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 q, float S, float dir, float flap, float a)
    {
        var white = Rgb(1, 1, 1, a); var ink = Rgb(0.08f, 0.08f, 0.1f, a);
        Oval(dc, b, q, S * 0.55f, S * 0.2f, white); Path(dc, b, new List<Vector2> { q + new Vector2(dir * S * 0.4f, -S * 0.05f), q + new Vector2(dir * S * 0.82f, -S * 0.26f), q + new Vector2(dir * S * 1.18f, -S * 0.2f) }, white, S * 0.075f); Dot(dc, b, q + new Vector2(dir * S * 1.18f, -S * 0.2f), S * 0.09f, Rgb(0.9f, 0.1f, 0.12f, a)); Ln(dc, b, q + new Vector2(dir * S * 1.24f, -S * 0.2f), q + new Vector2(dir * S * 1.48f, -S * 0.16f), Rgb(0.95f, 0.7f, 0.2f, a), S * 0.035f);
        Poly(dc, b, new List<Vector2> { q + new Vector2(dir * S * 0.28f, -S * 0.08f), q + new Vector2(-dir * S * 0.28f, -S * 0.05f), q + new Vector2(-dir * S * 0.7f, -S * (0.65f + flap * 0.45f)), q + new Vector2(dir * S * 0.12f, -S * (0.84f + flap * 0.38f)) }, white); Poly(dc, b, new List<Vector2> { q + new Vector2(-dir * S * 0.7f, -S * (0.65f + flap * 0.45f)), q + new Vector2(dir * S * 0.12f, -S * (0.84f + flap * 0.38f)), q + new Vector2(-dir * S * 0.18f, -S * (0.92f + flap * 0.3f)) }, ink); Ln(dc, b, q - new Vector2(dir * S * 0.45f, 0), q - new Vector2(dir * S * 1.05f, -S * 0.1f), white, S * 0.04f);
    }
    protected static void Lantern(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 q, float S, Color4 col, float a)
    {
        Dot(dc, b, q, S * 1.8f, new Color4(col.R, col.G, col.B, col.A * 0.16f)); Oval(dc, b, q, S * 0.72f, S, col); for (int i = -2; i <= 2; i++) Ln(dc, b, q + new Vector2(-S * 0.67f * MathF.Sqrt(1f - i * i / 6.25f), i * S * 0.36f), q + new Vector2(S * 0.67f * MathF.Sqrt(1f - i * i / 6.25f), i * S * 0.36f), Rgb(0.45f, 0.08f, 0.04f, 0.4f * a), S * 0.035f); Box(dc, b, q.X - S * 0.34f, q.Y - S * 1.1f, q.X + S * 0.34f, q.Y - S * 0.86f, Rgb(0.1f, 0.08f, 0.07f, a)); Box(dc, b, q.X - S * 0.34f, q.Y + S * 0.86f, q.X + S * 0.34f, q.Y + S * 1.1f, Rgb(0.1f, 0.08f, 0.07f, a));
    }
}

public sealed class ComingOfAgePlugin : QuickShapePlugin { public override string Name => "演出：成人式（振袖と鶴）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ComingOfAgeParameter(s); }
public sealed class ComingOfAgeParameter : Q3WaParam
{
    public ComingOfAgeParameter() : this(null) { }
    public ComingOfAgeParameter(SharedDataStore? s) : base(s) { Text = "祝 成人"; Count = 5; Color1 = Hex(0xC8102E); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xE6B422); }
    static readonly Dictionary<string, ParamGuide> g = G("成人式", "Text=文字", "Count=鶴の数||1|12", "Color1=背景の色1", "Color2=背景の色2", "Color3=金の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time; for (int i = -8; i < 17; i++) { float x = i / 16f * w; Poly(dc, b, new List<Vector2> { new(x, 0), new(x + w / 16f, 0), new(x + w / 16f + h * 0.25f, h), new(x + h * 0.25f, h) }, i % 2 == 0 ? C1(a) : Col(Gfx.Mix(Color1, Color2, 0.86f), a)); }
        Frame(dc, b, 34f * u, 34f * u, w - 34f * u, h - 34f * u, C3(a), 8f * u); for (int i = 0; i < Math.Clamp(Count, 1, 12); i++) { float dir = i % 2 == 0 ? 1f : -1f, span = w + 500f * u, x = Gfx.Mod(Rnd(i) * span + dir * t * (35f + Rnd(i + 3) * 30f) * u, span) - 250f * u, y = h * (0.2f + Rnd(i + 8) * 0.62f) + MathF.Sin(t + i) * 22f * u; Crane(dc, b, new(x, y), (68f + Rnd(i + 4) * 40f) * u, dir, MathF.Sin(t * 4f + i), a); }
        var o = new Vector2(w / 2f, h / 2f); float p = DrawKit.EaseOutBack(Intro(c, 0.15f, 0.5f)); Panel(dc, b, o, 900f * u * p, 280f * u * p, 14f * u, Rgb(0.08f, 0.05f, 0.04f, 0.72f * a), C3(a), 5f * u); Say(dc, b, Str, 142f * u, o, C3(a), Rgb(0.2f, 0.05f, 0.03f, a), 5f * u, maxW: 840f * u, scale: MathF.Max(0.01f, p));
    }
}

public sealed class KabutoDecorPlugin : QuickShapePlugin { public override string Name => "演出：端午の節句の兜飾り"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new KabutoDecorParameter(s); }
public sealed class KabutoDecorParameter : Q3WaParam
{
    public KabutoDecorParameter() : this(null) { }
    public KabutoDecorParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x233A5E); Color2 = Hex(0xE6B422); Color3 = Hex(0x6C4AB6); }
    static readonly Dictionary<string, ParamGuide> g = G("兜飾り", "Background=背景を塗る", "Color1=兜の色", "Color2=金の色", "Color3=菖蒲の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0f, 0.5f)); var o = P0(c); if (Background) Sky(dc, c.Width, c.Height, Rgb(0.65f, 0.86f, 0.95f, a), Rgb(0.92f, 0.96f, 0.85f, a));
        foreach (int s in new[] { -1, 1 }) { var st = o + new Vector2(s * 470f * u, 330f * u); for (int l = -1; l <= 1; l++) Path(dc, b, new List<Vector2> { st, st + new Vector2(l * 55f * u, -250f * u), st + new Vector2(l * 96f * u, -480f * u) }, Rgb(0.2f, 0.55f, 0.27f, a), 14f * u); var f = st - new Vector2(0, 410f * u); for (int k = 0; k < 3; k++) { float th = -MathF.PI / 2f + k * MathF.Tau / 3f; Oval(dc, b, Pol(f, 42f * u, th), 28f * u, 45f * u, C3(a)); } Dot(dc, b, f, 15f * u, C2(a)); }
        var h0 = o + new Vector2(0, 40f * u); for (int r = 0; r < 4; r++) { float y = h0.Y + 30f * u + r * 52f * u, W = (250f + r * 42f) * u * p; Poly(dc, b, new List<Vector2> { new(h0.X - W, y), new(h0.X + W, y), new(h0.X + W + 34f * u, y + 48f * u), new(h0.X - W - 34f * u, y + 48f * u) }, r % 2 == 0 ? C1(a) : Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.15f), a)); Ln(dc, b, new(h0.X - W - 34f * u, y + 48f * u), new(h0.X + W + 34f * u, y + 48f * u), C2(a), 4f * u); }
        Pie(dc, b, h0 + new Vector2(0, 34f * u), 245f * u * p, MathF.PI, MathF.Tau, C1(a)); for (int i = -4; i <= 4; i++) Ln(dc, b, h0 + new Vector2(i * 24f * u, -190f * u * MathF.Sqrt(1f - i * i / 36f)), h0 + new Vector2(i * 54f * u, 30f * u), C2(0.45f * a), 3f * u); foreach (int s in new[] { -1, 1 }) { Path(dc, b, new List<Vector2> { h0 + new Vector2(s * 58f * u, -115f * u), h0 + new Vector2(s * 165f * u, -240f * u), h0 + new Vector2(s * 190f * u, -405f * u) }, C2(a), 30f * u); RBox(dc, b, h0 + new Vector2(s * 305f * u, 24f * u), 110f * u, 64f * u, 18f * u, C2(a)); } Star(dc, b, h0 - new Vector2(0, 88f * u), 58f * u, 8, 0.52f, 0f, C2(a));
    }
}

public sealed class FloatingLanternsPlugin : QuickShapePlugin { public override string Name => "演出：灯籠流し"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FloatingLanternsParameter(s); }
public sealed class FloatingLanternsParameter : Q3WaParam
{
    public FloatingLanternsParameter() : this(null) { }
    public FloatingLanternsParameter(SharedDataStore? s) : base(s) { Count = 16; Color1 = Hex(0xFFB84D); Color2 = Hex(0x071426); Color3 = Hex(0x172D45); }
    static readonly Dictionary<string, ParamGuide> g = G("灯籠流し", "Count=灯籠の数||1|60", "Speed=流れる速さ", "Color1=灯りの色", "Color2=空の色", "Color3=川の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c), bank = h * 0.33f; Box(dc, b, 0, 0, w, bank, C2(a)); Shade(dc, new(0, bank), new(0, h), C3(a), C2(a), 0, bank, w, h); var hill = new List<Vector2> { new(0, bank) }; for (int i = 0; i <= 40; i++) hill.Add(new(i / 40f * w, bank - (MathF.Sin(i * 0.37f) * 34f + MathF.Sin(i * 0.13f) * 46f + 62f) * u)); hill.Add(new(w, bank)); Poly(dc, b, hill, Rgb(0.015f, 0.04f, 0.07f, a));
        for (int i = 0; i < Math.Clamp(Count, 1, 60); i++)
        {
            float d = (i + 1f) / Math.Clamp(Count, 1, 60), S = (14f + d * 42f) * u, span = w + 240f * u, x = Gfx.Mod(Rnd(i) * span + t * (10f + d * 28f) * u, span) - 120f * u, y = bank + (0.1f + d * 0.8f) * (h - bank) + MathF.Sin(t + i) * 4f * u, fl = 0.8f + 0.2f * MathF.Sin(t * 7f + i * 2f);
            for (int k = 0; k < 7; k++) { float ry = y + S * 1.0f + k * S * 0.48f, rw = S * (1.0f + k * 0.08f) + MathF.Sin(t * 3f + k + i) * S * 0.2f; Ln(dc, b, new(x - rw, ry), new(x + rw, ry), C1((0.46f - k * 0.06f) * fl * a), MathF.Max(1.5f, S * 0.09f)); }
            Dot(dc, b, new(x, y), S * 2f, C1(0.09f * fl * a)); Poly(dc, b, new List<Vector2> { new(x - S * 0.72f, y - S * 0.86f), new(x + S * 0.72f, y - S * 0.86f), new(x + S * 0.84f, y + S * 0.68f), new(x - S * 0.84f, y + S * 0.68f) }, C1(fl * a)); Box(dc, b, x - S, y + S * 0.68f, x + S, y + S * 0.92f, Rgb(0.2f, 0.12f, 0.07f, a)); Ln(dc, b, new(x, y - S * 0.86f), new(x, y + S * 0.68f), Rgb(0.45f, 0.2f, 0.05f, 0.45f * a), MathF.Max(1f, S * 0.05f));
        }
    }
}

public sealed class BonDanceTowerPlugin : QuickShapePlugin { public override string Name => "演出：盆踊りのやぐら"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BonDanceTowerParameter(s); }
public sealed class BonDanceTowerParameter : Q3WaParam
{
    public BonDanceTowerParameter() : this(null) { }
    public BonDanceTowerParameter(SharedDataStore? s) : base(s) { Count = 14; Color1 = Hex(0xFF5A36); Color2 = Hex(0x101B3B); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("盆踊りのやぐら", "Count=提灯の数||4|40", "Speed=揺れの速さ", "Color1=提灯の色", "Color2=空の色", "Color3=幕の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c), beat = MathF.Pow(MathF.Abs(MathF.Sin(t * 3.14f)), 6f); int n = Math.Clamp(Count, 4, 40); var o = new Vector2(w / 2f, h); Sky(dc, w, h, C2(a), Col(Gfx.Mix(Color2, Hex(0x713D5B), 0.7f), a)); Box(dc, b, 0, h * 0.86f, w, h, Rgb(0.08f, 0.06f, 0.06f, a));
        var top = o - new Vector2(0, 760f * u); foreach (int s in new[] { -1, 1 }) { var pts = new List<Vector2>(); for (int i = 0; i <= n / 2; i++) { float k = i / MathF.Max(1f, n / 2), x = top.X + s * k * w * 0.52f, y = top.Y + 20f * u + k * 170f * u + MathF.Sin(k * MathF.PI) * 55f * u; pts.Add(new(x, y)); } Path(dc, b, pts, Rgb(0.08f, 0.08f, 0.1f, a), 3f * u); for (int i = 1; i < pts.Count; i++) { float sw = MathF.Sin(t * 3.14f + i * 0.8f) * 16f * u; Lantern(dc, b, pts[i] + new Vector2(sw, 44f * u), (27f + beat * 2.5f) * u, C1((0.86f + beat * 0.14f) * a), a); } }
        foreach (int s in new[] { -1, 1 }) { Ln(dc, b, o + new Vector2(s * 330f * u, -120f * u), o + new Vector2(s * 210f * u, -660f * u), Rgb(0.32f, 0.19f, 0.1f, a), 22f * u); Ln(dc, b, o + new Vector2(s * 330f * u, -120f * u), o + new Vector2(-s * 275f * u, -395f * u), Rgb(0.32f, 0.19f, 0.1f, a), 10f * u); }
        Box(dc, b, o.X - 300f * u, o.Y - 415f * u, o.X + 300f * u, o.Y - 310f * u, C3(a)); for (int i = 0; i < 10; i++) if (i % 2 == 0) Box(dc, b, o.X - 300f * u + i * 60f * u, o.Y - 415f * u, o.X - 240f * u + i * 60f * u, o.Y - 310f * u, C1(a)); Poly(dc, b, new List<Vector2> { o + new Vector2(-280f * u, -650f * u), o + new Vector2(280f * u, -650f * u), o + new Vector2(0, -790f * u) }, Rgb(0.2f, 0.12f, 0.08f, a)); var drum = o - new Vector2(0, 505f * u); Oval(dc, b, drum, 74f * u * (1f + beat * 0.05f), 84f * u, Rgb(0.45f, 0.14f, 0.07f, a)); Dot(dc, b, drum, 55f * u, Rgb(0.92f, 0.82f, 0.62f, a)); Ring(dc, b, drum, (68f + beat * 58f) * u, C3(a * beat), 4f * u);
    }
}

public sealed class ShichiGoSanPlugin : QuickShapePlugin { public override string Name => "演出：七五三（千歳飴）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ShichiGoSanParameter(s); }
public sealed class ShichiGoSanParameter : Q3WaParam
{
    public ShichiGoSanParameter() : this(null) { }
    public ShichiGoSanParameter(SharedDataStore? s) : base(s) { Text = "祝 七五三"; Color1 = Hex(0xD7263D); Color2 = Hex(0xFFF7E8); Color3 = Hex(0xE6B422); }
    static readonly Dictionary<string, ParamGuide> g = G("七五三", "Text=文字", "Color1=袋の色", "Color2=白の色", "Color3=金の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0f, 0.45f)), t = c.Time; var o = P0(c); var bag = o + new Vector2(-280f * u, 0); RBox(dc, b, bag, 280f * u * p, 760f * u * p, 14f * u, C1(a)); RFrame(dc, b, bag, 250f * u * p, 730f * u * p, 10f * u, C3(a), 4f * u); Say(dc, b, "千", 88f * u, bag - new Vector2(0, 170f * u), C2(a)); Say(dc, b, "歳", 88f * u, bag, C2(a)); Say(dc, b, "飴", 88f * u, bag + new Vector2(0, 170f * u), C2(a));
        foreach (int s in new[] { -1, 1 }) Path(dc, b, new List<Vector2> { bag + new Vector2(s * 136f * u, -340f * u), bag + new Vector2(s * 54f * u, -438f * u), bag + new Vector2(0, -470f * u) }, C2(a), 8f * u); for (int i = 0; i < 2; i++) { var cand = bag + new Vector2(-50f * u + i * 100f * u, -448f * u); RBox(dc, b, cand, 32f * u, 120f * u, 16f * u, i == 0 ? C2(a) : Rgb(1, 0.58f, 0.66f, a)); }
        Crane(dc, b, o + new Vector2(190f * u, -160f * u + MathF.Sin(t * 1.5f) * 12f * u), 120f * u, -1f, MathF.Sin(t * 4f), a); var turtle = o + new Vector2(265f * u, 240f * u); Oval(dc, b, turtle, 150f * u, 92f * u, Rgb(0.2f, 0.55f, 0.32f, a)); for (int i = 0; i < 6; i++) { var hx = new List<Vector2>(); var ct = i == 5 ? turtle : Pol(turtle, 72f * u, i * MathF.Tau / 5f); for (int k = 0; k < 6; k++) hx.Add(Pol(ct, 30f * u, k * MathF.PI / 3f)); Outline(dc, b, hx, C3(a), 3f * u); } Dot(dc, b, turtle + new Vector2(178f * u, -20f * u), 42f * u, Rgb(0.45f, 0.72f, 0.42f, a)); Dot(dc, b, turtle + new Vector2(192f * u, -32f * u), 6f * u, Rgb(0, 0, 0, a));
        Say(dc, b, Str, 72f * u, o + new Vector2(150f * u, 420f * u), C3(a), Rgb(0.25f, 0.05f, 0.05f, a), 4f * u, maxW: 620f * u);
    }
}

public sealed class BigCleaningPlugin : QuickShapePlugin { public override string Name => "演出：大掃除（ほうきとほこり）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BigCleaningParameter(s); }
public sealed class BigCleaningParameter : Q3Param
{
    public BigCleaningParameter() : this(null) { }
    public BigCleaningParameter(SharedDataStore? s) : base(s) { Count = 60; Color1 = Hex(0xD8B66A); Color2 = Hex(0x9A9184); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("大掃除", "Variant=掃く向き|1=右へ　2=左へ|1|2", "Count=ほこりの量||0|300", "Speed=速さ", "Color1=ほうきの色", "Color2=ほこりの色", "Color3=光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)), dir = Variant == 2 ? -1f : 1f, k = Sub(t, 0f, 2.6f), W = 1320f * u, wig = MathF.Sin(t * 10f) * 28f * u * (1f - Sub(k, 0.92f, 0.08f)); var o = P0(c) + new Vector2(0, 245f * u); float x = o.X + dir * Lerp(-W / 2f, W / 2f, k); Ln(dc, b, o - new Vector2(760f * u, -35f * u), o + new Vector2(760f * u, 35f * u), Rgb(1, 1, 1, 0.45f * a), 4f * u);
        for (int i = 0; i < Math.Clamp(Count, 0, 300); i++) { float px = (Rnd(i) - 0.5f) * W, passed = dir * (x - (o.X + px)); if (passed > 100f * u) continue; float e = passed > -40f * u ? Sub(passed, -40f * u, 140f * u) : 0f; var q = new Vector2(o.X + px + dir * e * 110f * u, o.Y + 16f * u - e * (40f + Rnd(i + 3) * 170f) * u); Dot(dc, b, q, (3f + Rnd(i + 7) * 8f) * u, C2((0.72f - e * 0.65f) * a)); }
        var head = new Vector2(x + wig, o.Y); var hand = head + new Vector2(-dir * 170f * u, -560f * u); Ln(dc, b, hand, head, Rgb(0.52f, 0.31f, 0.15f, a), 15f * u); Poly(dc, b, new List<Vector2> { head + new Vector2(-34f * u, -40f * u), head + new Vector2(34f * u, -40f * u), head + new Vector2(120f * u, 34f * u), head + new Vector2(-120f * u, 34f * u) }, C1(a)); for (int i = -5; i <= 5; i++) Ln(dc, b, head + new Vector2(i * 6f * u, -38f * u), head + new Vector2(i * 22f * u, 34f * u), Rgb(0.55f, 0.4f, 0.16f, a), 2f * u); Ln(dc, b, head + new Vector2(-40f * u, -24f * u), head + new Vector2(40f * u, -24f * u), Rgb(0.75f, 0.1f, 0.1f, a), 7f * u);
        float shine = Sub(k, 0.9f, 0.1f); if (shine > 0f) for (int i = 0; i < 8; i++) { var q = o + new Vector2((Rnd(i + 90) - 0.5f) * W, -(80f + Rnd(i + 91) * 380f) * u); float tw = MathF.Max(0f, MathF.Sin(t * 5f + i)); Star(dc, b, q, 46f * u * tw * shine, 4, 0.13f, 0f, C3(a)); }
    }
}

public sealed class NewYearCardPlugin : QuickShapePlugin { public override string Name => "演出：年賀状（干支のはんこ）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new NewYearCardParameter(s); }
public sealed class NewYearCardParameter : Q3Param
{
    public NewYearCardParameter() : this(null) { }
    public NewYearCardParameter(SharedDataStore? s) : base(s) { Text = "謹賀新年\n2027\n未"; Color1 = Hex(0xFFF9EC); Color2 = Hex(0xC8102E); Color3 = Hex(0x20242A); }
    static readonly Dictionary<string, ParamGuide> g = G("年賀状", "Text=内容|1行目=あいさつ　2行目=年　3行目=干支の漢字", "Color1=はがきの色", "Color2=はんこの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; string s1 = r.Count > 0 ? r[0] : "", s2 = r.Count > 1 ? r[1] : "", s3 = r.Count > 2 ? r[2] : ""; float u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0f, 0.45f)), W = 620f * u, H = 920f * u; var o = P0(c); RBox(dc, b, o + new Vector2(10f, 12f) * u, W * p, H * p, 8f * u, Rgb(0, 0, 0, 0.25f * a)); RBox(dc, b, o, W * p, H * p, 8f * u, C1(a)); RFrame(dc, b, o, (W - 44f * u) * p, (H - 44f * u) * p, 4f * u, C2(0.8f * a), 3f * u);
        for (int i = 0; i < s1.Length; i++) { float k = Sub(c.Time, 0.25f + i * 0.13f, 0.2f); Say(dc, b, s1[i].ToString(), 118f * u, o + new Vector2(130f * u, -320f * u + i * 134f * u), C3(a * k)); } Say(dc, b, s2 + " 元旦", 36f * u, o + new Vector2(-145f * u, 370f * u), C3(a));
        float st = Sub(c.Time, 0.9f, 0.2f); if (st > 0f) { float sc = 1.6f - 0.6f * DrawKit.EaseOut(st); var q = o + new Vector2(-135f * u, 40f * u); RBox(dc, b, q, 210f * u * sc, 210f * u * sc, 28f * u, C2(a * st)); RFrame(dc, b, q, 174f * u * sc, 174f * u * sc, 18f * u, C1(a * st), 5f * u); Say(dc, b, s3, 112f * u, q, C1(a * st), scale: sc); }
    }
}

public sealed class KagamiMochiPlugin : QuickShapePlugin { public override string Name => "演出：鏡餅"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new KagamiMochiParameter(s); }
public sealed class KagamiMochiParameter : Q3Param
{
    public KagamiMochiParameter() : this(null) { }
    public KagamiMochiParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFF9EA); Color2 = Hex(0xFF9A1F); Color3 = Hex(0xD7263D); }
    static readonly Dictionary<string, ParamGuide> g = G("鏡餅", "Background=飾りをつける", "Color1=餅の色", "Color2=みかんの色", "Color3=敷き紙のふちの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0f, 0.5f)); var o = P0(c) + new Vector2(0, 80f * u); Poly(dc, b, new List<Vector2> { o + new Vector2(-390f * u, 215f * u), o + new Vector2(390f * u, 215f * u), o + new Vector2(310f * u, 335f * u), o + new Vector2(-310f * u, 335f * u) }, Rgb(0.67f, 0.42f, 0.2f, a)); Poly(dc, b, new List<Vector2> { o + new Vector2(0, 90f * u), o + new Vector2(430f * u, 215f * u), o + new Vector2(0, 300f * u), o + new Vector2(-430f * u, 215f * u) }, Rgb(1, 1, 1, a)); Outline(dc, b, new List<Vector2> { o + new Vector2(0, 90f * u), o + new Vector2(430f * u, 215f * u), o + new Vector2(0, 300f * u), o + new Vector2(-430f * u, 215f * u) }, C3(a), 7f * u);
        Oval(dc, b, o + new Vector2(0, 110f * u), 310f * u * p, 145f * u * p, C1(a)); Oval(dc, b, o + new Vector2(0, -60f * u), 230f * u * p, 118f * u * p, C1(a)); Arc(dc, b, o + new Vector2(0, -60f * u), 225f * u, 0.15f, MathF.PI - 0.15f, Rgb(0.82f, 0.74f, 0.6f, a), 3f * u); var m = o - new Vector2(0, 205f * u); Dot(dc, b, m, 72f * u * p, C2(a)); Dot(dc, b, m - new Vector2(20f, 20f) * u, 18f * u * p, Rgb(1, 0.82f, 0.5f, 0.75f * a)); Star(dc, b, m - new Vector2(0, 66f * u), 16f * u, 5, 0.45f, -MathF.PI / 2f, Rgb(0.2f, 0.45f, 0.18f, a));
        if (Background) { foreach (int s in new[] { -1, 1 }) { Oval(dc, b, m + new Vector2(s * 76f * u, -18f * u), 66f * u, 22f * u, Rgb(0.18f, 0.55f, 0.25f, a)); for (int k = 0; k < 5; k++) Ln(dc, b, o + new Vector2(s * (290f + k * 18f) * u, 110f * u), o + new Vector2(s * (345f + k * 10f) * u, -75f * u + k * 22f * u), Rgb(0.12f, 0.45f, 0.22f, a), 6f * u); } Box(dc, b, o.X - 225f * u, o.Y + 8f * u, o.X + 225f * u, o.Y + 44f * u, C3(0.92f * a)); }
    }
}

public sealed class JoyaBellPlugin : QuickShapePlugin { public override string Name => "演出：除夜の鐘"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new JoyaBellParameter(s); }
public sealed class JoyaBellParameter : Q3Param
{
    public JoyaBellParameter() : this(null) { }
    public JoyaBellParameter(SharedDataStore? s) : base(s) { Count = 108; Color1 = Hex(0x8C7A4F); Color2 = Hex(0x0D1426); Color3 = Hex(0xFFE9A3); }
    protected override double DefAmount => 30;
    static readonly Dictionary<string, ParamGuide> g = G("除夜の鐘", "Count=鳴らす回数||1|108", "Amount=鳴らす間隔|0〜100 = 0.5〜5.5秒", "Background=夜空を塗る", "Color1=鐘の色", "Color2=空の色", "Color3=音の輪と数字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), iv = 0.5f + Amt(c) * 5f, ph = c.Time / iv; int rung = Math.Min(Math.Clamp(Count, 1, 108), (int)ph + 1); float f = ph - MathF.Floor(ph), swing = MathF.Sin(f * MathF.Tau) * 0.055f * (1f - f); var o = P0(c) - new Vector2(0, 150f * u); if (Background) Sky(dc, c.Width, c.Height, C2(a), Col(Gfx.Mix(Color2, Hex(0x3B3656), 0.7f), a));
        for (int i = 0; i < 3; i++) { float k = Gfx.Mod(f + i / 3f, 1f); Ring(dc, b, o + new Vector2(0, 225f * u), (170f + k * 680f) * u, C3(0.55f * a * (1f - k)), 5f * u); }
        Box(dc, b, o.X - 430f * u, o.Y - 215f * u, o.X + 430f * u, o.Y - 175f * u, Rgb(0.25f, 0.13f, 0.08f, a)); foreach (int s in new[] { -1, 1 }) Box(dc, b, o.X + s * 405f * u - 18f * u, o.Y - 180f * u, o.X + s * 405f * u + 18f * u, o.Y + 520f * u, Rgb(0.25f, 0.13f, 0.08f, a));
        var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(swing, o - new Vector2(0, 175f * u)) * prev; RBox(dc, b, o - new Vector2(0, 155f * u), 92f * u, 72f * u, 22f * u, C1(a)); Poly(dc, b, new List<Vector2> { o + new Vector2(-135f * u, -125f * u), o + new Vector2(135f * u, -125f * u), o + new Vector2(185f * u, 330f * u), o + new Vector2(-185f * u, 330f * u) }, C1(a)); Oval(dc, b, o + new Vector2(0, -122f * u), 135f * u, 38f * u, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.18f), a));
        foreach (float y in new[] { -35f, 90f, 220f }) { float hw = 142f + (y + 125f) * 0.1f; Ln(dc, b, o + new Vector2(-hw * u, y * u), o + new Vector2(hw * u, y * u), Rgb(0.28f, 0.22f, 0.12f, a), 5f * u); } for (int x = -2; x <= 2; x++) for (int y = 0; y < 3; y++) Dot(dc, b, o + new Vector2(x * 42f * u, (-100f + y * 25f) * u), 7f * u, Rgb(0.32f, 0.25f, 0.13f, a)); Dot(dc, b, o + new Vector2(62f * u, 165f * u), 34f * u, Rgb(0.33f, 0.26f, 0.14f, a)); dc.Transform = prev;
        Say(dc, b, rung + " / " + Math.Clamp(Count, 1, 108), 64f * u, o + new Vector2(0, 465f * u), C3(a), Rgb(0, 0, 0, 0.75f * a), 4f * u);
    }
}

public sealed class OmikujiPlugin : QuickShapePlugin { public override string Name => "演出：おみくじ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new OmikujiParameter(s); }
public sealed class OmikujiParameter : Q3Param
{
    public OmikujiParameter() : this(null) { }
    public OmikujiParameter(SharedDataStore? s) : base(s) { Text = "大吉"; Color1 = Hex(0xC8102E); Color2 = Hex(0xFFF9EC); Color3 = Hex(0xE6B422); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("おみくじ", "Text=結果", "Amount=開く時刻|0〜100 = 0〜5秒", "Color1=筒の色", "Color2=紙の色", "Color3=金の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, at = Amt(c) * 5f, shake = t < at ? MathF.Sin(t * 28f) * 0.13f : 0f, k = DrawKit.EaseOut(Sub(t, at, 0.35f)), open = DrawKit.EaseOutBack(Sub(t, at + 0.35f, 0.42f)); var o = P0(c) + new Vector2(-300f * u, 90f * u); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(shake + k * MathF.PI * 0.82f, o) * prev; RBox(dc, b, o - new Vector2(0, 280f * u + k * 90f * u), 34f * u, 180f * u, 5f * u, Rgb(0.87f, 0.74f, 0.5f, a)); RBox(dc, b, o, 190f * u, 500f * u, 18f * u, C1(a)); Say(dc, b, "御", 60f * u, o - new Vector2(0, 110f * u), C3(a)); Say(dc, b, "神", 60f * u, o, C3(a)); Say(dc, b, "籤", 60f * u, o + new Vector2(0, 110f * u), C3(a)); dc.Transform = prev;
        if (open <= 0f) return; var q = P0(c) + new Vector2(250f * u, 0); float W = 470f * u * open, H = 760f * u; RBox(dc, b, q, W, H, 6f * u, C2(a)); RFrame(dc, b, q, W - 26f * u, H - 26f * u, 3f * u, C1(a), 4f * u); if (open < 0.7f) return; Say(dc, b, "おみくじ", 38f * u, q - new Vector2(0, 300f * u), C1(a)); string s = Str; float fs = s.Length <= 2 ? 170f : 120f; for (int i = 0; i < s.Length && i < 4; i++) Say(dc, b, s[i].ToString(), fs * u, q + new Vector2(0, (i - (MathF.Min(s.Length, 4) - 1) / 2f) * (fs + 16f) * u + 50f * u), C1(a), scale: DrawKit.EaseOutBack(Sub(t, at + 0.78f + i * 0.1f, 0.26f)));
    }
}

public sealed class KakizomePlugin : QuickShapePlugin { public override string Name => "演出：書き初め"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new KakizomeParameter(s); }
public sealed class KakizomeParameter : Q3Param
{
    public KakizomeParameter() : this(null) { }
    public KakizomeParameter(SharedDataStore? s) : base(s) { Text = "初夢"; Color1 = Hex(0xFFFCF3); Color2 = Hex(0x111111); Color3 = Hex(0xC8102E); }
    protected override double DefAmount => 50;
    static readonly Dictionary<string, ParamGuide> g = G("書き初め", "Text=文字|1〜4文字くらい", "Amount=書く速さ", "Color1=紙の色", "Color2=墨の色", "Color3=落款の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        string s = Str.Replace("\n", ""); if (s.Length == 0) return; float u = U(c), H = 980f * u, W = 560f * u, fs = MathF.Min(330f * u, 860f * u / s.Length), per = 1.4f - Amt(c) * 1.05f; var o = P0(c); RBox(dc, b, o, W, H, 4f * u, C1(a)); Box(dc, b, o.X - W / 2f - 12f * u, o.Y - H / 2f - 10f * u, o.X + W / 2f + 12f * u, o.Y - H / 2f + 30f * u, Rgb(0.18f, 0.18f, 0.2f, a));
        for (int i = 0; i < s.Length; i++)
        {
            var q = new Vector2(o.X, o.Y - 50f * u + (i - (s.Length - 1) / 2f) * fs * 1.04f); float k = Sub(c.Time, 0.2f + i * per, per * 0.92f); if (k <= 0f) continue; float top = q.Y - fs * 0.6f, y1 = top + fs * 1.2f * k; dc.PushAxisAlignedClip(new Vortice.RawRectF(q.X - fs * 0.7f, top, q.X + fs * 0.7f, y1), AntialiasMode.Aliased); Say(dc, b, s[i].ToString(), fs, q, C2(a), bold: true); dc.PopAxisAlignedClip();
            if (k < 1f) { var brush = new Vector2(q.X + MathF.Sin(k * 22f) * fs * 0.38f, y1); Ln(dc, b, brush, brush + new Vector2(90f * u, -300f * u), Rgb(0.55f, 0.32f, 0.15f, a), 18f * u); Poly(dc, b, new List<Vector2> { brush, brush + new Vector2(30f * u, -66f * u), brush + new Vector2(-6f * u, -76f * u) }, C2(a)); }
            for (int d = 0; d < 7 && k >= 1f; d++) Dot(dc, b, q + new Vector2((Rnd(i * 9 + d) - 0.5f) * fs * 1.1f, (Rnd(i * 13 + d) - 0.5f) * fs), (1.5f + Rnd(d + 40) * 3f) * u, C2(0.75f * a));
        }
        if (c.Time > 0.25f + s.Length * per) { var st = new Vector2(o.X + 190f * u, o.Y + 400f * u); RBox(dc, b, st, 72f * u, 72f * u, 5f * u, C3(a)); Say(dc, b, "印", 42f * u, st, C1(a)); }
    }
}

public sealed class DarumaEyesPlugin : QuickShapePlugin { public override string Name => "演出：だるま（目を入れる）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DarumaEyesParameter(s); }
public sealed class DarumaEyesParameter : Q3Param
{
    public DarumaEyesParameter() : this(null) { }
    public DarumaEyesParameter(SharedDataStore? s) : base(s) { Text = "必勝"; Count = 2; Color1 = Hex(0xD7192D); Color2 = Hex(0xFFE9D0); Color3 = Hex(0xE6B422); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("だるま", "Text=お腹の文字", "Count=目の数|0〜2|0|2", "Amount=両目が入る時刻|0〜100 = 0〜5秒", "Color1=体の色", "Color2=顔の色", "Color3=金の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), wob = MathF.Sin(c.Time * 3.2f) * 0.07f * MathF.Exp(-c.Time * 0.45f), R = 345f * u * DrawKit.EaseOutBack(Intro(c, 0f, 0.45f)); var o = P0(c); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(wob, o + new Vector2(0, R)) * prev; Oval(dc, b, o, R * 0.92f, R, C1(a)); Oval(dc, b, o - new Vector2(0, R * 0.34f), R * 0.56f, R * 0.46f, C2(a)); foreach (int s in new[] { -1, 1 }) { Path(dc, b, new List<Vector2> { o + new Vector2(s * R * 0.48f, -R * 0.62f), o + new Vector2(s * R * 0.25f, -R * 0.67f), o + new Vector2(s * R * 0.08f, -R * 0.55f) }, Rgb(0.08f, 0.07f, 0.07f, a), 15f * u); Path(dc, b, new List<Vector2> { o + new Vector2(s * R * 0.08f, -R * 0.12f), o + new Vector2(s * R * 0.25f, -R * 0.08f), o + new Vector2(s * R * 0.42f, -R * 0.18f) }, Rgb(0.08f, 0.07f, 0.07f, a), 10f * u); }
        int eyes = Math.Clamp(Count, 0, 2); float at = Amt(c) * 5f; foreach (int s in new[] { -1, 1 }) { var e = o + new Vector2(s * R * 0.24f, -R * 0.38f); Dot(dc, b, e, R * 0.125f, Rgb(1, 1, 1, a)); Ring(dc, b, e, R * 0.125f, Rgb(0.08f, 0.07f, 0.07f, a), 3f * u); bool on = s < 0 ? eyes >= 1 : eyes >= 2 && c.Time > at; float k = s < 0 ? 1f : DrawKit.EaseOutBack(Sub(c.Time, at, 0.2f)); if (on) Dot(dc, b, e, R * 0.075f * k, Rgb(0.04f, 0.04f, 0.05f, a)); }
        Say(dc, b, Str, MathF.Min(110f * u, R * 0.33f), o + new Vector2(0, R * 0.5f), C3(a), Rgb(0.35f, 0.04f, 0.05f, a), 3f * u, maxW: R * 1.25f); dc.Transform = prev;
    }
}

public sealed class LuckyCatPlugin : QuickShapePlugin { public override string Name => "演出：招き猫"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LuckyCatParameter(s); }
public sealed class LuckyCatParameter : Q3Param
{
    public LuckyCatParameter() : this(null) { }
    public LuckyCatParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFDF7); Color2 = Hex(0xD7192D); Color3 = Hex(0xE6B422); }
    static readonly Dictionary<string, ParamGuide> g = G("招き猫", "Variant=上げる手|1=右手　2=左手|1|2", "Speed=動く速さ", "Color1=体の色", "Color2=首輪の色", "Color3=小判の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), side = Variant == 2 ? -1f : 1f, wave = MathF.Sin(t * 3.6f); var o = P0(c); Oval(dc, b, o + new Vector2(0, 190f * u), 245f * u, 240f * u, C1(a)); Dot(dc, b, o - new Vector2(0, 165f * u), 205f * u, C1(a)); foreach (int s in new[] { -1, 1 }) { Poly(dc, b, new List<Vector2> { o + new Vector2(s * 175f * u, -270f * u), o + new Vector2(s * 190f * u, -430f * u), o + new Vector2(s * 55f * u, -350f * u) }, C1(a)); Poly(dc, b, new List<Vector2> { o + new Vector2(s * 160f * u, -295f * u), o + new Vector2(s * 172f * u, -392f * u), o + new Vector2(s * 88f * u, -340f * u) }, Rgb(1, 0.65f, 0.7f, a)); Arc(dc, b, o + new Vector2(s * 82f * u, -180f * u), 34f * u, MathF.PI * 1.1f, MathF.PI * 1.9f, Rgb(0.1f, 0.09f, 0.09f, a), 9f * u); for (int k = -1; k <= 1; k++) Ln(dc, b, o + new Vector2(s * 88f * u, (-108f + k * 12f) * u), o + new Vector2(s * 205f * u, (-122f + k * 32f) * u), Rgb(0.2f, 0.2f, 0.2f, a), 3f * u); }
        Poly(dc, b, new List<Vector2> { o + new Vector2(-14f * u, -130f * u), o + new Vector2(14f * u, -130f * u), o + new Vector2(0, -113f * u) }, Rgb(1, 0.45f, 0.5f, a)); Path(dc, b, new List<Vector2> { o + new Vector2(-34f * u, -92f * u), o + new Vector2(-14f * u, -80f * u), o + new Vector2(0, -105f * u), o + new Vector2(14f * u, -80f * u), o + new Vector2(34f * u, -92f * u) }, Rgb(0.1f, 0.09f, 0.09f, a), 5f * u); Ln(dc, b, o + new Vector2(-170f * u, -2f * u), o + new Vector2(170f * u, -2f * u), C2(a), 34f * u); Dot(dc, b, o + new Vector2(0, 24f * u), 28f * u, C3(a));
        var koban = o + new Vector2(0, 225f * u); float shine = 0.75f + 0.25f * MathF.Sin(t * 2.5f); Oval(dc, b, koban, 120f * u, 165f * u, Col(Gfx.Mix(Color3, Hex(0xFFFFFF), shine * 0.18f), a)); b.Color = Rgb(0.65f, 0.42f, 0.05f, a); dc.DrawEllipse(new Ellipse(koban, 105f * u, 150f * u), b, 4f * u); Say(dc, b, "千", 60f * u, koban - new Vector2(0, 62f * u), Rgb(0.45f, 0.26f, 0.03f, a)); Say(dc, b, "万", 60f * u, koban + new Vector2(0, 8f * u), Rgb(0.45f, 0.26f, 0.03f, a)); Say(dc, b, "両", 60f * u, koban + new Vector2(0, 78f * u), Rgb(0.45f, 0.26f, 0.03f, a));
        var sh = o + new Vector2(side * 170f * u, 40f * u); var hand = sh + new Vector2(side * (92f + wave * 24f) * u, -(210f + wave * 52f) * u); Ln(dc, b, sh, hand, C1(a), 88f * u); Dot(dc, b, sh, 44f * u, C1(a)); Dot(dc, b, hand, 66f * u, C1(a)); Dot(dc, b, hand + new Vector2(0, 10f * u), 25f * u, Rgb(1, 0.65f, 0.7f, a)); for (int i = -1; i <= 1; i++) Dot(dc, b, hand + new Vector2(i * 24f * u, -28f * u), 10f * u, Rgb(1, 0.65f, 0.7f, a)); var other = o + new Vector2(-side * 150f * u, 100f * u); Oval(dc, b, other, 62f * u, 118f * u, C1(a));
    }
}

public sealed class HanafudaCardsPlugin : QuickShapePlugin { public override string Name => "演出：花札"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HanafudaCardsParameter(s); }
public sealed class HanafudaCardsParameter : Q3Param
{
    public HanafudaCardsParameter() : this(null) { }
    public HanafudaCardsParameter(SharedDataStore? s) : base(s) { Text = "松,桜,藤,菊,紅葉"; Color1 = Hex(0x8B1E1E); Color2 = Hex(0xFFF7E3); Color3 = Hex(0x20242A); }
    static readonly Dictionary<string, ParamGuide> g = G("花札", "Text=札の種類|カンマ区切り。松 梅 桜 藤 菖蒲 牡丹 萩 芒 菊 紅葉 柳 桐", "Variant=並べ方|1=横一列　2=扇形|1|2", "Color1=裏の色", "Color2=札の地色", "Color3=ふちの色");
    static readonly Dictionary<string, uint> cols = new() { ["松"] = 0x1F7A3A, ["梅"] = 0xD7263D, ["桜"] = 0xFF8FB1, ["藤"] = 0x7D5BBE, ["菖蒲"] = 0x4B4FC4, ["牡丹"] = 0xD81B60, ["萩"] = 0xB23A68, ["芒"] = 0xC9A227, ["菊"] = 0xE6B422, ["紅葉"] = 0xE5541B, ["柳"] = 0x6A9E2F, ["桐"] = 0x7E57C2 };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Items.Take(12).ToList(); if (r.Count == 0) return; float u = U(c), W = 230f * u, H = 370f * u, step = MathF.Min(280f * u, 1500f * u / r.Count); var o = P0(c);
        for (int i = 0; i < r.Count; i++)
        {
            float off = i - (r.Count - 1) / 2f, flip = DrawKit.EaseOut(Sub(c.Time, 0.25f + i * 0.22f, 0.45f)), sx = MathF.Max(0.02f, MathF.Abs(MathF.Cos(flip * MathF.PI))), rot = Variant == 2 ? off * 0.14f : 0f; var q = Variant == 2 ? o + new Vector2(off * step * 0.76f, MathF.Abs(off) * MathF.Abs(off) * 11f * u) : o + new Vector2(off * step, 0); bool front = flip > 0.5f; var prev = dc.Transform; dc.Transform = Matrix3x2.CreateScale(sx, 1f, q) * Matrix3x2.CreateRotation(rot, q) * prev; RBox(dc, b, q, W, H, 12f * u, front ? C2(a) : C1(a)); RFrame(dc, b, q, W, H, 12f * u, C3(a), 5f * u);
            if (!front) { for (int k = 0; k < 5; k++) RFrame(dc, b, q, W - k * 30f * u, H - k * 30f * u, 6f * u, Rgb(1, 0.85f, 0.55f, 0.45f * a), 2f * u); }
            else { var col = Col(Hex(cols.TryGetValue(r[i], out var hx) ? hx : 0xD7263Du), a); Box(dc, b, q.X - W / 2f + 8f * u, q.Y + H * 0.15f, q.X + W / 2f - 8f * u, q.Y + H / 2f - 8f * u, Rgb(0.13f, 0.15f, 0.2f, a)); Ln(dc, b, q + new Vector2(0, H * 0.32f), q + new Vector2(0, -H * 0.16f), Rgb(0.34f, 0.2f, 0.1f, a), 11f * u); for (int k = 0; k < 7; k++) { var f = q + new Vector2(MathF.Cos(k * 2.4f) * (18f + k * 8f) * u, -H * 0.22f + MathF.Sin(k * 2.4f) * (14f + k * 7f) * u); for (int p = 0; p < 5; p++) Dot(dc, b, Pol(f, 15f * u, p * MathF.Tau / 5f), 12f * u, col); Dot(dc, b, f, 6f * u, Rgb(1, 0.9f, 0.45f, a)); } Say(dc, b, r[i], 42f * u, q + new Vector2(0, H * 0.32f), C2(a), maxW: W - 24f * u); }
            dc.Transform = prev;
        }
    }
}

public sealed class WagasaRainPlugin : QuickShapePlugin { public override string Name => "演出：和傘と雨"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WagasaRainParameter(s); }
public sealed class WagasaRainParameter : Q3Param
{
    public WagasaRainParameter() : this(null) { }
    public WagasaRainParameter(SharedDataStore? s) : base(s) { Count = 90; Color1 = Hex(0xC8102E); Color2 = Hex(0xBFE3FF); Color3 = Hex(0xFFF3D6); }
    static readonly Dictionary<string, ParamGuide> g = G("和傘と雨", "Speed=回る速さ", "Count=雨の量||0|400", "Color1=傘の色", "Color2=雨の色", "Color3=骨の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        const int ribs = 16; float u = U(c), t = c.Time, R = 400f * u, sq = 0.42f, rot = t * Spd(c) * 0.75f; var o = P0(c) - new Vector2(0, 80f * u); Vector2 Rim(float th) => o + new Vector2(MathF.Cos(th) * R, MathF.Sin(th) * R * sq + 90f * u);
        for (int i = 0; i < Math.Clamp(Count, 0, 400); i++) { float x = Rnd(i) * c.Width, life = 0.45f + Rnd(i + 2) * 0.25f, ph = Gfx.Mod(t / life + Rnd(i + 5), 1f), dx = x - o.X, topY = MathF.Abs(dx) < R ? o.Y + 90f * u - MathF.Sqrt(MathF.Max(0f, 1f - dx * dx / (R * R))) * R * sq - 75f * u * (1f - MathF.Abs(dx) / R) : c.Height, y = ph * c.Height; if (y < topY) Ln(dc, b, new(x, y), new(x - 5f * u, y + 42f * u), C2(0.58f * a), 2.5f * u); else if (topY < c.Height && ph * c.Height < topY + 60f * u) { float e = (y - topY) / (60f * u); Arc(dc, b, new(x, topY), (6f + e * 25f) * u, MathF.PI * 1.1f, MathF.PI * 1.9f, C2(a * (1f - e)), 2f * u); } }
        Ln(dc, b, o, o + new Vector2(0, 590f * u), Rgb(0.48f, 0.28f, 0.14f, a), 15f * u); var apex = o - new Vector2(0, 92f * u); for (int i = 0; i < ribs; i++) { float a0 = rot + i * MathF.Tau / ribs, a1 = a0 + MathF.Tau / ribs; if (MathF.Sin((a0 + a1) / 2f) < -0.05f) continue; var col = i % 4 == 0 ? Col(Gfx.Mix(Color1, Color3, 0.72f), a) : Col(Gfx.Mix(Color1, Hex(0x000000), i % 2 == 0 ? 0.12f : 0f), a); Poly(dc, b, new List<Vector2> { apex, Rim(a0), Rim(a1) }, col); Ln(dc, b, apex, Rim(a0), C3(0.8f * a), 2.5f * u); }
        var rim = new List<Vector2>(); for (int i = 0; i <= 70; i++) rim.Add(Rim(i / 70f * MathF.PI)); Path(dc, b, rim, Rgb(0.2f, 0.05f, 0.05f, a), 7f * u); Dot(dc, b, apex, 17f * u, Rgb(0.13f, 0.1f, 0.08f, a));
    }
}

public sealed class TeaRoomPlugin : QuickShapePlugin { public override string Name => "演出：茶室と抹茶"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TeaRoomParameter(s); }
public sealed class TeaRoomParameter : Q3Param
{
    public TeaRoomParameter() : this(null) { }
    public TeaRoomParameter(SharedDataStore? s) : base(s) { Count = 4; Color1 = Hex(0xB9BE7D); Color2 = Hex(0x2E6F43); Color3 = Hex(0xFFF7DF); }
    static readonly Dictionary<string, ParamGuide> g = G("茶室", "Count=湯気の量||0|10", "Variant=時間帯|1=昼　2=夕方　3=夜|1|3", "Color1=畳の色", "Color2=抹茶の色", "Color3=障子の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, floor = h * 0.66f, dim = Variant == 3 ? 0.42f : Variant == 2 ? 0.75f : 1f; var sh = Variant == 2 ? Hex(0xFFD08A) : Variant == 3 ? Hex(0x5A6C95) : Color3; Box(dc, b, 0, 0, w, floor, Rgb(0.48f * dim, 0.36f * dim, 0.24f * dim, a)); Box(dc, b, 0, floor, w, h, Col(Gfx.Mix(Hex(0x000000), Color1, dim), a));
        for (int i = 0; i < 4; i++) { float x0 = w * 0.08f + i * w * 0.21f, x1 = x0 + w * 0.2f, y0 = h * 0.08f, y1 = floor - 8f * u; Box(dc, b, x0, y0, x1, y1, Col(sh, a)); for (int k = 1; k < 4; k++) Ln(dc, b, new(x0 + (x1 - x0) * k / 4f, y0), new(x0 + (x1 - x0) * k / 4f, y1), Rgb(0.3f * dim, 0.2f * dim, 0.12f * dim, a), 3f * u); for (int k = 1; k < 6; k++) Ln(dc, b, new(x0, y0 + (y1 - y0) * k / 6f), new(x1, y0 + (y1 - y0) * k / 6f), Rgb(0.3f * dim, 0.2f * dim, 0.12f * dim, a), 3f * u); Frame(dc, b, x0, y0, x1, y1, Rgb(0.24f * dim, 0.16f * dim, 0.1f * dim, a), 9f * u); }
        for (int i = 1; i < 5; i++) Ln(dc, b, new(i / 5f * w, floor), new((i / 5f - 0.5f) * 1.6f * w + w / 2f, h), Rgb(0.2f, 0.25f, 0.1f, 0.65f * a), 4f * u); Ln(dc, b, new(0, floor + (h - floor) * 0.48f), new(w, floor + (h - floor) * 0.48f), Rgb(0.2f, 0.25f, 0.1f, 0.65f * a), 4f * u); var o = new Vector2(w / 2f, h * 0.76f); Oval(dc, b, o + new Vector2(0, 76f * u), 200f * u, 34f * u, Rgb(0, 0, 0, 0.28f * a)); Pie(dc, b, o - new Vector2(0, 20f * u), 170f * u, 0f, MathF.PI, Rgb(0.2f, 0.15f, 0.13f, a)); Oval(dc, b, o - new Vector2(0, 20f * u), 170f * u, 38f * u, Rgb(0.28f, 0.21f, 0.17f, a)); Oval(dc, b, o - new Vector2(0, 18f * u), 148f * u, 28f * u, C2(a)); Oval(dc, b, o - new Vector2(30f * u, 21f * u), 62f * u, 10f * u, Rgb(0.55f, 0.75f, 0.42f, 0.72f * a));
        for (int s = 0; s < Math.Clamp(Count, 0, 10); s++) { var pts = new List<Vector2>(); float x = (s - (Count - 1) / 2f) * 38f * u; for (int i = 0; i <= 26; i++) { float k = i / 26f; pts.Add(o + new Vector2(x + MathF.Sin(k * 7f - t * 1.5f + s) * 22f * u, -35f * u - k * 250f * u)); } Path(dc, b, pts, Rgb(1, 1, 1, 0.34f * a), 7f * u); }
    }
}

public sealed class CrestSymbolPlugin : QuickShapePlugin { public override string Name => "演出：家紋風のシンボル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CrestSymbolParameter(s); }
public sealed class CrestSymbolParameter : Q3Param
{
    public CrestSymbolParameter() : this(null) { }
    public CrestSymbolParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x16181D); Color2 = Hex(0xE6B422); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("家紋風", "Variant=模様の種類|1=花　2=三つ葉　3=波　4=菱|1|4", "Amount=線の太さ", "Background=地を塗る", "Color1=地の色", "Color2=模様の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), R = 330f * u * DrawKit.EaseOutBack(Intro(c, 0f, 0.5f)), lw = (5f + Amt(c) * 32f) * u; var o = P0(c); var ink = C2(a); if (Background) Dot(dc, b, o, R, C1(a)); Ring(dc, b, o, R - lw / 2f, ink, lw);
        switch (Variant)
        {
            case 2: for (int i = 0; i < 3; i++) { float th = -MathF.PI / 2f + i * MathF.Tau / 3f; var q = Pol(o, R * 0.36f, th); var leaf = new List<Vector2>(); for (int k = 0; k <= 22; k++) { float e = k / 22f, off = MathF.Sin(e * MathF.PI) * R * 0.22f; leaf.Add(Vector2.Lerp(o, Pol(o, R * 0.76f, th), e) + new Vector2(-MathF.Sin(th), MathF.Cos(th)) * off); } for (int k = 22; k >= 0; k--) { float e = k / 22f, off = MathF.Sin(e * MathF.PI) * R * 0.22f; leaf.Add(Vector2.Lerp(o, Pol(o, R * 0.76f, th), e) - new Vector2(-MathF.Sin(th), MathF.Cos(th)) * off); } Poly(dc, b, leaf, ink); Ln(dc, b, o, Pol(o, R * 0.66f, th), C1(a), lw * 0.3f); } break;
            case 3: for (int l = 0; l < 3; l++) { float y = o.Y - R * 0.34f + l * R * 0.34f; for (int i = -1; i <= 1; i++) { float x = o.X + i * R * 0.46f + (l % 2) * R * 0.23f; if (MathF.Abs(x - o.X) > R * 0.66f) continue; Arc(dc, b, new(x, y + R * 0.1f), R * 0.23f, MathF.PI, MathF.Tau, ink, lw * 0.8f); Arc(dc, b, new(x, y + R * 0.1f), R * 0.1f, MathF.PI, MathF.Tau, ink, lw * 0.6f); } } break;
            case 4: for (int i = -1; i <= 1; i += 2) for (int k = 0; k < 2; k++) { var q = k == 0 ? o + new Vector2(i * R * 0.31f, 0) : o + new Vector2(0, i * R * 0.31f); Poly(dc, b, new List<Vector2> { q - new Vector2(0, R * 0.27f), q + new Vector2(R * 0.27f, 0), q + new Vector2(0, R * 0.27f), q - new Vector2(R * 0.27f, 0) }, ink); } break;
            default: for (int i = 0; i < 8; i++) { float th = i * MathF.PI / 4f; var q = Pol(o, R * 0.45f, th); Oval(dc, b, q, R * 0.15f, R * 0.15f, ink); Ln(dc, b, o, q, ink, lw * 0.34f); } Dot(dc, b, o, R * 0.19f, ink); Dot(dc, b, o, R * 0.09f, Background ? C1(a) : Rgb(0, 0, 0, 0)); break;
        }
    }
}

public sealed class KabukiMiePlugin : QuickShapePlugin { public override string Name => "演出：歌舞伎の見得"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new KabukiMieParameter(s); }
public sealed class KabukiMieParameter : Q3Param
{
    public KabukiMieParameter() : this(null) { }
    public KabukiMieParameter(SharedDataStore? s) : base(s) { Text = "カンッ！"; Color1 = Hex(0xE11B22); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("歌舞伎の見得", "Text=拍子木の文字", "Background=幕を塗る", "Color1=隈取りの色", "Color2=顔と文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), t = c.Time, hit = DrawKit.EaseOutBack(Sub(t, 0.18f, 0.22f)); uint[] mk = [0x111111, 0xB5651D, 0x2E7D32]; var o = P0(c);
        if (Background) for (int i = 0; i < 9; i++) { float x0 = i * w / 9f; Box(dc, b, x0, 0, x0 + w / 9f + 1f, h, Col(Hex(mk[i % 3]), a)); }
        Oval(dc, b, o, 245f * u * hit, 330f * u * hit, C2(a)); foreach (int s in new[] { -1, 1 })
        {
            Path(dc, b, new List<Vector2> { o + new Vector2(s * 25f * u, -35f * u), o + new Vector2(s * 90f * u, -105f * u), o + new Vector2(s * 180f * u, -190f * u), o + new Vector2(s * 232f * u, -300f * u) }, C1(a * hit), 20f * u); Path(dc, b, new List<Vector2> { o + new Vector2(s * 58f * u, 30f * u), o + new Vector2(s * 145f * u, -8f * u), o + new Vector2(s * 225f * u, -105f * u) }, C1(a * hit), 15f * u); Path(dc, b, new List<Vector2> { o + new Vector2(s * 52f * u, 148f * u), o + new Vector2(s * 135f * u, 112f * u), o + new Vector2(s * 205f * u, 28f * u) }, C1(a * hit), 11f * u);
            var e = o + new Vector2(s * 90f * u, -50f * u); Oval(dc, b, e, 48f * u, 19f * u, Rgb(0.05f, 0.05f, 0.05f, a * hit)); Oval(dc, b, e, 38f * u, 13f * u, C2(a * hit)); Dot(dc, b, e + new Vector2(-s * 9f * u, 0), 10f * u, Rgb(0.03f, 0.03f, 0.03f, a * hit)); Path(dc, b, new List<Vector2> { o + new Vector2(s * 35f * u, -125f * u), o + new Vector2(s * 105f * u, -155f * u), o + new Vector2(s * 170f * u, -130f * u) }, Rgb(0.05f, 0.05f, 0.05f, a * hit), 16f * u);
        }
        Path(dc, b, new List<Vector2> { o + new Vector2(-82f * u, 205f * u), o + new Vector2(-28f * u, 188f * u), o + new Vector2(28f * u, 188f * u), o + new Vector2(82f * u, 205f * u) }, C1(a * hit), 13f * u); Ln(dc, b, o + new Vector2(0, -35f * u), o + new Vector2(0, 112f * u), Rgb(0.8f, 0.75f, 0.72f, a * hit), 4f * u); float k = Sub(t, 0f, 0.18f) * (1f - Sub(t, 0.75f, 0.35f)); if (k > 0f) { for (int i = 0; i < 24; i++) { float th = i / 24f * MathF.Tau; Ln(dc, b, Pol(o, 400f * u, th), Pol(o, (500f + (i % 2) * 90f) * u, th), C2(a * k), 7f * u); } Say(dc, b, Str, 118f * u, o + new Vector2(0, -395f * u), C2(a * k), Rgb(0, 0, 0, a * k), 7f * u); }
    }
}

public sealed class FoldingFanPlugin : QuickShapePlugin { public override string Name => "演出：扇子が開く"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FoldingFanParameter(s); }
public sealed class FoldingFanParameter : Q3Param
{
    public FoldingFanParameter() : this(null) { }
    public FoldingFanParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xC8102E); Color2 = Hex(0xFFF3D6); Color3 = Hex(0xE6B422); }
    protected override double DefAmount => 75;
    static readonly Dictionary<string, ParamGuide> g = G("扇子", "Variant=模様|1=波　2=桜　3=日の丸　4=無地|1|4", "Amount=開く角度", "Color1=紙の色", "Color2=模様の色", "Color3=骨と金の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        const int folds = 22; float u = U(c), R = 610f * u, inR = 190f * u, open = DrawKit.EaseOutBack(Intro(c, 0.05f, 0.55f)) * (0.3f + Amt(c) * 2.75f) / 2f; var o = P0(c) + new Vector2(0, 280f * u); float Ang(int i) => -MathF.PI / 2f - open + i * (open * 2f / folds);
        for (int i = 0; i < folds; i++) { float a0 = Ang(i), a1 = Ang(i + 1); var col = i % 2 == 0 ? C1(a) : Col(Gfx.Mix(Color1, Hex(0x000000), 0.14f), a); Poly(dc, b, new List<Vector2> { Pol(o, inR, a0), Pol(o, R, a0), Pol(o, R, a1), Pol(o, inR, a1) }, col); Ln(dc, b, o, Pol(o, inR, a0), Rgb(0.34f, 0.2f, 0.1f, a), 7f * u); }
        Ln(dc, b, o, Pol(o, R, Ang(0)), C3(a), 11f * u); Ln(dc, b, o, Pol(o, R, Ang(folds)), C3(a), 11f * u); Arc(dc, b, o, R, Ang(0), Ang(folds), C3(a), 7f * u); Arc(dc, b, o, inR, Ang(0), Ang(folds), C3(a), 4f * u); if (open > 0.35f)
        {
            float rm = (R + inR) / 2f; if (Variant == 3) Dot(dc, b, Pol(o, rm, -MathF.PI / 2f), 105f * u, C2(a));
            else if (Variant == 2) for (int i = 0; i < 16; i++) { var q = Pol(o, inR + (R - inR) * (0.16f + Rnd(i) * 0.7f), -MathF.PI / 2f - open * 0.86f + open * 1.72f * Rnd(i + 8)); for (int p = 0; p < 5; p++) Dot(dc, b, Pol(q, 13f * u, p * MathF.Tau / 5f + i), 10f * u, C2(0.95f * a)); Dot(dc, b, q, 5f * u, C3(a)); }
            else if (Variant == 1) for (int l = 0; l < 3; l++) { float rr = inR + (R - inR) * (0.28f + l * 0.25f); int n = 7 + l * 3; for (int i = 0; i < n; i++) { float th = Lerp(Ang(0) + 0.1f, Ang(folds) - 0.1f, (i + 0.5f) / n); Arc(dc, b, Pol(o, rr, th), 42f * u, th - 1.45f, th + 1.45f, C2(a), 5f * u); } }
        }
        Dot(dc, b, o, 19f * u, C3(a)); Dot(dc, b, o, 7f * u, Rgb(0.2f, 0.12f, 0.06f, a));
    }
}

public sealed class SantaMoonSleighPlugin : QuickShapePlugin { public override string Name => "演出：サンタのソリが月を横切る"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SantaMoonSleighParameter(s); }
public sealed class SantaMoonSleighParameter : Q3Param
{
    public SantaMoonSleighParameter() : this(null) { }
    public SantaMoonSleighParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFF0B8); Color2 = Hex(0x0B1630); Color3 = Hex(0x050608); }
    static readonly Dictionary<string, ParamGuide> g = G("サンタのソリ", "Variant=向き|1=右へ　2=左へ|1|2", "Speed=速さ", "Background=夜空を塗る", "Color1=月の色", "Color2=空の色", "Color3=影の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), t = c.Time * Spd(c), dir = Variant == 2 ? -1f : 1f; var moon = P0(c) - new Vector2(0, 40f * u); if (Background) { Sky(dc, w, h, C2(a), Col(Gfx.Mix(Color2, Hex(0x3A5180), 0.7f), a)); for (int i = 0; i < 90; i++) Dot(dc, b, new(Rnd(i) * w, Rnd(i + 200) * h * 0.82f), (0.8f + Rnd(i + 400) * 1.8f) * u, Rgb(1, 1, 1, (0.35f + 0.65f * MathF.Abs(MathF.Sin(t * 0.8f + i))) * a)); }
        Dot(dc, b, moon, 390f * u, C1(0.12f * a)); Dot(dc, b, moon, 320f * u, C1(a)); foreach (var (x, y, r) in new[] { (-120f, -70f, 56f), (95f, 70f, 76f), (55f, -140f, 34f), (-70f, 165f, 42f) }) Dot(dc, b, moon + new Vector2(x, y) * u, r * u, Rgb(0.87f, 0.77f, 0.5f, 0.42f * a));
        float span = w + 1500f * u, x0 = Gfx.Mod(t * 170f * u + 450f * u, span) - 750f * u; if (dir < 0f) x0 = w - x0; var q = new Vector2(x0, moon.Y + MathF.Sin(x0 * 0.005f) * 70f * u); var ink = C3(a); Vector2 P(float x, float y) => q + new Vector2(dir * x, y) * u;
        Poly(dc, b, new List<Vector2> { P(-210, -30), P(-65, -30), P(-40, 25), P(-175, 25) }, ink); Path(dc, b, new List<Vector2> { P(-230, 48), P(-30, 48), P(5, 25) }, ink, 7f * u); Ln(dc, b, P(-170, 25), P(-170, 48), ink, 6f * u); Ln(dc, b, P(-70, 25), P(-70, 48), ink, 6f * u); Dot(dc, b, P(-145, -62), 25f * u, ink); Poly(dc, b, new List<Vector2> { P(-170, -75), P(-122, -75), P(-160, -130) }, ink); Dot(dc, b, P(-164, -132), 8f * u, ink); Dot(dc, b, P(-95, -55), 32f * u, ink);
        for (int d = 0; d < 3; d++) { float x = 75f + d * 150f, gait = MathF.Sin(t * 8f + d * 1.2f) * 13f; Ln(dc, b, P(x - 150f, -8f), P(x - 25f, -12f), ink, 2f * u); Oval(dc, b, P(x, -18), 52f * u, 22f * u, ink); Ln(dc, b, P(x + 42, -30), P(x + 66, -68), ink, 12f * u); Oval(dc, b, P(x + 78, -76), 22f * u, 12f * u, ink); foreach (int s in new[] { -1, 1 }) { Ln(dc, b, P(x + s * 32, -5), P(x + s * 42 + gait * s, 42), ink, 6f * u); Ln(dc, b, P(x + 66, -84), P(x + 52 + s * 18, -124), ink, 4f * u); Ln(dc, b, P(x + 57 + s * 8, -105), P(x + 40 + s * 30, -112), ink, 3f * u); } }
    }
}
