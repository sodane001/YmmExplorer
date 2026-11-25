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
//  火山の噴火
//   山のシルエット・火口の赤熱・飛び散る溶岩・立ちのぼる噴煙・稲妻
// =====================================================================
public sealed class VolcanoPlugin : IShapePlugin
{
    public string Name => "演出：火山の噴火";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new VolcanoParameter(sharedData);
}

public sealed class VolcanoParameter : EffectShapeParameterBase
{
    public VolcanoParameter() : this(null) { }
    public VolcanoParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "山の色")]
    [ColorPicker]
    public WpfColor MountainColor { get => mt; set => Set(ref mt, value); }
    WpfColor mt = WpfColor.FromRgb(0x1C, 0x10, 0x14);

    [Display(GroupName = "色", Name = "溶岩の色")]
    [ColorPicker]
    public WpfColor LavaColor { get => lava; set => Set(ref lava, value); }
    WpfColor lava = WpfColor.FromRgb(0xFF, 0x6A, 0x14);

    [Display(GroupName = "色", Name = "噴煙の色")]
    [ColorPicker]
    public WpfColor AshColor { get => ash; set => Set(ref ash, value); }
    WpfColor ash = WpfColor.FromRgb(0x3A, 0x34, 0x34);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "山のすそのY位置")]
    [AnimationSlider("F0", "%", 0, 120)]
    public Animation BaseY { get; } = new Animation(100, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "山の高さ")]
    [AnimationSlider("F0", "px", 100, 900)]
    public Animation MountHeight { get; } = new Animation(380, 20, 6000);

    [Display(GroupName = "位置と大きさ", Name = "山のすその幅")]
    [AnimationSlider("F0", "px", 200, 2000)]
    public Animation MountWidth { get; } = new Animation(1100, 40, 12000);

    [Display(GroupName = "噴火", Name = "噴火の勢い")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Power { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "噴火", Name = "溶岩の粒の数")]
    [AnimationSlider("F0", "個", 0, 400)]
    public Animation Bombs { get; } = new Animation(120, 0, 5000);

    [Display(GroupName = "噴火", Name = "噴煙の量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation AshAmount { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "噴火", Name = "噴煙の中の稲妻")]
    [ToggleSlider]
    public bool Lightning { get => lightning; set => Set(ref lightning, value); }
    bool lightning = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, BaseY, MountHeight, MountWidth, Power, Bombs, AshAmount];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float cx = c.F(PosX) / 100f * w, by = c.F(BaseY) / 100f * h;
        float MH = c.F(MountHeight), MW = c.F(MountWidth), power = c.F(Power) / 100f;
        int nb = (int)c.F(Bombs);
        float ashK = c.F(AshAmount) / 100f;
        var crater = new Vector2(cx, by - MH);
        float topHalf = MW * 0.07f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 噴煙（山の後ろに立ちのぼる）----
        if (ashK > 0f)
        {
            var ra = new Random(Seed + 3);
            for (int i = 0; i < 38; i++)
            {
                float ph = ra.NextF(), sp = 0.5f + 0.8f * ra.NextF(), dx = (ra.NextF() * 2f - 1f), sz = 0.7f + 0.7f * ra.NextF();
                float u = Gfx.Mod(t * 0.16f * sp * (0.5f + 0.5f * power) + ph, 1f);
                float rise = u * MH * 1.9f * (0.6f + 0.4f * power);
                var p = crater + new Vector2(dx * (60f + 280f * u) + MathF.Sin(t * 0.5f + ph * 9f) * 20f * u, -rise);
                float r = (50f + 190f * u) * sz;
                float a = ashK * 0.5f * MathF.Sin(MathF.PI * MathF.Min(1f, u * 1.1f));
                var col = Gfx.Mix(AshColor, LavaColor, MathF.Pow(1f - u, 4f) * 0.35f);
                using var gb = Gfx.Radial(dc, p, r, (0f, Gfx.C4(col, a)), (0.6f, Gfx.C4(col, a * 0.5f)), (1f, Gfx.C4(col, 0f)));
                dc.FillEllipse(new Ellipse(p, r, r), gb);
            }
            // 噴煙の中の稲妻
            if (Lightning)
            {
                int tick = (int)MathF.Floor(t * 7f);
                var rl = new Random(Seed * 31 + tick);
                if (rl.NextF() < 0.55f)
                {
                    var q = crater + new Vector2((rl.NextF() - 0.5f) * 240f, -MH * (0.35f + 0.6f * rl.NextF()));
                    var pts = new List<Vector2> { q };
                    float a = MathF.PI / 2f + (rl.NextF() - 0.5f) * 1.2f;
                    for (int i = 0; i < 7; i++)
                    {
                        a += (rl.NextF() - 0.5f) * 1.4f;
                        q += new Vector2(MathF.Cos(a), MathF.Sin(a)) * (26f + 30f * rl.NextF());
                        pts.Add(q);
                    }
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    brush.Color = Gfx.C4(WpfColor.FromRgb(0xB0, 0xC0, 0xFF), 0.35f);
                    FormKit.Stroke(dc, brush, pts, 9f, false);
                    brush.Color = Gfx.C4(white, 0.95f);
                    FormKit.Stroke(dc, brush, pts, 2.2f, false);
                    dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                }
            }
        }

        // ---- 山 ----
        brush.Color = Gfx.C4(MountainColor, 1f);
        Gfx.FillPoly(dc, brush, new List<Vector2>
        {
            new(cx - MW / 2f, by + 40f), new(cx - MW * 0.28f, by - MH * 0.35f), new(cx - topHalf, by - MH),
            new(cx + topHalf * 0.6f, by - MH * 0.96f), new(cx + MW * 0.26f, by - MH * 0.4f), new(cx + MW / 2f, by + 40f),
        }, -2f, h + 2f);

        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 火口の赤熱 ----
            float gr = topHalf * 3.2f;
            using (var gb = Gfx.Radial(dc, crater, gr, (0f, Gfx.C4(WpfColor.FromRgb(255, 235, 160), 0.85f * power)), (0.35f, Gfx.C4(LavaColor, 0.45f * power)), (1f, Gfx.C4(LavaColor, 0f))))
                dc.FillEllipse(new Ellipse(crater, gr, gr * 0.7f), gb);

            // ---- 山肌を流れ下る溶岩 ----
            for (int k = 0; k < 3; k++)
            {
                float side = k - 1;
                var pts = new List<Vector2>();
                for (int i = 0; i <= 24; i++)
                {
                    float f = i / 24f;
                    pts.Add(new Vector2(cx + side * (topHalf * 0.6f + (MW * 0.2f) * f) + MathF.Sin(f * 9f + k * 2f) * 8f, by - MH + f * MH * (0.78f + 0.06f * k)));
                }
                float pulse = 0.6f + 0.4f * MathF.Sin(t * 1.5f + k * 2f);
                brush.Color = Gfx.C4(LavaColor, 0.25f * power * pulse);
                FormKit.Stroke(dc, brush, pts, 14f, false);
                brush.Color = Gfx.C4(WpfColor.FromRgb(255, 215, 110), 0.85f * power * pulse);
                FormKit.Stroke(dc, brush, pts, 3f, false);
            }

            // ---- 飛び散る溶岩（放物線）----
            var rb = new Random(Seed);
            for (int i = 0; i < nb; i++)
            {
                float ang = -MathF.PI / 2f + (rb.NextF() - 0.5f) * 1.25f, sp = (380f + 520f * rb.NextF()) * (0.5f + 0.6f * power);
                float ph = rb.NextF(), period = 1.6f + 1.2f * rb.NextF();
                float tau = Gfx.Mod(t + ph * period, period);
                float g = 700f;
                var d = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                var p = crater + d * (sp * tau) + new Vector2(0f, 0.5f * g * tau * tau);
                var p2 = crater + d * (sp * MathF.Max(0f, tau - 0.04f)) + new Vector2(0f, 0.5f * g * MathF.Pow(MathF.Max(0f, tau - 0.04f), 2f));
                if (p.Y > by + 20f) continue;
                float a = MathF.Min(1f, (period - tau) * 2f) * power;
                brush.Color = Gfx.C4(Gfx.Mix(WpfColor.FromRgb(255, 225, 120), LavaColor, tau / period), a);
                dc.DrawLine(p2, p, brush, 2.4f);
                dc.FillEllipse(new Ellipse(p, 2.6f, 2.6f), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
