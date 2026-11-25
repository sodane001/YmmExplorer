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
//  ロックオン・照準
//   探索 → ロック中 → ロック完了。ブラケットが縮んで標的を捉える
// =====================================================================
public sealed class LockOnPlugin : IShapePlugin
{
    public string Name => "演出：ロックオン・照準";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LockOnParameter(sharedData);
}

public sealed class LockOnParameter : EffectShapeParameterBase
{
    public LockOnParameter() : this(null) { }
    public LockOnParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "照準の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x39, 0xFF, 0x88);

    [Display(GroupName = "色", Name = "ロック完了の色")]
    [ColorPicker]
    public WpfColor LockColor { get => lockCol; set => Set(ref lockCol, value); }
    WpfColor lockCol = WpfColor.FromRgb(0xFF, 0x3A, 0x3A);

    [Display(GroupName = "位置と大きさ", Name = "標的のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "標的のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ（半径）")]
    [AnimationSlider("F0", "px", 30, 500)]
    public Animation Size { get; } = new Animation(150, 5, 5000);

    [Display(GroupName = "位置と大きさ", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 12)]
    public Animation Thickness { get; } = new Animation(3, 0.2, 100);

    [Display(GroupName = "動き", Name = "自動でロックオンする")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "動き", Name = "1回の長さ（自動）")]
    [AnimationSlider("F1", "秒", 1, 12)]
    public Animation Cycle { get; } = new Animation(4, 0.5, 120);

    [Display(GroupName = "動き", Name = "ロック進行（手動）", Description = "「自動」がオフのとき。0→100%でブラケットが縮んでロック完了")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Lock { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "装飾", Name = "目盛り・数値バーを表示する")]
    [ToggleSlider]
    public bool Decor { get => decor; set => Set(ref decor, value); }
    bool decor = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Size, Thickness, Cycle, Lock];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float R = c.F(Size), th = c.F(Thickness);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);

        // ---- 探索中(0) → ロック中(0〜1) → ロック完了(1) ----
        float lk, since = 0f;
        if (Auto)
        {
            float P = MathF.Max(0.5f, c.F(Cycle));
            float p = Gfx.Mod(t, P) / P;
            lk = Gfx.Smooth(0.1f, 0.65f, p);
            since = MathF.Max(0f, p - 0.65f) * P;                         // ロック完了からの秒数
        }
        else
        {
            lk = c.F(Lock) / 100f;
            since = lk >= 1f ? t : 0f;
        }
        bool locked = lk >= 0.999f;
        var col = Gfx.Mix(Color, LockColor, Gfx.Smooth(0.7f, 1f, lk));
        // 探索中は標的が少し揺れる
        var jit = locked ? Vector2.Zero : new Vector2(MathF.Sin(t * 3f) * 18f, MathF.Cos(t * 2.3f) * 12f) * (1f - lk);
        var tc = ctr + jit;

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 外側の破線リング（回転）----
        brush.Color = Gfx.C4(col, 0.7f);
        FormKit.Dash(dc, brush, Gfx.ArcPts(tc, R * 1.15f, t * 0.7f, t * 0.7f + MathF.PI * 2f, 100), 18f, 12f, 0f, th * 0.7f);

        // ---- 縮んでくる四隅のブラケット ----
        float bk = R * (1.0f + 1.4f * (1f - lk));
        float arm = R * 0.34f;
        brush.Color = Gfx.C4(col, 0.95f);
        for (int sx = -1; sx <= 1; sx += 2)
            for (int sy = -1; sy <= 1; sy += 2)
            {
                var cr = tc + new Vector2(sx * bk, sy * bk);
                dc.DrawLine(cr, cr - new Vector2(sx * arm, 0f), brush, th * 1.5f);
                dc.DrawLine(cr, cr - new Vector2(0f, sy * arm), brush, th * 1.5f);
            }

        // ---- 十字線と中心の円 ----
        float gap = R * 0.18f;
        brush.Color = Gfx.C4(col, 0.85f);
        dc.DrawLine(tc + new Vector2(-R * 0.75f, 0f), tc + new Vector2(-gap, 0f), brush, th);
        dc.DrawLine(tc + new Vector2(gap, 0f), tc + new Vector2(R * 0.75f, 0f), brush, th);
        dc.DrawLine(tc + new Vector2(0f, -R * 0.75f), tc + new Vector2(0f, -gap), brush, th);
        dc.DrawLine(tc + new Vector2(0f, gap), tc + new Vector2(0f, R * 0.75f), brush, th);
        dc.DrawEllipse(new Ellipse(tc, R * 0.45f, R * 0.45f), brush, th);
        dc.FillEllipse(new Ellipse(tc, th * 1.2f, th * 1.2f), brush);

        // ---- 目盛りと数値バー ----
        if (Decor)
        {
            for (int i = 0; i < 24; i++)
            {
                float a = i * MathF.PI / 12f;
                var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                brush.Color = Gfx.C4(col, 0.6f);
                dc.DrawLine(tc + d * (R * 0.45f + th * 2f), tc + d * (R * 0.45f + th * 2f + (i % 6 == 0 ? 14f : 6f)), brush, 1.2f);
            }
            // 右に出る「ロック進行」バー
            float bx = tc.X + R * 1.55f, by = tc.Y - R * 0.5f;
            for (int i = 0; i < 8; i++)
            {
                float on = (i + 1) / 8f <= lk + 0.001f ? 1f : 0.15f;
                brush.Color = Gfx.C4(col, 0.85f * on);
                Gfx.Rect(dc, brush, bx, by + i * R * 0.14f, R * 0.35f * (0.5f + 0.5f * (i % 2)), R * 0.08f);
            }
        }

        // ---- ロック完了：ブラケットの点滅と広がる輪 ----
        if (locked)
        {
            float blink = 0.5f + 0.5f * MathF.Sin(t * 22f);
            brush.Color = Gfx.C4(LockColor, 0.25f * blink);
            dc.FillEllipse(new Ellipse(tc, R * 0.45f, R * 0.45f), brush);
            float u = Gfx.Mod(since * 1.4f, 1f);
            brush.Color = Gfx.C4(LockColor, 0.8f * (1f - u));
            dc.DrawEllipse(new Ellipse(tc, R * (0.5f + 1.1f * u), R * (0.5f + 1.1f * u)), brush, th);
        }
    }
}
