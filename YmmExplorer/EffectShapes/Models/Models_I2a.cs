using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 214_QIcons2.cs のアイコン 821〜850(和楽器・工具・掃除道具・動物の顔 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    /// <summary>頭(楕円体)の正面に、光の入った目を2つ</summary>
    static void HeadEyes(Sculpt m, float hx, float hy, float hz, float rx, float ry, float rz, float dx, float y, float r)
    {
        foreach (int s in Sides)
        {
            float ex = hx + s * dx, ez = Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, ex, y);
            m.Ball(ex, y, ez - r * .3f, r, r, r * .6f, m.C(0x20242B));
            m.Ball(ex - r * .3f, y - r * .35f, ez + r * .25f, r * .32f, r * .32f, r * .15f, m.C(0xFFFFFF));
        }
    }

    /// <summary>頭の正面に沿う口の線(ω形)</summary>
    static void MouthW(Sculpt m, float hx, float hy, float hz, float rx, float ry, float rz, float x, float y, float r, float w)
    {
        foreach (int s in Sides) m.Tube(Sculpt.OnFront(hx, hy, hz, rx, ry, rz, 1f, Sculpt.ArcPts(x + s * r, y, r, 0, MathF.PI, 8)), w, m.C(0x20242B));
    }

    static partial void RegisterI2a(Dictionary<Type, QuickModel> d)
    {
        var K = new Vector4(.125f, .14f, .17f, 1f);
        // 三味線: 四角い胴(皮)、長いさお、糸巻き、3本の糸、ばち
        Icon(d, typeof(S821Parameter), (m, t, C, D) => m.Turn(0, 0, .45f, () =>
        {
            m.Box(0, 170, 0, 230, 220, 80, C, .3f);
            m.Box(0, 170, 41, 185, 175, 4, D, .1f);
            m.Box(0, -130, 10, 26, 430, 24, C, .3f);
            m.Box(0, -360, 10, 50, 70, 30, C, .3f);
            foreach (int s in Sides) m.Limb(0, -370, 10, s * 50, -370, 10, 5, 5, C);
            for (int i = -1; i <= 1; i++) m.Limb(i * 7, -340, 24, i * 7, 240, 44, .8f, .8f, m.C(0xFFFFFF));
            m.Slab(70, 8, m.C(0xD9A566), -60, 130, 60, 130, 0, 235);
        }));
        // 和太鼓: ふくらんだ胴、両面の皮、まわりの鋲、巴の模様
        Icon(d, typeof(S822Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 10, -40, 255, 230, 128, C);
            m.Disc(0, 10, 92, 205, 210, 8, D, .2f);
            for (int i = 0; i < 18; i++) { float a = i * .349f; m.Ball(MathF.Cos(a) * 225, 10 + MathF.Sin(a) * 216, 90, 8, K); }
            m.Tube([.. Sculpt.ArcPts(-45, 10, 62, 0, 3.14f, 10).Select(q => new Vector3(q, 98))], 7, K);
            m.Tube([.. Sculpt.ArcPts(45, 10, 62, 3.14f, 6.28f, 10).Select(q => new Vector3(q, 98))], 7, K);
        });
        // ハーモニカ: 金属のふた、黒い本体、並んだ吹き穴
        Icon(d, typeof(S823Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, 0, 580, 185, 110, C, .25f);
            m.Box(0, 0, 50, 548, 62, 20, D, .1f);
            for (int i = 0; i < 10; i++) m.Box(-225 + i * 50, 0, 58, 28, 34, 10, K, .2f);
        });
        // リコーダー: 白い管、吹き口、継ぎ目の帯、指穴
        Icon(d, typeof(S824Parameter), (m, t, C, D) => m.Turn(0, 0, .4f, () =>
        {
            m.Lathe(0, 0, C, 22, -335, 34, -285, 36, 0, 40, 290, 0, 295);
            m.Cyl(0, -150, 0, 37, 16, D, .2f); m.Cyl(0, 160, 0, 39, 16, D, .2f);
            for (int i = 0; i < 7; i++) m.Ball(0, -105 + i * 40, 34, 10, 10, 4, K);
        }));
        // ハンマー: 木の柄と、金属の頭(くぎ抜き付き)
        Icon(d, typeof(S825Parameter), (m, t, C, D) => m.Turn(0, 0, -.55f, () =>
        {
            m.Lathe(0, 0, D, 20, -140, 24, 290, 0, 295);
            m.Turn(-130, -185, MathF.PI / 2f, () => m.Cyl(-130, -185, 0, 52, 30, C, .2f));
            m.Box(0, -185, 0, 265, 90, 80, C, .2f);
            m.Slab(0, 60, C, 130, -235, 195, -210, 195, -160, 130, -135);
        }));
        // のこぎり: 薄い刃とギザギザの歯、穴の空いた持ち手
        Icon(d, typeof(S826Parameter), (m, t, C, D) => m.Turn(0, 0, -.25f, () =>
        {
            m.Slab(0, 6, C, -310, -60, 130, -105, 130, 70, -310, 20);
            for (int i = 0; i < 14; i++) m.Slab(0, 4, C, -310 + i * 31, 18, -295 + i * 31, 50, -279 + i * 31, 24);
            var grip = new List<float> { 120, -85, 300, -85, 300, 45, 120, 45, 120, -85 };
            m.Box(210, -60, 0, 180, 50, 50, D, .5f); m.Box(210, 20, 0, 180, 50, 50, D, .5f); m.Box(290, -20, 0, 30, 130, 50, D, .5f);
        }));
        // ドライバー: 溝のある太い握り、細い軸、平たい先
        Icon(d, typeof(S827Parameter), (m, t, C, D) => m.Turn(0, 0, .6f, () =>
        {
            m.Super(0, 160, 0, 46, 130, 46, .5f, 1f, C);
            for (int i = 0; i < 6; i++) { float a = i * MathF.Tau / 6f; m.Limb(MathF.Cos(a) * 46, 60, MathF.Sin(a) * 46, MathF.Cos(a) * 46, 260, MathF.Sin(a) * 46, 5, 5, m.C(0xC89B23)); }
            m.Cyl(0, -110, 0, 12, 330, D, .1f);
            m.Slab(0, 6, D, -12, -275, 12, -275, 6, -325, -6, -325);
        }));
        // スパナ: 平たい柄、口の開いた頭、穴
        Icon(d, typeof(S828Parameter), (m, t, C, D) => m.Turn(0, 0, .65f, () =>
        {
            m.Box(0, 50, 0, 72, 430, 26, C, .3f);
            var head = new List<float>();
            for (int i = 0; i <= 24; i++) { float a = -MathF.PI / 2f + .5f + (MathF.Tau - 1f) * i / 24f; head.Add(MathF.Cos(a) * 88); head.Add(-190 + MathF.Sin(a) * 88); }
            head.AddRange([28, -190 - 40, -28, -190 - 40]);
            m.Slab(0, 30, C, [.. head]);
            m.Torus(0, 275, 0, 30, 8, D);
        }));
        // ペンチ: 開いたり閉じたりする2本の口、赤い持ち手、中心の軸
        Icon(d, typeof(S829Parameter), (m, t, C, D) =>
        {
            float o = .09f + MathF.Abs(MathF.Sin(t * 2.5f)) * .08f;
            foreach (int s in Sides) m.Turn(0, -40, s * o, () =>
            {
                m.Slab(s * 10, 18, C, s * 2, -280, s * 52, -240, s * 48, -20, s * 2, -20);
                m.Tube([V3(s * 18, -20, s * 10), V3(s * 52, 110, s * 10), V3(s * 42, 290, s * 10)], 16, D);
            });
            m.Disc(0, -40, 0, 24, 24, 46, m.C(0xD5DBE1), .4f);
        });
        // 電動ドリル: 本体と持ち手、電池、先のチャックと回るドリル
        Icon(d, typeof(S830Parameter), (m, t, C, D) =>
        {
            m.Box(-25, -80, 0, 380, 130, 120, C, .45f);
            m.Slab(0, 100, C, -120, -20, 10, -20, -20, 250, -145, 250);
            m.Box(-90, 265, 0, 170, 70, 130, K, .3f);
            m.Limb(160, -80, 0, 220, -80, 0, 40, 26, m.C(0x8A95A0));
            m.Limb(220, -80, 0, 330, -80, 0, 7, 2, D);
            for (int i = 0; i < 4; i++) { float a = t * 20f + i; m.Limb(225 + i * 25, -80 + MathF.Sin(a) * 9, MathF.Cos(a) * 9, 245 + i * 25, -80 - MathF.Sin(a) * 9, -MathF.Cos(a) * 9, 2, 2, K); }
        });
        // 巻尺: 角の丸いケース、丸い巻き、のびたテープと目盛り、先の爪
        Icon(d, typeof(S831Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, 0, 330, 300, 120, C, .5f);
            m.Disc(0, 0, 60, 102, 102, 10, D, .3f); m.Disc(0, 0, 66, 42, 42, 8, C, .3f);
            m.Box(235, 110, 0, 160, 44, 4, m.C(0xF8EFA8), .1f);
            for (int i = 0; i < 8; i++) m.Box(165 + i * 20, 90 + (i % 2) * 6, 3, 3, 15 + i % 2 * 12, 2, K, .2f);
            m.Box(315, 110, 0, 12, 70, 20, K, .2f);
        });
        // 脚立: 前後に開いた2組の脚と、天板、段
        Icon(d, typeof(S832Parameter), (m, t, C, D) =>
        {
            foreach (int z in new[] { -90, 90 })
            {
                m.Limb(-105, -280, z * .3f, -235, 285, z, 10, 10, C); m.Limb(105, -280, z * .3f, 235, 285, z, 10, 10, C);
            }
            m.Box(0, -290, 0, 245, 38, 90, D, .3f);
            for (int i = 0; i < 4; i++) { float y = -160 + i * 105, w = 135 + i * 30, z = 90 * (y + 280) / 565f; m.Box(0, y, z, w * 2, 20, 40, C, .3f); }
        });
        // スコップ: 木の柄とにぎり、くぼんだ金属の刃
        Icon(d, typeof(S833Parameter), (m, t, C, D) =>
        {
            m.Cyl(0, -55, 0, 17, 360, D, .2f);
            m.Turn(0, -250, MathF.PI / 2f, () => m.Cyl(0, -250, 0, 17, 130, K, .3f));
            m.Pitch(0, 110, 0, -.25f, () => { m.Slab(0, 10, C, -110, 110, 110, 110, 105, 270, 0, 335, -105, 270); m.Box(0, 118, 0, 230, 16, 24, m.C(0xB7BEC6), .3f); });
        });
        // じょうろ: 台形の胴と上の持ち手、のびた注ぎ口とはす口、こぼれる水
        Icon(d, typeof(S834Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 0, -110, 135, -110, 178, 210, 0, 210);
            m.Tube([.. Sculpt.ArcPts(-20, -110, 110, 3.14f, 6.28f, 12).Select(q => new Vector3(q, 0))], 12, C);
            m.Limb(-120, 30, 0, -285, -145, 0, 32, 16, C);
            m.Turn(-300, -165, -MathF.PI / 4f, () => m.Lathe(-300, 0, D, 16, -165, 46, -215, 0, -212));
            int keep = m.Seg; m.Seg = 8;
            for (int i = 0; i < 8; i++) { float k = Fr(t * .9f + i / 8f); m.Ball(-340 - k * 40 - i * 8, -120 + k * 260, (R01(i) - .5f) * 60, 6, m.C(0x8FD8F7, 1f - k)); }
            m.Seg = keep;
        });
        // バケツ: 広がった筒と水、ふち、持ち手
        Icon(d, typeof(S835Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 0, 245, 155, 245, 210, -120, 200, -120, 150, 230, 0, 230);
            m.Super(0, -100, 0, 195, 3, 195, .3f, 1f, m.C(0x8FD8F7, .9f));
            m.Pitch(0, -120, 0, MathF.PI / 2f, () => m.Torus(0, -120, 0, 210, 9, m.C(0x2F6FBB)));
            m.Tube([.. Sculpt.ArcPts(0, -120, 205, 3.14f, 6.28f, 14).Select(q => new Vector3(q, 0))], 6, D);
        });
        // ほうき: 長い柄、赤い帯、広がる穂
        Icon(d, typeof(S836Parameter), (m, t, C, D) =>
        {
            m.Cyl(0, -120, 0, 14, 410, D, .2f);
            m.Lathe(0, 0, C, 35, 70, 175, 315, 0, 315);
            m.Cyl(0, 86, 0, 58, 26, m.C(0xE5392F), .2f);
            for (int i = 0; i < 16; i++) { float a = i * MathF.Tau / 16f; m.Limb(MathF.Cos(a) * 36, 95, MathF.Sin(a) * 36, MathF.Cos(a) * 172, 312, MathF.Sin(a) * 172, 2, 2, m.C(0xB9892F)); }
        });
        // ちりとり: 前に傾いた受け皿と立ち上がった背、長い柄
        Icon(d, typeof(S837Parameter), (m, t, C, D) =>
        {
            m.Quad3D(V3(-240, 20, 0), V3(240, 20, 0), V3(180, 220, 160), V3(-180, 220, 160), C);
            m.Box(0, -18, -5, 480, 80, 14, m.C(0xC95528), .2f);
            foreach (int s in Sides) m.Quad3D(V3(s * 240, 20, 0), V3(s * 180, 220, 160), V3(s * 180, 160, 160), V3(s * 240, -55, 0), m.C(0xC95528));
            m.Cyl(0, -175, -10, 17, 260, D, .3f);
            m.Turn(0, -300, MathF.PI / 2f, () => m.Cyl(0, -300, -10, 15, 120, D, .3f));
        });
        // 手持ちライト: 筒の本体、広がる頭、光るレンズ、前に広がる光
        Icon(d, typeof(S838Parameter), (m, t, C, D) =>
        {
            m.Limb(-270, 0, 0, 30, 0, 0, 55, 55, C);
            m.Limb(20, 0, 0, 110, 0, 0, 55, 105, C);
            m.Ball(118, 0, 0, 10, 100, 100, D);
            m.Box(-110, -52, 0, 60, 24, 30, m.C(0xE5392F), .4f);
            float a = .28f + .1f * MathF.Abs(MathF.Sin(t * 2f));
            m.Turn(125, 0, -MathF.PI / 2f, () => m.Lathe(125, 0, m.C(D, a), 100, 0, 260, 215, 0, 0));
        });
        // ねこの顔: 丸い頭と三角の耳、白い口元、目・鼻・口とひげ
        Icon(d, typeof(S839Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) { m.Slab(-60, 60, C, s * 205, -90, s * 185, -285, s * 55, -190); m.Slab(-28, 8, m.C(0xFF9FB0), s * 175, -115, s * 168, -240, s * 80, -185); }
            m.Ball(0, 10, 0, 240, 205, 190, C);
            m.Ball(0, 75, 110, 120, 92, 85, D);
            HeadEyes(m, 0, 10, 0, 240, 205, 190, 95, -20, 28);
            m.Ball(0, 60, 192, 16, 11, 10, m.C(0xE5787A));
            MouthW(m, 0, 75, 110, 120, 92, 85, 0, 80, 28, 3);
            foreach (int s in Sides) for (int i = -1; i <= 1; i++) m.Limb(s * 120, 55 + i * 20, 150, s * 260, 40 + i * 42, 90, 1.6f, 1.2f, K);
        });
        // いぬの顔: 丸い頭と垂れ耳、前に出た口元、目と鼻と口
        Icon(d, typeof(S840Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Yaw(s * 205, -30, -20, -s * .35f, () => m.Ball(s * 205, -20, -20, 62, 150, 30, D));
            m.Ball(0, 10, 0, 220, 205, 190, C);
            m.Ball(0, 85, 115, 110, 84, 80, m.C(0xFFF3E0));
            HeadEyes(m, 0, 10, 0, 220, 205, 190, 88, -25, 26);
            m.Ball(0, 50, 188, 34, 24, 20, K);
            m.Tube(Sculpt.OnFront(0, 85, 115, 110, 84, 80, 1f, [new(0, 74), new(0, 105)]), 3, K);
            MouthW(m, 0, 85, 115, 110, 84, 80, 0, 105, 26, 3);
        });
        // うさぎの顔: 長い耳(内側はピンク)、丸い顔、ほっぺ
        Icon(d, typeof(S841Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Pitch(s * 88, -100, -10, -.15f, () => { m.Ball(s * 88, -210, -10, 58, 145, 36, C); m.Ball(s * 88, -205, 18, 28, 105, 12, D); });
            m.Ball(0, 70, 0, 210, 180, 175, C);
            HeadEyes(m, 0, 70, 0, 210, 180, 175, 85, 35, 24);
            m.Ball(0, 92, Sculpt.FrontZ(0, 70, 0, 210, 180, 175, 0, 92), 16, 11, 8, D);
            MouthW(m, 0, 70, 0, 210, 180, 175, 0, 108, 18, 2.5f);
            foreach (int s in Sides) m.Ball(s * 140, 105, Sculpt.FrontZ(0, 70, 0, 210, 180, 175, s * 140, 105) - 3, 34, 18, 6, m.C(D, .6f));
        });
        // くまの顔: 丸い耳(内側)、丸い顔、前に出た口元、鼻と口
        Icon(d, typeof(S842Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) { m.Ball(s * 175, -165, -30, 78, 78, 40, C); m.Ball(s * 175, -165, 2, 40, 40, 10, D); }
            m.Ball(0, 20, 0, 230, 230, 200, C);
            m.Ball(0, 90, 130, 105, 82, 80, D);
            HeadEyes(m, 0, 20, 0, 230, 230, 200, 92, -20, 26);
            m.Ball(0, 55, 200, 32, 22, 18, K);
            m.Tube(Sculpt.OnFront(0, 90, 130, 105, 82, 80, 1f, Sculpt.ArcPts(0, 90, 34, .4f, 2.74f, 10)), 3, K);
        });
        // パンダの顔: 黒い耳、白い顔、ななめの黒い目のまわり、鼻と口
        Icon(d, typeof(S843Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Ball(s * 175, -170, -30, 75, 75, 40, D);
            m.Ball(0, 20, 0, 230, 230, 200, C);
            foreach (int s in Sides) { float ez = Sculpt.FrontZ(0, 20, 0, 230, 230, 200, s * 92, -5); m.Turn(s * 92, -5, s * .45f, () => m.Ball(s * 92, -5, ez - 14, 48, 64, 22, D)); }
            foreach (int s in Sides) { float ez = Sculpt.FrontZ(0, 20, 0, 230, 230, 200, s * 92, -8); m.Ball(s * 92, -8, ez + 6, 20, 20, 8, K); m.Ball(s * 92 - 6, -15, ez + 12, 7, 7, 3, m.C(0xFFFFFF)); }
            m.Ball(0, 70, Sculpt.FrontZ(0, 20, 0, 230, 230, 200, 0, 70), 26, 18, 12, D);
            m.Tube(Sculpt.OnFront(0, 20, 0, 230, 230, 200, 1f, Sculpt.ArcPts(0, 95, 30, .4f, 2.74f, 10)), 3, D);
        });
        // ぶたの顔: 三角の耳、丸い顔、前に出た鼻と鼻の穴
        Icon(d, typeof(S844Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Pitch(s * 140, -180, -30, .4f, () => m.Slab(-30, 30, C, s * 190, -110, s * 185, -260, s * 65, -185));
            m.Ball(0, 20, 0, 230, 230, 200, C);
            m.Disc(0, 75, 185, 95, 68, 60, D, .45f);
            foreach (int s in Sides) m.Ball(s * 34, 75, 215, 15, 23, 6, K);
            HeadEyes(m, 0, 20, 0, 230, 230, 200, 105, -35, 24);
        });
        // さるの顔: 横の耳、目のまわりと口元の明るい顔、目・鼻の穴・口
        Icon(d, typeof(S845Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) { m.Ball(s * 225, 0, -20, 70, 70, 36, D); m.Ball(s * 225, 0, 10, 40, 40, 10, C); }
            m.Ball(0, 10, 0, 215, 215, 190, C);
            foreach (int s in Sides) m.Ball(s * 72, -35, Sculpt.FrontZ(0, 10, 0, 215, 215, 190, s * 72, -35) - 40, 82, 82, 50, D);
            m.Ball(0, 85, 140, 142, 90, 70, D);
            foreach (int s in Sides) { float ez = Sculpt.FrontZ(0, 10, 0, 215, 215, 190, s * 72, -35) + 6; m.Ball(s * 72, -35, ez, 22, 22, 10, K); m.Ball(s * 72 - 7, -43, ez + 8, 7, 7, 3, m.C(0xFFFFFF)); }
            foreach (int s in Sides) m.Ball(s * 12, 55, 206, 5, K);
            m.Tube(Sculpt.OnFront(0, 85, 140, 142, 90, 70, 1f, Sculpt.ArcPts(0, 90, 62, .3f, 2.84f, 12)), 3.5f, K);
        });
        // きつねの顔: 大きな三角の耳、とがった鼻先、白いほお、目と鼻
        Icon(d, typeof(S846Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Slab(-40, 50, C, s * 215, -90, s * 190, -290, s * 55, -175);
            m.Ball(0, -20, 0, 240, 150, 160, C);
            m.Limb(0, 20, 60, 0, 230, 160, 130, 26, C);
            foreach (int s in Sides) m.Ball(s * 100, 95, 95, 85, 62, 50, D);
            HeadEyes(m, 0, -20, 0, 240, 150, 160, 92, -5, 22);
            m.Ball(0, 235, 182, 26, 20, 18, K);
        });
        // たぬきの顔: 黒い耳、目のまわりの黒い模様、白い口元、鼻
        Icon(d, typeof(S847Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Ball(s * 170, -160, -30, 64, 64, 34, D);
            m.Ball(0, 25, 0, 235, 205, 190, C);
            foreach (int s in Sides) { float ez = Sculpt.FrontZ(0, 25, 0, 235, 205, 190, s * 95, 0); m.Ball(s * 95, 0, ez - 20, 76, 62, 30, D); m.Ball(s * 95, 0, ez + 6, 20, 20, 8, m.C(0xFFFFFF)); m.Ball(s * 95, 0, ez + 12, 10, 10, 4, K); }
            m.Ball(0, 110, 140, 118, 86, 70, m.C(0xF3E5CC));
            m.Ball(0, 70, 200, 26, 18, 14, K);
        });
        // ライオンの顔: まわりを囲むふさふさのたてがみ、顔、口元、目・鼻・口
        Icon(d, typeof(S848Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 16;
            for (int i = 0; i < 16; i++) { float a = i * .3927f + MathF.Sin(t + i) * .02f; m.Ball(MathF.Cos(a) * 235, 20 + MathF.Sin(a) * 220, -40, 68, 68, 60, D); }
            for (int i = 0; i < 12; i++) { float a = i * .5236f + .2f; m.Ball(MathF.Cos(a) * 190, 20 + MathF.Sin(a) * 180, -90, 70, D); }
            m.Seg = keep;
            m.Ball(0, 20, 0, 205, 205, 175, C);
            m.Ball(0, 100, 125, 92, 70, 70, m.C(0xFFF3D6));
            HeadEyes(m, 0, 20, 0, 205, 205, 175, 82, -20, 24);
            m.Slab(190, 14, K, -28, 62, 28, 62, 0, 95);
            m.Tube(Sculpt.OnFront(0, 100, 125, 92, 70, 70, 1f, Sculpt.ArcPts(0, 115, 38, .35f, 2.8f, 10)), 3, K);
        });
        // ぞうの顔: 大きな耳(内側はピンク)、丸い頭、しわのある長い鼻
        Icon(d, typeof(S849Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Yaw(s * 200, -20, -40, -s * .5f, () => { m.Ball(s * 225, -20, -40, 125, 175, 26, C); m.Ball(s * 225, -15, -20, 76, 118, 10, D); });
            m.Ball(0, -25, 0, 190, 190, 175, C);
            var trunk = new List<Vector3> { V3(0, 40, 140), V3(0, 140, 175), V3(10, 230, 170), V3(30, 285, 150) };
            m.Tube(trunk, 44, C);
            for (int i = 0; i < 4; i++) m.Torus(0, 105 + i * 42, 165 - i * 2, 44, 2.5f, m.C(0x6B8097));
            HeadEyes(m, 0, -25, 0, 190, 190, 175, 85, -45, 22);
        });
        // きりん: 長い首(もよう)、頭と角、耳、目と鼻
        Icon(d, typeof(S850Parameter), (m, t, C, D) =>
        {
            m.Limb(0, 300, -20, 10, -140, 0, 70, 40, C);
            for (int i = 0; i < 9; i++) { float y = -50 + i * 38, r = 40 + (y + 140) / 440f * 30; float a = (i % 3 - 1) * .7f; m.Ball(MathF.Sin(a) * r, y, MathF.Cos(a) * r - 10, 18, 14, 6, D); }
            m.Ball(40, -205, 0, 120, 72, 70, C);
            m.Ball(140, -185, 0, 40, 30, 50, C);
            foreach (int s in Sides) { m.Limb(30, -260, s * 25, 30, -320, s * 25, 6, 5, D); m.Ball(30, -325, s * 25, 13, D); }
            m.Ball(-60, -240, 0, 40, 18, 18, C);
            m.Ball(72, -225, 60, 16, 16, 10, K); m.Ball(72, -225, -60, 16, 16, 10, K);
            m.Ball(170, -180, 30, 10, 6, 4, D);
        });
    }
}
