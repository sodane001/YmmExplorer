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

// =====================================================================
//  デジタルレイン
//   縦に流れる文字風の光の列
// =====================================================================
public enum RainGlyph
{
    [Display(Name = "文字風")] Letters,
    [Display(Name = "0と1")] Binary,
    [Display(Name = "ブロック")] Blocks,
}

public sealed class DigitalRainPlugin : IShapePlugin
{
    public string Name => "演出：デジタルレイン";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DigitalRainParameter(sharedData);
}

public sealed class DigitalRainParameter : EffectShapeParameterBase
{
    public DigitalRainParameter() : this(null) { }
    public DigitalRainParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "見た目", Name = "文字の種類")]
    [EnumComboBox]
    public RainGlyph Glyph { get => glyph; set => Set(ref glyph, value); }
    RainGlyph glyph = RainGlyph.Letters;

    [Display(GroupName = "見た目", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x39, 0xFF, 0x88);

    [Display(GroupName = "見た目", Name = "先頭の色")]
    [ColorPicker]
    public WpfColor HeadColor { get => head; set => Set(ref head, value); }
    WpfColor head = WpfColor.FromRgb(0xE8, 0xFF, 0xF0);

    [Display(GroupName = "見た目", Name = "1マスの大きさ")]
    [AnimationSlider("F0", "px", 10, 60)]
    public Animation CellSize { get; } = new Animation(22, 6, 300);

    [Display(GroupName = "見た目", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "動き", Name = "落ちる速さ")]
    [AnimationSlider("F0", "マス/秒", 2, 40)]
    public Animation Speed { get; } = new Animation(14, 0.5, 200);

    [Display(GroupName = "動き", Name = "尾の長さ")]
    [AnimationSlider("F0", "マス", 4, 50)]
    public Animation Trail { get; } = new Animation(18, 1, 200);

    [Display(GroupName = "動き", Name = "列の密度")]
    [AnimationSlider("F0", "%", 5, 100)]
    public Animation Density { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "動き", Name = "文字が変わる速さ")]
    [AnimationSlider("F0", "回/秒", 0, 30)]
    public Animation ChangeRate { get; } = new Animation(6, 0, 200);

    [Display(GroupName = "動き", Name = "1列あたりの流れの数", Description = "多いほど隙間なく降り続ける。流れごとに速さ・間隔・長さがバラバラになる")]
    [AnimationSlider("F0", "本", 1, 6)]
    public Animation Streams { get; } = new Animation(3, 1, 20);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [CellSize, Opacity, Speed, Trail, Density, ChangeRate, Streams];

    static void DrawGlyph(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, RainGlyph kind, Random rg,
        Vector2 center, float cell, float lineW)
    {
        float s = cell * 0.7f;
        switch (kind)
        {
            case RainGlyph.Blocks:
                Gfx.Rect(dc, brush, center.X - s / 2f, center.Y - s / 2f, s, s);
                break;
            case RainGlyph.Binary:
                if (rg.NextF() < 0.5f)
                {
                    dc.DrawEllipse(new Ellipse(center, s * 0.28f, s * 0.42f), brush, lineW);
                }
                else
                {
                    dc.DrawLine(new Vector2(center.X, center.Y - s * 0.42f), new Vector2(center.X, center.Y + s * 0.42f), brush, lineW);
                    dc.DrawLine(new Vector2(center.X, center.Y - s * 0.42f), new Vector2(center.X - s * 0.2f, center.Y - s * 0.24f), brush, lineW);
                }
                break;
            default:
                {
                    // 3×4の格子点から2〜4本の線を引いて、文字のような形にする
                    int segs = 2 + rg.Next(0, 3);
                    for (int i = 0; i < segs; i++)
                    {
                        float x0 = (rg.Next(0, 3) - 1) * s * 0.28f, y0 = (rg.Next(0, 4) - 1.5f) * s * 0.28f;
                        float x1 = (rg.Next(0, 3) - 1) * s * 0.28f, y1 = (rg.Next(0, 4) - 1.5f) * s * 0.28f;
                        if (x0 == x1 && y0 == y1) y1 += s * 0.28f;
                        dc.DrawLine(center + new Vector2(x0, y0), center + new Vector2(x1, y1), brush, lineW);
                    }
                    break;
                }
        }
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float cell = MathF.Max(6f, c.F(CellSize));
        float op = c.F(Opacity) / 100f, speed = c.F(Speed), trail = c.F(Trail);
        float density = c.F(Density) / 100f, rate = c.F(ChangeRate);
        int cols = (int)MathF.Ceiling(w / cell), rows = (int)MathF.Ceiling(h / cell);
        float lineW = MathF.Max(1f, cell * 0.07f);

        using var brush = dc.CreateSolidColorBrush(default);
        int streams = Math.Clamp((int)c.F(Streams), 1, 6);
        for (int col = 0; col < cols; col++)
        {
            var rc = new Random(Seed * 977 + col * 131);
            float colSpeed = 0.5f + 1.0f * rc.NextF();
            float chPh = rc.NextF() * 10f;

            // 1列に複数の流れを持たせ、流れごとに速さ・間隔・長さを変える（規則的な並びや一定間隔の空白をなくす）
            for (int st = 0; st < streams; st++)
            {
                float sf = colSpeed * (0.7f + 0.6f * rc.NextF());
                float phase = rc.NextF();
                float gap = rows * (0.15f + 1.1f * rc.NextF());          // 流れの間隔は流れごとにバラバラ
                float maxTrail = trail * 1.5f;
                float cycle = rows + maxTrail + gap;

                float pos = t * speed * sf + phase * cycle;
                int pass = (int)MathF.Floor(pos / cycle);
                float headPos = pos - pass * cycle;

                // この周回で現れるか・尾の長さは、周回ごとに抽選し直す
                var rp = new Random(unchecked(Seed * 7919 + col * 104729 + st * 1299709 + pass * 15485863));
                if (rp.NextF() > density) continue;
                float tl = MathF.Max(2f, trail * (0.35f + 1.15f * rp.NextF()));

                int headRow = (int)MathF.Floor(headPos);
                int len = (int)MathF.Ceiling(tl);
                for (int k = 0; k < len; k++)
                {
                    int row = headRow - k;
                    if (row < 0 || row >= rows) continue;

                    float f = k / tl;
                    float alpha = MathF.Pow(MathF.Max(0f, 1f - f), 1.5f) * op;
                    if (alpha <= 0.01f) continue;

                    int tick = (int)MathF.Floor(t * rate + chPh + row * 0.37f);
                    int hash = unchecked(col * 73856093 ^ row * 19349663 ^ tick * 83492791 ^ Seed * 2654435 ^ st * 40503);
                    var rg = new Random(hash);

                    var center = new Vector2((col + 0.5f) * cell, (row + 0.5f) * cell);
                    if (k == 0)
                    {
                        brush.Color = Gfx.C4(Color, 0.28f * op);
                        dc.FillEllipse(new Ellipse(center, cell * 0.8f, cell * 0.8f), brush);
                        brush.Color = Gfx.C4(HeadColor, op);
                    }
                    else
                    {
                        brush.Color = Gfx.C4(Color, alpha);
                    }
                    DrawGlyph(dc, brush, Glyph, rg, center, cell, lineW);
                }
            }
        }
    }
}
