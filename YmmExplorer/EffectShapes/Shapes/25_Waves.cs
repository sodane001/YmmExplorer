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
//  波
//   横から見た海の波。遠近の層・うねり・白波の泡
// =====================================================================
public sealed class WavesPlugin : IShapePlugin
{
    public string Name => "演出：波";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WavesParameter(sharedData);
}

public sealed class WavesParameter : EffectShapeParameterBase
{
    public WavesParameter() : this(null) { }
    public WavesParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "遠い波の色")]
    [ColorPicker]
    public WpfColor ShallowColor { get => shallow; set => Set(ref shallow, value); }
    WpfColor shallow = WpfColor.FromRgb(0x4F, 0xC1, 0xE0);

    [Display(GroupName = "色", Name = "手前の波の色")]
    [ColorPicker]
    public WpfColor DeepColor { get => deep; set => Set(ref deep, value); }
    WpfColor deep = WpfColor.FromRgb(0x0A, 0x3A, 0x66);

    [Display(GroupName = "色", Name = "白波の色")]
    [ColorPicker]
    public WpfColor FoamColor { get => foam; set => Set(ref foam, value); }
    WpfColor foam = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "波", Name = "層の数")]
    [AnimationSlider("F0", "層", 1, 8)]
    public Animation Layers { get; } = new Animation(4, 1, 20);

    [Display(GroupName = "波", Name = "水面の高さ位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation WaterLine { get; } = new Animation(55, -100, 200);

    [Display(GroupName = "波", Name = "波の高さ")]
    [AnimationSlider("F0", "px", 5, 150)]
    public Animation WaveHeight { get; } = new Animation(42, 1, 1000);

    [Display(GroupName = "波", Name = "波の長さ")]
    [AnimationSlider("F0", "px", 150, 1200)]
    public Animation Wavelength { get; } = new Animation(520, 20, 8000);

    [Display(GroupName = "波", Name = "うねりの速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "波", Name = "荒れ具合")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Choppy { get; } = new Animation(60, 0, 500);

    [Display(GroupName = "波", Name = "白波の量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Foam { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "波", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Layers, WaterLine, WaveHeight, Wavelength, Speed, Choppy, Foam, Opacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float ts = c.Time * c.F(Speed) / 100f;
        int L = Math.Clamp((int)c.F(Layers), 1, 8);
        float wl = c.F(WaterLine) / 100f * h, A = c.F(WaveHeight), lam = MathF.Max(40f, c.F(Wavelength));
        float chop = c.F(Choppy) / 100f, foamK = c.F(Foam) / 100f, op = c.F(Opacity) / 100f;
        float spacing = (h - wl) / MathF.Max(1f, L + 0.5f);
        const float step = 6f;

        using var brush = dc.CreateSolidColorBrush(default);
        for (int k = 0; k < L; k++)
        {
            float f = L == 1 ? 1f : k / (float)(L - 1);                   // 0=遠い 1=手前
            float baseY = wl + k * spacing + spacing * 0.3f;
            float amp = A * (0.35f + 0.65f * f);
            float lk = lam * (0.6f + 0.6f * f);
            float ph = k * 1.9f + Seed * 0.1f;
            float dirSign = (k % 2 == 0) ? 1f : -1f;
            var col = Gfx.Mix(ShallowColor, DeepColor, f * 0.85f);

            var crest = new List<Vector2>();
            var rf = new Random(Seed * 131 + k * 17);
            for (float x = -step; x <= w + step; x += step)
            {
                float phi = x / lk * MathF.PI * 2f + ts * (0.9f + 0.4f * f) * dirSign + ph;
                float shape = MathF.Sin(phi) + chop * 0.35f * MathF.Sin(2f * phi + 0.8f) + chop * 0.18f * MathF.Sin(3.1f * phi + 2f);
                float y = baseY - amp * shape * 0.6f;
                crest.Add(new Vector2(x, y));

                brush.Color = Gfx.C4(col, op);
                Gfx.Rect(dc, brush, x, y, step + 1f, h - y);

                // 白波の泡（波頭付近に出る）
                if (foamK > 0f && MathF.Sin(phi) > 0.82f && rf.NextF() < foamK * 0.6f)
                {
                    float r = (2f + 5f * rf.NextF()) * (0.6f + 0.6f * f);
                    brush.Color = Gfx.C4(FoamColor, (0.35f + 0.5f * rf.NextF()) * op);
                    dc.FillEllipse(new Ellipse(new Vector2(x + rf.NextF() * step, y + rf.NextF() * 6f), r, r * 0.7f), brush);
                }
            }
            // 波頭の明るい線
            brush.Color = Gfx.C4(Gfx.Mix(col, FoamColor, 0.55f), 0.7f * op);
            FormKit.Stroke(dc, brush, crest, 1.5f + 1.8f * f, false);
        }

        // 奥行きの暗がり
        using var gb = Gfx.Linear(dc, new Vector2(0, wl), new Vector2(0, h),
            (0f, Gfx.C4(DeepColor, 0f)), (1f, Gfx.C4(DeepColor, 0.35f * op)));
        Gfx.Rect(dc, gb, 0, wl, w, h - wl);
    }
}
