using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 197番(季節2 8)・198番(ファンタジー2 10)・199番(背景2 9)・200番(解説の図3 20)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>部屋の背景: 奥の壁・床・天井(本当に奥行きのある箱の中)</summary>
    static void Room(Sculpt m, Vector4 wall, Vector4 floor, float depth = 1400, Vector4? ceil = null)
    {
        float w = m.W * .6f, h = m.H * .55f, z0 = 300, z1 = -depth;
        m.Quad3D(new(-w, -h, z1), new(w, -h, z1), new(w, h, z1), new(-w, h, z1), wall);
        m.Quad3D(new(-w, h, z1), new(w, h, z1), new(w, h, z0), new(-w, h, z0), floor);
        foreach (int s in Sides) m.Quad3D(new(s * w, -h, z1), new(s * w, h, z1), new(s * w, h, z0), new(s * w, -h, z0), Sculpt.Dark(wall, .85f));
        m.Quad3D(new(-w, -h, z1), new(w, -h, z1), new(w, -h, z0), new(-w, -h, z0), ceil ?? Sculpt.Dark(wall, .7f));
    }

    static partial void RegisterJ1a(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.12f, .13f, .16f, 1);
        // ---------------------------------------------------------------- 197 季節2
        QS(d, typeof(SummerCalendarParameter), (p, m, t, v, v2) => // 夏休みの日めくり: 青空と入道雲、めくれていく日めくり
        {
            SkyWall(m, m.C(0x2E9BFF), m.C(0xBFE6FF), -900);
            for (int i = 0; i < 10; i++) { float f = i / 9f; m.Ball(m.W * .2f + (DrawKit.H(p.Seed, i) - .5f) * 500, m.H * .1f - f * 400, -800, 150 * (1 - f * .4f), W); }
            float fl = (m.Now * .7f) % 1;
            m.Box(-m.W * .2f, 0, 0, 360, 420, 50, W, .05f); m.Box(-m.W * .2f, -170, 10, 360, 80, 54, m.C(p.Color1), .1f);
            m.Text(Str(p), 160, -m.W * .2f, 40, 26, 8, K);
            m.Pitch(-m.W * .2f, -130, 28, -MathF.PI * fl, () => m.Box(-m.W * .2f, 40, 28, 350, 340, 3, m.C(0xFFFFFF, 1 - fl), .02f));
        });
        QS(d, typeof(SportsDayParameter), (p, m, t, v, v2) => // 運動会: 奥へわたる万国旗の綱と、手前のピストルと煙
        {
            for (int r = 0; r < 2; r++)
            {
                float z = -600 + r * 300; var rope = Enumerable.Range(0, 21).Select(i => { float f = i / 20f; return new Vector3(-m.W / 2 + m.W * f, -m.H * .3f + MathF.Sin(f * MathF.PI) * 80 + r * 60, z); }).ToList();
                m.Tube(rope, 2, m.C(0x555555));
                for (int i = 1; i < 20; i++) { var q = rope[i]; float sw = MathF.Sin(m.Now * 4 + i) * .2f; m.Pitch(q.X, q.Y, q.Z, sw, () => m.Tri3D(new(q.X - 30, q.Y, q.Z), new(q.X + 30, q.Y, q.Z), new(q.X, q.Y + 60, q.Z), Pal(m, i + r))); }
            }
            m.Box(m.W * .25f, m.H * .2f, 100, 160, 50, 40, K, .3f); m.Box(m.W * .25f - 40, m.H * .2f + 50, 100, 40, 90, 40, K, .3f);
            float f = (m.Now * .5f) % 1; m.Ball(m.W * .25f + 120 + 100 * f, m.H * .2f - 40 * f, 100, 30 + 60 * f, m.C(0xFFFFFF, .6f * (1 - f)));
        });
        Q(d, typeof(NewYearHagoitaParameter), (p, m, t, v, v2) => // 羽子板: 厚い板の羽子板と柄、飛ぶ羽根(玉と羽)
        {
            m.Turn(0, 0, -.3f, () => { m.Box(0, -80, 0, 260, 400, 24, m.C(p.Color1), .3f); m.Box(0, 170, 0, 70, 140, 24, m.C(0xC8A070), .3f); m.Text(Str(p), 60, 0, -80, 14, 4, W, 220); });
            float f = (m.Now * .6f) % 1, x = 200 + 200 * f, y = -300 + 200 * MathF.Sin(f * MathF.PI);
            m.Ball(x, y, 40, 22, K); foreach (int s in Sides) m.Limb(x, y, 40, x + s * 20, y - 60, 40, 8, 2, Pal(m, s + 2));
        });
        Q(d, typeof(TreeLightUpParameter), (p, m, t, v, v2) => // クリスマスツリー: 段の円すい、点いていく電球の玉、てっぺんの星
        {
            for (int i = 0; i < 4; i++) { float y = -260 + i * 120, w = 100 + i * 70; m.Lathe(0, 0, m.C(p.Color1), 0, y - 40, w * .5f, y + 20, w, y + 110, 0, y + 110); }
            m.Cyl(0, 360, 0, 40, 100, m.C(0x8A5A32), .2f);
            float on = In(m, .3f, 2);
            for (int i = 0; i < 30; i++) { float f = i / 30f, ag = i * 2.4f, y = -280 + f * 520, r = 40 + f * 300; if (f > on) continue; m.Ball(MathF.Cos(ag) * r, y, MathF.Sin(ag) * r, 12, Pal(m, i) with { W = (.6f + .4f * MathF.Sin(m.Now * 6 + i)) * m.Al }); }
            Gem(m, Pts(StarPts(0, -330, 60, 5, .45f, 0)), 0, -330, 0f, 6, 20, m.C(0xFFD54A), m.C(0xFFD54A));
        });
        QS(d, typeof(FestivalStallsParameter), (p, m, t, v, v2) => // 屋台の列: 奥へ並ぶ屋台の屋根と提灯、夜空に開く花火
        {
            SkyWall(m, m.C(0x0A1030), m.C(0x1A2050), -2000);
            for (int i = 0; i < 6; i++) { float z = 200 - i * 350; foreach (int s in Sides) { m.Box(s * 600, m.H * .2f, z, 400, 300, 280, m.C(0x8A5A32), .05f); m.Box(s * 600, m.H * .2f - 170, z, 440, 40, 320, Pal(m, i), .1f); m.Ball(s * 400, m.H * .2f - 120, z, 30, 40, 30, m.C(p.Color1)); } }
            float f = (m.Now * .4f) % 1; for (int k = 0; k < 24; k++) { float ag = k * MathF.Tau / 24; m.Ball(MathF.Cos(ag) * 300 * f, -m.H * .25f + MathF.Sin(ag) * 300 * f, -1800 + MathF.Sin(ag * 3) * 200 * f, 10, Pal(m, k) with { W = (1 - f) * m.Al }); }
        });
        QS(d, typeof(SnowFestivalParameter), (p, m, t, v, v2) => // かまくら: 雪の地面、半球のかまくらと入口の灯り、降る雪
        {
            SkyWall(m, m.C(0x2A3A60), m.C(0x6A7AA0), -1500);
            m.Quad3D(new(-m.W, m.H * .2f, -1500), new(m.W, m.H * .2f, -1500), new(m.W, m.H * .2f, 400), new(-m.W, m.H * .2f, 400), W);
            var prof = new List<Vector2>(); for (int i = 0; i <= 10; i++) { float a0 = i / 10f * 1.5708f; prof.Add(new(MathF.Sin(a0) * 300, m.H * .2f - MathF.Cos(a0) * 240)); }
            m.Lathe(0, -400, W, prof);
            m.Disc(0, m.H * .2f - 70, -100, 90, 80, 10, m.C(0xFFB74D), .5f);
            for (int i = 0; i < 40; i++) { float y = ((DrawKit.H(p.Seed, i) * m.H + m.Now * 60) % m.H) - m.H / 2; m.Ball((DrawKit.H(p.Seed, i, 1) - .5f) * m.W, y, (DrawKit.H(p.Seed, i, 2) - .5f) * 800, 6, W); }
        });
        Q(d, typeof(CarnationMothersDayParameter), (p, m, t, v, v2) => // カーネーション: ふりふりの花びらの花、茎と葉、リボンの札
        {
            for (int k = 0; k < 3; k++) { float x = (k - 1) * 200, y = -100 + (k == 1 ? -60 : 0); m.Limb(x, y, 0, x * .3f, 300, 0, 6, 6, m.C(0x4E8F4A)); for (int i = 0; i < 14; i++) { float ag = i * 2.4f, r = 20 + i * 4; m.Ball(x + MathF.Cos(ag) * r, y + MathF.Sin(ag) * r * .6f, 30 - i * 2, 34, 26, 16, m.C(p.Color1)); } }
            m.Box(0, 260, 40, 360, 70, 20, m.C(0xFFFFFF), .4f); m.Text(Str(p), 40, 0, 260, 52, 4, m.C(p.Color1));
        });
        QS(d, typeof(NewYearCountdownParameter), (p, m, t, v, v2) => // 年越しカウントダウン: 夜空、手前に飛び出す大きな数字と花火
        {
            SkyWall(m, m.C(0x060818), m.C(0x141A3A), -1500);
            int n = Math.Max(0, 10 - (int)m.Now); float f = m.Now % 1;
            if (n > 0) m.Text(n.ToString(), 400, 0, 0, 200 * f, 60, m.C(0xFFD54A, 1 - f * .5f), 0, m.C(0xFF7043), 10);
            else { m.Text(Str(p).Length > 0 ? Str(p) : "HAPPY NEW YEAR", 140, 0, 0, 40, 30, m.C(0xFFD54A)); for (int k = 0; k < 30; k++) { float ag = k * MathF.Tau / 30, r = 500 * ((m.Now - 10) % 1); m.Ball(MathF.Cos(ag) * r, MathF.Sin(ag) * r, -800, 12, Pal(m, k)); } }
        });

        // ---------------------------------------------------------------- 198 ファンタジー2
        Q(d, typeof(PotionBottleParameter), (p, m, t, v, v2) => // ポーションの瓶: 丸いガラス瓶と色の液、栓、立ちのぼる泡
        {
            var prof = new List<Vector2> { new(0, 260) }; for (int i = 1; i <= 12; i++) { float th = i / 12f * 2.4f; prof.Add(new(MathF.Sin(th) * 180, 80 + MathF.Cos(th) * 180)); } prof.Add(new(60, -120)); prof.Add(new(60, -200)); prof.Add(new(0, -200));
            m.Lathe(0, 0, m.C(0xFFFFFF, .3f), prof);
            var liq = new List<Vector2> { new(0, 255) }; for (int i = 1; i <= 8; i++) { float th = i / 8f * 1.9f; liq.Add(new(MathF.Sin(th) * 172, 80 + MathF.Cos(th) * 172)); } liq.Add(new(0, liq[^1].Y));
            m.Lathe(0, 0, m.C(p.Color1, .85f), liq);
            m.Cyl(0, -230, 0, 52, 60, m.C(0xB9823E), .3f);
            for (int i = 0; i < 6; i++) { float f = ((m.Now * .5f + i / 6f) % 1); m.Ball((DrawKit.H(p.Seed, i) - .5f) * 200, 220 - f * 200, (DrawKit.H(p.Seed, i, 1) - .5f) * 160, 8 + 6 * f, m.C(0xFFFFFF, .6f * (1 - f))); }
        });
        Q(d, typeof(WandTrailParameter), (p, m, t, v, v2) => // 魔法の杖: 振られる杖と先の星、うしろに残る光の粒
        {
            float ag = MathF.Sin(m.Now * 2) * .8f;
            m.Turn(0, 200, ag, () => { m.Limb(0, 200, 0, 0, -150, 0, 10, 7, m.C(0x6D4C41)); Gem(m, Pts(StarPts(0, -190, 50, 5, .45f, m.Now)), 0, -190, 0f, 6, 18, m.C(p.Color1), m.C(p.Color1)); });
            for (int i = 0; i < 30; i++) { float tt = m.Now - i * .03f, a1 = MathF.Sin(tt * 2) * .8f; var q = new Vector2(MathF.Sin(a1) * 390, 200 - MathF.Cos(a1) * 390); m.Ball(q.X, q.Y, -i * 4, 10 * (1 - i / 30f), m.C(p.Color1, 1 - i / 30f)); }
        });
        QS(d, typeof(TimeStopParameter), (p, m, t, v, v2) => // 時間停止: 灰色に沈む画面、止まった大きな時計の輪と針
        {
            m.Box(0, 0, -200, m.W, m.H, 10, m.C(0x404050, .5f), .01f);
            m.Torus(0, 0, 0, 300, 14, m.C(p.Color1)); for (int i = 0; i < 12; i++) { float ag = i * MathF.PI / 6; m.Ball(MathF.Cos(ag) * 260, MathF.Sin(ag) * 260, 0, 10, m.C(p.Color1)); }
            m.Limb(0, 0, 10, 0, -220, 10, 8, 4, m.C(p.Color1)); m.Limb(0, 0, 14, 150, 0, 14, 10, 5, m.C(p.Color1));
            float f = In(m, 0, .5f); m.Torus(0, 0, -50, 300 + 600 * f, 6, m.C(p.Color1, 1 - f));
        });
        QS(d, typeof(TeleportBeamParameter), (p, m, t, v, v2) => // 転送ビーム: 上から降りる光の柱と、上へ消えていく光の粒
        {
            float k = In(m, 0, 1.5f);
            m.Cyl(0, -m.H / 4, 0, 150, m.H * 1.5f, m.C(p.Color1, .25f), .05f); m.Cyl(0, -m.H / 4, 0, 60, m.H * 1.5f, m.C(0xFFFFFF, .4f), .05f);
            for (int i = 0; i < 40; i++) { float ag = DrawKit.H(p.Seed, i) * MathF.Tau, r = 140 * DrawKit.H(p.Seed, i, 1), y = m.H / 3 - ((k * 600 + DrawKit.H(p.Seed, i, 2) * 600) % 900); m.Ball(MathF.Cos(ag) * r, y, MathF.Sin(ag) * r, 6, m.C(p.Color1)); }
            m.Pitch(0, m.H * .35f, 0, MathF.PI / 2, () => m.Torus(0, m.H * .35f, 0, 170, 12, m.C(p.Color1)));
        });
        QS(d, typeof(RobotEyeCloseupParameter), (p, m, t, v, v2) => // ロボの目のアップ: 金属の顔の板とボルト、何重ものレンズが光る
        {
            m.Box(0, 0, -100, m.W, m.H, 60, m.C(0x37474F), .02f);
            for (int i = 0; i < 8; i++) m.Ball((i - 3.5f) * m.W / 8, -m.H * .4f, -60, 16, m.C(0x90A4AE));
            float on = In(m, .2f, .6f);
            m.Torus(0, 0, -60, 300, 40, m.C(0x546E7A)); m.Torus(0, 0, -40, 220, 20, m.C(0x263238));
            m.Ball(0, 0, -60, 200, 200, 80 * on + 10, m.C(p.Color1, on)); m.Ball(0, 0, 20 * on, 60, m.C(0xFFFFFF, on));
            for (int i = 0; i < 6; i++) m.Turn(0, 0, m.Now + i * MathF.PI / 3, () => m.Box(0, -250, -40, 30, 80, 20, m.C(0x263238), .2f));
        });
        QS(d, typeof(SpaceCommParameter), (p, m, t, v, v2) => // 宇宙の通信画面: 枠の付いた画面、波形の線、走るノイズ、文字
        {
            m.Box(0, 0, -20, m.W * .8f, m.H * .7f, 20, m.C(0x0A1A2A, .9f), .03f);
            m.Tube([new(-m.W * .4f, -m.H * .35f, 0), new(m.W * .4f, -m.H * .35f, 0), new(m.W * .4f, m.H * .35f, 0), new(-m.W * .4f, m.H * .35f, 0), new(-m.W * .4f, -m.H * .35f, 0)], 6, m.C(p.Color1));
            m.Tube([.. Enumerable.Range(0, 60).Select(i => new Vector3(-m.W * .35f + m.W * .7f * i / 59f, MathF.Sin(i * .5f + m.Now * 6) * 60 * MathF.Sin(m.Now * 3 + i * .1f), 4))], 3, m.C(p.Color1));
            m.Text(Str(p), 50, 0, m.H * .25f, 4, 4, m.C(p.Color1), m.W * .7f);
        });
        Q(d, typeof(BarrierDomeParameter), (p, m, t, v, v2) => // 結界: 透けた半球のドームと、六角の網目、地面の輪
        {
            float R = 360 * Ease(In(m, 0, .6f));
            var prof = new List<Vector2>(); for (int i = 0; i <= 12; i++) { float a0 = i / 12f * 1.5708f; prof.Add(new(MathF.Sin(a0) * R, 200 - MathF.Cos(a0) * R)); }
            m.Lathe(0, 0, m.C(p.Color1, .22f), prof);
            for (int i = 0; i < 6; i++) m.Yaw(0, 0, 0, i * MathF.PI / 6, () => m.Tube(ArcPts3(0, 200, R, MathF.PI, MathF.Tau, 0, 16), 2, m.C(p.Color1, .7f)));
            m.Pitch(0, 200, 0, MathF.PI / 2, () => m.Torus(0, 200, 0, R, 6, m.C(p.Color1)));
        });
        Q(d, typeof(SwordClashParameter), (p, m, t, v, v2) => // 剣の打ち合い: 交差する2本の剣と、飛び散る火花
        {
            foreach (int s in Sides) m.Turn(0, 0, s * .7f + MathF.Sin(m.Now * 10) * .03f, () => { m.Box(0, -150, s * 20, 40, 400, 10, m.C(0xCFD8DC), .1f); m.Box(0, 70, s * 20, 140, 30, 30, m.C(0xC9A227), .3f); m.Cyl(0, 140, s * 20, 18, 110, m.C(0x5D4037), .2f); });
            for (int i = 0; i < 20; i++) { float f = ((m.Now * 2 + DrawKit.H(p.Seed, i)) % 1), ag = DrawKit.H(p.Seed, i, 1) * MathF.Tau; m.Limb(MathF.Cos(ag) * 200 * f, -60 + MathF.Sin(ag) * 200 * f, MathF.Sin(ag * 2) * 100 * f, MathF.Cos(ag) * (200 * f + 30), -60 + MathF.Sin(ag) * (200 * f + 30), MathF.Sin(ag * 2) * 100 * f, 3, 1, m.C(p.Color1, 1 - f)); }
        });
        Q(d, typeof(ShadowCloneParameter), (p, m, t, v, v2) => // 影分身: 前後左右にずれて並ぶ、半透明の人影
        {
            int n = Math.Clamp(p.Count, 2, 7);
            for (int i = 0; i < n; i++) { float f = i / (float)(n - 1) - .5f, x = f * 700, z = -MathF.Abs(f) * 400, a0 = i == n / 2 ? 1 : .45f; var c = m.C(p.Color1, a0); m.Ball(x, -220, z, 60, c); m.Limb(x, -150, z, x, 80, z, 70, 55, c); foreach (int s in Sides) { m.Limb(x + s * 30, 80, z, x + s * 50, 320, z, 26, 22, c); m.Limb(x + s * 60, -130, z, x + s * 110, 60, z, 20, 18, c); } }
        });
        QS(d, typeof(CardSummonParameter), (p, m, t, v, v2) => // 召喚陣: 床に寝た光る魔法陣と、せり上がってくる1枚のカード
        {
            m.Pitch(0, m.H * .25f, 0, MathF.PI / 2 - .2f, () =>
            {
                m.Torus(0, m.H * .25f, 0, 400, 6, m.C(p.Color1)); m.Torus(0, m.H * .25f, 0, 300, 4, m.C(p.Color1));
                m.Turn(0, m.H * .25f, m.Now * .5f, () => Gem(m, Pts(StarPts(0, m.H * .25f, 300, 6, .58f, 0)), 0, m.H * .25f, 0f, 2, 6, m.C(p.Color1, .8f), m.C(p.Color1, .8f)));
            });
            float k = Ease(In(m, .5f, 1));
            m.Yaw(0, 0, 0, (1 - k) * MathF.Tau, () => { m.Box(0, m.H * .2f - 400 * k, 0, 260, 380, 8, m.C(0x1A237E), .05f); m.Box(0, m.H * .2f - 400 * k, 5, 230, 350, 3, m.C(0xFFD54A), .05f); });
        });

        // ---------------------------------------------------------------- 199 背景2
        Q(d, typeof(ColorFocusLinesParameter), (p, m, t, v, v2) => // 色つき集中線: 中心に向かって奥へすぼまる、回る色の三角の板
        {
            int n = Math.Max(16, p.Count); float rot = m.Now * .3f, R = 1400;
            for (int i = 0; i < n; i++) { float a0 = rot + i * MathF.Tau / n, w = .02f + .02f * DrawKit.H(p.Seed, i); m.Tri3D(new(MathF.Cos(a0 - w) * R, MathF.Sin(a0 - w) * R, 0), new(MathF.Cos(a0 + w) * R, MathF.Sin(a0 + w) * R, 0), new(MathF.Cos(a0) * 200, MathF.Sin(a0) * 200, -600), Pal(m, i)); }
        });
        QS(d, typeof(NeonRainStreetParameter), (p, m, t, v, v2) => // 雨のネオン街: 奥へ続く両側のビルと光る看板、ぬれた地面、雨
        {
            SkyWall(m, m.C(0x0A0A1A), m.C(0x1A1030), -2500);
            m.Quad3D(new(-m.W, m.H * .4f, -2500), new(m.W, m.H * .4f, -2500), new(m.W, m.H * .4f, 400), new(-m.W, m.H * .4f, 400), m.C(0x14142A));
            for (int i = 0; i < 8; i++) { float z = 200 - i * 330; foreach (int s in Sides) { m.Box(s * 800, 0, z, 300, m.H * 1.2f, 300, m.C(0x1A1A2A), .02f); m.Box(s * 640, -m.H * .1f + (i % 3) * 80, z, 10, 120, 200, Pal(m, i + s), .3f); } }
            for (int i = 0; i < 60; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W * 1.5f, y = ((m.Now * 900 + DrawKit.H(p.Seed, i, 1) * m.H) % m.H) - m.H / 2, z = -DrawKit.H(p.Seed, i, 2) * 1500; m.Limb(x, y, z, x - 6, y + 40, z, 1.5f, 1.5f, m.C(0xAAD4FF, .5f)); }
        });
        QS(d, typeof(ClassroomParameter), (p, m, t, v, v2) => // 教室: 奥の壁の黒板と文字、窓、床に並ぶ机と椅子
        {
            Room(m, m.C(0xF2EAD8), m.C(0xC8A070));
            m.Box(0, -m.H * .15f, -1390, m.W * .7f, m.H * .4f, 20, m.C(0x2F5D44), .02f); m.Text(Str(p), 80, 0, -m.H * .15f, -1375, 4, W, m.W * .65f);
            for (int i = 0; i < 3; i++) m.Box(m.W * .6f - 5, -m.H * .1f, -1000 + i * 400, 10, m.H * .35f, 300, m.C(0xBFE6FF), .02f);
            for (int r = 0; r < 3; r++) for (int c = -2; c <= 2; c++) { float x = c * 280, z = -800 + r * 330, y = m.H * .55f; m.Box(x, y - 160, z, 180, 14, 120, m.C(0xC8A070), .1f); foreach (int sx in Sides) foreach (int sz in Sides) m.Limb(x + sx * 80, y - 150, z + sz * 50, x + sx * 80, y, z + sz * 50, 4, 4, m.C(0x8A95A0)); m.Box(x, y - 110, z + 120, 120, 12, 100, m.C(0xC8A070), .1f); }
        });
        QS(d, typeof(OfficeDeskParameter), (p, m, t, v, v2) => // オフィス: 壁と窓、デスクとPC、観葉植物
        {
            Room(m, m.C(0xE8ECEF), m.C(0x9AA3AD));
            m.Box(0, -m.H * .2f, -1390, m.W * .8f, m.H * .4f, 10, m.C(0xBFE6FF), .02f);
            m.Box(0, m.H * .25f, -300, 900, 30, 400, m.C(0xD9C3A0), .05f);
            m.Box(-150, m.H * .1f, -400, 300, 200, 20, K, .05f); m.Box(-150, m.H * .1f, -388, 280, 180, 4, m.C(p.Color1), .05f);
            m.Lathe(400, -300, m.C(0xA85E38), 0, m.H * .25f - 15, 60, m.H * .25f - 15, 80, m.H * .25f - 120, 0, m.H * .25f - 120);
            for (int i = -3; i <= 3; i++) m.Turn(400, m.H * .25f - 120, i * .35f, () => m.Ball(400, m.H * .25f - 220, -300, 26, 100, 10, m.C(0x3B8F4B)));
        });
        QS(d, typeof(ConvenienceStoreNightParameter), (p, m, t, v, v2) => // 夜のコンビニ前: 夜空、光る店の箱と看板、駐車場の白線
        {
            SkyWall(m, m.C(0x0A0F25), m.C(0x1A2040), -1500);
            m.Box(0, 0, -800, m.W * .9f, m.H * .7f, 400, m.C(0xF5F5F5), .02f);
            m.Box(0, -m.H * .25f, -595, m.W * .9f, 80, 20, m.C(p.Color1), .05f);
            m.Box(0, m.H * .05f, -595, m.W * .8f, m.H * .4f, 6, m.C(0xFFF8E0, .9f), .02f);
            m.Quad3D(new(-m.W, m.H * .35f, -600), new(m.W, m.H * .35f, -600), new(m.W, m.H * .35f, 400), new(-m.W, m.H * .35f, 400), m.C(0x303038));
            for (int i = -3; i <= 3; i++) m.Box(i * 300, m.H * .35f - 2, -100, 10, 4, 600, W, .3f);
        });
        QS(d, typeof(GameCenterParameter), (p, m, t, v, v2) => // ゲームセンター: 暗い部屋、並ぶ筐体の箱と光る画面
        {
            Room(m, m.C(0x140A20), m.C(0x201030), 1600);
            for (int i = 0; i < 5; i++) foreach (int s in Sides) { float z = -1300 + i * 300, x = s * m.W * .45f; m.Box(x, m.H * .15f, z, 220, m.H * .6f, 220, m.C(0x2A2A3A), .05f); m.Box(x - s * 112, 0, z, 4, 160, 180, Pal(m, i + s) with { W = (.6f + .4f * MathF.Sin(m.Now * 4 + i)) * m.Al }, .1f); }
        });
        QS(d, typeof(InfirmaryRoomParameter), (p, m, t, v, v2) => // 保健室: 白い部屋、カーテンで仕切ったベッド、窓
        {
            Room(m, m.C(0xF5F7F8), m.C(0xD8DEE3));
            m.Box(-m.W * .2f, m.H * .3f, -800, 700, 120, 400, W, .2f); m.Box(-m.W * .2f - 300, m.H * .2f, -800, 150, 60, 380, m.C(0xFFFFFF), .4f);
            for (int i = 0; i < 8; i++) m.Cyl(m.W * .1f + i * 60, 0, -800 + MathF.Sin(i) * 30, 30, m.H * .9f, m.C(p.Color1, .7f), .1f);
            m.Box(m.W * .4f, -m.H * .1f, -1390, 500, 400, 10, m.C(0xBFE6FF), .02f);
        });
        Q(d, typeof(FisheyeFrameParameter), (p, m, t, v, v2) => // 魚眼レンズ枠: ふくらんだ透けたレンズと、厚い丸い黒い枠
        {
            HoleMask(m, Ellipse(0, 0, 500, 500), m.C(p.Color1), 0, 60);
            m.Torus(0, 0, 10, 500, 20, m.C(0x333338));
            m.Ball(0, 0, 0, 500, 500, 120, m.C(0xFFFFFF, .08f));
        });
        QS(d, typeof(TvStaticParameter), (p, m, t, v, v2) => // テレビの砂嵐: 厚いテレビの枠と、ちらつく粒の画面
        {
            m.Box(0, 0, -40, m.W * .95f, m.H * .95f, 80, m.C(0x2A2A2A), .05f);
            int fr = (int)(m.Now * 30); int keep = m.Seg; m.Seg = 4;
            for (int i = 0; i < 300; i++) m.Box((DrawKit.H(fr, i) - .5f) * m.W * .85f, (DrawKit.H(fr, i, 1) - .5f) * m.H * .8f, 4, 14, 6, 2, m.C(0xFFFFFF, DrawKit.H(fr, i, 2)), .1f);
            m.Seg = keep;
            m.Text(Str(p), 80, m.W * .3f, -m.H * .33f, 10, 4, m.C(0x39FF14));
        });

        // ---------------------------------------------------------------- 200 解説の図3
        Q(d, typeof(NormalCurveParameter), (p, m, t, v, v2) => // 正規分布: 奥行きのあるベル型の山の板と、平均の線
        {
            var pts = new List<Vector2> { new(600, 200), new(-600, 200) };
            for (int i = 0; i <= 60; i++) { float x = -600 + 1200 * i / 60f, z = x / 200; pts.Add(new(x, 200 - 380 * MathF.Exp(-z * z / 2) * Ease(In(m, .2f, 1)))); }
            pts.RemoveAt(0); pts.RemoveAt(0); pts.Insert(0, new(-600, 200)); pts.Add(new(600, 200));
            m.Slab(pts, 0, 120, m.C(p.Color1, .85f));
            m.Limb(0, 200, 70, 0, -200, 70, 3, 3, W); m.Limb(-650, 200, 0, 650, 200, 0, 3, 3, W);
        });
        Q(d, typeof(HistogramParameter), (p, m, t, v, v2) => // ヒストグラム: すき間なく並ぶ、高さの違う立体の柱
        {
            var vals = Nums(p); if (vals.Count == 0) vals = [.. Enumerable.Range(0, 9).Select(i => 10 + 40 * MathF.Exp(-(i - 4) * (i - 4) / 6f))];
            float mx = MathF.Max(1, vals.Max()), bw = 1100f / vals.Count, k = Ease(In(m, .2f, 1));
            for (int i = 0; i < vals.Count; i++) { float hh = 500 * vals[i] / mx * k; m.Box(-550 + bw * (i + .5f), 250 - hh / 2, 0, bw - 2, hh, 160, m.C(p.Color1), .02f); }
            m.Limb(-600, 250, 0, 600, 250, 0, 3, 3, W);
        });
        QS(d, typeof(BoxPlotParameter), (p, m, t, v, v2) => // 箱ひげ図: 並ぶ立体の箱と、上下のひげの棒、中央の線
        {
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * 300, md = 40 * MathF.Sin(i * 1.7f), q1 = md + 80, q3 = md - 90, lo = q1 + 140, hi = q3 - 150;
                m.Limb(x, lo, 0, x, hi, 0, 3, 3, W); m.Limb(x - 40, lo, 0, x + 40, lo, 0, 3, 3, W); m.Limb(x - 40, hi, 0, x + 40, hi, 0, 3, 3, W);
                m.Box(x, (q1 + q3) / 2, 0, 140, q1 - q3, 100, Pal(m, i), .05f); m.Box(x, md, 52, 140, 6, 4, W, .3f);
            }
        });
        QS(d, typeof(WaterfallChartParameter), (p, m, t, v, v2) => // ウォーターフォール図: 前の値から続いて上下する、浮いた柱と点線
        {
            var vals = Nums(p); if (vals.Count == 0) vals = [100, 30, -20, 45, -15];
            float cum = 0, mx = MathF.Max(1, vals.Select((_, i) => vals.Take(i + 1).Sum()).Max()) * 1.2f, bw = 1100f / (vals.Count + 1);
            for (int i = 0; i < vals.Count; i++) { float y0 = 250 - 500 * cum / mx; cum += vals[i]; float y1 = 250 - 500 * cum / mx, k = Ease(In(m, i * .2f, .3f)); m.Box(-550 + bw * (i + .5f), (y0 + y1) / 2, 0, bw * .7f, MathF.Abs(y1 - y0) * k + 1, 100, vals[i] >= 0 ? m.C(0x43A047) : m.C(0xE53935), .05f); }
            m.Box(-550 + bw * (vals.Count + .5f), 250 - 250 * cum / mx, 0, bw * .7f, 500 * cum / mx, 100, m.C(p.Color1), .05f);
        });
        Q(d, typeof(FunnelChartParameter), (p, m, t, v, v2) => // ファネル: 下ほど細くなる、重なった円すい台
        {
            var it = Lines(p); int n = Math.Max(3, it.Count);
            for (int i = 0; i < n; i++) { float y = -280 + i * 140, w0 = 400 - i * 90, w1 = w0 - 80, k = Ease(In(m, i * .2f, .4f)); m.Lathe(0, 0, Mix(m, p.Color1, p.Color2, i / (float)(n - 1)), 0, y, w0 * k, y, w1 * k, y + 130, 0, y + 130); if (i < it.Count) m.Text(it[i], 40, 0, y + 65, w0 * .9f + 4, 4, W); }
        });
        Q(d, typeof(HeatmapParameter), (p, m, t, v, v2) => Diagram3D(m, t, m.C(p.Color1), m.C(p.Color2), 25));
        Q(d, typeof(RoadmapParameter), (p, m, t, v, v2) => // ロードマップ: うねって奥へ続く道と、立てた旗(項目)
        {
            var it = Lines(p);
            var path = Enumerable.Range(0, 30).Select(i => { float f = i / 29f; return new Vector3(MathF.Sin(f * 5) * 400, 250, 300 - f * 1500); }).ToList();
            for (int i = 0; i < path.Count - 1; i++) m.Quad3D(path[i] + new Vector3(-60, 0, 0), path[i] + new Vector3(60, 0, 0), path[i + 1] + new Vector3(60, 0, 0), path[i + 1] + new Vector3(-60, 0, 0), m.C(0x9E8E70));
            for (int i = 0; i < it.Count; i++) { var q = path[Math.Min(path.Count - 1, (i + 1) * 29 / (it.Count + 1))]; float k = In(m, i * .3f, .3f); if (k <= 0) continue; m.Limb(q.X + 80, q.Y, q.Z, q.X + 80, q.Y - 200, q.Z, 3, 3, W); m.Box(q.X + 160, q.Y - 170, q.Z, 160, 70, 6, Pal(m, i), .1f); m.Text(it[i], 30, q.X + 160, q.Y - 170, q.Z + 4, 2, W, 150); }
        });
        Q(d, typeof(LeverDiagramParameter), (p, m, t, v, v2) => // てこ: 三角の支点、傾く板、両端の重りの箱
        {
            float tilt = (v - .5f) * .5f;
            Gem(m, Pts(0, 0, 70, 120, -70, 120), 0, 60, 0f, 40, 40, m.C(p.Color1), m.C(p.Color1));
            m.Turn(0, 0, tilt, () => { m.Box(0, -10, 0, 900, 20, 120, m.C(0xC8A070), .1f); m.Box(-380, -70, 0, 100, 100, 100, m.C(0x8A95A0), .1f); m.Box(380, -55, 0, 70, 70, 70, m.C(0x8A95A0), .1f); });
            m.Limb(-600, 120, 0, 600, 120, 0, 3, 3, W);
        });
        Q(d, typeof(CircuitDiagramParameter), (p, m, t, v, v2) => // 回路: 円柱の電池、導線の管、点く豆電球とスイッチ
        {
            m.Turn(-300, 0, MathF.PI / 2, () => m.Cyl(-300, 0, 0, 50, 180, m.C(0xE53935), .2f));
            m.Tube([new(-300, -100, 0), new(-300, -250, 0), new(300, -250, 0), new(300, 250, 0), new(-300, 250, 0), new(-300, 100, 0)], 5, m.C(0xC9A227));
            float on = (m.Now % 2) < 1.5f ? 1 : .2f;
            m.Ball(300, 0, 20, 70, m.C(0xFFEB3B, on)); m.Cyl(300, 80, 20, 40, 50, m.C(0x9E9E9E), .3f);
            m.Turn(0, 250, on > .5f ? 0 : -.4f, () => m.Limb(-60, 250, 10, 60, 250, 10, 6, 6, K));
        });
        Q(d, typeof(ChemEquationParameter), (p, m, t, v, v2) => // 化学反応式: 玉と棒の分子が左右に並び、間の矢印と式の文字
        {
            void Mol(float x, int a) { for (int i = 0; i < a; i++) { float ag = i * MathF.Tau / a; m.Ball(x + MathF.Cos(ag) * 50, MathF.Sin(ag) * 50 - 60, MathF.Sin(ag * 2) * 30, 36, i == 0 ? m.C(0xE53935) : W); if (i > 0) m.Limb(x + 50, -60, 0, x + MathF.Cos(ag) * 50, MathF.Sin(ag) * 50 - 60, 0, 6, 6, m.C(0xCFD8DC)); } }
            Mol(-400, 3); Mol(-150, 2); Arr3(m, 0, -60, 150, -60, 0, W); Mol(350, 4);
            m.Text(Str(p), 60, 0, 120, 0, 6, W, 1200);
        });
        Q(d, typeof(CellDiagramParameter), (p, m, t, v, v2) => // 細胞: 透けた膜の中に、核・ミトコンドリアなどの立体の粒
        {
            m.Ball(0, 0, 0, 450, 300, 200, m.C(p.Color1, .3f));
            m.Ball(0, 0, 0, 130, m.C(0x7E57C2)); m.Ball(0, 0, 60, 40, m.C(0x4527A0));
            for (int i = 0; i < 5; i++) { float ag = i * 1.25f; m.Turn(MathF.Cos(ag) * 280, MathF.Sin(ag) * 170, ag, () => m.Ball(MathF.Cos(ag) * 280, MathF.Sin(ag) * 170, 40, 60, 28, 28, m.C(0xFF7043))); }
            for (int i = 0; i < 20; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * 700, (DrawKit.H(p.Seed, i, 1) - .5f) * 400, (DrawKit.H(p.Seed, i, 2) - .5f) * 200, 8, m.C(0x43A047));
        });
        Q(d, typeof(BodySilhouetteParameter), (p, m, t, v, v2) => // 人体: 丸い体の人形と、光る臓器のしるし
        {
            var c = m.C(p.Color1, .8f);
            m.Ball(0, -330, 0, 80, c); m.Limb(0, -240, 0, 0, 60, 0, 120, 90, c);
            foreach (int s in Sides) { m.Limb(s * 140, -200, 0, s * 200, 80, 0, 35, 28, c); m.Limb(s * 60, 80, 0, s * 70, 400, 0, 45, 35, c); }
            Heart3D(m, -30, -120, 80, 40, m.C(0xE53935, .5f + .5f * MathF.Abs(MathF.Sin(m.Now * 4))));
        });
        Q(d, typeof(JapanTileMapParameter), (p, m, t, v, v2) => // 日本地図(タイル): 都道府県のタイルが並ぶ、高さの違う四角い柱
        {
            int[,] g = { { 0, 0, 0, 0, 0, 0, 0, 1 }, { 0, 0, 0, 0, 0, 0, 1, 1 }, { 0, 0, 0, 0, 0, 1, 1, 1 }, { 0, 0, 0, 1, 1, 1, 1, 0 }, { 0, 1, 1, 1, 1, 1, 0, 0 }, { 1, 1, 1, 1, 1, 0, 0, 0 }, { 1, 1, 0, 1, 0, 0, 0, 0 } };
            float cs = 90;
            for (int y = 0; y < 7; y++) for (int x = 0; x < 8; x++) if (g[y, x] == 1) { float hh = 20 + 60 * DrawKit.H(p.Seed, x + y * 8), k = Ease(In(m, (x + y) * .05f, .4f)); m.Box((x - 3.5f) * cs, (y - 3) * cs, hh * k / 2, cs - 6, cs - 6, hh * k, Mix(m, p.Color1, p.Color2, DrawKit.H(p.Seed, x + y * 8)), .1f); }
        });
        Q(d, typeof(WorldClocksParameter), (p, m, t, v, v2) => // 世界の時計: 並んだ厚い掛け時計(時差ぶん針がずれる)と都市名
        {
            var it = p.ItemsForModel; int n = Math.Clamp(Math.Max(3, it.Count), 1, 5);
            for (int i = 0; i < n; i++) { float x = (i - (n - 1) / 2f) * 360; m.Move(x, 0, () => m.Scale(0, 0, 0, .5f, () => WallClock(m, m.C(p.Color1), m.C(0xE53935), (3 + i * 4) % 12, (int)(m.Now * 10) % 60))); if (i < it.Count) m.Text(it[i], 40, x, 170, 0, 4, W); }
        });
        Q(d, typeof(StrataSectionParameter), (p, m, t, v, v2) => // 地層の断面: 色の違う層が重なる、切り取った土のブロック
        {
            uint[] cols = [0x8D6E63, 0xBCAAA4, 0xA1887F, 0x6D4C41, 0xD7CCC8, 0x5D4037];
            for (int i = 0; i < 6; i++) { float y = -250 + i * 90 + MathF.Sin(i) * 10, k = Ease(In(m, i * .1f, .4f)); m.Box(0, y, 0, 1000, 88, 400 * k + 20, m.C(cols[i]), .02f); }
            m.Box(0, -310, 0, 1000, 30, 420, m.C(0x7CB342), .05f);
        });
        Q(d, typeof(SolarOrbitParameter), (p, m, t, v, v2) => // 太陽系: 光る太陽と、傾いた軌道を回る惑星の玉
        {
            m.Ball(0, 0, 0, 80, m.C(0xFFC93C)); m.Ball(0, 0, 0, 110, m.C(0xFFC93C, .2f));
            m.Pitch(0, 0, 0, 1.1f, () => { for (int i = 0; i < 6; i++) { float R = 150 + i * 80, ag = m.Now * (1.2f / (i + 1)) + i; m.Torus(0, 0, 0, R, 1.5f, m.C(0xFFFFFF, .3f)); m.Ball(MathF.Cos(ag) * R, MathF.Sin(ag) * R, 0, 14 + i * 3, Pal(m, i)); } });
        });
        Q(d, typeof(HajikiDiagramParameter), (p, m, t, v, v2) => // はじきの図: 3つに区切った厚い丸い札(は・じ・き)
        {
            m.Disc(0, 0, 0, 300, 300, 60, m.C(p.Color1), .3f);
            m.Box(0, 0, 32, 600, 8, 6, W, .3f); m.Box(0, 150, 32, 8, 300, 6, W, .3f);
            m.Text("き", 120, 0, -130, 32, 10, W); m.Text("は", 100, -140, 120, 32, 10, W); m.Text("じ", 100, 140, 120, 32, 10, W);
        });
        Q(d, typeof(WordCardParameter), (p, m, t, v, v2) => // 単語カード: リングでとじた厚い札の束、めくれる1枚
        {
            var ln = Str(p).Split('\n'); float f = (m.Now * .4f) % 1;
            m.Torus(-260, -120, 0, 30, 6, m.C(0xC9A227));
            for (int i = 3; i >= 1; i--) m.Box(0, i * 6, -i * 6, 560, 300, 6, W, .1f);
            m.Pitch(0, -150, 4, -MathF.PI * Ease(f) * .9f, () => { m.Box(0, 0, 4, 560, 300, 6, m.C(p.Color1), .1f); m.Text(ln.ElementAtOrDefault(0) ?? "", 70, 0, 0, 10, 4, W, 500); });
            m.Text(ln.ElementAtOrDefault(1) ?? "", 60, 0, 0, -8, 4, K, 500);
        });
        Q(d, typeof(KanbanBoardParameter), (p, m, t, v, v2) => // カンバン: 3つの列の板と、移動していく付せんの札
        {
            string[] cols = ["ToDo", "Doing", "Done"]; var it = Lines(p);
            for (int c = 0; c < 3; c++) { float x = (c - 1) * 400; m.Box(x, 0, -10, 360, 600, 20, m.C(0xFFFFFF, .15f), .05f); m.Text(cols[c], 40, x, -260, 4, 4, W); }
            for (int i = 0; i < it.Count; i++) { float stage = Math.Clamp(m.Now * .4f - i * .3f, 0, 2), x = (stage - 1) * 400, y = -170 + i * 110; m.Box(x, y, 20, 320, 90, 16, Pal(m, i), .2f); m.Text(it[i], 32, x, y, 30, 3, W, 300); }
        });
        Q(d, typeof(KpiCardParameter), (p, m, t, v, v2) => // KPIカード: 厚いカード、大きな数字、上下の矢印と小さな棒グラフ
        {
            var ln = Str(p).Split('\n'); bool up = !(ln.ElementAtOrDefault(2) ?? "").Contains('-');
            m.Box(0, 0, 0, 600, 360, 40, m.C(0x1F2430), .15f);
            m.Text(ln.ElementAtOrDefault(0) ?? "", 40, 0, -120, 22, 4, m.C(0xFFFFFF, .7f));
            m.Text(ln.ElementAtOrDefault(1) ?? "", 110, -60, 10, 22, 10, W);
            Puff(m, 30, 16, up ? m.C(0x43A047) : m.C(0xE53935), 200, up ? -20 : 40, 250, up ? 40 : -20, 150, up ? 40 : -20);
            for (int i = 0; i < 6; i++) m.Box(-200 + i * 60, 140 - 15 * i * (up ? 1 : .3f) / 2, 22, 40, 15 * i * (up ? 1 : .3f) + 10, 10, m.C(p.Color1), .2f);
        });
    }
}
