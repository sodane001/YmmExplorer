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
//  ひらめき電球
//   電球がピカッと光り、まわりに線が飛び出す。ひらめいた時に
// =====================================================================
public sealed class IdeaBulbPlugin : QuickShapePlugin
{
    public override string Name => "演出：ひらめき電球";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new IdeaBulbParameter(sharedData);
}

public sealed class IdeaBulbParameter : QuickShapeParameter
{
    public IdeaBulbParameter() : this(null) { }
    public IdeaBulbParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFEB3B);
        Color2 = Hex(0x9E9E9E);
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
        ["Color2"] = new ParamGuide(Label: "口金の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.3f));
        float on = Intro(c, 0.35f, 0.1f);
        float r = 90f * u * k;
        if (on > 0f) DrawKit.Glow(dc, o, 300f * u * (1f + 0.05f * MathF.Sin(t * 8f)), C1(0.55f * a * on));
        Dot(dc, b, o, r, on > 0f ? C1(a) : Rgb(0.95f, 0.95f, 0.9f, a));
        Poly(dc, b, Pts(o, u * k, u * k, -50f, 60f, 50f, 60f, 34f, 110f, -34f, 110f), on > 0f ? C1(a) : Rgb(0.95f, 0.95f, 0.9f, a));
        for (int i = 0; i < 3; i++) Box(dc, b, o.X - 36f * u * k, o.Y + (110f + i * 18f) * u * k, o.X + 36f * u * k, o.Y + (122f + i * 18f) * u * k, C2(a));
        Path(dc, b, Pts(o, u * k, u * k, -20f, 50f, -10f, 0f, 0f, 20f, 10f, 0f, 20f, 50f), Rgb(1f, 0.6f, 0.1f, 0.8f * a), 5f * u);
        if (on > 0f)
        {
            float f = DrawKit.EaseOut(Math.Clamp((c.Time - 0.35f) * 3f, 0f, 1f));
            for (int i = 0; i < 8; i++)
            {
                float ang = -MathF.PI / 2f + (i - 3.5f) * 0.42f;
                Ln(dc, b, Pol(o, (130f + 30f * f) * u, ang), Pol(o, (170f + 80f * f) * u, ang), C1(a), 10f * u);
            }
        }
    }
}

// =====================================================================
//  もやもや
//   灰色のもやが、頭の上でぐるぐる渦を巻く。悩んでいる時に
// =====================================================================
public sealed class MoyamoyaPlugin : QuickShapePlugin
{
    public override string Name => "演出：もやもや";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MoyamoyaParameter(sharedData);
}

public sealed class MoyamoyaParameter : QuickShapeParameter
{
    public MoyamoyaParameter() : this(null) { }
    public MoyamoyaParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x78909C);
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
        ["Color1"] = new ParamGuide(Label: "もやの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        for (int k = 0; k < 3; k++)
        {
            var sp = new List<Vector2>();
            for (int i = 0; i <= 60; i++)
            {
                float f = i / 60f;
                float ang = f * MathF.PI * 4f + t * (1.5f + k * 0.4f) * Spd(c) + k * 2f;
                float r = (30f + 140f * f) * u;
                sp.Add(o + new Vector2(MathF.Cos(ang) * r * 1.4f, MathF.Sin(ang) * r * 0.7f) + new Vector2(MathF.Sin(f * 9f + t * 3f) * 8f * u, 0));
            }
            Path(dc, b, sp, C1((0.5f - k * 0.12f) * a), (14f - k * 3f) * u);
        }
    }
}

// =====================================================================
//  イライラの湯気
//   頭のてっぺんから、湯気がシューッと勢いよく出る
// =====================================================================
public sealed class AngrySteamPlugin : QuickShapePlugin
{
    public override string Name => "演出：イライラの湯気";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AngrySteamParameter(sharedData);
}

public sealed class AngrySteamParameter : QuickShapeParameter
{
    public AngrySteamParameter() : this(null) { }
    public AngrySteamParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 3;
        Color1 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "湯気の本数", Min: 1, Max: 6, Group: "「イライラの湯気」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "湯気の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        for (int i = 0; i < Count; i++)
        {
            float side = Count == 1 ? 0f : (i / (float)(Count - 1) - 0.5f) * 2f;
            for (int j = 0; j < 4; j++)
            {
                float ph = Gfx.Mod(t * 1.6f * Spd(c) + j * 0.25f + i * 0.13f, 1f);
                var q = o + new Vector2(side * 80f * u + side * ph * 120f * u + MathF.Sin(ph * 10f + i) * 10f * u, -ph * 260f * u);
                Dot(dc, b, q, (18f + ph * 40f) * u, C1(0.8f * a * (1f - ph)));
            }
        }
    }
}

// =====================================================================
//  ハートの目
//   目の位置に2つのハートがドキドキ脈打つ。「値」で目の間隔
// =====================================================================
public sealed class HeartEyesPlugin : QuickShapePlugin
{
    public override string Name => "演出：ハートの目";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HeartEyesParameter(sharedData);
}

public sealed class HeartEyesParameter : QuickShapeParameter
{
    public HeartEyesParameter() : this(null) { }
    public HeartEyesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF1744);
    }
    protected override double DefAmount => 40;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "目の間隔", Group: "「ハートの目」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ハートの色"),
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
        float beat = 1f + 0.18f * MathF.Pow(MathF.Max(0f, MathF.Sin(t * 7f * Spd(c))), 4f);
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.3f));
        for (int e = -1; e <= 1; e += 2)
        {
            var q = o + new Vector2(e * gap / 2f, 0);
            Heart(dc, b, q, 70f * u * beat * k, 0f, C1(a));
            Dot(dc, b, q + new Vector2(-24f, -20f) * u * beat, 12f * u * k, Rgb(1, 1, 1, 0.7f * a));
        }
    }
}

// =====================================================================
//  涙目のうるうる
//   目のふちに涙がたまって、光がゆれる。「値」で目の間隔
// =====================================================================
public sealed class TearyEyesPlugin : QuickShapePlugin
{
    public override string Name => "演出：涙目のうるうる";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TearyEyesParameter(sharedData);
}

public sealed class TearyEyesParameter : QuickShapeParameter
{
    public TearyEyesParameter() : this(null) { }
    public TearyEyesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x81D4FA);
    }
    protected override double DefAmount => 40;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "目の間隔", Group: "「涙目のうるうる」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "涙の色"),
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
        float gap = (80f + 300f * Amt(c)) * u;
        float fill = DrawKit.EaseOut(Intro(c, 0f, 1f));
        for (int e = -1; e <= 1; e += 2)
        {
            var q = o + new Vector2(e * gap / 2f, 0);
            var lens = new List<Vector2>();
            for (int i = 0; i <= 24; i++) { float f = i / 24f; float x = (f - 0.5f) * 140f * u; lens.Add(q + new Vector2(x, 20f * u - MathF.Sin(f * MathF.PI) * 50f * u * fill + MathF.Sin(t * 6f + f * 6f) * 3f * u)); }
            for (int i = 24; i >= 0; i--) { float f = i / 24f; float x = (f - 0.5f) * 140f * u; lens.Add(q + new Vector2(x, 20f * u + MathF.Sin(f * MathF.PI) * 18f * u)); }
            Poly(dc, b, lens, C1(0.6f * a));
            float sh = MathF.Sin(t * 5f) * 15f * u;
            Dot(dc, b, q + new Vector2(-25f * u + sh, -2f * u), 12f * u, Rgb(1, 1, 1, 0.9f * a));
            Dot(dc, b, q + new Vector2(20f * u + sh * 0.5f, 10f * u), 6f * u, Rgb(1, 1, 1, 0.8f * a));
        }
    }
}

// =====================================================================
//  ドキドキ（鼓動）
//   胸の位置でハートが鼓動して、波紋が広がる。「ドキッ」の文字つき
// =====================================================================
public sealed class DokidokiPlugin : QuickShapePlugin
{
    public override string Name => "演出：ドキドキ（鼓動）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DokidokiParameter(sharedData);
}

public sealed class DokidokiParameter : QuickShapeParameter
{
    public DokidokiParameter() : this(null) { }
    public DokidokiParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ドキッ";
        Color1 = Hex(0xFF4081);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Group: "「ドキドキ（鼓動）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ハートの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float per = 0.8f / MathF.Max(0.1f, Spd(c));
        float ph = Gfx.Mod(c.Time, per) / per;
        float beat = 1f + 0.25f * MathF.Exp(-ph * 8f) + 0.12f * MathF.Exp(-MathF.Max(0f, ph - 0.2f) * 10f) * (ph > 0.2f ? 1f : 0f);
        for (int i = 0; i < 2; i++)
        {
            float f = Gfx.Mod(ph + i * 0.5f, 1f);
            Ring(dc, b, o, (90f + 220f * f) * u, C1((1f - f) * 0.6f * a), 6f * u * (1f - f) + 1f);
        }
        Heart(dc, b, o, 90f * u * beat, 0f, C1(a));
        if (Str.Length > 0) Say(dc, b, Str, 60f * u, o + new Vector2(180f, -160f) * u, C1(a), Rgb(1, 1, 1, a), 5f * u, scale: beat);
    }
}

// =====================================================================
//  ため息
//   口から、ふわっと白い息が横に流れて消える
// =====================================================================
public sealed class SighPlugin : QuickShapePlugin
{
    public override string Name => "演出：ため息";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SighParameter(sharedData);
}

public sealed class SighParameter : QuickShapeParameter
{
    public SighParameter() : this(null) { }
    public SighParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFFFFF);
        Variant = 1;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=右へ　2=左へ", Min: 1, Max: 2, Group: "「ため息」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "息の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float dir = Variant >= 2 ? -1f : 1f;
        float per = 2.4f / MathF.Max(0.1f, Spd(c));
        float f = Gfx.Mod(c.Time, per) / per;
        for (int i = 0; i < 5; i++)
        {
            float g = Math.Clamp(f * 1.4f - i * 0.08f, 0f, 1f);
            if (g <= 0f) continue;
            var q = o + new Vector2(dir * (40f + 280f * DrawKit.EaseOut(g)) * u, -g * 60f * u + MathF.Sin(g * 6f + i) * 12f * u);
            Dot(dc, b, q, (20f + 50f * g) * u * (1f - i * 0.12f), C1(0.6f * a * (1f - g)));
        }
    }
}

// =====================================================================
//  よだれ・おいしそう
//   口元からよだれがたれて、まわりにキラキラ。おいしそうな時に
// =====================================================================
public sealed class DroolPlugin : QuickShapePlugin
{
    public override string Name => "演出：よだれ・おいしそう";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DroolParameter(sharedData);
}

public sealed class DroolParameter : QuickShapeParameter
{
    public DroolParameter() : this(null) { }
    public DroolParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xB3E5FC);
        Color3 = Hex(0xFFF59D);
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
        ["Color1"] = new ParamGuide(Label: "よだれの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "キラキラの色"),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float per = 1.6f / MathF.Max(0.1f, Spd(c));
        float f = Gfx.Mod(c.Time, per) / per;
        float len = (20f + 90f * MathF.Min(1f, f * 1.6f)) * u;
        Poly(dc, b, new List<Vector2> { o + new Vector2(-8f * u, 0), o + new Vector2(8f * u, 0), o + new Vector2(5f * u, len), o + new Vector2(-5f * u, len) }, C1(0.85f * a));
        Dot(dc, b, o + new Vector2(0, len + 8f * u), 12f * u, C1(0.9f * a));
        if (f > 0.7f) Dot(dc, b, o + new Vector2(0, len + 8f * u + (f - 0.7f) * 900f * u), 10f * u, C1(0.9f * a * (1f - (f - 0.7f) / 0.3f)));
        for (int i = 0; i < 4; i++)
        {
            var q = o + new Vector2((i % 2 == 0 ? -1f : 1f) * (120f + i * 30f) * u, -60f * u - i * 40f * u);
            DrawKit.Twinkle(dc, b, q, 40f * u * (0.5f + 0.5f * MathF.Sin(t * 6f + i * 1.7f)), 4f * u, Color3, a);
        }
    }
}

// =====================================================================
//  ひやあせの線
//   こめかみに、斜めの3本線と汗のしずく。ギクッとした時に
// =====================================================================
public sealed class ColdSweatLinesPlugin : QuickShapePlugin
{
    public override string Name => "演出：ひやあせの線";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ColdSweatLinesParameter(sharedData);
}

public sealed class ColdSweatLinesParameter : QuickShapeParameter
{
    public ColdSweatLinesParameter() : this(null) { }
    public ColdSweatLinesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x3F51B5);
        Color2 = Hex(0x81D4FA);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=右側　2=左側", Min: 1, Max: 2, Group: "「ひやあせの線」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "線の色"),
        ["Color2"] = new ParamGuide(Label: "汗の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float dir = Variant >= 2 ? -1f : 1f;
        float k = DrawKit.EaseOut(Intro(c, 0f, 0.3f));
        for (int i = 0; i < 3; i++)
        {
            var a0 = o + new Vector2(dir * i * 26f * u, i * 10f * u);
            Ln(dc, b, a0, a0 + new Vector2(dir * -30f * u, 80f * u) * k, C1(a), 7f * u);
        }
        float ph = Gfx.Mod(c.Time * 0.7f * Spd(c), 1f);
        var dp = o + new Vector2(dir * 100f * u, 40f * u + ph * 80f * u);
        Poly(dc, b, Pts(dp, 20f * u, 20f * u, 0f, -1.4f, 0.75f, 0f, 0.6f, 0.6f, 0f, 0.9f, -0.6f, 0.6f, -0.75f, 0f), C2(a * (1f - ph * 0.5f)));
    }
}

// =====================================================================
//  背後に雷（ガーン）
//   背景が暗くなって、後ろに稲妻がピシャーンと落ちる。「ガーン」の文字つき
// =====================================================================
public sealed class ShockThunderPlugin : QuickShapePlugin
{
    public override string Name => "演出：背後に雷（ガーン）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ShockThunderParameter(sharedData);
}

public sealed class ShockThunderParameter : QuickShapeParameter
{
    public ShockThunderParameter() : this(null) { }
    public ShockThunderParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ガーン";
        Color1 = Hex(0xFFEB3B);
        Color2 = Hex(0x1A1A2E);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Group: "「背後に雷（ガーン）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "背景を暗くする", Group: "「背後に雷（ガーン）」の設定"),
        ["Color1"] = new ParamGuide(Label: "稲妻の色"),
        ["Color2"] = new ParamGuide(Label: "背景の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float dark = Intro(c, 0f, 0.15f);
        if (Background) Box(dc, b, 0, 0, w, h, C2(0.75f * dark * a));
        float flash = MathF.Max(0f, 1f - (c.Time - 0.15f) * 4f) * (c.Time > 0.15f ? 1f : 0f);
        if (Background && flash > 0f) Box(dc, b, 0, 0, w, h, Rgb(1, 1, 1, 0.5f * flash * a));
        if (c.Time < 0.15f) return;
        for (int k = 0; k < 2; k++)
        {
            var bolt = new List<Vector2>();
            var p = o + new Vector2((k == 0 ? -1f : 1f) * 260f * u, -600f * u);
            bolt.Add(p);
            for (int i = 0; i < 6; i++) { p += new Vector2((DrawKit.H(Seed + k, i) - 0.5f) * 160f * u, 110f * u); bolt.Add(p); }
            float flick = 0.7f + 0.3f * MathF.Sin(t * 50f + k);
            Path(dc, b, bolt, C1(0.4f * a * flick), 30f * u);
            Path(dc, b, bolt, C1(a * flick), 12f * u);
            Path(dc, b, bolt, Rgb(1, 1, 1, a * flick), 4f * u);
        }
        Say(dc, b, Str, 160f * u, o + new Vector2(0, -220f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, a), 8f * u);
    }
}
