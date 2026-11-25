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
//  年表・タイムライン
//   横線に年号の点が順に打たれていく。出来事の説明は YMM4 のテキストを重ねてください
// =====================================================================
public sealed class TimelinePlugin : IShapePlugin
{
    public string Name => "演出：年表・タイムライン";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TimelineParameter(sharedData);
}

public sealed class TimelineParameter : EffectShapeParameterBase
{
    public TimelineParameter() : this(null) { }
    public TimelineParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "年号", Name = "点の数")]
    [AnimationSlider("F0", "", 2, 12)]
    public Animation Points { get; } = new Animation(5, 2, 40);

    [Display(GroupName = "年号", Name = "最初の年")]
    [AnimationSlider("F0", "", 1000, 2100)]
    public Animation StartYear { get; } = new Animation(1990, -9999, 99999);

    [Display(GroupName = "年号", Name = "年の間隔")]
    [AnimationSlider("F0", "年", 1, 100)]
    public Animation YearStep { get; } = new Animation(10, -10000, 10000);

    [Display(GroupName = "年号", Name = "年号を表示する")]
    [ToggleSlider]
    public bool ShowYears { get => showYears; set => Set(ref showYears, value); }
    bool showYears = true;

    [Display(GroupName = "年号", Name = "説明用の枠を出す（上下交互）")]
    [ToggleSlider]
    public bool Boxes { get => boxes; set => Set(ref boxes, value); }
    bool boxes = true;

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(55, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "左右の余白")]
    [AnimationSlider("F0", "%", 0, 40)]
    public Animation Margin { get; } = new Animation(8, 0, 49);

    [Display(GroupName = "位置と大きさ", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 1, 20)]
    public Animation LineWidth { get; } = new Animation(6, 0.5, 200);

    [Display(GroupName = "色", Name = "線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(0xE8, 0xEC, 0xF2);

    [Display(GroupName = "色", Name = "点・強調の色")]
    [ColorPicker]
    public WpfColor AccentColor { get => accentColor; set => Set(ref accentColor, value); }
    WpfColor accentColor = WpfColor.FromRgb(0x39, 0xC8, 0xFF);

    [Display(GroupName = "動き", Name = "1つ打つ間隔")]
    [AnimationSlider("F1", "秒", 0.2, 5)]
    public Animation StepTime { get; } = new Animation(1.2, 0.05, 600);

    [Display(GroupName = "動き", Name = "最初に線が伸びる時間")]
    [AnimationSlider("F1", "秒", 0, 3)]
    public Animation LineTime { get; } = new Animation(0.8, 0, 600);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Points, StartYear, YearStep, PosY, Margin, LineWidth, StepTime, LineTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = Math.Max(2, (int)MathF.Round(c.F(Points)));
        float y = c.F(PosY) / 100f * h, m = c.F(Margin) / 100f * w, lw = c.F(LineWidth);
        float lineT = c.F(LineTime), stepT = MathF.Max(0.05f, c.F(StepTime));
        float kLine = lineT <= 0.001f ? 1f : DrawKit.EaseOut(t / lineT);
        float S = MathF.Min(w, h) / 1080f;
        int year0 = (int)MathF.Round(c.F(StartYear)), ystep = (int)MathF.Round(c.F(YearStep));
        float x0 = m, x1 = w - m;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // ---- 線と矢印 ----
            float xe = x0 + (x1 - x0 + 30f * S) * kLine;
            brush.Color = Gfx.C4(LineColor, 1f);
            dc.DrawLine(new Vector2(x0, y), new Vector2(xe, y), brush, lw);
            if (kLine > 0.95f)
                Gfx.FillPoly(dc, brush, new List<Vector2> { new(xe + lw * 3f, y), new(xe - lw, y - lw * 2.2f), new(xe - lw, y + lw * 2.2f) });
            // 目盛り
            for (int i = 0; i <= (n - 1) * 4; i++)
            {
                float x = x0 + (x1 - x0 - 40f * S) * i / ((n - 1) * 4f);
                if (x > xe) break;
                brush.Color = Gfx.C4(LineColor, 0.5f);
                dc.DrawLine(new Vector2(x, y - lw * 1.2f), new Vector2(x, y + lw * 1.2f), brush, MathF.Max(1f, lw * 0.3f));
            }

            // ---- 点 ----
            float since0 = t - lineT * 0.6f;
            for (int i = 0; i < n; i++)
            {
                float lt = since0 - i * stepT;
                if (lt < 0f) continue;
                float x = x0 + (x1 - x0 - 40f * S) * i / (n - 1f);
                var p = new Vector2(x, y);
                float pop = DrawKit.EaseOutBack(lt / 0.35f);
                bool current = lt < stepT || i == n - 1;
                float r = (14f * S + lw) * pop;
                if (current)
                {
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    float u = DrawKit.Frac(t * 0.9f);
                    brush.Color = Gfx.C4(AccentColor, 0.7f * (1f - u));
                    dc.DrawEllipse(new Ellipse(p, r * (1.2f + u), r * (1.2f + u)), brush, 3f);
                    DrawKit.Glow(dc, p, r * 3f, Gfx.C4(AccentColor, 0.35f));
                    dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                }
                brush.Color = Gfx.C4(AccentColor, 1f);
                dc.FillEllipse(new Ellipse(p, r, r), brush);
                brush.Color = Gfx.C4(white, 1f);
                dc.FillEllipse(new Ellipse(p, r * 0.45f, r * 0.45f), brush);

                bool upper = i % 2 == 0;
                float side = upper ? -1f : 1f;
                float a = Math.Clamp(lt / 0.3f, 0f, 1f);
                if (ShowYears)
                {
                    float th = 34f * S;
                    string ys = (year0 + ystep * i).ToString(CultureInfo.InvariantCulture);
                    brush.Color = Gfx.C4(AccentColor, a);
                    StrokeFont.DrawCentered(dc, brush, ys, p + new Vector2(0, side * (r + th * 1.1f)), th, MathF.Max(2f, th * 0.1f));
                }
                if (Boxes)
                {
                    float bw = MathF.Min((x1 - x0) / (n - 1f) * 1.7f, 360f * S), bh = 110f * S;
                    float stem = 150f * S * DrawKit.EaseOut(lt / 0.4f);
                    float by = y + side * (r + 70f * S);
                    brush.Color = Gfx.C4(LineColor, 0.6f * a);
                    dc.DrawLine(new Vector2(x, by), new Vector2(x, by + side * stem * 0.4f), brush, 2f);
                    float bt = upper ? by - stem * 0.4f - bh : by + stem * 0.4f;
                    var box = new RawRectF(x - bw / 2f, bt, x + bw / 2f, bt + bh);
                    brush.Color = Gfx.C4(0.05f, 0.07f, 0.12f, 0.75f * a);
                    dc.FillRectangle(box, brush);
                    brush.Color = Gfx.C4(AccentColor, a);
                    dc.FillRectangle(new RawRectF(box.Left, upper ? box.Bottom - 4f * S : box.Top, box.Right, upper ? box.Bottom : box.Top + 4f * S), brush);
                    brush.Color = Gfx.C4(LineColor, 0.35f * a);
                    dc.DrawRectangle(box, brush, 1.5f);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
