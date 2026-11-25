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
//  草原が風で揺れる
//   手前に置く草。風の波が、左から右へ草をなびかせていく
// =====================================================================
public sealed class GrassPlugin : IShapePlugin
{
    public string Name => "演出：草原が風で揺れる";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GrassParameter(sharedData);
}

public sealed class GrassParameter : EffectShapeParameterBase
{
    public GrassParameter() : this(null) { }
    public GrassParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "草", Name = "地面の位置（Y）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BaseY { get; } = new Animation(100, -100, 200);

    [Display(GroupName = "草", Name = "草の高さ")]
    [AnimationSlider("F0", "px", 30, 800)]
    public Animation GrassHeight { get; } = new Animation(230, 1, 5000);

    [Display(GroupName = "草", Name = "草の本数")]
    [TextBoxSlider("F0", "本", 50, 1500)]
    [Range(1, 6000)]
    public int Density { get => density; set => Set(ref density, value); }
    int density = 450;

    [Display(GroupName = "草", Name = "花を咲かせる")]
    [ToggleSlider]
    public bool Flowers { get => flowers; set => Set(ref flowers, value); }
    bool flowers = false;

    [Display(GroupName = "風", Name = "風の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Wind { get; } = new Animation(60, 0, 1000);

    [Display(GroupName = "風", Name = "風の波の速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation WaveSpeed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "色", Name = "先の色")]
    [ColorPicker]
    public WpfColor TipColor { get => tipColor; set => Set(ref tipColor, value); }
    WpfColor tipColor = WpfColor.FromRgb(0x9B, 0xD8, 0x4E);

    [Display(GroupName = "色", Name = "根もとの色")]
    [ColorPicker]
    public WpfColor RootColor { get => rootColor; set => Set(ref rootColor, value); }
    WpfColor rootColor = WpfColor.FromRgb(0x2E, 0x6B, 0x22);

    [Display(GroupName = "色", Name = "シルエット（黒一色）")]
    [ToggleSlider]
    public bool Silhouette { get => silhouette; set => Set(ref silhouette, value); }
    bool silhouette = false;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [BaseY, GrassHeight, Wind, WaveSpeed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float ground = c.F(BaseY) / 100f * h, gh = c.F(GrassHeight), wind = c.F(Wind) / 100f, ws = c.F(WaveSpeed) / 100f;
        int n = Math.Max(1, Density);
        using var brush = dc.CreateSolidColorBrush(default);
        // 奥の草から順に(奥ほど少し暗く・短く)
        for (int layer = 0; layer < 3; layer++)
        {
            float depth = layer / 2f;               // 0=奥 1=手前
            float shade = 0.65f + 0.35f * depth;
            for (int i = layer; i < n; i += 3)
            {
                float x = DrawKit.H(Seed, i, 1) * (w + 40f) - 20f;
                float hgt = gh * (0.55f + DrawKit.H(Seed, i, 2) * 0.55f) * (0.75f + 0.25f * depth);
                float bw = MathF.Max(2f, gh * 0.035f) * (0.7f + DrawKit.H(Seed, i, 3) * 0.6f);
                // 風: ゆっくり大きな波 + 小さなゆれ。先ほど大きく曲がる
                float wave = MathF.Sin(x * 0.006f - t * 1.6f * ws) * 0.6f + MathF.Sin(x * 0.017f - t * 2.7f * ws + i * 0.3f) * 0.25f;
                float lean = (0.25f + wave) * wind * 0.5f + (DrawKit.H(Seed, i, 4) - 0.5f) * 0.3f;
                var root = new Vector2(x, ground + 4f);
                // 1本を、細くなっていく線の折れ線で描く(多角形で塗るより、ずっと軽い)
                const int segs = 6;
                var pts = new Vector2[segs + 1];
                for (int k = 0; k <= segs; k++)
                {
                    float f = k / (float)segs;
                    float bend = lean * f * f;
                    pts[k] = root + new Vector2(MathF.Sin(bend) * hgt * f, -MathF.Cos(bend) * hgt * f);
                }
                if (Silhouette) brush.Color = Gfx.C4(0.03f, 0.04f, 0.05f, 1f);
                else
                {
                    var col = Gfx.Mix(RootColor, TipColor, 0.35f + DrawKit.H(Seed, i, 5) * 0.5f);
                    brush.Color = Gfx.C4(col.R / 255f * shade, col.G / 255f * shade, col.B / 255f * shade, 1f);
                }
                for (int k = 0; k < segs; k++)
                {
                    float ww = bw * (1f - (k + 0.5f) / segs);
                    dc.DrawLine(pts[k], pts[k + 1], brush, MathF.Max(0.6f, ww));
                }
                if (Flowers && layer == 2 && DrawKit.H(Seed, i, 6) < 0.06f)
                {
                    var tip = pts[segs];
                    var fc = Gfx.Hsv(DrawKit.H(Seed, i, 7) * 360f, 0.45f, 1f);
                    brush.Color = Silhouette ? Gfx.C4(0.03f, 0.04f, 0.05f, 1f) : Gfx.C4(fc, 1f);
                    for (int pIdx = 0; pIdx < 5; pIdx++)
                    {
                        float a = pIdx / 5f * MathF.PI * 2f + t * 0.2f;
                        dc.FillEllipse(new Ellipse(tip + new Vector2(MathF.Cos(a), MathF.Sin(a)) * bw * 1.2f, bw * 0.9f, bw * 0.9f), brush);
                    }
                    brush.Color = Silhouette ? Gfx.C4(0.03f, 0.04f, 0.05f, 1f) : Gfx.C4(1f, 0.85f, 0.25f, 1f);
                    dc.FillEllipse(new Ellipse(tip, bw * 0.8f, bw * 0.8f), brush);
                }
            }
        }
        // 地面より下は塗りつぶす(草の根もとのすき間を隠す)
        if (ground < h)
        {
            brush.Color = Silhouette ? Gfx.C4(0.03f, 0.04f, 0.05f, 1f) : Gfx.C4(RootColor, 1f);
            dc.FillRectangle(new RawRectF(0, ground, w, h), brush);
        }
    }
}
