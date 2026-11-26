using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 151〜180番(質感・表・ゲーム・配信・背景・画面効果)
// =====================================================================
internal static partial class Models3D
{
    static Vector4 WinBg(Sculpt m, GameWindowStyle s) => s switch { GameWindowStyle.Rpg => m.C(0x0D1B4Cu, .92f), GameWindowStyle.Retro => m.C(0x000000u), GameWindowStyle.Soft => m.C(0xFFF8F0u, .95f), _ => m.C(0x15171Eu, .9f) };
    static Vector4 WinText(Sculpt m, GameWindowStyle s) => s == GameWindowStyle.Soft ? m.C(0x4E342Eu) : new Vector4(1, 1, 1, 1);
    static List<string> SplitLines(string s) => s.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

    static partial void RegisterK1f(Dictionary<Type, SceneModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.08f, .09f, .11f, 1);
        S<OldPaperParameter>(d, (p, c, m) => // 古い写真・紙: 白いふちの厚い写真の台紙、折り目の山、めくれた角、しみ
        {
            float b = c.F(p.Border) * 1.5f; var tint = m.C(p.Tint, c.F(p.TintStrength) / 100);
            if (p.Kind == PaperKind.OldPhoto) { m.Box(0, 0, -40, m.W * 1.1f, m.H * 1.1f, 10, m.C(p.BackColor), .01f); RectBars(m, 0, 0, -10, m.W - b, m.H - b, b, m.C(p.BorderColor)); }
            m.Box(0, 0, -20, m.W - b * 2, m.H - b * 2, 4, tint, .01f);
            if (p.Kind == PaperKind.Burnt) foreach (int a in Sides) foreach (int bb in Sides) m.Ball(a * m.W / 2, bb * m.H / 2, -10, 260, 200, 20, m.C(0x3E2723u, .8f));
            for (int i = 0; i < Math.Min(p.Stains, 30); i++) m.Ball((R(p.Seed, i) - .5f) * m.W * .9f, (R(p.Seed, i, 1) - .5f) * m.H * .9f, -14, 30 + 80 * R(p.Seed, i, 2), 30 + 60 * R(p.Seed, i, 3), 3, m.C(0x8D6E63u, .2f));
            if (p.Folds) { m.Box(0, 0, -6, 4, m.H - b * 2, 8, m.C(0xFFFFFFu, .3f), .5f); m.Box(0, 0, -6, m.W - b * 2, 4, 8, m.C(0xFFFFFFu, .3f), .5f); }
            if (p.TornCorner) m.Pitch(m.W / 2, -m.H / 2 + 120, 0, .6f, () => m.Slab(-2, 4, m.C(p.BorderColor), m.W / 2 - 160, -m.H / 2, m.W / 2, -m.H / 2, m.W / 2, -m.H / 2 + 160));
            for (int i = 0; i < (int)(c.F(p.Scratches) / 10); i++) { int fr = (int)(m.Now * 12); m.Box((R(fr, i) - .5f) * m.W, 0, 0, 2, m.H, 1, m.C(0xFFFFFFu, .4f)); }
        });
        S<CrtFrameParameter>(d, (p, c, m) => // ブラウン管の枠: 画面を囲む厚い角丸のベゼル、ふくらんだガラスの映り込み、走査線、四隅の暗がり
        {
            float vs = c.F(p.VignetteStrength) / 100, fl = 1 - c.F(p.Flicker) / 100 * .2f * (R(p.Seed, (int)(m.Now * 30)) > .7f ? 1 : 0);
            if (p.Kind != CrtKind.Crt) for (int i = 0; i < 5; i++) { float f = vs * (.4f - i * .07f) * fl, w = (60 + i * 60) * (1.5f - c.F(p.VignetteSize) / 100); foreach (int s in Sides) { m.Box(s * (m.W / 2 - w / 2), 0, 20 + i, w, m.H, 4, m.C(p.VignetteColor, f), .01f); m.Box(0, s * (m.H / 2 - w / 2), 20 + i, m.W, w, 4, m.C(p.VignetteColor, f), .01f); } }
            if (p.Kind != CrtKind.Vignette)
            {
                float bz = c.F(p.BezelSize) / 100 * m.H * 1.2f, rd = c.F(p.Roundness) * 10; var bc = m.C(p.BezelColor);
                RectBars(m, 0, 0, 40, m.W - bz, m.H - bz, bz, bc); foreach (int a in Sides) foreach (int b in Sides) m.Slab(30, 30, bc, a * (m.W / 2 - bz), b * (m.H / 2 - bz), a * (m.W / 2 - bz - rd), b * (m.H / 2 - bz), a * (m.W / 2 - bz), b * (m.H / 2 - bz - rd));
                for (int i = 0; i < 60; i++) m.Box(0, -m.H / 2 + i * m.H / 60, 10, m.W, 3, 1, m.C(0x000000u, c.F(p.Scanlines) / 100 * .5f));
                m.Ball(-m.W * .2f, -m.H * .25f, 15, m.W * .25f, m.H * .15f, 4, m.C(0xFFFFFFu, c.F(p.Glare) / 100 * .15f));
            }
        });
        S<RankingBoardParameter>(d, (p, c, m) => // ランキング表: 見出しの帯の下に、下から順にせり上がる順位の厚い行(1〜3位はメダル色)
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); var rows = SplitLines(p.Items); float rh = c.F(p.RowHeight), bw = c.F(p.BoardWidth), iv = c.F(p.Interval); int n = rows.Count; float top = o.Y - (n * rh * 1.1f) / 2;
            m.Box(o.X, top - rh * .8f, 0, bw, rh * .9f, 40, m.C(0xC62828u), .2f); m.Text(p.Title, rh * .5f, o.X, top - rh * .8f, 22, 4, W);
            for (int i = 0; i < n; i++)
            {
                int order = p.FromBottom ? n - 1 - i : i; float k = Ease(Math.Clamp((m.Now - order * iv) / .4f, 0, 1)); if (k <= 0) continue; float y = top + rh * 1.1f * (i + .5f);
                var parts = rows[i].Split(',', 2); string name = parts[0], val = parts.Length > 1 ? parts[1] : "";
                m.Move(0, 0, -400 * (1 - k), () => m.Pitch(o.X, y, 0, (1 - k) * MathF.PI / 2, () =>
                {
                    m.Box(o.X, y, 0, bw, rh, 30, m.C(p.RowColor), .15f);
                    uint mc = i == 0 ? 0xFFD54Fu : i == 1 ? 0xCFD8DCu : i == 2 ? 0xD7A066u : 0x546E7Au; m.Disc(o.X - bw / 2 + rh * .7f, y, 18, rh * .38f, rh * .38f, 16, m.C(mc), .5f); m.Text((i + 1).ToString(), rh * .45f, o.X - bw / 2 + rh * .7f, y, 28, 3, i < 3 ? K : W);
                    m.Text(name, rh * .45f, o.X - bw * .1f, y, 18, 3, m.C(p.TextColor), bw * .5f); m.Text(val, rh * .4f, o.X + bw * .35f, y, 18, 3, m.C(p.TextColor));
                }));
            }
        });
        S<CompareTableParameter>(d, (p, c, m) => // 比較表: 見出しの列と、順に出る行の厚いマス、強調する列の枠、◎○×の印
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); var rows = SplitLines(p.Data).Select(r => r.Split(',')).ToList(); if (rows.Count == 0) return;
            int cols = rows.Max(r => r.Length); float tw = c.F(p.TableWidth), rh = c.F(p.RowHeight), cw = tw / cols, th = rh * rows.Count, x0 = o.X - tw / 2, y0 = o.Y - th / 2;
            for (int r = 0; r < rows.Count; r++)
            {
                float k = Ease(Math.Clamp((m.Now - r * c.F(p.Interval)) / .35f, 0, 1)); if (k <= 0) continue; float y = y0 + rh * (r + .5f);
                for (int col = 0; col < cols; col++)
                {
                    float x = x0 + cw * (col + .5f); string s = col < rows[r].Length ? rows[r][col].Trim() : ""; bool head = r == 0 || col == 0, hl = col == p.Highlight;
                    m.Scale(x, y, 0, k, () => { m.Box(x, y, hl ? 14 : 0, cw - 6, rh - 6, hl ? 40 : 24, head ? m.C(p.HeaderColor) : m.C(p.BodyColor), .1f); var tc = head ? W : m.C(p.TextColor); if (s == "◎" || s == "○") { m.Torus(x, y, 30, rh * .25f, rh * .05f, s == "◎" ? m.C(0xE53935u) : m.C(0xFB8C00u)); if (s == "◎") m.Torus(x, y, 30, rh * .14f, rh * .04f, m.C(0xE53935u)); } else if (s == "×") { m.Turn(x, y, MathF.PI / 4, () => { m.Box(x, y, 30, rh * .5f, rh * .09f, 10, m.C(0x1E88E5u), .4f); m.Box(x, y, 30, rh * .09f, rh * .5f, 10, m.C(0x1E88E5u), .4f); }); } else m.Text(s, rh * .38f, x, y, 26, 3, tc, cw - 20); });
                }
            }
            if (p.Highlight >= 0 && p.Highlight < cols) RectBars(m, x0 + cw * (p.Highlight + .5f), o.Y, 40, cw, th + 10, 10, m.C(p.Accent));
        });
        S<BarRaceParameter>(d, (p, c, m) => // 棒グラフのレース: 時代ごとに伸びて順位を入れ替える厚い棒と、大きな年の表示
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); var per = p.Periods.Split(',').Select(s => s.Trim()).ToList(); var data = SplitLines(p.Data).Select(r => { var kv = r.Split(':'); return (n: kv[0], v: kv.Length > 1 ? kv[1].Split(',').Select(x => F(x.Trim())).ToList() : new List<float>()); }).ToList(); if (data.Count == 0) return;
            int np = Math.Max(1, data.Max(x => x.v.Count)); float tt = Math.Clamp(m.Now / MathF.Max(.5f, c.F(p.Duration)), 0, 1) * (np - 1); int i0 = (int)tt; float fr = tt - i0; int i1 = Math.Min(np - 1, i0 + 1);
            float V(List<float> v, int i) => i < v.Count ? v[i] : (v.Count > 0 ? v[^1] : 0);
            var cur = data.Select((x, idx) => (x.n, idx, val: V(x.v, i0) + (V(x.v, i1) - V(x.v, i0)) * fr)).OrderByDescending(x => x.val).Take(Math.Max(1, p.TopN)).ToList(); float mx = MathF.Max(1, cur.Max(x => x.val));
            float cw = c.F(p.ChartWidth), chh = c.F(p.ChartHeight), bh = chh / cur.Count, x0 = o.X - cw / 2 + 200;
            m.Text(p.Title, 50, o.X, o.Y - chh / 2 - 60, 0, 4, m.C(p.TextColor)); m.Text(i0 < per.Count ? per[(int)MathF.Round(tt) < per.Count ? (int)MathF.Round(tt) : per.Count - 1] : "", 140, o.X + cw / 2 - 200, o.Y + chh / 2 - 80, -40, 20, m.C(p.TextColor, .5f));
            for (int r = 0; r < cur.Count; r++) { float y = o.Y - chh / 2 + bh * (r + .5f), L = (cw - 400) * cur[r].val / mx; m.Box(x0 + L / 2, y, 0, MathF.Max(2, L), bh * .7f, 60, ChartCol(m, p.Palette, cur[r].idx), .1f); m.Text(cur[r].n, 40, x0 - 100, y, 0, 3, m.C(p.TextColor)); m.Text($"{cur[r].val:0}{p.Unit}", 36, x0 + L + 100, y, 0, 3, m.C(p.TextColor)); }
        });
        S<MapPulseParameter>(d, (p, c, m) => // 地図の波紋: 床に寝かせた地点の柱の点と、広がる輪の波紋、地名の札
        {
            UseFont(m, p.Font); var pts = SplitLines(p.Points).Select(r => { var a = r.Split(':'); var xy = a[0].Split(','); return (x: F(xy[0]), y: xy.Length > 1 ? F(xy[1]) : 50, n: a.Length > 1 ? a[1] : ""); }).ToList();
            var col = m.C(p.Color); var pos = new List<Vector2>();
            for (int i = 0; i < pts.Count; i++)
            {
                var q = PctPos(m, pts[i].x, pts[i].y); pos.Add(q); float k = Ease(Math.Clamp((m.Now - i * c.F(p.Interval)) / .3f, 0, 1)); if (k <= 0) continue; float ds = c.F(p.DotSize) * 1.5f;
                m.Cyl(q.X, q.Y - ds, 0, ds * .6f * k, ds * 2 * k, col); m.Ball(q.X, q.Y - ds * 2 * k, 0, ds * k, col);
                for (int r = 0; r < 3; r++) { float f = Frac(m.Now * c.F(p.RippleSpeed) + r / 3f); m.Pitch(q.X, q.Y, 0, 1.2f, () => m.Torus(q.X, q.Y, 0, c.F(p.RippleSize) * f, 3, m.C(col, (1 - f) * k))); }
                if (pts[i].n.Length > 0) { m.Box(q.X, q.Y - ds * 4, 10, pts[i].n.Length * c.F(p.FontSize) + 30, c.F(p.FontSize) * 1.4f, 14, m.C(0x000000u, .6f), .4f); m.Text(pts[i].n, c.F(p.FontSize), q.X, q.Y - ds * 4, 20, 2, W); }
            }
            if (p.Connect) for (int i = 0; i + 1 < pos.Count; i++) m.Tube(Enumerable.Range(0, 13).Select(j => { float u = j / 12f; var q = Vector2.Lerp(pos[i], pos[i + 1], u); return V3(q.X, q.Y - MathF.Sin(u * MathF.PI) * 120, 0); }).ToList(), 3, m.C(col, .6f));
        });
        S<FlowchartParameter>(d, (p, c, m) => // フローチャート: 角丸の始まり・終わり、ひし形の分かれ道、四角の手順の厚い箱と、つなぐ矢印
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); var st = SplitLines(p.Steps); int n = st.Count; if (n == 0) return;
            float bw = c.F(p.BoxWidth), bh = c.F(p.BoxHeight), gap = c.F(p.Gap), fs = c.F(p.FontSize);
            Vector2 At(int i) => p.Direction switch { FlowchartLayout.Vertical => new(o.X, o.Y + (i - (n - 1) / 2f) * (bh + gap)), FlowchartLayout.Zigzag => new(o.X + ((i % 3) - 1) * (bw + gap) * ((i / 3) % 2 == 0 ? 1 : -1), o.Y + (i / 3 - ((n - 1) / 3) / 2f) * (bh + gap)), _ => new(o.X + (i - (n - 1) / 2f) * (bw + gap), o.Y) };
            for (int i = 0; i < n; i++)
            {
                float k = Ease(Math.Clamp((m.Now - i * c.F(p.Interval)) / .35f, 0, 1)); if (k <= 0) continue; var q = At(i); string s = st[i]; bool end = s.StartsWith("("), dec = s.StartsWith("?"); if (end || dec) s = s[1..].TrimEnd(')');
                m.Scale(q.X, q.Y, 0, k, () =>
                {
                    if (dec) m.Slab(-25, 50, m.C(p.DecisionColor), q.X, q.Y - bh * .7f, q.X + bw * .6f, q.Y, q.X, q.Y + bh * .7f, q.X - bw * .6f, q.Y);
                    else m.Box(q.X, q.Y, 0, bw, bh, 50, end ? m.C(p.EndColor) : m.C(p.BoxColor), end ? .48f : .12f);
                    m.Text(s, fs, q.X, q.Y, 28, 3, W, bw * .9f);
                });
                if (i > 0 && k > 0) { var a = At(i - 1); var dd = Vector2.Normalize(q - a); var s0 = a + dd * (MathF.Abs(dd.X) > .5f ? bw / 2 : bh / 2); var s1 = q - dd * (MathF.Abs(dd.X) > .5f ? bw / 2 + 10 : bh / 2 + 10); m.Limb(s0.X, s0.Y, 0, s1.X, s1.Y, 0, 5, 5, m.C(p.ArrowColor)); var nn = new Vector2(-dd.Y, dd.X) * 18; m.Slab(-8, 16, m.C(p.ArrowColor), s1.X + dd.X * 10, s1.Y + dd.Y * 10, s1.X - dd.X * 24 + nn.X, s1.Y - dd.Y * 24 + nn.Y, s1.X - dd.X * 24 - nn.X, s1.Y - dd.Y * 24 - nn.Y); }
            }
        });
        S<HandwritingParameter>(d, (p, c, m) => // 手書き文字: 左から書かれていく厚い文字と、その先で動くペン
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); string s = TextKit.Lines(p.Text); float fs = c.F(p.FontSize), k = Math.Clamp(m.Now / MathF.Max(.1f, c.F(p.WriteTime)), 0, 1); int n = (int)MathF.Ceiling(s.Length * k);
            m.Turn(o.X, o.Y, c.F(p.Tilt) * MathF.PI / 180, () =>
            {
                float w = s.Length * fs; string part = s[..Math.Min(n, s.Length)]; float pw = part.Length * fs; m.Text(part, fs, o.X - w / 2 + pw / 2, o.Y, 0, fs * .12f, m.C(p.Color));
                if (p.ShowPen && k < 1) { float px = o.X - w / 2 + pw, py = o.Y + MathF.Sin(m.Now * 20) * fs * .2f; m.Turn(px, py, -.6f, () => { m.Lathe(px, 30, m.C(0x37474Fu), 0, py, 10, py - 30, 14, py - 40, 14, py - 240); m.Lathe(px, 30, m.C(p.Color), 14, py - 240, 14, py - 300, 0, py - 305); }); }
            });
        });
        S<StatusScreenParameter>(d, (p, c, m) => // ステータス画面: 枠のある厚い窓、名前と職業、HP・MPの棒、能力値の棒グラフ
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float sc = c.F(p.Scale) / 100, k = Ease(Math.Clamp(m.Now / .4f, 0, 1)); var bg = WinBg(m, p.Style); var tc = WinText(m, p.Style); var ac = m.C(p.Accent);
            var stats = p.Stats.Split(',', '、').Select(x => x.Split(':')).Where(x => x.Length == 2).ToList(); float w = 1000, h = 300 + stats.Count * 70;
            m.Scale(o.X, o.Y, 0, sc * k, () =>
            {
                m.Box(o.X, o.Y, -10, w, h, 30, bg, .08f); RectBars(m, o.X, o.Y, 8, w - 14, h - 14, 6, ac);
                float y = o.Y - h / 2 + 70; m.Text(p.CharName, 56, o.X - w * .2f, y, 14, 4, tc); m.Text($"{p.Job}  Lv {(int)c.F(p.Level)}", 40, o.X + w * .25f, y, 14, 3, ac);
                void Bar(string lab, float v, float mx, float yy, uint col) { m.Text(lab, 36, o.X - w / 2 + 80, yy, 14, 2, tc); float bw = w * .55f, f = Math.Clamp(v / MathF.Max(1, mx), 0, 1) * k; m.Box(o.X + 40, yy, 6, bw, 26, 14, m.C(0x37474Fu), .4f); m.Box(o.X + 40 - bw / 2 + bw * f / 2, yy, 12, bw * f, 26, 18, m.C(col), .4f); m.Text($"{(int)v}/{(int)mx}", 30, o.X + w / 2 - 110, yy, 14, 2, tc); }
                Bar("HP", c.F(p.Hp), c.F(p.HpMax), y + 90, 0x66BB6Au); Bar("MP", c.F(p.Mp), c.F(p.MpMax), y + 150, 0x42A5F5u);
                for (int i = 0; i < stats.Count; i++) { float yy = y + 230 + i * 70, v = F(stats[i][1]); m.Text(stats[i][0], 34, o.X - w / 2 + 120, yy, 14, 2, tc); float bw = w * .6f * Math.Clamp(v / 100, 0, 1) * k; m.Box(o.X - w / 2 + 260 + bw / 2, yy, 10, MathF.Max(1, bw), 24, 20, ac, .3f); m.Text(((int)v).ToString(), 32, o.X - w / 2 + 300 + bw, yy, 14, 2, tc); }
            });
        });
        S<ItemGetParameter>(d, (p, c, m) => // アイテム入手: 回って光る剣・薬・鍵・宝石・金貨などの立体と、メッセージの窓
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float sc = c.F(p.Scale) / 100, k = Ease(Math.Clamp(m.Now / .35f, 0, 1)); var ac = m.C(p.Accent);
            m.Scale(o.X, o.Y, 0, sc, () =>
            {
                float ix = o.X, iy = o.Y - 220 - MathF.Sin(m.Now * 2) * 15; m.Ball(ix, iy, -40, 160 * k, m.C(p.Accent, .2f));
                for (int i = 0; i < 8; i++) { float a = i * MathF.PI / 4 + m.Now; m.Limb(ix, iy, -50, ix + MathF.Cos(a) * 230 * k, iy + MathF.Sin(a) * 230 * k, -50, 8, 1, m.C(p.Accent, .4f)); }
                m.Yaw(ix, iy, 0, m.Now * 2, () => m.Scale(ix, iy, 0, k, () =>
                {
                    switch (p.Icon)
                    {
                        case ItemIcon.Potion: m.Lathe(ix, 0, m.C(0xE53935u, .8f), 0, iy + 80, 70, iy + 60, 75, iy, 30, iy - 50, 24, iy - 90); m.Cyl(ix, iy - 105, 0, 22, 30, m.C(0x8D6E63u)); break;
                        case ItemIcon.Key: m.Torus(ix, iy - 60, 0, 40, 14, ac); m.Box(ix, iy + 40, 0, 20, 140, 20, ac, .3f); m.Box(ix + 25, iy + 90, 0, 40, 16, 16, ac, .3f); m.Box(ix + 20, iy + 60, 0, 30, 16, 16, ac, .3f); break;
                        case ItemIcon.Gem: m.Lathe(ix, 0, m.C(0x26C6DAu), 0, iy + 90, 90, iy - 10, 60, iy - 50, 0, iy - 50); break;
                        case ItemIcon.Coin: Coin(m, ix, iy, 0, 80, 0, ac); break;
                        case ItemIcon.Chest: m.Box(ix, iy + 20, 0, 180, 100, 110, m.C(0x8D5A2Bu), .1f); m.Box(ix, iy - 50, 0, 180, 50, 110, m.C(0x8D5A2Bu), .3f); m.Box(ix, iy, 60, 30, 40, 10, ac, .3f); break;
                        case ItemIcon.Scroll: m.Box(ix, iy, 0, 140, 160, 6, m.C(0xFFF3E0u), .02f); foreach (int s in Sides) m.Cyl(ix, iy + s * 85, 0, 18, 170, m.C(0x8D6E63u)); break;
                        default: m.Slab(-8, 16, m.C(0xCFD8DCu), ix - 14, iy - 30, ix, iy - 160, ix + 14, iy - 30); m.Box(ix, iy - 20, 0, 110, 20, 26, ac, .3f); m.Box(ix, iy + 30, 0, 22, 80, 22, m.C(0x6D4C41u), .3f); m.Ball(ix, iy + 76, 0, 16, ac); break;
                    }
                }));
                string msg = p.Message.Replace("{item}", p.ItemName); float w = MathF.Max(600, msg.Length * 52 + 100);
                m.Box(o.X, o.Y + 60, -10, w, 140, 30, WinBg(m, p.Style), .08f); RectBars(m, o.X, o.Y + 60, 8, w - 12, 128, 5, ac); m.Text(msg, 46, o.X, o.Y + 60, 14, 3, WinText(m, p.Style), w - 60);
            });
        });
        S<ImpactTextParameter>(d, (p, c, m) => // ドン！: 奥から飛んできてたたきつけられる、ふち取りの厚い大きな文字、集中線と飛ぶ破片
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float sl = MathF.Max(.05f, c.F(p.SlamTime)), k = Math.Clamp(m.Now / sl, 0, 1), after = MathF.Max(0, m.Now - sl), shake = after < .3f ? MathF.Sin(after * 80) * (1 - after / .3f) * 20 : 0, fs = c.F(p.FontSize);
            if (p.Flash && after > 0 && after < .15f) m.Box(0, 0, 200, m.W * 2, m.H * 2, 4, m.C(0xFFFFFFu, 1 - after / .15f), .01f);
            if (p.SpeedLines && after > 0) for (int i = 0; i < 60; i++) { float a = R(p.Seed, i) * MathF.Tau, r0 = fs * 1.4f + R(p.Seed, i, 1) * 200; m.Limb(o.X + MathF.Cos(a) * r0, o.Y + MathF.Sin(a) * r0, -100, o.X + MathF.Cos(a) * m.W, o.Y + MathF.Sin(a) * m.W, -100, 1, 10, m.C(0x111111u, .8f)); }
            m.Move(shake, 0, -2000 * (1 - k * k), () => m.Turn(o.X, o.Y, c.F(p.Tilt) * MathF.PI / 180, () => m.Text(TextKit.Lines(p.Text), fs, o.X, o.Y, 0, fs * .25f, m.C(p.Color), 0, m.C(p.OutlineColor), c.F(p.OutlineWidth))));
            if (p.Debris && after > 0) for (int i = 0; i < 20; i++) { float a = R(p.Seed, i, 2) * MathF.Tau, v = 600 + 600 * R(p.Seed, i, 3); float x = o.X + MathF.Cos(a) * v * after, y = o.Y + fs * .4f - MathF.Abs(MathF.Sin(a)) * v * after + 1500 * after * after; m.Turn(x, y, after * 10 + i, () => m.Box(x, y, MathF.Sin(a * 3) * v * after * .5f, 30, 20, 16, m.C(0x8D6E63u), .1f)); }
        });
        S<QuizParameter>(d, (p, c, m) => // クイズ: 問題の札、4つの選択肢の厚い板、減っていく時間の棒、正解が赤く浮き上がる
        {
            UseFont(m, p.Font); float y0 = (c.F(p.PosY) / 100 - .5f) * m.H, w = m.W * c.F(p.WidthRatio) / 100, fs = c.F(p.FontSize), rt = c.F(p.RevealTime); bool rev = m.Now >= rt; var ch = SplitLines(p.Choices); var pc = m.C(p.PanelColor);
            m.Box(0, y0 - 300, 0, w, fs * 2, 40, pc, .15f); m.Text("Q. " + p.Question, fs, 0, y0 - 300, 22, 4, W, w - 60);
            if (p.TimerBar && !rev) { float f = 1 - m.Now / MathF.Max(.1f, rt); m.Box(-w / 2 + w * f / 2, y0 - 300 + fs * 1.2f, 0, w * f, 14, 14, m.C(0xFFD54Fu), .4f); }
            for (int i = 0; i < ch.Count; i++)
            {
                int col = i % 2, row = i / 2; float x = (col - .5f) * (w / 2 + 10), y = y0 - 80 + row * (fs * 2.4f); bool ok = rev && i == p.Answer - 1; float lift = ok ? 60 + MathF.Sin(m.Now * 6) * 10 : 0;
                m.Box(x, y, lift, w / 2 - 20, fs * 2, 40, ok ? m.C(p.CorrectColor) : rev ? m.C(0x455A64u) : pc, .2f); m.Disc(x - w / 4 + fs * 1.2f, y, lift + 22, fs * .6f, fs * .6f, 10, W); m.Text(((char)('A' + i)).ToString(), fs * .8f, x - w / 4 + fs * 1.2f, y, lift + 28, 3, pc); m.Text(ch[i], fs, x + fs * .6f, y, lift + 22, 4, W, w / 2 - fs * 3);
            }
            if (rev) { var q = new Vector2(((p.Answer - 1) % 2 - .5f) * (w / 2 + 10), y0 - 80 + ((p.Answer - 1) / 2) * (fs * 2.4f)); float k = Ease(Math.Clamp((m.Now - rt) * 4, 0, 1)); m.Torus(q.X, q.Y, 100, fs * 1.6f * k, fs * .18f, m.C(0xE53935u)); }
        });
        S<RouletteParameter>(d, (p, c, m) => // ルーレット: 色分けされた厚い円盤が回って止まり、上の針が指す/3つ並んで回るスロットのリール
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); var items = p.Items.Split(',', '、').Select(s => s.Trim()).Where(s => s.Length > 0).ToList(); int n = Math.Max(1, items.Count), stop = Math.Clamp(p.StopAt - 1, 0, n - 1); float r = c.F(p.Radius), k = Math.Clamp(m.Now / MathF.Max(.5f, c.F(p.SpinTime)), 0, 1), e = 1 - MathF.Pow(1 - k, 3);
            if (p.Kind == RouletteKind.Wheel)
            {
                float seg = MathF.Tau / n, fin = -MathF.PI / 2 - (stop + .5f) * seg, ang = fin - (1 - e) * c.F(p.Turns) * MathF.Tau;
                m.Disc(o.X, o.Y, -40, r * 1.08f, r * 1.08f, 50, m.C(0x5D4037u), .4f);
                m.Turn(o.X, o.Y, ang, () => { for (int i = 0; i < n; i++) { m.Slab(-10, 30 + (i == stop && k >= 1 ? 30 : 0), ChartCol(m, p.Palette, i), PiePts(o.X, o.Y, r, i * seg, (i + 1) * seg)); float mid = (i + .5f) * seg; m.Turn(o.X + MathF.Cos(mid) * r * .62f, o.Y + MathF.Sin(mid) * r * .62f, mid, () => m.Text(items[i], r * .13f, o.X + MathF.Cos(mid) * r * .62f, o.Y + MathF.Sin(mid) * r * .62f, 24, 3, W)); } for (int i = 0; i < n; i++) { float a = i * seg; m.Ball(o.X + MathF.Cos(a) * r * 1.02f, o.Y + MathF.Sin(a) * r * 1.02f, 10, 12, m.C(0xFFD54Fu)); } });
                m.Disc(o.X, o.Y, 30, r * .12f, r * .12f, 40, m.C(0xFFD54Fu), .5f); m.Slab(20, 40, m.C(0xE53935u), o.X - 30, o.Y - r - 70, o.X + 30, o.Y - r - 70, o.X, o.Y - r + 30);
            }
            else
            {
                float rw = r * .55f; m.Box(o.X, o.Y, -60, rw * 3.4f, r * 1.1f, 60, m.C(0xC62828u), .15f);
                for (int j = 0; j < 3; j++) { float x = o.X + (j - 1) * rw * 1.08f, kk = Math.Clamp(m.Now / MathF.Max(.5f, c.F(p.SpinTime) * (.6f + .2f * j)), 0, 1), ee = 1 - MathF.Pow(1 - kk, 3), seg = MathF.Tau / n, ang = (stop * seg) + (1 - ee) * c.F(p.Turns) * MathF.Tau; m.Pitch(x, o.Y, -rw, ang, () => { for (int i = 0; i < n; i++) { float a = -i * seg; m.Pitch(x, o.Y, -rw, a, () => { m.Box(x, o.Y, 0, rw, 2 * rw * MathF.Tan(seg / 2) - 4, 6, i % 2 == 0 ? W : m.C(0xF5F5F5u), .02f); m.Text(items[i], rw * .25f, x, o.Y, 6, 3, ChartCol(m, p.Palette, i), rw * .9f); }); } }); }
                m.Box(o.X, o.Y, 30, rw * 3.4f, 6, 6, m.C(0xFFD54Fu, .7f));
            }
        });
        S<LiveFrameParameter>(d, (p, c, m) => // ライブ配信の枠: 上の LIVE 札と視聴者数、下のタイトルの帯、右のコメント欄の窓に流れるコメント
        {
            UseFont(m, p.Font); var ac = m.C(p.Accent); float cw = p.ShowChat ? m.W * c.F(p.ChatWidth) / 100 : 0;
            m.Box(-m.W / 2 + 120, -m.H / 2 + 60, 0, 160, 60, 30, ac, .3f); m.Text("LIVE", 40, -m.W / 2 + 120, -m.H / 2 + 60, 16, 3, W); m.Ball(-m.W / 2 + 60, -m.H / 2 + 60, 18, 8, m.C(0xFFFFFFu, MathF.Sin(m.Now * 4) > 0 ? 1 : .3f));
            m.Box(-m.W / 2 + 330, -m.H / 2 + 60, 0, 220, 60, 20, m.C(0x000000u, .6f), .3f); m.Text($"👁 {(int)(c.F(p.Viewers) + m.Now * c.F(p.ViewerGrowth))}", 34, -m.W / 2 + 330, -m.H / 2 + 60, 12, 2, W);
            float t = c.F(p.StartMinutes) * 60 + m.Now; m.Text($"{(int)t / 3600}:{(int)t / 60 % 60:00}:{(int)t % 60:00}", 30, -m.W / 2 + 560, -m.H / 2 + 60, 0, 2, W);
            m.Box(-cw / 2, m.H / 2 - 70, 0, m.W - cw, 140, 20, m.C(0x000000u, .65f), .05f); m.Disc(-m.W / 2 + 80, m.H / 2 - 70, 14, 46, 46, 10, ac); m.Text(p.Channel, 34, -m.W / 2 + 330, m.H / 2 - 100, 14, 2, m.C(0xFFFFFFu, .8f)); m.Text(p.StreamTitle, 44, -cw / 2 + 60, m.H / 2 - 50, 14, 3, W, m.W - cw - 260);
            if (p.ShowChat) { float x = m.W / 2 - cw / 2; m.Box(x, 0, -10, cw, m.H, 20, m.C(0x111318u, .85f), .02f); var cm = SplitLines(p.Comments); int shown = Math.Min(cm.Count, (int)(m.Now / MathF.Max(.2f, c.F(p.ChatInterval))) + 1); for (int i = 0; i < Math.Min(shown, 12); i++) { int idx = shown - 1 - i; var kv = cm[idx].Split(':', 2); float y = m.H / 2 - 220 - i * 80; m.Text(kv[0], 26, x - cw / 2 + 80, y, 6, 2, Pal(m, idx), 140); m.Text(kv.Length > 1 ? kv[1] : "", 30, x + 60, y, 6, 2, W, cw - 180); } }
        });
        S<DonationParameter>(d, (p, c, m) => // 投げ銭: 金額で色の変わる厚いカードが落ちてきて、名前・金額・メッセージを表示
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float amt = c.F(p.Amount), w = c.F(p.CardWidth), k = Ease(Math.Clamp(m.Now / .45f, 0, 1));
            uint auto = amt >= 10000 ? 0xE62117u : amt >= 5000 ? 0xF57C00u : amt >= 2000 ? 0xE91E63u : amt >= 1000 ? 0xFFCA28u : amt >= 500 ? 0x1DE9B6u : 0x1E88E5u; var col = p.AutoColor ? m.C(auto) : m.C(p.Color);
            m.Move(0, -400 * (1 - k), () => m.Pitch(o.X, o.Y, 0, (1 - k) * 1.2f, () =>
            {
                m.Box(o.X, o.Y - 50, 0, w, 100, 30, m.C(Sculpt.Dark(col, .8f), 1), .12f); m.Box(o.X, o.Y + 70, 0, w, 140, 30, col, .12f);
                m.Disc(o.X - w / 2 + 60, o.Y - 50, 18, 36, 36, 10, m.C(0xFFFFFFu, .8f)); m.Text(p.Sender, 34, o.X - w / 2 + 260, o.Y - 70, 18, 2, m.C(0xFFFFFFu, .85f)); m.Text($"{p.Currency}{amt:N0}", 44, o.X - w / 2 + 260, o.Y - 25, 18, 3, W);
                m.Text(p.Message, 34, o.X, o.Y + 70, 18, 2, W, w - 60);
            }));
        });
        S<SnsPostParameter>(d, (p, c, m) => // SNS投稿: 丸いアイコン・名前・本文の厚いカード、いいねで跳ねるハートと増える数
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float w = c.F(p.CardWidth), h = 520, k = Ease(Math.Clamp(m.Now / .4f, 0, 1)); bool liked = m.Now >= c.F(p.LikeAt); var bg = p.Dark ? m.C(0x15202Bu) : W; var tc = p.Dark ? W : m.C(0x0F1419u);
            m.Scale(o.X, o.Y, 0, k, () =>
            {
                m.Box(o.X, o.Y, 0, w, h, 24, bg, .08f); m.Ball(o.X - w / 2 + 90, o.Y - h / 2 + 90, 14, 50, 50, 14, m.C(p.IconColor));
                m.Text(p.DisplayName, 40, o.X - w / 2 + 330, o.Y - h / 2 + 70, 14, 3, tc); m.Text($"{p.Handle} ・ {p.When}", 28, o.X - w / 2 + 330, o.Y - h / 2 + 120, 14, 2, m.C(0x8899A6u));
                m.Text(p.Body, 36, o.X, o.Y - 10, 14, 2, tc, w - 100);
                float ly = o.Y + h / 2 - 60; float pop = liked ? 1 + .4f * MathF.Max(0, 1 - (m.Now - c.F(p.LikeAt)) * 4) : 1; Heart3D(m, o.X - w / 2 + 120, ly, 16, 26 * pop, liked ? m.C(0xF91880u) : m.C(0x8899A6u)); m.Text(((int)c.F(p.Likes) + (liked ? 1 : 0)).ToString("N0"), 30, o.X - w / 2 + 220, ly, 14, 2, liked ? m.C(0xF91880u) : m.C(0x8899A6u));
                m.Text($"⟲ {(int)c.F(p.Reposts):N0}", 30, o.X, ly, 14, 2, m.C(0x8899A6u));
            });
        });
        S<SearchBarParameter>(d, (p, c, m) => // 検索バー: 丸い厚い検索欄に1文字ずつ打たれる文字と点滅するカーソル、下に開く候補の板
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float w = c.F(p.BarWidth), h = 110; int n = Math.Min(p.Query.Length, (int)(m.Now * c.F(p.TypeSpeed))); var bg = p.Dark ? m.C(0x303134u) : W; var tc = p.Dark ? W : m.C(0x202124u);
            m.Box(o.X, o.Y, 0, w, h, 30, bg, .48f); m.Torus(o.X - w / 2 + 70, o.Y - 6, 18, 18, 4, m.C(0x9AA0A6u)); m.Limb(o.X - w / 2 + 82, o.Y + 8, 18, o.X - w / 2 + 96, o.Y + 22, 18, 4, 4, m.C(0x9AA0A6u));
            string s = p.Query[..n]; float tw = s.Length * 44; m.Text(s, 44, o.X - w / 2 + 120 + tw / 2, o.Y, 18, 2, tc); if (MathF.Sin(m.Now * 8) > 0) m.Box(o.X - w / 2 + 125 + tw, o.Y, 18, 3, 50, 2, m.C(p.Accent));
            if (n > 0) { var sg = SplitLines(p.Suggestions); float sh = sg.Count * 80 + 20; m.Box(o.X, o.Y + h / 2 + sh / 2, -10, w, sh, 20, bg, .08f); for (int i = 0; i < sg.Count; i++) m.Text(sg[i], 36, o.X - w / 2 + 120 + sg[i].Length * 18, o.Y + h / 2 + 50 + i * 80, 6, 2, tc); }
            m.Disc(o.X + w / 2 - 70, o.Y, 18, 34, 34, 10, m.C(p.Accent));
        });
        S<ReactionParameter>(d, (p, c, m) => // リアクション: 振り向きの曲線・びっくりの線・ドキッのハート・ピキッの怒りの線・ショックの稲妻と、効果音の文字
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float s = c.F(p.Size), k = Ease(Math.Clamp(m.Now / .25f, 0, 1)); var lc = m.C(p.LineColor);
            m.Zoom(o.X, o.Y, p.Mirror ? -1 : 1, 1, () => m.Scale(o.X, o.Y, 0, k, () =>
            {
                string snd = p.Sound;
                switch (p.Kind)
                {
                    case ReactionMark.Notice: for (int i = 0; i < 3; i++) { float a = -MathF.PI / 2 + (i - 1) * .5f; m.Limb(o.X + MathF.Cos(a) * s * .3f, o.Y + MathF.Sin(a) * s * .3f, 0, o.X + MathF.Cos(a) * s * .7f, o.Y + MathF.Sin(a) * s * .7f, 0, s * .03f, s * .05f, lc); } if (snd == "") snd = "！"; break;
                    case ReactionMark.Doki: Heart3D(m, o.X, o.Y, 0, s * .4f * (1 + .15f * MathF.Max(0, MathF.Sin(m.Now * 12))), m.C(0xFF4D8Du)); if (snd == "") snd = "ドキッ"; break;
                    case ReactionMark.Piki: for (int i = 0; i < 4; i++) { float a = i * MathF.PI / 2 + MathF.PI / 4; m.Turn(o.X, o.Y, a, () => m.Tube(ArcPts3(o.X + s * .3f, o.Y, s * .18f, MathF.PI * .6f, MathF.PI * 1.4f, 0, 10), s * .05f, m.C(0xE53935u))); } if (snd == "") snd = "ピキッ"; break;
                    case ReactionMark.Shock: m.Tube(new List<Vector3> { V3(o.X - s * .2f, o.Y - s * .5f), V3(o.X + s * .1f, o.Y - s * .1f), V3(o.X - s * .1f, o.Y), V3(o.X + s * .2f, o.Y + s * .5f) }, s * .05f, m.C(0xFFD54Fu)); if (snd == "") snd = "ガーン"; break;
                    default: m.Tube(ArcPts3(o.X, o.Y, s * .5f, MathF.PI * 1.1f, MathF.PI * 1.9f, 0, 16), s * .03f, lc); m.Tube(ArcPts3(o.X, o.Y, s * .35f, MathF.PI * 1.15f, MathF.PI * 1.85f, 0, 16), s * .03f, lc); if (snd == "") snd = "えっ？"; break;
                }
                m.Text(snd, s * .35f, o.X + s * .6f, o.Y - s * .5f, 10, s * .06f, m.C(p.TextColor), 0, lc, 5);
            }));
        });
        S<TearsParameter>(d, (p, c, m) => // 涙: 目元からこぼれるしずく/滝のように流れる涙の管/目にたまってうるむ涙
        {
            var col = m.C(p.Color, .85f); float s = c.F(p.Size);
            void Eye(Vector2 e, int side)
            {
                switch (p.Kind)
                {
                    case TearKind.Single: { float f = Frac(m.Now * .6f); float y = e.Y + f * 260; m.Lathe(e.X, 40, col, 0, y - s * .5f, s * .25f, y, s * .3f, y + s * .2f, 0, y + s * .35f); } break;
                    case TearKind.Teary: m.Ball(e.X, e.Y + s * .2f, 40, s * 1.1f, s * .3f, s * .3f, m.C(p.Color, .7f)); m.Ball(e.X - s * .3f, e.Y + s * .1f, 50, s * .12f, W); break;
                    default: { var pts = new List<Vector3>(); for (int i = 0; i <= 10; i++) { float f = i / 10f; pts.Add(V3(e.X + side * f * 30 + MathF.Sin(m.Now * 10 + i) * 4, e.Y + f * c.F(p.StreamLength), 40 + f * 20)); } m.Tube(pts, s * .25f, col); for (int i = 0; i < 4; i++) { float f = Frac(m.Now * 2 + i / 4f); m.Ball(e.X + side * (30 + f * 80), e.Y + c.F(p.StreamLength) + f * 60 - 200 * f * (1 - f), 50, s * .15f, col); } } break;
                }
            }
            Eye(PctPos(m, c.F(p.LeftX), c.F(p.LeftY)), -1); if (p.BothEyes) Eye(PctPos(m, c.F(p.RightX), c.F(p.RightY)), 1);
        });
        S<BlushParameter>(d, (p, c, m) => // 照れ: ほっぺに浮かぶ平たい赤い楕円と斜線/丸、頭から出る湯気
        {
            float k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.InTime)), 0, 1)), st = c.F(p.Strength) / 100 * k, s = c.F(p.Size); var col = m.C(p.Color, st);
            foreach (var e in new[] { PctPos(m, c.F(p.LeftX), c.F(p.LeftY)), PctPos(m, c.F(p.RightX), c.F(p.RightY)) })
            {
                if (p.Style == BlushStyle.Circle) m.Disc(e.X, e.Y, 30, s * .7f, s * .7f, 6, col, .5f);
                else m.Ball(e.X, e.Y, 30, s * 1.1f, s * .6f, 6, m.C(col, p.Style == BlushStyle.Soft ? .7f : .4f));
                if (p.Style == BlushStyle.Lines) for (int i = 0; i < 3; i++) m.Limb(e.X - s * .4f + i * s * .35f, e.Y + s * .25f, 36, e.X - s * .15f + i * s * .35f, e.Y - s * .25f, 36, 4, 4, m.C(p.Color, st));
            }
            if (p.Steam) { var mid = (PctPos(m, c.F(p.LeftX), c.F(p.LeftY)) + PctPos(m, c.F(p.RightX), c.F(p.RightY))) / 2; for (int i = 0; i < 3; i++) { float f = Frac(m.Now * .8f + i / 3f); m.Ball(mid.X + (i - 1) * 90, mid.Y - 300 - f * 200, 0, 30 + 40 * f, m.C(0xFFFFFFu, .7f * (1 - f) * k)); } }
        });
        S<MenacingParameter>(d, (p, c, m) => // ゴゴゴゴ: 揺れながら奥行きの中に並ぶ厚い「ゴ」の文字と、紫のオーラ
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float aw = c.F(p.AreaWidth), ah = c.F(p.AreaHeight), fs = c.F(p.FontSize);
            if (p.Aura) for (int i = 0; i < 3; i++) m.Ball(o.X, o.Y, -200 - i * 50, aw * (.4f + i * .1f) * (1 + .05f * MathF.Sin(m.Now * 4 + i)), ah * (.4f + i * .1f), 60, m.C(p.Color, .12f));
            for (int i = 0; i < Math.Clamp(p.Count, 1, 24); i++)
            {
                float x = o.X + (R(p.Seed, i) - .5f) * aw, y = o.Y + (R(p.Seed, i, 1) - .5f) * ah, z = (R(p.Seed, i, 2) - .5f) * 300, sh = MathF.Sin(m.Now * 25 + i) * 5, sz = fs * (.7f + .6f * R(p.Seed, i, 3)), pop = Ease(Math.Clamp(m.Now * 3 - i * .15f, 0, 1));
                m.Turn(x, y, (R(p.Seed, i, 4) - .5f) * .5f, () => m.Text(p.Sound, sz * pop, x + sh, y, z, sz * .2f, m.C(p.Color), 0, m.C(p.OutlineColor), 6));
            }
        });
        S<RoomBackgroundParameter>(d, (p, c, m) => // 部屋の背景: 奥行きのある壁と床、窓、教室は黒板と机、事務所は机と棚、寝室はベッド、居間はソファ
        {
            float dim = 1 - c.F(p.Dim) / 100; Vector4 Dm(Vector4 v) => new(v.X * dim, v.Y * dim, v.Z * dim, v.W);
            var wall = Dm(m.C(p.WallColor)); var floor = Dm(m.C(p.FloorColor)); Room(m, wall, floor, 1400);
            uint sky = p.When switch { TimeOfDay.Evening => 0xFF9E5Eu, TimeOfDay.Night => 0x0D1B3Eu, _ => 0x9ED8FFu };
            float bz = -1390; m.Box(m.W * .3f, -m.H * .12f, bz, 520, 380, 4, Dm(m.C(sky)), .01f); RectBars(m, m.W * .3f, -m.H * .12f, bz + 10, 520, 380, 20, Dm(m.C(0xECEFF1u))); m.Box(m.W * .3f, -m.H * .12f, bz + 10, 14, 380, 14, Dm(m.C(0xECEFF1u)), .1f);
            switch (p.Room)
            {
                case RoomKind.Classroom: m.Box(-m.W * .15f, -m.H * .1f, bz + 10, 800, 360, 20, Dm(m.C(0x24493Au)), .02f); RectBars(m, -m.W * .15f, -m.H * .1f, bz + 22, 800, 360, 20, Dm(m.C(0x8D5A2Bu))); m.Text(p.BoardText, 60, -m.W * .15f, -m.H * .1f, bz + 30, 2, Dm(W)); for (int r = 0; r < 2; r++) for (int i = -2; i <= 2; i++) { float x = i * 330, z = -500 + r * 380; m.Box(x, m.H / 2 - 180, z, 220, 16, 150, Dm(m.C(0xD7B98Eu)), .1f); foreach (int a in Sides) foreach (int b in Sides) m.Box(x + a * 95, m.H / 2 - 90, z + b * 60, 10, 180, 10, Dm(m.C(0x9E9E9Eu))); } break;
                case RoomKind.Office: for (int i = -1; i <= 1; i++) { float x = i * 520; m.Box(x, m.H / 2 - 190, -400, 400, 20, 220, Dm(m.C(0xECEFF1u)), .05f); m.Box(x, m.H / 2 - 290, -460, 220, 140, 14, Dm(K), .1f); } m.Box(-m.W * .4f, 0, -1100, 300, m.H * .9f, 300, Dm(m.C(0x8D6E63u)), .02f); break;
                case RoomKind.Bedroom: m.Box(-m.W * .15f, m.H / 2 - 120, -700, 900, 160, 600, Dm(m.C(0xF8BBD0u)), .2f); m.Box(-m.W * .15f - 380, m.H / 2 - 260, -700, 80, 400, 640, Dm(m.C(0x8D6E63u)), .05f); m.Ball(-m.W * .15f - 250, m.H / 2 - 230, -700, 110, 50, 180, Dm(W)); break;
                default: m.Box(-m.W * .1f, m.H / 2 - 120, -900, 900, 160, 320, Dm(m.C(0x5C6BC0u)), .3f); m.Box(-m.W * .1f, m.H / 2 - 260, -1040, 900, 200, 80, Dm(m.C(0x5C6BC0u)), .3f); m.Box(-m.W * .1f, m.H / 2 - 60, -400, 500, 20, 260, Dm(m.C(0x8D6E63u)), .1f); break;
            }
            m.Box(0, -m.H / 2 + 30, -700, 300, 20, 120, Dm(m.C(0xFFFDE7u)), .3f);
        });
        S<NightViewParameter>(d, (p, c, m) => // 夜景: 奥へ何列も並ぶ窓明かりのビル、赤く光る電波塔、走る車の光、水面に映る光、ぼけた光の玉
        {
            uint top = p.Sky switch { NightSkyTone.Dusk => 0x2A1E5Cu, NightSkyTone.City => 0x1A2238u, NightSkyTone.Black => 0x000000u, _ => 0x05081Cu }, bot = p.Sky switch { NightSkyTone.Dusk => 0xC96A6Au, NightSkyTone.City => 0x4A4060u, NightSkyTone.Black => 0x050505u, _ => 0x1D2C5Au };
            SkyBands(m, m.C(top), m.C(bot), -2000, 10);
            Low(m, () => { for (int i = 0; i < Math.Min(p.Stars, 200); i++) m.Ball((R(p.Seed, i) - .5f) * m.W * 2.6f, -R(p.Seed, i, 1) * m.H * 1.2f, -1950, 3, m.C(0xFFFFFFu, .7f)); }, 5);
            if (p.Moon) m.Ball(m.W * .6f, -m.H * .6f, -1900, 90, m.C(0xFFF8E1u));
            float hy = (c.F(p.Horizon) / 100 - .5f) * m.H, bs = c.F(p.BuildingScale) / 100, lit = c.F(p.LitRatio) / 100 * (c.F(p.LightUpTime) > 0 ? Math.Clamp(m.Now / c.F(p.LightUpTime), 0, 1) : 1);
            for (int l = 2; l >= 0; l--) for (int i = 0; i < (int)(18 * c.F(p.Density) / 100); i++)
            {
                float z = -400 - l * 400, bw = (80 + 80 * R(p.Seed + l, i)) * bs, bh = (150 + 350 * R(p.Seed + l, i, 1)) * bs * (1 - l * .15f), x = (i / 18f - .5f) * m.W * (1.3f + l * .3f);
                m.Box(x, hy - bh / 2, z, bw, bh, bw * .8f, m.C(Sculpt.Mix(m.C(0x0A0C18u), m.C(bot), l * .2f), 1), .02f);
                if (l == 0) for (int wy = 0; wy < (int)(bh / 40); wy++) for (int wx = 0; wx < 3; wx++) if (R(p.Seed + i, wy * 3 + wx, 7) < lit) m.Box(x - bw * .3f + wx * bw * .3f, hy - 25 - wy * 40, z + bw * .4f + 1, 12, 16, 2, m.C(p.WindowColor), .1f);
            }
            if (p.Tower) { float tx = (c.F(p.TowerX) / 100 - .5f) * m.W; m.Lathe(tx, -600, m.C(0xC62828u), 60, hy, 30, hy - 400, 8, hy - 700, 2, hy - 780); m.Ball(tx, hy - 780, -600, 10, m.C(0xFF5252u, MathF.Sin(m.Now * 3) > 0 ? 1 : .3f)); }
            if (p.Traffic) for (int i = 0; i < 20; i++) { float x = Cyc(m.Now * 400 * (i % 2 == 0 ? 1 : -1) + R(p.Seed, i, 9) * m.W * 1.6f, m.W * 1.6f) - m.W * .8f; m.Limb(x, hy + 10, -200 + (i % 2) * 40, x + (i % 2 == 0 ? -60 : 60), hy + 10, -200 + (i % 2) * 40, 4, 2, i % 2 == 0 ? m.C(0xFFF59Du) : m.C(0xFF5252u)); }
            if (p.Water) { m.Box(0, hy + m.H * .3f, -200, m.W * 2, m.H * .6f, 10, m.C(0x0A0F24u), .01f); for (int i = 0; i < 40; i++) m.Box((R(p.Seed, i, 11) - .5f) * m.W * 1.4f + MathF.Sin(m.Now * 2 + i) * 10, hy + 40 + R(p.Seed, i, 12) * m.H * .4f, -190, 30 + 50 * R(p.Seed, i, 13), 4, 2, m.C(p.WindowColor, .5f), .2f); }
            for (int i = 0; i < (int)c.F(p.Bokeh); i++) m.Disc((R(p.Seed, i, 14) - .5f) * m.W, (R(p.Seed, i, 15) - .2f) * m.H * .6f, 200, 40 + 40 * R(p.Seed, i, 16), 40 + 40 * R(p.Seed, i, 16), 4, m.C(p.WindowColor, .2f));
        });
        S<PuddlesParameter>(d, (p, c, m) => // 水たまり: 奥へ広がる暗い地面に、ネオンの色を映す平たい水たまりと、広がる波紋
        {
            float gy = (c.F(p.GroundY) / 100 - .5f) * m.H; var lights = new[] { p.Light1, p.Light2, p.Light3 };
            if (p.PaintGround) m.Box(0, m.H / 2 + 10, -600, m.W * 3, 20, 1800, m.C(p.GroundColor), .01f);
            for (int i = 0; i < Math.Min(p.Count, 12); i++)
            {
                float x = (R(p.Seed, i) - .5f) * m.W * 1.4f, z = -1200 + R(p.Seed, i, 1) * 1100, w = 200 + 300 * R(p.Seed, i, 2);
                m.Pitch(x, m.H / 2, z, MathF.PI / 2, () =>
                {
                    m.Disc(x, m.H / 2, z, w, w * .5f, 2, m.C(0x0A0C14u), .5f); for (int k = 0; k < 3; k++) m.Disc(x + (k - 1) * w * .3f, m.H / 2, z + 1, w * .2f, w * .15f, 1, m.C(lights[(i + k) % 3], .5f), .5f);
                    float f = Frac(m.Now * c.F(p.Drips) * .5f + R(p.Seed, i, 3)); m.Torus(x + (R(p.Seed, i, 4) - .5f) * w * .5f, m.H / 2, z + 2, w * .3f * f, 2, m.C(0xFFFFFFu, .5f * (1 - f)));
                });
            }
            for (int k = 0; k < 3; k++) m.Ball((k - 1) * m.W * .35f, gy - 200, -1500, 180, 100, 20, m.C(lights[k], .25f));
        });
        S<SpaceshipWindowParameter>(d, (p, c, m) => // 宇宙船の窓: 厚い金属の壁にあいたボルト付きの丸窓、外を流れる星と浮かぶ惑星
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), sp = c.F(p.Speed) / 100;
            m.Box(0, 0, -1600, m.W * 3, m.H * 3, 10, m.C(0x02030Au), .01f);
            Low(m, () => { for (int i = 0; i < Math.Min(p.StarCount, 300); i++) { float x = Cyc((R(p.Seed, i) - .5f) * m.W * 2 - m.Now * 200 * sp * (.3f + R(p.Seed, i, 2)), m.W * 2) - m.W; m.Limb(x, (R(p.Seed, i, 1) - .5f) * m.H * 2, -1500, x + 20 * sp, (R(p.Seed, i, 1) - .5f) * m.H * 2, -1500, 3, 2, W); } }, 4);
            if (p.Planet) { float px = m.W * .6f - Cyc(m.Now * 20 * sp, m.W * 2); m.Ball(px, m.H * .2f, -1200, 500, m.C(p.PlanetColor)); m.Ball(px, m.H * .2f, -1210, 540, m.C(0x88CCFFu, .15f)); }
            if (p.Wall) { var wc = m.C(p.WallColor); var pts = new List<Vector2>(); for (int i = 0; i <= 48; i++) { float a = i * MathF.Tau / 48; pts.Add(new Vector2(o.X + MathF.Cos(a) * r, o.Y + MathF.Sin(a) * r)); } m.Box(o.X - r - m.W, 0, 0, m.W * 2, m.H * 3, 60, wc, .01f); m.Box(o.X + r + m.W, 0, 0, m.W * 2, m.H * 3, 60, wc, .01f); m.Box(o.X, o.Y - r - m.H, 0, r * 2, m.H * 2, 60, wc, .01f); m.Box(o.X, o.Y + r + m.H, 0, r * 2, m.H * 2, 60, wc, .01f); foreach (int a in Sides) foreach (int b in Sides) m.Slab(-30, 60, wc, o.X + a * r, o.Y + b * r, o.X + a * r, o.Y, o.X + a * r * .7f, o.Y + b * r * .7f, o.X, o.Y + b * r); }
            m.Torus(o.X, o.Y, 30, r, 40, m.C(0x78909Cu)); for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12; m.Ball(o.X + MathF.Cos(a) * r, o.Y + MathF.Sin(a) * r, 70, 12, m.C(0xB0BEC5u)); }
            m.Disc(o.X, o.Y, 10, r, r, 4, m.C(0xAEE3FFu, .08f)); m.Ball(o.X - r * .4f, o.Y - r * .4f, 14, r * .3f, r * .1f, 2, m.C(0xFFFFFFu, .15f));
        });
        S<TreeTunnelParameter>(d, (p, c, m) => // 並木のトンネル: 両側に奥まで並ぶ木の幹と、上でつながる花や葉の天井、舞う花びら
        {
            uint leaf = p.Season switch { TreeSeason.Autumn => 0xE65100u, TreeSeason.Green => 0x43A047u, TreeSeason.Ginkgo => 0xFBC02Du, _ => 0xF8BBD0u };
            float vy = (c.F(p.VanishY) / 100 - .5f) * m.H, sp = c.F(p.Speed) * 4, off = Cyc(m.Now * sp, 300);
            SkyBands(m, m.C(p.SkyColor), m.C(Sculpt.Mix(m.C(p.SkyColor), W, .5f), 1), -3000, 6);
            m.Box(0, m.H / 2 + 10, -1500, 700, 20, 3200, m.C(0xBCAAA4u), .01f); foreach (int s in Sides) m.Box(s * 900, m.H / 2 + 10, -1500, 1100, 20, 3200, m.C(0x7CB342u), .01f);
            for (int i = 0; i < 12; i++)
            {
                float z = 200 - i * 300 + off; foreach (int s in Sides) { float x = s * 600; m.Tube(new List<Vector3> { V3(x, m.H / 2, z), V3(x * .95f, 0, z), V3(x * .6f, -m.H * .5f, z) }, 26, m.C(0x4E342Eu)); for (int k = 0; k < 4; k++) m.Ball(x * (.4f + .2f * k), -m.H * .45f - MathF.Sin(k) * 60, z + (k - 1.5f) * 60, 200, 140, 160, m.C(Sculpt.Mix(m.C(leaf), W, R(i, k + s) * .3f), 1)); }
            }
            Low(m, () => { for (int i = 0; i < Math.Min(p.Petals, 140); i++) { float y = -m.H / 2 + Cyc(m.Now * 80 + R(p.Seed, i) * m.H, m.H), x = (R(p.Seed, i, 1) - .5f) * m.W + MathF.Sin(m.Now + i) * 40, z = -1500 + R(p.Seed, i, 2) * 1600; Petal(m, x, y, z, 14, m.Now * 2 + i, m.C(leaf)); } }, 6);
            m.Ball(0, vy, -3000, 50, m.C(0xFFFFFFu, .5f));
        });
        S<SplitScreenParameter>(d, (p, c, m) => // 画面分割: 区切りの厚い白い枠の棒が伸びて画面を分け、隅にラベル
        {
            UseFont(m, p.Font); float k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.InTime)), 0, 1)), lw = c.F(p.LineWidth) * 1.3f; var lc = m.C(p.LineColor);
            void V(float x, float y0, float y1) { float h = (y1 - y0) * k; m.Box(x, y0 + h / 2, 10, lw, h, lw * 2, lc, .2f); }
            void H(float y, float x0, float x1) { float w = (x1 - x0) * k; m.Box(x0 + w / 2, y, 10, w, lw, lw * 2, lc, .2f); }
            float hw = m.W / 2, hh = m.H / 2; var cells = new List<Vector2>();
            switch (p.Layout)
            {
                case SplitLayout.TwoRows: H(0, -hw, hw); cells.AddRange([new(-hw, -hh), new(-hw, 0)]); break;
                case SplitLayout.ThreeLeft: V(0, -hh, hh); H(0, 0, hw); cells.AddRange([new(-hw, -hh), new(0, -hh), new(0, 0)]); break;
                case SplitLayout.Three: V(-m.W / 6, -hh, hh); V(m.W / 6, -hh, hh); cells.AddRange([new(-hw, -hh), new(-m.W / 6, -hh), new(m.W / 6, -hh)]); break;
                case SplitLayout.Four: V(0, -hh, hh); H(0, -hw, hw); cells.AddRange([new(-hw, -hh), new(0, -hh), new(-hw, 0), new(0, 0)]); break;
                case SplitLayout.Diagonal: m.Turn(0, 0, MathF.Atan2(m.H, m.W) * 0 + -1.1f, () => m.Box(0, 0, 10, lw, MathF.Sqrt(m.W * m.W + m.H * m.H) * k, lw * 2, lc, .2f)); cells.AddRange([new(-hw, -hh), new(m.W * .15f, m.H * .1f)]); break;
                default: V(0, -hh, hh); cells.AddRange([new(-hw, -hh), new(0, -hh)]); break;
            }
            if (p.OuterFrame) RectBars(m, 0, 0, 10, m.W - lw, m.H - lw, lw, lc);
            var labs = p.Labels.Split(',', '、', '\n').Select(s => s.Trim()).ToList();
            for (int i = 0; i < cells.Count && i < labs.Count; i++) { if (labs[i].Length == 0) continue; float w = labs[i].Length * 44 + 40; m.Box(cells[i].X + w / 2 + 30, cells[i].Y + 60, 20, w, 64, 20, m.C(p.LabelColor), .3f); m.Text(labs[i], 40, cells[i].X + w / 2 + 30, cells[i].Y + 60, 32, 2, W); }
        });
        S<CinemascopeParameter>(d, (p, c, m) => // シネマスコープ: 上下から閉じてくる厚い黒い帯
        {
            float ratio = p.Ratio switch { LetterboxRatio.Univisium => 2f, LetterboxRatio.Vista => 1.85f, LetterboxRatio.Classic => 1.33f, LetterboxRatio.Custom => 0, _ => 2.39f };
            float bh = ratio > 0 ? MathF.Max(0, (m.H - m.W / ratio) / 2) : m.H * c.F(p.CustomSize) / 100; if (ratio == 1.33f) { float bw = (m.W - m.H * 1.33f) / 2; float kk = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)); foreach (int s in Sides) m.Box(s * (m.W / 2 - bw * kk / 2), 0, 20, bw * kk, m.H, 40, m.C(p.Color), .02f); return; }
            float k = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)), h = bh * k + c.F(p.Feather);
            foreach (int s in Sides) m.Box(0, s * (m.H / 2 - h / 2), 20, m.W * 1.05f, h, 40, m.C(p.Color), .02f);
        });
        S<ImpactFlashParameter>(d, (p, c, m) => // 衝撃のフラッシュ: 一瞬光る白い板、中心から走る集中線、赤と青にずれた光の輪
        {
            var times = p.Times.Split(',', '、').Select(s => F(s.Trim(), -1)).Where(t => t >= 0).ToList(); float dec = MathF.Max(.05f, c.F(p.Decay)), it = c.F(p.Intensity) / 100; var o = PctPos(m, c.F(p.CenterX), c.F(p.CenterY));
            foreach (var t0 in times)
            {
                float a = m.Now - t0; if (a < 0 || a > dec * 3) continue; float f = MathF.Exp(-a / dec);
                m.Box(0, 0, 50, m.W * 2, m.H * 2, 4, m.C(p.Color, f * it), .01f);
                if (p.Lines) for (int i = 0; i < 40; i++) { float an = R(p.Seed, i) * MathF.Tau, r0 = 150 + R(p.Seed, i, 1) * 200 + a * 2000; m.Limb(o.X + MathF.Cos(an) * r0, o.Y + MathF.Sin(an) * r0, 60, o.X + MathF.Cos(an) * m.W, o.Y + MathF.Sin(an) * m.W, 60, 1, 8, m.C(p.Color, f)); }
                float fr = c.F(p.Fringe) / 100; m.Torus(o.X - 8, o.Y, 40, 200 + a * 2400, 20, m.C(0xFF3B30u, f * fr * .6f)); m.Torus(o.X + 8, o.Y, 40, 210 + a * 2400, 20, m.C(0x3BB2FFu, f * fr * .6f));
            }
        });
        S<KaleidoscopeParameter>(d, (p, c, m) => // 万華鏡: 鏡のように何回も折り返して並ぶ、色ガラスの厚い破片が回り形を変える
        {
            var o = PctPos(m, c.F(p.CenterX), c.F(p.CenterY)); int seg = Math.Clamp(p.Segments, 3, 16), n = Math.Clamp(p.Shapes, 1, 30); float spin = m.Now * c.F(p.Spin) * MathF.PI / 180, mo = c.F(p.Morph) / 100;
            m.Box(0, 0, -200, m.W * 2, m.H * 2, 10, m.C(p.BackColor), .01f);
            m.Turn(o.X, o.Y, spin, () =>
            {
                for (int sgi = 0; sgi < seg; sgi++)
                {
                    float ba = sgi * MathF.Tau / seg; bool flip = sgi % 2 == 1;
                    for (int i = 0; i < n; i++)
                    {
                        float rr = 80 + 420 * R(p.Seed, i) + MathF.Sin(m.Now * .8f * mo + i) * 60 * mo, aa = (R(p.Seed, i, 1) * .9f + .05f) * MathF.Tau / seg, a = ba + (flip ? MathF.Tau / seg - aa : aa), x = o.X + MathF.Cos(a) * rr, y = o.Y + MathF.Sin(a) * rr, s = 20 + 50 * R(p.Seed, i, 2), z = (R(p.Seed, i, 3) - .5f) * 120;
                        var col = m.C(ChartCol(m, p.Palette, i), p.Additive ? .65f : 1);
                        m.Turn(x, y, a + m.Now * mo, () => { if (i % 3 == 0) m.Slab(z - 8, 16, col, x - s, y, x + s, y - s * .5f, x + s * .4f, y + s * .6f); else if (i % 3 == 1) m.Ball(x, y, z, s * .6f, s * .6f, s * .3f, col); else m.Box(x, y, z, s * 1.2f, s * .5f, s * .3f, col, .2f); });
                    }
                }
            });
        });
    }
}
