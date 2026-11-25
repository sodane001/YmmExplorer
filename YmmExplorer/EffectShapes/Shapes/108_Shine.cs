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
//  光が走る（シャイン）
//   斜めの光の帯が通り過ぎる。ロゴや文字の上に重ねて強調に
// =====================================================================
public sealed class ShinePlugin : IShapePlugin
{
    public string Name => "演出：光が走る（シャイン）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ShineParameter(sharedData);
}

public sealed class ShineParameter : EffectShapeParameterBase
{
    public ShineParameter() : this(null) { }
    public ShineParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "光", Name = "光の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);

    [Display(GroupName = "光", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Intensity { get; } = new Animation(80, 0, 1000);

    [Display(GroupName = "光", Name = "帯の幅")]
    [AnimationSlider("F0", "px", 10, 400)]
    public Animation BandWidth { get; } = new Animation(110, 1, 4000);

    [Display(GroupName = "光", Name = "帯の傾き")]
    [AnimationSlider("F0", "°", -60, 60)]
    public Animation Angle { get; } = new Animation(25, -80, 80);

    [Display(GroupName = "光", Name = "細い光をもう1本")]
    [ToggleSlider]
    public bool TwinBand { get => dbl; set => Set(ref dbl, value); }
    bool dbl = true;

    [Display(GroupName = "範囲", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "範囲", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "範囲", Name = "光らせる範囲の幅")]
    [AnimationSlider("F0", "px", 50, 1920)]
    public Animation AreaWidth { get; } = new Animation(820, 2, 10000);

    [Display(GroupName = "範囲", Name = "光らせる範囲の高さ")]
    [AnimationSlider("F0", "px", 20, 1080)]
    public Animation AreaHeight { get; } = new Animation(220, 2, 10000);

    [Display(GroupName = "動き", Name = "通り過ぎる時間")]
    [AnimationSlider("F2", "秒", 0.1, 3)]
    public Animation Duration { get; } = new Animation(0.7, 0.03, 60);

    [Display(GroupName = "動き", Name = "くり返す間隔")]
    [AnimationSlider("F1", "秒", 0.3, 10)]
    public Animation Interval { get; } = new Animation(2.5, 0.05, 600);

    [Display(GroupName = "動き", Name = "くり返す")]
    [ToggleSlider]
    public bool Loop { get => loop; set => Set(ref loop, value); }
    bool loop = true;

    [Display(GroupName = "動き", Name = "最初に待つ時間")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation Delay { get; } = new Animation(0.2, 0, 600);

    [Display(GroupName = "動き", Name = "右から左へ")]
    [ToggleSlider]
    public bool Reverse { get => reverse; set => Set(ref reverse, value); }
    bool reverse = false;

    [Display(GroupName = "装飾", Name = "帯のはしにきらめきを出す")]
    [ToggleSlider]
    public bool Sparkle { get => sparkle; set => Set(ref sparkle, value); }
    bool sparkle = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Intensity, BandWidth, Angle, PosX, PosY, AreaWidth, AreaHeight, Duration, Interval, Delay];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time - c.F(Delay);
        if (t < 0f) return;
        float dur = MathF.Max(0.03f, c.F(Duration)), interval = MathF.Max(dur, c.F(Interval));
        if (!Loop && t > dur) return;
        float lt = Loop ? Gfx.Mod(t, interval) : t;
        if (lt > dur) return;
        float u = Gfx.Smooth(0f, 1f, lt / dur);
        if (Reverse) u = 1f - u;

        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float AW = c.F(AreaWidth), AH = c.F(AreaHeight), bw = c.F(BandWidth), inten = c.F(Intensity) / 100f;
        float x0 = ctr.X - AW / 2f, x1 = ctr.X + AW / 2f, y0 = ctr.Y - AH / 2f, y1 = ctr.Y + AH / 2f;
        float slant = AH * MathF.Tan(Gfx.Rad(c.F(Angle)));
        // 帯の中心（下辺での x）。範囲の左外から右外まで動く
        float cx = x0 - bw - MathF.Max(0f, slant) + (AW + bw * 2f + MathF.Abs(slant)) * u + MathF.Min(0f, slant) * (1f - u);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            void Strip(ID2D1SolidColorBrush b, float center, float width, float strength)
            {
                const int slices = 16;
                for (int i = 0; i < slices; i++)
                {
                    float o0 = -width / 2f + width * i / slices, o1 = o0 + width / slices;
                    float mid = (o0 + o1) / 2f / (width / 2f);
                    float a = strength * MathF.Exp(-mid * mid * 2.2f);
                    if (a < 0.003f) continue;
                    var poly = new List<Vector2>
                    {
                        new(center + o0 + slant, y0), new(center + o1 + slant, y0),
                        new(center + o1, y1), new(center + o0, y1),
                    };
                    var clipped = DrawKit.ClipRect(poly, x0, y0, x1, y1);
                    if (clipped.Count < 3) continue;
                    b.Color = Gfx.C4(Color, Math.Clamp(a, 0f, 1f));
                    Gfx.FillPoly(dc, b, clipped);
                }
            }
            Strip(brush, cx, bw, 0.55f * inten);
            if (TwinBand) Strip(brush, cx - bw * 0.95f, bw * 0.25f, 0.4f * inten);

            if (Sparkle)
            {
                float env = MathF.Sin(MathF.PI * Math.Clamp(lt / dur, 0f, 1f));
                var top = new Vector2(cx + slant, y0);
                var bot = new Vector2(cx, y1);
                if (top.X > x0 && top.X < x1)
                    DrawKit.Twinkle(dc, brush, top, bw * 0.7f * env, 2.5f, Color, 0.9f * env * MathF.Min(1f, inten), lt * 3f);
                if (bot.X > x0 && bot.X < x1)
                    DrawKit.Twinkle(dc, brush, bot, bw * 0.45f * env, 2f, Color, 0.7f * env * MathF.Min(1f, inten), -lt * 3f);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
