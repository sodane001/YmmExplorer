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
//  回路パターン
//   基板の配線・ランド・チップ。配線に沿って光のパルスが走る
// =====================================================================
public sealed class CircuitPlugin : IShapePlugin
{
    public string Name => "演出：回路パターン";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CircuitParameter(sharedData);
}

public sealed class CircuitParameter : EffectShapeParameterBase
{
    public CircuitParameter() : this(null) { }
    public CircuitParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "配線の色")]
    [ColorPicker]
    public WpfColor TraceColor { get => trace; set => Set(ref trace, value); }
    WpfColor trace = WpfColor.FromRgb(0x2F, 0xB8, 0x8A);

    [Display(GroupName = "色", Name = "光るパルスの色")]
    [ColorPicker]
    public WpfColor PulseColor { get => pulse; set => Set(ref pulse, value); }
    WpfColor pulse = WpfColor.FromRgb(0x9B, 0xFF, 0xD8);

    [Display(GroupName = "色", Name = "基板の背景色")]
    [ColorPicker]
    public WpfColor BgColor { get => bg; set => Set(ref bg, value); }
    WpfColor bg = WpfColor.FromRgb(0x04, 0x14, 0x0E);

    [Display(GroupName = "色", Name = "背景の不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BgOpacity { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "配線", Name = "配線の数")]
    [AnimationSlider("F0", "本", 5, 200)]
    public Animation Count { get; } = new Animation(50, 1, 1000);

    [Display(GroupName = "配線", Name = "マスの大きさ")]
    [AnimationSlider("F0", "px", 10, 80)]
    public Animation CellSize { get; } = new Animation(28, 6, 300);

    [Display(GroupName = "配線", Name = "配線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 8)]
    public Animation Thickness { get; } = new Animation(2, 0.3, 50);

    [Display(GroupName = "パルス", Name = "パルスの速さ")]
    [AnimationSlider("F0", "マス/秒", 1, 40)]
    public Animation PulseSpeed { get; } = new Animation(9, 0.1, 200);

    [Display(GroupName = "パルス", Name = "光の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glow { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "部品", Name = "チップを表示する")]
    [ToggleSlider]
    public bool ShowChips { get => chips; set => Set(ref chips, value); }
    bool chips = true;

    [Display(GroupName = "部品", Name = "チップの数")]
    [AnimationSlider("F0", "個", 0, 20)]
    public Animation ChipCount { get; } = new Animation(6, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [BgOpacity, Count, CellSize, Thickness, PulseSpeed, Glow, ChipCount];

    static readonly Vector2[] Dirs =
    {
        new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0), new(-1, -1), new(0, -1), new(1, -1),
    };

    static Vector2 PathPos(List<Vector2> pts, List<float> cum, float d)
    {
        d = Math.Clamp(d, 0f, cum[^1]);
        for (int i = 0; i < pts.Count - 1; i++)
            if (d <= cum[i + 1])
            {
                float seg = cum[i + 1] - cum[i];
                return seg < 0.001f ? pts[i] : Vector2.Lerp(pts[i], pts[i + 1], (d - cum[i]) / seg);
            }
        return pts[^1];
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float cell = MathF.Max(6f, c.F(CellSize)), th = c.F(Thickness);
        int cols = (int)(w / cell), rows = (int)(h / cell);
        int n = (int)c.F(Count);
        float glow = c.F(Glow) / 100f, speed = c.F(PulseSpeed) * cell;
        float bgA = c.F(BgOpacity) / 100f;

        using var brush = dc.CreateSolidColorBrush(default);

        if (bgA > 0f)
        {
            brush.Color = Gfx.C4(BgColor, bgA);
            Gfx.Rect(dc, brush, 0, 0, w, h);
        }

        var rng = new Random(Seed);

        // ---- チップ（四角い部品とピン）----
        if (ShowChips)
        {
            int nc = (int)c.F(ChipCount);
            for (int i = 0; i < nc; i++)
            {
                int cw = 3 + rng.Next(0, 4), ch = 3 + rng.Next(0, 3);
                int gx = rng.Next(1, Math.Max(2, cols - cw - 1)), gy = rng.Next(1, Math.Max(2, rows - ch - 1));
                float x0 = gx * cell, y0 = gy * cell, x1 = (gx + cw) * cell, y1 = (gy + ch) * cell;
                brush.Color = Gfx.C4(TraceColor, 0.10f);
                Gfx.Rect(dc, brush, x0, y0, x1 - x0, y1 - y0);
                brush.Color = Gfx.C4(TraceColor, 0.75f);
                dc.DrawRectangle(new RawRectF(x0, y0, x1, y1), brush, th);
                for (int k = 1; k < cw; k++)
                {
                    dc.DrawLine(new Vector2(x0 + k * cell, y0 - cell * 0.3f), new Vector2(x0 + k * cell, y0), brush, th);
                    dc.DrawLine(new Vector2(x0 + k * cell, y1), new Vector2(x0 + k * cell, y1 + cell * 0.3f), brush, th);
                }
                for (int k = 1; k < ch; k++)
                {
                    dc.DrawLine(new Vector2(x0 - cell * 0.3f, y0 + k * cell), new Vector2(x0, y0 + k * cell), brush, th);
                    dc.DrawLine(new Vector2(x1, y0 + k * cell), new Vector2(x1 + cell * 0.3f, y0 + k * cell), brush, th);
                }
                float pr = 0.5f + 0.5f * MathF.Sin(t * 2f + i);
                brush.Color = Gfx.C4(PulseColor, (0.3f + 0.5f * pr) * glow);
                dc.FillEllipse(new Ellipse(new Vector2((x0 + x1) / 2f, (y0 + y1) / 2f), cell * 0.18f, cell * 0.18f), brush);
            }
        }

        // ---- 配線とパルス ----
        for (int i = 0; i < n; i++)
        {
            var rt = new Random(Seed * 977 + i * 131);
            int gx = rt.Next(1, Math.Max(2, cols)), gy = rt.Next(1, Math.Max(2, rows));
            int d = rt.Next(0, 8);
            var pts = new List<Vector2> { new(gx * cell, gy * cell) };
            int steps = 3 + rt.Next(0, 6);
            for (int s = 0; s < steps; s++)
            {
                int len = 2 + rt.Next(0, 6);
                var dv = Dirs[d];
                int nx = gx + (int)dv.X * len, ny = gy + (int)dv.Y * len;
                if (nx < 0 || ny < 0 || nx > cols || ny > rows) break;
                gx = nx; gy = ny;
                pts.Add(new Vector2(gx * cell, gy * cell));
                d = (d + (rt.NextF() < 0.5f ? 1 : 7)) % 8;                  // 45°だけ曲がる
            }
            if (pts.Count < 2) continue;

            var cum = new List<float> { 0f };
            for (int k = 1; k < pts.Count; k++) cum.Add(cum[k - 1] + Vector2.Distance(pts[k - 1], pts[k]));
            float total = cum[^1];

            // 配線（にじみ→本線）
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            brush.Color = Gfx.C4(TraceColor, 0.45f);
            Gfx.Polyline(dc, brush, pts, th);
            // ランド（始点と終点）
            brush.Color = Gfx.C4(TraceColor, 0.85f);
            dc.DrawEllipse(new Ellipse(pts[0], cell * 0.16f, cell * 0.16f), brush, th);
            dc.FillEllipse(new Ellipse(pts[^1], cell * 0.10f, cell * 0.10f), brush);

            // パルス（尾を引いて走る）
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            int np = 1 + rt.Next(0, 2);
            for (int p = 0; p < np; p++)
            {
                float ph = rt.NextF(), sp = 0.7f + 0.6f * rt.NextF();
                float tail = cell * 7f;
                float cycle = total + tail + cell * 6f;
                float head = Gfx.Mod(t * speed * sp + ph * cycle, cycle);
                if (head - tail > total) continue;
                const int M = 10;
                Vector2 prev = PathPos(pts, cum, head);
                for (int j = 1; j <= M; j++)
                {
                    float dd = head - tail * j / M;
                    if (dd < 0f) break;
                    var cur = PathPos(pts, cum, dd);
                    float al = (1f - j / (float)M) * glow;
                    brush.Color = Gfx.C4(PulseColor, 0.18f * al);
                    dc.DrawLine(prev, cur, brush, th * 4f);
                    brush.Color = Gfx.C4(PulseColor, 0.9f * al);
                    dc.DrawLine(prev, cur, brush, th * 1.4f);
                    prev = cur;
                }
                if (head <= total)
                {
                    var hp = PathPos(pts, cum, head);
                    brush.Color = Gfx.C4(PulseColor, 0.35f * glow);
                    dc.FillEllipse(new Ellipse(hp, th * 4f, th * 4f), brush);
                    brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.95f * glow);
                    dc.FillEllipse(new Ellipse(hp, th * 1.5f, th * 1.5f), brush);
                }
            }
        }
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
    }
}
