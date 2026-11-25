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

public enum TimeKind
{
    [Display(Name = "アナログ時計")] Clock,
    [Display(Name = "砂時計")] Hourglass,
}

// =====================================================================
//  アナログ時計・砂時計
//   時間の経過を見せる。時計は速さを上げると「時間が飛ぶ」表現にも
// =====================================================================
public sealed class ClockHourglassPlugin : IShapePlugin
{
    public string Name => "演出：アナログ時計・砂時計";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ClockHourglassParameter(sharedData);
}

public sealed class ClockHourglassParameter : EffectShapeParameterBase
{
    public ClockHourglassParameter() : this(null) { }
    public ClockHourglassParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public TimeKind Kind { get => kind; set => Set(ref kind, value); }
    TimeKind kind = TimeKind.Clock;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ（半径）")]
    [AnimationSlider("F0", "px", 40, 500)]
    public Animation Radius { get; } = new Animation(220, 4, 4000);

    [Display(GroupName = "時計", Name = "開始の時刻（時）")]
    [AnimationSlider("F0", "時", 0, 23)]
    public Animation StartHour { get; } = new Animation(10, 0, 23);

    [Display(GroupName = "時計", Name = "開始の時刻（分）")]
    [AnimationSlider("F0", "分", 0, 59)]
    public Animation StartMinute { get; } = new Animation(8, 0, 59);

    [Display(GroupName = "時計", Name = "時間の進む速さ（倍）")]
    [AnimationSlider("F0", "倍", 1, 3600)]
    public Animation TimeScale { get; } = new Animation(1, 0, 100000);

    [Display(GroupName = "時計", Name = "数字を表示する")]
    [ToggleSlider]
    public bool Numbers { get => numbers; set => Set(ref numbers, value); }
    bool numbers = true;

    [Display(GroupName = "時計", Name = "秒針を表示する")]
    [ToggleSlider]
    public bool SecondHand { get => secondHand; set => Set(ref secondHand, value); }
    bool secondHand = true;

    [Display(GroupName = "砂時計", Name = "砂が落ちきる時間")]
    [AnimationSlider("F1", "秒", 1, 60)]
    public Animation SandTime { get; } = new Animation(8, 0.2, 3600);

    [Display(GroupName = "砂時計", Name = "落ちきったらひっくり返す")]
    [ToggleSlider]
    public bool Flip { get => flip; set => Set(ref flip, value); }
    bool flip = true;

    [Display(GroupName = "色", Name = "本体の色")]
    [ColorPicker]
    public WpfColor BodyColor { get => bodyColor; set => Set(ref bodyColor, value); }
    WpfColor bodyColor = WpfColor.FromRgb(0x8A, 0x5A, 0x32);

    [Display(GroupName = "色", Name = "文字盤・砂の色")]
    [ColorPicker]
    public WpfColor FaceColor { get => faceColor; set => Set(ref faceColor, value); }
    WpfColor faceColor = WpfColor.FromRgb(0xF6, 0xF0, 0xE2);

    [Display(GroupName = "色", Name = "針・目盛りの色")]
    [ColorPicker]
    public WpfColor HandColor { get => handColor; set => Set(ref handColor, value); }
    WpfColor handColor = WpfColor.FromRgb(0x22, 0x24, 0x2A);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, StartHour, StartMinute, TimeScale, SandTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius);
        using var brush = dc.CreateSolidColorBrush(default);
        var prev = dc.Transform;
        try
        {
            if (Kind == TimeKind.Clock)
            {
                double secs = (c.F(StartHour) * 3600.0 + c.F(StartMinute) * 60.0) + t * (double)c.F(TimeScale);
                DrawClock(dc, brush, ctr, R, secs);
            }
            else
            {
                float dur = MathF.Max(0.2f, c.F(SandTime));
                const float flipT = 0.6f;
                float cyc = Flip ? dur + flipT : dur;
                float lt = Flip ? Gfx.Mod(t, cyc) : MathF.Min(t, dur);
                float rot = 0f, sand;
                if (lt <= dur) sand = lt / dur;
                else { sand = 1f; rot = MathF.PI * DrawKit.EaseOutBack((lt - dur) / flipT); }
                dc.Transform = Matrix3x2.CreateRotation(rot, ctr) * prev;
                DrawHourglass(dc, brush, ctr, R, sand, t, lt <= dur);
            }
        }
        finally
        {
            dc.Transform = prev;
        }
    }

    void DrawClock(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float R, double secs)
    {
        var white = WpfColor.FromRgb(255, 255, 255);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f);
        dc.FillEllipse(new Ellipse(ctr + new Vector2(R * 0.04f, R * 0.06f), R * 1.06f, R * 1.06f), brush);
        using (var g = Gfx.Radial(dc, ctr - new Vector2(R * 0.3f, R * 0.3f), R * 1.6f, (0f, Gfx.C4(Gfx.Mix(BodyColor, white, 0.35f), 1f)), (1f, Gfx.C4(Gfx.Mix(BodyColor, WpfColor.FromRgb(0, 0, 0), 0.4f), 1f))))
            dc.FillEllipse(new Ellipse(ctr, R * 1.06f, R * 1.06f), g);
        using (var g = Gfx.Radial(dc, ctr, R * 0.95f, (0f, Gfx.C4(FaceColor, 1f)), (0.85f, Gfx.C4(FaceColor, 1f)), (1f, Gfx.C4(Gfx.Mix(FaceColor, WpfColor.FromRgb(0, 0, 0), 0.18f), 1f))))
            dc.FillEllipse(new Ellipse(ctr, R * 0.93f, R * 0.93f), g);
        // 目盛り
        for (int i = 0; i < 60; i++)
        {
            float a = i * MathF.PI / 30f - MathF.PI / 2f;
            var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
            bool hour = i % 5 == 0;
            brush.Color = Gfx.C4(HandColor, hour ? 1f : 0.6f);
            dc.DrawLine(ctr + d * R * (hour ? 0.78f : 0.84f), ctr + d * R * 0.88f, brush, hour ? R * 0.025f : R * 0.01f);
        }
        if (Numbers)
        {
            brush.Color = Gfx.C4(HandColor, 1f);
            for (int i = 1; i <= 12; i++)
            {
                float a = i * MathF.PI / 6f - MathF.PI / 2f;
                var p = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * R * 0.64f;
                StrokeFont.DrawCentered(dc, brush, i.ToString(CultureInfo.InvariantCulture), p, R * 0.15f, MathF.Max(1.5f, R * 0.018f), 0.1f);
            }
        }
        double s = secs % 60.0, m = (secs / 60.0) % 60.0, hr = (secs / 3600.0) % 12.0;
        void Hand(double frac, float len, float width, WpfColor col, float back)
        {
            float a = (float)(frac * Math.PI * 2.0) - MathF.PI / 2f;
            var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.2f);
            dc.DrawLine(ctr - d * R * back + new Vector2(R * 0.015f, R * 0.02f), ctr + d * R * len + new Vector2(R * 0.015f, R * 0.02f), brush, width);
            brush.Color = Gfx.C4(col, 1f);
            dc.DrawLine(ctr - d * R * back, ctr + d * R * len, brush, width);
        }
        Hand(hr / 12.0, 0.45f, R * 0.055f, HandColor, 0.08f);
        Hand(m / 60.0, 0.7f, R * 0.035f, HandColor, 0.1f);
        if (SecondHand) Hand(Math.Floor(s) / 60.0, 0.8f, R * 0.012f, WpfColor.FromRgb(0xD8, 0x2A, 0x2A), 0.18f);
        brush.Color = Gfx.C4(HandColor, 1f);
        dc.FillEllipse(new Ellipse(ctr, R * 0.05f, R * 0.05f), brush);
        // ガラスの反射
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.12f);
        var glare = DrawKit.EllipsePts(ctr + new Vector2(-R * 0.25f, -R * 0.35f), R * 0.5f, R * 0.25f, 30);
        Gfx.FillPoly(dc, brush, DrawKit.Place(glare.Select(p => p - (ctr + new Vector2(-R * 0.25f, -R * 0.35f))), ctr + new Vector2(-R * 0.25f, -R * 0.35f), -0.6f));
    }

    void DrawHourglass(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float R, float sand, float t, bool flowing)
    {
        float H = R * 1.1f, W = R * 0.62f, neck = R * 0.05f;
        var wood = BodyColor;
        var sandC = Gfx.Mix(FaceColor, WpfColor.FromRgb(0xE0, 0xB0, 0x60), 0.6f);
        // ガラスの形（上下のふくらみ）
        List<Vector2> Bulb(bool top)
        {
            var pts = new List<Vector2>();
            float s = top ? -1f : 1f;
            for (int i = 0; i <= 20; i++)
            {
                float u = i / 20f;                      // 0=首 1=端
                float x = neck + (W - neck) * MathF.Sin(u * MathF.PI / 2f);
                pts.Add(ctr + new Vector2(x, s * H * u));
            }
            for (int i = 20; i >= 0; i--)
            {
                float u = i / 20f;
                float x = neck + (W - neck) * MathF.Sin(u * MathF.PI / 2f);
                pts.Add(ctr + new Vector2(-x, s * H * u));
            }
            return pts;
        }
        float HalfWidthAt(float u) => neck + (W - neck) * MathF.Sin(u * MathF.PI / 2f);

        // 上の砂（減る）：首側から上へ、残りの高さ
        float topLevel = (1f - sand) * 0.85f;           // 首から何割の高さまで砂があるか
        if (topLevel > 0.01f)
        {
            var pts = new List<Vector2>();
            for (int i = 0; i <= 16; i++) { float u = topLevel * i / 16f; pts.Add(ctr + new Vector2(HalfWidthAt(u) * 0.94f, -H * u)); }
            for (int i = 16; i >= 0; i--) { float u = topLevel * i / 16f; pts.Add(ctr + new Vector2(-HalfWidthAt(u) * 0.94f, -H * u)); }
            brush.Color = Gfx.C4(sandC, 1f);
            Gfx.FillPoly(dc, brush, pts);
        }
        // 下の砂（たまる山）
        float pile = sand * 0.85f;
        if (pile > 0.01f)
        {
            var pts = new List<Vector2> { ctr + new Vector2(-HalfWidthAt(1f) * 0.94f, H * 0.98f) };
            float baseTop = H * (1f - pile * 0.7f);
            for (int i = 0; i <= 16; i++)
            {
                float u = i / 16f;
                float x = (-1f + 2f * u) * HalfWidthAt(1f - pile * 0.5f) * 0.94f;
                float peak = H * pile * 0.35f * (1f - MathF.Abs(-1f + 2f * u));
                pts.Add(ctr + new Vector2(x, baseTop - peak));
            }
            pts.Add(ctr + new Vector2(HalfWidthAt(1f) * 0.94f, H * 0.98f));
            brush.Color = Gfx.C4(sandC, 1f);
            Gfx.FillPoly(dc, brush, DrawKit.ClipRect(pts, ctr.X - W, ctr.Y, ctr.X + W, ctr.Y + H));
        }
        // 落ちる砂の筋
        if (flowing && sand < 0.99f)
        {
            brush.Color = Gfx.C4(sandC, 1f);
            dc.DrawLine(ctr, ctr + new Vector2(0, H * (1f - pile * 0.7f - pile * 0.3f)), brush, MathF.Max(1.5f, neck * 0.6f));
            var rng = new Random((int)(t * 30f));
            for (int i = 0; i < 6; i++)
                dc.FillEllipse(new Ellipse(ctr + new Vector2((rng.NextF() - 0.5f) * neck, H * rng.NextF() * (1f - pile)), 1.2f, 1.2f), brush);
        }
        // ガラス
        brush.Color = Gfx.C4(0.75f, 0.88f, 1f, 0.18f);
        Gfx.FillPoly(dc, brush, Bulb(true));
        Gfx.FillPoly(dc, brush, Bulb(false));
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.6f);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(Bulb(true)), MathF.Max(1.5f, R * 0.012f));
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(Bulb(false)), MathF.Max(1.5f, R * 0.012f));
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.35f);
        dc.DrawLine(ctr + new Vector2(-W * 0.6f, -H * 0.75f), ctr + new Vector2(-W * 0.35f, -H * 0.3f), brush, R * 0.03f);
        // 木の枠
        var wd = Gfx.Mix(wood, WpfColor.FromRgb(0, 0, 0), 0.35f);
        brush.Color = Gfx.C4(wood, 1f);
        dc.FillRectangle(new RawRectF(ctr.X - W * 1.25f, ctr.Y - H - R * 0.12f, ctr.X + W * 1.25f, ctr.Y - H), brush);
        dc.FillRectangle(new RawRectF(ctr.X - W * 1.25f, ctr.Y + H, ctr.X + W * 1.25f, ctr.Y + H + R * 0.12f), brush);
        brush.Color = Gfx.C4(wd, 1f);
        foreach (float sx in new[] { -1f, 1f })
            dc.DrawLine(new Vector2(ctr.X + sx * W * 1.1f, ctr.Y - H), new Vector2(ctr.X + sx * W * 1.1f, ctr.Y + H), brush, R * 0.06f);
    }
}
