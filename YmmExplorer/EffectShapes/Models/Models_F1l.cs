using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1412〜1441
//   矢印 札入り / 英字 A〜Z(角の丸い厚いタイル) / テロップ帯(1441〜1471 は v107 で追加)
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
        if (a is 4 or 8 or 10 && Lum(C) < .22f) C = D;    // 線だけの帯は、暗い色だと背景に沈むのでサブの色で
        switch (a)
        {
            case 1: // サイド線: 帯と、左に一段高い色の柱
                m.Box(0, 0, 0, w, h, 60, C, .06f);
                m.Box(-w / 2f + 14, 0, 10, 28, h + 8, 80, D, .2f);
                break;
            case 2: // 影つき: うしろにずれた色の板と、手前の帯
                m.Box(0, 12, -40, w, h, 30, D, .08f);
                m.Box(-10, 0, 0, w, h, 60, C, .06f);
                break;
            case 3: // 斜め: 平行四辺形の厚い帯
                m.Slab(0, 60, C, -w / 2f + 40, -h / 2f, w / 2f + 40, -h / 2f, w / 2f - 40, h / 2f, -w / 2f - 40, h / 2f);
                break;
            case 4: // 下線: 文字の下の太い棒だけ
                m.Box(0, h / 2f - 6, 0, w, 12, 20, C, .4f);
                z = 6; break;
            case 5: // 上下線: 少し透けた帯と、上下の線
                m.Box(0, 0, 0, w, h - 28, 50, C with { W = C.W * .86f }, .05f);
                m.Box(0, -h / 2f, 6, w, 9, 30, D, .4f); m.Box(0, h / 2f, 6, w, 9, 30, D, .4f);
                z = 27; break;
            case 6: // タブつき: 帯と、左上に飛び出たタブ
                m.Box(0, 12, 0, w, h, 60, C, .06f);
                m.Box(-w / 2f + 130, -h / 2f - 12, 16, 260, 58, 50, D, .3f);
                break;
            case 7: // カプセル: 丸い棒のような帯
                m.Box(0, 0, 0, w, h, 90, C, .5f);
                z = 46; break;
            case 8: // かっこ: 両端のかっこだけ
                foreach (int q in Sides)
                {
                    m.Tube([V3(q * (w / 2f - 70), -h / 2f), V3(q * w / 2f, -h / 2f), V3(q * w / 2f, h / 2f), V3(q * (w / 2f - 70), h / 2f)], 5.5f, C);
                    m.Ball(q * (w / 2f - 70), -h / 2f, 0, 5.5f, C); m.Ball(q * (w / 2f - 70), h / 2f, 0, 5.5f, C);
                }
                z = 6; break;
            case 9: // 流れるしま: 帯の上下のふちを流れる斜めのしま
                {
                    m.Box(0, 0, 0, w, h, 60, C, .06f);
                    float o = Frac(t * .4f) * 60f;
                    for (float x = -w / 2f + o; x < w / 2f - 22f; x += 60)
                    {
                        m.Slab(32, 6, D, x, h / 2f, x + 26, h / 2f, x + 42, h / 2f - 24, x + 16, h / 2f - 24);
                        m.Slab(32, 6, D, x, -h / 2f, x + 26, -h / 2f, x + 42, -h / 2f + 24, x + 16, -h / 2f + 24);
                    }
                    break;
                }
            case 10: // ネオン: うっすら光る板と、光る細い管の枠
                m.Box(0, 0, -6, w + 18, h + 18, 20, C with { W = C.W * .22f }, .2f);
                m.Tube(RoundRectPath(w, h, 26, 4), 4, C);
                m.Tube(RoundRectPath(w, h, 26, 4), 9, C with { W = C.W * .25f });
                z = 10; break;
            case 11: // 筆: 重なった丸い棒(長さが少しずつ違う)
                for (int i = 0; i < 9; i++) m.Box((H01(i + 3) - .5f) * 60f, -68 + i * 17f, i * 2f, w - H01(i) * 90f, 20, 26, C, .5f);
                z = 34; break;
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
        Type[] telops =
        [
            typeof(S1442Parameter), typeof(S1443Parameter), typeof(S1444Parameter), typeof(S1445Parameter), typeof(S1446Parameter), typeof(S1447Parameter),
            typeof(S1448Parameter), typeof(S1449Parameter), typeof(S1450Parameter), typeof(S1451Parameter), typeof(S1452Parameter), typeof(S1453Parameter),
            typeof(S1454Parameter), typeof(S1455Parameter), typeof(S1456Parameter), typeof(S1457Parameter), typeof(S1458Parameter), typeof(S1459Parameter),
            typeof(S1460Parameter), typeof(S1461Parameter), typeof(S1462Parameter), typeof(S1463Parameter), typeof(S1464Parameter), typeof(S1465Parameter),
            typeof(S1466Parameter), typeof(S1467Parameter), typeof(S1468Parameter), typeof(S1469Parameter), typeof(S1470Parameter), typeof(S1471Parameter),
        ];
        for (int i = 0; i < telops.Length; i++) { int a = (i + 1) / 3; FamT(d, telops[i], (m, t, C, D, s) => Telop3D(m, t, C, D, a, s)); }
    }
}
