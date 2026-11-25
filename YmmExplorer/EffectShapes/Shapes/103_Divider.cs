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

public enum DividerStyle
{
    [Display(Name = "筆の下線")] Underline,
    [Display(Name = "花形の罫線")] Floral,
    [Display(Name = "両端が飾りの線")] Ornament,
    [Display(Name = "波線")] Wave,
    [Display(Name = "二重線")] Double,
    [Display(Name = "点線と菱形")] Dots,
}

// =====================================================================
//  飾り罫・区切り線
//   見出しの下線、花形の罫線、両端が飾りの線など。場面の区切りに
// =====================================================================
public sealed class DividerPlugin : IShapePlugin
{
    public string Name => "演出：飾り罫・区切り線";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DividerParameter(sharedData);
}

public sealed class DividerParameter : EffectShapeParameterBase
{
    public DividerParameter() : this(null) { }
    public DividerParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "形")]
    [EnumComboBox]
    public DividerStyle Style { get => style; set => Set(ref style, value); }
    DividerStyle style = DividerStyle.Floral;

    [Display(GroupName = "形", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "形", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "形", Name = "長さ")]
    [AnimationSlider("F0", "px", 100, 1800)]
    public Animation LineLength { get; } = new Animation(820, 10, 10000);

    [Display(GroupName = "形", Name = "太さ")]
    [AnimationSlider("F1", "px", 1, 20)]
    public Animation Thickness { get; } = new Animation(3.5, 0.2, 200);

    [Display(GroupName = "色", Name = "線の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xE6, 0xD2, 0xA0);

    [Display(GroupName = "色", Name = "飾りの色")]
    [ColorPicker]
    public WpfColor AccentColor { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0x8A, 0xA8);

    [Display(GroupName = "色", Name = "光らせる")]
    [ToggleSlider]
    public bool Glow { get => glow; set => Set(ref glow, value); }
    bool glow = false;

    [Display(GroupName = "動き", Name = "引かれる時間（最初）")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation IntroTime { get; } = new Animation(0.8, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, LineLength, Thickness, IntroTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float L = c.F(LineLength), th = c.F(Thickness), intro = c.F(IntroTime);
        float k = intro <= 0.001f ? 1f : DrawKit.EaseOut(t / intro);
        float half = L / 2f * k;
        float deco = intro <= 0.001f ? 1f : DrawKit.EaseOutBack(Math.Clamp((t - intro * 0.6f) / MathF.Max(0.05f, intro * 0.6f), 0f, 1f));

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (Glow)
            {
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                brush.Color = Gfx.C4(Color, 0.15f);
                dc.DrawLine(ctr - new Vector2(half, 0), ctr + new Vector2(half, 0), brush, th * 6f);
                DrawKit.Glow(dc, ctr, th * 18f * deco, Gfx.C4(AccentColor, 0.4f));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }

            switch (Style)
            {
                case DividerStyle.Underline:
                    {
                        // 中央が太く、両端に向かって細くなる筆の線
                        var top = new List<Vector2>();
                        var bot = new List<Vector2>();
                        for (int i = 0; i <= 60; i++)
                        {
                            float u = i / 60f;
                            float x = ctr.X - half + 2f * half * u;
                            float wv = th * 1.6f * MathF.Pow(MathF.Sin(MathF.PI * u), 0.6f) * (0.85f + 0.15f * MathF.Sin(u * 17f + Seed));
                            float bend = MathF.Sin(u * MathF.PI) * th * 1.5f;
                            top.Add(new Vector2(x, ctr.Y - wv + bend));
                            bot.Add(new Vector2(x, ctr.Y + wv * 0.7f + bend));
                        }
                        bot.Reverse();
                        top.AddRange(bot);
                        brush.Color = Gfx.C4(Color, 1f);
                        Gfx.FillPoly(dc, brush, top);
                        break;
                    }
                case DividerStyle.Floral:
                    {
                        float gap = th * 9f * deco;
                        brush.Color = Gfx.C4(Color, 1f);
                        if (half > gap)
                        {
                            dc.DrawLine(ctr - new Vector2(half, 0), ctr - new Vector2(gap, 0), brush, th);
                            dc.DrawLine(ctr + new Vector2(gap, 0), ctr + new Vector2(half, 0), brush, th);
                            // 先の小さな丸
                            dc.FillEllipse(new Ellipse(ctr - new Vector2(half, 0), th * 1.2f, th * 1.2f), brush);
                            dc.FillEllipse(new Ellipse(ctr + new Vector2(half, 0), th * 1.2f, th * 1.2f), brush);
                        }
                        // 葉
                        for (int s = -1; s <= 1; s += 2)
                        {
                            var lp = ctr + new Vector2(s * th * 8f * deco, 0);
                            var leaf = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, th * 3.2f, th * 1.2f, 20), lp, s * 0.45f, deco, deco);
                            brush.Color = Gfx.C4(Gfx.Mix(Color, WpfColor.FromRgb(90, 160, 90), 0.4f), 1f);
                            Gfx.FillPoly(dc, brush, leaf);
                        }
                        // 花（5枚の花びら）
                        float pr = th * 2.2f * deco;
                        for (int i = 0; i < 5; i++)
                        {
                            float a = -MathF.PI / 2f + i * MathF.PI * 2f / 5f + t * 0.2f;
                            var pp = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * pr * 1.1f;
                            brush.Color = Gfx.C4(AccentColor, 1f);
                            dc.FillEllipse(new Ellipse(pp, pr, pr), brush);
                        }
                        brush.Color = Gfx.C4(Gfx.Mix(AccentColor, WpfColor.FromRgb(255, 230, 120), 0.7f), 1f);
                        dc.FillEllipse(new Ellipse(ctr, pr * 0.8f, pr * 0.8f), brush);
                        break;
                    }
                case DividerStyle.Ornament:
                    {
                        brush.Color = Gfx.C4(Color, 1f);
                        dc.DrawLine(ctr - new Vector2(half, 0), ctr + new Vector2(half, 0), brush, th);
                        dc.DrawLine(ctr - new Vector2(half * 0.7f, th * 2.5f), ctr + new Vector2(half * 0.7f, th * 2.5f), brush, th * 0.4f);
                        // 両端のうず巻き
                        for (int s = -1; s <= 1; s += 2)
                        {
                            var end = ctr + new Vector2(s * half, 0);
                            var spiral = new List<Vector2>();
                            for (int i = 0; i <= 40; i++)
                            {
                                float u = i / 40f;
                                float a = MathF.PI * 2.2f * u;
                                float r = th * 5f * (1f - u * 0.8f) * deco;
                                spiral.Add(end + new Vector2(s * (MathF.Sin(a) * r), -MathF.Cos(a) * r + r));
                            }
                            Gfx.Polyline(dc, brush, spiral, th * 0.8f);
                        }
                        // 中央の菱形
                        float d = th * 4f * deco;
                        brush.Color = Gfx.C4(AccentColor, 1f);
                        Gfx.FillPoly(dc, brush, new List<Vector2> { ctr + new Vector2(0, -d), ctr + new Vector2(d * 1.4f, 0), ctr + new Vector2(0, d), ctr + new Vector2(-d * 1.4f, 0) });
                        brush.Color = Gfx.C4(Color, 1f);
                        dc.FillEllipse(new Ellipse(ctr, d * 0.35f, d * 0.35f), brush);
                        break;
                    }
                case DividerStyle.Wave:
                    {
                        var pts = new List<Vector2>();
                        float wl = th * 14f;
                        for (float x = -half; x <= half; x += 2f)
                            pts.Add(ctr + new Vector2(x, MathF.Sin(x / wl * MathF.PI * 2f + t * 2f) * th * 2.2f));
                        brush.Color = Gfx.C4(Color, 1f);
                        if (pts.Count > 1) Gfx.Polyline(dc, brush, pts, th);
                        break;
                    }
                case DividerStyle.Double:
                    {
                        brush.Color = Gfx.C4(Color, 1f);
                        dc.DrawLine(ctr - new Vector2(half, th * 1.4f), ctr + new Vector2(half, -th * 1.4f), brush, th);
                        dc.DrawLine(ctr - new Vector2(half * 0.92f, -th * 1.4f), ctr + new Vector2(half * 0.92f, th * 1.4f), brush, th * 0.45f);
                        brush.Color = Gfx.C4(AccentColor, 1f);
                        float d = th * 2.4f * deco;
                        for (int s = -1; s <= 1; s += 2)
                        {
                            var e = ctr + new Vector2(s * (half + d * 1.8f), 0);
                            Gfx.FillPoly(dc, brush, new List<Vector2> { e + new Vector2(0, -d), e + new Vector2(d, 0), e + new Vector2(0, d), e + new Vector2(-d, 0) });
                        }
                        break;
                    }
                case DividerStyle.Dots:
                    {
                        float stepX = th * 4.5f;
                        brush.Color = Gfx.C4(Color, 1f);
                        for (float x = stepX * 2.5f; x <= half; x += stepX)
                        {
                            float r = th * 0.75f * (1f - 0.5f * x / MathF.Max(1f, L / 2f));
                            dc.FillEllipse(new Ellipse(ctr + new Vector2(x, 0), r, r), brush);
                            dc.FillEllipse(new Ellipse(ctr - new Vector2(x, 0), r, r), brush);
                        }
                        float d = th * 3f * deco;
                        brush.Color = Gfx.C4(AccentColor, 1f);
                        Gfx.FillPoly(dc, brush, new List<Vector2> { ctr + new Vector2(0, -d), ctr + new Vector2(d, 0), ctr + new Vector2(0, d), ctr + new Vector2(-d, 0) });
                        break;
                    }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
