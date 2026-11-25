using System.Collections.Concurrent;
using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using WpfColor = System.Windows.Media.Color;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// =====================================================================
//  611番以降の図形の土台。座標は「図形の中心からの 1920×1080 基準の値」で書く
//   Scene が true の図形は、画面全体(中心が画面の中心)に描く背景
// =====================================================================

/// <summary>1フレーム分の描く道具(図形の中心からの座標で描く)</summary>
internal sealed class ArtPen(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, Vector2 o, float k, float alpha, string font, float w, float h, float now)
{
    public readonly ID2D1DeviceContext Dc = dc;
    public readonly float Al = alpha, Now = now;
    /// <summary>画面の幅と高さ(座標の単位)</summary>
    public readonly float W = w / k, H = h / k;
    public Vector2 V(float x, float y) => o + new Vector2(x, y) * k;
    public Color4 C(uint rgb, float a = 1f) => new(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, Math.Clamp(a * Al, 0f, 1f));
    public Color4 C(int rgb, float a = 1f) => C((uint)rgb, a);
    public Color4 C(WpfColor c, float a = 1f) => Gfx.C4(c, a * Al);
    public Color4 C(Color4 c, float a) => new(c.R, c.G, c.B, Math.Clamp(c.A * a, 0f, 1f));
    public void D(float x, float y, float r, Color4 c) { if (r <= 0f) return; br.Color = c; dc.FillEllipse(new Ellipse(V(x, y), r * k, r * k), br); }
    public void E(float x, float y, float rx, float ry, Color4 c) { if (rx <= 0f || ry <= 0f) return; br.Color = c; dc.FillEllipse(new Ellipse(V(x, y), rx * k, ry * k), br); }
    public void EL(float x, float y, float rx, float ry, Color4 c, float lw) { if (rx <= 0f || ry <= 0f) return; br.Color = c; dc.DrawEllipse(new Ellipse(V(x, y), rx * k, ry * k), br, lw * k); }
    public void Ring(float x, float y, float r, Color4 c, float lw) => EL(x, y, r, r, c, lw);
    public void B(float x, float y, float bw, float bh, Color4 c) { br.Color = c; var p = V(x - bw / 2f, y - bh / 2f); dc.FillRectangle(new RawRectF(p.X, p.Y, p.X + bw * k, p.Y + bh * k), br); }
    public void R(float x, float y, float bw, float bh, float r, Color4 c) { if (bw <= 0f || bh <= 0f) return; br.Color = c; var q = V(x, y); Gfx.FillPoly(dc, br, ShapeGeom.RoundRect(q.X, q.Y, bw * k, bh * k, MathF.Min(r, MathF.Min(bw, bh) / 2f) * k)); }
    public void RL(float x, float y, float bw, float bh, float r, Color4 c, float lw) { if (bw <= 0f || bh <= 0f) return; br.Color = c; var q = V(x, y); Gfx.Polyline(dc, br, ShapeGeom.Closed(ShapeGeom.RoundRect(q.X, q.Y, bw * k, bh * k, MathF.Min(r, MathF.Min(bw, bh) / 2f) * k)), lw * k); }
    public void L(float x0, float y0, float x1, float y1, Color4 c, float lw) { br.Color = c; dc.DrawLine(V(x0, y0), V(x1, y1), br, lw * k); }
    List<Vector2> Pts(float[] xy) { var l = new List<Vector2>(xy.Length / 2); for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(V(xy[i], xy[i + 1])); return l; }
    public void F(Color4 c, params float[] xy) { br.Color = c; Gfx.FillPoly(dc, br, Pts(xy)); }
    public void P(Color4 c, float lw, params float[] xy) { br.Color = c; Gfx.Polyline(dc, br, Pts(xy), lw * k); }
    public void Line(Color4 c, float lw, IEnumerable<Vector2> pts) { br.Color = c; Gfx.Polyline(dc, br, pts.Select(q => V(q.X, q.Y)).ToList(), lw * k); }
    public void Fill(Color4 c, IEnumerable<Vector2> pts) { br.Color = c; Gfx.FillPoly(dc, br, pts.Select(q => V(q.X, q.Y)).ToList()); }
    public void Arc(float x, float y, float r, float a0, float a1, Color4 c, float lw) { br.Color = c; Gfx.Polyline(dc, br, Gfx.ArcPts(V(x, y), r * k, a0, a1, Math.Max(8, (int)(MathF.Abs(a1 - a0) * 16f))), lw * k); }
    public void Pie(float x, float y, float r, float a0, float a1, Color4 c) { var q = V(x, y); var pts = Gfx.ArcPts(q, r * k, a0, a1, Math.Max(8, (int)(MathF.Abs(a1 - a0) * 16f))); pts.Insert(0, q); br.Color = c; Gfx.FillPoly(dc, br, pts); }
    public void Star(float x, float y, float r, int n, float inner, float rot, Color4 c) { if (r <= 0f) return; br.Color = c; Gfx.FillPoly(dc, br, ShapeGeom.StarPoly(V(x, y), r * k, n, inner, rot)); }
    public void T(string s, float size, float x, float y, Color4 c, Color4 edge = default, float ew = 0f, float maxW = 0f)
    {
        if (string.IsNullOrEmpty(s) || size * k <= .5f) return;
        TextKit.DrawCentered(dc, br, TextKit.Get(s, size * k, true, font, maxW * k, 1), V(x, y), c, edge, ew * k, 1f);
    }
    /// <summary>(x,y) を中心に回して描く</summary>
    public void Turn(float x, float y, float ang, Action draw) { var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(ang, V(x, y)) * prev; draw(); dc.Transform = prev; }
    /// <summary>(x,y) を中心に拡大して描く</summary>
    public void Zoom(float x, float y, float sx, float sy, Action draw) { var prev = dc.Transform; dc.Transform = Matrix3x2.CreateScale(sx, sy, V(x, y)) * prev; draw(); dc.Transform = prev; }
    /// <summary>ずらして描く</summary>
    public void Move(float x, float y, Action draw) { var prev = dc.Transform; dc.Transform = Matrix3x2.CreateTranslation(x * k, y * k) * prev; draw(); dc.Transform = prev; }
    public static float R01(int n) => Gfx.Hash01(n);
    public static float Fr(float x) => x - MathF.Floor(x);
    public static float Sin(float x) => MathF.Sin(x);
    public static float Cos(float x) => MathF.Cos(x);
    public static float Cl(float x) => Math.Clamp(x, 0f, 1f);
}

public abstract class Q4Art : Q3Param
{
    protected Q4Art(SharedDataStore? s) : base(s) { }
    static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, ParamGuide>> cache = new();
    /// <summary>使う項目(「項目=名前|説明|最小|最大」)</summary>
    protected virtual string[] Use => ["Color1=メインの色", "Speed=動く速さ"];
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => cache.GetOrAdd(GetType(), _ => G("この図形", Use));
    protected virtual bool Scene => false;

    protected sealed override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        bool scene = Scene;
        var o = scene ? new Vector2(c.Width / 2f, c.Height / 2f) : P0(c);
        float k = scene ? U0(c) : U(c);
        var pen = new ArtPen(dc, b, o, k, a, Font, c.Width, c.Height, c.Time);
        var prev = dc.Transform;
        if (!scene) { float p = MathF.Max(.01f, DrawKit.EaseOutBack(Intro(c, 0f, .4f))); dc.Transform = Matrix3x2.CreateScale(p, p, o) * prev; }
        Art(pen, c.Time * Spd(c), Amt(c), Amt2(c));
        dc.Transform = prev;
    }

    private protected abstract void Art(ArtPen p, float t, float v, float v2);

    /// <summary>専用の3Dモデルがあれば、それを組み立てる(2Dの絵と同じ位置・大きさ・動き)</summary>
    internal override bool BuildNative3D(in DrawContext c, Native3D n)
    {
        if (!UseModel3D || !Models3D.TryGet(GetType(), out var model)) return false;
        bool scene = Scene;
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
        float pi = scene ? 1f : MathF.Max(.01f, DrawKit.EaseOutBack(Intro(c, 0f, .4f)));
        float t = c.Time * Spd(c), v = Amt(c), v2 = Amt2(c);
        float rot = Gfx.Rad(c.F(Rotation));
        m.Turn(0, 0, rot, () => m.Scale(0, 0, 0, pi, () => model(this, m, t, v, v2)));
        return true;
    }
}
