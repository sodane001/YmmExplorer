using System.ComponentModel.DataAnnotations;
using System.Numerics;
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
//  レーザー
//   加算合成で光る光線。放射 / 平行、首振り、点滅
// =====================================================================
public sealed class LaserPlugin : IShapePlugin
{
    public string Name => "演出：レーザー";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LaserParameter(sharedData);
}

public sealed class LaserParameter : EffectShapeParameterBase
{
    public LaserParameter() : this(null) { }
    public LaserParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "レーザー", Name = "色")]
    [ColorPicker]
    public WpfColor LaserColor { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x2A, 0x3A);

    [Display(GroupName = "レーザー", Name = "配置", Description = "0=放射（1点から扇状）、1=平行（並べて同じ向き）")]
    [TextBoxSlider("F0", "", 0, 1)]
    [Range(0, 1)]
    public int Mode { get => mode; set => Set(ref mode, value); }
    int mode = 0;

    [Display(GroupName = "レーザー", Name = "本数")]
    [AnimationSlider("F0", "本", 1, 30)]
    public Animation Count { get; } = new Animation(5, 1, 200);

    [Display(GroupName = "レーザー", Name = "太さ")]
    [AnimationSlider("F1", "px", 1, 20)]
    public Animation Thickness { get; } = new Animation(3, 0.5, 200);

    [Display(GroupName = "レーザー", Name = "光のにじみ")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation Glow { get; } = new Animation(24, 0, 1000);

    [Display(GroupName = "レーザー", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "レーザー", Name = "長さ")]
    [AnimationSlider("F0", "px", 200, 4000)]
    public Animation Length { get; } = new Animation(2500, 1, 20000);

    [Display(GroupName = "位置と向き", Name = "発射位置X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation OriginX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と向き", Name = "発射位置Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation OriginY { get; } = new Animation(-5, -100, 200);

    [Display(GroupName = "位置と向き", Name = "向き", Description = "0=真下、プラスで右へ")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "位置と向き", Name = "放射の広がり")]
    [AnimationSlider("F0", "°", 0, 180)]
    public Animation Spread { get; } = new Animation(80, 0, 360);

    [Display(GroupName = "位置と向き", Name = "平行の間隔")]
    [AnimationSlider("F0", "px", 10, 400)]
    public Animation Spacing { get; } = new Animation(120, 1, 5000);

    [Display(GroupName = "動き", Name = "首振りの角度")]
    [AnimationSlider("F0", "°", 0, 90)]
    public Animation SweepAmp { get; } = new Animation(25, 0, 360);

    [Display(GroupName = "動き", Name = "首振りの速さ")]
    [AnimationSlider("F2", "回/秒", 0, 3)]
    public Animation SweepSpeed { get; } = new Animation(0.3, 0, 30);

    [Display(GroupName = "動き", Name = "本ごとのずれ")]
    [AnimationSlider("F2", "rad", 0, 3.14)]
    public Animation PhaseStep { get; } = new Animation(0.5, 0, 10);

    [Display(GroupName = "動き", Name = "点滅の速さ", Description = "0で点滅しない")]
    [AnimationSlider("F1", "回/秒", 0, 20)]
    public Animation BlinkHz { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "動き", Name = "点灯の割合")]
    [AnimationSlider("F0", "%", 5, 100)]
    public Animation BlinkDuty { get; } = new Animation(60, 1, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Thickness, Glow, Brightness, Length, OriginX, OriginY, Angle, Spread, Spacing,
         SweepAmp, SweepSpeed, PhaseStep, BlinkHz, BlinkDuty];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = Math.Max(1, (int)c.F(Count));
        float th = c.F(Thickness), glow = c.F(Glow), bright = c.F(Brightness) / 100f, len = c.F(Length);
        var origin = new Vector2(c.F(OriginX) / 100f * w, c.F(OriginY) / 100f * h);
        float baseAng = c.F(Angle), spread = c.F(Spread), spacing = c.F(Spacing);
        float amp = c.F(SweepAmp), sp = c.F(SweepSpeed), step = c.F(PhaseStep);
        float blink = c.F(BlinkHz), duty = c.F(BlinkDuty) / 100f;

        float blinkMul = 1f;
        if (blink > 0f) blinkMul = Gfx.Mod(t * blink, 1f) < duty ? 1f : 0.04f;
        bright *= blinkMul;
        if (bright <= 0.001f) return;

        var d0 = Gfx.Dir(baseAng);
        var perp0 = new Vector2(-d0.Y, d0.X);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            for (int i = 0; i < n; i++)
            {
                float u = n == 1 ? 0f : i / (float)(n - 1) - 0.5f;
                float swing = amp * MathF.Sin(t * sp * MathF.PI * 2f + i * step);
                float ang = baseAng + swing + (Mode == 0 ? u * spread : 0f);
                var dir = Gfx.Dir(ang);
                var start = Mode == 0 ? origin : origin + perp0 * (u * spacing * (n - 1));
                var end = start + dir * len;

                if (glow > 0f)
                {
                    brush.Color = Gfx.C4(LaserColor, 0.10f * bright);
                    dc.DrawLine(start, end, brush, glow);
                    brush.Color = Gfx.C4(LaserColor, 0.22f * bright);
                    dc.DrawLine(start, end, brush, glow * 0.45f);
                }
                brush.Color = Gfx.C4(LaserColor, 0.6f * bright);
                dc.DrawLine(start, end, brush, th * 2.4f);
                brush.Color = Gfx.C4(white, 0.95f * bright);
                dc.DrawLine(start, end, brush, th);

                // 発射口の光
                float fr = MathF.Max(8f, glow * 1.5f);
                using var fb = Gfx.Radial(dc, start, fr,
                    (0f, Gfx.C4(white, 0.7f * bright)), (0.35f, Gfx.C4(LaserColor, 0.4f * bright)), (1f, Gfx.C4(LaserColor, 0f)));
                dc.FillEllipse(new Ellipse(start, fr, fr), fb);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
