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
//  ステータス画面
//   名前・レベル・HP/MP のバーと、能力値のレーダーチャート(六角形など)。
//   能力値は「攻撃:80,防御:60…」(0〜100)
// =====================================================================
public sealed class StatusScreenPlugin : IShapePlugin
{
    public string Name => "演出：ステータス画面";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StatusScreenParameter(sharedData);
}

public sealed class StatusScreenParameter : EffectShapeParameterBase
{
    public StatusScreenParameter() : this(null) { }
    public StatusScreenParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "ステータス", Name = "名前")]
    [TextEditor(AcceptsReturn = false)]
    public string CharName { get => charName; set => Set(ref charName, value); }
    string charName = "ゆっくり霊夢";

    [Display(GroupName = "ステータス", Name = "職業・肩書き")]
    [TextEditor(AcceptsReturn = false)]
    public string Job { get => job; set => Set(ref job, value); }
    string job = "巫女";

    [Display(GroupName = "ステータス", Name = "レベル")]
    [AnimationSlider("F0", "", 1, 99)]
    public Animation Level { get; } = new Animation(42, 0, 9999);

    [Display(GroupName = "ステータス", Name = "HP")]
    [AnimationSlider("F0", "", 0, 999)]
    public Animation Hp { get; } = new Animation(820, 0, 999999);

    [Display(GroupName = "ステータス", Name = "HPの最大")]
    [AnimationSlider("F0", "", 1, 999)]
    public Animation HpMax { get; } = new Animation(999, 1, 999999);

    [Display(GroupName = "ステータス", Name = "MP")]
    [AnimationSlider("F0", "", 0, 999)]
    public Animation Mp { get; } = new Animation(240, 0, 999999);

    [Display(GroupName = "ステータス", Name = "MPの最大")]
    [AnimationSlider("F0", "", 1, 999)]
    public Animation MpMax { get; } = new Animation(400, 1, 999999);

    [Display(GroupName = "ステータス", Name = "能力値（「名前:値」をカンマ区切り。0〜100）")]
    [TextEditor(AcceptsReturn = false)]
    public string Stats { get => stats; set => Set(ref stats, value); }
    string stats = "攻撃:80,防御:55,素早さ:90,魔力:70,運:40,体力:65";

    [Display(GroupName = "見た目", Name = "窓の形")]
    [EnumComboBox]
    public GameWindowStyle Style { get => style; set => Set(ref style, value); }
    GameWindowStyle style = GameWindowStyle.Modern;

    [Display(GroupName = "見た目", Name = "アクセントの色")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0x4F, 0xC3, 0xF7);

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 250)]
    public Animation Scale { get; } = new Animation(100, 5, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Level, Hp, HpMax, Mp, MpMax, PosX, PosY, Scale];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        float u = c.F(Scale) / 100f;
        float ww = 1200f * u, wh = 620f * u;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float open = DrawKit.EaseOut(k);
        var rect = new RawRectF(ctr.X - ww / 2f, ctr.Y - wh / 2f * open, ctr.X + ww / 2f, ctr.Y + wh / 2f * open);
        using var brush = dc.CreateSolidColorBrush(default);
        GameWindowKit.Window(dc, brush, Style, rect, Accent, k, u * 1.4f);
        if (open < 0.9f) return;
        float a = k * Math.Clamp((open - 0.9f) / 0.1f, 0f, 1f);
        var txt = GameWindowKit.TextColorFor(Style);
        float grow = DrawKit.EaseOut(Math.Clamp((t - 0.35f) / 0.9f, 0f, 1f));

        // 左: 名前・レベル・HP・MP
        float lx = rect.Left + 60f * u, ly = rect.Top + 50f * u;
        var ns = TextKit.Get(TextKit.Lines(CharName), 62f * u, true, Font);
        TextKit.Draw(dc, brush, ns, new Vector2(lx, ly), Gfx.C4(txt, a));
        var js = TextKit.Get(TextKit.Lines(Job), 34f * u, false, Font);
        TextKit.Draw(dc, brush, js, new Vector2(lx, ly + ns.Height + 4f * u), Gfx.C4(Accent, a));
        var lv = TextKit.Get("Lv " + MathF.Round(c.F(Level)).ToString(CultureInfo.InvariantCulture), 48f * u, true, Font);
        TextKit.Draw(dc, brush, lv, new Vector2(lx + 540f * u - lv.Width, ly + 8f * u), Gfx.C4(Accent, a));

        void Bar(string label, float v, float max, float y, WpfColor col)
        {
            float bw = 540f * u, bh = 30f * u;
            float f = Math.Clamp(v / MathF.Max(1f, max), 0f, 1f) * grow;
            var ls = TextKit.Get(label, 34f * u, true, Font);
            TextKit.Draw(dc, brush, ls, new Vector2(lx, y), Gfx.C4(txt, a));
            var vs = TextKit.Get($"{MathF.Round(v * grow).ToString(CultureInfo.InvariantCulture)} / {MathF.Round(max).ToString(CultureInfo.InvariantCulture)}", 30f * u, true, Font);
            TextKit.Draw(dc, brush, vs, new Vector2(lx + bw - vs.Width, y + 2f * u), Gfx.C4(txt, a));
            float by = y + ls.Height + 6f * u;
            var back = ShapeGeom.RoundRect(lx + bw / 2f, by + bh / 2f, bw, bh, bh / 2f);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.45f * a);
            Gfx.FillPoly(dc, brush, back);
            if (f > 0.005f)
            {
                var fill = ShapeGeom.RoundRect(lx + bw * f / 2f, by + bh / 2f, MathF.Max(bh, bw * f), bh, bh / 2f);
                using var g = Gfx.Linear(dc, new Vector2(lx, by), new Vector2(lx, by + bh), (0f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), 0.35f), a)), (1f, Gfx.C4(col, a)));
                Gfx.FillPoly(dc, g, fill);
            }
        }
        float hp = c.F(Hp), hpMax = c.F(HpMax), mp = c.F(Mp), mpMax = c.F(MpMax);
        var hpCol = hp / MathF.Max(1f, hpMax) < 0.25f ? WpfColor.FromRgb(0xEF, 0x44, 0x44) : WpfColor.FromRgb(0x4A, 0xDE, 0x80);
        Bar("HP", hp, hpMax, ly + 200f * u, hpCol);
        Bar("MP", mp, mpMax, ly + 320f * u, WpfColor.FromRgb(0x60, 0xA5, 0xFA));

        // 右: 能力値のレーダーチャート
        var statList = new List<(string n, float v)>();
        foreach (var part in TextKit.Split(Stats))
        {
            int colon = part.IndexOfAny(new[] { ':', '：' });
            if (colon <= 0) continue;
            float.TryParse(part[(colon + 1)..].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var sv);
            statList.Add((part[..colon].Trim(), Math.Clamp(sv, 0f, 100f)));
        }
        int m = statList.Count;
        if (m < 3) return;
        var rc = new Vector2(rect.Right - 300f * u, ctr.Y + 10f * u);
        float R = 190f * u;
        Vector2 Corner(int i, float r) { float ang = -MathF.PI / 2f + i * MathF.PI * 2f / m; return rc + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * r; }
        brush.Color = Gfx.C4(txt, 0.2f * a);
        for (int ring = 1; ring <= 4; ring++)
        {
            var poly = Enumerable.Range(0, m).Select(i => Corner(i, R * ring / 4f)).ToList();
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(poly), 1.5f * u);
        }
        for (int i = 0; i < m; i++) dc.DrawLine(rc, Corner(i, R), brush, 1.5f * u);
        var shape = Enumerable.Range(0, m).Select(i => Corner(i, R * statList[i].v / 100f * grow)).ToList();
        brush.Color = Gfx.C4(Accent, 0.4f * a);
        Gfx.FillPoly(dc, brush, shape);
        brush.Color = Gfx.C4(Accent, a);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(shape), 3f * u);
        for (int i = 0; i < m; i++)
        {
            dc.FillEllipse(new Ellipse(shape[i], 6f * u, 6f * u), brush);
            var ls = TextKit.Get(statList[i].n, 30f * u, true, Font);
            var lp = Corner(i, R + 42f * u);
            TextKit.DrawCentered(dc, brush, ls, lp, Gfx.C4(txt, a));
        }
    }
}
