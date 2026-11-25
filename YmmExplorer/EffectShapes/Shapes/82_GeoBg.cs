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
//  幾何学模様の背景
//   9種類のパターンが流れる・脈打つ。2色で使えるシンプルな背景
// =====================================================================
public sealed class GeoBgPlugin : IShapePlugin
{
    public string Name => "演出：幾何学模様の背景";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GeoBgParameter(sharedData);
}

public sealed class GeoBgParameter : EffectShapeParameterBase
{
    public GeoBgParameter() : this(null) { }
    public GeoBgParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "模様", Name = "パターン")]
    [EnumComboBox]
    public GeoPattern Pattern { get => pattern; set => Set(ref pattern, value); }
    GeoPattern pattern = GeoPattern.Dots;

    [Display(GroupName = "色", Name = "背景の色")]
    [ColorPicker]
    public WpfColor BgColor { get => bg; set => Set(ref bg, value); }
    WpfColor bg = WpfColor.FromRgb(0x14, 0x1E, 0x3C);

    [Display(GroupName = "色", Name = "背景の不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BgOpacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "色", Name = "模様の色")]
    [ColorPicker]
    public WpfColor PatternColor { get => pat; set => Set(ref pat, value); }
    WpfColor pat = WpfColor.FromRgb(0x4A, 0x6C, 0xFF);

    [Display(GroupName = "色", Name = "模様の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "模様", Name = "模様の大きさ")]
    [AnimationSlider("F0", "px", 12, 200)]
    public Animation Size { get; } = new Animation(54, 6, 1000);

    [Display(GroupName = "模様", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 20)]
    public Animation Thickness { get; } = new Animation(2.5, 0.2, 200);

    [Display(GroupName = "動き", Name = "流れる速さ")]
    [AnimationSlider("F0", "px/s", -300, 300)]
    public Animation Scroll { get; } = new Animation(30, -5000, 5000);

    [Display(GroupName = "動き", Name = "流れる向き")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Direction { get; } = new Animation(45, -3600, 3600);

    [Display(GroupName = "動き", Name = "脈打つ（拡大縮小・明滅）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Pulse { get; } = new Animation(30, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [BgOpacity, Strength, Size, Thickness, Scroll, Direction, Pulse];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float S = MathF.Max(6f, c.F(Size)), th = c.F(Thickness), k = c.F(Strength) / 100f, pulse = c.F(Pulse) / 100f;
        float ang = Gfx.Rad(c.F(Direction));
        var flow = new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * (c.F(Scroll) * t);
        float breath = 1f + 0.12f * pulse * MathF.Sin(t * 1.6f);
        float al = k * (1f - 0.25f * pulse + 0.25f * pulse * MathF.Sin(t * 1.6f + 1f));

        using var brush = dc.CreateSolidColorBrush(default);
        brush.Color = Gfx.C4(BgColor, c.F(BgOpacity) / 100f);
        Gfx.Rect(dc, brush, 0, 0, w, h);
        brush.Color = Gfx.C4(PatternColor, al);

        float ox = Gfx.Mod(flow.X, S), oy = Gfx.Mod(flow.Y, S);

        switch (Pattern)
        {
            case GeoPattern.Dots:
                {
                    for (float y = oy - S; y < h + S; y += S)
                        for (float x = ox - S; x < w + S; x += S)
                        {
                            float wave = 0.65f + 0.35f * MathF.Sin((x + y) * 0.01f + t * 1.4f);
                            float r = S * 0.16f * breath * (1f + pulse * (wave - 0.65f) * 2f);
                            dc.FillEllipse(new Ellipse(new Vector2(x, y), r, r), brush);
                        }
                    break;
                }
            case GeoPattern.Stripes:
                {
                    float diag = w + h, period = S * 1.6f;
                    float off = Gfx.Mod(flow.X + flow.Y, period);
                    for (float d = -h - period + off; d < w + period; d += period)
                        dc.DrawLine(new Vector2(d, 0f), new Vector2(d + h, h), brush, S * 0.3f * breath);
                    break;
                }
            case GeoPattern.Hex:
                {
                    float r = S * 0.62f * breath, dx = S * 1.5f * 0.88f, dy = S * 0.88f;
                    int row = 0;
                    for (float y = Gfx.Mod(flow.Y, dy * 2f) - dy * 2f; y < h + dy; y += dy, row++)
                        for (float x = Gfx.Mod(flow.X, dx * 2f) - dx * 2f + ((row % 2) * dx); x < w + dx; x += dx * 2f)
                            Gfx.Polyline(dc, brush, ShapeGeom.Closed(ShapeGeom.StarPoly(new Vector2(x, y), r, 6, 1f, MathF.PI / 6f)), th);
                    break;
                }
            case GeoPattern.Triangles:
                {
                    float hh = S * 0.866f;
                    // 3方向の平行線を引くと正三角形のタイルになる
                    for (float y = Gfx.Mod(flow.Y, hh) - hh; y < h + hh; y += hh)
                        dc.DrawLine(new Vector2(0, y), new Vector2(w, y), brush, th);
                    float diagLen = h / 0.866f;
                    for (float x = Gfx.Mod(flow.X, S) - S - h * 0.577f; x < w + h * 0.577f + S; x += S)
                    {
                        dc.DrawLine(new Vector2(x, 0), new Vector2(x + h * 0.577f, h), brush, th);
                        dc.DrawLine(new Vector2(x, 0), new Vector2(x - h * 0.577f, h), brush, th);
                    }
                    break;
                }
            case GeoPattern.Checker:
                {
                    int ix = 0;
                    for (float x = ox - S; x < w + S; x += S, ix++)
                    {
                        int iy = 0;
                        for (float y = oy - S; y < h + S; y += S, iy++)
                            if (((ix + iy) & 1) == 0) Gfx.Rect(dc, brush, x, y, S, S);
                    }
                    break;
                }
            case GeoPattern.Rings:
                {
                    var ctr = new Vector2(w / 2f, h / 2f);
                    float maxR = MathF.Sqrt(w * w + h * h) / 2f;
                    float off = Gfx.Mod(c.F(Scroll) * t, S);
                    for (float r = off; r < maxR; r += S)
                        dc.DrawEllipse(new Ellipse(ctr, r * breath, r * breath), brush, th);
                    break;
                }
            case GeoPattern.WaveLines:
                {
                    for (float y0 = oy - S; y0 < h + S; y0 += S * 0.7f)
                    {
                        Vector2 prev = default;
                        bool first = true;
                        for (float x = -20f; x <= w + 20f; x += 20f)
                        {
                            float y = y0 + MathF.Sin((x - flow.X) / S * 1.3f + t * 0.8f) * S * 0.28f * breath;
                            var cur = new Vector2(x, y);
                            if (!first) dc.DrawLine(prev, cur, brush, th);
                            prev = cur;
                            first = false;
                        }
                    }
                    break;
                }
            case GeoPattern.Plus:
                {
                    float arm = S * 0.2f * breath;
                    for (float y = oy - S; y < h + S; y += S)
                        for (float x = ox - S; x < w + S; x += S)
                        {
                            dc.DrawLine(new Vector2(x - arm, y), new Vector2(x + arm, y), brush, th);
                            dc.DrawLine(new Vector2(x, y - arm), new Vector2(x, y + arm), brush, th);
                        }
                    break;
                }
            default:    // Diamonds
                {
                    float period = S;
                    float off1 = Gfx.Mod(flow.X + flow.Y, period), off2 = Gfx.Mod(flow.X - flow.Y, period);
                    for (float d = -h - period + off1; d < w + period; d += period)
                        dc.DrawLine(new Vector2(d, 0f), new Vector2(d + h, h), brush, th);
                    for (float d = -period + off2; d < w + h + period; d += period)
                        dc.DrawLine(new Vector2(d, 0f), new Vector2(d - h, h), brush, th);
                    break;
                }
        }
    }
}
