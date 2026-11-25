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
//  雷
// =====================================================================
public sealed class LightningPlugin : IShapePlugin
{
    public string Name => "演出：雷";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LightningParameter(sharedData);
}

public sealed class LightningParameter : EffectShapeParameterBase
{
    public LightningParameter() : this(null) { }
    public LightningParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "雷", Name = "稲妻の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xF2, 0xF6, 0xFF);

    [Display(GroupName = "雷", Name = "光（グロー）の色")]
    [ColorPicker]
    public WpfColor GlowColor { get => glowColor; set => Set(ref glowColor, value); }
    WpfColor glowColor = WpfColor.FromRgb(0x6E, 0xA0, 0xFF);

    [Display(GroupName = "雷", Name = "発生間隔")]
    [AnimationSlider("F2", "秒", 0.5, 10)]
    public Animation Interval { get; } = new Animation(3, 0.2, 600);

    [Display(GroupName = "雷", Name = "光っている時間")]
    [AnimationSlider("F2", "秒", 0.1, 2)]
    public Animation Duration { get; } = new Animation(0.5, 0.05, 10);

    [Display(GroupName = "雷", Name = "太さ")]
    [AnimationSlider("F1", "px", 1, 20)]
    public Animation Thickness { get; } = new Animation(4, 0.5, 100);

    [Display(GroupName = "雷", Name = "ギザギザ")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Jag { get; } = new Animation(50, 0, 1000);

    [Display(GroupName = "雷", Name = "枝分かれ数")]
    [AnimationSlider("F0", "本", 0, 10)]
    public Animation Branches { get; } = new Animation(3, 0, 30);

    [Display(GroupName = "雷", Name = "画面フラッシュ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flash { get; } = new Animation(35, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Interval, Duration, Thickness, Jag, Branches, Flash];

    static float Env(float u)
    {
        if (u < 0.12f) return 1f;
        if (u < 0.25f) return 0.25f;
        if (u < 0.40f) return 0.85f;
        return MathF.Max(0f, 1f - (u - 0.4f) / 0.6f) * 0.6f;
    }

    static List<Vector2> Bolt(Random rng, Vector2 a, Vector2 b, float jag, int seg)
    {
        var pts = new List<Vector2> { a };
        var d = b - a;
        var nrm = Vector2.Normalize(new Vector2(-d.Y, d.X));
        for (int i = 1; i < seg; i++)
        {
            float t = i / (float)seg;
            pts.Add(a + d * t + nrm * ((rng.NextF() * 2f - 1f) * jag));
        }
        pts.Add(b);
        return pts;
    }

    void DrawBolt(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, List<Vector2> pts, float th, float env)
    {
        brush.Color = Gfx.C4(GlowColor, env * 0.18f);
        Gfx.Polyline(dc, brush, pts, th * 7f);
        brush.Color = Gfx.C4(GlowColor, env * 0.40f);
        Gfx.Polyline(dc, brush, pts, th * 3f);
        brush.Color = Gfx.C4(Color, env);
        Gfx.Polyline(dc, brush, pts, th);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float interval = MathF.Max(0.2f, c.F(Interval));
        float dur = MathF.Max(0.05f, c.F(Duration));
        int idx = (int)MathF.Floor(c.Time / interval);
        float local = c.Time - idx * interval;
        if (local >= dur) return;

        float env = Env(local / dur);
        float th = c.F(Thickness), jag = c.F(Jag);
        var rng = new Random(Seed * 1009 + idx * 7919);

        float x0 = w * (0.2f + 0.6f * rng.NextF());
        float x1 = x0 + (rng.NextF() - 0.5f) * w * 0.2f;
        float y1 = h * (0.7f + 0.3f * rng.NextF());
        var main = Bolt(rng, new Vector2(x0, -10f), new Vector2(x1, y1), jag, 16);

        using var brush = dc.CreateSolidColorBrush(default);

        // 画面フラッシュ
        float flash = c.F(Flash) / 100f;
        if (flash > 0f)
        {
            brush.Color = Gfx.C4(GlowColor, env * flash * 0.6f);
            Gfx.Rect(dc, brush, 0, 0, w, h);
        }

        DrawBolt(dc, brush, main, th, env);

        int nb = (int)c.F(Branches);
        for (int i = 0; i < nb; i++)
        {
            int pi = rng.Next(3, Math.Max(4, main.Count - 3));
            var s = main[pi];
            var e = s + new Vector2((rng.NextF() - 0.5f) * w * 0.3f, h * (0.15f + 0.25f * rng.NextF()));
            DrawBolt(dc, brush, Bolt(rng, s, e, jag * 0.6f, 8), th * 0.5f, env * 0.8f);
        }
    }
}
