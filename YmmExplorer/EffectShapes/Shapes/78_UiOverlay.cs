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
//  UIオーバーレイ
//   REC・LIVE・再生バー・通知バッジ・スマホ枠・ブラウザ枠
// =====================================================================
public sealed class UiOverlayPlugin : IShapePlugin
{
    public string Name => "演出：UIオーバーレイ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new UiOverlayParameter(sharedData);
}

public sealed class UiOverlayParameter : EffectShapeParameterBase
{
    public UiOverlayParameter() : this(null) { }
    public UiOverlayParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "UI", Name = "種類")]
    [EnumComboBox]
    public UiKind Kind { get => kind; set => Set(ref kind, value); }
    UiKind kind = UiKind.Rec;

    [Display(GroupName = "UI", Name = "メインの色")]
    [ColorPicker]
    public WpfColor MainColor { get => main; set => Set(ref main, value); }
    WpfColor main = WpfColor.FromRgb(0xFF, 0x2D, 0x2D);

    [Display(GroupName = "UI", Name = "文字・線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => line; set => Set(ref line, value); }
    WpfColor line = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "UI", Name = "背景の色")]
    [ColorPicker]
    public WpfColor PanelColor { get => panel; set => Set(ref panel, value); }
    WpfColor panel = WpfColor.FromRgb(0x14, 0x14, 0x1A);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(88, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(10, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ")]
    [AnimationSlider("F0", "px", 20, 800)]
    public Animation Size { get; } = new Animation(70, 5, 8000);

    [Display(GroupName = "値", Name = "再生の進み（再生バー）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Progress { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "値", Name = "通知の数（通知バッジ）")]
    [AnimationSlider("F0", "", 0, 99)]
    public Animation Count { get; } = new Animation(3, 0, 999);

    [Display(GroupName = "動き", Name = "点滅・脈打つ")]
    [ToggleSlider]
    public bool Pulse { get => pulse; set => Set(ref pulse, value); }
    bool pulse = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Size, Progress, Count];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float S = c.F(Size);
        float blink = Pulse ? (MathF.Floor(t * 1.6f) % 2f == 0f ? 1f : 0.25f) : 1f;
        float beat = Pulse ? 1f + 0.08f * MathF.Sin(t * 5f) : 1f;
        float lw = MathF.Max(1.5f, S * 0.07f);
        var white = LineColor;

        using var brush = dc.CreateSolidColorBrush(default);
        switch (Kind)
        {
            case UiKind.Rec:
                {
                    brush.Color = Gfx.C4(MainColor, blink);
                    dc.FillEllipse(new Ellipse(ctr + new Vector2(-S * 1.6f, 0f), S * 0.3f * beat, S * 0.3f * beat), brush);
                    brush.Color = Gfx.C4(white, 0.95f);
                    PathKit.Text(dc, brush, "REC", ctr + new Vector2(-S * 1.15f, -S * 0.3f), S * 0.6f, lw);
                    // 経過時間 00:00
                    int sec = (int)MathF.Floor(t);
                    string s = $"{Math.Min(99, sec / 60):00}{sec % 60:00}";
                    float dh = S * 0.5f, dw = dh * 0.55f, x = ctr.X + S * 0.55f;
                    for (int i = 0; i < 4; i++)
                    {
                        ShapeGeom.Digit(dc, brush, x, ctr.Y - dh / 2f, dw, dh, s[i] - '0', lw * 0.8f);
                        x += dw + dh * 0.28f;
                        if (i == 1)
                        {
                            dc.FillEllipse(new Ellipse(new Vector2(x - dh * 0.1f, ctr.Y - dh * 0.15f), lw * 0.7f, lw * 0.7f), brush);
                            dc.FillEllipse(new Ellipse(new Vector2(x - dh * 0.1f, ctr.Y + dh * 0.15f), lw * 0.7f, lw * 0.7f), brush);
                            x += dh * 0.25f;
                        }
                    }
                    break;
                }
            case UiKind.Live:
                {
                    float bw = S * 3.0f, bh = S * 0.95f;
                    brush.Color = Gfx.C4(MainColor, 1f);
                    Gfx.FillPoly(dc, brush, ShapeGeom.RoundRect(ctr.X, ctr.Y, bw, bh, bh * 0.22f));
                    brush.Color = Gfx.C4(white, blink);
                    dc.FillEllipse(new Ellipse(ctr + new Vector2(-bw * 0.36f, 0f), bh * 0.14f * beat, bh * 0.14f * beat), brush);
                    brush.Color = Gfx.C4(white, 1f);
                    float tw = PathKit.TextWidth("LIVE", bh * 0.5f);
                    PathKit.Text(dc, brush, "LIVE", new Vector2(ctr.X - tw / 2f + bw * 0.1f, ctr.Y - bh * 0.25f), bh * 0.5f, lw);
                    break;
                }
            case UiKind.PlayBar:
                {
                    float bw = S * 9f, bh = S * 0.16f;
                    float p = c.F(Progress) / 100f;
                    float x0 = ctr.X - bw / 2f;
                    // 再生ボタン
                    brush.Color = Gfx.C4(white, 0.95f);
                    var tri = new List<Vector2> { new(x0 - S * 0.9f, ctr.Y - S * 0.34f), new(x0 - S * 0.9f, ctr.Y + S * 0.34f), new(x0 - S * 0.36f, ctr.Y) };
                    Gfx.FillPoly(dc, brush, tri);
                    brush.Color = Gfx.C4(white, 0.3f);
                    Gfx.FillPoly(dc, brush, ShapeGeom.RoundRect(ctr.X, ctr.Y, bw, bh, bh / 2f));
                    brush.Color = Gfx.C4(MainColor, 1f);
                    if (p > 0.001f) Gfx.FillPoly(dc, brush, ShapeGeom.RoundRect(x0 + bw * p / 2f, ctr.Y, MathF.Max(bh, bw * p), bh, bh / 2f));
                    brush.Color = Gfx.C4(white, 1f);
                    dc.FillEllipse(new Ellipse(new Vector2(x0 + bw * p, ctr.Y), bh * 1.3f * beat, bh * 1.3f * beat), brush);
                    break;
                }
            case UiKind.Badge:
                {
                    // ベルの形と、右上の赤い数字バッジ
                    brush.Color = Gfx.C4(white, 0.95f);
                    var bell = new List<Vector2>();
                    for (int i = 0; i <= 20; i++)
                    {
                        float a = MathF.PI + i / 20f * MathF.PI;
                        bell.Add(ctr + new Vector2(MathF.Cos(a) * S * 0.62f, MathF.Sin(a) * S * 0.7f));
                    }
                    bell.Add(ctr + new Vector2(S * 0.75f, S * 0.55f)); bell.Add(ctr + new Vector2(-S * 0.75f, S * 0.55f));
                    Gfx.FillPoly(dc, brush, bell);
                    dc.FillEllipse(new Ellipse(ctr + new Vector2(0f, S * 0.78f), S * 0.17f, S * 0.17f), brush);
                    int cnt = (int)MathF.Round(c.F(Count));
                    if (cnt > 0)
                    {
                        var bc = ctr + new Vector2(S * 0.6f, -S * 0.58f);
                        float br = S * 0.42f * beat;
                        brush.Color = Gfx.C4(MainColor, 1f);
                        dc.FillEllipse(new Ellipse(bc, br, br), brush);
                        brush.Color = Gfx.C4(white, 1f);
                        string s = Math.Min(cnt, 99).ToString();
                        float dh = br * 0.9f, dw = dh * 0.55f, total = s.Length * dw + (s.Length - 1) * dh * 0.25f, x = bc.X - total / 2f;
                        foreach (char ch in s) { ShapeGeom.Digit(dc, brush, x, bc.Y - dh / 2f, dw, dh, ch - '0', MathF.Max(1.3f, dh * 0.14f)); x += dw + dh * 0.25f; }
                    }
                    break;
                }
            case UiKind.Phone:
                {
                    float pw = S * 3.2f, ph = S * 6.4f;
                    var body = ShapeGeom.RoundRect(ctr.X, ctr.Y, pw, ph, S * 0.5f);
                    brush.Color = Gfx.C4(PanelColor, 1f);
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(body), S * 0.16f);
                    brush.Color = Gfx.C4(white, 0.85f);
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(ShapeGeom.RoundRect(ctr.X, ctr.Y, pw + S * 0.16f, ph + S * 0.16f, S * 0.55f)), 1.5f);
                    brush.Color = Gfx.C4(PanelColor, 1f);
                    Gfx.FillPoly(dc, brush, ShapeGeom.RoundRect(ctr.X, ctr.Y - ph / 2f + S * 0.22f, pw * 0.42f, S * 0.26f, S * 0.13f));   // ノッチ
                    brush.Color = Gfx.C4(white, 0.7f);
                    Gfx.FillPoly(dc, brush, ShapeGeom.RoundRect(ctr.X, ctr.Y + ph / 2f - S * 0.2f, pw * 0.36f, S * 0.07f, S * 0.035f));   // ホームバー
                    break;
                }
            default:    // ブラウザ
                {
                    float bw = S * 9f, bh = S * 5.6f, tb = S * 0.7f;
                    float x0 = ctr.X - bw / 2f, y0 = ctr.Y - bh / 2f;
                    brush.Color = Gfx.C4(PanelColor, 0.96f);
                    Gfx.Rect(dc, brush, x0, y0, bw, tb);
                    brush.Color = Gfx.C4(white, 0.9f);
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(new List<Vector2> { new(x0, y0), new(x0 + bw, y0), new(x0 + bw, y0 + bh), new(x0, y0 + bh) }), lw * 0.7f);
                    dc.DrawLine(new Vector2(x0, y0 + tb), new Vector2(x0 + bw, y0 + tb), brush, lw * 0.6f);
                    var dots = new[] { WpfColor.FromRgb(0xFF, 0x5F, 0x57), WpfColor.FromRgb(0xFE, 0xBC, 0x2E), WpfColor.FromRgb(0x28, 0xC8, 0x40) };
                    for (int i = 0; i < 3; i++)
                    {
                        brush.Color = Gfx.C4(dots[i], 1f);
                        dc.FillEllipse(new Ellipse(new Vector2(x0 + tb * (0.6f + 0.55f * i), y0 + tb / 2f), tb * 0.16f, tb * 0.16f), brush);
                    }
                    brush.Color = Gfx.C4(white, 0.25f);
                    Gfx.FillPoly(dc, brush, ShapeGeom.RoundRect(x0 + bw * 0.55f, y0 + tb / 2f, bw * 0.62f, tb * 0.5f, tb * 0.25f));
                    break;
                }
        }
    }
}
