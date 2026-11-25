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
//  マズルフラッシュ・弾痕
//   発砲の閃光、薬莢、壁に残る弾痕
// =====================================================================
public sealed class MuzzleFlashPlugin : IShapePlugin
{
    public string Name => "演出：マズルフラッシュ・弾痕";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MuzzleFlashParameter(sharedData);
}

public sealed class MuzzleFlashParameter : EffectShapeParameterBase
{
    public MuzzleFlashParameter() : this(null) { }
    public MuzzleFlashParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "閃光の色")]
    [ColorPicker]
    public WpfColor FlashColor { get => flashColor; set => Set(ref flashColor, value); }
    WpfColor flashColor = WpfColor.FromRgb(0xFF, 0xC0, 0x50);

    [Display(GroupName = "銃口", Name = "銃口のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(25, -100, 200);

    [Display(GroupName = "銃口", Name = "銃口のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(58, -100, 200);

    [Display(GroupName = "銃口", Name = "撃つ向き（0°=右）")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Direction { get; } = new Animation(-5, -3600, 3600);

    [Display(GroupName = "銃口", Name = "1秒に撃つ数")]
    [AnimationSlider("F1", "発", 0.5, 20)]
    public Animation FireRate { get; } = new Animation(8, 0.1, 60);

    [Display(GroupName = "銃口", Name = "閃光の大きさ")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation FlashSize { get; } = new Animation(120, 1, 3000);

    [Display(GroupName = "銃口", Name = "薬莢を飛ばす")]
    [ToggleSlider]
    public bool Shells { get => shells; set => Set(ref shells, value); }
    bool shells = true;

    [Display(GroupName = "銃口", Name = "硝煙を出す")]
    [ToggleSlider]
    public bool Smoke { get => smoke; set => Set(ref smoke, value); }
    bool smoke = true;

    [Display(GroupName = "弾痕", Name = "弾痕を残す")]
    [ToggleSlider]
    public bool Holes { get => holes; set => Set(ref holes, value); }
    bool holes = true;

    [Display(GroupName = "弾痕", Name = "着弾のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation TargetX { get; } = new Animation(70, -100, 200);

    [Display(GroupName = "弾痕", Name = "着弾のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation TargetY { get; } = new Animation(45, -100, 200);

    [Display(GroupName = "弾痕", Name = "着弾のばらつき")]
    [AnimationSlider("F0", "px", 0, 600)]
    public Animation TargetSpread { get; } = new Animation(220, 0, 5000);

    [Display(GroupName = "弾痕", Name = "残る弾痕の数")]
    [AnimationSlider("F0", "個", 1, 60)]
    public Animation HoleCount { get; } = new Animation(14, 1, 500);

    [Display(GroupName = "弾痕", Name = "弾痕の大きさ")]
    [AnimationSlider("F0", "px", 4, 60)]
    public Animation HoleSize { get; } = new Animation(16, 1, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Direction, FireRate, FlashSize, TargetX, TargetY, TargetSpread, HoleCount, HoleSize];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var m = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float dirA = Gfx.Rad(c.F(Direction));
        var d = new Vector2(MathF.Cos(dirA), MathF.Sin(dirA));
        var nrm = new Vector2(-d.Y, d.X);
        float rate = MathF.Max(0.1f, c.F(FireRate)), size = c.F(FlashSize);
        int k = (int)MathF.Floor(t * rate);
        float age = t - k / rate;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // ---- 弾痕 ----
            if (Holes)
            {
                var tgt = new Vector2(c.F(TargetX) / 100f * w, c.F(TargetY) / 100f * h);
                float spread = c.F(TargetSpread), hs = c.F(HoleSize);
                int keep = Math.Max(1, (int)c.F(HoleCount));
                for (int i = Math.Max(0, k - keep + 1); i <= k; i++)
                {
                    float hitAge = t - i / rate - 0.03f;
                    if (hitAge < 0f) continue;
                    float fade = i == k - keep + 1 ? 1f - DrawKit.Frac(t * rate) : 1f;
                    var p = tgt + new Vector2((DrawKit.H(Seed, i, 1) - 0.5f) * 2f * spread, (DrawKit.H(Seed, i, 2) - 0.5f) * 1.4f * spread);
                    float s = hs * (0.75f + 0.5f * DrawKit.H(Seed, i, 3));
                    // ひび
                    brush.Color = Gfx.C4(0.08f, 0.07f, 0.06f, 0.75f * fade);
                    int cracks = 5 + (int)(DrawKit.H(Seed, i, 4) * 4f);
                    for (int j = 0; j < cracks; j++)
                    {
                        float a = DrawKit.H(Seed, i * 11 + j, 5) * MathF.PI * 2f;
                        float len = s * (1.2f + 2.2f * DrawKit.H(Seed, i * 11 + j, 6));
                        var cd = new Vector2(MathF.Cos(a), MathF.Sin(a));
                        var mid = p + cd * len * 0.55f + new Vector2(-cd.Y, cd.X) * len * 0.12f * (DrawKit.H(Seed, j, 7) - 0.5f);
                        Gfx.Polyline(dc, brush, new List<Vector2> { p + cd * s * 0.5f, mid, p + cd * len }, MathF.Max(1f, s * 0.09f));
                    }
                    // 周りの明るい欠け・黒い穴
                    brush.Color = Gfx.C4(0.75f, 0.72f, 0.68f, 0.35f * fade);
                    dc.FillEllipse(new Ellipse(p, s * 0.95f, s * 0.95f), brush);
                    brush.Color = Gfx.C4(0.05f, 0.04f, 0.04f, 0.92f * fade);
                    dc.FillEllipse(new Ellipse(p, s * 0.55f, s * 0.5f), brush);
                    brush.Color = Gfx.C4(0f, 0f, 0f, 0.95f * fade);
                    dc.FillEllipse(new Ellipse(p + new Vector2(s * 0.05f, s * 0.05f), s * 0.32f, s * 0.3f), brush);
                    // 当たった瞬間の火花
                    if (hitAge < 0.09f)
                    {
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        float u = hitAge / 0.09f;
                        DrawKit.Glow(dc, p, s * 3.5f, Gfx.C4(FlashColor, 0.8f * (1f - u)));
                        for (int j = 0; j < 6; j++)
                        {
                            float a = DrawKit.H(Seed, i * 7 + j, 8) * MathF.PI * 2f;
                            var cd = new Vector2(MathF.Cos(a), MathF.Sin(a));
                            Gfx.FadeLine(dc, brush, p + cd * s * 0.5f, p + cd * s * (1.5f + 3f * u), 2.5f, 0.5f, FlashColor, 1f - u, 5, 1f);
                        }
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                    }
                }
            }

            // ---- 硝煙 ----
            if (Smoke)
            {
                int back = (int)MathF.Ceiling(1.6f * rate);
                for (int i = Math.Max(0, k - back); i <= k; i++)
                {
                    float a = t - i / rate;
                    if (a < 0f || a > 1.6f) continue;
                    float u = a / 1.6f;
                    var p = m + d * (size * 0.3f + a * 60f) + new Vector2(0, -a * 45f) + nrm * (DrawKit.H(Seed, i, 9) - 0.5f) * 30f * u;
                    float r = size * 0.15f + a * 55f;
                    using var g = Gfx.Radial(dc, p, r, (0f, Gfx.C4(0.75f, 0.75f, 0.75f, 0.22f * (1f - u))), (1f, Gfx.C4(0.75f, 0.75f, 0.75f, 0f)));
                    dc.FillEllipse(new Ellipse(p, r, r), g);
                }
            }

            // ---- 薬莢 ----
            if (Shells)
            {
                int back = (int)MathF.Ceiling(1.0f * rate);
                for (int i = Math.Max(0, k - back); i <= k; i++)
                {
                    float a = t - i / rate;
                    if (a < 0f || a > 1.0f) continue;
                    var ej = m - d * size * 0.9f;
                    var v = -d * (60f + 60f * DrawKit.H(Seed, i, 10)) - nrm * (260f + 120f * DrawKit.H(Seed, i, 11)) * MathF.Sign(nrm.Y == 0 ? 1f : nrm.Y);
                    var p = ej + v * a + new Vector2(0, 1500f * a * a * 0.5f);
                    if (p.Y > h + 40f) continue;
                    float rot = a * (14f + 10f * DrawKit.H(Seed, i, 12));
                    var sd = new Vector2(MathF.Cos(rot), MathF.Sin(rot));
                    brush.Color = Gfx.C4(0.78f, 0.58f, 0.22f, 1f);
                    dc.DrawLine(p - sd * 7f, p + sd * 7f, brush, 6f);
                    brush.Color = Gfx.C4(1f, 0.86f, 0.5f, 0.9f);
                    dc.DrawLine(p - sd * 6f + new Vector2(-sd.Y, sd.X) * 1.2f, p + sd * 5f + new Vector2(-sd.Y, sd.X) * 1.2f, brush, 1.6f);
                }
            }

            // ---- 閃光 ----
            if (age < 0.055f && size > 1f)
            {
                float u = age / 0.055f;
                float inten = 1f - u;
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, m + d * size * 0.3f, size * 2.2f, Gfx.C4(FlashColor, 0.35f * inten));
                var star = new List<Vector2>();
                float rot0 = DrawKit.H(Seed, k, 13) * 0.6f;
                for (int i = 0; i < 18; i++)
                {
                    float a = i * MathF.PI * 2f / 18f + rot0;
                    float along = MathF.Max(0f, MathF.Cos(a));
                    float r = size * (i % 2 == 0 ? 0.3f + 0.35f * DrawKit.H(Seed, k * 19 + i, 14) : 0.1f) * (1f + 2.2f * MathF.Pow(along, 6f));
                    var ld = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    star.Add(m + d * (ld.X * r) + nrm * (ld.Y * r));
                }
                brush.Color = Gfx.C4(FlashColor, 0.85f * inten);
                Gfx.FillPoly(dc, brush, star);
                // 中心の白い芯
                var core = new List<Vector2>();
                foreach (var p in star) core.Add(Vector2.Lerp(m, p, 0.45f));
                brush.Color = Gfx.C4(Gfx.Mix(FlashColor, white, 0.8f), inten);
                Gfx.FillPoly(dc, brush, core);
                DrawKit.Glow(dc, m, size * 0.5f, Gfx.C4(white, inten));
                // 弾道の筋
                Gfx.FadeLine(dc, brush, m + d * size * 0.6f, m + d * (size * 0.6f + w * 0.6f), 3f, 0.5f, FlashColor, 0.5f * inten, 10, 1.5f);
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
