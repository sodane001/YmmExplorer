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

public enum RoomKind
{
    [Display(Name = "教室")] Classroom,
    [Display(Name = "オフィス")] Office,
    [Display(Name = "寝室")] Bedroom,
    [Display(Name = "リビング")] Living,
}

public enum TimeOfDay
{
    [Display(Name = "昼")] Day,
    [Display(Name = "夕方")] Evening,
    [Display(Name = "夜")] Night,
}

// =====================================================================
//  部屋の背景
//   シンプルなイラスト調の部屋(教室・オフィス・寝室・リビング)。
//   窓の外の空は、昼・夕方・夜で変わる。ゆっくり解説などの背景に
// =====================================================================
public sealed class RoomBackgroundPlugin : IShapePlugin
{
    public string Name => "演出：部屋の背景";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RoomBackgroundParameter(sharedData);
}

public sealed class RoomBackgroundParameter : EffectShapeParameterBase
{
    public RoomBackgroundParameter() : this(null) { }
    public RoomBackgroundParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "部屋", Name = "部屋の種類")]
    [EnumComboBox]
    public RoomKind Room { get => room; set => Set(ref room, value); }
    RoomKind room = RoomKind.Classroom;

    [Display(GroupName = "部屋", Name = "時間帯")]
    [EnumComboBox]
    public TimeOfDay When { get => when; set => Set(ref when, value); }
    TimeOfDay when = TimeOfDay.Day;

    [Display(GroupName = "部屋", Name = "壁の色")]
    [ColorPicker]
    public WpfColor WallColor { get => wallColor; set => Set(ref wallColor, value); }
    WpfColor wallColor = WpfColor.FromRgb(0xF3, 0xEC, 0xDD);

    [Display(GroupName = "部屋", Name = "床の色")]
    [ColorPicker]
    public WpfColor FloorColor { get => floorColor; set => Set(ref floorColor, value); }
    WpfColor floorColor = WpfColor.FromRgb(0xC8, 0x9B, 0x6A);

    [Display(GroupName = "部屋", Name = "黒板・モニターの文字", Description = "教室の黒板、オフィスのモニターなどに書く文字")]
    [TextEditor(AcceptsReturn = true)]
    public string BoardText { get => boardText; set => Set(ref boardText, value); }
    string boardText = "今日のテーマ";

    [Display(GroupName = "部屋", Name = "ぼかす（人物を目立たせる）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Dim { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Dim];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float u = MathF.Min(w / 1920f, h / 1080f);
        float floorY = h * 0.72f;
        using var brush = dc.CreateSolidColorBrush(default);
        var night = When == TimeOfDay.Night;
        var light = When switch { TimeOfDay.Evening => WpfColor.FromRgb(0xFF, 0xB0, 0x70), TimeOfDay.Night => WpfColor.FromRgb(0x60, 0x70, 0xA8), _ => WpfColor.FromRgb(255, 255, 255) };
        float tintA = When == TimeOfDay.Day ? 0f : 0.22f;

        // 壁と床
        brush.Color = Gfx.C4(WallColor, 1f);
        dc.FillRectangle(new RawRectF(0, 0, w, floorY), brush);
        brush.Color = Gfx.C4(Gfx.Mix(WallColor, WpfColor.FromRgb(0, 0, 0), 0.12f), 1f);
        dc.FillRectangle(new RawRectF(0, floorY - 26f * u, w, floorY), brush);    // 幅木
        using (var g = Gfx.Linear(dc, new Vector2(0, floorY), new Vector2(0, h), (0f, Gfx.C4(FloorColor, 1f)), (1f, Gfx.C4(Gfx.Mix(FloorColor, WpfColor.FromRgb(0, 0, 0), 0.25f), 1f))))
            dc.FillRectangle(new RawRectF(0, floorY, w, h), g);
        // 床板の線(手前ほど広い)
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.12f);
        for (int i = -8; i <= 8; i++)
        {
            float x = w / 2f + i * w * 0.06f;
            dc.DrawLine(new Vector2(x, floorY), new Vector2(w / 2f + i * w * 0.16f, h), brush, 2f * u);
        }

        // 窓(外の空)
        void Window(float x, float y, float ww, float wh)
        {
            var top = When switch { TimeOfDay.Evening => WpfColor.FromRgb(0xFF, 0x8A, 0x5B), TimeOfDay.Night => WpfColor.FromRgb(0x0B, 0x14, 0x38), _ => WpfColor.FromRgb(0x6C, 0xB8, 0xF5) };
            var bottom = When switch { TimeOfDay.Evening => WpfColor.FromRgb(0xFF, 0xD2, 0x8C), TimeOfDay.Night => WpfColor.FromRgb(0x25, 0x2E, 0x5E), _ => WpfColor.FromRgb(0xC8, 0xE8, 0xFF) };
            using (var g = Gfx.Linear(dc, new Vector2(0, y), new Vector2(0, y + wh), (0f, Gfx.C4(top, 1f)), (1f, Gfx.C4(bottom, 1f))))
                dc.FillRectangle(new RawRectF(x, y, x + ww, y + wh), g);
            if (night)
            {
                brush.Color = Gfx.C4(1f, 1f, 0.9f, 1f);
                for (int i = 0; i < 14; i++)
                {
                    float sx = x + DrawKit.H(7, i) * ww, sy = y + DrawKit.H(7, i, 1) * wh * 0.7f;
                    float tw = 0.5f + 0.5f * MathF.Sin(t * 2f + i);
                    dc.FillEllipse(new Ellipse(new Vector2(sx, sy), 2f * u * tw + 0.5f, 2f * u * tw + 0.5f), brush);
                }
                dc.FillEllipse(new Ellipse(new Vector2(x + ww * 0.75f, y + wh * 0.25f), 26f * u, 26f * u), brush);
            }
            else
            {
                // 流れる雲
                brush.Color = Gfx.C4(1f, 1f, 1f, When == TimeOfDay.Evening ? 0.6f : 0.85f);
                for (int i = 0; i < 2; i++)
                {
                    float cx = x + Gfx.Mod(i * ww * 0.55f + t * 12f * u, ww + 200f * u) - 100f * u;
                    var cloud = BubbleKit.CloudPts(new Vector2(cx, y + wh * (0.3f + i * 0.25f)), 70f * u, 22f * u, 6, 0.25f);
                    Gfx.FillPoly(dc, brush, DrawKit.ClipRect(cloud, x, y, x + ww, y + wh));
                }
            }
            // 窓枠
            brush.Color = Gfx.C4(0.96f, 0.96f, 0.96f, 1f);
            dc.DrawRectangle(new RawRectF(x, y, x + ww, y + wh), brush, 14f * u);
            dc.DrawLine(new Vector2(x + ww / 2f, y), new Vector2(x + ww / 2f, y + wh), brush, 10f * u);
            dc.DrawLine(new Vector2(x, y + wh / 2f), new Vector2(x + ww, y + wh / 2f), brush, 8f * u);
            // 窓から差し込む光
            if (!night)
            {
                var beam = new List<Vector2> { new(x, y + wh), new(x + ww, y + wh), new(x + ww + ww * 0.6f, floorY + (h - floorY) * 0.8f), new(x + ww * 0.6f, floorY + (h - floorY) * 0.8f) };
                brush.Color = Gfx.C4(light, 0.12f);
                Gfx.FillPoly(dc, brush, beam);
            }
        }

        void Rect(float x0, float y0, float x1, float y1, WpfColor col)
        {
            brush.Color = Gfx.C4(col, 1f);
            dc.FillRectangle(new RawRectF(x0, y0, x1, y1), brush);
        }
        void Plant(float x, float y, float s)
        {
            Rect(x - 40f * s, y - 70f * s, x + 40f * s, y, WpfColor.FromRgb(0xB5, 0x6A, 0x3C));
            brush.Color = Gfx.C4(WpfColor.FromRgb(0x3E, 0x9B, 0x4F), 1f);
            for (int i = 0; i < 7; i++)
            {
                float a = -MathF.PI / 2f + (i - 3) * 0.32f;
                var tip = new Vector2(x, y - 70f * s) + new Vector2(MathF.Cos(a), MathF.Sin(a)) * 150f * s * (0.7f + 0.3f * MathF.Sin(i * 1.7f));
                dc.FillEllipse(new Ellipse(Vector2.Lerp(new Vector2(x, y - 70f * s), tip, 0.6f), 30f * s, 14f * s), brush);
            }
        }

        string board = TextKit.Lines(BoardText).Trim();
        switch (Room)
        {
            case RoomKind.Classroom:
            {
                Window(w * 0.05f, h * 0.12f, w * 0.18f, h * 0.38f);
                // 黒板
                float bx0 = w * 0.3f, bx1 = w * 0.92f, by0 = h * 0.1f, by1 = h * 0.5f;
                Rect(bx0 - 16f * u, by0 - 16f * u, bx1 + 16f * u, by1 + 16f * u, WpfColor.FromRgb(0x8A, 0x5A, 0x34));
                Rect(bx0, by0, bx1, by1, WpfColor.FromRgb(0x2F, 0x5D, 0x44));
                Rect(bx0, by1 + 16f * u, bx1, by1 + 30f * u, WpfColor.FromRgb(0x7A, 0x4E, 0x2C));
                if (board.Length > 0)
                {
                    var s = TextKit.Get(board, 70f * u, true, null, (bx1 - bx0) * 0.85f, 1);
                    TextKit.DrawCentered(dc, brush, s, new Vector2((bx0 + bx1) / 2f, (by0 + by1) / 2f), Gfx.C4(0.95f, 0.95f, 0.9f, 0.92f));
                }
                // 机(手前)
                for (int i = 0; i < 3; i++)
                {
                    float dx = w * (0.18f + i * 0.32f), dy = h * 0.86f;
                    Rect(dx - 150f * u, dy - 20f * u, dx + 150f * u, dy, WpfColor.FromRgb(0xD9, 0xB0, 0x7A));
                    Rect(dx - 140f * u, dy, dx - 125f * u, h, WpfColor.FromRgb(0x77, 0x7A, 0x80));
                    Rect(dx + 125f * u, dy, dx + 140f * u, h, WpfColor.FromRgb(0x77, 0x7A, 0x80));
                }
                // 時計
                var clk = new Vector2(w * 0.2f, h * 0.06f + 30f * u);
                brush.Color = Gfx.C4(1f, 1f, 1f, 1f);
                dc.FillEllipse(new Ellipse(clk, 40f * u, 40f * u), brush);
                brush.Color = Gfx.C4(0.2f, 0.2f, 0.22f, 1f);
                dc.DrawEllipse(new Ellipse(clk, 40f * u, 40f * u), brush, 5f * u);
                dc.DrawLine(clk, clk + new Vector2(MathF.Cos(t * 0.1f - 1.5f), MathF.Sin(t * 0.1f - 1.5f)) * 28f * u, brush, 4f * u);
                dc.DrawLine(clk, clk + new Vector2(0f, -20f * u), brush, 5f * u);
                break;
            }
            case RoomKind.Office:
            {
                Window(w * 0.55f, h * 0.08f, w * 0.38f, h * 0.45f);
                // 机とモニター
                Rect(w * 0.08f, h * 0.62f, w * 0.48f, h * 0.66f, WpfColor.FromRgb(0xE8, 0xE8, 0xEC));
                Rect(w * 0.1f, h * 0.66f, w * 0.11f, floorY + 40f * u, WpfColor.FromRgb(0x90, 0x94, 0x9C));
                Rect(w * 0.45f, h * 0.66f, w * 0.46f, floorY + 40f * u, WpfColor.FromRgb(0x90, 0x94, 0x9C));
                Rect(w * 0.16f, h * 0.36f, w * 0.4f, h * 0.56f, WpfColor.FromRgb(0x22, 0x24, 0x2A));
                Rect(w * 0.17f, h * 0.375f, w * 0.39f, h * 0.545f, WpfColor.FromRgb(0x2A, 0x5C, 0xA8));
                Rect(w * 0.27f, h * 0.56f, w * 0.29f, h * 0.62f, WpfColor.FromRgb(0x50, 0x52, 0x58));
                if (board.Length > 0)
                {
                    var s = TextKit.Get(board, 40f * u, true, null, w * 0.2f, 1);
                    TextKit.DrawCentered(dc, brush, s, new Vector2(w * 0.28f, h * 0.46f), Gfx.C4(1f, 1f, 1f, 0.95f));
                }
                Plant(w * 0.92f, floorY + 60f * u, u * 1.4f);
                // 棚
                Rect(w * 0.02f, h * 0.12f, w * 0.12f, h * 0.14f, WpfColor.FromRgb(0x9A, 0x6B, 0x45));
                for (int i = 0; i < 5; i++) Rect(w * 0.025f + i * 32f * u, h * 0.12f - (60f + i % 2 * 14f) * u, w * 0.025f + i * 32f * u + 26f * u, h * 0.12f, BubbleKit.Palette(1, i));
                break;
            }
            case RoomKind.Bedroom:
            {
                Window(w * 0.6f, h * 0.1f, w * 0.26f, h * 0.36f);
                // ベッド
                Rect(w * 0.05f, h * 0.5f, w * 0.1f, h * 0.84f, WpfColor.FromRgb(0x9A, 0x6B, 0x45));
                Rect(w * 0.08f, h * 0.66f, w * 0.52f, h * 0.84f, WpfColor.FromRgb(0xF4, 0xF4, 0xF8));
                Rect(w * 0.2f, h * 0.64f, w * 0.52f, h * 0.84f, WpfColor.FromRgb(0x8E, 0xA9, 0xE8));
                brush.Color = Gfx.C4(1f, 1f, 1f, 1f);
                dc.FillEllipse(new Ellipse(new Vector2(w * 0.14f, h * 0.645f), 70f * u, 30f * u), brush);
                // ランプ
                var lamp = new Vector2(w * 0.6f, h * 0.6f);
                Rect(lamp.X - 60f * u, lamp.Y + 40f * u, lamp.X + 60f * u, floorY + 50f * u, WpfColor.FromRgb(0x9A, 0x6B, 0x45));
                Rect(lamp.X - 5f * u, lamp.Y - 20f * u, lamp.X + 5f * u, lamp.Y + 40f * u, WpfColor.FromRgb(0x60, 0x60, 0x66));
                brush.Color = Gfx.C4(WpfColor.FromRgb(0xFF, 0xE0, 0x9A), 1f);
                Gfx.FillPoly(dc, brush, new List<Vector2> { lamp + new Vector2(-50f * u, -20f * u), lamp + new Vector2(50f * u, -20f * u), lamp + new Vector2(30f * u, -90f * u), lamp + new Vector2(-30f * u, -90f * u) });
                if (When != TimeOfDay.Day) DrawKit.Glow(dc, lamp + new Vector2(0f, -50f * u), 260f * u, Gfx.C4(1f, 0.85f, 0.5f, 0.35f));
                break;
            }
            default:
            {
                Window(w * 0.07f, h * 0.1f, w * 0.3f, h * 0.42f);
                // テレビとソファ
                Rect(w * 0.55f, h * 0.18f, w * 0.9f, h * 0.48f, WpfColor.FromRgb(0x1A, 0x1B, 0x20));
                Rect(w * 0.56f, h * 0.195f, w * 0.89f, h * 0.465f, night ? WpfColor.FromRgb(0x30, 0x48, 0x80) : WpfColor.FromRgb(0x3A, 0x3D, 0x46));
                if (board.Length > 0)
                {
                    var s = TextKit.Get(board, 44f * u, true, null, w * 0.3f, 1);
                    TextKit.DrawCentered(dc, brush, s, new Vector2(w * 0.725f, h * 0.33f), Gfx.C4(1f, 1f, 1f, 0.9f));
                }
                Rect(w * 0.6f, h * 0.48f, w * 0.85f, h * 0.62f, WpfColor.FromRgb(0x9A, 0x6B, 0x45));
                var sofaCol = WpfColor.FromRgb(0xD4, 0x6A, 0x5A);
                Rect(w * 0.08f, h * 0.7f, w * 0.48f, h * 0.84f, sofaCol);
                Rect(w * 0.08f, h * 0.6f, w * 0.48f, h * 0.7f, Gfx.Mix(sofaCol, WpfColor.FromRgb(0, 0, 0), 0.12f));
                Rect(w * 0.06f, h * 0.64f, w * 0.1f, h * 0.84f, Gfx.Mix(sofaCol, WpfColor.FromRgb(0, 0, 0), 0.2f));
                Rect(w * 0.46f, h * 0.64f, w * 0.5f, h * 0.84f, Gfx.Mix(sofaCol, WpfColor.FromRgb(0, 0, 0), 0.2f));
                Plant(w * 0.95f, floorY + 40f * u, u * 1.2f);
                break;
            }
        }
        // 時間帯の色(夕方は橙、夜は青く暗く)
        if (tintA > 0f)
        {
            brush.Color = Gfx.C4(light, tintA);
            dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            if (night)
            {
                brush.Color = Gfx.C4(0.02f, 0.03f, 0.1f, 0.3f);
                dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
            }
        }
        float dim = c.F(Dim) / 100f;
        if (dim > 0.002f)
        {
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.5f * dim);
            dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
        }
    }
}
