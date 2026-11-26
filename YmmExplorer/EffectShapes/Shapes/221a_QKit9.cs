using System.Numerics;
using Vortice.Mathematics;

namespace YMM4EffectShapes;

// =====================================================================
//  図形 2201〜2400 の追加分(Q6Art の種類 37〜53)
//   37=旗 38=宝石 39=メダル・トロフィー 40=風船 41=プレゼント箱 42=信号・ランプ 43=ゲームパッドの部品 44=時間の道具
//   45=花 46=パズルのピース 47=通知バッジ 48=フローチャート部品 49=音符 50=一文字札 51=チェスの駒 52=将棋の駒 53=ボール
//   形の計算(輪郭・断面)は internal static にして、専用3Dモデル(Models_L1b.cs)でも同じ形を使う
// =====================================================================
public abstract partial class Q6Art
{
    internal static List<Vector2> PP(params float[] xy) { var l = new List<Vector2>(xy.Length / 2); for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(new(xy[i], xy[i + 1])); return l; }
    static Color4 Sh(Color4 c, float f) => new(c.R * f, c.G * f, c.B * f, c.A);
    static Color4 Al(Color4 c, float a) => new(c.R, c.G, c.B, c.A * a);

    // ---------------------------------------------------------------- 形の計算(3Dと共通)
    /// <summary>旗の、横の位置 u(0〜1)の縦の帯(上, 下, 色 0=Color1 1=Color2 2=白)</summary>
    internal static void FlagColumn(int shape, float u, int col, List<(float y0, float y1, int c)> o)
    {
        o.Clear();
        switch (shape)
        {
            case 0: o.Add((-280 + u * 120, -40 - u * 120, 0)); break;
            case 1: { float g = MathF.Max(0f, (u - .78f) / .22f) * 110f; o.Add((-280, -160 - g, 0)); o.Add((-160 + g, -40, 0)); break; }
            case 2: for (int r = 0; r < 4; r++) o.Add((-280 + r * 60, -220 + r * 60, ((r + col) & 1) == 0 ? 0 : 2)); break;
            default: o.Add((-280, -200, 0)); o.Add((-200, -120, 2)); o.Add((-120, -40, 1)); break;
        }
    }

    /// <summary>宝石の輪郭(0 ラウンド 1 エメラルド 2 ハート 3 しずく)</summary>
    internal static List<Vector2> GemOutline(int cut) => cut switch
    {
        0 => RegPts(230, 12, 0f),
        1 => PP(-150, -230, 150, -230, 215, -165, 215, 165, 150, 230, -150, 230, -215, 165, -215, -165),
        2 => HeartLocal(0, -10, 240, false).Where((_, i) => i % 2 == 0).ToList(),
        _ => Enumerable.Range(0, 14).Select(k => k == 0 ? new Vector2(0, -260) : new Vector2(MathF.Cos(-.5f + (k - 1) * (MathF.PI + 1f) / 12f) * 175f, 70f + MathF.Sin(-.5f + (k - 1) * (MathF.PI + 1f) / 12f) * 175f)).ToList(),
    };

    /// <summary>パズルのピース(a=0 と 1 で出っぱりの向きが違う)</summary>
    internal static List<Vector2> PuzzlePts(int a)
    {
        var l = new List<Vector2>(); const float h = 160, r = 50;
        int[] tabs = a == 0 ? [1, 1, -1, -1] : [-1, 1, 1, -1];
        Vector2[] c = [new(-h, -h), new(h, -h), new(h, h), new(-h, h)];
        for (int e = 0; e < 4; e++)
        {
            var A = c[e]; var B = c[(e + 1) % 4]; var d = Vector2.Normalize(B - A); var n = new Vector2(d.Y, -d.X); var mid = (A + B) / 2f;
            l.Add(A); l.Add(mid - d * r * .7f);
            var M = mid + n * tabs[e] * r * .75f;
            for (int k = 0; k <= 14; k++) { float f = MathF.PI * (1.2f - 1.4f * k / 14f); l.Add(M + d * MathF.Cos(f) * r + n * tabs[e] * MathF.Sin(f) * r); }
            l.Add(mid + d * r * .7f);
        }
        return l;
    }

    /// <summary>フローチャートの形(結合子は null)</summary>
    internal static List<Vector2>? FlowPts(int a)
    {
        switch (a)
        {
            case 0: case 9: return PP(-240, -130, 240, -130, 240, 130, -240, 130);
            case 1: return PP(0, -170, 270, 0, 0, 170, -270, 0);
            case 2: { var l = new List<Vector2>(); for (int k = 0; k <= 12; k++) { float b = -MathF.PI / 2f + k * MathF.PI / 12f; l.Add(new(130 + MathF.Cos(b) * 130, MathF.Sin(b) * 130)); } for (int k = 0; k <= 12; k++) { float b = MathF.PI / 2f + k * MathF.PI / 12f; l.Add(new(-130 + MathF.Cos(b) * 130, MathF.Sin(b) * 130)); } return l; }
            case 3: return PP(-180, -130, 270, -130, 180, 130, -270, 130);
            case 4: { var l = PP(-240, -150, 240, -150, 240, 100); for (int k = 0; k <= 16; k++) { float x = 240 - k * 30; l.Add(new(x, 100 + MathF.Sin(k / 16f * MathF.Tau) * 30)); } return l; }
            case 5: { var l = new List<Vector2>(); for (int k = 0; k <= 12; k++) { float b = MathF.PI + k * MathF.PI / 12f; l.Add(new(MathF.Cos(b) * 200, -120 + MathF.Sin(b) * 60)); } for (int k = 0; k <= 12; k++) { float b = k * MathF.PI / 12f; l.Add(new(MathF.Cos(b) * 200, 120 + MathF.Sin(b) * 60)); } return l; }
            case 7: return PP(-240, -60, 240, -140, 240, 130, -240, 130);
            case 8: return PP(-180, -130, 180, -130, 270, 0, 180, 130, -180, 130, -270, 0);
            default: return null;
        }
    }

    /// <summary>チェスの駒の断面(半径, 高さ)を上から。0 キング 1 クイーン 2 ルーク 3 ビショップ 4 ナイト(台だけ) 5 ポーン</summary>
    internal static float[] ChessProfile(int piece) => piece switch
    {
        0 => [0, -210, 90, -200, 85, -160, 68, -120, 90, -100, 55, -85, 65, 120, 120, 170, 150, 215, 150, 250, 0, 250],
        1 => [0, -250, 28, -240, 30, -215, 95, -200, 85, -170, 70, -130, 90, -110, 55, -95, 65, 120, 120, 170, 150, 215, 150, 250, 0, 250],
        2 => [0, -170, 105, -170, 105, -90, 80, -70, 70, 120, 120, 170, 150, 215, 150, 250, 0, 250],
        3 => [0, -230, 22, -215, 30, -200, 60, -160, 78, -105, 62, -60, 85, -45, 50, -30, 60, 120, 115, 170, 145, 215, 145, 250, 0, 250],
        4 => [0, -20, 80, -10, 60, 120, 115, 170, 145, 215, 145, 250, 0, 250],
        _ => [0, -160, 55, -150, 78, -115, 70, -75, 45, -55, 80, -40, 45, -25, 55, 120, 115, 170, 145, 215, 145, 250, 0, 250],
    };
    internal static List<Vector2> KnightHead() => PP(-80, -5, -75, -120, -25, -200, 30, -235, 55, -205, 112, -135, 100, -100, 35, -112, 65, -5);
    internal static List<Vector2> ShogiPts() => PP(0, -250, 150, -190, 185, 250, -185, 250, -150, -190);

    static List<Vector2> Mirror(float[] prof)
    {
        var l = new List<Vector2>();
        for (int i = 0; i + 1 < prof.Length; i += 2) l.Add(new(prof[i], prof[i + 1]));
        for (int i = prof.Length - 2; i >= 0; i -= 2) l.Add(new(-prof[i], prof[i + 1]));
        return l;
    }

    // ---------------------------------------------------------------- 入口
    void Art9(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W, string s)
    {
        if (s.Length == 0) s = " ";
        switch (kind)
        {
            case 37: Flag(p, t, C, D, W); break;
            case 38: Gem2D(p, t, C, W); break;
            case 39: Award(p, t, C, D, W); break;
            case 40: Balloon2D(p, t, C, W); break;
            case 41: Gift(p, t, C, D, K, W); break;
            case 42: Lamp(p, t, C, D, W); break;
            case 43: Pad(p, t, C, D, W); break;
            case 44: TimeTool(p, t, C, D, K, W); break;
            case 45: Flower(p, t, C, D, W); break;
            case 46: { var q = PuzzlePts(a); p.Move(0, MathF.Sin(t * 1.4f) * 8f, () => { p.Fill(C, q); q.Add(q[0]); p.Line(D, 10, q); }); break; }
            case 47:
                {
                    float k = 1f + .12f * MathF.Max(0f, MathF.Sin(t * 4f));
                    p.Zoom(0, 0, k, k, () => { p.D(8, 12, 170, p.C(0x000000, .22f)); p.D(0, 0, 170, C); p.Ring(0, 0, 156, W, 10); Fit(p, s, 0, 0, W, 250, 190); });
                    break;
                }
            case 48: FlowShape(p, C, D, W, s); break;
            case 49: Music(p, t, C); break;
            case 50:
                p.Turn(0, -260, MathF.Sin(t * 1.1f) * .03f, () =>
                {
                    p.R(8, 12, 300, 500, 14, p.C(0x000000, .2f)); p.R(0, 0, 300, 500, 14, Mx(C, W, .75f)); p.RL(0, 0, 270, 470, 10, C, 8);
                    p.D(0, -250, 14, D); Fit(p, s, 0, -20, K, 240, 230); p.R(85, 180, 70, 70, 8, D); p.RL(85, 180, 56, 56, 6, W, 4);
                });
                break;
            case 51: Chess(p, t, C, D); break;
            case 52:
                {
                    var q = ShogiPts();
                    p.Fill(p.C(0x000000, .2f), q.Select(v => v + new Vector2(8, 12)).ToList()); p.Fill(C, q); q.Add(q[0]); p.Line(D, 8, q);
                    Fit(p, s, 0, 30, a == 1 ? p.C(0xC62828) : K, 260, 240); break;
                }
            default: Ball2D(p, t, C, D, K, W); break;
        }
    }

    // ---------------------------------------------------------------- 描き方
    void Flag(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        p.L(-226, -310, -226, 320, p.C(0x8A8F98), 14); p.D(-226, -318, 16, p.C(0xF2C14E));
        const int n = 12; const float len = 430;
        var sa = new List<(float, float, int)>(); var sb = new List<(float, float, int)>();
        for (int i = 0; i < n; i++)
        {
            float u0 = i / (float)n, u1 = (i + 1f) / n, x0 = -220 + u0 * len, x1 = -220 + u1 * len + 1f;
            float w0 = MathF.Sin(u0 * 5f - t * 4f) * 16f * u0, w1 = MathF.Sin(u1 * 5f - t * 4f) * 16f * u1, sh = .82f + .18f * MathF.Cos(u0 * 5f - t * 4f);
            FlagColumn(a, u0, i / 2, sa); FlagColumn(a, u1, i / 2, sb);
            for (int k = 0; k < sa.Count; k++)
            {
                var c = sa[k].Item3 == 0 ? C : sa[k].Item3 == 1 ? D : W;
                p.F(Sh(c, sh), x0, sa[k].Item1 + w0, x1, sb[k].Item1 + w1, x1, sb[k].Item2 + w1, x0, sa[k].Item2 + w0);
            }
        }
    }

    void Gem2D(ArtPen p, float t, Color4 C, Color4 W)
    {
        var o = GemOutline(a); var inner = o.Select(v => v * .55f).ToList();
        p.Fill(Sh(C, .8f), o); p.Fill(Mx(C, W, .3f), inner);
        for (int i = 0; i < o.Count; i++)
        {
            int j = (i + 1) % o.Count;
            if (i % 2 == 0) p.F(Mx(C, W, .12f), o[i].X, o[i].Y, o[j].X, o[j].Y, inner[j].X, inner[j].Y, inner[i].X, inner[i].Y);
            p.L(o[i].X, o[i].Y, inner[i].X, inner[i].Y, Al(W, .45f), 3);
        }
        var oo = new List<Vector2>(o) { o[0] }; p.Line(Al(W, .6f), 4, oo);
        float tw = MathF.Max(0f, MathF.Sin(t * 2.5f));
        p.Star(90, -110, 50 * tw, 4, .2f, 0f, W);
    }

    void Award(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        float sp = MathF.Max(0f, MathF.Sin(t * 2f));
        switch (a)
        {
            case 0:
                p.F(p.C(0xD7263D), -110, -300, -30, -300, 40, -60, -40, -60); p.F(p.C(0x2F86C6), 110, -300, 30, -300, -40, -60, 40, -60);
                p.D(0, 60, 170, C); p.Ring(0, 60, 140, D, 12); p.Star(0, 60, 90, 5, .45f, -MathF.PI / 2f, D); break;
            case 1:
                p.Ring(-170, -120, 70, D, 22); p.Ring(170, -120, 70, D, 22);
                p.F(C, -170, -230, 170, -230, 120, -60, 40, 20, -40, 20, -120, -60);
                p.R(0, 70, 60, 120, 8, D); p.R(0, 150, 220, 44, 8, C); p.R(0, 210, 300, 80, 10, D); p.R(0, 210, 180, 30, 6, Mx(D, W, .4f));
                p.Star(0, -110, 55, 5, .45f, -MathF.PI / 2f, Mx(C, W, .35f)); break;
            case 2:
                p.F(C, -200, -250, 200, -250, 200, 0, 0, 260, -200, 0); p.F(D, -160, -210, 160, -210, 160, -10, 0, 200, -160, -10);
                p.Star(0, -40, 90, 5, .45f, -MathF.PI / 2f, C); break;
            case 3:
                p.F(C, -230, 120, -230, -120, -115, 0, 0, -170, 115, 0, 230, -120, 230, 120);
                p.R(0, 150, 470, 70, 10, D);
                foreach (var (x, y) in new[] { (-230f, -140f), (0f, -190f), (230f, -140f) }) p.D(x, y, 26, Mx(C, W, .3f));
                p.D(-120, 150, 18, p.C(0xD7263D)); p.D(0, 150, 22, p.C(0x2F86C6)); p.D(120, 150, 18, p.C(0x35A86B)); break;
            default:
                p.F(D, -90, 60, -20, 60, -60, 330, -110, 290); p.F(D, 90, 60, 20, 60, 60, 330, 110, 290);
                for (int i = 0; i < 16; i++) { float b = i * MathF.Tau / 16f; p.D(MathF.Cos(b) * 160, -40 + MathF.Sin(b) * 160, 52, i % 2 == 0 ? C : Mx(C, W, .25f)); }
                p.D(0, -40, 140, C); p.D(0, -40, 105, W); p.Star(0, -40, 70, 5, .45f, -MathF.PI / 2f, C); break;
        }
        p.Star(150, -200, 40 * sp, 4, .2f, 0f, W);
    }

    void Balloon2D(ArtPen p, float t, Color4 C, Color4 W)
    {
        float bob = MathF.Sin(t * 1.5f) * 12f;
        var str = new List<Vector2>(); for (int i = 0; i <= 20; i++) { float y = 130 + i * 10; str.Add(new(MathF.Sin(i * .5f + t * 2f) * 12f, y + bob * (1f - i / 20f))); }
        p.Line(p.C(0x9AA3AD), 4, str);
        p.Turn(0, 130, MathF.Sin(t * 1.2f) * .06f, () =>
        {
            if (a == 0) { p.E(0, -70 + bob, 170, 205, C); p.E(-60, -150 + bob, 36, 56, Al(W, .5f)); }
            else { p.Fill(C, HeartLocal(0, -70 + bob, 210, false)); p.E(-80, -150 + bob, 30, 46, Al(W, .5f)); }
            p.F(Sh(C, .8f), 0, 118 + bob, -18, 140 + bob, 18, 140 + bob);
        });
    }

    void Gift(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        float hop = MathF.Max(0f, MathF.Sin(t * 3f)) * 14f;
        p.Move(0, -hop, () =>
        {
            p.R(0, 90, 380, 300, 10, C);
            if (a == 1) for (int i = -3; i <= 3; i++) p.F(Al(W, .3f), -190 + (i + 3) * 60, -60, -160 + (i + 3) * 60, -60, -220 + (i + 4) * 60, 240, -250 + (i + 4) * 60, 240);
            p.R(0, -80, 420, 80, 10, Mx(C, K, .1f));
            p.R(0, 90, 60, 300, 4, D); p.R(0, -80, 60, 80, 4, D);
            p.Turn(-70, -150, -.4f, () => p.E(-70, -150, 80, 42, D)); p.Turn(70, -150, .4f, () => p.E(70, -150, 80, 42, D));
            p.D(0, -130, 30, Sh(D, .85f));
        });
    }

    void Lamp(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        float pulse = .75f + .25f * MathF.Sin(t * 3f);
        if (a <= 3)
        {
            Color4[] cols = [p.C(0xE53935), p.C(0xFFC107), p.C(0x2ECC71)];
            p.R(0, a == 3 ? 40 : 0, 560, a == 3 ? 300 : 220, 40, p.C(0x2B2F36));
            for (int i = 0; i < 3; i++)
            {
                bool on = i == a || (a == 3 && i == 2 && false); float x = -170 + i * 170;
                p.D(x, 0, 74, p.C(0x14171C));
                if (on) { p.D(x, 0, 96, Al(cols[i], .25f * pulse)); p.D(x, 0, 68, cols[i]); } else p.D(x, 0, 68, Sh(cols[i], .3f));
            }
            if (a == 3) { p.D(0, 130, 50, Al(cols[2], .3f * pulse)); p.R(-12, 130, 50, 18, 4, cols[2]); p.F(cols[2], 40, 130, 8, 104, 8, 156); }
            return;
        }
        p.D(0, 40, 210, p.C(0x5B6270)); p.D(0, 40, 175, p.C(0x2B2F36));
        p.D(0, 20, 240 * pulse, Al(C, .2f)); p.D(0, 20, 150, C); p.E(-50, -40, 45, 30, Al(W, .55f));
    }

    void Pad(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        var dark = p.C(0x3A3F47); int dir = (int)(t * 1.5f) % 4; bool press = ArtPen.Fr(t * .8f) < .15f; float k = press ? .92f : 1f;
        switch (a)
        {
            case 0:
                p.F(dark, -70, -230, 70, -230, 70, -70, 230, -70, 230, 70, 70, 70, 70, 230, -70, 230, -70, 70, -230, 70, -230, -70, -70, -70);
                for (int i = 0; i < 4; i++)
                {
                    float b = i * MathF.PI / 2f - MathF.PI / 2f, cx = MathF.Cos(b) * 150, cy = MathF.Sin(b) * 150;
                    p.F(i == dir ? C : Mx(dark, W, .4f), cx + MathF.Cos(b) * 40, cy + MathF.Sin(b) * 40, cx - MathF.Sin(b) * 35, cy + MathF.Cos(b) * 35, cx + MathF.Sin(b) * 35, cy - MathF.Cos(b) * 35);
                }
                p.D(0, 0, 40, Sh(dark, .8f)); break;
            case 1: case 2: case 3: case 4:
                p.Zoom(0, 0, k, k, () => { p.D(6, 10, 190, p.C(0x000000, .25f)); p.D(0, 0, 190, C); p.D(-50, -60, 70, Al(W, .2f)); p.T(new[] { "A", "B", "X", "Y" }[a - 1], 220, 0, 0, W); });
                break;
            case 5: case 6:
                p.Zoom(0, 0, k, k, () => { p.R(0, 0, 460, 170, 85, C); p.R(0, -40, 380, 50, 25, Al(W, .2f)); p.T(a == 5 ? "L" : "R", 140, 0, 0, W); });
                break;
            case 7:
                {
                    float sx = MathF.Cos(t * 2f) * 60, sy = MathF.Sin(t * 2f) * 60;
                    p.D(0, 0, 220, dark); p.Ring(0, 0, 200, Mx(dark, W, .2f), 8); p.D(sx * .5f, sy * .5f, 120, Sh(dark, .7f)); p.D(sx, sy, 140, C); p.Ring(sx, sy, 110, Al(W, .3f), 10);
                    break;
                }
            default:
                p.Zoom(0, 0, k, k, () => { p.R(0, -40, 300, 110, 55, dark); p.R(0, -40, 260, 74, 37, C); p.T(a == 8 ? "START" : "SELECT", 74, 0, 110, W, dark, 6); });
                break;
        }
    }

    void TimeTool(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        switch (a)
        {
            case 0:
                {
                    float f = ArtPen.Fr(t * .25f);
                    p.R(0, -250, 340, 40, 12, D); p.R(0, 250, 340, 40, 12, D); p.L(-150, -230, -150, 230, D, 14); p.L(150, -230, 150, 230, D, 14);
                    p.F(Al(W, .25f), -120, -230, 120, -230, 12, 0, 120, 230, -120, 230, -12, 0);
                    float ht = (1f - f) * 210f; p.F(C, -120 * ht / 230f, -ht, 120 * ht / 230f, -ht, 10, -8, -10, -8);
                    float hb = f * 150f; p.F(C, -120, 230, 120, 230, 120 * (1f - hb / 230f), 230 - hb, -120 * (1f - hb / 230f), 230 - hb);
                    if (f < .97f) p.L(0, -6, 0, 230 - hb, C, 6);
                    var gl = PP(-120, -230, 120, -230, 12, 0, 120, 230, -120, 230, -12, 0); gl.Add(gl[0]); p.Line(Al(W, .6f), 6, gl);
                    break;
                }
            case 1:
                {
                    p.R(0, -250, 70, 50, 10, D); p.D(0, 20, 240, C); p.D(0, 20, 205, W);
                    for (int i = 0; i < 12; i++) { float b = i * MathF.Tau / 12f; p.L(MathF.Cos(b) * 170, 20 + MathF.Sin(b) * 170, MathF.Cos(b) * 190, 20 + MathF.Sin(b) * 190, K, i % 3 == 0 ? 8 : 4); }
                    float h = t * 1.2f - MathF.PI / 2f; p.L(0, 20, MathF.Cos(h) * 165, 20 + MathF.Sin(h) * 165, C, 8); p.D(0, 20, 16, K);
                    break;
                }
            case 2:
                p.Turn(0, 20, MathF.Sin(t * 30f) * .05f * (ArtPen.Fr(t * .3f) < .4f ? 1f : 0f), () =>
                {
                    p.Pie(-150, -170, 80, MathF.PI, MathF.Tau, D); p.Pie(150, -170, 80, MathF.PI, MathF.Tau, D);
                    p.L(-120, 200, -170, 270, D, 18); p.L(120, 200, 170, 270, D, 18);
                    p.D(0, 20, 220, C); p.D(0, 20, 180, W);
                    float h = t * .5f; p.L(0, 20, MathF.Cos(h - 1.6f) * 100, 20 + MathF.Sin(h - 1.6f) * 100, K, 12); p.L(0, 20, MathF.Cos(h * 12f - 1.6f) * 150, 20 + MathF.Sin(h * 12f - 1.6f) * 150, K, 7);
                    p.D(0, 20, 14, K);
                });
                break;
            case 3:
                {
                    p.R(0, 0, 440, 440, 90, C); p.D(0, 0, 160, W);
                    float ang = -ArtPen.Fr(t * .1f) * MathF.Tau - MathF.PI / 2f;
                    for (int i = 0; i < 12; i++) { float b = i * MathF.Tau / 12f; p.L(MathF.Cos(b) * 130, MathF.Sin(b) * 130, MathF.Cos(b) * 150, MathF.Sin(b) * 150, K, 5); }
                    p.Pie(0, 0, 120, -MathF.PI / 2f, ang, Al(D, .6f)); p.D(0, 0, 50, D); p.R(0, -40, 26, 70, 10, Mx(D, W, .3f));
                    break;
                }
            default:
                {
                    p.R(0, 0, 640, 280, 40, p.C(0x1B1F26)); p.RL(0, 0, 610, 250, 30, Al(C, .4f), 6);
                    bool colon = ArtPen.Fr(t) < .5f; int m = (int)(t / 60f) % 60, sec = (int)t % 60;
                    p.T($"{12 + m / 60:00}{(colon ? ":" : " ")}{34 + m:00}", 170, -40, 0, C, Al(C, .35f), 10);
                    p.T($"{sec:00}", 70, 220, 50, Al(C, .8f));
                    break;
                }
        }
    }

    void Flower(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        var green = p.C(0x43A047);
        void Petal(float ang, float dist, float len, float wid, Color4 c) => p.Turn(0, 0, ang, () => p.E(dist, 0, len, wid, c));
        float sway = MathF.Sin(t * 1.2f) * .05f;
        p.Turn(0, 0, sway, () =>
        {
            switch (a)
            {
                case 0: for (int i = 0; i < 14; i++) Petal(i * MathF.Tau / 14f, 130, 110, 32, C); p.D(0, 0, 70, D); break;
                case 1:
                    for (int i = 0; i < 5; i++) { float b = i * MathF.Tau / 5f - MathF.PI / 2f; p.Turn(0, 0, b + MathF.PI / 2f, () => p.Fill(C, HeartLocal(0, -125, 115, false))); }
                    p.D(0, 0, 40, D); for (int i = 0; i < 5; i++) { float b = i * MathF.Tau / 5f; p.D(MathF.Cos(b) * 55, MathF.Sin(b) * 55, 10, D); }
                    break;
                case 2:
                    p.L(0, 0, 0, 300, green, 18); p.Turn(0, 230, -.6f, () => p.E(-60, 200, 90, 30, green));
                    p.E(-70, -60, 80, 140, Sh(C, .85f)); p.E(70, -60, 80, 140, Sh(C, .85f)); p.E(0, -50, 90, 160, C);
                    break;
                case 3:
                    for (int i = 0; i < 22; i++) Petal(i * MathF.Tau / 22f + .14f, 175, 85, 26, Sh(C, .85f));
                    for (int i = 0; i < 22; i++) Petal(i * MathF.Tau / 22f, 160, 80, 24, C);
                    p.D(0, 0, 115, D); for (int i = 0; i < 40; i++) { float b = i * 2.39996f, r = 10 + i * 2.4f; p.D(MathF.Cos(b) * r, MathF.Sin(b) * r, 7, Sh(D, .6f)); }
                    break;
                case 4:
                    for (int i = 0; i < 8; i++) { float b = i * MathF.Tau / 8f; Petal(b, 130, 115, 55, C); p.Turn(0, 0, b, () => p.F(W, 235, -14, 250, 0, 235, 14)); }
                    p.D(0, 0, 50, D); break;
                case 5:
                    for (int l = 0; l < 4; l++) for (int i = 0; i < 5; i++) { float b = i * MathF.Tau / 5f + l * .6f + t * .1f; p.D(MathF.Cos(b) * (180 - l * 45), MathF.Sin(b) * (180 - l * 45), 110 - l * 22, Sh(C, .7f + l * .1f)); }
                    { var q = new List<Vector2>(); for (int k = 0; k < 40; k++) { float b = k * .35f, r = 10 + k * 3f; q.Add(new(MathF.Cos(b) * r, MathF.Sin(b) * r)); } p.Line(Sh(C, .5f), 6, q); }
                    break;
                case 6:
                    for (int i = 0; i < 5; i++) Petal(i * MathF.Tau / 5f, 120, 130, 100, C);
                    p.D(0, 0, 50, Sh(C, .6f)); p.L(0, 0, 120, -150, D, 10); for (int i = 0; i < 6; i++) p.D(110 + i * 4, -140 - i * 6, 10, D);
                    break;
                case 7:
                    p.L(0, 0, 40, 300, green, 14);
                    for (int i = 0; i < 4; i++) { float b = i * MathF.Tau / 4f + MathF.PI / 4f; p.Turn(0, 0, b + MathF.PI / 2f, () => p.Fill(C, HeartLocal(0, -105, 105, false))); }
                    p.D(0, 0, 18, Sh(C, .6f)); break;
                case 8:
                    p.L(0, 0, 0, 320, green, 10);
                    for (int i = 0; i < 40; i++) { float b = i * MathF.Tau / 40f, r = 180 + (i % 3) * 15; p.L(0, 0, MathF.Cos(b) * r, MathF.Sin(b) * r, Al(W, .8f), 3); p.D(MathF.Cos(b) * r, MathF.Sin(b) * r, 10, C); }
                    p.D(0, 0, 32, D); break;
                default:
                    for (int i = 0; i < 6; i++) { float b = i * MathF.Tau / 6f; p.Turn(0, 0, b, () => p.F(C, 30, -40, 250, 0, 30, 40, 0, 0)); p.Turn(0, 0, b, () => p.L(40, 0, 200, 0, Sh(C, .7f), 4)); }
                    for (int i = 0; i < 6; i++) { float b = i * MathF.Tau / 6f + .5f; p.L(0, 0, MathF.Cos(b) * 110, MathF.Sin(b) * 110, D, 5); p.D(MathF.Cos(b) * 110, MathF.Sin(b) * 110, 12, D); }
                    break;
            }
        });
    }

    void FlowShape(ArtPen p, Color4 C, Color4 D, Color4 W, string s)
    {
        var q = FlowPts(a);
        if (q == null) { p.D(0, 0, 170, C); p.Ring(0, 0, 170, D, 10); }
        else { p.Fill(C, q); var o = new List<Vector2>(q) { q[0] }; p.Line(D, 10, o); }
        if (a == 5) p.EL(0, -120, 200, 60, D, 8);
        if (a == 9) { p.L(-200, -130, -200, 130, D, 8); p.L(200, -130, 200, 130, D, 8); }
        Fit(p, s, 0, a == 5 ? 30 : a == 4 ? -20 : 0, W, a == 6 ? 240 : 380, 110);
    }

    void Music(ArtPen p, float t, Color4 C)
    {
        float bob = MathF.Abs(MathF.Sin(t * 2.5f)) * -18f;
        p.Move(0, bob, () =>
        {
            void Head(float x, float y, bool hollow) { p.Turn(x, y, -.35f, () => { if (hollow) p.EL(x, y, 70, 48, C, 18); else p.E(x, y, 72, 50, C); }); }
            switch (a)
            {
                case 0: p.Turn(0, 0, -.2f, () => p.EL(0, 0, 112, 70, C, 34)); break;
                case 1: Head(-30, 180, true); p.L(32, 170, 32, -220, C, 14); break;
                case 2: Head(-30, 180, false); p.L(32, 170, 32, -220, C, 14); break;
                case 3: case 4:
                    Head(-30, 180, false); p.L(32, 170, 32, -220, C, 14);
                    for (int f = 0; f < a - 2; f++) p.P(C, 18, 32, -220 + f * 70, 110, -140 + f * 70, 120, -40 + f * 70);
                    break;
                case 5: Head(-140, 200, false); Head(110, 150, false); p.L(-78, 190, -78, -170, C, 14); p.L(172, 140, 172, -220, C, 14); p.F(C, -85, -200, 180, -250, 180, -200, -85, -150); break;
                case 6:
                    {
                        var q = new List<Vector2>();
                        for (int k = 0; k <= 40; k++) { float b = MathF.PI * .5f + k / 40f * MathF.PI * 2.3f, r = 25 + k * 3.2f; q.Add(new(MathF.Cos(b) * r * .9f, 80 + MathF.Sin(b) * r)); }
                        q.Add(new(40, -150)); q.Add(new(60, -250)); q.Add(new(10, -300)); q.Add(new(-35, -230)); q.Add(new(20, 120)); q.Add(new(40, 270)); q.Add(new(0, 310)); q.Add(new(-50, 280));
                        p.Line(C, 20, q); p.D(-45, 270, 26, C);
                        break;
                    }
                case 7: p.Arc(-40, -60, 140, -2.8f, 1.6f, C, 40); p.D(-150, -40, 34, C); p.D(150, -110, 20, C); p.D(150, 0, 20, C); break;
                case 8: p.P(C, 34, -30, -250, 50, -150, -40, -40, 50, 60, -20, 120, 40, 230); break;
                default: p.D(-50, -120, 46, C); p.L(-20, -110, 60, -140, C, 18); p.L(60, -140, -20, 220, C, 18); break;
            }
        });
    }

    void Chess(ArtPen p, float t, Color4 C, Color4 D)
    {
        int piece = a % 6;
        p.Move(0, MathF.Sin(t * 1.4f) * 6f, () =>
        {
            p.E(0, 262, 170, 26, p.C(0x000000, .25f));
            var body = Mirror(ChessProfile(piece)); p.Fill(C, body); body.Add(body[0]); p.Line(D, 8, body);
            switch (piece)
            {
                case 0: p.R(0, -250, 30, 110, 6, C); p.R(0, -260, 100, 30, 6, C); p.RL(0, -250, 30, 110, 6, D, 5); p.RL(0, -260, 100, 30, 6, D, 5); break;
                case 1: for (int i = -2; i <= 2; i++) { p.D(i * 42, -222 - (2 - Math.Abs(i)) * 6, 20, C); p.Ring(i * 42, -222 - (2 - Math.Abs(i)) * 6, 20, D, 5); } p.D(0, -262, 24, C); p.Ring(0, -262, 24, D, 5); break;
                case 2: for (int i = -1; i <= 1; i++) p.R(i * 70, -185, 38, 34, 4, D); break;
                case 3: p.L(-30, -140, 30, -190, D, 8); break;
                case 4: { var h = KnightHead(); p.Fill(C, h); h.Add(h[0]); p.Line(D, 8, h); p.D(30, -160, 12, D); break; }
            }
        });
    }

    void Ball2D(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        float rot = t * .8f;
        switch (a)
        {
            case 4:
                p.Turn(0, 0, -.5f + MathF.Sin(t) * .15f, () =>
                {
                    p.E(0, 0, 260, 170, C); p.EL(0, 0, 260, 170, Sh(C, .7f), 6); p.L(-90, 0, 90, 0, W, 10);
                    for (int i = -3; i <= 3; i++) p.L(i * 24, -22, i * 24, 22, W, 8);
                });
                return;
            case 6:
                for (int i = 0; i < 6; i++) p.Pie(0, 0, 230, rot + i * MathF.Tau / 6f, rot + (i + 1) * MathF.Tau / 6f, i % 3 == 0 ? C : i % 3 == 1 ? D : p.C(0x2F86C6));
                p.D(0, 0, 50, W); p.Ring(0, 0, 230, Al(K, .3f), 4); return;
        }
        p.D(0, 0, 230, C);
        p.Turn(0, 0, rot, () =>
        {
            switch (a)
            {
                case 0: p.L(-230, 0, 230, 0, K, 9); p.L(0, -230, 0, 230, K, 9); p.Arc(-330, 0, 230, -.75f, .75f, K, 9); p.Arc(330, 0, 230, MathF.PI - .75f, MathF.PI + .75f, K, 9); break;
                case 1:
                    foreach (int sx in new[] { -1, 1 })
                    {
                        p.Arc(sx * 330, 0, 210, sx < 0 ? -.8f : MathF.PI - .8f, sx < 0 ? .8f : MathF.PI + .8f, D, 6);
                        for (int i = -5; i <= 5; i++) { float b = (sx < 0 ? 0f : MathF.PI) + i * .14f, x = sx * 330 + MathF.Cos(b) * 210, y = MathF.Sin(b) * 210; p.L(x - 14, y - 8, x + 14, y + 8, D, 5); }
                    }
                    break;
                case 2: { var q = new List<Vector2>(); for (int k = 0; k <= 40; k++) { float u = k / 40f * MathF.Tau; q.Add(new(MathF.Cos(u) * 200, MathF.Sin(u) * 120 * MathF.Sin(u * 2f) + MathF.Sin(u) * 60)); } p.Line(W, 12, q); break; }
                case 3: for (int i = 0; i < 3; i++) { float b = i * MathF.Tau / 3f; p.Turn(0, 0, b, () => { p.Arc(0, -330, 260, 1.05f, 2.09f, D, 40); p.Arc(0, -330, 330, 1.25f, 1.9f, i == 0 ? p.C(0x2F86C6) : D, 30); }); } break;
                case 5: for (int y = -4; y <= 4; y++) for (int x = -4; x <= 4; x++) { float px = x * 48 + (y % 2) * 24, py = y * 46; if (px * px + py * py < 200 * 200) p.D(px, py, 12, Al(K, .15f)); } break;
                default: p.D(-40, -90, 34, Sh(C, .3f)); p.D(50, -110, 30, Sh(C, .3f)); p.D(10, -30, 30, Sh(C, .3f)); break;
            }
        });
        p.E(-80, -100, 70, 40, Al(W, .25f));
    }
}
