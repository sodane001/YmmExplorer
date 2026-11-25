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
//  クラッカー
//   破裂音の閃光 → 紙片とリボンが勢いよく飛び出す → ひらひら落ちる
// =====================================================================
public sealed class CrackerPlugin : IShapePlugin
{
    public string Name => "演出：クラッカー";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CrackerParameter(sharedData);
}

public sealed class CrackerParameter : EffectShapeParameterBase
{
    public CrackerParameter() : this(null) { }
    public CrackerParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "位置と向き", Name = "発射位置X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation OriginX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と向き", Name = "発射位置Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation OriginY { get; } = new Animation(90, -100, 200);

    [Display(GroupName = "位置と向き", Name = "向き", Description = "0=真上、プラスで右へ傾く")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Direction { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "位置と向き", Name = "広がり")]
    [AnimationSlider("F0", "°", 5, 180)]
    public Animation Spread { get; } = new Animation(55, 1, 360);

    [Display(GroupName = "紙片", Name = "カラフルにする")]
    [ToggleSlider]
    public bool Colorful { get => colorful; set => Set(ref colorful, value); }
    bool colorful = true;

    [Display(GroupName = "紙片", Name = "色（カラフルがオフのとき）")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xD0, 0x40);

    [Display(GroupName = "紙片", Name = "数")]
    [AnimationSlider("F0", "枚", 10, 400)]
    public Animation Count { get; } = new Animation(150, 1, 3000);

    [Display(GroupName = "紙片", Name = "大きさ")]
    [AnimationSlider("F0", "px", 4, 40)]
    public Animation Size { get; } = new Animation(13, 1, 500);

    [Display(GroupName = "紙片", Name = "リボンの数")]
    [AnimationSlider("F0", "本", 0, 60)]
    public Animation Streamers { get; } = new Animation(14, 0, 500);

    [Display(GroupName = "勢い", Name = "飛び出す勢い")]
    [AnimationSlider("F0", "px/s", 300, 3000)]
    public Animation Power { get; } = new Animation(1500, 10, 20000);

    [Display(GroupName = "勢い", Name = "重力")]
    [AnimationSlider("F0", "px/s²", 100, 2000)]
    public Animation Gravity { get; } = new Animation(520, 0, 20000);

    [Display(GroupName = "タイミング", Name = "発射間隔")]
    [AnimationSlider("F1", "秒", 1, 12)]
    public Animation Interval { get; } = new Animation(4, 0.5, 120);

    [Display(GroupName = "タイミング", Name = "紙片が舞う時間")]
    [AnimationSlider("F1", "秒", 1, 8)]
    public Animation Life { get; } = new Animation(4, 0.5, 60);

    [Display(GroupName = "演出", Name = "破裂の閃光と煙")]
    [ToggleSlider]
    public bool Flash { get => flash; set => Set(ref flash, value); }
    bool flash = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [OriginX, OriginY, Direction, Spread, Count, Size, Streamers, Power, Gravity, Interval, Life];

    static Vector2 Pos(Vector2 origin, Vector2 dir, float spd, float tau, float g)
    {
        const float drag = 2.6f;                     // 空気抵抗で急に減速する
        float d = spd * (1f - MathF.Exp(-drag * tau)) / drag;
        // 減速したあとは、ゆっくり落ちる（終端速度付き）
        float fallY = 0.5f * g * tau * tau;
        float vTerm = 160f;
        if (g * tau > vTerm) fallY = vTerm * tau - vTerm * vTerm / (2f * g);
        return origin + dir * d + new Vector2(0f, fallY);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var origin = new Vector2(c.F(OriginX) / 100f * w, c.F(OriginY) / 100f * h);
        float dirAng = Gfx.Rad(c.F(Direction)), spread = Gfx.Rad(c.F(Spread));
        int n = (int)c.F(Count), ns = (int)c.F(Streamers);
        float size = c.F(Size), power = c.F(Power), g = c.F(Gravity);
        float P = MathF.Max(0.5f, c.F(Interval)), life = MathF.Max(0.5f, c.F(Life));
        var white = WpfColor.FromRgb(255, 255, 255);
        int kmax = Math.Min(4, (int)MathF.Ceiling(life / P));

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            for (int k = 0; k <= kmax; k++)
            {
                int cyc = (int)MathF.Floor(t / P) - k;
                float te = t - cyc * P;
                if (te < 0f || te >= life) continue;

                // ---- 破裂の閃光と白い煙 ----
                if (Flash && te < 0.5f)
                {
                    if (te < 0.12f)
                    {
                        float fa = 1f - te / 0.12f;
                        float fr = 130f;
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        using (var fb = Gfx.Radial(dc, origin, fr,
                            (0f, Gfx.C4(white, 0.9f * fa)), (0.4f, Gfx.C4(WpfColor.FromRgb(255, 230, 160), 0.35f * fa)), (1f, Gfx.C4(white, 0f))))
                            dc.FillEllipse(new Ellipse(origin, fr, fr), fb);
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                    }
                    var rsm = new Random(Seed * 17 + cyc);
                    for (int i = 0; i < 6; i++)
                    {
                        float a = dirAng - MathF.PI / 2f + (rsm.NextF() - 0.5f) * spread;
                        float u = te / 0.5f;
                        float dist = 30f + 110f * u * (0.5f + rsm.NextF());
                        var p = origin + new Vector2(MathF.Cos(a), MathF.Sin(a)) * dist;
                        float r = 22f + 40f * u;
                        using var sb = Gfx.Radial(dc, p, r, (0f, Gfx.C4(white, 0.28f * (1f - u))), (1f, Gfx.C4(white, 0f)));
                        dc.FillEllipse(new Ellipse(p, r, r), sb);
                    }
                }

                // ---- 紙片 ----
                var rng = new Random(Seed * 7919 + cyc * 131);
                for (int i = 0; i < n; i++)
                {
                    float ang = dirAng + (rng.NextF() - 0.5f) * 2f * spread;
                    float spd = power * (0.35f + 0.75f * rng.NextF());
                    float rot0 = rng.NextF() * MathF.PI * 2f, rs = (rng.NextF() - 0.5f) * 8f;
                    float fs = 3f + 5f * rng.NextF(), fp = rng.NextF() * MathF.PI * 2f;
                    float fr2 = 0.8f + 1.2f * rng.NextF(), fph = rng.NextF() * MathF.PI * 2f;
                    int colIdx = rng.Next(0, PartyKit.Palette.Length);
                    float shapeSel = rng.NextF();

                    // 0=真上 を基準にした向きベクトル
                    var dir = new Vector2(MathF.Sin(ang), -MathF.Cos(ang));
                    var pos = Pos(origin, dir, spd, te, g);
                    pos += new Vector2(MathF.Sin(te * fr2 * 3f + fph) * 14f * MathF.Min(1f, te), 0f);
                    if (pos.Y > h + 60f || pos.X < -60f || pos.X > w + 60f) continue;

                    float alpha = 1f - Gfx.Smooth(life * 0.75f, life, te);
                    if (alpha <= 0.01f) continue;
                    var col = Colorful ? PartyKit.Palette[colIdx] : Color;
                    int shape = shapeSel < 0.6f ? 0 : (shapeSel < 0.82f ? 1 : 2);
                    PartyKit.Piece(dc, brush, pos, size, rot0 + te * rs, fp + te * fs, col, alpha, shape);
                }

                // ---- リボン（飛んだ軌跡に沿って波打つ）----
                var rr = new Random(Seed * 313 + cyc * 7);
                for (int i = 0; i < ns; i++)
                {
                    float ang = dirAng + (rr.NextF() - 0.5f) * 2f * spread * 0.9f;
                    float spd = power * (0.45f + 0.5f * rr.NextF());
                    float ph = rr.NextF() * MathF.PI * 2f;
                    int colIdx = rr.Next(0, PartyKit.Palette.Length);
                    var dir = new Vector2(MathF.Sin(ang), -MathF.Cos(ang));
                    var perp = new Vector2(-dir.Y, dir.X);
                    var col = Colorful ? PartyKit.Palette[colIdx] : Color;
                    float alpha = 1f - Gfx.Smooth(life * 0.7f, life, te);
                    if (alpha <= 0.01f) continue;

                    const int N = 14;
                    Vector2 prev = default;
                    for (int s = 0; s <= N; s++)
                    {
                        float tau = MathF.Max(0f, te - 0.6f * s / N);
                        var p = Pos(origin, dir, spd, tau, g) + perp * (MathF.Sin(tau * 11f + ph) * 7f * MathF.Min(1f, tau * 2f));
                        if (s > 0)
                        {
                            brush.Color = Gfx.C4(col, alpha * (1f - s / (float)N));
                            dc.DrawLine(prev, p, brush, 3.4f);
                        }
                        prev = p;
                    }
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            dc.Transform = Matrix3x2.Identity;
        }
    }
}
