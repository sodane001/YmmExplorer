using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;

namespace YMM4EffectShapes;

// =====================================================================
//  文字PV背景の2Dの背景(v126 で足した「並ぶ図形」「漂う図形」と模様)。
//  ICanvas に描くので、2D(平面の絵)と3D(厚みのある立体)の両方で同じ形になる。
//  座標は 1920×1080 を基準にした単位で、画面のまん中が (0,0)、y は下向き。z は手前への高さ(3Dだけで使う)
// =====================================================================

/// <summary>色付きで描く道具(2D と 3D で中身が違う)</summary>
internal interface ICanvas
{
    void Fill(List<Vector2> pts, Vector3 col, float a, float z, float d);
    void Line(float x0, float y0, float x1, float y1, float w, Vector3 col, float a, float z);
    void Dot(float x, float y, float r, Vector3 col, float a, float z);
    void Ring(float x, float y, float r, float w, Vector3 col, float a, float z);
    /// <summary>奥行き z(マイナスが奥)に置く物の、(位置の倍率, 大きさの倍率)。2Dは奥ほど小さく、3Dは遠近で自然に小さく見えるので位置を広げる</summary>
    Vector2 Depth(float z);
}

internal sealed class Canvas2D(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, float u, PvStage.Setup s) : ICanvas
{
    Color4 C(Vector3 c, float a)
    {
        var x = Vector3.Clamp(c + new Vector3(s.Flash * .12f), Vector3.Zero, Vector3.One);
        return new(x.X, x.Y, x.Z, Math.Clamp(a * s.Alpha, 0f, 1f));
    }
    public void Fill(List<Vector2> pts, Vector3 col, float a, float z, float d)
    {
        if (pts.Count < 3 || a <= .002f) return;
        var q = new List<Vector2>(pts.Count);
        foreach (var p in pts) q.Add(p * u);
        br.Color = C(col, a); Gfx.FillPoly(dc, br, q);
    }
    public void Line(float x0, float y0, float x1, float y1, float w, Vector3 col, float a, float z)
    {
        if (a <= .002f) return;
        br.Color = C(col, a); dc.DrawLine(new(x0 * u, y0 * u), new(x1 * u, y1 * u), br, MathF.Max(.8f, w * u));
    }
    public void Dot(float x, float y, float r, Vector3 col, float a, float z)
    {
        if (r <= .05f || a <= .002f) return;
        br.Color = C(col, a); dc.FillEllipse(new Ellipse(new(x * u, y * u), r * u, r * u), br);
    }
    public void Ring(float x, float y, float r, float w, Vector3 col, float a, float z)
    {
        if (r <= .05f || a <= .002f) return;
        br.Color = C(col, a); dc.DrawEllipse(new Ellipse(new(x * u, y * u), r * u, r * u), br, MathF.Max(.8f, w * u));
    }
    public Vector2 Depth(float z) => new(1f, z >= 0f ? 1f : 1058f / (1058f - z));
}

internal sealed class Canvas3D(Sculpt m, PvStage.Setup s) : ICanvas
{
    Vector4 V(Vector3 c, float a)
    {
        var x = Vector3.Clamp(c + new Vector3(s.Flash * .12f), Vector3.Zero, Vector3.One);
        return new(x.X, x.Y, x.Z, Math.Clamp(a * s.Alpha, 0f, 1f));
    }
    public void Fill(List<Vector2> pts, Vector3 col, float a, float z, float d) { if (pts.Count >= 3 && a > .002f) m.Slab(pts, z, MathF.Max(1f, d), V(col, a)); }
    public void Line(float x0, float y0, float x1, float y1, float w, Vector3 col, float a, float z) { if (a > .002f) m.Limb(x0, y0, z, x1, y1, z, w / 2f, w / 2f, V(col, a)); }
    public void Dot(float x, float y, float r, Vector3 col, float a, float z) { if (r > .05f && a > .002f) m.Ball(x, y, z, r, r, r * .6f, V(col, a)); }
    public void Ring(float x, float y, float r, float w, Vector3 col, float a, float z)
    {
        if (r <= .05f || a <= .002f) return;
        int keep = m.Seg; m.Seg = Math.Clamp((int)(r / 6f), 16, 48);
        m.Torus(x, y, z, r, w / 2f, V(col, a));
        m.Seg = keep;
    }
    public Vector2 Depth(float z) => new(z >= 0f ? 1f : (1058f - z) / 1058f, 1f);
}

internal sealed class PvPat
{
    public static bool IsPat(PvScene x) => (x >= PvScene.TlGridCircle && x <= PvScene.PtCircleEq) || x >= PvScene.TlSpinCircle;

    readonly ICanvas c;
    readonly PvStage.Setup s;
    readonly float ex, ey, t, mv, beats, sp;
    readonly int st;

    public PvPat(ICanvas canvas, in PvStage.Setup s, float ex, float ey)
    {
        c = canvas; this.s = s; this.ex = ex; this.ey = ey;
        t = s.T * s.Spin; mv = s.Dist * 38f; sp = MathF.Max(.2f, MathF.Abs(s.Spin));
        beats = s.Sync ? s.Beats * s.PerBeat : s.T * 2f * sp * s.PerBeat;
        st = (int)MathF.Floor(beats + 1e-4f);
    }

    // ---------------------------------------------------------------- 道具
    float R(int i, int k) => Gfx.Hash01(unchecked(s.Seed * 131 + i * 19 + k * 1013 + 11));
    static float Mod(float x, float m) => x - MathF.Floor(x / m) * m;
    static float Fr(float x) => x - MathF.Floor(x);
    Vector3 Pick(int i) { float r = R(i, 8); return r < .46f ? s.Pal.A : r < .8f ? s.Pal.B : s.Pal.C; }
    Vector3 Ramp(float q)
    {
        q = Math.Clamp(q, 0f, 1f);
        return q < .5f ? Vector3.Lerp(s.Pal.A, s.Pal.B, q * 2f) : Vector3.Lerp(s.Pal.B, s.Pal.C, q * 2f - 1f);
    }
    float Calm(float x, float y)
    {
        if (s.Clear <= 0f) return 1f;
        float d = MathF.Sqrt(x * x / (1920f * 1920f * .2f) + y * y / (1080f * 1080f * .13f));
        return Math.Clamp(d / s.Clear, .3f, 1f);
    }
    /// <summary>正多角形(inner が 1 より小さいと、へこみのある星形)</summary>
    static List<Vector2> Poly(float x, float y, float r, int n, float rot, float inner = 1f)
    {
        bool star = inner < .999f; int m = star ? n * 2 : n; var l = new List<Vector2>(m);
        for (int j = 0; j < m; j++) { float a = rot + j * MathF.Tau / m, rr = star && (j & 1) == 1 ? r * inner : r; l.Add(new(x + MathF.Cos(a) * rr, y + MathF.Sin(a) * rr)); }
        return l;
    }
    static List<Vector2> Local(float x, float y, float r, float rot, params float[] xy)
    {
        float cs = MathF.Cos(rot), sn = MathF.Sin(rot); var l = new List<Vector2>(xy.Length / 2);
        for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(new(x + (xy[i] * cs - xy[i + 1] * sn) * r, y + (xy[i] * sn + xy[i + 1] * cs) * r));
        return l;
    }
    void Outline(List<Vector2> p, float w, Vector3 col, float a, float z)
    {
        for (int i = 0; i < p.Count; i++) { var q0 = p[i]; var q1 = p[(i + 1) % p.Count]; c.Line(q0.X, q0.Y, q1.X, q1.Y, w, col, a, z); }
    }

    static readonly float[] HeartXY = MakeHeart();
    static float[] MakeHeart()
    {
        var l = new float[48];
        for (int k = 0; k < 24; k++)
        {
            float b = k * MathF.Tau / 24f;
            l[k * 2] = MathF.Pow(MathF.Sin(b), 3); l[k * 2 + 1] = -(13f * MathF.Cos(b) - 5f * MathF.Cos(2 * b) - 2f * MathF.Cos(3 * b) - MathF.Cos(4 * b)) / 16f;
        }
        return l;
    }

    /// <summary>図形を1つ描く。kind: 0丸 1四角 2三角 3六角形 4星 5ハート 6ひし形 7十字 8輪 9半円 10花 11矢印</summary>
    void Shape(int kind, float x, float y, float r, float rot, Vector3 col, float a, float z, float d)
    {
        switch (kind)
        {
            case 0: c.Fill(Poly(x, y, r, 28, rot), col, a, z, d); break;
            case 1: c.Fill(Poly(x, y, r, 4, rot + MathF.PI / 4f), col, a, z, d); break;
            case 2: c.Fill(Poly(x, y, r, 3, rot - MathF.PI / 2f), col, a, z, d); break;
            case 3: c.Fill(Poly(x, y, r, 6, rot), col, a, z, d); break;
            case 4: c.Fill(Poly(x, y, r, 5, rot - MathF.PI / 2f, .45f), col, a, z, d); break;
            case 5: c.Fill(Local(x, y, r, rot, HeartXY), col, a, z, d); break;
            case 6: c.Fill(Local(x, y, r, rot, 0, -1.3f, .8f, 0, 0, 1.3f, -.8f, 0), col, a, z, d); break;
            case 7: c.Fill(Local(x, y, r, rot, -.34f, -1, .34f, -1, .34f, -.34f, 1, -.34f, 1, .34f, .34f, .34f, .34f, 1, -.34f, 1, -.34f, .34f, -1, .34f, -1, -.34f, -.34f, -.34f), col, a, z, d); break;
            case 8: c.Ring(x, y, r * .78f, r * .36f, col, a, z); break;
            case 9:
                {
                    var l = new List<Vector2>(16);
                    for (int j = 0; j <= 14; j++) { float b = rot + j * MathF.PI / 14f; l.Add(new(x + MathF.Cos(b) * r, y + MathF.Sin(b) * r)); }
                    c.Fill(l, col, a, z, d); break;
                }
            case 10:
                for (int j = 0; j < 5; j++) { float b = rot + j * MathF.Tau / 5f; c.Dot(x + MathF.Cos(b) * r * .55f, y + MathF.Sin(b) * r * .55f, r * .42f, col, a, z); }
                c.Dot(x, y, r * .3f, Vector3.Lerp(col, Vector3.One, .6f), a, z + 4f); break;
            default: c.Fill(Local(x, y, r, rot, -1, -.3f, .2f, -.3f, .2f, -.7f, 1, 0, .2f, .7f, .2f, .3f, -1, .3f), col, a, z, d); break;
        }
    }

    // ---------------------------------------------------------------- 入口
    public void Run()
    {
        if (s.Scene >= PvScene.TlSpinCircle) { RunV127(); return; }
        int i = (int)s.Scene - (int)PvScene.TlGridCircle;
        if (i < 12) { TileGrid(i); return; }
        if (i < 24) { TileFloat(i - 12); return; }
        switch (s.Scene)
        {
            case PvScene.PtTunTri: Tunnel(3, false); break;
            case PvScene.PtTunSq: Tunnel(4, false); break;
            case PvScene.PtTunHex: Tunnel(6, false); break;
            case PvScene.PtTunStar: Tunnel(5, true); break;
            case PvScene.PtRadial2: Radial2(); break;
            case PvScene.PtRadialThin: RadialThin(); break;
            case PvScene.PtKaleido: Kaleido(); break;
            case PvScene.PtMandala: Mandala(); break;
            case PvScene.PtGears: Gears(); break;
            case PvScene.PtPulse: Pulse(); break;
            case PvScene.PtRipples: Ripples(); break;
            case PvScene.PtSynth: Synth(); break;
            case PvScene.PtBurst: Burst(); break;
            case PvScene.PtPopDots: PopDots(); break;
            case PvScene.PtBolt: Bolt(); break;
            case PvScene.PtStripes5: Stripes5(); break;
            case PvScene.PtBands: Bands(); break;
            case PvScene.PtWarpCheck: WarpCheck(); break;
            case PvScene.PtPetalRing: PetalRing(); break;
            default: CircleEq(); break;
        }
    }

    // ---------------------------------------------------------------- 並ぶ図形・漂う図形
    void TileGrid(int kind)
    {
        float g = MathF.Max(70f, 130f * s.Spread * MathF.Max(.4f, s.Size)), h = g * .9f, ox = Mod(mv, g);
        int c0 = (int)MathF.Ceiling(ex / g) + 1, r0 = (int)MathF.Ceiling(ey / h) + 1;
        for (int r = -r0; r <= r0; r++)
            for (int q = -c0; q <= c0; q++)
            {
                float x = q * g + ((r & 1) != 0 ? g / 2f : 0f) + ox - g / 2f, y = r * h;
                float p = Math.Clamp(.5f + .5f * MathF.Sin(q * .6f + r * .45f - t * 1.6f) + s.Flash * .3f, 0f, 1f);
                Shape(kind, x, y, g * .36f * (.45f + .55f * p), t * .6f + (q + r) * .4f, Ramp(p), .92f * Calm(x, y), p * 90f, 24f);
            }
    }

    void TileFloat(int kind)
    {
        int n = Math.Clamp(s.N, 10, 140);
        for (int i = 0; i < n; i++)
        {
            float z = 60f - R(i, 6) * 700f; var dep = c.Depth(z);
            float x = (Fr(R(i, 0) + mv * .0001f * (.3f + R(i, 3)) + t * .01f * (R(i, 4) - .5f)) - .5f) * ex * 2.2f, y = (R(i, 1) - .5f) * ey * 2f + MathF.Sin(t * .7f + i) * 30f;
            float r = (20f + R(i, 2) * 50f) * s.Size * dep.Y * (1f + s.Flash * .15f), rot = t * (R(i, 5) - .5f) * 2f + i;
            Shape(kind, x * dep.X, y * dep.X, r, rot, Pick(i), .88f * Calm(x, y), z, r * .4f);
        }
    }

    // ---------------------------------------------------------------- 模様
    /// <summary>多角形のトンネル(奥から手前へ広がってくる)</summary>
    void Tunnel(int sides, bool star)
    {
        const int n = 14;
        var order = Enumerable.Range(0, n).Select(j => (j, f: Fr(j / (float)n + t * .06f + mv * .0004f))).OrderBy(v => v.f);
        foreach (var (j, f) in order)
        {
            float z = -2600f * (1f - f); var dep = c.Depth(z);
            float r = 900f * s.Spread * dep.Y, a = Math.Clamp(f * 2f, 0f, 1f) * .95f, rot = j * .18f + t * .25f;
            Outline(Poly(0, 0, r, sides, rot - MathF.PI / 2f, star ? .5f : 1f), 12f * dep.Y, Ramp(Fr(j / (float)n + st * .07f)), a, z);
        }
    }

    void Radial2()
    {
        const int n = 24; float big = 1600f, rot = t * .2f;
        for (int k = 0; k < n; k++)
        {
            float a0 = rot + k * MathF.Tau / n, a1 = a0 + MathF.Tau / n;
            c.Fill([new(0, 0), new(MathF.Cos(a0) * big, MathF.Sin(a0) * big), new(MathF.Cos(a1) * big, MathF.Sin(a1) * big)], k % 2 == 0 ? s.Pal.A : s.Pal.B, .92f, k % 2 == 0 ? 0f : -40f, 6f);
        }
        c.Dot(0, 0, 90f + s.Flash * 30f, s.Pal.C, 1f, 30f);
    }

    void RadialThin()
    {
        const int n = 72; float rot = -t * .15f;
        for (int k = 0; k < n; k++)
        {
            float a = rot + k * MathF.Tau / n, r0 = 130f + (k % 3) * 40f, cs = MathF.Cos(a), sn = MathF.Sin(a), on = (k + st) % 6 == 0 ? 1f : .55f;
            c.Line(cs * r0, sn * r0, cs * 1500f, sn * 1500f, 3f + (k % 2) * 3f, Ramp(k / (float)n), on, (k % 4) * -20f);
        }
    }

    void Kaleido()
    {
        const int seg = 8;
        for (int i = 0; i < 10; i++)
        {
            float r = 120f + R(i, 0) * 700f + MathF.Sin(t * .5f + i) * 60f, a = R(i, 1) * MathF.PI / seg + MathF.Sin(t * .3f + i) * .2f, size = (30f + R(i, 2) * 60f) * s.Size;
            int kind = (int)(R(i, 3) * 8f) % 8; var col = Pick(i);
            for (int m = 0; m < seg; m++)
            {
                float b = m * MathF.Tau / seg + t * .1f, ang = m % 2 == 0 ? b + a : b + MathF.Tau / seg - a;
                Shape(kind, MathF.Cos(ang) * r, MathF.Sin(ang) * r, size, ang * (m % 2 == 0 ? 1f : -1f) + t * .4f, col, .88f, 20f + i * 6f, 12f);
            }
        }
    }

    void Mandala()
    {
        for (int l = 0; l < 6; l++)
        {
            float rad = (110f + l * 120f) * s.Spread, rot = t * (l % 2 == 0 ? .15f : -.12f), pl = (50f + l * 14f) * (1f + s.Flash * .2f); int cnt = 6 + l * 4; var col = Ramp(l / 5f);
            for (int k = 0; k < cnt; k++)
            {
                float a = rot + k * MathF.Tau / cnt, cs = MathF.Cos(a), sn = MathF.Sin(a), x = cs * rad, y = sn * rad;
                c.Fill([new(x - cs * pl, y - sn * pl), new(x - sn * pl * .35f, y + cs * pl * .35f), new(x + cs * pl, y + sn * pl), new(x + sn * pl * .35f, y - cs * pl * .35f)], col, .9f, 10f + l * 10f, 10f);
            }
        }
        c.Dot(0, 0, 70f, s.Pal.C, 1f, 80f);
    }

    void Gears()
    {
        (float x, float y, float r, int teeth, float dir)[] gs = [(-720f, -300f, 170f, 16, 1f), (-470f, -100f, 110f, 10, -1.6f), (700f, 260f, 200f, 18, -1f), (420f, 430f, 120f, 11, 1.6f), (720f, -380f, 90f, 8, 2f)];
        for (int i = 0; i < gs.Length; i++)
        {
            var g = gs[i]; float rot = t * .4f * g.dir; int m = g.teeth * 4; var l = new List<Vector2>(m);
            for (int k = 0; k < m; k++) { float a = rot + k * MathF.Tau / m, rr = (k % 4) < 2 ? g.r : g.r * .82f; l.Add(new(g.x + MathF.Cos(a) * rr, g.y + MathF.Sin(a) * rr)); }
            c.Fill(l, Pick(i), .95f, 20f, 40f);
            c.Dot(g.x, g.y, g.r * .3f, s.Pal.Back1, 1f, 45f);
        }
    }

    void Pulse()
    {
        float fb = Fr(beats); int b0 = (int)MathF.Floor(beats);
        for (int k = 0; k < 7; k++)
        {
            float age = fb + k, r = age * 260f * s.Spread, a = MathF.Max(0f, 1f - age / 7f);
            c.Ring(0, 0, r, 14f * (1f - age / 8f) + 2f, Ramp(((b0 - k) % 6 + 6) % 6 / 5f), a, -age * 60f);
        }
        c.Dot(0, 0, 60f + 40f * s.Flash + 30f * (1f - fb), s.Pal.C, 1f, 40f);
    }

    void Ripples()
    {
        for (int i = 0; i < 6; i++)
        {
            float per = 2.5f + R(i, 0), tt = s.T * sp / per + R(i, 1), ph = Fr(tt); int id = i * 31 + (int)MathF.Floor(tt) * 97;
            float cx = (R(id, 2) - .5f) * ex * 1.8f, cy = (R(id, 3) - .5f) * ey * 1.6f;
            for (int k = 0; k < 3; k++) { float age = ph - k * .12f; if (age > 0f) c.Ring(cx, cy, age * 380f * s.Spread, 4f, Pick(id), (1f - ph) * .9f, 10f + k * 10f); }
        }
    }

    void Synth()
    {
        float cy = -110f, rs = 300f * s.Spread, hz = 180f;
        // 横に切れ目の入った夕日
        for (int k = 0; k < 24; k++)
        {
            float y0 = cy - rs + k * (2f * rs / 24f), hgt = 2f * rs / 24f * (k < 12 ? 1f : 1f - (k - 12) / 14f), ym = y0 + hgt / 2f;
            if (y0 > hz) break;
            float w = MathF.Sqrt(MathF.Max(0f, rs * rs - (ym - cy) * (ym - cy)));
            c.Fill([new(-w, y0), new(w, y0), new(w, MathF.Min(y0 + hgt, hz)), new(-w, MathF.Min(y0 + hgt, hz))], Ramp(k / 23f), 1f, -300f, 6f);
        }
        // 山の影
        foreach (float side in new[] { -1f, 1f })
        {
            var l = new List<Vector2> { new(side * ex, hz) };
            for (int k = 0; k <= 8; k++) { float x = side * (ex - k * (ex - 260f) / 8f); l.Add(new(x, hz - (60f + 90f * R(k + (side > 0 ? 20 : 0), 40)) * (k % 2 == 0 ? 1f : .55f))); }
            l.Add(new(side * 260f, hz));
            c.Fill(l, s.Pal.Back1 * .55f, 1f, -150f, 10f);
        }
        c.Line(-ex, hz, ex, hz, 4f, s.Pal.C, 1f, 0f);
        for (int k = 0; k < 14; k++) { float f = Fr(k / 14f + s.T * .15f * sp + mv * .0003f), y = hz + (ey + 40f - hz) * f * f; c.Line(-ex, y, ex, y, 2f + 3f * f, s.Pal.B, .85f, 0f); }
        for (int q = -12; q <= 12; q++) c.Line(q * 22f, hz, q * 260f, ey + 40f, 2f, s.Pal.B, .85f, 0f);
    }

    void Burst()
    {
        const int n = 22; var outer = new List<Vector2>(n * 2); var inner = new List<Vector2>(n * 2);
        for (int k = 0; k < n * 2; k++)
        {
            float a = k * MathF.PI / n, big = (k & 1) == 0 ? (520f * (1f + .15f * R(k + st * 13, 1)) + s.Flash * 60f) : 330f;
            outer.Add(new(MathF.Cos(a) * big * s.Spread, MathF.Sin(a) * big * .7f * s.Spread));
            inner.Add(new(MathF.Cos(a) * big * .7f * s.Spread, MathF.Sin(a) * big * .5f * s.Spread));
        }
        c.Fill(outer, s.Pal.A, 1f, 0f, 20f);
        c.Fill(inner, s.Pal.B, 1f, 20f, 20f);
        for (int k = 0; k < 60; k++)
        {
            float a = k * MathF.Tau / 60f + R(k, 2) * .05f, r0 = 640f + R(k + st * 7, 3) * 120f, cs = MathF.Cos(a), sn = MathF.Sin(a);
            c.Line(cs * r0, sn * r0 * .75f, cs * 1500f, sn * 1500f * .75f, 3f + R(k, 4) * 5f, s.Pal.C, .8f, -20f);
        }
    }

    void PopDots()
    {
        float g = MathF.Max(60f, 70f * s.Spread);
        int c0 = (int)MathF.Ceiling(ex / g) + 1, r0 = (int)MathF.Ceiling(ey / g) + 1;
        for (int r = -r0; r <= r0; r++)
            for (int q = -c0; q <= c0; q++)
            {
                float x = q * g, y = r * g, p = Fr((x + y * 1.2f) / (ex + ey) * .5f + t * .15f);
                c.Dot(x, y, g * .48f * p, s.Pal.A, .95f, p * 40f);
                c.Dot(x + g / 2f, y + g / 2f, g * .48f * (1f - p), s.Pal.B, .95f, (1f - p) * 40f);
            }
    }

    void Bolt()
    {
        float fl = 1f - Fr(beats);
        for (int b = 0; b < 3; b++)
        {
            int id = b * 17 + st * 101;
            if (R(id, 9) < .35f) continue;
            float x = (R(id, 0) - .5f) * ex * 1.6f, y = -ey - 20f, dy = (2f * ey + 40f) / 14f;
            var glow = Vector3.Lerp(s.Pal.A, Vector3.One, .5f);
            for (int k = 0; k < 14; k++)
            {
                float nx = x + (R(id + k, 1) - .5f) * 160f, ny = y + dy;
                c.Line(x, y, nx, ny, 14f, glow, .35f * fl, 30f); c.Line(x, y, nx, ny, 4f, Vector3.One, fl, 34f);
                if (k == 5)
                {
                    float bx = nx, by = ny;
                    for (int j = 0; j < 4; j++) { float qx = bx + 50f + R(id + j, 3) * 60f, qy = by + dy * .7f; c.Line(bx, by, qx, qy, 3f, Vector3.One, fl * .8f, 34f); bx = qx; by = qy; }
                }
                x = nx; y = ny;
            }
        }
    }

    void Stripes5()
    {
        float w = MathF.Max(60f, 140f * s.Spread), h = 2f * ey + 40f, sk = h * .5f, run = mv + t * 40f, o = Mod(run, w); int b0 = (int)MathF.Floor(run / w);
        Vector3[] cols = [s.Pal.A, s.Pal.B, s.Pal.C, Vector3.Lerp(s.Pal.A, s.Pal.B, .5f), Vector3.Lerp(s.Pal.B, s.Pal.C, .5f)];
        int i0 = (int)MathF.Floor(-ex / w) - 2, i1 = (int)MathF.Ceiling((ex + sk) / w) + 2;
        for (int i = i0; i <= i1; i++)
        {
            float x = i * w + o; int idx = ((i - b0) % 5 + 5) % 5;
            c.Fill([new(x, -ey - 20f), new(x + w, -ey - 20f), new(x + w - sk, ey + 20f), new(x - sk, ey + 20f)], cols[idx], .95f, (i & 1) * -30f, 8f);
        }
    }

    void Bands()
    {
        const int n = 8; float bw = 2f * ex / n;
        float Edge(int k, float y) => -ex + k * bw + MathF.Sin(y / 200f + t + k) * 40f;
        for (int k = 0; k < n; k++)
        {
            var l = new List<Vector2>();
            for (float y = -ey - 20f; y <= ey + 20f; y += 40f) l.Add(new(Edge(k, y), y));
            for (float y = ey + 20f; y >= -ey - 20f; y -= 40f) l.Add(new(k == n - 1 ? ex + 20f : Edge(k + 1, y), y));
            if (k == 0) for (int j = 0; j < l.Count / 2; j++) l[j] = new(-ex - 20f, l[j].Y);
            c.Fill(l, Ramp(k / (n - 1f)), .95f, (k % 2) * -30f, 8f);
        }
    }

    void WarpCheck()
    {
        float g = MathF.Max(70f, 130f * s.Spread);
        int c0 = (int)MathF.Ceiling(ex / g) + 1, r0 = (int)MathF.Ceiling(ey / g) + 1;
        Vector2 P(int q, int r) => new(q * g + MathF.Sin(r * .6f + t) * g * .25f, r * g + MathF.Cos(q * .5f + t * .8f) * g * .25f);
        for (int r = -r0; r < r0; r++)
            for (int q = -c0; q < c0; q++)
                if (((q + r + st) & 1) == 0)
                    c.Fill([P(q, r), P(q + 1, r), P(q + 1, r + 1), P(q, r + 1)], (r & 1) == 0 ? s.Pal.A : s.Pal.B, .95f, 0f, 10f);
    }

    void PetalRing()
    {
        for (int l = 0; l < 2; l++)
        {
            int cnt = l == 0 ? 12 : 20; float rad = (l == 0 ? 300f : 520f) * s.Spread, rot = t * (l == 0 ? .3f : -.2f), size = (l == 0 ? 110f : 70f) * (1f + .15f * s.Flash) * s.Size;
            for (int k = 0; k < cnt; k++)
            {
                float a = rot + k * MathF.Tau / cnt, cs = MathF.Cos(a), sn = MathF.Sin(a), x = cs * rad, y = sn * rad; var pts = new List<Vector2>(16);
                for (int j = 0; j < 16; j++) { float b = j * MathF.Tau / 16f, lx = MathF.Cos(b) * size, ly = MathF.Sin(b) * size * .38f; pts.Add(new(x + lx * cs - ly * sn, y + lx * sn + ly * cs)); }
                c.Fill(pts, Ramp(k / (float)cnt), .92f, 10f + l * 20f, 10f);
            }
        }
        c.Ring(0, 0, 150f * s.Spread, 6f, s.Pal.C, .9f, 30f);
    }

    void CircleEq()
    {
        const int n = 64; float r0 = 260f * s.Spread, f = Fr(beats); f = f * f * (3f - 2f * f);
        for (int k = 0; k < n; k++)
        {
            float a = k * MathF.Tau / n + t * .05f, v = R(k + st * 37, 5) + (R(k + (st + 1) * 37, 5) - R(k + st * 37, 5)) * f;
            float len = (40f + 260f * (.2f + .8f * v)) * (.8f + .2f * s.Flash), half = r0 * MathF.PI / n * .6f, cs = MathF.Cos(a), sn = MathF.Sin(a);
            c.Fill([new(cs * r0 - sn * half, sn * r0 + cs * half), new(cs * (r0 + len) - sn * half, sn * (r0 + len) + cs * half), new(cs * (r0 + len) + sn * half, sn * (r0 + len) - cs * half), new(cs * r0 + sn * half, sn * r0 - cs * half)], Ramp(k / (float)n), .95f, 10f, 16f);
        }
        c.Ring(0, 0, r0 - 20f, 6f, s.Pal.C, .9f, 10f);
    }

    // ---------------------------------------------------------------- v127 で足した背景
    void RunV127()
    {
        int j = (int)s.Scene - (int)PvScene.TlSpinCircle;
        if (j < 12) { TileSpin(j); return; }
        if (j < 24) { TileFall(j - 12); return; }
        switch (s.Scene)
        {
            case PvScene.PtHoney: Honey(); break;
            case PvScene.PtMosaic: Mosaic(); break;
            case PvScene.PtArgyle: Argyle(); break;
            case PvScene.PtDiagDots: DiagDots(); break;
            case PvScene.PtWaveGrid: WaveGrid(); break;
            case PvScene.PtSquareZoom: SquareZoom(false); break;
            case PvScene.PtPinwheel: Pinwheel(); break;
            case PvScene.PtSunCircles: SunCircles(); break;
            case PvScene.PtPixels: Pixels(); break;
            case PvScene.PtWarp: Warp(); break;
            case PvScene.PtBubbleCols: BubbleCols(); break;
            case PvScene.PtStreamers: Streamers(); break;
            case PvScene.PtCheckTunnel: SquareZoom(true); break;
            case PvScene.PtPolar: Polar(); break;
            case PvScene.PtWaveDots: WaveDots(); break;
            case PvScene.PtSoftBlobs: SoftBlobs(); break;
            case PvScene.PtCrossHatch: CrossHatch(); break;
            default: SpiralLines(); break;
        }
    }

    /// <summary>回る図形: 図形が何重もの輪になって、輪ごとに反対へ回る</summary>
    void TileSpin(int kind)
    {
        for (int l = 1; l <= 5; l++)
        {
            float rad = (150f + l * 140f) * s.Spread, rot = t * (l % 2 == 0 ? .25f : -.25f), size = (46f - l * 4f) * s.Size * (1f + s.Flash * .2f); int cnt = 6 + l * 5;
            for (int k = 0; k < cnt; k++)
            {
                float a = rot + k * MathF.Tau / cnt, x = MathF.Cos(a) * rad, y = MathF.Sin(a) * rad;
                Shape(kind, x, y, size, a + MathF.PI / 2f, Ramp((l - 1) / 4f), .92f * Calm(x, y), l * 15f, 14f);
            }
        }
    }

    /// <summary>降る図形: ひらひら回りながら上から降る</summary>
    void TileFall(int kind)
    {
        int n = Math.Clamp(s.N, 10, 160); float h = ey * 2f + 200f;
        for (int i = 0; i < n; i++)
        {
            float z = 60f - R(i, 6) * 700f; var dep = c.Depth(z);
            float y = -ey - 100f + Mod(R(i, 1) * h + (mv * .5f + t * 80f) * (.5f + R(i, 2)), h), x = (R(i, 0) - .5f) * ex * 2.2f + MathF.Sin(t * .9f + i) * 40f;
            float r = (24f + R(i, 3) * 40f) * s.Size * dep.Y, rot = t * (1f + R(i, 4)) + i;
            Shape(kind, x * dep.X, y * dep.X, r, rot, Pick(i), .9f * Calm(x, y), z, r * .3f);
        }
    }

    void Honey()
    {
        float rr = MathF.Max(40f, 70f * s.Spread), w = rr * 1.732f, h = rr * 1.5f;
        int c0 = (int)MathF.Ceiling(ex / w) + 1, r0 = (int)MathF.Ceiling(ey / h) + 1;
        for (int r = -r0; r <= r0; r++)
            for (int q = -c0; q <= c0; q++)
            {
                float x = q * w + ((r & 1) != 0 ? w / 2f : 0f), y = r * h, d = MathF.Sqrt(x * x + y * y) / 200f, ph = Fr(beats * .5f - d * .15f);
                if (ph < .3f) c.Fill(Poly(x, y, rr * .9f, 6, MathF.PI / 6f), Ramp(1f - ph / .3f), .9f * Calm(x, y), 40f * (1f - ph / .3f), 12f);
                else Outline(Poly(x, y, rr * .9f, 6, MathF.PI / 6f), 3f, s.Pal.B, .6f, 0f);
            }
    }

    void Mosaic()
    {
        float g = MathF.Max(70f, 110f * s.Spread), h = g * .866f;
        int c0 = (int)MathF.Ceiling(ex / g) + 2, r0 = (int)MathF.Ceiling(ey / h) + 1;
        for (int r = -r0; r <= r0; r++)
            for (int q = -c0; q <= c0; q++)
            {
                float x = q * g + ((r & 1) != 0 ? g / 2f : 0f), y = r * h; int id = r * 131 + q * 7;
                c.Fill([new(x, y), new(x + g, y), new(x + g / 2f, y + h)], Pick(id + st * 7), .95f, 0f, 8f);
                c.Fill([new(x + g, y), new(x + g * 1.5f, y + h), new(x + g / 2f, y + h)], Pick(id + 3 + st * 11), .95f, -20f, 8f);
            }
    }

    void Argyle()
    {
        float w = MathF.Max(90f, 160f * s.Spread), h = w * 1.4f, oy = Mod(mv * .5f, h);
        int c0 = (int)MathF.Ceiling(ex / w) + 1, r0 = (int)MathF.Ceiling(ey / h) + 2;
        for (int r = -r0; r <= r0; r++)
            for (int q = -c0; q <= c0; q++)
            {
                float x = q * w, y = r * h + oy + ((q & 1) != 0 ? h / 2f : 0f);
                c.Fill([new(x, y - h / 2f), new(x + w / 2f, y), new(x, y + h / 2f), new(x - w / 2f, y)], ((q + r) & 1) == 0 ? s.Pal.A : s.Pal.B, .95f, 0f, 8f);
            }
        float dash = 18f, run = Mod(t * 30f, dash * 2f);
        for (int q = -c0 * 2; q <= c0 * 2; q++)
        {
            float x0 = q * w;
            foreach (float sg in new[] { 1f, -1f })
                for (float k = -ey - 200f + run; k < ey + 200f; k += dash * 2f)
                {
                    float ya = k, yb = k + dash, xa = x0 + sg * ya * w / h, xb = x0 + sg * yb * w / h;
                    c.Line(xa, ya, xb, yb, 3f, s.Pal.C, .9f, 12f);
                }
        }
    }

    void DiagDots()
    {
        float g = MathF.Max(40f, 60f * s.Spread), cs = MathF.Cos(-.5f), sn = MathF.Sin(-.5f), m = MathF.Max(ex, ey) * 1.5f, run = Mod(t * 50f + mv, g);
        for (float b = -m; b <= m; b += g)
            for (float a = -m; a <= m; a += g)
            {
                float lx = a + run, ly = b, x = lx * cs - ly * sn, y = lx * sn + ly * cs;
                if (MathF.Abs(x) > ex + g || MathF.Abs(y) > ey + g) continue;
                float p = .5f + .5f * MathF.Sin(b / 90f + t);
                c.Dot(x, y, g * (.15f + .25f * p), Ramp(p), .9f * Calm(x, y), p * 30f);
            }
    }

    void WaveGrid()
    {
        float g = MathF.Max(60f, 110f * s.Spread), stp = 40f;
        for (float x0 = -ex - g; x0 <= ex + g; x0 += g)
            for (float y = -ey - 40f; y < ey + 40f; y += stp)
                c.Line(x0 + MathF.Sin(y / 150f + t) * 22f, y, x0 + MathF.Sin((y + stp) / 150f + t) * 22f, y + stp, 3f, s.Pal.A, .9f, 0f);
        for (float y0 = -ey - g; y0 <= ey + g; y0 += g)
            for (float x = -ex - 40f; x < ex + 40f; x += stp)
                c.Line(x, y0 + MathF.Sin(x / 170f - t * .8f) * 22f, x + stp, y0 + MathF.Sin((x + stp) / 170f - t * .8f) * 22f, 3f, s.Pal.B, .9f, 10f);
    }

    /// <summary>広がる四角(checker=true で、輪ごとに市松に分けたトンネル)</summary>
    void SquareZoom(bool checker)
    {
        const int n = 12;
        var order = Enumerable.Range(0, n).Select(j => (j, f: Fr(j / (float)n + t * .08f + mv * .0004f))).OrderByDescending(v => v.f).ToList();
        foreach (var (j, f) in order)
        {
            float half = 1300f * f * f + 20f, inner = half * .82f, z = (1f - f) * 100f; var col = j % 2 == 0 ? s.Pal.A : s.Pal.B;
            if (!checker) { c.Fill([new(-half, -half), new(half, -half), new(half, half), new(-half, half)], col, .95f, z, 6f); continue; }
            for (int side = 0; side < 4; side++)
                for (int k = 0; k < 4; k++)
                {
                    float a0 = -half + k * half / 2f, a1 = a0 + half / 2f, b0 = -inner + k * inner / 2f, b1 = b0 + inner / 2f;
                    List<Vector2> q = side switch
                    {
                        0 => [new(a0, -half), new(a1, -half), new(b1, -inner), new(b0, -inner)],
                        1 => [new(half, a0), new(half, a1), new(inner, b1), new(inner, b0)],
                        2 => [new(a0, half), new(a1, half), new(b1, inner), new(b0, inner)],
                        _ => [new(-half, a0), new(-half, a1), new(-inner, b1), new(-inner, b0)],
                    };
                    c.Fill(q, ((k + side + j + st) & 1) == 0 ? s.Pal.A : s.Pal.C, .95f, z, 6f);
                }
        }
    }

    void Pinwheel()
    {
        for (int arm = 0; arm < 5; arm++)
            for (int k = 1; k < 44; k++)
            {
                float r = k * 22f * s.Spread, a = arm * MathF.Tau / 5f + r * .006f + t * .3f, x = MathF.Cos(a) * r, y = MathF.Sin(a) * r;
                c.Dot(x, y, (4f + k * .35f) * s.Size, Ramp(k / 44f), .9f * Calm(x, y), k * 2f);
            }
    }

    void SunCircles()
    {
        for (int k = 0; k < 36; k++)
        {
            float a = k * MathF.Tau / 36f + t * .1f, cs = MathF.Cos(a), sn = MathF.Sin(a);
            c.Line(cs * 200f, sn * 200f, cs * 1500f, sn * 1500f, k % 2 == 0 ? 10f : 3f, Ramp(k / 36f), .8f, -20f);
        }
        float fb = Fr(beats);
        for (int l = 0; l < 6; l++) c.Ring(0, 0, (200f + l * 120f + fb * 40f) * s.Spread, l == st % 6 ? 12f : 4f, s.Pal.C, .9f, l * 10f);
    }

    void Pixels()
    {
        float g = MathF.Max(50f, 80f * s.Spread);
        int c0 = (int)MathF.Ceiling(ex / g), r0 = (int)MathF.Ceiling(ey / g);
        for (int r = -r0; r < r0; r++)
            for (int q = -c0; q < c0; q++)
            {
                int id = r * 977 + q * 31; if (R(id + st * 977, 2) < .55f) continue;
                float x = q * g, y = r * g, z = R(id + st, 3) * 40f;
                c.Fill([new(x + 2f, y + 2f), new(x + g - 2f, y + 2f), new(x + g - 2f, y + g - 2f), new(x + 2f, y + g - 2f)], Pick(id + st), .9f * Calm(x + g / 2f, y + g / 2f), z, 10f);
            }
    }

    void Warp()
    {
        int n = Math.Clamp(s.N * 2, 60, 300);
        for (int i = 0; i < n; i++)
        {
            float a = R(i, 0) * MathF.Tau, f = Fr(R(i, 1) + s.T * .5f * sp), d = f * f * 1300f + 30f, len = d * .25f, cs = MathF.Cos(a), sn = MathF.Sin(a);
            c.Line(cs * d, sn * d, cs * (d + len), sn * (d + len), 1f + d / 300f, Vector3.Lerp(Pick(i), Vector3.One, .4f), MathF.Min(1f, d / 300f), -200f + f * 200f);
        }
    }

    void BubbleCols()
    {
        float h = ey * 2f + 100f;
        for (int col = 0; col < 9; col++)
        {
            float x0 = (col - 4) * ex / 4.5f;
            for (int k = 0; k < 10; k++)
            {
                int id = col * 31 + k; float y = ey + 50f - Mod(R(id, 0) * h + t * (90f + R(col, 1) * 60f), h), x = x0 + MathF.Sin(t * 2f + id) * 16f, r = (8f + R(id, 2) * 24f) * s.Size;
                c.Ring(x, y, r, MathF.Max(2f, r * .15f), Pick(col), .85f * Calm(x, y), col % 2 * 20f);
                c.Dot(x - r * .35f, y - r * .35f, r * .18f, Vector3.One, .8f, col % 2 * 20f + 4f);
            }
        }
    }

    void Streamers()
    {
        float h = ey * 2f + 500f;
        for (int i = 0; i < 26; i++)
        {
            float x0 = (R(i, 0) - .5f) * ex * 2f, head = Mod(R(i, 1) * h + t * (90f + R(i, 2) * 60f), h) - ey - 400f;
            for (int k = 0; k < 16; k++)
            {
                float y0 = head + k * 20f, y1 = y0 + 20f, xa = x0 + MathF.Sin(k * .4f + t * 3f + i) * 30f, xb = x0 + MathF.Sin((k + 1) * .4f + t * 3f + i) * 30f;
                c.Line(xa, y0, xb, y1, 10f * s.Size, Pick(i), .92f, (i % 3) * 15f);
            }
        }
    }

    void Polar()
    {
        for (int l = 1; l <= 8; l++) c.Ring(0, 0, (60f + l * 115f) * s.Spread, 2f, s.Pal.A, .8f, 0f);
        float rot = t * .1f;
        for (int k = 0; k < 24; k++) { float a = rot + k * MathF.Tau / 24f, cs = MathF.Cos(a), sn = MathF.Sin(a); c.Line(cs * 120f, sn * 120f, cs * 1100f, sn * 1100f, 2f, s.Pal.B, .8f, 4f); }
        float a0 = rot + (st % 24) * MathF.Tau / 24f, a1 = a0 + MathF.Tau / 24f; var w = new List<Vector2>();
        for (int j = 0; j <= 6; j++) { float a = a0 + (a1 - a0) * j / 6f; w.Add(new(MathF.Cos(a) * 1100f, MathF.Sin(a) * 1100f)); }
        for (int j = 6; j >= 0; j--) { float a = a0 + (a1 - a0) * j / 6f; w.Add(new(MathF.Cos(a) * 120f, MathF.Sin(a) * 120f)); }
        c.Fill(w, s.Pal.C, .45f, 8f, 6f);
    }

    void WaveDots()
    {
        float gx = MathF.Max(36f, 50f * s.Spread), gy = 90f;
        for (float y0 = -ey; y0 <= ey; y0 += gy)
            for (float x = -ex; x <= ex; x += gx)
            {
                float w = MathF.Sin(x / 180f + t * 2f + y0 * .005f), y = y0 + w * 30f;
                c.Dot(x, y, (6f + 4f * w) * s.Size, Ramp(.5f + .5f * w), .9f * Calm(x, y), 20f + w * 20f);
            }
    }

    void SoftBlobs()
    {
        for (int i = 0; i < 14; i++)
        {
            float x = (Fr(R(i, 0) + t * .01f * (R(i, 3) - .5f) + mv * .0001f) - .5f) * ex * 2.2f, y = (R(i, 1) - .5f) * ey * 2f + MathF.Sin(t * .4f + i) * 60f, r = (120f + R(i, 2) * 180f) * s.Size;
            c.Dot(x, y, r, Pick(i), .28f, -i * 20f);
        }
    }

    void CrossHatch()
    {
        float g = MathF.Max(30f, 50f * s.Spread), m = ex + ey, o1 = Mod(t * 40f + mv, g), o2 = Mod(-t * 40f, g);
        for (float k = -m; k <= m; k += g)
        {
            c.Line(k + o1 - ey, -ey - 20f, k + o1 + ey, ey + 20f, 4f, s.Pal.A, .8f, 0f);
            c.Line(k + o2 + ey, -ey - 20f, k + o2 - ey, ey + 20f, 4f, s.Pal.B, .8f, 15f);
        }
    }

    void SpiralLines()
    {
        for (int arm = 0; arm < 6; arm++)
        {
            float px = 0f, py = 0f;
            for (float r = 30f; r < 1300f; r += 24f)
            {
                float a = arm * MathF.Tau / 6f + r * .004f + t * .4f, x = MathF.Cos(a) * r, y = MathF.Sin(a) * r;
                if (r > 30f) c.Line(px, py, x, y, 6f * s.Size, Ramp(arm / 5f), .9f, arm * 8f);
                px = x; py = y;
            }
        }
    }
}
