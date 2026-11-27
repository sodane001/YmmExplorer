using System.Numerics;

namespace YMM4EffectShapes;

/// <summary>
/// 本物の3Dの形(三角形)をそのまま持つ入れ物。2Dの絵を立体にするのではなく、立体そのものを渡したい図形が使う。
/// 座標は3D描画の世界(単位はpx、左手系)。カメラは (0,0,-Cam) から +Z を見ていて、Z=0 の面が画面と同じ大きさに見える
/// </summary>
internal sealed class Native3D
{
    internal readonly record struct Group(int Start, int Count, Vector3 Center, bool Opaque);

    // 初めは小さく持つ(使い回すので、2回目からは前の大きさのまま使える)
    public readonly List<Vert3D> Verts = new(256);
    public readonly List<Group> Groups = new(16);
    /// <summary>カメラから画面(Z=0)までの距離(px)。画面の高さとあわせて、見え方の広さが決まる</summary>
    public float Cam = 1000f;
    /// <summary>ふだんの画面で回す時の中心(Z)</summary>
    public float PivotZ;
    /// <summary>ふだんの画面で回す時の中心(X・Y)</summary>
    public Vector2 PivotXY;
    /// <summary>true: 面の向きで光を当てる(専用3Dモデル)。false: 色に光が入っている(PV背景)</summary>
    public bool Lit;
    public float Width = 1920f, Height = 1080f;
    public Vector3 Min = new(float.MaxValue), Max = new(float.MinValue);

    int groupStart = -1, groupPoints;
    Vector3 groupSum;
    bool groupOpaque;
    readonly HashSet<(long, long)> edges = [];

    public bool IsEmpty => Verts.Count == 0;

    /// <summary>中身を空にして、作り直せるようにする(配列の大きさはそのまま残すので、毎回の確保が要らない)</summary>
    public void Reset()
    {
        Verts.Clear(); Groups.Clear(); edges.Clear();
        Cam = 1000f; PivotZ = 0f; PivotXY = default; Lit = false;
        Width = 1920f; Height = 1080f;
        Min = new Vector3(float.MaxValue); Max = new Vector3(float.MinValue);
        groupStart = -1; groupPoints = 0; groupSum = default; groupOpaque = false;
    }

    /// <summary>1つの立体のまとまりを始める(半透明の時は、まとまりごとに奥から重ねる)</summary>
    public void Begin()
    {
        if (groupStart >= 0) End();
        groupStart = Verts.Count; groupSum = Vector3.Zero; groupPoints = 0; groupOpaque = true;
        edges.Clear();
    }

    public void End()
    {
        if (groupStart < 0) return;
        int c = Verts.Count - groupStart;
        if (c > 0) Groups.Add(new Group(groupStart, c, groupPoints > 0 ? groupSum / groupPoints : Vector3.Zero, groupOpaque));
        groupStart = -1;
    }

    void Put(Vector3 p, Vector3 n, Vector4 c)
    {
        Verts.Add(new Vert3D(p, n, c));
        Min = Vector3.Min(Min, p); Max = Vector3.Max(Max, p);
        groupSum += p; groupPoints++;
        if (c.W < 0.995f) groupOpaque = false;
    }

    /// <summary>頂点ごとの向き付きの三角形(なめらかな陰になる)</summary>
    public void TriN(Vector3 a, Vector3 na, Vector3 b, Vector3 nb, Vector3 c, Vector3 nc, Vector4 col)
    {
        bool own = groupStart < 0;
        if (own) Begin();
        Put(a, na, col); Put(b, nb, col); Put(c, nc, col);
        if (own) End();
    }

    public void Tri(Vector3 a, Vector3 b, Vector3 c, Vector3 n, Vector4 col)
    {
        bool own = groupStart < 0;
        if (own) Begin();
        Put(a, n, col); Put(b, n, col); Put(c, n, col);
        if (own) End();
    }

    /// <summary>平らな多角形(扇形に三角形へ分ける)</summary>
    public void Poly(ReadOnlySpan<Vector3> p, Vector3 n, Vector4 col)
    {
        if (p.Length < 3 || col.W <= 0.002f) return;
        bool own = groupStart < 0;
        if (own) Begin();
        if (p.Length == 3) { Put(p[0], n, col); Put(p[1], n, col); Put(p[2], n, col); }
        else
        {
            var ctr = Vector3.Zero;
            foreach (var q in p) ctr += q;
            ctr /= p.Length;
            for (int i = 0; i < p.Length; i++) { Put(ctr, n, col); Put(p[i], n, col); Put(p[(i + 1) % p.Length], n, col); }
        }
        if (own) End();
    }

    /// <summary>上下で色の変わる四角(背景の板)</summary>
    public void Quad(Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Vector4 top, Vector4 bottom)
    {
        bool own = groupStart < 0;
        if (own) Begin();
        var n = new Vector3(0, 0, -1);
        Put(tl, n, top); Put(tr, n, top); Put(br, n, bottom);
        Put(tl, n, top); Put(br, n, bottom); Put(bl, n, bottom);
        if (own) End();
    }

    static long Key(Vector3 v) => unchecked(((long)MathF.Round(v.X * 4f) * 73856093L) ^ ((long)MathF.Round(v.Y * 4f) * 19349663L) ^ ((long)MathF.Round(v.Z * 4f) * 83492791L));

    /// <summary>辺を細い四角い棒にする(線で描く描き方用)。同じまとまりの中で同じ辺は1回だけ</summary>
    public void Edge(Vector3 a, Vector3 b, float width, Vector4 col)
    {
        if (width <= 0.01f || col.W <= 0.002f) return;
        long ka = Key(a), kb = Key(b);
        if (!edges.Add(ka < kb ? (ka, kb) : (kb, ka))) return;
        var d = b - a;
        float len = d.Length();
        if (len < 1e-4f) return;
        d /= len;
        var side = MathF.Abs(d.Y) < 0.9f ? Vector3.UnitY : Vector3.UnitX;
        var u = Vector3.Normalize(Vector3.Cross(d, side)) * (width / 2f);
        var v = Vector3.Normalize(Vector3.Cross(d, u)) * (width / 2f);
        Span<Vector3> r = [u + v, u - v, -u - v, -u + v];
        for (int i = 0; i < 4; i++)
        {
            var p0 = r[i]; var p1 = r[(i + 1) % 4];
            var n = Vector3.Normalize(p0 + p1);
            Put(a + p0, n, col); Put(b + p0, n, col); Put(b + p1, n, col);
            Put(a + p0, n, col); Put(b + p1, n, col); Put(a + p1, n, col);
        }
    }

    /// <summary>点(小さな八面体)</summary>
    public void Dot(Vector3 p, float r, Vector4 col)
    {
        if (r <= 0.01f || col.W <= 0.002f) return;
        Span<Vector3> ax = [new(r, 0, 0), new(0, r, 0), new(-r, 0, 0), new(0, -r, 0)];
        var f = new Vector3(0, 0, r);
        for (int i = 0; i < 4; i++)
        {
            var a = ax[i]; var b = ax[(i + 1) % 4];
            var n1 = Vector3.Normalize(a + b + f); var n2 = Vector3.Normalize(a + b - f);
            Put(p + a, n1, col); Put(p + b, n1, col); Put(p + f, n1, col);
            Put(p + b, n2, col); Put(p + a, n2, col); Put(p - f, n2, col);
        }
    }
}
