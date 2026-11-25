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
//  ホラー演出
//   人魂、滴る血、揺れる暗闇、赤いにじみ
// =====================================================================
public sealed class HorrorPlugin : IShapePlugin
{
    public string Name => "演出：ホラー演出";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HorrorParameter(sharedData);
}

public sealed class HorrorParameter : EffectShapeParameterBase
{
    public HorrorParameter() : this(null) { }
    public HorrorParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "人魂", Name = "人魂を出す")]
    [ToggleSlider]
    public bool Wisps { get => wisps; set => Set(ref wisps, value); }
    bool wisps = true;

    [Display(GroupName = "人魂", Name = "人魂の色")]
    [ColorPicker]
    public WpfColor WispColor { get => wispColor; set => Set(ref wispColor, value); }
    WpfColor wispColor = WpfColor.FromRgb(0x6A, 0xC8, 0xFF);

    [Display(GroupName = "人魂", Name = "人魂の数")]
    [AnimationSlider("F0", "個", 1, 20)]
    public Animation WispCount { get; } = new Animation(4, 0, 200);

    [Display(GroupName = "人魂", Name = "人魂の大きさ")]
    [AnimationSlider("F0", "px", 10, 150)]
    public Animation WispSize { get; } = new Animation(40, 2, 2000);

    [Display(GroupName = "血", Name = "滴る血を出す")]
    [ToggleSlider]
    public bool Blood { get => blood; set => Set(ref blood, value); }
    bool blood = true;

    [Display(GroupName = "血", Name = "血の色")]
    [ColorPicker]
    public WpfColor BloodColor { get => bloodColor; set => Set(ref bloodColor, value); }
    WpfColor bloodColor = WpfColor.FromRgb(0x7A, 0x00, 0x0C);

    [Display(GroupName = "血", Name = "滴る本数")]
    [AnimationSlider("F0", "本", 0, 50)]
    public Animation DripCount { get; } = new Animation(14, 0, 500);

    [Display(GroupName = "血", Name = "垂れる長さ")]
    [AnimationSlider("F0", "%", 5, 100)]
    public Animation DripLength { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "暗闇", Name = "揺れる暗闇を出す")]
    [ToggleSlider]
    public bool Darkness { get => darkness; set => Set(ref darkness, value); }
    bool darkness = true;

    [Display(GroupName = "暗闇", Name = "暗さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation DarkAmount { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "暗闇", Name = "ちらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(50, 0, 100);

    [Display(GroupName = "赤いにじみ", Name = "赤いにじみを出す")]
    [ToggleSlider]
    public bool RedBleed { get => redBleed; set => Set(ref redBleed, value); }
    bool redBleed = true;

    [Display(GroupName = "赤いにじみ", Name = "にじみの色")]
    [ColorPicker]
    public WpfColor BleedColor { get => bleedColor; set => Set(ref bleedColor, value); }
    WpfColor bleedColor = WpfColor.FromRgb(0xC0, 0x00, 0x18);

    [Display(GroupName = "赤いにじみ", Name = "鼓動の速さ（BPM）")]
    [AnimationSlider("F0", "", 30, 180)]
    public Animation HeartRate { get; } = new Animation(72, 1, 600);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [WispCount, WispSize, DripCount, DripLength, DarkAmount, Flicker, HeartRate];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(w / 2f, h / 2f);
        float diag = MathF.Sqrt(w * w + h * h) / 2f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // ---- 揺れる暗闇 ----
            if (Darkness)
            {
                float amt = c.F(DarkAmount) / 100f, fl = c.F(Flicker) / 100f;
                float n = 0.5f + 0.5f * MathF.Sin(t * 7.3f) * MathF.Sin(t * 2.9f + 1f);
                float wob = 1f + fl * 0.25f * (n - 0.5f);
                using (var g = Gfx.Radial(dc, ctr + new Vector2(MathF.Sin(t * 0.7f) * 30f * fl, MathF.Cos(t * 0.5f) * 20f * fl), diag * wob,
                    (0f, Gfx.C4(0f, 0f, 0f, 0f)), (0.35f, Gfx.C4(0f, 0f, 0f, 0.15f * amt)), (0.75f, Gfx.C4(0f, 0f, 0f, 0.75f * amt)), (1f, Gfx.C4(0f, 0f, 0f, amt))))
                    dc.FillRectangle(new RawRectF(0, 0, w, h), g);
                // ときどき一瞬暗くなる
                float blink = Gfx.Hash01((int)MathF.Floor(t * 8f) + Seed * 31);
                if (blink > 1f - 0.08f * fl - 0.001f && fl > 0f)
                {
                    brush.Color = Gfx.C4(0f, 0f, 0f, 0.55f * amt);
                    dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
                }
            }

            // ---- 赤いにじみ（鼓動に合わせて）----
            if (RedBleed)
            {
                float beat = DrawKit.Frac(t * c.F(HeartRate) / 60f);
                float env = MathF.Exp(-beat * 9f) + 0.6f * MathF.Exp(-MathF.Max(0f, beat - 0.18f) * 10f) * (beat > 0.18f ? 1f : 0f);
                float a = 0.25f + 0.35f * MathF.Min(1f, env);
                using var g = Gfx.Radial(dc, ctr, diag,
                    (0f, Gfx.C4(BleedColor, 0f)), (0.55f, Gfx.C4(BleedColor, 0f)), (0.85f, Gfx.C4(BleedColor, a * 0.5f)), (1f, Gfx.C4(BleedColor, a)));
                dc.FillRectangle(new RawRectF(0, 0, w, h), g);
            }

            // ---- 滴る血 ----
            if (Blood)
            {
                int n = (int)c.F(DripCount);
                float maxLen = h * c.F(DripLength) / 100f;
                var dark = Gfx.Mix(BloodColor, WpfColor.FromRgb(0, 0, 0), 0.35f);
                var shine = Gfx.Mix(BloodColor, white, 0.35f);
                // 上のふちにたまった血
                var top = new List<Vector2> { new(-10f, -10f), new(w + 10f, -10f) };
                for (int k = 40; k >= 0; k--)
                {
                    float x = w * k / 40f;
                    top.Add(new Vector2(x, 14f + 18f * DrawKit.H(Seed, k, 50) + 6f * MathF.Sin(x * 0.02f + t * 0.5f)));
                }
                brush.Color = Gfx.C4(BloodColor, 0.95f);
                Gfx.FillPoly(dc, brush, top);
                for (int i = 0; i < n; i++)
                {
                    float x = (i + 0.3f + 0.4f * DrawKit.H(Seed, i, 51)) / Math.Max(1, n) * w;
                    float speed = 25f + 50f * DrawKit.H(Seed, i, 52);
                    float len = maxLen * (0.3f + 0.7f * DrawKit.H(Seed, i, 53));
                    float period = len / speed + 2.5f + 2f * DrawKit.H(Seed, i, 54);
                    float lt = Gfx.Mod(t + DrawKit.H(Seed, i, 55) * period, period);
                    float grow = MathF.Min(len, lt * speed * (1f - 0.4f * MathF.Min(1f, lt * speed / len)));
                    float wd = 5f + 7f * DrawKit.H(Seed, i, 56);
                    float y0 = 20f;
                    // 垂れている筋
                    var strip = new List<Vector2> { new(x - wd / 2f, y0 - 4f), new(x + wd / 2f, y0 - 4f) };
                    for (int k = 1; k <= 8; k++)
                    {
                        float u = k / 8f;
                        strip.Add(new Vector2(x + wd / 2f * (1f - 0.35f * u) + MathF.Sin(u * 5f + i) * 1.5f, y0 + grow * u));
                    }
                    for (int k = 8; k >= 1; k--)
                    {
                        float u = k / 8f;
                        strip.Add(new Vector2(x - wd / 2f * (1f - 0.35f * u) + MathF.Sin(u * 5f + i) * 1.5f, y0 + grow * u));
                    }
                    brush.Color = Gfx.C4(BloodColor, 0.96f);
                    Gfx.FillPoly(dc, brush, strip);
                    // 先のしずく
                    var tip = new Vector2(x, y0 + grow);
                    float dr = wd * 0.75f;
                    brush.Color = Gfx.C4(BloodColor, 0.96f);
                    dc.FillEllipse(new Ellipse(tip, dr, dr * 1.15f), brush);
                    brush.Color = Gfx.C4(shine, 0.5f);
                    dc.FillEllipse(new Ellipse(tip + new Vector2(-dr * 0.3f, -dr * 0.3f), dr * 0.22f, dr * 0.3f), brush);
                    brush.Color = Gfx.C4(dark, 0.6f);
                    dc.DrawLine(new Vector2(x + wd * 0.25f, y0), new Vector2(x + wd * 0.18f, y0 + grow * 0.95f), brush, 1.2f);
                    // 落ちるしずく
                    float fallT = lt - len / speed * 1.4f;
                    if (fallT > 0f)
                    {
                        float fy = y0 + len + 0.5f * 1600f * fallT * fallT;
                        if (fy < h + 30f)
                        {
                            brush.Color = Gfx.C4(BloodColor, 0.95f);
                            dc.FillEllipse(new Ellipse(new Vector2(x, fy), dr * 0.6f, dr * 0.9f), brush);
                        }
                    }
                }
            }

            // ---- 人魂 ----
            if (Wisps)
            {
                int n = (int)c.F(WispCount);
                float S = c.F(WispSize);
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                for (int i = 0; i < n; i++)
                {
                    var basePos = new Vector2(w * (0.15f + 0.7f * DrawKit.H(Seed, i, 60)), h * (0.3f + 0.45f * DrawKit.H(Seed, i, 61)));
                    var p = FormKit.Liss(basePos, w * 0.12f, h * 0.08f, t, 0.13f + 0.1f * DrawKit.H(Seed, i, 62), 0.21f + 0.1f * DrawKit.H(Seed, i, 63), i * 1.7f, i * 2.3f);
                    var pn = FormKit.Liss(basePos, w * 0.12f, h * 0.08f, t + 0.1f, 0.13f + 0.1f * DrawKit.H(Seed, i, 62), 0.21f + 0.1f * DrawKit.H(Seed, i, 63), i * 1.7f, i * 2.3f);
                    var mv = pn - p;
                    // 尾は動きと反対側、少し上へなびく
                    var tailDir = Vector2.Normalize(-(mv.LengthSquared() > 1e-6f ? Vector2.Normalize(mv) : Vector2.Zero) + new Vector2(0, -0.8f));
                    float flick = 0.8f + 0.2f * MathF.Sin(t * 11f + i * 3f);
                    float r = S * (0.4f + 0.2f * DrawKit.H(Seed, i, 64)) * flick;
                    var nrm = new Vector2(-tailDir.Y, tailDir.X);
                    var shape = new List<Vector2>();
                    for (int k = 0; k <= 12; k++)
                    {
                        float th = MathF.PI * k / 12f;
                        shape.Add(p + (nrm * MathF.Cos(th) - tailDir * MathF.Sin(th)) * r);
                    }
                    var tip = p + tailDir * r * 3.4f + nrm * MathF.Sin(t * 6f + i) * r * 0.6f;
                    shape.Add(Vector2.Lerp(p - nrm * r, tip, 0.5f) + nrm * r * 0.15f);
                    shape.Add(tip);
                    shape.Add(Vector2.Lerp(p + nrm * r, tip, 0.5f) - nrm * r * 0.15f);
                    DrawKit.Glow(dc, p, r * 4f, Gfx.C4(WispColor, 0.3f * flick));
                    brush.Color = Gfx.C4(WispColor, 0.45f);
                    Gfx.FillPoly(dc, brush, shape);
                    DrawKit.Glow(dc, p, r * 1.2f, Gfx.C4(Gfx.Mix(WispColor, white, 0.7f), 0.9f));
                }
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
