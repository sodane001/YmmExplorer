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
//  蛍
//   ゆっくり漂いながら明滅する光
// =====================================================================
public sealed class FirefliesPlugin : IShapePlugin
{
    public string Name => "演出：蛍";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FirefliesParameter(sharedData);
}

public sealed class FirefliesParameter : EffectShapeParameterBase
{
    public FirefliesParameter() : this(null) { }
    public FirefliesParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "蛍", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xC8, 0xFF, 0x6A);

    [Display(GroupName = "蛍", Name = "数")]
    [AnimationSlider("F0", "匹", 0, 200)]
    public Animation Count { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "蛍", Name = "光の核の大きさ")]
    [AnimationSlider("F1", "px", 1, 12)]
    public Animation Size { get; } = new Animation(3.5, 0.5, 100);

    [Display(GroupName = "蛍", Name = "光のにじみ")]
    [AnimationSlider("F0", "px", 5, 100)]
    public Animation Glow { get; } = new Animation(26, 2, 1000);

    [Display(GroupName = "動き", Name = "飛ぶ速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation FlySpeed { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "動き", Name = "飛ぶ範囲")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation FlyRange { get; } = new Animation(140, 1, 3000);

    [Display(GroupName = "動き", Name = "明滅の速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation BlinkSpeed { get; } = new Animation(100, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Glow, FlySpeed, FlyRange, BlinkSpeed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float ft = c.Time * c.F(FlySpeed) / 100f, bt = c.Time * c.F(BlinkSpeed) / 100f;
        int n = (int)c.F(Count);
        float size = c.F(Size), glow = c.F(Glow), range = c.F(FlyRange);
        var core = Gfx.Mix(Color, WpfColor.FromRgb(255, 255, 255), 0.6f);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            var rng = new Random(Seed);
            for (int i = 0; i < n; i++)
            {
                float bx = rng.NextF() * w, by = rng.NextF() * h;
                float ax = range * (0.4f + 0.6f * rng.NextF()), ay = range * 0.7f * (0.4f + 0.6f * rng.NextF());
                float fx = 0.05f + 0.2f * rng.NextF(), fy = 0.05f + 0.2f * rng.NextF();
                float p1 = rng.NextF() * MathF.PI * 2f, p2 = rng.NextF() * MathF.PI * 2f, p3 = rng.NextF() * MathF.PI * 2f;
                float bf = 0.4f + 0.8f * rng.NextF();
                float d = 0.5f + 0.5f * rng.NextF();

                var pos = FormKit.Liss(new Vector2(bx, by), ax, ay, ft, fx, fy, p1, p2);
                float b = MathF.Pow(MathF.Max(0f, MathF.Sin(bt * bf * 2.2f + p3)), 2f);
                float a = (0.12f + 0.88f * b) * d;
                float gr = glow * d * (0.5f + 0.5f * b);

                brush.Color = Gfx.C4(Color, 0.08f * a);
                dc.FillEllipse(new Ellipse(pos, gr, gr), brush);
                brush.Color = Gfx.C4(Color, 0.14f * a);
                dc.FillEllipse(new Ellipse(pos, gr * 0.55f, gr * 0.55f), brush);
                brush.Color = Gfx.C4(Color, 0.4f * a);
                dc.FillEllipse(new Ellipse(pos, size * 2f * d, size * 2f * d), brush);
                brush.Color = Gfx.C4(core, 0.95f * a);
                dc.FillEllipse(new Ellipse(pos, size * d, size * d), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
