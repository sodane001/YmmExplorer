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
//  雨
// =====================================================================
public sealed class RainPlugin : IShapePlugin
{
    public string Name => "演出：雨";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RainParameter(sharedData);
}

public sealed class RainParameter : EffectShapeParameterBase
{
    public RainParameter() : this(null) { }
    public RainParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "雨", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xBF, 0xD9, 0xFF);

    [Display(GroupName = "雨", Name = "本数")]
    [AnimationSlider("F0", "本", 0, 2000)]
    public Animation Count { get; } = new Animation(300, 0, 20000);

    [Display(GroupName = "雨", Name = "長さ")]
    [AnimationSlider("F0", "px", 5, 300)]
    public Animation Length { get; } = new Animation(60, 1, 2000);

    [Display(GroupName = "雨", Name = "速度")]
    [AnimationSlider("F0", "px/s", 100, 5000)]
    public Animation Speed { get; } = new Animation(1800, 1, 20000);

    [Display(GroupName = "雨", Name = "角度", Description = "0=真下、プラスで右向きに降る")]
    [AnimationSlider("F1", "°", -60, 60)]
    public Animation Angle { get; } = new Animation(10, -89, 89);

    [Display(GroupName = "雨", Name = "太さ")]
    [AnimationSlider("F1", "px", 0.5, 10)]
    public Animation Thickness { get; } = new Animation(1.5, 0.1, 100);

    [Display(GroupName = "雨", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(60, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Count, Length, Speed, Angle, Thickness, Opacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        int n = (int)c.F(Count);
        float len = c.F(Length), spd = c.F(Speed), th = c.F(Thickness), op = c.F(Opacity) / 100f;
        float ang = Gfx.Rad(c.F(Angle));
        float tan = MathF.Tan(ang);
        var dir = new Vector2(MathF.Sin(ang), MathF.Cos(ang));

        float span = h + len;
        float xr = w + MathF.Abs(tan) * h;
        float xo = -MathF.Max(tan, 0f) * h;

        var rng = new Random(Seed);
        using var brush = dc.CreateSolidColorBrush(default);
        for (int i = 0; i < n; i++)
        {
            float rx = rng.NextF(), ph = rng.NextF();
            float d = 0.5f + 0.5f * rng.NextF();            // 奥行き（手前ほど速く太い）
            float y = -len + Gfx.Mod(c.Time * spd * d + ph * span, span);
            float x = xo + rx * xr + y * tan;
            var head = new Vector2(x, y);
            var tail = head - dir * (len * d);
            brush.Color = Gfx.C4(Color, op * d);
            dc.DrawLine(tail, head, brush, th * d);
        }
    }
}
