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
//  雨上がりの水たまり
//   地面の水たまりに、街のネオンや明かりがゆれて映る。ときどき、しずくの波紋
// =====================================================================
public sealed class PuddlesPlugin : IShapePlugin
{
    public string Name => "演出：雨上がりの水たまり";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PuddlesParameter(sharedData);
}

public sealed class PuddlesParameter : EffectShapeParameterBase
{
    public PuddlesParameter() : this(null) { }
    public PuddlesParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "地面", Name = "地面の始まり（Y）")]
    [AnimationSlider("F0", "%", 30, 95)]
    public Animation GroundY { get; } = new Animation(62, 0, 100);

    [Display(GroupName = "地面", Name = "地面を塗る")]
    [ToggleSlider]
    public bool PaintGround { get => paintGround; set => Set(ref paintGround, value); }
    bool paintGround = true;

    [Display(GroupName = "地面", Name = "地面の色")]
    [ColorPicker]
    public WpfColor GroundColor { get => groundColor; set => Set(ref groundColor, value); }
    WpfColor groundColor = WpfColor.FromRgb(0x18, 0x1A, 0x22);

    [Display(GroupName = "水たまり", Name = "水たまりの数")]
    [TextBoxSlider("F0", "個", 1, 20)]
    [Range(1, 80)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 6;

    [Display(GroupName = "水たまり", Name = "映る光の色1")]
    [ColorPicker]
    public WpfColor Light1 { get => light1; set => Set(ref light1, value); }
    WpfColor light1 = WpfColor.FromRgb(0xFF, 0x3D, 0x9A);

    [Display(GroupName = "水たまり", Name = "映る光の色2")]
    [ColorPicker]
    public WpfColor Light2 { get => light2; set => Set(ref light2, value); }
    WpfColor light2 = WpfColor.FromRgb(0x3D, 0xD8, 0xFF);

    [Display(GroupName = "水たまり", Name = "映る光の色3")]
    [ColorPicker]
    public WpfColor Light3 { get => light3; set => Set(ref light3, value); }
    WpfColor light3 = WpfColor.FromRgb(0xFF, 0xC8, 0x5A);

    [Display(GroupName = "水たまり", Name = "しずくの波紋（1秒あたり）")]
    [AnimationSlider("F1", "回", 0, 10)]
    public Animation Drips { get; } = new Animation(1.5, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [GroundY, Drips];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float gy = c.F(GroundY) / 100f * h;
        float u = MathF.Min(w / 1920f, h / 1080f);
        using var brush = dc.CreateSolidColorBrush(default);
        if (PaintGround)
        {
            using var g = Gfx.Linear(dc, new Vector2(0, gy), new Vector2(0, h), (0f, Gfx.C4(Gfx.Mix(GroundColor, WpfColor.FromRgb(0, 0, 0), 0.3f), 1f)), (1f, Gfx.C4(GroundColor, 1f)));
            dc.FillRectangle(new RawRectF(0, gy, w, h), g);
        }
        var lights = new[] { Light1, Light2, Light3 };
        var puddles = new List<(Vector2 c, float rx, float ry)>();
        for (int i = 0; i < Count; i++)
        {
            // 手前ほど大きく、平たい楕円
            float f = DrawKit.H(Seed, i, 1);
            float y = gy + (h - gy) * (0.15f + f * 0.8f);
            float persp = (y - gy) / MathF.Max(1f, h - gy);
            float rx = (120f + DrawKit.H(Seed, i, 2) * 260f) * u * (0.4f + persp);
            float ry = rx * (0.12f + persp * 0.12f);
            puddles.Add((new Vector2(DrawKit.H(Seed, i, 3) * w, y), rx, ry));
        }
        foreach (var (pc, rx, ry) in puddles)
        {
            // 水たまりの形(少しいびつ)
            var shape = new List<Vector2>();
            for (int j = 0; j < 40; j++)
            {
                float a = j / 40f * MathF.PI * 2f;
                float k = 1f + 0.12f * MathF.Sin(a * 3f + pc.X * 0.01f) + 0.06f * MathF.Sin(a * 7f + pc.Y * 0.02f);
                shape.Add(pc + new Vector2(MathF.Cos(a) * rx * k, MathF.Sin(a) * ry * k));
            }
            brush.Color = Gfx.C4(0.02f, 0.03f, 0.06f, 0.9f);
            Gfx.FillPoly(dc, brush, shape);
            // 映り込む光: 縦長の帯が、ゆらゆら
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            for (int j = 0; j < 7; j++)
            {
                float lx = pc.X + (DrawKit.H((int)pc.X, j, 4) - 0.5f) * rx * 1.6f;
                var col = lights[j % 3];
                float bw = rx * (0.04f + DrawKit.H((int)pc.X, j, 5) * 0.08f);
                for (int s = 0; s < 6; s++)
                {
                    float yy = pc.Y - ry * 0.8f + s * ry * 0.32f;
                    float wob = MathF.Sin(t * 2.5f + s * 1.3f + j) * bw * 0.6f;
                    brush.Color = Gfx.C4(col, 0.35f * (1f - s / 6f));
                    var seg = new List<Vector2> { new(lx - bw / 2f + wob, yy), new(lx + bw / 2f + wob, yy), new(lx + bw / 2f - wob, yy + ry * 0.28f), new(lx - bw / 2f - wob, yy + ry * 0.28f) };
                    Gfx.FillPoly(dc, brush, DrawKit.ClipRect(seg, pc.X - rx, pc.Y - ry, pc.X + rx, pc.Y + ry).Where(q => ((q.X - pc.X) * (q.X - pc.X)) / (rx * rx) + ((q.Y - pc.Y) * (q.Y - pc.Y)) / (ry * ry) <= 1.3f).ToList());
                }
            }
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            // ふちの光
            brush.Color = Gfx.C4(0.7f, 0.75f, 0.9f, 0.25f);
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(shape), MathF.Max(1f, 2f * u));
        }
        // しずくの波紋
        float rate = c.F(Drips);
        if (rate > 0.001f && puddles.Count > 0)
        {
            int first = Math.Max(0, (int)MathF.Floor((t - 1.2f) * rate));
            int last = (int)MathF.Floor(t * rate);
            for (int i = first; i <= last; i++)
            {
                float born = i / rate;
                float age = t - born;
                if (age < 0f || age > 1.2f) continue;
                var (pc, rx, ry) = puddles[(int)(DrawKit.H(Seed, i, 6) * puddles.Count) % puddles.Count];
                var p = pc + new Vector2((DrawKit.H(Seed, i, 7) - 0.5f) * rx * 1.2f, (DrawKit.H(Seed, i, 8) - 0.5f) * ry * 1.2f);
                for (int r = 0; r < 2; r++)
                {
                    float f = Math.Clamp((age - r * 0.15f) / 1f, 0f, 1f);
                    if (f <= 0f) continue;
                    brush.Color = Gfx.C4(0.8f, 0.85f, 1f, 0.6f * (1f - f));
                    dc.DrawEllipse(new Ellipse(p, rx * 0.25f * f, ry * 0.25f * f), brush, MathF.Max(1f, 2.5f * u * (1f - f)));
                }
            }
        }
    }
}
