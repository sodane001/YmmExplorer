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
//  次元の裂け目・ポータル
//   開いていく異空間の入り口。渦・火花・稲妻つき
// =====================================================================
public sealed class PortalPlugin : IShapePlugin
{
    public string Name => "演出：次元の裂け目・ポータル";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PortalParameter(sharedData);
}

public sealed class PortalParameter : EffectShapeParameterBase
{
    public PortalParameter() : this(null) { }
    public PortalParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public PortalKind Kind { get => kind; set => Set(ref kind, value); }
    PortalKind kind = PortalKind.Portal;

    [Display(GroupName = "色", Name = "外側の色")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0xB2, 0x5C, 0xFF);

    [Display(GroupName = "色", Name = "内側の色")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0x4F, 0xE8, 0xFF);

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ（ポータル=半径 / 裂け目=高さの半分）")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Size { get; } = new Animation(260, 5, 5000);

    [Display(GroupName = "位置と大きさ", Name = "裂け目の幅")]
    [AnimationSlider("F0", "px", 10, 300)]
    public Animation RiftWidth { get; } = new Animation(70, 2, 3000);

    [Display(GroupName = "動き", Name = "渦の回転速度")]
    [AnimationSlider("F0", "%", -300, 300)]
    public Animation Spin { get; } = new Animation(100, -2000, 2000);

    [Display(GroupName = "動き", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "動き", Name = "火花の数")]
    [AnimationSlider("F0", "個", 0, 200)]
    public Animation Sparks { get; } = new Animation(50, 0, 2000);

    [Display(GroupName = "動き", Name = "開くアニメーション")]
    [ToggleSlider]
    public bool Intro { get => intro; set => Set(ref intro, value); }
    bool intro = true;

    [Display(GroupName = "動き", Name = "開くまでの時間")]
    [AnimationSlider("F1", "秒", 0.2, 6)]
    public Animation IntroTime { get; } = new Animation(1.5, 0.05, 30);

    [Display(GroupName = "動き", Name = "開き具合（手動）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Open { get; } = new Animation(100, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Size, RiftWidth, Spin, Brightness, Sparks, IntroTime, Open];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float open = c.F(Open) / 100f;
        if (Intro) open *= Gfx.Smooth(0f, MathF.Max(0.1f, c.F(IntroTime)), t);
        if (open <= 0.003f) return;
        float ease = 1f - (1f - open) * (1f - open);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float S = c.F(Size), bright = c.F(Brightness) / 100f, spin = c.F(Spin) / 100f * t;
        int ns = (int)c.F(Sparks);
        var white = WpfColor.FromRgb(255, 255, 255);
        var hot = Gfx.Mix(Color2, white, 0.55f);
        var deep = Gfx.Mix(Color1, WpfColor.FromRgb(0, 0, 0), 0.8f);

        using var brush = dc.CreateSolidColorBrush(default);

        if (Kind == PortalKind.Portal)
        {
            float R = S * ease;
            // 揺らぐ縁
            var edge = new List<Vector2>();
            for (int i = 0; i < 120; i++)
            {
                float a = i / 120f * MathF.PI * 2f;
                float r = R * (1f + 0.035f * MathF.Sin(5f * a + t * 3f) + 0.02f * MathF.Sin(11f * a - t * 4.3f));
                edge.Add(ctr + new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r));
            }

            // 異空間（暗い中身）
            brush.Color = Gfx.C4(deep, 0.95f * open);
            Gfx.FillPoly(dc, brush, edge);

            dc.PrimitiveBlend = PrimitiveBlend.Add;
            try
            {
                // 外側のにじみ
                float og = R * 1.5f;
                using (var ob = Gfx.Radial(dc, ctr, og,
                    (0.6f, Gfx.C4(Color1, 0f)), (0.68f, Gfx.C4(Color1, 0.30f * bright * open)), (1f, Gfx.C4(Color1, 0f))))
                    dc.FillEllipse(new Ellipse(ctr, og, og), ob);

                // 中心へ向かう渦
                for (int k = 0; k < 4; k++)
                {
                    float a0 = k * MathF.PI / 2f + spin * 1.4f;
                    var pts = new List<Vector2>();
                    for (int i = 0; i <= 50; i++)
                    {
                        float f = i / 50f;
                        float r = R * 0.95f * (1f - f);
                        float a = a0 + f * 5f;
                        pts.Add(ctr + new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r));
                    }
                    brush.Color = Gfx.C4(Color2, 0.10f * bright * open);
                    FormKit.Stroke(dc, brush, pts, R * 0.12f, false);
                    brush.Color = Gfx.C4(hot, 0.55f * bright * open);
                    FormKit.Stroke(dc, brush, pts, 3f, false);
                }

                // 中心の光
                float cg = R * 0.4f;
                using (var cb = Gfx.Radial(dc, ctr, cg, (0f, Gfx.C4(white, 0.7f * bright * open)), (0.4f, Gfx.C4(Color2, 0.35f * bright * open)), (1f, Gfx.C4(Color2, 0f))))
                    dc.FillEllipse(new Ellipse(ctr, cg, cg), cb);

                // 縁の輪（二重）
                var closed = ShapeGeom.Closed(edge);
                brush.Color = Gfx.C4(Color1, 0.30f * bright * open);
                Gfx.Polyline(dc, brush, closed, 14f);
                brush.Color = Gfx.C4(Color1, 0.9f * bright * open);
                Gfx.Polyline(dc, brush, closed, 4f);
                brush.Color = Gfx.C4(white, 0.8f * bright * open);
                Gfx.Polyline(dc, brush, closed, 1.6f);

                // 縁を周回する火花
                var rs = new Random(Seed);
                for (int i = 0; i < ns; i++)
                {
                    float a0 = rs.NextF() * MathF.PI * 2f, sp = 0.6f + rs.NextF(), ph = rs.NextF(), rr = 0.95f + 0.35f * rs.NextF();
                    float u = Gfx.Mod(t * 0.5f * sp + ph, 1f);
                    float a = a0 + t * 0.8f * sp;
                    var p = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * (R * (rr + 0.25f * u));
                    brush.Color = Gfx.C4(hot, 0.9f * (1f - u) * bright * open);
                    dc.FillEllipse(new Ellipse(p, 2.4f * (1f - u) + 0.8f, 2.4f * (1f - u) + 0.8f), brush);
                }
            }
            finally
            {
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }
        else
        {
            // ---- 次元の裂け目：縦にギザギザに裂ける ----
            float H = S * ease, W = c.F(RiftWidth) * ease;
            var rg = new Random(Seed);
            const int N = 24;
            var left = new List<Vector2>();
            var right = new List<Vector2>();
            for (int i = 0; i <= N; i++)
            {
                float f = i / (float)N;
                float y = ctr.Y - H + f * H * 2f;
                float prof = MathF.Pow(MathF.Sin(f * MathF.PI), 0.7f);                      // 中央が最も開く
                float jag = 0.65f + 0.7f * rg.NextF();
                float breath = 1f + 0.06f * MathF.Sin(t * 2f + f * 8f);
                float xw = W * 0.5f * prof * jag * breath;
                float cx = ctr.X + (rg.NextF() - 0.5f) * W * 0.25f * prof + MathF.Sin(t * 0.7f + f * 5f) * 4f;
                left.Add(new Vector2(cx - xw, y));
                right.Add(new Vector2(cx + xw, y));
            }
            var poly = new List<Vector2>(left);
            for (int i = right.Count - 1; i >= 0; i--) poly.Add(right[i]);

            // 縁のにじみ→内側の異空間→明るい芯
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            try
            {
                float gr = H * 0.9f;
                using (var gb = Gfx.Radial(dc, ctr, gr, (0f, Gfx.C4(Color1, 0.30f * bright * open)), (1f, Gfx.C4(Color1, 0f))))
                    dc.FillEllipse(new Ellipse(ctr, gr * 0.5f, gr), gb);
            }
            finally
            {
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
            brush.Color = Gfx.C4(deep, 0.97f * open);
            Gfx.FillPoly(dc, brush, poly);

            dc.PrimitiveBlend = PrimitiveBlend.Add;
            try
            {
                // 内側に向かって明るくなる層（縮小したコピーを重ねる）
                for (int layer = 1; layer <= 4; layer++)
                {
                    float k = 1f - layer * 0.2f;
                    var inner = poly.Select(p => new Vector2(ctr.X + (p.X - ctr.X) * k, ctr.Y + (p.Y - ctr.Y) * (0.5f + 0.5f * k))).ToList();
                    brush.Color = Gfx.C4(Gfx.Mix(Color1, Color2, layer / 4f), 0.22f * bright * open);
                    Gfx.FillPoly(dc, brush, inner);
                }
                var edge = ShapeGeom.Closed(poly);
                brush.Color = Gfx.C4(Color1, 0.30f * bright * open);
                Gfx.Polyline(dc, brush, edge, 12f);
                brush.Color = Gfx.C4(hot, 0.9f * bright * open);
                Gfx.Polyline(dc, brush, edge, 3f);
                brush.Color = Gfx.C4(white, 0.8f * bright * open);
                Gfx.Polyline(dc, brush, edge, 1.2f);

                // 縁から走る稲妻
                int tick = (int)MathF.Floor(t * 14f);
                var rb = new Random(Seed * 31 + tick);
                for (int i = 0; i < 4; i++)
                {
                    var a = poly[rb.Next(0, poly.Count)];
                    var pts = new List<Vector2> { a };
                    var d = new Vector2((a.X - ctr.X) >= 0 ? 1f : -1f, (rb.NextF() - 0.5f) * 0.8f);
                    for (int k = 0; k < 6; k++)
                    {
                        a += d * (10f + 18f * rb.NextF()) + new Vector2(0f, (rb.NextF() - 0.5f) * 22f);
                        pts.Add(a);
                    }
                    brush.Color = Gfx.C4(hot, 0.8f * bright * open);
                    FormKit.Stroke(dc, brush, pts, 1.6f, false);
                }

                // 吸い込まれる火花
                var rs = new Random(Seed);
                for (int i = 0; i < ns; i++)
                {
                    float sp = 0.6f + rs.NextF(), ph = rs.NextF(), side = rs.NextF() < 0.5f ? -1f : 1f, yy = (rs.NextF() * 2f - 1f) * H * 0.8f;
                    float u = Gfx.Mod(t * 0.5f * sp + ph, 1f);
                    var p = new Vector2(ctr.X + side * (W * 0.4f + (1f - u) * W * 1.6f), ctr.Y + yy * (1f - u * 0.3f));
                    brush.Color = Gfx.C4(hot, 0.9f * u * (1f - u) * 3f * bright * open);
                    dc.FillEllipse(new Ellipse(p, 2f, 2f), brush);
                }
            }
            finally
            {
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }
    }
}
