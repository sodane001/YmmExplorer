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
//  シールド・バリア
//   六角形のセルが球面に張られた防御壁。攻撃が当たると波紋が広がる
// =====================================================================
public sealed class ShieldPlugin : IShapePlugin
{
    public string Name => "演出：シールド・バリア";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ShieldParameter(sharedData);
}

public sealed class ShieldParameter : EffectShapeParameterBase
{
    public ShieldParameter() : this(null) { }
    public ShieldParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "シールドの色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x59, 0xF0, 0xFF);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "半径")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Radius { get; } = new Animation(300, 5, 5000);

    [Display(GroupName = "セル", Name = "セルの大きさ")]
    [AnimationSlider("F0", "px", 15, 120)]
    public Animation CellSize { get; } = new Animation(40, 6, 1000);

    [Display(GroupName = "セル", Name = "セルの塗り")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FillAmount { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "セル", Name = "ゆらめき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Shimmer { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "被弾", Name = "自動で攻撃を受ける")]
    [ToggleSlider]
    public bool AutoHit { get => autoHit; set => Set(ref autoHit, value); }
    bool autoHit = true;

    [Display(GroupName = "被弾", Name = "攻撃の間隔")]
    [AnimationSlider("F1", "秒", 0.5, 10)]
    public Animation HitInterval { get; } = new Animation(1.8, 0.2, 60);

    [Display(GroupName = "被弾", Name = "波紋の広がる速さ")]
    [AnimationSlider("F0", "%", 20, 300)]
    public Animation RippleSpeed { get; } = new Animation(100, 5, 2000);

    [Display(GroupName = "被弾", Name = "展開アニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "被弾", Name = "展開にかかる時間")]
    [AnimationSlider("F2", "秒", 0.1, 4)]
    public Animation IntroTime { get; } = new Animation(0.8, 0.05, 30);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, CellSize, FillAmount, Shimmer, HitInterval, RippleSpeed, IntroTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float ap = Intro ? Gfx.Smooth(0f, MathF.Max(0.05f, c.F(IntroTime)), t) : 1f;
        if (ap <= 0.002f) return;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius) * (0.85f + 0.15f * ap);
        float s = MathF.Max(0.03f, c.F(CellSize) / R);          // 球面の単位円での、セルの半径
        float fillK = c.F(FillAmount) / 100f, shim = c.F(Shimmer) / 100f;
        float spd = c.F(RippleSpeed) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);

        // ---- 攻撃（直近の3回分を同時に表示）----
        var hits = new List<(Vector2 pos, float age)>();
        float P = MathF.Max(0.2f, c.F(HitInterval));
        const float life = 1.4f;
        if (AutoHit)
        {
            int cur = (int)MathF.Floor(t / P);
            for (int k = 0; k < 3; k++)
            {
                int idx = cur - k;
                float age = t - idx * P;
                if (idx < 0 || age < 0f || age > life) continue;
                var rh = new Random(Seed * 977 + idx * 131);
                float a = rh.NextF() * MathF.PI * 2f, m = 0.25f + 0.65f * rh.NextF();
                hits.Add((new Vector2(MathF.Cos(a), MathF.Sin(a)) * m, age));
            }
        }

        Vector2 Map(Vector2 v)       // 平面の単位円→球面を正面から見た位置
        {
            float rg = v.Length();
            if (rg < 0.0001f) return ctr;
            float rc = MathF.Min(rg, 1f);
            return ctr + v / rg * (MathF.Sin(rc * MathF.PI / 2f) * R);
        }

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 球面のうっすらした面と縁の光（フレネル）----
            using (var eb = Gfx.Radial(dc, ctr, R,
                (0f, Gfx.C4(Color, 0.02f * ap)), (0.7f, Gfx.C4(Color, 0.05f * ap)), (0.93f, Gfx.C4(Color, 0.22f * ap)), (1f, Gfx.C4(Color, 0.5f * ap))))
                dc.FillEllipse(new Ellipse(ctr, R, R), eb);

            // ---- 六角形のセル ----
            int N = (int)(1.2f / (s * 1.5f)) + 2;
            for (int q = -N; q <= N; q++)
                for (int r = -N; r <= N; r++)
                {
                    var cc = new Vector2(s * 1.7320508f * (q + r / 2f), s * 1.5f * r);
                    float rg = cc.Length();
                    if (rg > 1f - s * 0.3f) continue;

                    // セルごとの光り具合：ゆらめき + 縁の強調 + 攻撃の波紋
                    float hash = Gfx.Hash01(q * 7919 + r * 104729 + Seed);
                    float a = 0.10f + 0.10f * shim * MathF.Sin(t * 1.6f + hash * 6.28f) + 0.40f * MathF.Pow(rg, 3f);
                    foreach (var hit in hits)
                    {
                        float dd = (cc - hit.pos).Length();
                        float ring = hit.age * 0.9f * spd;
                        float e = MathF.Exp(-MathF.Pow((dd - ring) * 7f, 2f)) * (1f - hit.age / life);
                        a += 1.0f * e + 0.5f * MathF.Exp(-dd * 5f) * MathF.Max(0f, 1f - hit.age * 2.5f);
                    }
                    a = MathF.Min(1.2f, a) * ap;

                    var verts = new List<Vector2>();
                    for (int i = 0; i < 6; i++)
                    {
                        float ang = MathF.PI / 6f + i * MathF.PI / 3f;
                        verts.Add(Map(cc + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * (s * 0.94f)));
                    }
                    if (fillK > 0f)
                    {
                        brush.Color = Gfx.C4(Color, a * 0.30f * fillK);
                        Gfx.FillPoly(dc, brush, verts);
                    }
                    brush.Color = Gfx.C4(Color, MathF.Min(1f, a * 1.3f));
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(verts), 1.6f);
                }

            // ---- 縁の輪郭 ----
            brush.Color = Gfx.C4(Color, 0.6f * ap);
            dc.DrawEllipse(new Ellipse(ctr, R, R), brush, 2.5f);

            // ---- 着弾点の閃光 ----
            foreach (var hit in hits)
            {
                if (hit.age > 0.35f) continue;
                var p = Map(hit.pos);
                float fr = R * 0.28f, fa = 1f - hit.age / 0.35f;
                using var fb = Gfx.Radial(dc, p, fr, (0f, Gfx.C4(white, 0.9f * fa)), (0.35f, Gfx.C4(Color, 0.4f * fa)), (1f, Gfx.C4(Color, 0f)));
                dc.FillEllipse(new Ellipse(p, fr, fr), fb);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}

public enum PortalKind
{
    [Display(Name = "ポータル（渦を巻く光の輪）")] Portal,
    [Display(Name = "次元の裂け目（縦に裂ける）")] Rift,
}
