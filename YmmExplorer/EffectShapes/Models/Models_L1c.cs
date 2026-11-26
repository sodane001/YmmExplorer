using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 222番 2401〜2500(Q6Art の種類 54〜63)
//   キッチン・文房具・スイーツ・家の小物・数字ブロック・季節の物・状態アイコン・惑星・スタンプ・お金
// =====================================================================
internal static partial class Models3D
{
    static void Kitchen3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = Wt(C); var metal = m.C(0xB8C0CC);
        switch (a)
        {
            case 0:
                m.Lathe(0, 0, C, 0, -130, 160, -130, 160, 210, 0, 210); m.Lathe(0, 0, m.C(0x6D4C41), 0, -118, 145, -118, 145, -110, 0, -110);
                m.Torus(170, 40, 0, 70, 22, D);
                for (int i = -1; i <= 1; i++) m.Tube(Enumerable.Range(0, 13).Select(k => V3(i * 70 + MathF.Sin(k * .6f + t * 3f + i) * 14, -160 - k * 14, 0)).ToList(), 5, new Vector4(1, 1, 1, .4f * C.W));
                break;
            case 1:
                m.Lathe(0, 0, D, 0, 165, 300, 160, 300, 180, 0, 180);
                m.Lathe(0, 0, C, 200, -60, 180, 40, 150, 130, 0, 130); m.Lathe(0, 0, m.C(0x8D5A3B), 0, -50, 170, -50, 170, -40, 0, -40);
                m.Torus(195, 20, 0, 55, 16, C); break;
            case 2: m.Pitch(0, 0, 0, -.9f, () => { m.Lathe(0, 0, W, 0, 20, 210, 15, 300, -15, 300, -5, 210, 30, 0, 30); m.Pitch(0, 0, 0, MathF.PI / 2f, () => m.Torus(0, 0, 0, 215, 3, D)); }); break;
            case 3: m.Lathe(0, 0, C, 260, -40, 240, 80, 160, 190, 0, 200); m.Lathe(0, 0, m.C(0xF2C14E), 0, -20, 230, -20, 230, -10, 0, -10); break;
            case 4: m.Pitch(0, 0, 0, -.9f, () => { m.Box(250, 0, 0, 180, 40, 40, m.C(0x5D4037), .5f); m.Cyl(0, 20, 0, 200, 40, m.C(0x2B2F36), .2f); m.Ball(-60, 0, -40, 50, 10, 30, m.C(0xFFF59D)); m.Ball(-60, -6, -40, 22, m.C(0xFFB300)); }); break;
            case 5:
                m.Cyl(0, 60, 0, 210, 260, C, .1f); m.Box(-250, 0, 0, 80, 30, 40, D, .4f); m.Box(250, 0, 0, 80, 30, 40, D, .4f);
                m.Lathe(0, 0, Sculpt.Mix(C, W, .2f), 0, -110, 120, -100, 225, -70, 0, -70); m.Ball(0, -130, 0, 35, 20, 35, D); break;
            case 6:
                m.Lathe(0, 0, C, 0, -110, 120, -90, 220, 40, 230, 160, 0, 170);
                m.Limb(170, 60, 0, 300, -40, 0, 26, 12, C); m.Tube([.. ArcPts3(0, -80, 160, MathF.PI, MathF.Tau, 0)], 10, D); m.Ball(0, -110, 0, 30, D); break;
            case 7: m.Box(0, 110, 0, 50, 300, 24, metal, .8f); m.Box(0, -100, 0, 150, 60, 20, metal, .5f); for (int i = -1; i <= 1; i++) m.Box(i * 50, -200, 0, 22, 160, 16, metal, .8f); m.Box(0, 230, 0, 54, 100, 30, C, .8f); break;
            case 8: m.Box(0, 120, 0, 50, 300, 24, metal, .8f); m.Ball(0, -150, 0, 100, 140, 40, metal); m.Box(0, 240, 0, 54, 100, 30, C, .8f); break;
            default: m.Slab(0, 16, metal, -40, -300, 40, -300, 40, 60, 0, 60); m.Box(0, 170, 0, 70, 230, 40, C, .6f); break;
        }
    }

    static void Stationery3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = Wt(C); var metal = m.C(0xB8C0CC); var wood = m.C(0xF2D3A2); var K = m.C(0x20242B);
        m.Turn(0, 0, a <= 1 ? -.6f + MathF.Sin(t) * .05f : MathF.Sin(t * .8f) * .04f, () =>
        {
            switch (a)
            {
                case 0: m.Lathe(0, 0, C, 35, -210, 35, 210); m.Lathe(0, 0, wood, 0, -290, 35, -210); m.Lathe(0, 0, K, 0, -300, 10, -286); m.Cyl(0, 225, 0, 37, 30, metal); m.Cyl(0, 265, 0, 35, 50, D, .4f); break;
                case 1: m.Cyl(0, 20, 0, 23, 440, C, .9f); m.Lathe(0, 0, Sculpt.Mix(C, W, .4f), 5, -280, 23, -200); m.Box(28, -120, 20, 14, 160, 10, metal, .4f); break;
                case 2: m.Box(0, 0, 0, 360, 170, 120, W, .2f); m.Box(50, 0, 0, 220, 176, 126, C, .05f); break;
                case 3:
                    m.Box(0, 0, 0, 640, 110, 12, new Vector4(C.X, C.Y, C.Z, .85f * C.W), .05f);
                    for (int i = 0; i <= 30; i++) m.Box(-300 + i * 20, i % 5 == 0 ? -32 : -42, 8, i % 10 == 0 ? 4 : 2, i % 5 == 0 ? 45 : 25, 2, K, .1f);
                    break;
                case 4:
                    m.Turn(0, 0, MathF.Sin(t * 3f) * .12f, () => m.Slab(4, 6, metal, 0, 0, 20, -20, 260, -60, 270, -40));
                    m.Turn(0, 0, -MathF.Sin(t * 3f) * .12f, () => m.Slab(-4, 6, metal, 0, 0, 20, 20, 260, 60, 270, 40));
                    m.Torus(-120, -70, 0, 60, 14, C); m.Torus(-120, 70, 0, 60, 14, C); m.Ball(0, 0, 0, 14, D); break;
                case 5:
                    {
                        var q = new List<Vector3> { V3(60, 200), V3(60, -150) };
                        q.AddRange(ArcPts3(0, -150, 60, 0, -MathF.PI, 0, 12)); q.Add(V3(-60, 160)); q.AddRange(ArcPts3(-15, 160, 45, MathF.PI, 0, 0, 10)); q.Add(V3(30, -110));
                        m.Tube(q, 8, C); break;
                    }
                case 6: m.Box(0, 60, 0, 460, 120, 120, D, .3f); m.Turn(-200, 30, -.08f - MathF.Max(0f, MathF.Sin(t * 3f)) * .08f, () => m.Box(0, -20, 0, 480, 90, 110, C, .5f)); break;
                case 7:
                    m.Box(0, 0, 0, 400, 500, 30, C, .05f); m.Box(30, 0, 18, 330, 470, 4, W, .02f);
                    for (int i = 0; i < 7; i++) m.Box(30, -170 + i * 60, 21, 280, 3, 1, new Vector4(C.X, C.Y, C.Z, .4f * C.W), .1f);
                    for (int i = 0; i < 8; i++) m.Yaw(-170, -210 + i * 60, 0, MathF.PI / 2f, () => m.Torus(-170, -210 + i * 60, 0, 24, 5, metal));
                    break;
                case 8: m.Limb(0, 0, 0, 0, 0, -160, 5, 2, metal); m.Lathe(0, 0, C, 0, -10, 150, -10, 150, 20, 80, 40, 80, 100, 0, 100); break;
                default: m.Slab(0, 20, new Vector4(C.X, C.Y, C.Z, .7f * C.W), -250, 230, 250, 230, -250, -270); m.Slab(12, 4, new Vector4(1, 1, 1, .3f * C.W), -170, 170, 80, 170, -170, -100); break;
            }
        });
    }

    static void Sweets3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = Wt(C); var dough = m.C(0xD9A066);
        float hop = MathF.Max(0f, MathF.Sin(t * 2.4f)) * 10f;
        m.Move(0, -hop, () =>
        {
            switch (a)
            {
                case 0: m.Slab(0, 40, D, -150, 0, -290, -90, -270, 0, -290, 90); m.Slab(0, 40, D, 150, 0, 290, -90, 270, 0, 290, 90); m.Ball(0, 0, 0, 170, 120, 110, C); break;
                case 1:
                    m.Cyl(0, 200, 0, 13, 300, W); m.Disc(0, -70, 0, 190, 190, 50, C, .5f);
                    m.Tube(Enumerable.Range(0, 61).Select(k => { float b = k * .35f + t, r = k * 3f; return V3(MathF.Cos(b) * r, -70 + MathF.Sin(b) * r, 26); }).ToList(), 10, D);
                    break;
                case 2:
                    m.Torus(0, 0, 0, 150, 70, dough); m.Torus(0, 0, 20, 150, 56, C);
                    for (int i = 0; i < 18; i++) { float b = i * 1.7f, r = 120 + (i % 3) * 25; m.Turn(MathF.Cos(b) * r, MathF.Sin(b) * r, b, () => m.Box(MathF.Cos(b) * r, MathF.Sin(b) * r, 74, 30, 9, 9, i % 3 == 0 ? D : i % 3 == 1 ? W : m.C(0x2F86C6), .5f)); }
                    break;
                case 3: m.Lathe(0, 0, D, 170, 20, 130, 260, 0, 260); m.Ball(0, -10, 0, 200, 70, 200, C); m.Ball(0, -80, 0, 160, 70, 160, Sculpt.Mix(C, W, .15f)); m.Ball(0, -150, 0, 110, 60, 110, Sculpt.Mix(C, W, .3f)); m.Ball(0, -220, 0, 40, m.C(0xD7263D)); break;
                case 4: m.Lathe(0, 0, dough, 120, -20, 0, 300); m.Ball(0, -60, 0, 150, 70, 150, C); m.Ball(0, -130, 0, 115, 60, 115, Sculpt.Mix(C, W, .15f)); m.Ball(0, -190, 0, 75, 50, 75, Sculpt.Mix(C, W, .3f)); m.Lathe(0, 0, Sculpt.Mix(C, W, .3f), 0, -280, 30, -220); break;
                case 5: m.Ball(0, -60, 0, 210, 80, 210, C); m.Cyl(0, 0, 0, 200, 50, D); m.Ball(0, 60, 0, 210, 80, 210, C); break;
                case 6: m.Disc(0, 0, 0, 210, 210, 40, C, .5f); for (int i = 0; i < 9; i++) { float b = i * 2.39996f, r = 40 + i * 16; m.Ball(MathF.Cos(b) * r, MathF.Sin(b) * r, 20, 22, 22, 12, D); } break;
                case 7:
                    m.Box(0, 0, 0, 440, 320, 30, C, .05f);
                    for (int y = 0; y < 3; y++) for (int x = 0; x < 4; x++) m.Box(-165 + x * 110, -100 + y * 100, 20, 96, 86, 18, Sculpt.Mix(C, W, .12f), .3f);
                    break;
                case 8:
                    m.Slab(0, 220, W, -260, 160, 260, 160, 260, -40, -260, 60);
                    m.Box(0, 40, 0, 520, 30, 224, m.C(0xF48FB1), .05f); m.Box(0, 180, 0, 520, 40, 224, m.C(0xF2D3A2), .05f);
                    m.Ball(150, -90, 0, 60, C); m.Slab(0, 20, m.C(0x43A047), 120, -150, 150, -170, 180, -150, 150, -130); break;
                default:
                    m.Lathe(0, 0, W, 0, 215, 280, 210, 280, 230, 0, 230);
                    m.Lathe(0, 0, m.C(0xF8D77A), 0, -80, 140, -80, 200, 200, 0, 200);
                    m.Lathe(0, 0, D, 0, -90, 145, -88, 150, -50, 0, -50); m.Ball(0, -140, 0, 30, C); break;
            }
        });
    }

    static void Home3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = Wt(C); var metal = m.C(0xB8C0CC); var K = m.C(0x20242B);
        switch (a)
        {
            case 0:
                {
                    float g = .5f + .5f * MathF.Sin(t * 2f);
                    m.Lathe(0, 0, C, 0, -230, 120, -200, 170, -60, 100, 60, 70, 100);
                    m.Cyl(0, 140, 0, 70, 80, metal, .1f); m.Lathe(0, 0, D, 0, 220, 30, 210, 30, 185);
                    m.Ball(0, -60, 0, 230 * (.8f + .2f * g), new Vector4(C.X, C.Y, C.Z, .15f * g * C.W));
                    break;
                }
            case 1: m.Torus(-160, 0, 0, 100, 26, C); m.Box(110, 0, 0, 380, 50, 40, C, .3f); m.Box(220, 50, 0, 30, 60, 40, C, .2f); m.Box(270, 50, 0, 30, 80, 40, C, .2f); break;
            case 2:
                m.Turn(0, -60, MathF.Sin(t * 1.5f) * .06f, () =>
                {
                    m.Lathe(0, -60, C, 0, -340, 120, -300, 230, -180, 290, -60, 0, -60);
                    m.Limb(0, -340, 0, 0, 220, 0, 7, 7, D); m.Tube([.. ArcPts3(-50, 220, 50, 0, MathF.PI, 0)], 7, D);
                });
                break;
            case 3: m.Box(-10, 0, 0, 420, 480, 120, C, .1f); m.Box(20, 0, 0, 380, 460, 112, W, .05f); m.Box(-190, 0, 0, 40, 480, 124, Tint(C, .8f), .3f); break;
            case 4:
                m.Torus(-140, 0, 0, 100, 10, C); m.Torus(140, 0, 0, 100, 10, C); m.Tube([.. ArcPts3(0, 10, 40, MathF.PI, MathF.Tau, 0)], 8, C);
                m.Limb(-240, -10, 0, -250, -20, -260, 7, 7, C); m.Limb(240, -10, 0, 250, -20, -260, 7, 7, C);
                m.Disc(-140, 0, 0, 92, 92, 4, new Vector4(.8f, .9f, 1f, .25f * C.W), .1f); m.Disc(140, 0, 0, 92, 92, 4, new Vector4(.8f, .9f, 1f, .25f * C.W), .1f); break;
            case 5:
                {
                    float fl = MathF.Sin(t * 13f) * 6f;
                    m.Cyl(0, 90, 0, 70, 340, C, .1f); m.Limb(0, -80, 0, 0, -110, 0, 3, 3, K);
                    m.Ball(fl * .3f, -170, 0, 40 + fl * .5f, 80, 40, m.C(0xFFB300)); m.Ball(fl * .2f, -150, 10, 20, 45, 20, m.C(0xFFF59D)); break;
                }
            case 6:
                m.Turn(0, -250, MathF.Sin(t * 5f) * .2f, () =>
                {
                    m.Ball(0, -250, 0, 30, D); m.Lathe(0, 0, C, 0, -220, 40, -220, 100, 0, 200, 140, 210, 170, 0, 170); m.Ball(MathF.Sin(t * 5f) * -20, 200, 0, 40, D);
                });
                break;
            case 7: m.Limb(110, 110, 0, 290, 290, 0, 20, 24, m.C(0x5D4037)); m.Torus(-40, -40, 0, 200, 26, C); m.Disc(-40, -40, 0, 180, 180, 12, new Vector4(.75f, .9f, 1f, .4f * C.W), .1f); break;
            case 8:
                m.Tube([.. ArcPts3(0, 0, 250, 1.9f, 4.4f, 0)], 7, D); m.Cyl(0, 270, 0, 100, 30, D, .3f); m.Limb(0, 230, 0, 0, 270, 0, 7, 7, D);
                m.Yaw(0, 0, 0, t * .6f, () =>
                {
                    m.Ball(0, 0, 0, 210, m.C(0x2F86C6));
                    foreach (var (x, y) in new[] { (-60f, -60f), (70f, 40f), (-20f, 120f) }) m.Ball(x, y, Sculpt.FrontZ(0, 0, 0, 210, 210, 210, x, y) - 20, 70, 50, 30, m.C(0x43A047));
                    foreach (var (x, y) in new[] { (60f, -80f), (-80f, 60f) }) m.Ball(x, y, -Sculpt.FrontZ(0, 0, 0, 210, 210, 210, x, y) + 20, 70, 50, 30, m.C(0x43A047));
                });
                break;
            default:
                m.Tube([.. ArcPts3(0, 0, 160, 0, MathF.PI, 0, 20)], 60, C); m.Box(-160, -120, 0, 120, 120, 120, C, .1f); m.Box(160, -120, 0, 120, 120, 120, C, .1f);
                m.Box(-160, -210, 0, 120, 60, 120, metal, .1f); m.Box(160, -210, 0, 120, 60, 120, metal, .1f); break;
        }
    }

    static void Season3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = Wt(C); var K = m.C(0x20242B); var green = m.C(0x2E7D32);
        switch (a)
        {
            case 0:
                m.Ball(0, 130, 0, 170, W); m.Ball(0, -130, 0, 120, W); m.Cyl(0, -250, 0, 100, 30, K); m.Cyl(0, -310, 0, 60, 100, K); m.Torus(0, -30, 0, 110, 22, C);
                m.Ball(-40, -150, 105, 12, K); m.Ball(40, -150, 105, 12, K); m.Limb(0, -120, 110, 0, -115, 180, 14, 2, m.C(0xFF8F00));
                m.Limb(-150, 80, 0, -280, 0, 0, 5, 5, m.C(0x6D4C41)); m.Limb(150, 80, 0, 280, 0, 0, 5, 5, m.C(0x6D4C41)); break;
            case 1:
                m.Limb(-260, -300, 0, -260, 320, 0, 7, 7, m.C(0x8D6E63));
                for (int k = 0; k < 2; k++)
                {
                    float y = -200 + k * 220; var col = k == 0 ? C : D;
                    var path = new List<Vector3>(); for (int i = 0; i <= 10; i++) path.Add(V3(-250 + i * 45, y + MathF.Sin(i * .6f - t * 4f) * 12f * i / 10f, MathF.Sin(i * .5f - t * 3f) * 20f));
                    for (int i = 0; i < path.Count; i++) m.Ball(path[i].X, path[i].Y, path[i].Z, 30, 70 - i * 2.5f, 60 - i * 2f, col);
                    m.Ball(-170, y - 10, 56, 30, 30, 10, W); m.Ball(-170, y - 10, 64, 14, 14, 6, K);
                }
                break;
            case 2:
                for (int i = -1; i <= 1; i++) { float h = i == 0 ? 360 : 280; m.Cyl(i * 80, 140 - h / 2f, 0, 35, h, green, .05f); }
                m.Cyl(0, 200, 0, 165, 150, m.C(0xC69C6D), .15f); m.Ball(-150, 60, 60, 40, C); m.Ball(150, 60, 60, 40, C); break;
            case 3:
                m.Cyl(0, 270, 0, 35, 70, m.C(0x6D4C41));
                for (int k = 0; k < 3; k++) { float y = -200 + k * 140, w = 120 + k * 70; m.Lathe(0, 0, green, 0, y - 80, w, y + 100, 0, y + 100); }
                for (int i = 0; i < 8; i++) { float y = -130 + i * 50, w = 80 + i * 20, b = i * 2.1f; m.Ball(MathF.Cos(b) * w * .8f, y + 40, MathF.Sin(b) * w * .8f, 18, i % 2 == 0 ? C : D); }
                Puff(m, 0, 30, m.C(0xFFD54F), StarPts(0, -300, 50 * (1f + .1f * MathF.Sin(t * 3f)), 5, .45f, -MathF.PI / 2f)); break;
            case 4:
                m.Limb(-100, 320, 0, 60, -320, 0, 7, 7, green);
                for (int i = 0; i < 7; i++) { float y = -240 + i * 80; m.Ball(-40 - i * 20 + 120, y, 0, 70, 20, 6, Sculpt.Mix(green, W, .2f)); }
                for (int i = 0; i < 4; i++) { float y = -160 + i * 120, x = 10 - i * 25; m.Turn(x, y, MathF.Sin(t * 2f + i) * .2f, () => m.Box(x, y + 70, 10, 50, 140, 4, i % 2 == 0 ? C : D, .05f)); }
                break;
            case 5:
                m.Limb(0, -330, 0, 0, -220, 0, 2, 2, K); m.Lathe(0, 0, new Vector4(.88f, .95f, 1f, .7f * C.W), 0, -230, 120, -200, 170, -80, 170, -60, 0, -60);
                m.Ball(-60, -150, 150, 30, 18, 6, C);
                m.Turn(0, -40, MathF.Sin(t * 3f) * .25f, () => { m.Limb(0, -40, 0, 0, 120, 0, 2, 2, K); m.Box(0, 200, 0, 80, 160, 3, C, .05f); });
                break;
            case 6:
                m.Turn(0, 0, MathF.Sin(t) * .1f, () =>
                {
                    var l = new List<Vector2>();
                    for (int i = 0; i < 7; i++) { float b = -MathF.PI / 2f + (i - 3) * .45f, r = i % 2 == 0 ? 250 : 120; l.Add(new(MathF.Cos(b) * r, MathF.Sin(b) * r + 40)); }
                    l.Add(new(0, 80)); m.Slab(l, 0, 14, C); m.Limb(0, 60, 0, 0, 260, 0, 5, 5, D);
                    for (int i = 0; i < 7; i += 2) { float b = -MathF.PI / 2f + (i - 3) * .45f; m.Limb(0, 40, 8, MathF.Cos(b) * 200, MathF.Sin(b) * 200 + 40, 8, 2.5f, 2.5f, D); }
                });
                break;
            case 7:
                m.Box(0, 230, 0, 440, 80, 300, m.C(0xD7A86E), .1f); m.Ball(0, 110, 0, 230, 100, 230, W); m.Ball(0, -30, 0, 170, 80, 170, W); m.Ball(0, -130, 0, 55, C);
                m.Slab(0, 10, green, 20, -175, 70, -200, 50, -160); break;
            case 8:
                m.Turn(0, -300, MathF.Sin(t * 2f) * .1f, () =>
                {
                    m.Limb(0, -300, 0, 0, -200, 0, 2, 2, K); m.Ball(0, -110, 0, 100, W); m.Lathe(0, 0, W, 0, -60, 90, -60, 150, 160, 0, 160);
                    m.Pitch(0, -50, 0, MathF.PI / 2f, () => m.Torus(0, -50, 0, 92, 9, C));
                    m.Ball(-30, -120, 96, 9, K); m.Ball(30, -120, 96, 9, K);
                });
                break;
            default:
                m.Lathe(0, 0, new Vector4(.9f, .96f, 1f, .9f * C.W), 0, 0, 200, 0, 140, 240, 0, 240);
                m.Lathe(0, 0, W, 0, -210, 120, -180, 210, 0, 0, 0);
                m.Ball(-60, -100, 60, 140, 100, 100, new Vector4(C.X, C.Y, C.Z, .8f * C.W)); m.Ball(80, -80, 60, 120, 90, 100, new Vector4(D.X, D.Y, D.Z, .8f * D.W));
                m.Limb(150, 20, 0, 240, -260, 0, 5, 5, m.C(0xB8C0CC)); break;
        }
    }

    static void Status3D(Sculpt m, float t, Vector4 C, int a)
    {
        var W = Wt(C); float k = 1f + .06f * MathF.Sin(t * 3f);
        m.Zoom(0, 0, k, k, () =>
        {
            m.Disc(0, 0, 0, 230, 230, 70, C, .4f); const float z = 40;
            switch (a)
            {
                case 0: m.Tube([V3(-110, 0, z), V3(-30, 90, z), V3(120, -90, z)], 22, W); break;
                case 1: m.Limb(-90, -90, z, 90, 90, z, 22, 22, W); m.Limb(90, -90, z, -90, 90, z, 22, 22, W); break;
                case 2: m.Slab(z, 10, W, 0, -150, 140, 110, -140, 110); m.Box(0, 0, z + 8, 26, 100, 6, C, .5f); m.Ball(0, 70, z + 8, 16, C); break;
                case 3: m.Torus(0, 0, z, 140, 18, W); m.Limb(-100, 100, z, 100, -100, z, 18, 18, W); break;
                case 4: m.Ball(0, -100, z, 28, W); m.Box(0, 40, z, 50, 170, 20, W, .5f); break;
                case 5: m.Tube([.. ArcPts3(0, -50, 70, -3.4f, .6f, z), V3(0, 40, z), V3(0, 70, z)], 20, W); m.Ball(0, 130, z, 26, W); break;
                case 6: Puff(m, z, 20, W, StarPts(0, 10, 150, 5, .45f, -MathF.PI / 2f)); break;
                case 7: m.Slab(Q6Art.HeartLocal(0, 0, 150, false), z, 20, W); break;
                case 8: m.Ball(0, -40, z, 90, 90, 30, W); m.Box(0, 80, z, 80, 60, 30, W, .3f); for (int i = 0; i < 5; i++) { float b = -MathF.PI / 2f + (i - 2) * .5f; m.Limb(MathF.Cos(b) * 120, -40 + MathF.Sin(b) * 120, z, MathF.Cos(b) * 165, -40 + MathF.Sin(b) * 165, z, 7, 7, W); } break;
                default: { float f = MathF.Sin(t * 8f) * 8f; m.Slab(z, 20, W, 0, -170 + f, 100, -20, 120, 60, 60, 140, -60, 140, -120, 60, -100, -20, -40, -60, -50, -120); m.Slab(z + 12, 6, C, 0, -40 + f * .5f, 50, 40, 30, 110, -30, 110, -50, 40); break; }
            }
        });
    }

    static void Planet3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = Wt(C);
        void Band(float lat, float w, Vector4 col, float R = 201f) { float y = MathF.Sin(lat) * R, r = MathF.Cos(lat) * R; m.Pitch(0, y, 0, MathF.PI / 2f, () => m.Torus(0, y, 0, r, w, col)); }
        switch (a)
        {
            case 0:
                m.Ball(0, 0, 0, 200, C); m.Ball(0, 0, 0, 250 * (1f + .03f * MathF.Sin(t * 2f)), new Vector4(D.X, D.Y, D.Z, .3f * D.W));
                for (int i = 0; i < 12; i++) { float b = i * MathF.Tau / 12f + t * .2f; m.Limb(MathF.Cos(b) * 210, MathF.Sin(b) * 210, 0, MathF.Cos(b) * 290, MathF.Sin(b) * 290, 0, 18, 2, new Vector4(C.X, C.Y, C.Z, .7f * C.W)); }
                break;
            case 6: case 7:
                {
                    bool uran = a == 7;
                    m.Yaw(0, 0, 0, t * .3f, () => { m.Ball(0, 0, 0, 200, C); if (!uran) for (int i = -2; i <= 2; i++) Band(i * .3f, 6, new Vector4(D.X, D.Y, D.Z, .5f * D.W)); });
                    m.Turn(0, 0, uran ? 1.3f : -.35f, () => m.Pitch(0, 0, 0, 1.25f, () => m.Torus(0, 0, 0, 320, 26, D)));
                    break;
                }
            default:
                m.Yaw(0, 0, 0, t * .4f, () =>
                {
                    m.Ball(0, 0, 0, 230, C);
                    switch (a)
                    {
                        case 3: foreach (var (x, y) in new[] { (-70f, -60f), (80f, 60f), (-20f, 160f) }) m.Ball(x, y, Sculpt.FrontZ(0, 0, 0, 230, 230, 230, x, y) - 25, 80, 70, 30, D); foreach (var (x, y) in new[] { (60f, -100f), (-90f, 40f) }) m.Ball(x, y, -Sculpt.FrontZ(0, 0, 0, 230, 230, 230, x, y) + 25, 80, 70, 30, D); break;
                        case 5: for (int i = -3; i <= 3; i++) Band(i * .25f, 10, i % 2 == 0 ? new Vector4(D.X, D.Y, D.Z, .8f * D.W) : new Vector4(1, 1, 1, .25f * C.W), 231f); break;
                        case 2: for (int i = -2; i <= 2; i++) Band(i * .35f, 9, new Vector4(D.X, D.Y, D.Z, .5f * D.W), 231f); break;
                        case 1: case 9: for (int i = 0; i < 12; i++) { float yy = 1f - (i + .5f) * 2f / 12f, rr = MathF.Sqrt(MathF.Max(0f, 1f - yy * yy)), b = i * 2.39996f; var d = new Vector3(MathF.Cos(b) * rr, yy, MathF.Sin(b) * rr); m.Ball(d.X * 222, d.Y * 222, d.Z * 222, 30, 30, 14, new Vector4(D.X, D.Y, D.Z, .7f * D.W)); } break;
                    }
                });
                break;
        }
    }

    static void Money3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        var W = Wt(C); var K = m.C(0x20242B);
        switch (a)
        {
            case 6:
                m.Turn(0, 0, MathF.Sin(t) * .05f, () =>
                {
                    m.Box(0, 0, 0, 620, 300, 6, C, .02f); m.Tube(RoundRectPath(580, 260, 10, 4), 3, D); m.Disc(-170, 0, 4, 80, 80, 4, Sculpt.Mix(C, W, .3f), .1f);
                    m.Text(s, 90, 130, -40, 4, 3, D); m.Box(130, 60, 4, 260, 14, 2, D, .3f); m.Box(130, 95, 4, 200, 14, 2, D, .3f);
                });
                break;
            case 7: m.Box(0, 40, 0, 500, 340, 80, C, .25f); m.Box(0, -60, 6, 500, 150, 84, Tint(C, .85f), .25f); m.Box(150, -10, 46, 200, 90, 10, D, .4f); m.Ball(200, -10, 54, 18, Sculpt.Mix(D, W, .5f)); break;
            case 8:
                for (int i = 0; i < 14; i++)
                {
                    int row = i < 5 ? 0 : i < 9 ? 1 : i < 12 ? 2 : 3; int col = i < 5 ? i : i < 9 ? i - 5 : i < 12 ? i - 9 : i - 12; int n = 5 - row;
                    float x = (col - (n - 1) / 2f) * 110, y = 200 - row * 70;
                    m.Cyl(x, y - 10, (i % 3 - 1) * 40, 70, 26, C, .3f);
                }
                break;
            case 9:
                m.Ball(0, 40, 0, 270, 210, 200, C);
                m.Disc(255, 40, 0, 50, 70, 40, Tint(C, .85f), .5f); m.Ball(270, 30, 10, 10, K); m.Ball(270, 60, 10, 10, K);
                m.Slab(0, 30, Tint(C, .9f), -120, -140, -60, -230, -20, -150); m.Slab(0, 30, Tint(C, .9f), 40, -150, 90, -230, 130, -140);
                m.Ball(100, -30, 180, 16, K); foreach (int sx in Sides) foreach (int sz in Sides) m.Cyl(sx * 110, 230, sz * 90, 34, 70, Tint(C, .85f), .3f);
                m.Box(0, -170, 0, 120, 16, 30, K, .4f); m.Disc(0, -220 - Fr1(t * .7f) * -60 - 60, 0, 28, 28, 8, m.C(0xF2C14E), .4f); break;
            default:
                {
                    bool hole = a == 1 || a == 3;
                    m.Yaw(0, 0, 0, MathF.Sin(t * 1.5f) * .5f, () =>
                    {
                        if (hole) m.Pitch(0, 0, 0, MathF.PI / 2f, () => m.Lathe(0, 0, C, 55, -15, 220, -15, 220, 15, 55, 15, 55, -15));
                        else m.Disc(0, 0, 0, 220, 220, 30, C, .3f);
                        m.Torus(0, 0, 16, 175, 4, Tint(D, .8f));
                        m.Text(s, FitSize(s, 260, hole ? 90 : 170), 0, hole ? 110 : 0, 17, 6, Tint(D, .7f), 260);
                    });
                    break;
                }
        }
    }

    /// <summary>2401〜2500 の図形を、種類と番号から描く</summary>
    static void Art10_3D(Sculpt m, float t, Vector4 C, Vector4 D, string s, int kind, int a)
    {
        if (s.Length == 0) s = " ";
        var W = Wt(C);
        switch (kind)
        {
            case 54: Kitchen3D(m, t, C, D, a); break;
            case 55: Stationery3D(m, t, C, D, a); break;
            case 56: Sweets3D(m, t, C, D, a); break;
            case 57: Home3D(m, t, C, D, a); break;
            case 58:
                {
                    float hop = MathF.Max(0f, MathF.Sin(t * 2.5f)) * 12f;
                    m.Move(0, -hop, () => m.Yaw(0, 0, 0, MathF.Sin(t * .8f) * .3f, () =>
                    {
                        m.Box(0, 0, 0, 400, 400, 400, C, .12f); m.Tube(RoundRectPath(360, 360, 18, 202), 4, new Vector4(1, 1, 1, .6f * C.W));
                        m.Text(s, FitSize(s, 300, 280), 0, 0, 202, 16, W, 300);
                    }));
                    break;
                }
            case 59: Season3D(m, t, C, D, a); break;
            case 60: Status3D(m, t, C, a); break;
            case 61: Planet3D(m, t, C, D, a); break;
            case 62:
                {
                    float k = 1f + .12f * MathF.Max(0f, 1f - Fr1(t * .5f) * 6f);
                    m.Zoom(0, 0, k, k, () => m.Turn(0, 0, -.18f, () =>
                    {
                        m.Torus(0, 0, 0, 220, 9, C); m.Torus(0, 0, 0, 180, 3, C);
                        m.Text(s, FitSize(s, 300, 180), 0, 0, 0, 10, C, 300);
                    }));
                    break;
                }
            default: Money3D(m, t, C, D, a, s); break;
        }
    }

    static partial void RegisterL1c(Dictionary<Type, QuickModel> d)
    {
        FamT(d, typeof(X2401Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 0));
        FamT(d, typeof(X2402Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 1));
        FamT(d, typeof(X2403Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 2));
        FamT(d, typeof(X2404Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 3));
        FamT(d, typeof(X2405Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 4));
        FamT(d, typeof(X2406Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 5));
        FamT(d, typeof(X2407Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 6));
        FamT(d, typeof(X2408Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 7));
        FamT(d, typeof(X2409Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 8));
        FamT(d, typeof(X2410Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 54, 9));
        FamT(d, typeof(X2411Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 0));
        FamT(d, typeof(X2412Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 1));
        FamT(d, typeof(X2413Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 2));
        FamT(d, typeof(X2414Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 3));
        FamT(d, typeof(X2415Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 4));
        FamT(d, typeof(X2416Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 5));
        FamT(d, typeof(X2417Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 6));
        FamT(d, typeof(X2418Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 7));
        FamT(d, typeof(X2419Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 8));
        FamT(d, typeof(X2420Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 55, 9));
        FamT(d, typeof(X2421Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 0));
        FamT(d, typeof(X2422Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 1));
        FamT(d, typeof(X2423Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 2));
        FamT(d, typeof(X2424Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 3));
        FamT(d, typeof(X2425Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 4));
        FamT(d, typeof(X2426Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 5));
        FamT(d, typeof(X2427Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 6));
        FamT(d, typeof(X2428Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 7));
        FamT(d, typeof(X2429Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 8));
        FamT(d, typeof(X2430Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 56, 9));
        FamT(d, typeof(X2431Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 0));
        FamT(d, typeof(X2432Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 1));
        FamT(d, typeof(X2433Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 2));
        FamT(d, typeof(X2434Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 3));
        FamT(d, typeof(X2435Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 4));
        FamT(d, typeof(X2436Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 5));
        FamT(d, typeof(X2437Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 6));
        FamT(d, typeof(X2438Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 7));
        FamT(d, typeof(X2439Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 8));
        FamT(d, typeof(X2440Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 57, 9));
        FamT(d, typeof(X2441Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2442Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2443Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2444Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2445Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2446Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2447Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2448Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2449Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2450Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 58, 0));
        FamT(d, typeof(X2451Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 0));
        FamT(d, typeof(X2452Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 1));
        FamT(d, typeof(X2453Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 2));
        FamT(d, typeof(X2454Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 3));
        FamT(d, typeof(X2455Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 4));
        FamT(d, typeof(X2456Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 5));
        FamT(d, typeof(X2457Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 6));
        FamT(d, typeof(X2458Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 7));
        FamT(d, typeof(X2459Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 8));
        FamT(d, typeof(X2460Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 59, 9));
        FamT(d, typeof(X2461Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 0));
        FamT(d, typeof(X2462Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 1));
        FamT(d, typeof(X2463Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 2));
        FamT(d, typeof(X2464Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 3));
        FamT(d, typeof(X2465Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 4));
        FamT(d, typeof(X2466Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 5));
        FamT(d, typeof(X2467Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 6));
        FamT(d, typeof(X2468Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 7));
        FamT(d, typeof(X2469Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 8));
        FamT(d, typeof(X2470Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 60, 9));
        FamT(d, typeof(X2471Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 0));
        FamT(d, typeof(X2472Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 1));
        FamT(d, typeof(X2473Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 2));
        FamT(d, typeof(X2474Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 3));
        FamT(d, typeof(X2475Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 4));
        FamT(d, typeof(X2476Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 5));
        FamT(d, typeof(X2477Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 6));
        FamT(d, typeof(X2478Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 7));
        FamT(d, typeof(X2479Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 8));
        FamT(d, typeof(X2480Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 61, 9));
        FamT(d, typeof(X2481Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2482Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2483Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2484Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2485Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2486Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2487Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2488Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2489Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2490Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 62, 0));
        FamT(d, typeof(X2491Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 0));
        FamT(d, typeof(X2492Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 1));
        FamT(d, typeof(X2493Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 2));
        FamT(d, typeof(X2494Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 3));
        FamT(d, typeof(X2495Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 4));
        FamT(d, typeof(X2496Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 5));
        FamT(d, typeof(X2497Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 6));
        FamT(d, typeof(X2498Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 7));
        FamT(d, typeof(X2499Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 8));
        FamT(d, typeof(X2500Parameter), (m, t, C, D, s) => Art10_3D(m, t, C, D, s, 63, 9));
    }
}
