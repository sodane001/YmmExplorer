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
//  レトロウェーブ
//   夕陽のグラデーション、遠近のグリッド、ネオンの山並み
// =====================================================================
public sealed class RetrowavePlugin : IShapePlugin
{
    public string Name => "演出：レトロウェーブ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RetrowaveParameter(sharedData);
}

public sealed class RetrowaveParameter : EffectShapeParameterBase
{
    public RetrowaveParameter() : this(null) { }
    public RetrowaveParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "空", Name = "空の色（上）")]
    [ColorPicker]
    public WpfColor SkyTop { get => skyTop; set => Set(ref skyTop, value); }
    WpfColor skyTop = WpfColor.FromRgb(0x10, 0x02, 0x2E);

    [Display(GroupName = "空", Name = "空の色（地平線）")]
    [ColorPicker]
    public WpfColor SkyBottom { get => skyBottom; set => Set(ref skyBottom, value); }
    WpfColor skyBottom = WpfColor.FromRgb(0xE8, 0x3C, 0x8C);

    [Display(GroupName = "空", Name = "地平線の高さ")]
    [AnimationSlider("F0", "%", 20, 90)]
    public Animation Horizon { get; } = new Animation(58, 0, 100);

    [Display(GroupName = "空", Name = "星を表示する")]
    [ToggleSlider]
    public bool Stars { get => stars; set => Set(ref stars, value); }
    bool stars = true;

    [Display(GroupName = "空", Name = "背景の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BgOpacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "太陽", Name = "太陽の色（上）")]
    [ColorPicker]
    public WpfColor SunTop { get => sunTop; set => Set(ref sunTop, value); }
    WpfColor sunTop = WpfColor.FromRgb(0xFF, 0xE2, 0x3C);

    [Display(GroupName = "太陽", Name = "太陽の色（下）")]
    [ColorPicker]
    public WpfColor SunBottom { get => sunBottom; set => Set(ref sunBottom, value); }
    WpfColor sunBottom = WpfColor.FromRgb(0xFF, 0x2E, 0x8A);

    [Display(GroupName = "太陽", Name = "太陽の大きさ")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation SunSize { get; } = new Animation(230, 0, 5000);

    [Display(GroupName = "太陽", Name = "太陽のしま模様")]
    [ToggleSlider]
    public bool SunStripes { get => sunStripes; set => Set(ref sunStripes, value); }
    bool sunStripes = true;

    [Display(GroupName = "地面", Name = "グリッドの色")]
    [ColorPicker]
    public WpfColor GridColor { get => gridColor; set => Set(ref gridColor, value); }
    WpfColor gridColor = WpfColor.FromRgb(0xFF, 0x3C, 0xE8);

    [Display(GroupName = "地面", Name = "グリッドの間隔")]
    [AnimationSlider("F0", "px", 30, 300)]
    public Animation GridSpacing { get; } = new Animation(110, 5, 3000);

    [Display(GroupName = "地面", Name = "進む速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, -2000, 2000);

    [Display(GroupName = "地面", Name = "ネオンの山並みを表示する")]
    [ToggleSlider]
    public bool Mountains { get => mountains; set => Set(ref mountains, value); }
    bool mountains = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Horizon, BgOpacity, SunSize, GridSpacing, Speed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float hy = c.F(Horizon) / 100f * h, bg = c.F(BgOpacity) / 100f;
        float R = c.F(SunSize), spacing = MathF.Max(5f, c.F(GridSpacing)), speed = c.F(Speed) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // ---- 空 ----
            if (bg > 0f)
            {
                using var sky = Gfx.Linear(dc, new Vector2(0, 0), new Vector2(0, hy), (0f, Gfx.C4(SkyTop, bg)), (0.6f, Gfx.C4(Gfx.Mix(SkyTop, SkyBottom, 0.5f), bg)), (1f, Gfx.C4(SkyBottom, bg)));
                dc.FillRectangle(new RawRectF(0, 0, w, hy), sky);
            }
            if (Stars)
            {
                var rng = new Random(Seed);
                for (int i = 0; i < 160; i++)
                {
                    var p = new Vector2(rng.NextF() * w, rng.NextF() * rng.NextF() * hy * 0.85f);
                    float tw = 0.5f + 0.5f * MathF.Sin(t * (1f + 2f * rng.NextF()) + i);
                    brush.Color = Gfx.C4(white, (0.3f + 0.6f * tw) * (1f - p.Y / hy));
                    float s = 0.6f + 1.2f * rng.NextF();
                    dc.FillEllipse(new Ellipse(p, s, s), brush);
                }
            }

            // ---- 太陽 ----
            if (R > 1f)
            {
                var sc = new Vector2(w / 2f, hy - R * 0.38f);
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, sc, R * 2.1f, Gfx.C4(SunBottom, 0.35f));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                float y0 = sc.Y - R, y1 = MathF.Min(sc.Y + R, hy);
                float stripeStart = sc.Y - R * 0.05f;
                float period = R * 0.17f;
                float scroll = DrawKit.Frac(t * 0.25f) * period;
                for (float y = y0; y < y1; y += 1.5f)
                {
                    float dy = y + 0.75f - sc.Y;
                    float half = MathF.Sqrt(MathF.Max(0f, R * R - dy * dy));
                    if (half < 0.5f) continue;
                    if (SunStripes && y > stripeStart)
                    {
                        float rel = (y - stripeStart) / (R * 1.05f);
                        float gap = period * (0.08f + 0.55f * rel);
                        float m = Gfx.Mod(y - stripeStart - scroll, period);
                        if (m < gap) continue;
                    }
                    var col = Gfx.Mix(SunTop, SunBottom, Gfx.Smooth(0f, 1f, (y - y0) / (2f * R)));
                    brush.Color = Gfx.C4(col, 1f);
                    dc.FillRectangle(new RawRectF(sc.X - half, y, sc.X + half, y + 1.6f), brush);
                }
            }

            // ---- ネオンの山並み ----
            if (Mountains)
            {
                for (int layer = 0; layer < 2; layer++)
                {
                    var pts = new List<Vector2> { new(-10f, hy) };
                    int seg = 16 + layer * 6;
                    float peakH = h * (layer == 0 ? 0.2f : 0.12f);
                    for (int i = 0; i <= seg; i++)
                    {
                        float x = -10f + (w + 20f) * i / seg;
                        float center = MathF.Abs(x - w / 2f) / (w / 2f);
                        float hh = peakH * (0.25f + 0.75f * DrawKit.H(Seed + layer * 50, i, 1)) * (0.25f + 0.75f * Gfx.Smooth(0.15f, 0.6f, center));
                        pts.Add(new Vector2(x, hy - hh));
                    }
                    pts.Add(new Vector2(w + 10f, hy));
                    brush.Color = Gfx.C4(Gfx.Mix(SkyTop, WpfColor.FromRgb(0, 0, 0), layer == 0 ? 0.35f : 0.1f), MathF.Max(0.85f, bg));
                    Gfx.FillPoly(dc, brush, pts);
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    var ridge = pts.GetRange(1, pts.Count - 2);
                    brush.Color = Gfx.C4(GridColor, layer == 0 ? 0.25f : 0.4f);
                    Gfx.Polyline(dc, brush, ridge, 6f);
                    brush.Color = Gfx.C4(Gfx.Mix(GridColor, white, 0.4f), layer == 0 ? 0.6f : 0.9f);
                    Gfx.Polyline(dc, brush, ridge, 1.8f);
                    // 山肌のワイヤー
                    brush.Color = Gfx.C4(GridColor, 0.22f);
                    for (int i = 2; i < ridge.Count - 1; i += 2)
                        dc.DrawLine(ridge[i], new Vector2(ridge[i].X + (ridge[i].X - w / 2f) * 0.08f, hy), brush, 1f);
                    dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                }
            }

            // ---- 地面とグリッド ----
            if (bg > 0f)
            {
                using var ground = Gfx.Linear(dc, new Vector2(0, hy), new Vector2(0, h), (0f, Gfx.C4(0.08f, 0f, 0.12f, bg)), (1f, Gfx.C4(0.01f, 0f, 0.03f, bg)));
                dc.FillRectangle(new RawRectF(0, hy, w, h), ground);
            }
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            var vp = new Vector2(w / 2f, hy);
            float depthH = h - hy;
            int nv = (int)MathF.Ceiling(w * 2.2f / spacing / 2f) + 2;
            for (int pass = 0; pass < 2; pass++)
            {
                float lw = pass == 0 ? 6f : 1.8f;
                var col = pass == 0 ? Gfx.C4(GridColor, 0.22f) : Gfx.C4(Gfx.Mix(GridColor, white, 0.25f), 0.85f);
                brush.Color = col;
                // 縦線（消失点に集まる）
                for (int i = -nv; i <= nv; i++)
                {
                    var bottom = new Vector2(w / 2f + i * spacing * 2.2f, h + depthH * 0.2f);
                    var top = Vector2.Lerp(vp, bottom, 0.02f);
                    dc.DrawLine(top, bottom, brush, lw);
                }
                // 横線（手前へ流れる）
                float scroll = DrawKit.Frac(t * speed * 1.2f);
                for (int j = 0; j < 40; j++)
                {
                    float z = j + 1f - scroll;
                    if (z <= 0.05f) continue;
                    float y = hy + depthH * (1.1f / z) * (spacing / 110f);
                    if (y > h + 4f) continue;
                    float fade = Gfx.Smooth(hy, hy + depthH * 0.25f, y);
                    brush.Color = new Color4(col.R, col.G, col.B, col.A * (0.15f + 0.85f * fade));
                    dc.DrawLine(new Vector2(0, y), new Vector2(w, y), brush, lw * (0.4f + 0.6f * fade));
                }
            }
            // 地平線の光
            using (var hg = Gfx.Linear(dc, new Vector2(0, hy - 30f), new Vector2(0, hy + 30f), (0f, Gfx.C4(SkyBottom, 0f)), (0.5f, Gfx.C4(Gfx.Mix(SkyBottom, white, 0.4f), 0.6f)), (1f, Gfx.C4(SkyBottom, 0f))))
                dc.FillRectangle(new RawRectF(0, hy - 30f, w, hy + 30f), hg);
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
