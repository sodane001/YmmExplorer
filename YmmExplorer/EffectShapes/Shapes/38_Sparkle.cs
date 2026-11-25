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

public enum SparkleShape
{
    [Display(Name = "4つ角の星（キラッ）")] Star4,
    [Display(Name = "5つ角の星")] Star5,
    [Display(Name = "ハート")] Heart,
    [Display(Name = "ひし形（ダイヤ）")] Diamond,
    [Display(Name = "丸い光の粒")] Dot,
}

// =====================================================================
//  キラキラ・光の粒
//   舞い上がりながら瞬く光。星・ハート・ダイヤ・丸の粒から選べる
// =====================================================================
public sealed class SparklePlugin : IShapePlugin
{
    public string Name => "演出：キラキラ・光の粒";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SparkleParameter(sharedData);
}

public sealed class SparkleParameter : EffectShapeParameterBase
{
    public SparkleParameter() : this(null) { }
    public SparkleParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "粒", Name = "形")]
    [EnumComboBox]
    public SparkleShape Shape { get => shape; set => Set(ref shape, value); }
    SparkleShape shape = SparkleShape.Star4;

    [Display(GroupName = "粒", Name = "色1")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0xFF, 0xE9, 0x8A);

    [Display(GroupName = "粒", Name = "色2")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0xFF, 0xA8, 0xE0);

    [Display(GroupName = "粒", Name = "虹色にする")]
    [ToggleSlider]
    public bool Rainbow { get => rainbow; set => Set(ref rainbow, value); }
    bool rainbow = false;

    [Display(GroupName = "粒", Name = "数")]
    [AnimationSlider("F0", "個", 0, 400)]
    public Animation Count { get; } = new Animation(80, 0, 5000);

    [Display(GroupName = "粒", Name = "大きさ")]
    [AnimationSlider("F0", "px", 4, 80)]
    public Animation Size { get; } = new Animation(18, 1, 800);

    [Display(GroupName = "粒", Name = "光のにじみ")]
    [ToggleSlider]
    public bool Glow { get => glow; set => Set(ref glow, value); }
    bool glow = true;

    [Display(GroupName = "動き", Name = "舞い上がる速さ")]
    [AnimationSlider("F0", "px/s", -200, 300)]
    public Animation Rise { get; } = new Animation(45, -5000, 5000);

    [Display(GroupName = "動き", Name = "横に漂う幅")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Drift { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "動き", Name = "またたく速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation TwinkleSpeed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "動き", Name = "回転の速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Spin { get; } = new Animation(60, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Rise, Drift, TwinkleSpeed, Spin];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), rise = c.F(Rise), drift = c.F(Drift);
        float tws = c.F(TwinkleSpeed) / 100f, spin = c.F(Spin) / 100f;
        float margin = size * 2f, spanY = h + margin * 2f;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            var rng = new Random(Seed);
            for (int i = 0; i < n; i++)
            {
                float x0 = rng.NextF() * w, ph = rng.NextF();
                float d = 0.4f + 0.6f * rng.NextF();
                float f = 0.6f + 1.4f * rng.NextF(), ph2 = rng.NextF() * MathF.PI * 2f;
                float rot0 = rng.NextF() * MathF.PI * 2f, rs = (rng.NextF() - 0.5f) * 3f;
                float cm = rng.NextF(), hue = rng.NextF() * 360f;

                float y = rise >= 0f ? h + margin - Gfx.Mod(t * rise * d + ph * spanY, spanY) : -margin + Gfx.Mod(t * -rise * d + ph * spanY, spanY);
                float x = x0 + MathF.Sin(t * 0.6f * f + ph2) * drift * d;
                float tw = MathF.Pow(MathF.Max(0f, MathF.Sin(t * f * 2f * tws + ph2)), 0.7f) * d;
                if (tw < 0.03f) continue;

                var col = Rainbow ? Gfx.Hsv(hue, 0.45f, 1f) : Gfx.Mix(Color1, Color2, cm);
                var hot = Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.5f);
                var p = new Vector2(x, y);
                float r = size * (0.4f + 0.6f * d) * (0.35f + 0.65f * tw);
                float rot = rot0 + t * rs * spin;

                if (Glow)
                {
                    brush.Color = Gfx.C4(col, 0.10f * tw);
                    dc.FillEllipse(new Ellipse(p, r * 2.2f, r * 2.2f), brush);
                }
                brush.Color = Gfx.C4(hot, 0.95f * tw);
                switch (Shape)
                {
                    case SparkleShape.Star4:
                        Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(p, r, 4, 0.2f, rot));
                        break;
                    case SparkleShape.Star5:
                        Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(p, r, 5, 0.5f, rot));
                        break;
                    case SparkleShape.Heart:
                        Gfx.FillPoly(dc, brush, ShapeGeom.HeartPoly(p, r, rot * 0.3f));
                        break;
                    case SparkleShape.Diamond:
                        Gfx.FillPoly(dc, brush, new List<Vector2>
                        {
                            p + new Vector2(0, -r), p + new Vector2(r * 0.6f, 0), p + new Vector2(0, r), p + new Vector2(-r * 0.6f, 0),
                        });
                        break;
                    default:
                        dc.FillEllipse(new Ellipse(p, r * 0.5f, r * 0.5f), brush);
                        brush.Color = Gfx.C4(col, 0.25f * tw);
                        dc.FillEllipse(new Ellipse(p, r, r), brush);
                        break;
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
