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

public enum StageKind
{
    [Display(Name = "ミラーボール")] MirrorBall,
    [Display(Name = "ステージ照明")] Spotlights,
    [Display(Name = "両方")] Both,
}

// =====================================================================
//  ミラーボール・ステージ照明
//   回るミラーボールと飛び回る光の粒、首を振るスポットライト
// =====================================================================
public sealed class MirrorBallPlugin : IShapePlugin
{
    public string Name => "演出：ミラーボール・ステージ照明";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MirrorBallParameter(sharedData);
}

public sealed class MirrorBallParameter : EffectShapeParameterBase
{
    public MirrorBallParameter() : this(null) { }
    public MirrorBallParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public StageKind Kind { get => kind; set => Set(ref kind, value); }
    StageKind kind = StageKind.Both;

    [Display(GroupName = "ミラーボール", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ミラーボール", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(16, -100, 200);

    [Display(GroupName = "ミラーボール", Name = "半径")]
    [AnimationSlider("F0", "px", 20, 300)]
    public Animation BallSize { get; } = new Animation(90, 2, 3000);

    [Display(GroupName = "ミラーボール", Name = "光の粒の数")]
    [AnimationSlider("F0", "個", 0, 300)]
    public Animation Spots { get; } = new Animation(90, 0, 2000);

    [Display(GroupName = "照明", Name = "スポットライトの数")]
    [AnimationSlider("F0", "本", 1, 10)]
    public Animation Lights { get; } = new Animation(5, 1, 30);

    [Display(GroupName = "照明", Name = "下から照らす")]
    [ToggleSlider]
    public bool FromBottom { get => fromBottom; set => Set(ref fromBottom, value); }
    bool fromBottom = false;

    [Display(GroupName = "照明", Name = "いろいろな色にする")]
    [ToggleSlider]
    public bool Multicolor { get => multicolor; set => Set(ref multicolor, value); }
    bool multicolor = true;

    [Display(GroupName = "照明", Name = "1色の時の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xF0, 0xC8);

    [Display(GroupName = "全体", Name = "速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "全体", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "全体", Name = "暗い会場の背景")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Darkness { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, BallSize, Spots, Lights, Speed, Brightness, Darkness];

    WpfColor Col(int i, float t) => Multicolor ? Gfx.Hsv(i * 67f + t * 20f, 0.7f, 1f) : Color;

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time * c.F(Speed) / 100f;
        float br = c.F(Brightness) / 100f, dark = c.F(Darkness) / 100f;
        var bc = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(BallSize);
        var white = WpfColor.FromRgb(255, 255, 255);
        bool ball = Kind != StageKind.Spotlights, lights = Kind != StageKind.MirrorBall;

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (dark > 0f)
            {
                using var g = Gfx.Radial(dc, new Vector2(w / 2f, h * 0.6f), MathF.Max(w, h) * 0.8f, (0f, Gfx.C4(0.06f, 0.03f, 0.1f, dark)), (1f, Gfx.C4(0f, 0f, 0.02f, dark)));
                dc.FillRectangle(new RawRectF(0, 0, w, h), g);
            }

            dc.PrimitiveBlend = PrimitiveBlend.Add;
            // ---- スポットライト ----
            if (lights)
            {
                int n = Math.Max(1, (int)c.F(Lights));
                for (int i = 0; i < n; i++)
                {
                    float x = w * (i + 0.5f) / n;
                    var src = new Vector2(x, FromBottom ? h + 20f : -20f);
                    float swing = MathF.Sin(t * (0.6f + 0.25f * (i % 3)) + i * 1.9f) * 0.55f;
                    float baseAng = FromBottom ? -MathF.PI / 2f : MathF.PI / 2f;
                    float ang = baseAng + swing * (FromBottom ? -1f : 1f);
                    var d = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                    float len = h * 1.25f, spread = 0.13f;
                    var nrm = new Vector2(-d.Y, d.X);
                    var col = Col(i, t);
                    for (int k = 0; k < 6; k++)
                    {
                        float sw = len * spread * (1f - k * 0.15f);
                        var cone = new List<Vector2> { src - nrm * 6f, src + nrm * 6f, src + d * len + nrm * sw, src + d * len - nrm * sw };
                        brush.Color = Gfx.C4(col, 0.045f * br);
                        Gfx.FillPoly(dc, brush, DrawKit.ClipRect(cone, 0, 0, w, h));
                    }
                    // 床（または天井）に当たる光
                    float hitY = FromBottom ? h * 0.12f : h * 0.88f;
                    float tt = (hitY - src.Y) / d.Y;
                    if (tt > 0f)
                    {
                        var hit = src + d * tt;
                        var prev = dc.Transform;
                        dc.Transform = Matrix3x2.CreateScale(1f, 0.3f, hit) * prev;
                        DrawKit.Glow(dc, hit, tt * spread * 1.1f, Gfx.C4(col, 0.35f * br));
                        dc.Transform = prev;
                    }
                    DrawKit.Glow(dc, src, 40f, Gfx.C4(col, 0.9f * br));
                }
            }

            // ---- ミラーボールの光の粒（画面を回る）----
            if (ball)
            {
                int n = (int)c.F(Spots);
                for (int i = 0; i < n; i++)
                {
                    float a = DrawKit.H(Seed, i, 1) * MathF.PI * 2f + t * 0.35f;
                    float el = DrawKit.H(Seed, i, 2);
                    // 球の上の向き → 画面の位置（横は回転で流れる）
                    float x = bc.X + MathF.Sin(a) * w * (0.3f + 0.7f * el);
                    float y = bc.Y + R + (h - bc.Y) * (0.1f + 0.9f * DrawKit.H(Seed, i, 3));
                    float vis = MathF.Cos(a);                                 // 奥を向いた面は光らない
                    if (vis < 0f) continue;
                    var col = Multicolor ? Col(i, t) : Color;
                    float sz = 6f + 8f * DrawKit.H(Seed, i, 4);
                    float tw = 0.6f + 0.4f * MathF.Sin(t * 6f + i);
                    DrawKit.Glow(dc, new Vector2(x, y), sz * 2.2f, Gfx.C4(col, 0.5f * vis * tw * br));
                    brush.Color = Gfx.C4(Gfx.Mix(col, white, 0.6f), 0.8f * vis * tw * br);
                    dc.FillEllipse(new Ellipse(new Vector2(x, y), sz * 0.35f, sz * 0.22f), brush);
                }
                // 球から出る光の筋
                for (int i = 0; i < 10; i++)
                {
                    float a = DrawKit.H(Seed, i, 5) * MathF.PI + t * 0.35f;
                    var d = new Vector2(MathF.Cos(a), MathF.Abs(MathF.Sin(a)) + 0.2f);
                    d = Vector2.Normalize(d);
                    Gfx.FadeLine(dc, brush, bc + d * R, bc + d * (R + h * 0.6f), 10f, 60f, Col(i, t), 0.12f * br, 10, 1.2f);
                }
            }
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

            // ---- ミラーボール本体 ----
            if (ball && R > 1f)
            {
                brush.Color = Gfx.C4(0.6f, 0.6f, 0.65f, 1f);
                dc.DrawLine(new Vector2(bc.X, 0), new Vector2(bc.X, bc.Y - R), brush, 3f);
                brush.Color = Gfx.C4(0.18f, 0.18f, 0.22f, 1f);
                dc.FillEllipse(new Ellipse(bc, R, R), brush);
                int rows = 12, cols = 24;
                float rot = t * 0.35f;
                for (int r = 0; r < rows; r++)
                {
                    float lat0 = -MathF.PI / 2f + MathF.PI * r / rows, lat1 = -MathF.PI / 2f + MathF.PI * (r + 1) / rows;
                    for (int q = 0; q < cols; q++)
                    {
                        float lon0 = rot + MathF.PI * 2f * q / cols, lon1 = rot + MathF.PI * 2f * (q + 1) / cols;
                        float zc = MathF.Cos((lat0 + lat1) / 2f) * MathF.Cos((lon0 + lon1) / 2f);
                        if (zc <= 0f) continue;
                        Vector2 P(float la, float lo) => bc + new Vector2(MathF.Cos(la) * MathF.Sin(lo), MathF.Sin(la)) * R;
                        var quad = new List<Vector2> { P(lat0, lon0), P(lat0, lon1), P(lat1, lon1), P(lat1, lon0) };
                        float shade = 0.35f + 0.65f * zc;
                        float glint = Gfx.Hash01(r * 97 + q * 13 + (int)(t * 6f)) > 0.93f ? 1f : 0f;
                        var fc = glint > 0f ? Gfx.Mix(Col(r + q, t), white, 0.6f) : Gfx.Mix(WpfColor.FromRgb(90, 95, 110), WpfColor.FromRgb(235, 238, 245), shade * (0.6f + 0.4f * Gfx.Hash01(r * 31 + q)));
                        brush.Color = Gfx.C4(fc, 1f);
                        var inner = quad.Select(p => Vector2.Lerp(p, (quad[0] + quad[2]) / 2f, 0.1f)).ToList();
                        Gfx.FillPoly(dc, brush, inner);
                    }
                }
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, bc - new Vector2(R * 0.35f, R * 0.35f), R * 0.6f, Gfx.C4(white, 0.4f * br));
                DrawKit.Twinkle(dc, brush, bc + new Vector2(R * 0.3f, -R * 0.4f), R * 0.8f * (0.5f + 0.5f * MathF.Sin(t * 5f)), 2f, white, 0.8f * br, t);
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
