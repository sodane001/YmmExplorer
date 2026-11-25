using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 213_QIcons1.cs のアイコン 761〜790(飲み物・台所・日用品・文房具 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    /// <summary>湯気・泡の白い玉が昇る</summary>
    static void Puffs(Sculpt m, int n, float x, float y, float dx, float rise, float r0, float r1, float t, float speed, float a)
    {
        int keep = m.Seg; m.Seg = 12;
        for (int i = 0; i < n; i++) { float k = Fr(t * speed + i / (float)n); m.Ball(x + k * dx + MathF.Sin(k * 6f + i) * 10f, y - k * rise, MathF.Cos(k * 5f + i) * 20f, r0 + k * r1, m.C(0xFFFFFF, a * (1f - k))); }
        m.Seg = keep;
    }

    static partial void RegisterI1c(Dictionary<Type, QuickModel> d)
    {
        // タピオカドリンク: 透明なカップ、ミルクティー、底の黒い粒、ふた、太いストロー
        Icon(d, typeof(S761Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 0, -150, 138, -150, 105, 295, 0, 295);
            int keep = m.Seg; m.Seg = 12;
            for (int i = 0; i < 26; i++) { float a = i * 2.4f, r = 30 + (i % 4) * 22, y = 270 - (i / 8) * 40; m.Ball(MathF.Cos(a) * r, y, MathF.Sin(a) * r, 20, D); }
            m.Seg = keep;
            m.Lathe(0, 0, m.C(0xFFFFFF, .35f), 145, -185, 108, 300, 0, 300);
            m.Super(0, -184, 0, 145, 10, 145, .4f, 1f, m.C(0xFFFFFF, .9f));
            m.Limb(35, -310, 0, 5, 230, 0, 12, 12, m.C(0xFF5C8A));
        });
        // 急須: 丸い胴、注ぎ口、横の取っ手、ふたとつまみ、昇る湯気
        Icon(d, typeof(S762Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 50, 0, 200, 165, 180, C);
            m.Limb(170, 40, 0, 300, -80, 0, 36, 14, C);
            m.Limb(-180, 50, 0, -300, 50, 0, 26, 30, D);
            m.Super(0, -110, 0, 110, 18, 110, .5f, 1f, m.C(0x365043));
            m.Ball(0, -134, 0, 20, D);
            Puffs(m, 3, 320, -100, 30, 130, 10, 16, t, .5f, .45f);
        });
        // カクテル: 逆三角すいのグラスと中のお酒、細い脚と台、レモン、ストロー
        Icon(d, typeof(S763Parameter), (m, t, C, D) =>
        {
            var glass = new Vector4(D.X, D.Y, D.Z, D.W * .35f);
            m.Lathe(0, 0, new Vector4(C.X, C.Y, C.Z, C.W * .9f), 0, -180, 205, -180, 0, 70);
            m.Lathe(0, 0, glass, 225, -200, 0, 80);
            m.Cyl(0, 180, 0, 6, 200, new Vector4(D.X, D.Y, D.Z, D.W * .8f), .3f);
            m.Super(0, 292, 0, 105, 7, 105, .4f, 1f, new Vector4(D.X, D.Y, D.Z, D.W * .8f));
            m.Disc(170, -205, 0, 48, 48, 14, m.C(0xFFD54A), .4f);
            m.Limb(50, -90, 0, 140, -300, 0, 4, 4, m.C(0x4FAE4F));
        });
        // フォークとナイフ: 先の分かれたフォーク(4本の歯)と、刃と柄のナイフ
        Icon(d, typeof(S764Parameter), (m, t, C, D) =>
        {
            for (int i = -1; i <= 2; i++) m.Box(-130 + (i - .5f) * 30, -210, 0, 12, 160, 12, C, .4f);
            m.Box(-130, -120, 0, 108, 42, 14, C, .5f);
            m.Box(-130, 90, 0, 34, 380, 18, C, .5f);
            m.Slab(0, 10, C, 105, -300, 175, -210, 175, 40, 105, 40);
            m.Box(125, 170, 0, 42, 280, 26, D, .5f);
        });
        // お箸: 2本の細長い箸(先が細い)と箸置き
        Icon(d, typeof(S765Parameter), (m, t, C, D) =>
        {
            m.Limb(-185, -280, 0, 75, 280, 20, 9, 5, C);
            m.Limb(-115, -300, -10, 120, 270, 10, 9, 5, C);
            m.Box(-90, 180, 30, 120, 36, 50, D, .5f);
        });
        // スプーン: くぼんだ丸い先と、長い柄
        Icon(d, typeof(S766Parameter), (m, t, C, D) => m.Turn(0, 0, .5f, () =>
        {
            m.Ball(0, -190, 0, 90, 125, 26, C);
            m.Ball(-10, -200, 18, 60, 95, 10, Vector4.Lerp(C, m.C(0x8A95A0), .4f));
            m.Box(0, 120, 0, 38, 430, 16, D, .5f);
        }));
        // 包丁: 薄い刃(反った先)と、木の柄・目くぎ
        Icon(d, typeof(S767Parameter), (m, t, C, D) => m.Turn(0, 0, -.45f, () =>
        {
            m.Slab(0, 8, C, -320, -80, 110, -80, 110, 70, -180, 50);
            m.Limb(-320, -80, 0, -180, 50, 0, 2, 2, m.C(0xFFFFFF));
            m.Box(230, -10, 0, 250, 62, 40, D, .45f);
            m.Ball(190, -10, 20, 6, m.C(0xFFFFFF)); m.Ball(280, -10, 20, 6, m.C(0xFFFFFF));
        }));
        // まな板: 厚い木の板と木目、穴、上の野菜
        Icon(d, typeof(S768Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, 0, 620, 400, 46, C, .15f);
            for (int i = -3; i <= 3; i++) m.Limb(-280, i * 42, 24, 280, i * 42 + 8, 24, 1.2f, 1.2f, D);
            m.Torus(245, -125, 0, 18, 6, D);
            m.Ball(-110, 40, 50, 90, 46, 40, m.C(0xE5532F));
        });
        // 片手鍋: 円柱の鍋、ふち、長い取っ手、ふたのつまみ、湯気
        Icon(d, typeof(S769Parameter), (m, t, C, D) =>
        {
            m.Cyl(-50, 70, 0, 210, 250, C, .2f);
            m.Pitch(-50, -55, 0, MathF.PI / 2f, () => m.Torus(-50, -55, 0, 214, 10, m.C(0xDDE2E8)));
            m.Super(-50, -58, 0, 205, 6, 205, .3f, 1f, m.C(0xA8B1BA));
            m.Box(290, -20, 0, 270, 44, 40, D, .5f);
            Puffs(m, 3, -50, -100, 0, 150, 14, 20, t, .5f, .4f);
        });
        // フライ返し: すき間のある平らな先と、細い柄、黒い持ち手
        Icon(d, typeof(S770Parameter), (m, t, C, D) => m.Turn(0, 0, .4f, () =>
        {
            m.Pitch(0, -90, 0, .3f, () =>
            {
                foreach (float x in new[] { -88f, -29f, 29f, 88f }) m.Box(x, -175, 0, 38, 190, 8, C, .2f);
                m.Box(0, -90, 0, 210, 26, 8, C, .2f);
                m.Box(0, -262, 0, 210, 18, 8, C, .2f);
            });
            m.Box(0, 70, 0, 30, 320, 12, C, .4f);
            m.Box(0, 260, 0, 52, 170, 30, D, .5f);
        }));
        // やかん: 丸い胴、注ぎ口、上のつる(取っ手)、ふた、湯気
        Icon(d, typeof(S771Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 70, 0, 225, 180, 210, C);
            m.Limb(175, 50, 0, 315, -70, 0, 36, 12, C);
            m.Tube([.. Sculpt.ArcPts(0, -65, 165, 3.4f, 6f, 14).Select(q => new Vector3(q, 0))], 10, D);
            m.Super(0, -106, 0, 110, 12, 110, .5f, 1f, m.C(0xAEB7C0));
            m.Ball(0, -126, 0, 16, D);
            Puffs(m, 4, 340, -90, 20, 150, 8, 22, t, .6f, .5f);
        });
        // ボウル: 半球のステンレスのボウルと、ふちと中、映りこみ
        Icon(d, typeof(S772Parameter), (m, t, C, D) =>
        {
            var outer = Dome(300, 300, -70, 14).Select(q => new Vector2(q.X, -70 - (q.Y + 70))).Reverse().ToList();
            m.Lathe(0, 0, C, outer);
            m.Lathe(0, 0, m.C(0xA8B1BA), [.. Dome(285, 285, -70, 12).Select(q => new Vector2(q.X, -70 - (q.Y + 70) - 2))]);
            m.Pitch(0, -70, 0, MathF.PI / 2f, () => m.Torus(0, -70, 0, 294, 8, m.C(0xE3E8ED)));
        });
        // 泡立て器: 立体に交わる針金の輪と、太い持ち手
        Icon(d, typeof(S773Parameter), (m, t, C, D) =>
        {
            for (int i = 0; i < 6; i++) m.Yaw(0, -115, 0, i * MathF.PI / 6f, () => m.Tube([.. Enumerable.Range(0, 25).Select(k => { float a = MathF.PI * k / 24f; return new Vector3(MathF.Sin(a) * 110, -115 - MathF.Cos(a) * 175 + (k > 12 ? 0 : 0), 0); }).Concat(Enumerable.Range(0, 25).Select(k => { float a = MathF.PI + MathF.PI * k / 24f; return new Vector3(MathF.Sin(a) * 110, -115 - MathF.Cos(a) * 175, 0); }))], 2.5f, C));
            m.Cyl(0, 175, 0, 28, 240, D, .5f);
        });
        // お玉: 長い柄と、半球のすくう所
        Icon(d, typeof(S774Parameter), (m, t, C, D) =>
        {
            m.Limb(120, -300, 0, 20, 70, 0, 9, 9, C);
            m.Lathe(0, 0, C, [.. Dome(145, 145, 90, 12).Select(q => new Vector2(q.X, 90 + (90 - q.Y))).Reverse(), new Vector2(0, 90)]);
            m.Super(0, 90, 0, 140, 3, 140, .3f, 1f, m.C(0xA8B1BA));
            m.Box(135, -310, 0, 44, 70, 30, D, .5f);
        });
        // トースター: 角の丸い本体と2つの差し込み口、飛び出すパン、レバー
        Icon(d, typeof(S775Parameter), (m, t, C, D) =>
        {
            float y0 = -90 - MathF.Max(0f, MathF.Sin(t * 2f)) * 130;
            m.Box(-80, y0, 0, 160, 190, 36, D, .25f); m.Box(80, y0, 0, 160, 190, 36, D, .25f);
            m.Box(0, 70, 0, 560, 330, 260, C, .35f);
            m.Box(-80, -95, 0, 180, 4, 50, m.C(0x20242B), .2f); m.Box(80, -95, 0, 180, 4, 50, m.C(0x20242B), .2f);
            m.Box(305, 60, 0, 28, 110, 30, m.C(0x20242B), .4f);
            m.Disc(-190, 160, 130, 20, 20, 10, m.C(0x20242B), .4f);
        });
        // ミキサー: 透明な容器、ふた、黒い台、回る刃
        Icon(d, typeof(S776Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 8;
            m.Lathe(0, 0, new Vector4(C.X, C.Y, C.Z, C.W * .55f), 130 * 1.08f, -240, 90 * 1.08f, 120, 0, 120);
            m.Seg = keep;
            m.Cyl(0, -262, 0, 150, 34, D, .3f);
            m.Box(0, 200, 0, 270, 175, 220, D, .35f);
            m.Disc(0, 200, 112, 38, 38, 12, m.C(0xB7BEC6), .4f);
            m.Yaw(0, 90, 0, t * 16f, () => { m.Box(0, 90, 0, 180, 6, 20, m.C(0xFFFFFF), .3f); m.Box(0, 85, 0, 20, 6, 160, m.C(0xFFFFFF), .3f); });
            m.Limb(0, 120, 0, 0, 50, 0, 5, 5, m.C(0xFFFFFF));
        });
        // お皿: ふちのある丸い皿と青い線
        Icon(d, typeof(S777Parameter), (m, t, C, D) =>
        {
            // 絵と同じく、皿の表を正面に向ける
            m.Pitch(0, 0, 0, -MathF.PI / 2f, () =>
            {
                m.Lathe(0, 0, C, 0, 26, 175, 26, 260, -6, 292, -18, 290, -4, 175, 36, 0, 36);
                m.Pitch(0, -10, 0, MathF.PI / 2f, () => m.Torus(0, -10, 0, 270, 4, D));
                m.Pitch(0, 24, 0, MathF.PI / 2f, () => m.Torus(0, 24, 0, 175, 2.5f, m.C(0xD8DEE5)));
            });
        });
        // お弁当: 箱、ごはんと梅干し、卵焼き、ブロッコリーとからあげ
        Icon(d, typeof(S778Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, -30, 620, 390, 80, C, .1f);
            m.Box(-145, 0, 5, 270, 330, 30, D, .2f);
            m.Ball(-145, 0, 24, 36, 36, 20, m.C(0xE5392F));
            m.Box(155, -90, 14, 270, 145, 46, m.C(0xFFD54A), .3f);
            m.Box(155, 90, 4, 270, 145, 26, m.C(0x6BBF59), .3f);
            m.Ball(115, 88, 34, 42, m.C(0xB9542D)); m.Ball(200, 92, 32, 38, m.C(0xFF7A7A));
        });
        // 歯ブラシ: 長い柄と頭、並んだ毛
        Icon(d, typeof(S779Parameter), (m, t, C, D) => m.Turn(0, 0, -.5f, () =>
        {
            m.Box(40, 0, 0, 560, 46, 30, C, .5f);
            m.Box(-235, -10, 0, 150, 50, 34, C, .45f);
            for (int i = 0; i < 9; i++) foreach (int z in new[] { -10, 10 }) m.Box(-295 + i * 15, -58, z, 10, 62, 10, D, .3f);
        }));
        // 石けん: 角の丸い石けんと、昇るしゃぼん玉
        Icon(d, typeof(S780Parameter), (m, t, C, D) =>
        {
            m.Box(0, 70, 0, 430, 230, 200, C, .45f);
            m.Box(-30, 45, 96, 270, 94, 12, m.C(0xFFFFFF, .45f), .5f);
            int keep = m.Seg; m.Seg = 16;
            for (int i = 0; i < 8; i++) { float k = Fr(t * .35f + i / 8f); m.Ball(-170 + i * 48, -60 - k * 210, (R01(i) - .5f) * 160, 12 + R01(i) * 24, m.C(0xFFFFFF, .45f * (1f - k))); }
            m.Seg = keep;
        });
        // シャンプー: 角の丸いボトル、首、押すポンプ、ラベルと文字
        Icon(d, typeof(S781Parameter), (m, t, C, D) =>
        {
            m.Box(0, 80, 0, 300, 430, 190, C, .45f);
            m.Cyl(0, -175, 0, 37, 86, D, .2f);
            m.Box(55, -228, 0, 180, 30, 50, D, .4f);
            m.Box(135, -208, 0, 28, 50, 28, D, .4f);
            m.Box(0, 90, 96, 190, 180, 4, D, .2f);
            m.Text("洗う", 48, 0, 90, 100, 3, C);
        });
        // タオル: たたんで重ねたタオル(厚み)と、白い線
        Icon(d, typeof(S782Parameter), (m, t, C, D) =>
        {
            for (int i = 2; i >= 0; i--)
            {
                m.Box(0, -70 + i * 78, -i * 6, 480, 120, 200, i % 2 == 0 ? C : D, .5f);
                m.Box(0, -35 + i * 78, 96 - i * 6, 430, 8, 4, m.C(0xFFFFFF, .55f), .3f);
            }
        });
        // トイレットペーパー: 巻いた紙の円柱(芯の穴)と、たれた紙とミシン目
        Icon(d, typeof(S783Parameter), (m, t, C, D) =>
        {
            m.Yaw(0, 0, 0, MathF.PI / 2f, () =>
            {
                m.Disc(0, 0, -30, 178, 178, 330, C, .15f);
            });
            m.Yaw(145, 0, 0, MathF.PI / 2f, () => m.Disc(145, 0, 0, 72, 72, 4, m.C(0x9A7A58), .1f));
            m.Box(-107, 237, 120, 175, 125, 4, C, .1f);
            for (int i = 0; i < 4; i++) m.Ball(-160 + i * 42, 245, 123, 4, D);
        });
        // ティッシュ箱: 箱、取り出し口、ふわっと出たティッシュ
        Icon(d, typeof(S784Parameter), (m, t, C, D) =>
        {
            m.Box(0, 70, 0, 560, 250, 300, C, .08f);
            m.Super(0, -55, 0, 160, 3, 60, .5f, 1f, m.C(0x3A4A32));
            m.Pitch(0, -40, 0, .5f, () => m.Slab(0, 4, D, -100, -40, 100, -40, 120, -215, 40, -180, -30, -250, -115, -190));
            m.Pitch(0, -40, 0, -.4f, () => m.Slab(0, 4, D, -80, -40, 80, -40, 70, -190, -40, -210));
            for (int i = 0; i < 5; i++) m.Disc(-200 + i * 100, 145, 151, 22, 22, 4, m.C(0xFFFFFF, .45f), .3f);
        });
        // ハンガー: フック(管)と三角の針金
        Icon(d, typeof(S785Parameter), (m, t, C, D) =>
        {
            m.Tube([.. Sculpt.ArcPts(0, -220, 50, 3.1f, 7.3f, 14).Select(q => new Vector3(q, 0))], 6, C);
            m.Tube([V3(0, -170), V3(0, -120), V3(-300, 120), V3(300, 120), V3(0, -120)], 6, C);
        });
        // はさみ: 開いたり閉じたりする2枚の刃と、輪の持ち手、中心のねじ
        Icon(d, typeof(S786Parameter), (m, t, C, D) =>
        {
            float o = .18f + MathF.Abs(MathF.Sin(t * 3f)) * .2f;
            foreach (int s in Sides) m.Turn(0, 40, s * o, () =>
            {
                m.Slab(s * 6, 8, D, s * -9, 50, s * 16, 50, s * 5, -320);
                m.Torus(s * 125, 195, s * 6, 56, 13, C);
                m.Limb(s * 2, 60, s * 6, s * 88, 140, s * 6, 10, 10, C);
            });
            m.Disc(0, 40, 0, 14, 14, 30, m.C(0x20242B), .3f);
        });
        // スティックのり: 円柱の本体とキャップ、帯と文字
        Icon(d, typeof(S787Parameter), (m, t, C, D) =>
        {
            m.Cyl(0, 110, 0, 95, 330, C, .15f);
            m.Cyl(0, -120, 0, 96, 160, D, .25f);
            m.Cyl(0, -15, 0, 102, 18, m.C(0x2F6FBB), .2f);
            m.Text("のり", 52, 0, 120, 97, 3, m.C(0x20242B));
        });
        // セロハンテープ: 透明なテープの巻き(厚みのある輪)と紙の芯
        Icon(d, typeof(S788Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 32;
            var ring = new List<float>();
            for (int i = 0; i <= 40; i++) { float a = i * MathF.Tau / 40f; ring.Add(MathF.Cos(a) * 240); ring.Add(MathF.Sin(a) * 240); }
            for (int i = 40; i >= 0; i--) { float a = i * MathF.Tau / 40f; ring.Add(MathF.Cos(a) * 112); ring.Add(MathF.Sin(a) * 112); }
            m.Slab(0, 80, new Vector4(C.X, C.Y, C.Z, C.W * .75f), [.. ring]);
            var core = new List<float>();
            for (int i = 0; i <= 40; i++) { float a = i * MathF.Tau / 40f; core.Add(MathF.Cos(a) * 112); core.Add(MathF.Sin(a) * 112); }
            for (int i = 40; i >= 0; i--) { float a = i * MathF.Tau / 40f; core.Add(MathF.Cos(a) * 86); core.Add(MathF.Sin(a) * 86); }
            m.Slab(0, 86, D, [.. core]);
            m.Seg = keep;
        });
        // カッターナイフ: 折る線のある刃、黄色い本体、スライダー
        Icon(d, typeof(S789Parameter), (m, t, C, D) => m.Turn(0, 0, -.6f, () =>
        {
            m.Slab(0, 4, D, -330, -22, -150, -34, -150, 34, -300, 34);
            for (int i = 0; i < 3; i++) m.Limb(-280 + i * 48, -28, 3, -300 + i * 48, 34, 3, 1.2f, 1.2f, m.C(0x8A95A0));
            m.Box(40, 0, 0, 400, 84, 50, C, .3f);
            m.Box(30, -44, 0, 80, 22, 30, m.C(0x20242B), .4f);
        }));
        // ホッチキス: 台、上の本体(押すと下がる)、針の出る先
        Icon(d, typeof(S790Parameter), (m, t, C, D) =>
        {
            float q = MathF.Max(0f, MathF.Sin(t * 2.6f)) * .14f;
            m.Box(0, 150, 0, 560, 46, 140, D, .3f);
            m.Box(150, 110, 0, 240, 68, 120, D, .3f);
            m.Turn(240, 90, q, () => { m.Box(-20, 20, 0, 520, 100, 130, C, .45f); m.Box(-235, 70, 0, 60, 40, 100, D, .3f); });
        });
    }
}
