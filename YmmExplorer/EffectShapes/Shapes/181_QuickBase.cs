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
//  181番以降の図形の共通の土台
//   どの図形にも同じ項目(文字・色・位置・大きさ・値・速さ・数・形のバリエーションなど)があり、
//   図形ごとに、使う項目と意味を決めている(使わない項目は、変えても何も起きない)
// =====================================================================

/// <summary>181番以降の図形のプラグインの共通部分</summary>
public abstract class QuickShapePlugin : IShapePlugin
{
    public abstract string Name { get; }
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public abstract IShapeParameter CreateShapeParameter(SharedDataStore? sharedData);
}

/// <summary>181番以降の図形の、共通の設定項目と描くための小道具</summary>
public abstract class QuickShapeParameter : EffectShapeParameterBase, IParamGuide
{
    static readonly IReadOnlyDictionary<string, ParamGuide> noGuides = new Dictionary<string, ParamGuide>();

    /// <summary>図形ごとの設定項目の案内(使わない項目を隠す・名前を図形に合わせる)</summary>
    protected virtual IReadOnlyDictionary<string, ParamGuide> Guides => noGuides;

    public ParamGuide? Guide(string propName) => Guides.TryGetValue(propName, out var g) ? g : null;

    protected QuickShapeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Amount = new Animation(DefAmount, -1000, 1000);
        Amount2 = new Animation(DefAmount2, -1000, 1000);
    }

    /// <summary>図形ごとの「値」「値2」の最初の値</summary>
    protected virtual double DefAmount => 70;
    protected virtual double DefAmount2 => 40;

    [Display(GroupName = "文字", Name = "文字", Description = "文字を使う図形で使います。1行に1つ、またはカンマ区切りで並べる図形もあります")]
    [TextEditor(AcceptsReturn = true)]
    public string Text { get => text; set => Set(ref text, value); }
    string text = "";

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "色", Name = "メインの色")]
    [ColorPicker]
    public WpfColor Color1 { get => color1; set => Set(ref color1, value); }
    WpfColor color1 = WpfColor.FromRgb(0x4F, 0xA8, 0xFF);

    [Display(GroupName = "色", Name = "サブの色")]
    [ColorPicker]
    public WpfColor Color2 { get => color2; set => Set(ref color2, value); }
    WpfColor color2 = WpfColor.FromRgb(0xFF, 0x7A, 0x59);

    [Display(GroupName = "色", Name = "アクセントの色")]
    [ColorPicker]
    public WpfColor Color3 { get => color3; set => Set(ref color3, value); }
    WpfColor color3 = WpfColor.FromRgb(0xFF, 0xD5, 0x4A);

    [Display(GroupName = "色", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ")]
    [AnimationSlider("F0", "%", 10, 300)]
    public Animation Scale { get; } = new Animation(100, 1, 2000);

    [Display(GroupName = "位置と大きさ", Name = "回転")]
    [AnimationSlider("F0", "°", -180, 180)]
    public Animation Rotation { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "調整", Name = "値・進み具合", Description = "ゲージの量・割合・角度など、図形ごとの主な値(0〜100)")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Amount { get; }

    [Display(GroupName = "調整", Name = "値2", Description = "図形ごとの、2つめの値(0〜100)")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Amount2 { get; }

    [Display(GroupName = "調整", Name = "速さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Speed { get; } = new Animation(100, -5000, 5000);

    [Display(GroupName = "調整", Name = "数")]
    [TextBoxSlider("F0", "", 1, 100)]
    [Range(0, 2000)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 8;

    [Display(GroupName = "調整", Name = "形のバリエーション", Description = "図形ごとに 1〜4 の形・模様の違いがあります")]
    [TextBoxSlider("F0", "", 1, 4)]
    [Range(1, 10)]
    public int Variant { get => variant; set => Set(ref variant, value); }
    int variant = 1;

    [Display(GroupName = "調整", Name = "背景も塗る")]
    [ToggleSlider]
    public bool Background { get => background; set => Set(ref background, value); }
    bool background = true;

    [Display(GroupName = "動き", Name = "出る時間")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation InTime { get; } = new Animation(0.3, 0, 60);

    [Display(GroupName = "動き", Name = "消える時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation OutTime { get; } = new Animation(0.25, 0, 60);

    protected sealed override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Opacity, PosX, PosY, Scale, Rotation, Amount, Amount2, Speed, InTime, OutTime];

    public sealed override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float a = DrawKit.InOut(c, c.F(InTime), c.F(OutTime)) * c.F(Opacity) / 100f;
        if (a <= 0.002f) return;
        using var b = dc.CreateSolidColorBrush(default);
        var prev = dc.Transform;
        float rot = c.F(Rotation);
        if (MathF.Abs(rot) > 0.01f) dc.Transform = Matrix3x2.CreateRotation(Gfx.Rad(rot), P0(c)) * prev;
        Paint(dc, c, b, a);
        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        dc.Transform = prev;
    }

    /// <summary>図形ごとの描き方(a = 出入りと不透明度を合わせた濃さ)</summary>
    protected abstract void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a);

    // ------------------------------------------------------------------ よく使う値
    /// <summary>1920×1080 を基準にした、1px の大きさ(「大きさ」もかかる)</summary>
    protected float U(in DrawContext c) => MathF.Min(c.Width / 1920f, c.Height / 1080f) * c.F(Scale) / 100f;
    /// <summary>画面の大きさだけの基準(背景用。「大きさ」はかからない)</summary>
    protected static float U0(in DrawContext c) => MathF.Min(c.Width / 1920f, c.Height / 1080f);
    protected Vector2 P0(in DrawContext c) => new(c.F(PosX) / 100f * c.Width, c.F(PosY) / 100f * c.Height);
    protected float Amt(in DrawContext c) => Math.Clamp(c.F(Amount) / 100f, 0f, 1f);
    protected float Amt2(in DrawContext c) => Math.Clamp(c.F(Amount2) / 100f, 0f, 1f);
    protected float Spd(in DrawContext c) => c.F(Speed) / 100f;
    /// <summary>出てからの進み(0→1、出る時間をかけて)</summary>
    protected float Intro(in DrawContext c, float delay = 0f, float dur = 0.5f) => Math.Clamp((c.Time - delay) / MathF.Max(0.01f, dur), 0f, 1f);
    protected string Str => TextKit.Lines(Text);
    /// <summary>専用3Dモデル用: 「文字」の行・項目(カンマ・読点・改行で区切る)</summary>
    internal List<string> ItemsForModel => Items;
    /// <summary>専用3Dモデル用: 画面いっぱいの背景として描く図形か(位置・大きさをかけない)</summary>
    internal virtual bool SceneForModel => Models3D.IsScene(GetType());
    /// <summary>専用3Dモデル用: 出る時に、はずんで大きくなるか</summary>
    private protected virtual bool PopInForModel => false;

    /// <summary>専用の3Dモデルがあれば、それを組み立てる(2Dの絵と同じ位置・大きさ・動き)</summary>
    internal override bool BuildNative3D(in DrawContext c, Native3D n)
    {
        if (!UseModel3D || !Models3D.TryGet(GetType(), out var model)) return false;
        bool scene = SceneForModel;
        float a = DrawKit.InOut(c, c.F(InTime), c.F(OutTime)) * c.F(Opacity) / 100f;
        float fov = Math.Clamp(c.F(Fov3D), 1f, 170f) * MathF.PI / 180f;
        var o = scene ? new Vector2(c.Width / 2f, c.Height / 2f) : P0(c);
        n.Lit = true; n.Width = c.Width; n.Height = c.Height;
        n.Cam = c.Height / 2f / MathF.Tan(fov / 2f);
        n.PivotXY = new Vector2(o.X - c.Width / 2f, c.Height / 2f - o.Y); n.PivotZ = 0f;
        if (a <= 0.002f) return true;
        var m = new Sculpt(n, o.X, o.Y, scene ? U0(c) : U(c), a, c.Width, c.Height);
        m.Font = string.IsNullOrWhiteSpace(Font) ? TextKit.DefaultFont : Font;
        m.Now = c.Time;
        float pi = scene || !PopInForModel ? 1f : MathF.Max(.01f, DrawKit.EaseOutBack(Intro(c, 0f, .4f)));
        float t = c.Time * Spd(c), v = Amt(c), v2 = Amt2(c);
        float rot = Gfx.Rad(c.F(Rotation));
        m.Turn(0, 0, rot, () => m.Scale(0, 0, 0, pi, () => model(this, m, t, v, v2)));
        return true;
    }
    protected List<string> Items => TextKit.Lines(Text).Split(new[] { '\n', ',', '、' }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
    protected Color4 C1(float a) => Gfx.C4(Color1, a);
    protected Color4 C2(float a) => Gfx.C4(Color2, a);
    protected Color4 C3(float a) => Gfx.C4(Color3, a);
    protected static Color4 Rgb(float r, float g, float bl, float a) => new(r, g, bl, a);
    protected static Color4 Col(WpfColor col, float a) => Gfx.C4(col, a);
    protected static WpfColor Hex(uint rgb) => WpfColor.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);

    // ------------------------------------------------------------------ 描く小道具
    protected static Vector2 Pol(Vector2 c, float r, float ang) => c + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * r;
    protected static void Poly(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, IList<Vector2> pts, Color4 col) { if (pts.Count < 3) return; b.Color = col; Gfx.FillPoly(dc, b, pts); }
    protected static void Outline(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, IList<Vector2> pts, Color4 col, float w) { if (pts.Count < 2) return; b.Color = col; Gfx.Polyline(dc, b, ShapeGeom.Closed(pts.ToList()), w); }
    protected static void Path(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, IReadOnlyList<Vector2> pts, Color4 col, float w) { if (pts.Count < 2) return; b.Color = col; Gfx.Polyline(dc, b, pts, w); }
    protected static void Ln(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 p, Vector2 q, Color4 col, float w) { b.Color = col; dc.DrawLine(p, q, b, w); }
    protected static void Dot(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 p, float r, Color4 col) { if (r <= 0f) return; b.Color = col; dc.FillEllipse(new Ellipse(p, r, r), b); }
    protected static void Oval(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 p, float rx, float ry, Color4 col) { if (rx <= 0f || ry <= 0f) return; b.Color = col; dc.FillEllipse(new Ellipse(p, rx, ry), b); }
    protected static void Ring(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 p, float r, Color4 col, float w) { if (r <= 0f) return; b.Color = col; dc.DrawEllipse(new Ellipse(p, r, r), b, w); }
    protected static void Box(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float x0, float y0, float x1, float y1, Color4 col) { b.Color = col; dc.FillRectangle(new RawRectF(MathF.Min(x0, x1), MathF.Min(y0, y1), MathF.Max(x0, x1), MathF.Max(y0, y1)), b); }
    protected static void Frame(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float x0, float y0, float x1, float y1, Color4 col, float w) { b.Color = col; dc.DrawRectangle(new RawRectF(x0, y0, x1, y1), b, w); }
    protected static void RBox(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 ctr, float bw, float bh, float r, Color4 col) { if (bw <= 0f || bh <= 0f) return; b.Color = col; Gfx.FillPoly(dc, b, ShapeGeom.RoundRect(ctr.X, ctr.Y, bw, bh, MathF.Min(r, MathF.Min(bw, bh) / 2f))); }
    protected static void RFrame(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 ctr, float bw, float bh, float r, Color4 col, float w) { if (bw <= 0f || bh <= 0f) return; b.Color = col; Gfx.Polyline(dc, b, ShapeGeom.Closed(ShapeGeom.RoundRect(ctr.X, ctr.Y, bw, bh, MathF.Min(r, MathF.Min(bw, bh) / 2f))), w); }
    protected static void Star(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 p, float r, int n, float inner, float rot, Color4 col) { if (r <= 0f) return; b.Color = col; Gfx.FillPoly(dc, b, ShapeGeom.StarPoly(p, r, n, inner, rot)); }
    protected static void Heart(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vector2 p, float r, float rot, Color4 col) { if (r <= 0f) return; b.Color = col; Gfx.FillPoly(dc, b, ShapeGeom.HeartPoly(p, r, rot)); }
    protected static void Shade(ID2D1DeviceContext dc, Vector2 p0, Vector2 p1, Color4 c0, Color4 c1, float x0, float y0, float x1, float y1)
    {
        using var g = Gfx.Linear(dc, p0, p1, (0f, c0), (1f, c1));
        dc.FillRectangle(new RawRectF(x0, y0, x1, y1), g);
    }
    protected static void Sky(ID2D1DeviceContext dc, float w, float h, Color4 top, Color4 bottom) => Shade(dc, new Vector2(0, 0), new Vector2(0, h), top, bottom, 0, 0, w, h);
    protected static List<Vector2> Pts(Vector2 c, float sx, float sy, params float[] xy)
    {
        var l = new List<Vector2>(xy.Length / 2);
        for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(c + new Vector2(xy[i] * sx, xy[i + 1] * sy));
        return l;
    }

    /// <summary>文字を、中心を合わせて描く(ふち取りつき)</summary>
    protected void Say(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, string s, float size, Vector2 center, Color4 col, Color4 outline = default, float ow = 0f, bool bold = true, float maxW = 0f, float scale = 1f)
    {
        if (string.IsNullOrEmpty(s) || size <= 0.5f) return;
        var ts = TextKit.Get(s, size, bold, Font, maxW, 1);
        TextKit.DrawCentered(dc, b, ts, center, col, outline, ow, scale);
    }

    /// <summary>文字を、左上を合わせて描く。幅を返す</summary>
    protected float SayAt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, string s, float size, Vector2 topLeft, Color4 col, bool bold = true, float maxW = 0f)
    {
        if (string.IsNullOrEmpty(s) || size <= 0.5f) return 0f;
        var ts = TextKit.Get(s, size, bold, Font, maxW, 0);
        TextKit.Draw(dc, b, ts, topLeft, col);
        return ts.Width;
    }

    private protected TextShape Measure(string s, float size, bool bold = true, float maxW = 0f) => TextKit.Get(s, size, bold, Font, maxW, 0);
}
