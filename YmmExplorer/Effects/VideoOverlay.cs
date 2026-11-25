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
//  重ねる演出(図形の描画から)
// =====================================================================================================

[VideoEffect("集中線", ["演出：重ねる"], ["集中線", "効果線", "漫画", "マンガ", "speed lines", "注目", "強調", "驚き", "演出"], IsAviUtlSupported = false)]
public sealed class SpeedLinesEffect : FxVideoEffectBase
{
    public override string Label => "集中線";

    [Display(GroupName = "集中線", Name = "本数")]
    [TextBoxSlider("F0", "本", 10, 200)]
    [Range(1, 1000)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 70;

    [Display(GroupName = "集中線", Name = "線の太さ")]
    [AnimationSlider("F0", "px", 1, 60)]
    public Animation Width { get; } = new Animation(14, 0.5, 500);

    [Display(GroupName = "集中線", Name = "まん中の空き")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Inner { get; } = new Animation(45, 0, 200);

    [Display(GroupName = "集中線", Name = "ちらつく速さ", Description = "線が描き変わる速さ(0で止まったまま)")]
    [AnimationSlider("F0", "回/秒", 0, 30)]
    public Animation Speed { get; } = new Animation(12, 0, 120);

    [Display(GroupName = "集中線", Name = "中心の横位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation X { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "集中線", Name = "中心の縦位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Y { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "集中線", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(15, 15, 20);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Width, Inner, Speed, X, Y];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var b = f.B; var c = f.At(f.V(X) / 100f, f.V(Y) / 100f);
        float diag = MathF.Sqrt(f.W * f.W + f.H * f.H), inner = f.V(Inner) / 100f * MathF.Min(f.W, f.H) * 0.5f, wid = f.V(Width);
        int step = (int)MathF.Floor(f.T * f.V(Speed)), n = Math.Max(1, count);
        var col = FxKit.Rgb(color);
        return k.Draw(b, dc =>
        {
            dc.DrawImage(input, InterpolationMode.Linear, CompositeMode.SourceOver);
            using var br = dc.CreateSolidColorBrush(col);
            var poly = new List<Vector2>(3);
            for (int i = 0; i < n; i++)
            {
                float ang = (i + (FxKit.Hash(step * 97 + i) - 0.5f) * 0.9f) / n * MathF.PI * 2f;
                float r0 = inner * (0.8f + 0.5f * FxKit.Hash(step * 53 + i * 3 + 1));
                float ww = wid * (0.3f + 1.2f * FxKit.Hash(step * 29 + i * 7 + 2));
                var d = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                var nrm = new Vector2(-d.Y, d.X);
                poly.Clear();
                poly.Add(c + d * r0);
                poly.Add(c + d * diag + nrm * ww / 2f);
                poly.Add(c + d * diag - nrm * ww / 2f);
                YMM4EffectShapes.Gfx.FillPoly(dc, br, poly);
            }
        });
    }
}

[VideoEffect("キラキラ", ["演出：重ねる"], ["キラキラ", "きらめき", "sparkle", "輝き", "星", "光", "かわいい", "演出"], IsAviUtlSupported = false)]
public sealed class SparkleEffect : FxVideoEffectBase
{
    public override string Label => "キラキラ";

    [Display(GroupName = "キラキラ", Name = "数")]
    [TextBoxSlider("F0", "個", 1, 100)]
    [Range(1, 1000)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 18;

    [Display(GroupName = "キラキラ", Name = "大きさ")]
    [AnimationSlider("F0", "px", 4, 120)]
    public Animation Size { get; } = new Animation(28, 1, 1000);

    [Display(GroupName = "キラキラ", Name = "またたく速さ")]
    [AnimationSlider("F1", "", 0.1, 5)]
    public Animation Speed { get; } = new Animation(1.2, 0, 50);

    [Display(GroupName = "キラキラ", Name = "形の上だけに出す", Description = "オンで、アイテムの形(不透明な所)の上だけに出します")]
    [ToggleSlider]
    public bool OnShape { get => onShape; set => Set(ref onShape, value); }
    bool onShape = false;

    [Display(GroupName = "キラキラ", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 250, 220);

    [Display(GroupName = "キラキラ", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Size, Speed];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var b = f.B; float size = f.V(Size), t = f.T * f.V(Speed), w = f.W, h = f.H;
        int n = Math.Max(1, count), sd = seed;
        var col = FxKit.Rgb(color);
        var stars = k.Draw(b, dc =>
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            for (int i = 0; i < n; i++)
            {
                float phase = FxKit.Hash(sd * 31 + i) * 10f;
                float cycle = t + phase;
                int gen = (int)MathF.Floor(cycle);              // またたくたびに場所を変える
                float life = cycle - gen;
                float a = MathF.Pow(MathF.Sin(life * MathF.PI), 2f);
                var p = new Vector2(b.Left + w * FxKit.Hash(sd * 7 + i * 13 + gen * 101), b.Top + h * FxKit.Hash(sd * 11 + i * 17 + gen * 103));
                float s = size * (0.5f + 0.7f * FxKit.Hash(sd * 3 + i * 5 + gen)) * (0.4f + 0.6f * a);
                FxDraw.Radial(dc, p, s * 0.35f, s * 0.35f, (0, FxDraw.A(col, 0.8f * a)), (1, FxDraw.A(col, 0)));
                FxDraw.Radial(dc, p, s, s * 0.08f + 0.5f, (0, FxDraw.A(col, a)), (1, FxDraw.A(col, 0)));
                FxDraw.Radial(dc, p, s * 0.08f + 0.5f, s, (0, FxDraw.A(col, a)), (1, FxDraw.A(col, 0)));
            }
        });
        if (onShape) stars = k.Comp("shape", stars, input, CompositeMode.DestinationIn);
        return k.Comp("add", input, stars, CompositeMode.Plus);
    }
}

public enum WeatherKind
{
    [Display(Name = "雨")] Rain,
    [Display(Name = "雪")] Snow,
}

[VideoEffect("雨・雪を重ねる", ["演出：重ねる"], ["雨", "雪", "rain", "snow", "天気", "降る", "冬", "梅雨", "演出"], IsAviUtlSupported = false)]
public sealed class WeatherEffect : FxVideoEffectBase
{
    public override string Label => "雨・雪を重ねる";

    [Display(GroupName = "雨・雪", Name = "種類")]
    [EnumComboBox]
    public WeatherKind Kind { get => kind; set => Set(ref kind, value); }
    WeatherKind kind = WeatherKind.Rain;

    [Display(GroupName = "雨・雪", Name = "量")]
    [TextBoxSlider("F0", "個", 10, 500)]
    [Range(1, 3000)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 150;

    [Display(GroupName = "雨・雪", Name = "落ちる速さ")]
    [AnimationSlider("F0", "%", 10, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "雨・雪", Name = "風(傾き)")]
    [AnimationSlider("F0", "°", -45, 45)]
    public Animation Wind { get; } = new Animation(10, -80, 80);

    [Display(GroupName = "雨・雪", Name = "粒の大きさ")]
    [AnimationSlider("F0", "%", 20, 300)]
    public Animation Size { get; } = new Animation(100, 1, 2000);

    [Display(GroupName = "雨・雪", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "雨・雪", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(225, 235, 255);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Speed, Wind, Size, Strength];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var b = f.B; float w = f.W, h = f.H, t = f.T, sp = f.V(Speed) / 100f, size = f.V(Size) / 100f, str = f.V(Strength) / 100f;
        float wind = f.V(Wind) * MathF.PI / 180f;
        int n = Math.Max(1, count); bool rain = kind == WeatherKind.Rain;
        var col = FxKit.Rgb(color);
        float unit = MathF.Max(w, h) / 1080f;
        return k.Draw(b, dc =>
        {
            dc.DrawImage(input, InterpolationMode.Linear, CompositeMode.SourceOver);
            using var br = dc.CreateSolidColorBrush(col);
            var dir = new Vector2(MathF.Sin(wind), MathF.Cos(wind));
            for (int i = 0; i < n; i++)
            {
                float depth = 0.4f + 0.6f * FxKit.Hash(i * 7 + 1);
                float v = (rain ? 1400f : 120f) * sp * depth * unit;
                float span = h + 200f * unit;
                float y = (FxKit.Hash(i * 13 + 2) * span + t * v) % span - 100f * unit;
                float x = b.Left + FxKit.Hash(i * 17 + 3) * (w + h * 0.5f) - h * 0.25f + (y + 100f) * MathF.Tan(wind);
                var p = new Vector2(x, b.Top + y);
                br.Color = FxDraw.A(col, str * (0.35f + 0.65f * depth));
                if (rain)
                {
                    float len = 40f * size * depth * unit;
                    dc.DrawLine(p, p + dir * len, br, MathF.Max(0.6f, 1.6f * size * depth * unit));
                }
                else
                {
                    float r = 4f * size * depth * unit;
                    p.X += MathF.Sin(t * 1.3f + i) * 18f * unit;
                    dc.FillEllipse(new Ellipse(p, r, r), br);
                }
            }
        });
    }
}

[VideoEffect("スポットライト", ["演出：重ねる"], ["スポットライト", "spotlight", "照明", "注目", "暗転", "舞台", "ステージ", "演出"], IsAviUtlSupported = false)]
public sealed class SpotlightEffect : FxVideoEffectBase
{
    public override string Label => "スポットライト";

    [Display(GroupName = "スポットライト", Name = "照らす横位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation X { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "スポットライト", Name = "照らす縦位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Y { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "スポットライト", Name = "光の大きさ")]
    [AnimationSlider("F0", "%", 5, 100)]
    public Animation Radius { get; } = new Animation(28, 0, 300);

    [Display(GroupName = "スポットライト", Name = "縁の柔らかさ")]
    [AnimationSlider("F0", "%", 1, 100)]
    public Animation Soft { get; } = new Animation(25, 1, 300);

    [Display(GroupName = "スポットライト", Name = "まわりの暗さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Dark { get; } = new Animation(75, 0, 100);

    [Display(GroupName = "スポットライト", Name = "光の色づけ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Tint { get; } = new Animation(15, 0, 100);

    [Display(GroupName = "スポットライト", Name = "光の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 240, 200);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [X, Y, Radius, Soft, Dark, Tint];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var b = f.B; var c = f.At(f.V(X) / 100f, f.V(Y) / 100f);
        float r = f.V(Radius) / 100f * MathF.Min(f.W, f.H), soft = f.V(Soft) / 100f * MathF.Min(f.W, f.H);
        float dark = f.V(Dark) / 100f, tint = f.V(Tint) / 100f;
        float outer = MathF.Max(1f, r + soft), p0 = Math.Clamp(r / outer, 0f, 0.99f);
        var col = FxKit.Rgb(color);
        var shade = k.Draw(b, dc =>
        {
            // まわりを暗く
            dc.FillRectangle(b, k.Keep(dc.CreateSolidColorBrush(new Color4(0, 0, 0, dark))));
            dc.PrimitiveBlend = PrimitiveBlend.Copy;
            FxDraw.Radial(dc, c, outer, outer, (0, new Color4(0, 0, 0, 0)), (p0, new Color4(0, 0, 0, 0)), (1, new Color4(0, 0, 0, dark)));
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            if (tint > 0) FxDraw.Radial(dc, c, outer, outer, (0, FxDraw.A(col, tint * 0.6f)), (p0, FxDraw.A(col, tint * 0.4f)), (1, FxDraw.A(col, 0)));
        });
        return k.Comp("atop", input, shade, CompositeMode.SourceAtop);
    }
}

[VideoEffect("紙吹雪", ["演出：重ねる"], ["紙吹雪", "confetti", "お祝い", "パーティー", "おめでとう", "クラッカー", "優勝", "演出"], IsAviUtlSupported = false)]
public sealed class ConfettiEffect : FxVideoEffectBase
{
    public override string Label => "紙吹雪";

    [Display(GroupName = "紙吹雪", Name = "数")]
    [TextBoxSlider("F0", "枚", 10, 400)]
    [Range(1, 3000)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 120;

    [Display(GroupName = "紙吹雪", Name = "落ちる速さ")]
    [AnimationSlider("F0", "%", 10, 300)]
    public Animation Speed { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "紙吹雪", Name = "紙の大きさ")]
    [AnimationSlider("F0", "%", 20, 300)]
    public Animation Size { get; } = new Animation(100, 1, 2000);

    [Display(GroupName = "紙吹雪", Name = "上から降らせる時間差", Description = "最初は上から順に降ってきます(0で、はじめから全体に)")]
    [ToggleSlider]
    public bool FromTop { get => fromTop; set => Set(ref fromTop, value); }
    bool fromTop = true;

    [Display(GroupName = "紙吹雪", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    static readonly Color4[] Palette =
    {
        new(1f, 0.35f, 0.4f, 1), new(1f, 0.8f, 0.2f, 1), new(0.3f, 0.75f, 1f, 1), new(0.45f, 0.9f, 0.45f, 1),
        new(0.8f, 0.5f, 1f, 1), new(1f, 0.6f, 0.2f, 1), new(1f, 1f, 1f, 1),
    };

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Speed, Size];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        var b = f.B; float w = f.W, h = f.H, t = f.T, sp = f.V(Speed) / 100f, size = f.V(Size) / 100f;
        int n = Math.Max(1, count), sd = seed; bool top = fromTop;
        float unit = MathF.Max(w, h) / 1080f;
        return k.Draw(b, dc =>
        {
            dc.DrawImage(input, InterpolationMode.Linear, CompositeMode.SourceOver);
            using var br = dc.CreateSolidColorBrush(Palette[0]);
            for (int i = 0; i < n; i++)
            {
                int id = sd * 7919 + i;
                float v = (150f + 120f * FxKit.Hash(id * 3)) * sp * unit;
                float y0 = top ? -FxKit.Hash(id * 5 + 1) * h * 0.8f - 30f * unit : FxKit.Hash(id * 5 + 1) * h;
                float y = y0 + t * v;
                if (!top) y = (y % (h + 60f * unit)) - 30f * unit;
                else if (y > h + 40f * unit) y = (y - h - 40f * unit) % (h + 80f * unit) - 40f * unit;
                float x = w * FxKit.Hash(id * 7 + 2) + MathF.Sin(t * (1.5f + FxKit.Hash(id)) + i) * 30f * unit;
                float pw = (8f + 8f * FxKit.Hash(id * 11)) * size * unit, ph = pw * 0.55f;
                float spin = t * (2f + 4f * FxKit.Hash(id * 13)) + i;
                float flip = MathF.Abs(MathF.Cos(t * (3f + 3f * FxKit.Hash(id * 17)) + i));     // くるくる裏返る
                br.Color = Palette[(int)(FxKit.Hash(id * 19) * Palette.Length) % Palette.Length];
                var p = new Vector2(b.Left + x, b.Top + y);
                dc.Transform = Matrix3x2.CreateScale(1f, MathF.Max(0.15f, flip)) * Matrix3x2.CreateRotation(spin) * Matrix3x2.CreateTranslation(p);
                dc.FillRectangle(new RawRectF(-pw / 2, -ph / 2, pw / 2, ph / 2), br);
            }
            dc.Transform = Matrix3x2.Identity;
        });
    }
}

[VideoEffect("炎のまとい（オーラ）", ["演出：重ねる"], ["炎", "オーラ", "aura", "燃える", "まとう", "気合", "怒り", "覚醒", "スーパー", "演出"], IsAviUtlSupported = false)]
public sealed class FlameAuraEffect : FxVideoEffectBase
{
    public override string Label => "炎のまとい（オーラ）";

    [Display(GroupName = "オーラ", Name = "炎の大きさ")]
    [AnimationSlider("F0", "px", 2, 100)]
    public Animation Size { get; } = new Animation(24, 0, 1000);

    [Display(GroupName = "オーラ", Name = "ゆらめき")]
    [AnimationSlider("F0", "px", 0, 100)]
    public Animation Wave { get; } = new Animation(30, 0, 1000);

    [Display(GroupName = "オーラ", Name = "立ちのぼる速さ")]
    [AnimationSlider("F0", "px/秒", 0, 600)]
    public Animation Speed { get; } = new Animation(180, -5000, 5000);

    [Display(GroupName = "オーラ", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Strength { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "オーラ", Name = "外側の色")]
    [ColorPicker]
    public WpfColor OuterColor { get => outer; set => Set(ref outer, value); }
    WpfColor outer = WpfColor.FromRgb(255, 90, 20);

    [Display(GroupName = "オーラ", Name = "内側の色")]
    [ColorPicker]
    public WpfColor InnerColor { get => inner; set => Set(ref inner, value); }
    WpfColor inner = WpfColor.FromRgb(255, 230, 120);

    [Display(GroupName = "オーラ", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Size, Wave, Speed, Strength];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float size = MathF.Max(0.5f, f.V(Size)), st = f.V(Strength) / 100f;
        var oc = FxKit.Rgb(outer); var ic = FxKit.Rgb(inner);
        var map = k.Turb("turb", new Vector2(0, f.T * f.V(Speed)), 0.012f, 3, seed);
        var outerFlame = k.Outline("outer", input, size * 2f, new Color4(oc.R, oc.G, oc.B, MathF.Min(1f, st)));
        var innerFlame = k.Outline("inner", input, size * 0.9f, new Color4(ic.R, ic.G, ic.B, MathF.Min(1f, st)));
        var o2 = k.Displace("dout", outerFlame, map, f.V(Wave));
        var i2 = k.Displace("din", innerFlame, map, f.V(Wave) * 0.6f);
        var flames = k.Comp("flames", o2, i2, CompositeMode.Plus);
        return k.Comp("over", flames, input, CompositeMode.SourceOver);
    }
}
