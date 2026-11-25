using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1262〜1291
//   星座(いて座〜うお座) / 時計 1〜12時(掛け時計) / 円グラフ 10〜100%(飛び出す扇) / 進捗バー 10〜40%
// =====================================================================
internal static partial class Models3D
{
    /// <summary>掛け時計: 厚いふち、へこんだ白い文字盤、目盛り、高さの違う短針・長針、中心の軸、ガラス</summary>
    static void WallClock(Sculpt m, Vector4 C, Vector4 D, int hour, int minute)
    {
        var K = m.C(0x20242B); var W = new Vector4(1, 1, 1, C.W);
        m.Disc(0, 0, -10, 250, 250, 70, C, .3f);
        m.Torus(0, 0, 22, 232, 18, C);
        m.Disc(0, 0, 30, 214, 214, 10, W, .1f);
        for (int i = 0; i < 12; i++)
        {
            float a = i * .5236f, r0 = i % 3 == 0 ? 170 : 188, w = i % 3 == 0 ? 10 : 5;
            var p0 = new Vector2(MathF.Cos(a) * r0, MathF.Sin(a) * r0); var p1 = new Vector2(MathF.Cos(a) * 202, MathF.Sin(a) * 202);
            m.Turn((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, a, () => m.Box((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2, 37, Vector2.Distance(p0, p1), w, 6, K, .3f));
        }
        float h = (hour + minute / 60f) * .5236f - 1.5708f, mn = minute * .10472f - 1.5708f;
        m.Limb(0, 0, 44, MathF.Cos(h) * 112, MathF.Sin(h) * 112, 44, 9, 6, K);
        m.Limb(0, 0, 54, MathF.Cos(mn) * 165, MathF.Sin(mn) * 165, 54, 5.5f, 3.5f, D);
        m.Ball(0, 0, 58, 18, 18, 10, K);
        m.Ball(0, 0, 36, 214, 214, 40, m.C(0xFFFFFF, .1f));
        m.Tube(ArcPts3(0, 0, 180, 3.6f, 4.5f, 70, 8), 5, m.C(0xFFFFFF, .5f));
    }

    /// <summary>円グラフ: 灰色の土台、その割合だけ高く飛び出した色の扇、まん中の白い円柱と数字</summary>
    static void PieChart3D(Sculpt m, Vector4 C, Vector4 D, int pct)
    {
        var K = m.C(0x20242B); var W = new Vector4(1, 1, 1, C.W);
        float a0 = -1.5708f, a1 = a0 + pct / 100f * MathF.Tau;
        if (pct < 100) m.Slab(0, 50, D, PiePts(0, 0, 240, a1, a0 + MathF.Tau, 40));
        if (pct > 0)
        {
            float mid = (a0 + a1) / 2f, off = pct < 100 ? 14f : 0f;
            m.Move(MathF.Cos(mid) * off, MathF.Sin(mid) * off, () => m.Slab(15, 80, C, PiePts(0, 0, 240, a0, a1, Math.Max(4, pct * 40 / 100))));
        }
        m.Disc(0, 0, 30, 118, 118, 120, W, .15f);
        m.Text(pct + "%", 82, 0, 0, 92, 10, K);
    }

    /// <summary>進捗バー: 丸い溝のレーン、中を進む色の棒、上に浮かぶ数字</summary>
    static void ProgressBar3D(Sculpt m, Vector4 C, Vector4 D, int pct)
    {
        m.Box(0, 0, 0, 720, 112, 80, D, .5f);
        float w = MathF.Max(84f, pct * 6.8f);
        m.Box(-340 + pct * 3.4f, 0, 20, w, 84, 70, C, .5f);
        m.Box(-340 + pct * 3.4f, -18, 54, w - 40, 14, 4, m.C(0xFFFFFF, .35f), .5f);
        m.Text(pct + "%", 92, 0, -130, 20, 22, C);
    }

    static partial void RegisterF1g(Dictionary<Type, QuickModel> d)
    {
        Type[] stars = [typeof(S1262Parameter), typeof(S1263Parameter), typeof(S1264Parameter), typeof(S1265Parameter)];
        for (int i = 0; i < stars.Length; i++) { int a = 8 + i; FamT(d, stars[i], (m, t, C, D, s) => Constellation3D(m, t, C, D, a, s)); }
        Type[] clocks =
        [
            typeof(S1266Parameter), typeof(S1267Parameter), typeof(S1268Parameter), typeof(S1269Parameter), typeof(S1270Parameter), typeof(S1271Parameter),
            typeof(S1272Parameter), typeof(S1273Parameter), typeof(S1274Parameter), typeof(S1275Parameter), typeof(S1276Parameter), typeof(S1277Parameter),
        ];
        for (int i = 0; i < clocks.Length; i++) { int h = i + 1; Fam(d, clocks[i], (m, t, C, D) => WallClock(m, C, D, h, 0)); }
        Type[] pies =
        [
            typeof(S1278Parameter), typeof(S1279Parameter), typeof(S1280Parameter), typeof(S1281Parameter), typeof(S1282Parameter),
            typeof(S1283Parameter), typeof(S1284Parameter), typeof(S1285Parameter), typeof(S1286Parameter), typeof(S1287Parameter),
        ];
        for (int i = 0; i < pies.Length; i++) { int p = (i + 1) * 10; Fam(d, pies[i], (m, t, C, D) => PieChart3D(m, C, D, p)); }
        Type[] bars = [typeof(S1288Parameter), typeof(S1289Parameter), typeof(S1290Parameter), typeof(S1291Parameter)];
        for (int i = 0; i < bars.Length; i++) { int p = (i + 1) * 10; Fam(d, bars[i], (m, t, C, D) => ProgressBar3D(m, C, D, p)); }
    }
}
