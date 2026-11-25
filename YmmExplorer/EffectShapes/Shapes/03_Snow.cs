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
//  雪
// =====================================================================
public sealed class SnowPlugin : IShapePlugin
{
    public string Name => "演出：雪";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SnowParameter(sharedData);
}

public sealed class SnowParameter : EffectShapeParameterBase
{
    public SnowParameter() : this(null) { }
    public SnowParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "雪", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "雪", Name = "個数")]
    [AnimationSlider("F0", "個", 0, 1000)]
    public Animation Count { get; } = new Animation(200, 0, 20000);

    [Display(GroupName = "雪", Name = "最大サイズ")]
    [AnimationSlider("F1", "px", 1, 30)]
    public Animation Size { get; } = new Animation(6, 0.5, 200);

    [Display(GroupName = "雪", Name = "落下速度")]
    [AnimationSlider("F0", "px/s", 10, 600)]
    public Animation Speed { get; } = new Animation(100, 1, 5000);

    [Display(GroupName = "雪", Name = "揺れ幅")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Sway { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "雪", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(90, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Count, Size, Speed, Sway, Opacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        int n = (int)c.F(Count);
        float size = c.F(Size), spd = c.F(Speed), sway = c.F(Sway), op = c.F(Opacity) / 100f;

        var rng = new Random(Seed);
        using var brush = dc.CreateSolidColorBrush(default);
        for (int i = 0; i < n; i++)
        {
            float rx = rng.NextF(), ph = rng.NextF();
            float d = 0.3f + 0.7f * rng.NextF();
            float fr = 0.5f + rng.NextF();
            float sp = rng.NextF() * MathF.PI * 2f;

            float r = size * d;
            float span = h + 4f * r;
            float y = -2f * r + Gfx.Mod(c.Time * spd * d + ph * span, span);
            float x = rx * w + MathF.Sin(c.Time * fr + sp) * sway * d;
            float a = op * (0.4f + 0.6f * d);

            var center = new Vector2(x, y);
            brush.Color = Gfx.C4(Color, a * 0.3f);           // ふんわりしたにじみ
            dc.FillEllipse(new Ellipse(center, r * 1.6f, r * 1.6f), brush);
            brush.Color = Gfx.C4(Color, a);
            dc.FillEllipse(new Ellipse(center, r, r), brush);
        }
    }
}
