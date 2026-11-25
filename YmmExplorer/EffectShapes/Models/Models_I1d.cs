using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: アイコン 791〜820(文房具・身の回りの物・服・楽器 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    /// <summary>布の物(服など): 厚みのある板に、ふちを丸い管で縁取ってやわらかく見せる</summary>
    static void Cloth(Sculpt m, float d, Vector4 col, params float[] xy)
    {
        m.Slab(0, d, col, xy);
        var pts = new List<Vector3>();
        for (int i = 0; i + 1 < xy.Length; i += 2) pts.Add(new(xy[i], xy[i + 1], 0));
        pts.Add(pts[0]);
        int keep = m.Seg; m.Seg = 10; m.Tube(pts, d / 2f, col); m.Seg = keep;
    }

    static partial void RegisterI1d(Dictionary<Type, QuickModel> d)
    {
        // ゼムクリップ: 曲げた針金(管)
        Icon(d, typeof(S791Parameter), (m, t, C, D) =>
        {
            var q = new List<Vector3>();
            void Seg(float x0, float y0, float x1, float y1) { q.Add(V3(x0, y0)); q.Add(V3(x1, y1)); }
            Seg(60, -150, 60, 160);
            q.AddRange(Sculpt.ArcPts(15, 160, 45, 0, MathF.PI, 8).Select(p => new Vector3(p, 0)));
            q.Add(V3(-30, -210)); q.AddRange(Sculpt.ArcPts(40, -210, 70, MathF.PI, MathF.Tau, 10).Select(p => new Vector3(p, 0)));
            q.Add(V3(110, 160)); q.AddRange(Sculpt.ArcPts(135, 160, 25, MathF.PI, 0, 1).Select(p => new Vector3(p, 0)));
            m.Tube(q, 7, C);
        });
        // 画びょう: 針、平たいつば、円すい台の頭
        Icon(d, typeof(S792Parameter), (m, t, C, D) =>
        {
            m.Limb(0, 60, 0, 0, 300, 0, 5, 1, D);
            m.Super(0, 50, 0, 175, 12, 175, .4f, 1f, C);
            m.Lathe(0, 0, C, 0, -140, 118, -140, 75, 40, 0, 40);
            m.Super(0, -142, 0, 118, 10, 118, .5f, 1f, m.C(0xFF7A85));
        });
        // 絵筆: 木の柄、金具、先のとがった筆先
        Icon(d, typeof(S793Parameter), (m, t, C, D) => m.Turn(0, 0, .55f, () =>
        {
            m.Lathe(0, 0, C, 16, -90, 22, 330, 0, 330);
            m.Cyl(0, -110, 0, 31, 78, m.C(0xC8D0D8), .2f);
            m.Lathe(0, 0, D, 0, -340, 14, -300, 32, -170, 28, -150, 0, -150);
        }));
        // パレット: 指穴のある板と、もり上がった絵の具
        Icon(d, typeof(S794Parameter), (m, t, C, D) =>
        {
            var pal = new List<float>();
            for (int i = 0; i <= 48; i++) { float a = i * MathF.Tau / 48f; pal.Add(MathF.Cos(a) * 335); pal.Add(MathF.Sin(a) * 250); }
            m.Slab(-10, 24, C, [.. pal]);
            m.Torus(180, 85, 0, 46, 8, D);
            uint[] c0 = [0xE5392F, 0xFFD54A, 0x4FAE4F, 0x2F86C6, 0x9A4DDB];
            for (int i = 0; i < 5; i++) { float a = 2.2f + i * .72f; m.Ball(MathF.Cos(a) * 210, MathF.Sin(a) * 150, 14, 46, 46, 22, m.C(c0[i])); }
        });
        // かさ: 骨のある半球の傘、柄と持ち手、先
        Icon(d, typeof(S795Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 8;
            var dome = Dome(300, 200, 0, 12);
            m.Lathe(0, 0, C, [.. dome, new Vector2(300, 6)]);
            m.Seg = keep;
            m.Limb(0, -200, 0, 0, 250, 0, 7, 7, D);
            m.Tube([.. Sculpt.ArcPts(-50, 250, 50, 0, MathF.PI, 10).Select(q => new Vector3(q, 0))], 7, D);
            m.Limb(0, -200, 0, 0, -240, 0, 5, 3, D);
        });
        // くつ: ふくらんだ甲、白い靴底、靴ひも
        Icon(d, typeof(S796Parameter), (m, t, C, D) =>
        {
            m.Super(-20, 50, 0, 280, 75, 110, .55f, .6f, C);
            m.Super(-200, -10, 0, 95, 95, 105, .55f, .7f, C);
            m.Ball(200, 75, 0, 115, 55, 108, C);
            m.Pitch(-200, -95, 0, MathF.PI / 2f, () => m.Torus(-200, -95, 0, 75, 12, Sculpt.Dark(C, .8f)));
            m.Box(10, 135, 0, 650, 50, 230, D, .4f);
            for (int i = 0; i < 4; i++) m.Limb(-105 + i * 44, -30 + i * 22, 90, -55 + i * 44, -60 + i * 22, 70, 4, 4, D);
            m.Ball(-240, 20, 100, 28, 44, 8, m.C(0xFFFFFF, .4f));
        });
        // 帽子: 丸い山、リボン、広いつば
        Icon(d, typeof(S797Parameter), (m, t, C, D) =>
        {
            m.Super(0, 120, 0, 330, 14, 330, .5f, 1f, C);
            m.Lathe(0, 0, C, [.. Dome(190, 190, 110, 12), new Vector2(190, 120), new Vector2(0, 120)]);
            m.Cyl(0, 84, 0, 192, 44, D, .2f);
        });
        // メガネ: 2つのレンズと太いふち、つる、ブリッジ
        Icon(d, typeof(S798Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides)
            {
                m.Disc(s * 135, 0, 0, 100, 100, 8, new Vector4(D.X, D.Y, D.Z, D.W * .55f), .2f);
                m.Torus(s * 135, 0, 0, 105, 9, C);
                m.Tube([V3(s * 238, -14), V3(s * 260, -20, -100), V3(s * 260, -10, -320)], 6, C);
                m.Limb(s * 180, -50, 8, s * 150, -14, 8, 3, 3, m.C(0xFFFFFF, .7f));
            }
            m.Tube([.. Sculpt.ArcPts(0, 20, 40, 3.5f, 5.92f, 8).Select(q => new Vector3(q, 0))], 7, C);
        });
        // 腕時計: 厚みのある文字盤、目盛り、回る針、上下のベルト
        Icon(d, typeof(S799Parameter), (m, t, C, D) =>
        {
            m.Box(0, -225, -20, 130, 220, 30, C, .4f); m.Box(0, 225, -20, 130, 220, 30, C, .4f);
            m.Disc(0, 0, 0, 158, 158, 60, m.C(0xB7BEC6), .4f);
            m.Disc(0, 0, 8, 132, 132, 50, D, .2f);
            for (int i = 0; i < 12; i++) { float a = i * .5236f; m.Box(MathF.Cos(a) * 118, MathF.Sin(a) * 118, 34, 14, 4, 4, m.C(0x20242B), .3f); }
            m.Limb(0, 0, 36, MathF.Cos(t * .5f - 1.57f) * 65, MathF.Sin(t * .5f - 1.57f) * 65, 36, 5, 4, m.C(0x20242B));
            m.Limb(0, 0, 40, MathF.Cos(t * 3f - 1.57f) * 105, MathF.Sin(t * 3f - 1.57f) * 105, 40, 2, 2, m.C(0xE5392F));
            m.Ball(0, 0, 42, 9, m.C(0x20242B));
            m.Disc(0, 0, 36, 130, 130, 4, m.C(0xFFFFFF, .15f), .2f);
        });
        // ネクタイ: 結び目と、ななめのしまの剣先
        Icon(d, typeof(S800Parameter), (m, t, C, D) =>
        {
            m.Slab(10, 50, C, -62, -250, 62, -250, 36, -165, -36, -165);
            Cloth(m, 30, C, -36, -165, 36, -165, 105, 190, 0, 305, -105, 190);
            for (int i = 0; i < 5; i++) m.Limb(-62, -90 + i * 72, 18, 62, -130 + i * 72, 18, 6, 6, D);
        });
        // Tシャツ: 厚みのある布の形(ふちは丸く)と胸の星
        Icon(d, typeof(S801Parameter), (m, t, C, D) =>
        {
            Cloth(m, 40, C, -95, -230, -300, -130, -235, -20, -155, -65, -155, 250, 155, 250, 155, -65, 235, -20, 300, -130, 95, -230, 50, -185, -50, -185);
            m.Slab(24, 6, D, StarPts(0, 20, 70, 5, .45f, 0f));
        });
        // ズボン: 2本の足に分かれた布、ウエストのベルト、ポケット
        Icon(d, typeof(S802Parameter), (m, t, C, D) =>
        {
            Cloth(m, 50, C, -160, -270, 160, -270, 185, 290, 50, 290, 0, -40, -50, 290, -185, 290);
            m.Box(0, -250, 0, 330, 36, 70, D, .3f);
            m.Limb(-105, -160, 28, -60, -205, 28, 3, 3, D); m.Limb(105, -160, 28, 60, -205, 28, 3, 3, D);
        });
        // スカート: 広がる台形の布とプリーツ、ウエスト
        Icon(d, typeof(S803Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 120, -190, 285, 210, 0, 210);
            for (int i = 0; i < 14; i++) { float a = i * MathF.Tau / 14f; m.Limb(MathF.Cos(a) * 122, -175, MathF.Sin(a) * 122, MathF.Cos(a) * 287, 205, MathF.Sin(a) * 287, 2.5f, 2.5f, D); }
            m.Cyl(0, -200, 0, 130, 44, D, .3f);
        });
        // ワンピース: 胸元から広がる服、ベルト、水玉
        Icon(d, typeof(S804Parameter), (m, t, C, D) =>
        {
            Cloth(m, 40, C, -85, -280, -40, -280, 0, -235, 40, -280, 85, -280, 105, -60, 265, 280, -265, 280, -105, -60);
            m.Box(0, -60, 0, 212, 26, 70, D, .3f);
            for (int i = 0; i < 9; i++) m.Ball(-180 + i * 45, 120 + i % 2 * 70, 24, 11, 11, 4, D);
        });
        // 靴下: L字の布、しま、つま先の色
        Icon(d, typeof(S805Parameter), (m, t, C, D) =>
        {
            Cloth(m, 60, C, -110, -270, 70, -270, 70, 70, 230, 145, 240, 250, 20, 265, -110, 120);
            for (int i = 0; i < 3; i++) m.Box(-20, -230 + i * 62, 0, 182, 26, 92, D, .3f);
            m.Ball(170, 205, 0, 75, 55, 48, D);
        });
        // 手袋: ふくらんだ手のひらと親指、編み目の手首
        Icon(d, typeof(S806Parameter), (m, t, C, D) =>
        {
            m.Super(0, -20, 0, 135, 185, 70, .7f, .7f, C);
            m.Turn(-130, 60, -.55f, () => m.Super(-135, -20, 10, 42, 95, 40, .8f, .8f, C));
            m.Box(0, 190, 0, 290, 76, 150, D, .4f);
            for (int i = -3; i <= 3; i++) m.Limb(i * 38, 155, 76, i * 38, 225, 76, 3, 3, m.C(0xD9C6A5));
        });
        // マフラー: 首に巻いた輪と、たれた端としま、房
        Icon(d, typeof(S807Parameter), (m, t, C, D) =>
        {
            m.Pitch(0, -120, 0, MathF.PI / 2.4f, () => m.Torus(0, -120, 0, 170, 62, C));
            m.Box(120, 80, 70, 130, 340, 40, C, .3f);
            for (int i = 0; i < 4; i++) m.Box(125, -30 + i * 72, 91, 132, 18, 4, D, .2f);
            for (int i = 0; i < 5; i++) m.Limb(72 + i * 26, 250, 70, 72 + i * 26, 290, 72, 4, 3, D);
        });
        // リュック: ふくらんだ本体、前ポケットとファスナー、持ち手
        Icon(d, typeof(S808Parameter), (m, t, C, D) =>
        {
            m.Tube([.. Sculpt.ArcPts(0, -200, 70, 3.14f, 6.28f, 10).Select(q => new Vector3(q, -30))], 9, D);
            m.Super(0, 20, -20, 185, 235, 130, .45f, .55f, C);
            m.Super(0, 120, 110, 130, 95, 50, .45f, .55f, m.C(0x2F7656));
            m.Box(0, 70, 158, 260, 9, 6, D, .3f);
            m.Box(0, -120, 106, 190, 30, 16, D, .4f);
            m.Ball(0, 120, 162, 14, D);
        });
        // ハンドバッグ: 台形のバッグ、持ち手、金具
        Icon(d, typeof(S809Parameter), (m, t, C, D) =>
        {
            m.Tube([.. Sculpt.ArcPts(0, -70, 135, 3.14f, 6.28f, 12).Select(q => new Vector3(q, 0))], 10, D);
            m.Slab(0, 160, C, -260, -70, 260, -70, 300, 230, -300, 230);
            m.Box(0, 10, 81, 500, 10, 4, m.C(0x8F3522), .2f);
            m.Box(0, 10, 86, 70, 62, 12, D, .3f);
        });
        // 長靴: 長い筒と足先、黒い底、上の折り返し
        Icon(d, typeof(S810Parameter), (m, t, C, D) =>
        {
            m.Super(-35, -40, 0, 105, 230, 95, .6f, .8f, C);
            m.Ball(110, 170, 0, 160, 80, 95, C);
            m.Box(60, 260, 0, 430, 44, 200, D, .4f);
            m.Cyl(-35, -260, 0, 112, 34, m.C(0xE5B820), .3f);
            m.Box(-80, -60, 92, 28, 210, 6, m.C(0xFFFFFF, .45f), .5f);
        });
        // ピアノの鍵盤: 黒い台、白い鍵と、上に出た黒い鍵
        Icon(d, typeof(S811Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, -30, 650, 330, 60, D, .1f);
            for (int i = 0; i < 9; i++) m.Box(-280 + i * 70, 15, 10, 64, 270, 40, C, .15f);
            foreach (int i in new[] { 0, 1, 3, 4, 5, 7 }) m.Box(-245 + i * 70, -55, 40, 38, 165, 40, D, .2f);
        });
        // ギター: くびれた胴(厚み)、サウンドホール、ネックとヘッド、弦
        Icon(d, typeof(S812Parameter), (m, t, C, D) => m.Turn(0, 0, .5f, () =>
        {
            m.Disc(0, 150, 0, 145, 145, 90, C, .2f); m.Disc(0, -5, 0, 105, 105, 90, C, .2f);
            m.Disc(0, 45, 44, 48, 48, 4, m.C(0x20242B), .2f);
            m.Box(0, -200, 30, 44, 330, 26, D, .3f);
            m.Box(0, -385, 30, 76, 84, 26, D, .3f);
            m.Box(0, 190, 46, 92, 14, 8, D, .3f);
            for (int i = -2; i <= 2; i++) m.Limb(i * 7, -360, 46, i * 7, 190, 48, .8f, .8f, m.C(0xFFFFFF));
        }));
        // バイオリン: くびれた胴、f字孔、ネックと渦巻き、テールピース、弦
        Icon(d, typeof(S813Parameter), (m, t, C, D) => m.Turn(0, 0, .45f, () =>
        {
            m.Ball(0, 135, 0, 125, 120, 45, C); m.Ball(0, -55, 0, 100, 100, 42, C); m.Box(0, 40, 0, 150, 120, 80, C, .5f);
            foreach (int s in Sides) m.Limb(s * 45, 10, 46, s * 45, 90, 46, 3, 3, m.C(0x20242B));
            m.Box(0, -195, 30, 28, 220, 20, D, .3f);
            m.Ball(0, -320, 30, 22, D);
            m.Slab(46, 6, D, -22, 120, 22, 120, 30, 215, -30, 215);
            for (int i = -1; i <= 1; i++) m.Limb(i * 7, -300, 44, i * 7, 190, 52, .8f, .8f, m.C(0xFFFFFF));
        }));
        // ドラム: 円柱の胴、白い打面、ジグザグの飾り、2本のスティック
        Icon(d, typeof(S814Parameter), (m, t, C, D) =>
        {
            m.Cyl(0, 60, 0, 210, 250, C, .1f);
            m.Super(0, -66, 0, 205, 4, 205, .2f, 1f, D);
            m.Pitch(0, -65, 0, MathF.PI / 2f, () => m.Torus(0, -65, 0, 210, 8, m.C(0xB7BEC6)));
            m.Pitch(0, 185, 0, MathF.PI / 2f, () => m.Torus(0, 185, 0, 210, 8, m.C(0xB7BEC6)));
            for (int i = 0; i < 12; i++) { float a0 = i * MathF.Tau / 12f, a1 = a0 + MathF.Tau / 24f; m.Limb(MathF.Cos(a0) * 212, -40, MathF.Sin(a0) * 212, MathF.Cos(a1) * 212, 160, MathF.Sin(a1) * 212, 3, 3, m.C(0xFFD54A)); }
            m.Limb(-235, -280, 40, -30, -90, 40, 6, 5, m.C(0xD9A566)); m.Limb(235, -280, 40, 30, -90, 40, 6, 5, m.C(0xD9A566));
        });
        // トランペット: 管、広がるベル(回転体)、巻いた管、3本のピストン
        Icon(d, typeof(S815Parameter), (m, t, C, D) =>
        {
            m.Limb(-275, 0, 0, 160, 0, 0, 12, 12, C);
            m.Turn(130, 0, -MathF.PI / 2f, () => m.Lathe(130, 0, C, 12, 0, 30, 100, 115, 170, 0, 168));
            m.Tube([V3(-140, 0), V3(-140, 45), V3(80, 45), V3(80, 90), V3(-140, 90)], 11, C);
            for (int i = 0; i < 3; i++) { m.Cyl(-95 + i * 62, -35, 0, 13, 110, D, .2f); m.Disc(-95 + i * 62, -96, 0, 17, 17, 12, C, .3f); m.Pitch(-95 + i * 62, -96, 0, MathF.PI / 2f, () => m.Disc(-95 + i * 62, -96, 0, 17, 17, 10, C, .3f)); }
            m.Limb(-300, 0, 0, -270, 0, 0, 20, 12, D);
        });
        // フルート: 長い銀の管と、並んだキー、歌口
        Icon(d, typeof(S816Parameter), (m, t, C, D) => m.Turn(0, 0, -.3f, () =>
        {
            m.Limb(-330, 0, 0, 330, 0, 0, 18, 18, C);
            for (int i = 0; i < 9; i++) m.Disc(-210 + i * 52, 0, 17, 12, 12, 6, D, .4f);
            m.Ball(-295, 0, 16, 18, 10, 4, m.C(0x20242B));
        }));
        // ハープ: 曲がった木の枠と台、張られた弦
        Icon(d, typeof(S817Parameter), (m, t, C, D) =>
        {
            m.Tube([V3(-180, 280), V3(-180, -250), V3(-20, -250), V3(140, -180), V3(240, -105), V3(170, 10), V3(-180, 280)], 13, C);
            m.Box(-20, 285, 0, 370, 30, 70, C, .3f);
            for (int i = 0; i < 8; i++) { float x = -145 + i * 42; m.Limb(x, -235 + i * 16, 0, x, 270, 0, 1.3f, 1.3f, D); }
        });
        // 木琴: 2本の台と、長さのちがう色とりどりの板
        Icon(d, typeof(S818Parameter), (m, t, C, D) =>
        {
            uint[] c0 = [0xE5392F, 0xFF9A3C, 0xFFD54A, 0x4FAE4F, 0x2F86C6, 0x7B3FA8];
            m.Limb(-300, -100, -20, 290, -55, -20, 8, 8, D); m.Limb(-300, 100, -20, 290, 55, -20, 8, 8, D);
            for (int i = 0; i < 6; i++) m.Box(-250 + i * 100, 0, 10, 76, 300 - i * 28, 30, m.C(c0[i]), .25f);
        });
        // タンバリン: 皮を張った輪、まわりの鈴(小さなシンバル)、星
        Icon(d, typeof(S819Parameter), (m, t, C, D) =>
        {
            m.Disc(0, 0, 0, 225, 225, 10, m.C(0xFFF3D6), .2f);
            m.Torus(0, 0, -20, 235, 22, C);
            for (int i = 0; i < 8; i++) { float a = i * .785f; m.Yaw(MathF.Cos(a) * 235, MathF.Sin(a) * 235, -20, MathF.PI / 2f, () => { m.Disc(MathF.Cos(a) * 235, MathF.Sin(a) * 235, -32, 24, 24, 4, m.C(0xD5DBE1), .3f); m.Disc(MathF.Cos(a) * 235, MathF.Sin(a) * 235, -8, 24, 24, 4, m.C(0xD5DBE1), .3f); }); }
            m.Slab(8, 6, D, StarPts(0, 0, 90, 5, .45f, 0f));
        });
        // マラカス: 丸い頭(白い帯)と持ち手の2本
        Icon(d, typeof(S820Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Turn(s * 130, 0, s * .3f, () =>
            {
                m.Lathe(s * 130, 0, m.C(0xB9823E), 12, 70, 16, 310, 0, 310);
                m.Ball(s * 130, -80, 0, 105, 135, 105, s < 0 ? C : D);
                m.Cyl(s * 130, -80, 0, 106, 20, m.C(0xFFFFFF), .2f);
            });
        });
    }
}
