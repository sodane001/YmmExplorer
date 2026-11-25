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
//  チェック・ストライプ柄
//   ゆっくり流れる柄の背景。バリエーションで 市松/ストライプ/ギンガム/ボーダー
// =====================================================================
public sealed class CheckStripePlugin : QuickShapePlugin
{
    public override string Name => "演出：チェック・ストライプ柄";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CheckStripeParameter(sharedData);
}

public sealed class CheckStripeParameter : QuickShapeParameter
{
    public CheckStripeParameter() : this(null) { }
    public CheckStripeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFE0E9);
        Color2 = Hex(0xFFFFFF);
    }
    protected override double DefAmount => 30;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "柄の大きさ", Group: "「チェック・ストライプ柄」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=市松　2=ストライプ　3=ギンガム　4=ボーダー", Min: 1, Max: 4, Group: "「チェック・ストライプ柄」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "柄の色"),
        ["Color2"] = new ParamGuide(Label: "地の色"),
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
        float cell = (40f + 200f * Amt(c)) * s;
        float off = Gfx.Mod(t * 30f * s * Spd(c), cell * 2f);
        Box(dc, b, 0, 0, w, h, C2(a));
        if (Variant == 2)
        {
            for (float x = -h - cell * 2f + off; x < w + h; x += cell * 2f) Poly(dc, b, new List<Vector2> { new(x, 0), new(x + cell, 0), new(x + cell + h, h), new(x + h, h) }, C1(a));
        }
        else if (Variant == 3)
        {
            for (float x = -cell * 2f + off; x < w + cell; x += cell * 2f) Box(dc, b, x, 0, x + cell, h, C1(0.5f * a));
            for (float y = -cell * 2f + off; y < h + cell; y += cell * 2f) Box(dc, b, 0, y, w, y + cell, C1(0.5f * a));
        }
        else if (Variant == 4)
        {
            for (float y = -cell * 2f + off; y < h + cell; y += cell * 2f) Box(dc, b, 0, y, w, y + cell, C1(a));
        }
        else
        {
            for (int j = -2; j * cell < h + cell; j++)
                for (int i = -2; i * cell < w + cell; i++)
                    if (((i + j) & 1) == 0) Box(dc, b, i * cell + off, j * cell + off, (i + 1) * cell + off, (j + 1) * cell + off, C1(a));
        }
    }
}

// =====================================================================
//  水玉ポップ背景
//   カラフルな水玉がふわふわ動く、明るい背景。「数」で水玉の数
// =====================================================================
public sealed class PopDotsPlugin : QuickShapePlugin
{
    public override string Name => "演出：水玉ポップ背景";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PopDotsParameter(sharedData);
}

public sealed class PopDotsParameter : QuickShapeParameter
{
    public PopDotsParameter() : this(null) { }
    public PopDotsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 40;
        Color1 = Hex(0xFFF59D);
        Color2 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "水玉の数", Min: 1, Max: 150, Group: "「水玉ポップ背景」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "地の色で塗る", Group: "「水玉ポップ背景」の設定"),
        ["Color1"] = new ParamGuide(Label: "地の色"),
        ["Color2"] = new ParamGuide(Label: "光の色"),
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
        if (Background) Box(dc, b, 0, 0, w, h, C1(a));
        for (int i = 0; i < Count; i++)
        {
            var q = new Vector2(Gfx.Mod(DrawKit.H(Seed, i) * w + t * 20f * s * (DrawKit.H(Seed, i, 3) - 0.5f) * Spd(c), w), Gfx.Mod(DrawKit.H(Seed, i, 1) * h - t * 30f * s * Spd(c), h + 200f * s) - 100f * s);
            float r = (20f + 80f * DrawKit.H(Seed, i, 2)) * s * (1f + 0.05f * MathF.Sin(t * 2f + i));
            Dot(dc, b, q, r, Col(BubbleKit.Palette(0, i), 0.75f * a));
            Dot(dc, b, q + new Vector2(-r * 0.35f, -r * 0.35f), r * 0.25f, C2(0.6f * a));
        }
    }
}

// =====================================================================
//  サンバースト（放射線）
//   中心からの放射状の光線がゆっくり回る。昭和・ポップな強調背景。「数」で線の本数
// =====================================================================
public sealed class SunburstPlugin : QuickShapePlugin
{
    public override string Name => "演出：サンバースト（放射線）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SunburstParameter(sharedData);
}

public sealed class SunburstParameter : QuickShapeParameter
{
    public SunburstParameter() : this(null) { }
    public SunburstParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 24;
        Color1 = Hex(0xFFB300);
        Color2 = Hex(0xFFE082);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "線の本数", Min: 4, Max: 80, Group: "「サンバースト（放射線）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "地の色で塗る", Group: "「サンバースト（放射線）」の設定"),
        ["Color1"] = new ParamGuide(Label: "線の色"),
        ["Color2"] = new ParamGuide(Label: "地の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = MathF.Max(w, h) * 1.5f;
        if (Background) Box(dc, b, 0, 0, w, h, C2(a));
        int n = Math.Max(4, Count);
        float rot = t * 0.2f * Spd(c);
        for (int i = 0; i < n; i += 2)
        {
            float a0 = rot + i * MathF.PI * 2f / n, a1 = rot + (i + 1) * MathF.PI * 2f / n;
            Poly(dc, b, new List<Vector2> { o, Pol(o, R, a0), Pol(o, R, a1) }, C1(a));
        }
    }
}

// =====================================================================
//  うねるグラデーション
//   3色がゆっくりうねって混ざる、なめらかな背景
// =====================================================================
public sealed class WavyGradientPlugin : QuickShapePlugin
{
    public override string Name => "演出：うねるグラデーション";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WavyGradientParameter(sharedData);
}

public sealed class WavyGradientParameter : QuickShapeParameter
{
    public WavyGradientParameter() : this(null) { }
    public WavyGradientParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x7F5AF0);
        Color2 = Hex(0x2CB1BC);
        Color3 = Hex(0xFF8BA7);
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
        ["Color1"] = new ParamGuide(Label: "色1"),
        ["Color2"] = new ParamGuide(Label: "色2"),
        ["Color3"] = new ParamGuide(Label: "色3"),
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
        Box(dc, b, 0, 0, w, h, C1(a));
        var cols = new[] { Color2, Color3, Color1 };
        for (int i = 0; i < 5; i++)
        {
            float ph = t * 0.15f * Spd(c) + i * 1.3f;
            var q = new Vector2(w * (0.5f + 0.45f * MathF.Sin(ph * 1.1f + i)), h * (0.5f + 0.45f * MathF.Cos(ph * 0.9f + i * 2f)));
            float r = MathF.Max(w, h) * (0.45f + 0.15f * MathF.Sin(ph));
            var col = cols[i % 3];
            using var g = Gfx.Radial(dc, q, r, (0f, Col(col, 0.9f * a)), (1f, Col(col, 0f)));
            dc.FillRectangle(new RawRectF(0, 0, w, h), g);
        }
    }
}

// =====================================================================
//  ワイヤーフレームの床（グリッド）
//   80年代風の、奥へ続く光る格子の床と、しま模様の夕日
// =====================================================================
public sealed class WireframeFloorPlugin : QuickShapePlugin
{
    public override string Name => "演出：ワイヤーフレームの床（グリッド）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WireframeFloorParameter(sharedData);
}

public sealed class WireframeFloorParameter : QuickShapeParameter
{
    public WireframeFloorParameter() : this(null) { }
    public WireframeFloorParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFF2E97);
        Color2 = Hex(0x1A0033);
        Color3 = Hex(0xFFB300);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "空の背景", Group: "「ワイヤーフレームの床（グリッド）」の設定"),
        ["Color1"] = new ParamGuide(Label: "格子の色"),
        ["Color2"] = new ParamGuide(Label: "床と空の色"),
        ["Color3"] = new ParamGuide(Label: "夕日の色"),
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
        float hz = h * 0.55f;
        if (Background) Sky(dc, w, hz, C2(a), Col(Hex(0x5A0F6B), a));
        var sun = new Vector2(w / 2f, hz - 40f * s);
        float sr = 230f * s;
        for (int i = 0; i < 12; i++)
        {
            float y0 = sun.Y - sr + sr * 2f * i / 12f, y1 = y0 + sr * 2f / 12f * (i < 6 ? 1f : 0.6f);
            float hw0 = MathF.Sqrt(MathF.Max(0f, sr * sr - MathF.Pow(y0 - sun.Y, 2f))), hw1 = MathF.Sqrt(MathF.Max(0f, sr * sr - MathF.Pow(y1 - sun.Y, 2f)));
            if (y0 > hz) break;
            Poly(dc, b, new List<Vector2> { new(sun.X - hw0, y0), new(sun.X + hw0, y0), new(sun.X + hw1, MathF.Min(y1, hz)), new(sun.X - hw1, MathF.Min(y1, hz)) }, Col(Gfx.Mix(Color3, Color1, i / 12f), a));
        }
        Box(dc, b, 0, hz, w, h, C2(a));
        for (int i = -20; i <= 20; i++) Ln(dc, b, new Vector2(w / 2f + i * 40f * s, hz), new Vector2(w / 2f + i * 400f * s, h), C1(0.8f * a), 2f * s);
        float off = Gfx.Mod(t * 0.8f * Spd(c), 1f);
        for (int i = 0; i < 14; i++)
        {
            float z = (i + off) / 14f;
            float y = hz + (h - hz) * z * z;
            Ln(dc, b, new Vector2(0, y), new Vector2(w, y), C1((0.3f + 0.7f * z) * a), (1f + 2f * z) * s);
        }
        DrawKit.Glow(dc, new Vector2(w / 2f, hz), w * 0.4f, C1(0.25f * a));
    }
}

// =====================================================================
//  格子のトンネル
//   四角い格子のトンネルを、奥へ進んでいくような背景
// =====================================================================
public sealed class GridTunnelPlugin : QuickShapePlugin
{
    public override string Name => "演出：格子のトンネル";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GridTunnelParameter(sharedData);
}

public sealed class GridTunnelParameter : QuickShapeParameter
{
    public GridTunnelParameter() : this(null) { }
    public GridTunnelParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x00E5FF);
        Color2 = Hex(0x000814);
        Count = 14;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "四角の数", Min: 4, Max: 40, Group: "「格子のトンネル」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "暗い背景", Group: "「格子のトンネル」の設定"),
        ["Color1"] = new ParamGuide(Label: "線の色"),
        ["Color2"] = new ParamGuide(Label: "背景の色"),
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
        if (Background) Box(dc, b, 0, 0, w, h, C2(a));
        var cc = new Vector2(w / 2f + MathF.Sin(t * 0.5f) * 60f * s, h / 2f + MathF.Cos(t * 0.4f) * 40f * s);
        float off = Gfx.Mod(t * 0.5f * Spd(c), 1f);
        var corners = new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(1, 1), new Vector2(-1, 1) };
        for (int k = 0; k < 4; k++) for (int j = 0; j <= 4; j++)
        {
            var e0 = Vector2.Lerp(corners[k], corners[(k + 1) % 4], j / 4f);
            Ln(dc, b, cc + e0 * new Vector2(w, h) * 0.02f, new Vector2(w / 2f, h / 2f) + e0 * new Vector2(w, h) * 0.6f, C1(0.4f * a), 2f * s);
        }
        for (int i = 0; i < Count; i++)
        {
            float z = (i + off) / Count;
            float k = MathF.Pow(z, 3f);
            var hs = new Vector2(w, h) * (0.02f + 0.58f * k);
            var center = Vector2.Lerp(cc, new Vector2(w / 2f, h / 2f), k);
            Frame(dc, b, center.X - hs.X, center.Y - hs.Y, center.X + hs.X, center.Y + hs.Y, C1(z * a), (1f + 4f * z) * s);
        }
    }
}

// =====================================================================
//  本棚
//   色とりどりの本がぎっしり並んだ本棚の背景。「数」で段の数
// =====================================================================
public sealed class BookshelfPlugin : QuickShapePlugin
{
    public override string Name => "演出：本棚";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BookshelfParameter(sharedData);
}

public sealed class BookshelfParameter : QuickShapeParameter
{
    public BookshelfParameter() : this(null) { }
    public BookshelfParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 4;
        Color1 = Hex(0x6D4C41);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "棚の段数", Min: 1, Max: 8, Group: "「本棚」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "棚の色"),
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
        int rows = Math.Clamp(Count, 1, 8);
        float s = U0(c);
        Box(dc, b, 0, 0, w, h, C1(a));
        float sh = h / rows;
        for (int r = 0; r < rows; r++)
        {
            float y1 = (r + 1) * sh - 24f * s;
            Box(dc, b, 0, y1, w, y1 + 24f * s, Col(Gfx.Mix(Color1, Hex(0x2B1A12), 0.3f), a));
            Box(dc, b, 0, r * sh, w, r * sh + 8f * s, Rgb(0, 0, 0, 0.2f * a));
            float x = 20f * s;
            int i = 0;
            while (x < w - 20f * s)
            {
                float bw = (28f + 40f * DrawKit.H(Seed + r, i)) * s, bh = sh * (0.6f + 0.3f * DrawKit.H(Seed + r, i, 1));
                bool lean = DrawKit.H(Seed + r, i, 3) < 0.06f;
                var col = Col(Gfx.Mix(BubbleKit.Palette(0, (int)(DrawKit.H(Seed + r, i, 2) * 10f)), Hex(0x403020), 0.35f), a);
                if (lean) { Poly(dc, b, new List<Vector2> { new(x, y1), new(x + bw, y1), new(x + bw + bh * 0.3f, y1 - bh * 0.95f), new(x + bh * 0.3f, y1 - bh * 0.95f) }, col); x += bw + bh * 0.3f; }
                else
                {
                    Box(dc, b, x, y1 - bh, x + bw, y1, col);
                    Box(dc, b, x, y1 - bh * 0.8f, x + bw, y1 - bh * 0.76f, Rgb(1, 0.9f, 0.6f, 0.6f * a));
                    Box(dc, b, x, y1 - bh * 0.25f, x + bw, y1 - bh * 0.21f, Rgb(1, 0.9f, 0.6f, 0.6f * a));
                    x += bw + 2f * s;
                }
                i++;
            }
        }
    }
}

// =====================================================================
//  カフェの窓辺
//   窓の外は雨(バリエーション2で晴れ)、手前のテーブルに湯気の立つコーヒー
// =====================================================================
public sealed class CafeWindowPlugin : QuickShapePlugin
{
    public override string Name => "演出：カフェの窓辺";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CafeWindowParameter(sharedData);
}

public sealed class CafeWindowParameter : QuickShapeParameter
{
    public CafeWindowParameter() : this(null) { }
    public CafeWindowParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x5D4037);
        Color2 = Hex(0x90A4AE);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=雨　2=晴れ", Min: 1, Max: 2, Group: "「カフェの窓辺」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "窓わくとテーブルの色"),
        ["Color2"] = new ParamGuide(Label: "雨の日の空の色"),
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
        bool rain = Variant < 2;
        Box(dc, b, 0, 0, w, h, Col(Hex(0xE8D9C4), a));
        float wx0 = w * 0.15f, wx1 = w * 0.85f, wy0 = h * 0.1f, wy1 = h * 0.65f;
        Sky(dc, w, h, rain ? C2(a) : Col(Hex(0x8FD3FF), a), rain ? Col(Hex(0x607D8B), a) : Col(Hex(0xE0F4FF), a));
        b.Color = Col(Hex(0xE8D9C4), a);
        DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), DrawKit.RectPts(wx0, wy0, wx1 - wx0, wy1 - wy0));
        for (int i = 0; i < 6; i++) { float bx = wx0 + (wx1 - wx0) * (i + 0.5f) / 6f; float bh = (80f + 200f * DrawKit.H(Seed, i)) * s; Box(dc, b, bx - 50f * s, wy1 - bh, bx + 50f * s, wy1, Rgb(0.35f, 0.4f, 0.45f, (rain ? 0.6f : 0.4f) * a)); }
        if (rain) for (int i = 0; i < 60; i++)
        {
            float x = wx0 + DrawKit.H(Seed, i) * (wx1 - wx0);
            float y = wy0 + Gfx.Mod(DrawKit.H(Seed, i, 1) * (wy1 - wy0) + t * 600f * s, wy1 - wy0);
            Ln(dc, b, new Vector2(x, y), new Vector2(x - 3f * s, MathF.Min(wy1, y + 30f * s)), Rgb(1, 1, 1, 0.4f * a), 2f * s);
        }
        Box(dc, b, (wx0 + wx1) / 2f - 8f * s, wy0, (wx0 + wx1) / 2f + 8f * s, wy1, C1(a));
        Frame(dc, b, wx0, wy0, wx1, wy1, C1(a), 20f * s);
        Box(dc, b, 0, h * 0.72f, w, h, C1(a));
        Box(dc, b, 0, h * 0.72f, w, h * 0.74f, Rgb(1, 1, 1, 0.15f * a));
        var cup = new Vector2(w * 0.65f, h * 0.8f);
        Oval(dc, b, cup + new Vector2(0, 50f * s), 130f * s, 25f * s, Rgb(0.95f, 0.95f, 0.95f, a));
        Poly(dc, b, Pts(cup, s, s, -80f, -40f, 80f, -40f, 65f, 40f, -65f, 40f), Rgb(1, 1, 1, a));
        Ring(dc, b, cup + new Vector2(90f * s, -5f * s), 25f * s, Rgb(1, 1, 1, a), 10f * s);
        Oval(dc, b, cup + new Vector2(0, -40f * s), 80f * s, 14f * s, Rgb(0.4f, 0.25f, 0.15f, a));
        for (int k = 0; k < 3; k++)
        {
            var st = new List<Vector2>();
            for (int j = 0; j <= 12; j++) { float f = j / 12f; st.Add(cup + new Vector2((k - 1) * 30f * s + MathF.Sin(f * 6f + t * 2f + k) * 15f * s, -60f * s - f * 160f * s)); }
            Path(dc, b, st, Rgb(1, 1, 1, 0.35f * a), 6f * s);
        }
    }
}

// =====================================================================
//  駅のホーム
//   屋根と柱のある駅のホーム、黄色い点字ブロック、駅名の看板(文字)
// =====================================================================
public sealed class TrainPlatformPlugin : QuickShapePlugin
{
    public override string Name => "演出：駅のホーム";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TrainPlatformParameter(sharedData);
}

public sealed class TrainPlatformParameter : QuickShapeParameter
{
    public TrainPlatformParameter() : this(null) { }
    public TrainPlatformParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ゆっくり駅";
        Color1 = Hex(0x546E7A);
        Color2 = Hex(0xFFD600);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "駅名", Group: "「駅のホーム」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "屋根と柱の色"),
        ["Color2"] = new ParamGuide(Label: "点字ブロックの色"),
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
        Sky(dc, w, h, Col(Hex(0xA7D8F5), a), Col(Hex(0xE3F2FD), a));
        Box(dc, b, 0, h * 0.68f, w, h, Rgb(0.6f, 0.6f, 0.62f, a));
        Box(dc, b, 0, h * 0.70f, w, h * 0.74f, C2(a));
        Box(dc, b, 0, h * 0.62f, w, h * 0.68f, Rgb(0.3f, 0.3f, 0.32f, a));
        for (int i = 0; i < 24; i++) Box(dc, b, i * w / 24f, h * 0.645f, i * w / 24f + w / 48f, h * 0.66f, Rgb(0.45f, 0.35f, 0.25f, a));
        Box(dc, b, 0, h * 0.635f, w, h * 0.642f, Rgb(0.75f, 0.75f, 0.78f, a));
        Box(dc, b, 0, 0, w, h * 0.12f, C1(a));
        for (int i = 0; i < 5; i++) { float x = w * (i + 0.5f) / 5f; Box(dc, b, x - 14f * s, h * 0.12f, x + 14f * s, h * 0.70f, C1(a)); }
        var sg = new Vector2(w / 2f, h * 0.3f);
        Ln(dc, b, sg + new Vector2(-180f, -100f) * s, new Vector2(sg.X - 180f * s, h * 0.12f), Rgb(0.3f, 0.3f, 0.3f, a), 6f * s);
        Ln(dc, b, sg + new Vector2(180f, -100f) * s, new Vector2(sg.X + 180f * s, h * 0.12f), Rgb(0.3f, 0.3f, 0.3f, a), 6f * s);
        RBox(dc, b, sg, 520f * s, 200f * s, 10f * s, Rgb(1, 1, 1, a));
        Box(dc, b, sg.X - 260f * s, sg.Y + 40f * s, sg.X + 260f * s, sg.Y + 56f * s, Rgb(0.1f, 0.6f, 0.3f, a));
        Say(dc, b, Str, 70f * s, sg + new Vector2(0, -20f * s), Rgb(0.1f, 0.1f, 0.12f, a));
    }
}

// =====================================================================
//  流れる車窓
//   電車の窓から見た景色。電柱がビュンビュン流れ、遠くの山はゆっくり
// =====================================================================
public sealed class TrainWindowPlugin : QuickShapePlugin
{
    public override string Name => "演出：流れる車窓";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TrainWindowParameter(sharedData);
}

public sealed class TrainWindowParameter : QuickShapeParameter
{
    public TrainWindowParameter() : this(null) { }
    public TrainWindowParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x455A64);
        Color2 = Hex(0x7CB342);
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
        ["Color1"] = new ParamGuide(Label: "車体の色"),
        ["Color2"] = new ParamGuide(Label: "野原の色"),
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
        float wx0 = w * 0.1f, wx1 = w * 0.9f, wy0 = h * 0.12f, wy1 = h * 0.72f;
        Sky(dc, w, h, Col(Hex(0x81D4FA), a), Col(Hex(0xE1F5FE), a));
        var m = new List<Vector2> { new(-10, h) };
        for (int i = 0; i <= 40; i++) { float x = w * i / 40f; float wx = x + t * 20f * s * Spd(c); m.Add(new Vector2(x, h * 0.45f - (MathF.Abs(MathF.Sin(wx * 0.003f / s)) * 120f + 30f) * s)); }
        m.Add(new Vector2(w + 10, h));
        Poly(dc, b, m, Rgb(0.45f, 0.6f, 0.7f, a));
        Box(dc, b, 0, h * 0.5f, w, h, C2(a));
        for (int i = 0; i < 12; i++) { float x = Gfx.Mod(i * 220f * s - t * 400f * s * Spd(c), w + 300f * s) - 150f * s; Dot(dc, b, new Vector2(x, h * 0.52f), 60f * s, Rgb(0.3f, 0.5f, 0.25f, a)); }
        float px = Gfx.Mod(-t * 1800f * s * Spd(c), w * 0.8f) + w * 0.1f;
        Box(dc, b, px, wy0, px + 26f * s, h, Rgb(0.35f, 0.3f, 0.25f, a));
        Ln(dc, b, new Vector2(0, h * 0.2f), new Vector2(w, h * 0.22f), Rgb(0.1f, 0.1f, 0.1f, 0.6f * a), 3f * s);
        b.Color = C1(a);
        DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), ShapeGeom.RoundRect((wx0 + wx1) / 2f, (wy0 + wy1) / 2f, wx1 - wx0, wy1 - wy0, 40f * s));
        Box(dc, b, 0, h * 0.8f, w, h * 0.84f, Rgb(0.6f, 0.62f, 0.65f, a));
    }
}

// =====================================================================
//  水族館の水槽
//   青い水槽に光の筋がゆれ、魚がすいすい、泡がぷくぷく。「数」で魚の数
// =====================================================================
public sealed class AquariumTankPlugin : QuickShapePlugin
{
    public override string Name => "演出：水族館の水槽";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AquariumTankParameter(sharedData);
}

public sealed class AquariumTankParameter : QuickShapeParameter
{
    public AquariumTankParameter() : this(null) { }
    public AquariumTankParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 10;
        Color1 = Hex(0x29B6F6);
        Color2 = Hex(0x01579B);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "魚の数", Min: 0, Max: 40, Group: "「水族館の水槽」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "水の明るい色"),
        ["Color2"] = new ParamGuide(Label: "水の奥の色"),
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
        Sky(dc, w, h, C1(a), C2(a));
        for (int i = 0; i < 6; i++) { float x = w * (i + 0.5f) / 6f + MathF.Sin(t * 0.5f + i) * 60f * s; Poly(dc, b, new List<Vector2> { new(x - 40f * s, 0), new(x + 40f * s, 0), new(x + 200f * s, h), new(x - 40f * s, h) }, Rgb(1, 1, 1, 0.06f * a)); }
        Box(dc, b, 0, h * 0.88f, w, h, Rgb(0.85f, 0.8f, 0.6f, a));
        for (int i = 0; i < 8; i++) { var wp = new List<Vector2>(); float x = w * DrawKit.H(Seed, i, 9); for (int k = 0; k <= 10; k++) wp.Add(new Vector2(x + MathF.Sin(t * 1.5f + k * 0.6f + i) * 15f * s * k / 10f, h * 0.9f - k * 30f * s)); Path(dc, b, wp, Rgb(0.2f, 0.6f, 0.3f, a), 10f * s); }
        for (int i = 0; i < Count; i++)
        {
            float sp = (60f + 80f * DrawKit.H(Seed, i)) * s * Spd(c) * (i % 2 == 0 ? 1f : -1f);
            float x = Gfx.Mod(DrawKit.H(Seed, i, 1) * w + t * sp, w + 300f * s) - 150f * s;
            float y = h * (0.15f + 0.65f * DrawKit.H(Seed, i, 2)) + MathF.Sin(t + i) * 20f * s;
            float sz = (30f + 40f * DrawKit.H(Seed, i, 3)) * s, dir = sp > 0 ? 1f : -1f;
            var col = Col(BubbleKit.Palette(0, i), a);
            Oval(dc, b, new Vector2(x, y), sz, sz * 0.55f, col);
            float wag = MathF.Sin(t * 8f + i) * 0.3f;
            Poly(dc, b, new List<Vector2> { new(x - dir * sz * 0.8f, y), new(x - dir * sz * 1.5f, y - sz * (0.5f + wag)), new(x - dir * sz * 1.5f, y + sz * (0.5f - wag)) }, col);
            Dot(dc, b, new Vector2(x + dir * sz * 0.5f, y - sz * 0.1f), sz * 0.12f, Rgb(0, 0, 0, a));
        }
        for (int i = 0; i < 25; i++) { float ph = Gfx.Mod(t * 0.3f + DrawKit.H(Seed, i, 5), 1f); var q = new Vector2(w * DrawKit.H(Seed, i, 6) + MathF.Sin(t * 3f + i) * 8f * s, h * (1f - ph)); Ring(dc, b, q, (4f + 8f * DrawKit.H(Seed, i, 7)) * s, Rgb(1, 1, 1, 0.6f * a), 2f * s); }
    }
}

// =====================================================================
//  ライブ会場のペンライト
//   暗い客席で、たくさんのペンライトがリズムに合わせて左右にふられる
// =====================================================================
public sealed class ConcertPenlightsPlugin : QuickShapePlugin
{
    public override string Name => "演出：ライブ会場のペンライト";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ConcertPenlightsParameter(sharedData);
}

public sealed class ConcertPenlightsParameter : QuickShapeParameter
{
    public ConcertPenlightsParameter() : this(null) { }
    public ConcertPenlightsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 80;
        Color1 = Hex(0xFF4081);
        Color2 = Hex(0x00E5FF);
        Color3 = Hex(0xFFEA00);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "ペンライトの数", Min: 5, Max: 300, Group: "「ライブ会場のペンライト」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "暗い会場の背景", Group: "「ライブ会場のペンライト」の設定"),
        ["Color1"] = new ParamGuide(Label: "ペンライトの色1"),
        ["Color2"] = new ParamGuide(Label: "ペンライトの色2"),
        ["Color3"] = new ParamGuide(Label: "ペンライトの色3"),
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
        if (Background) Sky(dc, w, h, Rgb(0.02f, 0.01f, 0.06f, a), Rgb(0.08f, 0.03f, 0.15f, a));
        for (int i = 0; i < 5; i++) { float ang = MathF.PI / 2f + MathF.Sin(t * 0.8f + i * 1.3f) * 0.5f; var top = new Vector2(w * (i + 0.5f) / 5f, -20f); Poly(dc, b, new List<Vector2> { top, top + new Vector2(MathF.Cos(ang - 0.08f), MathF.Sin(ang - 0.08f)) * h * 1.3f, top + new Vector2(MathF.Cos(ang + 0.08f), MathF.Sin(ang + 0.08f)) * h * 1.3f }, Col(BubbleKit.Palette(0, i), 0.12f * a)); }
        var cols = new[] { Color1, Color2, Color3 };
        float beat = t * 2.2f * Spd(c) * MathF.PI;
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        for (int i = 0; i < Count; i++)
        {
            float row = DrawKit.H(Seed, i, 1);
            var hand = new Vector2(DrawKit.H(Seed, i) * w, h * (0.6f + 0.4f * row));
            float ang = -MathF.PI / 2f + MathF.Sin(beat + (i % 2) * 0.3f) * 0.5f;
            float L = (60f + 60f * row) * s;
            var tip = Pol(hand, L, ang);
            var col = cols[(int)(DrawKit.H(Seed, i, 2) * 3f) % 3];
            DrawKit.Glow(dc, tip, 50f * s * (0.6f + row), Col(col, 0.35f * a));
            Ln(dc, b, hand, tip, Col(col, a), (8f + 6f * row) * s);
        }
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        for (int i = 0; i < Count; i++)
        {
            float row = DrawKit.H(Seed, i, 1);
            var hand = new Vector2(DrawKit.H(Seed, i) * w, h * (0.6f + 0.4f * row));
            Dot(dc, b, hand + new Vector2(0, 50f * s), (40f + 30f * row) * s, Rgb(0.03f, 0.02f, 0.06f, a));
        }
    }
}

// =====================================================================
//  神社・お寺
//   瓦屋根のお堂と、まわりの木々。バリエーション2で夜(灯りがともる)
// =====================================================================
public sealed class ShrineTemplePlugin : QuickShapePlugin
{
    public override string Name => "演出：神社・お寺";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ShrineTempleParameter(sharedData);
}

public sealed class ShrineTempleParameter : QuickShapeParameter
{
    public ShrineTempleParameter() : this(null) { }
    public ShrineTempleParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x37474F);
        Color2 = Hex(0xC62828);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=昼　2=夜", Min: 1, Max: 2, Group: "「神社・お寺」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "屋根の色"),
        ["Color2"] = new ParamGuide(Label: "お堂の色"),
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
        bool night = Variant >= 2;
        Sky(dc, w, h, night ? Rgb(0.05f, 0.06f, 0.15f, a) : Col(Hex(0x9FD8FF), a), night ? Rgb(0.2f, 0.15f, 0.3f, a) : Col(Hex(0xF0F8FF), a));
        for (int i = 0; i < 9; i++) { var q = new Vector2(w * (i + 0.5f) / 9f, h * 0.55f); Dot(dc, b, q, (120f + 60f * DrawKit.H(Seed, i)) * s, night ? Rgb(0.05f, 0.1f, 0.08f, a) : Rgb(0.25f, 0.45f, 0.25f, a)); }
        Box(dc, b, 0, h * 0.75f, w, h, night ? Rgb(0.2f, 0.18f, 0.16f, a) : Rgb(0.82f, 0.78f, 0.7f, a));
        var hall = new Vector2(w / 2f, h * 0.75f);
        Box(dc, b, hall.X - 440f * s, hall.Y - 40f * s, hall.X + 440f * s, hall.Y, Rgb(0.6f, 0.58f, 0.55f, a));
        Box(dc, b, hall.X - 360f * s, hall.Y - 300f * s, hall.X + 360f * s, hall.Y - 40f * s, night ? Rgb(0.3f, 0.12f, 0.1f, a) : C2(a));
        for (int i = 0; i < 4; i++) Box(dc, b, hall.X - 300f * s + i * 200f * s, hall.Y - 280f * s, hall.X - 220f * s + i * 200f * s, hall.Y - 40f * s, night ? Rgb(1f, 0.75f, 0.4f, a) : Rgb(0.95f, 0.9f, 0.8f, a));
        Poly(dc, b, Pts(new Vector2(hall.X, hall.Y - 300f * s), s, s, -520f, 20f, -440f, -20f, -300f, -120f, 300f, -120f, 440f, -20f, 520f, 20f, 380f, 0f, -380f, 0f), C1(a));
        Poly(dc, b, Pts(new Vector2(hall.X, hall.Y - 420f * s), s, s, -300f, 0f, -200f, -110f, 200f, -110f, 300f, 0f), C1(a));
        if (night) for (int e = -1; e <= 1; e += 2) DrawKit.Glow(dc, new Vector2(hall.X + e * 500f * s, hall.Y - 100f * s), 120f * s, Rgb(1f, 0.7f, 0.3f, 0.6f * a));
    }
}

// =====================================================================
//  研究室
//   フラスコがぶくぶく、ホワイトボードに数式、光るモニター。科学の解説の背景に
// =====================================================================
public sealed class LaboratoryPlugin : QuickShapePlugin
{
    public override string Name => "演出：研究室";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LaboratoryParameter(sharedData);
}

public sealed class LaboratoryParameter : QuickShapeParameter
{
    public LaboratoryParameter() : this(null) { }
    public LaboratoryParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "E=mc²  ∫f(x)dx";
        Color1 = Hex(0xECEFF1);
        Color2 = Hex(0x26C6DA);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "ホワイトボードの文字", Group: "「研究室」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "壁の色"),
        ["Color2"] = new ParamGuide(Label: "モニターの波形の色"),
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
        Box(dc, b, 0, h * 0.7f, w, h, Rgb(0.5f, 0.55f, 0.6f, a));
        Box(dc, b, 0, h * 0.7f, w, h * 0.72f, Rgb(0.3f, 0.33f, 0.36f, a));
        Box(dc, b, w * 0.08f, h * 0.12f, w * 0.5f, h * 0.5f, Rgb(1, 1, 1, a));
        Frame(dc, b, w * 0.08f, h * 0.12f, w * 0.5f, h * 0.5f, Rgb(0.6f, 0.62f, 0.65f, a), 8f * s);
        Say(dc, b, Str, 60f * s, new Vector2(w * 0.29f, h * 0.31f), Rgb(0.1f, 0.2f, 0.6f, a), bold: false, maxW: w * 0.38f);
        var mon = new Vector2(w * 0.75f, h * 0.45f);
        RBox(dc, b, mon, 420f * s, 280f * s, 12f * s, Rgb(0.15f, 0.16f, 0.2f, a));
        Box(dc, b, mon.X - 190f * s, mon.Y - 120f * s, mon.X + 190f * s, mon.Y + 120f * s, Rgb(0.02f, 0.1f, 0.12f, a));
        var wave = new List<Vector2>();
        for (int i = 0; i <= 40; i++) wave.Add(new Vector2(mon.X - 180f * s + 360f * s * i / 40f, mon.Y + MathF.Sin(i * 0.5f - t * 4f) * 50f * s * MathF.Sin(i / 40f * MathF.PI)));
        Path(dc, b, wave, C2(a), 4f * s);
        Box(dc, b, mon.X - 20f * s, mon.Y + 140f * s, mon.X + 20f * s, h * 0.7f, Rgb(0.3f, 0.32f, 0.35f, a));
        var cols = new[] { Hex(0x66BB6A), Hex(0xEC407A), Hex(0x42A5F5) };
        for (int i = 0; i < 3; i++)
        {
            var fb = new Vector2(w * (0.15f + i * 0.13f), h * 0.7f);
            var fl = Pts(fb, s, s, -70f, 0f, 70f, 0f, 20f, -110f, 20f, -180f, -20f, -180f, -20f, -110f);
            var liq = Pts(fb, s, s, -70f, 0f, 70f, 0f, 42f, -60f, -42f, -60f);
            Poly(dc, b, liq, Col(cols[i], 0.85f * a));
            Outline(dc, b, fl, Rgb(0.5f, 0.6f, 0.65f, a), 4f * s);
            for (int k = 0; k < 4; k++) { float ph = Gfx.Mod(t * 0.8f + k * 0.25f + i * 0.3f, 1f); Ring(dc, b, fb + new Vector2(MathF.Sin(ph * 9f + k) * 10f * s, -60f * s - ph * 200f * s), (5f + 4f * ph) * s, Col(cols[i], (1f - ph) * a), 2f * s); }
        }
    }
}
