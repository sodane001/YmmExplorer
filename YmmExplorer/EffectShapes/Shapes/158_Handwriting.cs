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

public enum HandwritingMode
{
    [Display(Name = "ペンで左から書いていく")] Wipe,
    [Display(Name = "文字のふちをなぞってから塗る")] Trace,
}

// =====================================================================
//  手書き文字(ホワイトボード・ふせん)
//   文字を、ペンで書くように少しずつ出す。ペン先も一緒に動く
// =====================================================================
public sealed class HandwritingPlugin : IShapePlugin
{
    public string Name => "演出：手書き文字";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HandwritingParameter(sharedData);
}

public sealed class HandwritingParameter : EffectShapeParameterBase
{
    public HandwritingParameter() : this(null) { }
    public HandwritingParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "文字")]
    [TextEditor(AcceptsReturn = true)]
    public string Text { get => text; set => Set(ref text, value); }
    string text = "ここが大事！";

    [Display(GroupName = "文字", Name = "フォント", Description = "手書き風のフォント(UD デジタル 教科書体 など)がおすすめです")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "UD デジタル 教科書体 N-B";

    [Display(GroupName = "文字", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 20, 400)]
    public Animation FontSize { get; } = new Animation(120, 4, 3000);

    [Display(GroupName = "文字", Name = "ペンの色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x1D, 0x4E, 0xD8);

    [Display(GroupName = "文字", Name = "書き方")]
    [EnumComboBox]
    public HandwritingMode Mode { get => mode; set => Set(ref mode, value); }
    HandwritingMode mode = HandwritingMode.Wipe;

    [Display(GroupName = "文字", Name = "ペン先を出す")]
    [ToggleSlider]
    public bool ShowPen { get => showPen; set => Set(ref showPen, value); }
    bool showPen = true;

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "傾き")]
    [AnimationSlider("F0", "°", -30, 30)]
    public Animation Tilt { get; } = new Animation(-2, -360, 360);

    [Display(GroupName = "動き", Name = "書く時間")]
    [AnimationSlider("F2", "秒", 0.2, 6)]
    public Animation WriteTime { get; } = new Animation(1.6, 0.05, 600);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [FontSize, PosX, PosY, Tilt, WriteTime];

    /// <summary>ペン(マーカー)の形。先が tip</summary>
    static void Pen(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 tip, float s, WpfColor ink, float a)
    {
        var dir = Vector2.Normalize(new Vector2(0.55f, -1f));
        var n = new Vector2(-dir.Y, dir.X);
        var neck = tip + dir * s * 0.35f;
        var end = tip + dir * s * 2.4f;
        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f * a);
        Gfx.FillPoly(dc, brush, new List<Vector2> { neck + n * s * 0.2f + new Vector2(6, 8), end + n * s * 0.2f + new Vector2(6, 8), end - n * s * 0.2f + new Vector2(6, 8), neck - n * s * 0.2f + new Vector2(6, 8) });
        brush.Color = Gfx.C4(ink, a);
        Gfx.FillPoly(dc, brush, new List<Vector2> { tip, neck + n * s * 0.12f, neck - n * s * 0.12f });
        brush.Color = Gfx.C4(0.95f, 0.95f, 0.97f, a);
        Gfx.FillPoly(dc, brush, new List<Vector2> { neck + n * s * 0.2f, end + n * s * 0.2f, end - n * s * 0.2f, neck - n * s * 0.2f });
        brush.Color = Gfx.C4(ink, a);
        Gfx.FillPoly(dc, brush, new List<Vector2> { end - dir * s * 0.5f + n * s * 0.21f, end + n * s * 0.21f, end - n * s * 0.21f, end - dir * s * 0.5f - n * s * 0.21f });
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float outK = DrawKit.InOut(c, 0f, 0.25f);
        if (outK <= 0.001f) return;
        float fs = c.F(FontSize), wt = MathF.Max(0.05f, c.F(WriteTime));
        float p = Math.Clamp(t / wt, 0f, 1f);
        var s = TextKit.Get(TextKit.Lines(Text), fs, true, Font, 0f, 1);
        if (s.Contours.Count == 0) s = TextKit.Get(TextKit.Lines(Text), fs, true, null, 0f, 1);
        if (s.Width <= 0f) return;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float rot = Gfx.Rad(c.F(Tilt));
        var tl = ctr - new Vector2(s.Width, s.Height) / 2f;
        using var brush = dc.CreateSolidColorBrush(default);
        var ink = Gfx.C4(Color, outK);
        var prev = dc.Transform;
        dc.Transform = Matrix3x2.CreateRotation(rot, ctr) * prev;
        Vector2 penAt;

        if (Mode == HandwritingMode.Trace)
        {
            // 輪郭を順番になぞる → 最後に中を塗る
            var contours = s.Contours.OrderBy(cn => cn.Min(q => q.X)).ToList();
            float total = contours.Sum(cn => BubbleKit.Length(cn) + Vector2.Distance(cn[^1], cn[0]));
            float want = MathF.Min(1f, p / 0.8f) * total, acc = 0f;
            penAt = tl;
            brush.Color = ink;
            foreach (var cn in contours)
            {
                var loop = cn.Select(q => tl + q).ToList();
                loop.Add(loop[0]);
                float len = BubbleKit.Length(loop);
                if (acc >= want) break;
                float f = Math.Clamp((want - acc) / MathF.Max(1e-3f, len), 0f, 1f);
                var part = BubbleKit.PartByLength(loop, f);
                Gfx.Polyline(dc, brush, part, MathF.Max(1.5f, fs * 0.025f));
                penAt = part[^1];
                acc += len;
            }
            float fillA = Math.Clamp((p - 0.75f) / 0.25f, 0f, 1f);
            if (fillA > 0f)
            {
                brush.Color = Gfx.C4(Color, outK * fillA);
                TextKit.Fill(dc, brush, s, tl);
            }
        }
        else
        {
            // 左から右へ、ペンの動きに合わせて見せる(行ごとに順番)
            float lineH = fs * 1.33f;
            int lines = Math.Max(1, (int)MathF.Round(s.Height / lineH));
            float per = 1f / lines;
            int cur = Math.Min(lines - 1, (int)(p / per));
            float lp = (p - cur * per) / per;
            if (p >= 1f) { cur = lines - 1; lp = 1f; }
            // 書き終わった行はすべて、いまの行は途中まで
            if (cur > 0)
            {
                dc.PushAxisAlignedClip(new RawRectF(tl.X - fs, tl.Y - fs, tl.X + s.Width + fs, tl.Y + cur * (s.Height / lines)), AntialiasMode.PerPrimitive);
                brush.Color = ink;
                TextKit.Fill(dc, brush, s, tl);
                dc.PopAxisAlignedClip();
            }
            float ly0 = tl.Y + cur * (s.Height / lines), ly1 = ly0 + s.Height / lines;
            float xNow = tl.X + s.Width * lp;
            dc.PushAxisAlignedClip(new RawRectF(tl.X - fs, ly0, xNow, ly1), AntialiasMode.PerPrimitive);
            brush.Color = ink;
            TextKit.Fill(dc, brush, s, tl);
            dc.PopAxisAlignedClip();
            // ペン先は、文字の高さの中で上下にくねくね動く
            penAt = new Vector2(xNow, (ly0 + ly1) / 2f + MathF.Sin(t * 26f) * fs * 0.25f + MathF.Sin(t * 11f) * fs * 0.1f);
        }
        if (ShowPen && p < 1f + 0.4f / wt)
        {
            float pa = outK * (p < 1f ? 1f : MathF.Max(0f, 1f - (t - wt) / 0.4f));
            Pen(dc, brush, penAt + (p >= 1f ? new Vector2(1, -1) * (t - wt) * fs * 2f : Vector2.Zero), fs * 0.45f, Color, pa);
        }
        dc.Transform = prev;
    }
}
