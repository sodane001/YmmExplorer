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
//  レンズフレア・光の筋
//   光源から伸びる光芒・横に流れる光・画面を横切るゴースト・ハロー
// =====================================================================
public sealed class LensFlarePlugin : IShapePlugin
{
    public string Name => "演出：レンズフレア・光の筋";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LensFlareParameter(sharedData);
}

public sealed class LensFlareParameter : EffectShapeParameterBase
{
    public LensFlareParameter() : this(null) { }
    public LensFlareParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "光源", Name = "光の色")]
    [ColorPicker]
    public WpfColor LightColor { get => light; set => Set(ref light, value); }
    WpfColor light = WpfColor.FromRgb(0xFF, 0xE0, 0xA8);

    [Display(GroupName = "光源", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SourceX { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "光源", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SourceY { get; } = new Animation(22, -100, 200);

    [Display(GroupName = "光源", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Intensity { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "光源", Name = "光源の大きさ")]
    [AnimationSlider("F0", "px", 20, 500)]
    public Animation CoreSize { get; } = new Animation(110, 5, 5000);

    [Display(GroupName = "光源", Name = "ゆらゆら動く")]
    [ToggleSlider]
    public bool Sway { get => sway; set => Set(ref sway, value); }
    bool sway = true;

    [Display(GroupName = "光芒", Name = "光の筋の数")]
    [AnimationSlider("F0", "本", 0, 48)]
    public Animation RayCount { get; } = new Animation(18, 0, 200);

    [Display(GroupName = "光芒", Name = "光の筋の長さ")]
    [AnimationSlider("F0", "px", 100, 1500)]
    public Animation RayLength { get; } = new Animation(520, 10, 8000);

    [Display(GroupName = "光芒", Name = "横に伸びる光の長さ")]
    [AnimationSlider("F0", "px", 0, 2000)]
    public Animation StreakLength { get; } = new Animation(900, 0, 20000);

    [Display(GroupName = "ゴースト", Name = "ゴーストの数")]
    [AnimationSlider("F0", "個", 0, 12)]
    public Animation Ghosts { get; } = new Animation(6, 0, 40);

    [Display(GroupName = "ゴースト", Name = "六角形にする")]
    [ToggleSlider]
    public bool Hexagon { get => hex; set => Set(ref hex, value); }
    bool hex = false;

    [Display(GroupName = "ゴースト", Name = "ハロー（輪）")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Halo { get; } = new Animation(80, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [SourceX, SourceY, Intensity, CoreSize, RayCount, RayLength, StreakLength, Ghosts, Halo];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var src = new Vector2(c.F(SourceX) / 100f * w, c.F(SourceY) / 100f * h);
        if (Sway) src += new Vector2(MathF.Sin(t * 0.5f) * w * 0.02f, MathF.Cos(t * 0.37f) * h * 0.02f);
        float I = c.F(Intensity) / 100f, core = c.F(CoreSize);
        int rays = (int)c.F(RayCount), ghosts = (int)c.F(Ghosts);
        float rayLen = c.F(RayLength), streak = c.F(StreakLength), halo = c.F(Halo) / 100f;
        var center = new Vector2(w / 2f, h / 2f);
        var white = WpfColor.FromRgb(255, 255, 255);
        float flick = 0.94f + 0.06f * MathF.Sin(t * 7f) * MathF.Sin(t * 2.3f + 1f);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            // ---- 光源のにじみと芯 ----
            float gr = core * 3.2f;
            using (var gb = Gfx.Radial(dc, src, gr,
                (0f, Gfx.C4(white, 0.9f * I * flick)), (0.15f, Gfx.C4(LightColor, 0.55f * I * flick)),
                (0.5f, Gfx.C4(LightColor, 0.16f * I)), (1f, Gfx.C4(LightColor, 0f))))
                dc.FillEllipse(new Ellipse(src, gr, gr), gb);

            // ---- 放射状の光の筋（長さと強さがゆっくり変わる）----
            var rr = new Random(Seed);
            for (int i = 0; i < rays; i++)
            {
                float a = rr.NextF() * MathF.PI * 2f;
                float len = rayLen * (0.35f + 0.65f * rr.NextF()) * (0.85f + 0.15f * MathF.Sin(t * (0.8f + rr.NextF()) + i));
                float wd = 1.5f + 5f * rr.NextF();
                var dir = new Vector2(MathF.Cos(a), MathF.Sin(a));
                Gfx.FadeLine(dc, brush, src + dir * core * 0.3f, src + dir * len, wd, 0.5f, LightColor, 0.35f * I * flick, 12, 1.4f);
            }

            // ---- 横に伸びる光（アナモルフィック）----
            if (streak > 0f)
            {
                Gfx.FadeLine(dc, brush, src, src + new Vector2(streak, 0), core * 0.10f, 1.5f, LightColor, 0.55f * I * flick, 18, 1.2f);
                Gfx.FadeLine(dc, brush, src, src - new Vector2(streak, 0), core * 0.10f, 1.5f, LightColor, 0.55f * I * flick, 18, 1.2f);
                Gfx.FadeLine(dc, brush, src, src + new Vector2(streak, 0), core * 0.03f, 1f, white, 0.8f * I * flick, 18, 1.0f);
                Gfx.FadeLine(dc, brush, src, src - new Vector2(streak, 0), core * 0.03f, 1f, white, 0.8f * I * flick, 18, 1.0f);
            }

            // ---- ハロー（虹色の輪）----
            if (halo > 0f)
            {
                float hr = core * 2.4f;
                Vector2 prev = default;
                const int N = 60;
                for (int k = 0; k <= N; k++)
                {
                    float a = k / (float)N * MathF.PI * 2f;
                    var p = src + new Vector2(MathF.Cos(a), MathF.Sin(a)) * hr;
                    if (k > 0)
                    {
                        brush.Color = Gfx.C4(Gfx.Hsv(a * 57.3f + t * 10f, 0.55f, 1f), 0.20f * halo * I);
                        dc.DrawLine(prev, p, brush, core * 0.12f);
                    }
                    prev = p;
                }
            }

            // ---- ゴースト（光源→画面の中心→反対側へ並ぶ）----
            var rg = new Random(Seed + 7);
            for (int i = 0; i < ghosts; i++)
            {
                float m = -0.35f + 1.9f * (i + 0.5f) / MathF.Max(1, ghosts) + (rg.NextF() - 0.5f) * 0.1f;
                var gp = center + (center - src) * (m - 0.5f) * 1.6f;
                float r = core * (0.25f + 0.9f * rg.NextF());
                var col = Gfx.Hsv(rg.NextF() * 360f, 0.5f, 1f);
                float a = (0.10f + 0.16f * rg.NextF()) * I;
                if (Hexagon)
                {
                    var hp = ShapeGeom.Closed(ShapeGeom.StarPoly(gp, r, 6, 1f, rg.NextF() * 1.0f));
                    brush.Color = Gfx.C4(col, a * 0.5f);
                    Gfx.FillPoly(dc, brush, hp);
                    brush.Color = Gfx.C4(col, a * 1.6f);
                    Gfx.Polyline(dc, brush, hp, 2f);
                }
                else
                {
                    using var gb = Gfx.Radial(dc, gp, r,
                        (0f, Gfx.C4(col, a * 0.15f)), (0.75f, Gfx.C4(col, a * 0.5f)), (0.92f, Gfx.C4(col, a * 1.8f)), (1f, Gfx.C4(col, 0f)));
                    dc.FillEllipse(new Ellipse(gp, r, r), gb);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}

public enum CountMode
{
    [Display(Name = "カウントダウン（数字）")] Countdown,
    [Display(Name = "カウントダウン（円形リング）")] Ring,
    [Display(Name = "タイマー（分：秒）")] Timer,
    [Display(Name = "ストップウォッチ（秒.コンマ）")] Stopwatch,
}
