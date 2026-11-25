using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 204番(自然3 20)・205番(和の行事・小物 20)・206番(ファンタジー・SF3 20)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>地面(奥へ広がる床の板)</summary>
    static void Ground(Sculpt m, float y, Vector4 col, float z0 = 400, float z1 = -2000) =>
        m.Quad3D(new(-m.W * 1.5f, y, z1), new(m.W * 1.5f, y, z1), new(m.W * 1.5f, y, z0), new(-m.W * 1.5f, y, z0), col);

    static partial void RegisterJ1c(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.12f, .13f, .16f, 1);
        // ---------------------------------------------------------------- 204 自然3
        QS(d, typeof(HailstormParameter), (p, m, t, v, v2) => // ひょう: 奥行きにも降る白い氷の粒と、地面ではねる粒
        {
            for (int i = 0; i < Math.Max(30, p.Count); i++) { float f = ((m.Now * 1.4f + DrawKit.H(p.Seed, i)) % 1), x = (DrawKit.H(p.Seed, i, 1) - .5f) * m.W, z = (DrawKit.H(p.Seed, i, 2) - .5f) * 900; m.Ball(x, -m.H / 2 + f * m.H, z, 8 + 8 * DrawKit.H(p.Seed, i, 3), m.C(p.Color1, .9f)); }
            Ground(m, m.H / 2, m.C(0x607080, .5f));
        });
        QS(d, typeof(DustHazeParameter), (p, m, t, v, v2) => // かすむ空: 奥ほど濃い黄色のかすみの層と、ただよう粒
        {
            for (int i = 0; i < 6; i++) m.Box(0, 0, -300 - i * 300, m.W * 2, m.H * 2, 4, m.C(p.Color1, .12f + i * .04f), .01f);
            for (int i = 0; i < 60; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * m.W + MathF.Sin(m.Now + i) * 30, (DrawKit.H(p.Seed, i, 1) - .5f) * m.H, -DrawKit.H(p.Seed, i, 2) * 1500, 4, m.C(p.Color1, .6f));
        });
        Q(d, typeof(TyphoonSatelliteParameter), (p, m, t, v, v2) => // 台風: 回る渦の雲の管と、まん中の目
        {
            for (int k = 0; k < 4; k++) m.Turn(0, 0, -m.Now * .5f + k * MathF.PI / 2, () => m.Tube([.. Enumerable.Range(0, 30).Select(i => { float f = i / 29f, ag = f * 4; return new Vector3(MathF.Cos(ag) * (60 + 380 * f), MathF.Sin(ag) * (60 + 380 * f), 20 * (1 - f)); })], 30, m.C(p.Color1, .85f)));
            m.Disc(0, 0, 0, 50, 50, 10, m.C(0x1E3A5F), .5f);
        });
        Q(d, typeof(MorningDewParameter), (p, m, t, v, v2) => // 朝露: 反った葉の上の、光る丸いしずく(ひとつずつ落ちる)
        {
            m.Turn(0, 0, -.3f, () => m.Pitch(0, 0, 0, .3f, () => m.Ball(0, 0, 0, 500, 130, 20, m.C(0x5E9F4A))));
            for (int i = 0; i < 6; i++) { float x = -300 + i * 120, y = -20 + i * -30; m.Ball(x, y, 40, 24, 22, 22, m.C(p.Color1, .6f)); m.Ball(x - 8, y - 8, 58, 6, W); }
            float f = (m.Now * .5f) % 1; m.Ball(320, -170 + f * 400, 40, 16, 20, 16, m.C(p.Color1, .7f * (1 - f)));
        });
        QS(d, typeof(MossForestFloorParameter), (p, m, t, v, v2) => // 苔の地面: でこぼこの苔のかたまり、落ち葉、木の根、差しこむ光
        {
            Ground(m, m.H * .3f, m.C(0x2E4A20));
            for (int i = 0; i < 40; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * m.W * 1.5f, m.H * .3f, -DrawKit.H(p.Seed, i, 1) * 1500 + 300, 60 + 60 * DrawKit.H(p.Seed, i, 2), 30, 60, Vector4.Lerp(m.C(p.Color1), m.C(0x6B8E3B), DrawKit.H(p.Seed, i, 3)));
            for (int i = 0; i < 4; i++) m.Box(-m.W * .3f + i * 200, 0, -800, 40, m.H * 1.5f, 4, m.C(0xFFF3B0, .12f), .5f);
        });
        QS(d, typeof(MushroomForestParameter), (p, m, t, v, v2) => // キノコの森: 奥まで並ぶ、大小の立体のキノコ
        {
            Ground(m, m.H * .35f, m.C(0x3A2A20));
            for (int i = 0; i < 18; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W * 1.4f, z = 300 - DrawKit.H(p.Seed, i, 1) * 1600, s = .6f + .8f * DrawKit.H(p.Seed, i, 2), y = m.H * .35f; m.Cyl(x, y - 80 * s, z, 25 * s, 160 * s, m.C(0xF5ECD7), .3f); m.Lathe(x, z, Pal(m, i), [.. Dome(110 * s, 90 * s, y - 150 * s, 10), new Vector2(0, y - 150 * s)]); }
        });
        QS(d, typeof(JellyfishDriftParameter), (p, m, t, v, v2) => // クラゲ: 深い海に浮かぶ、透けたかさと揺れる足
        {
            SkyWall(m, m.C(0x0A2A50), m.C(0x02101E), -1500);
            for (int i = 0; i < 8; i++)
            {
                float x = (DrawKit.H(p.Seed, i) - .5f) * m.W, y = (DrawKit.H(p.Seed, i, 1) - .5f) * m.H + MathF.Sin(m.Now * .5f + i) * 60, z = -1200 + DrawKit.H(p.Seed, i, 2) * 1100, pu = 1 + .1f * MathF.Sin(m.Now * 2 + i);
                var c = m.C(p.Color1, .5f);
                m.Ball(x, y, z, 80 * pu, 60, 80 * pu, c);
                for (int k = 0; k < 5; k++) m.Tube([.. Enumerable.Range(0, 8).Select(j => new Vector3(x + (k - 2) * 25 + MathF.Sin(m.Now * 2 + j * .6f + k) * 10, y + 40 + j * 25, z))], 2, c);
            }
        });
        Q(d, typeof(GoldfishBowlParameter), (p, m, t, v, v2) => // 金魚鉢: 透けたガラスの鉢と水、泳ぐ金魚と水草
        {
            var prof = new List<Vector2> { new(0, 250) }; for (int i = 1; i <= 12; i++) { float th = i / 12f * 2.6f; prof.Add(new(MathF.Sin(th) * 280, 0 + MathF.Cos(th) * 250)); } prof.Add(new(0, prof[^1].Y));
            m.Lathe(0, 0, m.C(0xFFFFFF, .25f), prof);
            m.Lathe(0, 0, m.C(0x81D4FA, .25f), 0, 240, 250, 100, 270, -60, 0, -60);
            for (int i = 0; i < 2; i++) { float ag = m.Now * (.8f + i * .3f) + i * 3, x = MathF.Cos(ag) * 150, z = MathF.Sin(ag) * 150, y = 60 + i * 60; m.Ball(x, y, z, 45, 28, 22, m.C(p.Color1)); m.Tri3D(new(x - 40, y, z), new(x - 90, y - 40, z), new(x - 90, y + 40, z), m.C(p.Color1, .8f)); }
            for (int k = 0; k < 3; k++) m.Tube([.. Enumerable.Range(0, 10).Select(j => new Vector3(-150 + k * 40 + MathF.Sin(m.Now + j * .5f) * 10, 240 - j * 25, -60))], 4, m.C(0x4E8F4A));
        });
        QS(d, typeof(WhaleSilhouetteParameter), (p, m, t, v, v2) => // クジラ: 深い海の奥を、ゆっくり泳ぐ大きな体とひれ・尾
        {
            SkyWall(m, m.C(0x0A3A6A), m.C(0x02101E), -2000);
            float x = ((m.Now * 60) % (m.W * 2)) - m.W, y = 0, z = -800, sw = MathF.Sin(m.Now) * .2f;
            var c = m.C(p.Color1, .85f);
            m.Ball(x, y, z, 380, 120, 140, c);
            m.Turn(x - 380, y, sw, () => { m.Limb(x - 380, y, z, x - 520, y, z, 50, 20, c); m.Tri3D(new(x - 520, y, z), new(x - 620, y - 90, z), new(x - 620, y + 90, z), c); });
            foreach (int s in Sides) m.Pitch(x + 100, y + 60, z, s * .6f, () => m.Ball(x + 100, y + 90, z + s * 100, 90, 20, 30, c));
        });
        QS(d, typeof(OwlNightForestParameter), (p, m, t, v, v2) => // 夜の森のフクロウ: 奥へ並ぶ木の幹と、まばたきする光る目
        {
            SkyWall(m, m.C(0x060A18), m.C(0x101828), -1800);
            for (int i = 0; i < 12; i++) m.Cyl((DrawKit.H(p.Seed, i) - .5f) * m.W * 1.4f, 0, -DrawKit.H(p.Seed, i, 1) * 1600, 40 + 40 * DrawKit.H(p.Seed, i, 2), m.H * 1.2f, m.C(0x15100C), .05f);
            float blink = (m.Now % 3) < .15f ? .1f : 1;
            for (int k = 0; k < 3; k++) { float x = (k - 1) * 400, y = -100 + k * 80, z = -400 - k * 300; foreach (int s in Sides) m.Ball(x + s * 30, y, z, 18, 18 * blink, 10, m.C(p.Color1)); }
        });
        QS(d, typeof(AntTrailParameter), (p, m, t, v, v2) => // 蟻の行列: 地面を奥から手前へ、くねって進む立体の蟻
        {
            Ground(m, m.H * .3f, m.C(0x8D6E63));
            for (int i = 0; i < Math.Max(12, p.Count); i++) { float f = ((m.Now * .1f + i / 20f) % 1), z = -1200 + 1400 * f, x = MathF.Sin(f * 8) * 200, y = m.H * .3f - 12; var c = m.C(p.Color1); m.Ball(x, y, z - 20, 12, c); m.Ball(x, y, z, 9, c); m.Ball(x, y, z + 22, 16, 12, 18, c); }
        });
        Q(d, typeof(DewySpiderWebParameter), (p, m, t, v, v2) => // 朝露のクモの巣: 放射の糸とらせんの糸、光る露の玉
        {
            for (int i = 0; i < 12; i++) { float ag = i * MathF.PI / 6; m.Limb(0, 0, 0, MathF.Cos(ag) * 400, MathF.Sin(ag) * 400, 0, 1, 1, m.C(p.Color1, .7f)); }
            m.Tube([.. Enumerable.Range(0, 200).Select(i => { float f = i / 199f, ag = f * MathF.PI * 14; return new Vector3(MathF.Cos(ag) * (30 + 360 * f), MathF.Sin(ag) * (30 + 360 * f), 0); })], 1, m.C(p.Color1, .6f));
            for (int i = 0; i < 24; i++) { float ag = DrawKit.H(p.Seed, i) * MathF.Tau, r = 60 + 330 * DrawKit.H(p.Seed, i, 1); m.Ball(MathF.Cos(ag) * r, MathF.Sin(ag) * r, 4, 8, m.C(0xBFE6FF, .8f)); }
        });
        QS(d, typeof(CoralReefParameter), (p, m, t, v, v2) => // サンゴ礁: 海の底の砂、枝や丸のサンゴ、泳ぐ小魚
        {
            SkyWall(m, m.C(0x29B6F6), m.C(0x01579B), -1500);
            Ground(m, m.H * .35f, m.C(0xF2D9A6));
            for (int i = 0; i < 14; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W * 1.4f, z = 200 - DrawKit.H(p.Seed, i, 1) * 1200, y = m.H * .35f; var c = Pal(m, i); if (i % 2 == 0) m.Ball(x, y - 50, z, 90, 60, 90, c); else for (int k = 0; k < 4; k++) m.Tube([new(x, y, z), new(x + (k - 1.5f) * 30, y - 80, z), new(x + (k - 1.5f) * 60, y - 160, z)], 8, c); }
            for (int i = 0; i < 10; i++) { float x = ((m.Now * 80 + i * 200) % (m.W * 1.4f)) - m.W * .7f, y = -m.H * .1f + MathF.Sin(i) * 100, z = -500 + i * 40; m.Ball(x, y, z, 20, 10, 8, m.C(0xFFD54A)); }
        });
        QS(d, typeof(AvalancheParameter), (p, m, t, v, v2) => // 雪崩: 斜面を転がり落ちてくる、雪の玉のかたまり
        {
            m.Quad3D(new(-m.W, -m.H, -1500), new(m.W, -m.H, -1500), new(m.W, m.H / 2, 400), new(-m.W, m.H / 2, 400), m.C(0xE8F0F8));
            float f = In(m, 0, 3);
            for (int i = 0; i < 40; i++) { float g = Math.Clamp(f * 1.3f - DrawKit.H(p.Seed, i) * .3f, 0, 1), x = (DrawKit.H(p.Seed, i, 1) - .5f) * m.W * 1.5f, y = -m.H + (m.H * 1.5f) * g, z = -1500 + 1900 * g; m.Ball(x, y - 60, z, 60 + 80 * DrawKit.H(p.Seed, i, 2), W); }
        });
        Q(d, typeof(GroundCrackParameter), (p, m, t, v, v2) => // 地割れ: 地面の板が割れて、ずれて段になる
        {
            float k = Ease(In(m, 0, .6f)), sh = MathF.Sin(m.Now * 40) * 6 * MathF.Max(0, 1 - m.Now);
            m.Move(sh, 0, () =>
            {
                var crack = Enumerable.Range(0, 9).Select(i => new Vector2(-600 + i * 150, MathF.Sin(i * 2.3f) * 40)).ToList();
                var top = new List<Vector2> { new(-600, -300), new(600, -300) }; top.AddRange(crack.AsEnumerable().Reverse());
                var bot = new List<Vector2>(crack) { new(600, 300), new(-600, 300) };
                m.Slab(top, 0, 60, m.C(p.Color1)); m.Move(0, 30 * k, -50 * k, () => m.Slab(bot, 0, 60, m.C(p.Color1)));
                m.Tube([.. crack.Select(q => new Vector3(q, -40))], 14 * k + 1, K);
            });
        });
        QS(d, typeof(HazyMoonParameter), (p, m, t, v, v2) => // おぼろ月: ぼんやりかすんだ月と、前を流れる透けた雲
        {
            SkyWall(m, m.C(0x1A2440), m.C(0x2A3460), -1500);
            m.Ball(0, -m.H * .1f, -1300, 300, m.C(p.Color1, .15f)); m.Ball(0, -m.H * .1f, -1300, 180, m.C(p.Color1, .9f));
            for (int i = 0; i < 5; i++) m.Ball(((m.Now * 20 + i * 400) % (m.W * 2)) - m.W, -m.H * .1f + (i - 2) * 60, -900 + i * 100, 400, 60, 100, m.C(0xC8D0E0, .2f));
        });
        QS(d, typeof(MistyLakeParameter), (p, m, t, v, v2) => // 朝霧の湖: 山並み、水面に映る逆さの山、ただよう霧の層
        {
            SkyWall(m, m.C(0xC8D8E8), m.C(0xE8EEF2), -2000);
            for (int i = 0; i < 3; i++) { float x = (i - 1) * m.W * .4f; m.Lathe(x, -1500 + i * 100, m.C(0x5A7080), 0, -m.H * .25f, m.W * .3f, 0, 0, 0); m.Lathe(x, -1500 + i * 100, m.C(0x5A7080, .4f), 0, m.H * .25f, m.W * .3f, 0, 0, 0); }
            Ground(m, 0, m.C(p.Color1, .6f), 400, -2000);
            for (int i = 0; i < 4; i++) m.Box(MathF.Sin(m.Now * .3f + i) * 100, -20 - i * 30, -400 - i * 300, m.W * 2, 40, 300, m.C(0xFFFFFF, .25f), .5f);
        });
        QS(d, typeof(SunflowerFieldParameter), (p, m, t, v, v2) => // ひまわり畑: 青空と、奥まで並ぶ立体のひまわり
        {
            SkyWall(m, m.C(0x4FB8F2), m.C(0xBFE6FF), -2000);
            for (int r = 0; r < 5; r++) for (int i = 0; i < 9; i++)
                {
                    float x = (i - 4) * 260 + (r % 2) * 130, z = 200 - r * 350, y = m.H * .1f + r * -10, sw = MathF.Sin(m.Now + i + r) * .05f;
                    m.Turn(x, m.H * .5f, sw, () => { m.Limb(x, m.H * .5f, z, x, y, z, 8, 8, m.C(0x4E8F4A)); for (int k = 0; k < 14; k++) { float ag = k * MathF.Tau / 14; m.Ball(x + MathF.Cos(ag) * 60, y + MathF.Sin(ag) * 60, z + 4, 26, 12, 8, m.C(p.Color1)); } m.Disc(x, y, z + 8, 42, 42, 16, m.C(0x6D4C41), .5f); });
                }
        });
        Q(d, typeof(RollingAcornsParameter), (p, m, t, v, v2) => // どんぐり: 坂を転がっていく、帽子つきのどんぐり
        {
            m.Turn(0, 0, .15f, () => m.Box(0, 120, 0, 1400, 20, 300, m.C(0x8D6E63), .1f));
            for (int i = 0; i < Math.Max(3, p.Count); i++)
            {
                float f = ((m.Now * .3f + i / 5f) % 1), x = -600 + 1200 * f, y = 80 + x * .15f, z = (i % 3 - 1) * 80;
                m.Turn(x, y, f * 20, () => { m.Ball(x, y + 5, z, 34, 40, 34, m.C(p.Color1)); m.Ball(x, y - 25, z, 38, 20, 38, m.C(0x8A6A40)); m.Limb(x, y - 45, z, x, y - 60, z, 4, 3, m.C(0x6D4C41)); });
            }
        });
        Q(d, typeof(SnailOnLeafParameter), (p, m, t, v, v2) => // 雨上がりのカタツムリ: 大きな葉の上を、ゆっくり進むカタツムリ
        {
            m.Pitch(0, 0, 0, .5f, () => m.Ball(0, 100, 0, 600, 250, 20, m.C(0x5E9F4A)));
            float x = -200 + ((m.Now * 20) % 400);
            m.Super(x, 60, 60, 180, 25, 60, .6f, .7f, m.C(0xD9C4A0));
            m.Ball(x - 40, -30, 60, 110, m.C(p.Color1));
            m.Tube([.. Enumerable.Range(0, 30).Select(i => { float ag = i * .3f, r = 100 - i * 3; return new Vector3(x - 40 + MathF.Cos(ag) * r, -30 + MathF.Sin(ag) * r, 165); })], 4, Sculpt.Dark(m.C(p.Color1), .7f));
            foreach (int s in Sides) { m.Limb(x + 150, 40, 60 + s * 15, x + 170, -40, 60 + s * 25, 4, 4, m.C(0xD9C4A0)); m.Ball(x + 170, -45, 60 + s * 25, 9, K); }
        });

        // ---------------------------------------------------------------- 205 和の行事・小物
        QS(d, typeof(ComingOfAgeParameter), (p, m, t, v, v2) => // 成人式: 振袖の人影(円すいの着物と帯)と、舞う鶴
        {
            SkyWall(m, m.C(0xFFF3E0), m.C(0xFFE0B2), -1500);
            m.Lathe(0, -200, m.C(p.Color1), 0, -100, 140, -100, 220, m.H * .45f, 0, m.H * .45f);
            m.Cyl(0, -60, -200, 150, 80, m.C(0xFFD54A), .1f);
            m.Ball(0, -230, -200, 70, m.C(0xFFE0C8)); m.Ball(0, -260, -210, 80, 70, 70, K);
            for (int i = 0; i < 3; i++) { float x = -m.W * .3f + ((m.Now * 60 + i * 300) % (m.W * .8f)), y = -m.H * .3f + i * 60, fl = MathF.Sin(m.Now * 4 + i) * .4f; m.Ball(x, y, -400, 40, 14, 14, W); foreach (int s in Sides) m.Pitch(x, y, -400, s * fl, () => m.Tri3D(new(x - 20, y, -400), new(x + 20, y, -400), new(x, y, -400 + s * 90), W)); }
        });
        Q(d, typeof(KabutoDecorParameter), (p, m, t, v, v2) => // 兜飾り: 半球の鉢、左右に張り出す吹き返し、金の鍬形、台
        {
            var prof = new List<Vector2>(); for (int i = 0; i <= 10; i++) { float a0 = i / 10f * 1.5708f; prof.Add(new(MathF.Sin(a0) * 200, 50 - MathF.Cos(a0) * 200)); }
            m.Lathe(0, 0, m.C(p.Color1), prof);
            foreach (int s in Sides) { m.Turn(s * 200, 50, s * .4f, () => m.Box(s * 260, 80, 0, 140, 160, 30, m.C(p.Color1), .2f)); m.Tube([new(s * 40, -120, 160), new(s * 160, -260, 200), new(s * 240, -420, 180)], 14, m.C(0xFFD54A)); }
            m.Box(0, 280, 0, 600, 60, 400, m.C(0x8A1F1F), .1f);
        });
        QS(d, typeof(FloatingLanternsParameter), (p, m, t, v, v2) => // 灯籠流し: 暗い水面に浮かび流れていく、光る灯籠とその映り
        {
            SkyWall(m, m.C(0x060818), m.C(0x101830), -2000);
            Ground(m, m.H * .2f, m.C(0x0A1830, .95f), 400, -2000);
            for (int i = 0; i < 12; i++) { float x = (((DrawKit.H(p.Seed, i) * m.W + m.Now * 30) % (m.W * 1.6f)) - m.W * .8f), z = 200 - DrawKit.H(p.Seed, i, 1) * 1800, y = m.H * .2f; m.Box(x, y - 50, z, 80, 100, 80, m.C(p.Color1, .9f), .1f); m.Box(x, y - 4, z, 100, 10, 100, m.C(0x6D4C41), .1f); m.Ball(x, y + 60, z, 40, 60, 40, m.C(p.Color1, .25f)); }
        });
        QS(d, typeof(BonDanceTowerParameter), (p, m, t, v, v2) => // 盆踊りのやぐら: 柱と紅白の幕、四方へ張った提灯の綱
        {
            SkyWall(m, m.C(0x0A1030), m.C(0x1A2050), -1800);
            Ground(m, m.H * .4f, m.C(0x3A3020));
            m.Box(0, m.H * .1f, -600, 400, 300, 400, m.C(0x8A5A32), .05f);
            for (int i = 0; i < 8; i++) m.Box(-180 + i * 52, m.H * .2f, -395, 50, 120, 4, i % 2 == 0 ? m.C(0xE53935) : W, .05f);
            foreach (int s in Sides) { var rope = Enumerable.Range(0, 11).Select(i => { float f = i / 10f; return new Vector3(s * (200 + 900 * f), -m.H * .15f + MathF.Sin(f * MathF.PI) * 60 + f * 100, -600 + 400 * f); }).ToList(); m.Tube(rope, 2, K); for (int i = 1; i < 10; i++) m.Ball(rope[i].X, rope[i].Y + 40, rope[i].Z, 26, 36, 26, m.C(p.Color1)); }
        });
        Q(d, typeof(ShichiGoSanParameter), (p, m, t, v, v2) => // 七五三: 紅白のしまの千歳飴と、鶴亀の絵の長い袋
        {
            m.Box(-150, 0, 0, 200, 600, 30, m.C(p.Color1), .1f); m.Text("千歳飴", 50, -150, -150, 16, 4, W);
            m.Ball(-150, 80, 20, 40, 14, 6, W);
            m.Turn(150, 0, -.2f, () => { m.Cyl(150, 0, 0, 30, 650, W, .2f); for (int i = 0; i < 8; i++) m.Cyl(150, -280 + i * 80, 0, 31, 20, m.C(0xE53935), .2f); });
        });
        Q(d, typeof(BigCleaningParameter), (p, m, t, v, v2) => // 大掃除: 左右に動くほうき(柄と穂)と、舞い上がるほこりの玉
        {
            float sw = MathF.Sin(m.Now * 4) * .5f;
            m.Turn(0, -300, sw, () => { m.Cyl(0, -50, 0, 12, 500, m.C(0x8A5A32), .2f); m.Lathe(0, 0, m.C(0xD9B060), 30, 200, 120, 320, 0, 320); });
            for (int i = 0; i < 16; i++) { float f = ((m.Now * .6f + i / 16f) % 1); m.Ball((DrawKit.H(p.Seed, i) - .5f) * 600 * f, 320 - 300 * f, (DrawKit.H(p.Seed, i, 1) - .5f) * 300, 10 + 30 * f, m.C(0xBDBDBD, .6f * (1 - f))); }
        });
        Q(d, typeof(NewYearCardParameter), (p, m, t, v, v2) => // 年賀状: 少し傾いた厚いはがき、文字と赤い丸いはんこ
        {
            m.Turn(0, 0, -.08f, () => { m.Box(0, 0, 0, 420, 620, 6, m.C(0xFFFBF2), .02f); m.Text(Str(p), 80, 0, -100, 4, 3, K, 380); m.Disc(100, 180, 6, 70, 70, 10, m.C(p.Color1), .3f); m.Text("寿", 60, 100, 180, 12, 2, W); });
        });
        Q(d, typeof(KagamiMochiParameter), (p, m, t, v, v2) => // 鏡餅: 三方の台、紙、大小2段の丸い餅、上のみかん
        {
            m.Box(0, 250, 0, 500, 120, 400, m.C(0xD9B07E), .1f); m.Box(0, 180, 0, 440, 20, 360, W, .1f);
            m.Ball(0, 100, 0, 220, 90, 220, m.C(0xFFFBF2)); m.Ball(0, -20, 0, 170, 70, 170, m.C(0xFFFBF2));
            m.Ball(0, -120, 0, 60, m.C(p.Color1)); m.Ball(0, -180, 0, 20, 10, 30, m.C(0x4E8F4A));
        });
        Q(d, typeof(JoyaBellParameter), (p, m, t, v, v2) => // 除夜の鐘: つり鐘(回転体)と、ぶつかる撞木、広がる音の輪
        {
            m.Box(0, -360, 0, 700, 40, 100, m.C(0x5D4037), .1f);
            m.Lathe(0, 0, m.C(p.Color1), 0, -330, 120, -320, 140, -200, 160, 50, 190, 120, 0, 120);
            float ph = (m.Now * .5f) % 1, hit = ph < .15f ? 0 : MathF.Min(1, (ph - .15f) * 4);
            m.Turn(400, -300, 0, () => m.Cyl(220 + 200 * hit, -40, 0, 30, 300, m.C(0x8A6A40), .2f));
            m.Torus(0, -40, 0, 200 + 400 * ph, 5, m.C(0xFFFFFF, (1 - ph) * .6f));
        });
        Q(d, typeof(OmikujiParameter), (p, m, t, v, v2) => // おみくじ: ふられる六角の筒と、飛び出す棒、開く紙と結果
        {
            float sh = m.Now < 1.2f ? MathF.Sin(m.Now * 20) * .3f : 0;
            m.Turn(-200, 100, sh, () => { int keep = m.Seg; m.Seg = 6; m.Cyl(-200, 0, 0, 90, 340, m.C(0xC9A06A), .1f); m.Seg = keep; });
            if (m.Now > 1.2f) { float k = Ease(In(m, 1.4f, .5f)); m.Box(220, 0, 0, 260 * k, 520, 6, W, .02f); m.Text(Str(p), 140, 220, 0, 6, 6, m.C(p.Color1)); }
        });
        Q(d, typeof(KakizomeParameter), (p, m, t, v, v2) => // 書き初め: 下敷きの毛せん、長い半紙と墨の字、筆
        {
            m.Box(0, 0, -10, 600, 900, 10, m.C(0xC62828), .02f); m.Box(0, 0, 0, 440, 820, 4, W, .01f);
            m.Text(Str(p), 160, 0, 0, 4, 6, m.C(p.Color1), 400);
            m.Turn(260, -200, .6f, () => { m.Cyl(300, -280, 60, 14, 300, m.C(0x5D4037), .2f); m.Lathe(300, 60, K, 14, -130, 4, -60, 0, -60); });
        });
        Q(d, typeof(DarumaEyesParameter), (p, m, t, v, v2) => // だるま: 赤い丸い体、白い顔、入れられる目の玉
        {
            m.Ball(0, 0, 0, 280, 300, 260, m.C(p.Color1));
            m.Ball(0, -80, 200, 160, 120, 60, m.C(0xFFF5E6));
            foreach (int s in Sides) { m.Ball(s * 70, -100, 250, 34, W); if (s < 0 || m.Now > 1) m.Ball(s * 70, -100, 280, 20, K); }
            m.Tube(ArcPts3(0, 50, 100, .3f, 2.8f, 230, 12), 6, m.C(0xFFD54A));
        });
        Q(d, typeof(LuckyCatParameter), (p, m, t, v, v2) => // 招き猫: 丸い体と頭、三角の耳、上下にゆれる招く手、小判
        {
            var c = m.C(0xFFFFFF);
            m.Ball(0, 120, 0, 200, 220, 170, c); m.Ball(0, -150, 0, 160, 140, 140, c);
            foreach (int s in Sides) { m.Tri3D(new(s * 60, -260, 20), new(s * 150, -250, 20), new(s * 120, -340, 20), c); m.Ball(s * 60, -170, 120, 14, K); }
            float wave = MathF.Sin(m.Now * 4) * .3f;
            m.Turn(-200, 60, wave, () => { m.Limb(-200, 60, 0, -250, -120, 0, 45, 40, c); m.Ball(-250, -145, 0, 48, c); });
            m.Disc(0, 130, 170, 90, 50, 14, m.C(0xFFD54A), .3f); m.Torus(0, -40, 120, 100, 10, m.C(p.Color1));
        });
        Q(d, typeof(HanafudaCardsParameter), (p, m, t, v, v2) => // 花札: 扇形に開く厚い札(赤いふちと絵の玉)
        {
            for (int i = 0; i < 5; i++) { float ag = (i - 2) * .25f, k = Ease(In(m, i * .1f, .4f)); m.Turn(0, 300, ag * k, () => { m.Box(0, 0, i * 8, 200, 320, 10, m.C(0x8A1F1F), .05f); m.Box(0, 0, i * 8 + 6, 180, 300, 2, m.C(0xFFF8EC), .05f); m.Ball(0, -40, i * 8 + 10, 50, 50, 10, Pal(m, i)); }); }
        });
        Q(d, typeof(WagasaRainParameter), (p, m, t, v, v2) => // 和傘と雨: 骨の見える円すいの傘、柄、降る雨粒
        {
            m.Turn(0, 0, -.2f, () =>
            {
                m.Lathe(0, 0, m.C(p.Color1), 0, -260, 380, -100, 0, -100);
                for (int i = 0; i < 16; i++) { float ag = i * MathF.Tau / 16; m.Limb(0, -258, 0, MathF.Cos(ag) * 378, -100, MathF.Sin(ag) * 378, 2, 2, m.C(0x6D4C41)); }
                m.Cyl(0, 120, 0, 8, 440, m.C(0x6D4C41), .2f);
            });
            for (int i = 0; i < 30; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * 1400, y = ((m.Now * 800 + DrawKit.H(p.Seed, i, 1) * 900) % 900) - 450, z = (DrawKit.H(p.Seed, i, 2) - .5f) * 600; m.Limb(x, y, z, x - 6, y + 40, z, 1.5f, 1.5f, m.C(0xBFD9FF, .6f)); }
        });
        QS(d, typeof(TeaRoomParameter), (p, m, t, v, v2) => // 茶室: 畳の床、障子の壁、床の間の掛け軸、手前の抹茶の茶碗
        {
            Room(m, m.C(0xE8DCC0), m.C(0xA8B070), 1200, m.C(0x8A6A40));
            for (int i = 0; i < 4; i++) m.Box(-m.W * .3f + i * m.W * .2f, 0, -1190, m.W * .19f, m.H * .7f, 6, m.C(0xFFFBF0), .02f);
            m.Box(m.W * .35f, -m.H * .1f, -1180, 140, 400, 6, m.C(0xF5ECD7), .02f);
            m.Lathe(0, 0, m.C(0x4A3A30), 0, m.H * .4f, 90, m.H * .4f, 110, m.H * .25f, 0, m.H * .25f);
            m.Ball(0, m.H * .25f, 0, 100, 10, 100, m.C(p.Color1));
        });
        QS(d, typeof(CrestSymbolParameter), (p, m, t, v, v2) => // 家紋: 厚い丸い黒の札に、浮き彫りの花の紋
        {
            m.Disc(0, 0, 0, 300, 300, 60, m.C(p.Color1), .3f); m.Torus(0, 0, 32, 270, 8, W);
            for (int i = 0; i < 5; i++) { float ag = -MathF.PI / 2 + i * MathF.Tau / 5; m.Ball(MathF.Cos(ag) * 110, MathF.Sin(ag) * 110, 40, 90, 90, 16, W); }
            m.Disc(0, 0, 50, 50, 50, 10, m.C(p.Color1), .5f);
        });
        Q(d, typeof(KabukiMieParameter), (p, m, t, v, v2) => // 歌舞伎の見得: 白い顔に赤い隈取りの線、にらむ目、放射の光
        {
            float k = Ease(In(m, 0, .3f));
            for (int i = 0; i < 16; i++) { float ag = i * MathF.Tau / 16; m.Tri3D(new(0, 0, -50), new(MathF.Cos(ag - .1f) * 700 * k, MathF.Sin(ag - .1f) * 700 * k, -100), new(MathF.Cos(ag + .1f) * 700 * k, MathF.Sin(ag + .1f) * 700 * k, -100), m.C(0xFFD54A, .4f)); }
            m.Ball(0, 0, 0, 220, 260, 200, W);
            foreach (int s in Sides) { m.Tube(Sculpt.OnFront(0, 0, 0, 220, 260, 200, 3, [new(s * 40, -80), new(s * 140, -160), new(s * 200, -60)]), 8, m.C(p.Color1)); m.Ball(s * 80, -60, 180, 30, 14, 8, K); }
            m.Tube(Sculpt.OnFront(0, 0, 0, 220, 260, 200, 3, Sculpt.ArcPts(0, 160, 80, 3.6f, 5.8f, 10)), 8, m.C(p.Color1));
        });
        Q(d, typeof(FoldingFanParameter), (p, m, t, v, v2) => // 扇子: 骨を軸に、山折り谷折りしながら開く扇
        {
            float k = Ease(In(m, 0, .8f)), span = MathF.PI * .9f * k; int n = 12;
            for (int i = 0; i < n; i++)
            {
                float a0 = -MathF.PI / 2 - span / 2 + span * i / n, a1 = a0 + span / n, z0 = i % 2 == 0 ? 0 : 20, z1 = i % 2 == 0 ? 20 : 0;
                m.Quad3D(new(MathF.Cos(a0) * 120, 200 + MathF.Sin(a0) * 120, z0), new(MathF.Cos(a0) * 420, 200 + MathF.Sin(a0) * 420, z0), new(MathF.Cos(a1) * 420, 200 + MathF.Sin(a1) * 420, z1), new(MathF.Cos(a1) * 120, 200 + MathF.Sin(a1) * 120, z1), i % 2 == 0 ? m.C(p.Color1) : Sculpt.Dark(m.C(p.Color1), .85f));
                m.Limb(0, 200, -4, MathF.Cos(a0) * 420, 200 + MathF.Sin(a0) * 420, z0 - 4, 3, 3, m.C(0x6D4C41));
            }
            m.Ball(0, 200, 10, 14, m.C(0xC9A227));
        });
        QS(d, typeof(SantaMoonSleighParameter), (p, m, t, v, v2) => // サンタのソリ: 大きな月の前を横切る、ソリとトナカイの影
        {
            SkyWall(m, m.C(0x0A1030), m.C(0x1A2050), -1500);
            m.Ball(0, -m.H * .1f, -1400, 400, m.C(0xFFF3C0));
            float x = ((m.Now * 150) % (m.W * 2)) - m.W, y = -m.H * .1f + MathF.Sin(m.Now) * 40;
            var c = m.C(p.Color1);
            m.Box(x, y, -800, 200, 60, 80, c, .3f); m.Tube([new(x - 110, y + 40, -800), new(x + 110, y + 40, -800), new(x + 140, y + 10, -800)], 6, c);
            for (int i = 0; i < 3; i++) { float rx = x + 250 + i * 150; m.Ball(rx, y - 20, -800, 60, 30, 30, c); m.Ball(rx + 60, y - 60, -800, 20, c); m.Limb(rx + 60, y - 70, -800, rx + 80, y - 110, -800, 3, 3, c); }
        });

        // ---------------------------------------------------------------- 206 ファンタジー・SF3
        Q(d, typeof(AlchemyCauldronParameter), (p, m, t, v, v2) => // 錬金の釜: 丸い黒い釜と3本の脚、ぐつぐつ泡立つ色の液と煙
        {
            var prof = new List<Vector2> { new(0, 220) }; for (int i = 1; i <= 10; i++) { float th = i / 10f * 1.8f; prof.Add(new(MathF.Sin(th) * 260, 0 + MathF.Cos(th) * 220)); } prof.Add(new(240, -90));
            m.Lathe(0, 0, m.C(0x2A2A2A), prof);
            m.Pitch(0, -90, 0, MathF.PI / 2, () => m.Torus(0, -90, 0, 240, 14, m.C(0x3A3A3A)));
            m.Ball(0, -80, 0, 230, 20, 230, m.C(p.Color1));
            for (int i = 0; i < 8; i++) { float f = ((m.Now * .8f + i / 8f) % 1); m.Ball((DrawKit.H(p.Seed, i) - .5f) * 300, -90 - f * 250, (DrawKit.H(p.Seed, i, 1) - .5f) * 300, 14 + 20 * f, m.C(p.Color1, .7f * (1 - f))); }
            for (int i = 0; i < 3; i++) { float ag = i * MathF.Tau / 3; m.Limb(MathF.Cos(ag) * 180, 150, MathF.Sin(ag) * 180, MathF.Cos(ag) * 220, 300, MathF.Sin(ag) * 220, 14, 10, K); }
        });
        QS(d, typeof(DragonBreathParameter), (p, m, t, v, v2) => // ドラゴンの炎: 奥から手前へ、ふくらみながら吹きつける炎の玉
        {
            for (int i = 0; i < 40; i++) { float f = ((m.Now * 1.2f + i / 40f) % 1), x = -m.W * .4f + m.W * .5f * f, y = -m.H * .2f + MathF.Sin(i) * 80 * f, z = -1200 + 1500 * f; m.Ball(x, y, z, 40 + 200 * f, Vector4.Lerp(m.C(0xFFEB3B), m.C(p.Color1), f) with { W = (1 - f) * .8f * m.Al }); }
        });
        Q(d, typeof(ElementOrbsParameter), (p, m, t, v, v2) => // 4属性のオーブ: 回る4色の光る玉と、まん中の輪
        {
            uint[] cols = [0xE53935, 0x1E88E5, 0x43A047, 0xFDD835];
            for (int i = 0; i < 4; i++) { float ag = m.Now * .6f + i * MathF.PI / 2; float x = MathF.Cos(ag) * 280, z = MathF.Sin(ag) * 280; m.Ball(x, MathF.Sin(m.Now * 2 + i) * 30, z, 80, m.C(cols[i], .35f)); m.Ball(x, MathF.Sin(m.Now * 2 + i) * 30, z, 50, m.C(cols[i])); }
            m.Pitch(0, 0, 0, MathF.PI / 2, () => m.Torus(0, 0, 0, 280, 3, m.C(0xFFFFFF, .4f)));
        });
        Q(d, typeof(CurseGlyphsParameter), (p, m, t, v, v2) => // 呪いの紋様: 広がっていく黒紫の線のつるが、地面の板をはう
        {
            float k = Ease(In(m, 0, 2));
            for (int b = 0; b < 8; b++) { float ag = b * MathF.PI / 4; var pts = Enumerable.Range(0, 20).Select(i => { float f = i / 19f * k, a1 = ag + MathF.Sin(f * 6 + b) * .5f; return new Vector3(MathF.Cos(a1) * 450 * f, MathF.Sin(a1) * 300 * f, 4); }).ToList(); m.Tube(pts, 5, m.C(p.Color1)); }
            m.Ball(0, 0, 0, 80 * k, m.C(p.Color1, .6f));
        });
        Q(d, typeof(SealingTalismanParameter), (p, m, t, v, v2) => // 封印のお札: 貼りついた長い札と朱の文字、まわりで光る結界の輪
        {
            float sh = MathF.Sin(m.Now * 30) * .03f;
            m.Turn(0, -300, sh, () => { m.Box(0, 0, 10, 220, 640, 4, m.C(0xFFF3D6), .01f); m.Text(Str(p).Length > 0 ? Str(p) : "封", 140, 0, 0, 14, 4, m.C(p.Color1)); });
            m.Torus(0, 0, 0, 380, 4, m.C(p.Color1, .5f + .5f * MathF.Sin(m.Now * 3)));
        });
        Q(d, typeof(PetrifyParameter), (p, m, t, v, v2) => // 石化: 下から灰色の石に変わっていく人形と、ひび
        {
            float k = Ease(In(m, 0, 2)), line = 300 - 600 * k;
            Vector4 Col(float y) => y > line ? m.C(0x8A8A8A) : m.C(p.Color1);
            m.Ball(0, -220, 0, 70, Col(-220)); m.Limb(0, -140, 0, 0, 80, 0, 80, 60, Col(-30));
            foreach (int s in Sides) { m.Limb(s * 40, 80, 0, s * 50, 300, 0, 30, 26, Col(190)); m.Limb(s * 90, -120, 0, s * 150, 60, 0, 24, 20, Col(-30)); }
            m.Tube([new(-30, 300, 70), new(10, 200, 70), new(-20, line + 40, 70)], 2, K);
        });
        QS(d, typeof(TimeRewindParameter), (p, m, t, v, v2) => // 時を戻す: 逆に回る大きな時計の輪と針、流れていく光の線
        {
            m.Torus(0, 0, 0, 340, 12, m.C(p.Color1));
            for (int i = 0; i < 12; i++) { float ag = i * MathF.PI / 6; m.Ball(MathF.Cos(ag) * 300, MathF.Sin(ag) * 300, 0, 10, m.C(p.Color1)); }
            m.Turn(0, 0, -m.Now * 6, () => m.Limb(0, 0, 10, 0, -260, 10, 8, 3, m.C(p.Color1))); m.Turn(0, 0, -m.Now * .5f, () => m.Limb(0, 0, 14, 0, -180, 14, 10, 4, m.C(p.Color1)));
            for (int i = 0; i < 20; i++) { float f = ((m.Now + i / 20f) % 1); m.Box(m.W / 2 - m.W * f, (DrawKit.H(p.Seed, i) - .5f) * m.H, -300, 200, 3, 3, m.C(p.Color1, .4f * (1 - f)), .5f); }
        });
        Q(d, typeof(TelepathyWaveParameter), (p, m, t, v, v2) => // テレパシー: 2つの頭の間を行き来する、波打つ光の輪
        {
            foreach (int s in Sides) m.Ball(s * 400, 0, 0, 100, m.C(0x90A4AE));
            for (int i = 0; i < 6; i++) { float f = ((m.Now * .6f + i / 6f) % 1), x = -300 + 600 * f; m.Yaw(x, 0, 0, MathF.PI / 2, () => m.Torus(x, 0, 0, 40 + 40 * MathF.Sin(f * MathF.PI), 4, m.C(p.Color1, MathF.Sin(f * MathF.PI)))); }
        });
        QS(d, typeof(FleetBarrageParameter), (p, m, t, v, v2) => // 宇宙戦艦の砲撃: 星空に浮かぶ戦艦の箱と砲身、奥へ飛ぶ光の弾
        {
            m.Box(0, 0, -1500, m.W * 3, m.H * 3, 10, m.C(0x020210), .01f);
            for (int k = 0; k < 3; k++) { float x = (k - 1) * 500, y = (k - 1) * 120, z = -400 - k * 300; m.Box(x, y, z, 500, 80, 120, m.C(0x607D8B), .1f); m.Box(x - 60, y - 60, z, 160, 60, 100, m.C(0x546E7A), .1f); m.Limb(x + 250, y, z, x + 350, y, z, 10, 10, m.C(0x37474F)); for (int i = 0; i < 4; i++) { float f = ((m.Now * 1.5f + i / 4f + k * .2f) % 1); m.Limb(x + 350 + 1200 * f, y, z - 800 * f, x + 450 + 1200 * f, y, z - 800 * f - 60, 6, 6, m.C(p.Color1)); } }
        });
        Q(d, typeof(UfoEscapeParameter), (p, m, t, v, v2) => // UFO: 円盤(ドームとふち)が光の柱を残して奥へ飛び去る
        {
            float k = Ease(In(m, .3f, 1.2f)), x = 600 * k, y = -500 * k, z = -1500 * k;
            m.Move(x, y, z, () =>
            {
                m.Ball(0, 0, 0, 260, 50, 260, m.C(0x90A4AE));
                var prof = new List<Vector2>(); for (int i = 0; i <= 8; i++) { float a0 = i / 8f * 1.5708f; prof.Add(new(MathF.Sin(a0) * 110, -20 - MathF.Cos(a0) * 100)); }
                m.Lathe(0, 0, m.C(p.Color1, .6f), prof);
                for (int i = 0; i < 8; i++) { float ag = i * MathF.PI / 4 + m.Now * 3; m.Ball(MathF.Cos(ag) * 230, 10, MathF.Sin(ag) * 230, 14, Pal(m, i)); }
            });
            if (k < .3f) m.Cyl(0, 300, 0, 160, 600, m.C(0xFFF59D, .25f * (1 - k / .3f)), .05f);
        });
        QS(d, typeof(AlienTextFlowParameter), (p, m, t, v, v2) => // 異星の文字: 奥行きのある列で流れ落ちる、光る記号のブロック
        {
            for (int c = 0; c < 18; c++) { float x = -m.W / 2 + m.W * (c + .5f) / 18, z = -DrawKit.H(p.Seed, c) * 800; for (int r = 0; r < 12; r++) { float y = ((m.Now * 120 * (1 + DrawKit.H(p.Seed, c, 1)) + r * 70) % (m.H + 200)) - m.H / 2 - 100; if (DrawKit.H(p.Seed, c * 30 + r) < .5f) m.Box(x, y, z, 30, 40, 6, m.C(p.Color1, 1 - r / 12f), .2f); } }
        });
        QS(d, typeof(MechPanelsParameter), (p, m, t, v, v2) => // メカの変形: 開いていく装甲パネルと、中の光る機械
        {
            float k = Ease(In(m, .2f, 1));
            m.Box(0, 0, -200, m.W * .6f, m.H * .6f, 40, m.C(0x263238), .05f);
            for (int i = 0; i < 6; i++) m.Turn(0, 0, i * MathF.PI / 3 + m.Now * .3f, () => m.Box(0, -150, -150, 40, 200, 30, m.C(p.Color1, .6f), .3f));
            m.Ball(0, 0, -150, 80, m.C(p.Color1));
            foreach (int x in Sides) foreach (int y in Sides) m.Turn(x * m.W * .15f, y * m.H * .15f, x * y * k * .3f, () => m.Move(x * k * m.W * .15f, y * k * m.H * .15f, 100 * k, () => m.Box(x * m.W * .15f, y * m.H * .15f, 0, m.W * .3f - 10, m.H * .3f - 10, 50, m.C(0x607D8B), .05f)));
        });
        QS(d, typeof(PlanetSurfaceParameter), (p, m, t, v, v2) => // 惑星の地表: 奥へ広がる地面とクレーター、空に浮かぶ大きな惑星
        {
            SkyWall(m, m.C(0x050510), m.C(0x101020), -2500);
            m.Ball(m.W * .3f, -m.H * .35f, -2300, 600, m.C(0x8E6E9F));
            Ground(m, m.H * .25f, m.C(p.Color1), 400, -2500);
            for (int i = 0; i < 12; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W * 2, z = -DrawKit.H(p.Seed, i, 1) * 2200 + 300, r = 80 + 160 * DrawKit.H(p.Seed, i, 2); m.Pitch(x, m.H * .25f, z, MathF.PI / 2, () => m.Torus(x, m.H * .25f, z, r, r * .15f, Sculpt.Dark(m.C(p.Color1), .8f))); }
        });
        QS(d, typeof(SpaceHelmetViewParameter), (p, m, t, v, v2) => // ヘルメット越し: 丸い穴の厚いヘルメットのふち、表示の文字と曇り
        {
            HoleMask(m, Ellipse(0, 0, m.W * .42f, m.H * .45f, 64), m.C(0xE0E0E0), 0, 80);
            m.Ball(0, 0, -20, m.W * .42f, m.H * .45f, 60, m.C(0xFFFFFF, .05f));
            m.Text("O2 " + (98 - (int)(m.Now) % 10) + "%", 36, -m.W * .3f, -m.H * .3f, 10, 3, m.C(p.Color1));
            m.Text(Str(p), 34, m.W * .25f, m.H * .3f, 10, 3, m.C(p.Color1));
        });
        Q(d, typeof(HoloTableParameter), (p, m, t, v, v2) => // ホログラムのテーブル: 丸いテーブルと、上に浮かぶ回る透けた地形
        {
            m.Cyl(0, 250, 0, 400, 60, m.C(0x37474F), .1f);
            m.Pitch(0, 210, 0, MathF.PI / 2, () => m.Torus(0, 210, 0, 380, 4, m.C(p.Color1)));
            m.Yaw(0, 0, 0, m.Now * .4f, () => { for (int i = 0; i < 6; i++) for (int j = 0; j < 6; j++) { float x = (i - 2.5f) * 90, z = (j - 2.5f) * 90, hh = 40 + 160 * DrawKit.H(p.Seed, i + j * 6); m.Box(x, 100 - hh / 2, z, 70, hh, 70, m.C(p.Color1, .35f), .05f); } });
            m.Cyl(0, 50, 0, 380, 300, m.C(p.Color1, .06f), .05f);
        });
        Q(d, typeof(NanoSwarmParameter), (p, m, t, v, v2) => // ナノマシン: 渦を巻いて集まったり散ったりする、小さな粒の群れ
        {
            int keep = m.Seg; m.Seg = 6;
            for (int i = 0; i < Math.Max(80, p.Count); i++) { float ph = m.Now * .8f + DrawKit.H(p.Seed, i) * 10, r = 200 + 150 * MathF.Sin(m.Now * .7f + i * .1f), ag = ph; m.Ball(MathF.Cos(ag) * r, (DrawKit.H(p.Seed, i, 1) - .5f) * 300, MathF.Sin(ag) * r, 5, m.C(p.Color1)); }
            m.Seg = keep;
        });
        Q(d, typeof(DataCubeParameter), (p, m, t, v, v2) => // データの立方体: 回る透けた箱、中の小さな光る立方体の格子
        {
            m.Yaw(0, 0, 0, m.Now * .5f, () => m.Pitch(0, 0, 0, .4f, () =>
            {
                m.Box(0, 0, 0, 400, 400, 400, m.C(p.Color1, .15f), .02f);
                for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) for (int k = 0; k < 3; k++) { bool on = DrawKit.H(p.Seed + (int)(m.Now * 2), i * 9 + j * 3 + k) > .5f; m.Box((i - 1) * 120, (j - 1) * 120, (k - 1) * 120, 60, 60, 60, m.C(p.Color1, on ? .9f : .2f), .1f); }
            }));
        });
        Q(d, typeof(TarotSpreadParameter), (p, m, t, v, v2) => // タロット: 布の上に並べた厚いカードが、順に表に返る
        {
            m.Box(0, 0, -20, 1400, 700, 10, m.C(0x4A148C), .02f);
            var it = p.ItemsForModel;
            for (int i = 0; i < 3; i++) { float x = (i - 1) * 380, k = Ease(In(m, .3f + i * .5f, .4f)); m.Yaw(x, 0, 0, MathF.PI * (1 - k), () => { m.Box(x, 0, 0, 260, 420, 8, m.C(0xFFF8E1), .05f); m.Box(x, 0, -5, 250, 410, 2, m.C(p.Color1), .05f); Gem(m, Pts(StarPts(x, -40, 60, 8, .5f, 0)), x, -40, 0f, 2, 6, m.C(0xFFD54A), m.C(0xFFD54A)); if (i < it.Count) m.Text(it[i], 36, x, 150, 6, 3, K, 240); }); }
        });
        Q(d, typeof(CrystalBallParameter), (p, m, t, v, v2) => // 水晶玉: 台座と透けた球、中で渦巻く光、手の影
        {
            m.Lathe(0, 0, m.C(0x6D4C41), 0, 340, 200, 340, 160, 220, 0, 220);
            m.Ball(0, 0, 0, 240, m.C(0xFFFFFF, .25f));
            for (int i = 0; i < 3; i++) m.Turn(0, 0, m.Now * (1 + i * .3f) + i * 2, () => m.Tube(ArcPts3(0, 0, 60 + i * 40, 0, 4, (i - 1) * 40, 16), 6, m.C(p.Color1, .7f)));
            m.Ball(-80, -100, 180, 40, 30, 10, m.C(0xFFFFFF, .5f));
        });
        Q(d, typeof(AnvilForgeParameter), (p, m, t, v, v2) => // 鍛冶: 金床、熱く光る金属の棒、打ちつけるハンマーと火花
        {
            m.Slab(0, 200, m.C(0x37474F), -300, 100, 300, 100, 260, 40, 380, -20, -200, -20, -260, 40);
            m.Box(0, 200, 0, 300, 200, 160, m.C(0x37474F), .1f);
            m.Box(0, -40, 0, 360, 30, 30, m.C(0xFF7043), .3f);
            float ph = (m.Now * 1.5f) % 1, up = ph < .7f ? ph / .7f : 0;
            m.Turn(300, -300, -.2f - up * .8f, () => { m.Cyl(300, -150, 0, 12, 300, m.C(0x6D4C41), .2f); m.Box(300, -310, 0, 160, 70, 70, m.C(0x607D8B), .2f); });
            if (ph > .7f) for (int i = 0; i < 16; i++) { float f = (ph - .7f) / .3f, ag = -MathF.PI + i * MathF.PI / 15; m.Limb(MathF.Cos(ag) * 200 * f, -60 + MathF.Sin(ag) * 200 * f, MathF.Sin(ag * 3) * 100 * f, MathF.Cos(ag) * (200 * f + 30), -60 + MathF.Sin(ag) * (200 * f + 30), MathF.Sin(ag * 3) * 100 * f, 3, 1, m.C(p.Color2, 1 - f)); }
        });
    }
}
