using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 213_QIcons1.cs のアイコン 701〜730(果物・野菜 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方(ふわふわ・ゆれる・回る)も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    internal delegate void IconModel(Sculpt m, float t, Vector4 C, Vector4 D);

    static void Icon(Dictionary<Type, QuickModel> d, Type type, IconModel draw) =>
        d[type] = (p, m, t, v, v2) => IconMotion((Q5Icon)p, m, t, () => draw(m, t, m.C(p.Color1), m.C(p.Color2)));

    /// <summary>アイコンの動き(2Dの Q5Icon と同じ)</summary>
    static void IconMotion(Q5Icon p, Sculpt m, float t, Action draw)
    {
        switch (p.IconMode)
        {
            case 1: m.Move(0, MathF.Sin(t * 2f) * 14f, draw); break;
            case 2: m.Turn(0, 260, MathF.Sin(t * 2f) * .09f, draw); break;
            case 3: { float s = 1f + MathF.Sin(t * 3.4f) * .05f; m.Scale(0, 0, 0, s, draw); break; }
            case 4: m.Turn(0, 0, t * .9f, draw); break;
            case 5: m.Move(MathF.Sin(t * 37f) * 3f, MathF.Cos(t * 31f) * 2f, draw); break;
            default: draw(); break;
        }
    }

    /// <summary>回転体の断面(r, y の並び)から、高さ y での半径を求める</summary>
    static float RadiusAt(float[] prof, float y)
    {
        for (int i = 0; i + 3 < prof.Length; i += 2)
        {
            float y0 = prof[i + 1], y1 = prof[i + 3];
            if ((y >= y0 && y <= y1) || (y <= y0 && y >= y1)) { float k = MathF.Abs(y1 - y0) < 1e-3f ? 0f : (y - y0) / (y1 - y0); return prof[i] + (prof[i + 2] - prof[i]) * k; }
        }
        return 0f;
    }

    /// <summary>回転体の表面に小さな粒を散らす(種・ぶつぶつ)。前側だけ(a0〜a1)</summary>
    static void Speckle(Sculpt m, float[] prof, int n, int seed, float y0, float y1, float rx, float ry, float rz, Vector4 col, float a0 = .15f, float a1 = MathF.PI - .15f)
    {
        int keep = m.Seg; m.Seg = 8;
        for (int i = 0; i < n; i++)
        {
            float y = y0 + R01(seed + i * 3) * (y1 - y0), a = a0 + R01(seed + i * 3 + 1) * (a1 - a0), r = RadiusAt(prof, y);
            if (r < 4f) continue;
            m.Ball(MathF.Cos(a) * r, y, MathF.Sin(a) * r, rx, ry, rz, col);
        }
        m.Seg = keep;
    }

    static void Leaf(Sculpt m, float x, float y, float z, float rx, float ry, float rot, Vector4 col) =>
        m.Turn(x, y, rot, () => m.Pitch(x, y, z, -.4f, () => m.Ball(x, y, z, rx, ry, MathF.Min(rx, ry) * .25f, col)));

    static partial void RegisterI1a(Dictionary<Type, QuickModel> d)
    {
        var stemC = new Vector4(.42f, .27f, .16f, 1f);
        // りんご: くぼんだ上と下のある回転体、軸と葉、つや
        Icon(d, typeof(S701Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 0, -105, 60, -118, 130, -150, 205, -95, 240, 20, 215, 150, 130, 228, 40, 222, 0, 205);
            m.Limb(0, -110, 0, 18, -215, 0, 7, 6, m.C(0x6B4428));
            Leaf(m, 85, -205, 0, 70, 30, -.3f, D);
            m.Ball(-105, -30, 175, 28, 58, 10, m.C(0xFFFFFF, .5f));
        });
        // みかん: 少しつぶれた球、表面のつぶつぶ、上のへた
        Icon(d, typeof(S702Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 20, 0, 235, 205, 225, C);
            for (int i = 0; i < 40; i++) { float a = R01(i) * MathF.PI, b = (R01(i + 50) - .5f) * 2.6f; var dir = new Vector3(MathF.Cos(a) * MathF.Cos(b), MathF.Sin(b) * .9f, MathF.Sin(a) * MathF.Cos(b)); m.Ball(dir.X * 235, 20 + dir.Y * 205, dir.Z * 225, 4, 4, 2, m.C(0xD97700)); }
            m.Pitch(0, -184, 0, MathF.PI / 2f, () => m.Slab(0, 10, D, StarPts(0, -184, 44, 5, .42f, 0f)));
            m.Ball(-90, -70, 190, 34, 50, 10, m.C(0xFFFFFF, .45f));
        });
        // バナナ: 弓なりの太い房(先と軸は茶色)
        Icon(d, typeof(S703Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 14;
            var arc = Sculpt.ArcPts(0, -230, 360, .45f, 2.7f, 18).Select(q => new Vector3(q, 0)).ToList();
            m.Tube(arc, 44, C);
            m.Tube([.. Sculpt.ArcPts(0, -230, 395, .7f, 2.4f, 14).Select(q => new Vector3(q, 20))], 5, m.C(0xE8B923));
            m.Seg = keep;
            m.Limb(-300, -40, 0, -315, 10, 0, 18, 16, D);
            m.Ball(325, -58, 0, 18, D);
        });
        // ぶどう: 立体の房(下へ細くなる)、軸と葉
        Icon(d, typeof(S704Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 18;
            int[] n = [6, 5, 4, 3, 1];
            for (int r = 0; r < 5; r++)
            {
                float y = -120 + r * 76, rr = (n[r] - 1) * 32f;
                for (int i = 0; i < n[r]; i++) { float a = i * MathF.Tau / n[r] + r * .5f; m.Ball(MathF.Cos(a) * rr, y, MathF.Sin(a) * rr, 50, C); }
                if (n[r] > 3) m.Ball(0, y, 0, 50, C);
            }
            m.Seg = keep;
            m.Limb(0, -170, 0, 0, -260, 0, 6, 6, m.C(0x6B4428));
            Leaf(m, 82, -235, 0, 84, 38, -.2f, D);
        });
        // いちご: ふくらんだ円すい、表面の種、上の葉(星形)
        Icon(d, typeof(S705Parameter), (m, t, C, D) =>
        {
            float[] prof = [0, -175, 120, -172, 205, -100, 200, -10, 160, 90, 90, 200, 0, 270];
            m.Lathe(0, 0, C, prof);
            Speckle(m, prof, 46, 11, -140, 220, 5, 9, 3, m.C(0xFFE27A), 0, MathF.Tau);
            m.Pitch(0, -178, 0, MathF.PI / 2f, () => m.Slab(0, 10, D, StarPts(0, -178, 105, 7, .34f, 0f)));
            m.Limb(0, -185, 0, 0, -230, 0, 8, 7, D);
        });
        // さくらんぼ: 2つの実と曲がった軸、葉
        Icon(d, typeof(S706Parameter), (m, t, C, D) =>
        {
            var stem = m.C(0x6B8E23);
            m.Tube([V3(-125, 60, -10), V3(-30, -160, 0), V3(45, -240, 0)], 4, stem);
            m.Tube([V3(125, 85, 10), V3(70, -120, 0), V3(45, -240, 0)], 4, stem);
            m.Ball(-125, 150, -10, 100, C); m.Ball(125, 175, 10, 100, C);
            m.Ball(-160, 115, 78, 20, 20, 8, m.C(0xFFFFFF)); m.Ball(90, 140, 98, 20, 20, 8, m.C(0xFFFFFF));
            Leaf(m, 120, -235, 0, 88, 34, -.1f, D);
        });
        // スイカ: 半月形に切ったひと切れ(皮・白い所・赤い実、両面に種)
        Icon(d, typeof(S707Parameter), (m, t, C, D) =>
        {
            m.Slab(0, 120, D, PiePts(0, -120, 340, 0, MathF.PI, 30));
            m.Slab(0, 124, m.C(0xF4F4D8), PiePts(0, -120, 315, 0, MathF.PI, 30));
            m.Slab(0, 128, C, PiePts(0, -120, 295, 0, MathF.PI, 30));
            foreach (int s in Sides) for (int i = 0; i < 12; i++) { float a = .2f + i * .25f, r = 120 + i % 3 * 60; m.Ball(MathF.Cos(a) * r, -120 + MathF.Sin(a) * r, s * 64, 8, 14, 4, m.C(0x20242B)); }
        });
        // もも: 丸い実とすじ(くぼみ)、2枚の葉
        Icon(d, typeof(S708Parameter), (m, t, C, D) =>
        {
            m.Ball(-40, 50, 0, 200, 205, 200, C);
            m.Ball(40, 50, 0, 200, 205, 200, Vector4.Lerp(C, m.C(0xFF9A98), .5f));
            m.Tube(Sculpt.OnFront(0, 50, 0, 240, 205, 200, -6f, Sculpt.ArcPts(-20, 40, 165, -1.0f, 1.1f, 14)), 4, m.C(0xE5787A));
            Leaf(m, -90, -140, 0, 70, 28, .3f, D); Leaf(m, 90, -150, 0, 70, 28, -.3f, D);
        });
        // パイナップル: だ円の実とひし形の模様、上に広がる葉の冠
        Icon(d, typeof(S709Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 75, 0, 145, 205, 145, C);
            var mark = m.C(0xB97A18);
            for (int y = -3; y <= 3; y++)
            {
                float cy = 75 + y * 52;
                for (int i = 0; i < 9; i++)
                {
                    float a = .2f + i * .34f, r = 145f * MathF.Sqrt(MathF.Max(0f, 1f - (cy - 75) * (cy - 75) / (205f * 205f)));
                    m.Ball(MathF.Cos(a) * r, cy + (i % 2) * 26 - 13, MathF.Sin(a) * r, 14, 20, 6, mark);
                }
            }
            for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12f, h = 170 + (i % 3) * 40; m.Limb(MathF.Cos(a) * 20, -115, MathF.Sin(a) * 20, MathF.Cos(a) * 70, -115 - h, MathF.Sin(a) * 70, 18, 2, D); }
        });
        // レモン: とがった両はしのだ円体、つや、葉
        Icon(d, typeof(S710Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 235, 165, 165, C);
            foreach (int s in Sides) m.Limb(s * 200, 0, 0, s * 290, 0, 0, 60, 8, C);
            m.Ball(-85, -60, 150, 48, 22, 8, m.C(0xFFFFFF, .55f));
            Leaf(m, 150, -160, 0, 60, 24, -.3f, D);
        });
        // メロン: まん丸の実と、網目の筋(縦と横)、T字の軸
        Icon(d, typeof(S711Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 20, 0, 235, C);
            var net = m.C(0xE5EFCE, .8f);
            int keep = m.Seg; m.Seg = 24;
            for (int i = 0; i < 6; i++) m.Yaw(0, 20, 0, i * MathF.PI / 6f, () => m.Torus(0, 20, 0, 237, 2.5f, net));
            for (int i = -3; i <= 3; i++) { float y = 20 + i * 55, r = MathF.Sqrt(MathF.Max(0f, 237f * 237f - (i * 55f) * (i * 55f))); m.Pitch(0, y, 0, MathF.PI / 2f, () => m.Torus(0, y, 0, r, 2.5f, net)); }
            m.Seg = keep;
            m.Tube([V3(0, -210, 0), V3(-14, -262, 0)], 7, D); m.Limb(-40, -268, 0, 30, -262, 0, 7, 7, D);
        });
        // キウイ: 輪切り(茶色い皮・緑の実・白い芯・黒い種・放射状の筋)
        Icon(d, typeof(S712Parameter), (m, t, C, D) =>
        {
            m.Disc(0, 0, 0, 245, 245, 70, D, .35f);
            m.Disc(0, 0, 4, 222, 222, 70, C, .2f);
            m.Disc(0, 0, 8, 62, 62, 70, m.C(0xF6F7D7), .2f);
            for (int i = 0; i < 18; i++)
            {
                float a = i * .349f;
                m.Limb(MathF.Cos(a) * 72, MathF.Sin(a) * 72, 40, MathF.Cos(a) * 190, MathF.Sin(a) * 190, 40, 2, 2, m.C(0xDCEBA8));
                m.Ball(MathF.Cos(a) * 105, MathF.Sin(a) * 105, 42, 6, 6, 3, m.C(0x20242B));
            }
        });
        // 柿: 少し平たい実、4枚のへたと軸
        Icon(d, typeof(S713Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 30, 0, 235, 190, 225, C);
            m.Ball(-85, -30, 200, 40, 60, 10, m.C(0xFFFFFF, .35f));
            m.Pitch(0, -150, 0, MathF.PI / 2f, () => m.Slab(0, 10, D, StarPts(0, -150, 120, 4, .35f, MathF.PI / 4f)));
            m.Cyl(0, -180, 0, 12, 44, m.C(0x4B3420), .3f);
        });
        // 梨: 下ぶくれの回転体、点々、軸
        Icon(d, typeof(S714Parameter), (m, t, C, D) =>
        {
            float[] prof = [0, -205, 60, -200, 105, -130, 112, -40, 170, 40, 188, 120, 150, 230, 0, 275];
            m.Lathe(0, 0, C, prof);
            Speckle(m, prof, 30, 7, -150, 240, 3, 3, 2, D);
            m.Limb(0, -205, 0, 16, -280, 0, 6, 5, m.C(0x6B4428));
        });
        // ブルーベリー: 3つの実(上に星形のくぼみ)、つや、葉
        Icon(d, typeof(S715Parameter), (m, t, C, D) =>
        {
            foreach (var (x, y, r, z) in new[] { (-120f, 60f, 120f, 20f), (120f, 70f, 115f, -10f), (0f, -90f, 125f, -40f) })
            {
                m.Ball(x, y, z, r, C);
                m.Slab(z + r * .92f, 6, m.C(0x1F2A6B), StarPts(x, y - r * .45f, r * .3f, 5, .45f, 0f));
                m.Ball(x - r * .45f, y - r * .1f, z + r * .85f, r * .13f, r * .13f, r * .05f, m.C(0xFFFFFF, .5f));
            }
            Leaf(m, 170, -170, -40, 84, 36, -.2f, D);
        });
        // にんじん: 細長い円すい、横の筋、上の葉
        Icon(d, typeof(S716Parameter), (m, t, C, D) =>
        {
            float[] prof = [0, -150, 100, -148, 105, -120, 60, 150, 18, 290, 0, 300];
            m.Lathe(0, 0, C, prof);
            for (int i = 0; i < 6; i++) { float y = -90 + i * 62, r = RadiusAt(prof, y) + 1; m.Tube([.. Sculpt.ArcPts(0, 0, r, .5f + i * .3f, 1.3f + i * .3f, 6).Select(q => new Vector3(q.X, y, q.Y))], 2.5f, m.C(0xD9660B)); }
            for (int i = -2; i <= 2; i++) m.Turn(i * 40, -150, i * .15f, () => m.Ball(i * 40, -235, i % 2 * 20, 28, 88, 14, D));
        });
        // トマト: つやのある丸い実、星形のへたと軸
        Icon(d, typeof(S717Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 30, 0, 240, 205, 230, C);
            m.Ball(-95, -40, 200, 42, 66, 10, m.C(0xFFFFFF, .4f));
            m.Pitch(0, -168, 0, MathF.PI / 2f, () => m.Slab(0, 10, D, StarPts(0, -168, 115, 5, .3f, 0f)));
            m.Cyl(0, -195, 0, 10, 48, D, .3f);
        });
        // なす: ふくらんだ実とへた(かさ)、軸。斜めに置く
        Icon(d, typeof(S718Parameter), (m, t, C, D) => m.Turn(0, 0, -.5f, () =>
        {
            m.Limb(-140, 0, 0, 80, 0, 0, 80, 122, C);
            m.Turn(205, 0, -MathF.PI / 2f, () => m.Lathe(205, 0, D, 108, 30, 70, -30, 20, -60, 0, -62));
            m.Limb(240, 0, 0, 330, 0, 0, 18, 14, D);
            m.Ball(20, -58, 80, 100, 16, 10, m.C(0xFFFFFF, .35f));
        }));
        // きゅうり: 長い棒といぼいぼ、先の黄色い花
        Icon(d, typeof(S719Parameter), (m, t, C, D) => m.Turn(0, 0, -.35f, () =>
        {
            m.Limb(-258, 0, 0, 258, 0, 0, 52, 52, C);
            for (int i = 0; i < 24; i++) { float x = -250 + i * 22, a = (i * 2.4f) % MathF.Tau; m.Ball(x, MathF.Cos(a) * 52, MathF.Sin(a) * 52, 4, D); }
            m.Ball(312, 0, 0, 16, 18, 18, m.C(0xFFE27A));
        }));
        // 大根: 白い太い根、細い筋、上の葉
        Icon(d, typeof(S720Parameter), (m, t, C, D) =>
        {
            float[] prof = [0, -120, 90, -118, 92, -90, 80, 100, 50, 220, 0, 320];
            m.Lathe(0, 0, C, prof);
            for (int i = 0; i < 4; i++) { float y = -50 + i * 60, r = RadiusAt(prof, y) + 1; m.Tube([.. Sculpt.ArcPts(0, 0, r, 1.6f, 2.2f, 5).Select(q => new Vector3(q.X, y, q.Y))], 1.5f, m.C(0xD9D9C8)); }
            for (int i = -2; i <= 2; i++) m.Turn(i * 36, -120, i * .12f, () => m.Ball(i * 36, -200, i % 2 * 18, 26, 92, 12, D));
        });
        // 玉ねぎ: とがった上のある丸い回転体、縦の筋、根
        Icon(d, typeof(S721Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 0, -285, 12, -282, 52, -130, 150, -40, 205, 60, 185, 190, 90, 252, 0, 262);
            for (int i = 0; i < 7; i++) m.Yaw(0, 0, 0, i * MathF.PI / 7f + .2f, () => m.Tube([.. Enumerable.Range(0, 12).Select(k => { float y = -260 + k * 45f, r = RadiusAt([0, -285, 12, -282, 52, -130, 150, -40, 205, 60, 185, 190, 90, 252, 0, 262], y) + 1.5f; return new Vector3(0, y, r); })], 2, D));
            for (int i = -4; i <= 4; i++) m.Limb(i * 10, 255, 0, i * 14, 280, (i % 2) * 10, 2, 1.5f, m.C(0xE8D8B0));
        });
        // キャベツ: 何枚も重なる葉(少しずつずらした球)と葉脈
        Icon(d, typeof(S722Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 240, 235, 235, C);
            for (int i = 0; i < 4; i++)
            {
                float r = 218 - i * 42;
                m.Ball(-30 + i * 18, 0, 60 + i * 28, r, r * .98f, r * .75f, Vector4.Lerp(C, m.C(0xBFE69A), i * .2f));
                m.Tube(Sculpt.OnFront(-30 + i * 18, 0, 60 + i * 28, r, r * .98f, r * .75f, 1f, Sculpt.ArcPts(-40 + i * 24, 0, r * .9f, -1.1f + i * .3f, 1.8f + i * .3f, 12)), 3, D);
            }
        });
        // かぼちゃ: 縦のふくらみ(まわりに並ぶだ円体)と太い軸
        Icon(d, typeof(S723Parameter), (m, t, C, D) =>
        {
            for (int i = 0; i < 10; i++) { float a = i * MathF.Tau / 10f; m.Yaw(0, 40, 0, -a, () => m.Ball(90, 40, 0, 120, 185, 90, i % 2 == 0 ? C : m.C(0x336A40))); }
            m.Cyl(0, -170, 0, 26, 70, m.C(0x6B5A2B), .3f);
        });
        // とうもろこし: 粒の並んだ実と、両側の皮
        Icon(d, typeof(S724Parameter), (m, t, C, D) =>
        {
            m.Ball(0, -20, 0, 100, 265, 100, m.C(0xF0B820));
            int keep = m.Seg; m.Seg = 8;
            for (int y = -6; y <= 6; y++)
            {
                float cy = -20 + y * 38, r = 100f * MathF.Sqrt(MathF.Max(0f, 1f - (y * 38f) * (y * 38f) / (265f * 265f)));
                for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12f + (y % 2) * .26f; m.Ball(MathF.Cos(a) * r, cy, MathF.Sin(a) * r, 17, 17, 12, C); }
            }
            m.Seg = keep;
            foreach (int s in Sides) m.Yaw(0, 0, 0, s * .5f, () => m.Slab(s * 40, 8, D, -120 * s, 290, -30 * s, -60, 10 * s, 300));
        });
        // ピーマン: 3つのふくらみ、へこみの筋、軸、つや
        Icon(d, typeof(S725Parameter), (m, t, C, D) =>
        {
            m.Ball(-85, 30, -10, 120, 160, 120, C); m.Ball(85, 30, -10, 120, 160, 120, C); m.Ball(0, 60, 60, 135, 150, 110, C);
            m.Ball(0, 175, 0, 110, 70, 90, C);
            m.Cyl(0, -115, 0, 24, 62, D, .3f);
            m.Ball(-120, -20, 90, 22, 50, 8, m.C(0xFFFFFF, .4f));
        });
        // じゃがいも: でこぼこの塊と芽のくぼみ
        Icon(d, typeof(S726Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 245, 175, 165, C);
            m.Ball(-110, -40, 20, 120, 110, 120, C); m.Ball(120, 40, -10, 130, 100, 130, C);
            foreach (var (x, y, r) in new[] { (-140f, -30f, 10f), (60f, -70f, 8f), (120f, 50f, 10f), (-40f, 80f, 7f) }) m.Ball(x, y, Sculpt.FrontZ(0, 0, 0, 245, 175, 165, x, y) + 1, r, r, 3, D);
        });
        // ブロッコリー: 太い茎と、もこもこの房(立体に重なる)
        Icon(d, typeof(S727Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, D, 58, 40, 90, 290, 0, 290);
            int keep = m.Seg; m.Seg = 18;
            foreach (var (x, y, r, z) in new[] { (-150f, -40f, 95f, -20f), (150f, -40f, 95f, -20f), (0f, -145f, 115f, -30f), (-70f, 20f, 88f, 60f), (80f, 20f, 88f, 60f), (0f, -40f, 100f, -100f), (0f, -60f, 100f, 80f) }) m.Ball(x, y, z, r, C);
            for (int i = 0; i < 30; i++) { float a = R01(i) * MathF.Tau, b = (R01(i + 9) - .5f) * 1.6f; m.Ball(MathF.Cos(a) * 180 * MathF.Cos(b), -60 + MathF.Sin(b) * 120, MathF.Sin(a) * 120 * MathF.Cos(b) + 20, 7, m.C(0x2F702F)); }
            m.Seg = keep;
        });
        // しいたけ: 太い軸、丸いかさ(裏のひだ)、かさの白い十字
        Icon(d, typeof(S728Parameter), (m, t, C, D) =>
        {
            m.Cyl(0, 120, 0, 46, 230, D, .4f);
            m.Lathe(0, 0, C, 0, -170, 120, -150, 220, -60, 252, 30, 240, 46, 0, 46);
            m.Super(0, 44, 0, 235, 8, 235, .5f, 1f, m.C(0xD9BE96));
            foreach (int a in new[] { -120, -40, 60, 130 }) m.Tube(Sculpt.OnFront(0, 40, 0, 252, 210, 252, 2f, [new(a - 26, -90), new(a, -112), new(a + 26, -90)]), 5, m.C(0xFFFFFF));
        });
        // ねぎ: 長い白い根元と、2つに分かれる緑の葉
        Icon(d, typeof(S729Parameter), (m, t, C, D) => m.Turn(0, 0, .45f, () =>
        {
            m.Limb(-300, 0, 0, 120, 0, 0, 28, 28, C);
            m.Tube([V3(100, -10, 0), V3(230, -40, -10), V3(355, -40, -20)], 16, D);
            m.Tube([V3(100, 10, 0), V3(230, 40, 10), V3(345, 45, 20)], 16, D);
            for (int i = 0; i < 5; i++) m.Limb(-300, -24 + i * 12, 0, -325, -28 + i * 14, 0, 2, 1.5f, m.C(0xBDBDB0));
        }));
        // たけのこ: 重なる皮(円すいが段になる)と根元
        Icon(d, typeof(S730Parameter), (m, t, C, D) =>
        {
            m.Super(0, 235, 0, 235, 30, 160, .5f, 1f, m.C(0x5A4030));
            for (int i = 0; i < 5; i++) { float y = 200 - i * 90, w = 165 - i * 30; m.Lathe(0, 0, i % 2 == 0 ? C : m.C(0x9A7040), 0, y - 180, w * .55f, y - 130, w, y, 0, y); }
        });
    }
}
