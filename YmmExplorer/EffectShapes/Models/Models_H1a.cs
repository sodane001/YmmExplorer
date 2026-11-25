using System.Globalization;
using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 182番(解説の図 20種類)・183番(ゲーム 17種類)
//  QuickShapeParameter の図形。「文字」「数」「値」などの設定は2Dと同じように使う
// =====================================================================
internal static partial class Models3D
{
    static List<float> Nums(QuickShapeParameter p) =>
        [.. p.ItemsForModel.Select(s => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0f)];

    static List<(string n, float[] v)> Rows(QuickShapeParameter p) =>
        [.. Str(p).Split('\n').Select(s => s.Trim()).Where(s => s.Contains(':') || s.Contains('：')).Select(r =>
        {
            int ci = r.IndexOfAny([':', '：']);
            return (r[..ci].Trim(), r[(ci + 1)..].Split(',', '-').Select(s => float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : 0f).ToArray());
        })];

    static List<string> Lines(QuickShapeParameter p) => [.. Str(p).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0)];
    static Vector4 Mix(Sculpt m, System.Windows.Media.Color a, System.Windows.Media.Color b, float f) => Vector4.Lerp(m.C(a), m.C(b), f);
    static float Ease(float x) => 1f - (1f - x) * (1f - x);
    static float In(Sculpt m, float delay, float dur) => Math.Clamp((m.Now - delay) / MathF.Max(.01f, dur), 0f, 1f);
    static Vector4 Pal(Sculpt m, int i) => m.C(new uint[] { 0x4FC3F7, 0xFF8A65, 0x81C784, 0xFFD54F, 0xBA68C8, 0x4DB6AC, 0xF06292 }[i % 7]);

    static void Q(Dictionary<Type, QuickModel> d, Type t, QuickModel f) => d[t] = f;
    static void QS(Dictionary<Type, QuickModel> d, Type t, QuickModel f) { d[t] = f; sceneTypes.Add(t); }

    static partial void RegisterH1a(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        // ---------------------------------------------------------------- 182 解説の図
        Q(d, typeof(LineChartParameter), (p, m, t, v, v2) => // 折れ線グラフ: 奥行きのある面と、管の線・玉
        {
            var vals = Nums(p); if (vals.Count < 2) return;
            float gw = 1200, gh = 560, x0 = -gw / 2, y1 = gh / 2, mx = MathF.Max(1e-3f, vals.Max()) * 1.15f;
            for (int i = 0; i <= 4; i++) m.Box(0, y1 - gh * i / 4f, -40, gw, 2, 2, m.C(0xFFFFFF, .15f), .5f);
            m.Box(0, y1, 0, gw, 6, 120, m.C(0xFFFFFF, .8f), .3f);
            var pts = vals.Select((f, i) => new Vector2(x0 + gw * i / (vals.Count - 1), y1 - gh * f / mx)).ToList();
            int shown = Math.Max(2, (int)MathF.Ceiling(pts.Count * In(m, .2f, 1.6f)));
            var part = pts.Take(shown).ToList();
            var area = new List<Vector2>(part) { new(part[^1].X, y1), new(x0, y1) }; area.Reverse();
            m.Slab(area, 0, 60, m.C(p.Color1, .25f));
            m.Tube([.. part.Select(q => new Vector3(q, 32))], 5, m.C(p.Color1));
            foreach (var q in part) { m.Ball(q.X, q.Y, 32, 11, W); }
        });
        Q(d, typeof(BubbleChartParameter), (p, m, t, v, v2) => // 散布図: 軸と、奥行きにも散らばる半透明の玉
        {
            float gw = 1200, gh = 620, x0 = -gw / 2, y1 = gh / 2;
            m.Limb(x0, y1, 0, x0 + gw, y1, 0, 2, 2, W); m.Limb(x0, y1, 0, x0, y1 - gh, 0, 2, 2, W);
            for (int i = 0; i < p.Count; i++)
            {
                float k = Ease(In(m, .2f + i * .08f, .35f)); if (k <= 0) continue;
                float x = x0 + gw * (.06f + DrawKit.H(p.Seed, i, 1) * .9f), y = y1 - gh * (.08f + DrawKit.H(p.Seed, i, 2) * .85f), r = (14 + DrawKit.H(p.Seed, i, 3) * 60) * k;
                m.Ball(x, y, (DrawKit.H(p.Seed, i, 5) - .5f) * 200, r, Mix(m, p.Color1, p.Color2, DrawKit.H(p.Seed, i, 4)) with { W = .75f * m.Al });
            }
        });
        Q(d, typeof(StackedBarParameter), (p, m, t, v, v2) => // 積み上げ棒グラフ: 奥行きのある柱を積む
        {
            var data = Rows(p); if (data.Count == 0) return;
            float mx = MathF.Max(1e-3f, data.Max(r => r.v.Sum())) * 1.1f, gw = 1200, gh = 560, x0 = -gw / 2, y1 = gh / 2, slot = gw / data.Count, bw = slot * .55f;
            m.Box(0, y1 + 3, 0, gw, 6, bw + 40, m.C(0xFFFFFF, .8f), .3f);
            float g = Ease(In(m, .2f, 1.2f));
            for (int i = 0; i < data.Count; i++)
            {
                float y = y1, x = x0 + slot * (i + .5f);
                for (int k = 0; k < data[i].v.Length; k++) { float hh = gh * data[i].v[k] / mx * g; if (hh < 1) continue; m.Box(x, y - hh / 2, 0, bw, hh - 2, bw, Pal(m, k), .06f); y -= hh; }
                m.Text(data[i].n, 34, x, y1 + 40, 0, 4, W);
            }
        });
        Q(d, typeof(VennParameter), (p, m, t, v, v2) => // ベン図: 前後にずらした半透明の円盤と名前
        {
            var names = p.ItemsForModel; int n = Math.Clamp(names.Count, 2, 3); var cols = new[] { p.Color1, p.Color2, p.Color3 };
            for (int i = 0; i < n; i++)
            {
                float k = Ease(In(m, i * .2f, .4f)), ang = n == 2 ? (i == 0 ? MathF.PI : 0f) : -MathF.PI / 2 + i * MathF.Tau / 3, x = MathF.Cos(ang) * 126, y = MathF.Sin(ang) * 126;
                m.Disc(x, y, (i - 1) * 30, 230 * k, 230 * k, 24, m.C(cols[i], .5f), .4f);
                m.Torus(x, y, (i - 1) * 30, 230 * k, 4, m.C(cols[i]));
                if (i < names.Count) m.Text(names[i], 46, MathF.Cos(ang) * 230, MathF.Sin(ang) * 230, 70, 6, W);
            }
        });
        Q(d, typeof(PyramidChartParameter), (p, m, t, v, v2) => // ピラミッド図: 段ごとの四角すい台を下から積む
        {
            var it = Lines(p); int n = it.Count; if (n == 0) return;
            float pw = 1000, ph = 700, top = -ph / 2, lh = ph / n;
            for (int i = 0; i < n; i++)
            {
                float k = Ease(In(m, (n - 1 - i) * .25f, .4f)); if (k <= 0) continue;
                float y0 = top + i * lh + (1 - k) * 40, y1 = y0 + lh - 6;
                SquareFrustum(m, 0, 0, y0, pw * i / n / 2f, y1, pw * (i + 1) / n / 2f, Mix(m, p.Color1, p.Color2, i / (float)Math.Max(1, n - 1)));
                m.Text(it[i], MathF.Min(46, lh * .45f), 0, (y0 + y1) / 2, pw * (i + 1) / n / 2f * .7f + 4, 6, W);
            }
        });
        Q(d, typeof(MindMapParameter), (p, m, t, v, v2) => // マインドマップ: 前後にのびる枝と、厚い札
        {
            var it = Lines(p); if (it.Count == 0) return; int n = it.Count - 1;
            for (int i = 0; i < n; i++)
            {
                float k = Ease(In(m, .3f + i * .15f, .4f)); if (k <= 0) continue;
                float ang = -MathF.PI / 2 + i * MathF.Tau / n, ex = MathF.Cos(ang) * 520 * k, ey = MathF.Sin(ang) * 300 * k, ez = (i % 2 == 0 ? 60 : -60) * k;
                m.Tube([new(0, 0, 0), new(ex / 2, ey / 2 - 60 * MathF.Cos(ang), ez / 2), new(ex, ey, ez)], 4, Pal(m, i));
                if (k >= 1) { float tw = MathF.Max(120, it[i + 1].Length * 40 + 40); m.Box(ex, ey, ez, tw, 66, 40, Pal(m, i), .5f); m.Text(it[i + 1], 40, ex, ey, ez + 22, 4, W); }
            }
            float rw = MathF.Max(200, it[0].Length * 50 + 60);
            m.Ball(0, 0, 0, rw / 2, 60, 50, m.C(p.Color1)); m.Text(it[0], 50, 0, 0, 52, 6, W);
        });
        Q(d, typeof(OrgChartParameter), (p, m, t, v, v2) => // 組織図: 厚い箱と、つなぐ管
        {
            var lines = Lines(p); if (lines.Count == 0) return;
            var kids = new List<(string n, List<string> sub)>();
            foreach (var s in lines.Skip(1)) { if (s.StartsWith('-') && kids.Count > 0) kids[^1].sub.Add(s[1..].Trim()); else kids.Add((s, [])); }
            float bw = 230, bh = 80, rg = 160, span = Math.Max(1, kids.Count) * (bw + 50);
            m.Box(0, -rg, 0, bw, bh, 50, m.C(p.Color1), .3f); m.Text(lines[0], 36, 0, -rg, 27, 4, W);
            for (int i = 0; i < kids.Count; i++)
            {
                if (In(m, .3f + i * .15f, .35f) <= 0) continue;
                float kx = -span / 2 + (bw + 50) * (i + .5f);
                m.Tube([new(0, -rg + bh / 2, 0), new(0, -rg / 2, 0), new(kx, -rg / 2, 0), new(kx, -bh / 2, 0)], 2.5f, W);
                m.Box(kx, 0, 0, bw, bh, 50, m.C(p.Color2), .3f); m.Text(kids[i].n, 34, kx, 0, 27, 4, W);
                for (int j = 0; j < kids[i].sub.Count; j++) { float sy = rg * (j + 1) * .7f + 40; m.Limb(kx, bh / 2, 0, kx, sy - 30, 0, 2, 2, W); m.Box(kx, sy, 0, bw * .85f, 60, 36, Vector4.Lerp(m.C(p.Color2), W, .3f), .3f); m.Text(kids[i].sub[j], 28, kx, sy, 20, 4, W); }
            }
        });
        Q(d, typeof(Matrix4Parameter), (p, m, t, v, v2) => // 4象限: 高さの違う4つのブロックと、管の十字の軸
        {
            var it = p.ItemsForModel; string L(int i) => i < it.Count ? it[i] : "";
            uint[] cols = [0x43A047, 0xFB8C00, 0x757575, 0x1E88E5]; Vector2[] off = [new(1, -1), new(-1, -1), new(-1, 1), new(1, 1)];
            for (int i = 0; i < 4; i++)
            {
                float k = In(m, i * .12f, .3f), x = off[i].X * 165, y = off[i].Y * 165, hz = 30 + (3 - i) * 20;
                m.Box(x, y, hz / 2, 320 * k, 320 * k, hz, m.C(cols[i], .85f), .1f);
                m.Text(L(i), 40, x, y, hz + 2, 4, W);
            }
            m.Limb(-363, 0, 0, 380, 0, 0, 3, 3, W); m.Limb(0, 363, 0, 0, -380, 0, 3, 3, W);
            m.Limb(380, 0, 0, 400, 0, 0, 14, 1, W); m.Limb(0, -380, 0, 0, -400, 0, 14, 1, W);
            m.Text(L(4), 32, 380, 36, 4, 4, W); m.Text(L(5), 32, 0, -410, 4, 4, W);
        });
        Q(d, typeof(CycleDiagramParameter), (p, m, t, v, v2) => // サイクル図: 輪の上の玉と、矢印の管
        {
            var it = Lines(p); int n = it.Count; if (n == 0) return; float R = 300;
            for (int i = 0; i < n; i++)
            {
                float a0 = -MathF.PI / 2 + i * MathF.Tau / n + .38f, a1 = -MathF.PI / 2 + (i + 1) * MathF.Tau / n - .38f, k = In(m, .4f + i * .2f, .4f); if (k <= 0) continue;
                float e = a0 + (a1 - a0) * k;
                m.Tube(ArcPts3(0, 0, R, a0, e, 0, 16), 3.5f, m.C(0xFFFFFF, .85f));
                if (k >= 1) m.Limb(MathF.Cos(a1 - .08f) * R, MathF.Sin(a1 - .08f) * R, 0, MathF.Cos(a1) * R, MathF.Sin(a1) * R, 0, 14, 1, W);
            }
            float hl = (t * .4f % 1f + 1f) % 1f * n;
            for (int i = 0; i < n; i++)
            {
                float k = Ease(In(m, i * .2f, .35f)), ang = -MathF.PI / 2 + i * MathF.Tau / n, x = MathF.Cos(ang) * R, y = MathF.Sin(ang) * R;
                bool on = (int)hl == i;
                m.Ball(x, y, on ? 30 : 0, 95 * k, 95 * k, 60 * k, Pal(m, i));
                m.Text(it[i], 34, x, y, (on ? 30 : 0) + 62, 4, W, 170);
            }
        });
        QS(d, typeof(BeforeAfterParameter), (p, m, t, v, v2) => // ビフォーアフター: 画面を仕切る立った板と、つまみ
        {
            float x = -m.W / 2 + m.W * v + (1 - In(m, 0, 1.2f)) * MathF.Sin(m.Now * 9) * m.W * .05f;
            if (p.Background) m.Box((-m.W / 2 + x) / 2, 0, -10, x + m.W / 2, m.H, 4, m.C(0x000000, .25f), .02f);
            m.Box(x, 0, 0, 8, m.H, 40, W, .3f);
            m.Disc(x, 0, 0, 40, 40, 40, W, .4f);
            foreach (int s in Sides) m.Slab(22, 8, m.C(0x33333F), x + s * 6, -13, x + s * 24, 0, x + s * 6, 13);
            var it = p.ItemsForModel;
            m.Text(it.Count > 0 ? it[0] : "BEFORE", 44, -m.W / 2 + 160, -m.H / 2 + 60, 4, 6, W);
            m.Text(it.Count > 1 ? it[1] : "AFTER", 44, m.W / 2 - 160, -m.H / 2 + 60, 4, 6, W);
        });
        Q(d, typeof(FormulaBoxParameter), (p, m, t, v, v2) => // 黒板: 木のわく、厚い黒板、チョークの字、下線
        {
            string s = Str(p); if (s.Length == 0) return;
            float k = Ease(In(m, 0, .35f)), bw = MathF.Min(1400, s.Length * 70 + 160), bh = 230;
            m.Box(0, 0, -10, (bw + 30) * k, (bh + 30) * k, 40, m.C(0x8A5A34), .1f);
            m.Box(0, 0, 0, bw * k, bh * k, 40, m.C(p.Color1), .05f);
            if (k < .9f) return;
            m.Text(s, 90, 0, 0, 22, 4, m.C(0xF2F2E6, .92f), 1400);
            float q = In(m, 1.2f, .5f); if (q > 0) m.Limb(-bw * .4f, 70, 22, -bw * .4f + bw * .8f * q, 70, 22, 4, 4, m.C(p.Color2));
            m.Box(0, bh / 2 + 18, 18, bw * .9f, 10, 30, m.C(0x6B4428), .3f);
        });
        Q(d, typeof(SizeCompareParameter), (p, m, t, v, v2) => // 大きさ比べ: 床の上に、大きさの違う人形の柱
        {
            var data = Rows(p).Select(r => (r.n, v: MathF.Max(.01f, r.v.Length > 0 ? r.v[0] : 0))).ToList(); if (data.Count == 0) return;
            float gw = 1400, gh = 650, x0 = -gw / 2, y1 = gh / 2, mx = data.Max(dd => dd.v), slot = gw / data.Count;
            m.Box(0, y1 + 3, 0, gw + 60, 6, 300, m.C(0xFFFFFF, .9f), .3f);
            for (int i = 0; i < data.Count; i++)
            {
                float k = Ease(In(m, .2f + i * .2f, .5f)), hh = gh * data[i].v / mx * k, x = x0 + slot * (i + .5f), r = MathF.Min(slot * .25f, hh * .18f);
                var col = i == data.Count - 1 ? m.C(p.Color2) : m.C(p.Color1);
                if (hh < 4) continue;
                m.Ball(x, y1 - hh + r, 0, r, col);
                m.Limb(x, y1 - hh + r * 2.3f, 0, x, y1 - r * .8f, 0, r * 1.1f, r * .9f, col);
                m.Text(data[i].n, 34, x, y1 + 40, 4, 4, W);
            }
        });
        Q(d, typeof(PictogramParameter), (p, m, t, v, v2) => // ピクトグラム: 並んだ小さな立体の人形(割合ぶんだけ色付き)
        {
            int n = Math.Clamp(p.Count, 1, 400), cols = (int)MathF.Ceiling(MathF.Sqrt(n * 1.8f)), rn = (int)MathF.Ceiling(n / (float)cols);
            float cell = MathF.Min(1300f / cols, 650f / rn), x0 = -cols * cell / 2, y0 = -rn * cell / 2;
            int on = (int)MathF.Round(n * v * Ease(In(m, .3f, 1.5f)));
            int keep = m.Seg; m.Seg = 10;
            for (int i = 0; i < n; i++)
            {
                float x = x0 + (i % cols + .5f) * cell, y = y0 + (i / cols + .5f) * cell; var col = i < on ? m.C(p.Color1) : m.C(p.Color2);
                m.Ball(x, y - cell * .22f, 0, cell * .14f, col);
                m.Limb(x, y, 0, x, y + cell * .3f, 0, cell * .2f, cell * .24f, col);
            }
            m.Seg = keep;
            m.Text($"{on} / {n}", 60, 0, y0 + rn * cell + 60, 4, 8, W);
        });
        Q(d, typeof(BalanceScaleParameter), (p, m, t, v, v2) => // 天秤: 台・柱・傾くさお、ひもと皿、名前
        {
            float tilt = (v - .5f) * .6f, st = In(m, 0, 1.2f); tilt *= 1 + MathF.Sin(st * 14) * (1 - st) * .8f;
            var C = m.C(p.Color1);
            SquareFrustum(m, 0, 0, 230, 90, 260, 140, C);
            m.Cyl(0, 5, 0, 12, 450, C, .2f);
            var it = p.ItemsForModel;
            m.Turn(0, -220, tilt, () =>
            {
                m.Limb(-380, -220, 0, 380, -220, 0, 7, 7, C);
                foreach (int s in Sides)
                {
                    float x = s * 380;
                    foreach (int r in Sides) m.Limb(x, -220, 0, x + r * 110, -20, 0, 2, 2, C);
                    m.Turn(x, -20, -tilt, () =>
                    {
                        var prof = new List<Vector2>(); for (int i = 0; i <= 8; i++) { float a0 = i / 8f * 1.5708f; prof.Add(new(MathF.Sin(a0) * 120, -20 + MathF.Cos(a0) * 50)); }
                        m.Lathe(x, 0, C, prof);
                        int idx = s < 0 ? 0 : 1; if (idx < it.Count) m.Text(it[idx], 40, x, 60, 4, 4, W);
                    });
                }
            });
            m.Ball(0, -220, 10, 22, m.C(p.Color3));
        });
        Q(d, typeof(GanttParameter), (p, m, t, v, v2) => // ガント: 行の名前と、奥行きのある期間の棒
        {
            var data = Rows(p); if (data.Count == 0) return;
            float gw = 1150, rh = 80, lw0 = 220, x0 = -gw / 2 + lw0 / 2, y0 = -data.Count * rh / 2, tw = gw - lw0;
            for (int i = 0; i < data.Count; i++)
            {
                float k = Ease(In(m, .2f + i * .15f, .5f)), s0 = data[i].v.Length > 0 ? data[i].v[0] / 100f : 0, s1 = data[i].v.Length > 1 ? data[i].v[1] / 100f : s0, y = y0 + rh * (i + .5f);
                m.Text(data[i].n, 34, -gw / 2 + 100, y, 4, 4, W);
                m.Limb(x0 + lw0 / 2, y, -10, x0 + lw0 / 2 + tw, y, -10, 1, 1, m.C(0xFFFFFF, .2f));
                float bx0 = x0 + lw0 / 2 + tw * s0, bx1 = bx0 + tw * (s1 - s0) * k;
                if (bx1 - bx0 > 2) m.Box((bx0 + bx1) / 2, y, 0, bx1 - bx0, rh * .6f, 40, Pal(m, i), .4f);
            }
        });
        Q(d, typeof(ThermometerParameter), (p, m, t, v, v2) => // 温度計: 透けたガラス管と玉、上がる液(メーター型は弧のメーター)
        {
            float lv = v * Ease(In(m, .2f, 1.2f));
            if (p.Variant >= 2)
            {
                float R = 230;
                m.Tube(ArcPts3(0, 0, R, MathF.PI * .75f, MathF.PI * 2.25f, -6, 40), 15, m.C(0xFFFFFF, .25f));
                if (lv > .01f) m.Tube(ArcPts3(0, 0, R, MathF.PI * .75f, MathF.PI * .75f + MathF.PI * 1.5f * lv, 6, 40), 15, m.C(p.Color1));
                float na = MathF.PI * .75f + MathF.PI * 1.5f * lv;
                m.Limb(0, 0, 20, MathF.Cos(na) * R * .8f, MathF.Sin(na) * R * .8f, 20, 5, 2, W); m.Ball(0, 0, 20, 18, W);
                m.Text(Str(p), 70, 0, R * .55f, 10, 8, W);
                return;
            }
            float th = 600;
            m.Box(0, 0, 0, 80, th + 24, 80, m.C(0xFFFFFF, .3f), .5f);
            m.Ball(0, th / 2, 0, 70, m.C(0xFFFFFF, .3f));
            m.Ball(0, th / 2, 0, 52, m.C(p.Color1));
            float hh = th * lv; m.Cyl(0, th / 2 - hh / 2, 0, 20, hh, m.C(p.Color1), .2f);
            for (int i = 0; i <= 10; i++) m.Box(52, -th / 2 + th * i / 10f, 0, i % 5 == 0 ? 28 : 16, 3, 3, W, .5f);
            m.Text(Str(p), 60, 160, th / 2 - hh, 4, 6, W);
        });
        Q(d, typeof(MagnifierFrameParameter), (p, m, t, v, v2) => // 虫めがね: 太い輪、透けたレンズ、木の柄、つや
        {
            float R = (120 + 260 * v) * Ease(In(m, 0, .4f));
            float ca = MathF.Cos(MathF.PI / 4), sa = MathF.Sin(MathF.PI / 4);
            m.Limb(ca * R * 1.05f, sa * R * 1.05f, 0, ca * R * 1.9f, sa * R * 1.9f, 0, 23, 23, m.C(p.Color1));
            m.Limb(ca * R * 1.18f, sa * R * 1.18f, 0, ca * R * 1.9f, sa * R * 1.9f, 0, 25, 25, m.C(0x8D6E63));
            m.Torus(0, 0, 0, R, 14, m.C(p.Color1));
            m.Disc(0, 0, 0, R - 10, R - 10, 14, m.C(0xBFE6F7, .25f), .6f);
            m.Tube(ArcPts3(0, 0, R * .78f, MathF.PI * 1.1f, MathF.PI * 1.45f, 12, 10), 5, m.C(0xFFFFFF, .5f));
        });
        Q(d, typeof(BulletListParameter), (p, m, t, v, v2) => // 箇条書き: 番号の玉(またはチェック)と、手前の文字
        {
            var it = Lines(p); if (it.Count == 0) return; float rh = 110, y0 = -it.Count * rh / 2, x0 = -520;
            for (int i = 0; i < it.Count; i++)
            {
                float k = Ease(In(m, .2f + i * .4f, .4f)); if (k <= 0) continue;
                float y = y0 + i * rh + rh / 2, mx = x0 + 40 + (1 - k) * 80;
                m.Ball(mx, y, 0, 36, 36, 26, m.C(p.Color1));
                if (p.Variant >= 2) m.Tube([new(mx - 16, y, 28), new(mx - 4, y + 14, 28), new(mx + 18, y - 14, 28)], 3.5f, W);
                else m.Text((i + 1).ToString(), 40, mx, y, 26, 4, W);
                m.Text(it[i], 50, mx + 64 + it[i].Length * 25, y, 6, 6, W, 1000);
            }
        });
        Q(d, typeof(DotWorldMapParameter), (p, m, t, v, v2) => // ドットの世界地図: 地球を思わせる、ゆるく曲がった面に並ぶ玉
        {
            int cols = 58, rows = 22; float cw = 1400f / cols;
            int keep = m.Seg; m.Seg = 6;
            for (int y = 0; y < rows; y++) for (int x = 0; x < cols; x++)
                {
                    float fx = (x - cols / 2f) / cols, fy = (y - rows / 2f) / rows, land = MathF.Sin(fx * 9 + 1) * MathF.Cos(fy * 7) + MathF.Sin(fx * 23 + fy * 5) * .4f;
                    if (land < .15f) continue;
                    float px = (x - cols / 2f) * cw, py = (y - rows / 2f) * cw, pz = -MathF.Abs(fx) * 200;
                    bool hi = DrawKit.H(p.Seed, x * 31 + y) > .97f;
                    m.Ball(px, py, pz, hi ? 9 : 6, hi ? m.C(p.Color2) : m.C(p.Color1, .85f));
                }
            m.Seg = keep;
        });
        Q(d, typeof(CompassParameter), (p, m, t, v, v2) => // 方位磁針: 木のふち、文字盤、目盛り、方角の字、回る針
        {
            float R = 280;
            m.Disc(0, 0, -10, R * 1.08f, R * 1.08f, 50, m.C(0x8D6E63), .3f);
            m.Disc(0, 0, 0, R, R, 40, m.C(p.Color2), .1f);
            for (int i = 0; i < 36; i++) { float ag = i * MathF.Tau / 36, r0 = R * (i % 9 == 0 ? .78f : .88f); m.Limb(MathF.Cos(ag) * r0, MathF.Sin(ag) * r0, 22, MathF.Cos(ag) * R * .96f, MathF.Sin(ag) * R * .96f, 22, 1.5f, 1.5f, m.C(0x333338)); }
            string[] nm = ["北", "東", "南", "西"];
            for (int i = 0; i < 4; i++) { float ag = -MathF.PI / 2 + i * MathF.PI / 2; m.Text(nm[i], 52, MathF.Cos(ag) * R * .62f, MathF.Sin(ag) * R * .62f, 22, 4, i == 0 ? m.C(p.Color1) : m.C(0x333338)); }
            float st = In(m, 0, 2), a0 = v * MathF.Tau + MathF.Sin(m.Now * 8) * .6f * (1 - st) + MathF.Sin(m.Now * 1.3f) * .03f;
            m.Move(0, 0, 32, () => m.Turn(0, 0, a0, () =>
            {
                Gem(m, Pts(0, -R * .82f, 24, 0, 0, 0, -24, 0), 0, -R * .4f, 0f, 3, 14, m.C(p.Color1), m.C(p.Color1));
                Gem(m, Pts(0, R * .82f, 24, 0, 0, 0, -24, 0), 0, R * .4f, 0f, 3, 14, m.C(0x9AA3AD), m.C(0x9AA3AD));
            }));
            m.Ball(0, 0, 40, 16, m.C(0xE8C15A));
            m.Ball(0, 0, 30, R, R, 40, m.C(0xFFFFFF, .08f));
        });

        // ---------------------------------------------------------------- 183 ゲーム
        Q(d, typeof(BossHpBarParameter), (p, m, t, v, v2) => // ボスのHP: 厚い枠と、段ごとに色の違う棒
        {
            int n = Math.Max(1, p.Count); float hp = v * n; int stage = Math.Min(n - 1, (int)MathF.Floor(hp - 1e-4f)); float cur = hp - stage;
            float bw = 1300, bh = 44, x0 = -bw / 2, k = Ease(In(m, 0, .4f));
            m.Box(0, 0, -4, (bw + 16) * k, bh + 16, 40, m.C(0x000000, .7f), .2f);
            var under = stage > 0 ? Mix(m, p.Color1, p.Color2, (stage - 1) / (float)Math.Max(1, n - 1)) : m.C(0x303030);
            m.Box(x0 + bw * k / 2, 0, 10, bw * k, bh, 30, under, .2f);
            var top = Mix(m, p.Color1, p.Color2, stage / (float)Math.Max(1, n - 1));
            m.Box(x0 + bw * cur * k / 2, 0, 16, bw * cur * k, bh, 34, top, .2f);
            m.Text(Str(p), 50, x0 + 120, -70, 4, 6, W);
            m.Text($"×{stage + 1}", 44, x0 + bw - 40, -70, 4, 6, W);
        });
        Q(d, typeof(ExpBarParameter), (p, m, t, v, v2) => // 経験値バー: 丸い溝と進む棒、レベルの文字
        {
            float total = In(m, .2f, 1.2f * Math.Max(1, p.Count)) * Math.Max(1, p.Count); int ups = Math.Min(p.Count, (int)MathF.Floor(total)); float fill = ups >= p.Count ? 1 : total - ups;
            float bw = 900, x0 = -bw / 2;
            m.Box(0, 0, 0, bw + 12, 42, 40, m.C(0x000000, .7f), .5f);
            if (fill > .01f) m.Box(x0 + bw * fill / 2, 0, 12, bw * fill, 30, 36, m.C(p.Color1), .5f);
            m.Text("EXP", 30, x0 - 60, 0, 4, 4, W);
            m.Text(Str(p) + " " + (1 + ups), 44, x0 + bw + 100, 0, 4, 6, W);
            float since = total - ups; if (ups > 0 && since < .8f) m.Text("LEVEL UP!", 80 * Ease(MathF.Min(1, since / .3f)), 0, -90, 30, 10, m.C(p.Color2));
        });
        Q(d, typeof(CommandInputParameter), (p, m, t, v, v2) => // コマンド入力: 丸いボタン(文字)や角の丸いキーが順に出る
        {
            var it = p.ItemsForModel; if (it.Count == 0) return; float s = 110, gap = 30, x0 = -(it.Count * s + (it.Count - 1) * gap) / 2 + s / 2;
            for (int i = 0; i < it.Count; i++)
            {
                float k = Ease(In(m, .1f + i * .12f, .25f)); if (k <= 0) continue; float x = x0 + i * (s + gap);
                bool btn = it[i].Length == 1 && char.IsLetter(it[i][0]);
                if (btn) { m.Disc(x, 0, 0, s / 2 * k, s / 2 * k, 40, m.C(p.Color2), .5f); m.Torus(x, 0, 20, s / 2 * k, 3, W); }
                else { m.Box(x, 0, 0, s * k, s * k, 40, m.C(0x1A1A24, .9f), .3f); }
                m.Text(it[i], 64 * k, x, 0, 22, 6, W);
            }
        });
        Q(d, typeof(KoTextParameter), (p, m, t, v, v2) => // K.O.: 放射する光の板と、ぶ厚い文字
        {
            float k0 = In(m, 0, .25f), k = k0 < 1 ? 3 - 2 * Ease(k0) : 1, shake = MathF.Max(0, 1 - m.Now * 2.5f);
            float sx = MathF.Sin(m.Now * 70) * 20 * shake, sy = MathF.Cos(m.Now * 53) * 20 * shake;
            for (int i = 0; i < 16; i++) { float ag = i * MathF.Tau / 16 + .1f, len = (500 + 300 * DrawKit.H(p.Seed, i)) * Ease(Math.Clamp(m.Now * 3, 0, 1)); m.Tri3D(new(sx + MathF.Cos(ag - .05f) * 100, sy + MathF.Sin(ag - .05f) * 100, -20), new(sx + MathF.Cos(ag) * len, sy + MathF.Sin(ag) * len, -60), new(sx + MathF.Cos(ag + .05f) * 100, sy + MathF.Sin(ag + .05f) * 100, -20), m.C(p.Color3, .6f)); }
            m.Text(Str(p), 260 * k, sx, sy, 10, 60, m.C(p.Color1), 0, m.C(p.Color3), 12);
        });
        Q(d, typeof(StageMapParameter), (p, m, t, v, v2) => // ステージマップ: 草の台、道、ステージの丸い台(クリア・次・まだ)
        {
            int n = Math.Max(2, p.Count);
            if (p.Background) m.Box(0, 0, -30, 1500, 600, 40, m.C(0x7CB342), .1f);
            var pts = Enumerable.Range(0, n).Select(i => new Vector2(-650 + 1300f * i / (n - 1), MathF.Sin(i * 1.7f + .5f) * 180)).ToList();
            var path = new List<Vector3>();
            for (int i = 0; i < n - 1; i++) { var mid = (pts[i] + pts[i + 1]) / 2 + new Vector2(0, i % 2 == 0 ? -80 : 80); for (int j = 0; j <= 10; j++) { float f = j / 10f; var q = (1 - f) * (1 - f) * pts[i] + 2 * (1 - f) * f * mid + f * f * pts[i + 1]; path.Add(new(q, -6)); } }
            m.Tube(path, 15, m.C(0xF5E6C8));
            int done = (int)MathF.Round(v * (n - 1));
            for (int i = 0; i < n; i++)
            {
                bool cl = i < done, nx = i == done; var col = cl ? m.C(p.Color1) : nx ? m.C(p.Color3) : m.C(0x9E9E9E);
                m.Disc(pts[i].X, pts[i].Y, 10, 56, 56, 40 + (nx ? 20 * MathF.Abs(MathF.Sin(m.Now * 4)) : 0), col, .4f);
                m.Text((i + 1).ToString(), 44, pts[i].X, pts[i].Y, 34, 4, W);
                if (cl) Gem(m, Pts(StarPts(pts[i].X, pts[i].Y - 80, 22, 5, .45f, 0)), pts[i].X, pts[i].Y - 80, 0f, 3, 10, m.C(0xFFD54A), m.C(0xFFD54A));
            }
        });
        Q(d, typeof(CardFlipParameter), (p, m, t, v, v2) => // カードめくり: 本当に裏返る厚いカード
        {
            int n = Math.Clamp(p.Count, 1, 7); var it = p.ItemsForModel; float cw = 300, ch = 440, gap = 50, x0 = -(n * cw + (n - 1) * gap) / 2 + cw / 2;
            for (int i = 0; i < n; i++)
            {
                float k = In(m, .4f + i * .3f, .5f), x = x0 + i * (cw + gap), y = -MathF.Sin(k * MathF.PI) * 60;
                m.Yaw(x, y, 0, MathF.PI * (1 - k), () =>
                {
                    m.Box(x, y, 0, cw, ch, 12, W, .08f);
                    m.Box(x, y, -7, cw - 10, ch - 10, 3, m.C(p.Color1), .08f);
                    Gem(m, Pts(StarPts(x, y, 70, 4, .4f, 0)), x, y, 0f, 2, 8, m.C(0xFFFFFF, .5f), m.C(0xFFFFFF, .5f));
                    if (i < it.Count) m.Text(it[i], 60, x, y, 8, 4, m.C(p.Color2), cw - 40);
                });
            }
        });
        Q(d, typeof(ItemDropParameter), (p, m, t, v, v2) => // アイテムドロップ: 光の柱、はねる宝石とコイン
        {
            float pillar = MathF.Max(0, 1 - m.Now * .8f);
            if (pillar > 0) m.Cyl(0, -300, 0, 60 * pillar, 600, m.C(p.Color3, .4f * pillar), .1f);
            for (int i = 0; i < p.Count; i++)
            {
                float tt = m.Now - i * .05f; if (tt <= 0) continue;
                float vx = (DrawKit.H(p.Seed, i) - .5f) * 900, vy = -(700 + 500 * DrawKit.H(p.Seed, i, 1)), g = 2600, tl = -2 * vy / g, x = vx * MathF.Min(tt, tl * 1.4f), y = tt < tl ? vy * tt + .5f * g * tt * tt : 0;
                float z = (DrawKit.H(p.Seed, i, 2) - .5f) * 300 * MathF.Min(1, tt / tl);
                bool gem = i % 3 == 0;
                if (gem) m.Move(0, 0, z, () => m.Yaw(x, y, 0, m.Now * 6 + i, () => Gem(m, Pts(x, y - 30, x + 22, y, x, y + 30, x - 22, y), x, y, .5f, 6, 16, m.C(p.Color2), Vector4.Lerp(m.C(p.Color2), W, .4f))));
                else m.Move(0, 0, z, () => m.Yaw(x, y, 0, m.Now * 6 + i, () => m.Disc(x, y, 0, 22, 22, 8, m.C(p.Color1), .3f)));
            }
        });
        Q(d, typeof(MiniMapParameter), (p, m, t, v, v2) => // ミニマップ: 厚い盤、格子、回る探知の光、点滅する敵
        {
            float s = 420; bool round = p.Variant >= 2;
            if (round) m.Disc(0, 0, 0, s / 2, s / 2, 30, m.C(0x0D1F1A, .9f), .3f); else m.Box(0, 0, 0, s, s, 30, m.C(0x0D1F1A, .9f), .1f);
            for (int i = 1; i < 4; i++) { float g = -s / 2 + s * i / 4; m.Box(g, 0, 16, 2, s * .95f, 2, m.C(p.Color1, .2f), .5f); m.Box(0, g, 16, s * .95f, 2, 2, m.C(p.Color1, .2f), .5f); }
            float sw = m.Now * 2;
            m.Turn(0, 0, sw, () => m.Slab(18, 4, m.C(p.Color1, .3f), PiePts(0, 0, s / 2 * .95f, -.8f, 0, 10)));
            for (int i = 0; i < p.Count; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * s * .8f, y = (DrawKit.H(p.Seed, i, 1) - .5f) * s * .8f; m.Ball(x, y, 24, 10 + 4 * MathF.Abs(MathF.Sin(m.Now * 5 + i)), m.C(p.Color2)); }
            Gem(m, Pts(0, -16, 12, 12, -12, 12), 0, 0, 0f, 3, 10, W, W);
        });
        Q(d, typeof(TurnOrderParameter), (p, m, t, v, v2) => // 行動順: 並んだ厚い札(先頭は大きく手前)
        {
            var it = p.ItemsForModel; if (it.Count == 0) return; float s = 120, gap = 20;
            float shift = ((t * .5f) % it.Count + it.Count) % it.Count; int head = (int)shift; float fr = Ease(Math.Clamp((shift - head) * 4, 0, 1));
            int show = Math.Min(6, it.Count); float x0 = -(show * (s + gap)) / 2 + s / 2;
            m.Text("NEXT", 34, x0 - s, 0, 4, 4, W);
            for (int j = 0; j < show; j++)
            {
                int idx = (head + j) % it.Count; float x = j == 0 ? x0 : x0 + (j - fr) * (s + gap), sc = j == 0 ? 1.2f : 1, z = j == 0 ? 40 : 0;
                m.Box(x, 0, z, s * sc, s * sc, 40, Pal(m, idx), .3f);
                m.Ball(x, -10, z + 20, s * .22f * sc, s * .22f * sc, 10, m.C(0xFFFFFF, .9f));
                m.Text(it[idx], 26, x, s * .3f, z + 22, 4, W, s);
            }
        });
        QS(d, typeof(LoadingScreenParameter), (p, m, t, v, v2) => // ローディング: 暗い画面、回る玉の輪、文字、進む棒
        {
            if (p.Background) m.Box(0, 0, -20, m.W, m.H, 10, m.C(0x05050A), .01f);
            float sx = m.W / 2 - 220, sy = m.H / 2 - 140;
            for (int i = 0; i < 8; i++) { float k = ((i / 8f - m.Now * 1.5f) % 1 + 1) % 1, ag = i * MathF.PI / 4; m.Ball(sx + MathF.Cos(ag) * 40, sy + MathF.Sin(ag) * 40, 0, 9, m.C(p.Color1, .2f + .8f * k)); }
            m.Text("NOW LOADING" + new string('.', 1 + (int)(m.Now * 3) % 3), 44, m.W / 2 - 500, m.H / 2 - 170, 4, 6, m.C(p.Color1));
            m.Text(Str(p), 40, 0, 0, 4, 6, m.C(0xCCCCD9), m.W * .8f);
            float bar = (m.Now * .3f) % 1; m.Box(-m.W * .4f + m.W * .8f * bar / 2, m.H / 2 - 56, 0, m.W * .8f * bar, 8, 8, m.C(p.Color1, .6f), .4f);
        });
        Q(d, typeof(GameOverParameter), (p, m, t, v, v2) => // ゲームオーバー: 落ちてくるぶ厚い文字と、コンティニューの選択
        {
            float k = In(m, .5f, 1.2f), dy = (1 - Ease(k)) * -200;
            if (k > 0) m.Text(Str(p), 180, 0, -120 + dy, 20, 50, m.C(p.Color1, k), 0, m.C(0x000000, k), 6);
            float q = In(m, 1.8f, .4f);
            if (q > 0) { m.Text("CONTINUE?", 70, 0, 80, 10, 10, m.C(0xFFFFFF, q)); bool blink = (m.Now * 2) % 1 < .5f; m.Text("YES     NO", 56, 0, 180, 10, 8, m.C(0xFFFFFF, q)); if (blink) Gem(m, Pts(-180, 165, -150, 180, -180, 195), -170, 180, 0f, 3, 8, W, W); }
        });
        Q(d, typeof(ResultStarsParameter), (p, m, t, v, v2) => // リザルト: CLEAR の文字と、順に光る3つの立体の星
        {
            float k = Ease(In(m, 0, .4f));
            m.Text("CLEAR!", 150 * k, 0, -260, 20, 40, m.C(p.Color1), 0, m.C(0x804D00), 7);
            int got = Math.Clamp((int)MathF.Round(v * 3), 0, 3);
            for (int i = 0; i < 3; i++)
            {
                float q = Ease(In(m, .5f + i * .35f, .35f)), x = (i - 1) * 220, y = i == 1 ? -40 : 0, r = (i == 1 ? 110 : 90) * (i < got ? q : 1);
                var col = i < got ? m.C(p.Color1) : m.C(p.Color2);
                m.Yaw(x, y, 0, i < got ? (1 - q) * MathF.Tau : 0, () => Gem(m, Pts(StarPts(x, y, r, 5, .45f, 0)), x, y, 0f, 10, 30, col, col));
            }
            m.Text(Str(p), 60, 0, 200, 10, 8, W);
        });
        Q(d, typeof(DiceParameter), (p, m, t, v, v2) => // サイコロ: 転がりながら止まる立方体(上の目が値)
        {
            int n = Math.Clamp(p.Count, 1, 5); float roll = In(m, 0, 1.2f);
            for (int dd = 0; dd < n; dd++)
            {
                float x = (dd - (n - 1) / 2f) * 280, spin = (1 - Ease(roll)) * 12;
                int face = Math.Clamp((int)MathF.Ceiling(v * 6), 1, 6);
                m.Pitch(x, 0, 0, spin + (dd * .3f), () => m.Yaw(x, 0, 0, spin * .7f + .4f, () =>
                {
                    m.Box(x, 0, 0, 200, 200, 200, m.C(p.Color1), .2f);
                    Pips(m, x, 0, 101, face, m.C(p.Color2));
                }));
            }
        });
        Q(d, typeof(DealCardsParameter), (p, m, t, v, v2) => // トランプを配る: 山札から、回りながら飛んで並ぶカード
        {
            int n = Math.Clamp(p.Count, 1, 13); var it = p.ItemsForModel; float cw = 170, ch = 250;
            for (int i = 0; i < 3; i++) m.Box(i * 3, -300 - i * 3, -i * 6, cw, ch, 6, m.C(p.Color1), .1f);
            for (int i = 0; i < n; i++)
            {
                float k = Ease(In(m, .2f + i * .15f, .4f)); if (k <= 0) continue;
                float tx = (i - (n - 1) / 2f) * (cw + 20), x = tx * k, y = -300 + 450 * k, flip = MathF.PI * (1 - k);
                m.Yaw(x, y, 0, flip, () =>
                {
                    m.Box(x, y, 0, cw, ch, 6, W, .1f);
                    m.Box(x, y, -4, cw - 8, ch - 8, 2, m.C(p.Color1), .1f);
                    m.Text(i < it.Count ? it[i] : "A", 60, x, y, 5, 3, m.C(0xC62828));
                });
            }
        });
        Q(d, typeof(MashMeterParameter), (p, m, t, v, v2) => // 連打メーター: 立った筒の中を上下する液、押すボタン
        {
            float lv = Math.Clamp(v * Ease(In(m, 0, 2.5f)) + MathF.Sin(t * 30) * .02f, 0, 1), bh = 600, x = -200;
            m.Box(x, 0, 0, 136, bh + 16, 136, m.C(0x000000, .5f), .3f);
            float hh = bh * lv; m.Box(x, bh / 2 - hh / 2, 0, 110, hh, 110, Vector4.Lerp(m.C(p.Color1), m.C(p.Color2), lv), .2f);
            bool hit = (m.Now * 12) % 1 < .5f;
            m.Disc(200, 100, hit ? 10 : 30, 110, 110, 60, m.C(p.Color1), .4f);
            m.Text(Str(p), 60, 200, 100, (hit ? 10 : 30) + 32, 6, W);
        });
        QS(d, typeof(SpecialCutinParameter), (p, m, t, v, v2) => // カットイン: 斜めの厚い帯が飛びこみ、流れる線と大きな文字
        {
            float k = Ease(In(m, 0, .3f)), bh = 280;
            m.Turn(0, 0, -.18f, () =>
            {
                m.Box(-m.W * (1 - k) / 2, 0, 0, m.W * 1.4f, bh, 60, m.C(p.Color1), .02f);
                m.Box(-m.W * (1 - k) / 2, -bh / 2 - 10, 10, m.W * 1.4f, 20, 70, m.C(p.Color2), .2f);
                m.Box(-m.W * (1 - k) / 2, bh / 2 + 10, 10, m.W * 1.4f, 20, 70, m.C(p.Color2), .2f);
                for (int i = 0; i < 10; i++) { float y = -bh / 2 + bh * DrawKit.H(p.Seed, i), x = ((DrawKit.H(p.Seed, i, 1) * m.W - m.Now * 3000) % m.W + m.W) % m.W - m.W / 2; m.Box(x, y, 34, 300, 4, 4, m.C(p.Color3, .5f), .5f); }
                m.Text(Str(p), 160, 0, 0, 40, 30, m.C(p.Color3), m.W * .9f, m.C(p.Color2), 8);
            });
        });
        Q(d, typeof(StatusIconsParameter), (p, m, t, v, v2) => // 状態異常: ゆれる厚い丸札に文字
        {
            var it = p.ItemsForModel; float s = 110, gap = 30, x0 = -(it.Count * (s + gap) - gap) / 2 + s / 2;
            for (int i = 0; i < it.Count; i++)
            {
                float k = Ease(In(m, i * .1f, .3f)), x = x0 + i * (s + gap), y = MathF.Sin(m.Now * 3 + i) * 10;
                m.Disc(x, y, 0, s / 2 * k, s / 2 * k, 40, Pal(m, i), .4f);
                m.Torus(x, y, 20, s / 2 * k, 3, W);
                m.Text(it[i], 40 * k, x, y, 22, 4, W, s);
            }
        });
    }

    /// <summary>サイコロの目(手前の面に、玉のくぼみ)</summary>
    static void Pips(Sculpt m, float cx, float cy, float z, int face, Vector4 col)
    {
        float o = 55;
        (float, float)[] at = face switch
        {
            1 => [(0, 0)],
            2 => [(-o, -o), (o, o)],
            3 => [(-o, -o), (0, 0), (o, o)],
            4 => [(-o, -o), (o, -o), (-o, o), (o, o)],
            5 => [(-o, -o), (o, -o), (0, 0), (-o, o), (o, o)],
            _ => [(-o, -o), (o, -o), (-o, 0), (o, 0), (-o, o), (o, o)],
        };
        foreach (var (x, y) in at) m.Ball(cx + x, cy + y, z, face == 1 ? 30 : 18, face == 1 ? 30 : 18, 6, col);
    }
}
