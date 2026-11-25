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
//  水面（見下げ）
//   水底のコースティクス / うねりの明暗 / 屈折感のある波紋 / 反射のきらめき
// =====================================================================
public sealed class WaterSurfaceDownPlugin : IShapePlugin
{
    public string Name => "演出：水面（見下げ）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WaterSurfaceDownParameter(sharedData);
}

public sealed class WaterSurfaceDownParameter : EffectShapeParameterBase
{
    public WaterSurfaceDownParameter() : this(null) { }
    public WaterSurfaceDownParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "水", Name = "浅い側の色")]
    [ColorPicker]
    public WpfColor ShallowColor { get => shallow; set => Set(ref shallow, value); }
    WpfColor shallow = WpfColor.FromRgb(0x4F, 0xC3, 0xD0);

    [Display(GroupName = "水", Name = "深い側の色")]
    [ColorPicker]
    public WpfColor DeepColor { get => deep; set => Set(ref deep, value); }
    WpfColor deep = WpfColor.FromRgb(0x07, 0x35, 0x58);

    [Display(GroupName = "水", Name = "光の色")]
    [ColorPicker]
    public WpfColor HighlightColor { get => hl; set => Set(ref hl, value); }
    WpfColor hl = WpfColor.FromRgb(0xE8, 0xFF, 0xFF);

    [Display(GroupName = "水", Name = "深さ（周辺の暗さ）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Depth { get; } = new Animation(50, 0, 100);

    [Display(GroupName = "水", Name = "流れの速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Flow { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "水", Name = "うねりの強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation WaveStrength { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "コースティクス", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation CausticStrength { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "コースティクス", Name = "模様の大きさ")]
    [AnimationSlider("F0", "px", 40, 400)]
    public Animation CausticSize { get; } = new Animation(120, 20, 5000);

    [Display(GroupName = "コースティクス", Name = "きめ細かさ", Description = "小さいほど滑らかですが重くなります")]
    [AnimationSlider("F0", "px", 8, 40)]
    public Animation CellSize { get; } = new Animation(14, 6, 200);

    [Display(GroupName = "波紋", Name = "波紋の数")]
    [AnimationSlider("F0", "個", 0, 30)]
    public Animation RippleCount { get; } = new Animation(6, 0, 200);

    [Display(GroupName = "波紋", Name = "広がる速度")]
    [AnimationSlider("F0", "px/s", 10, 400)]
    public Animation RippleSpeed { get; } = new Animation(70, 1, 5000);

    [Display(GroupName = "波紋", Name = "最大半径")]
    [AnimationSlider("F0", "px", 50, 800)]
    public Animation RippleRadius { get; } = new Animation(260, 10, 5000);

    [Display(GroupName = "きらめき", Name = "数")]
    [AnimationSlider("F0", "個", 0, 300)]
    public Animation SparkleCount { get; } = new Animation(70, 0, 5000);

    [Display(GroupName = "水", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Depth, Flow, WaveStrength, CausticStrength, CausticSize, CellSize,
         RippleCount, RippleSpeed, RippleRadius, SparkleCount, Opacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float t = c.Time * c.F(Flow) / 100f;
        float op = c.F(Opacity) / 100f;
        float ws = c.F(WaveStrength) / 100f;
        var dark = Gfx.Mix(DeepColor, WpfColor.FromRgb(0, 8, 20), 0.6f);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 1. 水の色（浅い→深い）+ 周辺を暗く ----
        using (var g = Gfx.Linear(dc, Vector2.Zero, new Vector2(w, h),
            (0f, Gfx.C4(ShallowColor, op)), (1f, Gfx.C4(DeepColor, op))))
            Gfx.Rect(dc, g, 0, 0, w, h);

        float dep = c.F(Depth) / 100f;
        float rad = MathF.Sqrt(w * w + h * h) / 2f;
        using (var v = Gfx.Radial(dc, new Vector2(w / 2f, h / 2f), rad,
            (0f, Gfx.C4(dark, 0f)), (0.5f, Gfx.C4(dark, 0f)), (1f, Gfx.C4(dark, 0.75f * dep * op))))
            Gfx.Rect(dc, v, 0, 0, w, h);

        // ---- 2. うねりによる大きな明暗 ----
        float big = 64f;
        for (float y = big / 2f; y < h + big; y += big)
            for (float x = big / 2f; x < w + big; x += big)
            {
                float s = Gfx.Shade(x, y, t);
                float a = MathF.Abs(s) * 0.12f * ws * op;
                if (a < 0.005f) continue;
                brush.Color = Gfx.C4(s > 0 ? HighlightColor : dark, a);
                dc.FillEllipse(new Ellipse(new Vector2(x, y), big * 0.85f, big * 0.85f), brush);
            }

        // ---- 3. コースティクス（水底の光の網目）----
        float cell = MathF.Max(6f, c.F(CellSize));
        float scale = 1.4f / MathF.Max(20f, c.F(CausticSize));
        float cstr = c.F(CausticStrength) / 100f * op;
        float cr = cell * 0.8f;
        if (cstr > 0f)
        {
            for (float y = cell / 2f; y < h + cell; y += cell)
                for (float x = cell / 2f; x < w + cell; x += cell)
                {
                    float I = Gfx.Caustic(x, y, t, scale);
                    if (I < 0.04f) continue;
                    float shallowBias = 1f - 0.5f * ((x / w + y / h) * 0.5f);   // 浅い側ほど明るい
                    brush.Color = Gfx.C4(HighlightColor, I * 0.55f * cstr * shallowBias);
                    dc.FillEllipse(new Ellipse(new Vector2(x, y), cr, cr), brush);
                }
        }

        // ---- 4. 波紋（暗い縁と明るい縁の2本線で屈折感を出す）----
        int rc = (int)c.F(RippleCount);
        float R = c.F(RippleRadius), sp = c.F(RippleSpeed);
        float period = R / sp;
        for (int k = 0; k < rc; k++)
        {
            float lt = c.Time + (k / (float)rc) * period;
            int id = (int)MathF.Floor(lt / period);
            float u = Gfx.Mod(lt, period) / period;
            var rr = new Random(Seed * 100003 + k * 1009 + id * 31);
            var ctr = new Vector2(rr.NextF() * w, rr.NextF() * h);
            float radius = u * R;
            float env = (1f - u) * (1f - u) * MathF.Min(1f, u * 10f);
            float gap = R * 0.05f;
            for (int j = 0; j < 4; j++)
            {
                float r0 = radius - j * gap;
                if (r0 <= 2f) continue;
                float amp = env * (1f - j * 0.22f) * op;
                brush.Color = Gfx.C4(dark, amp * 0.35f);
                dc.DrawEllipse(new Ellipse(ctr, r0 - 1.6f, r0 - 1.6f), brush, 2.4f);
                brush.Color = Gfx.C4(HighlightColor, amp * 0.65f);
                dc.DrawEllipse(new Ellipse(ctr, r0 + 1.4f, r0 + 1.4f), brush, 1.8f);
            }
        }

        // ---- 5. 水面の反射のきらめき（細長い光）----
        int sc = (int)c.F(SparkleCount);
        var rs = new Random(Seed + 5);
        for (int i = 0; i < sc; i++)
        {
            float x = rs.NextF() * w, y = rs.NextF() * h;
            float fr = 0.8f + rs.NextF() * 2.2f, ph = rs.NextF() * MathF.PI * 2f;
            float len = 6f + rs.NextF() * 16f, th = 1f + rs.NextF() * 1.5f;
            float tw = 0.5f + 0.5f * MathF.Sin(c.Time * fr + ph);
            float a = MathF.Pow(tw, 8f) * op;
            if (a < 0.02f) continue;
            var ctr = new Vector2(x, y);
            brush.Color = Gfx.C4(HighlightColor, a * 0.25f);
            dc.FillEllipse(new Ellipse(ctr, len * 2.2f, th * 3f), brush);
            brush.Color = Gfx.C4(HighlightColor, a * 0.95f);
            dc.FillEllipse(new Ellipse(ctr, len, th), brush);
        }
    }
}
