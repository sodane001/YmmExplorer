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
//  宝箱が開く
//   ガタガタ揺れたあとフタが開き、光と金貨があふれ出す
// =====================================================================
public sealed class TreasureChestPlugin : IShapePlugin
{
    public string Name => "演出：宝箱が開く";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TreasureChestParameter(sharedData);
}

public sealed class TreasureChestParameter : EffectShapeParameterBase
{
    public TreasureChestParameter() : this(null) { }
    public TreasureChestParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "箱の色")]
    [ColorPicker]
    public WpfColor BoxColor { get => boxColor; set => Set(ref boxColor, value); }
    WpfColor boxColor = WpfColor.FromRgb(0x9A, 0x52, 0x22);

    [Display(GroupName = "色", Name = "金具の色")]
    [ColorPicker]
    public WpfColor MetalColor { get => metalColor; set => Set(ref metalColor, value); }
    WpfColor metalColor = WpfColor.FromRgb(0xF2, 0xC2, 0x2E);

    [Display(GroupName = "色", Name = "あふれる光の色")]
    [ColorPicker]
    public WpfColor LightColor { get => lightColor; set => Set(ref lightColor, value); }
    WpfColor lightColor = WpfColor.FromRgb(0xFF, 0xE6, 0x8A);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（箱の底）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "箱の幅")]
    [AnimationSlider("F0", "px", 80, 900)]
    public Animation BoxWidth { get; } = new Animation(380, 10, 5000);

    [Display(GroupName = "動き", Name = "開くまでの時間（揺れる）")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation OpenTime { get; } = new Animation(0.8, 0, 600);

    [Display(GroupName = "動き", Name = "金貨の数")]
    [AnimationSlider("F0", "枚", 0, 80)]
    public Animation Coins { get; } = new Animation(24, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, BoxWidth, OpenTime, Coins];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var bottom = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float BW = c.F(BoxWidth), BH = BW * 0.5f, LH = BW * 0.3f;
        float openAt = c.F(OpenTime);
        float since = t - openAt;
        float open = since <= 0f ? 0f : DrawKit.EaseOutBack(Math.Clamp(since / 0.35f, 0f, 1f));
        float shake = since < 0f ? MathF.Sin(t * 45f) * BW * 0.012f * Gfx.Smooth(0f, openAt, t) : 0f;
        var b = bottom + new Vector2(shake, 0);
        float x0 = b.X - BW / 2f, x1 = b.X + BW / 2f, yTop = b.Y - BH;
        var dark = Gfx.Mix(BoxColor, WpfColor.FromRgb(0, 0, 0), 0.45f);
        var lite = Gfx.Mix(BoxColor, WpfColor.FromRgb(255, 220, 160), 0.25f);
        var mdark = Gfx.Mix(MetalColor, WpfColor.FromRgb(90, 50, 0), 0.5f);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // 影
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f);
            dc.FillEllipse(new Ellipse(b + new Vector2(0, BH * 0.02f), BW * 0.62f, BH * 0.12f), brush);

            // ---- 開いた口の中の光（箱の後ろ）----
            if (open > 0f)
            {
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                var mouth = new Vector2(b.X, yTop);
                DrawKit.Glow(dc, mouth, BW * 1.6f * open, Gfx.C4(LightColor, 0.55f));
                for (int i = 0; i < 14; i++)
                {
                    float a = -MathF.PI / 2f + (i / 13f - 0.5f) * 1.6f + MathF.Sin(t * 1.5f + i) * 0.04f;
                    var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    float len = BW * (1.6f + 0.8f * DrawKit.H(Seed, i, 1)) * open;
                    Gfx.FadeLine(dc, brush, mouth + new Vector2((i / 13f - 0.5f) * BW * 0.7f, 0), mouth + d * len, BW * 0.12f, BW * 0.35f, LightColor, 0.35f, 10, 1.2f);
                }
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }

            // ---- 本体 ----
            using (var lg = Gfx.Linear(dc, new Vector2(0, yTop), new Vector2(0, b.Y), (0f, Gfx.C4(lite, 1f)), (1f, Gfx.C4(dark, 1f))))
                dc.FillRectangle(new RawRectF(x0, yTop, x1, b.Y), lg);
            brush.Color = Gfx.C4(dark, 0.6f);
            for (int i = 1; i < 4; i++) dc.DrawLine(new Vector2(x0, yTop + BH * i / 4f), new Vector2(x1, yTop + BH * i / 4f), brush, 2f);
            // 金具（縦の帯と角）
            brush.Color = Gfx.C4(MetalColor, 1f);
            foreach (float bx in new[] { x0 + BW * 0.06f, x1 - BW * 0.06f - BW * 0.08f })
                dc.FillRectangle(new RawRectF(bx, yTop, bx + BW * 0.08f, b.Y), brush);
            dc.FillRectangle(new RawRectF(x0 - 3f, b.Y - BH * 0.1f, x1 + 3f, b.Y), brush);
            brush.Color = Gfx.C4(mdark, 1f);
            dc.DrawRectangle(new RawRectF(x0, yTop, x1, b.Y), brush, 3f);

            // ---- フタ（奥に倒れる＝縦につぶれて上へ）----
            float lidH = LH * MathF.Cos(MathF.Min(1.35f, open * 1.35f) * 1.0f);
            float lidTop = yTop - LH - (LH - lidH) * 0.0f;
            float lidBottom = yTop;
            if (open > 0f)
            {
                // 開いたフタは箱の上の奥にあり、内側が見える
                float hingeY = yTop - BH * 0.02f;
                float inside = LH * MathF.Sin(MathF.Min(1.35f, open * 1.35f));
                brush.Color = Gfx.C4(Gfx.Mix(dark, WpfColor.FromRgb(0, 0, 0), 0.3f), 1f);
                dc.FillRectangle(new RawRectF(x0 + BW * 0.02f, hingeY - inside, x1 - BW * 0.02f, hingeY), brush);
                brush.Color = Gfx.C4(MetalColor, 1f);
                dc.FillRectangle(new RawRectF(x0, hingeY - inside, x1, hingeY - inside + BH * 0.05f), brush);
                // 口の中の金色
                brush.Color = Gfx.C4(Gfx.Mix(MetalColor, white, 0.3f), 1f);
                dc.FillRectangle(new RawRectF(x0 + BW * 0.04f, yTop - BH * 0.02f, x1 - BW * 0.04f, yTop + BH * 0.06f), brush);
            }
            else
            {
                var lid = new List<Vector2>();
                for (int i = 0; i <= 24; i++)
                {
                    float u = i / 24f;
                    lid.Add(new Vector2(x0 + BW * u, lidTop + LH * (1f - MathF.Sin(MathF.PI * u)) * 0.55f));
                }
                lid.Add(new Vector2(x1, lidBottom));
                lid.Add(new Vector2(x0, lidBottom));
                using (var lg = Gfx.Linear(dc, new Vector2(0, lidTop), new Vector2(0, lidBottom), (0f, Gfx.C4(lite, 1f)), (1f, Gfx.C4(BoxColor, 1f))))
                    Gfx.FillPoly(dc, lg, lid);
                brush.Color = Gfx.C4(MetalColor, 1f);
                dc.FillRectangle(new RawRectF(x0 - 2f, lidBottom - BH * 0.08f, x1 + 2f, lidBottom), brush);
                // 錠前
                var lockC = new Vector2(b.X, lidBottom);
                brush.Color = Gfx.C4(MetalColor, 1f);
                dc.FillRectangle(new RawRectF(lockC.X - BW * 0.07f, lockC.Y - BH * 0.12f, lockC.X + BW * 0.07f, lockC.Y + BH * 0.18f), brush);
                brush.Color = Gfx.C4(mdark, 1f);
                dc.FillEllipse(new Ellipse(lockC + new Vector2(0, BH * 0.02f), BW * 0.018f, BW * 0.018f), brush);
                dc.FillRectangle(new RawRectF(lockC.X - BW * 0.008f, lockC.Y + BH * 0.02f, lockC.X + BW * 0.008f, lockC.Y + BH * 0.1f), brush);
            }

            // ---- あふれる金貨ときらめき ----
            if (open > 0f)
            {
                int n = (int)c.F(Coins);
                for (int i = 0; i < n; i++)
                {
                    float d = DrawKit.H(Seed, i, 2) * 0.5f;
                    float a = since - d;
                    if (a < 0f || a > 1.6f) continue;
                    float vx = (DrawKit.H(Seed, i, 3) - 0.5f) * BW * 2.2f;
                    float vy = -BW * (2.2f + 1.4f * DrawKit.H(Seed, i, 4));
                    var p = new Vector2(b.X + (DrawKit.H(Seed, i, 5) - 0.5f) * BW * 0.5f, yTop) + new Vector2(vx * a, vy * a + 0.5f * BW * 6f * a * a);
                    if (p.Y > b.Y + BW) continue;
                    float r = BW * 0.045f;
                    float flip = MathF.Abs(MathF.Cos(a * 10f + i));
                    brush.Color = Gfx.C4(mdark, 1f);
                    dc.FillEllipse(new Ellipse(p, r * flip + 1f, r), brush);
                    brush.Color = Gfx.C4(MetalColor, 1f);
                    dc.FillEllipse(new Ellipse(p, r * flip * 0.8f + 0.5f, r * 0.8f), brush);
                }
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                for (int i = 0; i < 8; i++)
                {
                    float ph = DrawKit.Frac(t * 0.8f + DrawKit.H(Seed, i, 6));
                    var sp = new Vector2(b.X + (DrawKit.H(Seed, i, 7) - 0.5f) * BW * 1.6f, yTop - BW * 0.9f * DrawKit.H(Seed, i, 8));
                    DrawKit.Twinkle(dc, brush, sp, BW * 0.08f * MathF.Sin(MathF.PI * ph), 2f, white, 0.9f * MathF.Sin(MathF.PI * ph));
                }
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
