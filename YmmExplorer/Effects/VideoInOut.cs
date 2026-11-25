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
//  出入り(登場・退場)
// =====================================================================================================

public enum InOutMode
{
    [Display(Name = "登場と退場")] Both,
    [Display(Name = "登場だけ")] InOnly,
    [Display(Name = "退場だけ")] OutOnly,
}

/// <summary>登場・退場の長さを持つ演出エフェクト</summary>
public abstract class FxInOutBase : FxVideoEffectBase
{
    [Display(GroupName = "登場・退場", Name = "使う場面")]
    [EnumComboBox]
    public InOutMode Mode { get => mode; set => Set(ref mode, value); }
    InOutMode mode = InOutMode.Both;

    [Display(GroupName = "登場・退場", Name = "登場の長さ")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation InLength { get; } = new Animation(0.6, 0, 600);

    [Display(GroupName = "登場・退場", Name = "退場の長さ")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation OutLength { get; } = new Animation(0.6, 0, 600);

    /// <summary>見えている割合(0=見えない 1=全部見える)</summary>
    protected float Visible(in FxFrame f)
        => f.InOut(mode == InOutMode.OutOnly ? 0 : f.V(InLength), mode == InOutMode.InOnly ? 0 : f.V(OutLength));

    /// <summary>登場中なら true(割れる・破れる演出の向きを決める)</summary>
    protected bool Entering(in FxFrame f) => mode != InOutMode.OutOnly && f.T < f.Dur / 2f;

    protected IEnumerable<IAnimatable> BaseAnimatables => [InLength, OutLength];
}

[VideoEffect("ディゾルブ（粒になって消える）", ["演出：出入り"], ["ディゾルブ", "dissolve", "粒", "砂", "消える", "現れる", "フェード", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class DissolveEffect : FxInOutBase
{
    public override string Label => "ディゾルブ（粒になって消える）";

    [Display(GroupName = "ディゾルブ", Name = "粒の大きさ")]
    [AnimationSlider("F0", "px", 1, 100)]
    public Animation Grain { get; } = new Animation(12, 0.5, 1000);

    [Display(GroupName = "ディゾルブ", Name = "境目のくっきりさ")]
    [AnimationSlider("F0", "", 2, 60)]
    public Animation Sharp { get; } = new Animation(18, 1, 500);

    [Display(GroupName = "ディゾルブ", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    protected override IEnumerable<IAnimatable> GetAnimatables() => [.. BaseAnimatables, Grain, Sharp];

    internal static float[] Threshold(float p, float sharp, float r = 0, float g = 0, float b = 0)
    {
        float kk = MathF.Max(1f, sharp);
        float thr = (1f - p) * (1f + 2f / kk) - 1f / kk;
        return new float[] { 0, 0, 0, kk, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, r, g, b, -kk * thr };
    }

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float p = Visible(f);
        if (p >= 0.999f) return input;
        var noise = k.Turb("turb", Vector2.Zero, 1f / MathF.Max(0.5f, f.V(Grain)), 3, seed);
        var mask = k.Matrix("mask", noise, Threshold(p, f.V(Sharp)));
        return k.Comp("in", input, mask, CompositeMode.DestinationIn);
    }
}

[VideoEffect("燃えて消える", ["演出：出入り"], ["燃える", "燃えて消える", "burn", "炎", "焼ける", "灰", "消える", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class BurnAwayEffect : FxInOutBase
{
    public override string Label => "燃えて消える";

    [Display(GroupName = "燃える", Name = "燃える縁の太さ")]
    [AnimationSlider("F0", "%", 1, 30)]
    public Animation Band { get; } = new Animation(8, 0.5, 100);

    [Display(GroupName = "燃える", Name = "模様の大きさ")]
    [AnimationSlider("F0", "px", 5, 200)]
    public Animation Grain { get; } = new Animation(40, 1, 2000);

    [Display(GroupName = "燃える", Name = "火の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 140, 30);

    [Display(GroupName = "燃える", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    public BurnAwayEffect() { Mode = InOutMode.OutOnly; }

    protected override IEnumerable<IAnimatable> GetAnimatables() => [.. BaseAnimatables, Band, Grain];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float p = Visible(f);
        if (p >= 0.999f) return input;
        float band = f.V(Band) / 100f;
        var col = FxKit.Rgb(color);
        var noise = k.Turb("turb", Vector2.Zero, 1f / MathF.Max(1f, f.V(Grain)), 4, seed);
        var keep = k.Matrix("keep", noise, DissolveEffect.Threshold(p, 25f));
        var fire = k.Matrix("fire", noise, DissolveEffect.Threshold(MathF.Min(1f, p + band), 25f, col.R, col.G, col.B));
        var ring = k.Comp("ring", fire, keep, CompositeMode.DestinationOut);
        var ringIn = k.Comp("ringIn", ring, input, CompositeMode.DestinationIn);
        var visible = k.Comp("visible", input, keep, CompositeMode.DestinationIn);
        var withRing = k.Comp("with", visible, ringIn, CompositeMode.SourceOver);
        return k.Comp("glow", withRing, k.Blur("blur", ringIn, 6f), CompositeMode.Plus);
    }
}

[VideoEffect("ガラスが割れる", ["演出：出入り"], ["ガラス", "割れる", "粉々", "破片", "shatter", "砕ける", "衝撃", "退場", "登場", "演出"], IsAviUtlSupported = false)]
public sealed class GlassShatterEffect : FxInOutBase
{
    public override string Label => "ガラスが割れる";

    [Display(GroupName = "ガラス", Name = "破片の細かさ")]
    [TextBoxSlider("F0", "", 2, 20)]
    [Range(2, 40)]
    public int Pieces { get => pieces; set => Set(ref pieces, value); }
    int pieces = 7;

    [Display(GroupName = "ガラス", Name = "飛び散る強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Force { get; } = new Animation(60, 0, 1000);

    [Display(GroupName = "ガラス", Name = "落ちる重さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Gravity { get; } = new Animation(80, -500, 1000);

    [Display(GroupName = "ガラス", Name = "割れる中心の横位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation X { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ガラス", Name = "割れる中心の縦位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Y { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ガラス", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    public GlassShatterEffect() { Mode = InOutMode.OutOnly; }

    protected override IEnumerable<IAnimatable> GetAnimatables() => [.. BaseAnimatables, Force, Gravity, X, Y];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float q = 1f - Visible(f);
        if (q <= 0.001f) return input;
        int n = Math.Clamp(pieces, 2, 40);
        var b = f.B; float w = f.W, h = f.H;
        var hit = f.At(f.V(X) / 100f, f.V(Y) / 100f);
        float force = f.V(Force) / 100f, grav = f.V(Gravity) / 100f;
        int sd = seed;
        // ゆがんだ格子の点
        var pts = new Vector2[n + 1, n + 1];
        for (int i = 0; i <= n; i++)
            for (int j = 0; j <= n; j++)
            {
                float jx = (i == 0 || i == n) ? 0 : (FxKit.Hash(sd * 977 + i * 131 + j) - 0.5f) * 0.7f;
                float jy = (j == 0 || j == n) ? 0 : (FxKit.Hash(sd * 977 + i * 71 + j * 13 + 5) - 0.5f) * 0.7f;
                pts[i, j] = new Vector2(b.Left + w * (i + jx) / n, b.Top + h * (j + jy) / n);
            }
        var bigR = new RawRectF(b.Left - w, b.Top - h, b.Right + w, b.Bottom + h * 3);
        return k.Draw(bigR, dc =>
        {
            var brush = k.ImageBrush(dc, input, b, 1f);
            var tri = new List<Vector2>(3);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int s = 0; s < 2; s++)
                    {
                        tri.Clear();
                        if (s == 0) { tri.Add(pts[i, j]); tri.Add(pts[i + 1, j]); tri.Add(pts[i, j + 1]); }
                        else { tri.Add(pts[i + 1, j]); tri.Add(pts[i + 1, j + 1]); tri.Add(pts[i, j + 1]); }
                        var c = (tri[0] + tri[1] + tri[2]) / 3f;
                        int id = (i * 41 + j) * 2 + s + sd * 7919;
                        var dir = c - hit;
                        float len = dir.Length();
                        dir = len > 0.01f ? dir / len : new Vector2(0, -1);
                        float near = 1f - Math.Clamp(len / MathF.Max(w, h), 0f, 1f);
                        // 中心に近い破片ほど早く、強く飛ぶ
                        float qq = Math.Clamp(q * 1.3f - (1f - near) * 0.3f, 0f, 1f);
                        var off = dir * (force * MathF.Max(w, h) * 0.5f * (0.4f + near) * qq) + new Vector2(0, grav * h * 1.4f * qq * qq);
                        float rot = (FxKit.Hash(id) - 0.5f) * 6f * qq;
                        brush.Opacity = Math.Clamp(1f - qq * qq * 1.1f, 0f, 1f);
                        if (brush.Opacity <= 0.01f) continue;
                        dc.Transform = Matrix3x2.CreateRotation(rot, c) * Matrix3x2.CreateTranslation(off);
                        YMM4EffectShapes.Gfx.FillPoly(dc, brush, tri);
                    }
            dc.Transform = Matrix3x2.Identity;
        }, clip: false);
    }
}

[VideoEffect("紙が破れる", ["演出：出入り"], ["紙", "破れる", "破る", "tear", "引き裂く", "二つに割れる", "退場", "登場", "演出"], IsAviUtlSupported = false)]
public sealed class PaperTearEffect : FxInOutBase
{
    public override string Label => "紙が破れる";

    [Display(GroupName = "紙", Name = "破れ目のギザギザ")]
    [AnimationSlider("F0", "%", 0, 10)]
    public Animation Jag { get; } = new Animation(3, 0, 50);

    [Display(GroupName = "紙", Name = "離れる量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Apart { get; } = new Animation(45, 0, 300);

    [Display(GroupName = "紙", Name = "傾き")]
    [AnimationSlider("F0", "°", 0, 45)]
    public Animation Tilt { get; } = new Animation(12, 0, 180);

    [Display(GroupName = "紙", Name = "破れ目の縁の色")]
    [ColorPicker]
    public WpfColor EdgeColor { get => edge; set => Set(ref edge, value); }
    WpfColor edge = WpfColor.FromRgb(250, 248, 240);

    [Display(GroupName = "紙", Name = "シード")]
    [TextBoxSlider("F0", "", 0, 999)]
    [Range(0, 99999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    public PaperTearEffect() { Mode = InOutMode.OutOnly; }

    protected override IEnumerable<IAnimatable> GetAnimatables() => [.. BaseAnimatables, Jag, Apart, Tilt];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float q = 1f - Visible(f);
        if (q <= 0.001f) return input;
        var b = f.B; float w = f.W, h = f.H;
        float jag = f.V(Jag) / 100f * w, apart = f.V(Apart) / 100f * w * q, tilt = f.V(Tilt) * MathF.PI / 180f * q;
        int segs = 24, sd = seed;
        var line = new List<Vector2>();
        for (int i = 0; i <= segs; i++)
        {
            float y = b.Top + h * i / segs;
            float x = f.C.X + (FxKit.Hash(sd * 131 + i) - 0.5f) * 2f * jag + MathF.Sin(i * 0.7f + sd) * jag * 0.5f;
            line.Add(new Vector2(x, y));
        }
        var left = new List<Vector2> { new(b.Left, b.Top) };
        left.AddRange(line);
        left.Add(new Vector2(b.Left, b.Bottom));
        var right = new List<Vector2> { new(b.Right, b.Top) };
        right.AddRange(line);
        right.Add(new Vector2(b.Right, b.Bottom));
        var col = FxKit.Rgb(edge);
        float fade = Math.Clamp(1.4f - q * 1.4f, 0f, 1f);
        var pivotL = new Vector2(b.Left, b.Bottom); var pivotR = new Vector2(b.Right, b.Bottom);
        var big = new RawRectF(b.Left - w, b.Top - h, b.Right + w, b.Bottom + h);
        return k.Draw(big, dc =>
        {
            var brush = k.ImageBrush(dc, input, b, fade);
            using var edgeBrush = dc.CreateSolidColorBrush(FxDraw.A(col, fade));
            dc.Transform = Matrix3x2.CreateRotation(-tilt, pivotL) * Matrix3x2.CreateTranslation(-apart, h * 0.15f * q * q);
            YMM4EffectShapes.Gfx.FillPoly(dc, brush, left);
            YMM4EffectShapes.Gfx.Polyline(dc, edgeBrush, line, 3f);
            dc.Transform = Matrix3x2.CreateRotation(tilt, pivotR) * Matrix3x2.CreateTranslation(apart, h * 0.15f * q * q);
            YMM4EffectShapes.Gfx.FillPoly(dc, brush, right);
            YMM4EffectShapes.Gfx.Polyline(dc, edgeBrush, line, 3f);
            dc.Transform = Matrix3x2.Identity;
        }, clip: false);
    }
}

public enum BlindDirection
{
    [Display(Name = "横の帯")] Horizontal,
    [Display(Name = "縦の帯")] Vertical,
}

[VideoEffect("ブラインド", ["演出：出入り"], ["ブラインド", "blinds", "帯", "しま", "切り替え", "場面転換", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class BlindsEffect : FxInOutBase
{
    public override string Label => "ブラインド";

    [Display(GroupName = "ブラインド", Name = "帯の向き")]
    [EnumComboBox]
    public BlindDirection Direction { get => dir; set => Set(ref dir, value); }
    BlindDirection dir = BlindDirection.Horizontal;

    [Display(GroupName = "ブラインド", Name = "帯の数")]
    [TextBoxSlider("F0", "本", 2, 40)]
    [Range(1, 200)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 10;

    [Display(GroupName = "ブラインド", Name = "ずらして開く", Description = "帯ごとに開くタイミングをずらす量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Stagger { get; } = new Animation(40, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [.. BaseAnimatables, Stagger];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float p = Visible(f);
        if (p >= 0.999f) return input;
        int n = Math.Max(1, count);
        float st = f.V(Stagger) / 100f;
        var b = f.B; float w = f.W, h = f.H; bool horiz = dir == BlindDirection.Horizontal;
        var mask = k.Draw(b, dc =>
        {
            using var br = dc.CreateSolidColorBrush(new Color4(1, 1, 1, 1));
            for (int i = 0; i < n; i++)
            {
                float delay = n > 1 ? st * i / (n - 1) : 0f;
                float pi = Math.Clamp((p - delay) / MathF.Max(0.01f, 1f - st), 0f, 1f);
                if (pi <= 0) continue;
                if (horiz)
                {
                    float y0 = b.Top + h * i / n, band = h / n;
                    dc.FillRectangle(new RawRectF(b.Left, y0, b.Right, y0 + band * pi), br);
                }
                else
                {
                    float x0 = b.Left + w * i / n, band = w / n;
                    dc.FillRectangle(new RawRectF(x0, b.Top, x0 + band * pi, b.Bottom), br);
                }
            }
        });
        return k.Comp("in", input, mask, CompositeMode.DestinationIn);
    }
}

public enum WipeDirection
{
    [Display(Name = "左から")] FromLeft,
    [Display(Name = "右から")] FromRight,
    [Display(Name = "上から")] FromTop,
    [Display(Name = "下から")] FromBottom,
    [Display(Name = "まん中から(円)")] Circle,
    [Display(Name = "まん中から(横に開く)")] Center,
}

[VideoEffect("ワイプで出る", ["演出：出入り"], ["ワイプ", "wipe", "スライド", "現れる", "消える", "場面転換", "登場", "退場", "演出"], IsAviUtlSupported = false)]
public sealed class WipeEffect : FxInOutBase
{
    public override string Label => "ワイプで出る";

    [Display(GroupName = "ワイプ", Name = "向き")]
    [EnumComboBox]
    public WipeDirection Direction { get => dir; set => Set(ref dir, value); }
    WipeDirection dir = WipeDirection.FromLeft;

    [Display(GroupName = "ワイプ", Name = "境目のぼかし")]
    [AnimationSlider("F0", "%", 0, 50)]
    public Animation Soft { get; } = new Animation(10, 0, 100);

    protected override IEnumerable<IAnimatable> GetAnimatables() => [.. BaseAnimatables, Soft];

    internal override ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f)
    {
        float p = Visible(f);
        if (p >= 0.999f) return input;
        float soft = MathF.Max(0.001f, f.V(Soft) / 100f);
        // 境目のぼかしの分だけ、少し先まで進める(p=0 で完全に隠れ、p=1 で完全に見える)
        float edge = p * (1f + soft);
        float lo = Math.Clamp(edge - soft, 0f, 1f), hi = Math.Clamp(edge, 0.0001f, 1f);
        if (hi <= lo) hi = MathF.Min(1f, lo + 0.0001f);
        var b = f.B; var c = f.C; var d = dir;
        var on = new Color4(1, 1, 1, 1); var off = new Color4(1, 1, 1, 0);
        var mask = k.Draw(b, dc =>
        {
            switch (d)
            {
                case WipeDirection.FromLeft: FxDraw.LinearRect(dc, b, new Vector2(b.Left, 0), new Vector2(b.Right, 0), (lo, on), (hi, off)); break;
                case WipeDirection.FromRight: FxDraw.LinearRect(dc, b, new Vector2(b.Right, 0), new Vector2(b.Left, 0), (lo, on), (hi, off)); break;
                case WipeDirection.FromTop: FxDraw.LinearRect(dc, b, new Vector2(0, b.Top), new Vector2(0, b.Bottom), (lo, on), (hi, off)); break;
                case WipeDirection.FromBottom: FxDraw.LinearRect(dc, b, new Vector2(0, b.Bottom), new Vector2(0, b.Top), (lo, on), (hi, off)); break;
                case WipeDirection.Center:
                    FxDraw.LinearRect(dc, new RawRectF(c.X, b.Top, b.Right, b.Bottom), new Vector2(c.X, 0), new Vector2(b.Right, 0), (lo, on), (hi, off));
                    FxDraw.LinearRect(dc, new RawRectF(b.Left, b.Top, c.X, b.Bottom), new Vector2(c.X, 0), new Vector2(b.Left, 0), (lo, on), (hi, off));
                    break;
                default:
                    float r = MathF.Sqrt((b.Right - b.Left) * (b.Right - b.Left) + (b.Bottom - b.Top) * (b.Bottom - b.Top)) / 2f;
                    FxDraw.RadialRect(dc, b, c, r, r, (lo, on), (hi, off));
                    break;
            }
        });
        return k.Comp("in", input, mask, CompositeMode.DestinationIn);
    }
}
