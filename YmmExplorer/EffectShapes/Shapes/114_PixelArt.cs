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

public enum PixelKind
{
    [Display(Name = "爆発")] Explosion,
    [Display(Name = "ハート")] Heart,
    [Display(Name = "コイン")] Coin,
    [Display(Name = "3つ並べる")] All,
}

// =====================================================================
//  ドット絵（8bit）風
//   カクカク動く爆発・ハート・コイン
// =====================================================================
public sealed class PixelArtPlugin : IShapePlugin
{
    public string Name => "演出：ドット絵（8bit）風";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PixelArtParameter(sharedData);
}

public sealed class PixelArtParameter : EffectShapeParameterBase
{
    public PixelArtParameter() : this(null) { }
    public PixelArtParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public PixelKind Kind { get => kind; set => Set(ref kind, value); }
    PixelKind kind = PixelKind.All;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "1ドットの大きさ")]
    [AnimationSlider("F0", "px", 2, 40)]
    public Animation Dot { get; } = new Animation(12, 1, 400);

    [Display(GroupName = "動き", Name = "コマ数（1秒あたり）")]
    [AnimationSlider("F0", "コマ", 2, 30)]
    public Animation FrameRate { get; } = new Animation(10, 1, 60);

    [Display(GroupName = "動き", Name = "爆発のくり返し間隔")]
    [AnimationSlider("F1", "秒", 0.3, 5)]
    public Animation BoomInterval { get; } = new Animation(1.2, 0.1, 600);

    [Display(GroupName = "色", Name = "ハートの色")]
    [ColorPicker]
    public WpfColor HeartColor { get => heartColor; set => Set(ref heartColor, value); }
    WpfColor heartColor = WpfColor.FromRgb(0xF0, 0x28, 0x3C);

    [Display(GroupName = "色", Name = "コインの色")]
    [ColorPicker]
    public WpfColor CoinColor { get => coinColor; set => Set(ref coinColor, value); }
    WpfColor coinColor = WpfColor.FromRgb(0xF8, 0xC8, 0x20);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Dot, FrameRate, BoomInterval];

    // 9x8 のハート（1=ふち 2=中 3=光）
    static readonly string[] HeartMap =
    {
        ".11...11.",
        "1331.1221",
        "132212221",
        "122222221",
        ".1222221.",
        "..12221..",
        "...121...",
        "....1....",
    };

    static void Px(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 org, int x, int y, float d)
        => dc.FillRectangle(new RawRectF(MathF.Round(org.X + x * d), MathF.Round(org.Y + y * d), MathF.Round(org.X + (x + 1) * d), MathF.Round(org.Y + (y + 1) * d)), brush);

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float d = MathF.Max(1f, MathF.Round(c.F(Dot)));
        float fps = MathF.Max(1f, c.F(FrameRate));
        int frame = (int)MathF.Floor(t * fps);
        float boomI = MathF.Max(0.1f, c.F(BoomInterval));

        using var brush = dc.CreateSolidColorBrush(default);
        if (Kind == PixelKind.All)
        {
            DrawBoom(dc, brush, ctr - new Vector2(d * 16f, 0), d, t, fps, boomI);
            DrawHeart(dc, brush, ctr, d, frame);
            DrawCoin(dc, brush, ctr + new Vector2(d * 16f, 0), d, frame);
        }
        else if (Kind == PixelKind.Explosion) DrawBoom(dc, brush, ctr, d, t, fps, boomI);
        else if (Kind == PixelKind.Heart) DrawHeart(dc, brush, ctr, d, frame);
        else DrawCoin(dc, brush, ctr, d, frame);
    }

    void DrawHeart(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float d, int frame)
    {
        // 2コマごとに少し大きくなる（ドクン）
        float dd = d * (frame % 4 == 0 ? 1.15f : 1f);
        var org = ctr - new Vector2(4.5f * dd, 4f * dd);
        var dark = Gfx.Mix(HeartColor, WpfColor.FromRgb(40, 0, 10), 0.6f);
        for (int y = 0; y < HeartMap.Length; y++)
            for (int x = 0; x < HeartMap[y].Length && x < 9; x++)
            {
                char ch = HeartMap[y][x];
                if (ch == '.') continue;
                brush.Color = Gfx.C4(ch switch { '1' => dark, '3' => WpfColor.FromRgb(255, 255, 255), _ => HeartColor }, 1f);
                Px(dc, brush, org, x, y, dd);
            }
    }

    void DrawCoin(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float d, int frame)
    {
        // 回るコイン：幅 8 → 6 → 3 → 1 → 3 → 6
        int[] widths = { 8, 6, 3, 1, 3, 6 };
        int cw = widths[frame % widths.Length];
        const int ch = 10;
        var dark = Gfx.Mix(CoinColor, WpfColor.FromRgb(90, 40, 0), 0.55f);
        var lite = Gfx.Mix(CoinColor, WpfColor.FromRgb(255, 255, 255), 0.6f);
        var org = ctr - new Vector2(cw / 2f * d, ch / 2f * d);
        for (int y = 0; y < ch; y++)
        {
            // 角を丸める
            int inset = (y == 0 || y == ch - 1) ? Math.Min(2, cw / 3) : (y == 1 || y == ch - 2) ? Math.Min(1, cw / 3) : 0;
            for (int x = inset; x < cw - inset; x++)
            {
                bool edge = x == inset || x == cw - inset - 1 || y == 0 || y == ch - 1;
                bool shine = cw >= 6 && x == inset + 1 && y >= 2 && y <= 4;
                bool mark = cw >= 6 && x == cw / 2 && y >= 3 && y <= 6;
                brush.Color = Gfx.C4(edge ? dark : shine ? lite : mark ? dark : CoinColor, 1f);
                Px(dc, brush, org, x, y, d);
            }
        }
    }

    static void DrawBoom(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 ctr, float d, float t, float fps, float interval)
    {
        float lt = Gfx.Mod(t, interval);
        int f = (int)MathF.Floor(lt * fps);
        if (f > 6) return;
        var cols = new[] { WpfColor.FromRgb(255, 255, 255), WpfColor.FromRgb(255, 236, 80), WpfColor.FromRgb(255, 150, 30), WpfColor.FromRgb(220, 50, 20), WpfColor.FromRgb(90, 80, 80) };
        float r = 1.5f + f * 1.6f;
        int R = (int)MathF.Ceiling(r) + 1;
        var org = ctr - new Vector2(0.5f * d, 0.5f * d);
        for (int y = -R; y <= R; y++)
            for (int x = -R; x <= R; x++)
            {
                float dist = MathF.Sqrt(x * x + y * y);
                // ぎざぎざの外形
                float jag = 1f + 0.25f * MathF.Sin(MathF.Atan2(y, x) * 5f + f);
                if (dist > r * jag) continue;
                float u = dist / (r * jag);
                int ci = Math.Clamp((int)(u * 3f + f * 0.45f), 0, cols.Length - 1);
                // 後半は中が抜けて煙の輪になる
                if (f >= 4 && u < 0.35f + (f - 4) * 0.2f) continue;
                brush.Color = Gfx.C4(cols[ci], 1f);
                Px(dc, brush, org, x, y, d);
            }
        // 飛び散る四角いかけら
        for (int i = 0; i < 8; i++)
        {
            float a = i * MathF.PI / 4f + 0.3f;
            float dist = (2f + f * 2.4f);
            var p = new Vector2(MathF.Round(MathF.Cos(a) * dist), MathF.Round(MathF.Sin(a) * dist));
            brush.Color = Gfx.C4(cols[Math.Min(cols.Length - 1, 1 + f / 2)], 1f);
            Px(dc, brush, org, (int)p.X, (int)p.Y, d);
        }
    }
}
