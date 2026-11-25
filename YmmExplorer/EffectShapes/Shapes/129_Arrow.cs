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

public enum ArrowStyle
{
    [Display(Name = "まっすぐ")] Straight,
    [Display(Name = "曲がり矢印")] Curve,
    [Display(Name = "手書き風")] Hand,
    [Display(Name = "点線")] Dotted,
    [Display(Name = "太い矢印（ブロック）")] Block,
}

// =====================================================================
//  矢印・指し示し
//   始点から終点へ、線が伸びていく。始点・終点は配置画面のつまみで動かせる
// =====================================================================
public sealed class ArrowPlugin : IShapePlugin
{
    public string Name => "演出：矢印・指し示し";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ArrowParameter(sharedData);
}

public sealed class ArrowParameter : EffectShapeParameterBase
{
    public ArrowParameter() : this(null) { }
    public ArrowParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "形")]
    [EnumComboBox]
    public ArrowStyle Style { get => style; set => Set(ref style, value); }
    ArrowStyle style = ArrowStyle.Curve;

    [Display(GroupName = "形", Name = "曲がり具合", Description = "マイナスにすると、反対側へ曲がります")]
    [AnimationSlider("F0", "%", -100, 100)]
    public Animation Bend { get; } = new Animation(30, -300, 300);

    [Display(GroupName = "形", Name = "線の太さ")]
    [AnimationSlider("F0", "px", 2, 60)]
    public Animation LineWidth { get; } = new Animation(14, 0.5, 500);

    [Display(GroupName = "形", Name = "矢じりの大きさ")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation HeadSize { get; } = new Animation(52, 0, 1000);

    [Display(GroupName = "形", Name = "両側に矢じり")]
    [ToggleSlider]
    public bool BothEnds { get => bothEnds; set => Set(ref bothEnds, value); }
    bool bothEnds = false;

    [Display(GroupName = "色", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x3B, 0x30);

    [Display(GroupName = "色", Name = "ふちの色")]
    [ColorPicker]
    public WpfColor OutlineColor { get => outlineColor; set => Set(ref outlineColor, value); }
    WpfColor outlineColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "色", Name = "ふちの太さ")]
    [AnimationSlider("F1", "px", 0, 20)]
    public Animation OutlineWidth { get; } = new Animation(4, 0, 200);

    [Display(GroupName = "色", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "位置", Name = "始点X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation StartX { get; } = new Animation(25, -100, 200);

    [Display(GroupName = "位置", Name = "始点Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation StartY { get; } = new Animation(70, -100, 200);

    [Display(GroupName = "位置", Name = "終点X（矢じり）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation EndX { get; } = new Animation(62, -100, 200);

    [Display(GroupName = "位置", Name = "終点Y（矢じり）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation EndY { get; } = new Animation(38, -100, 200);

    [Display(GroupName = "動き", Name = "伸びる時間")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation DrawTime { get; } = new Animation(0.5, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.2, 0, 60);

    [Display(GroupName = "動き", Name = "先がつつく動き")]
    [AnimationSlider("F0", "px", 0, 40)]
    public Animation Poke { get; } = new Animation(8, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Bend, LineWidth, HeadSize, OutlineWidth, Opacity, StartX, StartY, EndX, EndY, DrawTime, OutTime, Poke];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float drawT = c.F(DrawTime), outT = c.F(OutTime);
        float p = drawT <= 0.001f ? 1f : DrawKit.EaseOut(t / drawT);
        float alpha = c.F(Opacity) / 100f * (outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f));
        if (p <= 0.001f || alpha <= 0.002f) return;

        var a = new Vector2(c.F(StartX) / 100f * w, c.F(StartY) / 100f * h);
        var b = new Vector2(c.F(EndX) / 100f * w, c.F(EndY) / 100f * h);
        var d = b - a;
        float len = d.Length();
        if (len < 1f) return;
        var dir = d / len;
        var n = new Vector2(-dir.Y, dir.X);
        // 伸びきった後に、先が少しつつく
        if (p >= 1f) b += dir * MathF.Sin(MathF.Max(0f, t - drawT) * 9f) * c.F(Poke) * 0.5f;

        float bend = Style == ArrowStyle.Straight || Style == ArrowStyle.Block ? 0f : c.F(Bend) / 100f;
        var ctrl = (a + b) / 2f + n * len * 0.5f * bend;
        var path = BubbleKit.Quad(a, ctrl, b, 64);
        if (Style == ArrowStyle.Hand)
            for (int i = 1; i < path.Count - 1; i++)
                path[i] += n * MathF.Sin(i * 0.9f + Seed) * 2.2f + dir * MathF.Cos(i * 1.7f + Seed * 0.3f) * 1.2f;

        float lw = c.F(LineWidth), hs = c.F(HeadSize), ow = c.F(OutlineWidth);
        var shown = BubbleKit.PartByLength(path, p);
        if (shown.Count < 2) return;
        // 矢じりの向きは、見えている線の先の向き
        var tip = shown[^1];
        var back = shown[Math.Max(0, shown.Count - 4)];
        var headDir = tip - back;
        if (headDir.LengthSquared() < 1e-4f) headDir = dir;
        headDir = Vector2.Normalize(headDir);
        // 線は矢じりの手前で止める(先が太らないように)
        var body = BubbleKit.PartByLength(shown, MathF.Max(0f, 1f - hs * 0.6f / MathF.Max(1f, BubbleKit.Length(shown))));
        var tailHeadDir = path.Count > 3 ? Vector2.Normalize(path[0] - path[3]) : -dir;

        using var brush = dc.CreateSolidColorBrush(default);
        var col = Gfx.C4(Color, alpha);
        var oc = Gfx.C4(OutlineColor, alpha);
        float headK = Math.Clamp((p - 0.6f) / 0.4f, 0f, 1f);

        void DrawAll(Color4 cc, float extra)
        {
            brush.Color = cc;
            if (Style == ArrowStyle.Block)
            {
                // 太い矢印: 帯 + 大きな三角
                float bodyW = lw * 2.2f + extra * 2f;
                var bEnd = tip - headDir * hs * 0.9f;
                var bn = new Vector2(-headDir.Y, headDir.X);
                if (Vector2.Distance(a, bEnd) > 1f)
                    Gfx.FillPoly(dc, brush, new List<Vector2> { a + bn * bodyW / 2f, bEnd + bn * bodyW / 2f, bEnd - bn * bodyW / 2f, a - bn * bodyW / 2f });
                if (headK > 0f)
                {
                    var head = BubbleKit.ArrowHead(tip + headDir * extra * 1.4f, headDir, (hs + extra * 2f) * headK, 0.75f);
                    Gfx.FillPoly(dc, brush, head);
                }
                return;
            }
            if (Style == ArrowStyle.Dotted)
            {
                float gap = MathF.Max(lw * 2.2f, 6f);
                float total = BubbleKit.Length(body);
                for (float s = 0f; s <= total; s += gap)
                {
                    var q = BubbleKit.PartByLength(body, total <= 0 ? 0 : s / total)[^1];
                    dc.FillEllipse(new Ellipse(q, lw * 0.55f + extra, lw * 0.55f + extra), brush);
                }
            }
            else
            {
                Gfx.Polyline(dc, brush, body, lw + extra * 2f);
                if (Style == ArrowStyle.Hand)
                {
                    // 手書き風: 少しずれた細い線を重ねる
                    var second = body.Select((q, i) => q + n * MathF.Sin(i * 0.5f + Seed) * 1.6f).ToList();
                    Gfx.Polyline(dc, brush, second, MathF.Max(1f, lw * 0.45f + extra * 2f));
                }
            }
            if (headK > 0f && hs > 0.5f)
            {
                if (Style == ArrowStyle.Hand)
                {
                    // 手書き風の矢じりは、線2本
                    float L = hs * headK;
                    var hn = new Vector2(-headDir.Y, headDir.X);
                    Gfx.Polyline(dc, brush, new List<Vector2> { tip - headDir * L + hn * L * 0.62f, tip, tip - headDir * L - hn * L * 0.62f }, lw + extra * 2f);
                }
                else Gfx.FillPoly(dc, brush, BubbleKit.ArrowHead(tip + headDir * extra * 1.4f, headDir, (hs + extra * 2f) * headK));
            }
            if (BothEnds && hs > 0.5f)
                Gfx.FillPoly(dc, brush, BubbleKit.ArrowHead(a + tailHeadDir * extra * 1.4f, tailHeadDir, hs + extra * 2f));
        }

        if (ow > 0.05f && oc.A > 0.002f) DrawAll(oc, ow);
        DrawAll(col, 0f);
    }
}
