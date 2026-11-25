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
//  ステップ・進行表示
//   「Step1→2→3」を線と丸でつなぎ、順に点灯する。手順の解説に
// =====================================================================
public sealed class StepProgressPlugin : IShapePlugin
{
    public string Name => "演出：ステップ・進行表示";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StepProgressParameter(sharedData);
}

public sealed class StepProgressParameter : EffectShapeParameterBase
{
    public StepProgressParameter() : this(null) { }
    public StepProgressParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "ステップ", Name = "ステップの数")]
    [AnimationSlider("F0", "", 2, 10)]
    public Animation Steps { get; } = new Animation(4, 2, 30);

    [Display(GroupName = "ステップ", Name = "いまのステップ（手動）", Description = "1.5 のように小数にすると、線が途中まで伸びます")]
    [AnimationSlider("F1", "", 0, 10)]
    public Animation Current { get; } = new Animation(1, 0, 30);

    [Display(GroupName = "ステップ", Name = "自動で進める")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "ステップ", Name = "1ステップの時間（自動）")]
    [AnimationSlider("F1", "秒", 0.3, 5)]
    public Animation StepTime { get; } = new Animation(1.5, 0.05, 120);

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(80, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "全体の幅")]
    [AnimationSlider("F0", "px", 200, 1800)]
    public Animation TotalWidth { get; } = new Animation(1200, 10, 10000);

    [Display(GroupName = "位置と大きさ", Name = "丸の大きさ（半径）")]
    [AnimationSlider("F0", "px", 10, 100)]
    public Animation NodeSize { get; } = new Animation(36, 2, 1000);

    [Display(GroupName = "位置と大きさ", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 1, 30)]
    public Animation LineWidth { get; } = new Animation(8, 0.2, 300);

    [Display(GroupName = "位置と大きさ", Name = "縦に並べる")]
    [ToggleSlider]
    public bool Vertical { get => vertical; set => Set(ref vertical, value); }
    bool vertical = false;

    [Display(GroupName = "色", Name = "いまのステップの色")]
    [ColorPicker]
    public WpfColor ActiveColor { get => activeColor; set => Set(ref activeColor, value); }
    WpfColor activeColor = WpfColor.FromRgb(0x39, 0xD0, 0xFF);

    [Display(GroupName = "色", Name = "終わったステップの色")]
    [ColorPicker]
    public WpfColor DoneColor { get => doneColor; set => Set(ref doneColor, value); }
    WpfColor doneColor = WpfColor.FromRgb(0x4C, 0xE0, 0x7A);

    [Display(GroupName = "色", Name = "まだのステップの色")]
    [ColorPicker]
    public WpfColor IdleColor { get => idleColor; set => Set(ref idleColor, value); }
    WpfColor idleColor = WpfColor.FromRgb(0x5A, 0x64, 0x78);

    [Display(GroupName = "色", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);

    [Display(GroupName = "表示", Name = "「STEP 1」などの文字を表示する")]
    [ToggleSlider]
    public bool Labels { get => labels; set => Set(ref labels, value); }
    bool labels = true;

    [Display(GroupName = "表示", Name = "終わったら ✓ にする")]
    [ToggleSlider]
    public bool Check { get => check; set => Set(ref check, value); }
    bool check = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Steps, Current, StepTime, PosX, PosY, TotalWidth, NodeSize, LineWidth];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = Math.Max(2, (int)MathF.Round(c.F(Steps)));
        float cur = Auto ? 1f + t / MathF.Max(0.05f, c.F(StepTime)) : c.F(Current);
        // 自動のときは、次の丸へ線が伸びる動きをなめらかに
        float prog = Auto ? MathF.Floor(cur) + Gfx.Smooth(0.0f, 0.45f, cur - MathF.Floor(cur)) : cur;
        prog = Math.Clamp(prog, 0f, n);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float W = c.F(TotalWidth), R = c.F(NodeSize), lw = c.F(LineWidth);
        var dir = Vertical ? new Vector2(0, 1) : new Vector2(1, 0);
        var start = ctr - dir * W / 2f;
        Vector2 NodePos(int i) => start + dir * (W * i / (n - 1));
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // ---- 線 ----
            brush.Color = Gfx.C4(IdleColor, 0.9f);
            dc.DrawLine(NodePos(0), NodePos(n - 1), brush, lw);
            float fill = Math.Clamp((prog - 1f) / (n - 1), 0f, 1f);
            if (fill > 0f)
            {
                var end = Vector2.Lerp(NodePos(0), NodePos(n - 1), fill);
                using var lg = Gfx.Linear(dc, NodePos(0), NodePos(n - 1), (0f, Gfx.C4(DoneColor, 1f)), (1f, Gfx.C4(ActiveColor, 1f)));
                dc.DrawLine(NodePos(0), end, lg, lw);
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, end, lw * 3f, Gfx.C4(ActiveColor, 0.7f));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }

            // ---- 丸 ----
            for (int i = 0; i < n; i++)
            {
                var p = NodePos(i);
                float lit = Math.Clamp(prog - i, 0f, 1f);       // 0=まだ, 1=点灯
                bool done = prog >= i + 2f - 0.001f;
                bool active = lit > 0f && !done;
                float since = Auto ? MathF.Max(0f, (cur - (i + 1f)) * MathF.Max(0.05f, c.F(StepTime))) : 1f;
                float pop = active && Auto ? 1f + 0.35f * MathF.Exp(-since * 6f) * MathF.Cos(since * 18f) : 1f;
                var col = done ? DoneColor : active ? ActiveColor : IdleColor;
                float r = R * pop;

                if (active)
                {
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    DrawKit.Glow(dc, p, r * 2.4f, Gfx.C4(ActiveColor, 0.45f));
                    float u = DrawKit.Frac(t * 0.9f);
                    brush.Color = Gfx.C4(ActiveColor, 0.7f * (1f - u));
                    dc.DrawEllipse(new Ellipse(p, r * (1.1f + 0.7f * u), r * (1.1f + 0.7f * u)), brush, 3f);
                    dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                }
                brush.Color = Gfx.C4(0.06f, 0.07f, 0.1f, 1f);
                dc.FillEllipse(new Ellipse(p, r + lw * 0.6f, r + lw * 0.6f), brush);
                brush.Color = Gfx.C4(col, lit > 0f ? 1f : 0.9f);
                if (lit > 0f) dc.FillEllipse(new Ellipse(p, r, r), brush);
                else dc.DrawEllipse(new Ellipse(p, r - 2f, r - 2f), brush, 4f);

                if (done && Check)
                {
                    brush.Color = Gfx.C4(white, 1f);
                    Gfx.Polyline(dc, brush, new List<Vector2> { p + new Vector2(-r * 0.42f, 0), p + new Vector2(-r * 0.12f, r * 0.32f), p + new Vector2(r * 0.45f, -r * 0.35f) }, MathF.Max(2f, r * 0.16f));
                }
                else
                {
                    brush.Color = Gfx.C4(lit > 0f ? white : IdleColor, 1f);
                    StrokeFont.DrawCentered(dc, brush, (i + 1).ToString(CultureInfo.InvariantCulture), p, r * 0.9f, MathF.Max(1.5f, r * 0.12f));
                }

                if (Labels)
                {
                    var lp = Vertical ? p + new Vector2(r * 1.6f + StrokeFont.Width("STEP " + (i + 1), r * 0.55f) / 2f, 0) : p + new Vector2(0, r * 1.9f);
                    brush.Color = Gfx.C4(lit > 0f ? TextColor : IdleColor, lit > 0f ? 1f : 0.8f);
                    StrokeFont.DrawCentered(dc, brush, "STEP " + (i + 1).ToString(CultureInfo.InvariantCulture), lp, r * 0.55f, MathF.Max(1.2f, r * 0.07f));
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
