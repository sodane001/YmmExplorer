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

public enum ScreenCorner
{
    [Display(Name = "右上")] TopRight,
    [Display(Name = "左上")] TopLeft,
    [Display(Name = "右下")] BottomRight,
    [Display(Name = "左下")] BottomLeft,
    [Display(Name = "上のまん中")] TopCenter,
    [Display(Name = "下のまん中")] BottomCenter,
}

public enum AchievementIcon
{
    [Display(Name = "トロフィー")] Trophy,
    [Display(Name = "星")] Star,
    [Display(Name = "メダル")] Medal,
    [Display(Name = "チェック")] Check,
    [Display(Name = "王冠")] Crown,
}

/// <summary>小さなアイコンの描き方(実績・通知などで共通)</summary>
internal static class IconKit
{
    public static void Draw(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, AchievementIcon icon, Vector2 c, float r, WpfColor color, float alpha)
    {
        var dark = Gfx.Mix(color, WpfColor.FromRgb(0, 0, 0), 0.35f);
        switch (icon)
        {
            case AchievementIcon.Star:
                brush.Color = Gfx.C4(color, alpha);
                Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(c, r, 5, 0.45f, 0f));
                break;
            case AchievementIcon.Medal:
                brush.Color = Gfx.C4(WpfColor.FromRgb(0xE5, 0x39, 0x35), alpha);
                Gfx.FillPoly(dc, brush, new List<Vector2> { c + new Vector2(-r * 0.5f, -r), c + new Vector2(-r * 0.1f, -r), c + new Vector2(r * 0.15f, -r * 0.1f), c + new Vector2(-r * 0.25f, -r * 0.1f) });
                brush.Color = Gfx.C4(WpfColor.FromRgb(0x1E, 0x88, 0xE5), alpha);
                Gfx.FillPoly(dc, brush, new List<Vector2> { c + new Vector2(r * 0.5f, -r), c + new Vector2(r * 0.1f, -r), c + new Vector2(-r * 0.15f, -r * 0.1f), c + new Vector2(r * 0.25f, -r * 0.1f) });
                brush.Color = Gfx.C4(color, alpha);
                dc.FillEllipse(new Ellipse(c + new Vector2(0, r * 0.3f), r * 0.6f, r * 0.6f), brush);
                brush.Color = Gfx.C4(dark, alpha);
                dc.DrawEllipse(new Ellipse(c + new Vector2(0, r * 0.3f), r * 0.42f, r * 0.42f), brush, r * 0.08f);
                break;
            case AchievementIcon.Check:
                brush.Color = Gfx.C4(color, alpha);
                dc.FillEllipse(new Ellipse(c, r * 0.85f, r * 0.85f), brush);
                brush.Color = Gfx.C4(1f, 1f, 1f, alpha);
                Gfx.Polyline(dc, brush, new List<Vector2> { c + new Vector2(-r * 0.4f, 0f), c + new Vector2(-r * 0.1f, r * 0.32f), c + new Vector2(r * 0.45f, -r * 0.3f) }, r * 0.18f);
                break;
            case AchievementIcon.Crown:
                brush.Color = Gfx.C4(color, alpha);
                Gfx.FillPoly(dc, brush, new List<Vector2>
                {
                    c + new Vector2(-r * 0.8f, r * 0.55f), c + new Vector2(-r * 0.85f, -r * 0.45f), c + new Vector2(-r * 0.4f, 0f), c + new Vector2(0f, -r * 0.7f),
                    c + new Vector2(r * 0.4f, 0f), c + new Vector2(r * 0.85f, -r * 0.45f), c + new Vector2(r * 0.8f, r * 0.55f),
                });
                brush.Color = Gfx.C4(dark, alpha);
                dc.FillRectangle(new RawRectF(c.X - r * 0.8f, c.Y + r * 0.4f, c.X + r * 0.8f, c.Y + r * 0.58f), brush);
                break;
            default:
            {
                // トロフィー: カップ・取っ手・台
                brush.Color = Gfx.C4(color, alpha);
                var cup = new List<Vector2>();
                cup.Add(c + new Vector2(-r * 0.6f, -r * 0.75f));
                cup.Add(c + new Vector2(r * 0.6f, -r * 0.75f));
                for (int i = 0; i <= 10; i++)
                {
                    float a = i / 10f * MathF.PI;
                    cup.Add(c + new Vector2(MathF.Cos(a) * r * 0.6f, -r * 0.2f + MathF.Sin(a) * r * 0.45f));
                }
                Gfx.FillPoly(dc, brush, cup);
                dc.DrawEllipse(new Ellipse(c + new Vector2(-r * 0.62f, -r * 0.4f), r * 0.25f, r * 0.22f), brush, r * 0.1f);
                dc.DrawEllipse(new Ellipse(c + new Vector2(r * 0.62f, -r * 0.4f), r * 0.25f, r * 0.22f), brush, r * 0.1f);
                dc.FillRectangle(new RawRectF(c.X - r * 0.1f, c.Y + r * 0.2f, c.X + r * 0.1f, c.Y + r * 0.6f), brush);
                brush.Color = Gfx.C4(dark, alpha);
                dc.FillRectangle(new RawRectF(c.X - r * 0.45f, c.Y + r * 0.58f, c.X + r * 0.45f, c.Y + r * 0.82f), brush);
                break;
            }
        }
    }
}

// =====================================================================
//  実績解除・トロフィー通知
//   画面の端から、通知のカードがスッと出て、しばらくして引っこむ
// =====================================================================
public sealed class AchievementPlugin : IShapePlugin
{
    public string Name => "演出：実績解除・トロフィー通知";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AchievementParameter(sharedData);
}

public sealed class AchievementParameter : EffectShapeParameterBase
{
    public AchievementParameter() : this(null) { }
    public AchievementParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "見出し")]
    [TextEditor(AcceptsReturn = false)]
    public string Title { get => title; set => Set(ref title, value); }
    string title = "実績を解除しました";

    [Display(GroupName = "文字", Name = "内容")]
    [TextEditor(AcceptsReturn = false)]
    public string Detail { get => detail; set => Set(ref detail, value); }
    string detail = "はじめての動画投稿";

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "アイコン")]
    [EnumComboBox]
    public AchievementIcon Icon { get => icon; set => Set(ref icon, value); }
    AchievementIcon icon = AchievementIcon.Trophy;

    [Display(GroupName = "見た目", Name = "出す場所")]
    [EnumComboBox]
    public ScreenCorner Corner { get => corner; set => Set(ref corner, value); }
    ScreenCorner corner = ScreenCorner.TopRight;

    [Display(GroupName = "見た目", Name = "アクセントの色")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0xC4, 0x2E);

    [Display(GroupName = "見た目", Name = "カードの色")]
    [ColorPicker]
    public WpfColor CardColor { get => cardColor; set => Set(ref cardColor, value); }
    WpfColor cardColor = WpfColor.FromArgb(0xF0, 0x1C, 0x1F, 0x27);

    [Display(GroupName = "見た目", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 250)]
    public Animation Scale { get; } = new Animation(100, 5, 1000);

    [Display(GroupName = "見た目", Name = "画面の端からの間")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation EdgeMargin { get; } = new Animation(40, -500, 2000);

    [Display(GroupName = "動き", Name = "出る時間")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation InTime { get; } = new Animation(0.35, 0, 60);

    [Display(GroupName = "動き", Name = "引っこむ時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.3, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Scale, EdgeMargin, InTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, c.F(InTime), c.F(OutTime));
        if (k <= 0.001f) return;
        float sc = c.F(Scale) / 100f, mg = c.F(EdgeMargin);
        float th = 34f * sc, dh = 28f * sc;
        var ts = TextKit.Get(TextKit.Lines(Title), th, true, Font);
        var ds = TextKit.Get(TextKit.Lines(Detail), dh, false, Font);
        float iconR = 44f * sc;
        float cardH = MathF.Max(iconR * 2f + 30f * sc, ts.Height + ds.Height + 34f * sc);
        float cardW = iconR * 2f + 50f * sc + MathF.Max(ts.Width, ds.Width) + 30f * sc;

        bool right = Corner is ScreenCorner.TopRight or ScreenCorner.BottomRight;
        bool bottom = Corner is ScreenCorner.BottomRight or ScreenCorner.BottomLeft or ScreenCorner.BottomCenter;
        bool center = Corner is ScreenCorner.TopCenter or ScreenCorner.BottomCenter;
        float e = DrawKit.EaseOutBack(k);
        float cx, cy;
        if (center)
        {
            cx = w / 2f;
            cy = bottom ? h - mg - cardH / 2f + (1f - e) * (cardH + mg * 2f) : mg + cardH / 2f - (1f - e) * (cardH + mg * 2f);
        }
        else
        {
            cx = right ? w - mg - cardW / 2f + (1f - e) * (cardW + mg * 2f) : mg + cardW / 2f - (1f - e) * (cardW + mg * 2f);
            cy = bottom ? h - mg - cardH / 2f : mg + cardH / 2f;
        }
        var card = ShapeGeom.RoundRect(cx, cy, cardW, cardH, cardH * 0.18f);
        using var brush = dc.CreateSolidColorBrush(default);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f * k);
        Gfx.FillPoly(dc, brush, card.Select(p => p + new Vector2(0f, 8f * sc)).ToList());
        brush.Color = Gfx.C4(CardColor, k);
        Gfx.FillPoly(dc, brush, card);
        brush.Color = Gfx.C4(Accent, k);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(card), 3f * sc);

        // アイコン(光の輪つき)
        var ic = new Vector2(cx - cardW / 2f + 22f * sc + iconR, cy);
        float ring = DrawKit.Frac(t * 0.7f);
        DrawKit.Glow(dc, ic, iconR * 1.6f, Gfx.C4(Accent, 0.35f * k));
        brush.Color = Gfx.C4(Accent, 0.6f * (1f - ring) * k);
        dc.DrawEllipse(new Ellipse(ic, iconR * (0.9f + ring * 0.6f), iconR * (0.9f + ring * 0.6f)), brush, 3f * sc);
        brush.Color = Gfx.C4(Gfx.Mix(CardColor, WpfColor.FromRgb(255, 255, 255), 0.1f), k);
        dc.FillEllipse(new Ellipse(ic, iconR * 0.95f, iconR * 0.95f), brush);
        float pop = DrawKit.EaseOutBack(Math.Clamp((t - 0.15f) / 0.35f, 0f, 1f));
        IconKit.Draw(dc, brush, Icon, ic, iconR * 0.72f * pop, Accent, k);

        // 文字
        float tx = ic.X + iconR + 26f * sc;
        float ty = cy - (ts.Height + ds.Height + 4f * sc) / 2f;
        TextKit.Draw(dc, brush, ts, new Vector2(tx, ty), Gfx.C4(Accent, k));
        TextKit.Draw(dc, brush, ds, new Vector2(tx, ty + ts.Height + 4f * sc), Gfx.C4(1f, 1f, 1f, 0.95f * k));

        // カードの上を、光がひと筋走る
        float sweep = (t - 0.3f) / 0.7f;
        if (sweep > 0f && sweep < 1f)
        {
            float sx = cx - cardW / 2f + (cardW + cardH) * sweep - cardH / 2f;
            var band = new List<Vector2> { new(sx, cy - cardH / 2f), new(sx + cardH * 0.35f, cy - cardH / 2f), new(sx + cardH * 0.35f - cardH * 0.4f, cy + cardH / 2f), new(sx - cardH * 0.4f, cy + cardH / 2f) };
            var clipped = DrawKit.ClipRect(band, cx - cardW / 2f + 4f, cy - cardH / 2f + 4f, cx + cardW / 2f - 4f, cy + cardH / 2f - 4f);
            brush.Color = Gfx.C4(1f, 1f, 1f, 0.22f * k);
            Gfx.FillPoly(dc, brush, clipped);
        }
    }
}
