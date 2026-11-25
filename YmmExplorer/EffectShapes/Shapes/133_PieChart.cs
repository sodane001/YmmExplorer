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

public enum ChartPalette
{
    [Display(Name = "あざやか")] Vivid,
    [Display(Name = "パステル")] Pastel,
    [Display(Name = "青系")] Cool,
    [Display(Name = "赤・橙系")] Warm,
    [Display(Name = "モノクロ")] Mono,
}

public enum PieLabelMode
{
    [Display(Name = "出さない")] None,
    [Display(Name = "名前")] Name,
    [Display(Name = "割合（%）")] Percent,
    [Display(Name = "名前と割合")] Both,
}

// =====================================================================
//  円グラフ・ドーナツグラフ
//   値は「40,25,20,15」のようにカンマで並べる。名前も同じ順に並べる
// =====================================================================
public sealed class PieChartPlugin : IShapePlugin
{
    public string Name => "演出：円グラフ・ドーナツグラフ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PieChartParameter(sharedData);
}

public sealed class PieChartParameter : EffectShapeParameterBase
{
    public PieChartParameter() : this(null) { }
    public PieChartParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "データ", Name = "値（カンマ区切り）")]
    [TextEditor(AcceptsReturn = false)]
    public string Values { get => values; set => Set(ref values, value); }
    string values = "40,25,20,15";

    [Display(GroupName = "データ", Name = "名前（カンマ区切り）")]
    [TextEditor(AcceptsReturn = false)]
    public string Labels { get => labels; set => Set(ref labels, value); }
    string labels = "A案,B案,C案,その他";

    [Display(GroupName = "データ", Name = "ラベル")]
    [EnumComboBox]
    public PieLabelMode LabelMode { get => labelMode; set => Set(ref labelMode, value); }
    PieLabelMode labelMode = PieLabelMode.Both;

    [Display(GroupName = "形", Name = "ドーナツにする")]
    [ToggleSlider]
    public bool Donut { get => donut; set => Set(ref donut, value); }
    bool donut = true;

    [Display(GroupName = "形", Name = "穴の大きさ")]
    [AnimationSlider("F0", "%", 10, 90)]
    public Animation Hole { get; } = new Animation(55, 0, 95);

    [Display(GroupName = "形", Name = "まん中の文字（ドーナツ）")]
    [TextEditor(AcceptsReturn = true)]
    public string CenterText { get => centerText; set => Set(ref centerText, value); }
    string centerText = "アンケート";

    [Display(GroupName = "形", Name = "すき間")]
    [AnimationSlider("F1", "°", 0, 10)]
    public Animation Gap { get; } = new Animation(1.5, 0, 45);

    [Display(GroupName = "形", Name = "いちばん大きい所を飛び出させる")]
    [ToggleSlider]
    public bool Explode { get => explode; set => Set(ref explode, value); }
    bool explode = false;

    [Display(GroupName = "色", Name = "色の組み合わせ")]
    [EnumComboBox]
    public ChartPalette Palette { get => palette; set => Set(ref palette, value); }
    ChartPalette palette = ChartPalette.Vivid;

    [Display(GroupName = "色", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "色", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation FontSize { get; } = new Animation(38, 4, 1000);

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "半径")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Radius { get; } = new Animation(300, 1, 5000);

    [Display(GroupName = "位置と大きさ", Name = "はじまりの角度", Description = "0°=真上から時計回り")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation StartAngle { get; } = new Animation(0, -720, 720);

    [Display(GroupName = "動き", Name = "グラフが伸びる時間")]
    [AnimationSlider("F2", "秒", 0, 4)]
    public Animation GrowTime { get; } = new Animation(1.2, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Hole, Gap, FontSize, PosX, PosY, Radius, StartAngle, GrowTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var nums = TextKit.Split(Values).Select(s => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && v > 0 ? v : 0f).ToList();
        var names = TextKit.Split(Labels);
        float sum = nums.Sum();
        if (sum <= 0f) return;
        float grow = c.F(GrowTime);
        float prog = grow <= 0.001f ? 1f : 1f - MathF.Pow(1f - Math.Clamp(t / grow, 0f, 1f), 3f);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), hole = Donut ? Math.Clamp(c.F(Hole) / 100f, 0f, 0.95f) : 0f;
        float gap = Gfx.Rad(c.F(Gap)), fs = c.F(FontSize);
        float start = Gfx.Rad(c.F(StartAngle)) - MathF.PI / 2f;
        int biggest = nums.IndexOf(nums.Max());

        using var brush = dc.CreateSolidColorBrush(default);
        float acc = 0f;
        float sweepAll = MathF.PI * 2f * prog;
        var labelsToDraw = new List<(Vector2 pos, string text, float a)>();
        for (int i = 0; i < nums.Count; i++)
        {
            if (nums[i] <= 0f) continue;
            float a0 = start + acc / sum * MathF.PI * 2f;
            float a1 = start + (acc + nums[i]) / sum * MathF.PI * 2f;
            acc += nums[i];
            // 伸びている途中は、そこまでで切る
            float limit = start + sweepAll;
            if (a0 >= limit) break;
            float e1 = MathF.Min(a1, limit);
            float g = MathF.Min(gap / 2f, (e1 - a0) / 3f);
            float s0 = a0 + g, s1 = e1 - g;
            if (s1 <= s0) continue;
            var off = Explode && i == biggest ? new Vector2(MathF.Cos((a0 + a1) / 2f), MathF.Sin((a0 + a1) / 2f)) * R * 0.07f * prog : Vector2.Zero;
            var c0 = ctr + off;
            int n = Math.Max(4, (int)((s1 - s0) / (MathF.PI * 2f) * 160f));
            var outer = Gfx.ArcPts(c0, R, s0, s1, n);
            var poly = new List<Vector2>(outer);
            if (hole > 0.001f) poly.AddRange(Enumerable.Reverse(Gfx.ArcPts(c0, R * hole, s0, s1, n)));
            else poly.Add(c0);
            var col = BubbleKit.Palette((int)Palette, i);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f);
            Gfx.FillPoly(dc, brush, poly.Select(p => p + new Vector2(5f, 7f)).ToList());
            using (var grad = Gfx.Radial(dc, c0, R, (hole, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(0, 0, 0), 0.12f), 1f)), (1f, Gfx.C4(col, 1f))))
                Gfx.FillPoly(dc, grad, poly);

            if (LabelMode != PieLabelMode.None && e1 >= a1 - 1e-4f)
            {
                float mid = (a0 + a1) / 2f;
                float rr = hole > 0.001f ? R * (1f + hole) / 2f : R * 0.62f;
                string pct = (nums[i] / sum * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";
                string nm = i < names.Count ? names[i] : "";
                string label = LabelMode switch
                {
                    PieLabelMode.Name => nm,
                    PieLabelMode.Percent => pct,
                    _ => nm.Length > 0 ? nm + "\n" + pct : pct,
                };
                float la = Math.Clamp((t - (grow * (acc / sum))) / 0.3f + 1f, 0f, 1f);
                labelsToDraw.Add((c0 + new Vector2(MathF.Cos(mid), MathF.Sin(mid)) * rr, label, la));
            }
        }
        foreach (var (pos, text, a) in labelsToDraw)
        {
            if (a <= 0.01f || text.Length == 0) continue;
            var s = TextKit.Get(text, fs, true, null, 0f, 1);
            TextKit.DrawCentered(dc, brush, s, pos, Gfx.C4(TextColor, a), Gfx.C4(0f, 0f, 0f, 0.55f * a), fs * 0.08f);
        }
        if (hole > 0.001f && !string.IsNullOrWhiteSpace(CenterText))
        {
            var s = TextKit.Get(TextKit.Lines(CenterText), fs * 1.15f, true, null, R * hole * 1.6f, 1);
            TextKit.DrawCentered(dc, brush, s, ctr, Gfx.C4(TextColor, prog), Gfx.C4(0f, 0f, 0f, 0.5f * prog), fs * 0.08f);
        }
    }
}
