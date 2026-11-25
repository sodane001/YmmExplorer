using System.ComponentModel.DataAnnotations;
using System.Numerics;
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
//  太陽（朝日・夕日・光の筋・影）
//  色の目安:  朝日  コア #FFF4D6 / グロー #FFD27A
//             昼    コア #FFFFFF / グロー #FFF2B0
//             夕日  コア #FFE3B0 / グロー #FF6A2A
// =====================================================================
public sealed class SunPlugin : IShapePlugin
{
    public string Name => "演出：太陽";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SunParameter(sharedData);
}

public sealed class SunParameter : EffectShapeParameterBase
{
    public SunParameter() : this(null) { }
    public SunParameter(SharedDataStore? sharedData) : base(sharedData) { }

    // ---- 位置 ----
    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(70, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置", Description = "夕日は大きめ、朝日は小さめにすると地平線に近づく")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(25, -100, 200);

    // ---- 太陽本体 ----
    [Display(GroupName = "太陽", Name = "コアの色")]
    [ColorPicker]
    public WpfColor CoreColor { get => core; set => Set(ref core, value); }
    WpfColor core = WpfColor.FromRgb(0xFF, 0xF4, 0xD6);

    [Display(GroupName = "太陽", Name = "光（グロー）の色")]
    [ColorPicker]
    public WpfColor GlowColor { get => glow; set => Set(ref glow, value); }
    WpfColor glow = WpfColor.FromRgb(0xFF, 0xAA, 0x4D);

    [Display(GroupName = "太陽", Name = "半径")]
    [AnimationSlider("F0", "px", 10, 400)]
    public Animation Radius { get; } = new Animation(70, 1, 3000);

    [Display(GroupName = "太陽", Name = "光の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Intensity { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "太陽", Name = "グローの広がり")]
    [AnimationSlider("F0", "px", 100, 2000)]
    public Animation GlowSize { get; } = new Animation(700, 10, 10000);

    [Display(GroupName = "太陽", Name = "揺らぎ", Description = "光の明滅（朝日・夕日の陽炎のような揺れ）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Shimmer { get; } = new Animation(10, 0, 100);

    // ---- 放射状の光芒 ----
    [Display(GroupName = "光芒（放射）", Name = "本数")]
    [AnimationSlider("F0", "本", 0, 48)]
    public Animation RayCount { get; } = new Animation(14, 0, 200);

    [Display(GroupName = "光芒（放射）", Name = "長さ")]
    [AnimationSlider("F0", "px", 50, 1500)]
    public Animation RayLength { get; } = new Animation(350, 1, 10000);

    [Display(GroupName = "光芒（放射）", Name = "回転角度")]
    [AnimationSlider("F1", "°", 0, 360)]
    public Animation RayRotation { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "光芒（放射）", Name = "自動回転")]
    [AnimationSlider("F1", "°/s", -30, 30)]
    public Animation RaySpin { get; } = new Animation(2, -1000, 1000);

    [Display(GroupName = "光芒（放射）", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RayOpacity { get; } = new Animation(35, 0, 100);

    // ---- 差し込む光の筋（ゴッドレイ）----
    [Display(GroupName = "光の筋", Name = "本数")]
    [AnimationSlider("F0", "本", 0, 20)]
    public Animation ShaftCount { get; } = new Animation(7, 0, 100);

    [Display(GroupName = "光の筋", Name = "角度", Description = "0=真下、プラスで右、マイナスで左へ向かう")]
    [AnimationSlider("F1", "°", -90, 90)]
    public Animation ShaftAngle { get; } = new Animation(-40, -360, 360);

    [Display(GroupName = "光の筋", Name = "広がり角")]
    [AnimationSlider("F0", "°", 0, 120)]
    public Animation ShaftSpread { get; } = new Animation(50, 0, 360);

    [Display(GroupName = "光の筋", Name = "長さ")]
    [AnimationSlider("F0", "px", 200, 4000)]
    public Animation ShaftLength { get; } = new Animation(1800, 1, 20000);

    [Display(GroupName = "光の筋", Name = "太さ")]
    [AnimationSlider("F0", "px", 20, 500)]
    public Animation ShaftWidth { get; } = new Animation(160, 1, 5000);

    [Display(GroupName = "光の筋", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ShaftOpacity { get; } = new Animation(25, 0, 100);

    // ---- 影（ブラインド・窓枠・木漏れ日風のストライプ影）----
    [Display(GroupName = "影", Name = "影の色")]
    [ColorPicker]
    public WpfColor ShadowColor { get => shadow; set => Set(ref shadow, value); }
    WpfColor shadow = WpfColor.FromRgb(0, 0, 0);

    [Display(GroupName = "影", Name = "影の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ShadowOpacity { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "影", Name = "影の角度", Description = "ストライプの向き")]
    [AnimationSlider("F1", "°", -90, 90)]
    public Animation ShadowAngle { get; } = new Animation(20, -360, 360);

    [Display(GroupName = "影", Name = "影の幅")]
    [AnimationSlider("F0", "px", 5, 400)]
    public Animation ShadowWidth { get; } = new Animation(50, 1, 5000);

    [Display(GroupName = "影", Name = "影の間隔（周期）")]
    [AnimationSlider("F0", "px", 20, 800)]
    public Animation ShadowPeriod { get; } = new Animation(140, 5, 10000);

    [Display(GroupName = "影", Name = "ぼかし")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation ShadowSoftness { get; } = new Animation(30, 0, 1000);

    [Display(GroupName = "影", Name = "スクロール速度")]
    [AnimationSlider("F0", "px/s", -200, 200)]
    public Animation ShadowScroll { get; } = new Animation(0, -5000, 5000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
    [
        PosX, PosY, Radius, Intensity, GlowSize, Shimmer,
        RayCount, RayLength, RayRotation, RaySpin, RayOpacity,
        ShaftCount, ShaftAngle, ShaftSpread, ShaftLength, ShaftWidth, ShaftOpacity,
        ShadowOpacity, ShadowAngle, ShadowWidth, ShadowPeriod, ShadowSoftness, ShadowScroll,
    ];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var center = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float radius = c.F(Radius);

        // 光の強さ（＋揺らぎ）
        float shim = c.F(Shimmer) / 100f;
        float flick = 1f - shim * (0.5f + 0.5f * MathF.Sin(t * 2.3f) * MathF.Sin(t * 0.9f + 1.3f));
        float I = c.F(Intensity) / 100f * flick;

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 1. 影（先に敷く。光はその上に重ねる）----
        float sOp = c.F(ShadowOpacity) / 100f;
        if (sOp > 0f)
        {
            float diag = MathF.Sqrt(w * w + h * h);
            float period = MathF.Max(5f, c.F(ShadowPeriod));
            float sw = c.F(ShadowWidth), soft = c.F(ShadowSoftness);
            float scroll = Gfx.Mod(t * c.F(ShadowScroll), period);
            var mid = new Vector2(w / 2f, h / 2f);

            dc.Transform = Matrix3x2.CreateRotation(Gfx.Rad(c.F(ShadowAngle)), mid);
            for (int k = 0; k < 3; k++)   // 3層重ねてエッジをぼかす
            {
                float ex = soft * (2 - k) * 0.5f;
                brush.Color = Gfx.C4(ShadowColor, sOp * 0.34f);
                for (float x = mid.X - diag / 2f - period + scroll; x < mid.X + diag / 2f + period; x += period)
                    Gfx.Rect(dc, brush, x - ex, mid.Y - diag / 2f, sw + 2f * ex, diag);
            }
            dc.Transform = Matrix3x2.Identity;
        }

        // ---- 2. グロー ----
        float gs = c.F(GlowSize);
        using (var g = Gfx.Radial(dc, center, gs,
            (0f, Gfx.C4(GlowColor, 0.9f * I)),
            (0.18f, Gfx.C4(GlowColor, 0.55f * I)),
            (0.5f, Gfx.C4(GlowColor, 0.18f * I)),
            (1f, Gfx.C4(GlowColor, 0f))))
        {
            dc.FillEllipse(new Ellipse(center, gs, gs), g);
        }
        float gs2 = gs * 0.35f;
        using (var g2 = Gfx.Radial(dc, center, gs2,
            (0f, Gfx.C4(CoreColor, 0.9f * I)),
            (1f, Gfx.C4(CoreColor, 0f))))
        {
            dc.FillEllipse(new Ellipse(center, gs2, gs2), g2);
        }

        // ---- 3. 差し込む光の筋 ----
        int shn = (int)c.F(ShaftCount);
        if (shn > 0)
        {
            float baseAng = c.F(ShaftAngle), spread = c.F(ShaftSpread);
            float len = c.F(ShaftLength), wd = c.F(ShaftWidth), so = c.F(ShaftOpacity) / 100f;
            var rng = new Random(Seed);
            for (int j = 0; j < shn; j++)
            {
                float u = shn == 1 ? 0f : j / (float)(shn - 1) - 0.5f;
                float a = baseAng + u * spread + (rng.NextF() - 0.5f) * 6f;
                float wj = wd * (0.5f + rng.NextF());
                float ph = rng.NextF() * MathF.PI * 2f;
                float pulse = 0.6f + 0.4f * MathF.Sin(t * 0.7f + ph);
                var end = center + Gfx.Dir(a) * len;
                Gfx.FadeLine(dc, brush, center, end, wj * 0.3f, wj, CoreColor, so * pulse * I, 14, 1.6f);
            }
        }

        // ---- 4. 放射状の光芒 ----
        int rn = (int)c.F(RayCount);
        if (rn > 0)
        {
            float rl = c.F(RayLength), ro = c.F(RayOpacity) / 100f;
            float rot = c.F(RayRotation) + t * c.F(RaySpin);
            for (int r = 0; r < rn; r++)
            {
                float a = rot + r * 360f / rn;
                var dir = new Vector2(MathF.Cos(Gfx.Rad(a)), MathF.Sin(Gfx.Rad(a)));
                float lenR = rl * (0.6f + 0.4f * MathF.Sin(r * 12.9898f));  // 本ごとに長さを変える
                var p0 = center + dir * (radius * 0.9f);
                var p1 = center + dir * (radius + MathF.Abs(lenR));
                Gfx.FadeLine(dc, brush, p0, p1, radius * 0.18f, 1f, CoreColor, ro * I, 10, 1.4f);
            }
        }

        // ---- 5. 太陽の核 ----
        float cr = radius * 1.15f;
        using (var core = Gfx.Radial(dc, center, cr,
            (0f, Gfx.C4(CoreColor, 1f)),
            (0.82f, Gfx.C4(CoreColor, 1f)),
            (1f, Gfx.C4(CoreColor, 0f))))
        {
            dc.FillEllipse(new Ellipse(center, cr, cr), core);
        }
    }
}
