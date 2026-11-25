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

public enum GaugeKind
{
    [Display(Name = "横バー")] BarH,
    [Display(Name = "縦バー")] BarV,
    [Display(Name = "HPバー（斜め）")] HpBar,
    [Display(Name = "円形ゲージ")] Circle,
    [Display(Name = "半円メーター（針）")] Meter,
}

// =====================================================================
//  ゲージ・メーター
//   「値」にキーフレームを打つと、HPバーや進捗、スピードメーターとして使える
// =====================================================================
public sealed class GaugePlugin : IShapePlugin
{
    public string Name => "演出：ゲージ・メーター";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GaugeParameter(sharedData);
}

public sealed class GaugeParameter : EffectShapeParameterBase
{
    public GaugeParameter() : this(null) { }
    public GaugeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "ゲージ", Name = "種類")]
    [EnumComboBox]
    public GaugeKind Kind { get => kind; set => Set(ref kind, value); }
    GaugeKind kind = GaugeKind.HpBar;

    [Display(GroupName = "ゲージ", Name = "値", Description = "キーフレームで変化させる")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Value { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "ゲージ", Name = "値で色を変える", Description = "満タンは緑、半分で黄、少ないと赤")]
    [ToggleSlider]
    public bool ColorByValue { get => byValue; set => Set(ref byValue, value); }
    bool byValue = true;

    [Display(GroupName = "ゲージ", Name = "数字を表示する")]
    [ToggleSlider]
    public bool ShowNumber { get => showNumber; set => Set(ref showNumber, value); }
    bool showNumber = true;

    [Display(GroupName = "ゲージ", Name = "減ったときの残像を出す")]
    [ToggleSlider]
    public bool DamageTrail { get => trail; set => Set(ref trail, value); }
    bool trail = true;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "ゲージの幅")]
    [AnimationSlider("F0", "px", 50, 1600)]
    public Animation GaugeWidth { get; } = new Animation(640, 10, 8000);

    [Display(GroupName = "位置と大きさ", Name = "ゲージの高さ")]
    [AnimationSlider("F0", "px", 10, 600)]
    public Animation GaugeHeight { get; } = new Animation(56, 3, 4000);

    [Display(GroupName = "位置と大きさ", Name = "斜めの角度（HPバー）")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation Slant { get; } = new Animation(28, 0, 1000);

    [Display(GroupName = "位置と大きさ", Name = "目盛りの数（バー・メーター）")]
    [AnimationSlider("F0", "個", 0, 40)]
    public Animation Segments { get; } = new Animation(0, 0, 200);

    [Display(GroupName = "色", Name = "ゲージの色（値で色を変えないとき）")]
    [ColorPicker]
    public WpfColor FillColor { get => fill; set => Set(ref fill, value); }
    WpfColor fill = WpfColor.FromRgb(0x39, 0xE8, 0xFF);

    [Display(GroupName = "色", Name = "背景の色")]
    [ColorPicker]
    public WpfColor TrackColor { get => track; set => Set(ref track, value); }
    WpfColor track = WpfColor.FromRgb(0x14, 0x1C, 0x28);

    [Display(GroupName = "色", Name = "枠の色")]
    [ColorPicker]
    public WpfColor FrameColor { get => frame; set => Set(ref frame, value); }
    WpfColor frame = WpfColor.FromRgb(0xE8, 0xEE, 0xF4);

    [Display(GroupName = "色", Name = "枠・線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 12)]
    public Animation Thickness { get; } = new Animation(3, 0.2, 100);

    [Display(GroupName = "色", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Value, PosX, PosY, GaugeWidth, GaugeHeight, Slant, Segments, Thickness, Opacity];

    // ---- 7セグ風の数字 ----
    static readonly int[] SegMask = { 0b0111111, 0b0000110, 0b1011011, 0b1001111, 0b1100110, 0b1101101, 0b1111101, 0b0000111, 0b1111111, 0b1101111 };

    static void Digit(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, float x, float y, float w, float h, int d, float lw)
    {
        int m = SegMask[Math.Clamp(d, 0, 9)];
        float g = lw * 0.9f, hm = y + h / 2f;
        // a=上 b=右上 c=右下 d=下 e=左下 f=左上 g=中
        if ((m & 1) != 0) dc.DrawLine(new Vector2(x + g, y), new Vector2(x + w - g, y), brush, lw);
        if ((m & 2) != 0) dc.DrawLine(new Vector2(x + w, y + g), new Vector2(x + w, hm - g), brush, lw);
        if ((m & 4) != 0) dc.DrawLine(new Vector2(x + w, hm + g), new Vector2(x + w, y + h - g), brush, lw);
        if ((m & 8) != 0) dc.DrawLine(new Vector2(x + g, y + h), new Vector2(x + w - g, y + h), brush, lw);
        if ((m & 16) != 0) dc.DrawLine(new Vector2(x, hm + g), new Vector2(x, y + h - g), brush, lw);
        if ((m & 32) != 0) dc.DrawLine(new Vector2(x, y + g), new Vector2(x, hm - g), brush, lw);
        if ((m & 64) != 0) dc.DrawLine(new Vector2(x + g, hm), new Vector2(x + w - g, hm), brush, lw);
    }

    static void Number(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 center, float h, int value, float lw)
    {
        string s = Math.Clamp(value, 0, 100).ToString();
        float dw = h * 0.55f, gap = h * 0.28f;
        float total = s.Length * dw + (s.Length - 1) * gap;
        float x = center.X - total / 2f;
        foreach (char ch in s)
        {
            Digit(dc, brush, x, center.Y - h / 2f, dw, h, ch - '0', lw);
            x += dw + gap;
        }
    }

    WpfColor ValueColor(float v)
    {
        if (!ColorByValue) return FillColor;
        var green = WpfColor.FromRgb(0x3C, 0xE0, 0x6A);
        var yellow = WpfColor.FromRgb(0xFF, 0xD2, 0x3C);
        var red = WpfColor.FromRgb(0xFF, 0x3C, 0x3C);
        return v > 0.5f ? Gfx.Mix(yellow, green, (v - 0.5f) * 2f) : Gfx.Mix(red, yellow, v * 2f);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float GW = c.F(GaugeWidth), GH = c.F(GaugeHeight), th = c.F(Thickness), op = c.F(Opacity) / 100f;
        int segs = (int)c.F(Segments);
        float v = Math.Clamp(c.F(Value) / 100f, 0f, 1f);

        // 減ったときの残像：少し前の値より下がっていれば、その差を残して見せる
        float lagFrames = 0.5f * c.Fps;
        float vPrev = Math.Clamp((float)Value.GetValue(c.Frame - (long)lagFrames, c.Length, c.Fps) / 100f, 0f, 1f);
        float vTrail = DamageTrail ? MathF.Max(v, vPrev) : v;

        var fillCol = ValueColor(v);
        var white = WpfColor.FromRgb(255, 255, 255);
        using var brush = dc.CreateSolidColorBrush(default);

        switch (Kind)
        {
            case GaugeKind.BarH:
            case GaugeKind.BarV:
                {
                    bool horiz = Kind == GaugeKind.BarH;
                    float bw = horiz ? GW : GH, bh = horiz ? GH : GW;
                    float x0 = ctr.X - bw / 2f, y0 = ctr.Y - bh / 2f;
                    brush.Color = Gfx.C4(TrackColor, 0.85f * op);
                    Gfx.Rect(dc, brush, x0, y0, bw, bh);

                    RectF Part(float from, float to) => horiz
                        ? new RectF(x0 + bw * from, y0, x0 + bw * to, y0 + bh)
                        : new RectF(x0, y0 + bh * (1f - to), x0 + bw, y0 + bh * (1f - from));

                    if (vTrail > v)
                    {
                        var r = Part(v, vTrail);
                        brush.Color = Gfx.C4(white, 0.8f * op);
                        dc.FillRectangle(new RawRectF(r.L, r.T, r.R, r.B), brush);
                    }
                    var f = Part(0f, v);
                    brush.Color = Gfx.C4(fillCol, op);
                    dc.FillRectangle(new RawRectF(f.L, f.T, f.R, f.B), brush);
                    // 上側のつや
                    brush.Color = Gfx.C4(white, 0.22f * op);
                    if (horiz) Gfx.Rect(dc, brush, f.L, f.T, f.R - f.L, bh * 0.35f);
                    else Gfx.Rect(dc, brush, f.L, f.T, bw * 0.35f, f.B - f.T);

                    // 目盛り（区切り線）
                    brush.Color = Gfx.C4(TrackColor, op);
                    for (int i = 1; i < segs; i++)
                    {
                        float k = i / (float)segs;
                        if (horiz) Gfx.Rect(dc, brush, x0 + bw * k - th / 2f, y0, th, bh);
                        else Gfx.Rect(dc, brush, x0, y0 + bh * k - th / 2f, bw, th);
                    }
                    brush.Color = Gfx.C4(FrameColor, op);
                    dc.DrawRectangle(new RawRectF(x0, y0, x0 + bw, y0 + bh), brush, th);
                    if (ShowNumber) Number(dc, brush, horiz ? new Vector2(ctr.X, ctr.Y) : new Vector2(ctr.X, y0 - bw * 0.5f), MathF.Min(bw, bh) * (horiz ? 0.6f : 0.9f), (int)MathF.Round(v * 100f), MathF.Max(1.5f, th * 0.8f));
                    break;
                }
            case GaugeKind.HpBar:
                {
                    float sl = c.F(Slant);
                    float x0 = ctr.X - GW / 2f, y0 = ctr.Y - GH / 2f, x1 = x0 + GW, y1 = y0 + GH;
                    // 平行四辺形：左端と右端が斜め
                    List<Vector2> Para(float from, float to) => new()
                    {
                        new(x0 + sl + (GW - sl) * from, y0), new(x0 + sl + (GW - sl) * to, y0),
                        new(x0 + (GW - sl) * to, y1), new(x0 + (GW - sl) * from, y1),
                    };
                    // 背景（全体）
                    var full = new List<Vector2> { new(x0 + sl, y0), new(x1, y0), new(x1 - sl, y1), new(x0, y1) };
                    brush.Color = Gfx.C4(TrackColor, 0.85f * op);
                    Gfx.FillPoly(dc, brush, full);
                    if (vTrail > v)
                    {
                        brush.Color = Gfx.C4(white, 0.8f * op);
                        Gfx.FillPoly(dc, brush, Para(v, vTrail));
                    }
                    brush.Color = Gfx.C4(fillCol, op);
                    Gfx.FillPoly(dc, brush, Para(0f, v));
                    brush.Color = Gfx.C4(white, 0.2f * op);
                    var shine = Para(0f, v);
                    shine[2] = new Vector2(shine[2].X + sl * 0.65f, y0 + GH * 0.4f);
                    shine[3] = new Vector2(shine[3].X + sl * 0.65f, y0 + GH * 0.4f);
                    Gfx.FillPoly(dc, brush, shine);
                    brush.Color = Gfx.C4(TrackColor, op);
                    for (int i = 1; i < segs; i++)
                    {
                        float k = i / (float)segs;
                        dc.DrawLine(new Vector2(x0 + sl + (GW - sl) * k, y0), new Vector2(x0 + (GW - sl) * k, y1), brush, th);
                    }
                    brush.Color = Gfx.C4(FrameColor, op);
                    Gfx.Polyline(dc, brush, new List<Vector2>(full) { full[0] }, th);
                    if (ShowNumber) Number(dc, brush, new Vector2(x1 + GH * 0.9f, ctr.Y), GH * 0.8f, (int)MathF.Round(v * 100f), MathF.Max(1.5f, th * 0.8f));
                    break;
                }
            case GaugeKind.Circle:
                {
                    float R = GW / 2f, T = MathF.Max(4f, GH);
                    float rr = R - T / 2f;
                    float start = -MathF.PI / 2f;
                    brush.Color = Gfx.C4(TrackColor, 0.85f * op);
                    Gfx.Polyline(dc, brush, Gfx.ArcPts(ctr, rr, 0f, MathF.PI * 2f, 120), T);
                    if (vTrail > v)
                    {
                        brush.Color = Gfx.C4(white, 0.8f * op);
                        FormKit.Stroke(dc, brush, Gfx.ArcPts(ctr, rr, start + v * MathF.PI * 2f, start + vTrail * MathF.PI * 2f, 60), T, false);
                    }
                    if (v > 0.001f)
                    {
                        brush.Color = Gfx.C4(fillCol, 0.18f * op);
                        FormKit.Stroke(dc, brush, Gfx.ArcPts(ctr, rr, start, start + v * MathF.PI * 2f, 120), T * 1.6f, false);
                        brush.Color = Gfx.C4(fillCol, op);
                        FormKit.Stroke(dc, brush, Gfx.ArcPts(ctr, rr, start, start + v * MathF.PI * 2f, 120), T, false);
                        float ea = start + v * MathF.PI * 2f;
                        dc.FillEllipse(new Ellipse(ctr + new Vector2(MathF.Cos(ea), MathF.Sin(ea)) * rr, T * 0.5f, T * 0.5f), brush);
                    }
                    // 目盛り
                    brush.Color = Gfx.C4(FrameColor, 0.8f * op);
                    int ticks = segs > 0 ? segs : 40;
                    for (int i = 0; i < ticks; i++)
                    {
                        float a = i * MathF.PI * 2f / ticks;
                        var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                        dc.DrawLine(ctr + d * (R + th * 2f), ctr + d * (R + th * 2f + (i % 5 == 0 ? 12f : 6f)), brush, MathF.Max(1f, th * 0.5f));
                    }
                    brush.Color = Gfx.C4(FrameColor, op);
                    dc.DrawEllipse(new Ellipse(ctr, R, R), brush, th * 0.7f);
                    dc.DrawEllipse(new Ellipse(ctr, R - T, R - T), brush, th * 0.7f);
                    if (ShowNumber) Number(dc, brush, ctr, (R - T) * 0.55f, (int)MathF.Round(v * 100f), MathF.Max(2f, th));
                    break;
                }
            default:    // Meter
                {
                    float R = GW / 2f, T = MathF.Max(4f, GH * 0.3f);
                    float a0 = MathF.PI, a1 = MathF.PI * 2f;
                    float rr = R - T / 2f;
                    brush.Color = Gfx.C4(TrackColor, 0.85f * op);
                    FormKit.Stroke(dc, brush, Gfx.ArcPts(ctr, rr, a0, a1, 80), T, false);
                    // 緑・黄・赤のゾーン
                    var zones = new[] { (0f, 0.6f, WpfColor.FromRgb(0x3C, 0xE0, 0x6A)), (0.6f, 0.85f, WpfColor.FromRgb(0xFF, 0xD2, 0x3C)), (0.85f, 1f, WpfColor.FromRgb(0xFF, 0x3C, 0x3C)) };
                    foreach (var z in zones)
                    {
                        brush.Color = Gfx.C4(z.Item3, 0.85f * op);
                        FormKit.Stroke(dc, brush, Gfx.ArcPts(ctr, R + T * 0.15f, a0 + z.Item1 * MathF.PI, a0 + z.Item2 * MathF.PI, 40), T * 0.28f, false);
                    }
                    // 目盛り
                    int ticks = segs > 0 ? segs : 10;
                    brush.Color = Gfx.C4(FrameColor, op);
                    for (int i = 0; i <= ticks; i++)
                    {
                        float a = a0 + i / (float)ticks * MathF.PI;
                        var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                        dc.DrawLine(ctr + d * (R - T - 6f), ctr + d * (R - T - 6f - (i % 5 == 0 || ticks <= 10 ? 16f : 9f)), brush, MathF.Max(1.2f, th * 0.6f));
                    }
                    // 針
                    float na = a0 + v * MathF.PI;
                    var nd = new Vector2(MathF.Cos(na), MathF.Sin(na));
                    brush.Color = Gfx.C4(fillCol, 0.2f * op);
                    dc.DrawLine(ctr, ctr + nd * (R - T - 10f), brush, th * 4f);
                    brush.Color = Gfx.C4(fillCol, op);
                    dc.DrawLine(ctr - nd * (R * 0.1f), ctr + nd * (R - T - 10f), brush, th * 1.6f);
                    dc.FillEllipse(new Ellipse(ctr, R * 0.07f, R * 0.07f), brush);
                    brush.Color = Gfx.C4(FrameColor, op);
                    dc.DrawEllipse(new Ellipse(ctr, R * 0.07f, R * 0.07f), brush, th * 0.6f);
                    if (ShowNumber) Number(dc, brush, ctr + new Vector2(0, R * 0.3f), R * 0.22f, (int)MathF.Round(v * 100f), MathF.Max(1.5f, th * 0.7f));
                    break;
                }
        }
    }

    readonly record struct RectF(float L, float T, float R, float B);
}
