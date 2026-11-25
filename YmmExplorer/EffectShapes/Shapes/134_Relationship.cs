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

public enum RelationLayout
{
    [Display(Name = "円形に並べる")] Circle,
    [Display(Name = "横一列")] Row,
    [Display(Name = "まん中に1人＋まわり")] Hub,
}

// =====================================================================
//  相関図・関係線
//   人物(丸)と、それを結ぶ線・矢印・関係のラベル。
//   関係は「1-2:友達」(線)、「1>2:片思い」(1から2への矢印)、「1<>2:ライバル」(両向き)のように書く
// =====================================================================
public sealed class RelationshipPlugin : IShapePlugin
{
    public string Name => "演出：相関図・関係線";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RelationshipParameter(sharedData);
}

public sealed class RelationshipParameter : EffectShapeParameterBase
{
    public RelationshipParameter() : this(null) { }
    public RelationshipParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "データ", Name = "人物（カンマ区切り）")]
    [TextEditor(AcceptsReturn = false)]
    public string People { get => people; set => Set(ref people, value); }
    string people = "主人公,ヒロイン,ライバル,先生";

    [Display(GroupName = "データ", Name = "関係（1行に1つ）", Description = "例: 1-2:友達 / 1>3:あこがれ / 2<>3:ライバル(番号は人物の順番)")]
    [TextEditor(AcceptsReturn = true)]
    public string Links { get => links; set => Set(ref links, value); }
    string links = "1<>3:ライバル\n1>2:片思い\n4-1:師弟\n4-3:担任";

    [Display(GroupName = "形", Name = "並べ方")]
    [EnumComboBox]
    public RelationLayout Layout { get => layout; set => Set(ref layout, value); }
    RelationLayout layout = RelationLayout.Circle;

    [Display(GroupName = "形", Name = "人物の丸の大きさ")]
    [AnimationSlider("F0", "px", 30, 250)]
    public Animation NodeRadius { get; } = new Animation(95, 5, 2000);

    [Display(GroupName = "形", Name = "広がり")]
    [AnimationSlider("F0", "px", 100, 900)]
    public Animation Spread { get; } = new Animation(380, 1, 5000);

    [Display(GroupName = "形", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 1, 16)]
    public Animation LineWidth { get; } = new Animation(5, 0.5, 200);

    [Display(GroupName = "色", Name = "色の組み合わせ（人物）")]
    [EnumComboBox]
    public ChartPalette Palette { get => palette; set => Set(ref palette, value); }
    ChartPalette palette = ChartPalette.Vivid;

    [Display(GroupName = "色", Name = "線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(0xEE, 0xEE, 0xEE);

    [Display(GroupName = "色", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "色", Name = "関係ラベルの地の色")]
    [ColorPicker]
    public WpfColor LabelBg { get => labelBg; set => Set(ref labelBg, value); }
    WpfColor labelBg = WpfColor.FromRgb(0x22, 0x26, 0x30);

    [Display(GroupName = "文字", Name = "名前の大きさ")]
    [AnimationSlider("F0", "px", 10, 100)]
    public Animation FontSize { get; } = new Animation(36, 4, 1000);

    [Display(GroupName = "文字", Name = "関係ラベルの大きさ")]
    [AnimationSlider("F0", "px", 10, 100)]
    public Animation LabelSize { get; } = new Animation(28, 4, 1000);

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "動き", Name = "1人ずつ出る間隔")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation Stagger { get; } = new Animation(0.2, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [NodeRadius, Spread, LineWidth, FontSize, LabelSize, PosX, PosY, Stagger];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var names = TextKit.Split(People).Where(s => s.Length > 0).ToList();
        int n = names.Count;
        if (n == 0) return;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(NodeRadius), spread = c.F(Spread), lw = c.F(LineWidth), stag = c.F(Stagger);
        float fs = c.F(FontSize), ls = c.F(LabelSize);

        // 人物の位置
        var pos = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            switch (Layout)
            {
                case RelationLayout.Row:
                    pos[i] = ctr + new Vector2(n == 1 ? 0f : (i / (float)(n - 1) - 0.5f) * spread * 2.2f, 0f);
                    break;
                case RelationLayout.Hub:
                    if (i == 0) pos[i] = ctr;
                    else
                    {
                        float a = -MathF.PI / 2f + (i - 1) / (float)Math.Max(1, n - 1) * MathF.PI * 2f;
                        pos[i] = ctr + new Vector2(MathF.Cos(a) * spread * 1.25f, MathF.Sin(a) * spread);
                    }
                    break;
                default:
                {
                    float a = -MathF.PI / 2f + i / (float)n * MathF.PI * 2f;
                    pos[i] = ctr + new Vector2(MathF.Cos(a) * spread * 1.3f, MathF.Sin(a) * spread);
                    break;
                }
            }
        }
        float Appear(int i) => DrawKit.EaseOutBack(Math.Clamp((t - i * stag) / 0.35f, 0f, 1f));

        using var brush = dc.CreateSolidColorBrush(default);

        // 関係の線(人物より先に描く)
        int li = 0;
        foreach (var raw in TextKit.Lines(Links).Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            string label = "";
            int colon = line.IndexOfAny(new[] { ':', '：' });
            if (colon >= 0) { label = line[(colon + 1)..].Trim(); line = line[..colon].Trim(); }
            int kind; string[] ab;
            if (line.Contains("<>")) { kind = 2; ab = line.Split("<>"); }
            else if (line.Contains('>')) { kind = 1; ab = line.Split('>'); }
            else if (line.Contains('<')) { kind = 3; ab = line.Split('<'); }
            else { kind = 0; ab = line.Split('-', 'ー', '－'); }
            if (ab.Length < 2 || !int.TryParse(ab[0].Trim(), out var ia) || !int.TryParse(ab[1].Trim(), out var ib)) continue;
            ia--; ib--;
            if (ia < 0 || ib < 0 || ia >= n || ib >= n || ia == ib) continue;
            float appear = Math.Clamp((t - (n * stag + li * stag * 0.7f)) / 0.4f, 0f, 1f);
            li++;
            if (appear <= 0f) continue;
            var pa = pos[ia]; var pb = pos[ib];
            var d = pb - pa;
            float len = d.Length();
            if (len < 1f) continue;
            var dir = d / len;
            var nrm = new Vector2(-dir.Y, dir.X);
            // 同じ2人に線が2本ある時、重ならないよう少しずらす
            var shift = nrm * (kind == 3 ? -lw * 2f : 0f);
            var a0 = pa + dir * (R + lw * 2f) + shift;
            var b0 = pb - dir * (R + lw * 2f) + shift;
            var b1 = Vector2.Lerp(a0, b0, DrawKit.EaseOut(appear));
            brush.Color = Gfx.C4(LineColor, 1f);
            dc.DrawLine(a0, b1, brush, lw);
            float head = lw * 4.5f;
            if (appear >= 1f)
            {
                if (kind == 1 || kind == 2) Gfx.FillPoly(dc, brush, BubbleKit.ArrowHead(b0, dir, head));
                if (kind == 2 || kind == 3) Gfx.FillPoly(dc, brush, BubbleKit.ArrowHead(a0, -dir, head));
            }
            if (label.Length > 0 && appear > 0.5f)
            {
                float la = (appear - 0.5f) * 2f;
                var s = TextKit.Get(label, ls, true);
                var mid = (a0 + b0) / 2f;
                var box = ShapeGeom.RoundRect(mid.X, mid.Y, s.Width + ls * 0.8f, s.Height + ls * 0.3f, (s.Height + ls * 0.3f) / 2f);
                brush.Color = Gfx.C4(LabelBg, la);
                Gfx.FillPoly(dc, brush, box);
                brush.Color = Gfx.C4(LineColor, la);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(box), MathF.Max(1f, lw * 0.5f));
                TextKit.DrawCentered(dc, brush, s, mid, Gfx.C4(TextColor, la));
            }
        }

        // 人物
        for (int i = 0; i < n; i++)
        {
            float k = Appear(i);
            if (k <= 0.01f) continue;
            float r = R * k;
            var col = BubbleKit.Palette((int)Palette, i);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f);
            dc.FillEllipse(new Ellipse(pos[i] + new Vector2(5f, 7f), r, r), brush);
            using (var g = Gfx.Radial(dc, pos[i] - new Vector2(r * 0.3f, r * 0.35f), r * 1.5f, (0f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.35f), 1f)), (1f, Gfx.C4(col, 1f))))
                dc.FillEllipse(new Ellipse(pos[i], r, r), g);
            brush.Color = Gfx.C4(255f / 255f, 1f, 1f, 0.9f);
            dc.DrawEllipse(new Ellipse(pos[i], r, r), brush, MathF.Max(1.5f, lw * 0.7f));
            var s = TextKit.Get(names[i], fs, true, null, R * 1.7f, 1);
            TextKit.DrawCentered(dc, brush, s, pos[i], Gfx.C4(TextColor, 1f), Gfx.C4(0f, 0f, 0f, 0.6f), fs * 0.06f, k);
        }
    }
}
