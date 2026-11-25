using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1202〜1231
//   丸数字 10〜20 / 順位 1〜10位(リボン付きのロゼット) / カレンダー 1〜9月(厚い日めくり)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>順位: うしろへ垂れる2本のリボン、ぎざぎざに面取りしたロゼット、白いふちとメダル、文字</summary>
    static void RankMedal(Sculpt m, float t, Vector4 C, Vector4 D, string s)
    {
        var K = m.C(0x20242B); var W = new Vector4(1, 1, 1, C.W);
        m.Pitch(0, 30, -30, .35f, () =>
        {
            m.Slab(-30, 14, D, -125, 30, -35, 40, -55, 310, -110, 265, -175, 280);
            m.Slab(-30, 14, D, 125, 30, 35, 40, 55, 310, 110, 265, 175, 280);
        });
        m.Turn(0, -35, t * .15f, () => Gem(m, Pts(StarPts(0, -35, 240, 16, .88f, 0f)), 0, -35, .8f, 14, 34, C, Sculpt.Dark(C, .92f)));
        m.Disc(0, -35, 30, 175, 175, 24, W, .4f);
        m.Disc(0, -35, 38, 158, 158, 26, C, .35f);
        m.Text(s, FitSize(s, 270, 140), 0, -35, 52, 14, K, 270);
    }

    /// <summary>カレンダー: 厚い紙の束、色の上の帯、2つのとじ金具、月の文字</summary>
    static void DeskCalendar(Sculpt m, Vector4 C, Vector4 D, string s)
    {
        var K = m.C(0x20242B); var W = new Vector4(1, 1, 1, C.W);
        m.Box(0, 15, 0, 460, 450, 110, W, .08f);
        for (int i = 1; i <= 4; i++) m.Box(0, 240.5f, 55 - i * 22, 440, 1, 4, m.C(0xC9D0D8), .5f);
        m.Box(0, -110, 4, 466, 200, 116, C, .1f);
        foreach (int x in new[] { -125, 125 })
        {
            m.Limb(x, -270, 0, x, -180, 0, 17, 17, D);
            m.Ball(x, -205, 60, 16, 16, 6, Sculpt.Dark(C, .6f));
        }
        m.Text(s, FitSize(s, 390, 170), 0, 85, 58, 14, K, 390);
    }

    static partial void RegisterF1e(Dictionary<Type, QuickModel> d)
    {
        Type[] nums =
        [
            typeof(S1202Parameter), typeof(S1203Parameter), typeof(S1204Parameter), typeof(S1205Parameter), typeof(S1206Parameter), typeof(S1207Parameter),
            typeof(S1208Parameter), typeof(S1209Parameter), typeof(S1210Parameter), typeof(S1211Parameter), typeof(S1212Parameter),
        ];
        foreach (var ty in nums) FamT(d, ty, (m, t, C, D, s) => RoundNumber(m, C, D, s));
        Type[] ranks =
        [
            typeof(S1213Parameter), typeof(S1214Parameter), typeof(S1215Parameter), typeof(S1216Parameter), typeof(S1217Parameter),
            typeof(S1218Parameter), typeof(S1219Parameter), typeof(S1220Parameter), typeof(S1221Parameter), typeof(S1222Parameter),
        ];
        foreach (var ty in ranks) FamT(d, ty, (m, t, C, D, s) => RankMedal(m, t, C, D, s));
        Type[] cals =
        [
            typeof(S1223Parameter), typeof(S1224Parameter), typeof(S1225Parameter), typeof(S1226Parameter), typeof(S1227Parameter),
            typeof(S1228Parameter), typeof(S1229Parameter), typeof(S1230Parameter), typeof(S1231Parameter),
        ];
        foreach (var ty in cals) FamT(d, ty, (m, t, C, D, s) => DeskCalendar(m, C, D, s));
    }
}
