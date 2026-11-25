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
//  ビーム
//   溜め → 発射 → 照射 → 消える。うねる光線・内部の電光・着弾の閃光
// =====================================================================
public sealed class BeamPlugin : IShapePlugin
{
    public string Name => "演出：ビーム";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BeamParameter(sharedData);
}

public sealed class BeamParameter : EffectShapeParameterBase
{
    public BeamParameter() : this(null) { }
    public BeamParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "ビームの色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x6A, 0xE0, 0xFF);

    [Display(GroupName = "位置と向き", Name = "発射位置X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation OriginX { get; } = new Animation(15, -100, 200);

    [Display(GroupName = "位置と向き", Name = "発射位置Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation OriginY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と向き", Name = "向き", Description = "0=右、90=下")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "位置と向き", Name = "長さ")]
    [AnimationSlider("F0", "px", 200, 4000)]
    public Animation Length { get; } = new Animation(2400, 10, 30000);

    [Display(GroupName = "ビーム", Name = "太さ")]
    [AnimationSlider("F0", "px", 20, 500)]
    public Animation BeamWidth { get; } = new Animation(150, 3, 5000);

    [Display(GroupName = "ビーム", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "ビーム", Name = "うねり")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Wave { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "動き", Name = "自動で繰り返す")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "動き", Name = "1回の長さ（自動）")]
    [AnimationSlider("F1", "秒", 2, 20)]
    public Animation Cycle { get; } = new Animation(6, 1, 120);

    [Display(GroupName = "動き", Name = "溜める時間（自動）")]
    [AnimationSlider("F1", "秒", 0, 5)]
    public Animation ChargeTime { get; } = new Animation(1.2, 0, 60);

    [Display(GroupName = "動き", Name = "照射する時間（自動）")]
    [AnimationSlider("F1", "秒", 0.5, 10)]
    public Animation FireTime { get; } = new Animation(3, 0.2, 60);

    [Display(GroupName = "動き", Name = "溜め具合（手動）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Charge { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "動き", Name = "照射の強さ（手動）", Description = "「自動で繰り返す」がオフのとき。キーフレームで発射と消失を操作")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Power { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [OriginX, OriginY, Angle, Length, BeamWidth, Brightness, Wave, Cycle, ChargeTime, FireTime, Charge, Power];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var origin = new Vector2(c.F(OriginX) / 100f * w, c.F(OriginY) / 100f * h);
        float ang = Gfx.Rad(c.F(Angle)), L = c.F(Length), W = c.F(BeamWidth);
        float bright = c.F(Brightness) / 100f, wave = c.F(Wave) / 100f;
        var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
        var perp = new Vector2(-dir.Y, dir.X);
        var white = WpfColor.FromRgb(255, 255, 255);
        var hot = Gfx.Mix(Color, white, 0.65f);

        // ---- 溜め具合(chg)と照射の強さ(pw) ----
        float chg, pw;
        if (Auto)
        {
            float P = MathF.Max(1f, c.F(Cycle)), ct = MathF.Max(0f, c.F(ChargeTime)), ft = MathF.Max(0.2f, c.F(FireTime));
            float local = Gfx.Mod(t, P);
            if (local < ct) { chg = ct > 0f ? local / ct : 1f; pw = 0f; }
            else
            {
                float f = local - ct;
                chg = 1f;
                pw = f < ft ? Gfx.Smooth(0f, 0.15f, f) * (1f - Gfx.Smooth(ft - 0.4f, ft, f)) : 0f;
            }
        }
        else
        {
            chg = c.F(Charge) / 100f;
            pw = c.F(Power) / 100f;
        }
        if (chg <= 0.002f && pw <= 0.002f) return;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 発射口の光球 ----
            float orbR = W * (0.25f + 0.6f * chg + 0.5f * pw) * (1f + 0.08f * MathF.Sin(t * 25f));
            float og = orbR * 2.4f;
            using (var ob = Gfx.Radial(dc, origin, og,
                (0f, Gfx.C4(white, 0.8f * bright)), (0.3f, Gfx.C4(Color, 0.35f * bright)), (1f, Gfx.C4(Color, 0f))))
                dc.FillEllipse(new Ellipse(origin, og, og), ob);
            for (int k = 0; k < 3; k++)
            {
                float a0 = t * (3f + k) * (k % 2 == 0 ? 1f : -1f) + k * 2f;
                brush.Color = Gfx.C4(hot, 0.7f * bright);
                FormKit.Stroke(dc, brush, Gfx.ArcPts(origin, orbR * (1.1f + 0.15f * k), a0, a0 + 1.2f, 16), 2.4f, false);
            }

            if (pw <= 0.002f) return;

            // ---- 本体（うねる太さ）----
            const int N = 90;
            for (int s = 0; s < N; s++)
            {
                float f0 = s / (float)N, f1 = (s + 1) / (float)N;
                float d0 = L * f0, d1 = L * f1;
                float wv = 1f + wave * (0.10f * MathF.Sin(d0 * 0.012f - t * 25f) + 0.05f * MathF.Sin(d0 * 0.031f - t * 41f));
                float grow = 0.55f + 0.45f * MathF.Min(1f, d0 / (W * 2f + 1f));     // 根元は少し細い
                float lw = W * pw * wv * grow;
                var p0 = origin + dir * d0;
                var p1 = origin + dir * d1;
                brush.Color = Gfx.C4(Color, 0.10f * bright * pw);
                dc.DrawLine(p0, p1, brush, lw * 2.3f);
                brush.Color = Gfx.C4(Color, 0.32f * bright * pw);
                dc.DrawLine(p0, p1, brush, lw * 1.35f);
                brush.Color = Gfx.C4(hot, 0.85f * bright * pw);
                dc.DrawLine(p0, p1, brush, lw * 0.7f);
                brush.Color = Gfx.C4(white, 0.95f * bright * pw);
                dc.DrawLine(p0, p1, brush, lw * 0.3f);
            }

            // ---- 内部を走る電光 ----
            for (int k = 0; k < 5; k++)
            {
                float off0 = (k - 2) * W * 0.17f, ph = k * 1.9f;
                var pts = new List<Vector2>();
                for (int s = 0; s <= 60; s++)
                {
                    float d = L * s / 60f;
                    float o = off0 + MathF.Sin(d * 0.02f - t * 30f + ph) * W * 0.12f * pw;
                    pts.Add(origin + dir * d + perp * o);
                }
                brush.Color = Gfx.C4(white, 0.45f * bright * pw);
                FormKit.Stroke(dc, brush, pts, 1.6f, false);
            }

            // ---- 縁からこぼれる火花 ----
            var rs = new Random(Seed);
            for (int i = 0; i < 40; i++)
            {
                float d = rs.NextF() * L, side = rs.NextF() < 0.5f ? -1f : 1f;
                float ph = rs.NextF(), sp = 0.8f + rs.NextF();
                float u = Gfx.Mod(t * sp + ph, 1f);
                var p = origin + dir * (d + u * 60f) + perp * (side * (W * 0.55f * pw + u * W * 0.9f));
                brush.Color = Gfx.C4(hot, 0.8f * (1f - u) * bright * pw);
                dc.FillEllipse(new Ellipse(p, 2.2f * (1f - u) + 0.8f, 2.2f * (1f - u) + 0.8f), brush);
            }

            // ---- 着弾の閃光（ビームの終点が画面内のとき）----
            var end = origin + dir * L;
            if (end.X > -200f && end.X < w + 200f && end.Y > -200f && end.Y < h + 200f)
            {
                float er = W * 2.2f * pw * (1f + 0.1f * MathF.Sin(t * 30f));
                using var eb = Gfx.Radial(dc, end, er,
                    (0f, Gfx.C4(white, 0.9f * bright * pw)), (0.35f, Gfx.C4(Color, 0.4f * bright * pw)), (1f, Gfx.C4(Color, 0f)));
                dc.FillEllipse(new Ellipse(end, er, er), eb);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
