using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1172〜1201
//   顔: 20種類(怒り・眠い・やられた・どや顔など) / 丸数字: 0〜9(厚いメダルに浮き出た数字)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>文字入りの仲間図形(設定の文字も渡す)</summary>
    static void FamT(Dictionary<Type, QuickModel> d, Type type, Action<Sculpt, float, Vector4, Vector4, string> draw) =>
        d[type] = (p, m, t, v, v2) => draw(m, t, m.C(p.Color1), m.C(p.Color2), Str(p).Replace("\n", " ").Trim());

    /// <summary>2Dの Fit と同じ: 文字が枠の幅に収まる大きさ</summary>
    static float FitSize(string s, float maxW, float size)
    {
        float units = 0f;
        foreach (char ch in s) units += ch < 128 ? .64f : 1f;
        return units > 0f ? MathF.Min(size, maxW * .9f / units) : size;
    }

    /// <summary>丸数字: ふちの丸い厚いメダル、まわりの輪、浮き出た白い文字</summary>
    static void RoundNumber(Sculpt m, Vector4 C, Vector4 D, string s)
    {
        m.Disc(0, 0, 0, 235, 235, 80, C, .35f);
        m.Torus(0, 0, 36, 214, 9, D);
        m.Text(s, FitSize(s, 360, 270), 0, 0, 40, 24, new Vector4(1, 1, 1, D.W), 360);
    }

    static partial void RegisterF1d(Dictionary<Type, QuickModel> d)
    {
        (Type type, int a, int b, int c)[] faces =
        [
            (typeof(S1172Parameter), 3, 3, 3), (typeof(S1173Parameter), 4, 2, 0), (typeof(S1174Parameter), 4, 3, 4), (typeof(S1175Parameter), 4, 6, 4),
            (typeof(S1176Parameter), 4, 5, 4), (typeof(S1177Parameter), 2, 2, 5), (typeof(S1178Parameter), 2, 4, 5), (typeof(S1179Parameter), 1, 2, 5),
            (typeof(S1180Parameter), 5, 4, 0), (typeof(S1181Parameter), 5, 5, 1), (typeof(S1182Parameter), 5, 1, 1), (typeof(S1183Parameter), 2, 4, 2),
            (typeof(S1184Parameter), 2, 5, 1), (typeof(S1185Parameter), 1, 7, 0), (typeof(S1186Parameter), 2, 0, 6), (typeof(S1187Parameter), 3, 0, 6),
            (typeof(S1188Parameter), 2, 3, 0), (typeof(S1189Parameter), 0, 2, 0), (typeof(S1190Parameter), 2, 6, 0), (typeof(S1191Parameter), 1, 1, 6),
        ];
        foreach (var (type, a, b, c) in faces) Fam(d, type, (m, t, C, D) => Face3D(m, t, C, D, a, b, c));

        Type[] nums =
        [
            typeof(S1192Parameter), typeof(S1193Parameter), typeof(S1194Parameter), typeof(S1195Parameter), typeof(S1196Parameter),
            typeof(S1197Parameter), typeof(S1198Parameter), typeof(S1199Parameter), typeof(S1200Parameter), typeof(S1201Parameter),
        ];
        foreach (var ty in nums) FamT(d, ty, (m, t, C, D, s) => RoundNumber(m, C, D, s));
    }
}
