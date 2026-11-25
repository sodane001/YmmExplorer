using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 591〜610: 小物・アイコン・フレーム

public sealed class RibbonBannerPlugin : QuickShapePlugin { public override string Name => "演出：リボンの見出し帯"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RibbonBannerParameter(s); }
public sealed class RibbonBannerParameter : Q3Param
{
    public RibbonBannerParameter() : this(null) { }
    public RibbonBannerParameter(SharedDataStore? s) : base(s) { Text = "本日のテーマ"; Color1 = Hex(0xD7263D); Color2 = Hex(0x8F1527); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("リボンの見出し帯", "Text=文字", "Amount=曲がり具合", "Color1=帯の色", "Color2=折り返しの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0f, 0.5f)), W = MathF.Max(520f * u, Measure(Str, 64f * u).Width + 140f * u) * p, H = 118f * u, bend = Amt(c) * 70f * u; var o = P0(c); Vector2 At(float k, float dy) => o + new Vector2(k * W / 2f, dy + (k * k - 0.35f) * bend);
        foreach (int s in new[] { -1, 1 }) { Poly(dc, b, new List<Vector2> { At(s * 0.86f, -H * 0.12f + 26f * u), o + new Vector2(s * (W / 2f + 118f * u), H * 0.34f + bend * 0.7f), o + new Vector2(s * (W / 2f + 64f * u), H * 0.72f + bend * 0.7f), o + new Vector2(s * (W / 2f + 118f * u), H + bend * 0.7f), At(s * 0.86f, H * 0.84f) }, C2(a)); Poly(dc, b, new List<Vector2> { At(s, H / 2f), At(s * 0.86f, H / 2f), At(s * 0.86f, H * 0.84f) }, Col(Gfx.Mix(Color2, Hex(0x000000), 0.35f), a)); }
        var band = new List<Vector2>(); for (int i = 0; i <= 40; i++) band.Add(At(-1f + i / 20f, -H / 2f)); for (int i = 40; i >= 0; i--) band.Add(At(-1f + i / 20f, H / 2f)); Poly(dc, b, band, C1(a)); var line = new List<Vector2>(); for (int i = 0; i <= 40; i++) line.Add(At(-1f + i / 20f, -H / 2f + 10f * u)); Path(dc, b, line, Rgb(1, 1, 1, 0.34f * a), 3f * u); Say(dc, b, Str, 64f * u, At(0f, 0f), C3(a * Sub(c.Time, 0.2f, 0.25f)), Rgb(0, 0, 0, 0.35f * a), 3f * u, maxW: W - 44f * u);
    }
}

public sealed class RoyalCrownPlugin : QuickShapePlugin { public override string Name => "演出：王冠"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RoyalCrownParameter(s); }
public sealed class RoyalCrownParameter : Q3Param
{
    public RoyalCrownParameter() : this(null) { }
    public RoyalCrownParameter(SharedDataStore? s) : base(s) { Color2 = Hex(0xE53935); Color3 = Hex(0x29B6F6); }
    protected override double DefAmount => 45;
    static readonly Dictionary<string, ParamGuide> g = G("王冠", "Variant=色|1=金　2=銀|1|2", "Amount=傾き|50でまっすぐ", "Color2=宝石の色1", "Color3=宝石の色2");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), S = 230f * u * DrawKit.EaseOutBack(Intro(c, 0f, 0.45f)), t = c.Time; var o = P0(c); var mt = Variant == 2 ? Hex(0xD7DCE2) : Hex(0xF4C430); var dk = Gfx.Mix(mt, Hex(0x5A3A0A), 0.42f); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation((Amt(c) - 0.5f) * 1.2f, o) * prev;
        var body = new List<Vector2> { o + new Vector2(-S, S * 0.56f), o + new Vector2(-S * 1.08f, -S * 0.5f), o + new Vector2(-S * 0.52f, S * 0.02f), o + new Vector2(0, -S * 0.82f), o + new Vector2(S * 0.52f, S * 0.02f), o + new Vector2(S * 1.08f, -S * 0.5f), o + new Vector2(S, S * 0.56f) }; Poly(dc, b, body, Col(mt, a)); Outline(dc, b, body, Col(dk, a), 5f * u); RBox(dc, b, o + new Vector2(0, S * 0.56f), S * 2.08f, S * 0.32f, 8f * u, Col(Gfx.Mix(mt, Hex(0xFFFFFF), 0.18f), a)); RFrame(dc, b, o + new Vector2(0, S * 0.56f), S * 2.08f, S * 0.32f, 8f * u, Col(dk, a), 4f * u); foreach (var (x, y) in new[] { (-1.08f, -0.56f), (0f, -0.9f), (1.08f, -0.56f) }) { Dot(dc, b, o + new Vector2(x, y) * S, S * 0.13f, Col(Gfx.Mix(mt, Hex(0xFFFFFF), 0.25f), a)); Ring(dc, b, o + new Vector2(x, y) * S, S * 0.13f, Col(dk, a), 3f * u); } for (int i = -2; i <= 2; i++) { var q = o + new Vector2(i * S * 0.4f, S * 0.56f); var col = i % 2 == 0 ? C2(a) : C3(a); Poly(dc, b, new List<Vector2> { q - new Vector2(0, S * 0.12f), q + new Vector2(S * 0.1f, 0), q + new Vector2(0, S * 0.12f), q - new Vector2(S * 0.1f, 0) }, col); } Dot(dc, b, o + new Vector2(0, S * 0.1f), S * 0.15f, C2(a)); Dot(dc, b, o + new Vector2(-S * 0.04f, S * 0.05f), S * 0.04f, Rgb(1, 1, 1, 0.9f * a)); for (int i = 0; i < 3; i++) { float tw = MathF.Pow(MathF.Max(0f, MathF.Sin(t * 2.4f + i * 2.1f)), 8f); Star(dc, b, o + new Vector2((i - 1) * S * 0.75f, -S * (0.55f + (i % 2) * 0.28f)), S * 0.3f * tw, 4, 0.12f, 0f, Rgb(1, 1, 1, a)); } dc.Transform = prev;
    }
}

public sealed class BatteryLevelPlugin : QuickShapePlugin { public override string Name => "演出：電池の残量アイコン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BatteryLevelParameter(s); }
public sealed class BatteryLevelParameter : Q3Param
{
    public BatteryLevelParameter() : this(null) { }
    public BatteryLevelParameter(SharedDataStore? s) : base(s) { Text = "やる気の残量"; Color1 = Hex(0x4CDA64); Color2 = Hex(0xFF3B30); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount => 90;
    protected override double DefAmount2 => 8;
    static readonly Dictionary<string, ParamGuide> g = G("電池の残量", "Text=文字", "Amount=はじめの残量", "Amount2=終わりの残量", "Speed=減る速さ", "Color1=多い時の色", "Color2=少ない時の色", "Color3=枠と文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), k = DrawKit.EaseOut(Sub(c.Time * MathF.Max(0.05f, Spd(c)), 0.2f, 2.3f)), lv = Lerp(Amt(c), Amt2(c), k), W = 560f * u, H = 250f * u; var o = P0(c); bool low = lv < 0.2f; var col = low ? ((int)(c.Time * 4f) % 2 == 0 ? C2(a) : C2(0.35f * a)) : Col(Gfx.Mix(Color2, Color1, Math.Clamp((lv - 0.15f) / 0.3f, 0f, 1f)), a); RFrame(dc, b, o, W, H, 34f * u, C3(a), 13f * u); RBox(dc, b, o + new Vector2(W / 2f + 28f * u, 0), 34f * u, 96f * u, 12f * u, C3(a)); float iw = W - 44f * u, ih = H - 44f * u, fw = iw * lv; if (fw > 1f) RBox(dc, b, new(o.X - iw / 2f + fw / 2f, o.Y), fw, ih, 16f * u, col); Say(dc, b, (lv * 100f).ToString("0") + "%", 90f * u, o, C3(a), Rgb(0, 0, 0, 0.55f * a), 4f * u); Say(dc, b, Str, 52f * u, o + new Vector2(0, H / 2f + 62f * u), C3(a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
    }
}

public sealed class SignalStrengthPlugin : QuickShapePlugin { public override string Name => "演出：電波・Wi-Fiの強さ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SignalStrengthParameter(s); }
public sealed class SignalStrengthParameter : Q3Param
{
    public SignalStrengthParameter() : this(null) { }
    public SignalStrengthParameter(SharedDataStore? s) : base(s) { Count = 4; Color1 = Hex(0x4FE08A); Color2 = Hex(0x546E7A); Color3 = Hex(0xFF5252); }
    static readonly Dictionary<string, ParamGuide> g = G("電波の強さ", "Count=本数|0で圏外|0|4", "Variant=形|1=棒　2=扇|1|2", "Background=増えたり減ったりする", "Color1=ついた色", "Color2=消えた色", "Color3=圏外の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c); var o = P0(c); int lv = Math.Clamp(Count, 0, 4); if (Background) { int step = (int)(c.Time * 1.6f) % 8; lv = Math.Min(lv, step <= 4 ? step : 8 - step); }
        if (Variant == 2) { var base0 = o + new Vector2(0, 150f * u); Dot(dc, b, base0, 30f * u, lv > 0 ? C1(a) : C2(a)); for (int i = 1; i <= 3; i++) Arc(dc, b, base0, (72f + (i - 1) * 76f) * u, -MathF.PI * 0.77f, -MathF.PI * 0.23f, lv > i ? C1(a) : C2(a), 34f * u); }
        else for (int i = 0; i < 4; i++) { float H = (70f + i * 72f) * u, x = o.X + (i - 1.5f) * 96f * u; RBox(dc, b, new(x, o.Y + 150f * u - H / 2f), 66f * u, H, 12f * u, lv > i ? C1(a) : C2(a)); }
        if (lv == 0) { Ln(dc, b, o + new Vector2(-180f * u, -160f * u), o + new Vector2(180f * u, 180f * u), C3(a), 18f * u); Say(dc, b, "圏外", 74f * u, o + new Vector2(0, 245f * u), C3(a), Rgb(0, 0, 0, 0.6f * a), 4f * u); }
    }
}

public sealed class StampCardPlugin : QuickShapePlugin { public override string Name => "演出：スタンプカード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new StampCardParameter(s); }
public sealed class StampCardParameter : Q3Param
{
    public StampCardParameter() : this(null) { }
    public StampCardParameter(SharedDataStore? s) : base(s) { Text = "達成！"; Count = 10; Color1 = Hex(0xFFF7E3); Color2 = Hex(0xE53935); Color3 = Hex(0x6D4C41); }
    protected override double DefAmount => 100;
    static readonly Dictionary<string, ParamGuide> g = G("スタンプカード", "Text=満ちた時の文字", "Count=マスの数||2|30", "Amount=押す割合", "Speed=押す速さ", "Color1=カードの色", "Color2=スタンプの色", "Color3=線の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 2, 30), cols = Math.Min(n, n <= 10 ? 5 : 6), rows = (n + cols - 1) / cols, push = (int)MathF.Round(Amt(c) * n); float u = U(c), cell = MathF.Min(190f * u, 1200f * u / cols), W = cell * cols + 70f * u, H = cell * rows + 150f * u, t = c.Time * MathF.Max(0.05f, Spd(c)); var o = P0(c); RBox(dc, b, o + new Vector2(8f, 10f) * u, W, H, 24f * u, Rgb(0, 0, 0, 0.26f * a)); RBox(dc, b, o, W, H, 24f * u, C1(a)); RFrame(dc, b, o, W - 18f * u, H - 18f * u, 18f * u, C3(a), 3f * u); Say(dc, b, "STAMP CARD", 44f * u, new(o.X, o.Y - H / 2f + 56f * u), C3(a));
        for (int i = 0; i < n; i++) { var q = new Vector2(o.X - cell * cols / 2f + (i % cols + 0.5f) * cell, o.Y - H / 2f + 112f * u + (i / cols + 0.5f) * cell); Ring(dc, b, q, cell * 0.39f, C3(0.6f * a), 3f * u); Say(dc, b, (i + 1).ToString(), cell * 0.25f, q, C3(0.45f * a)); if (i >= push) continue; float k = Sub(t, 0.2f + i * 0.2f, 0.16f); if (k <= 0f) continue; float sc = 1.7f - 0.7f * DrawKit.EaseOut(k); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation((Rnd(i) - 0.5f) * 0.5f, q) * prev; Ring(dc, b, q, cell * 0.37f * sc, C2(a * k), 6f * u); Star(dc, b, q, cell * 0.25f * sc, 5, 0.46f, -MathF.PI / 2f, C2(a * k)); dc.Transform = prev; }
        if (push >= n) { float f = DrawKit.EaseOutBack(Sub(t, 0.45f + n * 0.2f, 0.32f)); if (f > 0f) { var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(-0.16f, o) * prev; Say(dc, b, Str, 150f * u, o, C2(a), Rgb(1, 1, 1, a), 8f * u, maxW: W * 0.9f, scale: f); dc.Transform = prev; } }
    }
}

public sealed class TearTicketPlugin : QuickShapePlugin { public override string Name => "演出：チケット（半券がちぎれる）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TearTicketParameter(s); }
public sealed class TearTicketParameter : Q3Param
{
    public TearTicketParameter() : this(null) { }
    public TearTicketParameter(SharedDataStore? s) : base(s) { Text = "ゆっくり大感謝祭\n2026.12.24\nA列 12番"; Color1 = Hex(0xFFD54A); Color2 = Hex(0x263238); Color3 = Hex(0xE53935); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("チケット", "Text=文字|1行目=公演名　2行目=日付　3行目=座席", "Amount=ちぎれる時刻|0〜100 = 0〜5秒", "Color1=券の色", "Color2=文字の色", "Color3=帯の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; string S0 = r.Count > 0 ? r[0] : "", S1 = r.Count > 1 ? r[1] : "", S2 = r.Count > 2 ? r[2] : ""; float u = U(c), k = DrawKit.EaseOut(Sub(c.Time, Amt(c) * 5f, 0.55f)), W = 980f * u, H = 410f * u; var o = P0(c) - new Vector2(40f * u, 0); float perf = o.X + W * 0.27f; var main = new Vector2(o.X - W * 0.115f, o.Y); RBox(dc, b, main, W * 0.77f, H, 22f * u, C1(a)); Box(dc, b, main.X - W * 0.385f, o.Y - H / 2f, main.X - W * 0.34f, o.Y + H / 2f, C3(a)); SayAt(dc, b, "ADMIT ONE", 28f * u, new(main.X - W * 0.31f, o.Y - H * 0.37f), C3(a)); SayAt(dc, b, S0, 62f * u, new(main.X - W * 0.31f, o.Y - H * 0.2f), C2(a), maxW: W * 0.65f); SayAt(dc, b, S1, 42f * u, new(main.X - W * 0.31f, o.Y + H * 0.14f), C2(a));
        var stub = new Vector2(perf + W * 0.115f + k * 140f * u, o.Y + k * 90f * u); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(k * 0.32f, stub) * prev; RBox(dc, b, stub, W * 0.23f, H, 22f * u, C1(a)); for (int i = 0; i < 8; i++) Box(dc, b, stub.X - W * 0.075f + i * 19f * u, o.Y - H * 0.34f + k * 90f * u, stub.X - W * 0.075f + (i + (i % 3 == 0 ? 0.55f : 0.3f)) * 19f * u, o.Y - H * 0.08f + k * 90f * u, C2(a)); Say(dc, b, S2, 34f * u, stub + new Vector2(0, H * 0.22f), C2(a), maxW: W * 0.2f); dc.Transform = prev; if (k < 0.04f) for (float y = o.Y - H / 2f + 8f * u; y < o.Y + H / 2f; y += 24f * u) Ln(dc, b, new(perf, y), new(perf, y + 12f * u), C2(0.6f * a), 3f * u); else for (int i = 0; i < 18; i++) { float y = o.Y - H / 2f + i / 17f * H, e = Sub(k, 0f, 0.7f); Dot(dc, b, new(perf + 16f * u + e * Rnd(i) * 110f * u, y + e * e * 120f * u), (2f + Rnd(i + 3) * 3f) * u, C1(a * (1f - e))); }
    }
}

public sealed class PriceTagPlugin : QuickShapePlugin { public override string Name => "演出：値札・セールのタグ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PriceTagParameter(s); }
public sealed class PriceTagParameter : Q3Param
{
    public PriceTagParameter() : this(null) { }
    public PriceTagParameter(SharedDataStore? s) : base(s) { Text = "¥12,800\n¥7,980"; Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("値札", "Text=値段|1行目=元の値段　2行目=セール価格", "Color1=札の色", "Color2=文字の色", "Color3=SALEの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; string old = r.Count > 0 ? r[0] : "", now = r.Count > 1 ? r[1] : ""; float u = U(c), t = c.Time, sw = MathF.Sin(t * 4.2f) * MathF.Exp(-t * 0.7f) * 0.25f + MathF.Sin(t * 1.2f) * 0.025f; var pin = P0(c) - new Vector2(0, 320f * u); Dot(dc, b, pin, 11f * u, Rgb(0.75f, 0.75f, 0.78f, a)); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(sw, pin) * prev; var hole = pin + new Vector2(0, 140f * u); Ln(dc, b, pin, hole, Rgb(0.9f, 0.86f, 0.75f, a), 4f * u); var q = pin + new Vector2(0, 345f * u); var tag = new List<Vector2> { q + new Vector2(-82f * u, -240f * u), q + new Vector2(82f * u, -240f * u), q + new Vector2(205f * u, -125f * u), q + new Vector2(205f * u, 235f * u), q + new Vector2(-205f * u, 235f * u), q + new Vector2(-205f * u, -125f * u) }; Poly(dc, b, tag, C1(a)); Outline(dc, b, tag, Rgb(0.45f, 0.05f, 0.05f, a), 4f * u); Dot(dc, b, hole, 22f * u, Rgb(1, 1, 1, a)); Ring(dc, b, hole, 22f * u, C3(a), 5f * u); Say(dc, b, "SALE", 74f * u, q + new Vector2(0, -60f * u), C3(a)); var oldP = q + new Vector2(0, 42f * u); Say(dc, b, old, 46f * u, oldP, C2(0.82f * a)); float ow = Measure(old, 46f * u).Width, strike = DrawKit.EaseOut(Sub(t, 0.35f, 0.3f)); Ln(dc, b, oldP - new Vector2(ow / 2f, 0), oldP + new Vector2(-ow / 2f + ow * strike, 0), C3(a), 5f * u); Say(dc, b, now, 84f * u, q + new Vector2(0, 142f * u), C2(a), Rgb(0.4f, 0.03f, 0.03f, a), 3f * u, maxW: 380f * u, scale: DrawKit.EaseOutBack(Sub(t, 0.62f, 0.3f))); dc.Transform = prev;
    }
}

public sealed class AlarmClockRingPlugin : QuickShapePlugin { public override string Name => "演出：目覚まし時計が鳴る"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AlarmClockRingParameter(s); }
public sealed class AlarmClockRingParameter : Q3Param
{
    public AlarmClockRingParameter() : this(null) { }
    public AlarmClockRingParameter(SharedDataStore? s) : base(s) { Text = "7:00"; Color1 = Hex(0xE53935); Color2 = Hex(0xFFF9EC); Color3 = Hex(0xFFD54A); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("目覚まし時計", "Text=時刻|「7:00」のように書きます", "Amount=揺れの強さ", "Color1=本体の色", "Color2=文字盤の色", "Color3=ベルと音の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, s0 = Amt(c), rot = MathF.Sin(t * 42f) * 0.075f * s0, sx = MathF.Sin(t * 37f) * 12f * u * s0, R = 240f * u; var o = P0(c) + new Vector2(sx, 0); var pts = Str.Split([':', '：']); float hh = pts.Length > 0 ? Num(pts[0], 7f) : 7f, mm = pts.Length > 1 ? Num(pts[1]) : 0f; for (int i = 0; i < 3; i++) { float k = Gfx.Mod(t * 3f + i / 3f, 1f); foreach (int s in new[] { -1, 1 }) Arc(dc, b, P0(c) - new Vector2(0, 40f * u), (310f + k * 140f) * u, s < 0 ? MathF.PI * 1.08f : -MathF.PI * 0.32f, s < 0 ? MathF.PI * 1.32f : -MathF.PI * 0.08f, C3(a * (1f - k)), 9f * u); }
        var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(rot, o) * prev; foreach (int s in new[] { -1, 1 }) { Ln(dc, b, o + new Vector2(s * 130f * u, 195f * u), o + new Vector2(s * 185f * u, 300f * u), C1(a), 26f * u); Pie(dc, b, o + new Vector2(s * 150f * u, -222f * u), 96f * u, MathF.PI + s * 0.4f, MathF.Tau + s * 0.4f, C3(a)); } Ln(dc, b, o - new Vector2(0, 240f * u), o - new Vector2(0, 298f * u), C1(a), 18f * u); float ham = MathF.Sin(t * 58f) * 58f * u; Ln(dc, b, o - new Vector2(0, 290f * u), o + new Vector2(ham, -335f * u), Rgb(0.45f, 0.48f, 0.52f, a), 9f * u); Dot(dc, b, o + new Vector2(ham, -335f * u), 16f * u, Rgb(0.45f, 0.48f, 0.52f, a)); Dot(dc, b, o, R, C1(a)); Dot(dc, b, o, R * 0.82f, C2(a)); for (int i = 0; i < 12; i++) { float th = i * MathF.PI / 6f; Ln(dc, b, Pol(o, R * 0.68f, th), Pol(o, R * 0.76f, th), Rgb(0.15f, 0.15f, 0.18f, a), (i % 3 == 0 ? 8f : 3f) * u); } float hA = (hh % 12f + mm / 60f) / 12f * MathF.Tau - MathF.PI / 2f, mA = mm / 60f * MathF.Tau - MathF.PI / 2f; Ln(dc, b, o, Pol(o, R * 0.42f, hA), Rgb(0.12f, 0.12f, 0.15f, a), 13f * u); Ln(dc, b, o, Pol(o, R * 0.64f, mA), Rgb(0.12f, 0.12f, 0.15f, a), 8f * u); Ln(dc, b, o, Pol(o, R * 0.56f, t * 8f), C1(a), 3f * u); Dot(dc, b, o, 13f * u, C1(a)); dc.Transform = prev;
    }
}

public sealed class OnAirMicPlugin : QuickShapePlugin { public override string Name => "演出：マイクとON AIRランプ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new OnAirMicParameter(s); }
public sealed class OnAirMicParameter : Q3Param
{
    public OnAirMicParameter() : this(null) { }
    public OnAirMicParameter(SharedDataStore? s) : base(s) { Text = "ON AIR"; Background = true; Color1 = Hex(0xFF2D2D); Color2 = Hex(0x596370); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("マイクとON AIR", "Text=ランプの文字", "Background=点滅する", "Color1=ランプの色", "Color2=マイクの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), lit = Background ? 0.55f + 0.45f * MathF.Max(0f, MathF.Sin(c.Time * 4f)) : 1f; var o = P0(c); var lamp = o + new Vector2(290f * u, -220f * u); Dot(dc, b, lamp, 310f * u, C1(0.08f * lit * a)); RBox(dc, b, lamp, 520f * u, 190f * u, 24f * u, Rgb(0.08f, 0.08f, 0.1f, a)); RBox(dc, b, lamp, 480f * u, 150f * u, 14f * u, Col(Gfx.Mix(Hex(0x3A0A0A), Color1, lit), a)); Say(dc, b, Str, 88f * u, lamp, C3((0.55f + 0.45f * lit) * a), maxW: 450f * u);
        var mic = o + new Vector2(-300f * u, -135f * u); var col = C2(a); Ln(dc, b, mic + new Vector2(0, 170f * u), o + new Vector2(-300f * u, 390f * u), col, 20f * u); Oval(dc, b, o + new Vector2(-300f * u, 405f * u), 165f * u, 34f * u, col); Arc(dc, b, mic + new Vector2(0, 42f * u), 118f * u, 0f, MathF.PI, col, 16f * u); RBox(dc, b, mic, 142f * u, 280f * u, 70f * u, Rgb(0.72f, 0.75f, 0.8f, a)); for (int i = -5; i <= 5; i++) Ln(dc, b, mic + new Vector2(-66f * u, i * 23f * u - 26f * u), mic + new Vector2(66f * u, i * 23f * u - 26f * u), Rgb(0.35f, 0.38f, 0.42f, 0.7f * a), 3f * u); RBox(dc, b, mic + new Vector2(0, 88f * u), 148f * u, 24f * u, 8f * u, Rgb(0.32f, 0.35f, 0.4f, a));
    }
}

public sealed class HeadphoneNotesPlugin : QuickShapePlugin { public override string Name => "演出：ヘッドホンと音符"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HeadphoneNotesParameter(s); }
public sealed class HeadphoneNotesParameter : Q3Param
{
    public HeadphoneNotesParameter() : this(null) { }
    public HeadphoneNotesParameter(SharedDataStore? s) : base(s) { Count = 10; Color1 = Hex(0x37474F); Color2 = Hex(0xFF4F9A); Color3 = Hex(0x54E8FF); }
    static readonly Dictionary<string, ParamGuide> g = G("ヘッドホンと音符", "Count=音符の数||1|40", "Speed=速さ", "Color1=本体の色", "Color2=音符の色1", "Color3=音符の色2");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), R = 270f * u, beat = MathF.Pow(MathF.Abs(MathF.Sin(t * 3.14f)), 4f); var o = P0(c); Arc(dc, b, o + new Vector2(0, 50f * u), R, MathF.PI, MathF.Tau, C1(a), 34f * u); foreach (int s in new[] { -1, 1 }) { var e = o + new Vector2(s * R, 95f * u); RBox(dc, b, e - new Vector2(s * 22f * u, 0), 60f * u, 170f * u, 22f * u, C1(a)); Oval(dc, b, e + new Vector2(s * 26f * u, 0), 58f * u * (1f + beat * 0.05f), 118f * u * (1f + beat * 0.05f), s < 0 ? C2(a) : C3(a)); Oval(dc, b, e + new Vector2(s * 26f * u, 0), 31f * u, 70f * u, Rgb(1, 1, 1, 0.3f * a)); }
        for (int i = 0; i < Math.Clamp(Count, 1, 40); i++) { float k = Gfx.Mod(t * (0.2f + Rnd(i) * 0.18f) + Rnd(i + 3), 1f), side = i % 2 == 0 ? -1f : 1f, x = o.X + side * (R + 60f * u + k * (70f + Rnd(i + 5) * 210f) * u) + MathF.Sin(k * 8f + i) * 22f * u, y = o.Y + 100f * u - k * 460f * u, S = (15f + Rnd(i + 8) * 13f) * u, al = MathF.Min(k * 6f, 1f) * (1f - Sub(k, 0.72f, 0.28f)); var col = i % 3 == 0 ? C3(a * al) : C2(a * al); var q = new Vector2(x, y); Oval(dc, b, q, S * 1.15f, S * 0.86f, col); Ln(dc, b, q + new Vector2(S, 0), q + new Vector2(S, -S * 3.2f), col, S * 0.26f); if (i % 2 == 0) { var q2 = q + new Vector2(S * 2.5f, -S * 0.5f); Oval(dc, b, q2, S * 1.15f, S * 0.86f, col); Ln(dc, b, q2 + new Vector2(S, 0), q2 + new Vector2(S, -S * 3.2f), col, S * 0.26f); Ln(dc, b, q + new Vector2(S, -S * 3.2f), q2 + new Vector2(S, -S * 3.2f), col, S * 0.56f); } else Ln(dc, b, q + new Vector2(S, -S * 3.2f), q + new Vector2(S * 2f, -S * 2.4f), col, S * 0.32f); }
    }
}

public sealed class OpeningNotebookPlugin : QuickShapePlugin { public override string Name => "演出：開くノート"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new OpeningNotebookParameter(s); }
public sealed class OpeningNotebookParameter : Q3Param
{
    public OpeningNotebookParameter() : this(null) { }
    public OpeningNotebookParameter(SharedDataStore? s) : base(s) { Text = "今日のまとめ\n・ゆっくり確認する\n・図で説明する\n・最後に復習する"; Color1 = Hex(0x2F6FBB); Color2 = Hex(0xFFFDF4); Color3 = Hex(0x263238); }
    static readonly Dictionary<string, ParamGuide> g = G("開くノート", "Text=文字|1行に1つ", "Speed=書く速さ", "Color1=表紙の色", "Color2=紙の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), t = c.Time, open = DrawKit.EaseOut(Sub(t, 0.15f, 0.6f)), PW = 620f * u, H = 820f * u; var o = P0(c); var sp = o - new Vector2(PW * (1f - open) * 0.5f, 0); Box(dc, b, sp.X, o.Y - H / 2f, sp.X + PW, o.Y + H / 2f, C2(a)); Frame(dc, b, sp.X, o.Y - H / 2f, sp.X + PW, o.Y + H / 2f, Rgb(0.2f, 0.25f, 0.3f, 0.5f * a), 2f * u); for (int i = 1; i < 13; i++) { float y = o.Y - H / 2f + 80f * u + i * 56f * u; Ln(dc, b, new(sp.X + 28f * u, y), new(sp.X + PW - 26f * u, y), Rgb(0.45f, 0.65f, 0.86f, 0.55f * a), 2f * u); }
        if (open > 0.4f) { float prog = MathF.Max(0f, (t - 0.8f) * Spd(c)); int chars = (int)(prog / 0.05f); float y = o.Y - H / 2f + 88f * u; for (int i = 0; i < r.Count && i < 12; i++) { int n = Math.Clamp(chars, 0, r[i].Length); chars -= r[i].Length + 3; if (n > 0) SayAt(dc, b, r[i][..n], i == 0 ? 46f * u : 38f * u, new(sp.X + 46f * u, y + 2f * u), i == 0 ? C1(a) : C3(a), maxW: PW - 80f * u); y += i == 0 ? 112f * u : 56f * u; } }
        float cvA = open * MathF.PI, sx = MathF.Cos(cvA); if (sx > 0f) { Box(dc, b, sp.X, o.Y - H / 2f - 8f * u, sp.X + PW * sx, o.Y + H / 2f + 8f * u, C1(a)); if (sx > 0.45f) { var lab = new Vector2(sp.X + PW * sx / 2f, o.Y - 130f * u); RBox(dc, b, lab, 370f * u * sx, 130f * u, 10f * u, Rgb(1, 1, 1, 0.94f * a)); Say(dc, b, "NOTE", 54f * u, lab, C1(a)); } } else { float cw = PW * -sx; Box(dc, b, sp.X - cw, o.Y - H / 2f - 8f * u, sp.X, o.Y + H / 2f + 8f * u, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.08f), a)); Box(dc, b, sp.X - cw + 14f * u, o.Y - H / 2f + 8f * u, sp.X, o.Y + H / 2f - 8f * u, C2(0.92f * a)); }
        for (int i = 0; i < 12; i++) { float y = o.Y - H / 2f + 38f * u + i * 68f * u; Ring(dc, b, new(sp.X, y), 15f * u, Rgb(0.55f, 0.58f, 0.62f, a), 4f * u); }
    }
}

public sealed class TrashTossPlugin : QuickShapePlugin { public override string Name => "演出：ごみ箱に捨てる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TrashTossParameter(s); }
public sealed class TrashTossParameter : Q3Param
{
    public TrashTossParameter() : this(null) { }
    public TrashTossParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0xF3F0E5); Color2 = Hex(0x607D8B); Color3 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("ごみ箱に捨てる", "Background=入る|オフにすると外れます", "Variant=投げる向き|1=左から　2=右から|1|2", "Color1=紙くずの色", "Color2=ごみ箱の色", "Color3=入った時の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, dir = Variant == 2 ? 1f : -1f, k = Sub(t, 0.1f, 0.9f); var o = P0(c); var bin = o + new Vector2(-dir * 300f * u, 170f * u); var from = o + new Vector2(dir * 480f * u, 130f * u); var goal = Background ? bin - new Vector2(0, 80f * u) : bin + new Vector2(dir * 165f * u, -90f * u); bool done = k >= 1f; var q = Vector2.Lerp(from, goal, k) - new Vector2(0, MathF.Sin(k * MathF.PI) * 380f * u);
        if (done && !Background) { float e = Sub(t, 1f, 0.7f), hop = MathF.Abs(MathF.Sin(e * MathF.PI * 2f)) * (1f - e) * 110f * u; q = Vector2.Lerp(goal, goal + new Vector2(dir * 230f * u, 240f * u), DrawKit.EaseOut(e)) - new Vector2(0, hop); }
        if (!(done && Background)) { var ball = new List<Vector2>(); for (int i = 0; i < 12; i++) ball.Add(Pol(q, (42f + Rnd(i) * 22f) * u, i * MathF.Tau / 12f + t * 7f * (done ? 0.1f : 1f))); Poly(dc, b, ball, C1(a)); for (int i = 0; i < 12; i += 3) Ln(dc, b, q, ball[i], Rgb(0.45f, 0.42f, 0.35f, 0.55f * a), 2f * u); }
        Poly(dc, b, new List<Vector2> { bin + new Vector2(-130f * u, -85f * u), bin + new Vector2(130f * u, -85f * u), bin + new Vector2(98f * u, 205f * u), bin + new Vector2(-98f * u, 205f * u) }, C2(a)); RBox(dc, b, bin - new Vector2(0, 85f * u), 300f * u, 28f * u, 10f * u, Col(Gfx.Mix(Color2, Hex(0xFFFFFF), 0.25f), a)); for (int i = -1; i <= 1; i++) Ln(dc, b, bin + new Vector2(i * 62f * u, -42f * u), bin + new Vector2(i * 47f * u, 170f * u), Rgb(0, 0, 0, 0.3f * a), 8f * u);
        if (done) { float e = Sub(t, 1f, 0.5f); if (Background) { for (int i = 0; i < 10; i++) Star(dc, b, Pol(bin - new Vector2(0, 110f * u), 40f * u + e * 180f * u, i * MathF.Tau / 10f), 22f * u * (1f - e), 4, 0.25f, 0f, C3(a)); Say(dc, b, "ナイス！", 80f * u, bin - new Vector2(0, 280f * u), C3(a * MathF.Min(1f, e * 3f)), Rgb(0, 0, 0, 0.7f * a), 5f * u); } else Say(dc, b, "外れた…", 72f * u, bin - new Vector2(0, 260f * u), Rgb(1, 1, 1, a * MathF.Min(1f, e * 3f)), Rgb(0, 0, 0, 0.7f * a), 5f * u); }
    }
}

public sealed class InstantCameraPlugin : QuickShapePlugin { public override string Name => "演出：インスタントカメラ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new InstantCameraParameter(s); }
public sealed class InstantCameraParameter : Q3Param
{
    public InstantCameraParameter() : this(null) { }
    public InstantCameraParameter(SharedDataStore? s) : base(s) { Text = "思い出の一枚"; Color1 = Hex(0xE9E2D0); Color2 = Hex(0x37474F); Color3 = Hex(0x7EC8E3); }
    static readonly Dictionary<string, ParamGuide> g = G("インスタントカメラ", "Text=写真の下の文字", "Color1=カメラの色", "Color2=レンズの色", "Color3=写真の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, flash = 1f - Sub(t, 0.35f, 0.3f), out0 = DrawKit.EaseOut(Sub(t, 0.55f, 0.75f)), dev = Sub(t, 1.2f, 1.5f); var o = P0(c) - new Vector2(0, 170f * u); var ph = o + new Vector2(0, 95f * u + out0 * 390f * u); float PW = 360f * u, PH = 420f * u; if (out0 > 0f) { var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(out0 * 0.05f, ph) * prev; RBox(dc, b, ph, PW, PH, 5f * u, Rgb(0.99f, 0.98f, 0.95f, a)); float ix0 = ph.X - PW * 0.43f, ix1 = ph.X + PW * 0.43f, iy0 = ph.Y - PH * 0.44f, iy1 = ph.Y + PH * 0.25f; Box(dc, b, ix0, iy0, ix1, iy1, Rgb(0.13f, 0.14f, 0.15f, a)); Shade(dc, new(0, iy0), new(0, iy1), C3(dev * a), Rgb(1, 0.92f, 0.76f, dev * a), ix0, iy0, ix1, iy1); Dot(dc, b, new(ix1 - 58f * u, iy0 + 56f * u), 28f * u, Rgb(1, 0.82f, 0.25f, dev * a)); Poly(dc, b, new List<Vector2> { new(ix0, iy1), new(ix0 + (ix1 - ix0) * 0.34f, iy0 + (iy1 - iy0) * 0.46f), new(ix0 + (ix1 - ix0) * 0.58f, iy1 - (iy1 - iy0) * 0.22f), new(ix0 + (ix1 - ix0) * 0.76f, iy0 + (iy1 - iy0) * 0.56f), new(ix1, iy1) }, Rgb(0.26f, 0.56f, 0.36f, dev * a)); Say(dc, b, Str, 34f * u, new(ph.X, ph.Y + PH * 0.36f), Rgb(0.18f, 0.18f, 0.2f, dev * a), maxW: PW * 0.9f); dc.Transform = prev; }
        RBox(dc, b, o, 560f * u, 370f * u, 34f * u, C1(a)); Box(dc, b, o.X - 280f * u, o.Y + 92f * u, o.X + 280f * u, o.Y + 118f * u, Rgb(0.82f, 0.28f, 0.25f, a)); Box(dc, b, o.X - 200f * u, o.Y + 150f * u, o.X + 200f * u, o.Y + 164f * u, Rgb(0.12f, 0.12f, 0.13f, a)); Dot(dc, b, o - new Vector2(0, 20f * u), 118f * u, Rgb(0.2f, 0.21f, 0.23f, a)); Dot(dc, b, o - new Vector2(0, 20f * u), 86f * u, C2(a)); Dot(dc, b, o - new Vector2(0, 20f * u), 46f * u, Rgb(0.05f, 0.07f, 0.1f, a)); Dot(dc, b, o - new Vector2(18f * u, 38f * u), 13f * u, Rgb(1, 1, 1, 0.6f * a)); RBox(dc, b, o + new Vector2(190f * u, -115f * u), 90f * u, 58f * u, 8f * u, Rgb(0.95f, 0.95f, 0.88f, a)); Dot(dc, b, o + new Vector2(-205f * u, -120f * u), 26f * u, Rgb(0.82f, 0.2f, 0.18f, a)); if (flash > 0f && t >= 0.35f) { Dot(dc, b, o + new Vector2(190f * u, -115f * u), 520f * u * flash, Rgb(1, 1, 1, 0.55f * flash * a)); Box(dc, b, 0, 0, c.Width, c.Height, Rgb(1, 1, 1, 0.75f * flash * a)); }
    }
}

public sealed class ClapperboardPlugin : QuickShapePlugin { public override string Name => "演出：映画のカチンコ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ClapperboardParameter(s); }
public sealed class ClapperboardParameter : Q3Param
{
    public ClapperboardParameter() : this(null) { }
    public ClapperboardParameter(SharedDataStore? s) : base(s) { Text = "ゆっくり物語\n12\n3"; Color1 = Hex(0x1E1F24); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("カチンコ", "Text=内容|1行目=題名　2行目=シーン番号　3行目=テイク", "Color1=本体の色", "Color2=線と文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), t = c.Time, W = 760f * u, H = 500f * u, close = t < 0.45f ? 0f : DrawKit.EaseOut(Sub(t, 0.45f, 0.1f)), open = (1f - close) * 0.46f, shake = t > 0.55f ? MathF.Sin(t * 60f) * 5f * u * (1f - Sub(t, 0.55f, 0.25f)) : 0f; var o = P0(c) + new Vector2(shake, 56f * u); RBox(dc, b, o, W, H, 16f * u, C1(a)); float top = o.Y - H / 2f; for (int x = 0; x < 9; x++) Poly(dc, b, new List<Vector2> { new(o.X - W / 2f + x * W / 8f, top), new(o.X - W / 2f + (x + 0.5f) * W / 8f, top), new(o.X - W / 2f + (x + 0.25f) * W / 8f, top + 42f * u), new(o.X - W / 2f + (x - 0.25f) * W / 8f, top + 42f * u) }, x % 2 == 0 ? C2(a) : C1(a));
        SayAt(dc, b, "TITLE", 26f * u, new(o.X - W / 2f + 32f * u, top + 58f * u), C2(0.7f * a)); SayAt(dc, b, r.Count > 0 ? r[0] : "", 56f * u, new(o.X - W / 2f + 32f * u, top + 94f * u), C2(a), maxW: W - 64f * u); Ln(dc, b, new(o.X - W / 2f, top + 186f * u), new(o.X + W / 2f, top + 186f * u), C2(a), 4f * u); Ln(dc, b, new(o.X, top + 186f * u), new(o.X, o.Y + H / 2f), C2(a), 4f * u); foreach (var (x, lbl, idx) in new[] { (-W / 4f, "SCENE", 1), (W / 4f, "TAKE", 2) }) { Say(dc, b, lbl, 30f * u, new(o.X + x, top + 228f * u), C2(0.7f * a)); Say(dc, b, r.Count > idx ? r[idx] : "", 112f * u, new(o.X + x, top + 340f * u), C2(a)); }
        var hinge = new Vector2(o.X - W / 2f + 14f * u, top - 8f * u); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(-open, hinge) * prev; float y0 = top - 66f * u, y1 = top - 4f * u; Box(dc, b, o.X - W / 2f, y0, o.X + W / 2f, y1, C1(a)); for (int x = 0; x < 9; x++) Poly(dc, b, new List<Vector2> { new(o.X - W / 2f + (x - 0.25f) * W / 8f, y0), new(o.X - W / 2f + (x + 0.25f) * W / 8f, y0), new(o.X - W / 2f + (x + 0.5f) * W / 8f, y1), new(o.X - W / 2f + x * W / 8f, y1) }, x % 2 == 1 ? C2(a) : C1(a)); dc.Transform = prev; Dot(dc, b, hinge, 10f * u, Rgb(0.7f, 0.72f, 0.75f, a)); float pop = Sub(t, 0.54f, 0.26f); if (pop is > 0f and < 1f) for (int i = 0; i < 10; i++) { float th = -MathF.PI * (i + 0.5f) / 10f; var q = new Vector2(o.X, top - 10f * u); Ln(dc, b, Pol(q, (W * 0.52f + pop * 40f * u), th), Pol(q, W * 0.52f + (24f + pop * 80f) * u, th), C2(a * (1f - pop)), 5f * u); }
    }
}

public sealed class PlacardSignPlugin : QuickShapePlugin { public override string Name => "演出：吹き出し型の看板"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PlacardSignParameter(s); }
public sealed class PlacardSignParameter : Q3Param
{
    public PlacardSignParameter() : this(null) { }
    public PlacardSignParameter(SharedDataStore? s) : base(s) { Text = "ここ重要！"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x8A5B36); Color3 = Hex(0xE53935); }
    protected override double DefAmount => 35;
    static readonly Dictionary<string, ParamGuide> g = G("プラカード", "Text=文字", "Variant=形|1=四角　2=丸　3=吹き出し　4=矢印|1|4", "Amount=揺れ", "Color1=板の色", "Color2=棒の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), sway = MathF.Sin(c.Time * 3f) * Amt(c) * 0.28f, p = DrawKit.EaseOutBack(Intro(c, 0f, 0.4f)), W = 620f * u * p, H = 330f * u * p; var hand = P0(c) + new Vector2(0, 320f * u); var q = hand - new Vector2(0, 470f * u); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(sway, hand) * prev; Ln(dc, b, hand + new Vector2(0, 120f * u), q, C2(a), 30f * u);
        if (Variant == 2) { Oval(dc, b, q, W * 0.52f, H * 0.68f, C1(a)); b.Color = C3(a); dc.DrawEllipse(new Ellipse(q, W * 0.52f, H * 0.68f), b, 8f * u); }
        else if (Variant == 3) { Poly(dc, b, new List<Vector2> { q + new Vector2(-70f * u, H * 0.42f), q + new Vector2(70f * u, H * 0.42f), q + new Vector2(0, H * 0.82f) }, C3(a)); RBox(dc, b, q, W, H, 70f * u, C1(a)); RFrame(dc, b, q, W, H, 70f * u, C3(a), 8f * u); }
        else if (Variant == 4) { var ar = new List<Vector2> { q + new Vector2(-W / 2f, -H * 0.34f), q + new Vector2(W * 0.2f, -H * 0.34f), q + new Vector2(W * 0.2f, -H * 0.58f), q + new Vector2(W * 0.62f, 0), q + new Vector2(W * 0.2f, H * 0.58f), q + new Vector2(W * 0.2f, H * 0.34f), q + new Vector2(-W / 2f, H * 0.34f) }; Poly(dc, b, ar, C1(a)); Outline(dc, b, ar, C3(a), 8f * u); }
        else { RBox(dc, b, q, W, H, 14f * u, C1(a)); RFrame(dc, b, q, W, H, 14f * u, C3(a), 8f * u); }
        Say(dc, b, Str, 76f * u, q, C3(a), maxW: W * 0.86f, scale: MathF.Max(0.01f, p)); RBox(dc, b, hand + new Vector2(0, 8f * u), 94f * u, 72f * u, 28f * u, Rgb(1, 0.82f, 0.68f, a)); dc.Transform = prev;
    }
}

public sealed class GoldFramePlugin : QuickShapePlugin { public override string Name => "演出：金の額縁"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GoldFrameParameter(s); }
public sealed class GoldFrameParameter : Q3Param
{
    public GoldFrameParameter() : this(null) { }
    public GoldFrameParameter(SharedDataStore? s) : base(s) { Text = "傑作"; Color1 = Hex(0xD4AF37); Color2 = Hex(0x7A5A12); Color3 = Hex(0x1A1A1E); }
    protected override double DefAmount => 45;
    static readonly Dictionary<string, ParamGuide> g = G("金の額縁", "Text=名札の文字|空にすると名札を出しません", "Amount=縁の太さ", "Background=中を塗る|オフなら下の画像や動画が見えます", "Color1=金の色", "Color2=影の色", "Color3=中の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), p = DrawKit.EaseOut(Intro(c, 0f, 0.4f)), W = 1320f * u * p, H = 760f * u * p, th = (34f + Amt(c) * 78f) * u; var o = P0(c); float x0 = o.X - W / 2f, y0 = o.Y - H / 2f, x1 = o.X + W / 2f, y1 = o.Y + H / 2f; if (Background) Box(dc, b, x0 + th, y0 + th, x1 - th, y1 - th, C3(a)); Box(dc, b, x0, y0, x1, y0 + th, C1(a)); Box(dc, b, x0, y1 - th, x1, y1, Col(Gfx.Mix(Color1, Color2, 0.3f), a)); Box(dc, b, x0, y0 + th, x0 + th, y1 - th, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.12f), a)); Box(dc, b, x1 - th, y0 + th, x1, y1 - th, Col(Gfx.Mix(Color1, Color2, 0.45f), a)); Frame(dc, b, x0, y0, x1, y1, C2(a), 5f * u); Frame(dc, b, x0 + th, y0 + th, x1 - th, y1 - th, C2(a), 6f * u); Frame(dc, b, x0 + th * 0.48f, y0 + th * 0.48f, x1 - th * 0.48f, y1 - th * 0.48f, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.5f), 0.65f * a), 2f * u);
        foreach (var (cx, cy) in new[] { (x0, y0), (x1, y0), (x0, y1), (x1, y1) }) { var q = new Vector2(cx, cy); Star(dc, b, q, th * 1.05f, 8, 0.62f, 0f, C1(a)); Dot(dc, b, q, th * 0.34f, C2(a)); Dot(dc, b, q, th * 0.17f, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.5f), a)); } int n = (int)(W / (th * 1.1f)); for (int i = 1; i < n; i++) { float x = x0 + i * W / n; Dot(dc, b, new(x, y0 + th / 2f), th * 0.1f, C2(0.72f * a)); Dot(dc, b, new(x, y1 - th / 2f), th * 0.1f, C2(0.72f * a)); } if (Str.Length > 0) { var pl = new Vector2(o.X, y1 + 12f * u); RBox(dc, b, pl, MathF.Max(240f * u, Measure(Str, 36f * u).Width + 70f * u), 68f * u, 6f * u, C1(a)); RFrame(dc, b, pl, MathF.Max(240f * u, Measure(Str, 36f * u).Width + 70f * u), 68f * u, 6f * u, C2(a), 3f * u); Say(dc, b, Str, 36f * u, pl, Rgb(0.15f, 0.1f, 0.03f, a)); }
    }
}

public sealed class WoodSignPostPlugin : QuickShapePlugin { public override string Name => "演出：木の看板・立て札"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WoodSignPostParameter(s); }
public sealed class WoodSignPostParameter : Q3Param
{
    public WoodSignPostParameter() : this(null) { }
    public WoodSignPostParameter(SharedDataStore? s) : base(s) { Text = "この先、危険"; Color1 = Hex(0xA87444); Color2 = Hex(0x5E3C22); Color3 = Hex(0x2B1A0F); }
    static readonly Dictionary<string, ParamGuide> g = G("木の看板", "Text=文字", "Variant=矢印の向き|1=なし　2=右　3=左|1|3", "Color1=板の色", "Color2=木目と柱の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), drop = DrawKit.EaseOut(Sub(c.Time, 0f, 0.28f)), wob = MathF.Sin(MathF.Max(0f, c.Time - 0.28f) * 18f) * 0.04f * MathF.Exp(-MathF.Max(0f, c.Time - 0.28f) * 4f), W = 760f * u, H = 280f * u; var ground = P0(c) + new Vector2(0, 360f * u); var q = ground - new Vector2(0, 520f * u + (1f - drop) * 320f * u); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(wob, ground) * prev; Box(dc, b, q.X - 28f * u, q.Y, q.X + 28f * u, ground.Y, C2(a)); float dir = Variant == 2 ? 1f : Variant == 3 ? -1f : 0f; var board = dir == 0f ? new List<Vector2> { q + new Vector2(-W / 2f, -H / 2f), q + new Vector2(W / 2f, -H / 2f), q + new Vector2(W / 2f + 6f * u, H / 2f - 4f * u), q + new Vector2(-W / 2f - 5f * u, H / 2f) } : new List<Vector2> { q + new Vector2(-dir * W / 2f, -H / 2f), q + new Vector2(dir * W * 0.32f, -H / 2f), q + new Vector2(dir * W * 0.58f, 0), q + new Vector2(dir * W * 0.32f, H / 2f), q + new Vector2(-dir * W / 2f, H / 2f) }; Poly(dc, b, board, C1(a)); for (int i = 1; i < 4; i++) Ln(dc, b, q + new Vector2(-W * 0.47f, -H / 2f + i * H / 4f), q + new Vector2(W * 0.46f, -H / 2f + i * H / 4f + MathF.Sin(i) * 5f * u), C2(0.55f * a), 3f * u); for (int i = 0; i < 12; i++) { var e = q + new Vector2((Rnd(i) - 0.5f) * W * 0.86f, (Rnd(i + 5) - 0.5f) * H * 0.8f); Oval(dc, b, e, (10f + Rnd(i + 8) * 26f) * u, 3f * u, C2(0.4f * a)); } Outline(dc, b, board, C2(a), 7f * u); foreach (int s in new[] { -1, 1 }) Dot(dc, b, q + new Vector2(s * W * 0.36f, 0), 8f * u, Rgb(0.2f, 0.2f, 0.22f, a)); Say(dc, b, Str, 78f * u, q, C3(a), maxW: W * 0.76f); dc.Transform = prev; Oval(dc, b, ground, 170f * u, 26f * u, Rgb(0.2f, 0.32f, 0.14f, a)); for (int i = 0; i < 10; i++) { float x = (i - 4.5f) * 28f * u; Poly(dc, b, new List<Vector2> { ground + new Vector2(x - 9f * u, 8f * u), ground + new Vector2(x + 9f * u, 8f * u), ground + new Vector2(x + (Rnd(i) - 0.5f) * 18f * u, -(26f + Rnd(i + 3) * 28f) * u) }, Rgb(0.28f, 0.52f, 0.2f, a)); }
    }
}

public sealed class HangingSignPlugin : QuickShapePlugin { public override string Name => "演出：吊り下げ看板"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HangingSignParameter(s); }
public sealed class HangingSignParameter : Q3Param
{
    public HangingSignParameter() : this(null) { }
    public HangingSignParameter(SharedDataStore? s) : base(s) { Text = "営業中"; Color1 = Hex(0x8A5B36); Color2 = Hex(0xB9BEC6); Color3 = Hex(0xFFF3D6); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("吊り下げ看板", "Text=文字", "Amount=揺れの大きさ", "Color1=板の色", "Color2=鎖の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, ang = MathF.Sin(t * 4.4f) * MathF.Exp(-t * 0.65f) * Amt(c) * 0.62f, W = 720f * u, H = 300f * u; var top = P0(c) - new Vector2(0, 390f * u); Box(dc, b, top.X - 500f * u, top.Y - 16f * u, top.X + 500f * u, top.Y + 16f * u, Rgb(0.28f, 0.18f, 0.11f, a)); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(ang, top) * prev; var q = top + new Vector2(0, 430f * u); foreach (int s in new[] { -1, 1 }) { var A = top + new Vector2(s * 12f * u, 8f * u); var B = q + new Vector2(s * W * 0.34f, -H / 2f); var d = B - A; float L = d.Length(); int n = (int)(L / (26f * u)); float rot = MathF.Atan2(d.Y, d.X); d /= L; for (int i = 0; i < n; i++) { var m = A + d * (i + 0.5f) * 26f * u; var pv = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(rot, m) * pv; b.Color = C2(a); dc.DrawEllipse(new Ellipse(m, 16f * u, i % 2 == 0 ? 8f * u : 3f * u), b, 3.5f * u); dc.Transform = pv; } } RBox(dc, b, q, W, H, 18f * u, C1(a)); RFrame(dc, b, q, W - 24f * u, H - 24f * u, 12f * u, Col(Gfx.Mix(Color1, Hex(0x000000), 0.48f), a), 5f * u); for (int i = 1; i < 4; i++) Ln(dc, b, q + new Vector2(-W * 0.45f, -H / 2f + i * H / 4f), q + new Vector2(W * 0.45f, -H / 2f + i * H / 4f), Col(Gfx.Mix(Color1, Hex(0x000000), 0.28f), 0.6f * a), 2f * u); Say(dc, b, Str, 96f * u, q, C3(a), Rgb(0.2f, 0.11f, 0.05f, a), 4f * u, maxW: W * 0.82f); dc.Transform = prev;
    }
}

public sealed class PressButton3DPlugin : QuickShapePlugin { public override string Name => "演出：押せる立体ボタン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PressButton3DParameter(s); }
public sealed class PressButton3DParameter : Q3Param
{
    public PressButton3DParameter() : this(null) { }
    public PressButton3DParameter(SharedDataStore? s) : base(s) { Text = "早押し"; Color1 = Hex(0xE53935); Color2 = Hex(0x2B2F36); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("立体ボタン", "Text=文字", "Amount=押す時刻|0〜100 = 0〜5秒", "Color1=ボタンの色", "Color2=台の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, at = Amt(c) * 5f, d = t - at, press = d < 0f ? 0f : d < 0.08f ? d / 0.08f : d < 0.28f ? 1f : 1f - Sub(d, 0.28f, 0.14f) * 0.72f, R = 270f * u, dep = Lerp(82f, 22f, press) * u, ry = R * 0.48f; var o = P0(c) + new Vector2(0, 100f * u); Oval(dc, b, o + new Vector2(0, 52f * u), R * 1.3f, ry * 1.3f, C2(a)); Oval(dc, b, o + new Vector2(0, 26f * u), R * 1.3f, ry * 1.3f, Col(Gfx.Mix(Color2, Hex(0xFFFFFF), 0.18f), a)); if (d > 0f && d < 0.8f) { float e = d / 0.8f; b.Color = C1((1f - e) * a); dc.DrawEllipse(new Ellipse(o, R * (1.3f + e * 0.8f), ry * (1.3f + e * 0.8f)), b, 10f * u); }
        Box(dc, b, o.X - R, o.Y - dep, o.X + R, o.Y, Col(Gfx.Mix(Color1, Hex(0x000000), 0.38f), a)); Oval(dc, b, o, R, ry, Col(Gfx.Mix(Color1, Hex(0x000000), 0.38f), a)); var top = o - new Vector2(0, dep); Oval(dc, b, top, R, ry, press > 0.5f ? Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.28f), a) : C1(a)); Oval(dc, b, top - new Vector2(R * 0.28f, ry * 0.3f), R * 0.34f, ry * 0.2f, Rgb(1, 1, 1, 0.36f * a)); if (press > 0.5f) { for (int i = 0; i < 14; i++) { float th = i / 14f * MathF.Tau; Ln(dc, b, top + new Vector2(MathF.Cos(th) * R * 1.12f, MathF.Sin(th) * ry * 1.12f), top + new Vector2(MathF.Cos(th) * R * 1.32f, MathF.Sin(th) * ry * 1.32f), Rgb(1, 0.9f, 0.45f, a), 6f * u); } } Say(dc, b, Str, 76f * u, top, C3(a), Rgb(0, 0, 0, 0.45f * a), 3f * u, maxW: R * 1.55f);
    }
}

public sealed class ToggleSliderUiPlugin : QuickShapePlugin { public override string Name => "演出：スイッチとスライダー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ToggleSliderUiParameter(s); }
public sealed class ToggleSliderUiParameter : Q3Param
{
    public ToggleSliderUiParameter() : this(null) { }
    public ToggleSliderUiParameter(SharedDataStore? s) : base(s) { Text = "通知\n音量\nダークモード\n明るさ"; Color1 = Hex(0x34C759); Color2 = Hex(0x1F2733); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount => 20;
    static readonly Dictionary<string, ParamGuide> g = G("スイッチとスライダー", "Text=項目|1行に1つ。奇数行がスイッチ、偶数行がスライダー", "Variant=種類|1=混ぜる　2=スイッチだけ　3=スライダーだけ|1|3", "Amount=オンになる時刻|0〜100 = 0〜5秒", "Color1=オンの色", "Color2=パネルの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return; float u = U(c), t = c.Time, at = Amt(c) * 5f, rh = 124f * u, H = rh * r.Count + 62f * u, W = 900f * u; var o = P0(c); Panel(dc, b, o, W, H, 30f * u, C2(0.97f * a), Rgb(1, 1, 1, 0.16f * a), 2f * u);
        for (int i = 0; i < r.Count; i++)
        {
            float y = o.Y - rh * r.Count / 2f + (i + 0.5f) * rh, k = DrawKit.EaseOut(Sub(t, at + i * 0.28f, 0.3f)); bool toggle = Variant == 2 || (Variant != 3 && i % 2 == 0); SayAt(dc, b, r[i], 44f * u, new(o.X - W / 2f + 48f * u, y - 29f * u), C3(a), maxW: 400f * u); if (i + 1 < r.Count) Ln(dc, b, new(o.X - W / 2f + 42f * u, y + rh / 2f), new(o.X + W / 2f - 42f * u, y + rh / 2f), Rgb(1, 1, 1, 0.12f * a), 2f * u);
            if (toggle) { var q = new Vector2(o.X + W / 2f - 120f * u, y); RBox(dc, b, q, 150f * u, 74f * u, 37f * u, Col(Gfx.Mix(Hex(0x5A6472), Color1, k), a)); Dot(dc, b, q + new Vector2(Lerp(-38f, 38f, k) * u, 0), 31f * u, Rgb(1, 1, 1, a)); }
            else { float x0 = o.X + 40f * u, x1 = o.X + W / 2f - 54f * u, v = Lerp(0.12f, 0.35f + Rnd(i + 3) * 0.55f, k), xv = Lerp(x0, x1, v); RBox(dc, b, new((x0 + x1) / 2f, y), x1 - x0, 13f * u, 6f * u, Rgb(0.35f, 0.39f, 0.45f, a)); RBox(dc, b, new((x0 + xv) / 2f, y), xv - x0, 13f * u, 6f * u, C1(a)); Dot(dc, b, new(xv, y), 25f * u, Rgb(1, 1, 1, a)); }
        }
    }
}
