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

public enum TearKind
{
    [Display(Name = "ひとすじの涙")] Single,
    [Display(Name = "号泣（滝のような涙）")] Stream,
    [Display(Name = "うるうる（目がうるむ）")] Teary,
}

// =====================================================================
//  涙・号泣
//   目の位置(左・右)から、涙がつたう・滝のように流れる・目がうるむ
// =====================================================================
public sealed class TearsPlugin : IShapePlugin
{
    public string Name => "演出：涙・号泣";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TearsParameter(sharedData);
}

public sealed class TearsParameter : EffectShapeParameterBase
{
    public TearsParameter() : this(null) { }
    public TearsParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "種類")]
    [EnumComboBox]
    public TearKind Kind { get => kind; set => Set(ref kind, value); }
    TearKind kind = TearKind.Stream;

    [Display(GroupName = "形", Name = "涙の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x7F, 0xC8, 0xFF);

    [Display(GroupName = "形", Name = "大きさ")]
    [AnimationSlider("F0", "px", 10, 200)]
    public Animation Size { get; } = new Animation(46, 2, 2000);

    [Display(GroupName = "形", Name = "涙の長さ（号泣）")]
    [AnimationSlider("F0", "px", 50, 800)]
    public Animation StreamLength { get; } = new Animation(320, 1, 5000);

    [Display(GroupName = "位置", Name = "左目X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LeftX { get; } = new Animation(44, -100, 200);

    [Display(GroupName = "位置", Name = "左目Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LeftY { get; } = new Animation(40, -100, 200);

    [Display(GroupName = "位置", Name = "右目X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RightX { get; } = new Animation(56, -100, 200);

    [Display(GroupName = "位置", Name = "右目Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RightY { get; } = new Animation(40, -100, 200);

    [Display(GroupName = "位置", Name = "右目にも出す")]
    [ToggleSlider]
    public bool BothEyes { get => bothEyes; set => Set(ref bothEyes, value); }
    bool bothEyes = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Size, StreamLength, LeftX, LeftY, RightX, RightY];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.2f, 0.25f);
        if (k <= 0.001f) return;
        float s = c.F(Size), len = c.F(StreamLength);
        var eyes = new List<(Vector2 p, float side)> { (new Vector2(c.F(LeftX) / 100f * w, c.F(LeftY) / 100f * h), -1f) };
        if (BothEyes) eyes.Add((new Vector2(c.F(RightX) / 100f * w, c.F(RightY) / 100f * h), 1f));
        using var brush = dc.CreateSolidColorBrush(default);
        var col = Gfx.C4(Color, 0.85f * k);
        var hi = Gfx.C4(1f, 1f, 1f, 0.8f * k);

        foreach (var (eye, side) in eyes)
        {
            switch (Kind)
            {
                case TearKind.Single:
                {
                    // 目尻からしずくが出て、ほおをゆっくりつたう(くり返し)
                    float f = DrawKit.Frac(t * 0.35f + (side > 0 ? 0.4f : 0f));
                    var p = eye + new Vector2(side * s * 0.6f, s * 0.3f + f * s * 4f);
                    // つたった跡
                    brush.Color = Gfx.C4(Color, 0.35f * k);
                    dc.DrawLine(eye + new Vector2(side * s * 0.6f, s * 0.3f), p, brush, s * 0.12f);
                    var drop = new List<Vector2> { p + new Vector2(0f, -s * 0.55f) };
                    drop.AddRange(Gfx.ArcPts(p, s * 0.28f, -MathF.PI * 0.15f, MathF.PI * 1.15f, 14));
                    brush.Color = col;
                    Gfx.FillPoly(dc, brush, drop);
                    brush.Color = hi;
                    dc.FillEllipse(new Ellipse(p + new Vector2(-s * 0.08f, -s * 0.05f), s * 0.06f, s * 0.09f), brush);
                    break;
                }
                case TearKind.Stream:
                {
                    // 目から外へ噴き出して、弧を描いて落ちる涙の帯(波打つ)
                    var pts = new List<Vector2>();
                    for (int i = 0; i <= 24; i++)
                    {
                        float f = i / 24f;
                        float x = side * (s * 0.3f + f * len * 0.35f);
                        float y = -s * 0.1f + f * f * len;
                        float wave = MathF.Sin(f * 10f - t * 14f) * s * 0.08f;
                        pts.Add(eye + new Vector2(x + wave, y));
                    }
                    float grow = DrawKit.EaseOut(Math.Clamp(t / 0.35f, 0f, 1f));
                    var part = BubbleKit.PartByLength(pts, grow);
                    for (int i = 0; i + 1 < part.Count; i++)
                    {
                        float f = i / (float)Math.Max(1, part.Count - 1);
                        float ww = s * (0.55f + f * 0.45f);
                        brush.Color = col;
                        dc.DrawLine(part[i], part[i + 1], brush, ww);
                        dc.FillEllipse(new Ellipse(part[i + 1], ww / 2f, ww / 2f), brush);
                    }
                    // 流れの中の白い筋
                    brush.Color = hi;
                    for (int i = 0; i + 1 < part.Count; i += 2)
                    {
                        float ph = DrawKit.Frac(i * 0.13f - t * 2.5f);
                        if (ph < 0.5f) dc.DrawLine(part[i], part[i + 1], brush, s * 0.12f);
                    }
                    // 下で飛び散る、しずく
                    if (grow >= 1f)
                    {
                        var end = pts[^1];
                        for (int j = 0; j < 7; j++)
                        {
                            float f = DrawKit.Frac(t * 1.6f + j / 7f);
                            float ang = -MathF.PI / 2f + (DrawKit.H(Seed, j) - 0.5f) * 2.2f;
                            var d = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                            var dp = end + d * s * 2.5f * f + new Vector2(0f, s * 6f * f * f);
                            brush.Color = Gfx.C4(Color, 0.8f * k * (1f - f));
                            dc.FillEllipse(new Ellipse(dp, s * 0.16f, s * 0.2f), brush);
                        }
                    }
                    break;
                }
                default:
                {
                    // うるうる: 目の下に水がたまり、光がゆらぐ
                    float wob = MathF.Sin(t * 3f) * s * 0.05f;
                    brush.Color = Gfx.C4(Color, 0.55f * k);
                    dc.FillEllipse(new Ellipse(eye + new Vector2(0f, s * 0.35f + wob), s * 0.9f, s * 0.3f), brush);
                    brush.Color = hi;
                    dc.FillEllipse(new Ellipse(eye + new Vector2(-s * 0.3f, s * 0.28f + wob), s * 0.18f, s * 0.07f), brush);
                    for (int j = 0; j < 3; j++)
                    {
                        float tw = 0.5f + 0.5f * MathF.Sin(t * 5f + j * 2f);
                        var sp = eye + new Vector2((j - 1) * s * 0.45f, -s * 0.15f + j % 2 * s * 0.2f);
                        brush.Color = Gfx.C4(1f, 1f, 1f, tw * k);
                        Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(sp, s * 0.22f * tw, 4, 0.25f, 0f));
                    }
                    break;
                }
            }
        }
    }
}
