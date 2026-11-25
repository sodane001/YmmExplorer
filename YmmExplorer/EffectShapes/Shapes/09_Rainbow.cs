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
//  虹
//   七色のアーチ。二重の虹・霧・左から右へ架かる登場アニメーション
// =====================================================================
public sealed class RainbowPlugin : IShapePlugin
{
    public string Name => "演出：虹";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RainbowParameter(sharedData);
}

public sealed class RainbowParameter : EffectShapeParameterBase
{
    public RainbowParameter() : this(null) { }
    public RainbowParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "位置", Name = "アーチの中心X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "アーチの足元の高さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(85, -100, 200);

    [Display(GroupName = "虹", Name = "半径（外側）")]
    [AnimationSlider("F0", "px", 200, 1500)]
    public Animation Radius { get; } = new Animation(720, 20, 10000);

    [Display(GroupName = "虹", Name = "帯の太さ（全体）")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation Band { get; } = new Animation(170, 7, 3000);

    [Display(GroupName = "虹", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Strength { get; } = new Animation(70, 0, 500);

    [Display(GroupName = "虹", Name = "二重の虹")]
    [ToggleSlider]
    public bool DoubleRainbow { get => dbl; set => Set(ref dbl, value); }
    bool dbl = true;

    [Display(GroupName = "虹", Name = "足元の霧")]
    [ToggleSlider]
    public bool Mist { get => mist; set => Set(ref mist, value); }
    bool mist = true;

    [Display(GroupName = "動き", Name = "架かる登場アニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "架かるまでの時間")]
    [AnimationSlider("F1", "秒", 0.5, 10)]
    public Animation IntroTime { get; } = new Animation(3, 0.1, 60);

    [Display(GroupName = "動き", Name = "出現度（手動）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Appear { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Band, Strength, IntroTime, Appear];

    static readonly WpfColor[] Bands =
    {
        WpfColor.FromRgb(255, 40, 40), WpfColor.FromRgb(255, 140, 20), WpfColor.FromRgb(255, 230, 40),
        WpfColor.FromRgb(60, 210, 70), WpfColor.FromRgb(40, 150, 255), WpfColor.FromRgb(90, 50, 220), WpfColor.FromRgb(160, 60, 230),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float prog = c.F(Appear) / 100f;
        if (Intro) prog *= Gfx.Smooth(0f, MathF.Max(0.1f, c.F(IntroTime)), t);
        if (prog <= 0.002f) return;

        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), band = c.F(Band), k = c.F(Strength) / 100f;
        float bw = band / 7f;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            void Arch(float outer, float bwid, float a, bool reverse)
            {
                for (int i = 0; i < 7; i++)
                {
                    var col = Bands[reverse ? 6 - i : i];
                    float r = outer - (i + 0.5f) * bwid;
                    // 左（π）から右へ向かって、進み具合ぶんだけ描く
                    var pts = Gfx.ArcPts(ctr, r, MathF.PI, MathF.PI + MathF.PI * prog, 140);
                    brush.Color = Gfx.C4(col, 0.10f * a * k);
                    FormKit.Stroke(dc, brush, pts, bwid * 2.2f, false);
                    brush.Color = Gfx.C4(col, 0.45f * a * k);
                    FormKit.Stroke(dc, brush, pts, bwid * 1.08f, false);
                }
            }
            Arch(R, bw, 1f, false);
            if (DoubleRainbow) Arch(R * 1.28f, bw * 0.85f, 0.45f, true);

            // 足元の霧
            if (Mist)
            {
                float mr = band * 1.6f;
                float mrad = R - band / 2f;
                foreach (float sgn in new[] { -1f, 1f })
                {
                    var p = ctr + new Vector2(sgn * mrad, 0f);
                    float visible = sgn < 0 ? Gfx.Smooth(0f, 0.15f, prog) : Gfx.Smooth(0.85f, 1f, prog);
                    using var mb = Gfx.Radial(dc, p, mr, (0f, Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.30f * k * visible)), (1f, Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0f)));
                    dc.FillEllipse(new Ellipse(p, mr, mr * 0.7f), mb);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
