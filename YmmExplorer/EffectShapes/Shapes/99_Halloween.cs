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
//  ハロウィン
//   コウモリの群れ、ジャック・オー・ランタン、幽霊
// =====================================================================
public sealed class HalloweenPlugin : IShapePlugin
{
    public string Name => "演出：ハロウィン";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HalloweenParameter(sharedData);
}

public sealed class HalloweenParameter : EffectShapeParameterBase
{
    public HalloweenParameter() : this(null) { }
    public HalloweenParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "コウモリ", Name = "コウモリの数")]
    [AnimationSlider("F0", "匹", 0, 60)]
    public Animation BatCount { get; } = new Animation(14, 0, 500);

    [Display(GroupName = "コウモリ", Name = "コウモリの色")]
    [ColorPicker]
    public WpfColor BatColor { get => batColor; set => Set(ref batColor, value); }
    WpfColor batColor = WpfColor.FromRgb(0x16, 0x10, 0x1C);

    [Display(GroupName = "かぼちゃ", Name = "かぼちゃの数")]
    [AnimationSlider("F0", "個", 0, 8)]
    public Animation PumpkinCount { get; } = new Animation(3, 0, 50);

    [Display(GroupName = "かぼちゃ", Name = "かぼちゃの色")]
    [ColorPicker]
    public WpfColor PumpkinColor { get => pumpkinColor; set => Set(ref pumpkinColor, value); }
    WpfColor pumpkinColor = WpfColor.FromRgb(0xFF, 0x7A, 0x1A);

    [Display(GroupName = "幽霊", Name = "幽霊の数")]
    [AnimationSlider("F0", "体", 0, 12)]
    public Animation GhostCount { get; } = new Animation(3, 0, 100);

    [Display(GroupName = "幽霊", Name = "幽霊の色")]
    [ColorPicker]
    public WpfColor GhostColor { get => ghostColor; set => Set(ref ghostColor, value); }
    WpfColor ghostColor = WpfColor.FromRgb(0xF4, 0xF4, 0xFF);

    [Display(GroupName = "全体", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 300)]
    public Animation ItemScale { get; } = new Animation(100, 5, 2000);

    [Display(GroupName = "全体", Name = "動きの速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "全体", Name = "大きな月を表示する")]
    [ToggleSlider]
    public bool Moon { get => moon; set => Set(ref moon, value); }
    bool moon = true;

    [Display(GroupName = "全体", Name = "夜空を描く")]
    [ToggleSlider]
    public bool Sky { get => sky; set => Set(ref sky, value); }
    bool sky = false;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [BatCount, PumpkinCount, GhostCount, ItemScale, Speed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time, tt = c.Time * c.F(Speed) / 100f;
        float S = MathF.Min(w, h) / 1080f * c.F(ItemScale) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (Sky)
            {
                using var g = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, h), (0f, Gfx.C4(0.06f, 0.02f, 0.12f, 1f)), (1f, Gfx.C4(0.28f, 0.1f, 0.22f, 1f)));
                dc.FillRectangle(new RawRectF(0, 0, w, h), g);
            }

            // ---- 月 ----
            if (Moon)
            {
                var mp = new Vector2(w * 0.78f, h * 0.24f);
                float mr = 140f * S;
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, mp, mr * 3f, Gfx.C4(1f, 0.85f, 0.5f, 0.3f));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                using (var g = Gfx.Radial(dc, mp - new Vector2(mr * 0.2f, mr * 0.2f), mr * 1.3f, (0f, Gfx.C4(1f, 0.97f, 0.82f, 1f)), (1f, Gfx.C4(0.95f, 0.78f, 0.45f, 1f))))
                    dc.FillEllipse(new Ellipse(mp, mr, mr), g);
                brush.Color = Gfx.C4(0.75f, 0.58f, 0.32f, 0.25f);
                dc.FillEllipse(new Ellipse(mp + new Vector2(-mr * 0.3f, mr * 0.1f), mr * 0.22f, mr * 0.2f), brush);
                dc.FillEllipse(new Ellipse(mp + new Vector2(mr * 0.35f, -mr * 0.3f), mr * 0.14f, mr * 0.13f), brush);
                dc.FillEllipse(new Ellipse(mp + new Vector2(mr * 0.15f, mr * 0.45f), mr * 0.1f, mr * 0.09f), brush);
            }

            // ---- 幽霊 ----
            int ng = (int)c.F(GhostCount);
            for (int i = 0; i < ng; i++)
            {
                float g = 70f * S * (0.8f + 0.4f * DrawKit.H(Seed, i, 10));
                var basePos = new Vector2(w * (0.12f + 0.76f * DrawKit.H(Seed, i, 11)), h * (0.3f + 0.3f * DrawKit.H(Seed, i, 12)));
                var p = FormKit.Liss(basePos, w * 0.08f, h * 0.05f, tt, 0.25f + 0.1f * DrawKit.H(Seed, i, 13), 0.5f, i * 2f, i);
                float alpha = 0.75f + 0.2f * MathF.Sin(t * 2.5f + i * 2f);
                var body = new List<Vector2>();
                for (int k = 0; k <= 16; k++)
                {
                    float a = MathF.PI + MathF.PI * k / 16f;
                    body.Add(p + new Vector2(MathF.Cos(a) * g, MathF.Sin(a) * g));
                }
                body.Add(p + new Vector2(g * 1.05f, g * 1.1f));
                for (int k = 0; k <= 24; k++)
                {
                    float u = k / 24f;
                    float x = g * 1.05f - u * g * 2.1f;
                    float y = g * 1.1f + MathF.Sin(u * MathF.PI * 6f + tt * 5f + i) * g * 0.12f + (MathF.Sin(u * MathF.PI * 3f) > 0 ? g * 0.1f : 0f);
                    body.Add(p + new Vector2(x, y));
                }
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, p + new Vector2(0, g * 0.3f), g * 2f, Gfx.C4(GhostColor, 0.15f * alpha));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                brush.Color = Gfx.C4(GhostColor, 0.85f * alpha);
                Gfx.FillPoly(dc, brush, body);
                brush.Color = Gfx.C4(0.05f, 0.04f, 0.1f, 0.9f * alpha);
                dc.FillEllipse(new Ellipse(p + new Vector2(-g * 0.32f, -g * 0.1f), g * 0.13f, g * 0.2f), brush);
                dc.FillEllipse(new Ellipse(p + new Vector2(g * 0.32f, -g * 0.1f), g * 0.13f, g * 0.2f), brush);
                float mo = 0.12f + 0.06f * MathF.Sin(t * 3f + i);
                dc.FillEllipse(new Ellipse(p + new Vector2(0, g * 0.35f), g * 0.12f, g * mo + 1f), brush);
            }

            // ---- かぼちゃ ----
            int np = (int)c.F(PumpkinCount);
            for (int i = 0; i < np; i++)
            {
                float P = 95f * S * (0.75f + 0.5f * DrawKit.H(Seed, i, 20));
                float x = w * (i + 0.5f) / Math.Max(1, np) + (DrawKit.H(Seed, i, 21) - 0.5f) * w * 0.1f;
                var p = new Vector2(x, h - P * 0.85f - h * 0.02f);
                DrawPumpkin(dc, brush, p, P, t + i * 1.7f);
            }

            // ---- コウモリ ----
            int nb = (int)c.F(BatCount);
            for (int i = 0; i < nb; i++)
            {
                float bs = 46f * S * (0.6f + 0.8f * DrawKit.H(Seed, i, 30));
                bool toRight = DrawKit.H(Seed, i, 31) > 0.5f;
                float travel = w + bs * 6f;
                float u = DrawKit.Frac(tt * (0.05f + 0.06f * DrawKit.H(Seed, i, 32)) + DrawKit.H(Seed, i, 33));
                float x = toRight ? -bs * 3f + u * travel : w + bs * 3f - u * travel;
                float y = h * (0.08f + 0.45f * DrawKit.H(Seed, i, 34)) + MathF.Sin(tt * 2.2f + i) * bs * 0.8f;
                float flap = MathF.Sin(tt * 15f * (0.8f + 0.4f * DrawKit.H(Seed, i, 35)) + i);
                DrawBat(dc, brush, new Vector2(x, y), bs, flap, toRight ? 1f : -1f);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    void DrawBat(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 p, float s, float flap, float dir)
    {
        float up = flap * 0.55f;
        var wing = new List<Vector2>
        {
            new(0.12f, -0.05f), new(0.45f, -0.35f - up * 0.4f), new(1f, -0.25f - up),
            new(0.85f, 0.05f - up * 0.4f), new(0.72f, 0.0f - up * 0.25f), new(0.58f, 0.12f - up * 0.2f),
            new(0.42f, 0.05f - up * 0.1f), new(0.28f, 0.16f), new(0.12f, 0.1f),
        };
        brush.Color = Gfx.C4(BatColor, 0.95f);
        var right = wing.Select(v => p + new Vector2(v.X * dir, v.Y) * s).ToList();
        var left = wing.Select(v => p + new Vector2(-v.X * dir, v.Y) * s).ToList();
        Gfx.FillPoly(dc, brush, right);
        Gfx.FillPoly(dc, brush, left);
        dc.FillEllipse(new Ellipse(p, s * 0.13f, s * 0.2f), brush);
        dc.FillEllipse(new Ellipse(p + new Vector2(0, -s * 0.17f), s * 0.1f, s * 0.09f), brush);
        Gfx.FillPoly(dc, brush, new List<Vector2> { p + new Vector2(-s * 0.09f, -s * 0.2f), p + new Vector2(-s * 0.06f, -s * 0.34f), p + new Vector2(-s * 0.02f, -s * 0.22f) });
        Gfx.FillPoly(dc, brush, new List<Vector2> { p + new Vector2(s * 0.09f, -s * 0.2f), p + new Vector2(s * 0.06f, -s * 0.34f), p + new Vector2(s * 0.02f, -s * 0.22f) });
        brush.Color = Gfx.C4(1f, 0.25f, 0.2f, 0.9f);
        dc.FillEllipse(new Ellipse(p + new Vector2(-s * 0.04f, -s * 0.18f), s * 0.018f + 0.5f, s * 0.018f + 0.5f), brush);
        dc.FillEllipse(new Ellipse(p + new Vector2(s * 0.04f, -s * 0.18f), s * 0.018f + 0.5f, s * 0.018f + 0.5f), brush);
    }

    void DrawPumpkin(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 p, float P, float t)
    {
        var dark = Gfx.Mix(PumpkinColor, WpfColor.FromRgb(0x40, 0x10, 0x00), 0.45f);
        var light = Gfx.Mix(PumpkinColor, WpfColor.FromRgb(0xFF, 0xE0, 0x80), 0.35f);
        // 影
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f);
        dc.FillEllipse(new Ellipse(p + new Vector2(0, P * 0.78f), P * 1.1f, P * 0.15f), brush);
        // ふくらみ（外側から）
        float[] xs = { -0.55f, 0.55f, -0.28f, 0.28f, 0f };
        float[] rx = { 0.55f, 0.55f, 0.5f, 0.5f, 0.45f };
        for (int k = 0; k < xs.Length; k++)
        {
            var cc = p + new Vector2(xs[k] * P, 0);
            using var g = Gfx.Radial(dc, cc - new Vector2(P * 0.15f, P * 0.25f), P * 0.95f, (0f, Gfx.C4(light, 1f)), (0.6f, Gfx.C4(PumpkinColor, 1f)), (1f, Gfx.C4(dark, 1f)));
            dc.FillEllipse(new Ellipse(cc, rx[k] * P, P * 0.78f), g);
        }
        // へた
        brush.Color = Gfx.C4(0.3f, 0.36f, 0.12f, 1f);
        Gfx.FillPoly(dc, brush, new List<Vector2> { p + new Vector2(-P * 0.07f, -P * 0.7f), p + new Vector2(P * 0.08f, -P * 0.72f), p + new Vector2(P * 0.16f, -P * 1.0f), p + new Vector2(P * 0.05f, -P * 1.02f) });
        // 顔（中のろうそくでゆらめく）
        float flick = 0.8f + 0.2f * MathF.Sin(t * 13f) * MathF.Sin(t * 7.1f);
        var glowC = Gfx.Mix(WpfColor.FromRgb(0xFF, 0xD2, 0x40), WpfColor.FromRgb(0xFF, 0xF8, 0xC0), 0.3f);
        var eyeL = new List<Vector2> { p + new Vector2(-P * 0.42f, -P * 0.05f), p + new Vector2(-P * 0.12f, -P * 0.05f), p + new Vector2(-P * 0.27f, -P * 0.35f) };
        var eyeR = new List<Vector2> { p + new Vector2(P * 0.12f, -P * 0.05f), p + new Vector2(P * 0.42f, -P * 0.05f), p + new Vector2(P * 0.27f, -P * 0.35f) };
        var nose = new List<Vector2> { p + new Vector2(-P * 0.07f, P * 0.12f), p + new Vector2(P * 0.07f, P * 0.12f), p + new Vector2(0, -P * 0.0f) };
        var mouth = new List<Vector2>();
        for (int k = 0; k <= 8; k++)
        {
            float u = k / 8f;
            float x = (-0.5f + u) * P * 1.05f;
            float y = P * 0.25f + MathF.Sin(u * MathF.PI) * P * 0.18f + ((k % 2 == 1 && k > 1 && k < 7) ? -P * 0.09f : 0f);
            mouth.Add(p + new Vector2(x, y));
        }
        for (int k = 8; k >= 0; k--)
        {
            float u = k / 8f;
            float x = (-0.5f + u) * P * 0.95f;
            float y = P * 0.3f + MathF.Sin(u * MathF.PI) * P * 0.35f + ((k % 2 == 0 && k > 1 && k < 7) ? P * 0.08f : 0f);
            mouth.Add(p + new Vector2(x, y));
        }
        brush.Color = Gfx.C4(glowC, flick);
        Gfx.FillPoly(dc, brush, eyeL);
        Gfx.FillPoly(dc, brush, eyeR);
        Gfx.FillPoly(dc, brush, nose);
        Gfx.FillPoly(dc, brush, mouth);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        DrawKit.Glow(dc, p + new Vector2(0, P * 0.1f), P * 1.4f, Gfx.C4(1f, 0.6f, 0.15f, 0.3f * flick));
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
    }
}
