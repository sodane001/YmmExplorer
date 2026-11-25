using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1472〜1500(217番の最後)
//   テロップ帯 ネオン・筆風 / バッジ 星形・メダル・ロゼット・六角・札・盾(どくどく脈打つ)
// =====================================================================
internal static partial class Models3D
{
    static void Badge3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        var W = new Vector4(1, 1, 1, C.W);
        float k = 1f + MathF.Sin(t * 3f) * .025f, z = 40;
        m.Scale(0, 0, 0, k, () =>
        {
            switch (a)
            {
                case 1: // メダル: 厚い円盤、色の面、白い輪
                    m.Disc(0, 0, 0, 250, 250, 70, D, .3f);
                    m.Disc(0, 0, 12, 222, 222, 60, C, .3f);
                    m.Torus(0, 0, 42, 195, 5, W);
                    z = 46; break;
                case 2: // ロゼット: うしろへ垂れるリボンと、ぎざぎざの花形
                    m.Pitch(0, 70, -30, .35f, () =>
                    {
                        m.Slab(-30, 14, D, -115, 70, -30, 115, -65, 310, -105, 270, -165, 285);
                        m.Slab(-30, 14, D, 115, 70, 30, 115, 65, 310, 105, 270, 165, 285);
                    });
                    Gem(m, Pts(StarPts(0, -20, 225, 20, .9f, 0f)), 0, -20, .82f, 14, 36, C, Sculpt.Dark(C, .93f));
                    z = 40; break;
                case 3: // 六角: 面取りした六角の宝石
                    Gem(m, Pts(250, 0, 125, 216, -125, 216, -250, 0, -125, -216, 125, -216), 0, 0, .86f, 18, 40, D, C);
                    z = 44; break;
                case 4: // 札: 右がとがった厚い札と、白いはと目
                    Puff(m, 0, 60, C, -300, -130, 200, -130, 320, 0, 200, 130, -300, 130);
                    m.Torus(190, 0, 30, 24, 6, W);
                    z = 32; break;
                case 5: // 盾: 色のふちの盾と、一段高い面
                    m.Slab(0, 50, D, -225, -235, 225, -235, 225, 45, 0, 275, -225, 45);
                    m.Slab(14, 50, C, -195, -205, 195, -205, 195, 32, 0, 232, -195, 32);
                    z = 42; break;
                default: // 星形: ゆっくり回る、ぎざぎざの立体の星
                    m.Turn(0, 0, t * .2f, () => Gem(m, Pts(StarPts(0, 0, 268, 14, .78f, 0f)), 0, 0, .7f, 14, 40, C, Sculpt.Dark(C, .92f)));
                    z = 44; break;
            }
            float mw = a == 4 ? 410 : 320;
            m.Text(s, FitSize(s, mw, 130), a == 4 ? -45 : 0, a == 2 ? -20 : 0, z, 10, W, mw);
        });
    }

    static partial void RegisterF1m(Dictionary<Type, QuickModel> d)
    {
        Type[] telops = [typeof(S1472Parameter), typeof(S1473Parameter), typeof(S1474Parameter), typeof(S1475Parameter), typeof(S1476Parameter)];
        for (int i = 0; i < telops.Length; i++) { int a = i < 2 ? 10 : 11; FamT(d, telops[i], (m, t, C, D, s) => Telop3D(m, t, C, D, a, s)); }
        Type[] badges =
        [
            typeof(S1477Parameter), typeof(S1478Parameter), typeof(S1479Parameter), typeof(S1480Parameter), typeof(S1481Parameter), typeof(S1482Parameter),
            typeof(S1483Parameter), typeof(S1484Parameter), typeof(S1485Parameter), typeof(S1486Parameter), typeof(S1487Parameter), typeof(S1488Parameter),
            typeof(S1489Parameter), typeof(S1490Parameter), typeof(S1491Parameter), typeof(S1492Parameter), typeof(S1493Parameter), typeof(S1494Parameter),
            typeof(S1495Parameter), typeof(S1496Parameter), typeof(S1497Parameter), typeof(S1498Parameter), typeof(S1499Parameter), typeof(S1500Parameter),
        ];
        for (int i = 0; i < badges.Length; i++) { int a = i % 6; FamT(d, badges[i], (m, t, C, D, s) => Badge3D(m, t, C, D, a, s)); }
    }
}
