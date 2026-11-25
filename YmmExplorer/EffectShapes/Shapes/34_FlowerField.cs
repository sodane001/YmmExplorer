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
//  花畑・花びらの舞
//   風に揺れる茎と花。草の葉と、舞い上がる花びら
// =====================================================================
public sealed class FlowerFieldPlugin : IShapePlugin
{
    public string Name => "演出：花畑・花びらの舞";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FlowerFieldParameter(sharedData);
}

public sealed class FlowerFieldParameter : EffectShapeParameterBase
{
    public FlowerFieldParameter() : this(null) { }
    public FlowerFieldParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "花", Name = "カラフルにする")]
    [ToggleSlider]
    public bool Colorful { get => colorful; set => Set(ref colorful, value); }
    bool colorful = true;

    [Display(GroupName = "花", Name = "色（カラフルがオフのとき）")]
    [ColorPicker]
    public WpfColor FlowerColor { get => flower; set => Set(ref flower, value); }
    WpfColor flower = WpfColor.FromRgb(0xFF, 0x7A, 0xB8);

    [Display(GroupName = "花", Name = "草の色")]
    [ColorPicker]
    public WpfColor GrassColor { get => grass; set => Set(ref grass, value); }
    WpfColor grass = WpfColor.FromRgb(0x3E, 0xA8, 0x50);

    [Display(GroupName = "花", Name = "花の数")]
    [AnimationSlider("F0", "本", 3, 150)]
    public Animation Count { get; } = new Animation(40, 1, 1000);

    [Display(GroupName = "花", Name = "花の大きさ")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation FlowerSize { get; } = new Animation(34, 3, 1000);

    [Display(GroupName = "花", Name = "茎の高さ")]
    [AnimationSlider("F0", "px", 60, 700)]
    public Animation StemHeight { get; } = new Animation(260, 10, 5000);

    [Display(GroupName = "花", Name = "草の本数")]
    [AnimationSlider("F0", "本", 0, 600)]
    public Animation GrassCount { get; } = new Animation(220, 0, 5000);

    [Display(GroupName = "風", Name = "風の強さ（揺れ幅）")]
    [AnimationSlider("F0", "px", 0, 120)]
    public Animation Sway { get; } = new Animation(32, 0, 1000);

    [Display(GroupName = "風", Name = "風の速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation WindSpeed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "風", Name = "舞い上がる花びらの数")]
    [AnimationSlider("F0", "枚", 0, 150)]
    public Animation Petals { get; } = new Animation(30, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, FlowerSize, StemHeight, GrassCount, Sway, WindSpeed, Petals];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float ts = c.Time * c.F(WindSpeed) / 100f;
        int n = (int)c.F(Count), ng = (int)c.F(GrassCount), npt = (int)c.F(Petals);
        float fs = c.F(FlowerSize), sh = c.F(StemHeight), sway = c.F(Sway);
        var darkGrass = Gfx.Mix(GrassColor, WpfColor.FromRgb(0, 0, 0), 0.35f);
        var yellow = WpfColor.FromRgb(0xFF, 0xD8, 0x3A);

        using var brush = dc.CreateSolidColorBrush(default);
        var rg = new Random(Seed + 11);

        // ---- 草の葉（背景）----
        for (int i = 0; i < ng; i++)
        {
            float x = rg.NextF() * w, bl = (0.15f + 0.4f * rg.NextF()) * sh, ph = rg.NextF() * 6.28f;
            float by = h + 4f;
            float sw = MathF.Sin(ts * 1.6f + x * 0.01f + ph) * sway * 0.7f * (bl / sh);
            brush.Color = Gfx.C4(Gfx.Mix(darkGrass, GrassColor, rg.NextF()), 0.95f);
            var tip = new Vector2(x + sw, by - bl);
            Gfx.FillPoly(dc, brush, new List<Vector2> { new(x - 3f, by), new(x + 3f, by), tip });
        }

        // ---- 花（茎→葉→花びら→中心）----
        var rf = new Random(Seed);
        try
        {
            for (int i = 0; i < n; i++)
            {
                float x = (i + rf.NextF()) / n * w, baseY = h + 4f - rf.NextF() * h * 0.06f;
                float stem = sh * (0.5f + 0.5f * rf.NextF()), ph = rf.NextF() * 6.28f, size = fs * (0.7f + 0.6f * rf.NextF());
                int petals = 5 + rf.Next(0, 4), colIdx = rf.Next(0, PartyKit.Palette.Length);
                float rot = rf.NextF() * 6.28f;

                float sw = MathF.Sin(ts * 1.6f + x * 0.01f + ph) * sway * (stem / sh);
                var p0 = new Vector2(x, baseY);
                var tip = new Vector2(x + sw, baseY - stem);
                var ctrl = new Vector2(x + sw * 0.2f, baseY - stem * 0.55f);
                var pts = new List<Vector2>();
                for (int k = 0; k <= 16; k++)
                {
                    float f = k / 16f;
                    pts.Add((1 - f) * (1 - f) * p0 + 2 * (1 - f) * f * ctrl + f * f * tip);
                }
                brush.Color = Gfx.C4(GrassColor, 1f);
                FormKit.Stroke(dc, brush, pts, MathF.Max(2f, size * 0.12f), false);
                // 葉
                var lp = pts[7];
                brush.Color = Gfx.C4(Gfx.Mix(GrassColor, darkGrass, 0.4f), 1f);
                dc.Transform = Matrix3x2.CreateRotation(-0.6f + sw * 0.01f, lp);
                dc.FillEllipse(new Ellipse(lp + new Vector2(size * 0.45f, 0f), size * 0.5f, size * 0.17f), brush);
                dc.Transform = Matrix3x2.Identity;

                // 花びら
                var col = Colorful ? PartyKit.Palette[colIdx] : FlowerColor;
                var tipDir = Vector2.Normalize(tip - ctrl);
                float baseAng = MathF.Atan2(tipDir.Y, tipDir.X) + rot * 0.15f;
                for (int k = 0; k < petals; k++)
                {
                    float a = baseAng + k * MathF.PI * 2f / petals;
                    var pc = tip + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (size * 0.52f);
                    var poly = new List<Vector2>();
                    for (int e = 0; e < 16; e++)
                    {
                        float ea = e / 16f * MathF.PI * 2f;
                        float ex = MathF.Cos(ea) * size * 0.5f, ey = MathF.Sin(ea) * size * 0.27f;
                        poly.Add(pc + new Vector2(ex * MathF.Cos(a) - ey * MathF.Sin(a), ex * MathF.Sin(a) + ey * MathF.Cos(a)));
                    }
                    brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.12f * (k % 2)), 1f);
                    Gfx.FillPoly(dc, brush, poly);
                }
                brush.Color = Gfx.C4(yellow, 1f);
                dc.FillEllipse(new Ellipse(tip, size * 0.22f, size * 0.22f), brush);
                brush.Color = Gfx.C4(WpfColor.FromRgb(0xD0, 0x90, 0x10), 0.8f);
                dc.FillEllipse(new Ellipse(tip + new Vector2(size * 0.06f, size * 0.05f), size * 0.08f, size * 0.08f), brush);
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }

        // ---- 舞い上がる花びら ----
        var rp = new Random(Seed + 21);
        for (int i = 0; i < npt; i++)
        {
            float x0 = rp.NextF() * w, ph = rp.NextF(), sp = 0.5f + rp.NextF(), r = 3f + 4f * rp.NextF();
            int colIdx = rp.Next(0, PartyKit.Palette.Length);
            float u = Gfx.Mod(ts * 0.1f * sp + ph, 1f);
            var p = new Vector2(x0 + u * 300f + MathF.Sin(ts * 2f + ph * 30f) * 30f, h * 0.95f - u * h * 0.9f);
            brush.Color = Gfx.C4(Colorful ? PartyKit.Palette[colIdx] : FlowerColor, 0.9f * MathF.Sin(u * MathF.PI));
            dc.FillEllipse(new Ellipse(p, r, r * 0.6f), brush);
        }
    }
}
