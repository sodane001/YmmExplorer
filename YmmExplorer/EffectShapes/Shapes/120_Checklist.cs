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
//  チェックリスト
//   項目のチェック欄に、順番にチェックが入る。項目の文字は YMM4 のテキストを重ねてください
// =====================================================================
public sealed class ChecklistPlugin : IShapePlugin
{
    public string Name => "演出：チェックリスト";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ChecklistParameter(sharedData);
}

public sealed class ChecklistParameter : EffectShapeParameterBase
{
    public ChecklistParameter() : this(null) { }
    public ChecklistParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "項目", Name = "項目の数")]
    [AnimationSlider("F0", "", 1, 10)]
    public Animation Items { get; } = new Animation(4, 1, 30);

    [Display(GroupName = "項目", Name = "チェック済みの数（手動）")]
    [AnimationSlider("F1", "", 0, 10)]
    public Animation Checked { get; } = new Animation(0, 0, 30);

    [Display(GroupName = "項目", Name = "時間で自動でチェックする")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "項目", Name = "1つチェックする間隔（自動）")]
    [AnimationSlider("F1", "秒", 0.3, 5)]
    public Animation StepTime { get; } = new Animation(1.2, 0.05, 600);

    [Display(GroupName = "項目", Name = "チェックした行に線を引く")]
    [ToggleSlider]
    public bool Strike { get => strike; set => Set(ref strike, value); }
    bool strike = false;

    [Display(GroupName = "項目", Name = "番号を表示する")]
    [ToggleSlider]
    public bool Numbers { get => numbers; set => Set(ref numbers, value); }
    bool numbers = false;

    [Display(GroupName = "位置と大きさ", Name = "左上のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(18, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "左上のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(22, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "行の幅")]
    [AnimationSlider("F0", "px", 200, 1800)]
    public Animation RowWidth { get; } = new Animation(1000, 10, 10000);

    [Display(GroupName = "位置と大きさ", Name = "行の高さ")]
    [AnimationSlider("F0", "px", 30, 200)]
    public Animation RowHeight { get; } = new Animation(96, 4, 2000);

    [Display(GroupName = "色", Name = "チェックの色")]
    [ColorPicker]
    public WpfColor CheckColor { get => checkColor; set => Set(ref checkColor, value); }
    WpfColor checkColor = WpfColor.FromRgb(0x2E, 0xC8, 0x6A);

    [Display(GroupName = "色", Name = "枠・線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(0xF2, 0xF4, 0xF8);

    [Display(GroupName = "色", Name = "行の背景の色")]
    [ColorPicker]
    public WpfColor RowColor { get => rowColor; set => Set(ref rowColor, value); }
    WpfColor rowColor = WpfColor.FromRgb(0x18, 0x20, 0x34);

    [Display(GroupName = "色", Name = "行の背景の不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation RowOpacity { get; } = new Animation(70, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Items, Checked, StepTime, PosX, PosY, RowWidth, RowHeight, RowOpacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = Math.Max(1, (int)MathF.Round(c.F(Items)));
        float step = MathF.Max(0.05f, c.F(StepTime));
        float done = Auto ? (t - 0.5f) / step + 1f : c.F(Checked);   // 1.0 で1つ目が完了
        float x0 = c.F(PosX) / 100f * w, y0 = c.F(PosY) / 100f * h;
        float RW = c.F(RowWidth), RH = c.F(RowHeight), rop = c.F(RowOpacity) / 100f;
        float box = RH * 0.52f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            for (int i = 0; i < n; i++)
            {
                float appear = DrawKit.EaseOut((t - i * 0.08f) / 0.35f);
                if (appear <= 0f) continue;
                float y = y0 + i * RH * 1.15f;
                float xs = x0 - (1f - appear) * 60f;
                float k = Auto ? Math.Clamp((done - i) * 2.5f, 0f, 1f) : Math.Clamp(done - i, 0f, 1f);   // このチェックの進み
                bool isDone = k >= 1f;
                bool active = Auto ? (done > i && done < i + 1.6f) : (k > 0f && k < 1f);
                // 行
                var rowRect = new RawRectF(xs, y, xs + RW, y + RH);
                brush.Color = Gfx.C4(isDone ? Gfx.Mix(RowColor, CheckColor, 0.18f) : RowColor, rop * appear);
                dc.FillRectangle(rowRect, brush);
                if (active)
                {
                    brush.Color = Gfx.C4(CheckColor, 0.9f * appear);
                    dc.FillRectangle(new RawRectF(xs, y, xs + 6f, y + RH), brush);
                }
                float bx = xs + RH * 0.3f + (Numbers ? RH * 0.55f : 0f), by = y + (RH - box) / 2f;
                if (Numbers)
                {
                    brush.Color = Gfx.C4(LineColor, 0.7f * appear);
                    StrokeFont.DrawCentered(dc, brush, (i + 1).ToString(CultureInfo.InvariantCulture), new Vector2(xs + RH * 0.45f, y + RH / 2f), RH * 0.34f, MathF.Max(1.5f, RH * 0.04f));
                }
                // チェック欄
                brush.Color = Gfx.C4(LineColor, appear);
                dc.DrawRectangle(new RawRectF(bx, by, bx + box, by + box), brush, MathF.Max(2f, RH * 0.04f));
                if (k > 0f)
                {
                    float pop = Auto ? DrawKit.EaseOutBack(k) : 1f;
                    var ctr = new Vector2(bx + box / 2f, by + box / 2f);
                    var mark = new List<Vector2> { ctr + new Vector2(-box * 0.38f, 0), ctr + new Vector2(-box * 0.08f, box * 0.32f), ctr + new Vector2(box * 0.55f, -box * 0.55f) };
                    var scaled = mark.Select(p => ctr + (p - ctr) * pop).ToList();
                    brush.Color = Gfx.C4(CheckColor, appear);
                    Gfx.Polyline(dc, brush, PathKit.Part(scaled, Math.Clamp(k * 1.6f, 0f, 1f)), MathF.Max(3f, box * 0.16f));
                    if (k < 1f && Auto)
                    {
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        DrawKit.Glow(dc, ctr, box * 1.4f, Gfx.C4(CheckColor, 0.5f * (1f - k)));
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                    }
                }
                // 取り消し線
                if (Strike && k > 0.5f)
                {
                    float sx0 = bx + box + RH * 0.35f, sx1 = xs + RW - RH * 0.3f;
                    float u = Math.Clamp((k - 0.5f) * 2f, 0f, 1f);
                    brush.Color = Gfx.C4(LineColor, 0.75f * appear);
                    dc.DrawLine(new Vector2(sx0, y + RH / 2f), new Vector2(sx0 + (sx1 - sx0) * u, y + RH / 2f), brush, MathF.Max(2f, RH * 0.04f));
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
