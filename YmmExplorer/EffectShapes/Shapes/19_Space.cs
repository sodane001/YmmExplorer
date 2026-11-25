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
//  宇宙
//   深い闇のグラデーション + 星雲 + 渦巻銀河 + 星 + 惑星（縞・大気・リング）+ 彗星
// =====================================================================
public sealed class SpacePlugin : IShapePlugin
{
    public string Name => "演出：宇宙";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpaceParameter(sharedData);
}

public sealed class SpaceParameter : EffectShapeParameterBase
{
    public SpaceParameter() : this(null) { }
    public SpaceParameter(SharedDataStore? sharedData) : base(sharedData) { }

    // ---- 背景 ----
    [Display(GroupName = "背景", Name = "上の色")]
    [ColorPicker]
    public WpfColor BgTop { get => bgTop; set => Set(ref bgTop, value); }
    WpfColor bgTop = WpfColor.FromRgb(0x01, 0x02, 0x0A);

    [Display(GroupName = "背景", Name = "下の色")]
    [ColorPicker]
    public WpfColor BgBottom { get => bgBottom; set => Set(ref bgBottom, value); }
    WpfColor bgBottom = WpfColor.FromRgb(0x0B, 0x12, 0x36);

    [Display(GroupName = "背景", Name = "背景の不透明度", Description = "0で背景なし（星や惑星だけを重ねられる）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BgOpacity { get; } = new Animation(100, 0, 100);

    // ---- 星雲 ----
    [Display(GroupName = "星雲", Name = "色1")]
    [ColorPicker]
    public WpfColor Nebula1 { get => n1; set => Set(ref n1, value); }
    WpfColor n1 = WpfColor.FromRgb(0x7A, 0x3C, 0xFF);

    [Display(GroupName = "星雲", Name = "色2")]
    [ColorPicker]
    public WpfColor Nebula2 { get => n2; set => Set(ref n2, value); }
    WpfColor n2 = WpfColor.FromRgb(0xFF, 0x4F, 0xA8);

    [Display(GroupName = "星雲", Name = "色3")]
    [ColorPicker]
    public WpfColor Nebula3 { get => n3; set => Set(ref n3, value); }
    WpfColor n3 = WpfColor.FromRgb(0x2F, 0xC9, 0xD8);

    [Display(GroupName = "星雲", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation NebulaStrength { get; } = new Animation(60, 0, 500);

    [Display(GroupName = "星雲", Name = "大きさ")]
    [AnimationSlider("F0", "px", 100, 1000)]
    public Animation NebulaSize { get; } = new Animation(420, 20, 8000);

    // ---- 星 ----
    [Display(GroupName = "星", Name = "数")]
    [AnimationSlider("F0", "個", 0, 1500)]
    public Animation StarCount { get; } = new Animation(500, 0, 20000);

    [Display(GroupName = "星", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation StarBrightness { get; } = new Animation(100, 0, 500);

    // ---- 銀河 ----
    [Display(GroupName = "銀河", Name = "強さ", Description = "0で表示しない")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation GalaxyStrength { get; } = new Animation(70, 0, 500);

    [Display(GroupName = "銀河", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation GalaxyX { get; } = new Animation(30, -100, 200);

    [Display(GroupName = "銀河", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation GalaxyY { get; } = new Animation(35, -100, 200);

    [Display(GroupName = "銀河", Name = "大きさ")]
    [AnimationSlider("F0", "px", 50, 800)]
    public Animation GalaxySize { get; } = new Animation(320, 5, 8000);

    [Display(GroupName = "銀河", Name = "傾き")]
    [AnimationSlider("F0", "°", -90, 90)]
    public Animation GalaxyTilt { get; } = new Animation(-25, -360, 360);

    [Display(GroupName = "銀河", Name = "回転速度")]
    [AnimationSlider("F1", "°/s", -20, 20)]
    public Animation GalaxySpin { get; } = new Animation(1.5, -360, 360);

    [Display(GroupName = "銀河", Name = "中心の色")]
    [ColorPicker]
    public WpfColor GalaxyCore { get => gCore; set => Set(ref gCore, value); }
    WpfColor gCore = WpfColor.FromRgb(0xFF, 0xE9, 0xB8);

    [Display(GroupName = "銀河", Name = "腕の色")]
    [ColorPicker]
    public WpfColor GalaxyArm { get => gArm; set => Set(ref gArm, value); }
    WpfColor gArm = WpfColor.FromRgb(0x9D, 0xB8, 0xFF);

    // ---- 惑星 ----
    [Display(GroupName = "惑星", Name = "半径", Description = "0で表示しない")]
    [AnimationSlider("F0", "px", 0, 400)]
    public Animation PlanetSize { get; } = new Animation(140, 0, 5000);

    [Display(GroupName = "惑星", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PlanetX { get; } = new Animation(72, -100, 200);

    [Display(GroupName = "惑星", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PlanetY { get; } = new Animation(62, -100, 200);

    [Display(GroupName = "惑星", Name = "本体の色")]
    [ColorPicker]
    public WpfColor PlanetColor { get => pColor; set => Set(ref pColor, value); }
    WpfColor pColor = WpfColor.FromRgb(0xC4, 0x6A, 0x3C);

    [Display(GroupName = "惑星", Name = "縞の色")]
    [ColorPicker]
    public WpfColor PlanetBand { get => pBand; set => Set(ref pBand, value); }
    WpfColor pBand = WpfColor.FromRgb(0xF0, 0xC2, 0x8A);

    [Display(GroupName = "惑星", Name = "大気の色")]
    [ColorPicker]
    public WpfColor AtmosphereColor { get => pAtm; set => Set(ref pAtm, value); }
    WpfColor pAtm = WpfColor.FromRgb(0x7F, 0xB8, 0xFF);

    [Display(GroupName = "惑星", Name = "光の向き", Description = "太陽の光が来る向き。0=右、90=下")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation LightAngle { get; } = new Animation(-135, -3600, 3600);

    [Display(GroupName = "惑星", Name = "リングをつける")]
    [ToggleSlider]
    public bool Ring { get => ring; set => Set(ref ring, value); }
    bool ring = true;

    [Display(GroupName = "惑星", Name = "リングの傾き")]
    [AnimationSlider("F0", "°", -90, 90)]
    public Animation RingTilt { get; } = new Animation(-18, -360, 360);

    // ---- 彗星 ----
    [Display(GroupName = "彗星", Name = "出現間隔", Description = "0で出現しない")]
    [AnimationSlider("F1", "秒", 0, 20)]
    public Animation CometInterval { get; } = new Animation(7, 0, 600);

    [Display(GroupName = "彗星", Name = "尾の長さ")]
    [AnimationSlider("F0", "px", 100, 1000)]
    public Animation CometLength { get; } = new Animation(520, 10, 8000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [BgOpacity, NebulaStrength, NebulaSize, StarCount, StarBrightness,
         GalaxyStrength, GalaxyX, GalaxyY, GalaxySize, GalaxyTilt, GalaxySpin,
         PlanetSize, PlanetX, PlanetY, LightAngle, RingTilt, CometInterval, CometLength];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var white = WpfColor.FromRgb(255, 255, 255);
        using var brush = dc.CreateSolidColorBrush(default);

        try
        {
            // ---- 1. 背景 ----
            float bgOp = c.F(BgOpacity) / 100f;
            if (bgOp > 0f)
            {
                using var bg = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, h),
                    (0f, Gfx.C4(BgTop, bgOp)), (1f, Gfx.C4(BgBottom, bgOp)));
                Gfx.Rect(dc, bg, 0, 0, w, h);
            }

            // ---- 2. 星雲（加算でふんわり光る）----
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            float ns = c.F(NebulaStrength) / 100f, nsz = c.F(NebulaSize);
            if (ns > 0f)
            {
                var rn = new Random(Seed + 3);
                var cols = new[] { Nebula1, Nebula2, Nebula3 };
                for (int i = 0; i < 8; i++)
                {
                    float x = rn.NextF() * w, y = rn.NextF() * h;
                    float r = nsz * (0.6f + 0.6f * rn.NextF());
                    float ph = rn.NextF() * MathF.PI * 2f;
                    var ctr = new Vector2(x + MathF.Sin(t * 0.07f + ph) * 40f, y + MathF.Cos(t * 0.05f + ph) * 30f);
                    var col = cols[i % 3];
                    using var nb = Gfx.Radial(dc, ctr, r,
                        (0f, Gfx.C4(col, 0.16f * ns)), (0.5f, Gfx.C4(col, 0.07f * ns)), (1f, Gfx.C4(col, 0f)));
                    dc.FillEllipse(new Ellipse(ctr, r, r), nb);
                }
            }

            // ---- 3. 渦巻銀河 ----
            float gs = c.F(GalaxyStrength) / 100f;
            if (gs > 0f)
            {
                var gc = new Vector2(c.F(GalaxyX) / 100f * w, c.F(GalaxyY) / 100f * h);
                float size = c.F(GalaxySize);
                float tilt = Gfx.Rad(c.F(GalaxyTilt));
                float spin = Gfx.Rad(c.F(GalaxySpin)) * t;
                float cs = MathF.Cos(tilt), sn = MathF.Sin(tilt);

                using (var core = Gfx.Radial(dc, gc, size * 0.35f,
                    (0f, Gfx.C4(GalaxyCore, 0.65f * gs)), (0.4f, Gfx.C4(GalaxyCore, 0.2f * gs)), (1f, Gfx.C4(GalaxyCore, 0f))))
                    dc.FillEllipse(new Ellipse(gc, size * 0.35f, size * 0.35f), core);

                var rg = new Random(Seed + 9);
                for (int i = 0; i < 900; i++)
                {
                    int arm = i % 2;
                    float u = MathF.Pow(rg.NextF(), 0.7f);
                    float ang = arm * MathF.PI + u * 5.5f + spin * (1f - 0.5f * u);
                    float scatter = size * 0.07f * (1f - 0.5f * u);
                    float sx = (rg.NextF() + rg.NextF() + rg.NextF() - 1.5f) * scatter;
                    float sy = (rg.NextF() + rg.NextF() + rg.NextF() - 1.5f) * scatter;
                    var local = new Vector2(MathF.Cos(ang) * u * size + sx, MathF.Sin(ang) * u * size * 0.45f + sy * 0.45f);
                    var p = gc + new Vector2(local.X * cs - local.Y * sn, local.X * sn + local.Y * cs);
                    float r = 0.6f + 1.0f * rg.NextF();
                    var col = Gfx.Mix(GalaxyCore, GalaxyArm, u);
                    brush.Color = Gfx.C4(col, (0.15f + 0.45f * rg.NextF()) * gs * (1f - 0.4f * u));
                    dc.FillEllipse(new Ellipse(p, r, r), brush);
                }
            }
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

            // ---- 4. 星（またたく）----
            int sc = (int)c.F(StarCount);
            float sb = c.F(StarBrightness) / 100f;
            var rs = new Random(Seed);
            var blue = WpfColor.FromRgb(0x9B, 0xB8, 0xFF);
            var yellow = WpfColor.FromRgb(0xFF, 0xE2, 0xA0);
            for (int i = 0; i < sc; i++)
            {
                float x = rs.NextF() * w, y = rs.NextF() * h;
                float m = rs.NextF(); m = m * m * m;
                float ph = rs.NextF() * MathF.PI * 2f, ts = 0.5f + rs.NextF() * 2f;
                float tintSel = rs.NextF();
                float r = 0.6f + 2.4f * m;
                float tw = 0.65f + 0.35f * MathF.Sin(t * 2f * ts + ph);
                float a = (0.3f + 0.7f * m) * tw * sb;
                var col = tintSel < 0.25f ? Gfx.Mix(white, blue, 0.7f) : (tintSel < 0.4f ? Gfx.Mix(white, yellow, 0.7f) : white);
                var ctr = new Vector2(x, y);
                brush.Color = Gfx.C4(col, a * 0.15f);
                dc.FillEllipse(new Ellipse(ctr, r * 2.4f, r * 2.4f), brush);
                brush.Color = Gfx.C4(col, a);
                dc.FillEllipse(new Ellipse(ctr, r, r), brush);
                if (m > 0.6f)
                {
                    float fl = r * 6f * tw;
                    brush.Color = Gfx.C4(col, a * 0.5f);
                    dc.DrawLine(new Vector2(x - fl, y), new Vector2(x + fl, y), brush, 1f);
                    dc.DrawLine(new Vector2(x, y - fl), new Vector2(x, y + fl), brush, 1f);
                }
            }

            // ---- 5. 惑星 ----
            float pr = c.F(PlanetSize);
            if (pr > 1f)
            {
                var pc = new Vector2(c.F(PlanetX) / 100f * w, c.F(PlanetY) / 100f * h);
                float la = Gfx.Rad(c.F(LightAngle));
                var lightDir = new Vector2(MathF.Cos(la), MathF.Sin(la));      // 光が来る向き（光源側）
                float ringTilt = Gfx.Rad(c.F(RingTilt));

                // 大気のにじみ（本体の背後）
                float ar = pr * 1.25f;
                using (var at = Gfx.Radial(dc, pc, ar,
                    (0.76f, Gfx.C4(AtmosphereColor, 0f)), (0.82f, Gfx.C4(AtmosphereColor, 0.35f)), (1f, Gfx.C4(AtmosphereColor, 0f))))
                    dc.FillEllipse(new Ellipse(pc, ar, ar), at);

                // リングの奥側（本体より先に描く）
                if (Ring) DrawRing(dc, brush, pc, pr, ringTilt, false);

                // 本体
                brush.Color = Gfx.C4(PlanetColor);
                dc.FillEllipse(new Ellipse(pc, pr, pr), brush);

                // 縞（円の弦の幅で描く）
                var rb = new Random(Seed + 13);
                for (int k = 0; k < 7; k++)
                {
                    float yk = -pr * 0.8f + k * pr * 0.26f + (rb.NextF() - 0.5f) * pr * 0.06f;
                    float th = pr * (0.05f + 0.08f * rb.NextF());
                    float chord = MathF.Sqrt(MathF.Max(0f, pr * pr - yk * yk));
                    brush.Color = Gfx.C4(PlanetBand, (k % 2 == 0 ? 0.35f : 0.18f));
                    Gfx.Rect(dc, brush, pc.X - chord, pc.Y + yk - th / 2f, chord * 2f, th);
                }

                // 陰影（光源側が明るく、反対側が暗い）
                var litCenter = pc + lightDir * pr * 0.55f;
                using (var sh = Gfx.Radial(dc, litCenter, pr * 1.9f,
                    (0f, Gfx.C4(white, 0.28f)), (0.45f, Gfx.C4(0f, 0f, 0f, 0.0f)), (1f, Gfx.C4(0f, 0f, 0.05f, 0.85f))))
                    dc.FillEllipse(new Ellipse(pc, pr, pr), sh);

                // 縁の大気光
                brush.Color = Gfx.C4(AtmosphereColor, 0.35f);
                dc.DrawEllipse(new Ellipse(pc, pr, pr), brush, 1.5f);

                // リングの手前側
                if (Ring) DrawRing(dc, brush, pc, pr, ringTilt, true);
            }

            // ---- 6. 彗星 ----
            float interval = c.F(CometInterval);
            if (interval > 0.5f)
            {
                const float dur = 1.4f;
                int idx = (int)MathF.Floor(t / interval);
                float local = t - idx * interval;
                if (local < dur)
                {
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    var rc = new Random(Seed * 1009 + idx * 7919);
                    float u = local / dur;
                    var start = new Vector2(w * (0.05f + 0.5f * rc.NextF()), h * (0.05f + 0.3f * rc.NextF()));
                    float ang = Gfx.Rad(20f + 25f * rc.NextF());
                    var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                    float len = c.F(CometLength);
                    var head = start + dir * (MathF.Max(w, h) * 1.1f * u);
                    float env = MathF.Sin(u * MathF.PI);
                    var ice = WpfColor.FromRgb(0xBF, 0xE6, 0xFF);

                    Gfx.FadeLine(dc, brush, head, head - dir * len, 18f, 2f, ice, 0.25f * env, 20, 1.3f);
                    Gfx.FadeLine(dc, brush, head, head - dir * (len * 0.7f), 7f, 1f, white, 0.6f * env, 20, 1.4f);
                    using var hg = Gfx.Radial(dc, head, 26f,
                        (0f, Gfx.C4(white, 0.9f * env)), (0.4f, Gfx.C4(ice, 0.4f * env)), (1f, Gfx.C4(ice, 0f)));
                    dc.FillEllipse(new Ellipse(head, 26f, 26f), hg);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            dc.Transform = Matrix3x2.Identity;
        }
    }

    // 惑星のリング。front=false で奥側（上半分）、true で手前側（下半分）
    void DrawRing(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 pc, float pr, float tilt, bool front)
    {
        dc.Transform = Matrix3x2.CreateRotation(tilt, pc);
        try
        {
            const int rings = 8;
            for (int k = 0; k < rings; k++)
            {
                float f = k / (float)(rings - 1);
                float rx = pr * (1.35f + 0.7f * f);
                float ry = rx * 0.28f;
                float a = front ? MathF.PI : MathF.PI;           // 半周分
                var pts = new List<Vector2>();
                const int N = 60;
                for (int i = 0; i <= N; i++)
                {
                    float ang = (front ? 0f : MathF.PI) + i / (float)N * a;
                    pts.Add(pc + new Vector2(MathF.Cos(ang) * rx, MathF.Sin(ang) * ry));
                }
                var col = Gfx.Mix(PlanetBand, WpfColor.FromRgb(0xD9, 0xCF, 0xC0), f);
                float alpha = (k % 3 == 1) ? 0.20f : 0.55f;
                brush.Color = Gfx.C4(col, alpha);
                Gfx.Polyline(dc, brush, pts, pr * 0.05f);
            }
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
