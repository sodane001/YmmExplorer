using System.ComponentModel.DataAnnotations;
using System.Numerics;
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
//  夜空
//   グラデーション + 地平線の光 + 天の川 + 星 + 月
// =====================================================================
public sealed class NightSkyPlugin : IShapePlugin
{
    public string Name => "演出：夜空";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NightSkyParameter(sharedData);
}

public sealed class NightSkyParameter : EffectShapeParameterBase
{
    public NightSkyParameter() : this(null) { }
    public NightSkyParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "空", Name = "天頂の色")]
    [ColorPicker]
    public WpfColor TopColor { get => top; set => Set(ref top, value); }
    WpfColor top = WpfColor.FromRgb(0x04, 0x08, 0x1C);

    [Display(GroupName = "空", Name = "地平線の色")]
    [ColorPicker]
    public WpfColor HorizonColor { get => hor; set => Set(ref hor, value); }
    WpfColor hor = WpfColor.FromRgb(0x1D, 0x2C, 0x5A);

    [Display(GroupName = "空", Name = "地平線の光の色")]
    [ColorPicker]
    public WpfColor GlowColor { get => glow; set => Set(ref glow, value); }
    WpfColor glow = WpfColor.FromRgb(0xE0, 0x80, 0x5C);

    [Display(GroupName = "空", Name = "地平線の光の強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation GlowStrength { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "空", Name = "地平線の光のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation GlowX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "星", Name = "星の数")]
    [AnimationSlider("F0", "個", 0, 1500)]
    public Animation StarCount { get; } = new Animation(450, 0, 20000);

    [Display(GroupName = "星", Name = "星の明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation StarBrightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "天の川", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation MilkyWay { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "天の川", Name = "角度")]
    [AnimationSlider("F0", "°", -90, 90)]
    public Animation MilkyAngle { get; } = new Animation(-35, -360, 360);

    [Display(GroupName = "天の川", Name = "幅")]
    [AnimationSlider("F0", "px", 80, 800)]
    public Animation MilkyWidth { get; } = new Animation(260, 10, 5000);

    [Display(GroupName = "月", Name = "月の半径", Description = "0で表示しない")]
    [AnimationSlider("F0", "px", 0, 300)]
    public Animation MoonSize { get; } = new Animation(60, 0, 2000);

    [Display(GroupName = "月", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation MoonX { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "月", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation MoonY { get; } = new Animation(22, -100, 200);

    [Display(GroupName = "月", Name = "月の色")]
    [ColorPicker]
    public WpfColor MoonColor { get => moon; set => Set(ref moon, value); }
    WpfColor moon = WpfColor.FromRgb(0xF4, 0xF1, 0xE2);

    [Display(GroupName = "月", Name = "光（にじみ）の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation MoonGlow { get; } = new Animation(60, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [GlowStrength, GlowX, StarCount, StarBrightness, MilkyWay, MilkyAngle, MilkyWidth, MoonSize, MoonX, MoonY, MoonGlow];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 1. 空のグラデーション ----
        using (var g = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, h),
            (0f, Gfx.C4(TopColor)),
            (0.55f, Gfx.C4(Gfx.Mix(TopColor, HorizonColor, 0.45f))),
            (1f, Gfx.C4(HorizonColor))))
            Gfx.Rect(dc, g, 0, 0, w, h);

        // ---- 2. 地平線の光 ----
        float gs = c.F(GlowStrength) / 100f;
        if (gs > 0f)
        {
            var gc = new Vector2(c.F(GlowX) / 100f * w, h * 1.02f);
            float gr = MathF.Max(w, h) * 0.7f;
            using var gb = Gfx.Radial(dc, gc, gr,
                (0f, Gfx.C4(GlowColor, 0.6f * gs)),
                (0.5f, Gfx.C4(GlowColor, 0.2f * gs)),
                (1f, Gfx.C4(GlowColor, 0f)));
            Gfx.Rect(dc, gb, 0, 0, w, h);
        }

        // ---- 3. 天の川 ----
        float mw = c.F(MilkyWay) / 100f;
        if (mw > 0f)
        {
            float ang = Gfx.Rad(c.F(MilkyAngle));
            var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
            var perp = new Vector2(-dir.Y, dir.X);
            var mc = new Vector2(w / 2f, h / 2f);
            float diag = MathF.Sqrt(w * w + h * h);
            float bw = c.F(MilkyWidth);
            var rm = new Random(Seed + 21);

            var tintA = WpfColor.FromRgb(0xB8, 0xC8, 0xFF);
            var tintB = WpfColor.FromRgb(0xD7, 0xB8, 0xFF);
            for (int i = 0; i < 14; i++)
            {
                float s = (i / 13f - 0.5f) * diag * 0.9f;
                var p = mc + dir * s + perp * ((rm.NextF() - 0.5f) * bw * 0.25f);
                float r = bw * (0.6f + 0.5f * rm.NextF());
                var col = (i % 2 == 0) ? tintA : tintB;
                using var b = Gfx.Radial(dc, p, r, (0f, Gfx.C4(col, 0.09f * mw)), (1f, Gfx.C4(col, 0f)));
                dc.FillEllipse(new Ellipse(p, r, r), b);
            }
            for (int i = 0; i < 6; i++)          // 暗黒星雲（暗い筋）
            {
                float s = (rm.NextF() - 0.5f) * diag * 0.7f;
                var p = mc + dir * s + perp * ((rm.NextF() - 0.5f) * bw * 0.4f);
                float r = bw * (0.25f + 0.25f * rm.NextF());
                using var b = Gfx.Radial(dc, p, r, (0f, Gfx.C4(TopColor, 0.30f * mw)), (1f, Gfx.C4(TopColor, 0f)));
                dc.FillEllipse(new Ellipse(p, r, r), b);
            }
            for (int i = 0; i < 600; i++)        // 細かい星屑
            {
                float s = (rm.NextF() - 0.5f) * diag;
                float v = (rm.NextF() + rm.NextF() + rm.NextF() - 1.5f) * bw * 0.6f;
                var p = mc + dir * s + perp * v;
                float edge = 1f - MathF.Min(1f, MathF.Abs(v) / (bw * 0.9f));
                float r = 0.5f + rm.NextF() * 0.9f;
                brush.Color = Gfx.C4(tintA, (0.15f + 0.45f * rm.NextF()) * mw * edge);
                dc.FillEllipse(new Ellipse(p, r, r), brush);
            }
        }

        // ---- 4. 星（地平線付近は薄くする）----
        int n = (int)c.F(StarCount);
        float bright = c.F(StarBrightness) / 100f;
        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float x = rng.NextF() * w, y = rng.NextF() * h;
            float m = rng.NextF(); m = m * m * m;
            float ph = rng.NextF() * MathF.PI * 2f, ts = 0.5f + rng.NextF() * 2f;
            float r = 0.6f + 2.2f * m;
            float tw = 0.65f + 0.35f * MathF.Sin(t * 2f * ts + ph);
            float fade = 1f - 0.8f * Gfx.Smooth(0.6f, 1.0f, y / h);
            float a = (0.3f + 0.7f * m) * tw * bright * fade;
            var ctr = new Vector2(x, y);
            brush.Color = Gfx.C4(255 / 255f, 255 / 255f, 255 / 255f, a * 0.15f);
            dc.FillEllipse(new Ellipse(ctr, r * 2.4f, r * 2.4f), brush);
            brush.Color = Gfx.C4(255 / 255f, 255 / 255f, 255 / 255f, a);
            dc.FillEllipse(new Ellipse(ctr, r, r), brush);
        }

        // ---- 5. 月 ----
        float mr = c.F(MoonSize);
        if (mr > 0.5f)
        {
            var mp = new Vector2(c.F(MoonX) / 100f * w, c.F(MoonY) / 100f * h);
            float glowK = c.F(MoonGlow) / 100f;
            float gr = mr * 5f;
            using (var gb = Gfx.Radial(dc, mp, gr,
                (0f, Gfx.C4(MoonColor, 0.55f * glowK)),
                (0.2f, Gfx.C4(MoonColor, 0.25f * glowK)),
                (1f, Gfx.C4(MoonColor, 0f))))
                dc.FillEllipse(new Ellipse(mp, gr, gr), gb);

            brush.Color = Gfx.C4(MoonColor);
            dc.FillEllipse(new Ellipse(mp, mr, mr), brush);

            // 立体感（左上が明るく右下が暗い）
            var lit = new Vector2(mp.X - mr * 0.3f, mp.Y - mr * 0.3f);
            using (var sh = Gfx.Radial(dc, lit, mr * 1.7f,
                (0f, Gfx.C4(1f, 1f, 1f, 0.25f)),
                (0.6f, Gfx.C4(0f, 0f, 0f, 0.05f)),
                (1f, Gfx.C4(0f, 0f, 0.1f, 0.45f))))
                dc.FillEllipse(new Ellipse(mp, mr, mr), sh);

            // クレーター
            var rc = new Random(Seed + 31);
            var crater = Gfx.Mix(MoonColor, WpfColor.FromRgb(0x50, 0x50, 0x60), 0.6f);
            for (int i = 0; i < 8; i++)
            {
                float ca = rc.NextF() * MathF.PI * 2f;
                float cd = MathF.Sqrt(rc.NextF()) * mr * 0.7f;
                float cr = mr * (0.06f + 0.14f * rc.NextF());
                var cp = mp + new Vector2(MathF.Cos(ca), MathF.Sin(ca)) * cd;
                brush.Color = Gfx.C4(crater, 0.22f);
                dc.FillEllipse(new Ellipse(cp, cr, cr), brush);
            }
        }
    }
}
