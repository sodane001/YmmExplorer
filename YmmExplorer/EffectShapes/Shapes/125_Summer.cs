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
//  夏（風鈴とうちわ）
//   風にゆれる風鈴と、ぱたぱたあおぐうちわ
// =====================================================================
public sealed class SummerPlugin : IShapePlugin
{
    public string Name => "演出：夏（風鈴とうちわ）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SummerParameter(sharedData);
}

public sealed class SummerParameter : EffectShapeParameterBase
{
    public SummerParameter() : this(null) { }
    public SummerParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "風鈴", Name = "風鈴を表示する")]
    [ToggleSlider]
    public bool Chime { get => chime; set => Set(ref chime, value); }
    bool chime = true;

    [Display(GroupName = "風鈴", Name = "風鈴の数")]
    [AnimationSlider("F0", "個", 1, 5)]
    public Animation ChimeCount { get; } = new Animation(2, 1, 20);

    [Display(GroupName = "風鈴", Name = "ガラスの柄の色")]
    [ColorPicker]
    public WpfColor ChimeColor { get => chimeColor; set => Set(ref chimeColor, value); }
    WpfColor chimeColor = WpfColor.FromRgb(0x3C, 0x8A, 0xE8);

    [Display(GroupName = "うちわ", Name = "うちわを表示する")]
    [ToggleSlider]
    public bool Fan { get => fan; set => Set(ref fan, value); }
    bool fan = true;

    [Display(GroupName = "うちわ", Name = "うちわの色")]
    [ColorPicker]
    public WpfColor FanColor { get => fanColor; set => Set(ref fanColor, value); }
    WpfColor fanColor = WpfColor.FromRgb(0xE8, 0x46, 0x5A);

    [Display(GroupName = "うちわ", Name = "うちわのX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FanX { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "うちわ", Name = "うちわのY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FanY { get; } = new Animation(66, -100, 200);

    [Display(GroupName = "全体", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 250)]
    public Animation ItemScale { get; } = new Animation(100, 5, 1000);

    [Display(GroupName = "全体", Name = "風の強さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Wind { get; } = new Animation(100, 0, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [ChimeCount, FanX, FanY, ItemScale, Wind];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float S = MathF.Min(w, h) / 1080f * c.F(ItemScale) / 100f;
        float wind = c.F(Wind) / 100f;
        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (Chime)
            {
                int n = Math.Max(1, (int)c.F(ChimeCount));
                for (int i = 0; i < n; i++)
                {
                    var hook = new Vector2(w * (0.18f + 0.22f * i), 0);
                    float swing = (MathF.Sin(t * 1.6f + i * 1.3f) * 0.12f + MathF.Sin(t * 3.7f + i) * 0.04f) * wind;
                    DrawChime(dc, brush, hook, S, swing, t + i);
                }
            }
            if (Fan)
            {
                var p = new Vector2(c.F(FanX) / 100f * w, c.F(FanY) / 100f * h);
                float ang = -0.35f + MathF.Sin(t * 7f) * 0.28f;
                DrawFan(dc, brush, p, S, ang);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    void DrawChime(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 hook, float S, float swing, float t)
    {
        float strL = 160f * S;
        var bellTop = hook + new Vector2(MathF.Sin(swing) * strL, MathF.Cos(swing) * strL);
        brush.Color = Gfx.C4(0.9f, 0.9f, 0.85f, 0.8f);
        dc.DrawLine(hook, bellTop, brush, 1.5f);
        float R = 56f * S;
        var bc = bellTop + new Vector2(0, R * 0.9f);
        // 舌と短冊（ガラスより大きくゆれる）
        float s2 = swing * 2.2f + MathF.Sin(t * 5f) * 0.05f;
        var tongue = bc + new Vector2(MathF.Sin(s2) * R * 0.6f, R * 0.5f);
        brush.Color = Gfx.C4(0.9f, 0.9f, 0.85f, 0.8f);
        dc.DrawLine(bellTop, tongue, brush, 1.2f);
        var stripTop = tongue + new Vector2(MathF.Sin(s2) * R * 0.3f, R * 0.35f);
        dc.DrawLine(tongue, stripTop, brush, 1.2f);
        var strip = DrawKit.Place(new[] { new Vector2(-R * 0.28f, 0), new Vector2(R * 0.28f, 0), new Vector2(R * 0.28f, R * 1.7f), new Vector2(-R * 0.28f, R * 1.7f) }, stripTop, -s2 * 1.3f);
        brush.Color = Gfx.C4(0.96f, 0.94f, 0.86f, 1f);
        Gfx.FillPoly(dc, brush, strip);
        brush.Color = Gfx.C4(ChimeColor, 0.8f);
        var sl = DrawKit.Place(new[] { new Vector2(-R * 0.14f, R * 0.35f), new Vector2(R * 0.12f, R * 0.7f), new Vector2(-R * 0.1f, R * 1.05f), new Vector2(R * 0.12f, R * 1.4f) }, stripTop, -s2 * 1.3f);
        Gfx.Polyline(dc, brush, sl, 2.5f * S + 1f);
        dc.FillEllipse(new Ellipse(tongue, R * 0.08f, R * 0.08f), brush);
        // ガラスの鈴（半球）
        var dome = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, R, R * 0.95f, 40, MathF.PI, MathF.PI * 2f), bc, swing);
        var rim = DrawKit.Place(new[] { new Vector2(R, 0), new Vector2(-R, 0) }, bc, swing);
        dome.Add(rim[0]);
        brush.Color = Gfx.C4(0.85f, 0.95f, 1f, 0.45f);
        Gfx.FillPoly(dc, brush, dome);
        // 金魚の柄（赤い丸と尾）
        var fishC = DrawKit.Place(new[] { new Vector2(-R * 0.3f, -R * 0.45f) }, bc, swing)[0];
        brush.Color = Gfx.C4(0.92f, 0.22f, 0.2f, 0.9f);
        dc.FillEllipse(new Ellipse(fishC, R * 0.16f, R * 0.1f), brush);
        Gfx.FillPoly(dc, brush, DrawKit.Place(new[] { new Vector2(R * 0.12f, 0), new Vector2(R * 0.32f, -R * 0.12f), new Vector2(R * 0.3f, R * 0.12f) }, fishC, swing));
        brush.Color = Gfx.C4(ChimeColor, 0.75f);
        var wave = new List<Vector2>();
        for (int k = 0; k <= 12; k++) wave.Add(new Vector2(-R * 0.85f + R * 1.7f * k / 12f, -R * 0.18f + MathF.Sin(k * 1.2f) * R * 0.06f));
        Gfx.Polyline(dc, brush, DrawKit.Place(wave, bc, swing), 3f * S + 1f);
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.7f);
        Gfx.Polyline(dc, brush, dome, 1.5f);
        dc.FillEllipse(new Ellipse(DrawKit.Place(new[] { new Vector2(-R * 0.5f, -R * 0.6f) }, bc, swing)[0], R * 0.1f, R * 0.18f), brush);
        brush.Color = Gfx.C4(0.95f, 0.95f, 0.95f, 0.6f);
        dc.DrawLine(rim[0], rim[1], brush, 2f);
    }

    void DrawFan(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 grip, float S, float ang)
    {
        float R = 150f * S, handle = 170f * S;
        var dir = new Vector2(MathF.Sin(ang), -MathF.Cos(ang));
        var head = grip + dir * (handle + R * 0.9f);
        // 柄
        brush.Color = Gfx.C4(0.55f, 0.38f, 0.2f, 1f);
        dc.DrawLine(grip, grip + dir * (handle + R * 0.2f), brush, 16f * S);
        // 面（少しつぶれた円）と骨
        var face = DrawKit.Place(DrawKit.EllipsePts(Vector2.Zero, R, R * 0.92f, 48), head, ang);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.2f);
        Gfx.FillPoly(dc, brush, face.Select(p => p + new Vector2(6f, 8f)).ToList());
        brush.Color = Gfx.C4(0.98f, 0.96f, 0.9f, 1f);
        Gfx.FillPoly(dc, brush, face);
        var light = Gfx.Mix(FanColor, WpfColor.FromRgb(255, 255, 255), 0.3f);
        // 柄：大きな丸と波
        brush.Color = Gfx.C4(FanColor, 1f);
        dc.FillEllipse(new Ellipse(head + DrawKit.Place(new[] { new Vector2(R * 0.25f, -R * 0.2f) }, Vector2.Zero, ang)[0], R * 0.35f, R * 0.35f), brush);
        brush.Color = Gfx.C4(light, 1f);
        for (int k = 0; k < 3; k++)
        {
            var wv = new List<Vector2>();
            for (int i = 0; i <= 14; i++) wv.Add(new Vector2(-R * 0.8f + R * 1.6f * i / 14f, R * (0.2f + k * 0.18f) + MathF.Sin(i * 1.1f + k) * R * 0.05f));
            Gfx.Polyline(dc, brush, DrawKit.Place(wv, head, ang).Where(p => Vector2.Distance(p, head) < R * 0.88f).ToList(), 5f * S + 1f);
        }
        brush.Color = Gfx.C4(0.6f, 0.45f, 0.25f, 0.45f);
        for (int i = 0; i < 15; i++)
        {
            float a = -MathF.PI * 0.95f + MathF.PI * 0.9f * i / 14f;
            var tip = DrawKit.Place(new[] { new Vector2(MathF.Cos(a) * R * 0.95f, MathF.Sin(a) * R * 0.88f + R * 0.0f) }, head, ang)[0];
            dc.DrawLine(head + dir * R * 0.0f - dir * R * 0.75f, tip, brush, 1.2f);
        }
        brush.Color = Gfx.C4(0.35f, 0.25f, 0.15f, 1f);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(face), 3f * S + 1f);
        // あおいだ風の線
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.6f);
        var side = new Vector2(-dir.Y, dir.X);
        for (int k = 0; k < 3; k++)
        {
            var s0 = head - side * R * (1.25f + k * 0.15f) + dir * (k - 1) * R * 0.4f;
            dc.DrawLine(s0, s0 - side * R * 0.35f, brush, 3f);
        }
    }
}
