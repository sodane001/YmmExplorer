using Vortice.Mathematics;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// =====================================================================
//  1082番以降の図形の土台。系統(kind)ごとの描き方を1か所にまとめる
//   0=柄 1=顔 2=丸数字 3=順位 4=暦 5=文字タイル 6=干支 7=星座 8=時計 9=円グラフ
//   10=進捗 11=電池 12=電波 13=吹き出し 14=帯 15=枠 16=矢印 18=テロップ帯 19=バッジ
// =====================================================================
public abstract partial class Q6Art : Q4Art
{
    readonly int kind, a, b, c;

    private protected Q6Art(SharedDataStore? s, int kind, int a = 0, int b = 0, int c = 0) : base(s) { this.kind = kind; this.a = a; this.b = b; this.c = c; }

    protected override bool Scene => kind == 0;

    protected override string[] Use => kind switch
    {
        0 => ["Color1=地の色", "Color2=模様の色", "Speed=流れる速さ"],
        1 or 8 or 9 or 10 or 11 or 12 or 16 or 21 or 22 or 23 or 28 or 29 or 31 or 34 or 37 or 38 or 39 or 40 or 41 or 42 or 43 or 44 or 45 or 46 or 49 or 51 or 53 or 54 or 55 or 56 or 57 or 59 or 60 or 61 => ["Color1=メインの色", "Color2=サブの色", "Speed=動く速さ"],
        64 => ["Color1=地の色(禁止は輪の色)", "Color2=絵の色", "Speed=動く速さ"],
        _ when kind >= 65 => ["Color1=メインの色", "Color2=サブの色", "Speed=動く速さ"],
        15 => ["Color1=枠の色", "Color2=サブの色", "Speed=動く速さ"],
        _ => ["Text=文字", "Color1=メインの色", "Color2=サブの色", "Speed=動く速さ"],
    };

    private protected sealed override void Art(ArtPen p, float t, float v, float v2)
    {
        var C = p.C(Color1); var D = p.C(Color2); var K = p.C(0x20242B); var W = p.C(0xFFFFFF);
        var s = Str.Replace("\n", " ").Trim();
        switch (kind)
        {
            case 0: Pattern(p, t, C, D); break;
            case 1: p.Move(0, MathF.Sin(t * 2f) * 8f, () => Face(p, t, C, D, K, W)); break;
            case 2: p.D(0, 0, 235, C); p.Ring(0, 0, 214, D, 16); Fit(p, s, 0, 0, W, 360, 270); break;
            case 3: Rank(p, t, C, D, K, W, s); break;
            case 4: p.R(0, 15, 460, 450, 34, W); p.R(0, -135, 460, 150, 30, C); p.B(0, -85, 460, 50, C); foreach (int x in new[] { -125, 125 }) p.R(x, -225, 34, 92, 14, D); Fit(p, s, 0, 85, K, 390, 170); break;
            case 5: p.R(0, 0, 440, 440, 58, C); p.RL(0, 0, 398, 398, 40, D, 10); Fit(p, s, 0, 0, W, 320, 260); break;
            case 6: p.D(0, 0, 240, C); p.Ring(0, 0, 222, D, 7); p.Ring(0, 0, 192, D, 3); Fit(p, s, 0, 0, W, 300, 250); break;
            case 7: Constellation(p, t, C, D, W, s); break;
            case 8: Clock(p, C, D, K, W); break;
            case 9: p.D(0, 0, 240, D); p.Pie(0, 0, 240, -1.5708f, -1.5708f + a / 100f * MathF.Tau, C); p.D(0, 0, 118, W); p.T(a + "%", 82, 0, 0, K); break;
            case 10: p.R(0, 0, 720, 112, 56, D); p.R(-340 + a * 3.4f, 0, MathF.Max(84f, a * 6.8f), 84, 42, C); p.T(a + "%", 92, 0, -130, C); break;
            case 11: Battery(p, t, C, D, W); break;
            case 12: for (int i = 0; i < 4; i++) { float h = 90 + i * 85; p.R(-165 + i * 110, 170 - h / 2f, 74, h, 16, i < a ? C : D); } break;
            case 13: Balloon(p, C, D, K, s); break;
            case 14: Ribbon(p, C, D, W, s); break;
            case 15: Frame(p, t, C, D); break;
            case 16: Arrow(p, t, C, D); break;
            case 18: Telop(p, t, C, D, W, s); break;
            case 19: Badge(p, t, C, D, W, s); break;
            case 20: ElementCard(p, C, D, W, s); break;
            case 21: Person(p, t, C, D); break;
            case 22: Diagram(p, t, C, D, K, W); break;
            case 23: WeatherMark(p, t, C, D, W); break;
            default: if (kind >= 65) Icon2D(p, t, C, D); else if (kind >= 64) Picto2D(p, t, C, D, K, W); else if (kind >= 54) Art10(p, t, C, D, K, W, s); else if (kind >= 37) Art9(p, t, C, D, K, W, s); else Art8(p, t, C, D, K, W, s); break;
        }
    }

    static void Fit(ArtPen p, string s, float x, float y, Color4 col, float maxW, float size)
    {
        // 文字が枠の幅に収まる大きさにする(折り返すと枠からはみ出すため)
        float units = 0f;
        foreach (char ch in s) units += ch < 128 ? .64f : 1f;
        if (units > 0f) size = MathF.Min(size, maxW * .9f / units);
        p.T(s, size, x, y, col, default, 0f, maxW);
    }

    // ---------------------------------------------------------------- 柄
    void Pattern(ArtPen p, float t, Color4 C, Color4 D)
    {
        float k = MathF.Min(p.W / 1920f, p.H / 1080f), hw = p.W / 2f / k, hh = p.H / 2f / k, u = 120f, o = ArtPen.Fr(t * .08f) * u * 2f;
        p.B(0, 0, hw * 2f + 4f, hh * 2f + 4f, C);
        void Grid(float sx, float sy, bool stagger, Action<float, float, int, int> draw)
        {
            int rows = (int)((hh * 2f + sy * 4f) / sy), cols = (int)((hw * 2f + sx * 4f) / sx);
            for (int r = 0; r < rows; r++)
                for (int q = 0; q < cols; q++)
                    draw(-hw - sx * 2f + q * sx + (stagger && r % 2 == 1 ? sx / 2f : 0f) + o, -hh - sy * 2f + r * sy, r, q);
        }
        switch (a)
        {
            case 0: Grid(u, u * .8f, true, (x, y, _, _) => p.D(x, y, 30, D)); break;
            case 1: for (float x = -hw - hh * 2f - u; x < hw + u * 2f; x += u) p.F(D, x + o, -hh - 4f, x + o + u * .5f, -hh - 4f, x + o + u * .5f + hh * 2f, hh + 4f, x + o + hh * 2f, hh + 4f); break;
            case 2: Grid(u, u, false, (x, y, r, q) => { if ((r + q) % 2 == 0) p.B(x + u / 2f, y + u / 2f, u + 1f, u + 1f, D); }); break;
            case 3: Grid(u, u, false, (x, y, _, _) => { p.L(x, -hh, x, hh, D, 6); p.L(-hw, y, hw, y, D, 6); }); break;
            case 4: for (float x = -hw - hh * 2f - u; x < hw + hh * 2f + u; x += u) { p.L(x + o, -hh, x + o + hh * 2f, hh, D, 7); p.L(x + o + hh * 2f, -hh, x + o, hh, D, 7); } break;
            case 5:
                for (float y = -hh; y <= hh + u; y += u * .6f)
                {
                    var w = new List<System.Numerics.Vector2>();
                    for (float x = -hw - 20f; x <= hw + 20f; x += 18f) w.Add(new(x, y + MathF.Sin((x + o) * .052f) * 22f));
                    p.Line(D, 8, w);
                }
                break;
            case 6:
                Grid(u * 1.5f, u * .375f, true, (x, y, _, _) => { p.D(x, y, u * .75f, C); for (int i = 1; i <= 3; i++) p.Ring(x, y, u * .75f * i / 3f, D, 5); });
                break;
            case 7:
                Grid(u * 1.6f, u * 1.386f, true, (x, y, _, _) =>
                {
                    float r = u * .92f;
                    for (int i = 0; i < 6; i++) { float m = i * 1.0472f, n = m + 1.0472f; p.L(x + MathF.Cos(m) * r, y + MathF.Sin(m) * r, x + MathF.Cos(n) * r, y + MathF.Sin(n) * r, D, 3); p.L(x, y, x + MathF.Cos(m) * r, y + MathF.Sin(m) * r, D, 3); p.L(x, y, x + MathF.Cos(m + .5236f) * r * .866f, y + MathF.Sin(m + .5236f) * r * .866f, D, 3); }
                });
                break;
            case 8:
                Grid(u * 1.5f, u * .866f, true, (x, y, _, _) =>
                {
                    var h = new List<System.Numerics.Vector2>();
                    for (int i = 0; i <= 6; i++) { float m = i * 1.0472f; h.Add(new(x + MathF.Cos(m) * u * .72f, y + MathF.Sin(m) * u * .72f)); }
                    p.Line(D, 6, h);
                });
                break;
            case 9: Grid(u, u, false, (x, y, _, _) => { p.Ring(x, y, u * .71f, D, 5); }); break;
            case 10: Grid(u * .9f, u * .8f, false, (x, y, r, q) => { float f = q % 2 == 0 ? 1f : -1f; p.F(D, x, y, x + u * .45f, y + u * .22f * f, x + u * .9f, y, x + u * .9f, y + u * .5f, x + u * .45f, y + u * .72f * f + (f < 0 ? u : 0f), x, y + u * .5f); }); break;
            case 11: Grid(u, u * .86f, true, (x, y, _, _) => p.F(D, x, y - u * .4f, x + u * .46f, y + u * .4f, x - u * .46f, y + u * .4f)); break;
            case 12: Grid(u, u * .7f, true, (x, y, _, _) => p.F(D, x, y - u * .34f, x + u * .34f, y, x, y + u * .34f, x - u * .34f, y)); break;
            default: Grid(u * 1.1f, u * .9f, true, (x, y, r, q) => p.Star(x, y, 42, 5, .45f, (r + q) * .4f, D)); break;
        }
    }

    // ---------------------------------------------------------------- 顔
    void Face(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        p.D(0, 0, 250, C);
        p.Arc(0, 0, 232, 3.6f, 4.6f, p.C(0xFFFFFF, .35f), 12);
        foreach (int q in new[] { -1, 1 })
        {
            float x = q * 92, y = -55;
            switch (a)
            {
                case 1: p.Arc(x, y + 18, 40, 3.5f, 5.92f, K, 14); break;
                case 2: p.L(x - 40, y, x + 40, y, K, 13); break;
                case 3: p.D(x, y, 56, W); p.D(x, y + 6, 26, K); p.D(x - 8, y - 4, 8, W); break;
                case 4: p.D(x, y + 12, 22, K); p.L(x - q * -48, y - 42, x + q * -48, y - 14, K, 15); break;
                case 5: p.L(x - 32, y - 32, x + 32, y + 32, K, 13); p.L(x + 32, y - 32, x - 32, y + 32, K, 13); break;
                case 6: p.D(x - 18, y - 8, 23, p.C(0xE5294F)); p.D(x + 18, y - 8, 23, p.C(0xE5294F)); p.F(p.C(0xE5294F), x - 40, y + 2, x + 40, y + 2, x, y + 46); break;
                case 7: p.Star(x, y, 50, 4, .32f, t, K); p.Star(x, y, 26, 4, .32f, t, W); break;
                default: p.D(x, y, 25, K); p.D(x - 8, y - 9, 8, W); break;
            }
        }
        switch (b)
        {
            case 1: p.Pie(0, 65, 108, 0, 3.1416f, K); p.E(0, 135, 58, 26, p.C(0xE5546F)); break;
            case 2: p.L(-60, 105, 60, 105, K, 14); break;
            case 3: p.Arc(0, 165, 72, 3.65f, 5.77f, K, 14); break;
            case 4: p.E(0, 110, 34, 42, K); break;
            case 5: { var w = new List<System.Numerics.Vector2>(); for (int i = 0; i <= 24; i++) w.Add(new(-80 + i * 6.67f, 105 + MathF.Sin(i * 1.05f) * 13f)); p.Line(K, 12, w); break; }
            case 6: p.R(0, 108, 170, 66, 18, W); p.RL(0, 108, 170, 66, 18, K, 8); for (int i = -2; i <= 2; i++) p.L(i * 28, 76, i * 28, 140, K, 4); break;
            case 7: p.Arc(0, 62, 72, .35f, 2.8f, K, 13); p.R(28, 128, 44, 54, 20, p.C(0xE5546F)); break;
            default: p.Arc(0, 58, 78, .35f, 2.8f, K, 14); break;
        }
        switch (c)
        {
            case 1: { float y = -130 + ArtPen.Fr(t * .6f) * 60f; p.D(190, y + 18, 26, p.C(0x6CCBF5)); p.F(p.C(0x6CCBF5), 190, y - 34, 214, y + 10, 166, y + 10); break; }
            case 2: p.E(-155, 45, 46, 26, D); p.E(155, 45, 46, 26, D); break;
            case 3: foreach (int q in new[] { -1, 1 }) { float k = ArtPen.Fr(t * .7f + (q + 1) * .25f); p.E(q * 112, -10 + k * 140f, 16, 28, p.C(0x6CCBF5, 1f - k * .6f)); } break;
            case 4: foreach (int q in new[] { -1, 1 }) foreach (int r in new[] { -1, 1 }) p.Arc(170 + q * 32, -175 + r * 32, 24, q * r > 0 ? 0f : 1.57f, q * r > 0 ? 1.57f : 3.14f, p.C(0xE5294F), 11); break;
            case 5: for (int i = 0; i < 3; i++) { float k = ArtPen.Fr(t * .35f + i / 3f); p.T("Z", 38 + k * 42f, 170 + k * 80f, -130 - k * 130f, p.C(0x8FA8E8, 1f - k)); } break;
            case 6: for (int i = 0; i < 4; i++) { float m = i * 1.5708f + .7f + t * .5f; p.Star(MathF.Cos(m) * 292, MathF.Sin(m) * 262, 34 + MathF.Sin(t * 3f + i) * 10f, 4, .3f, 0, p.C(0xFFE27A)); } break;
        }
    }

    // ---------------------------------------------------------------- 順位・星座・時計・電池
    void Rank(ArtPen p, float t, Color4 C, Color4 D, Color4 K, Color4 W, string s)
    {
        p.F(D, -125, 30, -35, 40, -55, 310, -110, 265, -175, 280);
        p.F(D, 125, 30, 35, 40, 55, 310, 110, 265, 175, 280);
        p.Star(0, -35, 240, 16, .88f, t * .15f, C);
        p.D(0, -35, 175, W); p.D(0, -35, 158, C);
        Fit(p, s, 0, -35, K, 270, 140);
    }

    internal static readonly float[][] Sky =
    [
        [-170, 60, -60, -10, 60, -40, 150, -30],
        [-190, 70, -110, 10, -50, 30, 30, -60, 130, -90, 190, -30],
        [-160, -120, -90, -40, -40, 50, 60, 70, 110, -50, 170, -120],
        [-120, 110, -30, 30, 0, -60, 100, -30, 60, 60, -30, 30],
        [-190, 20, -120, -60, -30, -90, 50, -30, 70, 60, 150, 80, 190, 20],
        [-180, -100, -100, -60, -30, -10, 30, 40, 120, 20, 170, 100, 90, 110],
        [-130, 70, -50, -40, 70, -90, 140, 50],
        [-190, -40, -110, -60, -60, 0, 0, 40, 40, 110, 130, 130, 180, 60],
        [-170, 100, -60, 30, 30, -20, 110, -90, 170, -130],
        [-170, -30, -110, 80, -30, 20, 60, 70, 140, 30, 180, -60],
        [-180, -40, -100, 10, -20, -30, 60, 30, 130, -20, 190, 40],
        [-130, -120, -60, -30, -90, 70, 90, 90, 60, -20, 130, -110],
    ];

    void Constellation(ArtPen p, float t, Color4 C, Color4 D, Color4 W, string s)
    {
        p.D(0, -35, 262, C);
        var q = Sky[Math.Clamp(a, 0, Sky.Length - 1)];
        for (int i = 0; i + 3 < q.Length; i += 2) p.L(q[i], q[i + 1] - 35, q[i + 2], q[i + 3] - 35, p.C(0xFFFFFF, .55f), 4);
        for (int i = 0; i + 1 < q.Length; i += 2) p.Star(q[i], q[i + 1] - 35, 25 + MathF.Sin(t * 2.4f + i) * 7f, 4, .34f, 0, D);
        p.R(0, 240, 390, 86, 26, D);
        p.T(s, 50, 0, 240, C, default, 0f, 355);
    }

    void Clock(ArtPen p, Color4 C, Color4 D, Color4 K, Color4 W)
    {
        p.D(0, 0, 250, C); p.D(0, 0, 214, W);
        for (int i = 0; i < 12; i++) { float m = i * .5236f; float r0 = i % 3 == 0 ? 170 : 188; p.L(MathF.Cos(m) * r0, MathF.Sin(m) * r0, MathF.Cos(m) * 202, MathF.Sin(m) * 202, K, i % 3 == 0 ? 10 : 5); }
        float h = (a + c / 60f) * .5236f - 1.5708f, mn = c * .10472f - 1.5708f;
        p.L(0, 0, MathF.Cos(h) * 112, MathF.Sin(h) * 112, K, 17);
        p.L(0, 0, MathF.Cos(mn) * 165, MathF.Sin(mn) * 165, D, 10);
        p.D(0, 0, 18, K);
    }

    void Battery(ArtPen p, float t, Color4 C, Color4 D, Color4 W)
    {
        p.RL(-20, 0, 520, 250, 34, W, 18); p.R(262, 0, 34, 104, 10, W);
        var col = a <= 1 ? p.C(0xE5392F, .55f + .45f * MathF.Abs(MathF.Sin(t * 3f))) : a == 2 ? D : C;
        for (int i = 0; i < a; i++) p.R(-200 + i * 120, 0, 102, 188, 14, col);
    }

    // ---------------------------------------------------------------- 吹き出し・帯・枠・矢印
    void Balloon(ArtPen p, Color4 C, Color4 D, Color4 K, string s)
    {
        void Tail(float x, float y) => p.F(C, x - 62, y - 26, x + 62, y - 26, x - 42, y + 112);
        switch (a)
        {
            case 1: p.E(0, -22, 410, 195, C); Tail(-130, 158); break;
            case 2: for (int i = 0; i < 12; i++) { float m = i * .5236f; p.E(MathF.Cos(m) * 305, -25 + MathF.Sin(m) * 130, 118, 92, C); } p.E(0, -25, 345, 165, C); p.D(-260, 195, 34, C); p.D(-320, 250, 18, C); break;
            case 3: p.Star(0, -15, 1, 3, 1, 0, C); p.Zoom(0, -15, 1.75f, .86f, () => p.Star(0, -15, 275, 16, .72f, 0, C)); break;
            case 4: p.E(0, -30, 395, 178, C); p.D(-235, 195, 38, C); p.D(-300, 255, 20, C); break;
            case 5: p.B(0, -25, 760, 320, C); p.F(C, -210, 130, -60, 130, -255, 280); break;
            case 6: p.R(35, -25, 700, 310, 46, C); p.F(C, -300, -90, -300, 30, -430, -30); break;
            case 7: p.R(0, -25, 760, 310, 150, C); Tail(190, 138); break;
            default: p.R(0, -25, 760, 320, 60, C); Tail(-160, 142); break;
        }
        p.T(s, 82, 0, -25, K.R + K.G + K.B >= 0 ? (Lum(C) > .55f ? K : p.C(0xFFFFFF)) : K, default, 0f, 620);
    }

    static float Lum(Color4 c) => c.R * .3f + c.G * .59f + c.B * .11f;

    void Ribbon(ArtPen p, Color4 C, Color4 D, Color4 W, string s)
    {
        switch (a)
        {
            case 1: p.F(C, -470, -82, 470, -82, 410, 0, 470, 82, -470, 82, -410, 0); break;
            case 2: p.F(D, -500, -30, -350, -30, -350, 112, -500, 112, -445, 42); p.F(D, 500, -30, 350, -30, 350, 112, 500, 112, 445, 42); p.B(0, 0, 780, 164, C); break;
            case 3: p.R(0, 0, 900, 150, 75, C); p.Ring(-375, 0, 30, W, 8); break;
            case 4: p.F(C, -440, -82, 470, -82, 440, 82, -470, 82); p.F(D, -470, 82, -440, 82, -440, 120); break;
            case 5: p.F(C, -480, -80, 400, -80, 480, 0, 400, 80, -480, 80); p.D(-430, 0, 18, W); break;
            default: p.B(0, 0, 900, 150, C); p.B(0, -66, 900, 10, D); p.B(0, 66, 900, 10, D); break;
        }
        p.T(s, 78, 0, 0, W, default, 0f, 700);
    }

    void Frame(ArtPen p, float t, Color4 C, Color4 D)
    {
        const float w = 1180, h = 650;
        switch (a)
        {
            case 1: p.RL(0, 0, w, h, 4, C, 12); p.RL(0, 0, w - 54, h - 54, 2, D, 5); break;
            case 2: p.RL(0, 0, w, h, 66, C, 16); break;
            case 3: foreach (int x in new[] { -1, 1 }) foreach (int y in new[] { -1, 1 }) { p.L(x * w / 2f, y * h / 2f, x * (w / 2f - 150), y * h / 2f, C, 16); p.L(x * w / 2f, y * h / 2f, x * w / 2f, y * (h / 2f - 150), C, 16); } break;
            case 4:
                {
                    float m = ArtPen.Fr(t * .4f) * 60f;
                    for (float x = -w / 2f + m; x < w / 2f; x += 60) { p.L(x, -h / 2f, x + 34, -h / 2f, C, 10); p.L(-x, h / 2f, -x - 34, h / 2f, C, 10); }
                    for (float y = -h / 2f + m; y < h / 2f; y += 60) { p.L(w / 2f, y, w / 2f, y + 34, C, 10); p.L(-w / 2f, -y, -w / 2f, -y - 34, C, 10); }
                    break;
                }
            case 5: for (int i = 0; i < 28; i++) { float x = -w / 2f + i * w / 27f; p.D(x, -h / 2f, 11, C); p.D(x, h / 2f, 11, C); } for (int i = 1; i < 15; i++) { float y = -h / 2f + i * h / 15f; p.D(-w / 2f, y, 11, C); p.D(w / 2f, y, 11, C); } break;
            case 6: p.B(0, -h / 2f, w, 72, C); p.B(0, h / 2f, w, 72, C); for (int i = 0; i < 18; i++) { float x = -w / 2f + 34 + i * 66; p.R(x, -h / 2f, 34, 34, 6, D); p.R(x, h / 2f, 34, 34, 6, D); } break;
            case 7: p.RL(0, 0, w, h, 8, C, 8); foreach (int x in new[] { -1, 1 }) foreach (int y in new[] { -1, 1 }) p.Star(x * w / 2f, y * h / 2f, 58, 4, .35f, 0, D); break;
            case 8: p.L(-w / 2f, -h / 2f, w / 2f, -h / 2f, C, 15); p.L(-w / 2f, h / 2f, w / 2f, h / 2f, C, 15); foreach (int x in new[] { -1, 1 }) { p.L(x * w / 2f, -h / 2f, x * (w / 2f + 60), 0, C, 15); p.L(x * w / 2f, h / 2f, x * (w / 2f + 60), 0, C, 15); } break;
            case 9: foreach (int x in new[] { -1, 1 }) { p.L(x * (w / 2f - 100), -h / 2f, x * w / 2f, -h / 2f, C, 16); p.L(x * w / 2f, -h / 2f, x * w / 2f, h / 2f, C, 16); p.L(x * w / 2f, h / 2f, x * (w / 2f - 100), h / 2f, C, 16); } break;
            default: p.RL(0, 0, w, h, 2, C, 8); break;
        }
    }

    void Arrow(ArtPen p, float t, Color4 C, Color4 D)
    {
        float m = MathF.Sin(t * 3f) * 16f;
        switch (a)
        {
            case 1: p.Move(m, 0, () => { p.B(-85, 0, 330, 150, C); p.F(C, 70, -205, 310, 0, 70, 205); }); break;
            case 2: p.B(0, 0, 400, 86, C); p.F(C, 170, -130, 340, 0, 170, 130); p.F(C, -170, -130, -340, 0, -170, 130); break;
            case 3: p.Arc(0, 130, 245, 3.45f, 5.5f, C, 54); p.F(C, 120, -80, 280, 120, 80, 150); break;
            case 4: p.Arc(0, -30, 130, 0, 3.1416f, C, 52); p.L(130, -30, 130, -220, C, 52); p.L(-130, -30, -130, -80, C, 52); p.F(C, -240, -80, -20, -80, -130, -240); break;
            case 5: p.Move(m, 0, () => p.F(C, -150, -220, 40, -220, 200, 0, 40, 220, -150, 220, 10, 0)); break;
            case 6: for (int i = 0; i < 3; i++) { float x = -210 + i * 150; var q = p.C(C, .35f + .65f * ArtPen.Cl(MathF.Sin(t * 4f - i) * .5f + .5f)); p.F(q, x - 70, -180, x, -180, x + 100, 0, x, 180, x - 70, 180, x + 30, 0); } break;
            case 7: p.Turn(0, 0, t * 1.2f, () => { p.Arc(0, 0, 190, -.2f, 4.8f, C, 50); p.F(C, 110, -230, 300, -140, 120, -30); }); break;
            case 8: p.Turn(0, 0, -.7854f, () => p.Move(m, 0, () => { p.B(-80, 0, 340, 76, C); p.F(C, 70, -140, 270, 0, 70, 140); })); break;
            case 9: p.R(0, 0, 500, 300, 42, C); p.F(D, -120, -92, 40, -92, 40, -150, 190, 0, 40, 150, 40, 92, -120, 92); break;
            default: p.Move(m, 0, () => { p.L(-280, 0, 180, 0, C, 28); p.F(C, 140, -90, 300, 0, 140, 90); }); break;
        }
    }

    void Telop(ArtPen p, float t, Color4 C, Color4 D, Color4 W, string s)
    {
        const float w = 1240, h = 170;
        var txt = W;
        if (a is 4 or 8 or 10 && Lum(C) < .22f) C = D;    // 線だけの帯は、暗い色だと背景に沈むのでサブの色で描く
        switch (a)
        {
            case 1: p.B(0, 0, w, h, C); p.B(-w / 2f + 14, 0, 28, h, D); break;
            case 2: p.B(0, 12, w, h, D); p.B(-10, 0, w, h, C); break;
            case 3: p.F(C, -w / 2f + 40, -h / 2f, w / 2f + 40, -h / 2f, w / 2f - 40, h / 2f, -w / 2f - 40, h / 2f); break;
            case 4: p.B(0, h / 2f - 6, w, 12, C); break;
            case 5: p.B(0, -h / 2f, w, 9, D); p.B(0, h / 2f, w, 9, D); p.B(0, 0, w, h - 28, p.C(C, .86f)); break;
            case 6: p.B(0, 12, w, h, C); p.R(-w / 2f + 130, -h / 2f - 12, 260, 58, 12, D); break;
            case 7: p.R(0, 0, w, h, h / 2f, C); break;
            case 8: foreach (int q in new[] { -1, 1 }) { p.L(q * w / 2f, -h / 2f, q * (w / 2f - 70), -h / 2f, C, 11); p.L(q * w / 2f, -h / 2f, q * w / 2f, h / 2f, C, 11); p.L(q * w / 2f, h / 2f, q * (w / 2f - 70), h / 2f, C, 11); } break;
            case 9: p.B(0, 0, w, h, C); { float o = ArtPen.Fr(t * .4f) * 60f; for (float x = -w / 2f + o; x < w / 2f - 22f; x += 60) { p.F(D, x, h / 2f, x + 26, h / 2f, x + 42, h / 2f - 24, x + 16, h / 2f - 24); p.F(D, x, -h / 2f, x + 26, -h / 2f, x + 42, -h / 2f + 24, x + 16, -h / 2f + 24); } } break;
            case 10: p.R(0, 0, w + 18, h + 18, 34, p.C(C, .22f)); p.RL(0, 0, w, h, 26, C, 6); break;
            case 11: for (int i = 0; i < 9; i++) p.R((ArtPen.R01(i + 3) - .5f) * 60f, -68 + i * 17f, w - ArtPen.R01(i) * 90f, 20, 9, C); break;
            default: p.B(0, 0, w, h, C); break;
        }
        p.T(s, 82, 0, 0, txt, default, 0f, w - 110);
    }

    void Badge(ArtPen p, float t, Color4 C, Color4 D, Color4 W, string s)
    {
        float z = 1f + MathF.Sin(t * 3f) * .025f;
        p.Zoom(0, 0, z, z, () =>
        {
            switch (a)
            {
                case 1: p.D(0, 0, 250, D); p.D(0, 0, 222, C); p.Ring(0, 0, 195, W, 5); break;
                case 2: p.F(D, -115, 70, -30, 115, -65, 310, -105, 270, -165, 285); p.F(D, 115, 70, 30, 115, 65, 310, 105, 270, 165, 285); p.Star(0, -20, 225, 20, .9f, 0, C); break;
                case 3: p.F(D, 250, 0, 125, 216, -125, 216, -250, 0, -125, -216, 125, -216); p.F(C, 220, 0, 110, 190, -110, 190, -220, 0, -110, -190, 110, -190); break;
                case 4: p.F(C, -300, -130, 200, -130, 320, 0, 200, 130, -300, 130); p.D(190, 0, 24, W); break;
                case 5: p.F(D, -225, -235, 225, -235, 225, 45, 0, 275, -225, 45); p.F(C, -195, -205, 195, -205, 195, 32, 0, 232, -195, 32); break;
                default: p.Star(0, 0, 268, 14, .78f, t * .2f, C); break;
            }
            Fit(p, s, a == 4 ? -45 : 0, a == 2 ? -20 : 0, W, a == 4 ? 410 : 320, 130);
        });
    }
}
