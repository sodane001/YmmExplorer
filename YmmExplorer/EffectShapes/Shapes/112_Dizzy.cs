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

public enum DizzyKind
{
    [Display(Name = "星")] Stars,
    [Display(Name = "ひよこ")] Chicks,
    [Display(Name = "星とひよこ")] Both,
}

// =====================================================================
//  気絶の星
//   頭の上を星やひよこがくるくる回る
// =====================================================================
public sealed class DizzyPlugin : IShapePlugin
{
    public string Name => "演出：気絶の星";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DizzyParameter(sharedData);
}

public sealed class DizzyParameter : EffectShapeParameterBase
{
    public DizzyParameter() : this(null) { }
    public DizzyParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "回るもの")]
    [EnumComboBox]
    public DizzyKind Kind { get => kind; set => Set(ref kind, value); }
    DizzyKind kind = DizzyKind.Both;

    [Display(GroupName = "種類", Name = "星の色")]
    [ColorPicker]
    public WpfColor StarColor { get => starColor; set => Set(ref starColor, value); }
    WpfColor starColor = WpfColor.FromRgb(0xFF, 0xDC, 0x3C);

    [Display(GroupName = "位置と大きさ", Name = "頭のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "頭のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(30, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "回る輪の半径")]
    [AnimationSlider("F0", "px", 30, 400)]
    public Animation Radius { get; } = new Animation(130, 2, 3000);

    [Display(GroupName = "位置と大きさ", Name = "1つの大きさ")]
    [AnimationSlider("F0", "px", 8, 100)]
    public Animation ItemSize { get; } = new Animation(30, 2, 1000);

    [Display(GroupName = "動き", Name = "数")]
    [AnimationSlider("F0", "個", 1, 10)]
    public Animation Count { get; } = new Animation(5, 1, 40);

    [Display(GroupName = "動き", Name = "回る速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, -2000, 2000);

    [Display(GroupName = "動き", Name = "輪の線を表示する")]
    [ToggleSlider]
    public bool Orbit { get => orbit; set => Set(ref orbit, value); }
    bool orbit = false;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, ItemSize, Count, Speed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), S = c.F(ItemSize), sp = c.F(Speed) / 100f;
        int n = Math.Max(1, (int)c.F(Count));
        const float flat = 0.32f;

        using var brush = dc.CreateSolidColorBrush(default);
        if (Orbit)
        {
            brush.Color = Gfx.C4(1f, 1f, 1f, 0.35f);
            dc.DrawEllipse(new Ellipse(ctr, R, R * flat), brush, 2f);
        }
        var items = new List<(float z, Vector2 p, int i)>();
        for (int i = 0; i < n; i++)
        {
            float a = t * 3.2f * sp + i * MathF.PI * 2f / n;
            var p = ctr + new Vector2(MathF.Cos(a) * R, MathF.Sin(a) * R * flat + MathF.Sin(t * 6f + i) * S * 0.15f);
            items.Add((MathF.Sin(a), p, i));
        }
        items.Sort((x, y) => x.z.CompareTo(y.z));
        foreach (var (z, p, i) in items)
        {
            float depth = 0.7f + 0.3f * z;
            float s = S * depth;
            bool chick = Kind == DizzyKind.Chicks || (Kind == DizzyKind.Both && i % 2 == 1);
            if (chick) DrawChick(dc, brush, p, s, t * 8f + i, depth);
            else
            {
                float rot = t * 4f + i;
                brush.Color = Gfx.C4(Gfx.Mix(StarColor, WpfColor.FromRgb(120, 70, 0), 0.6f), 1f);
                Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(p, s * 1.12f, 5, 0.48f, rot));
                brush.Color = Gfx.C4(Gfx.Mix(StarColor, WpfColor.FromRgb(20, 20, 40), 0.3f * (1f - depth)), 1f);
                Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(p, s, 5, 0.48f, rot));
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.6f * depth);
                dc.FillEllipse(new Ellipse(p + new Vector2(-s * 0.15f, -s * 0.2f), s * 0.12f, s * 0.12f), brush);
            }
        }
    }

    static void DrawChick(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 p, float s, float ph, float depth)
    {
        var body = WpfColor.FromRgb(0xFF, 0xE0, 0x4A);
        var shade = Gfx.Mix(body, WpfColor.FromRgb(20, 20, 40), 0.3f * (1f - depth));
        brush.Color = Gfx.C4(Gfx.Mix(shade, WpfColor.FromRgb(150, 90, 0), 0.4f), 1f);
        dc.FillEllipse(new Ellipse(p, s * 0.95f, s * 0.8f), brush);
        brush.Color = Gfx.C4(shade, 1f);
        dc.FillEllipse(new Ellipse(p, s * 0.85f, s * 0.7f), brush);
        // 頭のてっぺんの毛
        dc.DrawLine(p + new Vector2(0, -s * 0.65f), p + new Vector2(s * 0.1f, -s * 0.95f), brush, s * 0.12f);
        // 羽（ぱたぱた）
        float flap = MathF.Sin(ph) * 0.4f;
        var wing = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, s * 0.32f, s * 0.18f, 16), p + new Vector2(-s * 0.55f, s * 0.1f), -0.5f + flap);
        brush.Color = Gfx.C4(Gfx.Mix(shade, WpfColor.FromRgb(200, 130, 0), 0.3f), 1f);
        Gfx.FillPoly(dc, brush, wing);
        // くちばしと目（目は「＠」のぐるぐる）
        brush.Color = Gfx.C4(1f, 0.55f, 0.1f, 1f);
        Gfx.FillPoly(dc, brush, new List<Vector2> { p + new Vector2(s * 0.55f, -s * 0.05f), p + new Vector2(s * 0.95f, s * 0.05f), p + new Vector2(s * 0.55f, s * 0.15f) });
        brush.Color = Gfx.C4(0.1f, 0.08f, 0.05f, 1f);
        var eye = p + new Vector2(s * 0.3f, -s * 0.2f);
        var spiral = new List<Vector2>();
        for (int k = 0; k <= 16; k++)
        {
            float a = k / 16f * MathF.PI * 3f + ph;
            float r = s * 0.13f * k / 16f;
            spiral.Add(eye + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r);
        }
        Gfx.Polyline(dc, brush, spiral, MathF.Max(1f, s * 0.04f));
    }
}
