using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using WpfColor = System.Windows.Media.Color;

namespace YMM4EffectShapes;

// =====================================================================
//  画面転換（追加の21種類）
//   cov: 0=覆いなし → 1=画面全体を「色」で覆う。「覆いを開く」はこの逆再生
//   画面の外にはみ出す形は、画面の四角で切り取ってから塗る（位置ずれ防止）
// =====================================================================
public sealed partial class TransitionParameter
{
    static void FillClip(ID2D1DeviceContext dc, ID2D1Brush brush, IList<Vector2> poly, float w, float h)
    {
        var cp = DrawKit.ClipRect(poly, 0f, 0f, w, h);
        if (cp.Count >= 3) Gfx.FillPoly(dc, brush, cp);
    }

    static List<Vector2> Circle(Vector2 c, float r, int n = 72) => DrawKit.EllipsePts(c, r, r, n);

    /// <summary>ふちがでこぼこした円（インク・煙・爆炎）</summary>
    static List<Vector2> Blob(Vector2 c, float r, float rough, int seed, float t, int n = 72)
    {
        var pts = new List<Vector2>(n);
        for (int i = 0; i < n; i++)
        {
            float a = i * MathF.PI * 2f / n;
            float k = 1f + rough * (0.55f * MathF.Sin(a * 3f + seed) + 0.3f * MathF.Sin(a * 7f + seed * 1.7f + t) + 0.15f * MathF.Sin(a * 13f + seed * 2.3f));
            pts.Add(c + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r * k);
        }
        return pts;
    }

    static float Seg(float cov, float a, float b) => Math.Clamp((cov - a) / MathF.Max(1e-4f, b - a), 0f, 1f);

    void DrawMore(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, float w, float h, float t, float cov, int n, float soft)
    {
        var ctr = new Vector2(w / 2f, h / 2f);
        float maxR = MathF.Sqrt(w * w + h * h) / 2f * 1.02f;
        var white = WpfColor.FromRgb(255, 255, 255);
        var prev = dc.Transform;
        try
        {
            switch (Kind)
            {
                // ---------------------------------------------------------------- カーテン
                case TransKind.Curtain:
                    {
                        float half = w / 2f * cov + (cov >= 0.999f ? 2f : 0f);
                        var dark = Gfx.Mix(Color, WpfColor.FromRgb(0, 0, 0), 0.55f);
                        var lite = Gfx.Mix(Color, white, 0.25f);
                        for (int side = 0; side < 2; side++)
                        {
                            float x0 = side == 0 ? 0f : w - half;
                            int strips = Math.Max(8, (int)(half / 6f));
                            float sw = half / strips;
                            int folds = 7;
                            for (int i = 0; i < strips; i++)
                            {
                                float u = (i + 0.5f) / strips;
                                float shade = 0.5f + 0.5f * MathF.Cos(u * folds * MathF.PI * 2f + (side == 0 ? 0f : 1f));
                                brush.Color = Gfx.C4(Gfx.Mix(dark, lite, shade), 1f);
                                // すそが少しふくらむ
                                float hem = h + 6f * MathF.Sin(u * folds * MathF.PI * 2f) * (1f - cov * 0.5f);
                                Gfx.Rect(dc, brush, x0 + i * sw - 0.5f, 0, sw + 1f, MathF.Min(h, hem));
                            }
                            // 内側のふちの影
                            if (half > 4f && cov < 0.999f)
                            {
                                float ex = side == 0 ? half : w - half;
                                using var g = Gfx.Linear(dc, new Vector2(ex, 0), new Vector2(ex + (side == 0 ? 30f : -30f), 0), (0f, Gfx.C4(0f, 0f, 0f, 0.35f)), (1f, Gfx.C4(0f, 0f, 0f, 0f)));
                                dc.FillRectangle(side == 0 ? new RawRectF(ex, 0, MathF.Min(w, ex + 30f), h) : new RawRectF(MathF.Max(0f, ex - 30f), 0, ex, h), g);
                            }
                        }
                        // 上の飾り幕（バランス）とふさ
                        float vh = h * 0.11f * Gfx.Smooth(0f, 0.25f, cov);
                        if (vh > 1f)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                float x = w * (i + 0.5f) / 12f;
                                var swag = DrawKit.EllipsePts(new Vector2(x, 0), w / 24f + 2f, vh, 24, 0f, MathF.PI);
                                swag.Add(new Vector2(x - w / 24f - 2f, 0));
                                brush.Color = Gfx.C4(Gfx.Mix(Color, WpfColor.FromRgb(0, 0, 0), 0.25f), 1f);
                                FillClip(dc, brush, swag, w, h);
                            }
                            brush.Color = Gfx.C4(Color2, 1f);
                            dc.FillRectangle(new RawRectF(0, vh * 0.15f, w, vh * 0.25f), brush);
                            for (float x = 4f; x < w; x += 9f)
                                dc.DrawLine(new Vector2(x, vh * 0.25f), new Vector2(x, vh * 0.42f), brush, 2f);
                        }
                        break;
                    }

                // ---------------------------------------------------------------- 本のページめくり
                case TransKind.PageTurn:
                    {
                        float fw = MathF.Min(w * 0.22f, 360f) * MathF.Sin(MathF.PI * Math.Clamp(cov, 0f, 1f)) + 1f;
                        float tilt = h * 0.18f;
                        float xe = w + fw + tilt - cov * (w + 2f * fw + 2f * tilt);
                        float xt = xe + tilt, xb = xe - tilt;          // 折り目の上端・下端
                        // めくれた後ろの新しいページ
                        FillClip(dc, brush, new List<Vector2> { new(xt, 0), new(w + 5f, 0), new(w + 5f, h), new(xb, h) }, w, h);
                        // めくれている紙（裏面）
                        var flap = new List<Vector2> { new(xt, 0), new(xb, h), new(xb - fw * 0.8f, h), new(xt - fw, 0) };
                        var paper = Gfx.Mix(Color2, white, 0.75f);
                        using (var g = Gfx.Linear(dc, new Vector2(xe - fw, h / 2f), new Vector2(xe, h / 2f),
                            (0f, Gfx.C4(paper, 1f)), (0.7f, Gfx.C4(Gfx.Mix(paper, WpfColor.FromRgb(150, 140, 120), 0.3f), 1f)), (1f, Gfx.C4(Gfx.Mix(paper, WpfColor.FromRgb(80, 70, 60), 0.5f), 1f))))
                            FillClip(dc, g, flap, w, h);
                        // 紙の落とす影
                        var shadow = new List<Vector2> { new(xt - fw, 0), new(xb - fw * 0.8f, h), new(xb - fw * 0.8f - 40f, h), new(xt - fw - 40f, 0) };
                        using (var g = Gfx.Linear(dc, new Vector2(xe - fw, 0), new Vector2(xe - fw - 40f, 0), (0f, Gfx.C4(0f, 0f, 0f, 0.3f)), (1f, Gfx.C4(0f, 0f, 0f, 0f))))
                            FillClip(dc, g, shadow, w, h);
                        break;
                    }

                // ---------------------------------------------------------------- 吹き出し
                case TransKind.Bubble:
                    {
                        float e = DrawKit.EaseOutBack(cov);
                        float r = maxR * 1.25f * e;
                        if (r < 1f) break;
                        var body = DrawKit.EllipsePts(ctr, r * 1.1f, r * 0.85f, 96);
                        var tail = new List<Vector2> { ctr + new Vector2(-r * 0.35f, r * 0.6f), ctr + new Vector2(-r * 0.05f, r * 0.75f), ctr + new Vector2(-r * 0.75f, r * 1.25f) };
                        brush.Color = Gfx.C4(Color2, 1f);
                        FillClip(dc, brush, DrawKit.EllipsePts(ctr, r * 1.1f + 10f, r * 0.85f + 10f, 96), w, h);
                        FillClip(dc, brush, DrawKit.Place(tail, Vector2.Zero, 0f).Select(p => p + (p - ctr) * 0.04f).ToList(), w, h);
                        brush.Color = Gfx.C4(Color, 1f);
                        FillClip(dc, brush, body, w, h);
                        FillClip(dc, brush, tail, w, h);
                        // パンッという勢いの線
                        if (cov < 0.5f)
                        {
                            brush.Color = Gfx.C4(Color2, 1f - cov * 2f);
                            for (int i = 0; i < 16; i++)
                            {
                                float a = i * MathF.PI * 2f / 16f;
                                var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                                dc.DrawLine(ctr + d * (r * 1.2f + 10f), ctr + d * (r * 1.2f + 40f + cov * 120f), brush, 6f);
                            }
                        }
                        if (cov >= 0.999f) { brush.Color = Gfx.C4(Color, 1f); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }
                        break;
                    }

                // ---------------------------------------------------------------- スタンプ
                case TransKind.Stamp:
                    {
                        float R0 = MathF.Min(w, h) * 0.3f;
                        float drop = Seg(cov, 0f, 0.35f);
                        float scale = 2.6f - 1.6f * DrawKit.EaseOut(drop);
                        float shake = drop >= 1f ? MathF.Exp(-(cov - 0.35f) * 25f) * 14f : 0f;
                        var sc = ctr + new Vector2(MathF.Sin(t * 80f) * shake, MathF.Cos(t * 70f) * shake);
                        float spread = Seg(cov, 0.35f, 1f);
                        // インクが広がって画面を覆う
                        if (spread > 0f)
                        {
                            float rr = R0 * 1.05f + (maxR * 1.35f - R0) * DrawKit.EaseOut(spread);
                            brush.Color = Gfx.C4(Color, 1f);
                            FillClip(dc, brush, Blob(sc, rr, 0.12f * (1f - spread) + 0.05f, Seed, 0f), w, h);
                        }
                        // スタンプの面
                        float R = R0 * scale;
                        float alpha = Math.Clamp(drop * 2f, 0f, 1f) * (1f - Seg(cov, 0.6f, 0.9f));
                        if (alpha > 0.01f)
                        {
                            brush.Color = Gfx.C4(Color2, alpha);
                            dc.DrawEllipse(new Ellipse(sc, R, R), brush, R * 0.08f);
                            dc.DrawEllipse(new Ellipse(sc, R * 0.82f, R * 0.82f), brush, R * 0.03f);
                            Gfx.FillPoly(dc, brush, ShapeGeom.StarPoly(sc, R * 0.55f, 5, 0.45f, 0f));
                            for (int i = 0; i < 2; i++)
                            {
                                float yy = sc.Y + (i == 0 ? -R * 0.68f : R * 0.68f);
                                for (int k = -2; k <= 2; k++)
                                    dc.FillEllipse(new Ellipse(new Vector2(sc.X + k * R * 0.14f, yy), R * 0.035f, R * 0.035f), brush);
                            }
                        }
                        break;
                    }

                // ---------------------------------------------------------------- インク
                case TransKind.Ink:
                    {
                        brush.Color = Gfx.C4(Color, 1f);
                        var centers = new[] { ctr, new Vector2(w * 0.2f, h * 0.3f), new Vector2(w * 0.8f, h * 0.7f), new Vector2(w * 0.75f, h * 0.2f), new Vector2(w * 0.25f, h * 0.8f) };
                        float[] delay = { 0f, 0.08f, 0.15f, 0.22f, 0.3f };
                        for (int i = 0; i < centers.Length; i++)
                        {
                            float k = Seg(cov, delay[i], i == 0 ? 1f : 0.9f);
                            if (k <= 0f) continue;
                            float rr = (i == 0 ? maxR * 1.35f : maxR * 0.6f) * DrawKit.EaseOut(k);
                            FillClip(dc, brush, Blob(centers[i], rr, 0.18f, Seed + i * 7, 0f, 90), w, h);
                            // 飛び散るしずく
                            for (int j = 0; j < 14; j++)
                            {
                                float a = DrawKit.H(Seed, i * 31 + j, 1) * MathF.PI * 2f;
                                float dist = rr * (1.1f + 0.5f * DrawKit.H(Seed, i * 31 + j, 2));
                                float dr = rr * 0.05f * (0.5f + DrawKit.H(Seed, i * 31 + j, 3));
                                var dp = centers[i] + new Vector2(MathF.Cos(a), MathF.Sin(a)) * dist;
                                if (dp.X > -dr && dp.X < w + dr && dp.Y > -dr && dp.Y < h + dr)
                                    FillClip(dc, brush, Circle(dp, dr, 20), w, h);
                            }
                        }
                        break;
                    }

                // ---------------------------------------------------------------- 煙幕
                case TransKind.Smoke:
                    {
                        var rng = new Random(Seed);
                        int puffs = 46;
                        for (int i = 0; i < puffs; i++)
                        {
                            var p0 = new Vector2(rng.NextF() * w, h * (0.45f + 0.6f * rng.NextF()));
                            float d = rng.NextF() * 0.4f;
                            float k = Seg(cov, d, d + 0.6f);
                            if (k <= 0f) continue;
                            float rr = maxR * (0.25f + 0.25f * rng.NextF()) * (0.3f + 0.9f * DrawKit.EaseOut(k));
                            var p = p0 + new Vector2(MathF.Sin(t * 0.8f + i) * 30f, -k * h * 0.35f);
                            var col = Gfx.Mix(Color, white, 0.12f * rng.NextF());
                            using var g = Gfx.Radial(dc, p, rr, (0f, Gfx.C4(col, 0.95f * k)), (0.6f, Gfx.C4(col, 0.7f * k)), (1f, Gfx.C4(col, 0f)));
                            dc.FillRectangle(new RawRectF(MathF.Max(0f, p.X - rr), MathF.Max(0f, p.Y - rr), MathF.Min(w, p.X + rr), MathF.Min(h, p.Y + rr)), g);
                        }
                        float full = Gfx.Smooth(0.7f, 1f, cov);
                        if (full > 0f) { brush.Color = Gfx.C4(Color, full); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }
                        break;
                    }

                // ---------------------------------------------------------------- 爆発
                case TransKind.Explosion:
                    {
                        float e = DrawKit.EaseOut(Seg(cov, 0f, 0.75f));
                        float rr = maxR * 1.3f * e;
                        var fire = Color2;
                        var deep = Gfx.Mix(Color2, WpfColor.FromRgb(120, 10, 0), 0.6f);
                        if (rr > 1f)
                        {
                            dc.PrimitiveBlend = PrimitiveBlend.Add;
                            using (var g = Gfx.Radial(dc, ctr, rr * 1.2f, (0f, Gfx.C4(white, 1f)), (0.3f, Gfx.C4(Gfx.Mix(fire, white, 0.6f), 0.95f)), (0.65f, Gfx.C4(fire, 0.8f)), (1f, Gfx.C4(deep, 0f))))
                                dc.FillRectangle(new RawRectF(MathF.Max(0f, ctr.X - rr * 1.2f), MathF.Max(0f, ctr.Y - rr * 1.2f), MathF.Min(w, ctr.X + rr * 1.2f), MathF.Min(h, ctr.Y + rr * 1.2f)), g);
                            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                            brush.Color = Gfx.C4(Gfx.Mix(fire, white, 0.3f), 0.9f);
                            FillClip(dc, brush, Blob(ctr, rr * 0.75f, 0.15f, Seed, t * 3f), w, h);
                            brush.Color = Gfx.C4(white, 0.95f);
                            FillClip(dc, brush, Blob(ctr, rr * 0.45f, 0.12f, Seed + 3, t * 4f), w, h);
                            // 衝撃波
                            brush.Color = Gfx.C4(white, 0.7f * (1f - e));
                            dc.DrawEllipse(new Ellipse(ctr, rr * 1.05f, rr * 1.05f), brush, 12f * (1f - e) + 1f);
                        }
                        // 最初の閃光
                        if (cov < 0.15f) { brush.Color = Gfx.C4(white, (1f - cov / 0.15f) * 0.8f); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }
                        float full = Gfx.Smooth(0.6f, 1f, cov);
                        if (full > 0f) { brush.Color = Gfx.C4(Color, full); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }
                        break;
                    }

                // ---------------------------------------------------------------- 集中線ズーム
                case TransKind.SpeedLines:
                    {
                        int lines = 96;
                        float inner = maxR * MathF.Pow(1f - Math.Clamp(cov, 0f, 1f), 0.8f);
                        float outer = maxR * 1.05f;
                        brush.Color = Gfx.C4(Color, 1f);
                        for (int i = 0; i < lines; i++)
                        {
                            float a = (i + 0.5f * DrawKit.H(Seed, i, 1)) * MathF.PI * 2f / lines;
                            float wa = MathF.PI * 2f / lines * (0.12f + 0.25f * DrawKit.H(Seed, i, 2) + 0.9f * MathF.Pow(cov, 1.5f));
                            float rin = inner * (0.9f + 0.25f * DrawKit.H(Seed, i, 3));
                            var tip = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * rin;
                            var b1 = ctr + new Vector2(MathF.Cos(a - wa / 2f), MathF.Sin(a - wa / 2f)) * outer;
                            var b2 = ctr + new Vector2(MathF.Cos(a + wa / 2f), MathF.Sin(a + wa / 2f)) * outer;
                            FillClip(dc, brush, new List<Vector2> { tip, b1, b2 }, w, h);
                        }
                        float full = Gfx.Smooth(0.8f, 1f, cov);
                        if (full > 0f) { brush.Color = Gfx.C4(Color, full); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }
                        break;
                    }

                // ---------------------------------------------------------------- 斬撃
                case TransKind.Slash:
                    {
                        float ang = Gfx.Rad(-18f);
                        var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                        var nrm = new Vector2(-dir.Y, dir.X);
                        float L = maxR * 2.2f;
                        float e = DrawKit.EaseOut(Seg(cov, 0.1f, 0.85f));
                        var off = dir * (1f - e) * L;
                        var upper = new List<Vector2> { ctr - dir * L, ctr + dir * L, ctr + dir * L - nrm * L, ctr - dir * L - nrm * L };
                        var lower = new List<Vector2> { ctr - dir * L, ctr + dir * L, ctr + dir * L + nrm * L, ctr - dir * L + nrm * L };
                        brush.Color = Gfx.C4(Color, 1f);
                        if (e > 0f)
                        {
                            FillClip(dc, brush, upper.Select(p => p - off).ToList(), w, h);
                            FillClip(dc, brush, lower.Select(p => p + off).ToList(), w, h);
                        }
                        // 一閃（最初）と、閉じた時の光
                        float flash = cov < 0.15f ? cov / 0.15f : cov < 0.3f ? 1f - (cov - 0.15f) / 0.15f : Gfx.Smooth(0.75f, 0.9f, cov) * (1f - Gfx.Smooth(0.92f, 1f, cov));
                        if (flash > 0.01f)
                        {
                            dc.PrimitiveBlend = PrimitiveBlend.Add;
                            float reach = cov < 0.3f ? Math.Clamp(cov / 0.15f, 0f, 1f) : 1f;
                            var a0 = ctr - dir * L * 0.5f;
                            var a1 = Vector2.Lerp(a0, ctr + dir * L * 0.5f, reach);
                            brush.Color = Gfx.C4(Color2, 0.35f * flash);
                            dc.DrawLine(a0, a1, brush, 26f);
                            brush.Color = Gfx.C4(white, flash);
                            dc.DrawLine(a0, a1, brush, 5f);
                            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                        }
                        if (cov >= 0.999f) { brush.Color = Gfx.C4(Color, 1f); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }
                        break;
                    }

                // ---------------------------------------------------------------- 星・ハートのアイリス
                case TransKind.StarIris:
                case TransKind.HeartIris:
                    {
                        float k = 1f - cov;
                        var screen = DrawKit.RectPts(0, 0, w, h);
                        if (k <= 0.002f) { brush.Color = Gfx.C4(Color, 1f); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); break; }
                        List<Vector2> hole = Kind == TransKind.StarIris
                            ? ShapeGeom.StarPoly(ctr, maxR * 2.6f * k, 5, 0.48f, cov * 0.8f)
                            : ShapeGeom.HeartPoly(ctr + new Vector2(0, maxR * 0.1f * k), maxR * 1.9f * k, 0f);
                        var clipped = DrawKit.ClipRect(hole, 0f, 0f, w, h);
                        brush.Color = Gfx.C4(Color, 1f);
                        DrawKit.FillWithHole(dc, brush, screen, clipped);
                        break;
                    }

                // ---------------------------------------------------------------- ダイヤ
                case TransKind.Diamonds:
                    {
                        float cs = w / n;
                        int rows = (int)MathF.Ceiling(h / cs) + 1;
                        brush.Color = Gfx.C4(Color, 1f);
                        for (int r = 0; r <= rows; r++)
                            for (int ci = 0; ci <= n; ci++)
                            {
                                float d = (ci / (float)n * 0.75f + MathF.Sin(r * 0.7f) * 0.04f + 0.04f) * 0.6f;
                                float local = Math.Clamp((cov * 1.6f - d) / 0.4f, 0f, 1f);
                                if (local <= 0f) continue;
                                var p = new Vector2(ci * cs + (r % 2 == 1 ? cs / 2f : 0f), r * cs / 2f);
                                float s = (cs * 0.5f + 1f) * local;
                                FillClip(dc, brush, new List<Vector2> { p + new Vector2(0, -s), p + new Vector2(s * 1.0f, 0), p + new Vector2(0, s), p + new Vector2(-s * 1.0f, 0) }, w, h);
                            }
                        break;
                    }

                // ---------------------------------------------------------------- 六角形のタイル
                case TransKind.Hexagons:
                    {
                        float r = w / n * 0.62f;
                        float dx = r * 1.5f, dy = r * MathF.Sqrt(3f);
                        var back = Gfx.Mix(Color2, Color, 0.3f);
                        for (int i = -1; i <= w / dx + 1; i++)
                            for (int j = -1; j <= h / dy + 1; j++)
                            {
                                var p = new Vector2(i * dx, j * dy + ((i & 1) == 1 ? dy / 2f : 0f));
                                float d = (Vector2.Distance(p, ctr) / maxR) * 0.6f;
                                float local = Math.Clamp((cov * 1.6f - d) / 0.4f, 0f, 1f);
                                if (local <= 0f) continue;
                                // 前半は裏面が縮み、後半は表（色）が広がる＝パタッと裏返る
                                bool front = local >= 0.5f;
                                float sx = front ? (local - 0.5f) * 2f : 1f - local * 2f;
                                if (!front) sx = MathF.Max(sx, 0.05f);
                                var hex = new List<Vector2>();
                                float rr = r * (front && local >= 1f ? 1.04f : 1f);
                                for (int k = 0; k < 6; k++)
                                {
                                    float a = k * MathF.PI / 3f;
                                    hex.Add(p + new Vector2(MathF.Cos(a) * rr * sx, MathF.Sin(a) * rr));
                                }
                                brush.Color = Gfx.C4(front ? Color : back, 1f);
                                FillClip(dc, brush, hex, w, h);
                                if (!front)
                                {
                                    brush.Color = Gfx.C4(white, 0.4f);
                                    var cl = DrawKit.ClipRect(hex, 0, 0, w, h);
                                    if (cl.Count >= 3) Gfx.Polyline(dc, brush, ShapeGeom.Closed(cl), 1.5f);
                                }
                            }
                        break;
                    }

                // ---------------------------------------------------------------- 三角形の割れ
                case TransKind.Triangles:
                    {
                        float s = w / n * 1.3f;
                        float hgt = s * MathF.Sqrt(3f) / 2f;
                        int rows = (int)MathF.Ceiling(h / hgt) + 2;
                        Vector2 V(int i, int j) => new(i * s + j * s / 2f - s, j * hgt - hgt);
                        brush.Color = Gfx.C4(Color, 1f);
                        for (int j = 0; j < rows; j++)
                        {
                            int i0 = (int)MathF.Floor(-j / 2f) - 1, i1 = i0 + (int)MathF.Ceiling(w / s) + 4;
                            for (int i = i0; i < i1; i++)
                                for (int up = 0; up < 2; up++)
                                {
                                    var a = up == 0 ? V(i, j) : V(i + 1, j);
                                    var b = up == 0 ? V(i + 1, j) : V(i, j + 1);
                                    var cc = up == 0 ? V(i, j + 1) : V(i + 1, j + 1);
                                    var g = (a + b + cc) / 3f;
                                    if (g.X < -s || g.X > w + s || g.Y < -s || g.Y > h + s) continue;
                                    int id = i * 977 + j * 131 + up;
                                    float d = DrawKit.H(Seed, id, 1) * 0.55f;
                                    float local = Math.Clamp((cov - d) / 0.45f, 0f, 1f);
                                    if (local <= 0f) continue;
                                    float e = DrawKit.EaseOut(local);
                                    float fa = DrawKit.H(Seed, id, 2) * MathF.PI * 2f;
                                    var from = new Vector2(MathF.Cos(fa), MathF.Sin(fa)) * maxR * 0.6f;
                                    float rot = (1f - e) * (DrawKit.H(Seed, id, 3) - 0.5f) * 4f;
                                    float sc = (0.3f + 0.7f * e) * (local >= 1f ? 1.03f : 1f);
                                    var gp = g + from * (1f - e);
                                    var tri = new List<Vector2> { DrawKit.Rot(gp + (a - g) * sc, gp, rot), DrawKit.Rot(gp + (b - g) * sc, gp, rot), DrawKit.Rot(gp + (cc - g) * sc, gp, rot) };
                                    brush.Color = Gfx.C4(Gfx.Mix(Color, Color2, 0.35f * (1f - e)), 1f);
                                    FillClip(dc, brush, tri, w, h);
                                }
                        }
                        if (cov >= 0.999f) { brush.Color = Gfx.C4(Color, 1f); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }
                        break;
                    }

                // ---------------------------------------------------------------- 斜めの帯
                case TransKind.Bands:
                    {
                        var cols = new[] { Color2, Color3, Gfx.Mix(Color2, white, 0.5f), Color };
                        float slant = h * 0.45f;
                        for (int k = 0; k < cols.Length; k++)
                        {
                            float d = k * 0.12f;
                            float e = DrawKit.EaseOut(Seg(cov, d, d + 0.64f));
                            if (e <= 0f) continue;
                            float lead = -slant + e * (w + slant * 2f);
                            float tailX = k == cols.Length - 1 ? -slant - 10f : lead - w * 0.4f - slant;
                            var band = new List<Vector2> { new(tailX, h), new(lead, h), new(lead + slant, 0), new(tailX + slant, 0) };
                            brush.Color = Gfx.C4(cols[k], 1f);
                            FillClip(dc, brush, band, w, h);
                        }
                        if (cov >= 0.999f) { brush.Color = Gfx.C4(Color, 1f); dc.FillRectangle(new RawRectF(0, 0, w, h), brush); }
                        break;
                    }

                // ---------------------------------------------------------------- ジグザグ・波形ワイプ
                case TransKind.Zigzag:
                case TransKind.WaveWipe:
                    {
                        float amp = h / n * 0.9f;
                        float period = h / n * 2f;
                        float edge = -amp + cov * (w + amp * 2f);
                        var poly = new List<Vector2> { new(-2f, -2f) };
                        for (float y = -2f; y <= h + 2f; y += Kind == TransKind.Zigzag ? period / 2f : 6f)
                        {
                            float off = Kind == TransKind.Zigzag
                                ? ((int)MathF.Round(y / (period / 2f)) % 2 == 0 ? -amp : amp)
                                : MathF.Sin(y / period * MathF.PI * 2f + t * 4f) * amp;
                            poly.Add(new Vector2(edge + off, y));
                        }
                        poly.Add(new Vector2(edge, h + 2f));
                        poly.Add(new Vector2(-2f, h + 2f));
                        brush.Color = Gfx.C4(Color, 1f);
                        FillClip(dc, brush, poly, w, h);
                        break;
                    }

                // ---------------------------------------------------------------- ブラウン管オフ
                case TransKind.CrtOff:
                    {
                        float p1 = Seg(cov, 0f, 0.45f), p2 = Seg(cov, 0.45f, 0.8f), p3 = Seg(cov, 0.8f, 1f);
                        float wh = MathF.Max(2f, h * (1f - DrawKit.EaseOut(p1)));
                        float ww = MathF.Max(2f, w * (1f - DrawKit.EaseOut(p2)));
                        float x0 = ctr.X - ww / 2f, x1 = ctr.X + ww / 2f, y0 = ctr.Y - wh / 2f, y1 = ctr.Y + wh / 2f;
                        brush.Color = Gfx.C4(Color, 1f);
                        dc.FillRectangle(new RawRectF(0, 0, w, y0), brush);
                        dc.FillRectangle(new RawRectF(0, y1, w, h), brush);
                        dc.FillRectangle(new RawRectF(0, y0, x0, y1), brush);
                        dc.FillRectangle(new RawRectF(x1, y0, w, y1), brush);
                        // 白く光る線・点
                        dc.PrimitiveBlend = PrimitiveBlend.Add;
                        float glow = p1 * (1f - p3);
                        brush.Color = Gfx.C4(white, 0.6f * glow);
                        dc.FillRectangle(new RawRectF(x0, y0, x1, y1), brush);
                        DrawKit.Glow(dc, ctr, (p2 > 0f ? 60f : 30f) * (1f - p3) + 2f, Gfx.C4(Gfx.Mix(Color3, white, 0.6f), glow));
                        dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                        if (p3 > 0f) { brush.Color = Gfx.C4(Color, p3); dc.FillRectangle(new RawRectF(x0, y0, x1, y1), brush); }
                        break;
                    }

                // ---------------------------------------------------------------- ドット・ピクセル化
                case TransKind.Pixelate:
                    {
                        float q = MathF.Floor(cov * 12f) / 12f;                // 8bit 風に、カクカク進む
                        float cs = w / n;
                        int rows = (int)MathF.Ceiling(h / cs);
                        for (int r = 0; r < rows; r++)
                            for (int ci = 0; ci < n; ci++)
                            {
                                float hsh = DrawKit.H(Seed, ci * 131 + r, 1) * 0.6f + (ci / (float)n) * 0.4f;
                                if (q < hsh * 0.98f + 0.02f && cov < 0.999f) continue;
                                bool edge = q < hsh + 0.12f && cov < 0.999f;
                                brush.Color = Gfx.C4(edge ? Gfx.Mix(Color, Color2, 0.5f) : Color, 1f);
                                Gfx.Rect(dc, brush, ci * cs, r * cs, cs + 0.5f, cs + 0.5f);
                            }
                        break;
                    }

                // ---------------------------------------------------------------- グリッチ
                case TransKind.Glitch:
                    {
                        int fseed = (int)MathF.Floor(t * 24f) + Seed * 977;
                        int bars = (int)(6 + 40 * cov);
                        for (int i = 0; i < bars; i++)
                        {
                            float y = Gfx.Hash01(fseed * 31 + i * 7) * h;
                            float bh = (4f + Gfx.Hash01(fseed * 17 + i * 5) * h * 0.12f) * (0.4f + cov);
                            float x0 = (Gfx.Hash01(fseed * 13 + i * 3) - 0.3f) * w * (1f - cov);
                            float len = w * (0.3f + 0.9f * Gfx.Hash01(fseed * 7 + i * 11)) * (0.5f + cov);
                            float shift = 8f + 20f * Gfx.Hash01(fseed + i);
                            brush.Color = Gfx.C4(Color3, 0.7f);
                            dc.FillRectangle(new RawRectF(MathF.Max(0f, x0 - shift), y, MathF.Min(w, x0 + len - shift), MathF.Min(h, y + bh)), brush);
                            brush.Color = Gfx.C4(Color2, 0.7f);
                            dc.FillRectangle(new RawRectF(MathF.Max(0f, x0 + shift), y, MathF.Min(w, x0 + len + shift), MathF.Min(h, y + bh)), brush);
                            brush.Color = Gfx.C4(Color, 1f);
                            dc.FillRectangle(new RawRectF(MathF.Max(0f, x0), y, MathF.Min(w, x0 + len), MathF.Min(h, y + bh)), brush);
                        }
                        if (cov > 0.85f && (Gfx.Hash01(fseed + 5) > 0.3f || cov >= 0.97f))
                        {
                            brush.Color = Gfx.C4(Color, 1f);
                            dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
                        }
                        break;
                    }

                // ---------------------------------------------------------------- ローディング
                case TransKind.Loading:
                    {
                        float bg = Gfx.Smooth(0f, 0.25f, cov);
                        brush.Color = Gfx.C4(Color, bg);
                        dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
                        float k = Seg(cov, 0.25f, 0.95f);
                        float ta = Gfx.Smooth(0.15f, 0.3f, cov);
                        if (ta <= 0f) break;
                        float S = MathF.Min(w, h) / 1080f;
                        float th = 46f * S;
                        brush.Color = Gfx.C4(Color2, ta);
                        string dots = new string('.', 1 + (int)(t * 3f) % 3);
                        StrokeFont.Draw(dc, brush, "NOW LOADING" + dots, new Vector2(ctr.X - StrokeFont.Width("NOW LOADING...", th) / 2f, ctr.Y - th * 1.6f), th, MathF.Max(2f, th * 0.1f));
                        float bw = w * 0.45f, bh = 22f * S;
                        var bar = new RawRectF(ctr.X - bw / 2f, ctr.Y + th * 0.3f, ctr.X + bw / 2f, ctr.Y + th * 0.3f + bh);
                        brush.Color = Gfx.C4(Color2, 0.9f * ta);
                        dc.DrawRectangle(bar, brush, 3f * S + 1f);
                        brush.Color = Gfx.C4(Color3, ta);
                        float inset = 6f * S;
                        float fillW = (bw - inset * 2f) * k;
                        // 区切りのあるブロック
                        int blocks = 20;
                        float bwid = (bw - inset * 2f) / blocks;
                        for (int i = 0; i < blocks; i++)
                        {
                            float x = bar.Left + inset + i * bwid;
                            if (x - bar.Left - inset >= fillW) break;
                            dc.FillRectangle(new RawRectF(x, bar.Top + inset, x + bwid * 0.8f, bar.Bottom - inset), brush);
                        }
                        brush.Color = Gfx.C4(Color2, ta);
                        string pct = ((int)MathF.Round(k * 100f)).ToString(System.Globalization.CultureInfo.InvariantCulture) + "%";
                        StrokeFont.DrawCentered(dc, brush, pct, new Vector2(ctr.X, bar.Bottom + th * 0.9f), th * 0.6f, MathF.Max(1.5f, th * 0.07f));
                        // 回るマーク
                        var sp = new Vector2(bar.Right - th * 0.4f, ctr.Y - th * 1.1f);
                        for (int i = 0; i < 8; i++)
                        {
                            float a = i * MathF.PI / 4f + t * 6f;
                            brush.Color = Gfx.C4(Color2, ta * (0.2f + 0.8f * i / 7f));
                            dc.FillEllipse(new Ellipse(sp + new Vector2(MathF.Cos(a), MathF.Sin(a)) * th * 0.35f, th * 0.07f, th * 0.07f), brush);
                        }
                        break;
                    }
            }
        }
        finally
        {
            dc.Transform = prev;
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
