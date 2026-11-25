using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Shape;
using WpfColor = System.Windows.Media.Color;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// =====================================================================
//  ガラス（窓）
//   透明なガラス + 空の映り込み + 斜めの反射光 + 窓枠と桟 + 汚れ
//   + ひび割れ（「自動で割れる」で、衝撃 → ひびの成長 → 崩れ落ちるまで動く）
// =====================================================================
public sealed class WindowGlassPlugin : IShapePlugin
{
    public string Name => "演出：ガラス（窓）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WindowGlassParameter(sharedData);
}

public sealed class WindowGlassParameter : EffectShapeParameterBase
{
    public WindowGlassParameter() : this(null) { }
    public WindowGlassParameter(SharedDataStore? sharedData) : base(sharedData) { }

    // ---- ガラス ----
    [Display(GroupName = "ガラス", Name = "ガラスの色")]
    [ColorPicker]
    public WpfColor GlassColor { get => glass; set => Set(ref glass, value); }
    WpfColor glass = WpfColor.FromRgb(0xCF, 0xE8, 0xF5);

    [Display(GroupName = "ガラス", Name = "色の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Tint { get; } = new Animation(12, 0, 100);

    [Display(GroupName = "ガラス", Name = "映り込みの強さ", Description = "空の映り込みと反射光の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Reflection { get; } = new Animation(100, 0, 500);

    // ---- 反射光 ----
    [Display(GroupName = "反射光", Name = "本数")]
    [AnimationSlider("F0", "本", 0, 12)]
    public Animation StreakCount { get; } = new Animation(4, 0, 100);

    [Display(GroupName = "反射光", Name = "角度", Description = "0=縦、プラスで右上がり")]
    [AnimationSlider("F0", "°", -80, 80)]
    public Animation StreakAngle { get; } = new Animation(35, -89, 89);

    [Display(GroupName = "反射光", Name = "太さ")]
    [AnimationSlider("F0", "px", 10, 300)]
    public Animation StreakWidth { get; } = new Animation(60, 1, 3000);

    [Display(GroupName = "反射光", Name = "動く速さ", Description = "光が窓を横切る速さ（0で静止）")]
    [AnimationSlider("F0", "px/s", -300, 300)]
    public Animation StreakMove { get; } = new Animation(0, -5000, 5000);

    // ---- 窓枠 ----
    [Display(GroupName = "窓枠", Name = "枠の色")]
    [ColorPicker]
    public WpfColor FrameColor { get => frame; set => Set(ref frame, value); }
    WpfColor frame = WpfColor.FromRgb(0xD9, 0xD4, 0xC8);

    [Display(GroupName = "窓枠", Name = "枠の太さ", Description = "0で枠なし")]
    [AnimationSlider("F0", "px", 0, 150)]
    public Animation FrameThickness { get; } = new Animation(36, 0, 1000);

    [Display(GroupName = "窓枠", Name = "桟の太さ")]
    [AnimationSlider("F0", "px", 0, 60)]
    public Animation BarThickness { get; } = new Animation(14, 0, 500);

    [Display(GroupName = "窓枠", Name = "縦の桟の数")]
    [AnimationSlider("F0", "本", 0, 6)]
    public Animation VBars { get; } = new Animation(1, 0, 30);

    [Display(GroupName = "窓枠", Name = "横の桟の数")]
    [AnimationSlider("F0", "本", 0, 6)]
    public Animation HBars { get; } = new Animation(1, 0, 30);

    // ---- 汚れ ----
    [Display(GroupName = "汚れ", Name = "汚れの量")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Dirt { get; } = new Animation(25, 0, 100);

    // ---- ひび割れ（静止表示）----
    [Display(GroupName = "ひび割れ", Name = "ひびの量", Description = "0でひびなし。自動で割れる場合は広がりの大きさになる（0なら70%）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Crack { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "ひび割れ", Name = "中心X")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CrackX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "ひび割れ", Name = "中心Y")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CrackY { get; } = new Animation(45, -100, 200);

    // ---- 割れる演出（自動）----
    [Display(GroupName = "割れる演出", Name = "自動で割れる", Description = "オンにすると、指定した時刻に衝撃が入り、ひびが広がる")]
    [ToggleSlider]
    public bool AutoCrack { get => autoCrack; set => Set(ref autoCrack, value); }
    bool autoCrack = false;

    [Display(GroupName = "割れる演出", Name = "割れる時刻", Description = "アイテム開始からの秒数")]
    [AnimationSlider("F2", "秒", 0, 10)]
    public Animation CrackTime { get; } = new Animation(1, 0, 600);

    [Display(GroupName = "割れる演出", Name = "ひびが広がる時間")]
    [AnimationSlider("F2", "秒", 0.05, 2)]
    public Animation SpreadTime { get; } = new Animation(0.35, 0.05, 10);

    [Display(GroupName = "割れる演出", Name = "崩れ落ちる", Description = "ひびが広がったあと、ガラスが破片になって落ちる")]
    [ToggleSlider]
    public bool ShatterFall { get => shatterFall; set => Set(ref shatterFall, value); }
    bool shatterFall = true;

    [Display(GroupName = "割れる演出", Name = "破片の数")]
    [AnimationSlider("F0", "枚", 8, 200)]
    public Animation PieceCount { get; } = new Animation(48, 4, 1000);

    [Display(GroupName = "割れる演出", Name = "落下の重力")]
    [AnimationSlider("F0", "px/s²", 300, 5000)]
    public Animation FallGravity { get; } = new Animation(1800, 10, 20000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Tint, Reflection, StreakCount, StreakAngle, StreakWidth, StreakMove,
         FrameThickness, BarThickness, VBars, HBars, Dirt, Crack, CrackX, CrackY,
         CrackTime, SpreadTime, PieceCount, FallGravity];

    // 折れ線を先頭から割合 p だけ切り出す
    static List<Vector2> Partial(List<Vector2> pts, float p)
    {
        float f = Math.Clamp(p, 0f, 1f) * (pts.Count - 1);
        int idx = (int)MathF.Floor(f);
        var res = new List<Vector2>();
        for (int i = 0; i <= Math.Min(idx, pts.Count - 1); i++) res.Add(pts[i]);
        if (idx < pts.Count - 1) res.Add(Vector2.Lerp(pts[idx], pts[idx + 1], f - idx));
        return res;
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float F = MathF.Max(0f, c.F(FrameThickness));
        float B = MathF.Max(0f, c.F(BarThickness));
        int vb = Math.Max(0, (int)c.F(VBars)), hb = Math.Max(0, (int)c.F(HBars));
        float tint = c.F(Tint) / 100f, refl = c.F(Reflection) / 100f, dirt = c.F(Dirt) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);
        var black = WpfColor.FromRgb(0, 0, 0);

        // ---- 割れる演出のタイムライン ----
        bool auto = AutoCrack;
        float te = auto ? t - c.F(CrackTime) : float.MaxValue;     // 衝撃からの経過秒
        bool shatter = auto && ShatterFall;
        const float fallStart = 0.55f, fadeDur = 0.25f;
        float ga = 1f;                                             // ガラス面の残り具合（崩れ落ちるときに0へ）
        if (shatter && te > fallStart) ga = 1f - Gfx.Smooth(fallStart, fallStart + fadeDur, te);

        float crackAmt = c.F(Crack) / 100f;
        if (auto && crackAmt < 0.01f) crackAmt = 0.7f;
        float prog = 1f;                                           // ひびの成長（0〜1）
        bool showCrack = crackAmt > 0f;
        if (auto)
        {
            if (te < 0f) showCrack = false;
            else
            {
                float u = Math.Clamp(te / MathF.Max(0.05f, c.F(SpreadTime)), 0f, 1f);
                prog = 1f - (1f - u) * (1f - u) * (1f - u);        // 勢いよく走って減速
            }
        }
        var impact = new Vector2(c.F(CrackX) / 100f * w, c.F(CrackY) / 100f * h);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- ガラス面（ペイン）の矩形を求める ----
        float ix = F, iy = F, iw = MathF.Max(1f, w - F * 2f), ih = MathF.Max(1f, h - F * 2f);
        float colW = MathF.Max(1f, (iw - vb * B) / (vb + 1));
        float rowH = MathF.Max(1f, (ih - hb * B) / (hb + 1));
        var panes = new List<(float x, float y, float pw, float ph)>();
        for (int r = 0; r <= hb; r++)
            for (int cI = 0; cI <= vb; cI++)
                panes.Add((ix + cI * (colW + B), iy + r * (rowH + B), colW, rowH));

        if (ga > 0.003f)
        {
            // ---- 1. ガラスの色味 + 空の映り込み（ペインごと）----
            foreach (var p in panes)
            {
                brush.Color = Gfx.C4(GlassColor, tint * ga);
                Gfx.Rect(dc, brush, p.x, p.y, p.pw, p.ph);

                using (var sky = Gfx.Linear(dc, new Vector2(0, p.y), new Vector2(0, p.y + p.ph),
                    (0f, Gfx.C4(white, 0.22f * refl * ga)),
                    (0.5f, Gfx.C4(white, 0.04f * refl * ga)),
                    (1f, Gfx.C4(white, 0.10f * refl * ga))))
                    Gfx.Rect(dc, sky, p.x, p.y, p.pw, p.ph);

                brush.Color = Gfx.C4(white, 0.35f * MathF.Min(1f, refl) * ga);
                Gfx.Rect(dc, brush, p.x, p.y, p.pw, 2f);
                Gfx.Rect(dc, brush, p.x, p.y, 2f, p.ph);
                brush.Color = Gfx.C4(black, 0.18f * ga);
                Gfx.Rect(dc, brush, p.x, p.y + p.ph - 2f, p.pw, 2f);
                Gfx.Rect(dc, brush, p.x + p.pw - 2f, p.y, 2f, p.ph);
            }

            // ---- 2. 斜めの反射光 ----
            int sn = (int)c.F(StreakCount);
            if (sn > 0 && refl > 0f)
            {
                var dir = Gfx.Dir(c.F(StreakAngle));
                float diag = MathF.Sqrt(w * w + h * h);
                float sw = c.F(StreakWidth), move = c.F(StreakMove);
                float margin = sw * 2f, span = w + margin * 2f;
                var rs = new Random(Seed);
                for (int i = 0; i < sn; i++)
                {
                    float x0 = rs.NextF() * span;
                    float wd = sw * (0.6f + 0.8f * rs.NextF());
                    float st = 0.6f + 0.4f * rs.NextF();
                    float ph = rs.NextF() * MathF.PI * 2f;

                    float x = Gfx.Mod(x0 + t * move, span) - margin;
                    var ctr = new Vector2(x, h / 2f);
                    var a = ctr - dir * diag;
                    var b = ctr + dir * diag;
                    float shim = 0.75f + 0.25f * MathF.Sin(t * 0.5f + ph);
                    float k = refl * st * shim * ga;

                    brush.Color = Gfx.C4(white, 0.10f * k);
                    dc.DrawLine(a, b, brush, wd);
                    brush.Color = Gfx.C4(white, 0.16f * k);
                    dc.DrawLine(a, b, brush, wd * 0.28f);
                    brush.Color = Gfx.C4(white, 0.28f * k);
                    dc.DrawLine(a, b, brush, MathF.Max(1.5f, wd * 0.06f));
                }
            }

            // ---- 3. 汚れ ----
            if (dirt > 0f)
            {
                var rd = new Random(Seed + 17);
                var smudge = WpfColor.FromRgb(0xC8, 0xC8, 0xC0);
                for (int i = 0; i < 14; i++)
                {
                    var p = new Vector2(rd.NextF() * w, rd.NextF() * h);
                    float r = 40f + rd.NextF() * 120f;
                    using var sb = Gfx.Radial(dc, p, r, (0f, Gfx.C4(smudge, 0.10f * dirt * ga)), (1f, Gfx.C4(smudge, 0f)));
                    dc.FillEllipse(new Ellipse(p, r, r), sb);
                }
                for (int i = 0; i < 10; i++)
                {
                    float x = rd.NextF() * w, len = h * (0.2f + 0.6f * rd.NextF()), wd = 4f + rd.NextF() * 10f;
                    brush.Color = Gfx.C4(smudge, 0.06f * dirt * ga);
                    dc.DrawLine(new Vector2(x, 0), new Vector2(x + (rd.NextF() - 0.5f) * 20f, len), brush, wd);
                }
                for (int i = 0; i < 60; i++)
                {
                    var p = new Vector2(rd.NextF() * w, rd.NextF() * h);
                    float r = 0.6f + rd.NextF() * 1.6f;
                    brush.Color = Gfx.C4(smudge, (0.15f + 0.25f * rd.NextF()) * dirt * ga);
                    dc.FillEllipse(new Ellipse(p, r, r), brush);
                }
            }

            // ---- 4. ひび割れ（成長アニメーション）----
            if (showCrack)
            {
                var rc = new Random(Seed + 41);
                int rays = 8 + (int)(crackAmt * 8f);
                float maxLen = MathF.Max(w, h) * (0.15f + 0.6f * crackAmt);
                const int seg = 8;
                var lines = new List<List<Vector2>>();
                for (int r = 0; r < rays; r++)
                {
                    float ang = r / (float)rays * MathF.PI * 2f + (rc.NextF() - 0.5f) * 0.4f;
                    var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                    var perp = new Vector2(-dir.Y, dir.X);
                    float len = maxLen * (0.5f + 0.5f * rc.NextF());
                    var pts = new List<Vector2> { impact };
                    for (int s = 1; s <= seg; s++)
                        pts.Add(impact + dir * (len * s / seg) + perp * ((rc.NextF() - 0.5f) * len * 0.06f));
                    lines.Add(pts);
                }
                // 同心円状の亀裂は、放射状のひびが届いてから現れる
                for (int ring = 1; ring <= 3; ring++)
                {
                    int idx = ring * 2;
                    float thr = idx / (float)seg;
                    float ra = Gfx.Smooth(thr, thr + 0.15f, prog);
                    for (int r = 0; r < rays; r++)
                    {
                        var a = lines[r][idx];
                        var b = lines[(r + 1) % rays][idx];
                        var mid = (a + b) / 2f + new Vector2((rc.NextF() - 0.5f) * 8f, (rc.NextF() - 0.5f) * 8f);
                        if (ra <= 0.01f) continue;
                        var ring3 = new List<Vector2> { a, mid, b };
                        brush.Color = Gfx.C4(black, 0.25f * ra * ga);
                        Gfx.Polyline(dc, brush, ring3.Select(q => q + new Vector2(1f, 1f)).ToList(), 1.2f);
                        brush.Color = Gfx.C4(white, 0.65f * ra * ga);
                        Gfx.Polyline(dc, brush, ring3, 0.9f);
                    }
                }
                foreach (var full in lines)
                {
                    var pts = Partial(full, prog);
                    if (pts.Count < 2) continue;
                    brush.Color = Gfx.C4(black, 0.35f * ga);
                    Gfx.Polyline(dc, brush, pts.Select(q => q + new Vector2(1.2f, 1.2f)).ToList(), 1.8f);
                    brush.Color = Gfx.C4(white, 0.9f * ga);
                    Gfx.Polyline(dc, brush, pts, 1.2f);
                }
                for (int i = 0; i < 12; i++)
                {
                    var p = impact + new Vector2((rc.NextF() - 0.5f) * 24f, (rc.NextF() - 0.5f) * 24f);
                    float r = 1f + rc.NextF() * 3f;
                    brush.Color = Gfx.C4(white, 0.8f * ga);
                    dc.FillEllipse(new Ellipse(p, r, r), brush);
                }
            }
        }

        // ---- 5. 衝撃の閃光と飛び散る細かい破片 ----
        if (auto && te >= 0f)
        {
            if (te < 0.3f)
            {
                float fa = 1f - te / 0.3f;
                float fr = MathF.Max(w, h) * 0.3f;
                using var fb = Gfx.Radial(dc, impact, fr,
                    (0f, Gfx.C4(white, 0.7f * fa)), (0.3f, Gfx.C4(white, 0.25f * fa)), (1f, Gfx.C4(white, 0f)));
                dc.FillEllipse(new Ellipse(impact, fr, fr), fb);
            }
            if (te < 0.8f)
            {
                float g2 = c.F(FallGravity) * 0.3f;
                var rsh = new Random(Seed + 61);
                for (int i = 0; i < 28; i++)
                {
                    float ang = rsh.NextF() * MathF.PI * 2f;
                    float spd = 300f + rsh.NextF() * 700f;
                    float len = 10f + rsh.NextF() * 22f;
                    var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                    var pos = impact + dir * (spd * te * (1f - 0.3f * te)) + new Vector2(0f, 0.5f * g2 * te * te);
                    brush.Color = Gfx.C4(white, 0.9f * (1f - te / 0.8f));
                    dc.DrawLine(pos, pos - dir * len, brush, 1.5f);
                }
            }
        }

        // ---- 6. 崩れ落ちる破片（回転しながら落下）----
        if (shatter && te >= fallStart)
        {
            int pcs = Math.Max(4, (int)c.F(PieceCount));
            int cols = Math.Max(2, (int)MathF.Round(MathF.Sqrt(pcs * iw / ih)));
            int rows = Math.Max(2, (int)MathF.Round(pcs / (float)cols));
            float tw = iw / cols, th = ih / rows;
            float g = c.F(FallGravity);
            float maxd = MathF.Sqrt(iw * iw + ih * ih);
            var rt = new Random(Seed + 71);
            try
            {
                for (int rr = 0; rr < rows; rr++)
                    for (int cc = 0; cc < cols; cc++)
                    {
                        float jx = (rt.NextF() - 0.5f) * tw * 0.3f, jy = (rt.NextF() - 0.5f) * th * 0.3f;
                        float rot = (rt.NextF() - 0.5f) * 6f, flipS = 1f + rt.NextF() * 3f;
                        float delayR = rt.NextF(), vx = (rt.NextF() - 0.5f) * 160f;
                        float sizeK = 0.8f + 0.25f * rt.NextF();

                        var center = new Vector2(ix + (cc + 0.5f) * tw + jx, iy + (rr + 0.5f) * th + jy);
                        float d = (center - impact).Length() / maxd;
                        float start = fallStart + d * 0.35f + delayR * 0.15f;
                        float tau = te - start;
                        var pos = center;
                        float angle = 0f, sx = 1f;
                        if (tau > 0f)
                        {
                            pos = center + new Vector2(vx * tau, 0.5f * g * tau * tau);
                            angle = rot * tau;
                            sx = MathF.Max(0.25f, MathF.Abs(MathF.Cos(tau * flipS)));
                        }
                        if (pos.Y - th > h) continue;
                        float alpha = tau > 0f ? 1f - Gfx.Smooth(0.9f, 1.4f, tau) : 1f;
                        if (alpha <= 0.01f) continue;

                        float hw = tw * 0.5f * sizeK, hh = th * 0.5f * sizeK;
                        var rect = new RawRectF(pos.X - hw, pos.Y - hh, pos.X + hw, pos.Y + hh);
                        dc.Transform = Matrix3x2.CreateScale(sx, 1f, pos) * Matrix3x2.CreateRotation(angle, pos);
                        brush.Color = Gfx.C4(GlassColor, (0.18f + tint) * alpha);
                        dc.FillRectangle(rect, brush);
                        brush.Color = Gfx.C4(white, 0.12f * refl * alpha);
                        dc.FillRectangle(rect, brush);
                        brush.Color = Gfx.C4(white, 0.8f * alpha);
                        dc.DrawRectangle(rect, brush, 1.3f);
                    }
            }
            finally
            {
                dc.Transform = Matrix3x2.Identity;
            }
        }

        // ---- 7. 窓枠と桟（一番上）----
        var light = Gfx.Mix(FrameColor, white, 0.5f);
        var dark = Gfx.Mix(FrameColor, black, 0.35f);

        void Bar(float x, float y, float bw, float bh, bool vertical)
        {
            brush.Color = Gfx.C4(FrameColor);
            Gfx.Rect(dc, brush, x, y, bw, bh);
            brush.Color = Gfx.C4(light);
            if (vertical) Gfx.Rect(dc, brush, x, y, MathF.Min(2f, bw), bh);
            else Gfx.Rect(dc, brush, x, y, bw, MathF.Min(2f, bh));
            brush.Color = Gfx.C4(dark);
            if (vertical) Gfx.Rect(dc, brush, x + bw - MathF.Min(2f, bw), y, MathF.Min(2f, bw), bh);
            else Gfx.Rect(dc, brush, x, y + bh - MathF.Min(2f, bh), bw, MathF.Min(2f, bh));
        }

        if (B > 0f)
        {
            for (int i = 1; i <= vb; i++)
                Bar(ix + i * colW + (i - 1) * B, iy, B, ih, true);
            for (int i = 1; i <= hb; i++)
                Bar(ix, iy + i * rowH + (i - 1) * B, iw, B, false);
        }
        if (F > 0f)
        {
            Bar(0, 0, w, F, false);
            Bar(0, h - F, w, F, false);
            Bar(0, 0, F, h, true);
            Bar(w - F, 0, F, h, true);

            brush.Color = Gfx.C4(black, 0.18f * ga);
            Gfx.Rect(dc, brush, F, F, iw, 3f);
            Gfx.Rect(dc, brush, F, F, 3f, ih);
            brush.Color = Gfx.C4(black, 0.08f * ga);
            Gfx.Rect(dc, brush, F, F + 3f, iw, 4f);
            Gfx.Rect(dc, brush, F + 3f, F, 4f, ih);
        }
    }
}
