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
//  シャボン玉
//   虹色の薄い膜・ハイライト・揺れ。途中で割れる演出も選べる
// =====================================================================
public sealed class BubblePlugin : IShapePlugin
{
    public string Name => "演出：シャボン玉";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BubbleParameter(sharedData);
}

public sealed class BubbleParameter : EffectShapeParameterBase
{
    public BubbleParameter() : this(null) { }
    public BubbleParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "シャボン玉", Name = "数")]
    [AnimationSlider("F0", "個", 0, 100)]
    public Animation Count { get; } = new Animation(25, 0, 1000);

    [Display(GroupName = "シャボン玉", Name = "最大の大きさ（半径）")]
    [AnimationSlider("F0", "px", 10, 150)]
    public Animation Size { get; } = new Animation(44, 3, 1000);

    [Display(GroupName = "シャボン玉", Name = "虹色の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Iridescence { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "シャボン玉", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "動き", Name = "上昇する速さ")]
    [AnimationSlider("F0", "px/s", -200, 200)]
    public Animation Rise { get; } = new Animation(45, -5000, 5000);

    [Display(GroupName = "動き", Name = "横に漂う幅")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Drift { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "動き", Name = "形のゆがみ")]
    [AnimationSlider("F0", "%", 0, 30)]
    public Animation Wobble { get; } = new Animation(7, 0, 100);

    [Display(GroupName = "動き", Name = "途中で割れる")]
    [ToggleSlider]
    public bool Pop { get => pop; set => Set(ref pop, value); }
    bool pop = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, Size, Iridescence, Opacity, Rise, Drift, Wobble];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = (int)c.F(Count);
        float maxR = c.F(Size), iri = c.F(Iridescence) / 100f, op = c.F(Opacity) / 100f;
        float rise = c.F(Rise), drift = c.F(Drift), wob = c.F(Wobble) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float x0 = rng.NextF() * w;
            float rr = rng.NextF();
            float r = maxR * (0.35f + 0.65f * rr);
            float ph = rng.NextF();
            float sp = (0.7f + 0.6f * rng.NextF()) * rise;
            float wph = rng.NextF() * MathF.PI * 2f, wf = 0.6f + 0.8f * rng.NextF();
            float hue0 = rng.NextF() * 360f;
            float popAt = 0.35f + 0.6f * rng.NextF();

            float span = h + 2f * r;
            float prog = Gfx.Mod(t * MathF.Abs(sp) / span + ph, 1f);        // 0=出発 → 1=到達
            float y = sp >= 0f ? h + r - prog * span : -r + prog * span;
            float x = x0 + MathF.Sin(t * 0.7f * wf + wph) * drift;

            // 割れる演出
            float alpha = 1f, grow = 1f;
            bool popping = false;
            float pu = 0f;
            if (Pop && prog > popAt)
            {
                pu = (prog - popAt) * span / MathF.Max(1f, MathF.Abs(sp)) / 0.25f;     // 0.25秒で割れ切る
                if (pu >= 1f) continue;
                popping = true;
                grow = 1f + 0.25f * pu;
                alpha = 1f - pu;
            }
            alpha *= op;

            var ctr = new Vector2(x, y);
            float rx = r * grow * (1f + wob * MathF.Sin(t * 3f * wf + wph));
            float ry = r * grow * (1f - wob * MathF.Sin(t * 3f * wf + wph));

            // 内側のうっすらした膜
            brush.Color = Gfx.C4(white, 0.05f * alpha);
            dc.FillEllipse(new Ellipse(ctr, rx, ry), brush);

            // 縁の虹色（角度ごとに色相がずれ、ゆっくり回る）
            const int N = 40;
            Vector2 prev = default;
            for (int k = 0; k <= N; k++)
            {
                float a = k / (float)N * MathF.PI * 2f;
                var p = ctr + new Vector2(MathF.Cos(a) * rx, MathF.Sin(a) * ry);
                if (k > 0)
                {
                    var col = Gfx.Hsv(hue0 + a * 57.3f * 0.8f + t * 25f, 0.55f, 1f);
                    float rim = 0.5f + 0.5f * MathF.Cos(a - 2.3f);               // 光源側（左上）ほど強い
                    brush.Color = Gfx.C4(col, (0.25f + 0.35f * rim) * iri * alpha);
                    dc.DrawLine(prev, p, brush, MathF.Max(1.2f, r * 0.12f));
                    brush.Color = Gfx.C4(white, 0.25f * rim * alpha);
                    dc.DrawLine(prev, p, brush, 1f);
                }
                prev = p;
            }

            // ハイライト
            brush.Color = Gfx.C4(white, 0.85f * alpha);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(-rx * 0.42f, -ry * 0.45f), rx * 0.18f, ry * 0.10f), brush);
            brush.Color = Gfx.C4(white, 0.35f * alpha);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(rx * 0.40f, ry * 0.45f), rx * 0.09f, ry * 0.05f), brush);

            // 割れたときの飛沫
            if (popping)
            {
                var rp = new Random(Seed * 31 + i);
                for (int k = 0; k < 10; k++)
                {
                    float a = rp.NextF() * MathF.PI * 2f;
                    float dist = r * (1.1f + 1.4f * pu) * (0.7f + 0.5f * rp.NextF());
                    brush.Color = Gfx.C4(white, 0.7f * (1f - pu) * op);
                    dc.FillEllipse(new Ellipse(ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * dist, 1.6f, 1.6f), brush);
                }
            }
        }
    }
}
