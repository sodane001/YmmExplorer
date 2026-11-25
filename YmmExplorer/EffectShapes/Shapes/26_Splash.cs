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
//  水しぶき
//   水柱 + 飛び散る水滴（残像つき）+ 霧状のしぶき + 水面の波紋
// =====================================================================
public sealed class SplashPlugin : IShapePlugin
{
    public string Name => "演出：水しぶき";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SplashParameter(sharedData);
}

public sealed class SplashParameter : EffectShapeParameterBase
{
    public SplashParameter() : this(null) { }
    public SplashParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "水面の高さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation WaterY { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "水しぶき", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xBF, 0xE6, 0xFF);

    [Display(GroupName = "水しぶき", Name = "発生間隔", Description = "短くすると連続して噴き上がる")]
    [AnimationSlider("F1", "秒", 1, 10)]
    public Animation Interval { get; } = new Animation(1.6, 0.8, 60);

    [Display(GroupName = "水しぶき", Name = "勢い")]
    [AnimationSlider("F0", "px/s", 300, 2000)]
    public Animation Power { get; } = new Animation(900, 10, 10000);

    [Display(GroupName = "水しぶき", Name = "広がる角度")]
    [AnimationSlider("F0", "°", 5, 90)]
    public Animation Spread { get; } = new Animation(55, 1, 170);

    [Display(GroupName = "水しぶき", Name = "水滴の数")]
    [AnimationSlider("F0", "個", 10, 300)]
    public Animation Count { get; } = new Animation(90, 1, 3000);

    [Display(GroupName = "水しぶき", Name = "水滴の大きさ")]
    [AnimationSlider("F1", "px", 1, 20)]
    public Animation DropSize { get; } = new Animation(5, 0.5, 200);

    [Display(GroupName = "水しぶき", Name = "重力")]
    [AnimationSlider("F0", "px/s²", 500, 4000)]
    public Animation Gravity { get; } = new Animation(1800, 10, 20000);

    [Display(GroupName = "水しぶき", Name = "波紋の広がり")]
    [AnimationSlider("F0", "px", 50, 800)]
    public Animation RippleRadius { get; } = new Animation(300, 1, 5000);

    [Display(GroupName = "水しぶき", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, WaterY, Interval, Power, Spread, Count, DropSize, Gravity, RippleRadius, Opacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float P = MathF.Max(0.8f, c.F(Interval));
        float power = c.F(Power), g = MathF.Max(10f, c.F(Gravity));
        float spreadRad = Gfx.Rad(c.F(Spread));
        int n = (int)c.F(Count);
        float dsize = c.F(DropSize), op = c.F(Opacity) / 100f;
        float rippleR = c.F(RippleRadius);
        float y0 = c.F(WaterY) / 100f * h;
        var white = WpfColor.FromRgb(255, 255, 255);
        const float rippleLife = 1.8f, totalLife = 2.2f;

        using var brush = dc.CreateSolidColorBrush(default);

        // 直前の発生分も重ねて描く（途中で切れないように）
        for (int k = 0; k < 2; k++)
        {
            int idx = (int)MathF.Floor(t / P) - k;
            float te = t - idx * P;
            if (te < 0f || te >= totalLife) continue;

            var rng = new Random(Seed * 1009 + idx * 7919);
            float x0 = c.F(PosX) / 100f * w + (rng.NextF() - 0.5f) * w * 0.08f;

            // ---- 水面の波紋（遠近で横に平たい楕円）----
            for (int j = 0; j < 2; j++)
            {
                float tj = te - j * 0.18f;
                if (tj <= 0f || tj >= rippleLife) continue;
                float r = tj / rippleLife * rippleR;
                float a = MathF.Pow(1f - tj / rippleLife, 1.5f) * op;
                var rc = new Vector2(x0, y0);
                brush.Color = Gfx.C4(Color, a * 0.25f);
                dc.DrawEllipse(new Ellipse(rc, r, r * 0.28f), brush, 6f);
                brush.Color = Gfx.C4(white, a * 0.55f);
                dc.DrawEllipse(new Ellipse(rc, r, r * 0.28f), brush, 1.8f);
            }

            // ---- 水柱 ----
            const float jetDur = 0.38f;
            if (te < jetDur)
            {
                float v = te / jetDur;
                float hh = power * jetDur * 0.45f * MathF.Sin(v * MathF.PI / 2f);
                float w0 = dsize * 5f * (1f - v * 0.6f);
                Gfx.FadeLine(dc, brush, new Vector2(x0, y0), new Vector2(x0, y0 - hh), w0, w0 * 0.25f,
                    Color, 0.7f * (1f - v * 0.5f) * op, 8, 0.6f);
            }

            // ---- 水滴（残像つき）----
            for (int i = 0; i < n; i++)
            {
                float ang = (rng.NextF() - 0.5f) * 2f * spreadRad;
                float spd = power * (0.45f + 0.65f * MathF.Pow(rng.NextF(), 0.7f));
                float rr = rng.NextF();
                float r = dsize * (0.25f + 0.75f * rr * rr);
                float delay = rng.NextF() * 0.05f;

                float tau = te - delay;
                if (tau <= 0f) continue;
                float vx = MathF.Sin(ang) * spd, vy = -MathF.Cos(ang) * spd;
                float tauEnd = 2f * spd * MathF.Cos(ang) / g;     // 水面に戻る時刻
                if (tau > tauEnd) continue;
                float landFade = MathF.Min(1f, (tauEnd - tau) / 0.05f);

                var p1 = new Vector2(x0 + vx * tau, y0 + vy * tau + 0.5f * g * tau * tau);
                float tp = MathF.Max(0f, tau - 0.04f);
                var p0 = new Vector2(x0 + vx * tp, y0 + vy * tp + 0.5f * g * tp * tp);

                brush.Color = Gfx.C4(Color, 0.5f * landFade * op);
                dc.DrawLine(p0, p1, brush, r * 1.5f);
                brush.Color = Gfx.C4(Color, 0.85f * landFade * op);
                dc.FillEllipse(new Ellipse(p1, r, r * 1.15f), brush);
                brush.Color = Gfx.C4(white, 0.8f * landFade * op);
                dc.FillEllipse(new Ellipse(p1 + new Vector2(-r * 0.3f, -r * 0.35f), r * 0.3f, r * 0.22f), brush);
            }

            // ---- 霧状の細かいしぶき ----
            var rm = new Random(Seed * 37 + idx * 101);
            for (int i = 0; i < n / 2; i++)
            {
                float ang = (rm.NextF() - 0.5f) * 2f * spreadRad * 1.4f;
                float spd = power * 0.7f * (0.3f + 0.7f * rm.NextF());
                float r = 1f + rm.NextF() * 1.5f;
                float tau = te - rm.NextF() * 0.08f;
                if (tau <= 0f) continue;
                float vx = MathF.Sin(ang) * spd, vy = -MathF.Cos(ang) * spd;
                float tauEnd = 2f * spd * MathF.Cos(ang) / g;
                if (tau > tauEnd) continue;
                var p = new Vector2(x0 + vx * tau, y0 + vy * tau + 0.5f * g * tau * tau);
                brush.Color = Gfx.C4(Color, 0.35f * op * (1f - tau / tauEnd));
                dc.FillEllipse(new Ellipse(p, r, r), brush);
            }
        }
    }
}
