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
//  爆発
//   閃光 → 火球 → 衝撃波 → 火花と破片 → 立ちのぼる煙
//   連鎖爆発にも対応。前の爆発は消えずに余韻が残る
// =====================================================================
public sealed class ExplosionPlugin : IShapePlugin
{
    public string Name => "演出：爆発";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ExplosionParameter(sharedData);
}

public sealed class ExplosionParameter : EffectShapeParameterBase
{
    public ExplosionParameter() : this(null) { }
    public ExplosionParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(60, -100, 200);

    [Display(GroupName = "爆発", Name = "大きさ（半径）")]
    [AnimationSlider("F0", "px", 50, 700)]
    public Animation Size { get; } = new Animation(220, 5, 5000);

    [Display(GroupName = "爆発", Name = "持続時間")]
    [AnimationSlider("F1", "秒", 0.8, 6)]
    public Animation Life { get; } = new Animation(2.6, 0.5, 30);

    [Display(GroupName = "爆発", Name = "発生間隔")]
    [AnimationSlider("F1", "秒", 1, 12)]
    public Animation Interval { get; } = new Animation(4, 0.5, 120);

    [Display(GroupName = "爆発", Name = "連鎖する数", Description = "1回の発生で、位置をずらして続けて爆発する数")]
    [AnimationSlider("F0", "発", 1, 6)]
    public Animation Chain { get; } = new Animation(1, 1, 30);

    [Display(GroupName = "爆発", Name = "連鎖の広がり")]
    [AnimationSlider("F0", "px", 0, 600)]
    public Animation Spread { get; } = new Animation(180, 0, 5000);

    [Display(GroupName = "爆発", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "色", Name = "中心の色")]
    [ColorPicker]
    public WpfColor CoreColor { get => core; set => Set(ref core, value); }
    WpfColor core = WpfColor.FromRgb(0xFF, 0xF4, 0xC0);

    [Display(GroupName = "色", Name = "炎の色")]
    [ColorPicker]
    public WpfColor FireColor { get => fire; set => Set(ref fire, value); }
    WpfColor fire = WpfColor.FromRgb(0xFF, 0x6A, 0x14);

    [Display(GroupName = "色", Name = "煙の色")]
    [ColorPicker]
    public WpfColor SmokeColor { get => smoke; set => Set(ref smoke, value); }
    WpfColor smoke = WpfColor.FromRgb(0x2E, 0x2A, 0x28);

    [Display(GroupName = "要素", Name = "衝撃波の強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Shockwave { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "要素", Name = "煙の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Smoke { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "要素", Name = "煙の量", Description = "煙のかたまりの数。0で煙なし、多いほどもくもくする")]
    [AnimationSlider("F0", "個", 0, 60)]
    public Animation SmokeAmount { get; } = new Animation(18, 0, 300);

    [Display(GroupName = "要素", Name = "火花の数")]
    [AnimationSlider("F0", "個", 0, 300)]
    public Animation SparkCount { get; } = new Animation(70, 0, 3000);

    [Display(GroupName = "要素", Name = "破片の数")]
    [AnimationSlider("F0", "個", 0, 60)]
    public Animation DebrisCount { get; } = new Animation(12, 0, 500);

    [Display(GroupName = "要素", Name = "重力")]
    [AnimationSlider("F0", "px/s²", 0, 3000)]
    public Animation Gravity { get; } = new Animation(900, 0, 20000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Size, Life, Interval, Chain, Spread, Brightness, Shockwave, Smoke, SmokeAmount, SparkCount, DebrisCount, Gravity];

    readonly record struct Boom(
        float R, float Life, float Bright, float Shock, float SmokeK, int SmokeBlobs, int Sparks, int Debris, float G,
        WpfColor Core, WpfColor Fire, WpfColor SmokeCol, float W, float H);

    static float EaseOut(float u) { u = Math.Clamp(u, 0f, 1f); return 1f - (1f - u) * (1f - u) * (1f - u); }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float P = MathF.Max(0.5f, c.F(Interval));
        int chain = Math.Max(1, (int)c.F(Chain));
        float spreadPx = c.F(Spread);
        var cfg = new Boom(
            c.F(Size), MathF.Max(0.5f, c.F(Life)), c.F(Brightness) / 100f, c.F(Shockwave) / 100f, c.F(Smoke) / 100f, (int)c.F(SmokeAmount),
            (int)c.F(SparkCount), (int)c.F(DebrisCount), c.F(Gravity),
            CoreColor, FireColor, SmokeColor, w, h);
        var basePos = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);

        float chainSpan = chain > 1 ? 0.18f * chain + 0.1f : 0f;
        int kmax = Math.Min(3, (int)MathF.Ceiling((cfg.Life + chainSpan) / P));

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // パス0: 煙と破片（通常合成）→ パス1: 炎・衝撃波・火花（加算合成）
            for (int pass = 0; pass < 2; pass++)
            {
                dc.PrimitiveBlend = pass == 0 ? PrimitiveBlend.SourceOver : PrimitiveBlend.Add;
                for (int k = 0; k <= kmax; k++)
                {
                    int cyc = (int)MathF.Floor(t / P) - k;
                    float local = t - cyc * P;
                    for (int j = 0; j < chain; j++)
                    {
                        var rj = new Random(Seed * 7919 + cyc * 131 + j * 17);
                        float delay = j * 0.18f + rj.NextF() * 0.1f;
                        float te = local - delay;
                        if (te < 0f || te >= cfg.Life) continue;

                        var offset = j == 0 ? Vector2.Zero
                            : new Vector2((rj.NextF() * 2f - 1f) * spreadPx, (rj.NextF() * 2f - 1f) * spreadPx * 0.6f);
                        var center = basePos + offset;
                        int seed = Seed * 1009 + cyc * 7919 + j * 31;
                        DrawBoom(dc, brush, cfg, pass, center, te, seed);
                    }
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            dc.Transform = Matrix3x2.Identity;
        }
    }

    static void DrawBoom(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Boom b, int pass, Vector2 center, float te, int seed)
    {
        float R = b.R, life = b.Life;
        var white = WpfColor.FromRgb(255, 255, 255);

        if (pass == 0)
        {
            // ---- 煙（立ちのぼりながら大きくなり、薄れる）----
            if (b.SmokeK > 0f && b.SmokeBlobs > 0)
            {
                var rs = new Random(seed + 3);
                for (int i = 0; i < b.SmokeBlobs; i++)
                {
                    float ang = rs.NextF() * MathF.PI * 2f;
                    float dist = 0.5f + 0.5f * rs.NextF();
                    float delay = 0.1f + rs.NextF() * 0.25f;
                    float sz = 0.7f + 0.6f * rs.NextF();
                    float u = (te - delay) / (life - delay);
                    if (u <= 0f || u >= 1f) continue;

                    var pos = center + new Vector2(MathF.Cos(ang), MathF.Sin(ang) * 0.6f) * (R * 0.9f * EaseOut(u) * dist)
                              + new Vector2(0f, -R * 0.6f * u);
                    float r = R * (0.45f + 0.75f * u) * sz;
                    float a = b.SmokeK * 0.5f * MathF.Sin(MathF.PI * u);
                    var col = Gfx.Mix(b.SmokeCol, b.Fire, MathF.Pow(1f - u, 3f) * 0.5f);
                    using var g = Gfx.Radial(dc, pos, r,
                        (0f, Gfx.C4(col, a)), (0.55f, Gfx.C4(col, a * 0.6f)), (1f, Gfx.C4(col, 0f)));
                    dc.FillEllipse(new Ellipse(pos, r, r), g);
                }
            }

            // ---- 破片（暗い塊が回りながら飛んで落ちる）----
            if (b.Debris > 0)
            {
                var rd = new Random(seed + 5);
                var dark = Gfx.Mix(b.SmokeCol, WpfColor.FromRgb(0, 0, 0), 0.4f);
                for (int i = 0; i < b.Debris; i++)
                {
                    float ang = -MathF.PI / 2f + (rd.NextF() - 0.5f) * 2.4f;
                    float sp = R * (1.2f + 2.2f * rd.NextF());
                    float spin = (rd.NextF() - 0.5f) * 14f;
                    float sz = R * 0.04f * (0.6f + rd.NextF());
                    float rot0 = rd.NextF() * MathF.PI * 2f;

                    var pos = center + new Vector2(MathF.Cos(ang) * sp * te, MathF.Sin(ang) * sp * te + 0.5f * b.G * te * te);
                    if (pos.Y > b.H + 60f) continue;
                    float alpha = 1f - Gfx.Smooth(life * 0.6f, life * 0.9f, te);
                    if (alpha <= 0.01f) continue;

                    dc.Transform = Matrix3x2.CreateRotation(rot0 + spin * te, pos);
                    brush.Color = Gfx.C4(dark, 0.95f * alpha);
                    Gfx.Rect(dc, brush, pos.X - sz, pos.Y - sz * 0.6f, sz * 2f, sz * 1.2f);
                    brush.Color = Gfx.C4(b.Fire, 0.7f * alpha * (1f - te / life));
                    Gfx.Rect(dc, brush, pos.X - sz * 0.5f, pos.Y - sz * 0.3f, sz, sz * 0.6f);
                    dc.Transform = Matrix3x2.Identity;
                }
            }
            return;
        }

        // ---------------- パス1：加算合成 ----------------

        // ---- 閃光 ----
        if (te < 0.15f)
        {
            float fa = 1f - te / 0.15f;
            float fr = R * 1.6f;
            using var fb = Gfx.Radial(dc, center, fr,
                (0f, Gfx.C4(white, 0.9f * fa * b.Bright)),
                (0.3f, Gfx.C4(b.Core, 0.5f * fa * b.Bright)),
                (1f, Gfx.C4(b.Fire, 0f)));
            dc.FillEllipse(new Ellipse(center, fr, fr), fb);
        }

        // ---- 火球（もくもくと広がって冷めていく）----
        float fireLife = life * 0.45f;
        float fu = te / fireLife;
        if (fu < 1f)
        {
            var rf = new Random(seed + 1);
            var emberRed = WpfColor.FromRgb(120, 20, 5);
            for (int i = 0; i < 26; i++)
            {
                float ang = rf.NextF() * MathF.PI * 2f;
                float df = 0.3f + 0.7f * rf.NextF();
                float sz = 0.6f + 0.8f * rf.NextF();
                float rr = rf.NextF();

                float e = EaseOut(fu);
                var pos = center + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * (R * 0.7f * e * df);
                float r = R * (0.28f + 0.55f * e) * sz;
                var c1 = Gfx.Mix(b.Core, b.Fire, Math.Clamp(fu * 1.6f + rr * 0.2f, 0f, 1f));
                c1 = Gfx.Mix(c1, emberRed, fu * fu * 0.6f);
                float a = MathF.Pow(1f - fu, 1.3f) * 0.55f * b.Bright;
                using var g = Gfx.Radial(dc, pos, r,
                    (0f, Gfx.C4(c1, a)), (0.5f, Gfx.C4(c1, a * 0.5f)), (1f, Gfx.C4(c1, 0f)));
                dc.FillEllipse(new Ellipse(pos, r, r), g);
            }
            // 中心の白熱
            float cr = R * 0.5f * (1f - fu);
            if (cr > 1f)
            {
                using var cb = Gfx.Radial(dc, center, cr,
                    (0f, Gfx.C4(white, 0.8f * (1f - fu) * b.Bright)), (1f, Gfx.C4(b.Core, 0f)));
                dc.FillEllipse(new Ellipse(center, cr, cr), cb);
            }
        }

        // ---- 衝撃波 ----
        float su = te / 0.7f;
        if (su < 1f && b.Shock > 0f)
        {
            float r = R * 2.4f * EaseOut(su);
            float a = MathF.Pow(1f - su, 1.5f) * b.Shock * b.Bright;
            brush.Color = Gfx.C4(b.Core, a * 0.25f);
            dc.DrawEllipse(new Ellipse(center, r, r), brush, 22f * (1f - su) + 4f);
            brush.Color = Gfx.C4(white, a * 0.8f);
            dc.DrawEllipse(new Ellipse(center, r, r), brush, 5f * (1f - su) + 1.5f);
        }

        // ---- 火花（尾を引いて飛び散り、重力で落ちる）----
        float sparkLife = life * 0.55f;
        if (te < sparkLife && b.Sparks > 0)
        {
            var rp = new Random(seed + 2);
            for (int i = 0; i < b.Sparks; i++)
            {
                float ang = rp.NextF() * MathF.PI * 2f;
                float sp = R * (2f + 4f * rp.NextF());
                float cm = rp.NextF();
                float wd = 2f * (0.5f + rp.NextF());

                float te2 = MathF.Max(0f, te - 0.06f);
                var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                float d1 = sp * (1f - MathF.Exp(-te * 2.5f)) / 2.5f;
                float d0 = sp * (1f - MathF.Exp(-te2 * 2.5f)) / 2.5f;
                var p1 = center + dir * d1 + new Vector2(0f, 0.5f * b.G * te * te);
                var p0 = center + dir * d0 + new Vector2(0f, 0.5f * b.G * te2 * te2);

                float a = MathF.Pow(1f - te / sparkLife, 1.2f) * b.Bright;
                brush.Color = Gfx.C4(Gfx.Mix(b.Core, b.Fire, cm), a);
                dc.DrawLine(p0, p1, brush, wd);
            }
        }
    }
}
