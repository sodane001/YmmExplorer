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
//  波形・心電図
//   流れる心電図・オシロスコープ・音声バー・スペクトラム
// =====================================================================
public sealed class WaveformPlugin : IShapePlugin
{
    public string Name => "演出：波形・心電図";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WaveformParameter(sharedData);
}

public sealed class WaveformParameter : EffectShapeParameterBase
{
    public WaveformParameter() : this(null) { }
    public WaveformParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "波形", Name = "種類")]
    [EnumComboBox]
    public WaveKind Kind { get => kind; set => Set(ref kind, value); }
    WaveKind kind = WaveKind.Ecg;

    [Display(GroupName = "波形", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x39, 0xFF, 0x88);

    [Display(GroupName = "波形", Name = "色2（音声バーの先端など）")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0xFF, 0xE0, 0x4A);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "波形の幅")]
    [AnimationSlider("F0", "px", 100, 1800)]
    public Animation WaveWidth { get; } = new Animation(900, 20, 8000);

    [Display(GroupName = "位置と大きさ", Name = "波形の高さ")]
    [AnimationSlider("F0", "px", 40, 800)]
    public Animation WaveHeight { get; } = new Animation(300, 10, 6000);

    [Display(GroupName = "波形", Name = "線の太さ（バーの太さ）")]
    [AnimationSlider("F1", "px", 0.5, 20)]
    public Animation Thickness { get; } = new Animation(3, 0.2, 200);

    [Display(GroupName = "動き", Name = "速さ（心拍数・周波数）")]
    [AnimationSlider("F0", "%", 10, 400)]
    public Animation Speed { get; } = new Animation(100, 1, 3000);

    [Display(GroupName = "動き", Name = "振れ幅")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Amplitude { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "見た目", Name = "グリッドを表示する")]
    [ToggleSlider]
    public bool ShowGrid { get => grid; set => Set(ref grid, value); }
    bool grid = true;

    [Display(GroupName = "見た目", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glow { get; } = new Animation(100, 0, 500);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, WaveWidth, WaveHeight, Thickness, Speed, Amplitude, Glow];

    static float Gauss(float x, float mu, float s) => MathF.Exp(-((x - mu) / s) * ((x - mu) / s));

    // 心電図の1拍分（P波・QRS・T波）
    static float Ecg(float x)
        => 0.12f * Gauss(x, 0.18f, 0.04f) - 0.15f * Gauss(x, 0.33f, 0.012f) + 1.0f * Gauss(x, 0.36f, 0.012f)
           - 0.28f * Gauss(x, 0.39f, 0.014f) + 0.26f * Gauss(x, 0.62f, 0.07f);

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float W = c.F(WaveWidth), H = c.F(WaveHeight), th = c.F(Thickness), sp = c.F(Speed) / 100f, amp = c.F(Amplitude) / 100f, glow = c.F(Glow) / 100f;
        float x0 = ctr.X - W / 2f, y0 = ctr.Y - H / 2f, x1 = ctr.X + W / 2f, y1 = ctr.Y + H / 2f;
        var white = WpfColor.FromRgb(255, 255, 255);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- グリッド ----
        if (ShowGrid)
        {
            brush.Color = Gfx.C4(Color, 0.10f);
            for (int i = 0; i <= 20; i++) dc.DrawLine(new Vector2(x0 + W * i / 20f, y0), new Vector2(x0 + W * i / 20f, y1), brush, i % 5 == 0 ? 1.4f : 0.7f);
            for (int i = 0; i <= 8; i++) dc.DrawLine(new Vector2(x0, y0 + H * i / 8f), new Vector2(x1, y0 + H * i / 8f), brush, i % 4 == 0 ? 1.4f : 0.7f);
        }

        switch (Kind)
        {
            case WaveKind.Ecg:
            case WaveKind.Scope:
                {
                    const int N = 260;
                    var pts = new List<Vector2>();
                    for (int i = 0; i <= N; i++)
                    {
                        float xn = i / (float)N;
                        float v;
                        if (Kind == WaveKind.Ecg)
                        {
                            float phase = Gfx.Mod(xn * 3f + t * 1.2f * sp, 1f);               // 左へ流れる
                            v = Ecg(phase) * 0.85f;
                        }
                        else
                        {
                            float ph = xn * 6f * MathF.PI * 2f * 0.5f - t * 3f * sp;
                            v = 0.55f * MathF.Sin(ph) + 0.22f * MathF.Sin(ph * 2.7f + 1f) + 0.1f * MathF.Sin(ph * 6.1f + t);
                        }
                        pts.Add(new Vector2(x0 + W * xn, ctr.Y - v * amp * H * 0.45f));
                    }
                    // 右端ほど明るい（過去ほど薄れる）
                    for (int i = 0; i < N; i++)
                    {
                        float xn = i / (float)N, al = 0.12f + 0.88f * MathF.Pow(xn, 1.6f);
                        brush.Color = Gfx.C4(Color, 0.14f * al * glow);
                        dc.DrawLine(pts[i], pts[i + 1], brush, th * 4f);
                        brush.Color = Gfx.C4(Color, 0.95f * al);
                        dc.DrawLine(pts[i], pts[i + 1], brush, th);
                    }
                    var head = pts[N];
                    brush.Color = Gfx.C4(Color, 0.25f * glow);
                    dc.FillEllipse(new Ellipse(head, th * 4.5f, th * 4.5f), brush);
                    brush.Color = Gfx.C4(white, 0.95f);
                    dc.FillEllipse(new Ellipse(head, th * 1.7f, th * 1.7f), brush);
                    break;
                }
            default:    // 音声バー・スペクトラム
                {
                    bool mirror = Kind == WaveKind.AudioBars;
                    int n = Kind == WaveKind.AudioBars ? 56 : 40;
                    float slot = W / n, bw = slot * 0.62f;
                    for (int i = 0; i < n; i++)
                    {
                        float hsh = Gfx.Hash01(i * 7919 + Seed);
                        float env = mirror ? 0.5f + 0.5f * MathF.Sin(i / (float)n * MathF.PI) : 1f - 0.65f * (i / (float)n);          // スペクトラムは高音ほど小さい
                        float beat = 0.5f + 0.5f * MathF.Sin(t * 6.5f * sp);                                                       // 全体の脈動
                        float a1 = 0.5f + 0.5f * MathF.Sin(t * (2.2f + hsh * 5f) * sp + i * 0.9f);
                        float a2 = 0.5f + 0.5f * MathF.Sin(t * (4.1f + hsh * 3f) * sp + i * 1.7f + 2f);
                        float v = Math.Clamp((0.15f + 0.55f * a1 * a2 + 0.3f * beat * a1) * env * amp, 0.02f, 1f);
                        float bh = (mirror ? H / 2f : H) * v;
                        float x = x0 + slot * i + (slot - bw) / 2f;
                        var col = Gfx.Mix(Color, Color2, v);
                        brush.Color = Gfx.C4(col, 0.18f * glow);
                        if (mirror) Gfx.Rect(dc, brush, x - 2f, ctr.Y - bh - 2f, bw + 4f, bh * 2f + 4f);
                        else Gfx.Rect(dc, brush, x - 2f, y1 - bh - 2f, bw + 4f, bh + 4f);
                        brush.Color = Gfx.C4(col, 0.95f);
                        if (mirror) Gfx.Rect(dc, brush, x, ctr.Y - bh, bw, bh * 2f);
                        else Gfx.Rect(dc, brush, x, y1 - bh, bw, bh);
                        // 先端のキャップ
                        brush.Color = Gfx.C4(white, 0.9f);
                        float capY = mirror ? ctr.Y - bh - th * 1.2f : y1 - bh - th * 1.2f;
                        Gfx.Rect(dc, brush, x, capY, bw, th);
                        if (mirror) Gfx.Rect(dc, brush, x, ctr.Y + bh + th * 0.2f, bw, th);
                    }
                    break;
                }
        }
    }
}
