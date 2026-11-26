using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 61〜90番(SF・漫画・UI・ゲーム)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>四角い枠(4本の棒)</summary>
    static void RectBars(Sculpt m, float x, float y, float z, float w, float h, float th, Vector4 col)
    {
        m.Box(x, y - h / 2, z, w + th, th, th, col, .3f); m.Box(x, y + h / 2, z, w + th, th, th, col, .3f);
        m.Box(x - w / 2, y, z, th, h, th, col, .3f); m.Box(x + w / 2, y, z, th, h, th, col, .3f);
    }
    /// <summary>四隅のかぎ括弧</summary>
    static void Corners(Sculpt m, float x, float y, float z, float w, float h, float len, float th, Vector4 col)
    {
        foreach (int a in Sides) foreach (int b in Sides) { float cx = x + a * w / 2, cy = y + b * h / 2; m.Limb(cx, cy, z, cx - a * len, cy, z, th, th, col); m.Limb(cx, cy, z, cx, cy - b * len, z, th, th, col); }
    }

    static partial void RegisterK1c(Dictionary<Type, SceneModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.08f, .09f, .11f, 1);
        S<DataStreamParameter>(d, (p, c, m) => // データストリーム: 何本もの通り道を流れる光る四角いパケットと数字
        {
            float dir = c.F(p.Direction) * MathF.PI / 180, spd = c.F(p.Speed), ph = c.F(p.PacketHeight) * 1.5f; int lanes = Math.Clamp((int)c.F(p.Lanes), 1, 40);
            m.Turn(0, 0, dir, () =>
            {
                for (int l = 0; l < lanes; l++)
                {
                    float y = ((l + .5f) / lanes - .5f) * m.H, z = -400 + R(p.Seed, l, 1) * 500;
                    m.Box(0, y, z - 6, m.W * 1.4f, 2, 2, m.C(p.Color, .25f));
                    int n = (int)(c.F(p.Density) / 10) + 1;
                    for (int i = 0; i < n; i++) { float span = m.W * 1.6f, x = -span / 2 + Cyc(m.Now * spd * (.6f + .5f * R(p.Seed, l, 2)) + R(p.Seed, l * 31 + i, 3) * span, span), w = 40 + 120 * R(p.Seed, l * 31 + i, 4); bool acc = R(p.Seed, l * 31 + i, 5) < .15f; m.Box(x, y, z, w, ph, ph, m.C(acc ? p.AccentColor : p.Color, c.F(p.Glow) / 100), .2f); if (p.ShowNumbers && i == 0) m.Text(((int)(R(p.Seed + (int)m.Now, l) * 99999)).ToString("X5"), c.F(p.DigitHeight) * 1.5f, x, y - ph * 2, z, 2, m.C(p.Color, .8f)); }
                }
            });
        });
        S<WarpParameter>(d, (p, c, m) => // ワープ: 中心から手前へ飛んでくる星の筋(奥から手前へ伸びる光の線)
        {
            var o = PctPos(m, c.F(p.CenterX), c.F(p.CenterY)); float it = c.F(p.Intensity) / 100, sp = c.F(p.Speed) / 100;
            m.Ball(o.X, o.Y, -2000, 300 * c.F(p.CenterGlow) / 100, m.C(p.TailColor, .5f)); m.Ball(o.X, o.Y, -1990, 120, m.C(0xFFFFFF, .8f));
            Low(m, () =>
            {
                for (int i = 0; i < Math.Min((int)c.F(p.Count), 260); i++)
                {
                    float a = R(p.Seed, i) * MathF.Tau, r = 120 + R(p.Seed, i, 1) * m.W * .6f, z = -2000 + Cyc(m.Now * 1500 * sp + R(p.Seed, i, 2) * 2400, 2400), L = 100 + 900 * it;
                    m.Limb(o.X + MathF.Cos(a) * r, o.Y + MathF.Sin(a) * r, z, o.X + MathF.Cos(a) * r, o.Y + MathF.Sin(a) * r, z - L, 3, 1, m.C(p.StarColor)); m.Limb(o.X + MathF.Cos(a) * r, o.Y + MathF.Sin(a) * r, z - L * .1f, o.X + MathF.Cos(a) * r, o.Y + MathF.Sin(a) * r, z - L, 6, 2, m.C(p.TailColor, .3f));
                }
            }, 5);
            if (p.Rings) for (int k = 0; k < 5; k++) { float z = -2000 + Cyc(m.Now * 1200 * sp + k * 480, 2400); m.Torus(o.X, o.Y, z, m.W * .45f, 6, m.C(p.TailColor, .35f * it)); }
        });
        S<ShieldParameter>(d, (p, c, m) => // シールド: 六角のセルでできた半透明の球、当たると波紋が広がる
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius) * (p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.IntroTime)), 0, 1)) : 1), cs = MathF.Max(15, c.F(p.CellSize)) * 1.3f;
            if (r < 2) return;
            m.Ball(o.X, o.Y, 0, r, m.C(p.Color, .1f + .2f * c.F(p.FillAmount) / 100));
            int lat = Math.Clamp((int)(r * MathF.PI / cs / 2), 3, 14);
            float hit = p.AutoHit ? Frac(m.Now / MathF.Max(.3f, c.F(p.HitInterval))) : 1;
            for (int i = 1; i < lat; i++)
            {
                float th = i * MathF.PI / lat, rr = MathF.Sin(th) * r, y = -MathF.Cos(th) * r; int lon = Math.Max(4, (int)(rr * MathF.Tau / cs));
                for (int j = 0; j < lon; j++) { float a = (j + (i % 2) * .5f) * MathF.Tau / lon; var q = V3(o.X + MathF.Cos(a) * rr, o.Y + y, MathF.Sin(a) * rr); if (q.Z < -r * .2f) continue; float dd = Vector3.Distance(q, V3(o.X + r * .5f, o.Y - r * .3f, r * .8f)) / (r * 2), glow = MathF.Max(0, 1 - MathF.Abs(dd - hit) * 8); float sh = .3f + .3f * MathF.Sin(m.Now * 3 + i + j) * c.F(p.Shimmer) / 100; var n = Vector3.Normalize(q - V3(o.X, o.Y, 0)); m.Ball(q.X, q.Y, q.Z, cs * .35f, cs * .35f, 3, m.C(p.Color, MathF.Min(1, sh + glow))); }
            }
        });
        S<PortalParameter>(d, (p, c, m) => // ポータル: 渦巻く光の輪の門、奥へ吸い込む渦/縦に裂けた光の裂け目
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size), k = (p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.1f, c.F(p.IntroTime)), 0, 1)) : 1) * c.F(p.Open) / 100, sp = m.Now * c.F(p.Spin) / 50, br = c.F(p.Brightness) / 100;
            if (p.Kind == PortalKind.Rift)
            {
                var pts = new List<Vector2>(); for (int i = 0; i <= 12; i++) { float f = i / 12f; pts.Add(new Vector2(o.X + MathF.Sin(f * MathF.PI) * c.F(p.RiftWidth) * k + (R(p.Seed, i) - .5f) * 30, o.Y - s * 1.4f + f * s * 2.8f)); } for (int i = 12; i >= 0; i--) { float f = i / 12f; pts.Add(new Vector2(o.X - MathF.Sin(f * MathF.PI) * c.F(p.RiftWidth) * k + (R(p.Seed, i, 1) - .5f) * 30, o.Y - s * 1.4f + f * s * 2.8f)); }
                m.Slab(pts, -10, 20, m.C(0x0A0014)); m.Tube(pts.Select(q => V3(q.X, q.Y, 12)).ToList(), 8, m.C(p.Color1, br)); m.Tube(pts.Select(q => V3(q.X, q.Y, 12)).ToList(), 20, m.C(p.Color2, .3f * br));
            }
            else
            {
                m.Torus(o.X, o.Y, 0, s * k, 20 * k, m.C(p.Color1, br)); m.Torus(o.X, o.Y, 0, s * k, 40 * k, m.C(p.Color2, .25f * br));
                for (int l = 0; l < 8; l++) { float f = l / 8f; m.Turn(o.X, o.Y, sp * (1 + f) + l, () => m.Tube(ArcPts3(o.X, o.Y, s * k * (1 - f * .9f), 0, 4, -f * 600, 20), 6, m.C(l % 2 == 0 ? p.Color1 : p.Color2, (1 - f) * br))); }
                m.Disc(o.X, o.Y, -620, s * k * .3f, s * k * .3f, 10, m.C(0xFFFFFF, .8f * br));
            }
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Sparks), 60); i++) { float f = Frac(m.Now * .6f + R(p.Seed, i, 3)), a = R(p.Seed, i, 4) * MathF.Tau + sp; m.Ball(o.X + MathF.Cos(a) * s * k * (1.4f - f * .4f), o.Y + MathF.Sin(a) * s * k * (1.4f - f * .4f), -f * 400, 5, m.C(p.Color2, 1 - f)); } }, 5);
        });
        S<LockOnParameter>(d, (p, c, m) => // ロックオン: 回りながら縮んで狙いを定める照準の輪と十字、捉えると赤く光る
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size), th = c.F(p.Thickness) * 2, cy = MathF.Max(.5f, c.F(p.Cycle)), k = p.Auto ? Math.Clamp(Frac(m.Now / cy) * 1.6f, 0, 1) : c.F(p.Lock) / 100, e = Ease(k);
            var col = k >= 1 ? m.C(p.LockColor) : m.C(p.Color); float r = s * (2 - e);
            m.Turn(o.X, o.Y, (1 - e) * 3, () => { for (int i = 0; i < 4; i++) m.Tube(ArcPts3(o.X, o.Y, r, i * MathF.PI / 2 + .2f, i * MathF.PI / 2 + 1.37f, 0, 10), th, col); });
            foreach (int s2 in Sides) { m.Limb(o.X + s2 * r * 1.1f, o.Y, 0, o.X + s2 * r * .5f, o.Y, 0, th, th, col); m.Limb(o.X, o.Y + s2 * r * 1.1f, 0, o.X, o.Y + s2 * r * .5f, 0, th, th, col); }
            m.Ball(o.X, o.Y, 0, th * 2, col);
            if (p.Decor) { Corners(m, o.X, o.Y, -20, s * 3, s * 3, 40, th * .7f, m.C(col, .6f)); if (k >= 1) m.Text("LOCK", 40, o.X, o.Y + s * 1.8f, 0, 4, col); }
        });
        S<RadarParameter>(d, (p, c, m) => // レーダー: 丸い画面の台、同心の輪と十字、回る扇の光、点滅する目標の点
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), br = c.F(p.Brightness) / 100, a0 = m.Now * c.F(p.SweepSpeed) / 50;
            m.Disc(o.X, o.Y, -30, r * 1.08f, r * 1.08f, 40, m.C(0x263238), .3f);
            if (p.Fill) m.Disc(o.X, o.Y, -8, r, r, 4, m.C(Sculpt.Dark(m.C(p.Color), .15f), 1));
            for (int i = 1; i <= Math.Clamp((int)c.F(p.Rings), 1, 8); i++) m.Torus(o.X, o.Y, -4, r * i / c.F(p.Rings), 2, m.C(p.Color, .5f * br));
            m.Box(o.X, o.Y, -4, r * 2, 3, 3, m.C(p.Color, .4f * br)); m.Box(o.X, o.Y, -4, 3, r * 2, 3, m.C(p.Color, .4f * br));
            for (int k = 0; k < 8; k++) { float a = a0 - k * .06f * c.F(p.Trail) / 80; m.Slab(-2, 4 + (8 - k), m.C(p.Color, (.5f - k * .06f) * br), PiePts(o.X, o.Y, r, a - .06f, a)); }
            for (int i = 0; i < Math.Min((int)c.F(p.Blips), 30); i++) { float a = R(p.Seed, i) * MathF.Tau, rr = r * (.15f + .8f * R(p.Seed, i, 1)), age = Frac((a0 - a) / MathF.Tau); m.Ball(o.X + MathF.Cos(a) * rr, o.Y + MathF.Sin(a) * rr, 6 + 20 * (1 - age), 8, m.C(p.BlipColor, (1 - age) * br)); }
            if (p.Ticks) for (int i = 0; i < 36; i++) { float a = i * MathF.Tau / 36; m.Limb(o.X + MathF.Cos(a) * r, o.Y + MathF.Sin(a) * r, 0, o.X + MathF.Cos(a) * r * (i % 3 == 0 ? .92f : .96f), o.Y + MathF.Sin(a) * r * (i % 3 == 0 ? .92f : .96f), 0, 2, 2, m.C(p.Color, br)); }
        });
        S<WaveformParameter>(d, (p, c, m) => // 波形: 方眼の板の前を流れる、光る心電図の線(または音のバー)
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.WaveWidth), h = c.F(p.WaveHeight), amp = c.F(p.Amplitude) / 100, sp = c.F(p.Speed) / 100, th = c.F(p.Thickness) * 2;
            if (p.ShowGrid) { m.Box(o.X, o.Y, -20, w, h, 10, m.C(0x06140C), .02f); for (int i = 1; i < 10; i++) m.Box(o.X - w / 2 + w * i / 10, o.Y, -14, 2, h, 2, m.C(p.Color, .2f)); for (int i = 1; i < 6; i++) m.Box(o.X, o.Y - h / 2 + h * i / 6, -14, w, 2, 2, m.C(p.Color, .2f)); }
            if (p.Kind == WaveKind.AudioBars || p.Kind == WaveKind.Spectrum)
            {
                int n = 32; for (int i = 0; i < n; i++) { float v = (.2f + .8f * MathF.Abs(MathF.Sin(m.Now * 6 * sp + i * .7f) * MathF.Sin(m.Now * 2.3f + i * .3f))) * amp, bh = h * .9f * v; m.Box(o.X - w / 2 + w * (i + .5f) / n, o.Y + h / 2 - bh / 2, 0, w / n * .7f, bh, 30, Sculpt.Mix(m.C(p.Color), m.C(p.Color2), v), .2f); }
                return;
            }
            var pts = new List<Vector3>(); int seg = 120;
            for (int i = 0; i <= seg; i++)
            {
                float f = i / (float)seg, x = o.X - w / 2 + w * f, u = Frac(f * 2 - m.Now * sp * .8f), y;
                if (p.Kind == WaveKind.Ecg) y = u > .4f && u < .44f ? -h * .45f : u > .44f && u < .48f ? h * .2f : u > .36f && u < .4f ? h * .05f : u > .6f && u < .7f ? -h * .1f * MathF.Sin((u - .6f) * 31) : 0;
                else y = MathF.Sin(f * 18 - m.Now * 6 * sp) * h * .35f;
                pts.Add(V3(x, o.Y + y * amp, 0));
            }
            m.Tube(pts, th, m.C(p.Color)); m.Tube(pts, th * 3, m.C(p.Color, .2f * c.F(p.Glow) / 100));
        });
        S<ScanBeamParameter>(d, (p, c, m) => // スキャンビーム: 四隅の括弧の範囲を、光の板が行き来して調べる
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.ScanWidth), h = c.F(p.ScanHeight), br = c.F(p.Brightness) / 100, per = MathF.Max(.2f, c.F(p.Period)), f = Frac(m.Now / per); if (p.PingPong) f = 1 - MathF.Abs(f * 2 - 1);
            if (p.ShowGrid) for (int i = 1; i < 8; i++) { m.Box(o.X - w / 2 + w * i / 8, o.Y, -10, 2, h, 2, m.C(p.Color, .2f * br)); m.Box(o.X, o.Y - h / 2 + h * i / 8, -10, w, 2, 2, m.C(p.Color, .2f * br)); }
            if (p.ShowBrackets) Corners(m, o.X, o.Y, 0, w, h, 60, 4, m.C(p.Color, br));
            if (p.Vertical) { float x = o.X - w / 2 + w * f; m.Box(x, o.Y, 0, c.F(p.Thickness) * 2, h, 200, m.C(p.Color, .35f * br), .1f); m.Box(x, o.Y, 0, c.F(p.Thickness) * 2, h, 6, m.C(0xFFFFFF, br)); }
            else { float y = o.Y - h / 2 + h * f; m.Box(o.X, y, 0, w, c.F(p.Thickness) * 2, 200, m.C(p.Color, .35f * br), .1f); m.Box(o.X, y, 0, w, c.F(p.Thickness) * 2, 6, m.C(0xFFFFFF, br)); }
            if (p.ShowBars) for (int i = 0; i < 5; i++) { float v = .3f + .7f * Frac(R(p.Seed, i) + m.Now * .3f); m.Box(o.X + w / 2 + 40 + v * 60, o.Y - h / 2 + 30 + i * 30, 0, v * 120, 14, 8, m.C(p.Color, br), .2f); }
        });
        S<MangaFxParameter>(d, (p, c, m) => // 漫画エフェクト: 集中線・スピード線・ギザギザの爆発・怒りマーク・汗・ハートなど
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size), op = c.F(p.Opacity) / 100, th = c.F(p.Thickness); int fr = (int)(m.Now * c.F(p.FrameRate)), n = Math.Min((int)c.F(p.Count), 120);
            var lc = m.C(p.LineColor, op); var fc = m.C(p.FillColor, op); var ac = m.C(p.AccentColor, op);
            switch (p.Kind)
            {
                case MangaKind.FocusLines:
                    for (int i = 0; i < n; i++) { float a = R(p.Seed + fr, i) * MathF.Tau, r0 = s + R(p.Seed + fr, i, 1) * s * .6f, L = m.W; m.Limb(o.X + MathF.Cos(a) * r0, o.Y + MathF.Sin(a) * r0, -50, o.X + MathF.Cos(a) * L, o.Y + MathF.Sin(a) * L, 200, 1, th * (.4f + R(p.Seed + fr, i, 2)), lc); }
                    break;
                case MangaKind.SpeedLines:
                    m.Turn(0, 0, c.F(p.Direction) * MathF.PI / 180, () => { for (int i = 0; i < n; i++) { float y = (R(p.Seed + fr, i) - .5f) * m.H, x = (R(p.Seed + fr, i, 1) - .5f) * m.W, L = 200 + 600 * R(p.Seed + fr, i, 2), z = -400 + R(p.Seed, i, 3) * 600; m.Limb(x, y, z, x + L, y, z, 1, th * .5f, lc); } });
                    break;
                case MangaKind.Impact:
                    { var pts = new List<float>(); for (int i = 0; i < 24; i++) { float a = i * MathF.Tau / 24, r = i % 2 == 0 ? s * (1 + .3f * R(p.Seed + fr, i)) : s * .55f; pts.Add(o.X + MathF.Cos(a) * r); pts.Add(o.Y + MathF.Sin(a) * r); } m.Slab(-20, 40, fc, pts.ToArray()); var p2 = pts.Select((v, i) => i % 2 == 0 ? o.X + (v - o.X) * .6f : o.Y + (v - o.Y) * .6f).ToArray(); m.Slab(15, 20, ac, p2); }
                    break;
                case MangaKind.Anger:
                    for (int i = 0; i < 4; i++) { float a = i * MathF.PI / 2 + MathF.PI / 4; m.Turn(o.X, o.Y, a, () => { m.Tube(ArcPts3(o.X + s * .45f, o.Y, s * .3f, MathF.PI * .6f, MathF.PI * 1.4f, 0, 10), th * .8f, ac); }); }
                    break;
                case MangaKind.Sweat:
                    for (int i = 0; i < 3; i++) { float x = o.X + (i - 1) * s * .5f, y = o.Y + i % 2 * s * .3f + Frac(m.Now + i * .3f) * 30; m.Lathe(x, 0, m.C(0x7EC8FF, op), 0, y - s * .35f, s * .12f, y, s * .14f, y + s * .12f, 0, y + s * .2f); }
                    break;
                case MangaKind.Hearts:
                    for (int i = 0; i < 5; i++) { float f = Frac(m.Now * .5f + i / 5f); Heart3D(m, o.X + (i - 2) * s * .4f + MathF.Sin(m.Now * 3 + i) * 20, o.Y - f * s * 1.5f, 0, s * .35f, m.C(p.AccentColor, op * (1 - f))); }
                    break;
                case MangaKind.Surprise:
                    m.Box(o.X, o.Y - s * .2f, 0, s * .22f, s * .9f, s * .2f, ac, .4f); m.Ball(o.X, o.Y + s * .55f, 0, s * .14f, ac);
                    for (int i = 0; i < 6; i++) { float a = -MathF.PI / 2 + (i - 2.5f) * .45f; m.Limb(o.X + MathF.Cos(a) * s * .8f, o.Y + MathF.Sin(a) * s * .8f, 0, o.X + MathF.Cos(a) * s * 1.2f, o.Y + MathF.Sin(a) * s * 1.2f, 0, th * .4f, th * .6f, lc); }
                    break;
                default:
                    for (int i = 0; i < 8; i++) { float x = o.X + (R(p.Seed, i) - .5f) * s * 2, y = o.Y + (R(p.Seed, i, 1) - .5f) * s * 2, tw = MathF.Abs(MathF.Sin(m.Now * 4 + i)); m.Slab(-5, 10, m.C(p.FillColor, op * tw), StarPts(x, y, s * .25f * tw + 4, 4, .25f, 0)); }
                    break;
            }
        });
        S<TelopDecoParameter>(d, (p, c, m) => // テロップ装飾: 帯・吹き出し・叫び・札・リボンなどの、厚みのある字幕の台
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float k = (p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.IntroTime)), 0, 1)) : 1) * c.F(p.Appear) / 100; if (k <= 0) return;
            float w = c.F(p.BoxWidth) * k, h = c.F(p.BoxHeight), cr = c.F(p.Corner), lw = c.F(p.LineWidth), bob = p.Bob ? MathF.Sin(m.Now * 3) * 8 : 0; var fc = m.C(p.FillColor, c.F(p.FillOpacity) / 100); var lc = m.C(p.LineColor); var ac = m.C(p.AccentColor);
            m.Move(0, bob, () =>
            {
                if (p.Shadow) m.Box(o.X + 12, o.Y + 12, -30, w, h, 10, m.C(0x000000, .35f), .1f);
                switch (p.Style)
                {
                    case TelopStyle.Bubble: Puff(m, -12, 24, fc, RoundRectFlat(o.X, o.Y, w, h, MathF.Min(cr * 2, h / 2))); { float tx = o.X + c.F(p.TailX) / 100 * w, ty = p.TailUp ? o.Y - h / 2 : o.Y + h / 2, s = p.TailUp ? -1 : 1; m.Slab(-12, 24, fc, tx - 30, ty - s * 5, tx + 30, ty - s * 5, tx - 10, ty + s * 60); } break;
                    case TelopStyle.Shout: { var pts = new List<float>(); for (int i = 0; i < 28; i++) { float a = i * MathF.Tau / 28, rr = i % 2 == 0 ? 1.15f : .92f; pts.Add(o.X + MathF.Cos(a) * w / 2 * rr); pts.Add(o.Y + MathF.Sin(a) * h / 2 * rr * 1.2f); } m.Slab(-12, 24, fc, pts.ToArray()); } break;
                    case TelopStyle.Tag: m.Slab(-12, 24, fc, o.X - w / 2, o.Y - h / 2, o.X + w / 2 - h / 2, o.Y - h / 2, o.X + w / 2, o.Y, o.X + w / 2 - h / 2, o.Y + h / 2, o.X - w / 2, o.Y + h / 2); m.Disc(o.X + w / 2 - h * .6f, o.Y, 14, 12, 12, 6, lc); break;
                    case TelopStyle.Ribbon: m.Box(o.X, o.Y, 0, w, h, 24, ac, .1f); foreach (int s in Sides) m.Slab(-30, 24, m.C(Sculpt.Dark(ac, .7f), 1), o.X + s * w / 2, o.Y - h * .3f, o.X + s * (w / 2 + h * .8f), o.Y - h * .3f, o.X + s * (w / 2 + h * .5f), o.Y + h * .2f, o.X + s * (w / 2 + h * .8f), o.Y + h * .7f, o.X + s * w / 2, o.Y + h * .7f); break;
                    case TelopStyle.DoubleFrame: m.Box(o.X, o.Y, 0, w, h, 20, fc, .1f); RectBars(m, o.X, o.Y, 14, w - 20, h - 20, lw, lc); RectBars(m, o.X, o.Y, 14, w + 14, h + 14, lw * .6f, ac); break;
                    case TelopStyle.Underline: m.Box(o.X, o.Y, -6, w, h, 6, m.C(p.FillColor, .2f), .1f); m.Box(o.X, o.Y + h / 2, 0, w, lw * 2, 20, ac, .3f); break;
                    case TelopStyle.Brackets: foreach (int s in Sides) { m.Box(o.X + s * w / 2, o.Y, 0, lw * 2, h, 20, lc, .3f); m.Box(o.X + s * (w / 2 - 30), o.Y - h / 2, 0, 60, lw * 2, 20, lc, .3f); m.Box(o.X + s * (w / 2 - 30), o.Y + h / 2, 0, 60, lw * 2, 20, lc, .3f); } break;
                    default: m.Box(o.X, o.Y, 0, w, h, 24, fc, MathF.Min(.4f, cr / h)); m.Box(o.X - w / 2 + 14, o.Y, 14, 16, h - 20, 8, ac, .3f); m.Box(o.X, o.Y + h / 2 - 6, 10, w - 20, lw, 6, lc, .3f); break;
                }
            });
        });
        S<MarkerParameter>(d, (p, c, m) => // 強調マーカー: 手書き風の丸・四角・下線・波線・矢印・チェック・バツの太い線
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.MarkWidth), h = c.F(p.MarkHeight), th = c.F(p.Thickness) * (p.Highlighter ? 3 : 1), k = (p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.IntroTime)), 0, 1)) : 1) * c.F(p.Progress) / 100, wb = c.F(p.Wobble) / 100;
            var col = m.C(p.Color, c.F(p.Opacity) / 100 * (p.Highlighter ? .5f : 1)); if (k <= 0) return;
            var pts = new List<Vector3>(); void Add(float x, float y, int i) => pts.Add(V3(o.X + x + (R(p.Seed, i) - .5f) * 14 * wb, o.Y + y + (R(p.Seed, i, 1) - .5f) * 14 * wb, 10));
            switch (p.Kind)
            {
                case MarkKind.Circle: for (int i = 0; i <= 40; i++) { float a = -MathF.PI * .6f + i / 40f * MathF.Tau * 1.08f; Add(MathF.Cos(a) * w / 2 * (1 + i * .002f), MathF.Sin(a) * h / 2, i); } break;
                case MarkKind.Box: { float[] xy = { -1, -1, 1, -1, 1, 1, -1, 1, -1, -1.1f }; for (int i = 0; i < 5; i++) Add(xy[i * 2] * w / 2, xy[i * 2 + 1] * h / 2, i); } break;
                case MarkKind.Underline: case MarkKind.DoubleUnderline: for (int i = 0; i <= 10; i++) Add(-w / 2 + w * i / 10, h / 2, i); break;
                case MarkKind.Wave: for (int i = 0; i <= 30; i++) Add(-w / 2 + w * i / 30, h / 2 + MathF.Sin(i * .9f) * 15, i); break;
                case MarkKind.Arrow: Add(-w / 2, h / 2, 0); Add(0, h * .1f, 1); Add(w / 2, -h / 2, 2); break;
                case MarkKind.Check: Add(-w * .3f, 0, 0); Add(-w * .05f, h / 2, 1); Add(w * .35f, -h / 2, 2); break;
                default: Add(-w * .3f, -h / 2, 0); Add(w * .3f, h / 2, 1); break;
            }
            int cnt = Math.Max(2, (int)MathF.Ceiling(pts.Count * k)); var part = pts.Take(cnt).ToList();
            m.Turn(o.X, o.Y, c.F(p.Rotation) * MathF.PI / 180, () =>
            {
                m.Tube(part, th, col);
                if (p.Kind == MarkKind.DoubleUnderline) m.Tube(part.Select(q => q + V3(0, th * 3)).ToList(), th, col);
                if (p.Kind == MarkKind.Cross && k > .5f) m.Tube(new List<Vector3> { V3(o.X + w * .3f, o.Y - h / 2, 10), V3(o.X - w * .3f, o.Y + h / 2, 10) }, th, col);
                if (p.Kind == MarkKind.Arrow && k >= 1) { m.Limb(o.X + w / 2, o.Y - h / 2, 10, o.X + w / 2 - 60, o.Y - h / 2 + 10, 10, th, th, col); m.Limb(o.X + w / 2, o.Y - h / 2, 10, o.X + w / 2 - 20, o.Y - h / 2 + 60, 10, th, th, col); }
            });
        });
        S<TransitionParameter>(d, (p, c, m) => // 画面転換: 色の板・羽板・格子のタイル・円などが、立体に回って画面を覆う
        {
            float k = p.Auto ? Math.Clamp((m.Now - c.F(p.StartTime)) / MathF.Max(.05f, c.F(p.Duration)), 0, 1) : c.F(p.Progress) / 100; if (p.Reveal) k = 1 - k; k = Ease(k);
            var col = m.C(p.Color); int n = Math.Clamp((int)c.F(p.CellCount), 2, 24); string kind = p.Kind.ToString();
            if (k <= 0) return;
            if (kind.StartsWith("Blinds")) { bool v = kind == "BlindsV"; for (int i = 0; i < n; i++) { float pos = ((i + .5f) / n - .5f) * (v ? m.W : m.H); if (v) m.Yaw(pos, 0, 0, (1 - k) * MathF.PI / 2, () => m.Box(pos, 0, 0, m.W / n + 2, m.H, 10, col, .01f)); else m.Pitch(0, pos, 0, (1 - k) * MathF.PI / 2, () => m.Box(0, pos, 0, m.W, m.H / n + 2, 10, col, .01f)); } }
            else if (kind == "Grid" || kind == "Pixelate" || kind == "Hexagons" || kind == "Diamonds" || kind == "Triangles" || kind == "Dots")
            { int cols = n, rows = Math.Max(2, (int)(n * m.H / m.W)); float cw = m.W / cols, ch = m.H / rows; for (int y = 0; y < rows; y++) for (int x = 0; x < cols; x++) { float g = Math.Clamp(k * 2 - (x + y) / (float)(cols + rows), 0, 1); if (g <= 0) continue; float px = -m.W / 2 + cw * (x + .5f), py = -m.H / 2 + ch * (y + .5f); m.Yaw(px, py, 0, (1 - g) * MathF.PI / 2, () => { if (kind == "Dots") m.Disc(px, py, 0, cw * .75f * g, cw * .75f * g, 10, col); else m.Box(px, py, 0, cw + 2, ch + 2, 10, Sculpt.Mix(col, m.C(p.Color2), kind == "Grid" ? 0 : (x + y) % 2 * .3f), .02f); }); } }
            else if (kind.Contains("Circle") || kind.Contains("Iris")) { float r = MathF.Sqrt(m.W * m.W + m.H * m.H) / 2 * (kind == "CircleOpen" ? k : 1 - k); var hole = kind == "IrisClose" || kind.Contains("Star") || kind.Contains("Heart"); for (int i = 0; i < 24; i++) { float a = i * MathF.Tau / 24; m.Box(MathF.Cos(a) * (r + m.W / 2), MathF.Sin(a) * (r + m.W / 2), 0, m.W, m.W, 10, col, .01f); } if (!hole) m.Disc(0, 0, 0, r, r, 10, col); }
            else if (kind == "Curtain") foreach (int s in Sides) { for (int i = 0; i < 8; i++) { float x = s * (m.W / 2 - (i + .5f) * m.W / 16 * k); m.Cyl(x, 0, MathF.Sin(i * 1.7f) * 20, m.W / 32 * k + 4, m.H, Sculpt.Mix(m.C(0xB71C1C), m.C(0x7F0000), i % 2), .1f); } }
            else if (kind == "PageTurn") m.Yaw(-m.W / 2, 0, 0, (1 - k) * -MathF.PI * .9f, () => m.Box(0, 0, 0, m.W, m.H, 10, col, .01f));
            else if (kind == "Stamp") { float s = 1 + (1 - k) * 3; m.Scale(0, 0, 0, s, () => { m.Box(0, 0, 0, m.W, m.H, 10, m.C(p.Color2), .01f); m.Disc(0, 0, 10, 300, 300, 30, col); }); }
            else { bool v = kind == "WipeV"; float sh = kind == "WipeDiag" ? .35f : 0; m.Turn(0, 0, sh, () => { float span = (v ? m.H : m.W) * 1.4f, pos = -span / 2 + span * k / 2; if (v) m.Box(0, -span / 2 + span * k / 2, 0, m.W * 1.5f, span * k, 30, col, .01f); else m.Box(-span / 2 + span * k / 2, 0, 0, span * k, m.H * 1.5f, 30, col, .01f); m.Box(v ? 0 : -span / 2 + span * k, v ? -span / 2 + span * k : 0, 20, v ? m.W * 1.5f : 30, v ? 30 : m.H * 1.5f, 40, m.C(p.Color2), .2f); }); }
        });
        S<FilmLookParameter>(d, (p, c, m) => // フィルム風: 画面のふちのフィルムの穴の帯、ちらつく粒・傷、光漏れ
        {
            int fr = (int)(m.Now * c.F(p.FrameRate));
            m.Box(0, 0, -400, m.W * 1.4f, m.H * 1.4f, 4, m.C(p.TintColor, c.F(p.TintAmount) / 100 * .5f), .01f);
            foreach (int s in Sides) { m.Box(s * (m.W / 2 - 40), 0, 0, 80, m.H, 10, K, .01f); for (int i = 0; i < 10; i++) m.Box(s * (m.W / 2 - 40), -m.H / 2 + (i + .5f) * m.H / 10 + Cyc(m.Now * 400, m.H / 10) - m.H / 20, 6, 36, 50, 4, m.C(0xE8E0D0), .3f); }
            if (p.Letterbox) foreach (int s in Sides) m.Box(0, s * (m.H / 2 - c.F(p.BarHeight) / 100 * m.H / 2), 20, m.W, c.F(p.BarHeight) / 100 * m.H, 10, K, .01f);
            for (int i = 0; i < (int)(c.F(p.Scratches) / 10); i++) { float x = (R(fr, i) - .5f) * m.W; m.Box(x, 0, 30, 2, m.H, 2, m.C(0xFFFFFF, .4f)); }
            Low(m, () => { for (int i = 0; i < (int)(c.F(p.Dust)); i++) if (R(fr, i, 3) < .5f) m.Ball((R(fr, i, 1) - .5f) * m.W, (R(fr, i, 2) - .5f) * m.H, 30, 3 + 5 * R(fr, i, 4), m.C(0x111111, .7f)); }, 5);
            m.Ball(-m.W / 2, -m.H * .3f, -100, m.W * .35f, m.H * .5f, 100, m.C(p.LeakColor, c.F(p.LeakAmount) / 100 * .5f * (.7f + .3f * MathF.Sin(m.Now))));
            for (int i = 0; i < 4; i++) { float f = c.F(p.Vignette) / 100 * (.5f - i * .1f), w = 60 + i * 50; foreach (int s in Sides) { m.Box(s * (m.W / 2 - w / 2), 0, -50 + i, w, m.H, 4, m.C(0x000000u, f), .01f); m.Box(0, s * (m.H / 2 - w / 2), -50 + i, m.W, w, 4, m.C(0x000000u, f), .01f); } }
        });
        S<GlitchParameter>(d, (p, c, m) => // グリッチ: ずれて飛び出す横長の板、赤・青にずれた帯、ノイズのブロック
        {
            float it = c.F(p.Intensity) / 100; int fr = (int)(m.Now * MathF.Max(1, c.F(p.Rate))); if (R(p.Seed, fr, 9) > c.F(p.Chance) / 100) return;
            for (int i = 0; i < (int)(c.F(p.Strips) / 10); i++) { float y = (R(fr, i) - .5f) * m.H, h = 10 + 60 * R(fr, i, 1), sh = (R(fr, i, 2) - .5f) * 300 * it, z = (R(fr, i, 3) - .5f) * 300 * it; m.Box(sh, y, z, m.W, h, 10, m.C(R(fr, i, 4) < .5f ? 0xFF2050u : 0x20E0FFu, .4f * c.F(p.RgbShift) / 100), .01f); }
            for (int i = 0; i < (int)(c.F(p.Blocks) / 5); i++) { float x = (R(fr, i, 5) - .5f) * m.W, y = (R(fr, i, 6) - .5f) * m.H, s = 30 + 100 * R(fr, i, 7); m.Box(x, y, R(fr, i, 8) * 200 * it, s * 2, s, s * .4f, Pal(m, i + fr), .02f); }
            Low(m, () => { for (int i = 0; i < (int)(c.F(p.Noise) * 2); i++) m.Box((R(fr, i, 10) - .5f) * m.W, (R(fr, i, 11) - .5f) * m.H, 40, 6, 6, 2, m.C(0xFFFFFF, .6f)); }, 4);
            if (c.F(p.Scanlines) > 0) for (int i = 0; i < 30; i++) m.Box(0, -m.H / 2 + i * m.H / 30, 50, m.W, 2, 2, m.C(0x000000, c.F(p.Scanlines) / 100 * .5f));
        });
        S<GaugeParameter>(d, (p, c, m) => // ゲージ: 斜めに切った枠の中で伸び縮みする厚いバー、丸いメーターの針
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float v = Math.Clamp(c.F(p.Value) / 100, 0, 1), w = c.F(p.GaugeWidth), h = c.F(p.GaugeHeight), op = c.F(p.Opacity) / 100;
            var fc = p.ColorByValue ? (v < .25f ? m.C(0xFF3B30u, op) : v < .5f ? m.C(0xFFCC00u, op) : m.C(p.FillColor, op)) : m.C(p.FillColor, op);
            if (p.Kind == GaugeKind.Circle || p.Kind == GaugeKind.Meter)
            {
                float r = w * .35f; bool meter = p.Kind == GaugeKind.Meter; float a0 = meter ? MathF.PI * .75f : -MathF.PI / 2, span = meter ? MathF.PI * 1.5f : MathF.Tau;
                m.Tube(ArcPts3(o.X, o.Y, r, a0, a0 + span, 0, 60), h * .3f, m.C(p.TrackColor, op)); m.Tube(ArcPts3(o.X, o.Y, r, a0, a0 + span * v, 8, 60), h * .32f, fc);
                if (meter) { float a = a0 + span * v; m.Limb(o.X, o.Y, 20, o.X + MathF.Cos(a) * r * .85f, o.Y + MathF.Sin(a) * r * .85f, 20, 10, 3, m.C(0xFF3B30u, op)); m.Ball(o.X, o.Y, 20, 20, m.C(p.FrameColor, op)); }
                if (p.ShowNumber) m.Text($"{(int)(v * 100)}", r * .5f, o.X, o.Y + (meter ? r * .5f : 0), 10, 6, m.C(p.FrameColor, op));
                return;
            }
            bool vert = p.Kind == GaugeKind.BarV; float sl = p.Kind == GaugeKind.HpBar ? c.F(p.Slant) : 0;
            m.Turn(o.X, o.Y, vert ? -MathF.PI / 2 : 0, () =>
            {
                m.Slab(-14, 28, m.C(p.FrameColor, op), o.X - w / 2 - 8 + sl, o.Y - h / 2 - 8, o.X + w / 2 + 8 + sl, o.Y - h / 2 - 8, o.X + w / 2 + 8 - sl, o.Y + h / 2 + 8, o.X - w / 2 - 8 - sl, o.Y + h / 2 + 8);
                m.Slab(-4, 22, m.C(p.TrackColor, op), o.X - w / 2 + sl, o.Y - h / 2, o.X + w / 2 + sl, o.Y - h / 2, o.X + w / 2 - sl, o.Y + h / 2, o.X - w / 2 - sl, o.Y + h / 2);
                float fx = o.X - w / 2 + w * v; if (p.DamageTrail) { float tv = MathF.Min(1, v + .12f * MathF.Max(0, 1 - Frac(m.Now * .5f) * 2)); float tx = o.X - w / 2 + w * tv; m.Slab(0, 24, m.C(0xFF5252u, op), o.X - w / 2 + sl, o.Y - h / 2, tx + sl, o.Y - h / 2, tx - sl, o.Y + h / 2, o.X - w / 2 - sl, o.Y + h / 2); }
                if (v > 0) m.Slab(2, 26, fc, o.X - w / 2 + sl, o.Y - h / 2, fx + sl, o.Y - h / 2, fx - sl, o.Y + h / 2, o.X - w / 2 - sl, o.Y + h / 2);
                int seg = (int)c.F(p.Segments); for (int i = 1; i < seg; i++) m.Box(o.X - w / 2 + w * i / seg, o.Y, 20, 4, h, 10, m.C(p.TrackColor, op));
                if (p.ShowNumber && !vert) m.Text($"{(int)(v * 100)}%", h * .8f, o.X + w / 2 + h * 1.2f, o.Y, 0, 6, m.C(p.FrameColor, op));
            });
        });
        S<ChartParameter>(d, (p, c, m) => // グラフ: 床と軸の上に立つ角柱の棒、折れ線の管、厚い円グラフ
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.ChartWidth), h = c.F(p.ChartHeight), mx = MathF.Max(1, c.F(p.MaxValue)), g = p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.IntroTime)), 0, 1)) * c.F(p.Grow) / 100 : c.F(p.Grow) / 100;
            int n = Math.Clamp(p.Items, 1, 6); var vals = new[] { c.F(p.V1), c.F(p.V2), c.F(p.V3), c.F(p.V4), c.F(p.V5), c.F(p.V6) };
            Vector4 Col(int i) => p.Colorful ? Pal(m, i) : Sculpt.Mix(m.C(p.Color1), m.C(p.Color2), i / (float)MathF.Max(1, n - 1));
            if (p.Kind == ChartKind.Pie || p.Kind == ChartKind.Donut)
            {
                float tot = 0; for (int i = 0; i < n; i++) tot += MathF.Max(0, vals[i]); float a = -MathF.PI / 2, r = MathF.Min(w, h) * .45f;
                for (int i = 0; i < n; i++) { float sp = MathF.Max(0, vals[i]) / MathF.Max(1, tot) * MathF.Tau * g; if (sp > .01f) { if (p.Kind == ChartKind.Donut) m.Tube(ArcPts3(o.X, o.Y, r * .75f, a, a + sp, 0, 30), r * .25f, Col(i)); else m.Slab(-30 - i * 6, 60 + i * 12, Col(i), PiePts(o.X, o.Y, r, a, a + sp)); } a += sp; }
                return;
            }
            float bx = o.X - w / 2, by = o.Y + h / 2;
            m.Box(o.X, by + 6, 0, w + 20, 12, 160, m.C(p.AxisColor), .1f); m.Box(bx - 6, o.Y, -70, 12, h, 12, m.C(p.AxisColor), .1f);
            if (p.ShowGrid) for (int i = 1; i <= 4; i++) m.Box(o.X, by - h * i / 4, -76, w, 2, 2, m.C(p.AxisColor, .3f));
            if (p.Kind == ChartKind.Line)
            {
                var pts = new List<Vector3>(); for (int i = 0; i < n; i++) pts.Add(V3(bx + w * (i + .5f) / n, by - h * vals[i] / mx * g, 0)); m.Tube(pts, c.F(p.Thickness) * 2.5f, m.C(p.Color1)); foreach (var q in pts) m.Ball(q.X, q.Y, q.Z, 16, m.C(p.Color2));
            }
            else for (int i = 0; i < n; i++)
            {
                float bh = (p.Kind == ChartKind.BarH ? w : h) * vals[i] / mx * g;
                if (p.Kind == ChartKind.BarH) { float y = o.Y - h / 2 + h * (i + .5f) / n; m.Box(bx + bh / 2, y, 0, MathF.Max(1, bh), h / n * .6f, 80, Col(i), .1f); if (p.ShowNumbers) m.Text(((int)vals[i]).ToString(), 40, bx + bh + 40, y, 0, 4, m.C(p.AxisColor)); }
                else { float x = bx + w * (i + .5f) / n; m.Box(x, by - bh / 2, 0, w / n * .6f, MathF.Max(1, bh), 80, Col(i), .1f); if (p.ShowNumbers) m.Text(((int)vals[i]).ToString(), 40, x, by - bh - 34, 0, 4, m.C(p.AxisColor)); }
            }
        });
        S<CountdownParameter>(d, (p, c, m) => // カウントダウン: 厚い数字の板と、減っていく輪の枠(残りが少ないと赤く)
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float st = MathF.Max(1, c.F(p.StartSeconds)), t = MathF.Max(0, m.Now - c.F(p.Delay)); if (p.Loop) t %= st;
            bool up = p.Mode == CountMode.Stopwatch; float rem = up ? t : MathF.Max(0, st - t), dh = c.F(p.DigitHeight); bool warn = !up && rem <= 3;
            var col = warn ? m.C(p.WarnColor) : m.C(p.DigitColor); string s = p.Mode == CountMode.Timer || up ? $"{(int)rem / 60:00}:{(int)rem % 60:00}" : ((int)MathF.Ceiling(rem)).ToString();
            float pop = p.Pop ? 1 + .15f * MathF.Max(0, 1 - Frac(t) * 4) : 1;
            if (p.ShowFrame || p.Mode == CountMode.Ring) { float r = dh * .9f; m.Torus(o.X, o.Y, -20, r, c.F(p.Thickness) * .6f, m.C(p.FrameColor, .3f)); m.Tube(ArcPts3(o.X, o.Y, r, -MathF.PI / 2, -MathF.PI / 2 + MathF.Tau * (up ? Frac(t / 60) : rem / st), -10, 60), c.F(p.Thickness) * .8f, col); }
            m.Scale(o.X, o.Y, 0, pop, () => m.Text(s, dh, o.X, o.Y, 0, dh * .18f, col));
        });
        S<UiOverlayParameter>(d, (p, c, m) => // UIオーバーレイ: 録画中の赤い点・LIVE札・再生バー・電話の着信などの立体の部品
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size) * 1.6f, pulse = p.Pulse ? .5f + .5f * MathF.Sin(m.Now * 5) : 1;
            switch (p.Kind)
            {
                case UiKind.Rec: m.Box(o.X, o.Y, -10, s * 2.6f, s, 20, m.C(p.PanelColor, .8f), .4f); m.Ball(o.X - s * .8f, o.Y, 10, s * .28f, m.C(p.MainColor, .3f + .7f * pulse)); m.Text("REC", s * .5f, o.X + s * .35f, o.Y, 5, 4, m.C(p.LineColor)); break;
                case UiKind.Live: m.Box(o.X, o.Y, 0, s * 2.4f, s, 30, m.C(p.MainColor), .3f); m.Text("LIVE", s * .55f, o.X, o.Y, 16, 4, m.C(p.LineColor)); m.Ball(o.X - s * .9f, o.Y, 18, s * .12f, m.C(p.LineColor, pulse)); break;
                case UiKind.PlayBar: { float w = m.W * .8f, v = c.F(p.Progress) / 100, y = m.H / 2 - 80; m.Box(0, y, 0, w, 14, 14, m.C(p.PanelColor), .5f); m.Box(-w / 2 + w * v / 2, y, 4, w * v, 16, 16, m.C(p.MainColor), .5f); m.Ball(-w / 2 + w * v, y, 10, 20, m.C(p.LineColor)); m.Slab(-6, 12, m.C(p.LineColor), -w / 2, y - 70, -w / 2, y - 30, -w / 2 + 34, y - 50); } break;
                case UiKind.Badge: m.Disc(o.X, o.Y, 0, s * .6f, s * .6f, 30, m.C(p.MainColor), .4f); m.Text(((int)c.F(p.Count)).ToString(), s * .6f, o.X, o.Y, 18, 4, m.C(p.LineColor)); break;
                case UiKind.Phone: m.Box(o.X, o.Y, 0, s * 2, s * 4, 30, m.C(p.PanelColor), .3f); m.Box(o.X, o.Y, 16, s * 1.8f, s * 3.6f, 4, m.C(0x223040u), .1f); m.Ball(o.X, o.Y + s, 24, s * .35f, m.C(0x34C759u, pulse)); m.Turn(o.X, o.Y - s * .5f, MathF.Sin(m.Now * 20) * .2f, () => m.Text("☎", s, o.X, o.Y - s * .5f, 20, 4, m.C(p.LineColor))); break;
                default: m.Box(o.X - m.W * .1f, o.Y + m.H * .2f, 0, m.W * .6f, m.H * .6f, 20, m.C(0xFFFFFFu), .05f); m.Box(o.X - m.W * .1f, o.Y + m.H * .2f - m.H * .3f + 25, 12, m.W * .6f, 50, 10, m.C(p.PanelColor), .1f); for (int i = 0; i < 3; i++) m.Ball(o.X - m.W * .4f + 30 + i * 30, o.Y + m.H * .2f - m.H * .3f + 25, 20, 9, m.C(new uint[] { 0xFF5F57, 0xFEBC2E, 0x28C840 }[i])); break;
            }
        });
        S<ViewfinderParameter>(d, (p, c, m) => // ファインダー: 画面の四隅の括弧、三分割の線、ピントの枠、録画・電池・水平の表示
        {
            float mg = c.F(p.Margin) / 100, w = m.W * (1 - mg * 2), h = m.H * (1 - mg * 2), th = c.F(p.Thickness) * 2; var lc = m.C(p.LineColor);
            Corners(m, 0, 0, 0, w, h, 90, th, lc);
            if (p.ShowGrid) for (int i = 1; i < 3; i++) { m.Box(-w / 2 + w * i / 3, 0, -4, 2, h, 2, m.C(p.LineColor, .35f)); m.Box(0, -h / 2 + h * i / 3, -4, w, 2, 2, m.C(p.LineColor, .35f)); }
            if (p.ShowFocus) { var f = PctPos(m, c.F(p.FocusX), c.F(p.FocusY)); float fs = c.F(p.FocusSize), b = 1 + .1f * MathF.Max(0, 1 - m.Now * 2); Corners(m, f.X, f.Y, 20, fs * b, fs * b, 30, th * .8f, m.C(p.FocusColor)); }
            if (p.ShowRec) { m.Ball(-w / 2 + 50, -h / 2 + 50, 10, 16, m.C(p.RecColor, MathF.Sin(m.Now * 4) > 0 ? 1 : .2f)); m.Text("REC", 40, -w / 2 + 120, -h / 2 + 50, 5, 3, lc); }
            if (p.ShowBattery) { m.Box(w / 2 - 90, -h / 2 + 50, 0, 90, 40, 14, m.C(p.LineColor, .3f), .2f); m.Box(w / 2 - 105, -h / 2 + 50, 8, 52, 28, 8, lc, .2f); m.Box(w / 2 - 40, -h / 2 + 50, 0, 10, 18, 14, lc, .2f); }
            if (p.ShowLevel) { m.Box(0, 0, 10, 200, 4, 4, lc); m.Ball(0, 0, 10, 8, lc); }
            if (p.ShowZoom) m.Text("x1.0", 36, w / 2 - 90, h / 2 - 50, 5, 3, lc);
        });
        S<WindowGlassParameter>(d, (p, c, m) => // ガラス窓: 厚い窓枠と格子の桟、薄く色づいたガラスの板、反射の筋、割れて落ちる破片
        {
            float ft = c.F(p.FrameThickness), bt = c.F(p.BarThickness); int vb = (int)c.F(p.VBars), hb = (int)c.F(p.HBars); var fc = m.C(p.FrameColor);
            float crackT = p.AutoCrack ? m.Now - c.F(p.CrackTime) : -1; bool broken = p.AutoCrack && crackT > c.F(p.SpreadTime);
            if (!broken) m.Box(0, 0, -6, m.W, m.H, 6, m.C(p.GlassColor, c.F(p.Tint) / 100 + .08f), .01f);
            else if (p.ShatterFall) { int n = Math.Min((int)c.F(p.PieceCount), 60); float ft2 = crackT - c.F(p.SpreadTime); for (int i = 0; i < n; i++) { float x = (R(p.Seed, i) - .5f) * m.W, y = (R(p.Seed, i, 1) - .5f) * m.H + c.F(p.FallGravity) * ft2 * ft2 / 2 * (.5f + R(p.Seed, i, 2)), s = 60 + 100 * R(p.Seed, i, 3); m.Turn(x, y, ft2 * 3 * (R(p.Seed, i, 4) - .5f), () => m.Slab(-3 + ft2 * 200 * R(p.Seed, i, 5), 6, m.C(p.GlassColor, .35f), x - s / 2, y - s / 3, x + s / 2, y - s / 2, x + s / 3, y + s / 2)); } }
            for (int i = 0; i < Math.Min((int)c.F(p.StreakCount), 8); i++) { float x = -m.W * .4f + i * 140 + Cyc(m.Now * c.F(p.StreakMove), m.W); m.Turn(x, 0, c.F(p.StreakAngle) * MathF.PI / 180, () => m.Box(x, 0, -2, c.F(p.StreakWidth) * (i % 2 == 0 ? 1 : .4f), m.H * 1.5f, 2, m.C(0xFFFFFF, .12f * c.F(p.Reflection) / 100), .01f)); }
            RectBars(m, 0, 0, 0, m.W - ft, m.H - ft, ft, fc);
            for (int i = 1; i <= vb; i++) m.Box(-m.W / 2 + m.W * i / (vb + 1), 0, 0, bt, m.H, bt, fc, .2f);
            for (int i = 1; i <= hb; i++) m.Box(0, -m.H / 2 + m.H * i / (hb + 1), 0, m.W, bt, bt, fc, .2f);
            float cr = p.AutoCrack ? Math.Clamp(crackT / MathF.Max(.01f, c.F(p.SpreadTime)), 0, 1) : c.F(p.Crack) / 100;
            if (cr > 0 && !broken) { var o = PctPos(m, c.F(p.CrackX), c.F(p.CrackY)); for (int i = 0; i < 10; i++) { float a = i * MathF.Tau / 10 + R(p.Seed, i, 6) * .4f; var pts = new List<Vector3> { V3(o.X, o.Y, 2) }; for (int k = 1; k <= 4; k++) pts.Add(V3(o.X + MathF.Cos(a + (R(p.Seed, i * 5 + k, 7) - .5f) * .4f) * 120 * k * cr, o.Y + MathF.Sin(a + (R(p.Seed, i * 5 + k, 8) - .5f) * .4f) * 120 * k * cr, 2)); m.Tube(pts, 2, m.C(0xFFFFFF, .8f)); } }
        });
        S<GradientBgParameter>(d, (p, c, m) => // 動くグラデーション背景: 奥の色の壁の前を、ゆっくり漂う大きな色の玉
        {
            var cols = new[] { p.Color1, p.Color2, p.Color3, p.Color4 }; float op = c.F(p.Opacity) / 100, sp = c.F(p.Speed) / 100, bs = c.F(p.BlobSize);
            SkyBands(m, m.C(p.Color1, op), m.C(p.Color2, op), -1200, 10);
            for (int i = 0; i < 6; i++) { float x = MathF.Sin(m.Now * .2f * sp + i * 1.9f) * m.W * .5f, y = MathF.Cos(m.Now * .17f * sp + i * 2.3f) * m.H * .4f; m.Ball(x, y, -900 + i * 60, bs * .5f, bs * .4f, 80, m.C(cols[i % 4], .35f * c.F(p.BlobStrength) / 100 * op)); }
        });
        S<GeoBgParameter>(d, (p, c, m) => // 幾何学模様の背景: 板の上に並ぶ、点・しま・六角・市松などの浮き出た模様
        {
            m.Box(0, 0, -40, m.W * 1.2f, m.H * 1.2f, 20, m.C(p.BgColor, c.F(p.BgOpacity) / 100), .01f);
            float s = MathF.Max(20, c.F(p.Size)), dir = c.F(p.Direction) * MathF.PI / 180, off = m.Now * c.F(p.Scroll), ox = Cyc(MathF.Cos(dir) * off, s), oy = Cyc(MathF.Sin(dir) * off, s), pu = 1 + c.F(p.Pulse) / 100 * .3f * MathF.Sin(m.Now * 2);
            var col = m.C(p.PatternColor, c.F(p.Strength) / 100); int nx = (int)(m.W / s) + 2, ny = (int)(m.H / s) + 2;
            Low(m, () =>
            {
                for (int y = 0; y < ny; y++) for (int x = 0; x < nx; x++)
                {
                    float px = -m.W / 2 + x * s + ox - s, py = -m.H / 2 + y * s + oy - s;
                    switch (p.Pattern)
                    {
                        case GeoPattern.Dots: m.Ball(px, py, -20, s * .15f * pu, col); break;
                        case GeoPattern.Checker: if ((x + y) % 2 == 0) m.Box(px, py, -26, s, s, 10, col, .05f); break;
                        case GeoPattern.Plus: m.Box(px, py, -26, s * .5f, s * .12f, 10, col, .2f); m.Box(px, py, -26, s * .12f, s * .5f, 10, col, .2f); break;
                        case GeoPattern.Diamonds: m.Turn(px, py, MathF.PI / 4, () => m.Box(px, py, -26, s * .4f * pu, s * .4f * pu, 10, col, .1f)); break;
                        case GeoPattern.Hex: m.Torus(px + (y % 2) * s / 2, py, -26, s * .4f, c.F(p.Thickness), col); break;
                        case GeoPattern.Rings: m.Torus(px, py, -26, s * .3f * pu, c.F(p.Thickness), col); break;
                        case GeoPattern.Triangles: m.Slab(-28, 10, col, px, py - s * .3f, px + s * .3f, py + s * .25f, px - s * .3f, py + s * .25f); break;
                        default: if (x == 0) m.Turn(0, py, p.Pattern == GeoPattern.Stripes ? 0 : MathF.Sin(m.Now) * .05f, () => m.Box(0, py, -26, m.W * 1.3f, s * .25f, 10, col, .1f)); break;
                    }
                }
            }, 6);
        });
        S<BokehParameter>(d, (p, c, m) => // ボケ光: 奥行きの中にふわっと浮かぶ、半透明の丸(六角)い光の円盤
        {
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 60); i++)
            {
                float z = -1200 + R(p.Seed, i, 2) * 1300, x = (R(p.Seed, i) - .5f) * m.W * 1.4f + MathF.Sin(m.Now * .3f + i) * c.F(p.Drift) * 3, y = (R(p.Seed, i, 1) - .5f) * m.H * 1.4f + MathF.Cos(m.Now * .25f + i) * c.F(p.Drift) * 2, s = c.F(p.Size) * (.5f + R(p.Seed, i, 3)), tw = .6f + .4f * MathF.Sin(m.Now * c.F(p.Twinkle) / 60 + i);
                Vector4 col = p.Palette switch { BokehPalette.Cool => Sculpt.Mix(m.C(0x4FC3F7u), m.C(0x9575CDu), R(p.Seed, i, 4)), BokehPalette.Rainbow => Pal(m, i), BokehPalette.TwoColor => i % 2 == 0 ? m.C(p.Color1) : m.C(p.Color2), _ => Sculpt.Mix(m.C(p.Color1), m.C(p.Color2), R(p.Seed, i, 4)) };
                col = m.C(col, .35f * tw * c.F(p.Brightness) / 100);
                if (p.Hexagon) m.Slab(z, 4, col, StarPts(x, y, s, 6, .87f, 0)); else m.Disc(x, y, z, s, s, 4, col);
            }
        });
        S<DamageHitParameter>(d, (p, c, m) => // ダメージ表示: はねて上がる厚い数字、白い衝撃の星形、広がる輪
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float iv = MathF.Max(.1f, c.F(p.Interval)), life = c.F(p.Life); int nHits = (int)MathF.Ceiling(life / iv) + 1;
            for (int h = 0; h < nHits; h++)
            {
                int idx = (int)(m.Now / iv) - h; if (idx < 0) continue; float age = m.Now - idx * iv; if (age > life) continue; float k = age / life;
                bool crit = R(p.Seed, idx, 3) < c.F(p.CritChance) / 100; int val = (int)(c.F(p.Value) * (p.Vary ? .7f + .6f * R(p.Seed, idx) : 1) * (crit ? 2 : 1));
                float x = o.X + (R(p.Seed, idx, 1) - .5f) * c.F(p.Spread) * 2, y = o.Y + (R(p.Seed, idx, 2) - .5f) * c.F(p.Spread) - 120 * Ease(k) - 60 * MathF.Sin(MathF.Min(1, k * 4) * MathF.PI), dh = c.F(p.DigitHeight) * (crit ? 1.5f : 1) * (1 + .3f * MathF.Max(0, 1 - k * 6));
                m.Text(val.ToString(), dh * 1.4f, x, y, 40, dh * .25f, m.C(crit ? p.CritColor : p.NumberColor, 1 - MathF.Max(0, k - .7f) / .3f), 0, m.C(p.OutlineColor), 4);
                if (k < .25f) { float f = k / .25f; m.Slab(0, 10, m.C(p.SparkColor, 1 - f), StarPts(x, y + 60, c.F(p.SparkSize) * (.5f + f), 8, .35f, idx)); if (p.Ring) m.Torus(x, y + 60, 5, c.F(p.SparkSize) * f * 1.5f, 5, m.C(p.SparkColor, 1 - f)); }
            }
        });
        S<HealBuffParameter>(d, (p, c, m) => // 回復・強化: 足元の光の輪、立ちのぼる光の柱と、十字・矢印の粒が上る(弱体は下がる)
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), rh = c.F(p.RiseHeight), sp = c.F(p.Speed) / 100, br = c.F(p.Brightness) / 100;
            var col = p.Kind == BuffKind.Heal ? m.C(p.HealColor) : p.Kind == BuffKind.Buff ? m.C(p.BuffColor) : m.C(p.DebuffColor); bool down = p.Kind == BuffKind.Debuff;
            if (p.BaseRing) m.Pitch(o.X, o.Y, 0, MathF.PI / 2, () => { m.Torus(o.X, o.Y, 0, r, 6, m.C(col, br)); m.Torus(o.X, o.Y, 0, r * .7f, 3, m.C(col, .6f * br)); });
            if (p.Pillar) m.Lathe(o.X, 0, m.C(col, .2f * br), r * .9f, o.Y, r * .8f, o.Y - rh);
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 40); i++)
            {
                float f = Frac(m.Now * .6f * sp + R(p.Seed, i)), a = R(p.Seed, i, 1) * MathF.Tau, rr = r * (.2f + .7f * R(p.Seed, i, 2)), x = o.X + MathF.Cos(a) * rr, z = MathF.Sin(a) * rr, y = down ? o.Y - rh + f * rh : o.Y - f * rh, s = c.F(p.IconSize), al = MathF.Sin(f * MathF.PI) * br;
                if (p.Kind == BuffKind.Heal) { m.Box(x, y, z, s * 1.2f, s * .4f, s * .4f, m.C(col, al), .2f); m.Box(x, y, z, s * .4f, s * 1.2f, s * .4f, m.C(col, al), .2f); }
                else m.Slab(z - s * .15f, s * .3f, m.C(col, al), x, y + (down ? s * .6f : -s * .6f), x + s * .6f, y, x + s * .25f, y, x + s * .25f, y + (down ? -s * .6f : s * .6f), x - s * .25f, y + (down ? -s * .6f : s * .6f), x - s * .25f, y, x - s * .6f, y);
            }
        });
        S<LightSwordParameter>(d, (p, c, m) => // 光の剣: 金属の柄から伸びる光る刃、振ると残る扇形の光跡
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float L = c.F(p.Length) * Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.Ignite)), 0, 1)), bw = c.F(p.BladeWidth), fl = 1 - c.F(p.Flicker) / 100 * .2f * MathF.Abs(MathF.Sin(m.Now * 37));
            float ang = c.F(p.Angle) * MathF.PI / 180, prev = ang;
            if (p.Swing) { float iv = MathF.Max(.2f, c.F(p.SwingInterval)), st = c.F(p.SwingTime), ph = Frac(m.Now / iv) * iv; if (ph < st) { float k = Ease(ph / st); prev = ang + c.F(p.SwingAngle) * MathF.PI / 180 * MathF.Max(0, k - .3f); ang += c.F(p.SwingAngle) * MathF.PI / 180 * k; if (p.Trail) m.Slab(-4, 8, m.C(p.BladeColor, .25f), PiePts(o.X, o.Y, L + 60, prev - MathF.PI / 2, ang - MathF.PI / 2)); } else ang += c.F(p.SwingAngle) * MathF.PI / 180; }
            m.Turn(o.X, o.Y, ang, () =>
            {
                m.Cyl(o.X, o.Y + 70, 0, bw * 1.3f, 140, m.C(p.HiltColor), .3f); for (int i = 0; i < 4; i++) m.Lathe(o.X, 0, m.C(0x2B2F36u), bw * 1.4f, o.Y + 30 + i * 30, bw * 1.4f, o.Y + 40 + i * 30); m.Box(o.X, o.Y, 0, bw * 4, 14, bw * 2, m.C(p.HiltColor), .3f);
                if (L > 2) { m.Limb(o.X, o.Y, 0, o.X, o.Y - L, 0, bw * .5f, bw * .45f, m.C(0xFFFFFFu, fl)); m.Limb(o.X, o.Y, 0, o.X, o.Y - L, 0, bw * 1.2f, bw, m.C(p.BladeColor, .55f * fl * c.F(p.Glow) / 100)); m.Limb(o.X, o.Y, 0, o.X, o.Y - L, 0, bw * 2.2f, bw * 2, m.C(p.BladeColor, .18f * fl * c.F(p.Glow) / 100)); }
            });
        });
        S<MuzzleFlashParameter>(d, (p, c, m) => // マズルフラッシュ: 銃口から十字に開く火の板、飛ぶ薬きょう、壁にあく弾痕
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float dir = c.F(p.Direction) * MathF.PI / 180, rate = MathF.Max(.5f, c.F(p.FireRate)), ph = Frac(m.Now * rate), fs = c.F(p.FlashSize); int shot = (int)(m.Now * rate);
            m.Turn(o.X, o.Y, dir, () =>
            {
                m.Box(o.X - 160, o.Y, 0, 320, 40, 40, m.C(0x37474Fu), .2f); m.Box(o.X - 280, o.Y + 60, 0, 60, 120, 36, m.C(0x263238u), .2f);
                if (ph < .35f) { float k = 1 - ph / .35f; m.Turn(o.X, o.Y, shot, () => m.Slab(-6, 12, m.C(p.FlashColor, k), StarPts(o.X + fs * .5f, o.Y, fs * k, 4, .3f, 0))); m.Lathe(o.X + fs * .5f, 0, m.C(0xFFFFFFu, k), 0, o.Y - fs * .3f * k, fs * .2f * k, o.Y, 0, o.Y + fs * .3f * k); m.Ball(o.X + fs * .5f, o.Y, 0, fs * .5f * k, m.C(p.FlashColor, .4f * k)); }
                if (p.Shells) for (int i = 0; i < 4; i++) { float age = Frac(m.Now * rate / 4 + i / 4f) * 4 / rate; m.Turn(o.X - 160 + age * 200, o.Y - 30 - age * 300 + 900 * age * age, age * 20, () => m.Cyl(o.X - 160 + age * 200, o.Y - 30 - age * 300 + 900 * age * age, age * 300, 6, 22, m.C(0xD4AF37u), .3f)); }
                if (p.Smoke) for (int i = 0; i < 5; i++) { float f = Frac(m.Now * .5f + i / 5f); m.Ball(o.X + fs * .4f + f * 120, o.Y - f * 140, 0, 30 + 60 * f, m.C(0xB0B0B0u, .3f * (1 - f))); }
            });
            if (p.Holes) { var t = PctPos(m, c.F(p.TargetX), c.F(p.TargetY)); int n = Math.Min(Math.Min((int)c.F(p.HoleCount), shot + 1), 30); for (int i = 0; i < n; i++) { float x = t.X + (R(p.Seed, i) - .5f) * c.F(p.TargetSpread), y = t.Y + (R(p.Seed, i, 1) - .5f) * c.F(p.TargetSpread), s = c.F(p.HoleSize); m.Torus(x, y, -200, s, s * .4f, m.C(0x5D4037u)); m.Disc(x, y, -205, s, s, 6, K); } }
        });
        S<BlackHoleParameter>(d, (p, c, m) => // ブラックホール: まっ黒な球と、傾いて回る光る降着円盤、光の輪
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), ds = c.F(p.DiskSize), sp = m.Now * c.F(p.Spin) / 100, br = c.F(p.Brightness) / 100;
            m.Box(0, 0, -1500, m.W * 3, m.H * 3, 10, m.C(0x02030Au, c.F(p.BgOpacity) / 100), .01f);
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.StarCount), 220); i++) m.Ball((R(p.Seed, i) - .5f) * m.W * 2.5f, (R(p.Seed, i, 1) - .5f) * m.H * 2.5f, -1450, 3, m.C(0xFFFFFFu, .6f)); }, 5);
            m.Turn(o.X, o.Y, c.F(p.Rotation) * MathF.PI / 180, () => m.Pitch(o.X, o.Y, 0, MathF.PI / 2 - c.F(p.Tilt) * MathF.PI / 180, () => m.Turn(o.X, o.Y, sp, () =>
            {
                for (int i = 0; i < 10; i++) { float f = i / 10f, rr = r * 1.3f + (ds - r * 1.3f) * f; m.Torus(o.X, o.Y, 0, rr, (ds - r) / 18, m.C(Sculpt.Mix(m.C(p.InnerColor), m.C(p.DiskColor), f), (1 - f * .7f) * br)); }
                for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12; m.Ball(o.X + MathF.Cos(a) * ds * .7f, o.Y + MathF.Sin(a) * ds * .7f, 0, 14, 6, 4, m.C(p.InnerColor, br)); }
            })));
            m.Torus(o.X, o.Y, 0, r * 1.15f, r * .08f, m.C(p.InnerColor, .8f * br));
            m.Ball(o.X, o.Y, 0, r, m.C(0x000000u));
        });
        S<CircleVisualizerParameter>(d, (p, c, m) => // 円形ビジュアライザー: 輪に並んで拍に合わせて伸びる角柱のバー、中心の円盤
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), bl = c.F(p.BarLength), bw = c.F(p.BarWidth) * 1.5f, beat = MathF.Pow(1 - Frac(m.Now * c.F(p.Bpm) / 60), 3), en = c.F(p.Energy) / 100, rot = m.Now * c.F(p.RotSpeed) * MathF.PI / 180;
            int n = Math.Clamp((int)c.F(p.Bars), 8, 128);
            m.Turn(o.X, o.Y, rot, () =>
            {
                for (int i = 0; i < n; i++)
                {
                    int j = p.Symmetric ? Math.Min(i, n - i) : i; float v = (.15f + .85f * MathF.Abs(MathF.Sin(j * .37f + m.Now * 3) * MathF.Sin(j * .11f + m.Now * 1.3f))) * (.6f + .4f * beat) * en, a = i * MathF.Tau / n, L = bl * v;
                    var col = Sculpt.Mix(m.C(p.Color1), m.C(p.Color2), i / (float)n);
                    m.Turn(o.X, o.Y, a, () => { m.Box(o.X + r + L / 2, o.Y, 0, L, bw, bw * 2, col, .2f); if (p.Mirror) m.Box(o.X + r - L * .25f, o.Y, 0, L * .5f, bw, bw * 2, m.C(col, .5f), .2f); });
                }
            });
            if (p.Center) { m.Disc(o.X, o.Y, -10, r * .9f * (1 + beat * .05f), r * .9f * (1 + beat * .05f), 40, m.C(0x111522u), .4f); m.Torus(o.X, o.Y, 12, r * .85f, 6, m.C(p.Color1, c.F(p.Glow) / 100)); }
        });
        S<CyberGlobeParameter>(d, (p, c, m) => // サイバー地球儀: 緯線・経線の輪でできた回る球、都市の点と、点の間を結ぶ光の弧
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), th = c.F(p.Thickness) * 2, spin = m.Now * c.F(p.Spin) * MathF.PI / 180;
            m.Ball(o.X, o.Y, 0, r * .98f, m.C(p.Color, .08f));
            m.Turn(o.X, o.Y, c.F(p.Tilt) * MathF.PI / 180, () => m.Yaw(o.X, o.Y, 0, spin, () =>
            {
                int lat = Math.Clamp((int)c.F(p.LatLines), 2, 20), lon = Math.Clamp((int)c.F(p.LonLines), 2, 30);
                for (int i = 1; i < lat; i++) { float a = -MathF.PI / 2 + i * MathF.PI / lat; m.Pitch(o.X, o.Y + MathF.Sin(a) * r, 0, MathF.PI / 2, () => m.Torus(o.X, o.Y + MathF.Sin(a) * r, 0, MathF.Cos(a) * r, th, m.C(p.Color, .7f))); }
                for (int i = 0; i < lon; i++) m.Yaw(o.X, o.Y, 0, i * MathF.PI / lon, () => m.Torus(o.X, o.Y, 0, r, th, m.C(p.Color, .5f)));
                var cities = new List<Vector3>(); for (int i = 0; i < 14; i++) { float la = (R(p.Seed, i) - .5f) * 2.4f, lo = R(p.Seed, i, 1) * MathF.Tau; cities.Add(V3(o.X + MathF.Cos(la) * MathF.Cos(lo) * r, o.Y - MathF.Sin(la) * r, MathF.Cos(la) * MathF.Sin(lo) * r)); if (p.Cities) m.Ball(cities[i].X, cities[i].Y, cities[i].Z, 8, m.C(p.ArcColor)); }
                for (int i = 0; i < Math.Min((int)c.F(p.Arcs), 20); i++)
                {
                    var a = cities[i % 14]; var b = cities[(i * 5 + 3) % 14]; float f = Frac(m.Now * c.F(p.ArcSpeed) / 200 + i / 7f); var pts = new List<Vector3>(); var ctr = V3(o.X, o.Y, 0);
                    for (int k = 0; k <= 16; k++) { float u = k / 16f * MathF.Min(1, f * 2); var q = Vector3.Lerp(a, b, u) - ctr; pts.Add(ctr + Vector3.Normalize(q) * r * (1 + MathF.Sin(u * MathF.PI) * c.F(p.ArcLift) / 100)); }
                    if (pts.Count > 1) m.Tube(pts, 3, m.C(p.ArcColor, 1 - MathF.Max(0, f - .5f) * 2));
                }
            }));
            if (p.Ring) m.Pitch(o.X, o.Y, 0, 1.3f, () => m.Torus(o.X, o.Y, 0, r * 1.3f, 4, m.C(p.Color, .5f)));
        });
    }
}
