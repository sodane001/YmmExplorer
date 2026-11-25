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
//  クリスマス
//   雪だるま、上からぶら下がるオーナメント、降る雪
// =====================================================================
public sealed class ChristmasPlugin : IShapePlugin
{
    public string Name => "演出：クリスマス（雪だるまとオーナメント）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ChristmasParameter(sharedData);
}

public sealed class ChristmasParameter : EffectShapeParameterBase
{
    public ChristmasParameter() : this(null) { }
    public ChristmasParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "表示", Name = "雪だるま")]
    [ToggleSlider]
    public bool Snowman { get => snowman; set => Set(ref snowman, value); }
    bool snowman = true;

    [Display(GroupName = "表示", Name = "オーナメント")]
    [ToggleSlider]
    public bool Ornaments { get => ornaments; set => Set(ref ornaments, value); }
    bool ornaments = true;

    [Display(GroupName = "表示", Name = "降る雪")]
    [ToggleSlider]
    public bool Snow { get => snow; set => Set(ref snow, value); }
    bool snow = true;

    [Display(GroupName = "表示", Name = "地面の雪")]
    [ToggleSlider]
    public bool Ground { get => ground; set => Set(ref ground, value); }
    bool ground = true;

    [Display(GroupName = "雪だるま", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(22, -100, 200);

    [Display(GroupName = "雪だるま", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 250)]
    public Animation ItemScale { get; } = new Animation(100, 5, 1000);

    [Display(GroupName = "雪だるま", Name = "マフラーの色")]
    [ColorPicker]
    public WpfColor ScarfColor { get => scarfColor; set => Set(ref scarfColor, value); }
    WpfColor scarfColor = WpfColor.FromRgb(0xD8, 0x2A, 0x36);

    [Display(GroupName = "オーナメント", Name = "数")]
    [AnimationSlider("F0", "個", 1, 20)]
    public Animation OrnamentCount { get; } = new Animation(7, 0, 100);

    [Display(GroupName = "雪", Name = "雪の数")]
    [AnimationSlider("F0", "粒", 0, 500)]
    public Animation SnowCount { get; } = new Animation(160, 0, 5000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [PosX, ItemScale, OrnamentCount, SnowCount];

    static readonly WpfColor[] BallColors =
    {
        WpfColor.FromRgb(0xD8, 0x2A, 0x36), WpfColor.FromRgb(0xF2, 0xC2, 0x2E), WpfColor.FromRgb(0x2E, 0x9A, 0x5A),
        WpfColor.FromRgb(0x3C, 0x78, 0xE8), WpfColor.FromRgb(0xC8, 0xD0, 0xDA),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float S = MathF.Min(w, h) / 1080f * c.F(ItemScale) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);
        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (Ground)
            {
                var g = new List<Vector2> { new(0, h) };
                for (int i = 0; i <= 30; i++) g.Add(new Vector2(w * i / 30f, h * 0.86f + MathF.Sin(i * 0.7f + Seed) * h * 0.015f));
                g.Add(new Vector2(w, h));
                brush.Color = Gfx.C4(0.93f, 0.96f, 1f, 1f);
                Gfx.FillPoly(dc, brush, g);
            }

            if (Ornaments)
            {
                int n = (int)c.F(OrnamentCount);
                for (int i = 0; i < n; i++)
                {
                    float x = w * (i + 0.5f) / Math.Max(1, n);
                    float len = h * (0.1f + 0.18f * DrawKit.H(Seed, i, 1));
                    float sw = MathF.Sin(t * 1.2f + i * 1.7f) * 0.12f;
                    var top = new Vector2(x, -2f);
                    var bc = top + new Vector2(MathF.Sin(sw) * len, MathF.Cos(sw) * len);
                    float r = 34f * S * (0.75f + 0.5f * DrawKit.H(Seed, i, 2));
                    brush.Color = Gfx.C4(0.85f, 0.75f, 0.4f, 0.9f);
                    dc.DrawLine(top, bc - new Vector2(0, r), brush, 1.5f);
                    bool star = i % 4 == 3;
                    var col = BallColors[i % BallColors.Length];
                    if (star)
                    {
                        brush.Color = Gfx.C4(1f, 0.85f, 0.3f, 1f);
                        Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(bc, r * 1.1f, 5, 0.45f, sw));
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        DrawKit.Glow(dc, bc, r * 2.5f, Gfx.C4(1f, 0.8f, 0.3f, 0.35f));
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                        continue;
                    }
                    brush.Color = Gfx.C4(0.75f, 0.68f, 0.45f, 1f);
                    dc.FillRectangle(new RawRectF(bc.X - r * 0.22f, bc.Y - r * 1.12f, bc.X + r * 0.22f, bc.Y - r * 0.88f), brush);
                    using (var gb = Gfx.Radial(dc, bc - new Vector2(r * 0.35f, r * 0.4f), r * 1.5f,
                        (0f, Gfx.C4(Gfx.Mix(col, white, 0.7f), 1f)), (0.4f, Gfx.C4(col, 1f)), (1f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(0, 0, 0), 0.55f), 1f))))
                        dc.FillEllipse(new Ellipse(bc, r, r), gb);
                    brush.Color = Gfx.C4(white, 0.35f);
                    dc.DrawEllipse(new Ellipse(bc, r * 0.95f, r * 0.28f), brush, 2f);
                    if (DrawKit.Frac(t * 0.5f + DrawKit.H(Seed, i, 3)) < 0.1f)
                    {
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        DrawKit.Twinkle(dc, brush, bc - new Vector2(r * 0.4f, r * 0.45f), r * 0.9f, 1.5f, white, 0.9f);
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                    }
                }
            }

            if (Snowman)
            {
                var b = new Vector2(c.F(PosX) / 100f * w, h * 0.9f);
                float r1 = 150f * S, r2 = 105f * S;
                var c1 = b - new Vector2(0, r1 * 0.9f);
                var c2 = c1 - new Vector2(0, r1 * 0.8f + r2 * 0.75f);
                brush.Color = Gfx.C4(0.55f, 0.6f, 0.7f, 0.3f);
                dc.FillEllipse(new Ellipse(b + new Vector2(0, 6f * S), r1 * 1.1f, r1 * 0.18f), brush);
                // 腕
                brush.Color = Gfx.C4(0.38f, 0.24f, 0.12f, 1f);
                float wave = MathF.Sin(t * 3f) * 0.25f;
                foreach (float sx in new[] { -1f, 1f })
                {
                    var a0 = c1 + new Vector2(sx * r1 * 0.75f, -r1 * 0.55f);
                    float ang = (sx < 0 ? MathF.PI + 0.5f : -0.5f) + (sx > 0 ? wave : 0f);
                    var a1 = a0 + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * r1 * 0.9f;
                    dc.DrawLine(a0, a1, brush, 7f * S + 1f);
                    dc.DrawLine(Vector2.Lerp(a0, a1, 0.7f), Vector2.Lerp(a0, a1, 0.7f) + new Vector2(MathF.Cos(ang - 0.7f), MathF.Sin(ang - 0.7f)) * r1 * 0.25f, brush, 5f * S + 1f);
                }
                foreach (var (cc, rr) in new[] { (c1, r1), (c2, r2) })
                    using (var g = Gfx.Radial(dc, cc - new Vector2(rr * 0.35f, rr * 0.35f), rr * 1.5f, (0f, Gfx.C4(1f, 1f, 1f, 1f)), (0.7f, Gfx.C4(0.92f, 0.95f, 1f, 1f)), (1f, Gfx.C4(0.7f, 0.78f, 0.9f, 1f))))
                        dc.FillEllipse(new Ellipse(cc, rr, rr), g);
                // ボタン
                brush.Color = Gfx.C4(0.15f, 0.15f, 0.18f, 1f);
                for (int i = 0; i < 3; i++) dc.FillEllipse(new Ellipse(c1 + new Vector2(0, -r1 * 0.45f + i * r1 * 0.32f), r1 * 0.07f, r1 * 0.07f), brush);
                // 顔
                dc.FillEllipse(new Ellipse(c2 + new Vector2(-r2 * 0.32f, -r2 * 0.15f), r2 * 0.08f, r2 * 0.1f), brush);
                dc.FillEllipse(new Ellipse(c2 + new Vector2(r2 * 0.32f, -r2 * 0.15f), r2 * 0.08f, r2 * 0.1f), brush);
                for (int i = 0; i < 5; i++)
                {
                    float a = MathF.PI * (0.2f + 0.6f * i / 4f);
                    dc.FillEllipse(new Ellipse(c2 + new Vector2(MathF.Cos(a) * r2 * 0.45f, MathF.Sin(a) * r2 * 0.4f), r2 * 0.045f, r2 * 0.045f), brush);
                }
                brush.Color = Gfx.C4(1f, 0.5f, 0.1f, 1f);
                Gfx.FillPoly(dc, brush, new List<Vector2> { c2 + new Vector2(0, -r2 * 0.05f), c2 + new Vector2(r2 * 0.55f, r2 * 0.08f), c2 + new Vector2(0, r2 * 0.12f) });
                // マフラー
                brush.Color = Gfx.C4(ScarfColor, 1f);
                var sc = c2 + new Vector2(0, r2 * 0.85f);
                Gfx.FillPoly(dc, brush, DrawKit.EllipsePts(sc, r2 * 0.95f, r2 * 0.2f, 30));
                Gfx.FillPoly(dc, brush, new List<Vector2> { sc + new Vector2(r2 * 0.35f, 0), sc + new Vector2(r2 * 0.65f, 0), sc + new Vector2(r2 * 0.75f + MathF.Sin(t * 2f) * 6f, r2 * 0.8f), sc + new Vector2(r2 * 0.45f + MathF.Sin(t * 2f) * 6f, r2 * 0.82f) });
                // 帽子（バケツ）
                brush.Color = Gfx.C4(0.2f, 0.22f, 0.3f, 1f);
                var ht = c2 - new Vector2(0, r2 * 0.8f);
                Gfx.FillPoly(dc, brush, new List<Vector2> { ht + new Vector2(-r2 * 0.55f, 0), ht + new Vector2(r2 * 0.55f, 0), ht + new Vector2(r2 * 0.42f, -r2 * 0.7f), ht + new Vector2(-r2 * 0.42f, -r2 * 0.7f) });
                brush.Color = Gfx.C4(ScarfColor, 1f);
                dc.FillRectangle(new RawRectF(ht.X - r2 * 0.52f, ht.Y - r2 * 0.18f, ht.X + r2 * 0.52f, ht.Y - r2 * 0.06f), brush);
            }

            if (Snow)
            {
                var rng = new Random(Seed + 9);
                int n = (int)c.F(SnowCount);
                for (int i = 0; i < n; i++)
                {
                    float d = 0.4f + 0.6f * rng.NextF();
                    float x0 = rng.NextF() * w, off = rng.NextF();
                    float y = Gfx.Mod(off * h + t * 60f * d, h + 20f) - 10f;
                    float x = x0 + MathF.Sin(t * 0.8f * d + i) * 20f * d;
                    brush.Color = Gfx.C4(white, 0.5f + 0.5f * d);
                    float r = (1.5f + 3f * d) * MathF.Max(0.6f, S);
                    dc.FillEllipse(new Ellipse(new Vector2(x, y), r, r), brush);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
