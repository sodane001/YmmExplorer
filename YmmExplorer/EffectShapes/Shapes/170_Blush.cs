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

public enum BlushStyle
{
    [Display(Name = "ふんわり（ぼかし）")] Soft,
    [Display(Name = "斜線つき")] Lines,
    [Display(Name = "まる（アニメ風）")] Circle,
}

// =====================================================================
//  照れ・ほっぺの赤み
//   両ほおに赤みがふわっと出る。斜線や湯気(ぽっ)もつけられる
// =====================================================================
public sealed class BlushPlugin : IShapePlugin
{
    public string Name => "演出：照れ・ほっぺの赤み";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BlushParameter(sharedData);
}

public sealed class BlushParameter : EffectShapeParameterBase
{
    public BlushParameter() : this(null) { }
    public BlushParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "形")]
    [EnumComboBox]
    public BlushStyle Style { get => style; set => Set(ref style, value); }
    BlushStyle style = BlushStyle.Lines;

    [Display(GroupName = "形", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x5C, 0x7A);

    [Display(GroupName = "形", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "形", Name = "大きさ")]
    [AnimationSlider("F0", "px", 10, 300)]
    public Animation Size { get; } = new Animation(70, 2, 3000);

    [Display(GroupName = "形", Name = "湯気（ぽっ）を出す")]
    [ToggleSlider]
    public bool Steam { get => steam; set => Set(ref steam, value); }
    bool steam = false;

    [Display(GroupName = "位置", Name = "左のほおX")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LeftX { get; } = new Animation(43, -100, 200);

    [Display(GroupName = "位置", Name = "左のほおY")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LeftY { get; } = new Animation(48, -100, 200);

    [Display(GroupName = "位置", Name = "右のほおX")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RightX { get; } = new Animation(57, -100, 200);

    [Display(GroupName = "位置", Name = "右のほおY")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RightY { get; } = new Animation(48, -100, 200);

    [Display(GroupName = "動き", Name = "ふわっと出る時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation InTime { get; } = new Animation(0.5, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Strength, Size, LeftX, LeftY, RightX, RightY, InTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, c.F(InTime), 0.3f);
        if (k <= 0.001f) return;
        float a = c.F(Strength) / 100f * k, s = c.F(Size);
        float pulse = 1f + 0.05f * MathF.Sin(t * 3f);
        using var brush = dc.CreateSolidColorBrush(default);
        foreach (var p in new[] { new Vector2(c.F(LeftX) / 100f * w, c.F(LeftY) / 100f * h), new Vector2(c.F(RightX) / 100f * w, c.F(RightY) / 100f * h) })
        {
            float rx = s * pulse, ry = s * 0.55f * pulse;
            switch (Style)
            {
                case BlushStyle.Circle:
                    brush.Color = Gfx.C4(Color, 0.75f * a);
                    dc.FillEllipse(new Ellipse(p, rx * 0.8f, ry * 0.9f), brush);
                    break;
                default:
                {
                    // ぼかした楕円(縦につぶした円のぼかし)
                    var prev = dc.Transform;
                    dc.Transform = Matrix3x2.CreateScale(1f, ry / rx, p) * prev;
                    using (var g = Gfx.Radial(dc, p, rx, (0f, Gfx.C4(Color, 0.7f * a)), (0.6f, Gfx.C4(Color, 0.4f * a)), (1f, Gfx.C4(Color, 0f))))
                        dc.FillEllipse(new Ellipse(p, rx, rx), g);
                    dc.Transform = prev;
                    if (Style == BlushStyle.Lines)
                    {
                        brush.Color = Gfx.C4(Gfx.Mix(Color, WpfColor.FromRgb(0x80, 0, 0x20), 0.35f), 0.9f * a);
                        for (int i = 0; i < 4; i++)
                        {
                            float x = (i - 1.5f) * rx * 0.38f;
                            dc.DrawLine(p + new Vector2(x + rx * 0.12f, -ry * 0.4f), p + new Vector2(x - rx * 0.12f, ry * 0.4f), brush, MathF.Max(2f, s * 0.05f));
                        }
                    }
                    break;
                }
            }
        }
        if (Steam)
        {
            // 頭の上から「ぽっ」と湯気(2つのほおのまん中の上)
            var mid = (new Vector2(c.F(LeftX) / 100f * w, c.F(LeftY) / 100f * h) + new Vector2(c.F(RightX) / 100f * w, c.F(RightY) / 100f * h)) / 2f;
            for (int i = 0; i < 3; i++)
            {
                float f = DrawKit.Frac(t * 0.8f + i / 3f);
                var sp = mid + new Vector2((i - 1) * s * 0.8f + MathF.Sin(f * 6f) * s * 0.1f, -s * 3.2f - f * s * 1.5f);
                brush.Color = Gfx.C4(1f, 1f, 1f, (1f - f) * 0.8f * k);
                var cloud = BubbleKit.CloudPts(sp, s * 0.3f * (0.6f + f * 0.6f), s * 0.2f * (0.6f + f * 0.6f), 5, 0.2f);
                Gfx.FillPoly(dc, brush, cloud);
            }
        }
    }
}
