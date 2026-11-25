using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 184番(画面・UI 15)・185番(キャラの気持ち 14)・186番(自然 17)・187番(季節の行事 12)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>空の背景(奥の大きな板。上下の色)</summary>
    static void SkyWall(Sculpt m, Vector4 top, Vector4 bot, float z = -400)
    {
        m.Quad3D(new(-m.W, -m.H, z), new(m.W, -m.H, z), new(m.W, 0, z), new(-m.W, 0, z), top);
        m.Quad3D(new(-m.W, 0, z), new(m.W, 0, z), new(m.W, m.H, z), new(-m.W, m.H, z), bot);
    }

    /// <summary>ふっくらしたハート(厚い板＋丸いふち)</summary>
    static void Heart3D(Sculpt m, float x, float y, float z, float r, Vector4 col) => Puff(m, z, r * .5f, col, HeartPts(x, y + r * .05f, r / 260f));

    static partial void RegisterH1b(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.12f, .13f, .16f, 1);
        // ---------------------------------------------------------------- 184 画面・UI
        QS(d, typeof(VideoPlayerUiParameter), (p, m, t, v, v2) => // 動画プレーヤー: 下の操作の帯、進む棒と玉、ボタン
        {
            float y = m.H / 2 - 90, pos = Math.Clamp(v + m.Now * .01f, 0, 1), x0 = -m.W / 2 + 60, bw = m.W - 120;
            m.Box(0, m.H / 2 - 80, -10, m.W, 160, 10, m.C(0x000000, .6f), .02f);
            m.Box(0, y - 30, 0, bw, 8, 8, m.C(0xFFFFFF, .3f), .5f);
            m.Box(x0 + bw * pos / 2, y - 30, 4, bw * pos, 8, 10, m.C(p.Color1), .5f);
            m.Ball(x0 + bw * pos, y - 30, 8, 14, m.C(p.Color1));
            m.Slab(8, 12, W, x0 + 10, y + 5, x0 + 40, y + 22, x0 + 10, y + 39);
            m.Text(Str(p), 40, -m.W / 2 + 400, -m.H / 2 + 60, 4, 6, W);
        });
        Q(d, typeof(MailReceivedParameter), (p, m, t, v, v2) => // メール: 厚い白い板、色の帯、封筒、差出人と件名
        {
            var ln = Str(p).Split('\n'); float bw = 900, rh = 120;
            m.Box(0, 0, 0, bw + 20, rh * 4.4f, 30, m.C(0xF7F7FA), .1f);
            m.Box(0, -rh * 1.7f, 16, bw + 20, rh, 6, m.C(p.Color1), .1f);
            m.Box(-bw / 2 + 90, 10, 20, 100, 70, 16, m.C(0xFFFFFF), .2f);
            m.Tube([new(-bw / 2 + 40, -25, 30), new(-bw / 2 + 90, 15, 30), new(-bw / 2 + 140, -25, 30)], 3, m.C(p.Color1));
            m.Text("新着メール", 44, 0, -rh * 1.7f, 22, 4, W);
            m.Text(ln.Length > 0 ? ln[0] : "", 44, 60, -10, 18, 4, K, bw - 220);
            m.Text(ln.Length > 1 ? ln[1] : "", 38, 60, 70, 18, 4, m.C(0x555560), bw - 220);
        });
        Q(d, typeof(DialogBoxParameter), (p, m, t, v, v2) => // ダイアログ: 浮いた厚い板、題名の帯、押しこむボタン2つ
        {
            var ln = Str(p).Split('\n'); float k = Ease(In(m, 0, .3f)), bw = 760 * k, bh = 360 * k; if (bw < 2) return;
            m.Box(0, 0, 0, bw, bh, 30, m.C(0xFFFFFF), .1f);
            m.Box(0, -bh / 2 + 35 * k, 16, bw, 70 * k, 6, m.C(p.Color1), .1f);
            m.Text(ln[0], 40 * k, 0, -bh / 2 + 35 * k, 22, 4, W);
            m.Text(string.Join(" ", ln.Skip(1)), 36 * k, 0, -10, 16, 4, K, bw - 60);
            m.Box(-130 * k, bh / 2 - 60 * k, 20, 200 * k, 70 * k, 24, m.C(p.Color1), .4f); m.Text("OK", 36 * k, -130 * k, bh / 2 - 60 * k, 33, 4, W);
            m.Box(130 * k, bh / 2 - 60 * k, 20, 200 * k, 70 * k, 24, m.C(0xDDDDE3), .4f); m.Text("キャンセル", 32 * k, 130 * k, bh / 2 - 60 * k, 33, 4, K);
        });
        Q(d, typeof(PcWindowParameter), (p, m, t, v, v2) => // パソコンのウィンドウ: 厚い板、題名の帯、右上の3つのボタン
        {
            float k = Ease(In(m, 0, .3f)), bw = 1300 * k, bh = 760 * k; if (bw < 2) return;
            m.Box(0, 0, 0, bw, bh, 30, m.C(0xF4F4F6), .03f);
            m.Box(0, -bh / 2 + 28, 16, bw, 56, 6, m.C(p.Color1), .03f);
            uint[] bc = [0xFF5F57, 0xFFBD2E, 0x28C840];
            for (int i = 0; i < 3; i++) m.Disc(bw / 2 - 40 - i * 46, -bh / 2 + 28, 22, 13, 13, 8, m.C(bc[i]), .5f);
            m.Text(Str(p), 30, -bw / 2 + 200, -bh / 2 + 28, 22, 4, W);
        });
        Q(d, typeof(ClickRippleParameter), (p, m, t, v, v2) => // クリック: 動いてくる立体のカーソルと、広がる波紋の輪
        {
            float mv = Ease(In(m, 0, .6f)); var cur = Vector2.Lerp(new(300, 220), Vector2.Zero, mv);
            for (int i = 0; i < p.Count; i++) { float tt = m.Now - .6f - i * .45f; if (tt < 0 || tt > .8f) continue; float f = tt / .8f; m.Torus(0, 0, 0, 30 + 150 * f, 5 * (1 - f) + 1, m.C(p.Color1, 1 - f)); }
            m.Slab(30, 20, W, cur.X, cur.Y, cur.X, cur.Y + 80, cur.X + 20, cur.Y + 62, cur.X + 36, cur.Y + 96, cur.X + 48, cur.Y + 90, cur.X + 32, cur.Y + 56, cur.X + 58, cur.Y + 56);
        });
        Q(d, typeof(CodeTypingParameter), (p, m, t, v, v2) => // コード: 黒い厚い画面と、打ちこまれていく色の文字
        {
            string shown = TextKit.Typed(Str(p), m.Now, 25f * MathF.Max(.1f, t / MathF.Max(.01f, m.Now)));
            float bw = 1300, bh = 760;
            if (p.Background) m.Box(0, 0, 0, bw, bh, 40, m.C(p.Color1), .03f);
            m.Box(0, -bh / 2 + 22, 22, bw, 44, 4, m.C(0x333338), .03f);
            var lines = shown.Split('\n');
            for (int i = 0; i < Math.Min(lines.Length, 14); i++) m.Text(lines[i], 34, -bw / 2 + 60 + lines[i].Length * 9, -bh / 2 + 90 + i * 48, 24, 3, i % 3 == 0 ? m.C(p.Color2) : m.C(0xD4D4D4), bw - 100);
        });
        Q(d, typeof(DownloadProgressParameter), (p, m, t, v, v2) => // ダウンロード: 厚い札、ファイルの形、進む棒と%
        {
            float k = v * Ease(In(m, .2f, 3f)), bw = 900, bh = 220;
            m.Box(0, 0, 0, bw, bh, 30, m.C(0xFAFAFC), .15f);
            m.Slab(20, 14, m.C(p.Color1), -bw / 2 + 55, -55, -bw / 2 + 105, -55, -bw / 2 + 125, -35, -bw / 2 + 125, 35, -bw / 2 + 55, 35);
            m.Box(60, 40, 18, 600, 24, 8, m.C(0xDDDDE3), .5f);
            if (k > .01f) m.Box(-240 + 300 * k, 40, 22, 600 * k, 24, 10, m.C(p.Color1), .5f);
            m.Text(Str(p), 40, 0, -40, 16, 4, K, 600);
            m.Text($"{(int)(k * 100)}%", 40, bw / 2 - 70, 40, 16, 4, m.C(p.Color1));
        });
        Q(d, typeof(PollResultParameter), (p, m, t, v, v2) => // 投票の結果: 項目ごとの立体の棒と%
        {
            var rows = Lines(p).Select(r => { int ci = r.LastIndexOfAny([':', '：']); float.TryParse(ci > 0 ? r[(ci + 1)..] : "0", out var f); return (n: ci > 0 ? r[..ci] : r, v: f); }).ToList();
            if (rows.Count == 0) return; float sum = MathF.Max(1e-3f, rows.Sum(r => r.v)), rh = 110, y0 = -rows.Count * rh / 2, bw = 900;
            for (int i = 0; i < rows.Count; i++)
            {
                float k = Ease(In(m, .2f + i * .15f, .8f)), f = rows[i].v / sum, y = y0 + rh * (i + .5f);
                m.Box(0, y, -6, bw, 70, 10, m.C(0xFFFFFF, .15f), .4f);
                if (f * k > .005f) m.Box(-bw / 2 + bw * f * k / 2, y, 10, bw * f * k, 70, 40, i == 0 ? m.C(p.Color1) : m.C(0x9AA3AD), .4f);
                m.Text(rows[i].n, 40, -bw / 2 + 140, y, 32, 4, W, 400);
                m.Text($"{(int)MathF.Round(f * 100 * k)}%", 40, bw / 2 - 70, y, 32, 4, W);
            }
        });
        Q(d, typeof(FollowerCounterParameter), (p, m, t, v, v2) => // フォロワー数: 丸い厚い札、ハートと増える数字
        {
            long.TryParse(new string(Str(p).Where(char.IsDigit).ToArray()), out long goal);
            float k = Ease(In(m, 0, .3f)); long cur = (long)(goal * Ease(In(m, .2f, 3)));
            m.Box(0, 0, 0, 760 * k, 260 * k, 60, m.C(0xFFFFFF), .5f);
            Heart3D(m, -250 * k, 0, 30, 90 * k, m.C(p.Color1));
            m.Text(cur.ToString("N0"), 110 * k, 80 * k, 0, 30, 12, K);
        });
        QS(d, typeof(StreamWaitingParameter), (p, m, t, v, v2) => // 配信の待機画面: 色の背景、流れる玉、大きな文字、点滅する輪
        {
            if (p.Background) SkyWall(m, m.C(p.Color1), m.C(p.Color2), -200);
            for (int i = 0; i < 12; i++) { float x = ((DrawKit.H(p.Seed, i) * m.W + m.Now * 40 * (1 + i % 3)) % (m.W + 400) + m.W + 400) % (m.W + 400) - m.W / 2 - 200, y = (DrawKit.H(p.Seed, i, 1) - .5f) * m.H; m.Ball(x, y, -100 + DrawKit.H(p.Seed, i, 2) * 200, 30 + 60 * DrawKit.H(p.Seed, i, 3), m.C(0xFFFFFF, .15f)); }
            m.Text(Str(p), 120, 0, -40, 20, 30, W, m.W * .8f);
            float k = .5f + .5f * MathF.Sin(m.Now * 3);
            m.Torus(0, 120, 20, 24, 4, m.C(0xFF4040, .5f + .5f * k)); m.Text("まもなく開始", 50, 0, 200, 10, 6, W);
        });
        QS(d, typeof(EndCardParameter), (p, m, t, v, v2) => // エンドカード: 暗い背景、動画の枠2つ、丸い登録ボタン
        {
            if (p.Background) m.Box(0, 0, -30, m.W, m.H, 10, m.C(0x141419), .01f);
            m.Text(Str(p), 64, 0, -m.H / 2 + 130, 4, 8, W);
            for (int i = 0; i < 2; i++)
            {
                float k = Ease(In(m, .2f + i * .15f, .4f)), x = (i == 0 ? -1 : 1) * m.W * .2f;
                m.Box(x, -40, 0, 560 * k, 320 * k, 30, m.C(0x2A2A33), .05f);
                m.Box(x, -40, 16, 540 * k, 300 * k, 4, m.C(0x3A3A45), .05f);
                m.Slab(20, 14, W, x - 25, -70, x + 35, -40, x - 25, -10);
            }
            float q = Ease(In(m, .5f, .4f));
            m.Disc(0, m.H / 2 - 220, 0, 110 * q, 110 * q, 50, m.C(p.Color1), .4f); m.Text("登録", 44 * q, 0, m.H / 2 - 220, 26, 6, W);
        });
        QS(d, typeof(NewsLowerThirdParameter), (p, m, t, v, v2) => // ニュースの字幕帯: 下の厚い帯、色のタグ、見出し
        {
            var ln = Str(p).Split('\n'); float y = m.H / 2 - 170, k = Ease(In(m, 0, .5f)), bw = m.W * .8f * k;
            m.Box(-m.W / 2 + 60 + bw / 2, y, 0, bw, 120, 40, m.C(0xFFFFFF, .95f), .05f);
            m.Box(-m.W / 2 + 60 + 140, y - 90, 10, 280 * k, 60, 40, m.C(p.Color2), .1f);
            m.Box(-m.W / 2 + 60, y, 10, 20, 120, 50, m.C(p.Color1), .1f);
            m.Text(ln.Length > 0 ? ln[0] : "", 34 * k, -m.W / 2 + 200, y - 90, 32, 4, W);
            m.Text(ln.Length > 1 ? ln[1] : "", 56 * k, -m.W / 2 + 60 + bw / 2, y, 22, 6, m.C(p.Color1), bw - 60);
        });
        QS(d, typeof(BreakingNewsParameter), (p, m, t, v, v2) => // 速報: 上から下りる厚い帯と、赤い「速報」の箱
        {
            float bh = 90, k = Ease(In(m, 0, .3f)), y = -m.H / 2 - bh / 2 + bh * k;
            m.Box(0, y, 0, m.W, bh, 30, m.C(0xFFFFFF, .97f), .02f);
            m.Box(-m.W / 2 + 150, y, 12, 300, bh, 40, m.C(p.Color1), .05f);
            m.Text("ニュース速報", 44, -m.W / 2 + 150, y, 34, 4, W);
            m.Text(Str(p), 44, 160, y, 18, 4, K, m.W - 400);
        });
        Q(d, typeof(WeatherForecastParameter), (p, m, t, v, v2) => // 天気予報: 並んだ厚い札に、立体の天気マーク
        {
            var rows = Lines(p); if (rows.Count == 0) return; float cw = MathF.Min(330, 1600f / rows.Count), ch = 460, x0 = -rows.Count * cw / 2 + cw / 2;
            if (p.Background) m.Box(0, 0, -20, rows.Count * cw + 40, ch + 40, 20, m.C(p.Color1, .9f), .05f);
            for (int i = 0; i < rows.Count; i++)
            {
                var c = rows[i].Split([':', '：', ',']); float x = x0 + i * cw, k = Ease(In(m, i * .12f, .4f));
                m.Box(x, 0, 0, cw - 20, ch * k, 30, m.C(0xFFFFFF, .95f), .1f);
                m.Text(c[0].Trim(), 38, x, -ch / 2 + 50, 16, 4, K);
                string wx = c.Length > 1 ? c[1].Trim() : "";
                if (wx.Contains('雨')) { m.Ball(x, -20, 40, 60, 40, 40, m.C(0x9AA3AD)); for (int j = -1; j <= 1; j++) m.Limb(x + j * 30, 30, 40, x + j * 30 - 8, 60, 40, 3, 5, m.C(0x4FB8F2)); }
                else if (wx.Contains('曇')) { m.Ball(x - 25, -10, 40, 50, m.C(0xBFC6D0)); m.Ball(x + 25, 0, 40, 40, m.C(0xBFC6D0)); }
                else { m.Ball(x, -10, 40, 50, m.C(0xFFC93C)); for (int j = 0; j < 8; j++) { float ag = j * MathF.PI / 4; m.Limb(x + MathF.Cos(ag) * 60, -10 + MathF.Sin(ag) * 60, 40, x + MathF.Cos(ag) * 80, -10 + MathF.Sin(ag) * 80, 40, 4, 2, m.C(0xFFC93C)); } }
                m.Text(c.Length > 2 ? c[2].Trim() : "", 50, x, ch / 2 - 80, 16, 6, m.C(0xE53935));
            }
        });
        Q(d, typeof(ReceiptParameter), (p, m, t, v, v2) => // レシート: 少し丸まった白い紙、店名と明細、合計
        {
            var rows = Lines(p); if (rows.Count == 0) return; float k = Ease(In(m, 0, .8f)), bw = 520, bh = (rows.Count + 3) * 60 * k;
            m.Pitch(0, -bh / 2, 0, -.12f, () => m.Box(0, 0, 0, bw, bh, 4, m.C(p.Color1), .02f));
            m.Text(rows[0], 50, 0, -bh / 2 + 60, 8, 4, K);
            long sum = 0;
            for (int i = 1; i < rows.Count; i++) { int ci = rows[i].LastIndexOfAny([':', '：']); string n = ci > 0 ? rows[i][..ci] : rows[i]; long.TryParse(ci > 0 ? new string(rows[i][(ci + 1)..].Where(char.IsDigit).ToArray()) : "0", out long vv); sum += vv; float y = -bh / 2 + 60 + i * 60; if (y > bh / 2 - 60) break; m.Text(n, 34, -100, y, 8, 3, K); m.Text("¥" + vv.ToString("N0"), 34, 150, y, 8, 3, K); }
            m.Text("合計 ¥" + sum.ToString("N0"), 44, 0, bh / 2 - 60, 8, 4, K);
        });

        // ---------------------------------------------------------------- 185 キャラの気持ち
        Q(d, typeof(HeartBreakParameter), (p, m, t, v, v2) => // ハートが割れる: どくどくするハートが、左右に割れて落ちる
        {
            float r = 160, beat = 1 + .08f * MathF.Max(0, MathF.Sin(m.Now * 12)) * (m.Now < .6f ? 1 : 0), br = In(m, .7f, .9f);
            if (br <= 0) { Heart3D(m, 0, 0, 0, r * beat, m.C(p.Color1)); return; }
            float sep = Ease(br) * 80, fall = br * br * 200;
            foreach (int s in Sides) m.Turn(s * sep, fall, s * br * .4f, () => m.Move(s * sep, fall, () =>
            {
                var hp = HeartPts(0, r * .05f, r / 260f); var half = new List<float>();
                for (int i = 0; i < hp.Length; i += 2) if (s < 0 ? hp[i] <= 5 : hp[i] >= -5) { half.Add(hp[i]); half.Add(hp[i + 1]); }
                if (half.Count >= 6) m.Slab(0, r * .5f, m.C(p.Color1), [.. half]);
            }));
        });
        Q(d, typeof(AngerFlameParameter), (p, m, t, v, v2) => // 怒りのオーラ: 3重のゆれる炎の板(奥ほど大きい)
        {
            float hh = 200 + 500 * v, ww = 500;
            for (int L = 0; L < 3; L++)
            {
                var pts = new List<float>(); int n = 14; float lw = ww * (1 - L * .22f), lh = hh * (1 - L * .25f);
                for (int i = 0; i <= n; i++) { float f = i / (float)n, x = (f - .5f) * lw, wob = MathF.Sin(m.Now * 8 + i * 1.7f + L) * 30; pts.Add(x); pts.Add(-lh * (MathF.Sin(f * MathF.PI)) * (i % 2 == 0 ? 1 : .7f) + wob); }
                pts.Add(lw / 2); pts.Add(40); pts.Add(-lw / 2); pts.Add(40);
                m.Slab(-L * -30 - 60, 20, Vector4.Lerp(m.C(p.Color1), m.C(p.Color2), L / 2f) with { W = (.5f + L * .2f) * m.Al }, [.. pts]);
            }
        });
        Q(d, typeof(StarEyesParameter), (p, m, t, v, v2) => // 星の目: ぼんやり光る玉と、脈打つ立体の星2つ
        {
            float gap = 80 + 300 * v, pulse = 1 + .15f * MathF.Sin(t * 8);
            foreach (int s in Sides)
            {
                float x = s * gap / 2;
                m.Ball(x, 0, -20, 110, m.C(p.Color1, .25f));
                float r = 70 * pulse * Ease(In(m, 0, .3f));
                m.Turn(x, 0, m.Now, () => Gem(m, Pts(StarPts(x, 0, r, 5, .45f, 0)), x, 0, 0f, 6, 26, m.C(p.Color1), m.C(p.Color1)));
            }
        });
        Q(d, typeof(SpiralEyesParameter), (p, m, t, v, v2) => // ぐるぐる目: 白い玉の目に、回るうずの管
        {
            float gap = 80 + 300 * v, r = 70;
            foreach (int e in Sides)
            {
                float x = e * gap / 2;
                m.Ball(x, 0, 0, r * 1.05f, r * 1.05f, 40, W);
                var sp = new List<Vector3>(); for (int i = 0; i <= 60; i++) { float f = i / 60f, ag = f * MathF.PI * 6 + m.Now * 6 * e; sp.Add(new(x + MathF.Cos(ag) * r * f * .9f, MathF.Sin(ag) * r * f * .9f, 40 * MathF.Sqrt(1 - f * f * .8f))); }
                m.Tube(sp, 4, m.C(p.Color1));
            }
        });
        Q(d, typeof(SweatSplashParameter), (p, m, t, v, v2) => // 汗: 放射状に飛び散るしずく(とがった玉)
        {
            for (int i = 0; i < p.Count; i++)
            {
                float ph = ((t * 1.4f + DrawKit.H(p.Seed, i) * .3f) % 1 + 1) % 1, ag = -MathF.PI / 2 + (i - (p.Count - 1) / 2f) * .45f, dist = 120 + 180 * Ease(ph);
                float x = MathF.Cos(ag) * dist, y = MathF.Sin(ag) * dist, z = (i % 2 == 0 ? 1 : -1) * 60 * ph;
                var col = m.C(p.Color1, 1 - ph * .6f);
                m.Ball(x, y, z, 22, col); m.Limb(x, y, z, x - MathF.Cos(ag) * 36, y - MathF.Sin(ag) * 36, z, 18, 1, col);
            }
        });
        Q(d, typeof(PaleLinesParameter), (p, m, t, v, v2) => // 青ざめ: 上から下りる青い影と、細い縦の棒
        {
            float bw = 400, bh = 260, k = Ease(In(m, 0, .6f));
            m.Box(0, -bh / 2 + bh * k / 2, -10, bw, bh * k, 6, m.C(p.Color1, .3f), .1f);
            for (int i = 0; i < 9; i++) { float x = -bw / 2 + bw * (i + .5f) / 9, len = bh * k * (.5f + .5f * DrawKit.H(p.Seed, i)); m.Box(x, -bh / 2 + len / 2, 6, 6, len, 6, m.C(p.Color1, .85f), .5f); }
        });
        Q(d, typeof(SnotBubbleParameter), (p, m, t, v, v2) => // 鼻ちょうちん: ふくらんだりしぼんだりする透けた玉
        {
            float k = .55f + .45f * (.5f + .5f * MathF.Sin(t * 2));
            if (p.Variant >= 2 && In(m, 2.5f, .15f) >= 1) { for (int i = 0; i < 8; i++) { float ag = i * MathF.PI / 4, d0 = 100 + (m.Now - 2.65f) * 200; m.Ball(MathF.Cos(ag) * d0, MathF.Sin(ag) * d0, 0, 8, m.C(p.Color1, MathF.Max(0, 1 - (m.Now - 2.65f) * 2))); } return; }
            m.Ball(100 * k, -20, 0, 100 * k, m.C(p.Color1, .55f));
            m.Ball(60 * k, -60 * k, 70 * k, 20 * k, 14 * k, 6, m.C(0xFFFFFF, .7f));
        });
        Q(d, typeof(SoulLeavingParameter), (p, m, t, v, v2) => // 魂が抜ける: ふわっと上る半透明のおばけ(頭・しっぽ)
        {
            float rise = Ease(In(m, 0, 2)) * 300, x = MathF.Sin(m.Now * 2) * 20, y = -rise, r = 80;
            m.Ball(x, y, 0, r, r, r * .8f, m.C(p.Color1, .7f));
            m.Tube([new(x - 20, y + r * .6f, 0), new(x + 30 * MathF.Sin(m.Now * 4), y + r * 1.5f, -10), new(x - 10, y + r * 2.4f, -20)], 18, m.C(p.Color1, .6f));
            foreach (int s in Sides) m.Ball(x + s * 25, y - 10, r * .75f, 9, 13, 5, m.C(0x223344, .8f));
        });
        Q(d, typeof(CupidArrowParameter), (p, m, t, v, v2) => // 恋の矢: 飛んできてハートに刺さる、羽根の付いた矢
        {
            float r = 130, hit = In(m, .5f, .01f), boing = hit > 0 ? 1 + .2f * MathF.Exp(-(m.Now - .5f) * 6) * MathF.Sin((m.Now - .5f) * 30) : 1;
            Heart3D(m, 0, 0, 0, r * boing, m.C(p.Color1));
            float k = Math.Clamp(m.Now / .5f, 0, 1); var dir = Vector2.Normalize(new Vector2(1, -.4f)); var tip = -dir * (500 * (1 - k)) + dir * 40;
            var tail = tip - dir * 380; var gold = m.C(p.Color2);
            m.Limb(tail.X, tail.Y, 60 * (1 - k), tip.X, tip.Y, 60 * (1 - k), 5, 5, gold);
            m.Limb(tip.X - dir.X * 30, tip.Y - dir.Y * 30, 60 * (1 - k), tip.X + dir.X * 20, tip.Y + dir.Y * 20, 60 * (1 - k), 14, 1, gold);
            m.Tri3D(new(tail, 60 * (1 - k)), new(tail + new Vector2(30, -30), 60 * (1 - k)), new(tail + dir * 60, 60 * (1 - k)), m.C(p.Color1));
        });
        Q(d, typeof(KiranParameter), (p, m, t, v, v2) => // キラーン: ぱっと光る玉と、立体の十字の光
        {
            float k = MathF.Sin(In(m, .1f, .5f) * MathF.PI); if (k <= 0) return;
            m.Ball(0, 0, 0, 90 * k, m.C(p.Color1, .5f * k));
            m.Turn(0, 0, k * .8f, () => Gem(m, Pts(StarPts(0, 0, 260 * k, 4, .08f, 0)), 0, 0, 0f, 2, 30 * k, m.C(p.Color1, k), m.C(p.Color1, k)));
        });
        Q(d, typeof(FluffyFlowersParameter), (p, m, t, v, v2) => // ほわほわお花: まわりに浮かぶ、立体の花(花びらとしべ)
        {
            var cols = new[] { p.Color1, p.Color2, p.Color3 };
            for (int i = 0; i < p.Count; i++)
            {
                float ag = i * MathF.Tau / p.Count + DrawKit.H(p.Seed, i) * .4f, rr = 280 + 80 * DrawKit.H(p.Seed, i, 1), x = MathF.Cos(ag) * rr * 1.3f, y = MathF.Sin(ag) * rr * .8f, z = MathF.Sin(ag * 2) * 80;
                float k = Ease(In(m, i * .08f, .4f)), bob = MathF.Sin(m.Now * 2 + i) * 10;
                m.Move(0, bob, z, () => { for (int j = 0; j < 5; j++) { float pa = j * MathF.Tau / 5 + m.Now * .5f; m.Ball(x + MathF.Cos(pa) * 26 * k, y + MathF.Sin(pa) * 26 * k, 0, 22 * k, 22 * k, 10, m.C(cols[i % 3])); } m.Ball(x, y, 6, 14 * k, m.C(0xFFF59D)); });
            }
        });
        Q(d, typeof(ShakeLinesParameter), (p, m, t, v, v2) => // ガタガタ: 左右でふるえる、3本ずつの弧の管
        {
            float gap = 150 + 400 * v, jit = MathF.Sin(t * 50) * 6;
            foreach (int s in Sides) for (int k = 0; k < 3; k++)
                {
                    float x = s * (gap / 2 + k * 30) + jit * s, r = 60 + k * 30;
                    m.Tube(ArcPts3(x - s * r, 0, r, s > 0 ? -.6f : MathF.PI - .6f, s > 0 ? .6f : MathF.PI + .6f, 0, 10), 4, m.C(p.Color1));
                }
        });
        Q(d, typeof(OnomatopoeiaStampParameter), (p, m, t, v, v2) => // 擬音スタンプ: ぶ厚いふち取りの文字が、ドンと押される
        {
            var it = Str(p).Split('\n'); float fs = 220;
            for (int i = 0; i < it.Length; i++)
            {
                float k0 = In(m, i * .2f, .2f); if (k0 <= 0) continue;
                float k = p.Variant == 2 ? 1 + .06f * MathF.Sin(t * 30) : p.Variant == 3 ? Ease(k0) : (k0 < 1 ? 2.5f - 1.5f * Ease(k0) : 1);
                float y = (i - (it.Length - 1) / 2f) * fs * 1.05f;
                m.Turn(0, y, -.12f + i * .1f, () => m.Text(it[i], fs * k, 0, y, 20, 50, m.C(p.Color1), 0, m.C(p.Color2), 14));
            }
        });
        Q(d, typeof(TsukkomiHandParameter), (p, m, t, v, v2) => // ツッコミの手: 横から飛んでくる立体の腕と手のひら、衝撃の光
        {
            float k = In(m, 0, .18f), back = MathF.Max(0, In(m, .6f, .3f)), x = -900 * (1 - Ease(k)) - back * 900;
            var skin = m.C(p.Color1);
            m.Box(x - 360, 0, 0, 480, 100, 100, m.C(0x3F51B5), .4f);
            m.Box(x - 60, 0, 0, 140, 180, 60, skin, .4f);
            m.Limb(x + 20, -80, 0, x + 60, -110, 0, 18, 15, skin);
            if (k >= 1 && back <= 0) { m.Turn(x + 80, 0, 0, () => Gem(m, Pts(StarPts(x + 140, 0, 120, 8, .5f, 0)), x + 140, 0, 0f, 4, 20, m.C(p.Color2), m.C(p.Color2))); }
        });

        // ---------------------------------------------------------------- 186 自然
        QS(d, typeof(RainSplashParameter), (p, m, t, v, v2) => // 雨粒がはねる: 落ちる粒、地面の水たまりと広がる輪
        {
            float gy = -m.H / 2 + m.H * v;
            m.Box(0, gy + 10, 0, m.W, 20, 600, m.C(0x3A4A5A, .6f), .02f);
            for (int i = 0; i < p.Count; i++)
            {
                float per = .6f + DrawKit.H(p.Seed, i) * .5f, ph = ((t / per + DrawKit.H(p.Seed, i, 1)) % 1 + 1) % 1, x = (DrawKit.H(p.Seed, i, 2) - .5f) * m.W, z = (DrawKit.H(p.Seed, i, 3) - .5f) * 500;
                if (ph < .7f) { float y = gy - (1 - ph / .7f) * 700; m.Limb(x, y - 30, z, x, y, z, 2, 5, m.C(p.Color1, .8f)); }
                else { float f = (ph - .7f) / .3f; m.Pitch(x, gy, z, MathF.PI / 2, () => m.Torus(x, gy, z, 10 + 50 * f, 2, m.C(p.Color1, 1 - f))); for (int j = 0; j < 3; j++) m.Ball(x + (j - 1) * 20 * f, gy - 40 * f * (1 - f) * 4, z, 4, m.C(p.Color1, 1 - f)); }
            }
        });
        QS(d, typeof(FrostSpreadParameter), (p, m, t, v, v2) => // 霜: 画面のふちから伸びる、とがった氷の結晶
        {
            float k = v * Ease(In(m, 0, 2.5f)), reach = MathF.Min(m.W, m.H) * .5f * k;
            for (int i = 0; i < 40; i++)
            {
                int side = i % 4; float f = DrawKit.H(p.Seed, i), len = reach * (.4f + .6f * DrawKit.H(p.Seed, i, 1));
                Vector2 st = side switch { 0 => new((f - .5f) * m.W, -m.H / 2), 1 => new((f - .5f) * m.W, m.H / 2), 2 => new(-m.W / 2, (f - .5f) * m.H), _ => new(m.W / 2, (f - .5f) * m.H) };
                Vector2 dir = side switch { 0 => new(0, 1), 1 => new(0, -1), 2 => new(1, 0), _ => new(-1, 0) };
                dir = Vector2.Normalize(dir + new Vector2(DrawKit.H(p.Seed, i, 2) - .5f, DrawKit.H(p.Seed, i, 3) - .5f) * .8f);
                var e = st + dir * len; m.Limb(st.X, st.Y, 0, e.X, e.Y, 10, 10, 1, m.C(p.Color1, .8f));
            }
        });
        QS(d, typeof(IciclesParameter), (p, m, t, v, v2) => // つらら: 雪の積もった軒と、垂れ下がる透けた氷の円すい
        {
            float top = -m.H / 2 + m.H * v, grow = Ease(In(m, 0, 1.5f));
            m.Box(0, top + 10, 0, m.W, 28, 160, m.C(0xFFFFFF, .95f), .3f);
            for (int i = 0; i < p.Count; i++)
            {
                float x = -m.W / 2 + m.W * (i + .3f + DrawKit.H(p.Seed, i) * .4f) / p.Count, len = (80 + 260 * DrawKit.H(p.Seed, i, 1)) * grow, z = (DrawKit.H(p.Seed, i, 2) - .5f) * 100;
                m.Lathe(x, z, m.C(p.Color1, .85f), 0, top + 24 + len, 18 + 10 * DrawKit.H(p.Seed, i, 3), top + 24, 0, top + 24);
                if ((m.Now * .7f + DrawKit.H(p.Seed, i, 4)) % 1 < .3f) m.Ball(x, top + 30 + len + ((m.Now * .7f + DrawKit.H(p.Seed, i, 4)) % 1) * 300, z, 5, m.C(p.Color1, .8f));
            }
        });
        QS(d, typeof(RainRadarParameter), (p, m, t, v, v2) => // 雨雲レーダー: 地図の板と陸地、流れる色つきの雨雲(高さで強さ)
        {
            if (p.Background) { m.Box(0, 0, -20, m.W, m.H, 10, m.C(0xD1DBE6), .01f); m.Slab(-10, 20, m.C(0xEDEBDB), -.2f * m.W, -.45f * m.H, .1f * m.W, -.3f * m.H, .25f * m.W, 0, .15f * m.W, .35f * m.H, -.1f * m.W, .45f * m.H, -.3f * m.W, .1f * m.H); }
            for (int i = 0; i < p.Count; i++)
            {
                float x = (DrawKit.H(p.Seed, i) - .5f) * m.W + m.Now * 30, y = (DrawKit.H(p.Seed, i, 1) - .5f) * m.H, s = 60 + 120 * DrawKit.H(p.Seed, i, 2), lv = DrawKit.H(p.Seed, i, 3);
                var col = lv > .7f ? m.C(0xE53935, .7f) : lv > .4f ? m.C(0xFDD835, .7f) : m.C(0x42A5F5, .6f);
                m.Ball(x, y, 10, s, s * .8f, 20 + 60 * lv, col);
            }
        });
        QS(d, typeof(SunriseSunsetParameter), (p, m, t, v, v2) => // 日の出: 空の板、地平線の厚い陸、上ってくる大きな太陽
        {
            float k = In(m, 0, 6); if (p.Variant >= 2) k = 1 - k;
            SkyWall(m, Vector4.Lerp(m.C(p.Color2), m.C(0x6EC6FF), k), Vector4.Lerp(m.C(0x3A2A5C), m.C(p.Color1), MathF.Min(1, k * 1.6f)), -500);
            float hz = m.H * .2f;
            m.Ball(0, hz + 150 - k * (m.H * .5f), -300, 150, m.C(0xFFD27A));
            m.Box(0, hz + m.H * .2f, -100, m.W * 1.2f, m.H * .4f, 300, m.C(0x1E2440), .02f);
        });
        QS(d, typeof(SeaOfCloudsParameter), (p, m, t, v, v2) => // 雲海: 遠くの山の三角すいと、手前に流れるもこもこの雲
        {
            if (p.Background) SkyWall(m, m.C(0x5B8BD9), m.C(p.Color2), -600);
            for (int i = 0; i < 3; i++) { float px = -m.W / 2 + m.W * (.2f + i * .33f), z = -300 - i * 50; m.Lathe(px, z, m.C(0x3A4A6A), 0, -m.H * (.05f + i * .04f), 300, m.H * .15f, 0, m.H * .15f); }
            for (int i = 0; i < 26; i++) { float x = ((DrawKit.H(p.Seed, i) * m.W + m.Now * 20 * (1 + i % 3)) % (m.W + 400)) - m.W / 2 - 200, y = m.H * (.1f + .2f * DrawKit.H(p.Seed, i, 1)), z = -200 + 400 * DrawKit.H(p.Seed, i, 2); m.Ball(x, y, z, 160, 90, 120, m.C(p.Color1, .9f)); }
        });
        QS(d, typeof(SusukiFieldParameter), (p, m, t, v, v2) => // ススキ・稲穂: 奥行きにも並んだ、風にゆれる穂
        {
            bool rice = p.Variant >= 2;
            for (int i = 0; i < p.Count; i++)
            {
                float x = -m.W / 2 + m.W * (i + DrawKit.H(p.Seed, i)) / p.Count, z = (DrawKit.H(p.Seed, i, 5) - .5f) * 400, hh = m.H * (.25f + .25f * DrawKit.H(p.Seed, i, 1)), sway = MathF.Sin(t * 1.5f - x * .004f) * 40 + 20, by = m.H / 2;
                var top = new Vector3(x + sway, by - hh, z);
                m.Tube([new(x, by, z), new(x + sway * .4f, by - hh * .5f, z), top], 2.5f, m.C(p.Color2));
                m.Limb(top.X, top.Y, z, top.X + sway * .6f + (rice ? 10 : 30), top.Y + (rice ? 60 : -10), z, rice ? 9 : 12, 3, m.C(p.Color1));
            }
        });
        QS(d, typeof(SnowPileParameter), (p, m, t, v, v2) => // 雪が積もる: 落ちる雪の玉と、だんだん高くなる雪の地面
        {
            float lv = v * m.H * Math.Clamp(m.Now / 8f, 0, 1);
            for (int i = 0; i < p.Count; i++) { float sp = 80 + 80 * DrawKit.H(p.Seed, i), y = ((DrawKit.H(p.Seed, i, 1) * m.H + m.Now * sp) % m.H) - m.H / 2, x = (DrawKit.H(p.Seed, i, 2) - .5f) * m.W + MathF.Sin(m.Now + i) * 20, z = (DrawKit.H(p.Seed, i, 3) - .5f) * 400; m.Ball(x, y, z, 6 + 6 * DrawKit.H(p.Seed, i, 4), m.C(p.Color1)); }
            if (lv > 1) for (int i = 0; i < 9; i++) m.Ball(-m.W / 2 + m.W * i / 8f, m.H / 2, 0, m.W / 7, lv, 260, m.C(p.Color1));
        });
        QS(d, typeof(DandelionParameter), (p, m, t, v, v2) => // タンポポの綿毛: ふわふわ飛ぶ、軸と放射の毛の綿毛
        {
            for (int i = 0; i < p.Count; i++)
            {
                float f = ((DrawKit.H(p.Seed, i, 1) + m.Now * (.05f + .06f * DrawKit.H(p.Seed, i))) % 1 + 1) % 1, x = -m.W / 2 - 100 + (m.W + 200) * f, y = m.H * (-.2f + .7f * DrawKit.H(p.Seed, i, 2)) - f * m.H * .4f, z = (DrawKit.H(p.Seed, i, 3) - .5f) * 500, rot = MathF.Sin(m.Now + i) * .3f;
                m.Turn(x, y, rot, () => { m.Limb(x, y, z, x, y + 50, z, 1.2f, 1.2f, m.C(p.Color1, .8f)); for (int j = 0; j < 10; j++) { float ag = j * MathF.Tau / 10; m.Limb(x, y, z, x + MathF.Cos(ag) * 30, y + MathF.Sin(ag) * 30 - 10, z + MathF.Sin(ag * 2) * 15, .8f, .8f, m.C(p.Color1, .7f)); } });
            }
        });
        Q(d, typeof(HydrangeaRainParameter), (p, m, t, v, v2) => // あじさい: 葉と、小花の玉が集まった丸い花、伝う雨粒
        {
            for (int f = 0; f < p.Count; f++)
            {
                float cx = (f - (p.Count - 1) / 2f) * 320, cy = (f % 2) * 80;
                foreach (int s in Sides) m.Turn(cx, cy + 120, s * .6f, () => m.Ball(cx + s * 100, cy + 140, -40, 110, 50, 14, m.C(0x4E8F4A)));
                for (int k = 0; k < 40; k++) { float ag = k * 2.39996f, r = 140 * MathF.Sqrt(k / 40f), x = cx + MathF.Cos(ag) * r, y = cy + MathF.Sin(ag) * r * .8f, z = MathF.Sqrt(MathF.Max(0, 140 * 140 - r * r)) * .7f; m.Ball(x, y, z, 22, Vector4.Lerp(m.C(p.Color1), m.C(p.Color2), DrawKit.H(p.Seed, f * 50 + k))); }
                float dr = ((m.Now * .5f + f * .3f) % 1); m.Ball(cx + 60, cy - 120 + dr * 300, 120, 7, 10, 7, m.C(0xBFE6FF, 1 - dr));
            }
        });
        QS(d, typeof(SunsetDragonflyParameter), (p, m, t, v, v2) => // 夕焼けとトンボ: 夕日・地面、羽ばたいて飛ぶ立体のトンボ
        {
            if (p.Background) { SkyWall(m, m.C(p.Color1), m.C(p.Color2), -500); m.Ball(m.W * .2f, m.H * .25f, -450, 140, m.C(0xFFD999)); m.Box(0, m.H * .43f, -100, m.W * 1.2f, m.H * .15f, 400, m.C(0x40261F), .02f); }
            for (int i = 0; i < p.Count; i++)
            {
                float tt = m.Now * .3f + i * 1.3f, x = MathF.Sin(tt * 1.3f) * m.W * .35f, y = MathF.Sin(tt * 2.1f) * m.H * .2f - m.H * .1f, z = MathF.Cos(tt) * 200, flap = MathF.Sin(m.Now * 40 + i) * .4f;
                m.Limb(x - 50, y, z, x + 40, y, z, 4, 6, m.C(0x8A2A1A)); m.Ball(x + 46, y, z, 9, m.C(0x8A2A1A));
                foreach (int s in Sides) foreach (int r in Sides) m.Pitch(x, y, z, s * (.2f + flap), () => m.Ball(x + r * 12, y, z + s * 40, 10, 4, 40, m.C(0xFFFFFF, .5f)));
            }
        });
        QS(d, typeof(CicadaCloudParameter), (p, m, t, v, v2) => // 入道雲: 青空と、もくもく盛り上がる大きな雲
        {
            if (p.Background) SkyWall(m, m.C(p.Color1), m.C(0xBFE6FF), -700);
            for (int i = 0; i < 14; i++) { float f = i / 13f, x = m.W * .05f + (DrawKit.H(p.Seed, i) - .5f) * 700 * (1 - f * .5f), y = m.H * .12f - f * 520, z = -400 + (DrawKit.H(p.Seed, i, 2) - .5f) * 200, r = (120 + 80 * DrawKit.H(p.Seed, i, 1)) * (1 - f * .4f); m.Ball(x, y, z, r, m.C(p.Color2)); }
        });
        QS(d, typeof(MilkyWayParameter), (p, m, t, v, v2) => // 天の川: 夜空と、ななめに流れる星の帯(奥行きにも散らばる)
        {
            if (p.Background) SkyWall(m, m.C(p.Color2), m.C(0x1C2552), -800);
            int keep = m.Seg; m.Seg = 6;
            for (int i = 0; i < 220; i++)
            {
                float f = DrawKit.H(p.Seed, i), off = (DrawKit.H(p.Seed, i, 1) + DrawKit.H(p.Seed, i, 2) - 1) * 220;
                var a0 = new Vector2(-m.W * .6f, m.H * .4f); var a1 = new Vector2(m.W * .6f, -m.H * .4f); var dir = Vector2.Normalize(a1 - a0); var nrm = new Vector2(-dir.Y, dir.X);
                var q = Vector2.Lerp(a0, a1, f) + nrm * off; float tw = .5f + .5f * MathF.Sin(m.Now * 3 + i);
                m.Ball(q.X, q.Y, -600 + DrawKit.H(p.Seed, i, 3) * 500, 2 + 3 * DrawKit.H(p.Seed, i, 4), Vector4.Lerp(m.C(p.Color1), W, tw) with { W = (.4f + .6f * tw) * m.Al });
            }
            m.Seg = keep;
        });
        QS(d, typeof(BeachWavesParameter), (p, m, t, v, v2) => // 砂浜と波: 砂の地面、寄せては返す海の板と白い波がしら
        {
            float ph = .5f - .5f * MathF.Cos(m.Now * .9f), shore = -m.H / 2 + m.H * (.45f + .12f * ph);
            m.Quad3D(new(-m.W, m.H / 2, 200), new(m.W, m.H / 2, 200), new(m.W, -m.H / 2, -400), new(-m.W, -m.H / 2, -400), m.C(p.Color2));
            float zs = -400 + 600 * ((shore + m.H / 2) / m.H);
            m.Quad3D(new(-m.W, -m.H / 2, -398), new(m.W, -m.H / 2, -398), new(m.W, shore, zs + 2), new(-m.W, shore, zs + 2), m.C(p.Color1, .9f));
            m.Tube([.. Enumerable.Range(0, 41).Select(i => new Vector3(-m.W / 2 + m.W * i / 40f, shore + MathF.Sin(i * .7f + m.Now * 2) * 10, zs + 6))], 8, m.C(0xFFFFFF, .9f));
        });
        QS(d, typeof(MountainParallaxParameter), (p, m, t, v, v2) => // 山並み: 奥ほど薄い、何層もの山の板(本当に奥へ並ぶ)
        {
            if (p.Background) SkyWall(m, m.C(0x88C1E0), m.C(p.Color2), -1200);
            int n = Math.Clamp(p.Count, 1, 8);
            for (int L = 0; L < n; L++)
            {
                float depth = (L + 1f) / n, z = -1000 + depth * 900, baseY = -m.H / 2 + m.H * (.45f + .4f * depth), sh = m.Now * 30 * depth * depth * 6;
                var pts = new List<Vector2> { new(-m.W, m.H) };
                for (int i = 0; i <= 30; i++) { float x = -m.W + 2 * m.W * i / 30f, y = baseY - (MathF.Sin((x + sh) * .004f + L * 2) * .5f + .5f) * m.H * .18f - MathF.Sin((x + sh) * .011f + L) * 30; pts.Add(new(x, y)); }
                pts.Add(new(m.W, m.H));
                pts.Reverse();
                m.Slab(pts, z, 20, Vector4.Lerp(m.C(p.Color2), m.C(p.Color1), depth));
            }
        });
        QS(d, typeof(BambooForestParameter), (p, m, t, v, v2) => // 竹林: 奥行きにも立ち並ぶ、節のある竹と葉
        {
            if (p.Background) SkyWall(m, m.C(p.Color2), m.C(0x8FB86B), -900);
            for (int i = 0; i < p.Count; i++)
            {
                float depth = DrawKit.H(p.Seed, i, 4), x = -m.W / 2 + m.W * (i + DrawKit.H(p.Seed, i)) / p.Count, z = -700 + depth * 800, bw = 10 + 15 * depth, sway = MathF.Sin(m.Now * .8f + i) * 20;
                var col = Vector4.Lerp(m.C(p.Color2), m.C(p.Color1), .4f + .6f * depth);
                m.Turn(x, m.H / 2, sway * .002f, () =>
                {
                    m.Cyl(x, 0, z, bw, m.H * 1.1f, col, .1f);
                    for (int k = 0; k < 6; k++) m.Cyl(x, -m.H / 2 + k * m.H / 5, z, bw * 1.15f, 6, Sculpt.Dark(col, .8f), .3f);
                    for (int k = 0; k < 3; k++) m.Ball(x + 40, -m.H / 2 + 100 + k * 150, z, 50, 10, 6, col);
                });
            }
        });
        Q(d, typeof(WaterfallParameter), (p, m, t, v, v2) => // 滝: 岩の崖、流れ落ちる水の筋、下のしぶき
        {
            float ww = 150 + 700 * v, top = -450, bot = 300;
            foreach (int s in Sides) m.Box(s * (ww / 2 + 150), (top + bot) / 2, -60, 300, bot - top + 100, 200, m.C(0x5A5A60), .2f);
            m.Box(0, (top + bot) / 2, -20, ww, bot - top, 20, m.C(p.Color2, .9f), .05f);
            for (int i = 0; i < 40; i++) { float x = -ww / 2 + ww * DrawKit.H(p.Seed, i), len = 60 + 140 * DrawKit.H(p.Seed, i, 1), y = top + ((m.Now * 500 + DrawKit.H(p.Seed, i, 2) * (bot - top)) % (bot - top)); m.Box(x, y, 0, 4, len, 4, m.C(p.Color1, .8f), .5f); }
            for (int i = 0; i < 16; i++) { float f = ((m.Now * .8f + i / 16f) % 1); m.Ball((DrawKit.H(p.Seed, i + 70) - .5f) * ww, bot - f * 120, f * 80, 20 + 30 * f, m.C(p.Color1, .6f * (1 - f))); }
        });

        // ---------------------------------------------------------------- 187 季節の行事
        Q(d, typeof(HanamiParameter), (p, m, t, v, v2) => // お花見: 枝分かれした幹と、ふんわりした花のかたまり、舞う花びら
        {
            var br = m.C(p.Color2);
            m.Lathe(0, 0, br, 0, 330, 40, 330, 25, 80, 0, 80);
            m.Tube([new(0, 80, 0), new(80, -40, 20), new(120, -60, 30)], 18, br); m.Tube([new(0, 80, 0), new(-70, -60, -20), new(-110, -80, -30)], 18, br); m.Tube([new(0, 80, 0), new(10, -40, 0)], 20, br);
            for (int i = 0; i < 26; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * 560, y = -100 - DrawKit.H(p.Seed, i, 1) * 260, z = (DrawKit.H(p.Seed, i, 2) - .5f) * 300; m.Ball(x, y, z, 70 + 40 * DrawKit.H(p.Seed, i, 3), m.C(p.Color1, .95f)); }
            for (int i = 0; i < 12; i++) { float f = ((m.Now * .3f + i / 12f) % 1), x = (DrawKit.H(p.Seed, i + 40) - .5f) * 700 + MathF.Sin(m.Now + i) * 40, y = -200 + f * 550; m.Turn(x, y, m.Now * 3 + i, () => m.Ball(x, y, 100, 10, 6, 2, m.C(p.Color1))); }
        });
        Q(d, typeof(SetsubunParameter), (p, m, t, v, v2) => // 節分: 角の生えた鬼の顔(震える)と、飛んでくる豆
        {
            float sh = MathF.Sin(m.Now * 40) * 5;
            m.Move(sh, 0, () =>
            {
                m.Ball(0, 0, 0, 220, 220, 180, m.C(p.Color1));
                foreach (int s in Sides) { m.Lathe(s * 120, 0, m.C(0xFFF2D9), 0, -330, 30, -220, 0, -220); m.Ball(s * 80, -40, 160, 34, W); m.Ball(s * 80, -36, 192, 14, K); }
                m.Box(0, 90, 160, 160, 40, 30, W, .3f);
            });
            for (int i = 0; i < 8; i++) { float f = ((m.Now * 1.2f + i / 8f) % 1); float x = -500 + 500 * f + (i % 3 - 1) * 40, y = 200 - 260 * f + (i % 2) * 60, z = 300 * (1 - f); m.Ball(x, y, z, 16, m.C(p.Color2)); }
        });
        Q(d, typeof(HinamatsuriParameter), (p, m, t, v, v2) => // ひな祭り: 赤い3段の台と、お内裏様・おひな様(着物の円すいと頭)
        {
            for (int i = 0; i < 3; i++) { float y = -120 + i * 130, ww = 600 + i * 200; m.Box(0, y + 65, -i * 60, ww, 130, 200 + i * 120, m.C(p.Color1), .05f); }
            foreach (int e in Sides)
            {
                float x = e * 130; var robe = e < 0 ? m.C(0x283593) : m.C(p.Color2);
                m.Lathe(x, -20, robe, 0, -20, 90, -20, 50, -170, 0, -170);
                m.Ball(x, -205, -20, 38, m.C(0xFFF2E6)); m.Ball(x, -225, -30, 40, 34, 36, K);
            }
        });
        Q(d, typeof(KoinoboriParameter), (p, m, t, v, v2) => // こいのぼり: 柱と玉、風に泳ぐ筒形の鯉(目・うろこ)
        {
            m.Cyl(-450, 50, 0, 8, 900, m.C(0x998066), .2f); m.Ball(-450, -420, 0, 26, m.C(0xFFD94D));
            var cols = new[] { p.Color1, p.Color2, p.Color3 };
            for (int i = 0; i < 3; i++)
            {
                float y = -330 + i * 190, len = 520 - i * 80, r = 70 - i * 10;
                var pts = new List<Vector2>(); for (int j = 0; j <= 12; j++) { float f = j / 12f; pts.Add(new(r * (1 - f * .3f), f * len)); }
                m.Turn(-430, y, -MathF.PI / 2, () => m.Yaw(-430, y, 0, MathF.Sin(m.Now * 3 + i) * .15f, () => m.Lathe(-430, 0, m.C(cols[i]), [.. pts.Select(q => new Vector2(q.X * (1 + .1f * MathF.Sin(m.Now * 5 + q.Y * .02f)), y + q.Y))])));
                m.Ball(-430 + 50, y, r * .7f, 22, W); m.Ball(-430 + 50, y, r * .7f + 14, 10, K);
            }
        });
        QS(d, typeof(FestivalLanternsParameter), (p, m, t, v, v2) => // ちょうちん: たるんだ綱に下がる、光る丸いちょうちん(ゆれる)
        {
            float y0 = -m.H / 2 + m.H * .3f;
            var rope = Enumerable.Range(0, 41).Select(i => { float f = i / 40f; return new Vector3(-m.W / 2 + m.W * f, y0 + MathF.Sin(f * MathF.PI) * 80, 0); }).ToList();
            m.Tube(rope, 3, m.C(0x33261A));
            for (int i = 1; i < 8; i++)
            {
                var q = rope[i * 5]; float sw = MathF.Sin(m.Now * 2 + i) * .1f;
                m.Turn(q.X, q.Y, sw, () =>
                {
                    m.Ball(q.X, q.Y + 90, 0, 60, 80, 60, m.C(p.Color1));
                    m.Cyl(q.X, q.Y + 15, 0, 34, 14, K, .3f); m.Cyl(q.X, q.Y + 165, 0, 34, 14, K, .3f);
                    m.Ball(q.X, q.Y + 90, 0, 90, m.C(0xFFD27A, .12f));
                });
            }
        });
        Q(d, typeof(JugoyaParameter), (p, m, t, v, v2) => // 十五夜: 光る大きな月と影のうさぎ、三方に積んだお団子、すすき
        {
            m.Ball(150, -150, -200, 330, m.C(p.Color1, .25f));
            m.Ball(150, -150, -200, 250, m.C(p.Color1));
            m.Ball(130, -110, 40, 70, 50, 10, m.C(0xD9C88A));
            m.Box(-250, 300, 0, 260, 40, 200, m.C(0xC8A070), .2f); m.Box(-250, 250, 0, 200, 60, 160, m.C(0xD9B07E), .1f);
            int k = 0; for (int r = 0; r < 3; r++) for (int c = 0; c <= 2 - r; c++) { m.Ball(-250 + (c - (2 - r) / 2f) * 60, 190 - r * 50, 0, 30, W); k++; }
            for (int i = 0; i < 3; i++) m.Tube([new(-420 + i * 20, 300, -40), new(-440 + i * 30, 50, -40), new(-410 + i * 40, -100, -40)], 3, m.C(0xC9B38A));
        });
        Q(d, typeof(ValentineParameter), (p, m, t, v, v2) => // バレンタイン: チョコのハート(2段)と、リボン
        {
            float k = Ease(In(m, 0, .4f));
            Heart3D(m, 0, 0, 0, 220 * k, m.C(p.Color1));
            Heart3D(m, 0, 0, 40, 180 * k, Sculpt.Dark(m.C(p.Color1), 1.25f));
            m.Box(0, 0, 70, 30 * k, 400 * k, 20, m.C(p.Color2), .4f); m.Box(0, -40, 70, 400 * k, 30 * k, 20, m.C(p.Color2), .4f);
            foreach (int s in Sides) m.Ball(s * 40 * k, -80 * k, 80, 40 * k, 26 * k, 14, m.C(p.Color2));
        });
        QS(d, typeof(GraduationEntranceParameter), (p, m, t, v, v2) => // 卒業・入学: 空、石の門柱、桜の木、舞う花びら
        {
            if (p.Background) SkyWall(m, m.C(0x8FD3FF), m.C(0xE8F6FF), -700);
            foreach (int e in Sides) { m.Box(e * 420, 100, 0, 100, 600, 100, m.C(0xB8B3AD), .05f); m.Box(e * 420, -210, 0, 130, 30, 130, m.C(0xA09A92), .1f); }
            for (int i = 0; i < 20; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W, y = -m.H / 2 + 100 + DrawKit.H(p.Seed, i, 1) * 250, z = -300 + DrawKit.H(p.Seed, i, 2) * 200; m.Ball(x, y, z, 90, m.C(p.Color1, .95f)); }
            for (int i = 0; i < 16; i++) { float f = ((m.Now * .25f + i / 16f) % 1); m.Ball((DrawKit.H(p.Seed, i + 50) - .5f) * m.W, -m.H / 2 + f * m.H, 100, 8, 5, 2, m.C(p.Color1)); }
        });
        Q(d, typeof(BirthdayCakeParameter), (p, m, t, v, v2) => // 誕生日ケーキ: お皿、2段の丸いケーキ、クリーム、ろうそくとゆれる炎
        {
            m.Cyl(0, 330, 0, 420, 14, m.C(0xE6E6F2), .3f);
            m.Cyl(0, 220, 0, 340, 200, m.C(p.Color1), .1f); m.Cyl(0, 60, 0, 240, 140, m.C(p.Color1), .1f);
            for (int i = 0; i < 16; i++) { float ag = i * MathF.Tau / 16; m.Ball(MathF.Cos(ag) * 330, 120, MathF.Sin(ag) * 330, 26, m.C(p.Color2)); }
            int n = Math.Clamp(p.Count, 1, 12);
            for (int i = 0; i < n; i++) { float ag = i * MathF.Tau / n, x = MathF.Cos(ag) * 160, z = MathF.Sin(ag) * 160; m.Cyl(x, -40, z, 8, 70, m.C(0x80CBC4), .3f); float fl = 1 + .2f * MathF.Sin(m.Now * 12 + i); m.Ball(x, -95, z, 10, 18 * fl, 10, m.C(0xFFC93C)); }
        });
        Q(d, typeof(WeddingParameter), (p, m, t, v, v2) => // 結婚式: 左右にゆれる2つの金の鐘とリボン
        {
            foreach (int e in Sides)
            {
                float sw = MathF.Sin(m.Now * 3 + (e > 0 ? MathF.PI : 0)) * .35f, px = e * 120;
                m.Turn(px, -280, sw, () =>
                {
                    m.Lathe(px, 0, m.C(p.Color1), 0, -280, 20, -280, 50, -220, 70, -110, 110, -70, 120, -60, 0, -60);
                    m.Ball(px, -50, 0, 18, m.C(p.Color1));
                });
            }
            m.Ball(0, -300, 30, 60, 30, 20, m.C(p.Color2)); foreach (int s in Sides) m.Tube([new(0, -300, 30), new(s * 60, -240, 40), new(s * 40, -150, 40)], 8, m.C(p.Color2));
        });
        QS(d, typeof(GoldScreenCloudParameter), (p, m, t, v, v2) => // 金屏風: 山折り谷折りの6枚の金の板と、流れる白い瑞雲
        {
            int panels = 6;
            for (int i = 0; i < panels; i++)
            {
                float x0 = -m.W / 2 + m.W * i / panels, x1 = -m.W / 2 + m.W * (i + 1) / panels, z0 = i % 2 == 0 ? -200 : -100, z1 = i % 2 == 0 ? -100 : -200;
                m.Quad3D(new(x0, -m.H / 2, z0), new(x1, -m.H / 2, z1), new(x1, m.H / 2, z1), new(x0, m.H / 2, z0), Vector4.Lerp(m.C(p.Color1), m.C(0xFFF2B0), i % 2 == 0 ? .25f : 0));
            }
            for (int i = 0; i < 4; i++) { float x = ((m.Now * 30 + i * m.W / 4) % (m.W + 600)) - m.W / 2 - 300, y = -m.H / 3 + i * m.H / 5; for (int k = 0; k < 5; k++) m.Ball(x + k * 70, y + MathF.Sin(k) * 15, -50, 60, 40, 30, m.C(p.Color2, .9f)); }
        });
        QS(d, typeof(ToriiPathParameter), (p, m, t, v, v2) => // 参道: 奥へ続く石畳と、次々に並ぶ鳥居、灯籠の明かり
        {
            if (p.Background) SkyWall(m, m.C(p.Color2), m.C(0x4A3A5A), -2500);
            m.Quad3D(new(-200, m.H / 2, 300), new(200, m.H / 2, 300), new(200, m.H / 2, -2400), new(-200, m.H / 2, -2400), m.C(0x8C8073));
            for (int i = 0; i < 6; i++)
            {
                float z = 200 - i * 450, y = m.H / 2;
                foreach (int s in Sides) { m.Cyl(s * 300, y - 280, z, 22, 560, m.C(p.Color1), .1f); m.Box(s * 420, y - 80, z + 100, 50, 160, 50, m.C(0x8A8070), .2f); m.Ball(s * 420, y - 130, z + 100, 20, m.C(0xFFD27A, .8f + .2f * MathF.Sin(m.Now * 4 + i))); }
                m.Box(0, y - 560, z, 800, 40, 50, m.C(0x2A1A1A), .1f); m.Box(0, y - 500, z, 680, 28, 40, m.C(p.Color1), .1f);
            }
        });
    }
}
