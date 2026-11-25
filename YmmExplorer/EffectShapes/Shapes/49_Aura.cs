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
//  オーラ・光の柱
//   キャラクターの周りに立ちのぼる気、または天から差す光の柱
// =====================================================================
public sealed class AuraPlugin : IShapePlugin
{
    public string Name => "演出：オーラ・光の柱";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AuraParameter(sharedData);
}

public sealed class AuraParameter : EffectShapeParameterBase
{
    public AuraParameter() : this(null) { }
    public AuraParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public AuraKind Kind { get => kind; set => Set(ref kind, value); }
    AuraKind kind = AuraKind.Aura;

    [Display(GroupName = "色", Name = "外側の色")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0xFF, 0xC8, 0x2E);

    [Display(GroupName = "色", Name = "内側の色")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0xFF, 0xF4, 0xB0);

    [Display(GroupName = "位置と大きさ", Name = "足元のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "足元のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(88, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "幅")]
    [AnimationSlider("F0", "px", 40, 800)]
    public Animation BodyWidth { get; } = new Animation(260, 5, 6000);

    [Display(GroupName = "位置と大きさ", Name = "高さ")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation BodyHeight { get; } = new Animation(640, 10, 8000);

    [Display(GroupName = "動き", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Intensity { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "動き", Name = "勢い")]
    [AnimationSlider("F0", "%", 0, 400)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "動き", Name = "舞い上がる粒の数")]
    [AnimationSlider("F0", "個", 0, 300)]
    public Animation Particles { get; } = new Animation(70, 0, 3000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, BodyWidth, BodyHeight, Intensity, Speed, Particles];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float ts = c.Time * c.F(Speed) / 100f, t = c.Time;
        var foot = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float W = c.F(BodyWidth), H = c.F(BodyHeight), I = c.F(Intensity) / 100f;
        int np = (int)c.F(Particles);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            if (Kind == AuraKind.Aura)
            {
                // 足元の光だまり
                float gr = W * 1.1f;
                using (var gb = Gfx.Radial(dc, foot, gr, (0f, Gfx.C4(Color1, 0.45f * I)), (1f, Gfx.C4(Color1, 0f))))
                    dc.FillEllipse(new Ellipse(foot, gr, gr * 0.28f), gb);

                // 揺らめきながら立ちのぼる気（細い炎状の筋）
                var rng = new Random(Seed);
                for (int i = 0; i < 70; i++)
                {
                    float rx = rng.NextF() * 2f - 1f, ph = rng.NextF(), sp = 0.6f + 0.8f * rng.NextF(), hh = 0.5f + 0.5f * rng.NextF();
                    float u = Gfx.Mod(ts * 0.6f * sp + ph, 1f);
                    float x = foot.X + rx * W * 0.5f * (1f - 0.35f * u) + MathF.Sin(t * 3f * sp + ph * 20f + u * 6f) * 14f * u;
                    float yTop = foot.Y - H * hh * u;
                    var p0 = new Vector2(x, MathF.Min(foot.Y, yTop + H * 0.22f * hh));
                    var p1 = new Vector2(x + MathF.Sin(u * 5f + ph * 9f) * 8f, yTop);
                    float a = (1f - u) * 0.55f * I * (0.6f + 0.4f * MathF.Sin(t * 12f + ph * 30f));
                    var col = Gfx.Mix(Color2, Color1, u);
                    Gfx.FadeLine(dc, brush, p0, p1, 5f * (1f - 0.5f * u), 0.6f, col, a, 6, 0.8f);
                }
                // 芯になる大きな光
                float cr = W * 0.55f;
                using (var cb = Gfx.Radial(dc, foot + new Vector2(0f, -H * 0.25f), cr, (0f, Gfx.C4(Color2, 0.20f * I)), (1f, Gfx.C4(Color1, 0f))))
                    dc.FillEllipse(new Ellipse(foot + new Vector2(0f, -H * 0.25f), cr, H * 0.45f), cb);
            }
            else
            {
                // 光の柱：中心が明るく、上に向かって薄れる縦の帯
                float top = foot.Y - H;
                using (var lg = Gfx.Linear(dc, foot, new Vector2(foot.X, top), (0f, Gfx.C4(Color1, 1f)), (0.6f, Gfx.C4(Color1, 0.5f)), (1f, Gfx.C4(Color1, 0f))))
                {
                    Gfx.Rect(dc, lg, foot.X - W * 0.5f, top, W, H);
                }
                using (var lg2 = Gfx.Linear(dc, foot, new Vector2(foot.X, top), (0f, Gfx.C4(Color2, 1f)), (0.6f, Gfx.C4(Color2, 0.6f)), (1f, Gfx.C4(Color2, 0f))))
                {
                    Gfx.Rect(dc, lg2, foot.X - W * 0.22f, top, W * 0.44f, H);
                }
                using (var lg3 = Gfx.Linear(dc, foot, new Vector2(foot.X, top), (0f, Gfx.C4(white, 1f)), (1f, Gfx.C4(white, 0f))))
                {
                    Gfx.Rect(dc, lg3, foot.X - W * 0.07f, top, W * 0.14f, H);
                }
                // 光に強弱をつけるゆらぎ（半透明の帯を重ねる）
                for (int i = 0; i < 6; i++)
                {
                    float x = foot.X + MathF.Sin(t * (0.6f + i * 0.2f) + i * 2f) * W * 0.3f;
                    brush.Color = Gfx.C4(Color2, 0.05f * I);
                    Gfx.Rect(dc, brush, x - W * 0.05f, top, W * 0.1f, H);
                }
                // 足元の波紋
                for (int k = 0; k < 3; k++)
                {
                    float u = Gfx.Mod(ts * 0.5f + k / 3f, 1f);
                    float rr = W * (0.4f + 1.2f * u);
                    brush.Color = Gfx.C4(Color2, 0.6f * (1f - u) * I);
                    dc.DrawEllipse(new Ellipse(foot, rr, rr * 0.26f), brush, 2.5f * (1f - u) + 0.8f);
                }
                using (var fb = Gfx.Radial(dc, foot, W * 1.1f, (0f, Gfx.C4(white, 0.6f * I)), (0.4f, Gfx.C4(Color1, 0.25f * I)), (1f, Gfx.C4(Color1, 0f))))
                    dc.FillEllipse(new Ellipse(foot, W * 1.1f, W * 0.3f), fb);
            }

            // 舞い上がる光の粒
            var rp = new Random(Seed + 5);
            for (int i = 0; i < np; i++)
            {
                float rx = rp.NextF() * 2f - 1f, ph = rp.NextF(), sp = 0.4f + 0.9f * rp.NextF(), r = 1f + 2.2f * rp.NextF();
                float u = Gfx.Mod(ts * 0.35f * sp + ph, 1f);
                var p = new Vector2(foot.X + rx * W * 0.55f + MathF.Sin(t * 2f + ph * 30f) * 12f, foot.Y - u * H * 1.05f);
                brush.Color = Gfx.C4(Gfx.Mix(Color2, white, 0.4f), 0.9f * MathF.Sin(u * MathF.PI) * I);
                dc.FillEllipse(new Ellipse(p, r, r), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}

public enum ElementKind
{
    [Display(Name = "炎")] Fire,
    [Display(Name = "氷")] Ice,
    [Display(Name = "雷")] Thunder,
    [Display(Name = "風")] Wind,
    [Display(Name = "闇")] Dark,
    [Display(Name = "光")] Light,
}
