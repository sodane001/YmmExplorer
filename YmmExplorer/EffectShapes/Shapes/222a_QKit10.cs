using System.Numerics;
using Vortice.Mathematics;

namespace YMM4EffectShapes;

// =====================================================================
//  図形 2401〜2500 の追加分(Q6Art の種類 54〜63)
//   54=キッチン 55=文房具 56=スイーツ 57=家の小物 58=数字ブロック 59=季節の物 60=状態アイコン 61=惑星 62=スタンプ 63=お金
// =====================================================================
public abstract partial class Q6Art
{
    void Art10(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W, string s)
    {
        if (s.Length == 0) s = " ";
        switch (kind)
        {
            case 54: Kitchen(p, t, C, D, W); break;
            case 55: Stationery(p, t, C, D, K, W); break;
            case 56: Sweets(p, t, C, D, W); break;
            case 57: HomeItem(p, t, C, D, K, W); break;
            case 58:
                {
                    float hop = MathF.Max(0f, MathF.Sin(t * 2.5f)) * 12f;
                    p.Move(0, -hop, () =>
                    {
                        p.F(Mx(C, W, .3f), -200, -200, -120, -280, 280, -280, 200, -200);
                        p.F(Sh(C, .7f), 200, -200, 280, -280, 280, 120, 200, 200);
                        p.R(0, 0, 400, 400, 24, C); p.RL(0, 0, 360, 360, 18, Al(W, .6f), 8);
                        Fit(p, s, 0, 0, W, 300, 280);
                    });
                    break;
                }
            case 59: Season(p, t, C, D, K, W); break;
            case 60: Status(p, t, C, W); break;
            case 61: Planet(p, t, C, D, W); break;
            case 62:
                {
                    float k = 1f + .12f * MathF.Max(0f, 1f - ArtPen.Fr(t * .5f) * 6f);
                    p.Zoom(0, 0, k, k, () => p.Turn(0, 0, -.18f, () =>
                    {
                        p.Ring(0, 0, 220, C, 18); p.Ring(0, 0, 180, C, 6);
                        Fit(p, s, 0, 0, C, 300, 180);
                    }));
                    break;
                }
            default: Money(p, t, C, D, K, W, s); break;
        }
    }

    void Kitchen(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        var metal = p.C(0xB8C0CC);
        switch (a)
        {
            case 0:
                p.Ring(170, 30, 80, D, 30); p.R(0, 40, 320, 340, 40, C); p.R(0, -110, 320, 40, 16, Mx(C, W, .3f));
                for (int i = -1; i <= 1; i++) { var q = new List<Vector2>(); for (int k = 0; k <= 12; k++) { float y = -160 - k * 14; q.Add(new(i * 70 + MathF.Sin(k * .6f + t * 3f + i) * 14, y)); } p.Line(Al(W, .5f * (.5f + .5f * MathF.Sin(t * 2f + i))), 10, q); }
                break;
            case 1:
                p.E(0, 170, 300, 50, D); p.Ring(190, 20, 60, C, 22);
                p.F(C, -200, -60, 200, -60, 150, 130, -150, 130); p.E(0, -60, 200, 40, Mx(C, W, .35f)); p.E(0, -60, 170, 28, p.C(0x8D5A3B)); break;
            case 2: p.E(0, 30, 300, 120, D); p.E(0, 20, 300, 110, W); p.EL(0, 20, 220, 75, Al(D, .5f), 6); break;
            case 3: p.Pie(0, -40, 260, 0, MathF.PI, C); p.E(0, -40, 260, 60, Mx(C, W, .35f)); p.E(0, -40, 220, 45, p.C(0xF2C14E)); break;
            case 4: p.L(150, 0, 330, 0, p.C(0x5D4037), 40); p.D(0, 0, 200, p.C(0x2B2F36)); p.D(0, 0, 170, Sh(C, .4f)); p.E(-60, -40, 50, 30, p.C(0xFFF59D)); p.D(-60, -40, 22, p.C(0xFFB300)); break;
            case 5:
                p.R(0, 60, 420, 260, 30, C); p.R(-250, 0, 80, 30, 12, D); p.R(250, 0, 80, 30, 12, D);
                p.E(0, -80, 230, 40, Mx(C, W, .2f)); p.R(0, -130, 70, 40, 14, D);
                for (int i = -1; i <= 1; i++) p.D(i * 60, -170 - ArtPen.Fr(t * .8f + i * .3f) * 80, 14, Al(W, .5f)); break;
            case 6:
                p.Arc(0, -80, 160, MathF.PI, MathF.Tau, D, 18);
                p.P(C, 34, 170, 60, 280, -40, 330, -50);
                p.Pie(0, 120, 230, MathF.PI, MathF.Tau, C); p.R(0, 140, 460, 60, 20, C); p.D(0, -100, 30, D); break;
            case 7: p.R(0, 110, 50, 300, 25, metal); p.R(0, -100, 150, 60, 20, metal); for (int i = -1; i <= 1; i++) p.R(i * 50, -200, 22, 160, 11, metal); p.R(0, 230, 50, 100, 20, C); break;
            case 8: p.R(0, 120, 50, 300, 25, metal); p.E(0, -150, 100, 140, metal); p.E(0, -150, 76, 112, Mx(metal, W, .35f)); p.R(0, 240, 50, 100, 20, C); break;
            default: p.F(metal, -40, -300, 40, -300, 40, 60, 0, 60); p.F(Mx(metal, W, .4f), -40, -300, 0, -300, 0, 60); p.R(0, 170, 70, 230, 20, C); break;
        }
    }

    void Stationery(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        var metal = p.C(0xB8C0CC); var wood = p.C(0xF2D3A2);
        p.Turn(0, 0, a <= 1 ? -.6f + MathF.Sin(t) * .05f : MathF.Sin(t * .8f) * .04f, () =>
        {
            switch (a)
            {
                case 0: p.R(0, 0, 70, 420, 6, C); p.R(-12, 0, 16, 420, 4, Mx(C, W, .3f)); p.F(wood, -35, -210, 35, -210, 0, -300); p.F(K, -10, -286, 10, -286, 0, -300); p.R(0, 225, 70, 30, 4, metal); p.R(0, 265, 70, 50, 16, D); break;
                case 1: p.R(0, 20, 46, 440, 23, C); p.F(Mx(C, W, .4f), -23, -200, 23, -200, 5, -280, -5, -280); p.R(28, -120, 14, 160, 6, metal); p.D(0, -284, 6, K); break;
                case 2: p.R(0, 0, 360, 170, 20, W); p.R(50, 0, 220, 176, 6, C); p.R(50, 0, 220, 40, 4, D); break;
                case 3:
                    p.R(0, 0, 640, 110, 10, Al(C, .85f));
                    for (int i = 0; i <= 30; i++) p.L(-300 + i * 20, -55, -300 + i * 20, i % 5 == 0 ? -10 : -30, K, i % 10 == 0 ? 4 : 2);
                    break;
                case 4:
                    p.Turn(0, 0, MathF.Sin(t * 3f) * .12f, () => { p.F(metal, 0, 0, 20, -20, 260, -60, 270, -40); });
                    p.Turn(0, 0, -MathF.Sin(t * 3f) * .12f, () => { p.F(Mx(metal, W, .2f), 0, 0, 20, 20, 260, 60, 270, 40); });
                    p.Ring(-120, -70, 60, C, 24); p.Ring(-120, 70, 60, C, 24); p.D(0, 0, 14, D); break;
                case 5:
                    {
                        var q = new List<Vector2> { new(60, 200), new(60, -150) };
                        foreach (var v in Sculpt.ArcPts(0, -150, 60, 0, -MathF.PI, 12)) q.Add(v);
                        q.Add(new(-60, 160));
                        foreach (var v in Sculpt.ArcPts(-15, 160, 45, MathF.PI, 0, 10)) q.Add(v);
                        q.Add(new(30, -110));
                        p.Line(C, 16, q); break;
                    }
                case 6:
                    p.R(0, 60, 460, 120, 20, D); p.Turn(-200, 30, -.08f - MathF.Max(0f, MathF.Sin(t * 3f)) * .08f, () => p.R(0, -20, 480, 90, 30, C)); p.R(180, 50, 60, 30, 8, metal); break;
                case 7:
                    p.R(0, 0, 400, 500, 14, C); p.R(30, 0, 330, 470, 8, W);
                    for (int i = 0; i < 7; i++) p.L(-110, -170 + i * 60, 170, -170 + i * 60, Al(C, .4f), 3);
                    for (int i = 0; i < 8; i++) p.Ring(-170, -210 + i * 60, 22, metal, 8);
                    break;
                case 8: p.L(0, 0, 0, 200, metal, 10); p.D(0, -10, 150, C); p.D(-40, -50, 40, Al(W, .45f)); p.D(0, -10, 80, Sh(C, .8f)); break;
                default:
                    p.F(Al(C, .75f), -250, 230, 250, 230, -250, -270); p.F(p.C(0x000000, .0f), 0, 0);
                    p.F(Al(Mx(C, W, .5f), .6f), -170, 170, 80, 170, -170, -100);
                    var o = PP(-250, 230, 250, 230, -250, -270); o.Add(o[0]); p.Line(D, 6, o); break;
            }
        });
    }

    void Sweets(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        float hop = MathF.Max(0f, MathF.Sin(t * 2.4f)) * 10f;
        p.Move(0, -hop, () =>
        {
            switch (a)
            {
                case 0:
                    p.F(D, -150, 0, -290, -90, -270, 0, -290, 90); p.F(D, 150, 0, 290, -90, 270, 0, 290, 90);
                    p.E(0, 0, 170, 120, C); for (int i = -2; i <= 2; i++) p.Turn(i * 60, 0, .5f, () => p.R(i * 60, 0, 18, 230, 9, Al(W, .45f))); p.E(-60, -50, 40, 20, Al(W, .5f)); break;
                case 1:
                    p.R(0, 200, 26, 300, 10, W);
                    p.D(0, -70, 190, C);
                    { var q = new List<Vector2>(); for (int k = 0; k <= 60; k++) { float b = k * .35f + t, r = k * 3f; q.Add(new(MathF.Cos(b) * r, -70 + MathF.Sin(b) * r)); } p.Line(D, 26, q); }
                    break;
                case 2:
                    p.Ring(0, 0, 150, p.C(0xD9A066), 120); p.Ring(0, 0, 145, C, 100);
                    for (int i = 0; i < 18; i++) { float b = i * 1.7f, r = 120 + (i % 3) * 25; p.Turn(MathF.Cos(b) * r, MathF.Sin(b) * r, b, () => p.R(MathF.Cos(b) * r, MathF.Sin(b) * r, 30, 9, 4, i % 3 == 0 ? D : i % 3 == 1 ? W : p.C(0x2F86C6))); }
                    break;
                case 3:
                    p.F(D, -170, 20, 170, 20, 130, 260, -130, 260); for (int i = -3; i <= 3; i++) p.L(i * 45, 30, i * 37, 250, Al(W, .35f), 6);
                    p.E(0, 0, 200, 70, C); p.E(0, -70, 160, 70, Mx(C, W, .15f)); p.E(0, -140, 110, 60, Mx(C, W, .3f)); p.D(0, -220, 40, p.C(0xD7263D)); break;
                case 4:
                    p.F(p.C(0xD9A066), -120, -20, 120, -20, 0, 300); for (int i = -2; i <= 2; i++) { p.L(i * 50 - 60, -20, i * 50 + 60 * .5f, 200, Al(D, .4f), 4); }
                    p.E(0, -60, 150, 70, C); p.E(0, -130, 115, 60, Mx(C, W, .15f)); p.E(0, -190, 75, 50, Mx(C, W, .3f)); p.F(Mx(C, W, .3f), -30, -220, 30, -220, 0, -280); break;
                case 5: p.E(0, -60, 210, 80, C); p.R(0, 0, 400, 50, 25, D); p.E(0, 60, 210, 80, C); p.E(-70, -90, 50, 20, Al(W, .4f)); break;
                case 6: p.D(0, 0, 210, C); for (int i = 0; i < 9; i++) { float b = i * 2.39996f, r = 40 + i * 16; p.D(MathF.Cos(b) * r, MathF.Sin(b) * r, 22, D); } break;
                case 7:
                    p.R(0, 0, 440, 320, 14, C);
                    for (int y = 0; y < 3; y++) for (int x = 0; x < 4; x++) { p.R(-165 + x * 110, -100 + y * 100, 96, 86, 10, Mx(C, W, .12f)); p.R(-165 + x * 110, -125 + y * 100, 80, 20, 6, Al(W, .15f)); }
                    break;
                case 8:
                    p.F(W, -260, 160, 260, 160, 260, -40, -260, 60); p.R(0, 40, 520, 30, 2, p.C(0xF48FB1)); p.R(0, 110, 520, 26, 2, p.C(0xF2D3A2)); p.F(p.C(0xF2D3A2), -260, 160, 260, 160, 260, 200, -260, 200);
                    p.D(150, -90, 60, C); p.F(p.C(0x43A047), 120, -150, 150, -170, 180, -150, 150, -130); break;
                default:
                    p.E(0, 220, 280, 50, W); p.F(p.C(0xF8D77A), -200, 200, 200, 200, 140, -80, -140, -80);
                    p.F(D, -140, -80, 140, -80, 150, -40, -150, -40); p.E(0, -80, 140, 30, D); p.D(0, -140, 30, C); break;
            }
        });
    }

    void HomeItem(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        var metal = p.C(0xB8C0CC);
        switch (a)
        {
            case 0:
                { float g = .5f + .5f * MathF.Sin(t * 2f); p.D(0, -60, 260 * (.8f + .2f * g), Al(C, .2f * g)); p.D(0, -60, 170, C); p.R(0, 140, 140, 80, 10, metal); for (int i = 0; i < 3; i++) p.L(-70, 110 + i * 25, 70, 110 + i * 25, Sh(metal, .7f), 5); p.R(0, 200, 60, 30, 10, D); break; }
            case 1: p.Ring(-160, 0, 100, C, 50); p.R(110, 0, 380, 50, 10, C); p.R(220, 50, 30, 60, 4, C); p.R(270, 50, 30, 80, 4, C); break;
            case 2:
                p.Turn(0, -60, MathF.Sin(t * 1.5f) * .06f, () =>
                {
                    p.Pie(0, -60, 280, MathF.PI, MathF.Tau, C);
                    for (int i = 0; i < 4; i++) p.Pie(-210 + i * 140, -60, 70, 0, MathF.PI, p.C(0x000000, 0f));
                    for (int i = 0; i < 4; i++) p.L(0, -340, -210 + i * 140, -60, Al(D, .6f), 4);
                    p.L(0, -60, 0, 220, D, 14); p.Arc(-50, 220, 50, 0, MathF.PI, D, 14);
                });
                break;
            case 3: p.R(10, 0, 420, 480, 16, W); p.R(-10, 0, 420, 480, 16, C); p.R(-190, 0, 40, 480, 10, Sh(C, .8f)); p.R(10, -80, 260, 60, 10, Al(W, .8f)); break;
            case 4: p.Ring(-140, 0, 100, C, 20); p.Ring(140, 0, 100, C, 20); p.Arc(0, 10, 40, MathF.PI, MathF.Tau, C, 16); p.L(-240, -10, -310, -40, C, 14); p.L(240, -10, 310, -40, C, 14); p.D(-140, 0, 82, Al(W, .15f)); p.D(140, 0, 82, Al(W, .15f)); break;
            case 5:
                {
                    float fl = MathF.Sin(t * 13f) * 6f;
                    p.R(0, 90, 140, 340, 14, C); p.L(0, -80, 0, -110, K, 6);
                    p.E(fl * .3f, -170, 40 + fl * .5f, 80, p.C(0xFFB300)); p.E(fl * .2f, -150, 20, 45, p.C(0xFFF59D)); break;
                }
            case 6:
                p.Turn(0, -250, MathF.Sin(t * 5f) * .2f, () =>
                {
                    p.D(0, -250, 30, D); p.F(C, -40, -220, 40, -220, 100, 0, 200, 140, -200, 140, -100, 0); p.R(0, 150, 420, 40, 20, Sh(C, .85f));
                    p.D(MathF.Sin(t * 5f) * -20, 200, 40, D);
                });
                break;
            case 7: p.L(110, 110, 290, 290, p.C(0x5D4037), 40); p.D(-40, -40, 210, Al(p.C(0xBFE6FF), .45f)); p.Ring(-40, -40, 200, C, 30); p.Arc(-40, -40, 150, 3.6f, 4.4f, Al(W, .7f), 14); break;
            case 8:
                p.Arc(0, 0, 250, 1.9f, 4.4f, D, 14); p.R(0, 270, 200, 30, 10, D); p.L(0, 230, 0, 270, D, 14);
                p.D(0, 0, 210, p.C(0x2F86C6));
                p.Turn(0, 0, 0f, () => { p.E(-60 + MathF.Sin(t) * 30, -60, 70, 50, p.C(0x43A047)); p.E(70 + MathF.Sin(t) * 30, 40, 60, 80, p.C(0x43A047)); p.E(-20 + MathF.Sin(t) * 30, 120, 40, 30, p.C(0x43A047)); });
                p.Ring(0, 0, 210, Al(W, .3f), 6); break;
            default:
                p.Arc(0, 0, 160, 0, MathF.PI, C, 120); p.R(-160, -120, 120, 120, 4, C); p.R(160, -120, 120, 120, 4, C);
                p.R(-160, -210, 120, 60, 4, metal); p.R(160, -210, 120, 60, 4, metal);
                for (int i = 0; i < 3; i++) p.Arc(0, 0, 330 + i * 40, 3.6f, 5.8f, Al(D, .35f + .3f * MathF.Sin(t * 3f - i)), 6); break;
        }
    }

    void Season(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        var green = p.C(0x2E7D32);
        switch (a)
        {
            case 0:
                p.D(0, 130, 170, W); p.D(0, -130, 120, W); p.R(0, -250, 200, 30, 8, K); p.R(0, -300, 120, 100, 8, K); p.R(0, -30, 220, 40, 16, C);
                p.D(-40, -150, 12, K); p.D(40, -150, 12, K); p.F(p.C(0xFF8F00), 0, -120, 60, -110, 0, -100);
                p.L(-150, 80, -280, 0, p.C(0x6D4C41), 10); p.L(150, 80, 280, 0, p.C(0x6D4C41), 10); break;
            case 1:
                p.L(-260, -300, -260, 320, p.C(0x8D6E63), 14);
                for (int k = 0; k < 2; k++)
                {
                    float y = -200 + k * 220; var col = k == 0 ? C : D;
                    var top = new List<Vector2>(); var bot = new List<Vector2>();
                    for (int i = 0; i <= 10; i++) { float x = -250 + i * 45, w = MathF.Sin(i * .6f - t * 4f) * 12f * i / 10f; top.Add(new(x, y - 70 + w)); bot.Add(new(x, y + 70 + w)); }
                    bot.Reverse(); top.AddRange(bot); p.Fill(col, top);
                    p.D(-170, y - 10 + MathF.Sin(-t * 4f) * 0, 30, W); p.D(-170, y - 10, 14, K);
                }
                break;
            case 2:
                for (int i = -1; i <= 1; i++) { float h = i == 0 ? 360 : 280; p.R(i * 80, 140 - h / 2f, 70, h, 6, green); p.F(Mx(green, W, .4f), i * 80 - 35, 140 - h, i * 80 + 35, 140 - h + 40, i * 80 + 35, 140 - h); }
                p.R(0, 200, 330, 150, 20, p.C(0xC69C6D)); for (int i = 0; i < 4; i++) p.L(-165, 150 + i * 30, 165, 150 + i * 30, p.C(0x8D6E63), 4); p.D(-150, 60, 40, C); p.D(150, 60, 40, C); break;
            case 3:
                p.R(0, 270, 70, 70, 6, p.C(0x6D4C41));
                for (int k = 0; k < 3; k++) { float y = -200 + k * 140, w = 120 + k * 70; p.F(green, 0, y - 80, w, y + 100, -w, y + 100); }
                for (int i = 0; i < 8; i++) { float x = (i % 2 == 0 ? -1 : 1) * (40 + i * 18), y = -130 + i * 50; p.D(x, y, 18, i % 2 == 0 ? C : D); }
                p.Star(0, -300, 50 * (1f + .1f * MathF.Sin(t * 3f)), 5, .45f, -MathF.PI / 2f, p.C(0xFFD54F)); break;
            case 4:
                p.L(-100, 320, 60, -320, green, 14);
                for (int i = 0; i < 7; i++) { float y = -240 + i * 80, x = -40 + i * -20 + 60; p.E(x + 60, y, 70, 20, Mx(green, W, .2f)); }
                for (int i = 0; i < 4; i++) { float y = -160 + i * 120, x = 10 - i * 25; p.Turn(x, y, MathF.Sin(t * 2f + i) * .2f, () => p.R(x, y + 70, 50, 140, 4, i % 2 == 0 ? C : D)); }
                break;
            case 5:
                p.L(0, -330, 0, -220, K, 4); p.Pie(0, -60, 170, MathF.PI, MathF.Tau, Al(p.C(0xE3F2FD), .8f)); p.R(0, -60, 340, 10, 4, Al(W, .8f));
                p.E(-60, -150, 30, 18, C); p.E(40, -120, 24, 14, D);
                p.Turn(0, -40, MathF.Sin(t * 3f) * .25f, () => { p.L(0, -40, 0, 120, K, 3); p.R(0, 200, 80, 160, 6, C); }); break;
            case 6:
                p.Turn(0, 0, MathF.Sin(t) * .1f, () =>
                {
                    var l = new List<Vector2>();
                    for (int i = 0; i < 7; i++) { float b = -MathF.PI / 2f + (i - 3) * .45f, r = i % 2 == 0 ? 250 : 120; l.Add(new(MathF.Cos(b) * r, MathF.Sin(b) * r + 40)); }
                    l.Add(new(0, 80)); p.Fill(C, l); p.L(0, 60, 0, 260, D, 10);
                    for (int i = 0; i < 7; i += 2) { float b = -MathF.PI / 2f + (i - 3) * .45f; p.L(0, 40, MathF.Cos(b) * 200, MathF.Sin(b) * 200 + 40, D, 5); }
                });
                break;
            case 7:
                p.R(0, 230, 440, 80, 10, p.C(0xD7A86E)); p.E(0, 110, 230, 100, W); p.E(0, -30, 170, 80, W); p.D(0, -130, 55, C); p.F(green, 20, -175, 70, -200, 50, -160);
                p.R(0, 180, 300, 30, 4, D); break;
            case 8:
                p.Turn(0, -300, MathF.Sin(t * 2f) * .1f, () =>
                {
                    p.L(0, -300, 0, -200, K, 4); p.D(0, -110, 100, W); p.F(W, -90, -60, 90, -60, 150, 160, -150, 160); p.R(0, -50, 180, 20, 8, C);
                    p.D(-30, -120, 9, K); p.D(30, -120, 9, K); p.Arc(0, -110, 30, .4f, 2.7f, K, 6);
                });
                break;
            default:
                p.F(Al(p.C(0xE3F2FD), .9f), -200, 0, 200, 0, 140, 240, -140, 240);
                p.Pie(0, 0, 210, MathF.PI, MathF.Tau, W); p.Pie(0, 0, 210, MathF.PI, MathF.PI * 1.55f, Al(C, .85f)); p.Pie(0, 0, 210, MathF.PI * 1.45f, MathF.Tau, Al(D, .8f));
                p.L(150, 20, 240, -260, p.C(0xB8C0CC), 10); break;
        }
    }

    void Status(ArtPen p, float t, Color4 C, Color4 W)
    {
        float k = 1f + .06f * MathF.Sin(t * 3f);
        p.Zoom(0, 0, k, k, () =>
        {
            p.D(8, 12, 230, p.C(0x000000, .2f)); p.D(0, 0, 230, C);
            switch (a)
            {
                case 0: p.P(W, 44, -110, 0, -30, 90, 120, -90); break;
                case 1: p.L(-90, -90, 90, 90, W, 44); p.L(90, -90, -90, 90, W, 44); break;
                case 2: p.F(W, 0, -150, 140, 110, -140, 110); p.R(0, 0, 26, 100, 10, C); p.D(0, 70, 16, C); break;
                case 3: p.Ring(0, 0, 140, W, 36); p.L(-100, 100, 100, -100, W, 36); break;
                case 4: p.D(0, -100, 28, W); p.R(0, 40, 50, 170, 14, W); break;
                case 5: p.Arc(0, -50, 70, -3.4f, .6f, W, 40); p.L(40, 0, 0, 40, W, 40); p.L(0, 40, 0, 70, W, 40); p.D(0, 130, 26, W); break;
                case 6: p.Star(0, 10, 150, 5, .45f, -MathF.PI / 2f, W); break;
                case 7: p.Fill(W, HeartLocal(0, 0, 150, false)); break;
                case 8: p.D(0, -40, 90, W); p.R(0, 80, 80, 60, 10, W); for (int i = 0; i < 5; i++) { float b = -MathF.PI / 2f + (i - 2) * .5f; p.L(MathF.Cos(b) * 120, -40 + MathF.Sin(b) * 120, MathF.Cos(b) * 165, -40 + MathF.Sin(b) * 165, W, 14); } break;
                default:
                    {
                        float f = MathF.Sin(t * 8f) * 8f;
                        p.F(W, 0, -170 + f, 100, -20, 120, 60, 60, 140, -60, 140, -120, 60, -100, -20, -40, -60, -50, -120);
                        p.F(C, 0, -40 + f * .5f, 50, 40, 30, 110, -30, 110, -50, 40); break;
                    }
            }
        });
    }

    void Planet(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        switch (a)
        {
            case 0:
                for (int i = 0; i < 12; i++) { float b = i * MathF.Tau / 12f + t * .2f; p.F(Al(C, .6f), MathF.Cos(b - .1f) * 200, MathF.Sin(b - .1f) * 200, MathF.Cos(b) * 290, MathF.Sin(b) * 290, MathF.Cos(b + .1f) * 200, MathF.Sin(b + .1f) * 200); }
                p.D(0, 0, 250 * (1f + .03f * MathF.Sin(t * 2f)), Al(D, .35f)); p.D(0, 0, 200, C); break;
            case 6: case 7:
                {
                    bool uran = a == 7; float tilt = uran ? 1.3f : -.35f;
                    p.Turn(0, 0, tilt, () => p.Zoom(0, 0, 1f, .3f, () => p.Arc(0, 0, 330, MathF.PI, MathF.Tau, D, 30)));
                    p.D(0, 0, 200, C); if (!uran) for (int i = -2; i <= 2; i++) p.R(0, i * 60, 380 - MathF.Abs(i) * 70, 14, 7, Al(D, .4f));
                    p.Turn(0, 0, tilt, () => p.Zoom(0, 0, 1f, .3f, () => p.Arc(0, 0, 330, 0, MathF.PI, D, 30)));
                    break;
                }
            default:
                p.D(0, 0, 230, C);
                switch (a)
                {
                    case 3: p.E(-70 + MathF.Sin(t * .5f) * 40, -60, 80, 60, D); p.E(80 + MathF.Sin(t * .5f) * 40, 60, 70, 90, D); p.E(-20, 160, 60, 30, D); break;
                    case 5: for (int i = -3; i <= 3; i++) p.R(0, i * 60, 460 - MathF.Abs(i) * 80, 26, 13, i % 2 == 0 ? Al(D, .7f) : Al(W, .2f)); p.E(60, 70, 50, 30, p.C(0xC0392B)); break;
                    case 1: case 9: for (int i = 0; i < 7; i++) { float b = i * 2.1f, r = 40 + i * 25; p.D(MathF.Cos(b) * r, MathF.Sin(b) * r, 18 + i * 3, Al(D, .6f)); } break;
                    case 2: for (int i = -2; i <= 2; i++) p.R(0, i * 70, 420 - MathF.Abs(i) * 90, 30, 15, Al(D, .4f)); break;
                    default: p.E(-60, -60, 90, 50, Al(D, .4f)); break;
                }
                p.Pie(0, 0, 230, -MathF.PI / 2f, MathF.PI / 2f, p.C(0x000000, .25f));
                break;
        }
    }

    void Money(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W, string s)
    {
        switch (a)
        {
            case 6:
                p.Turn(0, 0, MathF.Sin(t) * .05f, () =>
                {
                    p.R(0, 0, 620, 300, 16, C); p.RL(0, 0, 580, 260, 10, D, 6); p.D(-170, 0, 80, Mx(C, W, .3f)); p.Ring(-170, 0, 80, D, 4);
                    p.T(s, 90, 130, -40, D); p.R(130, 60, 260, 14, 7, Al(D, .5f)); p.R(130, 95, 200, 14, 7, Al(D, .5f));
                });
                break;
            case 7:
                p.R(0, 40, 500, 340, 50, C); p.R(0, -60, 500, 150, 50, Sh(C, .85f)); p.R(150, -10, 200, 90, 30, D); p.D(200, -10, 18, Mx(D, W, .5f));
                for (int i = 0; i < 4; i++) p.L(-230, 120 + i * 18, 230, 120 + i * 18, Al(Sh(C, .7f), .6f), 3); break;
            case 8:
                for (int i = 0; i < 14; i++)
                {
                    int row = i < 5 ? 0 : i < 9 ? 1 : i < 12 ? 2 : 3; int col = i < 5 ? i : i < 9 ? i - 5 : i < 12 ? i - 9 : i - 12; int n = 5 - row;
                    float x = (col - (n - 1) / 2f) * 110, y = 200 - row * 70;
                    p.E(x, y, 70, 26, D); p.E(x, y - 10, 70, 26, C); p.EL(x, y - 10, 52, 18, Sh(C, .8f), 4);
                }
                p.Star(120, -150, 40 * MathF.Max(0f, MathF.Sin(t * 2.5f)), 4, .2f, 0f, W); break;
            case 9:
                p.E(0, 40, 270, 210, C); p.E(240, 40, 50, 70, Sh(C, .85f)); p.D(220, 30, 10, K); p.D(220, 60, 10, K);
                p.F(Sh(C, .9f), -120, -140, -60, -230, -20, -150); p.F(Sh(C, .9f), 40, -150, 90, -230, 130, -140);
                p.D(100, -30, 16, K); for (int i = -1; i <= 1; i += 2) { p.R(i * 140, 240, 60, 60, 10, Sh(C, .85f)); p.R(i * 40, 240, 60, 60, 10, Sh(C, .85f)); }
                p.R(0, -170, 120, 16, 8, K); p.D(0, -220 - ArtPen.Fr(t * .7f) * -60 - 60, 28, p.C(0xF2C14E)); break;
            default:
                {
                    bool hole = a == 1 || a == 3;
                    p.Turn(0, 0, 0f, () =>
                    {
                        float sx = MathF.Abs(MathF.Cos(t * 1.5f)) * .7f + .3f;
                        p.Zoom(0, 0, sx, 1f, () =>
                        {
                            p.D(0, 0, 220, D); p.D(0, 0, 200, C); p.Ring(0, 0, 175, Al(D, .6f), 6);
                            if (hole) p.D(0, 0, 55, p.C(0x000000, .0f));
                            Fit(p, s, 0, hole ? 110 : 0, Sh(D, .7f), 260, hole ? 90 : 170);
                            if (hole) p.Ring(0, 0, 55, Sh(D, .7f), 10);
                        });
                    });
                    break;
                }
        }
    }
}
