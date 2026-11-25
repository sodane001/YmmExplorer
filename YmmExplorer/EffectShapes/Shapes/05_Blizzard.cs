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
//  吹雪
//   強い風に流される雪の筋 + 風の強弱 + 視界を白くする霧
// =====================================================================
public sealed class BlizzardPlugin : IShapePlugin
{
    public string Name => "演出：吹雪";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BlizzardParameter(sharedData);
}

public sealed class BlizzardParameter : EffectShapeParameterBase
{
    public BlizzardParameter() : this(null) { }
    public BlizzardParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "吹雪", Name = "色")]
    [ColorPicker]
    public WpfColor SnowColor { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xF4, 0xF8, 0xFF);

    [Display(GroupName = "吹雪", Name = "雪の数")]
    [AnimationSlider("F0", "個", 0, 1000)]
    public Animation Count { get; } = new Animation(400, 0, 10000);

    [Display(GroupName = "吹雪", Name = "雪の大きさ")]
    [AnimationSlider("F1", "px", 0.5, 8)]
    public Animation Size { get; } = new Animation(2.4, 0.2, 100);

    [Display(GroupName = "風", Name = "落ちる速さ")]
    [AnimationSlider("F0", "px/s", 0, 1200)]
    public Animation Fall { get; } = new Animation(280, 0, 10000);

    [Display(GroupName = "風", Name = "横風の強さ", Description = "プラスで右へ、マイナスで左へ吹く")]
    [AnimationSlider("F0", "px/s", -2500, 2500)]
    public Animation Wind { get; } = new Animation(900, -20000, 20000);

    [Display(GroupName = "風", Name = "突風の強弱")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Gust { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "風", Name = "筋の長さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Streak { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "視界", Name = "白い霧の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Veil { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "視界", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(95, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Fall, Wind, Gust, Streak, Veil, Opacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), fall = c.F(Fall), wind = c.F(Wind), gust = c.F(Gust) / 100f;
        float streak = c.F(Streak) / 100f, veil = c.F(Veil) / 100f, op = c.F(Opacity) / 100f;
        float margin = 80f, spanX = w + margin * 2f, spanY = h + margin * 2f;

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 視界を白くする霧（風の向きに流れる、横に長い塊）----
        if (veil > 0f)
        {
            var rv = new Random(Seed + 4);
            float dirSign = wind >= 0f ? 1f : -1f;
            try
            {
                for (int i = 0; i < 16; i++)
                {
                    float x0 = rv.NextF() * spanX, y = rv.NextF() * h;
                    float r = 180f + 220f * rv.NextF();
                    float sp = 0.6f + 0.8f * rv.NextF();
                    float gustK = 1f + gust * 0.6f * MathF.Sin(t * 0.7f + i);
                    float x = Gfx.Mod(x0 + t * MathF.Abs(wind) * 0.35f * sp * dirSign * gustK, spanX) - margin;
                    float a = veil * 0.22f * (0.7f + 0.3f * MathF.Sin(t * 0.5f + i * 2f)) * op;
                    var ctr = new Vector2(x, y);
                    dc.Transform = Matrix3x2.CreateScale(2.2f, 0.7f, ctr);
                    using var b = Gfx.Radial(dc, ctr, r, (0f, Gfx.C4(SnowColor, a)), (1f, Gfx.C4(SnowColor, 0f)));
                    dc.FillEllipse(new Ellipse(ctr, r, r), b);
                }
            }
            finally
            {
                dc.Transform = Matrix3x2.Identity;
            }
        }

        // ---- 吹き付ける雪の筋 ----
        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float x0 = rng.NextF() * spanX, y0 = rng.NextF() * spanY;
            float d = 0.35f + 0.65f * rng.NextF();                   // 奥行き（1が手前）
            float ph = rng.NextF() * MathF.PI * 2f;
            float sz = size * (0.5f + rng.NextF()) * d;

            float gustK = 1f + gust * 0.5f * MathF.Sin(t * 0.8f + y0 * 0.004f);   // 場所と時間で風の強さが変わる
            float vx = wind * d * gustK;
            float vy = fall * d;
            float x = Gfx.Mod(x0 + t * vx + MathF.Sin(t * 1.3f + ph) * 40f * d, spanX) - margin;
            float y = Gfx.Mod(y0 + t * vy + MathF.Sin(t * 0.9f + ph * 2f) * 20f * d, spanY) - margin;

            var v = new Vector2(vx, vy);
            float len = v.Length();
            var dirv = len > 1f ? v / len : new Vector2(1, 0);
            float tail = MathF.Min(120f, len * 0.05f * streak) + sz;
            var head = new Vector2(x, y);

            brush.Color = Gfx.C4(SnowColor, (0.35f + 0.55f * d) * op);
            dc.DrawLine(head - dirv * tail, head, brush, MathF.Max(0.6f, sz));
            if (d > 0.8f)
            {
                brush.Color = Gfx.C4(SnowColor, 0.25f * op);
                dc.FillEllipse(new Ellipse(head, sz * 1.6f, sz * 1.6f), brush);
            }
        }
    }
}
