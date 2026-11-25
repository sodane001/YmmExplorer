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
//  データストリーム
//   流れる配線のレーンに、データのパケットと数字が走る
// =====================================================================
public sealed class DataStreamPlugin : IShapePlugin
{
    public string Name => "演出：データストリーム";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DataStreamParameter(sharedData);
}

public sealed class DataStreamParameter : EffectShapeParameterBase
{
    public DataStreamParameter() : this(null) { }
    public DataStreamParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x4F, 0xE8, 0xFF);

    [Display(GroupName = "色", Name = "強調の色")]
    [ColorPicker]
    public WpfColor AccentColor { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0xE0, 0x4A);

    [Display(GroupName = "ストリーム", Name = "流れる向き")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Direction { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "ストリーム", Name = "レーンの数")]
    [AnimationSlider("F0", "本", 2, 60)]
    public Animation Lanes { get; } = new Animation(18, 1, 400);

    [Display(GroupName = "ストリーム", Name = "流れる速さ")]
    [AnimationSlider("F0", "px/s", 50, 1500)]
    public Animation Speed { get; } = new Animation(380, 1, 20000);

    [Display(GroupName = "ストリーム", Name = "パケットの密度")]
    [AnimationSlider("F0", "%", 5, 100)]
    public Animation Density { get; } = new Animation(60, 1, 100);

    [Display(GroupName = "ストリーム", Name = "パケットの高さ")]
    [AnimationSlider("F0", "px", 3, 40)]
    public Animation PacketHeight { get; } = new Animation(10, 1, 300);

    [Display(GroupName = "ストリーム", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glow { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "数字", Name = "数字を流す")]
    [ToggleSlider]
    public bool ShowNumbers { get => showNumbers; set => Set(ref showNumbers, value); }
    bool showNumbers = true;

    [Display(GroupName = "数字", Name = "数字の高さ")]
    [AnimationSlider("F0", "px", 8, 60)]
    public Animation DigitHeight { get; } = new Animation(18, 4, 400);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Direction, Lanes, Speed, Density, PacketHeight, Glow, DigitHeight];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float ang = Gfx.Rad(c.F(Direction));
        var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
        var perp = new Vector2(-dir.Y, dir.X);
        var center = new Vector2(w / 2f, h / 2f);
        float diag = MathF.Sqrt(w * w + h * h);
        int lanes = (int)c.F(Lanes);
        float speed = c.F(Speed), dens = c.F(Density) / 100f, ph = c.F(PacketHeight), glow = c.F(Glow) / 100f;
        float dh = c.F(DigitHeight);
        float laneGap = diag / MathF.Max(1, lanes);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            for (int l = 0; l < lanes; l++)
            {
                var rl = new Random(Seed * 977 + l * 131);
                float v = (l + 0.5f) * laneGap - diag / 2f;
                float sp = speed * (0.5f + rl.NextF());
                float off = rl.NextF() * 1000f;

                // レーンの細い線
                var a = center + perp * v - dir * diag / 2f;
                var b = center + perp * v + dir * diag / 2f;
                brush.Color = Gfx.C4(Color, 0.12f);
                dc.DrawLine(a, b, brush, 1f);

                // パケット列：決まった並び（周期）を繰り返して流す（つなぎ目で飛ばない）
                var items = new List<(float len, float gap, float pr, int digit)>();
                float period = 0f;
                for (int k = 0; k < 40; k++)
                {
                    float len0 = 8f + rl.NextF() * 60f, gap0 = 10f + rl.NextF() * 70f, pr0 = rl.NextF();
                    int dg = rl.Next(0, 10);
                    if (ShowNumbers && pr0 >= dens && pr0 < dens + 0.2f) len0 = dh * 0.55f + 8f;
                    items.Add((len0, gap0, pr0, dg));
                    period += len0 + gap0;
                }
                float x = -Gfx.Mod(off + t * sp, period);
                int j = 0;
                while (x < diag)
                {
                    var it = items[j % items.Count];
                    j++;
                    var p0 = center + perp * v + dir * (x - diag / 2f);
                    var p1 = center + perp * v + dir * (x + it.len - diag / 2f);
                    if (it.pr < dens)
                    {
                        bool hi = it.pr < dens * 0.15f;
                        var col = hi ? AccentColor : Color;
                        if (glow > 0f)
                        {
                            brush.Color = Gfx.C4(col, 0.14f * glow);
                            dc.DrawLine(p0, p1, brush, ph * 2.6f);
                        }
                        brush.Color = Gfx.C4(col, hi ? 0.95f : 0.65f);
                        dc.DrawLine(p0, p1, brush, ph);
                        if (hi)
                        {
                            brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.9f);
                            dc.FillEllipse(new Ellipse(p1, ph * 0.7f, ph * 0.7f), brush);
                        }
                    }
                    else if (ShowNumbers && it.pr < dens + 0.2f)
                    {
                        if (p0.X > -40f && p0.X < w + 40f && p0.Y > -40f && p0.Y < h + 40f)
                        {
                            brush.Color = Gfx.C4(Color, 0.8f);
                            ShapeGeom.Digit(dc, brush, p0.X, p0.Y - dh / 2f, dh * 0.55f, dh, it.digit, MathF.Max(1.2f, dh * 0.08f));
                        }
                    }
                    x += it.len + it.gap;
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
