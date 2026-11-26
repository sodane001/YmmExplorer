using System.Numerics;
using Vortice.Mathematics;

namespace YMM4EffectShapes;

// =====================================================================
//  図形 2001〜2200 の追加分(Q6Art の種類 24〜36)
//   24=ローマ数字 25=ギリシャ文字 26=音階 27=トランプ 28=サイコロの目 29=画面の部品 30=キーボードのキー
//   31=基本図形 32=付箋 33=看板 34=ゲームの画面表示 35=数学の記号 36=単位
// =====================================================================
public abstract partial class Q6Art
{
    /// <summary>ハートの形(r=大きさ。flip=true で上下反対=スペードの上の部分)</summary>
    internal static List<Vector2> HeartLocal(float x, float y, float r, bool flip)
    {
        var l = new List<Vector2>(32);
        for (int k = 0; k < 32; k++)
        {
            float b = k * MathF.Tau / 32f, hx = MathF.Pow(MathF.Sin(b), 3), hy = -(13f * MathF.Cos(b) - 5f * MathF.Cos(2 * b) - 2f * MathF.Cos(3 * b) - MathF.Cos(4 * b)) / 16f - .1f;
            l.Add(new(x + hx * r, y + (flip ? -hy : hy) * r));
        }
        return l;
    }

    static List<Vector2> RegPts(float r, int n, float rot, float inner = 1f)
    {
        bool star = inner < .999f; int m = star ? n * 2 : n; var l = new List<Vector2>(m);
        for (int j = 0; j < m; j++) { float a0 = rot + j * MathF.Tau / m, rr = star && (j & 1) == 1 ? r * inner : r; l.Add(new(MathF.Cos(a0) * rr, MathF.Sin(a0) * rr)); }
        return l;
    }

    /// <summary>基本図形の輪郭(円・だ円・リングは null)</summary>
    internal static List<Vector2>? BasicPts(int a)
    {
        static List<Vector2> P(params float[] xy) { var l = new List<Vector2>(xy.Length / 2); for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(new(xy[i], xy[i + 1])); return l; }
        switch (a)
        {
            case 0: return RegPts(240, 3, -MathF.PI / 2f).Select(v => v + new Vector2(0, 40)).ToList();
            case 1: return RegPts(250, 4, MathF.PI / 4f);
            case 2: return RegPts(240, 5, -MathF.PI / 2f);
            case 3: return RegPts(240, 6, 0f);
            case 4: return RegPts(240, 8, MathF.PI / 8f);
            case 7: return P(0, -260, 180, 0, 0, 260, -180, 0);
            case 8: return P(-260, -150, 160, -150, 260, 150, -160, 150);
            case 9: return P(-130, -150, 130, -150, 260, 150, -260, 150);
            case 10: return RegPts(250, 5, -MathF.PI / 2f, .42f);
            case 11: return RegPts(250, 6, -MathF.PI / 2f, .55f);
            case 12: { float w = 82f, r = 240f; return P(-w, -r, w, -r, w, -w, r, -w, r, w, w, w, w, r, -w, r, -w, w, -r, w, -r, -w, -w, -w); }
            case 13: return HeartLocal(0, -10, 240, false);
            case 14:
                {
                    var l = new List<Vector2>();
                    for (int k = 0; k <= 30; k++) { float b = .75f + k * (MathF.Tau - 1.5f) / 30f; l.Add(new(MathF.Cos(b) * 240f, MathF.Sin(b) * 240f)); }
                    for (int k = 0; k <= 30; k++) { float b = MathF.Tau - 1.05f - k * (MathF.Tau - 2.1f) / 30f; l.Add(new(110f + MathF.Cos(b) * 205f, MathF.Sin(b) * 205f)); }
                    return l;
                }
            case 15: { var l = new List<Vector2>(); for (int k = 0; k <= 30; k++) { float b = k * MathF.PI / 30f; l.Add(new(MathF.Cos(b) * 260f, MathF.Sin(b) * 260f - 90f)); } return l; }
            case 16: { var l = new List<Vector2> { new(0, 140) }; for (int k = 0; k <= 24; k++) { float b = -MathF.PI / 2f - 1f + k * 2f / 24f; l.Add(new(MathF.Cos(b) * 320f, 140f + MathF.Sin(b) * 320f)); } return l; }
            case 18: return P(-200, -220, 240, 0, -200, 220, -80, 0);
            case 19: { var l = new List<Vector2> { new(0, -270) }; for (int k = 0; k <= 30; k++) { float b = -.4f + k * (MathF.PI + .8f) / 30f; l.Add(new(MathF.Cos(b) * 170f, 70f + MathF.Sin(b) * 170f)); } return l; }
            default: return null;
        }
    }

    void Art8(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W, string s)
    {
        if (s.Length == 0) s = " ";
        switch (kind)
        {
            case 24: // ローマ数字: 石の札
                p.R(0, 0, 400, 460, 26, C); p.RL(0, 0, 360, 420, 18, D, 8); p.R(0, -190, 300, 20, 8, D); p.R(0, 190, 300, 20, 8, D);
                Fit(p, s, 0, 0, W, 330, 230); break;
            case 25: // ギリシャ文字: 丸い札と読み方
                {
                    var q = s.Split(' ', 2);
                    p.D(0, 0, 240, C); p.Ring(0, 0, 222, D, 10); p.T(q[0], 250, 0, -30, W);
                    if (q.Length > 1) { p.R(0, 150, 300, 70, 35, D); Fit(p, q[1], 0, 150, W, 280, 52); }
                    break;
                }
            case 26: Note(p, t, C, D, W, s); break;
            case 27: Card(p, t, C, D, W, s); break;
            case 28: DiceFace(p, C, D); break;
            case 29: Widget(p, t, C, D, W); break;
            case 30: KeyCap(p, t, C, D, K, s); break;
            case 31: Basic(p, t, C, D); break;
            case 32: Sticky(p, t, C, D, K, s); break;
            case 33: Sign(p, t, C, D, K, W, s); break;
            case 34: Hud(p, t, C, D, K, W); break;
            case 35: p.R(0, 0, 420, 420, 60, C); p.RL(0, 0, 380, 380, 44, D, 8); Fit(p, s, 0, 0, W, 320, 270); break;
            default: p.R(0, 0, 520, 240, 120, C); p.RL(0, 0, 488, 208, 104, D, 8); Fit(p, s, 0, 0, W, 440, 170); break;
        }
    }

    /// <summary>音階: 五線の上の音符(a=0 ド 〜 7 高いド)と、右の丸い札</summary>
    void Note(ArtPen p, float t, Color4 C, Color4 D, Color4 W, string s)
    {
        p.R(0, -10, 620, 380, 30, C);
        for (int i = 0; i < 5; i++) p.L(-280, -60 + i * 30, 100, -60 + i * 30, W, 4);
        float y = 90 - a * 15 + MathF.Sin(t * 3f) * 4f;
        if (a == 0) p.L(-100, 90, -20, 90, W, 4);
        p.E(-60, y, 30, 22, D); p.L(-32, y, -32, y - 120, D, 7);
        p.D(190, -10, 76, D); Fit(p, s, 190, -10, W, 120, 80);
    }

    /// <summary>トランプのマーク(0 スペード 1 ハート 2 ダイヤ 3 クラブ)</summary>
    static void Suit2D(ArtPen p, float x, float y, float r, int suit, Color4 col)
    {
        switch (suit)
        {
            case 0: p.Fill(col, HeartLocal(x, y - r * .15f, r, true)); p.F(col, x, y + r * .2f, x - r * .38f, y + r * 1.05f, x + r * .38f, y + r * 1.05f); break;
            case 1: p.Fill(col, HeartLocal(x, y, r, false)); break;
            case 2: p.F(col, x, y - r * 1.2f, x + r * .8f, y, x, y + r * 1.2f, x - r * .8f, y); break;
            default:
                p.D(x, y - r * .5f, r * .5f, col); p.D(x - r * .52f, y + r * .12f, r * .5f, col); p.D(x + r * .52f, y + r * .12f, r * .5f, col);
                p.F(col, x, y, x - r * .38f, y + r * 1.05f, x + r * .38f, y + r * 1.05f); break;
        }
    }

    /// <summary>トランプ: a=マーク、文字=数字。Color1=マークの色</summary>
    void Card(ArtPen p, float t, Color4 C, Color4 D, Color4 W, string s)
    {
        p.Turn(0, 0, MathF.Sin(t * 1.2f) * .04f, () =>
        {
            p.R(0, 0, 360, 500, 30, W); p.RL(0, 0, 336, 476, 22, D, 4);
            Suit2D(p, 0, 0, 110, a, C);
            p.T(s, 64, -125, -190, C); Suit2D(p, -125, -122, 20, a, C);
            p.Turn(0, 0, MathF.PI, () => { p.T(s, 64, -125, -190, C); Suit2D(p, -125, -122, 20, a, C); });
        });
    }

    /// <summary>サイコロの目(a=1〜6)。Color1=サイコロの色、Color2=目の色(1は赤)</summary>
    void DiceFace(ArtPen p, Color4 C, Color4 D)
    {
        p.R(8, 12, 440, 440, 70, p.C(0x000000, .2f)); p.R(0, 0, 440, 440, 70, C); p.RL(0, 0, 440, 440, 70, p.C(D, .25f), 4);
        float o = 115;
        (float, float)[] at = a switch
        {
            1 => [(0, 0)],
            2 => [(-o, -o), (o, o)],
            3 => [(-o, -o), (0, 0), (o, o)],
            4 => [(-o, -o), (o, -o), (-o, o), (o, o)],
            5 => [(-o, -o), (o, -o), (0, 0), (-o, o), (o, o)],
            _ => [(-o, -o), (o, -o), (-o, 0), (o, 0), (-o, o), (o, o)],
        };
        foreach (var (x, y) in at) p.D(x, y, a == 1 ? 72 : 44, a == 1 ? p.C(0xE53935) : D);
    }

    /// <summary>画面の部品(a=0〜19)</summary>
    void Widget(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        switch (a)
        {
            case 0: case 1:
                p.R(0, 0, 300, 300, 40, a == 1 ? C : W); p.RL(0, 0, 300, 300, 40, C, 14);
                if (a == 1) p.P(W, 34, -90, 0, -25, 70, 100, -80); break;
            case 2: case 3: p.D(0, 0, 150, W); p.Ring(0, 0, 150, C, 18); if (a == 3) p.D(0, 0, 80, C); break;
            case 4: case 5: p.R(0, 0, 520, 240, 120, a == 5 ? C : D); p.D(a == 5 ? 140 : -140, 0, 100, W); break;
            case 6: case 7: case 8:
                {
                    float v = (a - 5) * .25f, x = -320 + 640 * v;
                    p.R(0, 0, 640, 40, 20, D); p.R(-320 + 320 * v, 0, 640 * v, 40, 20, C); p.D(x, 0, 62, W); p.Ring(x, 0, 62, C, 10); break;
                }
            case 9: case 10: case 11: case 12: case 13:
                {
                    int n = a - 8;
                    for (int i = 0; i < 5; i++) { float pop = i == n - 1 ? 1f + .1f * MathF.Max(0f, MathF.Sin(t * 4f)) : 1f; p.Star(-240 + i * 120, 0, 60 * pop, 5, .45f, -MathF.PI / 2f, i < n ? C : D); }
                    break;
                }
            case 14: { float k = 1f + .08f * MathF.Max(0f, MathF.Sin(t * 5f)); p.Fill(C, HeartLocal(0, -10, 200 * k, false)); p.Fill(p.C(0xFFFFFF, .35f), HeartLocal(-70 * k, -80 * k, 40 * k, false)); break; }
            case 15:
                p.R(0, 0, 640, 140, 70, W); p.RL(0, 0, 640, 140, 70, D, 8); p.Ring(-230, -6, 40, C, 12); p.L(-202, 22, -170, 54, C, 14);
                p.R(40, 0, 340 * (.3f + .7f * ArtPen.Fr(t * .25f)), 22, 11, D); break;
            case 16:
                {
                    float f = ArtPen.Fr(t * .1f);
                    p.F(C, -320, -130, -320, -40, -245, -85);
                    p.R(0, 40, 640, 30, 15, D); p.R(-320 + 320 * f, 40, 640 * f, 30, 15, C); p.D(-320 + 640 * f, 40, 34, W); p.Ring(-320 + 640 * f, 40, 34, C, 6);
                    break;
                }
            case 17: for (int i = 0; i < 8; i++) { float b = i * MathF.Tau / 8f, k = ArtPen.Fr(i / 8f - t * .9f); p.D(MathF.Cos(b) * 150, MathF.Sin(b) * 150, 22 + 16 * k, p.C(C, .25f + .75f * k)); } break;
            case 18: p.D(0, 0, 180, C); p.L(-75, -75, 75, 75, W, 30); p.L(75, -75, -75, 75, W, 30); break;
            default: p.R(0, 0, 360, 360, 50, C); for (int i = -1; i <= 1; i++) p.R(0, i * 85, 220, 34, 17, W); break;
        }
    }

    /// <summary>キーボードのキー(a=1 で横長)。ときどき押し込まれる</summary>
    void KeyCap(ArtPen p, float t, Color4 C, Color4 D, Color4 K, string s)
    {
        float w = a == 1 ? 600 : 320, h = a == 2 ? 600 : 300, press = ArtPen.Fr(t * .7f) < .12f ? 16f : 0f;
        p.R(0, 24, w, h, 44, D);
        p.R(0, -6 + press, w - 44, h - 54, 36, C);
        Fit(p, s, 0, -6 + press, K, w - 90, 120);
    }

    /// <summary>基本図形(a=0〜19)。Color1=塗り、Color2=ふち</summary>
    void Basic(ArtPen p, float t, Color4 C, Color4 D)
    {
        p.Move(0, MathF.Sin(t * 1.5f) * 8f, () =>
        {
            switch (a)
            {
                case 5: p.D(0, 0, 250, C); p.Ring(0, 0, 250, D, 14); return;
                case 6: p.E(0, 0, 290, 180, C); p.EL(0, 0, 290, 180, D, 14); return;
                case 17: p.Ring(0, 0, 190, C, 90); p.Ring(0, 0, 238, D, 10); p.Ring(0, 0, 142, D, 10); return;
            }
            var pts = BasicPts(a)!;
            p.Fill(C, pts); var o = new List<Vector2>(pts) { pts[0] }; p.Line(D, 14, o);
        });
    }

    /// <summary>付箋(a=0 四角、1 角折れ、2 テープ、3 ピン)。Color1=紙、Color2=テープやピン</summary>
    void Sticky(ArtPen p, float t, Color4 C, Color4 D, Color4 K, string s)
    {
        p.Turn(0, -190, MathF.Sin(t * 1.2f) * .03f, () =>
        {
            var shade = p.C(0x000000, .18f);
            if (a == 1)
            {
                p.F(shade, -180, -176, 200, -176, 200, 104, 100, 204, -180, 204);
                p.F(C, -190, -190, 190, -190, 190, 90, 90, 190, -190, 190);
                p.F(Mx(C, K, .18f), 190, 90, 90, 90, 90, 190);
            }
            else { p.R(10, 14, 380, 380, 6, shade); p.R(0, 0, 380, 380, 6, C); p.R(0, -170, 380, 40, 6, Mx(C, K, .07f)); }
            if (a == 2) p.Turn(0, -190, -.12f, () => p.R(0, -190, 170, 60, 4, p.C(D, .65f)));
            if (a == 3) { p.D(4, -154, 26, p.C(0x000000, .25f)); p.D(0, -160, 26, D); p.D(-8, -168, 8, p.C(0xFFFFFF, .7f)); }
            Fit(p, s, 0, 20, K, 320, 110);
        });
    }

    /// <summary>看板(a=0 吊り下げ、1 立て看板、2 電光、3 木の札、4 ネオン)</summary>
    void Sign(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W, string s)
    {
        switch (a)
        {
            case 0:
                p.Turn(0, -300, MathF.Sin(t * 1.3f) * .05f, () =>
                {
                    p.L(-160, -300, -200, -120, D, 6); p.L(160, -300, 200, -120, D, 6); p.D(0, -300, 14, D);
                    p.R(0, 0, 540, 250, 24, C); p.RL(0, 0, 500, 210, 16, D, 6); Fit(p, s, 0, 0, W, 450, 120);
                });
                break;
            case 1:
                p.L(-200, -150, -260, 280, D, 18); p.L(200, -150, 260, 280, D, 18);
                p.R(0, -40, 460, 320, 18, D); p.R(0, -40, 420, 280, 12, C); Fit(p, s, 0, -40, W, 380, 110); break;
            case 2:
                {
                    p.R(0, 0, 640, 250, 20, K);
                    for (int y = -3; y <= 3; y++) for (int x = -14; x <= 14; x++) p.D(x * 21, y * 30, 3, p.C(D, .25f));
                    float run = ArtPen.Fr(t * .2f); p.Turn(0, 0, 0f, () => p.T(s, 120, 0, 0, C, p.C(D, .6f), 6, 600));
                    p.R(-320 + 640 * run, 0, 18, 250, 4, p.C(0xFFFFFF, .12f));
                    break;
                }
            case 3:
                p.R(0, 0, 560, 230, 30, C); for (int i = -2; i <= 2; i++) p.L(-250, i * 38, 250, i * 38 + MathF.Sin(i) * 8, p.C(D, .45f), 3);
                p.RL(0, 0, 560, 230, 30, D, 10); p.D(-240, -80, 12, D); p.D(240, -80, 12, D); Fit(p, s, 0, 10, K, 460, 120); break;
            default:
                {
                    float fl = ArtPen.Fr(t * .5f) < .05f ? .4f : 1f;
                    p.R(0, 0, 620, 270, 30, K);
                    p.RL(0, 0, 580, 230, 26, p.C(C, .35f * fl), 26); p.RL(0, 0, 580, 230, 26, p.C(C, fl), 8);
                    p.T(s, 120, 0, 0, p.C(C, fl), p.C(D, .6f * fl), 10, 520);
                    break;
                }
        }
    }

    /// <summary>ゲームの画面表示(a=0〜9)</summary>
    void Hud(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        void Bar(string label, float v, float h)
        {
            p.T(label, 70, -300, 0, W, K, 8);
            p.R(80, 0, 540, h, h / 2f, K); p.R(80 - 270 + 270 * v, 0, 540 * v, h - 14, (h - 14) / 2f, C); p.RL(80, 0, 540, h, h / 2f, W, 5);
        }
        switch (a)
        {
            case 0: Bar("HP", .45f + .4f * (.5f + .5f * MathF.Sin(t * .8f)), 64); break;
            case 1: Bar("MP", .3f + .5f * (.5f + .5f * MathF.Sin(t * .6f + 1f)), 64); break;
            case 2: Bar("EXP", ArtPen.Fr(t * .12f), 36); break;
            case 3: p.R(0, 0, 440, 210, 40, C); p.RL(0, 0, 410, 180, 30, W, 6); p.T("Lv." + (12 + (int)(t / 4f) % 88), 120, 0, 0, W, K, 6); break;
            case 4: p.D(-190, 0, 82, C); p.Ring(-190, 0, 64, D, 10); p.T("×" + (128 + (int)(t * 3f)), 110, 90, 0, W, K, 10); break;
            case 5:
                {
                    int sec = Math.Max(0, 99 - (int)t);
                    p.Ring(-210, 0, 70, W, 12); p.L(-210, 0, -210, -45, W, 10); p.L(-210, 0, -175, 15, W, 10);
                    p.T($"{sec / 60:00}:{sec % 60:00}", 120, 80, 0, sec < 10 ? C : W, K, 10); break;
                }
            case 6: for (int i = 0; i < 3; i++) p.Fill(C, HeartLocal(-180 + i * 150, -10, 62, false)); p.T("×3", 90, 300, 0, W, K, 8); break;
            case 7: p.T("SCORE", 60, 0, -80, C, K, 6); p.T(((int)(t * 250f) % 1000000).ToString("000000"), 130, 0, 40, W, K, 10); break;
            case 8:
                p.T("BOSS", 64, 0, -80, C, K, 8);
                p.R(0, 10, 820, 54, 10, K); p.R(-400 + 400 * (.25f + .7f * (.5f + .5f * MathF.Cos(t * .5f))), 10, 800 * (.25f + .7f * (.5f + .5f * MathF.Cos(t * .5f))), 40, 6, C); p.RL(0, 10, 820, 54, 10, W, 5); break;
            default:
                {
                    float k = 1f + .15f * MathF.Max(0f, 1f - ArtPen.Fr(t) * 4f);
                    p.Zoom(0, 0, k, k, () => { p.T((12 + (int)t).ToString(), 220, -40, -20, C, K, 14); p.T("COMBO", 70, 0, 120, W, K, 8); });
                    break;
                }
        }
    }
}
