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

public enum GachaRarity
{
    [Display(Name = "★1（白）")] N,
    [Display(Name = "★2（青）")] R,
    [Display(Name = "★3（金）")] SR,
    [Display(Name = "★4（虹）")] SSR,
}

// =====================================================================
//  ガチャ演出
//   光る玉がふるえ、割れて、レア度に応じた色の光と星が出る
// =====================================================================
public sealed class GachaPlugin : IShapePlugin
{
    public string Name => "演出：ガチャ演出";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GachaParameter(sharedData);
}

public sealed class GachaParameter : EffectShapeParameterBase
{
    public GachaParameter() : this(null) { }
    public GachaParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "ガチャ", Name = "レア度")]
    [EnumComboBox]
    public GachaRarity Rarity { get => rarity; set => Set(ref rarity, value); }
    GachaRarity rarity = GachaRarity.SSR;

    [Display(GroupName = "ガチャ", Name = "割れる前は白く光る（レア度をかくす）")]
    [ToggleSlider]
    public bool Hide { get => hide; set => Set(ref hide, value); }
    bool hide = true;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(48, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "玉の半径")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation OrbSize { get; } = new Animation(95, 2, 3000);

    [Display(GroupName = "動き", Name = "割れるまでの時間")]
    [AnimationSlider("F2", "秒", 0.3, 5)]
    public Animation BurstTime { get; } = new Animation(1.4, 0.05, 600);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, OrbSize, BurstTime];

    WpfColor RarityColor(float t, int k = 0) => Rarity switch
    {
        GachaRarity.N => WpfColor.FromRgb(0xE8, 0xEC, 0xF2),
        GachaRarity.R => WpfColor.FromRgb(0x4C, 0xA8, 0xFF),
        GachaRarity.SR => WpfColor.FromRgb(0xFF, 0xC8, 0x2E),
        _ => Gfx.Hsv(t * 140f + k * 37f, 0.75f, 1f),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(OrbSize), burst = c.F(BurstTime);
        float since = t - burst;
        int stars = (int)Rarity + 1;
        var white = WpfColor.FromRgb(255, 255, 255);
        var pre = Hide ? WpfColor.FromRgb(0xF4, 0xF6, 0xFF) : RarityColor(t);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            if (since < 0f)
            {
                // ---- ためる：集まる光とふるえる玉 ----
                float k = Math.Clamp(t / burst, 0f, 1f);
                float shake = MathF.Pow(k, 3f) * R * 0.08f;
                var p = ctr + new Vector2(MathF.Sin(t * 70f), MathF.Cos(t * 61f)) * shake;
                for (int i = 0; i < 30; i++)
                {
                    float u = DrawKit.Frac(t * 1.2f + DrawKit.H(Seed, i, 1));
                    float a = DrawKit.H(Seed, i, 2) * MathF.PI * 2f;
                    var sp = p + new Vector2(MathF.Cos(a), MathF.Sin(a)) * R * (3.5f - 2.5f * u);
                    brush.Color = Gfx.C4(pre, 0.8f * u * k);
                    dc.FillEllipse(new Ellipse(sp, 2.5f, 2.5f), brush);
                }
                DrawKit.Glow(dc, p, R * (2f + 1.5f * k), Gfx.C4(pre, 0.4f + 0.4f * k));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                using (var g = Gfx.Radial(dc, p - new Vector2(R * 0.3f, R * 0.35f), R * 1.4f,
                    (0f, Gfx.C4(white, 1f)), (0.5f, Gfx.C4(Gfx.Mix(pre, white, 0.5f), 1f)), (1f, Gfx.C4(Gfx.Mix(pre, WpfColor.FromRgb(40, 40, 70), 0.4f), 1f))))
                    dc.FillEllipse(new Ellipse(p, R, R), g);
                // ひび（割れる直前）
                if (k > 0.65f)
                {
                    brush.Color = Gfx.C4(white, Gfx.Smooth(0.65f, 1f, k));
                    dc.PrimitiveBlend = PrimitiveBlend.Add;
                    var crack = new List<Vector2> { p + new Vector2(-R * 0.1f, -R * 0.9f), p + new Vector2(R * 0.12f, -R * 0.4f), p + new Vector2(-R * 0.15f, 0f), p + new Vector2(R * 0.18f, R * 0.45f), p + new Vector2(0, R * 0.9f) };
                    Gfx.Polyline(dc, brush, PathKit.Part(crack, Gfx.Smooth(0.65f, 0.95f, k)), 3f);
                }
            }
            else
            {
                // ---- 割れた：光の柱・光線・かけら・星 ----
                float e = DrawKit.EaseOut(since / 0.6f);
                var col = RarityColor(t);
                if (since < 0.25f)
                {
                    brush.Color = Gfx.C4(white, (1f - since / 0.25f) * 0.85f);
                    dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
                }
                DrawKit.Glow(dc, ctr, R * (3f + 2f * e), Gfx.C4(col, 0.55f));
                int rays = Rarity >= GachaRarity.SR ? 24 : 14;
                for (int i = 0; i < rays; i++)
                {
                    float a = i * MathF.PI * 2f / rays + t * 0.4f;
                    var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    Gfx.FadeLine(dc, brush, ctr, ctr + d * R * (4f + 2f * DrawKit.H(Seed, i, 3)) * e, R * 0.25f, R * 0.7f, RarityColor(t, i), 0.3f, 10, 1.2f);
                }
                if (Rarity >= GachaRarity.SR)
                {
                    using var lg = Gfx.Linear(dc, new Vector2(0, ctr.Y + R), new Vector2(0, 0), (0f, Gfx.C4(col, 0.5f)), (1f, Gfx.C4(col, 0f)));
                    dc.FillRectangle(new RawRectF(ctr.X - R * 0.9f * e, 0, ctr.X + R * 0.9f * e, ctr.Y + R), lg);
                }
                // かけら
                for (int i = 0; i < 18; i++)
                {
                    float a = DrawKit.H(Seed, i, 4) * MathF.PI * 2f;
                    float dist = R * (0.5f + 3.5f * DrawKit.EaseOut(since / 1.2f)) * (0.6f + 0.6f * DrawKit.H(Seed, i, 5));
                    var p = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * dist + new Vector2(0, since * since * R * 1.5f);
                    float sz = R * 0.12f * (0.5f + DrawKit.H(Seed, i, 6));
                    float fa = Math.Clamp(1f - since / 1.5f, 0f, 1f);
                    brush.Color = Gfx.C4(Gfx.Mix(RarityColor(t, i), white, 0.5f), fa);
                    var tri = DrawKit.Place(new[] { new Vector2(0, -sz), new Vector2(sz * 0.8f, sz * 0.6f), new Vector2(-sz * 0.7f, sz * 0.5f) }, p, since * 8f + i);
                    Gfx.FillPoly(dc, brush, tri);
                }
                // きらめき
                for (int i = 0; i < 16; i++)
                {
                    float ph = DrawKit.Frac(t * 0.7f + DrawKit.H(Seed, i, 7));
                    var sp = ctr + new Vector2((DrawKit.H(Seed, i, 8) - 0.5f) * R * 7f, (DrawKit.H(Seed, i, 9) - 0.5f) * R * 4f);
                    DrawKit.Twinkle(dc, brush, sp, R * 0.25f * MathF.Sin(MathF.PI * ph), 2f, RarityColor(t, i + 3), 0.9f * MathF.Sin(MathF.PI * ph));
                }
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                // 星（レア度）が順に出る
                float sr = R * 0.38f;
                float total = stars * sr * 2.3f;
                for (int i = 0; i < stars; i++)
                {
                    float st = since - 0.35f - i * 0.18f;
                    if (st < 0f) continue;
                    float pop = DrawKit.EaseOutBack(st / 0.3f);
                    var sp = new Vector2(ctr.X - total / 2f + sr * 1.15f + i * sr * 2.3f, ctr.Y + R * 1.9f);
                    var sc = Rarity == GachaRarity.SSR ? RarityColor(t, i) : Rarity == GachaRarity.N ? WpfColor.FromRgb(0xC8, 0xCC, 0xD4) : WpfColor.FromRgb(0xFF, 0xD2, 0x3C);
                    brush.Color = Gfx.C4(Gfx.Mix(sc, WpfColor.FromRgb(60, 30, 0), 0.6f), 1f);
                    Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(sp, sr * pop * 1.15f, 5, 0.48f, 0f));
                    brush.Color = Gfx.C4(sc, 1f);
                    Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(sp, sr * pop, 5, 0.48f, 0f));
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
