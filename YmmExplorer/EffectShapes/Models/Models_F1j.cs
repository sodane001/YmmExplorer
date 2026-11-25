using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 仲間図形 1352〜1381
//   帯 矢印札(残り) / 枠 細線・二重線・角丸・四隅・点線・ドット・フィルム・星飾り・六角(額縁のような立体の枠)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>角の丸い四角の輪郭(閉じた点の並び)</summary>
    static List<Vector3> RoundRectPath(float w, float h, float r, float z)
    {
        float hw = w / 2f - r, hh = h / 2f - r;
        var l = new List<Vector3>();
        foreach (var (cx, cy, a0) in new[] { (hw, -hh, -1.5708f), (hw, hh, 0f), (-hw, hh, 1.5708f), (-hw, -hh, 3.1416f) })
            l.AddRange(Sculpt.ArcPts(cx, cy, r, a0, a0 + 1.5708f, r > 10 ? 8 : 1).Select(v => new Vector3(v, z)));
        l.Add(l[0]);
        return l;
    }

    /// <summary>角が四角い額縁: 4本の棒(太さ bw、奥行き d)</summary>
    static void BarFrame(Sculpt m, float w, float h, float bw, float d, Vector4 col)
    {
        m.Box(0, -h / 2f, 0, w + bw, bw, d, col, .25f); m.Box(0, h / 2f, 0, w + bw, bw, d, col, .25f);
        m.Box(-w / 2f, 0, 0, bw, h - bw, d, col, .25f); m.Box(w / 2f, 0, 0, bw, h - bw, d, col, .25f);
    }

    static void Frame3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        const float w = 1180, h = 650;
        int keep = m.Seg; m.Seg = 12;
        switch (a)
        {
            case 1: // 二重線: 太い額縁と、内側の細い線
                BarFrame(m, w, h, 14, 30, C);
                m.Tube(RoundRectPath(w - 54, h - 54, 2, 4), 3, D);
                break;
            case 2: // 角丸: 角の丸い太い管
                m.Tube(RoundRectPath(w, h, 66, 0), 9, C);
                break;
            case 3: // 四隅: 4つの角だけのL字金具
                foreach (int x in Sides) foreach (int y in Sides)
                    {
                        m.Limb(x * w / 2f, y * h / 2f, 0, x * (w / 2f - 150), y * h / 2f, 0, 9, 9, C);
                        m.Limb(x * w / 2f, y * h / 2f, 0, x * w / 2f, y * (h / 2f - 150), 0, 9, 9, C);
                        m.Ball(x * w / 2f, y * h / 2f, 0, 12, C);
                    }
                break;
            case 4: // 点線: ぐるりと流れる短い棒
                {
                    float mm = Frac(t * .4f) * 60f;
                    for (float x = -w / 2f + mm; x < w / 2f; x += 60) { m.Box(x + 17, -h / 2f, 0, 34, 10, 12, C, .4f); m.Box(-x - 17, h / 2f, 0, 34, 10, 12, C, .4f); }
                    for (float y = -h / 2f + mm; y < h / 2f; y += 60) { m.Box(w / 2f, y + 17, 0, 10, 34, 12, C, .4f); m.Box(-w / 2f, -y - 17, 0, 10, 34, 12, C, .4f); }
                    break;
                }
            case 5: // ドット: 並んだ玉
                for (int i = 0; i < 28; i++) { float x = -w / 2f + i * w / 27f; m.Ball(x, -h / 2f, 0, 11, C); m.Ball(x, h / 2f, 0, 11, C); }
                for (int i = 1; i < 15; i++) { float y = -h / 2f + i * h / 15f; m.Ball(-w / 2f, y, 0, 11, C); m.Ball(w / 2f, y, 0, 11, C); }
                break;
            case 6: // フィルム: 上下の厚いフィルムの帯と、送り穴
                foreach (int s in Sides)
                {
                    m.Box(0, s * h / 2f, 0, w, 72, 16, C, .08f);
                    for (int i = 0; i < 18; i++) m.Box(-w / 2f + 34 + i * 66, s * h / 2f, 6, 34, 34, 8, D, .3f);
                }
                break;
            case 7: // 星飾り: 細い管の枠と、四隅の立体の星
                m.Tube(RoundRectPath(w, h, 8, 0), 4.5f, C);
                foreach (int x in Sides) foreach (int y in Sides)
                    {
                        float cx = x * w / 2f, cy = y * h / 2f;
                        Gem(m, Pts(StarPts(cx, cy, 58, 4, .35f, 0f)), cx, cy, 0f, 4, 22, D, D);
                    }
                break;
            case 8: // 六角: 左右がとがった六角の太い管
                m.Tube([V3(-w / 2f, -h / 2f), V3(w / 2f, -h / 2f), V3(w / 2f + 60, 0), V3(w / 2f, h / 2f), V3(-w / 2f, h / 2f), V3(-w / 2f - 60, 0), V3(-w / 2f, -h / 2f)], 7.5f, C);
                foreach (var (x, y) in new[] { (-w / 2f, -h / 2f), (w / 2f, -h / 2f), (w / 2f + 60, 0f), (w / 2f, h / 2f), (-w / 2f, h / 2f), (-w / 2f - 60, 0f) }) m.Ball(x, y, 0, 7.5f, C);
                break;
            default: // 細線: 細い丸い枠
                m.Tube(RoundRectPath(w, h, 2, 0), 4.5f, C);
                break;
        }
        m.Seg = keep;
    }

    static partial void RegisterF1j(Dictionary<Type, QuickModel> d)
    {
        foreach (var ty in new[] { typeof(S1352Parameter), typeof(S1353Parameter), typeof(S1354Parameter) }) FamT(d, ty, (m, t, C, D, s) => Ribbon3D(m, C, D, 5, s));
        Type[] frames =
        [
            typeof(S1355Parameter), typeof(S1356Parameter), typeof(S1357Parameter), typeof(S1358Parameter), typeof(S1359Parameter), typeof(S1360Parameter),
            typeof(S1361Parameter), typeof(S1362Parameter), typeof(S1363Parameter), typeof(S1364Parameter), typeof(S1365Parameter), typeof(S1366Parameter),
            typeof(S1367Parameter), typeof(S1368Parameter), typeof(S1369Parameter), typeof(S1370Parameter), typeof(S1371Parameter), typeof(S1372Parameter),
            typeof(S1373Parameter), typeof(S1374Parameter), typeof(S1375Parameter), typeof(S1376Parameter), typeof(S1377Parameter), typeof(S1378Parameter),
            typeof(S1379Parameter), typeof(S1380Parameter), typeof(S1381Parameter),
        ];
        for (int i = 0; i < frames.Length; i++) { int a = i / 3; Fam(d, frames[i], (m, t, C, D) => Frame3D(m, t, C, D, a)); }
    }
}
