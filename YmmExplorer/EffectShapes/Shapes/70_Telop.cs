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

/// <summary>多角形の生成や7セグ数字など、複数の図形で使う幾何ヘルパー</summary>
internal static class ShapeGeom
{
    /// <summary>角丸の長方形の点列</summary>
    public static List<Vector2> RoundRect(float cx, float cy, float w, float h, float r)
    {
        r = MathF.Min(r, MathF.Min(w, h) / 2f);
        var pts = new List<Vector2>();
        float x0 = cx - w / 2f, x1 = cx + w / 2f, y0 = cy - h / 2f, y1 = cy + h / 2f;
        void Corner(float ccx, float ccy, float a0)
        {
            for (int i = 0; i <= 6; i++)
            {
                float a = a0 + i * MathF.PI / 2f / 6f;
                pts.Add(new Vector2(ccx + MathF.Cos(a) * r, ccy + MathF.Sin(a) * r));
            }
        }
        Corner(x1 - r, y0 + r, -MathF.PI / 2f);
        Corner(x1 - r, y1 - r, 0f);
        Corner(x0 + r, y1 - r, MathF.PI / 2f);
        Corner(x0 + r, y0 + r, MathF.PI);
        return pts;
    }

    /// <summary>星形（頂点数と内側の比率を指定）</summary>
    public static List<Vector2> StarPoly(Vector2 c, float r, int points, float inner, float rot)
    {
        var pts = new List<Vector2>();
        for (int i = 0; i < points * 2; i++)
        {
            float a = rot - MathF.PI / 2f + i * MathF.PI / points;
            float rr = (i % 2 == 0) ? r : r * inner;
            pts.Add(c + new Vector2(MathF.Cos(a), MathF.Sin(a)) * rr);
        }
        return pts;
    }

    public static List<Vector2> HeartPoly(Vector2 c, float r, float rot)
    {
        var pts = new List<Vector2>();
        float s = r / 16f, cs = MathF.Cos(rot), sn = MathF.Sin(rot);
        for (int i = 0; i < 48; i++)
        {
            float tt = i / 48f * MathF.PI * 2f;
            float x = 16f * MathF.Pow(MathF.Sin(tt), 3f);
            float y = -(13f * MathF.Cos(tt) - 5f * MathF.Cos(2f * tt) - 2f * MathF.Cos(3f * tt) - MathF.Cos(4f * tt)) + 1.5f;
            pts.Add(c + new Vector2(x * cs - y * sn, x * sn + y * cs) * s);
        }
        return pts;
    }

    public static List<Vector2> Closed(List<Vector2> p) => new(p) { p[0] };

    // ---- 7セグ風の数字 ----
    static readonly int[] SegMask = { 0b0111111, 0b0000110, 0b1011011, 0b1001111, 0b1100110, 0b1101101, 0b1111101, 0b0000111, 0b1111111, 0b1101111 };

    public static void Digit(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, float x, float y, float w, float h, int d, float lw)
    {
        int m = SegMask[Math.Clamp(d, 0, 9)];
        float g = lw * 0.9f, hm = y + h / 2f;
        if ((m & 1) != 0) dc.DrawLine(new Vector2(x + g, y), new Vector2(x + w - g, y), brush, lw);
        if ((m & 2) != 0) dc.DrawLine(new Vector2(x + w, y + g), new Vector2(x + w, hm - g), brush, lw);
        if ((m & 4) != 0) dc.DrawLine(new Vector2(x + w, hm + g), new Vector2(x + w, y + h - g), brush, lw);
        if ((m & 8) != 0) dc.DrawLine(new Vector2(x + g, y + h), new Vector2(x + w - g, y + h), brush, lw);
        if ((m & 16) != 0) dc.DrawLine(new Vector2(x, hm + g), new Vector2(x, y + h - g), brush, lw);
        if ((m & 32) != 0) dc.DrawLine(new Vector2(x, y + g), new Vector2(x, hm - g), brush, lw);
        if ((m & 64) != 0) dc.DrawLine(new Vector2(x + g, hm), new Vector2(x + w - g, hm), brush, lw);
    }
}

public enum TelopStyle
{
    [Display(Name = "斜めの帯")] Band,
    [Display(Name = "吹き出し")] Bubble,
    [Display(Name = "叫び（ギザギザ）")] Shout,
    [Display(Name = "矢印タグ")] Tag,
    [Display(Name = "リボン")] Ribbon,
    [Display(Name = "二重枠")] DoubleFrame,
    [Display(Name = "下線")] Underline,
    [Display(Name = "かぎ括弧")] Brackets,
}

// =====================================================================
//  テロップ装飾
//   帯・吹き出し・叫び・タグ・リボン・枠・下線・かぎ括弧。文字は別のテキストを重ねる
// =====================================================================
public sealed class TelopDecoPlugin : IShapePlugin
{
    public string Name => "演出：テロップ装飾";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TelopDecoParameter(sharedData);
}

public sealed class TelopDecoParameter : EffectShapeParameterBase
{
    public TelopDecoParameter() : this(null) { }
    public TelopDecoParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "スタイル", Name = "スタイル")]
    [EnumComboBox]
    public TelopStyle Style { get => style; set => Set(ref style, value); }
    TelopStyle style = TelopStyle.Band;

    [Display(GroupName = "色", Name = "塗りの色")]
    [ColorPicker]
    public WpfColor FillColor { get => fill; set => Set(ref fill, value); }
    WpfColor fill = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "色", Name = "線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => line; set => Set(ref line, value); }
    WpfColor line = WpfColor.FromRgb(0x22, 0x22, 0x22);

    [Display(GroupName = "色", Name = "アクセントの色")]
    [ColorPicker]
    public WpfColor AccentColor { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0x4A, 0x6A);

    [Display(GroupName = "色", Name = "塗りの不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FillOpacity { get; } = new Animation(95, 0, 100);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(82, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "枠の幅")]
    [AnimationSlider("F0", "px", 100, 1800)]
    public Animation BoxWidth { get; } = new Animation(900, 20, 8000);

    [Display(GroupName = "位置と大きさ", Name = "枠の高さ")]
    [AnimationSlider("F0", "px", 30, 600)]
    public Animation BoxHeight { get; } = new Animation(150, 10, 4000);

    [Display(GroupName = "位置と大きさ", Name = "角の丸み")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation Corner { get; } = new Animation(30, 0, 1000);

    [Display(GroupName = "位置と大きさ", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0, 24)]
    public Animation LineWidth { get; } = new Animation(6, 0, 200);

    [Display(GroupName = "位置と大きさ", Name = "しっぽの位置（吹き出し）")]
    [AnimationSlider("F0", "%", -50, 50)]
    public Animation TailX { get; } = new Animation(-25, -100, 100);

    [Display(GroupName = "位置と大きさ", Name = "しっぽを上につける（吹き出し）")]
    [ToggleSlider]
    public bool TailUp { get => tailUp; set => Set(ref tailUp, value); }
    bool tailUp = false;

    [Display(GroupName = "効果", Name = "影をつける")]
    [ToggleSlider]
    public bool Shadow { get => shadow; set => Set(ref shadow, value); }
    bool shadow = true;

    [Display(GroupName = "効果", Name = "ふわふわ揺らす")]
    [ToggleSlider]
    public bool Bob { get => bob; set => Set(ref bob, value); }
    bool bob = false;

    [Display(GroupName = "効果", Name = "登場アニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "効果", Name = "登場にかかる時間")]
    [AnimationSlider("F2", "秒", 0.05, 3)]
    public Animation IntroTime { get; } = new Animation(0.4, 0.05, 30);

    [Display(GroupName = "効果", Name = "出現度（手動）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Appear { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [FillOpacity, PosX, PosY, BoxWidth, BoxHeight, Corner, LineWidth, TailX, IntroTime, Appear];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float ap = c.F(Appear) / 100f;
        if (Intro) ap *= Gfx.Smooth(0f, MathF.Max(0.05f, c.F(IntroTime)), t);
        if (ap <= 0.002f) return;
        float ease = 1f - (1f - ap) * (1f - ap) * (1f - ap);
        float sc = 0.82f + 0.18f * ease;

        float BW = c.F(BoxWidth) * sc, BH = c.F(BoxHeight) * sc;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        if (Bob) ctr.Y += MathF.Sin(t * 2.2f) * 6f;
        float L = ctr.X - BW / 2f, R = ctr.X + BW / 2f, T = ctr.Y - BH / 2f, B = ctr.Y + BH / 2f;
        float lw = c.F(LineWidth), corner = c.F(Corner);
        float fa = c.F(FillOpacity) / 100f * ap;

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- スタイルごとの外形 ----
        List<Vector2> poly;
        switch (Style)
        {
            case TelopStyle.Band:
                {
                    float s = BH * 0.35f;
                    poly = new List<Vector2> { new(L + s, T), new(R, T), new(R - s, B), new(L, B) };
                    break;
                }
            case TelopStyle.Shout:
                {
                    var rs = new Random(Seed + 5);
                    poly = new List<Vector2>();
                    int n = 30;
                    for (int i = 0; i < n; i++)
                    {
                        float a = i * MathF.PI * 2f / n;
                        float k = (i % 2 == 0) ? 1.0f : 0.80f + 0.06f * rs.NextF();
                        poly.Add(ctr + new Vector2(MathF.Cos(a) * BW / 2f * k, MathF.Sin(a) * BH / 2f * k));
                    }
                    break;
                }
            case TelopStyle.Tag:
                {
                    float tip = BH * 0.5f;
                    poly = new List<Vector2> { new(L, T), new(R - tip, T), new(R, ctr.Y), new(R - tip, B), new(L, B) };
                    break;
                }
            case TelopStyle.Ribbon:
                {
                    float tail = BW * 0.08f;
                    poly = new List<Vector2> { new(L + tail, T), new(R - tail, T), new(R - tail, B), new(L + tail, B) };
                    break;
                }
            case TelopStyle.DoubleFrame:
                {
                    float cs = MathF.Min(BH * 0.22f, 26f);
                    poly = new List<Vector2>
                    {
                        new(L + cs, T), new(R - cs, T), new(R, T + cs), new(R, B - cs), new(R - cs, B), new(L + cs, B), new(L, B - cs), new(L, T + cs),
                    };
                    break;
                }
            case TelopStyle.Underline:
            case TelopStyle.Brackets:
                poly = new List<Vector2> { new(L, T), new(R, T), new(R, B), new(L, B) };
                break;
            default:    // Bubble
                poly = ShapeGeom.RoundRect(ctr.X, ctr.Y, BW, BH, corner);
                break;
        }

        // ---- 影 ----
        if (Shadow && Style != TelopStyle.Underline)
        {
            var moved = poly.Select(p => p + new Vector2(6f, 9f)).ToList();
            brush.Color = Gfx.C4(WpfColor.FromRgb(0, 0, 0), 0.30f * ap);
            Gfx.FillPoly(dc, brush, moved);
        }

        brush.Color = Gfx.C4(FillColor, fa);
        var outline = ShapeGeom.Closed(poly);

        switch (Style)
        {
            case TelopStyle.Band:
                {
                    float s = BH * 0.35f, stripe = BH * 0.22f;
                    Gfx.FillPoly(dc, brush, poly);
                    brush.Color = Gfx.C4(AccentColor, ap);
                    Gfx.FillPoly(dc, brush, new List<Vector2> { new(L + s, T), new(L + s + stripe, T), new(L + stripe, B), new(L, B) });
                    brush.Color = Gfx.C4(LineColor, ap);
                    Gfx.Polyline(dc, brush, outline, lw);
                    brush.Color = Gfx.C4(AccentColor, ap);
                    Gfx.Rect(dc, brush, L + s * 0.4f, B + lw + 4f, BW * 0.55f, MathF.Max(3f, lw * 0.8f));
                    break;
                }
            case TelopStyle.Bubble:
                {
                    float dirY = TailUp ? -1f : 1f;
                    float edgeY = TailUp ? T : B;
                    float tx = ctr.X + c.F(TailX) / 100f * BW, tw = BH * 0.18f;
                    var tip = new Vector2(tx + BH * 0.12f, edgeY + dirY * BH * 0.38f);
                    Gfx.FillPoly(dc, brush, poly);
                    Gfx.FillPoly(dc, brush, new List<Vector2> { new(tx - tw, edgeY), new(tx + tw, edgeY), tip });
                    brush.Color = Gfx.C4(LineColor, ap);
                    Gfx.Polyline(dc, brush, outline, lw);
                    brush.Color = Gfx.C4(FillColor, fa);
                    dc.DrawLine(new Vector2(tx - tw + lw, edgeY), new Vector2(tx + tw - lw, edgeY), brush, lw * 1.3f);
                    brush.Color = Gfx.C4(LineColor, ap);
                    dc.DrawLine(new Vector2(tx - tw, edgeY), tip, brush, lw);
                    dc.DrawLine(tip, new Vector2(tx + tw, edgeY), brush, lw);
                    break;
                }
            case TelopStyle.Shout:
                Gfx.FillPoly(dc, brush, poly);
                brush.Color = Gfx.C4(LineColor, ap);
                Gfx.Polyline(dc, brush, outline, lw * 1.2f);
                break;
            case TelopStyle.Tag:
                {
                    Gfx.FillPoly(dc, brush, poly);
                    brush.Color = Gfx.C4(LineColor, ap);
                    Gfx.Polyline(dc, brush, outline, lw);
                    var hole = new Vector2(L + BH * 0.32f, ctr.Y);
                    brush.Color = Gfx.C4(AccentColor, ap);
                    dc.FillEllipse(new Ellipse(hole, BH * 0.12f, BH * 0.12f), brush);
                    brush.Color = Gfx.C4(LineColor, ap);
                    dc.DrawEllipse(new Ellipse(hole, BH * 0.12f, BH * 0.12f), brush, lw * 0.8f);
                    break;
                }
            case TelopStyle.Ribbon:
                {
                    float tail = BW * 0.08f, ofs = BH * 0.16f;
                    var dark = Gfx.Mix(AccentColor, WpfColor.FromRgb(0, 0, 0), 0.35f);
                    // 左右のしっぽ（燕尾）
                    var lt = new List<Vector2> { new(L, T + ofs), new(L + tail + 6f, T + ofs), new(L + tail + 6f, B + ofs), new(L, B + ofs), new(L + tail * 0.45f, ctr.Y + ofs) };
                    var rt = new List<Vector2> { new(R, T + ofs), new(R - tail - 6f, T + ofs), new(R - tail - 6f, B + ofs), new(R, B + ofs), new(R - tail * 0.45f, ctr.Y + ofs) };
                    brush.Color = Gfx.C4(AccentColor, ap);
                    Gfx.FillPoly(dc, brush, lt);
                    Gfx.FillPoly(dc, brush, rt);
                    brush.Color = Gfx.C4(dark, ap);
                    Gfx.FillPoly(dc, brush, new List<Vector2> { new(L + tail, B), new(L + tail + 6f, B), new(L + tail + 6f, B + ofs) });
                    Gfx.FillPoly(dc, brush, new List<Vector2> { new(R - tail, B), new(R - tail - 6f, B), new(R - tail - 6f, B + ofs) });
                    // 中央の帯
                    brush.Color = Gfx.C4(FillColor, fa);
                    Gfx.FillPoly(dc, brush, poly);
                    brush.Color = Gfx.C4(LineColor, ap);
                    Gfx.Polyline(dc, brush, outline, lw);
                    break;
                }
            case TelopStyle.DoubleFrame:
                {
                    Gfx.FillPoly(dc, brush, poly);
                    brush.Color = Gfx.C4(LineColor, ap);
                    Gfx.Polyline(dc, brush, outline, lw);
                    float g = MathF.Max(8f, lw * 2f);
                    float cs = MathF.Min(BH * 0.22f, 26f) * 0.7f;
                    var inner = new List<Vector2>
                    {
                        new(L + g + cs, T + g), new(R - g - cs, T + g), new(R - g, T + g + cs), new(R - g, B - g - cs),
                        new(R - g - cs, B - g), new(L + g + cs, B - g), new(L + g, B - g - cs), new(L + g, T + g + cs),
                    };
                    brush.Color = Gfx.C4(AccentColor, ap);
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(inner), MathF.Max(1.5f, lw * 0.5f));
                    foreach (var p in new[] { new Vector2(L, T), new Vector2(R, T), new Vector2(L, B), new Vector2(R, B) })
                        Gfx.Rect(dc, brush, p.X - 6f, p.Y - 6f, 12f, 12f);
                    break;
                }
            case TelopStyle.Underline:
                {
                    float y = B;
                    brush.Color = Gfx.C4(LineColor, ap);
                    Gfx.FadeLine(dc, brush, new Vector2(ctr.X, y), new Vector2(L, y), lw * 1.3f, lw * 0.5f, LineColor, ap, 12, 0.8f);
                    Gfx.FadeLine(dc, brush, new Vector2(ctr.X, y), new Vector2(R, y), lw * 1.3f, lw * 0.5f, LineColor, ap, 12, 0.8f);
                    brush.Color = Gfx.C4(AccentColor, ap);
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(new List<Vector2> { new(ctr.X, y - lw * 2f), new(ctr.X + lw * 2.2f, y), new(ctr.X, y + lw * 2f), new(ctr.X - lw * 2.2f, y) }), lw * 0.8f);
                    dc.FillEllipse(new Ellipse(new Vector2(L, y), lw * 1.2f, lw * 1.2f), brush);
                    dc.FillEllipse(new Ellipse(new Vector2(R, y), lw * 1.2f, lw * 1.2f), brush);
                    break;
                }
            default:    // Brackets
                {
                    Gfx.FillPoly(dc, brush, poly);
                    float arm = MathF.Min(BH * 0.28f, BW * 0.1f);
                    brush.Color = Gfx.C4(AccentColor, ap);
                    // 【 】：縦線と上下の短い横線
                    Gfx.Rect(dc, brush, L - lw, T, lw * 1.6f, BH);
                    Gfx.Rect(dc, brush, L - lw, T, arm, lw * 1.6f);
                    Gfx.Rect(dc, brush, L - lw, B - lw * 1.6f, arm, lw * 1.6f);
                    Gfx.Rect(dc, brush, R - lw * 0.6f, T, lw * 1.6f, BH);
                    Gfx.Rect(dc, brush, R + lw - arm, T, arm, lw * 1.6f);
                    Gfx.Rect(dc, brush, R + lw - arm, B - lw * 1.6f, arm, lw * 1.6f);
                    break;
                }
        }
    }
}

public enum TransKind
{
    [Display(Name = "横ワイプ")] WipeH,
    [Display(Name = "縦ワイプ")] WipeV,
    [Display(Name = "斜めワイプ")] WipeDiag,
    [Display(Name = "円形（広がる）")] CircleOpen,
    [Display(Name = "円形（すぼまる）")] IrisClose,
    [Display(Name = "ブラインド（縦）")] BlindsV,
    [Display(Name = "ブラインド（横）")] BlindsH,
    [Display(Name = "マス目")] Grid,
    [Display(Name = "時計ワイプ")] Clock,
    [Display(Name = "丸の連続")] Dots,
    [Display(Name = "カーテン（舞台の幕）")] Curtain,
    [Display(Name = "本のページめくり風")] PageTurn,
    [Display(Name = "吹き出しがパンッと広がる")] Bubble,
    [Display(Name = "スタンプがドン")] Stamp,
    [Display(Name = "インクの飛び散り")] Ink,
    [Display(Name = "煙幕")] Smoke,
    [Display(Name = "爆発転換")] Explosion,
    [Display(Name = "集中線ズーム")] SpeedLines,
    [Display(Name = "斬撃で割れる")] Slash,
    [Display(Name = "星形のアイリス")] StarIris,
    [Display(Name = "ハート形のアイリス")] HeartIris,
    [Display(Name = "ダイヤ（菱形）の連続")] Diamonds,
    [Display(Name = "六角形のタイル")] Hexagons,
    [Display(Name = "三角形の割れ")] Triangles,
    [Display(Name = "斜めの帯が数本走る")] Bands,
    [Display(Name = "ジグザグワイプ")] Zigzag,
    [Display(Name = "波形ワイプ")] WaveWipe,
    [Display(Name = "ブラウン管オフ")] CrtOff,
    [Display(Name = "ドット・ピクセル化")] Pixelate,
    [Display(Name = "グリッチ転換")] Glitch,
    [Display(Name = "データのローディング")] Loading,
}
