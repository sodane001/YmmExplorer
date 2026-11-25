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
//  ランキング表
//   下の順位から1行ずつせり上がり、最後に1位が光る。1行に「名前,値」
// =====================================================================
public sealed class RankingBoardPlugin : IShapePlugin
{
    public string Name => "演出：ランキング表";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RankingBoardParameter(sharedData);
}

public sealed class RankingBoardParameter : EffectShapeParameterBase
{
    public RankingBoardParameter() : this(null) { }
    public RankingBoardParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "データ", Name = "見出し")]
    [TextEditor(AcceptsReturn = false)]
    public string Title { get => title; set => Set(ref title, value); }
    string title = "人気ランキング";

    [Display(GroupName = "データ", Name = "順位（1行に「名前,値」。上が1位）")]
    [TextEditor(AcceptsReturn = true)]
    public string Items { get => items; set => Set(ref items, value); }
    string items = "カレー,1,234票\nラーメン,987票\n寿司,850票\n焼肉,612票\nうどん,455票";

    [Display(GroupName = "見た目", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "行の高さ")]
    [AnimationSlider("F0", "px", 40, 200)]
    public Animation RowHeight { get; } = new Animation(104, 10, 2000);

    [Display(GroupName = "見た目", Name = "表の幅")]
    [AnimationSlider("F0", "px", 300, 1800)]
    public Animation BoardWidth { get; } = new Animation(1100, 50, 8000);

    [Display(GroupName = "見た目", Name = "行の色")]
    [ColorPicker]
    public WpfColor RowColor { get => rowColor; set => Set(ref rowColor, value); }
    WpfColor rowColor = WpfColor.FromArgb(0xE8, 0x1E, 0x22, 0x2C);

    [Display(GroupName = "見た目", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(52, -100, 200);

    [Display(GroupName = "動き", Name = "1行ずつ出る間隔")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation Interval { get; } = new Animation(0.6, 0, 60);

    [Display(GroupName = "動き", Name = "下の順位から出す")]
    [ToggleSlider]
    public bool FromBottom { get => fromBottom; set => Set(ref fromBottom, value); }
    bool fromBottom = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [RowHeight, BoardWidth, PosX, PosY, Interval];

    internal static WpfColor MedalColor(int rank) => rank switch
    {
        1 => WpfColor.FromRgb(0xF5, 0xC2, 0x2B),
        2 => WpfColor.FromRgb(0xC5, 0xCC, 0xD6),
        3 => WpfColor.FromRgb(0xD3, 0x8A, 0x4E),
        _ => WpfColor.FromRgb(0x5A, 0x63, 0x75),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var rows = TextKit.Lines(Items).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        if (rows.Count == 0) return;
        float k = DrawKit.InOut(c, 0.3f, 0.25f);
        if (k <= 0.001f) return;
        float rh = c.F(RowHeight), bw = c.F(BoardWidth), iv = c.F(Interval);
        float gap = rh * 0.14f;
        string title = TextKit.Lines(Title).Trim();
        float titleH = title.Length > 0 ? rh * 0.9f : 0f;
        float totalH = titleH + rows.Count * (rh + gap);
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float top = ctr.Y - totalH / 2f, left = ctr.X - bw / 2f;
        using var brush = dc.CreateSolidColorBrush(default);

        if (titleH > 0f)
        {
            var ts = TextKit.Get(title, rh * 0.55f, true, Font);
            TextKit.DrawCentered(dc, brush, ts, new Vector2(ctr.X, top + titleH * 0.45f), Gfx.C4(TextColor, k), Gfx.C4(0f, 0f, 0f, 0.6f * k), rh * 0.03f);
        }
        float lastReveal = 0f;
        for (int i = 0; i < rows.Count; i++)
        {
            int order = FromBottom ? rows.Count - 1 - i : i;
            float at = 0.2f + order * iv;
            lastReveal = MathF.Max(lastReveal, at);
            float p = Math.Clamp((t - at) / 0.35f, 0f, 1f);
            if (p <= 0f) continue;
            float e = DrawKit.EaseOutBack(p);
            int rank = i + 1;
            var parts = rows[i].Split(new[] { ',', '、' }, 2);
            string name = parts[0].Trim(), val = parts.Length > 1 ? parts[1].Trim() : "";
            float y = top + titleH + i * (rh + gap) + rh / 2f;
            float x = ctr.X + (1f - e) * bw * 0.6f;
            float a = k * MathF.Min(1f, p * 2f);
            var row = ShapeGeom.RoundRect(x, y, bw, rh, rh * 0.18f);
            var medal = MedalColor(rank);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f * a);
            Gfx.FillPoly(dc, brush, row.Select(q => q + new Vector2(0f, rh * 0.06f)).ToList());
            brush.Color = Gfx.C4(rank <= 3 ? Gfx.Mix(RowColor, medal, 0.18f) : RowColor, a);
            Gfx.FillPoly(dc, brush, row);
            if (rank <= 3)
            {
                brush.Color = Gfx.C4(medal, a);
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(row), MathF.Max(2f, rh * 0.03f));
            }
            // 順位のメダル
            var mc = new Vector2(x - bw / 2f + rh * 0.65f, y);
            brush.Color = Gfx.C4(medal, a);
            dc.FillEllipse(new Ellipse(mc, rh * 0.36f, rh * 0.36f), brush);
            var rs = TextKit.Get(rank.ToString(), rh * 0.42f, true, Font);
            TextKit.DrawCentered(dc, brush, rs, mc, Gfx.C4(rank <= 3 ? WpfColor.FromRgb(0x30, 0x22, 0x08) : WpfColor.FromRgb(255, 255, 255), a));
            var ns = TextKit.Get(name, rh * 0.42f, true, Font);
            TextKit.Draw(dc, brush, ns, new Vector2(mc.X + rh * 0.6f, y - ns.Height / 2f), Gfx.C4(TextColor, a));
            if (val.Length > 0)
            {
                var vs = TextKit.Get(val, rh * 0.36f, true, Font);
                TextKit.Draw(dc, brush, vs, new Vector2(x + bw / 2f - rh * 0.4f - vs.Width, y - vs.Height / 2f), Gfx.C4(rank <= 3 ? medal : TextColor, a));
            }
            // 1位: 出そろった後に光が走り、王冠が乗る
            if (rank == 1)
            {
                float st = t - (FromBottom ? at : 0.2f + (rows.Count - 1) * iv) - 0.4f;
                if (st > 0f)
                {
                    DrawKit.Glow(dc, mc, rh * 1.1f, Gfx.C4(medal, 0.35f * a * (0.7f + 0.3f * MathF.Sin(t * 4f))));
                    float sw = DrawKit.Frac(st * 0.6f);
                    float sx = x - bw / 2f + (bw + rh) * sw - rh / 2f;
                    var band = new List<Vector2> { new(sx, y - rh / 2f), new(sx + rh * 0.4f, y - rh / 2f), new(sx, y + rh / 2f), new(sx - rh * 0.4f, y + rh / 2f) };
                    brush.Color = Gfx.C4(1f, 1f, 1f, 0.2f * a);
                    Gfx.FillPoly(dc, brush, DrawKit.ClipRect(band, x - bw / 2f, y - rh / 2f, x + bw / 2f, y + rh / 2f));
                    float cp = DrawKit.EaseOutBack(Math.Clamp(st / 0.4f, 0f, 1f));
                    IconKit.Draw(dc, brush, AchievementIcon.Crown, mc + new Vector2(0f, -rh * 0.5f), rh * 0.3f * cp, medal, a);
                }
            }
        }
    }
}
