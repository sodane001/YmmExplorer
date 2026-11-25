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
//  折れ線グラフ
//   「文字」にカンマ区切りの値。線が左から伸びていき、最後の値を表示する
// =====================================================================
public sealed class LineChartPlugin : QuickShapePlugin
{
    public override string Name => "演出：折れ線グラフ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LineChartParameter(sharedData);
}

public sealed class LineChartParameter : QuickShapeParameter
{
    public LineChartParameter() : this(null) { }
    public LineChartParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "30,45,40,62,58,75,90";
        Color1 = Hex(0x4FC3F7);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "グラフの値", Tip: "カンマ区切りで数値を並べます(例: 30,45,62)", Group: "「折れ線グラフ」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "線の色"),
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
        var vals = Items.Select(s => float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var fv) ? fv : 0f).ToList();
        if (vals.Count < 2) return;
        float gw = 1200f * u, gh = 560f * u, x0 = o.X - gw / 2f, y1 = o.Y + gh / 2f;
        float mx = MathF.Max(1e-3f, vals.Max()) * 1.15f;
        for (int i = 0; i <= 4; i++) Ln(dc, b, new Vector2(x0, y1 - gh * i / 4f), new Vector2(x0 + gw, y1 - gh * i / 4f), Rgb(1, 1, 1, 0.15f * a), 2f * u);
        Ln(dc, b, new Vector2(x0, y1), new Vector2(x0 + gw, y1), Rgb(1, 1, 1, 0.8f * a), 4f * u);
        var pts = new List<Vector2>();
        for (int i = 0; i < vals.Count; i++) pts.Add(new Vector2(x0 + gw * i / (vals.Count - 1), y1 - gh * vals[i] / mx));
        float p = Intro(c, 0.2f, 1.6f);
        var part = BubbleKit.PartByLength(pts, p);
        var area = new List<Vector2>(part) { new Vector2(part[^1].X, y1), new Vector2(x0, y1) };
        Poly(dc, b, area, C1(0.18f * a));
        Path(dc, b, part, C1(a), 8f * u);
        for (int i = 0; i < pts.Count; i++) if (pts[i].X <= part[^1].X + 1f) { Dot(dc, b, pts[i], 11f * u, Rgb(1, 1, 1, a)); Dot(dc, b, pts[i], 6f * u, C1(a)); }
        if (p >= 1f) Say(dc, b, vals[^1].ToString("0.#", System.Globalization.CultureInfo.InvariantCulture), 52f * u, pts[^1] + new Vector2(0f, -50f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
    }
}

// =====================================================================
//  散布図・バブルチャート
//   「数」の数だけ、大きさのちがう丸がポンポン現れる。軸つき
// =====================================================================
public sealed class BubbleChartPlugin : QuickShapePlugin
{
    public override string Name => "演出：散布図・バブルチャート";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BubbleChartParameter(sharedData);
}

public sealed class BubbleChartParameter : QuickShapeParameter
{
    public BubbleChartParameter() : this(null) { }
    public BubbleChartParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 14;
        Color1 = Hex(0x7C4DFF);
        Color2 = Hex(0x26C6DA);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "丸の数", Min: 1, Max: 60, Group: "「散布図・バブルチャート」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "丸の色1"),
        ["Color2"] = new ParamGuide(Label: "丸の色2"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float gw = 1200f * u, gh = 620f * u, x0 = o.X - gw / 2f, y1 = o.Y + gh / 2f;
        Ln(dc, b, new Vector2(x0, y1), new Vector2(x0 + gw, y1), Rgb(1, 1, 1, 0.8f * a), 4f * u);
        Ln(dc, b, new Vector2(x0, y1), new Vector2(x0, y1 - gh), Rgb(1, 1, 1, 0.8f * a), 4f * u);
        for (int i = 0; i < Count; i++)
        {
            float p = DrawKit.EaseOutBack(Intro(c, 0.2f + i * 0.08f, 0.35f));
            if (p <= 0f) continue;
            var q = new Vector2(x0 + gw * (0.06f + DrawKit.H(Seed, i, 1) * 0.9f), y1 - gh * (0.08f + DrawKit.H(Seed, i, 2) * 0.85f));
            float r = (14f + DrawKit.H(Seed, i, 3) * 60f) * u * p;
            var col = Gfx.Mix(Color1, Color2, DrawKit.H(Seed, i, 4));
            Dot(dc, b, q, r, Col(col, 0.55f * a));
            Ring(dc, b, q, r, Col(col, a), 3f * u);
        }
    }
}

// =====================================================================
//  積み上げ棒グラフ
//   1行に「名前:値,値,値」。色ごとに積み上がり、下から伸びる
// =====================================================================
public sealed class StackedBarPlugin : QuickShapePlugin
{
    public override string Name => "演出：積み上げ棒グラフ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StackedBarParameter(sharedData);
}

public sealed class StackedBarParameter : QuickShapeParameter
{
    public StackedBarParameter() : this(null) { }
    public StackedBarParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "1月:20,15,10\n2月:25,18,12\n3月:30,22,15\n4月:28,30,20";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "グラフのデータ", Tip: "1行に「名前:値,値,値」。色ごとに積み上がります", Group: "「積み上げ棒グラフ」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
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
        var rows = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Contains(':') || s.Contains('：')).ToList();
        if (rows.Count == 0) return;
        var data = new List<(string n, float[] v)>();
        foreach (var r0 in rows)
        {
            int ci = r0.IndexOfAny(new[] { ':', '：' });
            var v = r0[(ci + 1)..].Split(',').Select(s => float.TryParse(s.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var fv) ? fv : 0f).ToArray();
            data.Add((r0[..ci].Trim(), v));
        }
        float mx = MathF.Max(1e-3f, data.Max(d => d.v.Sum())) * 1.1f;
        float gw = 1200f * u, gh = 560f * u, x0 = o.X - gw / 2f, y1 = o.Y + gh / 2f;
        float slot = gw / data.Count, bw = slot * 0.55f;
        Ln(dc, b, new Vector2(x0, y1), new Vector2(x0 + gw, y1), Rgb(1, 1, 1, 0.8f * a), 4f * u);
        for (int i = 0; i < data.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.2f + i * 0.12f, 0.6f));
            float cx = x0 + slot * (i + 0.5f), y = y1;
            for (int j = 0; j < data[i].v.Length; j++)
            {
                float hh = gh * data[i].v[j] / mx * p;
                Box(dc, b, cx - bw / 2f, y - hh, cx + bw / 2f, y, Col(BubbleKit.Palette(0, j), a));
                y -= hh;
            }
            Say(dc, b, data[i].n, 34f * u, new Vector2(cx, y1 + 34f * u), Rgb(1, 1, 1, a));
        }
    }
}

// =====================================================================
//  ベン図
//   「文字」にカンマ区切りで円の名前(2つか3つ)。円が重なって現れる
// =====================================================================
public sealed class VennPlugin : QuickShapePlugin
{
    public override string Name => "演出：ベン図";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new VennParameter(sharedData);
}

public sealed class VennParameter : QuickShapeParameter
{
    public VennParameter() : this(null) { }
    public VennParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "速い,安い,うまい";
        Color1 = Hex(0xFF5C8A);
        Color2 = Hex(0x4FC3F7);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "円の名前", Tip: "カンマ区切りで2つか3つ", Group: "「ベン図」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "円1の色"),
        ["Color2"] = new ParamGuide(Label: "円2の色"),
        ["Color3"] = new ParamGuide(Label: "円3の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var names = Items;
        int n = Math.Clamp(names.Count, 2, 3);
        float r = 230f * u;
        var cols = new[] { Color1, Color2, Color3 };
        for (int i = 0; i < n; i++)
        {
            float p = DrawKit.EaseOutBack(Intro(c, i * 0.2f, 0.4f));
            float ang = n == 2 ? (i == 0 ? MathF.PI : 0f) : -MathF.PI / 2f + i * MathF.PI * 2f / 3f;
            var q = Pol(o, r * 0.55f, ang);
            Dot(dc, b, q, r * p, Col(cols[i], 0.45f * a));
            Ring(dc, b, q, r * p, Col(cols[i], a), 5f * u);
            if (i < names.Count) Say(dc, b, names[i], 46f * u, Pol(o, r * 1.0f, ang), Rgb(1, 1, 1, a * p), Rgb(0, 0, 0, 0.6f * a), 3f * u);
        }
    }
}

// =====================================================================
//  ピラミッド図・階層図
//   「文字」に1行ずつ(上から)。段が下から順に積み上がる
// =====================================================================
public sealed class PyramidChartPlugin : QuickShapePlugin
{
    public override string Name => "演出：ピラミッド図・階層図";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PyramidChartParameter(sharedData);
}

public sealed class PyramidChartParameter : QuickShapeParameter
{
    public PyramidChartParameter() : this(null) { }
    public PyramidChartParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "理念\n戦略\n戦術\n行動";
        Color1 = Hex(0x26A69A);
        Color2 = Hex(0x1E3A5F);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "段の言葉", Tip: "1行に1段(上から順)", Group: "「ピラミッド図・階層図」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "上の段の色"),
        ["Color2"] = new ParamGuide(Label: "下の段の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var items = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        int n = items.Count;
        if (n == 0) return;
        float pw = 1000f * u, ph = 700f * u, top = o.Y - ph / 2f, lh = ph / n;
        for (int i = 0; i < n; i++)
        {
            int order = n - 1 - i;
            float p = DrawKit.EaseOut(Intro(c, order * 0.25f, 0.4f));
            if (p <= 0f) continue;
            float y0 = top + i * lh, y1 = y0 + lh - 6f * u;
            float w0 = pw * i / n, w1 = pw * (i + 1) / n;
            var col = Gfx.Mix(Color1, Color2, i / (float)Math.Max(1, n - 1));
            var trap = new List<Vector2> { new(o.X - w0 / 2f, y0 + (1f - p) * 40f * u), new(o.X + w0 / 2f, y0 + (1f - p) * 40f * u), new(o.X + w1 / 2f, y1 + (1f - p) * 40f * u), new(o.X - w1 / 2f, y1 + (1f - p) * 40f * u) };
            if (i == 0) trap[1] = trap[0];
            Poly(dc, b, trap, Col(col, a * p));
            Say(dc, b, items[i], MathF.Min(44f * u, lh * 0.45f), new Vector2(o.X, (y0 + y1) / 2f + (i == 0 ? lh * 0.15f : 0f)), Rgb(1, 1, 1, a * p));
        }
    }
}

// =====================================================================
//  マインドマップ
//   「文字」の1行目がまん中、2行目からが枝。枝が伸びて言葉が出る
// =====================================================================
public sealed class MindMapPlugin : QuickShapePlugin
{
    public override string Name => "演出：マインドマップ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MindMapParameter(sharedData);
}

public sealed class MindMapParameter : QuickShapeParameter
{
    public MindMapParameter() : this(null) { }
    public MindMapParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "動画づくり\n企画\n台本\n素材\n編集\n投稿\n分析";
        Color1 = Hex(0x5C6BC0);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "言葉", Tip: "1行目がまん中、2行目からが枝", Group: "「マインドマップ」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "まん中の色"),
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
        var items = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        if (items.Count == 0) return;
        int n = items.Count - 1;
        for (int i = 0; i < n; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.3f + i * 0.15f, 0.4f));
            if (p <= 0f) continue;
            float ang = -MathF.PI / 2f + i * MathF.PI * 2f / n;
            var end = new Vector2(o.X + MathF.Cos(ang) * 520f * u, o.Y + MathF.Sin(ang) * 300f * u);
            var ctrl = Vector2.Lerp(o, end, 0.5f) + new Vector2(0f, -60f * u * MathF.Cos(ang));
            var col = BubbleKit.Palette(0, i);
            Path(dc, b, BubbleKit.PartByLength(BubbleKit.Quad(o, ctrl, end, 30), p), Col(col, a), 8f * u);
            if (p >= 1f)
            {
                var ts = Measure(items[i + 1], 40f * u);
                RBox(dc, b, end, ts.Width + 40f * u, ts.Height + 18f * u, 30f * u, Col(col, a));
                Say(dc, b, items[i + 1], 40f * u, end, Rgb(1, 1, 1, a));
            }
        }
        var cs = Measure(items[0], 56f * u);
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.35f));
        RBox(dc, b, o, (cs.Width + 70f * u) * k, (cs.Height + 40f * u) * k, 50f * u, C1(a));
        Say(dc, b, items[0], 56f * u, o, Rgb(1, 1, 1, a), scale: k);
    }
}

// =====================================================================
//  ツリー図・組織図
//   1行目が一番上。2行目からが下の段。「-」で始まる行は、その上の行のさらに下
// =====================================================================
public sealed class OrgChartPlugin : QuickShapePlugin
{
    public override string Name => "演出：ツリー図・組織図";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new OrgChartParameter(sharedData);
}

public sealed class OrgChartParameter : QuickShapeParameter
{
    public OrgChartParameter() : this(null) { }
    public OrgChartParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "社長\n営業部\n-東京\n-大阪\n開発部\n-アプリ\n総務部";
        Color1 = Hex(0x1E88E5);
        Color2 = Hex(0x43A047);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "組織", Tip: "1行目が一番上。「-」で始まる行は、その上の行の下につきます", Group: "「ツリー図・組織図」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "2段目の色"),
        ["Color2"] = new ParamGuide(Label: "3段目の色"),
        ["Color3"] = new ParamGuide(Label: "一番上の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var lines = Str.Split('\n').Select(s => s.TrimEnd()).Where(s => s.Trim().Length > 0).ToList();
        if (lines.Count == 0) return;
        var kids = new List<(string n, List<string> sub)>();
        for (int i = 1; i < lines.Count; i++)
        {
            var s = lines[i].Trim();
            if (s.StartsWith("-") && kids.Count > 0) kids[^1].sub.Add(s[1..].Trim());
            else kids.Add((s, new List<string>()));
        }
        float bw = 230f * u, bh = 80f * u, rowGap = 160f * u;
        var rootP = new Vector2(o.X, o.Y - rowGap);
        float span = kids.Count > 0 ? MathF.Max(1, kids.Count) * (bw + 50f * u) : 0f;
        float lw = 4f * u;
        for (int i = 0; i < kids.Count; i++)
        {
            float p = Intro(c, 0.3f + i * 0.15f, 0.35f);
            if (p <= 0f) continue;
            var kp = new Vector2(o.X - span / 2f + (bw + 50f * u) * (i + 0.5f), o.Y);
            Ln(dc, b, rootP + new Vector2(0, bh / 2f), new Vector2(rootP.X, (rootP.Y + kp.Y) / 2f), Rgb(1, 1, 1, 0.8f * a), lw);
            Ln(dc, b, new Vector2(rootP.X, (rootP.Y + kp.Y) / 2f), new Vector2(kp.X, (rootP.Y + kp.Y) / 2f), Rgb(1, 1, 1, 0.8f * a), lw);
            Ln(dc, b, new Vector2(kp.X, (rootP.Y + kp.Y) / 2f), kp - new Vector2(0, bh / 2f), Rgb(1, 1, 1, 0.8f * a), lw);
            RBox(dc, b, kp, bw * p, bh, 12f * u, C1(a));
            Say(dc, b, kids[i].n, 34f * u, kp, Rgb(1, 1, 1, a * p), maxW: bw * 0.9f);
            for (int j = 0; j < kids[i].sub.Count; j++)
            {
                float q = Intro(c, 0.6f + i * 0.15f + j * 0.1f, 0.3f);
                if (q <= 0f) continue;
                var sp = kp + new Vector2(0f, (j + 1) * (bh + 30f * u));
                Ln(dc, b, sp - new Vector2(0, bh + 30f * u - bh / 2f), sp - new Vector2(0, bh / 2f), Rgb(1, 1, 1, 0.6f * a), lw * 0.8f);
                RBox(dc, b, sp, bw * 0.85f * q, bh * 0.85f, 12f * u, C2(a));
                Say(dc, b, kids[i].sub[j], 30f * u, sp, Rgb(1, 1, 1, a * q), maxW: bw * 0.8f);
            }
        }
        float rk = DrawKit.EaseOutBack(Intro(c, 0f, 0.35f));
        RBox(dc, b, rootP, bw * 1.2f * rk, bh * 1.1f, 14f * u, C3(a));
        Say(dc, b, lines[0].Trim(), 40f * u, rootP, Rgb(0.1f, 0.1f, 0.12f, a), scale: rk);
    }
}

// =====================================================================
//  マトリクス図（4象限）
//   「文字」に カンマ区切りで「右上,左上,左下,右下,横軸,縦軸」。値・値2で点が動く
// =====================================================================
public sealed class Matrix4Plugin : QuickShapePlugin
{
    public override string Name => "演出：マトリクス図（4象限）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new Matrix4Parameter(sharedData);
}

public sealed class Matrix4Parameter : QuickShapeParameter
{
    public Matrix4Parameter() : this(null) { }
    public Matrix4Parameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "スター,問題児,負け犬,金のなる木,シェア,成長率";
    }
    protected override double DefAmount => 70;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "マスと軸の名前", Tip: "カンマ区切りで「右上,左上,左下,右下,横軸,縦軸」", Group: "「マトリクス図（4象限）」の設定"),
        ["Amount"] = new ParamGuide(Label: "点の横の位置", Tip: "0=左はし 50=まん中 100=右はし", Group: "「マトリクス図（4象限）」の設定"),
        ["Amount2"] = new ParamGuide(Label: "点の縦の位置", Tip: "0=下 50=まん中 100=上", Group: "「マトリクス図（4象限）」の設定"),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "点の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        string L(int i) => i < it.Count ? it[i] : "";
        float s = 330f * u;
        var cols = new[] { Hex(0x43A047), Hex(0xFB8C00), Hex(0x757575), Hex(0x1E88E5) };
        var off = new[] { new Vector2(1, -1), new Vector2(-1, -1), new Vector2(-1, 1), new Vector2(1, 1) };
        for (int i = 0; i < 4; i++)
        {
            float p = Intro(c, i * 0.12f, 0.3f);
            var q = o + off[i] * s / 2f;
            RBox(dc, b, q, (s - 10f * u) * p, (s - 10f * u) * p, 14f * u, Col(cols[i], 0.75f * a));
            Say(dc, b, L(i), 40f * u, q, Rgb(1, 1, 1, a * p));
        }
        Ln(dc, b, o - new Vector2(s * 1.1f, 0), o + new Vector2(s * 1.15f, 0), Rgb(1, 1, 1, a), 5f * u);
        Ln(dc, b, o + new Vector2(0, s * 1.1f), o - new Vector2(0, s * 1.15f), Rgb(1, 1, 1, a), 5f * u);
        Say(dc, b, L(4), 32f * u, o + new Vector2(s * 1.15f, 36f * u), Rgb(1, 1, 1, a));
        Say(dc, b, L(5), 32f * u, o - new Vector2(-60f * u, s * 1.15f), Rgb(1, 1, 1, a));
        var dp = o + new Vector2((Amt(c) - 0.5f) * 2f * s, -(Amt2(c) - 0.5f) * 2f * s);
        float pulse = 1f + 0.2f * MathF.Sin(t * 5f);
        Dot(dc, b, dp, 22f * u * pulse, C3(a));
        Ring(dc, b, dp, 34f * u * pulse, C3(0.6f * a), 4f * u);
    }
}

// =====================================================================
//  サイクル図（循環）
//   「文字」に1行ずつ。円く並び、矢印でぐるっとつながる。光がめぐる
// =====================================================================
public sealed class CycleDiagramPlugin : QuickShapePlugin
{
    public override string Name => "演出：サイクル図（循環）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CycleDiagramParameter(sharedData);
}

public sealed class CycleDiagramParameter : QuickShapeParameter
{
    public CycleDiagramParameter() : this(null) { }
    public CycleDiagramParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "計画\n実行\n評価\n改善";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "項目", Tip: "1行に1つ。円く並びます", Group: "「サイクル図（循環）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var items = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        int n = items.Count;
        if (n == 0) return;
        float R = 300f * u, nr = 95f * u;
        for (int i = 0; i < n; i++)
        {
            float a0 = -MathF.PI / 2f + i * MathF.PI * 2f / n + 0.38f, a1 = -MathF.PI / 2f + (i + 1) * MathF.PI * 2f / n - 0.38f;
            float p = Intro(c, 0.4f + i * 0.2f, 0.4f);
            if (p <= 0f) continue;
            var arc = Gfx.ArcPts(o, R, a0, a0 + (a1 - a0) * p, 24);
            Path(dc, b, arc, Rgb(1, 1, 1, 0.85f * a), 7f * u);
            if (p >= 1f) Poly(dc, b, BubbleKit.ArrowHead(arc[^1], new Vector2(-MathF.Sin(a1), MathF.Cos(a1)), 24f * u), Rgb(1, 1, 1, a));
        }
        float hl = Gfx.Mod(t * 0.4f * Spd(c), 1f) * n;
        for (int i = 0; i < n; i++)
        {
            float k = DrawKit.EaseOutBack(Intro(c, i * 0.2f, 0.35f));
            var q = Pol(o, R, -MathF.PI / 2f + i * MathF.PI * 2f / n);
            float glow = MathF.Max(0f, 1f - MathF.Abs(hl - i));
            if (glow > 0f) DrawKit.Glow(dc, q, nr * 1.8f, Col(BubbleKit.Palette(0, i), 0.5f * glow * a));
            Dot(dc, b, q, nr * k, Col(BubbleKit.Palette(0, i), a));
            Say(dc, b, items[i], 36f * u, q, Rgb(1, 1, 1, a), maxW: nr * 1.7f, scale: k);
        }
    }
}

// =====================================================================
//  ビフォーアフターの比較スライダー
//   「値」の位置に区切り線。左がBEFORE、右がAFTER(文字で名前を変えられる)。重ねた画像の比較に
// =====================================================================
public sealed class BeforeAfterPlugin : QuickShapePlugin
{
    public override string Name => "演出：ビフォーアフターの比較スライダー";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BeforeAfterParameter(sharedData);
}

public sealed class BeforeAfterParameter : QuickShapeParameter
{
    public BeforeAfterParameter() : this(null) { }
    public BeforeAfterParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "BEFORE,AFTER";
        Background = false;
    }
    protected override double DefAmount => 50;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "左右の名前", Tip: "カンマ区切りで「左,右」", Group: "「ビフォーアフターの比較スライダー」の設定"),
        ["Amount"] = new ParamGuide(Label: "区切り線の位置", Tip: "0=左はし 100=右はし", Group: "「ビフォーアフターの比較スライダー」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "左側を少し暗くする", Group: "「ビフォーアフターの比較スライダー」の設定"),
        ["Color1"] = new ParamGuide(Label: "右の札の色"),
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
        float x = w * Amt(c);
        float wig = (1f - Intro(c, 0f, 1.2f)) * MathF.Sin(t * 9f) * w * 0.05f;
        x += wig;
        if (Background)
        {
            Box(dc, b, 0, 0, x, h, Rgb(0, 0, 0, 0.25f * a));
        }
        Ln(dc, b, new Vector2(x, 0), new Vector2(x, h), Rgb(1, 1, 1, a), 6f * U0(c));
        var hp = new Vector2(x, h / 2f);
        Dot(dc, b, hp, 40f * U0(c), Rgb(1, 1, 1, a));
        float s0 = 16f * U0(c);
        Poly(dc, b, Pts(hp, s0, s0, -0.4f, 0f, -1.5f, -0.8f, -1.5f, 0.8f), Rgb(0.2f, 0.2f, 0.25f, a));
        Poly(dc, b, Pts(hp, s0, s0, 0.4f, 0f, 1.5f, -0.8f, 1.5f, 0.8f), Rgb(0.2f, 0.2f, 0.25f, a));
        var it = Items;
        string l0 = it.Count > 0 ? it[0] : "BEFORE", l1 = it.Count > 1 ? it[1] : "AFTER";
        float fs = 44f * U0(c);
        var t0 = Measure(l0, fs); var t1 = Measure(l1, fs);
        RBox(dc, b, new Vector2(40f * U0(c) + t0.Width / 2f + 20f * U0(c), 60f * U0(c)), t0.Width + 40f * U0(c), t0.Height + 14f * U0(c), 10f * U0(c), Rgb(0, 0, 0, 0.6f * a));
        Say(dc, b, l0, fs, new Vector2(40f * U0(c) + t0.Width / 2f + 20f * U0(c), 60f * U0(c)), Rgb(1, 1, 1, a));
        RBox(dc, b, new Vector2(w - 40f * U0(c) - t1.Width / 2f - 20f * U0(c), 60f * U0(c)), t1.Width + 40f * U0(c), t1.Height + 14f * U0(c), 10f * U0(c), C1(0.9f * a));
        Say(dc, b, l1, fs, new Vector2(w - 40f * U0(c) - t1.Width / 2f - 20f * U0(c), 60f * U0(c)), Rgb(1, 1, 1, a));
    }
}

// =====================================================================
//  数式・公式の囲み（黒板風）
//   「文字」の式を、黒板の板にチョークで書いたように出す。下に色チョークの線
// =====================================================================
public sealed class FormulaBoxPlugin : QuickShapePlugin
{
    public override string Name => "演出：数式・公式の囲み（黒板風）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FormulaBoxParameter(sharedData);
}

public sealed class FormulaBoxParameter : QuickShapeParameter
{
    public FormulaBoxParameter() : this(null) { }
    public FormulaBoxParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "E = mc²";
        Color1 = Hex(0x2F5D44);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "式", Group: "「数式・公式の囲み（黒板風）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "黒板の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "下線のチョークの色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        string s = Str;
        if (s.Length == 0) return;
        float fs = 90f * u;
        var ts = Measure(s, fs, true, 1400f * u);
        float bw = ts.Width + 160f * u, bh = ts.Height + 120f * u;
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.35f));
        RBox(dc, b, o, (bw + 30f * u) * k, (bh + 30f * u) * k, 10f * u, Col(Hex(0x8A5A34), a));
        RBox(dc, b, o, bw * k, bh * k, 6f * u, C1(a));
        if (k < 0.9f) return;
        float p = Intro(c, 0.3f, 0.9f);
        dc.PushAxisAlignedClip(new RawRectF(o.X - ts.Width / 2f - 10f, o.Y - bh, o.X - ts.Width / 2f + ts.Width * p + 4f, o.Y + bh), AntialiasMode.PerPrimitive);
        TextKit.Draw(dc, b, ts, o - new Vector2(ts.Width, ts.Height) / 2f, Rgb(0.95f, 0.95f, 0.9f, 0.92f * a));
        dc.PopAxisAlignedClip();
        float q = Intro(c, 1.2f, 0.5f);
        if (q > 0f)
        {
            var line = new List<Vector2>();
            for (int i = 0; i <= 20; i++) line.Add(new Vector2(o.X - ts.Width / 2f + ts.Width * q * i / 20f, o.Y + ts.Height / 2f + 16f * u + MathF.Sin(i * 1.3f) * 3f * u));
            Path(dc, b, line, C3(0.9f * a), 8f * u);
        }
    }
}

// =====================================================================
//  大きさ比べ（シルエット）
//   1行に「名前:高さ」。高さに合わせたシルエットが、地面から順に伸びる
// =====================================================================
public sealed class SizeComparePlugin : QuickShapePlugin
{
    public override string Name => "演出：大きさ比べ（シルエット）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SizeCompareParameter(sharedData);
}

public sealed class SizeCompareParameter : QuickShapeParameter
{
    public SizeCompareParameter() : this(null) { }
    public SizeCompareParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "人:1.7\nキリン:5.5\n家:8\nビル:30";
        Color1 = Hex(0x546E7A);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "比べるもの", Tip: "1行に「名前:高さ」", Group: "「大きさ比べ（シルエット）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "シルエットの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "数値の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var rows = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        var data = new List<(string n, float v)>();
        foreach (var r0 in rows)
        {
            int ci = r0.IndexOfAny(new[] { ':', '：' });
            if (ci <= 0) continue;
            float.TryParse(r0[(ci + 1)..].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var fv);
            data.Add((r0[..ci].Trim(), MathF.Max(0.01f, fv)));
        }
        if (data.Count == 0) return;
        float gw = 1400f * u, gh = 650f * u, x0 = o.X - gw / 2f, y1 = o.Y + gh / 2f;
        float mx = data.Max(d => d.v);
        float slot = gw / data.Count;
        Ln(dc, b, new Vector2(x0 - 30f * u, y1), new Vector2(x0 + gw + 30f * u, y1), Rgb(1, 1, 1, 0.9f * a), 5f * u);
        for (int i = 0; i < data.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.2f + i * 0.3f, 0.6f));
            float hh = gh * data[i].v / mx * p, bw = MathF.Min(slot * 0.6f, MathF.Max(30f * u, hh * 0.45f));
            float cx = x0 + slot * (i + 0.5f);
            Box(dc, b, cx - bw / 2f, y1 - hh + bw * 0.25f, cx + bw / 2f, y1, C1(a));
            Oval(dc, b, new Vector2(cx, y1 - hh + bw * 0.25f), bw / 2f, bw * 0.25f, C1(a));
            Say(dc, b, data[i].n, 34f * u, new Vector2(cx, y1 + 36f * u), Rgb(1, 1, 1, a));
            if (p > 0.95f) Say(dc, b, data[i].v.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "m", 36f * u, new Vector2(cx, y1 - hh - 30f * u), C3(a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
        }
    }
}

// =====================================================================
//  ピクトグラムの割合図
//   「数」の人の形を並べ、「値」の割合だけ順に色がつく(例: 100人中○人)
// =====================================================================
public sealed class PictogramPlugin : QuickShapePlugin
{
    public override string Name => "演出：ピクトグラムの割合図";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PictogramParameter(sharedData);
}

public sealed class PictogramParameter : QuickShapeParameter
{
    public PictogramParameter() : this(null) { }
    public PictogramParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 100;
        Color1 = Hex(0xFF5C5C);
        Color2 = Hex(0x5A6070);
    }
    protected override double DefAmount => 37;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "色がつく割合", Tip: "全体の何%に色をつけるか", Group: "「ピクトグラムの割合図」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "人の数(全体)", Min: 1, Max: 400, Group: "「ピクトグラムの割合図」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "色がつく人"),
        ["Color2"] = new ParamGuide(Label: "残りの人"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Clamp(Count, 1, 400);
        int cols = (int)MathF.Ceiling(MathF.Sqrt(n * 1.8f));
        int rowsN = (int)MathF.Ceiling(n / (float)cols);
        float cell = MathF.Min(1300f * u / cols, 650f * u / rowsN);
        float x0 = o.X - cols * cell / 2f, y0 = o.Y - rowsN * cell / 2f;
        int on = (int)MathF.Round(n * Amt(c) * DrawKit.EaseOut(Intro(c, 0.3f, 1.5f)));
        for (int i = 0; i < n; i++)
        {
            var p = new Vector2(x0 + (i % cols + 0.5f) * cell, y0 + (i / cols + 0.5f) * cell);
            var col = i < on ? C1(a) : C2(a);
            Dot(dc, b, p - new Vector2(0, cell * 0.22f), cell * 0.14f, col);
            Poly(dc, b, Pts(p, cell, cell, -0.2f, -0.04f, 0.2f, -0.04f, 0.24f, 0.38f, -0.24f, 0.38f), col);
        }
        Say(dc, b, on.ToString() + " / " + n.ToString(), 60f * u, new Vector2(o.X, y0 + rowsN * cell + 60f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
    }
}

// =====================================================================
//  天秤
//   「値」が50で水平、0で左、100で右に傾く。「文字」にカンマ区切りで左右の名前
// =====================================================================
public sealed class BalanceScalePlugin : QuickShapePlugin
{
    public override string Name => "演出：天秤";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BalanceScaleParameter(sharedData);
}

public sealed class BalanceScaleParameter : QuickShapeParameter
{
    public BalanceScaleParameter() : this(null) { }
    public BalanceScaleParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "理想,現実";
        Color1 = Hex(0xC9A227);
    }
    protected override double DefAmount => 70;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "左右の名前", Tip: "カンマ区切りで「左,右」", Group: "「天秤」の設定"),
        ["Amount"] = new ParamGuide(Label: "傾き", Tip: "0=左が下がる 50=水平 100=右が下がる", Group: "「天秤」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "天秤の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "支点の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float tilt = (Amt(c) - 0.5f) * 0.6f;
        float settle = Intro(c, 0f, 1.2f);
        tilt *= 1f + MathF.Sin(settle * 14f) * (1f - settle) * 0.8f;
        var top = o + new Vector2(0, -220f * u);
        Poly(dc, b, Pts(o + new Vector2(0, 260f * u), u, u, -140f, 0f, 140f, 0f, 90f, -30f, -90f, -30f), C1(a));
        Box(dc, b, o.X - 12f * u, top.Y, o.X + 12f * u, o.Y + 235f * u, C1(a));
        var lt = top + new Vector2(-MathF.Cos(tilt), -MathF.Sin(tilt)) * 380f * u;
        var rt = top + new Vector2(MathF.Cos(tilt), MathF.Sin(tilt)) * 380f * u;
        Ln(dc, b, lt, rt, C1(a), 14f * u);
        Dot(dc, b, top, 22f * u, C3(a));
        var it = Items;
        foreach (var (end, i) in new[] { (lt, 0), (rt, 1) })
        {
            var pan = end + new Vector2(0, 200f * u);
            Ln(dc, b, end, pan + new Vector2(-110f * u, 0), C1(0.8f * a), 4f * u);
            Ln(dc, b, end, pan + new Vector2(110f * u, 0), C1(0.8f * a), 4f * u);
            Poly(dc, b, Pts(pan, u, u, -130f, 0f, 130f, 0f, 90f, 40f, -90f, 40f), C1(a));
            if (i < it.Count) Say(dc, b, it[i], 44f * u, pan + new Vector2(0, -50f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
        }
    }
}

// =====================================================================
//  ガントチャート
//   1行に「作業:始め-終わり」(0〜100)。帯が伸び、「値」の位置に今日の線
// =====================================================================
public sealed class GanttPlugin : QuickShapePlugin
{
    public override string Name => "演出：ガントチャート";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GanttParameter(sharedData);
}

public sealed class GanttParameter : QuickShapeParameter
{
    public GanttParameter() : this(null) { }
    public GanttParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "企画:0-20\n台本:15-40\n収録:35-60\n編集:55-90\n公開:88-100";
    }
    protected override double DefAmount => 62;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "作業", Tip: "1行に「作業名:始め-終わり」(0〜100)", Group: "「ガントチャート」の設定"),
        ["Amount"] = new ParamGuide(Label: "今日の線の位置", Tip: "0〜100(帯と同じ目盛り)", Group: "「ガントチャート」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
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
        var rows = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        var data = new List<(string n, float s0, float s1)>();
        foreach (var r0 in rows)
        {
            int ci = r0.IndexOfAny(new[] { ':', '：' });
            if (ci <= 0) continue;
            var se = r0[(ci + 1)..].Split('-');
            float.TryParse(se[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f0);
            float f1 = f0;
            if (se.Length > 1) float.TryParse(se[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out f1);
            data.Add((r0[..ci].Trim(), f0 / 100f, f1 / 100f));
        }
        if (data.Count == 0) return;
        float gw = 1150f * u, rh = 80f * u, lw0 = 220f * u;
        float x0 = o.X - gw / 2f + lw0 / 2f, y0 = o.Y - data.Count * rh / 2f;
        for (int i = 0; i <= 10; i++) Ln(dc, b, new Vector2(x0 + gw * i / 10f, y0 - 10f * u), new Vector2(x0 + gw * i / 10f, y0 + data.Count * rh), Rgb(1, 1, 1, 0.12f * a), 2f * u);
        for (int i = 0; i < data.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.2f + i * 0.15f, 0.5f));
            float y = y0 + i * rh + rh / 2f;
            SayAt(dc, b, data[i].n, 34f * u, new Vector2(x0 - lw0, y - 22f * u), Rgb(1, 1, 1, a));
            float bx0 = x0 + gw * data[i].s0, bx1 = bx0 + gw * (data[i].s1 - data[i].s0) * p;
            RBox(dc, b, new Vector2((bx0 + bx1) / 2f, y), MathF.Max(1f, bx1 - bx0), rh * 0.55f, 10f * u, Col(BubbleKit.Palette(0, i), a));
        }
        float tx = x0 + gw * Amt(c);
        Ln(dc, b, new Vector2(tx, y0 - 30f * u), new Vector2(tx, y0 + data.Count * rh + 10f * u), Rgb(1, 0.3f, 0.3f, a), 4f * u);
        Say(dc, b, "今日", 28f * u, new Vector2(tx, y0 - 50f * u), Rgb(1, 0.4f, 0.4f, a));
    }
}

// =====================================================================
//  温度計・湿度計
//   「値」が目盛りの高さ。「文字」に表示する値(例: 36.5℃)。バリエーション2は丸いメーター
// =====================================================================
public sealed class ThermometerPlugin : QuickShapePlugin
{
    public override string Name => "演出：温度計・湿度計";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ThermometerParameter(sharedData);
}

public sealed class ThermometerParameter : QuickShapeParameter
{
    public ThermometerParameter() : this(null) { }
    public ThermometerParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "36.5℃";
        Color1 = Hex(0xE53935);
    }
    protected override double DefAmount => 65;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "表示する値", Tip: "例: 36.5℃", Group: "「温度計・湿度計」の設定"),
        ["Amount"] = new ParamGuide(Label: "目盛りの高さ", Group: "「温度計・湿度計」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=温度計　2=丸いメーター", Min: 1, Max: 2, Group: "「温度計・湿度計」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "中の液の色"),
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
        float lv = Amt(c) * DrawKit.EaseOut(Intro(c, 0.2f, 1.2f));
        if (Variant >= 2)
        {
            float R = 230f * u;
            var arc = Gfx.ArcPts(o, R, MathF.PI * 0.75f, MathF.PI * 2.25f, 60);
            Path(dc, b, arc, Rgb(1, 1, 1, 0.25f * a), 30f * u);
            Path(dc, b, Gfx.ArcPts(o, R, MathF.PI * 0.75f, MathF.PI * 0.75f + MathF.PI * 1.5f * lv, 60), C1(a), 30f * u);
            var nd = Pol(o, R * 0.8f, MathF.PI * 0.75f + MathF.PI * 1.5f * lv);
            Ln(dc, b, o, nd, Rgb(1, 1, 1, a), 8f * u);
            Dot(dc, b, o, 18f * u, Rgb(1, 1, 1, a));
            Say(dc, b, Str, 70f * u, o + new Vector2(0, R * 0.55f), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
            return;
        }
        float th = 600f * u, tw0 = 56f * u;
        var bulb = o + new Vector2(0, th / 2f);
        RBox(dc, b, o, tw0 + 24f * u, th + 24f * u, tw0, Rgb(1, 1, 1, 0.9f * a));
        Dot(dc, b, bulb, 70f * u, Rgb(1, 1, 1, 0.9f * a));
        Dot(dc, b, bulb, 56f * u, C1(a));
        float top = bulb.Y - (th * 0.92f) * lv;
        Box(dc, b, o.X - tw0 / 2f + 4f * u, top, o.X + tw0 / 2f - 4f * u, bulb.Y, C1(a));
        for (int i = 0; i <= 10; i++) Ln(dc, b, new Vector2(o.X + tw0 / 2f + 18f * u, bulb.Y - th * 0.92f * i / 10f), new Vector2(o.X + tw0 / 2f + (i % 5 == 0 ? 50f : 34f) * u, bulb.Y - th * 0.92f * i / 10f), Rgb(1, 1, 1, 0.8f * a), 3f * u);
        Say(dc, b, Str, 64f * u, new Vector2(o.X + 200f * u, top), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
    }
}

// =====================================================================
//  虫めがねの強調枠
//   虫めがねの枠が現れて、見せたい所を強調する(中は透明)。「値」でレンズの大きさ
// =====================================================================
public sealed class MagnifierFramePlugin : QuickShapePlugin
{
    public override string Name => "演出：虫めがねの強調枠";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MagnifierFrameParameter(sharedData);
}

public sealed class MagnifierFrameParameter : QuickShapeParameter
{
    public MagnifierFrameParameter() : this(null) { }
    public MagnifierFrameParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x37474F);
        Background = false;
    }
    protected override double DefAmount => 50;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "レンズの大きさ", Group: "「虫めがねの強調枠」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "まわりを暗くする", Group: "「虫めがねの強調枠」の設定"),
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
        float R = (120f + 260f * Amt(c)) * u * DrawKit.EaseOutBack(Intro(c, 0f, 0.4f));
        if (Background)
        {
            b.Color = Rgb(0, 0, 0, 0.45f * a);
            DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), DrawKit.EllipsePts(o, R, R, 80));
        }
        var hd = Pol(o, R * 1.05f, MathF.PI * 0.25f);
        var he = Pol(o, R * 1.9f, MathF.PI * 0.25f);
        Ln(dc, b, hd, he, C1(a), 46f * u);
        Ln(dc, b, Vector2.Lerp(hd, he, 0.15f), he, Col(Hex(0x8D6E63), a), 40f * u);
        Ring(dc, b, o, R, C1(a), 26f * u);
        Ring(dc, b, o, R - 13f * u, Rgb(1, 1, 1, 0.35f * a), 4f * u);
        Path(dc, b, Gfx.ArcPts(o, R * 0.78f, MathF.PI * 1.1f, MathF.PI * 1.45f, 12), Rgb(1, 1, 1, 0.5f * a), 10f * u);
    }
}

// =====================================================================
//  番号つき箇条書き
//   「文字」に1行ずつ。番号の丸と一緒に、上から順にスッと出る。バリエーション2はチェックマーク
// =====================================================================
public sealed class BulletListPlugin : QuickShapePlugin
{
    public override string Name => "演出：番号つき箇条書き";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BulletListParameter(sharedData);
}

public sealed class BulletListParameter : QuickShapeParameter
{
    public BulletListParameter() : this(null) { }
    public BulletListParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "結論を先に言う\n理由を3つ挙げる\n具体例で補う\nもう一度まとめる";
        Color1 = Hex(0x1E88E5);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "項目", Tip: "1行に1つ", Group: "「番号つき箇条書き」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=番号　2=チェックマーク", Min: 1, Max: 2, Group: "「番号つき箇条書き」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "丸の色"),
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
        var items = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        if (items.Count == 0) return;
        float rh = 110f * u, fs = 50f * u;
        float y0 = o.Y - items.Count * rh / 2f, x0 = o.X - 520f * u;
        for (int i = 0; i < items.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.2f + i * 0.4f, 0.4f));
            if (p <= 0f) continue;
            float y = y0 + i * rh + rh / 2f, dx = (1f - p) * 80f * u;
            var mc = new Vector2(x0 + 40f * u + dx, y);
            Dot(dc, b, mc, 36f * u, C1(a * p));
            if (Variant >= 2)
                Path(dc, b, Pts(mc, u, u, -16f, 0f, -4f, 14f, 18f, -14f), Rgb(1, 1, 1, a * p), 7f * u);
            else Say(dc, b, (i + 1).ToString(), 40f * u, mc, Rgb(1, 1, 1, a * p));
            SayAt(dc, b, items[i], fs, new Vector2(mc.X + 64f * u, y - fs * 0.68f), Rgb(1, 1, 1, a * p), true, 1000f * u);
        }
    }
}

// =====================================================================
//  ドットの世界地図
//   点で描いた世界地図。ところどころが光る。「数」で光る点の数
// =====================================================================
public sealed class DotWorldMapPlugin : QuickShapePlugin
{
    public override string Name => "演出：ドットの世界地図";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DotWorldMapParameter(sharedData);
}

public sealed class DotWorldMapParameter : QuickShapeParameter
{
    public DotWorldMapParameter() : this(null) { }
    public DotWorldMapParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 12;
        Color1 = Hex(0x4FC3F7);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "光る点の数", Min: 0, Max: 60, Group: "「ドットの世界地図」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "陸の点の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "光る点の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        string[] map =
        {
            "..........................................................",
            "......XXXXXXXXX.........XXX......XXXXXXXXXXXXXXXXXX.......",
            "...XXXXXXXXXXXXXXX.....XXXX...XXXXXXXXXXXXXXXXXXXXXXXX....",
            "....XXXXXXXXXXXXXX.......X...XXXXXXXXXXXXXXXXXXXXXXXXXX...",
            ".....XXXXXXXXXXXX..........XXXXXXXXXXXXXXXXXXXXXXXXXX.X...",
            "......XXXXXXXXXX..........XXXXXXXXXXXXXXXXXXXXXXXXXX..XX..",
            ".......XXXXXXXX...........XXXXXXXXXXXXXXXXXXXXXXXXX..X....",
            "........XXXXX.............XXXXXX.XXXXXXXXXXXXXXXXX........",
            ".........XXX..............XXXXXXXXXXXXXXXXXXXXXXX.........",
            "..........XXX...........XXXXXXXXXXX.XXXXX..XXXX...........",
            "...........XXXX.........XXXXXXXXXXXX.XXXX...XX.X..........",
            "............XXXXXX.......XXXXXXXXXX...XX.....X.XX.........",
            "............XXXXXXX........XXXXXXX.............XXX........",
            ".............XXXXXX........XXXXXX............XXXXXXX......",
            "..............XXXX..........XXXXX...........XXXXXXXX......",
            "..............XXX...........XXXX.............XXXXXX.......",
            "..............XX.............XX................XX......XX.",
            "..............X...........................................",
        };
        int cols = map[0].Length, rowsN = map.Length;
        float cell = MathF.Min(1600f * u / cols, 850f * u / rowsN);
        float x0 = o.X - cols * cell / 2f, y0 = o.Y - rowsN * cell / 2f;
        var land = new List<Vector2>();
        for (int r = 0; r < rowsN; r++)
            for (int k = 0; k < cols; k++)
                if (map[r][k] == 'X')
                {
                    var p = new Vector2(x0 + (k + 0.5f) * cell, y0 + (r + 0.5f) * cell);
                    land.Add(p);
                    float wave = 0.55f + 0.45f * MathF.Sin(t * 1.5f - k * 0.15f);
                    Dot(dc, b, p, cell * 0.32f, C1(a * wave));
                }
        for (int i = 0; i < Count && land.Count > 0; i++)
        {
            var p = land[(int)(DrawKit.H(Seed, i) * land.Count) % land.Count];
            float f = Gfx.Mod(t * 0.6f + DrawKit.H(Seed, i, 1), 1f);
            Ring(dc, b, p, cell * (0.5f + f * 2.5f), C3((1f - f) * a), 2f * u);
            Dot(dc, b, p, cell * 0.45f, C3(a));
        }
    }
}

// =====================================================================
//  方位磁針・コンパス
//   方角の文字がついた方位磁針。針は「値」の向き(0=北、25=東…)を指し、少しゆれて止まる
// =====================================================================
public sealed class CompassPlugin : QuickShapePlugin
{
    public override string Name => "演出：方位磁針・コンパス";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CompassParameter(sharedData);
}

public sealed class CompassParameter : QuickShapeParameter
{
    public CompassParameter() : this(null) { }
    public CompassParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xE53935);
        Color2 = Hex(0xF5F0E6);
    }
    protected override double DefAmount => 0;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "針の向き", Tip: "0=北 25=東 50=南 75=西", Group: "「方位磁針・コンパス」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "北の針の色"),
        ["Color2"] = new ParamGuide(Label: "文字盤の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = 280f * u;
        Dot(dc, b, o, R * 1.08f, Col(Hex(0x8D6E63), a));
        Dot(dc, b, o, R, C2(a));
        for (int i = 0; i < 36; i++)
        {
            float ang = i * MathF.PI * 2f / 36f;
            Ln(dc, b, Pol(o, R * (i % 9 == 0 ? 0.78f : 0.88f), ang), Pol(o, R * 0.96f, ang), Rgb(0.2f, 0.2f, 0.22f, a), 3f * u);
        }
        string[] nm = { "北", "東", "南", "西" };
        for (int i = 0; i < 4; i++) Say(dc, b, nm[i], 52f * u, Pol(o, R * 0.62f, -MathF.PI / 2f + i * MathF.PI / 2f), i == 0 ? C1(a) : Rgb(0.2f, 0.2f, 0.22f, a));
        float settle = Intro(c, 0f, 2f);
        float ang0 = Amt(c) * MathF.PI * 2f + MathF.Sin(t * 8f) * 0.6f * (1f - settle) + MathF.Sin(t * 1.3f) * 0.03f;
        var dir = new Vector2(MathF.Sin(ang0), -MathF.Cos(ang0));
        var nrm = new Vector2(-dir.Y, dir.X);
        Poly(dc, b, new List<Vector2> { o + dir * R * 0.82f, o + nrm * 24f * u, o - nrm * 24f * u }, C1(a));
        Poly(dc, b, new List<Vector2> { o - dir * R * 0.82f, o + nrm * 24f * u, o - nrm * 24f * u }, Rgb(0.3f, 0.32f, 0.36f, a));
        Dot(dc, b, o, 16f * u, Col(Hex(0xC9A227), a));
    }
}
