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

public enum DnaMode
{
    [Display(Name = "二重らせん")] Helix,
    [Display(Name = "分子構造（結合する）")] Molecule,
    [Display(Name = "両方")] Both,
}

// =====================================================================
//  DNAの螺旋・分子構造
//   回る二重らせんと、結合する分子
// =====================================================================
public sealed class DnaMoleculePlugin : IShapePlugin
{
    public string Name => "演出：DNAの螺旋・分子構造";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DnaMoleculeParameter(sharedData);
}

public sealed class DnaMoleculeParameter : EffectShapeParameterBase
{
    public DnaMoleculeParameter() : this(null) { }
    public DnaMoleculeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "表示するもの")]
    [EnumComboBox]
    public DnaMode Mode { get => mode; set => Set(ref mode, value); }
    DnaMode mode = DnaMode.Helix;

    [Display(GroupName = "色", Name = "色1")]
    [ColorPicker]
    public WpfColor Color1 { get => color1; set => Set(ref color1, value); }
    WpfColor color1 = WpfColor.FromRgb(0x3C, 0xC8, 0xFF);

    [Display(GroupName = "色", Name = "色2")]
    [ColorPicker]
    public WpfColor Color2 { get => color2; set => Set(ref color2, value); }
    WpfColor color2 = WpfColor.FromRgb(0xFF, 0x5C, 0x8A);

    [Display(GroupName = "色", Name = "結合の色")]
    [ColorPicker]
    public WpfColor BondColor { get => bondColor; set => Set(ref bondColor, value); }
    WpfColor bondColor = WpfColor.FromRgb(0xE8, 0xF0, 0xFF);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "らせん", Name = "長さ")]
    [AnimationSlider("F0", "px", 100, 2000)]
    public Animation Length { get; } = new Animation(900, 10, 10000);

    [Display(GroupName = "らせん", Name = "太さ（半径）")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation HelixRadius { get; } = new Animation(90, 2, 3000);

    [Display(GroupName = "らせん", Name = "巻き数")]
    [AnimationSlider("F1", "回", 0.5, 8)]
    public Animation Turns { get; } = new Animation(3, 0.1, 50);

    [Display(GroupName = "らせん", Name = "向き")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(-20, -3600, 3600);

    [Display(GroupName = "らせん", Name = "回る速さ")]
    [AnimationSlider("F0", "°/秒", -360, 360)]
    public Animation Spin { get; } = new Animation(70, -7200, 7200);

    [Display(GroupName = "分子", Name = "原子の大きさ")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation AtomSize { get; } = new Animation(38, 2, 1000);

    [Display(GroupName = "分子", Name = "結合する周期")]
    [AnimationSlider("F1", "秒", 1, 12)]
    public Animation BondCycle { get; } = new Animation(4, 0.3, 120);

    [Display(GroupName = "装飾", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glow { get; } = new Animation(100, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Length, HelixRadius, Turns, Angle, Spin, AtomSize, BondCycle, Glow];

    readonly record struct Ball(Vector2 P, float Z, float R, WpfColor Col, float A);

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float glow = c.F(Glow) / 100f;
        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (Mode != DnaMode.Molecule)
                DrawHelix(dc, brush, ctr, c.F(Length), c.F(HelixRadius), c.F(Turns), Gfx.Rad(c.F(Angle)), Gfx.Rad(c.F(Spin) * t), glow);
            if (Mode != DnaMode.Helix)
            {
                var mc = Mode == DnaMode.Both ? ctr + new Vector2(c.F(Length) * 0.38f, c.F(HelixRadius) * 2.6f) : ctr;
                DrawMolecules(dc, brush, mc, c.F(AtomSize), MathF.Max(0.3f, c.F(BondCycle)), t, glow);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    void DrawHelix(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float L, float R, float turns, float ang, float spin, float glow)
    {
        var u = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
        var nrm = new Vector2(-u.Y, u.X);
        int N = Math.Max(12, (int)(turns * 24f));
        var p1 = new (Vector2 p, float z)[N + 1];
        var p2 = new (Vector2 p, float z)[N + 1];
        for (int k = 0; k <= N; k++)
        {
            float s = k / (float)N;
            float along = (s - 0.5f) * L;
            float ph = s * turns * MathF.PI * 2f + spin;
            p1[k] = (ctr + u * along + nrm * (R * MathF.Sin(ph)), MathF.Cos(ph));
            p2[k] = (ctr + u * along + nrm * (R * MathF.Sin(ph + MathF.PI)), MathF.Cos(ph + MathF.PI));
        }
        // 端は薄く
        float End(int k) => Gfx.Smooth(0f, 0.12f, k / (float)N) * Gfx.Smooth(1f, 0.88f, k / (float)N);

        // 奥 → 手前の順に描くため、2回に分ける
        for (int pass = 0; pass < 2; pass++)
        {
            bool front = pass == 1;
            // 横木（塩基対）
            for (int k = 0; k <= N; k += 2)
            {
                var a = p1[k].p;
                var b = p2[k].p;
                var mid = Vector2.Lerp(a, b, 0.5f);
                float depth = 0.35f + 0.65f * (1f - MathF.Abs(p1[k].z - p2[k].z) / 2f);
                if (pass == 0)
                {
                    float al = 0.55f * depth * End(k);
                    brush.Color = Gfx.C4(Color1, al);
                    dc.DrawLine(a, mid, brush, R * 0.07f + 1f);
                    brush.Color = Gfx.C4(Color2, al);
                    dc.DrawLine(mid, b, brush, R * 0.07f + 1f);
                }
            }
            // 背骨
            DrawStrand(dc, brush, p1, front, Color1, R, End);
            DrawStrand(dc, brush, p2, front, Color2, R, End);
        }
        if (glow > 0f)
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            for (int k = 0; k <= N; k += 3)
            {
                if (p1[k].z > 0.3f) DrawKit.Glow(dc, p1[k].p, R * 0.35f, Gfx.C4(Color1, 0.18f * glow * End(k)));
                if (p2[k].z > 0.3f) DrawKit.Glow(dc, p2[k].p, R * 0.35f, Gfx.C4(Color2, 0.18f * glow * End(k)));
            }
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    static void DrawStrand(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, (Vector2 p, float z)[] pts, bool front, WpfColor col, float R, Func<int, float> end)
    {
        for (int k = 0; k < pts.Length - 1; k++)
        {
            float z = (pts[k].z + pts[k + 1].z) / 2f;
            if ((z >= 0f) != front) continue;
            float depth = 0.5f + 0.5f * z;
            var cc = Gfx.Mix(Gfx.Mix(col, WpfColor.FromRgb(10, 14, 30), 0.6f * (1f - depth)), WpfColor.FromRgb(255, 255, 255), 0.25f * depth);
            brush.Color = Gfx.C4(cc, (0.35f + 0.65f * depth) * end(k));
            float wdt = R * (0.09f + 0.07f * depth);
            dc.DrawLine(pts[k].p, pts[k + 1].p, brush, wdt);
            dc.FillEllipse(new Ellipse(pts[k + 1].p, wdt / 2f, wdt / 2f), brush);
            if (k % 2 == 0 && front)
            {
                brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.5f * depth * end(k));
                dc.FillEllipse(new Ellipse(pts[k].p, wdt * 0.25f, wdt * 0.25f), brush);
            }
        }
    }

    void DrawMolecules(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float A, float cycle, float t, float glow)
    {
        float p = DrawKit.Frac(t / cycle);
        float approach = Gfx.Smooth(0.05f, 0.5f, p);
        float sep = A * (5.5f - 3.1f * approach);               // 分子どうしの距離
        bool bonded = p >= 0.5f;
        float flash = bonded ? MathF.Exp(-(p - 0.5f) * cycle * 4f) : 0f;
        float rotA = t * 0.9f, rotB = -t * 0.7f + 1f;
        var balls = new List<Ball>();
        var bonds = new List<(int a, int b, float alpha)>();

        // 分子A（中心＋3つ）、分子B（中心＋2つ）
        void Add(Vector2 center, Vector3[] local, float rot, WpfColor main, WpfColor sub)
        {
            int baseIdx = balls.Count;
            for (int i = 0; i < local.Length; i++)
            {
                var v = local[i];
                float cs = MathF.Cos(rot), sn = MathF.Sin(rot);
                var r3 = new Vector3(v.X * cs + v.Z * sn, v.Y, -v.X * sn + v.Z * cs);
                float tilt = 0.4f, ct = MathF.Cos(tilt), st = MathF.Sin(tilt);
                var r4 = new Vector3(r3.X, r3.Y * ct - r3.Z * st, r3.Y * st + r3.Z * ct);
                float persp = 1f + r4.Z * 0.15f;
                balls.Add(new Ball(center + new Vector2(r4.X, r4.Y) * A * persp, r4.Z, A * (i == 0 ? 0.62f : 0.42f) * persp, i == 0 ? main : sub, 1f));
                if (i > 0) bonds.Add((baseIdx, baseIdx + i, 1f));
            }
        }
        var tetra = new[] { Vector3.Zero, new Vector3(0, -1.6f, 0), new Vector3(1.5f, 0.55f, 0.5f), new Vector3(-1.5f, 0.55f, 0.5f), new Vector3(0, 0.55f, -1.55f) };
        var bent = new[] { Vector3.Zero, new Vector3(1.4f, 0.9f, 0), new Vector3(-1.4f, 0.9f, 0) };
        var ca = ctr - new Vector2(sep / 2f, 0);
        var cb = ctr + new Vector2(sep / 2f, 0);
        Add(ca, tetra, rotA, Color1, BondColor);
        int bStart = balls.Count;
        Add(cb, bent, rotB, Color2, BondColor);
        // 結合：A と B の中心を結ぶ
        if (bonded) bonds.Add((0, bStart, 1f));
        else if (approach > 0.6f) bonds.Add((0, bStart, (approach - 0.6f) / 0.4f * 0.4f));

        // 奥から順に
        var order = Enumerable.Range(0, balls.Count).OrderBy(i => balls[i].Z).ToList();
        foreach (var (a, b, alpha) in bonds)
        {
            var pa = balls[a].P;
            var pb = balls[b].P;
            float z = (balls[a].Z + balls[b].Z) / 2f;
            brush.Color = Gfx.C4(BondColor, alpha * (0.45f + 0.35f * (z + 1f) / 2f));
            bool between = (a == 0 && b == bStart);
            if (between && !bonded) FormKit.Dash(dc, brush, new List<Vector2> { pa, pb }, 8f, 8f, t * 30f, A * 0.12f);
            else dc.DrawLine(pa, pb, brush, A * (between ? 0.2f : 0.16f));
        }
        foreach (var i in order)
        {
            var bl = balls[i];
            float depth = 0.5f + 0.5f * bl.Z;
            var hl = bl.P + new Vector2(-bl.R * 0.35f, -bl.R * 0.4f);
            using var g = Gfx.Radial(dc, hl, bl.R * 1.35f,
                (0f, Gfx.C4(Gfx.Mix(bl.Col, WpfColor.FromRgb(255, 255, 255), 0.75f), 1f)),
                (0.35f, Gfx.C4(bl.Col, 1f)),
                (1f, Gfx.C4(Gfx.Mix(bl.Col, WpfColor.FromRgb(5, 8, 20), 0.7f - 0.3f * depth), 1f)));
            dc.FillEllipse(new Ellipse(bl.P, bl.R, bl.R), g);
        }
        if (flash > 0.01f || glow > 0f)
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            var mid = Vector2.Lerp(balls[0].P, balls[bStart].P, 0.5f);
            if (flash > 0.01f)
            {
                DrawKit.Glow(dc, mid, A * 4f * (1.5f - flash * 0.5f), Gfx.C4(BondColor, 0.8f * flash));
                brush.Color = Gfx.C4(BondColor, 0.9f * flash);
                float rr = A * (1f + 5f * (1f - flash));
                dc.DrawEllipse(new Ellipse(mid, rr, rr), brush, 3f);
            }
            DrawKit.Glow(dc, balls[0].P, A * 2f, Gfx.C4(Color1, 0.15f * glow));
            DrawKit.Glow(dc, balls[bStart].P, A * 2f, Gfx.C4(Color2, 0.15f * glow));
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
