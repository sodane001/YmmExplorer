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

public enum FlowchartLayout
{
    [Display(Name = "横（左から右）")] Horizontal,
    [Display(Name = "縦（上から下）")] Vertical,
    [Display(Name = "折り返し（2段）")] Zigzag,
}

// =====================================================================
//  フローチャート・手順図
//   箱と矢印が、順番に現れてつながる。1行に1つの手順。
//   「?」で始めるとひし形(分かれ道)、「(」で始めると角の丸い箱(はじめ・おわり)
// =====================================================================
public sealed class FlowchartPlugin : IShapePlugin
{
    public string Name => "演出：フローチャート・手順図";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FlowchartParameter(sharedData);
}

public sealed class FlowchartParameter : EffectShapeParameterBase
{
    public FlowchartParameter() : this(null) { }
    public FlowchartParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "手順", Name = "手順（1行に1つ）")]
    [TextEditor(AcceptsReturn = true)]
    public string Steps { get => steps; set => Set(ref steps, value); }
    string steps = "(スタート\n材料を準備\n?うまく混ざった？\n焼く\n(完成！";

    [Display(GroupName = "手順", Name = "並べ方")]
    [EnumComboBox]
    public FlowchartLayout Direction { get => direction; set => Set(ref direction, value); }
    FlowchartLayout direction = FlowchartLayout.Horizontal;

    [Display(GroupName = "見た目", Name = "箱の幅")]
    [AnimationSlider("F0", "px", 80, 600)]
    public Animation BoxWidth { get; } = new Animation(270, 10, 4000);

    [Display(GroupName = "見た目", Name = "箱の高さ")]
    [AnimationSlider("F0", "px", 40, 400)]
    public Animation BoxHeight { get; } = new Animation(130, 10, 4000);

    [Display(GroupName = "見た目", Name = "箱の間")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation Gap { get; } = new Animation(90, 0, 3000);

    [Display(GroupName = "見た目", Name = "箱の色")]
    [ColorPicker]
    public WpfColor BoxColor { get => boxColor; set => Set(ref boxColor, value); }
    WpfColor boxColor = WpfColor.FromRgb(0x2B, 0x7B, 0xE8);

    [Display(GroupName = "見た目", Name = "分かれ道の色")]
    [ColorPicker]
    public WpfColor DecisionColor { get => decisionColor; set => Set(ref decisionColor, value); }
    WpfColor decisionColor = WpfColor.FromRgb(0xF5, 0x9E, 0x0B);

    [Display(GroupName = "見た目", Name = "はじめ・おわりの色")]
    [ColorPicker]
    public WpfColor EndColor { get => endColor; set => Set(ref endColor, value); }
    WpfColor endColor = WpfColor.FromRgb(0x2E, 0xA0, 0x4E);

    [Display(GroupName = "見た目", Name = "矢印の色")]
    [ColorPicker]
    public WpfColor ArrowColor { get => arrowColor; set => Set(ref arrowColor, value); }
    WpfColor arrowColor = WpfColor.FromRgb(0xEE, 0xEE, 0xEE);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 10, 100)]
    public Animation FontSize { get; } = new Animation(34, 4, 1000);

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "動き", Name = "1つずつ出る間隔")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation Interval { get; } = new Animation(0.55, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [BoxWidth, BoxHeight, Gap, FontSize, PosX, PosY, Interval];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var items = TextKit.Lines(Steps).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        int n = items.Count;
        if (n == 0) return;
        float k = DrawKit.InOut(c, 0.2f, 0.25f);
        if (k <= 0.001f) return;
        float bw = c.F(BoxWidth), bh = c.F(BoxHeight), gap = c.F(Gap), fs = c.F(FontSize), iv = c.F(Interval);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);

        // 箱の位置
        var pos = new Vector2[n];
        if (Direction == FlowchartLayout.Vertical)
        {
            float total = n * bh + (n - 1) * gap;
            for (int i = 0; i < n; i++) pos[i] = new Vector2(ctr.X, ctr.Y - total / 2f + bh / 2f + i * (bh + gap));
        }
        else if (Direction == FlowchartLayout.Zigzag)
        {
            int per = (n + 1) / 2;
            float total = per * bw + (per - 1) * gap;
            for (int i = 0; i < n; i++)
            {
                int row = i / per, col = i % per;
                if (row == 1) col = per - 1 - col;      // 2段目は右から左へ
                pos[i] = new Vector2(ctr.X - total / 2f + bw / 2f + col * (bw + gap), ctr.Y + (row - 0.5f) * (bh + gap * 1.2f));
            }
        }
        else
        {
            float total = n * bw + (n - 1) * gap;
            for (int i = 0; i < n; i++) pos[i] = new Vector2(ctr.X - total / 2f + bw / 2f + i * (bw + gap), ctr.Y);
        }

        using var brush = dc.CreateSolidColorBrush(default);
        // 矢印(箱の前に描く)
        for (int i = 0; i + 1 < n; i++)
        {
            float at = 0.15f + i * iv + iv * 0.55f;
            float p = Math.Clamp((t - at) / MathF.Max(0.05f, iv * 0.45f), 0f, 1f);
            if (p <= 0f) continue;
            var a = pos[i]; var b = pos[i + 1];
            var d = b - a;
            if (d.LengthSquared() < 1f) continue;
            var dir = Vector2.Normalize(d);
            // 箱のふちから、ふちへ
            float ea = MathF.Abs(dir.X) > MathF.Abs(dir.Y) ? bw / 2f / MathF.Max(0.01f, MathF.Abs(dir.X)) : bh / 2f / MathF.Max(0.01f, MathF.Abs(dir.Y));
            ea = MathF.Min(ea, MathF.Sqrt(bw * bw + bh * bh) / 2f);
            var s0 = a + dir * (ea + 6f);
            var s1 = b - dir * (ea + 6f);
            var tip = Vector2.Lerp(s0, s1, p);
            float lw = MathF.Max(3f, bh * 0.05f);
            brush.Color = Gfx.C4(ArrowColor, k);
            dc.DrawLine(s0, tip - dir * lw * 2f * MathF.Min(1f, p * 3f), brush, lw);
            Gfx.FillPoly(dc, brush, BubbleKit.ArrowHead(tip, dir, lw * 4f));
        }
        // 箱
        for (int i = 0; i < n; i++)
        {
            float at = 0.15f + i * iv;
            float p = Math.Clamp((t - at) / 0.3f, 0f, 1f);
            if (p <= 0f) continue;
            float e = DrawKit.EaseOutBack(p);
            string raw = items[i];
            bool decision = raw.StartsWith("?") || raw.StartsWith("？");
            bool term = raw.StartsWith("(") || raw.StartsWith("（");
            string text = decision || term ? raw[1..].Trim() : raw;
            var pc = pos[i];
            List<Vector2> shape;
            WpfColor col;
            if (decision)
            {
                shape = new List<Vector2> { pc + new Vector2(0, -bh * 0.62f), pc + new Vector2(bw * 0.58f, 0), pc + new Vector2(0, bh * 0.62f), pc + new Vector2(-bw * 0.58f, 0) };
                col = DecisionColor;
            }
            else if (term) { shape = ShapeGeom.RoundRect(pc.X, pc.Y, bw, bh, bh / 2f); col = EndColor; }
            else { shape = ShapeGeom.RoundRect(pc.X, pc.Y, bw, bh, bh * 0.14f); col = BoxColor; }
            shape = BubbleKit.Scale(shape, pc, e);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f * k);
            Gfx.FillPoly(dc, brush, shape.Select(q => q + new Vector2(0f, bh * 0.07f)).ToList());
            using (var g = Gfx.Linear(dc, pc - new Vector2(0, bh / 2f), pc + new Vector2(0, bh / 2f), (0f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.18f), k)), (1f, Gfx.C4(col, k))))
                Gfx.FillPoly(dc, g, shape);
            var ts = TextKit.Get(text, fs, true, Font, (decision ? bw * 0.7f : bw * 0.88f), 1);
            float fit = MathF.Min(1f, (decision ? bh * 0.8f : bh * 0.88f) / MathF.Max(1f, ts.Height));
            TextKit.DrawCentered(dc, brush, ts, pc, Gfx.C4(1f, 1f, 1f, k), Gfx.C4(0f, 0f, 0f, 0.3f * k), fs * 0.03f, e * fit);
        }
    }
}
