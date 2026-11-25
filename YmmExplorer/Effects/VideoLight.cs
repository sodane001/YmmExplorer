using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin.Effects;
using WpfColor = System.Windows.Media.Color;

namespace YmmExplorer.Fx;

internal static class FxDraw
{
    /// <summary>楕円の放射グラデーションで塗る</summary>
    public static void Radial(ID2D1DeviceContext dc, Vector2 c, float rx, float ry, params (float pos, Color4 col)[] stops)
    {
        if (rx <= 0.5f || ry <= 0.5f) return;
        var gs = stops.Select(s => new GradientStop { Position = s.pos, Color = s.col }).ToArray();
        using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
        using var br = dc.CreateRadialGradientBrush(new RadialGradientBrushProperties
        {
            Center = c, GradientOriginOffset = Vector2.Zero, RadiusX = rx, RadiusY = ry,
        }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
        dc.FillEllipse(new Ellipse(c, rx, ry), br);
    }

    /// <summary>四角の範囲を、放射グラデーションで塗る(ビネット・スポットライト用)</summary>
    public static void RadialRect(ID2D1DeviceContext dc, RawRectF r, Vector2 c, float rx, float ry, params (float pos, Color4 col)[] stops)
    {
        var gs = stops.Select(s => new GradientStop { Position = s.pos, Color = s.col }).ToArray();
        using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
        using var br = dc.CreateRadialGradientBrush(new RadialGradientBrushProperties
        {
            Center = c, GradientOriginOffset = Vector2.Zero, RadiusX = MathF.Max(1f, rx), RadiusY = MathF.Max(1f, ry),
        }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
        dc.FillRectangle(r, br);
    }

    /// <summary>四角の範囲を、直線グラデーションで塗る</summary>
    public static void LinearRect(ID2D1DeviceContext dc, RawRectF r, Vector2 a, Vector2 b, params (float pos, Color4 col)[] stops)
    {
        var gs = stops.Select(s => new GradientStop { Position = s.pos, Color = s.col }).ToArray();
        using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
        using var br = dc.CreateLinearGradientBrush(new LinearGradientBrushProperties { StartPoint = a, EndPoint = b },
            new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
        dc.FillRectangle(r, br);
    }

    public static Color4 A(Color4 c, float a) => new(c.R, c.G, c.B, Math.Clamp(c.A * a, 0f, 1f));

    /// <summary>色相をずらした色(ゴーストの色づけ)</summary>
    public static Color4 Hue(Color4 c, float shift)
    {
        float r = c.R, g = c.G, b = c.B;
        float cos = MathF.Cos(shift), sin = MathF.Sin(shift);
        float nr = (0.299f + 0.701f * cos + 0.168f * sin) * r + (0.587f - 0.587f * cos + 0.330f * sin) * g + (0.114f - 0.114f * cos - 0.497f * sin) * b;
        float ng = (0.299f - 0.299f * cos - 0.328f * sin) * r + (0.587f + 0.413f * cos + 0.035f * sin) * g + (0.114f - 0.114f * cos + 0.292f * sin) * b;
        float nb = (0.299f - 0.300f * cos + 1.250f * sin) * r + (0.587f - 0.588f * cos - 1.050f * sin) * g + (0.114f + 0.886f * cos - 0.203f * sin) * b;
        return new Color4(Math.Clamp(nr, 0f, 1f), Math.Clamp(ng, 0f, 1f), Math.Clamp(nb, 0f, 1f), c.A);
    }
}

// =====================================================================================================
//  光・カメラ
// =====================================================================================================

[VideoEffect("レンズフレア", ["演出：光・カメラ"], ["レンズフレア", "lens flare", "光", "太陽", "逆光", "ゴースト", "きらめき", "演出"], IsAviUtlSupported = false)]
public sealed class LensFlareEffect : FxVideoEffectBase
{
    public override string Label => "レンズフレア";

    [Display(GroupName = "レンズフレア", Name = "光源の横位置", Description = "0%=左端、100%=右端")]
    [AnimationSlider("F0", "%", -50, 150)]
    public Animation X { get; } = new Animation(25, -500, 500);

    [Display(GroupName = "レンズフレア", Name = "光源の縦位置", Description = "0%=上端、100%=下端")]
    [AnimationSlider("F0", "%", -50, 150)]
    public Animation Y { get; } = new Animation(25, -500, 500);

    [Display(GroupName = "レンズフレア", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Strength { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "レンズフレア", Name = "大きさ")]
    [AnimationSlider("F0", "%", 10, 300)]
    public Animation Size { get; } = new Animation(100, 1, 2000);

    [Display(GroupName = "レンズフレア", Name = "光の筋(横)", Description = "光源から横にのびる筋の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Streak { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "レンズフレア", Name = "ゴーストの数", Description = "光源と画面の中心を結ぶ線上に出る、丸い光の数")]
    [TextBoxSlider("F0", "個", 0, 12)]
    [Range(0, 30)]
    public int Ghosts { get => ghosts; set => Set(ref ghosts, value); }
    int ghosts = 6;

    [Display(GroupName = "レンズフレア", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 222, 170);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [X, Y, Strength, Size, Streak];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var src = f.At(f.V(X) / 100f, f.V(Y) / 100f);
        float a = f.V(Strength) / 100f, size = f.V(Size) / 100f * MathF.Min(f.W, f.H), streak = f.V(Streak) / 100f;
        var col = FxKit.Rgb(color);
        var center = f.C;
        int n = ghosts;
        return k.Draw(f.B, dc =>
        {
            dc.DrawImage(input, InterpolationMode.Linear, CompositeMode.SourceOver);
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            // 光源の大きな光
            FxDraw.Radial(dc, src, size * 0.55f, size * 0.55f, (0, FxDraw.A(col, 0.9f * a)), (0.25f, FxDraw.A(col, 0.35f * a)), (1, FxDraw.A(col, 0)));
            FxDraw.Radial(dc, src, size * 0.12f, size * 0.12f, (0, new Color4(1, 1, 1, MathF.Min(1, a))), (1, new Color4(1, 1, 1, 0)));
            // 横の筋
            if (streak > 0)
                FxDraw.Radial(dc, src, size * 1.8f * streak, size * 0.025f + 1, (0, FxDraw.A(col, 0.8f * a)), (1, FxDraw.A(col, 0)));
            // 光の輪
            FxDraw.Radial(dc, src, size * 0.9f, size * 0.9f, (0.80f, FxDraw.A(col, 0)), (0.88f, FxDraw.A(col, 0.12f * a)), (1, FxDraw.A(col, 0)));
            // ゴースト(光源 → 中心 → 反対側)
            for (int i = 0; i < n; i++)
            {
                float t = 0.35f + 1.6f * i / Math.Max(1, n - 1) + (FxKit.Hash(i * 13 + 5) - 0.5f) * 0.15f;
                var p = src + (center - src) * t;
                float r = size * (0.04f + 0.16f * FxKit.Hash(i * 31 + 7));
                var gc = FxDraw.Hue(col, (i * 1.3f) % 6.28f);
                FxDraw.Radial(dc, p, r, r, (0, FxDraw.A(gc, 0.10f * a)), (0.75f, FxDraw.A(gc, 0.22f * a)), (1, FxDraw.A(gc, 0)));
            }
        });
    }
}

[VideoEffect("光芒（ゴッドレイ）", ["演出：光・カメラ"], ["光芒", "ゴッドレイ", "god ray", "光の筋", "天使の梯子", "後光", "光", "演出"], IsAviUtlSupported = false)]
public sealed class GodRaysEffect : FxVideoEffectBase
{
    public override string Label => "光芒（ゴッドレイ）";

    [Display(GroupName = "光芒", Name = "光源の横位置", Description = "0%=左端、100%=右端")]
    [AnimationSlider("F0", "%", -50, 150)]
    public Animation X { get; } = new Animation(50, -500, 500);

    [Display(GroupName = "光芒", Name = "光源の縦位置", Description = "0%=上端、100%=下端")]
    [AnimationSlider("F0", "%", -50, 150)]
    public Animation Y { get; } = new Animation(10, -500, 500);

    [Display(GroupName = "光芒", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Strength { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "光芒", Name = "長さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Length { get; } = new Animation(40, 0, 300);

    [Display(GroupName = "光芒", Name = "明るい所のしきい値", Description = "これより明るい所から光がのびます")]
    [AnimationSlider("F0", "%", 0, 95)]
    public Animation Threshold { get; } = new Animation(55, 0, 99);

    [Display(GroupName = "光芒", Name = "光の筋の本数", Description = "光源から放射状に描く筋(0で描かない)")]
    [TextBoxSlider("F0", "本", 0, 40)]
    [Range(0, 200)]
    public int Rays { get => rays; set => Set(ref rays, value); }
    int rays = 12;

    [Display(GroupName = "光芒", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 240, 200);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [X, Y, Strength, Length, Threshold];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var src = f.At(f.V(X) / 100f, f.V(Y) / 100f);
        float a = f.V(Strength) / 100f, len = f.V(Length) / 100f, th = f.V(Threshold) / 100f;
        var col = FxKit.Rgb(color);
        const int copies = 10;
        var bp = FxKit.BrightPass(th, 1.6f * a / copies);
        for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) bp[j * 4 + i] *= i switch { 0 => col.R, 1 => col.G, _ => col.B };
        bp[16] *= col.R; bp[17] *= col.G; bp[18] *= col.B;
        var bright = k.Matrix("bp", k.Blur("b0", input, 2f), bp);
        int nr = rays;
        float t = f.T, w = f.W, h = f.H;
        return k.Draw(f.B, dc =>
        {
            dc.DrawImage(input, InterpolationMode.Linear, CompositeMode.SourceOver);
            // 明るい所を、光源から外へ少しずつ大きくして重ねる(放射状のぼかし)
            for (int i = 1; i <= copies; i++)
            {
                float s = 1f + len * i / copies;
                dc.Transform = Matrix3x2.CreateScale(s, s, src);
                dc.DrawImage(bright, InterpolationMode.Linear, CompositeMode.Plus);
            }
            dc.Transform = Matrix3x2.Identity;
            if (nr > 0)
            {
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                float reach = MathF.Sqrt(w * w + h * h);
                using var br = dc.CreateSolidColorBrush(col);
                for (int i = 0; i < nr; i++)
                {
                    float ang = FxKit.Hash(i * 17 + 3) * MathF.PI * 2f + 0.15f * MathF.Sin(t * 0.5f + i);
                    float wid = (0.02f + 0.05f * FxKit.Hash(i * 29 + 1)) * reach * 0.25f;
                    float alpha = 0.10f * a * (0.5f + 0.5f * MathF.Sin(t * (0.8f + FxKit.Hash(i) ) + i * 2.1f));
                    var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                    var nrm = new Vector2(-dir.Y, dir.X);
                    var end = src + dir * reach * (0.4f + len);
                    YMM4EffectShapes.Gfx.FadeLine(dc, br, src, end, 1f, wid, color, MathF.Max(0, alpha), 10, 1.2f);
                }
            }
        });
    }
}

[VideoEffect("ブルーム（光のにじみ）", ["演出：光・カメラ"], ["ブルーム", "bloom", "グロー", "glow", "光のにじみ", "発光", "ソフトフォーカス", "演出"], IsAviUtlSupported = false)]
public sealed class BloomEffect : FxVideoEffectBase
{
    public override string Label => "ブルーム（光のにじみ）";

    [Display(GroupName = "ブルーム", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Strength { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "ブルーム", Name = "にじみの広さ")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation Radius { get; } = new Animation(18, 0, 500);

    [Display(GroupName = "ブルーム", Name = "明るい所のしきい値", Description = "これより明るい所だけ光らせます(0%で全体)")]
    [AnimationSlider("F0", "%", 0, 95)]
    public Animation Threshold { get; } = new Animation(60, 0, 99);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Strength, Radius, Threshold];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var bright = k.Matrix("bp", input, FxKit.BrightPass(f.V(Threshold) / 100f, f.V(Strength) / 100f));
        var glow = k.Blur("blur", bright, f.V(Radius));
        return k.Comp("add", input, glow, CompositeMode.Plus);
    }
}

[VideoEffect("色収差", ["演出：光・カメラ"], ["色収差", "chromatic aberration", "RGBずれ", "色ずれ", "にじみ", "レンズ", "演出"], IsAviUtlSupported = false)]
public sealed class ChromaticAberrationEffect : FxVideoEffectBase
{
    public override string Label => "色収差";

    [Display(GroupName = "色収差", Name = "ずれ(外側へ)", Description = "中心から外側ほど、赤と青が逆向きにずれます")]
    [AnimationSlider("F1", "%", 0, 5)]
    public Animation Radial { get; } = new Animation(1, 0, 50);

    [Display(GroupName = "色収差", Name = "ずれ(横)")]
    [AnimationSlider("F0", "px", -30, 30)]
    public Animation ShiftX { get; } = new Animation(0, -1000, 1000);

    [Display(GroupName = "色収差", Name = "ずれ(縦)")]
    [AnimationSlider("F0", "px", -30, 30)]
    public Animation ShiftY { get; } = new Animation(0, -1000, 1000);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Radial, ShiftX, ShiftY];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float rad = f.V(Radial) / 100f, dx = f.V(ShiftX), dy = f.V(ShiftY);
        var c = f.C;
        var r = k.Affine("tr", k.Matrix("r", input, FxKit.Channel(0)), Matrix3x2.CreateScale(1 + rad, 1 + rad, c) * Matrix3x2.CreateTranslation(dx, dy));
        var g = k.Matrix("g", input, FxKit.Channel(1));
        var b = k.Affine("tb", k.Matrix("b", input, FxKit.Channel(2)), Matrix3x2.CreateScale(1 - rad, 1 - rad, c) * Matrix3x2.CreateTranslation(-dx, -dy));
        return k.Comp("rgb", k.Comp("rg", r, g, CompositeMode.Plus), b, CompositeMode.Plus);
    }
}

[VideoEffect("ビネット（周辺減光）", ["演出：光・カメラ"], ["ビネット", "vignette", "周辺減光", "まわりを暗く", "シネマ", "レトロ", "演出"], IsAviUtlSupported = false)]
public sealed class VignetteEffect : FxVideoEffectBase
{
    public override string Label => "ビネット（周辺減光）";

    [Display(GroupName = "ビネット", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "ビネット", Name = "明るい範囲", Description = "まん中の、暗くしない範囲の大きさ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Inner { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "ビネット", Name = "柔らかさ")]
    [AnimationSlider("F0", "%", 1, 100)]
    public Animation Soft { get; } = new Animation(60, 1, 100);

    [Display(GroupName = "ビネット", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0, 0, 0);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Strength, Inner, Soft];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float s = f.V(Strength) / 100f, inner = f.V(Inner) / 100f, soft = f.V(Soft) / 100f;
        var col = FxKit.Rgb(color);
        var b = f.B; var c = f.C; float rx = f.W * 0.71f, ry = f.H * 0.71f;
        float p0 = Math.Clamp(inner, 0f, 0.98f), p1 = Math.Clamp(inner + soft, p0 + 0.01f, 1f);
        var shade = k.Draw(b, dc => FxDraw.RadialRect(dc, b, c, rx, ry, (0, FxDraw.A(col, 0)), (p0, FxDraw.A(col, 0)), (p1, FxDraw.A(col, s)), (1, FxDraw.A(col, s))));
        return k.Comp("atop", input, shade, CompositeMode.SourceAtop);
    }
}

public enum FocusShape
{
    [Display(Name = "丸(点にピント)")] Circle,
    [Display(Name = "横の帯(ティルトシフト)")] Band,
}

[VideoEffect("被写界深度風ぼかし", ["演出：光・カメラ"], ["被写界深度", "ボケ", "ピント", "背景ぼかし", "depth of field", "dof", "ぼかし", "演出"], IsAviUtlSupported = false)]
public sealed class DepthBlurEffect : FxVideoEffectBase
{
    public override string Label => "被写界深度風ぼかし";

    [Display(GroupName = "ピント", Name = "ピントの横位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation X { get; } = new Animation(50, -200, 300);

    [Display(GroupName = "ピント", Name = "ピントの縦位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Y { get; } = new Animation(50, -200, 300);

    [Display(GroupName = "ピント", Name = "ピントの合う範囲")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Range { get; } = new Animation(25, 0, 300);

    [Display(GroupName = "ピント", Name = "境目の柔らかさ")]
    [AnimationSlider("F0", "%", 1, 100)]
    public Animation Soft { get; } = new Animation(30, 1, 300);

    [Display(GroupName = "ピント", Name = "ぼかしの強さ")]
    [AnimationSlider("F0", "px", 0, 50)]
    public Animation Blur { get; } = new Animation(12, 0, 500);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [X, Y, Range, Soft, Blur];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
        => FocusBlur.Make(k, input, f, FocusShape.Circle, f.V(X) / 100f, f.V(Y) / 100f, f.V(Range) / 100f, f.V(Soft) / 100f, f.V(Blur), 1f);
}

[VideoEffect("ティルトシフト（ミニチュア風）", ["演出：光・カメラ"], ["ティルトシフト", "tilt shift", "ミニチュア", "ジオラマ", "ぼかし", "演出"], IsAviUtlSupported = false)]
public sealed class TiltShiftEffect : FxVideoEffectBase
{
    public override string Label => "ティルトシフト（ミニチュア風）";

    [Display(GroupName = "ティルトシフト", Name = "ピントの縦位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Y { get; } = new Animation(55, -200, 300);

    [Display(GroupName = "ティルトシフト", Name = "ピントの合う幅")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Range { get; } = new Animation(18, 0, 300);

    [Display(GroupName = "ティルトシフト", Name = "境目の柔らかさ")]
    [AnimationSlider("F0", "%", 1, 100)]
    public Animation Soft { get; } = new Animation(25, 1, 300);

    [Display(GroupName = "ティルトシフト", Name = "ぼかしの強さ")]
    [AnimationSlider("F0", "px", 0, 50)]
    public Animation Blur { get; } = new Animation(10, 0, 500);

    [Display(GroupName = "ティルトシフト", Name = "鮮やかさ", Description = "おもちゃっぽく見えるように、色を鮮やかにします")]
    [AnimationSlider("F0", "%", 50, 200)]
    public Animation Saturation { get; } = new Animation(135, 0, 400);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Y, Range, Soft, Blur, Saturation];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var toned = k.Matrix("tone", input, FxKit.Tone(0f, 1.08f, f.V(Saturation) / 100f));
        return FocusBlur.Make(k, toned, f, FocusShape.Band, 0.5f, f.V(Y) / 100f, f.V(Range) / 100f, f.V(Soft) / 100f, f.V(Blur), 1f);
    }
}

internal static class FocusBlur
{
    /// <summary>ピントの合う所はそのまま、外側をぼかす</summary>
    public static ID2D1Image Make(FxKit k, ID2D1Image input, in FxFrame f, FocusShape shape, float px, float py, float range, float soft, float blur, float _)
    {
        var blurred = k.Blur("fb_blur", input, blur);
        var b = f.B; var c = f.At(px, py); float w = f.W, h = f.H;
        ID2D1Image mask;
        if (shape == FocusShape.Circle)
        {
            float r = MathF.Max(1f, MathF.Min(w, h) * (range + soft));
            float p0 = Math.Clamp(range / MathF.Max(0.001f, range + soft), 0f, 0.99f);
            mask = k.Draw(b, dc => FxDraw.RadialRect(dc, b, c, r, r, (0, new Color4(1, 1, 1, 1)), (p0, new Color4(1, 1, 1, 1)), (1, new Color4(1, 1, 1, 0))));
        }
        else
        {
            float half = h * range / 2f, fade = MathF.Max(1f, h * soft);
            float y0 = c.Y - half - fade, y1 = c.Y + half + fade, span = MathF.Max(1f, y1 - y0);
            float a = fade / span, z = 1f - a;
            mask = k.Draw(b, dc => FxDraw.LinearRect(dc, b, new Vector2(c.X, y0), new Vector2(c.X, y1),
                (0, new Color4(1, 1, 1, 0)), (a, new Color4(1, 1, 1, 1)), (z, new Color4(1, 1, 1, 1)), (1, new Color4(1, 1, 1, 0))));
        }
        var sharp = k.Comp("fb_mask", input, mask, CompositeMode.DestinationIn);
        return k.Comp("fb_over", blurred, sharp, CompositeMode.SourceOver);
    }
}

[VideoEffect("フィルムグレイン", ["演出：光・カメラ"], ["フィルムグレイン", "film grain", "粒子", "ざらざら", "ノイズ", "フィルム", "映画", "レトロ", "演出"], IsAviUtlSupported = false)]
public sealed class FilmGrainEffect : FxVideoEffectBase
{
    public override string Label => "フィルムグレイン";

    [Display(GroupName = "フィルムグレイン", Name = "粒の強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(20, 0, 100);

    [Display(GroupName = "フィルムグレイン", Name = "粒の大きさ")]
    [AnimationSlider("F1", "px", 0.5, 6)]
    public Animation GrainSize { get; } = new Animation(1.2, 0.2, 50);

    [Display(GroupName = "フィルムグレイン", Name = "明るさのちらつき")]
    [AnimationSlider("F0", "%", 0, 30)]
    public Animation Flicker { get; } = new Animation(4, 0, 100);

    [Display(GroupName = "フィルムグレイン", Name = "色あせ", Description = "古いフィルムのように、色を少し薄く黄ばませます")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Fade { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Strength, GrainSize, Flicker, Fade];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float s = f.V(Strength) / 100f, size = MathF.Max(0.2f, f.V(GrainSize)), fl = f.V(Flicker) / 100f, fade = f.V(Fade) / 100f;
        int fr = (int)f.Frame;
        float bright = (FxKit.Hash(fr * 3 + 11) - 0.5f) * fl;
        var toned = k.Matrix("tone", input, FxKit.Tone(bright + fade * 0.06f, 1f - fade * 0.25f, 1f - fade * 0.45f));
        if (fade > 0)
        {
            var warm = FxKit.Tone(0, 1, 1);
            warm[18] -= fade * 0.06f; warm[16] += fade * 0.03f;   // 少し黄色っぽく
            toned = k.Matrix("warm", toned, warm);
        }
        if (s <= 0.001f) return toned;
        // ノイズを白黒にして、薄く重ねる
        var noise = k.Turb("turb", new Vector2(FxKit.Hash(fr) * 1000f, FxKit.Hash(fr + 7) * 1000f), 0.9f / size, 1, fr % 1000);
        var gray = k.Matrix("gray", noise, new float[] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, s * 0.6f });
        var clipped = k.Draw(f.B, dc => dc.DrawImage(gray, InterpolationMode.Linear, CompositeMode.SourceOver));
        var grained = k.Comp("atop", toned, clipped, CompositeMode.SourceAtop);
        // 灰色を重ねた分、コントラストを戻す
        return k.Matrix("restore", grained, FxKit.Tone(0, 1f / (1f - s * 0.45f), 1));
    }
}

[VideoEffect("光漏れ（ライトリーク）", ["演出：光・カメラ"], ["光漏れ", "ライトリーク", "light leak", "フィルム", "エモい", "レトロ", "暖色", "演出"], IsAviUtlSupported = false)]
public sealed class LightLeakEffect : FxVideoEffectBase
{
    public override string Label => "光漏れ（ライトリーク）";

    [Display(GroupName = "光漏れ", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Strength { get; } = new Animation(70, 0, 1000);

    [Display(GroupName = "光漏れ", Name = "動く速さ")]
    [AnimationSlider("F2", "", 0, 2)]
    public Animation Speed { get; } = new Animation(0.3, 0, 20);

    [Display(GroupName = "光漏れ", Name = "色1")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(255, 120, 40);

    [Display(GroupName = "光漏れ", Name = "色2")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(255, 60, 110);

    [Display(GroupName = "光漏れ", Name = "シード", Description = "変えると光の出方が変わります")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Strength, Speed];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float s = f.V(Strength) / 100f, t = f.T * f.V(Speed);
        var cols = new[] { FxKit.Rgb(c1), FxKit.Rgb(c2), FxKit.Rgb(WpfColor.FromRgb(255, 210, 120)) };
        var fr = f; int sd = seed;
        var leak = k.Draw(f.B, dc =>
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            for (int i = 0; i < 3; i++)
            {
                // 画面の端のほうを、ゆっくり動き回る光
                float ex = FxKit.Hash(sd * 11 + i) < 0.5f ? 0.0f : 1.0f;
                float px = ex + FxKit.Noise(t + i * 3, sd + i) * 0.35f;
                float py = 0.5f + FxKit.Noise(t * 0.8f + i * 5, sd + i + 50) * 0.6f;
                var p = fr.At(px, py);
                float r = MathF.Max(fr.W, fr.H) * (0.35f + 0.25f * (0.5f + 0.5f * FxKit.Noise(t * 0.6f + i, sd + i + 90)));
                float a = s * 0.55f * (0.6f + 0.4f * FxKit.Noise(t * 1.3f + i * 7, sd + i + 130));
                FxDraw.Radial(dc, p, r, r * 1.2f, (0, FxDraw.A(cols[i], MathF.Max(0, a))), (1, FxDraw.A(cols[i], 0)));
            }
        });
        var masked = k.Comp("mask", leak, input, CompositeMode.DestinationIn);
        return k.Comp("add", input, masked, CompositeMode.Plus);
    }
}
