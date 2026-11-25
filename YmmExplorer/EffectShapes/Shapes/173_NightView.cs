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

public enum NightSkyTone
{
    [Display(Name = "深い夜（紺）")] Deep,
    [Display(Name = "日が沈んだ直後（紫〜橙）")] Dusk,
    [Display(Name = "都会の夜空（明るい紫）")] City,
    [Display(Name = "真っ暗")] Black,
}

// =====================================================================
//  夜景
//   奥から手前へ3層のビル群。窓の明かりがぽつぽつ灯り、ときどきまたたく。
//   高い建物の赤い航空障害灯、電波塔、道路を流れる車の光、
//   水面に映る街の光、手前のぼけた光(玉ボケ)、星と月
// =====================================================================
public sealed class NightViewPlugin : IShapePlugin
{
    public string Name => "演出：夜景";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NightViewParameter(sharedData);
}

public sealed class NightViewParameter : EffectShapeParameterBase
{
    public NightViewParameter() : this(null) { }
    public NightViewParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "空", Name = "空の色")]
    [EnumComboBox]
    public NightSkyTone Sky { get => sky; set => Set(ref sky, value); }
    NightSkyTone sky = NightSkyTone.Deep;

    [Display(GroupName = "空", Name = "星の数")]
    [TextBoxSlider("F0", "個", 0, 400)]
    [Range(0, 3000)]
    public int Stars { get => stars; set => Set(ref stars, value); }
    int stars = 120;

    [Display(GroupName = "空", Name = "月を出す")]
    [ToggleSlider]
    public bool Moon { get => moon; set => Set(ref moon, value); }
    bool moon = true;

    [Display(GroupName = "街", Name = "地平線の高さ（Y）")]
    [AnimationSlider("F0", "%", 30, 95)]
    public Animation Horizon { get; } = new Animation(72, 0, 100);

    [Display(GroupName = "街", Name = "ビルの高さ")]
    [AnimationSlider("F0", "%", 20, 200)]
    public Animation BuildingScale { get; } = new Animation(100, 5, 500);

    [Display(GroupName = "街", Name = "ビルの数（密度）")]
    [AnimationSlider("F0", "%", 30, 200)]
    public Animation Density { get; } = new Animation(100, 10, 500);

    [Display(GroupName = "街", Name = "電波塔を出す")]
    [ToggleSlider]
    public bool Tower { get => tower; set => Set(ref tower, value); }
    bool tower = true;

    [Display(GroupName = "街", Name = "電波塔のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation TowerX { get; } = new Animation(68, -50, 150);

    [Display(GroupName = "明かり", Name = "窓の明かりの色")]
    [ColorPicker]
    public WpfColor WindowColor { get => windowColor; set => Set(ref windowColor, value); }
    WpfColor windowColor = WpfColor.FromRgb(0xFF, 0xD9, 0x8A);

    [Display(GroupName = "明かり", Name = "明かりのついた窓の割合")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation LitRatio { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "明かり", Name = "窓がぽつぽつ灯る時間（0=はじめから）")]
    [AnimationSlider("F1", "秒", 0, 10)]
    public Animation LightUpTime { get; } = new Animation(0, 0, 600);

    [Display(GroupName = "明かり", Name = "車の光の流れ")]
    [ToggleSlider]
    public bool Traffic { get => traffic; set => Set(ref traffic, value); }
    bool traffic = true;

    [Display(GroupName = "明かり", Name = "水面に映す")]
    [ToggleSlider]
    public bool Water { get => water; set => Set(ref water, value); }
    bool water = true;

    [Display(GroupName = "明かり", Name = "手前の玉ボケ")]
    [AnimationSlider("F0", "個", 0, 60)]
    public Animation Bokeh { get; } = new Animation(14, 0, 300);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Horizon, BuildingScale, Density, TowerX, LitRatio, LightUpTime, Bokeh];

    /// <summary>窓が明るいか(灯る時間・またたきを含む)。0〜1</summary>
    float WindowLight(int seed, int b, int i, float lit, float t, float lightUp)
    {
        float r = DrawKit.H(seed, b * 977 + i, 3);
        if (r > lit) return 0f;
        if (lightUp > 0.001f)
        {
            float on = DrawKit.H(seed, b * 977 + i, 4) * lightUp;
            if (t < on) return 0f;
            if (t < on + 0.15f) return (t - on) / 0.15f;
        }
        // ごくまれに消えたり点いたりする
        float flick = DrawKit.H(seed + (int)(t * 0.7f), b * 977 + i, 5);
        return flick < 0.01f ? 0.15f : 1f;
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float u = MathF.Min(w / 1920f, h / 1080f);
        float hy = c.F(Horizon) / 100f * h;
        float bs = c.F(BuildingScale) / 100f, dens = c.F(Density) / 100f, lit = c.F(LitRatio) / 100f, lightUp = c.F(LightUpTime);
        int seed = Seed;
        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 空 ----
        var (top, mid, low) = Sky switch
        {
            NightSkyTone.Dusk => (WpfColor.FromRgb(0x1B, 0x16, 0x45), WpfColor.FromRgb(0x6A, 0x2C, 0x70), WpfColor.FromRgb(0xF0, 0x8A, 0x4B)),
            NightSkyTone.City => (WpfColor.FromRgb(0x14, 0x12, 0x35), WpfColor.FromRgb(0x3A, 0x2B, 0x66), WpfColor.FromRgb(0x8A, 0x55, 0x8C)),
            NightSkyTone.Black => (WpfColor.FromRgb(0x02, 0x02, 0x05), WpfColor.FromRgb(0x05, 0x06, 0x0C), WpfColor.FromRgb(0x12, 0x14, 0x22)),
            _ => (WpfColor.FromRgb(0x03, 0x07, 0x1A), WpfColor.FromRgb(0x0B, 0x1A, 0x3D), WpfColor.FromRgb(0x2A, 0x3A, 0x6A)),
        };
        using (var g = Gfx.Linear(dc, new Vector2(0, 0), new Vector2(0, hy), (0f, Gfx.C4(top, 1f)), (0.65f, Gfx.C4(mid, 1f)), (1f, Gfx.C4(low, 1f))))
            dc.FillRectangle(new RawRectF(0, 0, w, hy + 2f), g);
        // 星
        for (int i = 0; i < Stars; i++)
        {
            var sp = new Vector2(DrawKit.H(seed, i, 10) * w, DrawKit.H(seed, i, 11) * hy * 0.85f);
            float tw = 0.55f + 0.45f * MathF.Sin(t * (1f + DrawKit.H(seed, i, 12) * 3f) + i);
            float sr = (0.6f + MathF.Pow(DrawKit.H(seed, i, 13), 4f) * 2.2f) * u;
            brush.Color = Gfx.C4(1f, 1f, 0.95f, tw * (1f - sp.Y / hy) * 0.95f);
            dc.FillEllipse(new Ellipse(sp, sr, sr), brush);
        }
        if (Moon)
        {
            var mc = new Vector2(w * 0.82f, h * 0.16f);
            float mr = 46f * u;
            DrawKit.Glow(dc, mc, mr * 4f, Gfx.C4(1f, 0.95f, 0.8f, 0.25f));
            brush.Color = Gfx.C4(1f, 0.97f, 0.88f, 1f);
            dc.FillEllipse(new Ellipse(mc, mr, mr), brush);
            brush.Color = Gfx.C4(0.85f, 0.82f, 0.7f, 0.5f);
            dc.FillEllipse(new Ellipse(mc + new Vector2(-mr * 0.3f, -mr * 0.2f), mr * 0.18f, mr * 0.15f), brush);
            dc.FillEllipse(new Ellipse(mc + new Vector2(mr * 0.25f, mr * 0.3f), mr * 0.12f, mr * 0.1f), brush);
        }
        // 地平線の街あかりのもや
        using (var g = Gfx.Linear(dc, new Vector2(0, hy - h * 0.25f), new Vector2(0, hy), (0f, Gfx.C4(WindowColor, 0f)), (1f, Gfx.C4(WindowColor, 0.22f))))
            dc.FillRectangle(new RawRectF(0, hy - h * 0.25f, w, hy), g);

        // ---- ビル(奥 → 手前) ----
        var reflections = new List<(RawRectF r, Color4 col)>();
        for (int layer = 0; layer < 3; layer++)
        {
            float depth = layer / 2f;                      // 0=奥 1=手前
            float minW = (40f + 50f * depth) * u, maxW = (90f + 140f * depth) * u;
            float maxH = h * (0.18f + 0.22f * depth) * bs;
            float baseY = hy + (layer == 2 ? 0f : -2f);
            var bodyCol = Gfx.Mix(Gfx.Mix(top, mid, 0.5f), WpfColor.FromRgb(0, 0, 0), 0.25f + 0.45f * depth);
            float x = -20f * u + DrawKit.H(seed, layer, 20) * 30f * u;
            int b = 0;
            while (x < w + 20f * u && b < 400)
            {
                int id = layer * 1000 + b;
                float bw = minW + DrawKit.H(seed, id, 21) * (maxW - minW);
                float bh = maxH * (0.35f + MathF.Pow(DrawKit.H(seed, id, 22), 1.5f) * 0.65f);
                float gap = (6f + DrawKit.H(seed, id, 23) * 24f) * u / MathF.Max(0.3f, dens);
                float bx0 = x, bx1 = x + bw, by0 = baseY - bh;
                brush.Color = Gfx.C4(bodyCol, 1f);
                // 屋上の形(平ら・段・とがり)
                int roof = (int)(DrawKit.H(seed, id, 24) * 4f);
                dc.FillRectangle(new RawRectF(bx0, by0, bx1, baseY), brush);
                if (roof == 1) dc.FillRectangle(new RawRectF(bx0 + bw * 0.2f, by0 - bh * 0.06f, bx1 - bw * 0.2f, by0), brush);
                if (roof == 2) Gfx.FillPoly(dc, brush, new List<Vector2> { new(bx0 + bw * 0.3f, by0), new(bx1 - bw * 0.3f, by0), new((bx0 + bx1) / 2f, by0 - bh * 0.12f) });
                // 窓(手前ほど大きい)
                float ww = (4f + 4f * depth) * u, wh = (5f + 5f * depth) * u;
                float gx = ww * 1.9f, gy = wh * 1.8f;
                int cols = Math.Max(1, (int)((bw - ww) / gx)), rows = Math.Max(1, (int)((bh - wh * 2f) / gy));
                float ox = bx0 + (bw - (cols - 1) * gx - ww) / 2f;
                var winTint = Gfx.Mix(WindowColor, DrawKit.H(seed, id, 25) < 0.3f ? WpfColor.FromRgb(0xC8, 0xE6, 0xFF) : WindowColor, 0.7f);
                for (int r = 0; r < rows; r++)
                    for (int cc = 0; cc < cols; cc++)
                    {
                        float l = WindowLight(seed, id, r * 64 + cc, lit, t, lightUp);
                        if (l <= 0.01f) continue;
                        float wx = ox + cc * gx, wy = by0 + wh + r * gy;
                        float a = l * (0.55f + 0.45f * depth) * (0.75f + 0.25f * DrawKit.H(seed, id * 31 + r, cc));
                        brush.Color = Gfx.C4(winTint, a);
                        var wr = new RawRectF(wx, wy, wx + ww, wy + wh);
                        dc.FillRectangle(wr, brush);
                        if (layer == 2 && Water && r % 2 == 0) reflections.Add((wr, Gfx.C4(winTint, a * 0.5f)));
                    }
                // 高いビルの屋上の赤いライト(ゆっくり点滅)
                if (bh > maxH * 0.75f)
                {
                    float blink = 0.5f + 0.5f * MathF.Sin(t * 2.2f + id);
                    var lp = new Vector2((bx0 + bx1) / 2f, by0 - (roof == 2 ? bh * 0.12f : roof == 1 ? bh * 0.06f : 0f) - 3f * u);
                    DrawKit.Glow(dc, lp, 14f * u * (0.6f + depth), Gfx.C4(1f, 0.15f, 0.1f, 0.7f * blink));
                    brush.Color = Gfx.C4(1f, 0.3f, 0.25f, blink);
                    dc.FillEllipse(new Ellipse(lp, 2.5f * u, 2.5f * u), brush);
                }
                x += bw + gap;
                b++;
            }
            // 電波塔は、真ん中の層の後ろに立てる
            if (layer == 0 && Tower)
            {
                float tx = c.F(TowerX) / 100f * w, th = h * 0.55f * bs, tb = 90f * u;
                var tcol = Gfx.C4(Gfx.Mix(top, WpfColor.FromRgb(0, 0, 0), 0.35f), 1f);
                brush.Color = tcol;
                Gfx.FillPoly(dc, brush, new List<Vector2> { new(tx - tb, hy), new(tx - 6f * u, hy - th), new(tx + 6f * u, hy - th), new(tx + tb, hy) });
                // 塔の照明(縦の光と、展望台の帯)
                var lightCol = WpfColor.FromRgb(0xFF, 0x8A, 0x3D);
                for (int i = 0; i < 18; i++)
                {
                    float f = i / 18f;
                    float y = hy - th * f;
                    float half = tb * (1f - f) + 6f * u * f;
                    float on = 0.6f + 0.4f * MathF.Sin(t * 3f - i * 0.5f);
                    brush.Color = Gfx.C4(lightCol, 0.75f * on);
                    dc.DrawLine(new Vector2(tx - half, y), new Vector2(tx + half, y), brush, 2f * u);
                    dc.DrawLine(new Vector2(tx - half, y), new Vector2(tx - (tb * (1f - f - 1f / 18f) + 6f * u * (f + 1f / 18f)), y - th / 18f), brush, 1.5f * u);
                    dc.DrawLine(new Vector2(tx + half, y), new Vector2(tx + (tb * (1f - f - 1f / 18f) + 6f * u * (f + 1f / 18f)), y - th / 18f), brush, 1.5f * u);
                }
                var deck = new Vector2(tx, hy - th * 0.62f);
                brush.Color = Gfx.C4(1f, 0.95f, 0.8f, 1f);
                dc.FillRectangle(new RawRectF(deck.X - tb * 0.42f, deck.Y - 8f * u, deck.X + tb * 0.42f, deck.Y + 8f * u), brush);
                DrawKit.Glow(dc, deck, tb * 0.8f, Gfx.C4(1f, 0.8f, 0.5f, 0.35f));
                float topBlink = DrawKit.Frac(t * 0.8f) < 0.5f ? 1f : 0.2f;
                DrawKit.Glow(dc, new Vector2(tx, hy - th), 20f * u, Gfx.C4(1f, 0.2f, 0.1f, 0.9f * topBlink));
            }
        }

        // ---- 道路と車の光 ----
        float roadY = hy + (h - hy) * 0.12f;
        if (Traffic)
        {
            brush.Color = Gfx.C4(0.02f, 0.02f, 0.04f, 1f);
            dc.FillRectangle(new RawRectF(0, hy, w, roadY + 14f * u), brush);
            for (int i = 0; i < 40; i++)
            {
                bool toRight = i % 2 == 0;
                float speed = (180f + DrawKit.H(seed, i, 30) * 160f) * u;
                float x0 = Gfx.Mod(DrawKit.H(seed, i, 31) * (w + 400f * u) + (toRight ? 1f : -1f) * t * speed, w + 400f * u) - 200f * u;
                float y = roadY + (toRight ? 6f : -2f) * u;
                float len = (40f + DrawKit.H(seed, i, 32) * 80f) * u;
                // 向こうへ行く車は赤いテールランプ、こちらへ来る車は白いヘッドライト
                var col = toRight ? Gfx.C4(1f, 0.2f, 0.15f, 0.85f) : Gfx.C4(1f, 0.95f, 0.85f, 0.9f);
                using var g = Gfx.Linear(dc, new Vector2(x0, y), new Vector2(x0 + (toRight ? -len : len), y), (0f, col), (1f, Gfx.C4(0f, 0f, 0f, 0f)));
                dc.DrawLine(new Vector2(x0, y), new Vector2(x0 + (toRight ? -len : len), y), g, 3f * u);
            }
        }

        // ---- 水面と映り込み ----
        float waterY = Traffic ? roadY + 14f * u : hy;
        if (Water && waterY < h)
        {
            using (var g = Gfx.Linear(dc, new Vector2(0, waterY), new Vector2(0, h), (0f, Gfx.C4(Gfx.Mix(low, WpfColor.FromRgb(0, 0, 0), 0.6f), 1f)), (1f, Gfx.C4(0.01f, 0.01f, 0.03f, 1f))))
                dc.FillRectangle(new RawRectF(0, waterY, w, h), g);
            float depthW = h - waterY;
            foreach (var (r, col) in reflections)
            {
                // 映り込み: 上下反転した位置に、横にゆらぐ縦長の光
                float ry = waterY + (hy - r.Bottom) * 0.9f;
                if (ry > h) continue;
                float wob = MathF.Sin(t * 2.4f + r.Left * 0.05f + ry * 0.08f) * 4f * u;
                float len = (r.Bottom - r.Top) * (2.5f + MathF.Sin(t * 3f + r.Left) * 0.8f);
                brush.Color = new Color4(col.R, col.G, col.B, col.A * (1f - (ry - waterY) / MathF.Max(1f, depthW)));
                dc.FillRectangle(new RawRectF(r.Left + wob, ry, r.Right + wob, ry + len), brush);
            }
            // 水面のきらめきの線
            for (int i = 0; i < 60; i++)
            {
                float y = waterY + DrawKit.H(seed, i, 40) * depthW;
                float x = DrawKit.H(seed, i, 41) * w + MathF.Sin(t * 1.5f + i) * 20f * u;
                float len = (20f + DrawKit.H(seed, i, 42) * 60f) * u * (0.5f + (y - waterY) / depthW);
                brush.Color = Gfx.C4(WindowColor, 0.25f * (0.5f + 0.5f * MathF.Sin(t * 4f + i * 1.3f)));
                dc.DrawLine(new Vector2(x, y), new Vector2(x + len, y), brush, 1.5f * u);
            }
        }
        else if (!Water && hy < h)
        {
            brush.Color = Gfx.C4(0.01f, 0.01f, 0.02f, 1f);
            dc.FillRectangle(new RawRectF(0, Traffic ? roadY + 14f * u : hy, w, h), brush);
        }

        // ---- 手前の玉ボケ ----
        int bokeh = (int)c.F(Bokeh);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        for (int i = 0; i < bokeh; i++)
        {
            var p = new Vector2(DrawKit.H(seed, i, 50) * w + MathF.Sin(t * 0.3f + i) * 10f * u, h * (0.55f + DrawKit.H(seed, i, 51) * 0.45f));
            float r = (30f + DrawKit.H(seed, i, 52) * 70f) * u;
            var bc = DrawKit.H(seed, i, 53) < 0.6f ? WindowColor : Gfx.Hsv(DrawKit.H(seed, i, 54) * 360f, 0.5f, 1f);
            float a = 0.12f + 0.08f * MathF.Sin(t * 1.2f + i);
            brush.Color = Gfx.C4(bc, a);
            dc.FillEllipse(new Ellipse(p, r, r), brush);
            brush.Color = Gfx.C4(bc, a * 0.8f);
            dc.DrawEllipse(new Ellipse(p, r * 0.97f, r * 0.97f), brush, 2f * u);
        }
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
    }
}
