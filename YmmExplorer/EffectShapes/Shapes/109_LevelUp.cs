using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
//  LEVEL UP 表示
//   足元の輪から光の柱が立ち、LEVEL UP の文字が弾んで出る
// =====================================================================
public sealed class LevelUpPlugin : IShapePlugin
{
    public string Name => "演出：LEVEL UP 表示";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LevelUpParameter(sharedData);
}

public sealed class LevelUpParameter : EffectShapeParameterBase
{
    public LevelUpParameter() : this(null) { }
    public LevelUpParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "光の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xD2, 0x3C);

    [Display(GroupName = "色", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(0xFF, 0xF6, 0xC8);

    [Display(GroupName = "位置と大きさ", Name = "足元のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "足元のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 300)]
    public Animation SizePct { get; } = new Animation(100, 5, 2000);

    [Display(GroupName = "文字", Name = "LEVEL UP の文字を出す")]
    [ToggleSlider]
    public bool ShowText { get => showText; set => Set(ref showText, value); }
    bool showText = true;

    [Display(GroupName = "文字", Name = "レベルの数字も出す")]
    [ToggleSlider]
    public bool ShowLevel { get => showLevel; set => Set(ref showLevel, value); }
    bool showLevel = true;

    [Display(GroupName = "文字", Name = "上がった後のレベル")]
    [AnimationSlider("F0", "", 1, 99)]
    public Animation Level { get; } = new Animation(2, 0, 9999);

    [Display(GroupName = "動き", Name = "光の柱が立つ時間")]
    [AnimationSlider("F2", "秒", 0.1, 2)]
    public Animation RiseTime { get; } = new Animation(0.5, 0.02, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, SizePct, Level, RiseTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var feet = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float S = MathF.Min(w, h) / 1080f * c.F(SizePct) / 100f;
        float rise = MathF.Max(0.02f, c.F(RiseTime));
        float k = DrawKit.EaseOut(t / rise);
        float fade = Math.Clamp(DrawKit.TimeLeft(c) / 0.4f, 0f, 1f);
        float R = 170f * S, H = 560f * S;
        var white = WpfColor.FromRgb(255, 255, 255);
        var hot = Gfx.Mix(Color, white, 0.6f);

        using var brush = dc.CreateSolidColorBrush(default);
        var prev = dc.Transform;
        try
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            // ---- 足元の輪 ----
            dc.Transform = Matrix3x2.CreateScale(1f, 0.28f, feet) * prev;
            DrawKit.Glow(dc, feet, R * 1.4f, Gfx.C4(Color, 0.5f * fade));
            for (int i = 0; i < 3; i++)
            {
                float u = DrawKit.Frac(t * 0.8f + i / 3f);
                brush.Color = Gfx.C4(hot, (1f - u) * 0.8f * fade);
                dc.DrawEllipse(new Ellipse(feet, R * (0.4f + 0.9f * u), R * (0.4f + 0.9f * u)), brush, 6f * S + 1f);
            }
            brush.Color = Gfx.C4(Color, 0.9f * fade);
            dc.DrawEllipse(new Ellipse(feet, R, R), brush, 5f * S + 1f);
            dc.Transform = prev;

            // ---- 光の柱 ----
            float ph = H * k;
            float pulse = 0.85f + 0.15f * MathF.Sin(t * 6f);
            using (var lg = Gfx.Linear(dc, feet, feet - new Vector2(0, ph), (0f, Gfx.C4(Color, 0.55f * fade * pulse)), (0.7f, Gfx.C4(Color, 0.25f * fade)), (1f, Gfx.C4(Color, 0f))))
                dc.FillRectangle(new RawRectF(feet.X - R * 0.8f, feet.Y - ph, feet.X + R * 0.8f, feet.Y), lg);
            using (var lg = Gfx.Linear(dc, feet, feet - new Vector2(0, ph), (0f, Gfx.C4(white, 0.6f * fade)), (1f, Gfx.C4(white, 0f))))
                dc.FillRectangle(new RawRectF(feet.X - R * 0.18f, feet.Y - ph, feet.X + R * 0.18f, feet.Y), lg);

            // ---- 上がる光の粒 ----
            var rng = new Random(Seed);
            for (int i = 0; i < 40; i++)
            {
                float ox = (rng.NextF() - 0.5f) * 2f * R * 0.9f, sp = 0.4f + 0.6f * rng.NextF(), off = rng.NextF(), sz = (2f + 4f * rng.NextF()) * S + 1f;
                float u = DrawKit.Frac(t * sp * 0.8f + off);
                var p = feet + new Vector2(ox + MathF.Sin(t * 3f + i) * 8f * S, -u * H * k);
                float a = MathF.Sin(MathF.PI * u) * fade;
                DrawKit.Glow(dc, p, sz * 4f, Gfx.C4(Color, 0.5f * a));
                brush.Color = Gfx.C4(white, 0.9f * a);
                dc.FillEllipse(new Ellipse(p, sz * 0.6f, sz * 0.6f), brush);
            }
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

            // ---- 文字 ----
            float tt = t - rise * 0.6f;
            if (ShowText && tt > 0f)
            {
                float pop = tt < 0.12f ? 0.3f + 0.9f * tt / 0.12f : 1f + 0.25f * MathF.Exp(-(tt - 0.12f) * 7f) * MathF.Cos((tt - 0.12f) * 20f);
                float th = 120f * S * pop;
                var tc = feet - new Vector2(0, H * 0.72f);
                var outline = Gfx.C4(Gfx.Mix(Color, WpfColor.FromRgb(60, 20, 0), 0.7f), fade);
                StrokeFont.DrawOutlined(dc, brush, "LEVEL UP", tc, th, MathF.Max(2.5f, th * 0.12f), Gfx.C4(TextColor, fade), outline, th * 0.3f, 0.16f, 0.12f);
                // 文字を横切る光
                float sweep = DrawKit.Frac(tt * 0.6f);
                if (sweep < 0.4f)
                {
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    float tw = StrokeFont.Width("LEVEL UP", th, 0.16f);
                    float sx = tc.X - tw / 2f + tw * sweep / 0.4f;
                    DrawKit.Glow(dc, new Vector2(sx, tc.Y), th * 0.7f, Gfx.C4(white, 0.6f * fade));
                    dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                }
                if (ShowLevel)
                {
                    int lv = Math.Max(0, (int)MathF.Round(c.F(Level)));
                    string s = "LV " + (lv - 1).ToString(CultureInfo.InvariantCulture) + "  " + lv.ToString(CultureInfo.InvariantCulture);
                    float lh = th * 0.42f / pop;
                    var lp = tc + new Vector2(0, th * 0.95f / pop);
                    StrokeFont.DrawOutlined(dc, brush, s, lp, lh, MathF.Max(2f, lh * 0.12f), Gfx.C4(white, fade), outline, lh * 0.32f, 0.2f);
                    // 矢印（→）
                    float sw = StrokeFont.Width(s, lh, 0.2f);
                    float ax = lp.X - sw / 2f + StrokeFont.Width("LV " + (lv - 1).ToString(CultureInfo.InvariantCulture) + " ", lh, 0.2f);
                    brush.Color = Gfx.C4(Color, fade);
                    Gfx.Polyline(dc, brush, new List<Vector2> { new(ax - lh * 0.1f, lp.Y - lh * 0.25f), new(ax + lh * 0.25f, lp.Y), new(ax - lh * 0.1f, lp.Y + lh * 0.25f) }, MathF.Max(2f, lh * 0.12f));
                }
            }
        }
        finally
        {
            dc.Transform = prev;
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
