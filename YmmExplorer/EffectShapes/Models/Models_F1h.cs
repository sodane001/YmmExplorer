using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1292〜1321
//   進捗バー 50〜100% / 電池(透けたケースと中の電池) / 電波(立体の棒) / 吹き出し 角丸・楕円・もくもく・トゲトゲ・考えごと
// =====================================================================
internal static partial class Models3D
{
    /// <summary>電池: 透けたケースと白いふち、プラス極、中に並ぶ電池(少ない時は赤く点滅)</summary>
    static void Battery3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = new Vector4(1, 1, 1, C.W);
        m.Box(-20, 0, 0, 520, 250, 130, m.C(0xFFFFFF, .14f), .2f);
        m.Box(-20, -116, 0, 520, 18, 130, W, .3f); m.Box(-20, 116, 0, 520, 18, 130, W, .3f);
        m.Box(-271, 0, 0, 18, 250, 130, W, .3f); m.Box(231, 0, 0, 18, 250, 130, W, .3f);
        m.Box(262, 0, 0, 34, 104, 70, W, .3f);
        var col = a <= 1 ? m.C(0xE5392F, .55f + .45f * MathF.Abs(MathF.Sin(t * 3f))) : a == 2 ? D : C;
        for (int i = 0; i < a; i++) m.Box(-200 + i * 120, 0, 0, 102, 188, 96, col, .2f);
    }

    /// <summary>電波: 右ほど高い4本の立体の棒(届いている本数だけ色付き)</summary>
    static void Signal3D(Sculpt m, Vector4 C, Vector4 D, int a)
    {
        for (int i = 0; i < 4; i++) { float h = 90 + i * 85; m.Box(-165 + i * 110, 170 - h / 2f, 0, 74, h, 74, i < a ? C : D, .25f); }
    }

    static float Lum(Vector4 c) => c.X * .3f + c.Y * .59f + c.Z * .11f;

    /// <summary>吹き出し: ふくらんだ立体の吹き出しと、しっぽ、手前の文字</summary>
    static void Balloon3D(Sculpt m, Vector4 C, Vector4 D, int a, string s)
    {
        var K = m.C(0x20242B); var tc = Lum(C) > .55f ? K : new Vector4(1, 1, 1, C.W);
        void Tail(float x, float y) => Puff(m, 0, 44, C, x - 62, y - 26, x + 62, y - 26, x - 42, y + 112);
        float z;
        switch (a)
        {
            case 1: m.Ball(0, -22, 0, 410, 195, 70, C); Tail(-130, 158); z = 72; break;
            case 2:
                for (int i = 0; i < 12; i++) { float mm = i * .5236f; m.Ball(MathF.Cos(mm) * 305, -25 + MathF.Sin(mm) * 130, 0, 118, 92, 60, C); }
                m.Ball(0, -25, 0, 345, 165, 70, C);
                m.Ball(-260, 195, 0, 34, C); m.Ball(-320, 250, 0, 18, C);
                z = 72; break;
            case 3:
                {
                    var st = StarPts(0, -15, 275, 16, .72f, 0f);
                    for (int i = 0; i < st.Length; i += 2) { st[i] *= 1.75f; st[i + 1] = -15 + (st[i + 1] + 15) * .86f; }
                    Puff(m, 0, 70, C, st);
                    m.Ball(0, -15, 10, 380, 170, 42, C);
                    z = 54; break;
                }
            case 4: m.Ball(0, -30, 0, 395, 178, 70, C); m.Ball(-235, 195, 0, 38, C); m.Ball(-300, 255, 0, 20, C); z = 72; break;
            default: m.Box(0, -25, 0, 760, 320, 90, C, .35f); Tail(-160, 142); z = 47; break;
        }
        m.Text(s, 82, 0, a == 4 ? -30 : -25, z, 8, tc, 620);
    }

    static partial void RegisterF1h(Dictionary<Type, QuickModel> d)
    {
        Type[] bars = [typeof(S1292Parameter), typeof(S1293Parameter), typeof(S1294Parameter), typeof(S1295Parameter), typeof(S1296Parameter), typeof(S1297Parameter)];
        for (int i = 0; i < bars.Length; i++) { int p = 50 + i * 10; Fam(d, bars[i], (m, t, C, D) => ProgressBar3D(m, C, D, p)); }
        Type[] bats = [typeof(S1298Parameter), typeof(S1299Parameter), typeof(S1300Parameter), typeof(S1301Parameter), typeof(S1302Parameter)];
        for (int i = 0; i < bats.Length; i++) { int a = i; Fam(d, bats[i], (m, t, C, D) => Battery3D(m, t, C, D, a)); }
        Type[] sig = [typeof(S1303Parameter), typeof(S1304Parameter), typeof(S1305Parameter), typeof(S1306Parameter)];
        for (int i = 0; i < sig.Length; i++) { int a = i + 1; Fam(d, sig[i], (m, t, C, D) => Signal3D(m, C, D, a)); }
        Type[] balloons =
        [
            typeof(S1307Parameter), typeof(S1308Parameter), typeof(S1309Parameter), typeof(S1310Parameter), typeof(S1311Parameter),
            typeof(S1312Parameter), typeof(S1313Parameter), typeof(S1314Parameter), typeof(S1315Parameter), typeof(S1316Parameter),
            typeof(S1317Parameter), typeof(S1318Parameter), typeof(S1319Parameter), typeof(S1320Parameter), typeof(S1321Parameter),
        ];
        for (int i = 0; i < balloons.Length; i++) { int a = i / 3; FamT(d, balloons[i], (m, t, C, D, s) => Balloon3D(m, C, D, a, s)); }
    }
}
