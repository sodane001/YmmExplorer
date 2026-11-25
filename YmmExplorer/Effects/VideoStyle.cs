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

// =====================================================================================================
//  質感・画風
// =====================================================================================================

[VideoEffect("アニメ調（トゥーン化）", ["演出：質感・画風"], ["アニメ調", "トゥーン", "toon", "セル画", "輪郭線", "縁取り", "イラスト風", "演出"], IsAviUtlSupported = false)]
public sealed class ToonEffect : FxVideoEffectBase
{
    public override string Label => "アニメ調（トゥーン化）";

    [Display(GroupName = "アニメ調", Name = "鮮やかさ")]
    [AnimationSlider("F0", "%", 50, 250)]
    public Animation Saturation { get; } = new Animation(145, 0, 500);

    [Display(GroupName = "アニメ調", Name = "くっきりさ(コントラスト)")]
    [AnimationSlider("F0", "%", 50, 250)]
    public Animation Contrast { get; } = new Animation(125, 0, 500);

    [Display(GroupName = "アニメ調", Name = "輪郭線の太さ", Description = "形のまわりに付ける線(0で付けない)")]
    [AnimationSlider("F1", "px", 0, 20)]
    public Animation LineWidth { get; } = new Animation(4, 0, 200);

    [Display(GroupName = "アニメ調", Name = "輪郭線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(30, 24, 36);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Saturation, Contrast, LineWidth];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var toned = k.Matrix("tone", input, FxKit.Tone(0.02f, f.V(Contrast) / 100f, f.V(Saturation) / 100f));
        float lw = f.V(LineWidth);
        if (lw <= 0.05f) return toned;
        var line = k.Outline("line", input, lw, FxKit.Rgb(lineColor));
        return k.Comp("over", line, toned, CompositeMode.SourceOver);
    }
}

[VideoEffect("鉛筆・スケッチ風", ["演出：質感・画風"], ["鉛筆", "スケッチ", "sketch", "デッサン", "線画", "モノクロ", "手描き", "ハッチング", "演出"], IsAviUtlSupported = false)]
public sealed class PencilSketchEffect : FxVideoEffectBase
{
    public override string Label => "鉛筆・スケッチ風";

    [Display(GroupName = "スケッチ", Name = "白っぽさ", Description = "紙に描いたように、全体を明るく薄くします")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Light { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "スケッチ", Name = "斜線(ハッチング)の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Hatch { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "スケッチ", Name = "斜線の間隔")]
    [AnimationSlider("F0", "px", 3, 30)]
    public Animation Spacing { get; } = new Animation(7, 2, 200);

    [Display(GroupName = "スケッチ", Name = "輪郭線の太さ")]
    [AnimationSlider("F1", "px", 0, 10)]
    public Animation LineWidth { get; } = new Animation(2, 0, 100);

    [Display(GroupName = "スケッチ", Name = "色を少し残す")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation KeepColor { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Light, Hatch, Spacing, LineWidth, KeepColor];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float light = f.V(Light) / 100f, hatch = f.V(Hatch) / 100f, sp = MathF.Max(2f, f.V(Spacing));
        var gray = k.Matrix("gray", input, FxKit.Tone(light * 0.45f, 1.5f - light * 0.4f, f.V(KeepColor) / 100f));
        var b = f.B;
        // 斜線の1マス(右上がりの線)
        var cellRect = new RawRectF(0, 0, sp, sp);
        var cell = k.Draw(cellRect, dc =>
        {
            using var br = dc.CreateSolidColorBrush(new Color4(0.15f, 0.15f, 0.18f, 1f));
            dc.DrawLine(new Vector2(-1, sp + 1), new Vector2(sp + 1, -1), br, 1.1f);
        }, clip: false);
        // 暗い所ほど斜線が濃く見えるように、暗さで斜線を切り抜く
        var dark = k.Matrix("dark", input, new float[] { 0, 0, 0, -0.2126f, 0, 0, 0, -0.7152f, 0, 0, 0, -0.0722f, 0, 0, 0, 0, 0, 0, 0, 1f });
        var lines = k.Draw(b, dc => k.Tile(dc, cell, cellRect, b, Vector2.Zero, hatch));
        var hatchMasked = k.Comp("hm", lines, dark, CompositeMode.DestinationIn);
        var drawn = k.Comp("atop", gray, hatchMasked, CompositeMode.SourceAtop);
        float lw = f.V(LineWidth);
        if (lw <= 0.05f) return drawn;
        var line = k.Outline("line", input, lw, new Color4(0.2f, 0.2f, 0.24f, 0.85f));
        return k.Comp("over", line, drawn, CompositeMode.SourceOver);
    }
}

[VideoEffect("水彩風", ["演出：質感・画風"], ["水彩", "watercolor", "絵の具", "にじみ", "やわらか", "イラスト風", "絵本", "演出"], IsAviUtlSupported = false)]
public sealed class WatercolorEffect : FxVideoEffectBase
{
    public override string Label => "水彩風";

    [Display(GroupName = "水彩", Name = "にじみ")]
    [AnimationSlider("F0", "px", 0, 30)]
    public Animation Bleed { get; } = new Animation(6, 0, 300);

    [Display(GroupName = "水彩", Name = "やわらかさ(ぼかし)")]
    [AnimationSlider("F1", "px", 0, 10)]
    public Animation Soft { get; } = new Animation(2, 0, 100);

    [Display(GroupName = "水彩", Name = "紙の質感")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Paper { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "水彩", Name = "明るさ")]
    [AnimationSlider("F0", "%", -50, 50)]
    public Animation Bright { get; } = new Animation(8, -100, 100);

    [Display(GroupName = "水彩", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Bleed, Soft, Paper, Bright];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var soft = k.Blur("soft", input, f.V(Soft));
        var toned = k.Matrix("tone", soft, FxKit.Tone(f.V(Bright) / 100f, 0.92f, 1.15f));
        var map = k.Turb("tmap", Vector2.Zero, 0.02f, 3, seed);
        var bled = k.Displace("bleed", toned, map, f.V(Bleed));
        float paper = f.V(Paper) / 100f;
        if (paper <= 0.001f) return bled;
        var tex = k.Turb("tpaper", new Vector2(37, 91), 0.18f, 3, seed + 7);
        var gray = k.Matrix("pgray", tex, new float[] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, paper * 0.35f });
        var clipped = k.Draw(f.B, dc => dc.DrawImage(gray, InterpolationMode.Linear, CompositeMode.SourceOver));
        return k.Comp("atop", bled, clipped, CompositeMode.SourceAtop);
    }
}

[VideoEffect("ドット絵化（モザイク）", ["演出：質感・画風"], ["ドット絵", "ピクセル", "pixel", "モザイク", "mosaic", "レトロゲーム", "8bit", "演出"], IsAviUtlSupported = false)]
public sealed class PixelateEffect : FxVideoEffectBase
{
    public override string Label => "ドット絵化（モザイク）";

    [Display(GroupName = "ドット絵", Name = "ドットの大きさ")]
    [AnimationSlider("F0", "px", 1, 64)]
    public Animation Size { get; } = new Animation(8, 1, 1000);

    [Display(GroupName = "ドット絵", Name = "鮮やかさ")]
    [AnimationSlider("F0", "%", 50, 200)]
    public Animation Saturation { get; } = new Animation(115, 0, 400);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Size, Saturation];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float s = MathF.Max(1f, f.V(Size));
        var toned = k.Matrix("tone", input, FxKit.Tone(0, 1.05f, f.V(Saturation) / 100f));
        if (s <= 1.01f) return toned;
        var c = f.C;
        var down = k.Affine("down", toned, Matrix3x2.CreateScale(1f / s, 1f / s, c), nearest: false);
        return k.Affine("up", down, Matrix3x2.CreateScale(s, s, c), nearest: true);
    }
}

[VideoEffect("ハーフトーン（漫画の網点）", ["演出：質感・画風"], ["ハーフトーン", "halftone", "網点", "スクリーントーン", "トーン", "漫画", "マンガ", "アメコミ", "演出"], IsAviUtlSupported = false)]
public sealed class HalftoneEffect : FxVideoEffectBase
{
    public override string Label => "ハーフトーン（漫画の網点）";

    [Display(GroupName = "ハーフトーン", Name = "点の間隔")]
    [AnimationSlider("F0", "px", 3, 40)]
    public Animation Spacing { get; } = new Animation(9, 2, 400);

    [Display(GroupName = "ハーフトーン", Name = "点の大きさ")]
    [AnimationSlider("F0", "%", 10, 100)]
    public Animation DotSize { get; } = new Animation(55, 1, 100);

    [Display(GroupName = "ハーフトーン", Name = "網点の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(80, 0, 100);

    [Display(GroupName = "ハーフトーン", Name = "白黒にする")]
    [ToggleSlider]
    public bool Mono { get => mono; set => Set(ref mono, value); }
    bool mono = false;

    [Display(GroupName = "ハーフトーン", Name = "点の色")]
    [ColorPicker]
    public WpfColor DotColor { get => dotColor; set => Set(ref dotColor, value); }
    WpfColor dotColor = WpfColor.FromRgb(20, 20, 30);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Spacing, DotSize, Strength];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float sp = MathF.Max(2f, f.V(Spacing)), ds = f.V(DotSize) / 100f, st = f.V(Strength) / 100f;
        var light = k.Matrix("light", input, FxKit.Tone(0.18f, 1.3f, mono ? 0f : 1.2f));
        var col = FxKit.Rgb(dotColor);
        var cellRect = new RawRectF(0, 0, sp, sp);
        var cell = k.Draw(cellRect, dc =>
        {
            using var br = dc.CreateSolidColorBrush(col);
            float r = sp * 0.5f * ds;
            dc.FillEllipse(new Ellipse(new Vector2(sp / 2f, sp / 2f), r, r), br);
        }, clip: false);
        var b = f.B;
        var dots = k.Draw(b, dc => k.Tile(dc, cell, cellRect, b, Vector2.Zero, 1f));
        // 暗い所ほど網点を濃く
        var dark = k.Matrix("dark", input, new float[] { 0, 0, 0, -0.2126f * 1.2f, 0, 0, 0, -0.7152f * 1.2f, 0, 0, 0, -0.0722f * 1.2f, 0, 0, 0, 0, 0, 0, 0, 1.1f * st });
        var masked = k.Comp("mask", dots, dark, CompositeMode.DestinationIn);
        return k.Comp("atop", light, masked, CompositeMode.SourceAtop);
    }
}

[VideoEffect("ネオン縁取り", ["演出：質感・画風"], ["ネオン", "neon", "縁取り", "光る輪郭", "発光", "サイバー", "グロー", "演出"], IsAviUtlSupported = false)]
public sealed class NeonEdgeEffect : FxVideoEffectBase
{
    public override string Label => "ネオン縁取り";

    [Display(GroupName = "ネオン", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 1, 20)]
    public Animation Width { get; } = new Animation(4, 0.5, 200);

    [Display(GroupName = "ネオン", Name = "光の広がり")]
    [AnimationSlider("F0", "px", 0, 60)]
    public Animation Glow { get; } = new Animation(14, 0, 500);

    [Display(GroupName = "ネオン", Name = "光の強さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Strength { get; } = new Animation(130, 0, 1000);

    [Display(GroupName = "ネオン", Name = "中を暗くする")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Darken { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "ネオン", Name = "点滅")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "ネオン", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(60, 230, 255);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Width, Glow, Strength, Darken, Flicker];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float fl = f.V(Flicker) / 100f;
        float on = 1f - fl * (FxKit.Hash((int)(f.T * 12)) < 0.18f ? 0.85f : 0.1f * FxKit.Hash((int)f.Frame));
        float st = f.V(Strength) / 100f * on;
        var col = FxKit.Rgb(color);
        var grown = k.Outline("line", input, f.V(Width), new Color4(col.R, col.G, col.B, 1f));
        var ring = k.Comp("ring", grown, input, CompositeMode.DestinationOut);
        var bright = k.Matrix("bright", ring, FxKit.Tone(0.25f, 1f, 1f, MathF.Min(1f, st)));
        var glow = k.Matrix("galpha", k.Blur("glow", ring, f.V(Glow)), FxKit.Tone(0, 1, 1, MathF.Min(2f, st)));
        var inner = k.Matrix("inner", input, FxKit.Tone(-f.V(Darken) / 100f * 0.8f, 1f, 1f - f.V(Darken) / 100f * 0.5f));
        return k.Comp("add2", k.Comp("add1", inner, glow, CompositeMode.Plus), bright, CompositeMode.Plus);
    }
}

[VideoEffect("ホログラム", ["演出：質感・画風"], ["ホログラム", "hologram", "立体映像", "SF", "サイバー", "走査線", "未来", "演出"], IsAviUtlSupported = false)]
public sealed class HologramEffect : FxVideoEffectBase
{
    public override string Label => "ホログラム";

    [Display(GroupName = "ホログラム", Name = "透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Alpha { get; } = new Animation(75, 0, 100);

    [Display(GroupName = "ホログラム", Name = "走査線の間隔")]
    [AnimationSlider("F0", "px", 2, 20)]
    public Animation Lines { get; } = new Animation(4, 1, 200);

    [Display(GroupName = "ホログラム", Name = "ちらつき")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(30, 0, 100);

    [Display(GroupName = "ホログラム", Name = "横ずれのノイズ")]
    [AnimationSlider("F0", "px", 0, 30)]
    public Animation Jitter { get; } = new Animation(4, 0, 300);

    [Display(GroupName = "ホログラム", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(90, 220, 255);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Alpha, Lines, Flicker, Jitter];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var col = FxKit.Rgb(color);
        float fl = f.V(Flicker) / 100f;
        float op = f.V(Alpha) / 100f * (1f - fl * 0.35f * FxKit.Hash((int)f.Frame * 7 + 1));
        // 明るさを、指定の色に置き換える
        float l = 1.25f;
        var tint = new float[]
        {
            0.2126f * l * col.R, 0.2126f * l * col.G, 0.2126f * l * col.B, 0,
            0.7152f * l * col.R, 0.7152f * l * col.G, 0.7152f * l * col.B, 0,
            0.0722f * l * col.R, 0.0722f * l * col.G, 0.0722f * l * col.B, 0,
            0, 0, 0, op,
            col.R * 0.15f, col.G * 0.15f, col.B * 0.15f, 0,
        };
        var holo = k.Matrix("tint", input, tint);
        float sp = MathF.Max(1f, f.V(Lines)), jit = f.V(Jitter);
        var cellRect = new RawRectF(0, 0, 4, sp);
        var cell = k.Draw(cellRect, dc =>
        {
            using var br = dc.CreateSolidColorBrush(new Color4(0, 0, 0, 0.45f));
            dc.FillRectangle(new RawRectF(0, 0, 4, MathF.Max(0.5f, sp * 0.4f)), br);
        }, clip: false);
        var b = f.B; float t = f.T; int fr = (int)f.Frame;
        var ghost = k.Matrix("ghost", holo, FxKit.Tone(0, 1, 1, 0.35f));
        bool glitch = FxKit.Hash(fr * 13 + 5) < 0.12f * (0.3f + fl);
        float gx = glitch ? (FxKit.Hash(fr * 3) - 0.5f) * jit * 4f : 0f;
        var lines = k.Draw(b, dc => k.Tile(dc, cell, cellRect, b, new Vector2(0, t * 30f % sp), 1f));
        var lined = k.Comp("lines", holo, lines, CompositeMode.SourceAtop);
        return k.Draw(new RawRectF(b.Left - jit * 3, b.Top, b.Right + jit * 3, b.Bottom), dc =>
        {
            dc.Transform = Matrix3x2.CreateTranslation(jit + gx, 0);
            dc.DrawImage(ghost, InterpolationMode.Linear, CompositeMode.Plus);
            dc.Transform = Matrix3x2.CreateTranslation(gx * 0.3f, 0);
            dc.DrawImage(lined, InterpolationMode.Linear, CompositeMode.SourceOver);
        });
    }
}

[VideoEffect("グリッチ", ["演出：質感・画風"], ["グリッチ", "glitch", "バグ", "ノイズ", "乱れ", "電波", "デジタル", "ハッキング", "演出"], IsAviUtlSupported = false)]
public sealed class GlitchEffect : FxVideoEffectBase
{
    public override string Label => "グリッチ";

    [Display(GroupName = "グリッチ", Name = "頻度", Description = "乱れが起きる割合(100%で常に乱れる)")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Frequency { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "グリッチ", Name = "横ずれの大きさ")]
    [AnimationSlider("F0", "px", 0, 200)]
    public Animation Shift { get; } = new Animation(40, 0, 2000);

    [Display(GroupName = "グリッチ", Name = "色ずれ")]
    [AnimationSlider("F0", "px", 0, 50)]
    public Animation Split { get; } = new Animation(8, 0, 500);

    [Display(GroupName = "グリッチ", Name = "帯の数")]
    [TextBoxSlider("F0", "本", 1, 40)]
    [Range(1, 200)]
    public int Slices { get => slices; set => Set(ref slices, value); }
    int slices = 12;

    [Display(GroupName = "グリッチ", Name = "乱れの速さ")]
    [AnimationSlider("F0", "回/秒", 1, 30)]
    public Animation Rate { get; } = new Animation(12, 0.1, 120);

    [Display(GroupName = "グリッチ", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Frequency, Shift, Split, Rate];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        int step = (int)MathF.Floor(f.T * f.V(Rate)) + seed * 1000;
        if (FxKit.Hash(step * 7 + 3) >= f.V(Frequency) / 100f) return input;
        float shift = f.V(Shift), split = f.V(Split);
        var r = k.Matrix("r", input, FxKit.Channel(0));
        var g = k.Matrix("g", input, FxKit.Channel(1));
        var bch = k.Matrix("b", input, FxKit.Channel(2));
        var b = f.B; float h = f.H; int n = Math.Max(1, slices);
        var big = new RawRectF(b.Left - shift - split, b.Top, b.Right + shift + split, b.Bottom);
        return k.Draw(big, dc =>
        {
            float y = b.Top;
            for (int i = 0; i < n && y < b.Bottom; i++)
            {
                float hh = h / n * (0.4f + 1.2f * FxKit.Hash(step * 31 + i));
                float y1 = MathF.Min(b.Bottom, y + hh);
                bool move = FxKit.Hash(step * 17 + i * 5) < 0.45f;
                float dx = move ? (FxKit.Hash(step * 41 + i) - 0.5f) * 2f * shift : 0f;
                dc.PushAxisAlignedClip(new RawRectF(big.Left, y, big.Right, y1), AntialiasMode.Aliased);
                dc.Transform = Matrix3x2.CreateTranslation(dx + split, 0);
                dc.DrawImage(r, InterpolationMode.Linear, CompositeMode.Plus);
                dc.Transform = Matrix3x2.CreateTranslation(dx, 0);
                dc.DrawImage(g, InterpolationMode.Linear, CompositeMode.Plus);
                dc.Transform = Matrix3x2.CreateTranslation(dx - split, 0);
                dc.DrawImage(bch, InterpolationMode.Linear, CompositeMode.Plus);
                dc.Transform = Matrix3x2.Identity;
                dc.PopAxisAlignedClip();
                y = y1;
            }
        }, clip: false);
    }
}

[VideoEffect("VHS・ブラウン管風", ["演出：質感・画風"], ["VHS", "ビデオ", "ブラウン管", "CRT", "走査線", "レトロ", "昭和", "平成", "テープ", "演出"], IsAviUtlSupported = false)]
public sealed class VhsEffect : FxVideoEffectBase
{
    public override string Label => "VHS・ブラウン管風";

    [Display(GroupName = "VHS", Name = "色のにじみ")]
    [AnimationSlider("F0", "px", 0, 20)]
    public Animation Bleed { get; } = new Animation(4, 0, 200);

    [Display(GroupName = "VHS", Name = "走査線の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Scan { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "VHS", Name = "ノイズ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Noise { get; } = new Animation(15, 0, 100);

    [Display(GroupName = "VHS", Name = "トラッキングの乱れ", Description = "画面を上下に流れる、横にずれた帯")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Tracking { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "VHS", Name = "色あせ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Fade { get; } = new Animation(25, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Bleed, Scan, Noise, Tracking, Fade];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float fade = f.V(Fade) / 100f, bleed = f.V(Bleed), scan = f.V(Scan) / 100f, noise = f.V(Noise) / 100f, track = f.V(Tracking) / 100f;
        var toned = k.Matrix("tone", k.Blur("soft", input, 0.6f), FxKit.Tone(0.03f * fade, 1f - 0.2f * fade, 1f - 0.4f * fade));
        var r = k.Matrix("r", toned, FxKit.Channel(0));
        var g = k.Matrix("g", toned, FxKit.Channel(1));
        var bl = k.Matrix("b", toned, FxKit.Channel(2));
        var b = f.B; float h = f.H, t = f.T; int fr = (int)f.Frame;
        var cellRect = new RawRectF(0, 0, 4, 3);
        var cell = k.Draw(cellRect, dc =>
        {
            using var br = dc.CreateSolidColorBrush(new Color4(0, 0, 0, scan * 0.6f));
            dc.FillRectangle(new RawRectF(0, 0, 4, 1.2f), br);
        }, clip: false);
        var nz = k.Turb("noise", new Vector2(FxKit.Hash(fr) * 999f, FxKit.Hash(fr + 3) * 999f), 0.8f, 1, fr % 997);
        var nzGray = k.Matrix("ngray", nz, new float[] { 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, noise * 0.4f });
        float bandY = b.Top + ((t * 0.17f) % 1.3f - 0.15f) * h, bandH = h * 0.06f;
        float bandShift = track * 30f * (0.5f + FxKit.Hash(fr));
        var picture = k.Draw(new RawRectF(b.Left - 40, b.Top, b.Right + 40, b.Bottom), dc =>
        {
            void Rgb(float dx)
            {
                dc.Transform = Matrix3x2.CreateTranslation(dx + bleed, 0);
                dc.DrawImage(r, InterpolationMode.Linear, CompositeMode.Plus);
                dc.Transform = Matrix3x2.CreateTranslation(dx, 0);
                dc.DrawImage(g, InterpolationMode.Linear, CompositeMode.Plus);
                dc.Transform = Matrix3x2.CreateTranslation(dx - bleed, 0);
                dc.DrawImage(bl, InterpolationMode.Linear, CompositeMode.Plus);
                dc.Transform = Matrix3x2.Identity;
            }
            if (track > 0)
            {
                dc.PushAxisAlignedClip(new RawRectF(b.Left - 40, b.Top, b.Right + 40, bandY), AntialiasMode.Aliased); Rgb(0); dc.PopAxisAlignedClip();
                dc.PushAxisAlignedClip(new RawRectF(b.Left - 40, bandY, b.Right + 40, bandY + bandH), AntialiasMode.Aliased); Rgb(bandShift); dc.PopAxisAlignedClip();
                dc.PushAxisAlignedClip(new RawRectF(b.Left - 40, bandY + bandH, b.Right + 40, b.Bottom), AntialiasMode.Aliased); Rgb(0); dc.PopAxisAlignedClip();
            }
            else Rgb(0);
        });
        var overlay = k.Draw(b, dc =>
        {
            k.Tile(dc, cell, cellRect, b, Vector2.Zero, 1f);
            dc.DrawImage(nzGray, InterpolationMode.Linear, CompositeMode.SourceOver);
        });
        return k.Comp("atop", picture, overlay, CompositeMode.SourceAtop);
    }
}
