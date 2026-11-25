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

public enum PanelStyle
{
    [Display(Name = "スタイル1：斜め切り＋外側のライン")] Style1,
    [Display(Name = "スタイル2：斜め切り＋コーナーのL字")] Style2,
    [Display(Name = "スタイル3：斜め切り＋太いバーと斜線")] Style3,
    [Display(Name = "スタイル4：切り欠き＋中央のバー")] Style4,
    [Display(Name = "スタイル5：四隅を切って三角の飾り")] Style5,
    [Display(Name = "スタイル6：四隅の丸リング")] Style6,
    [Display(Name = "スタイル7：四隅のブラケット")] Style7,
    [Display(Name = "スタイル8：段差＋バーと斜線")] Style8,
    [Display(Name = "スタイル9：四隅を切って上下のバー")] Style9,
}

// =====================================================================
//  近未来図形
//   半透明のパネルと、アクセントラインで作るHUD風のフレーム（9スタイル）
// =====================================================================
public sealed class FuturePanelPlugin : IShapePlugin
{
    public string Name => "演出：近未来図形";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FuturePanelParameter(sharedData);
}

public sealed class FuturePanelParameter : EffectShapeParameterBase
{
    public FuturePanelParameter() : this(null) { }
    public FuturePanelParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "フレーム", Name = "スタイル")]
    [EnumComboBox]
    public PanelStyle Style { get => style; set => Set(ref style, value); }
    PanelStyle style = PanelStyle.Style1;

    [Display(GroupName = "位置とサイズ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置とサイズ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置とサイズ", Name = "パネルの幅")]
    [AnimationSlider("F0", "px", 100, 1800)]
    public Animation PanelWidth { get; } = new Animation(760, 20, 8000);

    [Display(GroupName = "位置とサイズ", Name = "パネルの高さ")]
    [AnimationSlider("F0", "px", 60, 1000)]
    public Animation PanelHeight { get; } = new Animation(400, 20, 8000);

    [Display(GroupName = "色", Name = "線とアクセントの色")]
    [ColorPicker]
    public WpfColor LineColor { get => line; set => Set(ref line, value); }
    WpfColor line = WpfColor.FromRgb(0x5B, 0xE5, 0xE0);

    [Display(GroupName = "色", Name = "パネルの色")]
    [ColorPicker]
    public WpfColor FillColor { get => fill; set => Set(ref fill, value); }
    WpfColor fill = WpfColor.FromRgb(0x2F, 0x6A, 0x78);

    [Display(GroupName = "色", Name = "パネルの不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FillOpacity { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "線", Name = "細い線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 6)]
    public Animation Thickness { get; } = new Animation(1.6, 0.2, 50);

    [Display(GroupName = "線", Name = "アクセントの太さ")]
    [AnimationSlider("F0", "px", 2, 30)]
    public Animation AccentThickness { get; } = new Animation(9, 1, 200);

    [Display(GroupName = "線", Name = "角の斜め切りの大きさ")]
    [AnimationSlider("F0", "px", 5, 120)]
    public Animation Chamfer { get; } = new Animation(36, 1, 1000);

    [Display(GroupName = "線", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glow { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "背景", Name = "グリッドを表示する")]
    [ToggleSlider]
    public bool ShowGrid { get => showGrid; set => Set(ref showGrid, value); }
    bool showGrid = true;

    [Display(GroupName = "背景", Name = "グリッドの間隔")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation GridSize { get; } = new Animation(40, 4, 1000);

    [Display(GroupName = "動き", Name = "登場アニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "登場にかかる時間")]
    [AnimationSlider("F2", "秒", 0.1, 3)]
    public Animation IntroTime { get; } = new Animation(0.7, 0.05, 20);

    [Display(GroupName = "動き", Name = "出現度（手動）", Description = "0→100%で出現。キーフレームで自由に出し入れできる")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Appear { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "動き", Name = "走査線")]
    [ToggleSlider]
    public bool ScanLine { get => scan; set => Set(ref scan, value); }
    bool scan = true;

    [Display(GroupName = "動き", Name = "アクセントの明滅")]
    [ToggleSlider]
    public bool Blink { get => blink; set => Set(ref blink, value); }
    bool blink = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, PanelWidth, PanelHeight, FillOpacity, Thickness, AccentThickness, Chamfer, Glow, GridSize, IntroTime, Appear];

    // ---- 多角形の塗りつぶし（1pxの行ごとに内側を塗る）----
    static void FillPoly(ID2D1DeviceContext dc, ID2D1Brush brush, IList<Vector2> p)
    {
        if (p.Count < 3) return;
        if (GeoKit.FillPoly(dc, brush, p)) return;
        float minY = p.Min(v => v.Y), maxY = p.Max(v => v.Y);
        int y0 = (int)MathF.Floor(minY), y1 = (int)MathF.Ceiling(maxY);
        var xs = new List<float>();
        for (int y = y0; y < y1; y++)
        {
            float sy = y + 0.5f;
            xs.Clear();
            for (int i = 0; i < p.Count; i++)
            {
                var a = p[i];
                var b = p[(i + 1) % p.Count];
                if ((a.Y <= sy && b.Y > sy) || (b.Y <= sy && a.Y > sy))
                {
                    float tt = (sy - a.Y) / (b.Y - a.Y);
                    xs.Add(a.X + (b.X - a.X) * tt);
                }
            }
            xs.Sort();
            for (int k = 0; k + 1 < xs.Count; k += 2)
                dc.FillRectangle(new RawRectF(xs[k], y, xs[k + 1], y + 1f), brush);
        }
    }

    static void RectXY(ID2D1DeviceContext dc, ID2D1Brush brush, float x0, float y0, float x1, float y1)
        => dc.FillRectangle(new RawRectF(MathF.Min(x0, x1), MathF.Min(y0, y1), MathF.Max(x0, x1), MathF.Max(y0, y1)), brush);

    /// <summary>L字のブラケット。sx,syは角から内側へ向かう向き</summary>
    static void Bracket(ID2D1DeviceContext dc, ID2D1Brush brush, Vector2 corner, float sx, float sy, float len, float th)
    {
        RectXY(dc, brush, corner.X, corner.Y, corner.X + sx * len, corner.Y + sy * th);
        RectXY(dc, brush, corner.X, corner.Y, corner.X + sx * th, corner.Y + sy * len);
    }

    /// <summary>太いバー（左端を斜めに切った平行四辺形）</summary>
    static void SlantBar(ID2D1DeviceContext dc, ID2D1Brush brush, float xa, float xb, float yc, float th, float slant)
    {
        var q = new List<Vector2>
        {
            new(xa, yc - th / 2f), new(xb, yc - th / 2f),
            new(xb - slant, yc + th / 2f), new(xa + slant * 0f, yc + th / 2f),
        };
        // 左端を斜めにするため、下側の左端を右へずらす
        q[3] = new Vector2(xa + slant, yc + th / 2f);
        q[2] = new Vector2(xb, yc + th / 2f);
        FillPoly(dc, brush, q);
    }

    /// <summary>斜線のマーク（///）</summary>
    static void Hash(ID2D1DeviceContext dc, ID2D1Brush brush, float x, float yc, int count, float th)
    {
        for (int i = 0; i < count; i++)
        {
            float xi = x + i * th * 1.7f;
            var q = new List<Vector2>
            {
                new(xi + th * 0.8f, yc - th * 0.6f), new(xi + th * 1.5f, yc - th * 0.6f),
                new(xi + th * 0.7f, yc + th * 0.6f), new(xi, yc + th * 0.6f),
            };
            FillPoly(dc, brush, q);
        }
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;

        // ---- 登場アニメーション ----
        float appear = c.F(Appear) / 100f;
        if (Intro) appear *= Gfx.Smooth(0f, MathF.Max(0.05f, c.F(IntroTime)), t);
        if (appear <= 0.002f) return;
        float ease = 1f - (1f - appear) * (1f - appear) * (1f - appear);
        float scale = 0.7f + 0.3f * ease;

        float PW = c.F(PanelWidth) * scale, PH = c.F(PanelHeight) * scale;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float L = ctr.X - PW / 2f, R = ctr.X + PW / 2f, T = ctr.Y - PH / 2f, B = ctr.Y + PH / 2f;

        float tn = c.F(Thickness), ac = c.F(AccentThickness);
        float cm = Math.Clamp(c.F(Chamfer), 2f, MathF.Min(PW, PH) * 0.4f);
        float o = ac * 0.9f;                                   // 外側への飛び出し量
        float glowK = c.F(Glow) / 100f;
        float a = appear;
        float flick = Blink ? 0.78f + 0.22f * MathF.Sin(t * 9f) * MathF.Sin(t * 2.3f + 1f) : 1f;
        var lc = LineColor;

        // ---- スタイルごとのパネル外形 ----
        List<Vector2> poly;
        switch (Style)
        {
            case PanelStyle.Style4:
                {
                    float n = cm * 0.5f, cs = cm * 0.4f, fx = PW * 0.30f, fy = PH * 0.30f;
                    poly = new List<Vector2>
                    {
                        new(L + cs, T), new(L + fx, T), new(L + fx + n, T + n), new(R - fx - n, T + n), new(R - fx, T), new(R - cs, T),
                        new(R, T + cs), new(R, T + fy), new(R - n, T + fy + n), new(R - n, B - fy - n), new(R, B - fy), new(R, B - cs),
                        new(R - cs, B), new(R - fx, B), new(R - fx - n, B - n), new(L + fx + n, B - n), new(L + fx, B), new(L + cs, B),
                        new(L, B - cs), new(L, B - fy), new(L + n, B - fy - n), new(L + n, T + fy + n), new(L, T + fy), new(L, T + cs),
                    };
                    break;
                }
            case PanelStyle.Style5:
                {
                    float cs = cm * 0.8f;
                    poly = new List<Vector2>
                    {
                        new(L + cs, T), new(R - cs, T), new(R, T + cs), new(R, B - cs), new(R - cs, B), new(L + cs, B), new(L, B - cs), new(L, T + cs),
                    };
                    break;
                }
            case PanelStyle.Style6:
            case PanelStyle.Style7:
                poly = new List<Vector2> { new(L, T), new(R, T), new(R, B), new(L, B) };
                break;
            case PanelStyle.Style8:
                {
                    float s = cm * 0.45f;
                    poly = new List<Vector2>
                    {
                        new(L + cm * 1.3f, T + s), new(L + PW * 0.58f, T + s), new(L + PW * 0.58f + s, T), new(R, T),
                        new(R, B - cm), new(R - cm, B), new(L + cm, B), new(L, B - cm), new(L, T + s + cm * 1.3f),
                    };
                    break;
                }
            case PanelStyle.Style9:
                {
                    float cs = cm * 0.7f;
                    poly = new List<Vector2>
                    {
                        new(L + cs, T), new(R - cs, T), new(R, T + cs), new(R, B - cs), new(R - cs, B), new(L + cs, B), new(L, B - cs), new(L, T + cs),
                    };
                    break;
                }
            default:    // スタイル1〜3：左上と右下を斜めに切る
                poly = new List<Vector2>
                {
                    new(L + cm, T), new(R, T), new(R, B - cm), new(R - cm, B), new(L, B), new(L, T + cm),
                };
                break;
        }

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 背景のぼんやりした光 ----
        float gr = MathF.Max(PW, PH) * 0.8f;
        using (var gl = Gfx.Radial(dc, ctr, gr, (0f, Gfx.C4(lc, 0.08f * glowK * a)), (1f, Gfx.C4(lc, 0f))))
            dc.FillEllipse(new Ellipse(ctr, gr, gr), gl);

        // ---- パネルの塗り ----
        brush.Color = Gfx.C4(FillColor, c.F(FillOpacity) / 100f * a);
        FillPoly(dc, brush, poly);

        // ---- グリッド ----
        if (ShowGrid)
        {
            float gs = MathF.Max(4f, c.F(GridSize));
            brush.Color = Gfx.C4(lc, 0.08f * a);
            for (float x = L + gs; x < R; x += gs) dc.DrawLine(new Vector2(x, T + 1f), new Vector2(x, B - 1f), brush, 1f);
            for (float y = T + gs; y < B; y += gs) dc.DrawLine(new Vector2(L + 1f, y), new Vector2(R - 1f, y), brush, 1f);
        }

        // ---- 走査線 ----
        if (ScanLine)
        {
            float sy = T + Gfx.Mod(t * PH * 0.45f, PH + 60f) - 30f;
            for (int i = 0; i < 10; i++)
            {
                float yy = sy - i * 3f;
                if (yy < T || yy > B) continue;
                brush.Color = Gfx.C4(lc, (i == 0 ? 0.40f : 0.16f * (1f - i / 10f)) * a);
                dc.DrawLine(new Vector2(L + 2f, yy), new Vector2(R - 2f, yy), brush, i == 0 ? 1.6f : 3f);
            }
        }

        // ---- 外形の線（にじみ＋細い線）----
        var closed = new List<Vector2>(poly) { poly[0] };
        brush.Color = Gfx.C4(lc, 0.14f * glowK * a);
        Gfx.Polyline(dc, brush, closed, tn * 6f);
        brush.Color = Gfx.C4(lc, 0.95f * a);
        Gfx.Polyline(dc, brush, closed, tn);

        // ---- アクセント ----
        brush.Color = Gfx.C4(lc, 0.95f * a * flick);
        switch (Style)
        {
            case PanelStyle.Style1:
                {
                    // 左上の斜め切りに沿った外側のライン
                    var d0 = new Vector2(L - o, T + cm - o);
                    var d1 = new Vector2(L + cm - o, T - o);
                    dc.DrawLine(Vector2.Lerp(d0, d1, 0.12f), Vector2.Lerp(d0, d1, 0.88f), brush, ac * 0.7f);
                    // 右の縦ラインと、右下の斜めライン
                    dc.DrawLine(new Vector2(R + o, T + cm * 0.3f), new Vector2(R + o, B - cm - o * 0.3f), brush, ac * 0.7f);
                    var e0 = new Vector2(R + o, B - cm + o);
                    var e1 = new Vector2(R - cm + o, B + o);
                    dc.DrawLine(Vector2.Lerp(e0, e1, 0.1f), Vector2.Lerp(e0, e1, 0.85f), brush, ac * 0.7f);
                    break;
                }
            case PanelStyle.Style2:
                Bracket(dc, brush, new Vector2(R + o * 0.4f, T - o * 0.4f), -1f, 1f, cm * 1.2f, ac);
                Bracket(dc, brush, new Vector2(L - o * 0.4f, B + o * 0.4f), 1f, -1f, cm * 1.7f, ac);
                break;
            case PanelStyle.Style3:
                SlantBar(dc, brush, R - PW * 0.30f, R, T, ac, ac);
                SlantBar(dc, brush, L, L + PW * 0.42f, B, ac, 0f);
                Hash(dc, brush, L + PW * 0.50f, B, 3, ac);
                break;
            case PanelStyle.Style4:
                {
                    float n = cm * 0.5f, fx = PW * 0.30f, fy = PH * 0.30f;
                    RectXY(dc, brush, L + fx + n * 0.2f, T + n - ac / 2f, R - fx - n * 0.2f, T + n + ac / 2f);
                    RectXY(dc, brush, L + fx + n * 0.2f, B - n - ac / 2f, R - fx - n * 0.2f, B - n + ac / 2f);
                    RectXY(dc, brush, L + n - ac / 2f, T + fy + n * 1.2f, L + n + ac / 2f, B - fy - n * 1.2f);
                    RectXY(dc, brush, R - n - ac / 2f, T + fy + n * 1.2f, R - n + ac / 2f, B - fy - n * 1.2f);
                    break;
                }
            case PanelStyle.Style5:
                {
                    float e = ac * 0.7f, tl = cm * 1.0f;
                    FillPoly(dc, brush, new List<Vector2> { new(L - e, T - e), new(L - e + tl, T - e), new(L - e, T - e + tl) });
                    FillPoly(dc, brush, new List<Vector2> { new(R + e, T - e), new(R + e - tl, T - e), new(R + e, T - e + tl) });
                    FillPoly(dc, brush, new List<Vector2> { new(L - e, B + e), new(L - e + tl, B + e), new(L - e, B + e - tl) });
                    FillPoly(dc, brush, new List<Vector2> { new(R + e, B + e), new(R + e - tl, B + e), new(R + e, B + e - tl) });
                    break;
                }
            case PanelStyle.Style6:
                {
                    float rr = ac * 1.7f;
                    foreach (var p in new[] { new Vector2(L, T), new Vector2(R, T), new Vector2(L, B), new Vector2(R, B) })
                    {
                        dc.DrawEllipse(new Ellipse(p, rr, rr), brush, ac * 0.7f);
                    }
                    break;
                }
            case PanelStyle.Style7:
                {
                    float bl = MathF.Min(PW, PH) * 0.16f, e = ac * 0.3f;
                    Bracket(dc, brush, new Vector2(L - e, T - e), 1f, 1f, bl, ac);
                    Bracket(dc, brush, new Vector2(R + e, T - e), -1f, 1f, bl, ac);
                    Bracket(dc, brush, new Vector2(L - e, B + e), 1f, -1f, bl, ac);
                    Bracket(dc, brush, new Vector2(R + e, B + e), -1f, -1f, bl, ac);
                    break;
                }
            case PanelStyle.Style8:
                {
                    float s = cm * 0.45f;
                    SlantBar(dc, brush, R - PW * 0.26f, R, T, ac, ac);
                    Hash(dc, brush, R - PW * 0.26f - ac * 5.4f, T, 3, ac);
                    dc.DrawLine(new Vector2(L - o, T + s + cm * 1.3f), new Vector2(L - o, B - cm * 0.5f), brush, ac * 0.7f);
                    SlantBar(dc, brush, L + cm * 0.4f, L + PW * 0.40f, B + o * 0.9f, ac * 0.8f, ac * 0.8f);
                    break;
                }
            case PanelStyle.Style9:
                RectXY(dc, brush, L + PW * 0.40f, T - ac / 2f, R - PW * 0.12f, T + ac / 2f);
                RectXY(dc, brush, L + PW * 0.12f, B - ac / 2f, R - PW * 0.40f, B + ac / 2f);
                break;
        }
    }
}
