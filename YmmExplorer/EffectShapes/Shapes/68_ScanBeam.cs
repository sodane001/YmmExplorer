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
//  スキャンビーム
//   範囲をなぞる走査の光。走査済みの部分にグリッドが現れる
// =====================================================================
public sealed class ScanBeamPlugin : IShapePlugin
{
    public string Name => "演出：スキャンビーム";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ScanBeamParameter(sharedData);
}

public sealed class ScanBeamParameter : EffectShapeParameterBase
{
    public ScanBeamParameter() : this(null) { }
    public ScanBeamParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x4F, 0xE8, 0xFF);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "走査する範囲の幅")]
    [AnimationSlider("F0", "px", 100, 1800)]
    public Animation ScanWidth { get; } = new Animation(640, 20, 8000);

    [Display(GroupName = "位置と大きさ", Name = "走査する範囲の高さ")]
    [AnimationSlider("F0", "px", 100, 1000)]
    public Animation ScanHeight { get; } = new Animation(420, 20, 6000);

    [Display(GroupName = "走査", Name = "左から右へ走査する（オフ=上から下）")]
    [ToggleSlider]
    public bool Vertical { get => vertical; set => Set(ref vertical, value); }
    bool vertical = false;

    [Display(GroupName = "走査", Name = "往復する")]
    [ToggleSlider]
    public bool PingPong { get => pingPong; set => Set(ref pingPong, value); }
    bool pingPong = true;

    [Display(GroupName = "走査", Name = "1回の時間")]
    [AnimationSlider("F1", "秒", 0.5, 12)]
    public Animation Period { get; } = new Animation(3, 0.2, 120);

    [Display(GroupName = "走査", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 1, 12)]
    public Animation Thickness { get; } = new Animation(3, 0.5, 100);

    [Display(GroupName = "見た目", Name = "走査済みにグリッドを出す")]
    [ToggleSlider]
    public bool ShowGrid { get => showGrid; set => Set(ref showGrid, value); }
    bool showGrid = true;

    [Display(GroupName = "見た目", Name = "四隅の枠を表示する")]
    [ToggleSlider]
    public bool ShowBrackets { get => brackets; set => Set(ref brackets, value); }
    bool brackets = true;

    [Display(GroupName = "見た目", Name = "データバーを表示する")]
    [ToggleSlider]
    public bool ShowBars { get => bars; set => Set(ref bars, value); }
    bool bars = true;

    [Display(GroupName = "見た目", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, ScanWidth, ScanHeight, Period, Thickness, Brightness];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float W = c.F(ScanWidth), H = c.F(ScanHeight), th = c.F(Thickness), B = c.F(Brightness) / 100f;
        float x0 = ctr.X - W / 2f, y0 = ctr.Y - H / 2f, x1 = ctr.X + W / 2f, y1 = ctr.Y + H / 2f;
        var white = WpfColor.FromRgb(255, 255, 255);

        float u = Gfx.Mod(t / MathF.Max(0.2f, c.F(Period)), 1f);
        float p = PingPong ? (u < 0.5f ? u * 2f : 2f - u * 2f) : u;               // 0〜1
        bool forward = !PingPong || u < 0.5f;
        float len = Vertical ? W : H;                                             // 走査する方向の長さ
        float pos = p * len;

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 走査済みの部分：うっすら色が付き、グリッドが現れる ----
        float sx0 = x0, sy0 = y0, sw, sh;
        if (Vertical) { sw = pos; sh = H; } else { sw = W; sh = pos; }
        brush.Color = Gfx.C4(Color, 0.07f * B);
        Gfx.Rect(dc, brush, sx0, sy0, sw, sh);
        if (ShowGrid)
        {
            float g = 28f;
            brush.Color = Gfx.C4(Color, 0.22f * B);
            for (float x = x0; x <= x0 + sw + 0.1f; x += g) dc.DrawLine(new Vector2(x, y0), new Vector2(x, y0 + sh), brush, 0.9f);
            for (float y = y0; y <= y0 + sh + 0.1f; y += g) dc.DrawLine(new Vector2(x0, y), new Vector2(x0 + sw, y), brush, 0.9f);
            brush.Color = Gfx.C4(white, 0.4f * B);
            for (float x = x0; x <= x0 + sw + 0.1f; x += g * 2f)
                for (float y = y0; y <= y0 + sh + 0.1f; y += g * 2f)
                    dc.FillEllipse(new Ellipse(new Vector2(x, y), 1.5f, 1.5f), brush);
        }

        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 走査線の後ろに引く光の帯 ----
            const int N = 28;
            float dirSign = forward ? -1f : 1f;                                    // 進行方向の後ろ側
            for (int i = 0; i < N; i++)
            {
                float f = i / (float)N;
                float off = dirSign * f * (Vertical ? W : H) * 0.22f;
                float al = (1f - f) * (1f - f) * 0.16f * B;
                brush.Color = Gfx.C4(Color, al);
                if (Vertical) Gfx.Rect(dc, brush, MathF.Min(x0 + pos + off, x0 + pos + off + (Vertical ? W : H) * 0.22f / N), y0, (Vertical ? W : H) * 0.22f / N + 1f, H);
                else Gfx.Rect(dc, brush, x0, MathF.Min(y0 + pos + off, y0 + pos + off + H * 0.22f / N), W, H * 0.22f / N + 1f);
            }

            // ---- 走査線の本体 ----
            Vector2 a, b2;
            if (Vertical) { a = new Vector2(x0 + pos, y0 - 14f); b2 = new Vector2(x0 + pos, y1 + 14f); }
            else { a = new Vector2(x0 - 14f, y0 + pos); b2 = new Vector2(x1 + 14f, y0 + pos); }
            brush.Color = Gfx.C4(Color, 0.14f * B);
            dc.DrawLine(a, b2, brush, th * 7f);
            brush.Color = Gfx.C4(Color, 0.45f * B);
            dc.DrawLine(a, b2, brush, th * 3f);
            brush.Color = Gfx.C4(white, 0.95f * B);
            dc.DrawLine(a, b2, brush, th);
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }

        // ---- 四隅の枠 ----
        brush.Color = Gfx.C4(Color, 0.95f * B);
        if (ShowBrackets)
        {
            float arm = MathF.Min(W, H) * 0.09f;
            foreach (var (sx, sy) in new[] { (1, 1), (-1, 1), (1, -1), (-1, -1) })
            {
                var cr = new Vector2(sx > 0 ? x0 : x1, sy > 0 ? y0 : y1);
                dc.DrawLine(cr, cr + new Vector2(sx * arm, 0f), brush, th * 1.4f);
                dc.DrawLine(cr, cr + new Vector2(0f, sy * arm), brush, th * 1.4f);
            }
        }

        // ---- 横にそえる目盛りとデータバー ----
        if (ShowBars)
        {
            // 走査位置を示す目盛り（範囲の脇）
            for (int i = 0; i <= 10; i++)
            {
                float f = i / 10f;
                brush.Color = Gfx.C4(Color, 0.7f * B);
                if (Vertical) dc.DrawLine(new Vector2(x0 + W * f, y0 - 8f), new Vector2(x0 + W * f, y0 - (i % 5 == 0 ? 20f : 13f)), brush, 1.2f);
                else dc.DrawLine(new Vector2(x0 - 8f, y0 + H * f), new Vector2(x0 - (i % 5 == 0 ? 20f : 13f), y0 + H * f), brush, 1.2f);
            }
            int tick = (int)MathF.Floor(t * 10f);
            var rb = new Random(Seed * 31 + tick);
            for (int i = 0; i < 6; i++)
            {
                float lenb = 20f + 90f * rb.NextF();
                brush.Color = Gfx.C4(Color, 0.75f * B);
                if (Vertical) Gfx.Rect(dc, brush, x0 + 12f + i * 22f, y1 + 14f, 14f, lenb * 0.5f);
                else Gfx.Rect(dc, brush, x1 + 14f, y0 + 12f + i * 20f, lenb, 10f);
            }
        }
    }
}
