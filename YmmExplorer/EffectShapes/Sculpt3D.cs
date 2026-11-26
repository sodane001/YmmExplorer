using System.Collections.Concurrent;
using System.Numerics;
using Vortice.Direct2D1;
using WpfColor = System.Windows.Media.Color;

namespace YMM4EffectShapes;

/// <summary>
/// 図形ごとの専用3Dモデルを組み立てる道具。座標は2Dの絵(ArtPen)と同じ「絵の単位」:
/// x=右、y=下、z=手前(見る人の方)。原点は図形の位置(PosX・PosY)。1単位 = 1920×1080 基準の 1px × 大きさ
/// 立体はすべて「超楕円体」(球・角の丸い箱・円柱を1つの式で作れる)と、筒・回転体・押し出しで作る
/// </summary>
internal sealed class Sculpt
{
    readonly Native3D n;
    readonly float k, ox, oy, cw, ch;
    Matrix4x4 xf = Matrix4x4.Identity;
    /// <summary>出入りと不透明度を合わせた濃さ</summary>
    public readonly float Al;
    /// <summary>画面の幅・高さ(絵の単位)</summary>
    public readonly float W, H;
    /// <summary>なめらかさ(円の分割数)</summary>
    public int Seg = 28;
    /// <summary>図形が出てからの時間(秒、速さはかからない)</summary>
    public float Now;

    public Sculpt(Native3D n, float ox, float oy, float k, float alpha, float cw, float ch)
    {
        this.n = n; this.ox = ox; this.oy = oy; this.k = MathF.Max(k, 1e-4f); this.cw = cw; this.ch = ch;
        Al = alpha; W = cw / this.k; H = ch / this.k;
    }

    // ------------------------------------------------------------ 色
    public Vector4 C(uint rgb, float a = 1f) => new(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, Math.Clamp(a * Al, 0f, 1f));
    public Vector4 C(int rgb, float a = 1f) => C((uint)rgb, a);
    public Vector4 C(WpfColor c, float a = 1f) => new(c.R / 255f, c.G / 255f, c.B / 255f, Math.Clamp(c.A / 255f * a * Al, 0f, 1f));
    /// <summary>色の濃さだけ変える(元の色の濃さにかける)</summary>
    public Vector4 C(Vector4 c, float a) => new(c.X, c.Y, c.Z, Math.Clamp(c.W * a, 0f, 1f));
    public static Vector4 Mix(Vector4 a, Vector4 b, float t) => Vector4.Lerp(a, b, t);
    public static Vector4 Dark(Vector4 c, float f) => new(c.X * f, c.Y * f, c.Z * f, c.W);

    // ------------------------------------------------------------ 動かす(絵の単位で、中の物だけにかかる)
    void With(Matrix4x4 m, Action draw) { var prev = xf; xf = m * xf; try { draw(); } finally { xf = prev; } }
    /// <summary>画面の上で (x,y) を中心に回す(2Dの Turn と同じ向き)</summary>
    public void Turn(float x, float y, float ang, Action draw) => With(Matrix4x4.CreateTranslation(-x, -y, 0) * Matrix4x4.CreateRotationZ(ang) * Matrix4x4.CreateTranslation(x, y, 0), draw);
    /// <summary>縦の軸で回す(プラスで右側が奥へ)</summary>
    public void Yaw(float x, float y, float z, float ang, Action draw) => With(Matrix4x4.CreateTranslation(-x, -y, -z) * Matrix4x4.CreateRotationY(ang) * Matrix4x4.CreateTranslation(x, y, z), draw);
    /// <summary>横の軸で回す(プラスで上側が手前へ)</summary>
    public void Pitch(float x, float y, float z, float ang, Action draw) => With(Matrix4x4.CreateTranslation(-x, -y, -z) * Matrix4x4.CreateRotationX(ang) * Matrix4x4.CreateTranslation(x, y, z), draw);
    public void Move(float x, float y, float z, Action draw) => With(Matrix4x4.CreateTranslation(x, y, z), draw);
    public void Move(float x, float y, Action draw) => Move(x, y, 0, draw);
    public void Zoom(float x, float y, float sx, float sy, Action draw) => With(Matrix4x4.CreateTranslation(-x, -y, 0) * Matrix4x4.CreateScale(sx, sy, (sx + sy) / 2f) * Matrix4x4.CreateTranslation(x, y, 0), draw);
    public void Scale(float x, float y, float z, float s, Action draw) => With(Matrix4x4.CreateTranslation(-x, -y, -z) * Matrix4x4.CreateScale(s) * Matrix4x4.CreateTranslation(x, y, z), draw);

    Vector3 Pw(Vector3 a)
    {
        var p = Vector3.Transform(a, xf);
        return new(ox + p.X * k - cw / 2f, ch / 2f - (oy + p.Y * k), -p.Z * k);
    }
    Vector3 Nw(Vector3 v)
    {
        var q = Vector3.TransformNormal(v, xf);
        var r = new Vector3(q.X, -q.Y, -q.Z);
        float l = r.Length();
        return l > 1e-8f ? r / l : Vector3.UnitZ;
    }

    // ------------------------------------------------------------ 形
    static float Sp(float c, float e) => MathF.Sign(c) * MathF.Pow(MathF.Abs(c), e);

    /// <summary>
    /// 超楕円体。e1=上下の角ばり、e2=まわりの角ばり(1=丸、0.1〜0.3=角の丸い箱、e1小+e2=1=円柱)
    /// </summary>
    public void Super(float x, float y, float z, float rx, float ry, float rz, float e1, float e2, Vector4 col)
    {
        if (rx <= 0f || ry <= 0f || rz <= 0f || col.W <= 0.002f) return;
        int nu = Seg, nv = Math.Max(8, Seg / 2 + 2);
        var pos = new Vector3[(nu + 1) * (nv + 1)];
        var nrm = new Vector3[pos.Length];
        for (int j = 0; j <= nv; j++)
        {
            float v = -MathF.PI / 2f + MathF.PI * j / nv;          // 上(-y)から下へ
            float cv = MathF.Cos(v), sv = MathF.Sin(v);
            for (int i = 0; i <= nu; i++)
            {
                float u = -MathF.PI + MathF.Tau * i / nu;
                float cu = MathF.Cos(u), su = MathF.Sin(u);
                var p = new Vector3(x + rx * Sp(cv, e1) * Sp(cu, e2), y + ry * Sp(sv, e1), z + rz * Sp(cv, e1) * Sp(su, e2));
                var nn = new Vector3(Sp(cv, 2f - e1) * Sp(cu, 2f - e2) / rx, Sp(sv, 2f - e1) / ry, Sp(cv, 2f - e1) * Sp(su, 2f - e2) / rz);
                if (nn.LengthSquared() < 1e-12f) nn = new Vector3(0, MathF.Sign(sv), 0);
                pos[j * (nu + 1) + i] = Pw(p);
                nrm[j * (nu + 1) + i] = Nw(nn);
            }
        }
        n.Begin();
        for (int j = 0; j < nv; j++)
            for (int i = 0; i < nu; i++)
            {
                int a = j * (nu + 1) + i, b = a + 1, c = a + nu + 1, d = c + 1;
                n.TriN(pos[a], nrm[a], pos[c], nrm[c], pos[b], nrm[b], col);
                n.TriN(pos[b], nrm[b], pos[c], nrm[c], pos[d], nrm[d], col);
            }
        n.End();
    }

    /// <summary>楕円体(球)</summary>
    public void Ball(float x, float y, float z, float rx, float ry, float rz, Vector4 col) => Super(x, y, z, rx, ry, rz, 1f, 1f, col);
    public void Ball(float x, float y, float z, float r, Vector4 col) => Super(x, y, z, r, r, r, 1f, 1f, col);
    /// <summary>角の丸い箱(幅・高さ・奥行きは全体の大きさ)</summary>
    public void Box(float x, float y, float z, float w, float h, float d, Vector4 col, float round = .22f) => Super(x, y, z, w / 2f, h / 2f, d / 2f, round, round, col);
    /// <summary>縦向きの円柱(角の丸み付き)</summary>
    public void Cyl(float x, float y, float z, float r, float h, Vector4 col, float round = .18f) => Super(x, y, z, r, h / 2f, r, round, 1f, col);
    /// <summary>奥行き方向に向いた円柱(コイン・皿のように正面を向く)</summary>
    public void Disc(float x, float y, float z, float rx, float ry, float d, Vector4 col, float round = .2f) =>
        Pitch(x, y, z, MathF.PI / 2f, () => Super(x, y, z, rx, d / 2f, ry, round, 1f, col));

    /// <summary>2点を結ぶ、太さの変わる棒(両はしは丸い)</summary>
    public void Limb(float x0, float y0, float z0, float x1, float y1, float z1, float r0, float r1, Vector4 col)
    {
        var a = new Vector3(x0, y0, z0); var b = new Vector3(x1, y1, z1);
        var d = b - a; float len = d.Length();
        if (len < 1e-4f) { Ball(x0, y0, z0, MathF.Max(r0, r1), col); return; }
        if (col.W <= 0.002f) return;
        d /= len;
        var side = MathF.Abs(d.Y) < .9f ? Vector3.UnitY : Vector3.UnitX;
        var u = Vector3.Normalize(Vector3.Cross(d, side)); var v = Vector3.Cross(d, u);
        int nu = Math.Max(10, Seg * 2 / 3), capN = Math.Max(4, Seg / 6);
        // 輪の並び: 始まりの半球 → 胴 → 終わりの半球
        var rings = new List<(Vector3 C, float R, float Nd)>();
        for (int i = capN; i >= 1; i--) { float t = MathF.PI / 2f * i / capN; rings.Add((a - d * r0 * MathF.Sin(t), r0 * MathF.Cos(t), -MathF.Sin(t))); }
        rings.Add((a, r0, 0f));
        rings.Add((b, r1, 0f));
        for (int i = 1; i <= capN; i++) { float t = MathF.PI / 2f * i / capN; rings.Add((b + d * r1 * MathF.Sin(t), r1 * MathF.Cos(t), MathF.Sin(t))); }
        float slope = (r0 - r1) / len;
        var pos = new Vector3[rings.Count * (nu + 1)]; var nrm = new Vector3[pos.Length];
        for (int j = 0; j < rings.Count; j++)
        {
            var (c, r, nd) = rings[j];
            for (int i = 0; i <= nu; i++)
            {
                float ang = MathF.Tau * i / nu;
                var dir = u * MathF.Cos(ang) + v * MathF.Sin(ang);
                pos[j * (nu + 1) + i] = Pw(c + dir * r);
                var nn = nd != 0f ? dir * MathF.Sqrt(MathF.Max(0f, 1f - nd * nd)) + d * nd : Vector3.Normalize(dir + d * slope);
                nrm[j * (nu + 1) + i] = Nw(nn);
            }
        }
        n.Begin();
        for (int j = 0; j + 1 < rings.Count; j++)
            for (int i = 0; i < nu; i++)
            {
                int p0 = j * (nu + 1) + i, p1 = p0 + 1, p2 = p0 + nu + 1, p3 = p2 + 1;
                n.TriN(pos[p0], nrm[p0], pos[p2], nrm[p2], pos[p1], nrm[p1], col);
                n.TriN(pos[p1], nrm[p1], pos[p2], nrm[p2], pos[p3], nrm[p3], col);
            }
        n.End();
    }
    public void Limb(Vector3 a, Vector3 b, float r0, float r1, Vector4 col) => Limb(a.X, a.Y, a.Z, b.X, b.Y, b.Z, r0, r1, col);

    /// <summary>点をつないだ管(線の代わり)</summary>
    public void Tube(IReadOnlyList<Vector3> pts, float r, Vector4 col)
    {
        int keep = Seg; Seg = Math.Max(10, Seg / 2);
        for (int i = 0; i + 1 < pts.Count; i++) Limb(pts[i], pts[i + 1], r, r, col);
        Seg = keep;
    }

    /// <summary>
    /// 回転体(つぼ・カップ・コーンなど)。prof は (半径, y) の並び(上から下へ)。縦の軸 (x,z) のまわりに回す
    /// </summary>
    public void Lathe(float x, float z, IReadOnlyList<Vector2> prof, Vector4 col) => Lathe(x, z, prof, col, 0f, MathF.Tau);
    public void Lathe(float x, float z, Vector4 col, IReadOnlyList<Vector2> prof, float a0 = 0f, float a1 = MathF.Tau) => Lathe(x, z, prof, col, a0, a1);

    /// <summary>回転体の一部(a0〜a1 の角度だけ。気球の色分けなど)。角度0 = 右、π/2 = 手前</summary>
    public void Lathe(float x, float z, IReadOnlyList<Vector2> prof, Vector4 col, float a0, float a1)
    {
        if (prof.Count < 2 || col.W <= 0.002f) return;
        int nu = Math.Max(2, (int)MathF.Ceiling(Seg * MathF.Abs(a1 - a0) / MathF.Tau));
        var pos = new Vector3[prof.Count * (nu + 1)]; var nrm = new Vector3[pos.Length];
        for (int j = 0; j < prof.Count; j++)
        {
            var pPrev = prof[Math.Max(0, j - 1)]; var pNext = prof[Math.Min(prof.Count - 1, j + 1)];
            var t = pNext - pPrev;                                    // 断面の向き(r, y)
            var n2 = new Vector2(t.Y, -t.X);                          // 外向き(右手で回した法線)
            if (n2.LengthSquared() < 1e-10f) n2 = new Vector2(1, 0);
            n2 = Vector2.Normalize(n2);
            if (n2.X < 0f && prof[j].X > 0f) n2 = -n2;
            for (int i = 0; i <= nu; i++)
            {
                float a = a0 + (a1 - a0) * i / nu, ca = MathF.Cos(a), sa = MathF.Sin(a);
                pos[j * (nu + 1) + i] = Pw(new Vector3(x + prof[j].X * ca, prof[j].Y, z + prof[j].X * sa));
                nrm[j * (nu + 1) + i] = Nw(new Vector3(n2.X * ca, n2.Y, n2.X * sa));
            }
        }
        n.Begin();
        for (int j = 0; j + 1 < prof.Count; j++)
            for (int i = 0; i < nu; i++)
            {
                int p0 = j * (nu + 1) + i, p1 = p0 + 1, p2 = p0 + nu + 1, p3 = p2 + 1;
                n.TriN(pos[p0], nrm[p0], pos[p2], nrm[p2], pos[p1], nrm[p1], col);
                n.TriN(pos[p1], nrm[p1], pos[p2], nrm[p2], pos[p3], nrm[p3], col);
            }
        n.End();
    }
    public void Lathe(float x, float z, Vector4 col, params float[] ry)
    {
        var l = new List<Vector2>(ry.Length / 2);
        for (int i = 0; i + 1 < ry.Length; i += 2) l.Add(new Vector2(ry[i], ry[i + 1]));
        Lathe(x, z, l, col);
    }

    /// <summary>輪(ドーナツ形)。正面を向く。R=輪の半径、r=太さ</summary>
    public void Torus(float x, float y, float z, float R, float r, Vector4 col)
    {
        if (col.W <= 0.002f) return;
        int nu = Seg, nv = Math.Max(8, Seg / 2);
        var pos = new Vector3[(nu + 1) * (nv + 1)]; var nrm = new Vector3[pos.Length];
        for (int j = 0; j <= nv; j++)
        {
            float b = MathF.Tau * j / nv;
            for (int i = 0; i <= nu; i++)
            {
                float a = MathF.Tau * i / nu;
                var dir = new Vector3(MathF.Cos(a), MathF.Sin(a), 0);
                var nn = dir * MathF.Cos(b) + new Vector3(0, 0, MathF.Sin(b));
                pos[j * (nu + 1) + i] = Pw(new Vector3(x, y, z) + dir * R + nn * r);
                nrm[j * (nu + 1) + i] = Nw(nn);
            }
        }
        n.Begin();
        for (int j = 0; j < nv; j++)
            for (int i = 0; i < nu; i++)
            {
                int p0 = j * (nu + 1) + i, p1 = p0 + 1, p2 = p0 + nu + 1, p3 = p2 + 1;
                n.TriN(pos[p0], nrm[p0], pos[p2], nrm[p2], pos[p1], nrm[p1], col);
                n.TriN(pos[p1], nrm[p1], pos[p2], nrm[p2], pos[p3], nrm[p3], col);
            }
        n.End();
    }

    /// <summary>平らな形に厚みを付けた板(星・ハート・文字の板など)。pts は絵の座標、z=まん中、d=厚さ</summary>
    public void Slab(IReadOnlyList<Vector2> pts, float z, float d, Vector4 col)
    {
        if (pts.Count < 3 || col.W <= 0.002f) return;
        var tris = Ear(pts);
        float z0 = z + d / 2f, z1 = z - d / 2f;
        n.Begin();
        var nf = Nw(new Vector3(0, 0, 1)); var nb = Nw(new Vector3(0, 0, -1));
        for (int i = 0; i + 2 < tris.Count; i += 3)
        {
            Vector3 P(int q, float zz) => Pw(new Vector3(pts[tris[q]].X, pts[tris[q]].Y, zz));
            n.TriN(P(i, z0), nf, P(i + 1, z0), nf, P(i + 2, z0), nf, col);
            n.TriN(P(i, z1), nb, P(i + 2, z1), nb, P(i + 1, z1), nb, col);
        }
        float area = 0f;
        for (int i = 0; i < pts.Count; i++) { var a = pts[i]; var b = pts[(i + 1) % pts.Count]; area += a.X * b.Y - b.X * a.Y; }
        float sgn = area >= 0f ? 1f : -1f;
        for (int i = 0; i < pts.Count; i++)
        {
            var a = pts[i]; var b = pts[(i + 1) % pts.Count];
            var e = b - a; var nn = Nw(new Vector3(e.Y * sgn, -e.X * sgn, 0));
            var a0 = Pw(new Vector3(a, z0)); var b0 = Pw(new Vector3(b, z0)); var a1 = Pw(new Vector3(a, z1)); var b1 = Pw(new Vector3(b, z1));
            n.TriN(a0, nn, b0, nn, b1, nn, col); n.TriN(a0, nn, b1, nn, a1, nn, col);
        }
        n.End();
    }
    public void Slab(float z, float d, Vector4 col, params float[] xy)
    {
        var l = new List<Vector2>(xy.Length / 2);
        for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(new Vector2(xy[i], xy[i + 1]));
        Slab(l, z, d, col);
    }

    /// <summary>多角形を三角形に分ける(へこんだ形も可)</summary>
    static List<int> Ear(IReadOnlyList<Vector2> p)
    {
        var idx = Enumerable.Range(0, p.Count).ToList();
        var res = new List<int>();
        float area = 0f;
        for (int i = 0; i < p.Count; i++) { var a = p[i]; var b = p[(i + 1) % p.Count]; area += a.X * b.Y - b.X * a.Y; }
        bool ccw = area > 0f;
        int guard = 0;
        while (idx.Count > 3 && guard++ < 10000)
        {
            bool cut = false;
            for (int i = 0; i < idx.Count; i++)
            {
                int ia = idx[(i + idx.Count - 1) % idx.Count], ib = idx[i], ic = idx[(i + 1) % idx.Count];
                var a = p[ia]; var b = p[ib]; var c = p[ic];
                float cr = (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
                if (ccw ? cr <= 0f : cr >= 0f) continue;
                bool inside = false;
                foreach (int q in idx)
                {
                    if (q == ia || q == ib || q == ic) continue;
                    if (InTri(p[q], a, b, c)) { inside = true; break; }
                }
                if (inside) continue;
                res.Add(ia); res.Add(ib); res.Add(ic); idx.RemoveAt(i); cut = true; break;
            }
            if (!cut) break;
        }
        if (idx.Count == 3) { res.Add(idx[0]); res.Add(idx[1]); res.Add(idx[2]); }
        else for (int i = 1; i + 1 < idx.Count; i++) { res.Add(idx[0]); res.Add(idx[i]); res.Add(idx[i + 1]); }
        return res;
    }

    static bool InTri(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = (p.X - b.X) * (a.Y - b.Y) - (a.X - b.X) * (p.Y - b.Y);
        float d2 = (p.X - c.X) * (b.Y - c.Y) - (b.X - c.X) * (p.Y - c.Y);
        float d3 = (p.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (p.Y - a.Y);
        bool neg = d1 < 0 || d2 < 0 || d3 < 0, pos = d1 > 0 || d2 > 0 || d3 > 0;
        return !(neg && pos);
    }

    /// <summary>平らな三角形(折り紙・紙飛行機など)。裏表どちらからも見える</summary>
    public void Tri3D(Vector3 a, Vector3 b, Vector3 c, Vector4 col)
    {
        if (col.W <= 0.002f) return;
        var nn = Vector3.Cross(b - a, c - a);
        if (nn.LengthSquared() < 1e-10f) return;
        var nw = Nw(Vector3.Normalize(nn));
        n.TriN(Pw(a), nw, Pw(b), nw, Pw(c), nw, col);
    }
    public void Quad3D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector4 col) { Tri3D(a, b, c, col); Tri3D(a, c, d, col); }

    /// <summary>上下で色の変わる、うしろの背景の板(空など)。z は奥行き、scale は画面の何倍の大きさか</summary>
    public void Backdrop(Vector4 top, Vector4 bottom, float z, float scale = 1.8f)
    {
        float w = W * scale / 2f, h = H * scale / 2f;
        n.Begin();
        n.Quad(Pw(new(-w, -h, z)), Pw(new(w, -h, z)), Pw(new(w, h, z)), Pw(new(-w, h, z)), top, bottom);
        n.End();
    }

    /// <summary>3Dの矢印(棒と円すいの先)</summary>
    public void Arrow(Vector3 a, Vector3 b, float r, Vector4 col)
    {
        var d = b - a; float l = d.Length(); if (l < 1f) return; d /= l;
        float h = MathF.Min(r * 5f, l * .45f);
        Limb(a, b - d * h, r, r, col);
        Limb(b - d * h, b, r * 2.6f, .5f, col);
    }

    // ------------------------------------------------------------ 文字
    /// <summary>文字のフォント(図形の「フォント」)</summary>
    public string Font = TextKit.DefaultFont;

    /// <summary>
    /// 厚みのある文字。(x,y) が文字のまん中、z が厚みのまん中、d が厚さ。
    /// edge と ew を指定すると、ふち取り(少し後ろに太い文字)も付ける
    /// </summary>
    public void Text(string? s, float size, float x, float y, float z, float d, Vector4 col, float maxW = 0f, Vector4 edge = default, float ew = 0f)
    {
        if (string.IsNullOrEmpty(s) || size <= .5f || col.W <= 0.002f) return;
        var tm = TextMesh3D.Get(s, size, Font, maxW, 0f);
        if (tm is null) return;
        float x0 = x - tm.W / 2f, y0 = y - tm.H / 2f;
        if (ew > 0.05f && edge.W > 0.002f && TextMesh3D.Get(s, size, Font, maxW, ew) is { } em)
            Extrude(em, x0, y0, z - d * .15f, d * 1.1f, edge);
        Extrude(tm, x0, y0, z, d, col);
    }

    void Extrude(TextMesh3D tm, float x0, float y0, float z, float d, Vector4 col)
    {
        float z0 = z + d / 2f, z1 = z - d / 2f;
        var nf = Nw(new Vector3(0, 0, 1)); var nb = Nw(new Vector3(0, 0, -1));
        n.Begin();
        var tr = tm.Tris;
        for (int i = 0; i + 2 < tr.Length; i += 3)
        {
            Vector3 P(int q, float zz) => Pw(new Vector3(x0 + tr[q].X, y0 + tr[q].Y, zz));
            n.TriN(P(i, z0), nf, P(i + 1, z0), nf, P(i + 2, z0), nf, col);
            n.TriN(P(i, z1), nb, P(i + 2, z1), nb, P(i + 1, z1), nb, col);
        }
        foreach (var c in tm.Outlines)
            for (int i = 0; i < c.Length; i++)
            {
                var a = c[i]; var b = c[(i + 1) % c.Length];
                var e = b - a; var nn = Nw(new Vector3(e.Y, -e.X, 0));
                var a0 = Pw(new Vector3(x0 + a.X, y0 + a.Y, z0)); var b0 = Pw(new Vector3(x0 + b.X, y0 + b.Y, z0));
                var a1 = Pw(new Vector3(x0 + a.X, y0 + a.Y, z1)); var b1 = Pw(new Vector3(x0 + b.X, y0 + b.Y, z1));
                n.TriN(a0, nn, b0, nn, b1, nn, col); n.TriN(a0, nn, b1, nn, a1, nn, col);
            }
        n.End();
    }

    // ------------------------------------------------------------ 表面に物を置くための計算
    /// <summary>楕円体 (cx,cy,cz,rx,ry,rz) の、正面側の表面の z(位置 x,y で)</summary>
    public static float FrontZ(float cx, float cy, float cz, float rx, float ry, float rz, float x, float y)
    {
        float dx = (x - cx) / rx, dy = (y - cy) / ry;
        return cz + rz * MathF.Sqrt(MathF.Max(0f, 1f - dx * dx - dy * dy));
    }

    /// <summary>楕円体の正面側の表面をなぞる点の列(口の線などを表面に沿わせる)</summary>
    public static List<Vector3> OnFront(float cx, float cy, float cz, float rx, float ry, float rz, float lift, IEnumerable<Vector2> xy) =>
        xy.Select(q => new Vector3(q.X, q.Y, FrontZ(cx, cy, cz, rx, ry, rz, q.X, q.Y) + lift)).ToList();

    public static IEnumerable<Vector2> ArcPts(float x, float y, float r, float a0, float a1, int steps = 12)
    {
        for (int i = 0; i <= steps; i++) { float a = a0 + (a1 - a0) * i / steps; yield return new Vector2(x + MathF.Cos(a) * r, y + MathF.Sin(a) * r); }
    }
}

/// <summary>専用3Dモデルの一覧(図形の設定の型 → 組み立て方)</summary>
internal static partial class Models3D
{
    internal delegate void QuickModel(QuickShapeParameter p, Sculpt m, float t, float v, float v2);

    static Dictionary<Type, QuickModel>? quick;

    public static bool TryGet(Type t, out QuickModel model)
    {
        quick ??= BuildQuick();
        return quick.TryGetValue(t, out model!);
    }

    public static IReadOnlyCollection<Type> Done => (quick ??= BuildQuick()).Keys;

    /// <summary>画面いっぱいに描く図形(位置・大きさをかけず、画面のまん中が原点)</summary>
    static readonly HashSet<Type> sceneTypes = [];
    public static bool IsScene(Type t) { quick ??= BuildQuick(); return sceneTypes.Contains(t); }

    /// <summary>2D版の図形のキー(例: YMM4EffectShapes.S881Plugin)に、専用の3Dモデルがあるか</summary>
    public static bool HasModelFor(string key2D)
    {
        if (!key2D.EndsWith("Plugin", StringComparison.Ordinal)) return false;
        var t = typeof(Models3D).Assembly.GetType(key2D[..^"Plugin".Length] + "Parameter");
        return t != null && ((quick ??= BuildQuick()).ContainsKey(t) || HasScene(t));
    }

    static Dictionary<Type, QuickModel> BuildQuick()
    {
        var d = new Dictionary<Type, QuickModel>();
        RegisterQ4a(d);
        RegisterQ5a(d);
        RegisterQ6a(d);
        RegisterI1a(d);
        RegisterI1b(d);
        RegisterI1c(d);
        RegisterI1d(d);
        RegisterI2a(d);
        RegisterI2b(d);
        RegisterI2c(d);
        RegisterI2d(d);
        RegisterI2e(d);
        RegisterI2f(d);
        RegisterF1a(d);
        RegisterF1b(d);
        RegisterF1c(d);
        RegisterF1d(d);
        RegisterF1e(d);
        RegisterF1f(d);
        RegisterF1g(d);
        RegisterF1h(d);
        RegisterF1i(d);
        RegisterF1j(d);
        RegisterF1k(d);
        RegisterF1l(d);
        RegisterF1m(d);
        RegisterG1a(d);
        RegisterG1b(d);
        RegisterG1c(d);
        RegisterG1d(d);
        RegisterG1e(d);
        RegisterG1f(d);
        RegisterG1g(d);
        RegisterG1h(d);
        RegisterH1a(d);
        RegisterH1b(d);
        RegisterH1c(d);
        RegisterH1d(d);
        RegisterJ1a(d);
        RegisterJ1b(d);
        RegisterJ1c(d);
        RegisterJ1d(d);
        RegisterJ1e(d);
        RegisterL1a(d);
        return d;
    }

    static partial void RegisterQ4a(Dictionary<Type, QuickModel> d);
    static partial void RegisterQ5a(Dictionary<Type, QuickModel> d);
    static partial void RegisterQ6a(Dictionary<Type, QuickModel> d);
    static partial void RegisterI1a(Dictionary<Type, QuickModel> d);
    static partial void RegisterI1b(Dictionary<Type, QuickModel> d);
    static partial void RegisterI1c(Dictionary<Type, QuickModel> d);
    static partial void RegisterI1d(Dictionary<Type, QuickModel> d);
    static partial void RegisterI2a(Dictionary<Type, QuickModel> d);
    static partial void RegisterI2b(Dictionary<Type, QuickModel> d);
    static partial void RegisterI2c(Dictionary<Type, QuickModel> d);
    static partial void RegisterI2d(Dictionary<Type, QuickModel> d);
    static partial void RegisterI2e(Dictionary<Type, QuickModel> d);
    static partial void RegisterI2f(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1a(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1b(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1c(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1d(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1e(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1f(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1g(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1h(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1i(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1j(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1k(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1l(Dictionary<Type, QuickModel> d);
    static partial void RegisterF1m(Dictionary<Type, QuickModel> d);
    static partial void RegisterG1a(Dictionary<Type, QuickModel> d);
    static partial void RegisterG1b(Dictionary<Type, QuickModel> d);
    static partial void RegisterG1c(Dictionary<Type, QuickModel> d);
    static partial void RegisterG1d(Dictionary<Type, QuickModel> d);
    static partial void RegisterG1e(Dictionary<Type, QuickModel> d);
    static partial void RegisterG1f(Dictionary<Type, QuickModel> d);
    static partial void RegisterG1g(Dictionary<Type, QuickModel> d);
    static partial void RegisterG1h(Dictionary<Type, QuickModel> d);
    static partial void RegisterH1a(Dictionary<Type, QuickModel> d);
    static partial void RegisterH1b(Dictionary<Type, QuickModel> d);
    static partial void RegisterH1c(Dictionary<Type, QuickModel> d);
    static partial void RegisterH1d(Dictionary<Type, QuickModel> d);
    static partial void RegisterJ1a(Dictionary<Type, QuickModel> d);
    static partial void RegisterJ1b(Dictionary<Type, QuickModel> d);
    static partial void RegisterJ1c(Dictionary<Type, QuickModel> d);
    static partial void RegisterJ1d(Dictionary<Type, QuickModel> d);
    static partial void RegisterJ1e(Dictionary<Type, QuickModel> d);
    static partial void RegisterL1a(Dictionary<Type, QuickModel> d);
}

/// <summary>文字の形を三角形と輪郭にしたもの(同じ文字・大きさは覚えておく)</summary>
internal sealed class TextMesh3D
{
    public Vector2[] Tris = [];
    public List<Vector2[]> Outlines = [];
    public float W, H;

    static readonly ConcurrentDictionary<(string, float, string, float, float), TextMesh3D?> cache = new();
    static ID2D1Factory? factory;
    static readonly object gate = new();

    public static TextMesh3D? Get(string s, float size, string font, float maxW, float edge)
    {
        if (cache.Count > 4000) cache.Clear();
        return cache.GetOrAdd((s, MathF.Round(size, 1), font, maxW, MathF.Round(edge, 1)), k => Build(k.Item1, k.Item2, k.Item3, k.Item4, k.Item5));
    }

    static TextMesh3D? Build(string s, float size, string font, float maxW, float edge)
    {
        try
        {
            var shape = TextKit.Get(s, size, true, font, maxW, 1);
            if (shape.Contours.Count == 0) return new TextMesh3D { W = shape.Width, H = shape.Height };
            lock (gate)
            {
                factory ??= D2D1.D2D1CreateFactory<ID2D1Factory>(FactoryType.MultiThreaded);
                using var geo = factory.CreatePathGeometry();
                using (var sink = geo.Open())
                {
                    sink.SetFillMode(FillMode.Winding);
                    foreach (var c in shape.Contours)
                    {
                        sink.BeginFigure(c[0], FigureBegin.Filled);
                        sink.AddLines(c[1..]);
                        sink.EndFigure(FigureEnd.Closed);
                    }
                    sink.Close();
                }
                // ふち取りの時は、太らせた形と元の形を合わせる
                ID2D1PathGeometry target = factory.CreatePathGeometry();
                using (var sink = target.Open())
                {
                    if (edge > 0f) geo.Widen(edge * 2f, null, null, .25f, sink);
                    else geo.Outline(.25f, sink);
                    sink.Close();
                }
                if (edge > 0f)
                {
                    var union = factory.CreatePathGeometry();
                    using (var sink = union.Open()) { target.CombineWithGeometry(geo, CombineMode.Union, .25f, sink); sink.Close(); }
                    target.Dispose(); target = union;
                }
                var tess = new TriSink();
                target.Tessellate(.25f, tess);
                var outl = new LineSink();
                target.Simplify(GeometrySimplificationOption.Lines, .25f, outl);
                target.Dispose();
                return new TextMesh3D { Tris = [.. tess.Points], Outlines = outl.Figures, W = shape.Width, H = shape.Height };
            }
        }
        catch (Exception e) { Shape3DRenderer.Report(e); return null; }
    }
}
