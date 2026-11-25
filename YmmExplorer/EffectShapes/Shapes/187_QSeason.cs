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
//  お花見（桜の木）
//   満開の桜の木。花びらが舞い、根元にレジャーシート。春の場面に
// =====================================================================
public sealed class HanamiPlugin : QuickShapePlugin
{
    public override string Name => "演出：お花見（桜の木）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HanamiParameter(sharedData);
}

public sealed class HanamiParameter : QuickShapeParameter
{
    public HanamiParameter() : this(null) { }
    public HanamiParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 40;
        Color1 = Hex(0xFFC1D6);
        Color2 = Hex(0x6B4A3A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "舞う花びらの数", Min: 0, Max: 150, Group: "「お花見（桜の木）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "花の色"),
        ["Color2"] = new ParamGuide(Label: "幹の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var root = o + new Vector2(0, 330f * u);
        Poly(dc, b, Pts(root, u, u, -40f, 0f, 40f, 0f, 22f, -260f, 120f, -380f, 100f, -400f, 10f, -300f, -90f, -420f, -110f, -400f, -20f, -260f), C2(a));
        for (int i = 0; i < 26; i++)
        {
            var q = root + new Vector2((DrawKit.H(Seed, i) - 0.5f) * 760f * u, -380f * u - DrawKit.H(Seed, i, 1) * 280f * u + MathF.Abs(DrawKit.H(Seed, i) - 0.5f) * 200f * u);
            Dot(dc, b, q, (90f + 60f * DrawKit.H(Seed, i, 2)) * u, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), DrawKit.H(Seed, i, 3) * 0.5f), 0.9f * a));
        }
        Poly(dc, b, Pts(root + new Vector2(220f * u, 30f * u), u, u, -170f, -30f, 170f, -30f, 200f, 40f, -200f, 40f), Rgb(0.25f, 0.5f, 0.9f, 0.9f * a));
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 0.15f * Spd(c) + DrawKit.H(Seed, i, 5), 1f);
            var q = new Vector2(o.X + (DrawKit.H(Seed, i, 6) - 0.3f) * 1400f * u + ph * 300f * u + MathF.Sin(t * 2f + i) * 30f * u, o.Y - 500f * u + ph * 1000f * u);
            var petal = DrawKit.Place(Pts(Vector2.Zero, 12f * u, 8f * u, 0f, -1f, 1f, 0f, 0f, 1f, -1f, 0f), q, t * 2f + i);
            Poly(dc, b, petal, C1(a * (1f - ph * 0.3f)));
        }
    }
}

// =====================================================================
//  節分（豆まきと鬼）
//   赤鬼のお面に豆がパラパラ当たる。「鬼は外！」(文字)
// =====================================================================
public sealed class SetsubunPlugin : QuickShapePlugin
{
    public override string Name => "演出：節分（豆まきと鬼）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SetsubunParameter(sharedData);
}

public sealed class SetsubunParameter : QuickShapeParameter
{
    public SetsubunParameter() : this(null) { }
    public SetsubunParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "鬼は外！";
        Count = 20;
        Color1 = Hex(0xE53935);
        Color2 = Hex(0xF5DEB3);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "かけ声", Group: "「節分（豆まきと鬼）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "豆の数", Min: 0, Max: 60, Group: "「節分（豆まきと鬼）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "鬼の色"),
        ["Color2"] = new ParamGuide(Label: "豆の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float r = 220f * u;
        float hitShake = MathF.Sin(t * 40f) * 5f * u;
        var f = o + new Vector2(hitShake, 0);
        Poly(dc, b, Pts(f + new Vector2(-120f, -200f) * u, u, u, -30f, 40f, 0f, -70f, 30f, 40f), Rgb(1f, 0.95f, 0.85f, a));
        Poly(dc, b, Pts(f + new Vector2(120f, -200f) * u, u, u, -30f, 40f, 0f, -70f, 30f, 40f), Rgb(1f, 0.95f, 0.85f, a));
        Dot(dc, b, f, r, C1(a));
        for (int i = 0; i < 10; i++) Dot(dc, b, f + new Vector2((i - 4.5f) * 40f * u, -r * 0.9f + MathF.Abs(i - 4.5f) * 10f * u), 40f * u, Rgb(0.15f, 0.12f, 0.1f, a));
        for (int e = -1; e <= 1; e += 2) { Dot(dc, b, f + new Vector2(e * 80f, -20f) * u, 40f * u, Rgb(1, 1, 1, a)); Dot(dc, b, f + new Vector2(e * 75f, -15f) * u, 18f * u, Rgb(0, 0, 0, a)); Ln(dc, b, f + new Vector2(e * 130f, -90f) * u, f + new Vector2(e * 40f, -60f) * u, Rgb(0.1f, 0.05f, 0.05f, a), 14f * u); }
        Path(dc, b, Gfx.ArcPts(f + new Vector2(0, 150f * u), 110f * u, MathF.PI * 1.15f, MathF.PI * 1.85f, 20), Rgb(0.15f, 0.05f, 0.05f, a), 12f * u);
        Poly(dc, b, Pts(f + new Vector2(-60f, 70f) * u, u, u, -12f, 0f, 12f, 0f, 0f, 36f), Rgb(1, 1, 1, a));
        Poly(dc, b, Pts(f + new Vector2(60f, 70f) * u, u, u, -12f, 0f, 12f, 0f, 0f, 36f), Rgb(1, 1, 1, a));
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 1.2f + DrawKit.H(Seed, i), 1f);
            var st = o + new Vector2(-900f * u, 300f * u + DrawKit.H(Seed, i, 1) * 200f * u);
            var en = o + new Vector2((DrawKit.H(Seed, i, 2) - 0.5f) * 300f * u, (DrawKit.H(Seed, i, 3) - 0.5f) * 300f * u);
            var q = ph < 0.7f ? Vector2.Lerp(st, en, ph / 0.7f) - new Vector2(0, MathF.Sin(ph / 0.7f * MathF.PI) * 200f * u) : en + new Vector2(-(ph - 0.7f) * 300f * u, (ph - 0.7f) * (ph - 0.7f) * 3000f * u);
            Dot(dc, b, q, 14f * u, C2(a));
        }
        Say(dc, b, Str, 100f * u, o + new Vector2(-500f * u, -300f * u), Rgb(1, 1, 1, a), Rgb(0.6f, 0.1f, 0.1f, a), 6f * u);
    }
}

// =====================================================================
//  ひな祭り（ひな段）
//   赤いひな段に、おだいりさまとおひなさま。桃の花とぼんぼり
// =====================================================================
public sealed class HinamatsuriPlugin : QuickShapePlugin
{
    public override string Name => "演出：ひな祭り（ひな段）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HinamatsuriParameter(sharedData);
}

public sealed class HinamatsuriParameter : QuickShapeParameter
{
    public HinamatsuriParameter() : this(null) { }
    public HinamatsuriParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xD32F2F);
        Color2 = Hex(0xFFB6C9);
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
        ["Color1"] = new ParamGuide(Label: "ひな段の色"),
        ["Color2"] = new ParamGuide(Label: "桃の花の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        for (int i = 0; i < 3; i++)
        {
            float y = o.Y - 120f * u + i * 130f * u, ww = (600f + i * 200f) * u;
            Box(dc, b, o.X - ww / 2f, y, o.X + ww / 2f, y + 130f * u, C1(a));
            Box(dc, b, o.X - ww / 2f, y, o.X + ww / 2f, y + 10f * u, Rgb(1, 1, 1, 0.2f * a));
        }
        for (int e = -1; e <= 1; e += 2)
        {
            var q = o + new Vector2(e * 120f * u, -200f * u);
            Poly(dc, b, Pts(q, u, u, -90f, 80f, 90f, 80f, 50f, -20f, -50f, -20f), e < 0 ? Rgb(0.15f, 0.15f, 0.3f, a) : Rgb(0.85f, 0.2f, 0.3f, a));
            Dot(dc, b, q + new Vector2(0, -60f * u), 46f * u, Rgb(1f, 0.95f, 0.9f, a));
            Oval(dc, b, q + new Vector2(0, -95f * u), 46f * u, 20f * u, Rgb(0.1f, 0.1f, 0.1f, a));
            if (e < 0) Box(dc, b, q.X - 8f * u, q.Y - 160f * u, q.X + 8f * u, q.Y - 100f * u, Rgb(0.1f, 0.1f, 0.1f, a));
            else Box(dc, b, q.X - 40f * u, q.Y - 130f * u, q.X + 40f * u, q.Y - 120f * u, Rgb(1f, 0.85f, 0.3f, a));
        }
        for (int e = -1; e <= 1; e += 2)
        {
            var lp = o + new Vector2(e * 360f * u, -200f * u);
            Ln(dc, b, lp, lp + new Vector2(0, 80f * u), Rgb(0.3f, 0.2f, 0.1f, a), 6f * u);
            Oval(dc, b, lp - new Vector2(0, 30f * u), 40f * u, 55f * u, Rgb(1f, 0.95f, 0.85f, a));
            DrawKit.Glow(dc, lp - new Vector2(0, 30f * u), 90f * u, Rgb(1f, 0.85f, 0.5f, 0.4f * a * (0.8f + 0.2f * MathF.Sin(t * 3f))));
            for (int k = 0; k < 6; k++) { var fp = o + new Vector2(e * (420f + k * 30f) * u, -380f * u + k * 25f * u); for (int j = 0; j < 5; j++) Dot(dc, b, Pol(fp, 10f * u, j * 1.256f), 9f * u, C2(a)); }
        }
    }
}

// =====================================================================
//  こいのぼり
//   ポールにつけたこいのぼりが、風でゆらゆら泳ぐ。「数」で匹数
// =====================================================================
public sealed class KoinoboriPlugin : QuickShapePlugin
{
    public override string Name => "演出：こいのぼり";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new KoinoboriParameter(sharedData);
}

public sealed class KoinoboriParameter : QuickShapeParameter
{
    public KoinoboriParameter() : this(null) { }
    public KoinoboriParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 3;
        Color1 = Hex(0x1A237E);
        Color2 = Hex(0xE53935);
        Color3 = Hex(0x1E88E5);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "こいの数", Min: 1, Max: 5, Group: "「こいのぼり」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "1匹目の色"),
        ["Color2"] = new ParamGuide(Label: "2匹目の色"),
        ["Color3"] = new ParamGuide(Label: "3匹目の色"),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var pole = o + new Vector2(-450f * u, -400f * u);
        Ln(dc, b, pole, pole + new Vector2(0, 900f * u), Rgb(0.6f, 0.5f, 0.4f, a), 14f * u);
        Dot(dc, b, pole - new Vector2(0, 20f * u), 26f * u, Rgb(1f, 0.85f, 0.3f, a));
        var cols = new[] { Color1, Color2, Color3, Hex(0x43A047), Hex(0xFB8C00) };
        for (int i = 0; i < Count; i++)
        {
            var at = pole + new Vector2(0, 80f * u + i * 170f * u);
            float len = (560f - i * 70f) * u, fat = (80f - i * 8f) * u;
            var top = new List<Vector2>(); var bot = new List<Vector2>();
            for (int k = 0; k <= 16; k++)
            {
                float f = k / 16f;
                float wave = MathF.Sin(t * 5f * Spd(c) - f * 5f + i) * 30f * u * f;
                float th = fat * (1f - f * 0.35f);
                var cpt = at + new Vector2(f * len, wave + f * 30f * u);
                top.Add(cpt - new Vector2(0, th)); bot.Add(cpt + new Vector2(0, th));
            }
            var tailTip = at + new Vector2(len + 50f * u, MathF.Sin(t * 5f * Spd(c) - 5f + i) * 30f * u + 30f * u);
            var poly = new List<Vector2>(top) { tailTip - new Vector2(0, fat * 0.7f), top[^1] + (bot[^1] - top[^1]) * 0.5f, tailTip + new Vector2(0, fat * 0.7f) };
            for (int k = bot.Count - 1; k >= 0; k--) poly.Add(bot[k]);
            Poly(dc, b, poly, Col(cols[i % cols.Length], a));
            for (int k = 3; k < 15; k += 3) Path(dc, b, Gfx.ArcPts((top[k] + bot[k]) / 2f, fat * 0.5f, -0.9f, 0.9f, 8), Rgb(1, 1, 1, 0.5f * a), 4f * u);
            var eye = (top[1] + bot[1]) / 2f + new Vector2(20f * u, -fat * 0.2f);
            Dot(dc, b, eye, fat * 0.3f, Rgb(1, 1, 1, a)); Dot(dc, b, eye, fat * 0.15f, Rgb(0, 0, 0, a));
        }
    }
}

// =====================================================================
//  夏祭りのちょうちん列
//   横に張ったひもに、赤いちょうちんがずらり。ゆらゆら光る
// =====================================================================
public sealed class FestivalLanternsPlugin : QuickShapePlugin
{
    public override string Name => "演出：夏祭りのちょうちん列";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FestivalLanternsParameter(sharedData);
}

public sealed class FestivalLanternsParameter : QuickShapeParameter
{
    public FestivalLanternsParameter() : this(null) { }
    public FestivalLanternsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 9;
        Color1 = Hex(0xE53935);
        Text = "祭";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "ちょうちんの文字", Group: "「夏祭りのちょうちん列」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "ちょうちんの数", Min: 2, Max: 24, Group: "「夏祭りのちょうちん列」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ちょうちんの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
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
        float s = U0(c);
        float y0 = h * c.F(PosY) / 100f * 0.6f;
        var rope = new List<Vector2>();
        for (int i = 0; i <= 40; i++) { float f = i / 40f; rope.Add(new Vector2(-20 + (w + 40) * f, y0 + MathF.Sin(f * MathF.PI) * 80f * s)); }
        Path(dc, b, rope, Rgb(0.2f, 0.15f, 0.1f, a), 4f * s);
        for (int i = 0; i < Count; i++)
        {
            float f = (i + 0.5f) / Count;
            var hang = new Vector2(-20 + (w + 40) * f, y0 + MathF.Sin(f * MathF.PI) * 80f * s);
            float sw = MathF.Sin(t * 1.5f + i) * 0.08f;
            var q = hang + new Vector2(MathF.Sin(sw) * 90f * s, 90f * s);
            float fl = 0.85f + 0.15f * MathF.Sin(t * 7f + i * 3f);
            DrawKit.Glow(dc, q, 160f * s, Rgb(1f, 0.6f, 0.3f, 0.35f * fl * a));
            Ln(dc, b, hang, q - new Vector2(0, 60f * s), Rgb(0.2f, 0.15f, 0.1f, a), 3f * s);
            Oval(dc, b, q, 52f * s, 66f * s, C1(a));
            Oval(dc, b, q, 40f * s, 56f * s, Rgb(1f, 0.55f, 0.35f, 0.5f * fl * a));
            for (int k = -2; k <= 2; k++) Path(dc, b, Gfx.ArcPts(q + new Vector2(0, k * 22f * s), 52f * s * MathF.Sqrt(1f - k * k / 9f), 0.2f, MathF.PI - 0.2f, 10), Rgb(0, 0, 0, 0.2f * a), 2f * s);
            Box(dc, b, q.X - 24f * s, q.Y - 72f * s, q.X + 24f * s, q.Y - 60f * s, Rgb(0.15f, 0.12f, 0.1f, a));
            Box(dc, b, q.X - 24f * s, q.Y + 60f * s, q.X + 24f * s, q.Y + 72f * s, Rgb(0.15f, 0.12f, 0.1f, a));
            Say(dc, b, Str, 40f * s, q, Rgb(0.1f, 0.05f, 0.05f, 0.8f * a));
        }
    }
}

// =====================================================================
//  十五夜（お月見）
//   大きな満月にうさぎのかげ、手前にお団子とススキ
// =====================================================================
public sealed class JugoyaPlugin : QuickShapePlugin
{
    public override string Name => "演出：十五夜（お月見）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new JugoyaParameter(sharedData);
}

public sealed class JugoyaParameter : QuickShapeParameter
{
    public JugoyaParameter() : this(null) { }
    public JugoyaParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFF3B0);
        Color2 = Hex(0x0E1A3A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "夜空の背景", Group: "「十五夜（お月見）」の設定"),
        ["Color1"] = new ParamGuide(Label: "月の色"),
        ["Color2"] = new ParamGuide(Label: "夜空の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        if (Background) Sky(dc, w, h, C2(a), Col(Hex(0x24345E), a));
        var moon = o + new Vector2(150f * u, -150f * u);
        DrawKit.Glow(dc, moon, 500f * u, C1(0.4f * a));
        Dot(dc, b, moon, 250f * u, C1(a));
        var rb = moon + new Vector2(-20f * u, 40f * u);
        Oval(dc, b, rb, 70f * u, 50f * u, Rgb(0.85f, 0.75f, 0.45f, 0.6f * a));
        Oval(dc, b, rb + new Vector2(-70f, -50f) * u, 34f * u, 30f * u, Rgb(0.85f, 0.75f, 0.45f, 0.6f * a));
        Oval(dc, b, rb + new Vector2(-80f, -110f) * u, 10f * u, 40f * u, Rgb(0.85f, 0.75f, 0.45f, 0.6f * a));
        Oval(dc, b, rb + new Vector2(-60f, -110f) * u, 10f * u, 40f * u, Rgb(0.85f, 0.75f, 0.45f, 0.6f * a));
        var base0 = o + new Vector2(-350f * u, 330f * u);
        Poly(dc, b, Pts(base0, u, u, -150f, 0f, 150f, 0f, 120f, 60f, -120f, 60f), Rgb(0.75f, 0.55f, 0.3f, a));
        Box(dc, b, base0.X - 160f * u, base0.Y - 10f * u, base0.X + 160f * u, base0.Y + 4f * u, Rgb(1, 1, 1, a));
        int[] rowsN = { 3, 2, 1 };
        for (int r = 0; r < 3; r++) for (int k = 0; k < rowsN[r]; k++) Dot(dc, b, base0 + new Vector2((k - (rowsN[r] - 1) / 2f) * 80f * u, -50f * u - r * 66f * u), 42f * u, Rgb(1f, 0.98f, 0.93f, a));
        for (int i = 0; i < 5; i++)
        {
            var st = o + new Vector2(350f * u + i * 30f * u, 380f * u);
            float sway = MathF.Sin(t * 1.2f + i) * 20f * u;
            var tip = st + new Vector2(-60f * u + i * 20f * u + sway, -520f * u + i * 30f * u);
            Ln(dc, b, st, tip, Rgb(0.7f, 0.6f, 0.4f, a), 4f * u);
            for (int k = 0; k < 6; k++) Ln(dc, b, Vector2.Lerp(st, tip, 0.8f + k * 0.035f), Vector2.Lerp(st, tip, 0.8f + k * 0.035f) + new Vector2(30f, 20f) * u, Rgb(0.85f, 0.75f, 0.55f, a), 3f * u);
        }
    }
}

// =====================================================================
//  バレンタイン
//   リボンをかけたハートの箱。まわりにハートがふわふわ。「文字」でメッセージ
// =====================================================================
public sealed class ValentinePlugin : QuickShapePlugin
{
    public override string Name => "演出：バレンタイン";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ValentineParameter(sharedData);
}

public sealed class ValentineParameter : QuickShapeParameter
{
    public ValentineParameter() : this(null) { }
    public ValentineParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "Happy Valentine";
        Count = 12;
        Color1 = Hex(0x8D2B3A);
        Color2 = Hex(0xFF6F91);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "メッセージ", Group: "「バレンタイン」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "ハートの数", Min: 0, Max: 40, Group: "「バレンタイン」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "箱の色"),
        ["Color2"] = new ParamGuide(Label: "リボンの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.4f));
        Heart(dc, b, o + new Vector2(10f, 16f) * u, 220f * u * k, 0f, Rgb(0, 0, 0, 0.3f * a));
        Heart(dc, b, o, 220f * u * k, 0f, C1(a));
        Heart(dc, b, o, 180f * u * k, 0f, Col(Gfx.Mix(Color1, Hex(0x4E1A22), 0.3f), a));
        Box(dc, b, o.X - 18f * u * k, o.Y - 230f * u * k, o.X + 18f * u * k, o.Y + 230f * u * k, C2(a));
        for (int e = -1; e <= 1; e += 2) Poly(dc, b, Pts(o + new Vector2(0, -200f * u * k), u * k, u * k, 0f, 0f, e * 90f, -60f, e * 90f, 30f), C2(a));
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 0.3f + DrawKit.H(Seed, i), 1f);
            var q = o + new Vector2((DrawKit.H(Seed, i, 1) - 0.5f) * 1200f * u + MathF.Sin(t * 2f + i) * 30f * u, 400f * u - ph * 900f * u);
            Heart(dc, b, q, (20f + 20f * DrawKit.H(Seed, i, 2)) * u, MathF.Sin(t + i) * 0.3f, C2(a * MathF.Sin(ph * MathF.PI)));
        }
        Say(dc, b, Str, 80f * u, o + new Vector2(0, 330f * u), Rgb(1, 1, 1, a), C2(a), 5f * u);
    }
}

// =====================================================================
//  卒業・入学（校門と桜）
//   校門と「卒業式」などの看板(文字)。桜の花びらが舞う
// =====================================================================
public sealed class GraduationEntrancePlugin : QuickShapePlugin
{
    public override string Name => "演出：卒業・入学（校門と桜）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GraduationEntranceParameter(sharedData);
}

public sealed class GraduationEntranceParameter : QuickShapeParameter
{
    public GraduationEntranceParameter() : this(null) { }
    public GraduationEntranceParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "卒業式";
        Count = 40;
        Color1 = Hex(0xFFC1D6);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "看板の文字", Tip: "縦に書かれます(8文字まで)", Group: "「卒業・入学（校門と桜）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "花びらの数", Min: 0, Max: 150, Group: "「卒業・入学（校門と桜）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "空の背景", Group: "「卒業・入学（校門と桜）」の設定"),
        ["Color1"] = new ParamGuide(Label: "花びらの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        if (Background) Sky(dc, w, h, Col(Hex(0x8FD3FF), a), Col(Hex(0xE8F6FF), a));
        var g = o + new Vector2(0, 200f * u);
        for (int e = -1; e <= 1; e += 2)
        {
            Box(dc, b, g.X + e * 420f * u - 50f * u, g.Y - 400f * u, g.X + e * 420f * u + 50f * u, g.Y + 200f * u, Rgb(0.72f, 0.7f, 0.66f, a));
            Box(dc, b, g.X + e * 420f * u - 62f * u, g.Y - 420f * u, g.X + e * 420f * u + 62f * u, g.Y - 390f * u, Rgb(0.6f, 0.58f, 0.55f, a));
        }
        var sign = g + new Vector2(-420f * u, -150f * u);
        Box(dc, b, sign.X - 32f * u, sign.Y - 220f * u, sign.X + 32f * u, sign.Y + 220f * u, Rgb(1, 1, 1, a));
        Frame(dc, b, sign.X - 32f * u, sign.Y - 220f * u, sign.X + 32f * u, sign.Y + 220f * u, Rgb(0.2f, 0.2f, 0.2f, a), 3f * u);
        string s = Str;
        for (int i = 0; i < s.Length && i < 8; i++) Say(dc, b, s[i].ToString(), 50f * u, sign + new Vector2(0, -220f * u + 440f * u * (i + 0.5f) / Math.Max(1, Math.Min(8, s.Length))), Rgb(0.1f, 0.1f, 0.1f, a), bold: false);
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 0.12f * Spd(c) + DrawKit.H(Seed, i), 1f);
            var q = new Vector2(DrawKit.H(Seed, i, 1) * w + ph * 400f * U0(c) + MathF.Sin(t * 2f + i) * 30f * U0(c) - 200f * U0(c), -20f + ph * (h + 40f));
            Poly(dc, b, DrawKit.Place(Pts(Vector2.Zero, 12f * U0(c), 8f * U0(c), 0f, -1f, 1f, 0f, 0f, 1f, -1f, 0f), q, t * 2f + i), C1(a));
        }
    }
}

// =====================================================================
//  誕生日ケーキ
//   ろうそくの火がゆれるケーキ。「数」でろうそくの本数、バリエーション2で火が消える
// =====================================================================
public sealed class BirthdayCakePlugin : QuickShapePlugin
{
    public override string Name => "演出：誕生日ケーキ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BirthdayCakeParameter(sharedData);
}

public sealed class BirthdayCakeParameter : QuickShapeParameter
{
    public BirthdayCakeParameter() : this(null) { }
    public BirthdayCakeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 5;
        Text = "HAPPY BIRTHDAY";
        Color1 = Hex(0xFFF3E0);
        Color2 = Hex(0xFF80AB);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "メッセージ", Group: "「誕生日ケーキ」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "ろうそくの数", Min: 1, Max: 20, Group: "「誕生日ケーキ」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=火がゆれる　2=火が消える", Min: 1, Max: 2, Group: "「誕生日ケーキ」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ケーキの色"),
        ["Color2"] = new ParamGuide(Label: "クリームと文字の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var cb = o + new Vector2(0, 120f * u);
        Oval(dc, b, cb + new Vector2(0, 200f * u), 420f * u, 60f * u, Rgb(0.9f, 0.9f, 0.95f, a));
        Box(dc, b, cb.X - 340f * u, cb.Y, cb.X + 340f * u, cb.Y + 200f * u, C1(a));
        Oval(dc, b, cb + new Vector2(0, 200f * u), 340f * u, 50f * u, C1(a));
        Box(dc, b, cb.X - 340f * u, cb.Y + 80f * u, cb.X + 340f * u, cb.Y + 110f * u, C2(a));
        Oval(dc, b, cb, 340f * u, 50f * u, Rgb(1, 1, 1, a));
        for (int i = 0; i < 8; i++) { var sp = cb + new Vector2(-300f * u + i * 86f * u, MathF.Sin(i) * 10f * u); Dot(dc, b, sp, 22f * u, Rgb(0.9f, 0.1f, 0.2f, a)); }
        int n = Math.Clamp(Count, 1, 20);
        bool off = Variant >= 2 && c.Time > 1.5f;
        for (int i = 0; i < n; i++)
        {
            float x = cb.X - 260f * u + 520f * u * (n == 1 ? 0.5f : i / (float)(n - 1));
            float y = cb.Y - 10f * u + MathF.Abs(i - (n - 1) / 2f) * 4f * u;
            Box(dc, b, x - 9f * u, y - 120f * u, x + 9f * u, y, Col(BubbleKit.Palette(0, i), a));
            var fp = new Vector2(x + MathF.Sin(t * 9f + i) * 3f * u, y - 150f * u);
            if (off) { float sm = c.Time - 1.5f; Dot(dc, b, fp - new Vector2(0, sm * 60f * u), 10f * u + sm * 20f * u, Rgb(0.7f, 0.7f, 0.7f, MathF.Max(0f, 0.6f - sm * 0.4f) * a)); continue; }
            DrawKit.Glow(dc, fp, 60f * u, Rgb(1f, 0.8f, 0.3f, 0.5f * a));
            Poly(dc, b, Pts(fp, 14f * u, 28f * u * (1f + 0.1f * MathF.Sin(t * 13f + i)), 0f, -1f, 0.7f, 0.3f, 0f, 1f, -0.7f, 0.3f), Rgb(1f, 0.75f, 0.2f, a));
            Dot(dc, b, fp + new Vector2(0, 8f * u), 6f * u, Rgb(1, 1, 0.9f, a));
        }
        Say(dc, b, Str, 80f * u, o + new Vector2(0, -330f * u), C2(a), Rgb(1, 1, 1, a), 5f * u);
    }
}

// =====================================================================
//  結婚式（チャペルの鐘）
//   金色の鐘がカランカランとゆれ、白いハトとハートの花びらが舞う
// =====================================================================
public sealed class WeddingPlugin : QuickShapePlugin
{
    public override string Name => "演出：結婚式（チャペルの鐘）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WeddingParameter(sharedData);
}

public sealed class WeddingParameter : QuickShapeParameter
{
    public WeddingParameter() : this(null) { }
    public WeddingParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 30;
        Text = "Happy Wedding";
        Color1 = Hex(0xE9C46A);
        Color2 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "メッセージ", Group: "「結婚式（チャペルの鐘）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "花びらの数", Min: 0, Max: 80, Group: "「結婚式（チャペルの鐘）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "鐘の色"),
        ["Color2"] = new ParamGuide(Label: "ハトと花びらの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        for (int e = -1; e <= 1; e += 2)
        {
            float sw = MathF.Sin(t * 3f * Spd(c) + (e > 0 ? MathF.PI : 0f)) * 0.35f;
            var pivot = o + new Vector2(e * 120f * u, -280f * u);
            var bell = DrawKit.Place(Pts(Vector2.Zero, u, u, -20f, 0f, 20f, 0f, 50f, 60f, 70f, 170f, 110f, 210f, -110f, 210f, -70f, 170f, -50f, 60f), pivot, sw);
            Poly(dc, b, bell, C1(a));
            Dot(dc, b, DrawKit.Rot(pivot + new Vector2(0, 225f * u), pivot, sw * 1.3f), 22f * u, Col(Gfx.Mix(Color1, Hex(0x7A5A10), 0.4f), a));
        }
        Poly(dc, b, Pts(o + new Vector2(0, -290f * u), u, u, -60f, 0f, 60f, 0f, 0f, 30f), Rgb(0.95f, 0.5f, 0.6f, a));
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 0.15f + DrawKit.H(Seed, i), 1f);
            var q = o + new Vector2((DrawKit.H(Seed, i, 1) - 0.5f) * 1500f * u + MathF.Sin(t * 1.5f + i) * 40f * u, -500f * u + ph * 1000f * u);
            if (i % 3 == 0) Heart(dc, b, q, 16f * u, t + i, Rgb(1f, 0.7f, 0.8f, a));
            else Poly(dc, b, DrawKit.Place(Pts(Vector2.Zero, 10f * u, 7f * u, 0f, -1f, 1f, 0f, 0f, 1f, -1f, 0f), q, t * 2f + i), C2(0.9f * a));
        }
        for (int i = 0; i < 2; i++)
        {
            float f = Gfx.Mod(t * 0.2f + i * 0.5f, 1f);
            var q = o + new Vector2(-900f * u + f * 1800f * u, -100f * u - i * 120f * u + MathF.Sin(f * 6f) * 40f * u);
            float fl = MathF.Sin(t * 14f + i) * 0.6f;
            Oval(dc, b, q, 34f * u, 18f * u, C2(a));
            Poly(dc, b, Pts(q, u, u, -10f, 0f, 30f, -50f * fl - 10f, 40f, -40f * fl), C2(a));
            Dot(dc, b, q + new Vector2(32f, -8f) * u, 12f * u, C2(a));
        }
        Say(dc, b, Str, 90f * u, o + new Vector2(0, 280f * u), C1(a), Rgb(1, 1, 1, a), 4f * u);
    }
}

// =====================================================================
//  瑞雲と金屏風
//   金屏風の背景に、めでたい雲(瑞雲)がゆっくり流れる。お正月や祝いの場面に
// =====================================================================
public sealed class GoldScreenCloudPlugin : QuickShapePlugin
{
    public override string Name => "演出：瑞雲と金屏風";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GoldScreenCloudParameter(sharedData);
}

public sealed class GoldScreenCloudParameter : QuickShapeParameter
{
    public GoldScreenCloudParameter() : this(null) { }
    public GoldScreenCloudParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 6;
        Color1 = Hex(0xD9A93B);
        Color2 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "雲の数", Min: 1, Max: 20, Group: "「瑞雲と金屏風」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "金屏風の色"),
        ["Color2"] = new ParamGuide(Label: "雲の色"),
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
        int panels = 6;
        for (int i = 0; i < panels; i++)
        {
            float x0 = w * i / panels, x1 = w * (i + 1) / panels;
            Shade(dc, new Vector2(x0, 0), new Vector2(x1, 0), Col(Gfx.Mix(Color1, Hex(0xFFF2B0), i % 2 == 0 ? 0.25f : 0f), a), Col(Gfx.Mix(Color1, Hex(0x8A6418), i % 2 == 0 ? 0f : 0.25f), a), x0, 0, x1, h);
            Ln(dc, b, new Vector2(x0, 0), new Vector2(x0, h), Rgb(0.4f, 0.28f, 0.05f, 0.5f * a), 3f * s);
        }
        for (int i = 0; i < 10; i++) for (int k = 0; k < 6; k++) Box(dc, b, w * i / 10f + 10f * s, h * k / 6f + 10f * s, w * (i + 1) / 10f - 10f * s, h * (k + 1) / 6f - 10f * s, Rgb(1, 0.95f, 0.7f, 0.05f * a));
        for (int i = 0; i < Count; i++)
        {
            var q = new Vector2(Gfx.Mod(DrawKit.H(Seed, i) * w + t * 30f * s * Spd(c), w + 600f * s) - 300f * s, h * (0.15f + 0.7f * DrawKit.H(Seed, i, 1)));
            float r = (50f + 40f * DrawKit.H(Seed, i, 2)) * s;
            for (int k = 0; k < 5; k++)
            {
                var cp = q + new Vector2((k - 2) * r * 1.1f, MathF.Abs(k - 2) * r * 0.25f);
                Dot(dc, b, cp, r * (1f - MathF.Abs(k - 2) * 0.15f), C2(0.9f * a));
                Path(dc, b, Gfx.ArcPts(cp, r * 0.6f * (1f - MathF.Abs(k - 2) * 0.15f), MathF.PI, MathF.PI * 2.4f, 14), Col(Color1, 0.6f * a), 3f * s);
            }
            Box(dc, b, q.X - r * 3f, q.Y + r * 0.5f, q.X + r * 3.5f, q.Y + r * 0.8f, C2(0.9f * a));
        }
    }
}

// =====================================================================
//  鳥居と灯籠の参道
//   奥へ続く参道に、朱色の鳥居がいくつも並び、両わきの灯籠に火がともる
// =====================================================================
public sealed class ToriiPathPlugin : QuickShapePlugin
{
    public override string Name => "演出：鳥居と灯籠の参道";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ToriiPathParameter(sharedData);
}

public sealed class ToriiPathParameter : QuickShapeParameter
{
    public ToriiPathParameter() : this(null) { }
    public ToriiPathParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 5;
        Color1 = Hex(0xD84315);
        Color2 = Hex(0x1B1F3B);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "鳥居の数", Min: 1, Max: 12, Group: "「鳥居と灯籠の参道」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=止まっている　2=奥へ進む", Min: 1, Max: 2, Group: "「鳥居と灯籠の参道」の設定"),
        ["Background"] = new ParamGuide(Label: "夜空の背景", Group: "「鳥居と灯籠の参道」の設定"),
        ["Color1"] = new ParamGuide(Label: "鳥居の色"),
        ["Color2"] = new ParamGuide(Label: "空の色"),
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
        if (Background) Sky(dc, w, h, C2(a), Col(Hex(0x4A3A5A), a));
        var vp = new Vector2(w / 2f, h * 0.42f);
        Poly(dc, b, new List<Vector2> { new(w * 0.3f, h), new(w * 0.7f, h), vp + new Vector2(10f, 0), vp - new Vector2(10f, 0) }, Rgb(0.55f, 0.5f, 0.45f, a));
        for (int i = Count - 1; i >= 0; i--)
        {
            float z = (i + 1f - Gfx.Mod(t * 0.2f * Spd(c), 1f) * (Variant >= 2 ? 1f : 0f)) / Count;
            float k = 1f / (0.4f + z * 3f);
            var bp = vp + new Vector2(0, (h - vp.Y) * k);
            float tw = 700f * s * k, th = 600f * s * k, pw = 40f * s * k;
            var col = C1(a * MathF.Min(1f, k * 2f));
            Box(dc, b, bp.X - tw * 0.38f - pw / 2f, bp.Y - th, bp.X - tw * 0.38f + pw / 2f, bp.Y, col);
            Box(dc, b, bp.X + tw * 0.38f - pw / 2f, bp.Y - th, bp.X + tw * 0.38f + pw / 2f, bp.Y, col);
            Poly(dc, b, new List<Vector2> { new(bp.X - tw * 0.55f, bp.Y - th - 30f * s * k), new(bp.X + tw * 0.55f, bp.Y - th - 30f * s * k), new(bp.X + tw * 0.5f, bp.Y - th + 15f * s * k), new(bp.X - tw * 0.5f, bp.Y - th + 15f * s * k) }, Rgb(0.1f, 0.08f, 0.08f, a));
            Box(dc, b, bp.X - tw * 0.46f, bp.Y - th * 0.82f, bp.X + tw * 0.46f, bp.Y - th * 0.82f + pw * 0.7f, col);
            for (int e = -1; e <= 1; e += 2)
            {
                var lp = bp + new Vector2(e * tw * 0.62f, 0);
                Box(dc, b, lp.X - 12f * s * k, lp.Y - 150f * s * k, lp.X + 12f * s * k, lp.Y, Rgb(0.45f, 0.43f, 0.4f, a));
                Box(dc, b, lp.X - 34f * s * k, lp.Y - 220f * s * k, lp.X + 34f * s * k, lp.Y - 150f * s * k, Rgb(0.5f, 0.48f, 0.45f, a));
                DrawKit.Glow(dc, lp - new Vector2(0, 185f * s * k), 90f * s * k, Rgb(1f, 0.7f, 0.3f, 0.6f * a * (0.85f + 0.15f * MathF.Sin(t * 6f + i))));
            }
        }
    }
}
