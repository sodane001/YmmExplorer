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
//  溶岩
//   黒い地殻の割れ目から赤熱した溶岩がのぞき、泡が弾け、火の粉が舞う
// =====================================================================
public sealed class LavaPlugin : IShapePlugin
{
    public string Name => "演出：溶岩";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LavaParameter(sharedData);
}

public sealed class LavaParameter : EffectShapeParameterBase
{
    public LavaParameter() : this(null) { }
    public LavaParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "高温の色")]
    [ColorPicker]
    public WpfColor HotColor { get => hot; set => Set(ref hot, value); }
    WpfColor hot = WpfColor.FromRgb(0xFF, 0xE0, 0x4A);

    [Display(GroupName = "色", Name = "溶岩の色")]
    [ColorPicker]
    public WpfColor MidColor { get => mid; set => Set(ref mid, value); }
    WpfColor mid = WpfColor.FromRgb(0xFF, 0x5A, 0x0A);

    [Display(GroupName = "色", Name = "地殻の色")]
    [ColorPicker]
    public WpfColor CrustColor { get => crust; set => Set(ref crust, value); }
    WpfColor crust = WpfColor.FromRgb(0x1A, 0x08, 0x05);

    [Display(GroupName = "溶岩", Name = "地殻の量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CrustAmount { get; } = new Animation(65, 0, 100);

    [Display(GroupName = "溶岩", Name = "地殻の塊の大きさ")]
    [AnimationSlider("F0", "px", 60, 500)]
    public Animation PlateSize { get; } = new Animation(220, 10, 5000);

    [Display(GroupName = "溶岩", Name = "流れる速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Flow { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "溶岩", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "要素", Name = "泡の数")]
    [AnimationSlider("F0", "個", 0, 60)]
    public Animation Bubbles { get; } = new Animation(14, 0, 500);

    [Display(GroupName = "要素", Name = "火の粉の数")]
    [AnimationSlider("F0", "個", 0, 200)]
    public Animation Embers { get; } = new Animation(50, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [CrustAmount, PlateSize, Flow, Brightness, Bubbles, Embers];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float ts = c.Time * c.F(Flow) / 100f, t = c.Time;
        float bright = c.F(Brightness) / 100f * (0.92f + 0.08f * MathF.Sin(t * 5f));
        float crustK = c.F(CrustAmount) / 100f, ps = c.F(PlateSize);
        var black = WpfColor.FromRgb(0, 0, 0);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 赤熱した溶岩の層 ----
        using (var g = Gfx.Linear(dc, Vector2.Zero, new Vector2(w * 0.3f, h),
            (0f, Gfx.C4(Gfx.Mix(MidColor, HotColor, 0.15f), bright)), (1f, Gfx.C4(Gfx.Mix(MidColor, black, 0.5f), bright))))
            Gfx.Rect(dc, g, 0, 0, w, h);

        // ---- 流れる高温部 ----
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            var rh = new Random(Seed + 4);
            for (int i = 0; i < 26; i++)
            {
                float bx = rh.NextF() * w, by = rh.NextF() * h, ph = rh.NextF() * MathF.PI * 2f, f = 0.1f + 0.3f * rh.NextF();
                float r = ps * (0.6f + 0.9f * rh.NextF());
                var p = new Vector2(bx + MathF.Sin(ts * f + ph) * 120f, by + MathF.Cos(ts * f * 0.8f + ph) * 70f);
                float a = (0.12f + 0.2f * rh.NextF()) * bright;
                using var hb = Gfx.Radial(dc, p, r, (0f, Gfx.C4(HotColor, a)), (0.6f, Gfx.C4(MidColor, a * 0.4f)), (1f, Gfx.C4(MidColor, 0f)));
                dc.FillEllipse(new Ellipse(p, r, r), hb);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }

        // ---- 地殻（縁がぼけているので、すき間から溶岩が光って見える）----
        int plates = (int)(crustK * 34f);
        var rc = new Random(Seed + 8);
        float span = w + ps * 3f;
        for (int i = 0; i < plates; i++)
        {
            float x0 = rc.NextF() * span, y = rc.NextF() * h, sp = 6f + 10f * rc.NextF(), size = ps * (0.6f + 0.8f * rc.NextF());
            float aspect = 0.5f + 0.6f * rc.NextF(), ph = rc.NextF() * MathF.PI * 2f;
            float x = Gfx.Mod(x0 + ts * sp, span) - ps * 1.5f;
            var ctr = new Vector2(x, y + MathF.Sin(ts * 0.2f + ph) * 15f);
            for (int k = 0; k < 3; k++)
            {
                var p = ctr + new Vector2((k - 1) * size * 0.45f, ((k * 37 + i) % 3 - 1) * size * 0.18f);
                float r = size * (0.55f + 0.2f * k % 3);
                try
                {
                    dc.Transform = Matrix3x2.CreateScale(1f, aspect, p);
                    using var cb = Gfx.Radial(dc, p, r, (0f, Gfx.C4(CrustColor, 0.97f)), (0.65f, Gfx.C4(CrustColor, 0.92f)), (1f, Gfx.C4(CrustColor, 0f)));
                    dc.FillEllipse(new Ellipse(p, r, r), cb);
                }
                finally
                {
                    dc.Transform = Matrix3x2.Identity;
                }
            }
        }

        // ---- 泡（膨らんで弾ける）----
        int nb = (int)c.F(Bubbles);
        var rb = new Random(Seed + 12);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            for (int i = 0; i < nb; i++)
            {
                float bx = rb.NextF() * w, by = rb.NextF() * h, period = 1.6f + 2.4f * rb.NextF(), ph = rb.NextF();
                float R = 14f + 34f * rb.NextF();
                float u = Gfx.Mod(t / period + ph, 1f);
                var p = new Vector2(bx, by);
                float r = R * MathF.Min(1f, u * 1.2f);
                if (u < 0.8f)
                {
                    brush.Color = Gfx.C4(HotColor, 0.25f * bright);
                    dc.FillEllipse(new Ellipse(p, r, r * 0.7f), brush);
                    brush.Color = Gfx.C4(HotColor, 0.7f * bright);
                    dc.DrawEllipse(new Ellipse(p, r, r * 0.7f), brush, 2f);
                }
                else
                {
                    float pu = (u - 0.8f) / 0.2f;                          // 弾けて広がる
                    brush.Color = Gfx.C4(HotColor, 0.7f * (1f - pu) * bright);
                    dc.DrawEllipse(new Ellipse(p, R * (1f + 0.8f * pu), R * 0.7f * (1f + 0.8f * pu)), brush, 2f * (1f - pu) + 0.5f);
                    for (int k = 0; k < 6; k++)
                    {
                        float a = k * MathF.PI / 3f + i;
                        brush.Color = Gfx.C4(HotColor, 0.8f * (1f - pu) * bright);
                        dc.FillEllipse(new Ellipse(p + new Vector2(MathF.Cos(a), MathF.Sin(a) * 0.6f) * R * (0.8f + 1.3f * pu) - new Vector2(0f, 20f * pu), 2f, 2f), brush);
                    }
                }
            }

            // ---- 火の粉 ----
            int ne = (int)c.F(Embers);
            var re = new Random(Seed + 16);
            for (int i = 0; i < ne; i++)
            {
                float x0 = re.NextF() * w, ph = re.NextF(), sp = 0.1f + 0.2f * re.NextF(), r = 1f + 2f * re.NextF();
                float u = Gfx.Mod(t * sp + ph, 1f);
                var p = new Vector2(x0 + MathF.Sin(t + ph * 9f) * 30f * u, h * 0.9f - u * h * 0.9f);
                brush.Color = Gfx.C4(Gfx.Mix(HotColor, MidColor, u), 0.9f * (1f - u) * bright * (0.5f + 0.5f * MathF.Sin(t * 20f + ph * 30f)));
                dc.FillEllipse(new Ellipse(p, r, r), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
