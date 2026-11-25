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

public enum GameWindowStyle
{
    [Display(Name = "RPG風（黒地に白い枠）")] Rpg,
    [Display(Name = "いまどき（角丸・半透明）")] Modern,
    [Display(Name = "レトロ（青い窓）")] Retro,
    [Display(Name = "やわらか（白・パステル）")] Soft,
}

/// <summary>ゲーム風の窓の描き方(選択肢・会話ウィンドウで共通)</summary>
internal static class GameWindowKit
{
    public static void Window(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, GameWindowStyle style, RawRectF r, WpfColor accent, float alpha, float scale)
    {
        float cx = (r.Left + r.Right) / 2f, cy = (r.Top + r.Bottom) / 2f, bw = r.Right - r.Left, bh = r.Bottom - r.Top;
        float u = MathF.Max(1f, scale);
        switch (style)
        {
            case GameWindowStyle.Modern:
            {
                var rr = ShapeGeom.RoundRect(cx, cy, bw, bh, 18f * u);
                brush.Color = Gfx.C4(0.04f, 0.05f, 0.08f, 0.78f * alpha);
                Gfx.FillPoly(dc, brush, rr);
                brush.Color = Gfx.C4(accent, 0.9f * alpha);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(rr), 2.5f * u);
                break;
            }
            case GameWindowStyle.Retro:
            {
                using (var g = Gfx.Linear(dc, new Vector2(cx, r.Top), new Vector2(cx, r.Bottom), (0f, Gfx.C4(WpfColor.FromRgb(0x24, 0x3C, 0xC8), alpha)), (1f, Gfx.C4(WpfColor.FromRgb(0x0C, 0x14, 0x5A), alpha))))
                    dc.FillRectangle(r, g);
                brush.Color = Gfx.C4(0.86f, 0.86f, 0.9f, alpha);
                dc.DrawRectangle(new RawRectF(r.Left + 3f * u, r.Top + 3f * u, r.Right - 3f * u, r.Bottom - 3f * u), brush, 4f * u);
                break;
            }
            case GameWindowStyle.Soft:
            {
                var rr = ShapeGeom.RoundRect(cx, cy, bw, bh, MathF.Min(bh / 2f, 28f * u));
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.15f * alpha);
                Gfx.FillPoly(dc, brush, rr.Select(p => p + new Vector2(0f, 6f * u)).ToList());
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.95f * alpha);
                Gfx.FillPoly(dc, brush, rr);
                brush.Color = Gfx.C4(accent, alpha);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(rr), 4f * u);
                break;
            }
            default:
            {
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.88f * alpha);
                dc.FillRectangle(r, brush);
                brush.Color = Gfx.C4(1f, 1f, 1f, alpha);
                dc.DrawRectangle(new RawRectF(r.Left + 5f * u, r.Top + 5f * u, r.Right - 5f * u, r.Bottom - 5f * u), brush, 4f * u);
                break;
            }
        }
    }

    /// <summary>窓の地の色に合う、文字の色</summary>
    public static WpfColor TextColorFor(GameWindowStyle style) => style == GameWindowStyle.Soft ? WpfColor.FromRgb(0x33, 0x33, 0x40) : WpfColor.FromRgb(255, 255, 255);

    /// <summary>▶ の形</summary>
    public static List<Vector2> Cursor(Vector2 c, float s) => new() { c + new Vector2(-s * 0.45f, -s * 0.5f), c + new Vector2(s * 0.55f, 0f), c + new Vector2(-s * 0.45f, s * 0.5f) };
}

// =====================================================================
//  選択肢ウィンドウ
//   「はい/いいえ」などの選択肢と、点滅するカーソル(RPG 風)
// =====================================================================
public sealed class ChoiceWindowPlugin : IShapePlugin
{
    public string Name => "演出：選択肢ウィンドウ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ChoiceWindowParameter(sharedData);
}

public sealed class ChoiceWindowParameter : EffectShapeParameterBase
{
    public ChoiceWindowParameter() : this(null) { }
    public ChoiceWindowParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "選択肢", Name = "選択肢（1行に1つ）")]
    [TextEditor(AcceptsReturn = true)]
    public string Choices { get => choices; set => Set(ref choices, value); }
    string choices = "はい\nいいえ";

    [Display(GroupName = "選択肢", Name = "選んでいる番号", Description = "上から 1, 2, 3 …。移動を使えば、途中でカーソルを動かせます")]
    [AnimationSlider("F0", "番", 1, 6)]
    public Animation Selected { get; } = new Animation(1, 1, 50);

    [Display(GroupName = "選択肢", Name = "決定する時刻（0=しない）", Description = "この秒数で、選んだ項目が光って決定します")]
    [AnimationSlider("F2", "秒", 0, 10)]
    public Animation DecideTime { get; } = new Animation(0, 0, 3600);

    [Display(GroupName = "選択肢", Name = "横に並べる")]
    [ToggleSlider]
    public bool Horizontal { get => horizontal; set => Set(ref horizontal, value); }
    bool horizontal = false;

    [Display(GroupName = "見た目", Name = "窓の形")]
    [EnumComboBox]
    public GameWindowStyle Style { get => style; set => Set(ref style, value); }
    GameWindowStyle style = GameWindowStyle.Rpg;

    [Display(GroupName = "見た目", Name = "アクセントの色")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0xD5, 0x4A);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation FontSize { get; } = new Animation(50, 4, 1000);

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(55, -100, 200);

    [Display(GroupName = "動き", Name = "開く時間")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation InTime { get; } = new Animation(0.2, 0, 60);

    [Display(GroupName = "動き", Name = "閉じる時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.15, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Selected, DecideTime, FontSize, PosX, PosY, InTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var items = TextKit.Lines(Choices).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        if (items.Count == 0) return;
        float k = DrawKit.InOut(c, c.F(InTime), c.F(OutTime));
        if (k <= 0.001f) return;
        float fs = c.F(FontSize);
        var shapes = items.Select(s => TextKit.Get(s, fs, true, Font)).ToList();
        float pad = fs * 0.55f, cursorW = fs * 0.9f, rowH = fs * 1.45f;
        float bw, bh;
        if (Horizontal) { bw = shapes.Sum(s => s.Width + cursorW + pad) + pad; bh = rowH + pad * 1.2f; }
        else { bw = shapes.Max(s => s.Width) + cursorW + pad * 2f; bh = rowH * items.Count + pad * 1.2f; }
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        // 開く: 縦にのびる
        float ky = DrawKit.EaseOut(k);
        var rect = new RawRectF(ctr.X - bw / 2f, ctr.Y - bh / 2f * ky, ctr.X + bw / 2f, ctr.Y + bh / 2f * ky);
        using var brush = dc.CreateSolidColorBrush(default);
        float u = fs / 50f;
        GameWindowKit.Window(dc, brush, Style, rect, Accent, MathF.Min(1f, k * 2f), u);
        if (ky < 0.85f) return;
        float ta = Math.Clamp((ky - 0.85f) / 0.15f, 0f, 1f);

        int sel = Math.Clamp((int)MathF.Round(c.F(Selected)) - 1, 0, items.Count - 1);
        float decide = c.F(DecideTime);
        bool decided = decide > 0.001f && t >= decide;
        var textCol = GameWindowKit.TextColorFor(Style);
        float x = ctr.X - bw / 2f + pad, y = ctr.Y - bh / 2f + pad * 0.6f;
        for (int i = 0; i < items.Count; i++)
        {
            var s = shapes[i];
            var cell = Horizontal ? new Vector2(x, y) : new Vector2(x, y + i * rowH);
            var cursorPos = cell + new Vector2(cursorW * 0.4f, rowH / 2f);
            bool on = i == sel;
            if (on)
            {
                // 選んでいる行: 帯で強調し、カーソルを点滅させる
                float blink = decided ? 1f : 0.55f + 0.45f * MathF.Abs(MathF.Sin(t * 5f));
                brush.Color = Gfx.C4(Accent, 0.22f * ta * (decided ? 1f + 0.8f * MathF.Max(0f, 1f - (t - decide) * 3f) : 1f));
                dc.FillRectangle(new RawRectF(cell.X - pad * 0.3f, cell.Y + rowH * 0.08f, cell.X + cursorW + s.Width + pad * 0.3f, cell.Y + rowH * 0.92f), brush);
                brush.Color = Gfx.C4(Accent, ta * blink);
                var nudge = decided ? 0f : MathF.Sin(t * 8f) * fs * 0.06f;
                Gfx.FillPoly(dc, brush, GameWindowKit.Cursor(cursorPos + new Vector2(nudge, 0f), fs * 0.5f));
            }
            var col = on ? (decided && DrawKit.Frac((t - decide) * 6f) < 0.5f && t - decide < 0.6f ? WpfColor.FromRgb(255, 255, 255) : Accent) : textCol;
            if (Style == GameWindowStyle.Soft && on) col = Gfx.Mix(Accent, WpfColor.FromRgb(0, 0, 0), 0.35f);
            TextKit.Draw(dc, brush, s, new Vector2(cell.X + cursorW, cell.Y + (rowH - s.Height) / 2f), Gfx.C4(col, ta));
            if (Horizontal) x += s.Width + cursorW + pad;
        }
    }
}
