using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  文字PV背景の「平面の背景」30種を、本物の3Dの形で組み立てる(3Dのとき)
//  2Dの模様をそのまま板にするのではなく、粒・柱・輪・帯などを奥行きの中に並べる
//  単位は 1080 を高さの基準にした絵の単位(x 右、y 下、z 手前)
// =====================================================================
internal sealed class PvFlat3D
{
    readonly PvStage.Setup s;
    readonly Sculpt m;
    readonly float t, mv, cell, ex, ey;
    /// <summary>カメラから画面までの距離(絵の単位)</summary>
    const float CamA = 1058f;

    public static void Build(in PvStage.Setup s, Native3D n, bool back)
    {
        float k = s.H / 1080f;
        n.Lit = true; n.Width = s.W; n.Height = s.H;
        n.Cam = s.H * .98f;
        n.PivotXY = Vector2.Zero; n.PivotZ = 400f * k;
        var m = new Sculpt(n, s.W / 2f, s.H / 2f, k, 1f, s.W, s.H) { Seg = 14, Now = s.T };
        new PvFlat3D(s, m).Run(back);
    }

    PvFlat3D(PvStage.Setup s, Sculpt m)
    {
        this.s = s; this.m = m;
        t = s.T * s.Spin;
        mv = s.Dist * 38f;
        cell = MathF.Max(18f, 104f * s.Spread * MathF.Max(.3f, s.Size));
        float z = MathF.Max(.05f, s.Zoom);
        ex = m.W / 2f / z * 1.12f + 14f;
        ey = m.H / 2f / z * 1.12f + 14f;
    }

    void Run(bool back)
    {
        if (back) m.Backdrop(V(s.Pal.Back1), V(s.Pal.Back2), -1500f, 7f);
        float z = MathF.Max(.05f, s.Zoom);
        m.Pitch(0, 0, -400, s.Pitch, () => m.Yaw(0, 0, -400, s.Yaw, () => m.Turn(0, 0, s.Roll, () => m.Scale(0, 0, 0, z, Scene))));
    }

    void Scene()
    {
        switch (s.Scene)
        {
            case PvScene.Gradient: Gradient(); break;
            case PvScene.Stripes: Stripes(); break;
            case PvScene.Sunburst: Sunburst(); break;
            case PvScene.SpeedLines: SpeedLines(); break;
            case PvScene.Circles: Circles(); break;
            case PvScene.WaveLines: WaveLines(); break;
            case PvScene.Dots: Dots(); break;
            case PvScene.Checks: Checks(); break;
            case PvScene.Triangles: Triangles(); break;
            case PvScene.Hexagons: Hexagons(); break;
            case PvScene.Diamonds: Diamonds(); break;
            case PvScene.GridLines: GridLines(); break;
            case PvScene.Scan: Scan(); break;
            case PvScene.Particles: Particles(); break;
            case PvScene.Bokeh: Bokeh(); break;
            case PvScene.Meteors: Meteors(); break;
            case PvScene.RainLines: RainLines(); break;
            case PvScene.Petals: Petals(); break;
            case PvScene.Clouds: Clouds(); break;
            case PvScene.Hills: Hills(); break;
            case PvScene.Skyline: Skyline(); break;
            case PvScene.Waveform: Waveform(); break;
            case PvScene.Equalizer: Equalizer(); break;
            case PvScene.Aurora: Aurora(); break;
            case PvScene.Memphis: Memphis(); break;
            case PvScene.ArcRings: ArcRings(); break;
            case PvScene.Glitch: Glitch(); break;
            case PvScene.Confetti: Confetti(); break;
            case PvScene.Halftone: Halftone(); break;
            case PvScene.Curtain: Curtain(); break;
        }
    }

    // ---------------------------------------------------------------- 道具
    float R(int i, int k) => Gfx.Hash01(unchecked(s.Seed * 131 + i * 19 + k * 1013 + 11));
    static float Mod(float x, float q) => x - MathF.Floor(x / q) * q;
    static float Fr(float x) => x - MathF.Floor(x);
    Vector3 Pick(int i) { float r = R(i, 8); return r < .46f ? s.Pal.A : r < .8f ? s.Pal.B : s.Pal.C; }
    Vector3 Ramp(float q)
    {
        q = Math.Clamp(q, 0f, 1f);
        return q < .5f ? Vector3.Lerp(s.Pal.A, s.Pal.B, q * 2f) : Vector3.Lerp(s.Pal.B, s.Pal.C, q * 2f - 1f);
    }
    Vector3 Col3(int k) => k % 3 == 0 ? s.Pal.A : k % 3 == 1 ? s.Pal.B : s.Pal.C;
    Vector4 V(Vector3 v, float a = 1f)
    {
        var x = Vector3.Clamp(v + new Vector3(s.Flash * .12f), Vector3.Zero, Vector3.One);
        return new(x.X, x.Y, x.Z, Math.Clamp(a * s.Alpha, 0f, 1f));
    }
    /// <summary>文字を置く中央ほど薄くする割合</summary>
    float Calm(float x, float y)
    {
        if (s.Clear <= 0f) return 1f;
        float d = MathF.Sqrt(x * x / (m.W * m.W * .2f) + y * y / (m.H * m.H * .13f));
        return Math.Clamp(d / s.Clear, .3f, 1f);
    }
    /// <summary>奥行き z の所で、画面に見える広さが何倍になるか</summary>
    static float D(float z) => (CamA - z) / CamA;
    /// <summary>並べる数が多くなりすぎないように、升目を広げる</summary>
    float Cap(float c, float ratio, int max) => MathF.Max(c, MathF.Sqrt(4f * ex * ey / ratio / max));
    void Grid(float sx, float sy, bool stagger, float vx, float vy, Action<float, float, int, int> draw)
    {
        int c0 = (int)MathF.Floor((-ex - vx) / sx) - 1, c1 = (int)MathF.Ceiling((ex - vx) / sx) + 1, r0 = (int)MathF.Floor((-ey - vy) / sy) - 1, r1 = (int)MathF.Ceiling((ey - vy) / sy) + 1;
        for (int r = r0; r <= r1; r++)
            for (int q = c0; q <= c1; q++)
                draw(q * sx + vx + (stagger && (r & 1) != 0 ? sx / 2f : 0f), r * sy + vy, r, q);
    }
    void Seg(int n, Action draw) { int keep = m.Seg; m.Seg = n; draw(); m.Seg = keep; }

    // ---------------------------------------------------------------- 色と光
    void Gradient()
    {
        float mm = MathF.Max(ex, ey);
        Seg(22, () =>
        {
            for (int i = 0; i < 6; i++)
            {
                float a = i * 1.047f + t * (.18f + i * .03f), r = mm * (.34f + .13f * MathF.Sin(t * .4f + i)), z = -350f - 260f * MathF.Sin(t * .3f + i * 1.3f), f = D(z), rr = mm * (.42f + R(i, 1) * .2f) * f;
                m.Ball(MathF.Cos(a) * r * 1.2f * f, MathF.Sin(a * 1.2f) * r * .75f * f, z, rr, rr * .8f, rr * .45f, V(Col3(i), .5f));
            }
        });
    }

    void Aurora()
    {
        for (int k = 0; k < 4; k++)
        {
            float z0 = -250f - k * 170f, f = D(z0), step = 36f; var col = Col3(k);
            float X(float x) => x * f;
            float Top(float x) { float w = MathF.Sin(x / 230f + t * .5f + k * 1.7f) + MathF.Sin(x / 97f - t * .32f + k) * .42f; return (-ey * .72f + k * 42f + w * 74f) * f; }
            float Hh(float x) => (250f + 116f * MathF.Sin(x / 150f + t * .7f + k)) * f;
            float Z(float x) => z0 + 90f * MathF.Sin(x / 260f + t * .6f + k);
            float Al(float x) => .32f + .14f * MathF.Sin(x / 41f + t + k);
            for (float x = -ex; x < ex; x += step)
            {
                float x1 = x + step, a = Al(x);
                for (int j = 0; j < 3; j++)
                {
                    float q0 = j / 3f, q1 = (j + 1) / 3f;
                    m.Quad3D(new(X(x), Top(x) + Hh(x) * q0, Z(x)), new(X(x1), Top(x1) + Hh(x1) * q0, Z(x1)), new(X(x1), Top(x1) + Hh(x1) * q1, Z(x1)), new(X(x), Top(x) + Hh(x) * q1, Z(x)), V(col, a * (1f - j * .3f)));
                }
            }
        }
    }

    void Curtain()
    {
        int n = Math.Clamp(s.N / 2, 10, 40); float span = ex * 2f / n;
        Seg(12, () =>
        {
            for (int i = -1; i <= n; i++)
            {
                float z = -150f - R(i + 40, 5) * 650f, f = D(z), x = (-ex + (i + Fr(mv / (span * 8f))) * span) * f, q = .5f + .5f * MathF.Sin(i * .9f + t * 1.1f), w = span * (.45f + q * 1.5f) * f;
                var col = Ramp((i & 7) / 7f);
                m.Limb(x * .7f, -ey * f * 1.05f, z, x + MathF.Sin(t * .5f + i) * 60f * f, ey * f * 1.05f, z + 120f, w * .08f, w * .38f, V(col, .07f + q * .17f));
            }
        });
    }

    void Bokeh()
    {
        int n = Math.Clamp(s.N, 8, 160);
        Seg(16, () =>
        {
            for (int i = 0; i < n; i++)
            {
                float z = -80f - R(i, 5) * 900f, f = D(z);
                float x = (Fr(R(i, 0) + mv * .00016f * (.4f + R(i, 3))) - .5f) * ex * 2.1f * f, y = ((R(i, 1) - .5f) * ey * 2f + MathF.Sin(t * .55f + i) * 18f) * f;
                float r = (24f + R(i, 2) * R(i, 2) * 118f) * s.Size * f, a = (.14f + .17f * (.5f + .5f * MathF.Sin(t * 1.3f + i * 1.7f))) * Calm(x / f, y / f);
                m.Disc(x, y, z, r, r, r * .1f, V(Pick(i), a), .6f);
                m.Torus(x, y, z, r, MathF.Max(1.5f, r * .04f), V(Pick(i), a * 1.8f));
            }
        });
    }

    void Particles()
    {
        int n = Math.Clamp(s.N * 3, 40, 600);
        Seg(8, () =>
        {
            for (int i = 0; i < n; i++)
            {
                float z = 120f - R(i, 5) * 1000f, f = D(z);
                float x = ((R(i, 0) - .5f) * ex * 2f + MathF.Sin(t * .6f + i) * 12f) * f, y = (ey - Mod(R(i, 1) * ey * 2f + (mv + t * 34f) * (.45f + R(i, 2)), ey * 2f)) * f;
                float r = (2.6f + R(i, 3) * 5f) * s.Size * f, a = (.45f + .55f * MathF.Abs(MathF.Sin(t * 1.8f + i))) * MathF.Max(.6f, Calm(x / f, y / f));
                m.Ball(x, y, z, r * 2.8f, V(Pick(i), a * .2f));
                m.Ball(x, y, z, r, V(Vector3.Lerp(Pick(i), Vector3.One, .5f), a));
            }
        });
    }

    void Meteors()
    {
        int n = Math.Clamp(s.N / 2, 8, 80); float span = ex * 2f + ey * 2f;
        Seg(10, () =>
        {
            for (int i = 0; i < n; i++)
            {
                float z = -100f - R(i, 4) * 900f, f = D(z);
                float d = Mod(R(i, 0) * span + (mv + t * 90f) * (1.2f + R(i, 1) * 1.6f), span), x = (ex + ey - d) * f, y = (-ey + d * .62f + (R(i, 2) - .5f) * ey * 1.7f) * f, l = (90f + R(i, 3) * 260f) * s.Size * f;
                m.Limb(x, y, z, x + l, y - l * .62f, z - l * .4f, 6f * f, 1f, V(Vector3.Lerp(Pick(i), Vector3.One, .25f), .8f));
                m.Ball(x, y, z, 14f * f, V(Pick(i), .3f));
                m.Ball(x, y, z, 6f * f, V(Vector3.One));
            }
        });
    }

    void RainLines()
    {
        int n = Math.Clamp(s.N * 2, 30, 500); float h = ey * 2f + 200f;
        Seg(6, () =>
        {
            for (int i = 0; i < n; i++)
            {
                float z = -40f - R(i, 5) * 900f, f = D(z);
                float y0 = -ey - 100f + Mod(R(i, 1) * h + (mv + t * 300f) * (.7f + R(i, 2)), h), x0 = (R(i, 0) - .5f) * ex * 2.3f - y0 * .16f, l = (24f + R(i, 3) * 70f) * f;
                float x = x0 * f, y = y0 * f;
                m.Limb(x, y, z, x - l * .16f, y - l, z, 2f * f, 2f * f, V(Vector3.Lerp(Pick(i), Vector3.One, .2f), (.5f + R(i, 4) * .45f) * MathF.Max(.6f, Calm(x0, y0))));
            }
        });
    }

    // ---------------------------------------------------------------- 線と放射
    void Stripes()
    {
        float w = MathF.Max(cell, ex / 30f), h = ey * 2f; int i0 = (int)MathF.Floor((-ex - h - mv) / w) - 1, i1 = (int)MathF.Ceiling((ex + h - mv) / w) + 1;
        for (int i = i0; i <= i1; i++)
        {
            if ((i & 1) != 0) continue;
            float x = i * w + mv, z = 40f * MathF.Sin(i * .7f + t * 2f), d = 30f + 30f * (.5f + .5f * MathF.Sin(i * 1.3f + t));
            m.Slab(z, d, V(Ramp(((i % 3) + 3) % 3 / 2f), .85f), x, -ey, x + w * .62f, -ey, x + w * .62f - h * .55f, ey, x - h * .55f, ey);
        }
    }

    void Sunburst()
    {
        int n = Math.Clamp(s.N / 4, 8, 48); float mm = MathF.Sqrt(ex * ex + ey * ey) * 1.3f, step = MathF.Tau / n;
        for (int i = 0; i < n; i += 2)
        {
            float a = i * step + t * .16f, b = a + step, ri = 40f, zi = -1100f, zo = 60f + 50f * MathF.Sin(t + i);
            m.Quad3D(new(MathF.Cos(a) * ri, MathF.Sin(a) * ri, zi), new(MathF.Cos(a) * mm, MathF.Sin(a) * mm, zo), new(MathF.Cos(b) * mm, MathF.Sin(b) * mm, zo), new(MathF.Cos(b) * ri, MathF.Sin(b) * ri, zi), V(Ramp(i / (float)n), .8f));
        }
        Seg(18, () => m.Ball(0, 0, -1100f, 110f, V(Vector3.Lerp(s.Pal.C, Vector3.One, .5f), .9f)));
    }

    void SpeedLines()
    {
        int n = Math.Clamp(s.N * 2, 40, 300); float rr = MathF.Sqrt(ex * ex + ey * ey) * .75f, len = 2600f, sp = 600f + 900f * MathF.Max(.2f, MathF.Abs(s.Spin));
        Seg(6, () =>
        {
            for (int i = 0; i < n; i++)
            {
                float a = i * MathF.Tau / n + (R(i, 0) - .5f) * .05f, r = rr * (.85f + R(i, 1) * .4f + MathF.Max(0f, s.Clear - .4f) * .3f), w = 2f + R(i, 2) * 12f, l = 300f + R(i, 3) * 900f;
                float z0 = 150f - Mod(R(i, 4) * len - s.T * sp * (.7f + R(i, 5) * .6f), len);
                float cs = MathF.Cos(a), sn = MathF.Sin(a);
                m.Limb(cs * r, sn * r, z0, cs * r, sn * r, z0 - l, w, w * .3f, V(Pick(i), .75f));
            }
        });
    }

    void Circles()
    {
        float gap = MathF.Max(cell, 60f) * 2.6f, rr = MathF.Sqrt(ex * ex + ey * ey) * .62f, o = Mod(mv * 3f + t * 90f, gap); int id = (int)MathF.Floor((mv * 3f + t * 90f) / gap);
        Seg(36, () =>
        {
            for (int i = 0; ; i++)
            {
                float z = 200f - gap * i + o; if (z < -3200f) break;
                int c = i - id;
                float th = rr * (((c & 1) == 0) ? .07f : .035f), fade = Math.Clamp((z + 3200f) / 1200f, 0f, 1f);
                m.Torus(0, 0, z, rr, th, V(Ramp(((c % 5) + 5) % 5 / 4f), .85f * fade));
            }
        });
    }

    void WaveLines()
    {
        int n = Math.Clamp(s.N / 3, 6, 24); float gap = ey * 2.2f / n, st = 40f, r = MathF.Max(2f, 3.2f * s.Size);
        Seg(10, () =>
        {
            for (int k = 0; k < n; k++)
            {
                var q = new List<Vector3>((int)(ex * 2f / st) + 2); float y0 = -ey * 1.1f + (k + .5f) * gap;
                for (float x = -ex; x <= ex + st; x += st)
                    q.Add(new(x, y0 + MathF.Sin(x / 118f + t * 1.2f + k * .42f) * 34f + MathF.Sin(x / 47f - t * .8f + k) * 10f, 60f * MathF.Sin(x / 210f - t * .9f + k * .6f)));
                m.Tube(q, r, V(Ramp(k / (n - 1f)), .95f));
            }
        });
    }

    void GridLines()
    {
        float g = MathF.Max(cell * 1.1f, 80f), oz = Mod(mv * 4f + s.T * 60f, g), far = -3400f, side = 3600f;
        Seg(6, () =>
        {
            foreach (float yy in new[] { ey * .55f, -ey * .55f })
            {
                for (float x = -side; x <= side; x += g) m.Limb(x, yy, 200f, x, yy, far, 1.8f, 1.8f, V(s.Pal.A, .6f));
                for (float z = 200f - g + oz; z > far; z -= g) m.Limb(-side, yy, z, side, yy, z, 1.8f, 1.8f, V(s.Pal.B, .6f * Math.Clamp((z - far) / 1200f, 0f, 1f)));
            }
        });
        for (int r = 0; r < 14; r++)
            for (int q = -8; q < 8; q++)
            {
                float p = MathF.Max(0f, MathF.Sin(r * .7f + q * 1.1f + t * 1.5f));
                if (p < .55f) continue;
                float x = q * g + g / 2f, z = 200f - (r + .5f) * g + oz;
                m.Box(x, ey * .55f - 3f, z, g * .86f, 5f, g * .86f, V(Ramp((q & 3) / 3f), p * .45f), .05f);
            }
    }

    void Scan()
    {
        float g = 16f, o = Mod(mv * .5f, g);
        for (float y = -ey + o; y < ey; y += g) m.Box(0, y, -60f, ex * 2.2f, 2f, 2f, V(s.Pal.A, .16f), .1f);
        for (int i = 0; i < 5; i++)
        {
            float z = -40f - R(i, 2) * 500f, f = D(z), y = (-ey + Mod(R(i, 0) * ey * 2f + t * (86f + i * 42f), ey * 2f)) * f, h = (34f + R(i, 1) * 96f) * f;
            m.Box(0, y, z, ex * 2.4f * f, h, h * .5f, V(Col3(i), .3f), .2f);
            m.Box(0, y, z, ex * 2.4f * f, h * .25f, h * .5f + 2f, V(Vector3.Lerp(Col3(i), Vector3.One, .5f), .5f), .2f);
        }
    }

    void Waveform()
    {
        for (int k = 0; k < 3; k++)
        {
            float st = 24f, z = -k * 220f, f = D(z), y0 = (k - 1) * 165f * f; var col = Col3(k);
            for (float x = -ex; x < ex; x += st * 1.5f)
            {
                float e = .34f + .66f * MathF.Abs(MathF.Sin(x / 230f + t * .8f + k)), h = (MathF.Abs(MathF.Sin(x / 21f + t * 4.4f + k)) * .62f + MathF.Abs(MathF.Sin(x / 53f - t * 2.7f)) * .38f) * e * 118f * s.Size;
                m.Box((x + st / 2f) * f, y0, z, st * f, MathF.Max(4f, h * 2f * f), st * f, V(col, .8f * Calm(x, y0 / f)), .25f);
            }
        }
    }

    void Equalizer()
    {
        int n = Math.Clamp(s.N / 2, 12, 64); float w = ex * 2f / n;
        for (int row = 0; row < 2; row++)
        {
            float z = -row * 320f, f = D(z), base0 = ey * f;
            for (int i = 0; i < n; i++)
            {
                float q = MathF.Abs(MathF.Sin(i * .31f + t * 3f + row * 1.7f)) * .55f + MathF.Abs(MathF.Sin(i * 1.27f - t * 5.2f + row)) * .45f, h = (70f + q * 610f) * s.Size * f, x = (-ex + (i + .5f) * w) * f;
                int seg = Math.Clamp((int)(h / (90f * f)) + 1, 1, 7); float sh = h / seg;
                for (int b = 0; b < seg; b++)
                    m.Box(x, base0 - sh * (b + .5f), z, w * .84f * f, sh * .9f, w * .84f * f, V(Ramp(sh * (b + 1) / (680f * f)), row == 0 ? .9f : .6f), .12f);
            }
        }
    }

    // ---------------------------------------------------------------- タイル
    void Dots()
    {
        float c = Cap(cell, .86f, 700);
        Seg(10, () => Grid(c, c * .86f, true, Mod(mv, c), 0f, (x, y, r, q) =>
        {
            float p = .5f + .5f * MathF.Sin((x + y) / 160f - t * 1.6f);
            m.Ball(x, y, p * 90f - 45f, c * (.1f + p * .26f), V(Ramp(p), .85f * Calm(x, y)));
        }));
    }

    void Checks()
    {
        float c = Cap(cell, 1f, 900);
        Grid(c, c, false, Mod(mv, c * 2f), Mod(mv * .5f, c * 2f), (x, y, r, q) =>
        {
            if (((r + q) & 1) != 0) return;
            float p = .5f + .5f * MathF.Sin(q * .5f + r * .37f + t), cx = x + c / 2f, cy = y + c / 2f, ang = MathF.PI * Math.Clamp((p - .3f) / .4f, 0f, 1f);
            m.Pitch(cx, cy, 0, ang, () => m.Box(cx, cy, 0, c, c, c * .16f, V(Ramp(p), .78f * Calm(cx, cy)), .08f));
        });
    }

    void Triangles()
    {
        float c = Cap(cell, .866f, 500), h = c * .866f;
        Grid(c, h, true, Mod(mv, c), 0f, (x, y, r, q) =>
        {
            float p = .5f + .5f * MathF.Sin(q * .62f + r * .91f + t * 1.25f), a = (.4f + p * .55f) * Calm(x, y), d = 16f + p * 70f;
            m.Slab(d / 2f, d, V(Ramp(p), a), x, y, x + c, y, x + c / 2f, y + h);
            float d2 = 16f + (1f - p) * 70f;
            m.Slab(d2 / 2f, d2, V(Ramp(1f - p), a * .7f), x + c, y, x + c * 1.5f, y + h, x + c / 2f, y + h);
        });
    }

    void Hexagons()
    {
        float rr = Cap(cell, 1.5f / 1.732f, 600) * .62f;
        Grid(rr * 1.732f, rr * 1.5f, true, Mod(mv, rr * 1.732f), 0f, (x, y, r, q) =>
        {
            float p = .5f + .5f * MathF.Sin(q * .55f - r * .8f + t * 1.4f), d = 20f + p * 160f, h = rr * .94f;
            var pts = new List<Vector2>(6);
            for (int i = 0; i < 6; i++) { float a = MathF.PI / 6f + i * MathF.Tau / 6f; pts.Add(new(x + MathF.Cos(a) * h, y + MathF.Sin(a) * h)); }
            m.Slab(pts, d / 2f - 40f, d, V(Ramp(p), (.55f + p * .45f) * Calm(x, y)));
        });
    }

    void Diamonds()
    {
        float c = Cap(cell, .62f, 600);
        Grid(c, c * .62f, true, Mod(mv, c), 0f, (x, y, r, q) =>
        {
            float p = .5f + .5f * MathF.Sin(r * .77f + q * .35f - t * 1.5f), a = c * (.2f + p * .3f), b = c * .31f * (.5f + p * .5f);
            m.Yaw(x, y, 0, (p - .5f) * 2.4f, () => m.Slab(0, 14f + p * 20f, V(Ramp(p), (.45f + p * .5f) * Calm(x, y)), x, y - b, x + a, y, x, y + b, x - a, y));
        });
    }

    void Halftone()
    {
        float g = Cap(MathF.Max(14f, cell * .34f), .866f, 900);
        Seg(8, () => Grid(g, g * .866f, true, Mod(mv * .4f, g), 0f, (x, y, r, q) =>
        {
            float d = MathF.Sqrt(x * x + y * y) / MathF.Max(ex, ey), p = Math.Clamp(.5f + .5f * MathF.Sin(d * 8f - t * 1.5f) + (d - .65f) * .8f, 0f, 1f);
            if (p > .05f) m.Ball(x, y, -(1f - p) * 160f, g * .56f * p, V(Ramp(d), .88f));
        }));
    }

    // ---------------------------------------------------------------- 風景
    void Clouds()
    {
        Seg(14, () =>
        {
            for (int k = 0; k < 4; k++)
            {
                float z = -1000f + k * 280f, f = D(z), span = ex * 2f + 620f, y0 = -ey * .68f + k * ey * .52f; var col = Vector3.Lerp(k % 2 == 0 ? s.Pal.A : s.Pal.B, Vector3.One, .34f + k * .1f);
                for (int i = 0; i < 6; i++)
                {
                    float x = (Mod(R(i + k * 13, 0) * span + (mv + t * 16f) * (.42f + k * .3f), span) - ex - 310f) * f, y = (y0 + (R(i + k * 13, 1) - .5f) * 92f) * f, zz = (.7f + k * .22f + R(i, 2) * .34f) * s.Size * f, a = .5f + k * .12f;
                    for (int j = 0; j < 7; j++)
                    {
                        float rr = (78f - MathF.Abs(j - 3) * 13f) * zz;
                        m.Ball(x + (j - 3) * 56f * zz, y + MathF.Abs(j - 3) * 11f * zz, z + R(i * 7 + j, 3) * 40f, rr, rr * .9f, rr * .7f, V(col, a));
                    }
                }
            }
        });
    }

    void Hills()
    {
        Seg(22, () => m.Ball(ex * .36f * D(-1500f), -ey * .42f * D(-1500f), -1500f, 118f * D(-1500f), V(s.Pal.C, .95f)));
        for (int k = 0; k < 6; k++)
        {
            float q0 = k / 5f, z = -1300f + k * 240f, f = D(z), base0 = -ey * .22f + q0 * ey * 1.18f, amp = 112f - k * 8f, sp = (mv + t * 8f) * (.2f + q0 * .9f), st = 30f;
            var pts = new List<Vector2>((int)(ex * 2f / st) + 5) { new(-ex * f, (ey + 40f) * f) };
            for (float x = -ex; x <= ex + st; x += st) pts.Add(new(x * f, (base0 + MathF.Sin((x + sp) / 278f + k * 1.8f) * amp + MathF.Sin((x + sp) / 97f + k) * amp * .24f) * f));
            pts.Add(new((ex + st) * f, (ey + 40f) * f));
            pts.Reverse();
            m.Slab(pts, z, 80f, V(Vector3.Lerp(Ramp(q0), s.Pal.Back1, (1f - q0) * .55f)));
        }
    }

    void Skyline()
    {
        for (int k = 0; k < 3; k++)
        {
            float z = -900f + k * 330f, f = D(z), w0 = 58f + k * 20f, span = ex * 2f + w0 * 6f, sp = mv * (.26f + k * .42f), floor0 = ey; int n = (int)(span / w0) + 2, off = (int)MathF.Floor(sp / w0);
            var col = Vector3.Lerp(Vector3.Lerp(s.Pal.A, s.Pal.Back1, .35f), s.Pal.Back1, (2 - k) * .3f + .1f);
            var lit = V(k == 2 ? s.Pal.C : s.Pal.B, .85f);
            for (int i = 0; i < n; i++)
            {
                int id = i - off + k * 211; float x = -ex - w0 * 2f + i * w0 + Mod(sp, w0), h = (120f + R(id, 1) * 300f + k * 92f) * MathF.Max(.5f, s.Size);
                float bx = (x + w0 / 2f) * f, bw = (w0 - 4f) * f, bh = h * f, by = floor0 * f - bh / 2f, bd = w0 * .8f * f, front = z + bd / 2f + 1f;
                m.Box(bx, by, z, bw, bh, bd, V(col), .04f);
                for (float wy = floor0 - h + 14f; wy < floor0 - 12f; wy += 24f)
                    for (float wx = x + 9f; wx < x + w0 - 13f; wx += 16f)
                        if (R((int)(wx * 3f + wy * 17f) + id, 3) > .62f)
                            m.Quad3D(new(wx * f, wy * f, front), new((wx + 6f) * f, wy * f, front), new((wx + 6f) * f, (wy + 9f) * f, front), new(wx * f, (wy + 9f) * f, front), lit);
            }
        }
    }

    // ---------------------------------------------------------------- 飾り
    void Petals()
    {
        int n = Math.Clamp(s.N * 2, 16, 300); float h = ey * 2f + 120f;
        for (int i = 0; i < n; i++)
        {
            float z = 60f - R(i, 5) * 900f, f = D(z);
            float y0 = -ey - 60f + Mod(R(i, 1) * h + (mv * .5f + t * 72f) * (.5f + R(i, 2)), h), x0 = (R(i, 0) - .5f) * ex * 2.15f + MathF.Sin(t * .9f + i) * 44f, sz = (13f + R(i, 3) * 26f) * s.Size * f, a = t * (.7f + R(i, 4)) + i, fl = t * 1.5f + i * 1.3f;
            float x = x0 * f, y = y0 * f;
            var col = V(Vector3.Lerp(Pick(i), Vector3.One, .22f), .9f * Calm(x0, y0));
            m.Move(x, y, z, () => m.Turn(0, 0, a, () => m.Yaw(0, 0, 0, fl, () => m.Pitch(0, 0, 0, fl * .7f, () =>
                m.Slab(1.5f * f, 1.5f * f, col, -1.2f * sz, 0, -.55f * sz, -.58f * sz, .45f * sz, -.56f * sz, 1.2f * sz, -.12f * sz, .84f * sz, 0, 1.2f * sz, .12f * sz, .45f * sz, .56f * sz, -.55f * sz, .58f * sz)))));
        }
    }

    void Confetti()
    {
        int n = Math.Clamp(s.N * 3, 24, 500); float h = ey * 2f + 80f;
        for (int i = 0; i < n; i++)
        {
            float z = 60f - R(i, 5) * 900f, f = D(z);
            float y0 = -ey - 40f + Mod(R(i, 1) * h + (mv * .5f + t * 118f) * (.5f + R(i, 2)), h), x0 = (R(i, 0) - .5f) * ex * 2.1f + MathF.Sin(t * 1.5f + i) * 24f, w = (6f + R(i, 3) * 9f) * s.Size * f, a = t * (1f + R(i, 4) * 2f) + i;
            float x = x0 * f, y = y0 * f;
            var col = V(Pick(i), .95f * Calm(x0, y0));
            m.Move(x, y, z, () => m.Turn(0, 0, a, () => m.Pitch(0, 0, 0, t * 3f + i, () => m.Box(0, 0, 0, w * 2f, w * 1.3f, 1.5f * f, col, .05f))));
        }
    }

    void Memphis()
    {
        int n = Math.Clamp(s.N, 12, 160);
        Seg(14, () =>
        {
            for (int i = 0; i < n; i++)
            {
                float z = 40f - R(i, 5) * 900f, f = D(z);
                float x0 = (Fr(R(i, 0) + mv * .00011f) - .5f) * ex * 2.2f, y0 = (R(i, 1) - .5f) * ey * 2f + MathF.Sin(t * .8f + i) * 14f, sz = (15f + R(i, 2) * 39f) * s.Size * f, a = t * (R(i, 3) - .5f) * 1.6f + i;
                float x = x0 * f, y = y0 * f;
                var c = V(Pick(i), .95f * Calm(x0, y0));
                m.Move(x, y, z, () => m.Turn(0, 0, a, () => m.Yaw(0, 0, 0, a * .8f, () =>
                {
                    switch (i % 6)
                    {
                        case 0: m.Ball(0, 0, 0, sz, c); break;
                        case 1: m.Torus(0, 0, 0, sz * .85f, sz * .2f, c); break;
                        case 2: m.Lathe(0, 0, c, 0, -sz * 1.2f, sz, sz * .8f, 0, sz * .8f); break;
                        case 3: m.Box(0, 0, 0, sz * 1.6f, sz * 1.6f, sz * 1.6f, c, .15f); break;
                        case 4: m.Box(0, 0, 0, sz * 2f, sz * .45f, sz * .45f, c, .3f); m.Box(0, 0, 0, sz * .45f, sz * 2f, sz * .45f, c, .3f); break;
                        default:
                            {
                                var q = new List<Vector3>(13);
                                for (int k = 0; k <= 12; k++) q.Add(new((k - 6) * sz * .34f, MathF.Sin(k * 1.57f) * sz * .34f, 0));
                                m.Tube(q, sz * .12f, c);
                                break;
                            }
                    }
                })));
            }
        });
    }

    void ArcRings()
    {
        int n = Math.Clamp(s.N / 5, 5, 20); float mm = MathF.Sqrt(ex * ex + ey * ey) * .8f, gap = mm / (n + 1), c0 = MathF.Max(0f, s.Clear - .35f) * 180f;
        Seg(10, () =>
        {
            for (int i = 0; i < n; i++)
            {
                float r = gap * (i + 1) + c0, dir = (i & 1) == 0 ? 1f : -1f, a = t * dir * (.32f + R(i, 1) * .5f) + R(i, 2) * MathF.Tau, tilt = (R(i, 6) - .5f) * 1.1f + MathF.Sin(t * .4f + i) * .25f, yaw = (R(i, 7) - .5f) * 1.1f; int arcs = 1 + i % 3;
                var col = V(Ramp(i / (n - 1f)), .85f); float th = MathF.Max(2f, gap * (.07f + R(i, 4) * .11f));
                m.Pitch(0, 0, -300, tilt, () => m.Yaw(0, 0, -300, yaw, () =>
                {
                    for (int k = 0; k < arcs; k++)
                    {
                        float a0 = a + k * MathF.Tau / arcs, len = (.5f + R(i + k * 20, 3) * 1.1f) * MathF.PI / arcs * 1.4f;
                        int steps = Math.Clamp((int)(len * r / 40f), 6, 48);
                        var q = new List<Vector3>(steps + 1);
                        for (int j = 0; j <= steps; j++) { float b = a0 + len * j / steps; q.Add(new(MathF.Cos(b) * r, MathF.Sin(b) * r, -300f)); }
                        m.Tube(q, th, col);
                    }
                }));
            }
        });
    }

    void Glitch()
    {
        int tick = (int)MathF.Floor(s.T * 11f * MathF.Max(.2f, MathF.Abs(s.Spin))), n = Math.Clamp(s.N / 2, 10, 90);
        for (int i = 0; i < n; i++)
        {
            int id = i * 7 + tick * 31; float y = (R(id, 0) - .5f) * ey * 2f, h = 3f + R(id, 1) * R(id, 1) * 74f, x = (R(id, 2) - .5f) * ex * 2f, w = 60f + R(id, 3) * 760f, z = (R(id, 5) - .5f) * 400f;
            m.Box(x, y, z, w, h, 10f + R(id, 6) * 60f, V(Pick(id), (.3f + R(id, 4) * .6f) * Calm(x, y)), .05f);
            if (i % 4 == 0)
            {
                m.Box(x + 12f, y + h / 2f + 2f, z + 20f, w, 3f, 3f, V(s.Pal.A, .85f), .1f);
                m.Box(x - 12f, y - h / 2f - 2f, z - 20f, w, 3f, 3f, V(s.Pal.B, .85f), .1f);
            }
        }
    }
}
