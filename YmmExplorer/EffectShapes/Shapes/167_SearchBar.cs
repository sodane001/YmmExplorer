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
//  検索バーに文字が打ち込まれる
//   1文字ずつ入力され、候補が並び、検索ボタンが押される(汎用のデザイン)
// =====================================================================
public sealed class SearchBarPlugin : IShapePlugin
{
    public string Name => "演出：検索バーに打ち込む";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SearchBarParameter(sharedData);
}

public sealed class SearchBarParameter : EffectShapeParameterBase
{
    public SearchBarParameter() : this(null) { }
    public SearchBarParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "入力", Name = "打ち込む文字")]
    [TextEditor(AcceptsReturn = false)]
    public string Query { get => query; set => Set(ref query, value); }
    string query = "ゆっくり解説 おすすめ";

    [Display(GroupName = "入力", Name = "打ち込む速さ（文字/秒）")]
    [AnimationSlider("F0", "文字/秒", 2, 30)]
    public Animation TypeSpeed { get; } = new Animation(9, 0.5, 1000);

    [Display(GroupName = "入力", Name = "候補（1行に1つ。空なら出さない）")]
    [TextEditor(AcceptsReturn = true)]
    public string Suggestions { get => suggestions; set => Set(ref suggestions, value); }
    string suggestions = "ゆっくり解説 おすすめ 歴史\nゆっくり解説 おすすめ 宇宙\nゆっくり解説 おすすめ 雑学";

    [Display(GroupName = "見た目", Name = "ダークモード")]
    [ToggleSlider]
    public bool Dark { get => dark; set => Set(ref dark, value); }
    bool dark = false;

    [Display(GroupName = "見た目", Name = "アクセントの色")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0x1A, 0x73, 0xE8);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(40, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "幅")]
    [AnimationSlider("F0", "px", 400, 1800)]
    public Animation BarWidth { get; } = new Animation(1150, 50, 8000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [TypeSpeed, PosX, PosY, BarWidth];

    static void Magnifier(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 c, float r, float lw)
    {
        dc.DrawEllipse(new Ellipse(c + new Vector2(-r * 0.15f, -r * 0.15f), r * 0.55f, r * 0.55f), brush, lw);
        dc.DrawLine(c + new Vector2(r * 0.25f, r * 0.25f), c + new Vector2(r * 0.7f, r * 0.7f), brush, lw * 1.2f);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        float bw = c.F(BarWidth), u = bw / 1150f, bh = 110f * u;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        var bg = Dark ? WpfColor.FromRgb(0x30, 0x31, 0x34) : WpfColor.FromRgb(255, 255, 255);
        var fg = Dark ? WpfColor.FromRgb(0xE8, 0xEA, 0xED) : WpfColor.FromRgb(0x20, 0x21, 0x24);
        var sub = Dark ? WpfColor.FromRgb(0x9A, 0xA0, 0xA6) : WpfColor.FromRgb(0x70, 0x75, 0x7A);
        string full = TextKit.Lines(Query);
        float cps = c.F(TypeSpeed), start = 0.4f;
        string typed = TextKit.Typed(full, t - start, cps);
        bool doneTyping = typed.Length >= full.Length;
        float doneAt = start + full.Length / MathF.Max(0.5f, cps);
        float pressAt = doneAt + 0.6f;
        bool pressed = t >= pressAt;
        using var brush = dc.CreateSolidColorBrush(default);

        // 候補(入力中に下へ開く)
        var sugg = TextKit.Lines(Suggestions).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        float open = sugg.Count > 0 && typed.Length > 0 && !pressed ? Math.Clamp((t - start - 0.3f) / 0.25f, 0f, 1f) : 0f;
        float listH = sugg.Count * 80f * u * open;
        var whole = ShapeGeom.RoundRect(ctr.X, ctr.Y + listH / 2f, bw, bh + listH, MathF.Min(bh / 2f, open > 0f ? 30f * u : bh / 2f));
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f * k);
        Gfx.FillPoly(dc, brush, whole.Select(q => q + new Vector2(0f, 8f * u)).ToList());
        brush.Color = Gfx.C4(bg, k);
        Gfx.FillPoly(dc, brush, whole);
        brush.Color = Gfx.C4(sub, 0.4f * k);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(whole), 1.5f * u);

        // 虫めがね・入力した文字・カーソル
        brush.Color = Gfx.C4(sub, k);
        Magnifier(dc, brush, new Vector2(ctr.X - bw / 2f + 60f * u, ctr.Y), 34f * u, 4f * u);
        float tx = ctr.X - bw / 2f + 110f * u;
        var ts = TextKit.Get(typed.Length > 0 ? typed : "検索", 42f * u, false, Font);
        TextKit.Draw(dc, brush, ts, new Vector2(tx, ctr.Y - ts.Height / 2f), typed.Length > 0 ? Gfx.C4(fg, k) : Gfx.C4(sub, 0.7f * k));
        if (!pressed && DrawKit.Frac(t * 1.6f) < 0.55f)
        {
            float cx = tx + (typed.Length > 0 ? ts.Width : 0f) + 4f * u;
            brush.Color = Gfx.C4(Accent, k);
            dc.FillRectangle(new RawRectF(cx, ctr.Y - 26f * u, cx + 3f * u, ctr.Y + 26f * u), brush);
        }
        // 検索ボタン(押すと、へこんで波紋)
        var btn = new Vector2(ctr.X + bw / 2f - 70f * u, ctr.Y);
        float press = pressed && t < pressAt + 0.15f ? 0.9f : 1f;
        brush.Color = Gfx.C4(Accent, k);
        dc.FillEllipse(new Ellipse(btn, 40f * u * press, 40f * u * press), brush);
        brush.Color = Gfx.C4(1f, 1f, 1f, k);
        Magnifier(dc, brush, btn, 28f * u * press, 3.5f * u);
        if (pressed && t < pressAt + 0.6f)
        {
            float rp = (t - pressAt) / 0.6f;
            brush.Color = Gfx.C4(Accent, (1f - rp) * 0.6f * k);
            dc.DrawEllipse(new Ellipse(btn, 40f * u + rp * 50f * u, 40f * u + rp * 50f * u), brush, 4f * u * (1f - rp));
        }
        // 候補の行
        if (open > 0f)
        {
            brush.Color = Gfx.C4(sub, 0.3f * k);
            dc.DrawLine(new Vector2(ctr.X - bw / 2f + 30f * u, ctr.Y + bh / 2f), new Vector2(ctr.X + bw / 2f - 30f * u, ctr.Y + bh / 2f), brush, 1.5f * u);
            for (int i = 0; i < sugg.Count; i++)
            {
                float ry = ctr.Y + bh / 2f + (i + 0.5f) * 80f * u * open;
                float ra = k * open;
                brush.Color = Gfx.C4(sub, ra);
                Magnifier(dc, brush, new Vector2(ctr.X - bw / 2f + 60f * u, ry), 24f * u, 3f * u);
                var ss = TextKit.Get(sugg[i], 36f * u, false, Font);
                // 入力した部分は普通の太さ、残りは太字で(よくある見せ方)
                TextKit.Draw(dc, brush, ss, new Vector2(tx, ry - ss.Height / 2f), Gfx.C4(fg, ra));
            }
        }
        _ = doneTyping;
    }
}
