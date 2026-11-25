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
//  地図のピンとルート
//   ピンが落ちて、点線のルートが次のピンへ伸びていく。下に地図の画像を置いて使える
// =====================================================================
public sealed class MapRoutePlugin : IShapePlugin
{
    public string Name => "演出：地図のピンとルート";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MapRouteParameter(sharedData);
}

public sealed class MapRouteParameter : EffectShapeParameterBase
{
    public MapRouteParameter() : this(null) { }
    public MapRouteParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "地点", Name = "地点の数")]
    [AnimationSlider("F0", "", 2, 5)]
    public Animation Count { get; } = new Animation(4, 2, 5);

    [Display(GroupName = "地点", Name = "地点1 X")] [AnimationSlider("F0", "%", 0, 100)] public Animation X1 { get; } = new Animation(15, -100, 200);
    [Display(GroupName = "地点", Name = "地点1 Y")] [AnimationSlider("F0", "%", 0, 100)] public Animation Y1 { get; } = new Animation(70, -100, 200);
    [Display(GroupName = "地点", Name = "地点2 X")] [AnimationSlider("F0", "%", 0, 100)] public Animation X2 { get; } = new Animation(38, -100, 200);
    [Display(GroupName = "地点", Name = "地点2 Y")] [AnimationSlider("F0", "%", 0, 100)] public Animation Y2 { get; } = new Animation(35, -100, 200);
    [Display(GroupName = "地点", Name = "地点3 X")] [AnimationSlider("F0", "%", 0, 100)] public Animation X3 { get; } = new Animation(62, -100, 200);
    [Display(GroupName = "地点", Name = "地点3 Y")] [AnimationSlider("F0", "%", 0, 100)] public Animation Y3 { get; } = new Animation(62, -100, 200);
    [Display(GroupName = "地点", Name = "地点4 X")] [AnimationSlider("F0", "%", 0, 100)] public Animation X4 { get; } = new Animation(85, -100, 200);
    [Display(GroupName = "地点", Name = "地点4 Y")] [AnimationSlider("F0", "%", 0, 100)] public Animation Y4 { get; } = new Animation(28, -100, 200);
    [Display(GroupName = "地点", Name = "地点5 X")] [AnimationSlider("F0", "%", 0, 100)] public Animation X5 { get; } = new Animation(88, -100, 200);
    [Display(GroupName = "地点", Name = "地点5 Y")] [AnimationSlider("F0", "%", 0, 100)] public Animation Y5 { get; } = new Animation(78, -100, 200);

    [Display(GroupName = "見た目", Name = "ピンの色")]
    [ColorPicker]
    public WpfColor PinColor { get => pinColor; set => Set(ref pinColor, value); }
    WpfColor pinColor = WpfColor.FromRgb(0xE8, 0x34, 0x3C);

    [Display(GroupName = "見た目", Name = "ルートの色")]
    [ColorPicker]
    public WpfColor RouteColor { get => routeColor; set => Set(ref routeColor, value); }
    WpfColor routeColor = WpfColor.FromRgb(0x24, 0x5C, 0xD8);

    [Display(GroupName = "見た目", Name = "ピンの大きさ")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation PinSize { get; } = new Animation(40, 2, 1000);

    [Display(GroupName = "見た目", Name = "ルートの太さ")]
    [AnimationSlider("F1", "px", 1, 20)]
    public Animation RouteWidth { get; } = new Animation(7, 0.5, 200);

    [Display(GroupName = "見た目", Name = "ルートを曲線にする")]
    [ToggleSlider]
    public bool Curved { get => curved; set => Set(ref curved, value); }
    bool curved = true;

    [Display(GroupName = "見た目", Name = "簡単な地図の背景を描く")]
    [ToggleSlider]
    public bool MapBg { get => mapBg; set => Set(ref mapBg, value); }
    bool mapBg = false;

    [Display(GroupName = "動き", Name = "1区間の時間")]
    [AnimationSlider("F1", "秒", 0.3, 5)]
    public Animation LegTime { get; } = new Animation(1.3, 0.05, 600);

    [Display(GroupName = "動き", Name = "ルート上を進む点")]
    [ToggleSlider]
    public bool Traveler { get => traveler; set => Set(ref traveler, value); }
    bool traveler = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Count, X1, Y1, X2, Y2, X3, Y3, X4, Y4, X5, Y5, PinSize, RouteWidth, LegTime];

    static List<Vector2> Leg(Vector2 a, Vector2 b, bool curved, int i)
    {
        var pts = new List<Vector2>();
        var mid = (a + b) / 2f;
        var d = b - a;
        var nrm = new Vector2(-d.Y, d.X) * (curved ? 0.22f * (i % 2 == 0 ? 1f : -1f) : 0f);
        var ctrl = mid + nrm;
        for (int k = 0; k <= 40; k++)
        {
            float u = k / 40f;
            pts.Add((1 - u) * (1 - u) * a + 2 * (1 - u) * u * ctrl + u * u * b);
        }
        return pts;
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int n = Math.Clamp((int)MathF.Round(c.F(Count)), 2, 5);
        var pos = new[]
        {
            new Vector2(c.F(X1) / 100f * w, c.F(Y1) / 100f * h), new Vector2(c.F(X2) / 100f * w, c.F(Y2) / 100f * h),
            new Vector2(c.F(X3) / 100f * w, c.F(Y3) / 100f * h), new Vector2(c.F(X4) / 100f * w, c.F(Y4) / 100f * h),
            new Vector2(c.F(X5) / 100f * w, c.F(Y5) / 100f * h),
        };
        float ps = c.F(PinSize), rw = c.F(RouteWidth), leg = MathF.Max(0.05f, c.F(LegTime));
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);

        if (MapBg)
        {
            brush.Color = Gfx.C4(0.78f, 0.88f, 0.95f, 1f);
            dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            for (int i = 0; i < 5; i++)
            {
                var lc = new Vector2(w * (0.1f + 0.8f * DrawKit.H(Seed, i, 1)), h * (0.15f + 0.7f * DrawKit.H(Seed, i, 2)));
                var land = new List<Vector2>();
                float lr = MathF.Min(w, h) * (0.15f + 0.15f * DrawKit.H(Seed, i, 3));
                for (int k = 0; k < 40; k++)
                {
                    float a = k * MathF.PI * 2f / 40f;
                    float rr = lr * (1f + 0.25f * MathF.Sin(a * 3f + i) + 0.12f * MathF.Sin(a * 7f + i * 2f));
                    land.Add(lc + new Vector2(MathF.Cos(a) * rr * 1.3f, MathF.Sin(a) * rr));
                }
                brush.Color = Gfx.C4(0.86f, 0.9f, 0.76f, 1f);
                Gfx.FillPoly(dc, brush, DrawKit.ClipRect(land, 0, 0, w, h));
            }
            brush.Color = Gfx.C4(1f, 1f, 1f, 0.35f);
            for (float x = 0; x < w; x += 120f) dc.DrawLine(new Vector2(x, 0), new Vector2(x, h), brush, 1f);
            for (float y = 0; y < h; y += 120f) dc.DrawLine(new Vector2(0, y), new Vector2(w, y), brush, 1f);
        }

        // ---- ルート ----
        for (int i = 0; i < n - 1; i++)
        {
            float lt = (t - 0.4f - i * leg) / (leg * 0.8f);
            if (lt <= 0f) continue;
            var pts = Leg(pos[i], pos[i + 1], Curved, i);
            var part = PathKit.Part(pts, Math.Clamp(lt, 0f, 1f));
            if (part.Count < 2) continue;
            brush.Color = Gfx.C4(white, 0.9f);
            FormKit.Dash(dc, brush, part, rw * 2.2f, rw * 1.6f, 0f, rw * 1.8f);
            brush.Color = Gfx.C4(RouteColor, 1f);
            FormKit.Dash(dc, brush, part, rw * 2.2f, rw * 1.6f, 0f, rw);
            if (Traveler && lt < 1f)
            {
                var head = part[^1];
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, head, rw * 4f, Gfx.C4(RouteColor, 0.6f));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                brush.Color = Gfx.C4(white, 1f);
                dc.FillEllipse(new Ellipse(head, rw * 1.2f, rw * 1.2f), brush);
                brush.Color = Gfx.C4(RouteColor, 1f);
                dc.FillEllipse(new Ellipse(head, rw * 0.7f, rw * 0.7f), brush);
            }
        }

        // ---- ピン（落ちてきて弾む）----
        for (int i = 0; i < n; i++)
        {
            float lt = t - i * leg;
            if (lt < 0f) continue;
            float drop = Math.Clamp(lt / 0.35f, 0f, 1f);
            float bounce = drop < 1f ? (1f - drop * drop) * -ps * 4f : -MathF.Abs(MathF.Sin((lt - 0.35f) * 14f)) * ps * 0.3f * MathF.Exp(-(lt - 0.35f) * 6f);
            var tip = pos[i];
            // 影と着地の輪
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f * drop);
            dc.FillEllipse(new Ellipse(tip, ps * 0.35f * (0.5f + 0.5f * drop), ps * 0.12f * (0.5f + 0.5f * drop)), brush);
            if (lt > 0.35f && lt < 1f)
            {
                float u = (lt - 0.35f) / 0.65f;
                brush.Color = Gfx.C4(PinColor, 0.7f * (1f - u));
                dc.DrawEllipse(new Ellipse(tip, ps * (0.4f + 1.2f * u), ps * (0.15f + 0.45f * u)), brush, 3f);
            }
            var top = tip + new Vector2(0, bounce);
            float R = ps * 0.5f;
            var head = top + new Vector2(0, -ps * 1.05f);
            var pin = DrawKit.EllipsePts(head, R, R, 36, MathF.PI * 0.8f, MathF.PI * 2.2f);
            pin.Add(top);
            brush.Color = Gfx.C4(Gfx.Mix(PinColor, WpfColor.FromRgb(0, 0, 0), 0.25f), 1f);
            Gfx.FillPoly(dc, brush, pin.Select(p => p + new Vector2(2f, 2f)).ToList());
            brush.Color = Gfx.C4(PinColor, 1f);
            Gfx.FillPoly(dc, brush, pin);
            brush.Color = Gfx.C4(white, 1f);
            dc.FillEllipse(new Ellipse(head, R * 0.42f, R * 0.42f), brush);
            brush.Color = Gfx.C4(white, 0.5f);
            dc.FillEllipse(new Ellipse(head + new Vector2(-R * 0.45f, -R * 0.45f), R * 0.15f, R * 0.12f), brush);
            // 番号
            brush.Color = Gfx.C4(PinColor, 1f);
            StrokeFont.DrawCentered(dc, brush, (i + 1).ToString(), head, R * 0.55f, MathF.Max(1.5f, R * 0.09f));
        }
    }
}
