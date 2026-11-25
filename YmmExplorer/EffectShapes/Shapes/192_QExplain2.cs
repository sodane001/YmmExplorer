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
//  レーダーチャート
//   1行に「項目:値(0〜100)」。5〜8角形の能力値が、中心から広がる。2つめの系列は「項目:値:値」
// =====================================================================
public sealed class RadarChartPlugin : QuickShapePlugin
{
    public override string Name => "演出：レーダーチャート";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RadarChartParameter(sharedData);
}

public sealed class RadarChartParameter : QuickShapeParameter
{
    public RadarChartParameter() : this(null) { }
    public RadarChartParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "攻撃:80:55\n防御:60:75\n速さ:90:50\n魔力:40:85\n運:70:60";
        Color1 = Hex(0xFF5C8A);
        Color2 = Hex(0x4FC3F7);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "項目と値", Tip: "1行に「項目:値」。2つめの系列は「項目:値:値」(0〜100)", Group: "「レーダーチャート」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "1つめの色"),
        ["Color2"] = new ParamGuide(Label: "2つめの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var rows = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        int n = rows.Count;
        if (n < 3) return;
        var names = new string[n]; var v1 = new float[n]; var v2 = new float[n]; bool two = false;
        for (int i = 0; i < n; i++)
        {
            var f = rows[i].Split(new[] { ':', '：' });
            names[i] = f[0];
            v1[i] = f.Length > 1 && float.TryParse(f[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var a1) ? Math.Clamp(a1 / 100f, 0f, 1.2f) : 0f;
            if (f.Length > 2 && float.TryParse(f[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var a2)) { v2[i] = Math.Clamp(a2 / 100f, 0f, 1.2f); two = true; }
        }
        float R = 300f * u;
        float p = DrawKit.EaseOutBack(Intro(c, 0.2f, 0.8f));
        float Ang(int i) => -MathF.PI / 2f + i * MathF.PI * 2f / n;
        for (int k = 1; k <= 4; k++)
        {
            var ring = new List<Vector2>();
            for (int i = 0; i < n; i++) ring.Add(Pol(o, R * k / 4f, Ang(i)));
            Outline(dc, b, ring, Rgb(1, 1, 1, 0.2f * a), 2f * u);
        }
        for (int i = 0; i < n; i++)
        {
            Ln(dc, b, o, Pol(o, R, Ang(i)), Rgb(1, 1, 1, 0.2f * a), 2f * u);
            Say(dc, b, names[i], 36f * u, Pol(o, R + 60f * u, Ang(i)), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.5f * a), 3f * u);
        }
        for (int sIdx = two ? 1 : 0; sIdx >= 0; sIdx--)
        {
            var vals = sIdx == 0 ? v1 : v2;
            var col = sIdx == 0 ? Color1 : Color2;
            var poly = new List<Vector2>();
            for (int i = 0; i < n; i++) poly.Add(Pol(o, R * vals[i] * p, Ang(i)));
            Poly(dc, b, poly, Col(col, 0.35f * a));
            Outline(dc, b, poly, Col(col, a), 5f * u);
            foreach (var q in poly) Dot(dc, b, q, 8f * u, Col(col, a));
        }
    }
}

// =====================================================================
//  数直線と範囲
//   目盛りの上を点が動く。「値」が点、「値2」までの範囲に色がつく。「文字」に「最小,最大」
// =====================================================================
public sealed class NumberLinePlugin : QuickShapePlugin
{
    public override string Name => "演出：数直線と範囲";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NumberLineParameter(sharedData);
}

public sealed class NumberLineParameter : QuickShapeParameter
{
    public NumberLineParameter() : this(null) { }
    public NumberLineParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "0,10";
        Color1 = Hex(0xFF7043);
        Color2 = Hex(0x4FC3F7);
    }
    protected override double DefAmount => 30;
    protected override double DefAmount2 => 70;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "目盛りの端", Tip: "カンマ区切りで「最小,最大」", Group: "「数直線と範囲」の設定"),
        ["Amount"] = new ParamGuide(Label: "点の位置", Tip: "0=左はし 100=右はし", Group: "「数直線と範囲」の設定"),
        ["Amount2"] = new ParamGuide(Label: "範囲の終わり", Tip: "点からここまでに色がつきます", Group: "「数直線と範囲」の設定"),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "点の色"),
        ["Color2"] = new ParamGuide(Label: "範囲の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        float mn = it.Count > 0 && float.TryParse(it[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var m0) ? m0 : 0f;
        float mx = it.Count > 1 && float.TryParse(it[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var m1) ? m1 : 10f;
        float L = 1400f * u, x0 = o.X - L / 2f;
        Ln(dc, b, new Vector2(x0 - 40f * u, o.Y), new Vector2(x0 + L + 40f * u, o.Y), Rgb(1, 1, 1, a), 6f * u);
        Poly(dc, b, Pts(new Vector2(x0 + L + 60f * u, o.Y), u, u, 0f, 0f, -30f, -16f, -30f, 16f), Rgb(1, 1, 1, a));
        int ticks = (int)Math.Clamp(MathF.Round(mx - mn), 2, 20);
        for (int i = 0; i <= ticks; i++)
        {
            float x = x0 + L * i / ticks;
            Ln(dc, b, new Vector2(x, o.Y - 18f * u), new Vector2(x, o.Y + 18f * u), Rgb(1, 1, 1, a), 4f * u);
            Say(dc, b, (mn + (mx - mn) * i / ticks).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture), 32f * u, new Vector2(x, o.Y + 56f * u), Rgb(1, 1, 1, a));
        }
        float p = DrawKit.EaseOut(Intro(c, 0.3f, 1f));
        float a1 = x0 + L * Amt(c), a2 = x0 + L * Amt2(c);
        float e2 = a1 + (a2 - a1) * p;
        Box(dc, b, MathF.Min(a1, e2), o.Y - 12f * u, MathF.Max(a1, e2), o.Y + 12f * u, C2(0.8f * a));
        Dot(dc, b, new Vector2(a1, o.Y), 22f * u, C1(a));
        Ring(dc, b, new Vector2(a1, o.Y), 22f * u, Rgb(1, 1, 1, a), 4f * u);
        Say(dc, b, (mn + (mx - mn) * Amt(c)).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture), 44f * u, new Vector2(a1, o.Y - 70f * u), C1(a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
    }
}

// =====================================================================
//  分数・割合のピザ
//   円が「数」等分され、「値」の割合だけピースに色がつく。分数の文字つき
// =====================================================================
public sealed class FractionPizzaPlugin : QuickShapePlugin
{
    public override string Name => "演出：分数・割合のピザ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FractionPizzaParameter(sharedData);
}

public sealed class FractionPizzaParameter : QuickShapeParameter
{
    public FractionPizzaParameter() : this(null) { }
    public FractionPizzaParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 8;
        Color1 = Hex(0xFFB74D);
        Color2 = Hex(0xE53935);
    }
    protected override double DefAmount => 37.5;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "色をつける割合", Tip: "全体の何%か(8等分で3切れなら37.5)", Group: "「分数・割合のピザ」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "分ける数(分母)", Min: 2, Max: 24, Group: "「分数・割合のピザ」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "生地の色"),
        ["Color2"] = new ParamGuide(Label: "色がつくピースの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Clamp(Count, 2, 24);
        int on = Math.Clamp((int)MathF.Round(Amt(c) * n), 0, n);
        float R = 300f * u;
        Dot(dc, b, o, R + 16f * u, Rgb(0.75f, 0.45f, 0.2f, a));
        for (int i = 0; i < n; i++)
        {
            float a0 = -MathF.PI / 2f + i * MathF.PI * 2f / n, a1 = a0 + MathF.PI * 2f / n;
            float p = DrawKit.EaseOutBack(Intro(c, 0.3f + i * 0.12f, 0.3f));
            bool lit = i < on && p > 0f;
            var mid = (a0 + a1) / 2f;
            var off = lit ? new Vector2(MathF.Cos(mid), MathF.Sin(mid)) * 16f * u * p : Vector2.Zero;
            var slice = new List<Vector2> { o + off };
            slice.AddRange(Gfx.ArcPts(o + off, R, a0, a1, 16));
            Poly(dc, b, slice, lit ? C2(a) : C1(a));
            Outline(dc, b, slice, Rgb(1, 1, 1, 0.8f * a), 4f * u);
        }
        var fx = o + new Vector2(R + 200f * u, 0);
        Say(dc, b, on.ToString(), 100f * u, fx - new Vector2(0, 70f * u), Rgb(1, 1, 1, a));
        Ln(dc, b, fx - new Vector2(70f * u, 0), fx + new Vector2(70f * u, 0), Rgb(1, 1, 1, a), 8f * u);
        Say(dc, b, n.ToString(), 100f * u, fx + new Vector2(0, 70f * u), Rgb(1, 1, 1, a));
    }
}

// =====================================================================
//  座標グラフに関数を描く
//   座標軸の上に、直線や放物線がペンで描くように伸びる。バリエーションで式の形
// =====================================================================
public sealed class FunctionGraphPlugin : QuickShapePlugin
{
    public override string Name => "演出：座標グラフに関数を描く";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FunctionGraphParameter(sharedData);
}

public sealed class FunctionGraphParameter : QuickShapeParameter
{
    public FunctionGraphParameter() : this(null) { }
    public FunctionGraphParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "";
        Color1 = Hex(0xFFD54A);
    }
    protected override double DefAmount => 50;
    protected override double DefAmount2 => 45;

    [Display(GroupName = "「座標グラフに関数を描く」の設定", Name = "関数の式", Description = "y = の右側を書きます(例: 2x+1、x²÷2−3、3sin(2x))。ボタンで入れられます。空の時は「形の種類」の形を描きます")]
    [FormulaPad]
    public string Formula { get => formula; set => Set(ref formula, value ?? ""); }
    string formula = "x^2/2-3";

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Formula"] = new ParamGuide(Label: "関数の式", Tip: "y = の右側。例: 2x+1　x^2/2-3　3sin(2x)　sqrt(x)　1/x　abs(x)。a は「値」÷50", Group: "「座標グラフに関数を描く」の設定"),
        ["Text"] = new ParamGuide(Label: "グラフに出す文字", Tip: "空なら、式をそのまま表示します。「-」1文字なら何も出しません", Group: "「座標グラフに関数を描く」の設定"),
        ["Amount"] = new ParamGuide(Label: "値（式の a・形の開き具合）", Tip: "式の中の a は、この値÷50(50 で 1)。アニメーションでグラフの形を動かせます", Group: "「座標グラフに関数を描く」の設定"),
        ["Amount2"] = new ParamGuide(Label: "マス目の濃さ", Group: "「座標グラフに関数を描く」の設定"),
        ["Count"] = new ParamGuide(Label: "横の目盛りの範囲（±）", Tip: "x を −この数〜+この数 まで描きます", Min: 1, Max: 100, Group: "「座標グラフに関数を描く」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類（式が空の時）", Tip: "1=直線　2=放物線　3=波(サイン)　4=反比例", Min: 1, Max: 4, Group: "「座標グラフに関数を描く」の設定"),
        ["Background"] = new ParamGuide(Label: "マス目を表示", Group: "「座標グラフに関数を描く」の設定"),
        ["Color1"] = new ParamGuide(Label: "グラフの色"),
        ["Color2"] = new ParamGuide(Label: "マス目・目盛りの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Label: "描く速さ"),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c);
        var o = P0(c);
        float W2 = 700f * u, H2 = 420f * u;
        int range = Math.Clamp(Count, 1, 100);
        float S = W2 / range;                      // 1 目盛りの大きさ
        float yMax = H2 / S;                       // 縦に描ける範囲(±)
        var grid = Col(Color2, 1f);
        bool defaultGridColor = Color2 == WpfColor.FromRgb(0xFF, 0x7A, 0x59);
        if (defaultGridColor) grid = Rgb(1, 1, 1, 1);
        float gridA = Math.Clamp(c.F(Amount2) / 100f, 0f, 1f) * 0.6f * a;

        // マス目(1目盛りごと。細かすぎる時は間引く)
        int step = Math.Max(1, (int)MathF.Ceiling(18f * u / MathF.Max(0.001f, S)));
        if (Background && gridA > 0.002f)
        {
            var gc = new Color4(grid.R, grid.G, grid.B, gridA);
            for (int i = -range; i <= range; i += 1)
            {
                if (i % step != 0) continue;
                Ln(dc, b, o + new Vector2(i * S, -H2), o + new Vector2(i * S, H2), gc, (i % (step * 5) == 0 ? 2.5f : 1.5f) * u);
            }
            int ry = (int)MathF.Floor(yMax);
            for (int i = -ry; i <= ry; i++)
            {
                if (i % step != 0) continue;
                Ln(dc, b, o + new Vector2(-W2, i * S), o + new Vector2(W2, i * S), gc, (i % (step * 5) == 0 ? 2.5f : 1.5f) * u);
            }
        }
        // 軸と目盛りの数
        var axis = Rgb(1, 1, 1, a);
        Ln(dc, b, o - new Vector2(W2, 0), o + new Vector2(W2, 0), axis, 4f * u);
        Ln(dc, b, o + new Vector2(0, H2), o - new Vector2(0, H2), axis, 4f * u);
        Say(dc, b, "x", 34f * u, o + new Vector2(W2 + 30f * u, 0), axis);
        Say(dc, b, "y", 34f * u, o - new Vector2(0, H2 + 30f * u), axis);
        Say(dc, b, "O", 24f * u, o + new Vector2(-16f * u, 18f * u), Rgb(1, 1, 1, 0.8f * a), bold: false);
        int label = step * (range > 10 ? 5 : 1);
        if (range <= 10) label = Math.Max(1, step);
        for (int i = -range; i <= range; i++)
        {
            if (i == 0 || i % label != 0) continue;
            Ln(dc, b, o + new Vector2(i * S, -7f * u), o + new Vector2(i * S, 7f * u), axis, 3f * u);
            Say(dc, b, i.ToString(), 20f * u, o + new Vector2(i * S, 24f * u), Rgb(1, 1, 1, 0.75f * a), bold: false);
        }
        for (int i = -(int)yMax; i <= (int)yMax; i++)
        {
            if (i == 0 || i % label != 0) continue;
            Ln(dc, b, o + new Vector2(-7f * u, -i * S), o + new Vector2(7f * u, -i * S), axis, 3f * u);
            Say(dc, b, i.ToString(), 20f * u, o + new Vector2(-24f * u, -i * S), Rgb(1, 1, 1, 0.75f * a), bold: false);
        }

        // グラフ(式があれば式で、なければ「形の種類」で)
        float av = c.F(Amount) / 50f;
        Func<double, double, double>? f = null;
        string err = "";
        if (!string.IsNullOrWhiteSpace(Formula)) f = FormulaKit.Cached(Formula, out err);
        if (f == null)
        {
            float k = 0.2f + Amt(c) * 2f;
            int v = Variant;
            f = v switch
            {
                2 => (x, _) => k * 0.3 * x * x - 3,
                3 => (x, _) => 3 * Math.Sin(x * k * 0.8),
                4 => (x, _) => k * 2 / x,
                _ => (x, _) => k * x * 0.6,
            };
        }
        var segments = new List<List<Vector2>>();
        var cur = new List<Vector2>();
        const int N = 900;
        double lastY = double.NaN;
        float limit = yMax + 1f;
        for (int i = 0; i <= N; i++)
        {
            double x = -range + 2.0 * range * i / N, y;
            try { y = f(x, av); } catch { y = double.NaN; }
            bool ok = double.IsFinite(y) && Math.Abs(y) <= limit;
            // 急に飛ぶ所(1/x の 0 の前後、tan など)は、線をつながない
            bool jump = ok && double.IsFinite(lastY) && Math.Abs(y - lastY) > yMax * 1.2;
            if (!ok || jump)
            {
                if (cur.Count > 1) segments.Add(cur);
                cur = new List<Vector2>();
                if (!ok) { lastY = double.NaN; continue; }
            }
            cur.Add(o + new Vector2((float)(x * S), (float)(-Math.Clamp(y, -limit, limit) * S)));
            lastY = y;
        }
        if (cur.Count > 1) segments.Add(cur);

        // ペンで描くように、左から順に伸ばす(全体の長さで割合を決める)
        float spd = MathF.Max(0.05f, Spd(c));
        float p = DrawKit.EaseOut(Intro(c, 0.3f, 1.5f / spd));
        float total = 0f;
        foreach (var seg in segments) total += BubbleKit.Length(seg);
        float want = total * p, done = 0f;
        // マス目の外にはみ出さないように切る
        var clip = new RawRectF(o.X - W2, o.Y - H2, o.X + W2, o.Y + H2);
        dc.PushAxisAlignedClip(clip, AntialiasMode.PerPrimitive);
        Vector2? tip = null;
        foreach (var seg in segments)
        {
            float len = BubbleKit.Length(seg);
            if (done >= want) break;
            var part = done + len <= want ? seg : BubbleKit.PartByLength(seg, (want - done) / MathF.Max(0.001f, len));
            Path(dc, b, part, C1(a), 6f * u);
            if (part.Count > 0) tip = part[^1];
            done += len;
        }
        dc.PopAxisAlignedClip();
        if (tip.HasValue && p < 1f) Dot(dc, b, tip.Value, 10f * u, Rgb(1, 1, 1, a));

        // 式の表示
        string label2 = Str.Trim();
        if (label2.Length == 0) label2 = !string.IsNullOrWhiteSpace(Formula) && err.Length == 0 ? FormulaKit.Pretty(Formula) : "";
        if (label2 == "-") label2 = "";
        if (label2.Length > 0)
            Say(dc, b, label2, 52f * u, o + new Vector2(W2 * 0.55f, -H2 * 0.82f), C1(a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
        if (err.Length > 0)
            Say(dc, b, "式が読めません: " + err, 28f * u, o + new Vector2(0, H2 + 70f * u), Rgb(1, 0.55f, 0.5f, a), Rgb(0, 0, 0, 0.6f * a), 3f * u);
    }
}

// =====================================================================
//  因果の矢印チェーン
//   「原因 → 結果 → 結果」の箱が、矢印でつながりながら順に出る。1行に1つ
// =====================================================================
public sealed class CauseChainPlugin : QuickShapePlugin
{
    public override string Name => "演出：因果の矢印チェーン";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CauseChainParameter(sharedData);
}

public sealed class CauseChainParameter : QuickShapeParameter
{
    public CauseChainParameter() : this(null) { }
    public CauseChainParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "雨が降る\n地面がぬれる\n滑りやすくなる";
        Color1 = Hex(0x5C6BC0);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "箱の言葉", Tip: "1行に1つ(左から順)", Group: "「因果の矢印チェーン」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=横に並ぶ　2=縦に並ぶ", Min: 1, Max: 2, Group: "「因果の矢印チェーン」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "箱の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "矢印の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var items = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        int n = items.Count;
        if (n == 0) return;
        bool vert = Variant >= 2;
        float bw = vert ? 520f * u : MathF.Min(360f * u, 1500f * u / n - 80f * u), bh = vert ? MathF.Min(140f * u, 800f * u / n - 50f * u) : 170f * u;
        float gap = 90f * u;
        float total = vert ? n * bh + (n - 1) * gap : n * bw + (n - 1) * gap;
        for (int i = 0; i < n; i++)
        {
            float p = DrawKit.EaseOutBack(Intro(c, 0.2f + i * 0.5f, 0.35f));
            if (p <= 0f) continue;
            var q = vert ? new Vector2(o.X, o.Y - total / 2f + bh / 2f + i * (bh + gap)) : new Vector2(o.X - total / 2f + bw / 2f + i * (bw + gap), o.Y);
            RBox(dc, b, q, bw * p, bh * p, 20f * u, i == 0 ? C1(a) : Col(Gfx.Mix(Color1, Hex(0x26A69A), i / (float)Math.Max(1, n - 1)), a));
            Say(dc, b, items[i], 40f * u, q, Rgb(1, 1, 1, a), maxW: bw * 0.85f, scale: p);
            if (i < n - 1)
            {
                float ap = Intro(c, 0.45f + i * 0.5f, 0.25f);
                if (ap <= 0f) continue;
                var s0 = vert ? q + new Vector2(0, bh / 2f + 10f * u) : q + new Vector2(bw / 2f + 10f * u, 0);
                var dir = vert ? new Vector2(0, 1) : new Vector2(1, 0);
                var e0 = s0 + dir * (gap - 20f * u) * ap;
                Ln(dc, b, s0, e0, C3(a), 10f * u);
                Poly(dc, b, BubbleKit.ArrowHead(e0 + dir * 10f * u, dir, 34f * u), C3(a));
            }
        }
    }
}

// =====================================================================
//  メリット・デメリット表
//   左に○(メリット)、右に×(デメリット)が順に並んで出る。1行に「+言葉」か「-言葉」
// =====================================================================
public sealed class ProsConsPlugin : QuickShapePlugin
{
    public override string Name => "演出：メリット・デメリット表";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ProsConsParameter(sharedData);
}

public sealed class ProsConsParameter : QuickShapeParameter
{
    public ProsConsParameter() : this(null) { }
    public ProsConsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "+安い\n+軽い\n-こわれやすい\n+速い\n-音が大きい";
        Color1 = Hex(0x43A047);
        Color2 = Hex(0xE53935);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "項目", Tip: "「+」で始まる行がメリット、「-」で始まる行がデメリット", Group: "「メリット・デメリット表」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "メリットの色"),
        ["Color2"] = new ParamGuide(Label: "デメリットの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var rows = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 1).ToList();
        var pros = rows.Where(x => x[0] == '+' || x[0] == '＋').Select(x => x[1..].Trim()).ToList();
        var cons = rows.Where(x => x[0] == '-' || x[0] == 'ー' || x[0] == '－').Select(x => x[1..].Trim()).ToList();
        float cw = 620f * u, rh = 90f * u;
        int maxN = Math.Max(pros.Count, cons.Count);
        float top = o.Y - (maxN * rh + 120f * u) / 2f;
        for (int side = 0; side < 2; side++)
        {
            var list = side == 0 ? pros : cons;
            var col = side == 0 ? Color1 : Color2;
            float cx = o.X + (side == 0 ? -1f : 1f) * (cw / 2f + 20f * u);
            RBox(dc, b, new Vector2(cx, top + 45f * u), cw, 90f * u, 16f * u, Col(col, a));
            Say(dc, b, side == 0 ? "メリット" : "デメリット", 46f * u, new Vector2(cx, top + 45f * u), Rgb(1, 1, 1, a));
            for (int i = 0; i < list.Count; i++)
            {
                float p = DrawKit.EaseOut(Intro(c, 0.3f + (i * 2 + side) * 0.25f, 0.3f));
                if (p <= 0f) continue;
                float y = top + 120f * u + i * rh + rh / 2f;
                RBox(dc, b, new Vector2(cx, y), cw * p, rh - 12f * u, 12f * u, Col(col, 0.18f * a));
                var mk = new Vector2(cx - cw / 2f + 50f * u, y);
                if (side == 0) Ring(dc, b, mk, 24f * u, Col(col, a), 7f * u);
                else { Ln(dc, b, mk + new Vector2(-20f, -20f) * u, mk + new Vector2(20f, 20f) * u, Col(col, a), 7f * u); Ln(dc, b, mk + new Vector2(-20f, 20f) * u, mk + new Vector2(20f, -20f) * u, Col(col, a), 7f * u); }
                SayAt(dc, b, list[i], 38f * u, new Vector2(mk.X + 50f * u, y - 26f * u), Rgb(1, 1, 1, a * p), true, cw - 130f * u);
            }
        }
    }
}

// =====================================================================
//  ステップの階段
//   段を登っていくキャラの印と、各段の文字。「値」の段まで登る
// =====================================================================
public sealed class StepStairsPlugin : QuickShapePlugin
{
    public override string Name => "演出：ステップの階段";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StepStairsParameter(sharedData);
}

public sealed class StepStairsParameter : QuickShapeParameter
{
    public StepStairsParameter() : this(null) { }
    public StepStairsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "はじめる\n慣れる\n工夫する\n教える";
        Color1 = Hex(0x26A69A);
        Color3 = Hex(0xFFD54A);
    }
    protected override double DefAmount => 100;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "段の言葉", Tip: "1行に1段(下から順)", Group: "「ステップの階段」の設定"),
        ["Amount"] = new ParamGuide(Label: "どこまで登るか", Tip: "100=いちばん上", Group: "「ステップの階段」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "段の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "キャラ印の色"),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var items = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        int n = items.Count;
        if (n == 0) return;
        float sw = 1400f * u / n, sh = 600f * u / n;
        float x0 = o.X - sw * n / 2f, yb = o.Y + 300f * u;
        for (int i = 0; i < n; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.1f + i * 0.15f, 0.35f));
            float top = yb - (i + 1) * sh * p;
            Box(dc, b, x0 + i * sw, top, x0 + (i + 1) * sw - 4f * u, yb, Col(Gfx.Mix(Color1, Hex(0x004D40), i / (float)Math.Max(1, n)), a));
            Box(dc, b, x0 + i * sw, top, x0 + (i + 1) * sw - 4f * u, top + 10f * u, Rgb(1, 1, 1, 0.25f * a));
            Say(dc, b, items[i], 36f * u, new Vector2(x0 + (i + 0.5f) * sw, top + 50f * u), Rgb(1, 1, 1, a), maxW: sw * 0.9f);
        }
        float target = Amt(c) * (n - 1);
        float climb = Math.Clamp((c.Time - 0.8f) * 1.2f * Spd(c), 0f, target);
        int st = (int)MathF.Floor(climb); float fr = climb - st;
        float hop = MathF.Sin(fr * MathF.PI) * 50f * u;
        var pos = new Vector2(x0 + (st + 0.5f + fr) * sw, yb - (st + 1 + fr) * sh - 60f * u - hop);
        if (st >= n - 1) pos = new Vector2(x0 + (n - 0.5f) * sw, yb - n * sh - 60f * u);
        Dot(dc, b, pos - new Vector2(0, 40f * u), 26f * u, C3(a));
        Poly(dc, b, Pts(pos, u, u, -30f, -10f, 30f, -10f, 22f, 50f, -22f, 50f), C3(a));
        if (climb >= target && target >= n - 1.01f) Star(dc, b, pos + new Vector2(60f, -90f) * u, 30f * u * (1f + 0.15f * MathF.Sin(t * 6f)), 5, 0.45f, 0f, C3(a));
    }
}

// =====================================================================
//  ジグソーパズル
//   ピースが飛んできてはまり、最後に1枚の絵(文字)が完成する。「数」で横のピース数
// =====================================================================
public sealed class JigsawPuzzlePlugin : QuickShapePlugin
{
    public override string Name => "演出：ジグソーパズル";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new JigsawPuzzleParameter(sharedData);
}

public sealed class JigsawPuzzleParameter : QuickShapeParameter
{
    public JigsawPuzzleParameter() : this(null) { }
    public JigsawPuzzleParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 4;
        Text = "完成！";
        Color1 = Hex(0x42A5F5);
        Color2 = Hex(0x7E57C2);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "完成した時の文字", Group: "「ジグソーパズル」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "横のピース数", Min: 2, Max: 8, Group: "「ジグソーパズル」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ピースの色1"),
        ["Color2"] = new ParamGuide(Label: "ピースの色2"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int cols = Math.Clamp(Count, 2, 8), rows = Math.Max(2, (int)MathF.Round(cols * 0.6f));
        float pw = 900f * u / cols, ph = 560f * u / rows;
        float x0 = o.X - pw * cols / 2f, y0 = o.Y - ph * rows / 2f;
        for (int r = 0; r < rows; r++)
            for (int k = 0; k < cols; k++)
            {
                int idx = r * cols + k;
                float p = DrawKit.EaseOut(Intro(c, 0.1f + idx * 0.08f, 0.5f));
                if (p <= 0f) continue;
                var home = new Vector2(x0 + (k + 0.5f) * pw, y0 + (r + 0.5f) * ph);
                var from = home + new Vector2((DrawKit.H(Seed, idx) - 0.5f) * 1600f * u, (DrawKit.H(Seed, idx, 1) - 0.5f) * 1000f * u);
                var q = Vector2.Lerp(from, home, p);
                float rot = (1f - p) * (DrawKit.H(Seed, idx, 2) - 0.5f) * 3f;
                var shape = new List<Vector2>();
                float hw = pw / 2f, hh = ph / 2f, tab = MathF.Min(pw, ph) * 0.16f;
                void Edge(Vector2 e0, Vector2 e1, int bump)
                {
                    var d = e1 - e0;
                    var along = Vector2.Normalize(d);
                    var nrm = new Vector2(d.Y, -d.X) / d.Length();
                    shape.Add(e0);
                    if (bump == 0) return;
                    var m = (e0 + e1) / 2f;
                    foreach (var ap in Gfx.ArcPts(Vector2.Zero, tab, MathF.PI, 0f, 10))
                        shape.Add(m + along * ap.X + nrm * bump * (MathF.Abs(ap.Y) + tab * 0.15f));
                }
                int right = k < cols - 1 ? ((r + k) % 2 == 0 ? 1 : -1) : 0, bottom = r < rows - 1 ? ((r + k) % 2 == 0 ? -1 : 1) : 0;
                int left = k > 0 ? -(((r + k - 1) % 2 == 0) ? 1 : -1) : 0, topB = r > 0 ? -(((r - 1 + k) % 2 == 0) ? -1 : 1) : 0;
                Edge(new Vector2(-hw, -hh), new Vector2(hw, -hh), topB);
                Edge(new Vector2(hw, -hh), new Vector2(hw, hh), right);
                Edge(new Vector2(hw, hh), new Vector2(-hw, hh), bottom);
                Edge(new Vector2(-hw, hh), new Vector2(-hw, -hh), left);
                var placed = DrawKit.Place(shape, q, rot);
                Poly(dc, b, placed, Col(Gfx.Mix(Color1, Color2, (k + r) / (float)(cols + rows - 2)), a));
                Outline(dc, b, placed, Rgb(1, 1, 1, 0.7f * a), 3f * u);
            }
        float done = Intro(c, 0.6f + cols * rows * 0.08f, 0.4f);
        if (done > 0f) Say(dc, b, Str, 120f * u, o, Rgb(1, 1, 1, a * done), Rgb(0, 0, 0, 0.5f * a * done), 6f * u, scale: DrawKit.EaseOutBack(done));
    }
}

// =====================================================================
//  人口ピラミッド
//   左右に伸びる横棒グラフ。1行に「名前:左の値:右の値」
// =====================================================================
public sealed class PopulationPyramidPlugin : QuickShapePlugin
{
    public override string Name => "演出：人口ピラミッド";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PopulationPyramidParameter(sharedData);
}

public sealed class PopulationPyramidParameter : QuickShapeParameter
{
    public PopulationPyramidParameter() : this(null) { }
    public PopulationPyramidParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "80代:3:5\n60代:8:9\n40代:12:12\n20代:10:9\n0代:7:7";
        Color1 = Hex(0x42A5F5);
        Color2 = Hex(0xEF5350);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "データ", Tip: "1行に「名前:左の値:右の値」(上から順)", Group: "「人口ピラミッド」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "左の色"),
        ["Color2"] = new ParamGuide(Label: "右の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var rows = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        int n = rows.Count;
        if (n == 0) return;
        var nm = new string[n]; var L = new float[n]; var R = new float[n];
        for (int i = 0; i < n; i++)
        {
            var f = rows[i].Split(new[] { ':', '：' });
            nm[i] = f[0];
            L[i] = f.Length > 1 && float.TryParse(f[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var l0) ? l0 : 0f;
            R[i] = f.Length > 2 && float.TryParse(f[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var r0) ? r0 : 0f;
        }
        float mx = MathF.Max(1e-3f, MathF.Max(L.Max(), R.Max()));
        float half = 600f * u, rh = MathF.Min(90f * u, 700f * u / n), lab = 70f * u;
        float y0 = o.Y - n * rh / 2f;
        float p = DrawKit.EaseOut(Intro(c, 0.2f, 1f));
        for (int i = 0; i < n; i++)
        {
            float y = y0 + i * rh + rh / 2f, bh = rh * 0.75f;
            Box(dc, b, o.X - lab - half * L[i] / mx * p, y - bh / 2f, o.X - lab, y + bh / 2f, C1(a));
            Box(dc, b, o.X + lab, y - bh / 2f, o.X + lab + half * R[i] / mx * p, y + bh / 2f, C2(a));
            Say(dc, b, nm[i], MathF.Min(30f * u, rh * 0.5f), new Vector2(o.X, y), Rgb(1, 1, 1, a));
        }
    }
}

// =====================================================================
//  単位の換算メーター
//   左右2つのメーターの針が連動して回る。「文字」に「左の単位,右の単位,左の最大,倍率」
// =====================================================================
public sealed class UnitConverterPlugin : QuickShapePlugin
{
    public override string Name => "演出：単位の換算メーター";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new UnitConverterParameter(sharedData);
}

public sealed class UnitConverterParameter : QuickShapeParameter
{
    public UnitConverterParameter() : this(null) { }
    public UnitConverterParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "km,マイル,100,0.6214";
        Color1 = Hex(0x29B6F6);
        Color2 = Hex(0xFFA726);
    }
    protected override double DefAmount => 60;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "単位と倍率", Tip: "カンマ区切りで「左の単位,右の単位,左の最大,倍率」", Group: "「単位の換算メーター」の設定"),
        ["Amount"] = new ParamGuide(Label: "針の位置", Tip: "左のメーターの最大に対する割合", Group: "「単位の換算メーター」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "左の色"),
        ["Color2"] = new ParamGuide(Label: "右の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        string u1 = it.Count > 0 ? it[0] : "", u2 = it.Count > 1 ? it[1] : "";
        float max1 = it.Count > 2 && float.TryParse(it[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var m0) ? m0 : 100f;
        float rate = it.Count > 3 && float.TryParse(it[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var r0) ? r0 : 1f;
        float v = Amt(c) * DrawKit.EaseOut(Intro(c, 0.2f, 1.2f));
        for (int side = 0; side < 2; side++)
        {
            var q = o + new Vector2((side == 0 ? -1f : 1f) * 360f * u, 0);
            float R = 260f * u, mx = side == 0 ? max1 : max1 * rate;
            var col = side == 0 ? C1(a) : C2(a);
            Dot(dc, b, q, R, Rgb(0.12f, 0.13f, 0.16f, 0.9f * a));
            Path(dc, b, Gfx.ArcPts(q, R * 0.9f, MathF.PI * 0.75f, MathF.PI * 2.25f, 40), Rgb(1, 1, 1, 0.25f * a), 14f * u);
            Path(dc, b, Gfx.ArcPts(q, R * 0.9f, MathF.PI * 0.75f, MathF.PI * 0.75f + MathF.PI * 1.5f * v, 40), col, 14f * u);
            for (int i = 0; i <= 5; i++) Say(dc, b, (mx * i / 5f).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture), 24f * u, Pol(q, R * 0.68f, MathF.PI * 0.75f + MathF.PI * 1.5f * i / 5f), Rgb(1, 1, 1, 0.7f * a), bold: false);
            float na = MathF.PI * 0.75f + MathF.PI * 1.5f * v;
            Ln(dc, b, q, Pol(q, R * 0.8f, na), col, 7f * u);
            Dot(dc, b, q, 16f * u, Rgb(1, 1, 1, a));
            Say(dc, b, (mx * v).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + " " + (side == 0 ? u1 : u2), 46f * u, q + new Vector2(0, R * 0.55f), Rgb(1, 1, 1, a));
        }
        Poly(dc, b, BubbleKit.ArrowHead(o + new Vector2(30f * u, 0), new Vector2(1, 0), 40f * u), Rgb(1, 1, 1, 0.8f * a));
        Poly(dc, b, BubbleKit.ArrowHead(o - new Vector2(30f * u, 0), new Vector2(-1, 0), 40f * u), Rgb(1, 1, 1, 0.8f * a));
    }
}

// =====================================================================
//  辞書・用語解説カード
//   見出し語・読み・説明の3段のカード。1行目が見出し語、2行目が読み、3行目からが説明
// =====================================================================
public sealed class GlossaryCardPlugin : QuickShapePlugin
{
    public override string Name => "演出：辞書・用語解説カード";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GlossaryCardParameter(sharedData);
}

public sealed class GlossaryCardParameter : QuickShapeParameter
{
    public GlossaryCardParameter() : this(null) { }
    public GlossaryCardParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "演出\nえんしゅつ\n映像や舞台で、見る人を楽しませるための工夫や仕掛けのこと。";
        Color1 = Hex(0x3949AB);
        Color2 = Hex(0xFFFDF5);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "内容", Tip: "1行目が見出し語、2行目が読み、3行目からが説明", Group: "「辞書・用語解説カード」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "見出しの帯の色"),
        ["Color2"] = new ParamGuide(Label: "カードの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var ln = Str.Split('\n');
        string word = ln.Length > 0 ? ln[0] : "", yomi = ln.Length > 1 ? ln[1] : "", body = string.Join("\n", ln.Skip(2));
        float cw = 1100f * u, ch = 560f * u;
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.4f));
        var q = o + new Vector2(0, (1f - k) * 60f * u);
        RBox(dc, b, q + new Vector2(10f, 14f) * u, cw, ch, 22f * u, Rgb(0, 0, 0, 0.3f * a * k));
        RBox(dc, b, q, cw, ch, 22f * u, C2(a * k));
        Box(dc, b, q.X - cw / 2f, q.Y - ch / 2f + 20f * u, q.X - cw / 2f + 18f * u, q.Y + ch / 2f - 20f * u, C1(a * k));
        var ink = Rgb(0.12f, 0.12f, 0.16f, a * k);
        SayAt(dc, b, word, 90f * u, new Vector2(q.X - cw / 2f + 70f * u, q.Y - ch / 2f + 40f * u), ink);
        SayAt(dc, b, "【" + yomi + "】", 36f * u, new Vector2(q.X - cw / 2f + 74f * u, q.Y - ch / 2f + 160f * u), Col(Color1, a * k), false);
        Ln(dc, b, new Vector2(q.X - cw / 2f + 70f * u, q.Y - ch / 2f + 225f * u), new Vector2(q.X + cw / 2f - 60f * u, q.Y - ch / 2f + 225f * u), Rgb(0, 0, 0, 0.2f * a * k), 3f * u);
        float p = Intro(c, 0.5f, 0.4f);
        SayAt(dc, b, body, 40f * u, new Vector2(q.X - cw / 2f + 70f * u + (1f - p) * 30f * u, q.Y - ch / 2f + 255f * u), Rgb(0.2f, 0.2f, 0.25f, a * k * p), false, cw - 140f * u);
    }
}

// =====================================================================
//  よくある質問（Q&A）
//   Qの吹き出しが出て、少し遅れてAが開く。1行目が質問、2行目からが答え
// =====================================================================
public sealed class FaqQAPlugin : QuickShapePlugin
{
    public override string Name => "演出：よくある質問（Q&A）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FaqQAParameter(sharedData);
}

public sealed class FaqQAParameter : QuickShapeParameter
{
    public FaqQAParameter() : this(null) { }
    public FaqQAParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "どうやって使うの？\nアイテムに図形を追加して、文字を書きかえるだけです。";
        Color1 = Hex(0x1E88E5);
        Color2 = Hex(0xE53935);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "質問と答え", Tip: "1行目が質問、2行目からが答え", Group: "「よくある質問（Q&A）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "Qの色"),
        ["Color2"] = new ParamGuide(Label: "Aの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var ln = Str.Split('\n');
        string qs = ln.Length > 0 ? ln[0] : "", ans = string.Join("\n", ln.Skip(1));
        float bw = 1300f * u;
        for (int k = 0; k < 2; k++)
        {
            float p = DrawKit.EaseOutBack(Intro(c, k == 0 ? 0f : 0.9f, 0.35f));
            if (p <= 0f) continue;
            string s = k == 0 ? qs : ans;
            var ts = Measure(s, 44f * u, k == 0, bw - 200f * u);
            float bh = ts.Height + 70f * u;
            var q = o + new Vector2(0, k == 0 ? -bh / 2f - 30f * u : 60f * u + bh / 2f - 40f * u);
            if (k == 0) q.Y = o.Y - 40f * u - bh / 2f; else q.Y = o.Y + 20f * u + bh / 2f;
            var col = k == 0 ? C1(a) : C2(a);
            RBox(dc, b, q, bw * p, bh, 24f * u, k == 0 ? Rgb(1, 1, 1, 0.95f * a) : Rgb(1, 0.97f, 0.97f, 0.95f * a));
            RFrame(dc, b, q, bw * p, bh, 24f * u, col, 5f * u);
            var badge = new Vector2(q.X - bw / 2f + 60f * u, q.Y);
            Dot(dc, b, badge, 42f * u * p, col);
            Say(dc, b, k == 0 ? "Q" : "A", 50f * u, badge, Rgb(1, 1, 1, a), scale: p);
            TextKit.Draw(dc, b, ts, new Vector2(q.X - bw / 2f + 130f * u, q.Y - ts.Height / 2f), Rgb(0.12f, 0.12f, 0.16f, a * MathF.Min(1f, p)));
        }
    }
}
