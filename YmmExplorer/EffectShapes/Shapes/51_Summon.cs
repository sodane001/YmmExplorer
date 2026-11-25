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
//  召喚の光
//   床に描かれる光の輪から、らせん状に光が立ちのぼる
// =====================================================================
public sealed class SummonPlugin : IShapePlugin
{
    public string Name => "演出：召喚の光";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SummonParameter(sharedData);
}

public sealed class SummonParameter : EffectShapeParameterBase
{
    public SummonParameter() : this(null) { }
    public SummonParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "外側の色")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0x7A, 0xD0, 0xFF);

    [Display(GroupName = "色", Name = "内側の色")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);

    [Display(GroupName = "位置と大きさ", Name = "中心X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "床のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "光の輪の半径")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Radius { get; } = new Animation(260, 5, 5000);

    [Display(GroupName = "位置と大きさ", Name = "光が立ちのぼる高さ")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public new Animation Height { get; } = new Animation(620, 10, 8000);

    [Display(GroupName = "動き", Name = "回転の速さ")]
    [AnimationSlider("F0", "%", -300, 300)]
    public Animation Spin { get; } = new Animation(100, -2000, 2000);

    [Display(GroupName = "動き", Name = "光の粒の数")]
    [AnimationSlider("F0", "個", 0, 300)]
    public Animation Particles { get; } = new Animation(90, 0, 3000);

    [Display(GroupName = "動き", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "動き", Name = "開くアニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "開くまでの時間")]
    [AnimationSlider("F1", "秒", 0.2, 6)]
    public Animation IntroTime { get; } = new Animation(1.5, 0.05, 30);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Height, Spin, Particles, Brightness, IntroTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float ap = Intro ? Gfx.Smooth(0f, MathF.Max(0.1f, c.F(IntroTime)), t) : 1f;
        if (ap <= 0.002f) return;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius) * (0.6f + 0.4f * ap), H = c.F(Height) * ap, B = c.F(Brightness) / 100f * ap;
        float spin = c.F(Spin) / 100f * t;
        int np = (int)c.F(Particles);
        const float squash = 0.28f;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // 床の光
            float gr = R * 1.1f;
            using (var gb = Gfx.Radial(dc, ctr, gr, (0f, Gfx.C4(Color1, 0.35f * B)), (0.8f, Gfx.C4(Color1, 0.10f * B)), (1f, Gfx.C4(Color1, 0f))))
                dc.FillEllipse(new Ellipse(ctr, gr, gr * squash), gb);

            // 床に描かれた二重の輪と、回る目盛り
            for (int k = 0; k < 3; k++)
            {
                float rr = R * (1f - 0.18f * k);
                brush.Color = Gfx.C4(k == 0 ? Color1 : Color2, (0.8f - 0.2f * k) * B);
                dc.DrawEllipse(new Ellipse(ctr, rr, rr * squash), brush, k == 0 ? 3f : 1.5f);
            }
            for (int i = 0; i < 36; i++)
            {
                float a = spin + i * MathF.PI * 2f / 36f;
                var d = new Vector2(MathF.Cos(a), MathF.Sin(a) * squash);
                brush.Color = Gfx.C4(Color2, 0.7f * B);
                dc.DrawLine(ctr + d * (R * 0.78f), ctr + d * (R * (i % 3 == 0 ? 0.64f : 0.7f)), brush, i % 3 == 0 ? 2.4f : 1.2f);
            }

            // らせん状に立ちのぼる光の筋（3本）
            for (int k = 0; k < 3; k++)
            {
                var pts = new List<Vector2>();
                for (int i = 0; i <= 60; i++)
                {
                    float f = i / 60f;
                    float a = spin * 1.6f + k * MathF.PI * 2f / 3f + f * 7f;
                    float rr = R * 0.75f * (1f - 0.55f * f);
                    pts.Add(ctr + new Vector2(MathF.Cos(a) * rr, MathF.Sin(a) * rr * squash - f * H));
                }
                brush.Color = Gfx.C4(Color1, 0.16f * B);
                FormKit.Stroke(dc, brush, pts, 14f, false);
                brush.Color = Gfx.C4(Color2, 0.6f * B);
                FormKit.Stroke(dc, brush, pts, 2.2f, false);
            }

            // 中央に伸びる光の筒（縦グラデーション）
            using (var lg = Gfx.Linear(dc, ctr, new Vector2(ctr.X, ctr.Y - H), (0f, Gfx.C4(Color2, 0.55f * B)), (1f, Gfx.C4(Color1, 0f))))
                Gfx.Rect(dc, lg, ctr.X - R * 0.22f, ctr.Y - H, R * 0.44f, H);

            // 立ちのぼる粒
            var rp = new Random(Seed);
            for (int i = 0; i < np; i++)
            {
                float a0 = rp.NextF() * MathF.PI * 2f, rr = R * 0.8f * MathF.Sqrt(rp.NextF()), ph = rp.NextF(), sp = 0.4f + 0.9f * rp.NextF();
                float u = Gfx.Mod(t * 0.3f * sp + ph, 1f);
                float a = a0 + spin + u * 3f;
                var p = ctr + new Vector2(MathF.Cos(a) * rr, MathF.Sin(a) * rr * squash - u * H);
                brush.Color = Gfx.C4(Color2, 0.9f * MathF.Sin(u * MathF.PI) * B);
                dc.FillEllipse(new Ellipse(p, 1.4f + 1.8f * (1f - u), 1.4f + 1.8f * (1f - u)), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
