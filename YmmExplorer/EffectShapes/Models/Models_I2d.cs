using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: アイコン 911〜940(記号・ボタン 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。動き方も2Dと同じ
// =====================================================================
internal static partial class Models3D
{
    static List<Vector2> Pts(params float[] xy)
    {
        var l = new List<Vector2>(xy.Length / 2);
        for (int i = 0; i + 1 < xy.Length; i += 2) l.Add(new Vector2(xy[i], xy[i + 1]));
        return l;
    }

    /// <summary>ふちが丸い厚い板(板＋ふちを回る管)</summary>
    static void Puff(Sculpt m, float z, float d, Vector4 col, params float[] xy)
    {
        var p = Pts(xy);
        m.Slab(p, z, d, col);
        var rim = p.Select(v => new Vector3(v, z)).ToList(); rim.Add(rim[0]);
        m.Tube(rim, d / 2f, col);
        if (p.Count <= 12) foreach (var v in p) m.Ball(v.X, v.Y, z, d / 2f, col);
    }

    /// <summary>宝石のようにカットした形: ふちから表と裏へ面取りし、まん中は平らな面(inset=0 なら尖る)</summary>
    static void Gem(Sculpt m, List<Vector2> o, float cx, float cy, float inset, float side, float depth, Vector4 col, Vector4 face)
    {
        int n = o.Count;
        foreach (int s in Sides)
        {
            var inner = o.Select(v => new Vector3(cx + (v.X - cx) * inset, cy + (v.Y - cy) * inset, s * depth)).ToList();
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                var a = new Vector3(o[i], s * side); var b = new Vector3(o[j], s * side);
                if (inset <= 0.001f) m.Tri3D(a, b, new Vector3(cx, cy, s * depth), col);
                else m.Quad3D(a, b, inner[j], inner[i], col);
                m.Quad3D(new Vector3(o[i], side), new Vector3(o[j], side), new Vector3(o[j], -side), new Vector3(o[i], -side), col);
            }
            if (inset > 0.001f) for (int i = 0; i < n; i++) m.Tri3D(new Vector3(cx, cy, s * depth), inner[i], inner[(i + 1) % n], face);
        }
    }

    static List<Vector3> ArcPts3(float x, float y, float r, float a0, float a1, float z, int steps = 24)
        => [.. Sculpt.ArcPts(x, y, r, a0, a1, steps).Select(v => new Vector3(v, z))];

    /// <summary>丸いボタン(ふちを丸めた厚い円盤)。表の面の z を返す</summary>
    static float RoundButton(Sculpt m, Vector4 col)
    {
        m.Disc(0, 0, 0, 270, 270, 90, col, .35f);
        return 45f;
    }

    /// <summary>スピーカーの形(ボタンの表に浮かせる)</summary>
    static void Speaker(Sculpt m, float z, Vector4 col) => Puff(m, z, 34, col, -160, -55, -85, -55, 20, -145, 20, 145, -85, 55, -160, 55);

    /// <summary>トレイ(保存・送信の受け皿)</summary>
    static void Tray(Sculpt m, Vector4 col) => m.Tube([V3(-255, 120), V3(-255, 255), V3(255, 255), V3(255, 120)], 24, col);

    static partial void RegisterI2d(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        // 記号 ひし形: 宝石のようにカットしたひし形
        Icon(d, typeof(S911Parameter), (m, t, C, D) => Gem(m, Pts(0, -285, 240, 0, 0, 285, -240, 0), 0, 0, .45f, 22, 80, C, Vector4.Lerp(C, D, .4f) with { W = 1f }));
        // 記号 六角形: 面取りした六角形の宝石
        Icon(d, typeof(S912Parameter), (m, t, C, D) => Gem(m, Pts(270, 0, 135, 234, -135, 234, -270, 0, -135, -234, 135, -234), 0, 0, .63f, 30, 80, C, Vector4.Lerp(C, D, .4f) with { W = 1f }));
        // 記号 ハート: ふっくらしたハート(左右のふくらみと、つや)
        Icon(d, typeof(S913Parameter), (m, t, C, D) =>
        {
            var o = new List<float> { 0, 275 };
            foreach (var v in Sculpt.ArcPts(-118, -65, 145, 2.45f, 5.66f, 16)) { o.Add(v.X); o.Add(v.Y); }
            foreach (var v in Sculpt.ArcPts(118, -65, 145, 3.76f, 6.97f, 16)) { o.Add(v.X); o.Add(v.Y); }
            Puff(m, 0, 110, C, [.. o]);
            m.Ball(-140, -115, 58, 44, 28, 8, D);
        });
        // 記号 スター: 表と裏へとがった立体の星
        Icon(d, typeof(S914Parameter), (m, t, C, D) => Gem(m, Pts(StarPts(0, 0, 290, 5, .46f, 0f)), 0, 0, 0f, 18, 95, C, D));
        // 記号 音符: 斜めの玉、軸、旗
        Icon(d, typeof(S915Parameter), (m, t, C, D) =>
        {
            m.Turn(-105, 190, -.25f, () => m.Ball(-105, 190, 0, 100, 72, 60, C));
            m.Box(-20, -40, 0, 34, 470, 34, C, .4f);
            Puff(m, 0, 30, C, -30, -275, 160, -175, 175, -70, -30, -165);
        });
        // 記号 右矢印 / 上矢印: ふちが丸い厚い矢印
        Icon(d, typeof(S916Parameter), (m, t, C, D) => Puff(m, 0, 90, C, -275, -65, 50, -65, 50, -205, 310, 0, 50, 205, 50, 65, -275, 65));
        Icon(d, typeof(S917Parameter), (m, t, C, D) => Puff(m, 0, 90, C, -65, 275, -65, -50, -205, -50, 0, -310, 205, -50, 65, -50, 65, 275));
        // 記号 くるくる矢印: まるい管の矢印と、厚い矢じり
        Icon(d, typeof(S918Parameter), (m, t, C, D) =>
        {
            var arc = ArcPts3(0, 0, 200, -1.2f, 4.1f, 0, 32);
            m.Tube(arc, 31, C); m.Ball(arc[^1].X, arc[^1].Y, 0, 31, C);
            Puff(m, 0, 60, C, 20, -300, 190, -190, -5, -115);
        });
        // 記号 チェック: 太い管のチェック
        Icon(d, typeof(S919Parameter), (m, t, C, D) =>
        {
            m.Tube([V3(-225, 20), V3(-65, 180), V3(235, -175)], 40, C);
            foreach (var (x, y) in new[] { (-225f, 20f), (-65f, 180f), (235f, -175f) }) m.Ball(x, y, 0, 40, C);
        });
        // 記号 はてな: 曲がった管と下の点
        Icon(d, typeof(S920Parameter), (m, t, C, D) =>
        {
            var p = ArcPts3(0, -105, 125, 3.2f, 7.3f, 0, 28);
            p.Add(V3(70, 0)); p.Add(V3(0, 55)); p.Add(V3(0, 120));
            m.Tube(p, 31, C); m.Ball(p[0].X, p[0].Y, 0, 31, C); m.Ball(0, 120, 0, 31, C);
            m.Ball(0, 235, 0, 46, C);
        });
        // ボタン 再生 / 一時停止 / 停止 / 早送り: 丸いボタンに浮き出た記号
        Icon(d, typeof(S921Parameter), (m, t, C, D) => { float z = RoundButton(m, C); Puff(m, z, 34, D, -85, -140, 145, 0, -85, 140); });
        Icon(d, typeof(S922Parameter), (m, t, C, D) => { float z = RoundButton(m, C); m.Box(-70, 0, z, 70, 250, 40, D, .3f); m.Box(70, 0, z, 70, 250, 40, D, .3f); });
        Icon(d, typeof(S923Parameter), (m, t, C, D) => { float z = RoundButton(m, C); m.Box(0, 0, z, 230, 230, 40, D, .25f); });
        Icon(d, typeof(S924Parameter), (m, t, C, D) => { float z = RoundButton(m, C); Puff(m, z, 34, D, -155, -120, 5, 0, -155, 120); Puff(m, z, 34, D, 5, -120, 165, 0, 5, 120); });
        // ボタン 音量: スピーカーと、明るさが波打つ3本の音の線
        Icon(d, typeof(S925Parameter), (m, t, C, D) =>
        {
            float z = RoundButton(m, C); Speaker(m, z, D);
            for (int i = 0; i < 3; i++) { float k = .6f + .4f * MathF.Abs(MathF.Sin(t * 2f + i)); m.Tube(ArcPts3(30, 0, 55 + i * 48, -.8f, .8f, z, 10), 9, m.C(D, k)); }
        });
        // ボタン ミュート: スピーカーと赤いばつ
        Icon(d, typeof(S926Parameter), (m, t, C, D) =>
        {
            float z = RoundButton(m, C); Speaker(m, z, D);
            var R = m.C(0xFF6B6B);
            m.Limb(75, -65, z + 5, 185, 65, z + 5, 13, 13, R); m.Limb(185, -65, z + 5, 75, 65, z + 5, 13, 13, R);
        });
        // ボタン 歯車: 厚い歯車(8つの歯)と、まん中の穴の飾り
        Icon(d, typeof(S927Parameter), (m, t, C, D) =>
        {
            for (int i = 0; i < 8; i++) m.Turn(0, 0, i * .7854f, () => m.Box(0, -215, 0, 84, 110, 90, C, .25f));
            m.Disc(0, 0, 0, 205, 205, 100, C, .2f);
            m.Torus(0, 0, 50, 82, 16, Sculpt.Dark(C, .8f));
            m.Disc(0, 0, 50, 76, 76, 12, D, .5f);
        });
        // ボタン 虫めがね: 柄、ふちの輪、透けたレンズとつや
        Icon(d, typeof(S928Parameter), (m, t, C, D) =>
        {
            m.Limb(95, 95, 0, 265, 265, 0, 26, 30, C);
            m.Torus(-55, -55, 0, 180, 26, C);
            m.Disc(-55, -55, 0, 172, 172, 26, m.C(D, .6f), .6f);
            m.Tube(ArcPts3(-55, -55, 125, 3.5f, 4.8f, 14, 10), 8, W);
        });
        // ボタン ホーム: 角の丸いタイルに浮き出た家
        Icon(d, typeof(S929Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, 0, 560, 560, 90, C, .3f);
            Puff(m, 50, 30, D, -205, 0, 0, -185, 205, 0, 150, 0, 150, 185, 50, 185, 50, 65, -50, 65, -50, 185, -150, 185, -150, 0);
        });
        // ボタン メール: 封筒(薄い箱)と、ふたの折り目
        Icon(d, typeof(S930Parameter), (m, t, C, D) =>
        {
            m.Box(0, 0, 0, 590, 410, 70, C, .15f);
            var f = Sculpt.Dark(C, .92f);
            m.Tri3D(new Vector3(-280, -195, 35), new Vector3(280, -195, 35), new Vector3(0, 40, 48), f);
            m.Tube([V3(-265, -165, 38), V3(0, 55, 50), V3(265, -165, 38)], 12, D);
        });
        // ボタン 電話: 丸いボタンに浮き出た受話器
        Icon(d, typeof(S931Parameter), (m, t, C, D) =>
        {
            float z = RoundButton(m, C);
            m.Turn(0, 0, -.7854f, () =>
            {
                m.Box(0, -20, z + 10, 285, 62, 44, D, .5f);
                foreach (int s in Sides) m.Box(s * 125, 25, z + 16, 88, 120, 56, D, .5f);
            });
        });
        // ボタン 鍵閉じ / 鍵開き: 金属のつる、厚い本体、鍵穴
        void Lock(Sculpt m, Vector4 C, Vector4 D, bool open)
        {
            var g = m.C(0xB7BEC6);
            if (!open)
            {
                var p = new List<Vector3> { V3(-120, 20) }; p.AddRange(ArcPts3(0, -60, 120, 3.14f, 6.28f, 0, 16)); p.Add(V3(120, 20));
                m.Tube(p, 22, g);
            }
            else
            {
                m.Limb(-10, -105, 0, -10, 20, 0, 22, 22, g);
                m.Yaw(-10, 0, 0, -.7f, () =>
                {
                    var p = ArcPts3(110, -105, 120, 3.14f, 6.28f, 0, 16); p.Add(V3(230, -40));
                    m.Tube(p, 22, g); m.Ball(230, -40, 0, 22, g);
                });
            }
            m.Box(0, 120, 0, 390, 300, 120, C, .2f);
            m.Ball(0, 95, 60, 38, 38, 8, D);
            m.Box(0, 160, 60, 28, 86, 14, D, .4f);
        }
        Icon(d, typeof(S932Parameter), (m, t, C, D) => Lock(m, C, D, false));
        Icon(d, typeof(S933Parameter), (m, t, C, D) => Lock(m, C, D, true));
        // ボタン ベル: 回転体のベル、すそのふち、下の玉と上のつまみ
        Icon(d, typeof(S934Parameter), (m, t, C, D) =>
        {
            m.Ball(0, -235, 0, 26, D);
            m.Lathe(0, 0, C, 0, -215, 85, -205, 145, -150, 160, -60, 185, 60, 220, 165, 230, 185, 0, 185);
            m.Pitch(0, 185, 0, MathF.PI / 2f, () => m.Torus(0, 185, 0, 228, 22, D));
            m.Ball(0, 215, 0, 55, D);
        });
        // ボタン カレンダー: 厚い台紙、赤い上の帯、とじ具、日付のマス
        Icon(d, typeof(S935Parameter), (m, t, C, D) =>
        {
            m.Box(0, 20, 0, 520, 480, 60, C, .12f);
            m.Box(0, -155, 4, 524, 130, 68, D, .15f);
            foreach (int s in Sides) m.Limb(s * 130, -268, 0, s * 130, -182, 0, 17, 17, m.C(0x8A95A0));
            for (int y = 0; y < 3; y++) for (int x = 0; x < 4; x++) m.Box(-165 + x * 110, -20 + y * 95, 32, 62, 58, 10, x == 2 && y == 1 ? D : m.C(0xD8DEE5), .3f);
        });
        // ボタン フォルダー: 奥の板(つまみ付き)、中の紙、手前へ少し開いた板
        Icon(d, typeof(S936Parameter), (m, t, C, D) =>
        {
            Puff(m, -30, 24, C, -295, -195, -95, -195, -45, -130, 295, -130, 295, 215, -295, 215);
            m.Box(0, -40, -5, 500, 300, 8, W, .2f);
            m.Pitch(0, 215, 0, -.3f, () => Puff(m, 20, 24, D, -295, -60, 295, -60, 265, 215, -265, 215));
        });
        // ボタン 書類: 薄い紙、折れた角、文字の線
        Icon(d, typeof(S937Parameter), (m, t, C, D) =>
        {
            m.Slab(0, 16, C, -205, -280, 95, -280, 205, -170, 205, 280, -205, 280);
            m.Tri3D(new Vector3(95, -280, 8), new Vector3(95, -170, 8), new Vector3(195, -175, 40), D);
            for (int i = 0; i < 6; i++) m.Box(i == 5 ? -55 : 0, -85 + i * 62, 10, i == 5 ? 220 : 320, 20, 6, D, .5f);
        });
        // ボタン ごみ箱: すぼまった筒、ふたと取っ手、たての筋
        Icon(d, typeof(S938Parameter), (m, t, C, D) =>
        {
            m.Lathe(0, 0, C, 175, -165, 140, 270, 0, 270);
            m.Cyl(0, -205, 0, 215, 44, C, .3f);
            m.Box(0, -245, 0, 130, 44, 60, C, .4f);
            for (int i = -1; i <= 1; i++)
            {
                float x0 = i * 65, x1 = i * 52;
                m.Limb(x0, -105, MathF.Sqrt(170 * 170 - x0 * x0) + 4, x1, 210, MathF.Sqrt(145 * 145 - x1 * x1) + 4, 8, 8, D);
            }
        });
        // ボタン 保存 / 送信: 太い矢印と受け皿
        Icon(d, typeof(S939Parameter), (m, t, C, D) =>
        {
            m.Box(0, -120, 0, 110, 300, 90, C, .3f);
            Puff(m, 0, 90, C, -185, -5, 0, 195, 185, -5);
            Tray(m, C);
        });
        Icon(d, typeof(S940Parameter), (m, t, C, D) =>
        {
            m.Box(0, 40, 0, 110, 300, 90, C, .3f);
            Puff(m, 0, 90, C, -185, -65, 0, -265, 185, -65);
            Tray(m, C);
        });
    }
}
