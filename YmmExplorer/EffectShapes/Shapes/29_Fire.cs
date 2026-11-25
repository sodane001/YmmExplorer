using System.ComponentModel.DataAnnotations;
using System.Numerics;
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
//  炎
//   加算合成の粒で作る炎 + 火の粉 + 足元の照り返し
//   「一か所」または「画面の一辺」を覆う炎にできる
// =====================================================================
public enum FireEdge
{
    [Display(Name = "一か所（根元の位置を指定）")] Spot,
    [Display(Name = "画面の下の辺")] Bottom,
    [Display(Name = "画面の上の辺")] Top,
    [Display(Name = "画面の左の辺")] Left,
    [Display(Name = "画面の右の辺")] Right,
}

public sealed class FirePlugin : IShapePlugin
{
    public string Name => "演出：炎";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FireParameter(sharedData);
}

public sealed class FireParameter : EffectShapeParameterBase
{
    public FireParameter() : this(null) { }
    public FireParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "配置", Name = "炎を出す場所", Description = "画面の一辺を選ぶと、その辺全体を覆う炎になる")]
    [EnumComboBox]
    public FireEdge Edge { get => edge; set => Set(ref edge, value); }
    FireEdge edge = FireEdge.Spot;

    [Display(GroupName = "配置", Name = "辺からのずれ", Description = "辺を選んだとき。マイナスで画面の外側へ（根元を隠せる）")]
    [AnimationSlider("F0", "px", -100, 100)]
    public Animation EdgeOffset { get; } = new Animation(-20, -2000, 2000);

    [Display(GroupName = "配置", Name = "炎の根元X（一か所のとき）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "配置", Name = "炎の根元Y（一か所のとき）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(85, -100, 200);

    [Display(GroupName = "炎", Name = "外炎の色")]
    [ColorPicker]
    public WpfColor OuterColor { get => outer; set => Set(ref outer, value); }
    WpfColor outer = WpfColor.FromRgb(0xFF, 0x46, 0x10);

    [Display(GroupName = "炎", Name = "内炎の色")]
    [ColorPicker]
    public WpfColor InnerColor { get => inner; set => Set(ref inner, value); }
    WpfColor inner = WpfColor.FromRgb(0xFF, 0xF0, 0x9A);

    [Display(GroupName = "炎", Name = "幅（一か所のとき）")]
    [AnimationSlider("F0", "px", 20, 800)]
    public Animation FlameWidth { get; } = new Animation(220, 1, 5000);

    [Display(GroupName = "炎", Name = "高さ（辺から燃え上がる長さ）")]
    [AnimationSlider("F0", "px", 50, 1000)]
    public Animation FlameHeight { get; } = new Animation(420, 1, 8000);

    [Display(GroupName = "炎", Name = "粒の数", Description = "辺を覆うときは、辺の長さに応じて自動で増える（上限あり）")]
    [AnimationSlider("F0", "個", 20, 400)]
    public Animation Count { get; } = new Animation(140, 1, 3000);

    [Display(GroupName = "炎", Name = "粒の大きさ")]
    [AnimationSlider("F0", "px", 10, 200)]
    public Animation Size { get; } = new Animation(70, 1, 2000);

    [Display(GroupName = "炎", Name = "揺らぎ")]
    [AnimationSlider("F0", "px", 0, 150)]
    public Animation Sway { get; } = new Animation(30, 0, 1000);

    [Display(GroupName = "炎", Name = "勢い")]
    [AnimationSlider("F0", "%", 20, 300)]
    public Animation Speed { get; } = new Animation(100, 1, 2000);

    [Display(GroupName = "炎", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "火の粉", Name = "数")]
    [AnimationSlider("F0", "個", 0, 200)]
    public Animation SparkCount { get; } = new Animation(40, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [EdgeOffset, PosX, PosY, FlameWidth, FlameHeight, Count, Size, Sway, Speed, Brightness, SparkCount];

    // 辺を覆う炎では粒が多くなるため、グラデーションを使わない軽い柔らかい粒で描く
    static void SoftBlob(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 p, float r, WpfColor col, float a)
    {
        brush.Color = Gfx.C4(col, a * 0.22f);
        dc.FillEllipse(new Ellipse(p, r, r), brush);
        brush.Color = Gfx.C4(col, a * 0.32f);
        dc.FillEllipse(new Ellipse(p, r * 0.66f, r * 0.66f), brush);
        brush.Color = Gfx.C4(col, a * 0.45f);
        dc.FillEllipse(new Ellipse(p, r * 0.36f, r * 0.36f), brush);
    }

    static void Blob(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, bool wall, Vector2 p, float r, WpfColor col, float a)
    {
        if (wall) { SoftBlob(dc, brush, p, r, col, a); return; }
        using var b = Gfx.Radial(dc, p, r,
            (0f, Gfx.C4(col, a)), (0.5f, Gfx.C4(col, a * 0.5f)), (1f, Gfx.C4(col, 0f)));
        dc.FillEllipse(new Ellipse(p, r, r), b);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float height = c.F(FlameHeight);
        float size = c.F(Size), sway = c.F(Sway), speed = c.F(Speed) / 100f, bright = c.F(Brightness) / 100f;
        int sn = (int)c.F(SparkCount);
        float off = c.F(EdgeOffset);
        var smoke = WpfColor.FromRgb(0x40, 0x10, 0x08);

        // ---- 炎の根元（origin）・燃え上がる向き（dir）・根元に沿う向き（lat）----
        bool wall = Edge != FireEdge.Spot;
        Vector2 origin, dir, lat;
        float half;
        switch (Edge)
        {
            case FireEdge.Bottom: origin = new Vector2(w / 2f, h - off); dir = new Vector2(0, -1); lat = new Vector2(1, 0); half = w / 2f; break;
            case FireEdge.Top:    origin = new Vector2(w / 2f, off);     dir = new Vector2(0, 1);  lat = new Vector2(1, 0); half = w / 2f; break;
            case FireEdge.Left:   origin = new Vector2(off, h / 2f);     dir = new Vector2(1, 0);  lat = new Vector2(0, 1); half = h / 2f; break;
            case FireEdge.Right:  origin = new Vector2(w - off, h / 2f); dir = new Vector2(-1, 0); lat = new Vector2(0, 1); half = h / 2f; break;
            default:
                origin = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
                dir = new Vector2(0, -1); lat = new Vector2(1, 0); half = c.F(FlameWidth) / 2f;
                break;
        }

        int n = (int)c.F(Count);
        if (wall) n = Math.Min(900, (int)(n * (half * 2f) / 500f));
        float sizeEff = wall ? size * 0.9f : size;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 根元の照り返し ----
            if (wall)
            {
                using var gl = Gfx.Linear(dc, origin, origin + dir * (height * 1.1f),
                    (0f, Gfx.C4(OuterColor, 0.35f * bright)), (1f, Gfx.C4(OuterColor, 0f)));
                Gfx.Rect(dc, gl, 0, 0, w, h);
            }
            else
            {
                var gc = origin + dir * (height * 0.2f);
                float gr = height * 1.1f;
                using var gl = Gfx.Radial(dc, gc, gr,
                    (0f, Gfx.C4(OuterColor, 0.35f * bright)),
                    (0.5f, Gfx.C4(OuterColor, 0.12f * bright)),
                    (1f, Gfx.C4(OuterColor, 0f)));
                dc.FillEllipse(new Ellipse(gc, gr, gr), gl);
            }

            // ---- 外炎 ----
            var rng = new Random(Seed);
            for (int i = 0; i < n; i++)
            {
                float ph = rng.NextF(), rx = rng.NextF() * 2f - 1f;
                float sz = 0.5f + 0.5f * rng.NextF(), sp = 0.6f + 0.8f * rng.NextF(), f2 = rng.NextF();
                float u = Gfx.Mod(t * speed * sp * 0.9f + ph, 1f);
                float lx = rx * half;
                float taper = wall ? 1f : MathF.Pow(1f - u, 0.6f);
                // 辺を覆うときは、場所ごとに炎の高さが波打つ（舌のような形）
                float hf = wall ? 0.65f + 0.35f * MathF.Abs(MathF.Sin(lx * 0.012f + t * 1.2f) * MathF.Sin(lx * 0.031f - t * 0.8f + ph * 6f)) : 1f;
                float lateral = lx * taper + MathF.Sin(t * 3f * sp + ph * 6.28f + u * 4f) * sway * u;
                var pos = origin + lat * lateral + dir * (u * height * (0.7f + 0.3f * f2) * hf);
                float r = sizeEff * sz * (1f - u * 0.75f);
                var col = u < 0.5f ? Gfx.Mix(InnerColor, OuterColor, u * 2f) : Gfx.Mix(OuterColor, smoke, (u - 0.5f) * 2f);
                float a = 0.55f * MathF.Pow(1f - u, 1.1f) * bright;
                Blob(dc, brush, wall, pos, r, col, a);
            }

            // ---- 内炎（細く明るい芯）----
            var rc = new Random(Seed + 1);
            for (int i = 0; i < n / 2; i++)
            {
                float ph = rc.NextF(), rx = rc.NextF() * 2f - 1f;
                float sz = 0.5f + 0.5f * rc.NextF(), sp = 0.6f + 0.8f * rc.NextF();
                float u = Gfx.Mod(t * speed * sp * 0.9f + ph, 1f) * 0.45f;
                float lx = rx * half;
                float taper = wall ? 1f : MathF.Pow(1f - u, 0.6f) * 0.25f;
                float hf = wall ? 0.65f + 0.35f * MathF.Abs(MathF.Sin(lx * 0.012f + t * 1.2f) * MathF.Sin(lx * 0.031f - t * 0.8f + ph * 6f)) : 1f;
                float lateral = lx * taper + MathF.Sin(t * 3f * sp + ph * 6.28f + u * 4f) * sway * u * 0.6f;
                var pos = origin + lat * lateral + dir * (u * height * 0.8f * hf);
                float r = sizeEff * 0.5f * sz * (1f - u);
                float a = 0.7f * (1f - u * 1.6f) * bright;
                if (a <= 0f) continue;
                Blob(dc, brush, wall, pos, r, InnerColor, a);
            }

            // ---- 火の粉 ----
            var rs = new Random(Seed + 2);
            int sparks = wall ? Math.Min(400, (int)(sn * (half * 2f) / 500f)) : sn;
            for (int i = 0; i < sparks; i++)
            {
                float ph = rs.NextF(), rx = rs.NextF() * 2f - 1f, sp = 0.6f + 0.8f * rs.NextF();
                float r = 1f + 1.6f * rs.NextF(), drift = rs.NextF() * 2f - 1f;
                float u = Gfx.Mod(t * 0.35f * sp * speed + ph, 1f);
                float lateral = rx * half * (wall ? 1f : 0.4f) + MathF.Sin(t * 2f + ph * 6.28f) * sway * 0.6f * u + drift * 80f * u;
                var pos = origin + lat * lateral + dir * (u * height * 1.8f);
                float a = (1f - u) * (0.5f + 0.5f * MathF.Sin(t * 20f + ph * 40f)) * bright;
                brush.Color = Gfx.C4(InnerColor, a);
                dc.FillEllipse(new Ellipse(pos, r, r), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
