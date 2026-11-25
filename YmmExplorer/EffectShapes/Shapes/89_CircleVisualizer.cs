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
//  円形ビジュアライザー
//   円の周りに音声バーが並び、テンポに合わせて脈打つ（音には連動しない演出）
// =====================================================================
public sealed class CircleVisualizerPlugin : IShapePlugin
{
    public string Name => "演出：円形ビジュアライザー";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CircleVisualizerParameter(sharedData);
}

public sealed class CircleVisualizerParameter : EffectShapeParameterBase
{
    public CircleVisualizerParameter() : this(null) { }
    public CircleVisualizerParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "色", Name = "色1")]
    [ColorPicker]
    public WpfColor Color1 { get => color1; set => Set(ref color1, value); }
    WpfColor color1 = WpfColor.FromRgb(0x39, 0xE8, 0xFF);

    [Display(GroupName = "色", Name = "色2")]
    [ColorPicker]
    public WpfColor Color2 { get => color2; set => Set(ref color2, value); }
    WpfColor color2 = WpfColor.FromRgb(0xFF, 0x4C, 0xD2);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "円の半径")]
    [AnimationSlider("F0", "px", 30, 500)]
    public Animation Radius { get; } = new Animation(190, 2, 4000);

    [Display(GroupName = "バー", Name = "バーの数")]
    [AnimationSlider("F0", "本", 8, 240)]
    public Animation Bars { get; } = new Animation(96, 3, 1000);

    [Display(GroupName = "バー", Name = "バーの長さ（最大）")]
    [AnimationSlider("F0", "px", 10, 400)]
    public Animation BarLength { get; } = new Animation(130, 1, 4000);

    [Display(GroupName = "バー", Name = "バーの太さ")]
    [AnimationSlider("F1", "px", 1, 30)]
    public Animation BarWidth { get; } = new Animation(6, 0.5, 300);

    [Display(GroupName = "バー", Name = "内側にも伸ばす")]
    [ToggleSlider]
    public bool Mirror { get => mirror; set => Set(ref mirror, value); }
    bool mirror = false;

    [Display(GroupName = "バー", Name = "左右対称にする")]
    [ToggleSlider]
    public bool Symmetric { get => symmetric; set => Set(ref symmetric, value); }
    bool symmetric = true;

    [Display(GroupName = "動き", Name = "テンポ（BPM）")]
    [AnimationSlider("F0", "", 40, 220)]
    public Animation Bpm { get; } = new Animation(120, 1, 999);

    [Display(GroupName = "動き", Name = "はねる強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Energy { get; } = new Animation(100, 0, 1000);

    [Display(GroupName = "動き", Name = "回転の速さ")]
    [AnimationSlider("F0", "°/秒", -90, 90)]
    public Animation RotSpeed { get; } = new Animation(8, -3600, 3600);

    [Display(GroupName = "装飾", Name = "中央の円を脈打たせる")]
    [ToggleSlider]
    public bool Center { get => center; set => Set(ref center, value); }
    bool center = true;

    [Display(GroupName = "装飾", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glow { get; } = new Animation(100, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Radius, Bars, BarLength, BarWidth, Bpm, Energy, RotSpeed, Glow];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius), L = c.F(BarLength), bw = c.F(BarWidth), glow = c.F(Glow) / 100f, energy = c.F(Energy) / 100f;
        int n = Math.Max(3, (int)c.F(Bars));
        float beatT = t * c.F(Bpm) / 60f;
        float beat = DrawKit.Frac(beatT);
        float env = MathF.Exp(-beat * 5f);
        float env2 = MathF.Exp(-DrawKit.Frac(beatT * 2f + 0.5f) * 7f) * 0.5f;
        float rot = Gfx.Rad(c.F(RotSpeed) * t) - MathF.PI / 2f;
        float step = MathF.Floor(t * 15f), sub = t * 15f - step;

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            dc.PrimitiveBlend = PrimitiveBlend.Add;

            if (Center)
            {
                float cr = R * (0.9f + 0.08f * env);
                DrawKit.Glow(dc, ctr, cr, Gfx.C4(Color1, 0.28f * glow * (0.6f + 0.4f * env)));
                brush.Color = Gfx.C4(Gfx.Mix(Color1, Color2, 0.5f), 0.7f);
                dc.DrawEllipse(new Ellipse(ctr, R * (0.97f + 0.05f * env), R * (0.97f + 0.05f * env)), brush, 3f);
                brush.Color = Gfx.C4(Color2, 0.35f);
                FormKit.Dash(dc, brush, Gfx.ArcPts(ctr, R * 0.85f, -t * 0.6f, -t * 0.6f + MathF.PI * 2f, 120), 10f, 8f, 0f, 2f);
            }

            for (int i = 0; i < n; i++)
            {
                int idx = Symmetric ? Math.Min(i, n - i) : i;
                float f = idx / (float)(Symmetric ? n / 2f : n);   // 0=低い音 〜 1=高い音
                // 疑似スペクトル：低音ほど拍で大きく、高音は細かく揺れる
                float n0 = Gfx.Hash01(idx * 97 + (int)step * 13 + Seed), n1 = Gfx.Hash01(idx * 97 + ((int)step + 1) * 13 + Seed);
                float noise = n0 + (n1 - n0) * sub;
                float wave = 0.5f + 0.5f * MathF.Sin(t * 3.1f + idx * 0.37f) * MathF.Sin(t * 1.7f - idx * 0.21f);
                float v = (1f - f) * (0.35f + 0.65f * env) * 0.8f + f * (0.25f + 0.5f * noise) + 0.25f * wave + env2 * (1f - f) * 0.5f;
                v = Math.Clamp(v * (0.35f + 0.65f * energy), 0.04f, 1.2f);
                float len = L * v;
                float a = rot + i * MathF.PI * 2f / n;
                var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                var col = Gfx.Mix(Color1, Color2, f);
                var p0 = ctr + d * (R + bw);
                var p1 = p0 + d * len;
                if (Mirror) p0 = ctr + d * (R - len * 0.35f);
                if (glow > 0f)
                {
                    brush.Color = Gfx.C4(col, 0.22f * glow);
                    dc.DrawLine(p0, p1, brush, bw * 2.6f);
                }
                brush.Color = Gfx.C4(col, 0.95f);
                dc.DrawLine(p0, p1, brush, bw);
                brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.6f), 0.9f);
                dc.FillEllipse(new Ellipse(p1, bw * 0.55f, bw * 0.55f), brush);
            }

            // 外側を回る点
            for (int i = 0; i < 36; i++)
            {
                float a = -rot * 0.5f + i * MathF.PI * 2f / 36f;
                var p = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (R + L * 1.25f + 10f * env);
                brush.Color = Gfx.C4(Gfx.Mix(Color1, Color2, i / 36f), 0.5f);
                dc.FillEllipse(new Ellipse(p, 2f, 2f), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
