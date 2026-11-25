using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1112〜1141(柄: 青海波・麻の葉・亀甲・七宝・山形・うろこ × 5色)
//  画面いっぱいの板に、和柄が彫り物のように盛り上がる。2Dと同じ向きに流れる
// =====================================================================
internal static partial class Models3D
{
    /// <summary>三角の断面の細い畝(山形の稜線)。三角4枚で軽い</summary>
    static void Ridge(Sculpt m, Vector2 a, Vector2 b, float w, float h, Vector4 col)
    {
        var dd = b - a; if (dd.LengthSquared() < 1e-4f) return;
        var nn = Vector2.Normalize(new Vector2(-dd.Y, dd.X)) * (w / 2f);
        var at = new Vector3(a, h); var bt = new Vector3(b, h);
        m.Quad3D(new(a - nn, 0), new(b - nn, 0), bt, at, col);
        m.Quad3D(new(a + nn, 0), new(b + nn, 0), bt, at, col);
    }

    /// <summary>画面の中に入っている線分だけ畝にする</summary>
    static void RidgeIn(Sculpt m, Vector2 a, Vector2 b, float w, float h, Vector4 col, float hw, float hh)
    {
        if (ClipSeg(ref a, ref b, -hw + w, -hh + w, hw - w, hh - w)) Ridge(m, a, b, w, h, col);
    }

    /// <summary>点の並びを管にする。画面の外に出た所は切って、つながった所ごとに描く</summary>
    static void TubeIn(Sculpt m, IEnumerable<Vector2> pts, float z, float r, Vector4 col, float hw, float hh)
    {
        var run = new List<Vector3>();
        void Flush() { if (run.Count >= 2) m.Tube(run, r, col); run = []; }
        foreach (var p in pts)
        {
            if (MathF.Abs(p.X) <= hw - r && MathF.Abs(p.Y) <= hh - r) run.Add(new(p, z));
            else Flush();
        }
        Flush();
    }

    static void Pattern3D2(Sculpt m, float t, Vector4 C, Vector4 D, int kind)
    {
        PatternBase(m, C, out float hw, out float hh);
        const float u = 120f;
        float o = Frac(t * .08f) * u * 2f;
        int keep = m.Seg; m.Seg = 8;
        switch (kind)
        {
            case 6: // 青海波: 重なる波の半円(上半分の3重の弧)。下の段ほど手前に重なる
                PatternGrid(hw, hh, o, u * 1.5f, u * .375f, true, (x, y, r, _) =>
                {
                    if (y - u * .75f > hh || y < -hh - u) return;
                    for (int i = 1; i <= 3; i++) TubeIn(m, Sculpt.ArcPts(x, y, u * .75f * i / 3f, MathF.PI, MathF.Tau, 14), 2 + r * .4f, 3.2f, D, hw, hh);
                });
                break;
            case 7: // 麻の葉: 六角の中心から放射する細い畝
                PatternGrid(hw, hh, o, u * 1.6f, u * 1.386f, true, (x, y, _, _) =>
                {
                    float rr = u * .92f; var c0 = new Vector2(x, y);
                    if (MathF.Abs(x) > hw + rr || MathF.Abs(y) > hh + rr) return;
                    for (int i = 0; i < 6; i++)
                    {
                        float a0 = i * 1.0472f, a1 = a0 + 1.0472f;
                        var p0 = c0 + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * rr; var p1 = c0 + new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * rr;
                        RidgeIn(m, p0, p1, 7, 6, D, hw, hh);
                        RidgeIn(m, c0, p0, 7, 6, D, hw, hh);
                        RidgeIn(m, c0, c0 + new Vector2(MathF.Cos(a0 + .5236f), MathF.Sin(a0 + .5236f)) * rr * .866f, 6, 5, D, hw, hh);
                    }
                });
                break;
            case 8: // 亀甲: 六角の畝と、中が少し盛り上がった亀の甲羅
                var shell = Sculpt.Dark(C, .9f);
                PatternGrid(hw, hh, o, u * 1.5f, u * .866f, true, (x, y, _, _) =>
                {
                    float rr = u * .72f;
                    if (MathF.Abs(x) > hw + rr || MathF.Abs(y) > hh + rr) return;
                    var hex = new List<Vector2>();
                    for (int i = 0; i < 6; i++) { float a = i * 1.0472f; hex.Add(new(x + MathF.Cos(a) * rr, y + MathF.Sin(a) * rr)); }
                    for (int i = 0; i < 6; i++) RidgeIn(m, hex[i], hex[(i + 1) % 6], 10, 9, D, hw, hh);
                    var inner = ClipRect([.. hex.Select(v => new Vector2(x, y) + (v - new Vector2(x, y)) * .8f)], -hw, -hh, hw, hh);
                    if (inner.Count > 2) m.Slab(inner, 3, 6, shell);
                });
                break;
            case 9: // 七宝: 重なり合う丸い輪
                PatternGrid(hw, hh, o, u, u, false, (x, y, _, _) =>
                {
                    float rr = u * .71f;
                    if (MathF.Abs(x) > hw + rr || MathF.Abs(y) > hh + rr) return;
                    TubeIn(m, Sculpt.ArcPts(x, y, rr, 0, MathF.Tau, 28), 2, 3.4f, D, hw, hh);
                });
                break;
            case 10: // 山形: ぎざぎざに折れた厚い帯
                PatternGrid(hw, hh, o, u * .9f, u * .8f, false, (x, y, r, q) =>
                {
                    float f = q % 2 == 0 ? 1f : -1f;
                    var poly = ClipRect(Pts(x, y, x + u * .45f, y + u * .22f * f, x + u * .9f, y, x + u * .9f, y + u * .5f, x + u * .45f, y + u * .72f * f + (f < 0 ? u : 0f), x, y + u * .5f), -hw, -hh, hw, hh);
                    if (poly.Count > 2) m.Slab(poly, 7, 14, D);
                });
                break;
            default: // うろこ: 三角すいが並んだ、とがった鱗
                PatternGrid(hw, hh, o, u, u * .86f, true, (x, y, _, _) =>
                {
                    var tri = ClipRect(Pts(x, y - u * .4f, x + u * .46f, y + u * .4f, x - u * .46f, y + u * .4f), -hw, -hh, hw, hh);
                    if (tri.Count < 3) return;
                    var apex = new Vector3(Math.Clamp(x, -hw, hw), Math.Clamp(y + u * .13f, -hh, hh), 28);
                    for (int i = 0; i < tri.Count; i++) m.Tri3D(new(tri[i], 0), new(tri[(i + 1) % tri.Count], 0), apex, D);
                });
                break;
        }
        m.Seg = keep;
    }

    static partial void RegisterF1b(Dictionary<Type, QuickModel> d)
    {
        Type[] types =
        [
            typeof(S1112Parameter), typeof(S1113Parameter), typeof(S1114Parameter), typeof(S1115Parameter), typeof(S1116Parameter),
            typeof(S1117Parameter), typeof(S1118Parameter), typeof(S1119Parameter), typeof(S1120Parameter), typeof(S1121Parameter),
            typeof(S1122Parameter), typeof(S1123Parameter), typeof(S1124Parameter), typeof(S1125Parameter), typeof(S1126Parameter),
            typeof(S1127Parameter), typeof(S1128Parameter), typeof(S1129Parameter), typeof(S1130Parameter), typeof(S1131Parameter),
            typeof(S1132Parameter), typeof(S1133Parameter), typeof(S1134Parameter), typeof(S1135Parameter), typeof(S1136Parameter),
            typeof(S1137Parameter), typeof(S1138Parameter), typeof(S1139Parameter), typeof(S1140Parameter), typeof(S1141Parameter),
        ];
        for (int i = 0; i < types.Length; i++) { int kind = 6 + i / 5; Fam(d, types[i], (m, t, C, D) => Pattern3D2(m, t, C, D, kind)); }
    }
}
