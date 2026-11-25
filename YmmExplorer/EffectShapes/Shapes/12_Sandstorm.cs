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
//  砂嵐・砂煙
//   横なぐりの砂粒と、画面を覆う砂のもや。強い風の表現
// =====================================================================
public sealed class SandstormPlugin : IShapePlugin
{
    public string Name => "演出：砂嵐・砂煙";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SandstormParameter(sharedData);
}

public sealed class SandstormParameter : EffectShapeParameterBase
{
    public SandstormParameter() : this(null) { }
    public SandstormParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "砂", Name = "砂の色")]
    [ColorPicker]
    public WpfColor SandColor { get => sand; set => Set(ref sand, value); }
    WpfColor sand = WpfColor.FromRgb(0xD8, 0xB0, 0x70);

    [Display(GroupName = "砂", Name = "砂粒の数")]
    [AnimationSlider("F0", "個", 0, 1500)]
    public Animation Count { get; } = new Animation(600, 0, 20000);

    [Display(GroupName = "砂", Name = "砂粒の大きさ")]
    [AnimationSlider("F1", "px", 0.5, 8)]
    public Animation Size { get; } = new Animation(1.8, 0.2, 100);

    [Display(GroupName = "風", Name = "風の強さ", Description = "プラスで右へ、マイナスで左へ")]
    [AnimationSlider("F0", "px/s", -3000, 3000)]
    public Animation Wind { get; } = new Animation(1400, -30000, 30000);

    [Display(GroupName = "風", Name = "突風の強弱")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Gust { get; } = new Animation(50, 0, 100);

    [Display(GroupName = "風", Name = "筋の長さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Streak { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "砂煙", Name = "砂煙の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Haze { get; } = new Animation(55, 0, 100);

    [Display(GroupName = "砂煙", Name = "画面全体の黄ばみ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Tint { get; } = new Animation(25, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Wind, Gust, Streak, Haze, Tint];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), wind = c.F(Wind), gust = c.F(Gust) / 100f, streak = c.F(Streak) / 100f;
        float haze = c.F(Haze) / 100f, tint = c.F(Tint) / 100f;
        float margin = 100f, spanX = w + margin * 2f, spanY = h + margin * 2f;
        var dark = Gfx.Mix(SandColor, WpfColor.FromRgb(0x40, 0x28, 0x10), 0.45f);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 全体の黄ばみ ----
        if (tint > 0f)
        {
            brush.Color = Gfx.C4(SandColor, 0.22f * tint);
            Gfx.Rect(dc, brush, 0, 0, w, h);
        }

        // ---- 砂煙（大きな渦を巻く塊）----
        if (haze > 0f)
        {
            float dirSign = wind >= 0f ? 1f : -1f;
            var rv = new Random(Seed + 4);
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    float x0 = rv.NextF() * spanX, y = rv.NextF() * h;
                    float r = 160f + 260f * rv.NextF(), sp = 0.4f + 0.9f * rv.NextF(), ph = rv.NextF() * MathF.PI * 2f;
                    float gk = 1f + gust * 0.7f * MathF.Sin(t * 0.8f + i);
                    float x = Gfx.Mod(x0 + t * MathF.Abs(wind) * 0.4f * sp * dirSign * gk, spanX) - margin;
                    var ctr = new Vector2(x, y + MathF.Sin(t * 0.7f + ph) * 40f);
                    float a = haze * 0.26f * (0.7f + 0.3f * MathF.Sin(t * 0.5f + ph));
                    var col = Gfx.Mix(SandColor, dark, rv.NextF() * 0.6f);
                    dc.Transform = Matrix3x2.CreateScale(2.4f, 0.75f, ctr);
                    using var b = Gfx.Radial(dc, ctr, r, (0f, Gfx.C4(col, a)), (0.6f, Gfx.C4(col, a * 0.5f)), (1f, Gfx.C4(col, 0f)));
                    dc.FillEllipse(new Ellipse(ctr, r, r), b);
                }
            }
            finally
            {
                dc.Transform = Matrix3x2.Identity;
            }
        }

        // ---- 砂粒（風に流れる）----
        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float x0 = rng.NextF() * spanX, y0 = rng.NextF() * spanY;
            float d = 0.35f + 0.65f * rng.NextF(), ph = rng.NextF() * MathF.PI * 2f;
            float gk = 1f + gust * 0.5f * MathF.Sin(t * 0.9f + y0 * 0.004f);
            float vx = wind * d * gk, vy = 60f * d * MathF.Sin(ph);
            float x = Gfx.Mod(x0 + t * vx, spanX) - margin;
            float y = Gfx.Mod(y0 + t * vy + MathF.Sin(t * 1.6f + ph) * 24f * d, spanY) - margin;
            float len = MathF.Min(100f, MathF.Abs(vx) * 0.03f * streak) + 1f;
            var col = Gfx.Mix(SandColor, rng.NextF() < 0.4f ? dark : WpfColor.FromRgb(255, 240, 210), rng.NextF() * 0.5f);
            brush.Color = Gfx.C4(col, 0.35f + 0.5f * d);
            dc.DrawLine(new Vector2(x - MathF.Sign(vx) * len, y), new Vector2(x, y), brush, MathF.Max(0.6f, size * d));
        }
    }
}

public enum LeafKind
{
    [Display(Name = "もみじ")] Maple,
    [Display(Name = "広葉")] Broad,
    [Display(Name = "いちょう")] Ginkgo,
    [Display(Name = "ミックス")] Mixed,
}
