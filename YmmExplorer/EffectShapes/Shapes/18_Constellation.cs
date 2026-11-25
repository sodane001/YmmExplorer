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
//  星座
//   星を線でつないで描き上げる。星はまたたき、まわりに薄い輪が回る
// =====================================================================
public sealed class ConstellationPlugin : IShapePlugin
{
    public string Name => "演出：星座";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ConstellationParameter(sharedData);
}

public sealed class ConstellationParameter : EffectShapeParameterBase
{
    public ConstellationParameter() : this(null) { }
    public ConstellationParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => line; set => Set(ref line, value); }
    WpfColor line = WpfColor.FromRgb(0x9F, 0xC8, 0xFF);

    [Display(GroupName = "色", Name = "星の色")]
    [ColorPicker]
    public WpfColor StarColor { get => star; set => Set(ref star, value); }
    WpfColor star = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "星座", Name = "星座の数")]
    [AnimationSlider("F0", "個", 1, 10)]
    public Animation ConstCount { get; } = new Animation(4, 1, 40);

    [Display(GroupName = "星座", Name = "1つの星座の星の数")]
    [AnimationSlider("F0", "個", 3, 14)]
    public Animation StarsPer { get; } = new Animation(7, 2, 40);

    [Display(GroupName = "星座", Name = "広がり")]
    [AnimationSlider("F0", "px", 60, 500)]
    public Animation Spread { get; } = new Animation(220, 10, 3000);

    [Display(GroupName = "星座", Name = "星の大きさ")]
    [AnimationSlider("F1", "px", 1, 12)]
    public Animation StarSize { get; } = new Animation(4.5, 0.5, 100);

    [Display(GroupName = "星座", Name = "まわりの輪を表示する")]
    [ToggleSlider]
    public bool ShowRing { get => showRing; set => Set(ref showRing, value); }
    bool showRing = true;

    [Display(GroupName = "背景の星", Name = "数")]
    [AnimationSlider("F0", "個", 0, 600)]
    public Animation BgStars { get; } = new Animation(160, 0, 5000);

    [Display(GroupName = "動き", Name = "線が描き上がる登場アニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "描き上がる時間")]
    [AnimationSlider("F1", "秒", 0.5, 12)]
    public Animation IntroTime { get; } = new Animation(3.5, 0.1, 60);

    [Display(GroupName = "動き", Name = "出現度（手動）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Appear { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [ConstCount, StarsPer, Spread, StarSize, BgStars, IntroTime, Appear];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float prog = c.F(Appear) / 100f;
        if (Intro) prog *= Gfx.Smooth(0f, MathF.Max(0.1f, c.F(IntroTime)), t);
        int nc = (int)c.F(ConstCount), per = (int)c.F(StarsPer);
        float spread = c.F(Spread), ss = c.F(StarSize);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 背景の細かい星 ----
            int bn = (int)c.F(BgStars);
            var rb = new Random(Seed + 99);
            for (int i = 0; i < bn; i++)
            {
                float x = rb.NextF() * w, y = rb.NextF() * h, m = rb.NextF(), ph = rb.NextF() * MathF.PI * 2f, f = 0.5f + rb.NextF() * 2f;
                float a = (0.15f + 0.5f * m * m) * (0.6f + 0.4f * MathF.Sin(t * 2f * f + ph));
                brush.Color = Gfx.C4(StarColor, a);
                dc.FillEllipse(new Ellipse(new Vector2(x, y), 0.6f + 1.1f * m, 0.6f + 1.1f * m), brush);
            }

            if (prog <= 0.002f) return;

            for (int ci = 0; ci < nc; ci++)
            {
                var rng = new Random(Seed * 977 + ci * 131);
                var center = new Vector2(w * ((ci + 0.5f) / nc) + (rng.NextF() - 0.5f) * w * 0.08f,
                                         h * (0.28f + 0.44f * rng.NextF()));
                float sc = spread * (0.7f + 0.5f * rng.NextF());

                // 星を置き、最も近い既存の星とつなぐ（1本の木）＋ 少しだけ輪になる線
                var stars = new List<Vector2>();
                var edges = new List<(int a, int b)>();
                for (int i = 0; i < per; i++)
                {
                    float a = rng.NextF() * MathF.PI * 2f, r = sc * MathF.Sqrt(rng.NextF());
                    var p = center + new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r * 0.75f);
                    if (i > 0)
                    {
                        int best = 0; float bd = float.MaxValue;
                        for (int k = 0; k < stars.Count; k++)
                        {
                            float d = Vector2.DistanceSquared(p, stars[k]);
                            if (d < bd) { bd = d; best = k; }
                        }
                        edges.Add((best, i));
                        if (i > 2 && rng.NextF() < 0.25f) edges.Add((i, rng.Next(0, i - 1)));
                    }
                    stars.Add(p);
                }

                // まわりの輪
                if (ShowRing)
                {
                    float rr = sc * 1.25f;
                    var ring = Gfx.ArcPts(center, rr, t * 0.05f, t * 0.05f + MathF.PI * 2f, 100);
                    brush.Color = Gfx.C4(LineColor, 0.22f * prog);
                    FormKit.Dash(dc, brush, ring, 6f, 10f, t * 6f, 1f);
                }

                // 線（順番に描き上がる）
                int E = Math.Max(1, edges.Count);
                var starAppear = new float[stars.Count];
                for (int e = 0; e < edges.Count; e++)
                {
                    float pe = Math.Clamp(prog * (E + 1) - e, 0f, 1f);
                    var a = stars[edges[e].a];
                    var b = stars[edges[e].b];
                    starAppear[edges[e].b] = MathF.Max(starAppear[edges[e].b], Math.Clamp(pe * 4f, 0f, 1f));
                    starAppear[edges[e].a] = MathF.Max(starAppear[edges[e].a], Math.Clamp(pe * 4f, 0f, 1f));
                    if (pe <= 0f) continue;
                    var dir = Vector2.Normalize(b - a);
                    var s0 = a + dir * (ss * 2.2f);
                    var s1 = Vector2.Lerp(a, b, pe) - dir * (pe >= 1f ? ss * 2.2f : 0f);
                    brush.Color = Gfx.C4(LineColor, 0.18f);
                    dc.DrawLine(s0, s1, brush, 4f);
                    brush.Color = Gfx.C4(LineColor, 0.8f);
                    dc.DrawLine(s0, s1, brush, 1.2f);
                }
                if (edges.Count == 0) starAppear[0] = 1f;
                starAppear[0] = MathF.Max(starAppear[0], Math.Clamp(prog * 6f, 0f, 1f));

                // 星（十字の光と一緒にまたたく）
                for (int i = 0; i < stars.Count; i++)
                {
                    float ap = starAppear[i];
                    if (ap <= 0f) continue;
                    float tw = 0.7f + 0.3f * MathF.Sin(t * 2.2f + i * 1.7f + ci);
                    float r = ss * (0.8f + 0.5f * ((i * 37 + ci * 11) % 5) / 5f) * ap;
                    var p = stars[i];
                    brush.Color = Gfx.C4(StarColor, 0.12f * tw);
                    dc.FillEllipse(new Ellipse(p, r * 3.4f, r * 3.4f), brush);
                    brush.Color = Gfx.C4(StarColor, 0.95f * tw);
                    dc.FillEllipse(new Ellipse(p, r, r), brush);
                    float fl = r * 5f * tw;
                    brush.Color = Gfx.C4(white, 0.55f * tw);
                    dc.DrawLine(new Vector2(p.X - fl, p.Y), new Vector2(p.X + fl, p.Y), brush, 1f);
                    dc.DrawLine(new Vector2(p.X, p.Y - fl), new Vector2(p.X, p.Y + fl), brush, 1f);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
