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
//  お金が降る
//   くるくる回る金貨と、ひらひら舞うお札が降ってくる（特定の通貨ではない絵柄）
// =====================================================================
public sealed class MoneyRainPlugin : IShapePlugin
{
    public string Name => "演出：お金が降る";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MoneyRainParameter(sharedData);
}

public sealed class MoneyRainParameter : EffectShapeParameterBase
{
    public MoneyRainParameter() : this(null) { }
    public MoneyRainParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "金貨", Name = "金貨の数")]
    [AnimationSlider("F0", "枚", 0, 200)]
    public Animation CoinCount { get; } = new Animation(45, 0, 2000);

    [Display(GroupName = "金貨", Name = "金貨の色")]
    [ColorPicker]
    public WpfColor CoinColor { get => coinColor; set => Set(ref coinColor, value); }
    WpfColor coinColor = WpfColor.FromRgb(0xF2, 0xC2, 0x2E);

    [Display(GroupName = "金貨", Name = "金貨の大きさ")]
    [AnimationSlider("F0", "px", 6, 80)]
    public Animation CoinSize { get; } = new Animation(22, 1, 1000);

    [Display(GroupName = "お札", Name = "お札の数")]
    [AnimationSlider("F0", "枚", 0, 120)]
    public Animation BillCount { get; } = new Animation(22, 0, 1000);

    [Display(GroupName = "お札", Name = "お札の色")]
    [ColorPicker]
    public WpfColor BillColor { get => billColor; set => Set(ref billColor, value); }
    WpfColor billColor = WpfColor.FromRgb(0x7C, 0xC4, 0x8A);

    [Display(GroupName = "お札", Name = "お札の大きさ")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation BillSize { get; } = new Animation(110, 2, 3000);

    [Display(GroupName = "動き", Name = "落ちる速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "動き", Name = "きらめき")]
    [ToggleSlider]
    public bool Sparkle { get => sparkle; set => Set(ref sparkle, value); }
    bool sparkle = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [CoinCount, CoinSize, BillCount, BillSize, Speed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time, sp = c.F(Speed) / 100f;
        int nc = (int)c.F(CoinCount), nb = (int)c.F(BillCount);
        float cs = c.F(CoinSize), bs = c.F(BillSize);
        var white = WpfColor.FromRgb(255, 255, 255);
        var cdark = Gfx.Mix(CoinColor, WpfColor.FromRgb(90, 50, 0), 0.5f);
        var clite = Gfx.Mix(CoinColor, white, 0.5f);
        var bdark = Gfx.Mix(BillColor, WpfColor.FromRgb(0, 30, 10), 0.45f);
        var blite = Gfx.Mix(BillColor, white, 0.55f);

        using var brush = dc.CreateSolidColorBrush(default);
        var prev = dc.Transform;
        try
        {
            // ---- お札（奥）----
            for (int i = 0; i < nb; i++)
            {
                float d = 0.5f + 0.5f * DrawKit.H(Seed, i, 1);
                float s = bs * d;
                float travel = h + s * 2f;
                float y = -s + DrawKit.Frac(t * sp * 90f * d / travel + DrawKit.H(Seed, i, 2)) * travel;
                float x = DrawKit.H(Seed, i, 3) * w + MathF.Sin(t * 1.3f + i) * 50f * d;
                float rot = MathF.Sin(t * 1.7f + i * 2f) * 0.6f + i;
                float flip = MathF.Cos(t * 2.3f * (0.6f + 0.6f * DrawKit.H(Seed, i, 4)) + i);
                var p = new Vector2(x, y);
                dc.Transform = Matrix3x2.CreateScale(1f, MathF.Max(0.08f, MathF.Abs(flip)), p) * Matrix3x2.CreateRotation(rot, p) * prev;
                float bw = s, bh = s * 0.46f;
                brush.Color = Gfx.C4(flip > 0f ? BillColor : bdark, 1f);
                dc.FillRectangle(new RawRectF(p.X - bw / 2f, p.Y - bh / 2f, p.X + bw / 2f, p.Y + bh / 2f), brush);
                brush.Color = Gfx.C4(flip > 0f ? bdark : BillColor, 0.8f);
                dc.DrawRectangle(new RawRectF(p.X - bw * 0.44f, p.Y - bh * 0.38f, p.X + bw * 0.44f, p.Y + bh * 0.38f), brush, MathF.Max(1f, s * 0.02f));
                dc.DrawEllipse(new Ellipse(p, bh * 0.28f, bh * 0.28f), brush, MathF.Max(1f, s * 0.02f));
                brush.Color = Gfx.C4(blite, 0.7f);
                dc.FillEllipse(new Ellipse(p + new Vector2(-bw * 0.3f, 0), bh * 0.12f, bh * 0.12f), brush);
                dc.FillEllipse(new Ellipse(p + new Vector2(bw * 0.3f, 0), bh * 0.12f, bh * 0.12f), brush);
            }
            dc.Transform = prev;

            // ---- 金貨（手前）----
            for (int i = 0; i < nc; i++)
            {
                float d = 0.5f + 0.5f * DrawKit.H(Seed, i, 11);
                float r = cs * d;
                float travel = h + r * 4f;
                float y = -r * 2f + DrawKit.Frac(t * sp * 220f * d / travel + DrawKit.H(Seed, i, 12)) * travel;
                float x = DrawKit.H(Seed, i, 13) * w + MathF.Sin(t * 2f + i) * 10f;
                var p = new Vector2(x, y);
                float spin = MathF.Cos(t * 7f * (0.6f + 0.8f * DrawKit.H(Seed, i, 14)) + i);
                float rx = MathF.Max(0.12f, MathF.Abs(spin)) * r;
                brush.Color = Gfx.C4(cdark, 1f);
                dc.FillEllipse(new Ellipse(p + new Vector2(r * 0.12f * MathF.Sign(spin == 0 ? 1 : spin), 0), rx, r), brush);
                brush.Color = Gfx.C4(CoinColor, 1f);
                dc.FillEllipse(new Ellipse(p, rx, r), brush);
                brush.Color = Gfx.C4(cdark, 0.8f);
                dc.DrawEllipse(new Ellipse(p, rx * 0.72f, r * 0.72f), brush, MathF.Max(1f, r * 0.08f));
                brush.Color = Gfx.C4(clite, 0.9f);
                dc.FillEllipse(new Ellipse(p + new Vector2(-rx * 0.35f, -r * 0.35f), rx * 0.18f, r * 0.14f), brush);
                if (Sparkle && DrawKit.Frac(t * 0.9f + DrawKit.H(Seed, i, 15)) < 0.12f)
                {
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    DrawKit.Twinkle(dc, brush, p + new Vector2(-rx * 0.3f, -r * 0.3f), r * 1.2f, 1.5f, white, 0.9f);
                    dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                }
            }
        }
        finally
        {
            dc.Transform = prev;
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
