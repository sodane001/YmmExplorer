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
//  サイバー地球儀
//   緯線と経線の地球と、世界をつなぐ光の線
// =====================================================================
public sealed class CyberGlobePlugin : IShapePlugin
{
    public string Name => "演出：サイバー地球儀";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CyberGlobeParameter(sharedData);
}

public sealed class CyberGlobeParameter : EffectShapeParameterBase
{
    public CyberGlobeParameter() : this(null) { }
    public CyberGlobeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "地球の線の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x39, 0xD8, 0xFF);

    [Display(GroupName = "色", Name = "つなぐ光の色")]
    [ColorPicker]
    public WpfColor ArcColor { get => arcColor; set => Set(ref arcColor, value); }
    WpfColor arcColor = WpfColor.FromRgb(0xFF, 0x5C, 0xD8);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "半径")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Radius { get; } = new Animation(270, 5, 5000);

    [Display(GroupName = "位置と大きさ", Name = "傾き")]
    [AnimationSlider("F0", "°", -60, 60)]
    public Animation Tilt { get; } = new Animation(20, -90, 90);

    [Display(GroupName = "動き", Name = "自転の速さ")]
    [AnimationSlider("F0", "°/秒", -90, 90)]
    public Animation Spin { get; } = new Animation(14, -3600, 3600);

    [Display(GroupName = "線", Name = "緯線の数")]
    [AnimationSlider("F0", "本", 0, 30)]
    public Animation LatLines { get; } = new Animation(9, 0, 100);

    [Display(GroupName = "線", Name = "経線の数")]
    [AnimationSlider("F0", "本", 0, 48)]
    public Animation LonLines { get; } = new Animation(14, 0, 200);

    [Display(GroupName = "線", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 6)]
    public Animation Thickness { get; } = new Animation(1.6, 0.2, 50);

    [Display(GroupName = "つなぐ光", Name = "光の線の数")]
    [AnimationSlider("F0", "本", 0, 40)]
    public Animation Arcs { get; } = new Animation(12, 0, 300);

    [Display(GroupName = "つなぐ光", Name = "光が走る速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation ArcSpeed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "つなぐ光", Name = "光の線の高さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ArcLift { get; } = new Animation(30, 0, 300);

    [Display(GroupName = "装飾", Name = "都市の光点を表示する")]
    [ToggleSlider]
    public bool Cities { get => cities; set => Set(ref cities, value); }
    bool cities = true;

    [Display(GroupName = "装飾", Name = "周りの輪を表示する")]
    [ToggleSlider]
    public bool Ring { get => ring; set => Set(ref ring, value); }
    bool ring = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Tilt, Spin, LatLines, LonLines, Thickness, Arcs, ArcSpeed, ArcLift];

    static Vector3 Unit(float lat, float lon)
        => new(MathF.Cos(lat) * MathF.Sin(lon), MathF.Sin(lat), MathF.Cos(lat) * MathF.Cos(lon));

    /// <summary>地球上の点（単位ベクトル×倍率）を、自転と傾きをかけて画面に写す。z&gt;0 が手前</summary>
    static Vector3 Project(Vector3 v, float spin, float tilt)
    {
        float cs = MathF.Cos(spin), sn = MathF.Sin(spin);
        float x = v.X * cs + v.Z * sn, z = -v.X * sn + v.Z * cs, y = v.Y;
        float ct = MathF.Cos(tilt), st = MathF.Sin(tilt);
        float y2 = y * ct - z * st, z2 = y * st + z * ct;
        return new Vector3(x, y2, z2);
    }

    static Vector3 Slerp(Vector3 a, Vector3 b, float s)
    {
        float dot = Math.Clamp(Vector3.Dot(a, b), -1f, 1f);
        float om = MathF.Acos(dot);
        if (om < 1e-4f) return a;
        float so = MathF.Sin(om);
        return a * (MathF.Sin((1f - s) * om) / so) + b * (MathF.Sin(s * om) / so);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), th = c.F(Thickness), lift = c.F(ArcLift) / 100f, arcSpeed = c.F(ArcSpeed) / 100f;
        float spin = Gfx.Rad(c.F(Spin) * t), tilt = Gfx.Rad(c.F(Tilt));
        int nLat = (int)c.F(LatLines), nLon = (int)c.F(LonLines), nArc = (int)c.F(Arcs);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // ---- 球の光 ----
            using (var g = Gfx.Radial(dc, ctr, R, (0f, Gfx.C4(Color, 0.10f)), (0.85f, Gfx.C4(Color, 0.05f)), (1f, Gfx.C4(Color, 0.18f))))
                dc.FillEllipse(new Ellipse(ctr, R, R), g);
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            DrawKit.Glow(dc, ctr, R * 1.25f, Gfx.C4(Color, 0.12f));
            brush.Color = Gfx.C4(Color, 0.75f);
            dc.DrawEllipse(new Ellipse(ctr, R, R), brush, th * 1.3f);

            // ---- 緯線 ----
            for (int i = 1; i <= nLat; i++)
            {
                float lat = -MathF.PI / 2f + MathF.PI * i / (nLat + 1);
                DrawLoop(dc, brush, ctr, R, th, spin, tilt, k => Unit(lat, k * MathF.PI * 2f / 72f), 72);
            }
            // ---- 経線 ----
            for (int j = 0; j < nLon; j++)
            {
                float lon = MathF.PI * 2f * j / Math.Max(1, nLon);
                DrawLoop(dc, brush, ctr, R, th, spin, tilt, k => Unit(-MathF.PI / 2f + MathF.PI * k / 36f, lon), 36, closed: false);
            }

            // ---- 都市とつなぐ光 ----
            int nCity = Math.Max(nArc + 4, 18);
            var cityV = new Vector3[nCity];
            for (int i = 0; i < nCity; i++)
            {
                float lat = (DrawKit.H(Seed, i, 1) - 0.5f) * MathF.PI * 0.75f;
                float lon = DrawKit.H(Seed, i, 2) * MathF.PI * 2f;
                cityV[i] = Unit(lat, lon);
            }
            for (int a = 0; a < nArc; a++)
            {
                int i0 = (int)(DrawKit.H(Seed, a, 3) * nCity) % nCity;
                int i1 = (i0 + 1 + (int)(DrawKit.H(Seed, a, 4) * (nCity - 1))) % nCity;
                var va = cityV[i0];
                var vb = cityV[i1];
                const int seg = 40;
                var pts = new Vector3[seg + 1];
                for (int k = 0; k <= seg; k++)
                {
                    float s = k / (float)seg;
                    var v = Slerp(va, vb, s) * (1f + lift * MathF.Sin(MathF.PI * s) * MathF.Min(1f, MathF.Acos(Math.Clamp(Vector3.Dot(va, vb), -1f, 1f))));
                    pts[k] = Project(v, spin, tilt);
                }
                float head = DrawKit.Frac(t * arcSpeed * (0.3f + 0.3f * DrawKit.H(Seed, a, 5)) + DrawKit.H(Seed, a, 6));
                for (int k = 0; k < seg; k++)
                {
                    var p0 = pts[k];
                    var p1 = pts[k + 1];
                    bool hidden = (p0.Z < 0f && new Vector2(p0.X, p0.Y).Length() < 1f) || (p1.Z < 0f && new Vector2(p1.X, p1.Y).Length() < 1f);
                    float s = (k + 0.5f) / seg;
                    float trailA = s <= head && s >= head - 0.25f ? 1f - (head - s) / 0.25f : 0f;
                    float baseA = hidden ? 0.06f : 0.22f;
                    var s0 = ctr + new Vector2(p0.X, -p0.Y) * R;
                    var s1 = ctr + new Vector2(p1.X, -p1.Y) * R;
                    brush.Color = Gfx.C4(ArcColor, baseA);
                    dc.DrawLine(s0, s1, brush, th);
                    if (trailA > 0f)
                    {
                        brush.Color = Gfx.C4(Gfx.Mix(ArcColor, white, 0.4f), trailA * (hidden ? 0.25f : 1f));
                        dc.DrawLine(s0, s1, brush, th * 2.2f);
                    }
                }
                // 先頭の光
                int hk = Math.Clamp((int)(head * seg), 0, seg);
                var hp = pts[hk];
                if (hp.Z >= 0f || new Vector2(hp.X, hp.Y).Length() >= 1f)
                    DrawKit.Glow(dc, ctr + new Vector2(hp.X, -hp.Y) * R, th * 7f, Gfx.C4(Gfx.Mix(ArcColor, white, 0.5f), 0.9f));
            }

            if (Cities)
            {
                for (int i = 0; i < nCity; i++)
                {
                    var p = Project(cityV[i], spin, tilt);
                    if (p.Z < 0f) continue;
                    var sp = ctr + new Vector2(p.X, -p.Y) * R;
                    float pulse = 0.6f + 0.4f * MathF.Sin(t * 3f + i);
                    DrawKit.Glow(dc, sp, th * 6f, Gfx.C4(ArcColor, 0.6f * pulse * p.Z));
                    brush.Color = Gfx.C4(white, 0.9f * p.Z);
                    dc.FillEllipse(new Ellipse(sp, th * 1.1f, th * 1.1f), brush);
                }
            }

            // ---- 周りの輪 ----
            if (Ring)
            {
                var ringPts = new List<Vector2>();
                for (int k = 0; k <= 120; k++)
                {
                    float a = k * MathF.PI * 2f / 120f;
                    ringPts.Add(DrawKit.Rot(ctr + new Vector2(MathF.Cos(a) * R * 1.35f, MathF.Sin(a) * R * 0.3f), ctr, -0.25f));
                }
                brush.Color = Gfx.C4(Color, 0.45f);
                FormKit.Dash(dc, brush, ringPts, 14f, 10f, t * 40f, th);
                float ra = t * 0.8f;
                var dot = DrawKit.Rot(ctr + new Vector2(MathF.Cos(ra) * R * 1.35f, MathF.Sin(ra) * R * 0.3f), ctr, -0.25f);
                DrawKit.Glow(dc, dot, th * 8f, Gfx.C4(Color, 0.9f));
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    void DrawLoop(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float R, float th, float spin, float tilt,
        Func<int, Vector3> at, int n, bool closed = true)
    {
        int count = closed ? n : n;
        var prevP = Project(at(0), spin, tilt);
        for (int k = 1; k <= count; k++)
        {
            var p = Project(at(closed ? k % n : k), spin, tilt);
            float z = (prevP.Z + p.Z) / 2f;
            brush.Color = Gfx.C4(Color, z >= 0f ? 0.25f + 0.45f * z : 0.07f);
            dc.DrawLine(ctr + new Vector2(prevP.X, -prevP.Y) * R, ctr + new Vector2(p.X, -p.Y) * R, brush, z >= 0f ? th : th * 0.7f);
            prevP = p;
        }
    }
}
