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
//  ゲームUI（ライフ・スコア・コンボ）
//   ハートの残機、スコアの数字、コンボのカウンター。
//   値はキーフレームで動かせます（減った・増えた瞬間に反応します）
// =====================================================================
public sealed class GameUiPlugin : IShapePlugin
{
    public string Name => "演出：ゲームUI（ライフ・スコア・コンボ）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GameUiParameter(sharedData);
}

public sealed class GameUiParameter : EffectShapeParameterBase
{
    public GameUiParameter() : this(null) { }
    public GameUiParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "ライフ", Name = "ライフを表示する")]
    [ToggleSlider]
    public bool ShowLife { get => showLife; set => Set(ref showLife, value); }
    bool showLife = true;

    [Display(GroupName = "ライフ", Name = "ライフ（0.5 で半分のハート）")]
    [AnimationSlider("F1", "", 0, 10)]
    public Animation Life { get; } = new Animation(3, 0, 100);

    [Display(GroupName = "ライフ", Name = "最大ライフ")]
    [AnimationSlider("F0", "", 1, 10)]
    public Animation MaxLife { get; } = new Animation(5, 1, 100);

    [Display(GroupName = "ライフ", Name = "ハートの大きさ")]
    [AnimationSlider("F0", "px", 16, 120)]
    public Animation HeartSize { get; } = new Animation(44, 2, 1000);

    [Display(GroupName = "ライフ", Name = "ハートの色")]
    [ColorPicker]
    public WpfColor HeartColor { get => heartColor; set => Set(ref heartColor, value); }
    WpfColor heartColor = WpfColor.FromRgb(0xFF, 0x3C, 0x5A);

    [Display(GroupName = "ライフ", Name = "X位置（左上）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LifeX { get; } = new Animation(4, -100, 200);

    [Display(GroupName = "ライフ", Name = "Y位置（左上）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LifeY { get; } = new Animation(7, -100, 200);

    [Display(GroupName = "スコア", Name = "スコアを表示する")]
    [ToggleSlider]
    public bool ShowScore { get => showScore; set => Set(ref showScore, value); }
    bool showScore = true;

    [Display(GroupName = "スコア", Name = "スコア")]
    [AnimationSlider("F0", "", 0, 999999)]
    public Animation Score { get; } = new Animation(12450, 0, 99999999);

    [Display(GroupName = "スコア", Name = "けた数")]
    [AnimationSlider("F0", "けた", 1, 10)]
    public Animation ScoreDigits { get; } = new Animation(8, 1, 12);

    [Display(GroupName = "スコア", Name = "X位置（右上）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ScoreX { get; } = new Animation(96, -100, 200);

    [Display(GroupName = "スコア", Name = "Y位置（右上）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ScoreY { get; } = new Animation(7, -100, 200);

    [Display(GroupName = "コンボ", Name = "コンボを表示する")]
    [ToggleSlider]
    public bool ShowCombo { get => showCombo; set => Set(ref showCombo, value); }
    bool showCombo = true;

    [Display(GroupName = "コンボ", Name = "コンボ数")]
    [AnimationSlider("F0", "", 0, 999)]
    public Animation Combo { get; } = new Animation(12, 0, 99999);

    [Display(GroupName = "コンボ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ComboX { get; } = new Animation(84, -100, 200);

    [Display(GroupName = "コンボ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ComboY { get; } = new Animation(40, -100, 200);

    [Display(GroupName = "全体", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 16, 120)]
    public Animation TextSize { get; } = new Animation(44, 2, 1000);

    [Display(GroupName = "全体", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(0xFF, 0xFF, 0xFF);

    [Display(GroupName = "全体", Name = "強調の色")]
    [ColorPicker]
    public WpfColor AccentColor { get => accentColor; set => Set(ref accentColor, value); }
    WpfColor accentColor = WpfColor.FromRgb(0xFF, 0xD2, 0x1E);

    [Display(GroupName = "全体", Name = "スコアとコンボを自動で増やす")]
    [ToggleSlider]
    public bool AutoCount { get => autoCount; set => Set(ref autoCount, value); }
    bool autoCount = false;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Life, MaxLife, HeartSize, LifeX, LifeY, Score, ScoreDigits, ScoreX, ScoreY, Combo, ComboX, ComboY, TextSize];

    /// <summary>値が最後に変わってからの秒数（さかのぼって探す。見つからなければ大きな値）</summary>
    static float SinceChange(Animation a, long frame, long length, int fps, double now, float maxSec, out double before)
    {
        before = now;
        int maxF = Math.Max(1, (int)(maxSec * fps));
        for (int k = 1; k <= maxF; k++)
        {
            long f = frame - k;
            if (f < 0) break;
            double v = a.GetValue(f, length, fps);
            if (Math.Abs(v - now) > 1e-6) { before = v; return (k - 0.5f) / fps; }
        }
        return 999f;
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float ts = c.F(TextSize);
        var outline = Gfx.C4(0.05f, 0.05f, 0.08f, 0.9f);
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            // ---- ライフ ----
            if (ShowLife)
            {
                double life = c.F(Life);
                float since = SinceChange(Life, c.Frame, c.Length, c.Fps, life, 0.6f, out double lifeBefore);
                bool hurt = since < 0.6f && lifeBefore > life;
                bool healed = since < 0.6f && lifeBefore < life;
                int max = Math.Max(1, (int)MathF.Round(c.F(MaxLife)));
                float hs = c.F(HeartSize);
                var org = new Vector2(c.F(LifeX) / 100f * w, c.F(LifeY) / 100f * h);
                var shake = hurt ? new Vector2(MathF.Sin(t * 90f), MathF.Cos(t * 77f)) * hs * 0.12f * (1f - since / 0.6f) : Vector2.Zero;
                bool danger = life <= 1.0 && life > 0;
                float beat = danger ? 1f + 0.12f * MathF.Max(0f, MathF.Sin(t * 9f)) : 1f;
                for (int i = 0; i < max; i++)
                {
                    var p = org + shake + new Vector2(hs * 0.6f + i * hs * 1.25f, hs * 0.55f);
                    float fill = (float)Math.Clamp(life - i, 0.0, 1.0);
                    float hr = hs * 0.55f * (fill > 0f ? beat : 1f);
                    var heart = ShapeGeom.HeartPoly(p, hr, 0f);
                    brush.Color = outline;
                    Gfx.Polyline(dc, brush, ShapeGeom.Closed(heart), hs * 0.16f);
                    brush.Color = Gfx.C4(0.15f, 0.15f, 0.18f, 0.85f);
                    Gfx.FillPoly(dc, brush, heart);
                    if (fill > 0f)
                    {
                        var part = fill >= 0.999f ? heart : DrawKit.ClipRect(heart, p.X - hr * 2f, p.Y - hr * 2f, p.X - hr * 1.05f + 2.1f * hr * fill, p.Y + hr * 2f);
                        brush.Color = Gfx.C4(HeartColor, 1f);
                        if (part.Count >= 3) Gfx.FillPoly(dc, brush, part);
                        brush.Color = Gfx.C4(white, 0.55f);
                        dc.FillEllipse(new Ellipse(p + new Vector2(-hr * 0.4f, -hr * 0.35f), hr * 0.18f, hr * 0.13f), brush);
                    }
                    // 失ったハートが光って消える
                    bool lostNow = hurt && i >= life && i < lifeBefore;
                    if (lostNow)
                    {
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        DrawKit.Glow(dc, p, hs * (1f + since * 2f), Gfx.C4(HeartColor, 0.8f * (1f - since / 0.6f)));
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                    }
                    bool gainedNow = healed && i < life && i >= lifeBefore - 0.001;
                    if (gainedNow)
                    {
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        DrawKit.Twinkle(dc, brush, p, hs * (0.6f + since), 2f, white, 1f - since / 0.6f);
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                    }
                }
                if (hurt)
                {
                    brush.Color = Gfx.C4(1f, 0f, 0f, 0.18f * (1f - since / 0.6f));
                    dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
                }
            }

            // ---- スコア ----
            if (ShowScore)
            {
                double sc = c.F(Score);
                if (AutoCount) sc += Math.Floor(t * 3.0) * 150.0 + Math.Floor(t * 0.5) * 1000.0;
                float since = AutoCount ? (float)(t * 3.0 - Math.Floor(t * 3.0)) / 3f : SinceChange(Score, c.Frame, c.Length, c.Fps, c.F(Score), 0.3f, out _);
                int digits = Math.Clamp((int)MathF.Round(c.F(ScoreDigits)), 1, 12);
                string s = Math.Max(0, (long)Math.Round(sc)).ToString(new string('0', digits), CultureInfo.InvariantCulture);
                float wS = StrokeFont.Width(s, ts, 0.18f);
                var right = new Vector2(c.F(ScoreX) / 100f * w, c.F(ScoreY) / 100f * h);
                float flash = since < 0.3f ? 1f - since / 0.3f : 0f;
                var numCol = Gfx.Mix(TextColor, AccentColor, flash);
                var center = new Vector2(right.X - wS / 2f, right.Y + ts * 1.25f);
                StrokeFont.DrawOutlined(dc, brush, s, center, ts * (1f + 0.08f * flash), MathF.Max(2f, ts * 0.11f), Gfx.C4(numCol, 1f), outline, ts * 0.3f, 0.18f);
                float lh = ts * 0.5f;
                StrokeFont.DrawOutlined(dc, brush, "SCORE", new Vector2(right.X - StrokeFont.Width("SCORE", lh) / 2f, right.Y + lh * 0.2f), lh, MathF.Max(1.5f, lh * 0.13f), Gfx.C4(AccentColor, 1f), outline, lh * 0.35f);
            }

            // ---- コンボ ----
            if (ShowCombo)
            {
                double cb = c.F(Combo);
                float since;
                if (AutoCount) { cb += Math.Floor(t * 2.0); since = (float)(t * 2.0 - Math.Floor(t * 2.0)) / 2f; }
                else since = SinceChange(Combo, c.Frame, c.Length, c.Fps, cb, 0.4f, out _);
                if (cb >= 1)
                {
                    var p = new Vector2(c.F(ComboX) / 100f * w, c.F(ComboY) / 100f * h);
                    float pop = since < 0.4f ? 1f + 0.45f * MathF.Exp(-since * 9f) * MathF.Cos(since * 22f) : 1f;
                    float nh = ts * 1.8f * pop;
                    string s = ((long)Math.Round(cb)).ToString(CultureInfo.InvariantCulture);
                    if (since < 0.25f)
                    {
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        DrawKit.Glow(dc, p, nh * 1.4f, Gfx.C4(AccentColor, 0.5f * (1f - since / 0.25f)));
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                    }
                    StrokeFont.DrawOutlined(dc, brush, s, p, nh, MathF.Max(2.5f, nh * 0.13f), Gfx.C4(AccentColor, 1f), outline, nh * 0.32f, 0.15f, 0.15f);
                    float lh = ts * 0.6f;
                    StrokeFont.DrawOutlined(dc, brush, "COMBO", p + new Vector2(0, nh * 0.5f + lh * 0.9f), lh, MathF.Max(1.5f, lh * 0.14f), Gfx.C4(TextColor, 1f), outline, lh * 0.38f, 0.25f, 0.15f);
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
