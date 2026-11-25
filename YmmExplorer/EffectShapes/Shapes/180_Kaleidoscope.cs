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
//  万華鏡
//   いくつかの形を、鏡のように折り返して並べた模様が、ゆっくり回り・変わり続ける
// =====================================================================
public sealed class KaleidoscopePlugin : IShapePlugin
{
    public string Name => "演出：万華鏡";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new KaleidoscopeParameter(sharedData);
}

public sealed class KaleidoscopeParameter : EffectShapeParameterBase
{
    public KaleidoscopeParameter() : this(null) { }
    public KaleidoscopeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "模様", Name = "折り返しの数")]
    [TextBoxSlider("F0", "", 3, 16)]
    [Range(2, 40)]
    public int Segments { get => segments; set => Set(ref segments, value); }
    int segments = 8;

    [Display(GroupName = "模様", Name = "形の数")]
    [TextBoxSlider("F0", "個", 3, 30)]
    [Range(1, 120)]
    public int Shapes { get => shapes; set => Set(ref shapes, value); }
    int shapes = 12;

    [Display(GroupName = "模様", Name = "回る速さ")]
    [AnimationSlider("F0", "°/秒", -60, 60)]
    public Animation Spin { get; } = new Animation(8, -3600, 3600);

    [Display(GroupName = "模様", Name = "模様が変わる速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Morph { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "色", Name = "色の組み合わせ")]
    [EnumComboBox]
    public ChartPalette Palette { get => palette; set => Set(ref palette, value); }
    ChartPalette palette = ChartPalette.Vivid;

    [Display(GroupName = "色", Name = "背景の色")]
    [ColorPicker]
    public WpfColor BackColor { get => backColor; set => Set(ref backColor, value); }
    WpfColor backColor = WpfColor.FromRgb(0x0A, 0x08, 0x18);

    [Display(GroupName = "色", Name = "光らせる（加算）")]
    [ToggleSlider]
    public bool Additive { get => additive; set => Set(ref additive, value); }
    bool additive = true;

    [Display(GroupName = "位置", Name = "中心X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CenterX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "中心Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CenterY { get; } = new Animation(50, -100, 200);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Spin, Morph, CenterX, CenterY];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(CenterX) / 100f * w, c.F(CenterY) / 100f * h);
        float R = MathF.Sqrt(w * w + h * h) * 0.6f;
        int segs = Math.Max(2, Segments);
        float wedge = MathF.PI * 2f / segs;
        float rot = Gfx.Rad(c.F(Spin)) * t;
        float mt = t * c.F(Morph) / 100f;
        using var brush = dc.CreateSolidColorBrush(default);
        brush.Color = Gfx.C4(BackColor, 1f);
        dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
        if (Additive) dc.PrimitiveBlend = PrimitiveBlend.Add;

        // 1つの扇形の中の形(極座標: 半径 r, 角度 a は 0〜wedge/2)を作り、鏡で並べる
        for (int i = 0; i < Shapes; i++)
        {
            float r = R * (0.05f + DrawKit.H(Seed, i, 1) * 0.85f) * (0.85f + 0.15f * MathF.Sin(mt * 0.7f + i));
            float a = wedge * 0.5f * Gfx.Mod(DrawKit.H(Seed, i, 2) + mt * 0.05f * (DrawKit.H(Seed, i, 3) - 0.5f), 1f);
            float size = R * (0.03f + DrawKit.H(Seed, i, 4) * 0.08f) * (0.7f + 0.3f * MathF.Sin(mt * 1.3f + i * 2f));
            int kind = (int)(DrawKit.H(Seed, i, 5) * 4f);
            float selfRot = mt * (DrawKit.H(Seed, i, 6) - 0.5f) * 2f;
            var col = Gfx.C4(BubbleKit.Palette((int)Palette, i), Additive ? 0.45f : 0.85f);
            brush.Color = col;
            for (int s = 0; s < segs; s++)
            {
                for (int m = 0; m < 2; m++)
                {
                    // 鏡: 偶数は a、奇数は -a(扇の中で折り返す)
                    float ang = rot + s * wedge + (m == 0 ? a : -a);
                    var p = ctr + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * r;
                    float pr = ang + (m == 0 ? selfRot : -selfRot);
                    switch (kind)
                    {
                        case 0: dc.FillEllipse(new Ellipse(p, size, size), brush); break;
                        case 1: Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(p, size * 1.3f, 5, 0.45f, pr)); break;
                        case 2:
                            Gfx.FillPoly(dc, brush, DrawKit.Place(new[] { new Vector2(0f, -1f), new Vector2(0.5f, 0f), new Vector2(0f, 1f), new Vector2(-0.5f, 0f) }, p, pr, size * 1.2f, size * 1.2f));
                            break;
                        default:
                            Gfx.FillPoly(dc, brush, DrawKit.Place(new[] { new Vector2(0f, -1f), new Vector2(0.87f, 0.5f), new Vector2(-0.87f, 0.5f) }, p, pr, size, size));
                            break;
                    }
                }
            }
        }
        // 中心の光と、鏡の境目の細い線
        DrawKit.Glow(dc, ctr, R * 0.25f, Gfx.C4(1f, 1f, 1f, 0.3f));
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.06f);
        for (int s = 0; s < segs; s++)
        {
            float ang = rot + s * wedge;
            dc.DrawLine(ctr, ctr + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * R, brush, 1.5f);
        }
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
    }
}
