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
//  雪の結晶
//   6回対称の結晶がゆっくり回りながら舞い落ちる
// =====================================================================
public sealed class SnowflakePlugin : IShapePlugin
{
    public string Name => "演出：雪の結晶";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SnowflakeParameter(sharedData);
}

public sealed class SnowflakeParameter : EffectShapeParameterBase
{
    public SnowflakeParameter() : this(null) { }
    public SnowflakeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "結晶", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xDF, 0xF4, 0xFF);

    [Display(GroupName = "結晶", Name = "数")]
    [AnimationSlider("F0", "個", 0, 120)]
    public Animation Count { get; } = new Animation(28, 0, 1000);

    [Display(GroupName = "結晶", Name = "大きさ（半径）")]
    [AnimationSlider("F0", "px", 8, 120)]
    public Animation Size { get; } = new Animation(36, 3, 1000);

    [Display(GroupName = "結晶", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 6)]
    public Animation Thickness { get; } = new Animation(1.8, 0.2, 50);

    [Display(GroupName = "結晶", Name = "光のにじみ")]
    [ToggleSlider]
    public bool Glow { get => glow; set => Set(ref glow, value); }
    bool glow = true;

    [Display(GroupName = "動き", Name = "落ちる速さ")]
    [AnimationSlider("F0", "px/s", 10, 300)]
    public Animation Fall { get; } = new Animation(55, 1, 5000);

    [Display(GroupName = "動き", Name = "揺れ幅")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Sway { get; } = new Animation(45, 0, 2000);

    [Display(GroupName = "動き", Name = "回転の速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Spin { get; } = new Animation(60, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Thickness, Fall, Sway, Spin];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), th = c.F(Thickness), fall = c.F(Fall), sway = c.F(Sway), spin = c.F(Spin) / 100f;
        float margin = size * 2.5f, spanY = h + margin * 2f;

        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float x0 = rng.NextF() * w, ph = rng.NextF();
            float d = 0.45f + 0.55f * rng.NextF();
            float fr = 0.4f + 0.7f * rng.NextF(), ph2 = rng.NextF() * MathF.PI * 2f;
            float rot0 = rng.NextF() * MathF.PI * 2f, rs = (rng.NextF() - 0.5f) * 1.2f;
            int branches = 2 + rng.Next(0, 3);          // 側枝の段数
            bool ring = rng.NextF() < 0.5f, fork = rng.NextF() < 0.5f;

            float y = -margin + Gfx.Mod(t * fall * d + ph * spanY, spanY);
            float x = x0 + MathF.Sin(t * fr + ph2) * sway * d;
            float R = size * d;
            float rot = rot0 + t * rs * spin;
            var ctr = new Vector2(x, y);
            float a = 0.35f + 0.6f * d;

            if (Glow)
            {
                brush.Color = Gfx.C4(Color, 0.07f * a);
                dc.FillEllipse(new Ellipse(ctr, R * 1.25f, R * 1.25f), brush);
            }

            for (int arm = 0; arm < 6; arm++)
            {
                float ang = rot + arm * MathF.PI / 3f;
                var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                var perp = new Vector2(-dir.Y, dir.X);
                brush.Color = Gfx.C4(Color, a);
                dc.DrawLine(ctr, ctr + dir * R, brush, th);
                // 側枝（根元から先へ向かって短くなる）
                for (int b = 1; b <= branches; b++)
                {
                    float f = 0.25f + 0.55f * b / branches;
                    float bl = R * (0.42f - 0.08f * b) ;
                    var bp = ctr + dir * (R * f);
                    dc.DrawLine(bp, bp + (dir * 0.5f + perp * 0.866f) * bl, brush, th * 0.8f);
                    dc.DrawLine(bp, bp + (dir * 0.5f - perp * 0.866f) * bl, brush, th * 0.8f);
                }
                if (fork)   // 先端の小さなY字
                {
                    var tp = ctr + dir * R;
                    dc.DrawLine(tp, tp + (dir * 0.6f + perp * 0.8f) * (R * 0.16f), brush, th * 0.8f);
                    dc.DrawLine(tp, tp + (dir * 0.6f - perp * 0.8f) * (R * 0.16f), brush, th * 0.8f);
                }
            }
            if (ring)   // 中心の六角形
            {
                brush.Color = Gfx.C4(Color, a * 0.9f);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(ShapeGeom.StarPoly(ctr, R * 0.2f, 6, 1f, rot)), th * 0.9f);
            }
        }
    }
}
