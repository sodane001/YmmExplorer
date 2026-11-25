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
//  投げ銭のカード
//   名前と金額の色つきの帯に、メッセージ。金額で色が変わる(自動)。
//   (特定のサービスのロゴは使わない、汎用のデザイン)
// =====================================================================
public sealed class DonationPlugin : IShapePlugin
{
    public string Name => "演出：投げ銭のカード";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DonationParameter(sharedData);
}

public sealed class DonationParameter : EffectShapeParameterBase
{
    public DonationParameter() : this(null) { }
    public DonationParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "内容", Name = "名前")]
    [TextEditor(AcceptsReturn = false)]
    public string Sender { get => sender; set => Set(ref sender, value); }
    string sender = "ゆっくり好き";

    [Display(GroupName = "内容", Name = "金額")]
    [AnimationSlider("F0", "", 100, 50000)]
    public Animation Amount { get; } = new Animation(5000, 0, 1e9);

    [Display(GroupName = "内容", Name = "通貨の記号")]
    [TextEditor(AcceptsReturn = false)]
    public string Currency { get => currency; set => Set(ref currency, value); }
    string currency = "¥";

    [Display(GroupName = "内容", Name = "メッセージ")]
    [TextEditor(AcceptsReturn = true)]
    public string Message { get => message; set => Set(ref message, value); }
    string message = "いつも楽しく見ています！これからも応援しています。";

    [Display(GroupName = "見た目", Name = "金額で色を変える")]
    [ToggleSlider]
    public bool AutoColor { get => autoColor; set => Set(ref autoColor, value); }
    bool autoColor = true;

    [Display(GroupName = "見た目", Name = "色（自動でない時）")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xA0, 0x00);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(30, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "カードの幅")]
    [AnimationSlider("F0", "px", 300, 1400)]
    public Animation CardWidth { get; } = new Animation(820, 50, 8000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Amount, PosX, PosY, CardWidth];

    /// <summary>金額に応じた色(上の帯, 下の地)</summary>
    static (WpfColor top, WpfColor body) Tier(double v) => v switch
    {
        < 200 => (WpfColor.FromRgb(0x15, 0x65, 0xC0), WpfColor.FromRgb(0x1E, 0x88, 0xE5)),
        < 500 => (WpfColor.FromRgb(0x00, 0xB8, 0xD4), WpfColor.FromRgb(0x00, 0xE5, 0xFF)),
        < 1000 => (WpfColor.FromRgb(0x00, 0xBF, 0xA5), WpfColor.FromRgb(0x1D, 0xE9, 0xB6)),
        < 2000 => (WpfColor.FromRgb(0xFF, 0xB3, 0x00), WpfColor.FromRgb(0xFF, 0xCA, 0x28)),
        < 5000 => (WpfColor.FromRgb(0xE6, 0x51, 0x00), WpfColor.FromRgb(0xF5, 0x7C, 0x00)),
        < 10000 => (WpfColor.FromRgb(0xC2, 0x18, 0x5B), WpfColor.FromRgb(0xE9, 0x1E, 0x63)),
        _ => (WpfColor.FromRgb(0xD0, 0x00, 0x00), WpfColor.FromRgb(0xE6, 0x21, 0x17)),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.35f, 0.3f);
        if (k <= 0.001f) return;
        double amount = c.F(Amount);
        var (top, body) = AutoColor ? Tier(amount) : (Gfx.Mix(Color, WpfColor.FromRgb(0, 0, 0), 0.15f), Color);
        bool darkText = AutoColor ? amount is >= 200 and < 2000 : (Color.R * 0.3 + Color.G * 0.59 + Color.B * 0.11) > 170;
        var textCol = darkText ? Gfx.C4(0.05f, 0.05f, 0.07f, k) : Gfx.C4(1f, 1f, 1f, k);
        float cw = c.F(CardWidth), u = cw / 820f;
        var ns = TextKit.Get(TextKit.Lines(Sender), 30f * u, true, Font);
        var amt = TextKit.Get(TextKit.Lines(Currency) + amount.ToString("#,0", CultureInfo.InvariantCulture), 34f * u, true, Font);
        string msg = TextKit.Lines(Message).Trim();
        var ms = TextKit.Get(msg, 30f * u, false, Font, cw - 60f * u, 0);
        float topH = 110f * u, bodyH = msg.Length > 0 ? ms.Height + 40f * u : 0f;
        float ch = topH + bodyH;
        float slide = DrawKit.EaseOutBack(k);
        var ctr = new Vector2(c.F(PosX) / 100f * w + (1f - slide) * -cw * 0.3f, c.F(PosY) / 100f * h);
        float x0 = ctr.X - cw / 2f, y0 = ctr.Y - ch / 2f;
        using var brush = dc.CreateSolidColorBrush(default);
        var card = ShapeGeom.RoundRect(ctr.X, ctr.Y, cw, ch, 14f * u);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f * k);
        Gfx.FillPoly(dc, brush, card.Select(q => q + new Vector2(0f, 8f * u)).ToList());
        brush.Color = Gfx.C4(body, k);
        Gfx.FillPoly(dc, brush, card);
        brush.Color = Gfx.C4(top, k);
        Gfx.FillPoly(dc, brush, DrawKit.ClipRect(card, x0, y0, x0 + cw, y0 + topH));
        // アイコン・名前・金額
        var icon = new Vector2(x0 + 60f * u, y0 + topH / 2f);
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.9f * k);
        dc.FillEllipse(new Ellipse(icon, 34f * u, 34f * u), brush);
        brush.Color = Gfx.C4(top, k);
        dc.FillEllipse(new Ellipse(icon + new Vector2(0f, -8f * u), 12f * u, 12f * u), brush);
        dc.FillEllipse(new Ellipse(icon + new Vector2(0f, 20f * u), 20f * u, 13f * u), brush);
        var nameCol = darkText ? Gfx.C4(0.05f, 0.05f, 0.07f, 0.7f * k) : Gfx.C4(1f, 1f, 1f, 0.8f * k);
        TextKit.Draw(dc, brush, ns, new Vector2(x0 + 112f * u, y0 + 16f * u), nameCol);
        // 金額は、ポンと大きくなる
        float pop = 1f + 0.25f * MathF.Exp(-MathF.Max(0f, t - 0.3f) * 8f) * MathF.Sin(MathF.Min(1f, MathF.Max(0f, t - 0.3f) * 4f) * MathF.PI);
        TextKit.Draw(dc, brush, amt, new Vector2(x0 + 112f * u, y0 + 20f * u + ns.Height), textCol, scale: pop);
        if (msg.Length > 0)
            TextKit.Draw(dc, brush, ms, new Vector2(x0 + 30f * u, y0 + topH + 20f * u), textCol);
        // カードをなでる光
        float sw = (t - 0.4f) / 0.8f;
        if (sw > 0f && sw < 1f)
        {
            float sx = x0 + (cw + ch) * sw - ch / 2f;
            var band = new List<Vector2> { new(sx, y0), new(sx + ch * 0.3f, y0), new(sx, y0 + ch), new(sx - ch * 0.3f, y0 + ch) };
            brush.Color = Gfx.C4(1f, 1f, 1f, 0.25f * k);
            Gfx.FillPoly(dc, brush, DrawKit.ClipRect(band, x0, y0, x0 + cw, y0 + ch));
        }
    }
}
