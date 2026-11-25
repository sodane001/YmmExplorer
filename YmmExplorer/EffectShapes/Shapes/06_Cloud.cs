using System.ComponentModel.DataAnnotations;
using System.Numerics;
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
//  雲
// =====================================================================
public sealed class CloudPlugin : IShapePlugin
{
    public string Name => "演出：雲";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CloudParameter(sharedData);
}

public sealed class CloudParameter : EffectShapeParameterBase
{
    public CloudParameter() : this(null) { }
    public CloudParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "雲", Name = "明部の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "雲", Name = "影の色", Description = "雨雲にするなら暗いグレーに")]
    [ColorPicker]
    public WpfColor ShadowColor { get => shadowColor; set => Set(ref shadowColor, value); }
    WpfColor shadowColor = WpfColor.FromRgb(0x7C, 0x8C, 0xA3);

    [Display(GroupName = "雲", Name = "もこもこ数")]
    [AnimationSlider("F0", "個", 6, 300)]
    public Animation Count { get; } = new Animation(48, 1, 2000);

    [Display(GroupName = "雲", Name = "サイズ")]
    [AnimationSlider("F0", "px", 30, 600)]
    public Animation Size { get; } = new Animation(200, 5, 3000);

    [Display(GroupName = "雲", Name = "縦位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(30, -50, 150);

    [Display(GroupName = "雲", Name = "帯の厚み")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Spread { get; } = new Animation(25, 0, 200);

    [Display(GroupName = "雲", Name = "流れる速度")]
    [AnimationSlider("F0", "px/s", -200, 200)]
    public Animation Speed { get; } = new Animation(15, -5000, 5000);

    [Display(GroupName = "雲", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(85, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Count, Size, PosY, Spread, Speed, Opacity];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        int n = (int)c.F(Count);
        float size = c.F(Size), cy = c.F(PosY) / 100f * h, band = c.F(Spread) / 100f * h;
        float spd = c.F(Speed), op = c.F(Opacity) / 100f;

        float margin = size * 3f;
        float span = w + margin * 2f;
        int clusters = Math.Max(1, n / 6);

        var rng = new Random(Seed);
        var cc = new (float x, float y, float s, float v)[clusters];
        for (int k = 0; k < clusters; k++)
            cc[k] = (rng.NextF() * span, cy + (rng.NextF() - 0.5f) * band, 0.6f + 0.8f * rng.NextF(), 0.5f + rng.NextF());

        var puffs = new List<(float x, float y, float r)>(n);
        for (int i = 0; i < n; i++)
        {
            var cl = cc[i % clusters];
            float ox = (rng.NextF() - 0.5f) * size * 2.2f * cl.s;
            float oy = (rng.NextF() - 0.5f) * size * 0.6f * cl.s;
            float r = size * cl.s * (0.5f + 0.5f * rng.NextF());
            float x = Gfx.Mod(cl.x + c.Time * spd * cl.v, span) - margin + ox;
            puffs.Add((x, cl.y + oy, r));
        }

        // 1パス目: 影（少し下にずらす）
        foreach (var p in puffs)
        {
            var ctr = new Vector2(p.x, p.y + p.r * 0.25f);
            using var b = Gfx.Radial(dc, ctr, p.r,
                (0f, Gfx.C4(ShadowColor, op)),
                (0.55f, Gfx.C4(ShadowColor, op * 0.6f)),
                (1f, Gfx.C4(ShadowColor, 0f)));
            dc.FillEllipse(new Ellipse(ctr, p.r, p.r), b);
        }
        // 2パス目: 明部（少し上にずらす）
        foreach (var p in puffs)
        {
            var ctr = new Vector2(p.x, p.y - p.r * 0.12f);
            float r = p.r * 0.9f;
            using var b = Gfx.Radial(dc, ctr, r,
                (0f, Gfx.C4(Color, op)),
                (0.55f, Gfx.C4(Color, op * 0.6f)),
                (1f, Gfx.C4(Color, 0f)));
            dc.FillEllipse(new Ellipse(ctr, r, r), b);
        }
    }
}
