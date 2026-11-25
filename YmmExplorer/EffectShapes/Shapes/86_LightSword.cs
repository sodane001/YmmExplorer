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
//  光の剣
//   ライトセーバー風の刃が光り、振ると残光が出る
// =====================================================================
public sealed class LightSwordPlugin : IShapePlugin
{
    public string Name => "演出：光の剣";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LightSwordParameter(sharedData);
}

public sealed class LightSwordParameter : EffectShapeParameterBase
{
    public LightSwordParameter() : this(null) { }
    public LightSwordParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "刃の色")]
    [ColorPicker]
    public WpfColor BladeColor { get => bladeColor; set => Set(ref bladeColor, value); }
    WpfColor bladeColor = WpfColor.FromRgb(0x3C, 0xB4, 0xFF);

    [Display(GroupName = "色", Name = "柄の色")]
    [ColorPicker]
    public WpfColor HiltColor { get => hiltColor; set => Set(ref hiltColor, value); }
    WpfColor hiltColor = WpfColor.FromRgb(0x9A, 0xA2, 0xAE);

    [Display(GroupName = "位置と大きさ", Name = "柄のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "柄のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(75, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "向き（0°=真上）")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(-25, -3600, 3600);

    [Display(GroupName = "位置と大きさ", Name = "刃の長さ")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation Length { get; } = new Animation(560, 10, 10000);

    [Display(GroupName = "位置と大きさ", Name = "刃の太さ")]
    [AnimationSlider("F1", "px", 4, 60)]
    public Animation BladeWidth { get; } = new Animation(16, 1, 500);

    [Display(GroupName = "光", Name = "刃が伸びる時間（最初）")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation Ignite { get; } = new Animation(0.35, 0, 60);

    [Display(GroupName = "光", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Glow { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "光", Name = "ゆらぎ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "振る", Name = "自動で振る")]
    [ToggleSlider]
    public bool Swing { get => swing; set => Set(ref swing, value); }
    bool swing = true;

    [Display(GroupName = "振る", Name = "振る角度")]
    [AnimationSlider("F0", "°", 10, 270)]
    public Animation SwingAngle { get; } = new Animation(130, 0, 720);

    [Display(GroupName = "振る", Name = "振る速さ（1回の時間）")]
    [AnimationSlider("F2", "秒", 0.1, 1.5)]
    public Animation SwingTime { get; } = new Animation(0.3, 0.03, 30);

    [Display(GroupName = "振る", Name = "振る間隔")]
    [AnimationSlider("F1", "秒", 0.3, 5)]
    public Animation SwingInterval { get; } = new Animation(1.4, 0.1, 120);

    [Display(GroupName = "振る", Name = "残光を表示する")]
    [ToggleSlider]
    public bool Trail { get => trail; set => Set(ref trail, value); }
    bool trail = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Angle, Length, BladeWidth, Ignite, Glow, Flicker, SwingAngle, SwingTime, SwingInterval];

    /// <summary>時刻 τ の刃の角度（度）</summary>
    float AngleAt(float tau, float baseA, float sw, float st, float si)
    {
        if (!Swing || sw <= 0f) return baseA;
        if (tau < 0f) tau = 0f;
        int k = (int)MathF.Floor(tau / si);
        float p = tau - k * si;
        float s = Gfx.Smooth(0f, 1f, p / MathF.Max(0.001f, st));
        bool fwd = k % 2 == 0;
        float start = baseA + (fwd ? -sw / 2f : sw / 2f);
        return start + (fwd ? 1f : -1f) * sw * s;
    }

    static Vector2 Dir(float deg)
    {
        float r = Gfx.Rad(deg);
        return new Vector2(MathF.Sin(r), -MathF.Cos(r));
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var hilt = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float baseA = c.F(Angle), L = c.F(Length), bw = c.F(BladeWidth), glow = c.F(Glow) / 100f;
        float sw = c.F(SwingAngle), st = MathF.Max(0.03f, c.F(SwingTime)), si = MathF.Max(st + 0.05f, c.F(SwingInterval));
        float ign = c.F(Ignite);
        float ext = ign <= 0.001f ? 1f : DrawKit.EaseOut(t / ign);
        float fl = c.F(Flicker) / 100f;
        float flick = 1f - fl * 0.35f * (0.5f + 0.5f * MathF.Sin(t * 53f)) - fl * 0.25f * Gfx.Hash01((int)c.Frame + Seed * 97);

        float ang = AngleAt(t, baseA, sw, st, si);
        var d = Dir(ang);
        float hiltLen = bw * 7f;
        var emitter = hilt + d * hiltLen * 0.5f;
        var tip = emitter + d * L * ext;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;

            // ---- 残光（振った軌跡の扇形）----
            if (Trail && Swing && ext > 0.5f)
            {
                const int steps = 14;
                var prevTip = tip;
                var prevEm = emitter;
                for (int j = 1; j <= steps; j++)
                {
                    float tau = t - j * 0.014f;
                    var dj = Dir(AngleAt(tau, baseA, sw, st, si));
                    var em = hilt + dj * hiltLen * 0.5f;
                    var tp = em + dj * L * ext;
                    if (Vector2.Distance(tp, prevTip) < 0.5f) { prevTip = tp; prevEm = em; continue; }
                    float a = 0.28f * (1f - (j - 1f) / steps) * glow;
                    brush.Color = Gfx.C4(BladeColor, a);
                    Gfx.FillPoly(dc, brush, new List<Vector2> { prevEm, prevTip, tp, em });
                    brush.Color = Gfx.C4(Gfx.Mix(BladeColor, white, 0.6f), a * 0.6f);
                    Gfx.FillPoly(dc, brush, new List<Vector2> { Vector2.Lerp(prevEm, prevTip, 0.25f), prevTip, tp, Vector2.Lerp(em, tp, 0.25f) });
                    prevTip = tp; prevEm = em;
                }
            }

            // ---- 刃：外側のにじみ → 芯 ----
            if (ext > 0.001f)
            {
                float[] widths = { 7f, 4.2f, 2.6f, 1.6f };
                float[] alphas = { 0.06f, 0.11f, 0.22f, 0.45f };
                for (int k = 0; k < widths.Length; k++)
                {
                    float ww = bw * (k < 2 ? 1f + (widths[k] - 1f) * glow : widths[k]);
                    brush.Color = Gfx.C4(BladeColor, alphas[k] * flick);
                    dc.DrawLine(emitter, tip, brush, ww);
                    dc.FillEllipse(new Ellipse(tip, ww / 2f, ww / 2f), brush);
                }
                brush.Color = Gfx.C4(Gfx.Mix(BladeColor, white, 0.85f), 0.95f * flick);
                dc.DrawLine(emitter, tip, brush, bw * 0.55f);
                dc.FillEllipse(new Ellipse(tip, bw * 0.28f, bw * 0.28f), brush);
                DrawKit.Glow(dc, emitter, bw * 3f, Gfx.C4(BladeColor, 0.5f * flick));
            }

            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

            // ---- 柄 ----
            var hb = hilt - d * hiltLen * 0.5f;
            var nrm = new Vector2(-d.Y, d.X);
            brush.Color = Gfx.C4(Gfx.Mix(HiltColor, WpfColor.FromRgb(0, 0, 0), 0.45f), 1f);
            dc.DrawLine(hb, emitter, brush, bw * 2.3f);
            brush.Color = Gfx.C4(HiltColor, 1f);
            dc.DrawLine(hb, emitter, brush, bw * 1.9f);
            brush.Color = Gfx.C4(Gfx.Mix(HiltColor, white, 0.5f), 0.8f);
            dc.DrawLine(hb + nrm * bw * 0.45f, emitter + nrm * bw * 0.45f, brush, bw * 0.35f);
            brush.Color = Gfx.C4(0.12f, 0.12f, 0.14f, 1f);
            for (int k = 0; k < 4; k++)
            {
                var m = Vector2.Lerp(hb, emitter, 0.15f + k * 0.11f);
                dc.DrawLine(m - nrm * bw * 1.05f, m + nrm * bw * 1.05f, brush, bw * 0.4f);
            }
            brush.Color = Gfx.C4(Gfx.Mix(HiltColor, white, 0.25f), 1f);
            dc.DrawLine(emitter - nrm * bw * 1.35f, emitter + nrm * bw * 1.35f, brush, bw * 0.8f);
            dc.DrawLine(hb - nrm * bw * 1.15f, hb + nrm * bw * 1.15f, brush, bw * 0.6f);
            brush.Color = Gfx.C4(0.9f, 0.15f, 0.15f, 1f);
            dc.FillEllipse(new Ellipse(Vector2.Lerp(hb, emitter, 0.72f) + nrm * bw * 0.6f, bw * 0.22f, bw * 0.22f), brush);
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
