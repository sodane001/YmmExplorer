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
//  曇りガラス
// =====================================================================
public sealed class FrostedGlassPlugin : IShapePlugin
{
    public string Name => "演出：曇りガラス";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FrostedGlassParameter(sharedData);
}

public sealed class FrostedGlassParameter : EffectShapeParameterBase
{
    public FrostedGlassParameter() : this(null) { }
    public FrostedGlassParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "曇りガラス", Name = "曇りの色")]
    [ColorPicker]
    public WpfColor FogColor { get => fogColor; set => Set(ref fogColor, value); }
    WpfColor fogColor = WpfColor.FromRgb(0xDD, 0xE6, 0xEE);

    [Display(GroupName = "曇りガラス", Name = "曇りの濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FogOpacity { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "水滴", Name = "水滴の数")]
    [AnimationSlider("F0", "個", 0, 500)]
    public Animation DropletCount { get; } = new Animation(120, 0, 5000);

    [Display(GroupName = "水滴", Name = "水滴の最大サイズ")]
    [AnimationSlider("F1", "px", 2, 40)]
    public Animation DropletSize { get; } = new Animation(10, 1, 200);

    [Display(GroupName = "水滴", Name = "垂れる水滴の数")]
    [AnimationSlider("F0", "個", 0, 100)]
    public Animation DripCount { get; } = new Animation(14, 0, 1000);

    [Display(GroupName = "水滴", Name = "垂れる速度")]
    [AnimationSlider("F0", "px/s", 5, 300)]
    public Animation DripSpeed { get; } = new Animation(40, 1, 3000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [FogOpacity, DropletCount, DropletSize, DripCount, DripSpeed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float op = c.F(FogOpacity) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);
        var gray = WpfColor.FromRgb(0x70, 0x80, 0x90);

        using var brush = dc.CreateSolidColorBrush(default);

        // --- 曇り（ベース）---
        brush.Color = Gfx.C4(FogColor, op * 0.85f);
        Gfx.Rect(dc, brush, 0, 0, w, h);

        // --- 曇りのムラ（ゆっくり動く明暗）---
        var rng = new Random(Seed);
        for (int i = 0; i < 24; i++)
        {
            float x = rng.NextF() * w, y = rng.NextF() * h;
            float r = (0.15f + 0.25f * rng.NextF()) * MathF.Max(w, h);
            var col = (i % 2 == 0) ? white : gray;
            var ctr = new Vector2(x + MathF.Sin(t * 0.1f + i) * 30f, y);
            using var b = Gfx.Radial(dc, ctr, r, (0f, Gfx.C4(col, op * 0.25f)), (1f, Gfx.C4(col, 0f)));
            dc.FillEllipse(new Ellipse(ctr, r, r), b);
        }

        // --- 水滴の位置を決定 ---
        float size = c.F(DropletSize);
        var drops = new List<(float x, float y, float r)>();
        int nd = (int)c.F(DropletCount);
        var rd = new Random(Seed + 1);
        for (int i = 0; i < nd; i++)
        {
            float rr = rd.NextF();
            drops.Add((rd.NextF() * w, rd.NextF() * h, size * (0.25f + 0.75f * rr * rr)));
        }

        var drips = new List<(float x, float y, float r, float tail)>();
        int nv = (int)c.F(DripCount);
        float vs = c.F(DripSpeed);
        var rv = new Random(Seed + 2);
        for (int i = 0; i < nv; i++)
        {
            float x = rv.NextF() * w;
            float r = size * (0.6f + 0.5f * rv.NextF());
            float v = vs * (0.5f + rv.NextF());
            float ph = rv.NextF();
            float tail = r * 10f;
            float span = h + tail + 2f * r;
            float y = -r + Gfx.Mod(t * v + ph * span, span);
            drips.Add((x, y, r, tail));
        }

        // --- 曇りを消す（水滴部分は透明に）---
        dc.PrimitiveBlend = PrimitiveBlend.Copy;
        try
        {
            brush.Color = new Color4(0f, 0f, 0f, 0f);
            foreach (var d in drops)
                dc.FillEllipse(new Ellipse(new Vector2(d.x, d.y), d.r, d.r), brush);
            foreach (var d in drips)
            {
                dc.DrawLine(new Vector2(d.x, d.y - d.tail), new Vector2(d.x, d.y), brush, d.r * 0.8f);
                dc.FillEllipse(new Ellipse(new Vector2(d.x, d.y), d.r, d.r), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }

        // --- 水滴の縁とハイライト ---
        void Rim(float x, float y, float r)
        {
            var ctr = new Vector2(x, y);
            brush.Color = Gfx.C4(gray, 0.35f);
            dc.DrawEllipse(new Ellipse(ctr, r, r), brush, 1f);
            brush.Color = Gfx.C4(white, 0.75f);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(-r * 0.3f, -r * 0.3f), r * 0.28f, r * 0.2f), brush);
        }
        foreach (var d in drops) Rim(d.x, d.y, d.r);
        foreach (var d in drips) Rim(d.x, d.y, d.r);
    }
}
