using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 218番 1062〜1081・1501〜1510
//   図解 相関図〜温度計(Diagram3D) / 天気記号 20種類(ふくらんだ雲・光る太陽・落ちる雨粒や雪)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>天気記号: 2Dと同じく、太陽・月・雲・雨・雪・雷・霧・風・台風を組み合わせる(a はその組み合わせ)</summary>
    static void Weather3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = new Vector4(1, 1, 1, C.W);
        bool sun = (a & 1) != 0, moon = (a & 2) != 0, cloud = (a & 4) != 0, dark = (a & 8) != 0, rain = (a & 16) != 0, heavy = (a & 32) != 0,
             snow = (a & 64) != 0, bolt = (a & 128) != 0, fog = (a & 256) != 0, wind = (a & 512) != 0, swirl = (a & 1024) != 0;
        float sx = cloud ? -130 : 0, sy = cloud ? -120 : 0, sr = cloud ? 112 : 165, bz = cloud ? -60 : 0;
        if (sun)
        {
            var y = m.C(0xFFC93C);
            for (int i = 0; i < 10; i++) { float mm = i * .6283f + t * .3f; m.Limb(sx + MathF.Cos(mm) * (sr + 26), sy + MathF.Sin(mm) * (sr + 26), bz, sx + MathF.Cos(mm) * (sr + 76), sy + MathF.Sin(mm) * (sr + 76), bz, 11, 3, y); }
            m.Ball(sx, sy, bz, sr, y);
        }
        if (moon)
        {
            var q = new List<float>(); float z = cloud ? .68f : 1f;
            for (int i = 0; i <= 40; i++) { float mm = .95f + i * .1095f; q.Add(sx + MathF.Cos(mm) * 170 * z); q.Add(sy + MathF.Sin(mm) * 170 * z); }
            for (int i = 1; i < 40; i++) { float u = -1f + i / 20f; q.Add(sx + (98 - 136 * (1 - u * u)) * z); q.Add(sy + u * 139 * z); }
            Puff(m, bz, 90 * z, m.C(0xFFE58A), [.. q]);
        }
        if (swirl)
        {
            for (int k = 0; k < 3; k++) { float a0 = t * 1.2f + k * 2.1f; m.Tube(ArcPts3(0, 0, 70 + k * 72, a0, a0 + 4.1f, (k - 1) * 20f, 24), 17, k % 2 == 0 ? C : D); }
            m.Ball(0, 0, 0, 34, W);
        }
        if (cloud)
        {
            var c0 = dark ? m.C(0x6B7482) : m.C(0xE6EBF2);
            m.Ball(-125, 22, 0, 92, 92, 80, c0); m.Ball(0, -32, 0, 132, 132, 105, c0); m.Ball(135, 22, 0, 100, 100, 84, c0);
            m.Box(5, 78, 0, 440, 110, 150, c0, .5f);
        }
        float ry = cloud ? 150 : -120;
        if (rain || heavy)
        {
            int n = heavy ? 9 : 5;
            for (int i = 0; i < n; i++)
            {
                float k = Frac(t * 1.1f + i * .37f), x = -170 + i * 340f / Math.Max(1, n - 1), y = ry + k * 120, zz = (i % 3 - 1) * 40;
                var col = m.C(0x4FB8F2, 1f - k * .5f);
                m.Limb(x + 14, y, zz, x - 4, y + 36, zz, heavy ? 3 : 2, heavy ? 8 : 6, col);
                m.Ball(x - 6, y + 40, zz, heavy ? 8 : 6, heavy ? 10 : 8, heavy ? 8 : 6, col);
            }
        }
        if (snow)
            for (int i = 0; i < 5; i++)
            {
                float k = Frac(t * .55f + i * .31f), x = -170 + i * 85 + MathF.Sin(t + i) * 10, y = ry + k * 130, zz = (i % 3 - 1) * 40;
                var col = m.C(0xFFFFFF, 1f - k * .4f);
                m.Move(0, 0, zz, () => m.Turn(x, y, t, () => Gem(m, Pts(StarPts(x, y, 25, 6, .32f, 0f)), x, y, 0f, 2, 8, col, col)));
            }
        if (bolt) Puff(m, 70, 30, m.C(0xFFD23F), 35, 95, -62, 214, 2, 214, -42, 318, 88, 178, 16, 178, 74, 95);
        if (fog) for (int i = 0; i < 4; i++) { float o = MathF.Sin(t + i) * 22; m.Box(o, (cloud ? 170 : -90) + i * 56, 60 - i * 30, 480 - i % 2 * 90, 24, 24, m.C(0xC8D0D8, .85f), .5f); }
        if (wind)
            for (int i = 0; i < 3; i++)
            {
                float y = -100 + i * 100, o = MathF.Sin(t * 1.6f + i) * 24, z = (i - 1) * 30;
                var p = new List<Vector3> { new(-240 + o, y, z) };
                p.AddRange(ArcPts3(120 + o, y - 52, 52, 1.57f, -2.6f, z, 12));
                m.Tube(p, 9, W); m.Ball(-240 + o, y, z, 9, W);
            }
    }

    static partial void RegisterG1c(Dictionary<Type, QuickModel> d)
    {
        Type[] diagrams =
        [
            typeof(X1062Parameter), typeof(X1063Parameter), typeof(X1064Parameter), typeof(X1065Parameter), typeof(X1066Parameter),
            typeof(X1067Parameter), typeof(X1068Parameter), typeof(X1069Parameter), typeof(X1070Parameter), typeof(X1071Parameter),
        ];
        for (int i = 0; i < diagrams.Length; i++) { int a = 20 + i; Fam(d, diagrams[i], (m, t, C, D) => Diagram3D(m, t, C, D, a)); }
        (Type type, int a)[] weather =
        [
            (typeof(X1072Parameter), 1), (typeof(X1073Parameter), 4), (typeof(X1074Parameter), 28), (typeof(X1075Parameter), 44), (typeof(X1076Parameter), 68),
            (typeof(X1077Parameter), 332), (typeof(X1078Parameter), 140), (typeof(X1079Parameter), 156), (typeof(X1080Parameter), 256), (typeof(X1081Parameter), 260),
            (typeof(X1501Parameter), 512), (typeof(X1502Parameter), 92), (typeof(X1503Parameter), 5), (typeof(X1504Parameter), 21), (typeof(X1505Parameter), 69),
            (typeof(X1506Parameter), 2), (typeof(X1507Parameter), 6), (typeof(X1508Parameter), 22), (typeof(X1509Parameter), 12), (typeof(X1510Parameter), 1024),
        ];
        foreach (var (type, a) in weather) Fam(d, type, (m, t, C, D) => Weather3D(m, t, C, D, a));
    }
}
