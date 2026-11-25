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
//  ハートが割れる
//   ハートがドクンと動いたあと、ギザギザに割れて左右に落ちる
// =====================================================================
public sealed class HeartBreakPlugin : QuickShapePlugin
{
    public override string Name => "演出：ハートが割れる";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HeartBreakParameter(sharedData);
}

public sealed class HeartBreakParameter : QuickShapeParameter
{
    public HeartBreakParameter() : this(null) { }
    public HeartBreakParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF4D6D);
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
        ["Color1"] = new ParamGuide(Label: "ハートの色"),
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
        float r = 160f * u;
        float beat = 1f + 0.08f * MathF.Max(0f, MathF.Sin(c.Time * 12f)) * (c.Time < 0.6f ? 1f : 0f);
        float br = Intro(c, 0.7f, 0.9f);
        if (br <= 0f) { Heart(dc, b, o, r * beat, 0f, C1(a)); return; }
        var heart = ShapeGeom.HeartPoly(o, r, 0f);
        float sep = DrawKit.EaseOut(br) * 80f * u, fall = br * br * 400f * u;
        for (int side = -1; side <= 1; side += 2)
        {
            var half = DrawKit.ClipRect(heart, side < 0 ? o.X - 1e4f : o.X, -1e5f, side < 0 ? o.X : o.X + 1e4f, 1e5f);
            if (half.Count < 3) continue;
            var moved = DrawKit.Place(half.Select(v => v - o), o + new Vector2(side * sep, fall), side * br * 0.5f);
            Poly(dc, b, moved, C1(a * (1f - br * 0.3f)));
            var zig = new List<Vector2>();
            for (int i = 0; i <= 6; i++) zig.Add(new Vector2((i % 2 == 0 ? -1f : 1f) * 14f * u, -r * 0.9f + r * 1.9f * i / 6f));
            Path(dc, b, DrawKit.Place(zig, o + new Vector2(side * sep, fall), side * br * 0.5f), Rgb(0.4f, 0f, 0.1f, 0.8f * a), 6f * u);
        }
    }
}

// =====================================================================
//  メラメラ怒りのオーラ
//   キャラの後ろで赤い炎がメラメラ。「値」で炎の高さ
// =====================================================================
public sealed class AngerFlamePlugin : QuickShapePlugin
{
    public override string Name => "演出：メラメラ怒りのオーラ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AngerFlameParameter(sharedData);
}

public sealed class AngerFlameParameter : QuickShapeParameter
{
    public AngerFlameParameter() : this(null) { }
    public AngerFlameParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF1744);
        Color2 = Hex(0xFF9100);
    }
    protected override double DefAmount => 60;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "炎の高さ", Group: "「メラメラ怒りのオーラ」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "外側の色"),
        ["Color2"] = new ParamGuide(Label: "内側の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float hh = (200f + 500f * Amt(c)) * u, ww = 500f * u;
        for (int layer = 0; layer < 3; layer++)
        {
            var pts = new List<Vector2>();
            int n = 14;
            float lw = ww * (1f - layer * 0.22f), lh = hh * (1f - layer * 0.25f);
            for (int i = 0; i <= n; i++)
            {
                float f = i / (float)n, x = o.X - lw / 2f + lw * f;
                float tip = (i % 2 == 1 ? 1f : 0.55f) * (0.4f + 0.6f * MathF.Sin(f * MathF.PI));
                float flick = 0.8f + 0.2f * MathF.Sin(t * 9f * Spd(c) + i * 1.7f + layer);
                pts.Add(new Vector2(x + MathF.Sin(t * 5f + i) * 10f * u, o.Y - lh * tip * flick));
            }
            pts.Add(new Vector2(o.X + lw / 2f, o.Y + 40f * u));
            pts.Add(new Vector2(o.X - lw / 2f, o.Y + 40f * u));
            Poly(dc, b, pts, Col(Gfx.Mix(Color1, Color2, layer / 2f), (0.5f + layer * 0.2f) * a));
        }
    }
}

// =====================================================================
//  星の目（キラキラ目）
//   目の位置に2つの星がキラキラ回る。「値」で目の間隔
// =====================================================================
public sealed class StarEyesPlugin : QuickShapePlugin
{
    public override string Name => "演出：星の目（キラキラ目）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StarEyesParameter(sharedData);
}

public sealed class StarEyesParameter : QuickShapeParameter
{
    public StarEyesParameter() : this(null) { }
    public StarEyesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFD54A);
    }
    protected override double DefAmount => 40;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "目の間隔", Group: "「星の目（キラキラ目）」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "星の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float gap = (80f + 300f * Amt(c)) * u;
        float pulse = 1f + 0.15f * MathF.Sin(t * 8f * Spd(c));
        for (int i = -1; i <= 1; i += 2)
        {
            var q = o + new Vector2(i * gap / 2f, 0);
            DrawKit.Glow(dc, q, 120f * u, C1(0.5f * a));
            Star(dc, b, q, 70f * u * pulse * DrawKit.EaseOutBack(Intro(c, 0f, 0.3f)), 5, 0.45f, t * 1.5f * i, C1(a));
            Star(dc, b, q, 30f * u * pulse, 5, 0.45f, t * 1.5f * i, Rgb(1, 1, 1, 0.8f * a));
            DrawKit.Twinkle(dc, b, q + new Vector2(60f, -60f) * u, 40f * u * (0.5f + 0.5f * MathF.Sin(t * 6f + i)), 4f * u, System.Windows.Media.Colors.White, a);
        }
    }
}

// =====================================================================
//  ぐるぐる目
//   目がぐるぐるのうずまき。目を回した時に。「値」で目の間隔
// =====================================================================
public sealed class SpiralEyesPlugin : QuickShapePlugin
{
    public override string Name => "演出：ぐるぐる目";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpiralEyesParameter(sharedData);
}

public sealed class SpiralEyesParameter : QuickShapeParameter
{
    public SpiralEyesParameter() : this(null) { }
    public SpiralEyesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x222222);
    }
    protected override double DefAmount => 40;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "目の間隔", Group: "「ぐるぐる目」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "うずの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float gap = (80f + 300f * Amt(c)) * u, r = 70f * u;
        for (int e = -1; e <= 1; e += 2)
        {
            var q = o + new Vector2(e * gap / 2f, 0);
            Dot(dc, b, q, r * 1.05f, Rgb(1, 1, 1, a));
            var sp = new List<Vector2>();
            for (int i = 0; i <= 80; i++)
            {
                float f = i / 80f, ang = f * MathF.PI * 6f + t * 8f * Spd(c) * e;
                sp.Add(Pol(q, r * f, ang));
            }
            Path(dc, b, sp, C1(a), 6f * u);
        }
    }
}

// =====================================================================
//  汗が飛び散る
//   しずく形の汗が、まわりに勢いよく飛び散る。「数」で汗の数。くり返す
// =====================================================================
public sealed class SweatSplashPlugin : QuickShapePlugin
{
    public override string Name => "演出：汗が飛び散る";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SweatSplashParameter(sharedData);
}

public sealed class SweatSplashParameter : QuickShapeParameter
{
    public SweatSplashParameter() : this(null) { }
    public SweatSplashParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 6;
        Color1 = Hex(0x6EC6FF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "汗の数", Min: 1, Max: 16, Group: "「汗が飛び散る」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "汗の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 1.4f * Spd(c) + DrawKit.H(Seed, i) * 0.3f, 1f);
            float ang = -MathF.PI / 2f + (i - (Count - 1) / 2f) * 0.45f;
            var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
            var q = o + dir * (120f + 180f * DrawKit.EaseOut(ph)) * u + new Vector2(0, ph * ph * 120f * u);
            float s = 34f * u * (1f - ph * 0.4f);
            var drop = DrawKit.Place(Pts(Vector2.Zero, s, s, 0f, -1.4f, 0.7f, 0f, 0.6f, 0.6f, 0f, 0.95f, -0.6f, 0.6f, -0.7f, 0f), q, ang + MathF.PI / 2f);
            Poly(dc, b, drop, C1(a * (1f - ph)));
            Dot(dc, b, q + new Vector2(-s * 0.2f, -s * 0.1f), s * 0.18f, Rgb(1, 1, 1, 0.8f * a * (1f - ph)));
        }
    }
}

// =====================================================================
//  青ざめ（縦線）
//   顔の上に青い縦線がスーッと下りる。ガーン…という時に
// =====================================================================
public sealed class PaleLinesPlugin : QuickShapePlugin
{
    public override string Name => "演出：青ざめ（縦線）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PaleLinesParameter(sharedData);
}

public sealed class PaleLinesParameter : QuickShapeParameter
{
    public PaleLinesParameter() : this(null) { }
    public PaleLinesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 7;
        Color1 = Hex(0x3D5AFE);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "線の数", Min: 2, Max: 20, Group: "「青ざめ（縦線）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "線の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float bw = 400f * u, bh = 260f * u;
        float p = DrawKit.EaseOut(Intro(c, 0f, 0.6f));
        Shade(dc, new Vector2(0, o.Y - bh / 2f), new Vector2(0, o.Y + bh / 2f), C1(0.45f * a), C1(0f), o.X - bw / 2f, o.Y - bh / 2f, o.X + bw / 2f, o.Y - bh / 2f + bh * p);
        for (int i = 0; i < Count; i++)
        {
            float x = o.X - bw / 2f + bw * (i + 0.5f) / Count;
            float len = bh * (0.6f + 0.4f * DrawKit.H(Seed, i)) * p;
            Ln(dc, b, new Vector2(x, o.Y - bh / 2f), new Vector2(x, o.Y - bh / 2f + len), C1(0.9f * a), 6f * u);
        }
    }
}

// =====================================================================
//  鼻ちょうちん
//   寝ているときの鼻ちょうちん。ふくらんだりしぼんだり。最後にパチンと割れる(バリエーション2)
// =====================================================================
public sealed class SnotBubblePlugin : QuickShapePlugin
{
    public override string Name => "演出：鼻ちょうちん";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SnotBubbleParameter(sharedData);
}

public sealed class SnotBubbleParameter : QuickShapeParameter
{
    public SnotBubbleParameter() : this(null) { }
    public SnotBubbleParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xBFE9FF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=ふくらんだりしぼんだり　2=最後に割れる", Min: 1, Max: 2, Group: "「鼻ちょうちん」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ちょうちんの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float k = 0.55f + 0.45f * (0.5f + 0.5f * MathF.Sin(t * 2f * Spd(c)));
        if (Variant >= 2)
        {
            float pop = Intro(c, 2.5f, 0.15f);
            if (pop >= 1f) { for (int i = 0; i < 8; i++) Dot(dc, b, Pol(o, 100f * u + (c.Time - 2.65f) * 200f * u, i * MathF.PI / 4f), 8f * u, C1(MathF.Max(0f, 1f - (c.Time - 2.65f) * 3f) * a)); return; }
            k = MathF.Min(1.3f, 0.5f + c.Time * 0.3f);
        }
        float r = 100f * u * k;
        var q = o + new Vector2(r * 0.8f, r * 0.3f);
        Dot(dc, b, q, r, C1(0.35f * a));
        Ring(dc, b, q, r, C1(0.9f * a), 4f * u);
        Path(dc, b, Gfx.ArcPts(q, r * 0.7f, MathF.PI * 1.1f, MathF.PI * 1.45f, 10), Rgb(1, 1, 1, 0.9f * a), 7f * u);
    }
}

// =====================================================================
//  魂が抜ける
//   白いおばけのような魂が、口からふわ〜っと上へ抜けていく
// =====================================================================
public sealed class SoulLeavingPlugin : QuickShapePlugin
{
    public override string Name => "演出：魂が抜ける";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SoulLeavingParameter(sharedData);
}

public sealed class SoulLeavingParameter : QuickShapeParameter
{
    public SoulLeavingParameter() : this(null) { }
    public SoulLeavingParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFFFFF);
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
        ["Color1"] = new ParamGuide(Label: "魂の色"),
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
        float p = Intro(c, 0f, 2f);
        float rise = DrawKit.EaseOut(p) * 300f * u;
        var q = o + new Vector2(MathF.Sin(t * 2f) * 20f * u, -rise);
        float r = 80f * u;
        var body = new List<Vector2>();
        for (int i = 0; i <= 20; i++) body.Add(Pol(q, r, MathF.PI + i * MathF.PI / 20f));
        for (int i = 0; i <= 12; i++)
        {
            float f = i / 12f;
            float sway = MathF.Sin(t * 4f + f * 4f) * 25f * u * f;
            body.Add(new Vector2(q.X + r * (1f - f * 0.9f) + sway, q.Y + f * (rise + 30f * u)));
        }
        for (int i = 12; i >= 0; i--)
        {
            float f = i / 12f;
            float sway = MathF.Sin(t * 4f + f * 4f) * 25f * u * f;
            body.Add(new Vector2(q.X - r * (1f - f * 0.9f) * (f < 0.95f ? 1f : 0.5f) + sway, q.Y + f * (rise + 30f * u)));
        }
        Poly(dc, b, body, C1(0.85f * a));
        Dot(dc, b, q + new Vector2(-25f, -5f) * u, 8f * u, Rgb(0.2f, 0.2f, 0.25f, a));
        Dot(dc, b, q + new Vector2(25f, -5f) * u, 8f * u, Rgb(0.2f, 0.2f, 0.25f, a));
    }
}

// =====================================================================
//  恋の矢（ハートを射抜く）
//   矢が飛んできてハートに刺さり、ハートがドキッとはねる
// =====================================================================
public sealed class CupidArrowPlugin : QuickShapePlugin
{
    public override string Name => "演出：恋の矢（ハートを射抜く）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CupidArrowParameter(sharedData);
}

public sealed class CupidArrowParameter : QuickShapeParameter
{
    public CupidArrowParameter() : this(null) { }
    public CupidArrowParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF4D8D);
        Color2 = Hex(0xC9A227);
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
        ["Color1"] = new ParamGuide(Label: "ハートの色"),
        ["Color2"] = new ParamGuide(Label: "矢の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float r = 130f * u;
        float hit = Intro(c, 0.5f, 0.01f);
        float boing = hit > 0f ? 1f + 0.2f * MathF.Exp(-(c.Time - 0.5f) * 6f) * MathF.Sin((c.Time - 0.5f) * 30f) : 1f;
        Heart(dc, b, o, r * boing, 0f, C1(a));
        float p = Math.Clamp(c.Time / 0.5f, 0f, 1f);
        var dir = Vector2.Normalize(new Vector2(1f, -0.35f));
        var tip = o - dir * (1f - p) * 900f * u + dir * 30f * u;
        var tail = tip - dir * 380f * u;
        Ln(dc, b, tail, tip, C2(a), 10f * u);
        var nrm = new Vector2(-dir.Y, dir.X);
        Poly(dc, b, new List<Vector2> { tip + dir * 40f * u, tip + nrm * 22f * u, tip - nrm * 22f * u }, C2(a));
        for (int i = 0; i < 3; i++)
        {
            var f0 = tail + dir * i * 22f * u;
            Poly(dc, b, new List<Vector2> { f0, f0 - dir * 40f * u + nrm * 30f * u, f0 - dir * 20f * u + nrm * 30f * u, f0 + dir * 20f * u }, C1(a));
            Poly(dc, b, new List<Vector2> { f0, f0 - dir * 40f * u - nrm * 30f * u, f0 - dir * 20f * u - nrm * 30f * u, f0 + dir * 20f * u }, C1(a));
        }
        if (hit > 0f) for (int i = 0; i < 6; i++) Heart(dc, b, Pol(o, r * 1.4f + (c.Time - 0.5f) * 200f * u, -MathF.PI / 2f + (i - 2.5f) * 0.5f), 24f * u, 0f, C1(MathF.Max(0f, 1f - (c.Time - 0.5f)) * a));
    }
}

// =====================================================================
//  キラーン（歯や眼鏡の光）
//   一瞬の強い光のきらめき。決めゼリフの時の歯や眼鏡に
// =====================================================================
public sealed class KiranPlugin : QuickShapePlugin
{
    public override string Name => "演出：キラーン（歯や眼鏡の光）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new KiranParameter(sharedData);
}

public sealed class KiranParameter : QuickShapeParameter
{
    public KiranParameter() : this(null) { }
    public KiranParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFFFFF);
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
        ["Color1"] = new ParamGuide(Label: "光の色"),
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
        float p = Intro(c, 0.1f, 0.5f);
        float k = MathF.Sin(p * MathF.PI);
        if (k <= 0f) return;
        DrawKit.Glow(dc, o, 120f * u * k, C1(0.7f * a * k));
        DrawKit.Twinkle(dc, b, o, 260f * u * k, 10f * u, Color1, a * k, p * 0.8f);
        DrawKit.Twinkle(dc, b, o, 140f * u * k, 6f * u, Color1, a * k, MathF.PI / 4f + p * 0.8f);
    }
}

// =====================================================================
//  ほわほわお花
//   キャラのまわりに小さなお花が咲いて、ふわふわゆれる。うれしい・のんびり
// =====================================================================
public sealed class FluffyFlowersPlugin : QuickShapePlugin
{
    public override string Name => "演出：ほわほわお花";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FluffyFlowersParameter(sharedData);
}

public sealed class FluffyFlowersParameter : QuickShapeParameter
{
    public FluffyFlowersParameter() : this(null) { }
    public FluffyFlowersParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 8;
        Color1 = Hex(0xFFB3C6);
        Color2 = Hex(0xFFF59D);
        Color3 = Hex(0xB3E5FC);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "花の数", Min: 1, Max: 24, Group: "「ほわほわお花」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "花の色1"),
        ["Color2"] = new ParamGuide(Label: "花の色2"),
        ["Color3"] = new ParamGuide(Label: "花の色3"),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var cols = new[] { Color1, Color2, Color3 };
        for (int i = 0; i < Count; i++)
        {
            float ang = i * MathF.PI * 2f / Count + DrawKit.H(Seed, i) * 0.4f;
            float rr = (280f + 80f * DrawKit.H(Seed, i, 1)) * u;
            var q = o + new Vector2(MathF.Cos(ang) * rr * 1.3f, MathF.Sin(ang) * rr) + new Vector2(0, MathF.Sin(t * 2f + i) * 12f * u);
            float k = DrawKit.EaseOutBack(Intro(c, i * 0.08f, 0.4f));
            float s = (30f + 20f * DrawKit.H(Seed, i, 2)) * u * k;
            float rot = MathF.Sin(t * 1.5f + i) * 0.4f;
            for (int j = 0; j < 5; j++) Dot(dc, b, Pol(q, s * 0.75f, rot + j * MathF.PI * 2f / 5f), s * 0.55f, Col(cols[i % 3], 0.9f * a));
            Dot(dc, b, q, s * 0.4f, Rgb(1f, 0.85f, 0.3f, a));
        }
    }
}

// =====================================================================
//  ガタガタ震える線
//   左右に「))」のような震えの線がブルブル。こわい・寒い時に。「値」で幅
// =====================================================================
public sealed class ShakeLinesPlugin : QuickShapePlugin
{
    public override string Name => "演出：ガタガタ震える線";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ShakeLinesParameter(sharedData);
}

public sealed class ShakeLinesParameter : QuickShapeParameter
{
    public ShakeLinesParameter() : this(null) { }
    public ShakeLinesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x333333);
    }
    protected override double DefAmount => 50;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "左右の幅", Group: "「ガタガタ震える線」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "線の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float gap = (150f + 400f * Amt(c)) * u;
        float jit = MathF.Sin(t * 50f * Spd(c)) * 6f * u;
        for (int side = -1; side <= 1; side += 2)
            for (int k = 0; k < 3; k++)
            {
                var q = o + new Vector2(side * (gap / 2f + k * 30f * u) + jit * side, (k - 1) * 0f);
                float r = (60f + k * 25f) * u;
                float a0 = side > 0 ? -0.6f : MathF.PI - 0.6f;
                Path(dc, b, Gfx.ArcPts(q - new Vector2(side * r * 0.7f, 0), r, a0, a0 + 1.2f, 16), C1(a), 6f * u);
            }
    }
}

// =====================================================================
//  擬音スタンプ
//   「ドーン」「ゴゴゴ」などの太い文字がドンと出てゆれる。「文字」で言葉、バリエーションで出方
// =====================================================================
public sealed class OnomatopoeiaStampPlugin : QuickShapePlugin
{
    public override string Name => "演出：擬音スタンプ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new OnomatopoeiaStampParameter(sharedData);
}

public sealed class OnomatopoeiaStampParameter : QuickShapeParameter
{
    public OnomatopoeiaStampParameter() : this(null) { }
    public OnomatopoeiaStampParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ドーン！";
        Color1 = Hex(0xFFEB3B);
        Color2 = Hex(0x000000);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "言葉", Tip: "改行すると、ずらして並びます", Group: "「擬音スタンプ」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=ドンと出る　2=ふるえる　3=ポンと出る", Min: 1, Max: 3, Group: "「擬音スタンプ」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "文字の色"),
        ["Color2"] = new ParamGuide(Label: "ふちの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Str.Split('\n');
        float fs = 220f * u;
        for (int i = 0; i < it.Length; i++)
        {
            float p = Intro(c, i * 0.2f, 0.2f);
            if (p <= 0f) continue;
            float k = Variant == 2 ? 1f + 0.06f * MathF.Sin(t * 30f) : Variant == 3 ? DrawKit.EaseOutBack(p) : (p < 1f ? 2.5f - 1.5f * p : 1f);
            var jit = Variant == 2 ? new Vector2(MathF.Sin(t * 47f + i), MathF.Cos(t * 39f + i)) * 8f * u : Vector2.Zero;
            var q = o + new Vector2((i - (it.Length - 1) / 2f) * 120f * u, (i - (it.Length - 1) / 2f) * fs * 0.9f) + jit;
            var sk = Matrix3x2.CreateSkew(-0.2f, 0f, q);
            var prev = dc.Transform;
            dc.Transform = sk * prev;
            Say(dc, b, it[i], fs, q + new Vector2(10f, 12f) * u, Rgb(0, 0, 0, 0.4f * a), scale: k);
            Say(dc, b, it[i], fs, q, C1(a), C2(a), 16f * u, scale: k);
            dc.Transform = prev;
        }
    }
}

// =====================================================================
//  ツッコミの手
//   「なんでやねん！」の手が横からビシッと入り、当たったところに衝撃マーク
// =====================================================================
public sealed class TsukkomiHandPlugin : QuickShapePlugin
{
    public override string Name => "演出：ツッコミの手";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TsukkomiHandParameter(sharedData);
}

public sealed class TsukkomiHandParameter : QuickShapeParameter
{
    public TsukkomiHandParameter() : this(null) { }
    public TsukkomiHandParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "なんでやねん！";
        Color1 = Hex(0xFFD9B3);
        Color3 = Hex(0xFFEB3B);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "ツッコミの言葉", Group: "「ツッコミの手」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "手の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "衝撃の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float p = Intro(c, 0f, 0.18f);
        float back = MathF.Max(0f, Intro(c, 0.6f, 0.3f));
        float x = o.X - 900f * u * (1f - DrawKit.EaseOut(p)) - back * 900f * u;
        var hand = new Vector2(x, o.Y);
        Box(dc, b, hand.X - 600f * u, hand.Y - 50f * u, hand.X - 120f * u, hand.Y + 50f * u, Rgb(1, 1, 1, a));
        Ln(dc, b, new Vector2(hand.X - 600f * u, hand.Y - 50f * u), new Vector2(hand.X - 120f * u, hand.Y - 50f * u), Rgb(0, 0, 0, a), 5f * u);
        Ln(dc, b, new Vector2(hand.X - 600f * u, hand.Y + 50f * u), new Vector2(hand.X - 120f * u, hand.Y + 50f * u), Rgb(0, 0, 0, a), 5f * u);
        var palm = Pts(hand, u, u, -140f, -60f, 10f, -70f, 60f, -50f, 70f, 0f, 60f, 50f, 10f, 70f, -140f, 60f);
        Poly(dc, b, palm, C1(a));
        Outline(dc, b, palm, Rgb(0, 0, 0, a), 5f * u);
        for (int i = 0; i < 3; i++) Ln(dc, b, hand + new Vector2(-30f * u, (i - 1) * 28f * u), hand + new Vector2(50f * u, (i - 1) * 32f * u), Rgb(0, 0, 0, 0.6f * a), 4f * u);
        if (p >= 1f && back <= 0f)
        {
            float s = c.Time - 0.18f;
            var hit = hand + new Vector2(110f * u, 0);
            Poly(dc, b, BubbleKit.SpikyPts(hit, 90f * u + s * 120f * u, 80f * u + s * 100f * u, 10, 0.4f, Seed), C3(a));
            for (int i = 0; i < 5; i++) Ln(dc, b, Pol(hit, 120f * u, -0.9f + i * 0.45f), Pol(hit, 190f * u + s * 100f * u, -0.9f + i * 0.45f), Rgb(0, 0, 0, a), 6f * u);
            Say(dc, b, Str, 90f * u, o + new Vector2(0, -200f * u), Rgb(1, 1, 1, a), Rgb(0.8f, 0.1f, 0.1f, a), 8f * u);
        }
    }
}
