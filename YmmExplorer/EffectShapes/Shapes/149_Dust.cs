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
//  ほこり・光の中の粒子
//   室内に差し込む光の帯と、その中でゆっくり舞う、ほこりの粒
// =====================================================================
public sealed class DustPlugin : IShapePlugin
{
    public string Name => "演出：ほこり・光の中の粒子";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DustParameter(sharedData);
}

public sealed class DustParameter : EffectShapeParameterBase
{
    public DustParameter() : this(null) { }
    public DustParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "粒", Name = "粒の数")]
    [TextBoxSlider("F0", "個", 0, 600)]
    [Range(0, 4000)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 160;

    [Display(GroupName = "粒", Name = "粒の大きさ")]
    [AnimationSlider("F1", "px", 0.5, 12)]
    public Animation ParticleSize { get; } = new Animation(3, 0.1, 200);

    [Display(GroupName = "粒", Name = "舞う速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Drift { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "粒", Name = "光の外の粒も見せる")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Outside { get; } = new Animation(20, 0, 100);

    [Display(GroupName = "光", Name = "光の帯を出す")]
    [ToggleSlider]
    public bool Beam { get => beam; set => Set(ref beam, value); }
    bool beam = true;

    [Display(GroupName = "光", Name = "光の帯のX位置（上端）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BeamX { get; } = new Animation(30, -100, 200);

    [Display(GroupName = "光", Name = "光の帯の傾き")]
    [AnimationSlider("F0", "°", -60, 60)]
    public Animation BeamAngle { get; } = new Animation(-28, -89, 89);

    [Display(GroupName = "光", Name = "光の帯の幅")]
    [AnimationSlider("F0", "px", 50, 1200)]
    public Animation BeamWidth { get; } = new Animation(380, 1, 8000);

    [Display(GroupName = "光", Name = "光の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xF0, 0xCC);

    [Display(GroupName = "光", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(70, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [ParticleSize, Drift, Outside, BeamX, BeamAngle, BeamWidth, Brightness];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float bright = c.F(Brightness) / 100f, drift = c.F(Drift) / 100f, ps = c.F(ParticleSize), outside = c.F(Outside) / 100f;
        float ang = Gfx.Rad(c.F(BeamAngle));
        var dir = new Vector2(MathF.Sin(ang), MathF.Cos(ang));
        var nrm = new Vector2(dir.Y, -dir.X);
        var top = new Vector2(c.F(BeamX) / 100f * w, -h * 0.05f);
        float bwid = c.F(BeamWidth);
        float len = MathF.Sqrt(w * w + h * h) * 1.4f;

        dc.PrimitiveBlend = PrimitiveBlend.Add;
        if (Beam)
        {
            var a = top; var b = top + dir * len;
            var poly = new List<Vector2> { a + nrm * bwid * 0.35f, b + nrm * bwid * 0.75f, b - nrm * bwid * 0.75f, a - nrm * bwid * 0.35f };
            float flick = 0.85f + 0.15f * MathF.Sin(t * 0.9f);
            using (var g = Gfx.Linear(dc, a, a + dir * len * 0.7f, (0f, Gfx.C4(Color, 0.3f * bright * flick)), (1f, Gfx.C4(Color, 0f))))
                Gfx.FillPoly(dc, g, DrawKit.ClipRect(poly, 0, 0, w, h));
            // 帯のふちを、ぼかす(少し広い帯を薄く重ねる)
            var wide = new List<Vector2> { a + nrm * bwid * 0.5f, b + nrm * bwid, b - nrm * bwid, a - nrm * bwid * 0.5f };
            using (var g = Gfx.Linear(dc, a, a + dir * len * 0.6f, (0f, Gfx.C4(Color, 0.1f * bright * flick)), (1f, Gfx.C4(Color, 0f))))
                Gfx.FillPoly(dc, g, DrawKit.ClipRect(wide, 0, 0, w, h));
        }

        using var brush = dc.CreateSolidColorBrush(default);
        for (int i = 0; i < Count; i++)
        {
            // ゆっくり漂う(いくつかの波を重ねた、なめらかな動き)
            float px = DrawKit.H(Seed, i, 1) * w, py = DrawKit.H(Seed, i, 2) * h;
            float s1 = 0.2f + DrawKit.H(Seed, i, 3) * 0.4f, s2 = 0.15f + DrawKit.H(Seed, i, 4) * 0.3f;
            var p = new Vector2(
                Gfx.Mod(px + MathF.Sin(t * s1 * drift + i) * w * 0.03f + t * w * 0.004f * drift, w),
                Gfx.Mod(py + MathF.Cos(t * s2 * drift + i * 1.7f) * h * 0.04f - t * h * 0.006f * drift, h));
            // 光の帯の中にいるほど明るい
            float inBeam = 1f;
            if (Beam)
            {
                var rel = p - top;
                float across = MathF.Abs(Vector2.Dot(rel, nrm));
                float along = Vector2.Dot(rel, dir);
                float halfW = bwid * (0.35f + 0.4f * Math.Clamp(along / len, 0f, 1f));
                inBeam = Math.Clamp(1f - across / halfW, 0f, 1f);
                inBeam = MathF.Max(outside, inBeam * inBeam * (3f - 2f * inBeam));
            }
            float tw = 0.5f + 0.5f * MathF.Sin(t * (1.5f + DrawKit.H(Seed, i, 5) * 3f) + i);
            float r = ps * (0.5f + DrawKit.H(Seed, i, 6));
            float a2 = bright * inBeam * (0.35f + 0.65f * tw);
            if (a2 <= 0.01f) continue;
            DrawKit.Glow(dc, p, r * 3f, Gfx.C4(Color, 0.35f * a2));
            brush.Color = Gfx.C4(Color, MathF.Min(1f, a2));
            dc.FillEllipse(new Ellipse(p, r * 0.6f, r * 0.6f), brush);
        }
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
    }
}
