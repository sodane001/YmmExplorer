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
//  正月
//   初日の出、空に揚がる凧、左右の門松
// =====================================================================
public sealed class NewYearPlugin : IShapePlugin
{
    public string Name => "演出：正月（門松・凧・初日の出）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NewYearParameter(sharedData);
}

public sealed class NewYearParameter : EffectShapeParameterBase
{
    public NewYearParameter() : this(null) { }
    public NewYearParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "表示", Name = "初日の出（空と太陽）")]
    [ToggleSlider]
    public bool Sunrise { get => sunrise; set => Set(ref sunrise, value); }
    bool sunrise = true;

    [Display(GroupName = "表示", Name = "凧")]
    [ToggleSlider]
    public bool Kite { get => kite; set => Set(ref kite, value); }
    bool kite = true;

    [Display(GroupName = "表示", Name = "門松")]
    [ToggleSlider]
    public bool Kadomatsu { get => kadomatsu; set => Set(ref kadomatsu, value); }
    bool kadomatsu = true;

    [Display(GroupName = "表示", Name = "梅の花びら")]
    [ToggleSlider]
    public bool Plum { get => plum; set => Set(ref plum, value); }
    bool plum = true;

    [Display(GroupName = "初日の出", Name = "日が昇る時間")]
    [AnimationSlider("F1", "秒", 0, 10)]
    public Animation RiseTime { get; } = new Animation(3, 0, 600);

    [Display(GroupName = "全体", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 200)]
    public Animation ItemScale { get; } = new Animation(100, 5, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [RiseTime, ItemScale];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float S = MathF.Min(w, h) / 1080f * c.F(ItemScale) / 100f;
        float rt = c.F(RiseTime);
        float rise = rt <= 0.001f ? 1f : DrawKit.EaseOut(t / rt);
        var white = WpfColor.FromRgb(255, 255, 255);
        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (Sunrise)
            {
                float hy = h * 0.72f;
                using (var sky = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, hy),
                    (0f, Gfx.C4(Gfx.Mix(WpfColor.FromRgb(0x10, 0x1A, 0x48), WpfColor.FromRgb(0x3A, 0x6E, 0xC8), rise), 1f)),
                    (0.7f, Gfx.C4(Gfx.Mix(WpfColor.FromRgb(0x5A, 0x2A, 0x5A), WpfColor.FromRgb(0xF6, 0x9A, 0x6A), rise), 1f)),
                    (1f, Gfx.C4(Gfx.Mix(WpfColor.FromRgb(0xC8, 0x4A, 0x3A), WpfColor.FromRgb(0xFF, 0xD2, 0x8A), rise), 1f))))
                    dc.FillRectangle(new RawRectF(0, 0, w, hy), sky);
                float sr = 120f * S;
                var sc = new Vector2(w / 2f, hy + sr * 1.1f - rise * sr * 2.2f);
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                DrawKit.Glow(dc, sc, sr * 4f, Gfx.C4(1f, 0.6f, 0.3f, 0.55f));
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                brush.Color = Gfx.C4(1f, 0.32f, 0.18f, 1f);
                var sun = DrawKit.ClipRect(DrawKit.EllipsePts(sc, sr, sr, 64), 0, 0, w, hy);
                if (sun.Count >= 3) Gfx.FillPoly(dc, brush, sun);
                // たなびく雲
                for (int i = 0; i < 4; i++)
                {
                    float cy = hy - h * (0.08f + 0.07f * i);
                    float cx = Gfx.Mod(w * DrawKit.H(Seed, i, 1) + t * 12f * (i + 1), w + 600f * S) - 300f * S;
                    brush.Color = Gfx.C4(Gfx.Mix(WpfColor.FromRgb(0xC8, 0x6A, 0x6A), white, rise * 0.6f), 0.7f);
                    dc.FillRectangle(new RawRectF(cx, cy, cx + 520f * S, cy + 12f * S), brush);
                    dc.FillRectangle(new RawRectF(cx + 80f * S, cy - 10f * S, cx + 380f * S, cy), brush);
                }
                // 海と光の道
                using (var sea = Gfx.Linear(dc, new Vector2(0, hy), new Vector2(0, h), (0f, Gfx.C4(0.15f, 0.2f, 0.4f, 1f)), (1f, Gfx.C4(0.04f, 0.06f, 0.15f, 1f))))
                    dc.FillRectangle(new RawRectF(0, hy, w, h), sea);
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                for (int i = 0; i < 26; i++)
                {
                    float y = hy + 6f + i * i * 0.9f * S;
                    if (y > h) break;
                    float half = (12f + i * 9f) * S * (0.7f + 0.3f * MathF.Sin(t * 2f + i));
                    brush.Color = Gfx.C4(1f, 0.55f, 0.3f, 0.5f * rise * (1f - i / 26f));
                    dc.FillRectangle(new RawRectF(w / 2f - half, y, w / 2f + half, y + 3f * S), brush);
                }
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }

            if (Kite)
            {
                float ks = 110f * S;
                var kc = new Vector2(w * 0.72f + MathF.Sin(t * 0.7f) * 30f * S, h * 0.24f + MathF.Sin(t * 1.1f) * 20f * S);
                float rot = MathF.Sin(t * 0.9f) * 0.12f;
                var body = DrawKit.Place(new[] { new Vector2(-ks * 0.5f, -ks * 0.6f), new Vector2(ks * 0.5f, -ks * 0.6f), new Vector2(ks * 0.5f, ks * 0.6f), new Vector2(-ks * 0.5f, ks * 0.6f) }, kc, rot);
                // 糸
                brush.Color = Gfx.C4(0.95f, 0.95f, 0.95f, 0.6f);
                var str = new List<Vector2>();
                for (int i = 0; i <= 20; i++)
                {
                    float u = i / 20f;
                    str.Add(Vector2.Lerp(body[3], new Vector2(w * 0.35f, h + 20f), u) + new Vector2(0, MathF.Sin(u * MathF.PI) * 40f * S));
                }
                Gfx.Polyline(dc, brush, str, 1.5f);
                // しっぽ
                var tail = new List<Vector2>();
                var tb = Vector2.Lerp(body[2], body[3], 0.5f);
                for (int i = 0; i <= 16; i++)
                {
                    float u = i / 16f;
                    tail.Add(tb + new Vector2(MathF.Sin(u * 6f - t * 4f) * 18f * S * u, u * ks * 2.2f));
                }
                brush.Color = Gfx.C4(0.85f, 0.15f, 0.2f, 1f);
                Gfx.Polyline(dc, brush, tail, 6f * S + 1f);
                brush.Color = Gfx.C4(0.98f, 0.94f, 0.84f, 1f);
                Gfx.FillPoly(dc, brush, body);
                // 赤い丸と縁取り
                brush.Color = Gfx.C4(0.86f, 0.12f, 0.16f, 1f);
                dc.FillEllipse(new Ellipse(kc, ks * 0.3f, ks * 0.3f), brush);
                brush.Color = Gfx.C4(0.2f, 0.12f, 0.08f, 1f);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(body), 3f * S + 1f);
                dc.DrawLine(body[0], body[2], brush, 1f);
                dc.DrawLine(body[1], body[3], brush, 1f);
            }

            if (Plum)
            {
                var rng = new Random(Seed + 5);
                for (int i = 0; i < 22; i++)
                {
                    float sp = 30f + 40f * rng.NextF();
                    float y = Gfx.Mod(rng.NextF() * h + t * sp, h + 40f) - 20f;
                    float x = rng.NextF() * w + MathF.Sin(t + i) * 30f;
                    float s = (8f + 8f * rng.NextF()) * S;
                    var p = new Vector2(x, y);
                    brush.Color = Gfx.C4(0.95f, 0.45f, 0.6f, 0.9f);
                    for (int k = 0; k < 5; k++)
                    {
                        float a = k * MathF.PI * 2f / 5f + t + i;
                        dc.FillEllipse(new Ellipse(p + new Vector2(MathF.Cos(a), MathF.Sin(a)) * s * 0.55f, s * 0.42f, s * 0.42f), brush);
                    }
                    brush.Color = Gfx.C4(1f, 0.85f, 0.4f, 1f);
                    dc.FillEllipse(new Ellipse(p, s * 0.22f, s * 0.22f), brush);
                }
            }

            if (Kadomatsu)
            {
                DrawKadomatsu(dc, brush, new Vector2(w * 0.1f, h), 1f * S, t);
                DrawKadomatsu(dc, brush, new Vector2(w * 0.9f, h), 1f * S, t + 1f);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    static void DrawKadomatsu(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 bottom, float S, float t)
    {
        float baseW = 170f * S, baseH = 130f * S;
        // 松の葉（後ろ）
        for (int i = 0; i < 7; i++)
        {
            float a = -MathF.PI / 2f + (i - 3) * 0.32f;
            var p = bottom + new Vector2((i - 3) * 22f * S, -baseH - 40f * S) + new Vector2(MathF.Cos(a), MathF.Sin(a)) * 30f * S;
            brush.Color = Gfx.C4(0.12f, 0.38f, 0.2f, 1f);
            dc.FillEllipse(new Ellipse(p, 42f * S, 24f * S), brush);
            brush.Color = Gfx.C4(0.18f, 0.5f, 0.28f, 1f);
            dc.FillEllipse(new Ellipse(p + new Vector2(0, -6f * S), 30f * S, 14f * S), brush);
        }
        // 竹3本（斜めに切った口）
        float[] hs = { 360f, 300f, 250f };
        float[] xs = { 0f, -34f, 34f };
        int[] order = { 1, 2, 0 };
        foreach (int i in order)
        {
            float bh = hs[i] * S, bx = bottom.X + xs[i] * S, bw = 30f * S;
            var top = new Vector2(bx, bottom.Y - bh);
            var poly = new List<Vector2> { new(bx - bw / 2f, bottom.Y - baseH * 0.5f), new(bx - bw / 2f, top.Y + bw * 0.8f), new(bx + bw / 2f, top.Y), new(bx + bw / 2f, bottom.Y - baseH * 0.5f) };
            using (var lg = Gfx.Linear(dc, new Vector2(bx - bw / 2f, 0), new Vector2(bx + bw / 2f, 0), (0f, Gfx.C4(0.25f, 0.55f, 0.25f, 1f)), (0.5f, Gfx.C4(0.45f, 0.75f, 0.38f, 1f)), (1f, Gfx.C4(0.2f, 0.45f, 0.2f, 1f))))
                Gfx.FillPoly(dc, lg, poly);
            // 切り口
            brush.Color = Gfx.C4(0.9f, 0.88f, 0.7f, 1f);
            dc.FillEllipse(new Ellipse(new Vector2(bx, top.Y + bw * 0.4f), bw * 0.45f, bw * 0.2f), brush);
            brush.Color = Gfx.C4(0.2f, 0.4f, 0.18f, 1f);
            dc.DrawLine(new Vector2(bx - bw / 2f, top.Y + bh * 0.45f), new Vector2(bx + bw / 2f, top.Y + bh * 0.45f), brush, 2f * S + 1f);
        }
        // わらの台と縄
        var b = new RawRectF(bottom.X - baseW / 2f, bottom.Y - baseH, bottom.X + baseW / 2f, bottom.Y);
        using (var lg = Gfx.Linear(dc, new Vector2(b.Left, 0), new Vector2(b.Right, 0), (0f, Gfx.C4(0.65f, 0.52f, 0.28f, 1f)), (0.5f, Gfx.C4(0.85f, 0.72f, 0.42f, 1f)), (1f, Gfx.C4(0.6f, 0.48f, 0.25f, 1f))))
            dc.FillRectangle(b, lg);
        brush.Color = Gfx.C4(0.5f, 0.38f, 0.18f, 0.5f);
        for (float x = b.Left + 6f; x < b.Right; x += 7f * S + 2f) dc.DrawLine(new Vector2(x, b.Top), new Vector2(x, b.Bottom), brush, 1.2f);
        brush.Color = Gfx.C4(0.75f, 0.12f, 0.12f, 1f);
        dc.FillRectangle(new RawRectF(b.Left, b.Top + baseH * 0.25f, b.Right, b.Top + baseH * 0.35f), brush);
        dc.FillRectangle(new RawRectF(b.Left, b.Top + baseH * 0.62f, b.Right, b.Top + baseH * 0.72f), brush);
        // 梅の飾り
        brush.Color = Gfx.C4(0.95f, 0.3f, 0.45f, 1f);
        for (int i = 0; i < 3; i++)
            dc.FillEllipse(new Ellipse(new Vector2(bottom.X + (i - 1) * 40f * S, bottom.Y - baseH - 12f * S + MathF.Sin(t + i) * 2f), 10f * S, 10f * S), brush);
    }
}
