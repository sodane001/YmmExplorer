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
//  霧
//   横に流れるやわらかい霧。地面付近ほど濃くできる
// =====================================================================
public sealed class FogPlugin : IShapePlugin
{
    public string Name => "演出：霧";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FogParameter(sharedData);
}

public sealed class FogParameter : EffectShapeParameterBase
{
    public FogParameter() : this(null) { }
    public FogParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "霧", Name = "色")]
    [ColorPicker]
    public WpfColor FogColor { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xDC, 0xE6, 0xF0);

    [Display(GroupName = "霧", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Density { get; } = new Animation(50, 0, 100);

    [Display(GroupName = "霧", Name = "粒の数")]
    [AnimationSlider("F0", "個", 5, 200)]
    public Animation Count { get; } = new Animation(40, 1, 2000);

    [Display(GroupName = "霧", Name = "粒の大きさ")]
    [AnimationSlider("F0", "px", 50, 800)]
    public Animation Size { get; } = new Animation(260, 10, 5000);

    [Display(GroupName = "霧", Name = "流れる速度")]
    [AnimationSlider("F0", "px/s", -200, 200)]
    public Animation Speed { get; } = new Animation(25, -5000, 5000);

    [Display(GroupName = "霧", Name = "ゆらぎ")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation Swirl { get; } = new Animation(30, 0, 1000);

    [Display(GroupName = "範囲", Name = "霧の高さ", Description = "画面下から何%の高さまで霧を出すか")]
    [AnimationSlider("F0", "%", 5, 100)]
    public new Animation Height { get; } = new Animation(55, 1, 100);

    [Display(GroupName = "範囲", Name = "地面付近の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Ground { get; } = new Animation(40, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Density, Count, Size, Speed, Swirl, Height, Ground];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float dens = c.F(Density) / 100f;
        int n = (int)c.F(Count);
        float size = c.F(Size), spd = c.F(Speed), swirl = c.F(Swirl);
        float hgt = c.F(Height) / 100f;
        float ground = c.F(Ground) / 100f;

        // 地面付近の霧だまり
        float y0 = h * (1f - hgt);
        if (ground > 0f)
        {
            using var gb = Gfx.Linear(dc, new Vector2(0, y0), new Vector2(0, h),
                (0f, Gfx.C4(FogColor, 0f)),
                (1f, Gfx.C4(FogColor, ground * dens * 0.8f)));
            Gfx.Rect(dc, gb, 0, y0, w, h - y0);
        }

        // 流れる霧の塊（横に平たい楕円）
        float margin = size * 2f;
        float span = w + margin * 2f;
        var rng = new Random(Seed);
        try
        {
            for (int i = 0; i < n; i++)
            {
                int layer = i % 3;
                float x0 = rng.NextF() * span;
                float yr = rng.NextF();
                float r = size * (0.6f + 0.8f * rng.NextF()) * (0.8f + 0.2f * layer);
                float ph = rng.NextF() * MathF.PI * 2f;
                float aa = 0.6f + 0.4f * rng.NextF();

                float layerSpeed = 0.5f + 0.5f * layer;
                float x = Gfx.Mod(x0 + t * spd * layerSpeed, span) - margin;
                float y = h * (1f - MathF.Pow(yr, 1.6f) * hgt) + MathF.Sin(t * 0.2f + ph) * swirl;
                float a = dens * 0.16f * aa;
                var ctr = new Vector2(x, y);

                dc.Transform = Matrix3x2.CreateScale(1f, 0.55f, ctr);
                using var b = Gfx.Radial(dc, ctr, r,
                    (0f, Gfx.C4(FogColor, a)),
                    (0.5f, Gfx.C4(FogColor, a * 0.55f)),
                    (1f, Gfx.C4(FogColor, 0f)));
                dc.FillEllipse(new Ellipse(ctr, r, r), b);
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
