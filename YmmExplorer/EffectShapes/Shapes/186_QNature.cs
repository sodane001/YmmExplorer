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
//  雨粒が地面ではねる
//   画面の下(「値」の高さ)に雨が当たって、小さな王冠や波紋ができる。「数」で量
// =====================================================================
public sealed class RainSplashPlugin : QuickShapePlugin
{
    public override string Name => "演出：雨粒が地面ではねる";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RainSplashParameter(sharedData);
}

public sealed class RainSplashParameter : QuickShapeParameter
{
    public RainSplashParameter() : this(null) { }
    public RainSplashParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 60;
        Color1 = Hex(0xBFD9FF);
    }
    protected override double DefAmount => 85;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "地面の高さ", Tip: "画面の上から何%か", Group: "「雨粒が地面ではねる」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "雨の量", Min: 5, Max: 300, Group: "「雨粒が地面ではねる」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "雨の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
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
        float gy = h * Amt(c);
        for (int i = 0; i < Count; i++)
        {
            float per = 0.6f + DrawKit.H(Seed, i) * 0.5f;
            float ph = Gfx.Mod(t * Spd(c) / per + DrawKit.H(Seed, i, 1), 1f);
            int cyc = (int)MathF.Floor(t * Spd(c) / per + DrawKit.H(Seed, i, 1));
            float x = DrawKit.H(Seed + cyc, i, 2) * w;
            float gy2 = gy + (DrawKit.H(Seed, i, 3) - 0.5f) * 60f * U0(c);
            if (ph < 0.7f)
            {
                float y = gy2 * (ph / 0.7f);
                Ln(dc, b, new Vector2(x, y - 40f * U0(c)), new Vector2(x, y), C1(0.5f * a), 2f * U0(c));
            }
            else
            {
                float f = (ph - 0.7f) / 0.3f;
                Oval(dc, b, new Vector2(x, gy2), 0.1f + 40f * U0(c) * f, 0.1f + 10f * U0(c) * f, C1(0.25f * (1f - f) * a));
                for (int k = -1; k <= 1; k++) Dot(dc, b, new Vector2(x + k * 16f * U0(c) * f, gy2 - MathF.Sin(f * MathF.PI) * 20f * U0(c)), 3f * U0(c), C1((1f - f) * a));
            }
        }
    }
}

// =====================================================================
//  霜が広がる
//   画面のふちから、白い霜の模様がピキピキと広がる。「値」でどこまで広がるか
// =====================================================================
public sealed class FrostSpreadPlugin : QuickShapePlugin
{
    public override string Name => "演出：霜が広がる";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FrostSpreadParameter(sharedData);
}

public sealed class FrostSpreadParameter : QuickShapeParameter
{
    public FrostSpreadParameter() : this(null) { }
    public FrostSpreadParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xE3F4FF);
        Count = 24;
    }
    protected override double DefAmount => 60;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "広がる範囲", Group: "「霜が広がる」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "霜の筋の数", Min: 4, Max: 80, Group: "「霜が広がる」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "霜の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
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
        float p = Amt(c) * DrawKit.EaseOut(Intro(c, 0f, 2.5f));
        float reach = MathF.Min(w, h) * 0.5f * p;
        Shade(dc, new Vector2(0, 0), new Vector2(0, reach), C1(0.35f * a), C1(0f), 0, 0, w, reach);
        Shade(dc, new Vector2(0, h), new Vector2(0, h - reach), C1(0.35f * a), C1(0f), 0, h - reach, w, h);
        for (int i = 0; i < Count; i++)
        {
            float f = DrawKit.H(Seed, i);
            int edge = i % 4;
            var st = edge == 0 ? new Vector2(f * w, 0) : edge == 1 ? new Vector2(w, f * h) : edge == 2 ? new Vector2(f * w, h) : new Vector2(0, f * h);
            var inward = edge == 0 ? new Vector2(0, 1) : edge == 1 ? new Vector2(-1, 0) : edge == 2 ? new Vector2(0, -1) : new Vector2(1, 0);
            float len = reach * (0.5f + 0.5f * DrawKit.H(Seed, i, 1));
            var en = st + inward * len;
            Ln(dc, b, st, en, C1(0.8f * a), 3f * U0(c));
            for (int k = 1; k < 6; k++)
            {
                var bp = Vector2.Lerp(st, en, k / 6f);
                float bl = len * 0.25f * (1f - k / 7f);
                var side = new Vector2(-inward.Y, inward.X);
                Ln(dc, b, bp, bp + (inward + side) * 0.7f * bl, C1(0.7f * a), 2f * U0(c));
                Ln(dc, b, bp, bp + (inward - side) * 0.7f * bl, C1(0.7f * a), 2f * U0(c));
            }
        }
    }
}

// =====================================================================
//  つらら
//   画面の上(「値」の高さ)から、つららが垂れて、先から水がポタッと落ちる
// =====================================================================
public sealed class IciclesPlugin : QuickShapePlugin
{
    public override string Name => "演出：つらら";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new IciclesParameter(sharedData);
}

public sealed class IciclesParameter : QuickShapeParameter
{
    public IciclesParameter() : this(null) { }
    public IciclesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 16;
        Color1 = Hex(0xD6F0FF);
    }
    protected override double DefAmount => 0;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "付け根の高さ", Tip: "画面の上から何%か", Group: "「つらら」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "つららの数", Min: 2, Max: 40, Group: "「つらら」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "つららの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
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
        float top = h * Amt(c);
        float grow = DrawKit.EaseOut(Intro(c, 0f, 1.5f));
        Box(dc, b, 0, top - 4f, w, top + 24f * U0(c), Rgb(1, 1, 1, 0.9f * a));
        for (int i = 0; i < Count; i++)
        {
            float x = w * (i + 0.3f + DrawKit.H(Seed, i) * 0.4f) / Count;
            float len = (80f + 260f * DrawKit.H(Seed, i, 1)) * U0(c) * grow, ww = (18f + 22f * DrawKit.H(Seed, i, 2)) * U0(c);
            var tip = new Vector2(x, top + 20f * U0(c) + len);
            Poly(dc, b, new List<Vector2> { new(x - ww, top + 20f * U0(c)), new(x + ww, top + 20f * U0(c)), new(x + ww * 0.2f, top + 20f * U0(c) + len * 0.6f), tip }, C1(0.8f * a));
            Ln(dc, b, new Vector2(x - ww * 0.4f, top + 26f * U0(c)), new Vector2(x - ww * 0.05f, tip.Y - len * 0.2f), Rgb(1, 1, 1, 0.7f * a), 3f * U0(c));
            float ph = Gfx.Mod(t * 0.5f + DrawKit.H(Seed, i, 3), 1f);
            if (ph > 0.6f) { float f = (ph - 0.6f) / 0.4f; Dot(dc, b, tip + new Vector2(0, f * f * h * 0.8f), 5f * U0(c), C1(a)); }
            else Dot(dc, b, tip, 5f * U0(c) * ph / 0.6f, C1(a));
        }
    }
}

// =====================================================================
//  雨雲レーダー
//   地図風の背景に、緑→黄→赤の雨雲のかたまりが流れていく
// =====================================================================
public sealed class RainRadarPlugin : QuickShapePlugin
{
    public override string Name => "演出：雨雲レーダー";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RainRadarParameter(sharedData);
}

public sealed class RainRadarParameter : QuickShapeParameter
{
    public RainRadarParameter() : this(null) { }
    public RainRadarParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 6;
        Color1 = Hex(0x2E7D32);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "雨雲の数", Min: 1, Max: 20, Group: "「雨雲レーダー」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "地図の背景", Group: "「雨雲レーダー」の設定"),
        ["Color1"] = new ParamGuide(Hidden: true),
        ["Color2"] = new ParamGuide(Hidden: true),
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
        if (Background) { Box(dc, b, 0, 0, w, h, Rgb(0.82f, 0.86f, 0.9f, a)); Poly(dc, b, Pts(new Vector2(w * 0.5f, h * 0.5f), w, h, -0.2f, -0.45f, 0.1f, -0.3f, 0.25f, 0.0f, 0.15f, 0.35f, -0.1f, 0.45f, -0.3f, 0.1f), Rgb(0.93f, 0.92f, 0.86f, a)); }
        float s = U0(c);
        for (int i = 0; i < Count; i++)
        {
            var q = new Vector2(Gfx.Mod(DrawKit.H(Seed, i) * w + t * 60f * s * Spd(c), w + 600f * s) - 300f * s, DrawKit.H(Seed, i, 1) * h);
            float r = (120f + 160f * DrawKit.H(Seed, i, 2)) * s;
            var lv = new[] { (1f, Rgb(0.3f, 0.7f, 1f, 0.45f * a)), (0.7f, Rgb(0.2f, 0.75f, 0.3f, 0.6f * a)), (0.45f, Rgb(1f, 0.85f, 0.1f, 0.7f * a)), (0.22f, Rgb(0.95f, 0.15f, 0.1f, 0.8f * a)) };
            foreach (var (k, col) in lv) Poly(dc, b, BubbleKit.CloudPts(q, r * k * 1.3f, r * k, 7, 0.22f), col);
        }
    }
}

// =====================================================================
//  日の出・日の入り
//   地平線から太陽が上る(バリエーション2で沈む)。空の色も夜→朝に変わる
// =====================================================================
public sealed class SunriseSunsetPlugin : QuickShapePlugin
{
    public override string Name => "演出：日の出・日の入り";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SunriseSunsetParameter(sharedData);
}

public sealed class SunriseSunsetParameter : QuickShapeParameter
{
    public SunriseSunsetParameter() : this(null) { }
    public SunriseSunsetParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF8A3D);
        Color2 = Hex(0x14213D);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=日の出　2=日の入り", Min: 1, Max: 2, Group: "「日の出・日の入り」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "太陽・夕焼けの色"),
        ["Color2"] = new ParamGuide(Label: "夜空の色"),
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
        float p = Intro(c, 0f, MathF.Max(0.5f, (float)c.Length / MathF.Max(1, c.Fps)));
        if (Variant >= 2) p = 1f - p;
        float hz = h * 0.7f;
        var top = Gfx.Mix(Color2, Hex(0x6EC6FF), p);
        var bot = Gfx.Mix(Hex(0x3A2A5C), Color1, MathF.Min(1f, p * 1.6f));
        if (p > 0.7f) bot = Gfx.Mix(Color1, Hex(0xBFE6FF), (p - 0.7f) / 0.3f);
        Sky(dc, w, hz, Col(top, a), Col(bot, a));
        var sun = new Vector2(w * 0.5f, hz + 120f * U0(c) - p * h * 0.55f);
        DrawKit.Glow(dc, sun, 500f * U0(c), Col(Color1, 0.5f * a));
        Dot(dc, b, sun, 110f * U0(c), Col(Gfx.Mix(Hex(0xFF5722), Hex(0xFFF3C4), p), a));
        Shade(dc, new Vector2(0, hz), new Vector2(0, h), Col(Gfx.Mix(Hex(0x1B1B2F), Hex(0x2E5E3E), p), a), Col(Gfx.Mix(Hex(0x0B0B16), Hex(0x1E3D28), p), a), 0, hz, w, h);
        for (int i = 0; i < 6; i++) { float y = hz + 20f * U0(c) + i * 30f * U0(c); float ww = (260f - i * 30f) * U0(c); Box(dc, b, sun.X - ww, y, sun.X + ww, y + 6f * U0(c), Col(Color1, 0.35f * a * (1f - i / 6f) * MathF.Min(1f, p * 2f))); }
    }
}

// =====================================================================
//  雲海
//   山の頂上から見下ろす雲の海。雲がゆっくり流れ、遠くに山の頭が出る
// =====================================================================
public sealed class SeaOfCloudsPlugin : QuickShapePlugin
{
    public override string Name => "演出：雲海";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SeaOfCloudsParameter(sharedData);
}

public sealed class SeaOfCloudsParameter : QuickShapeParameter
{
    public SeaOfCloudsParameter() : this(null) { }
    public SeaOfCloudsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFFFFF);
        Color2 = Hex(0xFFB37A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "空の背景", Group: "「雲海」の設定"),
        ["Color1"] = new ParamGuide(Label: "雲の色"),
        ["Color2"] = new ParamGuide(Label: "朝焼けの色"),
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
        if (Background) Sky(dc, w, h, Col(Hex(0x5B8BD9), a), C2(a));
        float s = U0(c);
        for (int i = 0; i < 3; i++)
        {
            float px = w * (0.2f + i * 0.33f), ph = h * (0.45f + i * 0.04f);
            Poly(dc, b, new List<Vector2> { new(px - 300f * s, h * 0.65f), new(px, ph), new(px + 340f * s, h * 0.65f) }, Rgb(0.25f + i * 0.05f, 0.28f + i * 0.05f, 0.38f, a));
            Poly(dc, b, new List<Vector2> { new(px - 60f * s, ph + 50f * s), new(px, ph), new(px + 70f * s, ph + 55f * s) }, Rgb(1, 1, 1, 0.85f * a));
        }
        for (int layer = 0; layer < 4; layer++)
        {
            float y = h * (0.58f + layer * 0.1f);
            float spd = (10f + layer * 14f) * s * Spd(c);
            var pts = new List<Vector2> { new(-10, h + 10) };
            for (int i = 0; i <= 40; i++)
            {
                float x = w * i / 40f;
                pts.Add(new Vector2(x, y - (MathF.Sin((x + t * spd) * 0.006f / s + layer) * 0.5f + 0.5f) * 50f * s - MathF.Abs(MathF.Sin((x + t * spd) * 0.013f / s + layer * 2f)) * 30f * s));
            }
            pts.Add(new Vector2(w + 10, h + 10));
            Poly(dc, b, pts, Col(Gfx.Mix(Color2, Color1, 0.5f + layer * 0.17f), (0.8f + layer * 0.05f) * a));
        }
    }
}

// =====================================================================
//  ススキ・稲穂
//   画面の下に、ススキ(バリエーション2は稲穂)が並んで風にゆれる。「数」で本数
// =====================================================================
public sealed class SusukiFieldPlugin : QuickShapePlugin
{
    public override string Name => "演出：ススキ・稲穂";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SusukiFieldParameter(sharedData);
}

public sealed class SusukiFieldParameter : QuickShapeParameter
{
    public SusukiFieldParameter() : this(null) { }
    public SusukiFieldParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 40;
        Color1 = Hex(0xC9B38A);
        Color2 = Hex(0x6B7B3A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "本数", Min: 5, Max: 120, Group: "「ススキ・稲穂」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=ススキ　2=稲穂", Min: 1, Max: 2, Group: "「ススキ・稲穂」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "穂の色"),
        ["Color2"] = new ParamGuide(Label: "茎の色"),
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
        bool rice = Variant >= 2;
        for (int i = 0; i < Count; i++)
        {
            float x = w * (i + DrawKit.H(Seed, i)) / Count;
            float hh = h * (0.25f + 0.25f * DrawKit.H(Seed, i, 1));
            float sway = MathF.Sin(t * 1.5f * Spd(c) - x * 0.004f / s) * 40f * s + 20f * s;
            var stem = new List<Vector2>();
            for (int k = 0; k <= 10; k++) { float f = k / 10f; stem.Add(new Vector2(x + sway * f * f, h - hh * f)); }
            Path(dc, b, stem, C2(a), 3f * s);
            var tip = stem[^1];
            var dir = Vector2.Normalize(stem[^1] - stem[^2]);
            if (rice)
            {
                var droop = new Vector2(dir.X + 0.9f, dir.Y + 1.2f);
                for (int k = 0; k < 8; k++) { var gp = tip + droop * k * 10f * s; Oval(dc, b, gp, 7f * s, 4f * s, Rgb(0.9f, 0.75f, 0.3f, a)); }
            }
            else
            {
                for (int k = 0; k < 10; k++)
                {
                    var bp = tip - dir * k * 9f * s;
                    var side = new Vector2(-dir.Y, dir.X);
                    float l = (26f - k * 1.5f) * s;
                    Ln(dc, b, bp, bp + (side * (k % 2 == 0 ? 1f : -1f) + dir * 0.6f + new Vector2(0.5f, 0)) * l, C1(0.85f * a), 3f * s);
                }
            }
        }
    }
}

// =====================================================================
//  雪が積もる
//   雪が降りながら、画面の下に白い雪がだんだん積もっていく。「値」で最後の高さ
// =====================================================================
public sealed class SnowPilePlugin : QuickShapePlugin
{
    public override string Name => "演出：雪が積もる";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SnowPileParameter(sharedData);
}

public sealed class SnowPileParameter : QuickShapeParameter
{
    public SnowPileParameter() : this(null) { }
    public SnowPileParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 120;
        Color1 = Hex(0xFFFFFF);
    }
    protected override double DefAmount => 25;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "最後に積もる高さ", Group: "「雪が積もる」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "降る雪の数", Min: 0, Max: 400, Group: "「雪が積もる」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "雪の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
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
        float total = MathF.Max(1f, (float)c.Length / MathF.Max(1, c.Fps));
        float lv = Amt(c) * h * Math.Clamp(c.Time / total, 0f, 1f);
        for (int i = 0; i < Count; i++)
        {
            float sp = 80f + 80f * DrawKit.H(Seed, i);
            float y = Gfx.Mod(DrawKit.H(Seed, i, 1) * h + t * sp * s * Spd(c), h + 40f) - 20f;
            float x = DrawKit.H(Seed, i, 2) * w + MathF.Sin(t + i) * 20f * s;
            Dot(dc, b, new Vector2(x, y), (3f + 4f * DrawKit.H(Seed, i, 3)) * s, C1(0.9f * a));
        }
        var pts = new List<Vector2> { new(-10, h + 10) };
        for (int i = 0; i <= 30; i++) { float x = w * i / 30f; pts.Add(new Vector2(x, h - lv - MathF.Sin(i * 1.3f + Seed) * lv * 0.15f)); }
        pts.Add(new Vector2(w + 10, h + 10));
        Poly(dc, b, pts, C1(a));
    }
}

// =====================================================================
//  タンポポの綿毛
//   白い綿毛がふわふわ風に乗って、右上へ流れていく。「数」で量
// =====================================================================
public sealed class DandelionPlugin : QuickShapePlugin
{
    public override string Name => "演出：タンポポの綿毛";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DandelionParameter(sharedData);
}

public sealed class DandelionParameter : QuickShapeParameter
{
    public DandelionParameter() : this(null) { }
    public DandelionParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 25;
        Color1 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "綿毛の数", Min: 1, Max: 80, Group: "「タンポポの綿毛」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "綿毛の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
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
        for (int i = 0; i < Count; i++)
        {
            float sp = 0.05f + 0.06f * DrawKit.H(Seed, i);
            float f = Gfx.Mod(DrawKit.H(Seed, i, 1) + t * sp * Spd(c), 1f);
            var q = new Vector2(-100f + (w + 200f) * f, h * (0.3f + 0.7f * DrawKit.H(Seed, i, 2)) - f * h * 0.4f + MathF.Sin(t * 1.3f + i) * 40f * s);
            float r = (18f + 16f * DrawKit.H(Seed, i, 3)) * s;
            float rot = MathF.Sin(t + i) * 0.4f;
            var down = new Vector2(MathF.Sin(-rot), MathF.Cos(rot));
            Ln(dc, b, q, q + down * r * 1.4f, C1(0.8f * a), 1.5f * s);
            Dot(dc, b, q + down * r * 1.4f, 2.5f * s, Rgb(0.55f, 0.45f, 0.3f, a));
            for (int k = 0; k < 12; k++) { float an = MathF.PI + rot + k * MathF.PI / 11f; Ln(dc, b, q, Pol(q, r, an), C1(0.7f * a), 1.2f * s); Dot(dc, b, Pol(q, r, an), 2f * s, C1(0.8f * a)); }
        }
    }
}

// =====================================================================
//  あじさいと雨粒
//   青むらさきのあじさいの花のかたまりと葉っぱ。雨粒がしたたる。梅雨の場面に
// =====================================================================
public sealed class HydrangeaRainPlugin : QuickShapePlugin
{
    public override string Name => "演出：あじさいと雨粒";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HydrangeaRainParameter(sharedData);
}

public sealed class HydrangeaRainParameter : QuickShapeParameter
{
    public HydrangeaRainParameter() : this(null) { }
    public HydrangeaRainParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 3;
        Color1 = Hex(0x7E8CE0);
        Color2 = Hex(0xC08BDB);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "花のかたまりの数", Min: 1, Max: 6, Group: "「あじさいと雨粒」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "花の色1"),
        ["Color2"] = new ParamGuide(Label: "花の色2"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        for (int f = 0; f < Count; f++)
        {
            var q = o + new Vector2((f - (Count - 1) / 2f) * 320f * u, (f % 2) * 80f * u);
            for (int k = 0; k < 2; k++)
            {
                float side = k == 0 ? -1f : 1f;
                var lf = DrawKit.Place(Pts(Vector2.Zero, 120f * u, 60f * u, 0f, 0f, 0.6f, -0.6f, 1.4f, 0f, 0.6f, 0.6f), q + new Vector2(side * 60f * u, 120f * u), side > 0 ? -0.3f : MathF.PI + 0.3f);
                Poly(dc, b, lf, Rgb(0.2f, 0.45f, 0.25f, a));
            }
            for (int i = 0; i < 40; i++)
            {
                var pp = q + new Vector2((DrawKit.H(Seed + f, i) - 0.5f) * 260f * u, (DrawKit.H(Seed + f, i, 1) - 0.5f) * 200f * u);
                if (Vector2.Distance(pp, q) > 130f * u) continue;
                var col = Gfx.Mix(Color1, Color2, DrawKit.H(Seed + f, i, 2));
                for (int j = 0; j < 4; j++) Dot(dc, b, Pol(pp, 11f * u, j * MathF.PI / 2f + i), 10f * u, Col(col, 0.95f * a));
                Dot(dc, b, pp, 3f * u, Rgb(1, 1, 1, 0.7f * a));
            }
            float ph = Gfx.Mod(t * 0.7f + f * 0.37f, 1f);
            var dp = q + new Vector2(60f * u, 170f * u + ph * ph * 300f * u);
            Poly(dc, b, Pts(dp, 9f * u, 9f * u, 0f, -2f, 1f, 0f, 0f, 1f, -1f, 0f), Rgb(0.8f, 0.9f, 1f, a * (1f - ph)));
        }
    }
}

// =====================================================================
//  夕焼けとトンボ
//   オレンジの夕焼け空を、赤トンボが「数」匹すいすい飛ぶ
// =====================================================================
public sealed class SunsetDragonflyPlugin : QuickShapePlugin
{
    public override string Name => "演出：夕焼けとトンボ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SunsetDragonflyParameter(sharedData);
}

public sealed class SunsetDragonflyParameter : QuickShapeParameter
{
    public SunsetDragonflyParameter() : this(null) { }
    public SunsetDragonflyParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 4;
        Color1 = Hex(0xFF7043);
        Color2 = Hex(0xFFD180);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "トンボの数", Min: 1, Max: 12, Group: "「夕焼けとトンボ」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "夕焼けの背景", Group: "「夕焼けとトンボ」の設定"),
        ["Color1"] = new ParamGuide(Label: "空の上の色"),
        ["Color2"] = new ParamGuide(Label: "空の下の色"),
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
        if (Background) { Sky(dc, w, h, C1(a), C2(a)); Dot(dc, b, new Vector2(w * 0.7f, h * 0.75f), 140f * s, Rgb(1f, 0.85f, 0.6f, a)); Box(dc, b, 0, h * 0.85f, w, h, Rgb(0.25f, 0.15f, 0.12f, a)); }
        for (int i = 0; i < Count; i++)
        {
            float tt = t * 0.3f * Spd(c) + i * 1.7f;
            var q = new Vector2(w * (0.5f + 0.4f * MathF.Sin(tt * 0.9f + i)), h * (0.35f + 0.2f * MathF.Sin(tt * 1.3f + i * 2f)));
            var nx = new Vector2(w * (0.5f + 0.4f * MathF.Sin((tt + 0.01f) * 0.9f + i)), h * (0.35f + 0.2f * MathF.Sin((tt + 0.01f) * 1.3f + i * 2f)));
            float ang = MathF.Atan2(nx.Y - q.Y, nx.X - q.X);
            var body = DrawKit.Place(Pts(Vector2.Zero, 30f * s, 4f * s, -1.4f, -1f, 0.5f, -1.4f, 0.8f, 0f, 0.5f, 1.4f, -1.4f, 1f), q, ang);
            float flap = 0.6f + 0.4f * MathF.Abs(MathF.Sin(t * 40f + i));
            for (int k = -1; k <= 1; k += 2)
                for (int j = 0; j < 2; j++)
                {
                    var wing = DrawKit.Place(Pts(Vector2.Zero, 34f * s, 9f * s * flap, 0f, 0f, 0.5f, -1f, 1.6f, -0.6f, 1.6f, 0.2f, 0.5f, 0.6f), q + DrawKit.Rot(new Vector2(14f - j * 10f, 0) * s, Vector2.Zero, ang), ang + k * (MathF.PI / 2f + (j == 0 ? -0.25f : 0.2f)));
                    Poly(dc, b, wing, Rgb(1, 1, 1, 0.35f * a));
                }
            Poly(dc, b, body, Rgb(0.8f, 0.15f, 0.1f, a));
        }
    }
}

// =====================================================================
//  セミと入道雲
//   青空にもくもくの入道雲。「ミーンミーン」の文字がふるえる。夏の場面に
// =====================================================================
public sealed class CicadaCloudPlugin : QuickShapePlugin
{
    public override string Name => "演出：セミと入道雲";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CicadaCloudParameter(sharedData);
}

public sealed class CicadaCloudParameter : QuickShapeParameter
{
    public CicadaCloudParameter() : this(null) { }
    public CicadaCloudParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ミーン ミーン";
        Color1 = Hex(0x2E9BFF);
        Color2 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "セミの声", Group: "「セミと入道雲」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "青空の背景", Group: "「セミと入道雲」の設定"),
        ["Color1"] = new ParamGuide(Label: "空の色"),
        ["Color2"] = new ParamGuide(Label: "雲の色"),
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
        if (Background) Sky(dc, w, h, C1(a), Col(Hex(0xBFE6FF), a));
        var cb = new Vector2(w * 0.55f, h * 0.62f);
        for (int i = 0; i < 14; i++)
        {
            float f = i / 13f;
            var q = cb + new Vector2((DrawKit.H(Seed, i) - 0.5f) * 700f * s * (1f - f * 0.5f), -f * 520f * s);
            float r = (180f - f * 80f) * s * (1f + 0.02f * MathF.Sin(t + i));
            Dot(dc, b, q + new Vector2(20f, 26f) * s, r, Rgb(0.75f, 0.82f, 0.9f, a));
            Dot(dc, b, q, r, C2(a));
        }
        Box(dc, b, 0, h * 0.82f, w, h, Rgb(0.25f, 0.55f, 0.25f, Background ? a : 0f));
        float shake = MathF.Sin(t * 40f) * 3f * s;
        Say(dc, b, Str, 70f * s, new Vector2(w * 0.2f + shake, h * 0.3f), Rgb(0.2f, 0.2f, 0.25f, 0.8f * a), Rgb(1, 1, 1, 0.8f * a), 4f * s);
    }
}

// =====================================================================
//  天の川
//   夜空を斜めに横切る天の川。細かい星がまたたき、ときどき流れ星
// =====================================================================
public sealed class MilkyWayPlugin : QuickShapePlugin
{
    public override string Name => "演出：天の川";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MilkyWayParameter(sharedData);
}

public sealed class MilkyWayParameter : QuickShapeParameter
{
    public MilkyWayParameter() : this(null) { }
    public MilkyWayParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 400;
        Color1 = Hex(0x9FA8FF);
        Color2 = Hex(0x0A0F2C);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "星の数", Min: 50, Max: 1500, Group: "「天の川」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "夜空の背景", Group: "「天の川」の設定"),
        ["Color1"] = new ParamGuide(Label: "天の川の色"),
        ["Color2"] = new ParamGuide(Label: "夜空の色"),
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
        if (Background) Sky(dc, w, h, C2(a), Col(Hex(0x1C2552), a));
        var a0 = new Vector2(-w * 0.1f, h * 0.9f); var a1 = new Vector2(w * 1.1f, h * 0.1f);
        var dir = Vector2.Normalize(a1 - a0); var nrm = new Vector2(-dir.Y, dir.X);
        for (int i = 0; i < 10; i++) DrawKit.Glow(dc, Vector2.Lerp(a0, a1, i / 9f) + nrm * MathF.Sin(i * 1.7f) * 60f * s, 340f * s, C1(0.16f * a));
        for (int i = 0; i < Count; i++)
        {
            float f = DrawKit.H(Seed, i), g = DrawKit.H(Seed, i, 1);
            float spread = (g - 0.5f) * (i % 3 == 0 ? h * 1.6f : 360f * s);
            var q = Vector2.Lerp(a0, a1, f) + nrm * spread;
            float tw = 0.5f + 0.5f * MathF.Sin(t * (2f + 3f * DrawKit.H(Seed, i, 2)) + i);
            Dot(dc, b, q, (0.8f + 2f * DrawKit.H(Seed, i, 3)) * s, Rgb(1, 1, 1, (0.3f + 0.7f * tw) * a));
        }
        float sh = Gfx.Mod(t * 0.25f, 1f);
        if (sh < 0.15f) { float f = sh / 0.15f; var st = new Vector2(w * 0.7f, h * 0.1f); var en = st + new Vector2(-400f, 250f) * s * f; Ln(dc, b, Vector2.Lerp(st, en, MathF.Max(0f, f - 0.4f) / MathF.Max(f, 0.01f)), en, Rgb(1, 1, 1, (1f - f) * a), 3f * s); }
    }
}

// =====================================================================
//  砂浜と寄せる波
//   砂浜に白い波がザザーンと寄せては返す。上は青い海
// =====================================================================
public sealed class BeachWavesPlugin : QuickShapePlugin
{
    public override string Name => "演出：砂浜と寄せる波";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BeachWavesParameter(sharedData);
}

public sealed class BeachWavesParameter : QuickShapeParameter
{
    public BeachWavesParameter() : this(null) { }
    public BeachWavesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x1E88E5);
        Color2 = Hex(0xF2D9A6);
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
        ["Color1"] = new ParamGuide(Label: "海の色"),
        ["Color2"] = new ParamGuide(Label: "砂浜の色"),
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
        float ph = 0.5f - 0.5f * MathF.Cos(t * 0.9f * Spd(c));
        float shore = h * (0.45f + 0.12f * ph);
        Box(dc, b, 0, 0, w, h, C2(a));
        var sea = new List<Vector2> { new(-10, -10), new(w + 10, -10) };
        for (int i = 40; i >= 0; i--) { float x = w * i / 40f; sea.Add(new Vector2(x, shore + MathF.Sin(x * 0.01f / s + t * 1.5f) * 20f * s + MathF.Sin(x * 0.023f / s) * 15f * s)); }
        Poly(dc, b, sea, C1(a));
        var foam = new List<Vector2>();
        for (int i = 0; i <= 40; i++) { float x = w * i / 40f; foam.Add(new Vector2(x, shore + MathF.Sin(x * 0.01f / s + t * 1.5f) * 20f * s + MathF.Sin(x * 0.023f / s) * 15f * s)); }
        Path(dc, b, foam, Rgb(1, 1, 1, 0.9f * a), 16f * s);
        var wet = foam.Select(p => p + new Vector2(0, 40f * s * (1f - ph))).ToList();
        Path(dc, b, wet, Rgb(0.6f, 0.5f, 0.35f, 0.25f * a), 30f * s);
        Shade(dc, new Vector2(0, 0), new Vector2(0, shore), Rgb(0.05f, 0.25f, 0.5f, 0.5f * a), Rgb(0, 0, 0, 0f), 0, 0, w, shore * 0.6f);
    }
}

// =====================================================================
//  山並みの多層スクロール
//   遠くの山ほどゆっくり動く、何層もの山並み。乗り物で移動している場面に。「数」で層の数
// =====================================================================
public sealed class MountainParallaxPlugin : QuickShapePlugin
{
    public override string Name => "演出：山並みの多層スクロール";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MountainParallaxParameter(sharedData);
}

public sealed class MountainParallaxParameter : QuickShapeParameter
{
    public MountainParallaxParameter() : this(null) { }
    public MountainParallaxParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 4;
        Color1 = Hex(0x3E5C76);
        Color2 = Hex(0xB8D8E8);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "山の層の数", Min: 1, Max: 8, Group: "「山並みの多層スクロール」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "空の背景", Group: "「山並みの多層スクロール」の設定"),
        ["Color1"] = new ParamGuide(Label: "手前の山の色"),
        ["Color2"] = new ParamGuide(Label: "奥の山の色"),
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
        if (Background) Sky(dc, w, h, Col(Hex(0x88C1E0), a), C2(a));
        int n = Math.Clamp(Count, 1, 8);
        for (int L = 0; L < n; L++)
        {
            float depth = (L + 1f) / n;
            float spd = 30f * s * depth * depth * 6f * Spd(c);
            float baseY = h * (0.45f + 0.4f * depth);
            var col = Gfx.Mix(Color2, Color1, depth);
            var pts = new List<Vector2> { new(-10, h + 10) };
            for (int i = 0; i <= 60; i++)
            {
                float x = w * i / 60f, wx = (x + t * spd) / s;
                float y = baseY - (MathF.Abs(MathF.Sin(wx * 0.004f + L * 3f)) * 160f + MathF.Sin(wx * 0.011f + L) * 50f + 60f) * s * (1.4f - depth * 0.6f);
                pts.Add(new Vector2(x, y));
            }
            pts.Add(new Vector2(w + 10, h + 10));
            Poly(dc, b, pts, Col(col, a));
        }
    }
}

// =====================================================================
//  竹林
//   まっすぐな竹が何本も並び、ゆっくりゆれる。上から光が差す
// =====================================================================
public sealed class BambooForestPlugin : QuickShapePlugin
{
    public override string Name => "演出：竹林";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BambooForestParameter(sharedData);
}

public sealed class BambooForestParameter : QuickShapeParameter
{
    public BambooForestParameter() : this(null) { }
    public BambooForestParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 14;
        Color1 = Hex(0x6E9F3A);
        Color2 = Hex(0xDDEFC4);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "竹の本数", Min: 3, Max: 40, Group: "「竹林」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "背景", Group: "「竹林」の設定"),
        ["Color1"] = new ParamGuide(Label: "竹の色"),
        ["Color2"] = new ParamGuide(Label: "奥の色"),
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
        if (Background) Sky(dc, w, h, C2(a), Col(Hex(0x8FB86B), a));
        for (int i = 0; i < Count; i++)
        {
            float depth = DrawKit.H(Seed, i, 4);
            float x = w * (i + DrawKit.H(Seed, i)) / Count;
            float bw = (20f + 30f * depth) * s;
            float sway = MathF.Sin(t * 0.8f + i) * 15f * s * (1f - depth * 0.5f);
            var col = Gfx.Mix(Color2, Color1, 0.4f + depth * 0.6f);
            Poly(dc, b, new List<Vector2> { new(x - bw / 2f + sway, -10), new(x + bw / 2f + sway, -10), new(x + bw / 2f, h + 10), new(x - bw / 2f, h + 10) }, Col(col, a));
            for (int k = 0; k < 7; k++)
            {
                float y = h * (k + 0.5f + DrawKit.H(Seed, i, k) * 0.3f) / 7f;
                float sx = sway * (1f - y / h);
                Box(dc, b, x - bw / 2f - 2f * s + sx, y, x + bw / 2f + 2f * s + sx, y + 5f * s, Col(Gfx.Mix(col, Hex(0x2E4A16), 0.4f), a));
            }
        }
        for (int i = 0; i < 4; i++) Poly(dc, b, new List<Vector2> { new(w * (0.2f + i * 0.2f), 0), new(w * (0.26f + i * 0.2f), 0), new(w * (0.1f + i * 0.2f), h), new(w * (0.02f + i * 0.2f), h) }, Rgb(1, 1, 0.85f, 0.07f * a));
    }
}

// =====================================================================
//  滝
//   上から白い水が落ち、下で水しぶきと霧が立つ。「値」で滝の幅
// =====================================================================
public sealed class WaterfallPlugin : QuickShapePlugin
{
    public override string Name => "演出：滝";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WaterfallParameter(sharedData);
}

public sealed class WaterfallParameter : QuickShapeParameter
{
    public WaterfallParameter() : this(null) { }
    public WaterfallParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xE6F6FF);
        Color2 = Hex(0x4C8FB8);
    }
    protected override double DefAmount => 30;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "滝の幅", Group: "「滝」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "水の色"),
        ["Color2"] = new ParamGuide(Label: "滝の奥の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        float ww = (150f + 700f * Amt(c)) * u;
        float top = o.Y - 450f * u, bot = o.Y + 300f * u;
        Box(dc, b, o.X - ww / 2f, top, o.X + ww / 2f, bot, C2(0.9f * a));
        for (int i = 0; i < 40; i++)
        {
            float x = o.X - ww / 2f + ww * DrawKit.H(Seed, i);
            float len = (60f + 120f * DrawKit.H(Seed, i, 1)) * u;
            float y = top + Gfx.Mod(DrawKit.H(Seed, i, 2) * (bot - top) + t * 700f * u * Spd(c), bot - top + len) - len;
            Ln(dc, b, new Vector2(x, MathF.Max(top, y)), new Vector2(x, MathF.Min(bot, y + len)), C1(0.8f * a), (3f + 5f * DrawKit.H(Seed, i, 3)) * u);
        }
        for (int i = 0; i < 20; i++)
        {
            float ph = Gfx.Mod(t * 1.2f + DrawKit.H(Seed, i, 5), 1f);
            var q = new Vector2(o.X + (DrawKit.H(Seed, i, 6) - 0.5f) * ww * 1.6f, bot - MathF.Sin(ph * MathF.PI) * 80f * u);
            Dot(dc, b, q, (30f + 40f * ph) * u, C1(0.3f * (1f - ph) * a));
        }
        Oval(dc, b, new Vector2(o.X, bot), ww * 0.8f, 40f * u, C1(0.6f * a));
    }
}
