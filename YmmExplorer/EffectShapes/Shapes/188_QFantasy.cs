using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Shape;
using WpfColor = System.Windows.Media.Color;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// =====================================================================
//  魔法の本
//   開いた本から光と文字のかけらが立ちのぼる。ページがパラパラめくれる
// =====================================================================
public sealed class MagicBookPlugin : QuickShapePlugin
{
    public override string Name => "演出：魔法の本";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MagicBookParameter(sharedData);
}

public sealed class MagicBookParameter : QuickShapeParameter
{
    public MagicBookParameter() : this(null) { }
    public MagicBookParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ᚠᚢᚦᚨᚱᚲ✦✧";
        Count = 24;
        Color1 = Hex(0x6D4C41);
        Color2 = Hex(0xFFE082);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "立ちのぼる文字", Tip: "1文字ずつ使われます", Group: "「魔法の本」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "文字の数", Min: 0, Max: 60, Group: "「魔法の本」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "本の色"),
        ["Color2"] = new ParamGuide(Label: "光の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var bk = o + new Vector2(0, 150f * u);
        Poly(dc, b, Pts(bk, u, u, -380f, -30f, 0f, 0f, 380f, -30f, 380f, 40f, 0f, 70f, -380f, 40f), C1(a));
        for (int e = -1; e <= 1; e += 2) Poly(dc, b, Pts(bk, u, u, 0f, 0f, e * 350f, -40f, e * 350f, -200f, 0f, -170f), Rgb(0.98f, 0.95f, 0.86f, a));
        float fl = Gfx.Mod(t * 1.5f * Spd(c), 1f);
        float px = MathF.Cos(fl * MathF.PI) * 350f;
        Poly(dc, b, Pts(bk, u, u, 0f, 0f, px, -40f - MathF.Sin(fl * MathF.PI) * 60f, px, -200f - MathF.Sin(fl * MathF.PI) * 60f, 0f, -170f), Rgb(1f, 0.98f, 0.9f, a));
        for (int k = 0; k < 5; k++) for (int e = -1; e <= 1; e += 2) Ln(dc, b, bk + new Vector2(e * 60f, -150f + k * 22f) * u, bk + new Vector2(e * 300f, -175f + k * 22f) * u, Rgb(0.5f, 0.45f, 0.4f, 0.4f * a), 3f * u);
        DrawKit.Glow(dc, bk + new Vector2(0, -200f * u), 420f * u, C2(0.5f * a));
        string runes = Str.Length > 0 ? Str : "✦";
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 0.4f + DrawKit.H(Seed, i), 1f);
            var q = bk + new Vector2((DrawKit.H(Seed, i, 1) - 0.5f) * 500f * u + MathF.Sin(t * 2f + i) * 40f * u, -180f * u - ph * 600f * u);
            Say(dc, b, runes[i % runes.Length].ToString(), (30f + 30f * DrawKit.H(Seed, i, 2)) * u, q, C2(a * MathF.Sin(ph * MathF.PI)), bold: false);
        }
    }
}

// =====================================================================
//  ルーン文字の輪
//   まわる輪の上にルーン文字が並び、内と外で反対に回る。「文字」で文字を変えられる
// =====================================================================
public sealed class RuneCirclePlugin : QuickShapePlugin
{
    public override string Name => "演出：ルーン文字の輪";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RuneCircleParameter(sharedData);
}

public sealed class RuneCircleParameter : QuickShapeParameter
{
    public RuneCircleParameter() : this(null) { }
    public RuneCircleParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ᚠᚢᚦᚨᚱᚲᚷᚹᚺᚾᛁᛃᛇᛈᛉᛊᛏᛒᛖᛗᛚᛜᛞᛟ";
        Color1 = Hex(0x80DEEA);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "ルーン文字", Tip: "1文字ずつ輪に並びます", Group: "「ルーン文字の輪」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "輪の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        string s = Str.Replace("\n", "");
        if (s.Length == 0) return;
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.5f));
        for (int ring = 0; ring < 2; ring++)
        {
            float R = (ring == 0 ? 360f : 250f) * u * k;
            float rot = t * 0.4f * Spd(c) * (ring == 0 ? 1f : -1.4f);
            Ring(dc, b, o, R + 34f * u, C1(0.8f * a), 3f * u);
            Ring(dc, b, o, R - 34f * u, C1(0.8f * a), 3f * u);
            int n = ring == 0 ? 24 : 16;
            for (int i = 0; i < n; i++)
            {
                float ang = rot + i * MathF.PI * 2f / n;
                var q = Pol(o, R, ang);
                var prev = dc.Transform;
                dc.Transform = Matrix3x2.CreateRotation(ang + MathF.PI / 2f, q) * prev;
                Say(dc, b, s[(i + ring * 7) % s.Length].ToString(), 44f * u, q, C1(a), bold: false);
                dc.Transform = prev;
            }
        }
        DrawKit.Glow(dc, o, 300f * u * k, C1(0.25f * a));
        Star(dc, b, o, 140f * u * k, 6, 0.55f, -t * 0.3f, C1(0.25f * a));
    }
}

// =====================================================================
//  ドラゴンの影が横切る
//   空を大きなドラゴンの影が、はばたきながら横切る。地面に影も
// =====================================================================
public sealed class DragonShadowPlugin : QuickShapePlugin
{
    public override string Name => "演出：ドラゴンの影が横切る";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DragonShadowParameter(sharedData);
}

public sealed class DragonShadowParameter : QuickShapeParameter
{
    public DragonShadowParameter() : this(null) { }
    public DragonShadowParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x111111);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "影の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
        ["PosX"] = new ParamGuide(Hidden: true),
        ["PosY"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        float p = Gfx.Mod(c.Time * 0.25f * Spd(c), 1.3f) - 0.15f;
        var q = new Vector2(-400f * s + (w + 800f * s) * p, h * 0.35f + MathF.Sin(p * 6f) * 40f * s);
        float flap = MathF.Sin(t * 5f);
        var body = DrawKit.Place(Pts(Vector2.Zero, s, s, -260f, 0f, -120f, -30f, 60f, -36f, 140f, -60f, 200f, -50f, 170f, -20f, 80f, 10f, -120f, 30f, -380f, 60f, -260f, 0f), q, 0f);
        Poly(dc, b, body, C1(0.85f * a));
        for (int e = -1; e <= 1; e += 2)
        {
            float wy = e * (160f + 120f * flap) * s;
            var wing = new List<Vector2> { q + new Vector2(-40f * s, 0), q + new Vector2(30f * s, wy * 0.6f - 20f * s), q + new Vector2(-20f * s, wy), q + new Vector2(-100f * s, wy * 0.8f), q + new Vector2(-180f * s, wy * 0.9f), q + new Vector2(-160f * s, wy * 0.5f), q + new Vector2(-120f * s, 0) };
            Poly(dc, b, wing, C1((e < 0 ? 0.85f : 0.6f) * a));
        }
        Oval(dc, b, new Vector2(q.X + 100f * s, h * 0.92f), 300f * s, 30f * s, C1(0.25f * a));
    }
}

// =====================================================================
//  妖精の光の軌跡
//   小さな光の玉が8の字に飛び、キラキラの粉の跡を残す
// =====================================================================
public sealed class FairyTrailPlugin : QuickShapePlugin
{
    public override string Name => "演出：妖精の光の軌跡";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FairyTrailParameter(sharedData);
}

public sealed class FairyTrailParameter : QuickShapeParameter
{
    public FairyTrailParameter() : this(null) { }
    public FairyTrailParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 40;
        Color1 = Hex(0xB2FF59);
        Color2 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "光の粉の数", Min: 5, Max: 120, Group: "「妖精の光の軌跡」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "粉の色"),
        ["Color2"] = new ParamGuide(Label: "光の玉の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float sp = 1.2f * Spd(c);
        Vector2 Pos(float x) => o + new Vector2(MathF.Sin(x) * 400f, MathF.Sin(x * 2f) * 160f) * u;
        for (int i = Count; i >= 0; i--)
        {
            float tt = t * sp - i * 0.04f;
            var q = Pos(tt) + new Vector2(DrawKit.H(Seed, i) - 0.5f, DrawKit.H(Seed, i, 1) - 0.5f) * 30f * u * (i / (float)Count) + new Vector2(0, i * 1.5f * u);
            float f = 1f - i / (float)Math.Max(1, Count);
            Dot(dc, b, q, (2f + 5f * f) * u, C1(f * a));
        }
        var hd = Pos(t * sp);
        DrawKit.Glow(dc, hd, 90f * u, C1(0.7f * a));
        Dot(dc, b, hd, 12f * u, C2(a));
        DrawKit.Twinkle(dc, b, hd, 50f * u * (0.7f + 0.3f * MathF.Sin(t * 10f)), 3f * u, Color2, a);
    }
}

// =====================================================================
//  歯車の時空ワープ
//   大小の歯車が回る中を、時計の針がぐるぐる。過去や未来へ飛ぶ場面に
// =====================================================================
public sealed class GearWarpPlugin : QuickShapePlugin
{
    public override string Name => "演出：歯車の時空ワープ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GearWarpParameter(sharedData);
}

public sealed class GearWarpParameter : QuickShapeParameter
{
    public GearWarpParameter() : this(null) { }
    public GearWarpParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 6;
        Color1 = Hex(0xC9A227);
        Color2 = Hex(0x1A1033);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "歯車の数", Min: 0, Max: 20, Group: "「歯車の時空ワープ」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "背景", Group: "「歯車の時空ワープ」の設定"),
        ["Color1"] = new ParamGuide(Label: "歯車と時計の色"),
        ["Color2"] = new ParamGuide(Label: "背景の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["PosX"] = new ParamGuide(Hidden: true),
        ["PosY"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        if (Background) { Box(dc, b, 0, 0, w, h, C2(a)); DrawKit.Glow(dc, new Vector2(w, h) / 2f, MathF.Max(w, h) * 0.6f, Rgb(0.5f, 0.3f, 0.9f, 0.35f * a)); }
        for (int i = 0; i < Count; i++)
        {
            var q = new Vector2(w * DrawKit.H(Seed, i), h * DrawKit.H(Seed, i, 1));
            float R = (80f + 200f * DrawKit.H(Seed, i, 2)) * s;
            int teeth = 8 + (int)(R / s / 20f);
            var g = new List<Vector2>();
            float rot = t * (i % 2 == 0 ? 1f : -1f) * 60f / (R / s) * Spd(c);
            for (int k = 0; k < teeth * 4; k++) { float an = rot + k * MathF.PI * 2f / (teeth * 4); g.Add(Pol(q, (k % 4 < 2) ? R : R * 0.85f, an)); }
            Poly(dc, b, g, C1(0.5f * a));
            Dot(dc, b, q, R * 0.5f, C2(0.8f * a));
            Ring(dc, b, q, R * 0.5f, C1(0.6f * a), 4f * s);
        }
        var cc = new Vector2(w, h) / 2f;
        Ring(dc, b, cc, 260f * s, C1(a), 8f * s);
        for (int i = 0; i < 12; i++) Ln(dc, b, Pol(cc, 220f * s, i * MathF.PI / 6f), Pol(cc, 250f * s, i * MathF.PI / 6f), C1(a), 6f * s);
        Ln(dc, b, cc, Pol(cc, 200f * s, t * 6f * Spd(c) - MathF.PI / 2f), C1(a), 8f * s);
        Ln(dc, b, cc, Pol(cc, 140f * s, t * 0.6f * Spd(c) - MathF.PI / 2f), C1(a), 12f * s);
    }
}

// =====================================================================
//  宇宙ステーション・人工衛星
//   太陽電池パネルを広げた人工衛星が、地球の前をゆっくり回る
// =====================================================================
public sealed class SpaceStationPlugin : QuickShapePlugin
{
    public override string Name => "演出：宇宙ステーション・人工衛星";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpaceStationParameter(sharedData);
}

public sealed class SpaceStationParameter : QuickShapeParameter
{
    public SpaceStationParameter() : this(null) { }
    public SpaceStationParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x1A3A8A);
        Color2 = Hex(0xCFD8DC);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "宇宙と地球の背景", Group: "「宇宙ステーション・人工衛星」の設定"),
        ["Color1"] = new ParamGuide(Label: "パネルの色"),
        ["Color2"] = new ParamGuide(Label: "本体の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        if (Background) { Box(dc, b, 0, 0, w, h, Rgb(0.01f, 0.01f, 0.05f, a)); for (int i = 0; i < 80; i++) Dot(dc, b, new Vector2(DrawKit.H(Seed, i) * w, DrawKit.H(Seed, i, 1) * h), 1.5f * s, Rgb(1, 1, 1, 0.7f * a)); }
        var earth = new Vector2(w * 0.5f, h * 1.35f);
        DrawKit.Glow(dc, earth, h * 0.95f, Rgb(0.3f, 0.6f, 1f, 0.4f * a));
        Dot(dc, b, earth, h * 0.8f, Rgb(0.12f, 0.35f, 0.7f, a));
        for (int i = 0; i < 6; i++) Poly(dc, b, BubbleKit.CloudPts(earth + new Vector2((DrawKit.H(Seed, i, 4) - 0.5f) * h * 1.2f + t * 5f * s, -h * 0.7f + DrawKit.H(Seed, i, 5) * 100f * s), 120f * s, 30f * s, 6, 0.2f), Rgb(1, 1, 1, 0.5f * a));
        float rot = MathF.Sin(t * 0.2f) * 0.2f;
        var q = o + new Vector2(MathF.Sin(t * 0.15f) * 100f, MathF.Cos(t * 0.2f) * 30f) * u;
        var prev = dc.Transform;
        dc.Transform = Matrix3x2.CreateRotation(rot, q) * prev;
        Box(dc, b, q.X - 380f * u, q.Y - 6f * u, q.X + 380f * u, q.Y + 6f * u, C2(a));
        for (int e = -1; e <= 1; e += 2)
            for (int k = 0; k < 2; k++)
            {
                float x0 = q.X + e * (120f + k * 140f) * u, x1 = x0 + e * 120f * u;
                Box(dc, b, MathF.Min(x0, x1), q.Y - 90f * u, MathF.Max(x0, x1), q.Y + 90f * u, C1(a));
                for (int j = 1; j < 4; j++) Ln(dc, b, new Vector2(MathF.Min(x0, x1), q.Y - 90f * u + j * 45f * u), new Vector2(MathF.Max(x0, x1), q.Y - 90f * u + j * 45f * u), Rgb(0.6f, 0.7f, 1f, 0.5f * a), 2f * u);
            }
        RBox(dc, b, q, 150f * u, 110f * u, 20f * u, C2(a));
        RBox(dc, b, q + new Vector2(0, 80f * u), 70f * u, 60f * u, 10f * u, Rgb(0.85f, 0.7f, 0.3f, a));
        Dot(dc, b, q + new Vector2(0, -90f * u), 30f * u, Rgb(0.9f, 0.9f, 0.9f, a));
        if ((int)(t * 2f) % 2 == 0) Dot(dc, b, q + new Vector2(70f, -50f) * u, 8f * u, Rgb(1f, 0.2f, 0.2f, a));
        dc.Transform = prev;
    }
}

// =====================================================================
//  隕石が落ちてくる
//   燃える隕石が斜めに落ちてきて、地面でドカンと光る
// =====================================================================
public sealed class MeteorStrikePlugin : QuickShapePlugin
{
    public override string Name => "演出：隕石が落ちてくる";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MeteorStrikeParameter(sharedData);
}

public sealed class MeteorStrikeParameter : QuickShapeParameter
{
    public MeteorStrikeParameter() : this(null) { }
    public MeteorStrikeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF6D00);
        Color2 = Hex(0xFFEA00);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "衝突の時に画面を光らせる", Group: "「隕石が落ちてくる」の設定"),
        ["Color1"] = new ParamGuide(Label: "炎の色"),
        ["Color2"] = new ParamGuide(Label: "光の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        float p = Intro(c, 0f, 1.6f / MathF.Max(0.1f, Spd(c)));
        var st = new Vector2(w * 1.1f, -h * 0.2f);
        var en = o;
        var q = Vector2.Lerp(st, en, p);
        var dir = Vector2.Normalize(en - st);
        if (p < 1f)
        {
            for (int i = 0; i < 30; i++)
            {
                var tp = q - dir * i * 18f * s + new Vector2(MathF.Sin(t * 30f + i) * 4f, MathF.Cos(t * 27f + i) * 4f) * s;
                Dot(dc, b, tp, (60f - i * 1.8f) * s * u / U0(c), Col(Gfx.Mix(Color2, Color1, i / 30f), (1f - i / 30f) * 0.8f * a));
            }
            Dot(dc, b, q, 40f * u, Rgb(0.35f, 0.2f, 0.15f, a));
            DrawKit.Glow(dc, q, 160f * u, C2(0.5f * a));
        }
        else
        {
            float f = Math.Clamp((c.Time - 1.6f / MathF.Max(0.1f, Spd(c))) / 1.2f, 0f, 1f);
            if (Background) Box(dc, b, 0, 0, w, h, Rgb(1, 1, 0.9f, (1f - f) * 0.8f * a));
            DrawKit.Glow(dc, en, (200f + 800f * DrawKit.EaseOut(f)) * u, C1((1f - f) * a));
            Ring(dc, b, en, (100f + 900f * DrawKit.EaseOut(f)) * u, C2((1f - f) * a), 20f * u * (1f - f) + 1f);
            for (int i = 0; i < 16; i++) Dot(dc, b, Pol(en, 600f * u * DrawKit.EaseOut(f) * (0.5f + DrawKit.H(Seed, i)), -MathF.PI * DrawKit.H(Seed, i, 1)) + new Vector2(0, f * f * 300f * u), 14f * u, Rgb(0.4f, 0.25f, 0.15f, (1f - f) * a));
        }
    }
}

// =====================================================================
//  環のある惑星
//   しまもようの惑星と、ななめの輪。輪の奥側は惑星にかくれる
// =====================================================================
public sealed class RingedPlanetPlugin : QuickShapePlugin
{
    public override string Name => "演出：環のある惑星";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RingedPlanetParameter(sharedData);
}

public sealed class RingedPlanetParameter : QuickShapeParameter
{
    public RingedPlanetParameter() : this(null) { }
    public RingedPlanetParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xE0B07A);
        Color2 = Hex(0xC9A0DC);
    }
    protected override double DefAmount => 30;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "輪の開き具合", Tip: "0=真横から 100=上から", Group: "「環のある惑星」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "惑星の色"),
        ["Color2"] = new ParamGuide(Label: "輪の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = 260f * u, tilt = -0.25f;
        float rx = R * 2.1f, ry = R * (0.15f + 0.6f * Amt(c));
        var back = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, rx, ry, 64, MathF.PI, MathF.PI * 2f), o, tilt);
        var backIn = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, rx * 0.72f, ry * 0.72f, 64, MathF.PI, MathF.PI * 2f), o, tilt);
        backIn.Reverse(); var bandB = new List<Vector2>(back); bandB.AddRange(backIn);
        Poly(dc, b, bandB, C2(0.75f * a));
        Dot(dc, b, o, R, C1(a));
        for (int i = -3; i <= 3; i++)
        {
            float y = i * R * 0.25f + MathF.Sin(t * 0.3f + i) * 4f * u;
            float hw = MathF.Sqrt(MathF.Max(0f, R * R - y * y));
            var band = DrawKit.Place(new List<Vector2> { new(-hw, y - R * 0.05f), new(hw, y - R * 0.05f), new(hw, y + R * 0.05f), new(-hw, y + R * 0.05f) }, o, tilt);
            Poly(dc, b, band, Col(Gfx.Mix(Color1, Hex(0x8D5A2B), 0.3f + 0.2f * (i % 2)), 0.6f * a));
        }
        var front = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, rx, ry, 64, 0f, MathF.PI), o, tilt);
        var frontIn = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, rx * 0.72f, ry * 0.72f, 64, 0f, MathF.PI), o, tilt);
        frontIn.Reverse(); var bandF = new List<Vector2>(front); bandF.AddRange(frontIn);
        Poly(dc, b, bandF, C2(0.9f * a));
    }
}

// =====================================================================
//  ロボットの起動画面
//   「SYSTEM BOOT」の文字と、チェック項目が次々OKになり、最後に目が光る
// =====================================================================
public sealed class RobotBootPlugin : QuickShapePlugin
{
    public override string Name => "演出：ロボットの起動画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RobotBootParameter(sharedData);
}

public sealed class RobotBootParameter : QuickShapeParameter
{
    public RobotBootParameter() : this(null) { }
    public RobotBootParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "MOTOR\nSENSOR\nPOWER\nAI CORE\nWEAPON";
        Color1 = Hex(0x00E5FF);
        Color2 = Hex(0xFF1744);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "チェック項目", Tip: "1行に1つ", Group: "「ロボットの起動画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "暗い背景", Group: "「ロボットの起動画面」の設定"),
        ["Color1"] = new ParamGuide(Label: "文字の色"),
        ["Color2"] = new ParamGuide(Label: "目の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
        ["PosX"] = new ParamGuide(Hidden: true),
        ["PosY"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var items = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        float s = U0(c);
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(0.01f, 0.03f, 0.05f, a));
        SayAt(dc, b, "SYSTEM BOOT v2.0", 44f * s, new Vector2(100f * s, 80f * s), C1(a));
        float y = 170f * s;
        for (int i = 0; i < items.Count; i++)
        {
            float p = Intro(c, 0.4f + i * 0.35f, 0.3f);
            if (p <= 0f) break;
            string dots = new string('.', (int)(p * 12));
            SayAt(dc, b, "> " + items[i] + " " + dots, 36f * s, new Vector2(100f * s, y), C1(0.85f * a), false);
            if (p >= 1f) SayAt(dc, b, "[ OK ]", 36f * s, new Vector2(700f * s, y), Rgb(0.4f, 1f, 0.5f, a), false);
            y += 60f * s;
        }
        float done = Intro(c, 0.6f + items.Count * 0.35f, 0.4f);
        if (done > 0f)
        {
            var ec = new Vector2(w * 0.7f, h * 0.55f);
            for (int e = -1; e <= 1; e += 2)
            {
                var q = ec + new Vector2(e * 160f * s, 0);
                DrawKit.Glow(dc, q, 180f * s * done, C2(0.6f * a));
                Oval(dc, b, q, 90f * s, 40f * s * done, C2(a));
                Oval(dc, b, q, 40f * s, 18f * s * done, Rgb(1, 0.9f, 0.9f, a));
            }
            if ((int)(t * 3f) % 2 == 0) Say(dc, b, "ONLINE", 60f * s, new Vector2(w * 0.7f, h * 0.75f), C2(a));
        }
    }
}

// =====================================================================
//  ワームホール
//   吸いこまれそうな、ねじれたトンネルの穴。光の筋が中心へ向かって回る
// =====================================================================
public sealed class WormholePlugin : QuickShapePlugin
{
    public override string Name => "演出：ワームホール";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WormholeParameter(sharedData);
}

public sealed class WormholeParameter : QuickShapeParameter
{
    public WormholeParameter() : this(null) { }
    public WormholeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 60;
        Color1 = Hex(0x7C4DFF);
        Color2 = Hex(0x18FFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "光の筋の数", Min: 0, Max: 200, Group: "「ワームホール」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "外側の色"),
        ["Color2"] = new ParamGuide(Label: "内側の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = 600f * u;
        for (int i = 0; i < 18; i++)
        {
            float f = Gfx.Mod(i / 18f + t * 0.3f * Spd(c), 1f);
            float rr = R * MathF.Pow(f, 2.2f);
            Ring(dc, b, o, rr, Col(Gfx.Mix(Color2, Color1, f), (1f - f * 0.6f) * f * 0.8f * a), (2f + 10f * f) * u);
        }
        for (int i = 0; i < Count; i++)
        {
            float f = Gfx.Mod(DrawKit.H(Seed, i) - t * 0.4f * Spd(c), 1f);
            float ang = DrawKit.H(Seed, i, 1) * MathF.PI * 2f + f * 4f;
            var p0 = Pol(o, R * f * f, ang);
            var p1 = Pol(o, R * (f + 0.04f) * (f + 0.04f), ang + 0.16f);
            Ln(dc, b, p0, p1, C2(f * a), (1f + 3f * f) * u);
        }
        Dot(dc, b, o, 40f * u, Rgb(0, 0, 0, a));
        DrawKit.Glow(dc, o, 120f * u, Rgb(1, 1, 1, 0.5f * a));
    }
}

// =====================================================================
//  原子モデル
//   まん中の原子核のまわりを、3本の軌道で電子が回る。「数」で電子の数
// =====================================================================
public sealed class AtomModelPlugin : QuickShapePlugin
{
    public override string Name => "演出：原子モデル";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AtomModelParameter(sharedData);
}

public sealed class AtomModelParameter : QuickShapeParameter
{
    public AtomModelParameter() : this(null) { }
    public AtomModelParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 3;
        Color1 = Hex(0x4FC3F7);
        Color2 = Hex(0xFF7043);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "電子(軌道)の数", Min: 1, Max: 8, Group: "「原子モデル」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "電子の色"),
        ["Color2"] = new ParamGuide(Label: "陽子の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = 340f * u;
        int n = Math.Clamp(Count, 1, 8);
        for (int i = 0; i < n; i++)
        {
            float tilt = i * MathF.PI / n;
            Outline(dc, b, DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, R, R * 0.32f, 64), o, tilt), C1(0.6f * a), 4f * u);
            float ang = t * 2f * Spd(c) * (1f + i * 0.13f) + i;
            var e = DrawKit.Rot(o + new Vector2(MathF.Cos(ang) * R, MathF.Sin(ang) * R * 0.32f), o, tilt);
            DrawKit.Glow(dc, e, 50f * u, C1(0.6f * a));
            Dot(dc, b, e, 18f * u, C1(a));
        }
        for (int i = 0; i < 7; i++) Dot(dc, b, o + new Vector2(MathF.Cos(i * 2.4f), MathF.Sin(i * 2.4f)) * (i == 0 ? 0f : 34f * u), 34f * u, i % 2 == 0 ? C2(a) : Rgb(0.6f, 0.6f, 0.65f, a));
    }
}

// =====================================================================
//  異世界の光の門
//   石の門の中が光の渦になり、強い光があふれ出す
// =====================================================================
public sealed class IsekaiGatePlugin : QuickShapePlugin
{
    public override string Name => "演出：異世界の光の門";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new IsekaiGateParameter(sharedData);
}

public sealed class IsekaiGateParameter : QuickShapeParameter
{
    public IsekaiGateParameter() : this(null) { }
    public IsekaiGateParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x9E9E9E);
        Color2 = Hex(0x80D8FF);
        Color3 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "門の色"),
        ["Color2"] = new ParamGuide(Label: "渦の色"),
        ["Color3"] = new ParamGuide(Label: "光の色"),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float gw = 520f * u, gh = 780f * u;
        var arch = new List<Vector2> { new(o.X - gw / 2f, o.Y + gh / 2f) };
        arch.AddRange(Gfx.ArcPts(new Vector2(o.X, o.Y - gh / 2f + gw / 2f), gw / 2f, MathF.PI, MathF.PI * 2f, 30));
        arch.Add(new Vector2(o.X + gw / 2f, o.Y + gh / 2f));
        float open = DrawKit.EaseOut(Intro(c, 0.2f, 1.2f));
        Poly(dc, b, DrawKit.Place(arch.Select(p => (p - o) * 1.18f), o, 0f), C1(a));
        Poly(dc, b, arch, Rgb(0.05f, 0.05f, 0.1f, a));
        Poly(dc, b, DrawKit.Place(arch.Select(p => (p - o) * open), o, 0f), C2(0.8f * a));
        for (int i = 0; i < 10; i++)
        {
            var sp = new List<Vector2>();
            for (int k = 0; k < 30; k++) { float f = k / 30f; sp.Add(Pol(o, gw * 0.45f * f * open, f * 6f - t * 2f * Spd(c) + i * MathF.PI / 5f)); }
            Path(dc, b, sp, C3(0.5f * a * open), 3f * u);
        }
        DrawKit.Glow(dc, o, 700f * u * open, C3(0.45f * a * open));
        for (int i = 0; i < 12; i++)
        {
            float ang = i * MathF.PI / 6f + t * 0.1f;
            Poly(dc, b, new List<Vector2> { Pol(o, 100f * u, ang - 0.04f), Pol(o, 1400f * u * open, ang), Pol(o, 100f * u, ang + 0.04f) }, C3(0.15f * a * open));
        }
    }
}

// =====================================================================
//  ステルス迷彩のゆらぎ
//   透明なものがそこにいるような、ゆらゆらした輪郭と光のゆがみ
// =====================================================================
public sealed class StealthShimmerPlugin : QuickShapePlugin
{
    public override string Name => "演出：ステルス迷彩のゆらぎ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StealthShimmerParameter(sharedData);
}

public sealed class StealthShimmerParameter : QuickShapeParameter
{
    public StealthShimmerParameter() : this(null) { }
    public StealthShimmerParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xB3E5FC);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ゆらぎの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float hw = 160f * u, hh = 400f * u;
        var outline = new List<Vector2>();
        for (int i = 0; i < 64; i++)
        {
            float ang = i * MathF.PI * 2f / 64f;
            float k = 1f + 0.06f * MathF.Sin(ang * 5f + t * 6f) + 0.04f * MathF.Sin(ang * 9f - t * 9f);
            float rx = hw * (MathF.Sin(ang) < -0.7f ? 0.55f : 1f);
            outline.Add(o + new Vector2(MathF.Cos(ang) * rx * k, MathF.Sin(ang) * hh * k));
        }
        Poly(dc, b, outline, C1(0.06f * a));
        Outline(dc, b, outline, C1(0.5f * a * (0.7f + 0.3f * MathF.Sin(t * 4f))), 3f * u);
        for (int i = 0; i < 8; i++)
        {
            float y = o.Y - hh + Gfx.Mod(t * 200f * u + i * hh / 4f, hh * 2f);
            float half = hw * MathF.Sqrt(MathF.Max(0f, 1f - MathF.Pow((y - o.Y) / hh, 2f)));
            Ln(dc, b, new Vector2(o.X - half, y), new Vector2(o.X + half, y + 4f * u), C1(0.25f * a), 2f * u);
        }
    }
}

// =====================================================================
//  星図・航路図
//   星と星を線でつないだ星図。点線の航路が伸びて、目的地の星が光る。「数」で星の数
// =====================================================================
public sealed class StarChartPlugin : QuickShapePlugin
{
    public override string Name => "演出：星図・航路図";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StarChartParameter(sharedData);
}

public sealed class StarChartParameter : QuickShapeParameter
{
    public StarChartParameter() : this(null) { }
    public StarChartParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 9;
        Text = "目的地";
        Color1 = Hex(0x90CAF9);
        Color2 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "目的地の名前", Group: "「星図・航路図」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "星の数", Min: 2, Max: 20, Group: "「星図・航路図」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "暗い背景", Group: "「星図・航路図」の設定"),
        ["Color1"] = new ParamGuide(Label: "星の色"),
        ["Color2"] = new ParamGuide(Label: "航路の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["PosX"] = new ParamGuide(Hidden: true),
        ["PosY"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(0.02f, 0.04f, 0.1f, a));
        for (int i = 0; i < 10; i++) { Ring(dc, b, new Vector2(w, h) / 2f, (100f + i * 120f) * s, C1(0.06f * a), 1.5f * s); }
        var pts = new List<Vector2>();
        for (int i = 0; i < Count; i++) pts.Add(new Vector2(w * (0.1f + 0.8f * i / MathF.Max(1, Count - 1)), h * (0.3f + 0.4f * DrawKit.H(Seed, i))));
        for (int i = 0; i < pts.Count - 1; i++) Ln(dc, b, pts[i], pts[i + 1], C1(0.3f * a), 2f * s);
        for (int i = 0; i < pts.Count; i++) Star(dc, b, pts[i], (8f + 8f * DrawKit.H(Seed, i, 1)) * s, 4, 0.4f, 0f, C1(a));
        float p = DrawKit.EaseOut(Intro(c, 0.3f, 2f));
        var route = BubbleKit.PartByLength(pts, p);
        for (int i = 0; i < route.Count - 1; i++)
        {
            float segLen = Vector2.Distance(route[i], route[i + 1]);
            int dashes = (int)(segLen / (24f * s));
            for (int d = 0; d < dashes; d += 2) Ln(dc, b, Vector2.Lerp(route[i], route[i + 1], d / (float)dashes), Vector2.Lerp(route[i], route[i + 1], (d + 1) / (float)dashes), C2(a), 4f * s);
        }
        var last = pts[^1];
        if (p >= 1f) { DrawKit.Glow(dc, last, 120f * s, C2(0.6f * a * (0.7f + 0.3f * MathF.Sin(t * 4f)))); Ring(dc, b, last, 40f * s, C2(a), 3f * s); Say(dc, b, Str, 36f * s, last + new Vector2(0, -70f * s), C2(a)); }
        Poly(dc, b, Pts(route[^1], 18f * s, 18f * s, 0f, -1f, 0.6f, 0.8f, 0f, 0.4f, -0.6f, 0.8f), Rgb(1, 1, 1, a));
    }
}
