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

public enum ChapterStyle
{
    [Display(Name = "斜めの帯")] Band,
    [Display(Name = "中央の枠")] Frame,
    [Display(Name = "左に大きな数字")] BigNumber,
}

// =====================================================================
//  章タイトルカード
//   「CHAPTER 1」と見出しの枠が出る、場面の区切り用の画面。
//   日本語の見出し（「第1章 ○○」など）は、上に YMM4 のテキストを重ねてください
// =====================================================================
public sealed class ChapterCardPlugin : IShapePlugin
{
    public string Name => "演出：章タイトルカード";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ChapterCardParameter(sharedData);
}

public sealed class ChapterCardParameter : EffectShapeParameterBase
{
    public ChapterCardParameter() : this(null) { }
    public ChapterCardParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "形")]
    [EnumComboBox]
    public ChapterStyle Style { get => style; set => Set(ref style, value); }
    ChapterStyle style = ChapterStyle.Band;

    [Display(GroupName = "形", Name = "章の番号")]
    [AnimationSlider("F0", "", 0, 20)]
    public Animation Number { get; } = new Animation(1, 0, 999);

    [Display(GroupName = "形", Name = "「CHAPTER」の文字を出す")]
    [ToggleSlider]
    public bool ShowLabel { get => showLabel; set => Set(ref showLabel, value); }
    bool showLabel = true;

    [Display(GroupName = "色", Name = "背景の色")]
    [ColorPicker]
    public WpfColor BgColor { get => bgColor; set => Set(ref bgColor, value); }
    WpfColor bgColor = WpfColor.FromRgb(0x12, 0x18, 0x2A);

    [Display(GroupName = "色", Name = "背景の不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BgOpacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "色", Name = "帯・アクセントの色")]
    [ColorPicker]
    public WpfColor AccentColor { get => accentColor; set => Set(ref accentColor, value); }
    WpfColor accentColor = WpfColor.FromRgb(0xFF, 0xB8, 0x2E);

    [Display(GroupName = "色", Name = "文字・線の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);

    [Display(GroupName = "動き", Name = "出る時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation InTime { get; } = new Animation(0.6, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation OutTime { get; } = new Animation(0.4, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Number, BgOpacity, InTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float inT = c.F(InTime), outT = c.F(OutTime);
        float kin = inT <= 0.001f ? 1f : Math.Clamp(t / inT, 0f, 1f);
        float kout = outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f);
        float a = MathF.Min(Gfx.Smooth(0f, 0.5f, kin), kout);
        if (a <= 0.001f) return;
        float S = MathF.Min(w, h) / 1080f;
        int num = Math.Max(0, (int)MathF.Round(c.F(Number)));
        string label = "CHAPTER " + num.ToString(CultureInfo.InvariantCulture);
        var ctr = new Vector2(w / 2f, h / 2f);

        using var brush = dc.CreateSolidColorBrush(default);
        brush.Color = Gfx.C4(BgColor, c.F(BgOpacity) / 100f * a);
        dc.FillRectangle(new RawRectF(0, 0, w, h), brush);

        switch (Style)
        {
            case ChapterStyle.Frame:
                {
                    float e = DrawKit.EaseOut(kin);
                    float fw = w * 0.62f * e, fh = h * 0.3f;
                    brush.Color = Gfx.C4(TextColor, a);
                    dc.DrawRectangle(new RawRectF(ctr.X - fw / 2f, ctr.Y - fh / 2f, ctr.X + fw / 2f, ctr.Y + fh / 2f), brush, 3f * S + 1f);
                    brush.Color = Gfx.C4(AccentColor, a);
                    float cl = 40f * S;
                    for (int sx = -1; sx <= 1; sx += 2)
                        for (int sy = -1; sy <= 1; sy += 2)
                        {
                            var cr = ctr + new Vector2(sx * (fw / 2f + 10f * S), sy * (fh / 2f + 10f * S));
                            dc.DrawLine(cr, cr - new Vector2(sx * cl, 0), brush, 6f * S + 1f);
                            dc.DrawLine(cr, cr - new Vector2(0, sy * cl), brush, 6f * S + 1f);
                        }
                    if (ShowLabel)
                    {
                        float th = 44f * S;
                        var lp = new Vector2(ctr.X, ctr.Y - fh / 2f);
                        float lw = StrokeFont.Width(label, th, 0.3f) + 40f * S;
                        brush.Color = Gfx.C4(BgColor, a);
                        dc.FillRectangle(new RawRectF(lp.X - lw / 2f, lp.Y - th * 0.8f, lp.X + lw / 2f, lp.Y + th * 0.8f), brush);
                        brush.Color = Gfx.C4(AccentColor, a);
                        StrokeFont.DrawCentered(dc, brush, label, lp, th, MathF.Max(2f, th * 0.1f), 0.3f);
                    }
                    break;
                }
            case ChapterStyle.BigNumber:
                {
                    float e = DrawKit.EaseOutBack(kin);
                    string ns = num.ToString("00", CultureInfo.InvariantCulture);
                    float nh = h * 0.42f;
                    var np = new Vector2(w * 0.27f - (1f - e) * w * 0.2f, ctr.Y);
                    brush.Color = Gfx.C4(AccentColor, a);
                    StrokeFont.DrawCentered(dc, brush, ns, np, nh, MathF.Max(4f, nh * 0.1f), 0.15f, 0.1f);
                    brush.Color = Gfx.C4(TextColor, a);
                    float lx = w * 0.45f;
                    float len = (w * 0.47f) * DrawKit.EaseOut(kin);
                    dc.FillRectangle(new RawRectF(lx, ctr.Y + 4f * S, lx + len, ctr.Y + 8f * S), brush);
                    if (ShowLabel)
                    {
                        float th = 40f * S;
                        brush.Color = Gfx.C4(AccentColor, a);
                        StrokeFont.Draw(dc, brush, "CHAPTER", new Vector2(lx, ctr.Y - th * 2.6f), th, MathF.Max(2f, th * 0.1f), 0.35f);
                    }
                    break;
                }
            default:
                {
                    float e = DrawKit.EaseOut(kin);
                    float bh = h * 0.26f;
                    float slant = bh * 0.5f;
                    // 背面の細い帯
                    float off = (1f - e) * w * 1.2f;
                    var thin = new List<Vector2> { new(-slant + off, ctr.Y - bh * 0.5f - 14f * S), new(w + slant + off, ctr.Y - bh * 0.5f - 14f * S), new(w + off, ctr.Y - bh * 0.5f - 4f * S), new(-slant * 2f + off, ctr.Y - bh * 0.5f - 4f * S) };
                    brush.Color = Gfx.C4(AccentColor, 0.7f * a);
                    Gfx.FillPoly(dc, brush, DrawKit.ClipRect(thin, 0, 0, w, h));
                    // 主の帯（左から）
                    float mo = -(1f - e) * w * 1.2f;
                    var main = new List<Vector2> { new(-slant + mo, ctr.Y + bh / 2f), new(w + mo, ctr.Y + bh / 2f), new(w + slant + mo, ctr.Y - bh / 2f), new(mo, ctr.Y - bh / 2f) };
                    brush.Color = Gfx.C4(Gfx.Mix(BgColor, WpfColor.FromRgb(255, 255, 255), 0.08f), a);
                    Gfx.FillPoly(dc, brush, DrawKit.ClipRect(main, 0, 0, w, h));
                    brush.Color = Gfx.C4(AccentColor, a);
                    dc.FillRectangle(new RawRectF(0, ctr.Y + bh / 2f - 6f * S, w * e, ctr.Y + bh / 2f), brush);
                    if (ShowLabel)
                    {
                        float th = 52f * S;
                        brush.Color = Gfx.C4(AccentColor, a * Gfx.Smooth(0.4f, 1f, kin));
                        StrokeFont.Draw(dc, brush, label, new Vector2(w * 0.08f, ctr.Y - bh / 2f - th * 1.6f), th, MathF.Max(2f, th * 0.1f), 0.3f, 0.1f);
                    }
                    break;
                }
        }
    }
}
