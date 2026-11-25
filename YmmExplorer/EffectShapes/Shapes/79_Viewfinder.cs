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
//  カメラ・ファインダー
//   四隅の枠・ピント枠（ピント合わせの動き）・REC・バッテリー・グリッド・水平器
// =====================================================================
public sealed class ViewfinderPlugin : IShapePlugin
{
    public string Name => "演出：カメラ・ファインダー";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ViewfinderParameter(sharedData);
}

public sealed class ViewfinderParameter : EffectShapeParameterBase
{
    public ViewfinderParameter() : this(null) { }
    public ViewfinderParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "表示の色")]
    [ColorPicker]
    public WpfColor LineColor { get => line; set => Set(ref line, value); }
    WpfColor line = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "色", Name = "RECの色")]
    [ColorPicker]
    public WpfColor RecColor { get => rec; set => Set(ref rec, value); }
    WpfColor rec = WpfColor.FromRgb(0xFF, 0x2D, 0x2D);

    [Display(GroupName = "色", Name = "ピントが合ったときの色")]
    [ColorPicker]
    public WpfColor FocusColor { get => focus; set => Set(ref focus, value); }
    WpfColor focus = WpfColor.FromRgb(0x4D, 0xFF, 0x7A);

    [Display(GroupName = "枠", Name = "画面の余白")]
    [AnimationSlider("F0", "%", 0, 20)]
    public Animation Margin { get; } = new Animation(5, 0, 40);

    [Display(GroupName = "枠", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 1, 12)]
    public Animation Thickness { get; } = new Animation(3, 0.5, 60);

    [Display(GroupName = "表示", Name = "三分割のグリッド")]
    [ToggleSlider]
    public bool ShowGrid { get => grid; set => Set(ref grid, value); }
    bool grid = true;

    [Display(GroupName = "表示", Name = "ピント枠")]
    [ToggleSlider]
    public bool ShowFocus { get => showFocus; set => Set(ref showFocus, value); }
    bool showFocus = true;

    [Display(GroupName = "表示", Name = "REC表示と時間")]
    [ToggleSlider]
    public bool ShowRec { get => showRec; set => Set(ref showRec, value); }
    bool showRec = true;

    [Display(GroupName = "表示", Name = "バッテリー")]
    [ToggleSlider]
    public bool ShowBattery { get => battery; set => Set(ref battery, value); }
    bool battery = true;

    [Display(GroupName = "表示", Name = "水平器")]
    [ToggleSlider]
    public bool ShowLevel { get => level; set => Set(ref level, value); }
    bool level = true;

    [Display(GroupName = "表示", Name = "ズームバー")]
    [ToggleSlider]
    public bool ShowZoom { get => zoom; set => Set(ref zoom, value); }
    bool zoom = true;

    [Display(GroupName = "ピント枠", Name = "ピント枠のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FocusX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ピント枠", Name = "ピント枠のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation FocusY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ピント枠", Name = "ピント枠の大きさ")]
    [AnimationSlider("F0", "px", 40, 600)]
    public Animation FocusSize { get; } = new Animation(150, 10, 4000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Margin, Thickness, FocusX, FocusY, FocusSize];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float m = c.F(Margin) / 100f * MathF.Min(w, h), th = c.F(Thickness);
        float L = m, R = w - m, T = m, B = h - m;
        float arm = MathF.Min(w, h) * 0.06f;

        using var brush = dc.CreateSolidColorBrush(default);
        brush.Color = Gfx.C4(LineColor, 0.95f);

        // ---- 四隅の枠 ----
        foreach (var (sx, sy) in new[] { (1, 1), (-1, 1), (1, -1), (-1, -1) })
        {
            var cr = new Vector2(sx > 0 ? L : R, sy > 0 ? T : B);
            dc.DrawLine(cr, cr + new Vector2(sx * arm, 0f), brush, th);
            dc.DrawLine(cr, cr + new Vector2(0f, sy * arm), brush, th);
        }

        // ---- 中心の十字 ----
        var mid = new Vector2(w / 2f, h / 2f);
        dc.DrawLine(mid - new Vector2(arm * 0.4f, 0f), mid + new Vector2(arm * 0.4f, 0f), brush, th * 0.6f);
        dc.DrawLine(mid - new Vector2(0f, arm * 0.4f), mid + new Vector2(0f, arm * 0.4f), brush, th * 0.6f);

        // ---- 三分割グリッド ----
        if (ShowGrid)
        {
            brush.Color = Gfx.C4(LineColor, 0.25f);
            for (int i = 1; i <= 2; i++)
            {
                dc.DrawLine(new Vector2(L + (R - L) * i / 3f, T), new Vector2(L + (R - L) * i / 3f, B), brush, 1f);
                dc.DrawLine(new Vector2(L, T + (B - T) * i / 3f), new Vector2(R, T + (B - T) * i / 3f), brush, 1f);
            }
        }

        // ---- ピント枠：迷いながら合焦する ----
        if (ShowFocus)
        {
            var fc = new Vector2(c.F(FocusX) / 100f * w, c.F(FocusY) / 100f * h);
            float fs = c.F(FocusSize);
            float P = 4f, p = Gfx.Mod(t, P) / P;
            float hunt = 1f - Gfx.Smooth(0.15f, 0.55f, p);                       // 0=合焦
            var pos = fc + new Vector2(MathF.Sin(t * 7f) * 40f, MathF.Cos(t * 5f) * 30f) * hunt;
            float size = fs * (1f + 0.5f * hunt);
            bool locked = hunt < 0.02f;
            var col = locked ? FocusColor : LineColor;
            brush.Color = Gfx.C4(col, 0.95f);
            float fa = size * 0.28f;
            foreach (var (sx, sy) in new[] { (1, 1), (-1, 1), (1, -1), (-1, -1) })
            {
                var cr = pos + new Vector2(sx * size, sy * size);
                dc.DrawLine(cr, cr - new Vector2(sx * fa, 0f), brush, th);
                dc.DrawLine(cr, cr - new Vector2(0f, sy * fa), brush, th);
            }
            if (locked)
            {
                float u = Gfx.Mod((p - 0.55f) * 3f, 1f);
                brush.Color = Gfx.C4(FocusColor, 0.6f * (1f - u));
                dc.DrawEllipse(new Ellipse(pos, size * (0.2f + 0.5f * u), size * (0.2f + 0.5f * u)), brush, th * 0.8f);
            }
        }

        // ---- REC表示と時間 ----
        if (ShowRec)
        {
            float rh = MathF.Min(w, h) * 0.035f;
            var p0 = new Vector2(L + arm * 0.5f, T + arm * 0.9f);
            brush.Color = Gfx.C4(RecColor, MathF.Floor(t * 1.6f) % 2f == 0f ? 1f : 0.2f);
            dc.FillEllipse(new Ellipse(p0 + new Vector2(rh * 0.5f, rh * 0.5f), rh * 0.45f, rh * 0.45f), brush);
            brush.Color = Gfx.C4(LineColor, 0.95f);
            PathKit.Text(dc, brush, "REC", p0 + new Vector2(rh * 1.5f, 0f), rh, th * 0.7f);
            int sec = (int)MathF.Floor(t);
            string s = $"{Math.Min(99, sec / 60):00}{sec % 60:00}";
            float dh = rh, dw = dh * 0.55f, x = p0.X + rh * 1.5f + PathKit.TextWidth("REC", rh) + rh * 0.9f;
            for (int i = 0; i < 4; i++)
            {
                ShapeGeom.Digit(dc, brush, x, p0.Y, dw, dh, s[i] - '0', th * 0.7f);
                x += dw + dh * 0.3f;
                if (i == 1)
                {
                    dc.FillEllipse(new Ellipse(new Vector2(x - dh * 0.1f, p0.Y + dh * 0.3f), th * 0.5f, th * 0.5f), brush);
                    dc.FillEllipse(new Ellipse(new Vector2(x - dh * 0.1f, p0.Y + dh * 0.7f), th * 0.5f, th * 0.5f), brush);
                    x += dh * 0.25f;
                }
            }
        }

        // ---- バッテリー（右上）----
        if (ShowBattery)
        {
            float bw = arm * 1.5f, bh = arm * 0.7f;
            var p0 = new Vector2(R - arm * 0.5f - bw, T + arm * 0.9f);
            float lvl = 0.35f + 0.65f * (0.5f + 0.5f * MathF.Sin(t * 0.2f));
            brush.Color = Gfx.C4(LineColor, 0.95f);
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(new List<Vector2> { p0, p0 + new Vector2(bw, 0), p0 + new Vector2(bw, bh), p0 + new Vector2(0, bh) }), th * 0.7f);
            Gfx.Rect(dc, brush, p0.X + bw, p0.Y + bh * 0.3f, bw * 0.06f, bh * 0.4f);
            brush.Color = Gfx.C4(lvl < 0.5f ? RecColor : FocusColor, 0.95f);
            Gfx.Rect(dc, brush, p0.X + bw * 0.06f, p0.Y + bh * 0.15f, (bw * 0.88f) * lvl, bh * 0.7f);
        }

        // ---- 水平器（下中央）----
        if (ShowLevel)
        {
            float tilt = MathF.Sin(t * 0.8f) * 0.08f * MathF.Exp(-0.15f * MathF.Max(0f, t - 1f));
            var lc = new Vector2(w / 2f, B - arm * 0.6f);
            float len = arm * 1.8f;
            var d = new Vector2(MathF.Cos(tilt), MathF.Sin(tilt));
            brush.Color = Gfx.C4(MathF.Abs(tilt) < 0.01f ? FocusColor : LineColor, 0.95f);
            dc.DrawLine(lc - d * len, lc - d * len * 0.35f, brush, th);
            dc.DrawLine(lc + d * len * 0.35f, lc + d * len, brush, th);
            dc.DrawLine(lc - d * len * 0.15f, lc + d * len * 0.15f, brush, th);
        }

        // ---- ズームバー（右下）----
        if (ShowZoom)
        {
            float zw = arm * 3f;
            var z0 = new Vector2(R - arm * 0.5f - zw, B - arm * 0.6f);
            float zv = 0.5f + 0.5f * MathF.Sin(t * 0.4f);
            brush.Color = Gfx.C4(LineColor, 0.5f);
            dc.DrawLine(z0, z0 + new Vector2(zw, 0f), brush, th * 0.7f);
            for (int i = 0; i <= 6; i++)
                dc.DrawLine(z0 + new Vector2(zw * i / 6f, -arm * 0.1f), z0 + new Vector2(zw * i / 6f, arm * 0.1f), brush, 1f);
            brush.Color = Gfx.C4(LineColor, 0.95f);
            dc.FillEllipse(new Ellipse(z0 + new Vector2(zw * zv, 0f), th * 1.6f, th * 1.6f), brush);
        }
    }
}
