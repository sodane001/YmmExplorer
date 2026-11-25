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
//  斬撃エフェクト
//   弧を描く刃の軌跡・残光・火花。連続斬りやX字斬りに対応
// =====================================================================
public sealed class SlashPlugin : IShapePlugin
{
    public string Name => "演出：斬撃エフェクト";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SlashParameter(sharedData);
}

public sealed class SlashParameter : EffectShapeParameterBase
{
    public SlashParameter() : this(null) { }
    public SlashParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "刃の光の色")]
    [ColorPicker]
    public WpfColor GlowColor { get => glow; set => Set(ref glow, value); }
    WpfColor glow = WpfColor.FromRgb(0x8A, 0xF0, 0xFF);

    [Display(GroupName = "位置と形", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と形", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と形", Name = "斬撃の長さ（半径）")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation Length { get; } = new Animation(520, 20, 8000);

    [Display(GroupName = "位置と形", Name = "刃の太さ")]
    [AnimationSlider("F0", "px", 6, 140)]
    public Animation Thickness { get; } = new Animation(46, 1, 1000);

    [Display(GroupName = "位置と形", Name = "斬る向き", Description = "0=左から右へ、90=上から下へ")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Angle { get; } = new Animation(35, -3600, 3600);

    [Display(GroupName = "位置と形", Name = "弧の曲がり")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Curve { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "連続斬り", Name = "斬る回数")]
    [AnimationSlider("F0", "回", 1, 5)]
    public Animation Strokes { get; } = new Animation(2, 1, 20);

    [Display(GroupName = "連続斬り", Name = "回ごとの向きの変化")]
    [AnimationSlider("F0", "°", 0, 180)]
    public Animation AngleStep { get; } = new Animation(90, 0, 360);

    [Display(GroupName = "連続斬り", Name = "斬る間隔")]
    [AnimationSlider("F2", "秒", 0.03, 0.6)]
    public Animation Gap { get; } = new Animation(0.14, 0.01, 5);

    [Display(GroupName = "タイミング", Name = "自動でくり返す")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "タイミング", Name = "くり返しの間隔")]
    [AnimationSlider("F1", "秒", 0.5, 12)]
    public Animation Interval { get; } = new Animation(2.5, 0.3, 120);

    [Display(GroupName = "タイミング", Name = "斬る進み（手動）", Description = "「自動」がオフのとき。0→100%で斬撃が走る")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Manual { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "演出", Name = "火花の数")]
    [AnimationSlider("F0", "個", 0, 100)]
    public Animation Sparks { get; } = new Animation(24, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Length, Thickness, Angle, Curve, Strokes, AngleStep, Gap, Interval, Manual, Sparks];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Length), th = c.F(Thickness), baseAng = Gfx.Rad(c.F(Angle)), curve = c.F(Curve) / 100f;
        int strokes = Math.Clamp((int)c.F(Strokes), 1, 5);
        float step = Gfx.Rad(c.F(AngleStep)), gap = c.F(Gap);
        int ns = (int)c.F(Sparks);
        var white = WpfColor.FromRgb(255, 255, 255);
        const float dur = 0.34f, fadeEnd = 0.9f;

        float cycleT;
        if (Auto) cycleT = Gfx.Mod(t, MathF.Max(0.3f, c.F(Interval)));
        else cycleT = c.F(Manual) / 100f * (dur + gap * (strokes - 1)) ;

        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            for (int s = 0; s < strokes; s++)
            {
                float u = (cycleT - s * gap) / dur;                       // この斬撃の進み（0〜1が振る、1〜が残光）
                if (u < 0f || u > fadeEnd + 1f) continue;
                float p = Gfx.Smooth(0f, 1f, MathF.Min(1f, u));
                float fade = u <= 1f ? 1f : 1f - (u - 1f) / fadeEnd;
                if (fade <= 0f) continue;

                float ang = baseAng + s * step;
                var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                var perp = new Vector2(-dir.Y, dir.X);
                // 弧：中心を横にずらした円の一部。curve=0でほぼ直線
                float bow = R * (0.15f + 1.6f * (1f - curve));
                // 刃の軌跡の点（-R〜+R）
                const int N = 40;
                var outer = new List<Vector2>();
                var inner = new List<Vector2>();
                float s0 = MathF.Max(0f, p - 0.65f) , s1 = p;                // 見えている区間（先頭が走り、後ろが遅れて消える）
                for (int i = 0; i <= N; i++)
                {
                    float f = s0 + (s1 - s0) * i / N;
                    float x = (f * 2f - 1f) * R;
                    float sag = (1f - (x / R) * (x / R)) * (R * 0.18f * curve + bow * 0.0f);
                    float prof = MathF.Sin(MathF.PI * (i / (float)N)) ;       // 先端と後端が細い
                    float tw = th * (0.15f + 0.85f * MathF.Pow(prof, 0.8f)) * (0.6f + 0.4f * (s1 - s0) / 0.65f);
                    var center = ctr + dir * x + perp * sag;
                    outer.Add(center + perp * tw * 0.5f);
                    inner.Add(center - perp * tw * 0.5f);
                }
                var poly = new List<Vector2>(outer);
                for (int i = inner.Count - 1; i >= 0; i--) poly.Add(inner[i]);

                // にじみ→本体→芯
                var edge = ShapeGeom.Closed(poly);
                brush.Color = Gfx.C4(GlowColor, 0.10f * fade);
                Gfx.Polyline(dc, brush, edge, th * 1.4f);
                brush.Color = Gfx.C4(GlowColor, 0.45f * fade);
                Gfx.FillPoly(dc, brush, poly);
                var coreP = new List<Vector2>();
                var coreI = new List<Vector2>();
                for (int i = 0; i < outer.Count; i++)
                {
                    var mid = (outer[i] + inner[i]) / 2f;
                    coreP.Add(mid + (outer[i] - mid) * 0.45f);
                    coreI.Add(mid + (inner[i] - mid) * 0.45f);
                }
                var core = new List<Vector2>(coreP);
                for (int i = coreI.Count - 1; i >= 0; i--) core.Add(coreI[i]);
                brush.Color = Gfx.C4(white, 0.9f * fade);
                Gfx.FillPoly(dc, brush, core);

                // 先端から散る火花
                if (u < 1.4f && ns > 0)
                {
                    var tip = ctr + dir * ((p * 2f - 1f) * R);
                    var rs = new Random(Seed + s * 77);
                    for (int i = 0; i < ns; i++)
                    {
                        float a = ang + (rs.NextF() - 0.5f) * 2.4f + (rs.NextF() < 0.5f ? 0f : MathF.PI);
                        float sp = 80f + 360f * rs.NextF();
                        float age = MathF.Max(0f, u - rs.NextF() * 0.7f);
                        if (age <= 0f) continue;
                        var d2 = new Vector2(MathF.Cos(a), MathF.Sin(a));
                        var pp = tip + d2 * (sp * age * 0.5f);
                        brush.Color = Gfx.C4(Gfx.Mix(GlowColor, white, 0.5f), 0.9f * MathF.Max(0f, 1f - age) * fade);
                        dc.DrawLine(pp, pp - d2 * (6f + 14f * rs.NextF()), brush, 1.6f);
                    }
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}

public enum AuraKind
{
    [Display(Name = "オーラ（足元から立ちのぼる気）")] Aura,
    [Display(Name = "光の柱（天から降る光）")] Pillar,
}
