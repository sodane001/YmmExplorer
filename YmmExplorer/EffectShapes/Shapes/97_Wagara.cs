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

public enum WagaraPattern
{
    [Display(Name = "青海波（せいがいは）")] Seigaiha,
    [Display(Name = "麻の葉（あさのは）")] Asanoha,
    [Display(Name = "七宝（しっぽう）")] Shippo,
    [Display(Name = "市松（いちまつ）")] Ichimatsu,
    [Display(Name = "矢絣（やがすり）")] Yagasuri,
    [Display(Name = "亀甲（きっこう）")] Kikko,
}

// =====================================================================
//  和柄の背景
//   青海波、麻の葉、七宝、市松などのくり返し模様
// =====================================================================
public sealed class WagaraPlugin : IShapePlugin
{
    public string Name => "演出：和柄の背景";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WagaraParameter(sharedData);
}

public sealed class WagaraParameter : EffectShapeParameterBase
{
    public WagaraParameter() : this(null) { }
    public WagaraParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "柄", Name = "柄")]
    [EnumComboBox]
    public WagaraPattern Pattern { get => pattern; set => Set(ref pattern, value); }
    WagaraPattern pattern = WagaraPattern.Seigaiha;

    [Display(GroupName = "柄", Name = "柄の大きさ")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation CellSize { get; } = new Animation(80, 4, 3000);

    [Display(GroupName = "柄", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 10)]
    public Animation LineWidth { get; } = new Animation(2.5, 0.1, 100);

    [Display(GroupName = "色", Name = "地の色")]
    [ColorPicker]
    public WpfColor BgColor { get => bgColor; set => Set(ref bgColor, value); }
    WpfColor bgColor = WpfColor.FromRgb(0x1E, 0x3A, 0x6E);

    [Display(GroupName = "色", Name = "柄の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(0xF0, 0xE6, 0xD2);

    [Display(GroupName = "色", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "色", Name = "地を塗る")]
    [ToggleSlider]
    public bool FillBg { get => fillBg; set => Set(ref fillBg, value); }
    bool fillBg = true;

    [Display(GroupName = "動き", Name = "横に流れる速さ")]
    [AnimationSlider("F0", "px/秒", -200, 200)]
    public Animation ScrollX { get; } = new Animation(0, -5000, 5000);

    [Display(GroupName = "動き", Name = "縦に流れる速さ")]
    [AnimationSlider("F0", "px/秒", -200, 200)]
    public Animation ScrollY { get; } = new Animation(0, -5000, 5000);

    [Display(GroupName = "動き", Name = "光の反射を流す")]
    [ToggleSlider]
    public bool Shimmer { get => shimmer; set => Set(ref shimmer, value); }
    bool shimmer = false;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [CellSize, LineWidth, Opacity, ScrollX, ScrollY];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float s = MathF.Max(4f, c.F(CellSize)), lw = c.F(LineWidth), op = c.F(Opacity) / 100f;
        float sx = c.F(ScrollX) * t, sy = c.F(ScrollY) * t;
        var bgc = Gfx.C4(BgColor, op);
        var fg = Gfx.C4(LineColor, op);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (FillBg)
            {
                brush.Color = bgc;
                dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            }
            brush.Color = fg;

            switch (Pattern)
            {
                case WagaraPattern.Seigaiha:
                    {
                        float R = s;
                        float ox = Gfx.Mod(sx, 2f * R), oy = Gfx.Mod(sy, R);
                        int rows = (int)MathF.Ceiling(h / (R * 0.5f)) + 6, cols = (int)MathF.Ceiling(w / (2f * R)) + 3;
                        for (int j = -4; j < rows; j++)
                        {
                            float y = j * R * 0.5f + oy;
                            for (int i = -2; i < cols; i++)
                            {
                                float x = i * 2f * R + ((j & 1) == 1 ? R : 0f) + ox;
                                var p = new Vector2(x, y);
                                if (FillBg)
                                {
                                    brush.Color = bgc;
                                    dc.FillEllipse(new Ellipse(p, R - lw * 0.5f, R - lw * 0.5f), brush);
                                }
                                brush.Color = fg;
                                for (int k = 0; k < 4; k++)
                                {
                                    float rr = R * (1f - k * 0.25f) - lw * 0.5f;
                                    if (rr > lw) dc.DrawEllipse(new Ellipse(p, rr, rr), brush, lw);
                                }
                            }
                        }
                        break;
                    }
                case WagaraPattern.Asanoha:
                    {
                        float hgt = s * MathF.Sqrt(3f) / 2f;
                        float ox = Gfx.Mod(sx, s), oy = Gfx.Mod(sy, hgt * 2f);
                        int rows = (int)MathF.Ceiling(h / hgt) + 4;
                        Vector2 V(int i, int j) => new(i * s + j * s / 2f + ox, j * hgt + oy);
                        for (int j = -2; j < rows; j++)
                        {
                            int i0 = (int)MathF.Floor(-j / 2f) - 3, i1 = i0 + (int)MathF.Ceiling(w / s) + 6;
                            for (int i = i0; i < i1; i++)
                            {
                                // 下向きと上向きの三角形
                                for (int up = 0; up < 2; up++)
                                {
                                    var a = up == 0 ? V(i, j) : V(i + 1, j);
                                    var b = up == 0 ? V(i + 1, j) : V(i, j + 1);
                                    var cc = up == 0 ? V(i, j + 1) : V(i + 1, j + 1);
                                    var g = (a + b + cc) / 3f;
                                    if (g.X < -s || g.X > w + s || g.Y < -s || g.Y > h + s) continue;
                                    dc.DrawLine(a, b, brush, lw);
                                    dc.DrawLine(b, cc, brush, lw);
                                    dc.DrawLine(cc, a, brush, lw);
                                    dc.DrawLine(g, a, brush, lw * 0.8f);
                                    dc.DrawLine(g, b, brush, lw * 0.8f);
                                    dc.DrawLine(g, cc, brush, lw * 0.8f);
                                }
                            }
                        }
                        break;
                    }
                case WagaraPattern.Shippo:
                    {
                        float r = s / MathF.Sqrt(2f);
                        float ox = Gfx.Mod(sx, s), oy = Gfx.Mod(sy, s);
                        for (int j = -2; j < h / s + 3; j++)
                            for (int i = -2; i < w / s + 3; i++)
                            {
                                var p = new Vector2(i * s + ox, j * s + oy);
                                dc.DrawEllipse(new Ellipse(p, r, r), brush, lw);
                                // 花の中心の小さな菱形
                                var q = p + new Vector2(s / 2f, s / 2f);
                                float d = s * 0.08f;
                                Gfx.FillPoly(dc, brush, new List<Vector2> { q + new Vector2(0, -d), q + new Vector2(d, 0), q + new Vector2(0, d), q + new Vector2(-d, 0) });
                            }
                        break;
                    }
                case WagaraPattern.Ichimatsu:
                    {
                        float ox = Gfx.Mod(sx, 2f * s), oy = Gfx.Mod(sy, 2f * s);
                        for (int j = -2; j < h / s + 3; j++)
                            for (int i = -2; i < w / s + 3; i++)
                            {
                                if (((i + j) & 1) == 0) continue;
                                Gfx.Rect(dc, brush, i * s + ox, j * s + oy, s, s);
                            }
                        break;
                    }
                case WagaraPattern.Yagasuri:
                    {
                        float cw = s * 0.75f;
                        float ox = Gfx.Mod(sx, 2f * cw), oy = Gfx.Mod(sy, s);
                        for (int i = -2; i < w / cw + 3; i++)
                        {
                            float x = i * cw + ox;
                            bool odd = (i & 1) == 1;
                            // 列の地
                            if (odd) { brush.Color = fg; Gfx.Rect(dc, brush, x, 0, cw, h); }
                            brush.Color = odd ? bgc : fg;
                            float shift = odd ? s * 0.5f : 0f;
                            for (int j = -2; j < h / s + 3; j++)
                            {
                                float y = j * s + oy + shift;
                                var chev = new List<Vector2>
                                {
                                    new(x, y), new(x + cw / 2f, y + s * 0.3f), new(x + cw, y),
                                    new(x + cw, y + s * 0.5f), new(x + cw / 2f, y + s * 0.8f), new(x, y + s * 0.5f),
                                };
                                Gfx.FillPoly(dc, brush, chev, 0f, h);
                            }
                        }
                        break;
                    }
                case WagaraPattern.Kikko:
                    {
                        float r = s / 2f;
                        float dx = r * 1.5f, dy = r * MathF.Sqrt(3f);
                        float ox = Gfx.Mod(sx, dx * 2f), oy = Gfx.Mod(sy, dy);
                        for (int i = -2; i < w / dx + 3; i++)
                            for (int j = -2; j < h / dy + 3; j++)
                            {
                                var p = new Vector2(i * dx + ox, j * dy + ((i & 1) == 1 ? dy / 2f : 0f) + oy);
                                var hex = new List<Vector2>();
                                var inner = new List<Vector2>();
                                for (int k = 0; k <= 6; k++)
                                {
                                    float a = k * MathF.PI / 3f;
                                    hex.Add(p + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r);
                                    inner.Add(p + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r * 0.72f);
                                }
                                for (int k = 0; k < 6; k++)
                                {
                                    dc.DrawLine(hex[k], hex[k + 1], brush, lw);
                                    dc.DrawLine(inner[k], inner[k + 1], brush, lw * 0.7f);
                                }
                            }
                        break;
                    }
            }

            // ---- 光の反射 ----
            if (Shimmer)
            {
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                float u = DrawKit.Frac(t * 0.15f);
                float bx = -w * 0.5f + u * w * 2f;
                using var lg = Gfx.Linear(dc, new Vector2(bx - w * 0.25f, 0), new Vector2(bx + w * 0.25f, h * 0.4f),
                    (0f, Gfx.C4(LineColor, 0f)), (0.5f, Gfx.C4(LineColor, 0.12f * op)), (1f, Gfx.C4(LineColor, 0f)));
                dc.FillRectangle(new RawRectF(0, 0, w, h), lg);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
