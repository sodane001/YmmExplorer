using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 218番 1032〜1061
//   人物(残りのポーズ) / 図解 フローチャート〜温度計(立体の箱・玉・管で組んだ図とグラフ。20〜29 は v111 で追加)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>立体の矢印(細い軸ととがった先)</summary>
    static void Arr3(Sculpt m, float x0, float y0, float x1, float y1, float z, Vector4 col)
    {
        var d = new Vector2(x1 - x0, y1 - y0); float l = d.Length(); if (l < 1f) return; d /= l;
        m.Limb(x0, y0, z, x1 - d.X * 26, y1 - d.Y * 26, z, 4.5f, 4.5f, col);
        m.Limb(x1 - d.X * 34, y1 - d.Y * 34, z, x1, y1, z, 19, 1, col);
    }

    /// <summary>ドーナツの一切れ(外の弧と内の弧で囲んだ形)を厚い板にする</summary>
    static void RingSector(Sculpt m, float r0, float r1, float a0, float a1, float z, float d, Vector4 col)
    {
        var pts = new List<Vector2>();
        int n = Math.Max(3, (int)((a1 - a0) * 16));
        for (int i = 0; i <= n; i++) { float a = a0 + (a1 - a0) * i / n; pts.Add(new(MathF.Cos(a) * r1, MathF.Sin(a) * r1)); }
        for (int i = n; i >= 0; i--) { float a = a0 + (a1 - a0) * i / n; pts.Add(new(MathF.Cos(a) * r0, MathF.Sin(a) * r0)); }
        m.Slab(pts, z, d, col);
    }

    static void Diagram3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var W = new Vector4(1, 1, 1, C.W);
        Vector4 Tone(int i, int n) => Vector4.Lerp(C, D, n <= 1 ? 0f : i / (n - 1f)) with { W = C.W };
        void Box(float x, float y, float w, float h, Vector4 c, float d = 50, float z = 0) => m.Box(x, y, z, w, h, d, c, .22f);
        float w0 = .5f + .5f * MathF.Sin(t * 1.6f);
        switch (a)
        {
            case 0: for (int i = 0; i < 3; i++) { Box(0, -200 + i * 200, 330, 110, Tone(i, 3)); if (i < 2) Arr3(m, 0, -140 + i * 200, 0, -62 + i * 200, 0, W); } break;
            case 1:
                Box(0, -230, 280, 86, C); Arr3(m, 0, -183, 0, -118, 0, W);
                Gem(m, Pts(0, -115, 170, 0, 0, 115, -170, 0), 0, 0, .55f, 12, 40, D, Vector4.Lerp(D, W, .25f));
                foreach (int s in Sides) { m.Limb(s * 170, 0, 0, s * 260, 0, 0, 4.5f, 4.5f, W); Arr3(m, s * 260, 0, s * 260, 130, 0, W); Box(s * 260, 190, 210, 96, Tone(s > 0 ? 1 : 0, 2)); }
                break;
            case 2:
                for (int i = 0; i < 4; i++)
                {
                    float mm = i * 1.5708f - 1.5708f + t * .25f, e = mm + 1.15f;
                    m.Tube(ArcPts3(0, 0, 210, mm + .42f, e, 0, 10), 4.5f, W);
                    m.Limb(MathF.Cos(e) * 210, MathF.Sin(e) * 210, 0, MathF.Cos(e + .16f) * 210, MathF.Sin(e + .16f) * 210, 0, 17, 1, W);
                    m.Ball(MathF.Cos(mm) * 210, MathF.Sin(mm) * 210, 0, 66, Tone(i, 4));
                }
                break;
            case 3: m.Disc(-110, 0, -20, 190, 190, 30, C with { W = C.W * .72f }, .4f); m.Disc(110, 0, 20, 190, 190, 30, D with { W = D.W * .72f }, .4f); break;
            case 4:
                m.Disc(0, -100, -30, 165, 165, 26, C with { W = C.W * .68f }, .4f);
                m.Disc(-105, 85, 0, 165, 165, 26, D with { W = D.W * .68f }, .4f);
                m.Disc(105, 85, 30, 165, 165, 26, m.C(0x35B56A, .68f), .4f);
                break;
            case 5: // ピラミッド図: 段ごとの四角すい台
                for (int i = 0; i < 4; i++) { float y = -260 + i * 135, a0 = 58 + i * 75, b0 = a0 + 62; SquareFrustum(m, 0, 0, y, i == 0 ? 0 : a0, y + 122, b0, Tone(i, 4)); }
                break;
            case 6: // じょうご図: 下ほど細くなる段
                for (int i = 0; i < 4; i++) { float y = -260 + i * 135, a0 = 340 - i * 72, b0 = a0 - 58; SquareFrustum(m, 0, 0, y, a0, y + 122, b0, Tone(i, 4)); }
                break;
            case 7: for (int y = 0; y < 2; y++) for (int x = 0; x < 2; x++) { int k = x + y * 2; Box(-125 + x * 250, -125 + y * 250, 232, 232, Tone(k, 4), 40 + k * 30, (40 + k * 30) / 2f); } break;
            case 8: for (int y = 0; y < 3; y++) for (int x = 0; x < 3; x++) { int k = x + y; Box(-170 + x * 170, -170 + y * 170, 156, 156, Tone(k, 5), 30 + k * 22, (30 + k * 22) / 2f); } break;
            case 9:
                for (int i = 0; i < 6; i++) { float h = (110 + 300 * (.5f + .5f * MathF.Sin(i * 1.7f + 1f))) * (.92f + w0 * .08f); m.Box(-225 + i * 90, 240 - h / 2f, 0, 62, h, 62, Tone(i, 6), .12f); }
                m.Box(0, 245, 0, 580, 7, 90, W, .3f);
                break;
            case 10:
                for (int i = 0; i < 5; i++) { float l = 150 + 380 * (.5f + .5f * MathF.Sin(i * 2.1f + .4f)); m.Box(-270 + l / 2f, -200 + i * 100, 0, l, 66, 66, Tone(i, 5), .12f); }
                m.Box(-275, 0, 0, 7, 510, 90, W, .3f);
                break;
            case 11:
            case 12:
                {
                    var q = new List<Vector2>(); for (int i = 0; i < 8; i++) q.Add(new(-270 + i * 77, 110 - 240 * (.5f + .5f * MathF.Sin(i * 1.05f + .3f)) * (.35f + i * .09f)));
                    if (a == 12) { var f = new List<Vector2>(q) { new(269, 220), new(-270, 220) }; f.Reverse(); m.Slab(f, -10, 60, C with { W = C.W * .55f }); }
                    m.Tube([.. q.Select(v => new Vector3(v, 0))], 6, C);
                    foreach (var v in q) m.Ball(v.X, v.Y, 0, 15, D);
                    m.Limb(-300, 222, 0, 300, 222, 0, 3, 3, W); m.Limb(-298, -230, 0, -298, 224, 0, 3, 3, W);
                    break;
                }
            case 13:
                for (int i = 0; i < 34; i++) { float x = -260 + H01(i) * 520, y = 170 - (x + 260) * .62f + (H01(i + 50) - .5f) * 130; m.Ball(x, y, (H01(i + 90) - .5f) * 120, 13, i % 2 == 0 ? C : D); }
                m.Limb(-292, 222, 0, 292, 222, 0, 3, 3, W); m.Limb(-290, -230, 0, -290, 224, 0, 3, 3, W);
                break;
            case 14: { float[] v = [.4f, .27f, .2f, .13f]; float mm = -1.5708f; for (int i = 0; i < 4; i++) { float e = mm + v[i] * MathF.Tau; RingSector(m, 144, 236, mm + .03f, e - .03f, 0, 60 + (3 - i) * 16, Tone(i, 4)); mm = e; } break; }
            case 15: // レーダーチャート: 目盛りの網と、中心が盛り上がった値の形
                {
                    float[] v = [.9f, .62f, .78f, .5f, .86f, .68f];
                    for (int r = 1; r <= 3; r++) m.Tube([.. Enumerable.Range(0, 7).Select(i => { float a0 = i * 1.0472f - 1.5708f; return new Vector3(MathF.Cos(a0) * r * 85, MathF.Sin(a0) * r * 85, 0); })], 1.5f, m.C(0xFFFFFF, .5f));
                    var pts = new List<Vector3>();
                    for (int i = 0; i < 6; i++) { float a0 = i * 1.0472f - 1.5708f; m.Limb(0, 0, 0, MathF.Cos(a0) * 255, MathF.Sin(a0) * 255, 0, 1.5f, 1.5f, m.C(0xFFFFFF, .4f)); pts.Add(new(MathF.Cos(a0) * 255 * v[i], MathF.Sin(a0) * 255 * v[i], 4)); }
                    for (int i = 0; i < 6; i++) m.Tri3D(new(0, 0, 70), pts[i], pts[(i + 1) % 6], C with { W = C.W * .8f });
                    pts.Add(pts[0]); m.Tube(pts, 4, D);
                    break;
                }
            case 16:
                for (int i = 0; i < 5; i++) { float y = 230; for (int k = 0; k < 3; k++) { float h = 62 + 74 * (.5f + .5f * MathF.Sin(i * 1.3f + k * 2f)); m.Box(-200 + i * 100, y - h / 2f, 0, 68, h - 3, 68, Tone(k, 3), .1f); y -= h; } }
                m.Box(0, 234, 0, 540, 6, 90, W, .3f);
                break;
            case 17:
                m.Limb(-310, 0, 0, 310, 0, 0, 4.5f, 4.5f, W);
                for (int i = 0; i < 5; i++) { float x = -250 + i * 125, s = i % 2 == 0 ? -1 : 1; m.Limb(x, 0, 0, x, s * 90, 0, 2, 2, W); m.Ball(x, 0, 0, 21, Tone(i, 5)); Box(x, s * 140, 112, 72, Tone(i, 5), 40); }
                break;
            case 20: // 相関図: まわりを回る玉と、中心の大きな玉
                for (int i = 0; i < 6; i++) { float mm = i * 1.0472f + t * .18f, x = MathF.Cos(mm) * 235, y = MathF.Sin(mm) * 205, z = MathF.Sin(mm * 2f) * 60f; m.Limb(0, 0, 0, x, y, z, 3, 3, W); m.Ball(x, y, z, 54, Tone(i, 6)); }
                m.Ball(0, 0, 0, 88, C);
                break;
            case 21: // 比較表: 2枚の厚い板と、浮き出た白い行
                foreach (int s in Sides)
                {
                    m.Box(s * 160, 0, 0, 292, 500, 50, (s < 0 ? C : D) with { W = C.W * .92f }, .1f);
                    for (int i = 0; i < 4; i++) m.Box(s * 160, -130 + i * 88, 28, 214, 26, 10, m.C(0xFFFFFF, .85f), .5f);
                }
                break;
            case 22: // 天びん: 柱と台、ゆれるさお、ひもと半球の皿
                {
                    float k = MathF.Sin(t * 1.3f) * .14f;
                    m.Cyl(0, 10, 0, 10, 460, W, .2f);
                    m.Box(0, 250, 0, 220, 24, 160, W, .3f);
                    m.Turn(0, -190, k, () =>
                    {
                        m.Limb(-250, -190, 0, 250, -190, 0, 7, 7, W);
                        foreach (int s in Sides)
                        {
                            foreach (int r in Sides) m.Limb(s * 250, -190, 0, s * 250 + r * 80, 0, 0, 1.5f, 1.5f, W);
                            m.Limb(s * 250, -190, 0, s * 250, 0, 80, 1.5f, 1.5f, W);
                            var prof = new List<Vector2>();
                            for (int i = 0; i <= 8; i++) { float a0 = i / 8f * 1.5708f; prof.Add(new(MathF.Sin(a0) * 82, MathF.Cos(a0) * 82)); }
                            m.Lathe(s * 250, 0, s < 0 ? C : D, prof);
                        }
                    });
                    m.Ball(0, -190, 10, 21, W);
                    break;
                }
            case 23: // 成長の階段: 奥行きのある段と、上へのびる矢印
                for (int i = 0; i < 5; i++) { float h = 95 + i * 95; m.Box(-240 + i * 120, 250 - h / 2f, 0, 110, h, 140, Tone(i, 5), .08f); }
                Arr3(m, -250, 60, 240, -310, 80, W);
                break;
            case 24: // ガントチャート: 行の線、期間の棒、動く今日の線
                for (int i = 0; i < 5; i++) { float s0 = i * 72, l = 170 + (i % 3) * 66; m.Limb(-280, -200 + i * 100, 0, 300, -200 + i * 100, 0, 1.5f, 1.5f, m.C(0xFFFFFF, .22f)); m.Box(-230 + s0 + l / 2f, -200 + i * 100, 0, l, 58, 50, Tone(i, 5), .4f); }
                m.Box(-230 + w0 * 420, 0, 30, 10, 510, 10, W, .4f);
                break;
            case 25: // ヒートマップ: 値に合わせて高さが変わる柱のマス
                for (int y = 0; y < 6; y++) for (int x = 0; x < 8; x++) { float v = .5f + .5f * MathF.Sin(x * .9f + y * 1.3f + t * .8f), h = 20 + v * 90; m.Box(-245 + x * 70, -175 + y * 70, h / 2f, 65, 65, h, Vector4.Lerp(D, C, v) with { W = C.W }, .15f); }
                break;
            case 26: // マインドマップ: 中心から曲がってのびる枝と、楕円の玉
                for (int i = 0; i < 6; i++)
                {
                    float mm = i * 1.0472f + .5f, x = MathF.Cos(mm) * 260, y = MathF.Sin(mm) * 205, cx = MathF.Cos(mm + .3f) * 130, cy = MathF.Sin(mm + .3f) * 120, z = (i % 2 == 0 ? 1 : -1) * 50;
                    m.Tube([new(0, 0, 0), new(cx, cy, z * .5f), new(x, y, z)], 4, Tone(i, 6));
                    m.Ball(x, y, z, 78, 40, 36, Tone(i, 6));
                }
                m.Ball(0, 0, 0, 118, 72, 50, W); m.Ball(0, 0, 14, 104, 60, 46, C);
                break;
            case 27: // 数直線: 管の直線と矢じり、目盛りと数字、動く印
                {
                    m.Limb(-320, 0, 0, 310, 0, 0, 4, 4, W); m.Limb(305, 0, 0, 345, 0, 0, 24, 1, W);
                    for (int i = -5; i <= 5; i++) { m.Limb(i * 56, -22, 0, i * 56, 22, 0, i == 0 ? 3.5f : 2, i == 0 ? 3.5f : 2, W); if (i % 5 == 0) m.Text(i.ToString(), 44, i * 56, 62, 4, 6, W); }
                    float px = MathF.Sin(t) * 200;
                    m.Ball(px, -52, 0, 22, C); m.Limb(px, -40, 0, px, -12, 0, 16, 1, C);
                    break;
                }
            case 28: // 座標とグラフ: 2本の軸と矢じり、波打つ管の曲線
                {
                    m.Limb(-310, 0, 0, 300, 0, 0, 3, 3, W); m.Limb(0, 260, 0, 0, -250, 0, 3, 3, W);
                    m.Limb(298, 0, 0, 330, 0, 0, 18, 1, W); m.Limb(0, -248, 0, 0, -280, 0, 18, 1, W);
                    var q = new List<Vector3>(); for (int i = 0; i <= 60; i++) { float x = -280 + i * 9.33f; q.Add(new(x, -MathF.Sin(x * .022f + t) * 150, 6)); }
                    m.Tube(q, 4.5f, C);
                    break;
                }
            case 29: // 温度計: 透けたガラスの管と玉、上下する赤い液、目盛り
                {
                    float h = 110 + w0 * 300;
                    m.Box(0, -50, 0, 112, 460, 112, m.C(0xFFFFFF, .3f), .5f);
                    m.Ball(0, 200, 0, 98, m.C(0xFFFFFF, .3f));
                    m.Ball(0, 200, 0, 74, C);
                    m.Cyl(0, 170 - h / 2f, 0, 21, h, C, .2f);
                    for (int i = 0; i < 7; i++) m.Box(79, -240 + i * 56, 0, 34, 5, 5, W, .5f);
                    break;
                }
            default: // 18 ツリー図 / 19 組織図
                {
                    float[] x1 = [-160, 160], x2 = [-240, -80, 80, 240];
                    foreach (var x in x1) m.Limb(0, -180, 0, x, 0, 0, 3, 3, W);
                    for (int i = 0; i < 4; i++) m.Limb(x1[i / 2], 0, 0, x2[i], 180, 0, 3, 3, W);
                    var mid = Vector4.Lerp(C, D, .5f) with { W = C.W };
                    if (a == 18) { m.Ball(0, -180, 0, 58, C); foreach (var x in x1) m.Ball(x, 0, 0, 48, mid); foreach (var x in x2) m.Ball(x, 180, 0, 38, D); }
                    else { Box(0, -180, 190, 82, C); foreach (var x in x1) Box(x, 0, 172, 72, mid); foreach (var x in x2) Box(x, 180, 134, 66, D); }
                    break;
                }
        }
    }

    static partial void RegisterG1b(Dictionary<Type, QuickModel> d)
    {
        Type[] people = [typeof(X1032Parameter), typeof(X1033Parameter), typeof(X1034Parameter), typeof(X1035Parameter), typeof(X1036Parameter), typeof(X1037Parameter), typeof(X1038Parameter), typeof(X1039Parameter), typeof(X1040Parameter), typeof(X1041Parameter)];
        for (int i = 0; i < people.Length; i++) { int a = 30 + i; Fam(d, people[i], (m, t, C, D) => Person3D(m, t, C, D, a)); }
        Type[] diagrams =
        [
            typeof(X1042Parameter), typeof(X1043Parameter), typeof(X1044Parameter), typeof(X1045Parameter), typeof(X1046Parameter),
            typeof(X1047Parameter), typeof(X1048Parameter), typeof(X1049Parameter), typeof(X1050Parameter), typeof(X1051Parameter),
            typeof(X1052Parameter), typeof(X1053Parameter), typeof(X1054Parameter), typeof(X1055Parameter), typeof(X1056Parameter),
            typeof(X1057Parameter), typeof(X1058Parameter), typeof(X1059Parameter), typeof(X1060Parameter), typeof(X1061Parameter),
        ];
        for (int i = 0; i < diagrams.Length; i++) { int a = i; Fam(d, diagrams[i], (m, t, C, D) => Diagram3D(m, t, C, D, a)); }
    }
}
