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
//  フィルム風
//   色味・ビネット・粒子ノイズ・傷・ほこり・ちらつき・光漏れ・黒帯
// =====================================================================
public sealed class FilmLookPlugin : IShapePlugin
{
    public string Name => "演出：フィルム風";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FilmLookParameter(sharedData);
}

public sealed class FilmLookParameter : EffectShapeParameterBase
{
    public FilmLookParameter() : this(null) { }
    public FilmLookParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色味", Name = "色味の色")]
    [ColorPicker]
    public WpfColor TintColor { get => tint; set => Set(ref tint, value); }
    WpfColor tint = WpfColor.FromRgb(0xC8, 0xA0, 0x60);

    [Display(GroupName = "色味", Name = "色味の強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation TintAmount { get; } = new Animation(12, 0, 100);

    [Display(GroupName = "色味", Name = "ビネット（四隅の暗がり）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Vignette { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "質感", Name = "粒子ノイズ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Grain { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "質感", Name = "縦傷")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Scratches { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "質感", Name = "ほこり・ゴミ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Dust { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "質感", Name = "明るさのちらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(25, 0, 100);

    [Display(GroupName = "光漏れ", Name = "色")]
    [ColorPicker]
    public WpfColor LeakColor { get => leak; set => Set(ref leak, value); }
    WpfColor leak = WpfColor.FromRgb(0xFF, 0x7A, 0x2A);

    [Display(GroupName = "光漏れ", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LeakAmount { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "黒帯", Name = "上下の黒帯を表示する")]
    [ToggleSlider]
    public bool Letterbox { get => bars; set => Set(ref bars, value); }
    bool bars = false;

    [Display(GroupName = "黒帯", Name = "黒帯の高さ")]
    [AnimationSlider("F0", "%", 0, 30)]
    public Animation BarHeight { get; } = new Animation(12, 0, 50);

    [Display(GroupName = "動き", Name = "コマ数")]
    [AnimationSlider("F0", "コマ/秒", 4, 30)]
    public Animation FrameRate { get; } = new Animation(24, 1, 120);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [TintAmount, Vignette, Grain, Scratches, Dust, Flicker, LeakAmount, BarHeight, FrameRate];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float fps = MathF.Max(1f, c.F(FrameRate));
        int tick = (int)MathF.Floor(t * fps);
        var rng = new Random(Seed * 7919 + tick * 131);
        float areaK = w * h / (1920f * 1080f);
        var white = WpfColor.FromRgb(255, 255, 255);
        var black = WpfColor.FromRgb(0, 0, 0);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 色味 ----
        float ta = c.F(TintAmount) / 100f;
        if (ta > 0f)
        {
            brush.Color = Gfx.C4(TintColor, ta * 0.6f);
            Gfx.Rect(dc, brush, 0, 0, w, h);
        }

        // ---- 明るさのちらつき ----
        float fk = c.F(Flicker) / 100f;
        if (fk > 0f)
        {
            float f = (rng.NextF() * 2f - 1f) * fk;
            brush.Color = Gfx.C4(f > 0f ? white : black, MathF.Abs(f) * 0.14f);
            Gfx.Rect(dc, brush, 0, 0, w, h);
        }

        // ---- 光漏れ（ゆっくり動く暖色の光）----
        float lk = c.F(LeakAmount) / 100f;
        if (lk > 0f)
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    float side = (i % 2 == 0) ? 0f : w;
                    var p = new Vector2(side + MathF.Sin(t * 0.3f + i * 2f) * w * 0.05f, h * (0.25f + 0.25f * i) + MathF.Sin(t * 0.2f + i) * h * 0.1f);
                    float r = MathF.Max(w, h) * (0.35f + 0.1f * MathF.Sin(t * 0.5f + i));
                    var col = Gfx.Mix(LeakColor, white, i == 2 ? 0.35f : 0f);
                    using var lb = Gfx.Radial(dc, p, r, (0f, Gfx.C4(col, 0.30f * lk * (0.7f + 0.3f * MathF.Sin(t * 1.3f + i)))), (1f, Gfx.C4(col, 0f)));
                    dc.FillEllipse(new Ellipse(p, r, r), lb);
                }
            }
            finally
            {
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }

        // ---- ビネット ----
        float vk = c.F(Vignette) / 100f;
        if (vk > 0f)
        {
            float vr = MathF.Sqrt(w * w + h * h) / 2f;
            using var vb = Gfx.Radial(dc, new Vector2(w / 2f, h / 2f), vr,
                (0f, Gfx.C4(black, 0f)), (0.55f, Gfx.C4(black, 0f)), (1f, Gfx.C4(black, 0.85f * vk)));
            Gfx.Rect(dc, vb, 0, 0, w, h);
        }

        // ---- 粒子ノイズ ----
        float gk = c.F(Grain) / 100f;
        int gn = (int)(gk * 2200f * areaK);
        for (int i = 0; i < gn; i++)
        {
            float x = rng.NextF() * w, y = rng.NextF() * h;
            float s = 1f + rng.NextF() * 1.5f;
            brush.Color = Gfx.C4(rng.NextF() < 0.5f ? white : black, 0.08f + 0.3f * rng.NextF());
            Gfx.Rect(dc, brush, x, y, s, s);
        }

        // ---- 縦傷（数コマだけ現れる）----
        float sk = c.F(Scratches) / 100f;
        if (sk > 0f)
        {
            for (int i = 0; i < 4; i++)
            {
                var rs = new Random(Seed * 31 + (tick / 3) * 977 + i * 53);     // 3コマごとに変わる
                if (rs.NextF() > sk * 0.8f) continue;
                float x = rs.NextF() * w;
                float y0 = rs.NextF() * h * 0.4f, y1 = y0 + h * (0.3f + 0.6f * rs.NextF());
                float wob = (rs.NextF() - 0.5f) * 6f;
                brush.Color = Gfx.C4(white, 0.18f + 0.35f * rs.NextF());
                dc.DrawLine(new Vector2(x, y0), new Vector2(x + wob, y1), brush, 1f + rs.NextF());
            }
        }

        // ---- ほこり・ゴミ・髪の毛 ----
        float dk = c.F(Dust) / 100f;
        if (dk > 0f)
        {
            int dn = (int)(dk * 24f * areaK);
            for (int i = 0; i < dn; i++)
            {
                float x = rng.NextF() * w, y = rng.NextF() * h;
                float r = 0.8f + rng.NextF() * 2.6f;
                brush.Color = Gfx.C4(rng.NextF() < 0.7f ? black : white, 0.3f + 0.4f * rng.NextF());
                dc.FillEllipse(new Ellipse(new Vector2(x, y), r, r * (0.6f + 0.6f * rng.NextF())), brush);
            }
            if (rng.NextF() < dk * 0.5f)
            {
                var p = new Vector2(rng.NextF() * w, rng.NextF() * h);
                var pts = new List<Vector2> { p };
                float a = rng.NextF() * MathF.PI * 2f;
                for (int k = 0; k < 6; k++)
                {
                    a += (rng.NextF() - 0.5f) * 1.2f;
                    p += new Vector2(MathF.Cos(a), MathF.Sin(a)) * (6f + 8f * rng.NextF());
                    pts.Add(p);
                }
                brush.Color = Gfx.C4(black, 0.55f);
                Gfx.Polyline(dc, brush, pts, 1.2f);
            }
        }

        // ---- 上下の黒帯 ----
        if (Letterbox)
        {
            float bh = h * c.F(BarHeight) / 100f;
            brush.Color = Gfx.C4(black, 1f);
            Gfx.Rect(dc, brush, 0, 0, w, bh);
            Gfx.Rect(dc, brush, 0, h - bh, w, bh);
        }
    }
}
