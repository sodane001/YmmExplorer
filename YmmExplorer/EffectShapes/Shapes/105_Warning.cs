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

public enum WarnLayout
{
    [Display(Name = "上下の帯")] TopBottom,
    [Display(Name = "中央の帯（WARNING）")] Center,
    [Display(Name = "両方")] Both,
}

// =====================================================================
//  警告表示
//   黄黒の斜線帯と点滅するマーク。WARNING 風の演出
// =====================================================================
public sealed class WarningPlugin : IShapePlugin
{
    public string Name => "演出：警告表示";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WarningParameter(sharedData);
}

public sealed class WarningParameter : EffectShapeParameterBase
{
    public WarningParameter() : this(null) { }
    public WarningParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "配置", Name = "配置")]
    [EnumComboBox]
    public WarnLayout Layout { get => layout; set => Set(ref layout, value); }
    WarnLayout layout = WarnLayout.Both;

    [Display(GroupName = "配置", Name = "中央の帯のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CenterY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "配置", Name = "帯の太さ")]
    [AnimationSlider("F0", "px", 20, 200)]
    public Animation BandHeight { get; } = new Animation(64, 2, 2000);

    [Display(GroupName = "色", Name = "色1（黄）")]
    [ColorPicker]
    public WpfColor Color1 { get => color1; set => Set(ref color1, value); }
    WpfColor color1 = WpfColor.FromRgb(0xFF, 0xD0, 0x1C);

    [Display(GroupName = "色", Name = "色2（黒）")]
    [ColorPicker]
    public WpfColor Color2 { get => color2; set => Set(ref color2, value); }
    WpfColor color2 = WpfColor.FromRgb(0x14, 0x14, 0x14);

    [Display(GroupName = "色", Name = "赤い点滅の色")]
    [ColorPicker]
    public WpfColor RedColor { get => redColor; set => Set(ref redColor, value); }
    WpfColor redColor = WpfColor.FromRgb(0xFF, 0x1A, 0x1A);

    [Display(GroupName = "斜線", Name = "斜線の幅")]
    [AnimationSlider("F0", "px", 10, 150)]
    public Animation StripeWidth { get; } = new Animation(44, 2, 2000);

    [Display(GroupName = "斜線", Name = "斜線の角度")]
    [AnimationSlider("F0", "°", -70, 70)]
    public Animation Slant { get; } = new Animation(45, -80, 80);

    [Display(GroupName = "斜線", Name = "流れる速さ")]
    [AnimationSlider("F0", "px/秒", -400, 400)]
    public Animation Scroll { get; } = new Animation(120, -5000, 5000);

    [Display(GroupName = "点滅", Name = "点滅の速さ")]
    [AnimationSlider("F1", "回/秒", 0, 6)]
    public Animation BlinkRate { get; } = new Animation(1.5, 0, 60);

    [Display(GroupName = "点滅", Name = "画面のふちを赤く点滅させる")]
    [ToggleSlider]
    public bool RedFlash { get => redFlash; set => Set(ref redFlash, value); }
    bool redFlash = true;

    [Display(GroupName = "点滅", Name = "⚠マークを表示する")]
    [ToggleSlider]
    public bool Icon { get => icon; set => Set(ref icon, value); }
    bool icon = true;

    [Display(GroupName = "点滅", Name = "WARNING の文字を表示する")]
    [ToggleSlider]
    public bool Text { get => text; set => Set(ref text, value); }
    bool text = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [CenterY, BandHeight, StripeWidth, Slant, Scroll, BlinkRate];

    void Band(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, float w, float y0, float y1, float sw, float slantDeg, float scroll)
    {
        brush.Color = Gfx.C4(Color1, 1f);
        dc.FillRectangle(new RawRectF(0, y0, w, y1), brush);
        float bh = y1 - y0;
        float dx = bh * MathF.Tan(Gfx.Rad(slantDeg));
        float period = sw * 2f;
        float off = Gfx.Mod(scroll, period);
        brush.Color = Gfx.C4(Color2, 1f);
        float start = -period * 2f - MathF.Abs(dx);
        for (float x = start + off; x < w + period + MathF.Abs(dx); x += period)
        {
            var poly = new List<Vector2> { new(x + dx, y0), new(x + dx + sw, y0), new(x + sw, y1), new(x, y1) };
            var clipped = DrawKit.ClipRect(poly, 0f, y0, w, y1);
            if (clipped.Count >= 3) Gfx.FillPoly(dc, brush, clipped);
        }
        brush.Color = Gfx.C4(Color2, 1f);
        dc.FillRectangle(new RawRectF(0, y0, w, y0 + MathF.Max(2f, bh * 0.06f)), brush);
        dc.FillRectangle(new RawRectF(0, y1 - MathF.Max(2f, bh * 0.06f), w, y1), brush);
    }

    void Triangle(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 c, float s, float alpha)
    {
        var tri = new List<Vector2> { c + new Vector2(0, -s * 0.62f), c + new Vector2(s * 0.62f, s * 0.48f), c + new Vector2(-s * 0.62f, s * 0.48f) };
        brush.Color = Gfx.C4(Color1, alpha);
        Gfx.FillPoly(dc, brush, tri);
        brush.Color = Gfx.C4(Color2, alpha);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(tri), MathF.Max(2f, s * 0.07f));
        StrokeFont.DrawCentered(dc, brush, "!", c + new Vector2(0, s * 0.08f), s * 0.5f, MathF.Max(2f, s * 0.1f));
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float bh = c.F(BandHeight), sw = c.F(StripeWidth), sl = c.F(Slant), scroll = c.F(Scroll) * t;
        float rate = c.F(BlinkRate);
        float blink = rate <= 0f ? 1f : (MathF.Sin(t * rate * MathF.PI * 2f) > -0.2f ? 1f : 0.25f);
        float soft = rate <= 0f ? 1f : 0.5f + 0.5f * MathF.Sin(t * rate * MathF.PI * 2f);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (RedFlash)
            {
                float diag = MathF.Sqrt(w * w + h * h) / 2f;
                using var g = Gfx.Radial(dc, new Vector2(w / 2f, h / 2f), diag,
                    (0f, Gfx.C4(RedColor, 0f)), (0.6f, Gfx.C4(RedColor, 0f)), (1f, Gfx.C4(RedColor, 0.55f * soft)));
                dc.FillRectangle(new RawRectF(0, 0, w, h), g);
            }

            if (Layout != WarnLayout.Center)
            {
                Band(dc, brush, w, 0f, bh, sw, sl, scroll);
                Band(dc, brush, w, h - bh, h, sw, sl, -scroll);
            }

            if (Layout != WarnLayout.TopBottom)
            {
                float cy = c.F(CenterY) / 100f * h;
                float stripe = bh * 0.38f, mid = bh * 1.5f;
                float top = cy - mid / 2f - stripe, bot = cy + mid / 2f + stripe;
                Band(dc, brush, w, top, top + stripe, sw * 0.6f, sl, -scroll * 0.7f);
                Band(dc, brush, w, bot - stripe, bot, sw * 0.6f, sl, scroll * 0.7f);
                brush.Color = Gfx.C4(Color2, 0.92f);
                dc.FillRectangle(new RawRectF(0, top + stripe, w, bot - stripe), brush);
                float th = mid * 0.55f;
                if (Text)
                {
                    float lw = MathF.Max(2f, th * 0.13f);
                    brush.Color = Gfx.C4(RedColor, 0.35f * blink);
                    StrokeFont.DrawCentered(dc, brush, "WARNING", new Vector2(w / 2f, cy), th, lw * 2.4f, 0.32f);
                    brush.Color = Gfx.C4(Color1, blink);
                    StrokeFont.DrawCentered(dc, brush, "WARNING", new Vector2(w / 2f, cy), th, lw, 0.32f);
                }
                if (Icon)
                {
                    float tw = Text ? StrokeFont.Width("WARNING", th, 0.32f) : 0f;
                    float gap = th * 1.2f;
                    Triangle(dc, brush, new Vector2(w / 2f - tw / 2f - gap, cy), mid * 0.75f, blink);
                    Triangle(dc, brush, new Vector2(w / 2f + tw / 2f + gap, cy), mid * 0.75f, blink);
                }
            }
            else if (Icon)
            {
                Triangle(dc, brush, new Vector2(w / 2f, h / 2f), MathF.Min(w, h) * 0.22f, blink);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
