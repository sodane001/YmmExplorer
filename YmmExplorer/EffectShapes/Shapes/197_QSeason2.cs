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
//  入道雲と夏休みの日めくり
//   青空に入道雲。手前の日めくりが「8月〇日」と進む。「文字」に「月,始めの日,終わりの日」
// =====================================================================
public sealed class SummerCalendarPlugin : QuickShapePlugin
{
    public override string Name => "演出：入道雲と夏休みの日めくり";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SummerCalendarParameter(sharedData);
}

public sealed class SummerCalendarParameter : QuickShapeParameter
{
    public SummerCalendarParameter() : this(null) { }
    public SummerCalendarParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "8,1,31";
        Color1 = Hex(0x29B6F6);
        Color2 = Hex(0xE53935);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "日付", Tip: "カンマ区切りで「月,始めの日,終わりの日」", Group: "「入道雲と夏休みの日めくり」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "青空と入道雲", Group: "「入道雲と夏休みの日めくり」の設定"),
        ["Color1"] = new ParamGuide(Label: "空の色"),
        ["Color2"] = new ParamGuide(Label: "日めくりの帯の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        if (Background)
        {
            Sky(dc, w, h, C1(a), Col(Hex(0xE1F5FE), a));
            var cb = new Vector2(w * 0.35f, h * 0.7f);
            for (int i = 0; i < 12; i++) { float f = i / 11f; var q = cb + new Vector2((DrawKit.H(Seed, i) - 0.5f) * 600f * s * (1f - f * 0.5f), -f * 500f * s); Dot(dc, b, q, (170f - f * 80f) * s, Rgb(1, 1, 1, a)); }
        }
        var it = Items;
        int mo = it.Count > 0 && int.TryParse(it[0], out var m0) ? m0 : 8;
        int d0 = it.Count > 1 && int.TryParse(it[1], out var m1) ? m1 : 1;
        int d1 = it.Count > 2 && int.TryParse(it[2], out var m2) ? m2 : 31;
        float p = DrawKit.EaseOut(Intro(c, 0.3f, 2.5f / MathF.Max(0.1f, Spd(c))));
        int day = d0 + (int)MathF.Round((d1 - d0) * p);
        float cw = 360f * u, ch = 420f * u;
        RBox(dc, b, o + new Vector2(10f, 14f) * u, cw, ch, 14f * u, Rgb(0, 0, 0, 0.25f * a));
        RBox(dc, b, o, cw, ch, 14f * u, Rgb(1, 1, 1, a));
        Box(dc, b, o.X - cw / 2f, o.Y - ch / 2f + 10f * u, o.X + cw / 2f, o.Y - ch / 2f + 100f * u, C2(a));
        Say(dc, b, mo + "月", 54f * u, o - new Vector2(0, ch / 2f - 55f * u), Rgb(1, 1, 1, a));
        Say(dc, b, day.ToString(), 210f * u, o + new Vector2(0, 50f * u), Rgb(0.15f, 0.15f, 0.2f, a));
        string[] wd = { "日", "月", "火", "水", "木", "金", "土" };
        Say(dc, b, wd[(day + 4) % 7] + "曜日", 34f * u, o + new Vector2(0, ch / 2f - 40f * u), Rgb(0.4f, 0.4f, 0.45f, a));
    }
}

// =====================================================================
//  運動会（万国旗とスタートピストル）
//   空に万国旗がはためき、スタートピストルが「パン！」と鳴って煙が出る
// =====================================================================
public sealed class SportsDayPlugin : QuickShapePlugin
{
    public override string Name => "演出：運動会（万国旗とスタートピストル）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SportsDayParameter(sharedData);
}

public sealed class SportsDayParameter : QuickShapeParameter
{
    public SportsDayParameter() : this(null) { }
    public SportsDayParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 14;
        Text = "パン！";
        Color1 = Hex(0x424242);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "音の文字", Group: "「運動会（万国旗とスタートピストル）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "旗の数", Min: 4, Max: 30, Group: "「運動会（万国旗とスタートピストル）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ピストルの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        for (int r = 0; r < 2; r++)
        {
            var rope = new List<Vector2>();
            for (int i = 0; i <= 30; i++) { float f = i / 30f; rope.Add(new Vector2(w * f, h * (0.08f + r * 0.12f) + MathF.Sin(f * MathF.PI) * 90f * s)); }
            Path(dc, b, rope, Rgb(0.3f, 0.3f, 0.3f, a), 3f * s);
            for (int i = 0; i < Count; i++)
            {
                float f = (i + 0.5f) / Count;
                var top = new Vector2(w * f, h * (0.08f + r * 0.12f) + MathF.Sin(f * MathF.PI) * 90f * s);
                float sway = MathF.Sin(t * 4f + i + r) * 0.15f;
                var fl = DrawKit.Place(new List<Vector2> { new(-30f * s, 0), new(30f * s, 0), new(30f * s, 44f * s), new(-30f * s, 44f * s) }, top, sway);
                Poly(dc, b, fl, Col(BubbleKit.Palette(0, i + r * 3), a));
                Dot(dc, b, DrawKit.Rot(top + new Vector2(0, 22f * s), top, sway), 10f * s, Rgb(1, 1, 1, 0.8f * a));
            }
        }
        var gp = o;
        float shot = Intro(c, 0.6f, 0.05f);
        float kick = shot > 0f ? MathF.Max(0f, 1f - (c.Time - 0.6f) * 6f) : 0f;
        var gun = DrawKit.Place(Pts(Vector2.Zero, u, u, -60f, -30f, 120f, -30f, 120f, 0f, 0f, 0f, -10f, 80f, -60f, 80f), gp, -0.4f - kick * 0.3f);
        Poly(dc, b, gun, C1(a));
        var muzzle = DrawKit.Rot(gp + new Vector2(130f * u, -15f * u), gp, -0.4f - kick * 0.3f);
        if (shot > 0f)
        {
            float f = Math.Clamp((c.Time - 0.6f) / 1.5f, 0f, 1f);
            for (int i = 0; i < 6; i++) Dot(dc, b, muzzle + new Vector2((i - 2.5f) * 12f * u, -f * 200f * u - i * 18f * u), (20f + 40f * f) * u, Rgb(0.85f, 0.85f, 0.85f, (1f - f) * 0.7f * a));
            if (f < 0.25f) Poly(dc, b, BubbleKit.SpikyPts(muzzle, 60f * u, 50f * u, 10, 0.45f, Seed), Rgb(1f, 0.85f, 0.3f, a));
            Say(dc, b, Str, 100f * u, muzzle + new Vector2(120f, -120f) * u, Rgb(1, 1, 1, a * (1f - f)), Rgb(0.8f, 0.1f, 0.1f, a * (1f - f)), 6f * u);
        }
    }
}

// =====================================================================
//  お正月の羽子板・獅子舞
//   羽子板と羽根が行ったり来たり。バリエーション2は獅子舞がカチカチ口を動かす
// =====================================================================
public sealed class NewYearHagoitaPlugin : QuickShapePlugin
{
    public override string Name => "演出：お正月の羽子板・獅子舞";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NewYearHagoitaParameter(sharedData);
}

public sealed class NewYearHagoitaParameter : QuickShapeParameter
{
    public NewYearHagoitaParameter() : this(null) { }
    public NewYearHagoitaParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xC62828);
        Color2 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=羽子板　2=獅子舞", Min: 1, Max: 2, Group: "「お正月の羽子板・獅子舞」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "赤の色"),
        ["Color2"] = new ParamGuide(Label: "金の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        if (Variant >= 2)
        {
            float bite = MathF.Abs(MathF.Sin(t * 4f * Spd(c)));
            var hd = o;
            Poly(dc, b, BubbleKit.CloudPts(hd + new Vector2(0, 260f * u), 360f * u, 120f * u, 8, 0.2f), Rgb(0.15f, 0.5f, 0.3f, a));
            for (int i = 0; i < 12; i++) Path(dc, b, Pts(hd + new Vector2(-300f * u + i * 55f * u, 200f * u), u, u, 0f, 0f, 12f, 20f, -6f, 40f), Rgb(1, 1, 1, a), 6f * u);
            RBox(dc, b, hd - new Vector2(0, 60f * u), 360f * u, 220f * u, 60f * u, C1(a));
            RBox(dc, b, hd + new Vector2(0, 80f * u + bite * 40f * u), 320f * u, 100f * u, 40f * u, C1(a));
            for (int e = -1; e <= 1; e += 2) { Dot(dc, b, hd + new Vector2(e * 80f, -90f) * u, 40f * u, C2(a)); Dot(dc, b, hd + new Vector2(e * 80f, -90f) * u, 18f * u, Rgb(0, 0, 0, a)); }
            for (int i = 0; i < 6; i++) Box(dc, b, hd.X - 130f * u + i * 50f * u, hd.Y + 40f * u, hd.X - 100f * u + i * 50f * u, hd.Y + 60f * u, Rgb(1, 1, 1, a));
            Box(dc, b, hd.X - 170f * u, hd.Y + 50f * u, hd.X + 170f * u, hd.Y + 50f * u + bite * 40f * u, Rgb(0.3f, 0.05f, 0.05f, a));
            Poly(dc, b, Pts(hd - new Vector2(0, 160f * u), u, u, -60f, 0f, 0f, -80f, 60f, 0f), C2(a));
            return;
        }
        float ph = t * 1.2f * Spd(c) * MathF.PI;
        float x = MathF.Cos(ph) * 420f * u;
        float y = -MathF.Abs(MathF.Sin(ph)) * 250f * u;
        var shuttle = o + new Vector2(x, y);
        for (int e = -1; e <= 1; e += 2)
        {
            float swing = MathF.Max(0f, MathF.Cos(ph) * e) > 0.95f ? 0.4f : 0f;
            var hp = o + new Vector2(e * 520f * u, 40f * u);
            var paddle = DrawKit.Place(Pts(Vector2.Zero, u, u, -70f, -170f, 70f, -170f, 80f, 60f, 20f, 90f, 20f, 200f, -20f, 200f, -20f, 90f, -80f, 60f), hp, e * (0.3f + swing));
            Poly(dc, b, paddle, e < 0 ? C1(a) : Rgb(0.1f, 0.3f, 0.7f, a));
            Poly(dc, b, DrawKit.Place(Pts(Vector2.Zero, u, u, -50f, -140f, 50f, -140f, 55f, 40f, -55f, 40f), hp, e * (0.3f + swing)), C2(0.85f * a));
        }
        Dot(dc, b, shuttle, 16f * u, Rgb(0.1f, 0.1f, 0.1f, a));
        for (int i = 0; i < 3; i++) Poly(dc, b, Pts(shuttle, u, u, (i - 1) * 14f, -8f, (i - 1) * 24f, -50f, (i - 1) * 24f + 10f, -50f), Col(BubbleKit.Palette(0, i), a));
    }
}

// =====================================================================
//  クリスマスツリーの点灯
//   ツリーの電球が上から順に光っていき、最後にてっぺんの星が輝く
// =====================================================================
public sealed class TreeLightUpPlugin : QuickShapePlugin
{
    public override string Name => "演出：クリスマスツリーの点灯";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TreeLightUpParameter(sharedData);
}

public sealed class TreeLightUpParameter : QuickShapeParameter
{
    public TreeLightUpParameter() : this(null) { }
    public TreeLightUpParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 40;
        Color1 = Hex(0x2E7D32);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "電球の数", Min: 10, Max: 120, Group: "「クリスマスツリーの点灯」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ツリーの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "星の色"),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var top = o + new Vector2(0, -380f * u);
        for (int L = 0; L < 4; L++)
        {
            float y0 = top.Y + L * 160f * u, y1 = y0 + 260f * u, hw = (140f + L * 90f) * u;
            Poly(dc, b, new List<Vector2> { new(o.X, y0), new(o.X + hw, y1), new(o.X - hw, y1) }, Col(Gfx.Mix(Color1, Hex(0x1B5E20), L * 0.15f), a));
        }
        Box(dc, b, o.X - 40f * u, top.Y + 740f * u, o.X + 40f * u, top.Y + 840f * u, Rgb(0.45f, 0.3f, 0.15f, a));
        float p = Intro(c, 0.3f, 2.2f / MathF.Max(0.1f, Spd(c)));
        for (int i = 0; i < Count; i++)
        {
            float f = (i + 0.5f) / Count;
            float y = top.Y + 60f * u + f * 660f * u;
            float hw = (60f + f * 380f) * u * 0.85f;
            var q = new Vector2(o.X + (DrawKit.H(Seed, i) - 0.5f) * 2f * hw * (0.4f + 0.6f * DrawKit.H(Seed, i, 2)), y);
            bool lit = f <= p;
            var col = BubbleKit.Palette(0, i);
            if (lit) DrawKit.Glow(dc, q, 34f * u, Col(col, 0.6f * a * (0.7f + 0.3f * MathF.Sin(t * 5f + i))));
            Dot(dc, b, q, 9f * u, lit ? Col(col, a) : Rgb(0.3f, 0.3f, 0.3f, a));
        }
        float sp = Intro(c, 0.3f + 2.2f / MathF.Max(0.1f, Spd(c)), 0.4f);
        if (sp > 0f) DrawKit.Glow(dc, top, 200f * u * sp, C3(0.6f * a));
        Star(dc, b, top, 60f * u * (sp > 0f ? 1f + 0.08f * MathF.Sin(t * 6f) : 1f), 5, 0.45f, 0f, sp > 0f ? C3(a) : Rgb(0.5f, 0.45f, 0.3f, a));
    }
}

// =====================================================================
//  花火大会の屋台の列
//   夜空に花火、手前にのれんの屋台がずらり。ちょうちんが灯る
// =====================================================================
public sealed class FestivalStallsPlugin : QuickShapePlugin
{
    public override string Name => "演出：花火大会の屋台の列";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FestivalStallsParameter(sharedData);
}

public sealed class FestivalStallsParameter : QuickShapeParameter
{
    public FestivalStallsParameter() : this(null) { }
    public FestivalStallsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "たこ焼き,わたあめ,金魚すくい,焼きそば,かき氷";
        Color1 = Hex(0x0B1026);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "屋台の名前", Tip: "カンマ区切り", Group: "「花火大会の屋台の列」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "夜空の色"),
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
        Sky(dc, w, h, C1(a), Col(Hex(0x2A1B3D), a));
        for (int i = 0; i < 3; i++)
        {
            float per = 2.2f, ph = Gfx.Mod(t + i * 0.73f, per) / per;
            var fc = new Vector2(w * (0.2f + 0.3f * i), h * (0.2f + 0.08f * (i % 2)));
            if (ph < 0.25f) { Ln(dc, b, new Vector2(fc.X, h * 0.6f), new Vector2(fc.X, h * 0.6f - (h * 0.6f - fc.Y) * ph / 0.25f), Rgb(1, 0.9f, 0.7f, a), 3f * s); continue; }
            float f = (ph - 0.25f) / 0.75f;
            var col = BubbleKit.Palette(0, i + (int)(t / per));
            for (int k = 0; k < 24; k++) Dot(dc, b, Pol(fc, 200f * s * DrawKit.EaseOut(f), k * MathF.PI / 12f) + new Vector2(0, f * f * 60f * s), 5f * s, Col(col, (1f - f) * a));
        }
        var it = Items;
        int n = Math.Max(1, it.Count);
        float sw = w / n;
        for (int i = 0; i < n; i++)
        {
            float x0 = i * sw + 10f * s, x1 = (i + 1) * sw - 10f * s, y0 = h * 0.6f;
            Box(dc, b, x0, y0, x1, h, Rgb(0.35f, 0.22f, 0.12f, a));
            Poly(dc, b, new List<Vector2> { new(x0 - 20f * s, y0), new(x1 + 20f * s, y0), new(x1, y0 - 60f * s), new(x0, y0 - 60f * s) }, Col(BubbleKit.Palette(0, i), a));
            for (int k = 0; k < 6; k++) Box(dc, b, x0 + (x1 - x0) * k / 6f, y0, x0 + (x1 - x0) * (k + 0.5f) / 6f, y0 + 120f * s, Rgb(1, 1, 1, 0.85f * a));
            Say(dc, b, i < it.Count ? it[i] : "", 40f * s, new Vector2((x0 + x1) / 2f, y0 + 60f * s), Rgb(0.7f, 0.1f, 0.1f, a), maxW: (x1 - x0) * 0.9f);
            var lp = new Vector2((x0 + x1) / 2f, y0 - 100f * s);
            DrawKit.Glow(dc, lp, 90f * s, Rgb(1f, 0.6f, 0.3f, 0.4f * a));
            Oval(dc, b, lp, 26f * s, 34f * s, Rgb(0.95f, 0.3f, 0.2f, a));
            Box(dc, b, x0, y0 + 140f * s, x1, y0 + 160f * s, Rgb(1f, 0.85f, 0.6f, 0.35f * a));
        }
    }
}

// =====================================================================
//  雪まつり・かまくら
//   雪の積もった夜に、中に灯りのともったかまくら。雪がしんしんと降る
// =====================================================================
public sealed class SnowFestivalPlugin : QuickShapePlugin
{
    public override string Name => "演出：雪まつり・かまくら";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SnowFestivalParameter(sharedData);
}

public sealed class SnowFestivalParameter : QuickShapeParameter
{
    public SnowFestivalParameter() : this(null) { }
    public SnowFestivalParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 120;
        Color1 = Hex(0xFFFFFF);
        Color2 = Hex(0xFFB74D);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "降る雪の数", Min: 0, Max: 400, Group: "「雪まつり・かまくら」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "雪の色"),
        ["Color2"] = new ParamGuide(Label: "灯りの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        Sky(dc, w, h, Rgb(0.05f, 0.08f, 0.2f, a), Rgb(0.2f, 0.25f, 0.4f, a));
        Box(dc, b, 0, h * 0.72f, w, h, C1(0.95f * a));
        for (int i = 0; i < 8; i++) Dot(dc, b, new Vector2(w * i / 7f, h * 0.72f), (80f + 50f * DrawKit.H(Seed, i)) * s, C1(0.95f * a));
        float R = 260f * u;
        var dome = new List<Vector2>(Gfx.ArcPts(o, R, MathF.PI, MathF.PI * 2f, 40));
        Poly(dc, b, dome, C1(a));
        Outline(dc, b, dome, Rgb(0.75f, 0.82f, 0.92f, a), 4f * u);
        var door = new List<Vector2>(Gfx.ArcPts(o + new Vector2(0, 0), R * 0.42f, MathF.PI, MathF.PI * 2f, 20));
        DrawKit.Glow(dc, o - new Vector2(0, R * 0.2f), R * 1.2f, C2(0.35f * a * (0.85f + 0.15f * MathF.Sin(t * 5f))));
        Poly(dc, b, door, C2(a));
        Poly(dc, b, Gfx.ArcPts(o, R * 0.3f, MathF.PI, MathF.PI * 2f, 14), Rgb(1f, 0.9f, 0.6f, a));
        for (int i = 0; i < Count; i++)
        {
            float x = DrawKit.H(Seed, i) * w + MathF.Sin(t + i) * 20f * s;
            float y = Gfx.Mod(DrawKit.H(Seed, i, 1) * h + t * (50f + 60f * DrawKit.H(Seed, i, 2)) * s * Spd(c), h + 20f) - 10f;
            Dot(dc, b, new Vector2(x, y), (2f + 3f * DrawKit.H(Seed, i, 3)) * s, C1(0.9f * a));
        }
    }
}

// =====================================================================
//  母の日・カーネーション
//   赤いカーネーションの花束と「ありがとう」のメッセージ。ハートがふわふわ
// =====================================================================
public sealed class CarnationMothersDayPlugin : QuickShapePlugin
{
    public override string Name => "演出：母の日・カーネーション";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CarnationMothersDayParameter(sharedData);
}

public sealed class CarnationMothersDayParameter : QuickShapeParameter
{
    public CarnationMothersDayParameter() : this(null) { }
    public CarnationMothersDayParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "いつもありがとう";
        Count = 3;
        Color1 = Hex(0xE53950);
        Color2 = Hex(0xFFC1CC);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "メッセージ", Group: "「母の日・カーネーション」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "花の数", Min: 1, Max: 7, Group: "「母の日・カーネーション」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "花の色"),
        ["Color2"] = new ParamGuide(Label: "ハートの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Clamp(Count, 1, 7);
        var bot = o + new Vector2(0, 300f * u);
        for (int i = 0; i < n; i++)
        {
            float f = n == 1 ? 0f : i / (float)(n - 1) - 0.5f;
            var head = o + new Vector2(f * 300f * u, -60f * u - MathF.Abs(f) * -60f * u + MathF.Sin(t * 1.5f + i) * 6f * u);
            Ln(dc, b, bot, head, Rgb(0.3f, 0.55f, 0.25f, a), 8f * u);
            float k = DrawKit.EaseOutBack(Intro(c, i * 0.15f, 0.4f));
            for (int j = 0; j < 3; j++)
            {
                var pts = new List<Vector2>();
                float r0 = (80f - j * 20f) * u * k;
                for (int z = 0; z <= 24; z++) { float an = MathF.PI * (1f + z / 24f); float zig = (z % 2 == 0 ? 1f : 0.82f); pts.Add(Pol(head + new Vector2(0, -j * 18f * u), r0 * zig, an)); }
                pts.Add(head + new Vector2(0, 20f * u));
                Poly(dc, b, pts, Col(Gfx.Mix(Color1, Hex(0x7F0000), j * 0.15f), a));
            }
            Poly(dc, b, Pts(head + new Vector2(0, 30f * u), u * k, u * k, -30f, 0f, 30f, 0f, 0f, 40f), Rgb(0.3f, 0.55f, 0.25f, a));
        }
        Poly(dc, b, Pts(bot + new Vector2(0, -60f * u), u, u, -120f, -60f, 120f, -60f, 30f, 80f, -30f, 80f), Rgb(1f, 0.93f, 0.8f, a));
        Poly(dc, b, Pts(bot + new Vector2(0, -30f * u), u, u, -40f, -16f, 40f, -16f, 40f, 16f, -40f, 16f), C1(a));
        for (int i = 0; i < 8; i++)
        {
            float ph = Gfx.Mod(t * 0.3f + DrawKit.H(Seed, i), 1f);
            Heart(dc, b, o + new Vector2((DrawKit.H(Seed, i, 1) - 0.5f) * 900f * u, 300f * u - ph * 700f * u), (16f + 16f * DrawKit.H(Seed, i, 2)) * u, MathF.Sin(t + i) * 0.3f, C2(a * MathF.Sin(ph * MathF.PI)));
        }
        Say(dc, b, Str, 70f * u, o + new Vector2(0, -330f * u), C1(a), Rgb(1, 1, 1, a), 5f * u);
    }
}

// =====================================================================
//  年越しのカウントダウン
//   「3・2・1」と数字が大きく出て、最後に「HAPPY NEW YEAR」と紙吹雪。「数」から数える
// =====================================================================
public sealed class NewYearCountdownPlugin : QuickShapePlugin
{
    public override string Name => "演出：年越しのカウントダウン";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NewYearCountdownParameter(sharedData);
}

public sealed class NewYearCountdownParameter : QuickShapeParameter
{
    public NewYearCountdownParameter() : this(null) { }
    public NewYearCountdownParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 3;
        Text = "HAPPY NEW YEAR";
        Color1 = Hex(0xFFD54A);
        Color2 = Hex(0x0D1B3D);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "最後の文字", Group: "「年越しのカウントダウン」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "数え始める数", Min: 1, Max: 10, Group: "「年越しのカウントダウン」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "夜空の背景", Group: "「年越しのカウントダウン」の設定"),
        ["Color1"] = new ParamGuide(Label: "文字の色"),
        ["Color2"] = new ParamGuide(Label: "背景の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        if (Background) Sky(dc, w, h, C2(a), Col(Gfx.Mix(Color2, Hex(0x000000), 0.4f), a));
        int n = Math.Clamp(Count, 1, 10);
        float tt = c.Time;
        int idx = (int)MathF.Floor(tt);
        float fr = tt - idx;
        if (idx < n)
        {
            int num = n - idx;
            float k = 1.6f - 0.6f * DrawKit.EaseOut(Math.Clamp(fr * 3f, 0f, 1f));
            Ring(dc, b, o, 260f * u, C1(0.3f * a), 10f * u);
            Path(dc, b, Gfx.ArcPts(o, 260f * u, -MathF.PI / 2f, -MathF.PI / 2f + MathF.PI * 2f * (1f - fr), 60), C1(a), 10f * u);
            Say(dc, b, num.ToString(), 300f * u, o, C1(a * (1f - fr * 0.4f)), Rgb(0, 0, 0, 0.5f * a), 6f * u, scale: k);
            return;
        }
        float f = tt - n;
        float pop = DrawKit.EaseOutBack(Math.Clamp(f * 2f, 0f, 1f));
        Say(dc, b, Str, 160f * u, o, C1(a), Rgb(0.5f, 0.3f, 0f, a), 8f * u, scale: pop);
        for (int i = 0; i < 80; i++)
        {
            float g = Math.Clamp(f / 3f, 0f, 1f);
            float ang = DrawKit.H(Seed, i) * MathF.PI * 2f;
            var q = Pol(o, (200f + 900f * DrawKit.H(Seed, i, 1)) * u * DrawKit.EaseOut(Math.Clamp(f * 1.5f, 0f, 1f)), ang) + new Vector2(MathF.Sin(t * 3f + i) * 20f * u, g * g * 600f * u);
            Poly(dc, b, DrawKit.Place(DrawKit.RectPts(-12f * u, -6f * u, 24f * u, 12f * u), q, t * 5f + i), Col(BubbleKit.Palette(0, i), a * (1f - g * 0.6f)));
        }
    }
}
