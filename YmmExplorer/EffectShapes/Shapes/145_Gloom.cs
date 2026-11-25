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

public enum GloomKind
{
    [Display(Name = "ガーン（縦線）")] Lines,
    [Display(Name = "ずーん（暗い雲）")] Cloud,
    [Display(Name = "両方")] Both,
}

// =====================================================================
//  ガーン・ずーん(落ち込み)
//   顔に縦線が降りる「ガーン」と、頭の上に暗い雲がかかる「ずーん」
// =====================================================================
public sealed class GloomPlugin : IShapePlugin
{
    public string Name => "演出：ガーン・ずーん（落ち込み）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GloomParameter(sharedData);
}

public sealed class GloomParameter : EffectShapeParameterBase
{
    public GloomParameter() : this(null) { }
    public GloomParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "種類")]
    [EnumComboBox]
    public GloomKind Kind { get => kind; set => Set(ref kind, value); }
    GloomKind kind = GloomKind.Both;

    [Display(GroupName = "形", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x2B, 0x1E, 0x5C);

    [Display(GroupName = "形", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Strength { get; } = new Animation(80, 0, 100);

    [Display(GroupName = "形", Name = "縦線の数")]
    [TextBoxSlider("F0", "本", 3, 40)]
    [Range(1, 200)]
    public int LineCount { get => lineCount; set => Set(ref lineCount, value); }
    int lineCount = 14;

    [Display(GroupName = "形", Name = "雲から雨を降らす")]
    [ToggleSlider]
    public bool Rain { get => rain; set => Set(ref rain, value); }
    bool rain = true;

    [Display(GroupName = "位置と大きさ", Name = "X位置（頭の中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（頭のてっぺん）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(25, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "幅")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation AreaWidth { get; } = new Animation(460, 10, 8000);

    [Display(GroupName = "位置と大きさ", Name = "縦線の長さ")]
    [AnimationSlider("F0", "px", 100, 1200)]
    public Animation AreaHeight { get; } = new Animation(420, 10, 8000);

    [Display(GroupName = "動き", Name = "降りる時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation InTime { get; } = new Animation(0.5, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1)]
    public Animation OutTime { get; } = new Animation(0.3, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Strength, PosX, PosY, AreaWidth, AreaHeight, InTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float inT = c.F(InTime), outT = c.F(OutTime);
        float kin = inT <= 0.001f ? 1f : DrawKit.EaseOut(t / inT);
        float kout = outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f);
        float str = c.F(Strength) / 100f * kout;
        if (kin <= 0.001f || str <= 0.002f) return;
        var top = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float aw = c.F(AreaWidth), ah = c.F(AreaHeight);
        using var brush = dc.CreateSolidColorBrush(default);

        if (Kind != GloomKind.Cloud)
        {
            // 顔の上半分に、上から暗い影
            float shade = ah * 0.6f * kin;
            using (var g = Gfx.Linear(dc, new Vector2(top.X, top.Y), new Vector2(top.X, top.Y + shade), (0f, Gfx.C4(Color, 0.55f * str)), (1f, Gfx.C4(Color, 0f))))
                dc.FillEllipse(new Ellipse(new Vector2(top.X, top.Y + shade * 0.3f), aw * 0.5f, shade * 0.75f), g);
            // 縦線: 上から順に降りてくる。下の方ほど薄い
            int n = Math.Max(1, LineCount);
            for (int i = 0; i < n; i++)
            {
                float f = n == 1 ? 0.5f : i / (float)(n - 1);
                float x = top.X - aw / 2f + aw * f + (DrawKit.H(Seed, i) - 0.5f) * aw / n * 0.6f;
                float len = ah * (0.6f + DrawKit.H(Seed, i, 1) * 0.4f) * (0.8f + 0.2f * MathF.Sin(f * MathF.PI));
                float delay = DrawKit.H(Seed, i, 2) * 0.35f;
                float grow = Math.Clamp((kin - delay) / (1f - delay), 0f, 1f);
                if (grow <= 0f) continue;
                float y0 = top.Y + ah * 0.02f;
                float y1 = y0 + len * grow;
                float lw = MathF.Max(2f, aw * 0.008f) * (i % 3 == 0 ? 1.6f : 1f);
                using (var g = Gfx.Linear(dc, new Vector2(x, y0), new Vector2(x, y1), (0f, Gfx.C4(Color, str)), (1f, Gfx.C4(Color, 0f))))
                    dc.DrawLine(new Vector2(x, y0), new Vector2(x, y1), g, lw);
            }
        }

        if (Kind != GloomKind.Lines)
        {
            // 頭の上の暗い雲(ゆっくりうごめく)
            float drop = (1f - kin) * aw * 0.3f;
            var cc = new Vector2(top.X + MathF.Sin(t * 0.8f) * aw * 0.03f, top.Y - aw * 0.22f - drop);
            for (int layer = 0; layer < 2; layer++)
            {
                var col = layer == 0 ? Gfx.Mix(Color, WpfColor.FromRgb(0, 0, 0), 0.3f) : Gfx.Mix(Color, WpfColor.FromRgb(0x88, 0x88, 0xA0), 0.25f);
                var cloud = BubbleKit.CloudPts(cc + new Vector2(0f, layer * -aw * 0.03f), aw * (0.42f - layer * 0.05f), aw * (0.16f - layer * 0.02f), 7, 0.22f + 0.03f * MathF.Sin(t * 2f + layer));
                brush.Color = Gfx.C4(col, str * (layer == 0 ? 0.95f : 0.9f));
                Gfx.FillPoly(dc, brush, cloud);
            }
            // 雨
            if (Rain)
            {
                brush.Color = Gfx.C4(Gfx.Mix(Color, WpfColor.FromRgb(0xB0, 0xC8, 0xFF), 0.5f), 0.8f * str);
                for (int i = 0; i < 14; i++)
                {
                    float x = cc.X + (DrawKit.H(Seed, i, 5) - 0.5f) * aw * 0.7f;
                    float speed = 0.9f + DrawKit.H(Seed, i, 6) * 0.6f;
                    float f = DrawKit.Frac(t * speed + DrawKit.H(Seed, i, 7));
                    float y = cc.Y + aw * 0.1f + f * ah * 0.55f;
                    dc.DrawLine(new Vector2(x, y), new Vector2(x - aw * 0.008f, y + aw * 0.045f), brush, MathF.Max(1.5f, aw * 0.006f));
                }
            }
        }
    }
}
