using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 214_QIcons2.cs のアイコン 851〜880(動物・鳥・海の生き物・虫 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    /// <summary>横向きの生き物の目(手前と奥の両側)</summary>
    static void SideEyes(Sculpt m, float x, float y, float zHalf, float r, bool white = false)
    {
        foreach (int s in Sides)
        {
            if (white) m.Ball(x, y, s * zHalf, r * 1.8f, r * 1.8f, r * .8f, m.C(0xFFFFFF));
            m.Ball(x, y, s * (zHalf + (white ? r * .5f : 0f)), r, r, r * .6f, m.C(0x20242B));
        }
    }

    /// <summary>細い脚(2本の点をつなぐ管)を左右(奥と手前)に</summary>
    static void Legs(Sculpt m, Vector4 col, float r, float z, params float[] xy)
    {
        foreach (int s in Sides) m.Tube([.. Enumerable.Range(0, xy.Length / 2).Select(i => new Vector3(xy[i * 2], xy[i * 2 + 1], s * z))], r, col);
    }

    static partial void RegisterI2b(Dictionary<Type, QuickModel> d)
    {
        var K = new Vector4(.125f, .14f, .17f, 1f);
        // しまうま: 胴・首・頭、黒いしま、4本の脚、たてがみ
        Icon(d, typeof(S851Parameter), (m, t, C, D) =>
        {
            m.Ball(-60, 80, 0, 215, 95, 95, C);
            m.Limb(140, 30, 0, 230, -170, 0, 55, 40, C);
            m.Ball(260, -190, 0, 95, 58, 55, C);
            for (int i = -4; i <= 4; i++) { float x = i * 46 - 60, rx = 215 * MathF.Sqrt(MathF.Max(0f, 1 - x / 215 * (x / 215))); m.Yaw(x, 80, 0, MathF.PI / 2f, () => m.Torus(x, 80, 0, 92 * MathF.Sqrt(MathF.Max(.05f, 1 - ((x + 60) / 215f) * ((x + 60) / 215f))), 8, D)); }
            foreach (int x in new[] { -210, -130, 30, 110 }) m.Limb(x, 140, x % 3 == 0 ? 45 : -45, x, 315, x % 3 == 0 ? 45 : -45, 17, 15, C);
            m.Tube([V3(105, -20), V3(170, -235), V3(240, -255)], 10, D);
            SideEyes(m, 255, -200, 50, 10);
            m.Ball(330, -175, 0, 22, 26, 30, D);
        });
        // ひつじ: もこもこの毛(球の集まり)、黒い顔と耳、脚
        Icon(d, typeof(S852Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 16;
            for (int i = 0; i < 13; i++) { float a = i * .483f; m.Ball(MathF.Cos(a) * 165, 10 + MathF.Sin(a) * 120, 40, 76, C); m.Ball(MathF.Cos(a) * 165, 10 + MathF.Sin(a) * 120, -40, 76, C); }
            m.Seg = keep;
            m.Ball(0, 10, 0, 185, 135, 130, C);
            foreach (int x in new[] { -110, -40, 40, 110 }) m.Limb(x, 150, x % 80 == 0 ? 50 : -50, x, 260, x % 80 == 0 ? 50 : -50, 16, 14, D);
            m.Ball(-190, -40, 0, 88, 78, 70, D);
            foreach (int s in Sides) m.Ball(-260, -65, s * 70, 40, 20, 20, D);
            foreach (int s in Sides) { m.Ball(-215, -55, s * 62, 13, 13, 8, m.C(0xFFFFFF)); m.Ball(-218, -55, s * 68, 6, K); }
        });
        // にわとり: 白い体と頭、赤いとさかと肉垂、くちばし、尾羽、脚
        Icon(d, typeof(S853Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 60, 0, 200, 165, 140, C);
            m.Ball(-125, -120, 0, 110, C);
            for (int i = 0; i < 3; i++) m.Ball(-175 + i * 50, -225 - i % 2 * 16, 0, 34, 34, 20, D);
            m.Limb(-225, -110, 0, -310, -95, 0, 22, 2, m.C(0xF2B134));
            m.Ball(-225, -60, 0, 20, 40, 14, D);
            SideEyes(m, -155, -140, 90, 14);
            m.Slab(0, 60, C, 170, 40, 300, -130, 320, 10, 230, 130);
            Legs(m, m.C(0xF2B134), 5, 50, -40, 215, -50, 300); Legs(m, m.C(0xF2B134), 5, 50, 60, 215, 70, 300);
        });
        // 黄色いひよこ: 丸い体と頭、くちばし、羽、頭の毛、細い脚
        Icon(d, typeof(S854Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 80, 0, 190, 165, 160, C);
            m.Ball(-20, -95, 0, 125, C);
            m.Limb(-148, -82, 0, -225, -82, 0, 26, 2, D);
            SideEyes(m, -65, -125, 105, 15);
            foreach (int s in Sides) m.Ball(50, 70, s * 150, 86, 52, 20, m.C(0xFFCC2E));
            m.Slab(0, 14, C, -15, -220, 10, -275, 28, -210);
            Legs(m, D, 5, 40, -45, 235, -55, 300); Legs(m, D, 5, 40, 55, 235, 65, 300);
        });
        // 立ちペンギン: 背中の黒と白いおなか、羽、目、くちばし、足
        Icon(d, typeof(S855Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 30, 0, 190, 270, 165, C);
            m.Ball(0, 85, Sculpt.FrontZ(0, 30, 0, 190, 270, 165, 0, 85) - 55, 125, 195, 60, D);
            foreach (int s in Sides) m.Turn(s * 190, -60, s * .2f, () => m.Ball(s * 195, 50, -10, 44, 135, 50, C));
            HeadEyes(m, 0, 30, 0, 190, 270, 165, 60, -145, 20);
            m.Limb(0, -95, Sculpt.FrontZ(0, 30, 0, 190, 270, 165, 0, -95) - 10, 0, -85, Sculpt.FrontZ(0, 30, 0, 190, 270, 165, 0, -95) + 45, 30, 2, m.C(0xFFA726));
            foreach (int s in Sides) m.Ball(s * 70, 296, 60, 68, 24, 70, m.C(0xFFA726));
        });
        // ふくろう: 耳の羽、大きな目(白・黒・光)、胸の模様、くちばし
        Icon(d, typeof(S856Parameter), (m, t, C, D) =>
        {
            foreach (int s in Sides) m.Slab(-30, 50, C, s * 170, -160, s * 155, -280, s * 55, -205);
            m.Ball(0, 30, 0, 205, 265, 170, C);
            m.Ball(0, 120, Sculpt.FrontZ(0, 30, 0, 205, 265, 170, 0, 120) - 50, 125, 150, 55, D);
            for (int y = 0; y < 3; y++) for (int x = -1; x <= 1; x++) m.Tube(Sculpt.OnFront(0, 120, 115, 125, 150, 55, 1f, Sculpt.ArcPts(x * 62 + (y % 2) * 20, 70 + y * 62, 24, .4f, 2.7f, 6)), 2.5f, C);
            foreach (int s in Sides) { float ez = Sculpt.FrontZ(0, 30, 0, 205, 265, 170, s * 82, -85); m.Ball(s * 82, -85, ez - 20, 76, 76, 36, m.C(0xFFFFFF)); m.Ball(s * 82, -85, ez + 12, 36, 36, 12, K); m.Ball(s * 70, -98, ez + 20, 11, 11, 4, m.C(0xFFFFFF)); }
            m.Limb(0, -50, Sculpt.FrontZ(0, 30, 0, 205, 265, 170, 0, -40), 0, -5, Sculpt.FrontZ(0, 30, 0, 205, 265, 170, 0, -40) + 25, 22, 2, m.C(0xF2B134));
        });
        // すずめ: 丸い体と頭、白いほお、くちばし、つばさと尾、脚
        Icon(d, typeof(S857Parameter), (m, t, C, D) =>
        {
            m.Ball(30, 70, 0, 205, 140, 125, C);
            m.Ball(-130, -55, 0, 105, C);
            foreach (int s in Sides) m.Ball(-105, 5, s * 60, 90, 62, 45, D);
            m.Limb(-228, -48, 0, -290, -48, 0, 18, 2, K);
            SideEyes(m, -165, -75, 85, 14);
            foreach (int s in Sides) m.Slab(s * 110, 20, m.C(0x7A5235), 40, 10, 295, -40, 190, 105);
            m.Slab(0, 30, m.C(0x7A5235), 205, 85, 340, 5, 330, 70);
            Legs(m, K, 4, 40, -10, 205, -15, 285); Legs(m, K, 4, 40, 70, 205, 75, 285);
        });
        // カメ: もり上がった甲羅と模様、頭、4本の脚、しっぽ
        Icon(d, typeof(S858Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, [.. Dome(235, 160, 110, 14), new Vector2(235, 116), new Vector2(0, 116)]);
            for (int i = 0; i < 5; i++) m.Slab(Sculpt.FrontZ(0, 110, 0, 235, 160, 235, -130 + i * 65, 10 + (i % 2) * 30), 6, m.C(0x2F7A42), StarPts(-130 + i * 65, 10 + (i % 2) * 30, 42, 6, .82f, 0));
            m.Cyl(0, 112, 0, 236, 16, m.C(0x2F7A42), .2f);
            m.Ball(-255, 20, 0, 76, 62, 60, D);
            SideEyes(m, -285, 5, 50, 10);
            foreach (int x in new[] { -130, 130 }) foreach (int s in Sides) m.Ball(x, 145, s * 150, 64, 44, 50, D);
            m.Limb(210, 70, 0, 300, 75, 0, 20, 4, D);
        });
        // アマガエル: 上に飛び出た目、平たい体、白いのど、口、ほっぺ
        Icon(d, typeof(S859Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 50, 0, 250, 175, 170, C);
            foreach (int s in Sides) { m.Ball(s * 115, -120, 20, 72, C); m.Ball(s * 115, -120, 70, 46, 46, 36, m.C(0xFFFFFF)); m.Ball(s * 115, -118, 102, 24, 24, 10, K); }
            m.Ball(0, 120, Sculpt.FrontZ(0, 50, 0, 250, 175, 170, 0, 120) - 40, 165, 95, 50, D);
            m.Tube(Sculpt.OnFront(0, 50, 0, 250, 175, 170, 1f, Sculpt.ArcPts(0, 20, 155, .35f, 2.8f, 14)), 3.5f, K);
            foreach (int s in Sides) m.Ball(s * 145, 45, Sculpt.FrontZ(0, 50, 0, 250, 175, 170, s * 145, 45) - 2, 28, 16, 5, m.C(0xFF9EAD, .7f));
        });
        // ヘビ: くねくね動く太い体(管)、頭、目、ちろちろ出る舌
        Icon(d, typeof(S860Parameter), (m, t, C, D) =>
        {
            var q = new List<Vector3>();
            for (int i = 0; i <= 44; i++) { float x = -310 + i * 12; q.Add(new(x, 100 + MathF.Sin(i * .28f + t * 2f) * 75, MathF.Cos(i * .2f + t) * 60)); }
            m.Tube(q, 24, C);
            var h = q[^1];
            m.Ball(h.X + 18, h.Y, h.Z, 60, 42, 45, C);
            foreach (int s in Sides) m.Ball(h.X + 30, h.Y - 16, h.Z + s * 35, 8, K);
            m.Tube([new(h.X + 70, h.Y, h.Z), new(h.X + 115, h.Y, h.Z), new(h.X + 135, h.Y - 15, h.Z)], 3, D);
            m.Limb(h.X + 115, h.Y, h.Z, h.X + 135, h.Y + 15, h.Z, 3, 3, D);
        });
        // 魚: ふくらんだ体、うろこ、えら、尾びれと背びれ、目
        Icon(d, typeof(S861Parameter), (m, t, C, D) =>
        {
            m.Ball(-30, 0, 0, 235, 135, 85, C);
            m.Slab(0, 14, D, 165, 0, 315, -120, 285, 0, 315, 120);
            m.Slab(0, 12, D, -30, -115, 70, -205, 110, -90);
            m.Tube(Sculpt.OnFront(-30, 0, 0, 235, 135, 85, 1f, Sculpt.ArcPts(-125, 0, 92, -1.2f, 1.2f, 10)), 4, m.C(0x2F6FBB));
            for (int y = -1; y <= 1; y++) for (int x = 0; x < 3; x++) m.Tube(Sculpt.OnFront(-30, 0, 0, 235, 135, 85, 1f, Sculpt.ArcPts(-20 + x * 62, y * 52, 28, -1.1f, 1.1f, 6)), 2.5f, m.C(0x2F6FBB));
            SideEyes(m, -175, -30, 60, 11, true);
        });
        // 金魚: 丸い体、ひらひらゆれる大きな尾びれ、背びれ、目
        Icon(d, typeof(S862Parameter), (m, t, C, D) =>
        {
            float w0 = MathF.Sin(t * 3f) * 26;
            m.Yaw(80, 0, 0, MathF.Sin(t * 3f) * .4f, () => m.Slab(0, 6, m.C(C, .85f), 80, 0, 300, -190 + w0, 240, 0, 300, 190 + w0));
            m.Ball(-40, 0, 0, 175, 125, 95, C);
            m.Ball(-70, 50, 50, 90, 55, 40, D);
            m.Slab(0, 10, C, -20, -115, 90, -190, 95, -80);
            SideEyes(m, -125, -35, 70, 11, true);
        });
        // クジラ: 大きな体と白いおなか、尾びれ、胸びれ、目、吹き上げる潮
        Icon(d, typeof(S863Parameter), (m, t, C, D) =>
        {
            m.Ball(-40, 10, 0, 280, 150, 140, C);
            m.Ball(-40, 70, 0, 250, 90, 125, D);
            m.Limb(200, -20, 0, 290, -40, 0, 70, 30, C);
            m.Pitch(310, -40, 0, MathF.PI / 2f, () => m.Slab(-40, 14, C, 290, 0, 350, -110, 340, 0, 350, 110));
            foreach (int s in Sides) m.Yaw(-60, 90, s * 120, s * .5f, () => m.Ball(-60, 110, s * 130, 80, 22, 40, C));
            SideEyes(m, -180, -20, 125, 14);
            int keep = m.Seg; m.Seg = 10;
            for (int i = -2; i <= 2; i++) { float k = Fr(t * .8f + MathF.Abs(i) * .08f); m.Ball(-70 + i * k * 42, -155 - k * 120, (R01(i + 5) - .5f) * k * 80, 10, m.C(D, 1f - k * .8f)); }
            m.Seg = keep;
        });
        // イルカ: 流線形の体と明るいおなか、くちばし、背びれ、尾びれ、胸びれ
        Icon(d, typeof(S864Parameter), (m, t, C, D) => m.Turn(0, 0, -.35f, () =>
        {
            m.Ball(-20, -5, 0, 250, 95, 90, C);
            m.Ball(-40, 40, 0, 220, 50, 75, D);
            m.Limb(-240, -10, 0, -330, -20, 0, 30, 14, C);
            m.Limb(200, -10, 0, 290, -40, 0, 45, 18, C);
            m.Pitch(300, -40, 0, MathF.PI / 2f, () => m.Slab(-40, 12, C, 280, 0, 345, -100, 330, 0, 345, 100));
            m.Slab(0, 16, C, -60, -95, 20, -200, 70, -85);
            foreach (int s in Sides) m.Yaw(-60, 60, s * 80, s * .6f, () => m.Ball(-60, 70, s * 90, 60, 16, 30, C));
            SideEyes(m, -200, -35, 75, 10);
        }));
        // サメ: とがった体と白いおなか、大きな背びれ、胸びれ、尾びれ、歯
        Icon(d, typeof(S865Parameter), (m, t, C, D) =>
        {
            m.Ball(-10, 10, 0, 300, 110, 100, C);
            m.Ball(-60, 60, 0, 260, 60, 85, D);
            m.Limb(250, 0, 0, 300, 0, 0, 50, 20, C);
            m.Slab(0, 16, C, 290, 0, 340, -120, 335, 20, 350, 135);
            m.Slab(0, 22, C, -20, -85, 50, -220, 120, -75);
            foreach (int s in Sides) m.Yaw(-20, 95, s * 90, s * .5f, () => m.Slab(s * 90, 14, C, -20, 95, 70, 190, 90, 85));
            SideEyes(m, -215, -25, 75, 10);
            for (int i = 0; i < 6; i++) m.Slab(0, 140, m.C(0xFFFFFF), -300 + i * 28, 42, -286 + i * 28, 62, -272 + i * 28, 48);
        });
        // タコ: 丸い頭、ゆれる8本の足(吸盤)、目、口
        Icon(d, typeof(S866Parameter), (m, t, C, D) =>
        {
            for (int i = 0; i < 8; i++)
            {
                float a = i * MathF.Tau / 8f, ca = MathF.Cos(a), sa = MathF.Sin(a), dx = MathF.Sin(t * 2f + i) * 22;
                m.Tube([V3(ca * 80, 40, sa * 80), V3(ca * 170, 155, sa * 150), V3(ca * 200 + dx, 270, sa * 180)], 17, C);
                m.Ball(ca * 200 + dx, 275, sa * 180, 17, C);
                m.Ball(ca * 150, 145, sa * 135 + 14, 6, D);
            }
            m.Ball(0, -85, 0, 200, 195, 180, C);
            HeadEyes(m, 0, -85, 0, 200, 195, 180, 70, -55, 24);
            m.Limb(0, 35, 130, 0, 35, 190, 28, 22, D);
            m.Ball(-65, -185, Sculpt.FrontZ(0, -85, 0, 200, 195, 180, -65, -185), 34, 15, 6, m.C(0xFFFFFF, .4f));
        });
        // イカ: とがった頭(ひれ)と胴、10本の足、目、ぶつぶつ
        Icon(d, typeof(S867Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 0, -300, 60, -230, 110, -60, 105, 70, 0, 72);
            m.Slab(0, 14, C, 0, -300, 175, -115, 60, -150);
            m.Slab(0, 14, C, 0, -300, -175, -115, -60, -150);
            for (int i = -4; i <= 4; i++) { float a = i * .3f, dx = MathF.Sin(t * 2f + i) * 16; m.Limb(MathF.Sin(a) * 60, 60, MathF.Cos(a) * 60, i * 44 + dx, 280, MathF.Cos(a) * 90, 10, 6, C); }
            foreach (int s in Sides) m.Limb(s * 65, 60, 0, s * 185, 300, 0, 6, 4, C);
            HeadEyes(m, 0, -20, 0, 110, 100, 105, 52, 5, 18);
            for (int i = 0; i < 7; i++) m.Ball(-60 + i * 20, -120 + i % 2 * 30, RadiusAt([0, -300, 60, -230, 110, -60, 105, 70], -120 + i % 2 * 30) * .95f, 5, D);
        });
        // カニ: 平たい甲羅とおなか、はさみ(開いたり閉じたり)、6本の脚、飛び出た目
        Icon(d, typeof(S868Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 60, 0, 195, 110, 130, C);
            m.Ball(0, 110, 70, 120, 50, 60, D);
            foreach (int s in Sides)
            {
                for (int i = 0; i < 3; i++) m.Tube([V3(s * 130, 75 + i * 25, -30 + i * 30), V3(s * 260, 105 + i * 35, -60 + i * 50), V3(s * 300, 175 + i * 35, -80 + i * 60)], 7, C);
                m.Tube([V3(s * 160, -10, 30), V3(s * 260, -90, 40), V3(s * 235, -155, 40)], 11, C);
                float o = MathF.Sin(t * 5f) * .1f;
                m.Turn(s * 235, -190, s * o, () => { m.Ball(s * 235, -190, 40, 72, 50, 40, C); m.Limb(s * 235, -190, 40, s * 260, -270, 40, 24, 8, C); });
                m.Limb(s * 48, -50, 40, s * 52, -120, 40, 5, 5, C);
                m.Ball(s * 52, -125, 40, 20, m.C(0xFFFFFF)); m.Ball(s * 52, -128, 58, 10, 10, 4, K);
            }
        });
        // エビ: 丸まった体の節、扇形の尾、目、長いひげ
        Icon(d, typeof(S869Parameter), (m, t, C, D) =>
        {
            for (int i = 0; i < 7; i++) { float a = -1.2f + i * .42f; m.Ball(MathF.Cos(a) * 165 - 30, MathF.Sin(a) * 165, 0, 58 - i * 3, i % 2 == 0 ? C : m.C(0xE86A40)); }
            m.Slab(0, 10, C, 10, 165, -130, 175, -75, 250); m.Slab(0, 10, C, 10, 165, -60, 275, 40, 270);
            SideEyes(m, 35, -155, 40, 12);
            m.Tube([V3(70, -155, 10), V3(190, -235, 30), V3(300, -190, 50)], 2, C);
            m.Tube([V3(70, -140, -10), V3(210, -185, -30), V3(320, -110, -50)], 2, C);
            for (int i = 0; i < 5; i++) { float a = -.6f + i * .3f; m.Limb(MathF.Cos(a) * 120 - 30, MathF.Sin(a) * 120, 0, MathF.Cos(a) * 80 - 30, MathF.Sin(a) * 80 + 40, 0, 4, 3, D); }
        });
        // 貝がら: 扇形にふくらんだ貝(すじ)、ふちの波、合わせ目
        Icon(d, typeof(S870Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 18;
            for (int i = -4; i <= 4; i++)
            {
                float a = 4.71f + i * .26f;
                m.Limb(0, 190, 0, MathF.Cos(a) * 320, 190 + MathF.Sin(a) * 320, 0, 20, 42, C);
                m.Limb(0, 190, 30, MathF.Cos(a) * 320, 190 + MathF.Sin(a) * 320, 32, 3, 4, D);
            }
            m.Seg = keep;
            m.Box(0, 205, 0, 110, 52, 50, D, .5f);
        });
        // ヒトデ: 厚みのある星形と、腕のぶつぶつ
        Icon(d, typeof(S871Parameter), (m, t, C, D) =>
        {
            for (int k = 0; k < 5; k++) { float a = -1.57f + k * 1.2566f; m.Limb(0, 0, 0, MathF.Cos(a) * 240, MathF.Sin(a) * 240, 0, 80, 20, C); }
            m.Ball(0, 0, 0, 110, 110, 70, C);
            for (int k = 0; k < 5; k++) { float a = -1.57f + k * 1.2566f; for (int i = 1; i < 5; i++) { float r = 80 - i * 14; m.Ball(MathF.Cos(a) * i * 46, MathF.Sin(a) * i * 46, MathF.Min(r, 70) * .85f, 9, D); } }
            m.Ball(0, 0, 70, 17, D);
        });
        // クラゲ: 透けたかさ(ドーム)、ゆらゆらゆれる触手、目
        Icon(d, typeof(S872Parameter), (m, t, C, D) =>
        {
            int keep = m.Seg; m.Seg = 10;
            for (int i = 0; i < 8; i++)
            {
                float a = i * MathF.Tau / 8f;
                var q = new List<Vector3>();
                for (int j = 0; j <= 14; j++) q.Add(new(MathF.Cos(a) * 120 + MathF.Sin(j * .6f + t * 2f + i) * 18, 10 + j * 20, MathF.Sin(a) * 120));
                m.Tube(q, 4, m.C(D, .85f));
            }
            m.Seg = keep;
            m.Lathe(0, 0, m.C(C, .85f), [.. Dome(195, 195, 20, 14), new Vector2(195, 24), new Vector2(0, 24)]);
            m.Ball(-70, -80, Sculpt.FrontZ(0, 20, 0, 195, 195, 195, -70, -80), 48, 24, 8, m.C(0xFFFFFF, .5f));
            HeadEyes(m, 0, 20, 0, 195, 195, 195, 50, -30, 13);
        });
        // タツノオトシゴ: 丸い胸、頭と口先、巻いたしっぽ、背びれ
        Icon(d, typeof(S873Parameter), (m, t, C, D) =>
        {
            m.Ball(0, -70, 0, 95, 170, 80, C);
            m.Ball(-55, -205, 0, 88, 62, 62, C);
            m.Limb(-120, -200, 0, -235, -205, 0, 26, 14, C);
            SideEyes(m, -60, -220, 55, 12);
            m.Tube([.. Sculpt.ArcPts(35, 160, 75, -1.57f, 3.3f, 16).Select(q => new Vector3(q, 0))], 22, C);
            m.Limb(0, 70, 0, 35, 85, 0, 50, 25, C);
            for (int i = 0; i < 6; i++) m.Slab(0, 10, D, 70, -200 + i * 50, 135, -180 + i * 50, 75, -160 + i * 50);
        });
        // ちょうちょ: はばたく4枚の羽(模様)、体、触角
        Icon(d, typeof(S874Parameter), (m, t, C, D) =>
        {
            float f = MathF.Abs(MathF.Sin(t * 4f)) * .9f;
            foreach (int s in Sides) m.Yaw(0, 0, 0, s * f, () =>
            {
                m.Ball(s * 150, -85, 0, 145, 125, 6, C); m.Ball(s * 120, 95, 0, 95, 85, 6, C);
                m.Ball(s * 175, -95, 4, 42, 42, 5, D); m.Ball(s * 125, 105, 4, 28, 28, 5, D);
            });
            var B0 = m.C(0x6B4428);
            m.Super(0, 10, 0, 15, 145, 15, .8f, 1f, B0);
            foreach (int s in Sides) m.Tube([V3(s * 5, -130), V3(s * 50, -225, 10), V3(s * 80, -235, 20)], 2.5f, B0);
        });
        // てんとう虫: 半球の赤い背中と黒い点、黒い頭、目、背中の線
        Icon(d, typeof(S875Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 20, 0, 225, 225, 130, C);
            m.Ball(0, -175, 30, 115, 80, 85, D);
            m.Tube(Sculpt.OnFront(0, 20, 0, 225, 225, 130, 1f, [new(0, -85), new(0, 80), new(0, 245)]), 4, D);
            foreach (var (x, y) in new[] { (-110f, -20f), (110f, -20f), (-125f, 100f), (125f, 100f), (-55f, 185f), (55f, 185f) }) m.Ball(x, y, Sculpt.FrontZ(0, 20, 0, 225, 225, 130, x, y) - 4, 34, 34, 10, D);
            HeadEyes(m, 0, -175, 30, 115, 80, 85, 50, -190, 13);
        });
        // はたらきバチ: しまの体、はばたく透明な羽、針、目
        Icon(d, typeof(S876Parameter), (m, t, C, D) =>
        {
            float w0 = MathF.Sin(t * 30f) * .3f;
            foreach (int s in Sides) m.Pitch(0, -60, 0, s * (.4f + w0), () => m.Ball(s * 40, -155, 0, 85, 125, 6, m.C(0xDFF4FF, .75f)));
            m.Ball(20, 40, 0, 225, 145, 140, C);
            for (int i = 0; i < 3; i++) { float x = -40 + i * 90, r = MathF.Sqrt(MathF.Max(.05f, 1 - ((x - 20) / 225f) * ((x - 20) / 225f))); m.Yaw(x, 40, 0, MathF.PI / 2f, () => m.Torus(x, 40, 0, 135 * r, 18, D)); }
            m.Limb(240, 40, 0, 310, 40, 0, 20, 2, D);
            SideEyes(m, -145, 10, 110, 18);
        });
        // カブトムシ: つやのある背中(はね)と胸、長い角、6本の脚
        Icon(d, typeof(S877Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 100, 0, 160, 205, 110, C);
            m.Tube(Sculpt.OnFront(0, 100, 0, 160, 205, 110, 1f, [new(0, -10), new(0, 120), new(0, 300)]), 2.5f, D);
            m.Ball(0, -115, 0, 125, 80, 90, C);
            m.Tube([V3(0, -165, 40), V3(0, -265, 60), V3(-50, -315, 60)], 14, C);
            m.Limb(0, -265, 60, 50, -315, 60, 10, 6, C);
            foreach (int s in Sides) for (int i = 0; i < 3; i++) m.Tube([V3(s * 110, -30 + i * 90, -20), V3(s * 235, -60 + i * 105, -40), V3(s * 265, 40 + i * 105, -60)], 6, C);
            m.Ball(-60, 70, Sculpt.FrontZ(0, 100, 0, 160, 205, 110, -60, 70), 18, 88, 6, m.C(0xFFFFFF, .18f));
        });
        // クワガタ: 平たい背中と胸、大きなあご(はさみ)、6本の脚
        Icon(d, typeof(S878Parameter), (m, t, C, D) =>
        {
            m.Ball(0, 110, 0, 150, 190, 85, C);
            m.Tube(Sculpt.OnFront(0, 110, 0, 150, 190, 85, 1f, [new(0, 20), new(0, 160), new(0, 300)]), 2.5f, D);
            m.Ball(0, -85, 0, 120, 72, 70, C);
            foreach (int s in Sides)
            {
                m.Tube([.. Sculpt.ArcPts(s * 55, -200, 105, s > 0 ? 1.4f : -1.75f, s > 0 ? -1.5f : 1.72f, 12).Select(q => new Vector3(q, 20))], 13, C);
                for (int i = 0; i < 3; i++) m.Tube([V3(s * 100, -15 + i * 90, -20), V3(s * 225, -50 + i * 105, -40), V3(s * 260, 45 + i * 105, -60)], 5, C);
            }
        });
        // カマキリ: 細長い体と胸、三角の頭と大きな目、かまの前脚、脚
        Icon(d, typeof(S879Parameter), (m, t, C, D) =>
        {
            m.Ball(70, 140, 0, 190, 55, 60, C);
            m.Limb(-40, 100, 0, -145, -75, 0, 16, 12, C);
            m.Slab(0, 40, C, -205, -175, -110, -145, -155, -80);
            foreach (int s in Sides) { m.Ball(-180, -155, s * 22, 18, K); }
            m.Ball(-120, -145, 0, 18, K);
            foreach (int s in Sides) { m.Tube([V3(-120, -40, s * 30), V3(-20, -120, s * 40), V3(-85, -200, s * 40)], 9, D); }
            foreach (int x in new[] { 25, 155 }) Legs(m, C, 5, 50, x, 160, x + 45, 235, x + 15, 300);
            m.Limb(250, 140, 0, 325, 110, 0, 4, 2, C);
        });
        // トンボ: 長い体、大きな目、はばたく4枚の透明な羽
        Icon(d, typeof(S880Parameter), (m, t, C, D) =>
        {
            float w0 = MathF.Sin(t * 26f) * .25f;
            foreach (int s in Sides)
            {
                m.Pitch(0, -60, 0, s * (.3f + w0), () => m.Turn(0, -60, s * .28f, () => m.Ball(s * 165, -70, 0, 160, 34, 4, m.C(D, .72f))));
                m.Pitch(0, -15, 0, s * (.2f - w0), () => m.Turn(0, -15, -s * .18f, () => m.Ball(s * 155, 5, 0, 145, 30, 4, m.C(D, .72f))));
            }
            m.Limb(0, -20, 0, 0, 270, 0, 14, 8, C);
            m.Ball(0, -45, 0, 34, 64, 34, C);
            foreach (int s in Sides) m.Ball(s * 20, -115, 10, 24, K);
        });
    }
}
