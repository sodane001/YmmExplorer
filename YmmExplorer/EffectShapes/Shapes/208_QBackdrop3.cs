using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using WpfColor = System.Windows.Media.Color;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 571〜590: 背景・場所(どれも 昼・夕方・夜 を切り替えられ、「もや」で文字を読みやすくできる)

public abstract class Q3BackParam : Q3Param
{
    protected Q3BackParam(SharedDataStore? s) : base(s) { }
    protected override double DefAmount2 => 0;
    protected const string Tod = "Variant=時間帯|1=昼　2=夕方　3=夜|1|3";
    protected const string Veil = "Amount2=もや(文字を読みやすく)|背景を少し白っぽく、または暗くします";
    protected float Dim => Variant == 3 ? 0.42f : Variant == 2 ? 0.78f : 1f;
    protected Color4 L(uint rgb, float a)
    {
        var col = Hex(rgb); float d = Dim; if (Variant == 2) col = Gfx.Mix(col, Hex(0xFF9A52), 0.13f); if (Variant == 3) col = Gfx.Mix(col, Hex(0x20366E), 0.25f);
        return new Color4(col.R / 255f * d, col.G / 255f * d, col.B / 255f * d, a);
    }
    protected Color4 L(int rgb, float a) => L((uint)rgb, a);
    protected (Color4 top, Color4 low) SkyCols(float a) => Variant switch
    {
        2 => (Col(Hex(0x6B4F9B), a), Col(Hex(0xFFB066), a)),
        3 => (Col(Hex(0x07102A), a), Col(Hex(0x25345F), a)),
        _ => (Col(Hex(0x4BA8EC), a), Col(Hex(0xCFEFFF), a)),
    };
    protected void SkyFill(ID2D1DeviceContext dc, float w, float h, float a) { var s = SkyCols(a); Shade(dc, new(0, 0), new(0, h), s.top, s.low, 0, 0, w, h); }
    protected void Stars(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a) { if (Variant != 3) return; for (int i = 0; i < 90; i++) Dot(dc, b, new(Rnd(i) * w, Rnd(i + 200) * h), (0.8f + Rnd(i + 400) * 1.6f) * u, Rgb(1, 1, 1, (0.35f + 0.65f * MathF.Abs(MathF.Sin(t * 0.7f + i))) * a)); }
    protected void Clouds(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float speed = 12f) { var col = Variant == 3 ? Rgb(0.35f, 0.4f, 0.55f, 0.35f * a) : Variant == 2 ? Rgb(1, 0.82f, 0.72f, 0.85f * a) : Rgb(1, 1, 1, 0.92f * a); for (int i = 0; i < 6; i++) { float span = w + 500f * u, x = Gfx.Mod(Rnd(i) * span + t * speed * (0.7f + Rnd(i + 3)) * u, span) - 250f * u, y = h * (0.07f + Rnd(i + 5) * 0.3f); for (int k = 0; k < 5; k++) Dot(dc, b, new(x + k * 46f * u, y + MathF.Sin(k * 2f) * 10f * u), (40f - MathF.Abs(k - 2f) * 8f) * u, col); } }
    protected abstract void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt);
    protected sealed override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height; Scene(dc, b, w, h, U0(c), c.Time * Spd(c), a, Amt(c)); float v = Amt2(c); if (v > 0.001f) Box(dc, b, 0, 0, w, h, Variant == 3 ? Rgb(0.02f, 0.03f, 0.08f, v * 0.75f * a) : Rgb(1, 1, 1, v * 0.68f * a));
    }
}

public sealed class LibraryBackPlugin : QuickShapePlugin { public override string Name => "演出：図書館"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LibraryBackParameter(s); }
public sealed class LibraryBackParameter : Q3BackParam
{
    public LibraryBackParameter() : this(null) { }
    public LibraryBackParameter(SharedDataStore? s) : base(s) { }
    static readonly Dictionary<string, ParamGuide> g = G("図書館", Tod, Veil);
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        Box(dc, b, 0, 0, w, h, L(0x8A6848, a)); Box(dc, b, 0, h * 0.8f, w, h, L(0x5E432D, a)); float wx = w * 0.42f, wy = h * 0.08f, ww = w * 0.16f, wh = h * 0.42f; var sk = SkyCols(a); Shade(dc, new(0, wy), new(0, wy + wh), sk.top, sk.low, wx, wy, wx + ww, wy + wh); Frame(dc, b, wx, wy, wx + ww, wy + wh, L(0x3D2A1B, a), 9f * u); Ln(dc, b, new(wx + ww / 2f, wy), new(wx + ww / 2f, wy + wh), L(0x3D2A1B, a), 6f * u);
        if (Variant != 3) Poly(dc, b, new List<Vector2> { new(wx, wy + wh), new(wx + ww, wy + wh), new(wx + ww * 1.5f, h), new(wx - ww * 0.5f, h) }, Rgb(1, 0.95f, 0.72f, 0.16f * a));
        foreach (int s in new[] { -1, 1 }) for (int depth = 0; depth < 3; depth++) { float x0 = s < 0 ? depth * w * 0.13f : w - (depth + 1) * w * 0.13f, x1 = x0 + w * 0.13f, top = h * (0.02f + depth * 0.07f), bot = h * (0.96f - depth * 0.06f), fade = 1f - depth * 0.18f; Box(dc, b, x0, top, x1, bot, L(0x4B3020, a)); int rows = 6; for (int r = 0; r < rows; r++) { float y0 = top + (bot - top) * r / rows + 7f * u, y1 = top + (bot - top) * (r + 1) / rows; float x = x0 + 8f * u; int id = r * 37 + depth * 91 + s * 3; while (x < x1 - 14f * u) { float bw = (10f + Rnd(id++) * 17f) * u, bh = (y1 - y0) * (0.68f + Rnd(id) * 0.25f); var col = Gfx.Hsv(Rnd(id + 5) * 360f, 0.5f, 0.72f * fade * Dim); Box(dc, b, x, y1 - bh, x + bw, y1, Col(col, a)); x += bw + 1.5f * u; } Box(dc, b, x0, y1 - 1f * u, x1, y1 + 6f * u, L(0x6B4630, a)); } }
    }
}

public sealed class GymBackPlugin : QuickShapePlugin { public override string Name => "演出：体育館"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GymBackParameter(s); }
public sealed class GymBackParameter : Q3BackParam
{
    public GymBackParameter() : this(null) { }
    public GymBackParameter(SharedDataStore? s) : base(s) { }
    protected override double DefAmount => 80;
    static readonly Dictionary<string, ParamGuide> g = G("体育館", Tod, Veil, "Amount=照明の明るさ");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float fl = h * 0.58f; Box(dc, b, 0, 0, w, fl, L(0xBFC6C2, a)); Box(dc, b, 0, h * 0.26f, w, h * 0.3f, L(0x546E7A, a)); for (int i = 0; i < 8; i++) { float x = w * (0.05f + i * 0.115f); var sk = SkyCols(a); Shade(dc, new(0, h * 0.06f), new(0, h * 0.22f), sk.top, sk.low, x, h * 0.06f, x + w * 0.08f, h * 0.22f); Frame(dc, b, x, h * 0.06f, x + w * 0.08f, h * 0.22f, L(0x37474F, a), 4f * u); }
        Poly(dc, b, new List<Vector2> { new(0, fl), new(w, fl), new(w * 1.25f, h), new(-w * 0.25f, h) }, L(0xD9A85F, a)); for (int i = -8; i <= 8; i++) Ln(dc, b, new(w / 2f + i * w * 0.08f, fl), new(w / 2f + i * w * 0.16f, h), L(0xB98545, 0.45f * a), 2f * u); foreach (float yy in new[] { 0.15f, 0.42f, 0.74f }) Ln(dc, b, new(0, fl + (h - fl) * yy), new(w, fl + (h - fl) * yy), Rgb(1, 1, 1, 0.75f * a), 3f * u); b.Color = Rgb(1, 1, 1, 0.78f * a); dc.DrawEllipse(new Ellipse(new(w / 2f, fl + (h - fl) * 0.42f), w * 0.15f, (h - fl) * 0.22f), b, 4f * u); Ln(dc, b, new(w / 2f, fl), new(w / 2f, h), Rgb(1, 1, 1, 0.7f * a), 3f * u);
        var hoop = new Vector2(w / 2f, h * 0.36f); Box(dc, b, hoop.X - 8f * u, h * 0.02f, hoop.X + 8f * u, hoop.Y - 90f * u, L(0x37474F, a)); Box(dc, b, hoop.X - 120f * u, hoop.Y - 96f * u, hoop.X + 120f * u, hoop.Y + 28f * u, Rgb(1, 1, 1, 0.84f * a)); Frame(dc, b, hoop.X - 120f * u, hoop.Y - 96f * u, hoop.X + 120f * u, hoop.Y + 28f * u, L(0xC62828, a), 5f * u); Frame(dc, b, hoop.X - 44f * u, hoop.Y - 50f * u, hoop.X + 44f * u, hoop.Y + 20f * u, L(0xC62828, a), 4f * u); b.Color = L(0xFF6F00, a); dc.DrawEllipse(new Ellipse(hoop + new Vector2(0, 31f * u), 48f * u, 10f * u), b, 6f * u); for (int i = -3; i <= 3; i++) Ln(dc, b, hoop + new Vector2(i * 14f * u, 36f * u), hoop + new Vector2(i * 8f * u, 92f * u), Rgb(1, 1, 1, 0.7f * a), 2f * u);
        for (int i = 0; i < 6; i++) { var q = new Vector2(w * (0.1f + i * 0.16f), h * 0.02f); Oval(dc, b, q + new Vector2(0, 12f * u), 56f * u, 12f * u, Rgb(1, 0.98f, 0.86f, amt * a)); using var gl = Gfx.Radial(dc, q, 300f * u, (0f, Rgb(1, 0.98f, 0.86f, amt * 0.26f * a)), (1f, Rgb(1, 0.98f, 0.86f, 0))); dc.FillEllipse(new Ellipse(q, 300f * u, 300f * u), gl); }
    }
}

public sealed class SchoolRooftopPlugin : QuickShapePlugin { public override string Name => "演出：学校の屋上"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SchoolRooftopParameter(s); }
public sealed class SchoolRooftopParameter : Q3BackParam
{
    public SchoolRooftopParameter() : this(null) { }
    public SchoolRooftopParameter(SharedDataStore? s) : base(s) { }
    static readonly Dictionary<string, ParamGuide> g = G("学校の屋上", Tod, Veil, "Speed=雲の速さ");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        SkyFill(dc, w, h, a); Stars(dc, b, w, h * 0.65f, u, t, a); Clouds(dc, b, w, h, u, t, a, 22f); float fy = h * 0.64f; for (int i = 0; i < 18; i++) { float x = i / 17f * w, H = (70f + Rnd(i) * 130f) * u; Box(dc, b, x - 45f * u, fy - H, x + 45f * u, fy + 10f * u, L(0x7F93A8, a)); if (Variant == 3) for (int k = 0; k < 8; k++) if (Rnd(i * 9 + k) > 0.55f) Box(dc, b, x - 34f * u + (k % 4) * 20f * u, fy - H + 10f * u + k / 4 * 28f * u, x - 24f * u + (k % 4) * 20f * u, fy - H + 24f * u + k / 4 * 28f * u, Rgb(1, 0.86f, 0.48f, a)); } Box(dc, b, 0, fy, w, h, L(0xA9AEAD, a)); for (int i = 0; i < 7; i++) Ln(dc, b, new(i / 6f * w, fy), new(w / 2f + (i / 6f - 0.5f) * w * 1.7f, h), L(0x7F8584, 0.65f * a), 3f * u);
        float top = h * 0.24f, base0 = h * 0.78f, cell = 48f * u; for (float x = -h; x < w + h; x += cell) { Ln(dc, b, new(x, top), new(x + (base0 - top), base0), L(0x5F7A70, 0.82f * a), 2.5f * u); Ln(dc, b, new(x + (base0 - top), top), new(x, base0), L(0x5F7A70, 0.82f * a), 2.5f * u); } Box(dc, b, 0, top - 7f * u, w, top + 7f * u, L(0x466358, a)); Box(dc, b, 0, base0 - 6f * u, w, base0 + 8f * u, L(0x466358, a)); for (int i = 0; i <= 8; i++) { float x = i / 8f * w; Box(dc, b, x - 7f * u, top, x + 7f * u, base0 + 8f * u, L(0x466358, a)); }
    }
}

public sealed class SchoolHillRoadPlugin : QuickShapePlugin { public override string Name => "演出：通学路の坂道"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SchoolHillRoadParameter(s); }
public sealed class SchoolHillRoadParameter : Q3BackParam
{
    public SchoolHillRoadParameter() : this(null) { }
    public SchoolHillRoadParameter(SharedDataStore? s) : base(s) { Count = 1; }
    static readonly Dictionary<string, ParamGuide> g = G("通学路の坂道", Tod, Veil, "Count=奥の景色|1=海　2=山|1|2");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        SkyFill(dc, w, h, a); Stars(dc, b, w, h * 0.42f, u, t, a); Clouds(dc, b, w, h * 0.7f, u, t, a); float hz = h * 0.42f; if (Count == 2) { var m = new List<Vector2> { new(0, hz + 40f * u) }; for (int i = 0; i <= 30; i++) { float k = i / 30f; m.Add(new(k * w, hz - (MathF.Abs(MathF.Sin(k * 6f)) * 120f + 22f) * u)); } m.Add(new(w, hz + 40f * u)); Poly(dc, b, m, L(0x5F9A73, a)); } else { Box(dc, b, 0, hz, w, hz + 110f * u, L(0x3D9BD6, a)); for (int i = 0; i < 18; i++) { float x = Gfx.Mod(Rnd(i) * w + MathF.Sin(t + i) * 12f * u, w), y = hz + (Rnd(i + 5) * 90f + 8f) * u; Ln(dc, b, new(x - 30f * u, y), new(x + 30f * u, y), Rgb(1, 1, 1, 0.45f * a), 2f * u); } }
        foreach (int s in new[] { -1, 1 }) { Poly(dc, b, new List<Vector2> { new(w / 2f + s * 40f * u, hz + 60f * u), new(w / 2f + s * w * 0.62f, hz + 50f * u), new(w / 2f + s * w * 0.62f, h), new(w / 2f + s * w * 0.3f, h) }, L(0x8DB678, a)); for (int i = 0; i < 4; i++) { float k = i / 3f, sc = 0.45f + k * 1.1f, x = w / 2f + s * (150f + k * 520f) * u * 1.1f, y = hz + (90f + k * 330f) * u, W2 = 135f * u * sc, H2 = 120f * u * sc; Box(dc, b, x - W2, y - H2, x + W2, y + H2 * 0.5f, L(i % 2 == 0 ? 0xF2E5CF : 0xD8C9B5, a)); Poly(dc, b, new List<Vector2> { new(x - W2 * 1.2f, y - H2), new(x + W2 * 1.2f, y - H2), new(x, y - H2 * 1.75f) }, L(i % 2 == 0 ? 0x9A4A3C : 0x486A8A, a)); Box(dc, b, x - W2 * 0.55f, y - H2 * 0.6f, x - W2 * 0.12f, y - H2 * 0.05f, Variant == 3 ? Rgb(1, 0.85f, 0.45f, a) : L(0x8FC6E8, a)); } }
        Poly(dc, b, new List<Vector2> { new(w / 2f - 42f * u, hz + 58f * u), new(w / 2f + 42f * u, hz + 58f * u), new(w * 0.86f, h), new(w * 0.14f, h) }, L(0x777C83, a)); for (int i = 0; i < 6; i++) { float k0 = i / 6f, k1 = k0 + 0.07f, y0 = hz + 58f * u + k0 * k0 * (h - hz - 58f * u), y1 = hz + 58f * u + k1 * k1 * (h - hz - 58f * u); Ln(dc, b, new(w / 2f, y0), new(w / 2f, y1), Rgb(1, 1, 1, 0.85f * a), (2f + k0 * 12f) * u); }
        foreach (int s in new[] { -1, 1 }) for (int i = 0; i < 3; i++) { float k = (i + 0.5f) / 3f, sc = 0.34f + k * k * 1.5f, x = w / 2f + s * (80f + k * k * w * 0.42f), by = hz + 72f * u + k * k * (h - hz - 90f * u), top = by - 440f * u * sc; Ln(dc, b, new(x, by), new(x, top), L(0x4E4A47, a), 12f * u * sc); for (int q = 0; q < 2; q++) Ln(dc, b, new(x - 55f * u * sc, top + (28f + q * 34f) * u * sc), new(x + 55f * u * sc, top + (28f + q * 34f) * u * sc), L(0x4E4A47, a), 7f * u * sc); }
    }
}

public sealed class ShoppingArcadePlugin : QuickShapePlugin { public override string Name => "演出：商店街のアーケード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ShoppingArcadeParameter(s); }
public sealed class ShoppingArcadeParameter : Q3BackParam
{
    public ShoppingArcadeParameter() : this(null) { }
    public ShoppingArcadeParameter(SharedDataStore? s) : base(s) { Text = "八百屋\n本屋\n喫茶\n洋品店\nパン\n薬局"; }
    static readonly Dictionary<string, ParamGuide> g = G("商店街", Tod, Veil, "Text=看板の文字|1行に1つ");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        var r = Rows; if (r.Count == 0) r = ["商店"]; var o = new Vector2(w / 2f, h * 0.46f); Box(dc, b, 0, 0, w, h, L(0xA9C4CF, a)); var sk = SkyCols(a); Shade(dc, new(0, o.Y - 70f * u), new(0, o.Y + 70f * u), sk.top, sk.low, o.X - 90f * u, o.Y - 70f * u, o.X + 90f * u, o.Y + 70f * u); Poly(dc, b, new List<Vector2> { new(o.X - 90f * u, o.Y + 70f * u), new(o.X + 90f * u, o.Y + 70f * u), new(w * 1.15f, h), new(-w * 0.15f, h) }, L(0xB5AFA5, a)); for (int i = 0; i < 9; i++) { float k = i / 8f; Ln(dc, b, new(o.X + (k - 0.5f) * 180f * u, o.Y + 70f * u), new(w / 2f + (k - 0.5f) * w * 1.3f, h), L(0x8C867C, 0.55f * a), 2f * u); }
        Poly(dc, b, new List<Vector2> { new(0, 0), new(w, 0), new(o.X + 90f * u, o.Y - 70f * u), new(o.X - 90f * u, o.Y - 70f * u) }, L(0xDDE7EA, a)); for (int i = 1; i < 8; i++) { float k = 1f - i / 8f, e = k * k, x = Lerp(90f * u, w * 0.54f, e), y = Lerp(o.Y - 70f * u, 0f, e); Ln(dc, b, new(o.X - x, y), new(o.X + x, y), L(0x7C8D96, a), (2f + e * 7f) * u); }
        foreach (int s in new[] { -1, 1 }) for (int i = 5; i >= 0; i--)
        {
            float k0 = i / 6f, k1 = (i + 1) / 6f, e0 = k0 * k0, e1 = k1 * k1, x0 = o.X + s * Lerp(92f * u, w * 0.52f, e0), x1 = o.X + s * Lerp(92f * u, w * 0.52f, e1), t0 = Lerp(o.Y - 68f * u, 0f, e0), t1 = Lerp(o.Y - 68f * u, 0f, e1), b0 = Lerp(o.Y + 70f * u, h, e0), b1 = Lerp(o.Y + 70f * u, h, e1); int id = i * 2 + (s > 0 ? 1 : 0); Poly(dc, b, new List<Vector2> { new(x0, t0), new(x1, t1), new(x1, b1), new(x0, b0) }, Col(Gfx.Hsv(id * 53f + 15f, 0.28f, 0.82f * Dim), a)); float sg0 = Lerp(t0, b0, 0.13f), sg1 = Lerp(t1, b1, 0.13f), sb0 = Lerp(t0, b0, 0.3f), sb1 = Lerp(t1, b1, 0.3f); Poly(dc, b, new List<Vector2> { new(x0, sg0), new(x1, sg1), new(x1, sb1), new(x0, sb0) }, Col(Gfx.Hsv(id * 53f + 5f, 0.72f, 0.82f), a)); Say(dc, b, r[id % r.Count], MathF.Max(12f * u, MathF.Abs(x1 - x0) * 0.2f), new((x0 + x1) / 2f, (sg0 + sb1) / 2f), Rgb(1, 1, 1, a), maxW: MathF.Abs(x1 - x0) * 0.86f); Poly(dc, b, new List<Vector2> { new(x0, Lerp(t0, b0, 0.42f)), new(x1, Lerp(t1, b1, 0.42f)), new(x1, Lerp(t1, b1, 0.88f)), new(x0, Lerp(t0, b0, 0.88f)) }, Variant == 3 ? Rgb(1, 0.86f, 0.52f, 0.85f * a) : Rgb(0.78f, 0.9f, 0.96f, 0.72f * a)); var lamp = new Vector2(x1, Lerp(t1, b1, 0.34f)); float sw = MathF.Sin(t * 1.4f + i) * 4f * u; Oval(dc, b, lamp + new Vector2(sw, 0), 26f * u * (0.45f + e1), 36f * u * (0.45f + e1), Rgb(1, 0.33f, 0.2f, a));
        }
    }
}

public sealed class AmusementParkPlugin : QuickShapePlugin { public override string Name => "演出：遊園地"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AmusementParkParameter(s); }
public sealed class AmusementParkParameter : Q3BackParam
{
    public AmusementParkParameter() : this(null) { }
    public AmusementParkParameter(SharedDataStore? s) : base(s) { Background = true; }
    static readonly Dictionary<string, ParamGuide> g = G("遊園地", Tod, Veil, "Speed=回る速さ", "Background=夜の電飾");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        SkyFill(dc, w, h, a); Stars(dc, b, w, h * 0.7f, u, t, a); Clouds(dc, b, w, h * 0.65f, u, t, a); Box(dc, b, 0, h * 0.82f, w, h, L(0x6FA05E, a)); bool lit = Background && Variant >= 2; var o = new Vector2(w * 0.31f, h * 0.43f); float R = 330f * u; foreach (int s in new[] { -1, 1 }) Ln(dc, b, o, o + new Vector2(s * 170f * u, h * 0.42f), L(0x54606E, a), 14f * u); b.Color = L(0xE9EEF2, a); dc.DrawEllipse(new Ellipse(o, R, R), b, 7f * u); dc.DrawEllipse(new Ellipse(o, R * 0.72f, R * 0.72f), b, 3f * u); const int cars = 12; float rot = t * 0.22f;
        for (int i = 0; i < cars; i++) { float th = rot + i * MathF.Tau / cars; var q = Pol(o, R, th); Ln(dc, b, o, q, L(0xD5DCE3, a), 3f * u); var col = Gfx.Hsv(i * 30f, 0.72f, 0.95f); RBox(dc, b, q + new Vector2(0, 31f * u), 54f * u, 44f * u, 11f * u, Col(col, a)); Box(dc, b, q.X - 19f * u, q.Y + 20f * u, q.X + 19f * u, q.Y + 34f * u, Rgb(0.8f, 0.94f, 1f, a)); if (lit) Dot(dc, b, q, 8f * u * (0.65f + 0.35f * MathF.Sin(t * 5f + i)), Col(col, a)); } Dot(dc, b, o, 18f * u, L(0x54606E, a));
        Vector2 Rail(float k) => new(w * (0.53f + k * 0.5f), h * (0.57f - MathF.Sin(k * 9f) * 0.17f * (1f - k * 0.3f) + k * 0.11f)); var rail = new List<Vector2>(); for (int i = 0; i <= 90; i++) rail.Add(Rail(i / 90f)); for (int i = 0; i <= 90; i += 5) Ln(dc, b, rail[i], new(rail[i].X, h * 0.84f), L(0x8A97A3, a), 3f * u); Path(dc, b, rail, L(0xE53935, a), 8f * u);
        float ph = Gfx.Mod(t * 0.17f, 1f); for (int i = 0; i < 4; i++) { float k = Gfx.Mod(ph - i * 0.035f, 1f); var q = Rail(k); var f = Rail(MathF.Min(1f, k + 0.006f)) - q; var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(MathF.Atan2(f.Y, f.X), q) * prev; RBox(dc, b, q - new Vector2(0, 16f * u), 48f * u, 26f * u, 7f * u, L(i == 0 ? 0xFFC107 : 0x1E88E5, a)); dc.Transform = prev; } if (lit) for (int i = 0; i <= 90; i += 3) if (((i / 3) + (int)(t * 4f)) % 3 == 0) Dot(dc, b, rail[i], 5f * u, Rgb(1, 0.92f, 0.45f, a));
    }
}

public sealed class AirportLobbyPlugin : QuickShapePlugin { public override string Name => "演出：空港の出発ロビー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AirportLobbyParameter(s); }
public sealed class AirportLobbyParameter : Q3BackParam
{
    public AirportLobbyParameter() : this(null) { }
    public AirportLobbyParameter(SharedDataStore? s) : base(s) { Text = "YK101:東京:10:30\nYK205:大阪:11:05\nYK318:札幌:11:40\nYK422:那覇:12:15"; }
    static readonly Dictionary<string, ParamGuide> g = G("空港の出発ロビー", Tod, Veil, "Text=掲示板|1行に「便名:行き先:時刻」(時刻の「:」もそのまま)");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float wy = h * 0.3f, wb = h * 0.72f; Box(dc, b, 0, 0, w, h, L(0xCBD3D8, a)); var sk = SkyCols(a); Shade(dc, new(0, wy), new(0, wb), sk.top, sk.low, 0, wy, w, wb); Stars(dc, b, w, wb, u, t, a); Box(dc, b, 0, wb - 48f * u, w, wb, L(0x757B80, a)); float k = Gfx.Mod(t * 0.075f, 1f); var pl = new Vector2(Lerp(-260f * u, w + 260f * u, k), wb - 50f * u - MathF.Max(0f, k - 0.28f) * 620f * u); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(k > 0.28f ? -0.3f : 0f, pl) * prev; RBox(dc, b, pl, 330f * u, 42f * u, 21f * u, Rgb(0.96f, 0.97f, 0.98f, a)); Poly(dc, b, new List<Vector2> { pl + new Vector2(-40f * u, 6f * u), pl + new Vector2(35f * u, 6f * u), pl + new Vector2(-65f * u, 72f * u), pl + new Vector2(-100f * u, 72f * u) }, L(0x2F80ED, a)); Poly(dc, b, new List<Vector2> { pl + new Vector2(-155f * u, -8f * u), pl + new Vector2(-112f * u, -8f * u), pl + new Vector2(-155f * u, -72f * u), pl + new Vector2(-180f * u, -72f * u) }, L(0x2F80ED, a)); dc.Transform = prev;
        for (int i = 0; i <= 7; i++) Box(dc, b, i / 7f * w - 8f * u, wy, i / 7f * w + 8f * u, wb, L(0x46525C, a)); Box(dc, b, 0, wy - 8f * u, w, wy + 8f * u, L(0x46525C, a)); Box(dc, b, 0, wb, w, h, L(0xE1E5E7, a)); for (int i = 0; i < 9; i++) Ln(dc, b, new(i / 8f * w, wb), new(w / 2f + (i / 8f - 0.5f) * w * 1.6f, h), L(0xB6BDC1, 0.65f * a), 2f * u);
        var rows = Rows; var board = new Vector2(w * 0.5f, h * 0.15f); Panel(dc, b, board, 1140f * u, 270f * u, 8f * u, Rgb(0.03f, 0.04f, 0.05f, a), L(0x37474F, a), 7f * u); SayAt(dc, b, "出発  DEPARTURES", 24f * u, board + new Vector2(-540f * u, -125f * u), Rgb(1, 0.8f, 0.25f, a)); for (int i = 0; i < rows.Count && i < 4; i++) { var f = rows[i].Split([':', '：'], 3); float y = board.Y - 78f * u + i * 50f * u; SayAt(dc, b, f[0], 30f * u, new(board.X - 540f * u, y), Rgb(1, 0.82f, 0.25f, a)); if (f.Length > 1) SayAt(dc, b, f[1], 30f * u, new(board.X - 250f * u, y), Rgb(1, 1, 1, a), maxW: 400f * u); if (f.Length > 2) SayAt(dc, b, f[2], 30f * u, new(board.X + 330f * u, y), Rgb(0.45f, 1f, 0.6f, a)); }
    }
}

public sealed class TrainInteriorPlugin : QuickShapePlugin { public override string Name => "演出：電車の車内"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TrainInteriorParameter(s); }
public sealed class TrainInteriorParameter : Q3BackParam
{
    public TrainInteriorParameter() : this(null) { }
    public TrainInteriorParameter(SharedDataStore? s) : base(s) { Count = 1; }
    protected override double DefAmount => 40;
    static readonly Dictionary<string, ParamGuide> g = G("電車の車内", Tod, Veil, "Amount=揺れの大きさ", "Count=外の景色|1=街　2=田んぼ　3=海|1|3", "Speed=流れる速さ");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float swing = MathF.Sin(t * 2.2f) * amt, shake = MathF.Sin(t * 13f) * amt * 2f * u, wy = h * 0.16f, wb = h * 0.56f; Box(dc, b, 0, 0, w, h, L(0xDDE2DD, a)); var sk = SkyCols(a); Shade(dc, new(0, wy), new(0, wb), sk.top, sk.low, 0, wy, w, wb); dc.PushAxisAlignedClip(new Vortice.RawRectF(0, wy, w, wb), AntialiasMode.Aliased); Stars(dc, b, w, wb, u, t, a); float gnd = wb - 60f * u;
        if (Count == 3) { Box(dc, b, 0, gnd - 45f * u, w, wb, L(0x3D9BD6, a)); for (int i = 0; i < 22; i++) { float x = Gfx.Mod(Rnd(i) * w - t * 80f * u, w), y = gnd - 30f * u + Rnd(i + 3) * 90f * u; Ln(dc, b, new(x, y), new(x + 44f * u, y), Rgb(1, 1, 1, 0.5f * a), 2f * u); } }
        else if (Count == 2) { Box(dc, b, 0, gnd, w, wb, L(0x8DBB54, a)); for (int l = 0; l < 2; l++) { var m = new List<Vector2> { new(0, gnd) }; for (int i = 0; i <= 24; i++) { float k = i / 24f; m.Add(new(k * w, gnd - (MathF.Abs(MathF.Sin(k * 4f + t * (0.05f + l * 0.05f))) * 92f + 18f) * u)); } m.Add(new(w, gnd)); Poly(dc, b, m, L(l == 0 ? 0x6B9A7A : 0x4F8A5B, a)); } for (int i = 0; i < 8; i++) { float x = Gfx.Mod(i / 8f * (w + 40f * u) - t * 540f * u, w + 40f * u); Ln(dc, b, new(x, wy), new(x, wb), L(0x5B4A3E, a), 7f * u); } }
        else { for (int l = 0; l < 2; l++) { float sp = l == 0 ? 90f : 260f, cell = (l == 0 ? 112f : 165f) * u; int n = (int)(w / cell) + 3; for (int i = 0; i < n; i++) { float x = Gfx.Mod(i * cell - t * sp * u, n * cell) - cell, H = (l == 0 ? 130f + Rnd(i) * 130f : 70f + Rnd(i + 8) * 110f) * u; Box(dc, b, x, wb - H, x + cell - 4f * u, wb, L(l == 0 ? 0x8FA0B2 : 0x657486, a)); if (Variant == 3) for (int k = 0; k < 8; k++) if (Rnd(i * 13 + k + l) > 0.5f) Box(dc, b, x + 10f * u + (k % 4) * 24f * u, wb - H + 12f * u + k / 4 * 30f * u, x + 22f * u + (k % 4) * 24f * u, wb - H + 28f * u + k / 4 * 30f * u, Rgb(1, 0.86f, 0.48f, a)); } } } dc.PopAxisAlignedClip();
        for (int i = 0; i <= 3; i++) { float x = i / 3f * w; Box(dc, b, x - 24f * u, wy, x + 24f * u, wb, L(0xB8C0B9, a)); } Box(dc, b, 0, wy - 12f * u, w, wy + 12f * u, L(0x87918B, a)); Box(dc, b, 0, wb - 7f * u, w, wb + 14f * u, L(0x87918B, a)); Box(dc, b, 0, h * 0.68f, w, h * 0.86f, L(0x3F6FA8, a)); for (int i = 0; i < 12; i++) Ln(dc, b, new(i / 11f * w, h * 0.68f), new(i / 11f * w, h * 0.86f), L(0x2D5888, a), 3f * u); Box(dc, b, 0, h * 0.86f, w, h, L(0x8F9288, a)); Ln(dc, b, new(0, h * 0.075f), new(w, h * 0.075f), L(0xA6ACA8, a), 8f * u);
        for (int i = 0; i < 11; i++) { float x = (i + 0.5f) / 11f * w; var top = new Vector2(x, h * 0.075f); var bot = top + new Vector2(swing * 58f * u + shake, 160f * u); Ln(dc, b, top, bot, L(0x6E7470, a), 8f * u); var ring = bot + Vector2.Normalize(bot - top + new Vector2(0.001f, 0)) * 34f * u; Ring(dc, b, ring, 33f * u, L(0xF5F5EF, a), 8f * u); }
    }
}

public sealed class CountryBusStopPlugin : QuickShapePlugin { public override string Name => "演出：バス停と田舎道"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CountryBusStopParameter(s); }
public sealed class CountryBusStopParameter : Q3BackParam
{
    public CountryBusStopParameter() : this(null) { }
    public CountryBusStopParameter(SharedDataStore? s) : base(s) { Text = "ゆっくり村"; Count = 2; }
    static readonly Dictionary<string, ParamGuide> g = G("バス停と田舎道", Tod, Veil, "Text=バス停の名前", "Count=季節|1=春　2=夏　3=秋　4=冬|1|4");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        int season = Math.Clamp(Count, 1, 4); uint field = season switch { 1 => 0xA7CF6A, 3 => 0xD9B44A, 4 => 0xF2F5F7, _ => 0x4FA650 }, hill = season switch { 1 => 0x8FBF7B, 3 => 0xC77942, 4 => 0xDCE5EA, _ => 0x3F8A52 }; float hz = h * 0.48f; SkyFill(dc, w, h, a); Stars(dc, b, w, hz, u, t, a); Clouds(dc, b, w, h * 0.75f, u, t, a); for (int l = 0; l < 2; l++) { var m = new List<Vector2> { new(0, hz + 12f * u) }; for (int i = 0; i <= 28; i++) { float k = i / 28f; m.Add(new(k * w, hz - (MathF.Sin(k * (4f + l * 2.2f) + l) * 54f + 86f - l * 38f) * u)); } m.Add(new(w, hz + 12f * u)); Poly(dc, b, m, L(l == 0 ? 0x7BA6B8 : hill, a)); } Box(dc, b, 0, hz, w, h, L(field, a)); for (int i = 0; i < 12; i++) { float y = hz + (i + 1f) * (i + 1f) * 3.6f * u; Ln(dc, b, new(0, y), new(w, y), L(season == 4 ? 0xC9D6DD : 0x3C8C45, 0.65f * a), 2f * u); } for (int i = -9; i <= 9; i++) Ln(dc, b, new(w / 2f + i * 70f * u, hz), new(w / 2f + i * 280f * u, h), L(season == 4 ? 0xC9D6DD : 0x3C8C45, 0.5f * a), 2f * u);
        Poly(dc, b, new List<Vector2> { new(w * 0.5f - 18f * u, hz), new(w * 0.5f + 18f * u, hz), new(w * 0.9f, h), new(w * 0.34f, h) }, L(0xA88F70, a)); var st = new Vector2(w * 0.2f, h * 0.76f); Box(dc, b, st.X - 190f * u, st.Y - 150f * u, st.X + 190f * u, st.Y + 90f * u, L(0x6D5138, a)); for (int i = -3; i <= 3; i++) Ln(dc, b, new(st.X + i * 54f * u, st.Y - 150f * u), new(st.X + i * 54f * u, st.Y + 90f * u), L(0x4E3827, a), 3f * u); Poly(dc, b, new List<Vector2> { new(st.X - 240f * u, st.Y - 145f * u), new(st.X + 240f * u, st.Y - 145f * u), new(st.X + 195f * u, st.Y - 240f * u), new(st.X - 195f * u, st.Y - 240f * u) }, L(season == 4 ? 0xEEF3F6 : 0x7C8A91, a)); RBox(dc, b, st + new Vector2(0, 40f * u), 280f * u, 32f * u, 8f * u, L(0xC69C6D, a)); var sign = new Vector2(st.X + 310f * u, st.Y - 145f * u); Ln(dc, b, sign, sign + new Vector2(0, 240f * u), L(0x78828A, a), 11f * u); Dot(dc, b, sign, 74f * u, L(0xE53935, a)); Dot(dc, b, sign, 58f * u, Rgb(1, 1, 1, a)); Say(dc, b, "BUS", 26f * u, sign - new Vector2(0, 16f * u), L(0xE53935, a)); Say(dc, b, Str, 21f * u, sign + new Vector2(0, 19f * u), Rgb(0.1f, 0.1f, 0.12f, a), maxW: 104f * u);
        if (season == 4) for (int i = 0; i < 70; i++) Dot(dc, b, new(Gfx.Mod(Rnd(i) * w + MathF.Sin(t + i) * 12f * u, w), Gfx.Mod(Rnd(i + 7) * h + t * (25f + Rnd(i + 2) * 40f) * u, h)), (1.5f + Rnd(i + 3) * 3f) * u, Rgb(1, 1, 1, 0.9f * a)); else if (season == 1) for (int i = 0; i < 34; i++) Oval(dc, b, new(Gfx.Mod(Rnd(i) * w + t * 26f * u, w), Gfx.Mod(Rnd(i + 7) * h + t * 34f * u, h)), 6f * u, 3.6f * u, Rgb(1, 0.72f, 0.82f, 0.92f * a));
    }
}

public sealed class KitchenBackPlugin : QuickShapePlugin { public override string Name => "演出：台所（キッチン）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new KitchenBackParameter(s); }
public sealed class KitchenBackParameter : Q3BackParam
{
    public KitchenBackParameter() : this(null) { }
    public KitchenBackParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0xF3E6C8); }
    static readonly Dictionary<string, ParamGuide> g = G("台所", Tod, Veil, "Background=鍋の湯気", "Color1=壁の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        var wall = Color1; uint wc = (uint)(wall.R << 16 | wall.G << 8 | wall.B); float ct = h * 0.58f; Box(dc, b, 0, 0, w, h, L(wc, a)); for (int y = 0; y < 8; y++) for (int x = 0; x < 16; x++) Frame(dc, b, x / 16f * w, h * 0.18f + y * h * 0.05f, (x + 1) / 16f * w, h * 0.18f + (y + 1) * h * 0.05f, L(0xC8B99D, 0.42f * a), 1.5f * u); Box(dc, b, w * 0.36f, 0, w * 0.68f, h * 0.17f, L(0x8A6849, a)); for (int i = 1; i < 4; i++) Ln(dc, b, new(w * 0.36f + w * 0.08f * i, 0), new(w * 0.36f + w * 0.08f * i, h * 0.17f), L(0x5E432D, a), 3f * u);
        var wn = new Vector2(w * 0.17f, h * 0.31f); var sk = SkyCols(a); Shade(dc, new(0, wn.Y - 130f * u), new(0, wn.Y + 130f * u), sk.top, sk.low, wn.X - 190f * u, wn.Y - 130f * u, wn.X + 190f * u, wn.Y + 130f * u); Frame(dc, b, wn.X - 190f * u, wn.Y - 130f * u, wn.X + 190f * u, wn.Y + 130f * u, L(0xFFFFFF, a), 11f * u); Ln(dc, b, wn - new Vector2(0, 130f * u), wn + new Vector2(0, 130f * u), L(0xFFFFFF, a), 7f * u); Box(dc, b, 0, ct, w * 0.8f, ct + 22f * u, L(0xD8DBDD, a)); Box(dc, b, 0, ct + 22f * u, w * 0.8f, h, L(0x9C7B59, a)); for (int i = 0; i < 6; i++) { float x0 = i / 6f * w * 0.8f, x1 = (i + 1) / 6f * w * 0.8f; Frame(dc, b, x0 + 6f * u, ct + 34f * u, x1 - 6f * u, h - 16f * u, L(0x6F563D, a), 4f * u); RBox(dc, b, new((x0 + x1) / 2f, ct + 76f * u), 76f * u, 11f * u, 5f * u, L(0xD8DBDD, a)); }
        Box(dc, b, w * 0.8f, h * 0.14f, w, h, L(0xE7EDF0, a)); Frame(dc, b, w * 0.8f, h * 0.14f, w - 3f * u, h, L(0x9FAAB0, a), 4f * u); Ln(dc, b, new(w * 0.8f, h * 0.45f), new(w, h * 0.45f), L(0x9FAAB0, a), 5f * u); RBox(dc, b, new(w * 0.83f, h * 0.31f), 14f * u, 170f * u, 7f * u, L(0x78858C, a)); RBox(dc, b, new(w * 0.83f, h * 0.62f), 14f * u, 190f * u, 7f * u, L(0x78858C, a)); var st = new Vector2(w * 0.56f, ct); Box(dc, b, st.X - 200f * u, st.Y - 12f * u, st.X + 200f * u, st.Y + 6f * u, L(0x2F3336, a)); foreach (int s in new[] { -1, 1 }) Oval(dc, b, st + new Vector2(s * 100f * u, -8f * u), 74f * u, 8f * u, Rgb(1, 0.35f, 0.1f, (0.5f + 0.3f * MathF.Sin(t * 8f + s)) * a)); RBox(dc, b, st + new Vector2(-100f * u, -70f * u), 180f * u, 100f * u, 12f * u, L(0xC9493A, a)); RBox(dc, b, st + new Vector2(-100f * u, -125f * u), 196f * u, 15f * u, 7f * u, L(0x963329, a)); Dot(dc, b, st + new Vector2(-100f * u, -142f * u), 11f * u, L(0x2F3336, a));
        if (Background) for (int s = 0; s < 4; s++) { var pts = new List<Vector2>(); for (int i = 0; i <= 24; i++) { float k = i / 24f; pts.Add(st + new Vector2(-100f * u + (s - 1.5f) * 34f * u + MathF.Sin(k * 7f - t * 2f + s) * 20f * u, -155f * u - k * 210f * u)); } Path(dc, b, pts, Rgb(1, 1, 1, 0.42f * a), 7f * u); }
    }
}

public sealed class WashitsuBackPlugin : QuickShapePlugin { public override string Name => "演出：和室"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WashitsuBackParameter(s); }
public sealed class WashitsuBackParameter : Q3BackParam
{
    public WashitsuBackParameter() : this(null) { }
    public WashitsuBackParameter(SharedDataStore? s) : base(s) { Text = "和"; }
    static readonly Dictionary<string, ParamGuide> g = G("和室", Tod, Veil, "Text=掛け軸の文字");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float fl = h * 0.67f; Box(dc, b, 0, 0, w, fl, L(0xCDB78F, a)); Box(dc, b, 0, 0, w, h * 0.09f, L(0x6E4E32, a)); float sx = w * 0.52f, sw = w * 0.46f; Box(dc, b, sx, h * 0.09f, sx + sw, fl, Variant == 3 ? L(0xAFC0E8, a) : Col(Hex(0xFFF8E7), a)); for (int i = 1; i < 6; i++) Ln(dc, b, new(sx + sw * i / 6f, h * 0.09f), new(sx + sw * i / 6f, fl), L(0x5F432C, a), 4f * u); for (int i = 1; i < 6; i++) Ln(dc, b, new(sx, h * 0.09f + (fl - h * 0.09f) * i / 6f), new(sx + sw, h * 0.09f + (fl - h * 0.09f) * i / 6f), L(0x5F432C, a), 4f * u); Frame(dc, b, sx, h * 0.09f, sx + sw, fl, L(0x5F432C, a), 12f * u); Ln(dc, b, new(sx + sw / 2f, h * 0.09f), new(sx + sw / 2f, fl), L(0x5F432C, a), 9f * u);
        Box(dc, b, w * 0.08f, h * 0.09f, w * 0.42f, fl, L(0xAD9772, a)); Frame(dc, b, w * 0.08f, h * 0.09f, w * 0.42f, fl, L(0x5F432C, a), 9f * u); var sc = new Vector2(w * 0.25f, h * 0.34f); RBox(dc, b, sc, 170f * u, 400f * u, 3f * u, L(0x496A72, a)); RBox(dc, b, sc, 124f * u, 300f * u, 2f * u, Col(Hex(0xFFF9EA), a)); Box(dc, b, sc.X - 98f * u, sc.Y - 205f * u, sc.X + 98f * u, sc.Y - 190f * u, L(0x3C2A1B, a)); Box(dc, b, sc.X - 98f * u, sc.Y + 190f * u, sc.X + 98f * u, sc.Y + 205f * u, L(0x3C2A1B, a)); string s = Str.Replace("\n", ""); float fs = MathF.Min(96f * u, 250f * u / MathF.Max(1, s.Length)); for (int i = 0; i < s.Length && i < 5; i++) Say(dc, b, s[i].ToString(), fs, sc + new Vector2(0, (i - (MathF.Min(s.Length, 5) - 1) / 2f) * fs * 1.08f), Rgb(0.08f, 0.07f, 0.07f, a));
        Box(dc, b, 0, fl, w, h, L(0xB7BC7A, a)); for (int i = 0; i <= 6; i++) Ln(dc, b, new(w / 2f + (i / 6f - 0.5f) * w, fl), new(w / 2f + (i / 6f - 0.5f) * w * 1.65f, h), L(0x4E5A32, 0.75f * a), 4f * u); Ln(dc, b, new(0, fl + (h - fl) * 0.45f), new(w, fl + (h - fl) * 0.45f), L(0x4E5A32, 0.75f * a), 4f * u); var tb = new Vector2(w * 0.5f, h * 0.84f); foreach (int s2 in new[] { -1, 1 }) Ln(dc, b, tb + new Vector2(s2 * 220f * u, 10f * u), tb + new Vector2(s2 * 195f * u, 92f * u), L(0x3F2A1A, a), 18f * u); Oval(dc, b, tb + new Vector2(0, 12f * u), 330f * u, 66f * u, L(0x5A3A22, a)); Oval(dc, b, tb, 330f * u, 62f * u, L(0x8A5B36, a)); Pie(dc, b, tb + new Vector2(-40f * u, -24f * u), 34f * u, 0f, MathF.PI, L(0x3F6F52, a)); Oval(dc, b, tb + new Vector2(-40f * u, -24f * u), 34f * u, 8f * u, L(0x88B060, a));
    }
}

public sealed class NightBedroomPlugin : QuickShapePlugin { public override string Name => "演出：夜の寝室"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new NightBedroomParameter(s); }
public sealed class NightBedroomParameter : Q3BackParam
{
    public NightBedroomParameter() : this(null) { }
    public NightBedroomParameter(SharedDataStore? s) : base(s) { Variant = 3; Color1 = Hex(0xFFC56E); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("夜の寝室", Tod, Veil, "Amount=明るさ", "Color1=ライトの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float fl = h * 0.72f; Box(dc, b, 0, 0, w, fl, L(0x7E8DAD, a)); Box(dc, b, 0, fl, w, h, L(0x6B5240, a)); for (int i = 0; i < 9; i++) Ln(dc, b, new(w / 2f + (i / 8f - 0.5f) * w, fl), new(w / 2f + (i / 8f - 0.5f) * w * 1.7f, h), L(0x4F3A2C, 0.7f * a), 2f * u); var wn = new Vector2(w * 0.73f, h * 0.31f); float ww = 245f * u, wh = 210f * u; var sk = SkyCols(a); Shade(dc, new(0, wn.Y - wh), new(0, wn.Y + wh), sk.top, sk.low, wn.X - ww, wn.Y - wh, wn.X + ww, wn.Y + wh); if (Variant == 3) { Dot(dc, b, wn + new Vector2(90f * u, -70f * u), 62f * u, Rgb(1, 0.96f, 0.76f, a)); for (int i = 0; i < 22; i++) Dot(dc, b, wn + new Vector2((Rnd(i) - 0.5f) * ww * 1.9f, (Rnd(i + 8) - 0.5f) * wh * 1.9f), (1f + Rnd(i + 3) * 1.8f) * u, Rgb(1, 1, 1, (0.4f + 0.6f * MathF.Abs(MathF.Sin(t + i))) * a)); Poly(dc, b, new List<Vector2> { wn + new Vector2(-ww, wh), wn + new Vector2(ww, wh), wn + new Vector2(ww * 0.4f, h - wn.Y), wn + new Vector2(-ww * 1.8f, h - wn.Y) }, Rgb(0.72f, 0.82f, 1f, 0.15f * a)); } Frame(dc, b, wn.X - ww, wn.Y - wh, wn.X + ww, wn.Y + wh, L(0xE7EAF0, a), 11f * u); Ln(dc, b, wn - new Vector2(0, wh), wn + new Vector2(0, wh), L(0xE7EAF0, a), 7f * u); foreach (int s in new[] { -1, 1 }) Poly(dc, b, new List<Vector2> { wn + new Vector2(s * (ww + 42f * u), -wh - 26f * u), wn + new Vector2(s * ww * 0.68f, -wh - 26f * u), wn + new Vector2(s * (ww + 8f * u), wh + 48f * u), wn + new Vector2(s * (ww + 42f * u), wh + 48f * u) }, L(0x4F5F8E, a));
        var bed = new Vector2(w * 0.31f, h * 0.76f); Box(dc, b, bed.X - 450f * u, bed.Y - 245f * u, bed.X - 415f * u, bed.Y + 130f * u, L(0x5E432D, a)); Box(dc, b, bed.X - 450f * u, bed.Y + 40f * u, bed.X + 430f * u, bed.Y + 130f * u, L(0x7A5A3D, a)); RBox(dc, b, bed + new Vector2(-10f * u, 0), 850f * u, 130f * u, 28f * u, L(0xE8E8EF, a)); RBox(dc, b, bed + new Vector2(-310f * u, -62f * u), 210f * u, 82f * u, 30f * u, L(0xFFFFFF, a)); RBox(dc, b, bed + new Vector2(120f * u, -26f * u), 590f * u, 136f * u, 34f * u, L(0x5472B8, a)); for (int i = 0; i < 5; i++) Ln(dc, b, bed + new Vector2((-110f + i * 110f) * u, -90f * u), bed + new Vector2((-110f + i * 110f) * u, 40f * u), L(0x3D5897, a), 3f * u);
        var lamp = new Vector2(w * 0.57f, h * 0.62f); Box(dc, b, lamp.X - 60f * u, lamp.Y + 60f * u, lamp.X + 60f * u, fl + 42f * u, L(0x5E432D, a)); Ln(dc, b, lamp + new Vector2(0, 62f * u), lamp - new Vector2(0, 15f * u), L(0xB6A68E, a), 8f * u); float glow = 0.2f + amt * 0.8f; using (var gl = Gfx.Radial(dc, lamp - new Vector2(0, 40f * u), 420f * u, (0f, Col(Color1, 0.55f * glow * a)), (1f, Col(Color1, 0f)))) dc.FillEllipse(new Ellipse(lamp - new Vector2(0, 40f * u), 420f * u, 420f * u), gl); Poly(dc, b, new List<Vector2> { lamp + new Vector2(-30f * u, -80f * u), lamp + new Vector2(30f * u, -80f * u), lamp + new Vector2(62f * u, -8f * u), lamp + new Vector2(-62f * u, -8f * u) }, Col(Color1, (0.6f + 0.4f * glow) * a));
    }
}

public sealed class RamenCounterPlugin : QuickShapePlugin { public override string Name => "演出：ラーメン屋のカウンター"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RamenCounterParameter(s); }
public sealed class RamenCounterParameter : Q3BackParam
{
    public RamenCounterParameter() : this(null) { }
    public RamenCounterParameter(SharedDataStore? s) : base(s) { Text = "醤油\n味噌\n塩\n豚骨\n餃子\n半炒飯"; Background = true; }
    static readonly Dictionary<string, ParamGuide> g = G("ラーメン屋", Tod, Veil, "Text=札の文字|1行に1つ", "Background=湯気");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        var r = Rows; float ct = h * 0.66f; Box(dc, b, 0, 0, w, ct, L(0xD9C3A0, a)); for (int i = 0; i < 12; i++) Ln(dc, b, new(i / 11f * w, 0), new(i / 11f * w, ct), L(0xB99D76, 0.5f * a), 2f * u); for (int i = 0; i < r.Count && i < 10; i++) { float x = w * 0.08f + i * MathF.Min(120f * u, w * 0.84f / r.Count); RBox(dc, b, new(x + 44f * u, h * 0.34f), 88f * u, 300f * u, 3f * u, Rgb(1, 0.97f, 0.88f, a)); Dot(dc, b, new(x + 44f * u, h * 0.34f - 136f * u), 5f * u, Rgb(0.2f, 0.15f, 0.1f, a)); string s = r[i]; float fs = MathF.Min(52f * u, 230f * u / MathF.Max(1, s.Length)); for (int k = 0; k < s.Length; k++) Say(dc, b, s[k].ToString(), fs, new(x + 44f * u, h * 0.34f + (k - (s.Length - 1) / 2f) * fs * 1.04f), Rgb(0.12f, 0.1f, 0.09f, a)); }
        for (int i = 0; i < 8; i++) { float x0 = i / 8f * w, x1 = (i + 1) / 8f * w - 5f * u, sw = MathF.Sin(t * 1.3f + i * 0.8f) * 7f * u; Poly(dc, b, new List<Vector2> { new(x0, 0), new(x1, 0), new(x1 + sw, h * 0.15f), new(x0 + sw, h * 0.15f) }, L(0xB71C1C, a)); if (i is >= 2 and <= 5) Say(dc, b, "らあめん"[i - 2].ToString(), 72f * u, new((x0 + x1) / 2f + sw * 0.5f, h * 0.075f), Rgb(1, 1, 1, a)); } Box(dc, b, 0, ct, w, ct + 40f * u, L(0x8A5B36, a)); Box(dc, b, 0, ct + 40f * u, w, h, L(0x5E3C25, a)); for (int i = 0; i < 5; i++) { var q = new Vector2((i + 0.5f) / 5f * w, h * 0.95f); Oval(dc, b, q, 94f * u, 28f * u, L(0x8F2E22, a)); Box(dc, b, q.X - 11f * u, q.Y, q.X + 11f * u, h, L(0x4A4A4C, a)); }
        var bowl = new Vector2(w / 2f, ct - 42f * u); Pie(dc, b, bowl, 190f * u, 0f, MathF.PI, L(0xB71C1C, a)); Box(dc, b, bowl.X - 58f * u, bowl.Y + 180f * u, bowl.X + 58f * u, bowl.Y + 200f * u, L(0x8A1712, a)); Oval(dc, b, bowl, 190f * u, 46f * u, Rgb(0.97f, 0.97f, 0.93f, a)); Oval(dc, b, bowl + new Vector2(0, 2f * u), 166f * u, 36f * u, L(0xC98536, a)); for (int i = 0; i < 12; i++) Arc(dc, b, bowl + new Vector2((i - 5.5f) * 22f * u, 8f * u), 22f * u, MathF.PI, MathF.Tau, L(0xFFE27A, a), 4f * u); Oval(dc, b, bowl + new Vector2(70f * u, -7f * u), 40f * u, 18f * u, L(0xFFFFFF, a)); Oval(dc, b, bowl + new Vector2(70f * u, -7f * u), 17f * u, 9f * u, L(0xFFB300, a)); Oval(dc, b, bowl + new Vector2(-72f * u, -5f * u), 48f * u, 20f * u, L(0xB66E5B, a)); Poly(dc, b, new List<Vector2> { bowl + new Vector2(-14f * u, -32f * u), bowl + new Vector2(34f * u, -30f * u), bowl + new Vector2(26f * u, 6f * u), bowl + new Vector2(-20f * u, 4f * u) }, L(0x1E3A2B, a));
        if (Background) for (int s = 0; s < 5; s++) { var pts = new List<Vector2>(); for (int i = 0; i <= 24; i++) { float k = i / 24f; pts.Add(bowl + new Vector2((s - 2f) * 50f * u + MathF.Sin(k * 7f - t * 2f + s) * 22f * u, -38f * u - k * 250f * u)); } Path(dc, b, pts, Rgb(1, 1, 1, 0.38f * a), 7f * u); }
    }
}

public sealed class ConvenienceStorePlugin : QuickShapePlugin { public override string Name => "演出：コンビニの店内"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ConvenienceStoreParameter(s); }
public sealed class ConvenienceStoreParameter : Q3BackParam
{
    public ConvenienceStoreParameter() : this(null) { }
    public ConvenienceStoreParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x2E9E6B); }
    static readonly Dictionary<string, ParamGuide> g = G("コンビニ", Tod, Veil, "Color1=棚の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        var sc = Color1; float fl = h * 0.72f; Box(dc, b, 0, 0, w, fl, Rgb(0.94f, 0.96f, 0.96f, a)); Box(dc, b, 0, 0, w, h * 0.06f, Rgb(0.84f, 0.87f, 0.88f, a)); for (int i = 0; i < 5; i++) RBox(dc, b, new((i + 0.5f) / 5f * w, h * 0.03f), 230f * u, 16f * u, 6f * u, Rgb(1, 1, 0.92f, a)); Box(dc, b, 0, fl, w, h, Rgb(0.82f, 0.84f, 0.84f, a)); for (int x = -10; x <= 10; x++) Ln(dc, b, new(w / 2f + x * 90f * u, fl), new(w / 2f + x * 230f * u, h), Rgb(0.65f, 0.68f, 0.69f, a), 2f * u); for (int y = 0; y < 5; y++) { float k = y / 4f; Ln(dc, b, new(0, fl + k * k * (h - fl)), new(w, fl + k * k * (h - fl)), Rgb(0.65f, 0.68f, 0.69f, a), 2f * u); }
        var door = new Vector2(w * 0.86f, h * 0.4f); float dw = 190f * u, dh = h * 0.32f, open = MathF.Max(0f, MathF.Sin(t * 0.7f)); var sk = SkyCols(a); Shade(dc, new(0, door.Y - dh), new(0, door.Y + dh), sk.top, sk.low, door.X - dw, door.Y - dh, door.X + dw, door.Y + dh); foreach (int s in new[] { -1, 1 }) { float x0 = door.X + s * open * dw * 0.82f; Box(dc, b, MathF.Min(x0, x0 + s * dw), door.Y - dh, MathF.Max(x0, x0 + s * dw), door.Y + dh, Rgb(0.75f, 0.92f, 0.98f, 0.45f * a)); Frame(dc, b, MathF.Min(x0, x0 + s * dw), door.Y - dh, MathF.Max(x0, x0 + s * dw), door.Y + dh, Rgb(0.4f, 0.45f, 0.48f, a), 6f * u); }
        foreach (float sx in new[] { 0.03f, 0.31f }) { float x0 = w * sx, x1 = x0 + w * 0.25f, top = h * 0.14f, bot = h * 0.82f; Box(dc, b, x0, top, x1, bot, Col(sc, a)); Box(dc, b, x0 + 10f * u, top + 10f * u, x1 - 10f * u, bot - 12f * u, Rgb(0.97f, 0.97f, 0.96f, a)); for (int r = 0; r < 5; r++) { float y1 = top + (r + 1) * (bot - top) / 5f - 8f * u; Box(dc, b, x0 + 10f * u, y1, x1 - 10f * u, y1 + 7f * u, Col(sc, a)); float x = x0 + 18f * u; int id = (int)(sx * 100f) + r * 31; while (x < x1 - 42f * u) { float pw = (22f + Rnd(id++) * 26f) * u, ph = (42f + Rnd(id) * 40f) * u; RBox(dc, b, new(x + pw / 2f, y1 - ph / 2f), pw, ph, 4f * u, Col(Gfx.Hsv(Rnd(id + 5) * 360f, 0.62f, 0.95f), a)); Box(dc, b, x + 3f * u, y1 - ph * 0.62f, x + pw - 3f * u, y1 - ph * 0.42f, Rgb(1, 1, 1, 0.8f * a)); x += pw + 3f * u; } } }
        var reg = new Vector2(w * 0.68f, h * 0.68f); Box(dc, b, reg.X - 118f * u, reg.Y, reg.X + 118f * u, h * 0.9f, Col(sc, a)); Box(dc, b, reg.X - 130f * u, reg.Y - 12f * u, reg.X + 130f * u, reg.Y + 10f * u, Rgb(0.82f, 0.84f, 0.85f, a)); RBox(dc, b, reg + new Vector2(0, -68f * u), 160f * u, 104f * u, 8f * u, Rgb(0.2f, 0.23f, 0.26f, a)); Box(dc, b, reg.X - 62f * u, reg.Y - 104f * u, reg.X + 62f * u, reg.Y - 48f * u, Rgb(0.45f, 0.86f, 0.98f, a)); if (Variant == 3) Box(dc, b, 0, 0, w, h, Rgb(0.08f, 0.12f, 0.3f, 0.12f * a));
    }
}

public sealed class StadiumBackPlugin : QuickShapePlugin { public override string Name => "演出：スタジアム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new StadiumBackParameter(s); }
public sealed class StadiumBackParameter : Q3BackParam
{
    public StadiumBackParameter() : this(null) { }
    public StadiumBackParameter(SharedDataStore? s) : base(s) { Count = 2; Background = true; }
    static readonly Dictionary<string, ParamGuide> g = G("スタジアム", Tod, Veil, "Count=競技|1=野球　2=サッカー|1|2", "Background=夜の照明");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        SkyFill(dc, w, h, a); Stars(dc, b, w, h * 0.3f, u, t, a); float top = h * 0.2f, fy = h * 0.54f; Poly(dc, b, new List<Vector2> { new(0, top + 54f * u), new(w * 0.5f, top), new(w, top + 54f * u), new(w, fy), new(0, fy) }, L(0x4D5766, a)); for (int i = 0; i < 620; i++) { float x = Rnd(i) * w, y = top + 20f * u + Rnd(i + 900) * (fy - top - 20f * u); Dot(dc, b, new(x, y + MathF.Abs(MathF.Sin(t * 3f + i)) * 2f * u), 3.4f * u, Col(Gfx.Hsv(Rnd(i + 500) * 360f, 0.55f, 0.95f * Dim), a)); } for (int r = 1; r < 5; r++) { float y = top + r * (fy - top) / 5f + 20f * u; Ln(dc, b, new(0, y), new(w, y), L(0x29313C, 0.45f * a), 3f * u); } Box(dc, b, 0, fy - 16f * u, w, fy, L(0x263C5C, a)); Poly(dc, b, new List<Vector2> { new(0, fy), new(w, fy), new(w * 1.25f, h), new(-w * 0.25f, h) }, L(0x3F9A46, a)); for (int i = 0; i < 9; i++) { float y0 = fy + (h - fy) * i * i / 81f, y1 = fy + (h - fy) * (i + 1) * (i + 1) / 81f; if (i % 2 == 0) Box(dc, b, 0, y0, w, y1, L(0x4FAD55, 0.72f * a)); }
        var line = Rgb(1, 1, 1, 0.9f * a); if (Count == 1) { var hp = new Vector2(w / 2f, h * 0.92f); var fst = new Vector2(w * 0.74f, h * 0.73f); var snd = new Vector2(w / 2f, h * 0.61f); var trd = new Vector2(w * 0.26f, h * 0.73f); Poly(dc, b, new List<Vector2> { hp, fst, snd, trd }, L(0xC7935B, a)); Outline(dc, b, new List<Vector2> { hp, fst, snd, trd }, line, 5f * u); Ln(dc, b, hp, new(w * 1.05f, h * 0.56f), line, 3f * u); Ln(dc, b, hp, new(-w * 0.05f, h * 0.56f), line, 3f * u); foreach (var q in new[] { hp, fst, snd, trd }) Box(dc, b, q.X - 10f * u, q.Y - 10f * u, q.X + 10f * u, q.Y + 10f * u, Rgb(1, 1, 1, a)); Oval(dc, b, new(w / 2f, h * 0.76f), 42f * u, 16f * u, L(0xB9804C, a)); }
        else { Outline(dc, b, new List<Vector2> { new(w * 0.2f, fy + 18f * u), new(w * 0.8f, fy + 18f * u), new(w * 0.98f, h * 0.97f), new(w * 0.02f, h * 0.97f) }, line, 4f * u); Ln(dc, b, new(w / 2f, fy + 18f * u), new(w / 2f, h * 0.97f), line, 4f * u); b.Color = line; dc.DrawEllipse(new Ellipse(new(w / 2f, h * 0.74f), 170f * u, 62f * u), b, 4f * u); foreach (int s in new[] { -1, 1 }) { Outline(dc, b, new List<Vector2> { new(w / 2f + s * w * 0.3f, fy + 18f * u), new(w / 2f + s * w * 0.21f, fy + 18f * u), new(w / 2f + s * w * 0.27f, h * 0.97f), new(w / 2f + s * w * 0.48f, h * 0.97f) }, line, 3f * u); } }
        foreach (int s in new[] { -1, 1 }) { var tw = new Vector2(w / 2f + s * w * 0.42f, h * 0.05f); Ln(dc, b, tw, new(tw.X, h * 0.42f), L(0x687380, a), 12f * u); Box(dc, b, tw.X - 88f * u, tw.Y - 30f * u, tw.X + 88f * u, tw.Y + 34f * u, L(0x2F3944, a)); for (int y = 0; y < 2; y++) for (int x = 0; x < 5; x++) { var q = tw + new Vector2((x - 2f) * 32f * u, (y - 0.5f) * 28f * u); Dot(dc, b, q, 10f * u, Rgb(1, 1, 0.86f, (Variant >= 2 || Background ? 1f : 0.45f) * a)); } if (Background && Variant >= 2) Poly(dc, b, new List<Vector2> { tw + new Vector2(-70f * u, 30f * u), tw + new Vector2(70f * u, 30f * u), new(w / 2f - s * w * 0.04f, h * 0.9f), new(w / 2f + s * w * 0.46f, h * 0.9f) }, Rgb(1, 1, 0.86f, 0.085f * a)); }
    }
}

public sealed class CourtroomPlugin : QuickShapePlugin { public override string Name => "演出：法廷"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CourtroomParameter(s); }
public sealed class CourtroomParameter : Q3BackParam
{
    public CourtroomParameter() : this(null) { }
    public CourtroomParameter(SharedDataStore? s) : base(s) { Count = 1; }
    static readonly Dictionary<string, ParamGuide> g = G("法廷", Tod, Veil, "Count=視点|1=正面　2=傍聴席から|1|2");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float fl = h * 0.74f; Box(dc, b, 0, 0, w, fl, L(0x8A6243, a)); for (int i = 0; i < 12; i++) { float x0 = i / 12f * w; Frame(dc, b, x0 + 7f * u, h * 0.04f, x0 + w / 12f - 7f * u, h * 0.28f, L(0x5F422D, a), 3f * u); Frame(dc, b, x0 + 7f * u, h * 0.3f, x0 + w / 12f - 7f * u, fl - 8f * u, L(0x5F422D, a), 3f * u); } Box(dc, b, 0, fl, w, h, L(0x6F4E36, a)); for (int i = 0; i < 11; i++) Ln(dc, b, new(i / 10f * w, fl), new(w / 2f + (i / 10f - 0.5f) * w * 1.6f, h), L(0x4F3726, 0.8f * a), 2f * u); var em = new Vector2(w / 2f, h * 0.15f); Dot(dc, b, em, 88f * u, L(0xC9A227, a)); Dot(dc, b, em, 70f * u, L(0x5F422D, a)); Ln(dc, b, em + new Vector2(-46f * u, -22f * u), em + new Vector2(46f * u, -22f * u), L(0xE6C65A, a), 5f * u); Ln(dc, b, em + new Vector2(0, -44f * u), em + new Vector2(0, 40f * u), L(0xE6C65A, a), 5f * u); foreach (int s in new[] { -1, 1 }) { Ln(dc, b, em + new Vector2(s * 44f * u, -22f * u), em + new Vector2(s * 58f * u, 16f * u), L(0xE6C65A, a), 2f * u); Ln(dc, b, em + new Vector2(s * 44f * u, -22f * u), em + new Vector2(s * 28f * u, 16f * u), L(0xE6C65A, a), 2f * u); Pie(dc, b, em + new Vector2(s * 43f * u, 14f * u), 18f * u, 0f, MathF.PI, L(0xE6C65A, a)); }
        bool gallery = Count == 2; float bx = w / 2f, by = gallery ? h * 0.47f : h * 0.56f, scale = gallery ? 0.72f : 1f; Box(dc, b, bx - 610f * u * scale, by - 110f * u * scale, bx + 610f * u * scale, fl + (gallery ? -50f * u : 28f * u), L(0x5A3B26, a)); Box(dc, b, bx - 640f * u * scale, by - 126f * u * scale, bx + 640f * u * scale, by - 100f * u * scale, L(0x7D5637, a)); for (int i = -2; i <= 2; i++) Frame(dc, b, bx + i * 230f * u * scale - 96f * u * scale, by - 72f * u * scale, bx + i * 230f * u * scale + 96f * u * scale, fl - (gallery ? 66f * u : 8f * u), L(0x3F2819, a), 4f * u); RBox(dc, b, new(bx, by - 205f * u * scale), 180f * u * scale, 170f * u * scale, 24f * u, L(0x2E211B, a)); var gav = new Vector2(bx + 230f * u * scale, by - 136f * u * scale); Oval(dc, b, gav + new Vector2(0, 8f * u), 42f * u, 11f * u, L(0x3F2819, a)); RBox(dc, b, gav - new Vector2(4f * u, 11f * u), 56f * u, 26f * u, 6f * u, L(0x7A4A25, a)); Ln(dc, b, gav - new Vector2(4f * u, 11f * u), gav + new Vector2(66f * u, -46f * u), L(0x7A4A25, a), 8f * u);
        if (!gallery) { var st = new Vector2(w / 2f, h * 0.88f); Box(dc, b, st.X - 150f * u, st.Y - 54f * u, st.X + 150f * u, h, L(0x4F3220, a)); Box(dc, b, st.X - 168f * u, st.Y - 68f * u, st.X + 168f * u, st.Y - 46f * u, L(0x7D5637, a)); Ln(dc, b, st + new Vector2(0, -68f * u), st + new Vector2(32f * u, -128f * u), L(0x2F3336, a), 5f * u); RBox(dc, b, st + new Vector2(36f * u, -138f * u), 20f * u, 34f * u, 9f * u, L(0x2F3336, a)); }
        else { Box(dc, b, 0, h * 0.74f, w, h * 0.78f, L(0x7D5637, a)); for (int r = 0; r < 2; r++) { float y = h * (0.82f + r * 0.09f); Box(dc, b, 0, y, w, y + 44f * u, L(0x4F3220, a)); Box(dc, b, 0, y - 10f * u, w, y + 6f * u, L(0x7D5637, a)); for (int i = 0; i < 9; i++) { var q = new Vector2((i + 0.5f + r * 0.5f) / 9f * w, y - 46f * u); Dot(dc, b, q, 34f * u, Rgb(0.08f, 0.07f, 0.07f, 0.85f * a)); Oval(dc, b, q + new Vector2(0, 56f * u), 62f * u, 40f * u, Rgb(0.08f, 0.07f, 0.07f, 0.85f * a)); } } }
    }
}

public sealed class ThroneRoomPlugin : QuickShapePlugin { public override string Name => "演出：城の玉座の間"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ThroneRoomParameter(s); }
public sealed class ThroneRoomParameter : Q3BackParam
{
    public ThroneRoomParameter() : this(null) { }
    public ThroneRoomParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xB71C1C); }
    protected override double DefAmount => 75;
    static readonly Dictionary<string, ParamGuide> g = G("玉座の間", Tod, Veil, "Amount=明るさ", "Color1=旗と絨毯の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float hz = h * 0.6f, ligh = 0.45f + amt * 0.55f; var o = new Vector2(w / 2f, hz); Box(dc, b, 0, 0, w, hz, L(0x6B7180, a)); for (int y = 0; y < 11; y++) for (int x = -1; x < 24; x++) Frame(dc, b, x / 22f * w + (y % 2) * w / 44f, y * hz / 10f, (x + 1) / 22f * w + (y % 2) * w / 44f, (y + 1) * hz / 10f, L(0x4C5160, 0.65f * a), 2f * u); Box(dc, b, 0, hz, w, h, L(0x8E8F96, a)); for (int y = 0; y < 8; y++) { float k0 = y / 8f, k1 = (y + 1) / 8f, y0 = hz + k0 * k0 * (h - hz), y1 = hz + k1 * k1 * (h - hz); for (int x = -8; x <= 8; x++) if ((x + y) % 2 == 0) { Poly(dc, b, new List<Vector2> { new(w / 2f + x * (70f + k0 * 200f) * u, y0), new(w / 2f + (x + 1) * (70f + k0 * 200f) * u, y0), new(w / 2f + (x + 1) * (70f + k1 * 200f) * u, y1), new(w / 2f + x * (70f + k1 * 200f) * u, y1) }, L(0x6B6C74, a)); } } Poly(dc, b, new List<Vector2> { new(w / 2f - 48f * u, hz), new(w / 2f + 48f * u, hz), new(w * 0.78f, h), new(w * 0.22f, h) }, Col(Gfx.Mix(Color1, Hex(0x000000), 0.1f), a)); foreach (int s in new[] { -1, 1 }) Ln(dc, b, new(w / 2f + s * 48f * u, hz), new(w / 2f + s * w * 0.28f, h), L(0xE6C65A, a), 5f * u);
        foreach (int s in new[] { -1, 1 }) for (int i = 2; i >= 0; i--) { float x = w / 2f + s * (250f + i * 280f) * u, W2 = (42f + i * 21f) * u, top = 0f, bot = hz + (18f + i * 58f) * u; Box(dc, b, x - W2, top, x + W2, bot, L(0xA9ACB5, a)); Box(dc, b, x - W2 * 0.45f, top, x - W2 * 0.2f, bot, L(0xD0D2D8, 0.65f * a)); Box(dc, b, x - W2 * 1.28f, bot - 20f * u, x + W2 * 1.28f, bot + 12f * u, L(0x7D808A, a)); float fw = W2 * 0.86f, fy = h * 0.09f, wv = MathF.Sin(t * 1.4f + i) * 7f * u; Poly(dc, b, new List<Vector2> { new(x - fw, fy), new(x + fw, fy), new(x + fw + wv, fy + 260f * u), new(x + wv, fy + 215f * u), new(x - fw + wv, fy + 260f * u) }, Col(Color1, a)); Star(dc, b, new(x + wv * 0.4f, fy + 92f * u), fw * 0.46f, 5, 0.45f, -MathF.PI / 2f, L(0xE6C65A, a)); var torch = new Vector2(x, h * 0.42f); Ln(dc, b, torch, torch + new Vector2(0, 46f * u), L(0x4A3524, a), 9f * u); float fl = 0.8f + 0.2f * MathF.Sin(t * 11f + i * 3f + s); Dot(dc, b, torch - new Vector2(0, 12f * u), 56f * u * fl, Rgb(1, 0.55f, 0.15f, 0.16f * ligh * a)); Oval(dc, b, torch - new Vector2(0, 16f * u), 13f * u * fl, 27f * u * fl, Rgb(1, 0.7f, 0.2f, a)); }
        var th = o + new Vector2(0, -95f * u); Box(dc, b, th.X - 215f * u, hz - 20f * u, th.X + 215f * u, hz + 32f * u, L(0x7D808A, a)); Poly(dc, b, new List<Vector2> { th + new Vector2(-105f * u, 90f * u), th + new Vector2(-105f * u, -170f * u), th + new Vector2(-50f * u, -235f * u), th + new Vector2(0, -190f * u), th + new Vector2(50f * u, -235f * u), th + new Vector2(105f * u, -170f * u), th + new Vector2(105f * u, 90f * u) }, L(0xD4AF37, a)); Box(dc, b, th.X - 78f * u, th.Y - 145f * u, th.X + 78f * u, th.Y + 45f * u, Col(Color1, a)); Box(dc, b, th.X - 132f * u, th.Y + 42f * u, th.X + 132f * u, th.Y + 76f * u, L(0xD4AF37, a)); RBox(dc, b, th + new Vector2(0, 28f * u), 170f * u, 36f * u, 12f * u, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.14f), a)); foreach (int s in new[] { -1, 1 }) { Box(dc, b, th.X + s * 124f * u - 16f * u, th.Y - 18f * u, th.X + s * 124f * u + 16f * u, th.Y + 118f * u, L(0xB8922E, a)); Dot(dc, b, th + new Vector2(s * 124f * u, -22f * u), 21f * u, L(0xE6C65A, a)); }
        if (ligh < 0.99f) Box(dc, b, 0, 0, w, h, Rgb(0.02f, 0.02f, 0.05f, (1f - ligh) * 0.72f * a));
    }
}

public sealed class LimestoneCavePlugin : QuickShapePlugin { public override string Name => "演出：鍾乳洞"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LimestoneCaveParameter(s); }
public sealed class LimestoneCaveParameter : Q3BackParam
{
    public LimestoneCaveParameter() : this(null) { }
    public LimestoneCaveParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x56C8FF); Count = 8; }
    static readonly Dictionary<string, ParamGuide> g = G("鍾乳洞", Tod, Veil, "Count=しずくの数||0|40", "Color1=光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float wl = h * 0.78f; var glow = Color1; Box(dc, b, 0, 0, w, h, Rgb(0.05f, 0.06f, 0.09f, a)); using (var rg = Gfx.Radial(dc, new(w / 2f, h * 0.5f), w * 0.6f, (0f, Col(glow, 0.65f * a)), (0.5f, Col(glow, 0.2f * a)), (1f, Col(glow, 0f)))) dc.FillRectangle(new Vortice.RawRectF(0, 0, w, h), rg); for (int l = 0; l < 3; l++) { float depth = 1f - l * 0.26f; var col = Rgb(0.28f * depth, 0.27f * depth, 0.3f * depth, a); int n = 16 + l * 5; var ceil = new List<Vector2> { new(0, 0), new(w, 0) }; for (int i = n; i >= 0; i--) { float x = i / (float)n * w, down = (40f + Rnd(i * 7 + l * 13) * (155f + l * 62f)) * u; ceil.Add(new(x + w / n * 0.5f, down * 0.35f)); ceil.Add(new(x, down)); ceil.Add(new(x - w / n * 0.5f, down * 0.35f)); } Poly(dc, b, ceil, col); for (int i = 0; i < n; i++) { float x = Rnd(i * 11 + l) * w, L2 = (130f + Rnd(i * 5 + l + 3) * 240f) * u, W2 = (16f + Rnd(i + l + 9) * 26f) * u; Poly(dc, b, new List<Vector2> { new(x - W2, 0), new(x + W2, 0), new(x + W2 * 0.3f, L2 * 0.72f), new(x, L2), new(x - W2 * 0.3f, L2 * 0.72f) }, col); if (l == 0) Ln(dc, b, new(x - W2 * 0.3f, 10f * u), new(x - 2f * u, L2 * 0.84f), Col(glow, 0.25f * a), 3f * u); } }
        for (int i = 0; i < 12; i++) { float x = Rnd(i + 70) * w, H2 = (70f + Rnd(i + 3) * 170f) * u, W2 = (28f + Rnd(i + 5) * 46f) * u; Poly(dc, b, new List<Vector2> { new(x - W2, wl + 12f * u), new(x - W2 * 0.25f, wl - H2 * 0.72f), new(x, wl - H2), new(x + W2 * 0.25f, wl - H2 * 0.72f), new(x + W2, wl + 12f * u) }, Rgb(0.2f, 0.2f, 0.23f, a)); } Shade(dc, new(0, wl), new(0, h), Col(glow, 0.6f * a), Rgb(0.02f, 0.06f, 0.11f, a), 0, wl, w, h); for (int i = 0; i < 26; i++) { float y = wl + (Rnd(i) * 0.92f + 0.04f) * (h - wl), x = Gfx.Mod(Rnd(i + 5) * w + MathF.Sin(t + i) * 26f * u, w), L2 = (34f + Rnd(i + 8) * 130f) * u; Ln(dc, b, new(x - L2, y), new(x + L2, y), Rgb(1, 1, 1, 0.22f * a), 2f * u); }
        for (int i = 0; i < Math.Clamp(Count, 0, 40); i++) { float x = Rnd(i + 120) * w, cy = Gfx.Mod(t * 0.42f + Rnd(i + 121), 1f), y = Lerp(h * 0.24f, wl, cy * cy); if (cy < 0.94f) Oval(dc, b, new(x, y), 4f * u, 9f * u, Rgb(0.82f, 0.95f, 1f, 0.9f * a)); else { float e = (cy - 0.94f) / 0.06f; b.Color = Rgb(1, 1, 1, (1f - e) * 0.85f * a); dc.DrawEllipse(new Ellipse(new(x, wl + 8f * u), (5f + e * 54f) * u, (2f + e * 13f) * u), b, 2f * u); } }
    }
}

public sealed class SnowOnsenPlugin : QuickShapePlugin { public override string Name => "演出：雪国の露天風呂"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SnowOnsenParameter(s); }
public sealed class SnowOnsenParameter : Q3BackParam
{
    public SnowOnsenParameter() : this(null) { }
    public SnowOnsenParameter(SharedDataStore? s) : base(s) { Variant = 3; Count = 70; }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("雪国の露天風呂", Tod, Veil, "Count=雪の量||0|300", "Amount=湯けむり");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        SkyFill(dc, w, h, a); Stars(dc, b, w, h * 0.42f, u, t, a); float gr = h * 0.5f; for (int l = 0; l < 2; l++) { var m = new List<Vector2> { new(0, gr) }; for (int i = 0; i <= 32; i++) { float k = i / 32f; m.Add(new(k * w, gr - (MathF.Abs(MathF.Sin(k * (4f + l * 3f) + l)) * (130f - l * 34f) + 34f) * u)); } m.Add(new(w, gr)); Poly(dc, b, m, L(l == 0 ? 0xB8C7D6 : 0xE7EEF4, a)); } for (int i = 0; i < 10; i++) { float x = Rnd(i + 8) * w, S = (54f + Rnd(i) * 54f) * u, by = gr + Rnd(i + 3) * 30f * u; for (int k = 0; k < 3; k++) { float W2 = S * (1f - k * 0.24f), y = by - k * S * 0.62f; Poly(dc, b, new List<Vector2> { new(x - W2, y), new(x + W2, y), new(x, y - S * 1.05f) }, L(0x2F5D4C, a)); Poly(dc, b, new List<Vector2> { new(x - W2 * 0.44f, y - S * 0.58f), new(x + W2 * 0.44f, y - S * 0.58f), new(x, y - S * 1.05f) }, L(0xF4F8FB, a)); } } Box(dc, b, 0, gr, w, h, L(0xF1F5F8, a));
        var o = new Vector2(w / 2f, h * 0.76f); float rx = w * 0.42f, ry = h * 0.2f; using (var wtr = Gfx.Radial(dc, o, rx, (0f, L(0x8EE3DE, a)), (1f, L(0x3A9EB0, a)))) dc.FillEllipse(new Ellipse(o, rx, ry), wtr); for (int i = 0; i < 16; i++) { float y = o.Y + (Rnd(i) - 0.5f) * ry * 1.5f, x = o.X + (Rnd(i + 3) - 0.5f) * rx * 1.4f + MathF.Sin(t + i) * 20f * u, L2 = (34f + Rnd(i + 7) * 70f) * u; Ln(dc, b, new(x - L2, y), new(x + L2, y), Rgb(1, 1, 1, 0.3f * a), 2f * u); } for (int i = 0; i < 34; i++) { float th = i / 34f * MathF.Tau, S = (42f + Rnd(i) * 40f) * u; var q = o + new Vector2(MathF.Cos(th) * rx, MathF.Sin(th) * ry); Oval(dc, b, q, S * 1.15f, S * 0.75f, L(i % 2 == 0 ? 0x565B62 : 0x70757B, a)); if (MathF.Sin(th) < 0.4f) Oval(dc, b, q - new Vector2(0, S * 0.3f), S * 0.86f, S * 0.36f, L(0xFFFFFF, a)); }
        for (int i = 0; i < 26; i++) { float k = Gfx.Mod(t * (0.1f + Rnd(i) * 0.09f) + Rnd(i + 3), 1f), x = o.X + (Rnd(i + 5) - 0.5f) * rx * 1.65f + MathF.Sin(k * 6f + i) * 52f * u, y = o.Y + ry * 0.2f - k * h * 0.56f, R = (80f + k * 155f) * u; using var st = Gfx.Radial(dc, new(x, y), R, (0f, Rgb(1, 1, 1, amt * 0.3f * MathF.Sin(k * MathF.PI) * a)), (1f, Rgb(1, 1, 1, 0))); dc.FillEllipse(new Ellipse(new(x, y), R, R), st); } for (int i = 0; i < Math.Clamp(Count, 0, 300); i++) Dot(dc, b, new(Gfx.Mod(Rnd(i) * w + MathF.Sin(t * 0.8f + i) * 18f * u, w), Gfx.Mod(Rnd(i + 7) * h + t * (32f + Rnd(i + 2) * 54f) * u, h)), (1.6f + Rnd(i + 3) * 3.6f) * u, Rgb(1, 1, 1, 0.92f * a));
    }
}

public sealed class HarborLighthousePlugin : QuickShapePlugin { public override string Name => "演出：港の灯台"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HarborLighthouseParameter(s); }
public sealed class HarborLighthouseParameter : Q3BackParam
{
    public HarborLighthouseParameter() : this(null) { }
    public HarborLighthouseParameter(SharedDataStore? s) : base(s) { Variant = 3; }
    protected override double DefAmount => 45;
    static readonly Dictionary<string, ParamGuide> g = G("港の灯台", Tod, Veil, "Amount=光の回る速さ");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Scene(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float w, float h, float u, float t, float a, float amt)
    {
        float sea = h * 0.62f, ang = t * (0.25f + amt * 1.6f); SkyFill(dc, w, h, a); Stars(dc, b, w, sea, u, t, a); Clouds(dc, b, w, h * 0.65f, u, t, a); var sCol = SkyCols(a); Shade(dc, new(0, sea), new(0, h), L(0x3B83B8, a), L(0x123C5E, a), 0, sea, w, h); for (int i = 0; i < 44; i++) { float y = sea + Rnd(i) * (h - sea), x = Gfx.Mod(Rnd(i + 3) * w + MathF.Sin(t * 0.8f + i) * 30f * u, w), L2 = (24f + Rnd(i + 7) * 76f) * u * (0.45f + (y - sea) / (h - sea)); Ln(dc, b, new(x - L2, y), new(x + L2, y), Rgb(1, 1, 1, 0.26f * a), 2f * u); }
        var isle = new List<Vector2> { new(w * 0.52f, sea + 36f * u), new(w * 0.62f, sea - 24f * u), new(w * 0.72f, sea - 76f * u), new(w * 0.9f, sea - 104f * u), new(w, sea - 92f * u), new(w, sea + 72f * u) }; Poly(dc, b, isle, L(0x4A4B50, a)); var base0 = new Vector2(w * 0.82f, sea - 96f * u); float H2 = 430f * u, W0 = 72f * u, W1 = 46f * u; int bands = 5; for (int i = 0; i < bands; i++) { float k0 = i / (float)bands, k1 = (i + 1f) / bands; Poly(dc, b, new List<Vector2> { base0 + new Vector2(-Lerp(W0, W1, k0), -k0 * H2), base0 + new Vector2(Lerp(W0, W1, k0), -k0 * H2), base0 + new Vector2(Lerp(W0, W1, k1), -k1 * H2), base0 + new Vector2(-Lerp(W0, W1, k1), -k1 * H2) }, L(i % 2 == 0 ? 0xF4F4F0 : 0xD32F2F, a)); }
        var lamp = base0 - new Vector2(0, H2 + 42f * u); Box(dc, b, lamp.X - 62f * u, lamp.Y + 34f * u, lamp.X + 62f * u, lamp.Y + 45f * u, L(0x2F3336, a)); Box(dc, b, lamp.X - 36f * u, lamp.Y - 34f * u, lamp.X + 36f * u, lamp.Y + 36f * u, Rgb(1, 0.92f, 0.58f, a)); Poly(dc, b, new List<Vector2> { lamp + new Vector2(-48f * u, -34f * u), lamp + new Vector2(48f * u, -34f * u), lamp + new Vector2(0, -88f * u) }, L(0xB71C1C, a));
        float beamA = Variant == 3 ? 0.42f : Variant == 2 ? 0.22f : 0.08f, L0 = w * 1.05f, swing = MathF.Cos(ang), spread = 0.07f + (1f - MathF.Abs(swing)) * 0.08f, fade = 0.45f + 0.55f * MathF.Abs(swing); foreach (int s in new[] { -1, 1 }) { float dir = s * swing, reach = L0 * MathF.Abs(swing); if (reach < 6f * u) continue; var tipC = lamp + new Vector2(dir >= 0 ? reach : -reach, 72f * u); using var bm = Gfx.Linear(dc, lamp, tipC, (0f, Rgb(1, 0.95f, 0.68f, beamA * fade * a)), (1f, Rgb(1, 0.95f, 0.68f, 0))); Gfx.FillPoly(dc, bm, new List<Vector2> { lamp, tipC + new Vector2(0, -reach * spread), tipC + new Vector2(0, reach * spread) }); } Dot(dc, b, lamp, 62f * u, Rgb(1, 0.95f, 0.68f, 0.28f * a));
        var ship = new Vector2(Gfx.Mod(w * 0.1f + t * 12f * u, w * 0.6f), sea + 72f * u + MathF.Sin(t * 1.3f) * 8f * u); float roll = MathF.Sin(t * 1.1f) * 0.05f; var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(roll, ship) * prev; Poly(dc, b, new List<Vector2> { ship + new Vector2(-140f * u, -22f * u), ship + new Vector2(150f * u, -22f * u), ship + new Vector2(100f * u, 34f * u), ship + new Vector2(-104f * u, 34f * u) }, L(0x263238, a)); Box(dc, b, ship.X - 74f * u, ship.Y - 72f * u, ship.X + 46f * u, ship.Y - 22f * u, L(0xE8E8E2, a)); for (int i = 0; i < 4; i++) Box(dc, b, ship.X + (-62f + i * 26f) * u, ship.Y - 60f * u, ship.X + (-46f + i * 26f) * u, ship.Y - 42f * u, Variant == 3 ? Rgb(1, 0.86f, 0.48f, a) : L(0x5B8AA8, a)); Box(dc, b, ship.X - 40f * u, ship.Y - 108f * u, ship.X - 14f * u, ship.Y - 72f * u, L(0xD32F2F, a)); dc.Transform = prev;
    }
}
