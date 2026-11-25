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

public enum EncircleKind
{
    [Display(Name = "丸で囲む")] Circle,
    [Display(Name = "四角で囲む")] Rect,
    [Display(Name = "下線")] Underline,
    [Display(Name = "二重線")] DoubleUnderline,
    [Display(Name = "波線")] Wavy,
    [Display(Name = "バツ")] Cross,
    [Display(Name = "チェック（レ点）")] Check,
    [Display(Name = "取り消し線")] Strike,
}

// =====================================================================
//  丸で囲む・下線を引く
//   手書き風に、ぐるっと囲んだり、線を引いたりする強調
// =====================================================================
public sealed class EncirclePlugin : IShapePlugin
{
    public string Name => "演出：丸で囲む・下線を引く";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new EncircleParameter(sharedData);
}

public sealed class EncircleParameter : EffectShapeParameterBase
{
    public EncircleParameter() : this(null) { }
    public EncircleParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "種類")]
    [EnumComboBox]
    public EncircleKind Kind { get => kind; set => Set(ref kind, value); }
    EncircleKind kind = EncircleKind.Circle;

    [Display(GroupName = "形", Name = "線の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x2D, 0x2D);

    [Display(GroupName = "形", Name = "線の太さ")]
    [AnimationSlider("F0", "px", 1, 40)]
    public Animation LineWidth { get; } = new Animation(10, 0.5, 400);

    [Display(GroupName = "形", Name = "手書きのゆれ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Rough { get; } = new Animation(40, 0, 300);

    [Display(GroupName = "形", Name = "蛍光ペン風（半透明で太く）")]
    [ToggleSlider]
    public bool Highlighter { get => highlighter; set => Set(ref highlighter, value); }
    bool highlighter = false;

    [Display(GroupName = "形", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "囲む幅")]
    [AnimationSlider("F0", "px", 20, 1600)]
    public Animation BoxWidth { get; } = new Animation(520, 1, 8000);

    [Display(GroupName = "位置と大きさ", Name = "囲む高さ")]
    [AnimationSlider("F0", "px", 20, 900)]
    public Animation BoxHeight { get; } = new Animation(200, 1, 8000);

    [Display(GroupName = "位置と大きさ", Name = "傾き")]
    [AnimationSlider("F0", "°", -45, 45)]
    public Animation Tilt { get; } = new Animation(-3, -360, 360);

    [Display(GroupName = "動き", Name = "描く時間")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation DrawTime { get; } = new Animation(0.6, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.2, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [LineWidth, Rough, Opacity, PosX, PosY, BoxWidth, BoxHeight, Tilt, DrawTime, OutTime];

    /// <summary>なめらかなゆれ(座標の並びに対して一定。毎フレーム変わらない)</summary>
    static float Noise(float x, int seed) => MathF.Sin(x * 1.3f + seed * 1.7f) * 0.6f + MathF.Sin(x * 2.9f + seed * 0.37f) * 0.3f + MathF.Sin(x * 5.3f + seed) * 0.1f;

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float drawT = c.F(DrawTime), outT = c.F(OutTime);
        float prog = drawT <= 0.001f ? 1f : DrawKit.EaseOut(t / drawT);
        float alpha = c.F(Opacity) / 100f * (outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f));
        if (prog <= 0.001f || alpha <= 0.002f) return;

        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float bw = c.F(BoxWidth), bh = c.F(BoxHeight), lw = c.F(LineWidth), rough = c.F(Rough) / 100f;
        float rot = Gfx.Rad(c.F(Tilt));
        int seed = Seed;
        float jit = MathF.Min(bw, bh) * 0.06f * rough;

        // 線(ひと筆ずつ)。座標は中心を0とした、回転前の位置
        var strokes = new List<List<Vector2>>();
        switch (Kind)
        {
            case EncircleKind.Circle:
            {
                // 少し行き過ぎて重なる楕円
                var pts = new List<Vector2>();
                int n = 90;
                float a0 = -MathF.PI * 0.62f, turn = MathF.PI * 2f * 1.1f;
                for (int i = 0; i <= n; i++)
                {
                    float f = i / (float)n;
                    float a = a0 + turn * f;
                    float k = 1f + Noise(f * 6f, seed) * 0.08f * rough + (f - 0.5f) * 0.06f * rough;
                    pts.Add(new Vector2(MathF.Cos(a) * bw * 0.58f * k, MathF.Sin(a) * bh * 0.62f * k));
                }
                strokes.Add(pts);
                break;
            }
            case EncircleKind.Rect:
            {
                float x0 = -bw / 2f - lw, x1 = bw / 2f + lw, y0 = -bh / 2f - lw, y1 = bh / 2f + lw;
                var corners = new[] { new Vector2(x0, y0), new Vector2(x1, y0), new Vector2(x1, y1), new Vector2(x0, y1), new Vector2(x0 + jit * 2f, y0 - jit) };
                var pts = new List<Vector2>();
                for (int e = 0; e < corners.Length - 1; e++)
                    for (int i = 0; i < 16; i++)
                    {
                        float f = i / 16f;
                        var q = Vector2.Lerp(corners[e], corners[e + 1], f);
                        pts.Add(q + new Vector2(Noise(e * 3f + f * 2f, seed), Noise(e * 3f + f * 2f + 9f, seed)) * jit);
                    }
                strokes.Add(pts);
                break;
            }
            case EncircleKind.Underline:
            case EncircleKind.Strike:
            {
                float y = Kind == EncircleKind.Strike ? 0f : bh / 2f + lw;
                var pts = new List<Vector2>();
                for (int i = 0; i <= 40; i++)
                {
                    float f = i / 40f;
                    pts.Add(new Vector2(-bw / 2f + bw * f, y + Noise(f * 4f, seed) * jit * 0.8f - f * jit * 0.6f));
                }
                strokes.Add(pts);
                break;
            }
            case EncircleKind.DoubleUnderline:
            {
                for (int line = 0; line < 2; line++)
                {
                    float y = bh / 2f + lw + line * lw * 2.4f;
                    var pts = new List<Vector2>();
                    for (int i = 0; i <= 40; i++)
                    {
                        float f = i / 40f;
                        pts.Add(new Vector2(-bw / 2f + bw * f * (line == 0 ? 1f : 0.96f), y + Noise(f * 4f + line * 5f, seed) * jit * 0.8f));
                    }
                    strokes.Add(pts);
                }
                break;
            }
            case EncircleKind.Wavy:
            {
                float y = bh / 2f + lw * 1.5f;
                var pts = new List<Vector2>();
                int waves = Math.Max(3, (int)(bw / MathF.Max(20f, lw * 4f)));
                for (int i = 0; i <= waves * 12; i++)
                {
                    float f = i / (waves * 12f);
                    pts.Add(new Vector2(-bw / 2f + bw * f, y + MathF.Sin(f * waves * MathF.PI * 2f) * lw * 1.4f));
                }
                strokes.Add(pts);
                break;
            }
            case EncircleKind.Cross:
            {
                float r = MathF.Min(bw, bh) / 2f;
                strokes.Add(Line(new Vector2(-r, -r), new Vector2(r, r), jit, seed, 0));
                strokes.Add(Line(new Vector2(r, -r), new Vector2(-r, r), jit, seed, 1));
                break;
            }
            case EncircleKind.Check:
            {
                float r = MathF.Min(bw, bh) / 2f;
                var pts = Line(new Vector2(-r, 0f), new Vector2(-r * 0.25f, r * 0.7f), jit, seed, 0);
                pts.AddRange(Line(new Vector2(-r * 0.25f, r * 0.7f), new Vector2(r, -r * 0.85f), jit, seed, 1));
                strokes.Add(pts);
                break;
            }
        }

        using var brush = dc.CreateSolidColorBrush(default);
        float width = Highlighter ? lw * 3.2f : lw;
        brush.Color = Gfx.C4(Color, alpha * (Highlighter ? 0.45f : 1f));
        // 複数の筆は、順番に描く
        float per = 1f / strokes.Count;
        for (int s = 0; s < strokes.Count; s++)
        {
            float local = Math.Clamp((prog - s * per) / per, 0f, 1f);
            if (local <= 0f) continue;
            var placed = DrawKit.Place(strokes[s], ctr, rot);
            var part = BubbleKit.PartByLength(placed, local);
            if (part.Count < 2) continue;
            if (Highlighter)
            {
                // 蛍光ペン: 角ばった太い線(重ね塗りで濃くならないよう、帯にして1回で塗る)
                for (int i = 0; i < part.Count - 1; i++)
                {
                    var d = part[i + 1] - part[i];
                    if (d.LengthSquared() < 1e-4f) continue;
                    var nn = Vector2.Normalize(new Vector2(-d.Y, d.X)) * width / 2f;
                    Gfx.FillPoly(dc, brush, new List<Vector2> { part[i] + nn, part[i + 1] + nn, part[i + 1] - nn, part[i] - nn });
                }
            }
            else
            {
                // 筆の入りと抜きを、少し細くする
                for (int i = 0; i < part.Count - 1; i++)
                {
                    float f = i / (float)Math.Max(1, part.Count - 1);
                    float taper = 0.55f + 0.45f * MathF.Sin(MathF.PI * Math.Clamp(f * 1.15f, 0f, 1f));
                    float ww = width * (0.7f + 0.3f * taper);
                    dc.DrawLine(part[i], part[i + 1], brush, ww);
                    dc.FillEllipse(new Ellipse(part[i + 1], ww / 2f, ww / 2f), brush);
                }
            }
        }
    }

    static List<Vector2> Line(Vector2 a, Vector2 b, float jit, int seed, int k)
    {
        var pts = new List<Vector2>();
        for (int i = 0; i <= 24; i++)
        {
            float f = i / 24f;
            var n = Vector2.Normalize(new Vector2(-(b - a).Y, (b - a).X));
            pts.Add(Vector2.Lerp(a, b, f) + n * Noise(f * 3f + k * 7f, seed) * jit);
        }
        return pts;
    }
}
