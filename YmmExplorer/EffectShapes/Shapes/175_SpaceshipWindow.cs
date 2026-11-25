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
//  宇宙船の窓
//   丸い窓枠(ボルトつき)の向こうを、星が流れていく。ときどき惑星が横切る。
//   窓の外側は、船内の壁で埋める(オフにすれば窓枠だけ)
// =====================================================================
public sealed class SpaceshipWindowPlugin : IShapePlugin
{
    public string Name => "演出：宇宙船の窓";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpaceshipWindowParameter(sharedData);
}

public sealed class SpaceshipWindowParameter : EffectShapeParameterBase
{
    public SpaceshipWindowParameter() : this(null) { }
    public SpaceshipWindowParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "窓", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "窓", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(48, -100, 200);

    [Display(GroupName = "窓", Name = "窓の半径")]
    [AnimationSlider("F0", "px", 100, 700)]
    public Animation Radius { get; } = new Animation(400, 10, 5000);

    [Display(GroupName = "窓", Name = "船内の壁で埋める")]
    [ToggleSlider]
    public bool Wall { get => wall; set => Set(ref wall, value); }
    bool wall = true;

    [Display(GroupName = "窓", Name = "壁の色")]
    [ColorPicker]
    public WpfColor WallColor { get => wallColor; set => Set(ref wallColor, value); }
    WpfColor wallColor = WpfColor.FromRgb(0x3A, 0x40, 0x4C);

    [Display(GroupName = "外の景色", Name = "星の流れる速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Speed { get; } = new Animation(100, 0, 5000);

    [Display(GroupName = "外の景色", Name = "星の数")]
    [TextBoxSlider("F0", "個", 20, 600)]
    [Range(0, 3000)]
    public int StarCount { get => starCount; set => Set(ref starCount, value); }
    int starCount = 220;

    [Display(GroupName = "外の景色", Name = "惑星が横切る")]
    [ToggleSlider]
    public bool Planet { get => planet; set => Set(ref planet, value); }
    bool planet = true;

    [Display(GroupName = "外の景色", Name = "惑星の色")]
    [ColorPicker]
    public WpfColor PlanetColor { get => planetColor; set => Set(ref planetColor, value); }
    WpfColor planetColor = WpfColor.FromRgb(0xE0, 0x8A, 0x4A);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [PosX, PosY, Radius, Speed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), sp = c.F(Speed) / 100f;
        using var brush = dc.CreateSolidColorBrush(default);
        // 窓の中: 宇宙
        var glass = DrawKit.EllipsePts(ctr, R, R, 96);
        using (var g = Gfx.Radial(dc, ctr, R, (0f, Gfx.C4(0.05f, 0.06f, 0.14f, 1f)), (1f, Gfx.C4(0.01f, 0.01f, 0.04f, 1f))))
            dc.FillEllipse(new Ellipse(ctr, R, R), g);
        dc.PushAxisAlignedClip(new RawRectF(ctr.X - R, ctr.Y - R, ctr.X + R, ctr.Y + R), AntialiasMode.PerPrimitive);
        // 星(手前ほど速く・長く)
        for (int i = 0; i < StarCount; i++)
        {
            float depth = DrawKit.H(Seed, i, 1);
            float v = (40f + depth * depth * 600f) * sp;
            float x = Gfx.Mod(DrawKit.H(Seed, i, 2) * R * 2.4f - t * v, R * 2.4f) - R * 1.2f;
            float y = (DrawKit.H(Seed, i, 3) - 0.5f) * R * 2f;
            var p = ctr + new Vector2(x, y);
            if (Vector2.DistanceSquared(p, ctr) > R * R) continue;
            float len = MathF.Min(R * 0.3f, v * 0.06f);
            brush.Color = Gfx.C4(0.85f + depth * 0.15f, 0.9f, 1f, 0.3f + depth * 0.7f);
            if (len > 2f) dc.DrawLine(p, p + new Vector2(len, 0f), brush, 1f + depth * 1.6f);
            else dc.FillEllipse(new Ellipse(p, 0.8f + depth, 0.8f + depth), brush);
        }
        // 惑星(ゆっくり横切る)
        if (Planet)
        {
            float period = 40f / MathF.Max(0.05f, sp);
            float f = Gfx.Mod(t / period + 0.35f, 1f);
            var pc = ctr + new Vector2(R * 1.8f - f * R * 3.6f, R * 0.35f);
            float pr = R * 0.45f;
            using (var g = Gfx.Radial(dc, pc - new Vector2(pr * 0.4f, pr * 0.4f), pr * 1.6f, (0f, Gfx.C4(Gfx.Mix(PlanetColor, WpfColor.FromRgb(255, 255, 255), 0.3f), 1f)), (0.6f, Gfx.C4(PlanetColor, 1f)), (1f, Gfx.C4(Gfx.Mix(PlanetColor, WpfColor.FromRgb(0, 0, 0), 0.85f), 1f))))
                dc.FillEllipse(new Ellipse(pc, pr, pr), g);
            brush.Color = Gfx.C4(Gfx.Mix(PlanetColor, WpfColor.FromRgb(255, 255, 255), 0.4f), 0.6f);
            dc.DrawEllipse(new Ellipse(pc, pr * 1.7f, pr * 0.35f), brush, pr * 0.05f);
        }
        dc.PopAxisAlignedClip();
        // ガラスの反射
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.08f);
        Gfx.FillPoly(dc, brush, DrawKit.EllipsePts(ctr + new Vector2(-R * 0.25f, -R * 0.3f), R * 0.55f, R * 0.25f, 40).Select(q => DrawKit.Rot(q, ctr, -0.6f)).ToList());

        // 船内の壁(窓のまわり)
        float ring = R * 0.14f;
        if (Wall)
        {
            using var g = Gfx.Linear(dc, new Vector2(0, 0), new Vector2(0, h), (0f, Gfx.C4(Gfx.Mix(WallColor, WpfColor.FromRgb(255, 255, 255), 0.1f), 1f)), (1f, Gfx.C4(Gfx.Mix(WallColor, WpfColor.FromRgb(0, 0, 0), 0.3f), 1f)));
            DrawKit.FillWithHole(dc, g, DrawKit.RectPts(-2f, -2f, w + 4f, h + 4f), DrawKit.EllipsePts(ctr, R + ring * 0.5f, R + ring * 0.5f, 96));
            // 壁のパネルの継ぎ目
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f);
            for (int i = 1; i < 4; i++) dc.DrawLine(new Vector2(w * i / 4f, 0), new Vector2(w * i / 4f, h), brush, 3f);
            dc.DrawLine(new Vector2(0, h * 0.85f), new Vector2(w, h * 0.85f), brush, 3f);
        }
        // 窓枠(金属の輪とボルト)
        using (var g = Gfx.Linear(dc, ctr - new Vector2(R, R), ctr + new Vector2(R, R), (0f, Gfx.C4(0.82f, 0.84f, 0.88f, 1f)), (0.5f, Gfx.C4(0.45f, 0.48f, 0.54f, 1f)), (1f, Gfx.C4(0.7f, 0.72f, 0.78f, 1f))))
            dc.DrawEllipse(new Ellipse(ctr, R + ring * 0.5f, R + ring * 0.5f), g, ring);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.5f);
        dc.DrawEllipse(new Ellipse(ctr, R, R), brush, 3f);
        for (int i = 0; i < 12; i++)
        {
            float a = i / 12f * MathF.PI * 2f;
            var bp = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (R + ring * 0.5f);
            brush.Color = Gfx.C4(0.3f, 0.32f, 0.36f, 1f);
            dc.FillEllipse(new Ellipse(bp, ring * 0.16f, ring * 0.16f), brush);
            brush.Color = Gfx.C4(0.9f, 0.92f, 0.95f, 1f);
            dc.FillEllipse(new Ellipse(bp - new Vector2(ring * 0.04f, ring * 0.04f), ring * 0.07f, ring * 0.07f), brush);
        }
        _ = glass;
    }
}
