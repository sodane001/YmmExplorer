using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 201番(ゲーム3 20)・202番(SNS・アプリ3 20)・203番(キャラの気持ち3 20)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>アプリの画面の板: 角の丸い厚い板と、上の色の帯・題名</summary>
    static void AppPanel(Sculpt m, float w, float h, Vector4 bar, string title, Vector4? body = null)
    {
        m.Box(0, 0, 0, w, h, 30, body ?? new Vector4(.97f, .97f, .98f, m.Al), .08f);
        m.Box(0, -h / 2 + 40, 16, w, 80, 6, bar, .08f);
        m.Text(title, 40, 0, -h / 2 + 40, 22, 4, new Vector4(1, 1, 1, m.Al), w - 60);
    }

    static partial void RegisterJ1b(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.12f, .13f, .16f, 1);
        // ---------------------------------------------------------------- 201 ゲーム3
        QS(d, typeof(BattleLogWindowParameter), (p, m, t, v, v2) => // バトルメッセージ: 下の厚い窓と白いふち、打ちこまれる文字、送りの三角
        {
            float y = m.H * .3f, bw = m.W * .85f;
            m.Box(0, y, 0, bw, 240, 30, m.C(0x0A0A30, .9f), .1f);
            m.Tube([new(-bw / 2, y - 120, 16), new(bw / 2, y - 120, 16), new(bw / 2, y + 120, 16), new(-bw / 2, y + 120, 16), new(-bw / 2, y - 120, 16)], 4, W);
            m.Text(TextKit.Typed(Str(p), m.Now, 20), 50, 0, y, 18, 4, W, bw - 100);
            if ((m.Now * 2) % 1 < .5f) Gem(m, Pts(bw / 2 - 60, y + 80, bw / 2 - 30, y + 80, bw / 2 - 45, y + 100), bw / 2 - 45, y + 88, 0f, 3, 8, W, W);
        });
        Q(d, typeof(CriticalHitParameter), (p, m, t, v, v2) => // クリティカル: とがった光の星と、飛び出す厚い文字
        {
            float k = Ease(In(m, 0, .25f));
            Gem(m, Pts(StarPts(0, 0, 360 * k, 12, .4f, .2f)), 0, 0, 0f, 6, 30, m.C(p.Color2), m.C(p.Color2));
            m.Text(Str(p).Length > 0 ? Str(p) : "CRITICAL!", 130 * (2 - k), 0, 0, 60, 40, m.C(p.Color1), 0, W, 8);
        });
        Q(d, typeof(MissDodgeParameter), (p, m, t, v, v2) => // ミス: すっと横にずれる残像と、ふわっと上がる文字
        {
            float k = In(m, 0, .4f);
            for (int i = 0; i < 4; i++) m.Ball(-120 * k * i / 3, 0, -i * 30, 80, 120, 50, m.C(p.Color1, .6f - i * .12f));
            m.Text(Str(p).Length > 0 ? Str(p) : "MISS", 90, 0, -200 - 60 * k, 20, 14, m.C(0xCFD8DC, 1 - k * .5f));
        });
        Q(d, typeof(GemStoneParameter), (p, m, t, v, v2) => // 宝石: ゆっくり回る、カットした大きな宝石ときらめき
        {
            m.Yaw(0, 0, 0, m.Now * .6f, () =>
            {
                int n = 8; for (int k = 0; k < n; k++) { float a0 = (k + .5f) * MathF.Tau / n, a1 = (k + 1.5f) * MathF.Tau / n; m.Quad3D(Ring(120, -160, a0), Ring(120, -160, a1), Ring(220, -60, a1), Ring(220, -60, a0), k % 2 == 0 ? m.C(p.Color1) : Vector4.Lerp(m.C(p.Color1), W, .3f)); m.Tri3D(Ring(220, -60, a0), Ring(220, -60, a1), new(0, 220, 0), Sculpt.Dark(m.C(p.Color1), .75f)); m.Tri3D(new(0, -160, 0), Ring(120, -160, a0), Ring(120, -160, a1), Vector4.Lerp(m.C(p.Color1), W, .4f)); }
            });
            Gem(m, Pts(StarPts(140, -170, 40 * MathF.Abs(MathF.Sin(m.Now * 3)), 4, .25f, 0)), 140, -170, 0f, 2, 10, W, W);
        });
        Q(d, typeof(CoinGainParameter), (p, m, t, v, v2) => // コイン獲得: 回って飛び上がるコインと「+数」の文字
        {
            for (int i = 0; i < Math.Max(1, p.Count); i++)
            {
                float f = ((m.Now * .8f + i * .17f) % 1), x = (DrawKit.H(p.Seed, i) - .5f) * 400, y = 100 - 400 * f + 300 * f * f;
                m.Yaw(x, y, 0, m.Now * 8 + i, () => { m.Disc(x, y, 0, 40, 40, 10, m.C(p.Color1), .3f); m.Torus(x, y, 6, 32, 3, Sculpt.Dark(m.C(p.Color1), .8f)); });
            }
            m.Text("+" + Str(p), 80, 0, -260, 20, 10, m.C(p.Color1), 0, K, 4);
        });
        Q(d, typeof(EnemyVanishParameter), (p, m, t, v, v2) => // 敵の消滅: ゆがんで消える影と、上へ散る光の粒
        {
            float k = In(m, 0, 1.2f);
            if (k < .6f) m.Ball(0, 0, 0, 160 * (1 - k), 220 * (1 + k), 100, m.C(p.Color2, 1 - k));
            for (int i = 0; i < 40; i++) { float ag = DrawKit.H(p.Seed, i) * MathF.Tau, r = 400 * k * DrawKit.H(p.Seed, i, 1); m.Ball(MathF.Cos(ag) * r, MathF.Sin(ag) * r - 200 * k, MathF.Sin(ag * 2) * r * .5f, 8, m.C(p.Color1, 1 - k)); }
        });
        Q(d, typeof(ExpOrbsParameter), (p, m, t, v, v2) => // 経験値の玉: 渦をまいて中心に吸いこまれる光の玉
        {
            for (int i = 0; i < Math.Max(4, p.Count); i++)
            {
                float f = ((m.Now * .6f + DrawKit.H(p.Seed, i)) % 1), ag = DrawKit.H(p.Seed, i, 1) * MathF.Tau + f * 6, r = 500 * (1 - f);
                m.Ball(MathF.Cos(ag) * r, MathF.Sin(ag) * r * .6f, MathF.Sin(ag) * r * .5f, 16, m.C(p.Color1, .4f + .6f * f));
            }
            m.Ball(0, 0, 0, 50, m.C(p.Color1, .5f));
        });
        Q(d, typeof(ShopWindowParameter), (p, m, t, v, v2) => // ショップ画面: 厚い窓、品物の札と値段、選択の光る枠、所持金
        {
            var it = Lines(p); m.Box(0, 0, 0, 900, 600, 30, m.C(0x10183A, .92f), .08f);
            for (int i = 0; i < it.Count; i++) { float y = -220 + i * 90; m.Ball(-380, y, 20, 26, Pal(m, i)); m.Text(it[i], 40, -50, y, 18, 3, W, 600); }
            int sel = (int)(m.Now * .8f) % Math.Max(1, it.Count);
            m.Tube([new(-430, -260 + sel * 90, 18), new(430, -260 + sel * 90, 18), new(430, -180 + sel * 90, 18), new(-430, -180 + sel * 90, 18), new(-430, -260 + sel * 90, 18)], 4, m.C(p.Color2));
        });
        Q(d, typeof(EquipWindowParameter), (p, m, t, v, v2) => // 装備画面: 人形のシルエットと、まわりの装備の枠(剣・盾・兜)
        {
            m.Box(0, 0, 0, 1000, 640, 30, m.C(p.Color1, .92f), .08f);
            var c = m.C(0x90A4AE); m.Ball(-250, -160, 20, 60, c); m.Limb(-250, -90, 20, -250, 120, 20, 70, 55, c);
            var it = Lines(p);
            for (int i = 0; i < 4; i++) { float y = -200 + i * 130; m.Box(150, y, 20, 520, 100, 16, m.C(0x37474F), .2f); m.Box(-60, y, 30, 80, 80, 20, Pal(m, i), .3f); if (i < it.Count) m.Text(it[i], 36, 200, y, 30, 3, W, 380); }
        });
        Q(d, typeof(StatCompareParameter), (p, m, t, v, v2) => // ステータス比較: 項目ごとの数値と、上下を向いた色の立体の矢印
        {
            var rows = Lines(p);
            for (int i = 0; i < rows.Count; i++)
            {
                float y = (i - (rows.Count - 1) / 2f) * 90; bool up = !rows[i].Contains('-') && !rows[i].Contains('↓');
                m.Text(rows[i], 44, -60, y, 4, 4, W, 600);
                Puff(m, 4, 16, up ? m.C(p.Color1) : m.C(p.Color2), 320, y + (up ? 20 : -20), 360, y + (up ? -25 : 25), 400, y + (up ? 20 : -20));
            }
        });
        Q(d, typeof(DungeonMapParameter), (p, m, t, v, v2) => // ダンジョンのマップ: 部屋の床の板と通路、高い壁、今いる場所の光る玉
        {
            float cs = 120;
            for (int y = 0; y < 5; y++) for (int x = 0; x < 7; x++)
                {
                    bool room = DrawKit.H(p.Seed, x + y * 7) > .35f; float px = (x - 3) * cs, py = (y - 2) * cs;
                    if (room) m.Box(px, py, 0, cs - 10, cs - 10, 10, m.C(p.Color1, .8f), .05f);
                    else m.Box(px, py, 30, cs, cs, 70, m.C(0x3A3A4A), .05f);
                }
            float b = MathF.Abs(MathF.Sin(m.Now * 4)) * 10; m.Ball(0, 0, 30 + b, 26, m.C(p.Color2));
        });
        Q(d, typeof(SavePointParameter), (p, m, t, v, v2) => // セーブポイント: 床の光る輪と、回りながら浮かぶ結晶
        {
            m.Pitch(0, 200, 0, MathF.PI / 2, () => { m.Torus(0, 200, 0, 160, 6, m.C(p.Color1)); m.Torus(0, 200, 0, 220, 3, m.C(p.Color1, .5f)); });
            float b = MathF.Sin(m.Now * 2) * 20;
            m.Yaw(0, 0, 0, m.Now, () => Gem(m, Pts(0, -160 + b, 70, b, 0, 160 + b, -70, b), 0, b, .3f, 10, 50, m.C(p.Color1), Vector4.Lerp(m.C(p.Color1), W, .5f)));
            m.Cyl(0, 0, 0, 120, 400, m.C(p.Color1, .12f), .05f);
        });
        Q(d, typeof(TargetCursorParameter), (p, m, t, v, v2) => // ターゲット: 上下にはずむ下向きの矢印と、足もとの輪
        {
            float b = MathF.Abs(MathF.Sin(m.Now * 5)) * 30;
            m.Move(0, -b, () => Gem(m, Pts(-60, -200, 60, -200, 0, -100), 0, -160, .3f, 10, 30, m.C(p.Color1), Vector4.Lerp(m.C(p.Color1), W, .4f)));
            m.Pitch(0, 100, 0, MathF.PI / 2 - .3f, () => m.Torus(0, 100, 0, 120 + 10 * MathF.Sin(m.Now * 5), 6, m.C(p.Color1)));
        });
        Q(d, typeof(JustDodgeParameter), (p, m, t, v, v2) => // ジャスト回避: 広がる輪、残像、ぶ厚い「JUST!」
        {
            float k = In(m, 0, .3f);
            m.Torus(0, 0, -40, 100 + 400 * k, 8 * (1 - k) + 2, m.C(p.Color2, 1 - k));
            for (int i = 0; i < 3; i++) m.Ball(-80 - i * 60, 0, -i * 40, 70, 110, 40, m.C(p.Color2, .4f - i * .1f));
            m.Text(Str(p).Length > 0 ? Str(p) : "JUST!", 120 * (1.5f - .5f * Ease(k)), 0, -40, 40, 30, m.C(p.Color1), 0, K, 6);
        });
        Q(d, typeof(RankResultParameter), (p, m, t, v, v2) => // ランク判定: 回って止まる大きな文字の円盤と、まわりの光
        {
            float k = Ease(In(m, .2f, .8f)); string rank = Str(p).Length > 0 ? Str(p) : "S";
            m.Yaw(0, 0, 0, (1 - k) * MathF.Tau * 2, () => { m.Disc(0, 0, 0, 220, 220, 50, m.C(p.Color1), .3f); m.Text(rank, 280, 0, 0, 30, 30, W); });
            for (int i = 0; i < 12; i++) { float ag = i * MathF.PI / 6 + m.Now; m.Limb(MathF.Cos(ag) * 260, MathF.Sin(ag) * 260, -20, MathF.Cos(ag) * 340, MathF.Sin(ag) * 340, -20, 8, 2, m.C(p.Color2, k)); }
        });
        Q(d, typeof(LoadingTipsParameter), (p, m, t, v, v2) => // ロード中のTips: 厚い枠、「TIPS」の札と本文、回る輪
        {
            m.Box(0, 0, 0, 1100, 260, 30, m.C(p.Color1, .9f), .1f);
            m.Box(-430, -100, 20, 160, 60, 20, m.C(p.Color2), .3f); m.Text("TIPS", 36, -430, -100, 32, 4, K);
            m.Text(Str(p), 40, 60, 10, 18, 3, W, 900);
            m.Turn(480, 80, m.Now * 4, () => m.Tube(ArcPts3(480, 80, 30, 0, 4.5f, 18, 12), 5, W));
        });
        Q(d, typeof(RhythmNotesParameter), (p, m, t, v, v2) => // リズムゲーム: 奥から流れてくるノーツの玉と、手前の判定ライン
        {
            for (int l = 0; l < 4; l++) { float x = (l - 1.5f) * 180; m.Box(x, 250, -600, 160, 6, 1600, m.C(0xFFFFFF, .1f), .2f); }
            m.Box(0, 250, 100, 760, 14, 30, m.C(p.Color1), .3f);
            for (int i = 0; i < 14; i++) { int lane = (int)(DrawKit.H(p.Seed, i) * 4); float z = 100 - ((m.Now * 500 + i * 220) % 1600); m.Box((lane - 1.5f) * 180, 230, z, 140, 30, 40, Pal(m, lane), .4f); }
        });
        Q(d, typeof(FighterHpBarsParameter), (p, m, t, v, v2) => // 格闘ゲームの体力: 左右の厚いバー、まん中のタイマー
        {
            var it = p.ItemsForModel;
            foreach (int s in Sides) { float lv = s < 0 ? v : v2; m.Box(s * 380, 0, 0, 640, 60, 40, m.C(0x000000, .7f), .2f); m.Box(s * (60 + 320 * lv), 0, 14, 640 * lv, 44, 30, s < 0 ? m.C(p.Color1) : m.C(p.Color2), .2f); string n = s < 0 ? it.ElementAtOrDefault(0) ?? "" : it.ElementAtOrDefault(1) ?? ""; m.Text(n, 36, s * 380, 60, 4, 4, W); }
            m.Disc(0, 0, 10, 60, 60, 50, m.C(0x222222), .4f); m.Text(((int)MathF.Max(0, 99 - m.Now)).ToString(), 50, 0, 0, 36, 4, W);
        });
        Q(d, typeof(MashQteParameter), (p, m, t, v, v2) => // 連打QTE: 押しこまれる丸いボタンと、たまる弧のゲージ
        {
            bool hit = (m.Now * 12) % 1 < .5f; float lv = Math.Clamp(m.Now * .3f, 0, 1);
            m.Disc(0, 0, hit ? 0 : 20, 140, 140, 60, m.C(p.Color1), .4f); m.Text(Str(p).Length > 0 ? Str(p) : "A", 120, 0, 0, (hit ? 0 : 20) + 32, 6, W);
            m.Tube(ArcPts3(0, 0, 200, -MathF.PI / 2, -MathF.PI / 2 + MathF.Tau * lv, 0, 40), 12, m.C(p.Color2));
        });
        Q(d, typeof(TargetGameParameter), (p, m, t, v, v2) => // 的あて: 奥で左右に動く的の円盤と、飛んでいく玉
        {
            for (int i = 0; i < 3; i++) { float x = MathF.Sin(m.Now * (1 + i * .4f) + i) * 400, z = -400 - i * 300; for (int r = 3; r >= 1; r--) m.Disc(x, -100 + i * 60, z + (3 - r) * 4, 60 * r, 60 * r, 10, r % 2 == 1 ? m.C(0xE53935) : W, .2f); }
            float f = (m.Now * .7f) % 1; m.Ball(0, 300 - 400 * f, 300 - 900 * f, 20, m.C(p.Color1));
        });

        // ---------------------------------------------------------------- 202 SNS・アプリ3
        Q(d, typeof(MessengerChatParameter), (p, m, t, v, v2) => // メッセージアプリ: 左右の吹き出しの板が順に出る
        {
            var rows = Lines(p); float y0 = -rows.Count * 60;
            for (int i = 0; i < rows.Count; i++) { float k = Ease(In(m, i * .5f, .3f)); if (k <= 0) continue; bool me = i % 2 == 1; float x = me ? 200 : -200, y = y0 + i * 120, bw = MathF.Min(600, rows[i].Length * 40 + 80) * k; m.Box(x, y, 0, bw, 90, 30, me ? m.C(p.Color1) : m.C(0xFFFFFF, .95f), .5f); m.Text(rows[i], 36, x, y, 16, 3, me ? W : K, 560); if (!me) m.Ball(x - bw / 2 - 60, y, 0, 34, m.C(0x90A4AE)); }
        });
        Q(d, typeof(HashtagPopParameter), (p, m, t, v, v2) => // ハッシュタグ: ぽんぽんと出る、厚いカプセルの札
        {
            var it = p.ItemsForModel;
            for (int i = 0; i < it.Count; i++) { float k = Ease(In(m, i * .2f, .3f)), x = (i % 3 - 1) * 380, y = (i / 3 - (it.Count - 1) / 6f) * 120, bw = (it[i].Length * 40 + 100) * k; m.Box(x, y, i % 2 * 30, bw, 80, 40, Pal(m, i), .5f); m.Text("#" + it[i], 40 * k, x, y, i % 2 * 30 + 22, 4, W, 360); }
        });
        Q(d, typeof(ProfileCardParameter), (p, m, t, v, v2) => // プロフィールカード: 厚いカード、丸いアイコン、名前と紹介、フォローのボタン
        {
            var ln = Str(p).Split('\n');
            m.Box(0, 0, 0, 700, 420, 30, W, .1f); m.Box(0, -150, 10, 700, 120, 20, m.C(p.Color1), .1f);
            m.Ball(-230, -60, 40, 80, m.C(0x90A4AE)); m.Torus(-230, -60, 40, 80, 6, W);
            m.Text(ln.ElementAtOrDefault(0) ?? "", 50, 60, -30, 18, 4, K); m.Text(string.Join(" ", ln.Skip(1)), 32, 0, 80, 18, 3, m.C(0x555560), 640);
            m.Box(220, 160, 20, 200, 60, 20, m.C(p.Color1), .5f); m.Text("フォロー", 32, 220, 160, 32, 3, W);
        });
        Q(d, typeof(StoryProgressParameter), (p, m, t, v, v2) => // ストーリーズの進行バー: 並んだ細い棒が順に満ちる
        {
            int n = Math.Clamp(p.Count, 1, 10); float bw = 1200f / n, cur = m.Now * .5f;
            for (int i = 0; i < n; i++) { float x = -600 + bw * (i + .5f), f = Math.Clamp(cur - i, 0, 1); m.Box(x, 0, 0, bw - 12, 10, 8, m.C(0xFFFFFF, .35f), .5f); if (f > 0) m.Box(x - (bw - 12) * (1 - f) / 2, 0, 4, (bw - 12) * f, 10, 10, m.C(p.Color1), .5f); }
        });
        Q(d, typeof(ReviewStarsParameter), (p, m, t, v, v2) => // 口コミの星: 順に光って回る5つの立体の星と点数
        {
            float score = v * 5;
            for (int i = 0; i < 5; i++) { float x = (i - 2) * 150, k = Ease(In(m, i * .15f, .3f)), on = Math.Clamp(score - i, 0, 1); m.Yaw(x, 0, 0, (1 - k) * MathF.Tau, () => Gem(m, Pts(StarPts(x, 0, 60, 5, .45f, 0)), x, 0, 0f, 6, 20, on > .5f ? m.C(p.Color1) : m.C(0x555560), on > .5f ? m.C(p.Color1) : m.C(0x555560))); }
            m.Text(score.ToString("0.0"), 70, 0, 120, 10, 6, W);
        });
        Q(d, typeof(ProductCardParameter), (p, m, t, v, v2) => // 商品カード: 厚いカード、商品の箱の絵、名前と値段、カートのボタン
        {
            var ln = Str(p).Split('\n');
            m.Box(0, 0, 0, 500, 640, 30, W, .08f);
            m.Box(0, -130, 40, 240, 240, 140, m.C(p.Color1), .15f);
            m.Text(ln.ElementAtOrDefault(0) ?? "", 40, 0, 90, 18, 3, K, 460); m.Text(ln.ElementAtOrDefault(1) ?? "", 56, 0, 160, 18, 4, m.C(0xE53935));
            m.Box(0, 250, 20, 360, 70, 20, m.C(0xFFB300), .5f); m.Text("カートに入れる", 32, 0, 250, 32, 3, K);
        });
        Q(d, typeof(FakeQrScanParameter), (p, m, t, v, v2) => // QR風の読み取り: 凸凹したマスの板と、上下に動く光の線
        {
            float cs = 30;
            for (int y = 0; y < 13; y++) for (int x = 0; x < 13; x++) { bool corner = (x < 3 && y < 3) || (x > 9 && y < 3) || (x < 3 && y > 9); if (corner || DrawKit.H(p.Seed, x + y * 13) > .5f) m.Box((x - 6) * cs, (y - 6) * cs, 8, cs - 2, cs - 2, 16, K, .1f); }
            m.Box(0, 0, 0, 13 * cs + 40, 13 * cs + 40, 4, W, .05f);
            float sy = MathF.Sin(m.Now * 2) * 200; m.Box(0, sy, 30, 460, 6, 6, m.C(p.Color1), .5f);
        });
        Q(d, typeof(LoginScreenParameter), (p, m, t, v, v2) => // ログイン画面: 厚い板、入力欄2つ(打たれる点)、ボタン
        {
            AppPanel(m, 700, 560, m.C(p.Color1), Str(p).Length > 0 ? Str(p) : "ログイン");
            for (int i = 0; i < 2; i++) { float y = -70 + i * 110; m.Box(0, y, 18, 560, 70, 10, m.C(0xE8ECEF), .3f); }
            int dots = (int)(m.Now * 4) % 10; for (int k = 0; k < dots; k++) m.Ball(-240 + k * 30, 40, 26, 8, K);
            m.Box(0, 180, 20, 560, 80, 20, m.C(p.Color1), .4f); m.Text("ログイン", 40, 0, 180, 32, 4, W);
        });
        Q(d, typeof(ErrorScreenParameter), (p, m, t, v, v2) => // エラー画面: 揺れる厚い板と、赤い×の丸、文字
        {
            float sh = MathF.Sin(m.Now * 40) * 8 * MathF.Max(0, 1 - m.Now);
            m.Move(sh, 0, () =>
            {
                AppPanel(m, 760, 400, m.C(p.Color1), "エラー");
                m.Disc(-260, 40, 20, 60, 60, 20, m.C(p.Color1), .4f); m.Limb(-290, 10, 32, -230, 70, 32, 6, 6, W); m.Limb(-230, 10, 32, -290, 70, 32, 6, 6, W);
                m.Text(Str(p), 40, 70, 40, 18, 3, K, 500);
            });
        });
        Q(d, typeof(FileCopyDialogParameter), (p, m, t, v, v2) => // ファイルのコピー: 左から右へ飛ぶ書類、2つのフォルダー、進む棒
        {
            AppPanel(m, 800, 420, m.C(p.Color1), Str(p).Length > 0 ? Str(p) : "コピー中");
            foreach (int s in Sides) { Puff(m, 30, 20, m.C(0xFFC93C), s * 260 - 70, -20, s * 260 - 20, -20, s * 260, -5, s * 260 + 70, -5, s * 260 + 70, 70, s * 260 - 70, 70); }
            float f = (m.Now * .8f) % 1; m.Box(-260 + 520 * f, 20 - 120 * MathF.Sin(f * MathF.PI), 50, 50, 64, 6, W, .1f);
            float lv = Math.Clamp(m.Now * .2f, 0, 1); m.Box(0, 150, 18, 700, 24, 8, m.C(0xDDDDE3), .5f); m.Box(-350 + 350 * lv, 150, 22, 700 * lv, 24, 10, m.C(0x43A047), .5f);
        });
        Q(d, typeof(TaskbarNoticeParameter), (p, m, t, v, v2) => // タスクバー: 下の長い板とアイコン、飛び出す通知の札
        {
            m.Box(0, 300, 0, 1400, 70, 30, m.C(0x1F2430, .95f), .1f);
            for (int i = 0; i < 6; i++) m.Box(-500 + i * 90, 300, 18, 54, 54, 14, Pal(m, i), .3f);
            float k = Ease(In(m, .3f, .4f)); m.Box(480, 300 - 220 * k, 30, 420, 160, 30, m.C(0x2A2F3A, .95f), .1f); m.Text(Str(p), 34, 480, 300 - 220 * k, 46, 3, W, 380);
        });
        QS(d, typeof(ContextMenuGuideParameter), (p, m, t, v, v2) => // 右クリックメニュー: 浮いた厚いメニューの板と項目、選んだ行と立体のカーソル
        {
            var it = Lines(p); float bw = 420, rh = 70, bh = it.Count * rh + 20;
            m.Box(0, 0, 30, bw, bh, 20, W, .05f);
            int sel = (int)(m.Now * .7f) % Math.Max(1, it.Count);
            for (int i = 0; i < it.Count; i++) { float y = -bh / 2 + 10 + rh * (i + .5f); if (i == sel) m.Box(0, y, 42, bw - 20, rh - 10, 4, m.C(p.Color1, .3f), .2f); m.Text(it[i], 34, 0, y, 44, 3, K, bw - 40); }
            m.Slab(80, 16, W, -bw / 2 - 40, -bh / 2, -bw / 2 - 40, -bh / 2 + 70, -bw / 2 - 22, -bh / 2 + 55, -bw / 2 - 8, -bh / 2 + 82, -bw / 2 + 2, -bh / 2 + 77, -bw / 2 - 12, -bh / 2 + 50, -bw / 2 + 10, -bh / 2 + 50);
        });
        Q(d, typeof(VideoCallGridParameter), (p, m, t, v, v2) => // ビデオ通話: 並んだ厚い映像の枠と人影、話している人の光る枠
        {
            int n = Math.Clamp(p.Count, 2, 9), cols = n <= 4 ? 2 : 3, rows = (n + cols - 1) / cols; float cw = 400, ch = 260; int speak = (int)(m.Now * .5f) % n;
            for (int i = 0; i < n; i++) { float x = (i % cols - (cols - 1) / 2f) * (cw + 20), y = (i / cols - (rows - 1) / 2f) * (ch + 20); m.Box(x, y, 0, cw, ch, 20, m.C(0x2A2F3A), .05f); m.Ball(x, y - 20, 14, 50, Pal(m, i)); m.Ball(x, y + 90, 14, 90, 50, 20, Pal(m, i)); if (i == speak) m.Tube([new(x - cw / 2, y - ch / 2, 14), new(x + cw / 2, y - ch / 2, 14), new(x + cw / 2, y + ch / 2, 14), new(x - cw / 2, y + ch / 2, 14), new(x - cw / 2, y - ch / 2, 14)], 5, m.C(p.Color1)); }
        });
        Q(d, typeof(RecIndicatorParameter), (p, m, t, v, v2) => // 録画中: 点滅する赤い玉と「REC」、経過時間
        {
            bool on = (m.Now % 1) < .6f;
            m.Ball(-120, 0, 0, 36, m.C(p.Color1, on ? 1 : .3f));
            m.Text("REC", 70, 40, 0, 4, 8, W);
            m.Text(TimeSpan.FromSeconds(m.Now).ToString(@"mm\:ss"), 50, 280, 0, 4, 4, W);
        });
        Q(d, typeof(CommentPickupParameter), (p, m, t, v, v2) => // コメントのピックアップ: 引き出された厚い札と、名前の丸
        {
            var ln = Str(p).Split('\n'); float k = Ease(In(m, 0, .4f));
            m.Box(0, 0, 40 * k, 900 * k, 200, 40, W, .2f); m.Box(-450 * k + 20, 0, 50 * k, 16, 200, 50, m.C(p.Color1), .2f);
            m.Ball(-330 * k, -30, 70 * k, 40, m.C(0x90A4AE)); m.Text(ln.ElementAtOrDefault(0) ?? "", 32, -150 * k, -50, 62 * k, 3, m.C(p.Color1));
            m.Text(string.Join(" ", ln.Skip(1)), 40, 60 * k, 30, 62 * k, 3, K, 700);
        });
        Q(d, typeof(TrendRankingParameter), (p, m, t, v, v2) => // トレンド: 順位の数字と、上から順に出る段の札
        {
            var it = Lines(p);
            for (int i = 0; i < it.Count; i++) { float k = Ease(In(m, i * .2f, .3f)), y = (i - (it.Count - 1) / 2f) * 100; m.Box(0, y, (it.Count - i) * 6, 800 * k, 84, 30, i == 0 ? m.C(p.Color1) : m.C(0xFFFFFF, .9f), .2f); m.Text($"{i + 1}", 50, -340, y, (it.Count - i) * 6 + 16, 4, i == 0 ? W : m.C(p.Color1)); m.Text(it[i], 38, 40, y, (it.Count - i) * 6 + 16, 3, i == 0 ? W : K, 600); }
        });
        Q(d, typeof(ScreenshotFlashParameter), (p, m, t, v, v2) => // スクリーンショット: 白く光る板と、縮んで隅へ飛ぶ写真の板
        {
            float f = In(m, 0, .2f), s = Ease(In(m, .3f, .5f));
            m.Box(0, 0, 100, 1600, 900, 4, m.C(0xFFFFFF, (1 - f) * .8f), .01f);
            float sc = 1 - .7f * s; m.Box(500 * s, 250 * s, 50, 1500 * sc, 840 * sc, 20, W, .02f); m.Box(500 * s, 250 * s, 62, 1460 * sc, 800 * sc, 4, m.C(p.Color1), .02f);
        });
        Q(d, typeof(CalendarAppParameter), (p, m, t, v, v2) => // カレンダーアプリ: 月のマスの板と、予定の色の札が浮く
        {
            AppPanel(m, 900, 700, m.C(p.Color1), Str(p).Split('\n')[0]);
            for (int y = 0; y < 5; y++) for (int x = 0; x < 7; x++) { float px = (x - 3) * 120, py = -180 + y * 110; m.Box(px, py, 18, 110, 100, 6, m.C(0xF0F2F5), .1f); m.Text((y * 7 + x + 1).ToString(), 22, px - 35, py - 30, 22, 2, K); if (DrawKit.H(p.Seed, x + y * 7) > .8f) m.Box(px, py + 20, 26, 90, 26, 10, Pal(m, x + y), .4f); }
        });
        Q(d, typeof(CalculatorScreenParameter), (p, m, t, v, v2) => // 電卓: 厚い本体、表示の窓と数字、押されるボタンのキー
        {
            m.Box(0, 0, 0, 500, 760, 60, m.C(0x2A2F3A), .2f);
            m.Box(0, -260, 32, 420, 150, 8, m.C(0xC8D8B0), .1f); m.Text(Str(p), 70, 60, -260, 38, 4, K, 380);
            int press = (int)(m.Now * 2) % 16;
            for (int i = 0; i < 16; i++) { float x = (i % 4 - 1.5f) * 100, y = -70 + (i / 4) * 100; m.Box(x, y, i == press ? 30 : 40, 80, 80, 20, i % 4 == 3 ? m.C(p.Color1) : m.C(0x505866), .3f); }
        });
        Q(d, typeof(NaviTurnParameter), (p, m, t, v, v2) => // ナビ: 奥へ続く道路と曲がる先、大きな曲がる矢印、距離
        {
            m.Quad3D(new(-150, 300, 300), new(150, 300, 300), new(150, 300, -900), new(-150, 300, -900), m.C(0x505866));
            m.Quad3D(new(150, 300, -700), new(900, 300, -700), new(900, 300, -900), new(150, 300, -900), m.C(0x505866));
            m.Tube([new(0, 250, 200), new(0, 250, -650), new(500, 250, -800)], 30, m.C(p.Color1));
            m.Limb(450, 250, -790, 600, 250, -830, 70, 1, m.C(p.Color1));
            m.Text(Str(p), 60, 0, -250, 0, 6, W);
        });

        // ---------------------------------------------------------------- 203 キャラの気持ち3
        Q(d, typeof(SleepyZzzParameter), (p, m, t, v, v2) => // Zzz: 大きさの違うZが、ゆれながら斜め上へ浮かぶ
        {
            for (int i = 0; i < 3; i++) { float f = ((m.Now * .35f + i / 3f) % 1); m.Text("Z", 60 + 90 * f, 100 + 160 * f, -100 - 200 * f, 80 * f, 12, m.C(p.Color1, 1 - f)); }
        });
        Q(d, typeof(StartleLinesParameter), (p, m, t, v, v2) => // ビクッ: 頭から放射に飛び出す、とがった太い線
        {
            float k = Ease(In(m, 0, .2f));
            for (int i = 0; i < 7; i++) { float ag = -MathF.PI + i * MathF.PI / 6; m.Limb(MathF.Cos(ag) * 150, MathF.Sin(ag) * 150, 0, MathF.Cos(ag) * (150 + 120 * k), MathF.Sin(ag) * (150 + 120 * k), MathF.Sin(ag * 3) * 40, 14, 2, m.C(p.Color1)); }
        });
        Q(d, typeof(ChiinBellParameter), (p, m, t, v, v2) => // ちーん: おりんの器、ゆれる棒、立ちのぼる線香の煙
        {
            m.Lathe(0, 0, m.C(0xC9A227), 0, 160, 160, 160, 180, 80, 170, 60, 0, 60);
            m.Cyl(0, 175, 0, 190, 30, m.C(0x8A1F1F), .3f);
            float sw = MathF.Sin(m.Now * 3) * .3f; m.Turn(220, 0, sw, () => m.Limb(220, 0, 60, 300, -200, 60, 8, 6, m.C(0x6D4C41)));
            m.Limb(-260, 180, 0, -260, -60, 0, 3, 3, m.C(0x4E7D3A));
            m.Tube([.. Enumerable.Range(0, 20).Select(i => new Vector3(-260 + MathF.Sin(i * .5f + m.Now * 2) * 20, -60 - i * 15, 0))], 4, m.C(0xFFFFFF, .4f));
            m.Text("ちーん", 70, 0, -260, 0, 6, m.C(p.Color1));
        });
        Q(d, typeof(SmirkShadowParameter), (p, m, t, v, v2) => // ニヤリ: 顔の上半分にかかる暗い影の板と、光る目
        {
            m.Box(0, -100, 30, 600, 260, 10, m.C(p.Color1, .6f), .3f);
            foreach (int s in Sides) m.Ball(s * 110, -100, 45, 30, 10, 8, m.C(0xFFFFFF, .9f));
        });
        Q(d, typeof(StareLineParameter), (p, m, t, v, v2) => // ジーッ: 目から伸びる2本の光の線と「ジーッ」
        {
            float k = Ease(In(m, 0, .5f));
            foreach (int s in Sides) { m.Ball(s * 60, 0, 0, 24, m.C(p.Color1)); m.Limb(s * 60, 0, 0, s * 60 + 700 * k, 30, 200 * k, 6, 2, m.C(p.Color1, .6f)); }
            m.Text("ジーッ", 70, 300, -120, 30, 8, m.C(p.Color1));
        });
        Q(d, typeof(WhisperBubbleParameter), (p, m, t, v, v2) => // ひそひそ: 点線のふちの小さな吹き出しの板と、手を添えた口元
        {
            m.Ball(0, 0, 0, 260, 120, 40, m.C(0xFFFFFF, .85f));
            for (int i = 0; i < 24; i++) { float ag = i * MathF.Tau / 24; m.Ball(MathF.Cos(ag) * 260, MathF.Sin(ag) * 120, 10, 6, m.C(p.Color1)); }
            m.Text(Str(p), 36, 0, 0, 42, 3, K, 440);
            m.Box(-320, 160, 0, 60, 120, 40, m.C(0xFFD9B3), .5f);
        });
        Q(d, typeof(CozyTeaParameter), (p, m, t, v, v2) => // ほっこり: 湯のみと受け皿、くねって立つ湯気
        {
            m.Cyl(0, 230, 0, 200, 20, m.C(0x8D6E63), .3f);
            m.Lathe(0, 0, m.C(p.Color1), 0, 220, 110, 220, 140, 40, 0, 40);
            m.Lathe(0, 0, m.C(0x6B8E3B), 0, 50, 130, 50, 0, 50);
            for (int k = 0; k < 3; k++) m.Tube([.. Enumerable.Range(0, 16).Select(i => new Vector3((k - 1) * 50 + MathF.Sin(i * .5f + m.Now * 2 + k) * 20, 20 - i * 18, 0))], 6, m.C(0xFFFFFF, .45f));
        });
        Q(d, typeof(GoosebumpsParameter), (p, m, t, v, v2) => // 鳥肌: ぶつぶつ盛り上がる小さな玉と、ふるえる線
        {
            int keep = m.Seg; m.Seg = 8;
            for (int i = 0; i < 40; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * 500, (DrawKit.H(p.Seed, i, 1) - .5f) * 300, 0, 10 + 4 * MathF.Sin(m.Now * 20 + i), m.C(p.Color1));
            m.Seg = keep;
            foreach (int s in Sides) for (int k = 0; k < 2; k++) m.Tube(ArcPts3(s * (300 + k * 40), 0, 60, s > 0 ? -.6f : 2.5f, s > 0 ? .6f : 3.8f, MathF.Sin(m.Now * 40) * 4, 8), 4, m.C(p.Color1));
        });
        Q(d, typeof(BulbFailParameter), (p, m, t, v, v2) => // 電球が消える: ちらついて消える電球と、落ちる黒い煙
        {
            float on = m.Now < 1 ? ((m.Now * 10) % 1 < .5f ? 1 : .3f) : 0;
            m.Ball(0, -40, 0, 140, Vector4.Lerp(m.C(0x9E9E9E, .5f), m.C(0xFFEB3B), on)); m.Cyl(0, 130, 0, 70, 90, m.C(0x9E9E9E), .3f);
            if (on == 0) for (int i = 0; i < 4; i++) { float f = ((m.Now * .5f + i / 4f) % 1); m.Ball(MathF.Sin(i) * 40, -200 - f * 150, 0, 30 + 30 * f, m.C(0x333333, .6f * (1 - f))); }
        });
        Q(d, typeof(BlankEyesParameter), (p, m, t, v, v2) => // 白目: 光る白い楕円の目と、後ろの影の線
        {
            foreach (int s in Sides) { m.Ball(s * 110, 0, 20, 70, 90, 40, W); m.Torus(s * 110, 0, 20, 80, 4, m.C(p.Color1)); }
            for (int i = 0; i < 8; i++) { float x = -280 + i * 80; m.Box(x, -150, -30, 8, 200, 4, m.C(0x3D5AFE, .5f), .5f); }
        });
        Q(d, typeof(ShiverColdParameter), (p, m, t, v, v2) => // ぶるぶる: 左右にふるえる人影と、まわりの青い線と雪の結晶
        {
            float sh = MathF.Sin(m.Now * 50) * 8;
            m.Move(sh, 0, () => { m.Ball(0, -150, 0, 70, m.C(p.Color1)); m.Limb(0, -80, 0, 0, 150, 0, 80, 60, m.C(p.Color1)); });
            foreach (int s in Sides) for (int k = 0; k < 3; k++) m.Limb(s * (150 + k * 30), -100 + k * 60, 0, s * (190 + k * 30), -80 + k * 60, 0, 4, 4, m.C(0x81D4FA));
            for (int i = 0; i < 4; i++) m.Turn(-250 + i * 160, -260, m.Now, () => Gem(m, Pts(StarPts(-250 + i * 160, -260, 24, 6, .3f, 0)), -250 + i * 160, -260, 0f, 2, 6, W, W));
        });
        Q(d, typeof(MuscleFlexParameter), (p, m, t, v, v2) => // ムキムキ: 曲げた太い腕と盛り上がる力こぶ、まわりのオーラの炎
        {
            var skin = m.C(p.Color1);
            m.Limb(0, 100, 0, 200, 60, 0, 70, 60, skin); m.Limb(200, 60, 0, 220, -160, 0, 55, 50, skin); m.Ball(220, -190, 0, 60, skin);
            float b = 1 + .1f * MathF.Sin(m.Now * 6); m.Ball(110, 20, 30, 90 * b, 70 * b, 70 * b, skin);
            for (int i = 0; i < 10; i++) { float ag = i * MathF.Tau / 10, f = ((m.Now + i * .1f) % 1); m.Ball(100 + MathF.Cos(ag) * 280, -40 + MathF.Sin(ag) * 220 - f * 80, -60, 30 * (1 - f) + 8, m.C(p.Color2, .5f * (1 - f))); }
        });
        Q(d, typeof(HungryGuuParameter), (p, m, t, v, v2) => // 腹ペコ: ふるえるお腹のあたりから出る「ぐぅ〜」と、うずの線
        {
            float sh = MathF.Sin(m.Now * 30) * 4;
            m.Move(sh, 0, () => m.Text("ぐぅ〜", 100, 0, 0, 20, 14, m.C(p.Color1), 0, W, 4));
            m.Tube([.. Enumerable.Range(0, 40).Select(i => { float f = i / 40f, ag = f * 12; return new Vector3(-280 + MathF.Cos(ag) * 60 * f, 60 + MathF.Sin(ag) * 60 * f, 0); })], 4, m.C(p.Color1));
        });
        Q(d, typeof(GiggleParameter), (p, m, t, v, v2) => // くすくす: はずんで浮かぶ「くすくす」の文字と音符
        {
            for (int i = 0; i < 3; i++) { float f = ((m.Now * .5f + i / 3f) % 1); m.Text(i % 2 == 0 ? "くす" : "♪", 50 + 30 * f, (i - 1) * 160, -100 - 200 * f + MathF.Abs(MathF.Sin(m.Now * 6 + i)) * 20, 40 * f, 6, m.C(p.Color1, 1 - f)); }
        });
        Q(d, typeof(SadRaincloudParameter), (p, m, t, v, v2) => // しょんぼりの雨雲: 頭の上の小さな灰色の雲と、落ちる雨粒
        {
            m.Ball(-60, -200, 0, 90, m.C(p.Color1)); m.Ball(40, -220, 0, 110, m.C(p.Color1)); m.Ball(140, -190, 0, 80, m.C(p.Color1));
            for (int i = 0; i < 8; i++) { float f = ((m.Now * 1.5f + i / 8f) % 1), x = -100 + i * 35; m.Limb(x, -140 + f * 260, 0, x - 3, -110 + f * 260, 0, 2, 4, m.C(0x81D4FA, 1 - f)); }
        });
        Q(d, typeof(AngryStompParameter), (p, m, t, v, v2) => // イライラ足踏み: ドンと下りる足と、広がる衝撃の輪、怒りマーク
        {
            float ph = (m.Now * 3) % 1, y = ph < .3f ? 200 : 200 - 120 * MathF.Sin((ph - .3f) / .7f * MathF.PI);
            m.Box(0, y, 40, 200, 70, 120, m.C(p.Color1), .4f);
            m.Pitch(0, 240, 0, MathF.PI / 2, () => m.Torus(0, 240, 0, 100 + 200 * ph, 5, m.C(0xFFFFFF, 1 - ph)));
            foreach (int a in Sides) foreach (int b in Sides) m.Tube(ArcPts3(200 + a * 30, -200 + b * 30, 22, a * b > 0 ? 0 : 1.57f, a * b > 0 ? 1.57f : 3.14f, 20, 6), 5, m.C(0xE5294F));
        });
        Q(d, typeof(BlushSteamParameter), (p, m, t, v, v2) => // 恥ずかしい: 赤くなる頬の玉と、頭からボンッと出る湯気
        {
            float k = Ease(In(m, 0, .3f));
            foreach (int s in Sides) m.Ball(s * 160, 40, 20, 60, 36, 14, m.C(p.Color1, .7f * k));
            for (int i = 0; i < 6; i++) { float f = ((m.Now * .7f + i / 6f) % 1), ag = -MathF.PI / 2 + (i - 2.5f) * .3f; m.Ball(MathF.Cos(ag) * (180 + 150 * f), -100 + MathF.Sin(ag) * (180 + 150 * f), 0, 30 + 40 * f, m.C(0xFFFFFF, .7f * (1 - f))); }
        });
        Q(d, typeof(ThinkingDotsParameter), (p, m, t, v, v2) => // 考え中: 順番にはずむ3つの玉と、小さな吹き出しの玉
        {
            for (int i = 0; i < 3; i++) m.Ball((i - 1) * 90, -MathF.Max(0, MathF.Sin(m.Now * 6 - i)) * 40, 0, 30, m.C(p.Color1));
            m.Ball(-200, 120, 0, 24, m.C(p.Color1, .6f)); m.Ball(-250, 180, 0, 14, m.C(p.Color1, .5f));
        });
        Q(d, typeof(EyeBeamParameter), (p, m, t, v, v2) => // 目からビーム: 光る目と、手前へ伸びる太い光の柱
        {
            float k = Ease(In(m, 0, .3f));
            foreach (int s in Sides) { m.Ball(s * 90, 0, 0, 30, W); m.Limb(s * 90, 0, 10, s * 90 + 400 * k, 100 * k, 600 * k, 24, 50, m.C(p.Color1, .7f)); m.Limb(s * 90, 0, 10, s * 90 + 400 * k, 100 * k, 600 * k, 10, 20, m.C(0xFFFFFF, .8f)); }
        });
        Q(d, typeof(WordsPierceParameter), (p, m, t, v, v2) => // 言葉が刺さる: 飛んでくる厚い文字の矢が、胸に刺さってゆれる
        {
            float k = Ease(In(m, 0, .4f)), sh = k >= 1 ? MathF.Sin(m.Now * 30) * 3 * MathF.Exp(-(m.Now - .4f) * 4) : 0;
            float x = -500 * (1 - k) + sh;
            m.Limb(x - 400, -20, 0, x, 0, 0, 10, 10, m.C(p.Color1)); m.Limb(x - 20, 0, 0, x + 40, 0, 0, 40, 1, m.C(p.Color1));
            m.Text(Str(p), 60, x - 220, -70, 10, 8, m.C(p.Color1), 400);
            m.Ball(100, 0, -60, 120, 160, 60, m.C(0xFFD9B3, .5f));
        });
    }
}
