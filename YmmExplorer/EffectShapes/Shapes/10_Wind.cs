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
//  風（気流）
//   うねる流線 + 渦 + 舞う塵や葉
// =====================================================================
public sealed class WindPlugin : IShapePlugin
{
    public string Name => "演出：風（気流）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WindParameter(sharedData);
}

public sealed class WindParameter : EffectShapeParameterBase
{
    public WindParameter() : this(null) { }
    public WindParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "風", Name = "色")]
    [ColorPicker]
    public WpfColor WindColor { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "風", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "風", Name = "向き", Description = "0=右へ、90=下へ、180=左へ")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Direction { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "風", Name = "速度")]
    [AnimationSlider("F0", "px/s", 50, 3000)]
    public Animation Speed { get; } = new Animation(700, 1, 20000);

    [Display(GroupName = "流線", Name = "本数")]
    [AnimationSlider("F0", "本", 0, 150)]
    public Animation Count { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "流線", Name = "長さ")]
    [AnimationSlider("F0", "px", 50, 800)]
    public Animation Length { get; } = new Animation(260, 10, 5000);

    [Display(GroupName = "流線", Name = "太さ")]
    [AnimationSlider("F1", "px", 0.5, 8)]
    public Animation Thickness { get; } = new Animation(2.2, 0.2, 100);

    [Display(GroupName = "流線", Name = "うねりの幅")]
    [AnimationSlider("F0", "px", 0, 150)]
    public Animation WaveAmp { get; } = new Animation(40, 0, 1000);

    [Display(GroupName = "流線", Name = "うねりの周期")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation WavePeriod { get; } = new Animation(420, 20, 10000);

    [Display(GroupName = "流線", Name = "乱れ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Turbulence { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "渦", Name = "渦の数")]
    [AnimationSlider("F0", "個", 0, 20)]
    public Animation VortexCount { get; } = new Animation(4, 0, 200);

    [Display(GroupName = "渦", Name = "渦の大きさ")]
    [AnimationSlider("F0", "px", 30, 400)]
    public Animation VortexSize { get; } = new Animation(110, 5, 3000);

    [Display(GroupName = "舞うもの", Name = "塵や葉の数")]
    [AnimationSlider("F0", "個", 0, 300)]
    public Animation ParticleCount { get; } = new Animation(60, 0, 3000);

    [Display(GroupName = "舞うもの", Name = "葉の色")]
    [ColorPicker]
    public WpfColor LeafColor { get => leaf; set => Set(ref leaf, value); }
    WpfColor leaf = WpfColor.FromRgb(0xA8, 0xB0, 0x70);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Strength, Direction, Speed, Count, Length, Thickness, WaveAmp, WavePeriod, Turbulence,
         VortexCount, VortexSize, ParticleCount];

    static Vector2 Pt(Vector2 center, Vector2 dir, Vector2 perp, float u, float v) => center + dir * u + perp * v;

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float str = c.F(Strength) / 100f, speed = c.F(Speed);
        float ang = Gfx.Rad(c.F(Direction));
        var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
        var perp = new Vector2(-dir.Y, dir.X);
        var center = new Vector2(w / 2f, h / 2f);
        float diag = MathF.Sqrt(w * w + h * h);
        float len = c.F(Length), thick = c.F(Thickness);
        float amp = c.F(WaveAmp), period = MathF.Max(20f, c.F(WavePeriod)), turb = c.F(Turbulence) / 100f;
        float span = diag + len * 2f;
        var leafCol = LeafColor;

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 1. 流線（うねりながら流れる筋）----
        int n = (int)c.F(Count);
        const int S = 14;
        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float v0 = (rng.NextF() * 2f - 1f) * diag / 2f;
            float u0 = rng.NextF() * span;
            float d = 0.6f + 0.8f * rng.NextF();
            float ph = rng.NextF() * MathF.PI * 2f;
            float ln = len * (0.5f + rng.NextF());
            float th = thick * (0.5f + 0.8f * rng.NextF());
            float wa = amp * (0.5f + rng.NextF());
            float a = str * (0.4f + 0.6f * rng.NextF());

            float uh = Gfx.Mod(u0 + t * speed * d, span) - span / 2f;
            for (int s = 0; s < S; s++)
            {
                float ua = uh - ln * s / S, ub = uh - ln * (s + 1) / S;
                float va = v0 + MathF.Sin(ua / period * MathF.PI * 2f + ph + t * 0.6f) * wa
                              + MathF.Sin(ua * 0.0123f + ph * 2f + t * 1.1f) * wa * 0.4f * turb;
                float vb = v0 + MathF.Sin(ub / period * MathF.PI * 2f + ph + t * 0.6f) * wa
                              + MathF.Sin(ub * 0.0123f + ph * 2f + t * 1.1f) * wa * 0.4f * turb;
                brush.Color = Gfx.C4(WindColor, a * MathF.Pow(1f - s / (float)S, 1.2f));
                dc.DrawLine(Pt(center, dir, perp, ua, va), Pt(center, dir, perp, ub, vb), brush, th * (1f - 0.5f * s / S));
            }
        }

        // ---- 2. 渦（現れて、回りながら流され、消える）----
        int vn = (int)c.F(VortexCount);
        float vr = c.F(VortexSize);
        const float vlife = 3.2f;
        const int K = 30;
        for (int i = 0; i < vn; i++)
        {
            float off = (i * 0.37f) % 1f;
            float tt = t / vlife + off;
            int cyc = (int)MathF.Floor(tt);
            float phase = tt - cyc;
            float alpha = MathF.Sin(phase * MathF.PI) * str;
            if (alpha <= 0.01f) continue;

            var rv = new Random(Seed * 977 + i * 131 + cyc * 17);
            float pu = (rv.NextF() * 2f - 1f) * diag * 0.45f;
            float pv = (rv.NextF() * 2f - 1f) * diag * 0.35f;
            float size = vr * (0.7f + 0.6f * rv.NextF());
            float dirSign = rv.NextF() < 0.5f ? -1f : 1f;
            var vc = Pt(center, dir, perp, pu + speed * 0.4f * phase * vlife, pv);

            for (int arm = 0; arm < 2; arm++)
            {
                float armOff = arm * MathF.PI;
                Vector2 prev = vc;
                for (int k = 1; k <= K; k++)
                {
                    float f = k / (float)K;
                    float a2 = armOff + f * 5.2f * dirSign + t * 3f * dirSign;
                    float r = size * f * (0.6f + 0.4f * MathF.Sin(phase * MathF.PI));
                    var p = vc + new Vector2(MathF.Cos(a2), MathF.Sin(a2)) * r;
                    brush.Color = Gfx.C4(WindColor, alpha * 0.8f * MathF.Pow(1f - f, 0.8f));
                    dc.DrawLine(prev, p, brush, MathF.Max(0.6f, thick * (1.2f - f)));
                    prev = p;
                }
            }
        }

        // ---- 3. 舞う塵と葉 ----
        int pn = (int)c.F(ParticleCount);
        var rp = new Random(Seed + 5);
        try
        {
            for (int i = 0; i < pn; i++)
            {
                float v0 = (rp.NextF() * 2f - 1f) * diag / 2f;
                float u0 = rp.NextF() * span;
                float d = 0.7f + 0.6f * rp.NextF();
                float ph = rp.NextF() * MathF.PI * 2f;
                float sel = rp.NextF();
                float sz = 1f + 2f * rp.NextF();
                float rs = (rp.NextF() - 0.5f) * 8f;

                float u = Gfx.Mod(u0 + t * speed * 0.9f * d, span) - span / 2f;
                float v = v0 + MathF.Sin(u * 0.01f + ph + t * 2f) * 30f * turb;
                var pos = Pt(center, dir, perp, u, v);
                if (pos.X < -20f || pos.X > w + 20f || pos.Y < -20f || pos.Y > h + 20f) continue;

                if (sel < 0.3f)
                {
                    float rot = ph + t * rs;
                    dc.Transform = Matrix3x2.CreateRotation(rot, pos);
                    brush.Color = Gfx.C4(leafCol, (0.55f + 0.3f * d) * str);
                    dc.FillEllipse(new Ellipse(pos, 4.5f * sz * 0.9f, 1.9f * sz * 0.9f), brush);
                    brush.Color = Gfx.C4(Gfx.Mix(leafCol, WpfColor.FromRgb(60, 50, 20), 0.5f), 0.5f * str);
                    dc.DrawLine(pos - new Vector2(4f * sz * 0.8f, 0f), pos + new Vector2(4f * sz * 0.8f, 0f), brush, 0.7f);
                    dc.Transform = Matrix3x2.Identity;
                }
                else
                {
                    brush.Color = Gfx.C4(WindColor, (0.25f + 0.35f * d) * str);
                    dc.FillEllipse(new Ellipse(pos, sz * 0.7f, sz * 0.7f), brush);
                }
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
