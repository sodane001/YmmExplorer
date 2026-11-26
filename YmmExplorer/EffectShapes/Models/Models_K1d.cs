using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 91〜120番(HUD・季節・UI・ゲーム・解説)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>魚(紡錘形の体・尾びれ・目)。x+ 向き</summary>
    static void Fish(Sculpt m, float x, float y, float z, float s, float wag, Vector4 col)
    {
        m.Ball(x, y, z, s, s * .45f, s * .3f, col);
        m.Yaw(x - s * .9f, y, z, wag, () => m.Slab(z - 3, 6, m.C(Sculpt.Dark(col, .8f), 1), x - s * .8f, y, x - s * 1.5f, y - s * .45f, x - s * 1.5f, y + s * .45f));
        m.Ball(x + s * .6f, y - s * .1f, z + s * .22f, s * .09f, new Vector4(.05f, .05f, .08f, col.W));
    }
    /// <summary>金貨(縁のある円盤)</summary>
    static void Coin(Sculpt m, float x, float y, float z, float r, float spin, Vector4 col)
    {
        m.Yaw(x, y, z, spin, () => { m.Disc(x, y, z, r, r, r * .25f, col, .5f); m.Torus(x, y, z, r * .8f, r * .06f, m.C(Sculpt.Dark(col, .8f), 1)); });
    }

    static partial void RegisterK1d(Dictionary<Type, SceneModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.08f, .09f, .11f, 1);
        S<CockpitHudParameter>(d, (p, c, m) => // コックピットHUD: 前のガラス面に浮かぶ、傾く水平線のはしご、左右の速度・高度の目盛り帯、方位
        {
            float op = c.F(p.Opacity) / 100, sc = c.F(p.HudScale) / 100, th = c.F(p.Thickness) * 2; var col = m.C(p.Color, op);
            float roll = (c.F(p.Roll) + (p.Auto ? MathF.Sin(m.Now * .7f) * 15 : 0)) * MathF.PI / 180, pitch = c.F(p.Pitch) + (p.Auto ? MathF.Sin(m.Now * .5f) * 8 : 0), hd = c.F(p.Heading) + (p.Auto ? m.Now * 3 : 0);
            m.Scale(0, 0, 0, sc, () =>
            {
                m.Turn(0, 0, roll, () => { for (int i = -3; i <= 3; i++) { float y = (i * 10 + pitch) * 12; if (MathF.Abs(y) > 360) continue; foreach (int s in Sides) m.Limb(s * 80, y, 0, s * (i == 0 ? 600 : 220), y, 0, th, th, col); if (i != 0) m.Text($"{i * 10}", 26, 260, y, 0, 2, col); } });
                m.Limb(-60, 0, 30, -20, 20, 30, th, th, col); m.Limb(-20, 20, 30, 20, 20, 30, th, th, col); m.Limb(20, 20, 30, 60, 0, 30, th, th, col);
                if (p.Tapes) foreach (int s in Sides) { m.Box(s * 640, 0, -10, 130, 520, 10, m.C(p.Color, .12f * op), .05f); for (int i = -5; i <= 5; i++) { float y = i * 50 + Cyc(m.Now * 40 * s, 50); m.Limb(s * 640 - s * 60, y, 0, s * 640 - s * 30, y, 0, 2, 2, col); } m.Box(s * 640, 0, 10, 140, 50, 12, m.C(0x000000u, .7f * op), .2f); m.Text(s < 0 ? $"{(int)c.F(p.Speed)}" : $"{(int)c.F(p.Altitude)}", 32, s * 640, 0, 18, 2, col); }
                m.Box(0, -420, -10, 700, 50, 10, m.C(p.Color, .12f * op), .05f); for (int i = -6; i <= 6; i++) { float x = i * 55 - (hd % 10) * 5.5f; m.Limb(x, -440, 0, x, -420, 0, 2, 2, col); } m.Text($"{((int)hd % 360 + 360) % 360:000}", 32, 0, -380, 0, 2, col);
                if (p.Minimap) { m.Disc(620, 380, 0, 110, 110, 10, m.C(0x000000u, .5f * op)); m.Torus(620, 380, 6, 110, 3, col); m.Slab(4, 8, col, 620, 350, 640, 400, 600, 400); }
            });
        });
        S<DnaMoleculeParameter>(d, (p, c, m) => // DNA: 2本のらせんの玉の鎖と、つなぐ横の棒/玉と棒の分子模型
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float L = c.F(p.Length), r = c.F(p.HelixRadius), turns = c.F(p.Turns), spin = m.Now * c.F(p.Spin) * MathF.PI / 180, as2 = c.F(p.AtomSize);
            if (p.Mode != DnaMode.Molecule) m.Turn(o.X, o.Y, c.F(p.Angle) * MathF.PI / 180, () =>
            {
                int n = (int)(turns * 12);
                for (int i = 0; i <= n; i++)
                {
                    float f = i / (float)n, x = o.X - L / 2 + L * f, a = f * turns * MathF.Tau + spin;
                    var p1 = V3(x, o.Y + MathF.Cos(a) * r, MathF.Sin(a) * r); var p2 = V3(x, o.Y - MathF.Cos(a) * r, -MathF.Sin(a) * r);
                    m.Ball(p1.X, p1.Y, p1.Z, as2 * .35f, m.C(p.Color1)); m.Ball(p2.X, p2.Y, p2.Z, as2 * .35f, m.C(p.Color2));
                    if (i % 2 == 0) m.Limb(p1, p2, as2 * .08f, as2 * .08f, m.C(p.BondColor, .8f));
                }
            });
            if (p.Mode != DnaMode.Helix)
            {
                var ctr = p.Mode == DnaMode.Both ? o + new Vector2(0, L * .45f) : o; var atoms = new List<Vector3>();
                for (int i = 0; i < 7; i++) { float a = i * MathF.Tau / 6 + spin; atoms.Add(i == 0 ? V3(ctr.X, ctr.Y, 0) : V3(ctr.X + MathF.Cos(a) * as2 * 3, ctr.Y + MathF.Sin(a * 1.3f) * as2 * 1.5f, MathF.Sin(a) * as2 * 3)); }
                for (int i = 1; i < 7; i++) { m.Limb(atoms[0], atoms[i], as2 * .15f, as2 * .15f, m.C(p.BondColor)); m.Ball(atoms[i].X, atoms[i].Y, atoms[i].Z, as2 * .6f, i % 2 == 0 ? m.C(p.Color1) : m.C(p.Color2)); }
                m.Ball(atoms[0].X, atoms[0].Y, atoms[0].Z, as2, m.C(0x505A66u));
            }
        });
        S<RetrowaveParameter>(d, (p, c, m) => // レトロウェーブ: 奥へ流れる光る格子の床、しまの入った大きな夕日、奥の山並み
        {
            float hy = (c.F(p.Horizon) / 100 - .5f) * m.H, op = c.F(p.BgOpacity) / 100;
            SkyBands(m, m.C(p.SkyTop, op), m.C(p.SkyBottom, op), -2000, 12);
            float ss = c.F(p.SunSize) * 2.4f, sy = hy * 2.2f - ss * .3f;
            for (int i = 0; i < 10; i++) { float f = i / 10f, y0 = sy - ss + f * ss * 2, y1 = y0 + ss * .2f - (p.SunStripes && f > .5f ? ss * (.02f + f * .06f) : 0); if (y0 > hy * 2.3f) continue; float half = ss * MathF.Sqrt(MathF.Max(0, 1 - MathF.Pow((y0 + ss * .1f - sy) / ss, 2))); m.Box(0, (y0 + y1) / 2, -1900, half * 2, MathF.Max(2, y1 - y0), 10, Sculpt.Mix(m.C(p.SunTop), m.C(p.SunBottom), f), .05f); }
            if (p.Mountains) m.Slab(-1700, 40, m.C(0x1A0838u), -m.W * 1.6f, hy * 2, -m.W, hy * 2 - 220, -m.W * .5f, hy * 2 - 120, -m.W * .1f, hy * 2 - 300, m.W * .5f, hy * 2 - 150, m.W, hy * 2 - 260, m.W * 1.6f, hy * 2);
            var gc = m.C(p.GridColor); float gsp = c.F(p.GridSpacing) * 2, off = Cyc(m.Now * c.F(p.Speed) * 5, gsp);
            m.Box(0, m.H / 2 + 2, -1000, m.W * 4, 4, 2000, m.C(0x0A0218u), .01f);
            for (int i = -14; i <= 14; i++) m.Limb(i * gsp * .6f, m.H / 2, 200, i * gsp * 1.5f, m.H / 2, -2000, 3, 3, gc);
            for (int k = 0; k < 14; k++) { float z = 200 - k * gsp + off; m.Limb(-m.W * 2, m.H / 2, z, m.W * 2, m.H / 2, z, 3, 3, gc); }
            if (p.Stars) Low(m, () => { for (int i = 0; i < 80; i++) m.Ball((R(p.Seed, i) - .5f) * m.W * 3, -m.H * (.2f + R(p.Seed, i, 1)), -1950, 3, W); }, 5);
        });
        S<FishSchoolParameter>(d, (p, c, m) => // 魚の群れ: 青い水の中を群れで回る魚、ふわふわ上下するくらげ、光の筋と泡
        {
            float op = c.F(p.BgOpacity) / 100; SkyBands(m, m.C(p.WaterTop, op), m.C(p.WaterBottom, op), -1800, 10);
            if (p.Rays) for (int i = 0; i < 6; i++) { float x = (i - 2.5f) * 300 + MathF.Sin(m.Now * .3f + i) * 40; m.Limb(x, -m.H * .7f, -1200, x + 200, m.H * .7f, -400, 30, 90, m.C(0xFFFFFFu, .07f)); }
            int sch = Math.Clamp((int)c.F(p.Schools), 1, 4), n = Math.Min((int)c.F(p.Count), 60); float sz = c.F(p.Size) * 1.6f, sp = c.F(p.Speed) / 100;
            for (int g = 0; g < sch; g++)
            {
                float a0 = m.Now * .3f * sp + g * 2.1f, cx = MathF.Cos(a0) * m.W * .3f, cz = -600 + MathF.Sin(a0) * 400, cy = (g - (sch - 1) / 2f) * 260, dir = -MathF.Sin(a0) * m.W * .3f, dz = MathF.Cos(a0) * 400, yaw = MathF.Atan2(-dz, dir);
                for (int i = 0; i < n / sch; i++) { float x = cx + (R(p.Seed + g, i) - .5f) * 360, y = cy + (R(p.Seed + g, i, 1) - .5f) * 180, z = cz + (R(p.Seed + g, i, 2) - .5f) * 300; m.Yaw(x, y, z, yaw, () => Fish(m, x, y, z, sz * (.8f + .4f * R(p.Seed + g, i, 3)), MathF.Sin(m.Now * 10 + i) * .4f, m.C(p.FishColor))); }
            }
            for (int j = 0; j < Math.Min((int)c.F(p.JellyCount), 10); j++)
            {
                float x = (R(p.Seed, j, 5) - .5f) * m.W * 1.2f, y = (R(p.Seed, j, 6) - .5f) * m.H * .8f - MathF.Sin(m.Now + j) * 40, z = -900 + R(p.Seed, j, 7) * 600, js = c.F(p.JellySize) * 1.5f, pu = 1 + .1f * MathF.Sin(m.Now * 3 + j);
                m.Lathe(x, z, m.C(p.JellyColor, .55f), js * pu, y, js * .9f * pu, y - js * .5f, 0, y - js * .8f);
                for (int k = 0; k < 5; k++) { float a = k * MathF.Tau / 5; m.Tube(new List<Vector3> { V3(x + MathF.Cos(a) * js * .6f, y, z + MathF.Sin(a) * js * .6f), V3(x + MathF.Cos(a) * js * .5f + MathF.Sin(m.Now * 2 + k) * 15, y + js, z + MathF.Sin(a) * js * .5f), V3(x + MathF.Cos(a) * js * .4f, y + js * 2, z + MathF.Sin(a) * js * .4f) }, 3, m.C(p.JellyColor, .5f)); }
            }
            if (p.Bubbles) for (int i = 0; i < 20; i++) { float y = m.H / 2 - Cyc(m.Now * 120 + R(p.Seed, i, 8) * m.H, m.H); m.Ball((R(p.Seed, i, 9) - .5f) * m.W, y, -500 + R(p.Seed, i, 10) * 500, 6 + 8 * R(p.Seed, i, 11), m.C(0xDFF6FFu, .45f)); }
        });
        S<SkyLanternsParameter>(d, (p, c, m) => // 空のランタン: 下が開いた紙の灯籠が、中の火で光りながら奥行きの中を上っていく
        {
            if (p.NightSky) SkyBands(m, m.C(p.SkyTop), m.C(p.SkyBottom), -2000, 8);
            float sz = c.F(p.Size) * 1.4f;
            for (int i = 0; i < Math.Min((int)c.F(p.Count), 60); i++)
            {
                float z = -1600 + R(p.Seed, i, 2) * 1700, life = 14, f = Frac(m.Now * c.F(p.Rise) / 100 / life + R(p.Seed, i)), x = (R(p.Seed, i, 1) - .5f) * m.W * 1.6f + MathF.Sin(m.Now * .6f + i) * c.F(p.Sway) * .6f, y = m.H * .7f - f * m.H * 1.6f, s = sz * (.6f + .4f * R(p.Seed, i, 3)), fl = .8f + .2f * MathF.Sin(m.Now * 9 + i);
                m.Lathe(x, z, m.C(p.Color, .9f * fl), s * .45f, y + s * .7f, s * .55f, y, s * .45f, y - s * .7f, s * .1f, y - s * .78f);
                m.Ball(x, y + s * .55f, z, s * .18f, m.C(0xFFF3C4u, fl)); m.Ball(x, y, z, s * 1.2f, m.C(p.Color, .1f * c.F(p.Glow) / 100 * fl));
            }
        });
        S<IlluminationParameter>(d, (p, c, m) => // イルミネーション: たるんだ電線に並ぶ電球の玉と、光の玉で巻かれた円すいのツリー
        {
            var cols = new[] { p.Color1, p.Color2, p.Color3, p.ColorD }; float sp = c.F(p.Speed) / 100, bs = c.F(p.BulbSize) * 2.5f, gl = c.F(p.Glow) / 100;
            float On(int i) { float t = m.Now * sp * 3; return p.Pattern switch { IllumPattern.Chase => (i + (int)(t * 2)) % 4 == 0 ? 1 : .2f, IllumPattern.Alternate => ((i + (int)t) % 2 == 0) ? 1 : .2f, IllumPattern.Twinkle => .4f + .6f * MathF.Abs(MathF.Sin(t + i * 1.7f)), IllumPattern.Random => R(p.Seed + (int)(t * 2), i) > .4f ? 1 : .2f, IllumPattern.Wave => .5f + .5f * MathF.Sin(t * 2 - i * .3f), _ => 1 }; }
            Vector4 Bulb(int i) { var col = p.Multicolor ? m.C(cols[i % 4]) : m.C(p.Color1); if (p.Pattern == IllumPattern.Cycle) col = m.C(cols[(i + (int)(m.Now * sp * 2)) % 4]); return col; }
            for (int s = 0; s < Math.Clamp((int)c.F(p.Strings), 0, 6); s++)
            {
                float y0 = -m.H / 2 + 60 + s * 120, sag = c.F(p.Sag) * 2, z = -200 - s * 150; var wire = new List<Vector3>(); int n = (int)(m.W * 1.2f / MathF.Max(20, c.F(p.Spacing) * 1.5f));
                for (int i = 0; i <= n; i++) { float f = i / (float)n, x = -m.W * .6f + m.W * 1.2f * f, y = y0 + MathF.Sin(f * MathF.PI * 2) * MathF.Sin(f * MathF.PI * 2) * sag * .5f + sag * 4 * f * (1 - f) * .5f; wire.Add(V3(x, y, z)); }
                m.Tube(wire, 2, m.C(p.WireColor));
                for (int i = 0; i < wire.Count; i++) { float on = On(i + s * 7); var q = wire[i]; m.Lathe(q.X, q.Z, m.C(Bulb(i + s), .4f + .6f * on), bs * .5f, q.Y + 4, bs * .7f, q.Y + bs * 1.2f, 0, q.Y + bs * 2); if (on > .6f) m.Ball(q.X, q.Y + bs * 1.2f, q.Z, bs * 2.2f * gl, m.C(Bulb(i + s), .15f)); }
            }
            if (p.Tree)
            {
                float tx = (c.F(p.TreeX) / 100 - .5f) * m.W, th = c.F(p.TreeHeight), base0 = m.H / 2 - 40;
                m.Lathe(tx, -300, m.C(0x1B4D2Bu), th * .35f, base0, th * .25f, base0 - th * .35f, th * .2f, base0 - th * .35f, 2, base0 - th); m.Cyl(tx, base0 + 20, -300, 30, 60, m.C(0x5D4037u));
                m.Slab(-310, 20, m.C(0xFFE082u), StarPts(tx, base0 - th - 20, 40, 5, .45f, 0));
                int nb = Math.Min((int)c.F(p.TreeBulbs), 160); for (int i = 0; i < nb; i++) { float f = i / (float)nb, y = base0 - th * f * .95f, r = th * .35f * (1 - f) + 6, a = f * 30; m.Ball(tx + MathF.Cos(a) * r, y, -300 + MathF.Sin(a) * r, bs * .9f, m.C(Bulb(i), .4f + .6f * On(i))); }
            }
        });
        S<WagaraParameter>(d, (p, c, m) => // 和柄の背景: 板の上に浮き彫りの青海波・麻の葉・七宝・市松・矢絣・亀甲
        {
            var bg = m.C(p.BgColor); var lc = m.C(p.LineColor, c.F(p.Opacity) / 100); float cs = MathF.Max(30, c.F(p.CellSize)) * 1.5f, lw = c.F(p.LineWidth) * 2, ox = Cyc(m.Now * c.F(p.ScrollX), cs), oy = Cyc(m.Now * c.F(p.ScrollY), cs);
            if (p.FillBg) m.Box(0, 0, -30, m.W * 1.1f, m.H * 1.1f, 20, bg, .01f);
            int nx = (int)(m.W / cs) + 3, ny = (int)(m.H / cs) + 3; float sh = p.Shimmer ? .7f + .3f * MathF.Sin(m.Now * 2) : 1;
            Low(m, () =>
            {
                for (int y = 0; y < ny; y++) for (int x = 0; x < nx; x++)
                {
                    float px = -m.W / 2 + (x - 1) * cs + ox, py = -m.H / 2 + (y - 1) * cs + oy; var col = m.C(lc, sh);
                    switch (p.Pattern)
                    {
                        case WagaraPattern.Seigaiha: { float sx = px + (y % 2) * cs / 2; for (int k = 1; k <= 3; k++) m.Tube(ArcPts3(sx, py, cs * .55f * k / 3, MathF.PI, MathF.Tau, -16 + k * 2, 12), lw * .5f, col); } break;
                        case WagaraPattern.Shippo: m.Torus(px, py, -16, cs * .5f, lw * .5f, col); break;
                        case WagaraPattern.Ichimatsu: if ((x + y) % 2 == 0) m.Box(px, py, -16, cs, cs, 10, col, .02f); break;
                        case WagaraPattern.Yagasuri: { float s = (x % 2 == 0) ? 1 : -1; m.Slab(-18, 8, col, px - cs * .3f, py - cs * .5f + s * cs * .15f, px + cs * .3f, py - cs * .5f - s * cs * .15f, px + cs * .3f, py + s * 0 - s * cs * .15f, px - cs * .3f, py + s * cs * .15f); } break;
                        case WagaraPattern.Kikko: { var hx = new List<Vector3>(); for (int k = 0; k <= 6; k++) { float a = k * MathF.PI / 3; hx.Add(V3(px + (y % 2) * cs / 2 + MathF.Cos(a) * cs * .5f, py + MathF.Sin(a) * cs * .5f, -16)); } m.Tube(hx, lw * .5f, col); } break;
                        default: for (int k = 0; k < 6; k++) { float a = k * MathF.PI / 3; m.Limb(px, py, -16, px + MathF.Cos(a) * cs * .5f, py + MathF.Sin(a) * cs * .5f, -16, lw * .4f, lw * .4f, col); } break;
                    }
                }
            }, 6);
        });
        S<HorrorParameter>(d, (p, c, m) => // ホラー: 暗い闇、尾を引いてただよう人魂、上から垂れる血のしずく、赤く脈打つにじみ
        {
            float fl = 1 - c.F(p.Flicker) / 100 * (R(p.Seed, (int)(m.Now * 12)) > .8f ? .6f : 0);
            if (p.Darkness) m.Box(0, 0, -800, m.W * 2.5f, m.H * 2.5f, 10, m.C(0x000000u, c.F(p.DarkAmount) / 100 * fl), .01f);
            if (p.RedBleed) { float hb = MathF.Pow(MathF.Max(0, MathF.Sin(m.Now * c.F(p.HeartRate) / 60 * MathF.PI)), 6); foreach (int a in Sides) m.Ball(a * m.W / 2, 0, -300, m.W * .25f, m.H * .7f, 60, m.C(p.BleedColor, .25f + .3f * hb)); }
            if (p.Wisps) for (int i = 0; i < Math.Min((int)c.F(p.WispCount), 10); i++) { float x = (R(p.Seed, i) - .5f) * m.W * .8f + MathF.Sin(m.Now * .7f + i) * 120, y = (R(p.Seed, i, 1) - .5f) * m.H * .6f + MathF.Cos(m.Now * .9f + i) * 60, z = -400 + R(p.Seed, i, 2) * 400, s = c.F(p.WispSize) * 1.4f; m.Ball(x, y, z, s, m.C(p.WispColor, .8f)); m.Limb(x, y, z, x - MathF.Sin(m.Now + i) * s, y + s * 3, z - s, s * .8f, 2, m.C(p.WispColor, .45f)); m.Ball(x, y, z, s * 2, m.C(p.WispColor, .12f)); }
            if (p.Blood) { m.Box(0, -m.H / 2 + 20, 20, m.W * 1.1f, 40, 20, m.C(p.BloodColor), .3f); for (int i = 0; i < Math.Min((int)c.F(p.DripCount), 30); i++) { float x = (R(p.Seed, i, 3) - .5f) * m.W, L = c.F(p.DripLength) * 4 * (.4f + R(p.Seed, i, 4)) * Math.Clamp(m.Now / 3 + R(p.Seed, i, 5) * .3f, 0, 1); m.Limb(x, -m.H / 2 + 30, 20, x, -m.H / 2 + 30 + L, 20, 12, 8, m.C(p.BloodColor)); m.Ball(x, -m.H / 2 + 34 + L, 20, 12, m.C(p.BloodColor)); float dy = Cyc(m.Now * 500 + R(p.Seed, i, 6) * m.H, m.H * 2); if (dy < m.H) m.Ball(x, -m.H / 2 + 34 + L + dy, 20, 8, 12, 8, m.C(p.BloodColor)); } }
        });
        S<HalloweenParameter>(d, (p, c, m) => // ハロウィン: 顔のくりぬかれたかぼちゃ、ふわふわ浮く幽霊、羽ばたくコウモリ、大きな月
        {
            float sc = c.F(p.ItemScale) / 100, sp = c.F(p.Speed) / 100;
            if (p.Sky) SkyBands(m, m.C(0x0D0620u), m.C(0x3A1650u), -1800, 8);
            if (p.Moon) { m.Ball(m.W * .3f, -m.H * .25f, -900, 200 * sc, m.C(0xFFE9A8u)); m.Ball(m.W * .3f, -m.H * .25f, -920, 320 * sc, m.C(0xFFE9A8u, .15f)); }
            for (int i = 0; i < Math.Min((int)c.F(p.PumpkinCount), 6); i++)
            {
                float x = (i - (c.F(p.PumpkinCount) - 1) / 2) * 360 * sc, y = m.H / 2 - 130 * sc, z = -100, r = 130 * sc;
                for (int k = 0; k < 6; k++) { float a = k * MathF.PI / 6; m.Ball(x + MathF.Cos(a) * r * .35f, y, z + MathF.Sin(a) * r * .35f, r * .55f, r * .8f, r * .55f, m.C(p.PumpkinColor)); }
                m.Limb(x, y - r * .75f, z, x + r * .1f, y - r * 1.05f, z, r * .1f, r * .07f, m.C(0x2E7D32u));
                var face = m.C(0x2A1000u); m.Slab(z + r * .62f, 20, face, x - r * .45f, y - r * .15f, x - r * .2f, y - r * .15f, x - r * .32f, y - r * .4f); m.Slab(z + r * .62f, 20, face, x + r * .2f, y - r * .15f, x + r * .45f, y - r * .15f, x + r * .32f, y - r * .4f); m.Slab(z + r * .6f, 20, face, x - r * .5f, y + r * .15f, x + r * .5f, y + r * .15f, x + r * .3f, y + r * .4f, x - r * .3f, y + r * .4f);
                m.Ball(x, y, z, r * 1.4f, m.C(0xFFA000u, .08f + .05f * MathF.Sin(m.Now * 8 + i)));
            }
            for (int i = 0; i < Math.Min((int)c.F(p.GhostCount), 6); i++) { float x = (R(p.Seed, i) - .5f) * m.W * .8f + MathF.Sin(m.Now * .6f * sp + i) * 150, y = -m.H * .15f + MathF.Sin(m.Now * 1.4f * sp + i) * 50, z = -300 + R(p.Seed, i, 1) * 300, s = 90 * sc; var gc = m.C(p.GhostColor, .9f); m.Lathe(x, z, gc, 0, y - s, s * .7f, y - s * .6f, s * .8f, y + s * .3f, s * .95f, y + s); m.Ball(x - s * .25f, y - s * .45f, z + s * .6f, s * .12f, K); m.Ball(x + s * .25f, y - s * .45f, z + s * .6f, s * .12f, K); m.Ball(x, y - s * .15f, z + s * .7f, s * .1f, s * .15f, s * .08f, K); }
            for (int i = 0; i < Math.Min((int)c.F(p.BatCount), 30); i++) { float span = m.W * 1.6f, x = -span / 2 + Cyc(m.Now * 260 * sp * (.6f + .4f * R(p.Seed, i, 3)) + R(p.Seed, i, 2) * span, span), y = -m.H * .35f + (R(p.Seed, i, 4) - .5f) * 300 + MathF.Sin(m.Now * 3 + i) * 40, z = -700 + R(p.Seed, i, 5) * 700, s = 40 * sc, f = MathF.Sin(m.Now * 14 + i); m.Ball(x, y, z, s * .3f, s * .35f, s * .3f, m.C(p.BatColor)); foreach (int sd in Sides) m.Pitch(x, y, z, 0, () => m.Slab(z - 2, 4, m.C(p.BatColor), x + sd * s * .2f, y - s * .1f, x + sd * s * 1.3f, y - s * .5f * f - s * .2f, x + sd * s * 1.1f, y + s * .2f, x + sd * s * .7f, y + s * .05f, x + sd * s * .4f, y + s * .25f)); }
        });
        S<MoonPhaseParameter>(d, (p, c, m) => // 月の満ち欠け: 光の当たる向きが回って欠けていく月の球/太陽に月が重なる日食/赤い月食
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), k = p.Auto ? Frac(m.Now / MathF.Max(.5f, c.F(p.Cycle))) : c.F(p.Progress) / 100;
            if (p.Stars) Low(m, () => { for (int i = 0; i < 80; i++) m.Ball((R(p.Seed, i) - .5f) * m.W * 1.8f, (R(p.Seed, i, 1) - .5f) * m.H * 1.8f, -1000, 3, W); }, 5);
            if (p.Mode == CelestialMode.SolarEclipse)
            {
                m.Ball(o.X, o.Y, -300, r, m.C(p.SunColor)); m.Ball(o.X, o.Y, -320, r * 1.5f, m.C(p.SunColor, .3f * c.F(p.Glow) / 100)); m.Ball(o.X, o.Y, -340, r * 2.2f, m.C(p.SunColor, .1f * c.F(p.Glow) / 100));
                m.Ball(o.X + (k - .5f) * r * 4, o.Y, 0, r * 1.02f, m.C(0x0A0A0Cu)); return;
            }
            var mc = p.Mode == CelestialMode.LunarEclipse ? Sculpt.Mix(m.C(p.MoonColor), m.C(p.BloodColor), MathF.Sin(k * MathF.PI)) : m.C(p.MoonColor);
            m.Ball(o.X, o.Y, 0, r, mc);
            if (p.Craters) for (int i = 0; i < 6; i++) { float a = R(p.Seed, i) * MathF.Tau, d2 = R(p.Seed, i, 1) * r * .6f; float cx = MathF.Cos(a) * d2, cy = MathF.Sin(a) * d2; m.Ball(o.X + cx, o.Y + cy, MathF.Sqrt(MathF.Max(0, r * r - cx * cx - cy * cy)) * .97f, r * (.08f + .08f * R(p.Seed, i, 2)), r * (.08f + .08f * R(p.Seed, i, 2)), r * .04f, m.C(Sculpt.Dark(mc, .82f), 1)); }
            if (p.Mode == CelestialMode.Phases) { float a = k * MathF.Tau; m.Yaw(o.X, o.Y, 0, a, () => m.Lathe(o.X, 0, Enumerable.Range(0, 17).Select(j => new Vector2(MathF.Max(.5f, MathF.Sin(j / 16f * MathF.PI) * r * 1.01f), o.Y - MathF.Cos(j / 16f * MathF.PI) * r * 1.01f)).ToList(), m.C(0x0A0A12u, c.F(p.ShadowAmount) / 100), MathF.PI * .5f, MathF.PI * 1.5f)); }
            else m.Ball(o.X + (k - .5f) * r * 3, o.Y, r * .2f, r * 1.1f, m.C(0x000000u, .35f * MathF.Sin(k * MathF.PI)));
            m.Ball(o.X, o.Y, -20, r * 1.3f, m.C(mc, .12f * c.F(p.Glow) / 100));
        });
        S<StepProgressParameter>(d, (p, c, m) => // ステップ表示: 太い線でつながった丸い段の台、済みはチェック、今の段は大きく脈打つ
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); int n = Math.Clamp((int)c.F(p.Steps), 2, 10); float cur = p.Auto ? MathF.Min(n - 1, m.Now / MathF.Max(.2f, c.F(p.StepTime))) : c.F(p.Current) - 1, L = c.F(p.TotalWidth), ns = c.F(p.NodeSize), lw = c.F(p.LineWidth);
            Vector2 At(int i) { float f = n == 1 ? 0 : i / (float)(n - 1) - .5f; return p.Vertical ? new Vector2(o.X, o.Y + f * L) : new Vector2(o.X + f * L, o.Y); }
            for (int i = 0; i < n - 1; i++) { var a = At(i); var b = At(i + 1); m.Limb(a.X, a.Y, 0, b.X, b.Y, 0, lw, lw, m.C(p.IdleColor)); float f = Math.Clamp(cur - i, 0, 1); if (f > 0) m.Limb(a.X, a.Y, 4, a.X + (b.X - a.X) * f, a.Y + (b.Y - a.Y) * f, 4, lw * 1.1f, lw * 1.1f, m.C(p.DoneColor)); }
            for (int i = 0; i < n; i++)
            {
                var q = At(i); bool done = i < (int)cur, act = i == (int)MathF.Round(cur) && !done; float s = ns * (act ? 1.3f + .1f * MathF.Sin(m.Now * 6) : 1);
                m.Disc(q.X, q.Y, 0, s, s, s * .6f, done ? m.C(p.DoneColor) : act ? m.C(p.ActiveColor) : m.C(p.IdleColor), .5f);
                if (done && p.Check) m.Tube(new List<Vector3> { V3(q.X - s * .4f, q.Y, s * .35f), V3(q.X - s * .1f, q.Y + s * .35f, s * .35f), V3(q.X + s * .45f, q.Y - s * .35f, s * .35f) }, s * .1f, m.C(p.TextColor));
                else m.Text((i + 1).ToString(), s, q.X, q.Y, s * .3f, 3, m.C(p.TextColor));
                if (p.Labels) m.Text($"STEP {i + 1}", ns * .7f, q.X + (p.Vertical ? ns * 3 : 0), q.Y + (p.Vertical ? 0 : ns * 2.2f), 0, 2, m.C(p.TextColor, .8f));
            }
        });
        S<StickyNoteParameter>(d, (p, c, m) => // 付箋: 少しそった紙と罫線、斜めに貼ったマスキングテープ、刺さった画びょう
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.NoteWidth), h = c.F(p.NoteHeight), k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.IntroTime)), 0, 1));
            bool paper = p.Kind != NoteKind.TapeOnly && p.Kind != NoteKind.PinOnly, tape = p.Kind is NoteKind.All or NoteKind.PaperTape or NoteKind.TapeOnly, pin = p.Kind is NoteKind.All or NoteKind.PaperPin or NoteKind.PinOnly;
            m.Move(0, 0, 300 * (1 - k), () => m.Turn(o.X, o.Y, c.F(p.Rotation) * MathF.PI / 180, () =>
            {
                if (paper)
                {
                    m.Box(o.X + 14, o.Y + 14, -14, w, h, 4, m.C(0x000000u, c.F(p.Shadow) / 100 * .5f), .02f);
                    for (int i = 0; i < 8; i++) { float y0 = o.Y - h / 2 + h * i / 8, y1 = y0 + h / 8; m.Quad3D(V3(o.X - w / 2, y0, MathF.Pow(i / 8f, 3) * 30), V3(o.X + w / 2, y0, MathF.Pow(i / 8f, 3) * 30), V3(o.X + w / 2, y1, MathF.Pow((i + 1) / 8f, 3) * 30), V3(o.X - w / 2, y1, MathF.Pow((i + 1) / 8f, 3) * 30), m.C(p.PaperColor)); }
                    if (p.Ruled) for (int i = 1; i < 6; i++) { float y = o.Y - h / 2 + h * i / 6; m.Box(o.X, y, MathF.Pow(i / 6f, 3) * 30 + 1, w * .85f, 2, 1, m.C(0x90A4AEu, .6f)); }
                    if (p.Torn) for (int i = 0; i < 12; i++) m.Slab(-1, 2, m.C(p.PaperColor), o.X - w / 2 + w * i / 12, o.Y - h / 2, o.X - w / 2 + w * (i + .5f) / 12, o.Y - h / 2 - 10, o.X - w / 2 + w * (i + 1) / 12, o.Y - h / 2);
                }
                if (tape) m.Turn(o.X, o.Y - h / 2, -.12f, () => { m.Box(o.X, o.Y - h / 2, 4, w * .4f, 60, 4, m.C(p.TapeColor, .85f), .02f); if (p.TapeStripes) for (int i = 0; i < 6; i++) m.Turn(o.X - w * .15f + i * w * .06f, o.Y - h / 2, .6f, () => m.Box(o.X - w * .15f + i * w * .06f, o.Y - h / 2, 7, 8, 70, 1, m.C(0xFFFFFFu, .5f))); });
                if (pin) { float ps = c.F(p.PinSize) * 1.5f; var q = new Vector2(o.X + w * .35f, o.Y - h * .35f); m.Disc(q.X, q.Y, ps * .8f, ps, ps, ps * .7f, m.C(p.PinColor), .5f); m.Lathe(q.X, 0, m.C(p.PinColor), ps * .5f, q.Y, ps * .3f, q.Y); m.Ball(q.X - ps * .3f, q.Y - ps * .3f, ps * 1.2f, ps * .2f, m.C(0xFFFFFFu, .6f)); }
            }));
        });
        S<DividerParameter>(d, (p, c, m) => // 飾り罫: のびる金色の線と、まん中の花・ひし形の飾り、端の玉
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float L = c.F(p.LineLength) * Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.IntroTime)), 0, 1)), th = c.F(p.Thickness) * 1.6f; var col = m.C(p.Color); var ac = m.C(p.AccentColor);
            if (L < 2) return;
            switch (p.Style)
            {
                case DividerStyle.Wave: { var pts = new List<Vector3>(); for (int i = 0; i <= 40; i++) { float x = o.X - L / 2 + L * i / 40; pts.Add(V3(x, o.Y + MathF.Sin(i * .9f) * 12, 0)); } m.Tube(pts, th, col); } break;
                case DividerStyle.Double: m.Limb(o.X - L / 2, o.Y - 8, 0, o.X + L / 2, o.Y - 8, 0, th, th, col); m.Limb(o.X - L / 2, o.Y + 8, 0, o.X + L / 2, o.Y + 8, 0, th * .6f, th * .6f, col); break;
                case DividerStyle.Dots: for (float x = -L / 2; x <= L / 2; x += 30) m.Ball(o.X + x, o.Y, 0, th * 1.4f, col); break;
                case DividerStyle.Underline: m.Box(o.X, o.Y, 0, L, th * 2, th * 2, col, .4f); break;
                default:
                    foreach (int s in Sides) { m.Limb(o.X + s * 50, o.Y, 0, o.X + s * L / 2, o.Y, 0, th, th * .4f, col); m.Ball(o.X + s * L / 2, o.Y, 0, th * 1.5f, col); m.Tube(ArcPts3(o.X + s * 90, o.Y - 16, 22, s > 0 ? MathF.PI : 0, s > 0 ? MathF.PI * 2 : MathF.PI, 0, 12), th * .6f, col); }
                    if (p.Style == DividerStyle.Floral) { for (int i = 0; i < 5; i++) { float a = i * MathF.Tau / 5 - MathF.PI / 2; m.Ball(o.X + MathF.Cos(a) * 18, o.Y + MathF.Sin(a) * 18, 4, 16, 16, 6, ac); } m.Ball(o.X, o.Y, 8, 9, col); }
                    else m.Turn(o.X, o.Y, MathF.PI / 4, () => m.Box(o.X, o.Y, 0, 34, 34, 14, ac, .2f));
                    break;
            }
            if (p.Glow) m.Limb(o.X - L / 2, o.Y, -10, o.X + L / 2, o.Y, -10, th * 4, th * 4, m.C(p.Color, .15f));
        });
        S<ScreenFrameParameter>(d, (p, c, m) => // 画面フレーム: ブラウン管テレビの箱・ポラロイドの白い枠・フィルムの帯・携帯ゲーム機の本体
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float ws = c.F(p.WindowScale) / 100, w = m.W * ws, h = m.H * ws;
            uint auto = p.Kind switch { FrameKind.Polaroid => 0xF4F1EAu, FrameKind.Film => 0x161616u, FrameKind.Handheld => 0x9EA3A8u, _ => 0x3A342Eu }; var bc = p.AutoColor ? m.C(auto) : m.C(p.BodyColor);
            void Around(float bw, float bh, float dx, float dy, float d) { float l = o.X - w / 2, r = o.X + w / 2, t = o.Y - h / 2, b = o.Y + h / 2; m.Box((l + (o.X - bw / 2 + dx)) / 2, o.Y + dy, 0, l - (o.X - bw / 2 + dx), bh, d, bc, .05f); m.Box((r + (o.X + bw / 2 + dx)) / 2, o.Y + dy, 0, (o.X + bw / 2 + dx) - r, bh, d, bc, .05f); m.Box(o.X + dx, (t + (o.Y - bh / 2 + dy)) / 2, 0, bw, t - (o.Y - bh / 2 + dy), d, bc, .05f); m.Box(o.X + dx, (b + (o.Y + bh / 2 + dy)) / 2, 0, bw, (o.Y + bh / 2 + dy) - b, d, bc, .05f); }
            switch (p.Kind)
            {
                case FrameKind.Polaroid: Around(w + 80, h + 260, 0, 70, 20); break;
                case FrameKind.Film: Around(m.W * 1.2f, h + 200, 0, 0, 16); foreach (int s in Sides) for (int i = 0; i < 16; i++) m.Box(-m.W * .55f + i * m.W * 1.1f / 15 - Cyc(m.Now * c.F(p.FilmSpeed) * 4, m.W * 1.1f / 15), o.Y + s * (h / 2 + 50), 10, 40, 30, 4, m.C(0xEEEEEEu), .3f); break;
                case FrameKind.Handheld: Around(w + 160, h + 700, 0, 260, 120); m.Box(o.X - w * .25f, o.Y + h / 2 + 260, 70, 140, 44, 30, K, .2f); m.Box(o.X - w * .25f, o.Y + h / 2 + 260, 70, 44, 140, 30, K, .2f); foreach (int s in new[] { 0, 1 }) m.Disc(o.X + w * .2f + s * 110, o.Y + h / 2 + 300 - s * 70, 70, 42, 42, 30, m.C(0xC2185Bu), .5f); break;
                default: Around(w + 300, h + 260, 100, 0, 80); m.Box(o.X + 100, o.Y, -260, w + 200, h + 160, 400, m.C(Sculpt.Dark(bc, .8f), 1), .1f); for (int i = 0; i < 2; i++) m.Disc(o.X + w / 2 + 170, o.Y - 80 + i * 140, 50, 40, 40, 30, m.C(0x9E9E9Eu), .5f); if (c.F(p.Scanlines) > 0) for (int i = 0; i < 40; i++) m.Box(o.X, o.Y - h / 2 + i * h / 40, 2, w, 2, 2, m.C(0x000000u, c.F(p.Scanlines) / 100 * .4f)); break;
            }
        });
        S<WarningParameter>(d, (p, c, m) => // 警告: 黄と黒の斜めしまの帯が流れ、点滅する赤い画面と三角の警告マーク
        {
            float cy = (c.F(p.CenterY) / 100 - .5f) * m.H, bh = c.F(p.BandHeight) * 1.5f, sw = c.F(p.StripeWidth) * 1.5f, sl = MathF.Tan(c.F(p.Slant) * MathF.PI / 180), off = Cyc(m.Now * c.F(p.Scroll) * 2, sw * 2), blink = .5f + .5f * MathF.Sin(m.Now * c.F(p.BlinkRate) * MathF.Tau);
            void Band(float y) { m.Box(0, y, 0, m.W * 1.2f, bh, 20, m.C(p.Color1), .05f); for (float x = -m.W * .7f + off; x < m.W * .7f; x += sw * 2) m.Slab(10, 6, m.C(p.Color2), x - bh / 2 * sl, y - bh / 2, x + sw - bh / 2 * sl, y - bh / 2, x + sw + bh / 2 * sl, y + bh / 2, x + bh / 2 * sl, y + bh / 2); }
            if (p.Layout != WarnLayout.Center) { Band(-m.H / 2 + bh / 2); Band(m.H / 2 - bh / 2); }
            if (p.Layout != WarnLayout.TopBottom) Band(cy + (p.Icon || p.Text ? 160 : 0));
            if (p.RedFlash) m.Box(0, 0, -200, m.W * 1.5f, m.H * 1.5f, 4, m.C(p.RedColor, .25f * blink), .01f);
            if (p.Icon) { m.Slab(-20, 40, m.C(p.Color1, .5f + .5f * blink), 0, cy - 130, 120, cy + 80, -120, cy + 80); m.Box(0, cy - 20, 22, 20, 90, 10, m.C(p.Color2), .4f); m.Ball(0, cy + 50, 22, 12, m.C(p.Color2)); }
            if (p.Text) m.Text("WARNING", 90, 0, cy + 160, 30, 8, m.C(p.Color2));
        });
        S<RankingBadgeParameter>(d, (p, c, m) => // 順位バッジ: 金・銀・銅の厚いメダルとリボン、または3段の表彰台
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float ms = c.F(p.MedalSize);
            void Medal(float x, float y, int rank, float delay)
            {
                float k = Ease(Math.Clamp((m.Now - delay) / MathF.Max(.05f, c.F(p.IntroTime)), 0, 1)); if (k <= 0) return; uint col = rank == 1 ? 0xF2C22Eu : rank == 2 ? 0xC9CED6u : 0xC8804Au;
                m.Move(0, -200 * (1 - k), () => m.Yaw(x, y, 0, (1 - k) * MathF.PI * 2 + (p.Shine ? MathF.Sin(m.Now * 1.5f) * .3f : 0), () =>
                {
                    if (p.Ribbon) foreach (int s in Sides) m.Slab(-20, 10, m.C(p.RibbonColor), x + s * ms * .1f, y - ms * .3f, x + s * ms * .55f, y + ms * 1.1f, x + s * ms * .4f, y + ms * .95f, x + s * ms * .3f, y + ms * 1.15f, x + s * ms * -.05f, y - ms * .2f);
                    m.Disc(x, y, 0, ms * .6f, ms * .6f, ms * .2f, m.C(col), .5f); m.Torus(x, y, ms * .1f, ms * .5f, ms * .04f, m.C(Sculpt.Dark(m.C(col), .8f), 1)); m.Text(rank.ToString(), ms * .6f, x, y, ms * .11f, ms * .08f, m.C(Sculpt.Dark(m.C(col), .55f), 1));
                }));
                if (p.Sparkle) for (int i = 0; i < 4; i++) { float a = m.Now * 2 + i * MathF.PI / 2; m.Slab(30, 6, m.C(0xFFFFFFu, MathF.Abs(MathF.Sin(m.Now * 3 + i))), StarPts(x + MathF.Cos(a) * ms * .8f, y + MathF.Sin(a) * ms * .8f, 16, 4, .3f, 0)); }
            }
            if (p.Kind == RankKind.Podium)
            {
                float st = c.F(p.Stagger); int[] order = { 2, 1, 3 }; for (int i = 0; i < 3; i++) { int rk = order[i]; float x = o.X + (i - 1) * ms * 1.6f, hh = ms * (rk == 1 ? 1.4f : rk == 2 ? 1f : .7f), base0 = o.Y + ms * 1.8f; m.Box(x, base0 - hh / 2, 0, ms * 1.5f, hh, ms, m.C(0xECEFF1u), .05f); m.Text(rk.ToString(), ms * .5f, x, base0 - hh / 2, ms * .5f, 4, m.C(0x455A64u)); Medal(x, base0 - hh - ms * .8f, rk, (3 - rk) * st); }
            }
            else Medal(o.X, o.Y, (int)p.Kind + 1, 0);
        });
        S<GameUiParameter>(d, (p, c, m) => // ゲームUI: 左上のハートのライフ、右上のスコアの数字板、跳ねるコンボの数
        {
            float ts = c.F(p.TextSize); var tc = m.C(p.TextColor); var ac = m.C(p.AccentColor);
            if (p.ShowLife) { var o = PctPos(m, c.F(p.LifeX), c.F(p.LifeY)); int mx = Math.Clamp((int)c.F(p.MaxLife), 1, 10), lf = (int)c.F(p.Life); float hs = c.F(p.HeartSize) * 1.3f; for (int i = 0; i < mx; i++) Heart3D(m, o.X + hs + i * hs * 2.2f, o.Y + hs * .2f, 0, hs, i < lf ? m.C(p.HeartColor) : m.C(0x455A64u)); }
            if (p.ShowScore) { var o = PctPos(m, c.F(p.ScoreX), c.F(p.ScoreY)); int sc = (int)(c.F(p.Score) + (p.AutoCount ? m.Now * 120 : 0)); string s = sc.ToString().PadLeft(Math.Clamp((int)c.F(p.ScoreDigits), 1, 12), '0'); float w = s.Length * ts * .7f + 40; m.Box(o.X - w / 2, o.Y + ts * .2f, -16, w, ts * 1.4f, 20, m.C(0x000000u, .5f), .3f); m.Text(s, ts * 1.3f, o.X - w / 2, o.Y + ts * .2f, 0, ts * .2f, tc); }
            if (p.ShowCombo) { var o = PctPos(m, c.F(p.ComboX), c.F(p.ComboY)); int cb = (int)(c.F(p.Combo) + (p.AutoCount ? m.Now * 2 : 0)); float pop = 1 + .25f * MathF.Max(0, 1 - Frac(m.Now * .5f) * 5); m.Scale(o.X, o.Y, 0, pop, () => { m.Text(cb.ToString(), ts * 2.4f, o.X, o.Y, 0, ts * .4f, ac, 0, K, 6); m.Text("COMBO", ts * .9f, o.X, o.Y + ts * 1.5f, 0, ts * .15f, tc); }); }
        });
        S<ShineParameter>(d, (p, c, m) => // シャイン: 範囲の板の上を、斜めの光の帯が奥から手前へ走り抜ける
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.AreaWidth), h = c.F(p.AreaHeight), bw = c.F(p.BandWidth), dur = MathF.Max(.05f, c.F(p.Duration)), iv = MathF.Max(dur, c.F(p.Interval)), t = m.Now - c.F(p.Delay);
            if (t < 0 || (!p.Loop && t > dur)) return; float f = Math.Clamp(Frac(t / iv) * iv / dur, 0, 1); if (f >= 1) return; if (p.Reverse) f = 1 - f;
            var col = m.C(p.Color, c.F(p.Intensity) / 100); float x = o.X - w / 2 - bw + (w + bw * 2) * f, sk = c.F(p.Angle) * MathF.PI / 180;
            m.Turn(x, o.Y, sk, () => { m.Box(x, o.Y, 30, bw, h * 1.3f, 60, m.C(col, .5f), .3f); if (p.TwinBand) m.Box(x - bw * 1.2f, o.Y, 30, bw * .35f, h * 1.3f, 40, m.C(col, .35f), .3f); });
            if (p.Sparkle) m.Slab(50, 8, col, StarPts(x + bw * .3f, o.Y - h * .4f, 30 * MathF.Sin(f * MathF.PI), 4, .25f, 0));
        });
        S<LevelUpParameter>(d, (p, c, m) => // LEVEL UP: 立ちのぼる光の柱の中から、跳ねて出る厚い金色の文字と上向きの矢印
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.SizePct) / 100, k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.RiseTime)), 0, 1)), pop = 1 + .2f * MathF.Max(0, 1 - MathF.Abs(m.Now - c.F(p.RiseTime)) * 5);
            m.Lathe(o.X, 0, m.C(p.Color, .25f), 200 * s, o.Y + 40, 160 * s, o.Y - 700 * s * k);
            for (int i = 0; i < 12; i++) { float f = Frac(m.Now * .7f + i / 12f), a = i * 2.4f; m.Ball(o.X + MathF.Cos(a) * 160 * s, o.Y - f * 600 * s, MathF.Sin(a) * 160 * s, 8, m.C(p.Color, 1 - f)); }
            m.Scale(o.X, o.Y - 300 * s * k, 0, s * pop, () =>
            {
                if (p.ShowText) m.Text("LEVEL UP!", 130, o.X, o.Y - 300 * s * k, 30, 26, m.C(p.TextColor), 0, m.C(p.Color), 8);
                if (p.ShowLevel) m.Text($"Lv {(int)c.F(p.Level) - 1} → {(int)c.F(p.Level)}", 60, o.X, o.Y - 300 * s * k + 120, 30, 10, m.C(p.Color));
                m.Slab(10, 40, m.C(p.Color), o.X - 380, o.Y - 300 * s * k - 140, o.X - 320, o.Y - 300 * s * k - 60, o.X - 345, o.Y - 300 * s * k - 60, o.X - 345, o.Y - 300 * s * k + 20, o.X - 415, o.Y - 300 * s * k + 20, o.X - 415, o.Y - 300 * s * k - 60, o.X - 440, o.Y - 300 * s * k - 60);
            });
        });
        S<TreasureChestParameter>(d, (p, c, m) => // 宝箱: 金具の帯のついた木の箱、ふたがパカッと開いて光と金貨が飛び出す
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.BoxWidth), h = w * .5f, dp = w * .6f, k = Ease(Math.Clamp((m.Now - c.F(p.OpenTime)) / .4f, 0, 1)), shake = m.Now < c.F(p.OpenTime) ? MathF.Sin(m.Now * 40) * 4 * m.Now / MathF.Max(.1f, c.F(p.OpenTime)) : 0;
            var bc = m.C(p.BoxColor); var mc = m.C(p.MetalColor);
            m.Move(shake, 0, () =>
            {
                m.Box(o.X, o.Y - h / 2, 0, w, h, dp, bc, .08f); foreach (int s in Sides) m.Box(o.X + s * w * .38f, o.Y - h / 2, 0, w * .08f, h + 4, dp + 4, mc, .1f);
                m.Box(o.X, o.Y - h * .6f, dp / 2 + 4, w * .12f, h * .3f, 10, mc, .2f);
                m.Pitch(o.X, o.Y - h, -dp / 2, -k * 1.9f, () => { m.Box(o.X, o.Y - h - dp * .15f, 0, w, dp * .3f, dp, bc, .3f); foreach (int s in Sides) m.Turn(o.X + s * w * .38f, o.Y - h, 0, () => m.Box(o.X + s * w * .38f, o.Y - h - dp * .25f, 0, w * .08f, dp * .5f, dp + 6, mc, .3f)); });
            });
            if (k > 0)
            {
                m.Lathe(o.X, 0, m.C(p.LightColor, .35f * k), w * .4f, o.Y - h, w * .7f, o.Y - h - 700 * k);
                for (int i = 0; i < Math.Min((int)c.F(p.Coins), 40); i++) { float t = (m.Now - c.F(p.OpenTime)) - i * .03f; if (t < 0) continue; float a = R(p.Seed, i) * MathF.Tau, v = 300 + 300 * R(p.Seed, i, 1), x = o.X + MathF.Cos(a) * 200 * t, y = o.Y - h - (900 * t - 1400 * t * t * .5f), z = MathF.Sin(a) * 200 * t; if (y > o.Y + 40) continue; Coin(m, x, y, z, 22, t * 12 + i, mc); }
            }
        });
        S<GachaParameter>(d, (p, c, m) => // ガチャ: 回る光の玉がためて割れ、レア度の色の光と星が飛び出す
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.OrbSize) * 1.5f, bt = c.F(p.BurstTime); int rar = (int)p.Rarity; uint[] cols = { 0xB0BEC5, 0x4FC3F7, 0xBA68C8, 0xFFD54F }; var col = m.C(cols[rar]);
            if (m.Now < bt)
            {
                float k = m.Now / bt; var oc = p.Hide ? m.C(0xFFFFFFu) : col;
                m.Yaw(o.X, o.Y, 0, m.Now * (2 + k * 12), () => { m.Ball(o.X, o.Y, 0, r, oc); m.Torus(o.X, o.Y, 0, r * 1.02f, r * .05f, m.C(0x37474Fu)); });
                m.Ball(o.X, o.Y, 0, r * (1.3f + k * .5f), m.C(oc, .15f + .2f * k)); return;
            }
            float f = Math.Clamp((m.Now - bt) / 1.2f, 0, 1);
            foreach (int s in Sides) m.Move(s * 200 * f, -100 * f + 300 * f * f, 0, () => m.Turn(o.X, o.Y, s * f * 2, () => m.Lathe(o.X, 0, col, Pts(0.01f, o.Y + s * r, r, o.Y), s > 0 ? 0 : 0, MathF.Tau)));
            m.Ball(o.X, o.Y, 0, r * 2 * Ease(f) + 1, m.C(col, .4f * (1 - f) + .1f));
            for (int i = 0; i < 6 + rar * 4; i++) { float a = i * MathF.Tau / (6 + rar * 4), d2 = 700 * Ease(f); m.Slab(-8, 16, m.C(col, 1 - f * .5f), StarPts(o.X + MathF.Cos(a) * d2, o.Y + MathF.Sin(a) * d2 * .6f, 40, 5, .45f, a)); }
            for (int i = 0; i <= rar; i++) m.Slab(20, 30, m.C(0xFFD54Fu), StarPts(o.X + (i - rar / 2f) * 110, o.Y + r * 2 + 40, 50 * Ease(Math.Clamp(f * 3 - i * .3f, 0, 1)), 5, .45f, 0));
        });
        S<DizzyParameter>(d, (p, c, m) => // 気絶の星: 頭の上をくるくる回る、立体の星とひよこ
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), s = c.F(p.ItemSize) * 1.6f, sp = m.Now * c.F(p.Speed) / 30; int n = Math.Clamp((int)c.F(p.Count), 1, 12);
            m.Pitch(o.X, o.Y, 0, p.Orbit ? 0 : 0, () =>
            {
                for (int i = 0; i < n; i++)
                {
                    float a = sp + i * MathF.Tau / n, x = o.X + MathF.Cos(a) * r, z = MathF.Sin(a) * r * .8f, y = o.Y + MathF.Sin(a) * r * .25f;
                    bool chick = p.Kind == DizzyKind.Chicks || (p.Kind == DizzyKind.Both && i % 2 == 1);
                    if (chick) { m.Ball(x, y, z, s * .6f, m.C(0xFFE45Cu)); m.Ball(x + s * .15f, y - s * .55f, z, s * .4f, m.C(0xFFE45Cu)); m.Ball(x + s * .3f, y - s * .62f, z + s * .3f, s * .07f, K); m.Turn(x + s * .5f, y - s * .5f, -MathF.PI / 2, () => m.Lathe(x + s * .5f, z, m.C(0xFF9800u), s * .1f, y - s * .5f, 0, y - s * .3f)); }
                    else m.Yaw(x, y, z, m.Now * 4 + i, () => m.Slab(z - s * .15f, s * .3f, m.C(p.StarColor), StarPts(x, y, s * .8f, 5, .45f, 0)));
                }
            });
        });
        S<MoneyRainParameter>(d, (p, c, m) => // お金が降る: 回りながら落ちる金貨と、ひらひら舞うお札
        {
            float sp = c.F(p.Speed) / 100, cs = c.F(p.CoinSize) * 1.4f, bs = c.F(p.BillSize) * 1.4f;
            for (int i = 0; i < Math.Min((int)c.F(p.CoinCount), 60); i++) { float y = -m.H / 2 - 40 + Cyc(m.Now * 400 * sp * (.7f + .3f * R(p.Seed, i, 3)) + R(p.Seed, i, 1) * m.H * 1.2f, m.H * 1.2f), x = (R(p.Seed, i) - .5f) * m.W * 1.2f, z = -900 + R(p.Seed, i, 2) * 1100; Coin(m, x, y, z, cs, m.Now * 6 + i, m.C(p.CoinColor)); if (p.Sparkle && MathF.Sin(m.Now * 5 + i) > .9f) m.Slab(z + cs, 4, W, StarPts(x + cs * .5f, y - cs * .5f, cs * .6f, 4, .25f, 0)); }
            for (int i = 0; i < Math.Min((int)c.F(p.BillCount), 30); i++) { float y = -m.H / 2 - 80 + Cyc(m.Now * 160 * sp * (.7f + .3f * R(p.Seed, i, 7)) + R(p.Seed, i, 5) * m.H * 1.2f, m.H * 1.2f), x = (R(p.Seed, i, 4) - .5f) * m.W * 1.2f + MathF.Sin(m.Now * 1.5f + i) * 80, z = -900 + R(p.Seed, i, 6) * 1100, sw = MathF.Sin(m.Now * 2 + i); m.Pitch(x, y, z, sw * .8f, () => m.Turn(x, y, sw * .5f + i, () => { m.Box(x, y, z, bs, bs * .45f, 2, m.C(p.BillColor), .02f); m.Disc(x, y, z + 1.5f, bs * .14f, bs * .14f, 1, m.C(Sculpt.Dark(m.C(p.BillColor), .7f), 1)); })); }
        });
        S<PixelArtParameter>(d, (p, c, m) => // ドット絵: 小さな立方体のブロックで組んだ、ハート・回るコイン・はじける爆発
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float dt = c.F(p.Dot) * 1.5f; int fr = (int)(m.Now * c.F(p.FrameRate));
            void Grid(string[] rows, float ox, float oy, Vector4[] pal, float sx = 1)
            {
                for (int y = 0; y < rows.Length; y++) for (int x = 0; x < rows[y].Length; x++) { int ci = rows[y][x] - '0'; if (ci <= 0) continue; m.Box(ox + (x - rows[y].Length / 2f + .5f) * dt * sx, oy + (y - rows.Length / 2f + .5f) * dt, 0, dt * MathF.Max(.15f, MathF.Abs(sx)), dt, dt, pal[ci], .05f); }
            }
            var hp = new[] { W, m.C(p.HeartColor), m.C(0xFFFFFFu), K }; var cp = new[] { W, m.C(p.CoinColor), m.C(0xFFF59Du), m.C(0xA0781Eu) };
            string[] heart = { "0110110", "1211111", "1111111", "1111111", "0111110", "0011100", "0001000" };
            string[] coin = { "0011100", "0122210", "1213121", "1213121", "1213121", "0122210", "0011100" };
            bool all = p.Kind == PixelKind.All; float gap = dt * 12;
            if (all || p.Kind == PixelKind.Heart) Grid(heart, o.X - (all ? gap : 0), o.Y, hp);
            if (all || p.Kind == PixelKind.Coin) Grid(coin, o.X, o.Y, cp, MathF.Cos(fr * .5f));
            if (all || p.Kind == PixelKind.Explosion) { float iv = MathF.Max(.2f, c.F(p.BoomInterval)), f = Frac(m.Now / iv); int st = (int)(f * 4); float ex = o.X + (all ? gap : 0); for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12; for (int k = 0; k <= st; k++) m.Box(ex + MathF.Round(MathF.Cos(a) * k * 1.5f) * dt, o.Y + MathF.Round(MathF.Sin(a) * k * 1.5f) * dt, 0, dt, dt, dt, m.C(k == st ? 0xFFF176u : k == st - 1 ? 0xFF9800u : 0xE53935u), .05f); } }
        });
        S<NamePlateParameter>(d, (p, c, m) => // 話者名プレート: 横から滑り込む厚い名札の板と、差し色の帯
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.PlateWidth), h = c.F(p.PlateHeight), k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.InTime)), 0, 1)), dir = p.FromRight ? -1 : 1, x0 = o.X + dir * w / 2 - dir * (1 - k) * (w + 200);
            var pc = m.C(p.PlateColor, c.F(p.Opacity) / 100); var ac = m.C(p.AccentColor);
            switch (p.Style)
            {
                case PlateStyle.Rounded: m.Box(x0, o.Y, 0, w, h, 24, pc, .45f); m.Disc(x0 - dir * w / 2 + dir * h * .5f, o.Y, 14, h * .3f, h * .3f, 10, ac); break;
                case PlateStyle.Underline: m.Box(x0, o.Y + h / 2, 0, w, 10, 14, ac, .3f); m.Box(x0, o.Y, -6, w, h, 6, m.C(p.PlateColor, .4f), .1f); break;
                case PlateStyle.GameWindow: m.Box(x0, o.Y, 0, w, h, 20, pc, .15f); RectBars(m, x0, o.Y, 12, w - 12, h - 12, 6, m.C(0xFFFFFFu)); break;
                default: m.Slab(-12, 24, pc, x0 - w / 2 + 30, o.Y - h / 2, x0 + w / 2 + 30, o.Y - h / 2, x0 + w / 2 - 30, o.Y + h / 2, x0 - w / 2 - 30, o.Y + h / 2); m.Slab(-14, 30, ac, x0 - w / 2 - 10, o.Y - h / 2, x0 - w / 2 + 30, o.Y - h / 2, x0 - w / 2 - 10 - 30 + 20, o.Y + h / 2, x0 - w / 2 - 50, o.Y + h / 2); break;
            }
        });
        S<ChapterCardParameter>(d, (p, c, m) => // 章タイトルカード: 暗い幕の前に、大きな章番号と差し色の帯・枠
        {
            float k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.InTime)), 0, 1)); var ac = m.C(p.AccentColor); var tc = m.C(p.TextColor); int num = (int)c.F(p.Number);
            m.Box(0, 0, -100, m.W * 1.1f, m.H * 1.1f, 20, m.C(p.BgColor, c.F(p.BgOpacity) / 100), .01f);
            switch (p.Style)
            {
                case ChapterStyle.Frame: RectBars(m, 0, 0, 0, 1100 * k, 420, 14, ac); m.Text(p.ShowLabel ? $"第{num}章" : num.ToString(), 150, 0, 0, 10, 20, tc); break;
                case ChapterStyle.BigNumber: m.Text(num.ToString(), 520 * k + 1, -m.W * .22f, 0, 0, 90, ac); if (p.ShowLabel) m.Text("CHAPTER", 90, m.W * .15f, 0, 0, 14, tc); m.Box(m.W * .15f, 80, 0, 500 * k, 10, 10, ac, .3f); break;
                default: m.Box(-m.W / 2 + m.W * k / 2, 0, 0, m.W * k, 260, 40, ac, .05f); if (p.ShowLabel) m.Text($"CHAPTER {num}", 130, 0, 0, 24, 16, tc); break;
            }
        });
        S<VsScreenParameter>(d, (p, c, m) => // VS画面: 左右から迫る斜めの色の板と、まん中の稲妻・立体の「VS」
        {
            float k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.InTime)), 0, 1)), tl = c.F(p.Tilt) * 10, op = c.F(p.SideOpacity) / 100, vs = c.F(p.VsSize);
            m.Slab(-40, 40, m.C(p.LeftColor, op), -m.W / 2 - m.W * (1 - k), -m.H / 2, tl - m.W * (1 - k), -m.H / 2, -tl - m.W * (1 - k), m.H / 2, -m.W / 2 - m.W * (1 - k), m.H / 2);
            m.Slab(-40, 40, m.C(p.RightColor, op), tl + m.W * (1 - k), -m.H / 2, m.W / 2 + m.W * (1 - k), -m.H / 2, m.W / 2 + m.W * (1 - k), m.H / 2, -tl + m.W * (1 - k), m.H / 2);
            if (p.Stripes) for (int i = 0; i < 8; i++) { float y = -m.H / 2 + i * m.H / 8 + Cyc(m.Now * 300, m.H / 8); foreach (int s in Sides) m.Box(s * m.W * .3f, y, 4, m.W * .3f, 6, 4, m.C(0xFFFFFFu, .2f), .1f); }
            if (k >= 1) { m.Tube(new List<Vector3> { V3(tl, -m.H / 2, 10), V3(30, -100, 10), V3(-30, 0, 10), V3(40, 120, 10), V3(-tl, m.H / 2, 10) }, 12, m.C(p.BoltColor)); float pop = 1 + .3f * MathF.Max(0, 1 - (m.Now - c.F(p.InTime)) * 4); m.Scale(0, 0, 0, pop, () => m.Text("VS", vs * 1.6f, 0, 0, 40, vs * .35f, m.C(p.BoltColor), 0, K, 8)); }
        });
        S<TimelineParameter>(d, (p, c, m) => // 年表: 左から伸びる太い線の上に、順に立つ点の柱と年号の札
        {
            int n = Math.Clamp((int)c.F(p.Points), 2, 10); float y = (c.F(p.PosY) / 100 - .5f) * m.H, mg = c.F(p.Margin) / 100 * m.W, x0 = -m.W / 2 + mg, x1 = m.W / 2 - mg, lk = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.LineTime)), 0, 1)), lw = c.F(p.LineWidth);
            m.Limb(x0, y, 0, x0 + (x1 - x0) * lk, y, 0, lw, lw, m.C(p.LineColor)); if (lk >= 1) m.Slab(-lw, lw * 2, m.C(p.LineColor), x1, y - 20, x1 + 40, y, x1, y + 20);
            for (int i = 0; i < n; i++)
            {
                float k = Ease(Math.Clamp((m.Now - c.F(p.LineTime) - i * c.F(p.StepTime)) / .4f, 0, 1)); if (k <= 0) continue; float x = x0 + (x1 - x0) * (i + .5f) / n, up = i % 2 == 0 ? -1 : 1;
                m.Ball(x, y, 0, 22 * k, m.C(p.AccentColor)); m.Limb(x, y, 0, x, y + up * 120 * k, 0, 4, 4, m.C(p.AccentColor));
                if (p.Boxes) m.Box(x, y + up * 170 * k, 0, 220, 90, 30, m.C(0x1C2433u), .2f);
                if (p.ShowYears) m.Text(((int)(c.F(p.StartYear) + i * c.F(p.YearStep))).ToString(), 50, x, y + up * 170 * k, 18, 4, m.C(p.LineColor));
            }
        });
        S<MapRouteParameter>(d, (p, c, m) => // 地図のピン: 地図の床の上に立つしずく形のピンと、その間を順に伸びるルートの管、進む人の玉
        {
            int n = Math.Clamp((int)c.F(p.Count), 2, 8); float ps = c.F(p.PinSize) * 1.6f, lt = MathF.Max(.2f, c.F(p.LegTime)), prog = m.Now / lt;
            m.Pitch(0, 0, 0, .9f, () =>
            {
                if (p.MapBg) { m.Box(0, 0, -20, m.W * 1.1f, m.H * 1.1f, 20, m.C(0xE8F0E0u), .01f); for (int i = 0; i < 6; i++) m.Ball((R(p.Seed, i, 9) - .5f) * m.W, (R(p.Seed, i, 8) - .5f) * m.H, -10, 200, 140, 4, m.C(0xA5D6A7u)); }
                var pts = new List<Vector3>(); for (int i = 0; i < n; i++) pts.Add(V3(((i + .5f) / n - .5f) * m.W * .8f, (R(p.Seed, i) - .5f) * m.H * .6f, 0));
                for (int i = 0; i < n - 1; i++)
                {
                    float f = Math.Clamp(prog - i, 0, 1); if (f <= 0) break; var a = pts[i]; var b = pts[i + 1]; var route = new List<Vector3>();
                    for (int k = 0; k <= 16; k++) { float u = k / 16f * f; var q = Vector3.Lerp(a, b, u); if (p.Curved) q += V3(0, -MathF.Sin(u / MathF.Max(f, .001f) * MathF.PI * f) * 80, 0); route.Add(q + V3(0, 0, 4)); }
                    m.Tube(route, c.F(p.RouteWidth), m.C(p.RouteColor));
                    if (p.Traveler && f < 1) { var q = route[^1]; m.Ball(q.X, q.Y, 24, 18, m.C(0xFFFFFFu)); }
                }
                for (int i = 0; i < n; i++) { float k = Ease(Math.Clamp((prog - i + .2f) * 3, 0, 1)); if (k <= 0) continue; var q = pts[i]; m.Pitch(q.X, q.Y, 0, -MathF.PI / 2, () => { m.Lathe(q.X, 0, m.C(p.PinColor), 0, q.Y + 0, ps * .3f * k, q.Y + ps * .9f * k, ps * .55f * k, q.Y + ps * 1.6f * k, ps * .4f * k, q.Y + ps * 2.1f * k, 0, q.Y + ps * 2.3f * k); m.Ball(q.X, q.Y + ps * 1.6f * k, ps * .5f * k, ps * .2f * k, W); }); }
            });
        });
        S<ChecklistParameter>(d, (p, c, m) => // チェックリスト: 並んだ厚い行の板と四角い箱、順にチェックの印が入る
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); int n = Math.Clamp((int)c.F(p.Items), 1, 10); float rw = c.F(p.RowWidth), rh = c.F(p.RowHeight), done = p.Auto ? m.Now / MathF.Max(.2f, c.F(p.StepTime)) : c.F(p.Checked);
            for (int i = 0; i < n; i++)
            {
                float y = o.Y + i * rh * 1.2f + rh / 2, x = o.X + rw / 2, k = Math.Clamp(done - i, 0, 1);
                m.Box(x, y, -10, rw, rh, 16, m.C(p.RowColor, c.F(p.RowOpacity) / 100), .2f);
                m.Box(o.X + rh * .6f, y, 6, rh * .55f, rh * .55f, 20, m.C(p.LineColor, .9f), .15f);
                if (k > 0) { var pts = new List<Vector3> { V3(o.X + rh * .4f, y, 24), V3(o.X + rh * .55f, y + rh * .18f, 24), V3(o.X + rh * .85f, y - rh * .25f, 24) }; m.Tube(pts.Take(k < .5f ? 2 : 3).ToList(), rh * .06f, m.C(p.CheckColor)); }
                m.Text(p.Numbers ? $"{i + 1}." : "", rh * .4f, o.X + rh * 1.3f, y, 4, 2, m.C(p.LineColor));
                m.Box(o.X + rh * 1.6f + rw * .3f, y, 4, rw * .55f, rh * .12f, 6, m.C(p.LineColor, .5f), .5f);
                if (p.Strike && k >= 1) m.Box(o.X + rh * 1.6f + rw * .3f, y, 12, rw * .6f, 6, 6, m.C(p.CheckColor), .3f);
            }
        });
    }
}
