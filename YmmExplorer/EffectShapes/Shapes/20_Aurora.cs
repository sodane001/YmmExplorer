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
//  オーロラ
//   ゆらめく光のカーテン。縦の筋が明るさを変えながら波打つ
// =====================================================================
public sealed class AuroraPlugin : IShapePlugin
{
    public string Name => "演出：オーロラ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AuroraParameter(sharedData);
}

public sealed class AuroraParameter : EffectShapeParameterBase
{
    public AuroraParameter() : this(null) { }
    public AuroraParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "下の色")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0x3D, 0xFF, 0x9A);

    [Display(GroupName = "色", Name = "中間の色")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0x7A, 0x5C, 0xFF);

    [Display(GroupName = "色", Name = "上端の色")]
    [ColorPicker]
    public WpfColor Color3 { get => c3; set => Set(ref c3, value); }
    WpfColor c3 = WpfColor.FromRgb(0xFF, 0x5C, 0xC8);

    [Display(GroupName = "カーテン", Name = "枚数")]
    [AnimationSlider("F0", "枚", 1, 6)]
    public Animation Curtains { get; } = new Animation(3, 1, 12);

    [Display(GroupName = "カーテン", Name = "下端の高さ位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BaseY { get; } = new Animation(62, -100, 200);

    [Display(GroupName = "カーテン", Name = "立ち上がる高さ")]
    [AnimationSlider("F0", "px", 100, 1000)]
    public Animation CurtainHeight { get; } = new Animation(520, 10, 8000);

    [Display(GroupName = "カーテン", Name = "うねりの幅")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation WaveAmp { get; } = new Animation(60, 0, 2000);

    [Display(GroupName = "カーテン", Name = "筋の間隔", Description = "小さいほど滑らかですが重くなります")]
    [AnimationSlider("F0", "px", 4, 24)]
    public Animation Detail { get; } = new Animation(8, 3, 100);

    [Display(GroupName = "光", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Intensity { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "光", Name = "ゆらめく速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Curtains, BaseY, CurtainHeight, WaveAmp, Detail, Intensity, Speed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float ts = c.Time * c.F(Speed) / 100f;
        int n = Math.Clamp((int)c.F(Curtains), 1, 12);
        float step = MathF.Max(3f, c.F(Detail));
        float amp = c.F(WaveAmp), hgt = c.F(CurtainHeight), I = c.F(Intensity) / 100f;
        const int S = 7;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            for (int k = 0; k < n; k++)
            {
                float ph = k * 1.7f + Seed * 0.13f;
                float by = c.F(BaseY) / 100f * h + (k - (n - 1) / 2f) * h * 0.06f;
                float mixK = n == 1 ? 0f : k / (float)(n - 1) * 0.6f;
                var low = Gfx.Mix(Color1, Color2, mixK);

                Vector2 prev = default;
                bool hasPrev = false;
                for (float x = 0f; x <= w + step; x += step)
                {
                    float baseY = by + MathF.Sin(x * 0.0042f + ts * 0.3f + ph) * amp
                                     + MathF.Sin(x * 0.011f - ts * 0.5f + ph * 2f) * amp * 0.4f;
                    float br = 0.5f + 0.5f * MathF.Sin(x * 0.021f + ts * 0.8f + ph) * MathF.Sin(x * 0.0073f - ts * 0.35f + ph * 1.3f);
                    br = MathF.Pow(Math.Clamp(br, 0f, 1f), 1.3f);
                    float colH = hgt * (0.45f + 0.55f * br);

                    for (int s = 0; s < S; s++)
                    {
                        float f0 = s / (float)S, f1 = (s + 1) / (float)S;
                        float y0 = baseY - colH * f0, y1 = baseY - colH * f1;
                        float a = I * (0.15f + 0.55f * br) * MathF.Pow(1f - f0, 1.6f) * 0.55f;
                        var col = Gfx.Mix(low, Color2, Gfx.Smooth(0.15f, 0.8f, f0));
                        col = Gfx.Mix(col, Color3, Gfx.Smooth(0.65f, 1f, f0) * 0.7f);
                        brush.Color = Gfx.C4(col, a);
                        dc.DrawLine(new Vector2(x, y0), new Vector2(x, y1), brush, step * 1.25f);
                    }

                    // 下端の明るい縁
                    var cur = new Vector2(x, baseY);
                    if (hasPrev)
                    {
                        brush.Color = Gfx.C4(Gfx.Mix(low, WpfColor.FromRgb(255, 255, 255), 0.4f), 0.45f * I * (0.4f + 0.6f * br));
                        dc.DrawLine(prev, cur, brush, 2.5f);
                    }
                    prev = cur;
                    hasPrev = true;
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
