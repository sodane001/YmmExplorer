using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 220番 2001〜2200(Q6Art の種類 24〜36)
//   ローマ数字・ギリシャ文字・音階・トランプ・サイコロの目・画面の部品・キー・基本図形・付箋・看板・ゲーム表示・数学の記号・単位
// =====================================================================
internal static partial class Models3D
{
    static List<Vector3> Close3(List<Vector2> pts, float z) { var l = pts.Select(v => new Vector3(v.X, v.Y, z)).ToList(); l.Add(l[0]); return l; }
    static float Fr1(float x) => x - MathF.Floor(x);

    static void Roman3D(Sculpt m, Vector4 C, Vector4 D, string s)
    {
        var W = new Vector4(1, 1, 1, C.W);
        m.Box(0, 0, 0, 400, 460, 60, C, .12f);
        m.Tube(RoundRectPath(360, 420, 18, 32), 4, D);
        m.Box(0, -190, 32, 300, 20, 8, D, .3f); m.Box(0, 190, 32, 300, 20, 8, D, .3f);
        m.Text(s, FitSize(s, 330, 230), 0, 0, 32, 20, W, 330);
    }

    static void Greek3D(Sculpt m, Vector4 C, Vector4 D, string s)
    {
        var W = new Vector4(1, 1, 1, C.W); var q = s.Split(' ', 2);
        m.Disc(0, 0, 0, 240, 240, 70, C, .3f); m.Torus(0, 0, 34, 222, 6, D);
        m.Text(q[0], 250, 0, -30, 36, 22, W);
        if (q.Length > 1) { m.Box(0, 150, 36, 300, 70, 16, D, .5f); m.Text(q[1], FitSize(q[1], 280, 52), 0, 150, 46, 6, W, 280); }
    }

    static void Note3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        var W = new Vector4(1, 1, 1, C.W);
        m.Box(0, -10, 0, 620, 380, 40, C, .1f);
        for (int i = 0; i < 5; i++) m.Box(-90, -60 + i * 30, 22, 380, 5, 4, W, .3f);
        float y = 90 - a * 15 + MathF.Sin(t * 3f) * 4f;
        if (a == 0) m.Box(-60, 90, 22, 80, 5, 4, W, .3f);
        m.Ball(-60, y, 40, 32, 23, 20, D);
        m.Limb(-32, y, 40, -32, y - 120, 40, 5, 5, D);
        m.Disc(190, -10, 22, 76, 76, 24, D, .3f);
        m.Text(s, FitSize(s, 120, 80), 190, -10, 36, 8, W, 120);
    }

    static void Suit3D(Sculpt m, float x, float y, float z, float r, int suit, Vector4 col)
    {
        float d = MathF.Max(6f, r * .3f);
        switch (suit)
        {
            case 0: m.Slab(Q6Art.HeartLocal(x, y - r * .15f, r, true), z, d, col); m.Slab(z, d, col, x, y + r * .2f, x - r * .38f, y + r * 1.05f, x + r * .38f, y + r * 1.05f); break;
            case 1: m.Slab(Q6Art.HeartLocal(x, y, r, false), z, d, col); break;
            case 2: m.Slab(z, d, col, x, y - r * 1.2f, x + r * .8f, y, x, y + r * 1.2f, x - r * .8f, y); break;
            default:
                m.Disc(x, y - r * .5f, z, r * .5f, r * .5f, d, col, .4f); m.Disc(x - r * .52f, y + r * .12f, z, r * .5f, r * .5f, d, col, .4f); m.Disc(x + r * .52f, y + r * .12f, z, r * .5f, r * .5f, d, col, .4f);
                m.Slab(z, d, col, x, y, x - r * .38f, y + r * 1.05f, x + r * .38f, y + r * 1.05f); break;
        }
    }

    static void Card3D(Sculpt m, float t, Vector4 C, Vector4 D, int suit, string s)
    {
        var W = new Vector4(1, 1, 1, C.W);
        m.Turn(0, 0, MathF.Sin(t * 1.2f) * .04f, () =>
        {
            m.Box(0, 0, 0, 360, 500, 16, W, .06f);
            m.Tube(RoundRectPath(336, 476, 22, 9), 2, D);
            Suit3D(m, 0, 0, 14, 110, suit, C);
            m.Text(s, 64, -125, -190, 10, 4, C); Suit3D(m, -125, -122, 10, 20, suit, C);
            m.Turn(0, 0, MathF.PI, () => { m.Text(s, 64, -125, -190, 10, 4, C); Suit3D(m, -125, -122, 10, 20, suit, C); });
        });
    }

    static void DiceFace3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        m.Yaw(0, 0, 0, MathF.Sin(t * .8f) * .25f, () =>
        {
            m.Box(0, 0, 0, 440, 440, 440, C, .2f);
            DiePips(m, 0, 0, 0, 220, a, a is 1 or 6 ? 2 : 1, D, m.C(0xE53935));
        });
    }

    static void Widget3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = new Vector4(1, 1, 1, C.W);
        switch (a)
        {
            case 0: case 1:
                m.Box(0, 0, 0, 300, 300, 60, a == 1 ? C : W, .2f); m.Tube(RoundRectPath(286, 286, 34, 32), 7, C);
                if (a == 1) m.Tube([V3(-90, 0, 40), V3(-25, 70, 40), V3(100, -80, 40)], 17, W);
                break;
            case 2: case 3: m.Disc(0, 0, 0, 150, 150, 50, W, .3f); m.Torus(0, 0, 0, 150, 18, C); if (a == 3) m.Ball(0, 0, 30, 80, 80, 40, C); break;
            case 4: case 5: m.Box(0, 0, 0, 520, 240, 80, a == 5 ? C : D, .95f); m.Ball(a == 5 ? 140 : -140, 0, 50, 100, W); break;
            case 6: case 7: case 8:
                {
                    float v = (a - 5) * .25f, x = -320 + 640 * v;
                    m.Box(0, 0, 0, 640, 40, 30, D, .9f); m.Box(-320 + 320 * v, 0, 6, 640 * v, 40, 32, C, .9f);
                    m.Disc(x, 0, 30, 62, 62, 40, W, .4f); m.Torus(x, 0, 30, 62, 8, C); break;
                }
            case 9: case 10: case 11: case 12: case 13:
                {
                    int n = a - 8;
                    for (int i = 0; i < 5; i++) { float pop = i == n - 1 ? 1f + .1f * MathF.Max(0f, MathF.Sin(t * 4f)) : 1f; Puff(m, 0, 24, i < n ? C : D, StarPts(-240 + i * 120, 0, 60 * pop, 5, .45f, -MathF.PI / 2f)); }
                    break;
                }
            case 14: { float k = 1f + .08f * MathF.Max(0f, MathF.Sin(t * 5f)); m.Slab(Q6Art.HeartLocal(0, -10, 200 * k, false), 0, 90, C); m.Ball(-70 * k, -90 * k, 50, 26 * k, 22 * k, 10, new Vector4(1, 1, 1, .5f * C.W)); break; }
            case 15:
                m.Box(0, 0, 0, 640, 140, 50, W, .95f); m.Tube(RoundRectPath(630, 130, 64, 26), 4, D);
                m.Torus(-230, -6, 30, 40, 7, C); m.Limb(-202, 22, 30, -170, 54, 30, 8, 8, C);
                m.Box(40, 0, 28, 340 * (.3f + .7f * Fr1(t * .25f)), 22, 6, D, .9f); break;
            case 16:
                {
                    float f = Fr1(t * .1f);
                    m.Slab(0, 30, C, -320, -130, -320, -40, -245, -85);
                    m.Box(0, 40, 0, 640, 30, 24, D, .9f); m.Box(-320 + 320 * f, 40, 4, 640 * f, 30, 26, C, .9f); m.Ball(-320 + 640 * f, 40, 20, 34, W);
                    break;
                }
            case 17: for (int i = 0; i < 8; i++) { float b = i * MathF.Tau / 8f, k = Fr1(i / 8f - t * .9f); m.Ball(MathF.Cos(b) * 150, MathF.Sin(b) * 150, 0, 22 + 16 * k, Sculpt.Mix(D, C, k)); } break;
            case 18: m.Disc(0, 0, 0, 180, 180, 70, C, .35f); m.Limb(-75, -75, 40, 75, 75, 40, 15, 15, W); m.Limb(75, -75, 40, -75, 75, 40, 15, 15, W); break;
            default: m.Box(0, 0, 0, 360, 360, 70, C, .3f); for (int i = -1; i <= 1; i++) m.Box(0, i * 85, 40, 220, 34, 16, W, .9f); break;
        }
    }

    static void Key3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        float w = a == 1 ? 600 : 320, press = Fr1(t * .7f) < .12f ? 24f : 0f;
        var K = m.C(0x20242B);
        m.Box(0, 20, 0, w, 300, 80, D, .25f);
        m.Box(0, -6, 70 - press, w - 44, 246, 60, C, .3f);
        m.Text(s, FitSize(s, w - 90, 120), 0, -6, 102 - press, 8, K, w - 90);
    }

    static void Basic3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        m.Move(0, MathF.Sin(t * 1.5f) * 8f, () =>
        {
            switch (a)
            {
                case 5: m.Disc(0, 0, 0, 250, 250, 80, C, .2f); m.Torus(0, 0, 0, 250, 8, D); return;
                case 6: m.Disc(0, 0, 0, 290, 180, 80, C, .2f); m.Tube(Enumerable.Range(0, 41).Select(i => V3(MathF.Cos(i * MathF.Tau / 40f) * 290, MathF.Sin(i * MathF.Tau / 40f) * 180, 0)).ToList(), 8, D); return;
                case 17: m.Torus(0, 0, 0, 190, 46, C); return;
                case 14:
                    {
                        // 三日月は細長くへこんだ形なので、外の弧と内の弧のあいだを小さな四角に分けて板にする
                        var q = Q6Art.BasicPts(14)!; int n = q.Count / 2;
                        for (int k = 0; k + 1 < n; k++) m.Slab([q[k], q[k + 1], q[q.Count - 2 - k], q[q.Count - 1 - k]], 0, 80, C);
                        m.Tube(Close3(q, 0), 8, D);
                        return;
                    }
            }
            var pts = Q6Art.BasicPts(a)!;
            m.Slab(pts, 0, 80, C);
            m.Tube(Close3(pts, 0), 8, D);
        });
    }

    static void Sticky3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        var K = m.C(0x20242B);
        m.Turn(0, -190, MathF.Sin(t * 1.2f) * .03f, () =>
        {
            if (a == 1)
            {
                m.Slab(0, 8, C, -190, -190, 190, -190, 190, 90, 90, 190, -190, 190);
                m.Slab(10, 6, Sculpt.Dark(C, .82f), 190, 90, 90, 90, 90, 190);
            }
            else { m.Box(0, 0, 0, 380, 380, 8, C, .04f); m.Box(0, -170, 4, 380, 40, 4, Sculpt.Dark(C, .93f), .04f); }
            if (a == 2) m.Turn(0, -190, -.12f, () => m.Box(0, -190, 8, 170, 60, 4, Sculpt.Mix(D, new Vector4(1, 1, 1, D.W), .3f) * new Vector4(1, 1, 1, .75f), .05f));
            if (a == 3) { m.Ball(0, -160, 30, 26, D); m.Limb(0, -160, 30, 0, -160, 0, 4, 2, m.C(0xB0B0B0)); }
            m.Text(s, FitSize(s, 320, 110), 0, 20, 6, 4, K, 320);
        });
    }

    static void Sign3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        var W = new Vector4(1, 1, 1, C.W); var K = m.C(0x20242B);
        switch (a)
        {
            case 0:
                m.Turn(0, -300, MathF.Sin(t * 1.3f) * .05f, () =>
                {
                    m.Limb(-160, -300, 0, -200, -120, 0, 4, 4, D); m.Limb(160, -300, 0, 200, -120, 0, 4, 4, D); m.Ball(0, -300, 0, 16, D);
                    m.Box(0, 0, 0, 540, 250, 40, C, .15f); m.Tube(RoundRectPath(500, 210, 16, 22), 4, D);
                    m.Text(s, FitSize(s, 450, 120), 0, 0, 22, 10, W, 450);
                });
                break;
            case 1:
                m.Limb(-200, -150, -40, -260, 280, -80, 9, 9, D); m.Limb(200, -150, -40, 260, 280, -80, 9, 9, D);
                m.Limb(-200, -150, -40, -240, 280, 60, 9, 9, D); m.Limb(200, -150, -40, 240, 280, 60, 9, 9, D);
                m.Pitch(0, -40, 0, .12f, () => { m.Box(0, -40, 0, 460, 320, 24, D, .1f); m.Box(0, -40, 14, 420, 280, 6, C, .05f); m.Text(s, FitSize(s, 380, 110), 0, -40, 18, 6, W, 380); });
                break;
            case 2:
                m.Box(0, 0, 0, 640, 250, 50, K, .12f);
                for (int y = -3; y <= 3; y += 2) for (int x = -14; x <= 14; x += 2) m.Ball(x * 21, y * 30, 26, 3, Sculpt.Mix(K, D, .35f));
                m.Text(s, FitSize(s, 600, 120), 0, 0, 30, 10, C, 600);
                break;
            case 3:
                m.Box(0, 0, 0, 560, 230, 46, C, .25f);
                for (int i = -2; i <= 2; i++) m.Box(0, i * 38, 24, 500, 3, 2, Sculpt.Dark(C, .75f), .3f);
                m.Tube(RoundRectPath(560, 230, 30, 24), 6, D); m.Ball(-240, -80, 26, 12, D); m.Ball(240, -80, 26, 12, D);
                m.Text(s, FitSize(s, 460, 120), 0, 10, 28, 8, K, 460);
                break;
            default:
                {
                    float fl = Fr1(t * .5f) < .05f ? .4f : 1f;
                    m.Box(0, 0, 0, 620, 270, 40, K, .12f);
                    m.Tube(RoundRectPath(580, 230, 26, 24), 7, C * new Vector4(1, 1, 1, fl));
                    m.Text(s, FitSize(s, 520, 120), 0, 0, 24, 12, C * new Vector4(1, 1, 1, fl), 520);
                    break;
                }
        }
    }

    static void Hud3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = new Vector4(1, 1, 1, C.W); var K = m.C(0x20242B);
        void Bar(string label, float v, float h)
        {
            m.Text(label, 70, -300, 0, 0, 16, W, 0, K, 4);
            m.Box(80, 0, 0, 540, h, 40, K, .9f); m.Box(80 - 270 + 270 * v, 0, 10, 540 * v, h - 14, 30, C, .9f); m.Tube(RoundRectPath(540, h, h / 2f, 0), 4, W);
        }
        switch (a)
        {
            case 0: Bar("HP", .45f + .4f * (.5f + .5f * MathF.Sin(t * .8f)), 64); break;
            case 1: Bar("MP", .3f + .5f * (.5f + .5f * MathF.Sin(t * .6f + 1f)), 64); break;
            case 2: Bar("EXP", Fr1(t * .12f), 36); break;
            case 3: m.Box(0, 0, 0, 440, 210, 60, C, .3f); m.Tube(RoundRectPath(410, 180, 30, 32), 4, W); m.Text("Lv." + (12 + (int)(t / 4f) % 88), 120, 0, 0, 32, 14, W, 400); break;
            case 4: m.Disc(-190, 0, 0, 82, 82, 30, C, .4f); m.Torus(-190, 0, 16, 64, 6, D); m.Text("×" + (128 + (int)(t * 3f)), 110, 90, 0, 0, 16, W, 0, K, 5); break;
            case 5:
                {
                    int sec = Math.Max(0, 99 - (int)t);
                    m.Torus(-210, 0, 0, 70, 7, W); m.Limb(-210, 0, 10, -210, -45, 10, 5, 5, W); m.Limb(-210, 0, 10, -175, 15, 10, 5, 5, W);
                    m.Text($"{sec / 60:00}:{sec % 60:00}", 120, 80, 0, 0, 16, sec < 10 ? C : W, 0, K, 5); break;
                }
            case 6: for (int i = 0; i < 3; i++) m.Slab(Q6Art.HeartLocal(-180 + i * 150, -10, 62, false), 0, 40, C); m.Text("×3", 90, 300, 0, 0, 14, W, 0, K, 4); break;
            case 7: m.Text("SCORE", 60, 0, -80, 0, 10, C, 0, K, 3); m.Text(((int)(t * 250f) % 1000000).ToString("000000"), 130, 0, 40, 0, 18, W, 0, K, 5); break;
            case 8:
                {
                    float v = .25f + .7f * (.5f + .5f * MathF.Cos(t * .5f));
                    m.Text("BOSS", 64, 0, -80, 0, 12, C, 0, K, 4);
                    m.Box(0, 10, 0, 820, 54, 40, K, .2f); m.Box(-400 + 400 * v, 10, 10, 800 * v, 40, 30, C, .2f); m.Tube(RoundRectPath(820, 54, 10, 0), 4, W); break;
                }
            default:
                {
                    float k = 1f + .15f * MathF.Max(0f, 1f - Fr1(t) * 4f);
                    m.Zoom(0, 0, k, k, () => { m.Text((12 + (int)t).ToString(), 220, -40, -20, 0, 30, C, 0, K, 7); m.Text("COMBO", 70, 0, 120, 0, 12, W, 0, K, 4); });
                    break;
                }
        }
    }

    static void Tile3D(Sculpt m, Vector4 C, Vector4 D, string s, bool wide)
    {
        var W = new Vector4(1, 1, 1, C.W);
        if (wide) { m.Box(0, 0, 0, 520, 240, 70, C, .9f); m.Tube(RoundRectPath(488, 208, 104, 36), 4, D); m.Text(s, FitSize(s, 440, 170), 0, 0, 36, 20, W, 440); }
        else { m.Box(0, 0, 0, 420, 420, 80, C, .3f); m.Tube(RoundRectPath(380, 380, 44, 42), 4, D); m.Text(s, FitSize(s, 320, 270), 0, 0, 42, 20, W, 320); }
    }

    /// <summary>2001〜2200 の図形を、種類と番号から描く</summary>
    static void Art8_3D(Sculpt m, float t, Vector4 C, Vector4 D, string s, int kind, int a)
    {
        if (s.Length == 0) s = " ";
        switch (kind)
        {
            case 24: Roman3D(m, C, D, s); break;
            case 25: Greek3D(m, C, D, s); break;
            case 26: Note3D(m, t, C, D, a, s); break;
            case 27: Card3D(m, t, C, D, a, s); break;
            case 28: DiceFace3D(m, t, C, D, a); break;
            case 29: Widget3D(m, t, C, D, a); break;
            case 30: Key3D(m, t, C, D, a, s); break;
            case 31: Basic3D(m, t, C, D, a); break;
            case 32: Sticky3D(m, t, C, D, a, s); break;
            case 33: Sign3D(m, t, C, D, a, s); break;
            case 34: Hud3D(m, t, C, D, a); break;
            case 35: Tile3D(m, C, D, s, false); break;
            default: Tile3D(m, C, D, s, true); break;
        }
    }

    static partial void RegisterL1a(Dictionary<Type, QuickModel> d)
    {
        FamT(d, typeof(X2001Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2002Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2003Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2004Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2005Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2006Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2007Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2008Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2009Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2010Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2011Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2012Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 24, 0));
        FamT(d, typeof(X2013Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2014Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2015Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2016Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2017Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2018Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2019Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2020Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2021Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2022Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2023Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2024Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2025Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2026Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2027Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2028Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2029Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2030Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2031Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2032Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2033Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2034Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2035Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2036Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 25, 0));
        FamT(d, typeof(X2037Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 26, 0));
        FamT(d, typeof(X2038Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 26, 1));
        FamT(d, typeof(X2039Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 26, 2));
        FamT(d, typeof(X2040Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 26, 3));
        FamT(d, typeof(X2041Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 26, 4));
        FamT(d, typeof(X2042Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 26, 5));
        FamT(d, typeof(X2043Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 26, 6));
        FamT(d, typeof(X2044Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 26, 7));
        FamT(d, typeof(X2045Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 0));
        FamT(d, typeof(X2046Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 0));
        FamT(d, typeof(X2047Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 0));
        FamT(d, typeof(X2048Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 0));
        FamT(d, typeof(X2049Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 0));
        FamT(d, typeof(X2050Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 1));
        FamT(d, typeof(X2051Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 1));
        FamT(d, typeof(X2052Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 1));
        FamT(d, typeof(X2053Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 1));
        FamT(d, typeof(X2054Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 1));
        FamT(d, typeof(X2055Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 2));
        FamT(d, typeof(X2056Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 2));
        FamT(d, typeof(X2057Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 2));
        FamT(d, typeof(X2058Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 2));
        FamT(d, typeof(X2059Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 2));
        FamT(d, typeof(X2060Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 3));
        FamT(d, typeof(X2061Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 3));
        FamT(d, typeof(X2062Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 3));
        FamT(d, typeof(X2063Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 3));
        FamT(d, typeof(X2064Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 27, 3));
        FamT(d, typeof(X2065Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 28, 1));
        FamT(d, typeof(X2066Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 28, 2));
        FamT(d, typeof(X2067Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 28, 3));
        FamT(d, typeof(X2068Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 28, 4));
        FamT(d, typeof(X2069Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 28, 5));
        FamT(d, typeof(X2070Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 28, 6));
        FamT(d, typeof(X2071Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 0));
        FamT(d, typeof(X2072Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 1));
        FamT(d, typeof(X2073Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 2));
        FamT(d, typeof(X2074Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 3));
        FamT(d, typeof(X2075Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 4));
        FamT(d, typeof(X2076Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 5));
        FamT(d, typeof(X2077Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 6));
        FamT(d, typeof(X2078Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 7));
        FamT(d, typeof(X2079Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 8));
        FamT(d, typeof(X2080Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 9));
        FamT(d, typeof(X2081Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 10));
        FamT(d, typeof(X2082Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 11));
        FamT(d, typeof(X2083Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 12));
        FamT(d, typeof(X2084Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 13));
        FamT(d, typeof(X2085Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 14));
        FamT(d, typeof(X2086Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 15));
        FamT(d, typeof(X2087Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 16));
        FamT(d, typeof(X2088Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 17));
        FamT(d, typeof(X2089Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 18));
        FamT(d, typeof(X2090Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 29, 19));
        FamT(d, typeof(X2091Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 1));
        FamT(d, typeof(X2092Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 1));
        FamT(d, typeof(X2093Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 1));
        FamT(d, typeof(X2094Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 1));
        FamT(d, typeof(X2095Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2096Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2097Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2098Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2099Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2100Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2101Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2102Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2103Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2104Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2105Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2106Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2107Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2108Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2109Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2110Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2111Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 0));
        FamT(d, typeof(X2112Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 1));
        FamT(d, typeof(X2113Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 2));
        FamT(d, typeof(X2114Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 3));
        FamT(d, typeof(X2115Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 4));
        FamT(d, typeof(X2116Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 5));
        FamT(d, typeof(X2117Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 6));
        FamT(d, typeof(X2118Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 7));
        FamT(d, typeof(X2119Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 8));
        FamT(d, typeof(X2120Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 9));
        FamT(d, typeof(X2121Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 10));
        FamT(d, typeof(X2122Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 11));
        FamT(d, typeof(X2123Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 12));
        FamT(d, typeof(X2124Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 13));
        FamT(d, typeof(X2125Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 14));
        FamT(d, typeof(X2126Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 15));
        FamT(d, typeof(X2127Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 16));
        FamT(d, typeof(X2128Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 17));
        FamT(d, typeof(X2129Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 18));
        FamT(d, typeof(X2130Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 31, 19));
        FamT(d, typeof(X2131Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 0));
        FamT(d, typeof(X2132Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 0));
        FamT(d, typeof(X2133Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 0));
        FamT(d, typeof(X2134Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 0));
        FamT(d, typeof(X2135Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 0));
        FamT(d, typeof(X2136Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 1));
        FamT(d, typeof(X2137Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 1));
        FamT(d, typeof(X2138Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 1));
        FamT(d, typeof(X2139Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 1));
        FamT(d, typeof(X2140Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 1));
        FamT(d, typeof(X2141Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 2));
        FamT(d, typeof(X2142Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 2));
        FamT(d, typeof(X2143Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 2));
        FamT(d, typeof(X2144Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 2));
        FamT(d, typeof(X2145Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 2));
        FamT(d, typeof(X2146Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 3));
        FamT(d, typeof(X2147Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 3));
        FamT(d, typeof(X2148Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 3));
        FamT(d, typeof(X2149Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 3));
        FamT(d, typeof(X2150Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 32, 3));
        FamT(d, typeof(X2151Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 0));
        FamT(d, typeof(X2152Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 1));
        FamT(d, typeof(X2153Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 2));
        FamT(d, typeof(X2154Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 3));
        FamT(d, typeof(X2155Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 4));
        FamT(d, typeof(X2156Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 0));
        FamT(d, typeof(X2157Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 1));
        FamT(d, typeof(X2158Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 2));
        FamT(d, typeof(X2159Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 3));
        FamT(d, typeof(X2160Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 33, 4));
        FamT(d, typeof(X2161Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 0));
        FamT(d, typeof(X2162Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 1));
        FamT(d, typeof(X2163Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 2));
        FamT(d, typeof(X2164Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 3));
        FamT(d, typeof(X2165Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 4));
        FamT(d, typeof(X2166Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 5));
        FamT(d, typeof(X2167Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 6));
        FamT(d, typeof(X2168Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 7));
        FamT(d, typeof(X2169Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 8));
        FamT(d, typeof(X2170Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 34, 9));
        FamT(d, typeof(X2171Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2172Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2173Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2174Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2175Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2176Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2177Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2178Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2179Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2180Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2181Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2182Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2183Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2184Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2185Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2186Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2187Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2188Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2189Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2190Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 35, 0));
        FamT(d, typeof(X2191Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2192Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2193Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2194Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2195Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2196Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2197Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2198Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2199Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
        FamT(d, typeof(X2200Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 36, 0));
    }
}
