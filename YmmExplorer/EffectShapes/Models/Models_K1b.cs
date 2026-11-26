using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 31〜60番(煙・花・光・魔法・近未来)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>平たい花びら(回りながら落ちる小さな板)</summary>
    static void Petal(Sculpt m, float x, float y, float z, float s, float spin, Vector4 col)
    {
        m.Yaw(x, y, z, spin * 1.3f, () => m.Turn(x, y, spin, () => m.Ball(x, y, z, s, s * .6f, s * .12f, col)));
    }
    /// <summary>星形・ハートなどのキラキラの粒(厚みのある板)</summary>
    static void SparkPiece(Sculpt m, float x, float y, float z, float s, int kind, float rot, Vector4 col)
    {
        if (kind == 2) { Puff(m, z, s * .4f, col, HeartPts(x, y, s / 260f)); return; }
        if (kind == 4) { m.Ball(x, y, z, s * .45f, col); return; }
        int n = kind == 1 ? 5 : 4; float inner = kind == 3 ? .7f : kind == 1 ? .45f : .28f;
        m.Slab(z - s * .12f, s * .24f, col, StarPts(x, y, s, n, inner, rot));
    }

    static partial void RegisterK1b(Dictionary<Type, SceneModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        // ---------------------------------------------------------------- 31 煙・蒸気
        S<SmokeParameter>(d, (p, c, m) => // 煙: 下の口から、ふくらみながら立ちのぼって流れる煙の玉
        {
            var o = PctPos(m, c.F(p.SourceX), c.F(p.SourceY)); float rise = c.F(p.RiseHeight), sz = c.F(p.Size), wind = c.F(p.Wind), sp = c.F(p.Speed) / 100;
            uint col = p.Kind == SmokeKind.Steam ? 0xF2F4F8u : p.Kind == SmokeKind.BlackSmoke ? 0x2A2A2Eu : 0x9A9AA0u; var cc = p.Kind == SmokeKind.Custom ? m.C(p.CustomColor) : m.C(col);
            int n = Math.Min((int)c.F(p.Count), 60);
            for (int i = 0; i < n; i++)
            {
                float f = Frac(m.Now * .25f * sp + R(p.Seed, i)), x = o.X + (R(p.Seed, i, 1) - .5f) * c.F(p.SourceWidth) + f * f * wind + MathF.Sin(m.Now + i) * c.F(p.Turbulence) * f, y = o.Y - f * rise, z = (R(p.Seed, i, 2) - .5f) * 200 * (1 + f);
                m.Ball(x, y, z, sz * (.4f + f), m.C(cc, (1 - f) * .45f * c.F(p.Density) / 100));
            }
        });
        S<SakuraParameter>(d, (p, c, m) => // 桜: 回りながらひらひら舞い落ちる、うす桃色の花びら
        {
            int n = Math.Min((int)c.F(p.Count), 160); float sz = c.F(p.Size) * 1.4f;
            Low(m, () => { for (int i = 0; i < n; i++) { float dd = .6f + .4f * R(p.Seed, i, 3), y = -m.H / 2 - 30 + Cyc(m.Now * c.F(p.Fall) * dd + R(p.Seed, i, 1) * m.H * 1.2f, m.H * 1.2f), x = (R(p.Seed, i) - .5f) * m.W * 1.3f + m.Now * c.F(p.Wind) * .5f % m.W * 0 + MathF.Sin(m.Now * 1.5f + i) * c.F(p.Sway), z = -900 + R(p.Seed, i, 2) * 1100; Petal(m, x, y, z, sz * dd, m.Now * c.F(p.Spin) / 40 + i, Sculpt.Mix(m.C(p.PetalColor, c.F(p.Opacity) / 100), m.C(0xFFFFFF, c.F(p.Opacity) / 100), R(p.Seed, i, 4) * c.F(p.Variation) / 100)); } }, 8);
        });
        S<FallingLeavesParameter>(d, (p, c, m) => // 落ち葉: 赤・橙・黄の葉(もみじは星形の板)が、くるくる回って落ちる
        {
            int n = Math.Min((int)c.F(p.Count), 80); float sz = c.F(p.Size); var cols = new[] { p.Color1, p.Color2, p.Color3 };
            for (int i = 0; i < n; i++)
            {
                float y = -m.H / 2 - 40 + Cyc(m.Now * c.F(p.Fall) * (.6f + .4f * R(p.Seed, i, 3)) + R(p.Seed, i, 1) * m.H * 1.2f, m.H * 1.2f), x = (R(p.Seed, i) - .5f) * m.W * 1.3f + MathF.Sin(m.Now * 1.2f + i) * c.F(p.Sway), z = -900 + R(p.Seed, i, 2) * 1100, sp = m.Now * c.F(p.Spin) / 40 + i;
                int kind = p.Kind == LeafKind.Mixed ? i % 3 : (int)p.Kind; var col = m.C(cols[i % 3]);
                m.Yaw(x, y, z, sp, () => m.Turn(x, y, sp * .7f, () =>
                {
                    if (kind == 0) m.Slab(z - 3, 6, col, StarPts(x, y, sz, 5, .5f, 0)); else if (kind == 2) m.Slab(z - 3, 6, col, PiePts(x, y + sz * .4f, sz, -2.4f, -.7f)); else m.Ball(x, y, z, sz * .5f, sz, 4, col);
                    m.Limb(x, y, z, x, y + sz * .8f, z, 2, 2, m.C(0x6D4C41));
                }));
            }
        });
        S<FlowerFieldParameter>(d, (p, c, m) => // 花畑: 奥まで広がる地面に、揺れる茎と花、草の葉
        {
            m.Box(0, m.H / 2 + 20, -600, m.W * 2.4f, 40, 1800, m.C(Sculpt.Dark(m.C(p.GrassColor), .6f), 1), .01f);
            float sw = c.F(p.Sway), sh = c.F(p.StemHeight), fs = c.F(p.FlowerSize) * 1.3f;
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.GrassCount), 160); i++) { float x = (R(p.Seed, i, 9) - .5f) * m.W * 1.8f, z = -1400 + R(p.Seed, i, 8) * 1500, h = 60 + 80 * R(p.Seed, i, 7), b = MathF.Sin(m.Now * 2 + x / 200) * sw * .5f; m.Limb(x, m.H / 2, z, x + b, m.H / 2 - h, z, 5, 1, m.C(p.GrassColor)); } }, 5);
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 50); i++)
            {
                float x = (R(p.Seed, i) - .5f) * m.W * 1.6f, z = -1300 + R(p.Seed, i, 1) * 1400, h = sh * (.6f + .4f * R(p.Seed, i, 2)), b = MathF.Sin(m.Now * 2 * c.F(p.WindSpeed) / 100 + x / 200) * sw; var top = V3(x + b, m.H / 2 - h, z);
                m.Tube(new List<Vector3> { V3(x, m.H / 2, z), V3(x + b * .3f, m.H / 2 - h * .5f, z), top }, 4, m.C(p.GrassColor));
                var fc = p.Colorful ? Pal(m, i) : m.C(p.FlowerColor);
                for (int k = 0; k < 5; k++) { float a = k * MathF.Tau / 5; m.Ball(top.X + MathF.Cos(a) * fs * .5f, top.Y + MathF.Sin(a) * fs * .5f, top.Z, fs * .4f, fs * .4f, fs * .15f, fc); }
                m.Ball(top.X, top.Y, top.Z + 4, fs * .25f, m.C(0xFFD54F));
            }
        });
        S<FlyersParameter>(d, (p, c, m) => // 蝶・鳥の群れ: 羽ばたく2枚の羽の蝶、またはV字の鳥が群れで飛ぶ
        {
            int n = Math.Min((int)c.F(p.Count), 30); float sz = c.F(p.Size) * 2.6f, fl = MathF.Sin(m.Now * c.F(p.Flap) / 8) , hd = c.F(p.Heading) * MathF.PI / 180;
            for (int i = 0; i < n; i++)
            {
                float span = m.W * 1.6f, x = -span / 2 + Cyc(m.Now * c.F(p.Speed) * (.7f + .3f * R(p.Seed, i, 3)) + R(p.Seed, i) * span, span), y = (R(p.Seed, i, 1) - .5f) * m.H * .7f + MathF.Sin(m.Now * 2 + i) * 40, z = -900 + R(p.Seed, i, 2) * 1000, f = MathF.Sin(m.Now * c.F(p.Flap) / 8 + i);
                m.Turn(x, y, hd, () =>
                {
                    if (p.Kind == FlyKind.Butterfly)
                    {
                        m.Limb(x - sz * .4f, y, z, x + sz * .4f, y, z, 4, 3, m.C(0x3E2723));
                        foreach (int s in Sides) m.Pitch(x, y, z, s * (.2f + f * .9f), () => { m.Ball(x + sz * .1f, y + s * sz * .5f, z, sz * .4f, sz * .45f, 2, m.C(p.Color1)); m.Ball(x - sz * .3f, y + s * sz * .35f, z, sz * .3f, sz * .3f, 2, m.C(p.Color2)); });
                    }
                    else
                    {
                        m.Ball(x, y, z, sz * .35f, sz * .14f, sz * .14f, m.C(p.Color1)); m.Ball(x + sz * .35f, y - 3, z, sz * .1f, m.C(p.Color1));
                        foreach (int s in Sides) m.Limb(x, y, z, x - sz * .2f, y - f * sz * .4f, z + s * sz * .7f, sz * .08f, 2, m.C(p.Color2));
                    }
                });
            }
        });
        S<FirefliesParameter>(d, (p, c, m) => // 蛍: 暗い空間をふらふら飛ぶ、点滅して光る小さな虫
        {
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 60); i++)
            {
                float r = c.F(p.FlyRange), t = m.Now * c.F(p.FlySpeed) / 100, x = (R(p.Seed, i) - .5f) * m.W + MathF.Sin(t * .7f + i) * r, y = (R(p.Seed, i, 1) - .3f) * m.H * .8f + MathF.Sin(t * .9f + i * 2) * r * .5f, z = -800 + R(p.Seed, i, 2) * 900 + MathF.Cos(t * .5f + i) * r;
                float bl = MathF.Max(0, MathF.Sin(m.Now * c.F(p.BlinkSpeed) / 40 + i * 1.3f)), s = c.F(p.Size) * 2;
                m.Ball(x, y, z, s * 1.4f, s, s, m.C(0x33301A)); m.Ball(x - s, y, z, s * 1.2f, m.C(p.Color, .3f + .7f * bl)); m.Ball(x - s, y, z, c.F(p.Glow) * bl, m.C(p.Color, .15f * bl));
            }
        });
        S<BubbleParameter>(d, (p, c, m) => // シャボン玉: 虹色に光る、透きとおった玉がゆらゆら上る
        {
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 40); i++)
            {
                float life = 6, f = Frac(m.Now / life + R(p.Seed, i)), x = (R(p.Seed, i, 1) - .5f) * m.W + MathF.Sin(m.Now + i) * c.F(p.Drift) * 2, y = m.H / 2 - f * m.H * 1.3f * c.F(p.Rise) / 45, z = -800 + R(p.Seed, i, 2) * 900, s = c.F(p.Size) * 1.4f * (.6f + .6f * R(p.Seed, i, 3)), wb = 1 + MathF.Sin(m.Now * 5 + i) * c.F(p.Wobble) / 100;
                if (p.Pop && f > .92f) continue;
                var col = Sculpt.Mix(m.C(0x9FE8FF), Pal(m, i), c.F(p.Iridescence) / 200);
                m.Ball(x, y, z, s * wb, s / wb, s, m.C(col, .3f * c.F(p.Opacity) / 100)); m.Ball(x - s * .35f, y - s * .35f, z + s * .8f, s * .15f, m.C(0xFFFFFF, .8f));
            }
        });
        S<SparkleParameter>(d, (p, c, m) => // キラキラ: 星・ハート・ひし形の厚い粒が、またたきながら回って上る
        {
            int kind = (int)p.Shape;
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 90); i++)
            {
                float f = Frac(m.Now * c.F(p.Rise) / 400 + R(p.Seed, i)), x = (R(p.Seed, i, 1) - .5f) * m.W + MathF.Sin(m.Now + i) * c.F(p.Drift), y = m.H / 2 - f * m.H * 1.1f, z = -700 + R(p.Seed, i, 2) * 900, tw = .4f + .6f * MathF.Abs(MathF.Sin(m.Now * c.F(p.TwinkleSpeed) / 40 + i)), s = c.F(p.Size) * 1.6f * tw;
                var col = p.Rainbow ? Pal(m, i) : Sculpt.Mix(m.C(p.Color1), m.C(p.Color2), R(p.Seed, i, 3));
                m.Yaw(x, y, z, m.Now * c.F(p.Spin) / 40 + i, () => SparkPiece(m, x, y, z, s, kind, 0, col));
            }
        });
        S<ExplosionParameter>(d, (p, c, m) => // 爆発: ふくらむ火の玉、黒い煙、広がる衝撃の輪、飛ぶ破片と火花
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float iv = MathF.Max(.5f, c.F(p.Interval)), life = c.F(p.Life), ph = Frac(m.Now / iv) * iv, sz = c.F(p.Size);
            if (ph > life) return; float k = ph / life, e = Ease(Math.Clamp(k * 3, 0, 1));
            for (int i = 0; i < 9; i++) { float a = R(p.Seed, i) * MathF.Tau, r = sz * .6f * e; m.Ball(o.X + MathF.Cos(a) * r * .7f, o.Y + MathF.Sin(a) * r * .5f - k * 80, MathF.Sin(a * 2) * r * .5f, sz * (.4f + .4f * e) * (1 - k * .5f), m.C(Sculpt.Mix(m.C(p.CoreColor), m.C(p.FireColor), Math.Clamp(k * 2 + i * .05f, 0, 1)), 1 - k)); }
            for (int i = 0; i < (int)c.F(p.SmokeAmount); i++) { float a = R(p.Seed, i, 2) * MathF.Tau, r = sz * (.5f + k); m.Ball(o.X + MathF.Cos(a) * r, o.Y + MathF.Sin(a) * r * .5f - k * 300, MathF.Sin(a) * r * .5f, sz * .5f * (.5f + k), m.C(p.SmokeColor, (1 - k) * .6f * c.F(p.Smoke) / 100)); }
            m.Pitch(o.X, o.Y + sz * .5f, 0, MathF.PI / 2, () => m.Torus(o.X, o.Y + sz * .5f, 0, sz * 3 * e, 10, m.C(p.CoreColor, (1 - k) * c.F(p.Shockwave) / 100)));
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.SparkCount), 80); i++) { float a = R(p.Seed, i, 4) * MathF.Tau, b = (R(p.Seed, i, 5) - .5f) * 2, v = c.F(p.Spread) * 4 * (.4f + .6f * R(p.Seed, i, 6)); m.Ball(o.X + MathF.Cos(a) * v * ph, o.Y + MathF.Sin(a) * v * ph * .6f - v * .3f * ph + c.F(p.Gravity) * ph * ph / 2, b * v * ph, 5, m.C(p.FireColor, 1 - k)); } }, 5);
            for (int i = 0; i < Math.Min((int)c.F(p.DebrisCount), 20); i++) { float a = R(p.Seed, i, 7) * MathF.Tau, v = c.F(p.Spread) * 3, x = o.X + MathF.Cos(a) * v * ph, y = o.Y - v * ph * .8f + c.F(p.Gravity) * ph * ph / 2; m.Turn(x, y, ph * 8 + i, () => m.Box(x, y, MathF.Sin(a) * v * ph, 26, 16, 12, m.C(0x3A302A), .1f)); }
        });
        S<FireworksParameter>(d, (p, c, m) => // 花火: 打ち上がる玉と、空いっぱいに球状に開く光の粒
        {
            float iv = MathF.Max(.5f, c.F(p.Interval)), life = c.F(p.Life), sz = c.F(p.Size); int bc = Math.Clamp((int)c.F(p.BurstCount), 1, 6);
            for (int b = 0; b < bc; b++)
            {
                float ph = Frac(m.Now / iv + b / (float)bc) * iv; int cyc = (int)(m.Now / iv + b / (float)bc); float bx = (R(p.Seed + cyc, b) - .5f) * m.W * .7f, by = -m.H * (.1f + .25f * R(p.Seed + cyc, b, 1)), bz = -700 + R(p.Seed + cyc, b, 2) * 500;
                var col = Sculpt.Mix(m.C(p.Color), Pal(m, cyc + b), c.F(p.HueVariation) / 100);
                if (ph < .6f) { float f = ph / .6f; m.Ball(bx, m.H / 2 + (by - m.H / 2) * Ease(f), bz, 8, m.C(0xFFE0A0)); continue; }
                float k = (ph - .6f) / life; if (k > 1) continue; int n = Math.Min((int)c.F(p.ParticleCount), 160);
                Low(m, () => { for (int i = 0; i < n; i++) { float u = R(p.Seed, i) * 2 - 1, a = R(p.Seed, i, 1) * MathF.Tau, s = MathF.Sqrt(1 - u * u), r = sz * Ease(Math.Clamp(k * 2, 0, 1)); var q = V3(bx + MathF.Cos(a) * s * r, by + u * r + c.F(p.Gravity) * k * k * 2, bz + MathF.Sin(a) * s * r); m.Ball(q.X, q.Y, q.Z, 6, m.C(col, (1 - k) * c.F(p.Brightness) / 100)); if (p.FireworkType % 2 == 1) m.Limb(q, V3(bx, by, bz) + (q - V3(bx, by, bz)) * .8f, 3, 1, m.C(col, (1 - k) * .5f)); } }, 5);
            }
        });
        S<LaserParameter>(d, (p, c, m) => // レーザー: 上の発射口から、首を振って伸びる光の棒
        {
            var o = PctPos(m, c.F(p.OriginX), c.F(p.OriginY)); int n = Math.Clamp((int)c.F(p.Count), 1, 16); float L = c.F(p.Length), th = c.F(p.Thickness) * 2;
            bool on = c.F(p.BlinkHz) <= 0 || Frac(m.Now * c.F(p.BlinkHz)) < c.F(p.BlinkDuty) / 100; if (!on) return;
            for (int i = 0; i < n; i++)
            {
                float fi = n == 1 ? 0 : i / (float)(n - 1) - .5f, a = MathF.PI / 2 + c.F(p.Angle) * MathF.PI / 180 + (p.Mode == 0 ? fi * c.F(p.Spread) * MathF.PI / 180 : 0) + MathF.Sin(m.Now * c.F(p.SweepSpeed) * MathF.Tau + i * c.F(p.PhaseStep)) * c.F(p.SweepAmp) * MathF.PI / 180;
                float ox = o.X + (p.Mode == 0 ? 0 : fi * c.F(p.Spacing) * n), zz = (R(p.Seed, i) - .5f) * 400;
                var e = V3(ox + MathF.Cos(a) * L, o.Y + MathF.Sin(a) * L, zz + 300);
                m.Limb(V3(ox, o.Y, zz), e, th, th, m.C(0xFFFFFF)); m.Limb(V3(ox, o.Y, zz), e, th + c.F(p.Glow) * .5f, th + c.F(p.Glow) * .5f, m.C(p.LaserColor, .35f * c.F(p.Brightness) / 100));
                m.Cyl(ox, o.Y - 20, zz, 30, 50, m.C(0x37474F));
            }
        });
        S<BeamParameter>(d, (p, c, m) => // ビーム: ためる光の玉から、太い光の柱がうねって伸びる
        {
            var o = PctPos(m, c.F(p.OriginX), c.F(p.OriginY)); float cy = MathF.Max(.5f, c.F(p.Cycle)), ph = p.Auto ? Frac(m.Now / cy) * cy : c.F(p.ChargeTime) + .5f, ct = c.F(p.ChargeTime), ft = c.F(p.FireTime);
            float charge = p.Auto ? Math.Clamp(ph / ct, 0, 1) : c.F(p.Charge) / 100, fire = p.Auto ? (ph > ct && ph < ct + ft ? 1 : 0) : c.F(p.Power) / 100, bw = c.F(p.BeamWidth), ang = c.F(p.Angle) * MathF.PI / 180;
            m.Turn(o.X, o.Y, ang, () =>
            {
                m.Ball(o.X, o.Y, 0, bw * .6f * charge + 10, m.C(0xFFFFFF)); m.Ball(o.X, o.Y, 0, bw * charge + 10, m.C(p.Color, .35f));
                if (fire <= 0) return; float L = c.F(p.Length) * Ease(Math.Clamp((ph - ct) * 4, 0, 1));
                var pts = new List<Vector3>(); for (int i = 0; i <= 20; i++) { float x = o.X + L * i / 20; pts.Add(V3(x, o.Y + MathF.Sin(x / 80 - m.Now * 20) * bw * .05f * c.F(p.Wave) / 100, 0)); }
                m.Tube(pts, bw * .25f * fire, m.C(0xFFFFFF)); m.Tube(pts, bw * .5f * fire, m.C(p.Color, .4f));
                for (int k = 0; k < 4; k++) { float x = o.X + Cyc(m.Now * 1800 + k * L / 4, L); m.Pitch(x, o.Y, 0, 0, () => m.Yaw(x, o.Y, 0, MathF.PI / 2, () => m.Torus(x, o.Y, 0, bw * .6f, 4, m.C(p.Color, .5f)))); }
            });
        });
        S<EnergyBallParameter>(d, (p, c, m) => // エネルギー弾: 光の粒が集まってふくらむ玉、ためたら飛んでいく
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float cy = MathF.Max(.5f, c.F(p.Cycle)), ph = Frac(m.Now / cy), cr = c.F(p.ChargeRatio) / 100, r = c.F(p.Radius);
            float ch = p.Auto ? Math.Clamp(ph / cr, 0, 1) : c.F(p.Charge) / 100, fl = p.Auto ? Math.Clamp((ph - cr) / (1 - cr), 0, 1) : c.F(p.Fire) / 100, a = c.F(p.Angle) * MathF.PI / 180;
            var q = o + new Vector2(MathF.Cos(a), MathF.Sin(a)) * c.F(p.Distance) * fl * fl;
            m.Ball(q.X, q.Y, 0, r * .55f * ch + 6, m.C(0xFFFFFF)); m.Ball(q.X, q.Y, 0, r * ch + 6, m.C(p.Color, .35f)); m.Ball(q.X, q.Y, 0, r * 1.5f * ch + 6, m.C(p.Color, .12f));
            if (fl > 0) m.Limb(q.X, q.Y, 0, q.X - MathF.Cos(a) * r * 3, q.Y - MathF.Sin(a) * r * 3, 0, r * .6f, 4, m.C(p.Color, .3f));
            else Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Particles), 60); i++) { float u = R(p.Seed, i) * 2 - 1, b = R(p.Seed, i, 1) * MathF.Tau, f = 1 - Frac(m.Now * 1.2f + R(p.Seed, i, 2)), rr = r * (1 + 2 * f); m.Ball(o.X + MathF.Cos(b) * MathF.Sqrt(1 - u * u) * rr, o.Y + u * rr, MathF.Sin(b) * MathF.Sqrt(1 - u * u) * rr, 5, m.C(p.Color, 1 - f)); } }, 5);
        });
        S<PlasmaParameter>(d, (p, c, m) => // 電撃: ガラス玉の中心から内側へ走る稲妻、または2点の間の放電
        {
            float th = c.F(p.Thickness) * 1.5f, jag = c.F(p.Jag); int fr = (int)(m.Now * c.F(p.FlickerHz));
            void Zap(Vector3 a, Vector3 b, int s) { var pts = new List<Vector3>(); for (int i = 0; i <= 10; i++) { var q = Vector3.Lerp(a, b, i / 10f); if (i > 0 && i < 10) q += V3((R(s, i) - .5f) * jag * 2, (R(s, i, 1) - .5f) * jag * 2, (R(s, i, 2) - .5f) * jag * 2); pts.Add(q); } m.Tube(pts, th, m.C(p.CoreColor)); m.Tube(pts, th + c.F(p.GlowSize) * .4f, m.C(p.GlowColor, .3f)); }
            if (p.Mode == PlasmaMode.Ball)
            {
                var o = PctPos(m, c.F(p.BallX), c.F(p.BallY)); float r = c.F(p.BallRadius);
                m.Ball(o.X, o.Y, 0, r, m.C(p.GlowColor, .12f)); m.Ball(o.X, o.Y, 0, r * .15f, m.C(p.CoreColor)); m.Lathe(o.X, 0, m.C(0x263238), r * .6f, o.Y + r * .85f, r * .8f, o.Y + r * 1.3f);
                for (int i = 0; i < Math.Min((int)c.F(p.BoltCount), 14); i++) { float u = R(fr, i) * 2 - 1, b = R(fr, i, 1) * MathF.Tau, s = MathF.Sqrt(1 - u * u); Zap(V3(o.X, o.Y, 0), V3(o.X + MathF.Cos(b) * s * r, o.Y + u * r, MathF.Sin(b) * s * r), fr * 31 + i); }
            }
            else if (p.Mode == PlasmaMode.Arc)
            {
                var a = PctPos(m, c.F(p.X1), c.F(p.Y1)); var b = PctPos(m, c.F(p.X2), c.F(p.Y2));
                foreach (var e in new[] { a, b }) { m.Ball(e.X, e.Y, 0, 30, m.C(0x90A4AE)); m.Cyl(e.X, e.Y + 80, 0, 14, 140, m.C(0x546E7A)); }
                for (int i = 0; i < Math.Min((int)c.F(p.Strands), 6); i++) Zap(V3(a.X, a.Y, 0), V3(b.X, b.Y, 0), fr * 17 + i);
            }
            else for (int i = 0; i < Math.Min((int)c.F(p.ScatterCount), 12); i++) { float x = (R(p.Seed + fr, i) - .5f) * m.W, y = (R(p.Seed + fr, i, 1) - .5f) * m.H, a = R(p.Seed + fr, i, 2) * MathF.Tau, L = c.F(p.ScatterLength); Zap(V3(x, y, -200), V3(x + MathF.Cos(a) * L, y + MathF.Sin(a) * L, 0), fr * 13 + i); }
        });
        S<LensFlareParameter>(d, (p, c, m) => // レンズフレア: 光源の玉と放射の筋、画面の奥から手前へ並ぶ丸いゴースト
        {
            var o = PctPos(m, c.F(p.SourceX), c.F(p.SourceY)); if (p.Sway) o += new Vector2(MathF.Sin(m.Now * .5f) * 30, MathF.Cos(m.Now * .4f) * 15); float it = c.F(p.Intensity) / 100, cs = c.F(p.CoreSize);
            m.Ball(o.X, o.Y, -400, cs * .6f, m.C(0xFFFFFF)); m.Ball(o.X, o.Y, -420, cs * 1.4f, m.C(p.LightColor, .3f * it));
            int rc = Math.Min((int)c.F(p.RayCount), 24); for (int i = 0; i < rc; i++) { float a = i * MathF.Tau / rc + m.Now * .05f, L = c.F(p.RayLength) * (.5f + .5f * R(p.Seed, i)); m.Limb(o.X, o.Y, -400, o.X + MathF.Cos(a) * L, o.Y + MathF.Sin(a) * L, -400, 5, 1, m.C(p.LightColor, .4f * it)); }
            m.Limb(o.X - c.F(p.StreakLength), o.Y, -390, o.X + c.F(p.StreakLength), o.Y, -390, 3, 3, m.C(p.LightColor, .6f * it));
            int g = Math.Min((int)c.F(p.Ghosts), 10); for (int i = 0; i < g; i++) { float f = (i + 1f) / (g + 1) * 2; var q = Vector2.Lerp(o, -o, f / 2); float s = 30 + 80 * R(p.Seed, i, 1); var col = m.C(Pal(m, i + 2), .25f * it); if (p.Hexagon) m.Slab(-400 + f * 350, 6, col, StarPts(q.X, q.Y, s, 6, .87f, 0)); else m.Disc(q.X, q.Y, -400 + f * 350, s, s, 6, col); }
            m.Torus(0, 0, -100, MathF.Max(80, o.Length()), 6, m.C(p.LightColor, .1f * c.F(p.Halo) / 100));
        });
        S<SpotlightParameter>(d, (p, c, m) => // スポットライト: 上の照明器具から円すいの光、床に明るい円
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); var b = PctPos(m, c.F(p.BeamX), c.F(p.BeamY)); float r = c.F(p.Radius), fl = 1 - c.F(p.Flicker) / 100 * .5f * (1 + MathF.Sin(m.Now * 30));
            m.Box(0, 0, -900, m.W * 2, m.H * 2, 10, m.C(p.DarkColor, c.F(p.Darkness) / 100), .01f);
            m.Box(0, m.H / 2 + 10, -400, m.W * 2, 20, 1200, m.C(0x2A2A30), .01f);
            m.Pitch(o.X, m.H / 2, -100, MathF.PI / 2, () => m.Disc(o.X, m.H / 2, -100, r, r * .7f, 4, m.C(p.LightColor, c.F(p.Intensity) / 100 * fl)));
            m.Limb(b.X, b.Y, -100, o.X, m.H / 2, -100, 30, r, m.C(p.LightColor, c.F(p.BeamStrength) / 100 * .3f * fl));
            m.Cyl(b.X, b.Y - 30, -100, 50, 90, m.C(0x37474F)); m.Disc(b.X, b.Y + 18, -100, 44, 44, 4, m.C(p.LightColor));
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.DustCount), 60); i++) { float f = R(p.Seed, i), y = b.Y + (m.H / 2 - b.Y) * f, x = b.X + (o.X - b.X) * f + (R(p.Seed, i, 1) - .5f) * r * f, zz = -100 + (R(p.Seed, i, 2) - .5f) * r * f; m.Ball(x + MathF.Sin(m.Now + i) * 10, y, zz, 3, m.C(p.LightColor, .7f)); } }, 5);
        });
        S<MagicCircleParameter>(d, (p, c, m) => // 魔法陣: 床に寝かせた光の輪と星形、浮かぶ文字の輪、回る光の玉
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), th = c.F(p.Thickness) * 3, k = p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.1f, c.F(p.IntroTime)), 0, 1)) * c.F(p.Appear) / 100 : c.F(p.Appear) / 100, sp = m.Now * c.F(p.Speed) * MathF.PI / 180;
            m.Pitch(o.X, o.Y, 0, 1.05f + c.F(p.Tilt) * MathF.PI / 180, () =>
            {
                m.Torus(o.X, o.Y, 0, r * k, th, m.C(p.Color1)); m.Torus(o.X, o.Y, 0, r * .86f * k, th * .7f, m.C(p.Color1)); m.Torus(o.X, o.Y, 0, r * .45f * k, th * .7f, m.C(p.Color2));
                m.Turn(o.X, o.Y, sp, () =>
                {
                    int n = Math.Max(3, p.PolyPoints), sk = Math.Max(1, p.StarSkip);
                    for (int i = 0; i < n; i++) { float a = i * MathF.Tau / n, b = (i + sk) * MathF.Tau / n; m.Limb(o.X + MathF.Cos(a) * r * .86f * k, o.Y + MathF.Sin(a) * r * .86f * k, 0, o.X + MathF.Cos(b) * r * .86f * k, o.Y + MathF.Sin(b) * r * .86f * k, 0, th * .6f, th * .6f, m.C(p.Color1)); }
                    int rn = Math.Min((int)c.F(p.RuneCount), 48); for (int i = 0; i < rn; i++) { float a = i * MathF.Tau / rn; m.Turn(o.X + MathF.Cos(a) * r * .93f * k, o.Y + MathF.Sin(a) * r * .93f * k, a, () => m.Box(o.X + MathF.Cos(a) * r * .93f * k, o.Y + MathF.Sin(a) * r * .93f * k, 0, 8, 18, 6, m.C(p.Color2), .2f)); }
                });
            });
            for (int i = 0; i < Math.Min((int)c.F(p.Orbs), 8); i++) { float a = -sp * 2 + i * MathF.Tau / MathF.Max(1, c.F(p.Orbs)); m.Ball(o.X + MathF.Cos(a) * r * 1.05f * k, o.Y - 60 + MathF.Sin(a) * r * .5f * k, MathF.Sin(a) * r * .9f * k, 18, m.C(p.Color2)); }
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.DustCount), 50); i++) { float f = Frac(m.Now * .4f + R(p.Seed, i)); m.Ball(o.X + (R(p.Seed, i, 1) - .5f) * r * 1.6f, o.Y - f * r, (R(p.Seed, i, 2) - .5f) * r, 4, m.C(p.Color1, 1 - f)); } }, 5);
        });
        S<SlashParameter>(d, (p, c, m) => // 斬撃: 弧を描く三日月形の光の刃が、続けて走る
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float L = c.F(p.Length), th = c.F(p.Thickness), cv = c.F(p.Curve) / 100, iv = MathF.Max(.3f, c.F(p.Interval)), ph = p.Auto ? Frac(m.Now / iv) * iv : c.F(p.Manual) / 100 * .5f;
            for (int s = 0; s < Math.Clamp((int)c.F(p.Strokes), 1, 6); s++)
            {
                float lp = ph - s * c.F(p.Gap); if (lp < 0) continue; float k = Math.Clamp(lp / .15f, 0, 1), fade = 1 - Math.Clamp((lp - .15f) / .35f, 0, 1); if (fade <= 0) continue;
                float ang = (c.F(p.Angle) + s * c.F(p.AngleStep)) * MathF.PI / 180;
                m.Turn(o.X, o.Y, ang, () =>
                {
                    var pts = new List<Vector3>(); int n = 20; for (int i = 0; i <= n; i++) { float f = i / (float)n * k, x = o.X - L / 2 + L * f; pts.Add(V3(x, o.Y - MathF.Sin(f * MathF.PI) * L * .25f * cv, MathF.Sin(f * MathF.PI) * 60)); }
                    for (int i = 0; i < n; i++) { float w = MathF.Sin((i + .5f) / n * MathF.PI) * th * .5f + 2; m.Limb(pts[i], pts[i + 1], w, w, m.C(0xFFFFFF, fade)); m.Limb(pts[i], pts[i + 1], w * 2, w * 2, m.C(p.GlowColor, .35f * fade)); }
                });
            }
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Sparks), 40); i++) { float a = R(p.Seed, i) * MathF.Tau, v = 600 * R(p.Seed, i, 1); m.Ball(o.X + MathF.Cos(a) * v * ph, o.Y + MathF.Sin(a) * v * ph, MathF.Sin(a * 3) * v * ph * .5f, 4, m.C(p.GlowColor, MathF.Max(0, 1 - ph * 2))); } }, 5);
        });
        S<AuraParameter>(d, (p, c, m) => // オーラ: 体をつつむ、炎のようにゆらめく光の殻と、立ちのぼる光の柱
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float bw = c.F(p.BodyWidth), bh = c.F(p.BodyHeight), it = c.F(p.Intensity) / 100, sp = c.F(p.Speed) / 100;
            if (p.Kind == AuraKind.Pillar) { m.Lathe(o.X, 0, m.C(p.Color1, .35f * it), bw * .5f, o.Y, bw * .6f, o.Y - m.H * 1.5f); m.Lathe(o.X, 0, m.C(p.Color2, .5f * it), bw * .25f, o.Y, bw * .3f, o.Y - m.H * 1.5f); m.Pitch(o.X, o.Y, 0, MathF.PI / 2, () => m.Torus(o.X, o.Y, 0, bw * .7f, 10, m.C(p.Color1, it))); }
            else for (int l = 0; l < 3; l++)
            {
                var prof = new List<Vector2>(); for (int i = 0; i <= 12; i++) { float f = i / 12f, w = bw * (.5f + l * .12f) * MathF.Sin(MathF.Min(1, f * 1.2f) * MathF.PI * .5f + .3f) * (1 + .08f * MathF.Sin(m.Now * 8 * sp + i + l)); prof.Add(new Vector2(MathF.Max(2, f > .9f ? w * (1 - f) * 10 : w), o.Y - bh * (1 + l * .12f) * f)); }
                m.Lathe(o.X, 0, prof, m.C(l == 0 ? p.Color2 : p.Color1, (.35f - l * .08f) * it));
            }
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Particles), 70); i++) { float f = Frac(m.Now * .5f * sp + R(p.Seed, i)), a = R(p.Seed, i, 1) * MathF.Tau; m.Ball(o.X + MathF.Cos(a) * bw * .6f, o.Y - f * bh * 1.3f, MathF.Sin(a) * bw * .6f, 6, m.C(p.Color2, 1 - f)); } }, 5);
        });
        S<SpellParameter>(d, (p, c, m) => // 魔法弾: 属性で形の違う弾(炎・氷のとげ・雷・風の渦・闇・光)が尾を引いて飛ぶ
        {
            var o = PctPos(m, c.F(p.StartX), c.F(p.StartY)); float a = c.F(p.Angle) * MathF.PI / 180, iv = MathF.Max(.3f, c.F(p.Interval)), fl = MathF.Max(.1f, c.F(p.Flight)), ph = Frac(m.Now / iv) * iv, f = Math.Clamp(ph / fl, 0, 1), r = c.F(p.Radius) * 1.3f;
            if (ph > fl + .3f) return; var dir = new Vector2(MathF.Cos(a), MathF.Sin(a)); var q = o + dir * c.F(p.Distance) * f;
            uint[] cols = { 0xFF6A1A, 0x8FE8FF, 0xFFE94A, 0x8CFFB0, 0x7A3CC8, 0xFFF6C8 }; var col = m.C(cols[(int)p.Element]);
            if (ph > fl) { float k = (ph - fl) / .3f; m.Ball(q.X, q.Y, 0, r * (1 + k * 2), m.C(col, (1 - k) * .6f)); return; }
            m.Turn(q.X, q.Y, a, () =>
            {
                switch (p.Element)
                {
                    case ElementKind.Ice: m.Lathe(q.X, 0, col, 0, q.Y - r * 1.4f, r * .6f, q.Y, 0, q.Y + r * .6f); m.Turn(q.X, q.Y, -MathF.PI / 2, () => m.Lathe(q.X, 0, col, 0, q.Y - r * 1.6f, r * .5f, q.Y, 0, q.Y + r * .5f)); break;
                    case ElementKind.Thunder: m.Tube(new List<Vector3> { V3(q.X - r * 2, q.Y, 0), V3(q.X - r, q.Y - r * .5f, 0), V3(q.X, q.Y + r * .4f, 0), V3(q.X + r, q.Y, 0) }, r * .2f, col); break;
                    case ElementKind.Wind: for (int k = 0; k < 3; k++) m.Yaw(q.X, q.Y, 0, MathF.PI / 2, () => m.Torus(q.X - k * r * .5f, q.Y, 0, r * (1 - k * .25f), r * .1f, m.C(col, .7f))); break;
                    default: m.Ball(q.X, q.Y, 0, r * .6f, m.C(0xFFFFFF)); m.Ball(q.X, q.Y, 0, r, m.C(col, .6f)); break;
                }
                m.Limb(q.X, q.Y, 0, q.X - c.F(p.Trail), q.Y, 0, r * .8f, 3, m.C(col, .35f * c.F(p.Brightness) / 100));
            });
        });
        S<SummonParameter>(d, (p, c, m) => // 召喚の光: 床の光の輪と、回りながら立ちのぼる光の柱、上る粒
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), k = p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.1f, c.F(p.IntroTime)), 0, 1)) : 1, it = c.F(p.Brightness) / 100, sp = m.Now * c.F(p.Spin) / 100;
            m.Pitch(o.X, o.Y, 0, MathF.PI / 2, () => { m.Torus(o.X, o.Y, 0, r * k, 8, m.C(p.Color1, it)); m.Torus(o.X, o.Y, 0, r * .7f * k, 5, m.C(p.Color2, it)); m.Turn(o.X, o.Y, sp, () => { for (int i = 0; i < 6; i++) { float a = i * MathF.PI / 3; m.Limb(o.X + MathF.Cos(a) * r * .7f * k, o.Y + MathF.Sin(a) * r * .7f * k, 0, o.X + MathF.Cos(a + 2.1f) * r * .7f * k, o.Y + MathF.Sin(a + 2.1f) * r * .7f * k, 0, 4, 4, m.C(p.Color1, it)); } }); });
            m.Lathe(o.X, 0, m.C(p.Color1, .25f * it * k), r * .9f, o.Y, r * .7f * k, o.Y - m.H * k); m.Lathe(o.X, 0, m.C(p.Color2, .3f * it * k), r * .3f, o.Y, r * .2f, o.Y - m.H * k);
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Particles), 90); i++) { float f = Frac(m.Now * .5f + R(p.Seed, i)), a = R(p.Seed, i, 1) * MathF.Tau + sp * 2, rr = r * (.3f + .6f * R(p.Seed, i, 2)) * k; m.Ball(o.X + MathF.Cos(a) * rr, o.Y - f * m.H * .9f, MathF.Sin(a) * rr, 5, m.C(p.Color2, 1 - f)); } }, 5);
        });
        S<FeathersParameter>(d, (p, c, m) => // 羽根: 軸と左右の羽のある白い羽根が、ゆっくり揺れて落ちる
        {
            float sz = c.F(p.Size) * 1.5f;
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 50); i++)
            {
                float y = -m.H / 2 - sz + Cyc(m.Now * c.F(p.Fall) * (.6f + .4f * R(p.Seed, i, 3)) + R(p.Seed, i, 1) * m.H * 1.2f, m.H * 1.2f), x = (R(p.Seed, i) - .5f) * m.W * 1.3f + MathF.Sin(m.Now * 1.1f + i) * c.F(p.Sway), z = -900 + R(p.Seed, i, 2) * 1000, sw = MathF.Sin(m.Now * 1.6f + i) * c.F(p.Spin) / 100;
                m.Turn(x, y, sw + .4f, () => m.Yaw(x, y, z, sw, () => { m.Ball(x, y, z, sz * .25f, sz, 3, m.C(p.FeatherColor)); m.Limb(x, y - sz, z + 2, x, y + sz * 1.2f, z + 2, 2, 1.5f, m.C(0xE0E0E0)); }));
                if (p.Glow) m.Ball(x, y, z, sz * .6f, sz * 1.2f, 6, m.C(p.FeatherColor, .12f));
            }
        });
        S<ConfettiParameter>(d, (p, c, m) => // 紙吹雪: 色とりどりの四角い紙が、ひらひら回って落ちる
        {
            float sz = c.F(p.Size) * 1.6f;
            Low(m, () =>
            {
                for (int i = 0; i < Math.Min((int)c.F(p.Count), 200); i++)
                {
                    float y = -m.H / 2 - 30 + Cyc(m.Now * c.F(p.Fall) * (.6f + .4f * R(p.Seed, i, 3)) + R(p.Seed, i, 1) * m.H * 1.2f, m.H * 1.2f), x = (R(p.Seed, i) - .5f) * m.W * 1.3f + MathF.Sin(m.Now * 2 + i) * c.F(p.Sway), z = -900 + R(p.Seed, i, 2) * 1100, sp = m.Now * c.F(p.Spin) / 25 + i;
                    var col = p.Colorful ? Pal(m, i) : m.C(p.Color); col = m.C(col, c.F(p.Opacity) / 100);
                    m.Pitch(x, y, z, sp, () => m.Turn(x, y, sp * .6f, () => m.Box(x, y, z, sz, sz * .6f, 2, col, .05f)));
                }
            }, 6);
        });
        S<CrackerParameter>(d, (p, c, m) => // クラッカー: 円すいの筒から、紙片とテープが勢いよく飛び出す
        {
            var o = PctPos(m, c.F(p.OriginX), c.F(p.OriginY)); float dir = c.F(p.Direction) * MathF.PI / 180, iv = MathF.Max(.5f, c.F(p.Interval)), ph = Frac(m.Now / iv) * iv, pw = c.F(p.Power), g = c.F(p.Gravity), spr = c.F(p.Spread) * MathF.PI / 180;
            m.Turn(o.X, o.Y, dir, () => { m.Lathe(o.X, 0, m.C(0xFFD54F), 6, o.Y + 120, 70, o.Y); m.Torus(o.X, o.Y, 0, 70, 6, m.C(0xE53935)); m.Pitch(o.X, o.Y, 0, MathF.PI / 2, () => m.Torus(o.X, o.Y, 0, 70, 8, m.C(0xE53935))); });
            if (ph > c.F(p.Life)) return;
            if (p.Flash && ph < .15f) m.Ball(o.X, o.Y - 60, 0, 120 * (1 - ph / .15f), m.C(0xFFF8E1, .7f));
            Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Count), 180); i++) { float a = -MathF.PI / 2 + dir + (R(p.Seed, i) - .5f) * spr * 2, v = pw * (.4f + .6f * R(p.Seed, i, 1)), drag = 1 - MathF.Exp(-ph * 2.5f); float x = o.X + MathF.Cos(a) * v * drag / 2.5f, y = o.Y + MathF.Sin(a) * v * drag / 2.5f + g * ph * ph * .5f, z = (R(p.Seed, i, 2) - .5f) * v * drag / 2.5f * .8f; var col = p.Colorful ? Pal(m, i) : m.C(p.Color); m.Pitch(x, y, z, ph * 10 + i, () => m.Box(x, y, z, c.F(p.Size) * 1.6f, c.F(p.Size), 2, col, .05f)); } }, 6);
            for (int i = 0; i < Math.Min((int)c.F(p.Streamers), 20); i++) { float a = -MathF.PI / 2 + dir + (R(p.Seed, i, 4) - .5f) * spr * 1.6f, v = pw * .35f * (.5f + .5f * R(p.Seed, i, 5)), drag = 1 - MathF.Exp(-ph * 2.5f); var pts = new List<Vector3>(); for (int k = 0; k <= 8; k++) { float f = k / 8f; pts.Add(V3(o.X + MathF.Cos(a) * v * drag * f + MathF.Sin(f * 9 + i) * 20, o.Y + MathF.Sin(a) * v * drag * f + g * ph * ph * .3f * f, (R(p.Seed, i, 6) - .5f) * 300 * f)); } m.Tube(pts, 4, Pal(m, i + 3)); }
        });
        S<FuturePanelParameter>(d, (p, c, m) => // 近未来図形: 角を落とした光る枠の板、格子と走査線、角の太い線
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float pw = c.F(p.PanelWidth), ph = c.F(p.PanelHeight), ch = c.F(p.Chamfer), k = p.Intro ? Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.IntroTime)), 0, 1)) * c.F(p.Appear) / 100 : c.F(p.Appear) / 100;
            float w = pw * k, h = ph; var pts = new[] { -w / 2 + ch, -h / 2, w / 2, -h / 2, w / 2, h / 2 - ch, w / 2 - ch, h / 2, -w / 2, h / 2, -w / 2, -h / 2 + ch };
            var xs = new float[12]; for (int i = 0; i < 12; i += 2) { xs[i] = o.X + pts[i]; xs[i + 1] = o.Y + pts[i + 1]; }
            m.Slab(-10, 20, m.C(p.FillColor, c.F(p.FillOpacity) / 100), xs);
            var line = new List<Vector3>(); for (int i = 0; i <= 12; i += 2) line.Add(V3(xs[i % 12], xs[(i + 1) % 12], 12)); m.Tube(line, c.F(p.Thickness) * 2, m.C(p.LineColor));
            m.Limb(xs[0], xs[1], 14, xs[0] + w * .3f, xs[1], 14, c.F(p.AccentThickness) * .7f, c.F(p.AccentThickness) * .7f, m.C(p.LineColor)); m.Limb(xs[6], xs[7], 14, xs[6] - w * .3f, xs[7], 14, c.F(p.AccentThickness) * .7f, c.F(p.AccentThickness) * .7f, m.C(p.LineColor));
            if (p.ShowGrid) { float gs = MathF.Max(10, c.F(p.GridSize)); for (float x = -w / 2 + gs; x < w / 2; x += gs) m.Box(o.X + x, o.Y, 10, 1.5f, h - 10, 2, m.C(p.LineColor, .2f)); }
            if (p.ScanLine) { float y = o.Y - h / 2 + Cyc(m.Now * 200, h); m.Box(o.X, y, 14, w - 10, 4, 4, m.C(p.LineColor, .7f)); }
            if (p.Blink) m.Ball(o.X + w / 2 - ch, o.Y - h / 2 + 24, 16, 8, m.C(p.LineColor, MathF.Sin(m.Now * 6) > 0 ? 1 : .2f));
        });
        S<FutureShapeParameter>(d, (p, c, m) => // HUD風図形: 重なって逆回転する目盛りの輪、角の括弧、周回する点
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size), th = c.F(p.Thickness) * 2, sp = m.Now * c.F(p.Speed) * MathF.PI / 180, op = c.F(p.Opacity) / 100;
            int L = Math.Clamp((int)c.F(p.Layers), 1, 8);
            for (int l = 0; l < L; l++) { float r = s * (1 - l * .15f), z = l * 25; m.Turn(o.X, o.Y, sp * (l % 2 == 0 ? 1 : -1.4f), () => m.Tube(ArcPts3(o.X, o.Y, r, l * .7f, l * .7f + MathF.PI * (1.2f + .15f * l), z, 40), th, m.C(l % 2 == 0 ? p.MainColor : p.SubColor, op))); }
            if (p.ShowTicks) { int n = 60; m.Turn(o.X, o.Y, -sp * .5f, () => { for (int i = 0; i < n; i++) { float a = i * MathF.Tau / n, L1 = i % 5 == 0 ? 22 : 10; m.Limb(o.X + MathF.Cos(a) * s * 1.1f, o.Y + MathF.Sin(a) * s * 1.1f, -10, o.X + MathF.Cos(a) * (s * 1.1f + L1), o.Y + MathF.Sin(a) * (s * 1.1f + L1), -10, 2, 2, m.C(p.SubColor, op * c.F(p.Ticks) / 100)); } }); }
            if (p.ShowBrackets) foreach (int a in Sides) foreach (int b in Sides) { float x = o.X + a * s * 1.35f, y = o.Y + b * s * 1.35f; m.Limb(x, y, 0, x - a * 50, y, 0, th, th, m.C(p.MainColor, op * c.F(p.Brackets) / 100)); m.Limb(x, y, 0, x, y - b * 50, 0, th, th, m.C(p.MainColor, op * c.F(p.Brackets) / 100)); }
            if (p.ShowNodes) for (int i = 0; i < 4; i++) { float a = sp * 2 + i * MathF.PI / 2; m.Ball(o.X + MathF.Cos(a) * s * .9f, o.Y + MathF.Sin(a) * s * .9f, 30, 10, m.C(p.MainColor, op)); }
            m.Ball(o.X, o.Y, 0, 14, m.C(p.MainColor, op));
        });
        S<NeonShapeParameter>(d, (p, c, m) => // ネオン図形: 光る管で作った多角形・星が、奥へ何重にも並ぶ
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size), th = c.F(p.Thickness) * 1.6f, fl = 1 - c.F(p.Flicker) / 100 * (MathF.Sin(m.Now * 40) > .7f ? 1 : 0), br = c.F(p.Brightness) / 100 * fl;
            int cp = Math.Clamp((int)c.F(p.Copies), 1, 8), n = Math.Max(3, p.PolygonSides);
            for (int k = 0; k < cp; k++)
            {
                float sc = MathF.Pow(c.F(p.Step) / 100, k), z = -k * 120; var col = Sculpt.Mix(m.C(p.Color1), m.C(p.Color2), cp == 1 ? 0 : k / (float)(cp - 1));
                m.Turn(o.X, o.Y, m.Now * c.F(p.Speed) * MathF.PI / 180 * (k % 2 == 0 ? 1 : -1), () =>
                {
                    var pts = new List<Vector3>(); for (int i = 0; i <= n; i++) { float a = -MathF.PI / 2 + i * MathF.Tau / n; pts.Add(V3(o.X + MathF.Cos(a) * s * sc, o.Y + MathF.Sin(a) * s * sc, z)); }
                    m.Tube(pts, th, m.C(0xFFFFFF, br)); m.Tube(pts, th + c.F(p.Glow) * .35f, m.C(col, .3f * br));
                });
            }
        });
        S<HologramParameter>(d, (p, c, m) => // ホログラム: 投影台の上で回る、半透明の立方体・球・八面体と走査線
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size), br = c.F(p.Brightness) / 100 * (1 - c.F(p.Flicker) / 100 * (MathF.Sin(m.Now * 23) > .8f ? .6f : 0)), rot = m.Now * c.F(p.RotSpeed) * MathF.PI / 180;
            var col = m.C(p.Color, .35f * br); var edge = m.C(p.Color, .9f * br);
            m.Pitch(o.X, o.Y, 0, c.F(p.Tilt) * MathF.PI / 180, () => m.Yaw(o.X, o.Y, 0, rot, () =>
            {
                if (p.Kind == HoloKind.Cube) { m.Box(o.X, o.Y, 0, s * 1.2f, s * 1.2f, s * 1.2f, col, .05f); foreach (int a in Sides) foreach (int b in Sides) { m.Limb(o.X - s * .6f, o.Y + a * s * .6f, b * s * .6f, o.X + s * .6f, o.Y + a * s * .6f, b * s * .6f, 3, 3, edge); m.Limb(o.X + a * s * .6f, o.Y - s * .6f, b * s * .6f, o.X + a * s * .6f, o.Y + s * .6f, b * s * .6f, 3, 3, edge); m.Limb(o.X + a * s * .6f, o.Y + b * s * .6f, -s * .6f, o.X + a * s * .6f, o.Y + b * s * .6f, s * .6f, 3, 3, edge); } }
                else if (p.Kind == HoloKind.Sphere) { m.Ball(o.X, o.Y, 0, s * .8f, col); for (int i = 0; i < 4; i++) m.Yaw(o.X, o.Y, 0, i * MathF.PI / 4, () => m.Torus(o.X, o.Y, 0, s * .8f, 2.5f, edge)); }
                else { m.Lathe(o.X, 0, col, 0, o.Y - s, s * .8f, o.Y, 0, o.Y + s); for (int i = 0; i < 4; i++) { float a = i * MathF.PI / 2; m.Limb(o.X, o.Y - s, 0, o.X + MathF.Cos(a) * s * .8f, o.Y, MathF.Sin(a) * s * .8f, 3, 3, edge); m.Limb(o.X, o.Y + s, 0, o.X + MathF.Cos(a) * s * .8f, o.Y, MathF.Sin(a) * s * .8f, 3, 3, edge); } }
            }));
            float sy = o.Y - s + Cyc(m.Now * 300, s * 2); m.Pitch(o.X, sy, 0, MathF.PI / 2, () => m.Disc(o.X, sy, 0, s * .9f, s * .9f, 2, m.C(p.Color, .3f * c.F(p.Scan) / 100 * br)));
            if (p.ShowBase) { m.Cyl(o.X, o.Y + s * 1.35f, 0, s * .7f, 40, m.C(0x263238)); m.Pitch(o.X, o.Y + s * 1.3f, 0, MathF.PI / 2, () => m.Torus(o.X, o.Y + s * 1.3f, 0, s * .6f, 5, edge)); m.Lathe(o.X, 0, m.C(p.Color, .12f * br), s * .6f, o.Y + s * 1.3f, s * .9f, o.Y - s * .9f); }
        });
        S<CircuitParameter>(d, (p, c, m) => // 回路パターン: 基板の板に、直角に曲がる配線の筋と流れる光、四角い部品
        {
            m.Box(0, 0, -30, m.W * 1.1f, m.H * 1.1f, 20, m.C(p.BgColor, MathF.Max(.3f, c.F(p.BgOpacity) / 100)), .01f);
            float cs = c.F(p.CellSize) * 2, th = c.F(p.Thickness) * 2.5f;
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 50); i++)
            {
                var pts = new List<Vector3>(); float x = MathF.Round((R(p.Seed, i) - .5f) * m.W / cs) * cs, y = MathF.Round((R(p.Seed, i, 1) - .5f) * m.H / cs) * cs; pts.Add(V3(x, y, -16));
                for (int k = 0; k < 4; k++) { int dir = (int)(R(p.Seed, i * 7 + k, 2) * 4); float L = cs * (2 + (int)(R(p.Seed, i * 7 + k, 3) * 5)); if (dir == 0) x += L; else if (dir == 1) x -= L; else if (dir == 2) y += L; else y -= L; pts.Add(V3(x, y, -16)); }
                m.Tube(pts, th, m.C(p.TraceColor)); m.Disc(pts[0].X, pts[0].Y, -14, th * 2.5f, th * 2.5f, 6, m.C(p.TraceColor)); m.Disc(x, y, -14, th * 2.5f, th * 2.5f, 6, m.C(p.TraceColor));
                float f = Frac(m.Now * c.F(p.PulseSpeed) / 20 + R(p.Seed, i, 4)) * (pts.Count - 1); int sgi = Math.Min((int)f, pts.Count - 2); var q = Vector3.Lerp(pts[sgi], pts[sgi + 1], f - sgi); m.Ball(q.X, q.Y, q.Z + 4, th * 2, m.C(p.PulseColor, c.F(p.Glow) / 100));
            }
            if (p.ShowChips) for (int i = 0; i < Math.Min((int)c.F(p.ChipCount), 12); i++) { float x = (R(p.Seed, i, 5) - .5f) * m.W * .8f, y = (R(p.Seed, i, 6) - .5f) * m.H * .8f; m.Box(x, y, 0, 140, 100, 30, m.C(0x1B1F24), .1f); for (int k = 0; k < 6; k++) foreach (int s in Sides) m.Box(x - 60 + k * 24, y + s * 58, -6, 8, 18, 6, m.C(0xB0BEC5), .1f); }
        });
        S<DigitalRainParameter>(d, (p, c, m) => // デジタルレイン: 奥行きのある何列もの光る文字の列が、上から流れ落ちる
        {
            float cs = c.F(p.CellSize) * 2.6f; int cols = (int)(m.W / cs * .8f), rows = (int)c.F(p.Trail);
            for (int i = 0; i < cols; i++)
            {
                if (R(p.Seed, i, 9) > c.F(p.Density) / 100) continue; float x = (i + .5f) / cols * m.W * 1.3f - m.W * .65f, z = -900 + R(p.Seed, i, 1) * 1100, head = -m.H / 2 + Cyc(m.Now * c.F(p.Speed) * cs * (.6f + .4f * R(p.Seed, i, 2)) + R(p.Seed, i) * m.H * 2, m.H * 2);
                for (int k = 0; k < rows; k++)
                {
                    float y = head - k * cs; if (y < -m.H * .7f || y > m.H * .7f) continue; var col = k == 0 ? m.C(p.HeadColor) : m.C(p.Color, (1 - k / (float)rows) * c.F(p.Opacity) / 100);
                    int g = (int)(R(p.Seed + (int)(m.Now * c.F(p.ChangeRate)), i * 50 + k) * 36);
                    if (p.Glyph == RainGlyph.Blocks) m.Box(x, y, z, cs * .7f, cs * .8f, cs * .3f, col, .1f);
                    else m.Text(p.Glyph == RainGlyph.Binary ? (g % 2).ToString() : ((char)(0x30A2 + g % 40)).ToString(), cs, x, y, z, 2, col);
                }
            }
        });
    }
}
