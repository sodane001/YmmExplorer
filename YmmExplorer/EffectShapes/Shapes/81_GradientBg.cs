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
//  グラデーション背景（動く）
//   色のかたまりがゆっくり漂うメッシュ風の背景。色相をゆっくり回すこともできる
// =====================================================================
public sealed class GradientBgPlugin : IShapePlugin
{
    public string Name => "演出：グラデーション背景（動く）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GradientBgParameter(sharedData);
}

public sealed class GradientBgParameter : EffectShapeParameterBase
{
    public GradientBgParameter() : this(null) { }
    public GradientBgParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "ベースの色（始点）")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0xFF, 0x6A, 0x88);

    [Display(GroupName = "色", Name = "ベースの色（終点）")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0x6A, 0x8C, 0xFF);

    [Display(GroupName = "色", Name = "漂う色1")]
    [ColorPicker]
    public WpfColor Color3 { get => c3; set => Set(ref c3, value); }
    WpfColor c3 = WpfColor.FromRgb(0xFF, 0xB3, 0x6A);

    [Display(GroupName = "色", Name = "漂う色2")]
    [ColorPicker]
    public WpfColor Color4 { get => c4; set => Set(ref c4, value); }
    WpfColor c4 = WpfColor.FromRgb(0x8A, 0x5C, 0xFF);

    [Display(GroupName = "色", Name = "色相をゆっくり回す")]
    [ToggleSlider]
    public bool HueCycle { get => hueCycle; set => Set(ref hueCycle, value); }
    bool hueCycle = false;

    [Display(GroupName = "色", Name = "色相の回る速さ")]
    [AnimationSlider("F0", "°/s", -90, 90)]
    public Animation HueSpeed { get; } = new Animation(12, -3600, 3600);

    [Display(GroupName = "動き", Name = "ベースのグラデーションの向き")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(60, -3600, 3600);

    [Display(GroupName = "動き", Name = "漂う速さ")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "動き", Name = "かたまりの大きさ")]
    [AnimationSlider("F0", "px", 200, 2000)]
    public Animation BlobSize { get; } = new Animation(900, 20, 10000);

    [Display(GroupName = "動き", Name = "かたまりの濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation BlobStrength { get; } = new Animation(80, 0, 100);

    [Display(GroupName = "質感", Name = "粒状のざらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Grain { get; } = new Animation(15, 0, 100);

    [Display(GroupName = "質感", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [HueSpeed, Angle, Speed, BlobSize, BlobStrength, Grain, Opacity];

    // 色相をずらす（彩度・明度は保つ）
    static WpfColor Shift(WpfColor col, float deg)
    {
        if (MathF.Abs(deg) < 0.01f) return col;
        float r = col.R / 255f, g = col.G / 255f, b = col.B / 255f;
        float max = MathF.Max(r, MathF.Max(g, b)), min = MathF.Min(r, MathF.Min(g, b));
        float s = max <= 0f ? 0f : (max - min) / max;
        return Gfx.Hsv(Gfx.Hue(col) + deg, s, max);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float ts = c.Time * c.F(Speed) / 100f;
        float op = c.F(Opacity) / 100f, bs = c.F(BlobStrength) / 100f, size = c.F(BlobSize);
        float hue = HueCycle ? c.F(HueSpeed) * c.Time : 0f;
        var k1 = Shift(Color1, hue);
        var k2 = Shift(Color2, hue);
        var k3 = Shift(Color3, hue);
        var k4 = Shift(Color4, hue);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- ベースのグラデーション ----
        float ang = Gfx.Rad(c.F(Angle));
        var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
        float half = (MathF.Abs(dir.X) * w + MathF.Abs(dir.Y) * h) / 2f;
        var mid = new Vector2(w / 2f, h / 2f);
        using (var g = Gfx.Linear(dc, mid - dir * half, mid + dir * half, (0f, Gfx.C4(k1, op)), (1f, Gfx.C4(k2, op))))
            Gfx.Rect(dc, g, 0, 0, w, h);

        // ---- 漂う色のかたまり ----
        var rng = new Random(Seed);
        var cols = new[] { k3, k4, k1, k2, k3, k4 };
        for (int i = 0; i < 6; i++)
        {
            float bx = rng.NextF(), by = rng.NextF();
            float fx = 0.05f + 0.12f * rng.NextF(), fy = 0.05f + 0.12f * rng.NextF();
            float p1 = rng.NextF() * MathF.PI * 2f, p2 = rng.NextF() * MathF.PI * 2f;
            float r = size * (0.7f + 0.6f * rng.NextF());
            var p = new Vector2((bx + 0.35f * MathF.Sin(ts * fx * 2f + p1)) * w, (by + 0.35f * MathF.Sin(ts * fy * 2f + p2)) * h);
            using var gb = Gfx.Radial(dc, p, r,
                (0f, Gfx.C4(cols[i], 0.85f * bs * op)), (0.5f, Gfx.C4(cols[i], 0.35f * bs * op)), (1f, Gfx.C4(cols[i], 0f)));
            dc.FillEllipse(new Ellipse(p, r, r), gb);
        }

        // ---- ざらつき ----
        float gk = c.F(Grain) / 100f;
        int gn = (int)(gk * 2500f * (w * h / (1920f * 1080f)));
        var rg = new Random(Seed + 1);
        for (int i = 0; i < gn; i++)
        {
            brush.Color = Gfx.C4(rg.NextF() < 0.5f ? WpfColor.FromRgb(255, 255, 255) : WpfColor.FromRgb(0, 0, 0), 0.05f + 0.10f * rg.NextF());
            Gfx.Rect(dc, brush, rg.NextF() * w, rg.NextF() * h, 1.5f, 1.5f);
        }
    }
}

public enum GeoPattern
{
    [Display(Name = "ドット")] Dots,
    [Display(Name = "斜めストライプ")] Stripes,
    [Display(Name = "六角形（ハニカム）")] Hex,
    [Display(Name = "三角形のタイル")] Triangles,
    [Display(Name = "市松（チェッカー）")] Checker,
    [Display(Name = "同心円")] Rings,
    [Display(Name = "波線")] WaveLines,
    [Display(Name = "十字")] Plus,
    [Display(Name = "ひし形（アーガイル）")] Diamonds,
}
