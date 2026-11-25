using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1382〜1411
//   枠 かぎかっこ / 矢印 細い線・太い・両方向・カーブ・Uターン・くの字・三連・回転・斜め(厚みのある立体の矢印)
// =====================================================================
internal static partial class Models3D
{
    static void Arrow3D(Sculpt m, float t, Vector4 C, int a)
    {
        float mv = MathF.Sin(t * 3f) * 16f;
        switch (a)
        {
            case 1: m.Move(mv, 0, () => Puff(m, 0, 70, C, -250, -75, 70, -75, 70, -205, 310, 0, 70, 205, 70, 75, -250, 75)); break;
            case 2: Puff(m, 0, 60, C, -170, -43, 170, -43, 170, -130, 340, 0, 170, 130, 170, 43, -170, 43, -170, 130, -340, 0, -170, -130); break;
            case 3:
                {
                    var arc = ArcPts3(0, 130, 245, 3.45f, 5.5f, 0, 20);
                    m.Tube(arc, 27, C); m.Ball(arc[0].X, arc[0].Y, 0, 27, C);
                    Puff(m, 0, 54, C, 120, -80, 280, 120, 80, 150);
                    break;
                }
            case 4:
                {
                    var p = new List<Vector3> { V3(130, -220) };
                    p.AddRange(ArcPts3(0, -30, 130, 0, 3.1416f, 0, 16));
                    p.Add(V3(-130, -80));
                    m.Tube(p, 26, C); m.Ball(130, -220, 0, 26, C);
                    Puff(m, 0, 52, C, -240, -80, -20, -80, -130, -240);
                    break;
                }
            case 5: m.Move(mv, 0, () => Puff(m, 0, 70, C, -150, -220, 40, -220, 200, 0, 40, 220, -150, 220, 10, 0)); break;
            case 6:
                for (int i = 0; i < 3; i++)
                {
                    float x = -210 + i * 150, k = .35f + .65f * Math.Clamp(MathF.Sin(t * 4f - i) * .5f + .5f, 0f, 1f);
                    Puff(m, i * 12f, 50, C with { W = C.W * k }, x - 70, -180, x, -180, x + 100, 0, x, 180, x - 70, 180, x + 30, 0);
                }
                break;
            case 7:
                m.Turn(0, 0, t * 1.2f, () =>
                {
                    var arc = ArcPts3(0, 0, 190, -.2f, 4.8f, 0, 30);
                    m.Tube(arc, 25, C); m.Ball(arc[^1].X, arc[^1].Y, 0, 25, C);
                    Puff(m, 0, 50, C, 110, -230, 300, -140, 120, -30);
                });
                break;
            case 8: m.Turn(0, 0, -.7854f, () => m.Move(mv, 0, () => Puff(m, 0, 60, C, -250, -38, 70, -38, 70, -140, 270, 0, 70, 140, 70, 38, -250, 38))); break;
            default:
                m.Move(mv, 0, () =>
                {
                    m.Limb(-280, 0, 0, 160, 0, 0, 14, 14, C); m.Ball(-280, 0, 0, 14, C);
                    m.Turn(140, 0, -MathF.PI / 2f, () => m.Lathe(140, 0, C, 0, 0, 90, 0, 0, 160));
                });
                break;
        }
    }

    static partial void RegisterF1k(Dictionary<Type, QuickModel> d)
    {
        foreach (var ty in new[] { typeof(S1382Parameter), typeof(S1383Parameter), typeof(S1384Parameter) })
            Fam(d, ty, (m, t, C, D) =>
            {
                const float w = 1180, h = 650;
                foreach (int x in Sides)
                {
                    m.Tube([V3(x * (w / 2f - 100), -h / 2f), V3(x * w / 2f, -h / 2f), V3(x * w / 2f, h / 2f), V3(x * (w / 2f - 100), h / 2f)], 8, C);
                    m.Ball(x * (w / 2f - 100), -h / 2f, 0, 8, C); m.Ball(x * (w / 2f - 100), h / 2f, 0, 8, C);
                }
            });
        Type[] arrows =
        [
            typeof(S1385Parameter), typeof(S1386Parameter), typeof(S1387Parameter), typeof(S1388Parameter), typeof(S1389Parameter), typeof(S1390Parameter),
            typeof(S1391Parameter), typeof(S1392Parameter), typeof(S1393Parameter), typeof(S1394Parameter), typeof(S1395Parameter), typeof(S1396Parameter),
            typeof(S1397Parameter), typeof(S1398Parameter), typeof(S1399Parameter), typeof(S1400Parameter), typeof(S1401Parameter), typeof(S1402Parameter),
            typeof(S1403Parameter), typeof(S1404Parameter), typeof(S1405Parameter), typeof(S1406Parameter), typeof(S1407Parameter), typeof(S1408Parameter),
            typeof(S1409Parameter), typeof(S1410Parameter), typeof(S1411Parameter),
        ];
        for (int i = 0; i < arrows.Length; i++) { int a = i / 3; Fam(d, arrows[i], (m, t, C, D) => Arrow3D(m, t, C, a)); }
    }
}
