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
//  集中線の背景（色つき・回る）
//   色つきの放射状の集中線が、ゆっくり回る背景。まん中は空く。「数」で線の本数
// =====================================================================
public sealed class ColorFocusLinesPlugin : QuickShapePlugin
{
    public override string Name => "演出：集中線の背景（色つき・回る）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ColorFocusLinesParameter(sharedData);
}

public sealed class ColorFocusLinesParameter : QuickShapeParameter
{
    public ColorFocusLinesParameter() : this(null) { }
    public ColorFocusLinesParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 60;
        Color1 = Hex(0xFF4081);
        Color2 = Hex(0xFFF59D);
    }
    protected override double DefAmount => 30;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "まん中の空き", Group: "「集中線の背景（色つき・回る）」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "線の本数", Min: 10, Max: 200, Group: "「集中線の背景（色つき・回る）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "地の色で塗る", Group: "「集中線の背景（色つき・回る）」の設定"),
        ["Color1"] = new ParamGuide(Label: "線の色"),
        ["Color2"] = new ParamGuide(Label: "地の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = MathF.Max(w, h) * 1.4f;
        if (Background) Box(dc, b, 0, 0, w, h, C2(a));
        float inner = MathF.Min(w, h) * (0.05f + 0.4f * Amt(c));
        float rot = t * 0.3f * Spd(c);
        for (int i = 0; i < Count; i++)
        {
            float ang = rot + i * MathF.PI * 2f / Count + DrawKit.H(Seed, i) * 0.05f;
            float wd = 0.008f + 0.02f * DrawKit.H(Seed, i, 1);
            float r0 = inner * (0.9f + 0.4f * DrawKit.H(Seed, i, 2));
            Poly(dc, b, new List<Vector2> { Pol(o, r0, ang), Pol(o, R, ang - wd), Pol(o, R, ang + wd) }, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), DrawKit.H(Seed, i, 3) * 0.3f), a));
        }
    }
}

// =====================================================================
//  雨の夜のネオン街
//   夜の街に雨が降り、ネオン看板の光がぬれた路面に長く映る
// =====================================================================
public sealed class NeonRainStreetPlugin : QuickShapePlugin
{
    public override string Name => "演出：雨の夜のネオン街";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NeonRainStreetParameter(sharedData);
}

public sealed class NeonRainStreetParameter : QuickShapeParameter
{
    public NeonRainStreetParameter() : this(null) { }
    public NeonRainStreetParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 120;
        Text = "BAR,ラーメン,HOTEL,24h";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "看板の文字", Tip: "カンマ区切り", Group: "「雨の夜のネオン街」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "雨の量", Min: 0, Max: 400, Group: "「雨の夜のネオン街」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
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
        float s = U0(c);
        Sky(dc, w, h, Rgb(0.03f, 0.03f, 0.08f, a), Rgb(0.08f, 0.05f, 0.15f, a));
        float road = h * 0.68f;
        var it = Items;
        for (int i = 0; i < 7; i++)
        {
            float x0 = w * i / 7f, bw = w / 7f - 10f * s, bh = h * (0.35f + 0.3f * DrawKit.H(Seed, i));
            Box(dc, b, x0, road - bh, x0 + bw, road, Rgb(0.07f, 0.07f, 0.12f, a));
            for (int k = 0; k < 10; k++) if (DrawKit.H(Seed, i, k) > 0.6f) Box(dc, b, x0 + 12f * s + (k % 3) * bw / 3f, road - bh + 30f * s + (k / 3) * 50f * s, x0 + 12f * s + (k % 3) * bw / 3f + 24f * s, road - bh + 60f * s + (k / 3) * 50f * s, Rgb(1f, 0.85f, 0.5f, 0.6f * a));
        }
        for (int i = 0; i < it.Count; i++)
        {
            var col = BubbleKit.Palette(0, i + 2);
            var sp = new Vector2(w * (0.12f + 0.25f * i) % w, road - h * (0.15f + 0.1f * (i % 2)));
            float flick = (int)(t * 7f + i * 3f) % 17 == 0 ? 0.3f : 1f;
            var ts = Measure(it[i], 60f * s);
            DrawKit.Glow(dc, sp, ts.Width * 0.9f, Col(col, 0.35f * flick * a));
            RFrame(dc, b, sp, ts.Width + 40f * s, ts.Height + 24f * s, 12f * s, Col(col, flick * a), 5f * s);
            Say(dc, b, it[i], 60f * s, sp, Col(col, flick * a));
            for (int k = 0; k < 14; k++)
            {
                float y = road + 10f * s + k * 22f * s;
                float wob = MathF.Sin(t * 3f + k + i) * 8f * s;
                Box(dc, b, sp.X - ts.Width / 2f + wob, y, sp.X + ts.Width / 2f + wob, y + 8f * s, Col(col, 0.25f * flick * a * (1f - k / 14f)));
            }
        }
        Box(dc, b, 0, road, w, road + 4f * s, Rgb(0.4f, 0.4f, 0.5f, 0.4f * a));
        for (int i = 0; i < Count; i++)
        {
            float x = DrawKit.H(Seed, i, 20) * w, y = Gfx.Mod(DrawKit.H(Seed, i, 21) * h + t * 1500f * s, h + 60f) - 30f;
            Ln(dc, b, new Vector2(x, y), new Vector2(x - 4f * s, y + 36f * s), Rgb(0.7f, 0.75f, 0.9f, 0.35f * a), 1.5f * s);
        }
    }
}

// =====================================================================
//  教室（黒板・机・窓）
//   黒板と窓と並んだ机の教室。黒板に「文字」を書ける
// =====================================================================
public sealed class ClassroomPlugin : QuickShapePlugin
{
    public override string Name => "演出：教室（黒板・机・窓）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ClassroomParameter(sharedData);
}

public sealed class ClassroomParameter : QuickShapeParameter
{
    public ClassroomParameter() : this(null) { }
    public ClassroomParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "今日のテーマ";
        Color1 = Hex(0x2E5D44);
        Color2 = Hex(0xF3E5C8);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "黒板の文字", Group: "「教室（黒板・机・窓）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "黒板の色"),
        ["Color2"] = new ParamGuide(Label: "壁の色"),
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
        Box(dc, b, 0, 0, w, h, C2(a));
        Box(dc, b, 0, h * 0.78f, w, h, Rgb(0.65f, 0.48f, 0.3f, a));
        for (int i = 0; i < 10; i++) Ln(dc, b, new Vector2(w * i / 10f, h * 0.78f), new Vector2(w * i / 10f - 80f * s, h), Rgb(0.5f, 0.35f, 0.2f, 0.4f * a), 3f * s);
        var bb0 = new Vector2(w * 0.08f, h * 0.12f); var bb1 = new Vector2(w * 0.62f, h * 0.5f);
        Box(dc, b, bb0.X - 14f * s, bb0.Y - 14f * s, bb1.X + 14f * s, bb1.Y + 14f * s, Rgb(0.55f, 0.38f, 0.22f, a));
        Box(dc, b, bb0.X, bb0.Y, bb1.X, bb1.Y, C1(a));
        Box(dc, b, bb0.X, bb1.Y + 4f * s, bb1.X, bb1.Y + 20f * s, Rgb(0.45f, 0.32f, 0.2f, a));
        Say(dc, b, Str, 70f * s, (bb0 + bb1) / 2f, Rgb(0.95f, 0.95f, 0.9f, 0.9f * a), bold: false, maxW: (bb1.X - bb0.X) * 0.9f);
        for (int i = 0; i < 2; i++)
        {
            float x0 = w * (0.7f + i * 0.14f), x1 = x0 + w * 0.12f;
            Shade(dc, new Vector2(0, h * 0.1f), new Vector2(0, h * 0.5f), Col(Hex(0x81D4FA), a), Col(Hex(0xE1F5FE), a), x0, h * 0.1f, x1, h * 0.5f);
            Frame(dc, b, x0, h * 0.1f, x1, h * 0.5f, Rgb(0.85f, 0.85f, 0.85f, a), 10f * s);
            Ln(dc, b, new Vector2((x0 + x1) / 2f, h * 0.1f), new Vector2((x0 + x1) / 2f, h * 0.5f), Rgb(0.85f, 0.85f, 0.85f, a), 6f * s);
        }
        for (int r = 0; r < 2; r++)
            for (int k = 0; k < 5; k++)
            {
                float sc = r == 0 ? 0.8f : 1.1f;
                var dp = new Vector2(w * (0.12f + k * 0.19f), h * (r == 0 ? 0.74f : 0.9f));
                Box(dc, b, dp.X - 110f * s * sc, dp.Y - 20f * s * sc, dp.X + 110f * s * sc, dp.Y, Rgb(0.75f, 0.55f, 0.35f, a));
                Box(dc, b, dp.X - 100f * s * sc, dp.Y, dp.X - 90f * s * sc, dp.Y + 80f * s * sc, Rgb(0.5f, 0.5f, 0.55f, a));
                Box(dc, b, dp.X + 90f * s * sc, dp.Y, dp.X + 100f * s * sc, dp.Y + 80f * s * sc, Rgb(0.5f, 0.5f, 0.55f, a));
            }
    }
}

// =====================================================================
//  オフィス（デスク・PC・観葉植物）
//   窓のあるオフィスの背景。デスクにパソコン、すみに観葉植物
// =====================================================================
public sealed class OfficeDeskPlugin : QuickShapePlugin
{
    public override string Name => "演出：オフィス（デスク・PC・観葉植物）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new OfficeDeskParameter(sharedData);
}

public sealed class OfficeDeskParameter : QuickShapeParameter
{
    public OfficeDeskParameter() : this(null) { }
    public OfficeDeskParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xECEFF1);
        Color2 = Hex(0x8D6E63);
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
        ["Color1"] = new ParamGuide(Label: "壁の色"),
        ["Color2"] = new ParamGuide(Label: "デスクの色"),
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
        Box(dc, b, 0, 0, w, h, C1(a));
        Shade(dc, new Vector2(0, h * 0.08f), new Vector2(0, h * 0.55f), Col(Hex(0x90CAF9), a), Col(Hex(0xE3F2FD), a), w * 0.1f, h * 0.08f, w * 0.9f, h * 0.55f);
        for (int i = 0; i < 8; i++) Box(dc, b, w * (0.12f + i * 0.1f), h * (0.3f + 0.15f * DrawKit.H(Seed, i)), w * (0.12f + i * 0.1f) + w * 0.07f, h * 0.55f, Rgb(0.6f, 0.68f, 0.78f, a));
        for (int i = 0; i <= 4; i++) Box(dc, b, w * (0.1f + i * 0.2f) - 6f * s, h * 0.08f, w * (0.1f + i * 0.2f) + 6f * s, h * 0.55f, Rgb(0.75f, 0.78f, 0.82f, a));
        Box(dc, b, 0, h * 0.82f, w, h, Rgb(0.55f, 0.57f, 0.6f, a));
        Box(dc, b, w * 0.15f, h * 0.68f, w * 0.85f, h * 0.72f, C2(a));
        Box(dc, b, w * 0.18f, h * 0.72f, w * 0.2f, h * 0.9f, Col(Gfx.Mix(Color2, Hex(0x000000), 0.3f), a));
        Box(dc, b, w * 0.8f, h * 0.72f, w * 0.82f, h * 0.9f, Col(Gfx.Mix(Color2, Hex(0x000000), 0.3f), a));
        var mon = new Vector2(w * 0.45f, h * 0.55f);
        RBox(dc, b, mon, 420f * s, 260f * s, 12f * s, Rgb(0.15f, 0.15f, 0.18f, a));
        Shade(dc, mon - new Vector2(190f * s, 110f * s), mon + new Vector2(190f * s, 110f * s), Rgb(0.25f, 0.5f, 0.9f, a), Rgb(0.4f, 0.2f, 0.7f, a), mon.X - 190f * s, mon.Y - 110f * s, mon.X + 190f * s, mon.Y + 110f * s);
        Box(dc, b, mon.X - 20f * s, mon.Y + 130f * s, mon.X + 20f * s, h * 0.68f, Rgb(0.2f, 0.2f, 0.22f, a));
        Box(dc, b, mon.X - 160f * s, h * 0.66f, mon.X + 160f * s, h * 0.68f, Rgb(0.25f, 0.25f, 0.28f, a));
        var mug = new Vector2(w * 0.68f, h * 0.65f);
        Box(dc, b, mug.X - 30f * s, mug.Y - 30f * s, mug.X + 30f * s, mug.Y + 30f * s, Rgb(1, 1, 1, a));
        var pot = new Vector2(w * 0.93f, h * 0.88f);
        Poly(dc, b, Pts(pot, s, s, -60f, -60f, 60f, -60f, 45f, 60f, -45f, 60f), Rgb(0.85f, 0.5f, 0.3f, a));
        for (int i = 0; i < 9; i++)
        {
            float ang = -MathF.PI / 2f + (i - 4f) * 0.3f + MathF.Sin(t * 0.8f + i) * 0.04f;
            var tip = Pol(pot - new Vector2(0, 60f * s), (200f + 60f * DrawKit.H(Seed, i, 4)) * s, ang);
            Poly(dc, b, new List<Vector2> { pot - new Vector2(8f * s, 60f * s), Vector2.Lerp(pot - new Vector2(0, 60f * s), tip, 0.5f) + new Vector2(30f * s, 0), tip, Vector2.Lerp(pot - new Vector2(0, 60f * s), tip, 0.5f) - new Vector2(30f * s, 0), pot - new Vector2(-8f * s, 60f * s) }, Rgb(0.25f, 0.55f, 0.3f, a));
        }
    }
}

// =====================================================================
//  夜のコンビニ前
//   夜の駐車場から見た、明るいコンビニの店先。看板と窓の光
// =====================================================================
public sealed class ConvenienceStoreNightPlugin : QuickShapePlugin
{
    public override string Name => "演出：夜のコンビニ前";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ConvenienceStoreNightParameter(sharedData);
}

public sealed class ConvenienceStoreNightParameter : QuickShapeParameter
{
    public ConvenienceStoreNightParameter() : this(null) { }
    public ConvenienceStoreNightParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "YUKKURI MART";
        Color1 = Hex(0x43A047);
        Color2 = Hex(0xFF7043);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "看板の文字", Group: "「夜のコンビニ前」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "看板の色1"),
        ["Color2"] = new ParamGuide(Label: "看板の色2"),
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
        Sky(dc, w, h, Rgb(0.03f, 0.04f, 0.1f, a), Rgb(0.08f, 0.1f, 0.2f, a));
        float gy = h * 0.72f;
        var shop0 = new Vector2(w * 0.12f, h * 0.25f); var shop1 = new Vector2(w * 0.88f, gy);
        Box(dc, b, shop0.X, shop0.Y, shop1.X, shop1.Y, Rgb(0.85f, 0.85f, 0.85f, a));
        Box(dc, b, shop0.X, shop0.Y, shop1.X, shop0.Y + 90f * s, Rgb(1, 1, 1, a));
        Box(dc, b, shop0.X, shop0.Y + 90f * s, shop1.X, shop0.Y + 110f * s, C1(a));
        Box(dc, b, shop0.X, shop0.Y + 110f * s, shop1.X, shop0.Y + 130f * s, C2(a));
        Say(dc, b, Str, 64f * s, new Vector2((shop0.X + shop1.X) / 2f, shop0.Y + 45f * s), C1(a));
        DrawKit.Glow(dc, new Vector2(w / 2f, gy - 100f * s), w * 0.5f, Rgb(1, 1, 0.9f, 0.25f * a));
        for (int i = 0; i < 5; i++)
        {
            float x0 = shop0.X + 40f * s + i * (shop1.X - shop0.X - 80f * s) / 5f, x1 = x0 + (shop1.X - shop0.X - 80f * s) / 5f - 20f * s;
            Box(dc, b, x0, shop0.Y + 170f * s, x1, gy - 10f * s, Rgb(1f, 1f, 0.92f, a));
            for (int k = 0; k < 3; k++) Box(dc, b, x0 + 10f * s, shop0.Y + 220f * s + k * 80f * s, x1 - 10f * s, shop0.Y + 240f * s + k * 80f * s, Col(BubbleKit.Palette(0, i + k), 0.6f * a));
        }
        Box(dc, b, 0, gy, w, h, Rgb(0.15f, 0.15f, 0.18f, a));
        for (int i = 0; i < 4; i++) Ln(dc, b, new Vector2(w * (0.15f + i * 0.23f), gy + 30f * s), new Vector2(w * (0.1f + i * 0.25f), h), Rgb(1, 1, 1, 0.5f * a), 6f * s);
        Shade(dc, new Vector2(0, gy), new Vector2(0, h), Rgb(1, 1, 0.9f, 0.25f * a), Rgb(1, 1, 0.9f, 0f), shop0.X, gy, shop1.X, h);
        for (int i = 0; i < 6; i++) { float ph = Gfx.Mod(t * 0.7f + i * 0.17f, 1f); var q = new Vector2(w * 0.5f + MathF.Sin(t * 3f + i) * 200f * s, shop0.Y + 60f * s + MathF.Cos(t * 2f + i * 2f) * 60f * s); Dot(dc, b, q, 4f * s, Rgb(1, 1, 0.8f, 0.8f * a * MathF.Sin(ph * MathF.PI))); }
    }
}

// =====================================================================
//  ゲームセンター（筐体の光）
//   暗い店内に、画面が光る筐体がずらり。床に光が映る。「数」で筐体の数
// =====================================================================
public sealed class GameCenterPlugin : QuickShapePlugin
{
    public override string Name => "演出：ゲームセンター（筐体の光）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GameCenterParameter(sharedData);
}

public sealed class GameCenterParameter : QuickShapeParameter
{
    public GameCenterParameter() : this(null) { }
    public GameCenterParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 5;
        Color1 = Hex(0x1A0033);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "筐体の数", Min: 2, Max: 10, Group: "「ゲームセンター（筐体の光）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "店内の色"),
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
        float s = U0(c);
        Sky(dc, w, h, C1(a), Col(Gfx.Mix(Color1, Hex(0x000000), 0.5f), a));
        float gy = h * 0.8f;
        Box(dc, b, 0, gy, w, h, Rgb(0.08f, 0.05f, 0.12f, a));
        for (int i = 0; i < 20; i++) Dot(dc, b, new Vector2(DrawKit.H(Seed, i) * w, gy + DrawKit.H(Seed, i, 1) * (h - gy)), 4f * s, Col(BubbleKit.Palette(0, i), 0.5f * a));
        int n = Math.Clamp(Count, 2, 10);
        float cw = w / n;
        for (int i = 0; i < n; i++)
        {
            float x0 = i * cw + cw * 0.12f, x1 = (i + 1) * cw - cw * 0.12f;
            var col = BubbleKit.Palette(0, i);
            Box(dc, b, x0, h * 0.2f, x1, gy, Rgb(0.12f, 0.1f, 0.18f, a));
            Box(dc, b, x0, h * 0.2f, x1, h * 0.28f, Col(col, a));
            var sc0 = new Vector2(x0 + cw * 0.06f, h * 0.31f); var sc1 = new Vector2(x1 - cw * 0.06f, h * 0.55f);
            Box(dc, b, sc0.X, sc0.Y, sc1.X, sc1.Y, Col(Gfx.Mix(col, Hex(0x000000), 0.6f), a));
            for (int k = 0; k < 6; k++) Dot(dc, b, new Vector2(sc0.X + (sc1.X - sc0.X) * Gfx.Mod(DrawKit.H(Seed + i, k) + t * 0.3f * (k % 2 == 0 ? 1 : -1), 1f), sc0.Y + (sc1.Y - sc0.Y) * DrawKit.H(Seed + i, k, 1)), 8f * s, Col(col, a));
            DrawKit.Glow(dc, (sc0 + sc1) / 2f, cw * 0.7f, Col(col, 0.25f * a * (0.8f + 0.2f * MathF.Sin(t * 3f + i))));
            Box(dc, b, x0 - 10f * s, h * 0.58f, x1 + 10f * s, h * 0.63f, Rgb(0.2f, 0.18f, 0.25f, a));
            Dot(dc, b, new Vector2(x0 + cw * 0.15f, h * 0.6f), 10f * s, Rgb(0.9f, 0.2f, 0.2f, a));
            for (int k = 0; k < 3; k++) Dot(dc, b, new Vector2(x0 + cw * (0.4f + k * 0.12f), h * 0.6f), 9f * s, Col(BubbleKit.Palette(0, k + 3), a));
            Shade(dc, new Vector2(0, gy), new Vector2(0, h), Col(col, 0.25f * a), Col(col, 0f), x0, gy, x1, h);
        }
    }
}

// =====================================================================
//  病院・保健室
//   白い壁とベッド、ゆれる仕切りのカーテン。窓からやわらかい光
// =====================================================================
public sealed class InfirmaryRoomPlugin : QuickShapePlugin
{
    public override string Name => "演出：病院・保健室";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new InfirmaryRoomParameter(sharedData);
}

public sealed class InfirmaryRoomParameter : QuickShapeParameter
{
    public InfirmaryRoomParameter() : this(null) { }
    public InfirmaryRoomParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xF5F7FA);
        Color2 = Hex(0xB2DFDB);
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
        ["Color1"] = new ParamGuide(Label: "壁の色"),
        ["Color2"] = new ParamGuide(Label: "カーテンの色"),
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
        Box(dc, b, 0, 0, w, h, C1(a));
        Box(dc, b, 0, h * 0.8f, w, h, Rgb(0.82f, 0.85f, 0.88f, a));
        Shade(dc, new Vector2(0, h * 0.1f), new Vector2(0, h * 0.5f), Col(Hex(0xB3E5FC), a), Col(Hex(0xFFFFFF), a), w * 0.62f, h * 0.1f, w * 0.92f, h * 0.5f);
        Frame(dc, b, w * 0.62f, h * 0.1f, w * 0.92f, h * 0.5f, Rgb(0.8f, 0.82f, 0.85f, a), 10f * s);
        for (int i = 0; i < 3; i++) Poly(dc, b, new List<Vector2> { new(w * (0.66f + i * 0.08f), h * 0.12f), new(w * (0.7f + i * 0.08f), h * 0.12f), new(w * (0.55f + i * 0.08f), h * 0.8f), new(w * (0.48f + i * 0.08f), h * 0.8f) }, Rgb(1, 1, 0.9f, 0.12f * a));
        var bed0 = new Vector2(w * 0.08f, h * 0.6f); var bed1 = new Vector2(w * 0.5f, h * 0.72f);
        Box(dc, b, bed0.X, bed0.Y, bed1.X, bed1.Y, Rgb(1, 1, 1, a));
        Box(dc, b, bed0.X, bed1.Y, bed1.X, bed1.Y + 20f * s, Rgb(0.7f, 0.75f, 0.8f, a));
        RBox(dc, b, new Vector2(bed0.X + 90f * s, bed0.Y - 10f * s), 140f * s, 60f * s, 20f * s, Rgb(0.95f, 0.97f, 1f, a));
        Box(dc, b, bed0.X + 200f * s, bed0.Y - 6f * s, bed1.X, bed0.Y + 40f * s, C2(0.6f * a));
        Box(dc, b, bed0.X, bed0.Y - 120f * s, bed0.X + 14f * s, bed1.Y + 120f * s, Rgb(0.7f, 0.75f, 0.8f, a));
        Box(dc, b, bed1.X - 14f * s, bed0.Y - 60f * s, bed1.X, bed1.Y + 120f * s, Rgb(0.7f, 0.75f, 0.8f, a));
        Ln(dc, b, new Vector2(w * 0.52f, h * 0.06f), new Vector2(w * 0.6f, h * 0.06f), Rgb(0.6f, 0.6f, 0.65f, a), 6f * s);
        var cur = new List<Vector2>();
        for (int i = 0; i <= 12; i++) cur.Add(new Vector2(w * 0.52f + i * w * 0.007f, h * 0.06f));
        for (int i = 12; i >= 0; i--) cur.Add(new Vector2(w * 0.52f + i * w * 0.007f + MathF.Sin(t * 1.5f + i * 0.8f) * 10f * s + 15f * s, h * 0.8f));
        Poly(dc, b, cur, C2(a));
        for (int i = 0; i < 6; i++) Ln(dc, b, new Vector2(w * 0.53f + i * w * 0.014f, h * 0.07f), new Vector2(w * 0.53f + i * w * 0.014f + MathF.Sin(t * 1.5f + i) * 10f * s + 15f * s, h * 0.79f), Rgb(0, 0, 0, 0.08f * a), 4f * s);
    }
}

// =====================================================================
//  魚眼レンズ枠
//   まるくゆがんだ魚眼レンズのふち。まわりが暗く、ふちに光の反射
// =====================================================================
public sealed class FisheyeFramePlugin : QuickShapePlugin
{
    public override string Name => "演出：魚眼レンズ枠";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FisheyeFrameParameter(sharedData);
}

public sealed class FisheyeFrameParameter : QuickShapeParameter
{
    public FisheyeFrameParameter() : this(null) { }
    public FisheyeFrameParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x000000);
    }
    protected override double DefAmount => 45;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "レンズの大きさ", Group: "「魚眼レンズ枠」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "まわりの色"),
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
        var cc = new Vector2(w, h) / 2f;
        float R = MathF.Min(w, h) * (0.35f + 0.3f * Amt(c));
        float rx = R * MathF.Min(1.35f, w / MathF.Max(1f, h)), ry = R;
        b.Color = C1(a);
        DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), DrawKit.EllipsePts(cc, rx, ry, 96));
        using (var g = Gfx.Radial(dc, cc, MathF.Max(rx, ry), (0f, Rgb(0, 0, 0, 0f)), (0.7f, Rgb(0, 0, 0, 0.15f * a)), (1f, Rgb(0, 0, 0, 0.9f * a))))
            dc.FillEllipse(new Ellipse(cc, rx, ry), g);
        Path(dc, b, DrawKit.EllipsePts(cc, rx * 0.92f, ry * 0.92f, 30, MathF.PI * 1.1f, MathF.PI * 1.4f), Rgb(1, 1, 1, 0.25f * a), 12f * U0(c));
        Outline(dc, b, DrawKit.EllipsePts(cc, rx, ry, 96), Rgb(0.2f, 0.2f, 0.22f, a), 8f * U0(c));
    }
}

// =====================================================================
//  テレビの砂嵐・チャンネル切り替え
//   ザーッという砂嵐のノイズ。バリエーション2は、チャンネル番号と縦にゆれる切り替え
// =====================================================================
public sealed class TvStaticPlugin : QuickShapePlugin
{
    public override string Name => "演出：テレビの砂嵐・チャンネル切り替え";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TvStaticParameter(sharedData);
}

public sealed class TvStaticParameter : QuickShapeParameter
{
    public TvStaticParameter() : this(null) { }
    public TvStaticParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 2500;
        Text = "4";
        Color1 = Hex(0x00E676);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "チャンネル番号", Group: "「テレビの砂嵐・チャンネル切り替え」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "ノイズの粒の数", Min: 200, Max: 6000, Group: "「テレビの砂嵐・チャンネル切り替え」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=砂嵐　2=チャンネル切り替え", Min: 1, Max: 2, Group: "「テレビの砂嵐・チャンネル切り替え」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "番号の色"),
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
        float s = U0(c);
        int fr = (int)(t * 30f);
        float k = Variant >= 2 ? MathF.Max(0f, 1f - c.Time * 1.5f) : 1f;
        if (k > 0f)
        {
            Box(dc, b, 0, 0, w, h, Rgb(0.25f, 0.25f, 0.25f, a * k));
            float cell = 6f * s;
            for (int i = 0; i < Count; i++)
            {
                float v = DrawKit.H(Seed + fr, i, 2);
                float x = DrawKit.H(Seed + fr, i) * w, y = DrawKit.H(Seed + fr, i, 1) * h;
                Box(dc, b, x, y, x + cell * (1f + 3f * v), y + cell, Rgb(v, v, v, 0.9f * a * k));
            }
            for (int i = 0; i < 4; i++) { float y = Gfx.Mod(t * 300f * s + i * h / 4f, h); Box(dc, b, 0, y, w, y + 30f * s, Rgb(1, 1, 1, 0.08f * a * k)); }
        }
        if (Variant >= 2)
        {
            float roll = MathF.Max(0f, 1f - c.Time * 2f) * h;
            Box(dc, b, 0, Gfx.Mod(roll, h), w, Gfx.Mod(roll, h) + 20f * s, Rgb(0, 0, 0, 0.6f * a));
            SayAt(dc, b, "CH " + Str, 90f * s, new Vector2(w - 380f * s, 60f * s), C1(a), true);
        }
    }
}
