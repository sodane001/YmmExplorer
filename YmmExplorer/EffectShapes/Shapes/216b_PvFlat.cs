using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;

namespace YMM4EffectShapes;

// =====================================================================
//  文字PV背景の2D(平面)の背景。色は配色、動きは「回る速さ」「進む速さ」、拍やズームも3Dと共通
// =====================================================================
internal sealed class PvFlat
{
    readonly ID2D1DeviceContext dc;
    readonly ID2D1SolidColorBrush br;
    readonly PvStage.Setup s;
    readonly float u, t, mv, cell;
    float ex, ey;

    public static bool IsFlat(PvScene x) => x >= PvScene.Gradient && (x <= PvScene.Curtain || x >= PvScene.Stars);

    public static void Paint(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, in PvStage.Setup s) => new PvFlat(dc, br, s).Run();

    PvFlat(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, PvStage.Setup s)
    {
        this.dc = dc; this.br = br; this.s = s;
        u = s.H / 1080f;
        t = s.T * s.Spin;
        mv = s.Dist * 38f * u;
        cell = MathF.Max(18f, 104f * u * s.Spread * MathF.Max(.3f, s.Size));
    }

    void Run()
    {
        var prev = dc.Transform;
        float z = MathF.Max(.05f, s.Zoom), cs = MathF.Abs(MathF.Cos(s.Roll)), sn = MathF.Abs(MathF.Sin(s.Roll));
        ex = (s.W * cs + s.H * sn) / 2f / z + 14f * u;
        ey = (s.W * sn + s.H * cs) / 2f / z + 14f * u;
        dc.Transform = Matrix3x2.CreateRotation(s.Roll) * Matrix3x2.CreateScale(z) * Matrix3x2.CreateTranslation(s.W / 2f, s.H / 2f) * prev;
        try
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
                case PvScene.Stars: Stars(); break;
                case PvScene.Bubbles: Bubbles(); break;
                case PvScene.Snow: Snow(); break;
                case PvScene.Fireworks: Fireworks(); break;
                case PvScene.Plaid: Plaid(); break;
                case PvScene.Zigzag: Zigzag(); break;
                case PvScene.Hearts: Hearts(); break;
                case PvScene.Sparkles: Sparkles(); break;
                case PvScene.Ribbons: Ribbons(); break;
                case PvScene.Rays: Rays(); break;
                case PvScene.MonoGeo: MonoGeo(); break;
            }
        }
        finally { dc.Transform = prev; }
    }

    // ---------------------------------------------------------------- 道具
    float R(int i, int k) => Gfx.Hash01(unchecked(s.Seed * 131 + i * 19 + k * 1013 + 11));
    static float Mod(float x, float m) => x - MathF.Floor(x / m) * m;
    static float Fr(float x) => x - MathF.Floor(x);
    Vector3 Pick(int i) { float r = R(i, 8); return r < .46f ? s.Pal.A : r < .8f ? s.Pal.B : s.Pal.C; }
    Vector3 Ramp(float q)
    {
        q = Math.Clamp(q, 0f, 1f);
        return q < .5f ? Vector3.Lerp(s.Pal.A, s.Pal.B, q * 2f) : Vector3.Lerp(s.Pal.B, s.Pal.C, q * 2f - 1f);
    }
    Color4 C(Vector3 v, float a = 1f)
    {
        var x = Vector3.Clamp(v + new Vector3(s.Flash * .12f), Vector3.Zero, Vector3.One);
        return new(x.X, x.Y, x.Z, Math.Clamp(a * s.Alpha, 0f, 1f));
    }
    /// <summary>文字を置く中央ほど薄くする割合</summary>
    float Calm(float x, float y)
    {
        if (s.Clear <= 0f) return 1f;
        float d = MathF.Sqrt(x * x / (s.W * s.W * .2f) + y * y / (s.H * s.H * .13f));
        return Math.Clamp(d / s.Clear, .3f, 1f);
    }
    void Rect(float l, float top, float r, float bottom, Color4 c) { br.Color = c; dc.FillRectangle(new RawRectF(l, top, r, bottom), br); }
    void Dot(float x, float y, float r, Color4 c) { if (r <= .1f) return; br.Color = c; dc.FillEllipse(new Ellipse(new(x, y), r, r), br); }
    void Ring(float x, float y, float r, Color4 c, float w) { if (r <= .1f) return; br.Color = c; dc.DrawEllipse(new Ellipse(new(x, y), r, r), br, w); }
    void Line(float x0, float y0, float x1, float y1, Color4 c, float w) { br.Color = c; dc.DrawLine(new(x0, y0), new(x1, y1), br, w); }
    void Poly(Color4 c, params float[] xy)
    {
        var q = new List<Vector2>(xy.Length / 2);
        for (int i = 0; i + 1 < xy.Length; i += 2) q.Add(new(xy[i], xy[i + 1]));
        br.Color = c; Gfx.FillPoly(dc, br, q);
    }
    void Path(List<Vector2> q, Color4 c, float w) { br.Color = c; Gfx.Polyline(dc, br, q, w); }
    void Fan(float x, float y, float r, int n, float rot, Color4 c, float yScale = 1f)
    {
        var q = new List<Vector2>(n);
        for (int i = 0; i < n; i++) { float a = rot + i * MathF.Tau / n; q.Add(new(x + MathF.Cos(a) * r, y + MathF.Sin(a) * r * yScale)); }
        br.Color = c; Gfx.FillPoly(dc, br, q);
    }
    void Blob(float x, float y, float r, Vector3 col, float a)
    {
        GradientStop[] gs = [new() { Position = 0f, Color = C(col, a) }, new() { Position = .55f, Color = C(col, a * .42f) }, new() { Position = 1f, Color = C(col, 0f) }];
        using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
        using var g = dc.CreateRadialGradientBrush(new RadialGradientBrushProperties { Center = new(x, y), GradientOriginOffset = Vector2.Zero, RadiusX = r, RadiusY = r }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
        dc.FillRectangle(new RawRectF(x - r, y - r, x + r, y + r), g);
    }
    void Grid(float sx, float sy, bool stagger, float vx, float vy, Action<float, float, int, int> draw)
    {
        int c0 = (int)MathF.Floor((-ex - vx) / sx) - 2, c1 = (int)MathF.Ceiling((ex - vx) / sx) + 2, r0 = (int)MathF.Floor((-ey - vy) / sy) - 2, r1 = (int)MathF.Ceiling((ey - vy) / sy) + 2;
        for (int r = r0; r <= r1; r++)
            for (int q = c0; q <= c1; q++)
                draw(q * sx + vx + (stagger && (r & 1) != 0 ? sx / 2f : 0f), r * sy + vy, r, q);
    }

    // ---------------------------------------------------------------- 色と光
    void Gradient()
    {
        float m = MathF.Max(ex, ey);
        for (int i = 0; i < 6; i++)
        {
            float a = i * 1.047f + t * (.18f + i * .03f), r = m * (.34f + .13f * MathF.Sin(t * .4f + i));
            Blob(MathF.Cos(a) * r * 1.2f, MathF.Sin(a * 1.2f) * r * .75f, m * (.62f + R(i, 1) * .28f), i % 3 == 0 ? s.Pal.A : i % 3 == 1 ? s.Pal.B : s.Pal.C, .72f);
        }
    }

    void Aurora()
    {
        float step = MathF.Max(4f, 9f * u);
        for (int k = 0; k < 4; k++)
        {
            var col = k % 3 == 0 ? s.Pal.A : k % 3 == 1 ? s.Pal.B : s.Pal.C;
            for (float x = -ex; x < ex; x += step)
            {
                float w = MathF.Sin(x / (230f * u) + t * .5f + k * 1.7f) + MathF.Sin(x / (97f * u) - t * .32f + k) * .42f;
                float top = -ey * .72f + k * 42f * u + w * 74f * u, h = (250f + 116f * MathF.Sin(x / (150f * u) + t * .7f + k)) * u;
                float a = .2f + .12f * MathF.Sin(x / (41f * u) + t + k);
                for (int j = 0; j < 4; j++) Rect(x, top + h * j * .25f, x + step + .6f, top + h * (j + 1) * .25f, C(col, a * (1f - j * .23f)));
            }
        }
    }

    void Curtain()
    {
        int n = Math.Clamp(s.N / 2, 10, 60); float span = ex * 2f / n;
        for (int i = -1; i <= n; i++)
        {
            float x = -ex + (i + Fr(mv / (span * 8f))) * span, q = .5f + .5f * MathF.Sin(i * .9f + t * 1.1f), w = span * (.45f + q * 1.5f);
            var col = Ramp((i & 7) / 7f);
            Poly(C(col, .1f + q * .28f), x - w * .3f, -ey, x + w * .3f, -ey, x + w, ey, x - w, ey);
        }
    }

    void Bokeh()
    {
        int n = Math.Clamp(s.N, 8, 220);
        for (int i = 0; i < n; i++)
        {
            float x = (Fr(R(i, 0) + mv * .00016f / u * (.4f + R(i, 3))) - .5f) * ex * 2.1f, y = (R(i, 1) - .5f) * ey * 2f + MathF.Sin(t * .55f + i) * 18f * u;
            float r = (24f + R(i, 2) * R(i, 2) * 118f) * u * s.Size, a = (.12f + .17f * (.5f + .5f * MathF.Sin(t * 1.3f + i * 1.7f))) * Calm(x, y);
            Dot(x, y, r, C(Pick(i), a)); Ring(x, y, r, C(Pick(i), a * 1.5f), MathF.Max(1f, 2f * u));
        }
    }

    void Particles()
    {
        int n = Math.Clamp(s.N * 4, 40, 1200);
        for (int i = 0; i < n; i++)
        {
            float x = (R(i, 0) - .5f) * ex * 2f + MathF.Sin(t * .6f + i) * 12f * u, y = ey - Mod(R(i, 1) * ey * 2f + (mv + t * 34f * u) * (.45f + R(i, 2)), ey * 2f);
            float r = (2.2f + R(i, 3) * 4.6f) * u * s.Size, a = (.45f + .55f * MathF.Abs(MathF.Sin(t * 1.8f + i))) * MathF.Max(.6f, Calm(x, y));
            Dot(x, y, r * 3.2f, C(Pick(i), a * .24f)); Dot(x, y, r, C(Vector3.Lerp(Pick(i), Vector3.One, .5f), a));
        }
    }

    void Meteors()
    {
        int n = Math.Clamp(s.N / 2, 8, 120); float span = ex * 2f + ey * 2f;
        for (int i = 0; i < n; i++)
        {
            float d = Mod(R(i, 0) * span + (mv + t * 90f * u) * (1.2f + R(i, 1) * 1.6f), span), x = ex + ey - d, y = -ey + d * .62f + (R(i, 2) - .5f) * ey * 1.7f, l = (90f + R(i, 3) * 260f) * u * s.Size;
            for (int k = 0; k < 5; k++) Line(x + l * k / 5f, y - l * .62f * k / 5f, x + l * (k + 1) / 5f, y - l * .62f * (k + 1) / 5f, C(Vector3.Lerp(Pick(i), Vector3.One, .25f), 1f - k * .17f), MathF.Max(1.4f, (6.4f - k * .95f) * u));
            Dot(x, y, 12f * u, C(Pick(i), .32f)); Dot(x, y, 5.6f * u, C(Vector3.One, 1f));
        }
    }

    void RainLines()
    {
        int n = Math.Clamp(s.N * 3, 30, 900); float h = ey * 2f + 200f * u;
        for (int i = 0; i < n; i++)
        {
            float y = -ey - 100f * u + Mod(R(i, 1) * h + (mv + t * 300f * u) * (.7f + R(i, 2)), h), x = (R(i, 0) - .5f) * ex * 2.3f - y * .16f, l = (24f + R(i, 3) * 70f) * u;
            Line(x, y, x - l * .16f, y - l, C(Vector3.Lerp(Pick(i), Vector3.One, .2f), (.5f + R(i, 4) * .45f) * MathF.Max(.6f, Calm(x, y))), MathF.Max(1.4f, 3.2f * u));
        }
    }

    // ---------------------------------------------------------------- 線と放射
    void Stripes()
    {
        float w = cell, h = ey * 2f; int i0 = (int)MathF.Floor((-ex - h - mv) / w) - 1, i1 = (int)MathF.Ceiling((ex + h - mv) / w) + 1;
        for (int i = i0; i <= i1; i++)
        {
            float x = i * w + mv; var col = Ramp(((i % 3) + 3) % 3 / 2f);
            if ((i & 1) == 0) Poly(C(col, .78f), x, -ey, x + w * .62f, -ey, x + w * .62f - h * .55f, ey, x - h * .55f, ey);
        }
    }

    void Sunburst()
    {
        int n = Math.Clamp(s.N / 4, 8, 48); float m = MathF.Sqrt(ex * ex + ey * ey) * 1.1f, step = MathF.Tau / n;
        for (int i = 0; i < n; i++)
        {
            float a = i * step + t * .16f;
            if ((i & 1) == 0) Poly(C(Ramp(i / (float)n), .72f), 0, 0, MathF.Cos(a) * m, MathF.Sin(a) * m, MathF.Cos(a + step) * m, MathF.Sin(a + step) * m);
        }
    }

    void SpeedLines()
    {
        int n = Math.Clamp(s.N * 2, 40, 360); float m = MathF.Sqrt(ex * ex + ey * ey) * 1.08f; int tick = (int)MathF.Floor(s.T * 14f * MathF.Max(.2f, MathF.Abs(s.Spin)));
        for (int i = 0; i < n; i++)
        {
            float a = i * MathF.Tau / n + (R(i, 0 + tick % 7) - .5f) * .05f, inner = m * (.34f + R(i, 1 + tick % 5) * .3f + MathF.Max(0f, s.Clear - .4f) * .3f), w = (2f + R(i, 2 + tick % 3) * 15f) * u;
            float cs = MathF.Cos(a), sn = MathF.Sin(a);
            Poly(C(Pick(i), .72f), cs * m - sn * w, sn * m + cs * w, cs * m + sn * w, sn * m - cs * w, cs * inner, sn * inner);
        }
    }

    void Circles()
    {
        float gap = cell * .78f, m = MathF.Sqrt(ex * ex + ey * ey) + gap, o = Mod(mv + t * 34f * u, gap); int id = (int)MathF.Floor((mv + t * 34f * u) / gap);
        for (int i = 0; gap * i < m; i++) Ring(0, 0, gap * i + o, C(Ramp((((i - id) % 5) + 5) % 5 / 4f), .32f + .36f * MathF.Min(1f, (gap * i + o) / (m * .35f))), gap * (.18f + .16f * (((i - id) & 1) == 0 ? 1f : 0f)));
    }

    void WaveLines()
    {
        int n = Math.Clamp(s.N / 3, 6, 42); float gap = ey * 2.2f / n, st = MathF.Max(6f, 12f * u);
        for (int k = 0; k < n; k++)
        {
            var q = new List<Vector2>((int)(ex * 2f / st) + 2); float y0 = -ey * 1.1f + (k + .5f) * gap;
            for (float x = -ex; x <= ex + st; x += st) q.Add(new(x, y0 + MathF.Sin(x / (118f * u) + t * 1.2f + k * .42f) * 34f * u + MathF.Sin(x / (47f * u) - t * .8f + k) * 10f * u));
            Path(q, C(Ramp(k / (n - 1f)), .92f), MathF.Max(1.6f, 5.4f * u * s.Size));
        }
    }

    void GridLines()
    {
        float g = cell * 1.1f, ox = Mod(mv, g), oy = Mod(mv * .55f, g);
        for (float x = -ex - g + ox; x <= ex + g; x += g) Line(x, -ey, x, ey, C(s.Pal.A, .42f), MathF.Max(1f, 1.5f * u));
        for (float y = -ey - g + oy; y <= ey + g; y += g) Line(-ex, y, ex, y, C(s.Pal.B, .42f), MathF.Max(1f, 1.5f * u));
        Grid(g, g, false, ox, oy, (x, y, r, q) =>
        {
            float p = MathF.Max(0f, MathF.Sin(r * .7f + q * 1.1f + t * 1.5f));
            if (p > .25f) Rect(x + 2f * u, y + 2f * u, x + g - 2f * u, y + g - 2f * u, C(Ramp((q & 3) / 3f), p * .22f * Calm(x, y)));
            Dot(x, y, 3f * u, C(s.Pal.C, .75f));
        });
    }

    void Scan()
    {
        float g = MathF.Max(3f, 7f * u), o = Mod(mv * .5f, g);
        for (float y = -ey + o; y < ey; y += g) Line(-ex, y, ex, y, C(s.Pal.A, .14f), MathF.Max(1f, 1.6f * u));
        for (int i = 0; i < 4; i++)
        {
            float y = -ey + Mod(R(i, 0) * ey * 2f + t * (86f + i * 42f) * u, ey * 2f), h = (34f + R(i, 1) * 96f) * u; var c = i % 3 == 0 ? s.Pal.A : i % 3 == 1 ? s.Pal.B : s.Pal.C;
            for (int j = 0; j < 8; j++) Rect(-ex, y + h * j / 8f, ex, y + h * (j + 1) / 8f + .5f, C(c, .34f * MathF.Sin((j + .5f) / 8f * MathF.PI)));
        }
    }

    void Waveform()
    {
        for (int k = 0; k < 3; k++)
        {
            float st = MathF.Max(5f, 7f * u), y0 = (k - 1) * 165f * u; var col = k == 0 ? s.Pal.A : k == 1 ? s.Pal.B : s.Pal.C;
            for (float x = -ex; x < ex; x += st * 1.7f)
            {
                float e = .34f + .66f * MathF.Abs(MathF.Sin(x / (230f * u) + t * .8f + k)), h = (MathF.Abs(MathF.Sin(x / (21f * u) + t * 4.4f + k)) * .62f + MathF.Abs(MathF.Sin(x / (53f * u) - t * 2.7f)) * .38f) * e * 118f * u * s.Size;
                Rect(x, y0 - h, x + st, y0 + h, C(col, .62f * Calm(x, y0)));
            }
        }
    }

    void Equalizer()
    {
        int n = Math.Clamp(s.N / 2, 12, 96); float w = ex * 2f / n, base0 = ey;
        for (int i = 0; i < n; i++)
        {
            float q = MathF.Abs(MathF.Sin(i * .31f + t * 3f)) * .55f + MathF.Abs(MathF.Sin(i * 1.27f - t * 5.2f)) * .45f, h = (70f + q * 610f) * u * s.Size, x = -ex + i * w;
            int blocks = Math.Max(1, (int)(h / (20f * u)));
            for (int b = 0; b < blocks; b++) Rect(x + w * .08f, base0 - (b + 1) * 20f * u + 3f * u, x + w * .92f, base0 - b * 20f * u, C(Ramp(b * 20f * u / (680f * u)), .78f));
        }
    }

    // ---------------------------------------------------------------- タイル
    void Dots()
    {
        Grid(cell, cell * .86f, true, Mod(mv, cell), 0f, (x, y, r, q) =>
        {
            float p = .5f + .5f * MathF.Sin((x + y) / (160f * u) - t * 1.6f);
            Dot(x, y, cell * (.1f + p * .26f), C(Ramp(p), .78f * Calm(x, y)));
        });
    }

    void Checks()
    {
        Grid(cell, cell, false, Mod(mv, cell * 2f), Mod(mv * .5f, cell * 2f), (x, y, r, q) =>
        {
            if (((r + q) & 1) != 0) return;
            float p = .5f + .5f * MathF.Sin(q * .5f + r * .37f + t);
            Rect(x, y, x + cell + .5f, y + cell + .5f, C(Ramp(p), .68f * Calm(x + cell / 2f, y + cell / 2f)));
        });
    }

    void Triangles()
    {
        float h = cell * .866f;
        Grid(cell, h, true, Mod(mv, cell), 0f, (x, y, r, q) =>
        {
            float p = .5f + .5f * MathF.Sin(q * .62f + r * .91f + t * 1.25f), a = (.26f + p * .58f) * Calm(x, y);
            Poly(C(Ramp(p), a), x, y, x + cell, y, x + cell / 2f, y + h);
            Poly(C(Ramp(1f - p), a * .55f), x + cell, y, x + cell * 1.5f, y + h, x + cell / 2f, y + h);
        });
    }

    void Hexagons()
    {
        float rr = cell * .62f;
        Grid(rr * 1.732f, rr * 1.5f, true, Mod(mv, rr * 1.732f), 0f, (x, y, r, q) =>
        {
            float p = .5f + .5f * MathF.Sin(q * .55f - r * .8f + t * 1.4f);
            Fan(x, y, rr * (.52f + p * .42f), 6, MathF.PI / 6f, C(Ramp(p), (.3f + p * .52f) * Calm(x, y)));
        });
    }

    void Diamonds()
    {
        Grid(cell, cell * .62f, true, Mod(mv, cell), 0f, (x, y, r, q) =>
        {
            float p = .5f + .5f * MathF.Sin(r * .77f + q * .35f - t * 1.5f), a = cell * (.2f + p * .3f), b = cell * .31f * (.5f + p * .5f);
            Poly(C(Ramp(p), (.34f + p * .5f) * Calm(x, y)), x, y - b, x + a, y, x, y + b, x - a, y);
        });
    }

    void Halftone()
    {
        float g = MathF.Max(9f, cell * .34f);
        Grid(g, g * .866f, true, Mod(mv * .4f, g), 0f, (x, y, r, q) =>
        {
            float d = MathF.Sqrt(x * x + y * y) / MathF.Max(ex, ey), p = Math.Clamp(.5f + .5f * MathF.Sin(d * 8f - t * 1.5f) + (d - .65f) * .8f, 0f, 1f);
            if (p > .05f) Dot(x, y, g * .56f * p, C(Ramp(d), .82f));
        });
    }

    // ---------------------------------------------------------------- 風景
    void Clouds()
    {
        for (int k = 0; k < 4; k++)
        {
            float span = ex * 2f + 620f * u, y0 = -ey * .68f + k * ey * .52f; var col = Vector3.Lerp(k % 2 == 0 ? s.Pal.A : s.Pal.B, Vector3.One, .34f + k * .1f);
            for (int i = 0; i < 6; i++)
            {
                float x = Mod(R(i + k * 13, 0) * span + (mv + t * 16f * u) * (.42f + k * .3f), span) - ex - 310f * u, y = y0 + (R(i + k * 13, 1) - .5f) * 92f * u, z = (.7f + k * .22f + R(i, 2) * .34f) * u * s.Size, a = .24f + k * .08f;
                for (int j = 0; j < 7; j++) Dot(x + (j - 3) * 56f * z, y + MathF.Abs(j - 3) * 11f * z, (78f - MathF.Abs(j - 3) * 13f) * z, C(col, a));
            }
        }
    }

    void Hills()
    {
        for (int k = 0; k < 6; k++)
        {
            float q0 = k / 5f, base0 = -ey * .22f + q0 * ey * 1.18f, amp = (112f - k * 8f) * u, sp = (mv + t * 8f * u) * (.2f + q0 * .9f), st = MathF.Max(8f, 14f * u);
            var pts = new List<Vector2>((int)(ex * 2f / st) + 5) { new(-ex, ey + 4f) };
            for (float x = -ex; x <= ex + st; x += st) pts.Add(new(x, base0 + MathF.Sin((x + sp) / (278f * u) + k * 1.8f) * amp + MathF.Sin((x + sp) / (97f * u) + k) * amp * .24f));
            pts.Add(new(ex + st, ey + 4f));
            br.Color = C(Vector3.Lerp(Ramp(q0), s.Pal.Back1, q0 * .72f), 1f); Gfx.FillPoly(dc, br, pts);
        }
        Dot(ex * .36f, -ey * .42f, 118f * u, C(s.Pal.C, .9f));
    }

    void Skyline()
    {
        for (int k = 0; k < 3; k++)
        {
            float w0 = (58f + k * 20f) * u, span = ex * 2f + w0 * 6f, sp = mv * (.26f + k * .42f), floor0 = ey; int n = (int)(span / w0) + 2, off = (int)MathF.Floor(sp / w0);
            var col = Vector3.Lerp(Vector3.Lerp(s.Pal.A, s.Pal.Back1, .35f), s.Pal.Back1, k * .32f + .28f);
            for (int i = 0; i < n; i++)
            {
                int id = i - off + k * 211; float x = -ex - w0 * 2f + i * w0 + Mod(sp, w0), h = (120f + R(id, 1) * 300f + k * 92f) * u * MathF.Max(.5f, s.Size);
                Rect(x, floor0 - h, x + w0 - 2f * u, floor0, C(col, 1f));
                for (float wy = floor0 - h + 12f * u; wy < floor0 - 12f * u; wy += 22f * u)
                    for (float wx = x + 8f * u; wx < x + w0 - 13f * u; wx += 16f * u)
                        if (R((int)(wx * 3f + wy * 17f) + id, 3) > .62f) Rect(wx, wy, wx + 6f * u, wy + 9f * u, C(k == 2 ? s.Pal.C : s.Pal.B, .62f + .28f * MathF.Sin(t + id)));
            }
        }
    }

    // ---------------------------------------------------------------- 飾り
    void Petals()
    {
        int n = Math.Clamp(s.N * 2, 16, 500); float h = ey * 2f + 120f * u;
        for (int i = 0; i < n; i++)
        {
            float y = -ey - 60f * u + Mod(R(i, 1) * h + (mv * .5f + t * 72f * u) * (.5f + R(i, 2)), h), x = (R(i, 0) - .5f) * ex * 2.15f + MathF.Sin(t * .9f + i) * 44f * u, z = (8f + R(i, 3) * 17f) * u * s.Size, a = t * (.7f + R(i, 4)) + i, fl = .3f + .7f * MathF.Abs(MathF.Sin(t * 1.5f + i)), cs = MathF.Cos(a), sn = MathF.Sin(a);
            Vector2 P(float px, float py) => new(x + (px * cs - py * fl * sn) * z, y + (px * sn + py * fl * cs) * z);
            var q = new List<Vector2> { P(-1.2f, 0), P(-.55f, -.58f), P(.45f, -.56f), P(1.2f, -.12f), P(.84f, 0), P(1.2f, .12f), P(.45f, .56f), P(-.55f, .58f) };
            br.Color = C(Vector3.Lerp(Pick(i), Vector3.One, .22f), .84f * Calm(x, y)); Gfx.FillPoly(dc, br, q);
        }
    }

    void Confetti()
    {
        int n = Math.Clamp(s.N * 3, 24, 700); float h = ey * 2f + 80f * u;
        for (int i = 0; i < n; i++)
        {
            float y = -ey - 40f * u + Mod(R(i, 1) * h + (mv * .5f + t * 118f * u) * (.5f + R(i, 2)), h), x = (R(i, 0) - .5f) * ex * 2.1f + MathF.Sin(t * 1.5f + i) * 24f * u, w = (6f + R(i, 3) * 9f) * u * s.Size, a = t * (1f + R(i, 4) * 2f) + i, l = w * MathF.Abs(MathF.Cos(t * 3f + i)) + w * .16f, cs = MathF.Cos(a), sn = MathF.Sin(a);
            Poly(C(Pick(i), .92f * Calm(x, y)), x + cs * w - sn * l, y + sn * w + cs * l, x - cs * w - sn * l, y - sn * w + cs * l, x - cs * w + sn * l, y - sn * w - cs * l, x + cs * w + sn * l, y + sn * w - cs * l);
        }
    }

    void Memphis()
    {
        int n = Math.Clamp(s.N, 12, 240);
        for (int i = 0; i < n; i++)
        {
            float x = (Fr(R(i, 0) + mv * .00011f / u) - .5f) * ex * 2.2f, y = (R(i, 1) - .5f) * ey * 2f + MathF.Sin(t * .8f + i) * 14f * u, z = (15f + R(i, 2) * 39f) * u * s.Size, a = t * (R(i, 3) - .5f) * 1.6f + i;
            var c = C(Pick(i), .9f * Calm(x, y));
            switch (i % 6)
            {
                case 0: Dot(x, y, z, c); break;
                case 1: Ring(x, y, z, c, z * .3f); break;
                case 2: Fan(x, y, z * 1.2f, 3, a, c); break;
                case 3: Fan(x, y, z * 1.25f, 4, a, c); break;
                case 4: { float cs = MathF.Cos(a) * z, sn = MathF.Sin(a) * z; Line(x - cs, y - sn, x + cs, y + sn, c, z * .3f); Line(x + sn, y - cs, x - sn, y + cs, c, z * .3f); break; }
                default:
                    {
                        var q = new List<Vector2>(13);
                        for (int k = 0; k <= 12; k++) { float d = (k - 6) * z * .34f; q.Add(new(x + MathF.Cos(a) * d - MathF.Sin(a) * MathF.Sin(k * 1.57f) * z * .34f, y + MathF.Sin(a) * d + MathF.Cos(a) * MathF.Sin(k * 1.57f) * z * .34f)); }
                        Path(q, c, z * .2f);
                        break;
                    }
            }
        }
    }

    void ArcRings()
    {
        int n = Math.Clamp(s.N / 5, 5, 28); float m = MathF.Sqrt(ex * ex + ey * ey), gap = m / (n + 1), c0 = MathF.Max(0f, s.Clear - .35f) * 180f * u;
        for (int i = 0; i < n; i++)
        {
            float r = gap * (i + 1) + c0, dir = (i & 1) == 0 ? 1f : -1f, a = t * dir * (.32f + R(i, 1) * .5f) + R(i, 2) * MathF.Tau; int arcs = 1 + i % 3;
            for (int k = 0; k < arcs; k++)
            {
                float a0 = a + k * MathF.Tau / arcs, len = (.5f + R(i + k * 20, 3) * 1.1f) * MathF.PI / arcs * 1.4f;
                Path(Gfx.ArcPts(Vector2.Zero, r, a0, a0 + len, Math.Max(10, (int)(len * r / (9f * u)))), C(Ramp(i / (n - 1f)), .76f), MathF.Max(1.5f, gap * (.14f + R(i, 4) * .22f)));
            }
        }
    }

    void Glitch()
    {
        int tick = (int)MathF.Floor(s.T * 11f * MathF.Max(.2f, MathF.Abs(s.Spin))), n = Math.Clamp(s.N / 2, 10, 90);
        for (int i = 0; i < n; i++)
        {
            int id = i * 7 + tick * 31; float y = (R(id, 0) - .5f) * ey * 2f, h = (3f + R(id, 1) * R(id, 1) * 74f) * u, x = (R(id, 2) - .5f) * ex * 2f, w = (60f + R(id, 3) * 760f) * u;
            Rect(x - w / 2f, y, x + w / 2f, y + h, C(Pick(id), (.24f + R(id, 4) * .62f) * Calm(x, y)));
            if (i % 4 == 0) { Rect(x - w / 2f + 12f * u, y + h, x + w / 2f + 12f * u, y + h + 2f * u, C(s.Pal.A, .78f)); Rect(x - w / 2f - 12f * u, y - 2f * u, x + w / 2f - 12f * u, y, C(s.Pal.B, .78f)); }
        }
    }

    // ---------------------------------------------------------------- v121 で足した背景
    float SpinT => s.T * MathF.Max(.2f, MathF.Abs(s.Spin));

    void Stars()
    {
        int n = Math.Clamp(s.N * 3, 40, 900);
        for (int i = 0; i < n; i++)
        {
            float x = (Fr(R(i, 0) + mv * .00005f / u * (.3f + R(i, 3))) - .5f) * ex * 2f, y = (R(i, 1) - .5f) * ey * 2f, tw = .35f + .65f * MathF.Abs(MathF.Sin(t * (1f + R(i, 4) * 2f) + i));
            float r = (1.2f + R(i, 2) * R(i, 2) * 3.6f) * u * s.Size;
            Dot(x, y, r * 2.6f, C(Pick(i), .18f * tw)); Dot(x, y, r, C(Vector3.Lerp(Pick(i), Vector3.One, .7f), tw));
            if (R(i, 5) > .94f)
            {
                Line(x - r * 6f, y, x + r * 6f, y, C(Vector3.One, .6f * tw), MathF.Max(1f, u));
                Line(x, y - r * 6f, x, y + r * 6f, C(Vector3.One, .6f * tw), MathF.Max(1f, u));
            }
        }
    }

    void Bubbles()
    {
        int n = Math.Clamp(s.N, 12, 260); float h = ey * 2f + 200f * u;
        for (int i = 0; i < n; i++)
        {
            float r = (10f + R(i, 2) * R(i, 2) * 60f) * u * s.Size, y = ey + 100f * u - Mod(R(i, 1) * h + (mv * .5f + t * 60f * u) * (.4f + R(i, 3)), h), x = (R(i, 0) - .5f) * ex * 2.1f + MathF.Sin(t * 1.2f + i) * 20f * u;
            float a = .7f * Calm(x, y);
            Dot(x, y, r, C(Pick(i), a * .15f));
            Ring(x, y, r, C(Vector3.Lerp(Pick(i), Vector3.One, .4f), a * .8f), MathF.Max(1.2f, r * .08f));
            Dot(x - r * .38f, y - r * .38f, r * .2f, C(Vector3.One, a * .8f));
        }
    }

    void Snow()
    {
        int n = Math.Clamp(s.N * 3, 30, 800); float h = ey * 2f + 60f * u;
        for (int i = 0; i < n; i++)
        {
            float y = -ey - 30f * u + Mod(R(i, 1) * h + (mv * .4f + t * 46f * u) * (.4f + R(i, 2)), h), x = (R(i, 0) - .5f) * ex * 2.1f + MathF.Sin(t * .8f + i) * 30f * u, r = (2f + R(i, 3) * R(i, 3) * 7f) * u * s.Size;
            var c = C(Vector3.Lerp(Pick(i), Vector3.One, .75f), (.55f + R(i, 4) * .45f) * MathF.Max(.6f, Calm(x, y)));
            if (R(i, 5) > .93f)
            {
                float rr = r * 3.2f, a0 = t * .5f + i;
                for (int k = 0; k < 3; k++) { float a = a0 + k * MathF.PI / 3f; Line(x - MathF.Cos(a) * rr, y - MathF.Sin(a) * rr, x + MathF.Cos(a) * rr, y + MathF.Sin(a) * rr, c, MathF.Max(1.2f, r * .35f)); }
            }
            else Dot(x, y, r, c);
        }
    }

    void Fireworks()
    {
        int n = Math.Clamp(s.N / 10, 3, 14);
        for (int i = 0; i < n; i++)
        {
            float per = 2.4f + R(i, 0) * 1.6f, ph = Fr((SpinT + R(i, 1) * per) / per); int cyc = (int)MathF.Floor((SpinT + R(i, 1) * per) / per), id = i * 37 + cyc * 101;
            float cx = (R(id, 2) - .5f) * ex * 1.7f, cy = (R(id, 3) - .5f) * ey * 1.2f - ey * .15f, rad = (120f + R(id, 4) * 160f) * u * s.Size;
            float k = 1f - (1f - ph) * (1f - ph) * (1f - ph), fade = 1f - ph, drop = ph * ph * 60f * u;
            int sparks = 24 + (int)(R(id, 5) * 24); var col = Pick(id);
            for (int j = 0; j < sparks; j++)
            {
                float a = j * MathF.Tau / sparks, r1 = rad * k, r0 = rad * MathF.Max(0f, k - .18f);
                Line(cx + MathF.Cos(a) * r0, cy + MathF.Sin(a) * r0 + drop, cx + MathF.Cos(a) * r1, cy + MathF.Sin(a) * r1 + drop, C(Vector3.Lerp(col, Vector3.One, .3f), fade), MathF.Max(1.4f, 3.4f * u));
                Dot(cx + MathF.Cos(a) * r1, cy + MathF.Sin(a) * r1 + drop, 3.4f * u, C(Vector3.One, fade));
            }
            Dot(cx, cy, rad * .35f * (1f - ph), C(col, .25f * fade));
        }
    }

    void Plaid()
    {
        float g = cell * 2.2f, ox = Mod(mv, g), oy = Mod(mv * .5f, g), sh = .08f * MathF.Sin(t);
        for (float x = -ex - g + ox; x <= ex + g; x += g) { Rect(x, -ey, x + g * .42f, ey, C(s.Pal.A, .32f + sh)); Rect(x + g * .62f, -ey, x + g * .7f, ey, C(s.Pal.C, .5f)); }
        for (float y = -ey - g + oy; y <= ey + g; y += g) { Rect(-ex, y, ex, y + g * .42f, C(s.Pal.B, .32f - sh)); Rect(-ex, y + g * .62f, ex, y + g * .7f, C(s.Pal.C, .5f)); }
    }

    void Zigzag()
    {
        float gap = cell * .9f, w = cell * .7f; int n = (int)(ey * 2f / gap) + 3; float off = Mod(mv * .5f + t * 20f * u, gap);
        for (int k = -1; k < n; k++)
        {
            float y0 = -ey + k * gap + off, ph = Mod(mv + t * 30f * u * (k % 2 == 0 ? 1f : -1f), w * 2f); var q = new List<Vector2>();
            int i = 0;
            for (float x = -ex - w * 2f + ph; x <= ex + w * 2f; x += w, i++) q.Add(new(x, y0 + (i & 1) * gap * .45f));
            Path(q, C(Ramp(Fr(k * .23f)), .85f), MathF.Max(2f, gap * .16f));
        }
    }

    static Vector2 HeartPt(float b) => new(MathF.Pow(MathF.Sin(b), 3), -(13f * MathF.Cos(b) - 5f * MathF.Cos(2 * b) - 2f * MathF.Cos(3 * b) - MathF.Cos(4 * b)) / 16f);

    void Hearts()
    {
        int n = Math.Clamp(s.N, 10, 200); float h = ey * 2f + 160f * u;
        for (int i = 0; i < n; i++)
        {
            float y = ey + 80f * u - Mod(R(i, 1) * h + (mv * .5f + t * 50f * u) * (.4f + R(i, 2)), h), x = (R(i, 0) - .5f) * ex * 2.1f + MathF.Sin(t + i) * 26f * u, z = (12f + R(i, 3) * 30f) * u * s.Size, a = MathF.Sin(t * 1.3f + i) * .4f;
            var q = new List<Vector2>(24);
            for (int k = 0; k < 24; k++) { var hp = HeartPt(k * MathF.Tau / 24f); q.Add(new(x + (hp.X * MathF.Cos(a) - hp.Y * MathF.Sin(a)) * z, y + (hp.X * MathF.Sin(a) + hp.Y * MathF.Cos(a)) * z)); }
            br.Color = C(Pick(i), .8f * Calm(x, y)); Gfx.FillPoly(dc, br, q);
        }
    }

    void Sparkles()
    {
        int n = Math.Clamp(s.N, 12, 300);
        for (int i = 0; i < n; i++)
        {
            float x = (Fr(R(i, 0) + mv * .0001f / u) - .5f) * ex * 2.1f, y = (R(i, 1) - .5f) * ey * 2f, tw = .3f + .7f * MathF.Abs(MathF.Sin(t * (1.2f + R(i, 2) * 2f) + i * 2.3f));
            float z = (16f + R(i, 3) * 44f) * u * s.Size * (.3f + tw * .7f), a = t * .4f + i;
            var q = new List<Vector2>(8);
            for (int k = 0; k < 8; k++) { float b = a + k * MathF.PI / 4f, r = k % 2 == 0 ? z : z * .22f; q.Add(new(x + MathF.Cos(b) * r, y + MathF.Sin(b) * r)); }
            Dot(x, y, z * .5f, C(Pick(i), tw * .15f));
            br.Color = C(Vector3.Lerp(Pick(i), Vector3.One, .5f), tw * MathF.Max(.5f, Calm(x, y))); Gfx.FillPoly(dc, br, q);
        }
    }

    void Ribbons()
    {
        int n = Math.Clamp(s.N / 12, 3, 10); float st = MathF.Max(8f, 16f * u);
        for (int k = 0; k < n; k++)
        {
            float y0 = -ey * .8f + k * ey * 1.6f / MathF.Max(1, n - 1), th = (26f + R(k, 1) * 40f) * u * s.Size; var top = new List<Vector2>(); var bot = new List<Vector2>();
            for (float x = -ex; x <= ex + st; x += st)
            {
                float y = y0 + MathF.Sin(x / (260f * u) + t * .9f + k * 1.3f) * 90f * u + MathF.Sin((x + mv) / (120f * u) - t * .6f + k) * 24f * u, w = th * (.6f + .4f * MathF.Sin(x / (180f * u) + t + k));
                top.Add(new(x, y - w)); bot.Add(new(x, y + w));
            }
            bot.Reverse(); top.AddRange(bot);
            br.Color = C(Ramp(k / MathF.Max(1f, n - 1f)), .7f); Gfx.FillPoly(dc, br, top);
        }
    }

    void Rays()
    {
        int n = Math.Clamp(s.N / 6, 5, 30); float l = MathF.Sqrt(ex * ex + ey * ey) * 2.4f, ox = -ex * 1.1f, oy = -ey * 1.1f;
        for (int i = 0; i < n; i++)
        {
            float a = .15f + (i + .5f) / n * 1.25f + MathF.Sin(t * .5f + i * 1.7f) * .04f, w = .012f + R(i, 1) * .03f, al = .1f + .16f * (.5f + .5f * MathF.Sin(t * 1.1f + i * 2.1f));
            Poly(C(Ramp(i / (float)n), al), ox, oy, ox + MathF.Cos(a - w) * l, oy + MathF.Sin(a - w) * l, ox + MathF.Cos(a + w) * l, oy + MathF.Sin(a + w) * l);
        }
        Blob(ox + ex * .1f, oy + ey * .1f, 260f * u, Vector3.One, .5f);
    }

    // ---------------------------------------------------------------- v122 白黒の図形(紙にインクで描いたような図形の組み合わせ)
    /// <summary>拍ごとに1つ進む数(回る速さで変わる)</summary>
    int Step => (int)MathF.Floor(s.T * 2f * MathF.Max(.2f, MathF.Abs(s.Spin)));

    void MonoGeo()
    {
        float hw = s.W / 2f, hh = s.H / 2f, lw = MathF.Max(1.2f, 2.4f * u); int st = Step;
        bool flip = (s.Seed & 1) != 0;
        float F(float x) => flip ? -x : x;
        var ink = C(s.Pal.A, .95f);
        void Ln(float x0, float y0, float x1, float y1, Color4 c, float w) => Line(F(x0), y0, F(x1), y1, c, w);

        // 紙のざらつき(1秒に12回変わる点とかすれ)
        int tick = (int)MathF.Floor(s.T * 12f);
        for (int i = 0; i < 700; i++)
        {
            int id = i * 7 + tick * 7919;
            Dot((R(id, 0) - .5f) * ex * 2f, (R(id, 1) - .5f) * ey * 2f, (.5f + R(id, 2) * 1.2f) * u, C(s.Pal.A, .12f + R(id, 3) * .3f));
        }
        for (int i = 0; i < 16; i++)
        {
            int id = i * 13 + (tick / 3) * 104729; float x = (R(id, 0) - .5f) * ex * 2f, y = (R(id, 1) - .5f) * ey * 2f, a = (R(id, 2) - .5f) * .5f, l = (20f + R(id, 3) * 70f) * u;
            Line(x, y, x + MathF.Cos(a) * l, y + MathF.Sin(a) * l, C(s.Pal.A, .18f + R(id, 4) * .2f), (.6f + R(id, 5) * 1.6f) * u);
        }

        // × の並び(ななめの列ごとに + に変わる)
        for (int r = 0; r < 4; r++)
            for (int q = 0; q < 6; q++)
            {
                float x = -hw + (92f + q * 114f) * u, y = -hh + (98f + r * 116f) * u, z = 15f * u, a = (q + r + st) % 6 == 0 ? 0f : MathF.PI / 4f;
                float cs = MathF.Cos(a) * z, sn = MathF.Sin(a) * z;
                Ln(x - cs, y - sn, x + cs, y + sn, ink, lw); Ln(x + sn, y - cs, x - sn, y + cs, ink, lw);
            }

        // ななめ線入りの四角の列(上は拍ごとに左へずれる、下はその場)
        void Boxes(float y, float x0, int n, float shift, int seedRow)
        {
            float g = 160f * u, b = 112f * u;
            for (int k = -1; k <= n; k++)
            {
                float x = x0 + k * g - shift;
                if (x < x0 - 1f || x > hw + 40f * u) continue;
                int id = k + (int)MathF.Floor(shift / g) + seedRow;
                Ln(x, y, x + b, y, ink, lw); Ln(x + b, y, x + b, y + b, ink, lw); Ln(x + b, y + b, x, y + b, ink, lw); Ln(x, y + b, x, y, ink, lw);
                if ((id + st) % 7 == 0) Poly(ink, F(x), y, F(x + b), y + b, F(x), y + b);
                else if ((id + st) % 5 == 0) Ln(x + b, y, x, y + b, ink, lw);
                else Ln(x, y, x + b, y + b, ink, lw);
            }
        }
        float scroll = Mod(st * 40f * u + mv, 160f * u * 64f);
        Boxes(-hh + 220f * u, -hw + 792f * u, 9, scroll, 0);
        Boxes(hh - 262f * u, -hw + 68f * u, 8, 0f, 31);

        // ななめ線の平行四辺形(線が1本ずつ伸びて、そろうとまた消える)
        {
            float x0 = -hw + 90f * u, x1 = -hw + 600f * u, top = -hh + 510f * u, bot = -hh + 672f * u, sk = (bot - top) * .55f, gap = 11f * u;
            int n = (int)((x1 - x0) / gap), ph = st % (n / 4 + 8), vis = Math.Min(n, ph * 4);
            for (int i = 0; i < vis; i++) { float x = x0 + i * gap; Ln(x, bot, x + sk, top, C(s.Pal.A, .85f), MathF.Max(1f, 1.5f * u)); }
        }

        // 下の太いななめ線の帯(流れる)
        {
            float top = hh - 105f * u, g = 46f * u, o = Mod(mv + s.T * 60f * u * MathF.Max(.2f, MathF.Abs(s.Spin)), g);
            for (float x = -ex - g * 3f + o; x < ex + g; x += g) Line(x, ey + 4f, x + (ey + 4f - top), top, ink, MathF.Max(1.4f, 3f * u));
        }

        // 目盛りの輪(拍ごとに少し回る。まばらに欠ける)
        {
            float cx = hw - 528f * u, cy = hh - 300f * u, r = 140f * u, rot = st * MathF.PI / 18f;
            for (int i = 0; i < 36; i++)
            {
                if (R(i + st * 3, 7) < .1f) continue;
                float a = rot + i * MathF.Tau / 36f, c = MathF.Cos(a), sn = MathF.Sin(a);
                Ln(cx + c * r * .8f, cy + sn * r * .8f, cx + c * r * 1.08f, cy + sn * r * 1.08f, ink, MathF.Max(1f, 1.8f * u));
                Dot(F(cx + c * r * .94f), cy + sn * r * .94f, 3.4f * u, ink);
            }
        }

        // 黒い三角の積み重ね(拍ごとに1つずつ増える)
        {
            float x = hw - 340f * u, y0 = hh - 520f * u, w = 90f * u, h = 92f * u; int vis = 1 + st % 5;
            for (int i = 0; i < Math.Min(4, vis); i++) { float y = y0 + i * h * 1.02f; Poly(ink, F(x), y, F(x), y + h, F(x + w), y + h); }
        }
    }
}
