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
//  氷
//   画面の縁から枝分かれする氷の結晶が伸びる。霜のもやと反射のきらめきつき
// =====================================================================
public sealed class IcePlugin : IShapePlugin
{
    public string Name => "演出：氷";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new IceParameter(sharedData);
}

public sealed class IceParameter : EffectShapeParameterBase
{
    public IceParameter() : this(null) { }
    public IceParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "氷の色")]
    [ColorPicker]
    public WpfColor IceColor { get => ice; set => Set(ref ice, value); }
    WpfColor ice = WpfColor.FromRgb(0xBF, 0xEF, 0xFF);

    [Display(GroupName = "結晶", Name = "結晶の元の数")]
    [AnimationSlider("F0", "本", 2, 40)]
    public Animation Roots { get; } = new Animation(16, 1, 200);

    [Display(GroupName = "結晶", Name = "最初の枝の長さ")]
    [AnimationSlider("F0", "px", 40, 500)]
    public Animation BranchLength { get; } = new Animation(170, 5, 3000);

    [Display(GroupName = "結晶", Name = "枝分かれの段数")]
    [TextBoxSlider("F0", "段", 1, 6)]
    [Range(1, 6)]
    public int Depth { get => depth; set => Set(ref depth, value); }
    int depth = 5;

    [Display(GroupName = "結晶", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 8)]
    public Animation Thickness { get; } = new Animation(3, 0.2, 100);

    [Display(GroupName = "霜", Name = "霜のもや")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Haze { get; } = new Animation(55, 0, 100);

    [Display(GroupName = "霜", Name = "きらめき")]
    [AnimationSlider("F0", "個", 0, 200)]
    public Animation Sparkles { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "動き", Name = "凍りつく登場アニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "凍りつくまでの時間")]
    [AnimationSlider("F1", "秒", 0.5, 15)]
    public Animation IntroTime { get; } = new Animation(4, 0.1, 120);

    [Display(GroupName = "動き", Name = "凍結度（手動）", Description = "0→100%で凍りつく。キーフレームで溶かすこともできる")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Growth { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Roots, BranchLength, Thickness, Haze, Sparkles, IntroTime, Growth];

    // 枝を再帰的に伸ばす。乱数は表示の有無に関わらず同じ順に消費する（形が毎フレーム変わらないように）
    void Branch(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Random rng, Vector2 p, float ang, float len,
        int depth, int maxDepth, float g, float tStart, float th)
    {
        float len2 = len * (0.8f + 0.4f * rng.NextF());
        float bend = (rng.NextF() - 0.5f) * 0.25f;
        float a2 = ang + bend;
        var dir = new Vector2(MathF.Cos(a2), MathF.Sin(a2));

        int kids = depth < maxDepth ? (rng.NextF() < 0.35f ? 3 : 2) : 0;
        var kt = new float[kids];
        var ka = new float[kids];
        for (int k = 0; k < kids; k++)
        {
            kt[k] = 0.35f + 0.55f * rng.NextF();
            ka[k] = (rng.NextF() < 0.5f ? -1f : 1f) * (0.5f + 0.5f * rng.NextF());
        }

        const float dur = 0.22f;
        float f = Math.Clamp((g - tStart) / dur, 0f, 1f);
        if (f > 0f)
        {
            var end = p + dir * (len2 * f);
            brush.Color = Gfx.C4(IceColor, 0.16f);
            dc.DrawLine(p, end, brush, th * 4f);
            brush.Color = Gfx.C4(IceColor, 0.85f);
            dc.DrawLine(p, end, brush, th);
            brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.7f);
            dc.DrawLine(p, end, brush, MathF.Max(0.6f, th * 0.35f));
        }
        for (int k = 0; k < kids; k++)
        {
            var cp = p + dir * (len2 * kt[k]);
            Branch(dc, brush, rng, cp, a2 + ka[k], len2 * 0.62f, depth + 1, maxDepth, g, tStart + dur * kt[k] + 0.03f, th * 0.68f);
        }
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float g = c.F(Growth) / 100f;
        if (Intro) g *= Gfx.Smooth(0f, MathF.Max(0.1f, c.F(IntroTime)), t);
        if (g <= 0.002f) return;

        int roots = (int)c.F(Roots), maxDepth = Math.Clamp(Depth, 1, 6);
        float blen = c.F(BranchLength), th = c.F(Thickness);
        float haze = c.F(Haze) / 100f * Math.Min(1f, g * 1.6f);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 縁の霜のもや ----
        if (haze > 0f)
        {
            float d = MathF.Min(w, h) * 0.18f;
            void Edge(Vector2 a, Vector2 b, float rx, float ry, float rw, float rh)
            {
                using var gb = Gfx.Linear(dc, a, b, (0f, Gfx.C4(IceColor, 0.55f * haze)), (1f, Gfx.C4(IceColor, 0f)));
                Gfx.Rect(dc, gb, rx, ry, rw, rh);
            }
            Edge(new Vector2(0, 0), new Vector2(0, d), 0, 0, w, d);
            Edge(new Vector2(0, h), new Vector2(0, h - d), 0, h - d, w, d);
            Edge(new Vector2(0, 0), new Vector2(d, 0), 0, 0, d, h);
            Edge(new Vector2(w, 0), new Vector2(w - d, 0), w - d, 0, d, h);
        }

        // ---- 氷の結晶（四辺から内側へ伸びる）----
        var rng = new Random(Seed);
        for (int i = 0; i < roots; i++)
        {
            int side = i % 4;
            float u = (i / 4 + 0.5f + (rng.NextF() - 0.5f) * 0.6f) / MathF.Ceiling(roots / 4f);
            Vector2 p; float ang;
            switch (side)
            {
                case 0: p = new Vector2(u * w, 0); ang = MathF.PI / 2f; break;
                case 1: p = new Vector2(u * w, h); ang = -MathF.PI / 2f; break;
                case 2: p = new Vector2(0, u * h); ang = 0f; break;
                default: p = new Vector2(w, u * h); ang = MathF.PI; break;
            }
            ang += (rng.NextF() - 0.5f) * 0.5f;
            float start = (i % 5) * 0.03f;
            Branch(dc, brush, rng, p, ang, blen * (0.8f + 0.4f * rng.NextF()), 0, maxDepth, g, start, th);
        }

        // ---- 氷の反射のきらめき ----
        int sn = (int)c.F(Sparkles);
        var rs = new Random(Seed + 77);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            for (int i = 0; i < sn; i++)
            {
                float x = rs.NextF() * w, y = rs.NextF() * h, ph = rs.NextF() * MathF.PI * 2f, f = 0.6f + 1.6f * rs.NextF();
                // 縁に近いほど出やすい
                float edge = MathF.Min(MathF.Min(x, w - x) / w, MathF.Min(y, h - y) / h);
                if (edge > 0.22f) continue;
                float tw = MathF.Pow(MathF.Max(0f, MathF.Sin(t * f + ph)), 6f) * g;
                if (tw < 0.03f) continue;
                float r = 2f + 4f * tw;
                brush.Color = Gfx.C4(white, 0.9f * tw);
                dc.DrawLine(new Vector2(x - r * 2f, y), new Vector2(x + r * 2f, y), brush, 1f);
                dc.DrawLine(new Vector2(x, y - r * 2f), new Vector2(x, y + r * 2f), brush, 1f);
                brush.Color = Gfx.C4(IceColor, 0.35f * tw);
                dc.FillEllipse(new Ellipse(new Vector2(x, y), r, r), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
