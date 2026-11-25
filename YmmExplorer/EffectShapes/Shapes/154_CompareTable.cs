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

/// <summary>◎○△× の記号を、図形で描く(フォントに左右されない)</summary>
internal static class MarkKit
{
    /// <summary>記号なら描いて true。記号でなければ false</summary>
    public static bool Draw(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, string s, Vector2 c, float size, float alpha)
    {
        float r = size * 0.36f, lw = MathF.Max(2f, size * 0.09f);
        switch (s.Trim())
        {
            case "◎":
                brush.Color = Gfx.C4(WpfColor.FromRgb(0xE5, 0x39, 0x35), alpha);
                dc.DrawEllipse(new Ellipse(c, r, r), brush, lw);
                dc.DrawEllipse(new Ellipse(c, r * 0.55f, r * 0.55f), brush, lw);
                return true;
            case "○": case "〇": case "O": case "o":
                brush.Color = Gfx.C4(WpfColor.FromRgb(0xE5, 0x39, 0x35), alpha);
                dc.DrawEllipse(new Ellipse(c, r, r), brush, lw);
                return true;
            case "△":
                brush.Color = Gfx.C4(WpfColor.FromRgb(0x2E, 0xA0, 0x4E), alpha);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(new List<Vector2> { c + new Vector2(0, -r), c + new Vector2(r * 0.95f, r * 0.7f), c + new Vector2(-r * 0.95f, r * 0.7f) }), lw);
                return true;
            case "×": case "✕": case "x": case "X":
                brush.Color = Gfx.C4(WpfColor.FromRgb(0x1E, 0x6B, 0xD6), alpha);
                dc.DrawLine(c + new Vector2(-r * 0.8f, -r * 0.8f), c + new Vector2(r * 0.8f, r * 0.8f), brush, lw);
                dc.DrawLine(c + new Vector2(r * 0.8f, -r * 0.8f), c + new Vector2(-r * 0.8f, r * 0.8f), brush, lw);
                return true;
            case "-": case "ー": case "―":
                brush.Color = Gfx.C4(0.6f, 0.6f, 0.65f, alpha);
                dc.DrawLine(c + new Vector2(-r * 0.6f, 0), c + new Vector2(r * 0.6f, 0), brush, lw);
                return true;
        }
        return false;
    }
}

// =====================================================================
//  比較表・○×表
//   1行目が見出し。「項目,A,B」のようにカンマで区切る。◎○△×は記号として描く
// =====================================================================
public sealed class CompareTablePlugin : IShapePlugin
{
    public string Name => "演出：比較表・○×表";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CompareTableParameter(sharedData);
}

public sealed class CompareTableParameter : EffectShapeParameterBase
{
    public CompareTableParameter() : this(null) { }
    public CompareTableParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "データ", Name = "表（1行目は見出し。カンマ区切り）")]
    [TextEditor(AcceptsReturn = true)]
    public string Data { get => data; set => Set(ref data, value); }
    string data = "項目,プランA,プランB,プランC\n値段,◎,○,×\n速さ,△,◎,○\nサポート,○,△,◎\n使いやすさ,◎,○,△";

    [Display(GroupName = "データ", Name = "目立たせる列（0=なし）")]
    [TextBoxSlider("F0", "列目", 0, 6)]
    [Range(0, 50)]
    public int Highlight { get => highlight; set => Set(ref highlight, value); }
    int highlight = 2;

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "見出しの色")]
    [ColorPicker]
    public WpfColor HeaderColor { get => headerColor; set => Set(ref headerColor, value); }
    WpfColor headerColor = WpfColor.FromRgb(0x2B, 0x5C, 0xB8);

    [Display(GroupName = "見た目", Name = "目立たせる色")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0xB3, 0x00);

    [Display(GroupName = "見た目", Name = "表の地の色")]
    [ColorPicker]
    public WpfColor BodyColor { get => bodyColor; set => Set(ref bodyColor, value); }
    WpfColor bodyColor = WpfColor.FromRgb(0xFB, 0xFB, 0xFD);

    [Display(GroupName = "見た目", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(0x22, 0x24, 0x2A);

    [Display(GroupName = "見た目", Name = "表の幅")]
    [AnimationSlider("F0", "px", 300, 1800)]
    public Animation TableWidth { get; } = new Animation(1300, 50, 8000);

    [Display(GroupName = "見た目", Name = "行の高さ")]
    [AnimationSlider("F0", "px", 40, 200)]
    public Animation RowHeight { get; } = new Animation(100, 10, 2000);

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "動き", Name = "1行ずつ出る間隔")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation Interval { get; } = new Animation(0.5, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [TableWidth, RowHeight, PosX, PosY, Interval];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var lines = TextKit.Lines(Data).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0)
            .Select(s => s.Split(new[] { ',', '、', '\t' }).Select(x => x.Trim()).ToArray()).ToList();
        if (lines.Count == 0) return;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        int cols = lines.Max(l => l.Length);
        float tw = c.F(TableWidth), rh = c.F(RowHeight), iv = c.F(Interval);
        float firstW = tw * (cols <= 2 ? 0.5f : 0.3f);
        float colW = cols > 1 ? (tw - firstW) / (cols - 1) : tw;
        float th = rh * lines.Count;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float x0 = ctr.X - tw / 2f, y0 = ctr.Y - th / 2f;
        float fs = rh * 0.38f;
        using var brush = dc.CreateSolidColorBrush(default);

        float ColX(int j) => j == 0 ? x0 : x0 + firstW + (j - 1) * colW;
        float ColW(int j) => j == 0 ? firstW : colW;

        // 影と、表の地
        var body = ShapeGeom.RoundRect(ctr.X, ctr.Y, tw, th, rh * 0.15f);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f * k);
        Gfx.FillPoly(dc, brush, body.Select(q => q + new Vector2(0f, rh * 0.1f)).ToList());
        brush.Color = Gfx.C4(BodyColor, k);
        Gfx.FillPoly(dc, brush, body);
        // 見出しの行
        brush.Color = Gfx.C4(HeaderColor, k);
        Gfx.FillPoly(dc, brush, DrawKit.ClipRect(body, x0, y0, x0 + tw, y0 + rh));

        for (int i = 0; i < lines.Count; i++)
        {
            float at = 0.15f + i * iv;
            float p = Math.Clamp((t - at) / 0.3f, 0f, 1f);
            float a = k * p;
            float y = y0 + i * rh;
            if (i > 0 && i % 2 == 0)
            {
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.035f * k);
                dc.FillRectangle(new RawRectF(x0, y, x0 + tw, y + rh), brush);
            }
            if (i > 0)
            {
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.1f * k);
                dc.DrawLine(new Vector2(x0, y), new Vector2(x0 + tw, y), brush, 1.5f);
            }
            if (a <= 0.001f) continue;
            for (int j = 0; j < lines[i].Length && j < cols; j++)
            {
                var cell = new Vector2(ColX(j) + ColW(j) / 2f + (1f - DrawKit.EaseOut(p)) * rh * 0.5f, y + rh / 2f);
                string s = lines[i][j];
                if (i > 0 && j > 0)
                {
                    float pop = DrawKit.EaseOutBack(Math.Clamp((t - at - j * 0.06f) / 0.3f, 0f, 1f));
                    if (pop > 0f && MarkKit.Draw(dc, brush, s, cell, rh * pop, a)) continue;
                }
                var ts = TextKit.Get(s, i == 0 ? fs * 1.05f : fs, i == 0 || j == 0, Font, ColW(j) * 0.9f, 1);
                var col = i == 0 ? Gfx.C4(1f, 1f, 1f, a) : Gfx.C4(TextColor, a);
                TextKit.DrawCentered(dc, brush, ts, cell, col);
            }
        }
        // 列の区切り
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.1f * k);
        for (int j = 1; j < cols; j++) dc.DrawLine(new Vector2(ColX(j), y0 + rh), new Vector2(ColX(j), y0 + th), brush, 1.5f);
        // 目立たせる列(全部出てから、枠が光る)
        if (Highlight >= 2 && Highlight <= cols)
        {
            int j = Highlight - 1;
            float at = 0.15f + lines.Count * iv;
            float p = DrawKit.EaseOutBack(Math.Clamp((t - at) / 0.4f, 0f, 1f));
            if (p > 0f)
            {
                float cx = ColX(j) + ColW(j) / 2f;
                var frame = ShapeGeom.RoundRect(cx, ctr.Y, ColW(j) * (0.9f + 0.1f * p), th + rh * 0.3f * p, rh * 0.2f);
                brush.Color = Gfx.C4(Accent, k * MathF.Min(1f, p));
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(frame), MathF.Max(3f, rh * 0.06f));
                var tag = TextKit.Get("おすすめ", rh * 0.3f, true, Font);
                var tc = new Vector2(cx, ctr.Y - th / 2f - rh * 0.15f * p - rh * 0.1f);
                var pill = ShapeGeom.RoundRect(tc.X, tc.Y, tag.Width + rh * 0.4f, tag.Height + rh * 0.1f, rh * 0.25f);
                Gfx.FillPoly(dc, brush, pill);
                TextKit.DrawCentered(dc, brush, tag, tc, Gfx.C4(1f, 1f, 1f, k * MathF.Min(1f, p)));
            }
        }
    }
}
