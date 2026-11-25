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
//  VS・比較画面
//   画面を斜めに二分割し、真ん中に「VS」と稲妻。
//   左右の半分は不透明度を下げると、下に置いた画像やキャラが見える
// =====================================================================
public sealed class VsScreenPlugin : IShapePlugin
{
    public string Name => "演出：VS・比較画面";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new VsScreenParameter(sharedData);
}

public sealed class VsScreenParameter : EffectShapeParameterBase
{
    public VsScreenParameter() : this(null) { }
    public VsScreenParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "左側の色")]
    [ColorPicker]
    public WpfColor LeftColor { get => leftColor; set => Set(ref leftColor, value); }
    WpfColor leftColor = WpfColor.FromRgb(0x1E, 0x6E, 0xE8);

    [Display(GroupName = "色", Name = "右側の色")]
    [ColorPicker]
    public WpfColor RightColor { get => rightColor; set => Set(ref rightColor, value); }
    WpfColor rightColor = WpfColor.FromRgb(0xE8, 0x2E, 0x3C);

    [Display(GroupName = "色", Name = "左右の不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SideOpacity { get; } = new Animation(85, 0, 100);

    [Display(GroupName = "色", Name = "稲妻・VS の色")]
    [ColorPicker]
    public WpfColor BoltColor { get => boltColor; set => Set(ref boltColor, value); }
    WpfColor boltColor = WpfColor.FromRgb(0xFF, 0xE0, 0x3C);

    [Display(GroupName = "形", Name = "分かれ目の傾き")]
    [AnimationSlider("F0", "°", -40, 40)]
    public Animation Tilt { get; } = new Animation(14, -80, 80);

    [Display(GroupName = "形", Name = "VS の大きさ")]
    [AnimationSlider("F0", "px", 50, 500)]
    public Animation VsSize { get; } = new Animation(200, 5, 3000);

    [Display(GroupName = "形", Name = "背景のしま模様")]
    [ToggleSlider]
    public bool Stripes { get => stripes; set => Set(ref stripes, value); }
    bool stripes = true;

    [Display(GroupName = "動き", Name = "左右が入ってくる時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation InTime { get; } = new Animation(0.45, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [SideOpacity, Tilt, VsSize, InTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(w / 2f, h / 2f);
        float inT = c.F(InTime);
        float k = inT <= 0.001f ? 1f : DrawKit.EaseOut(t / inT);
        float since = inT <= 0.001f ? t : t - inT;
        float op = c.F(SideOpacity) / 100f;
        float dx = h / 2f * MathF.Tan(Gfx.Rad(c.F(Tilt)));
        float slide = (1f - k) * w * 0.7f;
        float shake = since >= 0f && since < 0.3f ? (1f - since / 0.3f) * 12f : 0f;
        var sh = new Vector2(MathF.Sin(t * 80f) * shake, MathF.Cos(t * 70f) * shake);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            var left = new List<Vector2> { new(-10f - slide, -10f), new(ctr.X + dx - slide, -10f), new(ctr.X - dx - slide, h + 10f), new(-10f - slide, h + 10f) };
            var right = new List<Vector2> { new(ctr.X + dx + slide, -10f), new(w + 10f + slide, -10f), new(w + 10f + slide, h + 10f), new(ctr.X - dx + slide, h + 10f) };
            using (var lg = Gfx.Linear(dc, new Vector2(0, 0), new Vector2(w / 2f, 0), (0f, Gfx.C4(Gfx.Mix(LeftColor, WpfColor.FromRgb(0, 0, 0), 0.4f), op)), (1f, Gfx.C4(LeftColor, op))))
                Gfx.FillPoly(dc, lg, DrawKit.ClipRect(left, 0, 0, w, h));
            using (var lg = Gfx.Linear(dc, new Vector2(w, 0), new Vector2(w / 2f, 0), (0f, Gfx.C4(Gfx.Mix(RightColor, WpfColor.FromRgb(0, 0, 0), 0.4f), op)), (1f, Gfx.C4(RightColor, op))))
                Gfx.FillPoly(dc, lg, DrawKit.ClipRect(right, 0, 0, w, h));

            if (Stripes)
            {
                // 流れる細い斜線
                brush.Color = Gfx.C4(white, 0.08f * op);
                float step = 46f;
                float off = Gfx.Mod(t * 120f, step);
                for (float x = -h + off; x < w + h; x += step)
                {
                    var l = DrawKit.ClipRect(new List<Vector2> { new(x, 0), new(x + 14f, 0), new(x + 14f - h * 0.6f, h), new(x - h * 0.6f, h) }, 0, 0, w, h);
                    if (l.Count >= 3) Gfx.FillPoly(dc, brush, l);
                }
            }

            if (k < 0.999f) return;

            // ---- 稲妻（分かれ目に沿って）----
            var top = new Vector2(ctr.X + dx, -10f);
            var bot = new Vector2(ctr.X - dx, h + 10f);
            var bolt = new List<Vector2>();
            int segs = 18;
            int fr = (int)(t * 18f);
            for (int i = 0; i <= segs; i++)
            {
                var p = Vector2.Lerp(top, bot, i / (float)segs);
                float jag = (i == 0 || i == segs) ? 0f : (Gfx.Hash01(fr * 131 + i * 17 + Seed) - 0.5f) * 60f;
                bolt.Add(p + new Vector2(jag, 0));
            }
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            brush.Color = Gfx.C4(BoltColor, 0.25f);
            Gfx.Polyline(dc, brush, bolt, 30f);
            brush.Color = Gfx.C4(BoltColor, 0.6f);
            Gfx.Polyline(dc, brush, bolt, 10f);
            brush.Color = Gfx.C4(white, 0.95f);
            Gfx.Polyline(dc, brush, bolt, 3.5f);
            if (since < 0.25f)
            {
                brush.Color = Gfx.C4(white, (1f - since / 0.25f) * 0.7f);
                dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            }
            DrawKit.Glow(dc, ctr + sh, c.F(VsSize) * 1.3f, Gfx.C4(BoltColor, 0.45f));
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;

            // ---- VS ----
            float vs = c.F(VsSize);
            float pop = since < 0f ? 0f : since < 0.12f ? 2.2f - 1.2f * (since / 0.12f) : 1f + 0.08f * MathF.Exp(-(since - 0.12f) * 6f) * MathF.Cos((since - 0.12f) * 25f);
            if (pop > 0f)
            {
                float vh = vs * pop;
                StrokeFont.DrawOutlined(dc, brush, "VS", ctr + sh, vh, MathF.Max(4f, vh * 0.16f), Gfx.C4(BoltColor, 1f), Gfx.C4(0.08f, 0.05f, 0.02f, 1f), vh * 0.36f, 0.1f, 0.2f);
                brush.Color = Gfx.C4(white, 0.85f);
                StrokeFont.DrawCentered(dc, brush, "VS", ctr + sh + new Vector2(-vh * 0.03f, -vh * 0.03f), vh, MathF.Max(1.5f, vh * 0.04f), 0.1f, 0.2f);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
