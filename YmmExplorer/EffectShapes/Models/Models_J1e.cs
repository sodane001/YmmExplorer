using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 209番(小物・アイコン・フレーム 20)
// =====================================================================
internal static partial class Models3D
{
    static partial void RegisterJ1e(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.12f, .13f, .16f, 1);
        var Gold = new Vector4(.84f, .66f, .2f, 1);
        Q(d, typeof(RibbonBannerParameter), (p, m, t, v, v2) => // リボンの見出し帯: 弓なりの帯と、左右の切れ込みのある垂れ
        {
            string s = Str(p); float w = MathF.Max(520, s.Length * 70 + 140), bend = v * 70;
            foreach (int k in Sides)
            {
                m.Slab(-30, 30, m.C(p.Color2), k * w * .43f, 0 + bend * .6f, k * (w / 2 + 118), 40 + bend * .7f, k * (w / 2 + 64), 85 + bend * .7f, k * (w / 2 + 118), 118 + bend * .7f, k * w * .43f, 100 + bend * .6f);
            }
            var top = new List<Vector3>(); for (int i = 0; i <= 16; i++) { float f = i / 16f * 2 - 1; top.Add(V3(f * w / 2, (f * f - .35f) * bend, 0)); }
            for (int i = 0; i < 16; i++) { var a = top[i]; var b = top[i + 1]; m.Quad3D(a, b, b + V3(0, 118), a + V3(0, 118), m.C(p.Color1)); m.Quad3D(a + V3(0, 0, -24), b + V3(0, 0, -24), b + V3(0, 118, -24), a + V3(0, 118, -24), m.C(p.Color2)); }
            m.Tube(top.Select(q => q + V3(0, 0, -12)).ToList(), 12, m.C(p.Color1));
            m.Tube(top.Select(q => q + V3(0, 118, -12)).ToList(), 12, m.C(p.Color1));
            m.Text(s, 64, 0, 59 - .35f * bend, 6, 6, m.C(p.Color3), w - 120);
        });
        Q(d, typeof(RoyalCrownParameter), (p, m, t, v, v2) => // 王冠: 金の輪と5本の尖り、先の玉と宝石
        {
            var g = p.Variant == 2 ? m.C(0xD7DCE2) : Gold; float S = 230;
            m.Turn(0, 0, MathF.Sin(m.Now * 2) * .05f, () =>
            {
                m.Lathe(0, 0, g, S * .95f, S * .3f, S * .95f, S * .6f, S * .9f, S * .62f);
                m.Lathe(0, 0, g, S * .9f, S * .62f, S * .9f, S * .7f);
                for (int i = 0; i < 5; i++)
                {
                    float an = (i - 2) * .55f, x = MathF.Sin(an) * S * .92f, z = MathF.Cos(an) * S * .92f;
                    m.Limb(x, S * .3f, z, x * 1.05f, -S * .5f, z * 1.05f, S * .16f, S * .05f, g);
                    m.Ball(x * 1.05f, -S * .55f, z * 1.05f, S * .1f, m.C(0xFFF3C4));
                }
                for (int i = 0; i < 5; i++) { float an = (i - 2) * .55f; m.Ball(MathF.Sin(an) * S * .97f, S * .46f, MathF.Cos(an) * S * .97f, S * .09f, m.C(i % 2 == 0 ? p.Color1 : p.Color2)); }
            });
        });
        Q(d, typeof(BatteryLevelParameter), (p, m, t, v, v2) => // 電池の残量: 角の丸い電池の殻、中の残量の塊が減っていく
        {
            float lv = v + (v2 - v) * Ease(Math.Clamp(m.Now / 2.5f, 0, 1));
            var col = lv < .25f ? m.C(p.Color2) : m.C(p.Color1);
            m.Box(0, 0, -10, 560, 250, 60, m.C(0xECEFF1, .35f), .3f);
            m.Box(0, 0, 0, 560, 250, 40, m.C(0x37474F), .3f);
            m.Box(0, 0, 18, 520, 210, 20, m.C(0x111418), .3f);
            m.Box(300, 0, 0, 40, 110, 40, m.C(0x37474F), .3f);
            float fw = 500 * Math.Clamp(lv, 0, 1); if (fw > 2) m.Box(-250 + fw / 2, 0, 24, fw, 190, 40, col, .2f);
            m.Text($"{(int)MathF.Round(lv * 100)}%", 90, 0, 0, 50, 6, m.C(p.Color3));
            m.Text(Str(p), 50, 0, -190, 0, 4, m.C(p.Color3));
        });
        Q(d, typeof(SignalStrengthParameter), (p, m, t, v, v2) => // 電波の強さ: 高さの違う4本の棒が立つ
        {
            int lv = Math.Clamp(p.Count, 0, 4); if (p.Background) lv = Math.Min(lv, (int)(m.Now * 1.6f) % 8 > 4 ? 4 : (int)(m.Now * 1.6f) % 8);
            for (int i = 0; i < 4; i++) { float h = 80 + i * 70; m.Box(-165 + i * 110, 150 - h / 2, 0, 80, h, 60, i < lv ? m.C(p.Color1) : m.C(p.Color2), .25f); }
            if (lv == 0) m.Turn(0, 0, .785f, () => { m.Box(0, 0, 50, 260, 40, 30, m.C(p.Color3), .3f); m.Box(0, 0, 50, 40, 260, 30, m.C(p.Color3), .3f); });
        });
        Q(d, typeof(StampCardParameter), (p, m, t, v, v2) => // スタンプカード: 厚紙のカード、押された丸い判子が浮き出る
        {
            int n = Math.Clamp(p.Count, 2, 30), cols = Math.Min(n, n <= 10 ? 5 : 6), rows = (n + cols - 1) / cols, push = (int)MathF.Round(v * n);
            float cs = 130, cw = cols * cs + 80, ch = rows * cs + 170;
            m.Box(0, 0, 0, cw, ch, 16, m.C(p.Color1), .1f);
            m.Text(Str(p), 54, 0, -ch / 2 + 60, 10, 4, m.C(p.Color3));
            for (int i = 0; i < n; i++)
            {
                float x = -cw / 2 + 40 + cs * (i % cols + .5f), y = -ch / 2 + 130 + cs * (i / cols + .5f);
                m.Disc(x, y, 9, 52, 52, 2, m.C(p.Color3, .25f));
                float g = Math.Clamp(m.Now * 4 - i * .35f, 0, 1);
                if (i < push && g > 0) m.Move(0, 0, 60 * (1 - g), () => { m.Disc(x, y, 14, 52, 52, 10, m.C(p.Color2), .4f); m.Text("済", 44, x, y, 22, 3, W); });
            }
        });
        Q(d, typeof(TearTicketParameter), (p, m, t, v, v2) => // チケット: 本券と半券、ミシン目でちぎれて半券が傾いて落ちる
        {
            var r = Lines(p); float k = Ease(Math.Clamp((m.Now - .6f) / .8f, 0, 1));
            m.Box(-110, 0, 0, 640, 300, 14, m.C(p.Color1), .15f);
            m.Text(r.Count > 0 ? r[0] : "", 54, -110, -60, 9, 3, m.C(p.Color2), 560);
            m.Text(r.Count > 1 ? r[1] : "", 40, -110, 30, 9, 3, m.C(p.Color2), 560);
            m.Text(r.Count > 2 ? r[2] : "", 40, -110, 90, 9, 3, m.C(p.Color3), 560);
            for (int i = 0; i < 10; i++) m.Ball(210, -135 + i * 30, 0, 6, m.C(p.Color2, .6f));
            m.Move(30 * k, 260 * k * k, 0, () => m.Turn(215, 150, k * .5f, () => { m.Box(320, 0, 0, 200, 300, 14, m.C(p.Color1), .15f); m.Text("半券", 40, 320, 0, 9, 3, m.C(p.Color2)); }));
        });
        Q(d, typeof(PriceTagParameter), (p, m, t, v, v2) => // 値札: 紐で吊られて揺れる、穴のあいた五角のタグ
        {
            var r = Lines(p); float sw = MathF.Sin(m.Now * 4.2f) * MathF.Exp(-m.Now * .6f) * .25f;
            m.Limb(0, -360, 0, 0, -200, 0, 4, 4, m.C(0xBDBDBD));
            m.Turn(0, -200, sw, () =>
            {
                m.Slab(-14, 28, m.C(p.Color1), 0, -200, 190, -80, 190, 240, -190, 240, -190, -80);
                m.Torus(0, -110, 16, 24, 7, m.C(0xCFD8DC));
                m.Text(r.Count > 0 ? r[0] : "", 46, 0, 0, 17, 3, m.C(p.Color2, .7f), 340);
                m.Box(0, 0, 20, 220, 6, 4, m.C(p.Color2, .8f));
                m.Text(r.Count > 1 ? r[1] : "", 74, 0, 110, 17, 5, m.C(p.Color3), 340);
            });
        });
        Q(d, typeof(AlarmClockRingParameter), (p, m, t, v, v2) => // 目覚まし時計: 2つのベルと脚、震えて鳴る丸い時計
        {
            float R = 240, rot = MathF.Sin(m.Now * 42) * .075f * v, sx = MathF.Sin(m.Now * 37) * 12 * v;
            m.Move(sx, 0, 0, () => m.Turn(0, 0, rot, () =>
            {
                m.Disc(0, 0, 0, R, R, 90, m.C(p.Color1), .4f);
                m.Disc(0, 0, 46, R * .82f, R * .82f, 6, m.C(p.Color2), .2f);
                for (int i = 0; i < 12; i++) { float an = i * MathF.Tau / 12; m.Ball(MathF.Sin(an) * R * .7f, -MathF.Cos(an) * R * .7f, 52, 9, K); }
                m.Limb(0, 0, 54, 0, -R * .55f, 54, 10, 8, K);
                m.Limb(0, 0, 58, R * .4f, 0, 58, 8, 6, K);
                foreach (int s in Sides) { m.Lathe(s * R * .72f, 0, m.C(p.Color3), R * .02f, -R * 1.25f, R * .3f, -R * 1.08f, R * .34f, -R * .9f); m.Limb(s * R * .6f, R * .8f, 0, s * R * .8f, R * 1.15f, 0, 16, 12, K); }
                m.Limb(-R * .7f, -R * 1.28f, 0, R * .7f, -R * 1.28f, 0, 10, 10, K);
                m.Ball(0, -R * 1.02f, 0, R * .08f, K);
            }));
            m.Text(Str(p), 60, 0, 320, 20, 5, W);
        });
        Q(d, typeof(OnAirMicParameter), (p, m, t, v, v2) => // マイクとON AIRランプ: 網の頭のスタンドマイクと、光る箱のランプ
        {
            float lit = p.Background ? .55f + .45f * MathF.Max(0, MathF.Sin(m.Now * 4)) : 1;
            m.Ball(-160, -170, 0, 100, m.C(0x9EA7B0)); m.Torus(-160, -170, 0, 100, 10, m.C(p.Color2));
            m.Lathe(-160, 0, m.C(p.Color2), 70, -90, 50, 60, 40, 80);
            m.Limb(-160, 80, 0, -160, 330, 0, 14, 14, m.C(p.Color2));
            m.Lathe(-160, 0, m.C(0x2B2F36), 140, 330, 140, 350);
            m.Box(290, -200, 0, 380, 150, 60, m.C(0x222222), .2f);
            m.Box(290, -200, 32, 340, 115, 10, m.C(p.Color1, .35f + .65f * lit), .2f);
            m.Text(Str(p), 64, 290, -200, 40, 4, m.C(p.Color3));
        });
        Q(d, typeof(HeadphoneNotesParameter), (p, m, t, v, v2) => // ヘッドホンと音符: 輪のバンドと2つの耳当て、まわりを浮かぶ音符
        {
            float R = 270, beat = MathF.Pow(MathF.Abs(MathF.Sin(t * 3.14f)), 4);
            m.Tube(ArcPts3(0, 40, R, MathF.PI, MathF.Tau, 0), 26, m.C(p.Color1));
            foreach (int s in Sides) { m.Disc(s * R, 80, 0, 80 * (1 + beat * .06f), 110 * (1 + beat * .06f), 90, m.C(p.Color1), .5f); m.Disc(s * R * .9f, 80, 0, 60, 90, 70, m.C(p.Color2), .6f); }
            for (int i = 0; i < 5; i++)
            {
                float ph = Frac(t * .3f + i / 5f), x = (i - 2) * 140, y = -R - 50 - ph * 300;
                m.Move(x, y, 40, () => { m.Ball(0, 0, 0, 34, 26, 20, m.C(i % 2 == 0 ? p.Color2 : p.Color3, 1 - ph)); m.Limb(28, 0, 0, 28, -110, 0, 6, 6, m.C(i % 2 == 0 ? p.Color2 : p.Color3, 1 - ph)); });
            }
        });
        Q(d, typeof(OpeningNotebookParameter), (p, m, t, v, v2) => // 開くノート: 背のリングと、表紙がめくれて開くページ
        {
            var r = Lines(p); float open = Ease(Math.Clamp((m.Now - .15f) / .6f, 0, 1));
            m.Box(310, 0, -10, 620, 820, 20, m.C(p.Color2), .05f);
            for (int i = 0; i < Math.Min(r.Count, 8); i++) m.Text(r[i], i == 0 ? 52 : 40, 310, -300 + i * 90, 2, 3, m.C(p.Color3), 540);
            for (int i = 0; i < 12; i++) m.Torus(0, -360 + i * 66, 0, 24, 6, m.C(0xB0BEC5));
            m.Yaw(0, 0, 0, -open * MathF.PI * .95f, () => m.Box(310, 0, 10, 620, 820, 24, m.C(p.Color1), .05f));
        });
        Q(d, typeof(TrashTossParameter), (p, m, t, v, v2) => // ごみ箱に捨てる: 丸めた紙が弧を描いて、筒形のごみ箱へ
        {
            float dir = p.Variant == 2 ? 1 : -1, k = Math.Clamp((m.Now - .1f) / .9f, 0, 1);
            float bx = -dir * 300;
            m.Lathe(bx, 0, m.C(p.Color2), 120, 50, 150, 400); m.Lathe(bx, 0, m.C(Sculpt.Dark(m.C(p.Color2), .6f), 1), 140, 400, 105, 55);
            m.Pitch(bx, 55, 0, MathF.PI / 2, () => m.Torus(bx, 55, 0, 150, 12, m.C(p.Color2)));
            float x = dir * 300 + (bx - dir * 300) * k, y = -200 * MathF.Sin(k * MathF.PI) + 120 * k;
            if (k < 1) m.Ball(x, y - 60, 0, 60, m.C(p.Color1)); else m.Ball(bx, 200, 0, 50, m.C(p.Color1, .6f));
        });
        Q(d, typeof(InstantCameraParameter), (p, m, t, v, v2) => // インスタントカメラ: 箱の本体と大きなレンズ、下から写真が出てくる
        {
            float flash = 1 - Math.Clamp((m.Now - .35f) / .3f, 0, 1), o = Ease(Math.Clamp((m.Now - .55f) / .75f, 0, 1)), dev = Math.Clamp((m.Now - 1.2f) / 1.5f, 0, 1);
            m.Box(0, 300 * o, -20, 420, 480, 8, W, .05f);
            m.Box(0, 280 * o - 30, -14, 360, 340, 4, Sculpt.Mix(m.C(0x333333), m.C(p.Color3), dev), .02f);
            m.Text(Str(p), 40, 0, 300 * o + 190, -14, 3, K);
            m.Box(0, -60, 0, 640, 440, 200, m.C(p.Color1), .3f);
            m.Box(0, -230, 60, 640, 60, 100, m.C(p.Color2), .2f);
            m.Disc(0, -50, 120, 150, 150, 110, m.C(p.Color2), .4f); m.Disc(0, -50, 160, 115, 115, 30, m.C(0x222831), .3f);
            m.Disc(0, -50, 175, 95, 95, 10, m.C(0x0D1B2A), .3f); m.Ball(-30, -80, 185, 20, m.C(0xFFFFFF, .7f));
            m.Box(200, -170, 110, 120, 70, 10, m.C(0xFFFDE7, .4f + .6f * flash), .3f);
        });
        Q(d, typeof(ClapperboardParameter), (p, m, t, v, v2) => // カチンコ: 黒い板と、白黒しまの拍子木が閉じてカチン
        {
            var r = Lines(p); float close = m.Now < .45f ? 0 : Ease(Math.Clamp((m.Now - .45f) / .1f, 0, 1)), ang = -.45f * (1 - close);
            m.Box(0, 60, 0, 760, 440, 40, m.C(p.Color1), .05f);
            for (int i = 0; i < 2; i++) m.Box(0, -60 + i * 150, 22, 700, 4, 4, m.C(p.Color2, .7f));
            m.Text(r.Count > 0 ? r[0] : "", 56, 0, 0, 22, 4, m.C(p.Color2), 680);
            m.Text("SCENE " + (r.Count > 1 ? r[1] : ""), 40, -180, 150, 22, 3, m.C(p.Color2), 330);
            m.Text("TAKE " + (r.Count > 2 ? r[2] : ""), 40, 180, 150, 22, 3, m.C(p.Color2), 330);
            void Stick(float y) { m.Box(0, y, 0, 760, 70, 40, m.C(p.Color1), .05f); for (int i = 0; i < 6; i++) m.Turn(-320 + i * 130, y, -.6f, () => m.Box(-320 + i * 130, y, 22, 50, 80, 4, m.C(p.Color2), .05f)); }
            Stick(-200);
            m.Turn(-380, -235, ang, () => Stick(-270));
        });
        Q(d, typeof(PlacardSignParameter), (p, m, t, v, v2) => // 吹き出し型の看板: 木の柄に付いた、しっぽ付きの吹き出し板
        {
            float sway = MathF.Sin(m.Now * 3) * v * .28f;
            m.Turn(0, 460, sway, () =>
            {
                m.Box(0, 300, -20, 40, 380, 30, m.C(p.Color2), .3f);
                m.Slab(-14, 28, m.C(p.Color1), 70, 110, -10, 110, -40, 210);
                Puff(m, 0, 36, m.C(p.Color1), RoundRectFlat(0, -40, 620, 330, 60));
                m.Text(Str(p), 80, 0, -40, 20, 6, m.C(p.Color3), 560);
            });
        });
        Q(d, typeof(GoldFrameParameter), (p, m, t, v, v2) => // 金の額縁: 太い盛り上がった枠と、角の飾り玉、奥の暗い絵の面
        {
            float w = 1320, h = 760, th = 34 + v * 78;
            m.Box(0, 0, -20, w, h, 10, m.C(p.Color3), .02f);
            m.Box(0, -h / 2, 0, w + th * 2, th * 2, th, m.C(p.Color1), .4f); m.Box(0, h / 2, 0, w + th * 2, th * 2, th, m.C(p.Color1), .4f);
            m.Box(-w / 2, 0, 0, th * 2, h, th, m.C(p.Color1), .4f); m.Box(w / 2, 0, 0, th * 2, h, th, m.C(p.Color1), .4f);
            m.Box(0, -h / 2 + th, th * .4f, w - th * .2f, 10, 10, m.C(p.Color2)); m.Box(0, h / 2 - th, th * .4f, w - th * .2f, 10, 10, m.C(p.Color2));
            foreach (int a in Sides) foreach (int b in Sides) m.Ball(a * w / 2, b * h / 2, th * .6f, th * .8f, m.C(p.Color1));
            m.Text(Str(p), 140, 0, 0, -12, 6, m.C(p.Color1), w * .8f);
        });
        Q(d, typeof(WoodSignPostParameter), (p, m, t, v, v2) => // 木の立て札: 杭に打ち付けた板、落ちてきて刺さり少し揺れる
        {
            float drop = Ease(Math.Clamp(m.Now / .28f, 0, 1)), wob = MathF.Sin(MathF.Max(0, m.Now - .28f) * 18) * .04f * MathF.Exp(-MathF.Max(0, m.Now - .28f) * 3);
            m.Move(0, -500 * (1 - drop), 0, () => m.Turn(0, 420, wob, () =>
            {
                m.Box(0, 200, -30, 50, 500, 50, m.C(p.Color2), .1f);
                m.Lathe(0, -30, m.C(p.Color2), 0, 470, 25, 450);
                for (int i = 0; i < 3; i++) m.Box(0, -130 + i * 90, 0, 700, 84, 34, Sculpt.Mix(m.C(p.Color1), m.C(p.Color2), i * .15f), .15f);
                foreach (int s in Sides) m.Ball(s * 300, -40, 20, 10, K);
                m.Text(Str(p), 80, 0, -40, 20, 5, m.C(p.Color3), 640);
            }));
            m.Disc(0, 430, 0, 120, 30, 10, m.C(0x6D4C41, .6f));
        });
        Q(d, typeof(HangingSignParameter), (p, m, t, v, v2) => // 吊り下げ看板: 腕木と2本の鎖、ぶら下がって揺れる板
        {
            float ang = MathF.Sin(m.Now * 4.4f) * MathF.Exp(-m.Now * .65f) * v * .62f, w = 720, h = 300;
            m.Box(0, -360, 0, w + 200, 30, 40, m.C(0x3E2723), .2f);
            m.Turn(0, -345, ang, () =>
            {
                foreach (int s in Sides) for (int i = 0; i < 6; i++) m.Torus(s * w * .35f, -330 + i * 30, 0, 14, 4, m.C(p.Color2));
                m.Box(0, -100, 0, w, h, 40, m.C(p.Color1), .12f);
                m.Box(0, -100, 22, w - 50, h - 50, 6, m.C(Sculpt.Dark(m.C(p.Color1), .75f), 1), .1f);
                m.Text(Str(p), 110, 0, -100, 26, 6, m.C(p.Color3), w - 80);
            });
        });
        Q(d, typeof(PressButton3DParameter), (p, m, t, v, v2) => // 押せる立体ボタン: 台座と、沈む丸いボタン
        {
            float at = v * 5, dd = m.Now - at, press = dd < 0 ? 0 : dd < .08f ? dd / .08f : dd < .28f ? 1 : 1 - Math.Clamp((dd - .28f) / .1f, 0, 1);
            m.Disc(0, 0, -40, 300, 300, 80, m.C(p.Color2), .3f);
            m.Disc(0, 0, 30 - press * 50, 230, 230, 90, m.C(p.Color1), .45f);
            m.Text(Str(p), 80, 0, 0, 78 - press * 50, 6, m.C(p.Color3), 360);
        });
        Q(d, typeof(ToggleSliderUiParameter), (p, m, t, v, v2) => // スイッチとスライダー: 濃い板に並ぶ、丸いつまみのスイッチ
        {
            var r = Lines(p); if (r.Count == 0) return; float rh = 124, h = rh * r.Count + 62, w = 900, at = v * 5;
            m.Box(0, 0, -20, w, h, 30, m.C(p.Color2), .12f);
            for (int i = 0; i < r.Count; i++)
            {
                float y = -h / 2 + 31 + rh * (i + .5f), k = Ease(Math.Clamp((m.Now - at - i * .25f) / .25f, 0, 1));
                m.Text(r[i], 48, -170, y, 0, 3, m.C(p.Color3), 500);
                if (i % 2 == 0) { m.Box(300, y, 0, 180, 80, 30, Sculpt.Mix(m.C(0x55606E), m.C(p.Color1), k), .5f); m.Ball(250 + 100 * k, y, 22, 34, W); }
                else { m.Box(300, y, 0, 220, 12, 12, m.C(0x55606E), .5f); m.Box(190 + 110 * k, y, 0, 220 * k / 1, 12, 14, m.C(p.Color1), .5f); m.Ball(190 + 220 * k, y, 10, 26, W); }
            }
        });
    }

    static float[] RoundRectFlat(float cx, float cy, float w, float h, float r)
    {
        var l = new List<float>();
        for (int c = 0; c < 4; c++)
        {
            float ox = c == 0 || c == 3 ? cx + w / 2 - r : cx - w / 2 + r, oy = c < 2 ? cy + h / 2 - r : cy - h / 2 + r;
            for (int i = 0; i <= 4; i++) { float an = (c * 90 + i * 22.5f) * MathF.PI / 180; l.Add(ox + MathF.Cos(an) * r); l.Add(oy + MathF.Sin(an) * r); }
        }
        return [.. l];
    }
}
