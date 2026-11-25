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
//  水中の魚の群れ
//   光の差す水中を泳ぐ魚群と、くらげ
// =====================================================================
public sealed class FishSchoolPlugin : IShapePlugin
{
    public string Name => "演出：水中の魚の群れ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FishSchoolParameter(sharedData);
}

public sealed class FishSchoolParameter : EffectShapeParameterBase
{
    public FishSchoolParameter() : this(null) { }
    public FishSchoolParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "魚", Name = "魚の色")]
    [ColorPicker]
    public WpfColor FishColor { get => fishColor; set => Set(ref fishColor, value); }
    WpfColor fishColor = WpfColor.FromRgb(0xA8, 0xD8, 0xF0);

    [Display(GroupName = "魚", Name = "群れの数")]
    [AnimationSlider("F0", "群れ", 0, 6)]
    public Animation Schools { get; } = new Animation(2, 0, 30);

    [Display(GroupName = "魚", Name = "1つの群れの魚の数")]
    [AnimationSlider("F0", "匹", 1, 120)]
    public Animation Count { get; } = new Animation(36, 1, 1000);

    [Display(GroupName = "魚", Name = "魚の大きさ")]
    [AnimationSlider("F0", "px", 6, 120)]
    public Animation Size { get; } = new Animation(28, 1, 1000);

    [Display(GroupName = "魚", Name = "泳ぐ速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "くらげ", Name = "くらげの数")]
    [AnimationSlider("F0", "匹", 0, 20)]
    public Animation JellyCount { get; } = new Animation(5, 0, 200);

    [Display(GroupName = "くらげ", Name = "くらげの色")]
    [ColorPicker]
    public WpfColor JellyColor { get => jellyColor; set => Set(ref jellyColor, value); }
    WpfColor jellyColor = WpfColor.FromRgb(0xFF, 0x9A, 0xE0);

    [Display(GroupName = "くらげ", Name = "くらげの大きさ")]
    [AnimationSlider("F0", "px", 10, 200)]
    public Animation JellySize { get; } = new Animation(60, 2, 2000);

    [Display(GroupName = "水", Name = "差しこむ光を表示する")]
    [ToggleSlider]
    public bool Rays { get => rays; set => Set(ref rays, value); }
    bool rays = true;

    [Display(GroupName = "水", Name = "泡を表示する")]
    [ToggleSlider]
    public bool Bubbles { get => bubbles; set => Set(ref bubbles, value); }
    bool bubbles = true;

    [Display(GroupName = "水", Name = "水の背景の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BgOpacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "水", Name = "水の色（上）")]
    [ColorPicker]
    public WpfColor WaterTop { get => waterTop; set => Set(ref waterTop, value); }
    WpfColor waterTop = WpfColor.FromRgb(0x0E, 0x6E, 0xA0);

    [Display(GroupName = "水", Name = "水の色（下）")]
    [ColorPicker]
    public WpfColor WaterBottom { get => waterBottom; set => Set(ref waterBottom, value); }
    WpfColor waterBottom = WpfColor.FromRgb(0x02, 0x12, 0x2A);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Schools, Count, Size, Speed, JellyCount, JellySize, BgOpacity];

    static Vector2 SchoolPos(float tt, int s, float w, float h, int seed)
    {
        float p1 = DrawKit.H(seed, s, 1) * 6.28f, p2 = DrawKit.H(seed, s, 2) * 6.28f;
        float f1 = 0.09f + 0.05f * DrawKit.H(seed, s, 3), f2 = 0.13f + 0.06f * DrawKit.H(seed, s, 4);
        return new Vector2(w * 0.5f + w * 0.42f * MathF.Sin(tt * f1 + p1), h * (0.3f + 0.4f * DrawKit.H(seed, s, 5)) + h * 0.2f * MathF.Sin(tt * f2 + p2));
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time, tt = c.Time * c.F(Speed) / 100f;
        float size = c.F(Size), bg = c.F(BgOpacity) / 100f, js = c.F(JellySize);
        int schools = (int)c.F(Schools), count = (int)c.F(Count), jellies = (int)c.F(JellyCount);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (bg > 0f)
            {
                using var water = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, h), (0f, Gfx.C4(WaterTop, bg)), (1f, Gfx.C4(WaterBottom, bg)));
                dc.FillRectangle(new RawRectF(0, 0, w, h), water);
            }

            dc.PrimitiveBlend = PrimitiveBlend.Add;
            if (Rays)
            {
                for (int i = 0; i < 7; i++)
                {
                    float x = w * (0.1f + 0.8f * DrawKit.H(Seed, i, 20));
                    float sway = MathF.Sin(t * 0.3f + i * 1.7f) * 40f;
                    float a = 0.08f + 0.06f * MathF.Sin(t * 0.7f + i * 2.3f);
                    Gfx.FadeLine(dc, brush, new Vector2(x + sway, -20f), new Vector2(x + sway + h * 0.35f, h * 0.95f), 60f + 60f * DrawKit.H(Seed, i, 21), 140f, white, a, 14, 1.3f);
                }
            }

            // ---- くらげ（奥）----
            for (int i = 0; i < jellies; i++)
            {
                float s = js * (0.6f + 0.6f * DrawKit.H(Seed, i, 30));
                float y = h + s * 3f - DrawKit.Frac(tt * 0.018f * (0.6f + 0.6f * DrawKit.H(Seed, i, 31)) + DrawKit.H(Seed, i, 32)) * (h + s * 6f);
                float x = DrawKit.H(Seed, i, 33) * w + MathF.Sin(t * 0.3f + i) * 30f;
                float pulse = 0.5f + 0.5f * MathF.Sin(t * 2.2f + i * 1.3f);
                var top = new Vector2(x, y);
                float rx = s * (0.9f + 0.15f * pulse), ry = s * (0.7f - 0.15f * pulse);
                var dome = DrawKit.EllipsePts(top, rx, ry, 32, MathF.PI, MathF.PI * 2f);
                dome.Add(top + new Vector2(rx, 0));
                for (int k = 0; k <= 8; k++)
                    dome.Add(top + new Vector2(rx - 2f * rx * k / 8f, ry * 0.12f * MathF.Sin(k * 1.6f + t * 3f)));
                DrawKit.Glow(dc, top - new Vector2(0, ry * 0.4f), s * 1.6f, Gfx.C4(JellyColor, 0.15f));
                brush.Color = Gfx.C4(JellyColor, 0.28f);
                Gfx.FillPoly(dc, brush, dome);
                brush.Color = Gfx.C4(Gfx.Mix(JellyColor, white, 0.5f), 0.5f);
                Gfx.Polyline(dc, brush, DrawKit.EllipsePts(top, rx, ry, 32, MathF.PI, MathF.PI * 2f), 1.5f);
                for (int k = 0; k < 6; k++)
                {
                    float bx = x - rx * 0.75f + rx * 1.5f * k / 5f;
                    var tent = new List<Vector2>();
                    for (int q = 0; q <= 12; q++)
                    {
                        float u = q / 12f;
                        tent.Add(new Vector2(bx + MathF.Sin(t * 2f + u * 5f + k) * s * 0.18f * u, y + s * 2.4f * u * (0.8f + 0.2f * pulse)));
                    }
                    brush.Color = Gfx.C4(JellyColor, 0.25f);
                    Gfx.Polyline(dc, brush, tent, 1.6f);
                }
            }
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

            // ---- 魚の群れ ----
            for (int s = 0; s < schools; s++)
            {
                for (int i = 0; i < count; i++)
                {
                    int id = s * 1000 + i;
                    float lag = DrawKit.H(Seed, id, 40) * 2.5f;
                    var cp = SchoolPos(tt - lag, s, w, h, Seed);
                    var cn = SchoolPos(tt - lag + 0.05f, s, w, h, Seed);
                    var vel = cn - cp;
                    float heading = MathF.Atan2(vel.Y, vel.X);
                    var off = new Vector2((DrawKit.H(Seed, id, 41) - 0.5f) * size * 9f, (DrawKit.H(Seed, id, 42) - 0.5f) * size * 5f);
                    off += new Vector2(MathF.Sin(t * 1.3f + id), MathF.Cos(t * 1.1f + id * 0.7f)) * size * 0.4f;
                    var p = cp + off;
                    float fs = size * (0.75f + 0.5f * DrawKit.H(Seed, id, 43));
                    float flap = MathF.Sin(t * 12f * (0.8f + 0.4f * DrawKit.H(Seed, id, 44)) + id);
                    float depth = 0.6f + 0.4f * DrawKit.H(Seed, id, 45);
                    DrawFish(dc, brush, p, heading, fs, flap, Gfx.Mix(WaterBottom, FishColor, depth), bg);
                }
            }

            // ---- 泡 ----
            if (Bubbles)
            {
                var rng = new Random(Seed + 9);
                for (int i = 0; i < 40; i++)
                {
                    float bx = rng.NextF() * w, sp = 40f + 70f * rng.NextF(), r = 2f + 5f * rng.NextF(), off = rng.NextF();
                    float y = h + 20f - DrawKit.Frac(t * sp / (h + 40f) + off) * (h + 40f);
                    var p = new Vector2(bx + MathF.Sin(t * 2f + i) * 6f, y);
                    brush.Color = Gfx.C4(white, 0.4f);
                    dc.DrawEllipse(new Ellipse(p, r, r), brush, 1f);
                    brush.Color = Gfx.C4(white, 0.55f);
                    dc.FillEllipse(new Ellipse(p + new Vector2(-r * 0.35f, -r * 0.35f), r * 0.25f, r * 0.25f), brush);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    static void DrawFish(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 p, float heading, float L, float flap, WpfColor col, float bg)
    {
        // 体（頭が +X 向き）。左へ泳ぐ時は上下を反転して、おなかを下に保つ
        float sy = MathF.Abs(heading) > MathF.PI / 2f ? -1f : 1f;
        var body = new List<Vector2>();
        for (int i = 0; i <= 16; i++)
        {
            float a = i / 16f * MathF.PI * 2f;
            float x = MathF.Cos(a) * L * 0.5f;
            float y = MathF.Sin(a) * L * 0.2f * (x > 0 ? 1f : 0.75f + 0.25f * (1f + x / (L * 0.5f)));
            body.Add(new Vector2(x, y));
        }
        var tail = new List<Vector2>
        {
            new(-L * 0.42f, 0f),
            new(-L * 0.78f, -L * 0.2f + flap * L * 0.08f),
            new(-L * 0.68f, flap * L * 0.06f),
            new(-L * 0.78f, L * 0.2f + flap * L * 0.08f),
        };
        brush.Color = Gfx.C4(col, 0.92f);
        Gfx.FillPoly(dc, brush, DrawKit.Place(tail, p, heading, 1f, sy));
        Gfx.FillPoly(dc, brush, DrawKit.Place(body, p, heading, 1f, sy));
        // 背中の影と腹の光
        brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.5f), 0.6f);
        var belly = DrawKit.Place(new[] { new Vector2(L * 0.35f, L * 0.04f), new Vector2(-L * 0.3f, L * 0.06f) }, p, heading, 1f, sy);
        dc.DrawLine(belly[0], belly[1], brush, MathF.Max(1f, L * 0.05f));
        var eye = DrawKit.Place(new[] { new Vector2(L * 0.32f, -L * 0.04f) }, p, heading, 1f, sy)[0];
        brush.Color = Gfx.C4(0.02f, 0.03f, 0.06f, 0.9f);
        dc.FillEllipse(new Ellipse(eye, L * 0.035f + 0.6f, L * 0.035f + 0.6f), brush);
    }
}
