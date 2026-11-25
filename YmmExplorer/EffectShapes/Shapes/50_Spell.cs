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
//  魔法弾・属性弾
//   炎・氷・雷・風・闇・光の弾が尾を引いて飛び、着弾して弾ける
// =====================================================================
public sealed class SpellPlugin : IShapePlugin
{
    public string Name => "演出：魔法弾・属性弾";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpellParameter(sharedData);
}

public sealed class SpellParameter : EffectShapeParameterBase
{
    public SpellParameter() : this(null) { }
    public SpellParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "弾", Name = "属性")]
    [EnumComboBox]
    public ElementKind Element { get => element; set => Set(ref element, value); }
    ElementKind element = ElementKind.Fire;

    [Display(GroupName = "位置と向き", Name = "発射位置X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation StartX { get; } = new Animation(15, -100, 200);

    [Display(GroupName = "位置と向き", Name = "発射位置Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation StartY { get; } = new Animation(55, -100, 200);

    [Display(GroupName = "位置と向き", Name = "向き", Description = "0=右、90=下")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "位置と向き", Name = "飛ぶ距離")]
    [AnimationSlider("F0", "px", 200, 2400)]
    public Animation Distance { get; } = new Animation(1300, 10, 20000);

    [Display(GroupName = "弾", Name = "弾の半径")]
    [AnimationSlider("F0", "px", 10, 200)]
    public Animation Radius { get; } = new Animation(54, 3, 2000);

    [Display(GroupName = "弾", Name = "尾の長さ")]
    [AnimationSlider("F0", "px", 50, 1000)]
    public Animation Trail { get; } = new Animation(380, 5, 8000);

    [Display(GroupName = "弾", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "タイミング", Name = "くり返しの間隔")]
    [AnimationSlider("F1", "秒", 0.8, 12)]
    public Animation Interval { get; } = new Animation(3, 0.3, 120);

    [Display(GroupName = "タイミング", Name = "飛んでいる時間")]
    [AnimationSlider("F2", "秒", 0.2, 4)]
    public Animation Flight { get; } = new Animation(0.9, 0.05, 30);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [StartX, StartY, Angle, Distance, Radius, Trail, Brightness, Interval, Flight];

    // 属性ごとの色（外側・中間・芯）
    (WpfColor outer, WpfColor mid, WpfColor core) Palette() => Element switch
    {
        ElementKind.Fire => (WpfColor.FromRgb(0xFF, 0x3C, 0x0A), WpfColor.FromRgb(0xFF, 0x9A, 0x1E), WpfColor.FromRgb(0xFF, 0xF0, 0xA0)),
        ElementKind.Ice => (WpfColor.FromRgb(0x3A, 0x9C, 0xFF), WpfColor.FromRgb(0x8A, 0xDC, 0xFF), WpfColor.FromRgb(0xF0, 0xFC, 0xFF)),
        ElementKind.Thunder => (WpfColor.FromRgb(0x8A, 0x5C, 0xFF), WpfColor.FromRgb(0xFF, 0xE0, 0x4A), WpfColor.FromRgb(255, 255, 255)),
        ElementKind.Wind => (WpfColor.FromRgb(0x4D, 0xD0, 0x8A), WpfColor.FromRgb(0xB0, 0xFF, 0xD8), WpfColor.FromRgb(255, 255, 255)),
        ElementKind.Dark => (WpfColor.FromRgb(0x5A, 0x1E, 0xA0), WpfColor.FromRgb(0xA0, 0x40, 0xFF), WpfColor.FromRgb(0x1A, 0x08, 0x30)),
        _ => (WpfColor.FromRgb(0xFF, 0xD2, 0x4A), WpfColor.FromRgb(0xFF, 0xF0, 0xB0), WpfColor.FromRgb(255, 255, 255)),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var start = new Vector2(c.F(StartX) / 100f * w, c.F(StartY) / 100f * h);
        float ang = Gfx.Rad(c.F(Angle)), dist = c.F(Distance), R = c.F(Radius), trail = c.F(Trail), B = c.F(Brightness) / 100f;
        float P = MathF.Max(0.3f, c.F(Interval)), flight = MathF.Max(0.05f, c.F(Flight));
        var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
        var perp = new Vector2(-dir.Y, dir.X);
        var (outer, mid, core) = Palette();
        var white = WpfColor.FromRgb(255, 255, 255);

        float local = Gfx.Mod(t, P);
        float u = local / flight;                                  // 0〜1=飛行、1〜=着弾
        float fly = MathF.Min(1f, u);
        float travel = (1f - (1f - fly) * (1f - fly)) * 0.35f + fly * 0.65f;          // 出だしが少し速い
        var pos = start + dir * (dist * travel);
        float hit = u > 1f ? (u - 1f) * flight / 0.5f : -1f;      // 着弾からの進み（0〜1）

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            if (hit < 0f || hit < 0.2f)
            {
                float fadeBall = hit < 0f ? 1f : 1f - hit / 0.2f;
                // ---- 尾（属性ごとの粒）----
                var rt = new Random(Seed);
                int nt = 46;
                for (int i = 0; i < nt; i++)
                {
                    float age = i / (float)nt;
                    float jitter = (rt.NextF() - 0.5f);
                    var p = pos - dir * (trail * age * MathF.Min(1f, travel * 3f));
                    float rr = R * (1f - age) * 0.8f;
                    float a = (1f - age) * 0.55f * B * fadeBall;
                    switch (Element)
                    {
                        case ElementKind.Fire:
                            p += perp * jitter * R * 1.1f * age + new Vector2(0f, -age * R * 0.7f);
                            brush.Color = Gfx.C4(Gfx.Mix(mid, outer, age), a * 0.5f);
                            dc.FillEllipse(new Ellipse(p, rr, rr), brush);
                            break;
                        case ElementKind.Ice:
                            if (i % 3 == 0)
                            {
                                p += perp * jitter * R * 1.6f * age;
                                brush.Color = Gfx.C4(mid, a);
                                Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(p, rr * 0.55f + 2f, 4, 0.35f, age * 5f + i));
                            }
                            else
                            {
                                brush.Color = Gfx.C4(outer, a * 0.35f);
                                dc.FillEllipse(new Ellipse(p, rr * 0.8f, rr * 0.8f), brush);
                            }
                            break;
                        case ElementKind.Thunder:
                            p += perp * MathF.Sin(age * 22f + t * 30f) * R * 0.5f * age;
                            brush.Color = Gfx.C4(Gfx.Mix(mid, outer, age), a * 0.8f);
                            dc.FillEllipse(new Ellipse(p, rr * 0.55f, rr * 0.55f), brush);
                            break;
                        case ElementKind.Wind:
                            p += perp * MathF.Sin(age * 14f + t * 8f) * R * 0.9f * age;
                            brush.Color = Gfx.C4(mid, a * 0.7f);
                            dc.FillEllipse(new Ellipse(p, rr * 0.45f, rr * 0.45f), brush);
                            break;
                        case ElementKind.Dark:
                            p += perp * jitter * R * 1.4f * age;
                            brush.Color = Gfx.C4(Gfx.Mix(outer, mid, 1f - age), a * 0.7f);
                            dc.FillEllipse(new Ellipse(p, rr, rr), brush);
                            break;
                        default:
                            brush.Color = Gfx.C4(Gfx.Mix(mid, white, 0.4f), a * 0.7f);
                            dc.FillEllipse(new Ellipse(p, rr * 0.6f, rr * 0.6f), brush);
                            break;
                    }
                }
                if (Element == ElementKind.Wind)   // 風：弾を巻く渦の線
                {
                    for (int k = 0; k < 3; k++)
                    {
                        var pts = new List<Vector2>();
                        for (int i = 0; i <= 30; i++)
                        {
                            float f = i / 30f;
                            var q = pos - dir * (trail * 0.5f * f) + perp * MathF.Sin(f * 18f + t * 20f + k * 2.1f) * R * (0.9f - 0.5f * f);
                            pts.Add(q);
                        }
                        brush.Color = Gfx.C4(mid, 0.6f * B * fadeBall);
                        FormKit.Stroke(dc, brush, pts, 2f, false);
                    }
                }
                if (Element == ElementKind.Thunder)  // 雷：弾の周りの電光
                {
                    int tick = (int)MathF.Floor(t * 24f);
                    var rb = new Random(Seed * 31 + tick);
                    for (int k = 0; k < 5; k++)
                    {
                        float a0 = rb.NextF() * MathF.PI * 2f;
                        var pts = new List<Vector2> { pos };
                        var q = pos;
                        float aa = a0;
                        for (int i = 0; i < 5; i++)
                        {
                            aa += (rb.NextF() - 0.5f) * 1.6f;
                            q += new Vector2(MathF.Cos(aa), MathF.Sin(aa)) * (R * 0.45f);
                            pts.Add(q);
                        }
                        brush.Color = Gfx.C4(mid, 0.9f * B * fadeBall);
                        FormKit.Stroke(dc, brush, pts, 2f, false);
                    }
                }

                // ---- 弾本体 ----
                float og = R * 2.6f;
                using (var ob = Gfx.Radial(dc, pos, og, (0f, Gfx.C4(outer, 0.45f * B * fadeBall)), (1f, Gfx.C4(outer, 0f))))
                    dc.FillEllipse(new Ellipse(pos, og, og), ob);
                using (var bb = Gfx.Radial(dc, pos, R,
                    (0f, Gfx.C4(Element == ElementKind.Dark ? mid : core, B * fadeBall)), (0.5f, Gfx.C4(mid, 0.9f * B * fadeBall)), (1f, Gfx.C4(outer, 0f))))
                    dc.FillEllipse(new Ellipse(pos, R, R), bb);
                if (Element == ElementKind.Dark)
                {
                    using var db = Gfx.Radial(dc, pos, R * 0.7f, (0f, Gfx.C4(core, 0.9f * fadeBall)), (1f, Gfx.C4(core, 0f)));
                    dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                    dc.FillEllipse(new Ellipse(pos, R * 0.7f, R * 0.7f), db);
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                }
            }

            // ---- 着弾：閃光・衝撃の輪・火花 ----
            if (hit >= 0f && hit <= 1f)
            {
                var ip = start + dir * dist;
                float fa = 1f - hit;
                float fr = R * (1.5f + 3.5f * hit);
                using (var fb = Gfx.Radial(dc, ip, fr, (0f, Gfx.C4(core, 0.9f * fa * B)), (0.4f, Gfx.C4(mid, 0.5f * fa * B)), (1f, Gfx.C4(outer, 0f))))
                    dc.FillEllipse(new Ellipse(ip, fr, fr), fb);
                brush.Color = Gfx.C4(mid, 0.8f * fa * B);
                dc.DrawEllipse(new Ellipse(ip, R * (1f + 4f * hit), R * (1f + 4f * hit)), brush, 4f * fa + 1f);
                var rs = new Random(Seed + 9);
                for (int i = 0; i < 26; i++)
                {
                    float a = rs.NextF() * MathF.PI * 2f, sp = R * (1.5f + 4f * rs.NextF());
                    var d2 = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    var p = ip + d2 * (sp * hit);
                    brush.Color = Gfx.C4(Gfx.Mix(mid, white, 0.4f), 0.9f * fa * B);
                    dc.DrawLine(p, p - d2 * (8f + 16f * rs.NextF()), brush, 2f);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
