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
//  ワープ
//   中心から星が尾を引いて流れ出す超光速表現。「ワープの強さ」で点から線へ変わる
// =====================================================================
public sealed class WarpPlugin : IShapePlugin
{
    public string Name => "演出：ワープ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WarpParameter(sharedData);
}

public sealed class WarpParameter : EffectShapeParameterBase
{
    public WarpParameter() : this(null) { }
    public WarpParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "星の色")]
    [ColorPicker]
    public WpfColor StarColor { get => star; set => Set(ref star, value); }
    WpfColor star = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "色", Name = "尾の色")]
    [ColorPicker]
    public WpfColor TailColor { get => tail; set => Set(ref tail, value); }
    WpfColor tail = WpfColor.FromRgb(0x8F, 0xB8, 0xFF);

    [Display(GroupName = "位置", Name = "中心X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CenterX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "中心Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CenterY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ワープ", Name = "ワープの強さ", Description = "0で点の星、100で長い尾。キーフレームで加速を表現できる")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Intensity { get; } = new Animation(80, 0, 100);

    [Display(GroupName = "ワープ", Name = "速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "ワープ", Name = "星の数")]
    [AnimationSlider("F0", "個", 20, 800)]
    public Animation Count { get; } = new Animation(320, 1, 5000);

    [Display(GroupName = "ワープ", Name = "中心の光")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation CenterGlow { get; } = new Animation(80, 0, 500);

    [Display(GroupName = "ワープ", Name = "トンネルの輪を表示する")]
    [ToggleSlider]
    public bool Rings { get => rings; set => Set(ref rings, value); }
    bool rings = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [CenterX, CenterY, Intensity, Speed, Count, CenterGlow];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(CenterX) / 100f * w, c.F(CenterY) / 100f * h);
        float I = c.F(Intensity) / 100f, spd = c.F(Speed) / 100f, glow = c.F(CenterGlow) / 100f;
        int n = (int)c.F(Count);
        float maxR = MathF.Sqrt(w * w + h * h) * 0.75f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // 中心の光
            float gr = maxR * 0.28f;
            using (var gb = Gfx.Radial(dc, ctr, gr,
                (0f, Gfx.C4(white, 0.55f * glow * (0.3f + 0.7f * I))), (0.3f, Gfx.C4(TailColor, 0.22f * glow * I)), (1f, Gfx.C4(TailColor, 0f))))
                dc.FillEllipse(new Ellipse(ctr, gr, gr), gb);

            // トンネルの輪（外へ広がる）
            if (Rings)
            {
                for (int k = 0; k < 6; k++)
                {
                    float u = Gfx.Mod(t * 0.35f * spd + k / 6f, 1f);
                    float r = maxR * u * u;
                    brush.Color = Gfx.C4(TailColor, 0.35f * I * MathF.Sin(u * MathF.PI));
                    dc.DrawEllipse(new Ellipse(ctr, r, r), brush, 1f + 3f * u);
                }
            }

            // 星（外へ加速しながら、尾を引く）
            var rng = new Random(Seed);
            for (int i = 0; i < n; i++)
            {
                float ang = rng.NextF() * MathF.PI * 2f, ph = rng.NextF();
                float sp = 0.6f + 0.8f * rng.NextF(), cm = rng.NextF(), wd = 0.5f + 0.5f * rng.NextF();
                float u = Gfx.Mod(t * spd * sp * 0.5f + ph, 1f);
                float e = u * u;
                var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                float r = maxR * e;
                float len = maxR * 0.45f * I * e + 2f;
                float r0 = MathF.Max(0f, r - len);
                var col = Gfx.Mix(TailColor, StarColor, cm);
                float alpha = MathF.Pow(u, 0.7f) * (0.35f + 0.65f * MathF.Min(1f, u * 4f));
                Gfx.FadeLine(dc, brush, ctr + dir * r, ctr + dir * r0, (0.6f + 3.2f * u) * wd, 0.4f, col, alpha, 6, 1.2f);
                brush.Color = Gfx.C4(StarColor, alpha);
                dc.FillEllipse(new Ellipse(ctr + dir * r, (0.7f + 1.6f * u) * wd, (0.7f + 1.6f * u) * wd), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
