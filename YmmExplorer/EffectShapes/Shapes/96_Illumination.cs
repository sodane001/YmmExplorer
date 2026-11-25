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

public enum IllumPattern
{
    [Display(Name = "流れる")] Chase,
    [Display(Name = "交互に点滅")] Alternate,
    [Display(Name = "きらめき")] Twinkle,
    [Display(Name = "ランダム")] Random,
    [Display(Name = "波")] Wave,
    [Display(Name = "全部を順番に")] Cycle,
}

// =====================================================================
//  イルミネーション
//   木や壁に飾る電飾が、またたきパターンを変えて光る
// =====================================================================
public sealed class IlluminationPlugin : IShapePlugin
{
    public string Name => "演出：イルミネーション";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new IlluminationParameter(sharedData);
}

public sealed class IlluminationParameter : EffectShapeParameterBase
{
    public IlluminationParameter() : this(null) { }
    public IlluminationParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "光り方", Name = "パターン")]
    [EnumComboBox]
    public IllumPattern Pattern { get => pattern; set => Set(ref pattern, value); }
    IllumPattern pattern = IllumPattern.Cycle;

    [Display(GroupName = "光り方", Name = "速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "光り方", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Glow { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "色", Name = "いろいろな色にする")]
    [ToggleSlider]
    public bool Multicolor { get => multicolor; set => Set(ref multicolor, value); }
    bool multicolor = true;

    [Display(GroupName = "色", Name = "色1（1色の時はこの色）")]
    [ColorPicker]
    public WpfColor Color1 { get => color1; set => Set(ref color1, value); }
    WpfColor color1 = WpfColor.FromRgb(0xFF, 0xE2, 0xA0);

    [Display(GroupName = "色", Name = "色2")]
    [ColorPicker]
    public WpfColor Color2 { get => color2; set => Set(ref color2, value); }
    WpfColor color2 = WpfColor.FromRgb(0xFF, 0x4C, 0x4C);

    [Display(GroupName = "色", Name = "色3")]
    [ColorPicker]
    public WpfColor Color3 { get => color3; set => Set(ref color3, value); }
    WpfColor color3 = WpfColor.FromRgb(0x4C, 0xFF, 0x7A);

    [Display(GroupName = "色", Name = "色4")]
    [ColorPicker]
    public WpfColor ColorD { get => color4; set => Set(ref color4, value); }
    WpfColor color4 = WpfColor.FromRgb(0x4C, 0xA8, 0xFF);

    [Display(GroupName = "電飾", Name = "横に渡す電飾の本数")]
    [AnimationSlider("F0", "本", 0, 10)]
    public Animation Strings { get; } = new Animation(3, 0, 50);

    [Display(GroupName = "電飾", Name = "たるみ")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Sag { get; } = new Animation(60, -1000, 1000);

    [Display(GroupName = "電飾", Name = "電球の間隔")]
    [AnimationSlider("F0", "px", 15, 150)]
    public Animation Spacing { get; } = new Animation(48, 4, 1000);

    [Display(GroupName = "電飾", Name = "電球の大きさ")]
    [AnimationSlider("F1", "px", 2, 20)]
    public Animation BulbSize { get; } = new Animation(6, 0.5, 200);

    [Display(GroupName = "電飾", Name = "コードの色")]
    [ColorPicker]
    public WpfColor WireColor { get => wireColor; set => Set(ref wireColor, value); }
    WpfColor wireColor = WpfColor.FromRgb(0x1C, 0x2A, 0x1C);

    [Display(GroupName = "ツリー", Name = "ツリーを表示する")]
    [ToggleSlider]
    public bool Tree { get => tree; set => Set(ref tree, value); }
    bool tree = true;

    [Display(GroupName = "ツリー", Name = "ツリーのX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation TreeX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ツリー", Name = "ツリーの高さ")]
    [AnimationSlider("F0", "px", 100, 1000)]
    public Animation TreeHeight { get; } = new Animation(560, 10, 8000);

    [Display(GroupName = "ツリー", Name = "ツリーの電球の数")]
    [AnimationSlider("F0", "個", 10, 400)]
    public Animation TreeBulbs { get; } = new Animation(140, 0, 3000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Speed, Glow, Strings, Sag, Spacing, BulbSize, TreeX, TreeHeight, TreeBulbs];

    float Brightness(IllumPattern pat, int i, float t, float sp, int seed)
    {
        if (pat == IllumPattern.Cycle)
        {
            int k = (int)MathF.Floor(t * sp / 4f) % 5;
            pat = (IllumPattern)k;
        }
        return pat switch
        {
            IllumPattern.Chase => 0.2f + 0.8f * MathF.Pow(0.5f + 0.5f * MathF.Cos(MathF.PI * 2f * (i / 7f - t * sp * 0.8f)), 4f),
            IllumPattern.Alternate => ((i + (int)MathF.Floor(t * sp * 2f)) % 2 == 0) ? 1f : 0.15f,
            IllumPattern.Twinkle => 0.35f + 0.65f * MathF.Pow(MathF.Abs(MathF.Sin(t * sp * (1.5f + 2.5f * DrawKit.H(seed, i, 1)) + DrawKit.H(seed, i, 2) * 6f)), 5f),
            IllumPattern.Random => DrawKit.H(seed, i, 3 + (int)MathF.Floor(t * sp * 4f)) > 0.45f ? 1f : 0.12f,
            _ => 0.25f + 0.75f * (0.5f + 0.5f * MathF.Sin(i * 0.45f - t * sp * 3f)),
        };
    }

    WpfColor BulbColor(int i)
    {
        if (!Multicolor) return Color1;
        return (i % 4) switch { 0 => Color2, 1 => Color3, 2 => ColorD, _ => Color1 };
    }

    void Bulb(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 p, float r, WpfColor col, float b, float glow)
    {
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        DrawKit.Glow(dc, p, r * 5f * glow, Gfx.C4(col, 0.45f * b));
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(20, 20, 20), 0.6f * (1f - b)), 1f);
        dc.FillEllipse(new Ellipse(p, r, r * 1.2f), brush);
        brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.7f), 0.9f * b);
        dc.FillEllipse(new Ellipse(p - new Vector2(0, r * 0.2f), r * 0.5f, r * 0.6f), brush);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time, sp = c.F(Speed) / 100f, glow = c.F(Glow) / 100f;
        int strings = (int)c.F(Strings);
        float sag = c.F(Sag), spacing = MathF.Max(4f, c.F(Spacing)), r = c.F(BulbSize);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // ---- 横に渡した電飾（たるんだコード）----
            int bulbIndex = 0;
            for (int j = 0; j < strings; j++)
            {
                float y0 = h * (0.08f + 0.16f * j);
                int spans = 2 + (j % 2);
                float spanW = (w + 40f) / spans;
                for (int s = 0; s < spans; s++)
                {
                    float xs = -20f + s * spanW;
                    var wire = new List<Vector2>();
                    for (int k = 0; k <= 30; k++)
                    {
                        float u = k / 30f;
                        wire.Add(new Vector2(xs + spanW * u, y0 + sag * 4f * u * (1f - u)));
                    }
                    brush.Color = Gfx.C4(WireColor, 0.9f);
                    Gfx.Polyline(dc, brush, wire, 2f);
                    int nb = Math.Max(1, (int)(spanW / spacing));
                    for (int k = 0; k < nb; k++)
                    {
                        float u = (k + 0.5f) / nb;
                        var p = new Vector2(xs + spanW * u, y0 + sag * 4f * u * (1f - u) + r * 1.4f);
                        int id = bulbIndex++;
                        Bulb(dc, brush, p, r, BulbColor(id), Brightness(Pattern, id, t, sp, Seed), glow);
                    }
                }
            }

            // ---- ツリー ----
            if (Tree)
            {
                float TH = c.F(TreeHeight), tx = c.F(TreeX) / 100f * w;
                float baseY = h - h * 0.04f, topY = baseY - TH;
                var cone = new List<Vector2> { new(tx, topY), new(tx + TH * 0.36f, baseY - TH * 0.08f), new(tx - TH * 0.36f, baseY - TH * 0.08f) };
                brush.Color = Gfx.C4(0.04f, 0.16f, 0.08f, 0.85f);
                Gfx.FillPoly(dc, brush, cone);
                brush.Color = Gfx.C4(0.2f, 0.12f, 0.06f, 0.9f);
                dc.FillRectangle(new RawRectF(tx - TH * 0.04f, baseY - TH * 0.08f, tx + TH * 0.04f, baseY), brush);

                int nb = (int)c.F(TreeBulbs);
                const float turns = 6f;
                var order = new List<(float z, int k, Vector2 p)>();
                for (int k = 0; k < nb; k++)
                {
                    float s = (k + 0.5f) / nb;
                    float y = topY + TH * 0.05f + s * TH * 0.85f;
                    float rad = s * TH * 0.34f;
                    float a = s * turns * MathF.PI * 2f + t * 0.4f;
                    order.Add((MathF.Cos(a), k, new Vector2(tx + MathF.Sin(a) * rad, y + MathF.Cos(a) * rad * 0.12f)));
                }
                order.Sort((a, b) => a.z.CompareTo(b.z));
                foreach (var (z, k, p) in order)
                {
                    float depth = 0.5f + 0.5f * z;
                    Bulb(dc, brush, p, r * (0.6f + 0.4f * depth), BulbColor(k + 1), Brightness(Pattern, k, t, sp, Seed) * (0.45f + 0.55f * depth), glow);
                }
                // てっぺんの星
                var top = new Vector2(tx, topY);
                float sz = TH * 0.07f;
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, top, sz * 4f * glow, Gfx.C4(0.95f, 0.85f, 0.4f, 0.6f));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                brush.Color = Gfx.C4(1f, 0.86f, 0.3f, 1f);
                Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(top, sz, 5, 0.45f, MathF.Sin(t) * 0.1f));
                brush.Color = Gfx.C4(1f, 1f, 0.85f, 0.9f);
                Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(top, sz * 0.5f, 5, 0.45f, MathF.Sin(t) * 0.1f));
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Twinkle(dc, brush, top, sz * 2.5f * (0.7f + 0.3f * MathF.Sin(t * 3f)), 2f, WpfColor.FromRgb(255, 240, 200), 0.7f, t * 0.3f);
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
