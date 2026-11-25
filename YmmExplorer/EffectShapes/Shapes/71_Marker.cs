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

/// <summary>折れ線の一部だけ（先頭から割合 p）を取り出す</summary>
internal static class PathKit
{
    public static List<Vector2> Part(List<Vector2> pts, float p)
    {
        var res = new List<Vector2>();
        if (pts.Count == 0) return res;
        float f = Math.Clamp(p, 0f, 1f) * (pts.Count - 1);
        int idx = (int)MathF.Floor(f);
        for (int i = 0; i <= Math.Min(idx, pts.Count - 1); i++) res.Add(pts[i]);
        if (idx < pts.Count - 1) res.Add(Vector2.Lerp(pts[idx], pts[idx + 1], f - idx));
        return res;
    }

    // 手書きっぽい小さなゆれ（座標に対して一定）
    public static Vector2 Wobble(int i, float amt, int seed)
        => new(MathF.Sin(i * 1.7f + seed) * amt, MathF.Cos(i * 2.3f + seed * 0.7f) * amt);

    // 簡易ストローク文字（REC・LIVE などの英字を線で描く）
    static readonly Dictionary<char, Vector2[][]> Glyphs = new()
    {
        ['R'] = new[] { new Vector2[] { new(0, 1), new(0, 0), new(.5f, 0), new(.6f, .15f), new(.6f, .35f), new(.5f, .5f), new(0, .5f) }, new Vector2[] { new(.3f, .5f), new(.6f, 1) } },
        ['E'] = new[] { new Vector2[] { new(.6f, 0), new(0, 0), new(0, 1), new(.6f, 1) }, new Vector2[] { new(0, .5f), new(.45f, .5f) } },
        ['C'] = new[] { new Vector2[] { new(.6f, .15f), new(.45f, 0), new(.15f, 0), new(0, .15f), new(0, .85f), new(.15f, 1), new(.45f, 1), new(.6f, .85f) } },
        ['L'] = new[] { new Vector2[] { new(0, 0), new(0, 1), new(.6f, 1) } },
        ['I'] = new[] { new Vector2[] { new(.3f, 0), new(.3f, 1) }, new Vector2[] { new(.1f, 0), new(.5f, 0) }, new Vector2[] { new(.1f, 1), new(.5f, 1) } },
        ['V'] = new[] { new Vector2[] { new(0, 0), new(.3f, 1), new(.6f, 0) } },
        ['N'] = new[] { new Vector2[] { new(0, 1), new(0, 0), new(.6f, 1), new(.6f, 0) } },
        ['W'] = new[] { new Vector2[] { new(0, 0), new(.15f, 1), new(.3f, .4f), new(.45f, 1), new(.6f, 0) } },
        ['O'] = new[] { new Vector2[] { new(.15f, 0), new(.45f, 0), new(.6f, .15f), new(.6f, .85f), new(.45f, 1), new(.15f, 1), new(0, .85f), new(0, .15f), new(.15f, 0) } },
        ['A'] = new[] { new Vector2[] { new(0, 1), new(.3f, 0), new(.6f, 1) }, new Vector2[] { new(.12f, .65f), new(.48f, .65f) } },
        ['T'] = new[] { new Vector2[] { new(0, 0), new(.6f, 0) }, new Vector2[] { new(.3f, 0), new(.3f, 1) } },
        ['P'] = new[] { new Vector2[] { new(0, 1), new(0, 0), new(.5f, 0), new(.6f, .15f), new(.6f, .35f), new(.5f, .5f), new(0, .5f) } },
    };

    /// <summary>英大文字（R E C L I V N W O A T P）を線で描く。戻り値は描いた幅</summary>
    public static float Text(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, string text, Vector2 origin, float h, float lw)
    {
        float x = origin.X, w = h * 0.6f, gap = h * 0.22f;
        foreach (char ch in text)
        {
            if (Glyphs.TryGetValue(ch, out var strokes))
                foreach (var st in strokes)
                    for (int i = 0; i < st.Length - 1; i++)
                        dc.DrawLine(new Vector2(x + st[i].X / 0.6f * w, origin.Y + st[i].Y * h), new Vector2(x + st[i + 1].X / 0.6f * w, origin.Y + st[i + 1].Y * h), brush, lw);
            x += w + gap;
        }
        return x - origin.X - gap;
    }

    public static float TextWidth(string text, float h) => text.Length * (h * 0.6f + h * 0.22f) - h * 0.22f;
}

public enum MarkKind
{
    [Display(Name = "丸囲み")] Circle,
    [Display(Name = "四角囲み")] Box,
    [Display(Name = "下線")] Underline,
    [Display(Name = "二重下線")] DoubleUnderline,
    [Display(Name = "波線")] Wave,
    [Display(Name = "矢印")] Arrow,
    [Display(Name = "チェック")] Check,
    [Display(Name = "バツ")] Cross,
}

// =====================================================================
//  強調マーカー
//   手書き風に描き上がる丸囲み・四角囲み・下線・矢印・チェック・バツ
// =====================================================================
public sealed class MarkerPlugin : IShapePlugin
{
    public string Name => "演出：強調マーカー";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MarkerParameter(sharedData);
}

public sealed class MarkerParameter : EffectShapeParameterBase
{
    public MarkerParameter() : this(null) { }
    public MarkerParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "マーカー", Name = "種類")]
    [EnumComboBox]
    public MarkKind Kind { get => kind; set => Set(ref kind, value); }
    MarkKind kind = MarkKind.Circle;

    [Display(GroupName = "マーカー", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x3B, 0x3B);

    [Display(GroupName = "マーカー", Name = "蛍光ペン風（太く半透明）")]
    [ToggleSlider]
    public bool Highlighter { get => hl; set => Set(ref hl, value); }
    bool hl = false;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "幅")]
    [AnimationSlider("F0", "px", 20, 1200)]
    public Animation MarkWidth { get; } = new Animation(420, 5, 8000);

    [Display(GroupName = "位置と大きさ", Name = "高さ")]
    [AnimationSlider("F0", "px", 10, 800)]
    public Animation MarkHeight { get; } = new Animation(180, 3, 6000);

    [Display(GroupName = "位置と大きさ", Name = "回転")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Rotation { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "線", Name = "太さ")]
    [AnimationSlider("F1", "px", 1, 60)]
    public Animation Thickness { get; } = new Animation(10, 0.5, 400);

    [Display(GroupName = "線", Name = "手書きのゆれ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Wobble { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "線", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "動き", Name = "描き上がるアニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "描くのにかかる時間")]
    [AnimationSlider("F2", "秒", 0.1, 4)]
    public Animation IntroTime { get; } = new Animation(0.7, 0.05, 30);

    [Display(GroupName = "動き", Name = "描き上がり（手動）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Progress { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, MarkWidth, MarkHeight, Rotation, Thickness, Wobble, Opacity, IntroTime, Progress];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float p = c.F(Progress) / 100f;
        if (Intro) p *= Gfx.Smooth(0f, MathF.Max(0.05f, c.F(IntroTime)), t);
        if (p <= 0.002f) return;

        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float W = c.F(MarkWidth), H = c.F(MarkHeight), th = c.F(Thickness), wob = c.F(Wobble) / 100f * 6f, op = c.F(Opacity) / 100f;
        float rot = Gfx.Rad(c.F(Rotation));

        // ---- 形ごとの点列（中心が原点）----
        var pts = new List<Vector2>();
        var extra = new List<List<Vector2>>();           // 矢じりなど、描き終わりに現れる線
        switch (Kind)
        {
            case MarkKind.Circle:
                {
                    int n = 90;
                    for (int i = 0; i <= n; i++)
                    {
                        float f = i / (float)n;
                        float a = -MathF.PI / 2f + f * MathF.PI * 2f * 1.08f;
                        float r = 1f + 0.07f * f + 0.03f * MathF.Sin(f * 9f + Seed);
                        pts.Add(new Vector2(MathF.Cos(a) * W / 2f * r, MathF.Sin(a) * H / 2f * r) + PathKit.Wobble(i / 3, wob, Seed));
                    }
                    break;
                }
            case MarkKind.Box:
                {
                    float x0 = -W / 2f, x1 = W / 2f, y0 = -H / 2f, y1 = H / 2f;
                    var c4 = new[] { new Vector2(x0 - 6, y0 + 4), new Vector2(x1 + 4, y0 - 3), new Vector2(x1 - 3, y1 + 5), new Vector2(x0 + 5, y1 - 4), new Vector2(x0 - 4, y0 + 14) };
                    for (int k = 0; k < 4; k++)
                        for (int i = 0; i <= 12; i++)
                            pts.Add(Vector2.Lerp(c4[k], c4[k + 1], i / 12f) + PathKit.Wobble(k * 13 + i, wob * 0.6f, Seed));
                    break;
                }
            case MarkKind.Underline:
            case MarkKind.DoubleUnderline:
                {
                    for (int i = 0; i <= 40; i++)
                    {
                        float f = i / 40f;
                        pts.Add(new Vector2(-W / 2f + f * W, H / 2f + (f - 0.5f) * -6f) + PathKit.Wobble(i, wob * 0.5f, Seed));
                    }
                    if (Kind == MarkKind.DoubleUnderline)
                    {
                        var second = new List<Vector2>();
                        for (int i = 0; i <= 40; i++)
                        {
                            float f = i / 40f;
                            second.Add(new Vector2(W / 2f - f * W * 0.85f, H / 2f + th * 2.2f + (f - 0.5f) * 5f) + PathKit.Wobble(i + 50, wob * 0.5f, Seed));
                        }
                        pts.AddRange(second);
                    }
                    break;
                }
            case MarkKind.Wave:
                for (int i = 0; i <= 80; i++)
                {
                    float f = i / 80f;
                    pts.Add(new Vector2(-W / 2f + f * W, H / 2f + MathF.Sin(f * MathF.PI * 2f * (W / 70f)) * th * 0.9f));
                }
                break;
            case MarkKind.Arrow:
                {
                    // 左下から右上へ、ゆるく曲がる矢印
                    var a = new Vector2(-W / 2f, H / 2f);
                    var b = new Vector2(W / 2f, -H / 2f);
                    var m = new Vector2(-W * 0.05f, H * 0.35f);
                    for (int i = 0; i <= 50; i++)
                    {
                        float f = i / 50f;
                        var q = (1 - f) * (1 - f) * a + 2 * (1 - f) * f * m + f * f * b;
                        pts.Add(q + PathKit.Wobble(i / 2, wob * 0.5f, Seed));
                    }
                    var dir = Vector2.Normalize(b - m);
                    var perp = new Vector2(-dir.Y, dir.X);
                    float hl2 = MathF.Max(20f, th * 4f);
                    extra.Add(new List<Vector2> { b - dir * hl2 + perp * hl2 * 0.6f, b, b - dir * hl2 - perp * hl2 * 0.6f });
                    break;
                }
            case MarkKind.Check:
                pts.Add(new Vector2(-W * 0.38f, 0f)); pts.Add(new Vector2(-W * 0.12f, H * 0.38f)); pts.Add(new Vector2(W * 0.4f, -H * 0.42f));
                break;
            default:    // バツ
                pts.Add(new Vector2(-W * 0.4f, -H * 0.4f)); pts.Add(new Vector2(W * 0.4f, H * 0.4f));
                extra.Add(new List<Vector2> { new(W * 0.4f, -H * 0.4f), new(-W * 0.4f, H * 0.4f) });
                break;
        }

        // 回転と平行移動
        float cs = MathF.Cos(rot), sn = MathF.Sin(rot);
        Vector2 Tf(Vector2 v) => ctr + new Vector2(v.X * cs - v.Y * sn, v.X * sn + v.Y * cs);
        var main = Part(pts, p).Select(Tf).ToList();

        using var brush = dc.CreateSolidColorBrush(default);
        if (Highlighter)
        {
            brush.Color = Gfx.C4(Color, 0.35f * op);
            if (main.Count > 1) FormKit.Stroke(dc, brush, main, th * 2.4f, false);
        }
        else
        {
            brush.Color = Gfx.C4(Color, 0.18f * op);
            if (main.Count > 1) FormKit.Stroke(dc, brush, main, th * 2.2f, false);
            brush.Color = Gfx.C4(Color, 0.95f * op);
            if (main.Count > 1) FormKit.Stroke(dc, brush, main, th, true);
        }
        float ep = Gfx.Smooth(0.85f, 1f, p);
        foreach (var e in extra)
        {
            var q = Part(e, ep).Select(Tf).ToList();
            if (q.Count < 2) continue;
            brush.Color = Gfx.C4(Color, (Highlighter ? 0.35f : 0.95f) * op);
            FormKit.Stroke(dc, brush, q, Highlighter ? th * 2.4f : th, true);
        }
    }

    static List<Vector2> Part(List<Vector2> pts, float p) => PathKit.Part(pts, p);
}

public enum ChartKind
{
    [Display(Name = "棒グラフ")] Bar,
    [Display(Name = "横棒グラフ")] BarH,
    [Display(Name = "折れ線グラフ")] Line,
    [Display(Name = "円グラフ")] Pie,
    [Display(Name = "ドーナツグラフ")] Donut,
}
