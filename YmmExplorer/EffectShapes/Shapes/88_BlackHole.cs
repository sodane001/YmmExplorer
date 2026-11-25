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
//  ブラックホール
//   降着円盤と、光が曲がる重力レンズ。吸い込まれる星
// =====================================================================
public sealed class BlackHolePlugin : IShapePlugin
{
    public string Name => "演出：ブラックホール";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BlackHoleParameter(sharedData);
}

public sealed class BlackHoleParameter : EffectShapeParameterBase
{
    public BlackHoleParameter() : this(null) { }
    public BlackHoleParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "円盤の色（外側）")]
    [ColorPicker]
    public WpfColor DiskColor { get => diskColor; set => Set(ref diskColor, value); }
    WpfColor diskColor = WpfColor.FromRgb(0xFF, 0x7A, 0x28);

    [Display(GroupName = "色", Name = "円盤の色（内側）")]
    [ColorPicker]
    public WpfColor InnerColor { get => innerColor; set => Set(ref innerColor, value); }
    WpfColor innerColor = WpfColor.FromRgb(0xFF, 0xF0, 0xC8);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "黒い穴の半径")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation Radius { get; } = new Animation(110, 2, 3000);

    [Display(GroupName = "位置と大きさ", Name = "円盤の大きさ（外側の半径）")]
    [AnimationSlider("F0", "px", 50, 1200)]
    public Animation DiskSize { get; } = new Animation(330, 5, 8000);

    [Display(GroupName = "位置と大きさ", Name = "円盤の傾き")]
    [AnimationSlider("F0", "°", 0, 90)]
    public Animation Tilt { get; } = new Animation(14, 0, 90);

    [Display(GroupName = "位置と大きさ", Name = "全体の回転")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Rotation { get; } = new Animation(-8, -3600, 3600);

    [Display(GroupName = "動き", Name = "円盤の回る速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Spin { get; } = new Animation(100, -2000, 2000);

    [Display(GroupName = "動き", Name = "吸い込まれる星の数")]
    [AnimationSlider("F0", "個", 0, 800)]
    public Animation StarCount { get; } = new Animation(260, 0, 5000);

    [Display(GroupName = "動き", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "背景", Name = "宇宙の背景（黒）の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BgOpacity { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, DiskSize, Tilt, Rotation, Spin, StarCount, Brightness, BgOpacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), D = MathF.Max(R * 1.6f, c.F(DiskSize));
        float squash = MathF.Max(0.04f, MathF.Sin(Gfx.Rad(c.F(Tilt))));
        float spin = c.F(Spin) / 100f, bright = c.F(Brightness) / 100f, bg = c.F(BgOpacity) / 100f;
        float rot = Gfx.Rad(c.F(Rotation));
        int stars = (int)c.F(StarCount);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        var prev = dc.Transform;
        try
        {
            if (bg > 0f)
            {
                brush.Color = Gfx.C4(0f, 0f, 0.01f, bg);
                dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            }

            dc.PrimitiveBlend = PrimitiveBlend.Add;

            // ---- 吸い込まれる星（渦を巻いて中心へ）----
            float maxR = MathF.Sqrt(w * w + h * h) * 0.6f;
            var rng = new Random(Seed);
            for (int i = 0; i < stars; i++)
            {
                float a0 = rng.NextF() * MathF.PI * 2f, off = rng.NextF(), sp = 0.04f + 0.08f * rng.NextF(), sz = 0.6f + 1.6f * rng.NextF();
                float ph = DrawKit.Frac(t * sp * MathF.Abs(spin) + off);
                float rr = R * 1.15f + (maxR - R) * MathF.Pow(1f - ph, 1.6f);
                float ang = a0 + MathF.Sign(spin == 0 ? 1 : spin) * (R * 4f / rr) * 1.5f;
                var p = ctr + new Vector2(MathF.Cos(ang) * rr, MathF.Sin(ang) * rr);
                float near = Gfx.Smooth(maxR * 0.5f, R * 1.3f, rr);
                float alpha = (0.35f + 0.65f * near) * Gfx.Smooth(R * 1.1f, R * 1.6f, rr) * bright;
                var tang = new Vector2(-MathF.Sin(ang), MathF.Cos(ang)) * MathF.Sign(spin == 0 ? 1 : spin);
                var inward = -Vector2.Normalize(p - ctr);
                float len = 2f + 40f * near;
                brush.Color = Gfx.C4(Gfx.Mix(white, InnerColor, near), alpha);
                dc.DrawLine(p, p - (tang * 0.8f + inward * 0.6f) * len, brush, sz);
            }

            dc.Transform = Matrix3x2.CreateRotation(rot, ctr) * prev;

            // ---- 円盤の奥半分 ----
            DrawDisk(dc, brush, ctr, R, D, squash, t * spin, bright, back: true);

            // ---- 重力レンズ：円盤の向こう側が穴のふちに沿って曲がって見える ----
            for (int k = 0; k < 5; k++)
            {
                float rr = R * (1.12f + 0.09f * k);
                float a = (0.5f - k * 0.08f) * bright;
                brush.Color = Gfx.C4(Gfx.Mix(InnerColor, DiskColor, k / 5f), a);
                dc.DrawEllipse(new Ellipse(ctr, rr, rr * (0.92f + 0.02f * k)), brush, R * 0.08f);
            }
            DrawKit.Glow(dc, ctr, R * 2.2f, Gfx.C4(DiskColor, 0.35f * bright));

            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

            // ---- 事象の地平面（黒い穴）----
            brush.Color = Gfx.C4(0f, 0f, 0f, 1f);
            dc.FillEllipse(new Ellipse(ctr, R, R), brush);

            dc.PrimitiveBlend = PrimitiveBlend.Add;
            brush.Color = Gfx.C4(InnerColor, 0.85f * bright);
            dc.DrawEllipse(new Ellipse(ctr, R * 1.02f, R * 1.02f), brush, MathF.Max(1.5f, R * 0.025f));

            // ---- 円盤の手前半分（穴の前を横切る）----
            DrawDisk(dc, brush, ctr, R, D, squash, t * spin, bright, back: false);
        }
        finally
        {
            dc.Transform = prev;
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    void DrawDisk(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float R, float D, float squash, float phase, float bright, bool back)
    {
        float inner = R * 1.45f;
        const int bands = 26;
        float a0 = back ? MathF.PI : 0f, a1 = back ? MathF.PI * 2f : MathF.PI;
        const int seg = 48;
        for (int b = 0; b < bands; b++)
        {
            float u = b / (bands - 1f);
            float rr = inner + (D - inner) * MathF.Pow(u, 1.3f);
            float bandW = (D - inner) / bands * 1.8f;
            var col = Gfx.Mix(InnerColor, DiskColor, MathF.Pow(u, 0.7f));
            float baseA = (1f - u) * 0.55f * bright * (0.7f + 0.3f * Gfx.Hash01(b * 13 + Seed));
            float omega = 1.6f * MathF.Pow(inner / rr, 1.5f);
            for (int i = 0; i < seg; i++)
            {
                float aa = a0 + (a1 - a0) * i / seg, ab = a0 + (a1 - a0) * (i + 1) / seg;
                // ドップラー効果：近づく側（左）が明るい
                float dop = 1f + 0.55f * MathF.Cos(aa + MathF.PI);
                // 回転する濃淡（筋）
                float streak = 0.65f + 0.35f * MathF.Sin(aa * 3f - phase * omega * 4f + b * 1.7f) * MathF.Sin(aa * 7f + phase * omega * 2.5f + b);
                brush.Color = Gfx.C4(col, Math.Clamp(baseA * dop * streak, 0f, 1f));
                var pa = ctr + new Vector2(MathF.Cos(aa) * rr, MathF.Sin(aa) * rr * squash);
                var pb = ctr + new Vector2(MathF.Cos(ab) * rr, MathF.Sin(ab) * rr * squash);
                dc.DrawLine(pa, pb, brush, bandW);
            }
        }
        // 円盤の上を回る明るい塊
        var rng = new Random(Seed * 31 + (back ? 1 : 2));
        for (int i = 0; i < 70; i++)
        {
            float rr = inner + (D - inner) * MathF.Pow(rng.NextF(), 1.5f);
            float omega = 1.6f * MathF.Pow(inner / rr, 1.5f);
            float ang = rng.NextF() * MathF.PI * 2f + phase * omega;
            float s = MathF.Sin(ang);
            if (back ? s > 0f : s < 0f) continue;
            var p = ctr + new Vector2(MathF.Cos(ang) * rr, s * rr * squash);
            float dop = 1f + 0.55f * MathF.Cos(ang + MathF.PI);
            DrawKit.Glow(dc, p, 6f + 10f * rng.NextF(), Gfx.C4(InnerColor, 0.35f * dop * bright));
        }
    }
}
