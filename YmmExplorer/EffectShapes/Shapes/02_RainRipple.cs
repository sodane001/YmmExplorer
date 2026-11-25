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
//  雨の波紋
//   水たまりに雨粒が落ちて広がる波紋。遠近のついた楕円と、小さなしぶき
// =====================================================================
public sealed class RainRipplePlugin : IShapePlugin
{
    public string Name => "演出：雨の波紋";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RainRippleParameter(sharedData);
}

public sealed class RainRippleParameter : EffectShapeParameterBase
{
    public RainRippleParameter() : this(null) { }
    public RainRippleParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "波紋", Name = "色")]
    [ColorPicker]
    public WpfColor RippleColor { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xCF, 0xE6, 0xFF);

    [Display(GroupName = "波紋", Name = "落ちる数")]
    [AnimationSlider("F0", "個/秒", 1, 60)]
    public Animation Rate { get; } = new Animation(14, 0.2, 400);

    [Display(GroupName = "波紋", Name = "最大の大きさ（手前）")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation Size { get; } = new Animation(110, 4, 3000);

    [Display(GroupName = "波紋", Name = "水面の始まり位置")]
    [AnimationSlider("F0", "%", 0, 90)]
    public Animation FloorTop { get; } = new Animation(52, -50, 150);

    [Display(GroupName = "波紋", Name = "遠近の平たさ")]
    [AnimationSlider("F0", "%", 10, 100)]
    public Animation Perspective { get; } = new Animation(28, 5, 100);

    [Display(GroupName = "波紋", Name = "輪の数")]
    [TextBoxSlider("F0", "本", 1, 5)]
    [Range(1, 5)]
    public int Rings { get => rings; set => Set(ref rings, value); }
    int rings = 3;

    [Display(GroupName = "雨", Name = "雨の筋を表示する")]
    [ToggleSlider]
    public bool ShowRain { get => showRain; set => Set(ref showRain, value); }
    bool showRain = true;

    [Display(GroupName = "雨", Name = "雨の本数")]
    [AnimationSlider("F0", "本", 0, 500)]
    public Animation RainCount { get; } = new Animation(120, 0, 5000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Rate, Size, FloorTop, Perspective, RainCount];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float rate = MathF.Max(0.2f, c.F(Rate)), size = c.F(Size), top = c.F(FloorTop) / 100f * h, persp = c.F(Perspective) / 100f;
        int rings = Math.Clamp(Rings, 1, 5);
        const float life = 1.7f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);

        int sMax = (int)MathF.Floor(t * rate), sMin = (int)MathF.Floor((t - life) * rate);
        for (int s = sMin; s <= sMax; s++)
        {
            var rng = new Random(Seed * 7919 + s * 131);
            float birth = (s + rng.NextF() * 0.9f) / rate;
            float age = t - birth;
            float px = rng.NextF() * w, py = top + (h - top) * MathF.Sqrt(rng.NextF());
            float rv = rng.NextF();
            if (age < 0f || age >= life) continue;

            float u = age / life;
            float depth = (py - top) / MathF.Max(1f, h - top);              // 0=奥 1=手前
            float scale = 0.35f + 0.9f * depth;
            float radius = size * scale * (1f - (1f - u) * (1f - u)) * (0.7f + 0.5f * rv);
            var ctr = new Vector2(px, py);

            for (int j = 0; j < rings; j++)
            {
                float rj = radius * (1f - 0.2f * j);
                if (rj < 1.5f) continue;
                float a = MathF.Pow(1f - u, 1.4f) * (1f - 0.25f * j);
                float lw = (1f + 2.2f * (1f - u)) * scale;
                brush.Color = Gfx.C4(RippleColor, 0.20f * a);
                dc.DrawEllipse(new Ellipse(ctr, rj, rj * persp), brush, lw * 3f);
                brush.Color = Gfx.C4(white, 0.65f * a);
                dc.DrawEllipse(new Ellipse(ctr, rj, rj * persp), brush, lw);
            }
            // 落ちた瞬間の小さなしぶき
            if (u < 0.22f)
            {
                for (int k = 0; k < 5; k++)
                {
                    float a2 = (k - 2) * 0.5f + (rng.NextF() - 0.5f) * 0.3f, sp = (40f + 80f * rng.NextF()) * scale;
                    float tau = u * life;
                    var dp = ctr + new Vector2(MathF.Sin(a2) * sp * tau, -MathF.Cos(a2) * sp * tau + 0.5f * 700f * tau * tau);
                    brush.Color = Gfx.C4(white, 0.8f * (1f - u / 0.22f));
                    dc.FillEllipse(new Ellipse(dp, 1.6f * scale + 0.4f, 1.6f * scale + 0.4f), brush);
                }
            }
        }

        // ---- 降ってくる雨の筋 ----
        if (ShowRain)
        {
            int nr = (int)c.F(RainCount);
            var rr = new Random(Seed + 3);
            float span = h + 200f;
            for (int i = 0; i < nr; i++)
            {
                float x0 = rr.NextF() * (w + 200f), ph = rr.NextF(), d = 0.5f + 0.5f * rr.NextF();
                float y = -100f + Gfx.Mod(t * 1100f * d + ph * span, span);
                float x = x0 - y * 0.1f;
                brush.Color = Gfx.C4(RippleColor, 0.35f * d);
                dc.DrawLine(new Vector2(x, y), new Vector2(x - 4f, y - 38f * d), brush, 1.2f);
            }
        }
    }
}

public enum FlyKind
{
    [Display(Name = "蝶")] Butterfly,
    [Display(Name = "鳥（V字の編隊）")] BirdsV,
    [Display(Name = "鳥（群れ）")] BirdsFlock,
}
