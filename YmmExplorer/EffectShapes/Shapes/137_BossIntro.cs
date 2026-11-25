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
//  ボス登場の名前表示
//   画面を横切る帯が走り込み、大きな名前が反対側から飛び込む(格闘ゲーム風)
// =====================================================================
public sealed class BossIntroPlugin : IShapePlugin
{
    public string Name => "演出：ボス登場の名前表示";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BossIntroParameter(sharedData);
}

public sealed class BossIntroParameter : EffectShapeParameterBase
{
    public BossIntroParameter() : this(null) { }
    public BossIntroParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "名前")]
    [TextEditor(AcceptsReturn = false)]
    public string Title { get => title; set => Set(ref title, value); }
    string title = "闇の帝王";

    [Display(GroupName = "文字", Name = "肩書き（小さい文字）")]
    [TextEditor(AcceptsReturn = false)]
    public string SubTitle { get => subTitle; set => Set(ref subTitle, value); }
    string subTitle = "第三の刺客";

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "文字", Name = "名前の大きさ")]
    [AnimationSlider("F0", "px", 40, 300)]
    public Animation FontSize { get; } = new Animation(150, 4, 3000);

    [Display(GroupName = "色", Name = "帯の色")]
    [ColorPicker]
    public WpfColor BandColor { get => bandColor; set => Set(ref bandColor, value); }
    WpfColor bandColor = WpfColor.FromRgb(0xC6, 0x10, 0x1E);

    [Display(GroupName = "色", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "色", Name = "画面を暗くする")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Dim { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "位置", Name = "帯の高さ")]
    [AnimationSlider("F0", "px", 60, 600)]
    public Animation BandHeight { get; } = new Animation(230, 4, 4000);

    [Display(GroupName = "位置", Name = "帯のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(55, -100, 200);

    [Display(GroupName = "位置", Name = "帯の傾き")]
    [AnimationSlider("F0", "°", -20, 20)]
    public Animation Angle { get; } = new Animation(-6, -90, 90);

    [Display(GroupName = "動き", Name = "飛び込む時間")]
    [AnimationSlider("F2", "秒", 0.05, 1.5)]
    public Animation InTime { get; } = new Animation(0.45, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 1.5)]
    public Animation OutTime { get; } = new Animation(0.3, 0, 60);

    [Display(GroupName = "動き", Name = "登場の瞬間に光る")]
    [ToggleSlider]
    public bool Flash { get => flash; set => Set(ref flash, value); }
    bool flash = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [FontSize, Dim, BandHeight, PosY, Angle, InTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float inT = MathF.Max(0.05f, c.F(InTime)), outT = c.F(OutTime);
        float kin = Math.Clamp(t / inT, 0f, 1f);
        float kout = outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f);
        if (kout <= 0.001f) return;
        float bandH = c.F(BandHeight), fs = c.F(FontSize);
        var ctr = new Vector2(w / 2f, c.F(PosY) / 100f * h);
        float rot = Gfx.Rad(c.F(Angle));
        float diag = MathF.Sqrt(w * w + h * h);

        using var brush = dc.CreateSolidColorBrush(default);
        // 画面を暗く
        float dim = c.F(Dim) / 100f * MathF.Min(1f, kin * 2f) * kout;
        if (dim > 0.002f) { brush.Color = Gfx.C4(0f, 0f, 0f, dim); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }

        // 帯: 左から走り込む。退場は上下につぶれる
        float slide = DrawKit.EaseOut(kin);
        float squash = DrawKit.EaseOut(kout);
        float halfH = bandH / 2f * squash;
        float len = diag * 1.2f;
        float x0 = -len / 2f + (1f - slide) * -len, x1 = len / 2f + (1f - slide) * -len;
        var band = DrawKit.Place(new[] { new Vector2(x0, -halfH), new Vector2(x1, -halfH), new Vector2(x1, halfH), new Vector2(x0, halfH) }, ctr, rot);
        using (var g = Gfx.Linear(dc, DrawKit.Rot(ctr + new Vector2(0, -halfH), ctr, rot), DrawKit.Rot(ctr + new Vector2(0, halfH), ctr, rot),
            (0f, Gfx.C4(Gfx.Mix(BandColor, WpfColor.FromRgb(255, 255, 255), 0.2f), 0.96f)), (0.5f, Gfx.C4(BandColor, 0.96f)), (1f, Gfx.C4(Gfx.Mix(BandColor, WpfColor.FromRgb(0, 0, 0), 0.45f), 0.96f))))
            Gfx.FillPoly(dc, g, band);
        // 帯のふちの細い線と、流れる線(スピード感)
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.85f * squash);
        foreach (float yy in new[] { -halfH - bandH * 0.06f, halfH + bandH * 0.06f })
            dc.DrawLine(DrawKit.Rot(ctr + new Vector2(x0, yy), ctr, rot), DrawKit.Rot(ctr + new Vector2(x1, yy), ctr, rot), brush, MathF.Max(2f, bandH * 0.025f));
        for (int i = 0; i < 14; i++)
        {
            float yy = (DrawKit.H(Seed, i) - 0.5f) * halfH * 1.8f;
            float speed = 1.5f + DrawKit.H(Seed, i, 1) * 2f;
            float sx = Gfx.Mod(DrawKit.H(Seed, i, 2) * len - t * len * 0.35f * speed, len) - len / 2f;
            float sl = len * (0.05f + DrawKit.H(Seed, i, 3) * 0.12f);
            brush.Color = Gfx.C4(1f, 1f, 1f, 0.18f * squash);
            dc.DrawLine(DrawKit.Rot(ctr + new Vector2(sx, yy), ctr, rot), DrawKit.Rot(ctr + new Vector2(sx + sl, yy), ctr, rot), brush, MathF.Max(1f, bandH * 0.012f));
        }

        // 名前: 右から、少し遅れて飛び込む(行き過ぎて戻る)
        float kt = Math.Clamp((t - inT * 0.35f) / (inT * 0.8f), 0f, 1f);
        if (kt > 0f && squash > 0.05f)
        {
            float back = DrawKit.EaseOutBack(kt);
            var s = TextKit.Get(TextKit.Lines(Title), fs, true, Font);
            float off = (1f - back) * w * 0.8f;
            var p = DrawKit.Rot(ctr + new Vector2(off, -bandH * 0.02f), ctr, rot);
            // 文字の残像
            if (kt < 1f)
                for (int g = 1; g <= 3; g++)
                    TextKit.DrawCentered(dc, brush, s, p + DrawKit.Rot(new Vector2(g * fs * 0.35f, 0f), Vector2.Zero, rot), Gfx.C4(TextColor, 0.18f / g * squash), scale: squash);
            TextKit.DrawCentered(dc, brush, s, p, Gfx.C4(TextColor, squash), Gfx.C4(0f, 0f, 0f, 0.8f * squash), fs * 0.05f, squash,
                Gfx.C4(0f, 0f, 0f, 0.5f * squash), new Vector2(fs * 0.04f, fs * 0.05f));
            string sub = TextKit.Lines(SubTitle).Trim();
            if (sub.Length > 0)
            {
                var ss = TextKit.Get(sub, fs * 0.3f, true, Font);
                var sp = DrawKit.Rot(ctr + new Vector2(-s.Width / 2f + ss.Width / 2f + off * 1.2f, -s.Height * 0.55f - ss.Height * 0.2f), ctr, rot);
                TextKit.DrawCentered(dc, brush, ss, sp, Gfx.C4(1f, 1f, 1f, squash), Gfx.C4(BandColor, squash), fs * 0.02f, squash);
            }
        }

        // 登場の瞬間の光
        if (Flash)
        {
            float ft = t - inT * 1.05f;
            if (ft > 0f && ft < 0.35f)
            {
                brush.Color = Gfx.C4(1f, 1f, 1f, (1f - ft / 0.35f) * 0.55f);
                dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            }
        }
    }
}
