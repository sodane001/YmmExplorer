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

public enum RankKind
{
    [Display(Name = "1位（金）")] First,
    [Display(Name = "2位（銀）")] Second,
    [Display(Name = "3位（銅）")] Third,
    [Display(Name = "1〜3位を並べる")] Podium,
}

// =====================================================================
//  ランキング・順位バッジ
//   1位から3位のメダル、リボン、数字がせり上がる
// =====================================================================
public sealed class RankingBadgePlugin : IShapePlugin
{
    public string Name => "演出：ランキング・順位バッジ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RankingBadgeParameter(sharedData);
}

public sealed class RankingBadgeParameter : EffectShapeParameterBase
{
    public RankingBadgeParameter() : this(null) { }
    public RankingBadgeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "順位", Name = "順位")]
    [EnumComboBox]
    public RankKind Kind { get => kind; set => Set(ref kind, value); }
    RankKind kind = RankKind.First;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(42, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "メダルの半径")]
    [AnimationSlider("F0", "px", 30, 400)]
    public Animation MedalSize { get; } = new Animation(150, 5, 4000);

    [Display(GroupName = "装飾", Name = "リボンを付ける")]
    [ToggleSlider]
    public bool Ribbon { get => ribbon; set => Set(ref ribbon, value); }
    bool ribbon = true;

    [Display(GroupName = "装飾", Name = "リボンの色")]
    [ColorPicker]
    public WpfColor RibbonColor { get => ribbonColor; set => Set(ref ribbonColor, value); }
    WpfColor ribbonColor = WpfColor.FromRgb(0xD8, 0x2E, 0x46);

    [Display(GroupName = "装飾", Name = "光が走る")]
    [ToggleSlider]
    public bool Shine { get => shine; set => Set(ref shine, value); }
    bool shine = true;

    [Display(GroupName = "装飾", Name = "きらめき")]
    [ToggleSlider]
    public bool Sparkle { get => sparkle; set => Set(ref sparkle, value); }
    bool sparkle = true;

    [Display(GroupName = "動き", Name = "出てくる時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation IntroTime { get; } = new Animation(0.7, 0, 60);

    [Display(GroupName = "動き", Name = "並べる時の出る間隔")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation Stagger { get; } = new Animation(0.45, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, MedalSize, IntroTime, Stagger];

    static (WpfColor light, WpfColor main, WpfColor dark) Metal(int rank) => rank switch
    {
        1 => (WpfColor.FromRgb(0xFF, 0xF2, 0xA8), WpfColor.FromRgb(0xF2, 0xC2, 0x2E), WpfColor.FromRgb(0x9A, 0x6A, 0x08)),
        2 => (WpfColor.FromRgb(0xFF, 0xFF, 0xFF), WpfColor.FromRgb(0xC8, 0xD0, 0xDA), WpfColor.FromRgb(0x6A, 0x74, 0x84)),
        _ => (WpfColor.FromRgb(0xFF, 0xD2, 0xA0), WpfColor.FromRgb(0xD0, 0x84, 0x44), WpfColor.FromRgb(0x6E, 0x38, 0x14)),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(MedalSize), intro = c.F(IntroTime), stagger = c.F(Stagger);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (Kind == RankKind.Podium)
            {
                // 3位 → 2位 → 1位 の順に出る
                DrawMedal(dc, brush, ctr + new Vector2(R * 2.5f, R * 0.45f), R * 0.78f, 3, t, intro);
                DrawMedal(dc, brush, ctr + new Vector2(-R * 2.5f, R * 0.3f), R * 0.86f, 2, t - stagger, intro);
                DrawMedal(dc, brush, ctr, R, 1, t - stagger * 2f, intro);
            }
            else
            {
                int rank = Kind switch { RankKind.Second => 2, RankKind.Third => 3, _ => 1 };
                DrawMedal(dc, brush, ctr, R, rank, t, intro);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    void DrawMedal(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 basePos, float R, int rank, float lt, float intro)
    {
        if (lt < 0f) return;
        float k = intro <= 0.001f ? 1f : Math.Clamp(lt / intro, 0f, 1f);
        float ek = DrawKit.EaseOutBack(k);
        float alpha = Math.Clamp(k * 3f, 0f, 1f);
        var p = basePos + new Vector2(0, (1f - ek) * R * 2.2f);
        float s = 0.6f + 0.4f * ek;
        float r = R * s;
        float sway = MathF.Sin(lt * 2.2f) * 0.03f * Gfx.Smooth(0.8f, 1f, k);
        var (light, main, dark) = Metal(rank);
        var white = WpfColor.FromRgb(255, 255, 255);

        // ---- リボン ----
        if (Ribbon)
        {
            var rc = rank == 1 ? RibbonColor : rank == 2 ? Gfx.Mix(RibbonColor, WpfColor.FromRgb(0x2E, 0x5C, 0xD8), 0.8f) : Gfx.Mix(RibbonColor, WpfColor.FromRgb(0x2E, 0x9A, 0x5A), 0.8f);
            var rd = Gfx.Mix(rc, WpfColor.FromRgb(0, 0, 0), 0.35f);
            for (int sgn = -1; sgn <= 1; sgn += 2)
            {
                var tail = DrawKit.Place(new[]
                {
                    new Vector2(sgn * r * 0.15f, 0), new Vector2(sgn * r * 0.62f, 0),
                    new Vector2(sgn * r * 0.78f, r * 1.75f), new Vector2(sgn * r * 0.55f, r * 1.52f), new Vector2(sgn * r * 0.38f, r * 1.8f),
                }, p, sgn * 0.12f + sway);
                brush.Color = Gfx.C4(rd, alpha);
                Gfx.FillPoly(dc, brush, tail.Select(v => v + new Vector2(3f, 4f)).ToList());
                brush.Color = Gfx.C4(rc, alpha);
                Gfx.FillPoly(dc, brush, tail);
                brush.Color = Gfx.C4(white, 0.25f * alpha);
                dc.DrawLine(tail[1], tail[2], brush, MathF.Max(1f, r * 0.04f));
            }
        }

        // ---- メダル本体 ----
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f * alpha);
        dc.FillEllipse(new Ellipse(p + new Vector2(r * 0.05f, r * 0.08f), r, r), brush);
        // ギザギザのふち
        var rim = ShapeGeom.StarPoly(p, r, 36, 0.94f, sway);
        brush.Color = Gfx.C4(dark, alpha);
        Gfx.FillPoly(dc, brush, rim);
        using (var g = Gfx.Radial(dc, p - new Vector2(r * 0.3f, r * 0.35f), r * 1.5f,
            (0f, Gfx.C4(light, alpha)), (0.45f, Gfx.C4(main, alpha)), (1f, Gfx.C4(dark, alpha))))
            dc.FillEllipse(new Ellipse(p, r * 0.9f, r * 0.9f), g);
        brush.Color = Gfx.C4(dark, 0.55f * alpha);
        dc.DrawEllipse(new Ellipse(p, r * 0.74f, r * 0.74f), brush, MathF.Max(1.5f, r * 0.035f));
        brush.Color = Gfx.C4(light, 0.6f * alpha);
        dc.DrawEllipse(new Ellipse(p, r * 0.77f, r * 0.77f), brush, MathF.Max(1f, r * 0.015f));
        // 月桂樹（小さな葉）
        brush.Color = Gfx.C4(dark, 0.45f * alpha);
        for (int sgn = -1; sgn <= 1; sgn += 2)
            for (int i = 0; i < 6; i++)
            {
                float a = MathF.PI / 2f + sgn * (0.5f + i * 0.33f);
                var lp = p + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r * 0.64f;
                var leaf = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, r * 0.075f, r * 0.035f, 12), lp, a + MathF.PI / 2f * sgn);
                Gfx.FillPoly(dc, brush, leaf);
            }

        // ---- 数字（せり上がる）----
        float kn = intro <= 0.001f ? 1f : Math.Clamp((lt - intro * 0.55f) / MathF.Max(0.05f, intro * 0.6f), 0f, 1f);
        float en = DrawKit.EaseOutBack(kn);
        float nh = r * 0.78f;
        var np = p + new Vector2(-r * 0.08f, (1f - en) * r * 0.45f - r * 0.04f);
        float na = Math.Clamp(kn * 2f, 0f, 1f) * alpha;
        string num = rank.ToString();
        string suf = rank switch { 1 => "ST", 2 => "ND", _ => "RD" };
        brush.Color = Gfx.C4(light, 0.7f * na);
        StrokeFont.DrawCentered(dc, brush, num, np + new Vector2(-1.5f, -1.5f), nh, MathF.Max(2f, nh * 0.17f));
        brush.Color = Gfx.C4(dark, na);
        StrokeFont.DrawCentered(dc, brush, num, np, nh, MathF.Max(2f, nh * 0.15f));
        StrokeFont.DrawCentered(dc, brush, suf, np + new Vector2(r * 0.34f, nh * 0.2f), nh * 0.3f, MathF.Max(1.2f, nh * 0.055f));

        // ---- 光が走る ----
        if (Shine && k >= 1f)
        {
            float cyc = 2.4f;
            float u = Gfx.Mod(lt - intro, cyc) / 0.7f;
            if (u < 1f)
            {
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                var dir = Vector2.Normalize(new Vector2(1f, -1f));
                var along = new Vector2(dir.Y, -dir.X) * -1f;
                float band = r * 0.35f;
                float center = (-1.2f + 2.4f * u) * r;
                for (int i = -6; i <= 6; i++)
                {
                    float o = center + i * band / 6f;
                    if (MathF.Abs(o) >= r * 0.9f) continue;
                    float half = MathF.Sqrt(r * r * 0.81f - o * o);
                    var m = p + along * o;
                    float a = 0.35f * MathF.Exp(-(i * i) / 10f);
                    brush.Color = Gfx.C4(white, a);
                    dc.DrawLine(m - dir * half, m + dir * half, brush, band / 6f + 1f);
                }
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }

        // ---- きらめき ----
        if (Sparkle && k > 0.6f)
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            for (int i = 0; i < 6; i++)
            {
                float ph = DrawKit.Frac(lt * 0.6f + DrawKit.H(Seed + rank, i, 1));
                float a = DrawKit.H(Seed + rank, i, 2) * MathF.PI * 2f;
                var sp = p + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r * (0.95f + 0.35f * DrawKit.H(Seed + rank, i, 3));
                float sa = MathF.Sin(ph * MathF.PI);
                DrawKit.Twinkle(dc, brush, sp, r * 0.18f * sa, 2f, light, 0.9f * sa * alpha);
            }
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
