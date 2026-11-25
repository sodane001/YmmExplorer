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

public enum PlateStyle
{
    [Display(Name = "斜めの帯")] Slant,
    [Display(Name = "角丸のふだ")] Rounded,
    [Display(Name = "下線だけ")] Underline,
    [Display(Name = "ゲーム風の窓")] GameWindow,
}

// =====================================================================
//  話者名プレート
//   キャラ名がスッと出る帯。名前の文字は、上に YMM4 のテキストを重ねてください。
//   アイテムの終わりに自動で引っこむ
// =====================================================================
public sealed class NamePlatePlugin : IShapePlugin
{
    public string Name => "演出：話者名プレート";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NamePlateParameter(sharedData);
}

public sealed class NamePlateParameter : EffectShapeParameterBase
{
    public NamePlateParameter() : this(null) { }
    public NamePlateParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "形")]
    [EnumComboBox]
    public PlateStyle Style { get => style; set => Set(ref style, value); }
    PlateStyle style = PlateStyle.Slant;

    [Display(GroupName = "色", Name = "帯の色")]
    [ColorPicker]
    public WpfColor PlateColor { get => plateColor; set => Set(ref plateColor, value); }
    WpfColor plateColor = WpfColor.FromRgb(0x1E, 0x2A, 0x44);

    [Display(GroupName = "色", Name = "アクセントの色（キャラの色）")]
    [ColorPicker]
    public WpfColor AccentColor { get => accentColor; set => Set(ref accentColor, value); }
    WpfColor accentColor = WpfColor.FromRgb(0xFF, 0x5C, 0x8A);

    [Display(GroupName = "色", Name = "帯の不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(90, 0, 100);

    [Display(GroupName = "位置と大きさ", Name = "左端のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(6, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(70, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "幅")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation PlateWidth { get; } = new Animation(420, 10, 5000);

    [Display(GroupName = "位置と大きさ", Name = "高さ")]
    [AnimationSlider("F0", "px", 20, 200)]
    public Animation PlateHeight { get; } = new Animation(72, 4, 2000);

    [Display(GroupName = "位置と大きさ", Name = "右から出す")]
    [ToggleSlider]
    public bool FromRight { get => fromRight; set => Set(ref fromRight, value); }
    bool fromRight = false;

    [Display(GroupName = "動き", Name = "出る時間")]
    [AnimationSlider("F2", "秒", 0, 1.5)]
    public Animation InTime { get; } = new Animation(0.35, 0, 60);

    [Display(GroupName = "動き", Name = "引っこむ時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1.5)]
    public Animation OutTime { get; } = new Animation(0.25, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Opacity, PosX, PosY, PlateWidth, PlateHeight, InTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float PW = c.F(PlateWidth), PH = c.F(PlateHeight), op = c.F(Opacity) / 100f;
        float inT = c.F(InTime), outT = c.F(OutTime);
        float kin = inT <= 0.001f ? 1f : DrawKit.EaseOut(t / inT);
        float kout = outT <= 0.001f ? 1f : DrawKit.EaseOut(DrawKit.TimeLeft(c) / outT);
        float k = MathF.Min(kin, kout);
        if (k <= 0.001f) return;
        float x0 = c.F(PosX) / 100f * w, cy = c.F(PosY) / 100f * h;
        if (FromRight) x0 = w - x0 - PW;
        float dir = FromRight ? -1f : 1f;
        // アクセントの帯が先に、本体が少し遅れて伸びる
        float kAcc = Math.Clamp(k * 1.4f, 0f, 1f);
        float kBody = Math.Clamp(k * 1.4f - 0.3f, 0f, 1f) / 1.1f * 1.1f;
        float y0 = cy - PH / 2f, y1 = cy + PH / 2f;
        float slant = Style == PlateStyle.Slant ? PH * 0.45f : 0f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        float start = FromRight ? x0 + PW : x0;
        float bodyLen = PW * kBody;
        float accW = PH * 0.22f;

        switch (Style)
        {
            case PlateStyle.Underline:
                {
                    brush.Color = Gfx.C4(AccentColor, 1f);
                    float len = PW * kAcc;
                    float xa = FromRight ? start - len : start;
                    dc.FillRectangle(new RawRectF(xa, y1 - PH * 0.08f, xa + len, y1), brush);
                    brush.Color = Gfx.C4(AccentColor, 0.5f);
                    dc.FillRectangle(new RawRectF(xa, y1 + PH * 0.06f, xa + len * 0.6f, y1 + PH * 0.1f), brush);
                    if (bodyLen > 1f)
                    {
                        using var lg = Gfx.Linear(dc, new Vector2(start, 0), new Vector2(start + dir * PW, 0), (0f, Gfx.C4(PlateColor, op * 0.8f)), (1f, Gfx.C4(PlateColor, 0f)));
                        dc.FillRectangle(new RawRectF(FromRight ? start - bodyLen : start, y0, FromRight ? start : start + bodyLen, y1 - PH * 0.08f), lg);
                    }
                    break;
                }
            case PlateStyle.Rounded:
                {
                    if (bodyLen > PH * 0.3f)
                    {
                        float xa = FromRight ? start - bodyLen : start;
                        var rr = ShapeGeom.RoundRect(xa + bodyLen / 2f, cy, bodyLen, PH, PH * 0.5f);
                        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f * op);
                        Gfx.FillPoly(dc, brush, rr.Select(p => p + new Vector2(4f, 5f)).ToList());
                        brush.Color = Gfx.C4(PlateColor, op);
                        Gfx.FillPoly(dc, brush, rr);
                        brush.Color = Gfx.C4(AccentColor, 1f);
                        Gfx.Polyline(dc, brush, ShapeGeom.Closed(rr), MathF.Max(2f, PH * 0.06f));
                    }
                    brush.Color = Gfx.C4(AccentColor, 1f);
                    var dot = new Vector2(start + dir * PH * 0.5f, cy);
                    dc.FillEllipse(new Ellipse(dot, PH * 0.42f * kAcc, PH * 0.42f * kAcc), brush);
                    brush.Color = Gfx.C4(white, 0.9f * kAcc);
                    dc.FillEllipse(new Ellipse(dot, PH * 0.14f * kAcc, PH * 0.14f * kAcc), brush);
                    break;
                }
            case PlateStyle.GameWindow:
                {
                    if (bodyLen > 2f)
                    {
                        float xa = FromRight ? start - bodyLen : start;
                        brush.Color = Gfx.C4(PlateColor, op);
                        dc.FillRectangle(new RawRectF(xa, y0, xa + bodyLen, y1), brush);
                        brush.Color = Gfx.C4(white, 1f);
                        dc.DrawRectangle(new RawRectF(xa + 3f, y0 + 3f, xa + bodyLen - 3f, y1 - 3f), brush, MathF.Max(2f, PH * 0.05f));
                        brush.Color = Gfx.C4(AccentColor, 1f);
                        dc.DrawRectangle(new RawRectF(xa + PH * 0.12f, y0 + PH * 0.12f, xa + bodyLen - PH * 0.12f, y1 - PH * 0.12f), brush, MathF.Max(1f, PH * 0.025f));
                        // 右下の▼（点滅）
                        if (k >= 0.999f && MathF.Sin(t * 8f) > 0f)
                        {
                            float tx = FromRight ? xa + PH * 0.35f : xa + bodyLen - PH * 0.35f;
                            Gfx.FillPoly(dc, brush, new List<Vector2> { new(tx - PH * 0.1f, y1 - PH * 0.3f), new(tx + PH * 0.1f, y1 - PH * 0.3f), new(tx, y1 - PH * 0.18f) });
                        }
                    }
                    break;
                }
            default:
                {
                    // 斜めの帯：アクセント → 本体
                    float accLen = (accW + slant) * kAcc;
                    var acc = FromRight
                        ? new List<Vector2> { new(start, y0), new(start - accLen, y0), new(start - accLen + slant * kAcc, y1), new(start, y1) }
                        : new List<Vector2> { new(start, y1), new(start + accLen - slant * kAcc, y1), new(start + accLen, y0), new(start, y0) };
                    brush.Color = Gfx.C4(AccentColor, 1f);
                    Gfx.FillPoly(dc, brush, acc);
                    if (bodyLen > 1f)
                    {
                        float bs = start + dir * (accW + PH * 0.08f);
                        float be = bs + dir * bodyLen;
                        var body = FromRight
                            ? new List<Vector2> { new(bs, y0), new(be, y0), new(be + slant, y1), new(bs + slant, y1) }
                            : new List<Vector2> { new(bs - slant, y1), new(be - slant, y1), new(be, y0), new(bs, y0) };
                        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f * op);
                        Gfx.FillPoly(dc, brush, body.Select(p => p + new Vector2(5f, 5f)).ToList());
                        brush.Color = Gfx.C4(PlateColor, op);
                        Gfx.FillPoly(dc, brush, body);
                        // 下に細い光の線
                        brush.Color = Gfx.C4(AccentColor, 0.9f);
                        float lx0 = FromRight ? be + slant : bs - slant, lx1 = FromRight ? bs + slant : be - slant;
                        dc.FillRectangle(new RawRectF(MathF.Min(lx0, lx1), y1 + PH * 0.08f, MathF.Max(lx0, lx1), y1 + PH * 0.13f), brush);
                    }
                    break;
                }
        }
        // 出る瞬間の光
        if (kin < 1f)
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            float hx = start + dir * (PW * kBody + accW);
            DrawKit.Glow(dc, new Vector2(hx, cy), PH * 1.2f, Gfx.C4(AccentColor, 0.6f * (1f - kin)));
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
