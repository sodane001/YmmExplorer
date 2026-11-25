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

public enum BuffKind
{
    [Display(Name = "回復（緑の十字）")] Heal,
    [Display(Name = "強化（上向きの矢印）")] Buff,
    [Display(Name = "弱体化（下向きの暗い気）")] Debuff,
}

// =====================================================================
//  回復・強化エフェクト
//   緑の十字と上昇する粒で回復、矢印で強化、下向きの暗い気で弱体化
// =====================================================================
public sealed class HealBuffPlugin : IShapePlugin
{
    public string Name => "演出：回復・強化エフェクト";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HealBuffParameter(sharedData);
}

public sealed class HealBuffParameter : EffectShapeParameterBase
{
    public HealBuffParameter() : this(null) { }
    public HealBuffParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public BuffKind Kind { get => kind; set => Set(ref kind, value); }
    BuffKind kind = BuffKind.Heal;

    [Display(GroupName = "色", Name = "回復の色")]
    [ColorPicker]
    public WpfColor HealColor { get => healColor; set => Set(ref healColor, value); }
    WpfColor healColor = WpfColor.FromRgb(0x5C, 0xFF, 0x8A);

    [Display(GroupName = "色", Name = "強化の色")]
    [ColorPicker]
    public WpfColor BuffColor { get => buffColor; set => Set(ref buffColor, value); }
    WpfColor buffColor = WpfColor.FromRgb(0xFF, 0xC8, 0x3C);

    [Display(GroupName = "色", Name = "弱体化の色")]
    [ColorPicker]
    public WpfColor DebuffColor { get => debuffColor; set => Set(ref debuffColor, value); }
    WpfColor debuffColor = WpfColor.FromRgb(0x9A, 0x4C, 0xFF);

    [Display(GroupName = "位置と大きさ", Name = "足元のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "足元のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(75, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "広がり（半径）")]
    [AnimationSlider("F0", "px", 30, 500)]
    public Animation Radius { get; } = new Animation(170, 5, 5000);

    [Display(GroupName = "位置と大きさ", Name = "高さ")]
    [AnimationSlider("F0", "px", 50, 1000)]
    public Animation RiseHeight { get; } = new Animation(380, 10, 8000);

    [Display(GroupName = "粒", Name = "数")]
    [AnimationSlider("F0", "個", 0, 120)]
    public Animation Count { get; } = new Animation(26, 0, 1000);

    [Display(GroupName = "粒", Name = "十字・矢印の大きさ")]
    [AnimationSlider("F0", "px", 6, 80)]
    public Animation IconSize { get; } = new Animation(26, 2, 500);

    [Display(GroupName = "粒", Name = "速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "装飾", Name = "足元の輪を表示する")]
    [ToggleSlider]
    public bool BaseRing { get => baseRing; set => Set(ref baseRing, value); }
    bool baseRing = true;

    [Display(GroupName = "装飾", Name = "光の柱を表示する")]
    [ToggleSlider]
    public bool Pillar { get => pillar; set => Set(ref pillar, value); }
    bool pillar = true;

    [Display(GroupName = "装飾", Name = "全体の明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, RiseHeight, Count, IconSize, Speed, Brightness];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time * c.F(Speed) / 100f;
        var feet = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), H = c.F(RiseHeight), icon = c.F(IconSize), bright = c.F(Brightness) / 100f;
        int n = (int)c.F(Count);
        bool down = Kind == BuffKind.Debuff;
        var col = Kind switch { BuffKind.Buff => BuffColor, BuffKind.Debuff => DebuffColor, _ => HealColor };
        var white = WpfColor.FromRgb(255, 255, 255);
        var hot = Gfx.Mix(col, white, 0.55f);
        float pulse = 0.5f + 0.5f * MathF.Sin(t * 4f);

        using var brush = dc.CreateSolidColorBrush(default);
        var prev = dc.Transform;
        try
        {
            // ---- 弱体化：足元の暗い影と、まとわりつく黒い気 ----
            if (down)
            {
                dc.Transform = Matrix3x2.CreateScale(1f, 0.32f, feet) * prev;
                using (var sh = Gfx.Radial(dc, feet, R * 1.3f, (0f, Gfx.C4(0.02f, 0f, 0.05f, 0.75f * bright)), (1f, Gfx.C4(0f, 0f, 0f, 0f))))
                    dc.FillEllipse(new Ellipse(feet, R * 1.3f, R * 1.3f), sh);
                dc.Transform = prev;
                var rng0 = new Random(Seed + 5);
                for (int i = 0; i < 14; i++)
                {
                    float a = rng0.NextF() * MathF.PI * 2f + t * (0.5f + rng0.NextF());
                    float ph = DrawKit.Frac(t * 0.25f + rng0.NextF());
                    float rr = R * (0.3f + 0.7f * rng0.NextF());
                    var p = feet + new Vector2(MathF.Cos(a) * rr, -H * 0.55f * (1f - ph) + MathF.Sin(a) * rr * 0.25f);
                    float s = R * (0.35f + 0.3f * rng0.NextF()) * (0.6f + 0.6f * ph);
                    using var g = Gfx.Radial(dc, p, s, (0f, Gfx.C4(0.05f, 0f, 0.1f, 0.35f * MathF.Sin(MathF.PI * ph) * bright)), (1f, Gfx.C4(0f, 0f, 0f, 0f)));
                    dc.FillEllipse(new Ellipse(p, s, s), g);
                }
            }

            dc.PrimitiveBlend = PrimitiveBlend.Add;

            // ---- 光の柱 ----
            if (Pillar)
            {
                float a = (down ? 0.22f : 0.35f) * (0.8f + 0.2f * pulse) * bright;
                using var lg = Gfx.Linear(dc, feet, feet - new Vector2(0, H),
                    (0f, Gfx.C4(col, down ? 0f : a)), (down ? 0.6f : 0.15f, Gfx.C4(col, a)), (1f, Gfx.C4(col, down ? a * 0.8f : 0f)));
                dc.FillRectangle(new RawRectF(feet.X - R * 0.75f, feet.Y - H, feet.X + R * 0.75f, feet.Y), lg);
                using var lg2 = Gfx.Linear(dc, feet, feet - new Vector2(0, H),
                    (0f, Gfx.C4(hot, down ? 0f : a)), (1f, Gfx.C4(hot, 0f)));
                dc.FillRectangle(new RawRectF(feet.X - R * 0.2f, feet.Y - H, feet.X + R * 0.2f, feet.Y), lg2);
            }

            // ---- 足元の輪 ----
            if (BaseRing)
            {
                dc.Transform = Matrix3x2.CreateScale(1f, 0.3f, feet) * prev;
                DrawKit.Glow(dc, feet, R, Gfx.C4(col, 0.45f * bright));
                for (int k = 0; k < 2; k++)
                {
                    float u = DrawKit.Frac(t * 0.6f + k * 0.5f);
                    float uu = down ? 1f - u : u;
                    brush.Color = Gfx.C4(col, 0.9f * MathF.Sin(MathF.PI * u) * bright);
                    dc.DrawEllipse(new Ellipse(feet, R * (0.3f + 0.8f * uu), R * (0.3f + 0.8f * uu)), brush, 6f);
                }
                brush.Color = Gfx.C4(hot, (0.6f + 0.3f * pulse) * bright);
                dc.DrawEllipse(new Ellipse(feet, R, R), brush, 4f);
                dc.Transform = prev;
            }

            // ---- 上がる（下がる）十字・矢印と粒 ----
            var rng = new Random(Seed);
            for (int i = 0; i < n; i++)
            {
                float spd = 0.35f + 0.45f * rng.NextF();
                float ph = DrawKit.Frac(t * spd * 0.55f + rng.NextF());
                float ox = (rng.NextF() - 0.5f) * 2f * R;
                float wob = rng.NextF() * MathF.PI * 2f;
                bool isIcon = rng.NextF() < 0.45f;
                float sz = icon * (0.6f + 0.6f * rng.NextF());
                float along = down ? 1f - ph : ph;
                var p = feet + new Vector2(ox * (1f - 0.35f * ph) + MathF.Sin(t * 2f + wob) * 10f, -H * along);
                float alpha = MathF.Sin(MathF.PI * ph) * bright;

                if (!isIcon)
                {
                    DrawKit.Glow(dc, p, sz * 0.7f, Gfx.C4(col, 0.6f * alpha));
                    brush.Color = Gfx.C4(hot, 0.9f * alpha);
                    dc.FillEllipse(new Ellipse(p, sz * 0.1f + 1.5f, sz * 0.1f + 1.5f), brush);
                    continue;
                }

                DrawKit.Glow(dc, p, sz * 1.3f, Gfx.C4(col, 0.35f * alpha));
                switch (Kind)
                {
                    case BuffKind.Heal:
                        {
                            float a = sz * 0.5f, b = sz * 0.17f;
                            brush.Color = Gfx.C4(col, 0.85f * alpha);
                            dc.FillRectangle(new RawRectF(p.X - a, p.Y - b, p.X + a, p.Y + b), brush);
                            dc.FillRectangle(new RawRectF(p.X - b, p.Y - a, p.X + b, p.Y + a), brush);
                            brush.Color = Gfx.C4(white, 0.8f * alpha);
                            dc.FillRectangle(new RawRectF(p.X - a * 0.7f, p.Y - b * 0.4f, p.X + a * 0.7f, p.Y + b * 0.4f), brush);
                            dc.FillRectangle(new RawRectF(p.X - b * 0.4f, p.Y - a * 0.7f, p.X + b * 0.4f, p.Y + a * 0.7f), brush);
                            break;
                        }
                    default:
                        {
                            float dir = down ? 1f : -1f;
                            float lw = MathF.Max(1.5f, sz * 0.16f);
                            for (int k = 0; k < 2; k++)
                            {
                                var tip = p + new Vector2(0, dir * (sz * 0.35f - k * sz * 0.38f));
                                var l = tip + new Vector2(-sz * 0.45f, -dir * sz * 0.4f);
                                var r = tip + new Vector2(sz * 0.45f, -dir * sz * 0.4f);
                                brush.Color = Gfx.C4(col, 0.9f * alpha * (k == 0 ? 1f : 0.6f));
                                Gfx.Polyline(dc, brush, new List<Vector2> { l, tip, r }, lw);
                                brush.Color = Gfx.C4(white, 0.6f * alpha * (k == 0 ? 1f : 0.5f));
                                Gfx.Polyline(dc, brush, new List<Vector2> { l, tip, r }, lw * 0.4f);
                            }
                            break;
                        }
                }
            }
        }
        finally
        {
            dc.Transform = prev;
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
