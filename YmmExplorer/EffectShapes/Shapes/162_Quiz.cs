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
//  クイズの出題・正解演出
//   問題のパネルと、4択(2〜6択)のボタン。考える時間のバーが減り、
//   答えの時刻になると正解が光って大きな ◯、ほかは暗くなる
// =====================================================================
public sealed class QuizPlugin : IShapePlugin
{
    public string Name => "演出：クイズの出題・正解";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new QuizParameter(sharedData);
}

public sealed class QuizParameter : EffectShapeParameterBase
{
    public QuizParameter() : this(null) { }
    public QuizParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "問題", Name = "問題文")]
    [TextEditor(AcceptsReturn = true)]
    public string Question { get => question; set => Set(ref question, value); }
    string question = "日本でいちばん高い山は？";

    [Display(GroupName = "問題", Name = "選択肢（1行に1つ）")]
    [TextEditor(AcceptsReturn = true)]
    public string Choices { get => choices; set => Set(ref choices, value); }
    string choices = "北岳\n富士山\n奥穂高岳\n間ノ岳";

    [Display(GroupName = "問題", Name = "正解の番号")]
    [TextBoxSlider("F0", "番", 1, 6)]
    [Range(1, 10)]
    public int Answer { get => answer; set => Set(ref answer, value); }
    int answer = 2;

    [Display(GroupName = "問題", Name = "答えを出す時刻（秒）")]
    [AnimationSlider("F1", "秒", 1, 30)]
    public Animation RevealTime { get; } = new Animation(6, 0, 3600);

    [Display(GroupName = "問題", Name = "考える時間のバーを出す")]
    [ToggleSlider]
    public bool TimerBar { get => timerBar; set => Set(ref timerBar, value); }
    bool timerBar = true;

    [Display(GroupName = "見た目", Name = "パネルの色")]
    [ColorPicker]
    public WpfColor PanelColor { get => panelColor; set => Set(ref panelColor, value); }
    WpfColor panelColor = WpfColor.FromRgb(0x1E, 0x3A, 0x8A);

    [Display(GroupName = "見た目", Name = "正解の色")]
    [ColorPicker]
    public WpfColor CorrectColor { get => correctColor; set => Set(ref correctColor, value); }
    WpfColor correctColor = WpfColor.FromRgb(0xF0, 0x3E, 0x3E);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 20, 120)]
    public Animation FontSize { get; } = new Animation(58, 4, 1000);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(55, -100, 200);

    [Display(GroupName = "位置", Name = "全体の幅")]
    [AnimationSlider("F0", "%", 30, 100)]
    public Animation WidthRatio { get; } = new Animation(86, 5, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [RevealTime, FontSize, PosY, WidthRatio];

    static readonly WpfColor[] ChoiceColors =
    {
        WpfColor.FromRgb(0xE5, 0x39, 0x35), WpfColor.FromRgb(0x1E, 0x88, 0xE5), WpfColor.FromRgb(0x43, 0xA0, 0x47),
        WpfColor.FromRgb(0xFB, 0x8C, 0x00), WpfColor.FromRgb(0x8E, 0x24, 0xAA), WpfColor.FromRgb(0x00, 0x89, 0x7B),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        var list = TextKit.Lines(Choices).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).Take(6).ToList();
        float fs = c.F(FontSize), reveal = c.F(RevealTime);
        float tw = w * c.F(WidthRatio) / 100f;
        float cy = c.F(PosY) / 100f * h;
        using var brush = dc.CreateSolidColorBrush(default);

        // 問題のパネル
        var qs = TextKit.Get(TextKit.Lines(Question), fs, true, Font, tw - fs * 3f, 1);
        float qh = qs.Height + fs * 1.1f;
        int rows = (list.Count + 1) / 2;
        float bh = fs * 1.7f, gap = fs * 0.4f;
        float total = qh + gap * 1.5f + rows * bh + (rows - 1) * gap;
        float top = cy - total / 2f;
        float pin = DrawKit.EaseOutBack(Math.Clamp(t / 0.35f, 0f, 1f));
        var qc = new Vector2(w / 2f, top + qh / 2f);
        var panel = BubbleKit.Scale(ShapeGeom.RoundRect(qc.X, qc.Y, tw, qh, fs * 0.3f), qc, pin);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f * k);
        Gfx.FillPoly(dc, brush, panel.Select(q => q + new Vector2(0f, fs * 0.15f)).ToList());
        using (var g = Gfx.Linear(dc, qc - new Vector2(0, qh / 2f), qc + new Vector2(0, qh / 2f), (0f, Gfx.C4(Gfx.Mix(PanelColor, WpfColor.FromRgb(255, 255, 255), 0.2f), k)), (1f, Gfx.C4(PanelColor, k))))
            Gfx.FillPoly(dc, g, panel);
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.85f * k);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(panel), MathF.Max(2f, fs * 0.06f));
        // 「Q.」の札
        var qtag = TextKit.Get("Q.", fs * 1.05f, true, "Arial Black");
        TextKit.DrawCentered(dc, brush, qtag, new Vector2(qc.X - tw / 2f + fs * 0.9f, qc.Y), Gfx.C4(1f, 0.85f, 0.25f, k * pin), Gfx.C4(0f, 0f, 0f, 0.5f * k), fs * 0.04f, pin);
        TextKit.DrawCentered(dc, brush, qs, qc + new Vector2(fs * 0.5f, 0f), Gfx.C4(1f, 1f, 1f, k * Math.Clamp((t - 0.25f) / 0.3f, 0f, 1f)), scale: pin);

        // 選択肢
        bool revealed = reveal > 0.001f && t >= reveal;
        float rv = revealed ? Math.Clamp((t - reveal) / 0.4f, 0f, 1f) : 0f;
        float colW = (tw - gap) / 2f;
        float y0 = top + qh + gap * 1.5f;
        for (int i = 0; i < list.Count; i++)
        {
            float at = 0.5f + i * 0.15f;
            float p = DrawKit.EaseOutBack(Math.Clamp((t - at) / 0.3f, 0f, 1f));
            if (p <= 0f) continue;
            int row = i / 2, col = i % 2;
            bool alone = list.Count % 2 == 1 && i == list.Count - 1;
            var bc = new Vector2(alone ? w / 2f : w / 2f - tw / 2f + colW / 2f + col * (colW + gap), y0 + row * (bh + gap) + bh / 2f);
            bool correct = i + 1 == Answer;
            float dim = revealed && !correct ? 1f - 0.65f * rv : 1f;
            float grow = revealed && correct ? 1f + 0.06f * MathF.Sin(MathF.Min(1f, rv) * MathF.PI) : 1f;
            var box = BubbleKit.Scale(ShapeGeom.RoundRect(bc.X, bc.Y, colW, bh, bh * 0.22f), bc, p * grow);
            var tint = ChoiceColors[i % ChoiceColors.Length];
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f * k * dim);
            Gfx.FillPoly(dc, brush, box.Select(q => q + new Vector2(0f, fs * 0.12f)).ToList());
            brush.Color = Gfx.C4(1f, 1f, 1f, k * (revealed && !correct ? 0.5f : 1f));
            Gfx.FillPoly(dc, brush, box);
            brush.Color = Gfx.C4(revealed && correct ? CorrectColor : tint, k * dim);
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(box), MathF.Max(3f, fs * 0.08f) * (revealed && correct ? 1.6f : 1f));
            // A・B・C・D の丸
            var lc = new Vector2(bc.X - colW / 2f * p + bh * 0.55f, bc.Y);
            dc.FillEllipse(new Ellipse(lc, bh * 0.34f * p, bh * 0.34f * p), brush);
            var ls = TextKit.Get(((char)('A' + i)).ToString(), bh * 0.42f, true, "Arial Black");
            TextKit.DrawCentered(dc, brush, ls, lc, Gfx.C4(1f, 1f, 1f, k * dim), scale: p);
            var cs = TextKit.Get(list[i], fs * 0.85f, true, Font, colW - bh * 1.4f, 0);
            TextKit.Draw(dc, brush, cs, new Vector2(lc.X + bh * 0.55f, bc.Y - cs.Height * p / 2f), Gfx.C4(0.12f, 0.12f, 0.15f, k * dim), scale: p);
            if (revealed && correct)
            {
                // 大きな ◯
                float ok = DrawKit.EaseOutBack(rv);
                brush.Color = Gfx.C4(CorrectColor, k * 0.85f);
                dc.DrawEllipse(new Ellipse(bc, bh * 0.9f * ok, bh * 0.9f * ok), brush, bh * 0.16f);
                DrawKit.Glow(dc, bc, bh * 1.6f, Gfx.C4(CorrectColor, 0.3f * k * (1f - rv * 0.5f)));
            }
        }

        // 考える時間のバー(パネルの下ふち)
        if (TimerBar && reveal > 0.001f && !revealed)
        {
            float start = 0.6f;
            float f = Math.Clamp(1f - (t - start) / MathF.Max(0.1f, reveal - start), 0f, 1f);
            float by = top + qh + gap * 0.6f;
            float bw = tw * f;
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f * k);
            dc.FillRectangle(new RawRectF(w / 2f - tw / 2f, by - fs * 0.09f, w / 2f + tw / 2f, by + fs * 0.09f), brush);
            brush.Color = f < 0.25f && DrawKit.Frac(t * 4f) < 0.5f ? Gfx.C4(1f, 0.25f, 0.25f, k) : Gfx.C4(1f, 0.82f, 0.2f, k);
            dc.FillRectangle(new RawRectF(w / 2f - tw / 2f, by - fs * 0.09f, w / 2f - tw / 2f + bw, by + fs * 0.09f), brush);
        }
    }
}
