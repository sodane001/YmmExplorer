using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
//  衝撃のフラッシュ(カメラの揺れ・ズームの光)
//   決めた時刻に、画面が白く光り、まわりに色ずれのふちとスピード線が出る。
//   (図形なので下の映像そのものは揺らせない。揺れは YMM4 の「振動」エフェクトと組み合わせるとよい)
// =====================================================================
public sealed class ImpactFlashPlugin : IShapePlugin
{
    public string Name => "演出：衝撃のフラッシュ";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ImpactFlashParameter(sharedData);
}

public sealed class ImpactFlashParameter : EffectShapeParameterBase
{
    public ImpactFlashParameter() : this(null) { }
    public ImpactFlashParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "タイミング", Name = "光る時刻（秒。カンマ区切りで何回でも）")]
    [TextEditor(AcceptsReturn = false)]
    public string Times { get => times; set => Set(ref times, value); }
    string times = "0";

    [Display(GroupName = "タイミング", Name = "光が消えるまで")]
    [AnimationSlider("F2", "秒", 0.05, 2)]
    public Animation Decay { get; } = new Animation(0.35, 0.01, 60);

    [Display(GroupName = "見た目", Name = "光の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "見た目", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Intensity { get; } = new Animation(80, 0, 100);

    [Display(GroupName = "見た目", Name = "スピード線")]
    [ToggleSlider]
    public bool Lines { get => lines; set => Set(ref lines, value); }
    bool lines = true;

    [Display(GroupName = "見た目", Name = "色ずれのふち（赤・青）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Fringe { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "見た目", Name = "中心X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CenterX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "見た目", Name = "中心Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CenterY { get; } = new Animation(50, -100, 200);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Decay, Intensity, Fringe, CenterX, CenterY];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float decay = MathF.Max(0.01f, c.F(Decay)), inten = c.F(Intensity) / 100f;
        // いちばん近い(直前の)衝撃からの時間
        float since = float.MaxValue;
        int which = 0, idx = 0;
        foreach (var s in TextKit.Split(Times))
        {
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var at) && t >= at && t - at < since) { since = t - at; which = idx; }
            idx++;
        }
        if (since > decay * 2.5f) return;
        float f = Math.Clamp(since / decay, 0f, 1f);
        float a = (1f - f) * (1f - f) * inten;
        var ctr = new Vector2(c.F(CenterX) / 100f * w, c.F(CenterY) / 100f * h);
        float diag = MathF.Sqrt(w * w + h * h);
        using var brush = dc.CreateSolidColorBrush(default);

        // 全体の光(中心ほど強い)
        if (a > 0.002f)
        {
            brush.Color = Gfx.C4(Color, a * 0.6f);
            dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            using var g = Gfx.Radial(dc, ctr, diag * 0.5f, (0f, Gfx.C4(Color, a * 0.6f)), (1f, Gfx.C4(Color, 0f)));
            dc.FillRectangle(new RawRectF(0, 0, w, h), g);
        }
        // 色ずれのふち: 画面の四辺に、赤と青の帯が少しずれて重なる
        float fr = c.F(Fringe) / 100f * MathF.Max(0f, 1f - since / (decay * 2.5f));
        if (fr > 0.002f)
        {
            float bw = MathF.Min(w, h) * 0.08f;
            float shift = bw * 0.25f * (1f - f);
            var red = Gfx.C4(1f, 0.1f, 0.2f, 0.4f * fr);
            var blue = Gfx.C4(0.1f, 0.4f, 1f, 0.4f * fr);
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            foreach (var (col, off) in new[] { (red, -shift), (blue, shift) })
            {
                using var gl = Gfx.Linear(dc, new Vector2(off, 0), new Vector2(off + bw, 0), (0f, col), (1f, Gfx.C4(0f, 0f, 0f, 0f)));
                dc.FillRectangle(new RawRectF(0, 0, bw * 1.5f, h), gl);
                using var gr = Gfx.Linear(dc, new Vector2(w + off, 0), new Vector2(w + off - bw, 0), (0f, col), (1f, Gfx.C4(0f, 0f, 0f, 0f)));
                dc.FillRectangle(new RawRectF(w - bw * 1.5f, 0, w, h), gr);
                using var gt = Gfx.Linear(dc, new Vector2(0, off), new Vector2(0, off + bw), (0f, col), (1f, Gfx.C4(0f, 0f, 0f, 0f)));
                dc.FillRectangle(new RawRectF(0, 0, w, bw * 1.5f), gt);
                using var gb = Gfx.Linear(dc, new Vector2(0, h + off), new Vector2(0, h + off - bw), (0f, col), (1f, Gfx.C4(0f, 0f, 0f, 0f)));
                dc.FillRectangle(new RawRectF(0, h - bw * 1.5f, w, h), gb);
            }
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
        // スピード線(中心から外へ、一瞬だけ)
        if (Lines && since < decay * 1.5f)
        {
            float la = MathF.Max(0f, 1f - since / (decay * 1.5f)) * inten;
            for (int i = 0; i < 60; i++)
            {
                float ang = (i + DrawKit.H(Seed + which, i) * 0.9f) / 60f * MathF.PI * 2f;
                var d = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                float r0 = diag * (0.22f + DrawKit.H(Seed + which, i, 1) * 0.15f + f * 0.1f);
                float r1 = r0 + diag * (0.15f + DrawKit.H(Seed + which, i, 2) * 0.3f);
                brush.Color = Gfx.C4(Color, la * 0.7f);
                dc.DrawLine(ctr + d * r0, ctr + d * r1, brush, MathF.Max(1.5f, diag * 0.002f));
            }
        }
    }
}
