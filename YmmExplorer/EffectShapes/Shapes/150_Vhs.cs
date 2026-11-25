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

public enum VhsLabel
{
    [Display(Name = "▶ PLAY")] Play,
    [Display(Name = "● REC")] Rec,
    [Display(Name = "Ⅱ PAUSE")] Pause,
    [Display(Name = "◀◀ REW")] Rewind,
    [Display(Name = "出さない")] None,
}

// =====================================================================
//  VHS・ビデオテープ風
//   画面の上に重ねる、録画中の表示・日付・カウンター・走査線・ノイズの帯
// =====================================================================
public sealed class VhsPlugin : IShapePlugin
{
    public string Name => "演出：VHS・ビデオテープ風";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new VhsParameter(sharedData);
}

public sealed class VhsParameter : EffectShapeParameterBase
{
    public VhsParameter() : this(null) { }
    public VhsParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "表示", Name = "左上の表示")]
    [EnumComboBox]
    public VhsLabel Label { get => label; set => Set(ref label, value); }
    VhsLabel label = VhsLabel.Play;

    [Display(GroupName = "表示", Name = "日付", Description = "空にすると出しません")]
    [TextEditor(AcceptsReturn = false)]
    public string DateText { get => dateText; set => Set(ref dateText, value); }
    string dateText = "NOV. 21 1998";

    [Display(GroupName = "表示", Name = "時刻のカウンター（アイテムの経過時間）")]
    [ToggleSlider]
    public bool Counter { get => counter; set => Set(ref counter, value); }
    bool counter = true;

    [Display(GroupName = "表示", Name = "「SP」などのモード表示")]
    [TextEditor(AcceptsReturn = false)]
    public string Mode { get => mode; set => Set(ref mode, value); }
    string mode = "SP";

    [Display(GroupName = "表示", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 16, 120)]
    public Animation FontSize { get; } = new Animation(54, 4, 1000);

    [Display(GroupName = "表示", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(0xF2, 0xF2, 0xF2);

    [Display(GroupName = "画面の乱れ", Name = "走査線")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Scanlines { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "画面の乱れ", Name = "下のノイズの帯")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Tracking { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "画面の乱れ", Name = "流れるノイズの帯")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RollBand { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "画面の乱れ", Name = "白い点のノイズ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Speckle { get; } = new Animation(25, 0, 100);

    [Display(GroupName = "画面の乱れ", Name = "色のにじみ（赤・青のふち）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Bleed { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "画面の乱れ", Name = "四隅を暗く")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Vignette { get; } = new Animation(45, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [FontSize, Scanlines, Tracking, RollBand, Speckle, Bleed, Vignette];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        long frame = c.Frame;
        float fs = c.F(FontSize);
        using var brush = dc.CreateSolidColorBrush(default);

        // 四隅を暗く(横長の楕円の、ぼかし)
        float vig = c.F(Vignette) / 100f;
        if (vig > 0.002f)
        {
            var ctr = new Vector2(w / 2f, h / 2f);
            float rad = MathF.Sqrt(w * w + h * h) / 2f;
            using var g = Gfx.Radial(dc, ctr, rad, (0f, Gfx.C4(0f, 0f, 0f, 0f)), (0.62f, Gfx.C4(0f, 0f, 0f, 0f)), (1f, Gfx.C4(0f, 0f, 0f, 0.85f * vig)));
            dc.FillRectangle(new RawRectF(0, 0, w, h), g);
        }

        // 走査線
        float scan = c.F(Scanlines) / 100f;
        if (scan > 0.002f)
        {
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f * scan);
            float step = MathF.Max(3f, h / 270f);
            for (float y = Gfx.Mod(t * 12f, step); y < h; y += step)
                dc.FillRectangle(new RawRectF(0, y, w, y + step * 0.45f), brush);
        }

        // 流れるノイズの帯(上から下へ、ゆっくり)
        float roll = c.F(RollBand) / 100f;
        if (roll > 0.002f)
        {
            float by = Gfx.Mod(t * h * 0.12f, h * 1.4f) - h * 0.2f;
            float bh = h * 0.06f;
            using (var g = Gfx.Linear(dc, new Vector2(0, by - bh), new Vector2(0, by + bh), (0f, Gfx.C4(1f, 1f, 1f, 0f)), (0.5f, Gfx.C4(1f, 1f, 1f, 0.12f * roll)), (1f, Gfx.C4(1f, 1f, 1f, 0f))))
                dc.FillRectangle(new RawRectF(0, by - bh, w, by + bh), g);
            for (int i = 0; i < 30; i++)
            {
                float yy = by + (DrawKit.H((int)frame, i, 1) - 0.5f) * bh * 1.6f;
                float xx = DrawKit.H((int)frame, i, 2) * w;
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.35f * roll * DrawKit.H((int)frame, i, 3));
                dc.FillRectangle(new RawRectF(xx, yy, xx + w * (0.02f + DrawKit.H((int)frame, i, 4) * 0.15f), yy + 2f), brush);
            }
        }

        // 下のトラッキングのノイズ(画面の下端で、横に流れる白い線の束)
        float trk = c.F(Tracking) / 100f;
        if (trk > 0.002f)
        {
            float top = h * 0.93f;
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f * trk);
            dc.FillRectangle(new RawRectF(0, top, w, h), brush);
            for (int i = 0; i < 90; i++)
            {
                float yy = top + DrawKit.H((int)frame, i, 5) * (h - top);
                float xx = DrawKit.H((int)frame, i, 6) * w;
                float ll = w * (0.03f + DrawKit.H((int)frame, i, 7) * 0.3f);
                brush.Color = Gfx.C4(1f, 1f, 1f, (0.2f + 0.6f * DrawKit.H((int)frame, i, 8)) * trk);
                dc.FillRectangle(new RawRectF(xx, yy, xx + ll, yy + MathF.Max(1f, h / 540f)), brush);
            }
        }

        // 白い点のノイズ
        float spk = c.F(Speckle) / 100f;
        if (spk > 0.002f)
        {
            int n = (int)(spk * 220f);
            for (int i = 0; i < n; i++)
            {
                var p = new Vector2(DrawKit.H((int)frame, i, 9) * w, DrawKit.H((int)frame, i, 10) * h);
                float r = 0.8f + DrawKit.H((int)frame, i, 11) * 1.8f;
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.5f * spk);
                dc.FillRectangle(new RawRectF(p.X, p.Y, p.X + r * 2f, p.Y + r * 0.7f), brush);
            }
        }

        // 画面の表示(OSD): 赤・青のふちがずれる(色のにじみ)
        float bleed = c.F(Bleed) / 100f;
        string font = "Consolas";
        var main = Gfx.C4(TextColor, 0.95f);
        void Osd(string text, Vector2 pos, float size, Color4 col)
        {
            var s = TextKit.Get(text, size, true, font);
            if (s.Contours.Count == 0) s = TextKit.Get(text, size, true);
            float d = size * 0.05f * bleed;
            if (bleed > 0.01f)
            {
                brush.Color = Gfx.C4(1f, 0.1f, 0.2f, 0.55f * bleed);
                TextKit.Fill(dc, brush, s, pos + new Vector2(-d, 0f));
                brush.Color = Gfx.C4(0.1f, 0.5f, 1f, 0.55f * bleed);
                TextKit.Fill(dc, brush, s, pos + new Vector2(d, 0f));
            }
            TextKit.Draw(dc, brush, s, pos, col, shadow: Gfx.C4(0f, 0f, 0f, 0.5f), shadowOffset: new Vector2(size * 0.04f, size * 0.04f));
        }
        float margin = fs * 1.1f;
        string lab = Label switch
        {
            VhsLabel.Rec => "REC",
            VhsLabel.Pause => "PAUSE",
            VhsLabel.Rewind => "REW",
            VhsLabel.None => "",
            _ => "PLAY",
        };
        if (lab.Length > 0)
        {
            var pos = new Vector2(margin, margin * 0.8f);
            float mark = fs * 0.75f;
            var mc = pos + new Vector2(mark * 0.5f, fs * 0.62f);
            bool blink = Label != VhsLabel.Rec || DrawKit.Frac(t) < 0.5f;
            if (blink)
            {
                brush.Color = Label == VhsLabel.Rec ? Gfx.C4(1f, 0.15f, 0.15f, 0.95f) : main;
                switch (Label)
                {
                    case VhsLabel.Rec: dc.FillEllipse(new Ellipse(mc, mark * 0.4f, mark * 0.4f), brush); break;
                    case VhsLabel.Pause:
                        dc.FillRectangle(new RawRectF(mc.X - mark * 0.4f, mc.Y - mark * 0.4f, mc.X - mark * 0.12f, mc.Y + mark * 0.4f), brush);
                        dc.FillRectangle(new RawRectF(mc.X + mark * 0.12f, mc.Y - mark * 0.4f, mc.X + mark * 0.4f, mc.Y + mark * 0.4f), brush);
                        break;
                    case VhsLabel.Rewind:
                        for (int k = 0; k < 2; k++)
                        {
                            float ox = (k - 0.5f) * mark * 0.5f;
                            Gfx.FillPoly(dc, brush, new List<Vector2> { mc + new Vector2(ox + mark * 0.25f, -mark * 0.4f), mc + new Vector2(ox + mark * 0.25f, mark * 0.4f), mc + new Vector2(ox - mark * 0.3f, 0f) });
                        }
                        break;
                    default:
                        Gfx.FillPoly(dc, brush, new List<Vector2> { mc + new Vector2(-mark * 0.35f, -mark * 0.42f), mc + new Vector2(-mark * 0.35f, mark * 0.42f), mc + new Vector2(mark * 0.42f, 0f) });
                        break;
                }
                Osd(lab, pos + new Vector2(mark * 1.3f, 0f), fs, Label == VhsLabel.Rec ? Gfx.C4(1f, 0.25f, 0.25f, 0.95f) : main);
            }
        }
        string md = TextKit.Lines(Mode).Trim();
        if (md.Length > 0)
        {
            var s = TextKit.Get(md, fs, true, font);
            Osd(md, new Vector2(w - margin - s.Width, margin * 0.8f), fs, main);
        }
        if (Counter)
        {
            float secs = MathF.Max(0f, t);
            int hh = (int)(secs / 3600f), mm = (int)(secs / 60f) % 60, ss = (int)secs % 60;
            string ctrText = string.Format(CultureInfo.InvariantCulture, "{0}:{1:00}:{2:00}", hh, mm, ss);
            var s = TextKit.Get(ctrText, fs, true, font);
            Osd(ctrText, new Vector2(w - margin - s.Width, h - margin * 1.9f), fs, main);
        }
        string date = TextKit.Lines(DateText).Trim();
        if (date.Length > 0) Osd(date, new Vector2(margin, h - margin * 1.9f), fs, main);
    }
}
