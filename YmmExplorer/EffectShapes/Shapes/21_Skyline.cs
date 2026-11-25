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

public enum SkylineKind
{
    [Display(Name = "街（ビル群）")] City,
    [Display(Name = "山脈")] Mountains,
    [Display(Name = "森（針葉樹）")] Forest,
}

// =====================================================================
//  夕焼けの街・山のシルエット
//   遠くほど淡く、手前ほど濃い黒い影。ゆっくり流れ、街には窓の明かりが灯る
// =====================================================================
public sealed class SkylinePlugin : IShapePlugin
{
    public string Name => "演出：夕焼けの街・山のシルエット";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SkylineParameter(sharedData);
}

public sealed class SkylineParameter : EffectShapeParameterBase
{
    public SkylineParameter() : this(null) { }
    public SkylineParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "種類")]
    [EnumComboBox]
    public SkylineKind Kind { get => kind; set => Set(ref kind, value); }
    SkylineKind kind = SkylineKind.City;

    [Display(GroupName = "色", Name = "手前の影の色")]
    [ColorPicker]
    public WpfColor NearColor { get => near; set => Set(ref near, value); }
    WpfColor near = WpfColor.FromRgb(0x14, 0x0A, 0x24);

    [Display(GroupName = "色", Name = "遠くのかすみの色")]
    [ColorPicker]
    public WpfColor HazeColor { get => haze; set => Set(ref haze, value); }
    WpfColor haze = WpfColor.FromRgb(0xC0, 0x6A, 0x7A);

    [Display(GroupName = "形", Name = "底辺の高さ位置")]
    [AnimationSlider("F0", "%", 0, 120)]
    public Animation BaseY { get; } = new Animation(100, -100, 200);

    [Display(GroupName = "形", Name = "最も高い部分の高さ")]
    [AnimationSlider("F0", "px", 60, 1000)]
    public new Animation Height { get; } = new Animation(420, 10, 6000);

    [Display(GroupName = "形", Name = "層の数")]
    [AnimationSlider("F0", "層", 1, 5)]
    public Animation Layers { get; } = new Animation(3, 1, 12);

    [Display(GroupName = "動き", Name = "流れる速さ")]
    [AnimationSlider("F0", "px/s", -100, 100)]
    public Animation Scroll { get; } = new Animation(10, -3000, 3000);

    [Display(GroupName = "窓の明かり", Name = "窓の明かりをつける（街）")]
    [ToggleSlider]
    public bool Windows { get => windows; set => Set(ref windows, value); }
    bool windows = true;

    [Display(GroupName = "窓の明かり", Name = "明かりの色")]
    [ColorPicker]
    public WpfColor WindowColor { get => winCol; set => Set(ref winCol, value); }
    WpfColor winCol = WpfColor.FromRgb(0xFF, 0xD3, 0x6A);

    [Display(GroupName = "窓の明かり", Name = "明かりの量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation WindowAmount { get; } = new Animation(35, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [BaseY, Height, Layers, Scroll, WindowAmount];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float baseY = c.F(BaseY) / 100f * h, H = c.F(Height), scroll = c.F(Scroll) * t;
        int L = Math.Clamp((int)c.F(Layers), 1, 5);
        float winK = c.F(WindowAmount) / 100f;

        using var brush = dc.CreateSolidColorBrush(default);
        for (int k = 0; k < L; k++)
        {
            float f = L == 1 ? 1f : k / (float)(L - 1);                      // 0=遠い 1=手前
            var col = Gfx.Mix(HazeColor, NearColor, 0.3f + 0.7f * f);
            float hs = 0.45f + 0.55f * f, par = 0.3f + 0.7f * f;
            float off = scroll * par;
            var rng = new Random(Seed + k * 91);
            brush.Color = Gfx.C4(col, 1f);

            switch (Kind)
            {
                case SkylineKind.City:
                    {
                        var items = new List<(float bw, float bh, int kind)>();
                        float period = 0f;
                        for (int i = 0; i < 40; i++)
                        {
                            float bw = (36f + 90f * rng.NextF()) * (0.7f + 0.5f * f);
                            float bh = H * hs * (0.22f + 0.78f * MathF.Pow(rng.NextF(), 1.4f));
                            items.Add((bw, bh, rng.Next(0, 6)));
                            period += bw + 2f;
                        }
                        float x = -Gfx.Mod(off, period);
                        int idx = 0;
                        while (x < w)
                        {
                            var b = items[idx % items.Count];
                            Gfx.Rect(dc, brush, x, baseY - b.bh, b.bw, b.bh + 400f);
                            if (b.kind == 0)       // アンテナ
                                dc.DrawLine(new Vector2(x + b.bw * 0.5f, baseY - b.bh), new Vector2(x + b.bw * 0.5f, baseY - b.bh - 30f * hs), brush, 2f);
                            else if (b.kind == 1)  // 屋上の小さな出っ張り
                                Gfx.Rect(dc, brush, x + b.bw * 0.2f, baseY - b.bh - 10f * hs, b.bw * 0.4f, 10f * hs);

                            // 窓の明かり（手前の2層だけ）
                            if (Windows && k >= L - 2 && winK > 0f)
                            {
                                float ww = 6f * (0.7f + 0.5f * f), wh = 9f * (0.7f + 0.5f * f), gx = ww * 1.9f, gy = wh * 1.8f;
                                for (float wy = baseY - b.bh + gy * 0.6f; wy < baseY - gy * 0.5f && wy < h; wy += gy)
                                    for (float wx = x + gx * 0.5f; wx < x + b.bw - gx * 0.5f; wx += gx)
                                    {
                                        float hsh = Gfx.Hash01((int)(wx * 13f + wy * 7f + k * 1013));
                                        if (hsh > winK) continue;
                                        float tw = 0.7f + 0.3f * MathF.Sin(t * 0.5f + hsh * 40f);
                                        brush.Color = Gfx.C4(WindowColor, 0.9f * tw);
                                        Gfx.Rect(dc, brush, wx, wy, ww, wh);
                                    }
                                brush.Color = Gfx.C4(col, 1f);
                            }
                            x += b.bw + 2f;
                            idx++;
                        }
                        break;
                    }
                case SkylineKind.Mountains:
                    {
                        float p1 = rng.NextF() * 6.28f, p2 = rng.NextF() * 6.28f, p3 = rng.NextF() * 6.28f;
                        var pts = new List<Vector2>();
                        for (float x = -12f; x <= w + 12f; x += 12f)
                        {
                            float xs = x + off;
                            float n = 0.55f * (1f - MathF.Abs(MathF.Sin(xs * 0.0035f + p1))) + 0.30f * (1f - MathF.Abs(MathF.Sin(xs * 0.009f + p2))) + 0.15f * (1f - MathF.Abs(MathF.Sin(xs * 0.021f + p3)));
                            pts.Add(new Vector2(x, baseY - H * hs * (0.2f + 0.8f * n)));
                        }
                        pts.Add(new Vector2(w + 12f, baseY + 40f));
                        pts.Add(new Vector2(-12f, baseY + 40f));
                        Gfx.FillPoly(dc, brush, pts, -2f, h + 2f);
                        break;
                    }
                default:    // 森
                    {
                        // なだらかな丘
                        float p1 = rng.NextF() * 6.28f;
                        var hill = new List<Vector2>();
                        for (float x = -12f; x <= w + 12f; x += 16f)
                            hill.Add(new Vector2(x, baseY - H * hs * 0.12f * (1f + MathF.Sin((x + off) * 0.004f + p1))));
                        hill.Add(new Vector2(w + 12f, baseY + 40f)); hill.Add(new Vector2(-12f, baseY + 40f));
                        Gfx.FillPoly(dc, brush, hill, -2f, h + 2f);

                        var items = new List<(float tw, float th, float gap)>();
                        float period = 0f;
                        for (int i = 0; i < 50; i++)
                        {
                            float tw = (26f + 46f * rng.NextF()) * (0.7f + 0.5f * f), th = H * hs * (0.25f + 0.55f * rng.NextF()), gap = -6f + 14f * rng.NextF();
                            items.Add((tw, th, gap));
                            period += tw + gap;
                        }
                        float x0 = -Gfx.Mod(off, period);
                        int idx = 0;
                        while (x0 < w + 60f)
                        {
                            var tr = items[idx % items.Count];
                            float cx = x0 + tr.tw / 2f;
                            float by = baseY - H * hs * 0.1f;
                            for (int tier = 0; tier < 3; tier++)
                            {
                                float y1 = by - tr.th * (0.3f + 0.28f * tier), y0 = by - tr.th * (0.1f + 0.3f * tier);
                                float half = tr.tw * (0.5f - 0.1f * tier);
                                Gfx.FillPoly(dc, brush, new List<Vector2> { new(cx, y1 - tr.th * 0.28f), new(cx + half, y0), new(cx - half, y0) });
                            }
                            Gfx.Rect(dc, brush, cx - 2f, by - tr.th * 0.12f, 4f, tr.th * 0.14f + 30f);
                            x0 += tr.tw + tr.gap;
                            idx++;
                        }
                        break;
                    }
            }
        }
    }
}
