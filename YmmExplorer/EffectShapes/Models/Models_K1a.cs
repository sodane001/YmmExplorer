using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 01〜30番(天気・空・水・火)
//  粒は奥行き(z)にもばらまいて、回すと空間に広がって見えるようにする
// =====================================================================
internal static partial class Models3D
{
    /// <summary>粒の多い図形用: 円の分割を減らして描く</summary>
    static void Low(Sculpt m, Action draw, int seg = 8) { int s = m.Seg; m.Seg = seg; try { draw(); } finally { m.Seg = s; } }
    /// <summary>くり返し落ちる位置(0〜span)</summary>
    static float Cyc(float v, float span) => ((v % span) + span) % span;
    /// <summary>ふわっとした雲のかたまり(玉を重ねる)</summary>
    static void CloudPuff(Sculpt m, float x, float y, float z, float s, Vector4 lit, Vector4 shade, int seed)
    {
        m.Ball(x, y + s * .25f, z, s * 1.3f, s * .45f, s * .7f, shade);
        for (int i = 0; i < 5; i++) { float a = (i - 2) * .55f; m.Ball(x + a * s, y - MathF.Cos(a) * s * .3f, z + R(seed, i) * s * .3f, s * (.55f + .25f * R(seed, i, 1)), lit); }
    }
    /// <summary>空のグラデーションの奥の壁(上→下に色を変える細い帯の重なり)</summary>
    static void SkyBands(Sculpt m, Vector4 top, Vector4 bottom, float z = -1500, int n = 10)
    {
        float s = 1 + -z / 1600f;
        for (int i = 0; i < n; i++) { float f = (i + .5f) / n; m.Box(0, (f - .5f) * m.H * s, z, m.W * s * 1.05f, m.H * s / n + 2, 4, Sculpt.Mix(top, bottom, f), .01f); }
    }

    static partial void RegisterK1a(Dictionary<Type, SceneModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        // ---------------------------------------------------------------- 01 雨
        S<RainParameter>(d, (p, c, m) => // 雨: 奥行きのある空間に、斜めに落ちる細い雨の線
        {
            int n = Math.Min((int)c.F(p.Count), 260); float len = c.F(p.Length), spd = c.F(p.Speed), ang = c.F(p.Angle) * MathF.PI / 180, th = MathF.Max(1.5f, c.F(p.Thickness)), op = c.F(p.Opacity) / 100;
            var col = m.C(p.Color, op); float dx = MathF.Sin(ang), dy = MathF.Cos(ang);
            Low(m, () => { for (int i = 0; i < n; i++) { float z = -900 + R(p.Seed, i, 2) * 1200, dd = .5f + .5f * R(p.Seed, i, 3), y = -m.H / 2 - len + Cyc(m.Now * spd * dd + R(p.Seed, i, 1) * m.H * 1.3f, m.H * 1.3f), x = (R(p.Seed, i) - .5f) * m.W * 1.4f + y * MathF.Tan(ang); m.Limb(x, y, z, x + dx * len, y + dy * len, z, th, th * .6f, col); } }, 5);
        });
        S<RainRippleParameter>(d, (p, c, m) => // 雨の波紋: 手前に広がる水面の床と、輪になって広がる波紋
        {
            float top = (c.F(p.FloorTop) / 100 - .5f) * m.H, sz = c.F(p.Size), rate = MathF.Max(1, c.F(p.Rate)); int rings = Math.Clamp(p.Rings, 1, 6);
            m.Box(0, m.H / 2 + 10, -300, m.W * 1.6f, 20, 1600, m.C(0x223344, .6f), .01f);
            int n = (int)Math.Min(rate * 1.6f, 40);
            for (int i = 0; i < n; i++)
            {
                float ph = Frac(m.Now * rate / n + i / (float)n), x = (R(p.Seed, i) - .5f) * m.W, z = -1000 + R(p.Seed, i, 1) * 1100;
                for (int r = 0; r < rings; r++) { float f = ph - r * .12f; if (f <= 0) continue; m.Pitch(x, m.H / 2, z, MathF.PI / 2, () => m.Torus(x, m.H / 2, z, sz * f, 3, m.C(p.RippleColor, (1 - f) * .9f))); }
            }
            if (p.ShowRain) Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.RainCount), 150); i++) { float y = -m.H / 2 + Cyc(m.Now * 1600 + R(p.Seed, i, 4) * m.H, m.H), x = (R(p.Seed, i, 5) - .5f) * m.W * 1.2f, z = -900 + R(p.Seed, i, 6) * 1000; m.Limb(x, y, z, x + 6, y + 50, z, 2, 1.5f, m.C(p.RippleColor, .6f)); } }, 5);
        });
        S<SnowParameter>(d, (p, c, m) => // 雪: 奥行きの中をゆらゆら落ちる白い玉
        {
            int n = Math.Min((int)c.F(p.Count), 260); float sz = c.F(p.Size), spd = c.F(p.Speed), sw = c.F(p.Sway);
            Low(m, () => { for (int i = 0; i < n; i++) { float dd = .5f + .5f * R(p.Seed, i, 3), y = -m.H / 2 - 20 + Cyc(m.Now * spd * dd + R(p.Seed, i, 1) * m.H * 1.2f, m.H * 1.2f), x = (R(p.Seed, i) - .5f) * m.W * 1.2f + MathF.Sin(m.Now * 1.3f + i) * sw, z = -900 + R(p.Seed, i, 2) * 1200; m.Ball(x, y, z, sz * dd * 1.2f, m.C(p.Color, c.F(p.Opacity) / 100)); } });
        });
        S<SnowflakeParameter>(d, (p, c, m) => // 雪の結晶: 6本の腕と小枝のある、回りながら落ちる立体の結晶
        {
            int n = Math.Min((int)c.F(p.Count), 60); float sz = c.F(p.Size) * 1.4f, th = MathF.Max(3, c.F(p.Thickness) * 2.5f);
            var col = m.C(p.Color);
            for (int i = 0; i < n; i++)
            {
                float y = -m.H / 2 - sz + Cyc(m.Now * c.F(p.Fall) * (.6f + .4f * R(p.Seed, i, 3)) + R(p.Seed, i, 1) * m.H * 1.2f, m.H * 1.2f), x = (R(p.Seed, i) - .5f) * m.W + MathF.Sin(m.Now + i) * c.F(p.Sway), z = -800 + R(p.Seed, i, 2) * 1000, sp = m.Now * c.F(p.Spin) / 60 + i;
                m.Yaw(x, y, z, sp * .7f, () => m.Turn(x, y, sp, () => Low(m, () =>
                {
                    for (int k = 0; k < 6; k++) { float a = k * MathF.PI / 3, ca = MathF.Cos(a), sa = MathF.Sin(a); m.Limb(x, y, z, x + ca * sz, y + sa * sz, z, th, th * .7f, col); foreach (int s in Sides) { float b = a + s * .6f; m.Limb(x + ca * sz * .55f, y + sa * sz * .55f, z, x + ca * sz * .55f + MathF.Cos(b) * sz * .35f, y + sa * sz * .55f + MathF.Sin(b) * sz * .35f, z, th * .7f, th * .5f, col); } }
                    m.Ball(x, y, z, th * 1.6f, col);
                }, 6)));
            }
        });
        S<BlizzardParameter>(d, (p, c, m) => // 吹雪: 横なぐりに流れる雪の筋と、白くかすむ層
        {
            int n = Math.Min((int)c.F(p.Count), 300); float wind = c.F(p.Wind), fall = c.F(p.Fall), st = c.F(p.Streak) / 100 * 60;
            for (int k = 0; k < 3; k++) m.Box(0, 0, -1200 + k * 450, m.W * 2, m.H * 2, 4, m.C(p.SnowColor, c.F(p.Veil) / 100 * .2f), .01f);
            Low(m, () => { for (int i = 0; i < n; i++) { float span = m.W * 1.6f, x = -span / 2 + Cyc(m.Now * wind * (.6f + .4f * R(p.Seed, i, 3)) + R(p.Seed, i) * span, span), y = -m.H / 2 + Cyc(m.Now * fall + R(p.Seed, i, 1) * m.H, m.H), z = -1000 + R(p.Seed, i, 2) * 1300; float s = c.F(p.Size) * 2; m.Limb(x - st, y - st * fall / MathF.Max(1, wind), z, x, y, z, s * .5f, s, m.C(p.SnowColor, .9f)); } }, 5);
        });
        S<CloudParameter>(d, (p, c, m) => // 雲: 玉を重ねたもくもくの雲が、奥行きをもって流れる
        {
            int n = Math.Min((int)c.F(p.Count) / 3, 20); float sz = c.F(p.Size), y0 = (c.F(p.PosY) / 100 - .5f) * m.H, spr = c.F(p.Spread) / 100 * m.H;
            for (int i = 0; i < n; i++) { float span = m.W * 1.8f, x = -span / 2 + Cyc(m.Now * c.F(p.Speed) * (.5f + R(p.Seed, i, 3)) + R(p.Seed, i) * span, span), y = y0 + (R(p.Seed, i, 1) - .5f) * spr, z = -1200 + R(p.Seed, i, 2) * 1100; CloudPuff(m, x, y, z, sz * (.6f + .5f * R(p.Seed, i, 4)), m.C(p.Color, c.F(p.Opacity) / 100), m.C(p.ShadowColor, c.F(p.Opacity) / 100), p.Seed + i); }
        });
        S<LightningParameter>(d, (p, c, m) => // 雷: 上の雲から、ジグザグに枝分かれして落ちる光の筋
        {
            float iv = MathF.Max(.3f, c.F(p.Interval)), ph = Frac(m.Now / iv) * iv, on = ph < c.F(p.Duration) ? 1 - ph / MathF.Max(.05f, c.F(p.Duration)) : 0; int bolt = (int)(m.Now / iv);
            CloudPuff(m, 0, -m.H / 2 + 60, -400, 340, m.C(0x5A6070), m.C(0x30343E), 3);
            if (on <= 0) return;
            float th = c.F(p.Thickness) * 2, jag = c.F(p.Jag);
            void Bolt(float x, float y, float z, float len, int depth, int s)
            {
                var pts = new List<Vector3> { V3(x, y, z) }; int seg = 10;
                for (int i = 1; i <= seg; i++) pts.Add(V3(x + (R(s, i) - .5f) * jag * 2 + i * 8, y + len * i / seg, z + (R(s, i, 1) - .5f) * jag * 2));
                m.Tube(pts, th * (depth == 0 ? 1 : .55f), m.C(p.Color, on)); m.Tube(pts, th * 2.4f * (depth == 0 ? 1 : .55f), m.C(p.GlowColor, on * .3f));
                if (depth == 0) for (int b = 0; b < Math.Min((int)c.F(p.Branches), 6); b++) { var q = pts[2 + b % 6]; Bolt(q.X, q.Y, q.Z, len * .4f, 1, s * 7 + b); }
            }
            Bolt((R(p.Seed, bolt) - .5f) * m.W * .6f, -m.H / 2 + 100, -400, m.H * .9f, 0, p.Seed * 31 + bolt);
        });
        S<FogParameter>(d, (p, c, m) => // 霧: 低いところに重なってただよう、薄い楕円の霧のかたまり
        {
            int n = Math.Min((int)c.F(p.Count), 50); float sz = c.F(p.Size), den = c.F(p.Density) / 100, gy = m.H / 2 - c.F(p.Ground) / 100 * m.H;
            for (int i = 0; i < n; i++) { float span = m.W * 1.6f, x = -span / 2 + Cyc(m.Now * c.F(p.Speed) * (.5f + R(p.Seed, i, 3)) + R(p.Seed, i) * span, span), y = gy + R(p.Seed, i, 1) * (m.H / 2 - gy), z = -1200 + R(p.Seed, i, 2) * 1300; m.Ball(x, y, z, sz * 1.6f, sz * .45f, sz * .8f, m.C(p.FogColor, den * .28f)); }
        });
        S<RainbowParameter>(d, (p, c, m) => // 虹: 7色の太い半円の帯が、根元から伸びて架かる
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), band = c.F(p.Band), st = c.F(p.Strength) / 100, k = p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.1f, c.F(p.IntroTime)), 0, 1)) * c.F(p.Appear) / 100 : c.F(p.Appear) / 100;
            uint[] cols = { 0xFF3B30, 0xFF9500, 0xFFCC00, 0x34C759, 0x32ADE6, 0x5856D6, 0xAF52DE };
            for (int i = 0; i < 7; i++) m.Tube(ArcPts3(o.X, o.Y, r - i * band / 7, MathF.PI, MathF.PI + MathF.PI * k, -400, 40), band / 14, m.C(cols[i], st * .9f));
            if (p.DoubleRainbow) for (int i = 0; i < 7; i++) m.Tube(ArcPts3(o.X, o.Y, r * 1.3f + i * band / 10, MathF.PI, MathF.PI + MathF.PI * k, -500, 40), band / 22, m.C(cols[i], st * .35f));
            if (p.Mist) foreach (int s in Sides) m.Ball(o.X + s * (r - band / 2), o.Y, -400, band * 1.2f, band * .5f, band, m.C(0xFFFFFF, .25f));
        });
        S<WindParameter>(d, (p, c, m) => // 風: 波打って流れる白い流線と、くるくる回る渦、舞う葉っぱ
        {
            float dir = c.F(p.Direction) * MathF.PI / 180, amp = c.F(p.WaveAmp), per = MathF.Max(50, c.F(p.WavePeriod)), len = c.F(p.Length), spd = c.F(p.Speed), st = c.F(p.Strength) / 100;
            m.Turn(0, 0, dir, () =>
            {
                for (int i = 0; i < Math.Min((int)c.F(p.Count), 40); i++)
                {
                    float y0 = (R(p.Seed, i) - .5f) * m.H, z = -800 + R(p.Seed, i, 1) * 1000, span = m.W + len * 2, hx = -m.W / 2 - len + Cyc(m.Now * spd * (.6f + .4f * R(p.Seed, i, 2)) + R(p.Seed, i, 3) * span, span);
                    var pts = new List<Vector3>(); for (int k = 0; k <= 10; k++) { float x = hx - len * k / 10; pts.Add(V3(x, y0 + MathF.Sin(x / per * MathF.Tau + i) * amp, z)); }
                    m.Tube(pts, c.F(p.Thickness) * 1.5f, m.C(p.WindColor, st * .7f));
                }
                for (int v = 0; v < Math.Min((int)c.F(p.VortexCount), 8); v++) { float x = (R(p.Seed, v, 5) - .5f) * m.W * .8f, y = (R(p.Seed, v, 6) - .5f) * m.H * .6f, r = c.F(p.VortexSize); var sp = new List<Vector3>(); for (int k = 0; k <= 30; k++) { float a = k * .35f + m.Now * 4, rr = r * k / 30; sp.Add(V3(x + MathF.Cos(a) * rr, y + MathF.Sin(a) * rr, -300 + k * 4)); } m.Tube(sp, 3, m.C(p.WindColor, st * .6f)); }
                for (int i = 0; i < Math.Min((int)c.F(p.ParticleCount), 50); i++) { float span = m.W * 1.4f, x = -span / 2 + Cyc(m.Now * spd * .7f + R(p.Seed, i, 7) * span, span), y = (R(p.Seed, i, 8) - .5f) * m.H + MathF.Sin(m.Now * 3 + i) * 60, z = -600 + R(p.Seed, i, 9) * 800; m.Turn(x, y, m.Now * 5 + i, () => m.Ball(x, y, z, 14, 6, 3, m.C(p.LeafColor))); }
            });
        });
        S<VortexParameter>(d, (p, c, m) => // 竜巻: 下が細く上が広い、ねじれて回る粒の柱
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float top = c.F(p.TopRadius), hgt = m.H * .85f, spd = c.F(p.Speed) / 100;
            int n = Math.Min((int)c.F(p.Count), 360);
            Low(m, () =>
            {
                for (int i = 0; i < n; i++)
                {
                    float f = R(p.Seed, i), a = R(p.Seed, i, 1) * MathF.Tau + m.Now * spd * (6 - f * 3), r = 30 + top * f * f, y = o.Y - hgt * f, sw = MathF.Sin(m.Now * 1.5f + f * 3) * c.F(p.Sway) * f;
                    m.Ball(o.X + sw + MathF.Cos(a) * r, y, -300 + MathF.Sin(a) * r, 8 + 10 * f, m.C(p.Color, .55f));
                }
            }, 5);
            m.Lathe(o.X, -300, m.C(p.Color, .25f), 30, o.Y, top * .5f, o.Y - hgt * .5f, top, o.Y - hgt);
        });
        S<SandstormParameter>(d, (p, c, m) => // 砂嵐: 横に流れる砂粒の筋と、黄色くかすむ層
        {
            for (int k = 0; k < 4; k++) m.Box(0, 0, -1300 + k * 400, m.W * 2.2f, m.H * 2.2f, 4, m.C(p.SandColor, c.F(p.Haze) / 100 * .18f), .01f);
            int n = Math.Min((int)c.F(p.Count), 320); float wind = c.F(p.Wind), st = c.F(p.Streak) / 100 * 50;
            Low(m, () => { for (int i = 0; i < n; i++) { float span = m.W * 1.6f, x = -span / 2 + Cyc(m.Now * wind * (.5f + .5f * R(p.Seed, i, 3)) + R(p.Seed, i) * span, span), y = (R(p.Seed, i, 1) - .5f) * m.H + MathF.Sin(m.Now * 2 + i) * 20, z = -1100 + R(p.Seed, i, 2) * 1400; m.Limb(x - st, y, z, x, y, z, 2, c.F(p.Size) * 2, m.C(p.SandColor, .85f)); } }, 5);
        });
        S<SkyParameter>(d, (p, c, m) => // 空: 奥のグラデーションの空、光る太陽の玉、浮かぶ雲
        {
            SkyBands(m, m.C(p.TopColor), m.C(p.HorizonColor), -1600, 12);
            if (p.ShowSun) { var s = PctPos(m, c.F(p.SunX), c.F(p.SunY)) * 1.9f; float r = c.F(p.SunSize) * 2.2f; m.Ball(s.X, s.Y, -1500, r, m.C(p.SunColor)); m.Ball(s.X, s.Y, -1520, r * 2.2f, m.C(p.SunColor, .2f * c.F(p.SunGlow) / 100)); }
            int n = Math.Min((int)c.F(p.CloudCount) / 3, 16);
            for (int i = 0; i < n; i++) { float span = m.W * 2, x = -span / 2 + Cyc(m.Now * c.F(p.CloudSpeed) * (.5f + R(p.Seed, i, 3)) + R(p.Seed, i) * span, span), y = (c.F(p.CloudY) / 100 - .5f) * m.H + (R(p.Seed, i, 1) - .5f) * m.H * .4f, z = -1300 + R(p.Seed, i, 2) * 1000; CloudPuff(m, x, y, z, c.F(p.CloudSize) * (.5f + .5f * R(p.Seed, i, 4)), m.C(p.CloudLit, c.F(p.CloudOpacity) / 100), m.C(p.CloudShade, c.F(p.CloudOpacity) / 100), i); }
        });
        S<SunParameter>(d, (p, c, m) => // 太陽: 光る球と、まわりを回る光芒の板、ななめに差す光の筋
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius) * 1.6f, it = c.F(p.Intensity) / 100;
            m.Ball(o.X, o.Y, -200, r, m.C(p.CoreColor)); m.Ball(o.X, o.Y, -220, r * 1.6f, m.C(p.GlowColor, .35f * it)); m.Ball(o.X, o.Y, -240, r * 2.6f, m.C(p.GlowColor, .15f * it));
            int rc = Math.Min((int)c.F(p.RayCount), 24); float rl = c.F(p.RayLength), rot = (c.F(p.RayRotation) + m.Now * c.F(p.RaySpin) * 6) * MathF.PI / 180;
            for (int i = 0; i < rc; i++) { float a = rot + i * MathF.Tau / rc; m.Turn(o.X, o.Y, a, () => m.Slab(-212, 4, m.C(p.GlowColor, c.F(p.RayOpacity) / 100), o.X + r, o.Y - 14, o.X + r + rl, o.Y, o.X + r, o.Y + 14)); }
            int sc = Math.Min((int)c.F(p.ShaftCount), 12); float sa = c.F(p.ShaftAngle) * MathF.PI / 180;
            for (int i = 0; i < sc; i++) { float a = MathF.PI / 2 + sa + (i - sc / 2f) * c.F(p.ShaftSpread) / 100 * .15f, L = c.F(p.ShaftLength); m.Limb(o.X, o.Y, -200, o.X + MathF.Cos(a) * L, o.Y + MathF.Sin(a) * L, 300, 10, c.F(p.ShaftWidth) * .6f, m.C(p.CoreColor, c.F(p.ShaftOpacity) / 100 * .35f)); }
        });
        S<NightSkyParameter>(d, (p, c, m) => // 夜空: 奥の暗い空、天の川の粒の帯、満ちた月と星
        {
            SkyBands(m, m.C(p.TopColor), m.C(p.HorizonColor), -1600, 12);
            m.Ball((c.F(p.GlowX) / 100 - .5f) * m.W * 1.9f, m.H * .95f, -1550, m.W * .6f, m.H * .35f, 100, m.C(p.GlowColor, c.F(p.GlowStrength) / 100 * .5f));
            Low(m, () =>
            {
                for (int i = 0; i < Math.Min((int)c.F(p.StarCount), 300); i++) { float tw = .6f + .4f * MathF.Sin(m.Now * 3 + i); m.Ball((R(p.Seed, i) - .5f) * m.W * 1.9f, (R(p.Seed, i, 1) - .6f) * m.H * 1.7f, -1500 + R(p.Seed, i, 2) * 600, 2 + R(p.Seed, i, 3) * 4, m.C(0xFFFFFF, tw * c.F(p.StarBrightness) / 100)); }
                float ma = c.F(p.MilkyAngle) * MathF.PI / 180;
                for (int i = 0; i < (int)(c.F(p.MilkyWay) * 2.5f); i++) { float u = (R(p.Seed, i, 4) - .5f) * m.W * 2, w = (R(p.Seed, i, 5) + R(p.Seed, i, 6) - 1) * c.F(p.MilkyWidth); m.Ball(u * MathF.Cos(ma) - w * MathF.Sin(ma), u * MathF.Sin(ma) + w * MathF.Cos(ma), -1450, 3, m.C(0xDDE4FF, .6f)); }
            }, 5);
            var mo = PctPos(m, c.F(p.MoonX), c.F(p.MoonY)); float mr = c.F(p.MoonSize) * 1.5f;
            m.Ball(mo.X, mo.Y, -600, mr, m.C(p.MoonColor)); m.Ball(mo.X, mo.Y, -620, mr * 1.8f, m.C(p.MoonColor, c.F(p.MoonGlow) / 100 * .2f));
            for (int i = 0; i < 4; i++) m.Ball(mo.X + (R(p.Seed, i, 7) - .5f) * mr, mo.Y + (R(p.Seed, i, 8) - .5f) * mr, -600 + mr * .85f, mr * .15f, m.C(Sculpt.Dark(m.C(p.MoonColor), .85f), 1));
        });
        S<StarsParameter>(d, (p, c, m) => // 星: 奥行きのある空間にまたたく光の玉、ときどき流れ星
        {
            Low(m, () =>
            {
                int n = Math.Min((int)c.F(p.Count), 300);
                for (int i = 0; i < n; i++) { float tw = .5f + .5f * MathF.Sin(m.Now * c.F(p.TwinkleSpeed) / 30 + i * 1.7f), sz = (1 + R(p.Seed, i, 3) * c.F(p.MaxSize)) * 1.5f; var col = Sculpt.Mix(m.C(p.StarColor), Pal(m, i), R(p.Seed, i, 4) * c.F(p.ColorVariation) / 200); m.Ball((R(p.Seed, i) - .5f) * m.W * 1.5f, (R(p.Seed, i, 1) - .5f) * m.H * 1.5f, -1400 + R(p.Seed, i, 2) * 1300, sz, m.C(col, tw * c.F(p.Brightness) / 100)); }
            }, 5);
            float iv = MathF.Max(.5f, c.F(p.ShootingInterval)), f = Frac(m.Now / iv) * iv; if (f < .8f) { float k = f / .8f, L = c.F(p.ShootingLength); var h = V3(m.W * (.3f - k * .7f), -m.H * (.4f - k * .5f), -500); m.Limb(h.X, h.Y, h.Z, h.X + L * .8f, h.Y - L * .5f, h.Z, 5, 1, m.C(0xFFFFFF, 1 - k)); }
        });
        S<MeteorShowerParameter>(d, (p, c, m) => // 流星群: 光る頭と長い尾の流れ星が、次々と斜めに降る
        {
            float rate = MathF.Max(.2f, c.F(p.Rate)), L = c.F(p.Length), spd = c.F(p.Speed), ang = c.F(p.Angle) * MathF.PI / 180;
            int n = (int)Math.Min(rate * 6, 30);
            for (int i = 0; i < n; i++)
            {
                float life = (m.W + L) / spd, ph = Frac(m.Now / (n / rate) + i / (float)n) * (n / rate); if (ph > life) continue;
                float a = ang + (R(p.Seed, i, 4) - .5f) * c.F(p.AngleJitter) * MathF.PI / 90, sx = (R(p.Seed, i) - .3f) * m.W, sy = -m.H / 2 - 50, z = -900 + R(p.Seed, i, 2) * 800;
                var h = V3(sx - MathF.Cos(a) * ph * spd * 0, sy + ph * spd * MathF.Cos(a), z) + V3(-MathF.Sin(a) * ph * spd, 0);
                m.Limb(h.X, h.Y, h.Z, h.X + MathF.Sin(a) * L, h.Y - MathF.Cos(a) * L, h.Z, 7, 1, m.C(p.TailColor, .7f)); m.Ball(h.X, h.Y, h.Z, 10, m.C(p.HeadColor));
            }
        });
        S<ConstellationParameter>(d, (p, c, m) => // 星座: 光る星の玉を、線の棒でつないだ立体の星座
        {
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.BgStars), 200); i++) m.Ball((R(p.Seed, i, 9) - .5f) * m.W * 1.6f, (R(p.Seed, i, 8) - .5f) * m.H * 1.6f, -1400 + R(p.Seed, i, 7) * 400, 3, m.C(p.StarColor, .5f)); }, 5);
            int cc = Math.Clamp((int)c.F(p.ConstCount), 1, 8), sp = Math.Clamp((int)c.F(p.StarsPer), 2, 12); float spr = c.F(p.Spread), k = p.Intro ? Math.Clamp(m.Now / MathF.Max(.1f, c.F(p.IntroTime)), 0, 1) : 1;
            for (int g = 0; g < cc; g++)
            {
                float cx = ((g + .5f) / cc - .5f) * m.W * .85f, cy = (R(p.Seed, g, 1) - .5f) * m.H * .5f; var pts = new List<Vector3>();
                for (int i = 0; i < sp; i++) pts.Add(V3(cx + (R(p.Seed * 7 + g, i) - .5f) * spr * 2, cy + (R(p.Seed * 7 + g, i, 1) - .5f) * spr * 2, -300 + (R(p.Seed * 7 + g, i, 2) - .5f) * spr));
                for (int i = 0; i < sp; i++) { float f = Math.Clamp(k * sp - i, 0, 1); if (f <= 0) break; m.Ball(pts[i].X, pts[i].Y, pts[i].Z, c.F(p.StarSize) * 3, m.C(p.StarColor)); if (i > 0) m.Limb(pts[i - 1], Vector3.Lerp(pts[i - 1], pts[i], f), 3, 3, m.C(p.LineColor, .8f)); }
                if (p.ShowRing) m.Torus(cx, cy, -300, spr * 1.3f, 2, m.C(p.LineColor, .3f * k));
            }
        });
        S<SpaceParameter>(d, (p, c, m) => // 宇宙: 星雲の雲、渦巻く銀河の粒、輪のある惑星
        {
            SkyBands(m, m.C(p.BgTop, c.F(p.BgOpacity) / 100), m.C(p.BgBottom, c.F(p.BgOpacity) / 100), -1800, 8);
            var neb = new[] { p.Nebula1, p.Nebula2, p.Nebula3 }; for (int i = 0; i < 9; i++) m.Ball((R(p.Seed, i, 11) - .5f) * m.W * 1.6f, (R(p.Seed, i, 12) - .5f) * m.H * 1.4f, -1600, c.F(p.NebulaSize) * (.6f + R(p.Seed, i, 13)), c.F(p.NebulaSize) * .5f, 80, m.C(neb[i % 3], c.F(p.NebulaStrength) / 100 * .25f));
            Low(m, () =>
            {
                for (int i = 0; i < Math.Min((int)c.F(p.StarCount), 260); i++) m.Ball((R(p.Seed, i) - .5f) * m.W * 2, (R(p.Seed, i, 1) - .5f) * m.H * 2, -1700 + R(p.Seed, i, 2) * 1500, 2 + 3 * R(p.Seed, i, 3), m.C(0xFFFFFF, c.F(p.StarBrightness) / 100 * (.4f + .6f * R(p.Seed, i, 4))));
                var g = PctPos(m, c.F(p.GalaxyX), c.F(p.GalaxyY)); float gs = c.F(p.GalaxySize), spin = m.Now * c.F(p.GalaxySpin) * .1f;
                m.Turn(g.X, g.Y, c.F(p.GalaxyTilt) * MathF.PI / 180, () => m.Pitch(g.X, g.Y, -700, 1.1f, () =>
                {
                    m.Ball(g.X, g.Y, -700, gs * .18f, m.C(p.GalaxyCore, c.F(p.GalaxyStrength) / 100));
                    for (int i = 0; i < 160; i++) { float f = R(p.Seed, i, 5), a = (i % 2) * MathF.PI + f * 5 + spin, rr = gs * f; m.Ball(g.X + MathF.Cos(a) * rr, g.Y + MathF.Sin(a) * rr, -700 + (R(p.Seed, i, 6) - .5f) * 20, 4, m.C(Sculpt.Mix(m.C(p.GalaxyCore), m.C(p.GalaxyArm), f), c.F(p.GalaxyStrength) / 100)); }
                }));
            }, 5);
            var pl = PctPos(m, c.F(p.PlanetX), c.F(p.PlanetY)); float pr = c.F(p.PlanetSize) * 1.3f;
            m.Ball(pl.X, pl.Y, -200, pr, m.C(p.PlanetColor)); for (int i = -2; i <= 2; i++) m.Lathe(pl.X, -200, m.C(p.PlanetBand, .8f), pr * MathF.Sqrt(1 - (i * .3f) * (i * .3f)) * 1.01f, pl.Y + i * pr * .3f - 6, pr * MathF.Sqrt(1 - (i * .3f) * (i * .3f)) * 1.01f, pl.Y + i * pr * .3f + 6);
            m.Ball(pl.X, pl.Y, -200, pr * 1.08f, m.C(p.AtmosphereColor, .18f));
            if (p.Ring) m.Turn(pl.X, pl.Y, c.F(p.RingTilt) * MathF.PI / 180, () => m.Pitch(pl.X, pl.Y, -200, 1.25f, () => { m.Torus(pl.X, pl.Y, -200, pr * 1.7f, pr * .12f, m.C(p.PlanetBand, .8f)); m.Torus(pl.X, pl.Y, -200, pr * 1.45f, pr * .07f, m.C(p.PlanetBand, .5f)); }));
        });
        S<AuroraParameter>(d, (p, c, m) => // オーロラ: 波打つ光のカーテン(下ほど明るい、縦の帯の重なり)
        {
            int cu = Math.Clamp((int)c.F(p.Curtains), 1, 6); float by = (c.F(p.BaseY) / 100 - .5f) * m.H, hh = c.F(p.CurtainHeight), amp = c.F(p.WaveAmp), sp = c.F(p.Speed) / 100, it = c.F(p.Intensity) / 100;
            var cols = new[] { p.Color1, p.Color2, p.Color3 };
            for (int k = 0; k < cu; k++)
            {
                float z = -1200 + k * 300;
                for (int i = 0; i < 40; i++)
                {
                    float x = -m.W * .8f + m.W * 1.6f * i / 40, x2 = x + m.W * 1.6f / 40, w1 = MathF.Sin(x / 300 + m.Now * sp + k) * amp, w2 = MathF.Sin(x2 / 300 + m.Now * sp + k) * amp, zz1 = z + MathF.Cos(x / 200 + m.Now * sp * .7f) * 120, zz2 = z + MathF.Cos(x2 / 200 + m.Now * sp * .7f) * 120;
                    float hk = hh * (.7f + .3f * MathF.Sin(i * .7f + m.Now));
                    m.Quad3D(V3(x, by + w1, zz1), V3(x2, by + w2, zz2), V3(x2, by + w2 - hk * .4f, zz2), V3(x, by + w1 - hk * .4f, zz1), m.C(cols[k % 3], .55f * it));
                    m.Quad3D(V3(x, by + w1 - hk * .4f, zz1), V3(x2, by + w2 - hk * .4f, zz2), V3(x2, by + w2 - hk, zz2), V3(x, by + w1 - hk, zz1), m.C(cols[(k + 1) % 3], .18f * it));
                }
            }
        });
        S<SkylineParameter>(d, (p, c, m) => // 夕焼けの街: 奥ほど霞む何列ものビル(山)と、ともる窓の明かり
        {
            int L = Math.Clamp((int)c.F(p.Layers), 1, 5); float by = (c.F(p.BaseY) / 100 - .5f) * m.H, scr = m.Now * c.F(p.Scroll);
            bool city = p.Kind.ToString().Contains("City");
            for (int l = L - 1; l >= 0; l--)
            {
                float z = -300 - l * 450, s = 1 + l * .35f; var col = Sculpt.Mix(m.C(p.NearColor), m.C(p.HazeColor), l / (float)L);
                if (city)
                    for (int i = 0; i < 24; i++)
                    {
                        float bw = 70 + R(p.Seed + l, i) * 90, bh = (120 + R(p.Seed + l, i, 1) * 380) * (1 - l * .15f), x = Cyc(i * 120 * s - scr * (1 - l * .3f), m.W * s * 1.6f) - m.W * s * .8f;
                        m.Box(x, by - bh / 2, z, bw, bh, bw * .8f, col, .03f);
                        if (p.Windows && l == 0) for (int w = 0; w < (int)(bh / 50); w++) for (int q = 0; q < 2; q++) if (R(p.Seed, i * 37 + w * 3 + q, 5) < c.F(p.WindowAmount) / 100) m.Box(x - bw * .2f + q * bw * .4f, by - 30 - w * 50, z + bw * .4f + 1, 14, 20, 2, m.C(p.WindowColor), .1f);
                    }
                else
                {
                    var pts = new List<float> { -m.W * s, by + 10 }; for (int i = 0; i <= 12; i++) { float x = -m.W * s + m.W * 2 * s * i / 12; pts.Add(x); pts.Add(by - (150 + R(p.Seed + l, i) * 350) * (1 - l * .15f)); }
                    pts.Add(m.W * s); pts.Add(by + 10); m.Slab(z - 40, 80, col, pts.ToArray());
                }
            }
        });
        S<VolcanoParameter>(d, (p, c, m) => // 火山の噴火: 円すいの山、火口から噴き上がる溶岩の玉と灰の雲
        {
            float bx = (c.F(p.PosX) / 100 - .5f) * m.W, by = (c.F(p.BaseY) / 100 - .5f) * m.H, mh = c.F(p.MountHeight), mw = c.F(p.MountWidth), pw = c.F(p.Power) / 100;
            m.Lathe(bx, -500, m.C(p.MountainColor), mw / 2, by, mw * .12f, by - mh, mw * .08f, by - mh * .95f);
            m.Lathe(bx, -500, m.C(p.LavaColor), mw * .08f, by - mh * .96f, 0, by - mh * .9f);
            for (int i = 0; i < 4; i++) { float a = -1.2f + i * .8f; m.Tube(new List<Vector3> { V3(bx + MathF.Sin(a) * mw * .1f, by - mh * .95f, -500 + MathF.Cos(a) * mw * .1f), V3(bx + MathF.Sin(a) * mw * .25f, by - mh * .55f, -500 + MathF.Cos(a) * mw * .25f), V3(bx + MathF.Sin(a) * mw * .4f, by - mh * .1f, -500 + MathF.Cos(a) * mw * .4f) }, 10, m.C(p.LavaColor, .9f)); }
            Low(m, () =>
            {
                for (int i = 0; i < Math.Min((int)c.F(p.Bombs), 120); i++) { float life = 2.2f, ph = Frac(m.Now / life + R(p.Seed, i)) * life, vx = (R(p.Seed, i, 1) - .5f) * 600 * pw, vz = (R(p.Seed, i, 3) - .5f) * 600 * pw, vy = -(700 + 500 * R(p.Seed, i, 2)) * pw; m.Ball(bx + vx * ph, by - mh + vy * ph + 500 * ph * ph, -500 + vz * ph, 8 + 10 * R(p.Seed, i, 4), m.C(p.LavaColor, 1 - ph / life * .5f)); }
            }, 6);
            for (int i = 0; i < (int)(c.F(p.AshAmount) / 6); i++) { float f = Frac(m.Now * .15f + i / 12f); m.Ball(bx + (R(p.Seed, i, 6) - .5f) * 300 * f + f * 200, by - mh - 100 - f * 700, -500, 120 + f * 260, m.C(p.AshColor, .55f * (1 - f))); }
            if (p.Lightning && Frac(m.Now * .5f) < .1f) m.Tube(new List<Vector3> { V3(bx + 100, by - mh - 600, -450), V3(bx + 60, by - mh - 450, -450), V3(bx + 130, by - mh - 350, -450), V3(bx + 80, by - mh - 200, -450) }, 5, m.C(0xEEF4FF));
        });
        S<WaterSurfaceDownParameter>(d, (p, c, m) => // 水面(見下げ): 奥へ寝かせた水面のうねる帯と、広がる波紋、光る粒
        {
            m.Pitch(0, 0, -300, 1.0f, () =>
            {
                for (int i = 0; i < 16; i++) { float y = -m.H * .8f + m.H * 1.6f * i / 16, z = -300 + MathF.Sin(i * .9f + m.Now * c.F(p.Flow) / 50) * 18 * c.F(p.WaveStrength) / 100; m.Box(0, y, z, m.W * 1.8f, m.H * 1.6f / 16 + 2, 16, Sculpt.Mix(m.C(p.ShallowColor), m.C(p.DeepColor), c.F(p.Depth) / 100 * (.5f + .5f * MathF.Sin(i * 1.3f))), .3f); }
                for (int i = 0; i < Math.Min((int)c.F(p.RippleCount), 12); i++) { float f = Frac(m.Now * c.F(p.RippleSpeed) / 200 + i / 6f); m.Torus((R(p.Seed, i) - .5f) * m.W, (R(p.Seed, i, 1) - .5f) * m.H, -280, c.F(p.RippleRadius) * f, 5, m.C(p.HighlightColor, (1 - f) * .8f)); }
                Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.SparkleCount), 80); i++) { float tw = MathF.Max(0, MathF.Sin(m.Now * 4 + i * 2.1f)); if (tw > .2f) m.Ball((R(p.Seed, i, 2) - .5f) * m.W * 1.5f, (R(p.Seed, i, 3) - .5f) * m.H * 1.5f, -282, 6 * tw, m.C(p.HighlightColor)); } }, 5);
            });
        });
        S<WaterSurfaceUpParameter>(d, (p, c, m) => // 水面(見上げ): 頭上でゆらぐ明るい水面、斜めに差す光の柱、上る泡
        {
            m.Pitch(0, -m.H / 2, -500, -1.1f, () => { for (int i = 0; i < 12; i++) { float x = -m.W + m.W * 2 * i / 12; m.Box(x + m.W / 12, -m.H / 2, -500 + MathF.Sin(i + m.Now * c.F(p.Flow) / 60) * 30, m.W * 2 / 12 + 2, m.H * 2, 14, Sculpt.Mix(m.C(p.LightColor), m.C(p.DeepColor), .35f + .25f * MathF.Sin(i * 1.7f)), .3f); } });
            var s = PctPos(m, c.F(p.SunX), c.F(p.SunY)); m.Ball(s.X, -m.H / 2, -700, 160, m.C(p.LightColor, c.F(p.Glare) / 100 * .6f));
            for (int i = 0; i < Math.Min((int)c.F(p.RayCount), 16); i++) { float x = s.X + (i - c.F(p.RayCount) / 2) * 140, sw = MathF.Sin(m.Now + i) * 30; m.Limb(x, -m.H / 2, -600, x + sw + c.F(p.RayAngle) * 20, m.H / 2, 100, c.F(p.RayWidth) * .3f, c.F(p.RayWidth) * .6f, m.C(p.LightColor, c.F(p.RayStrength) / 100 * .3f)); }
            for (int i = 0; i < Math.Min((int)c.F(p.BubbleCount), 30); i++) { float y = m.H / 2 - Cyc(m.Now * 160 + R(p.Seed, i) * m.H, m.H); m.Ball((R(p.Seed, i, 1) - .5f) * m.W + MathF.Sin(m.Now * 3 + i) * 20, y, -400 + R(p.Seed, i, 2) * 500, 10 + 14 * R(p.Seed, i, 3), m.C(p.LightColor, .5f)); }
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.ParticleCount), 90); i++) m.Ball((R(p.Seed, i, 4) - .5f) * m.W * 1.4f, (R(p.Seed, i, 5) - .5f) * m.H + MathF.Sin(m.Now + i) * 10, -800 + R(p.Seed, i, 6) * 900, 3, m.C(p.LightColor, .4f)); }, 5);
            m.Box(0, 0, -1600, m.W * 3, m.H * 3, 10, m.C(p.DeepColor), .01f);
        });
        S<WavesParameter>(d, (p, c, m) => // 波: 奥へ何列も重なる、うねって進む海の波と白い波頭
        {
            int L = Math.Clamp((int)c.F(p.Layers), 1, 6); float wl = (c.F(p.WaterLine) / 100 - .5f) * m.H, wh = c.F(p.WaveHeight), lam = MathF.Max(80, c.F(p.Wavelength)), sp = c.F(p.Speed) / 100;
            for (int l = L - 1; l >= 0; l--)
            {
                float z = -l * 350, f = l / (float)MathF.Max(1, L - 1); var col = Sculpt.Mix(m.C(p.ShallowColor), m.C(p.DeepColor), f);
                var pts = new List<float> { -m.W, m.H }; var crest = new List<Vector3>();
                for (int i = 0; i <= 32; i++) { float x = -m.W + m.W * 2 * i / 32, y = wl + l * 40 - MathF.Sin(x / lam * MathF.Tau - m.Now * sp * 2 + l) * wh * (1 + l * .2f); pts.Add(x); pts.Add(y); crest.Add(V3(x, y, z + 30)); }
                pts.Add(m.W); pts.Add(m.H); m.Slab(z - 30, 60, col, pts.ToArray());
                m.Tube(crest, 8 * c.F(p.Foam) / 60, m.C(p.FoamColor, .8f));
            }
        });
        S<SplashParameter>(d, (p, c, m) => // 水しぶき: 水面から王冠のように跳ね上がる粒と、広がる輪
        {
            float x0 = (c.F(p.PosX) / 100 - .5f) * m.W, wy = (c.F(p.WaterY) / 100 - .5f) * m.H, iv = MathF.Max(.3f, c.F(p.Interval)), ph = Frac(m.Now / iv) * iv, pw = c.F(p.Power), g = c.F(p.Gravity);
            m.Box(0, wy + 10, -300, m.W * 2, 20, 1400, m.C(p.Color, .35f), .01f);
            m.Pitch(x0, wy, -100, MathF.PI / 2, () => { for (int r = 0; r < 3; r++) { float f = Math.Clamp(ph / 1.2f - r * .15f, 0, 1); if (f > 0) m.Torus(x0, wy, -100, c.F(p.RippleRadius) * f, 5, m.C(p.Color, 1 - f)); } });
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Count), 120); i++) { float a = R(p.Seed, i) * MathF.Tau, sp = c.F(p.Spread) / 100 * (.3f + .7f * R(p.Seed, i, 1)), v = pw * (.5f + .5f * R(p.Seed, i, 2)); float y = wy - v * MathF.Cos(sp) * ph + g * ph * ph / 2; if (y > wy) continue; float r = v * MathF.Sin(sp) * ph; m.Ball(x0 + MathF.Cos(a) * r, y, -100 + MathF.Sin(a) * r, c.F(p.DropSize) * 1.6f, m.C(p.Color, c.F(p.Opacity) / 100)); } }, 6);
            float cr = MathF.Min(1, ph * 3); m.Lathe(x0, -100, m.C(p.Color, .5f * (1 - ph / iv)), 40 * cr, wy, 90 * cr, wy - 120 * cr * (1 - ph / iv), 95 * cr, wy - 125 * cr * (1 - ph / iv));
        });
        S<FrostedGlassParameter>(d, (p, c, m) => // 曇りガラス: 画面の前の、白く曇った板と、ふくらんだ水滴・垂れる筋
        {
            m.Box(0, 0, 0, m.W, m.H, 10, m.C(p.FogColor, c.F(p.FogOpacity) / 100 * .7f), .01f);
            for (int i = 0; i < Math.Min((int)c.F(p.DropletCount), 120); i++) { float s = c.F(p.DropletSize) * (.5f + R(p.Seed, i, 2)); m.Ball((R(p.Seed, i) - .5f) * m.W, (R(p.Seed, i, 1) - .5f) * m.H, 6, s, s, s * .5f, m.C(0xFFFFFF, .5f)); }
            for (int i = 0; i < Math.Min((int)c.F(p.DripCount), 20); i++) { float x = (R(p.Seed, i, 3) - .5f) * m.W, y0 = (R(p.Seed, i, 4) - .5f) * m.H, len = Cyc(m.Now * c.F(p.DripSpeed) * 3 + R(p.Seed, i, 5) * 400, 500); m.Limb(x, y0, 6, x + MathF.Sin(i) * 10, y0 + len, 6, 4, 6, m.C(0xFFFFFF, .45f)); m.Ball(x + MathF.Sin(i) * 10, y0 + len, 8, 10, m.C(0xFFFFFF, .6f)); }
        });
        S<IceParameter>(d, (p, c, m) => // 氷: 画面のふちから伸びて枝分かれする、透きとおった角ばった氷の結晶
        {
            float k = p.Intro ? Math.Clamp(m.Now / MathF.Max(.1f, c.F(p.IntroTime)), 0, 1) * c.F(p.Growth) / 100 : c.F(p.Growth) / 100, bl = c.F(p.BranchLength), th = c.F(p.Thickness) * 3;
            m.Box(0, 0, -50, m.W, m.H, 6, m.C(p.IceColor, c.F(p.Haze) / 100 * .2f), .01f);
            int roots = Math.Min((int)c.F(p.Roots), 24), depth = Math.Clamp(p.Depth, 1, 6);
            void Branch(Vector3 a, float ang, float len, int dpt, int s)
            {
                if (dpt <= 0 || len < 8) return; float l = len * k; var b = a + V3(MathF.Cos(ang) * l, MathF.Sin(ang) * l, (R(s, 1) - .5f) * l * .4f);
                m.Limb(a, b, th * dpt / depth + 2, th * (dpt - 1) / depth + 1.5f, m.C(p.IceColor, .75f));
                Branch(b, ang + .5f + R(s, 2) * .3f, len * .7f, dpt - 1, s * 3 + 1); Branch(b, ang - .5f - R(s, 3) * .3f, len * .7f, dpt - 1, s * 3 + 2);
            }
            for (int r = 0; r < roots; r++) { float e = r / (float)roots * 4; Vector3 a; float ang; int side = (int)e; float f = e - side; if (side == 0) { a = V3(-m.W / 2 + f * m.W, -m.H / 2, 0); ang = MathF.PI / 2; } else if (side == 1) { a = V3(m.W / 2, -m.H / 2 + f * m.H, 0); ang = MathF.PI; } else if (side == 2) { a = V3(m.W / 2 - f * m.W, m.H / 2, 0); ang = -MathF.PI / 2; } else { a = V3(-m.W / 2, m.H / 2 - f * m.H, 0); ang = 0; } Branch(a, ang + (R(p.Seed, r) - .5f) * .8f, bl, depth, p.Seed * 50 + r); }
            for (int i = 0; i < Math.Min((int)c.F(p.Sparkles), 50); i++) if (MathF.Sin(m.Now * 4 + i) > .6f) m.Ball((R(p.Seed, i, 7) - .5f) * m.W, (R(p.Seed, i, 8) - .5f) * m.H, 10, 6, m.C(0xFFFFFF));
        });
        S<FireParameter>(d, (p, c, m) => // 炎: 下が太く先がとがってゆらめく、外は赤く芯は黄色い炎の舌
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float fw = c.F(p.FlameWidth), fh = c.F(p.FlameHeight), sw = c.F(p.Sway), sp = c.F(p.Speed) / 100;
            bool edge = !p.Edge.ToString().Contains("Spot"); int tongues = edge ? 14 : 7;
            for (int i = 0; i < tongues; i++)
            {
                float x = edge ? -m.W / 2 + m.W * (i + .5f) / tongues : o.X + (i - tongues / 2f + .5f) * fw / tongues * 1.3f, y = edge ? m.H / 2 + c.F(p.EdgeOffset) : o.Y, z = (R(p.Seed, i) - .5f) * fw * .6f;
                float h = fh * (.55f + .45f * R(p.Seed, i, 1)) * (edge ? .6f : 1 - MathF.Abs(i - tongues / 2f + .5f) / tongues), fl = MathF.Sin(m.Now * 8 * sp + i * 1.7f);
                var tip = V3(x + fl * sw, y - h * (.9f + .1f * fl), z);
                float r0 = fw / tongues * 1.1f; var mid = V3(x - fl * sw * .3f, y - h * .45f, z); m.Limb(V3(x, y - r0 * .5f, z), mid, r0, r0 * .7f, m.C(p.OuterColor, .85f)); m.Limb(mid, tip, r0 * .7f, 3, m.C(p.OuterColor, .85f));
                m.Limb(x, y, z + 6, tip.X * .6f + x * .4f, y - h * .55f, z + 6, fw / tongues * .45f, 6, m.C(p.InnerColor, .95f));
            }
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.SparkCount), 60); i++) { float f = Frac(m.Now * .6f + R(p.Seed, i, 3)); m.Ball((edge ? (R(p.Seed, i, 4) - .5f) * m.W : o.X + (R(p.Seed, i, 4) - .5f) * fw) + MathF.Sin(m.Now * 3 + i) * 30, (edge ? m.H / 2 : o.Y) - f * fh * 1.6f, (R(p.Seed, i, 5) - .5f) * 300, 5, m.C(p.InnerColor, 1 - f)); } }, 5);
        });
        S<LavaParameter>(d, (p, c, m) => // 溶岩: 光る溶岩の床に浮かぶ、ひび割れた黒い岩の板、ふくらむ泡
        {
            m.Pitch(0, 0, -300, 1.05f, () =>
            {
                m.Box(0, 0, -320, m.W * 2, m.H * 2, 20, m.C(p.MidColor), .01f);
                float ps = c.F(p.PlateSize), cr = c.F(p.CrustAmount) / 100; int nx = (int)(m.W * 2 / ps) + 1, ny = (int)(m.H * 2 / ps) + 1; float fl = m.Now * c.F(p.Flow) * .2f;
                for (int y = 0; y < ny; y++) for (int x = 0; x < nx; x++) { int i = x + y * nx; if (R(p.Seed, i) > cr + .2f) continue; float s = ps * (.6f + .3f * R(p.Seed, i, 1)); m.Box(-m.W + (x + .5f) * ps + Cyc(fl, ps), -m.H + (y + .5f) * ps, -300, s, s * (.8f + .3f * R(p.Seed, i, 2)), 30, m.C(p.CrustColor), .15f); }
                for (int i = 0; i < Math.Min((int)c.F(p.Bubbles), 24); i++) { float f = Frac(m.Now * .5f + R(p.Seed, i, 3)); m.Ball((R(p.Seed, i, 4) - .5f) * m.W * 1.6f, (R(p.Seed, i, 5) - .5f) * m.H * 1.6f, -310, 40 * f, m.C(p.HotColor, 1 - f * .5f)); }
            });
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Embers), 60); i++) { float f = Frac(m.Now * .4f + R(p.Seed, i, 6)); m.Ball((R(p.Seed, i, 7) - .5f) * m.W * 1.2f, m.H / 2 - f * m.H, (R(p.Seed, i, 8) - .5f) * 600, 5, m.C(p.HotColor, 1 - f)); } }, 5);
        });
    }
}
