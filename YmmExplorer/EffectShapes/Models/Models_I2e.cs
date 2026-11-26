using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: アイコン 941〜970(標識・天体・ゲーム 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    static float Frac(float v) => v - MathF.Floor(v);

    /// <summary>標識の板: 色の板と、うしろの金属の板</summary>
    static void SignDisc(Sculpt m, float r, Vector4 col)
    {
        m.Disc(0, 0, 4, r, r, 24, col, .25f);
        m.Disc(0, 0, -14, r - 6, r - 6, 14, m.C(0x9AA3AD), .3f);
    }
    static void SignBox(Sculpt m, float w, float h, Vector4 col, float round = .12f)
    {
        m.Box(0, 0, 4, w, h, 26, col, round);
        m.Box(0, 0, -14, w - 12, h - 12, 14, m.C(0x9AA3AD), round);
    }

    /// <summary>人の形(頭・胴・手足)を板の上に浮き出させる</summary>
    static void Runner(Sculpt m, float z, Vector4 col, float hx, float hy, float hr, float w, params float[] seg)
    {
        m.Ball(hx, hy, z, hr, hr, hr * .5f, col);
        for (int i = 0; i + 3 < seg.Length; i += 4) m.Limb(seg[i], seg[i + 1], z, seg[i + 2], seg[i + 3], z, w, w, col);
    }

    /// <summary>ハートの輪郭(左右の円と下のとがり)</summary>
    static float[] HeartPts(float cx, float cy, float k)
    {
        var o = new List<float> { cx, cy + 275 * k };
        foreach (var v in Sculpt.ArcPts(-118, -65, 145, 2.45f, 5.66f, 14)) { o.Add(cx + v.X * k); o.Add(cy + v.Y * k); }
        foreach (var v in Sculpt.ArcPts(118, -65, 145, 3.76f, 6.97f, 14)) { o.Add(cx + v.X * k); o.Add(cy + v.Y * k); }
        return [.. o];
    }

    /// <summary>球の表面に沿った、ひらたい模様(クレーター・陸地)</summary>
    static void OnSphere(Sculpt m, float R, float x, float y, float rx, float ry, Vector4 col, float lift = 2f)
    {
        float z = Sculpt.FrontZ(0, 0, 0, R, R, R, x, y);
        m.Ball(x, y, z - MathF.Min(rx, ry) * .15f + lift, rx, ry, MathF.Min(rx, ry) * .3f, col);
    }

    static partial void RegisterI2e(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.125f, .14f, .17f, 1f);
        // 標識 止まれ: 柱の上の逆三角の板、白いふちと文字
        Icon(d, typeof(S941Parameter), (m, t, C, D) =>
        {
            var g = m.C(0x8A95A0);
            m.Cyl(0, 100, -40, 14, 460, g, .2f);
            m.Box(0, -80, -26, 70, 30, 14, g, .3f);
            m.Slab(-8, 26, m.C(0x9AA3AD), -270, -232, 270, -232, 0, 200);
            m.Slab(4, 22, D, -280, -240, 280, -240, 0, 210);
            m.Slab(10, 18, C, -235, -212, 235, -212, 0, 165);
            m.Text("止まれ", 72, 0, -95, 20, 6, D);
        });
        // 標識 進入禁止: 赤い丸板と白い横棒
        Icon(d, typeof(S942Parameter), (m, t, C, D) => { SignDisc(m, 270, C); m.Box(0, 0, 20, 370, 82, 16, D, .25f); });
        // 標識 駐車禁止: 赤いふちの青い丸と赤い斜め線
        Icon(d, typeof(S943Parameter), (m, t, C, D) =>
        {
            SignDisc(m, 270, D);
            m.Disc(0, 0, 14, 205, 205, 10, C, .3f);
            m.Limb(-145, -145, 22, 145, 145, 22, 24, 24, D);
        });
        // 標識 注意: 黄色いひし形板、黒いふち、浮き出たびっくりマーク
        Icon(d, typeof(S944Parameter), (m, t, C, D) =>
        {
            m.Slab(-12, 14, m.C(0x9AA3AD), 0, -290, 290, 0, 0, 290, -290, 0);
            m.Slab(4, 22, D, 0, -300, 300, 0, 0, 300, -300, 0);
            m.Slab(10, 18, C, 0, -265, 265, 0, 0, 265, -265, 0);
            m.Box(0, -45, 24, 44, 170, 14, D, .5f);
            m.Ball(0, 95, 24, 30, 30, 10, D);
        });
        // 標識 横断歩道: 青い四角い板、白い三角、歩く人
        Icon(d, typeof(S945Parameter), (m, t, C, D) =>
        {
            SignBox(m, 530, 530, C, .1f);
            m.Slab(20, 8, D, 0, -225, 225, 175, -225, 175);
            m.Slab(25, 4, C, 0, -165, 165, 140, -165, 140);
            Runner(m, 30, D, 5, -75, 26, 11, 5, -45, 0, 40, 0, 35, -45, 125, 0, 35, 50, 125, -42, -10, 50, 5);
        });
        // 標識 一方通行: 横長の青い板と白い矢印
        Icon(d, typeof(S946Parameter), (m, t, C, D) =>
        {
            SignBox(m, 620, 250, C, .1f);
            m.Box(-65, 0, 20, 330, 70, 14, D, .2f);
            m.Slab(20, 14, D, 90, -90, 255, 0, 90, 90);
        });
        // 標識 速度制限: 白い丸板、赤い太い輪、青い数字
        Icon(d, typeof(S947Parameter), (m, t, C, D) =>
        {
            SignDisc(m, 270, C);
            m.Torus(0, 0, 14, 232, 31, D);
            m.Text("40", 170, 0, 0, 20, 12, m.C(0x2F6FBB));
        });
        // 標識 工事中: 黄色い板と黒い斜めじま、まん中の文字
        Icon(d, typeof(S948Parameter), (m, t, C, D) =>
        {
            SignBox(m, 620, 210, C, .08f);
            for (int i = -3; i <= 3; i++) m.Slab(18, 4, D, i * 80 - 20, -100, i * 80 + 30, -100, i * 80 - 20, 100, i * 80 - 70, 100);
            m.Box(0, 0, 22, 350, 92, 8, C, .2f);
            m.Text("工事中", 58, 0, 0, 27, 5, D);
        });
        // 標識 信号機: 横長の箱、ひさしの付いた3つのランプ(順番に光る)
        Icon(d, typeof(S949Parameter), (m, t, C, D) =>
        {
            int on = ((int)(t * .8f) % 3 + 3) % 3;
            uint[] c0 = [0x35D06F, 0xFFD23F, 0xFF4D4D];
            m.Box(0, 0, 0, 620, 230, 120, C, .35f);
            for (int i = 0; i < 3; i++)
            {
                float x = -190 + i * 190;
                m.Disc(x, 0, 60, 76, 76, 20, i == on ? m.C(c0[i]) : Sculpt.Dark(m.C(c0[i]), .3f), .5f);
                m.Super(x, -78, 95, 88, 12, 40, .3f, 1f, C);
            }
        });
        // 標識 カラーコーン: 四角い台、円すい、白い帯
        Icon(d, typeof(S950Parameter), (m, t, C, D) =>
        {
            m.Box(0, 265, 0, 500, 50, 500, C, .15f);
            m.Lathe(0, 0, C, 0, -285, 48, -285, 170, 250, 0, 250);
            float r(float y) => 48 + (170 - 48) * (y + 285) / 535f;
            m.Lathe(0, 0, D, r(-105) + 3, -105, r(20) + 3, 20);
        });
        // 標識 踏切: 柱、しまの付いたばつ印の板、交互に光る2つのランプ
        Icon(d, typeof(S951Parameter), (m, t, C, D) =>
        {
            var g = m.C(0x8A95A0);
            m.Cyl(0, 100, -30, 16, 560, g, .2f);
            m.Turn(0, -90, .7854f, () =>
            {
                for (int a = 0; a < 2; a++) m.Turn(0, -90, a * 1.5708f, () =>
                {
                    float z = a * 24f;
                    m.Box(0, -90, z, 520, 76, 20, C, .15f);
                    for (int i = -3; i <= 3; i++) m.Slab(z + 11, 3, D, i * 74 - 16, -128, i * 74 + 20, -128, i * 74 + 4, -52, i * 74 - 32, -52);
                });
            });
            bool b = Frac(t * .8f) < .5f;
            m.Box(0, -285, -20, 300, 30, 20, K, .4f);
            foreach (int s in Sides)
            {
                m.Ball(s * 110, -285, -10, 60, 60, 30, K);
                m.Disc(s * 110, -285, 18, 48, 48, 14, (s < 0) == b ? m.C(0xFF3030) : m.C(0x5A1A1A), .5f);
            }
        });
        // 標識 出口: 緑の板、ドアのわく、走る人、矢印
        Icon(d, typeof(S952Parameter), (m, t, C, D) =>
        {
            SignBox(m, 620, 290, C, .08f);
            m.Tube([V3(-265, -105, 20), V3(-115, -105, 20), V3(-115, 105, 20), V3(-265, 105, 20), V3(-265, -105, 20)], 6, D);
            Runner(m, 22, D, 45, -70, 30, 12, 45, -35, 20, 50, 20, 45, 90, 100, 20, 45, -25, 110, 35, -15, 105, 20);
            m.Box(200, 0, 20, 110, 28, 12, D, .3f);
            m.Slab(20, 12, D, 240, -44, 290, 0, 240, 44);
        });
        // 標識 駐車場: 青い四角い板と、浮き出た P
        Icon(d, typeof(S953Parameter), (m, t, C, D) => { SignBox(m, 520, 520, C, .12f); m.Text("P", 380, 0, 0, 26, 24, D); });
        // 標識 トイレ: 板と仕切り、男の人(体と足)、女の人(スカートと足)
        Icon(d, typeof(S954Parameter), (m, t, C, D) =>
        {
            SignBox(m, 620, 420, C, .1f);
            m.Box(0, 0, 18, 12, 300, 8, D, .5f);
            m.Ball(-145, -110, 24, 38, 38, 20, D);
            m.Box(-145, 20, 22, 86, 180, 18, D, .4f);
            foreach (int s in Sides) m.Box(-145 + s * 23, 130, 20, 28, 100, 14, D, .4f);
            m.Ball(145, -110, 24, 38, 38, 20, D);
            m.Slab(22, 18, D, 145, -65, 225, 105, 65, 105);
            foreach (int s in Sides) m.Box(145 + s * 23, 145, 20, 24, 80, 14, D, .4f);
        });
        // 標識 自転車マーク: 青い板と、管で作った自転車
        Icon(d, typeof(S955Parameter), (m, t, C, D) =>
        {
            SignBox(m, 540, 540, C, .1f);
            float z = 24;
            foreach (int s in Sides) m.Torus(s * 130, 85, z, 85, 8, D);
            m.Tube([V3(-130, 85, z), V3(-30, -40, z), V3(90, -40, z), V3(130, 85, z)], 7, D);
            m.Tube([V3(-30, -40, z), V3(0, 85, z), V3(90, -40, z)], 7, D);
            m.Limb(-50, -75, z, 0, -75, z, 8, 8, D);
            m.Limb(90, -40, z, 70, -95, z, 7, 7, D);
            m.Limb(45, -100, z, 110, -100, z, 7, 7, D);
        });
        // 天体 太陽: 光る球と、まわりにのびる12本のとがった光
        Icon(d, typeof(S956Parameter), (m, t, C, D) =>
        {
            for (int i = 0; i < 12; i++) { float a = i * .5236f - MathF.PI / 2f; m.Limb(MathF.Cos(a) * 170, MathF.Sin(a) * 170, 0, MathF.Cos(a) * 310, MathF.Sin(a) * 310, 0, 38, 2, D); }
            m.Ball(0, 0, 0, 175, C);
            m.Ball(-55, -55, 120, 60, 60, 30, m.C(0xFFFFFF, .3f));
        });
        // 天体 三日月: ふちの丸い厚い三日月
        Icon(d, typeof(S957Parameter), (m, t, C, D) =>
        {
            var q = new List<float>();
            for (int i = 0; i <= 40; i++) { float a = .95f + i * .1095f; q.Add(MathF.Cos(a) * 250); q.Add(MathF.Sin(a) * 250); }
            for (int i = 1; i < 40; i++) { float u = -1f + i / 20f; q.Add(143 - 200 * (1 - u * u)); q.Add(u * 204); }
            Puff(m, 0, 110, C, [.. q]);
        });
        // 天体 満月: 球と、表面にへばりついたクレーター
        Icon(d, typeof(S958Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 255, C);
            foreach (var (x, y, r) in new[] { (-90f, -80f, 48f), (70f, -120f, 28f), (95f, 60f, 62f), (-70f, 110f, 34f), (-160f, 35f, 18f) }) OnSphere(m, 255, x, y, r, r, D);
        });
        // 天体 土星: しまの球と、ななめに傾いた平たい輪
        Icon(d, typeof(S959Parameter), (m, t, C, D) => m.Turn(0, 0, -.3f, () =>
        {
            m.Ball(0, 0, 0, 170, C);
            m.Pitch(0, -60, 0, MathF.PI / 2f, () => m.Torus(0, -60, 0, MathF.Sqrt(170 * 170 - 60 * 60) + 1, 10, m.C(0xFFFFFF, .35f)));
            m.Pitch(0, 45, 0, MathF.PI / 2f, () => m.Torus(0, 45, 0, MathF.Sqrt(170 * 170 - 45 * 45) + 1, 12, m.C(0x8A5A1F, .35f)));
            m.Pitch(0, 0, 0, -.26f, () => m.Lathe(0, 0, D, 225, -5, 335, -5, 335, 5, 225, 5, 225, -5));
        }));
        // 天体 青い惑星: 球、表面の陸地、うすい大気
        Icon(d, typeof(S960Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 265, C);
            foreach (var (x, y, a, b) in new[] { (-90f, -95f, 105f, 70f), (95f, 30f, 88f, 120f), (-120f, 120f, 60f, 44f), (20f, -205f, 70f, 26f) }) OnSphere(m, 265, x, y, a, b, D);
            m.Ball(0, 0, 0, 282, m.C(0xBFE6FF, .18f));
        });
        // 天体 赤い惑星: 球と、ふちの明るいクレーター
        Icon(d, typeof(S961Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 0, 0, 255, C);
            foreach (var (x, y, r) in new[] { (-100f, -85f, 46f), (80f, -45f, 30f), (45f, 115f, 62f), (-115f, 105f, 26f) })
            {
                OnSphere(m, 255, x, y, r, r, D);
                float z = Sculpt.FrontZ(0, 0, 0, 255, 255, 255, x, y);
                m.Tube(ArcPts3(x, y, r, 3.5f, 5.6f, z + 4, 8), 4, m.C(0xF0946C));
            }
        });
        // 天体 ほうき星: 光る頭と、うしろへ広がる透けた尾
        Icon(d, typeof(S962Parameter), (m, t, C, D) =>
        {
            for (int i = 0; i < 4; i++) m.Limb(-20, 20, 0, 280 + i * 10, -250 - i * 12, -30 - i * 20, 80 - i * 8, 18 + i * 6, m.C(D, .22f - i * .04f));
            m.Ball(-20, 20, 0, 84, C);
            m.Ball(-40, 0, 50, 36, W);
        });
        // 天体 流れ星: ななめに飛ぶ立体の星と、光の粒の尾
        Icon(d, typeof(S963Parameter), (m, t, C, D) =>
        {
            float k0 = Frac(t * .35f), x0 = 250 - k0 * 480, y0 = -180 + k0 * 340;
            for (int i = 0; i < 10; i++) m.Ball(x0 + i * 24, y0 - i * 17, -i * 6, 14 - i, m.C(D, .75f - i * .07f));
            m.Turn(x0, y0, t, () => Gem(m, Pts(StarPts(x0, y0, 62, 5, .45f, 0f)), x0, y0, 0f, 5, 24, C, C));
        });
        // 天体 人工衛星: 箱の本体、両側の太陽電池板、アンテナ
        Icon(d, typeof(S964Parameter), (m, t, C, D) => m.Turn(0, 0, -.5f, () =>
        {
            foreach (int s in Sides)
            {
                m.Limb(s * 90, 0, 0, s * 135, 0, 0, 8, 8, C);
                m.Box(s * 235, 0, 0, 200, 120, 10, D, .05f);
                for (int i = 0; i < 4; i++) m.Box(s * (155 + i * 53), 0, 6, 4, 120, 2, W, .5f);
            }
            m.Box(0, 0, 0, 175, 130, 150, C, .2f);
            m.Limb(0, -65, 0, 0, -125, 0, 4, 4, C);
            m.Lathe(0, 0, C, 0, -150, 30, -158, 46, -176);
            m.Ball(0, -125, 0, 11, m.C(0xE5392F));
        }));
        // 天体 銀河のうず: 2本の腕にならんだ星の粒(厚みに少しばらつき)、明るい中心
        Icon(d, typeof(S965Parameter), (m, t, C, D) =>
        {
            for (int a = 0; a < 2; a++) for (int i = 0; i < 90; i += 2)
            {
                float q = i * .095f + a * 3.1416f, r = 12 + i * 3.2f, h = MathF.Sin(i * 12.9898f + a * 78.233f) * 43758.5453f;
                h -= MathF.Floor(h);
                m.Ball(MathF.Cos(q) * r, MathF.Sin(q) * r * .75f, (h - .5f) * 40f, (11f - i * .07f) + h * 4, m.C(i % 3 == 0 ? D : C, 1f - i / 110f));
            }
            m.Ball(0, 0, 0, 42, D);
            m.Ball(0, 0, 0, 90, 70, 30, m.C(D, .25f));
        });
        // ゲーム サイコロ: 本物の立方体(6つの面に1〜6の目。上が赤い1)
        Icon(d, typeof(S966Parameter), (m, t, C, D) => m.Pitch(0, 0, 0, -.55f, () => m.Yaw(0, 0, 0, .7854f, () =>
        {
            m.Box(0, 0, 0, 300, 300, 300, C, .14f);
            DiePips(m, 0, 0, 0, 150, 3, 1, K, D);
        })));
        // ゲーム トランプ: 薄いカード、浮き出た赤いハート、すみの文字
        Icon(d, typeof(S967Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, 0, 390, 540, 8, C, .08f);
            Puff(m, 10, 14, D, HeartPts(0, -5, .42f));
            m.Text("A", 64, -130, -210, 6, 3, D);
            m.Turn(130, 210, MathF.PI, () => m.Text("A", 64, 130, 210, 6, 3, D));
        });
        // ゲーム コイン: 本当にくるくる回る厚いコイン(両面に星)
        Icon(d, typeof(S968Parameter), (m, t, C, D) => m.Yaw(0, 0, 0, t * 2.4f, () =>
        {
            m.Disc(0, 0, 0, 250, 250, 44, D, .3f);
            foreach (int s in Sides)
            {
                m.Disc(0, 0, s * 14, 212, 212, 20, C, .2f);
                m.Torus(0, 0, s * 24, 178, 6, D);
                m.Slab(s * 26, 8, D, StarPts(0, 0, 118, 5, .46f, 0f));
            }
            m.Tube(ArcPts3(0, 0, 194, 3.6f, 4.7f, 26, 8), 5, W);
        }));
        // ゲーム 宝箱: 木の箱、丸いふた、金の帯と錠前
        Icon(d, typeof(S969Parameter), (m, t, C, D) =>
        {
            m.Box(0, 110, 0, 540, 270, 300, C, .06f);
            m.Turn(0, -25, -MathF.PI / 2f, () =>
            {
                float h = -MathF.PI / 2f;
                m.Lathe(0, 0, m.C(0xC17634), [new(0, -295), new(148, -295), new(150, -290), new(150, 240), new(148, 245), new(0, 245)], h, -h);
                foreach (int s in Sides) { float c = s * 195 - 25; m.Lathe(0, 0, D, [new(0, c - 25), new(156, c - 25), new(156, c + 25), new(0, c + 25)], h, -h); }
            });
            m.Box(0, -25, 0, 548, 28, 308, D, .2f);
            foreach (int s in Sides) m.Box(s * 195, 120, 0, 50, 250, 306, D, .1f);
            m.Box(0, 20, 158, 92, 110, 20, D, .3f);
            m.Ball(0, 10, 170, 18, 18, 4, K);
            m.Box(0, 42, 170, 12, 30, 4, K, .5f);
        });
        // ゲーム 剣: ななめの剣。刃は真ん中に稜線のある両刃、つば・にぎり・柄頭
        Icon(d, typeof(S970Parameter), (m, t, C, D) => m.Turn(0, 0, .7854f, () =>
        {
            var g = m.C(0xB7BEC6);
            var L1 = new Vector3(-34, 110, 0); var L2 = new Vector3(-34, -250, 0); var R1 = new Vector3(34, 110, 0); var R2 = new Vector3(34, -250, 0); var T = new Vector3(0, -325, 0);
            foreach (int s in Sides)
            {
                var M1 = new Vector3(0, 110, s * 14); var M2 = new Vector3(0, -250, s * 14);
                m.Quad3D(L1, L2, M2, M1, C); m.Tri3D(L2, T, M2, C);
                m.Quad3D(R1, R2, M2, M1, g); m.Tri3D(R2, T, M2, g);
            }
            m.Box(0, 125, 0, 210, 42, 60, D, .4f);
            m.Cyl(0, 205, 0, 22, 130, m.C(0x7A4A28), .2f);
            m.Ball(0, 285, 0, 32, D);
        }));
    }
}
