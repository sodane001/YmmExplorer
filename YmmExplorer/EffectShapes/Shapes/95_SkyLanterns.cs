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
//  空に浮かぶランタン
//   夜空にゆっくり上がる灯籠の群れ
// =====================================================================
public sealed class SkyLanternsPlugin : IShapePlugin
{
    public string Name => "演出：空に浮かぶランタン";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SkyLanternsParameter(sharedData);
}

public sealed class SkyLanternsParameter : EffectShapeParameterBase
{
    public SkyLanternsParameter() : this(null) { }
    public SkyLanternsParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "ランタン", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x9A, 0x3A);

    [Display(GroupName = "ランタン", Name = "数")]
    [AnimationSlider("F0", "個", 1, 150)]
    public Animation Count { get; } = new Animation(45, 0, 2000);

    [Display(GroupName = "ランタン", Name = "大きさ（手前）")]
    [AnimationSlider("F0", "px", 10, 150)]
    public Animation Size { get; } = new Animation(48, 2, 2000);

    [Display(GroupName = "ランタン", Name = "上がる速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Rise { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "ランタン", Name = "ゆれ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Sway { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "ランタン", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Glow { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "背景", Name = "夜空を描く")]
    [ToggleSlider]
    public bool NightSky { get => nightSky; set => Set(ref nightSky, value); }
    bool nightSky = false;

    [Display(GroupName = "背景", Name = "夜空の色（上）")]
    [ColorPicker]
    public WpfColor SkyTop { get => skyTop; set => Set(ref skyTop, value); }
    WpfColor skyTop = WpfColor.FromRgb(0x04, 0x07, 0x1A);

    [Display(GroupName = "背景", Name = "夜空の色（下）")]
    [ColorPicker]
    public WpfColor SkyBottom { get => skyBottom; set => Set(ref skyBottom, value); }
    WpfColor skyBottom = WpfColor.FromRgb(0x22, 0x1A, 0x3C);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Rise, Sway, Glow];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), rise = c.F(Rise) / 100f, sway = c.F(Sway) / 100f, glow = c.F(Glow) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);
        var hot = Gfx.Mix(Color, WpfColor.FromRgb(0xFF, 0xF0, 0xB0), 0.7f);
        var dark = Gfx.Mix(Color, WpfColor.FromRgb(0x30, 0x10, 0x05), 0.55f);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (NightSky)
            {
                using var sky = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, h), (0f, Gfx.C4(SkyTop, 1f)), (1f, Gfx.C4(SkyBottom, 1f)));
                dc.FillRectangle(new RawRectF(0, 0, w, h), sky);
                var rs = new Random(Seed + 3);
                for (int i = 0; i < 180; i++)
                {
                    var p = new Vector2(rs.NextF() * w, rs.NextF() * h * 0.8f);
                    brush.Color = Gfx.C4(white, (0.25f + 0.5f * rs.NextF()) * (0.6f + 0.4f * MathF.Sin(t * 2f + i)));
                    dc.FillEllipse(new Ellipse(p, 0.8f, 0.8f), brush);
                }
            }

            // 奥（小さい）から手前（大きい）の順に並べる
            var items = new List<(float depth, int i)>();
            var rng = new Random(Seed);
            var depths = new float[n];
            for (int i = 0; i < n; i++) { depths[i] = 0.25f + 0.75f * MathF.Pow(rng.NextF(), 1.6f); items.Add((depths[i], i)); }
            items.Sort((a, b) => a.depth.CompareTo(b.depth));

            foreach (var (d, i) in items)
            {
                float s = size * d;
                float travel = h + s * 6f;
                float y = h + s * 2f - DrawKit.Frac(t * rise * 18f * (0.5f + 0.5f * d) / travel + DrawKit.H(Seed, i, 1)) * travel;
                float x = DrawKit.H(Seed, i, 2) * w + MathF.Sin(t * 0.35f * (0.6f + d) + i * 1.3f) * 22f * sway * d;
                float tilt = MathF.Sin(t * 0.6f + i) * 0.06f * sway;
                float flick = 0.88f + 0.12f * MathF.Sin(t * 9f + i * 3.1f) * MathF.Sin(t * 5.3f + i);
                float al = 0.55f + 0.45f * d;
                var p = new Vector2(x, y);

                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, p + new Vector2(0, s * 0.2f), s * 2.4f * glow, Gfx.C4(Color, 0.3f * al * flick));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

                var body = DrawKit.Place(new[]
                {
                    new Vector2(-s * 0.36f, -s * 0.55f), new Vector2(s * 0.36f, -s * 0.55f),
                    new Vector2(s * 0.3f, s * 0.5f), new Vector2(-s * 0.3f, s * 0.5f),
                }, p, tilt);
                using (var lg = Gfx.Linear(dc, p - new Vector2(0, s * 0.55f), p + new Vector2(0, s * 0.5f),
                    (0f, Gfx.C4(dark, al)), (0.55f, Gfx.C4(Color, al)), (1f, Gfx.C4(hot, al))))
                    Gfx.FillPoly(dc, lg, body);
                // たての骨
                brush.Color = Gfx.C4(dark, 0.5f * al);
                var top = DrawKit.Place(new[] { new Vector2(0, -s * 0.55f), new Vector2(0, s * 0.5f) }, p, tilt);
                dc.DrawLine(top[0], top[1], brush, MathF.Max(0.6f, s * 0.03f));
                // 下の口と炎
                var mouth = DrawKit.Place(new[] { new Vector2(-s * 0.3f, s * 0.5f), new Vector2(s * 0.3f, s * 0.5f) }, p, tilt);
                brush.Color = Gfx.C4(0.15f, 0.05f, 0.02f, 0.8f * al);
                dc.DrawLine(mouth[0], mouth[1], brush, MathF.Max(1f, s * 0.06f));
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                var fl = Vector2.Lerp(mouth[0], mouth[1], 0.5f) - new Vector2(0, s * 0.08f);
                DrawKit.Glow(dc, fl, s * 0.35f, Gfx.C4(hot, 0.9f * flick * al));
                brush.Color = Gfx.C4(white, 0.85f * flick * al);
                dc.FillEllipse(new Ellipse(fl, s * 0.05f + 0.5f, s * 0.08f + 0.5f), brush);
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
