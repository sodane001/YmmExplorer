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
//  チャンネル登録ボタン
//   マウスのカーソルがやって来てボタンを押し、「登録済み」に変わる。ベル・高評価も押せる。
//   (特定のサービスのロゴは使わない、汎用のデザイン)
// =====================================================================
public sealed class SubscribeButtonPlugin : IShapePlugin
{
    public string Name => "演出：チャンネル登録ボタン";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SubscribeButtonParameter(sharedData);
}

public sealed class SubscribeButtonParameter : EffectShapeParameterBase
{
    public SubscribeButtonParameter() : this(null) { }
    public SubscribeButtonParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "ボタンの文字")]
    [TextEditor(AcceptsReturn = false)]
    public string Label { get => label; set => Set(ref label, value); }
    string label = "チャンネル登録";

    [Display(GroupName = "文字", Name = "押した後の文字")]
    [TextEditor(AcceptsReturn = false)]
    public string DoneLabel { get => doneLabel; set => Set(ref doneLabel, value); }
    string doneLabel = "登録済み";

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "ボタンの色")]
    [ColorPicker]
    public WpfColor ButtonColor { get => buttonColor; set => Set(ref buttonColor, value); }
    WpfColor buttonColor = WpfColor.FromRgb(0xE5, 0x2D, 0x27);

    [Display(GroupName = "見た目", Name = "押した後の色")]
    [ColorPicker]
    public WpfColor DoneColor { get => doneColor; set => Set(ref doneColor, value); }
    WpfColor doneColor = WpfColor.FromRgb(0x6B, 0x6F, 0x78);

    [Display(GroupName = "見た目", Name = "高評価ボタンも出す")]
    [ToggleSlider]
    public bool ShowLike { get => showLike; set => Set(ref showLike, value); }
    bool showLike = true;

    [Display(GroupName = "見た目", Name = "ベルも出す")]
    [ToggleSlider]
    public bool ShowBell { get => showBell; set => Set(ref showBell, value); }
    bool showBell = true;

    [Display(GroupName = "見た目", Name = "カーソルを出す")]
    [ToggleSlider]
    public bool ShowCursor { get => showCursor; set => Set(ref showCursor, value); }
    bool showCursor = true;

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "位置", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 250)]
    public Animation Scale { get; } = new Animation(100, 5, 1000);

    [Display(GroupName = "動き", Name = "押す時刻（秒）")]
    [AnimationSlider("F2", "秒", 0.3, 5)]
    public Animation ClickTime { get; } = new Animation(1.2, 0, 3600);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [PosX, PosY, Scale, ClickTime];

    /// <summary>マウスのカーソル(矢印)</summary>
    static List<Vector2> CursorPoly(Vector2 tip, float s) => new()
    {
        tip, tip + new Vector2(0f, s), tip + new Vector2(s * 0.27f, s * 0.75f), tip + new Vector2(s * 0.45f, s * 1.12f),
        tip + new Vector2(s * 0.6f, s * 1.05f), tip + new Vector2(s * 0.42f, s * 0.7f), tip + new Vector2(s * 0.75f, s * 0.7f),
    };

    /// <summary>ベル</summary>
    static List<Vector2> BellPoly(Vector2 c, float r, float rot)
    {
        var local = new List<Vector2>();
        local.AddRange(Gfx.ArcPts(new Vector2(0f, -0.25f), 0.5f, MathF.PI, MathF.PI * 2f, 16));
        local.Add(new Vector2(0.55f, 0.35f));
        local.Add(new Vector2(0.75f, 0.55f));
        local.Add(new Vector2(-0.75f, 0.55f));
        local.Add(new Vector2(-0.55f, 0.35f));
        return DrawKit.Place(local, c, rot, r, r);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float u = c.F(Scale) / 100f;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        float click = c.F(ClickTime);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float bh = 96f * u;
        float likeW = ShowLike ? bh * 1.9f : 0f, bellW = ShowBell ? bh * 1.05f : 0f;
        var labelShape = TextKit.Get(TextKit.Lines(Label), bh * 0.4f, true, Font);
        var doneShape = TextKit.Get(TextKit.Lines(DoneLabel), bh * 0.4f, true, Font);
        float btnW = MathF.Max(labelShape.Width, doneShape.Width) + bh * 1.1f;
        float gap = bh * 0.25f;
        float total = likeW + (ShowLike ? gap : 0f) + btnW + (ShowBell ? gap + bellW : 0f);
        float x = ctr.X - total / 2f;
        float appear = DrawKit.EaseOutBack(k);

        // それぞれを押す時刻(高評価 → 登録 → ベル)
        float likeAt = click - 0.75f, subAt = click, bellAt = click + 0.8f;
        using var brush = dc.CreateSolidColorBrush(default);
        Vector2 likeC = default, btnC, bellC = default;

        float Press(float at) => t >= at && t < at + 0.15f ? 0.92f : 1f;

        // 高評価
        if (ShowLike)
        {
            likeC = new Vector2(x + likeW / 2f, ctr.Y);
            bool done = t >= likeAt;
            float s = Press(likeAt) * appear;
            var pill = ShapeGeom.RoundRect(likeC.X, likeC.Y, likeW * s, bh * s, bh * s / 2f);
            brush.Color = Gfx.C4(done ? WpfColor.FromRgb(0x2B, 0x7B, 0xE8) : WpfColor.FromRgb(0x27, 0x29, 0x30), k);
            Gfx.FillPoly(dc, brush, pill);
            brush.Color = Gfx.C4(1f, 1f, 1f, k);
            float jump = done ? DrawKit.EaseOutBack(Math.Clamp((t - likeAt) / 0.3f, 0f, 1f)) : 0f;
            var tc = likeC + new Vector2(-likeW * 0.18f * s, -jump * bh * 0.08f);
            Gfx.FillPoly(dc, brush, ThumbKit.Thumb(tc, bh * 0.26f * s * (1f + 0.25f * MathF.Sin(MathF.Min(1f, jump) * MathF.PI)), -0.1f));
            Gfx.FillPoly(dc, brush, ThumbKit.Cuff(tc, bh * 0.26f * s, -0.1f));
            var num = TextKit.Get(done ? "1.2万" : "1.1万", bh * 0.3f * s, true, Font);
            TextKit.DrawCentered(dc, brush, num, likeC + new Vector2(likeW * 0.2f * s, 0f), Gfx.C4(1f, 1f, 1f, k));
            x += likeW + gap;
        }

        // 登録ボタン
        btnC = new Vector2(x + btnW / 2f, ctr.Y);
        {
            bool done = t >= subAt;
            float s = Press(subAt) * appear;
            var pill = ShapeGeom.RoundRect(btnC.X, btnC.Y, btnW * s, bh * s, bh * s / 2f);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f * k);
            Gfx.FillPoly(dc, brush, pill.Select(q => q + new Vector2(0f, 6f * u)).ToList());
            brush.Color = Gfx.C4(done ? DoneColor : ButtonColor, k);
            Gfx.FillPoly(dc, brush, pill);
            // 押した瞬間の、広がる輪
            if (done && t - subAt < 0.6f)
            {
                float rf = (t - subAt) / 0.6f;
                brush.Color = Gfx.C4(ButtonColor, (1f - rf) * 0.6f * k);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(ShapeGeom.RoundRect(btnC.X, btnC.Y, btnW + rf * bh, bh + rf * bh, (bh + rf * bh) / 2f)), 4f * u);
            }
            TextKit.DrawCentered(dc, brush, done ? doneShape : labelShape, btnC, Gfx.C4(1f, 1f, 1f, k), scale: s);
            x += btnW + gap;
        }

        // ベル
        if (ShowBell)
        {
            bellC = new Vector2(x + bellW / 2f, ctr.Y);
            bool done = t >= bellAt;
            float s = Press(bellAt) * appear;
            brush.Color = Gfx.C4(WpfColor.FromRgb(0x27, 0x29, 0x30), k);
            dc.FillEllipse(new Ellipse(bellC, bh * 0.5f * s, bh * 0.5f * s), brush);
            float ring = done ? MathF.Sin((t - bellAt) * 28f) * MathF.Exp(-(t - bellAt) * 3f) * 0.5f : 0f;
            brush.Color = Gfx.C4(1f, 1f, 1f, k);
            Gfx.FillPoly(dc, brush, BellPoly(bellC + new Vector2(0f, -bh * 0.03f), bh * 0.3f * s, ring));
            dc.FillEllipse(new Ellipse(bellC + DrawKit.Rot(new Vector2(0f, bh * 0.25f * s), Vector2.Zero, ring), bh * 0.06f * s, bh * 0.06f * s), brush);
            if (done)
            {
                // 鳴っている線
                float a2 = MathF.Max(0f, 1f - (t - bellAt) / 1.2f) * k;
                brush.Color = Gfx.C4(1f, 1f, 1f, a2);
                for (int side = -1; side <= 1; side += 2)
                    for (int j = 0; j < 2; j++)
                    {
                        var arc = Gfx.ArcPts(bellC, bh * (0.42f + j * 0.12f), side < 0 ? MathF.PI * 1.15f : -MathF.PI * 0.35f, side < 0 ? MathF.PI * 1.35f : -MathF.PI * 0.15f, 6);
                        Gfx.Polyline(dc, brush, arc, 3f * u);
                    }
            }
        }

        // カーソル: 高評価 → 登録 → ベルへ動いて押す
        if (ShowCursor)
        {
            var start = ctr + new Vector2(total * 0.3f, bh * 2.2f);
            var points = new List<(float time, Vector2 pos)> { (0f, start) };
            if (ShowLike) points.Add((likeAt, likeC + new Vector2(-likeW * 0.15f, bh * 0.1f)));
            points.Add((subAt, btnC + new Vector2(btnW * 0.1f, bh * 0.1f)));
            if (ShowBell) points.Add((bellAt, bellC + new Vector2(0f, bh * 0.15f)));
            points.Add((bellAt + 0.9f, start + new Vector2(total * 0.15f, 0f)));
            Vector2 cur = points[^1].pos;
            for (int i = 0; i < points.Count - 1; i++)
            {
                var (t0, p0) = points[i];
                var (t1, p1) = points[i + 1];
                float travel = MathF.Min(0.45f, (t1 - t0) * 0.7f);
                if (t < t1)
                {
                    float f = Math.Clamp((t - (t1 - travel)) / MathF.Max(0.01f, travel), 0f, 1f);
                    f = f * f * (3f - 2f * f);
                    cur = Vector2.Lerp(p0, p1, f);
                    break;
                }
            }
            float ca = k * Math.Clamp(t / 0.3f, 0f, 1f);
            bool pressing = (ShowLike && t >= likeAt && t < likeAt + 0.15f) || (t >= subAt && t < subAt + 0.15f) || (ShowBell && t >= bellAt && t < bellAt + 0.15f);
            float cs = bh * 0.55f * (pressing ? 0.9f : 1f);
            var poly = CursorPoly(cur, cs);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.85f * ca);
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(poly), 4f * u);
            brush.Color = Gfx.C4(1f, 1f, 1f, ca);
            Gfx.FillPoly(dc, brush, poly);
        }
    }
}
