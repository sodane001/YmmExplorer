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
//  スポットライト
//   周囲を暗くして、指定位置だけを照らす。ビームと舞う埃つき
// =====================================================================
public sealed class SpotlightPlugin : IShapePlugin
{
    public string Name => "演出：スポットライト";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpotlightParameter(sharedData);
}

public sealed class SpotlightParameter : EffectShapeParameterBase
{
    public SpotlightParameter() : this(null) { }
    public SpotlightParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(55, -100, 200);

    [Display(GroupName = "光", Name = "光の色")]
    [ColorPicker]
    public WpfColor LightColor { get => light; set => Set(ref light, value); }
    WpfColor light = WpfColor.FromRgb(0xFF, 0xF4, 0xD0);

    [Display(GroupName = "光", Name = "半径")]
    [AnimationSlider("F0", "px", 50, 1000)]
    public Animation Radius { get; } = new Animation(260, 5, 8000);

    [Display(GroupName = "光", Name = "ぼかし")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Softness { get; } = new Animation(50, 0, 100);

    [Display(GroupName = "光", Name = "光の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Intensity { get; } = new Animation(80, 0, 500);

    [Display(GroupName = "光", Name = "ちらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "周囲", Name = "周囲の色")]
    [ColorPicker]
    public WpfColor DarkColor { get => dark; set => Set(ref dark, value); }
    WpfColor dark = WpfColor.FromRgb(0, 0, 0);

    [Display(GroupName = "周囲", Name = "周囲の暗さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Darkness { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "ビーム", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BeamStrength { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "ビーム", Name = "光源X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BeamX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ビーム", Name = "光源Y位置")]
    [AnimationSlider("F0", "%", -50, 50)]
    public Animation BeamY { get; } = new Animation(-10, -300, 300);

    [Display(GroupName = "ビーム", Name = "舞う埃の数")]
    [AnimationSlider("F0", "個", 0, 300)]
    public Animation DustCount { get; } = new Animation(60, 0, 3000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Softness, Intensity, Flicker, Darkness, BeamStrength, BeamX, BeamY, DustCount];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var center = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius);
        float soft = c.F(Softness) / 100f;
        float flick = 1f - c.F(Flicker) / 100f * (0.5f + 0.5f * MathF.Sin(t * 13f) * MathF.Sin(t * 7.3f + 1f));
        float I = c.F(Intensity) / 100f * flick;
        float dark = c.F(Darkness) / 100f;
        float bs = c.F(BeamStrength) / 100f;

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 1. 周囲を暗くする（中心は透明）----
        float Ro = R * (1f + 0.6f * soft), Ri = R * (1f - 0.6f * soft);
        using (var g = Gfx.Radial(dc, center, Ro,
            (0f, Gfx.C4(DarkColor, 0f)),
            (Math.Clamp(Ri / Ro, 0f, 0.99f), Gfx.C4(DarkColor, 0f)),
            (1f, Gfx.C4(DarkColor, dark))))
            Gfx.Rect(dc, g, 0, 0, w, h);

        // ---- 2. 光だまり ----
        float gr = Ro * 1.1f;
        using (var gl = Gfx.Radial(dc, center, gr,
            (0f, Gfx.C4(LightColor, 0.55f * I)),
            (0.7f, Gfx.C4(LightColor, 0.22f * I)),
            (1f, Gfx.C4(LightColor, 0f))))
            dc.FillEllipse(new Ellipse(center, gr, gr), gl);

        // ---- 3. ビーム（光源から照らす位置へ広がる）----
        var origin = new Vector2(c.F(BeamX) / 100f * w, c.F(BeamY) / 100f * h);
        if (bs > 0f)
        {
            const int steps = 28;
            for (int s = 0; s < steps; s++)
            {
                float a = s / (float)steps, b = (s + 1) / (float)steps, m = (a + b) / 2f;
                var p0 = Vector2.Lerp(origin, center, a);
                var p1 = Vector2.Lerp(origin, center, b);
                float width = 10f + (R * 1.8f - 10f) * m;
                brush.Color = Gfx.C4(LightColor, bs * I * (0.35f + 0.65f * m) * 0.10f);
                dc.DrawLine(p0, p1, brush, width);
            }
        }

        // ---- 4. 舞う埃 ----
        int dn = (int)c.F(DustCount);
        if (dn > 0 && bs > 0f)
        {
            var dir = center - origin;
            float len = dir.Length();
            if (len > 1f)
            {
                dir /= len;
                var perp = new Vector2(-dir.Y, dir.X);
                var rd = new Random(Seed + 11);
                for (int i = 0; i < dn; i++)
                {
                    float s = 0.1f + 0.9f * rd.NextF();
                    float off = rd.NextF() * 2f - 1f;
                    float ph = rd.NextF() * MathF.PI * 2f, fr = 0.3f + rd.NextF();
                    float r = 0.8f + rd.NextF() * 1.6f;
                    var pos = Vector2.Lerp(origin, center, s) + perp * (off * (10f + (R * 1.8f - 10f) * s) * 0.5f);
                    pos += new Vector2(MathF.Sin(t * fr + ph) * 10f, MathF.Cos(t * fr * 0.8f + ph) * 14f);
                    float tw = 0.5f + 0.5f * MathF.Sin(t * 1.5f * fr + ph);
                    brush.Color = Gfx.C4(LightColor, 0.55f * tw * bs * I);
                    dc.FillEllipse(new Ellipse(pos, r, r), brush);
                }
            }
        }
    }
}
