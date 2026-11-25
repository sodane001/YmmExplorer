using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
//  線で描く文字（英大文字・数字・記号）。84番以降の図形で共通に使う
// =====================================================================
internal static class StrokeFont
{
    const float CellW = 0.6f;
    static readonly Dictionary<char, Vector2[][]> glyphs = Build();

    static Dictionary<char, Vector2[][]> Build()
    {
        const string O = ".15,0 .45,0 .6,.15 .6,.85 .45,1 .15,1 0,.85 0,.15 .15,0";
        const string P = "0,1 0,0 .5,0 .6,.15 .6,.35 .5,.5 0,.5";
        var src = new Dictionary<char, string>
        {
            ['A'] = "0,1 .3,0 .6,1|.1,.66 .5,.66",
            ['B'] = "0,0 0,1 .45,1 .6,.85 .6,.65 .45,.5 0,.5|0,0 .4,0 .55,.12 .55,.38 .4,.5",
            ['C'] = ".6,.15 .45,0 .15,0 0,.15 0,.85 .15,1 .45,1 .6,.85",
            ['D'] = "0,0 0,1 .4,1 .6,.8 .6,.2 .4,0 0,0",
            ['E'] = ".6,0 0,0 0,1 .6,1|0,.5 .45,.5",
            ['F'] = ".6,0 0,0 0,1|0,.5 .45,.5",
            ['G'] = ".6,.15 .45,0 .15,0 0,.15 0,.85 .15,1 .45,1 .6,.85 .6,.55 .35,.55",
            ['H'] = "0,0 0,1|.6,0 .6,1|0,.5 .6,.5",
            ['I'] = ".3,0 .3,1|.1,0 .5,0|.1,1 .5,1",
            ['J'] = ".6,0 .6,.85 .45,1 .15,1 0,.85",
            ['K'] = "0,0 0,1|.6,0 0,.6|.2,.42 .6,1",
            ['L'] = "0,0 0,1 .6,1",
            ['M'] = "0,1 0,0 .3,.55 .6,0 .6,1",
            ['N'] = "0,1 0,0 .6,1 .6,0",
            ['O'] = O,
            ['P'] = P,
            ['Q'] = O + "|.35,.75 .62,1.02",
            ['R'] = P + "|.3,.5 .6,1",
            ['S'] = ".6,.15 .45,0 .15,0 0,.15 0,.35 .15,.5 .45,.5 .6,.65 .6,.85 .45,1 .15,1 0,.85",
            ['T'] = "0,0 .6,0|.3,0 .3,1",
            ['U'] = "0,0 0,.85 .15,1 .45,1 .6,.85 .6,0",
            ['V'] = "0,0 .3,1 .6,0",
            ['W'] = "0,0 .15,1 .3,.4 .45,1 .6,0",
            ['X'] = "0,0 .6,1|.6,0 0,1",
            ['Y'] = "0,0 .3,.5 .6,0|.3,.5 .3,1",
            ['Z'] = "0,0 .6,0 0,1 .6,1",
            ['0'] = O + "|.48,.2 .12,.8",
            ['1'] = ".12,.2 .32,0 .32,1|.1,1 .52,1",
            ['2'] = "0,.15 .15,0 .45,0 .6,.15 .6,.38 0,1 .6,1",
            ['3'] = "0,.1 .15,0 .45,0 .6,.15 .6,.35 .45,.5 .2,.5|.45,.5 .6,.65 .6,.85 .45,1 .15,1 0,.9",
            ['4'] = ".45,1 .45,0 0,.7 .6,.7",
            ['5'] = ".6,0 0,0 0,.45 .45,.45 .6,.6 .6,.85 .45,1 .15,1 0,.9",
            ['6'] = ".55,.05 .4,0 .15,0 0,.15 0,.85 .15,1 .45,1 .6,.85 .6,.65 .45,.5 .15,.5 0,.62",
            ['7'] = "0,0 .6,0 .2,1",
            ['8'] = ".15,0 .45,0 .6,.12 .6,.35 .45,.5 .15,.5 0,.65 0,.88 .15,1 .45,1 .6,.88 .6,.65 .45,.5|.15,.5 0,.35 0,.12 .15,0",
            ['9'] = ".6,.4 .45,.5 .15,.5 0,.35 0,.15 .15,0 .45,0 .6,.15 .6,.85 .45,1 .2,1 .05,.95",
            ['+'] = ".05,.5 .55,.5|.3,.25 .3,.75",
            ['-'] = ".08,.5 .52,.5",
            ['!'] = ".3,0 .3,.68|.3,.9 .3,1",
            ['.'] = ".3,.93 .3,1",
            [':'] = ".3,.25 .3,.33|.3,.7 .3,.78",
            ['/'] = ".55,0 .05,1",
            ['%'] = ".55,0 .05,1|.08,.05 .18,.05 .18,.2 .08,.2 .08,.05|.42,.8 .52,.8 .52,.95 .42,.95 .42,.8",
            ['×'] = ".1,.35 .5,.85|.5,.35 .1,.85",
            ['#'] = ".2,.1 .15,.9|.45,.1 .4,.9|0,.35 .6,.35|0,.65 .6,.65",
        };
        var d = new Dictionary<char, Vector2[][]>();
        foreach (var kv in src)
            d[kv.Key] = kv.Value.Split('|')
                .Select(st => st.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(Pt).ToArray())
                .ToArray();
        return d;
    }

    static Vector2 Pt(string s)
    {
        var a = s.Split(',');
        return new Vector2(float.Parse(a[0], CultureInfo.InvariantCulture), float.Parse(a[1], CultureInfo.InvariantCulture));
    }

    /// <summary>文字列の幅（高さ h のとき）</summary>
    public static float Width(string text, float h, float spacing = 0.22f)
    {
        if (string.IsNullOrEmpty(text)) return 0f;
        float w = h * CellW, gap = h * spacing, sum = 0f;
        foreach (char ch in text) sum += (ch == ' ' ? w * 0.8f : w) + gap;
        return sum - gap;
    }

    /// <summary>左上を基準に描く。slant は斜体のかたむき（0〜0.3くらい）。戻り値は描いた幅</summary>
    public static float Draw(ID2D1DeviceContext dc, ID2D1Brush brush, string text, Vector2 topLeft, float h, float lw,
        float spacing = 0.22f, float slant = 0f)
    {
        float x = topLeft.X, w = h * CellW, gap = h * spacing;
        var pts = new List<Vector2>(16);
        foreach (char raw in text)
        {
            char ch = char.ToUpperInvariant(raw);
            if (ch == ' ') { x += w * 0.8f + gap; continue; }
            if (glyphs.TryGetValue(ch, out var strokes))
                foreach (var st in strokes)
                {
                    pts.Clear();
                    foreach (var p in st)
                        pts.Add(new Vector2(x + p.X / CellW * w + (1f - p.Y) * slant * h, topLeft.Y + p.Y * h));
                    if (pts.Count == 2 && pts[0] == pts[1]) dc.FillEllipse(new Ellipse(pts[0], lw * 0.6f, lw * 0.6f), brush);
                    else Gfx.Polyline(dc, brush, pts, lw);
                }
            x += w + gap;
        }
        return x - gap - topLeft.X;
    }

    /// <summary>中心を基準に描く</summary>
    public static void DrawCentered(ID2D1DeviceContext dc, ID2D1Brush brush, string text, Vector2 center, float h, float lw,
        float spacing = 0.22f, float slant = 0f)
        => Draw(dc, brush, text, new Vector2(center.X - Width(text, h, spacing) / 2f, center.Y - h / 2f), h, lw, spacing, slant);

    /// <summary>ふち取り付きで中心を基準に描く</summary>
    public static void DrawOutlined(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, string text, Vector2 center, float h, float lw,
        Color4 fill, Color4 outline, float outlineWidth, float spacing = 0.22f, float slant = 0f)
    {
        if (outline.A > 0.001f && outlineWidth > lw)
        {
            brush.Color = outline;
            DrawCentered(dc, brush, text, center, h, outlineWidth, spacing, slant);
        }
        brush.Color = fill;
        DrawCentered(dc, brush, text, center, h, lw, spacing, slant);
    }
}

/// <summary>84番以降の図形で使う描画の小道具</summary>
internal static class DrawKit
{
    /// <summary>多角形を長方形で切り取る（Sutherland–Hodgman）</summary>
    public static List<Vector2> ClipRect(IList<Vector2> poly, float x0, float y0, float x1, float y1)
    {
        var cur = new List<Vector2>(poly);
        for (int edge = 0; edge < 4 && cur.Count > 0; edge++)
        {
            var input = cur;
            cur = new List<Vector2>(input.Count + 4);
            for (int i = 0; i < input.Count; i++)
            {
                var a = input[i];
                var b = input[(i + 1) % input.Count];
                bool ina = Inside(a, edge, x0, y0, x1, y1), inb = Inside(b, edge, x0, y0, x1, y1);
                if (ina && inb) cur.Add(b);
                else if (ina && !inb) cur.Add(Cross(a, b, edge, x0, y0, x1, y1));
                else if (!ina && inb) { cur.Add(Cross(a, b, edge, x0, y0, x1, y1)); cur.Add(b); }
            }
        }
        return cur;
    }

    static bool Inside(Vector2 p, int edge, float x0, float y0, float x1, float y1) => edge switch
    {
        0 => p.X >= x0,
        1 => p.X <= x1,
        2 => p.Y >= y0,
        _ => p.Y <= y1,
    };

    static Vector2 Cross(Vector2 a, Vector2 b, int edge, float x0, float y0, float x1, float y1)
    {
        float t = edge switch
        {
            0 => (x0 - a.X) / (b.X - a.X),
            1 => (x1 - a.X) / (b.X - a.X),
            2 => (y0 - a.Y) / (b.Y - a.Y),
            _ => (y1 - a.Y) / (b.Y - a.Y),
        };
        return Vector2.Lerp(a, b, float.IsFinite(t) ? t : 0f);
    }

    /// <summary>穴あきの多角形を塗る（外側から内側の穴を抜く）</summary>
    public static void FillWithHole(ID2D1DeviceContext dc, ID2D1Brush brush, IList<Vector2> outer, IList<Vector2> hole)
    {
        if (outer.Count < 3) return;
        if (hole.Count >= 3 && GeoKit.FillFigures(dc, brush, new[] { AsRo(outer), AsRo(hole) })) return;
        var pts = new List<Vector2>(outer.Count + hole.Count + 3);
        pts.AddRange(outer);
        pts.Add(outer[0]);
        if (hole.Count >= 3)
        {
            for (int i = hole.Count - 1; i >= 0; i--) pts.Add(hole[i]);
            pts.Add(hole[hole.Count - 1]);
        }
        Gfx.FillPoly(dc, brush, pts);
    }

    static IReadOnlyList<Vector2> AsRo(IList<Vector2> l) => l as IReadOnlyList<Vector2> ?? l.ToList();

    /// <summary>長方形の点列（左上・幅・高さ）</summary>
    public static List<Vector2> RectPts(float x, float y, float w, float h)
        => new() { new(x, y), new(x + w, y), new(x + w, y + h), new(x, y + h) };

    /// <summary>点を中心のまわりに回す（ラジアン）</summary>
    public static Vector2 Rot(Vector2 p, Vector2 c, float a)
    {
        float cs = MathF.Cos(a), sn = MathF.Sin(a);
        var d = p - c;
        return c + new Vector2(d.X * cs - d.Y * sn, d.X * sn + d.Y * cs);
    }

    /// <summary>点列を平行移動・回転する</summary>
    public static List<Vector2> Place(IEnumerable<Vector2> local, Vector2 pos, float rot, float scaleX = 1f, float scaleY = 1f)
    {
        float cs = MathF.Cos(rot), sn = MathF.Sin(rot);
        var res = new List<Vector2>();
        foreach (var p in local)
        {
            float x = p.X * scaleX, y = p.Y * scaleY;
            res.Add(pos + new Vector2(x * cs - y * sn, x * sn + y * cs));
        }
        return res;
    }

    /// <summary>楕円の点列</summary>
    public static List<Vector2> EllipsePts(Vector2 c, float rx, float ry, int n = 48, float a0 = 0f, float a1 = MathF.PI * 2f)
    {
        var pts = new List<Vector2>(n + 1);
        for (int i = 0; i < n; i++)
        {
            float a = a0 + (a1 - a0) * i / n;
            pts.Add(c + new Vector2(MathF.Cos(a) * rx, MathF.Sin(a) * ry));
        }
        return pts;
    }

    /// <summary>やわらかい光の玉（加算向け）</summary>
    public static void Glow(ID2D1DeviceContext dc, Vector2 c, float r, Color4 col)
    {
        if (r <= 0.5f || col.A <= 0.002f) return;
        if (GeoKit.Glow(dc, c, r, col)) return;
        using var g = Gfx.Radial(dc, c, r, (0f, col), (0.35f, new Color4(col.R, col.G, col.B, col.A * 0.45f)), (1f, new Color4(col.R, col.G, col.B, 0f)));
        dc.FillEllipse(new Ellipse(c, r, r), g);
    }

    /// <summary>4方向に光が伸びるきらめき（加算向け）</summary>
    public static void Twinkle(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 c, float len, float width, WpfColor color, float alpha, float rot = 0f)
    {
        if (alpha <= 0.002f || len <= 0.5f) return;
        for (int i = 0; i < 4; i++)
        {
            float a = rot + i * MathF.PI / 2f;
            var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
            Gfx.FadeLine(dc, brush, c, c + d * len, width, 0.3f, color, alpha, 8, 1.2f);
        }
        brush.Color = Gfx.C4(color, alpha);
        dc.FillEllipse(new Ellipse(c, width * 0.8f, width * 0.8f), brush);
    }

    /// <summary>行き過ぎて戻るイージング（0→1）</summary>
    public static float EaseOutBack(float x)
    {
        x = Math.Clamp(x, 0f, 1f);
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * MathF.Pow(x - 1f, 3f) + c1 * MathF.Pow(x - 1f, 2f);
    }

    public static float EaseOut(float x)
    {
        x = Math.Clamp(x, 0f, 1f);
        return 1f - (1f - x) * (1f - x) * (1f - x);
    }

    public static float Frac(float x) => x - MathF.Floor(x);

    public static float H(int seed, int i, int k = 0) => Gfx.Hash01(seed * 7919 + i * 131 + k * 1543 + 17);

    /// <summary>アイテムの終わりまでの残り秒数（出ていく動きに使う）</summary>
    public static float TimeLeft(in DrawContext c) => (c.Length - c.Frame) / (float)Math.Max(1, c.Fps);

    /// <summary>登場（0→1）と退場（1→0）をかけた表示の度合い</summary>
    public static float InOut(in DrawContext c, float inTime, float outTime)
    {
        float a = inTime <= 0.001f ? 1f : Math.Clamp(c.Time / inTime, 0f, 1f);
        float b = outTime <= 0.001f ? 1f : Math.Clamp(TimeLeft(c) / outTime, 0f, 1f);
        return MathF.Min(a, b);
    }
}

// =====================================================================
//  ダメージ・ヒット表示
//   数字が弾んで飛び、ヒット火花と衝撃の輪がつく
// =====================================================================
public sealed class DamageHitPlugin : IShapePlugin
{
    public string Name => "演出：ダメージ・ヒット表示";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DamageHitParameter(sharedData);
}

public sealed class DamageHitParameter : EffectShapeParameterBase
{
    public DamageHitParameter() : this(null) { }
    public DamageHitParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "数字の色")]
    [ColorPicker]
    public WpfColor NumberColor { get => numberColor; set => Set(ref numberColor, value); }
    WpfColor numberColor = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);

    [Display(GroupName = "色", Name = "会心（クリティカル）の色")]
    [ColorPicker]
    public WpfColor CritColor { get => critColor; set => Set(ref critColor, value); }
    WpfColor critColor = WpfColor.FromRgb(0xFF, 0xD2, 0x3C);

    [Display(GroupName = "色", Name = "火花の色")]
    [ColorPicker]
    public WpfColor SparkColor { get => sparkColor; set => Set(ref sparkColor, value); }
    WpfColor sparkColor = WpfColor.FromRgb(0xFF, 0x96, 0x30);

    [Display(GroupName = "色", Name = "数字のふちの色")]
    [ColorPicker]
    public WpfColor OutlineColor { get => outlineColor; set => Set(ref outlineColor, value); }
    WpfColor outlineColor = WpfColor.FromRgb(0x24, 0x10, 0x10);

    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "ヒット位置のばらつき")]
    [AnimationSlider("F0", "px", 0, 500)]
    public Animation Spread { get; } = new Animation(120, 0, 5000);

    [Display(GroupName = "数字", Name = "ダメージの値")]
    [AnimationSlider("F0", "", 0, 99999)]
    public Animation Value { get; } = new Animation(1234, 0, 99999999);

    [Display(GroupName = "数字", Name = "ヒットごとに値を少し変える")]
    [ToggleSlider]
    public bool Vary { get => vary; set => Set(ref vary, value); }
    bool vary = true;

    [Display(GroupName = "数字", Name = "数字の高さ")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation DigitHeight { get; } = new Animation(72, 8, 2000);

    [Display(GroupName = "数字", Name = "会心の出る確率")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CritChance { get; } = new Animation(25, 0, 100);

    [Display(GroupName = "ヒット", Name = "ヒットの間隔")]
    [AnimationSlider("F2", "秒", 0.1, 3)]
    public Animation Interval { get; } = new Animation(0.7, 0.03, 600);

    [Display(GroupName = "ヒット", Name = "数字が出ている時間")]
    [AnimationSlider("F2", "秒", 0.3, 3)]
    public Animation Life { get; } = new Animation(1.2, 0.1, 60);

    [Display(GroupName = "ヒット", Name = "火花の大きさ")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation SparkSize { get; } = new Animation(140, 0, 3000);

    [Display(GroupName = "ヒット", Name = "衝撃の輪を表示する")]
    [ToggleSlider]
    public bool Ring { get => ring; set => Set(ref ring, value); }
    bool ring = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Spread, Value, DigitHeight, CritChance, Interval, Life, SparkSize];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float interval = MathF.Max(0.03f, c.F(Interval)), life = MathF.Max(0.1f, c.F(Life));
        float spread = c.F(Spread), dh0 = c.F(DigitHeight), spark = c.F(SparkSize), crit = c.F(CritChance);
        double value = c.F(Value);
        int last = (int)MathF.Floor(t / interval);
        int first = Math.Max(0, last - (int)MathF.Ceiling(life / interval) - 1);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            for (int k = first; k <= last; k++)
            {
                float age = t - k * interval;
                if (age < 0f || age > life) continue;
                var p = ctr + new Vector2((DrawKit.H(Seed, k, 1) - 0.5f) * 2f * spread, (DrawKit.H(Seed, k, 2) - 0.5f) * 1.2f * spread);
                bool isCrit = DrawKit.H(Seed, k, 3) * 100f < crit;
                var hitCol = isCrit ? CritColor : SparkColor;

                // ---- 火花と閃光 ----
                float sb = age / 0.28f;
                if (sb < 1f && spark > 1f)
                {
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    DrawKit.Glow(dc, p, spark * (0.45f + 0.35f * sb) * (isCrit ? 1.3f : 1f), Gfx.C4(hitCol, 0.85f * (1f - sb)));
                    int rays = isCrit ? 14 : 9;
                    for (int r = 0; r < rays; r++)
                    {
                        float a = DrawKit.H(Seed, k * 37 + r, 4) * MathF.PI * 2f;
                        float len = spark * (0.5f + 0.6f * DrawKit.H(Seed, k * 37 + r, 5)) * (isCrit ? 1.35f : 1f);
                        var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                        var a0 = p + d * len * (0.1f + 0.5f * sb);
                        var a1 = p + d * len * (0.35f + 0.75f * DrawKit.EaseOut(sb));
                        Gfx.FadeLine(dc, brush, a1, a0, MathF.Max(1f, spark * 0.05f * (1f - sb)), 0.5f, Gfx.Mix(hitCol, WpfColor.FromRgb(255, 255, 255), 0.5f), 1f - sb, 6, 1f);
                    }
                    dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                }

                // ---- 衝撃の輪 ----
                if (Ring && age < 0.45f && spark > 1f)
                {
                    float rr = age / 0.45f;
                    float rad = spark * (0.2f + 0.95f * DrawKit.EaseOut(rr));
                    brush.Color = Gfx.C4(hitCol, 0.85f * (1f - rr));
                    dc.DrawEllipse(new Ellipse(p, rad, rad * 0.8f), brush, 2f + spark * 0.05f * (1f - rr));
                }

                // ---- 数字 ----
                double v = value * (Vary ? 0.85 + 0.3 * DrawKit.H(Seed, k, 6) : 1.0) * (isCrit ? 2.0 : 1.0);
                string text = Math.Max(0, (long)Math.Round(v)).ToString(CultureInfo.InvariantCulture) + (isCrit ? "!" : "");
                float s = age < 0.1f ? 0.3f + 1.05f * (age / 0.1f)
                                     : 1f + 0.35f * MathF.Exp(-(age - 0.1f) * 9f) * MathF.Cos((age - 0.1f) * 24f);
                float u = age / life;
                float alpha = 1f - Gfx.Smooth(0.7f, 1f, u);
                float dh = dh0 * (isCrit ? 1.4f : 1f) * s;
                float side = (DrawKit.H(Seed, k, 7) - 0.5f) * dh0 * 0.8f;
                var np = p + new Vector2(side * u, -dh0 * 0.7f - dh0 * 1.0f * DrawKit.EaseOut(u));
                float lw = MathF.Max(1.5f, dh * 0.13f);
                StrokeFont.DrawOutlined(dc, brush, text, np, dh, lw,
                    Gfx.C4(isCrit ? CritColor : NumberColor, alpha), Gfx.C4(OutlineColor, alpha * 0.92f), lw * 2.6f, 0.12f, 0.12f);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
