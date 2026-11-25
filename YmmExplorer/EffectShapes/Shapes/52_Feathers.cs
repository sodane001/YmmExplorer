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
//  羽根の舞い
//   ふわりと揺れながら舞い落ちる羽根。柔らかい光のにじみつき
// =====================================================================
public sealed class FeathersPlugin : IShapePlugin
{
    public string Name => "演出：羽根の舞い";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FeathersParameter(sharedData);
}

public sealed class FeathersParameter : EffectShapeParameterBase
{
    public FeathersParameter() : this(null) { }
    public FeathersParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "羽根", Name = "色")]
    [ColorPicker]
    public WpfColor FeatherColor { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);

    [Display(GroupName = "羽根", Name = "光のにじみ")]
    [ToggleSlider]
    public bool Glow { get => glow; set => Set(ref glow, value); }
    bool glow = true;

    [Display(GroupName = "羽根", Name = "数")]
    [AnimationSlider("F0", "枚", 0, 200)]
    public Animation Count { get; } = new Animation(36, 0, 2000);

    [Display(GroupName = "羽根", Name = "大きさ（長さの半分）")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation Size { get; } = new Animation(42, 3, 1000);

    [Display(GroupName = "動き", Name = "落ちる速さ")]
    [AnimationSlider("F0", "px/s", 10, 300)]
    public Animation Fall { get; } = new Animation(60, 1, 3000);

    [Display(GroupName = "動き", Name = "風（横方向）")]
    [AnimationSlider("F0", "px/s", -300, 300)]
    public Animation Wind { get; } = new Animation(30, -5000, 5000);

    [Display(GroupName = "動き", Name = "揺れ幅")]
    [AnimationSlider("F0", "px", 0, 300)]
    public Animation Sway { get; } = new Animation(90, 0, 3000);

    [Display(GroupName = "動き", Name = "くるくる回る")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Spin { get; } = new Animation(80, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Fall, Wind, Sway, Spin];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), fall = c.F(Fall), wind = c.F(Wind), sway = c.F(Sway), spin = c.F(Spin) / 100f;
        float margin = size * 3f, spanX = w + margin * 2f, spanY = h + margin * 2f;
        var shade = Gfx.Mix(FeatherColor, WpfColor.FromRgb(0x90, 0xA0, 0xC0), 0.45f);

        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);
        try
        {
            for (int i = 0; i < n; i++)
            {
                float x0 = rng.NextF() * spanX, ph = rng.NextF();
                float d = 0.5f + 0.5f * rng.NextF(), fr = 0.5f + 0.7f * rng.NextF(), ph2 = rng.NextF() * MathF.PI * 2f;
                float rot0 = rng.NextF() * MathF.PI * 2f, fs = 1.2f + 1.8f * rng.NextF(), fp = rng.NextF() * MathF.PI * 2f;

                float swing = MathF.Sin(t * fr + ph2);
                float y = -margin + Gfx.Mod(t * fall * d + ph * spanY, spanY);
                float x = Gfx.Mod(x0 + t * wind * d + swing * sway * d, spanX) - margin;
                float angle = rot0 + swing * 1.1f + t * 0.3f * spin;
                float sy = MathF.Max(0.25f, MathF.Abs(MathF.Cos(t * fs * spin + fp)));
                float L = size * (0.6f + 0.4f * d);
                var ctr = new Vector2(x, y);
                float a = 0.55f + 0.45f * d;

                if (Glow)
                {
                    brush.Color = Gfx.C4(FeatherColor, 0.08f * a);
                    dc.FillEllipse(new Ellipse(ctr, L * 0.9f, L * 0.9f), brush);
                }

                dc.Transform = Matrix3x2.CreateScale(1f, sy, ctr) * Matrix3x2.CreateRotation(angle, ctr);
                // 羽根の形：先端が細くなる細長い葉形
                var poly = new List<Vector2>();
                for (int k = 0; k <= 14; k++)
                {
                    float f = k / 14f;
                    float xx = -L + 2f * L * f;
                    poly.Add(ctr + new Vector2(xx, -L * 0.22f * MathF.Pow(MathF.Sin(f * MathF.PI), 0.9f) * (1f - 0.35f * f)));
                }
                for (int k = 14; k >= 0; k--)
                {
                    float f = k / 14f;
                    float xx = -L + 2f * L * f;
                    poly.Add(ctr + new Vector2(xx, L * 0.22f * MathF.Pow(MathF.Sin(f * MathF.PI), 0.9f) * (1f - 0.35f * f)));
                }
                brush.Color = Gfx.C4(FeatherColor, 0.95f * a);
                Gfx.FillPoly(dc, brush, poly);
                // 羽枝（斜めの細い線）と羽軸
                brush.Color = Gfx.C4(shade, 0.6f * a);
                for (int k = 1; k < 9; k++)
                {
                    float f = k / 9f;
                    var bp = ctr + new Vector2(-L + 2f * L * f, 0f);
                    float bl = L * 0.2f * MathF.Sin(f * MathF.PI);
                    dc.DrawLine(bp, bp + new Vector2(bl * 0.7f, -bl), brush, 0.9f);
                    dc.DrawLine(bp, bp + new Vector2(bl * 0.7f, bl), brush, 0.9f);
                }
                brush.Color = Gfx.C4(shade, 0.9f * a);
                dc.DrawLine(ctr + new Vector2(-L * 1.15f, 0f), ctr + new Vector2(L, 0f), brush, 1.4f);
                dc.Transform = Matrix3x2.Identity;
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
