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

public enum ItemIcon
{
    [Display(Name = "剣")] Sword,
    [Display(Name = "薬（ポーション）")] Potion,
    [Display(Name = "鍵")] Key,
    [Display(Name = "宝石")] Gem,
    [Display(Name = "コイン")] Coin,
    [Display(Name = "宝箱")] Chest,
    [Display(Name = "巻物")] Scroll,
}

/// <summary>アイテムの絵(ゲーム風の、単純な形)</summary>
internal static class ItemIconKit
{
    public static void Draw(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, ItemIcon icon, Vector2 c, float r, float rot, float a)
    {
        Vector2 P(float x, float y) => c + DrawKit.Rot(new Vector2(x * r, y * r), Vector2.Zero, rot);
        List<Vector2> Poly(params float[] xy) { var l = new List<Vector2>(); for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(P(xy[i], xy[i + 1])); return l; }
        var outline = Gfx.C4(0.08f, 0.06f, 0.05f, a);
        void Fill(List<Vector2> pts, Color4 col)
        {
            brush.Color = outline;
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(pts), MathF.Max(2f, r * 0.07f));
            brush.Color = col;
            Gfx.FillPoly(dc, brush, pts);
        }
        switch (icon)
        {
            case ItemIcon.Sword:
                Fill(Poly(-0.09f, 0.35f, -0.09f, -0.85f, 0f, -1f, 0.09f, -0.85f, 0.09f, 0.35f), Gfx.C4(0.85f, 0.9f, 0.96f, a));
                Fill(Poly(-0.42f, 0.35f, 0.42f, 0.35f, 0.42f, 0.47f, -0.42f, 0.47f), Gfx.C4(0.95f, 0.75f, 0.2f, a));
                Fill(Poly(-0.07f, 0.47f, 0.07f, 0.47f, 0.07f, 0.85f, -0.07f, 0.85f), Gfx.C4(0.45f, 0.25f, 0.12f, a));
                brush.Color = Gfx.C4(0.95f, 0.75f, 0.2f, a);
                dc.FillEllipse(new Ellipse(P(0f, 0.92f), r * 0.11f, r * 0.11f), brush);
                break;
            case ItemIcon.Potion:
            {
                var body = DrawKit.EllipsePts(P(0f, 0.25f), r * 0.55f, r * 0.55f, 40);
                Fill(Poly(-0.16f, -0.65f, 0.16f, -0.65f, 0.16f, -0.25f, -0.16f, -0.25f), Gfx.C4(0.85f, 0.9f, 0.96f, a));
                Fill(body, Gfx.C4(0.95f, 0.25f, 0.4f, a));
                Fill(Poly(-0.22f, -0.85f, 0.22f, -0.85f, 0.22f, -0.65f, -0.22f, -0.65f), Gfx.C4(0.6f, 0.4f, 0.25f, a));
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.7f * a);
                dc.FillEllipse(new Ellipse(P(-0.2f, 0.1f), r * 0.1f, r * 0.16f), brush);
                break;
            }
            case ItemIcon.Key:
                brush.Color = outline;
                dc.DrawEllipse(new Ellipse(P(0f, -0.5f), r * 0.32f, r * 0.32f), brush, r * 0.22f);
                brush.Color = Gfx.C4(0.98f, 0.8f, 0.25f, a);
                dc.DrawEllipse(new Ellipse(P(0f, -0.5f), r * 0.32f, r * 0.32f), brush, r * 0.14f);
                Fill(Poly(-0.07f, -0.2f, 0.07f, -0.2f, 0.07f, 0.9f, -0.07f, 0.9f), Gfx.C4(0.98f, 0.8f, 0.25f, a));
                Fill(Poly(0.07f, 0.55f, 0.3f, 0.55f, 0.3f, 0.68f, 0.07f, 0.68f), Gfx.C4(0.98f, 0.8f, 0.25f, a));
                Fill(Poly(0.07f, 0.75f, 0.24f, 0.75f, 0.24f, 0.87f, 0.07f, 0.87f), Gfx.C4(0.98f, 0.8f, 0.25f, a));
                break;
            case ItemIcon.Gem:
                Fill(Poly(-0.6f, -0.25f, -0.3f, -0.65f, 0.3f, -0.65f, 0.6f, -0.25f, 0f, 0.8f), Gfx.C4(0.3f, 0.85f, 0.95f, a));
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.6f * a);
                Gfx.FillPoly(dc, brush, Poly(-0.3f, -0.65f, 0f, -0.25f, -0.6f, -0.25f));
                brush.Color = Gfx.C4(0f, 0.3f, 0.45f, 0.4f * a);
                Gfx.FillPoly(dc, brush, Poly(0f, -0.25f, 0.6f, -0.25f, 0f, 0.8f));
                break;
            case ItemIcon.Coin:
                brush.Color = outline;
                dc.FillEllipse(new Ellipse(c, r * 0.8f, r * 0.8f), brush);
                brush.Color = Gfx.C4(0.98f, 0.78f, 0.2f, a);
                dc.FillEllipse(new Ellipse(c, r * 0.74f, r * 0.74f), brush);
                brush.Color = Gfx.C4(0.85f, 0.6f, 0.1f, a);
                dc.DrawEllipse(new Ellipse(c, r * 0.55f, r * 0.55f), brush, r * 0.06f);
                brush.Color = Gfx.C4(1f, 1f, 0.85f, 0.8f * a);
                dc.FillEllipse(new Ellipse(P(-0.25f, -0.3f), r * 0.12f, r * 0.08f), brush);
                break;
            case ItemIcon.Chest:
                Fill(Poly(-0.8f, -0.1f, 0.8f, -0.1f, 0.8f, 0.7f, -0.8f, 0.7f), Gfx.C4(0.6f, 0.36f, 0.16f, a));
                Fill(Poly(-0.8f, -0.1f, -0.7f, -0.55f, 0.7f, -0.55f, 0.8f, -0.1f), Gfx.C4(0.7f, 0.43f, 0.2f, a));
                Fill(Poly(-0.12f, -0.2f, 0.12f, -0.2f, 0.12f, 0.15f, -0.12f, 0.15f), Gfx.C4(0.98f, 0.8f, 0.25f, a));
                break;
            default:
                Fill(Poly(-0.6f, -0.55f, 0.6f, -0.55f, 0.6f, 0.55f, -0.6f, 0.55f), Gfx.C4(0.96f, 0.9f, 0.75f, a));
                Fill(Poly(-0.75f, -0.7f, -0.55f, -0.7f, -0.55f, 0.7f, -0.75f, 0.7f), Gfx.C4(0.8f, 0.6f, 0.35f, a));
                Fill(Poly(0.55f, -0.7f, 0.75f, -0.7f, 0.75f, 0.7f, 0.55f, 0.7f), Gfx.C4(0.8f, 0.6f, 0.35f, a));
                brush.Color = Gfx.C4(0.5f, 0.35f, 0.25f, 0.6f * a);
                for (int i = 0; i < 3; i++) dc.DrawLine(P(-0.4f, -0.25f + i * 0.22f), P(0.4f, -0.25f + i * 0.22f), brush, MathF.Max(1.5f, r * 0.05f));
                break;
        }
    }
}

// =====================================================================
//  アイテム入手の窓
//   アイテムがくるっと現れて、後ろで光が回り、「○○を手に入れた！」の窓が出る
// =====================================================================
public sealed class ItemGetPlugin : IShapePlugin
{
    public string Name => "演出：アイテム入手の窓";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ItemGetParameter(sharedData);
}

public sealed class ItemGetParameter : EffectShapeParameterBase
{
    public ItemGetParameter() : this(null) { }
    public ItemGetParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "アイテムの名前")]
    [TextEditor(AcceptsReturn = false)]
    public string ItemName { get => itemName; set => Set(ref itemName, value); }
    string itemName = "伝説の剣";

    [Display(GroupName = "文字", Name = "文の形（{item} がアイテム名になる）")]
    [TextEditor(AcceptsReturn = false)]
    public string Message { get => message; set => Set(ref message, value); }
    string message = "{item}を手に入れた！";

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "見た目", Name = "アイテムの絵")]
    [EnumComboBox]
    public ItemIcon Icon { get => icon; set => Set(ref icon, value); }
    ItemIcon icon = ItemIcon.Sword;

    [Display(GroupName = "見た目", Name = "窓の形")]
    [EnumComboBox]
    public GameWindowStyle Style { get => style; set => Set(ref style, value); }
    GameWindowStyle style = GameWindowStyle.Rpg;

    [Display(GroupName = "見た目", Name = "光の色")]
    [ColorPicker]
    public WpfColor Accent { get => accent; set => Set(ref accent, value); }
    WpfColor accent = WpfColor.FromRgb(0xFF, 0xE0, 0x6A);

    [Display(GroupName = "位置", Name = "アイテムのX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "アイテムのY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(40, -100, 200);

    [Display(GroupName = "位置", Name = "大きさ")]
    [AnimationSlider("F0", "%", 30, 250)]
    public Animation Scale { get; } = new Animation(100, 5, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [PosX, PosY, Scale];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.2f, 0.25f);
        if (k <= 0.001f) return;
        float u = c.F(Scale) / 100f;
        var ic = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float r = 120f * u;
        using var brush = dc.CreateSolidColorBrush(default);

        // 後ろで回る光の筋
        float rays = DrawKit.EaseOut(Math.Clamp(t / 0.5f, 0f, 1f));
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        for (int i = 0; i < 12; i++)
        {
            float a0 = t * 0.6f + i * MathF.PI * 2f / 12f;
            float len = r * 3f * rays;
            var poly = new List<Vector2> { ic, ic + new Vector2(MathF.Cos(a0 - 0.08f), MathF.Sin(a0 - 0.08f)) * len, ic + new Vector2(MathF.Cos(a0 + 0.08f), MathF.Sin(a0 + 0.08f)) * len };
            using var g = Gfx.Radial(dc, ic, len, (0f, Gfx.C4(Accent, 0.45f * k)), (1f, Gfx.C4(Accent, 0f)));
            Gfx.FillPoly(dc, g, poly);
        }
        DrawKit.Glow(dc, ic, r * 1.6f, Gfx.C4(Accent, 0.6f * k));
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        // キラキラ
        for (int i = 0; i < 8; i++)
        {
            float tw = MathF.Max(0f, MathF.Sin(t * (3f + DrawKit.H(Seed, i, 1) * 3f) + DrawKit.H(Seed, i) * 6.28f));
            var sp = ic + new Vector2(MathF.Cos(i * 0.8f + 0.3f), MathF.Sin(i * 0.8f + 0.3f)) * r * (1.1f + DrawKit.H(Seed, i, 2) * 0.6f);
            brush.Color = Gfx.C4(1f, 1f, 0.9f, tw * k);
            Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(sp, r * 0.12f * tw, 4, 0.25f, 0f));
        }
        // アイテム: 下から飛び出して、くるっと回る
        float pop = Math.Clamp(t / 0.6f, 0f, 1f);
        float hop = MathF.Sin(pop * MathF.PI) * r * 0.5f * (1f - pop);
        float spin = (1f - DrawKit.EaseOut(pop)) * MathF.PI * 2f;
        float bob = MathF.Sin(t * 2.5f) * r * 0.05f;
        ItemIconKit.Draw(dc, brush, Icon, ic + new Vector2(0f, -hop + bob), r * DrawKit.EaseOutBack(pop), spin, k);

        // 窓
        float wp = Math.Clamp((t - 0.45f) / 0.25f, 0f, 1f);
        if (wp <= 0f) return;
        string item = TextKit.Lines(ItemName).Trim();
        string msg = TextKit.Lines(Message).Replace("{item}", "「" + item + "」");
        var ms = TextKit.Get(msg, 50f * u, true, Font);
        float ww = ms.Width + 120f * u, wh = ms.Height + 70f * u;
        var wc = new Vector2(ic.X, ic.Y + r * 1.9f + wh / 2f);
        float open = DrawKit.EaseOut(wp);
        var rect = new RawRectF(wc.X - ww / 2f * open, wc.Y - wh / 2f, wc.X + ww / 2f * open, wc.Y + wh / 2f);
        GameWindowKit.Window(dc, brush, Style, rect, Accent, k, u * 1.2f);
        if (wp >= 1f) TextKit.DrawCentered(dc, brush, ms, wc, Gfx.C4(GameWindowKit.TextColorFor(Style), k));
    }
}
