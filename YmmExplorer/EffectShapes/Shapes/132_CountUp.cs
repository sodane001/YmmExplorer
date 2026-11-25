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

public enum CountEase
{
    [Display(Name = "最後にゆっくり")] EaseOut,
    [Display(Name = "一定の速さ")] Linear,
    [Display(Name = "ゆっくり→速く→ゆっくり")] InOut,
    [Display(Name = "スロット風（数字が回る）")] Slot,
}

// =====================================================================
//  数字のカウントアップ
//   はじめの数から、目標の数まで、くるくる増えていく(減らすこともできる)
// =====================================================================
public sealed class CountUpPlugin : IShapePlugin
{
    public string Name => "演出：数字のカウントアップ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CountUpParameter(sharedData);
}

public sealed class CountUpParameter : EffectShapeParameterBase
{
    public CountUpParameter() : this(null) { }
    public CountUpParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "数", Name = "はじめの数")]
    [AnimationSlider("F0", "", 0, 1000)]
    public Animation From { get; } = new Animation(0, -1e12, 1e12);

    [Display(GroupName = "数", Name = "目標の数")]
    [AnimationSlider("F0", "", 0, 100000)]
    public Animation To { get; } = new Animation(12345, -1e12, 1e12);

    [Display(GroupName = "数", Name = "小数の桁数")]
    [TextBoxSlider("F0", "桁", 0, 4)]
    [Range(0, 6)]
    public int Decimals { get => decimals; set => Set(ref decimals, value); }
    int decimals = 0;

    [Display(GroupName = "数", Name = "3桁ごとにカンマ")]
    [ToggleSlider]
    public bool Comma { get => comma; set => Set(ref comma, value); }
    bool comma = true;

    [Display(GroupName = "数", Name = "前につける文字", Description = "例: ¥ / 約 / 第")]
    [TextEditor(AcceptsReturn = false)]
    public string Prefix { get => prefix; set => Set(ref prefix, value); }
    string prefix = "";

    [Display(GroupName = "数", Name = "後ろにつける文字", Description = "例: 人 / 円 / % / 回")]
    [TextEditor(AcceptsReturn = false)]
    public string Suffix { get => suffix; set => Set(ref suffix, value); }
    string suffix = "人";

    [Display(GroupName = "動き", Name = "増え方")]
    [EnumComboBox]
    public CountEase Ease { get => ease; set => Set(ref ease, value); }
    CountEase ease = CountEase.EaseOut;

    [Display(GroupName = "動き", Name = "数え始め")]
    [AnimationSlider("F2", "秒", 0, 5)]
    public Animation Delay { get; } = new Animation(0.1, 0, 600);

    [Display(GroupName = "動き", Name = "数える時間")]
    [AnimationSlider("F2", "秒", 0.1, 10)]
    public Animation Duration { get; } = new Animation(1.6, 0.01, 600);

    [Display(GroupName = "動き", Name = "数え終わりに弾む")]
    [ToggleSlider]
    public bool Pop { get => pop; set => Set(ref pop, value); }
    bool pop = true;

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "文字", Name = "数字の大きさ")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation FontSize { get; } = new Animation(150, 4, 3000);

    [Display(GroupName = "文字", Name = "前後の文字の大きさ")]
    [AnimationSlider("F0", "%", 20, 100)]
    public Animation AffixScale { get; } = new Animation(55, 5, 300);

    [Display(GroupName = "文字", Name = "文字の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "文字", Name = "ふちの色")]
    [ColorPicker]
    public WpfColor OutlineColor { get => outlineColor; set => Set(ref outlineColor, value); }
    WpfColor outlineColor = WpfColor.FromRgb(0xE5, 0x39, 0x35);

    [Display(GroupName = "文字", Name = "ふちの太さ")]
    [AnimationSlider("F1", "px", 0, 30)]
    public Animation OutlineWidth { get; } = new Animation(8, 0, 300);

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [From, To, Delay, Duration, FontSize, AffixScale, OutlineWidth, PosX, PosY];

    string Format(double v)
    {
        string fmt = (Comma ? "#,0" : "0") + (Decimals > 0 ? "." + new string('0', Decimals) : "");
        return v.ToString(fmt, CultureInfo.InvariantCulture);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float delay = c.F(Delay), dur = MathF.Max(0.01f, c.F(Duration));
        float x = Math.Clamp((c.Time - delay) / dur, 0f, 1f);
        float e = Ease switch
        {
            CountEase.Linear => x,
            CountEase.InOut => x * x * (3f - 2f * x),
            _ => 1f - MathF.Pow(1f - x, 3f),
        };
        double from = c.F(From), to = c.F(To);
        double value = from + (to - from) * e;
        double unit = Math.Pow(10, -Decimals);
        double shown = Ease == CountEase.Slot ? value : Math.Round(value / unit) * unit;
        if (x >= 1f) shown = to;

        float fs = c.F(FontSize), afs = fs * c.F(AffixScale) / 100f, ow = c.F(OutlineWidth);
        var center = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float popK = 1f;
        if (Pop && x >= 1f)
        {
            float pt = c.Time - delay - dur;
            popK = 1f + 0.12f * MathF.Exp(-pt * 7f) * MathF.Sin(MathF.Min(pt, 0.6f) * 18f + 1.2f);
        }

        // 数字は、すべて同じ幅のマスに入れる(数が変わっても、全体の幅がぶれない)
        float cell = 0f;
        for (char d = '0'; d <= '9'; d++) cell = MathF.Max(cell, TextKit.Get(d.ToString(), fs, true, Font).Width);
        string num = Format(Ease == CountEase.Slot ? Math.Floor(Math.Abs(shown) / unit) * unit * Math.Sign(shown) : shown);
        var pre = TextKit.Get(TextKit.Lines(Prefix), afs, true, Font);
        var suf = TextKit.Get(TextKit.Lines(Suffix), afs, true, Font);
        var parts = new List<(TextShape s, float width, bool digit, int place)>();
        int digitsLeft = num.Count(char.IsDigit) - Decimals;
        int placeCounter = digitsLeft;
        foreach (char ch in num)
        {
            var s = TextKit.Get(ch.ToString(), fs, true, Font);
            bool isDigit = char.IsDigit(ch);
            int place = 0;
            if (isDigit) { placeCounter--; place = placeCounter; }
            parts.Add((s, isDigit ? cell : s.Width, isDigit, place));
        }
        float gap = fs * 0.02f;
        float total = parts.Sum(p => p.width + gap) + (pre.Width > 0 ? pre.Width + fs * 0.08f : 0f) + (suf.Width > 0 ? suf.Width + fs * 0.08f : 0f);
        float x0 = center.X - total * popK / 2f;
        float baseY = center.Y + fs * 0.55f * popK;          // 数字の下端を、そろえる

        using var brush = dc.CreateSolidColorBrush(default);
        var fill = Gfx.C4(Color, 1f);
        var outline = Gfx.C4(OutlineColor, 1f);
        var shadow = Gfx.C4(0f, 0f, 0f, 0.35f);
        float cx = x0;
        if (pre.Width > 0)
        {
            TextKit.Draw(dc, brush, pre, new Vector2(cx, baseY - pre.Height * popK * 0.92f), fill, outline, ow * 0.7f, popK, shadow, new Vector2(4f, 5f));
            cx += (pre.Width + fs * 0.08f) * popK;
        }
        foreach (var (s, width, digit, place) in parts)
        {
            float top = baseY - s.Height * popK * 0.92f;
            float ox = (width - s.Width) / 2f * popK;
            if (digit && Ease == CountEase.Slot && x < 1f)
            {
                // スロット: その桁の「小数部分」だけ、数字が上へずれていく
                double placeVal = Math.Abs(value) / unit / Math.Pow(10, place + Decimals);
                float frac = (float)(placeVal - Math.Floor(placeVal));
                int cur = (int)Math.Floor(placeVal) % 10;
                int next = (cur + 1) % 10;
                float shift = frac * s.Height * popK * 0.8f;
                var sCur = TextKit.Get(cur.ToString(CultureInfo.InvariantCulture), fs, true, Font);
                var sNext = TextKit.Get(next.ToString(CultureInfo.InvariantCulture), fs, true, Font);
                float a1 = 1f - frac, a2 = frac;
                TextKit.Draw(dc, brush, sCur, new Vector2(cx + (width - sCur.Width) / 2f * popK, top - shift), WithA(fill, a1), WithA(outline, a1), ow, popK);
                TextKit.Draw(dc, brush, sNext, new Vector2(cx + (width - sNext.Width) / 2f * popK, top - shift + s.Height * popK * 0.8f), WithA(fill, a2), WithA(outline, a2), ow, popK);
            }
            else TextKit.Draw(dc, brush, s, new Vector2(cx + ox, top), fill, outline, ow, popK, shadow, new Vector2(4f, 5f));
            cx += (width + gap) * popK;
        }
        if (suf.Width > 0)
        {
            cx += fs * 0.08f * popK - gap * popK;
            TextKit.Draw(dc, brush, suf, new Vector2(cx, baseY - suf.Height * popK * 0.92f), fill, outline, ow * 0.7f, popK, shadow, new Vector2(4f, 5f));
        }
    }

    static Color4 WithA(Color4 c, float a) => new(c.R, c.G, c.B, c.A * Math.Clamp(a, 0f, 1f));
}
