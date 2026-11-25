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

public enum TreeSeason
{
    [Display(Name = "桜並木")] Sakura,
    [Display(Name = "紅葉のトンネル")] Autumn,
    [Display(Name = "新緑")] Green,
    [Display(Name = "銀杏並木")] Ginkgo,
}

// =====================================================================
//  桜並木・紅葉のトンネル
//   奥へ続く道と、両側の並木。ゆっくり前へ進み、花びら・葉が舞う
// =====================================================================
public sealed class TreeTunnelPlugin : IShapePlugin
{
    public string Name => "演出：桜並木・紅葉のトンネル";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TreeTunnelParameter(sharedData);
}

public sealed class TreeTunnelParameter : EffectShapeParameterBase
{
    public TreeTunnelParameter() : this(null) { }
    public TreeTunnelParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "景色", Name = "季節")]
    [EnumComboBox]
    public TreeSeason Season { get => season; set => Set(ref season, value); }
    TreeSeason season = TreeSeason.Sakura;

    [Display(GroupName = "景色", Name = "消える点の高さ（Y）")]
    [AnimationSlider("F0", "%", 20, 70)]
    public Animation VanishY { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "景色", Name = "進む速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(60, 0, 2000);

    [Display(GroupName = "景色", Name = "舞う花びら・葉の数")]
    [TextBoxSlider("F0", "枚", 0, 300)]
    [Range(0, 2000)]
    public int Petals { get => petals; set => Set(ref petals, value); }
    int petals = 90;

    [Display(GroupName = "景色", Name = "空の色")]
    [ColorPicker]
    public WpfColor SkyColor { get => skyColor; set => Set(ref skyColor, value); }
    WpfColor skyColor = WpfColor.FromRgb(0xBF, 0xE3, 0xFF);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [VanishY, Speed];

    (WpfColor a, WpfColor b) Leaves => Season switch
    {
        TreeSeason.Autumn => (WpfColor.FromRgb(0xE0, 0x4A, 0x2A), WpfColor.FromRgb(0xF2, 0x9A, 0x30)),
        TreeSeason.Green => (WpfColor.FromRgb(0x4C, 0xA8, 0x4A), WpfColor.FromRgb(0x8C, 0xCF, 0x5A)),
        TreeSeason.Ginkgo => (WpfColor.FromRgb(0xF2, 0xC2, 0x1E), WpfColor.FromRgb(0xFF, 0xE0, 0x5C)),
        _ => (WpfColor.FromRgb(0xF7, 0xB2, 0xC8), WpfColor.FromRgb(0xFF, 0xDA, 0xE6)),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float vy = c.F(VanishY) / 100f * h, sp = c.F(Speed) / 100f;
        var vp = new Vector2(w / 2f, vy);
        var (la, lb) = Leaves;
        using var brush = dc.CreateSolidColorBrush(default);
        // 空と地面
        using (var g = Gfx.Linear(dc, new Vector2(0, 0), new Vector2(0, vy), (0f, Gfx.C4(Gfx.Mix(SkyColor, WpfColor.FromRgb(0x50, 0x80, 0xD0), 0.3f), 1f)), (1f, Gfx.C4(SkyColor, 1f))))
            dc.FillRectangle(new RawRectF(0, 0, w, vy + 1f), g);
        var ground = Season == TreeSeason.Green ? WpfColor.FromRgb(0x6E, 0x9E, 0x4A) : WpfColor.FromRgb(0x9C, 0x8A, 0x6A);
        brush.Color = Gfx.C4(Gfx.Mix(ground, la, Season == TreeSeason.Green ? 0f : 0.25f), 1f);
        dc.FillRectangle(new RawRectF(0, vy, w, h), brush);
        // 道(消える点へ細くなる)
        brush.Color = Gfx.C4(0.82f, 0.78f, 0.7f, 1f);
        Gfx.FillPoly(dc, brush, new List<Vector2> { vp + new Vector2(-4f, 0f), vp + new Vector2(4f, 0f), new(w * 0.78f, h), new(w * 0.22f, h) });

        // 木: 奥から手前へ。z(0=奥,1=手前)を、時間とともに手前へ動かしてくり返す
        int perSide = 9;
        var trees = new List<(float z, int side, int id)>();
        for (int i = 0; i < perSide; i++)
            for (int side = -1; side <= 1; side += 2)
            {
                float z = Gfx.Mod(i / (float)perSide + t * 0.05f * sp + (side > 0 ? 0.5f / perSide : 0f), 1f);
                trees.Add((z, side, i * 2 + (side > 0 ? 1 : 0)));
            }
        foreach (var (z, side, id) in trees.OrderBy(q => q.z))
        {
            // 透視: 手前ほど大きく、外側・下へ
            float s = 0.08f + z * z * 1.6f;
            float x = vp.X + side * (w * 0.06f + s * w * 0.32f);
            float baseY = vy + s * (h - vy) * 0.62f;
            float trunkH = h * 0.45f * s, trunkW = w * 0.018f * s;
            float fade = Math.Clamp(z * 6f, 0f, 1f) * Math.Clamp((1f - z) * 8f, 0f, 1f);
            var trunk = Gfx.Mix(WpfColor.FromRgb(0x5A, 0x3A, 0x2A), SkyColor, (1f - z) * 0.5f);
            brush.Color = Gfx.C4(trunk, fade);
            Gfx.FillPoly(dc, brush, new List<Vector2> { new(x - trunkW, baseY), new(x + trunkW, baseY), new(x + trunkW * 0.6f - side * trunkW, baseY - trunkH), new(x - trunkW * 0.6f - side * trunkW, baseY - trunkH) });
            // 枝ぶり(内側へ張り出す、もこもこの塊)
            var crown = new Vector2(x - side * trunkW * 2f, baseY - trunkH);
            float cr = w * 0.12f * s;
            for (int k = 0; k < 7; k++)
            {
                var off = new Vector2((DrawKit.H(Seed, id, k) - 0.5f - side * 0.5f) * cr * 1.8f, (DrawKit.H(Seed, id, k + 10) - 0.7f) * cr * 1.1f);
                float rr = cr * (0.45f + DrawKit.H(Seed, id, k + 20) * 0.35f);
                var col = Gfx.Mix(Gfx.Mix(la, lb, DrawKit.H(Seed, id, k + 30)), SkyColor, (1f - z) * 0.45f);
                brush.Color = Gfx.C4(col, fade);
                dc.FillEllipse(new Ellipse(crown + off, rr, rr * 0.85f), brush);
            }
        }

        // 舞う花びら・葉
        for (int i = 0; i < Petals; i++)
        {
            float fall = 40f + DrawKit.H(Seed, i, 40) * 60f;
            float y = Gfx.Mod(DrawKit.H(Seed, i, 41) * h + t * fall, h + 40f) - 20f;
            float x = Gfx.Mod(DrawKit.H(Seed, i, 42) * w + MathF.Sin(t * 1.2f + i) * 40f + t * 20f * sp, w + 40f) - 20f;
            float sz = (6f + DrawKit.H(Seed, i, 43) * 10f) * MathF.Min(w / 1920f, h / 1080f);
            float spin = t * (1.5f + DrawKit.H(Seed, i, 44) * 2f) + i;
            var col = Gfx.Mix(la, lb, DrawKit.H(Seed, i, 45));
            brush.Color = Gfx.C4(col, 0.9f);
            var petal = DrawKit.Place(new[] { new Vector2(0f, -1f), new Vector2(0.6f, -0.2f), new Vector2(0.35f, 0.8f), new Vector2(0f, 0.55f), new Vector2(-0.35f, 0.8f), new Vector2(-0.6f, -0.2f) }, new Vector2(x, y), spin, sz, sz * MathF.Abs(MathF.Cos(spin * 0.7f)) + sz * 0.2f);
            Gfx.FillPoly(dc, brush, petal);
        }
    }
}
