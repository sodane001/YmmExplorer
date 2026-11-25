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

public enum SplitLayout
{
    [Display(Name = "左右に2分割")] Two,
    [Display(Name = "上下に2分割")] TwoRows,
    [Display(Name = "3分割（左に大きく1つ＋右に2つ）")] ThreeLeft,
    [Display(Name = "横に3分割")] Three,
    [Display(Name = "4分割（2×2）")] Four,
    [Display(Name = "斜めに2分割")] Diagonal,
}

// =====================================================================
//  画面分割の枠
//   分割の線が伸びて、枠ができる。枠の中は透明なので、下に置いた動画が見える。
//   それぞれの枠に、名前のラベルもつけられる
// =====================================================================
public sealed class SplitScreenPlugin : IShapePlugin
{
    public string Name => "演出：画面分割の枠";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SplitScreenParameter(sharedData);
}

public sealed class SplitScreenParameter : EffectShapeParameterBase
{
    public SplitScreenParameter() : this(null) { }
    public SplitScreenParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "分割", Name = "分け方")]
    [EnumComboBox]
    public SplitLayout Layout { get => layout; set => Set(ref layout, value); }
    SplitLayout layout = SplitLayout.Two;

    [Display(GroupName = "分割", Name = "線の太さ")]
    [AnimationSlider("F0", "px", 0, 60)]
    public Animation LineWidth { get; } = new Animation(14, 0, 1000);

    [Display(GroupName = "分割", Name = "線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "分割", Name = "外側にも枠をつける")]
    [ToggleSlider]
    public bool OuterFrame { get => outerFrame; set => Set(ref outerFrame, value); }
    bool outerFrame = false;

    [Display(GroupName = "ラベル", Name = "ラベル（カンマ区切り。枠の順）")]
    [TextEditor(AcceptsReturn = false)]
    public string Labels { get => labels; set => Set(ref labels, value); }
    string labels = "";

    [Display(GroupName = "ラベル", Name = "ラベルの色")]
    [ColorPicker]
    public WpfColor LabelColor { get => labelColor; set => Set(ref labelColor, value); }
    WpfColor labelColor = WpfColor.FromRgb(0xE5, 0x39, 0x35);

    [Display(GroupName = "ラベル", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "動き", Name = "線が伸びる時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation InTime { get; } = new Animation(0.5, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [LineWidth, InTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float inT = c.F(InTime);
        float p = inT <= 0.001f ? 1f : DrawKit.EaseOut(t / inT);
        float k = DrawKit.InOut(c, 0f, 0.25f);
        if (k <= 0.001f || p <= 0.001f) return;
        float lw = c.F(LineWidth);
        // 線(始点, 終点)と、枠(ラベルの位置用)
        var lines = new List<(Vector2 a, Vector2 b)>();
        var cells = new List<RawRectF>();
        switch (Layout)
        {
            case SplitLayout.TwoRows:
                lines.Add((new Vector2(0, h / 2f), new Vector2(w, h / 2f)));
                cells.Add(new RawRectF(0, 0, w, h / 2f)); cells.Add(new RawRectF(0, h / 2f, w, h));
                break;
            case SplitLayout.ThreeLeft:
                lines.Add((new Vector2(w * 0.6f, 0), new Vector2(w * 0.6f, h)));
                lines.Add((new Vector2(w, h / 2f), new Vector2(w * 0.6f, h / 2f)));
                cells.Add(new RawRectF(0, 0, w * 0.6f, h)); cells.Add(new RawRectF(w * 0.6f, 0, w, h / 2f)); cells.Add(new RawRectF(w * 0.6f, h / 2f, w, h));
                break;
            case SplitLayout.Three:
                lines.Add((new Vector2(w / 3f, 0), new Vector2(w / 3f, h)));
                lines.Add((new Vector2(w * 2f / 3f, h), new Vector2(w * 2f / 3f, 0)));
                for (int i = 0; i < 3; i++) cells.Add(new RawRectF(w * i / 3f, 0, w * (i + 1) / 3f, h));
                break;
            case SplitLayout.Four:
                lines.Add((new Vector2(w / 2f, 0), new Vector2(w / 2f, h)));
                lines.Add((new Vector2(0, h / 2f), new Vector2(w, h / 2f)));
                cells.Add(new RawRectF(0, 0, w / 2f, h / 2f)); cells.Add(new RawRectF(w / 2f, 0, w, h / 2f));
                cells.Add(new RawRectF(0, h / 2f, w / 2f, h)); cells.Add(new RawRectF(w / 2f, h / 2f, w, h));
                break;
            case SplitLayout.Diagonal:
                lines.Add((new Vector2(w * 0.62f, 0), new Vector2(w * 0.38f, h)));
                cells.Add(new RawRectF(0, 0, w * 0.5f, h)); cells.Add(new RawRectF(w * 0.5f, 0, w, h));
                break;
            default:
                lines.Add((new Vector2(w / 2f, 0), new Vector2(w / 2f, h)));
                cells.Add(new RawRectF(0, 0, w / 2f, h)); cells.Add(new RawRectF(w / 2f, 0, w, h));
                break;
        }
        using var brush = dc.CreateSolidColorBrush(default);
        if (lw > 0.05f)
        {
            // 影 → 線(中央から両端へ伸びる)
            foreach (var (a, b) in lines)
            {
                var m = (a + b) / 2f;
                var a1 = Vector2.Lerp(m, a, p);
                var b1 = Vector2.Lerp(m, b, p);
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f * k);
                dc.DrawLine(a1 + new Vector2(lw * 0.2f, lw * 0.3f), b1 + new Vector2(lw * 0.2f, lw * 0.3f), brush, lw);
                brush.Color = Gfx.C4(LineColor, k);
                dc.DrawLine(a1, b1, brush, lw);
            }
            if (OuterFrame)
            {
                brush.Color = Gfx.C4(LineColor, k * p);
                dc.DrawRectangle(new RawRectF(lw / 2f, lw / 2f, w - lw / 2f, h - lw / 2f), brush, lw);
            }
        }
        // ラベル
        var names = TextKit.Split(Labels);
        float fs = MathF.Min(w, h) * 0.04f;
        for (int i = 0; i < cells.Count && i < names.Count; i++)
        {
            if (names[i].Length == 0) continue;
            float lp = Math.Clamp((t - inT - i * 0.12f) / 0.3f, 0f, 1f);
            if (lp <= 0f) continue;
            var cell = cells[i];
            var s = TextKit.Get(names[i], fs, true, Font);
            float bw = s.Width + fs * 0.9f, bh = s.Height + fs * 0.3f;
            var lc = new Vector2(cell.Left + lw + fs * 0.6f + bw / 2f - (1f - DrawKit.EaseOut(lp)) * bw, cell.Top + lw + fs * 0.6f + bh / 2f);
            var tag = ShapeGeom.RoundRect(lc.X, lc.Y, bw, bh, bh * 0.2f);
            brush.Color = Gfx.C4(LabelColor, k * lp);
            Gfx.FillPoly(dc, brush, tag);
            TextKit.DrawCentered(dc, brush, s, lc, Gfx.C4(1f, 1f, 1f, k * lp));
        }
    }
}
