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

public enum RouletteKind
{
    [Display(Name = "ルーレット（円盤）")] Wheel,
    [Display(Name = "スロット（縦に回る）")] Slot,
}

// =====================================================================
//  ルーレット・スロット
//   回って、だんだん遅くなり、決めた項目でぴたりと止まる
// =====================================================================
public sealed class RoulettePlugin : IShapePlugin
{
    public string Name => "演出：ルーレット・スロット";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RouletteParameter(sharedData);
}

public sealed class RouletteParameter : EffectShapeParameterBase
{
    public RouletteParameter() : this(null) { }
    public RouletteParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "項目", Name = "種類")]
    [EnumComboBox]
    public RouletteKind Kind { get => kind; set => Set(ref kind, value); }
    RouletteKind kind = RouletteKind.Wheel;

    [Display(GroupName = "項目", Name = "項目（カンマ区切り）")]
    [TextEditor(AcceptsReturn = false)]
    public string Items { get => items; set => Set(ref items, value); }
    string items = "大吉,中吉,小吉,吉,末吉,凶";

    [Display(GroupName = "項目", Name = "止まる項目の番号")]
    [TextBoxSlider("F0", "番", 1, 12)]
    [Range(1, 100)]
    public int StopAt { get => stopAt; set => Set(ref stopAt, value); }
    int stopAt = 1;

    [Display(GroupName = "動き", Name = "回る時間")]
    [AnimationSlider("F1", "秒", 0.5, 15)]
    public Animation SpinTime { get; } = new Animation(4, 0.1, 600);

    [Display(GroupName = "動き", Name = "回転の数")]
    [AnimationSlider("F0", "回", 1, 20)]
    public Animation Turns { get; } = new Animation(6, 0, 200);

    [Display(GroupName = "見た目", Name = "色の組み合わせ")]
    [EnumComboBox]
    public ChartPalette Palette { get => palette; set => Set(ref palette, value); }
    ChartPalette palette = ChartPalette.Vivid;

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(52, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ（半径）")]
    [AnimationSlider("F0", "px", 80, 600)]
    public Animation Radius { get; } = new Animation(360, 10, 5000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [SpinTime, Turns, PosX, PosY, Radius];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var list = TextKit.Split(Items).Where(s => s.Length > 0).ToList();
        int n = list.Count;
        if (n == 0) return;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        int stop = Math.Clamp(StopAt, 1, n) - 1;
        float st = MathF.Max(0.1f, c.F(SpinTime)), turns = c.F(Turns);
        float x = Math.Clamp(t / st, 0f, 1f);
        float prog = 1f - MathF.Pow(1f - x, 3.2f);          // だんだん遅く
        bool done = x >= 1f;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Radius);
        using var brush = dc.CreateSolidColorBrush(default);
        float sector = MathF.PI * 2f / n;

        if (Kind == RouletteKind.Wheel)
        {
            // 止まった時、止まる項目のまん中が真上(針)に来るように
            float finalRot = -(stop + 0.5f) * sector;
            float rot = finalRot - MathF.PI * 2f * turns * (1f - prog);
            float start = -MathF.PI / 2f + rot;
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f * k);
            dc.FillEllipse(new Ellipse(ctr + new Vector2(R * 0.03f, R * 0.05f), R * 1.04f, R * 1.04f), brush);
            brush.Color = Gfx.C4(0.15f, 0.12f, 0.1f, k);
            dc.FillEllipse(new Ellipse(ctr, R * 1.06f, R * 1.06f), brush);
            for (int i = 0; i < n; i++)
            {
                float a0 = start + i * sector, a1 = a0 + sector;
                var poly = new List<Vector2> { ctr };
                poly.AddRange(Gfx.ArcPts(ctr, R, a0, a1, Math.Max(6, 120 / n)));
                var col = BubbleKit.Palette((int)Palette, i);
                bool win = done && i == stop && DrawKit.Frac(t * 3f) < 0.5f;
                brush.Color = Gfx.C4(win ? Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.5f) : col, k);
                Gfx.FillPoly(dc, brush, poly);
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.85f * k);
                dc.DrawLine(ctr, ctr + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * R, brush, MathF.Max(2f, R * 0.012f));
                // 文字は、中心から外へ向けて書く
                float mid = (a0 + a1) / 2f;
                var s = TextKit.Get(list[i], R * MathF.Min(0.16f, 1.6f / n), true, Font);
                var prev = dc.Transform;
                var lp = ctr + new Vector2(MathF.Cos(mid), MathF.Sin(mid)) * R * 0.62f;
                dc.Transform = Matrix3x2.CreateRotation(mid, lp) * prev;
                TextKit.DrawCentered(dc, brush, s, lp, Gfx.C4(1f, 1f, 1f, k), Gfx.C4(0f, 0f, 0f, 0.45f * k), R * 0.008f);
                dc.Transform = prev;
            }
            // ふちの電球(回っている間は流れ、止まると点滅)
            int bulbs = 24;
            for (int i = 0; i < bulbs; i++)
            {
                float a = i * MathF.PI * 2f / bulbs;
                var bp = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * R * 1.03f;
                bool on = done ? DrawKit.Frac(t * 2f) < 0.5f == (i % 2 == 0) : (i + (int)(t * 12f)) % 3 == 0;
                brush.Color = on ? Gfx.C4(1f, 0.92f, 0.5f, k) : Gfx.C4(0.5f, 0.45f, 0.3f, k);
                dc.FillEllipse(new Ellipse(bp, R * 0.022f, R * 0.022f), brush);
                if (on) DrawKit.Glow(dc, bp, R * 0.06f, Gfx.C4(1f, 0.9f, 0.4f, 0.5f * k));
            }
            // 中心のつまみと、上の針
            brush.Color = Gfx.C4(0.95f, 0.95f, 0.97f, k);
            dc.FillEllipse(new Ellipse(ctr, R * 0.1f, R * 0.1f), brush);
            float tick = done ? 0f : MathF.Max(0f, MathF.Sin((rot / sector) * MathF.PI * 2f)) * 0.25f * (1f - prog);
            var tip = ctr + new Vector2(0f, -R * 0.93f);
            var needle = DrawKit.Place(new[] { new Vector2(0f, 0f), new Vector2(-R * 0.08f, -R * 0.17f), new Vector2(R * 0.08f, -R * 0.17f) }, tip, tick);
            brush.Color = Gfx.C4(0.1f, 0.1f, 0.12f, k);
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(needle), MathF.Max(2f, R * 0.012f));
            brush.Color = Gfx.C4(0.95f, 0.2f, 0.2f, k);
            Gfx.FillPoly(dc, brush, needle);
        }
        else
        {
            // スロット: 縦に流れる1列の窓
            float cellH = R * 0.55f, winW = R * 1.6f;
            float pos = (stop + n * MathF.Max(1f, turns)) * prog;      // いま、何個ぶん流れたか
            if (done) pos = stop + n * MathF.Max(1f, turns);
            float frac = pos - MathF.Floor(pos);
            int baseIdx = (int)MathF.Floor(pos);
            var frame = ShapeGeom.RoundRect(ctr.X, ctr.Y, winW + R * 0.2f, cellH * 3f + R * 0.2f, R * 0.08f);
            brush.Color = Gfx.C4(0.2f, 0.12f, 0.1f, k);
            Gfx.FillPoly(dc, brush, frame);
            var win = new RawRectF(ctr.X - winW / 2f, ctr.Y - cellH * 1.5f, ctr.X + winW / 2f, ctr.Y + cellH * 1.5f);
            brush.Color = Gfx.C4(0.98f, 0.97f, 0.94f, k);
            dc.FillRectangle(win, brush);
            dc.PushAxisAlignedClip(win, AntialiasMode.PerPrimitive);
            for (int j = -2; j <= 2; j++)
            {
                int idx = ((baseIdx + j) % n + n) % n;
                float y = ctr.Y + (j - frac) * cellH;
                var col = BubbleKit.Palette((int)Palette, idx);
                var s = TextKit.Get(list[idx], cellH * 0.48f, true, Font, winW * 0.9f, 1);
                // 速く回っている時は、縦にぶれて見える
                float blur = done ? 0f : (1f - prog) * cellH * 0.35f;
                if (blur > 1f) TextKit.DrawCentered(dc, brush, s, new Vector2(ctr.X, y - blur), Gfx.C4(col, 0.3f * k));
                TextKit.DrawCentered(dc, brush, s, new Vector2(ctr.X, y), Gfx.C4(col, k));
            }
            dc.PopAxisAlignedClip();
            // 上下の影と、まん中の当たりの線
            using (var g = Gfx.Linear(dc, new Vector2(0, win.Top), new Vector2(0, win.Top + cellH), (0f, Gfx.C4(0f, 0f, 0f, 0.55f * k)), (1f, Gfx.C4(0f, 0f, 0f, 0f))))
                dc.FillRectangle(new RawRectF(win.Left, win.Top, win.Right, win.Top + cellH), g);
            using (var g = Gfx.Linear(dc, new Vector2(0, win.Bottom), new Vector2(0, win.Bottom - cellH), (0f, Gfx.C4(0f, 0f, 0f, 0.55f * k)), (1f, Gfx.C4(0f, 0f, 0f, 0f))))
                dc.FillRectangle(new RawRectF(win.Left, win.Bottom - cellH, win.Right, win.Bottom), g);
            brush.Color = done && DrawKit.Frac(t * 3f) < 0.5f ? Gfx.C4(1f, 0.85f, 0.2f, k) : Gfx.C4(0.9f, 0.2f, 0.2f, 0.8f * k);
            dc.DrawRectangle(new RawRectF(win.Left - 4f, ctr.Y - cellH / 2f, win.Right + 4f, ctr.Y + cellH / 2f), brush, MathF.Max(3f, R * 0.015f));
            if (done) DrawKit.Glow(dc, ctr, winW * 0.6f, Gfx.C4(1f, 0.85f, 0.3f, 0.25f * k));
        }
    }
}
