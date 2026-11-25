using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: アイコン 881〜910(虫・花と植物・動物の顔・記号 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    /// <summary>花びら(平たい楕円体)を (cx,cy) のまわりに n 枚。tilt で手前へ反らす</summary>
    static void Petals(Sculpt m, float cx, float cy, float cz, int n, float dist, float rx, float ry, float tilt, Vector4 col, float phase = 0f)
    {
        for (int i = 0; i < n; i++)
        {
            float a = phase + i * MathF.Tau / n;
            m.Turn(cx, cy, a, () => m.Pitch(cx, cy - dist * .3f, cz, tilt, () => m.Ball(cx, cy - dist, cz, rx, ry, MathF.Min(rx, ry) * .3f, col)));
        }
    }

    static partial void RegisterI2c(Dictionary<Type, QuickModel> d)
    {
        var K = new Vector4(.125f, .14f, .17f, 1f);
        // セミ: 体と頭、透明な羽、横に飛び出た目、腹のしま
        Icon(d, typeof(S881Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Pitch(0, -100, 0, -s * .25f, () => m.Slab(s * 40, 6, m.C(D, .72f), s * 20, -110, s * 180, -40, s * 125, 250, s * 18, 160));
            m.Ball(0, 0, 0, 95, 160, 70, C);
            m.Ball(0, -125, 10, 100, 45, 60, C);
            foreach (int s in Sides) m.Ball(s * 85, -135, 20, 20, K);
            for (int i = 0; i < 4; i++) m.Tube(Sculpt.OnFront(0, 0, 0, 95, 160, 70, 1f, [new(-70, 30 + i * 30), new(0, 30 + i * 30), new(70, 30 + i * 30)]), 2.5f, K);
        });
        // アリ: 頭・胸・おしりの3つの丸、6本の脚、触角
        Icon(d, typeof(S882Parameter), (m, t, C, D) =>
        {
            m.Ball(-165, -10, 0, 76, C); m.Ball(-20, 5, 0, 62, 50, 50, C); m.Ball(145, 20, 0, 118, 88, 88, C);
            foreach (int x in new[] { -55, 0, 45 }) foreach (int s in Sides) m.Tube([V3(x, 5, 0), V3(x + 18, 60, s * 80), V3(x + 55, 150, s * 120)], 4.5f, C);
            foreach (int s in Sides) m.Tube([V3(-205, -55, s * 20), V3(-250, -120, s * 60), V3(-295, -100, s * 90)], 3.5f, C);
            foreach (int s in Sides) m.Ball(-190, -30, s * 66, 11, m.C(0xFFFFFF));
        });
        // クモ: 立体の巣(放射の糸と輪)、体と頭、8本の脚、目
        Icon(d, typeof(S883Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 6;
            for (int i = 0; i < 8; i++) { float a = i * .785f; m.Limb(0, 0, -60, MathF.Cos(a) * 320, MathF.Sin(a) * 300, -60, 1, 1, D); }
            for (int r = 1; r <= 4; r++) m.Tube([.. Enumerable.Range(0, 9).Select(i => { float a = i * .785f; return new Vector3(MathF.Cos(a) * r * 75, MathF.Sin(a) * r * 70, -60); })], 1, D);
            m.Seg = keep;
            foreach (int s in Sides) for (int i = 0; i < 4; i++) m.Tube([V3(s * 38, -20 + i * 18, 10), V3(s * 125, -85 + i * 62, 60), V3(s * 170, -20 + i * 62, -40)], 4, C);
            m.Ball(0, 50, 0, 68, C); m.Ball(0, -35, 10, 42, C);
            HeadEyes(m, 0, -35, 10, 42, 42, 42, 17, -42, 8);
        });
        // カタツムリ: のびた体と目の付いた角、うず巻きの殻
        Icon(d, typeof(S884Parameter), (m, t, C, D) =>
        {
            m.Super(-10, 185, 0, 295, 31, 70, .6f, .7f, D);
            m.Limb(240, 185, 0, 270, 60, 0, 40, 30, D);
            foreach (int s in Sides) { m.Limb(265, 50, s * 15, 250 + s * 25, -20, s * 25, 4, 4, D); m.Ball(250 + s * 25, -25, s * 25, 12, K); }
            m.Ball(-40, 20, 0, 175, 175, 110, C);
            m.Tube([.. Enumerable.Range(0, 40).Select(i => { float a = i * .3f, r = 160 - i * 3.8f; return new Vector3(-40 + MathF.Cos(a) * r, 20 + MathF.Sin(a) * r, Sculpt.FrontZ(-40, 20, 0, 175, 175, 110, -40 + MathF.Cos(a) * r, 20 + MathF.Sin(a) * r) + 2); })], 6, m.C(0xB9702E));
        });
        // チューリップ: 茎と2枚の葉、花びらが重なるつぼみ形の花
        Icon(d, typeof(S885Parameter), (m, t, C, D) =>
        {
            m.Limb(0, 40, 0, 0, 300, 0, 8, 8, D);
            foreach (int s in Sides) m.Turn(0, 295, s * .5f, () => m.Ball(0, 160, s * 20, 40, 150, 14, D));
            for (int i = 0; i < 6; i++) { float a = i * MathF.Tau / 6f; m.Ball(MathF.Cos(a) * 60, -60, MathF.Sin(a) * 60, 75, 135, 50, i % 2 == 0 ? C : m.C(0xF06A5F)); }
        });
        // ひまわり: 太い茎、まわりの黄色い花びら、種のつまった中心
        Icon(d, typeof(S886Parameter), (m, t, C, D) =>
        {
            m.Limb(0, 80, -20, 0, 320, -20, 9, 9, m.C(0x4FAE4F));
            Petals(m, 0, -40, -6, 18, 160, 38, 92, .2f, C);
            m.Disc(0, -40, 4, 118, 118, 40, D, .5f);
            for (int r = 0; r < 4; r++) for (int i = 0; i < 12; i++) { float a = i * .5236f + r * .3f; m.Ball(MathF.Cos(a) * (28 + r * 25), -40 + MathF.Sin(a) * (28 + r * 25), 24, 4, m.C(0xC68A3C)); }
        });
        // バラ: 巻いた花びらの丸い花、茎と葉
        Icon(d, typeof(S887Parameter), (m, t, C, D) =>
        {
            m.Limb(0, 80, 0, 0, 320, 0, 6, 6, D);
            m.Ball(-105, 195, 0, 88, 34, 20, D); m.Ball(105, 235, 0, 88, 34, 20, D);
            m.Ball(0, -55, 0, 175, 175, 140, C);
            for (int i = 0; i < 4; i++) m.Tube(Sculpt.OnFront(0, -55, 0, 175, 175, 140, 1f, Sculpt.ArcPts(0, -55, 145 - i * 34, i * 1.6f, i * 1.6f + 4.3f, 14)), 8, m.C(0x9B1228));
            m.Ball(0, -55, 140, 18, m.C(0x9B1228));
        });
        // たんぽぽ: 何重にも重なる細い花びら、茎とぎざぎざの葉
        Icon(d, typeof(S888Parameter), (m, t, C, D) =>
        {
            m.Limb(0, 70, 0, 0, 320, 0, 5, 5, D);
            foreach (int s in Sides) m.Slab(-10, 10, D, 0, 300, s * 215, 185, s * 140, 235, s * 170, 285);
            for (int r = 3; r >= 1; r--) Petals(m, 0, -80, (3 - r) * 14, 18, r * 48, 14, 38, .45f - r * .1f, r % 2 == 0 ? m.C(0xF7BF20) : C, r * .1f);
            m.Ball(0, -80, 40, 30, m.C(0xF2A81E));
        });
        // あさがお: らっぱ形の花(筋)、白い奥、つるとつぼみの葉
        Icon(d, typeof(S889Parameter), (m, t, C, D) =>
        {
            m.Tube([V3(0, 120, -40), V3(50, 210, -40), V3(-40, 320, -40)], 4.5f, m.C(0x4FAE4F));
            m.Ball(115, 205, -30, 92, 50, 20, m.C(0x4FAE4F));
            m.Pitch(0, -55, 0, -MathF.PI / 2f, () =>
            {
                m.Lathe(0, 0, C, 20, -55 + 150, 72, -55 + 60, 205, -55);
                m.Lathe(0, 0, m.C(0x7088F0), 198, -55 + 1, 68, -55 + 60, 16, -55 + 148);
                m.Lathe(0, 0, D, 76, -55 + 54, 0, -55 + 54);
                m.Ball(0, -55 + 50, 0, 16, m.C(0xFFE27A));
                for (int i = 0; i < 5; i++) { float a = i * 1.2566f, c = MathF.Cos(a), s = MathF.Sin(a); m.Limb(c * 66, -55 + 54, s * 66, c * 188, -59, s * 188, 5, 5, m.C(0x8EA2F5)); }
            });
        });
        // さぼてん: 鉢、太い柱と両側の腕、白いとげ
        Icon(d, typeof(S890Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, D, 0, 130, 125, 130, 95, 300, 0, 300);
            m.Super(0, -25, 0, 75, 175, 75, .8f, 1f, C);
            m.Tube([V3(-75, 10), V3(-170, 10), V3(-170, -85)], 26, C); m.Ball(-170, -85, 0, 26, C);
            m.Tube([V3(75, 50), V3(175, 50), V3(175, -65)], 26, C); m.Ball(175, -65, 0, 26, C);
            for (int i = 0; i < 16; i++) { float a = (i * 2.4f) % MathF.Tau, y = -160 + i * 20, x = MathF.Cos(a) * 72, z = MathF.Sin(a) * 72; m.Limb(x, y, z, x * 1.2f, y, z * 1.2f, 1.5f, .5f, m.C(0xFFFFFF)); }
        });
        // 竹: 3本の節のある太い茎と、細長い葉
        Icon(d, typeof(S891Parameter), (m, t, C, D) =>
        {
            for (int k = 0; k < 3; k++)
            {
                float x = -170 + k * 170, z = (k - 1) * 60;
                for (int i = 0; i < 4; i++) { m.Cyl(x, -225 + i * 150, z, 35, 140, C, .15f); m.Cyl(x, -158 + i * 150, z, 42, 14, D, .4f); }
                m.Ball(x + 60, -130 + k * 42, z + 20, 70, 18, 8, D); m.Ball(x - 55, 20 - k * 25, z + 20, 64, 16, 8, D);
            }
        });
        // 松: 太い幹と、段になった円すいの葉
        Icon(d, typeof(S892Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, D, 42, 120, 70, 300, 0, 300);
            for (int i = 0; i < 4; i++) { float y = -230 + i * 92, w = 120 + i * 62; m.Lathe(0, 0, i % 2 == 0 ? C : m.C(0x3A9558), 0, y - 40, w * .45f, y + 20, w, y + 110, 0, y + 110); }
        });
        // もみじ: 7つにとがった葉(反った板)と葉脈、軸
        Icon(d, typeof(S893Parameter), (m, t, C, D) =>
        {
            m.Limb(0, 40, 0, 20, 310, 0, 4, 3, D);
            for (int k = 0; k < 7; k++) { float a = -1.57f + k * .8976f; m.Turn(0, -30, a + MathF.PI / 2f, () => m.Pitch(0, -30, 0, .15f, () => m.Slab(0, 8, C, 0, -30, 50, -120, 0, -300, -50, -120))); m.Limb(0, -30, 6, MathF.Cos(a) * 220, -30 + MathF.Sin(a) * 220, 6, 2, 1, D); }
        });
        // クローバー: ハート形の4枚の葉、葉脈、茎
        Icon(d, typeof(S894Parameter), (m, t, C, D) =>
        {
            for (int k = 0; k < 4; k++)
            {
                float a = k * 1.5708f + .7854f, x = MathF.Cos(a) * 118, y = -20 + MathF.Sin(a) * 118;
                foreach (int s in Sides) m.Ball(x * 1.2f + s * MathF.Cos(a + 1.57f) * 44, -20 + (y + 20) * 1.2f + s * MathF.Sin(a + 1.57f) * 44, 0, 64, 64, 20, C);
                m.Ball(x * .7f, -20 + (y + 20) * .7f, 0, 52, 52, 18, C);
                m.Limb(0, -20, 20, x * 1.45f, -20 + (y + 20) * 1.45f, 20, 2.5f, 1, D);
            }
            m.Tube([V3(0, -20), V3(15, 160, -10), V3(70, 300, -20)], 7, D);
        });
        // きのこの森: 大小3本のきのこ(かさ・かさの裏・軸・白い点)
        Icon(d, typeof(S895Parameter), (m, t, C, D) =>
        {
            foreach (var (x, y, k, z) in new[] { (-180f, 110f, .75f, -80f), (180f, 130f, .65f, -60f), (0f, 40f, 1f, 60f) })
            {
                m.Cyl(x, y + 95 * k, z, 44 * k, 210 * k, D, .4f);
                m.Lathe(x, z, C, [.. Dome(170 * k, 150 * k, y + 20 * k, 12), new Vector2(0, y + 20 * k)]);
                m.Super(x, y + 22 * k, z, 165 * k, 6 * k, 165 * k, .4f, 1f, m.C(0xF1D9C6));
                for (int i = 0; i < 6; i++) { float a = i * 1.05f, el = .7f; m.Ball(x + MathF.Cos(a) * MathF.Sin(el) * 170 * k, y + 20 * k - MathF.Cos(el) * 150 * k, z + MathF.Sin(a) * MathF.Sin(el) * 170 * k, 22 * k, D); }
            }
        });
        // 葉っぱ: 反った葉と、中心の葉脈・横の葉脈
        Icon(d, typeof(S896Parameter), (m, t, C, D) => m.Turn(0, 0, .45f, () =>
        {
            m.Pitch(0, 0, 0, .2f, () =>
            {
                m.Slab(0, 14, C, 0, -310, 120, -205, 185, -40, 145, 130, 50, 245, 0, 280, -50, 245, -145, 130, -185, -40, -120, -205);
                m.Limb(0, -260, 9, 0, 340, 9, 5, 3, D);
                for (int i = 0; i < 5; i++) { float y = -160 + i * 75, w = 125 - MathF.Abs(i - 2) * 20; m.Limb(0, y + 55, 8, w, y, 8, 2.5f, 1, D); m.Limb(0, y + 55, 8, -w, y, 8, 2.5f, 1, D); }
            });
        }));
        // コアラの顔: ふわふわの大きな耳、丸い顔、大きな黒い鼻
        Icon(d, typeof(S897Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) { m.Ball(s * 205, -105, -30, 105, 105, 50, C); m.Ball(s * 205, -105, 10, 60, 60, 15, D); }
            m.Ball(0, 30, 0, 215, 195, 185, C);
            HeadEyes(m, 0, 30, 0, 215, 195, 185, 100, -5, 22);
            m.Ball(0, 65, 170, 50, 78, 40, K);
            m.Ball(-14, 35, 205, 10, 18, 4, m.C(0xFFFFFF, .35f));
        });
        // ハムスターの顔: 小さな丸い耳、ふくらんだほお袋、鼻と前歯
        Icon(d, typeof(S898Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) { m.Ball(s * 150, -165, -20, 58, 58, 30, C); m.Ball(s * 150, -165, 4, 30, 30, 8, m.C(0xFFB6C1)); }
            m.Ball(0, 35, 0, 240, 195, 190, C);
            foreach (int s in Sides) m.Ball(s * 135, 90, 110, 88, 75, 70, D);
            m.Ball(0, 95, 140, 75, 70, 60, D);
            HeadEyes(m, 0, 35, 0, 240, 195, 190, 95, -20, 22);
            m.Ball(0, 62, 196, 14, 10, 8, m.C(0xE5787A));
            m.Box(0, 132, 192, 28, 30, 10, m.C(0xFFFFFF), .3f);
        });
        // しかの顔: 枝分かれした角、横の耳、細い顔と明るい口元、背中の白い点
        Icon(d, typeof(S899Parameter), (m, t, C, D) =>
        {
            var horn = m.C(0x7A5235);
            foreach (int s in Sides)
            {
                m.Tube([V3(s * 75, -150, -20), V3(s * 125, -260, -30), V3(s * 205, -315, -30)], 6, horn);
                m.Limb(s * 125, -260, -30, s * 65, -310, -30, 5, 4, horn); m.Limb(s * 165, -290, -30, s * 215, -235, -30, 5, 4, horn);
                m.Yaw(s * 205, -65, 0, -s * .4f, () => m.Ball(s * 205, -65, 0, 82, 36, 20, C));
            }
            m.Ball(0, -10, 0, 155, 160, 130, C);
            m.Limb(0, 60, 40, 0, 190, 90, 110, 70, C);
            m.Ball(0, 185, 120, 75, 70, 50, D);
            HeadEyes(m, 0, -10, 0, 155, 160, 130, 78, -25, 22);
            m.Ball(0, 150, 160, 25, 18, 14, K);
        });
        // うまの顔: 長い顔、とがった耳、明るい鼻先と鼻の穴、たてがみ
        Icon(d, typeof(S900Parameter), (m, t, C, D) =>
        {
            m.Limb(0, -80, 0, 0, 200, 40, 125, 85, C);
            foreach (int s in Sides) m.Slab(-30, 40, C, s * 125, -165, s * 120, -285, s * 45, -180);
            m.Ball(0, 245, 70, 88, 62, 70, m.C(0xD9B28C));
            foreach (int s in Sides) m.Ball(s * 30, 245, 135, 10, 14, 6, K);
            HeadEyes(m, 0, -80, 0, 125, 125, 125, 92, -55, 20);
            for (int i = -2; i <= 2; i++) m.Ball(i * 28, -190 + MathF.Abs(i) * 8, -40, 20, 66, 30, D);
        });
        // 記号: 角の丸い厚い板・輪・管で作る、ぽってりした記号
        Icon(d, typeof(S901Parameter), (m, t, C, D) => { m.Box(0, 0, 0, 500, 130, 130, C, .45f); m.Box(0, 0, 0, 130, 500, 130, C, .45f); });
        Icon(d, typeof(S902Parameter), (m, t, C, D) => m.Box(0, 0, 0, 500, 130, 130, C, .45f));
        Icon(d, typeof(S903Parameter), (m, t, C, D) => m.Turn(0, 0, .7854f, () => { m.Box(0, 0, 0, 500, 120, 120, C, .45f); m.Box(0, 0, 0, 120, 500, 120, C, .45f); }));
        Icon(d, typeof(S904Parameter), (m, t, C, D) => { m.Box(0, 0, 0, 500, 90, 90, C, .45f); m.Ball(0, -150, 0, 62, C); m.Ball(0, 150, 0, 62, C); });
        Icon(d, typeof(S905Parameter), (m, t, C, D) => { m.Box(0, -90, 0, 500, 100, 100, C, .45f); m.Box(0, 90, 0, 500, 100, 100, C, .45f); });
        Icon(d, typeof(S906Parameter), (m, t, C, D) => { m.Torus(-150, -140, 0, 82, 42, C); m.Torus(150, 140, 0, 82, 42, C); m.Limb(175, -250, 0, -175, 250, 0, 44, 44, C); });
        Icon(d, typeof(S907Parameter), (m, t, C, D) => m.Torus(0, 0, 0, 205, 62, C));
        Icon(d, typeof(S908Parameter), (m, t, C, D) => m.Turn(0, 0, .7854f, () => { m.Box(0, 0, 0, 560, 115, 115, C, .45f); m.Box(0, 0, 0, 115, 560, 115, C, .45f); }));
        Icon(d, typeof(S909Parameter), (m, t, C, D) => { m.Tube([V3(0, -215), V3(240, 185), V3(-240, 185), V3(0, -215)], 55, C); foreach (var (x, y) in new[] { (0f, -215f), (240f, 185f), (-240f, 185f) }) m.Ball(x, y, 0, 55, C); });
        Icon(d, typeof(S910Parameter), (m, t, C, D) => { m.Tube([V3(-205, -205), V3(205, -205), V3(205, 205), V3(-205, 205), V3(-205, -205)], 55, C); foreach (var (x, y) in new[] { (-205f, -205f), (205f, -205f), (205f, 205f), (-205f, 205f) }) m.Ball(x, y, 0, 55, C); });
    }
}
