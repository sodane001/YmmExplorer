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

public enum MangaMarkKind
{
    [Display(Name = "汗（たらっ）")] Sweat,
    [Display(Name = "汗（あせあせ・複数）")] SweatMany,
    [Display(Name = "怒りマーク")] Anger,
    [Display(Name = "はてな")] Question,
    [Display(Name = "びっくり")] Exclamation,
    [Display(Name = "ハート")] Heart,
    [Display(Name = "ため息（もわっ）")] Sigh,
    [Display(Name = "音符")] Notes,
    [Display(Name = "キラキラ")] Sparkle,
    [Display(Name = "「…」")] Dots,
    [Display(Name = "ひらめき（電球）")] Idea,
    [Display(Name = "ぷんぷん（湯気）")] Steam,
    [Display(Name = "ドキドキ（ハートが脈打つ）")] Heartbeat,
    [Display(Name = "zzz（眠い）")] Sleepy,
}

// =====================================================================
//  漫符(汗・怒りマーク・はてな など)
//   キャラクターの頭の横に出す、気持ちのマーク。それぞれに合った小さな動きがつく
// =====================================================================
public sealed class MangaMarkPlugin : IShapePlugin
{
    public string Name => "演出：漫符（汗・怒り・はてな など）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MangaMarkParameter(sharedData);
}

public sealed class MangaMarkParameter : EffectShapeParameterBase
{
    public MangaMarkParameter() : this(null) { }
    public MangaMarkParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "種類")]
    [EnumComboBox]
    public MangaMarkKind Kind { get => kind; set => Set(ref kind, value); }
    MangaMarkKind kind = MangaMarkKind.Sweat;

    [Display(GroupName = "形", Name = "色を自分で決める", Description = "オフなら、種類に合った色になります")]
    [ToggleSlider]
    public bool CustomColor { get => customColor; set => Set(ref customColor, value); }
    bool customColor = false;

    [Display(GroupName = "形", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x4F, 0xA8, 0xFF);

    [Display(GroupName = "形", Name = "ふちの色")]
    [ColorPicker]
    public WpfColor OutlineColor { get => outlineColor; set => Set(ref outlineColor, value); }
    WpfColor outlineColor = WpfColor.FromRgb(0x20, 0x20, 0x28);

    [Display(GroupName = "形", Name = "ふちの太さ")]
    [AnimationSlider("F1", "px", 0, 12)]
    public Animation OutlineWidth { get; } = new Animation(4, 0, 100);

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(60, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(30, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation Size { get; } = new Animation(130, 2, 3000);

    [Display(GroupName = "位置と大きさ", Name = "傾き")]
    [AnimationSlider("F0", "°", -90, 90)]
    public Animation Rotation { get; } = new Animation(0, -360, 360);

    [Display(GroupName = "位置と大きさ", Name = "左右反転")]
    [ToggleSlider]
    public bool Mirror { get => mirror; set => Set(ref mirror, value); }
    bool mirror = false;

    [Display(GroupName = "動き", Name = "動かす")]
    [ToggleSlider]
    public bool Animate { get => animate; set => Set(ref animate, value); }
    bool animate = true;

    [Display(GroupName = "動き", Name = "ポンと出る時間")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation InTime { get; } = new Animation(0.25, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.2, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [OutlineWidth, PosX, PosY, Size, Rotation, InTime, OutTime];

    WpfColor DefaultColor => Kind switch
    {
        MangaMarkKind.Sweat or MangaMarkKind.SweatMany => WpfColor.FromRgb(0x6C, 0xC4, 0xFF),
        MangaMarkKind.Anger or MangaMarkKind.Steam => WpfColor.FromRgb(0xF0, 0x2E, 0x3A),
        MangaMarkKind.Question => WpfColor.FromRgb(0xFF, 0xC4, 0x2E),
        MangaMarkKind.Exclamation => WpfColor.FromRgb(0xFF, 0x4D, 0x3D),
        MangaMarkKind.Heart or MangaMarkKind.Heartbeat => WpfColor.FromRgb(0xFF, 0x5C, 0x9A),
        MangaMarkKind.Sigh => WpfColor.FromRgb(0xE8, 0xEC, 0xF2),
        MangaMarkKind.Notes => WpfColor.FromRgb(0x5D, 0xA9, 0xFF),
        MangaMarkKind.Sparkle => WpfColor.FromRgb(0xFF, 0xE1, 0x5A),
        MangaMarkKind.Idea => WpfColor.FromRgb(0xFF, 0xE0, 0x4A),
        MangaMarkKind.Sleepy => WpfColor.FromRgb(0x8E, 0xA9, 0xFF),
        _ => WpfColor.FromRgb(0x33, 0x33, 0x3A),
    };

    /// <summary>しずく(先が上)</summary>
    static List<Vector2> Drop(Vector2 c, float r, float rot)
    {
        var local = new List<Vector2> { new(0f, -1.6f) };
        for (int i = 0; i <= 20; i++)
        {
            float a = -MathF.PI * 0.18f + i / 20f * MathF.PI * 1.36f;
            local.Add(new Vector2(MathF.Cos(a), MathF.Sin(a)));
        }
        return DrawKit.Place(local, c, rot, r, r);
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float inT = c.F(InTime), outT = c.F(OutTime);
        float kin = inT <= 0.001f ? 1f : DrawKit.EaseOutBack(t / inT);
        float kout = outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f);
        float alpha = MathF.Min(1f, (inT <= 0.001f ? 1f : t / (inT * 0.5f))) * kout;
        if (kin <= 0.01f || alpha <= 0.002f) return;
        float S = c.F(Size) * kin, ow = c.F(OutlineWidth);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float rot = Gfx.Rad(c.F(Rotation));
        float mir = Mirror ? -1f : 1f;
        float at = Animate ? t : 0f;
        var col = CustomColor ? Color : DefaultColor;
        var line = Gfx.C4(OutlineColor, alpha);
        var fill = Gfx.C4(col, alpha);
        using var brush = dc.CreateSolidColorBrush(default);

        // ふち取りして塗る
        void Shape(List<Vector2> pts)
        {
            if (ow > 0.05f)
            {
                brush.Color = line;
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(pts), ow * 2f);
            }
            brush.Color = fill;
            Gfx.FillPoly(dc, brush, pts);
        }
        Vector2 P(float x, float y) => ctr + DrawKit.Rot(new Vector2(x * mir, y), Vector2.Zero, rot);

        switch (Kind)
        {
            case MangaMarkKind.Sweat:
            {
                // ほおを、たらっと伝う
                float slide = Animate ? DrawKit.Frac(at * 0.45f) : 0.3f;
                var p = P(0f, -S * 0.2f + slide * S * 0.5f);
                Shape(Drop(p, S * 0.28f, rot));
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.8f * alpha);
                dc.FillEllipse(new Ellipse(p + new Vector2(-S * 0.08f, S * 0.02f), S * 0.06f, S * 0.09f), brush);
                break;
            }
            case MangaMarkKind.SweatMany:
            {
                for (int i = 0; i < 3; i++)
                {
                    float a = -0.9f + i * 0.9f;
                    float jump = Animate ? MathF.Abs(MathF.Sin(at * 9f + i * 1.3f)) * S * 0.08f : 0f;
                    var dir = new Vector2(MathF.Sin(a), -MathF.Cos(a));
                    var p = P(dir.X * S * 0.5f, dir.Y * S * 0.5f) + Vector2.Normalize(dir) * jump;
                    Shape(Drop(p, S * (0.16f + (i == 1 ? 0.04f : 0f)), rot + a * mir));
                }
                break;
            }
            case MangaMarkKind.Anger:
            {
                // 4つの、外へ反った角(十字の怒りマーク)
                float pulse = Animate ? 1f + 0.12f * MathF.Max(0f, MathF.Sin(at * 10f)) : 1f;
                float R = S * 0.5f * pulse;
                for (int q = 0; q < 4; q++)
                {
                    float a = q * MathF.PI / 2f + MathF.PI / 4f;
                    var dir = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    var nrm = new Vector2(-dir.Y, dir.X);
                    var basePt = P(0f, 0f) + dir * R * 0.25f;
                    var arc = new List<Vector2>();
                    for (int i = 0; i <= 12; i++)
                    {
                        float f = i / 12f;
                        float bend = MathF.Sin(f * MathF.PI) * R * 0.22f;
                        arc.Add(basePt + (dir * 0f) + Vector2.Lerp(-nrm, nrm, f) * R * 0.55f + dir * (R * 0.65f - bend));
                    }
                    brush.Color = line;
                    if (ow > 0.05f) Gfx.Polyline(dc, brush, arc, R * 0.24f + ow * 2f);
                    brush.Color = fill;
                    Gfx.Polyline(dc, brush, arc, R * 0.24f);
                }
                break;
            }
            case MangaMarkKind.Question:
            case MangaMarkKind.Exclamation:
            {
                string mark = Kind == MangaMarkKind.Question ? "？" : "！";
                float bounce = Animate ? -MathF.Abs(MathF.Sin(at * 5f)) * S * 0.12f : 0f;
                float tilt = Animate && Kind == MangaMarkKind.Question ? MathF.Sin(at * 3f) * 0.15f : 0f;
                var s = TextKit.Get(mark, S * 1.1f, true, "Arial Black");
                if (s.Contours.Count == 0) s = TextKit.Get(mark, S * 1.1f, true);
                // 2つ並べる「？？」「！！」も、よく使うので少しずらして重ねる
                var p = P(0f, bounce);
                float tilted = rot + tilt;
                var placed = DrawKit.Rot(p, p, tilted);
                TextKit.DrawCentered(dc, brush, s, placed, fill, line, ow, 1f, Gfx.C4(0f, 0f, 0f, 0.25f * alpha), new Vector2(S * 0.04f, S * 0.05f));
                break;
            }
            case MangaMarkKind.Heart:
            {
                float beat = Animate ? 1f + 0.1f * MathF.Max(0f, MathF.Sin(at * 7f)) : 1f;
                var p = P(0f, Animate ? -MathF.Sin(at * 2f) * S * 0.06f : 0f);
                Shape(ShapeGeom.HeartPoly(p, S * 0.5f * beat, rot));
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.7f * alpha);
                dc.FillEllipse(new Ellipse(p + new Vector2(-S * 0.18f, -S * 0.12f), S * 0.08f, S * 0.05f), brush);
                break;
            }
            case MangaMarkKind.Heartbeat:
            {
                float beatT = DrawKit.Frac(at * 1.4f);
                float beat = 1f + 0.22f * MathF.Exp(-beatT * 9f) + 0.12f * MathF.Exp(-MathF.Abs(beatT - 0.25f) * 14f);
                var p = P(0f, 0f);
                // 脈打つ線
                for (int i = 0; i < 2; i++)
                {
                    float rf = DrawKit.Frac(at * 1.4f + i * 0.5f);
                    brush.Color = Gfx.C4(col, (1f - rf) * 0.6f * alpha);
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(ShapeGeom.HeartPoly(p, S * 0.5f * (1f + rf * 0.6f), rot)), MathF.Max(2f, S * 0.03f));
                }
                Shape(ShapeGeom.HeartPoly(p, S * 0.45f * beat, rot));
                break;
            }
            case MangaMarkKind.Sigh:
            {
                // もわっとした息の雲が、ふわっと流れて薄くなる
                float drift = Animate ? DrawKit.Frac(at * 0.4f) : 0.3f;
                var p = P(drift * S * 0.6f, -drift * S * 0.2f);
                float a2 = alpha * (1f - drift * 0.7f);
                var cloud = BubbleKit.CloudPts(p, S * 0.42f * (0.8f + drift * 0.4f), S * 0.26f * (0.8f + drift * 0.4f), 6, 0.18f);
                brush.Color = Gfx.C4(OutlineColor, a2 * 0.8f);
                if (ow > 0.05f) Gfx.Polyline(dc, brush, ShapeGeom.Closed(cloud), ow * 1.5f);
                brush.Color = Gfx.C4(col, a2);
                Gfx.FillPoly(dc, brush, cloud);
                break;
            }
            case MangaMarkKind.Notes:
            {
                for (int i = 0; i < 2; i++)
                {
                    float f = Animate ? DrawKit.Frac(at * 0.5f + i * 0.5f) : 0.3f + i * 0.3f;
                    var p = P((i - 0.5f) * S * 0.5f + MathF.Sin(f * 6f) * S * 0.08f, S * 0.3f - f * S * 0.7f);
                    var s = TextKit.Get(i == 0 ? "♪" : "♫", S * 0.75f, true, "Segoe UI Symbol");
                    float a2 = alpha * (f < 0.8f ? 1f : (1f - f) / 0.2f);
                    TextKit.DrawCentered(dc, brush, s, p, Gfx.C4(col, a2), Gfx.C4(OutlineColor, a2), ow * 0.7f);
                }
                break;
            }
            case MangaMarkKind.Sparkle:
            {
                for (int i = 0; i < 3; i++)
                {
                    float tw = Animate ? 0.6f + 0.4f * MathF.Sin(at * 6f + i * 2.1f) : 1f;
                    var p = P((i - 1) * S * 0.38f, (i == 1 ? -S * 0.25f : S * 0.05f));
                    float r = S * (i == 1 ? 0.32f : 0.2f) * tw;
                    Shape(ShapeGeom.StarPoly(p, r, 4, 0.28f, rot));
                }
                break;
            }
            case MangaMarkKind.Dots:
            {
                int n = Animate ? Math.Min(3, (int)(at / 0.35f) + 1) : 3;
                for (int i = 0; i < n; i++)
                {
                    var p = P((i - 1) * S * 0.32f, 0f);
                    brush.Color = line;
                    dc.FillEllipse(new Ellipse(p, S * 0.09f + ow, S * 0.09f + ow), brush);
                    brush.Color = fill;
                    dc.FillEllipse(new Ellipse(p, S * 0.09f, S * 0.09f), brush);
                }
                break;
            }
            case MangaMarkKind.Idea:
            {
                var p = P(0f, -S * 0.1f);
                float glow = Animate ? 0.7f + 0.3f * MathF.Sin(at * 8f) : 1f;
                DrawKit.Glow(dc, p, S * 0.8f, Gfx.C4(col, 0.45f * alpha * glow));
                // 光の線
                brush.Color = Gfx.C4(col, alpha);
                for (int i = 0; i < 7; i++)
                {
                    float a = -MathF.PI + i / 6f * MathF.PI;
                    var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    float len = S * (0.12f + 0.06f * glow);
                    dc.DrawLine(p + d * S * 0.45f, p + d * (S * 0.45f + len), brush, MathF.Max(2f, S * 0.05f));
                }
                Shape(DrawKit.EllipsePts(p, S * 0.3f, S * 0.32f, 40));
                brush.Color = Gfx.C4(0.55f, 0.57f, 0.62f, alpha);
                dc.FillRectangle(new RawRectF(p.X - S * 0.13f, p.Y + S * 0.27f, p.X + S * 0.13f, p.Y + S * 0.45f), brush);
                brush.Color = line;
                for (int i = 0; i < 2; i++)
                    dc.DrawLine(new Vector2(p.X - S * 0.13f, p.Y + S * (0.33f + i * 0.07f)), new Vector2(p.X + S * 0.13f, p.Y + S * (0.33f + i * 0.07f)), brush, MathF.Max(1f, ow * 0.6f));
                break;
            }
            case MangaMarkKind.Steam:
            {
                // ぷんぷん: 頭の上から湯気が2本
                for (int i = 0; i < 2; i++)
                {
                    float f = Animate ? DrawKit.Frac(at * 0.9f + i * 0.5f) : 0.4f;
                    var basePt = P((i - 0.5f) * S * 0.55f, S * 0.3f);
                    var pts = new List<Vector2>();
                    for (int j = 0; j <= 16; j++)
                    {
                        float g = j / 16f;
                        pts.Add(basePt + new Vector2(MathF.Sin(g * 9f + f * 6f) * S * 0.08f * mir, -g * S * 0.7f * (0.6f + f * 0.4f)));
                    }
                    brush.Color = Gfx.C4(col, alpha * (1f - f * 0.6f));
                    Gfx.Polyline(dc, brush, pts, MathF.Max(2f, S * 0.07f));
                }
                break;
            }
            case MangaMarkKind.Sleepy:
            {
                for (int i = 0; i < 3; i++)
                {
                    float f = Animate ? DrawKit.Frac(at * 0.35f + i / 3f) : i / 3f;
                    var p = P(f * S * 0.6f, -f * S * 0.8f);
                    var s = TextKit.Get("Z", S * (0.35f + f * 0.35f), true, "Arial Black");
                    float a2 = alpha * MathF.Sin(MathF.Min(1f, f * 1.25f) * MathF.PI);
                    TextKit.DrawCentered(dc, brush, s, p, Gfx.C4(col, a2), Gfx.C4(OutlineColor, a2), ow * 0.7f);
                }
                break;
            }
        }
    }
}
