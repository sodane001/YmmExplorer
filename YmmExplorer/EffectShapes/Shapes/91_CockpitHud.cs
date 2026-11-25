using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
//  コックピットHUD
//   高度計、方位、ミニマップ、照準の組み合わせ
// =====================================================================
public sealed class CockpitHudPlugin : IShapePlugin
{
    public string Name => "演出：コックピットHUD";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CockpitHudParameter(sharedData);
}

public sealed class CockpitHudParameter : EffectShapeParameterBase
{
    public CockpitHudParameter() : this(null) { }
    public CockpitHudParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x5C, 0xFF, 0x8A);

    [Display(GroupName = "色", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(85, 0, 100);

    [Display(GroupName = "色", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 200)]
    public Animation HudScale { get; } = new Animation(100, 5, 1000);

    [Display(GroupName = "色", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 6)]
    public Animation Thickness { get; } = new Animation(2, 0.2, 50);

    [Display(GroupName = "機体", Name = "機首の上下（ピッチ）")]
    [AnimationSlider("F0", "°", -45, 45)]
    public Animation Pitch { get; } = new Animation(0, -90, 90);

    [Display(GroupName = "機体", Name = "左右の傾き（ロール）")]
    [AnimationSlider("F0", "°", -90, 90)]
    public Animation Roll { get; } = new Animation(0, -180, 180);

    [Display(GroupName = "機体", Name = "方位")]
    [AnimationSlider("F0", "°", 0, 360)]
    public Animation Heading { get; } = new Animation(40, -100000, 100000);

    [Display(GroupName = "機体", Name = "高度")]
    [AnimationSlider("F0", "ft", 0, 40000)]
    public Animation Altitude { get; } = new Animation(12000, 0, 999999);

    [Display(GroupName = "機体", Name = "速度")]
    [AnimationSlider("F0", "kt", 0, 1500)]
    public Animation Speed { get; } = new Animation(480, 0, 99999);

    [Display(GroupName = "機体", Name = "自動でゆらす（飛んでいる感じ）")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "表示", Name = "ミニマップを表示する")]
    [ToggleSlider]
    public bool Minimap { get => minimap; set => Set(ref minimap, value); }
    bool minimap = true;

    [Display(GroupName = "表示", Name = "速度・高度の目盛りを表示する")]
    [ToggleSlider]
    public bool Tapes { get => tapes; set => Set(ref tapes, value); }
    bool tapes = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Opacity, HudScale, Thickness, Pitch, Roll, Heading, Altitude, Speed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(w / 2f, h / 2f);
        float S = MathF.Min(w, h) / 1080f * c.F(HudScale) / 100f;
        float op = c.F(Opacity) / 100f, th = c.F(Thickness);
        float pitch = c.F(Pitch), roll = c.F(Roll), hdg = c.F(Heading), alt = c.F(Altitude), spd = c.F(Speed);
        if (Auto)
        {
            pitch += MathF.Sin(t * 0.5f) * 4f + MathF.Sin(t * 1.3f) * 1.2f;
            roll += MathF.Sin(t * 0.37f) * 9f;
            hdg += MathF.Sin(t * 0.21f) * 12f + t * 3f;
            alt += MathF.Sin(t * 0.3f) * 150f + t * 20f;
            spd += MathF.Sin(t * 0.45f) * 14f;
        }
        hdg = Gfx.Mod(hdg, 360f);
        var col = Gfx.C4(Color, op);
        var dim = Gfx.C4(Color, op * 0.45f);
        float txt = 22f * S;

        using var brush = dc.CreateSolidColorBrush(default);
        var prev = dc.Transform;
        try
        {
            // ---- 姿勢（ピッチ梯子）：ロールで回転 ----
            float pxDeg = 13f * S;
            dc.Transform = Matrix3x2.CreateRotation(Gfx.Rad(-roll), ctr) * prev;
            for (int p = -90; p <= 90; p += 5)
            {
                float y = ctr.Y + (pitch - p) * pxDeg;
                if (MathF.Abs(y - ctr.Y) > 300f * S) continue;
                float fade = 1f - Gfx.Smooth(200f * S, 300f * S, MathF.Abs(y - ctr.Y));
                brush.Color = Gfx.C4(Color, op * fade);
                if (p == 0)
                {
                    dc.DrawLine(new Vector2(ctr.X - 420f * S, y), new Vector2(ctr.X - 90f * S, y), brush, th);
                    dc.DrawLine(new Vector2(ctr.X + 90f * S, y), new Vector2(ctr.X + 420f * S, y), brush, th);
                    continue;
                }
                bool major = p % 10 == 0;
                float x0 = 90f * S, x1 = (major ? 220f : 150f) * S;
                if (p > 0)
                {
                    dc.DrawLine(new Vector2(ctr.X - x1, y), new Vector2(ctr.X - x0, y), brush, th);
                    dc.DrawLine(new Vector2(ctr.X + x0, y), new Vector2(ctr.X + x1, y), brush, th);
                }
                else
                {
                    FormKit.Dash(dc, brush, new List<Vector2> { new(ctr.X - x1, y), new(ctr.X - x0, y) }, 10f * S, 7f * S, 0f, th);
                    FormKit.Dash(dc, brush, new List<Vector2> { new(ctr.X + x0, y), new(ctr.X + x1, y) }, 10f * S, 7f * S, 0f, th);
                }
                float tick = (p > 0 ? 12f : -12f) * S;
                dc.DrawLine(new Vector2(ctr.X - x1, y), new Vector2(ctr.X - x1, y + tick), brush, th);
                dc.DrawLine(new Vector2(ctr.X + x1, y), new Vector2(ctr.X + x1, y + tick), brush, th);
                if (major)
                {
                    string s = Math.Abs(p).ToString(CultureInfo.InvariantCulture);
                    StrokeFont.DrawCentered(dc, brush, s, new Vector2(ctr.X - x1 - 30f * S, y), txt * 0.8f, th * 0.8f);
                    StrokeFont.DrawCentered(dc, brush, s, new Vector2(ctr.X + x1 + 30f * S, y), txt * 0.8f, th * 0.8f);
                }
            }
            dc.Transform = prev;

            // ---- 照準（中央）----
            brush.Color = col;
            dc.DrawEllipse(new Ellipse(ctr, 6f * S, 6f * S), brush, th);
            Gfx.Polyline(dc, brush, new List<Vector2> { ctr + new Vector2(-70f, 0) * S, ctr + new Vector2(-30f, 0) * S, ctr + new Vector2(-15f, 15f) * S, ctr + new Vector2(0, 0), ctr + new Vector2(15f, 15f) * S, ctr + new Vector2(30f, 0) * S, ctr + new Vector2(70f, 0) * S }, th);
            // 角のブラケット
            float bx = 140f * S, by = 100f * S, arm = 26f * S;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    var cr = ctr + new Vector2(sx * bx, sy * by);
                    dc.DrawLine(cr, cr - new Vector2(sx * arm, 0), brush, th);
                    dc.DrawLine(cr, cr - new Vector2(0, sy * arm), brush, th);
                }

            // ---- 方位テープ（上）----
            {
                float ty = ctr.Y - 390f * S, half = 330f * S, pxH = 6.5f * S;
                brush.Color = dim;
                dc.DrawLine(new Vector2(ctr.X - half, ty + 22f * S), new Vector2(ctr.X + half, ty + 22f * S), brush, th * 0.8f);
                int d0 = (int)MathF.Floor((hdg - half / pxH) / 5f) * 5;
                for (int d = d0; d <= hdg + half / pxH; d += 5)
                {
                    float x = ctr.X + (d - hdg) * pxH;
                    float fade = 1f - Gfx.Smooth(half * 0.75f, half, MathF.Abs(x - ctr.X));
                    brush.Color = Gfx.C4(Color, op * fade);
                    int dd = (int)Gfx.Mod(d, 360f);
                    bool major = dd % 10 == 0;
                    dc.DrawLine(new Vector2(x, ty + 22f * S), new Vector2(x, ty + (major ? 6f : 13f) * S), brush, th);
                    if (dd % 30 == 0)
                    {
                        string s = dd switch { 0 => "N", 90 => "E", 180 => "S", 270 => "W", _ => (dd / 10).ToString("00", CultureInfo.InvariantCulture) };
                        StrokeFont.DrawCentered(dc, brush, s, new Vector2(x, ty - 16f * S), txt * 0.85f, th * 0.9f);
                    }
                }
                brush.Color = col;
                Gfx.Polyline(dc, brush, new List<Vector2> { new(ctr.X - 9f * S, ty + 42f * S), new(ctr.X, ty + 28f * S), new(ctr.X + 9f * S, ty + 42f * S) }, th);
                string hs = ((int)MathF.Round(hdg) % 360).ToString("000", CultureInfo.InvariantCulture);
                dc.DrawRectangle(new RawRectF(ctr.X - 42f * S, ty + 48f * S, ctr.X + 42f * S, ty + 82f * S), brush, th);
                StrokeFont.DrawCentered(dc, brush, hs, new Vector2(ctr.X, ty + 65f * S), txt, th);
            }

            // ---- 速度（左）・高度（右）のテープ ----
            if (Tapes)
            {
                DrawTape(dc, brush, new Vector2(ctr.X - 520f * S, ctr.Y), S, th, op, spd, 10f, 5f, "SPD", left: true, txt);
                DrawTape(dc, brush, new Vector2(ctr.X + 520f * S, ctr.Y), S, th, op, alt, 100f, 0.45f, "ALT", left: false, txt);
            }

            // ---- 下の情報 ----
            brush.Color = dim;
            StrokeFont.Draw(dc, brush, "G 1.0", new Vector2(ctr.X - 560f * S, ctr.Y + 330f * S), txt * 0.8f, th * 0.8f);
            float blink = MathF.Sin(t * 6f) > 0f ? 1f : 0.3f;
            brush.Color = Gfx.C4(Color, op * blink);
            StrokeFont.Draw(dc, brush, "ARM", new Vector2(ctr.X - 560f * S, ctr.Y + 365f * S), txt * 0.8f, th * 0.8f);

            // ---- ミニマップ（右下）----
            if (Minimap)
            {
                float mr = 130f * S;
                var mc = new Vector2(MathF.Min(w - mr - 30f * S, ctr.X + 760f * S), MathF.Min(h - mr - 30f * S, ctr.Y + 370f * S));
                brush.Color = Gfx.C4(0f, 0.05f, 0.02f, 0.45f * op);
                dc.FillEllipse(new Ellipse(mc, mr, mr), brush);
                brush.Color = col;
                dc.DrawEllipse(new Ellipse(mc, mr, mr), brush, th);
                brush.Color = dim;
                dc.DrawEllipse(new Ellipse(mc, mr * 0.66f, mr * 0.66f), brush, th * 0.6f);
                dc.DrawEllipse(new Ellipse(mc, mr * 0.33f, mr * 0.33f), brush, th * 0.6f);
                dc.DrawLine(mc - new Vector2(mr, 0), mc + new Vector2(mr, 0), brush, th * 0.5f);
                dc.DrawLine(mc - new Vector2(0, mr), mc + new Vector2(0, mr), brush, th * 0.5f);
                // 回るレーダー
                float sw = t * 2.2f;
                for (int k = 0; k < 20; k++)
                {
                    float a = sw - k * 0.04f;
                    brush.Color = Gfx.C4(Color, op * 0.35f * (1f - k / 20f));
                    dc.DrawLine(mc, mc + new Vector2(MathF.Cos(a), MathF.Sin(a)) * mr, brush, th * 1.5f);
                }
                // 光点（方位で回る）
                float rotM = Gfx.Rad(-hdg);
                for (int i = 0; i < 6; i++)
                {
                    float a = DrawKit.H(Seed, i, 1) * MathF.PI * 2f + rotM;
                    float r = mr * (0.25f + 0.7f * DrawKit.H(Seed, i, 2));
                    var p = mc + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r;
                    float since = Gfx.Mod(sw - a, MathF.PI * 2f) / (MathF.PI * 2f);
                    brush.Color = Gfx.C4(i == 0 ? WpfColor.FromRgb(0xFF, 0x4A, 0x3A) : Color, op * (1f - since * 0.8f));
                    dc.FillEllipse(new Ellipse(p, 4f * S, 4f * S), brush);
                }
                // 自機と北
                brush.Color = col;
                Gfx.FillPoly(dc, brush, new List<Vector2> { mc + new Vector2(0, -12f) * S, mc + new Vector2(8f, 9f) * S, mc + new Vector2(0, 4f) * S, mc + new Vector2(-8f, 9f) * S });
                var np = mc + new Vector2(MathF.Sin(rotM), -MathF.Cos(rotM)) * (mr + 18f * S);
                StrokeFont.DrawCentered(dc, brush, "N", np, txt * 0.8f, th);
            }
        }
        finally
        {
            dc.Transform = prev;
        }
    }

    void DrawTape(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 pos, float S, float th, float op,
        float value, float step, float pxPerUnit, string label, bool left, float txt)
    {
        float half = 230f * S;
        float px = 260f * S / (step * 6f);   // 6目盛りでテープの半分
        float dirX = left ? -1f : 1f;
        brush.Color = Gfx.C4(Color, op * 0.45f);
        dc.DrawLine(pos - new Vector2(0, half), pos + new Vector2(0, half), brush, th);
        float v0 = MathF.Floor((value - half / px) / step) * step;
        for (float v = v0; v <= value + half / px; v += step)
        {
            float y = pos.Y - (v - value) * px;
            float fade = 1f - Gfx.Smooth(half * 0.7f, half, MathF.Abs(y - pos.Y));
            brush.Color = Gfx.C4(Color, op * fade);
            bool major = Math.Abs(Gfx.Mod(v, step * 5f)) < 0.01f || Math.Abs(Gfx.Mod(v, step * 5f) - step * 5f) < 0.01f;
            dc.DrawLine(new Vector2(pos.X, y), new Vector2(pos.X + dirX * (major ? 22f : 12f) * S, y), brush, th);
            if (major && v >= 0f)
            {
                string s = ((int)MathF.Round(v)).ToString(CultureInfo.InvariantCulture);
                var tp = new Vector2(pos.X + dirX * (34f * S + StrokeFont.Width(s, txt * 0.7f) / 2f), y);
                StrokeFont.DrawCentered(dc, brush, s, tp, txt * 0.7f, th * 0.8f);
            }
        }
        // 現在値の箱
        brush.Color = Gfx.C4(Color, op);
        string cur = ((int)MathF.Round(MathF.Max(0f, value))).ToString(CultureInfo.InvariantCulture);
        float bw = StrokeFont.Width(cur, txt) + 26f * S;
        float bx = left ? pos.X - 30f * S - bw : pos.X + 30f * S;
        dc.DrawRectangle(new RawRectF(bx, pos.Y - 20f * S, bx + bw, pos.Y + 20f * S), brush, th);
        brush.Color = Gfx.C4(0f, 0.05f, 0.02f, 0.5f * op);
        dc.FillRectangle(new RawRectF(bx, pos.Y - 20f * S, bx + bw, pos.Y + 20f * S), brush);
        brush.Color = Gfx.C4(Color, op);
        StrokeFont.DrawCentered(dc, brush, cur, new Vector2(bx + bw / 2f, pos.Y), txt, th);
        StrokeFont.DrawCentered(dc, brush, label, new Vector2(pos.X, pos.Y - half - 26f * S), txt * 0.85f, th * 0.9f);
    }
}
