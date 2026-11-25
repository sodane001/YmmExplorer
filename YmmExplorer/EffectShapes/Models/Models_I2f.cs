using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: アイコン 971〜1000(ゲーム・建物・乗り物 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    static float H01(int n) { float h = MathF.Sin(n * 12.9898f) * 43758.5453f; return h - MathF.Floor(h); }

    /// <summary>車輪(手前と奥に1つずつ): タイヤとホイール</summary>
    static void Wheels(Sculpt m, float x, float y, float r, float zHalf, Vector4 tire, Vector4 hub, float w = 46f)
    {
        foreach (int s in Sides)
        {
            m.Disc(x, y, s * zHalf, r, r, w, tire, .45f);
            m.Disc(x, y, s * (zHalf + w * .3f), r * .45f, r * .45f, 8, hub, .4f);
        }
    }

    /// <summary>箱の手前の面と奥の面に、同じ窓を付ける</summary>
    static void SideWin(Sculpt m, float x, float y, float w, float h, float zFace, Vector4 col, float round = .2f)
    {
        foreach (int s in Sides) m.Box(x, y, s * zFace, w, h, 6, col, round);
    }

    static Vector3 Ring(float r, float y, float a) => new(MathF.Sin(a) * r, y, MathF.Cos(a) * r);

    static partial void RegisterI2f(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.125f, .14f, .17f, 1f);
        var glass = new Vector4(.56f, .85f, .97f, 1f);
        var silver = new Vector4(.78f, .82f, .85f, 1f);
        // ゲーム 盾: 金のふちの厚い盾、青い面、浮き出た十字
        Icon(d, typeof(S971Parameter), (m, t, C, D) =>
        {
            Puff(m, 0, 60, D, -245, -255, 245, -255, 245, 30, 0, 305, -245, 30);
            m.Slab(26, 24, C, -210, -220, 210, -220, 210, 15, 0, 255, -210, 15);
            m.Box(0, 0, 42, 44, 420, 16, D, .3f); m.Box(0, -55, 42, 370, 44, 16, D, .3f);
        });
        // ゲーム 弓矢: 曲がった弓、つる、矢(矢じり・矢羽根)
        Icon(d, typeof(S972Parameter), (m, t, C, D) =>
        {
            m.Tube(ArcPts3(-130, 0, 300, -1.25f, 1.25f, 0, 20), 13, C);
            m.Limb(-35, -285, 0, -110, 0, 0, 2.5f, 2.5f, D); m.Limb(-35, 285, 0, -110, 0, 0, 2.5f, 2.5f, D);
            m.Limb(-110, 0, 0, 270, 0, 0, 6, 6, m.C(0xD9A566));
            m.Limb(262, 0, 0, 340, 0, 0, 26, 1, D);
            var R = m.C(0xE5392F);
            m.Tri3D(new(-125, -26, 0), new(-65, 0, 0), new(-125, 26, 0), R);
            m.Tri3D(new(-125, 0, -26), new(-65, 0, 0), new(-125, 0, 26), R);
        });
        // ゲーム ポーション: 丸いガラスびん、中の液体、首と栓、のぼる泡
        Icon(d, typeof(S973Parameter), (m, t, C, D) =>
        {
            var prof = new List<Vector2> { new(0, 75 + 185) };
            for (int i = 1; i <= 14; i++) { float th = i / 14f * 1.67f; prof.Add(new(MathF.Sin(th) * 185, 75 + MathF.Cos(th) * 185)); }
            prof.Add(new(0, prof[^1].Y));
            m.Lathe(0, 0, C, prof);
            m.Ball(0, prof[^1].Y + 1, 0, 180, 6, 180, m.C(0xFF7AA4));
            for (int i = 0; i < 5; i++) { float k = Frac(t * .4f + i / 5f); m.Ball(-100 + i * 48, 180 - k * 170, (i % 3 - 1) * 50, 7 + i % 2 * 4, m.C(0xFFFFFF, 1f - k)); }
            m.Ball(0, 75, 0, 205, m.C(D, .35f));
            m.Cyl(0, -160, 0, 55, 80, m.C(D, .6f), .1f);
            m.Cyl(0, -218, 0, 75, 36, D, .4f);
            m.Cyl(0, -262, 0, 46, 62, m.C(0xB9823E), .3f);
        });
        // ゲーム 爆弾: 黒い球、口金、導火線、ちらちらする火花
        Icon(d, typeof(S974Parameter), (m, t, C, D) =>
        {
            m.Ball(-20, 60, 0, 215, C);
            m.Turn(95, -130, .6f, () => m.Cyl(95, -130, 0, 55, 70, C, .2f));
            m.Ball(-90, -20, 150, 42, 62, 20, m.C(0xFFFFFF, .25f));
            m.Tube([V3(110, -150), V3(170, -250), V3(265, -225)], 5.5f, m.C(0xD9A566));
            float r = 42 + MathF.Sin(t * 14f) * 14;
            m.Turn(270, -228, t * 4f, () => Gem(m, Pts(StarPts(270, -228, r, 8, .4f, 0f)), 270, -228, 0f, 4, 18, D, D));
        });
        // ゲーム 王冠: 輪になった冠(5本のとがり)、金の帯、宝石
        Icon(d, typeof(S975Parameter), (m, t, C, D) =>
        {
            const float R = 250; int N = 80;
            float Top(float a) { float f = Frac(a * 5f / MathF.Tau + .5f); return 10 - (1 - MathF.Abs(2 * f - 1)) * 225; }
            var dark = Sculpt.Dark(C, .75f);
            for (int k = 0; k < N; k++)
            {
                float a0 = k * MathF.Tau / N, a1 = (k + 1) * MathF.Tau / N;
                m.Quad3D(Ring(R, Top(a0), a0), Ring(R, Top(a1), a1), Ring(R, 200, a1), Ring(R, 200, a0), C);
                m.Quad3D(Ring(R - 14, Top(a0), a0), Ring(R - 14, Top(a1), a1), Ring(R - 14, 200, a1), Ring(R - 14, 200, a0), dark);
                m.Quad3D(Ring(R, Top(a0), a0), Ring(R, Top(a1), a1), Ring(R - 14, Top(a1), a1), Ring(R - 14, Top(a0), a0), C);
            }
            for (int j = 0; j < 5; j++) { var p = Ring(R - 7, -222, j * MathF.Tau / 5); m.Ball(p.X, p.Y, p.Z, 30, C); }
            m.Lathe(0, 0, m.C(0xE5A81E), R + 8, 146, R + 8, 194);
            m.Ball(0, 95, R + 6, 42, 42, 22, D);
            m.Ball(-130, 105, MathF.Sqrt(R * R - 130 * 130) + 6, 26, 26, 14, m.C(0x2F86C6));
            m.Ball(130, 105, MathF.Sqrt(R * R - 130 * 130) + 6, 26, 26, 14, m.C(0x35B56A));
        });
        // ゲーム 宝石: 上の平らな面、面取り、下へとがる八角のカット
        Icon(d, typeof(S976Parameter), (m, t, C, D) =>
        {
            int n = 8; var p1 = m.C(0x22A7C8); var p2 = m.C(0x2BBAD6);
            for (int k = 0; k < n; k++)
            {
                float a0 = (k + .5f) * MathF.Tau / n, a1 = (k + 1.5f) * MathF.Tau / n;
                m.Quad3D(Ring(160, -210, a0), Ring(160, -210, a1), Ring(285, -60, a1), Ring(285, -60, a0), k % 2 == 0 ? C : D);
                m.Tri3D(Ring(285, -60, a0), Ring(285, -60, a1), new(0, 275, 0), k % 2 == 0 ? p1 : p2);
                m.Tri3D(new(0, -210, 0), Ring(160, -210, a0), Ring(160, -210, a1), D);
            }
        });
        // ゲーム 将棋の駒: 五角形の厚い駒(下ほど厚い)、木目のふち、王の字
        Icon(d, typeof(S977Parameter), (m, t, C, D) =>
        {
            var o = Pts(0, -285, 170, -180, 235, 270, -235, 270, -170, -180);
            float Zf(float y) => 22 + (y + 285) / 555f * 30;
            var e = m.C(0xB98A45);
            for (int i = 0; i < 5; i++)
            {
                var a = o[i]; var b = o[(i + 1) % 5];
                m.Quad3D(new(a, Zf(a.Y)), new(b, Zf(b.Y)), new(b, -Zf(b.Y)), new(a, -Zf(a.Y)), e);
                foreach (int s in Sides) m.Tri3D(new(0, 0, s * Zf(0) * 1.05f), new(a, s * Zf(a.Y)), new(b, s * Zf(b.Y)), C);
            }
            m.Text("王", 220, 0, 40, Zf(40) + 4, 4, D);
        });
        // ゲーム チェスの駒(ポーン): 回転体の台・胴・えり・丸い頭
        Icon(d, typeof(S978Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, D, 0, 294, 175, 294, 178, 260, 160, 236, 0, 236);
            m.Lathe(0, 0, C, 135, 240, 100, 120, 76, 40, 70, 20, 0, 20);
            m.Lathe(0, 0, D, 0, 22, 100, 22, 104, 5, 100, -12, 0, -12);
            m.Ball(0, -115, 0, 110, C);
            m.Ball(-35, -145, 85, 26, 38, 12, W);
        });
        // ゲーム ダーツの的: 段になった丸い的と、飛んできて刺さる矢
        Icon(d, typeof(S979Parameter), (m, t, C, D) =>
        {
            m.Disc(0, 0, -10, 285, 285, 30, K, .3f);
            for (int i = 5; i >= 1; i--) m.Disc(0, 0, (5 - i) * 3, 55 * i, 55 * i, 24, i % 2 == 1 ? C : D, .15f);
            m.Disc(0, 0, 16, 30, 30, 16, m.C(0xFFD23F), .4f);
            float k0 = MathF.Min(1f, Frac(t * .35f) * 5f), off = (1 - k0) * 500;
            m.Turn(0, 0, -.7f, () =>
            {
                var dir = Vector3.Normalize(new Vector3(.55f, 0, .85f));
                var tip = dir * off; var tail = tip + dir * 340;
                m.Limb(tip.X, tip.Y, tip.Z + 22, tail.X, tail.Y, tail.Z + 22, 2, 9, K);
                var b = m.C(0x2F86C6); var f0 = tip + dir * 250;
                m.Tri3D(f0 + new Vector3(0, 0, 22), tail + new Vector3(0, -36, 22), tail + new Vector3(0, 36, 22), b);
                m.Tri3D(f0 + new Vector3(0, 0, 22), tail + new Vector3(-30, 0, 40), tail + new Vector3(30, 0, 4), b);
            });
        });
        // ゲーム コントローラー: 丸い本体と両手のにぎり、十字キー、4色のボタン
        Icon(d, typeof(S980Parameter), (m, t, C, D) =>
        {
            m.Box(0, 10, 0, 560, 220, 110, C, .5f);
            foreach (int s in Sides) m.Ball(s * 190, 110, -10, 78, 110, 55, C);
            m.Box(-165, 10, 56, 105, 32, 18, D, .3f); m.Box(-165, 10, 56, 32, 105, 18, D, .3f);
            foreach (var (x, y, c0) in new[] { (165f, -35f, 0xFFD23Fu), (215f, 10f, 0xE5392Fu), (115f, 10f, 0x2F86C6u), (165f, 55f, 0x35B56Au) }) m.Disc(x, y, 56, 22, 22, 18, m.C(c0), .5f);
            m.Box(0, -35, 54, 70, 18, 10, D, .5f);
        });
        // 建物 一軒家: 箱の家、切妻の屋根、えんとつ、ドア、十字の窓
        Icon(d, typeof(S981Parameter), (m, t, C, D) =>
        {
            const float Z = 150;
            m.Box(0, 110, 0, 450, 360, Z * 2, C, .04f);
            m.Tri3D(new(-225, -70, Z), new(225, -70, Z), new(0, -255, Z), C); m.Tri3D(new(-225, -70, -Z), new(225, -70, -Z), new(0, -255, -Z), C);
            m.Box(165, -200, -40, 58, 130, 58, m.C(0x8A5A32), .1f);
            foreach (int s in Sides) m.Quad3D(new(0, -300, Z + 25), new(s * 300, -50, Z + 25), new(s * 300, -50, -Z - 25), new(0, -300, -Z - 25), D);
            var br = m.C(0x8A5A32);
            m.Box(-105, 155, Z + 2, 110, 190, 8, br, .1f); m.Ball(-75, 160, Z + 8, 8, m.C(0xFFD23F));
            m.Box(115, 70, Z + 2, 130, 120, 6, glass, .1f);
            m.Box(115, 70, Z + 6, 6, 120, 4, W, .5f); m.Box(115, 70, Z + 6, 130, 6, 4, W, .5f);
        });
        // 建物 高層ビル: 高い箱、屋上の箱とアンテナ、前と横の窓(明かりがつく)
        Icon(d, typeof(S982Parameter), (m, t, C, D) =>
        {
            m.Box(0, 20, 0, 300, 560, 260, C, .03f);
            m.Box(0, -285, 0, 70, 60, 70, C, .1f);
            m.Limb(0, -310, 0, 0, -345, 0, 4, 3, C);
            int tick = (int)(t * .6f); var off = m.C(0x34425A);
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 4; x++) m.Box(-90 + x * 60, -210 + y * 54, 131, 36, 32, 4, H01(x + y * 9 + tick) > .45f ? D : off, .1f);
                for (int x = 0; x < 4; x++) foreach (int s in Sides) m.Box(s * 151, -210 + y * 54, -90 + x * 60, 4, 32, 36, H01(x + y * 9 + 50 + s * 30 + tick) > .45f ? D : off, .1f);
            }
        });
        // 建物 学校: 横に長い校舎、まん中の塔と三角屋根、時計、窓と入口
        Icon(d, typeof(S983Parameter), (m, t, C, D) =>
        {
            m.Box(0, 120, 0, 620, 300, 240, C, .03f);
            m.Box(0, -20, 10, 160, 300, 260, C, .03f);
            foreach (int s in Sides) m.Quad3D(new(0, -280, 145), new(s * 110, -170, 145), new(s * 110, -170, -125), new(0, -280, -125), D);
            m.Tri3D(new(-110, -170, 145), new(110, -170, 145), new(0, -280, 145), D);
            m.Disc(0, -95, 142, 48, 48, 8, W, .4f); m.Torus(0, -95, 146, 48, 4, K);
            m.Limb(0, -95, 148, 0, -125, 148, 3, 3, K); m.Limb(0, -95, 148, 24, -95, 148, 3, 3, K);
            m.Box(0, 185, 142, 88, 170, 6, m.C(0x8A5A32), .1f);
            for (int y = 0; y < 2; y++) for (int x = 0; x < 4; x++) { float xx = x < 2 ? -250 + x * 90 : 120 + (x - 2) * 90; m.Box(xx, 60 + y * 105, 122, 58, 62, 6, glass, .1f); }
        });
        // 建物 病院: 白い建物、上の箱に赤十字、窓と入口
        Icon(d, typeof(S984Parameter), (m, t, C, D) =>
        {
            m.Box(0, 70, 0, 480, 450, 260, C, .03f);
            m.Box(0, -185, 0, 200, 70, 200, C, .05f);
            m.Box(0, -185, 102, 100, 34, 8, D, .2f); m.Box(0, -185, 102, 34, 62, 8, D, .2f);
            for (int y = 0; y < 3; y++) for (int x = 0; x < 4; x++) { if (y == 2 && (x == 1 || x == 2)) continue; m.Box(-165 + x * 110, -70 + y * 105, 132, 62, 60, 6, glass, .1f); }
            m.Box(0, 215, 132, 110, 160, 6, m.C(0x5A6B85), .1f);
        });
        // 建物 洋風のお城: 丸い3本の塔ととんがり屋根、城壁と凹凸、木の門
        Icon(d, typeof(S985Parameter), (m, t, C, D) =>
        {
            foreach (int x in new[] { -240, 0, 240 })
            {
                float h = x == 0 ? 1.25f : 1f, top = -90 * h, bot = 290;
                m.Cyl(x, (top + bot) / 2f, 0, 52, bot - top, C, .05f);
                m.Lathe(x, 0, D, 0, -270 * h, 72, top, 72, top + 6, 0, top + 6);
                m.Box(x, top + 70, 53, 24, 40, 4, K, .3f);
            }
            m.Box(0, 190, -20, 560, 200, 120, C, .03f);
            for (int i = 0; i < 8; i++) m.Box(-245 + i * 70, 80, -20, 36, 36, 120, C, .05f);
            var w = m.C(0x6B4428);
            m.Slab(42, 8, w, PiePts(0, 290, 76, MathF.PI, MathF.Tau));
        });
        // 建物 鳥居: 反った笠木、横の貫、すぼまった2本の柱、台石、額
        Icon(d, typeof(S986Parameter), (m, t, C, D) =>
        {
            m.Slab(0, 60, D, -340, -235, 340, -235, 300, -190, -300, -190);
            m.Box(0, -165, 0, 590, 48, 52, C, .1f);
            m.Box(0, -55, 0, 500, 36, 40, C, .1f);
            foreach (int s in Sides)
            {
                m.Limb(s * 182, -190, 0, s * 207, 300, 0, 22, 27, C);
                m.Box(s * 207, 300, 0, 84, 26, 84, D, .2f);
            }
            m.Box(0, -110, 0, 44, 76, 30, C, .2f);
        });
        // 建物 灯台: 赤白しまの塔、回廊、光る部屋と屋根、ぐるっと回る光
        Icon(d, typeof(S987Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 125, 300, 72, -130, 0, -130);
            float R(float y) => 72 + (125 - 72) * (y + 130) / 430f;
            for (int i = 0; i < 2; i++) { float y0 = 145 - i * 210, y1 = 245 - i * 210; m.Lathe(0, 0, D, R(y1) + 2, y1, R(y0) + 2, y0); }
            m.Cyl(0, -145, 0, 95, 24, D, .3f);
            m.Cyl(0, -205, 0, 55, 110, m.C(0xFFE27A), .1f);
            m.Lathe(0, 0, D, 0, -335, 85, -260, 0, -260);
            float a0 = t * .8f;
            m.Limb(0, -205, 0, MathF.Cos(a0) * 360, -205, MathF.Sin(a0) * 360, 30, 85, m.C(0xFFF3A8, .16f + .14f * MathF.Abs(MathF.Sin(a0))));
        });
        // 建物 風車: すぼまった塔ととんがり帽子、ドア、手前で回る4枚の羽
        Icon(d, typeof(S988Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 140, 300, 85, -85, 0, -85);
            m.Lathe(0, 0, D, 0, -205, 110, -85, 0, -85);
            m.Box(0, 240, 118, 60, 120, 8, m.C(0x6B4428), .4f);
            m.Limb(0, -90, 60, 0, -90, 120, 20, 20, K);
            for (int i = 0; i < 4; i++) m.Turn(0, -90, t * .9f + i * 1.5708f, () =>
            {
                m.Limb(0, -90, 120, 0, -350, 120, 5, 5, m.C(0x8A5A32));
                m.Slab(120, 6, m.C(0xFFFFFF, .95f), 4, -165, 75, -175, 75, -345, 4, -340);
            });
        });
        // 建物 テント: 四角すいのテント、手前の入口、柱と旗、草地
        Icon(d, typeof(S989Parameter), (m, t, C, D) =>
        {
            var top = new Vector3(0, -235, 0); var dk = m.C(0xC95528);
            var c = new[] { new Vector3(-335, 240, 240), new Vector3(335, 240, 240), new Vector3(335, 240, -240), new Vector3(-335, 240, -240) };
            for (int i = 0; i < 4; i++) m.Tri3D(top, c[i], c[(i + 1) % 4], i % 2 == 0 ? C : dk);
            float Fz(float y) => (y + 235) / 475f * 240 + 2;
            m.Tri3D(new(0, -120, Fz(-120)), new(80, 240, Fz(240)), new(-80, 240, Fz(240)), D);
            m.Limb(0, -235, 0, 0, -305, 0, 4, 4, m.C(0x8A5A32));
            m.Tri3D(new(0, -305, 0), new(70, -280, 0), new(0, -255, 0), m.C(0xE5392F));
            m.Box(0, 248, 0, 760, 14, 560, m.C(0x6B8E3B), .2f);
        });
        // 建物 工場: のこぎり屋根の建物、黄色い窓、えんとつと立ちのぼる煙
        Icon(d, typeof(S990Parameter), (m, t, C, D) =>
        {
            m.Slab(0, 260, C, -320, 270, -320, -40, -160, 70, -160, -40, 0, 70, 0, -40, 160, 70, 160, -140, 320, -140, 320, 270);
            for (int i = 0; i < 4; i++) m.Box(-240 + i * 150, 175, 131, 70, 70, 6, D, .1f);
            m.Cyl(240, -200, -40, 30, 130, Sculpt.Dark(C, .8f), .1f);
            for (int i = 0; i < 5; i++) { float k = Frac(t * .35f + i / 5f); m.Ball(240 + k * 60, -270 - k * 150, -40, 18 + k * 40, m.C(0xB7BEC6, .7f * (1f - k))); }
        });
        // 乗り物 乗用車: 丸い車体、屋根、両側の窓、4つのタイヤ、ライト
        Icon(d, typeof(S991Parameter), (m, t, C, D) =>
        {
            m.Box(0, 70, 0, 620, 150, 300, C, .35f);
            m.Slab(0, 250, C, -195, 10, -110, -120, 105, -120, 205, 10);
            foreach (int s in Sides) { m.Slab(s * 126, 4, D, -145, 5, -85, -90, 0, -90, 0, 5); m.Slab(s * 126, 4, D, 25, 5, 25, -90, 95, -90, 160, 5); }
            m.Quad3D(new(110, -112, 110), new(195, 2, 118), new(195, 2, -118), new(110, -112, -110), D);
            Wheels(m, -185, 150, 72, 135, K, m.C(0xC8D0D8));
            Wheels(m, 185, 150, 72, 135, K, m.C(0xC8D0D8));
            foreach (int s in Sides) m.Ball(305, 55, s * 95, 14, 22, 26, m.C(0xFFE27A));
        });
        // 乗り物 バス: 長い箱の車体、白い帯、両側の窓、前の窓、タイヤ
        Icon(d, typeof(S992Parameter), (m, t, C, D) =>
        {
            m.Box(0, 20, 0, 680, 300, 260, C, .15f);
            m.Box(0, 60, 0, 684, 20, 264, W, .1f);
            for (int i = 0; i < 5; i++) SideWin(m, -255 + i * 102, -55, 78, 88, 131, D);
            m.Box(341, -40, 0, 6, 110, 210, D, .15f);
            m.Box(285, 10, 131, 80, 190, 6, D, .1f);
            Wheels(m, -200, 170, 62, 125, K, m.C(0xC8D0D8)); Wheels(m, 190, 170, 62, 125, K, m.C(0xC8D0D8));
        });
        // 乗り物 トラック: 大きな荷台、運転席と窓、車台、6つのタイヤ
        Icon(d, typeof(S993Parameter), (m, t, C, D) =>
        {
            m.Box(-105, 5, 0, 470, 270, 250, D, .05f);
            m.Box(250, 50, 0, 190, 180, 230, C, .3f);
            m.Slab(116, 4, glass, 215, -20, 305, -20, 330, 50, 215, 50); m.Slab(-116, 4, glass, 215, -20, 305, -20, 330, 50, 215, 50);
            m.Box(346, 0, 0, 6, 70, 180, glass, .2f);
            m.Box(30, 155, 0, 680, 30, 200, m.C(0x4A5568), .1f);
            foreach (int x in new[] { -220, -90, 240 }) Wheels(m, x, 190, 54, 110, K, m.C(0xC8D0D8), 40);
        });
        // 乗り物 電車: 角の丸い車体、帯、窓、屋根のパンタグラフ、車輪とレール
        Icon(d, typeof(S994Parameter), (m, t, C, D) =>
        {
            m.Box(0, -10, 0, 650, 300, 250, C, .25f);
            m.Box(0, 75, 0, 654, 22, 254, D, .1f);
            for (int i = 0; i < 4; i++) SideWin(m, -225 + i * 150, -65, 96, 100, 126, glass);
            var g = m.C(0x4A5568);
            m.Tube([V3(-140, -160), V3(-60, -235), V3(40, -160)], 4, g);
            m.Box(-10, -238, 0, 180, 10, 60, g, .3f);
            foreach (int x in new[] { -210, -110, 110, 210 }) foreach (int s in Sides) m.Disc(x, 170, s * 100, 36, 36, 20, K, .3f);
            foreach (int s in Sides) m.Box(0, 210, s * 100, 720, 16, 20, m.C(0x8A95A0), .1f);
        });
        // 乗り物 高速列車: とがった鼻先の車体、青い帯、運転席と窓、レール
        Icon(d, typeof(S995Parameter), (m, t, C, D) =>
        {
            m.Slab(0, 220, C, -340, -95, 110, -95, 250, -30, 250, 120, -340, 120);
            foreach (int s in Sides)
            {
                m.Quad3D(new(250, -30, s * 110), new(345, 75, s * 20), new(345, 120, s * 20), new(250, 120, s * 110), C);
                m.Quad3D(new(250, 58, s * 111), new(345, 75, s * 21), new(345, 120, s * 21), new(250, 120, s * 111), D);
            }
            m.Quad3D(new(250, -30, 110), new(345, 75, 20), new(345, 75, -20), new(250, -30, -110), C);
            m.Quad3D(new(345, 75, 20), new(345, 120, 20), new(345, 120, -20), new(345, 75, -20), D);
            foreach (int s in Sides) m.Slab(s * 111, 4, D, -340, 55, 250, 58, 250, 120, -340, 120);
            var w = m.C(0x27364B);
            m.Quad3D(new(140, -88, 80), new(230, -40, 85), new(230, -40, -85), new(140, -88, -80), w);
            for (int i = 0; i < 4; i++) SideWin(m, -280 + i * 95, -30, 62, 54, 111, w);
            foreach (int s in Sides) m.Box(0, 145, s * 100, 720, 14, 20, m.C(0x8A95A0), .1f);
        });
        // 乗り物 飛行機: 上から見た丸い胴体、主翼と尾翼、立った垂直尾翼、窓
        Icon(d, typeof(S996Parameter), (m, t, C, D) => m.Turn(0, 0, -.2f, () =>
        {
            m.Ball(0, 0, 0, 310, 46, 46, C);
            m.Slab(0, 14, D, -80, -20, 125, -20, -110, -235, -195, -235);
            m.Slab(0, 14, D, -80, 20, 125, 20, -110, 235, -195, 235);
            m.Slab(0, 10, D, -300, -25, -215, -25, -310, -150, -345, -150);
            m.Slab(0, 10, D, -300, 25, -215, 25, -310, 150, -345, 150);
            m.Quad3D(new(-290, 0, 20), new(-210, 0, 30), new(-290, 0, 150), new(-330, 0, 150), D);
            for (int i = 0; i < 7; i++) m.Ball(-120 + i * 50, -14, 40, 8, 8, 6, m.C(0x27364B));
            m.Ball(255, -8, 22, 40, 22, 22, m.C(0x27364B));
        }));
        // 乗り物 ヘリコプター: 丸い胴、大きな窓、しっぽと回る後ろの羽、上で回る大きな羽、そり
        Icon(d, typeof(S997Parameter), (m, t, C, D) =>
        {
            var g = m.C(0xAEB7C2);
            m.Ball(40, 10, 0, 200, 130, 120, C);
            m.Ball(140, -20, 0, 100, 80, 104, D);
            m.Limb(-120, 5, 0, -345, -10, 0, 40, 16, C);
            m.Slab(0, 14, C, -350, -90, -300, -30, -345, -20);
            m.Turn(-345, -10, t * 22f, () => m.Limb(-345, -55, 18, -345, 35, 18, 5, 5, g));
            m.Box(0, -155, 0, 24, 60, 24, K, .3f);
            m.Yaw(0, -185, 0, t * 22f, () => { m.Limb(-310, -185, 0, 310, -185, 0, 6, 6, g); m.Limb(0, -185, -310, 0, -185, 310, 6, 6, g); });
            foreach (int s in Sides)
            {
                m.Limb(-90, 145, s * 80, 180, 145, s * 80, 6, 6, g);
                m.Limb(-20, 110, s * 50, -40, 145, s * 80, 5, 5, g); m.Limb(110, 110, s * 50, 130, 145, s * 80, 5, 5, g);
            }
        });
        // 乗り物 ヨット: 船体、マスト、白と黄色の帆、手前と奥でゆれる波
        Icon(d, typeof(S998Parameter), (m, t, C, D) =>
        {
            m.Slab(0, 150, D, -260, 120, 260, 120, 190, 235, -190, 235);
            m.Box(0, 118, 0, 500, 10, 130, C, .3f);
            m.Cyl(0, -88, 0, 9, 425, m.C(0x8A5A32), .2f);
            m.Tri3D(new(14, -285, 0), new(225, 90, 30), new(14, 90, 0), C);
            m.Tri3D(new(-14, -225, 0), new(-195, 90, 30), new(-14, 90, 0), m.C(0xFFD23F));
            foreach (int k in new[] { -1, 0, 1 }) m.Tube([.. Enumerable.Range(0, 41).Select(i => new Vector3(-340 + i * 17, 262 + MathF.Sin(i * .6f + t * 2f + k) * 10, k * 90))], 6, m.C(0x4FB8E8, k == 0 ? 1f : .7f));
        });
        // 乗り物 自転車: 回る車輪(リムとスポーク)、管のフレーム、左右にのびるハンドル、サドル、かご
        Icon(d, typeof(S999Parameter), (m, t, C, D) =>
        {
            var g = m.C(0xAEB7C2);
            foreach (int x in new[] { -185, 185 })
            {
                m.Torus(x, 110, 0, 118, 8, g);
                for (int i = 0; i < 6; i++) { float a = i * .5236f + t * 2f; m.Limb(x - MathF.Cos(a) * 108, 110 - MathF.Sin(a) * 108, 0, x + MathF.Cos(a) * 108, 110 + MathF.Sin(a) * 108, 0, 1.5f, 1.5f, m.C(0x8A95A0)); }
                m.Ball(x, 110, 0, 12, g);
            }
            m.Tube([V3(-185, 110), V3(-55, -80), V3(125, -80), V3(185, 110)], 8, C);
            m.Tube([V3(-55, -80), V3(5, 110), V3(125, -80)], 8, C);
            m.Limb(-185, 110, 0, 5, 110, 0, 8, 8, C);
            m.Limb(125, -80, 0, 105, -150, 0, 7, 7, C);
            m.Limb(105, -158, -80, 105, -158, 80, 7, 7, g);
            m.Box(-65, -115, 0, 100, 24, 50, K, .5f);
            m.Box(205, -120, 0, 120, 76, 100, D, .15f);
            m.Turn(5, 110, t * 2f, () => { m.Limb(5, 60, 0, 5, 160, 0, 4, 4, g); m.Box(5, 60, 30, 40, 10, 30, K, .3f); m.Box(5, 160, -30, 40, 10, 30, K, .3f); });
        });
        // 乗り物 気球: ふくらんだ球皮としまの布、ロープ、かご
        Icon(d, typeof(S1000Parameter), (m, t, C, D) =>
        {
            var prof = new List<Vector2> { new(0, -315) };
            for (int i = 1; i <= 12; i++) { float th = i / 12f * 1.95f; prof.Add(new(MathF.Sin(th) * 215, -100 - MathF.Cos(th) * 215)); }
            prof.Add(new(62, 190)); prof.Add(new(0, 190));
            m.Lathe(0, 0, C, prof);
            var band = prof.Select(v => v with { X = v.X * 1.01f + (v.X > 0 ? 1 : 0) }).ToList();
            for (int k = 0; k < 4; k++) { float a = k * MathF.PI / 2f; m.Lathe(0, 0, D, band, a - .2f, a + .2f); }
            foreach (int s in Sides) foreach (int s2 in Sides) m.Limb(s * 60, 190, s2 * 50, s * 48, 250, s2 * 45, 2.5f, 2.5f, K);
            m.Box(0, 285, 0, 120, 84, 120, m.C(0x9A6A3C), .12f);
        });
    }
}
