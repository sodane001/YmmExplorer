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

public enum ReactionKind
{
    [Display(Name = "ハート")] Heart,
    [Display(Name = "いいね（親指）")] Thumb,
    [Display(Name = "星")] Star,
    [Display(Name = "まぜる")] Mixed,
}

public enum ReactionColor
{
    [Display(Name = "ピンク系")] Pink,
    [Display(Name = "虹色")] Rainbow,
    [Display(Name = "指定の色")] Custom,
}

/// <summary>いいね(親指)の形</summary>
internal static class ThumbKit
{
    public static List<Vector2> Thumb(Vector2 c, float r, float rot)
    {
        // 単位の大きさ(-1〜1)で作って、置く
        var local = new List<Vector2>
        {
            new(-0.95f, -0.05f), new(-0.45f, -0.05f), new(-0.45f, 0.9f), new(-0.95f, 0.9f), // 袖
            new(-0.95f, -0.05f), new(-0.35f, -0.05f), new(-0.05f, -0.55f), new(0.0f, -0.95f), new(0.2f, -0.98f), new(0.32f, -0.8f),
            new(0.25f, -0.35f), new(0.8f, -0.32f), new(0.95f, -0.18f), new(0.9f, 0.05f), new(0.95f, 0.22f), new(0.85f, 0.42f),
            new(0.88f, 0.58f), new(0.75f, 0.75f), new(0.7f, 0.88f), new(-0.35f, 0.9f),
        };
        return DrawKit.Place(local.Skip(4), c, rot, r, r);
    }

    public static List<Vector2> Cuff(Vector2 c, float r, float rot)
        => DrawKit.Place(new[] { new Vector2(-0.98f, -0.08f), new Vector2(-0.48f, -0.08f), new Vector2(-0.48f, 0.92f), new Vector2(-0.98f, 0.92f) }, c, rot, r, r);
}

// =====================================================================
//  いいね・ハートが湧く
//   ライブ配信のように、下からハートなどが揺れながら浮かんで消える
// =====================================================================
public sealed class HeartsRisePlugin : IShapePlugin
{
    public string Name => "演出：いいね・ハートが湧く";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HeartsRiseParameter(sharedData);
}

public sealed class HeartsRiseParameter : EffectShapeParameterBase
{
    public HeartsRiseParameter() : this(null) { }
    public HeartsRiseParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "種類")]
    [EnumComboBox]
    public ReactionKind Kind { get => kind; set => Set(ref kind, value); }
    ReactionKind kind = ReactionKind.Heart;

    [Display(GroupName = "形", Name = "色")]
    [EnumComboBox]
    public ReactionColor ColorMode { get => colorMode; set => Set(ref colorMode, value); }
    ReactionColor colorMode = ReactionColor.Pink;

    [Display(GroupName = "形", Name = "指定の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0x4D, 0x8D);

    [Display(GroupName = "形", Name = "大きさ")]
    [AnimationSlider("F0", "px", 10, 150)]
    public Animation Size { get; } = new Animation(48, 2, 1000);

    [Display(GroupName = "形", Name = "白いふち")]
    [ToggleSlider]
    public bool Outline { get => outline; set => Set(ref outline, value); }
    bool outline = true;

    [Display(GroupName = "出方", Name = "出てくる所X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SourceX { get; } = new Animation(88, -100, 200);

    [Display(GroupName = "出方", Name = "出てくる所Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SourceY { get; } = new Animation(92, -100, 200);

    [Display(GroupName = "出方", Name = "1秒に出る数")]
    [AnimationSlider("F1", "個", 0.5, 30)]
    public Animation Rate { get; } = new Animation(6, 0, 200);

    [Display(GroupName = "出方", Name = "のぼる高さ")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation Rise { get; } = new Animation(620, 0, 8000);

    [Display(GroupName = "出方", Name = "左右のゆれ")]
    [AnimationSlider("F0", "px", 0, 150)]
    public Animation Sway { get; } = new Animation(45, 0, 2000);

    [Display(GroupName = "出方", Name = "消えるまでの時間")]
    [AnimationSlider("F1", "秒", 0.5, 6)]
    public Animation Life { get; } = new Animation(2.6, 0.1, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Size, SourceX, SourceY, Rate, Rise, Sway, Life];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float rate = c.F(Rate), life = MathF.Max(0.1f, c.F(Life));
        if (rate <= 0.001f) return;
        var src = new Vector2(c.F(SourceX) / 100f * w, c.F(SourceY) / 100f * h);
        float size = c.F(Size), rise = c.F(Rise), sway = c.F(Sway);
        int first = Math.Max(0, (int)MathF.Floor((t - life) * rate));
        int last = (int)MathF.Floor(t * rate);
        using var brush = dc.CreateSolidColorBrush(default);
        for (int i = first; i <= last; i++)
        {
            // 出る時刻を少しばらつかせる
            float born = (i + DrawKit.H(Seed, i, 9) * 0.8f) / rate;
            float age = t - born;
            if (age < 0f || age > life) continue;
            float f = age / life;
            float pop = DrawKit.EaseOutBack(Math.Clamp(age / 0.25f, 0f, 1f));
            float sc = size * (0.7f + DrawKit.H(Seed, i, 1) * 0.6f) * pop;
            float phase = DrawKit.H(Seed, i, 2) * MathF.PI * 2f;
            var p = src + new Vector2(MathF.Sin(age * 3.2f + phase) * sway * (0.4f + f) + (DrawKit.H(Seed, i, 3) - 0.5f) * sway, -rise * (1f - MathF.Pow(1f - f, 1.6f)));
            float a = f < 0.7f ? 1f : 1f - (f - 0.7f) / 0.3f;
            float rot = MathF.Sin(age * 2.5f + phase) * 0.35f;
            WpfColor col = ColorMode switch
            {
                ReactionColor.Rainbow => Gfx.Hsv(DrawKit.H(Seed, i, 4) * 360f, 0.65f, 1f),
                ReactionColor.Custom => Color,
                _ => Gfx.Hsv(330f + (DrawKit.H(Seed, i, 4) - 0.5f) * 40f, 0.55f + DrawKit.H(Seed, i, 5) * 0.35f, 1f),
            };
            var kindNow = Kind == ReactionKind.Mixed ? (ReactionKind)(int)(DrawKit.H(Seed, i, 6) * 3f) : Kind;
            List<Vector2> shape = kindNow switch
            {
                ReactionKind.Thumb => ThumbKit.Thumb(p, sc * 0.5f, rot),
                ReactionKind.Star => ShapeGeom.StarPoly(p, sc * 0.55f, 5, 0.48f, rot),
                _ => ShapeGeom.HeartPoly(p, sc * 0.55f, rot),
            };
            if (Outline)
            {
                brush.Color = Gfx.C4(1f, 1f, 1f, a);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(shape), MathF.Max(2f, sc * 0.1f));
            }
            brush.Color = Gfx.C4(col, a);
            Gfx.FillPoly(dc, brush, shape);
            if (kindNow == ReactionKind.Thumb)
            {
                brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(0, 0, 0), 0.25f), a);
                Gfx.FillPoly(dc, brush, ThumbKit.Cuff(p, sc * 0.5f, rot));
            }
            // つやの点
            brush.Color = Gfx.C4(1f, 1f, 1f, 0.55f * a);
            dc.FillEllipse(new Ellipse(p + new Vector2(-sc * 0.17f, -sc * 0.14f), sc * 0.07f, sc * 0.05f), brush);
        }
    }
}
