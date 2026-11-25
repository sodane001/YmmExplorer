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

public enum FrameKind
{
    [Display(Name = "ブラウン管テレビ")] Crt,
    [Display(Name = "ポラロイド写真")] Polaroid,
    [Display(Name = "フィルムのコマ")] Film,
    [Display(Name = "ゲーム機")] Handheld,
}

// =====================================================================
//  画面フレーム
//   ブラウン管テレビ、ポラロイド、フィルムのコマ、ゲーム機の画面などの額縁。
//   中央の窓は透明なので、下のレイヤーの映像がのぞきます
// =====================================================================
public sealed class ScreenFramePlugin : IShapePlugin
{
    public string Name => "演出：画面フレーム";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ScreenFrameParameter(sharedData);
}

public sealed class ScreenFrameParameter : EffectShapeParameterBase
{
    public ScreenFrameParameter() : this(null) { }
    public ScreenFrameParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public FrameKind Kind { get => kind; set => Set(ref kind, value); }
    FrameKind kind = FrameKind.Crt;

    [Display(GroupName = "窓", Name = "窓の大きさ（画面に対して）")]
    [AnimationSlider("F0", "%", 20, 95)]
    public Animation WindowScale { get; } = new Animation(70, 5, 100);

    [Display(GroupName = "窓", Name = "窓のX位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "窓", Name = "窓のY位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(47, -100, 200);

    [Display(GroupName = "色", Name = "本体・ふちの色")]
    [ColorPicker]
    public WpfColor BodyColor { get => bodyColor; set => Set(ref bodyColor, value); }
    WpfColor bodyColor = WpfColor.FromRgb(0x3A, 0x34, 0x2E);

    [Display(GroupName = "色", Name = "種類ごとのおすすめの色を使う")]
    [ToggleSlider]
    public bool AutoColor { get => autoColor; set => Set(ref autoColor, value); }
    bool autoColor = true;

    [Display(GroupName = "効果", Name = "走査線（テレビ）・ノイズ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Scanlines { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "効果", Name = "窓のふちを暗くする")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Vignette { get; } = new Animation(45, 0, 100);

    [Display(GroupName = "効果", Name = "フィルムが流れる速さ")]
    [AnimationSlider("F0", "px/秒", -400, 400)]
    public Animation FilmSpeed { get; } = new Animation(60, -5000, 5000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [WindowScale, PosX, PosY, Scanlines, Vignette, FilmSpeed];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float sc = c.F(WindowScale) / 100f;
        float ww = w * sc, wh = h * sc;
        var wc = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        var win = new RawRectF(wc.X - ww / 2f, wc.Y - wh / 2f, wc.X + ww / 2f, wc.Y + wh / 2f);
        float scan = c.F(Scanlines) / 100f, vig = c.F(Vignette) / 100f;
        var body = AutoColor ? Kind switch
        {
            FrameKind.Polaroid => WpfColor.FromRgb(0xF6, 0xF3, 0xEC),
            FrameKind.Film => WpfColor.FromRgb(0x12, 0x10, 0x0E),
            FrameKind.Handheld => WpfColor.FromRgb(0x5A, 0x5C, 0xD8),
            _ => WpfColor.FromRgb(0x3A, 0x34, 0x2E),
        } : BodyColor;

        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            switch (Kind)
            {
                case FrameKind.Crt: DrawCrt(dc, brush, w, h, win, body, t); break;
                case FrameKind.Polaroid: DrawPolaroid(dc, brush, win, body); break;
                case FrameKind.Film: DrawFilm(dc, brush, w, win, body, t * c.F(FilmSpeed)); break;
                default: DrawHandheld(dc, brush, win, body, t); break;
            }

            // ---- 窓の中の効果 ----
            if (vig > 0f)
            {
                var mid = new Vector2((win.Left + win.Right) / 2f, (win.Top + win.Bottom) / 2f);
                float rr = MathF.Sqrt(ww * ww + wh * wh) / 2f;
                using var g = Gfx.Radial(dc, mid, rr, (0f, Gfx.C4(0f, 0f, 0f, 0f)), (0.6f, Gfx.C4(0f, 0f, 0f, 0.1f * vig)), (1f, Gfx.C4(0f, 0f, 0f, 0.85f * vig)));
                dc.FillRectangle(win, g);
            }
            if (scan > 0f && (Kind == FrameKind.Crt || Kind == FrameKind.Handheld))
            {
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f * scan);
                float step = Kind == FrameKind.Handheld ? 3f : 4f;
                for (float y = win.Top; y < win.Bottom; y += step)
                    dc.FillRectangle(new RawRectF(win.Left, y, win.Right, y + step * 0.45f), brush);
                // ゆっくり流れる明るい帯
                float by = win.Top + DrawKit.Frac(t * 0.15f) * (wh + 120f) - 60f;
                using var lg = Gfx.Linear(dc, new Vector2(0, by - 60f), new Vector2(0, by + 60f), (0f, Gfx.C4(1f, 1f, 1f, 0f)), (0.5f, Gfx.C4(1f, 1f, 1f, 0.06f * scan)), (1f, Gfx.C4(1f, 1f, 1f, 0f)));
                dc.FillRectangle(new RawRectF(win.Left, MathF.Max(win.Top, by - 60f), win.Right, MathF.Min(win.Bottom, by + 60f)), lg);
            }
            if (scan > 0f && Kind == FrameKind.Film)
            {
                // 傷とほこり
                var rng = new Random(Seed + (int)(t * 12f));
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.25f * scan);
                for (int i = 0; i < 3; i++)
                {
                    float x = win.Left + rng.NextF() * ww;
                    dc.DrawLine(new Vector2(x, win.Top), new Vector2(x + (rng.NextF() - 0.5f) * 6f, win.Bottom), brush, 1f);
                }
                for (int i = 0; i < 8; i++)
                    dc.FillEllipse(new Ellipse(new Vector2(win.Left + rng.NextF() * ww, win.Top + rng.NextF() * wh), 1.5f, 1.5f), brush);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    static List<Vector2> Rect(RawRectF r) => DrawKit.RectPts(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);

    static RawRectF Grow(RawRectF r, float a, float b, float cc, float d) => new(r.Left - a, r.Top - b, r.Right + cc, r.Bottom + d);

    void DrawCrt(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, float w, float h, RawRectF win, WpfColor body, float t)
    {
        float ww = win.Right - win.Left, wh = win.Bottom - win.Top;
        float rad = MathF.Min(ww, wh) * 0.08f;
        var hole = ShapeGeom.RoundRect((win.Left + win.Right) / 2f, (win.Top + win.Bottom) / 2f, ww, wh, rad);
        // 本体（画面いっぱい）
        using (var lg = Gfx.Linear(dc, new Vector2(0, 0), new Vector2(0, h), (0f, Gfx.C4(Gfx.Mix(body, WpfColor.FromRgb(255, 255, 255), 0.12f), 1f)), (1f, Gfx.C4(Gfx.Mix(body, WpfColor.FromRgb(0, 0, 0), 0.3f), 1f))))
            DrawKit.FillWithHole(dc, lg, DrawKit.RectPts(-2f, -2f, w + 4f, h + 4f), hole);
        // 画面のまわりの黒いふち
        float bz = MathF.Min(ww, wh) * 0.05f;
        var bezel = ShapeGeom.RoundRect((win.Left + win.Right) / 2f, (win.Top + win.Bottom) / 2f, ww + bz * 2f, wh + bz * 2f, rad + bz);
        brush.Color = Gfx.C4(0.07f, 0.07f, 0.08f, 1f);
        DrawKit.FillWithHole(dc, brush, bezel, hole);
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.5f);
        Gfx.Polyline(dc, brush, ShapeGeom.Closed(hole), 3f);
        // ガラスの反射
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.06f);
        Gfx.FillPoly(dc, brush, new List<Vector2> { new(win.Left + ww * 0.05f, win.Top + wh * 0.04f), new(win.Left + ww * 0.45f, win.Top + wh * 0.04f), new(win.Left + ww * 0.05f, win.Top + wh * 0.4f) });
        // 右側のつまみとスピーカー（すき間があれば）
        float room = w - (win.Right + bz);
        if (room > 90f)
        {
            float kx = win.Right + bz + room / 2f;
            float kr = MathF.Min(room * 0.28f, wh * 0.08f);
            for (int i = 0; i < 2; i++)
            {
                var kp = new Vector2(kx, win.Top + wh * (0.2f + 0.22f * i));
                using (var g = Gfx.Radial(dc, kp - new Vector2(kr * 0.3f, kr * 0.3f), kr * 1.4f, (0f, Gfx.C4(0.55f, 0.55f, 0.55f, 1f)), (1f, Gfx.C4(0.12f, 0.12f, 0.12f, 1f))))
                    dc.FillEllipse(new Ellipse(kp, kr, kr), g);
                brush.Color = Gfx.C4(0.05f, 0.05f, 0.05f, 1f);
                float a = i * 1.2f + 0.5f;
                dc.DrawLine(kp, kp + new Vector2(MathF.Cos(a), MathF.Sin(a)) * kr * 0.8f, brush, MathF.Max(1.5f, kr * 0.12f));
            }
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f);
            for (int i = 0; i < 9; i++)
            {
                float y = win.Top + wh * (0.62f + i * 0.035f);
                dc.DrawLine(new Vector2(kx - room * 0.3f, y), new Vector2(kx + room * 0.3f, y), brush, 2f);
            }
            brush.Color = Gfx.C4(1f, 0.2f, 0.1f, 0.7f + 0.3f * MathF.Sin(t * 2f));
            dc.FillEllipse(new Ellipse(new Vector2(kx, win.Top + wh * 0.5f), 3.5f, 3.5f), brush);
        }
    }

    static void DrawPolaroid(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, RawRectF win, WpfColor body)
    {
        float ww = win.Right - win.Left, wh = win.Bottom - win.Top;
        float m = MathF.Min(ww, wh) * 0.06f;
        var card = Grow(win, m, m, m, m * 3.6f);
        for (int j = 0; j < 4; j++)
        {
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.1f);
            dc.FillRectangle(new RawRectF(card.Left + 4f + j * 2f, card.Top + 6f + j * 3f, card.Right + 4f + j * 2f, card.Bottom + 6f + j * 3f), brush);
        }
        brush.Color = Gfx.C4(body, 1f);
        DrawKit.FillWithHole(dc, brush, Rect(card), Rect(win));
        // 紙の質感（下のほうがほんの少し暗い）
        using (var lg = Gfx.Linear(dc, new Vector2(0, card.Top), new Vector2(0, card.Bottom), (0f, Gfx.C4(0f, 0f, 0f, 0f)), (1f, Gfx.C4(0f, 0f, 0f, 0.06f))))
            DrawKit.FillWithHole(dc, lg, Rect(card), Rect(win));
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f);
        dc.DrawRectangle(win, brush, 1.5f);
    }

    static void DrawFilm(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, float w, RawRectF win, WpfColor body, float scroll)
    {
        float ww = win.Right - win.Left, wh = win.Bottom - win.Top;
        float band = wh * 0.16f;
        var strip = new RawRectF(-2f, win.Top - band, w + 2f, win.Bottom + band);
        brush.Color = Gfx.C4(body, 1f);
        DrawKit.FillWithHole(dc, brush, Rect(strip), Rect(win));
        // となりのコマ（ちらっと見える）
        float gap = band * 0.4f;
        brush.Color = Gfx.C4(Gfx.Mix(body, WpfColor.FromRgb(70, 60, 50), 0.6f), 1f);
        for (int s = -1; s <= 1; s += 2)
        {
            float x0 = s < 0 ? win.Left - gap - ww : win.Right + gap;
            dc.FillRectangle(new RawRectF(x0, win.Top, x0 + ww, win.Bottom), brush);
        }
        // 送り穴
        float hw = band * 0.42f, hh = band * 0.32f, step = band * 0.9f;
        float off = Gfx.Mod(scroll, step);
        brush.Color = Gfx.C4(0.92f, 0.9f, 0.85f, 1f);
        for (float x = -step + off; x < w + step; x += step)
        {
            foreach (float y in new[] { strip.Top + band * 0.34f, strip.Bottom - band * 0.34f - hh })
            {
                var r = ShapeGeom.RoundRect(x + hw / 2f, y + hh / 2f, hw, hh, hh * 0.25f);
                Gfx.FillPoly(dc, brush, r);
            }
        }
        // コマ番号
        brush.Color = Gfx.C4(1f, 0.6f, 0.15f, 0.9f);
        int frameNo = 12 + (int)MathF.Floor(scroll / MathF.Max(1f, ww + gap));
        string label = frameNo.ToString(CultureInfo.InvariantCulture) + "A";
        StrokeFont.Draw(dc, brush, label, new Vector2(win.Left + band * 0.2f, strip.Bottom - band * 0.28f), band * 0.18f, 1.5f);
        StrokeFont.Draw(dc, brush, "400", new Vector2(win.Right - band * 1.2f, strip.Top + band * 0.08f), band * 0.16f, 1.3f);
    }

    static void DrawHandheld(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, RawRectF win, WpfColor body, float t)
    {
        float ww = win.Right - win.Left, wh = win.Bottom - win.Top;
        float side = wh * 0.62f, topM = wh * 0.12f, botM = wh * 0.16f;
        var outer = Grow(win, side, topM, side, botM);
        float cx = (outer.Left + outer.Right) / 2f, cy = (outer.Top + outer.Bottom) / 2f;
        var shape = ShapeGeom.RoundRect(cx, cy, outer.Right - outer.Left, outer.Bottom - outer.Top, wh * 0.28f);
        var bezelR = Grow(win, wh * 0.05f, wh * 0.05f, wh * 0.05f, wh * 0.05f);
        var bezel = ShapeGeom.RoundRect((bezelR.Left + bezelR.Right) / 2f, (bezelR.Top + bezelR.Bottom) / 2f, bezelR.Right - bezelR.Left, bezelR.Bottom - bezelR.Top, wh * 0.04f);
        // 影
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f);
        Gfx.FillPoly(dc, brush, shape.Select(p => p + new Vector2(8f, 12f)).ToList());
        using (var lg = Gfx.Linear(dc, new Vector2(0, outer.Top), new Vector2(0, outer.Bottom),
            (0f, Gfx.C4(Gfx.Mix(body, WpfColor.FromRgb(255, 255, 255), 0.18f), 1f)), (1f, Gfx.C4(Gfx.Mix(body, WpfColor.FromRgb(0, 0, 0), 0.25f), 1f))))
            DrawKit.FillWithHole(dc, lg, shape, bezel);
        brush.Color = Gfx.C4(0.08f, 0.08f, 0.1f, 1f);
        DrawKit.FillWithHole(dc, brush, bezel, Rect(win));
        // 十字キー
        var dp = new Vector2(outer.Left + side * 0.5f, cy + wh * 0.05f);
        float a = side * 0.17f, b = side * 0.055f;
        brush.Color = Gfx.C4(0.12f, 0.12f, 0.14f, 1f);
        dc.FillRectangle(new RawRectF(dp.X - a, dp.Y - b, dp.X + a, dp.Y + b), brush);
        dc.FillRectangle(new RawRectF(dp.X - b, dp.Y - a, dp.X + b, dp.Y + a), brush);
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.12f);
        dc.FillRectangle(new RawRectF(dp.X - a, dp.Y - b, dp.X + a, dp.Y - b * 0.4f), brush);
        // A・B ボタン
        var bp = new Vector2(outer.Right - side * 0.5f, cy + wh * 0.05f);
        float br = side * 0.075f;
        var press = (int)(t * 2f) % 2;
        for (int i = 0; i < 2; i++)
        {
            var p = bp + new Vector2(i == 0 ? br * 1.3f : -br * 1.3f, i == 0 ? -br * 0.8f : br * 0.8f);
            float pr = br * (press == i ? 0.92f : 1f);
            using var g = Gfx.Radial(dc, p - new Vector2(pr * 0.3f, pr * 0.3f), pr * 1.4f, (0f, Gfx.C4(1f, 0.45f, 0.5f, 1f)), (1f, Gfx.C4(0.55f, 0.08f, 0.15f, 1f)));
            dc.FillEllipse(new Ellipse(p, pr, pr), g);
            brush.Color = Gfx.C4(1f, 1f, 1f, 0.8f);
            StrokeFont.DrawCentered(dc, brush, i == 0 ? "A" : "B", p + new Vector2(0, pr * 1.75f), pr * 0.6f, 1.6f);
        }
        // START・SELECT
        brush.Color = Gfx.C4(0.1f, 0.1f, 0.12f, 1f);
        for (int i = 0; i < 2; i++)
        {
            var p = new Vector2(i == 0 ? dp.X : bp.X, win.Bottom + botM * 0.55f);
            var pill = ShapeGeom.RoundRect(p.X, p.Y, side * 0.2f, side * 0.06f, side * 0.03f);
            Gfx.FillPoly(dc, brush, pill);
        }
        brush.Color = Gfx.C4(1f, 1f, 1f, 0.55f);
        StrokeFont.DrawCentered(dc, brush, "SELECT", new Vector2(dp.X, win.Bottom + botM * 0.85f), side * 0.045f, 1.2f);
        StrokeFont.DrawCentered(dc, brush, "START", new Vector2(bp.X, win.Bottom + botM * 0.85f), side * 0.045f, 1.2f);
        // 電源ランプ
        brush.Color = Gfx.C4(1f, 0.25f, 0.2f, 0.7f + 0.3f * MathF.Sin(t * 2f));
        dc.FillEllipse(new Ellipse(new Vector2(outer.Left + side * 0.18f, outer.Top + topM * 0.9f), 4f, 4f), brush);
        // スピーカーの穴
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.35f);
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 4; j++)
                dc.FillEllipse(new Ellipse(new Vector2(bp.X - side * 0.1f + j * side * 0.07f, outer.Bottom - botM * 0.4f - wh * 0.08f - i * side * 0.07f), 2.5f, 2.5f), brush);
    }
}
