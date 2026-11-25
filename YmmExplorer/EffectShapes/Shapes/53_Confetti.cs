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

/// <summary>紙吹雪・クラッカー共通の色と紙片の描画</summary>
internal static class PartyKit
{
    public static readonly WpfColor[] Palette =
    {
        WpfColor.FromRgb(255, 76, 76), WpfColor.FromRgb(255, 159, 64), WpfColor.FromRgb(255, 224, 64),
        WpfColor.FromRgb(80, 220, 120), WpfColor.FromRgb(64, 200, 255), WpfColor.FromRgb(90, 120, 255),
        WpfColor.FromRgb(190, 100, 255), WpfColor.FromRgb(255, 105, 180),
    };

    /// <summary>ひらひら回る紙片を描く。shape: 0=長方形 1=円 2=細長いリボン片</summary>
    public static void Piece(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float size,
        float angle, float flip, WpfColor col, float alpha, int shape)
    {
        float cosF = MathF.Cos(flip);
        float sy = MathF.Max(0.12f, MathF.Abs(cosF));
        // 向きによって明るさが変わる（裏返ると少し暗い）
        var shaded = Gfx.Mix(col, WpfColor.FromRgb(0, 0, 0), (1f - sy) * 0.25f + (cosF < 0f ? 0.12f : 0f));
        dc.Transform = Matrix3x2.CreateScale(1f, sy, ctr) * Matrix3x2.CreateRotation(angle, ctr);
        brush.Color = Gfx.C4(shaded, alpha);
        switch (shape)
        {
            case 1:
                dc.FillEllipse(new Ellipse(ctr, size * 0.45f, size * 0.45f), brush);
                break;
            case 2:
                dc.FillRectangle(new RawRectF(ctr.X - size * 1.0f, ctr.Y - size * 0.14f, ctr.X + size * 1.0f, ctr.Y + size * 0.14f), brush);
                break;
            default:
                dc.FillRectangle(new RawRectF(ctr.X - size * 0.5f, ctr.Y - size * 0.3f, ctr.X + size * 0.5f, ctr.Y + size * 0.3f), brush);
                break;
        }
        dc.Transform = Matrix3x2.Identity;
    }
}

// =====================================================================
//  紙吹雪
//   ひらひら回りながら降り注ぐ紙片
// =====================================================================
public sealed class ConfettiPlugin : IShapePlugin
{
    public string Name => "演出：紙吹雪";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ConfettiParameter(sharedData);
}

public sealed class ConfettiParameter : EffectShapeParameterBase
{
    public ConfettiParameter() : this(null) { }
    public ConfettiParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "紙吹雪", Name = "カラフルにする")]
    [ToggleSlider]
    public bool Colorful { get => colorful; set => Set(ref colorful, value); }
    bool colorful = true;

    [Display(GroupName = "紙吹雪", Name = "色（カラフルがオフのとき）")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xD0, 0x40);

    [Display(GroupName = "紙吹雪", Name = "数")]
    [AnimationSlider("F0", "枚", 0, 600)]
    public Animation Count { get; } = new Animation(160, 0, 5000);

    [Display(GroupName = "紙吹雪", Name = "大きさ")]
    [AnimationSlider("F0", "px", 4, 40)]
    public Animation Size { get; } = new Animation(14, 1, 500);

    [Display(GroupName = "紙吹雪", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "動き", Name = "落ちる速さ")]
    [AnimationSlider("F0", "px/s", 20, 600)]
    public Animation Fall { get; } = new Animation(170, 1, 5000);

    [Display(GroupName = "動き", Name = "風（横方向）")]
    [AnimationSlider("F0", "px/s", -300, 300)]
    public Animation Wind { get; } = new Animation(20, -5000, 5000);

    [Display(GroupName = "動き", Name = "ゆらゆら揺れ幅")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Sway { get; } = new Animation(50, 0, 2000);

    [Display(GroupName = "動き", Name = "ひらひら回る速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Spin { get; } = new Animation(100, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Opacity, Fall, Wind, Sway, Spin];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), op = c.F(Opacity) / 100f;
        float fall = c.F(Fall), wind = c.F(Wind), sway = c.F(Sway), spin = c.F(Spin) / 100f;
        float margin = size * 3f, spanX = w + margin * 2f, spanY = h + margin * 2f;

        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);
        try
        {
            for (int i = 0; i < n; i++)
            {
                float x0 = rng.NextF() * spanX, ph = rng.NextF();
                float d = 0.5f + 0.5f * rng.NextF();
                float fr = 0.5f + rng.NextF(), ph2 = rng.NextF() * MathF.PI * 2f;
                float rot0 = rng.NextF() * MathF.PI * 2f, rs = (rng.NextF() - 0.5f) * 4f;
                float fs = 2f + 4f * rng.NextF(), fp = rng.NextF() * MathF.PI * 2f;
                int colIdx = rng.Next(0, PartyKit.Palette.Length);
                float shapeSel = rng.NextF();

                float y = -margin + Gfx.Mod(t * fall * d + ph * spanY, spanY);
                float x = Gfx.Mod(x0 + t * wind * d + MathF.Sin(t * fr + ph2) * sway * d, spanX) - margin;
                float angle = rot0 + t * rs * spin + MathF.Sin(t * fr * 1.5f + ph2) * 0.6f;
                float flip = fp + t * fs * spin;
                var col = Colorful ? PartyKit.Palette[colIdx] : Color;
                int shape = shapeSel < 0.6f ? 0 : (shapeSel < 0.82f ? 1 : 2);

                PartyKit.Piece(dc, brush, new Vector2(x, y), size * (0.6f + 0.4f * d), angle, flip, col, op * (0.6f + 0.4f * d), shape);
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
