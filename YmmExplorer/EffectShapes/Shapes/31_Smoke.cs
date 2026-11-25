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

public enum SmokeKind
{
    [Display(Name = "煙（灰色）")] Smoke,
    [Display(Name = "蒸気（白）")] Steam,
    [Display(Name = "黒煙")] BlackSmoke,
    [Display(Name = "カスタム色")] Custom,
}

// =====================================================================
//  煙・蒸気
//   立ちのぼりながら広がり、薄れていく。風・乱れ・濃さを調整できる
// =====================================================================
public sealed class SmokePlugin : IShapePlugin
{
    public string Name => "演出：煙・蒸気";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SmokeParameter(sharedData);
}

public sealed class SmokeParameter : EffectShapeParameterBase
{
    public SmokeParameter() : this(null) { }
    public SmokeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "煙", Name = "種類")]
    [EnumComboBox]
    public SmokeKind Kind { get => kind; set => Set(ref kind, value); }
    SmokeKind kind = SmokeKind.Smoke;

    [Display(GroupName = "煙", Name = "色（カスタム色のとき）")]
    [ColorPicker]
    public WpfColor CustomColor { get => custom; set => Set(ref custom, value); }
    WpfColor custom = WpfColor.FromRgb(0xC8, 0xB4, 0xA0);

    [Display(GroupName = "位置", Name = "発生位置X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SourceX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "発生位置Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SourceY { get; } = new Animation(92, -100, 200);

    [Display(GroupName = "位置", Name = "発生源の幅")]
    [AnimationSlider("F0", "px", 0, 800)]
    public Animation SourceWidth { get; } = new Animation(120, 0, 8000);

    [Display(GroupName = "煙", Name = "立ちのぼる高さ")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation RiseHeight { get; } = new Animation(700, 10, 8000);

    [Display(GroupName = "煙", Name = "粒の数")]
    [AnimationSlider("F0", "個", 10, 300)]
    public Animation Count { get; } = new Animation(80, 1, 3000);

    [Display(GroupName = "煙", Name = "粒の大きさ")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation Size { get; } = new Animation(90, 3, 3000);

    [Display(GroupName = "煙", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Density { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "動き", Name = "上昇の速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "動き", Name = "風（横に流れる）")]
    [AnimationSlider("F0", "px", -600, 600)]
    public Animation Wind { get; } = new Animation(120, -8000, 8000);

    [Display(GroupName = "動き", Name = "乱れ")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Turbulence { get; } = new Animation(50, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [SourceX, SourceY, SourceWidth, RiseHeight, Count, Size, Density, Speed, Wind, Turbulence];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var src = new Vector2(c.F(SourceX) / 100f * w, c.F(SourceY) / 100f * h);
        float sw = c.F(SourceWidth), rise = c.F(RiseHeight), size = c.F(Size);
        float dens = c.F(Density) / 100f, spd = c.F(Speed) / 100f, wind = c.F(Wind), turb = c.F(Turbulence);
        int n = (int)c.F(Count);

        WpfColor dark, light;
        switch (Kind)
        {
            case SmokeKind.Steam: dark = WpfColor.FromRgb(0xD8, 0xDE, 0xE6); light = WpfColor.FromRgb(255, 255, 255); break;
            case SmokeKind.BlackSmoke: dark = WpfColor.FromRgb(0x0E, 0x0C, 0x0B); light = WpfColor.FromRgb(0x4A, 0x45, 0x42); break;
            case SmokeKind.Custom: dark = Gfx.Mix(CustomColor, WpfColor.FromRgb(0, 0, 0), 0.3f); light = Gfx.Mix(CustomColor, WpfColor.FromRgb(255, 255, 255), 0.25f); break;
            default: dark = WpfColor.FromRgb(0x4A, 0x4A, 0x4C); light = WpfColor.FromRgb(0xA8, 0xA8, 0xAA); break;
        }
        float baseA = Kind == SmokeKind.Steam ? 0.16f : 0.22f;

        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float ph = rng.NextF(), rx = rng.NextF() * 2f - 1f, sp = 0.7f + 0.6f * rng.NextF(), sz = 0.7f + 0.6f * rng.NextF();
            float u = Gfx.Mod(t * spd * sp * 0.3f + ph, 1f);
            float x = src.X + rx * sw / 2f * (1f - 0.4f * u) + wind * u * u + MathF.Sin(t * 0.9f + ph * 6.28f + u * 5f) * turb * u;
            float y = src.Y - u * rise;
            float r = size * sz * (0.45f + 1.1f * u);
            float a = dens * baseA * MathF.Pow(MathF.Sin(u * MathF.PI), 0.8f);
            if (a <= 0.003f) continue;
            var col = Gfx.Mix(dark, light, Gfx.Smooth(0f, 0.9f, u));
            var ctr = new Vector2(x, y);
            using var b = Gfx.Radial(dc, ctr, r, (0f, Gfx.C4(col, a)), (0.5f, Gfx.C4(col, a * 0.55f)), (1f, Gfx.C4(col, 0f)));
            dc.FillEllipse(new Ellipse(ctr, r, r), b);
        }
    }
}
