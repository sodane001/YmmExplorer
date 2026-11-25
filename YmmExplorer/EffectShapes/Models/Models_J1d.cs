using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 207番(画面の切り替え 20)・208番(場所の背景 20)
//  切り替えは、アイテムの長さを0〜1で進める(2Dと同じく時間で進む)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>切り替えの進み(0→1、約1.2秒)</summary>
    static float Tr(Sculpt m) => Ease(Math.Clamp(m.Now / 1.2f, 0, 1));

    static partial void RegisterJ1d(Dictionary<Type, QuickModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.12f, .13f, .16f, 1);
        // ---------------------------------------------------------------- 207 画面の切り替え
        QS(d, typeof(DoubleDoorParameter), (p, m, t, v, v2) => // 両開きの扉: 蝶番で奥へ開く、取っ手の付いた2枚の厚い扉
        {
            float k = Tr(m);
            foreach (int s in Sides) m.Yaw(s * m.W / 2, 0, 0, s * k * 1.6f, () => { m.Box(s * m.W / 4, 0, 0, m.W / 2, m.H, 40, m.C(p.Color1), .02f); m.Box(s * 60, 0, 30, 20, 160, 20, m.C(0xC9A227), .4f); });
        });
        QS(d, typeof(RollingShutterParameter), (p, m, t, v, v2) => // シャッター: 上から下りてくる、横の段のある金属の板
        {
            float k = Tr(m), hh = m.H * k;
            for (int i = 0; i < 20; i++) { float y = -m.H / 2 + hh - (i + .5f) * m.H / 20; if (y < -m.H / 2) break; m.Box(0, y, 0, m.W, m.H / 20 - 4, 30 + (i % 2) * 10, m.C(p.Color1), .1f); }
            m.Box(0, -m.H / 2 + hh, 20, m.W, 30, 50, m.C(0x607D8B), .2f);
        });
        QS(d, typeof(CubeTurnParameter), (p, m, t, v, v2) => // キューブ回転: 画面が大きな立方体の面として、横に回って次の面へ
        {
            float k = Tr(m), half = m.W / 2;
            m.Yaw(0, 0, -half, -k * MathF.PI / 2, () =>
            {
                m.Box(0, 0, 0, m.W, m.H, 10, m.C(p.Color1), .01f);
                m.Box(half, 0, -half, 10, m.H, m.W, m.C(0x333333), .01f);
            });
        });
        QS(d, typeof(PaperBallTossParameter), (p, m, t, v, v2) => // 紙が丸まって飛ぶ: 画面の紙が丸い玉になり、放物線で飛んでいく
        {
            float k = Tr(m);
            if (k < .5f) { float s = 1 - k * 1.6f; m.Box(0, 0, 0, m.W * s, m.H * s, 4, W, .3f + k); }
            else { float f = (k - .5f) * 2; m.Ball(m.W * .6f * f, -m.H * .5f * MathF.Sin(f * MathF.PI) * .8f, -800 * f, 150, W); }
        });
        QS(d, typeof(PuzzleBreakParameter), (p, m, t, v, v2) => // パズルが外れる: 画面のピースがばらけて、回りながら落ちる
        {
            float k = Tr(m); int cols = 6, rows = 4; float cw = m.W / cols, ch = m.H / rows;
            for (int y = 0; y < rows; y++) for (int x = 0; x < cols; x++) { int i = x + y * cols; float g = Math.Clamp(k * 1.5f - DrawKit.H(p.Seed, i) * .5f, 0, 1), px = -m.W / 2 + cw * (x + .5f), py = -m.H / 2 + ch * (y + .5f); m.Move(0, m.H * g * g, 300 * g, () => m.Turn(px, py, g * 3 * (DrawKit.H(p.Seed, i, 1) - .5f), () => m.Box(px, py, 0, cw - 6, ch - 6, 20, m.C(p.Color1), .05f))); }
        });
        QS(d, typeof(ScreenMeltParameter), (p, m, t, v, v2) => // 画面が溶ける: 色の板が、しずくを垂らしながら下へ流れ落ちる
        {
            float k = Tr(m);
            for (int i = 0; i < 24; i++) { float x = -m.W / 2 + m.W * (i + .5f) / 24, drop = m.H * k * (1 + DrawKit.H(p.Seed, i) * .8f); m.Box(x, -m.H / 2 + drop / 2 - m.H * .02f, 0, m.W / 24 + 2, MathF.Max(1, m.H - drop), 20, m.C(p.Color1), .2f); m.Ball(x, -m.H / 2 + m.H - drop + m.H * .1f, 0, m.W / 48, m.C(p.Color1)); }
        });
        QS(d, typeof(MirrorSymmetryParameter), (p, m, t, v, v2) => // 鏡のシンメトリー: まん中の鏡の板と、左右に映る形
        {
            m.Box(0, 0, 0, 10, m.H, 60, m.C(0xCFD8DC), .3f);
            foreach (int s in Sides) for (int i = 0; i < 5; i++) { float y = (i - 2) * 160, x = s * (120 + 80 * MathF.Sin(m.Now + i)); m.Ball(x, y, 0, 50, Pal(m, i)); }
        });
        QS(d, typeof(ClockWipeParameter), (p, m, t, v, v2) => // 時計の針ワイプ: 回る針が、扇の板で画面を覆っていく
        {
            float k = Tr(m), R = MathF.Sqrt(m.W * m.W + m.H * m.H) / 2;
            if (k > .01f) m.Slab(0, 30, m.C(p.Color1), PiePts(0, 0, R, -MathF.PI / 2, -MathF.PI / 2 + MathF.Tau * k, 40));
            float ag = -MathF.PI / 2 + MathF.Tau * k; m.Limb(0, 0, 20, MathF.Cos(ag) * R, MathF.Sin(ag) * R, 20, 12, 12, W);
        });
        QS(d, typeof(LightningFlashParameter), (p, m, t, v, v2) => // 落雷のフラッシュ: 画面を白く照らす光の板と、奥へ落ちる稲妻
        {
            float f = m.Now % 1.5f, fl = f < .1f || (f > .2f && f < .25f) ? 1 : 0;
            m.Box(0, 0, 100, m.W, m.H, 4, m.C(0xFFFFFF, fl * .8f), .01f);
            if (fl > 0) Puff(m, -800, 60, m.C(p.Color1), 0, -m.H / 2, -120, -50, 40, -50, -100, m.H / 2, 120, 30, -40, 30, 80, -m.H / 2);
        });
        QS(d, typeof(FocusPullParameter), (p, m, t, v, v2) => // ピント合わせ: 前後に並ぶ玉のうち、ピントの合う距離が奥から手前へ動く
        {
            float fz = -800 + 900 * Tr(m);
            for (int i = 0; i < 20; i++) { float z = -900 + DrawKit.H(p.Seed, i) * 1000, blur = MathF.Min(1, MathF.Abs(z - fz) / 400), x = (DrawKit.H(p.Seed, i, 1) - .5f) * m.W, y = (DrawKit.H(p.Seed, i, 2) - .5f) * m.H; m.Ball(x, y, z, 40 + 40 * blur, Pal(m, i) with { W = (1 - blur * .6f) * m.Al }); }
        });
        QS(d, typeof(ScreenCrackParameter), (p, m, t, v, v2) => // 画面にひび: 割れた画面のかけらが少しずつずれる、ひびの線
        {
            float k = Tr(m); int n = 10; var c0 = new Vector2(0, 0);
            for (int i = 0; i < n; i++)
            {
                float a0 = i * MathF.Tau / n + DrawKit.H(p.Seed, i) * .3f, a1 = (i + 1) * MathF.Tau / n + DrawKit.H(p.Seed, i + 1) * .3f, R = m.W;
                var e0 = new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * R; var e1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1)) * R;
                float dz = 30 * k * DrawKit.H(p.Seed, i, 2);
                m.Tri3D(new(c0, dz), new(e0, dz), new(e1, dz), m.C(0xFFFFFF, .06f));
                m.Limb(c0.X, c0.Y, dz + 2, e0.X, e0.Y, dz + 2, 2, 2, m.C(p.Color1, k));
            }
        });
        QS(d, typeof(CenterSuckParameter), (p, m, t, v, v2) => // 中心へ吸いこむ: 画面がうずを巻いて縮み、奥の点へ消える
        {
            float k = Tr(m), s = 1 - k;
            m.Move(0, 0, -1500 * k, () => m.Turn(0, 0, k * 6, () => m.Box(0, 0, 0, m.W * s + 1, m.H * s + 1, 10, m.C(p.Color1), .02f)));
        });
        QS(d, typeof(ColorRippleParameter), (p, m, t, v, v2) => // 色の波: 中心から広がる色の輪の板が、波打って重なる
        {
            float k = Tr(m);
            for (int i = 0; i < 6; i++) { float r = (MathF.Max(m.W, m.H) * (k * 1.5f - i * .1f)); if (r <= 0) continue; m.Disc(0, 0, -i * 20, r, r, 20 + 30 * MathF.Sin(m.Now * 4 + i), Pal(m, i), .3f); }
        });
        QS(d, typeof(PhoneSwipeParameter), (p, m, t, v, v2) => // スマホのスワイプ: 画面の板が指と一緒に横へ滑り、次の板が来る
        {
            float k = Tr(m);
            m.Box(-m.W * k, 0, 0, m.W, m.H, 20, m.C(p.Color1), .03f); m.Box(m.W * (1 - k), 0, 0, m.W, m.H, 20, m.C(0x37474F), .03f);
            m.Ball(m.W * .2f - m.W * .5f * k, m.H * .2f, 80, 50, 70, 40, m.C(0xFFD9B3, .8f));
        });
        Q(d, typeof(BubblePopRevealParameter), (p, m, t, v, v2) => // ぽよんと弾ける: ふくらんだ透けた泡が弾けて、中の文字が出る
        {
            float k = In(m, 0, .8f);
            if (k < .8f) { float s = 1 + .1f * MathF.Sin(m.Now * 12); m.Ball(0, 0, 0, 300 * s, 280 / s, 280, m.C(p.Color1, .35f)); m.Ball(-100, -100, 220, 40, 30, 10, m.C(0xFFFFFF, .7f)); }
            else for (int i = 0; i < 12; i++) { float ag = i * MathF.Tau / 12, f = (k - .8f) * 5; m.Ball(MathF.Cos(ag) * 300 * (1 + f), MathF.Sin(ag) * 300 * (1 + f), 0, 14, m.C(p.Color1, 1 - f)); }
            m.Text(Str(p), 120 * Ease(MathF.Max(0, (k - .7f) * 3.3f)), 0, 0, 40, 20, m.C(p.Color2));
        });
        QS(d, typeof(WordFloodParameter), (p, m, t, v, v2) => // 文字で埋まる: 奥から飛んでくる文字のブロックが、画面を埋めていく
        {
            string s = Str(p).Length > 0 ? Str(p) : "ABC"; float k = Tr(m);
            for (int i = 0; i < 60; i++) { float g = Math.Clamp(k * 1.5f - DrawKit.H(p.Seed, i) * .5f, 0, 1); if (g <= 0) continue; float x = (DrawKit.H(p.Seed, i, 1) - .5f) * m.W, y = (DrawKit.H(p.Seed, i, 2) - .5f) * m.H; m.Text(s[i % s.Length].ToString(), 120, x, y, -1200 * (1 - g), 20, Pal(m, i)); }
        });
        QS(d, typeof(FireSpreadParameter), (p, m, t, v, v2) => // 炎が燃え広がる: 下から上へ広がる、ゆらめく炎の玉の山
        {
            float k = Tr(m);
            for (int i = 0; i < 60; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W, f = ((m.Now * 1.5f + DrawKit.H(p.Seed, i, 1)) % 1), y = m.H / 2 - m.H * 1.2f * k * f; m.Ball(x, y, (DrawKit.H(p.Seed, i, 2) - .5f) * 300, 60 + 120 * (1 - f), Vector4.Lerp(m.C(0xFFEB3B), m.C(p.Color1), f) with { W = (1 - f) * .8f * m.Al }); }
        });
        QS(d, typeof(WaterRiseFillParameter), (p, m, t, v, v2) => // 水が満ちる: 下から上がってくる波打つ水面と、上る泡
        {
            float k = Tr(m), lv = m.H / 2 - m.H * 1.1f * k;
            var surf = Enumerable.Range(0, 31).Select(i => { float x = -m.W / 2 + m.W * i / 30f; return new Vector2(x, lv + MathF.Sin(x * .01f + m.Now * 3) * 20); }).ToList();
            var poly = new List<Vector2>(surf) { new(m.W / 2, m.H / 2 + 10), new(-m.W / 2, m.H / 2 + 10) }; poly.Reverse();
            m.Slab(poly, 0, 300, m.C(p.Color1, .8f));
            for (int i = 0; i < 20; i++) { float f = ((m.Now * .6f + i / 20f) % 1), y = m.H / 2 - (m.H / 2 - lv) * f; m.Ball((DrawKit.H(p.Seed, i) - .5f) * m.W, y, 160, 10, m.C(0xFFFFFF, .5f)); }
        });
        QS(d, typeof(PaintSplashParameter), (p, m, t, v, v2) => // 絵の具のスプラッシュ: 飛び散ってふくらむ色のしぶきの玉
        {
            float k = Tr(m);
            for (int i = 0; i < 30; i++) { float ag = DrawKit.H(p.Seed, i) * MathF.Tau, r = MathF.Max(m.W, m.H) * .6f * k * DrawKit.H(p.Seed, i, 1), s = 60 + 200 * DrawKit.H(p.Seed, i, 2) * k; m.Ball(MathF.Cos(ag) * r, MathF.Sin(ag) * r, i * 4, s, s * .9f, 30, Pal(m, i % 3)); }
        });
        QS(d, typeof(LightTunnelParameter), (p, m, t, v, v2) => // 光のトンネル: 手前へ流れてくる光の輪と線、出口の白い光
        {
            for (int i = 0; i < 14; i++) { float z = -2000 + ((m.Now * 800 + i * 160) % 2200); m.Torus(0, 0, z, 600, 6, m.C(p.Color1, .7f)); }
            for (int i = 0; i < 24; i++) { float ag = i * MathF.Tau / 24, z0 = -2000 + ((m.Now * 1500 + i * 90) % 2200); m.Limb(MathF.Cos(ag) * 560, MathF.Sin(ag) * 560, z0, MathF.Cos(ag) * 560, MathF.Sin(ag) * 560, z0 + 200, 3, 3, W); }
            m.Ball(0, 0, -2000, 300 * Tr(m) + 50, W);
        });

        // ---------------------------------------------------------------- 208 場所の背景
        QS(d, typeof(LibraryBackParameter), (p, m, t, v, v2) => // 図書館: 奥へ並ぶ本棚の列、長い机と読書灯
        {
            Room(m, m.C(0x6D4C41), m.C(0x8A6A4A), 2000);
            for (int i = 0; i < 5; i++) foreach (int s in Sides) { float z = -1800 + i * 380; m.Box(s * m.W * .45f, 0, z, 80, m.H * .9f, 300, m.C(0x5D4037), .02f); for (int r = 0; r < 5; r++) for (int b = 0; b < 8; b++) m.Box(s * (m.W * .45f - 45), -m.H * .4f + r * m.H * .18f, z - 130 + b * 36, 10, m.H * .14f, 30, Pal(m, b + r + i), .05f); }
            m.Box(0, m.H * .3f, -600, 600, 30, 900, m.C(0xA9743B), .05f);
            m.Ball(0, m.H * .3f - 120, -600, 50, 30, 50, m.C(0x43A047)); m.Ball(0, m.H * .3f - 80, -600, 70, m.C(0xFFF3B0, .15f));
        });
        QS(d, typeof(GymBackParameter), (p, m, t, v, v2) => // 体育館: 板の床とライン、奥の舞台、バスケットのゴール
        {
            Room(m, m.C(0xE8DCC0), m.C(0xD9A866), 2200, m.C(0x9AA3AD));
            for (int i = -2; i <= 2; i++) m.Box(i * 300, m.H * .55f - 1, -800, 6, 2, 2400, W, .5f);
            m.Box(0, m.H * .3f, -2150, m.W, m.H * .5f, 100, m.C(0x8A1F1F), .02f);
            foreach (int s in Sides) { m.Box(s * m.W * .4f, -m.H * .1f, -1200, 200, 140, 10, W, .05f); m.Torus(s * m.W * .4f, -m.H * .02f, -1100, 40, 4, m.C(0xFF7043)); }
        });
        QS(d, typeof(SchoolRooftopParameter), (p, m, t, v, v2) => // 学校の屋上: 青空と雲、コンクリートの床、まわりの金網のフェンス
        {
            SkyWall(m, m.C(0x4FB8F2), m.C(0xBFE6FF), -2000);
            for (int i = 0; i < 6; i++) m.Ball((DrawKit.H(p.Seed, i) - .5f) * m.W * 2, -m.H * .3f, -1800, 200, 80, 100, W);
            Ground(m, m.H * .4f, m.C(0xB0B0B0), 400, -1500);
            for (int i = -10; i <= 10; i++) m.Cyl(i * 150, m.H * .4f - 120, -1000, 6, 240, m.C(0x7A8590), .2f);
            m.Box(0, m.H * .4f - 240, -1000, 3000, 8, 8, m.C(0x7A8590), .3f);
            for (int i = 0; i < 20; i++) m.Limb(-1500 + i * 150, m.H * .4f, -1000, -1350 + i * 150, m.H * .4f - 240, -1000, 1, 1, m.C(0x7A8590, .6f));
        });
        QS(d, typeof(SchoolHillRoadParameter), (p, m, t, v, v2) => // 通学路の坂道: 奥へのぼる道、両側の家と電柱、桜の木
        {
            SkyWall(m, m.C(0x8FD3FF), m.C(0xE8F6FF), -2000);
            m.Quad3D(new(-300, m.H / 2, 400), new(300, m.H / 2, 400), new(200, -m.H * .1f, -1800), new(-200, -m.H * .1f, -1800), m.C(0x808890));
            for (int i = 0; i < 5; i++) { float z = 200 - i * 450, y = m.H / 2 - (m.H * .6f) * i / 5; foreach (int s in Sides) { m.Box(s * 700, y - 150, z, 400, 300, 300, Pal(m, i + s), .03f); m.Cyl(s * 380, y - 300, z + 100, 8, 600, m.C(0x8A8070), .2f); m.Ball(s * 500, y - 400, z, 140, m.C(0xFFC1D6, .9f)); } }
        });
        QS(d, typeof(ShoppingArcadeParameter), (p, m, t, v, v2) => // 商店街: 半円の屋根のアーケード、両側の店と看板、ぶら下がる飾り
        {
            m.Quad3D(new(-600, m.H * .4f, 400), new(600, m.H * .4f, 400), new(600, m.H * .4f, -2400), new(-600, m.H * .4f, -2400), m.C(0xB0A898));
            for (int i = 0; i < 12; i++) { float z = 200 - i * 220; m.Tube(ArcPts3(0, m.H * .4f - 700, 650, MathF.PI, MathF.Tau, z, 16), 6, m.C(0xCFD8DC)); }
            for (int i = 0; i < 7; i++) { float z = 100 - i * 350; foreach (int s in Sides) { m.Box(s * 700, m.H * .2f, z, 200, 500, 320, Pal(m, i + s), .03f); m.Box(s * 590, -m.H * .05f, z, 10, 80, 260, W, .2f); } m.Ball(0, -m.H * .2f, z, 30, 40, 30, Pal(m, i)); }
        });
        QS(d, typeof(AmusementParkParameter), (p, m, t, v, v2) => // 遊園地: 回る観覧車(輪とゴンドラ)と、メリーゴーランドの屋根
        {
            SkyWall(m, m.C(0x8FD3FF), m.C(0xFFE0F0), -2000);
            Ground(m, m.H * .4f, m.C(0x7CB342));
            m.Turn(-m.W * .2f, -m.H * .1f, m.Now * .2f, () => { m.Torus(-m.W * .2f, -m.H * .1f, -1200, 500, 10, m.C(0xE0E0E0)); for (int i = 0; i < 12; i++) { float ag = i * MathF.PI / 6; m.Limb(-m.W * .2f, -m.H * .1f, -1200, -m.W * .2f + MathF.Cos(ag) * 500, -m.H * .1f + MathF.Sin(ag) * 500, -1200, 3, 3, m.C(0xE0E0E0)); m.Box(-m.W * .2f + MathF.Cos(ag) * 500, -m.H * .1f + MathF.Sin(ag) * 500 + 50, -1200, 60, 70, 60, Pal(m, i), .3f); } });
            m.Lathe(m.W * .3f, -600, m.C(0xE53935), 0, m.H * .05f, 350, m.H * .2f, 0, m.H * .2f);
            m.Yaw(m.W * .3f, 0, -600, m.Now * .5f, () => { for (int i = 0; i < 8; i++) { float ag = i * MathF.PI / 4; m.Ball(m.W * .3f + MathF.Cos(ag) * 250, m.H * .3f, -600 + MathF.Sin(ag) * 250, 40, Pal(m, i)); } });
        });
        QS(d, typeof(AirportLobbyParameter), (p, m, t, v, v2) => // 空港のロビー: 大きな窓と外の飛行機、出発の案内板、並ぶ椅子
        {
            Room(m, m.C(0xE8ECEF), m.C(0xC8CED4), 1800);
            m.Box(0, -m.H * .1f, -1790, m.W * 1.1f, m.H * .6f, 10, m.C(0xBFE6FF), .02f);
            m.Ball(m.W * .2f, -m.H * .1f, -2200, 400, 50, 50, W);
            m.Box(-m.W * .3f, -m.H * .35f, -900, 500, 200, 20, m.C(0x1F2430), .05f);
            for (int i = 0; i < 4; i++) m.Text($"{10 + i}:{(i * 15) % 60:00}  便", 30, -m.W * .3f, -m.H * .42f + i * 40, -888, 2, m.C(0xFFB300));
            for (int r = 0; r < 2; r++) for (int i = 0; i < 6; i++) m.Box(-300 + i * 120, m.H * .45f, -300 - r * 300, 100, 30, 100, m.C(0x1E88E5), .3f);
        });
        QS(d, typeof(TrainInteriorParameter), (p, m, t, v, v2) => // 電車の車内: 奥へ続く両側の長い座席、つり革、窓
        {
            Room(m, m.C(0xE0E4E8), m.C(0x9AA3AD), 2200);
            foreach (int s in Sides) { m.Box(s * m.W * .5f, m.H * .35f, -900, 200, 80, 2200, m.C(0x2E7D6B), .1f); for (int i = 0; i < 6; i++) m.Box(s * m.W * .59f, -m.H * .15f, -1800 + i * 360, 6, m.H * .35f, 280, m.C(0xBFE6FF), .02f); }
            for (int i = 0; i < 10; i++) foreach (int s in Sides) { float z = -1800 + i * 200, sw = MathF.Sin(m.Now * 2 + i) * .1f; m.Turn(s * m.W * .3f, -m.H * .5f, sw, () => { m.Limb(s * m.W * .3f, -m.H * .5f, z, s * m.W * .3f, -m.H * .3f, z, 2, 2, W); m.Torus(s * m.W * .3f, -m.H * .27f, z, 20, 4, W); }); }
        });
        QS(d, typeof(CountryBusStopParameter), (p, m, t, v, v2) => // バス停と田舎道: 田んぼの間の道、標識の柱と小さな屋根、遠くの山
        {
            SkyWall(m, m.C(0x8FD3FF), m.C(0xE8F6FF), -2500);
            m.Lathe(0, -2300, m.C(0x5A7A5A), 0, -m.H * .1f, m.W * .8f, m.H * .2f, 0, m.H * .2f);
            Ground(m, m.H * .3f, m.C(0x9CCC65), 400, -2200);
            m.Quad3D(new(-150, m.H * .3f - 1, 400), new(150, m.H * .3f - 1, 400), new(80, m.H * .3f - 1, -2200), new(-80, m.H * .3f - 1, -2200), m.C(0x9E9E9E));
            m.Cyl(300, m.H * .3f - 150, -200, 8, 300, m.C(0x9E9E9E), .2f); m.Disc(300, m.H * .3f - 320, -200, 50, 50, 10, m.C(0x1E88E5), .3f);
            m.Box(-400, m.H * .3f - 150, -400, 300, 300, 200, m.C(0x8A5A32), .05f); m.Slab(0, 260, m.C(0x6D4C41), -580, m.H * .3f - 300, -220, m.H * .3f - 300, -400, m.H * .3f - 420);
        });
        QS(d, typeof(KitchenBackParameter), (p, m, t, v, v2) => // 台所: 流しとコンロの台、つり戸棚、湯気の立つ鍋
        {
            Room(m, m.C(0xF5F0E6), m.C(0xC8A878), 1400);
            m.Box(0, m.H * .3f, -1200, m.W * 1.1f, m.H * .4f, 300, m.C(0xE0E0E0), .05f);
            m.Box(0, -m.H * .3f, -1300, m.W * 1.1f, m.H * .25f, 200, m.C(0xD9B07E), .05f);
            m.Lathe(200, -1100, m.C(0x9E9E9E), 0, m.H * .1f, 120, m.H * .1f, 120, -m.H * .02f, 0, -m.H * .02f);
            for (int i = 0; i < 3; i++) { float f = ((m.Now * .5f + i / 3f) % 1); m.Ball(200 + MathF.Sin(f * 6) * 20, -m.H * .05f - f * 200, -1100, 30 + 40 * f, m.C(0xFFFFFF, .4f * (1 - f))); }
        });
        QS(d, typeof(WashitsuBackParameter), (p, m, t, v, v2) => // 和室: 畳、ふすま、床の間の掛け軸と生け花、ちゃぶ台
        {
            Room(m, m.C(0xE8DCC0), m.C(0xB8C080), 1400, m.C(0x8A6A40));
            for (int i = 0; i < 3; i++) m.Box(-m.W * .25f + i * m.W * .25f, 0, -1390, m.W * .24f, m.H * .8f, 10, m.C(0xF0E8D8), .02f);
            m.Box(m.W * .4f, -m.H * .1f, -1380, 120, 380, 6, m.C(0xF5ECD7), .02f);
            m.Cyl(0, m.H * .38f, -400, 260, 20, m.C(0x8A5A32), .1f); for (int i = 0; i < 4; i++) { float ag = i * MathF.PI / 2 + .78f; m.Cyl(MathF.Cos(ag) * 180, m.H * .45f, -400 + MathF.Sin(ag) * 180, 12, 120, m.C(0x6D4C41), .2f); }
        });
        QS(d, typeof(NightBedroomParameter), (p, m, t, v, v2) => // 夜の寝室: 暗い部屋、月明かりの窓、ふとんの盛り上がり、小さな灯り
        {
            Room(m, m.C(0x1A2040), m.C(0x2A3050), 1400);
            m.Box(m.W * .3f, -m.H * .15f, -1390, 400, 400, 10, m.C(0x3A5A9A), .02f); m.Ball(m.W * .35f, -m.H * .25f, -1385, 60, m.C(0xFFF3C0));
            m.Box(-m.W * .1f, m.H * .4f, -700, 700, 120, 900, m.C(0x5A6A9A), .3f); m.Ball(-m.W * .1f, m.H * .3f, -700, 300, 80, 380, m.C(0x7080B0));
            m.Ball(m.W * .4f, m.H * .35f, -400, 40, m.C(0xFFB74D)); m.Ball(m.W * .4f, m.H * .35f, -400, 120, m.C(0xFFB74D, .15f));
        });
        QS(d, typeof(RamenCounterParameter), (p, m, t, v, v2) => // ラーメン屋のカウンター: 木のカウンター、湯気の立つどんぶり、奥のちょうちん
        {
            Room(m, m.C(0x6D4C41), m.C(0x4A3420), 1200);
            m.Box(0, m.H * .25f, -200, m.W * 1.1f, 40, 300, m.C(0xC8A070), .05f);
            m.Lathe(0, -200, m.C(0xE53935), 0, m.H * .23f, 70, m.H * .23f, 140, m.H * .12f, 0, m.H * .12f);
            m.Ball(0, m.H * .12f, -200, 135, 10, 135, m.C(0xE8C080));
            for (int i = 0; i < 3; i++) { float f = ((m.Now * .5f + i / 3f) % 1); m.Ball(MathF.Sin(f * 6 + i) * 30, m.H * .1f - f * 250, -200, 30 + 40 * f, m.C(0xFFFFFF, .4f * (1 - f))); }
            for (int i = 0; i < 4; i++) m.Ball(-m.W * .3f + i * m.W * .2f, -m.H * .3f, -1100, 50, 70, 50, m.C(0xE53935));
        });
        QS(d, typeof(ConvenienceStoreParameter), (p, m, t, v, v2) => // コンビニの店内: 明るい部屋、奥へ並ぶ棚と商品、冷蔵ケース
        {
            Room(m, m.C(0xF5F5F5), m.C(0xE0E0E0), 1800, W);
            for (int i = 0; i < 3; i++) { float x = (i - 1) * 500; m.Box(x, m.H * .2f, -900, 160, m.H * .5f, 1200, m.C(0xCFD8DC), .05f); for (int r = 0; r < 3; r++) for (int b = 0; b < 10; b++) foreach (int s in Sides) m.Box(x + s * 85, -m.H * .02f + r * m.H * .15f, -1450 + b * 110, 10, 70, 80, Pal(m, b + r + i), .2f); }
            m.Box(0, 0, -1790, m.W * .9f, m.H * .7f, 20, m.C(0xBFE6FF, .6f), .03f);
        });
        QS(d, typeof(StadiumBackParameter), (p, m, t, v, v2) => // スタジアム: 緑のフィールドと白線、まわりの段の観客席、照明の塔
        {
            SkyWall(m, m.C(0x0A1030), m.C(0x1A2050), -3000);
            Ground(m, m.H * .4f, m.C(0x43A047), 400, -2500);
            m.Box(0, m.H * .4f - 1, -1000, 1200, 2, 8, W, .3f); m.Pitch(0, m.H * .4f, -1000, MathF.PI / 2, () => m.Torus(0, m.H * .4f, -1000, 200, 3, W));
            for (int i = 0; i < 6; i++) m.Box(0, m.H * .4f - 80 - i * 60, -2400 + i * 0 - i * 60, m.W * 3, 60, 60, Pal(m, i) with { W = .6f * m.Al }, .05f);
            foreach (int s in Sides) { m.Cyl(s * m.W * .8f, -m.H * .1f, -1800, 20, m.H, m.C(0x9E9E9E), .2f); m.Box(s * m.W * .8f, -m.H * .6f, -1800, 200, 100, 20, m.C(0xFFFFE0), .2f); }
        });
        QS(d, typeof(CourtroomParameter), (p, m, t, v, v2) => // 法廷: 奥の高い裁判官席、左右の席、証言台、木の柵
        {
            Room(m, m.C(0xD9C3A0), m.C(0x8A5A32), 1600);
            m.Box(0, m.H * .1f, -1400, m.W * .6f, m.H * .5f, 200, m.C(0x6D4C41), .05f);
            foreach (int s in Sides) m.Box(s * m.W * .4f, m.H * .3f, -800, 300, m.H * .3f, 500, m.C(0x7A5235), .05f);
            m.Box(0, m.H * .35f, -500, 200, m.H * .2f, 120, m.C(0x7A5235), .05f);
            for (int i = -6; i <= 6; i++) m.Cyl(i * 80, m.H * .45f, -100, 8, 160, m.C(0x8A5A32), .2f);
        });
        QS(d, typeof(ThroneRoomParameter), (p, m, t, v, v2) => // 玉座の間: 赤いじゅうたんの道、並ぶ柱、奥の段と金の玉座
        {
            Room(m, m.C(0x5A4A6A), m.C(0x6A5A50), 2200);
            m.Box(0, m.H * .55f - 2, -900, 400, 4, 2400, m.C(0xB71C1C), .02f);
            for (int i = 0; i < 6; i++) foreach (int s in Sides) m.Cyl(s * m.W * .35f, 0, -1900 + i * 380, 50, m.H * 1.1f, m.C(0xD9C3A0), .05f);
            for (int i = 0; i < 3; i++) m.Box(0, m.H * .5f - i * 40, -2000 + i * 60, 900 - i * 200, 40, 200, m.C(0xD9C3A0), .05f);
            m.Box(0, m.H * .2f, -2050, 220, 400, 100, m.C(0xC9A227), .1f); m.Box(0, m.H * .3f, -2000, 240, 40, 160, m.C(0xB71C1C), .2f);
        });
        QS(d, typeof(LimestoneCaveParameter), (p, m, t, v, v2) => // 鍾乳洞: 天井から垂れる鍾乳石と、床から伸びる石筍、水たまりの光
        {
            m.Box(0, 0, -2000, m.W * 2, m.H * 2, 10, m.C(0x1A2030), .01f);
            Ground(m, m.H * .45f, m.C(0x3A4050), 400, -2000);
            for (int i = 0; i < 24; i++) { float x = (DrawKit.H(p.Seed, i) - .5f) * m.W * 1.6f, z = 200 - DrawKit.H(p.Seed, i, 1) * 2000, l = 150 + 350 * DrawKit.H(p.Seed, i, 2); var c = m.C(0xD9D0C0); m.Lathe(x, z, c, 0, -m.H * .55f + l, 40 + 40 * DrawKit.H(p.Seed, i, 3), -m.H * .55f, 0, -m.H * .55f); if (i % 2 == 0) m.Lathe(x + 50, z, c, 0, m.H * .45f - l * .6f, 50, m.H * .45f, 0, m.H * .45f); }
            m.Ball(0, m.H * .45f, -800, 400, 6, 200, m.C(0x4FC3F7, .4f));
        });
        QS(d, typeof(SnowOnsenParameter), (p, m, t, v, v2) => // 雪国の露天風呂: 岩に囲まれた湯船、立つ湯気、雪の積もった木と降る雪
        {
            SkyWall(m, m.C(0x3A4A70), m.C(0x8090B0), -2000);
            Ground(m, m.H * .3f, W, 400, -2000);
            for (int i = 0; i < 16; i++) { float ag = i * MathF.Tau / 16; m.Ball(MathF.Cos(ag) * 520, m.H * .3f - 30, -600 + MathF.Sin(ag) * 260, 70, 50, 60, m.C(0x707070)); }
            m.Ball(0, m.H * .3f - 10, -600, 480, 6, 230, m.C(0x80CBC4, .8f));
            for (int i = 0; i < 6; i++) { float f = ((m.Now * .3f + i / 6f) % 1); m.Ball((i - 2.5f) * 120, m.H * .3f - 40 - f * 300, -600, 60 + 60 * f, m.C(0xFFFFFF, .35f * (1 - f))); }
            foreach (int s in Sides) { m.Lathe(s * m.W * .5f, -1200, m.C(0x2E5A3A), 0, -m.H * .3f, 200, m.H * .3f, 0, m.H * .3f); m.Lathe(s * m.W * .5f, -1190, W, 0, -m.H * .32f, 80, -m.H * .1f, 0, -m.H * .1f); }
            for (int i = 0; i < 40; i++) { float y = ((DrawKit.H(p.Seed, i) * m.H + m.Now * 50) % m.H) - m.H / 2; m.Ball((DrawKit.H(p.Seed, i, 1) - .5f) * m.W, y, -DrawKit.H(p.Seed, i, 2) * 1500, 5, W); }
        });
        QS(d, typeof(HarborLighthouseParameter), (p, m, t, v, v2) => // 港の灯台: 夕方の海、防波堤の先に立つ灯台と回る光、浮かぶ船
        {
            SkyWall(m, m.C(0xFF8A65), m.C(0x4A5A8A), -2500);
            Ground(m, m.H * .25f, m.C(0x1E4A7A, .95f), 400, -2500);
            m.Box(m.W * .2f, m.H * .23f, -1000, 1200, 40, 160, m.C(0x9E9E9E), .05f);
            m.Lathe(m.W * .5f, -1000, W, 90, m.H * .23f, 60, -m.H * .25f, 0, -m.H * .25f);
            m.Cyl(m.W * .5f, -m.H * .3f, -1000, 50, 80, m.C(0xFFE27A), .2f);
            float ag = m.Now; m.Limb(m.W * .5f, -m.H * .3f, -1000, m.W * .5f + MathF.Cos(ag) * 800, -m.H * .3f, -1000 + MathF.Sin(ag) * 800, 20, 80, m.C(0xFFF3A8, .25f));
            for (int i = 0; i < 3; i++) { float x = -m.W * .4f + i * 300 + MathF.Sin(m.Now + i) * 20, z = -600 - i * 400; m.Box(x, m.H * .22f, z, 200, 50, 80, Pal(m, i), .3f); m.Limb(x, m.H * .2f, z, x, m.H * .05f, z, 3, 3, W); }
        });
    }
}
