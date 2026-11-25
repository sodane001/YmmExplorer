using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
//  SNS投稿のカード
//   アイコン・名前・ID・本文・いいね数の並ぶ、汎用の投稿の見た目。
//   決めた時刻に、ハートが押されて数が増える(特定のサービスのロゴは使わない)
// =====================================================================
public sealed class SnsPostPlugin : IShapePlugin
{
    public string Name => "演出：SNS投稿のカード";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SnsPostParameter(sharedData);
}

public sealed class SnsPostParameter : EffectShapeParameterBase
{
    public SnsPostParameter() : this(null) { }
    public SnsPostParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "投稿", Name = "表示名")]
    [TextEditor(AcceptsReturn = false)]
    public string DisplayName { get => displayName; set => Set(ref displayName, value); }
    string displayName = "ゆっくり解説";

    [Display(GroupName = "投稿", Name = "ID")]
    [TextEditor(AcceptsReturn = false)]
    public string Handle { get => handle; set => Set(ref handle, value); }
    string handle = "@yukkuri_kaisetsu";

    [Display(GroupName = "投稿", Name = "本文")]
    [TextEditor(AcceptsReturn = true)]
    public string Body { get => body; set => Set(ref body, value); }
    string body = "新しい動画を公開しました！\n今回は「宇宙のはじまり」について、わかりやすく解説しています。";

    [Display(GroupName = "投稿", Name = "投稿した時間")]
    [TextEditor(AcceptsReturn = false)]
    public string When { get => when; set => Set(ref when, value); }
    string when = "2時間前";

    [Display(GroupName = "投稿", Name = "いいねの数")]
    [AnimationSlider("F0", "", 0, 100000)]
    public Animation Likes { get; } = new Animation(1280, 0, 1e9);

    [Display(GroupName = "投稿", Name = "リポストの数")]
    [AnimationSlider("F0", "", 0, 100000)]
    public Animation Reposts { get; } = new Animation(342, 0, 1e9);

    [Display(GroupName = "投稿", Name = "いいねを押す時刻（0=押さない）")]
    [AnimationSlider("F1", "秒", 0, 10)]
    public Animation LikeAt { get; } = new Animation(1.5, 0, 3600);

    [Display(GroupName = "見た目", Name = "ダークモード")]
    [ToggleSlider]
    public bool Dark { get => dark; set => Set(ref dark, value); }
    bool dark = false;

    [Display(GroupName = "見た目", Name = "アイコンの色")]
    [ColorPicker]
    public WpfColor IconColor { get => iconColor; set => Set(ref iconColor, value); }
    WpfColor iconColor = WpfColor.FromRgb(0x4F, 0x8E, 0xF7);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "カードの幅")]
    [AnimationSlider("F0", "px", 400, 1600)]
    public Animation CardWidth { get; } = new Animation(1000, 50, 8000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Likes, Reposts, LikeAt, PosX, PosY, CardWidth];

    static string Short(double v) => v >= 10000 ? (v / 10000).ToString("0.#", CultureInfo.InvariantCulture) + "万" : v.ToString("#,0", CultureInfo.InvariantCulture);

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.35f, 0.25f);
        if (k <= 0.001f) return;
        float cw = c.F(CardWidth), u = cw / 1000f;
        var bg = Dark ? WpfColor.FromRgb(0x15, 0x18, 0x1E) : WpfColor.FromRgb(255, 255, 255);
        var fg = Dark ? WpfColor.FromRgb(0xE7, 0xE9, 0xEA) : WpfColor.FromRgb(0x0F, 0x14, 0x19);
        var sub = Dark ? WpfColor.FromRgb(0x71, 0x76, 0x7B) : WpfColor.FromRgb(0x53, 0x64, 0x71);
        var ns = TextKit.Get(TextKit.Lines(DisplayName), 36f * u, true, Font);
        var hs = TextKit.Get(TextKit.Lines(Handle) + " ・ " + TextKit.Lines(When), 30f * u, false, Font);
        var bs = TextKit.Get(TextKit.Lines(Body), 36f * u, false, Font, cw - 170f * u, 0);
        float ch = 60f * u + ns.Height + 14f * u + bs.Height + 110f * u;
        float rise = (1f - DrawKit.EaseOut(k)) * 60f * u;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h + rise);
        float x0 = ctr.X - cw / 2f, y0 = ctr.Y - ch / 2f;
        using var brush = dc.CreateSolidColorBrush(default);
        var card = ShapeGeom.RoundRect(ctr.X, ctr.Y, cw, ch, 26f * u);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f * k);
        Gfx.FillPoly(dc, brush, card.Select(q => q + new Vector2(0f, 12f * u)).ToList());
        brush.Color = Gfx.C4(bg, k);
        Gfx.FillPoly(dc, brush, card);
        brush.Color = Gfx.C4(sub, 0.3f * k);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(card), 1.5f * u);
        // アイコン(人の形)
        var ic = new Vector2(x0 + 80f * u, y0 + 80f * u);
        brush.Color = Gfx.C4(IconColor, k);
        dc.FillEllipse(new Ellipse(ic, 46f * u, 46f * u), brush);
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.9f * k);
        dc.FillEllipse(new Ellipse(ic + new Vector2(0f, -10f * u), 16f * u, 16f * u), brush);
        dc.FillEllipse(new Ellipse(ic + new Vector2(0f, 26f * u), 26f * u, 16f * u), brush);
        float tx = x0 + 145f * u;
        TextKit.Draw(dc, brush, ns, new Vector2(tx, y0 + 40f * u), Gfx.C4(fg, k));
        TextKit.Draw(dc, brush, hs, new Vector2(tx + ns.Width + 14f * u, y0 + 40f * u + (ns.Height - hs.Height) / 2f), Gfx.C4(sub, k));
        float by = y0 + 50f * u + ns.Height + 10f * u;
        TextKit.Draw(dc, brush, bs, new Vector2(tx, by), Gfx.C4(fg, k));
        // 下の操作の列: 返信・リポスト・いいね
        float ay = by + bs.Height + 50f * u;
        float likeAt = c.F(LikeAt);
        bool liked = likeAt > 0.001f && t >= likeAt;
        float lp = liked ? Math.Clamp((t - likeAt) / 0.4f, 0f, 1f) : 0f;
        double likes = c.F(Likes) + (liked ? 1 : 0);
        var subC = Gfx.C4(sub, k);
        // 返信(吹き出し)
        brush.Color = subC;
        var rp = new Vector2(tx + 14f * u, ay);
        dc.DrawEllipse(new Ellipse(rp, 16f * u, 13f * u), brush, 2.5f * u);
        // リポスト(2本の矢印)
        var rr = new Vector2(tx + 260f * u, ay);
        dc.DrawRectangle(new RawRectF(rr.X - 16f * u, rr.Y - 9f * u, rr.X + 16f * u, rr.Y + 9f * u), brush, 2.5f * u);
        var rs = TextKit.Get(Short(c.F(Reposts)), 28f * u, false, Font);
        TextKit.Draw(dc, brush, rs, new Vector2(rr.X + 30f * u, ay - rs.Height / 2f), subC);
        // いいね(ハート。押すとポンと赤くなり、輪が広がる)
        var lh = new Vector2(tx + 500f * u, ay);
        float pop = liked ? 1f + 0.4f * MathF.Sin(MathF.Min(1f, lp * 1.5f) * MathF.PI) : 1f;
        var heart = ShapeGeom.HeartPoly(lh, 17f * u * pop, 0f);
        if (liked)
        {
            brush.Color = Gfx.C4(0.98f, 0.2f, 0.47f, k);
            Gfx.FillPoly(dc, brush, heart);
            if (lp < 1f)
            {
                brush.Color = Gfx.C4(0.98f, 0.2f, 0.47f, (1f - lp) * k);
                dc.DrawEllipse(new Ellipse(lh, 20f * u + lp * 30f * u, 20f * u + lp * 30f * u), brush, 3f * u * (1f - lp));
                for (int i = 0; i < 6; i++)
                {
                    float a = i * MathF.PI / 3f;
                    var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    dc.FillEllipse(new Ellipse(lh + d * (24f * u + lp * 26f * u), 4f * u * (1f - lp), 4f * u * (1f - lp)), brush);
                }
            }
        }
        else
        {
            brush.Color = subC;
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(heart), 2.5f * u);
        }
        var ls = TextKit.Get(Short(likes), 28f * u, false, Font);
        TextKit.Draw(dc, brush, ls, new Vector2(lh.X + 30f * u, ay - ls.Height / 2f), liked ? Gfx.C4(0.98f, 0.2f, 0.47f, k) : subC);
    }
}
