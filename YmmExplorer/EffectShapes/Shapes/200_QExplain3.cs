using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 411〜430: 解説・図解

public sealed class NormalCurvePlugin : QuickShapePlugin { public override string Name => "演出：正規分布（ベル曲線）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new NormalCurveParameter(s); }
public sealed class NormalCurveParameter : Q3Param
{
    public NormalCurveParameter() : this(null) { }
    public NormalCurveParameter(SharedDataStore? s) : base(s) { Count = 1; Color1 = Hex(0x4FC3F7); Color2 = Hex(0xFFD54A); }
    protected override double DefAmount => 50;
    protected override double DefAmount2 => 38;
    static readonly Dictionary<string, ParamGuide> g = G("正規分布", "Amount=平均の位置", "Amount2=ばらつき", "Count=塗る範囲|1=68%　2=95%　3=99.7%|1|3", "Color1=曲線の色", "Color2=塗る色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), p = DrawKit.EaseOut(Intro(c, 0.1f, 0.9f)), mean = (Amt(c) - 0.5f) * 4f, sd = 0.4f + Amt2(c) * 1.6f;
        var o = P0(c); int k = Math.Clamp(Count, 1, 3);
        float W = 620f * u, H = 360f * u, baseY = o.Y + 190f * u;
        Vector2 At(float x) { float z = (x - mean) / sd; return new Vector2(o.X + x / 4f * W, baseY - MathF.Exp(-0.5f * z * z) * H * p); }
        var fill = new List<Vector2> { new(o.X + (mean - k * sd) / 4f * W, baseY) };
        for (int i = 0; i <= 60; i++) fill.Add(At(mean - k * sd + 2f * k * sd * i / 60f));
        fill.Add(new(o.X + (mean + k * sd) / 4f * W, baseY));
        Poly(dc, b, fill, C2(0.55f * a));
        var line = new List<Vector2>();
        for (int i = 0; i <= 120; i++) line.Add(At(-4f + 8f * i / 120f));
        Path(dc, b, line, C1(a), 6f * u);
        Ln(dc, b, new(o.X - W, baseY), new(o.X + W, baseY), Rgb(1, 1, 1, a), 3f * u);
        Say(dc, b, "全体の" + (k == 1 ? "68" : k == 2 ? "95" : "99.7") + "%", 48f * u, new(o.X + mean / 4f * W, baseY - 120f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 4f * u);
    }
}

public sealed class HistogramPlugin : QuickShapePlugin { public override string Name => "演出：ヒストグラム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HistogramParameter(s); }
public sealed class HistogramParameter : Q3Param
{
    public HistogramParameter() : this(null) { }
    public HistogramParameter(SharedDataStore? s) : base(s) { Text = "12,15,18,21,22,24,25,25,27,28,30,31,33,36,41"; Count = 6; Color1 = Hex(0x66BB6A); }
    static readonly Dictionary<string, ParamGuide> g = G("ヒストグラム", "Text=数の並び|カンマ区切りで入れます", "Count=階級の数||2|30", "Color1=棒の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var v = Nums; if (v.Count == 0) return;
        float u = U(c), min = v.Min(), max = v.Max(), span = MathF.Max(0.001f, max - min);
        var o = P0(c); int n = Math.Clamp(Count, 2, 30); var bins = new int[n];
        foreach (var x in v) bins[Math.Min(n - 1, (int)((x - min) / span * n))]++;
        float W = 1200f * u, H = 560f * u, x0 = o.X - W / 2f, y0 = o.Y + H / 2f, bw = W / n; int top = Math.Max(1, bins.Max());
        for (int i = 0; i < n; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.1f + i * 0.06f, 0.5f)), bh = H * bins[i] / top * p;
            Box(dc, b, x0 + i * bw + 3f * u, y0 - bh, x0 + (i + 1) * bw - 3f * u, y0, C1(0.9f * a));
            if (bins[i] > 0) Say(dc, b, bins[i].ToString(), 34f * u, new(x0 + (i + 0.5f) * bw, y0 - bh - 28f * u), Rgb(1, 1, 1, a * p));
            Say(dc, b, (min + span * i / n).ToString("0.#"), 26f * u, new(x0 + i * bw, y0 + 30f * u), Rgb(1, 1, 1, 0.8f * a));
        }
        Say(dc, b, max.ToString("0.#"), 26f * u, new(x0 + W, y0 + 30f * u), Rgb(1, 1, 1, 0.8f * a));
        Ln(dc, b, new(x0, y0), new(x0 + W, y0), Rgb(1, 1, 1, a), 4f * u);
    }
}

public sealed class BoxPlotPlugin : QuickShapePlugin { public override string Name => "演出：箱ひげ図"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BoxPlotParameter(s); }
public sealed class BoxPlotParameter : Q3Param
{
    public BoxPlotParameter() : this(null) { }
    public BoxPlotParameter(SharedDataStore? s) : base(s) { Text = "1組:32,48,60,72,95\n2組:20,41,55,66,88\n3組:45,58,70,81,99"; Color1 = Hex(0x4FC3F7); Color2 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("箱ひげ図", "Text=組ごとの値|1行に「名前:最小,第1四分位,中央値,第3四分位,最大」", "Color1=箱の色", "Color2=中央値の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var rows = Rows.Select(Cells).Where(x => x.Length >= 6).ToList(); if (rows.Count == 0) return;
        var vals = rows.Select(r => r.Skip(1).Take(5).Select(x => Num(x)).ToArray()).ToList();
        float lo = vals.Min(x => x.Min()), hi = vals.Max(x => x.Max()), span = MathF.Max(0.001f, hi - lo), u = U(c);
        var o = P0(c); float W = 1120f * u, rh = MathF.Min(170f * u, 620f * u / rows.Count), x0 = o.X - W / 2f + 80f * u, y0 = o.Y - rh * rows.Count / 2f;
        float X(float v) => x0 + (v - lo) / span * W;
        for (int i = 0; i < rows.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.1f + i * 0.15f, 0.6f)), y = y0 + rh * (i + 0.5f), bh = rh * 0.28f; var f = vals[i];
            Say(dc, b, rows[i][0], 38f * u, new(x0 - 100f * u, y), Rgb(1, 1, 1, a));
            float M = X(f[2]);
            float P(float q) => M + (X(q) - M) * p;
            Ln(dc, b, new(P(f[0]), y), new(P(f[4]), y), Rgb(1, 1, 1, a), 4f * u);
            foreach (int e in new[] { 0, 4 }) Ln(dc, b, new(P(f[e]), y - bh * 0.7f), new(P(f[e]), y + bh * 0.7f), Rgb(1, 1, 1, a), 4f * u);
            Box(dc, b, P(f[1]), y - bh, P(f[3]), y + bh, C1(0.85f * a));
            Ln(dc, b, new(M, y - bh), new(M, y + bh), C2(a), 7f * u);
        }
    }
}

public sealed class WaterfallChartPlugin : QuickShapePlugin { public override string Name => "演出：ウォーターフォール図"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WaterfallChartParameter(s); }
public sealed class WaterfallChartParameter : Q3Param
{
    public WaterfallChartParameter() : this(null) { }
    public WaterfallChartParameter(SharedDataStore? s) : base(s) { Text = "売上:+1000\n原価:-420\n人件費:-260\n広告費:-90\n利益:="; Color1 = Hex(0x66BB6A); Color2 = Hex(0xEF5350); Color3 = Hex(0x4FC3F7); }
    static readonly Dictionary<string, ParamGuide> g = G("ウォーターフォール図", "Text=項目と増減|1行に「項目:+100」か「項目:-50」。「項目:=」で合計", "Color1=増えた色", "Color2=減った色", "Color3=合計の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var rows = Rows.Select(Cells).Where(x => x.Length >= 2).ToList(); if (rows.Count == 0) return;
        var from = new float[rows.Count]; var to = new float[rows.Count]; float cur = 0f;
        for (int i = 0; i < rows.Count; i++) { if (rows[i][1] == "=") { from[i] = 0; to[i] = cur; } else { from[i] = cur; cur += Num(rows[i][1]); to[i] = cur; } }
        float lo = MathF.Min(0, MathF.Min(from.Min(), to.Min())), hi = MathF.Max(from.Max(), to.Max()), span = MathF.Max(0.001f, hi - lo), u = U(c);
        var o = P0(c); float W = 1280f * u, H = 560f * u, x0 = o.X - W / 2f, y0 = o.Y + H / 2f, bw = W / rows.Count;
        float Y(float v) => y0 - (v - lo) / span * H;
        for (int i = 0; i < rows.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.1f + i * 0.22f, 0.45f)), x = x0 + i * bw;
            bool total = rows[i][1] == "=";
            var col = total ? C3(a) : to[i] >= from[i] ? C1(a) : C2(a);
            Box(dc, b, x + bw * 0.14f, Y(from[i]), x + bw * 0.86f, Y(from[i] + (to[i] - from[i]) * p), col);
            if (i + 1 < rows.Count && p >= 1f) Ln(dc, b, new(x + bw * 0.86f, Y(to[i])), new(x + bw * 1.14f, Y(to[i])), Rgb(1, 1, 1, 0.6f * a), 2f * u);
            string label = total ? to[i].ToString("#,0") : (to[i] - from[i]).ToString("+#,0;-#,0;0");
            Say(dc, b, label, 34f * u, new(x + bw / 2f, MathF.Min(Y(from[i]), Y(to[i])) - 28f * u), Rgb(1, 1, 1, a * p));
            Say(dc, b, rows[i][0], 32f * u, new(x + bw / 2f, y0 + 36f * u), Rgb(1, 1, 1, a), maxW: bw);
        }
    }
}

public sealed class FunnelChartPlugin : QuickShapePlugin { public override string Name => "演出：ファネル図（じょうご）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FunnelChartParameter(s); }
public sealed class FunnelChartParameter : Q3Param
{
    public FunnelChartParameter() : this(null) { }
    public FunnelChartParameter(SharedDataStore? s) : base(s) { Text = "表示:10000\nクリック:3200\n登録:900\n購入:240"; Color1 = Hex(0x7E57C2); Color2 = Hex(0x26C6DA); }
    static readonly Dictionary<string, ParamGuide> g = G("ファネル図", "Text=段階と数|1行に「段階名:数」", "Background=減り方の割合を表示", "Color1=上の色", "Color2=下の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var rows = Rows.Select(Cells).Where(x => x.Length >= 2).ToList(); if (rows.Count == 0) return;
        var v = rows.Select(r => MathF.Max(0f, Num(r[1]))).ToArray(); float top = MathF.Max(1f, v.Max()), u = U(c);
        var o = P0(c); float W = 1000f * u, H = 640f * u, rh = H / rows.Count, y0 = o.Y - H / 2f;
        for (int i = 0; i < rows.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.1f + i * 0.18f, 0.5f)), w0 = W * MathF.Max(0.12f, v[i] / top) * p, w1 = W * MathF.Max(0.12f, (i + 1 < v.Length ? v[i + 1] : v[i]) / top) * p, y = y0 + i * rh;
            var col = Gfx.Mix(Color1, Color2, rows.Count > 1 ? i / (rows.Count - 1f) : 0f);
            Poly(dc, b, new List<Vector2> { new(o.X - w0 / 2f, y), new(o.X + w0 / 2f, y), new(o.X + w1 / 2f, y + rh - 5f * u), new(o.X - w1 / 2f, y + rh - 5f * u) }, Col(col, a));
            Say(dc, b, rows[i][0] + "  " + v[i].ToString("#,0"), 38f * u, new(o.X, y + rh / 2f), Rgb(1, 1, 1, a * p), Rgb(0, 0, 0, 0.5f * a), 3f * u);
            if (Background && i > 0 && v[i - 1] > 0f) Say(dc, b, (v[i] / v[i - 1] * 100f).ToString("0.#") + "%", 30f * u, new(o.X + W / 2f + 90f * u, y), Col(Color2, a * p));
        }
    }
}

public sealed class HeatmapPlugin : QuickShapePlugin { public override string Name => "演出：ヒートマップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HeatmapParameter(s); }
public sealed class HeatmapParameter : Q3Param
{
    public HeatmapParameter() : this(null) { }
    public HeatmapParameter(SharedDataStore? s) : base(s) { Text = "-:月,火,水,木,金\n朝:2,4,3,5,6\n昼:5,7,6,8,9\n夜:8,6,9,7,10"; Color1 = Hex(0x17324D); Color2 = Hex(0xFF7043); }
    static readonly Dictionary<string, ParamGuide> g = G("ヒートマップ", "Text=行と列と数|1行目は「-:列1,列2…」。2行目から「行名:数,数…」", "Background=数を表示", "Color1=小さい色", "Color2=大きい色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var rows = Rows.Select(Cells).ToList(); if (rows.Count < 2) return;
        var head = rows[0].Skip(1).ToArray(); var body = rows.Skip(1).ToList(); int nx = head.Length, ny = body.Count; if (nx == 0) return;
        var all = body.SelectMany(r => r.Skip(1).Select(x => Num(x))).ToList(); if (all.Count == 0) return;
        float lo = all.Min(), span = MathF.Max(0.001f, all.Max() - lo), u = U(c), cw = MathF.Min(190f * u, 1200f * u / nx), ch = MathF.Min(130f * u, 620f * u / ny);
        var o = P0(c); float x0 = o.X - cw * nx / 2f + 50f * u, y0 = o.Y - ch * ny / 2f + 30f * u;
        for (int x = 0; x < nx; x++) Say(dc, b, head[x], 34f * u, new(x0 + (x + 0.5f) * cw, y0 - 36f * u), Rgb(1, 1, 1, a));
        for (int y = 0; y < ny; y++)
        {
            Say(dc, b, body[y][0], 34f * u, new(x0 - 70f * u, y0 + (y + 0.5f) * ch), Rgb(1, 1, 1, a));
            for (int x = 0; x < nx && x + 1 < body[y].Length; x++)
            {
                float v = Num(body[y][x + 1]), p = DrawKit.EaseOut(Intro(c, 0.1f + (x + y) * 0.06f, 0.4f));
                RBox(dc, b, new(x0 + (x + 0.5f) * cw, y0 + (y + 0.5f) * ch), (cw - 6f * u) * p, (ch - 6f * u) * p, 10f * u, Col(Gfx.Mix(Color1, Color2, (v - lo) / span), a));
                if (Background) Say(dc, b, v.ToString("0.#"), 32f * u, new(x0 + (x + 0.5f) * cw, y0 + (y + 0.5f) * ch), Rgb(1, 1, 1, a * p));
            }
        }
    }
}

public sealed class RoadmapPlugin : QuickShapePlugin { public override string Name => "演出：ロードマップ（道と旗）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RoadmapParameter(s); }
public sealed class RoadmapParameter : Q3Param
{
    public RoadmapParameter() : this(null) { }
    public RoadmapParameter(SharedDataStore? s) : base(s) { Text = "企画\n収録\n編集\n公開"; Color1 = Hex(0x546E7A); Color2 = Hex(0xFF7043); Color3 = Hex(0xFFD54A); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("ロードマップ", "Text=旗の名前|1行に1つ", "Amount=いまの位置", "Color1=道の色", "Color2=旗の色", "Color3=進んだ道の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var rows = Rows; if (rows.Count == 0) return;
        float u = U(c), now = Amt(c) * DrawKit.EaseOut(Intro(c, 0.1f, 1.2f)); var o = P0(c);
        var road = new List<Vector2>();
        for (int i = 0; i <= 120; i++) { float k = i / 120f; road.Add(o + new Vector2((k - 0.5f) * 1400f * u, MathF.Sin(k * MathF.PI * 3f) * 190f * u + 90f * u)); }
        Path(dc, b, road, C1(a), 46f * u);
        Path(dc, b, BubbleKit.PartByLength(road, now), C3(a), 24f * u);
        for (int i = 0; i < rows.Count; i++)
        {
            float k = rows.Count == 1 ? 1f : i / (rows.Count - 1f); var q = road[(int)(k * 120f)]; bool on = now >= k - 0.001f;
            Ln(dc, b, q, q - new Vector2(0, 120f * u), Rgb(1, 1, 1, a), 5f * u);
            var f = q - new Vector2(0, 120f * u);
            Poly(dc, b, new List<Vector2> { f, f + new Vector2(76f * u, 24f * u), f + new Vector2(0, 50f * u) }, on ? C2(a) : Rgb(0.5f, 0.5f, 0.5f, a));
            Say(dc, b, rows[i], 34f * u, f - new Vector2(0, 34f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
        }
        var me = road[(int)(now * 120f)];
        Dot(dc, b, me, 22f * u, Rgb(1, 1, 1, a)); Dot(dc, b, me, 13f * u, C2(a));
    }
}

public sealed class LeverDiagramPlugin : QuickShapePlugin { public override string Name => "演出：てこ・滑車の図"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LeverDiagramParameter(s); }
public sealed class LeverDiagramParameter : Q3Param
{
    public LeverDiagramParameter() : this(null) { }
    public LeverDiagramParameter(SharedDataStore? s) : base(s) { Text = "30,20"; Color1 = Hex(0xC9A36A); Color2 = Hex(0xEF5350); Color3 = Hex(0x4FC3F7); }
    protected override double DefAmount => 40;
    static readonly Dictionary<string, ParamGuide> g = G("てこ・滑車", "Text=おもりの重さ|「左の重さ,右の重さ」", "Amount=支点の位置", "Variant=種類|1=てこ　2=滑車|1|2", "Color1=棒の色", "Color2=左の色", "Color3=右の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var n = Nums; float L = n.Count > 0 ? n[0] : 30f, R = n.Count > 1 ? n[1] : 20f, u = U(c), p = DrawKit.EaseOut(Intro(c, 0.2f, 1f)); var o = P0(c);
        if (Variant == 2)
        {
            var hub = o - new Vector2(0, 220f * u); float d = Math.Clamp((L - R) / MathF.Max(1f, L + R), -1f, 1f) * 140f * u * p;
            Ln(dc, b, hub - new Vector2(0, 100f * u), hub, Rgb(1, 1, 1, a), 6f * u); Ring(dc, b, hub, 90f * u, C1(a), 16f * u); Dot(dc, b, hub, 12f * u, Rgb(1, 1, 1, a));
            var lp = hub + new Vector2(-90f * u, 300f * u + d); var rp = hub + new Vector2(90f * u, 300f * u - d);
            Ln(dc, b, hub - new Vector2(90f * u, 0), lp, Rgb(1, 1, 1, a), 4f * u); Ln(dc, b, hub + new Vector2(90f * u, 0), rp, Rgb(1, 1, 1, a), 4f * u);
            RBox(dc, b, lp + new Vector2(0, 45f * u), 120f * u, 90f * u, 10f * u, C2(a)); RBox(dc, b, rp + new Vector2(0, 45f * u), 120f * u, 90f * u, 10f * u, C3(a));
            Say(dc, b, L.ToString("0.#"), 40f * u, lp + new Vector2(0, 45f * u), Rgb(1, 1, 1, a)); Say(dc, b, R.ToString("0.#"), 40f * u, rp + new Vector2(0, 45f * u), Rgb(1, 1, 1, a));
            return;
        }
        float bar = 1100f * u, fx = (Amt(c) - 0.5f) * bar, ll = bar / 2f + fx, rl = bar / 2f - fx, tq = L * ll - R * rl, tilt = Math.Clamp(tq / MathF.Max(1f, L * ll + R * rl) * 1.4f, -0.3f, 0.3f) * p;
        var piv = o + new Vector2(fx, 60f * u); var dir = new Vector2(MathF.Cos(tilt), -MathF.Sin(tilt)); var lEnd = piv - dir * ll; var rEnd = piv + dir * rl;
        Poly(dc, b, new List<Vector2> { piv, piv + new Vector2(60f * u, 110f * u), piv + new Vector2(-60f * u, 110f * u) }, Rgb(0.75f, 0.75f, 0.8f, a));
        Ln(dc, b, lEnd, rEnd, C1(a), 20f * u);
        RBox(dc, b, lEnd + new Vector2(0, -50f * u), 110f * u, 90f * u, 10f * u, C2(a)); RBox(dc, b, rEnd + new Vector2(0, -50f * u), 110f * u, 90f * u, 10f * u, C3(a));
        Say(dc, b, L.ToString("0.#"), 38f * u, lEnd + new Vector2(0, -50f * u), Rgb(1, 1, 1, a)); Say(dc, b, R.ToString("0.#"), 38f * u, rEnd + new Vector2(0, -50f * u), Rgb(1, 1, 1, a));
        Say(dc, b, "支点", 32f * u, piv + new Vector2(0, 145f * u), Rgb(1, 1, 1, a)); Say(dc, b, "力点", 30f * u, lEnd + new Vector2(0, 50f * u), Col(Color2, a)); Say(dc, b, "作用点", 30f * u, rEnd + new Vector2(0, 50f * u), Col(Color3, a));
    }
}

public sealed class CircuitDiagramPlugin : QuickShapePlugin { public override string Name => "演出：回路図（電池・豆電球）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CircuitDiagramParameter(s); }
public sealed class CircuitDiagramParameter : Q3Param
{
    public CircuitDiagramParameter() : this(null) { }
    public CircuitDiagramParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xE0E0E0); Color2 = Hex(0xFFD54A); Color3 = Hex(0x4FC3F7); }
    protected override double DefAmount => 30;
    static readonly Dictionary<string, ParamGuide> g = G("回路図", "Variant=つなぎ方|1=直列　2=並列|1|2", "Amount=スイッチを入れる時刻|0〜100 = 0〜5秒", "Background=スイッチを使う|オフにすると切れたまま", "Speed=電流の速さ", "Color1=導線の色", "Color2=電球の色", "Color3=電流の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, on = Background ? Sub(t, Amt(c) * 5f, 0.2f) : 0f, W = 520f * u, H = 270f * u; var o = P0(c);
        var loop = new List<Vector2> { o + new Vector2(-W, H), o + new Vector2(-W, -H), o + new Vector2(W, -H), o + new Vector2(W, H), o + new Vector2(-W, H) };
        Path(dc, b, loop, C1(a), 7f * u);
        var bat = o + new Vector2(-W, 0); Box(dc, b, bat.X - 34f * u, bat.Y - 16f * u, bat.X + 34f * u, bat.Y + 16f * u, Rgb(0.08f, 0.1f, 0.14f, a));
        Ln(dc, b, bat + new Vector2(-48f * u, -14f * u), bat + new Vector2(48f * u, -14f * u), C1(a), 7f * u); Ln(dc, b, bat + new Vector2(-24f * u, 14f * u), bat + new Vector2(24f * u, 14f * u), C1(a), 12f * u);
        Say(dc, b, "電池", 32f * u, bat - new Vector2(100f * u, 0), Rgb(1, 1, 1, a));
        var sw = o + new Vector2(0, H); Box(dc, b, sw.X - 62f * u, sw.Y - 8f * u, sw.X + 62f * u, sw.Y + 8f * u, Rgb(0.08f, 0.1f, 0.14f, a));
        Dot(dc, b, sw - new Vector2(60f * u, 0), 10f * u, C1(a)); Dot(dc, b, sw + new Vector2(60f * u, 0), 10f * u, C1(a));
        float ang = (1f - on) * -0.65f; Ln(dc, b, sw - new Vector2(60f * u, 0), sw - new Vector2(60f * u, 0) + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * 122f * u, C1(a), 7f * u);
        Say(dc, b, "スイッチ", 30f * u, sw + new Vector2(0, 55f * u), Rgb(1, 1, 1, a));
        var bulbs = Variant == 2 ? new[] { o + new Vector2(W * 0.2f, -H), o + new Vector2(W * 0.2f, -H * 0.1f) } : new[] { o + new Vector2(-W * 0.3f, -H), o + new Vector2(W * 0.4f, -H) };
        if (Variant == 2) Path(dc, b, new List<Vector2> { o + new Vector2(-W, -H * 0.1f), o + new Vector2(W, -H * 0.1f) }, C1(a), 7f * u);
        foreach (var q in bulbs)
        {
            if (on > 0f) Dot(dc, b, q, 80f * u, C2(0.22f * on * a));
            Dot(dc, b, q, 42f * u, on > 0f ? C2(a) : Rgb(0.4f, 0.4f, 0.42f, a)); Ring(dc, b, q, 42f * u, Rgb(1, 1, 1, a), 4f * u);
            Ln(dc, b, q + new Vector2(-24f * u, -24f * u), q + new Vector2(24f * u, 24f * u), Rgb(0.2f, 0.2f, 0.2f, a), 4f * u); Ln(dc, b, q + new Vector2(24f * u, -24f * u), q + new Vector2(-24f * u, 24f * u), Rgb(0.2f, 0.2f, 0.2f, a), 4f * u);
        }
        if (on >= 1f)
        {
            float len = BubbleKit.Length(loop);
            for (int i = 0; i < 16; i++)
            {
                float k = Gfx.Mod(i / 16f + t * Spd(c) * 0.12f, 1f); var part = BubbleKit.PartByLength(loop, k);
                if (part.Count > 0 && len > 0f) Dot(dc, b, part[^1], 7f * u, C3(a));
            }
        }
    }
}

public sealed class ChemEquationPlugin : QuickShapePlugin { public override string Name => "演出：化学反応式"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ChemEquationParameter(s); }
public sealed class ChemEquationParameter : Q3Param
{
    public ChemEquationParameter() : this(null) { }
    public ChemEquationParameter(SharedDataStore? s) : base(s) { Text = "2H2 + O2 -> 2H2O"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("化学反応式", "Text=反応式|矢印は -> と書きます。元素の後ろの数字は自動で小さくなります", "Color1=文字の色", "Color2=矢印の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    float Formula(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, string s, Vector2 p, float size, Color4 col, bool draw)
    {
        float x = p.X;
        for (int i = 0; i < s.Length; i++)
        {
            bool sub = char.IsDigit(s[i]) && i > 0 && (char.IsLetter(s[i - 1]) || s[i - 1] == ')' || char.IsDigit(s[i - 1]) && i > 1 && char.IsLetter(s[i - 2]));
            float sz = sub ? size * 0.6f : size; string ch = s[i].ToString();
            var m = Measure(ch == " " ? "n" : ch, sz);
            if (draw && ch != " ") SayAt(dc, b, ch, sz, new(x, p.Y + (sub ? size * 0.55f : 0f)), col);
            x += m.Width + size * 0.04f;
        }
        return x - p.X;
    }
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var parts = Str.Replace("→", "->").Split("->"); string l = parts[0].Trim(), r = parts.Length > 1 ? parts[1].Trim() : "";
        float u = U(c), size = 84f * u, gap = 220f * u, lw = Formula(dc, b, l, default, size, default, false), rw = Formula(dc, b, r, default, size, default, false), x = P0(c).X - (lw + gap + rw) / 2f, y = P0(c).Y - size * 0.6f;
        float p1 = DrawKit.EaseOut(Intro(c, 0.1f, 0.5f)), p2 = DrawKit.EaseOut(Intro(c, 0.6f, 0.5f)), p3 = DrawKit.EaseOut(Intro(c, 1.1f, 0.5f));
        Formula(dc, b, l, new(x - (1f - p1) * 50f * u, y), size, C1(a * p1), true);
        float ay = P0(c).Y; Arrow(dc, b, new(x + lw + 34f * u, ay), new(x + lw + 34f * u + (gap - 68f * u) * MathF.Max(0.01f, p2), ay), C2(a * MathF.Min(1f, p2 * 3f)), 8f * u);
        Formula(dc, b, r, new(x + lw + gap + (1f - p3) * 50f * u, y), size, C1(a * p3), true);
    }
}

public sealed class CellDiagramPlugin : QuickShapePlugin { public override string Name => "演出：細胞の図"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CellDiagramParameter(s); }
public sealed class CellDiagramParameter : Q3Param
{
    public CellDiagramParameter() : this(null) { }
    public CellDiagramParameter(SharedDataStore? s) : base(s) { Count = 1; Color1 = Hex(0x81C784); Color2 = Hex(0x7E57C2); Color3 = Hex(0xFFD54A); }
    static readonly string[] names = ["核", "ミトコンドリア", "細胞膜", "葉緑体", "液胞"];
    static readonly Dictionary<string, ParamGuide> g = G("細胞の図", "Variant=種類|1=動物　2=植物|1|2", "Count=指す部分|1=核 2=ミトコンドリア 3=細胞膜 4=葉緑体 5=液胞(4・5は植物のみ)|1|5", "Color1=細胞の色", "Color2=核の色", "Color3=指す線の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time; var o = P0(c); bool plant = Variant == 2; int pick = Math.Clamp(Count, 1, plant ? 5 : 3);
        if (plant) { RBox(dc, b, o, 900f * u, 640f * u, 50f * u, C1(0.3f * a)); RFrame(dc, b, o, 900f * u, 640f * u, 50f * u, C1(a), 14f * u); }
        else { Oval(dc, b, o, 450f * u, 320f * u, C1(0.3f * a)); Oval(dc, b, o, 450f * u, 320f * u, C1(0f)); b.Color = C1(a); dc.DrawEllipse(new Ellipse(o, 450f * u, 320f * u), b, 10f * u); }
        if (plant) RBox(dc, b, o + new Vector2(190f * u, 30f * u), 330f * u, 360f * u, 90f * u, Rgb(0.45f, 0.75f, 0.95f, 0.45f * a));
        var nuc = o + new Vector2(-150f * u, -40f * u); Dot(dc, b, nuc, 115f * u, C2(0.9f * a)); Dot(dc, b, nuc + new Vector2(24f * u, -18f * u), 34f * u, Rgb(1, 1, 1, 0.3f * a));
        var mito = new[] { o + new Vector2(-300f * u, 150f * u), o + new Vector2(30f * u, 200f * u), o + new Vector2(-40f * u, -220f * u) };
        foreach (var q in mito) { Oval(dc, b, q, 62f * u, 28f * u, Rgb(0.95f, 0.5f, 0.35f, a)); Path(dc, b, new List<Vector2> { q + new Vector2(-44f * u, 0), q + new Vector2(-22f * u, -13f * u), q, q + new Vector2(22f * u, 13f * u), q + new Vector2(44f * u, 0) }, Rgb(1, 0.85f, 0.7f, a), 3f * u); }
        var chl = new[] { o + new Vector2(-340f * u, -200f * u), o + new Vector2(330f * u, -240f * u), o + new Vector2(350f * u, 240f * u) };
        if (plant) foreach (var q in chl) Oval(dc, b, q, 54f * u, 30f * u, Rgb(0.2f, 0.65f, 0.3f, a));
        var target = pick switch { 1 => nuc, 2 => mito[1], 3 => plant ? o + new Vector2(450f * u, 0) : o + new Vector2(450f * u, 0), 4 => chl[1], _ => o + new Vector2(190f * u, 30f * u) };
        float p = DrawKit.EaseOut(Intro(c, 0.3f, 0.5f)), pulse = 0.5f + 0.5f * MathF.Sin(t * 5f); var labelP = o + new Vector2(690f * u, -250f * u);
        Ring(dc, b, target, (34f + pulse * 10f) * u, C3(a * p), 5f * u); Ln(dc, b, target, Vector2.Lerp(target, labelP, p), C3(a), 4f * u);
        Panel(dc, b, labelP, 320f * u, 84f * u, 16f * u, Rgb(0, 0, 0, 0.72f * a * p), C3(a * p), 3f * u); Say(dc, b, names[pick - 1], 36f * u, labelP, Rgb(1, 1, 1, a * p), maxW: 300f * u);
    }
}

public sealed class BodySilhouettePlugin : QuickShapePlugin { public override string Name => "演出：人体のシルエット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BodySilhouetteParameter(s); }
public sealed class BodySilhouetteParameter : Q3Param
{
    public BodySilhouetteParameter() : this(null) { }
    public BodySilhouetteParameter(SharedDataStore? s) : base(s) { Text = "胸:心臓は血液を全身に送ります"; Color1 = Hex(0x455A64); Color2 = Hex(0xFF5252); }
    static readonly Dictionary<string, Vector2> parts = new() { ["頭"] = new(0, -340), ["首"] = new(0, -255), ["肩"] = new(-115, -215), ["胸"] = new(0, -150), ["腕"] = new(-165, -70), ["お腹"] = new(0, -30), ["腹"] = new(0, -30), ["腰"] = new(0, 60), ["手"] = new(-195, 70), ["脚"] = new(-55, 210), ["足"] = new(-60, 390), ["膝"] = new(-55, 220) };
    static readonly Dictionary<string, ParamGuide> g = G("人体のシルエット", "Text=部位と説明|1行に「部位:説明」。部位は 頭 首 肩 胸 腕 お腹 腰 手 脚 膝 足", "Color1=体の色", "Color2=光る色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time; var o = P0(c) - new Vector2(250f * u, 0); var col = C1(a);
        Dot(dc, b, o + new Vector2(0, -345f * u), 72f * u, col); Box(dc, b, o.X - 28f * u, o.Y - 285f * u, o.X + 28f * u, o.Y - 235f * u, col);
        RBox(dc, b, o + new Vector2(0, -85f * u), 230f * u, 310f * u, 60f * u, col); RBox(dc, b, o + new Vector2(0, 65f * u), 190f * u, 90f * u, 34f * u, col);
        foreach (int s in new[] { -1, 1 })
        {
            Path(dc, b, new List<Vector2> { o + new Vector2(s * 112f * u, -205f * u), o + new Vector2(s * 165f * u, -70f * u), o + new Vector2(s * 190f * u, 70f * u) }, col, 56f * u);
            Path(dc, b, new List<Vector2> { o + new Vector2(s * 58f * u, 90f * u), o + new Vector2(s * 58f * u, 230f * u), o + new Vector2(s * 62f * u, 390f * u) }, col, 70f * u);
            RBox(dc, b, o + new Vector2(s * 76f * u, 425f * u), 112f * u, 42f * u, 16f * u, col);
        }
        var rows = Rows.Select(r => r.Split([':', '：'], 2)).Where(x => x.Length == 2).ToList();
        for (int i = 0; i < rows.Count && i < 6; i++)
        {
            if (!parts.TryGetValue(rows[i][0].Trim(), out var rel)) continue;
            float p = DrawKit.EaseOut(Intro(c, 0.3f + i * 0.35f, 0.5f)), pulse = 0.5f + 0.5f * MathF.Sin(t * 4f + i); var q = o + rel * u; var lab = o + new Vector2(600f * u, -300f * u + i * 130f * u);
            Dot(dc, b, q, (42f + pulse * 14f) * u * p, C2(0.3f * a)); Dot(dc, b, q, 16f * u * p, C2(a));
            Path(dc, b, new List<Vector2> { q, Vector2.Lerp(q, new Vector2(lab.X - 300f * u, lab.Y), p), Vector2.Lerp(q, new Vector2(lab.X - 270f * u, lab.Y), p) }, C2(a), 3f * u);
            Panel(dc, b, lab, 540f * u, 96f * u, 14f * u, Rgb(0, 0, 0, 0.72f * a * p), C2(a * p), 3f * u); Say(dc, b, rows[i][1].Trim(), 32f * u, lab, Rgb(1, 1, 1, a * p), maxW: 510f * u);
        }
    }
}

public sealed class JapanTileMapPlugin : QuickShapePlugin { public override string Name => "演出：日本地図（都道府県）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new JapanTileMapParameter(s); }
public sealed class JapanTileMapParameter : Q3Param
{
    public JapanTileMapParameter() : this(null) { }
    public JapanTileMapParameter(SharedDataStore? s) : base(s) { Text = "東京:#FF5252\n大阪:#FFD54A\n北海道:#4FC3F7"; Color1 = Hex(0x455A64); Color2 = Hex(0xFF7043); }
    static readonly (string n, int x, int y)[] tiles = [("北海道",12,0),("青森",12,2),("秋田",11,3),("岩手",12,3),("山形",11,4),("宮城",12,4),("福島",12,5),("新潟",10,5),("群馬",10,6),("栃木",11,6),("茨城",12,6),("長野",9,6),("埼玉",11,7),("千葉",12,7),("東京",11,8),("神奈川",10,8),("山梨",10,7),("富山",9,5),("石川",8,5),("福井",8,6),("岐阜",8,7),("静岡",9,8),("愛知",8,8),("三重",7,8),("滋賀",7,7),("京都",6,6),("大阪",6,7),("奈良",7,9),("和歌山",6,8),("兵庫",5,6),("鳥取",4,6),("岡山",4,7),("島根",3,6),("広島",3,7),("山口",2,7),("香川",5,8),("徳島",5,9),("愛媛",3,9),("高知",4,9),("福岡",1,8),("大分",2,9),("佐賀",0,8),("長崎",0,9),("熊本",1,9),("宮崎",2,10),("鹿児島",1,10),("沖縄",0,12)];
    static readonly Dictionary<string, ParamGuide> g = G("日本地図", "Text=塗る都道府県|1行に「県名」か「県名:#色」。「県」「府」は省けます", "Background=県名を表示", "Color1=ほかの県の色", "Color2=色を書かない時の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var map = new Dictionary<string, Color4>();
        foreach (var row in Rows)
        {
            var f = row.Split([':', '：'], 2); string name = f[0].Trim().TrimEnd('県', '府', '都'); if (name == "北海") name = "北海道";
            var col = C2(a);
            if (f.Length > 1 && f[1].Trim().TrimStart('#').Length == 6 && uint.TryParse(f[1].Trim().TrimStart('#'), System.Globalization.NumberStyles.HexNumber, null, out var hex)) col = Col(Hex(hex), a);
            map[name] = col;
        }
        float u = U(c), s = 74f * u, pulse = 0.82f + 0.18f * MathF.Sin(c.Time * 4f); var o = P0(c) - new Vector2(6.5f * s, 6.5f * s);
        for (int i = 0; i < tiles.Length; i++)
        {
            var q = tiles[i]; float p = DrawKit.EaseOut(Intro(c, 0.05f + i * 0.012f, 0.3f)); var ctr = o + new Vector2((q.x + 0.5f) * s, (q.y + 0.5f) * s); bool hit = map.TryGetValue(q.n, out var col);
            float bw = q.n == "北海道" ? s * 1.9f : s - 4f * u, bh = q.n == "北海道" ? s * 1.9f : s - 4f * u; if (q.n == "北海道") ctr += new Vector2(0, s * 0.45f);
            RBox(dc, b, ctr, bw * p, bh * p, 8f * u, hit ? new Color4(col.R, col.G, col.B, col.A * pulse) : C1(0.85f * a));
            if (Background) Say(dc, b, q.n, (q.n.Length >= 3 ? 19f : 23f) * u, ctr, Rgb(1, 1, 1, a * p), maxW: s - 6f * u);
        }
    }
}

public sealed class WorldClocksPlugin : QuickShapePlugin { public override string Name => "演出：世界の時差（時計が並ぶ）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WorldClocksParameter(s); }
public sealed class WorldClocksParameter : Q3Param
{
    public WorldClocksParameter() : this(null) { }
    public WorldClocksParameter(SharedDataStore? s) : base(s) { Text = "東京:+9\nロンドン:0\nニューヨーク:-5\nシドニー:+10"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFF7043); }
    protected override double DefAmount => 50;
    static readonly Dictionary<string, ParamGuide> g = G("世界の時差", "Text=都市と時差|1行に「都市:世界標準時との差」", "Amount=基準の時刻|0〜100 = 世界標準時の0〜24時", "Speed=時間の進む速さ", "Color1=文字盤の色", "Color2=針の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var rows = Rows.Select(Cells).Where(x => x.Length >= 2).ToList(); if (rows.Count == 0) return;
        float u = U(c), R = MathF.Min(150f * u, 620f * u / rows.Count), gap = R * 2.7f, utc = Amt(c) * 24f + c.Time * Spd(c) / 60f; var o = P0(c);
        for (int i = 0; i < rows.Count; i++)
        {
            var q = o + new Vector2((i - (rows.Count - 1) / 2f) * gap, -30f * u); float p = DrawKit.EaseOutBack(Intro(c, 0.1f + i * 0.1f, 0.5f)), hour = Gfx.Mod(utc + Num(rows[i][1]), 24f);
            Dot(dc, b, q, R * p, hour is >= 6f and < 18f ? C1(a) : Rgb(0.13f, 0.17f, 0.28f, a)); Ring(dc, b, q, R * p, C2(a), 6f * u);
            var ink = hour is >= 6f and < 18f ? Rgb(0.1f, 0.1f, 0.12f, a) : Rgb(1, 1, 1, a);
            for (int k = 0; k < 12; k++) { float th = k * MathF.PI / 6f; Ln(dc, b, Pol(q, R * 0.82f * p, th), Pol(q, R * 0.92f * p, th), ink, 3f * u); }
            float hAng = (hour % 12f) / 12f * MathF.Tau - MathF.PI / 2f, mAng = (hour % 1f) * MathF.Tau - MathF.PI / 2f;
            Ln(dc, b, q, Pol(q, R * 0.5f * p, hAng), ink, 8f * u); Ln(dc, b, q, Pol(q, R * 0.76f * p, mAng), C2(a), 5f * u); Dot(dc, b, q, 8f * u, C2(a));
            Say(dc, b, rows[i][0], 36f * u, q + new Vector2(0, R + 44f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 3f * u, maxW: gap * 0.95f);
            Say(dc, b, $"{(int)hour:00}:{(int)((hour % 1f) * 60f):00}", 34f * u, q + new Vector2(0, R + 92f * u), Col(Color2, a));
        }
    }
}

public sealed class StrataSectionPlugin : QuickShapePlugin { public override string Name => "演出：地層の断面図"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new StrataSectionParameter(s); }
public sealed class StrataSectionParameter : Q3Param
{
    public StrataSectionParameter() : this(null) { }
    public StrataSectionParameter(SharedDataStore? s) : base(s) { Text = "表土\n砂の層\n泥の層\n火山灰\n石灰岩"; Color1 = Hex(0x8D6E63); Color2 = Hex(0xE6C88F); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount => 30;
    static readonly Dictionary<string, ParamGuide> g = G("地層の断面図", "Text=層の名前|上から1行に1つ", "Amount=断層のずれ|0で断層なし", "Background=化石を入れる", "Color1=濃い層の色", "Color2=薄い層の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var rows = Rows; if (rows.Count == 0) return;
        float u = U(c), W = 1300f * u, H = 700f * u, rh = H / rows.Count, shift = Amt(c) * 130f * u * DrawKit.EaseOut(Intro(c, 0.8f, 0.5f)); var o = P0(c); float x0 = o.X - W / 2f, y0 = o.Y - H / 2f;
        Vector2 Top(float k, int i, float dy) => new(x0 + k * W, y0 + i * rh + dy + MathF.Sin(k * 7f + i * 1.3f) * 14f * u);
        for (int i = 0; i < rows.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.05f + (rows.Count - 1 - i) * 0.12f, 0.45f)); var col = Col(Gfx.Mix(Color1, Color2, i % 2 == 0 ? 0.08f + i * 0.05f : 0.72f), a * p);
            foreach (bool right in new[] { false, true })
            {
                float k0 = right ? 0.58f : 0f, k1 = right ? 1f : 0.58f, dy = right ? shift : 0f; var pts = new List<Vector2>();
                for (int s = 0; s <= 24; s++) pts.Add(Top(k0 + (k1 - k0) * s / 24f, i, dy));
                for (int s = 24; s >= 0; s--) pts.Add(Top(k0 + (k1 - k0) * s / 24f, i + 1, dy));
                Poly(dc, b, pts, col);
            }
            Say(dc, b, rows[i], 36f * u, new(x0 + 170f * u, y0 + (i + 0.5f) * rh), C3(a * p), Rgb(0, 0, 0, 0.6f * a), 3f * u);
        }
        if (shift > 0.5f) Ln(dc, b, new(x0 + 0.58f * W, y0 - 20f * u), new(x0 + 0.58f * W, y0 + H + shift), Rgb(0.1f, 0.05f, 0.03f, a), 7f * u);
        if (Background)
        {
            var q = new Vector2(x0 + 0.34f * W, y0 + rh * MathF.Min(rows.Count - 0.5f, 2.5f));
            for (int i = 0; i < 44; i++) { float th = i * 0.42f, r = (6f + i * 1.15f) * u; Dot(dc, b, q + new Vector2(MathF.Cos(th), MathF.Sin(th)) * r, 3.5f * u, Rgb(0.95f, 0.9f, 0.75f, 0.9f * a)); }
        }
    }
}

public sealed class SolarOrbitPlugin : QuickShapePlugin { public override string Name => "演出：太陽系の軌道図"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SolarOrbitParameter(s); }
public sealed class SolarOrbitParameter : Q3Param
{
    public SolarOrbitParameter() : this(null) { }
    public SolarOrbitParameter(SharedDataStore? s) : base(s) { Count = 8; Color1 = Hex(0xFFC107); Color2 = Hex(0xFFFFFF); }
    static readonly (string n, float d, float s, uint col)[] pl = [("水星",0.13f,3.2f,0xB0A89F),("金星",0.22f,2.3f,0xE8C37B),("地球",0.31f,2f,0x4FA8FF),("火星",0.40f,1.6f,0xE0603F),("木星",0.56f,0.9f,0xD9A877),("土星",0.70f,0.65f,0xE7D09A),("天王星",0.84f,0.45f,0x8FE3E8),("海王星",0.97f,0.35f,0x4B70DD)];
    static readonly Dictionary<string, ParamGuide> g = G("太陽系の軌道図", "Count=惑星の数||1|8", "Speed=回る速さ", "Background=名前を表示", "Color1=太陽の色", "Color2=軌道の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), R = 780f * u, sq = 0.56f; var o = P0(c); int n = Math.Clamp(Count, 1, 8);
        Dot(dc, b, o, 84f * u, C1(0.2f * a)); Dot(dc, b, o, 54f * u, C1(a));
        for (int i = 0; i < n; i++)
        {
            var q = pl[i]; float rx = R * q.d, ry = rx * sq, th = t * q.s * 0.7f + i * 1.7f; var ring = new List<Vector2>();
            for (int k = 0; k <= 72; k++) { float e = k / 72f * MathF.Tau; ring.Add(o + new Vector2(MathF.Cos(e) * rx, MathF.Sin(e) * ry)); }
            Path(dc, b, ring, C2(0.25f * a), 2f * u);
            var at = o + new Vector2(MathF.Cos(th) * rx, MathF.Sin(th) * ry); float size = (i is 4 or 5 ? 25f : i is 6 or 7 ? 18f : 12f) * u;
            if (i == 5) { b.Color = Rgb(0.95f, 0.85f, 0.6f, 0.8f * a); dc.DrawEllipse(new Ellipse(at, size * 1.8f, size * 0.55f), b, 4f * u); }
            Dot(dc, b, at, size, Col(Hex(q.col), a));
            if (Background) Say(dc, b, q.n, 24f * u, at + new Vector2(0, size + 20f * u), Rgb(1, 1, 1, 0.9f * a), Rgb(0, 0, 0, 0.6f * a), 2f * u);
        }
    }
}

public sealed class HajikiDiagramPlugin : QuickShapePlugin { public override string Name => "演出：はじきの図（速さ・時間・道のり）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HajikiDiagramParameter(s); }
public sealed class HajikiDiagramParameter : Q3Param
{
    public HajikiDiagramParameter() : this(null) { }
    public HajikiDiagramParameter(SharedDataStore? s) : base(s) { Text = "120km\n40km/時\n3時間"; Color1 = Hex(0xEF5350); Color2 = Hex(0x42A5F5); Color3 = Hex(0x66BB6A); }
    static readonly Dictionary<string, ParamGuide> g = G("はじきの図", "Text=数字|1行目=道のり　2行目=速さ　3行目=時間", "Variant=隠すもの|1=なし　2=道のり　3=速さ　4=時間|1|4", "Color1=道のりの色", "Color2=速さの色", "Color3=時間の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), R = 320f * u * DrawKit.EaseOutBack(Intro(c, 0.1f, 0.5f)); var o = P0(c);
        Pie(dc, b, o, R, MathF.PI, MathF.Tau, C1(a)); Pie(dc, b, o, R, MathF.PI / 2f, MathF.PI, C2(a)); Pie(dc, b, o, R, 0f, MathF.PI / 2f, C3(a));
        Ln(dc, b, o - new Vector2(R, 0), o + new Vector2(R, 0), Rgb(1, 1, 1, a), 7f * u); Ln(dc, b, o, o + new Vector2(0, R), Rgb(1, 1, 1, a), 7f * u); Ring(dc, b, o, R, Rgb(1, 1, 1, a), 7f * u);
        void Cell(string head, int row, int hide, Vector2 p)
        {
            Say(dc, b, head, 56f * u, p - new Vector2(0, 34f * u), Rgb(1, 1, 1, a));
            Say(dc, b, Variant == hide ? "？" : row < r.Count ? r[row] : "", 36f * u, p + new Vector2(0, 34f * u), Rgb(1, 1, 1, a), maxW: 240f * u);
        }
        Cell("道のり", 0, 2, o - new Vector2(0, R * 0.5f)); Cell("速さ", 1, 3, o + new Vector2(-R * 0.48f, R * 0.42f)); Cell("時間", 2, 4, o + new Vector2(R * 0.48f, R * 0.42f));
    }
}

public sealed class WordCardPlugin : QuickShapePlugin { public override string Name => "演出：単語カード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WordCardParameter(s); }
public sealed class WordCardParameter : Q3Param
{
    public WordCardParameter() : this(null) { }
    public WordCardParameter(SharedDataStore? s) : base(s) { Text = "apple\nりんご"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFFF3C4); Color3 = Hex(0x263238); }
    protected override double DefAmount => 30;
    static readonly Dictionary<string, ParamGuide> g = G("単語カード", "Text=表と裏の文字|1行目=表　2行目=裏", "Amount=めくる時刻|0〜100 = 0〜5秒", "Color1=表の色", "Color2=裏の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), k = DrawKit.EaseOut(Sub(c.Time, Amt(c) * 5f, 0.6f)), sx = MathF.Max(0.01f, MathF.Abs(MathF.Cos(k * MathF.PI))); bool back = k > 0.5f; var o = P0(c);
        var prev = dc.Transform; dc.Transform = Matrix3x2.CreateScale(sx, 1f, o) * prev;
        Panel(dc, b, o, 860f * u, 500f * u, 32f * u, back ? C2(a) : C1(a), Rgb(0, 0, 0, 0.3f * a), 4f * u);
        Ring(dc, b, o + new Vector2(-370f * u, -190f * u), 18f * u, Rgb(0.3f, 0.3f, 0.3f, a), 5f * u);
        string s = back ? (r.Count > 1 ? r[1] : "") : (r.Count > 0 ? r[0] : "");
        Say(dc, b, s, 92f * u, o, C3(a), maxW: 780f * u);
        dc.Transform = prev;
    }
}

public sealed class KanbanBoardPlugin : QuickShapePlugin { public override string Name => "演出：カンバン（やることボード）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new KanbanBoardParameter(s); }
public sealed class KanbanBoardParameter : Q3Param
{
    public KanbanBoardParameter() : this(null) { }
    public KanbanBoardParameter(SharedDataStore? s) : base(s) { Text = "やる,進行中,完了\n台本を書く:2\n収録する:1\n編集する:1\nサムネ作成:0"; Color1 = Hex(0x263238); Color2 = Hex(0xFFD54A); Color3 = Hex(0x66BB6A); }
    static readonly Dictionary<string, ParamGuide> g = G("カンバン", "Text=列と付箋|1行目は列名をカンマで。2行目から「付箋:最後の列番号(0から)」", "Speed=移る速さ", "Color1=列の色", "Color2=付箋の色", "Color3=完了の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var rows = Rows; if (rows.Count == 0) return;
        var cols = Cells(rows[0]); int nc = Math.Max(1, cols.Length); float u = U(c), cw = 1500f * u / nc, H = 760f * u, t = c.Time * MathF.Max(0.05f, Spd(c)); var o = P0(c); float x0 = o.X - cw * nc / 2f, y0 = o.Y - H / 2f;
        for (int i = 0; i < nc; i++) { var q = new Vector2(x0 + (i + 0.5f) * cw, o.Y); Panel(dc, b, q, cw - 22f * u, H, 18f * u, C1(0.9f * a), Rgb(1, 1, 1, 0.2f * a), 2f * u); Say(dc, b, cols[i], 40f * u, new(q.X, y0 + 50f * u), Rgb(1, 1, 1, a), maxW: cw - 40f * u); }
        var tasks = rows.Skip(1).Select(x => x.Split([':', '：'])).ToList(); var count = new int[nc];
        for (int i = 0; i < tasks.Count; i++)
        {
            int last = Math.Clamp((int)Num(tasks[i].Length > 1 ? tasks[i][1] : "0"), 0, nc - 1);
            float move = last == 0 ? 0f : DrawKit.EaseOut(Sub(t, 0.7f + i * 0.6f, 0.45f + (last - 1) * 0.25f)) * last;
            int slot = count[last]++; float y = y0 + 150f * u + slot * 120f * u, x = x0 + (move + 0.5f) * cw;
            RBox(dc, b, new(x, y), cw - 70f * u, 96f * u, 10f * u, move >= nc - 1 - 0.01f ? C3(a) : C2(a));
            Say(dc, b, tasks[i][0].Trim(), 32f * u, new(x, y), Rgb(0.1f, 0.1f, 0.12f, a), maxW: cw - 96f * u);
        }
    }
}

public sealed class KpiCardPlugin : QuickShapePlugin { public override string Name => "演出：KPIカード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new KpiCardParameter(s); }
public sealed class KpiCardParameter : Q3Param
{
    public KpiCardParameter() : this(null) { }
    public KpiCardParameter(SharedDataStore? s) : base(s) { Text = "チャンネル登録者\n12,480人\n+12\n30,42,38,55,61,70,86"; Color1 = Hex(0x1E2A38); Color2 = Hex(0x66BB6A); Color3 = Hex(0xEF5350); }
    static readonly Dictionary<string, ParamGuide> g = G("KPIカード", "Text=内容|1行目=見出し　2行目=数字　3行目=前月比(%)　4行目=推移(カンマ区切り)", "Color1=カードの色", "Color2=増えた色", "Color3=減った色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0.05f, 0.45f)), W = 820f * u * p, H = 480f * u * p; var o = P0(c); if (r.Count == 0) return;
        Panel(dc, b, o, W, H, 30f * u, C1(0.96f * a), Rgb(1, 1, 1, 0.15f * a), 3f * u); float x = o.X - 350f * u;
        SayAt(dc, b, r[0], 38f * u, new(x, o.Y - 200f * u), Rgb(0.8f, 0.86f, 0.92f, a), maxW: 700f * u);
        SayAt(dc, b, r.Count > 1 ? r[1] : "", 96f * u, new(x, o.Y - 145f * u), Rgb(1, 1, 1, a), maxW: 700f * u);
        float delta = r.Count > 2 ? Num(r[2]) : 0f; var tone = delta >= 0f ? C2(a) : C3(a); var tip = new Vector2(x + 24f * u, o.Y + (delta >= 0f ? 18f : 62f) * u); float dir = delta >= 0f ? 1f : -1f;
        Poly(dc, b, new List<Vector2> { tip, tip + new Vector2(24f * u, 42f * u * dir), tip + new Vector2(-24f * u, 42f * u * dir) }, tone);
        SayAt(dc, b, $"前月比 {delta:+0.#;-0.#;0}%", 40f * u, new(x + 64f * u, o.Y + 14f * u), tone);
        var v = r.Count > 3 ? Cells(r[3]).Select(q => Num(q)).ToList() : new List<float>();
        if (v.Count >= 2)
        {
            float lo = v.Min(), span = MathF.Max(0.001f, v.Max() - lo); var pts = new List<Vector2>();
            for (int i = 0; i < v.Count; i++) pts.Add(new(x + 700f * u * i / (v.Count - 1), o.Y + 205f * u - (v[i] - lo) / span * 95f * u));
            var part = BubbleKit.PartByLength(pts, DrawKit.EaseOut(Intro(c, 0.35f, 0.9f))); Path(dc, b, part, tone, 6f * u); if (part.Count > 0) Dot(dc, b, part[^1], 10f * u, Rgb(1, 1, 1, a));
        }
    }
}
