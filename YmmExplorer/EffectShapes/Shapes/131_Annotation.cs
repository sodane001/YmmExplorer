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

public enum AnnotationStyle
{
    [Display(Name = "枠で囲む")] Box,
    [Display(Name = "下線だけ")] Underline,
    [Display(Name = "角丸のふだ")] Pill,
}

// =====================================================================
//  注釈の引き出し線
//   指したい点から線を伸ばし、その先に注釈の文字を出す(図解でよく使う形)
// =====================================================================
public sealed class AnnotationPlugin : IShapePlugin
{
    public string Name => "演出：注釈の引き出し線";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AnnotationParameter(sharedData);
}

public sealed class AnnotationParameter : EffectShapeParameterBase
{
    public AnnotationParameter() : this(null) { }
    public AnnotationParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "注釈")]
    [TextEditor(AcceptsReturn = true)]
    public string Text { get => text; set => Set(ref text, value); }
    string text = "ここがポイント";

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "文字", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 10, 160)]
    public Animation FontSize { get; } = new Animation(46, 4, 1000);

    [Display(GroupName = "文字", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "形", Name = "形")]
    [EnumComboBox]
    public AnnotationStyle Style { get => style; set => Set(ref style, value); }
    AnnotationStyle style = AnnotationStyle.Box;

    [Display(GroupName = "形", Name = "線・枠の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(0xFF, 0xC1, 0x2E);

    [Display(GroupName = "形", Name = "枠の中の色")]
    [ColorPicker]
    public WpfColor BoxColor { get => boxColor; set => Set(ref boxColor, value); }
    WpfColor boxColor = WpfColor.FromArgb(0xD8, 0x14, 0x18, 0x22);

    [Display(GroupName = "形", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 1, 16)]
    public Animation LineWidth { get; } = new Animation(4, 0.5, 200);

    [Display(GroupName = "形", Name = "指す点の大きさ")]
    [AnimationSlider("F0", "px", 0, 50)]
    public Animation DotSize { get; } = new Animation(12, 0, 500);

    [Display(GroupName = "位置", Name = "指す点X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PointX { get; } = new Animation(40, -100, 200);

    [Display(GroupName = "位置", Name = "指す点Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PointY { get; } = new Animation(55, -100, 200);

    [Display(GroupName = "位置", Name = "注釈X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LabelX { get; } = new Animation(62, -100, 200);

    [Display(GroupName = "位置", Name = "注釈Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LabelY { get; } = new Animation(30, -100, 200);

    [Display(GroupName = "動き", Name = "線が伸びる時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation DrawTime { get; } = new Animation(0.45, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.2, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [FontSize, LineWidth, DotSize, PointX, PointY, LabelX, LabelY, DrawTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float drawT = c.F(DrawTime), outT = c.F(OutTime);
        float alpha = outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f);
        if (alpha <= 0.002f) return;
        float pLine = drawT <= 0.001f ? 1f : DrawKit.EaseOut(t / drawT);
        float pLabel = drawT <= 0.001f ? 1f : Math.Clamp((t - drawT * 0.85f) / MathF.Max(0.05f, drawT * 0.6f), 0f, 1f);

        var pt = new Vector2(c.F(PointX) / 100f * w, c.F(PointY) / 100f * h);
        var lb = new Vector2(c.F(LabelX) / 100f * w, c.F(LabelY) / 100f * h);
        float lw = c.F(LineWidth), dot = c.F(DotSize), fs = c.F(FontSize);
        bool right = lb.X >= pt.X;
        var shape = TextKit.Get(TextKit.Lines(Text), fs, true, Font, 0f, right ? 0 : 2);
        float pad = fs * 0.4f;
        float boxW = shape.Width + pad * 2f, boxH = shape.Height + pad * 1.2f;
        // 横の引き出し線(注釈の下側を、注釈の幅だけ横に伸ばす)
        var elbow = lb;
        var shelfEnd = lb + new Vector2((right ? 1f : -1f) * boxW, 0f);

        using var brush = dc.CreateSolidColorBrush(default);
        var lineCol = Gfx.C4(LineColor, alpha);

        // 指す点(ぽわっと広がる輪つき)
        if (dot > 0.1f)
        {
            float pulse = DrawKit.Frac(t * 0.8f);
            brush.Color = Gfx.C4(LineColor, alpha * (1f - pulse) * 0.6f);
            dc.DrawEllipse(new Ellipse(pt, dot * (1f + pulse * 1.6f), dot * (1f + pulse * 1.6f)), brush, MathF.Max(1f, lw * 0.6f));
            brush.Color = lineCol;
            float k = DrawKit.EaseOutBack(t / 0.25f);
            dc.FillEllipse(new Ellipse(pt, dot * 0.55f * k, dot * 0.55f * k), brush);
        }

        // 斜めの線 → 横の線(注釈の下)
        var path = new List<Vector2> { pt, elbow, shelfEnd };
        var part = BubbleKit.PartByLength(path, pLine);
        if (part.Count >= 2)
        {
            brush.Color = lineCol;
            Gfx.Polyline(dc, brush, part, lw);
        }
        if (pLabel <= 0f) return;

        // 注釈の枠と文字(線の上に乗る)
        float a2 = alpha * pLabel;
        float rise = (1f - DrawKit.EaseOut(pLabel)) * fs * 0.4f;
        float bx0 = right ? lb.X : lb.X - boxW;
        float by1 = lb.Y - lw * 0.5f;
        float by0 = by1 - boxH;
        var boxCenter = new Vector2(bx0 + boxW / 2f, (by0 + by1) / 2f + rise);
        switch (Style)
        {
            case AnnotationStyle.Box:
            {
                var rect = ShapeGeom.RoundRect(boxCenter.X, boxCenter.Y, boxW, boxH, fs * 0.12f);
                brush.Color = Gfx.C4(BoxColor, a2);
                Gfx.FillPoly(dc, brush, rect);
                brush.Color = Gfx.C4(LineColor, a2);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(rect), lw);
                break;
            }
            case AnnotationStyle.Pill:
            {
                var rect = ShapeGeom.RoundRect(boxCenter.X, boxCenter.Y, boxW + boxH * 0.4f, boxH, boxH / 2f);
                brush.Color = Gfx.C4(LineColor, a2);
                Gfx.FillPoly(dc, brush, rect);
                break;
            }
        }
        // 角丸のふだの時は、ふだの色の上に「枠の中の色」で文字を書く(色の透明度は使わない)
        var bc = BoxColor;
        var textCol = Style == AnnotationStyle.Pill ? Gfx.C4(bc.R / 255f, bc.G / 255f, bc.B / 255f, a2) : Gfx.C4(TextColor, a2);
        var shadow = Style == AnnotationStyle.Underline ? Gfx.C4(0f, 0f, 0f, 0.55f * a2) : default;
        TextKit.DrawCentered(dc, brush, shape, boxCenter, textCol, shadow: shadow, shadowOffset: new Vector2(2f, 3f));
    }
}
