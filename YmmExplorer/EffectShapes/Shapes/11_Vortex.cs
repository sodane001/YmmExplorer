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

public enum VortexKind
{
    [Display(Name = "竜巻（漏斗）")] Tornado,
    [Display(Name = "渦巻き（水・銀河）")] Whirl,
}

// =====================================================================
//  竜巻・渦
//   巻き上がる漏斗状の竜巻、または中心へ巻き込む渦巻き
// =====================================================================
public sealed class VortexPlugin : IShapePlugin
{
    public string Name => "演出：竜巻・渦";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new VortexParameter(sharedData);
}

public sealed class VortexParameter : EffectShapeParameterBase
{
    public VortexParameter() : this(null) { }
    public VortexParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public VortexKind Kind { get => kind; set => Set(ref kind, value); }
    VortexKind kind = VortexKind.Tornado;

    [Display(GroupName = "色", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x8A, 0x84, 0x7C);

    [Display(GroupName = "位置と大きさ", Name = "X位置（足元）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（足元／中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(88, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "高さ（竜巻）／半径（渦巻き）")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public new Animation Height { get; } = new Animation(760, 20, 8000);

    [Display(GroupName = "位置と大きさ", Name = "上の広がり（半径）")]
    [AnimationSlider("F0", "px", 20, 600)]
    public Animation TopRadius { get; } = new Animation(220, 5, 4000);

    [Display(GroupName = "動き", Name = "回転の速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "動き", Name = "ゆれ（竜巻の蛇行）")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Sway { get; } = new Animation(50, 0, 2000);

    [Display(GroupName = "粒", Name = "粒・破片の数")]
    [AnimationSlider("F0", "個", 20, 1000)]
    public Animation Count { get; } = new Animation(420, 1, 8000);

    [Display(GroupName = "粒", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Density { get; } = new Animation(100, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Height, TopRadius, Speed, Sway, Count, Density];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float ts = c.Time * c.F(Speed) / 100f, t = c.Time;
        var foot = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float H = c.F(Height), rTop = c.F(TopRadius), sway = c.F(Sway), dens = c.F(Density) / 100f;
        int n = (int)c.F(Count);
        var dark = Gfx.Mix(Color, WpfColor.FromRgb(0, 0, 0), 0.45f);
        var light = Gfx.Mix(Color, WpfColor.FromRgb(255, 255, 255), 0.35f);

        using var brush = dc.CreateSolidColorBrush(default);

        if (Kind == VortexKind.Tornado)
        {
            // 高さ u (0=足元,1=上端) での漏斗の中心と半径
            Vector2 Axis(float u) => foot + new Vector2(MathF.Sin(t * 0.9f + u * 3.2f) * sway * u * u, -u * H);
            float Rad(float u) => 14f + (rTop - 14f) * MathF.Pow(u, 1.7f);

            // 漏斗の本体（重なる半透明の楕円）
            for (int i = 0; i < 28; i++)
            {
                float u = i / 27f;
                var a = Axis(u);
                float r = Rad(u);
                brush.Color = Gfx.C4(dark, 0.06f * dens * (1.1f - u * 0.5f));
                dc.FillEllipse(new Ellipse(a, r * 1.05f, r * 0.26f), brush);
            }

            // 渦巻く粒（奥は暗く、手前は明るく）
            var rng = new Random(Seed);
            for (int i = 0; i < n; i++)
            {
                float u = MathF.Pow(rng.NextF(), 0.8f);
                float a0 = rng.NextF() * MathF.PI * 2f, sp = 0.7f + 0.8f * rng.NextF(), rr = 0.75f + 0.5f * rng.NextF();
                float ang = a0 + ts * 4.5f * sp * (1.6f - u);
                var axis = Axis(u);
                float r = Rad(u) * rr;
                var p = axis + new Vector2(MathF.Cos(ang) * r, MathF.Sin(ang) * r * 0.26f);
                float front = 0.5f + 0.5f * MathF.Sin(ang);
                var col = Gfx.Mix(dark, light, front);
                float len = 6f + 22f * (1f - u * 0.5f);
                float ang2 = ang - 0.22f * sp;
                var p2 = axis + new Vector2(MathF.Cos(ang2) * r, MathF.Sin(ang2) * r * 0.26f);
                brush.Color = Gfx.C4(col, (0.25f + 0.5f * front) * dens);
                dc.DrawLine(p2, p, brush, 1f + 2.5f * u + (rng.NextF() < 0.1f ? 2f : 0f));
            }

            // 足元の砂煙
            var rd = new Random(Seed + 3);
            for (int i = 0; i < 12; i++)
            {
                float a = rd.NextF() * MathF.PI * 2f + ts * 1.5f, rr = 30f + 130f * rd.NextF();
                var p = foot + new Vector2(MathF.Cos(a) * rr, MathF.Sin(a) * rr * 0.22f - 6f);
                float r = 40f + 70f * rd.NextF();
                using var gb = Gfx.Radial(dc, p, r, (0f, Gfx.C4(dark, 0.22f * dens)), (1f, Gfx.C4(dark, 0f)));
                dc.FillEllipse(new Ellipse(p, r, r * 0.5f), gb);
            }
        }
        else
        {
            // 中心へ巻き込む渦巻き：腕が渦を巻き、粒が流れ込む
            float R = H;
            int arms = 5;
            for (int k = 0; k < arms; k++)
            {
                float a0 = k * MathF.PI * 2f / arms + ts * 0.8f;
                var pts = new List<Vector2>();
                for (int i = 0; i <= 60; i++)
                {
                    float f = i / 60f;
                    float r = R * f;
                    float a = a0 + (1f - f) * 4.2f;
                    pts.Add(foot + new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r));
                }
                brush.Color = Gfx.C4(Color, 0.10f * dens);
                FormKit.Stroke(dc, brush, pts, 18f, false);
                brush.Color = Gfx.C4(light, 0.45f * dens);
                FormKit.Stroke(dc, brush, pts, 2.2f, false);
            }
            var rs = new Random(Seed);
            for (int i = 0; i < n; i++)
            {
                float a0 = rs.NextF() * MathF.PI * 2f, ph = rs.NextF(), sp = 0.5f + rs.NextF();
                float u = Gfx.Mod(t * 0.25f * sp + ph, 1f);                 // 外→中心へ
                float r = R * (1f - u);
                float a = a0 + u * 4.2f + ts * 0.8f;
                var p = foot + new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r);
                brush.Color = Gfx.C4(light, MathF.Sin(u * MathF.PI) * 0.7f * dens);
                dc.FillEllipse(new Ellipse(p, 1.4f + 1.6f * u, 1.4f + 1.6f * u), brush);
            }
            using var cb = Gfx.Radial(dc, foot, R * 0.2f, (0f, Gfx.C4(WpfColor.FromRgb(0, 0, 0), 0.5f * dens)), (1f, Gfx.C4(WpfColor.FromRgb(0, 0, 0), 0f)));
            dc.FillEllipse(new Ellipse(foot, R * 0.2f, R * 0.2f), cb);
        }
    }
}
