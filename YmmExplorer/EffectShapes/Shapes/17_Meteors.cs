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
//  流星群
//   次々に流れる流星。放射点から降る「流星群」の見え方にもできる
// =====================================================================
public sealed class MeteorShowerPlugin : IShapePlugin
{
    public string Name => "演出：流星群";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MeteorShowerParameter(sharedData);
}

public sealed class MeteorShowerParameter : EffectShapeParameterBase
{
    public MeteorShowerParameter() : this(null) { }
    public MeteorShowerParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "流星", Name = "頭の色")]
    [ColorPicker]
    public WpfColor HeadColor { get => head; set => Set(ref head, value); }
    WpfColor head = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "流星", Name = "尾の色")]
    [ColorPicker]
    public WpfColor TailColor { get => tail; set => Set(ref tail, value); }
    WpfColor tail = WpfColor.FromRgb(0x9B, 0xC4, 0xFF);

    [Display(GroupName = "流星", Name = "頻度")]
    [AnimationSlider("F1", "個/秒", 0.1, 8)]
    public Animation Rate { get; } = new Animation(1.5, 0.05, 60);

    [Display(GroupName = "流星", Name = "尾の長さ")]
    [AnimationSlider("F0", "px", 50, 900)]
    public Animation Length { get; } = new Animation(380, 5, 8000);

    [Display(GroupName = "流星", Name = "速さ")]
    [AnimationSlider("F0", "px/s", 300, 4000)]
    public Animation Speed { get; } = new Animation(1700, 10, 30000);

    [Display(GroupName = "流星", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "向き", Name = "放射点から降らせる")]
    [ToggleSlider]
    public bool UseRadiant { get => useRadiant; set => Set(ref useRadiant, value); }
    bool useRadiant = false;

    [Display(GroupName = "向き", Name = "放射点X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RadiantX { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "向き", Name = "放射点Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RadiantY { get; } = new Animation(-5, -100, 200);

    [Display(GroupName = "向き", Name = "進む向き", Description = "放射点を使わないとき。0=右、90=下")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(35, -3600, 3600);

    [Display(GroupName = "向き", Name = "向きのばらつき")]
    [AnimationSlider("F0", "°", 0, 60)]
    public Animation AngleJitter { get; } = new Animation(8, 0, 360);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Rate, Length, Speed, Brightness, RadiantX, RadiantY, Angle, AngleJitter];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float rate = MathF.Max(0.05f, c.F(Rate));
        float len = c.F(Length), speed = c.F(Speed), bright = c.F(Brightness) / 100f;
        float baseAng = Gfx.Rad(c.F(Angle)), jitter = Gfx.Rad(c.F(AngleJitter));
        var radiant = new Vector2(c.F(RadiantX) / 100f * w, c.F(RadiantY) / 100f * h);
        const float life = 1.1f;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // 時刻 t から見て、まだ流れている流星の発生番号の範囲
            int sMax = (int)MathF.Floor(t * rate);
            int sMin = (int)MathF.Floor((t - life) * rate);
            for (int s = sMin; s <= sMax; s++)
            {
                var rng = new Random(Seed * 7919 + s * 131);
                float birth = (s + rng.NextF() * 0.9f) / rate;
                float age = t - birth;
                if (age < 0f || age >= life) continue;

                float u = age / life;
                var start = new Vector2(w * (-0.1f + 1.2f * rng.NextF()), h * (-0.1f + 0.7f * rng.NextF()));
                float ang;
                if (UseRadiant)
                {
                    var dv = start - radiant;
                    ang = MathF.Atan2(dv.Y, dv.X) + (rng.NextF() - 0.5f) * jitter * 0.5f;
                }
                else
                {
                    ang = baseAng + (rng.NextF() - 0.5f) * 2f * jitter;
                }
                var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                float spd = speed * (0.7f + 0.6f * rng.NextF());
                float ln = len * (0.6f + 0.8f * rng.NextF());
                float b = bright * (0.5f + 0.5f * rng.NextF());

                var headPos = start + dir * (spd * age);
                float env = MathF.Sin(u * MathF.PI);

                Gfx.FadeLine(dc, brush, headPos, headPos - dir * ln, 5f, 0.6f, TailColor, 0.55f * env * b, 18, 1.3f);
                Gfx.FadeLine(dc, brush, headPos, headPos - dir * (ln * 0.55f), 2.4f, 0.5f, HeadColor, 0.9f * env * b, 14, 1.5f);
                brush.Color = Gfx.C4(TailColor, 0.25f * env * b);
                dc.FillEllipse(new Ellipse(headPos, 9f, 9f), brush);
                brush.Color = Gfx.C4(HeadColor, 0.95f * env * b);
                dc.FillEllipse(new Ellipse(headPos, 2.6f, 2.6f), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
