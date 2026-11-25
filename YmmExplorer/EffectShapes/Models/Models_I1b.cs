using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 213_QIcons1.cs のアイコン 731〜760(料理・お菓子・飲み物 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    /// <summary>お椀・どんぶり(外側の色・中の汁の色)。上のふちが y0、半径 r</summary>
    static void Bowl(Sculpt m, float y0, float r, float depth, Vector4 outer, Vector4 inner, Vector4 soup, float soupY)
    {
        m.Lathe(0, 0, outer, r, y0, r * .97f, y0 + depth * .45f, r * .72f, y0 + depth * .9f, r * .42f, y0 + depth, r * .44f, y0 + depth * 1.12f, 0, y0 + depth * 1.12f);
        m.Lathe(0, 0, inner, 0, y0 + depth * .8f, r * .6f, y0 + depth * .78f, r * .9f, y0 + depth * .4f, r * .95f, y0 + 4, r, y0);
        m.Pitch(0, y0, 0, MathF.PI / 2f, () => m.Torus(0, y0, 0, r, 6, outer));
        m.Super(0, soupY, 0, r * .93f, 3, r * .93f, .2f, 1f, soup);
    }

    /// <summary>平たいお皿(上から見下ろす料理の皿)</summary>
    static void Plate(Sculpt m, float y, float rx, float rz, Vector4 col)
    {
        m.Super(0, y, 0, rx, 14, rz, .35f, 1f, col);
        m.Super(0, y - 10, 0, rx * .86f, 6, rz * .86f, .3f, 1f, Vector4.Lerp(col, Vector4.One, .5f));
    }

    static partial void RegisterI1b(Dictionary<Type, QuickModel> d)
    {
        // ピザ一切れ: 三角の生地とチーズ、ふくらんだ耳、サラミ
        Icon(d, typeof(S731Parameter), (m, t, C, D) =>
        {
            m.Slab(-10, 26, m.C(0xC98A3C), -235, -210, 235, -210, 0, 300);
            m.Slab(8, 12, C, -205, -175, 205, -175, 0, 255);
            foreach (var (x, y) in new[] { (-80f, -110f), (85f, -115f), (0f, 30f) }) { m.Disc(x, y, 16, 48, 48, 10, D, .4f); m.Ball(x - 10, y - 10, 22, 10, 10, 3, m.C(0xFF7A7A)); }
            m.Limb(-240, -225, 0, 240, -225, 0, 30, 30, m.C(0xB9782E));
        });
        // ホットドッグ: 割れたパン、ソーセージ、波打つマスタード
        Icon(d, typeof(S732Parameter), (m, t, C, D) =>
        {
            m.Super(0, 45, 0, 300, 80, 110, .6f, .6f, C);
            foreach (int s in Sides) m.Super(0, -5, s * 70, 295, 60, 40, .6f, .7f, C);
            m.Limb(-300, -15, 0, 300, -15, 0, 46, 46, D);
            m.Tube([.. Enumerable.Range(0, 31).Select(i => new Vector3(-285 + i * 19, -55 + MathF.Sin(i * 1.2f) * 12, MathF.Cos(i * 1.2f) * 20))], 6, m.C(0xFFD93B));
        });
        // サンドイッチ: 三角のパンと、はさんだ具(レタス・チーズ・トマト)の層
        Icon(d, typeof(S733Parameter), (m, t, C, D) =>
        {
            const float dd = 180;
            m.Slab(0, dd, C, -280, 120, 280, 120, 0, -220);
            m.Slab(0, dd + 14, D, -300, 132, 300, 132, 265, 160, -265, 160);
            m.Slab(0, dd + 6, m.C(0xFFD54A), -280, 160, 280, 160, 235, 190, -235, 190);
            m.Slab(0, dd + 2, m.C(0xE5392F), -235, 190, 235, 190, 210, 215, -210, 215);
            m.Slab(0, dd, C, -260, 215, 260, 215, 235, 270, -235, 270);
        });
        // 食パン: 山形の一枚(茶色い耳と白い中身)
        Icon(d, typeof(S734Parameter), (m, t, C, D) =>
        {
            m.Disc(-115, -125, -10, 120, 120, 50, D, .3f); m.Disc(115, -125, -10, 120, 120, 50, D, .3f); m.Box(0, 50, -10, 370, 370, 50, D, .2f);
            m.Disc(-105, -115, 4, 102, 102, 50, C, .2f); m.Disc(105, -115, 4, 102, 102, 50, C, .2f); m.Box(0, 50, 4, 335, 340, 50, C, .15f);
        });
        // クロワッサン: 三日月形にふくらんだ生地と、巻きの筋
        Icon(d, typeof(S735Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 18;
            for (int i = 0; i <= 12; i++) { float e = i / 12f, a = 3.5f + e * 2.42f, r = 40 + MathF.Sin(e * MathF.PI) * 50; m.Ball(MathF.Cos(a) * 290, 190 + MathF.Sin(a) * 290, 0, r * 1.1f, r, r, C); }
            for (int i = -3; i <= 3; i++) { float a = 4.71f + i * .3f, r = 40 + MathF.Cos(i * .45f) * 50; m.Torus(MathF.Cos(a) * 290, 190 + MathF.Sin(a) * 290, 0, r * 1.02f, 3.5f, D); }
            m.Seg = keep;
        });
        // カレーライス: お皿、もりあがったごはん、カレーと具
        Icon(d, typeof(S736Parameter), (m, t, C, D) =>
        {
            Plate(m, 100, 370, 240, m.C(0xE7EDF2));
            m.Ball(-105, 85, -20, 185, 80, 150, D);
            m.Super(105, 82, 20, 185, 22, 150, 1f, 1f, C);
            foreach (var (x, z) in new[] { (120f, 40f), (190f, 80f), (60f, 100f) }) m.Box(x, 60, z, 50, 40, 50, m.C(0xFF9A3C), .3f);
            m.Box(90, 60, -20, 44, 40, 44, m.C(0xFFD27A), .3f);
        });
        // ラーメン: どんぶり、スープ、波打つめん、ゆで卵、はし
        Icon(d, typeof(S737Parameter), (m, t, C, D) =>
        {
            Bowl(m, -20, 300, 300, C, m.C(0xFFFFFF), m.C(0xD99A45), -12);
            int keep = m.Seg; m.Seg = 10;
            for (int i = 0; i < 8; i++) m.Tube([.. Sculpt.ArcPts(-180 + i * 50, -16, 40, 3.3f, 6f, 8).Select(q => new Vector3(q.X, -18, (q.Y + 16) * 2.5f + 40))], 5, D);
            m.Seg = keep;
            m.Ball(120, -22, -60, 44, 14, 44, m.C(0xFFFFFF)); m.Ball(120, -30, -60, 20, 8, 20, m.C(0xFFB300));
            m.Limb(170, -260, -120, 10, -14, -40, 6, 5, m.C(0x8A5A32)); m.Limb(215, -245, -100, 50, -14, -20, 6, 5, m.C(0x8A5A32));
        });
        // オムライス: お皿、ふっくらした卵、ジグザグのケチャップ
        Icon(d, typeof(S738Parameter), (m, t, C, D) =>
        {
            Plate(m, 110, 350, 230, m.C(0xFFFFFF));
            m.Ball(0, 60, 0, 270, 110, 160, C);
            m.Tube([.. new[] { (-120f, -10f), (-70f, -40f), (-20f, -10f), (30f, -40f), (80f, -10f), (130f, -40f) }.Select(q => new Vector3(q.Item1, -45, q.Item2 + 30))], 7, D);
            m.Ball(-110, -30, 80, 52, 10, 18, m.C(0xFFFFFF, .45f));
        });
        // えび天: ころもを付けた体(でこぼこ)と、赤い尾
        Icon(d, typeof(S739Parameter), (m, t, C, D) => m.Turn(0, 0, -.3f, () =>
        {
            m.Limb(-200, 0, 0, 140, 0, 0, 60, 66, C);
            for (int i = 0; i < 18; i++) { float x = -240 + i * 22, a = i * 2.1f; m.Ball(x, MathF.Cos(a) * 58, MathF.Sin(a) * 58, 14, m.C(0xFFE3A8)); }
            m.Slab(0, 10, D, 200, 0, 330, -72, 300, 0, 330, 72);
        }));
        // 餃子: 半月形にふくらんだ皮、ひだ、焼き色の底
        Icon(d, typeof(S740Parameter), (m, t, C, D) =>
        {
            // 半月形にふくらんだ皮(下は平ら)と、焼き色の底、上のふちのひだ
            m.Super(0, 74, 0, 236, 16, 96, .5f, 1f, D);
            const int ns = 18, nw = 10;
            Vector3 G(int i, int j) { float s = MathF.PI * i / ns, w = -MathF.PI / 2f + MathF.PI * j / nw; return new(-240 * MathF.Cos(s) * MathF.Cos(w), 70 - 200 * MathF.Sin(s) * MathF.Cos(w), 95 * MathF.Sin(w)); }
            for (int i = 0; i < ns; i++) for (int j = 0; j < nw; j++) m.Quad3D(G(i, j), G(i + 1, j), G(i + 1, j + 1), G(i, j + 1), C);
            for (int i = 0; i < 9; i++) { float a = 3.3f + i * .36f; m.Tube([V3(MathF.Cos(a) * 205, 70 + MathF.Sin(a) * 205, 0), V3(MathF.Cos(a + .12f) * 240, 70 + MathF.Sin(a + .12f) * 240, 0), V3(MathF.Cos(a + .24f) * 205, 70 + MathF.Sin(a + .24f) * 205, 0)], 7, m.C(0xD9C497)); }
        });
        // 焼き鳥: 串と、肉とねぎが交互に4つ
        Icon(d, typeof(S741Parameter), (m, t, C, D) => m.Turn(0, 0, -.55f, () =>
        {
            m.Limb(-340, 0, 0, 350, 0, 0, 5, 3, D);
            for (int i = 0; i < 4; i++) { m.Box(-150 + i * 112, 0, 0, 92, 120, 92, i % 2 == 0 ? C : m.C(0x72A84B), .55f); m.Ball(-160 + i * 112, -30, 46, 20, 8, 4, m.C(0xFFFFFF, .35f)); }
        }));
        // うどん: どんぶり、だし、太いめん、かまぼこ、ねぎ
        Icon(d, typeof(S742Parameter), (m, t, C, D) =>
        {
            Bowl(m, -10, 290, 290, C, m.C(0xFFFFFF), m.C(0xC98A3C), -2);
            int keep = m.Seg; m.Seg = 12;
            for (int i = 0; i < 7; i++) m.Tube([.. Sculpt.ArcPts(-160 + i * 52, -2, 48, 3.2f, 6.1f, 8).Select(q => new Vector3(q.X, -8, (q.Y + 2) * 2.2f + 20))], 9, D);
            m.Seg = keep;
            m.Disc(90, -24, -60, 24, 24, 12, m.C(0xF7D9D9), .3f);
            m.Limb(-140, -14, -80, -90, -14, -50, 4, 4, m.C(0x4FAE4F));
        });
        // 味噌汁: お椀、みそ汁、とうふ、わかめ、ゆらゆら昇る湯気
        Icon(d, typeof(S743Parameter), (m, t, C, D) =>
        {
            Bowl(m, -20, 245, 245, C, m.C(0x4A1A14), D, -12);
            m.Box(-60, -16, -30, 42, 18, 42, m.C(0xFFF8E7), .2f); m.Box(60, -16, 20, 42, 18, 42, m.C(0xFFF8E7), .2f);
            m.Ball(-110, -13, 50, 30, 4, 14, m.C(0x2F7D35));
            Steam(m, 3, 0, 60, -70, 120, 0, t, 4);
        });
        // たい焼き: ふくらんだ魚の形、尾びれと背びれ、目、うろこ
        Icon(d, typeof(S744Parameter), (m, t, C, D) =>
        {
            m.Ball(-20, 0, 0, 220, 145, 70, C);
            m.Slab(0, 50, C, 170, 0, 320, -120, 290, 0, 320, 120);
            m.Slab(0, 30, D, -30, -120, 70, -190, 120, -110);
            m.Ball(-155, -40, Sculpt.FrontZ(-20, 0, 0, 220, 145, 70, -155, -40), 20, 20, 8, m.C(0xFFFFFF)); m.Ball(-155, -40, Sculpt.FrontZ(-20, 0, 0, 220, 145, 70, -155, -40) + 6, 10, 10, 4, m.C(0x20242B));
            for (int y = -1; y <= 1; y++) for (int x = 0; x < 4; x++) m.Tube(Sculpt.OnFront(-20, 0, 0, 220, 145, 70, 1f, Sculpt.ArcPts(-20 + x * 48, y * 50, 24, -1.2f, 1.2f, 6)), 2.5f, D);
        });
        // 団子: 串と3色のだんご
        Icon(d, typeof(S745Parameter), (m, t, C, D) =>
        {
            m.Limb(0, -310, 0, 0, 320, 0, 6, 5, m.C(0xB98A52));
            m.Ball(0, -150, 0, 96, C); m.Ball(0, 0, 0, 96, m.C(0xFFFFFF)); m.Ball(0, 150, 0, 96, D);
        });
        // 大福: やわらかくつぶれた丸いもち、中の豆が透けた粒
        Icon(d, typeof(S746Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 50, 0, 235, 160, 220, C);
            m.Ball(-75, -40, 150, 52, 20, 10, m.C(0xFFFFFF));
            foreach (var (x, y) in new[] { (-60f, 50f), (40f, 20f), (90f, 95f), (-10f, 120f) }) m.Ball(x, y, Sculpt.FrontZ(0, 50, 0, 235, 160, 220, x, y) - 2, 16, 11, 4, D);
        });
        // せんべい: 厚みのある丸いせんべい、焼き目、巻いたのり
        Icon(d, typeof(S747Parameter), (m, t, C, D) =>
        {
            m.Disc(0, 0, 0, 230, 230, 40, C, .4f);
            for (int i = 0; i < 30; i++) { float x = -150 + R01(i) * 300, y = -150 + R01(i + 7) * 300; if (x * x + y * y < 210 * 210) m.Ball(x, y, 20, 5, 5, 2, m.C(0xB9823E)); }
            m.Box(0, 110, 0, 330, 120, 46, D, .1f);
        });
        // クッキー: 厚みのある丸いクッキーとチョコチップ
        Icon(d, typeof(S748Parameter), (m, t, C, D) =>
        {
            m.Disc(0, 0, 0, 235, 235, 50, C, .5f);
            for (int i = 0; i < 14; i++) { float a = R01(i) * 6.28f, r = MathF.Sqrt(R01(i + 3)) * 190; m.Ball(MathF.Cos(a) * r, MathF.Sin(a) * r, 24, 24, 17, 12, D); }
        });
        // キャンディ: まん丸の飴と、両はしのねじった包み紙、うず巻き模様
        Icon(d, typeof(S749Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) { m.Limb(s * 140, 0, 0, s * 190, 0, 0, 30, 20, D); m.Turn(s * 190, 0, s * -MathF.PI / 2f, () => m.Lathe(s * 190, 0, D, 20, 0, 100, -s * 0 + 115, 0, 115)); }
            m.Ball(0, 0, 0, 160, C);
            for (int i = 0; i < 3; i++) m.Tube(Sculpt.OnFront(0, 0, 0, 160, 160, 160, 1f, Sculpt.ArcPts(0, 0, 40 + i * 40, i, i + 3.8f, 12)), 5, m.C(0xFFFFFF));
        });
        // 棒付きキャンディ: 白い棒と、厚みのあるうず巻きの飴
        Icon(d, typeof(S750Parameter), (m, t, C, D) =>
        {
            m.Limb(0, 150, -10, 0, 330, -10, 8, 8, m.C(0xE8E8E8));
            m.Disc(0, 0, 0, 200, 200, 60, C, .55f);
            for (int i = 0; i < 3; i++) m.Tube([.. Sculpt.ArcPts(0, 0, 48 + i * 52, i * 2.1f, i * 2.1f + 4.6f, 16).Select(q => new Vector3(q, 30))], 10, D);
        });
        // 板チョコ: ブロックの盛り上がった板と、赤い包み紙と文字
        Icon(d, typeof(S751Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, 0, 420, 540, 30, C, .08f);
            for (int y = 0; y < 4; y++) for (int x = 0; x < 3; x++) m.Box(-125 + x * 125, -185 + y * 123, 18, 108, 105, 16, D, .25f);
            m.Box(0, 170, 4, 426, 200, 48, m.C(0xD7263D), .06f);
            m.Text("CHOCO", 48, 0, 175, 30, 4, m.C(0xFFFFFF));
        });
        // ショートケーキ: 三角に切ったスポンジ(クリームといちごの層)、上のいちご
        Icon(d, typeof(S752Parameter), (m, t, C, D) =>
        {
            const float dd = 300;
            m.Slab(0, dd, C, -260, 20, 260, 20, 260, 230, -260, 230);
            m.Slab(0, dd, m.C(0xFFFFFF), -260, 20, 260, 20, 170, -120, -120, -120);
            m.Box(0, 105, 0, 522, 22, dd + 2, m.C(0xFFB3B3), .1f);
            m.Box(0, 155, 0, 522, 18, dd + 2, m.C(0xFFE4B8), .1f);
            m.Lathe(35, 0, D, 0, -100, 40, -110, 62, -150, 40, -200, 0, -215);
            m.Pitch(35, -212, 0, MathF.PI / 2f, () => m.Slab(0, 6, m.C(0x4FA84F), StarPts(35, -212, 34, 5, .45f, 0)));
        });
        // マカロン: ドーム形の上下の生地、間のクリーム、ふちのギザギザ
        Icon(d, typeof(S753Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, [.. Dome(230, 150, -10, 12), new Vector2(0, -10)]);
            m.Cyl(0, 15, 0, 214, 40, D, .5f);
            m.Lathe(0, 0, C, 0, 40, 226, 40, 230, 60, 200, 110, 0, 115);
            for (int i = 0; i < 24; i++) { float a = i * MathF.Tau / 24f; m.Ball(MathF.Cos(a) * 228, -8, MathF.Sin(a) * 228, 10, m.C(0xE97FAA)); }
            m.Ball(-85, -115, 130, 62, 14, 16, m.C(0xFFFFFF, .45f));
        });
        // ポップコーン: しま模様の四角い箱と、山盛りのポップコーン
        Icon(d, typeof(S754Parameter), (m, t, C, D) =>
        {
            SquareFrustum(m, 0, 0, -85, 185, 310, 135, m.C(0xFFFFFF));
            foreach (int f in new[] { -1, 1 })
                for (int i = 0; i < 3; i++)
                {
                    float x = -132 + i * 88;
                    float ztop = f * 185.5f, zbot = f * 135.5f;
                    m.Quad3D(V3(x - 22, -85, ztop), V3(x + 36, -85, ztop), V3((x + 26) * .73f, 310, zbot), V3((x - 8) * .73f, 310, zbot), C);
                    m.Quad3D(V3(ztop, -85, x - 22), V3(ztop, -85, x + 36), V3(zbot, 310, (x + 26) * .73f), V3(zbot, 310, (x - 8) * .73f), C);
                }
            int keep = m.Seg; m.Seg = 12;
            for (int i = 0; i < 44; i++) m.Ball(-170 + R01(i) * 340, -100 - R01(i + 5) * 150, -170 + R01(i + 13) * 340, 34 + R01(i + 9) * 20, D);
            m.Seg = keep;
        });
        // アイスキャンディ: 角の丸い四角い氷菓と木の棒、つや
        Icon(d, typeof(S755Parameter), (m, t, C, D) =>
        {
            m.Box(0, 220, 0, 54, 190, 20, D, .4f);
            m.Box(0, -60, 0, 290, 470, 110, C, .55f);
            m.Box(-70, -55, 52, 28, 260, 6, m.C(0xFFFFFF, .45f), .5f); m.Box(0, -30, 52, 28, 260, 6, m.C(0xFFFFFF, .45f), .5f);
        });
        // 牛乳パック: 四角い紙パックと三角の屋根、上の合わせ目、文字
        Icon(d, typeof(S756Parameter), (m, t, C, D) =>
        {
            m.Box(0, 70, 0, 300, 430, 300, C, .05f);
            m.Slab(0, 300, D, -150, -145, 150, -145, 75, -250, -75, -250);
            m.Quad3D(V3(-150, -145, 150), V3(150, -145, 150), V3(150, -250, 0), V3(-150, -250, 0), D);
            m.Quad3D(V3(-150, -145, -150), V3(150, -145, -150), V3(150, -250, 0), V3(-150, -250, 0), D);
            m.Box(0, -265, 0, 150, 32, 16, m.C(0xB7D6EC), .2f);
            m.Text("牛乳", 68, 0, 40, 152, 4, D);
        });
        // ジュース缶: 円柱の缶、上下のふち、ふたとプルタブ、ラベルの丸と文字
        Icon(d, typeof(S757Parameter), (m, t, C, D) =>
        {
            m.Cyl(0, 0, 0, 145, 520, C, .12f);
            m.Cyl(0, -270, 0, 118, 30, D, .3f); m.Cyl(0, 275, 0, 118, 20, D, .3f);
            m.Super(0, -286, 0, 112, 2, 112, .2f, 1f, m.C(0xB7BEC6));
            m.Box(20, -289, 30, 50, 4, 26, m.C(0xD8DDE3), .5f);
            m.Disc(0, 30, 140, 86, 86, 10, m.C(0xFFFFFF), .3f);
            m.Text("果汁", 42, 0, 30, 147, 3, C);
            m.Lathe(0, 0, m.C(0xFFFFFF, .3f), 146, -220, 146, 220);
        });
        // ペットボトル: 透明な回転体のボトル、キャップ、ラベルと文字
        Icon(d, typeof(S758Parameter), (m, t, C, D) =>
        {
            m.Cyl(0, -295, 0, 46, 48, D, .3f);
            m.Lathe(0, 0, new Vector4(C.X, C.Y, C.Z, C.W * .55f), 40, -270, 40, -255, 135, -120, 135, 280, 120, 300, 0, 302);
            m.Cyl(0, 60, 0, 137, 130, D, .05f);
            m.Text("WATER", 40, 0, 60, 138, 3, m.C(0xFFFFFF));
            m.Limb(-95, -100, 90, -95, 280, 90, 6, 6, m.C(0xFFFFFF, .5f));
        });
        // ワイングラス: 透明なボウル(中のワイン)、細い脚、丸い台
        Icon(d, typeof(S759Parameter), (m, t, C, D) =>
        {
            var glass = new Vector4(D.X, D.Y, D.Z, D.W * .18f);
            m.Lathe(0, 0, C, 0, -110, 145, -110, 120, -30, 60, 20, 0, 30);
            m.Lathe(0, 0, glass, 150, -260, 155, -110, 125, -25, 60, 30, 12, 60, 0, 60);
            m.Cyl(0, 180, 0, 8, 220, new Vector4(D.X, D.Y, D.Z, D.W * .8f), .3f);
            m.Super(0, 295, 0, 110, 8, 110, .4f, 1f, new Vector4(D.X, D.Y, D.Z, D.W * .8f));
        });
        // ビールジョッキ: 透明なジョッキ(中のビール)、もこもこの泡、取っ手、昇る泡
        Icon(d, typeof(S760Parameter), (m, t, C, D) =>
        {
            m.Cyl(0, 75, 0, 150, 400, C, .1f);
            m.Cyl(0, 70, 0, 165, 420, m.C(0xFFFFFF, .25f), .1f);
            m.Torus(225, 60, 0, 88, 18, m.C(0xE7D9A8, .7f));
            int keep = m.Seg; m.Seg = 16;
            for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12f; m.Ball(MathF.Cos(a) * 110, -140 + (i % 2) * 18, MathF.Sin(a) * 110, 58, D); }
            m.Ball(0, -160, 0, 70, D);
            m.Seg = 8;
            for (int i = 0; i < 10; i++) { float k = Fr(t * .4f + i * .1f); m.Ball(-130 + i * 28, 240 - k * 340, (R01(i) - .5f) * 200, 5, m.C(0xFFFFFF)); }
            m.Seg = keep;
        });
    }
}
