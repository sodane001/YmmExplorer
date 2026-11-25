using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1412〜1441
//   矢印 札入り / 英字 A〜Z(角の丸い厚いタイル) / テロップ帯 ベタ
// =====================================================================
internal static partial class Models3D
{
    /// <summary>矢印 札入り: 角の丸い厚い札と、浮き出た矢印</summary>
    static void ArrowCard3D(Sculpt m, Vector4 C, Vector4 D)
    {
        m.Box(0, 0, 0, 500, 300, 80, C, .3f);
        Puff(m, 46, 24, D, -120, -92, 40, -92, 40, -150, 190, 0, 40, 150, 40, 92, -120, 92);
    }

    /// <summary>テロップ帯: 画面幅の厚い帯と、手前の白い文字(形は a で変える)</summary>
    static void Telop3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        const float w = 1240, h = 170;
        var W = new Vector4(1, 1, 1, C.W); float z = 32;
        switch (a)
        {
            default:
                m.Box(0, 0, 0, w, h, 60, C, .06f);
                break;
        }
        m.Text(s, 82, 0, 0, z, 8, W, w - 110);
    }

    static partial void RegisterF1l(Dictionary<Type, QuickModel> d)
    {
        foreach (var ty in new[] { typeof(S1412Parameter), typeof(S1413Parameter), typeof(S1414Parameter) }) Fam(d, ty, (m, t, C, D) => ArrowCard3D(m, C, D));
        Type[] letters =
        [
            typeof(S1415Parameter), typeof(S1416Parameter), typeof(S1417Parameter), typeof(S1418Parameter), typeof(S1419Parameter), typeof(S1420Parameter),
            typeof(S1421Parameter), typeof(S1422Parameter), typeof(S1423Parameter), typeof(S1424Parameter), typeof(S1425Parameter), typeof(S1426Parameter),
            typeof(S1427Parameter), typeof(S1428Parameter), typeof(S1429Parameter), typeof(S1430Parameter), typeof(S1431Parameter), typeof(S1432Parameter),
            typeof(S1433Parameter), typeof(S1434Parameter), typeof(S1435Parameter), typeof(S1436Parameter), typeof(S1437Parameter), typeof(S1438Parameter),
            typeof(S1439Parameter), typeof(S1440Parameter),
        ];
        foreach (var ty in letters) FamT(d, ty, (m, t, C, D, s) => DayTile(m, C, D, s));
        FamT(d, typeof(S1441Parameter), (m, t, C, D, s) => Telop3D(m, t, C, D, 0, s));
    }
}
