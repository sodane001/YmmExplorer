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
//  星
//   またたく星 + 十字の光 + 流れ星
// =====================================================================
public sealed class StarsPlugin : IShapePlugin
{
    public string Name => "演出：星";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StarsParameter(sharedData);
}

public sealed class StarsParameter : EffectShapeParameterBase
{
    public StarsParameter() : this(null) { }
    public StarsParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "星", Name = "色")]
    [ColorPicker]
    public WpfColor StarColor { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "星", Name = "数")]
    [AnimationSlider("F0", "個", 0, 1000)]
    public Animation Count { get; } = new Animation(250, 0, 20000);

    [Display(GroupName = "星", Name = "最大サイズ")]
    [AnimationSlider("F1", "px", 1, 12)]
    public Animation MaxSize { get; } = new Animation(3.5, 0.5, 100);

    [Display(GroupName = "星", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "星", Name = "色のばらつき", Description = "青白い星・黄色い星が混ざる割合")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ColorVariation { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "星", Name = "またたく速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation TwinkleSpeed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "星", Name = "十字の光の割合")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FlareRatio { get; } = new Animation(15, 0, 100);

    [Display(GroupName = "流れ星", Name = "出現間隔", Description = "0で出現しない")]
    [AnimationSlider("F1", "秒", 0, 20)]
    public Animation ShootingInterval { get; } = new Animation(6, 0, 600);

    [Display(GroupName = "流れ星", Name = "長さ")]
    [AnimationSlider("F0", "px", 100, 800)]
    public Animation ShootingLength { get; } = new Animation(350, 10, 5000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, MaxSize, Brightness, ColorVariation, TwinkleSpeed, FlareRatio, ShootingInterval, ShootingLength];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(MaxSize), bright = c.F(Brightness) / 100f;
        float tv = c.F(ColorVariation) / 100f, spd = c.F(TwinkleSpeed) / 100f, fr = c.F(FlareRatio) / 100f;
        var blue = WpfColor.FromRgb(0x9B, 0xB8, 0xFF);
        var yellow = WpfColor.FromRgb(0xFF, 0xE2, 0xA0);

        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float x = rng.NextF() * w, y = rng.NextF() * h;
            float m = rng.NextF(); m = m * m * m;                    // 暗い星が多く、明るい星は少ない
            float ph = rng.NextF() * MathF.PI * 2f;
            float tspeed = 0.5f + rng.NextF() * 2f;
            float tintSel = rng.NextF(), tintAmt = rng.NextF(), flareSel = rng.NextF();

            float r = size * (0.25f + 0.75f * m);
            float tw = 0.6f + 0.4f * MathF.Sin(t * spd * tspeed * 2f + ph);
            float a = (0.3f + 0.7f * m) * tw * bright;
            var col = Gfx.Mix(StarColor, tintSel < 0.5f ? blue : yellow, tv * tintAmt);
            var ctr = new Vector2(x, y);

            brush.Color = Gfx.C4(col, a * 0.18f);
            dc.FillEllipse(new Ellipse(ctr, r * 2.6f, r * 2.6f), brush);
            brush.Color = Gfx.C4(col, a);
            dc.FillEllipse(new Ellipse(ctr, r, r), brush);

            if (flareSel < fr && m > 0.35f)
            {
                float fl = r * 7f * tw;
                brush.Color = Gfx.C4(col, a * 0.6f);
                dc.DrawLine(new Vector2(x - fl, y), new Vector2(x + fl, y), brush, 1f);
                dc.DrawLine(new Vector2(x, y - fl), new Vector2(x, y + fl), brush, 1f);
            }
        }

        // ---- 流れ星 ----
        float interval = c.F(ShootingInterval);
        if (interval > 0.2f)
        {
            const float dur = 0.9f;
            int idx = (int)MathF.Floor(t / interval);
            float local = t - idx * interval;
            if (local < dur)
            {
                var rs = new Random(Seed * 1009 + idx * 7919);
                float u = local / dur;
                var start = new Vector2(w * (0.1f + 0.8f * rs.NextF()), h * (0.05f + 0.35f * rs.NextF()));
                float ang = Gfx.Rad(15f + 25f * rs.NextF());
                var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                if (rs.NextF() < 0.5f) dir.X = -dir.X;
                float len = c.F(ShootingLength);
                var head = start + dir * (len * 1.6f * u);
                float env = MathF.Sin(u * MathF.PI);
                Gfx.FadeLine(dc, brush, head, head - dir * (len * 0.5f), 3.5f, 0.6f, StarColor, env, 16, 1.4f);
                brush.Color = Gfx.C4(StarColor, env * 0.35f);
                dc.FillEllipse(new Ellipse(head, 7f, 7f), brush);
                brush.Color = Gfx.C4(StarColor, env);
                dc.FillEllipse(new Ellipse(head, 2.5f, 2.5f), brush);
            }
        }
    }
}
