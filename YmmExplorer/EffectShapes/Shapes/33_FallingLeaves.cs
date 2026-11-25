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
//  落ち葉
//   振り子のように揺れながら、くるくる回って落ちる秋の葉
// =====================================================================
public sealed class FallingLeavesPlugin : IShapePlugin
{
    public string Name => "演出：落ち葉";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FallingLeavesParameter(sharedData);
}

public sealed class FallingLeavesParameter : EffectShapeParameterBase
{
    public FallingLeavesParameter() : this(null) { }
    public FallingLeavesParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "葉", Name = "葉の種類")]
    [EnumComboBox]
    public LeafKind Kind { get => kind; set => Set(ref kind, value); }
    LeafKind kind = LeafKind.Mixed;

    [Display(GroupName = "葉", Name = "色1（赤）")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0xD8, 0x3A, 0x1E);

    [Display(GroupName = "葉", Name = "色2（橙）")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0xF0, 0x8A, 0x1A);

    [Display(GroupName = "葉", Name = "色3（黄）")]
    [ColorPicker]
    public WpfColor Color3 { get => c3; set => Set(ref c3, value); }
    WpfColor c3 = WpfColor.FromRgb(0xF2, 0xC8, 0x2A);

    [Display(GroupName = "葉", Name = "数")]
    [AnimationSlider("F0", "枚", 0, 300)]
    public Animation Count { get; } = new Animation(50, 0, 3000);

    [Display(GroupName = "葉", Name = "大きさ")]
    [AnimationSlider("F0", "px", 10, 80)]
    public Animation Size { get; } = new Animation(32, 3, 800);

    [Display(GroupName = "動き", Name = "落ちる速さ")]
    [AnimationSlider("F0", "px/s", 20, 400)]
    public Animation Fall { get; } = new Animation(110, 1, 5000);

    [Display(GroupName = "動き", Name = "風（横方向）")]
    [AnimationSlider("F0", "px/s", -400, 400)]
    public Animation Wind { get; } = new Animation(60, -8000, 8000);

    [Display(GroupName = "動き", Name = "揺れ幅")]
    [AnimationSlider("F0", "px", 0, 300)]
    public Animation Sway { get; } = new Animation(110, 0, 3000);

    [Display(GroupName = "動き", Name = "くるくる回る")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Spin { get; } = new Animation(100, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Fall, Wind, Sway, Spin];

    static List<Vector2> LeafPoly(LeafKind k, float r)
    {
        var pts = new List<Vector2>();
        switch (k)
        {
            case LeafKind.Maple:
                for (int i = 0; i < 90; i++)
                {
                    float a = i / 90f * MathF.PI * 2f;
                    float rr = r * (0.40f + 0.60f * MathF.Pow(MathF.Abs(MathF.Cos(2.5f * a)), 1.6f));
                    pts.Add(new Vector2(MathF.Cos(a) * rr, MathF.Sin(a) * rr));
                }
                break;
            case LeafKind.Ginkgo:
                for (int i = 0; i <= 30; i++)
                {
                    float a = -MathF.PI / 2f - 1.0f + i / 30f * 2.0f;
                    float rr = r * (1f - 0.12f * MathF.Pow(MathF.Cos((i / 30f - 0.5f) * 14f), 2f) * (i > 12 && i < 18 ? 2.5f : 0.4f));
                    pts.Add(new Vector2(MathF.Cos(a) * rr, MathF.Sin(a) * rr + r * 0.5f));
                }
                pts.Add(new Vector2(0f, r * 0.5f));
                break;
            default:    // 広葉
                for (int i = 0; i <= 16; i++)
                {
                    float x = -r + 2f * r * i / 16f;
                    pts.Add(new Vector2(x, -r * 0.55f * MathF.Pow(1f - (x / r) * (x / r), 0.85f)));
                }
                for (int i = 16; i >= 0; i--)
                {
                    float x = -r + 2f * r * i / 16f;
                    pts.Add(new Vector2(x, r * 0.55f * MathF.Pow(1f - (x / r) * (x / r), 0.85f)));
                }
                break;
        }
        return pts;
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float size = c.F(Size), fall = c.F(Fall), wind = c.F(Wind), sway = c.F(Sway), spin = c.F(Spin) / 100f;
        float margin = size * 3f, spanX = w + margin * 2f, spanY = h + margin * 2f;
        var palette = new[] { Color1, Color2, Color3, Gfx.Mix(Color1, Color2, 0.5f), WpfColor.FromRgb(0x8A, 0x5A, 0x2A) };

        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);
        try
        {
            for (int i = 0; i < n; i++)
            {
                float x0 = rng.NextF() * spanX, ph = rng.NextF();
                float d = 0.55f + 0.45f * rng.NextF();
                float fr = 0.6f + 0.8f * rng.NextF(), ph2 = rng.NextF() * MathF.PI * 2f;
                float rot0 = rng.NextF() * MathF.PI * 2f, fs = 1.5f + 2f * rng.NextF(), fp = rng.NextF() * MathF.PI * 2f;
                int colIdx = rng.Next(0, palette.Length);
                int kindSel = rng.Next(0, 3);

                float swing = MathF.Sin(t * fr + ph2);                           // 振り子の揺れ
                float y = -margin + Gfx.Mod(t * fall * d + ph * spanY, spanY);
                float x = Gfx.Mod(x0 + t * wind * d + swing * sway * d, spanX) - margin;
                float angle = rot0 + swing * 0.9f + t * 0.4f * spin;             // 揺れに合わせて傾く
                float flip = MathF.Cos(t * fs * spin + fp);
                float sy = MathF.Max(0.2f, MathF.Abs(flip));

                var k = Kind == LeafKind.Mixed ? (LeafKind)kindSel : Kind;
                float r = size * (0.6f + 0.4f * d);
                var col = Gfx.Mix(palette[colIdx], WpfColor.FromRgb(0, 0, 0), (1f - sy) * 0.3f);
                var ctr = new Vector2(x, y);

                dc.Transform = Matrix3x2.CreateScale(1f, sy, ctr) * Matrix3x2.CreateRotation(angle, ctr);
                var poly = LeafPoly(k, r).Select(p => p + ctr).ToList();
                brush.Color = Gfx.C4(col, 0.95f);
                Gfx.FillPoly(dc, brush, poly);
                // 葉脈と柄
                brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(0, 0, 0), 0.4f), 0.8f);
                dc.DrawLine(ctr + new Vector2(0f, k == LeafKind.Ginkgo ? r * 0.5f : r * 0.9f), ctr + new Vector2(0f, -r * 0.6f), brush, 1.2f);
                dc.DrawLine(ctr + new Vector2(-r * 0.2f, k == LeafKind.Broad ? 0f : r * 0.05f), ctr + new Vector2(r * 0.2f, k == LeafKind.Broad ? 0f : r * 0.05f), brush, 1f);
                dc.Transform = Matrix3x2.Identity;
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
