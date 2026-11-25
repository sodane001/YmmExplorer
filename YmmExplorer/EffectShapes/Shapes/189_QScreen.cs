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
//  フィルムの光漏れ
//   画面のはしから、オレンジや赤のぼんやりした光がゆらゆら入る。昔のフィルム風
// =====================================================================
public sealed class FilmLightLeakPlugin : QuickShapePlugin
{
    public override string Name => "演出：フィルムの光漏れ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FilmLightLeakParameter(sharedData);
}

public sealed class FilmLightLeakParameter : QuickShapeParameter
{
    public FilmLightLeakParameter() : this(null) { }
    public FilmLightLeakParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF6F00);
        Color2 = Hex(0xFF1744);
        Color3 = Hex(0xFFD180);
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
        ["Color1"] = new ParamGuide(Label: "光の色1"),
        ["Color2"] = new ParamGuide(Label: "光の色2"),
        ["Color3"] = new ParamGuide(Label: "光の色3"),
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
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        var cols = new[] { Color1, Color2, Color3 };
        for (int i = 0; i < 4; i++)
        {
            float ph = t * 0.3f * Spd(c) + i * 1.7f;
            var q = new Vector2(i % 2 == 0 ? w * (0.05f + 0.1f * MathF.Sin(ph)) : w * (0.95f - 0.1f * MathF.Sin(ph)), h * (0.5f + 0.4f * MathF.Sin(ph * 0.7f + i)));
            float k = 0.5f + 0.5f * MathF.Sin(ph * 1.3f);
            DrawKit.Glow(dc, q, (500f + 300f * k) * s, Col(cols[i % 3], 0.35f * k * a));
        }
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
    }
}

// =====================================================================
//  アイリスイン・アウト
//   丸い穴がしぼんで画面が黒くなる(バリエーション2で広がって明ける)。「値」が穴の位置の横
// =====================================================================
public sealed class IrisWipePlugin : QuickShapePlugin
{
    public override string Name => "演出：アイリスイン・アウト";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new IrisWipeParameter(sharedData);
}

public sealed class IrisWipeParameter : QuickShapeParameter
{
    public IrisWipeParameter() : this(null) { }
    public IrisWipeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x000000);
    }
    protected override double DefAmount => 50;
    protected override double DefAmount2 => 50;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "穴の中心の横位置", Group: "「アイリスイン・アウト」の設定"),
        ["Amount2"] = new ParamGuide(Label: "穴の中心の縦位置", Group: "「アイリスイン・アウト」の設定"),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=しぼんで閉じる　2=広がって開く", Min: 1, Max: 2, Group: "「アイリスイン・アウト」の設定"),
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
        float dur = MathF.Max(0.1f, (float)c.Length / MathF.Max(1, c.Fps));
        float p = Math.Clamp(c.Time / dur, 0f, 1f);
        float k = Variant >= 2 ? DrawKit.EaseOut(p) : 1f - DrawKit.EaseOut(p);
        var q = new Vector2(w * Amt(c), h * Amt2(c));
        float R = MathF.Sqrt(w * w + h * h) * k;
        b.Color = C1(a);
        if (R < 1f) { Box(dc, b, -2, -2, w + 2, h + 2, C1(a)); return; }
        DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), DrawKit.EllipsePts(q, R, R, 96));
    }
}

// =====================================================================
//  のぞき穴
//   ドアののぞき穴から見たような、丸い視界とゆがんだふち
// =====================================================================
public sealed class PeepholePlugin : QuickShapePlugin
{
    public override string Name => "演出：のぞき穴";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PeepholeParameter(sharedData);
}

public sealed class PeepholeParameter : QuickShapeParameter
{
    public PeepholeParameter() : this(null) { }
    public PeepholeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x050505);
    }
    protected override double DefAmount => 40;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "穴の大きさ", Group: "「のぞき穴」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "まわりの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = MathF.Min(w, h) * (0.2f + 0.35f * Amt(c));
        b.Color = C1(a);
        DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), DrawKit.EllipsePts(o, R, R, 96));
        using (var g = Gfx.Radial(dc, o, R, (0f, Rgb(0, 0, 0, 0f)), (0.7f, Rgb(0, 0, 0, 0.1f * a)), (1f, Rgb(0, 0, 0, 0.85f * a))))
            dc.FillEllipse(new Ellipse(o, R, R), g);
        Ring(dc, b, o, R, Rgb(0.3f, 0.3f, 0.3f, a), 10f * U0(c));
    }
}

// =====================================================================
//  双眼鏡の視界
//   双眼鏡をのぞいた、丸が2つつながった視界。少しゆれる
// =====================================================================
public sealed class BinocularsPlugin : QuickShapePlugin
{
    public override string Name => "演出：双眼鏡の視界";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BinocularsParameter(sharedData);
}

public sealed class BinocularsParameter : QuickShapeParameter
{
    public BinocularsParameter() : this(null) { }
    public BinocularsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x000000);
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
        ["Color1"] = new ParamGuide(Label: "まわりの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = h * 0.38f * c.F(Scale) / 100f;
        var q = o + new Vector2(MathF.Sin(t * 0.9f) * 10f, MathF.Cos(t * 1.1f) * 8f) * U0(c);
        var lc = q - new Vector2(R * 0.78f, 0); var rc = q + new Vector2(R * 0.78f, 0);
        var hole = new List<Vector2>();
        float join = MathF.Acos(0.78f);
        hole.AddRange(Gfx.ArcPts(rc, R, -MathF.PI + join, MathF.PI - join, 60));
        hole.AddRange(Gfx.ArcPts(lc, R, join, MathF.PI * 2f - join, 60));
        b.Color = C1(a);
        DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), hole);
        Outline(dc, b, hole, Rgb(0.1f, 0.1f, 0.1f, a), 14f * U0(c));
    }
}

// =====================================================================
//  監視カメラの画面
//   「REC●」と日付・時刻、カメラ番号、走査線。白黒っぽい監視カメラ風の枠
// =====================================================================
public sealed class SecurityCamPlugin : QuickShapePlugin
{
    public override string Name => "演出：監視カメラの画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SecurityCamParameter(sharedData);
}

public sealed class SecurityCamParameter : QuickShapeParameter
{
    public SecurityCamParameter() : this(null) { }
    public SecurityCamParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "CAM 03";
        Color1 = Hex(0xFFFFFF);
        Color2 = Hex(0xFF1744);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "カメラの名前", Group: "「監視カメラの画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "画面を緑っぽく暗くする", Group: "「監視カメラの画面」の設定"),
        ["Color1"] = new ParamGuide(Label: "文字と枠の色"),
        ["Color2"] = new ParamGuide(Label: "RECの色"),
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
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(0.1f, 0.15f, 0.1f, 0.25f * a));
        for (float y = Gfx.Mod(t * 40f * s, 6f * s); y < h; y += 6f * s) Box(dc, b, 0, y, w, y + 2f * s, Rgb(0, 0, 0, 0.12f * a));
        float bar = Gfx.Mod(t * 0.15f, 1.2f) * h;
        Box(dc, b, 0, bar, w, bar + 40f * s, Rgb(1, 1, 1, 0.05f * a));
        float L = 60f * s, m = 50f * s;
        foreach (var (x, y, dx, dy) in new[] { (m, m, 1f, 1f), (w - m, m, -1f, 1f), (m, h - m, 1f, -1f), (w - m, h - m, -1f, -1f) })
        {
            Ln(dc, b, new Vector2(x, y), new Vector2(x + dx * L, y), C1(a), 4f * s);
            Ln(dc, b, new Vector2(x, y), new Vector2(x, y + dy * L), C1(a), 4f * s);
        }
        if ((int)(t * 1.5f) % 2 == 0) Dot(dc, b, new Vector2(100f * s, 105f * s), 12f * s, C2(a));
        SayAt(dc, b, "REC", 36f * s, new Vector2(125f * s, 84f * s), C1(a), false);
        SayAt(dc, b, Str, 36f * s, new Vector2(w - 260f * s, 84f * s), C1(a), false);
        int sec = (int)c.Time + 13 * 3600 + 25 * 60;
        SayAt(dc, b, $"2026/11/22  {sec / 3600 % 24:00}:{sec / 60 % 60:00}:{sec % 60:00}", 34f * s, new Vector2(100f * s, h - 120f * s), C1(a), false);
    }
}

// =====================================================================
//  暗視ゴーグル
//   画面全体を緑色にして、ざらつきとまるい視界。重ねた映像が暗視ゴーグル風に
// =====================================================================
public sealed class NightVisionPlugin : QuickShapePlugin
{
    public override string Name => "演出：暗視ゴーグル";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NightVisionParameter(sharedData);
}

public sealed class NightVisionParameter : QuickShapeParameter
{
    public NightVisionParameter() : this(null) { }
    public NightVisionParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x39FF14);
        Count = 300;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "ざらつきの量", Min: 0, Max: 1500, Group: "「暗視ゴーグル」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "暗視の色"),
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
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        Box(dc, b, 0, 0, w, h, C1(0.25f * a));
        int fr = (int)(t * 24f);
        for (int i = 0; i < Count; i++) Box(dc, b, DrawKit.H(Seed + fr, i) * w, DrawKit.H(Seed + fr, i, 1) * h, DrawKit.H(Seed + fr, i) * w + 3f * s, DrawKit.H(Seed + fr, i, 1) * h + 3f * s, C1(0.4f * DrawKit.H(Seed + fr, i, 2) * a));
        for (float y = 0; y < h; y += 4f * s) Box(dc, b, 0, y, w, y + 1.5f * s, Rgb(0, 0, 0, 0.15f * a));
        var cc = new Vector2(w, h) / 2f;
        float R = MathF.Max(w, h) * 0.55f;
        using (var g = Gfx.Radial(dc, cc, R, (0f, Rgb(0, 0, 0, 0f)), (0.75f, Rgb(0, 0, 0, 0.3f * a)), (1f, Rgb(0, 0, 0, a))))
            dc.FillRectangle(new RawRectF(0, 0, w, h), g);
    }
}

// =====================================================================
//  サーモグラフィー風
//   青→緑→黄→赤の熱っぽい色のもやが、ゆっくり動く。温度の目盛りつき
// =====================================================================
public sealed class ThermalLookPlugin : QuickShapePlugin
{
    public override string Name => "演出：サーモグラフィー風";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ThermalLookParameter(sharedData);
}

public sealed class ThermalLookParameter : QuickShapeParameter
{
    public ThermalLookParameter() : this(null) { }
    public ThermalLookParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 6;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "熱のかたまりの数", Min: 1, Max: 20, Group: "「サーモグラフィー風」の設定"),
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
        Box(dc, b, 0, 0, w, h, Rgb(0.05f, 0.0f, 0.35f, 0.9f * a));
        for (int i = 0; i < Count; i++)
        {
            var q = new Vector2(w * (0.2f + 0.6f * DrawKit.H(Seed, i)) + MathF.Sin(t * 0.5f + i) * 80f * s, h * (0.2f + 0.6f * DrawKit.H(Seed, i, 1)) + MathF.Cos(t * 0.4f + i) * 60f * s);
            float r = (200f + 200f * DrawKit.H(Seed, i, 2)) * s;
            var stops = new[] { (1f, Rgb(0.1f, 0.3f, 1f, 0.6f)), (0.75f, Rgb(0.1f, 0.9f, 0.5f, 0.7f)), (0.5f, Rgb(1f, 0.95f, 0.1f, 0.8f)), (0.28f, Rgb(1f, 0.3f, 0.05f, 0.9f)), (0.1f, Rgb(1f, 1f, 1f, 0.9f)) };
            foreach (var (k, col) in stops) Dot(dc, b, q, r * k, new Color4(col.R, col.G, col.B, col.A * a * 0.6f));
        }
        float bx = w - 90f * s;
        Shade(dc, new Vector2(0, 100f * s), new Vector2(0, h - 100f * s), Rgb(1, 1, 1, a), Rgb(0.1f, 0.1f, 0.8f, a), bx, 100f * s, bx + 30f * s, h - 100f * s);
        SayAt(dc, b, "40℃", 26f * s, new Vector2(bx - 80f * s, 90f * s), Rgb(1, 1, 1, a), false);
        SayAt(dc, b, "10℃", 26f * s, new Vector2(bx - 80f * s, h - 130f * s), Rgb(1, 1, 1, a), false);
    }
}

// =====================================================================
//  夢・回想のふち
//   画面のまわりが白くぼんやりして、もこもこ雲のふちがゆれる。回想や夢の場面に
// =====================================================================
public sealed class DreamEdgePlugin : QuickShapePlugin
{
    public override string Name => "演出：夢・回想のふち";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DreamEdgeParameter(sharedData);
}

public sealed class DreamEdgeParameter : QuickShapeParameter
{
    public DreamEdgeParameter() : this(null) { }
    public DreamEdgeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFFFFF);
    }
    protected override double DefAmount => 50;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "ふちの太さ", Group: "「夢・回想のふち」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ふちの色"),
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
        float m = MathF.Min(w, h) * (0.08f + 0.25f * Amt(c));
        var hole = new List<Vector2>();
        int n = 120;
        for (int i = 0; i < n; i++)
        {
            float ang = i * MathF.PI * 2f / n;
            float bump = MathF.Abs(MathF.Sin(ang * 9f + t * 0.8f)) * m * 0.35f;
            hole.Add(new Vector2(w / 2f + MathF.Cos(ang) * (w / 2f - m + bump), h / 2f + MathF.Sin(ang) * (h / 2f - m + bump)));
        }
        b.Color = C1(0.85f * a);
        DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), hole);
        var cc = new Vector2(w, h) / 2f;
        using (var g = Gfx.Radial(dc, cc, MathF.Max(w, h) * 0.6f, (0.55f, Rgb(1, 1, 1, 0f)), (1f, C1(0.5f * a))))
            dc.FillRectangle(new RawRectF(0, 0, w, h), g);
    }
}

// =====================================================================
//  雨にぬれたレンズ
//   カメラのレンズに水滴がつき、ときどきツーッと流れ落ちる
// =====================================================================
public sealed class WetLensPlugin : QuickShapePlugin
{
    public override string Name => "演出：雨にぬれたレンズ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WetLensParameter(sharedData);
}

public sealed class WetLensParameter : QuickShapeParameter
{
    public WetLensParameter() : this(null) { }
    public WetLensParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 40;
        Color1 = Hex(0xDDEEFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "水滴の数", Min: 1, Max: 150, Group: "「雨にぬれたレンズ」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "水滴の色"),
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
        for (int i = 0; i < Count; i++)
        {
            float r = (6f + 26f * DrawKit.H(Seed, i, 2)) * s;
            var q = new Vector2(DrawKit.H(Seed, i) * w, DrawKit.H(Seed, i, 1) * h);
            bool runs = DrawKit.H(Seed, i, 3) < 0.25f;
            if (runs)
            {
                float ph = Gfx.Mod(t * 0.15f + DrawKit.H(Seed, i, 4), 1f);
                q.Y = Gfx.Mod(q.Y + ph * h, h);
                Ln(dc, b, q - new Vector2(0, 200f * s * ph), q, C1(0.2f * a), r * 0.6f);
            }
            Oval(dc, b, q, r, r * 1.1f, C1(0.18f * a));
            Path(dc, b, Gfx.ArcPts(q, r * 0.95f, 0.3f, 2.4f, 10), Rgb(0, 0, 0, 0.18f * a), MathF.Max(1f, r * 0.15f));
            Dot(dc, b, q + new Vector2(-r * 0.35f, -r * 0.4f), r * 0.22f, Rgb(1, 1, 1, 0.7f * a));
        }
    }
}

// =====================================================================
//  ハーフトーン（網点）
//   アメコミのような、大きさのちがう点の模様。ななめに向かって点が小さくなる
// =====================================================================
public sealed class HalftonePlugin : QuickShapePlugin
{
    public override string Name => "演出：ハーフトーン（網点）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HalftoneParameter(sharedData);
}

public sealed class HalftoneParameter : QuickShapeParameter
{
    public HalftoneParameter() : this(null) { }
    public HalftoneParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF4081);
    }
    protected override double DefAmount => 30;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "点の大きさ", Group: "「ハーフトーン（網点）」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "点の色"),
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
        float cell = (14f + 40f * Amt(c)) * s;
        float ang = 0.4f, ca = MathF.Cos(ang), sa = MathF.Sin(ang);
        int nx = (int)(w / cell) + 4, ny = (int)(h / cell) + 4;
        for (int j = -2; j < ny; j++)
            for (int i = -2; i < nx; i++)
            {
                var q = new Vector2(i * cell + (j % 2) * cell / 2f, j * cell * 0.87f);
                float f = Math.Clamp((q.X * ca + q.Y * sa) / (w * ca + h * sa) + 0.1f * MathF.Sin(t + q.Y * 0.01f), 0f, 1f);
                Dot(dc, b, q, cell * 0.48f * (1f - f), C1(a));
            }
    }
}

// =====================================================================
//  漫画のコマ割りの枠
//   黒い太線で画面を漫画のコマに分ける。バリエーションで割り方が変わる
// =====================================================================
public sealed class MangaPanelsPlugin : QuickShapePlugin
{
    public override string Name => "演出：漫画のコマ割りの枠";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MangaPanelsParameter(sharedData);
}

public sealed class MangaPanelsParameter : QuickShapeParameter
{
    public MangaPanelsParameter() : this(null) { }
    public MangaPanelsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x000000);
        Color2 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=上2つ下1つ　2=斜めの3コマ　3=横4段", Min: 1, Max: 3, Group: "「漫画のコマ割りの枠」の設定"),
        ["Background"] = new ParamGuide(Label: "コマの間を白く塗る", Group: "「漫画のコマ割りの枠」の設定"),
        ["Color1"] = new ParamGuide(Label: "枠線の色"),
        ["Color2"] = new ParamGuide(Label: "コマの間の色"),
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
        float m = 30f * s, g = 26f * s, lw = 8f * s;
        var panels = new List<List<Vector2>>();
        float sl = 60f * s;
        if (Variant == 2)
        {
            panels.Add(new List<Vector2> { new(m, m), new(w - m, m), new(w - m, h * 0.5f - g / 2f - sl), new(m, h * 0.5f - g / 2f + sl) });
            panels.Add(new List<Vector2> { new(m, h * 0.5f + g / 2f + sl), new(w * 0.45f - g / 2f, h * 0.5f + g / 2f + sl * 0.1f), new(w * 0.5f - g / 2f, h - m), new(m, h - m) });
            panels.Add(new List<Vector2> { new(w * 0.45f + g / 2f, h * 0.5f + g / 2f + sl * 0.1f - 2f * s), new(w - m, h * 0.5f + g / 2f - sl), new(w - m, h - m), new(w * 0.5f + g / 2f, h - m) });
        }
        else if (Variant == 3)
        {
            for (int i = 0; i < 4; i++) { float y0 = m + (h - 2f * m + g) * i / 4f, y1 = m + (h - 2f * m + g) * (i + 1) / 4f - g; panels.Add(new List<Vector2> { new(m, y0), new(w - m, y0), new(w - m, y1), new(m, y1) }); }
        }
        else
        {
            panels.Add(new List<Vector2> { new(m, m), new(w * 0.6f - g / 2f + sl, m), new(w * 0.6f - g / 2f - sl, h * 0.55f - g / 2f), new(m, h * 0.55f - g / 2f) });
            panels.Add(new List<Vector2> { new(w * 0.6f + g / 2f + sl, m), new(w - m, m), new(w - m, h * 0.55f - g / 2f), new(w * 0.6f + g / 2f - sl, h * 0.55f - g / 2f) });
            panels.Add(new List<Vector2> { new(m, h * 0.55f + g / 2f), new(w - m, h * 0.55f + g / 2f), new(w - m, h - m), new(m, h - m) });
        }
        if (Background)
        {
            b.Color = C2(a);
            DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), DrawKit.RectPts(m, m, w - 2f * m, h - 2f * m));
            foreach (var pn in panels) Outline(dc, b, pn, C2(a), g + lw);
        }
        for (int i = 0; i < panels.Count; i++)
        {
            float p = Intro(c, i * 0.15f, 0.3f);
            if (p <= 0f) continue;
            Outline(dc, b, panels[i], C1(a * p), lw);
        }
    }
}

// =====================================================================
//  舞台の幕（開く・閉じる）
//   赤いひだのある幕が左右に開く(バリエーション2で閉じる)。上に飾り幕
// =====================================================================
public sealed class StageCurtainPlugin : QuickShapePlugin
{
    public override string Name => "演出：舞台の幕（開く・閉じる）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StageCurtainParameter(sharedData);
}

public sealed class StageCurtainParameter : QuickShapeParameter
{
    public StageCurtainParameter() : this(null) { }
    public StageCurtainParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xB71C1C);
        Color2 = Hex(0xFFC107);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=開く　2=閉じる", Min: 1, Max: 2, Group: "「舞台の幕（開く・閉じる）」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "幕の色"),
        ["Color2"] = new ParamGuide(Label: "飾りひもの色"),
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
        float p = DrawKit.EaseOut(Intro(c, 0.2f, 1.5f / MathF.Max(0.1f, Spd(c))));
        if (Variant >= 2) p = 1f - p;
        float cw = w / 2f * (1f - p * 0.85f);
        for (int side = 0; side < 2; side++)
        {
            int folds = 8;
            for (int i = 0; i < folds; i++)
            {
                float f0 = i / (float)folds, f1 = (i + 1) / (float)folds;
                float x0 = side == 0 ? cw * f0 : w - cw * f0, x1 = side == 0 ? cw * f1 : w - cw * f1;
                float sway = MathF.Sin(t * 2f + i) * 6f * s * p;
                var col = Gfx.Mix(Color1, Hex(0x3E0000), i % 2 == 0 ? 0f : 0.35f);
                Poly(dc, b, new List<Vector2> { new(x0, 0), new(x1, 0), new(x1 + sway, h), new(x0 + sway, h) }, Col(col, a));
            }
        }
        var top = new List<Vector2> { new(-2, -2), new(w + 2, -2) };
        for (int i = 0; i <= 12; i++) top.Add(new Vector2(w - w * i / 12f, 130f * s + (i % 2 == 0 ? 0f : 40f * s)));
        Poly(dc, b, top, Col(Gfx.Mix(Color1, Hex(0x3E0000), 0.15f), a));
        Path(dc, b, top.Skip(2).ToList(), C2(a), 8f * s);
    }
}

// =====================================================================
//  ポラロイド写真の枠
//   白いふちのポラロイド写真。下の余白に手書き風の文字。少しかたむいて置かれる
// =====================================================================
public sealed class PolaroidFramePlugin : QuickShapePlugin
{
    public override string Name => "演出：ポラロイド写真の枠";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PolaroidFrameParameter(sharedData);
}

public sealed class PolaroidFrameParameter : QuickShapeParameter
{
    public PolaroidFrameParameter() : this(null) { }
    public PolaroidFrameParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "2026.11 旅行";
        Color1 = Hex(0xFAFAF5);
        Background = false;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "下に書く文字", Group: "「ポラロイド写真の枠」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "写真の部分も白く塗る", Group: "「ポラロイド写真の枠」の設定"),
        ["Color1"] = new ParamGuide(Label: "枠の色"),
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
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.4f));
        float pw = 700f * u * k, ph = 820f * u * k, side = 36f * u * k, bottom = 160f * u * k;
        var prev = dc.Transform;
        dc.Transform = Matrix3x2.CreateRotation(-0.06f + (1f - k) * 0.4f, o) * prev;
        Box(dc, b, o.X - pw / 2f + 12f * u, o.Y - ph / 2f + 16f * u, o.X + pw / 2f + 12f * u, o.Y + ph / 2f + 16f * u, Rgb(0, 0, 0, 0.25f * a));
        var outer = DrawKit.RectPts(o.X - pw / 2f, o.Y - ph / 2f, pw, ph);
        var inner = DrawKit.RectPts(o.X - pw / 2f + side, o.Y - ph / 2f + side, pw - side * 2f, ph - side - bottom);
        b.Color = C1(a);
        if (Background) Poly(dc, b, outer, C1(a)); else DrawKit.FillWithHole(dc, b, outer, inner);
        Frame(dc, b, inner[0].X, inner[0].Y, inner[2].X, inner[2].Y, Rgb(0, 0, 0, 0.15f * a), 2f * u);
        Say(dc, b, Str, 54f * u, new Vector2(o.X, o.Y + ph / 2f - bottom / 2f), Rgb(0.15f, 0.15f, 0.35f, a), bold: false, scale: k);
        Box(dc, b, o.X - 70f * u, o.Y - ph / 2f - 20f * u, o.X + 70f * u, o.Y - ph / 2f + 30f * u, Rgb(1f, 0.95f, 0.6f, 0.7f * a));
        dc.Transform = prev;
    }
}

// =====================================================================
//  スクリーントーン背景
//   漫画の背景のような、点や線のトーン。バリエーションで 点/斜線/集中線/グラデ点
// =====================================================================
public sealed class ScreenTonePlugin : QuickShapePlugin
{
    public override string Name => "演出：スクリーントーン背景";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ScreenToneParameter(sharedData);
}

public sealed class ScreenToneParameter : QuickShapeParameter
{
    public ScreenToneParameter() : this(null) { }
    public ScreenToneParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x000000);
    }
    protected override double DefAmount => 30;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "模様の細かさ", Group: "「スクリーントーン背景」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=点　2=斜線　3=集中線　4=だんだん大きくなる点", Min: 1, Max: 4, Group: "「スクリーントーン背景」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "トーンの色"),
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
        float cell = (8f + 20f * Amt(c)) * s;
        if (Variant == 2)
        {
            for (float x = -h; x < w + h; x += cell) Ln(dc, b, new Vector2(x, 0), new Vector2(x + h, h), C1(0.6f * a), cell * 0.25f);
        }
        else if (Variant == 3)
        {
            var cc = new Vector2(w, h) / 2f;
            for (int i = 0; i < 120; i++)
            {
                float ang = DrawKit.H(Seed, i) * MathF.PI * 2f, r0 = MathF.Min(w, h) * (0.25f + 0.15f * DrawKit.H(Seed, i, 1));
                Poly(dc, b, new List<Vector2> { Pol(cc, r0, ang), Pol(cc, MathF.Max(w, h), ang - 0.012f), Pol(cc, MathF.Max(w, h), ang + 0.012f) }, C1(0.8f * a));
            }
        }
        else
        {
            for (float y = 0; y < h + cell; y += cell * 0.87f)
                for (float x = 0; x < w + cell; x += cell)
                {
                    float xx = x + ((int)(y / (cell * 0.87f)) % 2) * cell / 2f;
                    float f = Variant == 4 ? y / h : 0.5f;
                    Dot(dc, b, new Vector2(xx, y), cell * 0.45f * f + 0.3f, C1(a));
                }
        }
    }
}
