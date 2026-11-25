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
//  会話ウィンドウ(ノベルゲーム風)
//   名前の欄と、1文字ずつ出るセリフ、言い終わると点滅する ▼
// =====================================================================
public sealed class NovelWindowPlugin : IShapePlugin
{
    public string Name => "演出：会話ウィンドウ（ノベルゲーム風）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NovelWindowParameter(sharedData);
}

public sealed class NovelWindowParameter : EffectShapeParameterBase
{
    public NovelWindowParameter() : this(null) { }
    public NovelWindowParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "名前")]
    [TextEditor(AcceptsReturn = false)]
    public string Speaker { get => speaker; set => Set(ref speaker, value); }
    string speaker = "キャラ名";

    [Display(GroupName = "文字", Name = "セリフ")]
    [TextEditor(AcceptsReturn = true)]
    public string Text { get => text; set => Set(ref text, value); }
    string text = "ここにセリフが入ります。\n1文字ずつ表示されます。";

    [Display(GroupName = "文字", Name = "1文字ずつ出す（文字/秒）", Description = "0 なら一度に出します")]
    [AnimationSlider("F0", "文字/秒", 0, 60)]
    public Animation TypeSpeed { get; } = new Animation(18, 0, 1000);

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "文字", Name = "セリフの大きさ")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation FontSize { get; } = new Animation(46, 4, 1000);

    [Display(GroupName = "文字", Name = "名前の大きさ")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation NameSize { get; } = new Animation(38, 4, 1000);

    [Display(GroupName = "見た目", Name = "窓の形")]
    [EnumComboBox]
    public GameWindowStyle Style { get => style; set => Set(ref style, value); }
    GameWindowStyle style = GameWindowStyle.Modern;

    [Display(GroupName = "見た目", Name = "アクセントの色（名前の欄）")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0x7A, 0xA8);

    [Display(GroupName = "見た目", Name = "言い終わったら ▼ を出す")]
    [ToggleSlider]
    public bool NextMark { get => nextMark; set => Set(ref nextMark, value); }
    bool nextMark = true;

    [Display(GroupName = "位置と大きさ", Name = "窓の下端（Y）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BottomY { get; } = new Animation(96, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "窓の高さ")]
    [AnimationSlider("F0", "px", 80, 600)]
    public Animation BoxHeight { get; } = new Animation(270, 10, 4000);

    [Display(GroupName = "位置と大きさ", Name = "左右の余白")]
    [AnimationSlider("F0", "px", 0, 400)]
    public Animation Margin { get; } = new Animation(70, 0, 4000);

    [Display(GroupName = "動き", Name = "出る時間")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation InTime { get; } = new Animation(0.25, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.2, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [TypeSpeed, FontSize, NameSize, BottomY, BoxHeight, Margin, InTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float inT = c.F(InTime);
        float k = DrawKit.InOut(c, inT, c.F(OutTime));
        if (k <= 0.001f) return;
        float m = c.F(Margin), bh = c.F(BoxHeight), fs = c.F(FontSize), ns = c.F(NameSize);
        float bottom = c.F(BottomY) / 100f * h;
        float rise = (1f - DrawKit.EaseOut(k)) * bh * 0.25f;
        var rect = new RawRectF(m, bottom - bh + rise, w - m, bottom + rise);
        float u = MathF.Max(1f, fs / 46f);
        using var brush = dc.CreateSolidColorBrush(default);
        GameWindowKit.Window(dc, brush, Style, rect, Accent, k, u);

        // 名前の欄(窓の左上に重ねる)
        string name = TextKit.Lines(Speaker).Trim();
        if (name.Length > 0)
        {
            var ns2 = TextKit.Get(name, ns, true, Font);
            float tw = ns2.Width + ns * 1.2f, th = ns2.Height + ns * 0.35f;
            var nc = new Vector2(rect.Left + fs * 0.6f + tw / 2f, rect.Top);
            var tag = ShapeGeom.RoundRect(nc.X, nc.Y, tw, th, th * (Style == GameWindowStyle.Rpg || Style == GameWindowStyle.Retro ? 0.08f : 0.5f));
            brush.Color = Gfx.C4(Accent, k);
            Gfx.FillPoly(dc, brush, tag);
            TextKit.DrawCentered(dc, brush, ns2, nc, Gfx.C4(1f, 1f, 1f, k), Gfx.C4(0f, 0f, 0f, 0.35f * k), ns * 0.05f);
        }

        // セリフ(1文字ずつ)
        string full = TextKit.Lines(Text);
        float wrap = rect.Right - rect.Left - fs * 1.6f;
        float cps = c.F(TypeSpeed);
        float start = inT;
        string shown = TextKit.Typed(full, t - start, cps);
        var textCol = GameWindowKit.TextColorFor(Style);
        var topLeft = new Vector2(rect.Left + fs * 0.8f, rect.Top + fs * (name.Length > 0 ? 0.95f : 0.6f));
        if (shown.Length > 0)
        {
            var s = TextKit.Get(shown, fs, false, Font, wrap, 0);
            var shadow = Style == GameWindowStyle.Soft ? default : Gfx.C4(0f, 0f, 0f, 0.5f * k);
            TextKit.Draw(dc, brush, s, topLeft, Gfx.C4(textCol, k), shadow: shadow, shadowOffset: new Vector2(2f, 2f));
        }

        // 言い終わったら ▼
        bool done = cps <= 0.001f || shown.Length >= full.Length;
        if (NextMark && done)
        {
            float bob = MathF.Abs(MathF.Sin(t * 4f)) * fs * 0.18f;
            var p = new Vector2(rect.Right - fs * 0.9f, rect.Bottom - fs * 0.75f + bob);
            float s2 = fs * 0.32f;
            brush.Color = Gfx.C4(Style == GameWindowStyle.Soft ? Accent : WpfColor.FromRgb(255, 255, 255), k * (0.6f + 0.4f * MathF.Abs(MathF.Sin(t * 4f))));
            Gfx.FillPoly(dc, brush, new List<Vector2> { p + new Vector2(-s2, -s2 * 0.6f), p + new Vector2(s2, -s2 * 0.6f), p + new Vector2(0f, s2 * 0.7f) });
        }
    }
}
