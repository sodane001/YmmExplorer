using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 531〜550: SF・ファンタジー・魔法

public sealed class AlchemyCauldronPlugin : QuickShapePlugin { public override string Name => "演出：錬金の釜"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AlchemyCauldronParameter(s); }
public sealed class AlchemyCauldronParameter : Q3Param
{
    public AlchemyCauldronParameter() : this(null) { }
    public AlchemyCauldronParameter(SharedDataStore? s) : base(s) { Count = 26; Color1 = Hex(0x56F06A); Color2 = Hex(0x444A56); Color3 = Hex(0xFF7A1A); }
    static readonly Dictionary<string, ParamGuide> g = G("錬金の釜", "Count=泡の量||1|100", "Speed=速さ", "Color1=液の色", "Color2=釜の色", "Color3=火の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c); var o = P0(c) + new Vector2(0, 70f * u);
        for (int i = 0; i < 18; i++) { float x = (i - 8.5f) * 34f * u, H = (70f + MathF.Abs(MathF.Sin(t * 7f + i * 1.8f)) * 100f) * u; Poly(dc, b, new List<Vector2> { o + new Vector2(x - 22f * u, 330f * u), o + new Vector2(x + MathF.Sin(t * 5f + i) * 12f * u, 330f * u - H), o + new Vector2(x + 22f * u, 330f * u) }, i % 2 == 0 ? C3(a) : Rgb(1, 0.86f, 0.25f, a)); }
        Dot(dc, b, o - new Vector2(0, 90f * u), 330f * u, C1(0.08f * a)); Pie(dc, b, o - new Vector2(0, 80f * u), 330f * u, 0f, MathF.PI, C2(a)); RBox(dc, b, o - new Vector2(0, 88f * u), 720f * u, 68f * u, 30f * u, Col(Gfx.Mix(Color2, Hex(0xFFFFFF), 0.14f), a)); Oval(dc, b, o - new Vector2(0, 97f * u), 315f * u, 40f * u, C1(a)); foreach (int s in new[] { -1, 1 }) Ring(dc, b, o + new Vector2(s * 342f * u, 25f * u), 42f * u, C2(a), 14f * u);
        for (int i = 0; i < Math.Clamp(Count, 1, 100); i++) { float k = Gfx.Mod(t * (0.3f + Rnd(i) * 0.45f) + Rnd(i + 3), 1f), x = (Rnd(i + 7) - 0.5f) * 560f * u + MathF.Sin(k * 7f + i) * 16f * u, r = (6f + Rnd(i + 2) * 22f) * u * (1f + k * 0.35f); var q = o + new Vector2(x, -102f * u - k * 390f * u); Dot(dc, b, q, r, C1(0.22f * a * (1f - k))); Ring(dc, b, q, r, C1(a * (1f - k)), 3f * u); Dot(dc, b, q - new Vector2(r, r) * 0.32f, r * 0.2f, Rgb(1, 1, 1, a * (1f - k))); }
    }
}

public sealed class DragonBreathPlugin : QuickShapePlugin { public override string Name => "演出：ドラゴンの吐く炎"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DragonBreathParameter(s); }
public sealed class DragonBreathParameter : Q3Param
{
    public DragonBreathParameter() : this(null) { }
    public DragonBreathParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFF4A12); Color2 = Hex(0xFFC21F); Color3 = Hex(0xFFFFE6); }
    protected override double DefAmount => 55;
    static readonly Dictionary<string, ParamGuide> g = G("ドラゴンの炎", "Variant=向き|1=左から右　2=右から左　3=上から下|1|3", "Amount=太さ", "Speed=勢い", "Color1=外側の色", "Color2=内側の色", "Color3=中心の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c), thick = (90f + Amt(c) * 320f) * u, reach = DrawKit.EaseOut(Sub(c.Time, 0f, 0.45f)); var start = Variant switch { 2 => new Vector2(w + 40f * u, h * 0.5f), 3 => new Vector2(w * 0.5f, -40f * u), _ => new Vector2(-40f * u, h * 0.5f) }; var dir = Variant switch { 2 => new Vector2(-1, 0), 3 => new Vector2(0, 1), _ => new Vector2(1, 0) }; var n = new Vector2(-dir.Y, dir.X); float L = (Variant == 3 ? h : w) * 1.08f;
        for (int layer = 0; layer < 3; layer++) for (int i = 0; i < 95; i++) { float k = Gfx.Mod(t * (0.85f + Rnd(i + layer) * 0.65f) + Rnd(i * 3 + layer), 1f), d = k * reach, side = (Rnd(i + 20 + layer) - 0.5f) * 2f + MathF.Sin(t * 5f + i) * 0.14f, spread = thick * (0.18f + d) * (1f - layer * 0.28f), R = thick * (0.12f + d * 0.42f) * (1f - layer * 0.3f); var q = start + dir * d * L + n * side * spread * 0.5f; var col = layer == 0 ? C1((0.3f + (1f - k) * 0.38f) * a) : layer == 1 ? C2((0.28f + (1f - k) * 0.42f) * a) : C3((0.22f + (1f - k) * 0.48f) * a); Dot(dc, b, q, R, col); }
    }
}

public sealed class ElementOrbsPlugin : QuickShapePlugin { public override string Name => "演出：4属性のオーブ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ElementOrbsParameter(s); }
public sealed class ElementOrbsParameter : Q3Param
{
    public ElementOrbsParameter() : this(null) { }
    public ElementOrbsParameter(SharedDataStore? s) : base(s) { Text = "火,水,風,土"; Color1 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, uint> cols = new() { ["火"] = 0xFF5A2B, ["水"] = 0x2FA8FF, ["風"] = 0x5CF0A8, ["土"] = 0xC48B3F, ["光"] = 0xFFE66B, ["闇"] = 0x8A4DFF };
    static readonly Dictionary<string, ParamGuide> g = G("4属性のオーブ", "Text=玉の属性|カンマ区切り。火 水 風 土 光 闇", "Speed=回る速さ", "Color1=光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Items.Take(8).ToList(); if (r.Count == 0) return; float u = U(c), t = c.Time * Spd(c), R = 300f * u * DrawKit.EaseOut(Intro(c, 0f, 0.6f)); var o = P0(c); var ring = new List<Vector2>(); for (int i = 0; i <= 72; i++) { float th = i / 72f * MathF.Tau; ring.Add(o + new Vector2(MathF.Cos(th) * R, MathF.Sin(th) * R * 0.46f)); } Path(dc, b, ring, C1(0.32f * a), 3f * u);
        foreach (var (i, th) in Enumerable.Range(0, r.Count).Select(i => (i, t + i * MathF.Tau / r.Count)).OrderBy(x => MathF.Sin(x.Item2)))
        {
            float depth = 0.72f + 0.28f * (MathF.Sin(th) + 1f) / 2f, S = 76f * u * depth; var q = o + new Vector2(MathF.Cos(th) * R, MathF.Sin(th) * R * 0.46f); var col = cols.TryGetValue(r[i], out var hx) ? Hex(hx) : Color1; Dot(dc, b, q, S * 1.5f, Col(col, 0.16f * a)); Dot(dc, b, q, S, Col(col, a)); Dot(dc, b, q - new Vector2(S, S) * 0.3f, S * 0.32f, Rgb(1, 1, 1, 0.68f * a)); Say(dc, b, r[i], S * 0.72f, q + new Vector2(0, S * 0.1f), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.5f * a), 2f * u);
        }
    }
}

public sealed class CurseGlyphsPlugin : QuickShapePlugin { public override string Name => "演出：呪いの紋様が広がる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CurseGlyphsParameter(s); }
public sealed class CurseGlyphsParameter : Q3Param
{
    public CurseGlyphsParameter() : this(null) { }
    public CurseGlyphsParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x8B2BE2); Color2 = Hex(0x14051F); }
    static readonly Dictionary<string, ParamGuide> g = G("呪いの紋様", "Speed=広がる速さ", "Seed=シード", "Color1=紋様の色", "Color2=影の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)), p = DrawKit.EaseOut(Sub(t, 0f, 2.6f)), pulse = 0.7f + 0.3f * MathF.Sin(c.Time * 4f); var o = P0(c) + new Vector2(0, 260f * u); using (var rg = Gfx.Radial(dc, o, 720f * u * p + 1f, (0f, C2(0.92f * a)), (0.75f, C2(0.55f * a)), (1f, C2(0f)))) dc.FillEllipse(new Ellipse(o, 720f * u * p + 1f, 300f * u * p + 1f), rg);
        for (int s = 0; s < 18; s++) { float th = s / 18f * MathF.Tau + Rnd(s + Seed) * 0.2f; var pts = new List<Vector2>(); for (int i = 0; i <= 32; i++) { float k = i / 32f, rr = k * (500f + Rnd(s + 4 + Seed) * 220f) * u, wig = th + MathF.Sin(k * 8f + s) * 0.24f * k; pts.Add(o + new Vector2(MathF.Cos(wig) * rr, MathF.Sin(wig) * rr * 0.42f)); } var part = BubbleKit.PartByLength(pts, p); Path(dc, b, part, C1(pulse * a), 5f * u); if (part.Count > 2) { var e = part[^1]; Star(dc, b, e, 16f * u, 4, 0.35f, th, C1(a)); } for (int d = 6; d < part.Count; d += 7) { var q = part[d]; Ring(dc, b, q, 8f * u, C1(0.75f * a), 2f * u); } }
        foreach (float rr in new[] { 130f, 250f }) { b.Color = C1(0.62f * pulse * a); dc.DrawEllipse(new Ellipse(o, rr * u * p + 0.1f, rr * 0.42f * u * p + 0.1f), b, 3f * u); }
    }
}

public sealed class SealingTalismanPlugin : QuickShapePlugin { public override string Name => "演出：封印のお札"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SealingTalismanParameter(s); }
public sealed class SealingTalismanParameter : Q3Param
{
    public SealingTalismanParameter() : this(null) { }
    public SealingTalismanParameter(SharedDataStore? s) : base(s) { Text = "封"; Count = 5; Color1 = Hex(0xFFF1C9); Color2 = Hex(0xC8102E); Color3 = Hex(0xFFE36E); }
    static readonly Dictionary<string, ParamGuide> g = G("封印のお札", "Text=札の文字", "Count=札の数||1|12", "Color1=札の色", "Color2=文字の色", "Color3=光の鎖の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 1, 12); float u = U(c), t = c.Time, R = 300f * u; var o = P0(c); var at = new Vector2[n];
        for (int i = 0; i < n; i++) at[i] = Pol(o, R, -MathF.PI / 2f + i * MathF.Tau / n);
        float chain = Sub(t, 0.35f + n * 0.13f, 0.45f); if (chain > 0f) for (int i = 0; i < n; i++) { var p = at[i]; var q = at[(i + 2) % n]; if (n < 5) q = at[(i + 1) % n]; float len = (q - p).Length(); var d = Vector2.Normalize(q - p + new Vector2(0.001f, 0)); int links = (int)(len / (34f * u)); for (int k = 0; k < links * chain; k++) { var m = p + d * (k + 0.5f) * 34f * u; b.Color = C3((0.72f + 0.28f * MathF.Sin(t * 5f + k)) * a); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(MathF.Atan2(d.Y, d.X), m) * prev; dc.DrawEllipse(new Ellipse(m, 20f * u, k % 2 == 0 ? 11f * u : 4f * u), b, 4f * u); dc.Transform = prev; } }
        for (int i = 0; i < n; i++) { float k = Sub(t, 0.05f + i * 0.13f, 0.28f); if (k <= 0f) continue; float e = 1f - (1f - k) * (1f - k), th = -MathF.PI / 2f + i * MathF.Tau / n; var from = Pol(o, 1300f * u, th + 1f); var q = Vector2.Lerp(from, at[i], e); float rot = (1f - e) * 8f + MathF.Sin(i * 2.4f) * 0.14f; var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(rot, q) * prev; RBox(dc, b, q, 120f * u, 300f * u, 4f * u, C1(a)); RFrame(dc, b, q, 100f * u, 276f * u, 3f * u, C2(a), 4f * u); Say(dc, b, Str, 72f * u, q - new Vector2(0, 42f * u), C2(a), maxW: 92f * u); for (int l = 0; l < 3; l++) Path(dc, b, new List<Vector2> { q + new Vector2(-30f * u, (50f + l * 28f) * u), q + new Vector2(-8f * u, (38f + l * 28f) * u), q + new Vector2(10f * u, (60f + l * 28f) * u), q + new Vector2(30f * u, (45f + l * 28f) * u) }, C2(a), 4f * u); dc.Transform = prev; if (k >= 1f) { float f = Sub(t, 0.33f + i * 0.13f, 0.3f); Ring(dc, b, q, (70f + f * 120f) * u, C3(a * (1f - f)), 6f * u); } }
    }
}

public sealed class PetrifyPlugin : QuickShapePlugin { public override string Name => "演出：石化"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PetrifyParameter(s); }
public sealed class PetrifyParameter : Q3Param
{
    public PetrifyParameter() : this(null) { }
    public PetrifyParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x858A8F); Color2 = Hex(0x34383D); }
    protected override double DefAmount => 45;
    static readonly Dictionary<string, ParamGuide> g = G("石化", "Variant=進む向き|1=下から　2=上から　3=左から　4=右から|1|4", "Amount=進む速さ", "Seed=シード", "Color1=石の色", "Color2=ひびの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), dur = 3.2f - Amt(c) * 2.7f, p = DrawKit.EaseOut(Sub(c.Time, 0f, dur)); int V = Variant; bool Stone(float x, float y, float ex = 0f) => (V switch { 2 => y / h, 3 => x / w, 4 => 1f - x / w, _ => 1f - y / h }) + ex < p * 1.12f;
        var edge = new List<Vector2>(); for (int i = 0; i <= 80; i++) { float k = i / 80f, wob = (MathF.Sin(k * 23f + Seed) * 0.016f + MathF.Sin(k * 53f + Seed * 2) * 0.008f), v = p * 1.12f + wob; edge.Add(V switch { 2 => new(k * w, v * h), 3 => new(v * w, k * h), 4 => new((1f - v) * w, k * h), _ => new(k * w, (1f - v) * h) }); } if (V <= 2) { edge.Add(new(w, V == 2 ? 0 : h)); edge.Add(new(0, V == 2 ? 0 : h)); } else { edge.Add(new(V == 3 ? 0 : w, h)); edge.Add(new(V == 3 ? 0 : w, 0)); } Poly(dc, b, edge, C1(0.94f * a));
        for (int i = 0; i < 420; i++) { float x = Rnd(i + Seed) * w, y = Rnd(i + 900 + Seed) * h; if (Stone(x, y, 0.02f)) Dot(dc, b, new(x, y), (2f + Rnd(i + 44) * 11f) * u, i % 2 == 0 ? C2(0.2f * a) : Rgb(1, 1, 1, 0.08f * a)); }
        for (int s = 0; s < 18; s++) { var q = new Vector2(Rnd(s * 3 + Seed + 7) * w, Rnd(s * 5 + Seed + 8) * h); var pts = new List<Vector2> { q }; for (int i = 0; i < 7; i++) { float th = Rnd(s * 11 + i + Seed) * MathF.Tau; q += new Vector2(MathF.Cos(th), MathF.Sin(th)) * (28f + Rnd(i + s) * 58f) * u; if (!Stone(q.X, q.Y, 0.03f)) break; pts.Add(q); } if (pts.Count > 1) Path(dc, b, pts, C2(a), 4f * u); }
    }
}

public sealed class TimeRewindPlugin : QuickShapePlugin { public override string Name => "演出：時を戻す"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TimeRewindParameter(s); }
public sealed class TimeRewindParameter : Q3Param
{
    public TimeRewindParameter() : this(null) { }
    public TimeRewindParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x74E7FF); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("時を戻す", "Amount=針の速さ", "Background=暗くする", "Color1=色", "Color2=針の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), t = c.Time, speed = 1f + Amt(c) * 12f, R = 330f * u; var o = P0(c); if (Background) Box(dc, b, 0, 0, w, h, Rgb(0.02f, 0.05f, 0.1f, 0.48f * a)); for (int i = 0; i < 36; i++) { float y = Rnd(i) * h, x = Gfx.Mod(Rnd(i + 3) * w * 1.3f + t * (800f + Rnd(i + 7) * 700f) * u, w * 1.3f) - w * 0.15f, L = (110f + Rnd(i + 9) * 340f) * u; Ln(dc, b, new(x, y), new(x - L, y), C1(0.34f * a), 3f * u); Poly(dc, b, new List<Vector2> { new(x - L - 18f * u, y), new(x - L, y - 7f * u), new(x - L, y + 7f * u) }, C1(0.45f * a)); }
        Ring(dc, b, o, R, C1(a), 9f * u); Ring(dc, b, o, R * 0.9f, C1(0.45f * a), 3f * u); for (int i = 0; i < 12; i++) { float th = i * MathF.PI / 6f; Ln(dc, b, Pol(o, R * (i % 3 == 0 ? 0.74f : 0.82f), th), Pol(o, R * 0.88f, th), C1(a), (i % 3 == 0 ? 8f : 4f) * u); }
        float mAng = -MathF.PI / 2f - t * speed, hAng = -MathF.PI / 2f - t * speed / 12f; Ln(dc, b, o, Pol(o, R * 0.48f, hAng), C2(a), 13f * u); Ln(dc, b, o, Pol(o, R * 0.76f, mAng), C2(a), 7f * u); Dot(dc, b, o, 16f * u, C1(a)); float rr = R * 1.2f, sw = -MathF.PI / 2f - t * 2.2f; Arc(dc, b, o, rr, sw, sw - 4.8f, C1(0.85f * a), 8f * u); var tip = Pol(o, rr, sw - 4.8f); var tan = new Vector2(MathF.Sin(sw - 4.8f), -MathF.Cos(sw - 4.8f)); var nn = new Vector2(-tan.Y, tan.X); Poly(dc, b, new List<Vector2> { tip + tan * 34f * u, tip + nn * 25f * u, tip - nn * 25f * u }, C1(a));
    }
}

public sealed class TelepathyWavePlugin : QuickShapePlugin { public override string Name => "演出：テレパシーの波"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TelepathyWaveParameter(s); }
public sealed class TelepathyWaveParameter : Q3Param
{
    public TelepathyWaveParameter() : this(null) { }
    public TelepathyWaveParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xC77DFF); Color2 = Hex(0x56E8FF); }
    protected override double DefAmount => 22;
    protected override double DefAmount2 => 78;
    static readonly Dictionary<string, ParamGuide> g = G("テレパシー", "Amount=左の点のX", "Amount2=右の点のX", "Variant=波の形|1=サイン波　2=ジグザグ　3=輪|1|3", "Speed=速さ", "Color1=色1", "Color2=色2");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), y = P0(c).Y; var A = new Vector2(Amt(c) * c.Width, y); var B = new Vector2(Amt2(c) * c.Width, y); var d = B - A; float L = d.Length(); if (L < 2f) return; d /= L; var n = new Vector2(-d.Y, d.X);
        foreach (var (q, col) in new[] { (A, Color1), (B, Color2) }) { Dot(dc, b, q, 74f * u, Col(col, 0.92f * a)); Dot(dc, b, q - new Vector2(0, 5f * u), 40f * u, Rgb(1, 1, 1, 0.3f * a)); for (int i = 0; i < 3; i++) { float k = Gfx.Mod(t * 0.8f + i / 3f, 1f); Ring(dc, b, q, (76f + k * 80f) * u, Col(col, a * (1f - k)), 4f * u); } }
        if (Variant == 3) { for (int i = 0; i < 9; i++) { float k = Gfx.Mod(t * 0.45f + i / 9f, 1f), x = 80f * u + k * (L - 160f * u); var prev = dc.Transform; var m = A + d * x; b.Color = Col(Gfx.Mix(Color1, Color2, k), a * MathF.Sin(k * MathF.PI)); dc.DrawEllipse(new Ellipse(m, 16f * u, 72f * u), b, 6f * u); dc.Transform = prev; } return; }
        for (int w2 = 0; w2 < 2; w2++) { var pts = new List<Vector2>(); int N = 130; for (int i = 0; i <= N; i++) { float k = i / (float)N, env = MathF.Pow(MathF.Max(0f, MathF.Sin(k * MathF.PI)), 0.55f), ph = k * 22f + (w2 == 0 ? -t * 8f : t * 8f), wave = Variant == 2 ? 2f / MathF.PI * MathF.Asin(MathF.Sin(ph)) : MathF.Sin(ph); pts.Add(A + d * k * L + n * wave * env * (58f - w2 * 18f) * u); } Path(dc, b, pts, w2 == 0 ? C1(0.9f * a) : C2(0.9f * a), 5f * u); }
    }
}

public sealed class FleetBarragePlugin : QuickShapePlugin { public override string Name => "演出：宇宙戦艦の砲撃"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FleetBarrageParameter(s); }
public sealed class FleetBarrageParameter : Q3Param
{
    public FleetBarrageParameter() : this(null) { }
    public FleetBarrageParameter(SharedDataStore? s) : base(s) { Count = 5; Color1 = Hex(0x25324A); Color2 = Hex(0x5CF2FF); Color3 = Hex(0xFF6A3D); }
    static readonly Dictionary<string, ParamGuide> g = G("宇宙戦艦の砲撃", "Count=艦の数||1|16", "Variant=向き|1=右へ撃つ　2=左へ撃つ|1|2", "Background=宇宙を塗る", "Color1=艦の色", "Color2=味方の弾の色", "Color3=敵の弾の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, dir = Variant == 2 ? -1f : 1f; int n = Math.Clamp(Count, 1, 16); if (Background) { Box(dc, b, 0, 0, w, h, Rgb(0.01f, 0.015f, 0.05f, a)); for (int i = 0; i < 130; i++) Dot(dc, b, new(Rnd(i) * w, Rnd(i + 300) * h), (0.7f + Rnd(i + 600) * 1.6f) * u, Rgb(1, 1, 1, (0.3f + Rnd(i + 900) * 0.7f) * a)); }
        void Ship(Vector2 q, float S, float d, Color4 col) { Poly(dc, b, new List<Vector2> { q + new Vector2(d * S * 1.25f, 0), q + new Vector2(d * S * 0.35f, -S * 0.22f), q + new Vector2(-d * S * 0.25f, -S * 0.2f), q + new Vector2(-d * S * 0.42f, -S * 0.52f), q + new Vector2(-d * S * 0.82f, -S * 0.52f), q + new Vector2(-d * S, -S * 0.12f), q + new Vector2(-d * S, S * 0.2f), q + new Vector2(d * S * 0.32f, S * 0.24f) }, col); Box(dc, b, q.X - d * S * 0.4f - S * 0.12f, q.Y - S * 0.36f, q.X - d * S * 0.4f + S * 0.12f, q.Y - S * 0.24f, Rgb(0.65f, 0.85f, 1f, a)); Oval(dc, b, q - new Vector2(d * S * 1.08f, 0), S * 0.16f, S * 0.1f, C2(a)); }
        for (int i = 0; i < n; i++) { float y = h * (i + 0.5f) / n, S = (54f + 22f * Rnd(i)) * u, side = dir > 0f ? w * 0.13f : w * 0.87f, ox = (Rnd(i + 3) - 0.5f) * 120f * u; Ship(new(side + ox, y + MathF.Sin(t + i) * 8f * u), S, dir, C1(a)); Ship(new(w - side - ox, h * (i + 0.5f) / n + MathF.Cos(t + i) * 8f * u), S * 0.8f, -dir, Rgb(0.26f, 0.13f, 0.13f, a)); }
        for (int i = 0; i < n * 7; i++) { bool mine = i % 3 != 0; float k = Gfx.Mod(t * (0.62f + Rnd(i) * 0.5f) + Rnd(i + 8), 1f), y0 = h * ((i % n) + 0.5f) / n, y1 = h * (((int)(Rnd(i + 14) * n)) + 0.5f) / n, x0 = dir > 0 ? w * 0.18f : w * 0.82f, x1 = w - x0; if (!mine) (x0, x1) = (x1, x0); var q = Vector2.Lerp(new(x0, y0), new(x1, y1), k); var v = Vector2.Normalize(new Vector2(x1 - x0, y1 - y0)); var col = mine ? C2(a) : C3(a); Ln(dc, b, q, q - v * 70f * u, new Color4(col.R, col.G, col.B, col.A * 0.35f), 8f * u); Ln(dc, b, q, q - v * 44f * u, col, 4f * u); if (k > 0.94f) { float e = (k - 0.94f) / 0.06f; Ring(dc, b, new(x1, y1), (12f + e * 56f) * u, new Color4(col.R, col.G, col.B, col.A * (1f - e)), 4f * u); } }
    }
}

public sealed class UfoEscapePlugin : QuickShapePlugin { public override string Name => "演出：UFOが飛び去る"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new UfoEscapeParameter(s); }
public sealed class UfoEscapeParameter : Q3Param
{
    public UfoEscapeParameter() : this(null) { }
    public UfoEscapeParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xA8B6C8); Color2 = Hex(0x68FFB5); Color3 = Hex(0xBDEBFF); }
    static readonly Dictionary<string, ParamGuide> g = G("UFO", "Variant=動きの形|1=ジグザグ　2=波　3=回転|1|3", "Color1=機体の色", "Color2=光の色", "Color3=ドームの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time; var o = P0(c); float k = Sub(t, 0f, 2.1f), leave = Sub(t, 2.1f, 0.2f); Vector2 path = Variant switch { 2 => new Vector2((k - 0.5f) * 1100f * u, MathF.Sin(k * 14f) * 160f * u), 3 => new Vector2(MathF.Cos(k * 12f) * (1f - k * 0.45f) * 360f * u, MathF.Sin(k * 12f) * 150f * u), _ => new Vector2(MathF.Asin(MathF.Sin(k * 9f)) * 280f * u, (int)(k * 6f) % 2 == 0 ? -120f * u : 120f * u) }; var q = o + path + new Vector2(leave * 1200f * u, -leave * 1200f * u); float S = 150f * u * (1f - leave * 0.94f), tilt = MathF.Sin(t * 8f) * 0.12f; if (leave >= 1f) return;
        for (int i = 1; i < 7; i++) { float k0 = MathF.Max(0f, k - i * 0.018f); Vector2 prevP = Variant switch { 2 => new Vector2((k0 - 0.5f) * 1100f * u, MathF.Sin(k0 * 14f) * 160f * u), 3 => new Vector2(MathF.Cos(k0 * 12f) * (1f - k0 * 0.45f) * 360f * u, MathF.Sin(k0 * 12f) * 150f * u), _ => new Vector2(MathF.Asin(MathF.Sin(k0 * 9f)) * 280f * u, (int)(k0 * 6f) % 2 == 0 ? -120f * u : 120f * u) }; Oval(dc, b, o + prevP, S * (1f - i * 0.1f), S * 0.22f, C2(0.09f * a)); }
        var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(tilt, q) * prev; Poly(dc, b, new List<Vector2> { q + new Vector2(-S * 0.38f, S * 0.12f), q + new Vector2(S * 0.38f, S * 0.12f), q + new Vector2(S * 0.9f, S * 1.45f), q + new Vector2(-S * 0.9f, S * 1.45f) }, C2(0.15f * a)); Pie(dc, b, q - new Vector2(0, S * 0.08f), S * 0.46f, MathF.PI, MathF.Tau, C3(0.85f * a)); Oval(dc, b, q, S, S * 0.26f, C1(a)); Oval(dc, b, q + new Vector2(0, S * 0.1f), S * 0.56f, S * 0.11f, Rgb(0.22f, 0.26f, 0.32f, a)); for (int i = 0; i < 7; i++) { float x = (i - 3f) * S * 0.25f; Dot(dc, b, q + new Vector2(x, S * 0.09f + (1f - MathF.Abs(i - 3f) / 3f) * S * 0.07f), S * 0.045f, MathF.Sin(t * 10f + i) > 0f ? C2(a) : Rgb(1, 0.85f, 0.3f, a)); } dc.Transform = prev;
    }
}

public sealed class AlienTextFlowPlugin : QuickShapePlugin { public override string Name => "演出：異星の文字が流れる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AlienTextFlowParameter(s); }
public sealed class AlienTextFlowParameter : Q3Param
{
    public AlienTextFlowParameter() : this(null) { }
    public AlienTextFlowParameter(SharedDataStore? s) : base(s) { Count = 14; Color1 = Hex(0x5CFFB0); Color2 = Hex(0x07111A); }
    static readonly Dictionary<string, ParamGuide> g = G("異星の文字", "Variant=向き|1=縦に下へ　2=縦に上へ　3=横に右へ　4=横に左へ|1|4", "Count=密度||3|50", "Speed=速さ", "Background=背景を塗る", "Color1=文字の色", "Color2=背景の色");
    void Glyph(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 q, float S, int id, Color4 col, float lw)
    {
        for (int k = 0; k < 4; k++) { int bits = (int)(Rnd(id * 7 + k) * 25f); var p0 = q + new Vector2((bits % 3 - 1) * S * 0.42f, (bits / 3 % 3 - 1) * S * 0.48f); int b2 = (int)(Rnd(id * 13 + k + 4) * 25f); var p1 = q + new Vector2((b2 % 3 - 1) * S * 0.42f, (b2 / 3 % 3 - 1) * S * 0.48f); Ln(dc, b, p0, p1, col, lw); } if (id % 3 == 0) Ring(dc, b, q, S * 0.2f, col, lw); else Dot(dc, b, q + new Vector2(S * 0.32f, -S * 0.34f), lw * 0.9f, col);
    }
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c); int n = Math.Clamp(Count, 3, 50); bool vertical = Variant <= 2; float dir = Variant is 2 or 4 ? -1f : 1f, across = vertical ? w : h, along = vertical ? h : w, laneW = across / n, S = MathF.Min(laneW * 0.72f, 54f * u), step = S * 1.34f; if (Background) Box(dc, b, 0, 0, w, h, C2(a)); int cells = (int)(along / step) + 3;
        for (int l = 0; l < n; l++) { float speed = 70f + Rnd(l) * 160f, head = Gfx.Mod(t * speed * u * dir + Rnd(l + 80) * along * 2f, along + cells * step * 0.6f); for (int i = 0; i < cells; i++) { float pos = i * step, dist = dir > 0f ? head - pos : pos - (along - head), fade = dist >= 0f ? MathF.Max(0f, 1f - dist / (cells * step * 0.55f)) : 0f; if (fade <= 0f) continue; var q = vertical ? new Vector2((l + 0.5f) * laneW, pos) : new Vector2(pos, (l + 0.5f) * laneW); int id = l * 41 + i * 17 + (int)(t * 2f + Rnd(l + i) * 8f); Glyph(dc, b, q, S, id, dist < step ? Rgb(0.88f, 1f, 0.94f, a) : C1(fade * 0.92f * a), MathF.Max(1.4f, S * 0.07f)); } }
    }
}

public sealed class MechPanelsPlugin : QuickShapePlugin { public override string Name => "演出：メカの変形（パネルが開く）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MechPanelsParameter(s); }
public sealed class MechPanelsParameter : Q3Param
{
    public MechPanelsParameter() : this(null) { }
    public MechPanelsParameter(SharedDataStore? s) : base(s) { Count = 6; Color1 = Hex(0x56616E); Color2 = Hex(0xFF9C2E); Color3 = Hex(0x12171E); }
    static readonly Dictionary<string, ParamGuide> g = G("メカの変形", "Count=板の数(1辺)||2|12", "Variant=開く順|1=外から　2=中から　3=ランダム|1|3", "Speed=速さ", "Color1=板の色", "Color2=中の光の色", "Color3=奥の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 2, 12); float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)), S = 860f * u, cell = S / n; var o = P0(c); float x0 = o.X - S / 2f, y0 = o.Y - S / 2f; Box(dc, b, x0, y0, x0 + S, y0 + S, C3(a)); using (var rg = Gfx.Radial(dc, o, S * 0.68f, (0f, C2(a)), (0.45f, C2(0.45f * a)), (1f, C2(0f)))) dc.FillRectangle(new Vortice.RawRectF(x0, y0, x0 + S, y0 + S), rg);
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            float dx = x - (n - 1) / 2f, dy = y - (n - 1) / 2f, dist = MathF.Max(MathF.Abs(dx), MathF.Abs(dy)) / MathF.Max(1f, (n - 1) / 2f), order = Variant == 2 ? dist : Variant == 3 ? Rnd(x * 19 + y * 31 + Seed) : 1f - dist, k = DrawKit.EaseOut(Sub(t, 0.15f + order * 1.25f, 0.28f)), fold = MathF.Max(0.02f, 1f - k), cx = x0 + (x + 0.5f) * cell, cy = y0 + (y + 0.5f) * cell; bool hor = (x + y) % 2 == 0; float pw = cell * (hor ? fold : 1f), ph = cell * (hor ? 1f : fold), side = (hor ? x : y) % 2 == 0 ? -1f : 1f, px = cx + (hor ? side * (cell - pw) / 2f : 0f), py = cy + (hor ? 0f : side * (cell - ph) / 2f); var col = Col(Gfx.Mix(Color1, Hex(0x000000), k * 0.45f + (x * 7 + y * 3) % 4 * 0.04f), a); Box(dc, b, px - pw / 2f + 1.5f * u, py - ph / 2f + 1.5f * u, px + pw / 2f - 1.5f * u, py + ph / 2f - 1.5f * u, col); Frame(dc, b, px - pw / 2f + 3f * u, py - ph / 2f + 3f * u, px + pw / 2f - 3f * u, py + ph / 2f - 3f * u, Rgb(1, 1, 1, 0.2f * a), 2f * u);
            if (k < 0.4f) foreach (var (mx, my) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) }) Dot(dc, b, new(px + mx * (pw / 2f - 11f * u), py + my * (ph / 2f - 11f * u)), 3.5f * u, Rgb(0.8f, 0.82f, 0.85f, a));
        }
    }
}

public sealed class PlanetSurfacePlugin : QuickShapePlugin { public override string Name => "演出：惑星の地表（クレーター）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PlanetSurfaceParameter(s); }
public sealed class PlanetSurfaceParameter : Q3Param
{
    public PlanetSurfaceParameter() : this(null) { }
    public PlanetSurfaceParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x9B9288); Color2 = Hex(0x03060F); Color3 = Hex(0x4FA8FF); }
    static readonly Dictionary<string, ParamGuide> g = G("惑星の地表", "Variant=空の天体|1=地球　2=赤い惑星　3=環のある星　4=なし|1|4", "Seed=シード", "Color1=地面の色", "Color2=空の色", "Color3=天体の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, gy = h * 0.62f; Box(dc, b, 0, 0, w, h, C2(a)); for (int i = 0; i < 180; i++) Dot(dc, b, new(Rnd(i) * w, Rnd(i + 300) * gy), (0.7f + Rnd(i + 600) * 1.6f) * u, Rgb(1, 1, 1, (0.3f + 0.7f * MathF.Abs(MathF.Sin(t * 0.5f + i))) * a));
        var p = new Vector2(w * 0.72f, h * 0.27f); float R = 165f * u; if (Variant != 4) { var col = Variant == 2 ? Hex(0xD9573E) : Variant == 3 ? Hex(0xE3C37F) : Color3; Dot(dc, b, p, R * 1.16f, Col(col, 0.14f * a)); Dot(dc, b, p, R, Col(col, a)); if (Variant == 1) for (int i = 0; i < 8; i++) { var q = Pol(p, R * (0.2f + Rnd(i + 3) * 0.62f), Rnd(i + 5) * MathF.Tau + t * 0.05f); Oval(dc, b, q, (28f + Rnd(i) * 42f) * u, (17f + Rnd(i + 2) * 26f) * u, Rgb(0.25f, 0.68f, 0.32f, a)); } if (Variant == 3) { b.Color = Rgb(0.95f, 0.85f, 0.62f, 0.85f * a); dc.DrawEllipse(new Ellipse(p, R * 1.75f, R * 0.42f), b, 10f * u); } Pie(dc, b, p, R, -0.9f, MathF.PI - 0.9f, Rgb(0, 0, 0, 0.48f * a)); }
        var land = new List<Vector2> { new(0, h) }; for (int i = 0; i <= 80; i++) { float k = i / 80f; land.Add(new(k * w, gy + (MathF.Sin(k * 7f + Seed) * 28f + MathF.Sin(k * 23f + Seed) * 10f) * u)); } land.Add(new(w, h)); Poly(dc, b, land, C1(a));
        for (int i = 0; i < 16; i++) { float d = Rnd(i * 2 + Seed), x = Rnd(i * 3 + 5 + Seed) * w, y = gy + (40f * u) + d * (h - gy), rx = (45f + d * 150f) * u, ry = rx * 0.3f; Oval(dc, b, new(x, y), rx * 1.14f, ry * 1.2f, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.2f), a)); Oval(dc, b, new(x, y), rx, ry, Col(Gfx.Mix(Color1, Hex(0x000000), 0.45f), a)); Oval(dc, b, new(x + rx * 0.12f, y + ry * 0.2f), rx * 0.78f, ry * 0.68f, Col(Gfx.Mix(Color1, Hex(0x000000), 0.24f), a)); }
    }
}

public sealed class SpaceHelmetViewPlugin : QuickShapePlugin { public override string Name => "演出：宇宙服のヘルメット越しの視界"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SpaceHelmetViewParameter(s); }
public sealed class SpaceHelmetViewParameter : Q3Param
{
    public SpaceHelmetViewParameter() : this(null) { }
    public SpaceHelmetViewParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x0E1218); Color2 = Hex(0x68FFB5); Color3 = Hex(0xFF5252); }
    protected override double DefAmount => 72;
    protected override double DefAmount2 => 18;
    static readonly Dictionary<string, ParamGuide> g = G("ヘルメットの視界", "Amount=酸素の残り", "Amount2=曇り", "Color1=ふちの色", "Color2=表示の色", "Color3=少ない時の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, o2 = Amt(c), fog = Amt2(c); var o = new Vector2(w / 2f, h / 2f); float rx = w * 0.46f, ry = h * 0.47f; var mask = new List<Vector2> { new(0, 0), new(w, 0), new(w, h / 2f) }; for (int i = 0; i <= 90; i++) { float th = i / 90f * MathF.Tau; mask.Add(o + new Vector2(MathF.Cos(th) * rx, MathF.Sin(th) * ry)); } mask.Add(new(w, h / 2f)); mask.Add(new(w, h)); mask.Add(new(0, h)); Poly(dc, b, mask, C1(a));
        b.Color = Rgb(0.42f, 0.47f, 0.54f, a); dc.DrawEllipse(new Ellipse(o, rx, ry), b, 16f * u); b.Color = Rgb(1, 1, 1, 0.16f * a); dc.DrawEllipse(new Ellipse(o, rx * 0.965f, ry * 0.955f), b, 8f * u); Path(dc, b, Enumerable.Range(0, 30).Select(i => { float th = MathF.PI * 1.08f + i / 29f * 0.65f; return o + new Vector2(MathF.Cos(th) * rx * 0.87f, MathF.Sin(th) * ry * 0.85f); }).ToList(), Rgb(1, 1, 1, 0.2f * a), 24f * u); if (fog > 0.01f) for (int i = 0; i < 13; i++) { float th = i / 13f * MathF.Tau, br = (1f + MathF.Sin(t * 1.4f)) * 0.5f; var q = o + new Vector2(MathF.Cos(th) * rx * 0.72f, MathF.Sin(th) * ry * 0.74f); float fr = (190f + br * 50f) * u; using var fb = Gfx.Radial(dc, q, fr, (0f, Rgb(0.9f, 0.95f, 1f, fog * 0.34f * a)), (1f, Rgb(0.9f, 0.95f, 1f, 0f))); dc.FillEllipse(new Ellipse(q, fr, fr), fb); }
        bool low = o2 < 0.25f; var hud = low && (int)(t * 3f) % 2 == 0 ? C3(a) : low ? C3(0.5f * a) : C2(a); var bar = new Vector2(w * 0.19f, h * 0.76f); SayAt(dc, b, "O₂", 34f * u, bar - new Vector2(0, 54f * u), hud); Frame(dc, b, bar.X, bar.Y, bar.X + 300f * u, bar.Y + 24f * u, hud, 3f * u); Box(dc, b, bar.X + 4f * u, bar.Y + 4f * u, bar.X + 4f * u + 292f * u * o2, bar.Y + 20f * u, hud); Say(dc, b, (o2 * 100f).ToString("0") + "%", 32f * u, bar + new Vector2(350f * u, 12f * u), hud); float hb = t * 3f; var pts = new List<Vector2>(); for (int i = 0; i <= 42; i++) { float k = i / 42f, f = Gfx.Mod(k * 2.5f - hb, 1f), y = f < 0.1f ? -MathF.Sin(f * 31.4f) * 30f : f < 0.16f ? MathF.Sin((f - 0.1f) * 52f) * 12f : 0f; pts.Add(new Vector2(w * 0.67f + k * 300f * u, h * 0.78f + y * u)); } Path(dc, b, pts, C2(a), 3f * u);
    }
}

public sealed class HoloTablePlugin : QuickShapePlugin { public override string Name => "演出：ホログラムの作戦テーブル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HoloTableParameter(s); }
public sealed class HoloTableParameter : Q3Param
{
    public HoloTableParameter() : this(null) { }
    public HoloTableParameter(SharedDataStore? s) : base(s) { Count = 5; Color1 = Hex(0x47D6FF); Color2 = Hex(0xFF5252); Color3 = Hex(0x1B2733); }
    static readonly Dictionary<string, ParamGuide> g = G("ホログラムの作戦テーブル", "Count=印の数||0|20", "Speed=回る速さ", "Seed=シード", "Color1=立体の色", "Color2=印の色", "Color3=テーブルの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        const int N = 22; float u = U(c), t = c.Time * Spd(c) * 0.28f, S = 26f * u, rise = DrawKit.EaseOut(Intro(c, 0.1f, 0.7f)); var o = P0(c) + new Vector2(0, 100f * u); float ca = MathF.Cos(t), sa = MathF.Sin(t); float H(int x, int y) => (MathF.Sin(x * 0.42f + Seed) * MathF.Cos(y * 0.36f + Seed) + 0.55f * MathF.Sin((x + y) * 0.23f)) * 58f + 62f; Vector2 P(float x, float y, float z) { float rx = x * ca - y * sa, ry = x * sa + y * ca; return o + new Vector2(rx * S, ry * S * 0.46f - z * u * rise); }
        var table = new List<Vector2>(); for (int i = 0; i < 6; i++) { float th = i * MathF.PI / 3f + 0.52f; table.Add(o + new Vector2(MathF.Cos(th) * 560f * u, MathF.Sin(th) * 245f * u + 36f * u)); } Poly(dc, b, table, C3(a)); Outline(dc, b, table, C1(0.8f * a), 4f * u); Oval(dc, b, o + new Vector2(0, 36f * u), 410f * u, 170f * u, C1(0.12f * a));
        for (int y = 0; y <= N; y++) { var row = new List<Vector2>(); var col = new List<Vector2>(); for (int x = 0; x <= N; x++) { row.Add(P(x - N / 2f, y - N / 2f, H(x, y))); col.Add(P(y - N / 2f, x - N / 2f, H(y, x))); } Path(dc, b, row, C1(0.62f * a), 1.7f * u); Path(dc, b, col, C1(0.42f * a), 1.7f * u); }
        for (int i = 0; i < Math.Clamp(Count, 0, 20); i++) { int x = 2 + (int)(Rnd(i + Seed) * (N - 3)), y = 2 + (int)(Rnd(i + 30 + Seed) * (N - 3)); var q = P(x - N / 2f, y - N / 2f, H(x, y)); var top = q - new Vector2(0, 68f * u); float blink = 0.5f + 0.5f * MathF.Sin(c.Time * 4f + i); Ln(dc, b, q, top, C2(a), 3f * u); Poly(dc, b, new List<Vector2> { q, top + new Vector2(13f * u, 0), top - new Vector2(13f * u, 0) }, C2((0.45f + blink * 0.55f) * a)); Ring(dc, b, q, (8f + blink * 16f) * u, C2((1f - blink) * a), 2f * u); }
    }
}

public sealed class NanoSwarmPlugin : QuickShapePlugin { public override string Name => "演出：ナノマシンの群れ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new NanoSwarmParameter(s); }
public sealed class NanoSwarmParameter : Q3Param
{
    public NanoSwarmParameter() : this(null) { }
    public NanoSwarmParameter(SharedDataStore? s) : base(s) { Text = "NANO"; Count = 360; Color1 = Hex(0x5CF2FF); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("ナノマシンの群れ", "Count=粒の数||40|1200", "Variant=できる形|1=丸　2=星　3=四角　4=文字|1|4", "Text=文字(4の時)", "Color1=粒の色", "Color2=光の色");
    Vector2 Target(int i, int n, float R)
    {
        float k = i / (float)n, th = k * MathF.Tau; switch (Variant)
        {
            case 2: { float m = k * 10f; int s = (int)m; float f = m - s, a0 = -MathF.PI / 2f + s * MathF.PI / 5f, a1 = a0 + MathF.PI / 5f, r0 = s % 2 == 0 ? R : R * 0.42f, r1 = s % 2 == 0 ? R * 0.42f : R; return Vector2.Lerp(new(MathF.Cos(a0) * r0, MathF.Sin(a0) * r0), new(MathF.Cos(a1) * r1, MathF.Sin(a1) * r1), f); }
            case 3: { float m = k * 4f; int s = (int)m; float f = m - s; return s switch { 0 => new(-R + 2f * R * f, -R), 1 => new(R, -R + 2f * R * f), 2 => new(R - 2f * R * f, R), _ => new(-R, R - 2f * R * f) }; }
            default: return new(MathF.Cos(th) * R, MathF.Sin(th) * R);
        }
    }
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 40, 1200); float u = U(c), t = c.Time, R = 300f * u; var o = P0(c);
        for (int i = 0; i < n; i++) { float k = DrawKit.EaseOut(Sub(t, 0.1f + Rnd(i) * 0.75f, 0.95f)), th = Rnd(i + 7) * MathF.Tau + t * (0.4f + Rnd(i + 9)), rr = (520f + Rnd(i + 11) * 520f) * u; var scat = new Vector2(MathF.Cos(th) * rr, MathF.Sin(th) * rr * 0.62f); var goal = Variant == 4 ? new Vector2((Rnd(i + 40) - 0.5f) * 880f * u, (Rnd(i + 41) - 0.5f) * 190f * u) : Target(i, n, R) + new Vector2(MathF.Sin(t * 3f + i) * 3f * u, MathF.Cos(t * 2.6f + i) * 3f * u); var q = o + Vector2.Lerp(scat, goal, k); Dot(dc, b, q, (1.6f + Rnd(i + 3) * 2.1f) * u, i % 7 == 0 ? C2(a) : C1((Variant == 4 ? 0.26f + 0.24f * (1f - k) : 0.9f) * a)); }
        if (Variant == 4) { float f = Sub(t, 0.9f, 0.5f); Say(dc, b, Str, 190f * u, o, C1(a * f), C2(0.65f * a * f), 2f * u, maxW: 900f * u); }
    }
}

public sealed class DataCubePlugin : QuickShapePlugin { public override string Name => "演出：データの立方体"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DataCubeParameter(s); }
public sealed class DataCubeParameter : Q3Param
{
    public DataCubeParameter() : this(null) { }
    public DataCubeParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x47D6FF); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 55;
    static readonly Dictionary<string, ParamGuide> g = G("データの立方体", "Amount=大きさ", "Speed=回る速さ", "Color1=色", "Color2=数字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), S = (130f + Amt(c) * 240f) * u, ax = 0.62f + MathF.Sin(t * 0.5f) * 0.18f, ay = t * 0.8f; var o = P0(c); Vector3 Rot(Vector3 v) { float x = v.X * MathF.Cos(ay) + v.Z * MathF.Sin(ay), z = -v.X * MathF.Sin(ay) + v.Z * MathF.Cos(ay), y = v.Y * MathF.Cos(ax) - z * MathF.Sin(ax); z = v.Y * MathF.Sin(ax) + z * MathF.Cos(ax); return new(x, y, z); } Vector2 Pj(Vector3 v) { float p = 3.4f / (3.4f + v.Z); return o + new Vector2(v.X, v.Y) * S * p; }
        (Vector3 n, Vector3 u1, Vector3 v1)[] faces = [(new(0, 0, -1), new(1, 0, 0), new(0, 1, 0)), (new(0, 0, 1), new(-1, 0, 0), new(0, 1, 0)), (new(1, 0, 0), new(0, 0, 1), new(0, 1, 0)), (new(-1, 0, 0), new(0, 0, -1), new(0, 1, 0)), (new(0, -1, 0), new(1, 0, 0), new(0, 0, 1)), (new(0, 1, 0), new(1, 0, 0), new(0, 0, -1))]; Dot(dc, b, o, S * 2.05f, C1(0.08f * a));
        foreach (var (f, idx) in faces.Select((f, idx) => (f, idx)).OrderByDescending(x => Rot(x.f.n).Z))
        {
            var nr = Rot(f.n); if (nr.Z > 0.08f) continue; var corners = new[] { f.n - f.u1 - f.v1, f.n + f.u1 - f.v1, f.n + f.u1 + f.v1, f.n - f.u1 + f.v1 }.Select(v => Pj(Rot(v))).ToList(); Poly(dc, b, corners, C1((0.15f + (-nr.Z) * 0.26f) * a)); Outline(dc, b, corners, C1(a), 4f * u); const int G2 = 6; for (int y = 0; y < G2; y++) for (int x = 0; x < G2; x++) { int digit = (int)(Rnd(idx * 97 + x * 11 + y * 37 + (int)(t * 3f + Rnd(x + y * 9) * 4f)) * 2f); var v = f.n + f.u1 * ((x + 0.5f) / G2 * 2f - 1f) + f.v1 * ((y + 0.5f) / G2 * 2f - 1f); var q = Pj(Rot(v)); float al = (0.35f + (-nr.Z) * 0.65f) * (0.45f + 0.55f * MathF.Abs(MathF.Sin(t * 1.5f + x + y))); float size = S * 0.15f; Say(dc, b, digit.ToString(), size, q, C2(al * a)); }
        }
    }
}

public sealed class TarotSpreadPlugin : QuickShapePlugin { public override string Name => "演出：タロットカード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TarotSpreadParameter(s); }
public sealed class TarotSpreadParameter : Q3Param
{
    public TarotSpreadParameter() : this(null) { }
    public TarotSpreadParameter(SharedDataStore? s) : base(s) { Text = "星の導き"; Count = 5; Color1 = Hex(0x2B1B54); Color2 = Hex(0xE6C15A); Color3 = Hex(0xFFF8E7); }
    protected override double DefAmount => 50;
    static readonly Dictionary<string, ParamGuide> g = G("タロットカード", "Text=カードの名前", "Count=枚数||1|9", "Amount=選ぶカードの位置", "Variant=絵柄|1=星　2=月　3=太陽　4=目|1|4", "Color1=裏の色", "Color2=金の色", "Color3=表の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 1, 9), pick = Math.Clamp((int)MathF.Round(Amt(c) * (n - 1)), 0, n - 1); float u = U(c), t = c.Time, W = 230f * u, H = 390f * u, gap = MathF.Min(260f * u, 1420f * u / n); var o = P0(c);
        for (int i = 0; i < n; i++)
        {
            bool sel = i == pick; float deal = DrawKit.EaseOut(Sub(t, i * 0.09f, 0.35f)), up = sel ? DrawKit.EaseOut(Sub(t, 0.75f + n * 0.05f, 0.35f)) : 0f, flip = sel ? DrawKit.EaseOut(Sub(t, 1.1f + n * 0.05f, 0.42f)) : 0f, sx = MathF.Max(0.02f, MathF.Abs(MathF.Cos(flip * MathF.PI))), scale = 1f + up * 0.24f; var q = Vector2.Lerp(o + new Vector2(0, 520f * u), o + new Vector2((i - (n - 1) / 2f) * gap, -up * 34f * u), deal); bool front = flip > 0.5f; if (sel) Dot(dc, b, q, 310f * u * up, C2(0.12f * a * up)); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateScale(sx * scale, scale, q) * prev; RBox(dc, b, q, W, H, 16f * u, front ? C3(a) : C1(a)); RFrame(dc, b, q, W - 16f * u, H - 16f * u, 10f * u, C2(a), 4f * u);
            if (!front) { Ring(dc, b, q, 62f * u, C2(a), 3f * u); Star(dc, b, q, 56f * u, 8, 0.42f, t * 0.2f, C2(0.85f * a)); for (int k = 0; k < 4; k++) Star(dc, b, q + new Vector2(k % 2 == 0 ? -72f * u : 72f * u, k < 2 ? -135f * u : 135f * u), 14f * u, 4, 0.25f, 0f, C2(a)); }
            else { var art = q - new Vector2(0, 42f * u); if (Variant == 2) { Dot(dc, b, art, 72f * u, C2(a)); Dot(dc, b, art + new Vector2(28f * u, -12f * u), 62f * u, C3(a)); } else if (Variant == 3) { for (int k = 0; k < 16; k++) { float th = k * MathF.PI / 8f; Ln(dc, b, Pol(art, 62f * u, th), Pol(art, (k % 2 == 0 ? 102f : 86f) * u, th), C2(a), 5f * u); } Dot(dc, b, art, 58f * u, C2(a)); } else if (Variant == 4) { Oval(dc, b, art, 90f * u, 42f * u, C2(a)); Oval(dc, b, art, 73f * u, 31f * u, C3(a)); Dot(dc, b, art, 26f * u, C1(a)); } else { Star(dc, b, art, 94f * u, 8, 0.28f, 0f, C2(a)); for (int k = 0; k < 7; k++) Star(dc, b, art + new Vector2((Rnd(k) - 0.5f) * 170f * u, (Rnd(k + 3) - 0.5f) * 190f * u), 8f * u, 4, 0.22f, 0f, C1(a)); } Ln(dc, b, q + new Vector2(-80f * u, 92f * u), q + new Vector2(80f * u, 92f * u), C2(a), 3f * u); Say(dc, b, Str, 30f * u, q + new Vector2(0, 136f * u), C1(a), maxW: W - 40f * u); }
            dc.Transform = prev;
        }
    }
}

public sealed class CrystalBallPlugin : QuickShapePlugin { public override string Name => "演出：水晶玉の占い"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CrystalBallParameter(s); }
public sealed class CrystalBallParameter : Q3Param
{
    public CrystalBallParameter() : this(null) { }
    public CrystalBallParameter(SharedDataStore? s) : base(s) { Text = "吉"; Color1 = Hex(0xA66BFF); Color2 = Hex(0x5CF2FF); Color3 = Hex(0xC9A227); }
    static readonly Dictionary<string, ParamGuide> g = G("水晶玉の占い", "Text=浮かぶ文字", "Speed=渦の速さ", "Color1=煙の色1", "Color2=煙の色2", "Color3=台の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), R = 310f * u; var o = P0(c) - new Vector2(0, 60f * u); Poly(dc, b, new List<Vector2> { o + new Vector2(-175f * u, R * 0.84f), o + new Vector2(175f * u, R * 0.84f), o + new Vector2(255f * u, R + 125f * u), o + new Vector2(-255f * u, R + 125f * u) }, C3(a)); RBox(dc, b, o + new Vector2(0, R + 135f * u), 560f * u, 44f * u, 14f * u, Col(Gfx.Mix(Color3, Hex(0x000000), 0.28f), a)); Dot(dc, b, o, R * 1.22f, C1(0.09f * a)); using (var rg = Gfx.Radial(dc, o, R, (0f, Rgb(0.08f, 0.04f, 0.2f, 0.98f * a)), (0.8f, Rgb(0.14f, 0.08f, 0.32f, 0.96f * a)), (1f, Rgb(0.55f, 0.48f, 0.9f, 0.95f * a)))) dc.FillEllipse(new Ellipse(o, R, R), rg);
        for (int arm = 0; arm < 3; arm++) for (int i = 0; i < 44; i++) { float k = i / 44f, th = arm * MathF.Tau / 3f + t * 0.8f + k * 4.5f, r = R * (0.06f + k * 0.72f), S = R * (0.15f - k * 0.05f); Dot(dc, b, Pol(o, r, th), S, Col(Gfx.Mix(Color1, Color2, k), (0.2f - k * 0.1f) * a)); }
        float rev = Sub(c.Time, 0.8f, 0.8f); Say(dc, b, Str, 150f * u, o, Rgb(1, 1, 1, rev * a), C1(rev * a), 5f * u, maxW: R * 1.55f); Oval(dc, b, o + new Vector2(-R * 0.42f, -R * 0.48f), R * 0.25f, R * 0.13f, Rgb(1, 1, 1, 0.38f * a)); Ring(dc, b, o, R, Rgb(0.82f, 0.78f, 1f, 0.85f * a), 4f * u);
    }
}

public sealed class AnvilForgePlugin : QuickShapePlugin { public override string Name => "演出：鍛冶の金床と火花"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AnvilForgeParameter(s); }
public sealed class AnvilForgeParameter : Q3Param
{
    public AnvilForgeParameter() : this(null) { }
    public AnvilForgeParameter(SharedDataStore? s) : base(s) { Count = 4; Color1 = Hex(0x3A3F47); Color2 = Hex(0xFFB02E); Color3 = Hex(0xFF5B1F); }
    protected override double DefAmount => 40;
    static readonly Dictionary<string, ParamGuide> g = G("鍛冶の金床", "Count=打つ回数(0でずっと)||0|40", "Amount=火花の量", "Speed=打つ速さ", "Color1=金床の色", "Color2=火花の色", "Color3=焼けた鉄の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)) * 1.25f, ph = Gfx.Mod(t, 1f); int strikes = (int)t; bool ended = Count > 0 && strikes >= Count; if (ended) ph = 0.82f; float swing = ph < 0.3f ? 1f - DrawKit.EaseOut(ph / 0.3f) * 0.1f + 0f : ph < 0.42f ? 0.9f - (ph - 0.3f) / 0.12f * 0.9f : ph < 0.82f ? (ph - 0.42f) / 0.4f : 1f; var o = P0(c) + new Vector2(0, 90f * u); var anvil = new List<Vector2> { o + new Vector2(-250f * u, -80f * u), o + new Vector2(320f * u, -80f * u), o + new Vector2(235f * u, -22f * u), o + new Vector2(80f * u, 0), o + new Vector2(120f * u, 170f * u), o + new Vector2(205f * u, 215f * u), o + new Vector2(-205f * u, 215f * u), o + new Vector2(-120f * u, 170f * u), o + new Vector2(-85f * u, 0), o + new Vector2(-235f * u, -36f * u) }; Poly(dc, b, anvil, C1(a)); Ln(dc, b, o + new Vector2(-245f * u, -78f * u), o + new Vector2(315f * u, -78f * u), Rgb(0.72f, 0.75f, 0.8f, a), 5f * u); RBox(dc, b, o + new Vector2(30f * u, -96f * u), 280f * u, 34f * u, 10f * u, C3(a)); Oval(dc, b, o + new Vector2(30f * u, -98f * u), 180f * u, 34f * u, Rgb(1, 0.82f, 0.25f, 0.42f * a));
        var pivot = o + new Vector2(460f * u, -180f * u); float ang = MathF.PI + 0.2f + swing * 1.15f; var head = Pol(pivot, 410f * u, ang); var along = Vector2.Normalize(head - pivot); var nor = new Vector2(-along.Y, along.X); Ln(dc, b, pivot, head, Rgb(0.45f, 0.28f, 0.14f, a), 22f * u); Poly(dc, b, new List<Vector2> { head + along * 42f * u + nor * 96f * u, head + along * 42f * u - nor * 96f * u, head - along * 42f * u - nor * 96f * u, head - along * 42f * u + nor * 96f * u }, Rgb(0.33f, 0.36f, 0.41f, a));
        var hit = o + new Vector2(30f * u, -112f * u); int total = (int)(16f + Amt(c) * 160f); float since = ended ? 9f : ph < 0.42f ? ph + 0.58f : ph - 0.42f; if (strikes > 0 || ph >= 0.42f) { for (int i = 0; i < total; i++) { float e = Sub(since, Rnd(i) * 0.05f, 0.42f + Rnd(i + 3) * 0.28f); if (e <= 0f || e >= 1f) continue; float th = -MathF.PI * (0.07f + Rnd(i + 5 + strikes * 13) * 0.86f), sp = (160f + Rnd(i + 9 + strikes * 7) * 520f) * u; var q = hit + new Vector2(MathF.Cos(th), MathF.Sin(th)) * sp * e + new Vector2(0, e * e * 300f * u); var tail = q - new Vector2(MathF.Cos(th), MathF.Sin(th) + e * 1.2f) * 24f * u; Ln(dc, b, q, tail, C2(a * (1f - e)), 3f * u); } float fl = Sub(since, 0f, 0.16f); if (fl < 1f) { Dot(dc, b, hit, 150f * u * (1f - fl), C2(0.35f * a * (1f - fl))); Star(dc, b, hit, 150f * u * (1f - fl * 0.5f), 8, 0.2f, 0f, Rgb(1, 0.95f, 0.65f, a * (1f - fl))); } }
    }
}
