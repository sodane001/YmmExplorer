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
//  水面（見上げ）
//   スネルの窓 / 揺らぐ縁 / 水面の光の網目 / 光芒 / 浮遊粒子 / 陰影つきの泡
// =====================================================================
public sealed class WaterSurfaceUpPlugin : IShapePlugin
{
    public string Name => "演出：水面（見上げ）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WaterSurfaceUpParameter(sharedData);
}

public sealed class WaterSurfaceUpParameter : EffectShapeParameterBase
{
    public WaterSurfaceUpParameter() : this(null) { }
    public WaterSurfaceUpParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "水", Name = "水面側（明）の色")]
    [ColorPicker]
    public WpfColor LightColor { get => light; set => Set(ref light, value); }
    WpfColor light = WpfColor.FromRgb(0xBF, 0xF3, 0xFF);

    [Display(GroupName = "水", Name = "深い側（暗）の色")]
    [ColorPicker]
    public WpfColor DeepColor { get => deep; set => Set(ref deep, value); }
    WpfColor deep = WpfColor.FromRgb(0x02, 0x1E, 0x38);

    [Display(GroupName = "水", Name = "窓の大きさ", Description = "水面の外が見える明るい円（スネルの窓）の大きさ")]
    [AnimationSlider("F0", "%", 20, 150)]
    public Animation WindowSize { get; } = new Animation(80, 5, 400);

    [Display(GroupName = "水", Name = "流れの速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Flow { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "水面の模様", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation PatternStrength { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "水面の模様", Name = "模様の大きさ")]
    [AnimationSlider("F0", "px", 40, 400)]
    public Animation PatternSize { get; } = new Animation(140, 20, 5000);

    [Display(GroupName = "水面の模様", Name = "きめ細かさ", Description = "小さいほど滑らかですが重くなります")]
    [AnimationSlider("F0", "px", 8, 40)]
    public Animation CellSize { get; } = new Animation(16, 6, 200);

    [Display(GroupName = "光源", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SunX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "光源", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SunY { get; } = new Animation(40, -100, 200);

    [Display(GroupName = "光源", Name = "まぶしさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glare { get; } = new Animation(70, 0, 500);

    [Display(GroupName = "光の筋", Name = "本数")]
    [AnimationSlider("F0", "本", 0, 40)]
    public Animation RayCount { get; } = new Animation(10, 0, 200);

    [Display(GroupName = "光の筋", Name = "角度", Description = "0=真下、プラスで右へ傾く")]
    [AnimationSlider("F1", "°", -45, 45)]
    public Animation RayAngle { get; } = new Animation(8, -89, 89);

    [Display(GroupName = "光の筋", Name = "太さ")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation RayWidth { get; } = new Animation(110, 5, 3000);

    [Display(GroupName = "光の筋", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RayStrength { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "浮遊物", Name = "粒子の数")]
    [AnimationSlider("F0", "個", 0, 500)]
    public Animation ParticleCount { get; } = new Animation(90, 0, 5000);

    [Display(GroupName = "浮遊物", Name = "泡の数")]
    [AnimationSlider("F0", "個", 0, 200)]
    public Animation BubbleCount { get; } = new Animation(18, 0, 2000);

    [Display(GroupName = "水", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [WindowSize, Flow, PatternStrength, PatternSize, CellSize, SunX, SunY, Glare,
         RayCount, RayAngle, RayWidth, RayStrength, ParticleCount, BubbleCount, Opacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float t = c.Time * c.F(Flow) / 100f;
        float op = c.F(Opacity) / 100f;
        float cx = w / 2f, cy = h / 2f;
        float R = c.F(WindowSize) / 100f * MathF.Min(w, h) * 0.5f;
        var white = WpfColor.FromRgb(255, 255, 255);
        var sun = new Vector2(c.F(SunX) / 100f * w, c.F(SunY) / 100f * h);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 1. 水の色（窓の中心が明るく、外側ほど深い青）----
        using (var g = Gfx.Radial(dc, new Vector2(cx, cy), R * 1.7f,
            (0f, Gfx.C4(LightColor, op)),
            (0.45f, Gfx.C4(Gfx.Mix(LightColor, DeepColor, 0.3f), op)),
            (0.72f, Gfx.C4(Gfx.Mix(LightColor, DeepColor, 0.72f), op)),
            (1f, Gfx.C4(DeepColor, op))))
            Gfx.Rect(dc, g, 0, 0, w, h);

        // ---- 2. 水面（裏側）の光の網目：窓の内側ほど強く ----
        float cell = MathF.Max(6f, c.F(CellSize));
        float scale = 1.4f / MathF.Max(20f, c.F(PatternSize));
        float pstr = c.F(PatternStrength) / 100f * op;
        float cr = cell * 0.8f;
        if (pstr > 0f)
        {
            for (float y = cell / 2f; y < h + cell; y += cell)
                for (float x = cell / 2f; x < w + cell; x += cell)
                {
                    float d = MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / R;
                    float m = 1f - Gfx.Smooth(0.55f, 1.3f, d);
                    if (m < 0.03f) continue;
                    float I = Gfx.Caustic(x, y, t, scale);
                    if (I < 0.04f) continue;
                    brush.Color = Gfx.C4(white, I * 0.6f * m * pstr);
                    dc.FillEllipse(new Ellipse(new Vector2(x, y), cr, cr), brush);
                }
        }

        // ---- 3. 窓の縁（ゆらゆら揺れる明るい輪）----
        var ring = new List<Vector2>();
        const int N = 120;
        for (int k = 0; k <= N; k++)
        {
            float a = k / (float)N * MathF.PI * 2f;
            float r = R * (1f + 0.025f * MathF.Sin(a * 5f + t * 0.8f) + 0.015f * MathF.Sin(a * 11f - t * 1.1f));
            ring.Add(new Vector2(cx + MathF.Cos(a) * r, cy + MathF.Sin(a) * r));
        }
        brush.Color = Gfx.C4(LightColor, 0.10f * op);
        Gfx.Polyline(dc, brush, ring, R * 0.10f);
        brush.Color = Gfx.C4(LightColor, 0.22f * op);
        Gfx.Polyline(dc, brush, ring, R * 0.035f);
        brush.Color = Gfx.C4(white, 0.45f * op);
        Gfx.Polyline(dc, brush, ring, 3f);

        // ---- 4. 太陽のまぶしさ ----
        float glare = c.F(Glare) / 100f;
        if (glare > 0f)
        {
            float flick = 0.85f + 0.15f * MathF.Sin(t * 1.7f) * MathF.Sin(t * 0.6f + 1f);
            float gr = R * 0.75f;
            using var gb = Gfx.Radial(dc, sun, gr,
                (0f, Gfx.C4(white, 0.75f * glare * flick * op)),
                (0.3f, Gfx.C4(LightColor, 0.30f * glare * flick * op)),
                (1f, Gfx.C4(LightColor, 0f)));
            dc.FillEllipse(new Ellipse(sun, gr, gr), gb);
        }

        // ---- 5. 光の筋（光源付近から差し込む）----
        int rn = (int)c.F(RayCount);
        float ang = c.F(RayAngle), rw = c.F(RayWidth), rstr = c.F(RayStrength) / 100f * op;
        var rng = new Random(Seed);
        for (int i = 0; i < rn; i++)
        {
            float sx = sun.X + (rng.NextF() - 0.5f) * R * 1.6f;
            float a = ang + (rng.NextF() - 0.5f) * 10f;
            float wd = rw * (0.4f + rng.NextF() * 1.2f);
            float fr = 0.3f + rng.NextF() * 0.5f, ph = rng.NextF() * MathF.PI * 2f;
            float amp = 0.35f + 0.65f * (0.5f + 0.5f * MathF.Sin(t * fr + ph));
            var start = new Vector2(sx, -h * 0.1f);
            var end = start + Gfx.Dir(a) * (h * 1.6f);
            Gfx.FadeLine(dc, brush, start, end, wd * 0.9f, wd * 2.4f, LightColor, rstr * amp * 0.30f, 14, 1.2f);
            Gfx.FadeLine(dc, brush, start, end, wd * 0.3f, wd * 0.8f, white, rstr * amp * 0.30f, 14, 1.5f);
        }

        // ---- 6. 浮遊粒子（マリンスノー）----
        int pn = (int)c.F(ParticleCount);
        var rp = new Random(Seed + 3);
        for (int i = 0; i < pn; i++)
        {
            float x0 = rp.NextF() * w, y0 = rp.NextF() * h;
            float sp = 3f + rp.NextF() * 10f;
            float ph = rp.NextF() * MathF.PI * 2f;
            float r = 0.8f + rp.NextF() * 1.8f;
            float x = x0 + MathF.Sin(t * 0.3f + ph) * 18f;
            float y = Gfx.Mod(y0 + t * sp, h);
            float tw = 0.55f + 0.45f * MathF.Sin(t * (0.8f + rp.NextF()) + ph);
            brush.Color = Gfx.C4(LightColor, (0.15f + 0.35f * rp.NextF()) * tw * op);
            dc.FillEllipse(new Ellipse(new Vector2(x, y), r, r), brush);
        }

        // ---- 7. 泡（縁・ハイライト・ゆらぎ）----
        int bn = (int)c.F(BubbleCount);
        var rb = new Random(Seed + 7);
        for (int i = 0; i < bn; i++)
        {
            float x0 = rb.NextF() * w;
            float r = 3f + rb.NextF() * 10f;
            float sp = 40f + rb.NextF() * 80f;
            float ph = rb.NextF();
            float span = h + 2f * r;
            float y = h + r - Gfx.Mod(c.Time * sp + ph * span, span);
            float x = x0 + MathF.Sin(c.Time * 1.3f + ph * 6.28f) * (6f + r);
            var ctr = new Vector2(x, y);
            brush.Color = Gfx.C4(white, 0.06f * op);
            dc.FillEllipse(new Ellipse(ctr, r, r), brush);
            brush.Color = Gfx.C4(white, 0.45f * op);
            dc.DrawEllipse(new Ellipse(ctr, r, r), brush, 1.4f);
            brush.Color = Gfx.C4(white, 0.85f * op);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(-r * 0.35f, -r * 0.4f), r * 0.28f, r * 0.2f), brush);
            brush.Color = Gfx.C4(LightColor, 0.25f * op);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(r * 0.3f, r * 0.4f), r * 0.2f, r * 0.12f), brush);
        }
    }
}
