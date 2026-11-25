using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1082〜1111(柄: 水玉・斜めストライプ・市松・格子・斜め格子・波線 × 5色)
//  画面いっぱいの板(地の色)に、模様が浮き彫りになって盛り上がる。2Dと同じ向きに流れる
// =====================================================================
internal static partial class Models3D
{
    /// <summary>多角形を四角の中に切り取る(画面の外へはみ出さないように)</summary>
    static List<Vector2> ClipRect(List<Vector2> poly, float x0, float y0, float x1, float y1)
    {
        List<Vector2> Cut(List<Vector2> src, Func<Vector2, float> f)
        {
            var o = new List<Vector2>();
            for (int i = 0; i < src.Count; i++)
            {
                var a = src[i]; var b = src[(i + 1) % src.Count];
                float fa = f(a), fb = f(b);
                if (fa >= 0) o.Add(a);
                if ((fa >= 0) != (fb >= 0)) o.Add(a + (b - a) * (fa / (fa - fb)));
            }
            return o;
        }
        var r = Cut(poly, v => v.X - x0);
        if (r.Count > 2) r = Cut(r, v => x1 - v.X);
        if (r.Count > 2) r = Cut(r, v => v.Y - y0);
        if (r.Count > 2) r = Cut(r, v => y1 - v.Y);
        return r;
    }

    /// <summary>線分を四角の中に切り取る。外なら false</summary>
    static bool ClipSeg(ref Vector2 a, ref Vector2 b, float x0, float y0, float x1, float y1)
    {
        float t0 = 0, t1 = 1; var dd = b - a;
        bool Edge(float p, float q)
        {
            if (MathF.Abs(p) < 1e-6f) return q >= 0;
            float r = q / p;
            if (p < 0) { if (r > t1) return false; if (r > t0) t0 = r; }
            else { if (r < t0) return false; if (r < t1) t1 = r; }
            return true;
        }
        if (!Edge(-dd.X, a.X - x0) || !Edge(dd.X, x1 - a.X) || !Edge(-dd.Y, a.Y - y0) || !Edge(dd.Y, y1 - a.Y)) return false;
        var na = a + dd * t0; b = a + dd * t1; a = na;
        return true;
    }

    /// <summary>柄の土台: 画面いっぱいの厚い板。表は z=0</summary>
    static void PatternBase(Sculpt m, Vector4 C, out float hw, out float hh)
    {
        hw = m.W / 2f; hh = m.H / 2f;
        m.Box(0, 0, -20, hw * 2f, hh * 2f, 40, C, .01f);
    }

    /// <summary>2Dの柄と同じ並べ方(ずらし・流れも同じ)</summary>
    static void PatternGrid(float hw, float hh, float o, float sx, float sy, bool stagger, Action<float, float, int, int> draw)
    {
        int rows = (int)((hh * 2f + sy * 4f) / sy), cols = (int)((hw * 2f + sx * 4f) / sx);
        for (int r = 0; r < rows; r++)
            for (int q = 0; q < cols; q++)
                draw(-hw - sx * 2f + q * sx + (stagger && r % 2 == 1 ? sx / 2f : 0f) + o, -hh - sy * 2f + r * sy, r, q);
    }

    static void Pattern3D(Sculpt m, float t, Vector4 C, Vector4 D, int kind)
    {
        PatternBase(m, C, out float hw, out float hh);
        const float u = 120f;
        float o = Frac(t * .08f) * u * 2f;
        int keep = m.Seg; m.Seg = 12;
        switch (kind)
        {
            case 0: // 水玉: 半球のつぶ
                PatternGrid(hw, hh, o, u, u * .8f, true, (x, y, _, _) =>
                {
                    if (MathF.Abs(x) > hw - 30 || MathF.Abs(y) > hh - 30) return;
                    m.Ball(x, y, 0, 30, 30, 20, D);
                });
                break;
            case 1: // 斜めストライプ: 斜めの盛り上がった帯
                for (float x = -hw - hh * 2f - u; x < hw + u * 2f; x += u)
                {
                    var poly = ClipRect(Pts(x + o, -hh - 4f, x + o + u * .5f, -hh - 4f, x + o + u * .5f + hh * 2f, hh + 4f, x + o + hh * 2f, hh + 4f), -hw, -hh, hw, hh);
                    if (poly.Count > 2) m.Slab(poly, 8, 16, D);
                }
                break;
            case 2: // 市松: 1つおきに高くなった四角
                PatternGrid(hw, hh, o, u, u, false, (x, y, r, q) =>
                {
                    if ((r + q) % 2 != 0) return;
                    float cx = x + u / 2f, cy = y + u / 2f;
                    float l = MathF.Max(-hw, cx - u / 2f), rr = MathF.Min(hw, cx + u / 2f), tp = MathF.Max(-hh, cy - u / 2f), bt = MathF.Min(hh, cy + u / 2f);
                    if (rr - l < 2 || bt - tp < 2) return;
                    m.Box((l + rr) / 2f, (tp + bt) / 2f, 12, rr - l, bt - tp, 24, D, .06f);
                });
                break;
            case 3: // 格子: たてと横の細い畝
                PatternGrid(hw, hh, o, u, u, false, (x, y, r, q) =>
                {
                    if (r == 0 && x > -hw + 3 && x < hw - 3) m.Box(x, 0, 6, 8, hh * 2f, 12, D, .3f);
                    if (q == 0 && y > -hh + 3 && y < hh - 3) m.Box(0, y, 6, hw * 2f, 8, 12, D, .3f);
                });
                break;
            case 4: // 斜め格子: 交差する丸い細い棒
                for (float x = -hw - hh * 2f - u; x < hw + hh * 2f + u; x += u)
                {
                    foreach (var (ax, bx) in new[] { (x + o, x + o + hh * 2f), (x + o + hh * 2f, x + o) })
                    {
                        Vector2 a = new(ax, -hh), b = new(bx, hh);
                        if (ClipSeg(ref a, ref b, -hw + 4, -hh + 4, hw - 4, hh - 4)) m.Limb(a.X, a.Y, 0, b.X, b.Y, 0, 4.5f, 4.5f, D);
                    }
                }
                break;
            default: // 波線: ゆれる丸い管
                for (float y = -hh; y <= hh + u; y += u * .6f)
                {
                    if (y > hh - 30) break;
                    var w = new List<Vector3>();
                    for (float x = -hw + 6f; x <= hw - 6f; x += 18f) w.Add(new(x, y + MathF.Sin((x + o) * .052f) * 22f, 0));
                    m.Tube(w, 5, D);
                }
                break;
        }
        m.Seg = keep;
    }

    static void Fam(Dictionary<Type, QuickModel> d, Type type, Action<Sculpt, float, Vector4, Vector4> draw) =>
        d[type] = (p, m, t, v, v2) => draw(m, t, m.C(p.Color1), m.C(p.Color2));

    static partial void RegisterF1a(Dictionary<Type, QuickModel> d)
    {
        Type[] types =
        [
            typeof(S1082Parameter), typeof(S1083Parameter), typeof(S1084Parameter), typeof(S1085Parameter), typeof(S1086Parameter),
            typeof(S1087Parameter), typeof(S1088Parameter), typeof(S1089Parameter), typeof(S1090Parameter), typeof(S1091Parameter),
            typeof(S1092Parameter), typeof(S1093Parameter), typeof(S1094Parameter), typeof(S1095Parameter), typeof(S1096Parameter),
            typeof(S1097Parameter), typeof(S1098Parameter), typeof(S1099Parameter), typeof(S1100Parameter), typeof(S1101Parameter),
            typeof(S1102Parameter), typeof(S1103Parameter), typeof(S1104Parameter), typeof(S1105Parameter), typeof(S1106Parameter),
            typeof(S1107Parameter), typeof(S1108Parameter), typeof(S1109Parameter), typeof(S1110Parameter), typeof(S1111Parameter),
        ];
        for (int i = 0; i < types.Length; i++) { int kind = i / 5; Fam(d, types[i], (m, t, C, D) => Pattern3D(m, t, C, D, kind)); }
    }
}
