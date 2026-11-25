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
//  グラフ
//   棒・横棒・折れ線・円・ドーナツ。値はキーフレームで変化させられる
// =====================================================================
public sealed class ChartPlugin : IShapePlugin
{
    public string Name => "演出：グラフ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ChartParameter(sharedData);
}

public sealed class ChartParameter : EffectShapeParameterBase
{
    public ChartParameter() : this(null) { }
    public ChartParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "グラフ", Name = "種類")]
    [EnumComboBox]
    public ChartKind Kind { get => kind; set => Set(ref kind, value); }
    ChartKind kind = ChartKind.Bar;

    [Display(GroupName = "グラフ", Name = "項目の数")]
    [TextBoxSlider("F0", "個", 2, 6)]
    [Range(2, 6)]
    public int Items { get => items; set => Set(ref items, value); }
    int items = 5;

    [Display(GroupName = "値", Name = "値1")]
    [AnimationSlider("F0", "", 0, 100)]
    public Animation V1 { get; } = new Animation(80, 0, 1000);
    [Display(GroupName = "値", Name = "値2")]
    [AnimationSlider("F0", "", 0, 100)]
    public Animation V2 { get; } = new Animation(55, 0, 1000);
    [Display(GroupName = "値", Name = "値3")]
    [AnimationSlider("F0", "", 0, 100)]
    public Animation V3 { get; } = new Animation(95, 0, 1000);
    [Display(GroupName = "値", Name = "値4")]
    [AnimationSlider("F0", "", 0, 100)]
    public Animation V4 { get; } = new Animation(40, 0, 1000);
    [Display(GroupName = "値", Name = "値5")]
    [AnimationSlider("F0", "", 0, 100)]
    public Animation V5 { get; } = new Animation(70, 0, 1000);
    [Display(GroupName = "値", Name = "値6")]
    [AnimationSlider("F0", "", 0, 100)]
    public Animation V6 { get; } = new Animation(60, 0, 1000);

    [Display(GroupName = "値", Name = "目盛りの最大値")]
    [AnimationSlider("F0", "", 10, 200)]
    public Animation MaxValue { get; } = new Animation(100, 1, 100000);

    [Display(GroupName = "見た目", Name = "カラフルにする")]
    [ToggleSlider]
    public bool Colorful { get => colorful; set => Set(ref colorful, value); }
    bool colorful = true;

    [Display(GroupName = "見た目", Name = "色1（カラフルがオフのとき）")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0x4A, 0x8C, 0xFF);

    [Display(GroupName = "見た目", Name = "色2（カラフルがオフのとき）")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0x4F, 0xE8, 0xC8);

    [Display(GroupName = "見た目", Name = "軸・枠の色")]
    [ColorPicker]
    public WpfColor AxisColor { get => axis; set => Set(ref axis, value); }
    WpfColor axis = WpfColor.FromRgb(0xE8, 0xEE, 0xF4);

    [Display(GroupName = "見た目", Name = "数字を表示する")]
    [ToggleSlider]
    public bool ShowNumbers { get => showNumbers; set => Set(ref showNumbers, value); }
    bool showNumbers = true;

    [Display(GroupName = "見た目", Name = "目盛り線を表示する")]
    [ToggleSlider]
    public bool ShowGrid { get => showGrid; set => Set(ref showGrid, value); }
    bool showGrid = true;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "グラフの幅")]
    [AnimationSlider("F0", "px", 100, 1600)]
    public Animation ChartWidth { get; } = new Animation(800, 20, 8000);

    [Display(GroupName = "位置と大きさ", Name = "グラフの高さ")]
    [AnimationSlider("F0", "px", 100, 1000)]
    public Animation ChartHeight { get; } = new Animation(460, 20, 6000);

    [Display(GroupName = "位置と大きさ", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 12)]
    public Animation Thickness { get; } = new Animation(3, 0.2, 100);

    [Display(GroupName = "動き", Name = "伸びるアニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "伸びるまでの時間")]
    [AnimationSlider("F1", "秒", 0.2, 8)]
    public Animation IntroTime { get; } = new Animation(1.4, 0.05, 60);

    [Display(GroupName = "動き", Name = "伸び具合（手動）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Grow { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [V1, V2, V3, V4, V5, V6, MaxValue, PosX, PosY, ChartWidth, ChartHeight, Thickness, IntroTime, Grow];

    static void Number(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 center, float h, int v, float lw)
    {
        string s = Math.Clamp(v, 0, 999).ToString();
        float dw = h * 0.55f, gap = h * 0.28f, total = s.Length * dw + (s.Length - 1) * gap, x = center.X - total / 2f;
        foreach (char ch in s) { ShapeGeom.Digit(dc, brush, x, center.Y - h / 2f, dw, h, ch - '0', lw); x += dw + gap; }
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float g = c.F(Grow) / 100f;
        if (Intro) g *= Gfx.Smooth(0f, MathF.Max(0.05f, c.F(IntroTime)), t);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float CW = c.F(ChartWidth), CH = c.F(ChartHeight), th = c.F(Thickness), mx = MathF.Max(1f, c.F(MaxValue));
        int n = Math.Clamp(Items, 2, 6);
        var vals = new[] { c.F(V1), c.F(V2), c.F(V3), c.F(V4), c.F(V5), c.F(V6) };
        float x0 = ctr.X - CW / 2f, y0 = ctr.Y - CH / 2f, x1 = ctr.X + CW / 2f, y1 = ctr.Y + CH / 2f;
        var white = WpfColor.FromRgb(255, 255, 255);

        WpfColor ColorOf(int i) => Colorful ? PartyKit.Palette[i % PartyKit.Palette.Length] : Gfx.Mix(Color1, Color2, n == 1 ? 0f : i / (float)(n - 1));
        float Ease(int i) => Gfx.Smooth(i * 0.08f, i * 0.08f + 0.6f, g);           // 項目ごとに少しずつ遅れて伸びる

        using var brush = dc.CreateSolidColorBrush(default);
        float lw = MathF.Max(1.5f, CH * 0.018f);

        switch (Kind)
        {
            case ChartKind.Bar:
            case ChartKind.BarH:
            case ChartKind.Line:
                {
                    bool horiz = Kind == ChartKind.BarH;
                    // 目盛り線と軸
                    if (ShowGrid)
                    {
                        brush.Color = Gfx.C4(AxisColor, 0.18f);
                        for (int k = 1; k <= 4; k++)
                        {
                            if (horiz) dc.DrawLine(new Vector2(x0 + CW * k / 4f, y0), new Vector2(x0 + CW * k / 4f, y1), brush, 1f);
                            else dc.DrawLine(new Vector2(x0, y1 - CH * k / 4f), new Vector2(x1, y1 - CH * k / 4f), brush, 1f);
                        }
                    }
                    brush.Color = Gfx.C4(AxisColor, 0.9f);
                    dc.DrawLine(new Vector2(x0, y0), new Vector2(x0, y1), brush, th);
                    dc.DrawLine(new Vector2(x0, y1), new Vector2(x1, y1), brush, th);

                    var linePts = new List<Vector2>();
                    for (int i = 0; i < n; i++)
                    {
                        float v = Math.Clamp(vals[i] / mx, 0f, 1.2f) * Ease(i);
                        var col = ColorOf(i);
                        if (Kind == ChartKind.Line)
                        {
                            float px = x0 + CW * (i + 0.5f) / n;
                            linePts.Add(new Vector2(px, y1 - CH * v));
                            continue;
                        }
                        float slot = (horiz ? CH : CW) / n, bw = slot * 0.62f;
                        float rx, ry, rw, rh;
                        if (horiz) { rx = x0; ry = y0 + slot * i + (slot - bw) / 2f; rw = CW * v; rh = bw; }
                        else { rw = bw; rx = x0 + slot * i + (slot - bw) / 2f; rh = CH * v; ry = y1 - rh; }
                        brush.Color = Gfx.C4(col, 0.95f);
                        Gfx.Rect(dc, brush, rx, ry, rw, rh);
                        brush.Color = Gfx.C4(white, 0.20f);
                        if (horiz) Gfx.Rect(dc, brush, rx, ry, rw, rh * 0.35f); else Gfx.Rect(dc, brush, rx, ry, rw * 0.35f, rh);
                        if (ShowNumbers && Ease(i) > 0.5f)
                        {
                            brush.Color = Gfx.C4(AxisColor, 0.95f);
                            var np = horiz ? new Vector2(rx + rw + CH * 0.1f, ry + rh / 2f) : new Vector2(rx + rw / 2f, ry - CH * 0.07f);
                            Number(dc, brush, np, CH * 0.075f, (int)MathF.Round(vals[i] * Ease(i)), lw);
                        }
                    }
                    if (Kind == ChartKind.Line && linePts.Count > 1)
                    {
                        // 折れ線の下をうっすら塗る
                        var area = new List<Vector2>(linePts) { new(linePts[^1].X, y1), new(linePts[0].X, y1) };
                        brush.Color = Gfx.C4(ColorOf(0), 0.18f);
                        Gfx.FillPoly(dc, brush, area);
                        brush.Color = Gfx.C4(ColorOf(0), 0.95f);
                        Gfx.Polyline(dc, brush, linePts, th * 1.8f);
                        for (int i = 0; i < linePts.Count; i++)
                        {
                            brush.Color = Gfx.C4(ColorOf(i), 1f);
                            dc.FillEllipse(new Ellipse(linePts[i], th * 2.4f, th * 2.4f), brush);
                            if (ShowNumbers && Ease(i) > 0.5f)
                            {
                                brush.Color = Gfx.C4(AxisColor, 0.95f);
                                Number(dc, brush, linePts[i] - new Vector2(0f, CH * 0.09f), CH * 0.07f, (int)MathF.Round(vals[i] * Ease(i)), lw);
                            }
                        }
                    }
                    break;
                }
            default:    // 円・ドーナツ
                {
                    float R = MathF.Min(CW, CH) / 2f;
                    float total = 0f;
                    for (int i = 0; i < n; i++) total += MathF.Max(0f, vals[i]);
                    if (total <= 0f) break;
                    float a0 = -MathF.PI / 2f;
                    float sweepMax = MathF.PI * 2f * Gfx.Smooth(0f, 1f, g);                   // 全体が時計回りに描き上がる
                    float used = 0f;
                    for (int i = 0; i < n; i++)
                    {
                        float share = MathF.Max(0f, vals[i]) / total;
                        float s0 = used, s1 = used + share * MathF.PI * 2f;
                        used = s1;
                        float e0 = MathF.Min(s0, sweepMax), e1 = MathF.Min(s1, sweepMax);
                        if (e1 - e0 < 0.001f) continue;
                        var col = ColorOf(i);
                        if (Kind == ChartKind.Pie)
                        {
                            var poly = new List<Vector2> { ctr };
                            poly.AddRange(Gfx.ArcPts(ctr, R, a0 + e0, a0 + e1, Math.Max(3, (int)(40 * (e1 - e0) / MathF.PI))));
                            brush.Color = Gfx.C4(col, 0.95f);
                            Gfx.FillPoly(dc, brush, poly);
                            brush.Color = Gfx.C4(AxisColor, 0.9f);
                            dc.DrawLine(ctr, ctr + new Vector2(MathF.Cos(a0 + e0), MathF.Sin(a0 + e0)) * R, brush, th);
                        }
                        else
                        {
                            float tw = R * 0.34f;
                            brush.Color = Gfx.C4(col, 0.95f);
                            FormKit.Stroke(dc, brush, Gfx.ArcPts(ctr, R - tw / 2f, a0 + e0, a0 + e1, Math.Max(3, (int)(40 * (e1 - e0) / MathF.PI))), tw, false);
                        }
                        if (ShowNumbers && e1 >= s1 - 0.001f)
                        {
                            float am = a0 + (s0 + s1) / 2f;
                            float rr = Kind == ChartKind.Pie ? R * 0.62f : R * 0.83f;
                            brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.95f);
                            Number(dc, brush, ctr + new Vector2(MathF.Cos(am), MathF.Sin(am)) * rr, R * 0.13f, (int)MathF.Round(share * 100f), lw * 0.8f);
                        }
                    }
                    brush.Color = Gfx.C4(AxisColor, 0.9f);
                    dc.DrawEllipse(new Ellipse(ctr, R, R), brush, th * 0.8f);
                    if (Kind == ChartKind.Donut) dc.DrawEllipse(new Ellipse(ctr, R * 0.66f, R * 0.66f), brush, th * 0.8f);
                    break;
                }
        }
    }
}

public enum UiKind
{
    [Display(Name = "REC表示")] Rec,
    [Display(Name = "LIVE表示")] Live,
    [Display(Name = "再生バー")] PlayBar,
    [Display(Name = "通知バッジ")] Badge,
    [Display(Name = "スマホ画面の枠")] Phone,
    [Display(Name = "ブラウザ画面の枠")] Browser,
}
