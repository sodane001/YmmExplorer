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
//  魔法陣
//   多重の輪・ルーン文字・星形・周回する光。書き上がる登場アニメーションつき
// =====================================================================
public sealed class MagicCirclePlugin : IShapePlugin
{
    public string Name => "演出：魔法陣";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MagicCircleParameter(sharedData);
}

public sealed class MagicCircleParameter : EffectShapeParameterBase
{
    public MagicCircleParameter() : this(null) { }
    public MagicCircleParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "メインの色")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0x7F, 0xD6, 0xFF);

    [Display(GroupName = "色", Name = "サブの色（文字・点）")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0xFF, 0xD2, 0x7F);

    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "大きさ（半径）")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Radius { get; } = new Animation(300, 5, 5000);

    [Display(GroupName = "位置", Name = "傾き", Description = "大きくすると、床に描かれた魔法陣のように平たく見える")]
    [AnimationSlider("F0", "°", 0, 80)]
    public Animation Tilt { get; } = new Animation(0, 0, 89);

    [Display(GroupName = "模様", Name = "星形の頂点の数")]
    [TextBoxSlider("F0", "個", 3, 12)]
    [Range(3, 12)]
    public int PolyPoints { get => points; set => Set(ref points, value); }
    int points = 6;

    [Display(GroupName = "模様", Name = "星形の飛ばし数", Description = "2で六芒星・五芒星のような形になる")]
    [TextBoxSlider("F0", "", 1, 5)]
    [Range(1, 5)]
    public int StarSkip { get => skip; set => Set(ref skip, value); }
    int skip = 2;

    [Display(GroupName = "模様", Name = "ルーン文字の数")]
    [AnimationSlider("F0", "個", 0, 72)]
    public Animation RuneCount { get; } = new Animation(36, 0, 200);

    [Display(GroupName = "模様", Name = "周回する光の数")]
    [AnimationSlider("F0", "個", 0, 8)]
    public Animation Orbs { get; } = new Animation(3, 0, 30);

    [Display(GroupName = "光", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 8)]
    public Animation Thickness { get; } = new Animation(2.2, 0.2, 100);

    [Display(GroupName = "光", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "光", Name = "舞う光の粒の数")]
    [AnimationSlider("F0", "個", 0, 200)]
    public Animation DustCount { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "動き", Name = "回転速度")]
    [AnimationSlider("F0", "°/s", -90, 90)]
    public Animation Speed { get; } = new Animation(12, -3600, 3600);

    [Display(GroupName = "動き", Name = "書き上がる登場アニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "登場にかかる時間")]
    [AnimationSlider("F1", "秒", 0.3, 6)]
    public Animation IntroTime { get; } = new Animation(2, 0.1, 30);

    [Display(GroupName = "動き", Name = "出現度（手動）", Description = "0→100%で書き上がる。キーフレームで消すこともできる")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Appear { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Tilt, RuneCount, Orbs, Thickness, Brightness, DustCount, Speed, IntroTime, Appear];

    // 円（またはその一部）の点列
    static List<Vector2> Arc(Vector2 c, float r, float rot, float prog, int n = 120)
    {
        var pts = new List<Vector2>();
        int m = Math.Max(2, (int)(n * Math.Clamp(prog, 0f, 1f)));
        for (int i = 0; i <= m; i++)
        {
            float a = rot + i / (float)n * MathF.PI * 2f;
            pts.Add(c + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r);
        }
        return pts;
    }

    // ネオン風の線（にじみ→本体→芯）
    static void Glow(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, List<Vector2> pts, WpfColor col, float w, float a, bool joints)
    {
        var core = Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.65f);
        brush.Color = Gfx.C4(col, a * 0.07f);
        FormKit.Stroke(dc, brush, pts, w * 5f, joints);
        brush.Color = Gfx.C4(col, a * 0.18f);
        FormKit.Stroke(dc, brush, pts, w * 2.6f, joints);
        brush.Color = Gfx.C4(col, a * 0.85f);
        FormKit.Stroke(dc, brush, pts, w, joints);
        brush.Color = Gfx.C4(core, a * 0.7f);
        FormKit.Stroke(dc, brush, pts, w * 0.4f, joints);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float appear = c.F(Appear) / 100f;
        if (Intro) appear *= Gfx.Smooth(0f, MathF.Max(0.1f, c.F(IntroTime)), t);
        if (appear <= 0.002f) return;

        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), th = c.F(Thickness);
        float bright = c.F(Brightness) / 100f * (0.92f + 0.08f * MathF.Sin(t * 3f));
        float rot = Gfx.Rad(c.F(Speed)) * t;
        float tilt = c.F(Tilt);
        int pn = Math.Clamp(PolyPoints, 3, 12);
        int sk = Math.Clamp(StarSkip, 1, 5);
        if (sk * 2 > pn && pn > 2) sk = Math.Max(1, pn / 2);

        float Stage(float s0, float len) => Gfx.Smooth(s0, s0 + len, appear);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // 足元の光
            float gr = R * 1.15f;
            using (var gb = Gfx.Radial(dc, ctr, gr, (0f, Gfx.C4(Color1, 0.14f * bright * appear)), (1f, Gfx.C4(Color1, 0f))))
            {
                dc.Transform = Matrix3x2.CreateScale(1f, MathF.Cos(Gfx.Rad(tilt)), ctr);
                dc.FillEllipse(new Ellipse(ctr, gr, gr), gb);
            }

            // 魔法陣本体は、傾けると楕円（床に描かれた見た目）になる
            dc.Transform = Matrix3x2.CreateScale(1f, MathF.Cos(Gfx.Rad(tilt)), ctr);

            // 1. 外側の二重の輪
            Glow(dc, brush, Arc(ctr, R, rot * 0.2f, Stage(0f, 0.45f)), Color1, th * 1.8f, bright, false);
            Glow(dc, brush, Arc(ctr, R * 0.955f, -rot * 0.2f, Stage(0.05f, 0.45f)), Color2, th * 0.8f, bright * 0.9f, false);

            // 2. ルーン文字の帯
            float rIn = R * 0.80f, rOut = R * 0.925f;
            Glow(dc, brush, Arc(ctr, rIn, 0f, Stage(0.1f, 0.4f)), Color1, th * 0.9f, bright * 0.9f, false);
            Glow(dc, brush, Arc(ctr, rOut, 0f, Stage(0.1f, 0.4f)), Color1, th * 0.9f, bright * 0.9f, false);
            int runes = (int)c.F(RuneCount);
            float runeShow = Stage(0.25f, 0.4f);
            float cs = R * 0.04f;
            for (int i = 0; i < runes; i++)
            {
                if (i / (float)Math.Max(1, runes) > runeShow) break;
                float a = -rot * 0.3f + i * MathF.PI * 2f / runes;
                var radial = new Vector2(MathF.Cos(a), MathF.Sin(a));
                var tang = new Vector2(-radial.Y, radial.X);
                var origin = ctr + radial * ((rIn + rOut) / 2f);
                var rg = new Random(Seed * 977 + i);
                int segs = 2 + rg.Next(0, 3);
                for (int s = 0; s < segs; s++)
                {
                    float u0 = (rg.Next(0, 3) - 1) * cs * 0.6f, v0 = (rg.Next(0, 4) - 1.5f) * cs * 0.8f;
                    float u1 = (rg.Next(0, 3) - 1) * cs * 0.6f, v1 = (rg.Next(0, 4) - 1.5f) * cs * 0.8f;
                    if (u0 == u1 && v0 == v1) v1 += cs * 0.6f;
                    var p0 = origin + tang * u0 + radial * v0;
                    var p1 = origin + tang * u1 + radial * v1;
                    brush.Color = Gfx.C4(Color2, 0.9f * bright);
                    dc.DrawLine(p0, p1, brush, th * 0.9f);
                }
            }

            // 3. 破線の輪
            var dashRing = Arc(ctr, R * 0.74f, 0f, Stage(0.3f, 0.4f));
            brush.Color = Gfx.C4(Color1, 0.8f * bright);
            FormKit.Dash(dc, brush, dashRing, 14f, 8f, t * 20f, th);

            // 4. 星形（頂点を飛ばしてつなぐ）
            float starShow = Stage(0.4f, 0.4f);
            float sr = R * 0.72f;
            var verts = new List<Vector2>();
            for (int i = 0; i < pn; i++)
            {
                float a = -MathF.PI / 2f + rot * 0.5f + i * MathF.PI * 2f / pn;
                verts.Add(ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * sr);
            }
            int shown = (int)MathF.Ceiling(pn * starShow);
            for (int i = 0; i < shown; i++)
            {
                var seg = new List<Vector2> { verts[i], verts[(i + sk) % pn] };
                Glow(dc, brush, seg, Color1, th, bright, true);
            }
            if (starShow > 0.3f)
                foreach (var v in verts)
                {
                    brush.Color = Gfx.C4(Color2, 0.9f * bright);
                    dc.DrawEllipse(new Ellipse(v, R * 0.03f, R * 0.03f), brush, th);
                }

            // 5. 内側の輪と多角形
            Glow(dc, brush, Arc(ctr, R * 0.40f, 0f, Stage(0.55f, 0.35f)), Color1, th, bright, false);
            var inner = Arc(ctr, R * 0.40f, -rot * 0.8f, 1f, pn);
            if (inner.Count > 1 && Stage(0.6f, 0.3f) > 0.2f)
                Glow(dc, brush, inner, Color2, th * 0.8f, bright * 0.9f, true);

            // 6. 中心の印
            float cp = Stage(0.7f, 0.3f);
            if (cp > 0f)
            {
                Glow(dc, brush, Arc(ctr, R * 0.18f, rot, cp), Color1, th, bright, false);
                for (int i = 0; i < 8; i++)
                {
                    float a = rot * 1.2f + i * MathF.PI / 4f;
                    var dir = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    brush.Color = Gfx.C4(Color2, 0.9f * bright * cp);
                    dc.DrawLine(ctr + dir * R * 0.18f, ctr + dir * R * (i % 2 == 0 ? 0.32f : 0.26f), brush, th);
                }
                float cg = R * 0.14f;
                using var cb = Gfx.Radial(dc, ctr, cg, (0f, Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.55f * bright * cp)), (1f, Gfx.C4(Color1, 0f)));
                dc.FillEllipse(new Ellipse(ctr, cg, cg), cb);
            }

            // 7. 周回する光
            int orbs = (int)c.F(Orbs);
            float orbShow = Stage(0.75f, 0.25f);
            for (int i = 0; i < orbs; i++)
            {
                float a = rot * 1.6f + i * MathF.PI * 2f / orbs;
                var p = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * R * 0.56f;
                brush.Color = Gfx.C4(Color2, 0.18f * bright * orbShow);
                dc.FillEllipse(new Ellipse(p, R * 0.07f, R * 0.07f), brush);
                brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.9f * bright * orbShow);
                dc.FillEllipse(new Ellipse(p, R * 0.018f, R * 0.018f), brush);
            }

            dc.Transform = Matrix3x2.Identity;

            // 8. 立ちのぼる光の粒
            int dn = (int)c.F(DustCount);
            var rd = new Random(Seed + 5);
            float ty = MathF.Cos(Gfx.Rad(tilt));
            for (int i = 0; i < dn; i++)
            {
                float a = rd.NextF() * MathF.PI * 2f;
                float rr = R * (0.15f + 0.8f * MathF.Sqrt(rd.NextF()));
                float ph = rd.NextF(), sp = 0.15f + 0.3f * rd.NextF();
                float u = Gfx.Mod(t * sp + ph, 1f);
                var p = ctr + new Vector2(MathF.Cos(a) * rr, MathF.Sin(a) * rr * ty) - new Vector2(0f, u * R * 0.8f);
                float al = MathF.Sin(u * MathF.PI) * bright * appear;
                brush.Color = Gfx.C4(Color2, 0.7f * al);
                dc.FillEllipse(new Ellipse(p, 1.8f, 1.8f), brush);
                brush.Color = Gfx.C4(Color2, 0.15f * al);
                dc.FillEllipse(new Ellipse(p, 5f, 5f), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
