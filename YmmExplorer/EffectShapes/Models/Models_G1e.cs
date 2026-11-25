using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 218番 1541〜1630(90種類)
//   運動・医療のアイコン 10 / 丸数字 21〜50 / 順位 11〜20位 / 日付 1〜31日 / ひらがな あ〜け
// =====================================================================
internal static partial class Models3D
{
    /// <summary>球の表面に沿った五角形の模様(サッカーボール)</summary>
    static void SpherePatch(Sculpt m, float R, Vector3 dir, float r, Vector4 col)
    {
        dir = Vector3.Normalize(dir);
        var up = MathF.Abs(dir.Y) > .9f ? Vector3.UnitX : Vector3.UnitY;
        var u = Vector3.Normalize(Vector3.Cross(up, dir)); var v = Vector3.Cross(dir, u);
        var c = dir * (R + 1.5f);
        for (int i = 0; i < 5; i++)
        {
            float a0 = i * MathF.Tau / 5f, a1 = (i + 1) * MathF.Tau / 5f;
            Vector3 P(float a) => Vector3.Normalize(dir * R + (u * MathF.Cos(a) + v * MathF.Sin(a)) * r) * (R + 1.5f);
            m.Tri3D(c, P(a0), P(a1), col);
        }
    }

    static partial void RegisterG1e(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.125f, .14f, .17f, 1f);
        // サッカーボール: 球と、表面に貼りついた黒い五角形(12枚)
        Icon(d, typeof(X1541Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 245, C);
            float g = (1 + MathF.Sqrt(5)) / 2f;
            Vector3[] ico = [new(0, 1, g), new(0, -1, g), new(0, 1, -g), new(0, -1, -g), new(1, g, 0), new(-1, g, 0), new(1, -g, 0), new(-1, -g, 0), new(g, 0, 1), new(-g, 0, 1), new(g, 0, -1), new(-g, 0, -1)];
            foreach (var p in ico) { var c = Vector3.Normalize(p) * 90; m.Ball(c.X, c.Y, c.Z, 162, D); }
        });
        // 野球ボール: 球と、表面を回る2本の赤い縫い目
        Icon(d, typeof(X1542Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 240, C);
            foreach (int s in Sides)
            {
                var seam = new List<Vector3>();
                for (int i = 0; i <= 30; i++)
                {
                    float a = (i / 30f - .5f) * 2.2f, x = s * (240 - MathF.Cos(a) * 130), y = MathF.Sin(a) * 200;
                    float zz = MathF.Sqrt(MathF.Max(0, 242 * 242 - x * x - y * y));
                    seam.Add(new(x * .98f, y, zz));
                }
                m.Tube(seam, 4, D);
                for (int i = 2; i < 29; i += 3) { var p = seam[i]; m.Limb(p.X - 14, p.Y - 9, p.Z - 2, p.X + 14, p.Y + 9, p.Z - 2, 2.5f, 2.5f, D); }
            }
        });
        // バスケットボール: 球と、表面を回る黒い線(縦・横・左右の弧)
        Icon(d, typeof(X1543Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 240, C);
            m.Torus(0, 0, 0, 242, 4.5f, D);
            m.Yaw(0, 0, 0, MathF.PI / 2f, () => m.Torus(0, 0, 0, 242, 4.5f, D));
            foreach (int s in Sides) m.Tube(Sculpt.OnFront(0, 0, 0, 240, 240, 240, 3f, Sculpt.ArcPts(s * 280, 0, 190, s > 0 ? 2.24f : -.9f, s > 0 ? 4.04f : .9f, 14)), 4.5f, D);
        });
        // テニスラケット: 楕円の枠、張られたガット、グリップ
        Icon(d, typeof(X1544Parameter), (m, t, C, D) => m.Turn(0, 0, .55f, () =>
        {
            m.Tube([.. Enumerable.Range(0, 41).Select(i => { float a = i * MathF.Tau / 40f; return new Vector3(MathF.Cos(a) * 145, -130 + MathF.Sin(a) * 190, 0); })], 12, C);
            for (int i = -5; i <= 5; i++)
            {
                float x = i * 25, h = 190 * MathF.Sqrt(MathF.Max(0f, 1 - x * x / 21025f));
                m.Limb(x, -130 - h, 0, x, -130 + h, 0, 1.5f, 1.5f, D);
                float y = i * 33, w = 145 * MathF.Sqrt(MathF.Max(0f, 1 - y * y / 36100f));
                m.Limb(-w, -130 + y, 0, w, -130 + y, 0, 1.5f, 1.5f, D);
            }
            m.Limb(0, 60, 0, 0, 290, 0, 14, 17, C);
            m.Cyl(0, 240, 0, 19, 110, Sculpt.Dark(C, .7f), .3f);
        }));
        // ダンベル: 握りの棒と、両側に重なる円盤のおもり
        Icon(d, typeof(X1545Parameter), (m, t, C, D) =>
        {
            m.Limb(-190, 0, 0, 190, 0, 0, 20, 20, D);
            foreach (int s in Sides)
            {
                m.Turn(s * 170, 0, MathF.PI / 2f, () => m.Cyl(s * 170, 0, 0, 110, 58, C, .2f));
                m.Turn(s * 232, 0, MathF.PI / 2f, () => m.Cyl(s * 232, 0, 0, 85, 50, C, .2f));
                m.Turn(s * 280, 0, MathF.PI / 2f, () => m.Cyl(s * 280, 0, 0, 55, 36, C, .2f));
            }
        });
        // ホイッスル: 丸い胴、吹き口、中の玉、ひもの輪
        Icon(d, typeof(X1546Parameter), (m, t, C, D) =>
        {
            m.Turn(-60, 50, MathF.PI / 2f, () => m.Cyl(-60, 50, 0, 150, 160, C, .3f));
            m.Box(130, -55, 0, 340, 90, 140, C, .3f);
            m.Disc(-60, 50, 82, 62, 62, 10, m.C(0x8A95A0), .5f);
            m.Box(95, -112, 0, 70, 22, 100, D, .4f);
            m.Tube(ArcPts3(-150, -100, 70, 3.1f, 5.4f, 0, 12), 4, D);
        });
        // 聴診器: 耳の管、Y字に分かれる管、チェストピース
        Icon(d, typeof(X1547Parameter), (m, t, C, D) =>
        {
            m.Tube(ArcPts3(0, -120, 130, 3.14f, 6.28f, 0, 16), 8, C);
            foreach (int s in Sides) { m.Limb(s * 130, -120, 0, s * 100, 20, 0, 8, 8, C); m.Ball(s * 130, -255 + 135, 0, 14, D); }
            m.Tube([V3(-100, 20), V3(0, 130), V3(100, 20)], 8, C);
            m.Tube([V3(0, 130), V3(0, 220, 20), V3(120, 260, 40)], 8, C);
            m.Disc(165, 260, 40, 62, 62, 40, D, .4f);
            m.Disc(165, 260, 62, 34, 34, 8, C, .5f);
        });
        // 注射器: 透けた筒と中の液、目盛り、針、押し子
        Icon(d, typeof(X1548Parameter), (m, t, C, D) => m.Turn(0, 0, -.65f, () =>
        {
            var g = m.C(0x8A95A0);
            m.Turn(0, 0, MathF.PI / 2f, () => m.Cyl(0, 0, 0, 46, 340, C with { W = C.W * .5f }, .1f));
            m.Turn(-60, 0, MathF.PI / 2f, () => m.Cyl(-60, 0, 0, 37, 210, D, .1f));
            for (int i = 0; i < 7; i++) m.Box(-140 + i * 46, -30, 40, 3, 26, 6, K, .3f);
            m.Turn(200, 0, MathF.PI / 2f, () => m.Cyl(200, 0, 0, 12, 70, g, .2f));
            m.Limb(235, 0, 0, 350, 0, 0, 2.5f, 1, g);
            m.Limb(-205, 0, 0, -243, 0, 0, 12, 12, g);
            m.Disc(-250, 0, 0, 12, 65, 50, g, .3f);
        }));
        // カプセル薬: 2色のカプセル(丸い両端)とつや
        Icon(d, typeof(X1549Parameter), (m, t, C, D) => m.Turn(0, 0, -.65f, () =>
        {
            m.Limb(-160, 0, 0, 0, 0, 0, 85, 85, C);
            m.Limb(0, 0, 0, 160, 0, 0, 85, 85, D);
            m.Limb(-180, -42, 70, -60, -42, 70, 10, 10, m.C(0xFFFFFF, .45f));
        }));
        // 救急箱: 取っ手、箱、ふたの境、浮き出た赤十字
        Icon(d, typeof(X1550Parameter), (m, t, C, D) =>
        {
            var g = m.C(0x8A95A0);
            m.Tube([V3(-75, -175), V3(-75, -240), V3(75, -240), V3(75, -175)], 16, g);
            m.Box(0, 40, 0, 560, 410, 300, C, .12f);
            m.Box(0, -85, 0, 564, 8, 304, m.C(0xB7C2CF), .3f);
            m.Box(0, 55, 154, 190, 62, 12, D, .3f); m.Box(0, 55, 154, 62, 190, 12, D, .3f);
        });

        Type[] nums = [typeof(X1551Parameter), typeof(X1552Parameter), typeof(X1553Parameter), typeof(X1554Parameter), typeof(X1555Parameter), typeof(X1556Parameter), typeof(X1557Parameter), typeof(X1558Parameter), typeof(X1559Parameter), typeof(X1560Parameter), typeof(X1561Parameter), typeof(X1562Parameter), typeof(X1563Parameter), typeof(X1564Parameter), typeof(X1565Parameter), typeof(X1566Parameter), typeof(X1567Parameter), typeof(X1568Parameter), typeof(X1569Parameter), typeof(X1570Parameter), typeof(X1571Parameter), typeof(X1572Parameter), typeof(X1573Parameter), typeof(X1574Parameter), typeof(X1575Parameter), typeof(X1576Parameter), typeof(X1577Parameter), typeof(X1578Parameter), typeof(X1579Parameter), typeof(X1580Parameter)];
        foreach (var ty in nums) FamT(d, ty, (m, t, C, D, s) => RoundNumber(m, C, D, s));
        Type[] ranks = [typeof(X1581Parameter), typeof(X1582Parameter), typeof(X1583Parameter), typeof(X1584Parameter), typeof(X1585Parameter), typeof(X1586Parameter), typeof(X1587Parameter), typeof(X1588Parameter), typeof(X1589Parameter), typeof(X1590Parameter)];
        foreach (var ty in ranks) FamT(d, ty, (m, t, C, D, s) => RankMedal(m, t, C, D, s));
        Type[] days = [typeof(X1591Parameter), typeof(X1592Parameter), typeof(X1593Parameter), typeof(X1594Parameter), typeof(X1595Parameter), typeof(X1596Parameter), typeof(X1597Parameter), typeof(X1598Parameter), typeof(X1599Parameter), typeof(X1600Parameter), typeof(X1601Parameter), typeof(X1602Parameter), typeof(X1603Parameter), typeof(X1604Parameter), typeof(X1605Parameter), typeof(X1606Parameter), typeof(X1607Parameter), typeof(X1608Parameter), typeof(X1609Parameter), typeof(X1610Parameter), typeof(X1611Parameter), typeof(X1612Parameter), typeof(X1613Parameter), typeof(X1614Parameter), typeof(X1615Parameter), typeof(X1616Parameter), typeof(X1617Parameter), typeof(X1618Parameter), typeof(X1619Parameter), typeof(X1620Parameter), typeof(X1621Parameter)];
        foreach (var ty in days) FamT(d, ty, (m, t, C, D, s) => DeskCalendar(m, C, D, s));
        Type[] kana = [typeof(X1622Parameter), typeof(X1623Parameter), typeof(X1624Parameter), typeof(X1625Parameter), typeof(X1626Parameter), typeof(X1627Parameter), typeof(X1628Parameter), typeof(X1629Parameter), typeof(X1630Parameter)];
        foreach (var ty in kana) FamT(d, ty, (m, t, C, D, s) => DayTile(m, C, D, s));
    }
}
