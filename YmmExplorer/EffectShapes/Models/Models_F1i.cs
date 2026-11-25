using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1322〜1351
//   吹き出し 四角・横向き・カプセル / 帯 線入り・つばめ尾・折り返し・カプセル札・斜めの札・矢印札
// =====================================================================
internal static partial class Models3D
{
    /// <summary>吹き出し(残りの形): 四角・横向き(左にしっぽ)・カプセル</summary>
    static void Balloon3D2(Sculpt m, Vector4 C, int a, string s)
    {
        var K = m.C(0x20242B); var tc = Lum(C) > .55f ? K : new Vector4(1, 1, 1, C.W);
        float tx = 0;
        switch (a)
        {
            case 5: m.Box(0, -25, 0, 760, 320, 90, C, .08f); Puff(m, 0, 44, C, -210, 130, -60, 130, -255, 280); break;
            case 6: m.Box(35, -25, 0, 700, 310, 90, C, .3f); Puff(m, 0, 44, C, -300, -90, -300, 30, -430, -30); tx = 35; break;
            default: m.Box(0, -25, 0, 760, 310, 110, C, .5f); Puff(m, 0, 44, C, 128, 112, 252, 112, 148, 250); break;
        }
        m.Text(s, 82, tx, -25, a == 7 ? 57 : 47, 8, tc, 620);
    }

    /// <summary>帯: 見出しの札。形ごとに厚み・折り返し・飾りを付ける。文字は手前に白で</summary>
    static void Ribbon3D(Sculpt m, Vector4 C, Vector4 D, int a, string s)
    {
        var W = new Vector4(1, 1, 1, C.W); float z = 32;
        switch (a)
        {
            case 1: // つばめ尾: 両端が切れこんだ帯。端は少しうしろへ反る
                m.Slab(0, 60, C, -410, -82, 410, -82, 410, 82, -410, 82);
                foreach (int sd in Sides) m.Yaw(sd * 410, 0, 0, -sd * .35f, () => m.Slab(0, 60, C, sd * 410, -82, sd * 470, -82, sd * 410, 0, sd * 470, 82, sd * 410, 82));
                break;
            case 2: // 折り返し: うしろに回った両端の帯と、折り目の三角
                foreach (int sd in Sides)
                {
                    m.Slab(-50, 40, D, sd * 500, -30, sd * 350, -30, sd * 350, 112, sd * 500, 112, sd * 445, 42);
                    m.Quad3D(new(sd * 390, 82, -30), new(sd * 350, 82, -30), new(sd * 350, 112, -70), new(sd * 390, 112, -70), Sculpt.Dark(D, .7f));
                }
                m.Box(0, 0, 0, 780, 164, 64, C, .06f);
                break;
            case 3: // カプセル札: 丸い棒のような札と、左の白い輪
                m.Box(0, 0, 0, 900, 150, 90, C, .5f);
                m.Torus(-375, 0, 46, 30, 6, W);
                z = 46; break;
            case 4: // 斜めの札: 平行四辺形の札と、下に折れた影の三角
                m.Slab(0, 60, C, -440, -82, 470, -82, 440, 82, -470, 82);
                m.Slab(-36, 12, Sculpt.Dark(D, .8f), -470, 82, -440, 82, -440, 120);
                break;
            case 5: // 矢印札: 右がとがった札と、左の白い穴
                m.Slab(0, 60, C, -480, -80, 400, -80, 480, 0, 400, 80, -480, 80);
                m.Ball(-430, 0, 28, 18, 18, 8, W);
                break;
            default: // 線入り: 厚い帯と、上下に浮き出た線
                m.Box(0, 0, 0, 900, 150, 60, C, .05f);
                m.Box(0, -66, 32, 900, 10, 6, D, .3f); m.Box(0, 66, 32, 900, 10, 6, D, .3f);
                break;
        }
        m.Text(s, 78, 0, 0, z, 8, W, 700);
    }

    static partial void RegisterF1i(Dictionary<Type, QuickModel> d)
    {
        Type[] balloons =
        [
            typeof(S1322Parameter), typeof(S1323Parameter), typeof(S1324Parameter), typeof(S1325Parameter), typeof(S1326Parameter),
            typeof(S1327Parameter), typeof(S1328Parameter), typeof(S1329Parameter), typeof(S1330Parameter),
        ];
        for (int i = 0; i < balloons.Length; i++) { int a = 5 + i / 3; FamT(d, balloons[i], (m, t, C, D, s) => Balloon3D2(m, C, a, s)); }
        Type[] ribbons =
        [
            typeof(S1331Parameter), typeof(S1332Parameter), typeof(S1333Parameter), typeof(S1334Parameter),
            typeof(S1335Parameter), typeof(S1336Parameter), typeof(S1337Parameter), typeof(S1338Parameter),
            typeof(S1339Parameter), typeof(S1340Parameter), typeof(S1341Parameter), typeof(S1342Parameter),
            typeof(S1343Parameter), typeof(S1344Parameter), typeof(S1345Parameter), typeof(S1346Parameter),
            typeof(S1347Parameter), typeof(S1348Parameter), typeof(S1349Parameter), typeof(S1350Parameter),
            typeof(S1351Parameter),
        ];
        for (int i = 0; i < ribbons.Length; i++) { int a = i / 4; FamT(d, ribbons[i], (m, t, C, D, s) => Ribbon3D(m, C, D, a, s)); }
    }
}
