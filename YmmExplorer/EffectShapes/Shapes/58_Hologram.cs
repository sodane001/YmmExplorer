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

public enum HoloKind
{
    [Display(Name = "立方体")] Cube,
    [Display(Name = "球（ワイヤー）")] Sphere,
    [Display(Name = "正八面体")] Octahedron,
}

// =====================================================================
//  ホログラム
//   投影台から浮かぶ回転するワイヤーフレーム。走査線・ちらつき・グリッチの帯つき
// =====================================================================
public sealed class HologramPlugin : IShapePlugin
{
    public string Name => "演出：ホログラム";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HologramParameter(sharedData);
}

public sealed class HologramParameter : EffectShapeParameterBase
{
    public HologramParameter() : this(null) { }
    public HologramParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "形")]
    [EnumComboBox]
    public HoloKind Kind { get => kind; set => Set(ref kind, value); }
    HoloKind kind = HoloKind.Cube;

    [Display(GroupName = "形", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x4F, 0xE8, 0xFF);

    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(45, -100, 200);

    [Display(GroupName = "形", Name = "大きさ")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Size { get; } = new Animation(200, 5, 5000);

    [Display(GroupName = "形", Name = "回転速度")]
    [AnimationSlider("F0", "°/s", -180, 180)]
    public Animation RotSpeed { get; } = new Animation(35, -3600, 3600);

    [Display(GroupName = "形", Name = "前後の傾き")]
    [AnimationSlider("F0", "°", -60, 60)]
    public Animation Tilt { get; } = new Animation(22, -360, 360);

    [Display(GroupName = "光", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "光", Name = "走査線")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Scan { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "光", Name = "ちらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "光", Name = "乱れる帯")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Glitch { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "投影台", Name = "投影台と光の円錐を表示する")]
    [ToggleSlider]
    public bool ShowBase { get => showBase; set => Set(ref showBase, value); }
    bool showBase = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Size, RotSpeed, Tilt, Brightness, Scan, Flicker, Glitch];

    // 3D点を回転・投影する
    static Vector3 Rotate(Vector3 p, float ay, float ax)
    {
        float cy = MathF.Cos(ay), sy = MathF.Sin(ay);
        var q = new Vector3(p.X * cy + p.Z * sy, p.Y, -p.X * sy + p.Z * cy);
        float cx = MathF.Cos(ax), sx = MathF.Sin(ax);
        return new Vector3(q.X, q.Y * cx - q.Z * sx, q.Y * sx + q.Z * cx);
    }

    static Vector2 Project(Vector3 p, Vector2 ctr, float focal)
    {
        float k = focal / (focal + p.Z);
        return ctr + new Vector2(p.X, p.Y) * k;
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float S = c.F(Size), bright = c.F(Brightness) / 100f;
        float ay = Gfx.Rad(c.F(RotSpeed)) * t, ax = Gfx.Rad(c.F(Tilt));
        float scan = c.F(Scan) / 100f, flick = c.F(Flicker) / 100f, glitch = c.F(Glitch) / 100f;
        float focal = S * 5f;
        var white = WpfColor.FromRgb(255, 255, 255);
        var hot = Gfx.Mix(Color, white, 0.5f);

        int tick = (int)MathF.Floor(t * 18f);
        var rng = new Random(Seed * 7919 + tick * 131);
        float fl = 1f - flick * (rng.NextF() < 0.18f ? 0.55f * rng.NextF() : 0f) - flick * 0.08f * MathF.Sin(t * 40f);
        bright *= fl;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 投影台と光の円錐 ----
            if (ShowBase)
            {
                float by = ctr.Y + S * 1.35f;
                float rx0 = S * 1.0f;
                for (int k = 0; k < 4; k++)
                {
                    float rx = rx0 * (1f - 0.18f * k) * (1f + 0.03f * MathF.Sin(t * 3f + k));
                    brush.Color = Gfx.C4(Color, (0.65f - 0.13f * k) * bright);
                    dc.DrawEllipse(new Ellipse(new Vector2(ctr.X, by), rx, rx * 0.22f), brush, k == 0 ? 3f : 1.6f);
                }
                brush.Color = Gfx.C4(Color, 0.10f * bright);
                dc.FillEllipse(new Ellipse(new Vector2(ctr.X, by), rx0, rx0 * 0.22f), brush);
                // 光の円錐（細い縦線の束）
                for (int i = 0; i < 28; i++)
                {
                    float a = i / 28f * MathF.PI * 2f + t * 0.3f;
                    var coneBottom = new Vector2(ctr.X + MathF.Cos(a) * rx0 * 0.82f, by + MathF.Sin(a) * rx0 * 0.18f);
                    var coneTop = new Vector2(ctr.X + MathF.Cos(a) * S * 0.55f, ctr.Y + S * 0.9f + MathF.Sin(a) * S * 0.12f);
                    Gfx.FadeLine(dc, brush, coneBottom, coneTop, 1.6f, 1f, Color, 0.22f * bright, 6, 1f);
                }
            }

            // ---- ワイヤーフレーム ----
            var segs = new List<(Vector3 a, Vector3 b)>();
            float R = S * 0.7f;
            switch (Kind)
            {
                case HoloKind.Cube:
                    {
                        var v = new Vector3[8];
                        for (int i = 0; i < 8; i++)
                            v[i] = new Vector3((i & 1) == 0 ? -R : R, (i & 2) == 0 ? -R : R, (i & 4) == 0 ? -R : R);
                        for (int i = 0; i < 8; i++)
                            for (int bit = 1; bit <= 4; bit <<= 1)
                                if ((i & bit) == 0) segs.Add((v[i], v[i | bit]));
                        break;
                    }
                case HoloKind.Octahedron:
                    {
                        float Ro = S * 0.95f;
                        var v = new[] { new Vector3(Ro, 0, 0), new Vector3(-Ro, 0, 0), new Vector3(0, Ro, 0), new Vector3(0, -Ro, 0), new Vector3(0, 0, Ro), new Vector3(0, 0, -Ro) };
                        for (int i = 0; i < 6; i++)
                            for (int j = i + 1; j < 6; j++)
                                if (j != (i ^ 1)) segs.Add((v[i], v[j]));
                        break;
                    }
                default:
                    {
                        float Rs = S * 0.85f;
                        const int N = 40;
                        for (int lat = -2; lat <= 2; lat++)
                        {
                            float phi = lat * MathF.PI / 6f;
                            for (int i = 0; i < N; i++)
                            {
                                float a0 = i * MathF.PI * 2f / N, a1 = (i + 1) * MathF.PI * 2f / N;
                                segs.Add((new Vector3(MathF.Cos(phi) * MathF.Cos(a0), MathF.Sin(phi), MathF.Cos(phi) * MathF.Sin(a0)) * Rs,
                                          new Vector3(MathF.Cos(phi) * MathF.Cos(a1), MathF.Sin(phi), MathF.Cos(phi) * MathF.Sin(a1)) * Rs));
                            }
                        }
                        for (int m = 0; m < 6; m++)
                        {
                            float lon = m * MathF.PI / 6f;
                            for (int i = 0; i < N; i++)
                            {
                                float p0 = i * MathF.PI * 2f / N, p1 = (i + 1) * MathF.PI * 2f / N;
                                segs.Add((new Vector3(MathF.Cos(p0) * MathF.Cos(lon), MathF.Sin(p0), MathF.Cos(p0) * MathF.Sin(lon)) * Rs,
                                          new Vector3(MathF.Cos(p1) * MathF.Cos(lon), MathF.Sin(p1), MathF.Cos(p1) * MathF.Sin(lon)) * Rs));
                            }
                        }
                        break;
                    }
            }

            float zMax = S * 1.2f;
            foreach (var s in segs)
            {
                var a = Rotate(s.a, ay, ax);
                var b = Rotate(s.b, ay, ax);
                float zAvg = (a.Z + b.Z) / 2f;
                float depth = Gfx.Smooth(-zMax, zMax, -zAvg);                   // 手前ほど明るい
                var pa = Project(a, ctr, focal);
                var pb = Project(b, ctr, focal);
                float al = (0.25f + 0.75f * depth) * bright;
                brush.Color = Gfx.C4(Color, 0.14f * al);
                dc.DrawLine(pa, pb, brush, 7f);
                brush.Color = Gfx.C4(hot, 0.9f * al);
                dc.DrawLine(pa, pb, brush, 1.6f);
            }
            if (Kind != HoloKind.Sphere)
            {
                // 頂点の光
                foreach (var s in segs)
                {
                    var a = Project(Rotate(s.a, ay, ax), ctr, focal);
                    brush.Color = Gfx.C4(white, 0.6f * bright);
                    dc.FillEllipse(new Ellipse(a, 2.2f, 2.2f), brush);
                }
            }

            // ---- 走査線（細い横線 + 流れる明るい帯）----
            float top = ctr.Y - S * 1.15f, bottom = ctr.Y + S * 1.25f;
            if (scan > 0f)
            {
                brush.Color = Gfx.C4(Color, 0.07f * scan * bright);
                for (float y = top; y < bottom; y += 5f)
                    dc.DrawLine(new Vector2(ctr.X - S * 1.2f, y), new Vector2(ctr.X + S * 1.2f, y), brush, 1.2f);
                float by2 = top + Gfx.Mod(t * 0.35f, 1f) * (bottom - top);
                for (int i = 0; i < 12; i++)
                {
                    brush.Color = Gfx.C4(hot, 0.22f * scan * bright * (1f - i / 12f));
                    dc.DrawLine(new Vector2(ctr.X - S * 1.2f, by2 - i * 2f), new Vector2(ctr.X + S * 1.2f, by2 - i * 2f), brush, 2f);
                }
            }

            // ---- 乱れる帯 ----
            if (glitch > 0f && rng.NextF() < glitch * 0.8f)
            {
                int nb = 1 + rng.Next(0, 3);
                for (int i = 0; i < nb; i++)
                {
                    float y = top + rng.NextF() * (bottom - top), hh = 2f + rng.NextF() * 14f;
                    float dx = (rng.NextF() - 0.5f) * S * 0.5f;
                    brush.Color = Gfx.C4(hot, 0.30f * bright);
                    Gfx.Rect(dc, brush, ctr.X - S * 1.1f + dx, y, S * 2.2f, hh);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
