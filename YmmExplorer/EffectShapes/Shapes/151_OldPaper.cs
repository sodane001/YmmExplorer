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

public enum PaperKind
{
    [Display(Name = "紙の質感（うすく重ねる）")] Paper,
    [Display(Name = "古い写真（白いふち）")] OldPhoto,
    [Display(Name = "焼けた紙（ふちが焦げる）")] Burnt,
}

// =====================================================================
//  紙の質感・古い写真
//   画面に重ねて、セピアの色・しみ・折り目・角の破れ・ふちを加える
// =====================================================================
public sealed class OldPaperPlugin : IShapePlugin
{
    public string Name => "演出：紙の質感・古い写真";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new OldPaperParameter(sharedData);
}

public sealed class OldPaperParameter : EffectShapeParameterBase
{
    public OldPaperParameter() : this(null) { }
    public OldPaperParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public PaperKind Kind { get => kind; set => Set(ref kind, value); }
    PaperKind kind = PaperKind.OldPhoto;

    [Display(GroupName = "色", Name = "紙の色（セピア）")]
    [ColorPicker]
    public WpfColor Tint { get => tint; set => Set(ref tint, value); }
    WpfColor tint = WpfColor.FromRgb(0xC8, 0xA2, 0x6A);

    [Display(GroupName = "色", Name = "色の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation TintStrength { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "色", Name = "ふちの色（古い写真）")]
    [ColorPicker]
    public WpfColor BorderColor { get => borderColor; set => Set(ref borderColor, value); }
    WpfColor borderColor = WpfColor.FromRgb(0xF1, 0xEA, 0xDA);

    [Display(GroupName = "色", Name = "ふちの太さ")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Border { get; } = new Animation(48, 0, 2000);

    [Display(GroupName = "よごれ", Name = "しみの数")]
    [TextBoxSlider("F0", "個", 0, 60)]
    [Range(0, 300)]
    public int Stains { get => stains; set => Set(ref stains, value); }
    int stains = 14;

    [Display(GroupName = "よごれ", Name = "折り目")]
    [ToggleSlider]
    public bool Folds { get => folds; set => Set(ref folds, value); }
    bool folds = true;

    [Display(GroupName = "よごれ", Name = "角の破れ")]
    [ToggleSlider]
    public bool TornCorner { get => tornCorner; set => Set(ref tornCorner, value); }
    bool tornCorner = true;

    [Display(GroupName = "よごれ", Name = "破れた所から見える色")]
    [ColorPicker]
    public WpfColor BackColor { get => backColor; set => Set(ref backColor, value); }
    WpfColor backColor = WpfColor.FromRgb(0x2A, 0x22, 0x1A);

    [Display(GroupName = "よごれ", Name = "ざらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Grain { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "よごれ", Name = "古いフィルムの傷（ちらつく）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Scratches { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [TintStrength, Border, Grain, Scratches];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        int frame = (int)c.Frame;
        using var brush = dc.CreateSolidColorBrush(default);
        var dark = Gfx.Mix(Tint, WpfColor.FromRgb(0x40, 0x28, 0x10), 0.6f);

        // セピアの色を、うすく全体にかける
        float ts = c.F(TintStrength) / 100f;
        if (ts > 0.002f)
        {
            brush.Color = Gfx.C4(Tint, 0.45f * ts);
            dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
        }
        // ふちに向かって、茶色く焼ける
        float rad = MathF.Sqrt(w * w + h * h) / 2f;
        using (var g = Gfx.Radial(dc, new Vector2(w / 2f, h / 2f), rad, (0f, Gfx.C4(dark, 0f)), (0.6f, Gfx.C4(dark, 0f)),
            (1f, Gfx.C4(dark, Kind == PaperKind.Burnt ? 0.95f : 0.55f))))
            dc.FillRectangle(new RawRectF(0, 0, w, h), g);

        // しみ(いびつな、うすい丸の重なり)
        for (int i = 0; i < Stains; i++)
        {
            var p = new Vector2(DrawKit.H(Seed, i, 1) * w, DrawKit.H(Seed, i, 2) * h);
            float r = MathF.Min(w, h) * (0.02f + MathF.Pow(DrawKit.H(Seed, i, 3), 2f) * 0.1f);
            for (int k = 0; k < 3; k++)
            {
                var q = p + new Vector2(DrawKit.H(Seed, i, 4 + k) - 0.5f, DrawKit.H(Seed, i, 8 + k) - 0.5f) * r;
                using var g = Gfx.Radial(dc, q, r * (0.6f + k * 0.2f), (0f, Gfx.C4(dark, 0.08f)), (0.8f, Gfx.C4(dark, 0.12f)), (1f, Gfx.C4(dark, 0f)));
                dc.FillEllipse(new Ellipse(q, r * (0.6f + k * 0.2f), r * (0.6f + k * 0.2f)), g);
            }
        }

        // 折り目(縦・横に1本ずつ。明るい線と暗い線を並べて、へこみに見せる)
        if (Folds)
        {
            float fx = w * (0.45f + DrawKit.H(Seed, 90) * 0.15f), fy = h * (0.4f + DrawKit.H(Seed, 91) * 0.2f);
            float lw = MathF.Max(1.5f, MathF.Min(w, h) * 0.003f);
            brush.Color = Gfx.C4(dark, 0.35f);
            dc.DrawLine(new Vector2(fx, 0), new Vector2(fx + w * 0.01f, h), brush, lw);
            dc.DrawLine(new Vector2(0, fy), new Vector2(w, fy - h * 0.008f), brush, lw);
            brush.Color = Gfx.C4(1f, 1f, 0.95f, 0.25f);
            dc.DrawLine(new Vector2(fx + lw * 1.5f, 0), new Vector2(fx + w * 0.01f + lw * 1.5f, h), brush, lw);
            dc.DrawLine(new Vector2(0, fy + lw * 1.5f), new Vector2(w, fy - h * 0.008f + lw * 1.5f), brush, lw);
        }

        // ざらつき
        float grain = c.F(Grain) / 100f;
        if (grain > 0.002f)
        {
            int n = (int)(grain * 1500f);
            for (int i = 0; i < n; i++)
            {
                var p = new Vector2(DrawKit.H(Seed + frame % 3, i, 20) * w, DrawKit.H(Seed + frame % 3, i, 21) * h);
                float v = DrawKit.H(Seed, i, 22);
                brush.Color = v < 0.5f ? Gfx.C4(dark, 0.18f * grain) : Gfx.C4(1f, 1f, 0.95f, 0.15f * grain);
                dc.FillRectangle(new RawRectF(p.X, p.Y, p.X + 1.6f, p.Y + 1.6f), brush);
            }
        }

        // 古いフィルムの傷(縦線が、ちらちら場所を変える)
        float scr = c.F(Scratches) / 100f;
        if (scr > 0.002f)
        {
            int n = 1 + (int)(scr * 5f);
            for (int i = 0; i < n; i++)
            {
                if (DrawKit.H(frame / 2, i, 30) > 0.6f) continue;
                float x = DrawKit.H(frame / 2, i, 31) * w;
                brush.Color = Gfx.C4(1f, 1f, 0.95f, 0.35f * scr);
                dc.DrawLine(new Vector2(x, 0), new Vector2(x + (DrawKit.H(frame / 2, i, 32) - 0.5f) * 20f, h), brush, 1.2f + DrawKit.H(frame / 2, i, 33) * 1.5f);
            }
            if (DrawKit.H(frame, 0, 34) < 0.2f * scr)
            {
                // ときどき全体がちらつく
                brush.Color = Gfx.C4(1f, 1f, 0.95f, 0.06f);
                dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            }
        }

        // 古い写真の白いふち / 焦げたふち
        float b = c.F(Border);
        if (Kind == PaperKind.OldPhoto && b > 0.5f)
        {
            var outer = DrawKit.RectPts(0, 0, w, h);
            var inner = DrawKit.RectPts(b, b, w - b * 2f, h - b * 2f);
            brush.Color = Gfx.C4(BorderColor, 1f);
            DrawKit.FillWithHole(dc, brush, outer, inner);
            // ふちの内側の細い影
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f);
            dc.DrawRectangle(new RawRectF(b, b, w - b, h - b), brush, MathF.Max(1f, b * 0.04f));
        }
        if (Kind == PaperKind.Burnt)
        {
            // ふちのギザギザの焦げ
            var pts = new List<Vector2>();
            int n = 160;
            for (int i = 0; i < n; i++)
            {
                float f = i / (float)n;
                Vector2 p;
                float per = 2f * (w + h);
                float d = f * per;
                if (d < w) p = new Vector2(d, 0);
                else if (d < w + h) p = new Vector2(w, d - w);
                else if (d < 2 * w + h) p = new Vector2(w - (d - w - h), h);
                else p = new Vector2(0, h - (d - 2 * w - h));
                var inward = Vector2.Normalize(new Vector2(w / 2f, h / 2f) - p);
                float depth = MathF.Min(w, h) * (0.02f + DrawKit.H(Seed, i, 40) * 0.04f);
                pts.Add(p + inward * depth);
            }
            brush.Color = Gfx.C4(0.08f, 0.05f, 0.02f, 0.95f);
            DrawKit.FillWithHole(dc, brush, DrawKit.RectPts(-2, -2, w + 4, h + 4), pts);
            brush.Color = Gfx.C4(0.75f, 0.35f, 0.08f, 0.6f);
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(pts), MathF.Max(1.5f, MathF.Min(w, h) * 0.003f));
        }

        // 角の破れ(右下)
        if (TornCorner)
        {
            float s = MathF.Min(w, h) * 0.12f;
            var tear = new List<Vector2> { new(w, h - s) };
            for (int i = 1; i < 10; i++)
            {
                float f = i / 10f;
                var p = Vector2.Lerp(new Vector2(w, h - s), new Vector2(w - s * 1.2f, h), f);
                p += new Vector2(DrawKit.H(Seed, i, 50) - 0.5f, DrawKit.H(Seed, i, 51) - 0.5f) * s * 0.12f;
                tear.Add(p);
            }
            tear.Add(new Vector2(w - s * 1.2f, h));
            tear.Add(new Vector2(w, h));
            brush.Color = Gfx.C4(BackColor, 1f);
            Gfx.FillPoly(dc, brush, tear);
            // 破れたふちの、白い紙の芯
            brush.Color = Gfx.C4(1f, 0.98f, 0.92f, 0.85f);
            Gfx.Polyline(dc, brush, tear.Take(tear.Count - 1).ToList(), MathF.Max(1.5f, s * 0.03f));
        }
    }
}
