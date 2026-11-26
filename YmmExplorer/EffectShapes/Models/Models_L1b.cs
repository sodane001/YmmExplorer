using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 221番 2201〜2400(Q6Art の種類 37〜53)
//   旗・宝石・メダル・風船・プレゼント箱・信号とランプ・ゲームパッド・時間の道具・花・パズル・通知バッジ・フローチャート・音符・一文字札・チェス・将棋・ボール
// =====================================================================
internal static partial class Models3D
{
    static Vector4 Tint(Vector4 c, float f) => new(c.X * f, c.Y * f, c.Z * f, c.W);
    static Vector4 Wt(Vector4 c) => new(1, 1, 1, c.W);

    static void Flag3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        m.Limb(-226, -310, 0, -226, 320, 0, 8, 8, m.C(0x8A8F98)); m.Ball(-226, -318, 0, 16, m.C(0xF2C14E));
        const int n = 12; const float len = 430;
        var sa = new List<(float, float, int)>(); var sb = new List<(float, float, int)>();
        for (int i = 0; i < n; i++)
        {
            float u0 = i / (float)n, u1 = (i + 1f) / n, x0 = -220 + u0 * len, x1 = -220 + u1 * len;
            float z0 = MathF.Sin(u0 * 5f - t * 4f) * 40f * u0, z1 = MathF.Sin(u1 * 5f - t * 4f) * 40f * u1;
            Q6Art.FlagColumn(a, u0, i / 2, sa); Q6Art.FlagColumn(a, u1, i / 2, sb);
            for (int k = 0; k < sa.Count; k++)
            {
                var c = sa[k].Item3 == 0 ? C : sa[k].Item3 == 1 ? D : Wt(C);
                m.Quad3D(new(x0, sa[k].Item1, z0), new(x1, sb[k].Item1, z1), new(x1, sb[k].Item2, z1), new(x0, sa[k].Item2, z0), c);
            }
        }
    }

    static void Gem3D(Sculpt m, float t, Vector4 C, int a)
    {
        m.Yaw(0, 0, 0, MathF.Sin(t * .7f) * .5f, () => Gem(m, Q6Art.GemOutline(a), 0, a == 3 ? 40 : 0, .55f, 18, 90, C, Sculpt.Mix(C, Wt(C), .35f)));
        float tw = MathF.Max(0f, MathF.Sin(t * 2.5f));
        if (tw > .05f) Puff(m, 100, 4, Wt(C), StarPts(90, -110, 50 * tw, 4, .2f, 0f));
    }

    static void Award3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        switch (a)
        {
            case 0:
                m.Slab(-10, 10, m.C(0xD7263D), -110, -300, -30, -300, 40, -60, -40, -60); m.Slab(-14, 10, m.C(0x2F86C6), 110, -300, 30, -300, -40, -60, 40, -60);
                m.Disc(0, 60, 0, 170, 170, 40, C, .3f); m.Torus(0, 60, 20, 140, 9, D); Puff(m, 22, 14, D, StarPts(0, 60, 90, 5, .45f, -MathF.PI / 2f)); break;
            case 1:
                m.Torus(-170, -120, 0, 70, 12, D); m.Torus(170, -120, 0, 70, 12, D);
                m.Lathe(0, 0, C, 175, -230, 160, -150, 120, -60, 50, 10, 30, 30);
                m.Cyl(0, 70, 0, 30, 120, D); m.Cyl(0, 150, 0, 110, 44, C, .2f); m.Box(0, 210, 0, 300, 80, 160, D, .1f);
                break;
            case 2:
                m.Slab(0, 50, C, -200, -250, 200, -250, 200, 0, 0, 260, -200, 0); m.Slab(30, 10, D, -160, -210, 160, -210, 160, -10, 0, 200, -160, -10);
                Puff(m, 36, 14, C, StarPts(0, -40, 90, 5, .45f, -MathF.PI / 2f)); break;
            case 3:
                m.Slab(0, 60, C, -230, 120, -230, -120, -115, 0, 0, -170, 115, 0, 230, -120, 230, 120);
                m.Box(0, 150, 0, 470, 70, 90, D, .3f);
                foreach (var (x, y) in new[] { (-230f, -140f), (0f, -190f), (230f, -140f) }) m.Ball(x, y, 0, 26, Sculpt.Mix(C, Wt(C), .3f));
                m.Ball(-120, 150, 44, 18, m.C(0xD7263D)); m.Ball(0, 150, 44, 22, m.C(0x2F86C6)); m.Ball(120, 150, 44, 18, m.C(0x35A86B)); break;
            default:
                m.Slab(-10, 8, D, -90, 60, -20, 60, -60, 330, -110, 290); m.Slab(-10, 8, D, 90, 60, 20, 60, 60, 330, 110, 290);
                for (int i = 0; i < 16; i++) { float b = i * MathF.Tau / 16f; m.Disc(MathF.Cos(b) * 160, -40 + MathF.Sin(b) * 160, 0, 52, 52, 16, i % 2 == 0 ? C : Sculpt.Mix(C, Wt(C), .25f), .4f); }
                m.Disc(0, -40, 10, 140, 140, 30, C, .3f); m.Disc(0, -40, 26, 105, 105, 8, Wt(C), .3f); Puff(m, 32, 10, C, StarPts(0, -40, 70, 5, .45f, -MathF.PI / 2f)); break;
        }
    }

    static void Balloon3D(Sculpt m, float t, Vector4 C, int a)
    {
        float bob = MathF.Sin(t * 1.5f) * 12f;
        var str = new List<Vector3>(); for (int i = 0; i <= 20; i++) { float y = 130 + i * 10; str.Add(new(MathF.Sin(i * .5f + t * 2f) * 12f, y + bob * (1f - i / 20f), 0)); }
        m.Tube(str, 2.5f, m.C(0x9AA3AD));
        if (a == 0) { m.Ball(0, -70 + bob, 0, 170, 205, 170, C); m.Ball(-60, -150 + bob, 140, 30, 46, 10, new Vector4(1, 1, 1, .45f * C.W)); }
        else { Puff(m, 0, 140, C, Q6Art.HeartLocal(0, -70 + bob, 210, false).SelectMany(v => new[] { v.X, v.Y }).ToArray()); }
        m.Lathe(0, 0, Tint(C, .8f), 0, 115 + bob, 18, 140 + bob, 0, 140 + bob);
    }

    static void Gift3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        float hop = MathF.Max(0f, MathF.Sin(t * 3f)) * 14f;
        m.Move(0, -hop, () =>
        {
            m.Box(0, 90, 0, 380, 300, 380, C, .06f);
            if (a == 1) for (int i = -3; i <= 3; i++) m.Box(i * 54, 90, 192, 20, 300, 4, new Vector4(1, 1, 1, .35f * C.W), .05f);
            m.Box(0, -80, 0, 420, 80, 420, Tint(C, .9f), .06f);
            m.Box(0, 90, 0, 60, 302, 384, D, .05f); m.Box(0, 90, 0, 384, 302, 60, D, .05f);
            m.Box(0, -80, 0, 60, 82, 424, D, .05f); m.Box(0, -80, 0, 424, 82, 60, D, .05f);
            m.Turn(-70, -150, -.4f, () => m.Torus(-70, -150, 0, 60, 18, D)); m.Turn(70, -150, .4f, () => m.Torus(70, -150, 0, 60, 18, D));
            m.Ball(0, -130, 0, 30, Tint(D, .85f));
        });
    }

    static void Lamp3D(Sculpt m, float t, Vector4 C, int a)
    {
        float pulse = .75f + .25f * MathF.Sin(t * 3f);
        if (a <= 3)
        {
            Vector4[] cols = [m.C(0xE53935), m.C(0xFFC107), m.C(0x2ECC71)];
            m.Box(0, a == 3 ? 40 : 0, 0, 560, a == 3 ? 300 : 220, 120, m.C(0x2B2F36), .2f);
            for (int i = 0; i < 3; i++)
            {
                float x = -170 + i * 170; bool on = i == a;
                m.Disc(x, 0, 62, 80, 80, 8, m.C(0x14171C), .2f);
                m.Ball(x, 0, 70, 66, 66, 26, on ? cols[i] : Tint(cols[i], .3f));
                if (on) m.Disc(x, 0, 92, 90 * pulse, 90 * pulse, 2, new Vector4(cols[i].X, cols[i].Y, cols[i].Z, .25f * cols[i].W), .1f);
            }
            if (a == 3) { m.Ball(0, 130, 62, 50, 50, 20, Tint(cols[2], .4f)); m.Slab(80, 6, cols[2], -40, 121, 8, 121, 8, 104, 40, 130, 8, 156, 8, 139, -40, 139); }
            return;
        }
        m.Disc(0, 40, 0, 210, 210, 60, m.C(0x5B6270), .3f); m.Disc(0, 40, 20, 175, 175, 40, m.C(0x2B2F36), .3f);
        m.Ball(0, 20, 40, 150, 150, 110, C); m.Ball(-50, -40, 140, 40, 28, 10, new Vector4(1, 1, 1, .5f * C.W));
        m.Disc(0, 20, 60, 230 * pulse, 230 * pulse, 2, new Vector4(C.X, C.Y, C.Z, .15f * C.W), .1f);
    }

    static void Pad3D(Sculpt m, float t, Vector4 C, int a)
    {
        var dark = m.C(0x3A3F47); var W = Wt(C); int dir = (int)(t * 1.5f) % 4; float press = Fr1(t * .8f) < .15f ? 14f : 0f;
        switch (a)
        {
            case 0:
                m.Slab(0, 60, dark, -70, -230, 70, -230, 70, -70, 230, -70, 230, 70, 70, 70, 70, 230, -70, 230, -70, 70, -230, 70, -230, -70, -70, -70);
                for (int i = 0; i < 4; i++)
                {
                    float b = i * MathF.PI / 2f - MathF.PI / 2f, cx = MathF.Cos(b) * 150, cy = MathF.Sin(b) * 150;
                    m.Slab(i == dir ? 26 : 32, 6, i == dir ? C : Sculpt.Mix(dark, W, .4f), cx + MathF.Cos(b) * 40, cy + MathF.Sin(b) * 40, cx - MathF.Sin(b) * 35, cy + MathF.Cos(b) * 35, cx + MathF.Sin(b) * 35, cy - MathF.Cos(b) * 35);
                }
                m.Lathe(0, 0, Tint(dark, .8f), 0, -40, 40, -40, 40, 0);
                break;
            case 1: case 2: case 3: case 4:
                m.Disc(0, 0, -press, 190, 190, 90, C, .5f); m.Text(new[] { "A", "B", "X", "Y" }[a - 1], 220, 0, 0, 46 - press, 14, W);
                break;
            case 5: case 6: m.Box(0, 0, -press, 460, 170, 110, C, .9f); m.Text(a == 5 ? "L" : "R", 140, 0, 0, 56 - press, 12, W); break;
            case 7:
                {
                    float sx = MathF.Cos(t * 2f) * 60, sy = MathF.Sin(t * 2f) * 60;
                    m.Disc(0, 0, 0, 220, 220, 60, dark, .3f); m.Limb(0, 0, 0, sx * .5f, sy * .5f, 90, 50, 50, Tint(dark, .7f));
                    m.Disc(sx, sy, 110, 140, 140, 50, C, .5f); m.Torus(sx, sy, 136, 110, 6, new Vector4(1, 1, 1, .3f * C.W));
                    break;
                }
            default:
                m.Box(0, -40, 0, 300, 110, 50, dark, .95f); m.Box(0, -40, 26 - press, 260, 74, 30, C, .95f);
                m.Text(a == 8 ? "START" : "SELECT", 74, 0, 110, 0, 10, W, 0, dark, 3);
                break;
        }
    }

    static void TimeTool3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = Wt(C); var K = m.C(0x20242B);
        switch (a)
        {
            case 0:
                {
                    float f = Fr1(t * .25f);
                    m.Cyl(0, -250, 0, 170, 40, D, .2f); m.Cyl(0, 250, 0, 170, 40, D, .2f);
                    foreach (int s in Sides) { m.Limb(s * 150, -230, 0, s * 150, 230, 0, 9, 9, D); m.Limb(0, -230, s * 150, 0, 230, s * 150, 9, 9, D); }
                    m.Lathe(0, 0, new Vector4(1, 1, 1, .22f * C.W), 0, -230, 120, -228, 60, -80, 12, 0, 60, 80, 120, 228, 0, 230);
                    float ht = (1f - f) * 210f; if (ht > 4f) m.Lathe(0, 0, C, 0, -ht, 120 * ht / 230f, -ht, 10, -8, 0, -8);
                    float hb = f * 150f; if (hb > 4f) m.Lathe(0, 0, C, 0, 230 - hb, 120 * (1f - hb / 230f), 230 - hb, 120, 229, 0, 229);
                    if (f < .97f) m.Limb(0, -6, 0, 0, 230 - hb, 0, 3, 3, C);
                    break;
                }
            case 1:
                m.Box(0, -250, 0, 70, 50, 70, D, .3f); m.Disc(0, 20, 0, 240, 240, 80, C, .3f); m.Disc(0, 20, 30, 205, 205, 24, W, .2f);
                for (int i = 0; i < 12; i++) { float b = i * MathF.Tau / 12f; m.Limb(MathF.Cos(b) * 170, 20 + MathF.Sin(b) * 170, 44, MathF.Cos(b) * 190, 20 + MathF.Sin(b) * 190, 44, i % 3 == 0 ? 4 : 2, i % 3 == 0 ? 4 : 2, K); }
                { float h = t * 1.2f - MathF.PI / 2f; m.Limb(0, 20, 50, MathF.Cos(h) * 165, 20 + MathF.Sin(h) * 165, 50, 4, 4, C); m.Ball(0, 20, 50, 16, K); }
                break;
            case 2:
                m.Turn(0, 20, MathF.Sin(t * 30f) * .05f * (Fr1(t * .3f) < .4f ? 1f : 0f), () =>
                {
                    m.Ball(-150, -170, 0, 80, 80, 60, D); m.Ball(150, -170, 0, 80, 80, 60, D);
                    m.Limb(-120, 200, 0, -170, 270, 0, 9, 9, D); m.Limb(120, 200, 0, 170, 270, 0, 9, 9, D);
                    m.Disc(0, 20, 0, 220, 220, 120, C, .4f); m.Disc(0, 20, 50, 180, 180, 24, W, .2f);
                    float h = t * .5f; m.Limb(0, 20, 66, MathF.Cos(h - 1.6f) * 100, 20 + MathF.Sin(h - 1.6f) * 100, 66, 6, 6, K); m.Limb(0, 20, 68, MathF.Cos(h * 12f - 1.6f) * 150, 20 + MathF.Sin(h * 12f - 1.6f) * 150, 68, 3.5f, 3.5f, K);
                    m.Ball(0, 20, 70, 14, K);
                });
                break;
            case 3:
                {
                    m.Box(0, 0, 0, 440, 440, 160, C, .4f); m.Disc(0, 0, 80, 160, 160, 20, W, .2f);
                    float ang = -Fr1(t * .1f) * MathF.Tau;
                    m.Disc(0, 0, 100, 50, 50, 60, D, .3f); m.Turn(0, 0, ang, () => m.Box(0, -40, 120, 26, 70, 20, Sculpt.Mix(D, W, .3f), .5f));
                    for (int i = 0; i < 12; i++) { float b = i * MathF.Tau / 12f; m.Limb(MathF.Cos(b) * 130, MathF.Sin(b) * 130, 92, MathF.Cos(b) * 150, MathF.Sin(b) * 150, 92, 2.5f, 2.5f, K); }
                    break;
                }
            default:
                {
                    m.Box(0, 0, 0, 640, 280, 80, m.C(0x1B1F26), .2f);
                    bool colon = Fr1(t) < .5f; int mm = (int)(t / 60f) % 60, sec = (int)t % 60;
                    m.Text($"{12 + mm / 60:00}{(colon ? ":" : " ")}{34 + mm:00}", 170, -40, 0, 42, 6, C);
                    m.Text($"{sec:00}", 70, 220, 50, 42, 4, C);
                    break;
                }
        }
    }

    static void Flower3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var green = m.C(0x43A047); var W = Wt(C);
        // 花びら: 中心から外へ向けて、少し手前へ反らせた平たい玉
        void Petal(float ang, float dist, float len, float wid, Vector4 c, float tilt = .25f) =>
            m.Turn(0, 0, ang, () => m.Yaw(0, 0, 0, -tilt, () => m.Ball(dist, 0, 0, len, wid, MathF.Max(6f, wid * .25f), c)));
        m.Turn(0, 0, MathF.Sin(t * 1.2f) * .05f, () =>
        {
            switch (a)
            {
                case 0: for (int i = 0; i < 14; i++) Petal(i * MathF.Tau / 14f, 130, 110, 32, C); m.Ball(0, 0, 20, 70, 70, 40, D); break;
                case 1:
                    for (int i = 0; i < 5; i++) { float b = i * MathF.Tau / 5f - MathF.PI / 2f; m.Turn(0, 0, b + MathF.PI / 2f, () => m.Slab(Q6Art.HeartLocal(0, -125, 115, false), 0, 12, C)); }
                    m.Ball(0, 0, 14, 40, 40, 20, D); for (int i = 0; i < 5; i++) { float b = i * MathF.Tau / 5f; m.Ball(MathF.Cos(b) * 55, MathF.Sin(b) * 55, 20, 10, D); }
                    break;
                case 2:
                    m.Limb(0, 40, 0, 0, 300, 0, 9, 9, green); m.Turn(0, 230, -.6f, () => m.Ball(-60, 200, 0, 90, 30, 10, green));
                    m.Lathe(0, 0, C, 0, 80, 70, 70, 100, 0, 95, -80, 80, -150, 40, -160, 50, -120, 0, -110);
                    break;
                case 3:
                    for (int i = 0; i < 22; i++) Petal(i * MathF.Tau / 22f + .14f, 175, 85, 26, Tint(C, .85f), .1f);
                    for (int i = 0; i < 22; i++) Petal(i * MathF.Tau / 22f, 160, 80, 24, C, .3f);
                    m.Ball(0, 0, 10, 115, 115, 45, D); for (int i = 0; i < 40; i++) { float b = i * 2.39996f, r = 10 + i * 2.4f; m.Ball(MathF.Cos(b) * r, MathF.Sin(b) * r, Sculpt.FrontZ(0, 0, 10, 115, 115, 45, MathF.Cos(b) * r, MathF.Sin(b) * r), 7, Tint(D, .6f)); }
                    break;
                case 4: for (int i = 0; i < 8; i++) Petal(i * MathF.Tau / 8f, 130, 115, 55, C); m.Ball(0, 0, 20, 50, 50, 30, D); break;
                case 5:
                    for (int l = 0; l < 4; l++) for (int i = 0; i < 5; i++) { float b = i * MathF.Tau / 5f + l * .6f + t * .1f; m.Ball(MathF.Cos(b) * (180 - l * 45), MathF.Sin(b) * (180 - l * 45), l * 30, 110 - l * 22, 110 - l * 22, 40, Tint(C, .7f + l * .1f)); }
                    break;
                case 6:
                    for (int i = 0; i < 5; i++) Petal(i * MathF.Tau / 5f, 120, 130, 100, C, .2f);
                    m.Ball(0, 0, 20, 50, 50, 20, Tint(C, .6f)); m.Limb(0, 0, 20, 120, -150, 90, 5, 5, D); for (int i = 0; i < 6; i++) m.Ball(110 + i * 4, -140 - i * 6, 90, 10, D);
                    break;
                case 7:
                    m.Limb(0, 0, 0, 40, 300, 0, 7, 7, green);
                    for (int i = 0; i < 4; i++) { float b = i * MathF.Tau / 4f + MathF.PI / 4f; m.Turn(0, 0, b + MathF.PI / 2f, () => m.Slab(Q6Art.HeartLocal(0, -105, 105, false), 0, 16, C)); }
                    m.Ball(0, 0, 10, 18, Tint(C, .6f)); break;
                case 8:
                    m.Limb(0, 0, 0, 0, 320, 0, 5, 5, green);
                    for (int i = 0; i < 40; i++)
                    {
                        float yy = 1f - (i + .5f) * 2f / 40f, rr = MathF.Sqrt(MathF.Max(0f, 1f - yy * yy)), b = i * 2.39996f, r = 180 + (i % 3) * 15;
                        var d = new Vector3(MathF.Cos(b) * rr, yy, MathF.Sin(b) * rr) * r;
                        m.Limb(0, 0, 0, d.X, d.Y, d.Z, 1.5f, 1.5f, new Vector4(1, 1, 1, .8f * C.W)); m.Ball(d.X, d.Y, d.Z, 10, C);
                    }
                    m.Ball(0, 0, 0, 32, D); break;
                default:
                    for (int i = 0; i < 6; i++) { float b = i * MathF.Tau / 6f; m.Turn(0, 0, b, () => m.Yaw(0, 0, 0, -.35f, () => m.Slab(0, 10, C, 30, -40, 250, 0, 30, 40, 0, 0))); }
                    for (int i = 0; i < 6; i++) { float b = i * MathF.Tau / 6f + .5f; m.Limb(0, 0, 0, MathF.Cos(b) * 110, MathF.Sin(b) * 110, 60, 3, 3, D); m.Ball(MathF.Cos(b) * 110, MathF.Sin(b) * 110, 60, 12, D); }
                    break;
            }
        });
    }

    static void Flow3D(Sculpt m, Vector4 C, Vector4 D, int a, string s)
    {
        var q = Q6Art.FlowPts(a); var W = Wt(C);
        if (q == null) { m.Disc(0, 0, 0, 170, 170, 60, C, .3f); m.Torus(0, 0, 30, 170, 6, D); }
        else if (a == 5)
        {
            m.Pitch(0, 0, 0, MathF.PI / 2f, () => { });
            m.Super(0, 0, 0, 200, 150, 60, .1f, 1f, C);
            m.Tube(Enumerable.Range(0, 41).Select(i => V3(MathF.Cos(i * MathF.Tau / 40f) * 200, -120 + MathF.Sin(i * MathF.Tau / 40f) * 60, 62)).ToList(), 5, D);
        }
        else { m.Slab(q, 0, 60, C); m.Tube(Close3(q, 30), 6, D); }
        if (a == 9) { m.Limb(-200, -130, 32, -200, 130, 32, 5, 5, D); m.Limb(200, -130, 32, 200, 130, 32, 5, 5, D); }
        m.Text(s, FitSize(s, a == 6 ? 240 : 380, 110), 0, a == 5 ? 30 : a == 4 ? -20 : 0, a == 5 ? 64 : 32, 10, W, a == 6 ? 240 : 380);
    }

    static void Music3D(Sculpt m, float t, Vector4 C, int a)
    {
        float bob = MathF.Abs(MathF.Sin(t * 2.5f)) * -18f;
        m.Move(0, bob, () =>
        {
            void Head(float x, float y, bool hollow) => m.Turn(x, y, -.35f, () => { if (hollow) m.Tube(Enumerable.Range(0, 33).Select(i => V3(x + MathF.Cos(i * MathF.Tau / 32f) * 62, y + MathF.Sin(i * MathF.Tau / 32f) * 40, 0)).ToList(), 10, C); else m.Ball(x, y, 0, 72, 50, 30, C); });
            switch (a)
            {
                case 0: m.Turn(0, 0, -.2f, () => m.Tube(Enumerable.Range(0, 33).Select(i => V3(MathF.Cos(i * MathF.Tau / 32f) * 112, MathF.Sin(i * MathF.Tau / 32f) * 70, 0)).ToList(), 18, C)); break;
                case 1: Head(-30, 180, true); m.Limb(32, 170, 0, 32, -220, 0, 7, 7, C); break;
                case 2: Head(-30, 180, false); m.Limb(32, 170, 0, 32, -220, 0, 7, 7, C); break;
                case 3: case 4:
                    Head(-30, 180, false); m.Limb(32, 170, 0, 32, -220, 0, 7, 7, C);
                    for (int f = 0; f < a - 2; f++) m.Tube([V3(32, -220 + f * 70), V3(110, -140 + f * 70), V3(120, -40 + f * 70)], 9, C);
                    break;
                case 5: Head(-140, 200, false); Head(110, 150, false); m.Limb(-78, 190, 0, -78, -170, 0, 7, 7, C); m.Limb(172, 140, 0, 172, -220, 0, 7, 7, C); m.Slab(0, 24, C, -85, -200, 180, -250, 180, -200, -85, -150); break;
                case 6:
                    {
                        var q = new List<Vector3>();
                        for (int k = 0; k <= 40; k++) { float b = MathF.PI * .5f + k / 40f * MathF.PI * 2.3f, r = 25 + k * 3.2f; q.Add(V3(MathF.Cos(b) * r * .9f, 80 + MathF.Sin(b) * r)); }
                        q.AddRange([V3(40, -150), V3(60, -250), V3(10, -300), V3(-35, -230), V3(20, 120), V3(40, 270), V3(0, 310), V3(-50, 280)]);
                        m.Tube(q, 10, C); m.Ball(-45, 270, 0, 26, C);
                        break;
                    }
                case 7: m.Tube([.. ArcPts3(-40, -60, 140, -2.8f, 1.6f, 0)], 20, C); m.Ball(-150, -40, 0, 34, C); m.Ball(150, -110, 0, 20, C); m.Ball(150, 0, 0, 20, C); break;
                case 8: m.Tube([V3(-30, -250), V3(50, -150), V3(-40, -40), V3(50, 60), V3(-20, 120), V3(40, 230)], 17, C); break;
                default: m.Ball(-50, -120, 0, 46, C); m.Limb(-20, -110, 0, 60, -140, 0, 9, 9, C); m.Limb(60, -140, 0, -20, 220, 0, 9, 9, C); break;
            }
        });
    }

    static void Chess3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        int piece = a % 6;
        m.Yaw(0, 0, 0, MathF.Sin(t * .7f) * .4f, () =>
        {
            m.Lathe(0, 0, C, Q6Art.ChessProfile(piece));
            switch (piece)
            {
                case 0: m.Box(0, -250, 0, 30, 110, 30, C, .2f); m.Box(0, -260, 0, 100, 30, 30, C, .2f); break;
                case 1: for (int i = 0; i < 8; i++) { float b = i * MathF.Tau / 8f; m.Ball(MathF.Cos(b) * 90, -215, MathF.Sin(b) * 90, 20, C); } m.Ball(0, -262, 0, 24, C); break;
                case 2: for (int i = 0; i < 4; i++) { float b = i * MathF.Tau / 4f + MathF.PI / 4f; m.Box(MathF.Cos(b) * 80, -190, MathF.Sin(b) * 80, 50, 40, 50, C, .15f); } break;
                case 3: m.Ball(0, -238, 0, 18, C); break;
                case 4: m.Slab(Q6Art.KnightHead(), 0, 110, C); m.Ball(30, -160, 56, 12, D); m.Ball(30, -160, -56, 12, D); break;
            }
        });
    }

    static void Shogi3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        var q = Q6Art.ShogiPts();
        m.Pitch(0, 0, 0, -.15f + MathF.Sin(t) * .05f, () =>
        {
            m.Slab(q, 0, 70, C); m.Tube(Close3(q, 36), 4, D);
            m.Text(s, FitSize(s, 260, 240), 0, 30, 36, 6, a == 1 ? m.C(0xC62828) : m.C(0x20242B), 260);
        });
    }

    static void Ball3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = Wt(C); var K = m.C(0x20242B); const float R = 230;
        List<Vector3> On(IEnumerable<Vector2> xy) => Sculpt.OnFront(0, 0, 0, R, R, R, 3, xy);
        m.Yaw(0, 0, 0, MathF.Sin(t * .8f) * .6f, () =>
        {
            switch (a)
            {
                case 4:
                    m.Turn(0, 0, -.5f, () =>
                    {
                        m.Ball(0, 0, 0, 260, 170, 170, C);
                        m.Tube(Sculpt.OnFront(0, 0, 0, 260, 170, 170, 3, [new(-90, 0), new(90, 0)]), 5, W);
                        for (int i = -3; i <= 3; i++) m.Tube(Sculpt.OnFront(0, 0, 0, 260, 170, 170, 3, [new(i * 24, -22), new(i * 24, 22)]), 4, W);
                    });
                    return;
                case 6:
                    for (int i = 0; i < 6; i++) m.Lathe(0, 0, Enumerable.Range(0, 13).Select(k => { float b = -MathF.PI / 2f + k * MathF.PI / 12f; return new Vector2(MathF.Cos(b) * R, MathF.Sin(b) * R); }).ToList(), i % 3 == 0 ? C : i % 3 == 1 ? D : m.C(0x2F86C6), i * MathF.Tau / 6f, (i + 1) * MathF.Tau / 6f);
                    m.Ball(0, -R + 2, 0, 50, 10, 50, W); return;
            }
            m.Ball(0, 0, 0, R, C);
            switch (a)
            {
                case 0:
                    m.Torus(0, 0, 0, R + 2, 4, K); m.Yaw(0, 0, 0, MathF.PI / 2f, () => m.Torus(0, 0, 0, R + 2, 4, K));
                    m.Pitch(0, 0, 0, MathF.PI / 2f, () => m.Torus(0, 0, 0, R + 2, 4, K));
                    break;
                case 1:
                    foreach (int sx in Sides) { m.Tube(On(Sculpt.ArcPts(sx * 330, 0, 210, sx < 0 ? -.8f : MathF.PI - .8f, sx < 0 ? .8f : MathF.PI + .8f, 20)), 3, D); m.Tube(On(Sculpt.ArcPts(sx * 330, 0, 210, sx < 0 ? -.8f : MathF.PI - .8f, sx < 0 ? .8f : MathF.PI + .8f, 20)).Select(v => v with { Z = -v.Z }).ToList(), 3, D); }
                    break;
                case 2:
                    {
                        var q = new List<Vector3>();
                        for (int k = 0; k <= 64; k++) { float u = k / 64f * MathF.Tau, y = MathF.Sin(u) * .55f; float rr = MathF.Sqrt(1f - y * y); q.Add(new Vector3(MathF.Cos(u) * rr, y, MathF.Sin(u * 2f) * .0f + MathF.Sin(u) * rr * .0f) * (R + 3)); }
                        var q2 = new List<Vector3>(); for (int k = 0; k <= 64; k++) { float u = k / 64f * MathF.Tau, h = MathF.Sin(2f * u) * .45f, rr = MathF.Sqrt(1f - h * h); q2.Add(new Vector3(MathF.Cos(u) * rr, h, MathF.Sin(u) * rr) * (R + 3)); }
                        m.Tube(q2, 6, W);
                        break;
                    }
                case 3: for (int i = 0; i < 3; i++) { float b = i * MathF.Tau / 3f; m.Turn(0, 0, b, () => m.Torus(0, 0, 0, R + 1, 14, i == 0 ? m.C(0x2F86C6) : D)); m.Yaw(0, 0, 0, MathF.PI / 2f, () => { }); } break;
                case 5:
                    for (int i = 0; i < 90; i++)
                    {
                        float yy = 1f - (i + .5f) * 2f / 90f, rr = MathF.Sqrt(MathF.Max(0f, 1f - yy * yy)), b = i * 2.39996f;
                        var d = new Vector3(MathF.Cos(b) * rr, yy, MathF.Sin(b) * rr);
                        m.Ball(d.X * (R - 4), d.Y * (R - 4), d.Z * (R - 4), 14, new Vector4(.8f, .82f, .85f, C.W));
                    }
                    break;
                default: foreach (var (x, y) in new[] { (-40f, -90f), (50f, -110f), (10f, -30f) }) m.Disc(x, y, Sculpt.FrontZ(0, 0, 0, R, R, R, x, y) - 6, 32, 32, 20, K, .3f); break;
            }
        });
    }

    /// <summary>2201〜2400 の図形を、種類と番号から描く</summary>
    static void Art9_3D(Sculpt m, float t, Vector4 C, Vector4 D, string s, int kind, int a)
    {
        if (s.Length == 0) s = " ";
        var W = Wt(C); var K = m.C(0x20242B);
        switch (kind)
        {
            case 37: Flag3D(m, t, C, D, a); break;
            case 38: Gem3D(m, t, C, a); break;
            case 39: Award3D(m, t, C, D, a); break;
            case 40: Balloon3D(m, t, C, a); break;
            case 41: Gift3D(m, t, C, D, a); break;
            case 42: Lamp3D(m, t, C, a); break;
            case 43: Pad3D(m, t, C, a); break;
            case 44: TimeTool3D(m, t, C, D, a); break;
            case 45: Flower3D(m, t, C, D, a); break;
            case 46: { var q = Q6Art.PuzzlePts(a); m.Move(0, MathF.Sin(t * 1.4f) * 8f, () => { m.Slab(q, 0, 70, C); m.Tube(Close3(q, 36), 4, D); }); break; }
            case 47:
                {
                    float k = 1f + .12f * MathF.Max(0f, MathF.Sin(t * 4f));
                    m.Zoom(0, 0, k, k, () => { m.Disc(0, 0, 0, 170, 170, 70, C, .5f); m.Torus(0, 0, 36, 156, 5, W); m.Text(s, FitSize(s, 250, 190), 0, 0, 36, 18, W, 250); });
                    break;
                }
            case 48: Flow3D(m, C, D, a, s); break;
            case 49: Music3D(m, t, C, a); break;
            case 50:
                m.Turn(0, -260, MathF.Sin(t * 1.1f) * .03f, () =>
                {
                    m.Box(0, 0, 0, 300, 500, 12, Sculpt.Mix(C, W, .75f), .05f); m.Tube(RoundRectPath(270, 470, 10, 7), 4, C);
                    m.Limb(0, -250, 0, 0, -320, 0, 3, 3, D); m.Ball(0, -250, 6, 14, D);
                    m.Text(s, FitSize(s, 240, 230), 0, -20, 6, 6, K, 240); m.Box(85, 180, 8, 70, 70, 6, D, .1f);
                });
                break;
            case 51: Chess3D(m, t, C, D, a); break;
            case 52: Shogi3D(m, t, C, D, a, s); break;
            default: Ball3D(m, t, C, D, a); break;
        }
    }

    static partial void RegisterL1b(Dictionary<Type, QuickModel> d)
    {
        FamT(d, typeof(X2201Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 0));
        FamT(d, typeof(X2202Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 0));
        FamT(d, typeof(X2203Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 0));
        FamT(d, typeof(X2204Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 0));
        FamT(d, typeof(X2205Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 0));
        FamT(d, typeof(X2206Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 1));
        FamT(d, typeof(X2207Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 1));
        FamT(d, typeof(X2208Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 1));
        FamT(d, typeof(X2209Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 1));
        FamT(d, typeof(X2210Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 1));
        FamT(d, typeof(X2211Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 2));
        FamT(d, typeof(X2212Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 2));
        FamT(d, typeof(X2213Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 2));
        FamT(d, typeof(X2214Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 2));
        FamT(d, typeof(X2215Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 2));
        FamT(d, typeof(X2216Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 3));
        FamT(d, typeof(X2217Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 3));
        FamT(d, typeof(X2218Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 3));
        FamT(d, typeof(X2219Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 3));
        FamT(d, typeof(X2220Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 37, 3));
        FamT(d, typeof(X2221Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 0));
        FamT(d, typeof(X2222Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 0));
        FamT(d, typeof(X2223Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 0));
        FamT(d, typeof(X2224Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 0));
        FamT(d, typeof(X2225Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 0));
        FamT(d, typeof(X2226Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 1));
        FamT(d, typeof(X2227Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 1));
        FamT(d, typeof(X2228Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 1));
        FamT(d, typeof(X2229Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 1));
        FamT(d, typeof(X2230Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 1));
        FamT(d, typeof(X2231Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 2));
        FamT(d, typeof(X2232Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 2));
        FamT(d, typeof(X2233Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 2));
        FamT(d, typeof(X2234Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 2));
        FamT(d, typeof(X2235Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 2));
        FamT(d, typeof(X2236Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 3));
        FamT(d, typeof(X2237Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 3));
        FamT(d, typeof(X2238Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 3));
        FamT(d, typeof(X2239Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 3));
        FamT(d, typeof(X2240Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 38, 3));
        FamT(d, typeof(X2241Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 0));
        FamT(d, typeof(X2242Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 0));
        FamT(d, typeof(X2243Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 0));
        FamT(d, typeof(X2244Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 1));
        FamT(d, typeof(X2245Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 1));
        FamT(d, typeof(X2246Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 1));
        FamT(d, typeof(X2247Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 2));
        FamT(d, typeof(X2248Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 2));
        FamT(d, typeof(X2249Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 2));
        FamT(d, typeof(X2250Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 3));
        FamT(d, typeof(X2251Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 3));
        FamT(d, typeof(X2252Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 3));
        FamT(d, typeof(X2253Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 4));
        FamT(d, typeof(X2254Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 4));
        FamT(d, typeof(X2255Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 39, 4));
        FamT(d, typeof(X2256Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 0));
        FamT(d, typeof(X2257Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 0));
        FamT(d, typeof(X2258Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 0));
        FamT(d, typeof(X2259Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 0));
        FamT(d, typeof(X2260Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 0));
        FamT(d, typeof(X2261Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 1));
        FamT(d, typeof(X2262Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 1));
        FamT(d, typeof(X2263Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 1));
        FamT(d, typeof(X2264Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 1));
        FamT(d, typeof(X2265Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 40, 1));
        FamT(d, typeof(X2266Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 0));
        FamT(d, typeof(X2267Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 0));
        FamT(d, typeof(X2268Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 0));
        FamT(d, typeof(X2269Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 0));
        FamT(d, typeof(X2270Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 0));
        FamT(d, typeof(X2271Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 1));
        FamT(d, typeof(X2272Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 1));
        FamT(d, typeof(X2273Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 1));
        FamT(d, typeof(X2274Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 1));
        FamT(d, typeof(X2275Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 41, 1));
        FamT(d, typeof(X2276Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 0));
        FamT(d, typeof(X2277Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 1));
        FamT(d, typeof(X2278Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 2));
        FamT(d, typeof(X2279Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 3));
        FamT(d, typeof(X2280Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 4));
        FamT(d, typeof(X2281Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 5));
        FamT(d, typeof(X2282Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 6));
        FamT(d, typeof(X2283Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 7));
        FamT(d, typeof(X2284Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 8));
        FamT(d, typeof(X2285Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 42, 9));
        FamT(d, typeof(X2286Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 0));
        FamT(d, typeof(X2287Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 1));
        FamT(d, typeof(X2288Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 2));
        FamT(d, typeof(X2289Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 3));
        FamT(d, typeof(X2290Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 4));
        FamT(d, typeof(X2291Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 5));
        FamT(d, typeof(X2292Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 6));
        FamT(d, typeof(X2293Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 7));
        FamT(d, typeof(X2294Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 8));
        FamT(d, typeof(X2295Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 43, 9));
        FamT(d, typeof(X2296Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 44, 0));
        FamT(d, typeof(X2297Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 44, 1));
        FamT(d, typeof(X2298Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 44, 2));
        FamT(d, typeof(X2299Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 44, 3));
        FamT(d, typeof(X2300Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 44, 4));
        FamT(d, typeof(X2301Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 0));
        FamT(d, typeof(X2302Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 0));
        FamT(d, typeof(X2303Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 1));
        FamT(d, typeof(X2304Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 1));
        FamT(d, typeof(X2305Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 2));
        FamT(d, typeof(X2306Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 2));
        FamT(d, typeof(X2307Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 3));
        FamT(d, typeof(X2308Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 3));
        FamT(d, typeof(X2309Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 4));
        FamT(d, typeof(X2310Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 4));
        FamT(d, typeof(X2311Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 5));
        FamT(d, typeof(X2312Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 5));
        FamT(d, typeof(X2313Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 6));
        FamT(d, typeof(X2314Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 6));
        FamT(d, typeof(X2315Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 7));
        FamT(d, typeof(X2316Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 7));
        FamT(d, typeof(X2317Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 8));
        FamT(d, typeof(X2318Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 8));
        FamT(d, typeof(X2319Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 9));
        FamT(d, typeof(X2320Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 45, 9));
        FamT(d, typeof(X2321Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 0));
        FamT(d, typeof(X2322Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 0));
        FamT(d, typeof(X2323Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 0));
        FamT(d, typeof(X2324Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 0));
        FamT(d, typeof(X2325Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 0));
        FamT(d, typeof(X2326Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 1));
        FamT(d, typeof(X2327Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 1));
        FamT(d, typeof(X2328Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 1));
        FamT(d, typeof(X2329Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 1));
        FamT(d, typeof(X2330Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 46, 1));
        FamT(d, typeof(X2331Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2332Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2333Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2334Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2335Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2336Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2337Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2338Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2339Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2340Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 47, 0));
        FamT(d, typeof(X2341Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 0));
        FamT(d, typeof(X2342Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 1));
        FamT(d, typeof(X2343Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 2));
        FamT(d, typeof(X2344Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 3));
        FamT(d, typeof(X2345Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 4));
        FamT(d, typeof(X2346Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 5));
        FamT(d, typeof(X2347Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 6));
        FamT(d, typeof(X2348Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 7));
        FamT(d, typeof(X2349Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 8));
        FamT(d, typeof(X2350Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 48, 9));
        FamT(d, typeof(X2351Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 0));
        FamT(d, typeof(X2352Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 1));
        FamT(d, typeof(X2353Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 2));
        FamT(d, typeof(X2354Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 3));
        FamT(d, typeof(X2355Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 4));
        FamT(d, typeof(X2356Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 5));
        FamT(d, typeof(X2357Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 6));
        FamT(d, typeof(X2358Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 7));
        FamT(d, typeof(X2359Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 8));
        FamT(d, typeof(X2360Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 49, 9));
        FamT(d, typeof(X2361Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2362Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2363Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2364Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2365Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2366Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2367Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2368Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2369Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2370Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 50, 0));
        FamT(d, typeof(X2371Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 0));
        FamT(d, typeof(X2372Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 1));
        FamT(d, typeof(X2373Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 2));
        FamT(d, typeof(X2374Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 3));
        FamT(d, typeof(X2375Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 4));
        FamT(d, typeof(X2376Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 5));
        FamT(d, typeof(X2377Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 0));
        FamT(d, typeof(X2378Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 1));
        FamT(d, typeof(X2379Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 2));
        FamT(d, typeof(X2380Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 3));
        FamT(d, typeof(X2381Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 4));
        FamT(d, typeof(X2382Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 51, 5));
        FamT(d, typeof(X2383Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2384Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2385Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2386Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2387Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2388Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2389Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2390Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2391Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 0));
        FamT(d, typeof(X2392Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 52, 1));
        FamT(d, typeof(X2393Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 53, 0));
        FamT(d, typeof(X2394Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 53, 1));
        FamT(d, typeof(X2395Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 53, 2));
        FamT(d, typeof(X2396Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 53, 3));
        FamT(d, typeof(X2397Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 53, 4));
        FamT(d, typeof(X2398Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 53, 5));
        FamT(d, typeof(X2399Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 53, 6));
        FamT(d, typeof(X2400Parameter), (m, t, C, D, s) => Art9_3D(m, t, C, D, s, 53, 7));
    }
}
