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
//  花火
//   打ち上げ → 開花 → 余韻。牡丹 / 菊 / 柳 / ランダム
// =====================================================================
public sealed class FireworksPlugin : IShapePlugin
{
    public string Name => "演出：花火";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FireworksParameter(sharedData);
}

public sealed class FireworksParameter : EffectShapeParameterBase
{
    public FireworksParameter() : this(null) { }
    public FireworksParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "花火", Name = "基本色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x8A, 0x3C);

    [Display(GroupName = "花火", Name = "色のばらつき", Description = "100%で毎回ランダムな色相")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation HueVariation { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "花火", Name = "タイプ", Description = "0=牡丹 1=菊 2=柳 3=ランダム")]
    [TextBoxSlider("F0", "", 0, 3)]
    [Range(0, 3)]
    public int FireworkType { get => ftype; set => Set(ref ftype, value); }
    int ftype = 3;

    [Display(GroupName = "花火", Name = "同時に上がる数")]
    [AnimationSlider("F0", "発", 1, 6)]
    public Animation BurstCount { get; } = new Animation(3, 1, 20);

    [Display(GroupName = "花火", Name = "打ち上げ間隔")]
    [AnimationSlider("F1", "秒", 1, 10)]
    public Animation Interval { get; } = new Animation(4, 0.5, 60);

    [Display(GroupName = "花火", Name = "大きさ")]
    [AnimationSlider("F0", "px", 100, 700)]
    public Animation Size { get; } = new Animation(320, 10, 5000);

    [Display(GroupName = "花火", Name = "粒の数")]
    [AnimationSlider("F0", "個", 20, 400)]
    public Animation ParticleCount { get; } = new Animation(140, 1, 3000);

    [Display(GroupName = "花火", Name = "持続時間")]
    [AnimationSlider("F1", "秒", 1, 5)]
    public Animation Life { get; } = new Animation(2.4, 0.5, 20);

    [Display(GroupName = "花火", Name = "重力")]
    [AnimationSlider("F0", "px/s²", 0, 300)]
    public Animation Gravity { get; } = new Animation(90, 0, 3000);

    [Display(GroupName = "花火", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [HueVariation, BurstCount, Interval, Size, ParticleCount, Life, Gravity, Brightness];

    static Vector2 FwPos(Vector2 center, float vx, float vy, float dist, float g, float tau)
        => new(center.X + vx * dist, center.Y + vy * dist + 0.5f * g * tau * tau);

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int nb = Math.Max(1, (int)c.F(BurstCount));
        float P = MathF.Max(0.5f, c.F(Interval));
        float size = c.F(Size), life = MathF.Max(0.5f, c.F(Life)), g = c.F(Gravity);
        int np = (int)c.F(ParticleCount);
        float bright = c.F(Brightness) / 100f;
        float hueVar = c.F(HueVariation) / 100f;
        float baseHue = Gfx.Hue(Color);
        const float launch = 0.9f;
        var white = WpfColor.FromRgb(255, 255, 255);
        var rocketColor = WpfColor.FromRgb(255, 210, 140);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // 打ち上げ間隔より花火の寿命が長くても、前の花火を消さずに重ねて描く
            int kmax = Math.Min(8, (int)MathF.Ceiling((launch + life) / P));
            for (int b = 0; b < nb; b++)
            for (int k0 = 0; k0 <= kmax; k0++)
            {
                float tt = t + b * P / nb;
                int cyc = (int)MathF.Floor(tt / P) - k0;
                float local = tt - cyc * P;
                if (local < 0f || local >= launch + life) continue;

                var rng = new Random(Seed * 7919 + b * 104729 + cyc * 31);
                float x0 = w * (0.15f + 0.7f * rng.NextF());
                float ty = h * (0.15f + 0.35f * rng.NextF());
                float hue = baseHue + (rng.NextF() - 0.5f) * 360f * hueVar;
                int rtype = rng.Next(0, 3);
                int type = (FireworkType >= 0 && FireworkType <= 2) ? FireworkType : rtype;

                if (local < launch)
                {
                    // 打ち上げ（ゆっくり減速しながら上昇）
                    float u = local / launch;
                    float e = 1f - (1f - u) * (1f - u);
                    var head = new Vector2(x0, h - (h - ty) * e);
                    Gfx.FadeLine(dc, brush, head, head + new Vector2(0, 130f), 3f, 1f, rocketColor, bright * 0.8f, 10, 1.5f);
                    brush.Color = Gfx.C4(rocketColor, bright);
                    dc.FillEllipse(new Ellipse(head, 3f, 3f), brush);
                    continue;
                }

                float te = local - launch;
                if (te >= life) continue;
                var center = new Vector2(x0, ty);

                // 開花の閃光
                if (te < 0.3f)
                {
                    float fa = 1f - te / 0.3f;
                    float fr = size * 0.55f;
                    using var fb = Gfx.Radial(dc, center, fr,
                        (0f, Gfx.C4(white, 0.8f * fa * bright)),
                        (0.4f, Gfx.C4(Gfx.Hsv(hue, 0.4f, 1f), 0.35f * fa * bright)),
                        (1f, Gfx.C4(Gfx.Hsv(hue, 0.6f, 1f), 0f)));
                    dc.FillEllipse(new Ellipse(center, fr, fr), fb);
                }

                var col = Gfx.Hsv(hue, 0.65f, 1f);
                if (type == 2) col = Gfx.Mix(WpfColor.FromRgb(255, 190, 80), col, 0.25f);   // 柳は金色寄り
                float k = type == 1 ? 2.2f : 2.8f;
                float gEff = type == 2 ? g * 2.2f : g;
                float tl = type == 0 ? 0.05f : (type == 1 ? 0.14f : 0.22f);
                int segs = type == 0 ? 1 : 4;

                var rp = new Random(Seed * 131 + b * 17 + cyc * 7);
                for (int i = 0; i < np; i++)
                {
                    // 球面上の一様な方向（画面に投影）
                    float z = rp.NextF() * 2f - 1f;
                    float an = rp.NextF() * MathF.PI * 2f;
                    float rxy = MathF.Sqrt(MathF.Max(0f, 1f - z * z));
                    float vx = rxy * MathF.Cos(an), vy = rxy * MathF.Sin(an);
                    float spd = 0.75f + 0.25f * rp.NextF();
                    float ph = rp.NextF() * MathF.PI * 2f;
                    float lifeP = life * (type == 2 ? 1f : (0.75f + 0.25f * rp.NextF()));
                    if (te >= lifeP) continue;

                    float f = te / lifeP;
                    float alpha = MathF.Pow(1f - f, type == 2 ? 0.9f : 1.4f)
                                  * (0.75f + 0.25f * MathF.Sin(te * 25f + ph)) * bright;

                    for (int s = 0; s < segs; s++)
                    {
                        float ta = MathF.Max(0f, te - tl * s / segs);
                        float tb = MathF.Max(0f, te - tl * (s + 1) / segs);
                        float da = size * spd * (1f - MathF.Exp(-ta * k));
                        float db = size * spd * (1f - MathF.Exp(-tb * k));
                        float fade = 1f - s / (float)segs;
                        brush.Color = Gfx.C4(col, alpha * fade);
                        dc.DrawLine(FwPos(center, vx, vy, db, gEff, tb), FwPos(center, vx, vy, da, gEff, ta), brush, 1.8f * (1f - 0.5f * s / segs));
                    }
                    float dist = size * spd * (1f - MathF.Exp(-te * k));
                    brush.Color = Gfx.C4(Gfx.Mix(col, white, 0.5f), alpha);
                    dc.FillEllipse(new Ellipse(FwPos(center, vx, vy, dist, gEff, te), 1.7f, 1.7f), brush);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
