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

public enum SpeechBubbleStyle
{
    [Display(Name = "角丸")] Round,
    [Display(Name = "楕円")] Oval,
    [Display(Name = "トゲ（叫び）")] Spiky,
    [Display(Name = "もくもく（考え中）")] Cloud,
    [Display(Name = "四角")] Box,
}

/// <summary>128番以降の図形で使う、形の小道具</summary>
internal static class BubbleKit
{
    /// <summary>2回塗りで、ふち取りが1つにつながった図形を描く(しっぽと本体の境目が出ない)</summary>
    public static void FillOutlined(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, IList<List<Vector2>> polys, IList<(Vector2 c, float r)> circles,
        Color4 fill, Color4 line, float lineWidth)
    {
        if (lineWidth > 0.1f && line.A > 0.002f)
        {
            brush.Color = line;
            float r = lineWidth;
            foreach (var p in polys)
            {
                Gfx.FillPoly(dc, brush, p);
                for (int i = 0; i < p.Count; i++)
                {
                    dc.DrawLine(p[i], p[(i + 1) % p.Count], brush, lineWidth * 2f);
                    dc.FillEllipse(new Ellipse(p[i], r, r), brush);
                }
            }
            foreach (var (c, rr) in circles) dc.FillEllipse(new Ellipse(c, rr + lineWidth, rr + lineWidth), brush);
        }
        brush.Color = fill;
        foreach (var p in polys) Gfx.FillPoly(dc, brush, p);
        foreach (var (c, rr) in circles) dc.FillEllipse(new Ellipse(c, rr, rr), brush);
    }

    /// <summary>点列を、中心 c のまわりに k 倍する</summary>
    public static List<Vector2> Scale(IEnumerable<Vector2> pts, Vector2 c, float k) => pts.Select(p => c + (p - c) * k).ToList();

    /// <summary>もくもく(雲形)の輪郭</summary>
    public static List<Vector2> CloudPts(Vector2 c, float rx, float ry, int bumps, float depth)
    {
        var pts = new List<Vector2>();
        int n = bumps * 10;
        for (int i = 0; i < n; i++)
        {
            float a = i / (float)n * MathF.PI * 2f;
            float f = DrawKit.Frac(i / 10f);
            float k = 1f + depth * MathF.Sin(f * MathF.PI);
            pts.Add(c + new Vector2(MathF.Cos(a) * rx * k, MathF.Sin(a) * ry * k));
        }
        return pts;
    }

    /// <summary>トゲトゲ(叫び)の輪郭</summary>
    public static List<Vector2> SpikyPts(Vector2 c, float rx, float ry, int spikes, float depth, int seed)
    {
        var pts = new List<Vector2>();
        for (int i = 0; i < spikes * 2; i++)
        {
            float a = (i + DrawKit.H(seed, i) * 0.4f) / (spikes * 2f) * MathF.PI * 2f;
            float k = i % 2 == 0 ? 1f + depth * (0.6f + DrawKit.H(seed, i, 1) * 0.8f) : 1f - depth * 0.15f;
            pts.Add(c + new Vector2(MathF.Cos(a) * rx * k, MathF.Sin(a) * ry * k));
        }
        return pts;
    }

    /// <summary>ちいさな矢じり(三角)</summary>
    public static List<Vector2> ArrowHead(Vector2 tip, Vector2 dir, float size, float widthRatio = 0.6f)
    {
        if (dir.LengthSquared() < 1e-6f) dir = new Vector2(1, 0);
        dir = Vector2.Normalize(dir);
        var n = new Vector2(-dir.Y, dir.X);
        var back = tip - dir * size;
        return new List<Vector2> { tip, back + n * size * widthRatio, back - n * size * widthRatio };
    }

    /// <summary>2次ベジェの点列</summary>
    public static List<Vector2> Quad(Vector2 a, Vector2 ctrl, Vector2 b, int n = 48)
    {
        var pts = new List<Vector2>(n + 1);
        for (int i = 0; i <= n; i++)
        {
            float t = i / (float)n, u = 1f - t;
            pts.Add(a * (u * u) + ctrl * (2f * u * t) + b * (t * t));
        }
        return pts;
    }

    /// <summary>点列の長さ</summary>
    public static float Length(IReadOnlyList<Vector2> pts)
    {
        float s = 0f;
        for (int i = 1; i < pts.Count; i++) s += Vector2.Distance(pts[i - 1], pts[i]);
        return s;
    }

    /// <summary>点列の、はじめから割合 p までの部分(長さで測る)</summary>
    public static List<Vector2> PartByLength(IReadOnlyList<Vector2> pts, float p)
    {
        var res = new List<Vector2>();
        if (pts.Count == 0) return res;
        float total = Length(pts), want = Math.Clamp(p, 0f, 1f) * total, acc = 0f;
        res.Add(pts[0]);
        for (int i = 1; i < pts.Count; i++)
        {
            float d = Vector2.Distance(pts[i - 1], pts[i]);
            if (acc + d >= want)
            {
                float t = d <= 1e-6f ? 0f : (want - acc) / d;
                res.Add(Vector2.Lerp(pts[i - 1], pts[i], t));
                return res;
            }
            acc += d;
            res.Add(pts[i]);
        }
        return res;
    }

    /// <summary>色の一覧(円グラフ・相関図などの、自動の色)</summary>
    public static WpfColor Palette(int scheme, int i)
    {
        WpfColor[] vivid = { WpfColor.FromRgb(0xFF, 0x5C, 0x5C), WpfColor.FromRgb(0x3D, 0x9C, 0xFF), WpfColor.FromRgb(0xFF, 0xC1, 0x2E), WpfColor.FromRgb(0x3C, 0xC8, 0x7A), WpfColor.FromRgb(0xA8, 0x6B, 0xFF), WpfColor.FromRgb(0xFF, 0x8A, 0x3D), WpfColor.FromRgb(0x2E, 0xD3, 0xD3), WpfColor.FromRgb(0xFF, 0x6B, 0xC1) };
        WpfColor[] pastel = { WpfColor.FromRgb(0xFF, 0xAD, 0xAD), WpfColor.FromRgb(0xA0, 0xC4, 0xFF), WpfColor.FromRgb(0xFD, 0xE2, 0x8C), WpfColor.FromRgb(0xA8, 0xE6, 0xB5), WpfColor.FromRgb(0xCF, 0xB8, 0xFF), WpfColor.FromRgb(0xFF, 0xC8, 0x9E), WpfColor.FromRgb(0x9E, 0xE6, 0xE6), WpfColor.FromRgb(0xFF, 0xB8, 0xE0) };
        WpfColor[] cool = { WpfColor.FromRgb(0x1E, 0x40, 0xAF), WpfColor.FromRgb(0x25, 0x63, 0xEB), WpfColor.FromRgb(0x3B, 0x82, 0xF6), WpfColor.FromRgb(0x06, 0xB6, 0xD4), WpfColor.FromRgb(0x14, 0xB8, 0xA6), WpfColor.FromRgb(0x81, 0x8C, 0xF8), WpfColor.FromRgb(0x93, 0xC5, 0xFD), WpfColor.FromRgb(0x67, 0xE8, 0xF9) };
        WpfColor[] warm = { WpfColor.FromRgb(0xB9, 0x1C, 0x1C), WpfColor.FromRgb(0xEA, 0x58, 0x0C), WpfColor.FromRgb(0xF5, 0x9E, 0x0B), WpfColor.FromRgb(0xEF, 0x44, 0x44), WpfColor.FromRgb(0xFB, 0x92, 0x3C), WpfColor.FromRgb(0xFA, 0xCC, 0x15), WpfColor.FromRgb(0xF4, 0x72, 0xB6), WpfColor.FromRgb(0xFD, 0xBA, 0x74) };
        WpfColor[] mono = { WpfColor.FromRgb(0x22, 0x22, 0x22), WpfColor.FromRgb(0x55, 0x55, 0x55), WpfColor.FromRgb(0x88, 0x88, 0x88), WpfColor.FromRgb(0xAA, 0xAA, 0xAA), WpfColor.FromRgb(0xCC, 0xCC, 0xCC), WpfColor.FromRgb(0x44, 0x44, 0x44), WpfColor.FromRgb(0x77, 0x77, 0x77), WpfColor.FromRgb(0xBB, 0xBB, 0xBB) };
        var set = scheme switch { 1 => pastel, 2 => cool, 3 => warm, 4 => mono, _ => vivid };
        return set[((i % set.Length) + set.Length) % set.Length];
    }
}

// =====================================================================
//  吹き出し
//   セリフの長さに合わせて、自動で大きさが変わる。しっぽの先は、配置画面のつまみで動かせる
// =====================================================================
public sealed class SpeechBubblePlugin : IShapePlugin
{
    public string Name => "演出：吹き出し";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpeechBubbleParameter(sharedData);
}

public sealed class SpeechBubbleParameter : EffectShapeParameterBase
{
    public SpeechBubbleParameter() : this(null) { }
    public SpeechBubbleParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "セリフ", Description = "改行もできます。「\\n」と書いても改行になります")]
    [TextEditor(AcceptsReturn = true)]
    public string Text { get => text; set => Set(ref text, value); }
    string text = "ここにセリフ";

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "文字", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 10, 200)]
    public Animation FontSize { get; } = new Animation(56, 4, 1000);

    [Display(GroupName = "文字", Name = "太字")]
    [ToggleSlider]
    public bool Bold { get => bold; set => Set(ref bold, value); }
    bool bold = true;

    [Display(GroupName = "文字", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(0x22, 0x22, 0x22);

    [Display(GroupName = "文字", Name = "1文字ずつ出す（文字/秒）", Description = "0 なら一度に出します")]
    [AnimationSlider("F0", "文字/秒", 0, 60)]
    public Animation TypeSpeed { get; } = new Animation(0, 0, 1000);

    [Display(GroupName = "形", Name = "形")]
    [EnumComboBox]
    public SpeechBubbleStyle Style { get => style; set => Set(ref style, value); }
    SpeechBubbleStyle style = SpeechBubbleStyle.Round;

    [Display(GroupName = "形", Name = "中の色")]
    [ColorPicker]
    public WpfColor FillColor { get => fillColor; set => Set(ref fillColor, value); }
    WpfColor fillColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "形", Name = "線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(0x22, 0x22, 0x22);

    [Display(GroupName = "形", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0, 20)]
    public Animation LineWidth { get; } = new Animation(5, 0, 200);

    [Display(GroupName = "形", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(35, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "セリフに合わせて大きさを変える")]
    [ToggleSlider]
    public bool AutoSize { get => autoSize; set => Set(ref autoSize, value); }
    bool autoSize = true;

    [Display(GroupName = "位置と大きさ", Name = "幅（自動でない時）")]
    [AnimationSlider("F0", "px", 100, 1600)]
    public Animation BoxWidth { get; } = new Animation(700, 10, 8000);

    [Display(GroupName = "位置と大きさ", Name = "高さ（自動でない時）")]
    [AnimationSlider("F0", "px", 60, 1000)]
    public Animation BoxHeight { get; } = new Animation(260, 10, 8000);

    [Display(GroupName = "位置と大きさ", Name = "折り返す幅", Description = "セリフがこの幅を超えると、改行します")]
    [AnimationSlider("F0", "px", 100, 1800)]
    public Animation WrapWidth { get; } = new Animation(900, 20, 8000);

    [Display(GroupName = "位置と大きさ", Name = "文字のまわりの余白")]
    [AnimationSlider("F0", "px", 0, 120)]
    public Animation Padding { get; } = new Animation(40, 0, 1000);

    [Display(GroupName = "しっぽ", Name = "しっぽを出す")]
    [ToggleSlider]
    public bool TailOn { get => tailOn; set => Set(ref tailOn, value); }
    bool tailOn = true;

    [Display(GroupName = "しっぽ", Name = "しっぽの先X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation TailX { get; } = new Animation(38, -100, 200);

    [Display(GroupName = "しっぽ", Name = "しっぽの先Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation TailY { get; } = new Animation(70, -100, 200);

    [Display(GroupName = "しっぽ", Name = "しっぽの太さ")]
    [AnimationSlider("F0", "px", 10, 200)]
    public Animation TailWidth { get; } = new Animation(64, 2, 1000);

    [Display(GroupName = "動き", Name = "ポンと出る時間")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation InTime { get; } = new Animation(0.3, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.2, 0, 60);

    [Display(GroupName = "動き", Name = "ふわふわ揺れる")]
    [AnimationSlider("F0", "px", 0, 30)]
    public Animation Float { get; } = new Animation(0, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [FontSize, TypeSpeed, LineWidth, Opacity, PosX, PosY, BoxWidth, BoxHeight, WrapWidth, Padding, TailX, TailY, TailWidth, InTime, OutTime, Float];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float inT = c.F(InTime), outT = c.F(OutTime);
        float kin = inT <= 0.001f ? 1f : DrawKit.EaseOutBack(t / inT);
        float kout = outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f);
        float k = kin * (0.7f + 0.3f * kout);
        float alpha = c.F(Opacity) / 100f * Math.Clamp(t / MathF.Max(0.001f, inT * 0.5f), 0f, 1f) * kout;
        if (k <= 0.01f || alpha <= 0.002f) return;

        string full = TextKit.Lines(Text);
        float fs = c.F(FontSize), pad = c.F(Padding), lw = c.F(LineWidth);
        float cps = c.F(TypeSpeed);
        var shapeFull = TextKit.Get(full, fs, Bold, Font, c.F(WrapWidth), cps > 0.001f ? 0 : 1);
        var center = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h + MathF.Sin(t * 2.2f) * c.F(Float));

        float bw, bh;
        if (AutoSize)
        {
            bw = MathF.Max(shapeFull.Width, fs) + pad * 2f;
            bh = MathF.Max(shapeFull.Height, fs) + pad * 2f;
            // 丸い形は、文字が角にかからないよう大きめに
            if (Style is SpeechBubbleStyle.Oval or SpeechBubbleStyle.Cloud) { bw *= 1.3f; bh *= 1.35f; }
            if (Style == SpeechBubbleStyle.Spiky) { bw *= 1.35f; bh *= 1.45f; }
        }
        else { bw = c.F(BoxWidth); bh = c.F(BoxHeight); }

        List<Vector2> body = Style switch
        {
            SpeechBubbleStyle.Oval => DrawKit.EllipsePts(center, bw / 2f, bh / 2f, 96),
            SpeechBubbleStyle.Cloud => BubbleKit.CloudPts(center, bw / 2f * 0.93f, bh / 2f * 0.9f, Math.Clamp((int)(bw / 70f), 8, 18), 0.09f),
            SpeechBubbleStyle.Spiky => BubbleKit.SpikyPts(center, bw / 2f * 0.86f, bh / 2f * 0.84f, Math.Clamp((int)(bw / 60f), 10, 26), 0.22f, Seed),
            SpeechBubbleStyle.Box => ShapeGeom.RoundRect(center.X, center.Y, bw, bh, MathF.Min(bw, bh) * 0.04f),
            _ => ShapeGeom.RoundRect(center.X, center.Y, bw, bh, MathF.Min(bw, bh) * 0.32f),
        };

        var polys = new List<List<Vector2>> { BubbleKit.Scale(body, center, k) };
        var circles = new List<(Vector2 c, float r)>();
        if (TailOn)
        {
            var tip = new Vector2(c.F(TailX) / 100f * w, c.F(TailY) / 100f * h);
            var d = tip - center;
            if (d.LengthSquared() > 1f)
            {
                var dir = Vector2.Normalize(d);
                if (Style == SpeechBubbleStyle.Cloud)
                {
                    // 考え中: 小さな丸が、先へ向かって並ぶ
                    float edge = MathF.Min(bw, bh) * 0.42f;
                    var from = center + dir * edge;
                    float tw = c.F(TailWidth);
                    for (int i = 0; i < 3; i++)
                    {
                        float f = (i + 1) / 3.2f;
                        var p = Vector2.Lerp(from, tip, f);
                        circles.Add((center + (p - center) * k, tw * (0.42f - i * 0.11f) * k));
                    }
                }
                else
                {
                    var n = new Vector2(-dir.Y, dir.X);
                    float tw = c.F(TailWidth);
                    // しっぽの付け根は、本体の内側に少しめり込ませる(つなぎ目が出ないように)
                    float inside = MathF.Min(bw, bh) * 0.25f;
                    var root = center + dir * inside;
                    // 先を少し曲げると、手描きの吹き出しらしくなる
                    var bend = n * tw * 0.25f;
                    var tail = new List<Vector2> { root + n * tw / 2f, Vector2.Lerp(root, tip, 0.55f) + bend + n * tw * 0.12f, tip, Vector2.Lerp(root, tip, 0.55f) + bend - n * tw * 0.18f, root - n * tw / 2f };
                    polys.Add(BubbleKit.Scale(tail, center, k));
                }
            }
        }

        using var brush = dc.CreateSolidColorBrush(default);
        // 影
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.18f * alpha);
        foreach (var p in polys) Gfx.FillPoly(dc, brush, p.Select(q => q + new Vector2(6f, 8f)).ToList());
        BubbleKit.FillOutlined(dc, brush, polys, circles, Gfx.C4(FillColor, alpha), Gfx.C4(LineColor, alpha), lw);

        // セリフ
        string shown = TextKit.Typed(full, t - inT * 0.6f, cps);
        if (shown.Length > 0)
        {
            var s = cps > 0.001f ? TextKit.Get(shown, fs, Bold, Font, c.F(WrapWidth), 0) : shapeFull;
            // 1文字ずつ出す時も、全体の位置は動かさない(左上を、全文の時と同じにする)
            var topLeft = center - new Vector2(shapeFull.Width, shapeFull.Height) * (k / 2f);
            TextKit.Draw(dc, brush, s, topLeft, Gfx.C4(TextColor, alpha), scale: k);
        }
    }
}
