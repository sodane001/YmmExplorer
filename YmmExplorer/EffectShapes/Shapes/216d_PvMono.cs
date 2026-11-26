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
    public static bool IsMono(PvScene x) => x == PvScene.MonoGeo || (x >= PvScene.MonoCircle && x <= PvScene.MonoRandom2) || (x >= PvScene.MonoSymbol && x <= PvScene.MonoAlt) || (x >= PvScene.MonoRecord && x <= PvScene.MonoBlocks);

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
        // カクッと動く回数: BPMに合わせるときは拍ぴったり(動くタイミングの設定で、半拍〜1小節ごと)
        st = s.Sync ? (int)MathF.Floor(s.Beats * s.PerBeat + 1e-4f) : (int)MathF.Floor(tm * 2f * s.PerBeat);
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
            case PvScene.MonoSquare: SquareLayout(); break;
            case PvScene.MonoArrow: ArrowLayout(); break;
            case PvScene.MonoStar: StarLayout(); break;
            case PvScene.MonoSpiral: SpiralLayout(); break;
            case PvScene.MonoBars: BarsLayout(); break;
            case PvScene.MonoNet: NetLayout(); break;
            case PvScene.MonoFrame: FrameLayout(); break;
            case PvScene.MonoTri: TriLayout(); break;
            case PvScene.MonoRandom2: RandomLayout2(); break;
            case PvScene.MonoSymbol: SymbolLayout(); break;
            case PvScene.MonoGauge: GaugeLayout(); break;
            case PvScene.MonoBands: BandsLayout(); break;
            case PvScene.MonoBlueprint: BlueprintLayout(); break;
            case PvScene.MonoScore: ScoreLayout(); break;
            case PvScene.MonoMap: MapLayout(); break;
            case PvScene.MonoSignal: SignalLayout(); break;
            case PvScene.MonoAlt: AltLayout(); break;
            case PvScene.MonoRecord: RecordLayout(); break;
            case PvScene.MonoConstellation: ConstellationLayout(); break;
            case PvScene.MonoCircuit: CircuitLayout(); break;
            case PvScene.MonoClock: ClockLayout(); break;
            case PvScene.MonoGraph: GraphPaperLayout(); break;
            case PvScene.MonoSun: SunLayout(); break;
            case PvScene.MonoWaves: WavesLayout(); break;
            case PvScene.MonoBlocks: BlocksLayout(); break;
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
            case 12: Chevrons(cx - 150f, cy, 5, 36f, 64f); break;
            default: SlotPart2(type, cx, cy); break;
        }
    }

    // ---------------------------------------------------------------- v125 で足した部品
    /// <summary>正多角形(枠か塗り)</summary>
    void RegPoly(float cx, float cy, float r, int sides, float rot, float w, float z, bool fill)
    {
        var xy = new float[sides * 2];
        for (int j = 0; j < sides; j++) { float a = rot + j * MathF.Tau / sides; xy[j * 2] = cx + MathF.Cos(a) * r; xy[j * 2 + 1] = cy + MathF.Sin(a) * r; }
        if (fill) { Pf(z, 10f, xy); return; }
        for (int j = 0; j < sides; j++) { int k2 = (j + 1) % sides; L(xy[j * 2], xy[j * 2 + 1], xy[k2 * 2], xy[k2 * 2 + 1], w, z); }
    }

    /// <summary>回る四角の入れ子(1つおきに反対向き。太い1本が内へ移る)</summary>
    void NestSquares(float cx, float cy, float r, int n)
    {
        int hi = st % n;
        for (int i = 0; i < n; i++)
        {
            float rr = r * (n - i) / n, a = st * MathF.PI / 12f * (i % 2 == 0 ? 1f : -1f) + i * .2f;
            RegPoly(cx, cy, rr, 4, a, i == hi ? 7f : LW, 20f + i * 10f, false);
        }
    }

    /// <summary>升目(1マスずつ塗りが進む)</summary>
    void CellGrid(float x0, float y0, int cols, int rows, float c)
    {
        for (int q = 0; q <= cols; q++) L(x0 + q * c, y0, x0 + q * c, y0 + rows * c, 1.6f, 20f);
        for (int r = 0; r <= rows; r++) L(x0, y0 + r * c, x0 + cols * c, y0 + r * c, 1.6f, 20f);
        int idx = st % (cols * rows), idx2 = (st * 7 + 3) % (cols * rows);
        Rect(x0 + idx % cols * c + 5f, y0 + idx / cols * c + 5f, c - 10f, c - 10f, 26f, 10f);
        D(x0 + idx2 % cols * c + c / 2f, y0 + idx2 / cols * c + c / 2f, c * .18f, 26f);
    }

    /// <summary>回る四角の列(拍ごとに22.5度ずつ回る。■が進む)</summary>
    void SpinSquares(float x, float y, int n, float sz, float gap)
    {
        int hi = st % n;
        for (int i = 0; i < n; i++) RegPoly(x + i * gap, y, sz, 4, (st + i) * MathF.PI / 8f, LW, 30f, i == hi);
    }

    /// <summary>大きなかっこ [ ] (拍ごとに少し開く)</summary>
    void Brackets(float cx, float cy, float w, float h, float lip)
    {
        float hw = w / 2f * ((st & 1) == 0 ? 1f : 1.05f), hh = h / 2f;
        foreach (float sx in new[] { -1f, 1f })
        {
            float x = cx + sx * hw;
            L(x, cy - hh, x, cy + hh, 4f, 30f);
            L(x, cy - hh, x - sx * lip, cy - hh, 4f, 30f);
            L(x, cy + hh, x - sx * lip, cy + hh, 4f, 30f);
        }
    }

    /// <summary>太い矢印(拍ごとに少しずつ前へ出て、4拍で戻る)。dir=1 で右向き、-1 で左向き</summary>
    void BigArrow(float x, float y, float len, float th, float dir)
    {
        float o = (st % 4) * 20f * dir, hl = th * 1.6f;
        float P(float d) => x + o + d * dir;
        Pf(40f, 30f, P(0), y - th / 2f, P(len - hl), y - th / 2f, P(len - hl), y - th * 1.3f, P(len), y, P(len - hl), y + th * 1.3f, P(len - hl), y + th / 2f, P(0), y + th / 2f);
    }

    /// <summary>進み具合のバー(拍ごとに1目盛り伸びる)</summary>
    void Progress(float x, float y, float w, float h, int seg)
    {
        Frame(x, y, w, h, LW, 25f);
        int n = st % (seg + 1); float cw = (w - 8f) / seg;
        for (int i = 0; i < n; i++) Rect(x + 4f + i * cw, y + 4f, cw - 4f, h - 8f, 25f, 8f);
    }

    /// <summary>星(5つの角)</summary>
    void Star(float cx, float cy, float r, float rot, bool fill)
    {
        var xy = new float[20];
        for (int j = 0; j < 10; j++) { float a = rot - MathF.PI / 2f + j * MathF.PI / 5f, rr = j % 2 == 0 ? r : r * .42f; xy[j * 2] = cx + MathF.Cos(a) * rr; xy[j * 2 + 1] = cy + MathF.Sin(a) * rr; }
        if (fill) { Pf(40f, 14f, xy); return; }
        for (int j = 0; j < 10; j++) { int k2 = (j + 1) % 10; L(xy[j * 2], xy[j * 2 + 1], xy[k2 * 2], xy[k2 * 2 + 1], LW, 40f); }
    }

    /// <summary>太陽のような放射の輪(長い線が拍ごとに回る)</summary>
    void SunRays(float cx, float cy, float r, int n)
    {
        O(cx, cy, r * .7f, LW, 30f);
        for (int i = 0; i < n; i++)
        {
            float a = i * MathF.Tau / n, c = MathF.Cos(a), sn = MathF.Sin(a), len = (i + st) % 3 == 0 ? r * .5f : r * .22f;
            L(cx + c * r, cy + sn * r, cx + c * (r + len), cy + sn * (r + len), 3f, 30f);
        }
    }

    /// <summary>◇の並び(◆が進む)</summary>
    void DiamondRow(float x, float y, int n, float r, float gap)
    {
        int hi = st % n;
        for (int i = 0; i < n; i++) RegPoly(x + i * gap, y, r, 4, 0f, LW, 30f, i == hi);
    }

    /// <summary>うずまき(拍ごとに回る)</summary>
    void Spiral(float cx, float cy, float r, float turns)
    {
        float rot = st * MathF.PI / 8f; int steps = (int)(turns * 40f);
        float px = cx, py = cy;
        for (int j = 1; j <= steps; j++)
        {
            float f = j / (float)steps, a = rot + f * turns * MathF.Tau, x = cx + MathF.Cos(a) * r * f, y = cy + MathF.Sin(a) * r * f;
            L(px, py, x, y, 3f, 30f); px = x; py = y;
        }
        D(px, py, 7f, 34f);
    }

    /// <summary>縦の棒グラフ(拍ごとに高さが変わる)。base0 は棒の下の位置</summary>
    void EqBars(float x, float base0, int n, float w, float gap, float hMax)
    {
        for (int i = 0; i < n; i++)
        {
            float h = hMax * (.15f + .85f * R(i + st * 17, 30));
            Rect(x + i * (w + gap), base0 - h, w, h, 30f, 20f);
        }
        L(x - 10f, base0 + 6f, x + n * (w + gap), base0 + 6f, LW, 30f);
    }

    /// <summary>つながった点(網)。塗った点が拍ごとに移る</summary>
    void Network(float x0, float y0, float w, float h, int n, int key)
    {
        var p = new Vector2[n];
        for (int i = 0; i < n; i++) p[i] = new(x0 + R(i + key, 40) * w, y0 + R(i + key, 41) * h);
        for (int i = 0; i < n; i++) { L(p[i].X, p[i].Y, p[(i + 1) % n].X, p[(i + 1) % n].Y, 1.4f, 20f); if (i % 2 == 0) L(p[i].X, p[i].Y, p[(i + 3) % n].X, p[(i + 3) % n].Y, 1.4f, 20f); }
        for (int i = 0; i < n; i++) { if ((i + st) % 4 == 0) D(p[i].X, p[i].Y, 9f, 30f); else O(p[i].X, p[i].Y, 8f, LW, 30f); }
    }

    /// <summary>画面のふちの二重の枠</summary>
    void DoubleFrame(float inset)
    {
        Frame(-HW + inset, -HH + inset, (HW - inset) * 2f, (HH - inset) * 2f, 3f, 10f);
        Frame(-HW + inset + 14f, -HH + inset + 14f, (HW - inset - 14f) * 2f, (HH - inset - 14f) * 2f, 1.2f, 10f);
    }

    /// <summary>大きな太い×(拍ごとに少し傾く)</summary>
    void BigX(float cx, float cy, float r)
    {
        float a = MathF.PI / 4f + ((st & 1) == 0 ? 0f : MathF.PI / 14f), c = MathF.Cos(a) * r, sn = MathF.Sin(a) * r;
        L(cx - c, cy - sn, cx + c, cy + sn, 14f, 40f); L(cx + sn, cy - c, cx - sn, cy + c, 14f, 40f);
    }

    /// <summary>▲▽の並び(塗りが進む)</summary>
    void TriRow(float x, float y, int n, float sz)
    {
        for (int i = 0; i < n; i++)
        {
            bool up = (i & 1) == 0, fill = (i + st) % 5 == 0; float cx = x + i * sz * .55f, t0 = up ? y - sz / 2f : y + sz / 2f, b0 = up ? y + sz / 2f : y - sz / 2f;
            if (fill) Pf(30f, 10f, cx, t0, cx + sz / 2f, b0, cx - sz / 2f, b0);
            else { L(cx, t0, cx + sz / 2f, b0, LW, 30f); L(cx + sz / 2f, b0, cx - sz / 2f, b0, LW, 30f); L(cx - sz / 2f, b0, cx, t0, LW, 30f); }
        }
    }

    /// <summary>点の濃淡(大きい点から小さい点へ。拍ごとに流れる)</summary>
    void DotFade(float x0, float y0, int cols, int rows, float g)
    {
        for (int r = 0; r < rows; r++)
            for (int q = 0; q < cols; q++)
            {
                float f = ((q + st) % cols) / (float)cols;
                D(x0 + q * g, y0 + r * g, g * .42f * (1f - f) + .8f, 20f);
            }
    }

    // ---------------------------------------------------------------- v125 で足した配置
    void SquareLayout()
    {
        PlusGrid(80f, 6f);
        Corners(28f, 60f);
        CellGrid(-880f, -460f, 8, 3, 52f);
        Checker(280f, 440f, 18, 1, 34f);
        NestSquares(620f, -200f, 180f, 5);
        SpinSquares(-860f, 330f, 8, 30f, 90f);
        Brackets(620f, 330f, 380f, 140f, 30f);
    }

    void ArrowLayout()
    {
        BigArrow(-900f, -390f, 520f, 40f, 1f);
        Chevrons(300f, -390f, 7, 30f, 56f);
        Dashes(-940f, -260f, 940f, -260f, 30f, 16f);
        Dashes(-940f, 250f, 940f, 250f, 30f, 16f);
        Progress(-880f, 310f, 520f, 50f, 10);
        BigArrow(900f, 420f, 520f, 40f, -1f);
    }

    void StarLayout()
    {
        Star(-640f, -250f, 130f, st * MathF.PI / 10f, false);
        SunRays(660f, -250f, 90f, 24);
        Star(700f, 270f, 90f, -st * MathF.PI / 10f, true);
        DiamondRow(-880f, 330f, 9, 20f, 70f);
        Band(470f, ey + 4f);
        for (int i = 0; i < 10; i++)
        {
            float x = (R(i, 50) - .5f) * 1800f, y = (R(i, 51) - .5f) * 980f;
            if (MathF.Abs(x) < 420f && MathF.Abs(y) < 200f) continue;
            Star(x, y, 14f + R(i, 52) * 10f, i + st * .3f, (i + st) % 3 == 0);
        }
    }

    void SpiralLayout()
    {
        Ruler(-480f, -940f, 940f, 1f);
        Spiral(-600f, 140f, 250f, 4f);
        Concentric(640f, -170f, 140f, 5);
        TickRing(640f, -170f, 200f);
        DotGrid(380f, 300f, 10, 4, 44f);
    }

    void BarsLayout()
    {
        EqBars(-900f, -300f, 14, 22f, 12f, 170f);
        Barcode(420f, -460f, 440f, 110f);
        Progress(360f, 300f, 560f, 46f, 12);
        EqBars(-900f, 420f, 20, 14f, 10f, 120f);
        Ruler(480f, -940f, 940f, -1f);
    }

    void NetLayout()
    {
        Corners(28f, 60f);
        Network(-900f, -470f, 700f, 300f, 14, 0);
        Crosshair(620f, -300f, 90f);
        Network(240f, 150f, 660f, 300f, 12, 300);
        DotGrid(-880f, 300f, 9, 4, 44f);
    }

    void FrameLayout()
    {
        DoubleFrame(30f);
        BigX(-640f, -230f, 100f);
        Brackets(560f, -250f, 420f, 160f, 36f);
        HalfMoon(-660f, 280f, 90f);
        XGrid(380f, 220f, 5, 2, 100f, 100f, 14f);
    }

    void TriLayout()
    {
        TriRow(-900f, -420f, 14, 70f);
        BigTri(640f, -220f, 160f, 0f);
        Tris(-860f, 140f, 3, 90f, 92f);
        DotFade(300f, 280f, 14, 4, 40f);
        TriRow(-500f, 440f, 18, 50f);
    }

    /// <summary>おまかせ2: 新しい部品もまぜて、種ごとに6つの場所へ部品を置く</summary>
    void RandomLayout2()
    {
        (float x, float y, bool wide)[] slots = [(-640f, -330f, false), (640f, -330f, false), (-640f, 300f, false), (640f, 300f, false), (0f, -420f, true), (0f, 400f, true)];
        int[] any = Enumerable.Range(0, 26).ToArray(), wide = [5, 9, 11, 12, 15, 18, 23, 24];
        var used = new HashSet<int>();
        for (int i = 0; i < slots.Length; i++)
        {
            var pool = slots[i].wide ? wide : any; int type = pool[0];
            for (int tries = 0; tries < 60; tries++) { int t = pool[(int)(R(i * 7 + tries, 190) * pool.Length) % pool.Length]; if (used.Add(t)) { type = t; break; } }
            SlotPart(type, slots[i].x, slots[i].y);
        }
        float b = R(1, 191);
        if (b < .34f) Band(470f, ey + 4f); else if (b < .67f) Ruler(490f, -940f, 940f, -1f); else DoubleFrame(24f);
    }

    void SlotPart2(int type, float cx, float cy)
    {
        switch (type)
        {
            case 13: NestSquares(cx, cy, 110f, 4); break;
            case 14: CellGrid(cx - 182f, cy - 78f, 7, 3, 52f); break;
            case 15: SpinSquares(cx - 160f, cy, 5, 26f, 80f); break;
            case 16: Star(cx, cy, 110f, st * MathF.PI / 10f, false); break;
            case 17: SunRays(cx, cy, 70f, 24); break;
            case 18: DiamondRow(cx - 160f, cy, 5, 20f, 80f); break;
            case 19: Spiral(cx, cy, 120f, 3f); break;
            case 20: EqBars(cx - 180f, cy + 90f, 10, 24f, 12f, 180f); break;
            case 21: Network(cx - 190f, cy - 100f, 380f, 200f, 8, (int)(cx + cy)); break;
            case 22: BigX(cx, cy, 90f); break;
            case 23: TriRow(cx - 180f, cy, 8, 60f); break;
            case 24: Progress(cx - 190f, cy - 25f, 380f, 50f, 8); break;
            default: DotFade(cx - 180f, cy - 60f, 10, 4, 40f); break;
        }
    }

    // ---------------------------------------------------------------- v126 で足した部品と配置
    /// <summary>五線(音符が拍ごとに左へ流れる)</summary>
    void Staff(float y, int key)
    {
        for (int l = 0; l < 5; l++) L(-940f, y + l * 18f, 940f, y + l * 18f, 1.4f, 15f);
        float run = st * 32f, sh = Mod(run, 130f); int off = (int)MathF.Floor(run / 130f);
        for (int i = 0; i < 16; i++)
        {
            float x = -900f + i * 130f - sh; if (x < -930f || x > 930f) continue;
            int id = i + off + key;
            if (id % 4 == 0) { L(x - 40f, y, x - 40f, y + 72f, 2f, 18f); continue; }
            float ny = y + 72f - (int)(R(id, 60) * 9f) * 9f;
            D(x, ny, 10f, 24f); L(x + 9f, ny, x + 9f, ny - 58f, 2f, 24f);
            if (R(id, 61) > .6f) L(x + 9f, ny - 58f, x + 30f, ny - 40f, 3f, 24f);
        }
    }

    void SymbolLayout()
    {
        Corners(28f, 60f);
        XGrid(-880f, -450f, 5, 3, 100f, 100f, 14f);
        DiamondRow(-200f, -440f, 6, 18f, 64f);
        Crosshair(620f, -260f, 110f);
        BigX(-600f, 150f, 70f);
        Chevrons(-880f, 360f, 6, 32f, 60f);
        Star(640f, 300f, 70f, st * MathF.PI / 10f, true);
    }

    void GaugeLayout()
    {
        Ruler(-480f, -940f, 940f, 1f);
        TickRing(-600f, -150f, 170f);
        Concentric(-600f, -150f, 110f, 4);
        TickRing(620f, 200f, 140f);
        HalfMoon(620f, 200f, 70f);
        Progress(-880f, 340f, 520f, 46f, 10);
    }

    void BandsLayout()
    {
        Band(-ey - 4f, -460f);
        Band(440f, ey + 4f);
        Checker(-880f, -400f, 26, 1, 34f);
        Checker(80f, 362f, 24, 1, 34f);
        Dashes(-940f, -300f, 940f, -300f, 30f, 16f);
        Dashes(-940f, 300f, 940f, 300f, 30f, 16f);
    }

    void BlueprintLayout()
    {
        PlusGrid(60f, 5f);
        DoubleFrame(30f);
        CellGrid(560f, -440f, 6, 4, 52f);
        Crosshair(-620f, -240f, 100f);
        Dashes(-900f, 380f, -300f, 140f, 24f, 12f);
        Ruler(440f, -900f, 900f, -1f);
    }

    void ScoreLayout()
    {
        Corners(28f, 60f);
        Staff(-420f, 0);
        Staff(300f, 50);
    }

    void MapLayout()
    {
        Corners(28f, 60f);
        Network(-900f, -470f, 800f, 320f, 16, 11);
        CircleRow(300f, -430f, 7, 14f, 60f);
        Crosshair(600f, 260f, 120f);
        Concentric(600f, 260f, 60f, 3);
        Dashes(-900f, 420f, -200f, 300f, 24f, 12f);
    }

    void SignalLayout()
    {
        CircleRow(-880f, -430f, 10, 18f, 56f);
        CircleRow(-880f, -370f, 9, 18f, 56f);
        EqBars(500f, -300f, 12, 18f, 10f, 150f);
        Barcode(-880f, 300f, 360f, 100f);
        Progress(380f, 330f, 520f, 40f, 10);
    }

    void AltLayout()
    {
        TriRow(-900f, -430f, 16, 64f);
        BigTri(640f, -160f, 120f, 0f);
        HalfMoon(-660f, -150f, 90f);
        SpinSquares(-880f, 330f, 9, 26f, 80f);
        DiamondRow(300f, 330f, 8, 18f, 70f);
    }

    // ---------------------------------------------------------------- v127 で足した部品と配置
    /// <summary>電子回路のような、折れ曲がった線と端の点</summary>
    void Traces(int n, int key)
    {
        for (int i = 0; i < n; i++)
        {
            int id = i + key; bool left = R(id, 70) < .5f;
            float y0 = (R(id, 71) - .5f) * 900f, x0 = left ? -940f : 940f, xm = (left ? -1f : 1f) * (300f + R(id, 72) * 500f), y1 = y0 + (R(id, 73) - .5f) * 300f;
            if (MathF.Abs(y1) < 160f) y1 = MathF.Sign(y1 == 0f ? 1f : y1) * 160f;
            L(x0, y0, xm, y0, 2f, 20f); L(xm, y0, xm, y1, 2f, 20f);
            if ((i + st) % 3 == 0) D(xm, y1, 8f, 24f); else O(xm, y1, 7f, 2f, 24f);
        }
    }

    /// <summary>時計の針(拍ごとに長い針が30度進む)</summary>
    void Hands(float cx, float cy, float r)
    {
        float a = st * MathF.PI / 6f - MathF.PI / 2f, b = st * MathF.PI / 72f - MathF.PI / 2f;
        L(cx, cy, cx + MathF.Cos(a) * r * .85f, cy + MathF.Sin(a) * r * .85f, 4f, 50f);
        L(cx, cy, cx + MathF.Cos(b) * r * .55f, cy + MathF.Sin(b) * r * .55f, 7f, 52f);
        D(cx, cy, 9f, 54f);
    }

    /// <summary>折れ線グラフ(拍ごとに点が1つ進む)</summary>
    void Graph(float x0, float y0, float w, float h, int n)
    {
        L(x0, y0 + h, x0 + w, y0 + h, LW, 20f); L(x0, y0, x0, y0 + h, LW, 20f);
        float px = 0f, py = 0f; int vis = 1 + st % n;
        for (int i = 0; i < vis; i++)
        {
            float x = x0 + i * w / (n - 1), y = y0 + h - h * (.15f + .8f * R(i + st / n * 37, 74));
            if (i > 0) L(px, py, x, y, 2.4f, 24f);
            D(x, y, 6f, 26f); px = x; py = y;
        }
    }

    void RecordLayout()
    {
        Barcode(-880f, -440f, 420f, 110f);
        Ruler(-280f, -940f, -360f, 1f);
        DotFade(380f, -440f, 12, 3, 40f);
        Progress(-880f, 330f, 500f, 46f, 10);
        EqBars(440f, 420f, 16, 20f, 8f, 130f);
    }

    void ConstellationLayout()
    {
        Corners(28f, 60f);
        Network(-900f, -460f, 760f, 300f, 10, 77);
        Network(140f, 120f, 760f, 320f, 10, 155);
        for (int i = 0; i < 12; i++)
        {
            float x = (R(i, 80) - .5f) * 1800f, y = (R(i, 81) - .5f) * 980f;
            if (MathF.Abs(x) < 420f && MathF.Abs(y) < 200f) continue;
            Star(x, y, 10f + R(i, 82) * 8f, i, true);
        }
    }

    void CircuitLayout()
    {
        CellGrid(-880f, -460f, 6, 3, 52f);
        Traces(12, 5);
        Checker(500f, 380f, 10, 2, 30f);
    }

    void ClockLayout()
    {
        TickRing(560f, 0f, 260f);
        Hands(560f, 0f, 220f);
        HalfMoon(-640f, -250f, 80f);
        Progress(-880f, 320f, 480f, 40f, 12);
    }

    void GraphPaperLayout()
    {
        PlusGrid(40f, 4f);
        Corners(28f, 60f);
        Crosshair(620f, -260f, 80f);
        Graph(-880f, 160f, 700f, 280f, 12);
    }

    void SunLayout()
    {
        SunRays(-560f, -120f, 140f, 32);
        Concentric(-560f, -120f, 90f, 4);
        SunRays(640f, 280f, 80f, 20);
        DiamondRow(400f, -430f, 7, 16f, 64f);
        Wave(-940f, 940f, 440f, 20f, 300f);
    }

    void WavesLayout()
    {
        Wave(-940f, 940f, -420f, 26f, 380f);
        Wave(-940f, 940f, -370f, 18f, 240f);
        Zigzag(-940f, 940f, -300f, 20f, 40f);
        Wave(-940f, 940f, 300f, 30f, 420f);
        Wave(-940f, 940f, 350f, 20f, 300f);
        Zigzag(-940f, 940f, 420f, 20f, 40f);
    }

    void BlocksLayout()
    {
        Checker(-880f, -460f, 8, 3, 40f);
        Tris(-860f, 120f, 3, 90f, 92f);
        SpinSquares(300f, -420f, 6, 28f, 90f);
        NestSquares(640f, 260f, 150f, 4);
    }
}
