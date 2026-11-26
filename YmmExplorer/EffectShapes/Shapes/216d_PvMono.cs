using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;

namespace YMM4EffectShapes;

// =====================================================================
//  文字PV背景「白黒の図形」の仲間。紙にインクで描いたような図形の部品を、決まった配置で並べる。
//  部品は IInk に描くので、2D(平面の絵)と3D(紙から浮いた立体)の両方で同じ形になる。
//  座標は 1920×1080 を基準にした単位で、画面のまん中が (0,0)、y は下向き。z は紙からの高さ(3Dだけで使う)
// =====================================================================

/// <summary>インクで描く道具(2D と 3D で中身が違う)</summary>
internal interface IInk
{
    /// <summary>紙のざらつきの点の数</summary>
    int GrainCount { get; }
    void Line(float x0, float y0, float x1, float y1, float w, float a, float z);
    void Fill(List<Vector2> pts, float a, float z, float d);
    void Dot(float x, float y, float r, float a, float z);
    void Ring(float x, float y, float r, float w, float a, float z);
    /// <summary>紙のざらつきの小さな点</summary>
    void Speck(float x, float y, float r, float a);
}

/// <summary>2Dの絵に描く(PvFlat が、まん中が原点になるように変換してから呼ぶ)</summary>
internal sealed class Ink2D(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, float u, PvStage.Setup s) : IInk
{
    public int GrainCount => 700;
    Color4 C(float a)
    {
        var x = Vector3.Clamp(s.Pal.A + new Vector3(s.Flash * .12f), Vector3.Zero, Vector3.One);
        return new(x.X, x.Y, x.Z, Math.Clamp(a * s.Alpha, 0f, 1f));
    }
    public void Line(float x0, float y0, float x1, float y1, float w, float a, float z) { br.Color = C(a); dc.DrawLine(new(x0 * u, y0 * u), new(x1 * u, y1 * u), br, MathF.Max(.8f, w * u)); }
    public void Fill(List<Vector2> pts, float a, float z, float d)
    {
        var q = new List<Vector2>(pts.Count);
        foreach (var p in pts) q.Add(p * u);
        br.Color = C(a); Gfx.FillPoly(dc, br, q);
    }
    public void Dot(float x, float y, float r, float a, float z) { if (r <= .05f) return; br.Color = C(a); dc.FillEllipse(new Ellipse(new(x * u, y * u), r * u, r * u), br); }
    public void Ring(float x, float y, float r, float w, float a, float z) { if (r <= .05f) return; br.Color = C(a); dc.DrawEllipse(new Ellipse(new(x * u, y * u), r * u, r * u), br, MathF.Max(.8f, w * u)); }
    public void Speck(float x, float y, float r, float a) => Dot(x, y, MathF.Max(r, .5f / MathF.Max(u, .01f)), a, 0f);
}

/// <summary>3Dに組み立てる(線は細い棒、塗りは厚い板、点は玉、輪はドーナツ)</summary>
internal sealed class Ink3D(Sculpt m, PvStage.Setup s) : IInk
{
    public int GrainCount => 260;
    Vector4 V(float a)
    {
        var x = Vector3.Clamp(s.Pal.A + new Vector3(s.Flash * .12f), Vector3.Zero, Vector3.One);
        return new(x.X, x.Y, x.Z, Math.Clamp(a * s.Alpha, 0f, 1f));
    }
    public void Line(float x0, float y0, float x1, float y1, float w, float a, float z) => m.Limb(x0, y0, z, x1, y1, z, w / 2f, w / 2f, V(a));
    public void Fill(List<Vector2> pts, float a, float z, float d) => m.Slab(pts, z, d, V(a));
    public void Dot(float x, float y, float r, float a, float z) { if (r > .05f) m.Ball(x, y, z, r, r, r * .5f, V(a)); }
    public void Ring(float x, float y, float r, float w, float a, float z)
    {
        if (r <= .05f) return;
        // 大きな輪は角ばって見えないよう、細かく分ける
        int keep = m.Seg; m.Seg = Math.Clamp((int)(r / 6f), 16, 48);
        m.Torus(x, y, z, r, w / 2f, V(a));
        m.Seg = keep;
    }
    public void Speck(float x, float y, float r, float a) => m.Quad3D(new(x - r, y - r, 1f), new(x + r, y - r, 1f), new(x + r, y + r, 1f), new(x - r, y + r, 1f), V(a));
}

internal sealed class PvMono
{
    /// <summary>白黒の図形の仲間か(紙とインクの配色・暗くしない・光を当てない、の対象)</summary>
    public static bool IsMono(PvScene x) => x == PvScene.MonoGeo || (x >= PvScene.MonoCircle && x <= PvScene.MonoRandom);

    readonly IInk k;
    readonly PvStage.Setup s;
    readonly float ex, ey, mv, tm;
    readonly int st;
    readonly bool flip;
    const float HW = 960f, HH = 540f, LW = 2.4f;

    /// <param name="ex">画面の半分の幅(基準の単位。回転やズームの分の余白込み)</param>
    public PvMono(IInk ink, in PvStage.Setup s, float ex, float ey)
    {
        k = ink; this.s = s; this.ex = ex; this.ey = ey;
        mv = s.Dist * 38f;
        tm = s.T * MathF.Max(.2f, MathF.Abs(s.Spin));
        st = (int)MathF.Floor(tm * 2f);          // 拍ごとに1つ進む数
        flip = (s.Seed & 1) != 0;                // 種が奇数なら左右反対
    }

    public void Run()
    {
        Grain();
        switch (s.Scene)
        {
            case PvScene.MonoCircle: CircleLayout(); break;
            case PvScene.MonoLine: LineLayout(); break;
            case PvScene.MonoGrid: GridLayout(); break;
            case PvScene.MonoSlash: SlashLayout(); break;
            case PvScene.MonoDots: DotsLayout(); break;
            case PvScene.MonoRandom: RandomLayout(); break;
            default: Classic(); break;
        }
    }

    // ---------------------------------------------------------------- 道具
    float R(int i, int j) => Gfx.Hash01(unchecked(s.Seed * 131 + i * 19 + j * 1013 + 11));
    static float Mod(float x, float m) => x - MathF.Floor(x / m) * m;
    float X(float x) => flip ? -x : x;
    void L(float x0, float y0, float x1, float y1, float w = LW, float z = 20f, float a = .95f) => k.Line(X(x0), y0, X(x1), y1, w, a, z);
    void Pf(float z, float d, params float[] xy)
    {
        var l = new List<Vector2>(xy.Length / 2);
        for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(new(X(xy[i]), xy[i + 1]));
        k.Fill(l, .96f, z, d);
    }
    void Rect(float x, float y, float w, float h, float z = 20f, float d = 8f) => Pf(z, d, x, y, x + w, y, x + w, y + h, x, y + h);
    void D(float x, float y, float r, float z = 20f) => k.Dot(X(x), y, r, .95f, z);
    void O(float x, float y, float r, float w = LW, float z = 20f) => k.Ring(X(x), y, r, w, .95f, z);
    void Frame(float x, float y, float w, float h, float lw, float z) { L(x, y, x + w, y, lw, z); L(x + w, y, x + w, y + h, lw, z); L(x + w, y + h, x, y + h, lw, z); L(x, y + h, x, y, lw, z); }

    // ---------------------------------------------------------------- 部品
    /// <summary>紙のざらつき(1秒に12回変わる点とかすれ)</summary>
    void Grain()
    {
        int tick = (int)MathF.Floor(s.T * 12f);
        for (int i = 0; i < k.GrainCount; i++)
        {
            int id = i * 7 + tick * 7919;
            k.Speck((R(id, 0) - .5f) * ex * 2f, (R(id, 1) - .5f) * ey * 2f, .5f + R(id, 2) * 1.2f, .12f + R(id, 3) * .3f);
        }
        for (int i = 0; i < 16; i++)
        {
            int id = i * 13 + (tick / 3) * 104729; float x = (R(id, 0) - .5f) * ex * 2f, y = (R(id, 1) - .5f) * ey * 2f, a = (R(id, 2) - .5f) * .5f, l = 20f + R(id, 3) * 70f;
            k.Line(x, y, x + MathF.Cos(a) * l, y + MathF.Sin(a) * l, .6f + R(id, 5) * 1.6f, .18f + R(id, 4) * .2f, 2f);
        }
    }

    /// <summary>× の並び(ななめの列ごとに + に回る)</summary>
    void XGrid(float x0, float y0, int cols, int rows, float gx, float gy, float sz)
    {
        for (int r = 0; r < rows; r++)
            for (int q = 0; q < cols; q++)
            {
                float x = x0 + q * gx, y = y0 + r * gy, a = (q + r + st) % 6 == 0 ? 0f : MathF.PI / 4f, cs = MathF.Cos(a) * sz, sn = MathF.Sin(a) * sz;
                L(x - cs, y - sn, x + cs, y + sn, LW, 30f); L(x + sn, y - cs, x - sn, y + cs, LW, 30f);
            }
    }

    /// <summary>ななめ線入りの四角の列。shift だけ左へずれる(x0 より左とxMaxより右は描かない)</summary>
    void Boxes(float y, float x0, int n, float shift, int row, float xMax = float.MaxValue, float b = 112f, float g = 160f)
    {
        float sh = Mod(shift, g); int off = (int)MathF.Floor(shift / g);
        for (int i = 0; i <= n + 1; i++)
        {
            float x = x0 + i * g - sh;
            if (x < x0 - 1f || x + b > xMax) continue;
            int id = i + off + row;
            Frame(x, y, b, b, LW, 50f);
            if ((id + st) % 7 == 0) Pf(50f, 8f, x, y, x + b, y + b, x, y + b);
            else if ((id + st) % 5 == 0) L(x + b, y, x, y + b, LW, 56f);
            else L(x, y, x + b, y + b, LW, 56f);
        }
    }

    /// <summary>ななめ線の平行四辺形(線が1本ずつ増え、そろうとまた最初から)</summary>
    void Hatch(float x0, float x1, float top, float bot, float gap = 11f)
    {
        float sk = (bot - top) * .55f; int n = (int)((x1 - x0) / gap), ph = st % (n / 4 + 8), vis = Math.Min(n, ph * 4);
        for (int i = 0; i < vis; i++) { float x = x0 + i * gap; L(x, bot, x + sk, top, 1.5f, 12f, .85f); }
    }

    /// <summary>太いななめ線の帯(横に流れる)</summary>
    void Band(float top, float bot)
    {
        float g = 46f, h = bot - top, o = Mod(mv + tm * 60f, g);
        for (float x = -ex - g * 3f - h + o; x < ex + g; x += g) k.Line(x, bot, x + h, top, 3f, .95f, 8f);
    }

    /// <summary>目盛りの輪(拍ごとに少し回る。まばらに欠ける)</summary>
    void TickRing(float cx, float cy, float r)
    {
        float rot = st * MathF.PI / 18f;
        for (int i = 0; i < 36; i++)
        {
            if (R(i + st * 3, 7) < .1f) continue;
            float a = rot + i * MathF.Tau / 36f, c = MathF.Cos(a), sn = MathF.Sin(a);
            L(cx + c * r * .8f, cy + sn * r * .8f, cx + c * r * 1.08f, cy + sn * r * 1.08f, 1.8f, 70f);
            D(cx + c * r * .94f, cy + sn * r * .94f, 3.4f, 70f);
        }
    }

    /// <summary>黒い直角三角の積み重ね(拍ごとに1つずつ増える)</summary>
    void Tris(float x, float y0, int count, float w, float h)
    {
        int vis = 1 + st % (count + 1);
        for (int i = 0; i < Math.Min(count, vis); i++) { float y = y0 + i * h * 1.02f; Pf(40f, 50f, x, y, x, y + h, x + w, y + h); }
    }

    /// <summary>バーコード(2拍ごとに模様が変わる)</summary>
    void Barcode(float x, float y, float w, float h)
    {
        float cx = x; int i = 0, sd = st / 2 * 41;
        while (i < 200)
        {
            float bw = 2f + R(i + sd, 20) * 9f, gp = 3f + R(i + sd, 21) * 7f;
            if (cx + bw > x + w) break;
            Rect(cx, y, bw, h * (R(i + sd, 22) > .85f ? 1.12f : 1f), 30f, 6f);
            cx += bw + gp; i++;
        }
    }

    /// <summary>同心円(1本ずつ太くなる輪が外へ移る)</summary>
    void Concentric(float cx, float cy, float r, int n)
    {
        int hi = st % n;
        for (int i = 0; i < n; i++) O(cx, cy, r * (i + 1) / n, i == hi ? 9f : LW, 20f + i * 8f);
        D(cx, cy, r * .08f, 60f);
    }

    /// <summary>点の並び(大きさが波のように変わる)</summary>
    void DotGrid(float x0, float y0, int cols, int rows, float g)
    {
        for (int r = 0; r < rows; r++)
            for (int q = 0; q < cols; q++)
            {
                float p = .5f + .5f * MathF.Sin(q * .7f + r * .5f - st * .8f);
                D(x0 + q * g, y0 + r * g, g * (.06f + .2f * p), 15f + p * 30f);
            }
    }

    /// <summary>照準(二重の輪と十字、まわりの三角が拍ごとに回る)</summary>
    void Crosshair(float cx, float cy, float r)
    {
        O(cx, cy, r, LW, 40f); O(cx, cy, r * .6f, 1.4f, 40f);
        L(cx - r * 1.25f, cy, cx - r * .2f, cy, 1.6f, 44f); L(cx + r * .2f, cy, cx + r * 1.25f, cy, 1.6f, 44f);
        L(cx, cy - r * 1.25f, cx, cy - r * .2f, 1.6f, 44f); L(cx, cy + r * .2f, cx, cy + r * 1.25f, 1.6f, 44f);
        float rot = st * MathF.PI / 12f;
        for (int i = 0; i < 4; i++)
        {
            float a = rot + i * MathF.PI / 2f, c = MathF.Cos(a), sn = MathF.Sin(a);
            Pf(50f, 10f, cx + c * r * 1.05f - sn * 8f, cy + sn * r * 1.05f + c * 8f, cx + c * r * 1.05f + sn * 8f, cy + sn * r * 1.05f - c * 8f, cx + c * r * 1.25f, cy + sn * r * 1.25f);
        }
        D(cx, cy, 5f, 50f);
    }

    /// <summary>「＞」の並び(塗りつぶしの1つが拍ごとに進む)</summary>
    void Chevrons(float x, float y, int n, float sz, float gap)
    {
        int hi = st % n;
        for (int i = 0; i < n; i++)
        {
            float cx = x + i * gap;
            if (i == hi) Pf(30f, 10f, cx - sz * .5f, y - sz, cx + sz * .5f, y, cx - sz * .5f, y + sz);
            else { L(cx - sz * .5f, y - sz, cx + sz * .5f, y, 3f, 30f); L(cx + sz * .5f, y, cx - sz * .5f, y + sz, 3f, 30f); }
        }
    }

    /// <summary>画面の四すみのかぎ</summary>
    void Corners(float inset, float len)
    {
        float x0 = -HW + inset, x1 = HW - inset, y0 = -HH + inset, y1 = HH - inset;
        L(x0, y0, x0 + len, y0, 3f, 10f); L(x0, y0, x0, y0 + len, 3f, 10f);
        L(x1, y0, x1 - len, y0, 3f, 10f); L(x1, y0, x1, y0 + len, 3f, 10f);
        L(x0, y1, x0 + len, y1, 3f, 10f); L(x0, y1, x0, y1 - len, 3f, 10f);
        L(x1, y1, x1 - len, y1, 3f, 10f); L(x1, y1, x1, y1 - len, 3f, 10f);
    }

    /// <summary>○の並び(●が拍ごとに進む)</summary>
    void CircleRow(float x, float y, int n, float r, float gap)
    {
        int hi = st % n;
        for (int i = 0; i < n; i++) { float cx = x + i * gap; if (i == hi) D(cx, y, r, 30f); else O(cx, y, r, LW, 30f); }
    }

    /// <summary>半分を塗った円(拍ごとに45度回る)</summary>
    void HalfMoon(float cx, float cy, float r)
    {
        O(cx, cy, r, LW, 30f);
        float a0 = st * MathF.PI / 4f; var xy = new float[34];
        for (int j = 0; j <= 16; j++) { float a = a0 + j * MathF.PI / 16f; xy[j * 2] = cx + MathF.Cos(a) * r * .9f; xy[j * 2 + 1] = cy + MathF.Sin(a) * r * .9f; }
        Pf(34f, 10f, xy);
    }

    /// <summary>ジグザグの線(拍ごとに山と谷が入れかわる)</summary>
    void Zigzag(float x0, float x1, float y, float amp, float seg)
    {
        int n = (int)((x1 - x0) / seg);
        for (int i = 0; i < n; i++)
        {
            bool up = ((i + st) & 1) == 0;
            L(x0 + i * seg, y + (up ? -amp : amp), x0 + (i + 1) * seg, y + (up ? amp : -amp), LW, 20f);
        }
    }

    /// <summary>なみ線(流れる)</summary>
    void Wave(float x0, float x1, float y, float amp, float len)
    {
        const float step = 24f; float ph = tm * 2f;
        float Y(float x) => y + MathF.Sin(x / len * MathF.Tau + ph) * amp;
        for (float x = x0; x < x1; x += step) L(x, Y(x), x + step, Y(x + step), LW, 20f);
    }

    /// <summary>ものさし(目盛りが流れる。dir=-1 で目盛りが上、1 で下)</summary>
    void Ruler(float y, float x0, float x1, float dir)
    {
        L(x0, y, x1, y, LW, 20f);
        float sh = mv + st * 20f, o = Mod(sh, 20f);
        for (float x = x0 + 20f - o; x < x1; x += 20f)
        {
            bool big = Mod(MathF.Round((x + sh) / 20f), 5f) < .5f;
            L(x, y, x, y + dir * (big ? 22f : 10f), 1.6f, 20f);
        }
    }

    /// <summary>市松の帯(拍ごとに白黒が入れかわる)</summary>
    void Checker(float x0, float y0, int cols, int rows, float c)
    {
        for (int r = 0; r < rows; r++)
            for (int q = 0; q < cols; q++)
                if (((r + q + st) & 1) == 0) Rect(x0 + q * c, y0 + r * c, c, c, 25f, 10f);
        Frame(x0, y0, cols * c, rows * c, LW, 25f);
    }

    /// <summary>点線(流れる)</summary>
    void Dashes(float x0, float y0, float x1, float y1, float dash, float gap)
    {
        var d = new Vector2(x1 - x0, y1 - y0); float len = d.Length(); if (len < 1f) return; d /= len;
        float o = Mod(tm * 40f, dash + gap);
        for (float a = -dash + o; a < len; a += dash + gap)
        {
            float a0 = MathF.Max(0f, a), a1 = MathF.Min(len, a + dash);
            if (a1 > a0) L(x0 + d.X * a0, y0 + d.Y * a0, x0 + d.X * a1, y0 + d.Y * a1, LW, 20f);
        }
    }

    /// <summary>うすい + の方眼(画面いっぱい)</summary>
    void PlusGrid(float g, float sz)
    {
        for (float y = -HH + g / 2f; y < HH; y += g)
            for (float x = -HW + g / 2f; x < HW; x += g) { L(x - sz, y, x + sz, y, 1.4f, 5f, .35f); L(x, y - sz, x, y + sz, 1.4f, 5f, .35f); }
    }

    /// <summary>三角の枠(拍ごとに30度回る)</summary>
    void BigTri(float cx, float cy, float r, float extra)
    {
        float rot = st * MathF.PI / 6f + extra;
        for (int i = 0; i < 3; i++)
        {
            float a = rot + i * MathF.Tau / 3f, b = rot + (i + 1) * MathF.Tau / 3f;
            L(cx + MathF.Cos(a) * r, cy + MathF.Sin(a) * r, cx + MathF.Cos(b) * r, cy + MathF.Sin(b) * r, 3f, 40f);
        }
    }

    // ---------------------------------------------------------------- 配置
    /// <summary>最初の「白黒の図形」</summary>
    void Classic()
    {
        XGrid(-868f, -442f, 6, 4, 114f, 116f, 15f);
        Boxes(-320f, -168f, 9, st * 40f + mv, 0, ex + 40f);
        Boxes(278f, -892f, 8, 0f, 31);
        Hatch(-870f, -360f, -30f, 132f);
        Band(435f, ey + 4f);
        TickRing(432f, 240f, 140f);
        Tris(620f, 20f, 4, 90f, 92f);
    }

    /// <summary>円: 照準・同心円・点・○の並び・ものさし</summary>
    void CircleLayout()
    {
        Corners(28f, 70f);
        DotGrid(-880f, -470f, 8, 4, 46f);
        CircleRow(-300f, -430f, 7, 24f, 78f);
        Crosshair(430f, -40f, 230f);
        Concentric(-640f, 200f, 190f, 6);
        HalfMoon(800f, 330f, 70f);
        Ruler(470f, -940f, 940f, -1f);
    }

    /// <summary>線: ものさし・バーコード・ジグザグ・なみ線・「＞」・点線</summary>
    void LineLayout()
    {
        Corners(28f, 70f);
        Ruler(-480f, -940f, 940f, 1f);
        Barcode(-880f, -410f, 380f, 120f);
        Chevrons(470f, -350f, 6, 34f, 62f);
        Zigzag(-940f, 940f, -220f, 26f, 46f);
        Wave(-940f, 940f, 290f, 34f, 420f);
        Wave(-940f, 940f, 330f, 20f, 260f);
        Dashes(-940f, 410f, 940f, 410f, 26f, 14f);
        Ruler(480f, -940f, 940f, -1f);
    }

    /// <summary>格子: うすい方眼・市松・四角・三角・半円・×</summary>
    void GridLayout()
    {
        PlusGrid(80f, 7f);
        Checker(-880f, -460f, 10, 3, 34f);
        Tris(700f, -440f, 4, 70f, 72f);
        HalfMoon(-660f, 140f, 100f);
        XGrid(480f, 160f, 5, 3, 90f, 80f, 12f);
        Boxes(300f, -892f, 5, st * 30f + mv, 57, -120f, 90f, 120f);
    }

    /// <summary>斜め: ななめ線の帯・平行四辺形・回る三角・「＞」・点線</summary>
    void SlashLayout()
    {
        Band(-ey - 4f, -470f);
        Band(440f, ey + 4f);
        Hatch(-900f, -300f, -420f, -200f);
        Hatch(260f, 880f, 200f, 380f);
        BigTri(640f, -250f, 150f, 0f);
        BigTri(-640f, 260f, 110f, 1f);
        Chevrons(-160f, 320f, 5, 36f, 60f);
        Dashes(300f, -430f, 940f, -430f, 30f, 16f);
    }

    /// <summary>点: 点の並び・○の並び・照準2つ・バーコード</summary>
    void DotsLayout()
    {
        Corners(28f, 60f);
        DotGrid(-910f, -470f, 19, 4, 52f);
        Crosshair(-600f, 60f, 110f);
        Crosshair(640f, 0f, 170f);
        CircleRow(-860f, 340f, 9, 22f, 70f);
        Barcode(360f, 300f, 440f, 100f);
    }

    /// <summary>おまかせ: 種ごとに、6つの場所へ部品を選んで置く(種を変えるたびに違う組み合わせ)</summary>
    void RandomLayout()
    {
        (float x, float y, bool wide)[] slots = [(-640f, -330f, false), (640f, -330f, false), (-640f, 300f, false), (640f, 300f, false), (0f, -420f, true), (0f, 400f, true)];
        int[] any = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12], wide = [5, 9, 11, 12];
        var used = new HashSet<int>();
        for (int i = 0; i < slots.Length; i++)
        {
            var pool = slots[i].wide ? wide : any; int type = pool[0];
            for (int tries = 0; tries < 40; tries++) { int t = pool[(int)(R(i * 7 + tries, 90) * pool.Length) % pool.Length]; if (used.Add(t)) { type = t; break; } }
            SlotPart(type, slots[i].x, slots[i].y);
        }
        if (R(1, 91) > .5f) Band(470f, ey + 4f); else Ruler(490f, -940f, 940f, -1f);
        if (R(2, 91) > .5f) Corners(28f, 60f);
    }

    void SlotPart(int type, float cx, float cy)
    {
        switch (type)
        {
            case 0: XGrid(cx - 170f, cy - 90f, 4, 3, 113f, 90f, 13f); break;
            case 1: Boxes(cy - 56f, cx - 230f, 3, 0f, 7); break;
            case 2: Hatch(cx - 200f, cx + 120f, cy - 80f, cy + 80f); break;
            case 3: TickRing(cx, cy, 110f); break;
            case 4: Tris(cx - 45f, cy - 126f, 3, 80f, 82f); break;
            case 5: Barcode(cx - 180f, cy - 50f, 360f, 100f); break;
            case 6: Concentric(cx, cy, 115f, 5); break;
            case 7: DotGrid(cx - 161f, cy - 69f, 8, 4, 46f); break;
            case 8: Crosshair(cx, cy, 90f); break;
            case 9: CircleRow(cx - 180f, cy, 5, 22f, 90f); break;
            case 10: HalfMoon(cx, cy, 100f); break;
            case 11: Checker(cx - 170f, cy - 51f, 10, 3, 34f); break;
            default: Chevrons(cx - 150f, cy, 5, 36f, 64f); break;
        }
    }
}
