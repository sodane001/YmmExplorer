using System.Numerics;
using Vortice.Mathematics;

namespace YMM4EffectShapes;

// =====================================================================
//  図形 2000種までの追加分: 20=元素カード 21=人物 22=図解 23=天気記号
// =====================================================================
public abstract partial class Q6Art
{
    static Color4 Mx(Color4 x, Color4 y, float f) => new(x.R + (y.R - x.R) * f, x.G + (y.G - x.G) * f, x.B + (y.B - x.B) * f, x.A);

    void ElementCard(ArtPen p, Color4 C, Color4 D, Color4 W, string s)
    {
        var q = s.Split(' ', 2);
        p.R(0, 0, 410, 480, 30, C);
        p.RL(0, 0, 372, 442, 20, D, 6);
        p.T(a.ToString(), 54, -112, -175, W);
        p.T(q[0], 190, 0, -18, W, default, 0f, 320);
        if (q.Length > 1) p.T(q[1], 58, 0, 172, W, default, 0f, 340);
    }

    // 体の傾き, 前の腕(上・下), 後ろの腕(上・下), 前の脚(上・下), 後ろの脚(上・下), 腰の高さ。角度は真下が0、前がプラス
    internal static readonly float[][] Poses =
    [
        [3, 25, 60, -25, -5, 22, 10, -22, -40, 0],
        [12, 60, 120, -50, 10, 55, 10, -35, -95, -10],
        [24, 80, 150, -70, 5, 75, 25, -50, -120, -20],
        [0, 6, 4, -6, -4, 5, 3, -5, -3, 0],
        [0, 170, 175, -8, -5, 5, 3, -5, -3, 0],
        [0, 155, 165, -155, -165, 8, 4, -8, -4, 0],
        [-4, 135, 150, -135, -150, 18, 10, -18, -10, -16],
        [0, 120, 165, -8, -5, 5, 3, -5, -3, 0],
        [0, 90, 90, -8, -5, 10, 5, -5, -3, 0],
        [0, 25, 105, -20, 100, 8, 4, -8, -4, 0],
        [0, 45, -40, -45, 40, 8, 4, -8, -4, 0],
        [5, 20, 165, -10, 70, 5, 3, -5, -3, 0],
        [0, 75, 165, -8, -5, 5, 3, -5, -3, 0],
        [55, 20, 15, -5, -5, 8, 4, -8, -4, 0],
        [-4, 30, 70, -10, 40, 85, 5, 75, 0, 80],
        [20, 45, 60, 30, 55, 130, 20, 120, 15, 190],
        [-5, 140, 160, -140, -160, 40, -30, -40, -90, -110],
        [-10, -30, -5, 40, 70, 95, 100, -5, -3, -10],
        [10, -110, -160, 70, 85, 30, 10, -30, -15, 0],
        [78, 165, 170, -20, -30, -82, -90, -96, -105, 110],
        [28, 85, 85, 75, 80, 30, 10, -30, -25, 0],
        [-20, 75, 80, 65, 75, 30, 35, -10, -5, 0],
        [0, 175, 175, -175, -175, 15, 5, -15, -5, 0],
        [65, 120, 150, 100, 130, -60, -95, -40, -80, 150],
        [90, 20, 10, -10, 10, -88, -92, -95, -100, 215],
        [72, 10, 5, -10, -5, 10, -95, -10, -95, 90],
        [0, 70, 170, -10, -5, 12, 5, -12, -5, 0],
        [0, 75, 150, -75, -150, 8, 4, -8, -4, 0],
        [12, 130, -120, -130, 120, 8, 4, -8, -4, 0],
        [5, 45, 100, 30, 100, 6, 3, -6, -3, 0],
        [0, 130, 60, -60, -130, 30, -20, -10, -5, -6],
        [8, -120, -170, 60, 120, 5, 3, -35, 10, -4],
        [-22, 165, 170, 150, 160, 8, 4, -8, -4, 0],
        [28, 85, 88, 75, 80, 85, -15, 70, -20, 75],
        [72, 8, 2, -2, -6, -78, -80, -82, -84, 95],
        [0, 90, 95, -90, -95, 80, -10, 0, 0, 0],
        [15, 60, 90, -20, 40, 40, -5, -30, -60, 6],
        [75, 160, 165, -160, -165, 0, 0, -85, -88, 0],
        [180, 2, 0, -2, 0, 175, 170, -175, -170, -200],
        [20, -40, -10, 60, 90, 85, 85, -60, -130, -100],
    ];

    void Person(ArtPen p, float t, Color4 C, Color4 D)
    {
        var q = Poses[Math.Clamp(a, 0, Poses.Length - 1)];
        static float Rad(float v) => v * MathF.PI / 180f;
        float bob = MathF.Sin(t * 2.4f) * 6f, ts = MathF.Sin(Rad(q[0])), tc = MathF.Cos(Rad(q[0]));
        var hip = new Vector2(0, 58 + q[9] + bob);
        var sh = hip + new Vector2(ts * 170f, -tc * 170f);
        var head = sh + new Vector2(ts * 84f, -tc * 84f);
        void Limb(Vector2 from, float a1, float a2, float l1, float l2, float w, Color4 col)
        {
            var m = from + new Vector2(MathF.Sin(Rad(a1)) * l1, MathF.Cos(Rad(a1)) * l1);
            var e = m + new Vector2(MathF.Sin(Rad(a2)) * l2, MathF.Cos(Rad(a2)) * l2);
            p.L(from.X, from.Y, m.X, m.Y, col, w); p.L(m.X, m.Y, e.X, e.Y, col, w);
            p.D(from.X, from.Y, w / 2f, col); p.D(m.X, m.Y, w / 2f, col); p.D(e.X, e.Y, w / 2f, col);
        }
        p.L(-230, 326, 230, 326, p.C(D, .6f), 6);
        Limb(sh, q[3], q[4], 92, 88, 34, D);
        Limb(hip, q[7], q[8], 128, 124, 42, D);
        p.L(hip.X, hip.Y, sh.X, sh.Y, C, 74); p.D(hip.X, hip.Y, 37, C); p.D(sh.X, sh.Y, 37, C);
        Limb(hip, q[5], q[6], 128, 124, 42, C);
        Limb(sh, q[1], q[2], 92, 88, 34, C);
        p.D(head.X, head.Y, 56, C);
    }

    void Diagram(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        Color4 Tone(int i, int n) => Mx(C, D, n <= 1 ? 0f : i / (n - 1f));
        void Box(float x, float y, float w, float h, Color4 c) => p.R(x, y, w, h, 14, c);
        void Ar(float x0, float y0, float x1, float y1, Color4 c)
        {
            var d = new Vector2(x1 - x0, y1 - y0); float l = d.Length(); if (l < 1f) return; d /= l; var n = new Vector2(-d.Y, d.X);
            p.L(x0, y0, x1 - d.X * 22, y1 - d.Y * 22, c, 9); p.F(c, x1, y1, x1 - d.X * 32 + n.X * 19, y1 - d.Y * 32 + n.Y * 19, x1 - d.X * 32 - n.X * 19, y1 - d.Y * 32 - n.Y * 19);
        }
        float w0 = .5f + .5f * MathF.Sin(t * 1.6f);
        switch (a)
        {
            case 0: for (int i = 0; i < 3; i++) { Box(0, -200 + i * 200, 330, 110, Tone(i, 3)); if (i < 2) Ar(0, -140 + i * 200, 0, -62 + i * 200, W); } break;
            case 1: Box(0, -230, 280, 86, C); Ar(0, -183, 0, -118, W); p.F(D, 0, -115, 170, 0, 0, 115, -170, 0); foreach (int s in new[] { -1, 1 }) { p.L(s * 170, 0, s * 260, 0, W, 9); Ar(s * 260, 0, s * 260, 130, W); Box(s * 260, 190, 210, 96, Tone(s > 0 ? 1 : 0, 2)); } break;
            case 2: for (int i = 0; i < 4; i++) { float m = i * 1.5708f - 1.5708f + t * .25f; p.Arc(0, 0, 210, m + .42f, m + 1.15f, W, 9); float e = m + 1.15f; p.F(W, MathF.Cos(e + .16f) * 210, MathF.Sin(e + .16f) * 210, MathF.Cos(e) * 240, MathF.Sin(e) * 240, MathF.Cos(e) * 180, MathF.Sin(e) * 180); p.D(MathF.Cos(m) * 210, MathF.Sin(m) * 210, 66, Tone(i, 4)); } break;
            case 3: p.D(-110, 0, 190, p.C(C, .72f)); p.D(110, 0, 190, p.C(D, .72f)); break;
            case 4: p.D(0, -100, 165, p.C(C, .68f)); p.D(-105, 85, 165, p.C(D, .68f)); p.D(105, 85, 165, p.C(0x35B56A, .68f)); break;
            case 5: for (int i = 0; i < 4; i++) { float y = -260 + i * 135, a0 = 58 + i * 75, b0 = a0 + 62; p.F(Tone(i, 4), -a0 + (i == 0 ? a0 : 0), y, a0 - (i == 0 ? a0 : 0), y, b0, y + 122, -b0, y + 122); } break;
            case 6: for (int i = 0; i < 4; i++) { float y = -260 + i * 135, a0 = 340 - i * 72, b0 = a0 - 58; p.F(Tone(i, 4), -a0, y, a0, y, b0, y + 122, -b0, y + 122); } break;
            case 7: for (int y = 0; y < 2; y++) for (int x = 0; x < 2; x++) Box(-125 + x * 250, -125 + y * 250, 232, 232, Tone(x + y * 2, 4)); break;
            case 8: for (int y = 0; y < 3; y++) for (int x = 0; x < 3; x++) Box(-170 + x * 170, -170 + y * 170, 156, 156, Tone(x + y, 5)); break;
            case 9: for (int i = 0; i < 6; i++) { float h = (110 + 300 * (.5f + .5f * MathF.Sin(i * 1.7f + 1f))) * (.92f + w0 * .08f); p.R(-225 + i * 90, 240 - h / 2f, 62, h, 8, Tone(i, 6)); } p.L(-290, 242, 290, 242, W, 7); break;
            case 10: for (int i = 0; i < 5; i++) { float l = 150 + 380 * (.5f + .5f * MathF.Sin(i * 2.1f + .4f)); p.R(-270 + l / 2f, -200 + i * 100, l, 66, 8, Tone(i, 5)); } p.L(-272, -255, -272, 255, W, 7); break;
            case 11: case 12:
                {
                    var q = new List<Vector2>(); for (int i = 0; i < 8; i++) q.Add(new(-270 + i * 77, 110 - 240 * (.5f + .5f * MathF.Sin(i * 1.05f + .3f)) * (.35f + i * .09f)));
                    if (a == 12) { var f = new List<Vector2>(q) { new(269, 220), new(-270, 220) }; p.Fill(p.C(C, .55f), f); }
                    p.Line(C, 12, q); foreach (var v in q) p.D(v.X, v.Y, 15, D); p.L(-300, 222, 300, 222, W, 6); p.L(-298, -230, -298, 224, W, 6); break;
                }
            case 13: for (int i = 0; i < 34; i++) { float x = -260 + ArtPen.R01(i) * 520, y = 170 - (x + 260) * .62f + (ArtPen.R01(i + 50) - .5f) * 130; p.D(x, y, 13, i % 2 == 0 ? C : D); } p.L(-292, 222, 292, 222, W, 6); p.L(-290, -230, -290, 224, W, 6); break;
            case 14: { float[] v = [.4f, .27f, .2f, .13f]; float m = -1.5708f; for (int i = 0; i < 4; i++) { float e = m + v[i] * MathF.Tau; p.Arc(0, 0, 190, m + .03f, e - .03f, Tone(i, 4), 92); m = e; } break; }
            case 15:
                {
                    var o = new List<Vector2>(); var q = new List<Vector2>(); float[] v = [.9f, .62f, .78f, .5f, .86f, .68f];
                    for (int r = 1; r <= 3; r++) { var g = new List<Vector2>(); for (int i = 0; i <= 6; i++) { float m = i * 1.0472f - 1.5708f; g.Add(new(MathF.Cos(m) * r * 85, MathF.Sin(m) * r * 85)); } p.Line(p.C(W, .5f), 3, g); }
                    for (int i = 0; i < 6; i++) { float m = i * 1.0472f - 1.5708f; p.L(0, 0, MathF.Cos(m) * 255, MathF.Sin(m) * 255, p.C(W, .4f), 3); q.Add(new(MathF.Cos(m) * 255 * v[i], MathF.Sin(m) * 255 * v[i])); }
                    p.Fill(p.C(C, .68f), q); q.Add(q[0]); p.Line(D, 7, q); break;
                }
            case 16: for (int i = 0; i < 5; i++) { float y = 230; for (int k = 0; k < 3; k++) { float h = 62 + 74 * (.5f + .5f * MathF.Sin(i * 1.3f + k * 2f)); p.B(-200 + i * 100, y - h / 2f, 68, h - 3, Tone(k, 3)); y -= h; } } p.L(-270, 232, 270, 232, W, 6); break;
            case 17: p.L(-310, 0, 310, 0, W, 9); for (int i = 0; i < 5; i++) { float x = -250 + i * 125, s = i % 2 == 0 ? -1 : 1; p.L(x, 0, x, s * 90, W, 4); p.D(x, 0, 21, Tone(i, 5)); Box(x, s * 140, 112, 72, Tone(i, 5)); } break;
            case 18: case 19:
                {
                    float[] x1 = [-160, 160], x2 = [-240, -80, 80, 240];
                    foreach (var x in x1) { p.L(0, -180, x, 0, W, 6); } for (int i = 0; i < 4; i++) p.L(x1[i / 2], 0, x2[i], 180, W, 6);
                    if (a == 18) { p.D(0, -180, 58, C); foreach (var x in x1) p.D(x, 0, 48, Mx(C, D, .5f)); foreach (var x in x2) p.D(x, 180, 38, D); }
                    else { Box(0, -180, 190, 82, C); foreach (var x in x1) Box(x, 0, 172, 72, Mx(C, D, .5f)); foreach (var x in x2) Box(x, 180, 134, 66, D); }
                    break;
                }
            case 20: for (int i = 0; i < 6; i++) { float m = i * 1.0472f + t * .18f, x = MathF.Cos(m) * 235, y = MathF.Sin(m) * 205; p.L(0, 0, x, y, W, 6); p.D(x, y, 54, Tone(i, 6)); } p.D(0, 0, 88, C); break;
            case 21: foreach (int s in new[] { -1, 1 }) { Box(s * 160, 0, 292, 500, p.C(s < 0 ? C : D, .9f)); for (int i = 0; i < 4; i++) p.R(s * 160, -130 + i * 88, 214, 26, 12, p.C(0xFFFFFF, .8f)); } break;
            case 22: { float k = MathF.Sin(t * 1.3f) * .14f; p.L(0, -220, 0, 240, W, 12); p.R(0, 250, 220, 24, 10, W); p.Turn(0, -190, k, () => { p.L(-250, -190, 250, -190, W, 11); foreach (int s in new[] { -1, 1 }) { p.L(s * 250, -190, s * 170, 0, W, 3); p.L(s * 250, -190, s * 330, 0, W, 3); p.Pie(s * 250, 0, 82, 0, 3.1416f, s < 0 ? C : D); } }); p.D(0, -190, 21, W); break; }
            case 23: for (int i = 0; i < 5; i++) { float h = 95 + i * 95; p.R(-240 + i * 120, 250 - h / 2f, 110, h, 8, Tone(i, 5)); } Ar(-250, 60, 240, -310, W); break;
            case 24: for (int i = 0; i < 5; i++) { float s0 = i * 72, l = 170 + (i % 3) * 66; p.L(-280, -200 + i * 100, 300, -200 + i * 100, p.C(W, .22f), 3); p.R(-230 + s0 + l / 2f, -200 + i * 100, l, 58, 14, Tone(i, 5)); } p.L(-230 + w0 * 420, -255, -230 + w0 * 420, 255, W, 5); break;
            case 25: for (int y = 0; y < 6; y++) for (int x = 0; x < 8; x++) { float v = .5f + .5f * MathF.Sin(x * .9f + y * 1.3f + t * .8f); p.R(-245 + x * 70, -175 + y * 70, 65, 65, 7, Mx(D, C, v)); } break;
            case 26: for (int i = 0; i < 6; i++) { float m = i * 1.0472f + .5f, x = MathF.Cos(m) * 260, y = MathF.Sin(m) * 205, cx = MathF.Cos(m + .3f) * 130, cy = MathF.Sin(m + .3f) * 120; p.P(Tone(i, 6), 8, 0, 0, cx, cy, x, y); p.E(x, y, 78, 40, Tone(i, 6)); } p.E(0, 0, 118, 72, W); p.E(0, 0, 104, 60, C); break;
            case 27: p.L(-320, 0, 320, 0, W, 8); p.F(W, 345, 0, 305, -24, 305, 24); for (int i = -5; i <= 5; i++) { p.L(i * 56, -22, i * 56, 22, W, i == 0 ? 7 : 4); if (i % 5 == 0) p.T(i.ToString(), 44, i * 56, 62, W); } p.D(MathF.Sin(t) * 200, -52, 22, C); p.F(C, MathF.Sin(t) * 200 - 16, -40, MathF.Sin(t) * 200 + 16, -40, MathF.Sin(t) * 200, -12); break;
            case 28: { p.L(-310, 0, 310, 0, W, 6); p.L(0, 260, 0, -260, W, 6); p.F(W, 330, 0, 298, -18, 298, 18); p.F(W, 0, -280, -18, -248, 18, -248); var q = new List<Vector2>(); for (int i = 0; i <= 60; i++) { float x = -280 + i * 9.33f; q.Add(new(x, -MathF.Sin(x * .022f + t) * 150)); } p.Line(C, 9, q); break; }
            default: { float h = 110 + w0 * 300; p.R(0, -50, 112, 460, 56, W); p.D(0, 200, 98, W); p.R(0, -50, 72, 430, 36, p.C(0x39414D)); p.D(0, 200, 74, C); p.B(0, 170 - h / 2f, 42, h, C); for (int i = 0; i < 7; i++) p.L(62, -240 + i * 56, 96, -240 + i * 56, W, 5); break; }
        }
    }

    void WeatherMark(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        bool sun = (a & 1) != 0, moon = (a & 2) != 0, cloud = (a & 4) != 0, dark = (a & 8) != 0, rain = (a & 16) != 0, heavy = (a & 32) != 0, snow = (a & 64) != 0, bolt = (a & 128) != 0, fog = (a & 256) != 0, wind = (a & 512) != 0, swirl = (a & 1024) != 0;
        float sx = cloud ? -130 : 0, sy = cloud ? -120 : 0, sr = cloud ? 112 : 165;
        if (sun) { for (int i = 0; i < 10; i++) { float m = i * .6283f + t * .3f; p.L(sx + MathF.Cos(m) * (sr + 28), sy + MathF.Sin(m) * (sr + 28), sx + MathF.Cos(m) * (sr + 74), sy + MathF.Sin(m) * (sr + 74), p.C(0xFFC93C), 15); } p.D(sx, sy, sr, p.C(0xFFC93C)); }
        if (moon) { var q = new List<Vector2>(); float z = cloud ? .68f : 1f; for (int i = 0; i <= 40; i++) { float m = .95f + i * .1095f; q.Add(new(sx + MathF.Cos(m) * 170 * z, sy + MathF.Sin(m) * 170 * z)); } for (int i = 1; i < 40; i++) { float u = -1f + i / 20f; q.Add(new(sx + (98 - 136 * (1 - u * u)) * z, sy + u * 139 * z)); } p.Fill(p.C(0xFFE58A), q); }
        if (swirl) { for (int k = 0; k < 3; k++) p.Arc(0, 0, 70 + k * 72, t * 1.2f + k * 2.1f, t * 1.2f + k * 2.1f + 4.1f, k % 2 == 0 ? C : D, 34); p.D(0, 0, 34, W); }
        if (cloud)
        {
            var c0 = dark ? p.C(0x6B7482) : p.C(0xE6EBF2);
            p.D(-125, 22, 92, c0); p.D(0, -32, 132, c0); p.D(135, 22, 100, c0); p.R(5, 78, 440, 110, 54, c0);
        }
        float ry = cloud ? 150 : -120;
        if (rain || heavy) { int n = heavy ? 9 : 5; for (int i = 0; i < n; i++) { float k = ArtPen.Fr(t * 1.1f + i * .37f), x = -170 + i * 340f / Math.Max(1, n - 1), y = ry + k * 120; p.L(x + 14, y, x - 8, y + 50, p.C(0x4FB8F2, 1f - k * .5f), heavy ? 15 : 12); } }
        if (snow) for (int i = 0; i < 5; i++) { float k = ArtPen.Fr(t * .55f + i * .31f), x = -170 + i * 85 + MathF.Sin(t + i) * 10, y = ry + k * 130; p.Star(x, y, 25, 6, .32f, t, p.C(0xFFFFFF, 1f - k * .4f)); }
        if (bolt) p.F(p.C(0xFFD23F), 35, 95, -62, 214, 2, 214, -42, 318, 88, 178, 16, 178, 74, 95);
        if (fog) for (int i = 0; i < 4; i++) { float o = MathF.Sin(t + i) * 22; p.R(o, (cloud ? 170 : -90) + i * 56, 480 - i % 2 * 90, 24, 12, p.C(0xC8D0D8, .85f)); }
        if (wind) for (int i = 0; i < 3; i++) { float y = -100 + i * 100, o = MathF.Sin(t * 1.6f + i) * 24; p.L(-240 + o, y, 120 + o, y, W, 18); p.Arc(120 + o, y - 52, 52, -2.6f, 1.57f, W, 18); }
    }
}
