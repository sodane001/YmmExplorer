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
//  電撃・プラズマ
//   プラズマボール / 2点間の放電 / 画面に散る電撃
// =====================================================================
public enum PlasmaMode
{
    [Display(Name = "プラズマボール")] Ball,
    [Display(Name = "2点間の放電")] Arc,
    [Display(Name = "画面に散る電撃")] Scatter,
}

public sealed class PlasmaPlugin : IShapePlugin
{
    public string Name => "演出：電撃・プラズマ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PlasmaParameter(sharedData);
}

public sealed class PlasmaParameter : EffectShapeParameterBase
{
    public PlasmaParameter() : this(null) { }
    public PlasmaParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "電撃", Name = "種類")]
    [EnumComboBox]
    public PlasmaMode Mode { get => mode; set => Set(ref mode, value); }
    PlasmaMode mode = PlasmaMode.Ball;

    [Display(GroupName = "電撃", Name = "光の色")]
    [ColorPicker]
    public WpfColor GlowColor { get => glow; set => Set(ref glow, value); }
    WpfColor glow = WpfColor.FromRgb(0x8C, 0xC8, 0xFF);

    [Display(GroupName = "電撃", Name = "芯の色")]
    [ColorPicker]
    public WpfColor CoreColor { get => core; set => Set(ref core, value); }
    WpfColor core = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "電撃", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 12)]
    public Animation Thickness { get; } = new Animation(2.6, 0.2, 100);

    [Display(GroupName = "電撃", Name = "光のにじみ")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation GlowSize { get; } = new Animation(22, 0, 1000);

    [Display(GroupName = "電撃", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "電撃", Name = "ギザギザ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Jag { get; } = new Animation(32, 0, 200);

    [Display(GroupName = "電撃", Name = "枝分かれ")]
    [AnimationSlider("F0", "本", 0, 8)]
    public Animation Branches { get; } = new Animation(2, 0, 40);

    [Display(GroupName = "電撃", Name = "ちらつきの速さ")]
    [AnimationSlider("F0", "回/秒", 2, 40)]
    public Animation FlickerHz { get; } = new Animation(16, 1, 120);

    // ---- プラズマボール ----
    [Display(GroupName = "プラズマボール", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BallX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "プラズマボール", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BallY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "プラズマボール", Name = "半径")]
    [AnimationSlider("F0", "px", 50, 500)]
    public Animation BallRadius { get; } = new Animation(180, 5, 5000);

    [Display(GroupName = "プラズマボール", Name = "電撃の数")]
    [AnimationSlider("F0", "本", 1, 16)]
    public Animation BoltCount { get; } = new Animation(7, 1, 60);

    // ---- 2点間の放電 ----
    [Display(GroupName = "2点間の放電", Name = "始点X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation X1 { get; } = new Animation(25, -100, 200);

    [Display(GroupName = "2点間の放電", Name = "始点Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Y1 { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "2点間の放電", Name = "終点X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation X2 { get; } = new Animation(75, -100, 200);

    [Display(GroupName = "2点間の放電", Name = "終点Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Y2 { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "2点間の放電", Name = "放電の本数")]
    [AnimationSlider("F0", "本", 1, 5)]
    public Animation Strands { get; } = new Animation(3, 1, 20);

    // ---- 画面に散る電撃 ----
    [Display(GroupName = "画面に散る電撃", Name = "数")]
    [AnimationSlider("F0", "本", 1, 30)]
    public Animation ScatterCount { get; } = new Animation(6, 1, 200);

    [Display(GroupName = "画面に散る電撃", Name = "長さ")]
    [AnimationSlider("F0", "px", 50, 800)]
    public Animation ScatterLength { get; } = new Animation(260, 5, 8000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Thickness, GlowSize, Brightness, Jag, Branches, FlickerHz,
         BallX, BallY, BallRadius, BoltCount, X1, Y1, X2, Y2, Strands, ScatterCount, ScatterLength];

    // 中点変位法で、自然なギザギザの電撃を作る
    static List<Vector2> Bolt(Random rng, Vector2 a, Vector2 b, float jagK, int levels)
    {
        var pts = new List<Vector2> { a, b };
        float disp = (b - a).Length() * jagK;
        for (int lv = 0; lv < levels; lv++)
        {
            var np = new List<Vector2>(pts.Count * 2);
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var p0 = pts[i];
                var p1 = pts[i + 1];
                var mid = (p0 + p1) / 2f;
                var d = p1 - p0;
                if (d.LengthSquared() > 0.0001f)
                {
                    var nrm = Vector2.Normalize(new Vector2(-d.Y, d.X));
                    mid += nrm * ((rng.NextF() * 2f - 1f) * disp);
                }
                np.Add(p0);
                np.Add(mid);
            }
            np.Add(pts[pts.Count - 1]);
            pts = np;
            disp *= 0.55f;
        }
        return pts;
    }

    void DrawBolt(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, List<Vector2> pts, float th, float gw, float bright)
    {
        if (gw > 0f)
        {
            brush.Color = Gfx.C4(GlowColor, 0.06f * bright);
            FormKit.Stroke(dc, brush, pts, gw * 2.2f, false);
            brush.Color = Gfx.C4(GlowColor, 0.14f * bright);
            FormKit.Stroke(dc, brush, pts, gw * 1.1f, false);
        }
        brush.Color = Gfx.C4(GlowColor, 0.6f * bright);
        FormKit.Stroke(dc, brush, pts, th * 2.4f, false);
        brush.Color = Gfx.C4(CoreColor, 0.95f * bright);
        FormKit.Stroke(dc, brush, pts, th, false);
    }

    // 主電撃 + 枝分かれ
    void DrawWithBranches(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Random rng, Vector2 a, Vector2 b,
        float jagK, int branches, float th, float gw, float bright)
    {
        var main = Bolt(rng, a, b, jagK, 5);
        DrawBolt(dc, brush, main, th, gw, bright);
        float len = (b - a).Length();
        for (int i = 0; i < branches; i++)
        {
            int idx = rng.Next(main.Count / 5, main.Count - main.Count / 5);
            var s = main[idx];
            var dirMain = Vector2.Normalize(b - a);
            float rot = (rng.NextF() < 0.5f ? -1f : 1f) * (0.35f + 0.6f * rng.NextF());
            float cs = MathF.Cos(rot), sn = MathF.Sin(rot);
            var d = new Vector2(dirMain.X * cs - dirMain.Y * sn, dirMain.X * sn + dirMain.Y * cs);
            var e = s + d * (len * (0.15f + 0.25f * rng.NextF()));
            DrawBolt(dc, brush, Bolt(rng, s, e, jagK, 3), th * 0.55f, gw * 0.5f, bright * 0.8f);
        }
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float th = c.F(Thickness), gw = c.F(GlowSize), bright = c.F(Brightness) / 100f;
        float jagK = c.F(Jag) / 100f * 0.35f;
        int branches = (int)c.F(Branches);
        float hz = MathF.Max(1f, c.F(FlickerHz));
        int tick = (int)MathF.Floor(t * hz);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            switch (Mode)
            {
                case PlasmaMode.Ball:
                    {
                        var ctr = new Vector2(c.F(BallX) / 100f * w, c.F(BallY) / 100f * h);
                        float R = c.F(BallRadius);
                        int nb = (int)c.F(BoltCount);

                        // 外側のにじみ
                        float og = R * 1.7f;
                        using (var ob = Gfx.Radial(dc, ctr, og, (0f, Gfx.C4(GlowColor, 0.16f * bright)), (1f, Gfx.C4(GlowColor, 0f))))
                            dc.FillEllipse(new Ellipse(ctr, og, og), ob);

                        // ガラス球（縁が明るい）
                        using (var sb = Gfx.Radial(dc, ctr, R,
                            (0f, Gfx.C4(GlowColor, 0.10f * bright)), (0.8f, Gfx.C4(GlowColor, 0.05f * bright)), (1f, Gfx.C4(GlowColor, 0.28f * bright))))
                            dc.FillEllipse(new Ellipse(ctr, R, R), sb);

                        // 中心の電極
                        float cg = R * 0.22f;
                        using (var cb = Gfx.Radial(dc, ctr, cg, (0f, Gfx.C4(white, 0.9f * bright)), (0.5f, Gfx.C4(GlowColor, 0.4f * bright)), (1f, Gfx.C4(GlowColor, 0f))))
                            dc.FillEllipse(new Ellipse(ctr, cg, cg), cb);

                        for (int i = 0; i < nb; i++)
                        {
                            var rng = new Random(Seed * 131 + tick * 7919 + i * 17);
                            // 終点はゆっくり回りながら、ときどき跳ぶ
                            float ang = i * MathF.PI * 2f / nb + t * 0.4f + (rng.NextF() - 0.5f) * 0.5f;
                            var end = ctr + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * (R * 0.97f);
                            DrawWithBranches(dc, brush, rng, ctr, end, jagK, branches, th, gw, bright);
                            // 球面に当たる光点
                            brush.Color = Gfx.C4(GlowColor, 0.5f * bright);
                            dc.FillEllipse(new Ellipse(end, 8f, 8f), brush);
                            brush.Color = Gfx.C4(white, 0.9f * bright);
                            dc.FillEllipse(new Ellipse(end, 2.5f, 2.5f), brush);
                        }

                        // ガラスの反射
                        brush.Color = Gfx.C4(white, 0.28f);
                        dc.FillEllipse(new Ellipse(ctr + new Vector2(-R * 0.4f, -R * 0.5f), R * 0.2f, R * 0.1f), brush);
                        break;
                    }
                case PlasmaMode.Arc:
                    {
                        var a = new Vector2(c.F(X1) / 100f * w, c.F(Y1) / 100f * h);
                        var b = new Vector2(c.F(X2) / 100f * w, c.F(Y2) / 100f * h);
                        int ns = (int)c.F(Strands);
                        for (int i = 0; i < ns; i++)
                        {
                            var rng = new Random(Seed * 131 + tick * 7919 + i * 17);
                            float k = i == 0 ? 1f : 0.65f;
                            DrawWithBranches(dc, brush, rng, a, b, jagK * (i == 0 ? 1f : 1.4f), i == 0 ? branches : branches / 2, th * k, gw * k, bright * k);
                        }
                        foreach (var p in new[] { a, b })
                        {
                            float r = 40f + gw;
                            using var eb = Gfx.Radial(dc, p, r, (0f, Gfx.C4(white, 0.8f * bright)), (0.35f, Gfx.C4(GlowColor, 0.35f * bright)), (1f, Gfx.C4(GlowColor, 0f)));
                            dc.FillEllipse(new Ellipse(p, r, r), eb);
                        }
                        break;
                    }
                default:
                    {
                        int cnt = (int)c.F(ScatterCount);
                        float len = c.F(ScatterLength);
                        for (int i = 0; i < cnt; i++)
                        {
                            var rng = new Random(Seed * 131 + tick * 7919 + i * 31);
                            if (rng.NextF() < 0.25f) continue;                       // 一部は消えている
                            var s = new Vector2(rng.NextF() * w, rng.NextF() * h);
                            float ang = rng.NextF() * MathF.PI * 2f;
                            var e = s + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * (len * (0.5f + 0.5f * rng.NextF()));
                            DrawWithBranches(dc, brush, rng, s, e, jagK, branches, th, gw, bright);
                            brush.Color = Gfx.C4(GlowColor, 0.35f * bright);
                            dc.FillEllipse(new Ellipse(s, 10f, 10f), brush);
                        }
                        break;
                    }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
