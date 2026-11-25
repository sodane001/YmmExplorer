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
//  ライブ配信の画面枠
//   「LIVE」の赤い表示・視聴者数・経過時間・チャンネル名と、右側のコメント欄
//   (特定のサービスのロゴは使わない、汎用のデザイン)
// =====================================================================
public sealed class LiveFramePlugin : IShapePlugin
{
    public string Name => "演出：ライブ配信の画面枠";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LiveFrameParameter(sharedData);
}

public sealed class LiveFrameParameter : EffectShapeParameterBase
{
    public LiveFrameParameter() : this(null) { }
    public LiveFrameParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "配信", Name = "チャンネル名")]
    [TextEditor(AcceptsReturn = false)]
    public string Channel { get => channel; set => Set(ref channel, value); }
    string channel = "ゆっくりチャンネル";

    [Display(GroupName = "配信", Name = "配信のタイトル")]
    [TextEditor(AcceptsReturn = false)]
    public string StreamTitle { get => streamTitle; set => Set(ref streamTitle, value); }
    string streamTitle = "【雑談】まったりお話しします";

    [Display(GroupName = "配信", Name = "視聴者数（はじめ）")]
    [AnimationSlider("F0", "人", 0, 10000)]
    public Animation Viewers { get; } = new Animation(1280, 0, 1e9);

    [Display(GroupName = "配信", Name = "1秒に増える視聴者")]
    [AnimationSlider("F0", "人", 0, 200)]
    public Animation ViewerGrowth { get; } = new Animation(12, -1e6, 1e6);

    [Display(GroupName = "配信", Name = "経過時間のはじめ（分）")]
    [AnimationSlider("F0", "分", 0, 300)]
    public Animation StartMinutes { get; } = new Animation(12, 0, 100000);

    [Display(GroupName = "コメント欄", Name = "コメント欄を出す")]
    [ToggleSlider]
    public bool ShowChat { get => showChat; set => Set(ref showChat, value); }
    bool showChat = true;

    [Display(GroupName = "コメント欄", Name = "コメント（1行に「名前:コメント」）")]
    [TextEditor(AcceptsReturn = true)]
    public string Comments { get => comments; set => Set(ref comments, value); }
    string comments = "たろう:こんばんは！\nみけ:初見です\nゆう:待ってました\nあお:わこつ\nそら:今日もかわいい\nけん:BGMいいね\nりん:www";

    [Display(GroupName = "コメント欄", Name = "コメントが流れる間隔")]
    [AnimationSlider("F2", "秒", 0.2, 5)]
    public Animation ChatInterval { get; } = new Animation(1.1, 0.05, 600);

    [Display(GroupName = "コメント欄", Name = "コメント欄の幅")]
    [AnimationSlider("F0", "%", 15, 45)]
    public Animation ChatWidth { get; } = new Animation(26, 5, 80);

    [Display(GroupName = "見た目", Name = "アクセントの色")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xE5, 0x2D, 0x27);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Viewers, ViewerGrowth, StartMinutes, ChatInterval, ChatWidth];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        float u = h / 1080f;
        using var brush = dc.CreateSolidColorBrush(default);
        float chatW = ShowChat ? w * c.F(ChatWidth) / 100f : 0f;

        // 左上: LIVE・視聴者数・経過時間
        float x = 36f * u, y = 32f * u;
        var live = TextKit.Get("LIVE", 34f * u, true, "Arial Black");
        float pw = live.Width + 62f * u, ph = live.Height + 14f * u;
        var pill = ShapeGeom.RoundRect(x + pw / 2f, y + ph / 2f, pw, ph, 8f * u);
        brush.Color = Gfx.C4(Accent, k);
        Gfx.FillPoly(dc, brush, pill);
        float blink = DrawKit.Frac(t * 0.8f) < 0.6f ? 1f : 0.25f;
        brush.Color = Gfx.C4(1f, 1f, 1f, k * blink);
        dc.FillEllipse(new Ellipse(new Vector2(x + 22f * u, y + ph / 2f), 8f * u, 8f * u), brush);
        TextKit.Draw(dc, brush, live, new Vector2(x + 40f * u, y + 7f * u), Gfx.C4(1f, 1f, 1f, k));
        x += pw + 14f * u;
        int viewers = (int)MathF.Max(0f, c.F(Viewers) + c.F(ViewerGrowth) * t + MathF.Sin(t * 1.3f) * 3f);
        float secs = c.F(StartMinutes) * 60f + t;
        string info = $"視聴 {viewers.ToString("#,0", CultureInfo.InvariantCulture)}人   {(int)(secs / 3600f)}:{(int)(secs / 60f) % 60:00}:{(int)secs % 60:00}";
        var infoS = TextKit.Get(info, 30f * u, true, Font);
        var ib = ShapeGeom.RoundRect(x + infoS.Width / 2f + 16f * u, y + ph / 2f, infoS.Width + 32f * u, ph, 8f * u);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.55f * k);
        Gfx.FillPoly(dc, brush, ib);
        TextKit.Draw(dc, brush, infoS, new Vector2(x + 16f * u, y + (ph - infoS.Height) / 2f), Gfx.C4(1f, 1f, 1f, k));

        // 下: チャンネル名とタイトルの帯
        float barH = 120f * u;
        float bx1 = w - chatW;
        using (var g = Gfx.Linear(dc, new Vector2(0, h - barH * 1.6f), new Vector2(0, h), (0f, Gfx.C4(0f, 0f, 0f, 0f)), (1f, Gfx.C4(0f, 0f, 0f, 0.75f * k))))
            dc.FillRectangle(new RawRectF(0, h - barH * 1.6f, bx1, h), g);
        var icon = new Vector2(70f * u, h - barH / 2f - 10f * u);
        brush.Color = Gfx.C4(Accent, k);
        dc.FillEllipse(new Ellipse(icon, 40f * u, 40f * u), brush);
        brush.Color = Gfx.C4(1f, 1f, 1f, k);
        Gfx.FillPoly(dc, brush, new List<Vector2> { icon + new Vector2(-12f * u, -18f * u), icon + new Vector2(20f * u, 0f), icon + new Vector2(-12f * u, 18f * u) });
        var ts = TextKit.Get(TextKit.Lines(StreamTitle), 38f * u, true, Font);
        var cs = TextKit.Get(TextKit.Lines(Channel), 28f * u, false, Font);
        TextKit.Draw(dc, brush, ts, new Vector2(126f * u, icon.Y - ts.Height + 2f * u), Gfx.C4(1f, 1f, 1f, k));
        TextKit.Draw(dc, brush, cs, new Vector2(126f * u, icon.Y + 6f * u), Gfx.C4(0.8f, 0.82f, 0.86f, k));

        if (!ShowChat) return;
        // 右: コメント欄(下から積み上がり、古いものは上へ消える)
        float cx0 = w - chatW;
        brush.Color = Gfx.C4(0.06f, 0.06f, 0.08f, 0.82f * k);
        dc.FillRectangle(new RawRectF(cx0, 0, w, h), brush);
        var head = TextKit.Get("チャット", 30f * u, true, Font);
        TextKit.Draw(dc, brush, head, new Vector2(cx0 + 24f * u, 26f * u), Gfx.C4(1f, 1f, 1f, k));
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.12f * k);
        dc.DrawLine(new Vector2(cx0, 26f * u + head.Height + 18f * u), new Vector2(w, 26f * u + head.Height + 18f * u), brush, 1.5f * u);
        float listTop = 26f * u + head.Height + 30f * u, listBottom = h - 40f * u;
        var lines = TextKit.Lines(Comments).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        if (lines.Count == 0) return;
        float iv = MathF.Max(0.05f, c.F(ChatInterval));
        int shown = (int)MathF.Floor(t / iv) + 1;
        float slide = DrawKit.EaseOut(Math.Clamp(DrawKit.Frac(t / iv) * iv / 0.25f, 0f, 1f));
        float fs = 28f * u;
        dc.PushAxisAlignedClip(new RawRectF(cx0, listTop, w, listBottom), AntialiasMode.PerPrimitive);
        float yy = listBottom;
        for (int j = shown - 1; j >= 0 && yy > listTop - 200f * u; j--)
        {
            string raw = lines[j % lines.Count];
            int colon = raw.IndexOfAny(new[] { ':', '：' });
            string name = colon > 0 ? raw[..colon].Trim() : "名無し";
            string msg = colon > 0 ? raw[(colon + 1)..].Trim() : raw;
            var ms = TextKit.Get(msg, fs, false, Font, chatW - 120f * u, 0);
            var ns = TextKit.Get(name, fs * 0.85f, true, Font);
            float rowH = ns.Height + ms.Height + 18f * u;
            float off = j == shown - 1 ? (1f - slide) * rowH : 0f;
            yy -= rowH;
            float ry = yy + off;
            var avatar = new Vector2(cx0 + 44f * u, ry + 28f * u);
            brush.Color = Gfx.C4(Gfx.Hsv(DrawKit.H(Seed, j % lines.Count) * 360f, 0.5f, 0.9f), k);
            dc.FillEllipse(new Ellipse(avatar, 20f * u, 20f * u), brush);
            TextKit.Draw(dc, brush, ns, new Vector2(cx0 + 78f * u, ry + 6f * u), Gfx.C4(0.65f, 0.7f, 0.78f, k));
            TextKit.Draw(dc, brush, ms, new Vector2(cx0 + 78f * u, ry + 8f * u + ns.Height), Gfx.C4(1f, 1f, 1f, k * (j == shown - 1 ? slide : 1f)));
        }
        dc.PopAxisAlignedClip();
    }
}
