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
//  グリッチ
//   乱れる帯・RGBずれ・ブロックノイズ・走査線・砂嵐。ときどきバースト的に起こる
// =====================================================================
public sealed class GlitchPlugin : IShapePlugin
{
    public string Name => "演出：グリッチ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GlitchParameter(sharedData);
}

public sealed class GlitchParameter : EffectShapeParameterBase
{
    public GlitchParameter() : this(null) { }
    public GlitchParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "グリッチ", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Intensity { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "グリッチ", Name = "バーストの頻度")]
    [AnimationSlider("F1", "回/秒", 0.5, 20)]
    public Animation Rate { get; } = new Animation(6, 0.1, 100);

    [Display(GroupName = "グリッチ", Name = "起こる確率")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Chance { get; } = new Animation(55, 0, 100);

    [Display(GroupName = "要素", Name = "乱れる帯")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strips { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "要素", Name = "RGBずれ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RgbShift { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "要素", Name = "ブロックノイズ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Blocks { get; } = new Animation(50, 0, 100);

    [Display(GroupName = "要素", Name = "砂嵐")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Noise { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "要素", Name = "走査線（常に表示）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Scanlines { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "要素", Name = "フラッシュ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flash { get; } = new Animation(20, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Intensity, Rate, Chance, Strips, RgbShift, Blocks, Noise, Scanlines, Flash];

    static readonly WpfColor[] Pal =
    {
        WpfColor.FromRgb(0, 255, 255), WpfColor.FromRgb(255, 0, 255), WpfColor.FromRgb(255, 255, 255),
        WpfColor.FromRgb(255, 40, 60), WpfColor.FromRgb(40, 255, 120), WpfColor.FromRgb(0, 0, 0),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float I = c.F(Intensity) / 100f;
        float rate = c.F(Rate), chance = c.F(Chance) / 100f;
        float strips = c.F(Strips) / 100f, rgb = c.F(RgbShift) / 100f, blocks = c.F(Blocks) / 100f;
        float noise = c.F(Noise) / 100f, scan = c.F(Scanlines) / 100f, flash = c.F(Flash) / 100f;

        using var brush = dc.CreateSolidColorBrush(default);

        // 走査線は常に表示
        if (scan > 0f)
        {
            brush.Color = Gfx.C4(WpfColor.FromRgb(0, 0, 0), 0.22f * scan);
            for (float y = 0f; y < h; y += 4f) Gfx.Rect(dc, brush, 0, y, w, 1.5f);
        }

        // バーストの発生判定
        int burst = (int)MathF.Floor(t * rate);
        if (Gfx.Hash01(Seed * 31 + burst * 7) > chance || I <= 0f) return;

        int tick = (int)MathF.Floor(t * 24f);
        var rng = new Random(Seed * 7919 + tick * 131);

        // ---- 乱れる帯とRGBずれ ----
        int ns = (int)(strips * (4f + 14f * I));
        for (int i = 0; i < ns; i++)
        {
            float y = rng.NextF() * h;
            float hh = 2f + rng.NextF() * rng.NextF() * 70f * I;
            float x0 = rng.NextF() * w * 0.35f, x1 = w - rng.NextF() * w * 0.35f;
            var col = Pal[rng.Next(0, Pal.Length)];
            brush.Color = Gfx.C4(col, 0.22f + 0.4f * rng.NextF());
            Gfx.Rect(dc, brush, x0, y, x1 - x0, hh);
            if (rgb > 0f)
            {
                float dx = (6f + 40f * rng.NextF()) * rgb * I * (rng.NextF() < 0.5f ? -1f : 1f);
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                brush.Color = Gfx.C4(WpfColor.FromRgb(255, 0, 40), 0.35f);
                Gfx.Rect(dc, brush, x0 + dx, y, x1 - x0, hh);
                brush.Color = Gfx.C4(WpfColor.FromRgb(0, 220, 255), 0.35f);
                Gfx.Rect(dc, brush, x0 - dx, y, x1 - x0, hh);
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }

        // ---- ブロックノイズ ----
        int nb = (int)(blocks * 30f * I);
        for (int i = 0; i < nb; i++)
        {
            float s = 8f + rng.NextF() * 70f;
            float x = rng.NextF() * w, y = rng.NextF() * h;
            brush.Color = Gfx.C4(Pal[rng.Next(0, Pal.Length)], 0.25f + 0.4f * rng.NextF());
            Gfx.Rect(dc, brush, x, y, s * (0.6f + rng.NextF() * 1.6f), s * 0.5f);
        }

        // ---- 砂嵐 ----
        int nn = (int)(noise * 1800f * I * (w * h / (1920f * 1080f)));
        for (int i = 0; i < nn; i++)
        {
            float x = rng.NextF() * w, y = rng.NextF() * h;
            float s = 1f + rng.NextF() * 2f;
            brush.Color = Gfx.C4(rng.NextF() < 0.5f ? WpfColor.FromRgb(255, 255, 255) : WpfColor.FromRgb(0, 0, 0), 0.15f + 0.4f * rng.NextF());
            Gfx.Rect(dc, brush, x, y, s * 2f, s);
        }

        // ---- フラッシュ ----
        if (flash > 0f && rng.NextF() < 0.25f)
        {
            brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.18f * flash);
            Gfx.Rect(dc, brush, 0, 0, w, h);
        }
    }
}
