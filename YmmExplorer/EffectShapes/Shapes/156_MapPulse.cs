using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
//  地図の広がり(波紋で場所を示す)
//   地図の画像の上に重ねて、場所から波紋を広げ、ラベルを出す。
//   1行に「X%,Y%:ラベル」(例: 62,40:東京)。順番に現れる
// =====================================================================
public sealed class MapPulsePlugin : IShapePlugin
{
    public string Name => "演出：地図の広がり（波紋で場所を示す）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MapPulseParameter(sharedData);
}

public sealed class MapPulseParameter : EffectShapeParameterBase
{
    public MapPulseParameter() : this(null) { }
    public MapPulseParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "場所", Name = "場所（1行に「X%,Y%:ラベル」）")]
    [TextEditor(AcceptsReturn = true)]
    public string Points { get => points; set => Set(ref points, value); }
    string points = "62,42:東京\n45,58:大阪\n22,70:福岡";

    [Display(GroupName = "場所", Name = "1つずつ出る間隔")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation Interval { get; } = new Animation(0.6, 0, 60);

    [Display(GroupName = "場所", Name = "場所どうしを線で結ぶ")]
    [ToggleSlider]
    public bool Connect { get => connect; set => Set(ref connect, value); }
    bool connect = false;

    [Display(GroupName = "見た目", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x3D, 0x57);

    [Display(GroupName = "見た目", Name = "点の大きさ")]
    [AnimationSlider("F0", "px", 4, 60)]
    public Animation DotSize { get; } = new Animation(16, 1, 500);

    [Display(GroupName = "見た目", Name = "波紋の大きさ")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation RippleSize { get; } = new Animation(120, 1, 3000);

    [Display(GroupName = "見た目", Name = "波紋の速さ")]
    [AnimationSlider("F1", "回/秒", 0.1, 3)]
    public Animation RippleSpeed { get; } = new Animation(0.8, 0, 30);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "ラベルの大きさ")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation FontSize { get; } = new Animation(40, 4, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Interval, DotSize, RippleSize, RippleSpeed, FontSize];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.2f, 0.25f);
        if (k <= 0.001f) return;
        var list = new List<(Vector2 p, string label)>();
        foreach (var raw in TextKit.Lines(Points).Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            string label = "";
            int colon = line.IndexOfAny(new[] { ':', '：' });
            if (colon >= 0) { label = line[(colon + 1)..].Trim(); line = line[..colon]; }
            var xy = line.Split(new[] { ',', '、' });
            if (xy.Length < 2) continue;
            if (!float.TryParse(xy[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var px)) continue;
            if (!float.TryParse(xy[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var py)) continue;
            list.Add((new Vector2(px / 100f * w, py / 100f * h), label));
        }
        float iv = c.F(Interval), dot = c.F(DotSize), rip = c.F(RippleSize), rs = c.F(RippleSpeed), fs = c.F(FontSize);
        using var brush = dc.CreateSolidColorBrush(default);
        if (Connect && list.Count > 1)
        {
            for (int i = 1; i < list.Count; i++)
            {
                float lp = Math.Clamp((t - 0.2f - i * iv) / MathF.Max(0.1f, iv * 0.8f), 0f, 1f);
                if (lp <= 0f) continue;
                var a = list[i - 1].p; var b = list[i].p;
                var mid = (a + b) / 2f + new Vector2(0f, -Vector2.Distance(a, b) * 0.25f);
                var arc = BubbleKit.PartByLength(BubbleKit.Quad(a, mid, b, 40), lp);
                brush.Color = Gfx.C4(Color, 0.8f * k);
                for (int j = 0; j + 1 < arc.Count; j += 2) dc.DrawLine(arc[j], arc[j + 1], brush, MathF.Max(2f, dot * 0.25f));
            }
        }
        for (int i = 0; i < list.Count; i++)
        {
            float at = 0.2f + i * iv;
            float age = t - at;
            if (age < 0f) continue;
            var (p, label) = list[i];
            // 波紋: 3重に、外へ広がって消える
            for (int r = 0; r < 3; r++)
            {
                float f = DrawKit.Frac(age * rs + r / 3f);
                if (age * rs + r / 3f < 1f / 3f * r) continue;
                brush.Color = Gfx.C4(Color, (1f - f) * 0.7f * k);
                dc.DrawEllipse(new Ellipse(p, dot + rip * f, (dot + rip * f) * 0.85f), brush, MathF.Max(1.5f, dot * 0.22f * (1f - f)));
            }
            float pop = DrawKit.EaseOutBack(Math.Clamp(age / 0.3f, 0f, 1f));
            DrawKit.Glow(dc, p, dot * 3f, Gfx.C4(Color, 0.5f * k));
            brush.Color = Gfx.C4(1f, 1f, 1f, k);
            dc.FillEllipse(new Ellipse(p, dot * 0.75f * pop, dot * 0.75f * pop), brush);
            brush.Color = Gfx.C4(Color, k);
            dc.FillEllipse(new Ellipse(p, dot * 0.5f * pop, dot * 0.5f * pop), brush);
            if (label.Length > 0)
            {
                float lp = Math.Clamp((age - 0.15f) / 0.3f, 0f, 1f);
                var s = TextKit.Get(label, fs, true, Font);
                var lc = p + new Vector2(0f, -dot - fs * 0.9f - (1f - DrawKit.EaseOut(lp)) * fs * 0.3f);
                var box = ShapeGeom.RoundRect(lc.X, lc.Y, s.Width + fs * 0.7f, s.Height + fs * 0.25f, fs * 0.25f);
                brush.Color = Gfx.C4(0.08f, 0.09f, 0.12f, 0.85f * k * lp);
                Gfx.FillPoly(dc, brush, box);
                Gfx.FillPoly(dc, brush, new List<Vector2> { lc + new Vector2(-fs * 0.2f, s.Height / 2f + fs * 0.1f), lc + new Vector2(fs * 0.2f, s.Height / 2f + fs * 0.1f), lc + new Vector2(0f, s.Height / 2f + fs * 0.4f) });
                brush.Color = Gfx.C4(Color, k * lp);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(box), MathF.Max(1.5f, fs * 0.05f));
                TextKit.DrawCentered(dc, brush, s, lc, Gfx.C4(1f, 1f, 1f, k * lp));
            }
        }
    }
}
