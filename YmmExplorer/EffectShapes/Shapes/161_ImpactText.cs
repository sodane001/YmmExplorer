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
//  ドン！(文字のたたきつけ)
//   大きな文字が画面にたたきつけられる。集中線・光・破片・文字のゆれつき
// =====================================================================
public sealed class ImpactTextPlugin : IShapePlugin
{
    public string Name => "演出：ドン！（文字のたたきつけ）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ImpactTextParameter(sharedData);
}

public sealed class ImpactTextParameter : EffectShapeParameterBase
{
    public ImpactTextParameter() : this(null) { }
    public ImpactTextParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "文字")]
    [TextEditor(AcceptsReturn = true)]
    public string Text { get => text; set => Set(ref text, value); }
    string text = "ドン！";

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "文字", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 40, 600)]
    public Animation FontSize { get; } = new Animation(260, 4, 5000);

    [Display(GroupName = "文字", Name = "文字の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0xFF, 0xE1, 0x3A);

    [Display(GroupName = "文字", Name = "ふちの色")]
    [ColorPicker]
    public WpfColor OutlineColor { get => outlineColor; set => Set(ref outlineColor, value); }
    WpfColor outlineColor = WpfColor.FromRgb(0xC6, 0x10, 0x1E);

    [Display(GroupName = "文字", Name = "ふちの太さ")]
    [AnimationSlider("F0", "px", 0, 40)]
    public Animation OutlineWidth { get; } = new Animation(14, 0, 300);

    [Display(GroupName = "文字", Name = "傾き")]
    [AnimationSlider("F0", "°", -30, 30)]
    public Animation Tilt { get; } = new Animation(-6, -360, 360);

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(48, -100, 200);

    [Display(GroupName = "効果", Name = "集中線")]
    [ToggleSlider]
    public bool SpeedLines { get => speedLines; set => Set(ref speedLines, value); }
    bool speedLines = true;

    [Display(GroupName = "効果", Name = "たたきつけた瞬間に光る")]
    [ToggleSlider]
    public bool Flash { get => flash; set => Set(ref flash, value); }
    bool flash = true;

    [Display(GroupName = "効果", Name = "破片が飛ぶ")]
    [ToggleSlider]
    public bool Debris { get => debris; set => Set(ref debris, value); }
    bool debris = true;

    [Display(GroupName = "効果", Name = "たたきつけるまでの時間")]
    [AnimationSlider("F2", "秒", 0.05, 1)]
    public Animation SlamTime { get; } = new Animation(0.18, 0.01, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [FontSize, OutlineWidth, Tilt, PosX, PosY, SlamTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float outK = DrawKit.InOut(c, 0f, 0.25f);
        if (outK <= 0.001f) return;
        float slam = MathF.Max(0.01f, c.F(SlamTime));
        float fs = c.F(FontSize), ow = c.F(OutlineWidth);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float after = t - slam;
        using var brush = dc.CreateSolidColorBrush(default);

        // 集中線(たたきつけた後)
        if (SpeedLines && after > 0f)
        {
            float la = outK * MathF.Min(1f, after / 0.05f) * (0.75f + 0.25f * MathF.Sin(t * 40f));
            float far = MathF.Sqrt(w * w + h * h);
            int n = 70;
            int frame = (int)(t * 20f);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.75f * la);
            for (int i = 0; i < n; i++)
            {
                float ang = (i + DrawKit.H(Seed + frame, i) * 0.8f) / n * MathF.PI * 2f;
                float inner = MathF.Min(w, h) * (0.32f + DrawKit.H(Seed + frame, i, 1) * 0.12f);
                float wid = MathF.Min(w, h) * (0.006f + DrawKit.H(Seed + frame, i, 2) * 0.012f);
                var d = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                var nn = new Vector2(-d.Y, d.X);
                Gfx.FillPoly(dc, brush, new List<Vector2> { ctr + d * inner, ctr + d * far + nn * wid, ctr + d * far - nn * wid });
            }
        }
        // 光
        if (Flash && after > 0f && after < 0.3f)
        {
            float fa = (1f - after / 0.3f) * outK;
            brush.Color = Gfx.C4(1f, 1f, 1f, 0.6f * fa);
            dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            DrawKit.Glow(dc, ctr, fs * 2.5f * (1f + after * 3f), Gfx.C4(Color, 0.8f * fa));
        }
        // 破片
        if (Debris && after > 0f)
        {
            for (int i = 0; i < 24; i++)
            {
                float ang = DrawKit.H(Seed, i, 3) * MathF.PI * 2f;
                float sp = fs * (2.5f + DrawKit.H(Seed, i, 4) * 4f);
                float life = 0.6f + DrawKit.H(Seed, i, 5) * 0.5f;
                if (after > life) continue;
                var p = ctr + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * sp * after + new Vector2(0f, 900f * after * after);
                float sz = fs * (0.04f + DrawKit.H(Seed, i, 6) * 0.06f) * (1f - after / life);
                brush.Color = Gfx.C4(i % 3 == 0 ? OutlineColor : Color, outK);
                Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(p, sz, 3, 0.5f, after * 10f + i));
            }
        }
        // 文字: 大きく・薄く始まり、一気に縮んで、たたきつけた後は少しゆれて弾む
        float sc, alpha;
        if (t < slam)
        {
            float p = t / slam;
            sc = 3.2f - 2.2f * p * p;
            alpha = p;
        }
        else
        {
            sc = 1f + 0.18f * MathF.Exp(-after * 9f) * MathF.Cos(after * 30f);
            alpha = 1f;
        }
        alpha *= outK;
        var shake = after > 0f && after < 0.35f ? new Vector2(DrawKit.H(Seed, (int)(t * 60f)) - 0.5f, DrawKit.H(Seed, (int)(t * 60f), 1) - 0.5f) * fs * 0.12f * (1f - after / 0.35f) : Vector2.Zero;
        var s = TextKit.Get(TextKit.Lines(Text), fs, true, Font, 0f, 1);
        var prev = dc.Transform;
        dc.Transform = Matrix3x2.CreateRotation(Gfx.Rad(c.F(Tilt)), ctr) * prev;
        // 文字の後ろの、太い影ふち
        TextKit.DrawCentered(dc, brush, s, ctr + shake + new Vector2(fs * 0.04f, fs * 0.06f), Gfx.C4(0f, 0f, 0f, 0.7f * alpha), Gfx.C4(0f, 0f, 0f, 0.7f * alpha), ow * 1.6f, sc);
        TextKit.DrawCentered(dc, brush, s, ctr + shake, Gfx.C4(Color, alpha), Gfx.C4(OutlineColor, alpha), ow, sc);
        dc.Transform = prev;
    }
}
