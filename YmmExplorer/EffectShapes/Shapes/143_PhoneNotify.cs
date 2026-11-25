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

public enum PhoneNotifyKind
{
    [Display(Name = "通知バナー（上から降りてくる）")] Banner,
    [Display(Name = "スマホの画面（チャット）")] Phone,
    [Display(Name = "チャットの吹き出しだけ")] ChatOnly,
}

// =====================================================================
//  通知バナー・スマホ画面
//   メッセージの通知が上から降りてくる。スマホの枠に、チャットの吹き出しを順に出すこともできる。
//   チャットは1行に1つ。「>」で始まる行は自分(右側)、ほかは相手(左側)
// =====================================================================
public sealed class PhoneNotifyPlugin : IShapePlugin
{
    public string Name => "演出：通知バナー・スマホ画面";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PhoneNotifyParameter(sharedData);
}

public sealed class PhoneNotifyParameter : EffectShapeParameterBase
{
    public PhoneNotifyParameter() : this(null) { }
    public PhoneNotifyParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public PhoneNotifyKind Kind { get => kind; set => Set(ref kind, value); }
    PhoneNotifyKind kind = PhoneNotifyKind.Banner;

    [Display(GroupName = "通知", Name = "アプリ名")]
    [TextEditor(AcceptsReturn = false)]
    public string AppName { get => appName; set => Set(ref appName, value); }
    string appName = "メッセージ";

    [Display(GroupName = "通知", Name = "差出人・見出し")]
    [TextEditor(AcceptsReturn = false)]
    public string Sender { get => sender; set => Set(ref sender, value); }
    string sender = "友だち";

    [Display(GroupName = "通知", Name = "本文")]
    [TextEditor(AcceptsReturn = true)]
    public string Body { get => body; set => Set(ref body, value); }
    string body = "いまから行くね！";

    [Display(GroupName = "チャット", Name = "会話（1行に1つ。「>」で始まる行は自分）")]
    [TextEditor(AcceptsReturn = true)]
    public string Chat { get => chat; set => Set(ref chat, value); }
    string chat = "おつかれ！\n>おつかれさま〜\n明日ひま？\n>ひまだよ\n>どうしたの？";

    [Display(GroupName = "チャット", Name = "1つずつ出る間隔")]
    [AnimationSlider("F2", "秒", 0.2, 3)]
    public Animation ChatInterval { get; } = new Animation(0.9, 0.05, 60);

    [Display(GroupName = "チャット", Name = "入力中の「…」を出す")]
    [ToggleSlider]
    public bool Typing { get => typing; set => Set(ref typing, value); }
    bool typing = true;

    [Display(GroupName = "見た目", Name = "アクセントの色（自分の吹き出し・アイコン）")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0x34, 0xC7, 0x59);

    [Display(GroupName = "見た目", Name = "ダークモード")]
    [ToggleSlider]
    public bool Dark { get => dark; set => Set(ref dark, value); }
    bool dark = false;

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心。バナーは上端）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(6, -100, 200);

    [Display(GroupName = "位置", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 250)]
    public Animation Scale { get; } = new Animation(100, 5, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [ChatInterval, PosX, PosY, Scale];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float u = c.F(Scale) / 100f;
        float k = DrawKit.InOut(c, 0.35f, 0.25f);
        if (k <= 0.001f) return;
        var anchor = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        var bg = Dark ? WpfColor.FromRgb(0x1C, 0x1D, 0x22) : WpfColor.FromRgb(0xF7, 0xF7, 0xFA);
        var fg = Dark ? WpfColor.FromRgb(0xF2, 0xF2, 0xF5) : WpfColor.FromRgb(0x1C, 0x1D, 0x22);
        var sub = Dark ? WpfColor.FromRgb(0x9A, 0x9C, 0xA5) : WpfColor.FromRgb(0x70, 0x72, 0x7A);
        using var brush = dc.CreateSolidColorBrush(default);

        if (Kind == PhoneNotifyKind.Banner)
        {
            float bw = 760f * u;
            var an = TextKit.Get(TextKit.Lines(AppName), 24f * u, false, Font);
            var sn = TextKit.Get(TextKit.Lines(Sender), 32f * u, true, Font);
            var bd = TextKit.Get(TextKit.Lines(Body), 30f * u, false, Font, bw - 150f * u, 0);
            float bh = 34f * u + an.Height + sn.Height + bd.Height + 18f * u;
            float drop = DrawKit.EaseOutBack(k);
            var cc = new Vector2(anchor.X, anchor.Y + bh / 2f - (1f - drop) * (bh + anchor.Y + 40f));
            var card = ShapeGeom.RoundRect(cc.X, cc.Y, bw, bh, 34f * u);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.28f * k);
            Gfx.FillPoly(dc, brush, card.Select(p => p + new Vector2(0f, 10f * u)).ToList());
            brush.Color = Gfx.C4(bg, 0.96f * k);
            Gfx.FillPoly(dc, brush, card);
            // アプリのアイコン(角丸の四角に吹き出し)
            var ic = new Vector2(cc.X - bw / 2f + 62f * u, cc.Y);
            var icon = ShapeGeom.RoundRect(ic.X, ic.Y, 76f * u, 76f * u, 18f * u);
            brush.Color = Gfx.C4(Accent, k);
            Gfx.FillPoly(dc, brush, icon);
            brush.Color = Gfx.C4(1f, 1f, 1f, k);
            dc.FillEllipse(new Ellipse(ic + new Vector2(0f, -2f * u), 24f * u, 19f * u), brush);
            Gfx.FillPoly(dc, brush, new List<Vector2> { ic + new Vector2(-14f * u, 10f * u), ic + new Vector2(-20f * u, 24f * u), ic + new Vector2(-2f * u, 14f * u) });
            float tx = ic.X + 58f * u, ty = cc.Y - bh / 2f + 17f * u;
            TextKit.Draw(dc, brush, an, new Vector2(tx, ty), Gfx.C4(sub, k));
            var now = TextKit.Get("いま", 22f * u, false, Font);
            TextKit.Draw(dc, brush, now, new Vector2(cc.X + bw / 2f - 30f * u - now.Width, ty), Gfx.C4(sub, k));
            TextKit.Draw(dc, brush, sn, new Vector2(tx, ty + an.Height + 4f * u), Gfx.C4(fg, k));
            TextKit.Draw(dc, brush, bd, new Vector2(tx, ty + an.Height + sn.Height + 8f * u), Gfx.C4(fg, k));
            return;
        }

        // スマホ(またはチャットだけ)
        bool phone = Kind == PhoneNotifyKind.Phone;
        float pw = 520f * u, ph = 980f * u;
        float rise = (1f - DrawKit.EaseOut(k)) * ph * 0.3f;
        var pc = new Vector2(anchor.X, (phone ? h / 2f : anchor.Y) + rise);
        if (!phone) pc = new Vector2(anchor.X, anchor.Y + rise);
        float sx0, sx1, sy0, sy1;
        if (phone)
        {
            pc = new Vector2(anchor.X, MathF.Max(anchor.Y, ph / 2f + 20f) + rise);
            var outer = ShapeGeom.RoundRect(pc.X, pc.Y, pw, ph, 70f * u);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f * k);
            Gfx.FillPoly(dc, brush, outer.Select(p => p + new Vector2(10f * u, 14f * u)).ToList());
            brush.Color = Gfx.C4(0.08f, 0.08f, 0.1f, k);
            Gfx.FillPoly(dc, brush, outer);
            var screen = ShapeGeom.RoundRect(pc.X, pc.Y, pw - 28f * u, ph - 28f * u, 56f * u);
            brush.Color = Gfx.C4(bg, k);
            Gfx.FillPoly(dc, brush, screen);
            // 上の切り欠きと、時刻
            var notch = ShapeGeom.RoundRect(pc.X, pc.Y - ph / 2f + 40f * u, 150f * u, 34f * u, 17f * u);
            brush.Color = Gfx.C4(0.08f, 0.08f, 0.1f, k);
            Gfx.FillPoly(dc, brush, notch);
            var clock = TextKit.Get("12:34", 24f * u, true, Font);
            TextKit.Draw(dc, brush, clock, new Vector2(pc.X - pw / 2f + 50f * u, pc.Y - ph / 2f + 26f * u), Gfx.C4(fg, k));
            // 相手の名前の帯
            var head = TextKit.Get(TextKit.Lines(Sender), 30f * u, true, Font);
            float hy = pc.Y - ph / 2f + 110f * u;
            TextKit.DrawCentered(dc, brush, head, new Vector2(pc.X, hy), Gfx.C4(fg, k));
            brush.Color = Gfx.C4(sub, 0.35f * k);
            dc.DrawLine(new Vector2(pc.X - pw / 2f + 14f * u, hy + 34f * u), new Vector2(pc.X + pw / 2f - 14f * u, hy + 34f * u), brush, 1.5f * u);
            sx0 = pc.X - pw / 2f + 34f * u; sx1 = pc.X + pw / 2f - 34f * u;
            sy0 = hy + 56f * u; sy1 = pc.Y + ph / 2f - 40f * u;
        }
        else
        {
            sx0 = pc.X - pw / 2f; sx1 = pc.X + pw / 2f;
            sy0 = pc.Y; sy1 = pc.Y + ph;
        }

        // チャットの吹き出し(下から積み上がる)
        var lines = TextKit.Lines(Chat).Split('\n').Where(s => s.Trim().Length > 0).ToList();
        float interval = MathF.Max(0.05f, c.F(ChatInterval));
        float fsz = 30f * u, maxW = (sx1 - sx0) * 0.72f;
        int visible = Math.Clamp((int)MathF.Floor((t - 0.3f) / interval) + 1, 0, lines.Count);
        var bubbles = new List<(TextShape s, bool mine, float appear)>();
        for (int i = 0; i < visible; i++)
        {
            var raw = lines[i].Trim();
            bool mine = raw.StartsWith(">") || raw.StartsWith("＞");
            string text = mine ? raw[1..].Trim() : raw;
            float appear = Math.Clamp((t - 0.3f - i * interval) / 0.25f, 0f, 1f);
            bubbles.Add((TextKit.Get(text, fsz, false, Font, maxW, 0), mine, appear));
        }
        // 次の相手の発言の前に「…」
        bool typingNow = Typing && visible < lines.Count && !(lines[visible].Trim().StartsWith(">") || lines[visible].Trim().StartsWith("＞"))
                         && (t - 0.3f - visible * interval) > -interval * 0.6f;
        float gapY = 14f * u, padX = 22f * u, padY = 14f * u;
        float y = phone ? sy0 : sy0;
        // 下へ並べて、はみ出たら上へずらす
        float totalH = bubbles.Sum(b => b.s.Height + padY * 2f + gapY) + (typingNow ? 60f * u : 0f);
        float shift = MathF.Max(0f, totalH - (sy1 - sy0));
        y -= shift;
        if (phone) dc.PushAxisAlignedClip(new RawRectF(sx0 - 20f * u, sy0 - 10f * u, sx1 + 20f * u, sy1), AntialiasMode.PerPrimitive);
        foreach (var (s, mine, appear) in bubbles)
        {
            float bw = s.Width + padX * 2f, bh = s.Height + padY * 2f;
            float pop = DrawKit.EaseOutBack(appear);
            float bx = mine ? sx1 - bw / 2f : sx0 + bw / 2f;
            var bc = new Vector2(bx, y + bh / 2f);
            var rr = BubbleKit.Scale(ShapeGeom.RoundRect(bc.X, bc.Y, bw, bh, MathF.Min(bh / 2f, 26f * u)), new Vector2(mine ? sx1 : sx0, bc.Y + bh / 2f), pop);
            var bubbleCol = mine ? Accent : (Dark ? WpfColor.FromRgb(0x33, 0x35, 0x3D) : WpfColor.FromRgb(0xE6, 0xE7, 0xEC));
            brush.Color = Gfx.C4(bubbleCol, k * appear);
            Gfx.FillPoly(dc, brush, rr);
            var textCol = mine ? WpfColor.FromRgb(255, 255, 255) : fg;
            var tl = new Vector2(mine ? sx1 : sx0, bc.Y + bh / 2f) + (new Vector2(bx - bw / 2f + padX, y + padY) - new Vector2(mine ? sx1 : sx0, bc.Y + bh / 2f)) * pop;
            TextKit.Draw(dc, brush, s, tl, Gfx.C4(textCol, k * appear), scale: pop);
            y += bh + gapY;
        }
        if (typingNow)
        {
            var bc = new Vector2(sx0 + 60f * u, y + 26f * u);
            var rr = ShapeGeom.RoundRect(bc.X, bc.Y, 110f * u, 52f * u, 26f * u);
            brush.Color = Gfx.C4(Dark ? WpfColor.FromRgb(0x33, 0x35, 0x3D) : WpfColor.FromRgb(0xE6, 0xE7, 0xEC), k);
            Gfx.FillPoly(dc, brush, rr);
            for (int i = 0; i < 3; i++)
            {
                float bob = MathF.Max(0f, MathF.Sin(t * 7f - i * 0.8f)) * 6f * u;
                brush.Color = Gfx.C4(sub, k);
                dc.FillEllipse(new Ellipse(bc + new Vector2((i - 1) * 24f * u, -bob), 7f * u, 7f * u), brush);
            }
        }
        if (phone) dc.PopAxisAlignedClip();
    }
}
