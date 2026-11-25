using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1232〜1261
//   カレンダー 10〜12月 / 曜日(角の丸い厚いタイル) / 干支(二重の金の輪の丸い札) / 星座(浮かぶ星と線、名前の札)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>曜日: 角の丸い厚いタイル、浮き出た枠、白い文字</summary>
    static void DayTile(Sculpt m, Vector4 C, Vector4 D, string s)
    {
        m.Box(0, 0, 0, 440, 440, 90, C, .28f);
        var r = new List<Vector3>();
        foreach (var (cx, cy, a0) in new[] { (159f, -159f, -1.5708f), (159f, 159f, 0f), (-159f, 159f, 1.5708f), (-159f, -159f, 3.1416f) })
            r.AddRange(Sculpt.ArcPts(cx, cy, 40, a0, a0 + 1.5708f, 6).Select(v => new Vector3(v, 46)));
        r.Add(r[0]);
        m.Tube(r, 6, D);
        m.Text(s, FitSize(s, 320, 260), 0, 0, 50, 22, D, 320);
    }

    /// <summary>干支: 厚い丸い札、二重の金の輪、文字</summary>
    static void ZodiacPlaque(Sculpt m, Vector4 C, Vector4 D, string s)
    {
        m.Disc(0, 0, 0, 240, 240, 70, C, .3f);
        m.Torus(0, 0, 32, 222, 5, D);
        m.Torus(0, 0, 34, 192, 2.5f, D);
        m.Text(s, FitSize(s, 300, 250), 0, 0, 38, 18, new Vector4(1, 1, 1, C.W), 300);
    }

    /// <summary>星座: 夜空の円盤、高さの違う所に浮かぶ星と結ぶ線、名前の札</summary>
    static void Constellation3D(Sculpt m, float t, Vector4 C, Vector4 D, int a, string s)
    {
        m.Disc(0, -35, 0, 262, 262, 40, C, .25f);
        var q = Q6Art.Sky[Math.Clamp(a, 0, Q6Art.Sky.Length - 1)];
        int n = q.Length / 2;
        var p = new Vector3[n];
        for (int i = 0; i < n; i++) p[i] = new(q[i * 2], q[i * 2 + 1] - 35, 40 + (i * 37 % 3) * 30);
        for (int i = 0; i + 1 < n; i++) m.Limb(p[i].X, p[i].Y, p[i].Z, p[i + 1].X, p[i + 1].Y, p[i + 1].Z, 2, 2, m.C(0xFFFFFF, .55f));
        for (int i = 0; i < n; i++)
        {
            float r = 25 + MathF.Sin(t * 2.4f + i * 2) * 7f; var c = p[i];
            m.Move(0, 0, c.Z, () => Gem(m, Pts(StarPts(c.X, c.Y, r, 4, .34f, 0f)), c.X, c.Y, 0f, 3, 10, D, D));
            m.Limb(c.X, c.Y, 20, c.X, c.Y, c.Z - 6, 1, 1, m.C(0xFFFFFF, .18f));
        }
        m.Box(0, 240, 10, 390, 86, 40, D, .3f);
        m.Text(s, 50, 0, 240, 32, 6, C, 355);
    }

    static partial void RegisterF1f(Dictionary<Type, QuickModel> d)
    {
        foreach (var ty in new[] { typeof(S1232Parameter), typeof(S1233Parameter), typeof(S1234Parameter) }) FamT(d, ty, (m, t, C, D, s) => DeskCalendar(m, C, D, s));
        Type[] days = [typeof(S1235Parameter), typeof(S1236Parameter), typeof(S1237Parameter), typeof(S1238Parameter), typeof(S1239Parameter), typeof(S1240Parameter), typeof(S1241Parameter)];
        foreach (var ty in days) FamT(d, ty, (m, t, C, D, s) => DayTile(m, C, D, s));
        Type[] eto =
        [
            typeof(S1242Parameter), typeof(S1243Parameter), typeof(S1244Parameter), typeof(S1245Parameter), typeof(S1246Parameter), typeof(S1247Parameter),
            typeof(S1248Parameter), typeof(S1249Parameter), typeof(S1250Parameter), typeof(S1251Parameter), typeof(S1252Parameter), typeof(S1253Parameter),
        ];
        foreach (var ty in eto) FamT(d, ty, (m, t, C, D, s) => ZodiacPlaque(m, C, D, s));
        Type[] stars = [typeof(S1254Parameter), typeof(S1255Parameter), typeof(S1256Parameter), typeof(S1257Parameter), typeof(S1258Parameter), typeof(S1259Parameter), typeof(S1260Parameter), typeof(S1261Parameter)];
        for (int i = 0; i < stars.Length; i++) { int a = i; FamT(d, stars[i], (m, t, C, D, s) => Constellation3D(m, t, C, D, a, s)); }
    }
}
