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
//  エネルギー弾
//   エネルギーを溜める → 弾が膨らむ → 発射。自動で繰り返すか、手動のキーフレームで操作できる
// =====================================================================
public sealed class EnergyBallPlugin : IShapePlugin
{
    public string Name => "演出：エネルギー弾";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new EnergyBallParameter(sharedData);
}

public sealed class EnergyBallParameter : EffectShapeParameterBase
{
    public EnergyBallParameter() : this(null) { }
    public EnergyBallParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "エネルギーの色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x6A, 0xE0, 0xFF);

    [Display(GroupName = "位置", Name = "X位置（溜める場所）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(30, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（溜める場所）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "弾", Name = "半径")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation Radius { get; } = new Animation(110, 3, 4000);

    [Display(GroupName = "弾", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "弾", Name = "集まる粒の数")]
    [AnimationSlider("F0", "個", 0, 200)]
    public Animation Particles { get; } = new Animation(60, 0, 1000);

    [Display(GroupName = "発射", Name = "発射の向き", Description = "0=右、90=下")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "発射", Name = "飛ぶ距離")]
    [AnimationSlider("F0", "px", 200, 2500)]
    public Animation Distance { get; } = new Animation(1300, 10, 20000);

    [Display(GroupName = "動き", Name = "自動で繰り返す")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "動き", Name = "1回の長さ（自動）")]
    [AnimationSlider("F1", "秒", 1, 12)]
    public Animation Cycle { get; } = new Animation(4, 0.5, 120);

    [Display(GroupName = "動き", Name = "溜める割合（自動）")]
    [AnimationSlider("F0", "%", 10, 90)]
    public Animation ChargeRatio { get; } = new Animation(65, 1, 99);

    [Display(GroupName = "動き", Name = "溜め具合（手動）", Description = "「自動で繰り返す」がオフのとき。0→100%で弾が大きくなる")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Charge { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "動き", Name = "発射の進み（手動）", Description = "「自動で繰り返す」がオフのとき。0→100%で発射して飛んでいく")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Fire { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Brightness, Particles, Angle, Distance, Cycle, ChargeRatio, Charge, Fire];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var home = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), bright = c.F(Brightness) / 100f;
        int np = (int)c.F(Particles);
        float ang = Gfx.Rad(c.F(Angle)), dist = c.F(Distance);
        var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
        var white = WpfColor.FromRgb(255, 255, 255);

        // ---- 溜め具合(chg)と発射の進み(fire) ----
        float chg, fire;
        if (Auto)
        {
            float P = MathF.Max(0.5f, c.F(Cycle));
            float ratio = c.F(ChargeRatio) / 100f;
            float p = Gfx.Mod(t, P) / P;
            chg = p < ratio ? p / ratio : 1f;
            fire = p < ratio ? 0f : (p - ratio) / (1f - ratio);
        }
        else
        {
            chg = c.F(Charge) / 100f;
            fire = c.F(Fire) / 100f;
        }
        if (chg <= 0.002f) return;

        float easeChg = 1f - (1f - chg) * (1f - chg);
        float orbR = R * (0.2f + 0.8f * easeChg) * (1f + 0.06f * MathF.Sin(t * 14f)) * (1f - 0.25f * fire);
        float travel = MathF.Pow(fire, 2.2f) * dist;
        var pos = home + dir * travel;
        float fade = 1f - Gfx.Smooth(0.85f, 1f, fire);
        float B = bright * fade;
        var col = Color;
        var hot = Gfx.Mix(col, white, 0.6f);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 集まってくる粒（溜めている間）----
            if (fire <= 0f)
            {
                var rp = new Random(Seed);
                for (int i = 0; i < np; i++)
                {
                    float a0 = rp.NextF() * MathF.PI * 2f, ph = rp.NextF(), sp = 0.6f + 0.8f * rp.NextF(), spin = (rp.NextF() - 0.5f) * 3f;
                    float u = Gfx.Mod(t * sp * 0.9f + ph, 1f);
                    float rr = orbR * (0.8f + 5f * (1f - u));
                    float a = a0 + spin * (1f - u);
                    var p = pos + new Vector2(MathF.Cos(a), MathF.Sin(a)) * rr;
                    var pTail = pos + new Vector2(MathF.Cos(a - spin * 0.15f), MathF.Sin(a - spin * 0.15f)) * (rr + orbR * 0.5f);
                    Gfx.FadeLine(dc, brush, p, pTail, 2.2f, 0.4f, col, 0.8f * u * chg * B, 5, 1f);
                }
            }

            // ---- 発射後の尾 ----
            if (fire > 0f)
            {
                float speedK = MathF.Min(1f, fire * 3f);
                Gfx.FadeLine(dc, brush, pos, pos - dir * MathF.Min(travel, R * (3f + 10f * speedK)), orbR * 1.5f, orbR * 0.2f, col, 0.7f * B, 16, 1.1f);
            }

            // ---- 外側のにじみ ----
            float og = orbR * 2.6f;
            using (var ob = Gfx.Radial(dc, pos, og,
                (0f, Gfx.C4(col, 0.30f * B)), (0.4f, Gfx.C4(col, 0.10f * B)), (1f, Gfx.C4(col, 0f))))
                dc.FillEllipse(new Ellipse(pos, og, og), ob);

            // ---- うずまく電光の弧 ----
            for (int k = 0; k < 6; k++)
            {
                float rr = orbR * (1.05f + 0.12f * k);
                float a0 = t * (2.5f + 0.6f * k) * (k % 2 == 0 ? 1f : -1f) + k * 1.3f;
                var arc = Gfx.ArcPts(pos, rr, a0, a0 + 0.9f + 0.2f * k, 18);
                brush.Color = Gfx.C4(col, 0.35f * B);
                FormKit.Stroke(dc, brush, arc, 6f, false);
                brush.Color = Gfx.C4(hot, 0.9f * B);
                FormKit.Stroke(dc, brush, arc, 2f, false);
            }

            // ---- 球本体 ----
            using (var bb = Gfx.Radial(dc, pos, orbR,
                (0f, Gfx.C4(white, 1f * B)), (0.45f, Gfx.C4(hot, 0.85f * B)), (0.8f, Gfx.C4(col, 0.5f * B)), (1f, Gfx.C4(col, 0f))))
                dc.FillEllipse(new Ellipse(pos, orbR, orbR), bb);

            // ---- ばちばち散る火花 ----
            int tick = (int)MathF.Floor(t * 20f);
            var rs = new Random(Seed * 31 + tick);
            for (int i = 0; i < 8; i++)
            {
                float a = rs.NextF() * MathF.PI * 2f;
                var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                float r0 = orbR * (0.9f + 0.2f * rs.NextF()), r1 = r0 + orbR * (0.2f + 0.5f * rs.NextF());
                brush.Color = Gfx.C4(hot, 0.8f * B * chg);
                dc.DrawLine(pos + d * r0, pos + d * r1, brush, 1.6f);
            }

            // ---- 発射の瞬間の衝撃波 ----
            if (fire > 0f && fire < 0.25f)
            {
                float u = fire / 0.25f;
                brush.Color = Gfx.C4(hot, 0.6f * (1f - u));
                dc.DrawEllipse(new Ellipse(home, R * (1f + 2.5f * u), R * (1f + 2.5f * u)), brush, 3f * (1f - u) + 1f);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
