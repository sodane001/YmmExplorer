using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 188番(ファンタジー・SF 14)・189番(画面効果 14)・190番(背景 14)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>画面に開いた穴(のぞき穴・双眼鏡など): 穴のまわりを厚い板でふさぐ</summary>
    static void HoleMask(Sculpt m, IReadOnlyList<Vector2> hole, Vector4 col, float z = 0, float d = 40)
    {
        int n = hole.Count; float far = MathF.Max(m.W, m.H) * 1.2f;
        for (int i = 0; i < n; i++)
        {
            var a = hole[i]; var b = hole[(i + 1) % n];
            var fa = Vector2.Normalize(a) * far; var fb = Vector2.Normalize(b) * far;
            m.Quad3D(new(a, z), new(b, z), new(fb, z), new(fa, z), col);
            m.Quad3D(new(a, z), new(b, z), new(b, z - d), new(a, z - d), Sculpt.Dark(col, .6f));
        }
    }

    static List<Vector2> Ellipse(float x, float y, float rx, float ry, int n = 48) =>
        [.. Enumerable.Range(0, n).Select(i => { float a = i * MathF.Tau / n; return new Vector2(x + MathF.Cos(a) * rx, y + MathF.Sin(a) * ry); })];

    static partial void RegisterH1c(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.1f, .1f, .12f, 1);
        // ---------------------------------------------------------------- 188 ファンタジー・SF
        Q(d, typeof(MagicBookParameter), (p, m, t, v, v2) => // 魔法の本: 開いた厚い本と反ったページ、立ちのぼる光の粒と文字
        {
            var br = m.C(p.Color1);
            foreach (int s in Sides)
            {
                m.Turn(0, 150, s * -.08f, () => m.Box(s * 190, 160, 0, 380, 30, 300, br, .1f));
                m.Turn(0, 140, s * -.15f, () => m.Box(s * 180, 135, 0, 350, 18, 280, m.C(0xF5ECD7), .3f));
            }
            for (int i = 0; i < 24; i++) { float f = ((m.Now * .4f + DrawKit.H(p.Seed, i)) % 1), x = (DrawKit.H(p.Seed, i, 1) - .5f) * 300 + MathF.Sin(m.Now + i) * 30, z = (DrawKit.H(p.Seed, i, 2) - .5f) * 200; m.Ball(x, 120 - f * 450, z, 6 + 6 * (1 - f), m.C(p.Color2, 1 - f)); }
            m.Ball(0, 60, 0, 140, 90, 120, m.C(p.Color2, .15f));
        });
        Q(d, typeof(RuneCircleParameter), (p, m, t, v, v2) => // ルーンの輪: 2重に回る光の輪と、輪に沿って立つ文字
        {
            string s = Str(p).Replace("\n", ""); if (s.Length == 0) return; float k = Ease(In(m, 0, .5f));
            m.Pitch(0, 0, 0, -.9f, () =>
            {
                for (int ring = 0; ring < 2; ring++)
                {
                    float R = (300 - ring * 90) * k, rot = m.Now * (ring == 0 ? .4f : -.6f);
                    m.Torus(0, 0, 0, R + 30, 3, m.C(p.Color1)); m.Torus(0, 0, 0, R - 30, 2, m.C(p.Color1, .7f));
                    int n = Math.Max(1, s.Length);
                    for (int i = 0; i < n * (2 - ring); i++) { float ag = rot + i * MathF.Tau / (n * (2 - ring)); char ch = s[i % n]; float x = MathF.Cos(ag) * R, y = MathF.Sin(ag) * R; m.Turn(x, y, ag + MathF.PI / 2, () => m.Text(ch.ToString(), 40, x, y, 0, 6, m.C(p.Color1))); }
                }
                Gem(m, Pts(StarPts(0, 0, 120 * k, 6, .58f, m.Now * .3f)), 0, 0, 0f, 2, 8, m.C(p.Color1, .8f), m.C(p.Color1, .8f));
            });
        });
        QS(d, typeof(DragonShadowParameter), (p, m, t, v, v2) => // ドラゴンの影: 羽ばたきながら横切る、厚みのある黒いドラゴン
        {
            float f = ((m.Now * .25f) % 1.3f) - .15f, x = -m.W / 2 - 400 + (m.W + 800) * f, y = -m.H * .15f + MathF.Sin(f * 6) * 40, flap = MathF.Sin(m.Now * 5) * .6f;
            var c = m.C(p.Color1, .9f);
            m.Ball(x, y, 0, 160, 50, 60, c); m.Tube([new(x - 150, y, 0), new(x - 300, y + 30, 0), new(x - 420, y - 10, 0)], 18, c);
            m.Tube([new(x + 150, y - 10, 0), new(x + 230, y - 40, 0)], 22, c); m.Ball(x + 250, y - 45, 0, 40, 25, 25, c);
            foreach (int s in Sides) m.Pitch(x, y, 0, s * (.3f + flap), () => m.Slab(0, 8, c, x - 60, y, x + 80, y, x + 20, y - 320, x - 60, y - 260, x - 160, y - 220));
        });
        Q(d, typeof(FairyTrailParameter), (p, m, t, v, v2) => // 妖精の軌跡: 8の字に飛ぶ光の玉と、うしろに残る光の粒
        {
            float sp = m.Now * 1.2f;
            Vector3 Pos(float x) => new(MathF.Sin(x) * 400, MathF.Sin(x * 2) * 160, MathF.Cos(x) * 200);
            for (int i = p.Count; i >= 0; i--) { var q = Pos(sp - i * .05f); float f = i / (float)Math.Max(1, p.Count); m.Ball(q.X, q.Y, q.Z, 12 * (1 - f) + 2, Vector4.Lerp(m.C(p.Color2), m.C(p.Color1), f) with { W = (1 - f) * m.Al }); }
            var h = Pos(sp); m.Ball(h.X, h.Y, h.Z, 40, m.C(p.Color1, .3f)); m.Ball(h.X, h.Y, h.Z, 16, W);
        });
        QS(d, typeof(GearWarpParameter), (p, m, t, v, v2) => // 歯車ワープ: 奥へ並んで回る、大きさの違う厚い歯車
        {
            if (p.Background) m.Box(0, 0, -1500, m.W * 3, m.H * 3, 10, m.C(p.Color2), .01f);
            for (int i = 0; i < 8; i++)
            {
                float z = -1200 + ((i * 160 + m.Now * 200) % 1300), r = 120 + 80 * DrawKit.H(p.Seed, i), x = (DrawKit.H(p.Seed, i, 1) - .5f) * m.W * .8f, y = (DrawKit.H(p.Seed, i, 2) - .5f) * m.H * .8f, rot = m.Now * (i % 2 == 0 ? 1 : -1) * .6f;
                m.Move(0, 0, z, () => m.Turn(x, y, rot, () => { for (int k = 0; k < 12; k++) m.Turn(x, y, k * MathF.Tau / 12, () => m.Box(x, y - r, 0, r * .25f, r * .3f, 40, m.C(p.Color1), .2f)); m.Disc(x, y, 0, r, r, 40, m.C(p.Color1), .2f); m.Disc(x, y, 0, r * .35f, r * .35f, 44, m.C(p.Color2), .2f); }));
            }
        });
        QS(d, typeof(SpaceStationParameter), (p, m, t, v, v2) => // 宇宙ステーション: 星空、回転する輪の居住区と太陽電池板
        {
            if (p.Background) { m.Box(0, 0, -1200, m.W * 3, m.H * 3, 10, m.C(0x03030D), .01f); int keep = m.Seg; m.Seg = 5; for (int i = 0; i < 80; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * m.W * 2, (DrawKit.H(p.Seed, i, 1) - .5f) * m.H * 2, -1100, 3, W); m.Seg = keep; }
            m.Pitch(0, 0, 0, -.5f, () => m.Turn(0, 0, m.Now * .2f, () =>
            {
                m.Torus(0, 0, 0, 260, 30, m.C(p.Color2));
                for (int i = 0; i < 4; i++) m.Turn(0, 0, i * MathF.PI / 2, () => m.Limb(0, 0, 0, 0, -240, 0, 8, 8, m.C(p.Color2)));
                m.Ball(0, 0, 0, 60, m.C(p.Color2));
            }));
            foreach (int s in Sides) { m.Limb(s * 60, 0, 0, s * 420, 0, 0, 6, 6, m.C(p.Color2)); m.Box(s * 420, 0, 0, 260, 120, 6, m.C(p.Color1), .05f); }
        });
        QS(d, typeof(MeteorStrikeParameter), (p, m, t, v, v2) => // 隕石: 燃える岩が奥から手前に飛んでくる、炎の尾
        {
            float k = In(m, 0, 1.6f); var st = new Vector3(m.W * .6f, -m.H * .7f, -1500); var en = new Vector3(0, 0, 0); var q = Vector3.Lerp(st, en, k * k);
            var dir = Vector3.Normalize(st - en);
            for (int i = 0; i < 12; i++) { var e = q + dir * (60 + i * 50); m.Ball(e.X, e.Y, e.Z, 70 - i * 4, Vector4.Lerp(m.C(p.Color2), m.C(p.Color1), i / 12f) with { W = (1 - i / 12f) * .8f * m.Al }); }
            m.Ball(q.X, q.Y, q.Z, 80, m.C(0x5D4037));
            if (k >= 1) { float f = MathF.Min(1, (m.Now - 1.6f) * 2); m.Ball(0, 0, 0, 300 * f, m.C(p.Color2, 1 - f)); }
        });
        Q(d, typeof(RingedPlanetParameter), (p, m, t, v, v2) => // 環のある惑星: 球と、傾いた平たい環(値で傾き)
        {
            float R = 260, tilt = .1f + .9f * v;
            m.Turn(0, 0, -.25f, () =>
            {
                m.Ball(0, 0, 0, R, m.C(p.Color1));
                m.Pitch(0, 0, 0, MathF.PI / 2 - tilt * .9f, () => m.Lathe(0, 0, m.C(p.Color2, .85f), R * 1.3f, -4, R * 2.1f, -4, R * 2.1f, 4, R * 1.3f, 4, R * 1.3f, -4));
            });
        });
        QS(d, typeof(RobotBootParameter), (p, m, t, v, v2) => // ロボット起動: 暗い画面、点灯する丸い目のレンズ、流れる起動ログ
        {
            var items = Lines(p);
            if (p.Background) m.Box(0, 0, -30, m.W, m.H, 10, m.C(0x020308), .01f);
            float on = In(m, .3f, .8f);
            foreach (int s in Sides) { m.Torus(s * 220, -60, 0, 110, 12, m.C(0x37474F)); m.Ball(s * 220, -60, 0, 90 * on, 90 * on, 30, m.C(p.Color1, on)); m.Ball(s * 220, -60, 40, 30 * on, m.C(0xFFFFFF, on)); }
            for (int i = 0; i < items.Count; i++) { float k = In(m, 1 + i * .3f, .2f); if (k > 0) m.Text("> " + items[i], 34, -m.W / 2 + 300, m.H / 2 - 300 + i * 50, 4, 3, m.C(p.Color1, k)); }
        });
        Q(d, typeof(WormholeParameter), (p, m, t, v, v2) => // ワームホール: 奥へ吸いこまれていく、光の輪のトンネル
        {
            float R = 600;
            for (int i = 0; i < 18; i++) { float f = ((i / 18f + m.Now * .3f) % 1 + 1) % 1, rr = R * MathF.Pow(f, 2.2f), z = -1200 * (1 - f); m.Torus(0, 0, z, rr + 10, 4 + 8 * f, Vector4.Lerp(m.C(p.Color1), m.C(p.Color2), f) with { W = f * m.Al }); }
            m.Ball(0, 0, -1200, 60, W);
        });
        Q(d, typeof(AtomModelParameter), (p, m, t, v, v2) => // 原子モデル: 中心の原子核(玉の集まり)と、傾いた軌道を回る電子
        {
            float R = 340; int n = Math.Clamp(p.Count, 1, 8);
            for (int i = 0; i < 7; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * 60, (DrawKit.H(p.Seed, i, 1) - .5f) * 60, (DrawKit.H(p.Seed, i, 2) - .5f) * 60, 30, i % 2 == 0 ? m.C(p.Color2) : m.C(0xB0BEC5));
            for (int i = 0; i < n; i++)
            {
                float tilt = i * MathF.PI / n;
                m.Turn(0, 0, tilt, () => m.Pitch(0, 0, 0, 1.2f, () =>
                {
                    m.Tube(ArcPts3(0, 0, R, 0, MathF.Tau, 0, 48), 2, m.C(p.Color1, .6f));
                    float ag = m.Now * (1.5f + i * .3f); m.Ball(MathF.Cos(ag) * R, MathF.Sin(ag) * R, 0, 18, m.C(p.Color1));
                }));
            }
        });
        Q(d, typeof(IsekaiGateParameter), (p, m, t, v, v2) => // 異世界の門: 石のアーチと、光の渦の面
        {
            float gw = 520, gh = 780;
            var arch = new List<Vector3> { new(-gw / 2, gh / 2, 0) };
            arch.AddRange(ArcPts3(0, -gh / 2 + gw / 2, gw / 2, MathF.PI, MathF.Tau, 0, 20)); arch.Add(new(gw / 2, gh / 2, 0));
            m.Tube(arch, 50, m.C(p.Color1));
            m.Box(0, gh / 2 + 40, 0, gw + 200, 80, 200, m.C(p.Color1), .2f);
            m.Box(0, 40, -10, gw - 60, gh - 80, 6, m.C(p.Color2, .7f), .2f);
            for (int i = 0; i < 6; i++) { float r = 60 + i * 40, ag = m.Now * (1 + i * .2f); m.Tube(ArcPts3(0, 0, r, ag, ag + 4, 0, 16), 4, m.C(p.Color3, .7f)); }
        });
        Q(d, typeof(StealthShimmerParameter), (p, m, t, v, v2) => // ステルス迷彩: ゆらめく透明な人型(ふちだけ光る)
        {
            float hw = 160, hh = 400, wob = MathF.Sin(m.Now * 6) * 6;
            var c = m.C(p.Color1, .15f);
            m.Ball(wob, -hh * .75f, 0, 70, c); m.Limb(0, -hh * .55f, 0, 0, hh * .1f, 0, hw * .55f, hw * .45f, c);
            foreach (int s in Sides) { m.Limb(s * 60, hh * .1f, 0, s * 60 + wob, hh, 0, 40, 30, c); m.Limb(s * 90, -hh * .5f, 0, s * 140, hh * .05f, 0, 26, 22, c); }
            m.Tube([.. Enumerable.Range(0, 33).Select(i => { float ag = i * MathF.Tau / 32; return new Vector3(MathF.Cos(ag) * hw * 1.1f + MathF.Sin(m.Now * 8 + i) * 8, MathF.Sin(ag) * hh, 30); })], 2, m.C(p.Color1, .5f));
        });
        QS(d, typeof(StarChartParameter), (p, m, t, v, v2) => // 星図: 夜空の板、同心の輪、星と、引かれていく航路
        {
            if (p.Background) m.Box(0, 0, -30, m.W, m.H, 10, m.C(0x050A1A), .01f);
            for (int i = 0; i < 6; i++) m.Torus(0, 0, -10, 100 + i * 120, 1.5f, m.C(p.Color1, .25f));
            var pts = Enumerable.Range(0, 6).Select(i => new Vector3((DrawKit.H(p.Seed, i) - .5f) * m.W * .8f, (DrawKit.H(p.Seed, i, 1) - .5f) * m.H * .7f, (DrawKit.H(p.Seed, i, 2) - .5f) * 300)).ToList();
            foreach (var q in pts) Gem(m, Pts(StarPts(q.X, q.Y, 24, 4, .35f, 0)), q.X, q.Y, 0f, 2, 10, m.C(p.Color2), m.C(p.Color2));
            float k = In(m, .3f, 2); int seg = (int)(k * (pts.Count - 1));
            if (seg >= 1) m.Tube([.. pts.Take(seg + 1)], 3, m.C(p.Color2, .8f));
        });

        // ---------------------------------------------------------------- 189 画面効果
        QS(d, typeof(FilmLightLeakParameter), (p, m, t, v, v2) => // 光漏れ: 画面のはしに、ふわっと浮かぶ大きな光の玉
        {
            var cols = new[] { p.Color1, p.Color2, p.Color3 };
            for (int i = 0; i < 4; i++) { float ph = m.Now * .3f + i * 1.7f, x = MathF.Sin(ph) * m.W * .45f, y = MathF.Cos(ph * .7f) * m.H * .4f; m.Ball(x, y, -100 + i * 60, m.H * (.3f + .1f * MathF.Sin(ph * 2)), m.C(cols[i % 3], .25f)); }
        });
        QS(d, typeof(IrisWipeParameter), (p, m, t, v, v2) => // アイリス: 丸い穴の開いた厚い黒い板(穴が開く・閉じる)
        {
            float k = In(m, 0, 2); if (p.Variant >= 2) k = 1 - k;
            float R = MathF.Max(2, MathF.Sqrt(m.W * m.W + m.H * m.H) / 2 * k);
            HoleMask(m, Ellipse(0, 0, R, R), m.C(p.Color1), 0, 40);
        });
        QS(d, typeof(PeepholeParameter), (p, m, t, v, v2) => // のぞき穴: 丸い穴の開いた厚い板と、穴のふちの金具
        {
            float R = MathF.Min(m.W, m.H) * (.2f + .35f * v);
            HoleMask(m, Ellipse(0, 0, R, R), m.C(p.Color1), 0, 60);
            m.Torus(0, 0, 10, R, 12, m.C(0x8A8070));
        });
        QS(d, typeof(BinocularsParameter), (p, m, t, v, v2) => // 双眼鏡: 2つの丸がつながった穴の開いた黒い板(少しゆれる)
        {
            float R = m.H * .38f, sx = MathF.Sin(m.Now * .9f) * 10, sy = MathF.Cos(m.Now * 1.1f) * 8;
            var hole = new List<Vector2>();
            for (int i = 0; i < 64; i++) { float ag = i * MathF.Tau / 64, x = MathF.Cos(ag), y = MathF.Sin(ag); float cx = x >= 0 ? R * .7f : -R * .7f; hole.Add(new(sx + cx + x * R, sy + y * R * .95f)); }
            HoleMask(m, hole, m.C(p.Color1), 0, 50);
        });
        QS(d, typeof(SecurityCamParameter), (p, m, t, v, v2) => // 監視カメラ: 走査線、四隅のかぎ、点滅する REC
        {
            for (int i = 0; i < 30; i++) { float y = -m.H / 2 + ((i * m.H / 30 + m.Now * 40) % m.H); m.Box(0, y, -20, m.W, 2, 2, m.C(0xFFFFFF, .06f), .5f); }
            foreach (int x in Sides) foreach (int y in Sides) { m.Limb(x * m.W * .42f, y * m.H * .4f, 0, x * (m.W * .42f - 80), y * m.H * .4f, 0, 3, 3, m.C(p.Color1)); m.Limb(x * m.W * .42f, y * m.H * .4f, 0, x * m.W * .42f, y * (m.H * .4f - 80), 0, 3, 3, m.C(p.Color1)); }
            if ((m.Now % 1) < .6f) m.Ball(-m.W * .38f, -m.H * .33f, 0, 14, m.C(p.Color2));
            m.Text("REC", 40, -m.W * .33f, -m.H * .33f, 0, 4, m.C(p.Color1));
            m.Text(Str(p), 34, m.W * .3f, m.H * .35f, 0, 4, m.C(p.Color1));
        });
        QS(d, typeof(NightVisionParameter), (p, m, t, v, v2) => // 暗視ゴーグル: 緑の画面、ちらつくノイズの粒、丸いふち
        {
            m.Box(0, 0, -30, m.W, m.H, 4, m.C(p.Color1, .25f), .01f);
            int fr = (int)(m.Now * 24); int keep = m.Seg; m.Seg = 5;
            for (int i = 0; i < p.Count; i++) m.Ball((DrawKit.H(fr, i) - .5f) * m.W, (DrawKit.H(fr, i, 1) - .5f) * m.H, -10, 2, m.C(p.Color1, .6f));
            m.Seg = keep;
            HoleMask(m, Ellipse(0, 0, m.W * .45f, m.H * .48f), m.C(0x000000), 0, 30);
        });
        QS(d, typeof(ThermalLookParameter), (p, m, t, v, v2) => // サーモグラフィー: 濃い紺の画面と、色の山が盛り上がる熱のかたまり
        {
            m.Box(0, 0, -40, m.W, m.H, 10, m.C(0x0D0059, .9f), .01f);
            for (int i = 0; i < p.Count; i++)
            {
                float x = (DrawKit.H(p.Seed, i) - .5f) * m.W * .6f + MathF.Sin(m.Now + i) * 30, y = (DrawKit.H(p.Seed, i, 1) - .5f) * m.H * .6f, r = 120 + 100 * DrawKit.H(p.Seed, i, 2);
                uint[] band = [0x4A00A0, 0xE0006A, 0xFF8000, 0xFFE000, 0xFFFFFF];
                for (int k = 0; k < 5; k++) m.Ball(x, y, -30 + k * 20, r * (1 - k * .18f), r * (1 - k * .18f), 10 + k * 15, m.C(band[k], .8f));
            }
        });
        QS(d, typeof(DreamEdgeParameter), (p, m, t, v, v2) => // 夢のふち: もこもこの雲のようなふちが画面を囲む
        {
            float mg = MathF.Min(m.W, m.H) * (.08f + .25f * v);
            for (int i = 0; i < 60; i++)
            {
                float f = i / 60f, per = 2 * (m.W + m.H); float d0 = f * per; Vector2 q;
                if (d0 < m.W) q = new(-m.W / 2 + d0, -m.H / 2); else if (d0 < m.W + m.H) q = new(m.W / 2, -m.H / 2 + d0 - m.W); else if (d0 < 2 * m.W + m.H) q = new(m.W / 2 - (d0 - m.W - m.H), m.H / 2); else q = new(-m.W / 2, m.H / 2 - (d0 - 2 * m.W - m.H));
                float r = mg * (.8f + .4f * MathF.Sin(i * 1.3f + m.Now));
                m.Ball(q.X, q.Y, 0, r, r, r * .5f, m.C(p.Color1, .85f));
            }
        });
        QS(d, typeof(WetLensParameter), (p, m, t, v, v2) => // ぬれたレンズ: 画面に付いた、ふくらんだ透明の水滴
        {
            for (int i = 0; i < p.Count; i++) { float r = 6 + 26 * DrawKit.H(p.Seed, i, 2), x = (DrawKit.H(p.Seed, i) - .5f) * m.W, y = (DrawKit.H(p.Seed, i, 1) - .5f) * m.H + ((m.Now * 20 * DrawKit.H(p.Seed, i, 3)) % 200); m.Ball(x, y, 0, r, r * 1.1f, r * .6f, m.C(p.Color1, .35f)); m.Ball(x - r * .3f, y - r * .4f, r * .5f, r * .25f, m.C(0xFFFFFF, .7f)); }
        });
        QS(d, typeof(HalftoneParameter), (p, m, t, v, v2) => // ハーフトーン: 斜めに並ぶ、大きさの変わる半球の網点
        {
            float cell = 14 + 40 * v, ang = .4f, ca = MathF.Cos(ang), sa = MathF.Sin(ang);
            int nx = (int)(m.W / cell) + 4, ny = (int)(m.H / cell) + 4; int keep = m.Seg; m.Seg = 8;
            for (int y = -ny / 2; y <= ny / 2; y++) for (int x = -nx / 2; x <= nx / 2; x++)
                {
                    float px = x * cell, py = y * cell, rx = px * ca - py * sa, ry = px * sa + py * ca;
                    if (MathF.Abs(rx) > m.W / 2 || MathF.Abs(ry) > m.H / 2) continue;
                    float f = .5f + .5f * MathF.Sin(rx * .004f + ry * .003f + m.Now), r = cell * .45f * f;
                    if (r > 1) m.Ball(rx, ry, 0, r, r, r * .6f, m.C(p.Color1));
                }
            m.Seg = keep;
        });
        QS(d, typeof(MangaPanelsParameter), (p, m, t, v, v2) => // 漫画のコマ割り: 白い厚いコマの枠と、太い黒い線
        {
            float mg = 30, g = 26, lw = 8, x0 = -m.W / 2 + mg, y0 = -m.H / 2 + mg, ww = m.W - mg * 2, hh = m.H - mg * 2;
            m.Box(0, 0, -30, m.W, m.H, 10, m.C(p.Color2), .01f);
            var cells = new List<(float x, float y, float w, float h)> { (x0, y0, ww * .6f - g / 2, hh * .5f - g / 2), (x0 + ww * .6f + g / 2, y0, ww * .4f - g / 2, hh * .5f - g / 2), (x0, y0 + hh * .5f + g / 2, ww * .35f - g / 2, hh * .5f - g / 2), (x0 + ww * .35f + g / 2, y0 + hh * .5f + g / 2, ww * .65f - g / 2, hh * .5f - g / 2) };
            foreach (var (x, y, w0, h0) in cells) { m.Box(x + w0 / 2, y + h0 / 2, -10, w0, h0, 10, m.C(0xFFFFFF, .05f), .01f); m.Tube([new(x, y, 0), new(x + w0, y, 0), new(x + w0, y + h0, 0), new(x, y + h0, 0), new(x, y, 0)], lw / 2, m.C(p.Color1)); }
        });
        QS(d, typeof(StageCurtainParameter), (p, m, t, v, v2) => // 舞台の幕: 波打つひだの厚い幕が左右に開く、上の飾り幕
        {
            float k = Ease(In(m, .2f, 1.5f)); if (p.Variant >= 2) k = 1 - k;
            float cw = m.W / 2 * (1 - k * .85f);
            foreach (int s in Sides)
            {
                for (int i = 0; i < 10; i++) { float x = s * (m.W / 2 - cw * (i + .5f) / 10); m.Cyl(x, 0, 30 * MathF.Sin(i * 1.7f), cw / 10 * .7f, m.H * 1.02f, m.C(p.Color1), .05f); }
            }
            m.Box(0, -m.H / 2 + 60, 60, m.W, 140, 80, m.C(p.Color1), .3f);
            m.Box(0, -m.H / 2 + 135, 100, m.W, 14, 20, m.C(p.Color2), .5f);
        });
        Q(d, typeof(PolaroidFrameParameter), (p, m, t, v, v2) => // ポラロイド: 少し傾いた厚い白い枠、下の余白に文字
        {
            float k = Ease(In(m, 0, .4f)), pw = 700 * k, ph = 820 * k, side = 36 * k, bottom = 160 * k;
            m.Turn(0, 0, -.06f, () =>
            {
                m.Box(0, 0, 0, pw, ph, 14, m.C(p.Color1), .02f);
                m.Box(0, -(bottom - side) / 2, 8, pw - side * 2, ph - side - bottom, 4, m.C(0x2A2A30), .02f);
                m.Text(Str(p), 50 * k, 0, ph / 2 - bottom / 2, 9, 3, m.C(0x333338), pw - 80);
            });
        });

        // ---------------------------------------------------------------- 190 背景
        QS(d, typeof(ScreenToneParameter), (p, m, t, v, v2) => // スクリーントーン: 並んだ小さな半球の点(または斜線の畝)
        {
            float cell = 8 + 20 * v;
            if (p.Variant == 2) { for (float x = -m.W / 2 - m.H; x < m.W / 2 + m.H; x += cell) { Vector2 a = new(x, -m.H / 2), b = new(x + m.H, m.H / 2); if (ClipSeg(ref a, ref b, -m.W / 2, -m.H / 2, m.W / 2, m.H / 2)) Ridge(m, a, b, cell * .3f, 3, m.C(p.Color1, .5f)); } return; }
            int keep = m.Seg; m.Seg = 5;
            for (float y = -m.H / 2; y < m.H / 2; y += cell) for (float x = -m.W / 2; x < m.W / 2; x += cell) m.Ball(x, y, 0, cell * .22f, cell * .22f, 2, m.C(p.Color1, .5f));
            m.Seg = keep;
        });
        QS(d, typeof(CheckStripeParameter), (p, m, t, v, v2) => Pattern3D(m, t, m.C(p.Color2), m.C(p.Color1), p.Variant >= 2 ? 1 : 2));
        QS(d, typeof(PopDotsParameter), (p, m, t, v, v2) => // 水玉ポップ: 地の板と、流れる大小の半球の玉
        {
            if (p.Background) m.Box(0, 0, -20, m.W, m.H, 40, m.C(p.Color1), .01f);
            for (int i = 0; i < p.Count; i++) { float r = 20 + 60 * DrawKit.H(p.Seed, i, 2), x = (((DrawKit.H(p.Seed, i) * m.W + m.Now * 30) % m.W) + m.W) % m.W - m.W / 2, y = (DrawKit.H(p.Seed, i, 1) - .5f) * m.H; m.Ball(x, y, 0, r, r, r * .6f, m.C(p.Color2, .85f)); }
        });
        QS(d, typeof(SunburstParameter), (p, m, t, v, v2) => // サンバースト: 中心から広がる、盛り上がった放射の板(回る)
        {
            if (p.Background) m.Box(0, 0, -20, m.W, m.H, 10, m.C(p.Color2), .01f);
            int n = Math.Max(4, p.Count); float R = MathF.Max(m.W, m.H) * 1.2f, rot = m.Now * .2f;
            for (int i = 0; i < n; i += 2) { float a0 = rot + i * MathF.Tau / n, a1 = rot + (i + 1) * MathF.Tau / n; m.Tri3D(new(0, 0, 40), new(MathF.Cos(a0) * R, MathF.Sin(a0) * R, -10), new(MathF.Cos(a1) * R, MathF.Sin(a1) * R, -10), m.C(p.Color1)); }
        });
        QS(d, typeof(WavyGradientParameter), (p, m, t, v, v2) => // うねるグラデーション: 前後に重なってうねる色の波の板
        {
            m.Box(0, 0, -300, m.W * 1.5f, m.H * 1.5f, 10, m.C(p.Color1), .01f);
            var cols = new[] { p.Color2, p.Color3, p.Color1 };
            for (int i = 0; i < 5; i++)
            {
                float ph = m.Now * .15f + i * 1.3f, by = -m.H / 2 + m.H * (.2f + i * .17f);
                var pts = new List<Vector2> { new(m.W, m.H) }; for (int j = 30; j >= 0; j--) { float x = -m.W / 2 + m.W * j / 30f; pts.Add(new(x, by + MathF.Sin(x * .004f + ph) * 80)); } pts.Add(new(-m.W, m.H));
                m.Slab(pts, -250 + i * 50, 20, m.C(cols[i % 3], .55f));
            }
        });
        QS(d, typeof(WireframeFloorParameter), (p, m, t, v, v2) => // ワイヤーフレームの床: 夕日と、奥へ流れる本物の格子の床
        {
            if (p.Background) SkyWall(m, m.C(p.Color2), m.C(0x5A0F6B), -2000);
            m.Ball(0, -60, -1900, 300, m.C(p.Color3));
            float y = m.H * .25f, off = (m.Now * 200) % 200;
            for (int i = -10; i <= 10; i++) m.Limb(i * 200, y, 400, i * 200, y, -2000, 2, 2, m.C(p.Color1));
            for (int i = 0; i < 13; i++) { float z = 400 - i * 200 + off; m.Limb(-2000, y, z, 2000, y, z, 2, 2, m.C(p.Color1)); }
        });
        QS(d, typeof(GridTunnelParameter), (p, m, t, v, v2) => // 格子のトンネル: 奥へ続く四角い枠と、4すみの線
        {
            if (p.Background) m.Box(0, 0, -2500, m.W * 3, m.H * 3, 10, m.C(p.Color2), .01f);
            float cx = MathF.Sin(m.Now * .5f) * 60, cy = MathF.Cos(m.Now * .4f) * 40;
            for (int i = 0; i < 14; i++) { float z = -((i * 180 + m.Now * 300) % 2500); m.Tube([new(-m.W / 2 + cx, -m.H / 2 + cy, z), new(m.W / 2 + cx, -m.H / 2 + cy, z), new(m.W / 2 + cx, m.H / 2 + cy, z), new(-m.W / 2 + cx, m.H / 2 + cy, z), new(-m.W / 2 + cx, -m.H / 2 + cy, z)], 2, m.C(p.Color1, .9f)); }
            foreach (int x in Sides) foreach (int y in Sides) m.Limb(x * m.W / 2 + cx, y * m.H / 2 + cy, 0, x * m.W / 2 + cx, y * m.H / 2 + cy, -2500, 2, 2, m.C(p.Color1));
        });
        QS(d, typeof(BookshelfParameter), (p, m, t, v, v2) => // 本棚: 段の板と、奥行きのある色とりどりの本
        {
            int rows = Math.Clamp(p.Count, 1, 8); float sh = m.H / rows;
            m.Box(0, 0, -150, m.W, m.H, 20, Sculpt.Dark(m.C(p.Color1), .7f), .01f);
            for (int r = 0; r < rows; r++)
            {
                float y1 = -m.H / 2 + (r + 1) * sh; m.Box(0, y1 - 10, -60, m.W, 20, 200, m.C(p.Color1), .05f);
                float x = -m.W / 2 + 20; int i = 0;
                while (x < m.W / 2 - 20) { float bw = 30 + 30 * DrawKit.H(p.Seed, r * 50 + i), bh = sh * (.6f + .3f * DrawKit.H(p.Seed, r * 50 + i, 1)); m.Box(x + bw / 2, y1 - 20 - bh / 2, -70, bw - 4, bh, 160, Pal(m, r * 7 + i), .05f); x += bw; i++; }
            }
        });
        QS(d, typeof(CafeWindowParameter), (p, m, t, v, v2) => // カフェの窓辺: 壁、窓枠と外の景色、伝う雨、手前のカップ
        {
            bool rain = p.Variant < 2;
            m.Box(0, 0, -60, m.W, m.H, 20, m.C(0xE8D9C4), .01f);
            float wx = m.W * .35f, wy = m.H * .3f;
            m.Box(0, -m.H * .1f, -55, wx * 2, wy * 2, 4, m.C(p.Color2), .01f);
            m.Tube([new(-wx, -m.H * .1f - wy, -40), new(wx, -m.H * .1f - wy, -40), new(wx, -m.H * .1f + wy, -40), new(-wx, -m.H * .1f + wy, -40), new(-wx, -m.H * .1f - wy, -40)], 16, m.C(p.Color1));
            m.Box(0, -m.H * .1f, -40, 14, wy * 2, 30, m.C(p.Color1), .2f);
            if (rain) for (int i = 0; i < 20; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * wx * 2, y = -m.H * .1f - wy + ((m.Now * 80 * (1 + DrawKit.H(p.Seed, i, 1)) + DrawKit.H(p.Seed, i, 2) * wy * 2) % (wy * 2)); m.Ball(x, y, -30, 5, 7, 4, m.C(0xFFFFFF, .5f)); }
            m.Box(0, m.H * .35f, 0, m.W, 30, 300, m.C(0x8D6E63), .1f);
            m.Lathe(m.W * .25f, 60, m.C(0xFFFFFF), 0, m.H * .34f, 70, m.H * .34f, 90, m.H * .2f, 0, m.H * .2f);
            m.Torus(m.W * .25f + 100, m.H * .27f, 60, 30, 8, m.C(0xFFFFFF));
            for (int i = 0; i < 3; i++) { float f = ((m.Now * .3f + i / 3f) % 1); m.Ball(m.W * .25f + MathF.Sin(f * 6) * 15, m.H * .18f - f * 150, 60, 10 + 20 * f, m.C(0xFFFFFF, .4f * (1 - f))); }
        });
        QS(d, typeof(TrainPlatformParameter), (p, m, t, v, v2) => // 駅のホーム: 空、奥へ続くホームと黄色い線、屋根の柱
        {
            SkyWall(m, m.C(0xA7D8F5), m.C(0xE3F2FD), -2500);
            float y = m.H * .2f;
            m.Box(-m.W * .3f, y + 30, -1000, m.W * .8f, 60, 2600, m.C(0x9E9E9E), .02f);
            m.Box(m.W * .1f - 20, y, -1000, 20, 4, 2600, m.C(p.Color2), .3f);
            for (int i = 0; i < 6; i++) { float z = 200 - i * 450; m.Cyl(-m.W * .4f, y - 300, z, 14, 600, m.C(p.Color1), .2f); }
            m.Box(-m.W * .3f, y - 600, -1000, m.W * .8f, 20, 2600, m.C(p.Color1), .05f);
        });
        QS(d, typeof(TrainWindowParameter), (p, m, t, v, v2) => // 車窓: 窓のくりぬかれた車内の壁と、外を流れる山や木や電柱
        {
            SkyWall(m, m.C(0x81D4FA), m.C(0xE1F5FE), -1500);
            for (int i = 0; i < 8; i++) { float x = ((i * 500 - m.Now * 200) % 4000 + 4000) % 4000 - 2000; m.Lathe(x, -1200, m.C(0x7E9F6A), 0, -300, 400, 300, 0, 300); }
            for (int i = 0; i < 12; i++) { float x = ((i * 300 - m.Now * 1200) % 3600 + 3600) % 3600 - 1800; m.Ball(x, 150, -500, 80, m.C(p.Color2)); m.Cyl(x + 150, 0, -300, 6, 600, m.C(0x6D4C41), .2f); }
            HoleMask(m, [new(-m.W * .4f, -m.H * .3f), new(m.W * .4f, -m.H * .3f), new(m.W * .4f, m.H * .22f), new(-m.W * .4f, m.H * .22f)], m.C(p.Color1), 0, 60);
        });
        QS(d, typeof(AquariumTankParameter), (p, m, t, v, v2) => // 水族館: 青い水の奥行き、ゆれる光の筋、泳ぐ魚と上る泡
        {
            SkyWall(m, m.C(p.Color1), m.C(p.Color2), -900);
            for (int i = 0; i < 6; i++) { float x = -m.W / 2 + m.W * (i + .5f) / 6 + MathF.Sin(m.Now * .5f + i) * 60; m.Box(x, -m.H / 4, -600, 40, m.H, 4, m.C(0xFFFFFF, .08f), .5f); }
            for (int i = 0; i < 9; i++)
            {
                float dir = i % 2 == 0 ? 1 : -1, x = dir * ((((DrawKit.H(p.Seed, i) * m.W + m.Now * 80 * (1 + i % 3)) % (m.W + 400)) - m.W / 2 - 200)), y = (DrawKit.H(p.Seed, i, 1) - .5f) * m.H * .7f, z = -700 + DrawKit.H(p.Seed, i, 2) * 600;
                var col = Pal(m, i);
                m.Ball(x, y, z, 50, 26, 18, col); m.Tri3D(new(x - dir * 45, y, z), new(x - dir * 80, y - 25, z), new(x - dir * 80, y + 25, z), col); m.Ball(x + dir * 30, y - 6, z + 15, 5, m.C(0x000000));
            }
            for (int i = 0; i < 14; i++) { float f = ((m.Now * .3f + DrawKit.H(p.Seed, i + 30)) % 1); m.Ball((DrawKit.H(p.Seed, i + 40) - .5f) * m.W, m.H / 2 - f * m.H, -200, 6 + 6 * f, m.C(0xFFFFFF, .5f)); }
        });
        QS(d, typeof(ConcertPenlightsParameter), (p, m, t, v, v2) => // ペンライト: 暗い会場、奥まで並んでゆれる光る棒
        {
            if (p.Background) SkyWall(m, m.C(0x050210), m.C(0x140826), -1500);
            var cols = new[] { p.Color1, p.Color2, p.Color3 };
            for (int r = 0; r < 6; r++) for (int i = 0; i < 14; i++)
                {
                    float x = -m.W / 2 + m.W * (i + .5f) / 14 + (r % 2) * 40, z = -r * 200, y = m.H * .35f, sw = MathF.Sin(m.Now * 4 + i * .3f + r) * .5f;
                    var col = m.C(cols[(i + r) % 3]);
                    m.Turn(x, y, sw, () => { m.Limb(x, y, z, x, y - 120, z, 6, 6, col); m.Ball(x, y - 120, z, 20, col with { W = .3f * m.Al }); });
                }
        });
        QS(d, typeof(ShrineTempleParameter), (p, m, t, v, v2) => // 神社・お寺: 空、反った屋根の本殿、柱、石段、(夜は灯籠)
        {
            bool night = p.Variant >= 2;
            SkyWall(m, night ? m.C(0x0D0F26) : m.C(0x9FD8FF), night ? m.C(0x332640) : m.C(0xE8F6FF), -1500);
            float y = m.H * .15f;
            m.Box(0, y + 80, -400, 1200, 160, 600, m.C(0x9E9E9E), .02f);
            for (int i = -2; i <= 2; i++) m.Cyl(i * 220, y - 150, -300, 22, 300, m.C(p.Color2), .1f);
            m.Box(0, y - 30, -400, 1000, 260, 300, m.C(0xF5ECD7), .02f);
            m.Slab(0, 500, m.C(p.Color1), -700, y - 280, 700, y - 280, 600, y - 330, 0, y - 480, -600, y - 330);
            for (int i = 0; i < 5; i++) m.Box(0, y + 180 + i * 30, -60 + i * 60, 400, 30, 60, m.C(0xB0B0B0), .05f);
            if (night) foreach (int s in Sides) { m.Box(s * 500, y + 100, 0, 60, 160, 60, m.C(0x8A8070), .1f); m.Ball(s * 500, y + 40, 0, 24, m.C(0xFFD27A)); }
        });
        QS(d, typeof(LaboratoryParameter), (p, m, t, v, v2) => // 研究室: 壁と床、実験台、ガラスのフラスコと泡立つ色の液
        {
            m.Box(0, -m.H * .15f, -400, m.W * 1.2f, m.H * .7f, 20, m.C(p.Color1), .01f);
            m.Box(0, m.H * .35f, 0, m.W * 1.2f, m.H * .3f, 800, m.C(0x80868F), .01f);
            m.Box(0, m.H * .18f, -100, m.W * .7f, 40, 300, m.C(0x4F5A62), .05f);
            for (int i = 0; i < 3; i++)
            {
                float x = -300 + i * 300, yb = m.H * .16f;
                m.Lathe(x, -100, m.C(0xFFFFFF, .3f), 0, yb, 70, yb, 70, yb - 40, 20, yb - 140, 20, yb - 200);
                m.Lathe(x, -100, m.C(p.Color2, .8f), 0, yb - 2, 66, yb - 2, 60, yb - 40, 0, yb - 40);
                for (int k = 0; k < 3; k++) { float f = ((m.Now * .8f + k / 3f + i * .2f) % 1); m.Ball(x, yb - 40 - f * 200, -100, 6 + 6 * f, m.C(p.Color2, .6f * (1 - f))); }
            }
        });
    }
}
