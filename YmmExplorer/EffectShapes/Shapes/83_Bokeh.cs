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
//  ボケ光
//   ふわっとにじむ丸い光。六角形ボケ・暖色/寒色/虹色
// =====================================================================
public enum BokehPalette
{
    [Display(Name = "暖色（夜景）")] Warm,
    [Display(Name = "寒色")] Cool,
    [Display(Name = "虹色")] Rainbow,
    [Display(Name = "2色を使う")] TwoColor,
}

public sealed class BokehPlugin : IShapePlugin
{
    public string Name => "演出：ボケ光";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BokehParameter(sharedData);
}

public sealed class BokehParameter : EffectShapeParameterBase
{
    public BokehParameter() : this(null) { }
    public BokehParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "配色")]
    [EnumComboBox]
    public BokehPalette Palette { get => pal; set => Set(ref pal, value); }
    BokehPalette pal = BokehPalette.Warm;

    [Display(GroupName = "色", Name = "色1（2色のとき）")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0xFF, 0xB0, 0x4A);

    [Display(GroupName = "色", Name = "色2（2色のとき）")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0xFF, 0x6A, 0xC0);

    [Display(GroupName = "ボケ", Name = "数")]
    [AnimationSlider("F0", "個", 0, 150)]
    public Animation Count { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "ボケ", Name = "最大の大きさ（半径）")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation Size { get; } = new Animation(90, 4, 3000);

    [Display(GroupName = "ボケ", Name = "六角形のボケにする")]
    [ToggleSlider]
    public bool Hexagon { get => hex; set => Set(ref hex, value); }
    bool hex = false;

    [Display(GroupName = "ボケ", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "動き", Name = "漂う速さ")]
    [AnimationSlider("F0", "px/s", -100, 100)]
    public Animation Drift { get; } = new Animation(14, -3000, 3000);

    [Display(GroupName = "動き", Name = "またたく速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Twinkle { get; } = new Animation(100, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Brightness, Drift, Twinkle];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), bright = c.F(Brightness) / 100f, drift = c.F(Drift), tw = c.F(Twinkle) / 100f;
        float margin = size * 1.5f, spanY = h + margin * 2f;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            var rng = new Random(Seed);
            for (int i = 0; i < n; i++)
            {
                float x0 = rng.NextF() * w, y0 = rng.NextF();
                float d = 0.25f + 0.75f * rng.NextF(), ph = rng.NextF() * MathF.PI * 2f, f = 0.2f + 0.6f * rng.NextF();
                float hue = rng.NextF(), pick = rng.NextF();
                WpfColor col = Palette switch
                {
                    BokehPalette.Warm => Gfx.Mix(WpfColor.FromRgb(0xFF, 0xC0, 0x60), WpfColor.FromRgb(0xFF, 0x60, 0x40), hue),
                    BokehPalette.Cool => Gfx.Mix(WpfColor.FromRgb(0x60, 0xC8, 0xFF), WpfColor.FromRgb(0xA0, 0x80, 0xFF), hue),
                    BokehPalette.Rainbow => Gfx.Hsv(hue * 360f, 0.55f, 1f),
                    _ => pick < 0.5f ? Color1 : Color2,
                };

                float y = Gfx.Mod(y0 * spanY - t * drift * d, spanY) - margin;
                float x = x0 + MathF.Sin(t * 0.3f + ph) * 14f * d;
                float r = size * d;
                float a = (0.45f + 0.55f * (0.5f + 0.5f * MathF.Sin(t * f * 2f * tw + ph))) * bright * (0.35f + 0.65f * d);
                var p = new Vector2(x, y);

                if (Hexagon)
                {
                    var poly = ShapeGeom.StarPoly(p, r, 6, 1f, ph);
                    brush.Color = Gfx.C4(col, 0.14f * a);
                    Gfx.FillPoly(dc, brush, poly);
                    brush.Color = Gfx.C4(col, 0.55f * a);
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(poly), 2f);
                }
                else
                {
                    brush.Color = Gfx.C4(col, 0.12f * a);
                    dc.FillEllipse(new Ellipse(p, r, r), brush);
                    brush.Color = Gfx.C4(col, 0.5f * a);
                    dc.DrawEllipse(new Ellipse(p, r, r), brush, 2f);
                    brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.5f), 0.12f * a);
                    dc.FillEllipse(new Ellipse(p, r * 0.5f, r * 0.5f), brush);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
