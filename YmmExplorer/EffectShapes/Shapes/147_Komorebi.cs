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
//  光が差し込む(木漏れ日)
//   斜めに差し込む光の帯と、風でゆらぐ葉の影・光の玉
// =====================================================================
public sealed class KomorebiPlugin : IShapePlugin
{
    public string Name => "演出：光が差し込む（木漏れ日）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new KomorebiParameter(sharedData);
}

public sealed class KomorebiParameter : EffectShapeParameterBase
{
    public KomorebiParameter() : this(null) { }
    public KomorebiParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "光", Name = "光の色")]
    [ColorPicker]
    public WpfColor LightColor { get => lightColor; set => Set(ref lightColor, value); }
    WpfColor lightColor = WpfColor.FromRgb(0xFF, 0xE6, 0xA8);

    [Display(GroupName = "光", Name = "光の強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Intensity { get; } = new Animation(55, 0, 200);

    [Display(GroupName = "光", Name = "差し込む向き", Description = "0°=真下。プラスで右下へ")]
    [AnimationSlider("F0", "°", -60, 60)]
    public Animation Angle { get; } = new Animation(-25, -89, 89);

    [Display(GroupName = "光", Name = "光の帯の数")]
    [TextBoxSlider("F0", "本", 0, 20)]
    [Range(0, 60)]
    public int RayCount { get => rayCount; set => Set(ref rayCount, value); }
    int rayCount = 7;

    [Display(GroupName = "光", Name = "光源のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SourceX { get; } = new Animation(25, -100, 200);

    [Display(GroupName = "葉の影", Name = "葉の影を出す")]
    [ToggleSlider]
    public bool Leaves { get => leaves; set => Set(ref leaves, value); }
    bool leaves = true;

    [Display(GroupName = "葉の影", Name = "影の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ShadowStrength { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "葉の影", Name = "光の玉の数")]
    [TextBoxSlider("F0", "個", 0, 120)]
    [Range(0, 500)]
    public int SpotCount { get => spotCount; set => Set(ref spotCount, value); }
    int spotCount = 40;

    [Display(GroupName = "葉の影", Name = "ゆれる速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation SwaySpeed { get; } = new Animation(100, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Intensity, Angle, SourceX, ShadowStrength, SwaySpeed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float inten = c.F(Intensity) / 100f, sway = c.F(SwaySpeed) / 100f;
        float ang = Gfx.Rad(c.F(Angle));
        var dir = new Vector2(MathF.Sin(ang), MathF.Cos(ang));
        var nrm = new Vector2(-dir.Y, dir.X);
        float len = MathF.Sqrt(w * w + h * h) * 1.3f;
        var src = new Vector2(c.F(SourceX) / 100f * w, -h * 0.08f);
        using var brush = dc.CreateSolidColorBrush(default);

        // 葉の影: 大きくぼんやりした暗い斑点が、ゆっくり揺れる
        if (Leaves)
        {
            float sh = c.F(ShadowStrength) / 100f;
            for (int i = 0; i < 26; i++)
            {
                var basePt = new Vector2(DrawKit.H(Seed, i, 1) * w, DrawKit.H(Seed, i, 2) * h);
                float r = MathF.Min(w, h) * (0.08f + DrawKit.H(Seed, i, 3) * 0.14f);
                var p = basePt + new Vector2(MathF.Sin(t * 0.7f * sway + i), MathF.Cos(t * 0.5f * sway + i * 1.3f)) * r * 0.15f;
                using var g = Gfx.Radial(dc, p, r, (0f, Gfx.C4(0.02f, 0.05f, 0.02f, sh * 0.6f)), (1f, Gfx.C4(0f, 0f, 0f, 0f)));
                dc.FillEllipse(new Ellipse(p, r, r), g);
            }
        }

        dc.PrimitiveBlend = PrimitiveBlend.Add;
        // 光の帯: 光源から放射状に広がる、ゆらゆら明るさの変わる帯
        for (int i = 0; i < RayCount; i++)
        {
            float off = (i - (RayCount - 1) / 2f) * w * 0.09f + (DrawKit.H(Seed, i, 4) - 0.5f) * w * 0.05f;
            float spreadA = ang + off / w * 0.5f;
            var d = new Vector2(MathF.Sin(spreadA), MathF.Cos(spreadA));
            var n2 = new Vector2(-d.Y, d.X);
            float width0 = w * (0.008f + DrawKit.H(Seed, i, 5) * 0.012f);
            float width1 = width0 * (5f + DrawKit.H(Seed, i, 6) * 6f);
            float flick = 0.55f + 0.45f * MathF.Sin(t * (0.8f + DrawKit.H(Seed, i, 7)) * sway + i * 2.3f);
            var a = src + nrm * off;
            var b = a + d * len;
            var poly = new List<Vector2> { a + n2 * width0, b + n2 * width1, b - n2 * width1, a - n2 * width0 };
            using var g = Gfx.Linear(dc, a, a + d * len * 0.75f, (0f, Gfx.C4(LightColor, 0.32f * inten * flick)), (1f, Gfx.C4(LightColor, 0f)));
            Gfx.FillPoly(dc, g, DrawKit.ClipRect(poly, 0, 0, w, h));
        }
        // 光の玉: 葉のすき間からこぼれる、まるい光
        for (int i = 0; i < SpotCount; i++)
        {
            var p = new Vector2(DrawKit.H(Seed, i, 21) * w, h * (0.35f + DrawKit.H(Seed, i, 22) * 0.65f));
            p += new Vector2(MathF.Sin(t * 0.9f * sway + i * 0.7f), MathF.Cos(t * 0.6f * sway + i)) * MathF.Min(w, h) * 0.006f;
            float r = MathF.Min(w, h) * (0.006f + DrawKit.H(Seed, i, 23) * 0.018f);
            float tw = 0.4f + 0.6f * MathF.Max(0f, MathF.Sin(t * (1.2f + DrawKit.H(Seed, i, 24) * 2f) * sway + i * 1.7f));
            DrawKit.Glow(dc, p, r * 2.4f, Gfx.C4(LightColor, 0.5f * inten * tw));
        }
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
    }
}
