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
//  レーダー・ソナー画面
//   回転する走査線・同心円・点が走査のたびに光って薄れていく
// =====================================================================
public sealed class RadarPlugin : IShapePlugin
{
    public string Name => "演出：レーダー・ソナー画面";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RadarParameter(sharedData);
}

public sealed class RadarParameter : EffectShapeParameterBase
{
    public RadarParameter() : this(null) { }
    public RadarParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "画面の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x39, 0xFF, 0x88);

    [Display(GroupName = "色", Name = "反応（点）の色")]
    [ColorPicker]
    public WpfColor BlipColor { get => blip; set => Set(ref blip, value); }
    WpfColor blip = WpfColor.FromRgb(0xE8, 0xFF, 0xF0);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "半径")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Radius { get; } = new Animation(300, 5, 5000);

    [Display(GroupName = "画面", Name = "同心円の数")]
    [AnimationSlider("F0", "本", 1, 8)]
    public Animation Rings { get; } = new Animation(4, 1, 30);

    [Display(GroupName = "画面", Name = "黒いガラスの塗り")]
    [ToggleSlider]
    public bool Fill { get => fill; set => Set(ref fill, value); }
    bool fill = true;

    [Display(GroupName = "画面", Name = "目盛りを表示する")]
    [ToggleSlider]
    public bool Ticks { get => ticks; set => Set(ref ticks, value); }
    bool ticks = true;

    [Display(GroupName = "走査", Name = "回転の速さ")]
    [AnimationSlider("F0", "°/s", -360, 360)]
    public Animation SweepSpeed { get; } = new Animation(100, -3600, 3600);

    [Display(GroupName = "走査", Name = "残光の長さ")]
    [AnimationSlider("F0", "°", 10, 180)]
    public Animation Trail { get; } = new Animation(80, 3, 360);

    [Display(GroupName = "走査", Name = "反応（点）の数")]
    [AnimationSlider("F0", "個", 0, 60)]
    public Animation Blips { get; } = new Animation(14, 0, 500);

    [Display(GroupName = "走査", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Rings, SweepSpeed, Trail, Blips, Brightness];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), B = c.F(Brightness) / 100f;
        int rings = Math.Clamp((int)c.F(Rings), 1, 8), nb = (int)c.F(Blips);
        float sweep = Gfx.Rad(c.F(SweepSpeed)) * t, trail = Gfx.Rad(c.F(Trail));
        float sgn = c.F(SweepSpeed) >= 0f ? 1f : -1f;
        const float twoPi = MathF.PI * 2f;

        using var brush = dc.CreateSolidColorBrush(default);

        if (Fill)
        {
            brush.Color = Gfx.C4(WpfColor.FromRgb(0x02, 0x10, 0x08), 0.55f);
            dc.FillEllipse(new Ellipse(ctr, R, R), brush);
        }

        // ---- 同心円・十字・目盛り ----
        brush.Color = Gfx.C4(Color, 0.35f * B);
        for (int k = 1; k <= rings; k++) dc.DrawEllipse(new Ellipse(ctr, R * k / rings, R * k / rings), brush, 1.2f);
        dc.DrawLine(ctr - new Vector2(R, 0f), ctr + new Vector2(R, 0f), brush, 1f);
        dc.DrawLine(ctr - new Vector2(0f, R), ctr + new Vector2(0f, R), brush, 1f);
        if (Ticks)
        {
            for (int i = 0; i < 72; i++)
            {
                float a = i * twoPi / 72f;
                var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                brush.Color = Gfx.C4(Color, 0.7f * B);
                dc.DrawLine(ctr + d * R, ctr + d * (R + (i % 6 == 0 ? 14f : 6f)), brush, i % 6 == 0 ? 2f : 1f);
            }
        }
        brush.Color = Gfx.C4(Color, 0.9f * B);
        dc.DrawEllipse(new Ellipse(ctr, R, R), brush, 2.5f);

        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 走査線の残光 ----
            const int N = 40;
            for (int j = N - 1; j >= 0; j--)
            {
                float a = sweep - sgn * j * trail / N;
                float al = MathF.Pow(1f - j / (float)N, 2f) * 0.28f * B;
                brush.Color = Gfx.C4(Color, al);
                dc.DrawLine(ctr, ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * R, brush, MathF.Max(3f, R * 2f * trail / N * 1.15f * 0.5f));
            }
            var lead = new Vector2(MathF.Cos(sweep), MathF.Sin(sweep));
            brush.Color = Gfx.C4(Color, 0.9f * B);
            dc.DrawLine(ctr, ctr + lead * R, brush, 2.4f);

            // ---- 反応（走査の線が通過した直後に光り、薄れていく）----
            var rb = new Random(Seed);
            for (int i = 0; i < nb; i++)
            {
                float a = rb.NextF() * twoPi, rr = R * MathF.Sqrt(rb.NextF()) * 0.94f, sz = 3f + 3f * rb.NextF();
                float drift = (rb.NextF() - 0.5f) * 0.06f;
                float ang = a + drift * t;
                var p = ctr + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * rr;
                float age = Gfx.Mod((sweep - ang) * sgn, twoPi) / twoPi;                    // 走査が通過してからの割合
                float al = MathF.Pow(1f - age, 2.2f);
                if (al < 0.03f) continue;
                brush.Color = Gfx.C4(Color, 0.25f * al * B);
                dc.FillEllipse(new Ellipse(p, sz * 2.6f, sz * 2.6f), brush);
                brush.Color = Gfx.C4(BlipColor, 0.95f * al * B);
                dc.FillEllipse(new Ellipse(p, sz, sz), brush);
                if (age < 0.08f)     // 通過直後の反響の輪
                {
                    float u = age / 0.08f;
                    brush.Color = Gfx.C4(BlipColor, 0.7f * (1f - u) * B);
                    dc.DrawEllipse(new Ellipse(p, sz * (1.5f + 4f * u), sz * (1.5f + 4f * u)), brush, 1.4f);
                }
            }

            // ---- 中心の光 ----
            using var cb = Gfx.Radial(dc, ctr, R * 0.08f, (0f, Gfx.C4(BlipColor, 0.9f * B)), (1f, Gfx.C4(Color, 0f)));
            dc.FillEllipse(new Ellipse(ctr, R * 0.08f, R * 0.08f), cb);
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}

public enum WaveKind
{
    [Display(Name = "心電図")] Ecg,
    [Display(Name = "オシロスコープ")] Scope,
    [Display(Name = "音声バー（左右対称）")] AudioBars,
    [Display(Name = "スペクトラム（下から）")] Spectrum,
}
