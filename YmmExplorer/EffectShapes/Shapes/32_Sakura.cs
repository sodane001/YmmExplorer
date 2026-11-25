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
//  桜（舞い落ちる花びら）
//   ひらひら回転しながら、風に流されて落ちる花びら。
//   奥行きで大きさ・速さ・濃さが変わる
// =====================================================================
public sealed class SakuraPlugin : IShapePlugin
{
    public string Name => "演出：桜（花びら）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SakuraParameter(sharedData);
}

public sealed class SakuraParameter : EffectShapeParameterBase
{
    public SakuraParameter() : this(null) { }
    public SakuraParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "花びら", Name = "色")]
    [ColorPicker]
    public WpfColor PetalColor { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xB7, 0xC5);

    [Display(GroupName = "花びら", Name = "色のばらつき", Description = "白っぽい花びら・濃いピンクの花びらが混ざる割合")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Variation { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "花びら", Name = "数")]
    [AnimationSlider("F0", "枚", 0, 500)]
    public Animation Count { get; } = new Animation(120, 0, 5000);

    [Display(GroupName = "花びら", Name = "サイズ")]
    [AnimationSlider("F1", "px", 4, 60)]
    public Animation Size { get; } = new Animation(14, 1, 500);

    [Display(GroupName = "花びら", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(95, 0, 100);

    [Display(GroupName = "動き", Name = "落下速度")]
    [AnimationSlider("F0", "px/s", 10, 400)]
    public Animation Fall { get; } = new Animation(90, 1, 5000);

    [Display(GroupName = "動き", Name = "風（横方向の流れ）", Description = "プラスで右へ、マイナスで左へ流れる")]
    [AnimationSlider("F0", "px/s", -300, 300)]
    public Animation Wind { get; } = new Animation(60, -5000, 5000);

    [Display(GroupName = "動き", Name = "ゆらゆら揺れ幅")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Sway { get; } = new Animation(50, 0, 2000);

    [Display(GroupName = "動き", Name = "ひらひら回転")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Spin { get; } = new Animation(100, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Variation, Count, Size, Opacity, Fall, Wind, Sway, Spin];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), op = c.F(Opacity) / 100f;
        float fall = c.F(Fall), wind = c.F(Wind), sway = c.F(Sway), spin = c.F(Spin) / 100f;
        float vari = c.F(Variation) / 100f;

        var white = WpfColor.FromRgb(255, 255, 255);
        var deep = WpfColor.FromRgb(0xE8, 0x70, 0x8F);

        float margin = size * 3f;
        float spanX = w + margin * 2f, spanY = h + margin * 2f;

        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);
        try
        {
            for (int i = 0; i < n; i++)
            {
                float x0 = rng.NextF() * spanX;
                float ph = rng.NextF();
                float d = 0.4f + 0.6f * rng.NextF();                 // 奥行き（1が手前）
                float fr = 0.4f + 0.8f * rng.NextF();
                float ph2 = rng.NextF() * MathF.PI * 2f;
                float rot0 = rng.NextF() * MathF.PI * 2f;
                float rs = (0.5f + rng.NextF()) * (rng.NextF() < 0.5f ? -1f : 1f);
                float flipPhase = rng.NextF() * MathF.PI * 2f;
                float flipSpeed = 1.5f + 2f * rng.NextF();
                float tint = rng.NextF(), deepSel = rng.NextF();

                float r = size * (0.5f + 0.5f * d);
                float y = -margin + Gfx.Mod(t * fall * d + ph * spanY, spanY);
                float x = Gfx.Mod(x0 + t * wind * d + MathF.Sin(t * fr + ph2) * sway * d, spanX) - margin;

                // 花びらが表裏にひっくり返る動き（横幅を縮める）
                float flip = MathF.Cos(t * flipSpeed * spin + flipPhase);
                float sx = MathF.Max(0.18f, MathF.Abs(flip));
                float angle = rot0 + MathF.Sin(t * fr * 1.3f + ph2) * 0.9f + t * rs * 0.5f * spin;

                var baseCol = Gfx.Mix(Gfx.Mix(PetalColor, white, tint * vari), deep, deepSel * vari * 0.5f);
                var col = Gfx.Mix(baseCol, deep, (1f - MathF.Abs(flip)) * 0.35f);   // 傾くほど少し濃い
                var hi = Gfx.Mix(baseCol, white, 0.55f);
                float a = op * (0.55f + 0.45f * d);

                var ctr = new Vector2(x, y);
                dc.Transform = Matrix3x2.CreateScale(sx, 1f, ctr) * Matrix3x2.CreateRotation(angle, ctr);

                // 花びらの本体（先端が2つに割れたハート型）
                brush.Color = Gfx.C4(col, a * 0.92f);
                dc.FillEllipse(new Ellipse(ctr + new Vector2(0f, r * 0.15f), r * 0.45f, r * 0.80f), brush);
                dc.FillEllipse(new Ellipse(ctr + new Vector2(-r * 0.22f, -r * 0.45f), r * 0.36f, r * 0.40f), brush);
                dc.FillEllipse(new Ellipse(ctr + new Vector2(r * 0.22f, -r * 0.45f), r * 0.36f, r * 0.40f), brush);

                // 中央のやわらかい明るみ
                brush.Color = Gfx.C4(hi, a * 0.45f);
                dc.FillEllipse(new Ellipse(ctr + new Vector2(0f, -r * 0.05f), r * 0.26f, r * 0.50f), brush);

                // 先端の切れ込みと付け根
                brush.Color = Gfx.C4(deep, a * 0.55f);
                dc.FillEllipse(new Ellipse(ctr + new Vector2(0f, -r * 0.80f), r * 0.07f, r * 0.12f), brush);
                dc.FillEllipse(new Ellipse(ctr + new Vector2(0f, r * 0.82f), r * 0.07f, r * 0.07f), brush);
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
