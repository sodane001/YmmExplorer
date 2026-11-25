using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1142〜1171
//   柄: ひし形・星 × 5色 / 顔: 20種類(球の頭の正面に、目・口・汗や涙が沿う)
// =====================================================================
internal static partial class Models3D
{
    static void Pattern3D3(Sculpt m, float t, Vector4 C, Vector4 D, int kind)
    {
        PatternBase(m, C, out float hw, out float hh);
        const float u = 120f;
        float o = Frac(t * .08f) * u * 2f;
        var lit = Vector4.Lerp(D, new Vector4(1, 1, 1, D.W), .25f);
        if (kind == 12) // ひし形: 面取りした宝石のようなひし形
            PatternGrid(hw, hh, o, u, u * .7f, true, (x, y, _, _) =>
            {
                float s = u * .34f;
                if (MathF.Abs(x) > hw - s || MathF.Abs(y) > hh - s) return;
                Gem(m, Pts(x, y - s, x + s, y, x, y + s, x - s, y), x, y, .45f, 4, 16, D, lit);
            });
        else // 星: とがった立体の星(並びごとに少し回っている)
            PatternGrid(hw, hh, o, u * 1.1f, u * .9f, true, (x, y, r, q) =>
            {
                if (MathF.Abs(x) > hw - 42 || MathF.Abs(y) > hh - 42) return;
                Gem(m, Pts(StarPts(x, y, 42, 5, .45f, (r + q) * .4f)), x, y, 0f, 3, 22, D, D);
            });
    }

    // ---------------------------------------------------------------- 顔
    const float FR = 250f, FZ = 225f;
    static float FaceZ(float x, float y) => Sculpt.FrontZ(0, 0, 0, FR, FR, FZ, x, y);
    static List<Vector3> OnFace(IEnumerable<Vector2> pts, float lift = 2f) => Sculpt.OnFront(0, 0, 0, FR, FR, FZ, lift, pts);
    static void FaceLine(Sculpt m, IEnumerable<Vector2> pts, float w, Vector4 col) { var p = OnFace(pts, w * .2f); m.Tube(p, w / 2f, col); m.Ball(p[0].X, p[0].Y, p[0].Z, w / 2f, col); m.Ball(p[^1].X, p[^1].Y, p[^1].Z, w / 2f, col); }
    static IEnumerable<Vector2> Seg2(float x0, float y0, float x1, float y1, int n = 8) { for (int i = 0; i <= n; i++) yield return new Vector2(x0 + (x1 - x0) * i / n, y0 + (y1 - y0) * i / n); }
    /// <summary>顔の表面に貼りつく、ひらたい玉</summary>
    static void FaceDot(Sculpt m, float x, float y, float rx, float ry, float th, Vector4 col, float lift = 0f) =>
        m.Ball(x, y, FaceZ(x, y) - th * .4f + lift, rx, ry, th, col);

    static void Face3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, int b, int c)
    {
        var K = m.C(0x20242B); var W = new Vector4(1, 1, 1, m.C(0xFFFFFF).W); var R = m.C(0xE5294F); var B = m.C(0x6CCBF5);
        m.Move(0, MathF.Sin(t * 2f) * 8f, () =>
        {
            m.Ball(0, 0, 0, FR, FR, FZ, C);
            m.Tube(OnFace(Sculpt.ArcPts(0, 0, 225, 3.6f, 4.6f, 10), 3), 6, m.C(0xFFFFFF, .35f));
            // 目
            foreach (int q in Sides)
            {
                float x = q * 92, y = -55;
                switch (a)
                {
                    case 1: FaceLine(m, Sculpt.ArcPts(x, y + 18, 40, 3.5f, 5.92f, 10), 14, K); break;
                    case 2: FaceLine(m, Seg2(x - 40, y, x + 40, y), 13, K); break;
                    case 3:
                        FaceDot(m, x, y, 56, 56, 26, W);
                        FaceDot(m, x, y + 6, 26, 26, 12, K, 22);
                        FaceDot(m, x - 8, y - 4, 8, 8, 5, W, 32);
                        break;
                    case 4:
                        FaceDot(m, x, y + 12, 22, 22, 12, K, 4);
                        FaceLine(m, Seg2(x + q * 48, y - 42, x - q * 48, y - 14), 15, K);
                        break;
                    case 5: FaceLine(m, Seg2(x - 32, y - 32, x + 32, y + 32), 13, K); FaceLine(m, Seg2(x + 32, y - 32, x - 32, y + 32), 13, K); break;
                    case 6: { float z = FaceZ(x, y); Puff(m, z + 4, 18, R, HeartPts(x, y - 4, .24f)); break; }
                    case 7:
                        {
                            float z = FaceZ(x, y) + 4;
                            m.Move(0, 0, z, () => m.Turn(x, y, t, () => { Gem(m, Pts(StarPts(x, y, 50, 4, .32f, 0f)), x, y, 0f, 4, 16, K, K); m.Move(0, 0, 12, () => Gem(m, Pts(StarPts(x, y, 26, 4, .32f, 0f)), x, y, 0f, 3, 10, W, W)); }));
                            break;
                        }
                    default:
                        FaceDot(m, x, y, 25, 25, 14, K, 6);
                        FaceDot(m, x - 8, y - 9, 8, 8, 4, W, 18);
                        break;
                }
            }
            // 口
            switch (b)
            {
                case 1:
                    {
                        var pie = new List<float> { 0, 65 };
                        foreach (var v in Sculpt.ArcPts(0, 65, 108, 0, MathF.PI, 16)) { pie.Add(v.X); pie.Add(v.Y); }
                        m.Slab(FaceZ(0, 110) - 14, 34, K, [.. pie]);
                        FaceDot(m, 0, 135, 58, 26, 14, m.C(0xE5546F), 10);
                        break;
                    }
                case 2: FaceLine(m, Seg2(-60, 105, 60, 105), 14, K); break;
                case 3: FaceLine(m, Sculpt.ArcPts(0, 165, 72, 3.65f, 5.77f, 12), 14, K); break;
                case 4: FaceDot(m, 0, 110, 34, 42, 16, K, 6); break;
                case 5: FaceLine(m, Enumerable.Range(0, 25).Select(i => new Vector2(-80 + i * 6.67f, 105 + MathF.Sin(i * 1.05f) * 13f)), 12, K); break;
                case 6:
                    {
                        for (int i = 0; i < 6; i++) { float xc = -70 + i * 28; m.Box(xc, 108, FaceZ(xc, 108), 26, 62, 18, W, .25f); }
                        FaceLine(m, [new(-85, 75), new(-40, 75), new(0, 75), new(40, 75), new(85, 75), new(85, 141), new(40, 141), new(0, 141), new(-40, 141), new(-85, 141), new(-85, 75)], 8, K);
                        break;
                    }
                case 7:
                    FaceLine(m, Sculpt.ArcPts(0, 62, 72, .35f, 2.8f, 12), 13, K);
                    m.Ball(28, 135, FaceZ(28, 128) + 10, 22, 28, 18, m.C(0xE5546F));
                    break;
                default: FaceLine(m, Sculpt.ArcPts(0, 58, 78, .35f, 2.8f, 12), 14, K); break;
            }
            // おまけ(汗・ほお・涙・怒りマーク・Z・きらきら)
            switch (c)
            {
                case 1:
                    {
                        float y = -130 + Frac(t * .6f) * 60f, x = 190;
                        float z = FaceZ(x, y + 10) + 6;
                        m.Ball(x, y + 18, z, 26, 26, 18, B);
                        m.Limb(x, y + 10, z, x, y - 34, z, 22, 2, B);
                        break;
                    }
                case 2: foreach (int q in Sides) FaceDot(m, q * 155, 45, 46, 26, 10, D, 2); break;
                case 3:
                    foreach (int q in Sides)
                    {
                        float k = Frac(t * .7f + (q + 1) * .25f), y = -10 + k * 140f, x = q * 112;
                        var col = m.C(0x6CCBF5, 1f - k * .6f);
                        m.Ball(x, y, FaceZ(x, y) + 8, 16, 28, 14, col);
                    }
                    break;
                case 4:
                    foreach (int q in Sides) foreach (int r in Sides)
                        {
                            float cx = 170 + q * 32, cy = -175 + r * 32;
                            m.Tube(OnFace(Sculpt.ArcPts(cx, cy, 24, q * r > 0 ? 0f : 1.57f, q * r > 0 ? 1.57f : 3.14f, 6), 8), 5.5f, R);
                        }
                    break;
                case 5:
                    for (int i = 0; i < 3; i++) { float k = Frac(t * .35f + i / 3f); m.Text("Z", 38 + k * 42f, 170 + k * 80f, -130 - k * 130f, 60 + k * 80f, 8, m.C(0x8FA8E8, 1f - k)); }
                    break;
                case 6:
                    for (int i = 0; i < 4; i++)
                    {
                        float mm = i * 1.5708f + .7f + t * .5f, x = MathF.Cos(mm) * 292, y = MathF.Sin(mm) * 262, z = MathF.Sin(mm * 2f) * 60f + 40f;
                        float s = 34 + MathF.Sin(t * 3f + i) * 10f;
                        m.Move(0, 0, z, () => Gem(m, Pts(StarPts(x, y, s, 4, .3f, 0f)), x, y, 0f, 3, 12, m.C(0xFFE27A), m.C(0xFFE27A)));
                    }
                    break;
            }
        });
    }

    static partial void RegisterF1c(Dictionary<Type, QuickModel> d)
    {
        Type[] pats =
        [
            typeof(S1142Parameter), typeof(S1143Parameter), typeof(S1144Parameter), typeof(S1145Parameter), typeof(S1146Parameter),
            typeof(S1147Parameter), typeof(S1148Parameter), typeof(S1149Parameter), typeof(S1150Parameter), typeof(S1151Parameter),
        ];
        for (int i = 0; i < pats.Length; i++) { int kind = 12 + i / 5; Fam(d, pats[i], (m, t, C, D) => Pattern3D3(m, t, C, D, kind)); }

        (Type type, int a, int b, int c)[] faces =
        [
            (typeof(S1152Parameter), 0, 0, 0), (typeof(S1153Parameter), 1, 1, 0), (typeof(S1154Parameter), 1, 0, 2), (typeof(S1155Parameter), 2, 0, 2),
            (typeof(S1156Parameter), 1, 7, 0), (typeof(S1157Parameter), 6, 0, 2), (typeof(S1158Parameter), 6, 1, 2), (typeof(S1159Parameter), 7, 1, 6),
            (typeof(S1160Parameter), 7, 0, 6), (typeof(S1161Parameter), 3, 4, 0), (typeof(S1162Parameter), 3, 2, 0), (typeof(S1163Parameter), 3, 4, 1),
            (typeof(S1164Parameter), 0, 5, 1), (typeof(S1165Parameter), 1, 6, 1), (typeof(S1166Parameter), 0, 2, 1), (typeof(S1167Parameter), 3, 5, 1),
            (typeof(S1168Parameter), 0, 3, 1), (typeof(S1169Parameter), 0, 3, 0), (typeof(S1170Parameter), 2, 3, 3), (typeof(S1171Parameter), 2, 1, 3),
        ];
        foreach (var (type, a, b, c) in faces) Fam(d, type, (m, t, C, D) => Face3D(m, t, C, D, a, b, c));
    }
}
