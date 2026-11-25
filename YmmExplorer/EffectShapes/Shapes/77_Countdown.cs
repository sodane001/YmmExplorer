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
//  カウントダウン・タイマー枠
//   7セグ風の数字と円形リング。秒の変わり目で数字が弾む
// =====================================================================
public sealed class CountdownPlugin : IShapePlugin
{
    public string Name => "演出：カウントダウン・タイマー枠";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CountdownParameter(sharedData);
}

public sealed class CountdownParameter : EffectShapeParameterBase
{
    public CountdownParameter() : this(null) { }
    public CountdownParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "表示", Name = "種類")]
    [EnumComboBox]
    public CountMode Mode { get => mode; set => Set(ref mode, value); }
    CountMode mode = CountMode.Countdown;

    [Display(GroupName = "表示", Name = "数字の色")]
    [ColorPicker]
    public WpfColor DigitColor { get => digit; set => Set(ref digit, value); }
    WpfColor digit = WpfColor.FromRgb(0x39, 0xE8, 0xFF);

    [Display(GroupName = "表示", Name = "枠・リングの色")]
    [ColorPicker]
    public WpfColor FrameColor { get => frame; set => Set(ref frame, value); }
    WpfColor frame = WpfColor.FromRgb(0xE8, 0xEE, 0xF4);

    [Display(GroupName = "表示", Name = "警告の色（残りわずか）")]
    [ColorPicker]
    public WpfColor WarnColor { get => warn; set => Set(ref warn, value); }
    WpfColor warn = WpfColor.FromRgb(0xFF, 0x3C, 0x3C);

    [Display(GroupName = "時間", Name = "開始する秒数（カウントダウン）")]
    [AnimationSlider("F0", "秒", 1, 120)]
    public Animation StartSeconds { get; } = new Animation(10, 1, 3600);

    [Display(GroupName = "時間", Name = "くり返す")]
    [ToggleSlider]
    public bool Loop { get => loop; set => Set(ref loop, value); }
    bool loop = false;

    [Display(GroupName = "時間", Name = "始まるまでの待ち時間")]
    [AnimationSlider("F1", "秒", 0, 10)]
    public Animation Delay { get; } = new Animation(0, 0, 600);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "数字の高さ")]
    [AnimationSlider("F0", "px", 40, 600)]
    public Animation DigitHeight { get; } = new Animation(220, 10, 4000);

    [Display(GroupName = "位置と大きさ", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 1, 24)]
    public Animation Thickness { get; } = new Animation(12, 0.5, 200);

    [Display(GroupName = "効果", Name = "秒の変わり目に弾ませる")]
    [ToggleSlider]
    public bool Pop { get => pop; set => Set(ref pop, value); }
    bool pop = true;

    [Display(GroupName = "効果", Name = "枠を表示する")]
    [ToggleSlider]
    public bool ShowFrame { get => showFrame; set => Set(ref showFrame, value); }
    bool showFrame = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [StartSeconds, Delay, PosX, PosY, DigitHeight, Thickness];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float DH = c.F(DigitHeight), th = c.F(Thickness);
        float start = MathF.Max(1f, c.F(StartSeconds));
        float el = MathF.Max(0f, t - c.F(Delay));

        // ---- 表示する値 ----
        float remaining, elapsed;
        if (Mode == CountMode.Countdown || Mode == CountMode.Ring)
        {
            float tt = Loop ? Gfx.Mod(el, start + 1f) : MathF.Min(el, start);
            remaining = MathF.Max(0f, start - tt);
            elapsed = tt;
        }
        else
        {
            remaining = 0f;
            elapsed = Loop ? Gfx.Mod(el, start) : el;
        }
        bool low = (Mode == CountMode.Countdown || Mode == CountMode.Ring) && remaining <= 3f && remaining > 0f;
        var col = low ? Gfx.Mix(DigitColor, WarnColor, 0.9f) : DigitColor;
        if ((Mode == CountMode.Countdown || Mode == CountMode.Ring) && remaining <= 0f && el > 0f) col = WarnColor;

        // 秒の変わり目で数字が大きくなってから戻る
        float frac = (Mode == CountMode.Countdown || Mode == CountMode.Ring) ? 1f - (remaining - MathF.Floor(remaining)) : elapsed - MathF.Floor(elapsed);
        float pop = Pop ? 1f + 0.22f * MathF.Exp(-9f * frac) : 1f;

        // ---- 表示する文字（桁）----
        string text;
        switch (Mode)
        {
            case CountMode.Timer:
                {
                    int total = (int)MathF.Floor(elapsed);
                    text = $"{Math.Min(99, total / 60):00}:{total % 60:00}";
                    break;
                }
            case CountMode.Stopwatch:
                {
                    int total = (int)MathF.Floor(elapsed * 100f);
                    text = $"{Math.Min(99, total / 100):00}.{total % 100:00}";
                    break;
                }
            default:
                text = ((int)MathF.Ceiling(remaining)).ToString();
                break;
        }

        float dh = DH * pop;
        float dw = dh * 0.52f, gap = dh * 0.22f;
        float totalW = 0f;
        foreach (char ch in text) totalW += (ch == ':' || ch == '.') ? gap * 1.2f : dw + gap;
        totalW -= gap;
        float x = ctr.X - totalW / 2f;

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 枠・リング ----
        if (ShowFrame)
        {
            float R = DH * 0.95f;
            if (Mode == CountMode.Ring || Mode == CountMode.Countdown)
            {
                brush.Color = Gfx.C4(FrameColor, 0.18f);
                Gfx.Polyline(dc, brush, Gfx.ArcPts(ctr, R, 0f, MathF.PI * 2f, 100), th * 0.8f);
                float prog = Math.Clamp(remaining / start, 0f, 1f);
                if (prog > 0.001f)
                {
                    var arc = Gfx.ArcPts(ctr, R, -MathF.PI / 2f, -MathF.PI / 2f + prog * MathF.PI * 2f, 100);
                    brush.Color = Gfx.C4(col, 0.18f);
                    FormKit.Stroke(dc, brush, arc, th * 2.4f, false);
                    brush.Color = Gfx.C4(col, 0.95f);
                    FormKit.Stroke(dc, brush, arc, th * 0.9f, false);
                }
                for (int i = 0; i < 60; i++)
                {
                    float a = -MathF.PI / 2f + i * MathF.PI * 2f / 60f;
                    var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    brush.Color = Gfx.C4(FrameColor, 0.6f);
                    dc.DrawLine(ctr + d * (R + th * 1.2f), ctr + d * (R + th * 1.2f + (i % 5 == 0 ? 14f : 7f)), brush, i % 5 == 0 ? 2f : 1f);
                }
            }
            else
            {
                float bw = totalW + DH * 0.9f, bh = DH * 1.5f;
                brush.Color = Gfx.C4(FrameColor, 0.9f);
                float arm = bh * 0.22f, bt = th * 0.55f;
                foreach (var (sx, sy) in new[] { (-1, -1), (1, -1), (-1, 1), (1, 1) })
                {
                    var cr = new Vector2(ctr.X + sx * bw / 2f, ctr.Y + sy * bh / 2f);
                    dc.DrawLine(cr, cr - new Vector2(sx * arm, 0f), brush, bt);
                    dc.DrawLine(cr, cr - new Vector2(0f, sy * arm), brush, bt);
                }
            }
        }

        // ---- 数字（にじみ→本体）----
        float lw = MathF.Max(2f, dh * 0.07f);
        foreach (char ch in text)
        {
            if (ch == ':' || ch == '.')
            {
                brush.Color = Gfx.C4(col, 0.95f);
                if (ch == ':')
                {
                    float blink = 0.35f + 0.65f * (MathF.Floor(elapsed * 2f) % 2f == 0f ? 1f : 0.25f);
                    brush.Color = Gfx.C4(col, blink);
                    dc.FillEllipse(new Ellipse(new Vector2(x + gap * 0.5f, ctr.Y - dh * 0.17f), lw * 0.8f, lw * 0.8f), brush);
                    dc.FillEllipse(new Ellipse(new Vector2(x + gap * 0.5f, ctr.Y + dh * 0.17f), lw * 0.8f, lw * 0.8f), brush);
                }
                else dc.FillEllipse(new Ellipse(new Vector2(x + gap * 0.5f, ctr.Y + dh * 0.5f - lw), lw * 0.9f, lw * 0.9f), brush);
                x += gap * 1.2f;
                continue;
            }
            int d = ch - '0';
            brush.Color = Gfx.C4(col, 0.20f);
            ShapeGeom.Digit(dc, brush, x, ctr.Y - dh / 2f, dw, dh, d, lw * 2.8f);
            brush.Color = Gfx.C4(col, 1f);
            ShapeGeom.Digit(dc, brush, x, ctr.Y - dh / 2f, dw, dh, d, lw);
            x += dw + gap;
        }
    }
}
