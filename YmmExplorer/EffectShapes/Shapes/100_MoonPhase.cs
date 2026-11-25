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

public enum CelestialMode
{
    [Display(Name = "月の満ち欠け")] Phases,
    [Display(Name = "皆既日食（ダイヤモンドリング）")] SolarEclipse,
    [Display(Name = "皆既月食（赤い月）")] LunarEclipse,
}

// =====================================================================
//  月の満ち欠け・日食
//   月相の変化と、リング状に光る皆既日食
// =====================================================================
public sealed class MoonPhasePlugin : IShapePlugin
{
    public string Name => "演出：月の満ち欠け・日食";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MoonPhaseParameter(sharedData);
}

public sealed class MoonPhaseParameter : EffectShapeParameterBase
{
    public MoonPhaseParameter() : this(null) { }
    public MoonPhaseParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public CelestialMode Mode { get => mode; set => Set(ref mode, value); }
    CelestialMode mode = CelestialMode.Phases;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(45, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "半径")]
    [AnimationSlider("F0", "px", 30, 500)]
    public Animation Radius { get; } = new Animation(170, 2, 4000);

    [Display(GroupName = "進み方", Name = "自動で進める")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "進み方", Name = "1周の時間（自動）")]
    [AnimationSlider("F1", "秒", 2, 30)]
    public Animation Cycle { get; } = new Animation(10, 0.2, 600);

    [Display(GroupName = "進み方", Name = "進み具合（手動）", Description = "満ち欠け: 0=新月 50=満月 100=新月。日食・月食: 50 が皆既")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Progress { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "色", Name = "月の色")]
    [ColorPicker]
    public WpfColor MoonColor { get => moonColor; set => Set(ref moonColor, value); }
    WpfColor moonColor = WpfColor.FromRgb(0xF4, 0xEB, 0xD0);

    [Display(GroupName = "色", Name = "影の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ShadowAmount { get; } = new Animation(88, 0, 100);

    [Display(GroupName = "色", Name = "太陽・コロナの色")]
    [ColorPicker]
    public WpfColor SunColor { get => sunColor; set => Set(ref sunColor, value); }
    WpfColor sunColor = WpfColor.FromRgb(0xFF, 0xF2, 0xD0);

    [Display(GroupName = "色", Name = "月食の赤い色")]
    [ColorPicker]
    public WpfColor BloodColor { get => bloodColor; set => Set(ref bloodColor, value); }
    WpfColor bloodColor = WpfColor.FromRgb(0xB0, 0x34, 0x1C);

    [Display(GroupName = "装飾", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glow { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "装飾", Name = "クレーター（もよう）を描く")]
    [ToggleSlider]
    public bool Craters { get => craters; set => Set(ref craters, value); }
    bool craters = true;

    [Display(GroupName = "装飾", Name = "星空を描く")]
    [ToggleSlider]
    public bool Stars { get => stars; set => Set(ref stars, value); }
    bool stars = false;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Cycle, Progress, ShadowAmount, Glow];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), glow = c.F(Glow) / 100f, shade = c.F(ShadowAmount) / 100f;
        float p = Auto ? DrawKit.Frac(t / MathF.Max(0.2f, c.F(Cycle))) : c.F(Progress) / 100f;

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (Stars)
            {
                brush.Color = Gfx.C4(0.01f, 0.015f, 0.04f, 1f);
                dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
                var rng = new Random(Seed);
                for (int i = 0; i < 200; i++)
                {
                    brush.Color = Gfx.C4(1f, 1f, 1f, (0.25f + 0.6f * rng.NextF()) * (0.7f + 0.3f * MathF.Sin(t * 2f + i)));
                    dc.FillEllipse(new Ellipse(new Vector2(rng.NextF() * w, rng.NextF() * h), 0.9f, 0.9f), brush);
                }
            }
            switch (Mode)
            {
                case CelestialMode.Phases: DrawPhases(dc, brush, ctr, R, p, glow, shade); break;
                case CelestialMode.SolarEclipse: DrawSolar(dc, brush, ctr, R, p, glow, t, w, h); break;
                default: DrawLunar(dc, brush, ctr, R, p, glow); break;
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    void MoonDisk(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float R, WpfColor col)
    {
        using (var g = Gfx.Radial(dc, ctr - new Vector2(R * 0.25f, R * 0.25f), R * 1.4f,
            (0f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.3f), 1f)), (1f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(90, 85, 75), 0.35f), 1f))))
            dc.FillEllipse(new Ellipse(ctr, R, R), g);
        if (!Craters) return;
        var dark = Gfx.Mix(col, WpfColor.FromRgb(60, 55, 50), 0.6f);
        (float x, float y, float r)[] cr =
        {
            (-0.35f, -0.25f, 0.28f), (0.2f, -0.4f, 0.16f), (0.35f, 0.15f, 0.22f), (-0.1f, 0.3f, 0.18f),
            (-0.5f, 0.25f, 0.12f), (0.05f, -0.05f, 0.1f), (0.55f, -0.2f, 0.09f), (-0.2f, 0.62f, 0.08f),
        };
        foreach (var (x, y, r) in cr)
        {
            brush.Color = Gfx.C4(dark, 0.22f);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(x, y) * R, r * R, r * R * 0.92f), brush);
            brush.Color = Gfx.C4(dark, 0.18f);
            dc.DrawEllipse(new Ellipse(ctr + new Vector2(x, y) * R, r * R, r * R * 0.92f), brush, MathF.Max(1f, R * 0.01f));
        }
    }

    void DrawPhases(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float R, float p, float glow, float shade)
    {
        float lit = (1f - MathF.Cos(p * MathF.PI * 2f)) / 2f;   // 0=新月, 1=満月
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        DrawKit.Glow(dc, ctr, R * (1.6f + 0.8f * lit) * glow, Gfx.C4(MoonColor, 0.35f * lit));
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        MoonDisk(dc, brush, ctr, R, MoonColor);

        // 影（地球照で少しだけ見える）。満ちる時は左側、欠ける時は右側が暗い
        bool waxing = p < 0.5f;
        float k = MathF.Cos(p * MathF.PI * 2f);
        var poly = new List<Vector2>();
        const int n = 48;
        float side = waxing ? -1f : 1f;
        for (int i = 0; i <= n; i++)
        {
            float a = -MathF.PI / 2f + MathF.PI * i / n;
            poly.Add(ctr + new Vector2(side * MathF.Cos(a) * R, MathF.Sin(a) * R));
        }
        for (int i = n; i >= 0; i--)
        {
            float a = -MathF.PI / 2f + MathF.PI * i / n;
            poly.Add(ctr + new Vector2(-side * k * MathF.Cos(a) * R, MathF.Sin(a) * R));
        }
        brush.Color = Gfx.C4(0.03f, 0.035f, 0.06f, shade);
        Gfx.FillPoly(dc, brush, poly);
        // 境目をやわらかく
        brush.Color = Gfx.C4(0.03f, 0.035f, 0.06f, shade * 0.35f);
        var soft = new List<Vector2>();
        for (int i = 0; i <= n; i++)
        {
            float a = -MathF.PI / 2f + MathF.PI * i / n;
            soft.Add(ctr + new Vector2(-side * k * MathF.Cos(a) * R + (-side) * R * 0.04f * MathF.Cos(a), MathF.Sin(a) * R));
        }
        for (int i = n; i >= 0; i--)
        {
            float a = -MathF.PI / 2f + MathF.PI * i / n;
            soft.Add(ctr + new Vector2(-side * k * MathF.Cos(a) * R, MathF.Sin(a) * R));
        }
        Gfx.FillPoly(dc, brush, soft);
    }

    void DrawSolar(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float R, float p, float glow, float t, float w, float h)
    {
        float off = (p - 0.5f) * 4.4f * R;                    // 月の中心の、太陽からのずれ
        var mc = ctr + new Vector2(off, off * 0.12f);
        float cover = Math.Clamp(1f - MathF.Abs(off) / (2f * R), 0f, 1f);
        float total = Gfx.Smooth(0.95f, 0.995f, cover);
        float diamond = Gfx.Smooth(0.88f, 0.96f, cover) * (1f - Gfx.Smooth(0.985f, 0.998f, cover));

        // 皆既に近づくと空が暗くなる
        brush.Color = Gfx.C4(0.01f, 0.01f, 0.03f, 0.55f * Gfx.Smooth(0.7f, 1f, cover));
        dc.FillRectangle(new RawRectF(0, 0, w, h), brush);

        dc.PrimitiveBlend = PrimitiveBlend.Add;
        // 太陽の光（欠けるほど弱く）
        float sunA = 1f - 0.92f * Gfx.Smooth(0.5f, 1f, cover);
        DrawKit.Glow(dc, ctr, R * 3.5f * glow, Gfx.C4(SunColor, 0.45f * sunA));
        // コロナ
        if (total > 0.01f || diamond > 0.01f)
        {
            float ca = MathF.Max(total, diamond * 0.5f);
            using (var g = Gfx.Radial(dc, ctr, R * 2.6f, (0f, Gfx.C4(SunColor, 0.9f * ca)), (0.38f, Gfx.C4(SunColor, 0.75f * ca)), (0.55f, Gfx.C4(SunColor, 0.22f * ca)), (1f, Gfx.C4(SunColor, 0f))))
                dc.FillEllipse(new Ellipse(ctr, R * 2.6f, R * 2.6f), g);
            for (int i = 0; i < 28; i++)
            {
                float a = DrawKit.H(Seed, i, 1) * MathF.PI * 2f + MathF.Sin(t * 0.2f + i) * 0.03f;
                float len = R * (0.8f + 1.8f * DrawKit.H(Seed, i, 2));
                var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                Gfx.FadeLine(dc, brush, ctr + d * R * 0.98f, ctr + d * (R + len), R * 0.12f, R * 0.02f, SunColor, 0.35f * ca, 10, 1.4f);
            }
        }
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        // 太陽本体
        brush.Color = Gfx.C4(SunColor, 1f);
        dc.FillEllipse(new Ellipse(ctr, R, R), brush);
        // 月（黒い円）
        brush.Color = Gfx.C4(0.01f, 0.01f, 0.02f, 1f);
        dc.FillEllipse(new Ellipse(mc, R * 1.015f, R * 1.015f), brush);
        // ダイヤモンドリング
        if (diamond > 0.01f)
        {
            var dirv = ctr - mc;
            var nd = dirv.LengthSquared() > 1e-4f ? Vector2.Normalize(dirv) : new Vector2(-1f, 0f);
            var spot = ctr + nd * R;
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            DrawKit.Glow(dc, spot, R * 1.1f * glow, Gfx.C4(1f, 1f, 1f, diamond));
            DrawKit.Twinkle(dc, brush, spot, R * 1.6f * diamond, R * 0.05f, WpfColor.FromRgb(255, 255, 255), diamond, MathF.PI / 4f);
            DrawKit.Twinkle(dc, brush, spot, R * 0.9f * diamond, R * 0.03f, WpfColor.FromRgb(255, 255, 255), diamond * 0.7f, 0f);
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
        else if (total > 0.01f)
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            brush.Color = Gfx.C4(SunColor, 0.9f * total);
            dc.DrawEllipse(new Ellipse(ctr, R * 1.01f, R * 1.01f), brush, MathF.Max(1.5f, R * 0.02f));
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    void DrawLunar(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float R, float p, float glow)
    {
        float U = R * 2.7f, Pn = U * 1.22f;                   // 本影・半影の半径
        var sc = ctr + new Vector2((0.5f - p) * 2f * (U + R * 1.2f), R * 0.25f);
        float cover = Math.Clamp(1f - (Vector2.Distance(sc, ctr) - (U - R)) / (2f * R), 0f, 1f);

        dc.PrimitiveBlend = PrimitiveBlend.Add;
        DrawKit.Glow(dc, ctr, R * 2.2f * glow, Gfx.C4(Gfx.Mix(MoonColor, BloodColor, cover), 0.35f * (1f - 0.6f * cover)));
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        MoonDisk(dc, brush, ctr, R, MoonColor);

        // 1px の行ごとに、半影・本影に入っている部分を塗る
        var deep = Gfx.Mix(BloodColor, WpfColor.FromRgb(20, 6, 4), 0.35f);
        for (float y = ctr.Y - R; y < ctr.Y + R; y += 1f)
        {
            float dy = y + 0.5f - ctr.Y;
            float mh = MathF.Sqrt(MathF.Max(0f, R * R - dy * dy));
            if (mh < 0.5f) continue;
            float x0 = ctr.X - mh, x1 = ctr.X + mh;
            float sdy = y + 0.5f - sc.Y;
            // 半影（うすく暗く）
            float ph = MathF.Sqrt(MathF.Max(0f, Pn * Pn - sdy * sdy));
            if (ph > 0f)
            {
                float a = MathF.Max(x0, sc.X - ph), b = MathF.Min(x1, sc.X + ph);
                if (b > a) { brush.Color = Gfx.C4(0.05f, 0.02f, 0.02f, 0.35f); dc.FillRectangle(new RawRectF(a, y, b, y + 1f), brush); }
            }
            // 本影（赤銅色）
            float uh = MathF.Sqrt(MathF.Max(0f, U * U - sdy * sdy));
            if (uh > 0f)
            {
                float a = MathF.Max(x0, sc.X - uh), b = MathF.Min(x1, sc.X + uh);
                if (b > a)
                {
                    brush.Color = Gfx.C4(Gfx.Mix(deep, BloodColor, cover), 0.88f);
                    dc.FillRectangle(new RawRectF(a, y, b, y + 1f), brush);
                }
            }
        }
        if (Craters && cover > 0.3f)
        {
            brush.Color = Gfx.C4(0.15f, 0.03f, 0.02f, 0.25f * cover);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(-0.35f, -0.25f) * R, 0.28f * R, 0.26f * R), brush);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(0.35f, 0.15f) * R, 0.22f * R, 0.2f * R), brush);
        }
    }
}
