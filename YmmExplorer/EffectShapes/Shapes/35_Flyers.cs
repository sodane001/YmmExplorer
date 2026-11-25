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
//  蝶・鳥の群れ
//   ひらひら舞う蝶、V字で飛ぶ鳥、群れで飛ぶ鳥
// =====================================================================
public sealed class FlyersPlugin : IShapePlugin
{
    public string Name => "演出：蝶・鳥の群れ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FlyersParameter(sharedData);
}

public sealed class FlyersParameter : EffectShapeParameterBase
{
    public FlyersParameter() : this(null) { }
    public FlyersParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public FlyKind Kind { get => kind; set => Set(ref kind, value); }
    FlyKind kind = FlyKind.Butterfly;

    [Display(GroupName = "色", Name = "上の羽・鳥の色")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0xFF, 0x9A, 0x2E);

    [Display(GroupName = "色", Name = "下の羽の色（蝶）")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0xFF, 0x4F, 0x8A);

    [Display(GroupName = "数と大きさ", Name = "数")]
    [AnimationSlider("F0", "匹", 1, 60)]
    public Animation Count { get; } = new Animation(8, 1, 500);

    [Display(GroupName = "数と大きさ", Name = "大きさ")]
    [AnimationSlider("F0", "px", 10, 200)]
    public Animation Size { get; } = new Animation(40, 3, 2000);

    [Display(GroupName = "動き", Name = "飛ぶ速さ")]
    [AnimationSlider("F0", "px/s", 20, 800)]
    public Animation Speed { get; } = new Animation(180, 1, 8000);

    [Display(GroupName = "動き", Name = "羽ばたく速さ")]
    [AnimationSlider("F0", "%", 20, 400)]
    public Animation Flap { get; } = new Animation(100, 5, 2000);

    [Display(GroupName = "動き", Name = "飛ぶ高さ位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public new Animation Height { get; } = new Animation(35, -100, 200);

    [Display(GroupName = "動き", Name = "飛ぶ向き（鳥）", Description = "0=右へ、180=左へ")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Heading { get; } = new Animation(0, -3600, 3600);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Speed, Flap, Height, Heading];

    // 鳥のシルエット（羽ばたきで「m」形が上下する）
    static void Bird(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 p, float L, float flap, float ang)
    {
        float cs = MathF.Cos(ang), sn = MathF.Sin(ang);
        Vector2 Tf(float x, float y) => p + new Vector2(x * cs - y * sn, x * sn + y * cs);
        for (int side = -1; side <= 1; side += 2)
        {
            var pts = new List<Vector2>
            {
                Tf(0f, 0f),
                Tf(side * L * 0.28f, -L * 0.28f * flap),
                Tf(side * L * 0.6f, -L * 0.18f * flap + L * 0.04f),
                Tf(side * L, L * 0.12f - L * 0.5f * flap * 0.5f),
            };
            Gfx.Polyline(dc, brush, pts, MathF.Max(1.6f, L * 0.09f));
        }
        dc.FillEllipse(new Ellipse(p, L * 0.07f, L * 0.07f), brush);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), speed = c.F(Speed), flapR = c.F(Flap) / 100f, hy = c.F(Height) / 100f * h;
        float heading = Gfx.Rad(c.F(Heading));
        var hv = new Vector2(MathF.Cos(heading), MathF.Sin(heading));
        var hp = new Vector2(-hv.Y, hv.X);
        float span = MathF.Abs(hv.X) > 0.3f ? w + size * 14f : h + size * 14f;

        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);
        try
        {
            if (Kind == FlyKind.Butterfly)
            {
                for (int i = 0; i < n; i++)
                {
                    float bx = rng.NextF() * w, by = rng.NextF() * h * 0.8f + h * 0.1f;
                    float ax = 160f + 260f * rng.NextF(), ay = 80f + 160f * rng.NextF();
                    float fx = 0.08f + 0.18f * rng.NextF(), fy = 0.1f + 0.2f * rng.NextF(), p1 = rng.NextF() * 6.28f, p2 = rng.NextF() * 6.28f;
                    float fl = (7f + 5f * rng.NextF()) * flapR, ph = rng.NextF() * 6.28f, d = 0.6f + 0.6f * rng.NextF();
                    var pos = FormKit.Liss(new Vector2(bx, by), ax, ay, t * speed / 180f, fx, fy, p1, p2);
                    var prev = FormKit.Liss(new Vector2(bx, by), ax, ay, t * speed / 180f - 0.05f, fx, fy, p1, p2);
                    float tilt = MathF.Atan2(pos.Y - prev.Y, pos.X - prev.X) * 0.25f;
                    float L = size * d;
                    float open = MathF.Abs(MathF.Cos(t * fl + ph));                  // 羽の開き（1=全開）
                    float ox = MathF.Max(0.12f, open);

                    dc.Transform = Matrix3x2.CreateRotation(tilt, pos);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        var up = pos + new Vector2(side * L * 0.55f * ox, -L * 0.25f);
                        var lo = pos + new Vector2(side * L * 0.42f * ox, L * 0.32f);
                        brush.Color = Gfx.C4(Color1, 0.95f);
                        dc.FillEllipse(new Ellipse(up, L * 0.62f * ox, L * 0.46f), brush);
                        brush.Color = Gfx.C4(Color2, 0.95f);
                        dc.FillEllipse(new Ellipse(lo, L * 0.42f * ox, L * 0.34f), brush);
                        brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.45f);
                        dc.FillEllipse(new Ellipse(up + new Vector2(side * L * 0.15f * ox, -L * 0.05f), L * 0.12f * ox, L * 0.1f), brush);
                    }
                    brush.Color = Gfx.C4(WpfColor.FromRgb(0x30, 0x20, 0x30), 1f);
                    dc.DrawLine(pos + new Vector2(0f, -L * 0.3f), pos + new Vector2(0f, L * 0.4f), brush, MathF.Max(1.5f, L * 0.08f));
                    dc.DrawLine(pos + new Vector2(0f, -L * 0.3f), pos + new Vector2(-L * 0.18f, -L * 0.55f), brush, 1f);
                    dc.DrawLine(pos + new Vector2(0f, -L * 0.3f), pos + new Vector2(L * 0.18f, -L * 0.55f), brush, 1f);
                    dc.Transform = Matrix3x2.Identity;
                }
            }
            else
            {
                brush.Color = Gfx.C4(Color1, 1f);
                float travel = speed * t;
                if (Kind == FlyKind.BirdsV)
                {
                    // 先頭を中心に、後ろへ左右交互に並ぶ
                    float lead = Gfx.Mod(travel, span) - size * 7f;
                    for (int i = 0; i < n; i++)
                    {
                        int rank = (i + 1) / 2, sideSign = i == 0 ? 0 : (i % 2 == 1 ? 1 : -1);
                        var origin = new Vector2(w / 2f, h / 2f);
                        float along = (MathF.Abs(hv.X) > 0.3f ? -w / 2f : -h / 2f) + lead;
                        var bp = origin + hv * (along - rank * size * 1.5f) + hp * (sideSign * rank * size * 1.1f);
                        bp += hp * (hy - h / 2f);
                        float fl = MathF.Sin(t * 7f * flapR + i * 0.4f);
                        Bird(dc, brush, bp, size, 0.5f + 0.5f * fl, heading * 0f);
                    }
                }
                else
                {
                    for (int i = 0; i < n; i++)
                    {
                        float off = rng.NextF() * span, lat = (rng.NextF() * 2f - 1f) * h * 0.14f, ph = rng.NextF() * 6.28f, sp = 0.85f + 0.3f * rng.NextF(), sc = 0.6f + 0.6f * rng.NextF();
                        float along = Gfx.Mod(travel * sp + off, span) - size * 7f;
                        var origin = new Vector2(w / 2f, h / 2f);
                        var bp = origin + hv * (along + (MathF.Abs(hv.X) > 0.3f ? -w / 2f : -h / 2f)) + hp * (hy - h / 2f + lat + MathF.Sin(t * 0.8f + ph) * 18f);
                        float fl = MathF.Sin(t * 7f * flapR * (0.8f + 0.4f * sp) + ph);
                        Bird(dc, brush, bp, size * sc, 0.5f + 0.5f * fl, 0f);
                    }
                }
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
