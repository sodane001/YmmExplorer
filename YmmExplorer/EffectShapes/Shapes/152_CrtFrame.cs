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

public enum CrtKind
{
    [Display(Name = "ビネット（四隅を暗く）だけ")] Vignette,
    [Display(Name = "ブラウン管テレビ")] Crt,
    [Display(Name = "ブラウン管＋ビネット")] Both,
}

// =====================================================================
//  ブラウン管の枠・ビネット(周辺減光)
//   画面の四隅を暗くする。ブラウン管テレビの枠・丸い画面・走査線・ガラスの反射も出せる
// =====================================================================
public sealed class CrtFramePlugin : IShapePlugin
{
    public string Name => "演出：ブラウン管の枠・ビネット";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CrtFrameParameter(sharedData);
}

public sealed class CrtFrameParameter : EffectShapeParameterBase
{
    public CrtFrameParameter() : this(null) { }
    public CrtFrameParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public CrtKind Kind { get => kind; set => Set(ref kind, value); }
    CrtKind kind = CrtKind.Vignette;

    [Display(GroupName = "ビネット", Name = "暗さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation VignetteStrength { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "ビネット", Name = "明るく残す範囲")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation VignetteSize { get; } = new Animation(55, 0, 100);

    [Display(GroupName = "ビネット", Name = "色")]
    [ColorPicker]
    public WpfColor VignetteColor { get => vignetteColor; set => Set(ref vignetteColor, value); }
    WpfColor vignetteColor = WpfColor.FromRgb(0, 0, 0);

    [Display(GroupName = "ブラウン管", Name = "枠の色")]
    [ColorPicker]
    public WpfColor BezelColor { get => bezelColor; set => Set(ref bezelColor, value); }
    WpfColor bezelColor = WpfColor.FromRgb(0x3A, 0x33, 0x2C);

    [Display(GroupName = "ブラウン管", Name = "枠の太さ")]
    [AnimationSlider("F0", "%", 0, 20)]
    public Animation BezelSize { get; } = new Animation(6, 0, 40);

    [Display(GroupName = "ブラウン管", Name = "画面の角の丸み")]
    [AnimationSlider("F0", "%", 0, 30)]
    public Animation Roundness { get; } = new Animation(9, 0, 50);

    [Display(GroupName = "ブラウン管", Name = "走査線")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Scanlines { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "ブラウン管", Name = "ガラスの反射")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Glare { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "ブラウン管", Name = "ちらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(15, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [VignetteStrength, VignetteSize, BezelSize, Roundness, Scanlines, Glare, Flicker];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(w / 2f, h / 2f);
        using var brush = dc.CreateSolidColorBrush(default);
        bool crt = Kind != CrtKind.Vignette;
        bool vig = Kind != CrtKind.Crt;

        float bez = crt ? c.F(BezelSize) / 100f * MathF.Min(w, h) : 0f;
        float sx0 = bez, sy0 = bez, sx1 = w - bez, sy1 = h - bez;
        float sw = sx1 - sx0, sh = sy1 - sy0;

        if (crt)
        {
            // 走査線とちらつき(画面の中だけ)
            float scan = c.F(Scanlines) / 100f;
            if (scan > 0.002f)
            {
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.4f * scan);
                float step = MathF.Max(3f, h / 300f);
                for (float y = sy0; y < sy1; y += step)
                    dc.FillRectangle(new RawRectF(sx0, y, sx1, y + step * 0.4f), brush);
            }
            float fl = c.F(Flicker) / 100f;
            if (fl > 0.002f)
            {
                float v = 0.5f + 0.5f * MathF.Sin(t * 60f) * MathF.Sin(t * 7.3f);
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.05f * fl * v);
                dc.FillRectangle(new RawRectF(sx0, sy0, sx1, sy1), brush);
            }
        }

        // ビネット: 画面の形に合わせた、横長のぼかし
        if (vig || crt)
        {
            float strength = vig ? c.F(VignetteStrength) / 100f : 0.5f;
            float keep = Math.Clamp(c.F(VignetteSize) / 100f, 0f, 0.98f);
            var center = new Vector2(sx0 + sw / 2f, sy0 + sh / 2f);
            float r = MathF.Sqrt(sw * sw + sh * sh) / 2f;
            // 円のぼかしを、横に引きのばして、画面の縦横比に合わせる
            var prev = dc.Transform;
            dc.Transform = Matrix3x2.CreateScale(1f, sh / sw, center) * prev;
            using (var g = Gfx.Radial(dc, center, r * (sw / MathF.Sqrt(sw * sw + sh * sh)) * 1.42f,
                (0f, Gfx.C4(VignetteColor, 0f)), (keep, Gfx.C4(VignetteColor, 0f)), (1f, Gfx.C4(VignetteColor, MathF.Min(1f, strength)))))
                dc.FillRectangle(new RawRectF(center.X - sw, center.Y - sw, center.X + sw, center.Y + sw), g);
            dc.Transform = prev;
        }

        if (crt)
        {
            // ガラスの反射(左上の、ななめの白いかすみ)
            float gl = c.F(Glare) / 100f;
            if (gl > 0.002f)
            {
                using var g = Gfx.Linear(dc, new Vector2(sx0, sy0), new Vector2(sx0 + sw * 0.6f, sy0 + sh * 0.6f),
                    (0f, Gfx.C4(1f, 1f, 1f, 0.16f * gl)), (0.45f, Gfx.C4(1f, 1f, 1f, 0.05f * gl)), (1f, Gfx.C4(1f, 1f, 1f, 0f)));
                Gfx.FillPoly(dc, g, new List<Vector2> { new(sx0, sy0), new(sx0 + sw * 0.75f, sy0), new(sx0, sy0 + sh * 0.65f) });
            }
            // 枠: 外は四角、内は角の丸い画面(少しふくらんだ形)
            float round = c.F(Roundness) / 100f * MathF.Min(sw, sh);
            var inner = ShapeGeom.RoundRect(sx0 + sw / 2f, sy0 + sh / 2f, sw, sh, round);
            var outer = DrawKit.RectPts(-2f, -2f, w + 4f, h + 4f);
            brush.Color = Gfx.C4(BezelColor, 1f);
            DrawKit.FillWithHole(dc, brush, outer, inner);
            // 枠の内側の、影と光
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.55f);
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(inner), MathF.Max(2f, bez * 0.12f));
            brush.Color = Gfx.C4(Gfx.Mix(BezelColor, WpfColor.FromRgb(255, 255, 255), 0.25f), 0.6f);
            var outline = ShapeGeom.RoundRect(sx0 + sw / 2f, sy0 + sh / 2f, sw + bez * 0.5f, sh + bez * 0.5f, round + bez * 0.25f);
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(outline), MathF.Max(1.5f, bez * 0.05f));
            // 電源ランプ
            if (bez > 8f)
            {
                var lamp = new Vector2(w - bez * 1.6f, h - bez * 0.5f);
                brush.Color = Gfx.C4(0.3f, 1f, 0.4f, 0.9f);
                dc.FillEllipse(new Ellipse(lamp, bez * 0.08f, bez * 0.08f), brush);
                DrawKit.Glow(dc, lamp, bez * 0.3f, Gfx.C4(0.3f, 1f, 0.4f, 0.4f));
            }
        }
    }
}
