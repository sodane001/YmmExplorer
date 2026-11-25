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

public enum MangaKind
{
    [Display(Name = "集中線")] FocusLines,
    [Display(Name = "スピード線")] SpeedLines,
    [Display(Name = "衝撃（ドーン）")] Impact,
    [Display(Name = "キラキラ")] Sparkle,
    [Display(Name = "怒りマーク")] Anger,
    [Display(Name = "汗")] Sweat,
    [Display(Name = "ハート")] Hearts,
    [Display(Name = "ビックリ")] Surprise,
}

// =====================================================================
//  漫画エフェクト
//   集中線 / スピード線 / 衝撃 / キラキラ / 怒り / 汗 / ハート / ビックリ
// =====================================================================
public sealed class MangaFxPlugin : IShapePlugin
{
    public string Name => "演出：漫画エフェクト";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MangaFxParameter(sharedData);
}

public sealed class MangaFxParameter : EffectShapeParameterBase
{
    public MangaFxParameter() : this(null) { }
    public MangaFxParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "エフェクトの種類")]
    [EnumComboBox]
    public MangaKind Kind { get => kind; set => Set(ref kind, value); }
    MangaKind kind = MangaKind.FocusLines;

    [Display(GroupName = "色", Name = "線・縁取りの色")]
    [ColorPicker]
    public WpfColor LineColor { get => line; set => Set(ref line, value); }
    WpfColor line = WpfColor.FromRgb(0x11, 0x11, 0x11);

    [Display(GroupName = "色", Name = "塗りの色", Description = "衝撃・キラキラ・ビックリの塗り")]
    [ColorPicker]
    public WpfColor FillColor { get => fill; set => Set(ref fill, value); }
    WpfColor fill = WpfColor.FromRgb(0xFF, 0xE9, 0x4A);

    [Display(GroupName = "色", Name = "アクセントの色", Description = "怒り=赤、ハート=ピンク、汗=水色 など")]
    [ColorPicker]
    public WpfColor AccentColor { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0x5A, 0x8A);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ", Description = "集中線は中央の空きの半径、他は図形の大きさ")]
    [AnimationSlider("F0", "px", 20, 800)]
    public Animation Size { get; } = new Animation(300, 3, 8000);

    [Display(GroupName = "位置と大きさ", Name = "数・本数")]
    [AnimationSlider("F0", "個", 3, 300)]
    public Animation Count { get; } = new Animation(90, 1, 2000);

    [Display(GroupName = "位置と大きさ", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 40)]
    public Animation Thickness { get; } = new Animation(14, 0.5, 400);

    [Display(GroupName = "位置と大きさ", Name = "向き（スピード線）", Description = "0=右へ、90=下へ")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Direction { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "動き", Name = "コマ送りの速さ", Description = "1秒あたりの描き直し回数。0でなめらか")]
    [AnimationSlider("F0", "回/秒", 0, 30)]
    public Animation FrameRate { get; } = new Animation(12, 0, 120);

    [Display(GroupName = "動き", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Size, Count, Thickness, Direction, FrameRate, Opacity];

    static List<Vector2> Heart(Vector2 c, float r)
    {
        var pts = new List<Vector2>();
        float s = r / 16f;
        for (int i = 0; i < 64; i++)
        {
            float tt = i / 64f * MathF.PI * 2f;
            float x = 16f * MathF.Pow(MathF.Sin(tt), 3f);
            float y = -(13f * MathF.Cos(tt) - 5f * MathF.Cos(2f * tt) - 2f * MathF.Cos(3f * tt) - MathF.Cos(4f * tt));
            pts.Add(c + new Vector2(x, y + 1.5f) * s);
        }
        return pts;
    }

    static List<Vector2> Star4(Vector2 c, float r, float inner)
    {
        var pts = new List<Vector2>();
        for (int i = 0; i < 8; i++)
        {
            float a = -MathF.PI / 2f + i * MathF.PI / 4f;
            float rr = (i % 2 == 0) ? r : r * inner;
            pts.Add(c + new Vector2(MathF.Cos(a), MathF.Sin(a)) * rr);
        }
        return pts;
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float size = c.F(Size), th = c.F(Thickness), op = c.F(Opacity) / 100f;
        int n = (int)c.F(Count);
        float rate = c.F(FrameRate);
        int tick = rate > 0f ? (int)MathF.Floor(t * rate) : 0;
        float tm = rate > 0f ? tick / rate : t;                          // コマ送りの時刻
        var rng = new Random(Seed * 7919 + tick * 131);
        var white = WpfColor.FromRgb(255, 255, 255);
        float diag = MathF.Sqrt(w * w + h * h);

        using var brush = dc.CreateSolidColorBrush(default);
        switch (Kind)
        {
            case MangaKind.FocusLines:
                {
                    // 中心に向かって細くなる三角形の線
                    for (int i = 0; i < n; i++)
                    {
                        float ang = (i + rng.NextF() * 0.6f) / n * MathF.PI * 2f;
                        var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                        float r0 = size * (0.9f + 0.4f * rng.NextF());
                        float r1 = diag;
                        float w1 = th * (0.35f + 1.3f * rng.NextF());
                        brush.Color = Gfx.C4(LineColor, op);
                        const int S = 6;
                        for (int s = 0; s < S; s++)
                        {
                            float f0 = s / (float)S, f1 = (s + 1) / (float)S;
                            dc.DrawLine(ctr + dir * (r0 + (r1 - r0) * f0), ctr + dir * (r0 + (r1 - r0) * f1), brush, MathF.Max(0.6f, w1 * (f0 + f1) / 2f));
                        }
                    }
                    break;
                }
            case MangaKind.SpeedLines:
                {
                    float ang = Gfx.Rad(c.F(Direction));
                    var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                    var perp = new Vector2(-dir.Y, dir.X);
                    float span = diag + 600f;
                    var rs = new Random(Seed);
                    for (int i = 0; i < n; i++)
                    {
                        float v = (rs.NextF() * 2f - 1f) * diag / 2f;
                        float u0 = rs.NextF() * span, len = size * (0.5f + 1.5f * rs.NextF());
                        float sp = 900f + 1400f * rs.NextF();
                        float wd = th * (0.3f + 0.9f * rs.NextF());
                        float u = Gfx.Mod(u0 + tm * sp, span) - span / 2f;
                        var center = new Vector2(w / 2f, h / 2f);
                        brush.Color = Gfx.C4(LineColor, op * (0.5f + 0.5f * rs.NextF()));
                        const int S = 6;
                        for (int s = 0; s < S; s++)
                        {
                            float f0 = s / (float)S, f1 = (s + 1) / (float)S;
                            float mid = (f0 + f1) / 2f;
                            float taper = 1f - MathF.Abs(mid * 2f - 1f);        // 両端が細い
                            var pa = center + dir * (u - len / 2f + len * f0) + perp * v;
                            var pb = center + dir * (u - len / 2f + len * f1) + perp * v;
                            dc.DrawLine(pa, pb, brush, MathF.Max(0.5f, wd * (0.2f + taper)));
                        }
                    }
                    break;
                }
            case MangaKind.Impact:
                {
                    int spikes = Math.Max(6, n / 5);
                    var pts = new List<Vector2>();
                    for (int i = 0; i < spikes * 2; i++)
                    {
                        float a = i * MathF.PI / spikes + (rng.NextF() - 0.5f) * 0.08f;
                        float rr = (i % 2 == 0) ? size * (0.85f + 0.3f * rng.NextF()) : size * (0.50f + 0.12f * rng.NextF());
                        pts.Add(ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * rr);
                    }
                    brush.Color = Gfx.C4(FillColor, op);
                    Gfx.FillPoly(dc, brush, pts);
                    var closed = new List<Vector2>(pts) { pts[0] };
                    brush.Color = Gfx.C4(LineColor, op);
                    Gfx.Polyline(dc, brush, closed, th * 0.5f);
                    // 外側に飛ぶ短い線
                    for (int i = 0; i < spikes; i++)
                    {
                        float a = i * MathF.PI * 2f / spikes + rng.NextF() * 0.2f;
                        var dir = new Vector2(MathF.Cos(a), MathF.Sin(a));
                        dc.DrawLine(ctr + dir * (size * 1.15f), ctr + dir * (size * (1.3f + 0.25f * rng.NextF())), brush, th * 0.4f);
                    }
                    break;
                }
            case MangaKind.Sparkle:
                {
                    int cnt = Math.Max(1, n / 6);
                    var rs = new Random(Seed);
                    for (int i = 0; i < cnt; i++)
                    {
                        float a = rs.NextF() * MathF.PI * 2f, d = size * 2f * MathF.Sqrt(rs.NextF());
                        float r0 = size * (0.12f + 0.25f * rs.NextF());
                        float f = 0.8f + 1.6f * rs.NextF(), ph = rs.NextF() * MathF.PI * 2f;
                        float s = MathF.Pow(MathF.Max(0f, MathF.Sin(tm * f * 2f + ph)), 0.8f);
                        if (s < 0.05f) continue;
                        var p = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * d;
                        var pts = Star4(p, r0 * s, 0.2f);
                        brush.Color = Gfx.C4(FillColor, op);
                        Gfx.FillPoly(dc, brush, pts);
                        var closed = new List<Vector2>(pts) { pts[0] };
                        brush.Color = Gfx.C4(LineColor, op * 0.8f);
                        Gfx.Polyline(dc, brush, closed, MathF.Max(1f, th * 0.12f));
                    }
                    break;
                }
            case MangaKind.Anger:
                {
                    // 💢：4本の弧を十字に並べる
                    float pulse = 1f + 0.10f * MathF.Sin(tm * 9f);
                    float r = size * 0.5f * pulse;
                    brush.Color = Gfx.C4(AccentColor, op);
                    for (int i = 0; i < 4; i++)
                    {
                        float baseA = i * MathF.PI / 2f + MathF.PI / 4f;
                        var side = ctr + new Vector2(MathF.Cos(baseA), MathF.Sin(baseA)) * (r * 0.62f);
                        // 中心に向かって膨らむ弧
                        var arc = Gfx.ArcPts(side + new Vector2(MathF.Cos(baseA), MathF.Sin(baseA)) * (r * 0.55f), r * 0.78f,
                            baseA + MathF.PI - 0.95f, baseA + MathF.PI + 0.95f, 24);
                        Gfx.Polyline(dc, brush, arc, th * 1.2f);
                    }
                    break;
                }
            case MangaKind.Sweat:
                {
                    int drops = Math.Clamp(n / 20, 2, 8);
                    for (int i = 0; i < drops; i++)
                    {
                        float u = Gfx.Mod(tm * 0.8f + i / (float)drops, 1f);
                        float a = Gfx.Rad(-60f + i * (120f / drops));
                        var outDir = new Vector2(MathF.Cos(a), MathF.Sin(a));
                        var p = ctr + outDir * (size * (0.7f + 0.7f * u));
                        float r = size * 0.09f * (0.8f + 0.4f * MathF.Sin(i * 2.1f));
                        float alpha = MathF.Sin(u * MathF.PI) * op;
                        var tipDir = -outDir;
                        var tip = p + tipDir * (r * 2.3f);
                        float cs = MathF.Cos(1.35f), sn = MathF.Sin(1.35f);
                        var b1 = p + new Vector2(tipDir.X * cs - tipDir.Y * sn, tipDir.X * sn + tipDir.Y * cs) * (r * 0.98f);
                        var b2 = p + new Vector2(tipDir.X * cs + tipDir.Y * sn, -tipDir.X * sn + tipDir.Y * cs) * (r * 0.98f);
                        brush.Color = Gfx.C4(AccentColor, alpha);
                        dc.FillEllipse(new Ellipse(p, r, r), brush);
                        Gfx.FillPoly(dc, brush, new List<Vector2> { tip, b1, b2 });
                        brush.Color = Gfx.C4(white, alpha * 0.8f);
                        dc.FillEllipse(new Ellipse(p + new Vector2(-r * 0.3f, r * 0.1f), r * 0.2f, r * 0.2f), brush);
                    }
                    break;
                }
            case MangaKind.Hearts:
                {
                    int cnt = Math.Max(1, n / 8);
                    var rs = new Random(Seed);
                    for (int i = 0; i < cnt; i++)
                    {
                        float ph = rs.NextF(), sw = rs.NextF() * MathF.PI * 2f, sc = 0.5f + 0.7f * rs.NextF();
                        float u = Gfx.Mod(tm * 0.35f + ph, 1f);
                        var p = ctr + new Vector2(MathF.Sin(u * 7f + sw) * size * 0.35f + (rs.NextF() - 0.5f) * size, -u * size * 1.8f);
                        float r = size * 0.16f * sc * (0.5f + 0.5f * MathF.Min(1f, u * 5f));
                        float alpha = MathF.Sin(u * MathF.PI) * op;
                        var pts = Heart(p, r);
                        brush.Color = Gfx.C4(AccentColor, alpha);
                        Gfx.FillPoly(dc, brush, pts);
                        var closed = new List<Vector2>(pts) { pts[0] };
                        brush.Color = Gfx.C4(LineColor, alpha * 0.7f);
                        Gfx.Polyline(dc, brush, closed, MathF.Max(1f, th * 0.15f));
                        brush.Color = Gfx.C4(white, alpha * 0.6f);
                        dc.FillEllipse(new Ellipse(p + new Vector2(-r * 0.4f, -r * 0.35f), r * 0.14f, r * 0.1f), brush);
                    }
                    break;
                }
            default:    // Surprise
                {
                    var jit = new Vector2((rng.NextF() - 0.5f) * 6f, (rng.NextF() - 0.5f) * 6f);
                    var o = ctr + jit;
                    float hh = size * 0.9f, wt = size * 0.28f, wb = size * 0.12f;
                    var bar = new List<Vector2>
                    {
                        new(o.X - wt / 2f, o.Y - hh / 2f), new(o.X + wt / 2f, o.Y - hh / 2f),
                        new(o.X + wb / 2f, o.Y + hh * 0.22f), new(o.X - wb / 2f, o.Y + hh * 0.22f),
                    };
                    brush.Color = Gfx.C4(FillColor, op);
                    Gfx.FillPoly(dc, brush, bar);
                    var dot = new Vector2(o.X, o.Y + hh * 0.42f);
                    dc.FillEllipse(new Ellipse(dot, size * 0.1f, size * 0.1f), brush);
                    brush.Color = Gfx.C4(LineColor, op);
                    Gfx.Polyline(dc, brush, new List<Vector2>(bar) { bar[0] }, MathF.Max(2f, th * 0.3f));
                    dc.DrawEllipse(new Ellipse(dot, size * 0.1f, size * 0.1f), brush, MathF.Max(2f, th * 0.3f));
                    // 周囲に走る短い線
                    for (int i = 0; i < 5; i++)
                    {
                        float a = -MathF.PI / 2f + (i - 2) * 0.55f;
                        var dir = new Vector2(MathF.Cos(a), MathF.Sin(a));
                        dc.DrawLine(o + dir * (size * 0.85f), o + dir * (size * (1.1f + 0.1f * rng.NextF())), brush, th * 0.35f);
                    }
                    break;
                }
        }
    }
}
