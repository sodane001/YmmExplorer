using System.Globalization;
using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 191番(小物 13)・192番(解説の図2 12)・193番(ゲーム2 10)・
//               194番(SNS・配信 11)・195番(キャラの気持ち2 10)・196番(自然2 10)
// =====================================================================
internal static partial class Models3D
{
    static float F(string? s, float def = 0) => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f) ? f : def;

    static partial void RegisterH1d(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.12f, .13f, .16f, 1);
        // ---------------------------------------------------------------- 191 小物
        Q(d, typeof(GlobeSpinParameter), (p, m, t, v, v2) => // 地球儀: 台と金の半円の軸受け、回る海の球と陸地
        {
            float R = 260;
            SquareFrustum(m, 0, 0, R + 140, 60, R + 170, 140, m.C(p.Color3));
            m.Cyl(0, R + 80, 0, 14, 120, m.C(p.Color3), .2f);
            m.Turn(0, 0, -.4f, () =>
            {
                m.Tube(ArcPts3(0, 0, R + 30, MathF.PI * .5f, MathF.PI * 1.5f, 0, 24), 8, m.C(p.Color3));
                m.Yaw(0, 0, 0, t * .5f, () =>
                {
                    m.Ball(0, 0, 0, R, m.C(p.Color1));
                    for (int i = 0; i < 7; i++) { float lo = i * .9f, la = (DrawKit.H(p.Seed, i) - .5f) * 2; var c = new Vector3(MathF.Cos(la) * MathF.Sin(lo), MathF.Sin(la), MathF.Cos(la) * MathF.Cos(lo)) * (R - 20); m.Ball(c.X, c.Y, c.Z, 70, 50, 60, m.C(p.Color2)); }
                });
            });
        });
        Q(d, typeof(GearSetParameter), (p, m, t, v, v2) => // 歯車: かみ合って逆向きに回る、厚い歯車の列
        {
            int n = Math.Clamp(p.Count, 1, 6); float x0 = -(n - 1) * 140;
            for (int i = 0; i < n; i++)
            {
                float x = x0 + i * 280, y = i % 2 == 0 ? 0 : 40, r = 120, rot = m.Now * (i % 2 == 0 ? 1 : -1) + (i % 2) * .26f;
                var col = i % 2 == 0 ? m.C(p.Color1) : m.C(p.Color2);
                m.Turn(x, y, rot, () => { for (int k = 0; k < 12; k++) m.Turn(x, y, k * MathF.Tau / 12, () => m.Box(x, y - r - 10, 0, 34, 40, 60, col, .2f)); m.Disc(x, y, 0, r, r, 60, col, .2f); m.Disc(x, y, 0, 30, 30, 66, Sculpt.Dark(col, .6f), .3f); });
            }
        });
        Q(d, typeof(NotifyBadgeParameter), (p, m, t, v, v2) => // 通知バッジ: アプリの四角い札と、ぽんと出る赤い玉の数字
        {
            m.Box(0, 0, 0, 260, 260, 60, m.C(0x2F86C6), .3f);
            Gem(m, Pts(0, -60, 50, 30, -50, 30), 0, 0, 0f, 4, 24, W, W);
            int n = Math.Max(1, (int)MathF.Round(p.Count * Ease(In(m, .1f, .2f + .25f * Math.Max(1, p.Count))))); float k = Ease(In(m, .1f, .3f));
            m.Ball(120, -120, 50, 70 * k, 70 * k, 40, m.C(p.Color1)); m.Text(n.ToString(), 70 * k, 120, -120, 90, 6, W);
        });
        Q(d, typeof(TimeBombParameter), (p, m, t, v, v2) => // 時限爆弾: 黒い球、導火線と火花、残り秒数の表示(0で爆発)
        {
            float left = p.Count - m.Now;
            if (left <= 0) { float f = Math.Clamp(-left, 0, 1); for (int i = 0; i < 14; i++) { float ag = i * MathF.Tau / 14; m.Ball(MathF.Cos(ag) * 400 * f, MathF.Sin(ag) * 400 * f, MathF.Sin(ag * 3) * 200 * f, 80 * (1 - f) + 10, Vector4.Lerp(m.C(0xFFEB3B), m.C(p.Color2), f) with { W = (1 - f) * m.Al }); } return; }
            m.Ball(0, 40, 0, 200, m.C(p.Color1));
            m.Cyl(90, -150, 0, 40, 50, m.C(0x555555), .3f);
            m.Tube([new(90, -175, 0), new(140, -260, 0), new(220, -250, 0)], 5, m.C(0xC8A070));
            m.Ball(220, -250, 0, 20 + 8 * MathF.Sin(m.Now * 20), m.C(p.Color2));
            m.Box(0, 60, 205, 220, 90, 10, m.C(0x111111), .2f); m.Text(MathF.Ceiling(left).ToString("00"), 70, 0, 60, 212, 4, m.C(0xFF3030));
        });
        Q(d, typeof(TrafficSignalParameter), (p, m, t, v, v2) => // 信号機: 縦長の箱、ひさしの付いた3つのランプ(順に点く)
        {
            int state = v <= .001f ? (int)(((m.Now * .3f) % 3 + 3) % 3) : Math.Clamp((int)(v * 3), 0, 2);
            uint[] cols = [0x35D06F, 0xFFD23F, 0xFF4D4D];
            m.Box(0, 0, 0, 220, 620, 140, m.C(p.Color1), .3f);
            m.Cyl(0, 560, -40, 16, 500, m.C(p.Color1), .2f);
            for (int i = 0; i < 3; i++) { float y = -190 + i * 190; m.Disc(0, y, 70, 76, 76, 20, i == state ? m.C(cols[i]) : Sculpt.Dark(m.C(cols[i]), .3f), .5f); m.Super(0, y - 78, 105, 88, 10, 40, .3f, 1, m.C(p.Color1)); }
        });
        Q(d, typeof(EnvelopeLetterParameter), (p, m, t, v, v2) => // 封筒と手紙: 開くふた、出てくる手紙とハートのシール
        {
            float open = Ease(In(m, 0, .6f)), up = Ease(In(m, .6f, .6f)), ew = 600, eh = 380;
            m.Box(0, 0, 0, 560, 340, 6, m.C(0xFFFFFF), .05f);
            m.Move(0, -up * 220, () => { m.Box(0, -40, 4, 520, 300, 4, W, .05f); m.Text(Str(p), 40, 0, -60, 8, 3, K, 460); });
            m.Box(0, 0, 10, ew, eh, 20, m.C(p.Color1), .05f);
            m.Pitch(0, -eh / 2, 20, -MathF.PI * open, () => m.Tri3D(new(-ew / 2, -eh / 2, 21), new(ew / 2, -eh / 2, 21), new(0, 40, 21), Sculpt.Dark(m.C(p.Color1), .92f)));
            if (open < .5f) Heart3D(m, 0, 30, 30, 50, m.C(p.Color2));
        });
        Q(d, typeof(PodiumParameter), (p, m, t, v, v2) => // 表彰台: 高さの違う3つの台と順位、名前
        {
            var it = p.ItemsForModel; float bw = 300; int[] order = [1, 0, 2]; float[] hh = [320, 220, 160];
            var cols = new[] { p.Color1, p.Color2, p.Color3 };
            for (int k = 0; k < 3; k++)
            {
                int rank = order[k]; float x = (k - 1) * bw, h0 = hh[rank] * Ease(In(m, .2f + (2 - rank) * .2f, .4f));
                m.Box(x, 300 - h0 / 2, 0, bw - 10, h0, 260, m.C(cols[rank]), .05f);
                m.Text((rank + 1).ToString(), 100, x, 300 - h0 / 2, 132, 10, W);
                if (rank < it.Count) m.Text(it[rank], 44, x, 300 - h0 - 60, 0, 6, W);
            }
        });
        Q(d, typeof(PenSignatureParameter), (p, m, t, v, v2) => // ペンでサイン: 書かれていく字の線と、動く立体のペン
        {
            float k = In(m, .2f, .6f + Str(p).Length * .15f), tw = Str(p).Length * 120;
            m.Text(Str(p), 160, 0, 0, 0, 4, m.C(p.Color1, k));
            float px = -tw / 2 + tw * k, py = MathF.Sin(m.Now * 20) * 20;
            m.Turn(px, py, .6f, () => { m.Limb(px, py, 30, px + 40, py - 260, 60, 4, 16, m.C(0x37474F)); m.Limb(px + 40, py - 260, 60, px + 50, py - 300, 62, 16, 16, m.C(0xC9A227)); });
        });
        Q(d, typeof(LockOpenParameter), (p, m, t, v, v2) => // 南京錠: 鍵がささって回り、つるがはね上がる
        {
            float ins = Ease(In(m, 0, .4f)), turn = Ease(In(m, .4f, .3f)), pop = Ease(In(m, .7f, .3f));
            var g = m.C(p.Color2);
            m.Move(0, -pop * 60, () => { var arc = ArcPts3(0, -60, 120, MathF.PI, MathF.Tau, 0, 16); arc.Insert(0, new(-120, 20, 0)); arc.Add(new(120, 20, 0)); m.Tube(arc, 22, g); });
            m.Box(0, 120, 0, 390, 300, 120, m.C(p.Color1), .2f);
            m.Move(0, 0, 60 + (1 - ins) * 200, () => m.Turn(0, 110, turn * MathF.PI / 2, () => { m.Limb(0, 110, 0, 0, 110, 80, 10, 10, g); m.Disc(0, 110, 100, 40, 40, 14, g, .3f); }));
        });
        Q(d, typeof(GiftBoxParameter), (p, m, t, v, v2) => // プレゼント: ゆれる箱、リボン、はね上がるふた
        {
            float sh = m.Now < 1 ? MathF.Sin(m.Now * 40) * 8 * MathF.Min(1, m.Now) : 0, open = Ease(In(m, 1, .4f));
            m.Move(sh, 0, () =>
            {
                m.Box(0, 100, 0, 400, 300, 400, m.C(p.Color1), .05f);
                m.Box(0, 100, 0, 60, 302, 402, m.C(p.Color2), .1f); m.Box(0, 100, 0, 402, 302, 60, m.C(p.Color2), .1f);
                m.Move(0, -open * 300, () => m.Turn(0, -60, open * .6f, () =>
                {
                    m.Box(0, -70, 0, 440, 80, 440, m.C(p.Color1), .1f);
                    m.Box(0, -70, 0, 64, 82, 442, m.C(p.Color2), .1f);
                    foreach (int s in Sides) m.Ball(s * 50, -130, 0, 60, 34, 30, m.C(p.Color2));
                }));
            });
            if (open > 0) for (int i = 0; i < 10; i++) { float ag = i * MathF.Tau / 10; Gem(m, Pts(StarPts(MathF.Cos(ag) * 300 * open, -100 + MathF.Sin(ag) * 200 * open, 20, 5, .45f, 0)), MathF.Cos(ag) * 300 * open, -100 + MathF.Sin(ag) * 200 * open, 0f, 2, 8, m.C(0xFFD54A), m.C(0xFFD54A)); }
        });
        Q(d, typeof(HankoStampParameter), (p, m, t, v, v2) => // ハンコ: 下りて押される円柱の判と、残る赤い丸い印
        {
            float down = In(m, 0, .25f), up = In(m, .4f, .3f), y = -300 + 260 * Ease(down) - 260 * Ease(up);
            if (down >= 1) { m.Torus(0, 0, 2, 120, 8, m.C(p.Color1)); m.Text(Str(p), 90, 0, 0, 4, 2, m.C(p.Color1)); }
            m.Pitch(0, y, 0, MathF.PI / 2, () => { m.Cyl(0, y - 140, 0, 125, 280, m.C(0xC9A06A), .1f); m.Cyl(0, y - 300, 0, 60, 60, m.C(0x8D6E63), .3f); });
        });
        Q(d, typeof(ScaleNeedleParameter), (p, m, t, v, v2) => // 体重計: 厚い本体と目盛り盤、ゆれて止まる赤い針
        {
            float R = 300;
            m.Box(0, 60, 0, 760, 820, 140, m.C(p.Color1), .25f);
            m.Disc(0, -40, 72, R, R, 10, W, .2f);
            for (int i = 0; i <= 20; i++) { float ag = MathF.PI * .75f + MathF.PI * 1.5f * i / 20; m.Limb(MathF.Cos(ag) * R * .85f, -40 + MathF.Sin(ag) * R * .85f, 80, MathF.Cos(ag) * R * .95f, -40 + MathF.Sin(ag) * R * .95f, 80, 2, 2, K); }
            float st = In(m, 0, 1.5f), val = v * (1 + MathF.Sin(st * 14) * (1 - st) * .2f), na = MathF.PI * .75f + MathF.PI * 1.5f * val;
            m.Limb(0, -40, 86, MathF.Cos(na) * R * .8f, -40 + MathF.Sin(na) * R * .8f, 86, 5, 2, m.C(p.Color2)); m.Ball(0, -40, 90, 20, m.C(p.Color2));
            m.Text(Str(p), 60, 0, 320, 72, 6, K);
        });
        Q(d, typeof(CandleFlameParameter), (p, m, t, v, v2) => // ろうそく: 並んだ円柱のろうそくと、ゆらめく炎と光
        {
            int n = Math.Clamp(p.Count, 1, 9);
            for (int i = 0; i < n; i++)
            {
                float x = (i - (n - 1) / 2f) * 160, hh = 300 + 60 * DrawKit.H(p.Seed, i), fl = MathF.Sin(m.Now * 9 + i * 2) * .15f;
                m.Cyl(x, 300 - hh / 2, 0, 40, hh, m.C(p.Color1), .2f);
                m.Limb(x, 300 - hh, 0, x, 300 - hh - 20, 0, 3, 3, K);
                m.Turn(x, 300 - hh - 20, fl, () => { m.Ball(x, 300 - hh - 60, 0, 22, 45, 22, m.C(p.Color2)); m.Ball(x, 300 - hh - 50, 0, 10, 22, 10, m.C(0xFFFFFF)); });
                m.Ball(x, 300 - hh - 60, 0, 90, m.C(p.Color2, .12f));
            }
        });

        // ---------------------------------------------------------------- 192 解説の図2
        Q(d, typeof(RadarChartParameter), (p, m, t, v, v2) => // レーダーチャート: 網の目盛りと、値で盛り上がる多角すい
        {
            var rows = Lines(p).Select(r => { int ci = r.LastIndexOfAny([':', '：']); return (n: ci > 0 ? r[..ci] : r, v: ci > 0 ? F(r[(ci + 1)..].Trim()) : 0); }).ToList();
            int n = Math.Max(3, rows.Count); float R = 300, mx = MathF.Max(1, rows.Count > 0 ? rows.Max(r => r.v) : 1), k = Ease(In(m, .2f, 1));
            for (int ring = 1; ring <= 4; ring++) m.Tube([.. Enumerable.Range(0, n + 1).Select(i => { float ag = -MathF.PI / 2 + i * MathF.Tau / n; return new Vector3(MathF.Cos(ag) * R * ring / 4, MathF.Sin(ag) * R * ring / 4, 0); })], 1.5f, m.C(0xFFFFFF, .4f));
            var pts = new List<Vector3>();
            for (int i = 0; i < n; i++) { float ag = -MathF.PI / 2 + i * MathF.Tau / n, f = i < rows.Count ? rows[i].v / mx * k : 0; pts.Add(new(MathF.Cos(ag) * R * f, MathF.Sin(ag) * R * f, 4)); if (i < rows.Count) m.Text(rows[i].n, 36, MathF.Cos(ag) * (R + 60), MathF.Sin(ag) * (R + 60), 4, 4, W); }
            for (int i = 0; i < n; i++) m.Tri3D(new(0, 0, 80 * k), pts[i], pts[(i + 1) % n], m.C(p.Color1, .75f));
            pts.Add(pts[0]); m.Tube(pts, 4, m.C(p.Color2));
        });
        Q(d, typeof(NumberLineParameter), (p, m, t, v, v2) => // 数直線と範囲: 管の直線と目盛り、色の範囲の帯、端の玉
        {
            var it = p.ItemsForModel; float mn = it.Count > 0 ? F(it[0]) : 0, mx = it.Count > 1 ? F(it[1], 10) : 10, a0 = it.Count > 2 ? F(it[2], mn) : mn, a1 = it.Count > 3 ? F(it[3], mx) : mx;
            const float L = 1200;
            float X(float f) => -L / 2 + L * (f - mn) / MathF.Max(1e-3f, mx - mn);
            m.Limb(-L / 2 - 40, 0, 0, L / 2 + 20, 0, 0, 4, 4, W); m.Limb(L / 2 + 20, 0, 0, L / 2 + 60, 0, 0, 18, 1, W);
            for (int i = 0; i <= 10; i++) { float x = -L / 2 + L * i / 10; m.Limb(x, -18, 0, x, 18, 0, 2, 2, W); m.Text((mn + (mx - mn) * i / 10).ToString("0.#", CultureInfo.InvariantCulture), 30, x, 50, 0, 3, W); }
            float k = Ease(In(m, .3f, .8f)), x0 = X(a0), x1 = x0 + (X(a1) - x0) * k;
            m.Box((x0 + x1) / 2, -40, 0, MathF.Abs(x1 - x0), 24, 30, m.C(p.Color1, .85f), .4f);
            m.Ball(x0, -40, 0, 18, m.C(p.Color2)); m.Ball(x1, -40, 0, 18, m.C(p.Color2));
        });
        Q(d, typeof(FractionPizzaParameter), (p, m, t, v, v2) => // ピザ: 厚い生地のピースが並び、割合ぶんだけ具が乗る(残りは離れる)
        {
            int n = Math.Clamp(p.Count, 2, 24), on = Math.Clamp((int)MathF.Round(v * n), 0, n); float R = 300;
            for (int i = 0; i < n; i++)
            {
                float a0 = -MathF.PI / 2 + i * MathF.Tau / n, a1 = a0 + MathF.Tau / n, mid = (a0 + a1) / 2, off = i < on ? 0 : 30 * Ease(In(m, .4f, .4f));
                float ox = MathF.Cos(mid) * off, oy = MathF.Sin(mid) * off;
                m.Move(ox, oy, () =>
                {
                    m.Slab(0, 40, m.C(0xE0A060), PiePts(0, 0, R, a0 + .01f, a1 - .01f, 6));
                    if (i < on) { m.Slab(22, 6, m.C(p.Color1), PiePts(0, 0, R * .9f, a0 + .03f, a1 - .03f, 6)); for (int k = 0; k < 2; k++) { float r0 = R * (.35f + .3f * k); m.Disc(MathF.Cos(mid) * r0, MathF.Sin(mid) * r0, 28, 26, 26, 8, m.C(p.Color2), .5f); } }
                });
            }
            m.Text($"{on}/{n}", 80, 0, R + 80, 4, 8, W);
        });
        Q(d, typeof(FunctionGraphParameter), (p, m, t, v, v2) => // 関数のグラフ: 軸と目盛り、描かれていく曲線の管
        {
            float W2 = 700, H2 = 420; int range = Math.Clamp(p.Count, 1, 100); float S = W2 / range;
            m.Limb(-W2, 0, 0, W2, 0, 0, 3, 3, W); m.Limb(0, H2, 0, 0, -H2, 0, 3, 3, W);
            m.Limb(W2, 0, 0, W2 + 30, 0, 0, 14, 1, W); m.Limb(0, -H2, 0, 0, -H2 - 30, 0, 14, 1, W);
            for (int i = -range; i <= range; i++) m.Limb(i * S, -8, 0, i * S, 8, 0, 1.5f, 1.5f, W);
            float k = In(m, .2f, 1.5f); var pts = new List<Vector3>();
            for (int i = 0; i <= 80 * k; i++) { float x = -range + 2f * range * i / 80, y = MathF.Sin(x * (.5f + v * 2)) * range * .7f; if (MathF.Abs(y * S) < H2) pts.Add(new(x * S, -y * S, 10)); }
            if (pts.Count > 1) m.Tube(pts, 5, m.C(p.Color1));
            m.Text(Str(p), 44, W2 - 140, -H2 + 40, 4, 4, m.C(p.Color1));
        });
        Q(d, typeof(CauseChainParameter), (p, m, t, v, v2) => // 因果のチェーン: 順に出る厚い箱と、つなぐ立体の矢印
        {
            var it = Lines(p); if (it.Count == 0) return; float gap = 380, x0 = -(it.Count - 1) * gap / 2;
            for (int i = 0; i < it.Count; i++)
            {
                float k = Ease(In(m, i * .4f, .35f)); if (k <= 0) continue; float x = x0 + i * gap;
                m.Box(x, 0, 0, 280 * k, 140 * k, 50, i == it.Count - 1 ? m.C(p.Color2) : m.C(p.Color1), .3f);
                m.Text(it[i], 40 * k, x, 0, 27, 4, W, 260);
                if (i > 0) Arr3(m, x - gap + 150, 0, x - 150, 0, 0, W);
            }
        });
        Q(d, typeof(ProsConsParameter), (p, m, t, v, v2) => // メリット・デメリット: 色の見出しの付いた2枚の厚い板と項目
        {
            var rows = Lines(p); var pros = rows.Where(r => r.StartsWith('+') || r.StartsWith('＋')).Select(r => r[1..].Trim()).ToList(); var cons = rows.Where(r => r.StartsWith('-') || r.StartsWith('－')).Select(r => r[1..].Trim()).ToList();
            foreach (int s in Sides)
            {
                var list = s < 0 ? pros : cons; float x = s * 330; var col = s < 0 ? m.C(p.Color1) : m.C(p.Color2);
                m.Box(x, 40, 0, 620, 560, 30, m.C(0xFFFFFF, .95f), .05f);
                m.Box(x, -220, 16, 620, 90, 20, col, .1f); m.Text(s < 0 ? "メリット" : "デメリット", 50, x, -220, 27, 4, W);
                for (int i = 0; i < list.Count; i++) { float k = In(m, .3f + i * .3f, .3f); if (k <= 0) continue; m.Ball(x - 260, -120 + i * 80, 20, 14, col); m.Text(list[i], 36, x + 20, -120 + i * 80, 17, 3, K, 480); }
            }
        });
        Q(d, typeof(StepStairsParameter), (p, m, t, v, v2) => // ステップの階段: 上っていく奥行きのある段と、各段の文字
        {
            var it = Lines(p); int n = it.Count; if (n == 0) return; float sw = 1200f / n, sh = 500f / n;
            for (int i = 0; i < n; i++)
            {
                float k = Ease(In(m, i * .3f, .4f)); if (k <= 0) continue;
                float x = -600 + sw * (i + .5f), hh = sh * (i + 1) * k;
                m.Box(x, 250 - hh / 2, 0, sw - 6, hh, 200, Mix(m, p.Color1, p.Color2, i / (float)Math.Max(1, n - 1)), .03f);
                m.Text(it[i], 36, x, 250 - hh + 40, 102, 4, W, sw - 20);
            }
        });
        Q(d, typeof(JigsawPuzzleParameter), (p, m, t, v, v2) => // ジグソーパズル: 飛んできてはまる、厚いピース
        {
            int cols = Math.Clamp(p.Count, 2, 8), rows = Math.Max(2, (int)MathF.Round(cols * .6f)); float cw = 1000f / cols, ch = 600f / rows;
            for (int y = 0; y < rows; y++) for (int x = 0; x < cols; x++)
                {
                    int i = y * cols + x; float k = Ease(In(m, i * .08f, .5f)), px = -500 + cw * (x + .5f), py = -300 + ch * (y + .5f);
                    float fx = px + (DrawKit.H(p.Seed, i) - .5f) * 800 * (1 - k), fy = py + (DrawKit.H(p.Seed, i, 1) - .5f) * 600 * (1 - k), fz = 400 * (1 - k);
                    m.Move(fx - px, fy - py, fz, () => { m.Box(px, py, 0, cw - 4, ch - 4, 30, Mix(m, p.Color1, p.Color2, (x + y) / (float)(cols + rows)), .1f); m.Ball(px + cw / 2, py, 0, cw * .15f, cw * .15f, 15, Mix(m, p.Color1, p.Color2, (x + y) / (float)(cols + rows))); });
                }
        });
        Q(d, typeof(PopulationPyramidParameter), (p, m, t, v, v2) => // 人口ピラミッド: 左右に伸びる立体の棒(男女)と年齢
        {
            var rows = Rows(p); if (rows.Count == 0) return; float mx = MathF.Max(1, rows.Max(r => MathF.Max(r.v.ElementAtOrDefault(0), r.v.ElementAtOrDefault(1)))), rh = 600f / rows.Count;
            for (int i = 0; i < rows.Count; i++)
            {
                float k = Ease(In(m, .1f + i * .05f, .6f)), y = 300 - rh * (i + .5f);
                float l0 = 500 * rows[i].v.ElementAtOrDefault(0) / mx * k, l1 = 500 * rows[i].v.ElementAtOrDefault(1) / mx * k;
                if (l0 > 1) m.Box(-60 - l0 / 2, y, 0, l0, rh - 6, 40, m.C(p.Color1), .2f);
                if (l1 > 1) m.Box(60 + l1 / 2, y, 0, l1, rh - 6, 40, m.C(p.Color2), .2f);
                m.Text(rows[i].n, MathF.Min(30, rh * .6f), 0, y, 4, 3, W);
            }
        });
        Q(d, typeof(UnitConverterParameter), (p, m, t, v, v2) => // 単位の換算: 2つの丸いメーターと、回る針、間の矢印
        {
            var it = p.ItemsForModel; string u1 = it.Count > 0 ? it[0] : "", u2 = it.Count > 1 ? it[1] : "";
            float k = Ease(In(m, .2f, 1.2f));
            foreach (int s in Sides)
            {
                float x = s * 330, R = 220, na = MathF.PI * .75f + MathF.PI * 1.5f * v * k;
                m.Disc(x, 0, 0, R, R, 40, s < 0 ? m.C(p.Color1) : m.C(p.Color2), .3f);
                m.Disc(x, 0, 18, R * .85f, R * .85f, 10, W, .2f);
                m.Limb(x, 0, 26, x + MathF.Cos(na) * R * .7f, MathF.Sin(na) * R * .7f, 26, 5, 2, K); m.Ball(x, 0, 28, 14, K);
                m.Text(s < 0 ? u1 : u2, 44, x, R * .45f, 26, 4, K);
            }
            Arr3(m, -80, 0, 80, 0, 0, W);
        });
        Q(d, typeof(GlossaryCardParameter), (p, m, t, v, v2) => // 用語解説カード: 厚いカード、色の見出しの帯、読みと説明
        {
            var ln = Str(p).Split('\n'); float k = Ease(In(m, 0, .35f));
            m.Box(0, 0, 0, 1000 * k, 560 * k, 30, m.C(p.Color2), .06f);
            m.Box(0, -200 * k, 16, 1000 * k, 140 * k, 8, m.C(p.Color1), .06f);
            m.Text(ln.ElementAtOrDefault(0) ?? "", 70 * k, 0, -210 * k, 22, 6, W);
            m.Text(ln.ElementAtOrDefault(1) ?? "", 34 * k, 0, -150 * k, 22, 3, m.C(0xFFFFFF, .8f));
            m.Text(string.Join(" ", ln.Skip(2)), 40 * k, 0, 80 * k, 17, 3, K, 900);
        });
        Q(d, typeof(FaqQAParameter), (p, m, t, v, v2) => // Q&A: 「Q」「A」の丸い札と、2つの吹き出しの板
        {
            var ln = Str(p).Split('\n'); string qs = ln.ElementAtOrDefault(0) ?? "", ans = string.Join(" ", ln.Skip(1));
            m.Disc(-480, -120, 0, 70, 70, 40, m.C(p.Color1), .4f); m.Text("Q", 80, -480, -120, 22, 6, W);
            m.Box(40, -120, 0, 900, 140, 30, m.C(0xFFFFFF, .95f), .3f); m.Text(qs, 44, 40, -120, 17, 4, K, 860);
            float k = In(m, .6f, .3f); if (k <= 0) return;
            m.Disc(-480, 120, 0, 70, 70, 40, m.C(p.Color2), .4f); m.Text("A", 80, -480, 120, 22, 6, W);
            m.Box(40, 120, 0, 900 * k, 160, 30, m.C(0xFFFFFF, .95f), .3f); m.Text(ans, 40, 40, 120, 17, 4, K, 860);
        });

        // ---------------------------------------------------------------- 193 ゲーム2
        Q(d, typeof(TreasureMapParameter), (p, m, t, v, v2) => // 宝の地図: 丸まったふちの紙、島、点線の道と赤い×
        {
            float mw = 1300, mh = 780;
            m.Box(0, 0, 0, mw, mh, 8, m.C(p.Color1), .02f);
            foreach (int s in Sides) m.Turn(s * mw / 2, 0, 0, () => m.Cyl(s * mw / 2, 0, 20, 30, mh, Sculpt.Dark(m.C(p.Color1), .9f), .1f));
            m.Ball(-200, 50, 4, 300, 200, 20, m.C(0xB8D88A)); m.Ball(300, -100, 4, 180, 130, 16, m.C(0xB8D88A));
            var path = Enumerable.Range(0, 20).Select(i => { float f = i / 19f; return new Vector2(-450 + 750 * f, 200 - 300 * f + MathF.Sin(f * 8) * 60); }).ToList();
            float k = In(m, .3f, 1.5f);
            for (int i = 0; i < path.Count * k; i += 1) m.Ball(path[i].X, path[i].Y, 12, 8, m.C(p.Color2));
            if (k >= 1) { var e = path[^1]; m.Limb(e.X - 40, e.Y - 40, 14, e.X + 40, e.Y + 40, 14, 8, 8, m.C(p.Color2)); m.Limb(e.X + 40, e.Y - 40, 14, e.X - 40, e.Y + 40, 14, 8, 8, m.C(p.Color2)); }
        });
        Q(d, typeof(QuestBannerParameter), (p, m, t, v, v2) => // クエストの帯: 飛びこむ厚い帯と金のふち、大きな文字
        {
            var ln = Str(p).Split('\n'); float k = Ease(In(m, 0, .35f));
            m.Box(0, 0, 0, 1400 * k, 220, 50, m.C(p.Color1), .05f);
            m.Box(0, -118, 10, 1400 * k, 14, 60, m.C(p.Color2), .3f); m.Box(0, 118, 10, 1400 * k, 14, 60, m.C(p.Color2), .3f);
            m.Text(ln.ElementAtOrDefault(0) ?? "", 110 * k, 0, -20, 30, 14, m.C(p.Color2));
            m.Text(ln.ElementAtOrDefault(1) ?? "", 44 * k, 0, 70, 27, 4, W);
        });
        Q(d, typeof(SkillTreeParameter), (p, m, t, v, v2) => // スキルツリー: 下から上へ枝分かれする玉と管(解放ぶんだけ光る)
        {
            int L = Math.Clamp(p.Count, 2, 7); var nodes = new List<(Vector3 q, int parent)> { (new(0, 300, 0), -1) };
            for (int l = 1; l < L; l++) { int cnt = Math.Min(1 << l, 8); for (int i = 0; i < cnt; i++) nodes.Add((new((i - (cnt - 1) / 2f) * 900f / cnt, 300 - l * 600f / (L - 1), (i % 2 == 0 ? 1 : -1) * 40), nodes.Count - 1 - i / 2 - (cnt - 1 - i) / 2 >= 0 ? Math.Max(0, nodes.Count - cnt / 2 - (cnt - 1 - i) / 2 - 1 + i / 2 - i / 2) : 0)); }
            int on = (int)(nodes.Count * v * Ease(In(m, .2f, 1.5f)));
            for (int i = 1; i < nodes.Count; i++) { var a0 = nodes[i].q; var pr = nodes[Math.Clamp(nodes[i].parent, 0, nodes.Count - 1)].q; m.Limb(pr.X, pr.Y, pr.Z, a0.X, a0.Y, a0.Z, 4, 4, i < on ? m.C(p.Color1) : m.C(p.Color2)); }
            for (int i = 0; i < nodes.Count; i++) { var q = nodes[i].q; m.Ball(q.X, q.Y, q.Z, 40, i < on ? m.C(p.Color1) : m.C(p.Color2)); }
        });
        Q(d, typeof(InventoryParameter), (p, m, t, v, v2) => // 持ち物: 厚い枠に並ぶマスと、入っている物の玉、選択の光る枠
        {
            var it = p.ItemsForModel; int n = Math.Clamp(p.Count, 4, 48), cols = 8, rows = (n + cols - 1) / cols; float cs = 110;
            m.Box(0, 0, -20, cols * cs + 40, rows * cs + 40, 30, m.C(p.Color1), .05f);
            int sel = (int)(m.Now * 1.5f) % n;
            for (int i = 0; i < n; i++)
            {
                float x = (i % cols - (cols - 1) / 2f) * cs, y = (i / cols - (rows - 1) / 2f) * cs;
                m.Box(x, y, 0, cs - 12, cs - 12, 10, m.C(0x37474F), .1f);
                if (i < it.Count) { m.Ball(x, y - 10, 20, 26, Pal(m, i)); m.Text(it[i], 20, x, y + 34, 10, 2, W, cs - 16); }
                if (i == sel) m.Tube([new(x - 50, y - 50, 12), new(x + 50, y - 50, 12), new(x + 50, y + 50, 12), new(x - 50, y + 50, 12), new(x - 50, y - 50, 12)], 4, m.C(p.Color2));
            }
        });
        Q(d, typeof(RouletteWheelParameter), (p, m, t, v, v2) => // ルーレット盤: 色分けされた厚い扇の円盤が回って止まる、上の矢印
        {
            var it = p.ItemsForModel; int n = Math.Max(2, it.Count); int target = Math.Clamp((int)(v * n), 0, n - 1);
            float st = Ease(In(m, 0, 3)), rot = -(target + .5f) * MathF.Tau / n - MathF.PI / 2 + (1 - st) * 20, R = 330;
            m.Turn(0, 0, rot, () =>
            {
                for (int i = 0; i < n; i++) { float a0 = i * MathF.Tau / n, a1 = a0 + MathF.Tau / n, mid = (a0 + a1) / 2; m.Slab(0, 40, Pal(m, i), PiePts(0, 0, R, a0, a1, 8)); if (i < it.Count) m.Text(it[i], 34, MathF.Cos(mid) * R * .65f, MathF.Sin(mid) * R * .65f, 22, 3, W, 140); }
                m.Torus(0, 0, 0, R, 14, m.C(p.Color1));
            });
            m.Disc(0, 0, 30, 40, 40, 30, m.C(p.Color1), .4f);
            Gem(m, Pts(0, -R + 30, 30, -R - 40, -30, -R - 40), 0, -R - 10, 0f, 8, 20, m.C(0xE53935), m.C(0xE53935));
        });
        Q(d, typeof(PuzzleChainParameter), (p, m, t, v, v2) => // パズルの連鎖: 落ち物の盤に積もった丸いブロック、消えて光る列
        {
            int cols = 6, rows = 10; float cs = 70;
            m.Box(0, 0, -30, cols * cs + 20, rows * cs + 20, 20, m.C(0x1A1A24, .8f), .05f);
            int clear = (int)(m.Now * .8f) % rows;
            for (int y = 0; y < rows; y++) for (int x = 0; x < cols; x++)
                {
                    if (DrawKit.H(p.Seed, x * 13 + y) < .25f + y * .03f) continue;
                    float px = (x - (cols - 1) / 2f) * cs, py = (y - (rows - 1) / 2f) * cs;
                    bool flash = y == clear && (m.Now * .8f % 1) > .6f;
                    m.Ball(px, py, 0, cs * .45f, cs * .45f, cs * .4f, flash ? m.C(p.Color1) : Pal(m, (int)(DrawKit.H(p.Seed, x + y * 7, 1) * 5)));
                }
            m.Text($"{1 + (int)(m.Now * .8f)} 連鎖!", 80, cols * cs / 2 + 220, 0, 20, 10, m.C(p.Color1));
        });
        Q(d, typeof(PauseScreenParameter), (p, m, t, v, v2) => // ポーズ画面: 一時停止の2本の柱、「PAUSE」、メニュー
        {
            var ln = Lines(p);
            foreach (int s in Sides) m.Box(s * 70, -200, 0, 70, 220, 60, m.C(p.Color1), .2f);
            m.Text("PAUSE", 110, 0, -20, 10, 14, W);
            int sel = (int)(m.Now * .7f) % Math.Max(1, ln.Count);
            for (int i = 0; i < ln.Count; i++) { float y = 110 + i * 80; m.Text(ln[i], 50, 0, y, 10, 6, i == sel ? m.C(p.Color1) : W); if (i == sel) Gem(m, Pts(-220, y - 18, -190, y, -220, y + 18), -205, y, 0f, 3, 10, m.C(p.Color1), m.C(p.Color1)); }
        });
        QS(d, typeof(TurnStartParameter), (p, m, t, v, v2) => // ターン開始: 斜めに飛びこむ厚い帯と、通り抜ける大きな文字
        {
            float tt = m.Now, inn = Ease(Math.Clamp(tt / .3f, 0, 1)), outk = Ease(Math.Clamp((tt - 1.3f) / .3f, 0, 1));
            float x = -m.W * (1 - inn) + m.W * outk;
            m.Turn(0, 0, -.05f, () =>
            {
                m.Box(x, 0, 0, m.W * 1.2f, 200, 50, m.C(p.Color1), .05f);
                m.Box(x, -108, 10, m.W * 1.2f, 12, 60, m.C(p.Color2), .3f); m.Box(x, 108, 10, m.W * 1.2f, 12, 60, m.C(p.Color2), .3f);
                m.Text(Str(p), 120, x * 1.2f, 0, 30, 20, m.C(p.Color2));
            });
        });
        Q(d, typeof(BuffDebuffParameter), (p, m, t, v, v2) => // バフ・デバフ: 上(または下)へ流れる立体の矢印
        {
            bool down = p.Variant >= 2; var col = m.C(down ? p.Color2 : p.Color1); float dir = down ? 1 : -1;
            for (int i = 0; i < Math.Max(1, p.Count); i++)
            {
                float f = ((m.Now * .8f + i / (float)Math.Max(1, p.Count)) % 1), x = (DrawKit.H(p.Seed, i) - .5f) * 400, y = -dir * 200 + dir * 400 * f, z = (DrawKit.H(p.Seed, i, 1) - .5f) * 200;
                var c = col with { W = MathF.Sin(f * MathF.PI) * m.Al };
                m.Move(0, 0, z, () => Puff(m, 0, 30, c, x - 20, y, x + 20, y, x + 20, y - dir * 50, x + 45, y - dir * 50, x, y - dir * 100, x - 45, y - dir * 50, x - 20, y - dir * 50));
            }
            m.Text(Str(p), 70, 0, 260, 10, 8, col);
        });
        Q(d, typeof(JumpDustParameter), (p, m, t, v, v2) => // 砂ぼこり: 足もとから左右に広がる、ふわっとした玉
        {
            float per = 1.2f, f = (m.Now % per) / per;
            for (int i = 0; i < 10; i++)
            {
                float s = i % 2 == 0 ? 1 : -1, sp = .5f + DrawKit.H(p.Seed, i) * .8f, x = s * (40 + 300 * f * sp), y = -60 * MathF.Sin(f * MathF.PI) * sp, z = (DrawKit.H(p.Seed, i, 1) - .5f) * 200 * f;
                m.Ball(x, y, z, (30 + 40 * f) * sp, m.C(p.Color1, .7f * (1 - f)));
            }
        });

        // ---------------------------------------------------------------- 194 SNS・配信
        Q(d, typeof(SuperChatParameter), (p, m, t, v, v2) => // スーパーチャット: 金額で色の変わる2段の厚い札
        {
            var ln = Str(p).Split('\n'); string name = ln.ElementAtOrDefault(0) ?? "", msg = ln.ElementAtOrDefault(1) ?? "", amt = ln.ElementAtOrDefault(2) ?? "";
            long.TryParse(new string(amt.Where(char.IsDigit).ToArray()), out long yen);
            uint c0 = yen >= 10000 ? 0xE62117u : yen >= 5000 ? 0xE91E63u : yen >= 2000 ? 0xF57C00u : yen >= 1000 ? 0xFFCA28u : yen >= 500 ? 0x1DE9B6u : 0x1E88E5u;
            float k = Ease(In(m, 0, .35f));
            m.Box(0, -50, 0, 900 * k, 110, 40, Sculpt.Dark(m.C(c0), .8f), .1f);
            m.Box(0, 50, 0, 900 * k, 100, 40, m.C(c0), .1f);
            m.Ball(-380 * k, -50, 22, 36, W);
            m.Text(name, 36, -200 * k, -70, 22, 3, W); m.Text(amt, 40, -200 * k, -25, 22, 3, W);
            m.Text(msg, 38, 0, 50, 22, 3, W, 860);
        });
        Q(d, typeof(ChatColumnParameter), (p, m, t, v, v2) => // チャット欄: 暗い厚い板と、下から順に出る行(名前の丸)
        {
            var rows = Lines(p); float bw = 600, rh = 70, bh = MathF.Max(400, rows.Count * rh + 60);
            m.Box(0, 0, 0, bw, bh, 30, m.C(p.Color1, .9f), .05f);
            for (int i = 0; i < rows.Count; i++) { float k = In(m, i * .4f, .3f); if (k <= 0) continue; float y = -bh / 2 + 50 + i * rh; m.Ball(-bw / 2 + 40, y, 20, 18, Pal(m, i)); m.Text(rows[i], 30, 30, y, 17, 3, W, bw - 120); }
        });
        Q(d, typeof(ViewCounterParameter), (p, m, t, v, v2) => // 再生回数: 再生マークと高評価の手、増える数字の厚い札
        {
            var ln = Str(p).Split('\n'); long.TryParse(new string((ln.ElementAtOrDefault(0) ?? "").Where(char.IsDigit).ToArray()), out long v0); long.TryParse(new string((ln.ElementAtOrDefault(1) ?? "").Where(char.IsDigit).ToArray()), out long v1);
            float k = Ease(In(m, .2f, 2));
            m.Box(0, 0, 0, 900, 200, 50, m.C(0xFFFFFF, .95f), .4f);
            m.Disc(-360, 0, 30, 50, 50, 20, m.C(p.Color1), .4f); m.Slab(42, 8, W, -375, -22, -335, 0, -375, 22);
            m.Text(((long)(v0 * k)).ToString("N0"), 60, -150, 0, 27, 6, K);
            m.Box(170, 10, 30, 40, 50, 20, K, .3f); m.Box(200, -20, 30, 20, 50, 20, K, .3f);
            m.Text(((long)(v1 * k)).ToString("N0"), 60, 320, 0, 27, 6, K);
        });
        Q(d, typeof(ShortVideoUiParameter), (p, m, t, v, v2) => // ショート動画UI: 右の丸いボタンの列(ハート・コメント・共有)と、下の名前
        {
            var ln = Str(p).Split('\n');
            for (int i = 0; i < 3; i++) { float y = -100 + i * 160; m.Disc(400, y, 0, 50, 50, 30, m.C(0x000000, .4f), .4f); }
            Heart3D(m, 400, -100, 20, 40, m.C(p.Color1));
            m.Ball(400, 60, 20, 30, 24, 10, W); m.Slab(20, 10, W, 380, 220, 420, 210, 380, 240);
            m.Text("@" + (ln.ElementAtOrDefault(0) ?? ""), 40, -200, 300, 4, 4, W);
            m.Text(ln.ElementAtOrDefault(1) ?? "", 34, -200, 350, 4, 3, W, 600);
        });
        Q(d, typeof(WipeFrameParameter), (p, m, t, v, v2) => // ワイプ枠: 白い太い輪と色のふち、名前の札
        {
            float R = 220, k = Ease(In(m, 0, .35f));
            m.Torus(0, 0, 0, R * k, 16, m.C(p.Color1)); m.Torus(0, 0, -6, (R + 18) * k, 6, m.C(p.Color2));
            m.Box(0, R * k + 40, 10, 300 * k, 70, 30, m.C(p.Color2), .4f); m.Text(Str(p), 40 * k, 0, R * k + 40, 26, 4, W);
        });
        Q(d, typeof(SubtitleBandParameter), (p, m, t, v, v2) => // 字幕の帯: 文字の長さに合わせた厚い半透明の帯と、白い文字
        {
            string s = Str(p); if (s.Length == 0) return; float bw = MathF.Min(1700, s.Length * 56 + 120);
            m.Box(0, 0, 0, bw, 110, 30, m.C(p.Color1, .6f), .3f);
            m.Text(s, 56, 0, 0, 17, 4, m.C(p.Color2), 1600);
        });
        Q(d, typeof(ChapterListParameter), (p, m, t, v, v2) => // 目次: 順に出る段の札(今のチャプターは手前に飛び出す)
        {
            var it = Lines(p); float rh = 100, y0 = -it.Count * rh / 2; int cur = (int)(m.Now * .5f) % Math.Max(1, it.Count);
            for (int i = 0; i < it.Count; i++)
            {
                float k = Ease(In(m, i * .2f, .35f)); if (k <= 0) continue; float y = y0 + rh * (i + .5f), z = i == cur ? 40 : 0;
                m.Box(0, y, z, 900 * k, rh - 12, 30, i == cur ? m.C(p.Color1) : m.C(0xFFFFFF, .9f), .2f);
                m.Text(it[i], 40, 0, y, z + 16, 3, i == cur ? W : K, 860);
            }
        });
        Q(d, typeof(DescriptionArrowParameter), (p, m, t, v, v2) => // 概要欄へ: ふち取りの厚い文字と、下を指してはずむ矢印
        {
            float k = Ease(In(m, 0, .35f)), b = MathF.Abs(MathF.Sin(m.Now * 4)) * 30;
            m.Text(Str(p), 80 * k, 0, -60, 10, 20, m.C(p.Color1), 0, m.C(p.Color2), 8);
            m.Move(0, b, () => Puff(m, 0, 50, m.C(p.Color1), -30, 40, 30, 40, 30, 120, 80, 120, 0, 200, -80, 120, -30, 120));
        });
        Q(d, typeof(IncomingCallParameter), (p, m, t, v, v2) => // 着信画面: 縦長の厚いスマホ、名前、ゆれる受話器のボタン2つ
        {
            var ln = Str(p).Split('\n');
            m.Box(0, 0, 0, 560, 1000, 40, m.C(p.Color1), .3f);
            m.Ball(0, -250, 24, 90, m.C(0x78909C));
            m.Text(ln.ElementAtOrDefault(0) ?? "", 60, 0, -100, 22, 4, W); m.Text(ln.ElementAtOrDefault(1) ?? "", 34, 0, -40, 22, 3, m.C(0xFFFFFF, .7f));
            float sh = MathF.Sin(m.Now * 20) * .2f;
            foreach (int s in Sides) { m.Disc(s * 150, 330, 30, 70, 70, 30, s < 0 ? m.C(0xE53935) : m.C(0x43A047), .4f); m.Turn(s * 150, 330, s > 0 ? sh : MathF.PI * .75f, () => { m.Box(s * 150, 330, 50, 80, 18, 10, W, .5f); }); }
        });
        QS(d, typeof(MapCurrentLocationParameter), (p, m, t, v, v2) => // 地図アプリ: 地面の地図(道)と、立体のピンと広がる輪
        {
            if (p.Background)
            {
                m.Box(0, 0, -20, m.W, m.H, 10, m.C(p.Color2), .01f);
                for (int i = 0; i < 6; i++) { m.Box((DrawKit.H(p.Seed, i) - .5f) * m.W, 0, -10, 30, m.H, 4, W, .3f); m.Box(0, (DrawKit.H(p.Seed, i, 1) - .5f) * m.H, -10, m.W, 30, 4, W, .3f); }
            }
            float f = (m.Now * .8f) % 1; m.Torus(0, 0, 0, 30 + 120 * f, 4, m.C(p.Color1, 1 - f));
            m.Disc(0, 0, 4, 26, 26, 10, m.C(p.Color1), .5f);
            float b = MathF.Abs(MathF.Sin(m.Now * 3)) * 20;
            m.Ball(0, -150 - b, 40, 60, m.C(0xE53935)); m.Ball(0, -150 - b, 90, 22, W); m.Limb(0, -110 - b, 40, 0, -20, 10, 30, 2, m.C(0xE53935));
        });
        Q(d, typeof(CalendarFlipParameter), (p, m, t, v, v2) => // カレンダーをめくる: とじ金具と、上へめくれていく日付の紙
        {
            var it = p.ItemsForModel; int d0 = it.Count > 0 && int.TryParse(it[0], out var a0) ? a0 : 1, d1 = it.Count > 1 && int.TryParse(it[1], out var a1) ? a1 : d0 + 3;
            int n = Math.Max(1, d1 - d0); float f = Math.Clamp(m.Now / 1.5f, 0, 1) * n; int done = (int)f; float fr = f - done;
            m.Box(0, 40, -20, 500, 560, 40, m.C(0xFFFFFF), .05f); m.Box(0, -220, 0, 500, 80, 50, m.C(p.Color1), .1f);
            m.Text((d0 + Math.Min(n, done + 1)).ToString(), 260, 0, 60, 2, 10, K);
            if (done < n) m.Pitch(0, -180, 4, -MathF.PI * Ease(fr), () => { m.Box(0, 60, 4, 490, 480, 4, W, .02f); m.Text((d0 + done).ToString(), 260, 0, 60, 8, 10, K); });
            foreach (int s in Sides) m.Limb(s * 140, -280, 0, s * 140, -180, 0, 14, 14, m.C(0x8A95A0));
        });

        // ---------------------------------------------------------------- 195 キャラの気持ち2
        Q(d, typeof(IdeaBulbParameter), (p, m, t, v, v2) => // ひらめき電球: ガラスの球と口金、点く光と放射の光
        {
            float k = Ease(In(m, 0, .3f)), on = In(m, .3f, .2f);
            m.Ball(0, -40, 0, 140 * k, Vector4.Lerp(m.C(0xFFFFFF, .4f), m.C(p.Color1), on));
            m.Cyl(0, 130 * k, 0, 70 * k, 90 * k, m.C(p.Color2), .3f);
            if (on > 0) for (int i = 0; i < 8; i++) { float ag = -MathF.PI + i * MathF.PI / 7; m.Limb(MathF.Cos(ag) * 190, -40 + MathF.Sin(ag) * 190, 0, MathF.Cos(ag) * (190 + 70 * on), -40 + MathF.Sin(ag) * (190 + 70 * on), 0, 10, 4, m.C(p.Color1)); }
        });
        Q(d, typeof(MoyamoyaParameter), (p, m, t, v, v2) => // もやもや: 前後に重なってゆれる、灰色のうずの管
        {
            for (int k = 0; k < 3; k++) { var sp = new List<Vector3>(); for (int i = 0; i <= 50; i++) { float f = i / 50f, ag = f * MathF.PI * 5 + m.Now * (1 + k * .3f); sp.Add(new(MathF.Cos(ag) * (40 + 140 * f) + (k - 1) * 80, MathF.Sin(ag) * (30 + 100 * f), (k - 1) * 40)); } m.Tube(sp, 6, m.C(p.Color1, .8f)); }
        });
        Q(d, typeof(AngrySteamParameter), (p, m, t, v, v2) => // イライラの湯気: 頭の上から噴き出す、ふくらむ白い玉
        {
            for (int i = 0; i < p.Count; i++)
            {
                float side = p.Count == 1 ? 0 : (i / (float)(p.Count - 1) - .5f) * 2, f = ((m.Now * 1.5f + i * .3f) % 1);
                m.Ball(side * 150 + side * 80 * f, -f * 220, MathF.Sin(i) * 40, 30 + 50 * f, m.C(p.Color1, .9f * (1 - f)));
            }
        });
        Q(d, typeof(HeartEyesParameter), (p, m, t, v, v2) => // ハートの目: どくんと脈打つ、ふっくらした2つのハート
        {
            float gap = 80 + 300 * v, beat = 1 + .18f * MathF.Pow(MathF.Max(0, MathF.Sin(m.Now * 6)), 8);
            foreach (int s in Sides) Heart3D(m, s * gap / 2, 0, 0, 90 * beat * Ease(In(m, 0, .3f)), m.C(p.Color1));
        });
        Q(d, typeof(TearyEyesParameter), (p, m, t, v, v2) => // 涙目: 白い目に、たまっていく水の玉とつや
        {
            float gap = 80 + 300 * v, fill = Ease(In(m, 0, 1));
            foreach (int s in Sides) { float x = s * gap / 2; m.Ball(x, 0, 0, 80, 80, 40, W); m.Ball(x, 10, 30, 50, 50, 20, K); m.Ball(x, 40 - 40 * fill, 50, 76, 40 * fill + 5, 30, m.C(p.Color1, .6f)); m.Ball(x - 20, -20, 70, 14, W); }
        });
        Q(d, typeof(DokidokiParameter), (p, m, t, v, v2) => // ドキドキ: 鼓動に合わせてふくらむハートと、広がる輪
        {
            float per = .8f, ph = (m.Now % per) / per, beat = 1 + .25f * MathF.Exp(-ph * 8);
            Heart3D(m, 0, 0, 0, 160 * beat, m.C(p.Color1));
            m.Torus(0, 0, -20, 180 + 200 * ph, 4, m.C(p.Color1, 1 - ph));
            m.Text("ドキッ", 60, 230, -180, 10, 6, m.C(p.Color1));
        });
        Q(d, typeof(SighParameter), (p, m, t, v, v2) => // ため息: 口から流れて消える、ふわっとした白い息のかたまり
        {
            float dir = p.Variant >= 2 ? -1 : 1, per = 2.4f, f = (m.Now % per) / per;
            for (int i = 0; i < 6; i++) { float g = MathF.Max(0, f - i * .05f); m.Ball(dir * 300 * g, 40 * MathF.Sin(g * 6) - 60 * g, 0, 30 + 60 * g, m.C(p.Color1, .7f * (1 - g))); }
        });
        Q(d, typeof(DroolParameter), (p, m, t, v, v2) => // よだれ: のびて落ちるしずくと、きらきら
        {
            float per = 1.6f, f = (m.Now % per) / per, len = 40 + 160 * f;
            m.Limb(0, 0, 20, 0, len, 20, 10, 16, m.C(p.Color1, .8f)); m.Ball(0, len, 20, 20, 26, 20, m.C(p.Color1, .85f));
            for (int i = 0; i < 3; i++) Gem(m, Pts(StarPts(-120 + i * 120, -120 - i * 20, 20 + 6 * MathF.Sin(m.Now * 6 + i), 4, .3f, 0)), -120 + i * 120, -120 - i * 20, 0f, 2, 8, m.C(p.Color2), m.C(p.Color2));
        });
        Q(d, typeof(ColdSweatLinesParameter), (p, m, t, v, v2) => // ひやあせ: 上からかかる青い影の縦線と、汗の玉
        {
            float dir = p.Variant >= 2 ? -1 : 1, k = Ease(In(m, 0, .5f));
            for (int i = 0; i < 7; i++) { float x = (i - 3) * 50, len = (120 + 80 * DrawKit.H(p.Seed, i)) * k; m.Box(x, -150 + len / 2, 0, 6, len, 6, m.C(p.Color1, .8f), .5f); }
            float f = (m.Now * .8f) % 1; m.Ball(dir * 200, -60 + f * 120, 30, 22, 30, 20, m.C(p.Color2, 1 - f * .5f));
        });
        Q(d, typeof(ShockThunderParameter), (p, m, t, v, v2) => // ガーン: 暗い後ろの板と、後ろに落ちるぶ厚い稲妻
        {
            float dark = In(m, 0, .15f);
            if (p.Background) m.Box(0, 0, -300, 2200, 1300, 10, m.C(p.Color2, .75f * dark), .01f);
            float fl = (m.Now * 6 % 1) < .5f ? 1 : .7f;
            foreach (int s in Sides) Puff(m, -150, 40, m.C(p.Color1, fl), s * 300 + 40, -400, s * 300 - 60, -80, s * 300 + 20, -80, s * 300 - 80, 300, s * 300 + 80, -20, s * 300 + 10, -20, s * 300 + 120, -400);
        });

        // ---------------------------------------------------------------- 196 自然2
        QS(d, typeof(ShowerClearingParameter), (p, m, t, v, v2) => // にわか雨と晴れ間: 灰色の雲から降る雨、だんだん差しこむ光
        {
            float k = Math.Clamp(m.Now / 6, 0, 1);
            for (int i = 0; i < 8; i++) m.Ball(-m.W / 2 + m.W * i / 7, -m.H * .35f, -300, 220, 120, 120, Vector4.Lerp(m.C(p.Color1), m.C(0xFFFFFF), k) with { W = (1 - k * .6f) * m.Al });
            if (k < .8f) for (int i = 0; i < 40; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W, y = -m.H * .2f + ((m.Now * 900 + DrawKit.H(p.Seed, i, 1) * m.H) % m.H) * .6f, z = (DrawKit.H(p.Seed, i, 2) - .5f) * 400; m.Limb(x, y, z, x - 8, y + 50, z, 2, 2, m.C(0xBFD9FF, (1 - k) * .8f)); }
            for (int i = 0; i < 5; i++) m.Box(-m.W * .2f + i * 120, 0, -200, 60, m.H * 1.2f, 4, m.C(p.Color2, k * .25f), .5f);
        });
        Q(d, typeof(WindFlagsParameter), (p, m, t, v, v2) => // 洗濯物・旗: 張った綱と、風にはためく布(波打つ)
        {
            float span = 1500;
            var rope = Enumerable.Range(0, 31).Select(i => { float f = i / 30f; return new Vector3(-span / 2 + span * f, -200 + MathF.Sin(f * MathF.PI) * 60, 0); }).ToList();
            m.Tube(rope, 3, m.C(0x8D6E63));
            for (int i = 1; i < 6; i++)
            {
                var q = rope[i * 5]; var col = Pal(m, i);
                for (int k = 0; k < 6; k++) { float y0 = q.Y + k * 30, y1 = y0 + 30, w0 = MathF.Sin(m.Now * 6 + k * .8f + i) * 30 * k / 5f, w1 = MathF.Sin(m.Now * 6 + (k + 1) * .8f + i) * 30 * (k + 1) / 5f; m.Quad3D(new(q.X - 70 + w0 * .3f, y0, w0), new(q.X + 70 + w0 * .3f, y0, w0), new(q.X + 70 + w1 * .3f, y1, w1), new(q.X - 70 + w1 * .3f, y1, w1), col); }
            }
        });
        QS(d, typeof(StormApproachParameter), (p, m, t, v, v2) => // 雷雲が近づく: 奥から迫る黒い雲の群れと、光る稲妻
        {
            float k = Ease(In(m, 0, 3));
            for (int i = 0; i < 14; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W * 1.3f, y = -m.H * .35f + (DrawKit.H(p.Seed, i, 1) - .5f) * 200, z = -1500 + 1300 * k + DrawKit.H(p.Seed, i, 2) * 200; m.Ball(x, y, z, 260, 160, 200, m.C(p.Color1)); }
            if ((m.Now * 1.3f % 1) < .12f) Puff(m, -300 + 1300 * k, 20, m.C(p.Color2), 0, -m.H * .3f, -80, 0, 20, 0, -60, m.H * .4f, 80, -40, -10, -40, 60, -m.H * .3f);
        });
        QS(d, typeof(AutumnMountainParameter), (p, m, t, v, v2) => // 紅葉の山: 空、山の三角すい、色づいた木の玉、舞う葉
        {
            if (p.Background) SkyWall(m, m.C(0x81D4FA), m.C(0xE1F5FE), -1500);
            for (int i = 0; i < 3; i++) m.Lathe(-m.W * .3f + i * m.W * .3f, -800 + i * 100, Sculpt.Dark(m.C(p.Color1), .7f), 0, -m.H * .1f - i * 40, m.W * .3f, m.H * .4f, 0, m.H * .4f);
            for (int i = 0; i < 40; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W, z = -600 + DrawKit.H(p.Seed, i, 1) * 600, y = m.H * .3f - DrawKit.H(p.Seed, i, 2) * 100; m.Ball(x, y, z, 50 + 30 * DrawKit.H(p.Seed, i, 3), Vector4.Lerp(m.C(p.Color1), m.C(p.Color2), DrawKit.H(p.Seed, i, 4))); }
            for (int i = 0; i < 12; i++) { float f = ((m.Now * .2f + i / 12f) % 1), x = (DrawKit.H(p.Seed, i + 70) - .5f) * m.W + MathF.Sin(m.Now + i) * 60; m.Turn(x, -m.H / 2 + f * m.H, m.Now * 2 + i, () => Puff(m, 200, 4, m.C(p.Color1), x, -m.H / 2 + f * m.H - 14, x + 12, -m.H / 2 + f * m.H, x, -m.H / 2 + f * m.H + 14, x - 12, -m.H / 2 + f * m.H)); }
        });
        QS(d, typeof(WinterBreathParameter), (p, m, t, v, v2) => // 吐く息・結露: 白い息のかたまり(または窓にくもった水の粒)
        {
            if (p.Variant >= 2)
            {
                m.Box(0, 0, -10, m.W, m.H, 4, m.C(p.Color1, .35f), .01f);
                for (int i = 0; i < 60; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * m.W, (DrawKit.H(p.Seed, i, 1) - .5f) * m.H, 0, 4 + 10 * DrawKit.H(p.Seed, i, 2), m.C(0xFFFFFF, .5f));
                return;
            }
            float f = (m.Now % 2) / 2;
            for (int i = 0; i < 6; i++) { float g = MathF.Max(0, f - i * .04f); m.Ball(200 * g, -80 * g, 100 * g, 40 + 120 * g, m.C(p.Color1, .6f * (1 - g))); }
        });
        QS(d, typeof(RiverTopViewParameter), (p, m, t, v, v2) => // 川の流れ(見下ろし): 草地の板、くぼんだ川と流れる光、岩
        {
            m.Box(0, 0, -20, m.W, m.H, 40, m.C(p.Color2), .01f);
            var bank = Enumerable.Range(0, 21).Select(i => { float f = i / 20f; return new Vector2(MathF.Sin(f * 4) * 200, -m.H / 2 + m.H * f); }).ToList();
            var poly = bank.Select(q => q + new Vector2(-180, 0)).Concat(bank.AsEnumerable().Reverse().Select(q => q + new Vector2(180, 0))).ToList();
            m.Slab(poly, 2, 10, m.C(p.Color1));
            for (int i = 0; i < 20; i++) { float f = ((m.Now * .2f + i / 20f) % 1); var q = bank[(int)(f * 20)]; m.Box(q.X + (DrawKit.H(p.Seed, i) - .5f) * 280, -m.H / 2 + f * m.H, 10, 8, 60, 2, m.C(0xFFFFFF, .5f), .5f); }
            for (int i = 0; i < 8; i++) m.Ball(bank[i * 2 + 2].X + (i % 2 == 0 ? 200 : -200), bank[i * 2 + 2].Y, 10, 40, m.C(0x9E9E9E));
        });
        QS(d, typeof(TidePoolParameter), (p, m, t, v, v2) => // 潮だまり: 岩の地面、透けた水のくぼみ、ヒトデと小さな波紋
        {
            m.Box(0, 0, -30, m.W, m.H, 40, m.C(p.Color2), .01f);
            for (int i = 0; i < 30; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * m.W, (DrawKit.H(p.Seed, i, 1) - .5f) * m.H, -10, 60 + 80 * DrawKit.H(p.Seed, i, 2), 50 + 60 * DrawKit.H(p.Seed, i, 3), 40, Sculpt.Dark(m.C(p.Color2), .8f + .2f * DrawKit.H(p.Seed, i, 4)));
            m.Ball(0, 0, 0, 500, 300, 20, m.C(p.Color1, .6f));
            Gem(m, Pts(StarPts(-100, 50, 60, 5, .4f, .3f)), -100, 50, 0f, 4, 12, m.C(0xFF7043), m.C(0xFF7043));
            float f = (m.Now * .5f) % 1; m.Torus(120, -40, 22, 20 + 100 * f, 2, m.C(0xFFFFFF, 1 - f));
        });
        QS(d, typeof(DesertHeatHazeParameter), (p, m, t, v, v2) => // 砂漠と陽炎: 空、波打つ砂丘の面、ゆらぐ空気の帯
        {
            SkyWall(m, m.C(0x64B5F6), m.C(p.Color2), -1500);
            for (int L = 0; L < 4; L++)
            {
                float z = -1200 + L * 350, by = m.H * (.05f + L * .1f);
                var pts = new List<Vector2> { new(m.W, m.H) }; for (int i = 30; i >= 0; i--) { float x = -m.W + 2 * m.W * i / 30f; pts.Add(new(x, by - (MathF.Sin(x * .003f + L) * .5f + .5f) * 120)); } pts.Add(new(-m.W, m.H));
                m.Slab(pts, z, 30, Sculpt.Dark(m.C(p.Color1), .8f + L * .06f));
            }
            for (int i = 0; i < 5; i++) m.Box(MathF.Sin(m.Now * 2 + i) * 40, m.H * .02f + i * 14, -300, m.W * 1.2f, 6, 4, m.C(0xFFFFFF, .12f), .5f);
        });
        QS(d, typeof(NightPaddyFrogParameter), (p, m, t, v, v2) => // 夜の田んぼ: 夜空、水を張った田の面に映る月、稲の株、鳴くカエル
        {
            SkyWall(m, m.C(p.Color2), m.C(0x1B3A5A), -1500);
            m.Ball(m.W * .25f, -m.H * .3f, -1400, 70, m.C(p.Color1));
            m.Quad3D(new(-m.W, 0, -1500), new(m.W, 0, -1500), new(m.W, m.H / 2, 300), new(-m.W, m.H / 2, 300), m.C(0x0E2036, .9f));
            m.Ball(m.W * .2f, m.H * .2f, -300, 60, 14, 20, m.C(p.Color1, .5f));
            for (int r = 0; r < 6; r++) for (int i = 0; i < 12; i++) { float x = -m.W / 2 + m.W * (i + .5f) / 12, z = 200 - r * 250, y = m.H / 2 - (300 - z) * (m.H / 2) / 1800; m.Limb(x, y, z, x + 10, y - 50, z, 3, 1, m.C(0x3A5A3A)); }
            float b = MathF.Max(0, MathF.Sin(m.Now * 5)); m.Ball(-m.W * .2f, m.H * .35f, 100, 40, 30, 30, m.C(0x4E7D3A)); m.Ball(-m.W * .2f, m.H * .35f + 20, 130, 26 * b + 5, 20 * b + 4, 14, m.C(0xF0E6C0));
        });
        QS(d, typeof(StarsCityLightsParameter), (p, m, t, v, v2) => // 星空と街の灯: 奥の星空、遠くのビル群の箱と窓の灯り
        {
            SkyWall(m, m.C(p.Color2), Vector4.Lerp(m.C(p.Color2), m.C(p.Color1), .3f), -2000);
            int keep = m.Seg; m.Seg = 5;
            for (int i = 0; i < 100; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * m.W * 2, -m.H * .6f + DrawKit.H(p.Seed, i, 1) * m.H * .6f, -1900, 2 + 3 * DrawKit.H(p.Seed, i, 2), m.C(0xFFFFFF, .5f + .5f * MathF.Sin(m.Now * 2 + i)));
            m.Seg = keep;
            for (int i = 0; i < 30; i++)
            {
                float x = -m.W + m.W * 2 * i / 30f, hh = 100 + 300 * DrawKit.H(p.Seed, i + 200), z = -800 + DrawKit.H(p.Seed, i + 210) * 400, yb = m.H * .4f;
                m.Box(x, yb - hh / 2, z, 120, hh, 100, m.C(0x0A0F20), .02f);
                for (int k = 0; k < 6; k++) if (DrawKit.H(p.Seed, i * 9 + k) > .4f) m.Box(x - 30 + (k % 2) * 60, yb - hh + 30 + (k / 2) * 40, z + 51, 20, 14, 2, m.C(p.Color1), .3f);
            }
        });
    }
}
