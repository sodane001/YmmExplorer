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
//  棒グラフのレース
//   時間とともに数値が変わり、順位が入れ替わりながら棒が伸びる。
//   1行に「名前:値1,値2,値3…」。区切り(年など)は「区切りの名前」に同じ数だけ書く
// =====================================================================
public sealed class BarRacePlugin : IShapePlugin
{
    public string Name => "演出：棒グラフのレース";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BarRaceParameter(sharedData);
}

public sealed class BarRaceParameter : EffectShapeParameterBase
{
    public BarRaceParameter() : this(null) { }
    public BarRaceParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "データ", Name = "データ（1行に「名前:値1,値2…」）")]
    [TextEditor(AcceptsReturn = true)]
    public string Data { get => data; set => Set(ref data, value); }
    string data = "A社:10,25,40,52,60\nB社:30,34,38,45,70\nC社:20,40,42,48,50\nD社:5,12,30,58,66\nE社:15,18,22,26,35";

    [Display(GroupName = "データ", Name = "区切りの名前（カンマ区切り）")]
    [TextEditor(AcceptsReturn = false)]
    public string Periods { get => periods; set => Set(ref periods, value); }
    string periods = "2020,2021,2022,2023,2024";

    [Display(GroupName = "データ", Name = "見出し")]
    [TextEditor(AcceptsReturn = false)]
    public string Title { get => title; set => Set(ref title, value); }
    string title = "売上の推移";

    [Display(GroupName = "データ", Name = "値につける単位")]
    [TextEditor(AcceptsReturn = false)]
    public string Unit { get => unit; set => Set(ref unit, value); }
    string unit = "億円";

    [Display(GroupName = "データ", Name = "表示する数")]
    [TextBoxSlider("F0", "本", 1, 15)]
    [Range(1, 50)]
    public int TopN { get => topN; set => Set(ref topN, value); }
    int topN = 5;

    [Display(GroupName = "動き", Name = "はじめから終わりまでの時間")]
    [AnimationSlider("F1", "秒", 1, 30)]
    public Animation Duration { get; } = new Animation(8, 0.1, 3600);

    [Display(GroupName = "見た目", Name = "色の組み合わせ")]
    [EnumComboBox]
    public ChartPalette Palette { get => palette; set => Set(ref palette, value); }
    ChartPalette palette = ChartPalette.Vivid;

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(52, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "グラフの幅")]
    [AnimationSlider("F0", "px", 300, 1800)]
    public Animation ChartWidth { get; } = new Animation(1400, 50, 8000);

    [Display(GroupName = "位置と大きさ", Name = "グラフの高さ")]
    [AnimationSlider("F0", "px", 200, 1000)]
    public Animation ChartHeight { get; } = new Animation(700, 50, 8000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Duration, PosX, PosY, ChartWidth, ChartHeight];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var series = new List<(string name, float[] v, int idx)>();
        int idx0 = 0;
        foreach (var raw in TextKit.Lines(Data).Split('\n'))
        {
            var line = raw.Trim();
            int colon = line.IndexOfAny(new[] { ':', '：' });
            if (colon <= 0) continue;
            var vals = line[(colon + 1)..].Split(new[] { ',', '、' }).Select(s => float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var fv) ? fv : 0f).ToArray();
            if (vals.Length == 0) continue;
            series.Add((line[..colon].Trim(), vals, idx0++));
        }
        if (series.Count == 0) return;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        int steps = series.Max(s => s.v.Length);
        float dur = MathF.Max(0.1f, c.F(Duration));
        float pos = Math.Clamp(t / dur, 0f, 1f) * MathF.Max(0, steps - 1);
        int i0 = (int)MathF.Floor(pos), i1 = Math.Min(steps - 1, i0 + 1);
        float f = pos - i0;
        float fe = f * f * (3f - 2f * f);
        float Val(float[] v, int i) => v.Length == 0 ? 0f : v[Math.Min(i, v.Length - 1)];
        var cur = series.Select(s => Val(s.v, i0) + (Val(s.v, i1) - Val(s.v, i0)) * fe).ToArray();
        float maxV = MathF.Max(1e-6f, cur.Max());
        // 順位: なめらかに入れ替わるよう、ほかとの差を S字で数える
        float scale = maxV * 0.03f;
        var rankPos = new float[series.Count];
        for (int a = 0; a < series.Count; a++)
        {
            float r = 0f;
            for (int b = 0; b < series.Count; b++)
            {
                if (a == b) continue;
                float d = (cur[b] - cur[a]) / scale;
                r += 1f / (1f + MathF.Exp(-d * 2f));
            }
            rankPos[a] = r;
        }

        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float cw = c.F(ChartWidth), ch = c.F(ChartHeight);
        int n = Math.Max(1, Math.Min(TopN, series.Count));
        float rowH = ch / n, barH = rowH * 0.72f;
        float labelW = cw * 0.16f;
        float x0 = ctr.X - cw / 2f + labelW, x1 = ctr.X + cw / 2f - cw * 0.12f;
        float y0 = ctr.Y - ch / 2f;
        using var brush = dc.CreateSolidColorBrush(default);
        string ttl = TextKit.Lines(Title).Trim();
        if (ttl.Length > 0)
        {
            var ts = TextKit.Get(ttl, rowH * 0.5f, true, Font);
            TextKit.Draw(dc, brush, ts, new Vector2(ctr.X - cw / 2f, y0 - ts.Height - rowH * 0.15f), Gfx.C4(TextColor, k), Gfx.C4(0f, 0f, 0f, 0.6f * k), rowH * 0.02f);
        }
        // 目盛り線
        brush.Color = Gfx.C4(TextColor, 0.12f * k);
        for (int g = 1; g <= 4; g++)
        {
            float gx = x0 + (x1 - x0) * g / 4f;
            dc.DrawLine(new Vector2(gx, y0), new Vector2(gx, y0 + ch), brush, 1.5f);
        }
        string unitS = TextKit.Lines(Unit).Trim();
        for (int a = 0; a < series.Count; a++)
        {
            float rp = rankPos[a];
            if (rp > n - 0.05f) continue;
            float vis = Math.Clamp(n - rp, 0f, 1f);       // 枠外へ出ていく時は、薄くなる
            float y = y0 + rp * rowH + rowH / 2f;
            float len = (x1 - x0) * cur[a] / maxV;
            var col = BubbleKit.Palette((int)Palette, series[a].idx);
            float alpha = k * vis;
            var bar = ShapeGeom.RoundRect(x0 + len / 2f, y, MathF.Max(barH * 0.3f, len), barH, barH * 0.18f);
            using (var gb = Gfx.Linear(dc, new Vector2(x0, y), new Vector2(x0 + MathF.Max(1f, len), y), (0f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(0, 0, 0), 0.15f), alpha)), (1f, Gfx.C4(col, alpha))))
                Gfx.FillPoly(dc, gb, bar);
            var ns = TextKit.Get(series[a].name, barH * 0.5f, true, Font, labelW * 0.95f, 2);
            TextKit.Draw(dc, brush, ns, new Vector2(x0 - ns.Width - barH * 0.25f, y - ns.Height / 2f), Gfx.C4(TextColor, alpha), Gfx.C4(0f, 0f, 0f, 0.5f * alpha), barH * 0.02f);
            string vt = MathF.Round(cur[a]).ToString("#,0", CultureInfo.InvariantCulture) + unitS;
            var vs = TextKit.Get(vt, barH * 0.42f, true, Font);
            TextKit.Draw(dc, brush, vs, new Vector2(x0 + len + barH * 0.2f, y - vs.Height / 2f), Gfx.C4(TextColor, alpha), Gfx.C4(0f, 0f, 0f, 0.5f * alpha), barH * 0.02f);
        }
        // いまの区切り(年など)を、右下に大きく
        var names = TextKit.Split(Periods);
        int pi = (int)MathF.Round(pos);
        if (pi >= 0 && pi < names.Count && names[pi].Length > 0)
        {
            var ps = TextKit.Get(names[pi], rowH * 1.1f, true, Font);
            TextKit.Draw(dc, brush, ps, new Vector2(ctr.X + cw / 2f - ps.Width, y0 + ch - ps.Height), Gfx.C4(TextColor, 0.35f * k));
        }
    }
}
