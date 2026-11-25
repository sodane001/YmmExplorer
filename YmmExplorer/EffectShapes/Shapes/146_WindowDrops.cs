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
//  水滴が窓を伝う
//   窓ガラスについた小さな水滴と、ときどき、つーっと流れ落ちる大きな水滴。
//   「曇りガラス」と重ねると、雨の日の窓になる
// =====================================================================
public sealed class WindowDropsPlugin : IShapePlugin
{
    public string Name => "演出：水滴が窓を伝う";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WindowDropsParameter(sharedData);
}

public sealed class WindowDropsParameter : EffectShapeParameterBase
{
    public WindowDropsParameter() : this(null) { }
    public WindowDropsParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "水滴", Name = "流れる水滴の数")]
    [TextBoxSlider("F0", "個", 0, 80)]
    [Range(0, 400)]
    public int RunCount { get => runCount; set => Set(ref runCount, value); }
    int runCount = 18;

    [Display(GroupName = "水滴", Name = "止まっている水滴の数")]
    [TextBoxSlider("F0", "個", 0, 800)]
    [Range(0, 3000)]
    public int DotCount { get => dotCount; set => Set(ref dotCount, value); }
    int dotCount = 260;

    [Display(GroupName = "水滴", Name = "水滴の大きさ")]
    [AnimationSlider("F0", "px", 4, 60)]
    public Animation DropSize { get; } = new Animation(16, 1, 500);

    [Display(GroupName = "水滴", Name = "流れる速さ")]
    [AnimationSlider("F0", "%", 10, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "水滴", Name = "流れたあとの筋")]
    [ToggleSlider]
    public bool Trail { get => trail; set => Set(ref trail, value); }
    bool trail = true;

    [Display(GroupName = "色", Name = "水の色")]
    [ColorPicker]
    public WpfColor Tint { get => tint; set => Set(ref tint, value); }
    WpfColor tint = WpfColor.FromRgb(0xDD, 0xEE, 0xFF);

    [Display(GroupName = "色", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(80, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [DropSize, Speed, Opacity];

    static void Droplet(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 p, float rx, float ry, WpfColor tint, float a)
    {
        // 影(下側が暗い)・本体(うすい)・光(左上の白い点)で、レンズのように見せる
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.22f * a);
        dc.FillEllipse(new Ellipse(p + new Vector2(0f, ry * 0.18f), rx, ry), brush);
        brush.Color = Gfx.C4(tint, 0.28f * a);
        dc.FillEllipse(new Ellipse(p, rx, ry), brush);
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.5f * a);
        dc.DrawEllipse(new Ellipse(p, rx * 0.92f, ry * 0.92f), brush, MathF.Max(0.6f, rx * 0.12f));
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.9f * a);
        dc.FillEllipse(new Ellipse(p + new Vector2(-rx * 0.35f, -ry * 0.38f), rx * 0.22f, ry * 0.18f), brush);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float a = c.F(Opacity) / 100f;
        if (a <= 0.002f) return;
        float ds = c.F(DropSize), speed = c.F(Speed) / 100f;
        using var brush = dc.CreateSolidColorBrush(default);

        // 止まっている小さな水滴(流れる水滴が通った後は消える…ところまではしない)
        for (int i = 0; i < DotCount; i++)
        {
            var p = new Vector2(DrawKit.H(Seed, i, 1) * w, DrawKit.H(Seed, i, 2) * h);
            float r = ds * (0.12f + MathF.Pow(DrawKit.H(Seed, i, 3), 3f) * 0.45f);
            Droplet(dc, brush, p, r, r * (0.85f + DrawKit.H(Seed, i, 4) * 0.25f), Tint, a);
        }

        // 流れる水滴: 止まったり進んだりしながら(ぎくしゃくと)下へ
        for (int i = 0; i < RunCount; i++)
        {
            float x0 = DrawKit.H(Seed, i, 11) * w;
            float r = ds * (0.55f + DrawKit.H(Seed, i, 12) * 0.6f);
            float period = (6f + DrawKit.H(Seed, i, 13) * 8f) / MathF.Max(0.05f, speed);
            float local = Gfx.Mod(t + DrawKit.H(Seed, i, 14) * period, period);
            float travel = 0f;
            // 0.25秒ごとに、進むか止まるかを決める
            int steps = (int)(local / 0.25f);
            for (int s = 0; s <= steps; s++)
            {
                float go = DrawKit.H(Seed, i * 97 + s, 15) < 0.55f ? 1f : 0.05f;
                float dt = s < steps ? 0.25f : local - steps * 0.25f;
                travel += go * dt * h * 0.12f * speed * (0.7f + DrawKit.H(Seed, i, 16) * 0.6f);
            }
            float y = -r * 2f + travel;
            if (y > h + r * 3f) continue;
            float x = x0 + MathF.Sin(travel * 0.012f + i) * r * 0.6f;
            if (Trail)
            {
                // 通った後の、細い水の筋
                float y0 = MathF.Max(-r, y - MathF.Min(travel, h * 0.5f));
                var trailPts = new List<Vector2>();
                for (int k = 0; k <= 12; k++)
                {
                    float yy = y0 + (y - y0) * k / 12f;
                    float tt = travel - (y - yy);
                    trailPts.Add(new Vector2(x0 + MathF.Sin(tt * 0.012f + i) * r * 0.6f, yy));
                }
                brush.Color = Gfx.C4(Tint, 0.16f * a);
                Gfx.Polyline(dc, brush, trailPts, r * 0.45f);
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.14f * a);
                Gfx.Polyline(dc, brush, trailPts.Select(q => q + new Vector2(-r * 0.1f, 0f)).ToList(), MathF.Max(0.6f, r * 0.1f));
            }
            // 流れている時は、縦に少しのびる
            Droplet(dc, brush, new Vector2(x, y), r * 0.82f, r * 1.05f, Tint, a);
        }
    }
}
