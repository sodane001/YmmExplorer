using System.Numerics;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 641〜670: 学校・スポーツ・家電

public sealed class PencilDrawPlugin : QuickShapePlugin { public override string Name => "演出：鉛筆が線を引く"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PencilDrawParameter(s); }
public sealed class PencilDrawParameter : Q4Art
{
    public PencilDrawParameter() : this(null) { }
    public PencilDrawParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFC62B); Color2 = Hex(0x30343B); }
    protected override string[] Use => ["Color1=鉛筆の色", "Color2=線の色", "Speed=描く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        var q = new List<Vector2>(); float k = ArtPen.Fr(t * .22f) * 1.15f, end = MathF.Min(1f, k); for (int i = 0; i <= 120 * end; i++) { float e = i / 120f; q.Add(new(-520 + e * 1040, 120 + ArtPen.Sin(e * 14f) * 90f * (1f - e * .3f))); }
        if (q.Count >= 2) p.Line(p.C(Color2), 7, q); var h = q.Count > 0 ? q[^1] : new Vector2(-520, 120);
        p.Turn(h.X, h.Y, .72f, () => { p.F(p.C(0x30343B), h.X, h.Y, h.X - 18, h.Y - 38, h.X + 18, h.Y - 38); p.F(p.C(0xF2D2A9), h.X - 18, h.Y - 38, h.X + 18, h.Y - 38, h.X + 36, h.Y - 98, h.X - 36, h.Y - 98); p.B(h.X, h.Y - 260, 72, 324, p.C(Color1)); p.B(h.X - 12, h.Y - 260, 11, 324, p.C(0xFFE27A)); p.B(h.X, h.Y - 438, 74, 34, p.C(0xB9BEC6)); p.R(h.X, h.Y - 484, 72, 62, 16, p.C(0xFF8FA8)); });
    }
}

public sealed class EraserWipePlugin : QuickShapePlugin { public override string Name => "演出：消しゴムで消す"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new EraserWipeParameter(s); }
public sealed class EraserWipeParameter : Q4Art
{
    public EraserWipeParameter() : this(null) { }
    public EraserWipeParameter(SharedDataStore? s) : base(s) { Text = "まちがい"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x2F6FBB); Color3 = Hex(0x2B2E33); }
    protected override string[] Use => ["Text=消される文字", "Color1=消しゴムの色", "Color2=カバーの色", "Color3=文字の色", "Speed=消す速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.R(0, 0, 1100, 460, 10, p.C(0xFFFDF5)); for (int i = -2; i <= 2; i++) p.L(-540, i * 76, 540, i * 76, p.C(0xA8C8E8), 2);
        float k = ArtPen.Cl(t * .35f), x = -470 + k * 940 + ArtPen.Sin(t * 22f) * 58f * (k < 1f ? 1f : 0f);
        p.T(Str, 150, 0, 0, p.C(Color3), maxW: 980); if (k > 0f) p.B(-520 + (x + 540) / 2f, 0, MathF.Max(0f, x + 560), 280, p.C(0xFFFDF5)); for (int i = -1; i <= 1; i++) if (x > -470) p.L(-510, i * 76, MathF.Min(530, x), i * 76, p.C(0xA8C8E8), 2);
        for (int i = 0; i < 18; i++) { float e = ArtPen.Fr(t * .9f + ArtPen.R01(i)), dx = (ArtPen.R01(i + 9) - .5f) * 90f; p.R(x + dx - e * 25f, 120 + e * 105, 22, 6, 3, p.C(0x8C8C92, 1f - e)); }
        p.Turn(x, -25, -.14f, () => { p.R(x, -25, 190, 300, 18, p.C(Color1)); p.R(x, 10, 200, 230, 6, p.C(Color2)); p.B(x, -60, 200, 22, p.C(0xFFFFFF, .85f)); });
    }
}

public sealed class RulerProtractorPlugin : QuickShapePlugin { public override string Name => "演出：定規と分度器"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RulerProtractorParameter(s); }
public sealed class RulerProtractorParameter : Q4Art
{
    public RulerProtractorParameter() : this(null) { }
    public RulerProtractorParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xBDEBFF); Color2 = Hex(0xFF5252); }
    protected override double DefAmount => 33;
    protected override string[] Use => ["Amount=角度|0〜100 = 0〜180度", "Color1=定規の色", "Color2=線の色", "Speed=動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float R = 370, y = 210; p.Pie(0, y, R, MathF.PI, MathF.Tau, p.C(Color1, .62f)); p.Arc(0, y, R, MathF.PI, MathF.Tau, p.C(0x2F6FBB), 5); p.Arc(0, y, R * .55f, MathF.PI, MathF.Tau, p.C(0x2F6FBB, .6f), 3);
        for (int d = 0; d <= 180; d += 5) { float a = MathF.PI + d * MathF.PI / 180f, l = d % 30 == 0 ? 38 : d % 10 == 0 ? 25 : 13; p.L(ArtPen.Cos(a) * R, y + ArtPen.Sin(a) * R, ArtPen.Cos(a) * (R - l), y + ArtPen.Sin(a) * (R - l), p.C(0x1F4F87), d % 30 == 0 ? 4 : 2); if (d % 30 == 0) p.T(d.ToString(), 26, ArtPen.Cos(a) * (R - 68), y + ArtPen.Sin(a) * (R - 68), p.C(0x1F4F87)); }
        p.R(0, y + 58, 900, 92, 5, p.C(0xFFF3C4, .86f)); for (int i = -21; i <= 21; i++) p.L(i * 20, y + 12, i * 20, y + (i % 5 == 0 ? 54 : 34), p.C(0x4A3A1A), i % 5 == 0 ? 3 : 2);
        float deg = v * 180f * ArtPen.Cl(t * .7f), an = MathF.PI + (180f - deg) * MathF.PI / 180f; p.L(-R - 30, y, R + 30, y, p.C(Color2), 6); p.L(0, y, ArtPen.Cos(an) * (R + 45), y + ArtPen.Sin(an) * (R + 45), p.C(Color2), 7); p.Arc(0, y, 96, an, MathF.Tau, p.C(Color2), 6); p.D(0, y, 10, p.C(Color2)); p.T($"{deg:0}°", 66, 0, y - 150, p.C(Color2), p.C(0xFFFFFF), 5);
    }
}

public sealed class AbacusPlugin : QuickShapePlugin { public override string Name => "演出：そろばん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AbacusParameter(s); }
public sealed class AbacusParameter : Q4Art
{
    public AbacusParameter() : this(null) { }
    public AbacusParameter(SharedDataStore? s) : base(s) { Text = "12345"; Color1 = Hex(0x5A3620); Color2 = Hex(0x2A1A10); }
    protected override string[] Use => ["Text=表す数|数字で書きます", "Color1=枠の色", "Color2=珠の色", "Speed=はじく速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        var dg = new string(Str.Where(char.IsDigit).ToArray()); const int n = 9; p.R(0, 0, 1180, 560, 16, p.C(Color1)); p.B(0, 0, 1120, 500, p.C(0xE9D7B5)); p.B(0, -112, 1120, 16, p.C(Color1));
        for (int c = 0; c < n; c++)
        {
            float x = (c - (n - 1) / 2f) * 120; p.L(x, -250, x, 250, p.C(0x7A5A35), 6); int idx = c - (n - dg.Length), d = idx >= 0 && idx < dg.Length ? dg[idx] - '0' : 0; float m = ArtPen.Cl((t - .15f - MathF.Max(0, idx) * .16f) * 5f); m = m * m * (3f - 2f * m);
            float fy = d >= 5 ? -210 + 66 * m : -210; p.E(x, fy, 54, 26, p.C(Color2)); p.E(x, fy - 7, 34, 8, p.C(0xFFFFFF, .22f)); int low = d % 5;
            for (int b = 0; b < 4; b++) { float rest = 68 + b * 52, up = -66 * (b < low ? m : 0f); float y = rest + up; p.E(x, y, 54, 26, p.C(Color2)); p.E(x, y - 7, 34, 8, p.C(0xFFFFFF, .22f)); }
        }
    }
}

public sealed class SchoolBagPlugin : QuickShapePlugin { public override string Name => "演出：ランドセル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SchoolBagParameter(s); }
public sealed class SchoolBagParameter : Q4Art
{
    public SchoolBagParameter() : this(null) { }
    public SchoolBagParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xC8202F); Color2 = Hex(0xE7C04A); }
    protected override double DefAmount => 25;
    protected override string[] Use => ["Amount=ふたの開き", "Color1=本体の色", "Color2=金具の色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        var dark = Gfx.Mix(Color1, Hex(0x000000), .35f); float op = v * (.65f + .35f * ArtPen.Sin(t * 1.3f)); p.R(0, 40, 520, 590, 62, p.C(dark)); p.R(0, 130, 440, 360, 24, p.C(Color1)); for (int i = 0; i < 4; i++) p.R(-110 + i * 72, -130 + 0, 56, 200, 4, p.C(i % 2 == 0 ? 0x4FA8FF : 0x66BB6A));
        p.Arc(0, -255, 96, MathF.PI, MathF.Tau, p.C(dark), 26); p.Zoom(0, -250, 1f, ArtPen.Cl(1f - op * .9f), () => { p.R(0, 0, 540, 510, 74, p.C(Color1)); p.RL(0, 0, 500, 470, 60, p.C(0xFFFFFF, .5f), 4); p.R(0, 195, 120, 72, 14, p.C(Color2)); p.R(0, 195, 52, 22, 8, p.C(dark)); });
    }
}

public sealed class MicroscopeViewPlugin : QuickShapePlugin { public override string Name => "演出：顕微鏡"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MicroscopeViewParameter(s); }
public sealed class MicroscopeViewParameter : Q4Art
{
    public MicroscopeViewParameter() : this(null) { }
    public MicroscopeViewParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x455A64); Color2 = Hex(0x8DE86A); Count = 16; }
    protected override string[] Use => ["Color1=本体の色", "Color2=微生物の色", "Count=微生物の数||1|60", "Speed=動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        var o = new Vector2(-340, 60); p.R(o.X, 300, 360, 44, 10, p.C(Color1)); p.Arc(o.X - 20, 90, 180, -1.15f, 1.3f, p.C(Color1), 40); p.R(o.X - 25, 175, 250, 22, 4, p.C(0x90A4AE)); p.Turn(o.X, -80, .24f, () => { p.R(o.X, -120, 86, 290, 12, p.C(Color1)); p.R(o.X, -285, 56, 72, 7, p.C(0x263238)); p.R(o.X, 45, 58, 66, 6, p.C(0x90A4AE)); }); p.D(o.X + 148, 80, 30, p.C(0x90A4AE));
        var c = new Vector2(330, -20); const float R = 300; p.Ring(c.X, c.Y, R + 14, p.C(0x263238), 28); p.D(c.X, c.Y, R, p.C(0xEAF7D8)); p.L(o.X + 40, -310, c.X - 230, c.Y - 210, p.C(0xFFFFFF, .4f), 3); p.L(o.X + 40, -260, c.X - 230, c.Y + 210, p.C(0xFFFFFF, .4f), 3);
        for (int i = 0; i < Math.Clamp(Count, 1, 60); i++) { float a = ArtPen.R01(i) * MathF.Tau + t * (.08f + ArtPen.R01(i + 3) * .25f), r = MathF.Sqrt(ArtPen.R01(i + 7)) * (R - 48), x = c.X + ArtPen.Cos(a) * r, y = c.Y + ArtPen.Sin(a) * r, s = 12f + ArtPen.R01(i + 11) * 20f; p.Turn(x, y, a + ArtPen.Sin(t * 3f + i) * .4f, () => { p.E(x, y, s * 1.5f, s * .75f, p.C(Color2, .78f)); p.D(x + s * .3f, y, s * .3f, p.C(0x2E7D32)); p.P(p.C(Color2), 2.5f, x - s * 1.4f, y, x - s * 2f, y + ArtPen.Sin(t * 9f + i) * 6f, x - s * 2.6f, y); }); }
    }
}

public sealed class FlaskExperimentPlugin : QuickShapePlugin { public override string Name => "演出：フラスコの実験"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FlaskExperimentParameter(s); }
public sealed class FlaskExperimentParameter : Q4Art
{
    public FlaskExperimentParameter() : this(null) { }
    public FlaskExperimentParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x4FC3F7); Color2 = Hex(0xE85DFF); Count = 16; }
    protected override string[] Use => ["Color1=はじめの色", "Color2=変わった色", "Count=泡の量||0|60", "Speed=反応の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float m = ArtPen.Cl((t - .6f) * .55f); var liq = Gfx.Mix(Color1, Color2, m);
        p.F(p.C(liq, .9f), -102, 28, 102, 28, 245, 292, 220, 325, -220, 325, -245, 292);
        for (int i = 0; i < Math.Clamp(Count, 0, 60); i++) { float k = ArtPen.Fr(t * (.5f + ArtPen.R01(i) * .5f) + ArtPen.R01(i + 5)), hw = 90f + (1f - k) * 118f, x = (ArtPen.R01(i + 8) - .5f) * hw * 1.8f + ArtPen.Sin(k * 9f + i) * 8f; p.Ring(x, 310f - k * 278f, 5f + ArtPen.R01(i + 3) * 13f, p.C(0xFFFFFF, .75f * (1f - k * .5f)), 3); }
        p.P(p.C(0xE8F4FA, .92f), 11, -70, -280, -70, -40, -252, 290, -226, 335, 226, 335, 252, 290, 70, -40, 70, -280); p.R(0, -285, 190, 28, 12, p.C(0xE8F4FA)); p.L(-150, 240, -100, 145, p.C(0xFFFFFF, .7f), 12);
        for (int i = 0; i < 8; i++) { float k = ArtPen.Fr(t * .42f + i / 8f); p.D(ArtPen.Sin(k * 7f + i) * (10f + k * 42f), -310f - k * 240f, 14f + k * 34f, p.C(liq, .34f * (1f - k))); }
    }
}

public sealed class TestPaperPlugin : QuickShapePlugin { public override string Name => "演出：テストの答案"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TestPaperParameter(s); }
public sealed class TestPaperParameter : Q4Art
{
    public TestPaperParameter() : this(null) { }
    public TestPaperParameter(SharedDataStore? s) : base(s) { Text = "100"; Color1 = Hex(0xFFFDF5); Color2 = Hex(0xE5202E); Count = 5; }
    protected override string[] Use => ["Text=点数", "Count=問題の数||1|8", "Color1=紙の色", "Color2=赤ペンの色", "Speed=採点の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        int n = Math.Clamp(Count, 1, 8), score = int.TryParse(Str, out var sc) ? sc : 100, ok = (int)MathF.Round(score / 100f * n); const float W = 760, H = 900; p.R(8, 10, W, H, 6, p.C(0x000000, .25f)); p.R(0, 0, W, H, 6, p.C(Color1)); p.L(-340, -300, 340, -300, p.C(0x444444), 3); p.T("テスト", 50, -200, -372, p.C(0x2B2E33)); p.Ring(235, -348, 88, p.C(Color2, ArtPen.Cl((t - .3f - n * .28f) * 4f)), 6);
        for (int i = 0; i < n; i++) { float y = -240 + i * 590f / n, k = ArtPen.Cl((t - .2f - i * .28f) * 5f); p.T($"問{i + 1}", 34, -285, y, p.C(0x2B2E33)); p.B(-20, y, 340, 3, p.C(0x9A9AA0)); p.L(-180, y - 26, 110, y - 26, p.C(0xB9BEC6), 8); if (k <= 0f) continue; if (i < ok) p.Arc(245, y, 40, 0, k * MathF.Tau, p.C(Color2), 7); else { p.L(205, y - 38, 205 + 80 * k, y - 38 + 76 * k, p.C(Color2), 7); if (k >= 1f) p.L(285, y - 38, 205, y + 38, p.C(Color2), 7); } }
        float q = ArtPen.Cl((t - .3f - n * .28f) * 4f); if (q > 0f) p.T(Str, 74, 235, -350, p.C(Color2, q));
    }
}

public sealed class ClassTimetablePlugin : QuickShapePlugin { public override string Name => "演出：時間割"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ClassTimetableParameter(s); }
public sealed class ClassTimetableParameter : Q4Art
{
    public ClassTimetableParameter() : this(null) { }
    public ClassTimetableParameter(SharedDataStore? s) : base(s) { Text = "国語,算数,理科,社会,英語\n算数,英語,国語,体育,音楽\n理科,国語,算数,図工,社会\n社会,体育,英語,算数,国語"; Color1 = Hex(0x2F6FBB); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xFFD54A); }
    protected override double DefAmount => 25;
    protected override string[] Use => ["Text=科目|1行が1時間目から。カンマで月〜金", "Amount=光らせる曜日|0=月 〜 100=金", "Color1=見出しの色", "Color2=マスの色", "Color3=光の色"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        var r = Str.Split('\n').Select(x => x.Split([',', '、']).Select(y => y.Trim()).ToArray()).Where(x => x.Length > 0 && x[0].Length > 0).Take(8).ToList(); if (r.Count == 0) return; const int d = 5; string wk = "月火水木金"; float cw = 230, rh = MathF.Min(135f, 680f / r.Count), x0 = -cw * d / 2f + 45, y0 = -rh * r.Count / 2f + 45; int hit = Math.Clamp((int)MathF.Round(v * 4f), 0, 4);
        for (int c = 0; c < d; c++) { p.R(x0 + (c + .5f) * cw, y0 - 45, cw - 7, 84, 8, p.C(c == hit ? Color3 : Color1)); p.T(wk[c].ToString(), 42, x0 + (c + .5f) * cw, y0 - 45, c == hit ? p.C(0x263238) : p.C(0xFFFFFF)); }
        for (int y = 0; y < r.Count; y++) { p.T($"{y + 1}", 44, x0 - 52, y0 + (y + .5f) * rh, p.C(0xFFFFFF), p.C(0x000000, .5f), 3); for (int c = 0; c < d; c++) { float k = ArtPen.Cl((p.Now - .1f - (c + y) * .05f) * 4f), q = DrawKit.EaseOutBack(k); p.R(x0 + (c + .5f) * cw, y0 + (y + .5f) * rh, (cw - 7) * q, (rh - 7) * q, 8, c == hit ? p.C(Gfx.Mix(Color2, Color3, .35f)) : p.C(Color2)); if (c < r[y].Length) p.T(r[y][c], 38, x0 + (c + .5f) * cw, y0 + (y + .5f) * rh, p.C(0x263238, k), maxW: cw - 22); } }
    }
}

public sealed class DiplomaScrollPlugin : QuickShapePlugin { public override string Name => "演出：卒業証書"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DiplomaScrollParameter(s); }
public sealed class DiplomaScrollParameter : Q4Art
{
    public DiplomaScrollParameter() : this(null) { }
    public DiplomaScrollParameter(SharedDataStore? s) : base(s) { Text = "卒業証書"; Color1 = Hex(0x1F2A44); Color2 = Hex(0xFFF9EA); Color3 = Hex(0xC9A227); }
    protected override string[] Use => ["Text=文字", "Color1=筒の色", "Color2=紙の色", "Color3=金の色", "Speed=開く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float k = DrawKit.EaseOut(ArtPen.Cl((t - .2f) * .9f)), W = 110 + k * 820; const float H = 560; p.B(0, 0, W, H, p.C(Color2)); if (k > .55f) { float a = ArtPen.Cl((k - .55f) * 3f); p.RL(0, 0, W - 66, H - 54, 4, p.C(Color3, a), 4); string s = Str; for (int i = 0; i < s.Length && i < 6; i++) p.T(s[i].ToString(), 72, W * .24f, (i - (MathF.Min(s.Length, 6) - 1) / 2f) * 84, p.C(0x1F2024, a)); for (int l = 0; l < 4; l++) p.L(W * .08f - l * 62, -180, W * .08f - l * 62, 180, p.C(0x55565C, .55f * a), 4); p.R(-W * .28f, 170, 88, 88, 6, p.C(0xD7263D, a)); p.T("印", 48, -W * .28f, 170, p.C(0xFFFFFF, a)); }
        foreach (int s in new[] { -1, 1 }) { float x = s * W / 2f; p.R(x, 0, 92, H + 42, 28, p.C(Color1)); p.B(x, -H / 2f + 22, 94, 12, p.C(Color3)); p.B(x, H / 2f - 22, 94, 12, p.C(Color3)); p.L(x - 24, -H / 2f + 40, x - 24, H / 2f - 40, p.C(0xFFFFFF, .24f), 7); }
    }
}

public sealed class SoccerGoalPlugin : QuickShapePlugin { public override string Name => "演出：サッカーのゴール"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SoccerGoalParameter(s); }
public sealed class SoccerGoalParameter : Q4Art
{
    public SoccerGoalParameter() : this(null) { }
    public SoccerGoalParameter(SharedDataStore? s) : base(s) { Text = "GOAL!"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFFD54A); }
    protected override string[] Use => ["Text=文字", "Color1=ゴールの色", "Color2=文字の色", "Speed=速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float k = ArtPen.Cl((t - .15f) * 1.5f), hit = ArtPen.Cl((t - .82f) * 2f), bulge = k >= 1f ? ArtPen.Sin(hit * MathF.PI * 3f) * (1f - hit) * 36f : 0f, bx = k < 1f ? -620 + k * 780 : 160 + bulge * .4f, by = k < 1f ? 300 - k * 370 - ArtPen.Sin(k * MathF.PI) * 150f : -70 + hit * hit * 330f;
        p.E(0, 320, 760, 42, p.C(0x3F9A46)); for (int i = -8; i <= 8; i++) { var q = new List<Vector2>(); for (int y = 0; y <= 12; y++) { float e = y / 12f, py = -260 + e * 560, d = new Vector2(i * 62 - 160, py + 70).Length(), w = MathF.Exp(-d * d / 26000f) * bulge; q.Add(new(i * 62 + w * .6f, py + w * .2f)); } p.Line(p.C(Color1, .42f), 2.5f, q); } for (int y = 0; y <= 12; y++) { var q = new List<Vector2>(); for (int i = -8; i <= 8; i++) { float py = -260 + y * 46.6f, d = new Vector2(i * 62 - 160, py + 70).Length(), w = MathF.Exp(-d * d / 26000f) * bulge; q.Add(new(i * 62 + w * .6f, py + w * .2f)); } p.Line(p.C(Color1, .42f), 2.5f, q); }
        p.P(p.C(Color1), 20, -510, 310, -510, -270, 510, -270, 510, 310); float R = 64f - k * 18f; p.D(bx, by, R, p.C(0xFFFFFF)); p.Turn(bx, by, t * 12f * (1f - hit), () => { p.Star(bx, by, R * .42f, 5, .75f, 0, p.C(0x20242B)); for (int i = 0; i < 5; i++) { float a = i * MathF.Tau / 5f - MathF.PI / 2f; p.L(bx + ArtPen.Cos(a) * R * .4f, by + ArtPen.Sin(a) * R * .4f, bx + ArtPen.Cos(a) * R, by + ArtPen.Sin(a) * R, p.C(0x20242B), 3); } }); p.Ring(bx, by, R, p.C(0x20242B), 3);
        if (k >= 1f) p.T(Str, 180, 0, -60, p.C(Color2, ArtPen.Cl(hit * 4f)), p.C(0xD7263D, ArtPen.Cl(hit * 4f)), 9);
    }
}

public sealed class BaseballHomerPlugin : QuickShapePlugin { public override string Name => "演出：野球のホームラン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BaseballHomerParameter(s); }
public sealed class BaseballHomerParameter : Q4Art
{
    public BaseballHomerParameter() : this(null) { }
    public BaseballHomerParameter(SharedDataStore? s) : base(s) { Text = "HOME RUN!"; Color1 = Hex(0xC98A4B); Color2 = Hex(0xFFD54A); }
    protected override string[] Use => ["Text=文字", "Color1=バットの色", "Color2=文字の色", "Speed=速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float inK = ArtPen.Cl(t * 1.8f), sw = ArtPen.Cl((t - .42f) * 7f), ang = 2.15f - sw * sw * 3.1f, h = ArtPen.Cl((t - .55f) * .7f); var hp = new Vector2(-340, 170);
        p.Turn(hp.X, hp.Y, ang, () => p.F(p.C(Color1), hp.X, hp.Y - 11, hp.X + 260, hp.Y - 17, hp.X + 500, hp.Y - 34, hp.X + 530, hp.Y, hp.X + 500, hp.Y + 34, hp.X + 260, hp.Y + 17, hp.X, hp.Y + 11)); p.D(hp.X, hp.Y, 22, p.C(0x3A3E45));
        float bx, by, r; if (t < .55f) { bx = 700 - inK * 740; by = -120 + inK * 145; r = 25; } else { bx = -35 + h * 880; by = 25 - ArtPen.Sin(h * MathF.PI * .72f) * 500f; r = 34f * (1f - h * .78f); for (int i = 1; i < 16; i++) { float q = MathF.Max(0f, h - i * .018f); p.D(-35 + q * 880, 25 - ArtPen.Sin(q * MathF.PI * .72f) * 500f, r * (1f - i / 17f), p.C(0xFFFFFF, .3f * (1f - i / 16f))); } }
        if (h < 1f) { p.D(bx, by, r, p.C(0xFFFFFF)); p.Arc(bx - r * .9f, by, r * .9f, -.75f, .75f, p.C(0xE5202E), MathF.Max(1f, r * .1f)); p.Arc(bx + r * .9f, by, r * .9f, MathF.PI - .75f, MathF.PI + .75f, p.C(0xE5202E), MathF.Max(1f, r * .1f)); } else p.Star(845, -320, 75f * ArtPen.Cl(1f - (t - 2f) * 2f), 4, .15f, t, p.C(0xFFFFFF));
        float f = ArtPen.Cl((t - .55f) * 8f) * (1f - ArtPen.Cl((t - .7f) * 6f)); if (f > 0f) p.Star(-35, 25, 170f * f, 10, .35f, 0, p.C(0xFFF2A8, f)); float tx = ArtPen.Cl((t - 1.1f) * 3f); if (tx > 0f) p.T(Str, 130, 0, 300, p.C(Color2, tx), p.C(0xD7263D, tx), 7);
    }
}

public sealed class BasketShotPlugin : QuickShapePlugin { public override string Name => "演出：バスケのシュート"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BasketShotParameter(s); }
public sealed class BasketShotParameter : Q4Art
{
    public BasketShotParameter() : this(null) { }
    public BasketShotParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xF07A1F); Color2 = Hex(0xE53935); }
    protected override string[] Use => ["Color1=ボールの色", "Color2=リングの色", "Speed=速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float cyc = ArtPen.Fr(t * .42f), k = ArtPen.Cl(cyc / .55f), fall = ArtPen.Cl((cyc - .55f) / .25f), bx = -620 + k * 930, by = k < 1f ? 310 - k * 520 - ArtPen.Sin(k * MathF.PI) * 410f : -210 + fall * fall * 560f; const float rx = 310, ry = -200;
        p.R(450, -340, 340, 240, 8, p.C(0xFFFFFF, .9f)); p.RL(450, -340, 340, 240, 8, p.C(Color2), 7); p.RL(400, -300, 125, 95, 3, p.C(Color2), 5); p.B(620, -60, 26, 760, p.C(0x70757B)); p.L(300, -210, 620, -210, p.C(0x70757B), 8);
        void Net(float pull) { for (int i = -3; i <= 3; i++) p.L(rx + i * 31, ry, rx + i * 18, ry + 125 + pull, p.C(0xFFFFFF, .8f), 3); for (int l = 1; l <= 3; l++) { float e = l / 3.4f; p.L(rx - 93 + 39 * e, ry + (125 + pull) * e, rx + 93 - 39 * e, ry + (125 + pull) * e, p.C(0xFFFFFF, .65f), 3); } }
        bool inFront = k >= 1f; if (!inFront) Net(0); const float R = 66; p.D(bx, by, R, p.C(Color1)); p.Turn(bx, by, t * 7f, () => { p.L(bx - R, by, bx + R, by, p.C(0x3A2010), 4); p.L(bx, by - R, bx, by + R, p.C(0x3A2010), 4); p.Arc(bx - R * 1.1f, by, R * .85f, -.9f, .9f, p.C(0x3A2010), 4); p.Arc(bx + R * 1.1f, by, R * .85f, MathF.PI - .9f, MathF.PI + .9f, p.C(0x3A2010), 4); });
        if (inFront) Net(MathF.Sin(fall * MathF.PI) * 38f); p.EL(rx, ry, 100, 18, p.C(Color2), 9); if (fall > 0f && fall < 1f) p.T("SWISH!", 72, 280, -420, p.C(0xFFD54A, 1f - fall), p.C(0x000000, .6f * (1f - fall)), 4);
    }
}

public sealed class TennisRallyPlugin : QuickShapePlugin { public override string Name => "演出：テニスのラリー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TennisRallyParameter(s); }
public sealed class TennisRallyParameter : Q4Art
{
    public TennisRallyParameter() : this(null) { }
    public TennisRallyParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x2F8E5B); Color2 = Hex(0xD9F24B); }
    protected override string[] Use => ["Color1=コートの色", "Color2=ボールの色", "Speed=ラリーの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.F(p.C(Color1), -600, 330, 600, 330, 390, -200, -390, -200); p.P(p.C(0xFFFFFF), 5, -600, 330, 600, 330, 390, -200, -390, -200, -600, 330); p.L(-500, 150, 500, 150, p.C(0xFFFFFF), 4); p.L(-430, -80, 430, -80, p.C(0xFFFFFF), 4); p.L(0, 150, 0, -80, p.C(0xFFFFFF), 4);
        float ph = t * .9f; int round = (int)MathF.Floor(ph); float k = ph - round; bool up = round % 2 == 0; float e = up ? k : 1f - k, side = (ArtPen.R01(round) - .5f) * 280f, side2 = (ArtPen.R01(round + 1) - .5f) * 280f, s0 = up ? side : side2, s1 = up ? side2 : side, x = s0 + (s1 - s0) * k, y = 270 - e * 440 - ArtPen.Sin(k * MathF.PI) * 150f, scale = 1f - e * .4f;
        p.E(x, 270 - e * 440, 22 * scale, 7 * scale, p.C(0x000000, .32f));
        for (int i = 0; i <= 12; i++) { p.L(-470 + i * 78.3f, -48, -470 + i * 78.3f, 34, p.C(0xFFFFFF, .35f), 2); } for (int i = 0; i < 4; i++) p.L(-470, -40 + i * 24, 470, -40 + i * 24, p.C(0xFFFFFF, .35f), 2); p.B(0, -52, 950, 11, p.C(0xFFFFFF)); foreach (int s in new[] { -1, 1 }) p.B(s * 475, -8, 11, 96, p.C(0x70757B));
        void Racket(float rx, float ry, float rs, float swing) => p.Turn(rx, ry, swing, () => { p.R(rx, ry + 78 * rs, 16 * rs, 100 * rs, 6, p.C(0x3A3E45)); p.EL(rx, ry, 48 * rs, 58 * rs, p.C(0xE53935), 7 * rs); for (int i = -2; i <= 2; i++) { p.L(rx + i * 15 * rs, ry - 50 * rs, rx + i * 15 * rs, ry + 50 * rs, p.C(0xFFFFFF, .6f), 1.5f); p.L(rx - 40 * rs, ry + i * 18 * rs, rx + 40 * rs, ry + i * 18 * rs, p.C(0xFFFFFF, .6f), 1.5f); } });
        float near = up ? 1f - ArtPen.Cl(k * 5f) : ArtPen.Cl((k - .8f) * 5f), far = up ? ArtPen.Cl((k - .8f) * 5f) : 1f - ArtPen.Cl(k * 5f); Racket(up ? side + 70 : side2 + 70, 270, 1.05f, -.7f + near * 1.1f); Racket((up ? side2 : side) - 40, -185, .62f, .6f - far * 1.1f);
        p.D(x, y, 22 * scale, p.C(Color2)); p.Arc(x - 18 * scale, y, 15 * scale, -.9f, .9f, p.C(0xFFFFFF), 2);
    }
}

public sealed class BowlingStrikePlugin : QuickShapePlugin { public override string Name => "演出：ボウリングのストライク"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BowlingStrikeParameter(s); }
public sealed class BowlingStrikeParameter : Q4Art
{
    public BowlingStrikeParameter() : this(null) { }
    public BowlingStrikeParameter(SharedDataStore? s) : base(s) { Text = "STRIKE!"; Color1 = Hex(0x2F48C8); Color2 = Hex(0xFFD54A); }
    protected override string[] Use => ["Text=文字", "Color1=ボールの色", "Color2=文字の色", "Speed=速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float k = ArtPen.Cl(t * .85f), hit = MathF.Max(0f, t - 1.18f), bx = -760 + k * 1090, by = 180 - k * 54; p.F(p.C(0xC99A5B), -900, 330, 900, 330, 620, 40, -620, 40); for (int i = -6; i <= 6; i++) p.L(i * 142, 330, i * 90, 40, p.C(0x9A7040, .55f), 2);
        int[][] rows = [[0], [-1, 1], [-2, 0, 2], [-3, -1, 1, 3]]; int id = 0;
        for (int r = 3; r >= 0; r--) foreach (int c in rows[r]) { float px = 330 + r * 66 + c * 40, py = 130 - r * 26, sc = 1f - r * .08f; int n = id++; float a = hit * (3f + ArtPen.R01(n) * 7f) * (c >= 0 ? 1f : -1f), dx = hit * (150f + ArtPen.R01(n + 3) * 280f) * (c >= 0 ? 1f : -.8f) + hit * 100f, dy = -hit * 420f + hit * hit * 620f; float op = 1f - ArtPen.Cl((hit - .7f) * 3f); if (op <= 0f) continue; p.Move(hit > 0f ? dx : 0, hit > 0f ? dy : 0, () => p.Turn(px, py, hit > 0f ? a : 0, () => { p.E(px, py + 12 * sc, 33 * sc, 54 * sc, p.C(0xFFFFFF, op)); p.E(px, py - 52 * sc, 20 * sc, 26 * sc, p.C(0xFFFFFF, op)); p.B(px, py - 25 * sc, 30 * sc, 7 * sc, p.C(0xE5202E, op)); })); }
        if (k < 1f || hit < .6f) { float bxx = k < 1f ? bx : 330 + hit * 260; p.D(bxx, by, 76, p.C(Color1)); p.Turn(bxx, by, t * 16f, () => { p.D(bxx + 22, by - 26, 8, p.C(0x0D1540)); p.D(bxx + 46, by - 18, 8, p.C(0x0D1540)); p.D(bxx + 36, by + 8, 8, p.C(0x0D1540)); }); }
        if (hit > 0f && hit < .25f) p.Star(330, 110, 240f * (1f - hit * 2f), 12, .4f, 0, p.C(0xFFF2A8, 1f - hit * 4f)); if (hit > .15f) p.T(Str, 190, 0, -200, p.C(Color2), p.C(0xD7263D), 9);
    }
}

public sealed class PingPongRallyPlugin : QuickShapePlugin { public override string Name => "演出：卓球のラリー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PingPongRallyParameter(s); }
public sealed class PingPongRallyParameter : Q4Art
{
    public PingPongRallyParameter() : this(null) { }
    public PingPongRallyParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x1F6E8C); Color2 = Hex(0xFF8A2B); }
    protected override string[] Use => ["Color1=台の色", "Color2=球の色", "Speed=ラリーの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float top = 110; p.B(0, top + 16, 1100, 32, p.C(Color1)); p.B(0, top + 1, 1100, 6, p.C(0xFFFFFF)); foreach (int s in new[] { -1, 1 }) p.B(s * 450, top + 170, 26, 270, p.C(0x3A3E45)); p.B(0, top - 44, 8, 90, p.C(0xEEEEEE)); for (int i = 0; i < 4; i++) p.L(-3, top - 80 + i * 22, 3, top - 80 + i * 22, p.C(0x70757B), 2);
        float ph = t * 1.5f; int rd = (int)MathF.Floor(ph); float k = ph - rd, dir = rd % 2 == 0 ? 1f : -1f, x = dir * (-640 + k * 1280), bounce = .68f, y = k < bounce ? top - 16 - MathF.Abs(ArtPen.Sin(k / bounce * MathF.PI)) * 230f : top - 16 - ArtPen.Sin((k - bounce) / (1f - bounce) * MathF.PI * .5f) * 180f; if (k < bounce * .5f) y -= (bounce * .5f - k) * 260f;
        foreach (int s in new[] { -1, 1 }) { bool hit = (s < 0) == (dir > 0) ? k < .12f : k > .9f; float rx = s * 690, swing = s * (hit ? .55f : -.2f); p.Turn(rx, -45, swing, () => { p.R(rx + s * 8, 65, 26, 92, 8, p.C(0xC98A4B)); p.D(rx, -45, 84, p.C(s < 0 ? 0xE53935u : 0x1E2228u)); }); }
        for (int i = 1; i <= 6; i++) { float q = k - i * .018f; if (q <= 0f) continue; float ox = dir * (-640 + q * 1280), oy = q < bounce ? top - 16 - MathF.Abs(ArtPen.Sin(q / bounce * MathF.PI)) * 230f : top - 16 - ArtPen.Sin((q - bounce) / (1f - bounce) * MathF.PI * .5f) * 180f; if (q < bounce * .5f) oy -= (bounce * .5f - q) * 260f; p.D(ox, oy, 16f * (1f - i / 7f), p.C(Color2, .32f * (1f - i / 7f))); }
        p.D(x, y, 18, p.C(Color2)); p.D(x - 5, y - 6, 5, p.C(0xFFFFFF, .75f));
    }
}

public sealed class GolfCupInPlugin : QuickShapePlugin { public override string Name => "演出：ゴルフのカップイン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GolfCupInParameter(s); }
public sealed class GolfCupInParameter : Q4Art
{
    public GolfCupInParameter() : this(null) { }
    public GolfCupInParameter(SharedDataStore? s) : base(s) { Text = "NICE IN!"; Color1 = Hex(0x56B84B); Color2 = Hex(0xE53935); }
    protected override string[] Use => ["Text=文字", "Color1=芝の色", "Color2=旗の色", "Speed=転がる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.E(0, 180, 760, 210, p.C(Color1)); p.E(40, 195, 560, 130, p.C(Gfx.Mix(Color1, Hex(0xFFFFFF), .16f))); const float cx = 330, cy = 200; p.E(cx, cy, 46, 18, p.C(0x20242B)); p.L(cx, cy, cx, -310, p.C(0xF2F2F2), 8); p.F(p.C(Color2), cx, -310, cx + 175, -258 + ArtPen.Sin(t * 5f) * 11f, cx, -196);
        float k = ArtPen.Cl(t * .55f), e = 1f - (1f - k) * (1f - k), x = -560 + e * (cx + 560), y = 240 - e * 40 + ArtPen.Sin(e * 8f) * 7f * (1f - e); bool inn = k >= 1f; if (!inn) { var q = new List<Vector2>(); for (int i = 0; i <= 30; i++) { float a = e * i / 30f; q.Add(new(-560 + a * (cx + 560), 240 - a * 40 + ArtPen.Sin(a * 8f) * 7f * (1f - a))); } p.Line(p.C(0xFFFFFF, .34f), 4, q); p.E(x, y + 19, 25, 7, p.C(0x000000, .3f)); p.D(x, y, 22, p.C(0xFFFFFF)); p.D(x - 7 + ArtPen.Sin(t * 10f) * 7f, y - 5, 2.6f, p.C(0xB9BEC6)); p.D(x + 7, y + 3 + ArtPen.Cos(t * 10f) * 6f, 2.6f, p.C(0xB9BEC6)); }
        if (inn) { float f = ArtPen.Cl((t - 1.82f) * 2f); for (int i = 0; i < 16; i++) { float a = i / 16f * MathF.Tau, r = 30f + f * 190f; p.Star(cx + ArtPen.Cos(a) * r, cy + ArtPen.Sin(a) * r * .6f - f * 50f, 18f * (1f - f), 4, .25f, 0, p.C(0xFFE45C)); } p.T(Str, 140, -80, -130, p.C(0xFFFFFF, ArtPen.Cl(f * 3f)), p.C(0x1F6E2C, ArtPen.Cl(f * 3f)), 7); }
    }
}

public sealed class FinishTapePlugin : QuickShapePlugin { public override string Name => "演出：ゴールテープ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FinishTapeParameter(s); }
public sealed class FinishTapeParameter : Q4Art
{
    public FinishTapeParameter() : this(null) { }
    public FinishTapeParameter(SharedDataStore? s) : base(s) { Text = "GOAL!"; Color1 = Hex(0xE53935); Color2 = Hex(0xFFD54A); }
    protected override string[] Use => ["Text=文字", "Color1=テープの色", "Color2=文字の色", "Speed=速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float k = ArtPen.Cl((t - .5f) * 2.4f); foreach (int s in new[] { -1, 1 }) { p.R(s * 620, 0, 30, 660, 8, p.C(0xD7DBE0)); var q = new List<Vector2>(); for (int i = 0; i <= 28; i++) { float e = i / 28f, wave = ArtPen.Sin(e * 8f + t * 9f) * 22f * e * k, reach = 1f - k * .62f; q.Add(new(s * (620 - e * 620 * reach) + s * k * e * 8f, -40 + wave + k * e * e * 180f)); } p.Line(p.C(Color1), 28, q); if (k < .02f) break; }
        if (k < .02f) { p.B(0, -40, 1240, 26, p.C(Color1)); p.T("FINISH", 22, 0, -40, p.C(0xFFFFFF)); }
        if (k > 0f) { for (int i = 0; i < 44; i++) { float e = ArtPen.Fr(t * .35f + ArtPen.R01(i)), x = (ArtPen.R01(i + 5) - .5f) * 1100f + ArtPen.Sin(e * 8f + i) * 26f; var c = (i % 4) switch { 0 => p.C(0xFF5C8A), 1 => p.C(0x4FC3F7), 2 => p.C(0xFFE45C), _ => p.C(0x66E08A) }; p.Turn(x, -360 + e * 720, e * 9f + i, () => p.B(x, -360 + e * 720, 22, 10, c)); } p.T(Str, 220, 0, 110, p.C(Color2, ArtPen.Cl(k * 3f)), p.C(0xD7263D, ArtPen.Cl(k * 3f)), 10); }
    }
}

public sealed class DartsHitPlugin : QuickShapePlugin { public override string Name => "演出：ダーツ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DartsHitParameter(s); }
public sealed class DartsHitParameter : Q4Art
{
    public DartsHitParameter() : this(null) { }
    public DartsHitParameter(SharedDataStore? s) : base(s) { Text = "BULL!"; Count = 3; Color1 = Hex(0xE53935); Color2 = Hex(0x2E9E6B); }
    protected override string[] Use => ["Text=文字", "Count=本数||1|6", "Color1=色1", "Color2=色2", "Speed=投げる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float R = 370; p.D(0, 0, R + 34, p.C(0x17181B)); for (int i = 0; i < 20; i++) { float a0 = i * MathF.Tau / 20f - MathF.PI / 20f - MathF.PI / 2f, a1 = a0 + MathF.Tau / 20f; p.Pie(0, 0, R, a0, a1, i % 2 == 0 ? p.C(0x20242B) : p.C(0xF2E6C8)); } foreach (float rr in new[] { R, R * .62f }) for (int i = 0; i < 20; i++) { float a0 = i * MathF.Tau / 20f - MathF.PI / 20f - MathF.PI / 2f; p.Arc(0, 0, rr - 12, a0, a0 + MathF.Tau / 20f, i % 2 == 0 ? p.C(Color1) : p.C(Color2), 24); } p.D(0, 0, 58, p.C(Color2)); p.D(0, 0, 26, p.C(Color1));
        int n = Math.Clamp(Count, 1, 6); for (int i = 0; i < n; i++)
        {
            float k = ArtPen.Cl((t - .2f - i * .5f) * 3.6f), hx = i == n - 1 ? 3 : (ArtPen.R01(i + 3) - .5f) * 330, hy = i == n - 1 ? 4 : (ArtPen.R01(i + 9) - .5f) * 330, x = hx + (1f - k) * 860, y = hy - (1f - k) * 240 + ArtPen.Sin(k * MathF.PI) * -60f; if (k <= 0f) continue; float shake = k >= 1f ? ArtPen.Sin((t - .2f - i * .5f) * 56f) * .08f * MathF.Exp(-(t - .48f - i * .5f) * 5f) : 0f;
            p.Turn(x, y, -.32f + shake, () => { p.L(x, y, x + 72, y, p.C(0xCDD2D8), 5); p.R(x + 118, y, 96, 22, 8, p.C(0x3A3E45)); p.L(x + 165, y, x + 225, y, p.C(0xE5E7EA), 5); p.F(p.C(0x4FC3F7), x + 205, y, x + 260, y - 42, x + 275, y); p.F(p.C(0x2F86C6), x + 205, y, x + 260, y + 42, x + 275, y); });
        }
        float d = ArtPen.Cl((t - .48f - (n - 1) * .5f) * 4f); if (d > 0f) p.T(Str, 150, 0, -440, p.C(0xFFE45C, d), p.C(0xD7263D, d), 8);
    }
}

public sealed class SumoRingPlugin : QuickShapePlugin { public override string Name => "演出：相撲の土俵"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SumoRingParameter(s); }
public sealed class SumoRingParameter : Q4Art
{
    public SumoRingParameter() : this(null) { }
    public SumoRingParameter(SharedDataStore? s) : base(s) { Text = "はっけよい"; Color1 = Hex(0xC9A36A); Color2 = Hex(0x5B3A23); Color3 = Hex(0x7A1E2C); }
    protected override bool Scene => true;
    protected override string[] Use => ["Text=文字", "Color1=土俵の色", "Color2=屋根の色", "Color3=房の色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.B(0, 0, p.W + 4, p.H + 4, p.C(0x1A1820)); for (int r = 0; r < 4; r++) for (int i = 0; i < 44; i++) { float x = -p.W / 2f + (i + .5f + (r % 2) * .5f) * p.W / 44f, y = -40 + r * 62; p.D(x, y + ArtPen.Sin(t * 2f + i) * 2f, 16, p.C(ArtPen.R01(i * 7 + r) > .5f ? 0x4A4048u : 0x35303Au)); }
        p.F(p.C(Color1), -700, 540, 700, 540, 520, 260, -520, 260); p.E(0, 260, 520, 112, p.C(Gfx.Mix(Color1, Hex(0xFFFFFF), .16f))); p.EL(0, 260, 395, 84, p.C(0xF4E5C4), 11); p.L(-36, 245, -36, 275, p.C(0xFFFFFF), 5); p.L(36, 245, 36, 275, p.C(0xFFFFFF), 5);
        p.F(p.C(Color2), -520, -360, 520, -360, 390, -515, -390, -515); p.B(0, -345, 1060, 34, p.C(0x3A2415)); for (int i = 0; i < 26; i++) p.L(-510 + i * 40.8f, -335, -510 + i * 40.8f, -270, p.C(0xF2F2EC), 11); foreach (int s in new[] { -1, 1 }) { float sw = ArtPen.Sin(t * 1.3f + s) * 6f; p.L(s * 520, -345, s * 520 + sw, -170, p.C(Color3), 13); p.E(s * 520 + sw, -120, 34, 66, p.C(Color3)); }
        p.Turn(0, 170, ArtPen.Sin(t * 3f) * .13f, () => { p.L(0, 215, 0, 110, p.C(0x4E2F16), 12); p.E(0, 62, 66, 68, p.C(0x7A4A25)); p.EL(0, 62, 66, 68, p.C(0xE6C65A), 5); }); p.T(Str, 90, 0, -130, p.C(0xFFFFFF), p.C(Color3), 6, 900);
    }
}

public sealed class ElectricFanPlugin : QuickShapePlugin { public override string Name => "演出：扇風機"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ElectricFanParameter(s); }
public sealed class ElectricFanParameter : Q4Art
{
    public ElectricFanParameter() : this(null) { }
    public ElectricFanParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x74C7E8); Color2 = Hex(0xF4F6F8); }
    protected override string[] Use => ["Color1=羽根の色", "Color2=本体の色", "Speed=回る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.E(0, 395, 230, 45, p.C(Color2)); p.R(0, 240, 44, 310, 12, p.C(Color2)); p.R(120, 380, 40, 18, 6, p.C(0x4FA8FF)); float w = .72f + .28f * ArtPen.Cos(ArtPen.Sin(t * .7f) * 1.1f), sk = ArtPen.Sin(t * .7f) * 34f;
        p.Move(sk, -80, () => p.Zoom(0, 0, w, 1f, () => { const float R = 255; for (int b = 0; b < 3; b++) { float a = t * 16f + b * MathF.Tau / 3f; p.Turn(0, 0, a, () => p.E(138, 0, 118, 66, p.C(Color1, .78f))); } p.D(0, 0, 45, p.C(Color2)); p.D(0, 0, 19, p.C(0x4FA8FF)); p.Ring(0, 0, R, p.C(0xB7C2CC), 9); p.Ring(0, 0, R * .66f, p.C(0xB7C2CC, .6f), 3); for (int i = 0; i < 24; i++) { float a = i * MathF.Tau / 24f; p.L(ArtPen.Cos(a) * 50, ArtPen.Sin(a) * 50, ArtPen.Cos(a) * R, ArtPen.Sin(a) * R, p.C(0xB7C2CC, .55f), 2); } }));
        for (int i = 0; i < 7; i++) { float k = ArtPen.Fr(t * .75f + i / 7f), y = -280 + i * 62, x = 300 + k * 330; p.P(p.C(0xFFFFFF, .55f * (1f - k)), 4, x, y, x + 45, y - 12, x + 90, y + 5); }
    }
}

public sealed class WashingMachinePlugin : QuickShapePlugin { public override string Name => "演出：洗濯機"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WashingMachineParameter(s); }
public sealed class WashingMachineParameter : Q4Art
{
    public WashingMachineParameter() : this(null) { }
    public WashingMachineParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xF2F4F7); Color2 = Hex(0x74C7E8); Count = 24; }
    protected override string[] Use => ["Color1=本体の色", "Color2=水の色", "Count=泡の量||0|80", "Speed=回る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float sh = ArtPen.Sin(t * 26f) * 4f; p.Move(sh, 0, () =>
        {
            p.R(0, 0, 680, 800, 28, p.C(Color1)); p.RL(0, 0, 680, 800, 28, p.C(0xAEB7C0), 4); p.B(0, -275, 680, 4, p.C(0xAEB7C0)); p.R(190, -335, 170, 60, 8, p.C(0x20303A)); p.T($"{Math.Max(0, 45 - (int)p.Now)}分", 34, 190, -335, p.C(0x5CFFB0)); p.D(-230, -335, 42, p.C(0xC4CCD4)); p.L(-230, -335, -230 + ArtPen.Cos(t) * 34, -335 + ArtPen.Sin(t) * 34, p.C(0x3A3E45), 7); p.R(-90, -335, 90, 40, 6, p.C(0xC4CCD4));
            const float cy = 90, R = 235; p.D(0, cy, R + 42, p.C(0xCBD3DA)); p.D(0, cy, R, p.C(0x233345)); p.Pie(0, cy, R, .18f + ArtPen.Sin(t * 2.2f) * .12f, MathF.PI - .18f + ArtPen.Sin(t * 2.2f) * .12f, p.C(Color2, .8f));
            for (int i = 0; i < 7; i++) { float a = t * 3.2f + i * MathF.Tau / 7f, r = 95f + ArtPen.R01(i) * 75f; float x = ArtPen.Cos(a) * r, y = cy + ArtPen.Sin(a) * r; uint col = (i % 3) switch { 0 => 0xFF7F9Fu, 1 => 0xFFE27Au, _ => 0xA3E37Au }; p.Turn(x, y, a * 1.4f, () => p.R(x, y, 112, 62, 18, p.C(col))); }
            for (int i = 0; i < Math.Clamp(Count, 0, 80); i++) { float a = ArtPen.R01(i) * MathF.Tau + t * (1f + ArtPen.R01(i + 3)), r = MathF.Sqrt(ArtPen.R01(i + 7)) * (R - 16); p.Ring(ArtPen.Cos(a) * r, cy + ArtPen.Sin(a) * r, 5f + ArtPen.R01(i + 9) * 11f, p.C(0xFFFFFF, .8f), 2.5f); }
            p.Arc(0, cy, R - 24, 3.5f, 4.6f, p.C(0xFFFFFF, .55f), 11);
        });
    }
}

public sealed class MicrowaveChinPlugin : QuickShapePlugin { public override string Name => "演出：電子レンジ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MicrowaveChinParameter(s); }
public sealed class MicrowaveChinParameter : Q4Art
{
    public MicrowaveChinParameter() : this(null) { }
    public MicrowaveChinParameter(SharedDataStore? s) : base(s) { Text = "チン！"; Color1 = Hex(0xF2F4F7); Color2 = Hex(0xFFD95C); }
    protected override double DefAmount => 25;
    protected override string[] Use => ["Text=音の文字", "Amount=温める時間|0〜100 = 0〜10秒", "Color1=本体の色", "Color2=中の光の色", "Speed=回る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float dur = MathF.Max(.5f, v * 10f), left = dur - p.Now; bool on = left > 0f; p.R(0, 0, 980, 600, 26, p.C(Color1)); p.RL(0, 0, 980, 600, 26, p.C(0xAEB7C0), 4); p.R(-125, 0, 640, 470, 16, p.C(0x2A333D)); p.R(-125, 0, 590, 420, 10, on ? p.C(Color2, .82f + ArtPen.Sin(t * 19f) * .08f) : p.C(0x3C4A56));
        p.E(-125, 130, 230, 44, p.C(0xFFFFFF, .7f)); float sx = ArtPen.Cos(t * 2.4f); p.Zoom(-125, 90, .7f + MathF.Abs(sx) * .3f, 1f, () => { p.E(-125, 120, 130, 25, p.C(0xFFFFFF)); p.R(-125, 66, 190, 104, 22, p.C(0xE57F4A)); p.B(-125 + sx * 44, 66, 26, 104, p.C(0xFFE27A)); }); for (int i = -8; i <= 8; i++) p.L(-125 + i * 34, -205, -125 + i * 34, 205, p.C(0x000000, .12f), 2);
        p.R(355, -190, 200, 86, 8, p.C(0x20303A)); p.T(on ? $"{(int)left / 60}:{(int)left % 60:00}" : "0:00", 48, 355, -190, p.C(0x5CFFB0)); for (int y = 0; y < 4; y++) for (int x = 0; x < 3; x++) p.R(295 + x * 60, -95 + y * 58, 46, 42, 7, p.C(0xCBD3DA)); p.R(355, 200, 170, 58, 10, p.C(0x4FA8FF));
        if (!on) { float k = ArtPen.Cl(-left * 2.5f), fade = 1f - ArtPen.Cl((-left - 1.4f) * 2f); if (fade > 0f) { for (int i = 0; i < 10; i++) { float a = i / 10f * MathF.Tau; p.L(ArtPen.Cos(a) * (170 + k * 40), -330 + ArtPen.Sin(a) * (80 + k * 20), ArtPen.Cos(a) * (220 + k * 70), -330 + ArtPen.Sin(a) * (110 + k * 36), p.C(0xFFE45C, fade), 7); } p.T(Str, 110, 0, -330, p.C(0xFFFFFF, fade), p.C(0xE5202E, fade), 7); } }
    }
}

public sealed class RiceCookerPlugin : QuickShapePlugin { public override string Name => "演出：炊飯器"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RiceCookerParameter(s); }
public sealed class RiceCookerParameter : Q4Art
{
    public RiceCookerParameter() : this(null) { }
    public RiceCookerParameter(SharedDataStore? s) : base(s) { Text = "炊きあがり"; Color1 = Hex(0xF4F5F7); Color2 = Hex(0xE85D5D); }
    protected override double DefAmount => 25;
    protected override string[] Use => ["Text=表示", "Amount=炊く時間|0〜100 = 0〜10秒", "Color1=本体の色", "Color2=ランプの色", "Speed=湯気の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float dur = MathF.Max(.5f, v * 10f); bool done = p.Now >= dur; p.R(0, 95, 640, 420, 90, p.C(Color1)); p.RL(0, 95, 640, 420, 90, p.C(0xAEB7C0), 3); p.E(0, -105, 322, 112, p.C(0xE3E7EB)); p.E(0, -125, 275, 88, p.C(Color1)); p.R(0, -212, 140, 40, 16, p.C(0xB7C2CC)); p.R(0, 150, 380, 160, 18, p.C(0x20303A)); p.T(done ? Str : "炊飯中", 52, 0, 120, p.C(done ? 0xFFE27Au : 0x5CFFB0u), maxW: 340); float bar = ArtPen.Cl(p.Now / dur); p.B(-140 + 140 * bar, 192, 280 * bar, 12, p.C(0x5CFFB0)); p.D(255, 150, 19, done || (int)(t * 3f) % 2 == 0 ? p.C(Color2) : p.C(0x7A4040));
        for (int s = 0; s < 3; s++) { var q = new List<Vector2>(); for (int i = 0; i <= 22; i++) { float k = i / 22f; q.Add(new((s - 1) * 48 + ArtPen.Sin(k * 7f - t * 2.4f + s) * 22f, -235f - k * (done ? 150f : 260f))); } p.Line(p.C(0xFFFFFF, done ? .25f : .48f), 8, q); }
    }
}

public sealed class VacuumCleanerPlugin : QuickShapePlugin { public override string Name => "演出：掃除機"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new VacuumCleanerParameter(s); }
public sealed class VacuumCleanerParameter : Q4Art
{
    public VacuumCleanerParameter() : this(null) { }
    public VacuumCleanerParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x4F7DE0); Color2 = Hex(0xA69B8C); Count = 34; }
    protected override string[] Use => ["Color1=本体の色", "Color2=ほこりの色", "Count=ほこりの量||0|120", "Speed=吸う速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float nx = 230 + ArtPen.Sin(t * .9f) * 150f; p.L(-700, 300, 700, 300, p.C(0xFFFFFF, .35f), 4);
        for (int i = 0; i < Math.Clamp(Count, 0, 120); i++) { float k = ArtPen.Fr(t * .7f + ArtPen.R01(i)), sx = nx + 90 + ArtPen.R01(i + 5) * 340, sy = 280 - ArtPen.R01(i + 8) * 45, x = sx + (nx - sx) * k * k, y = sy + (262 - sy) * k * k; p.D(x, y, (3f + ArtPen.R01(i + 9) * 6f) * (1f - k * .6f), p.C(Color2, 1f - k * .4f)); }
        p.R(nx, 270, 240, 48, 12, p.C(0x3A3E45)); p.L(nx - 28, 245, -115, -260, p.C(0xB9BEC6), 18); p.R(-115, -285, 98, 40, 14, p.C(0x3A3E45));
        var hose = new List<Vector2>(); for (int i = 0; i <= 26; i++) { float e = i / 26f; hose.Add(new(-120 - e * 220 + ArtPen.Sin(e * MathF.PI) * -60f, -270 + e * 420 - ArtPen.Sin(e * MathF.PI) * 170f + ArtPen.Sin(e * 18f + t * 3f) * 3f)); } p.Line(p.C(0x50555C), 26, hose); for (int i = 1; i < 26; i += 2) { var q = hose[i]; p.D(q.X, q.Y, 9, p.C(0x70757B)); }
        p.R(-470, 180, 340, 210, 96, p.C(Color1)); p.R(-470, 130, 180, 60, 26, p.C(0xBCD2FF)); foreach (float x in new[] { -575f, -365f }) { p.D(x, 268, 42, p.C(0x20242B)); p.D(x, 268, 15, p.C(0xB9BEC6)); }
    }
}

public sealed class BoomboxPlugin : QuickShapePlugin { public override string Name => "演出：ラジカセ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BoomboxParameter(s); }
public sealed class BoomboxParameter : Q4Art
{
    public BoomboxParameter() : this(null) { }
    public BoomboxParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x3A3E48); Color2 = Hex(0xFFD54A); }
    protected override string[] Use => ["Color1=本体の色", "Color2=音符の色", "Speed=リズムの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float beat = MathF.Pow(MathF.Abs(ArtPen.Sin(t * 3.4f)), 5f), s0 = 1f + beat * .025f; p.Zoom(0, 180, s0, s0, () =>
        {
            p.Arc(0, -175, 330, MathF.PI, MathF.Tau, p.C(0x8A8F98), 18); p.R(0, 40, 1240, 500, 34, p.C(Color1)); p.RL(0, 40, 1240, 500, 34, p.C(0x6A6F7A), 5);
            foreach (int s in new[] { -1, 1 }) { float x = s * 410; p.D(x, 70, 158, p.C(0x17181C)); p.Ring(x, 70, 158, p.C(0xB9BEC6), 8); p.D(x, 70, 105 + beat * 18, p.C(0x2C2F36)); p.D(x, 70, 44 + beat * 12, p.C(0xB9BEC6)); for (int a = 0; a < 16; a++) { float q = a * MathF.Tau / 16f; p.L(x + ArtPen.Cos(q) * 118, 70 + ArtPen.Sin(q) * 118, x + ArtPen.Cos(q) * 148, 70 + ArtPen.Sin(q) * 148, p.C(0x555963), 3); } }
            p.R(0, 90, 400, 245, 12, p.C(0x17181C)); p.R(0, 90, 370, 212, 8, p.C(0xE7DEC8)); p.B(0, 30, 330, 42, p.C(0xE5533D)); foreach (int s in new[] { -1, 1 }) { float x = s * 90; p.D(x, 125, 50, p.C(0x2C2F36)); p.Turn(x, 125, t * 4f * s, () => { for (int k = 0; k < 6; k++) { float q = k * MathF.Tau / 6f; p.L(x + ArtPen.Cos(q) * 12, 125 + ArtPen.Sin(q) * 12, x + ArtPen.Cos(q) * 36, 125 + ArtPen.Sin(q) * 36, p.C(0xFFFFFF), 6); } }); }
            for (int i = 0; i < 18; i++) { float h = (.2f + MathF.Abs(ArtPen.Sin(t * 5f + i * .9f)) * .8f) * 50f; p.B(-170 + i * 20, -130 - h / 2f, 13, h, p.C(0x5CFFB0)); }
        });
        for (int i = 0; i < 8; i++) { float k = ArtPen.Fr(t * .3f + i / 8f), side = i % 2 == 0 ? -1 : 1, x = side * (250 + k * 340) + ArtPen.Sin(k * 8f + i) * 26, y = -230 - k * 240; p.E(x, y, 22, 16, p.C(Color2, 1f - k)); p.L(x + 19, y, x + 19, y - 62, p.C(Color2, 1f - k), 6); p.L(x + 19, y - 62, x + 44, y - 42, p.C(Color2, 1f - k), 6); }
    }
}

public sealed class FlashlightBeamPlugin : QuickShapePlugin { public override string Name => "演出：懐中電灯"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FlashlightBeamParameter(s); }
public sealed class FlashlightBeamParameter : Q4Art
{
    public FlashlightBeamParameter() : this(null) { }
    public FlashlightBeamParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFF2B0); Color2 = Hex(0x3A3E45); }
    protected override string[] Use => ["Color1=光の色", "Color2=本体の色", "Speed=動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float bx = -560, by = 240; float a = -.22f + ArtPen.Sin(t * .9f) * .26f, L = 1250f, sp = .19f; var o = p.V(bx, by); var e = p.V(bx + ArtPen.Cos(a) * L, by + ArtPen.Sin(a) * L);
        using (var gb = Gfx.Linear(p.Dc, o, e, (0f, p.C(Color1, .68f)), (1f, p.C(Color1, 0f)))) { var q = new List<Vector2> { p.V(bx + ArtPen.Cos(a + MathF.PI / 2f) * 48, by + ArtPen.Sin(a + MathF.PI / 2f) * 48), p.V(bx + ArtPen.Cos(a + sp) * L, by + ArtPen.Sin(a + sp) * L), p.V(bx + ArtPen.Cos(a - sp) * L, by + ArtPen.Sin(a - sp) * L), p.V(bx + ArtPen.Cos(a - MathF.PI / 2f) * 48, by + ArtPen.Sin(a - MathF.PI / 2f) * 48) }; Gfx.FillPoly(p.Dc, gb, q); }
        p.Turn(bx, by, a, () => { p.F(p.C(Color2), bx - 62, by - 48, bx, by - 64, bx, by + 64, bx - 62, by + 48); p.R(bx - 195, by, 275, 76, 18, p.C(Color2)); p.B(bx + 3, by, 11, 118, p.C(Color1)); p.R(bx - 200, by - 38, 74, 16, 6, p.C(0xE53935)); for (int i = 0; i < 6; i++) p.L(bx - 300 + i * 18, by - 27, bx - 300 + i * 18, by + 27, p.C(0x70757B), 3); });
    }
}

public sealed class FaucetWaterPlugin : QuickShapePlugin { public override string Name => "演出：蛇口の水"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FaucetWaterParameter(s); }
public sealed class FaucetWaterParameter : Q4Art
{
    public FaucetWaterParameter() : this(null) { }
    public FaucetWaterParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xC7CDD4); Color2 = Hex(0x6FD0FF); Count = 28; }
    protected override string[] Use => ["Color1=蛇口の色", "Color2=水の色", "Count=しぶきの量||0|100", "Speed=流れる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float wx = 180, top = -130, bot = 300; for (int i = 0; i < 3; i++) { var q = new List<Vector2>(); for (int k = 0; k <= 20; k++) { float e = k / 20f; q.Add(new(wx + (i - 1) * 13 * (1f - e * .45f) + ArtPen.Sin(e * 18f - t * 16f + i) * 3.5f, top + e * (bot - top))); } p.Line(p.C(Color2, i == 1 ? .95f : .6f), i == 1 ? 15 : 8, q); }
        p.E(wx, bot + 6, 230, 34, p.C(Color2, .42f)); for (int r = 0; r < 3; r++) { float k = ArtPen.Fr(t * 1.1f + r / 3f); p.EL(wx, bot + 6, 40 + k * 195, 8 + k * 28, p.C(0xFFFFFF, .7f * (1f - k)), 3); }
        for (int i = 0; i < Math.Clamp(Count, 0, 100); i++) { float k = ArtPen.Fr(t * (1f + ArtPen.R01(i)) + ArtPen.R01(i + 3)), dir = (ArtPen.R01(i + 5) - .5f) * 2f, h = 60f + ArtPen.R01(i + 8) * 110f; p.D(wx + dir * 150f * k, bot - ArtPen.Sin(k * MathF.PI) * h, (2f + ArtPen.R01(i + 9) * 5f) * (1f - k * .5f), p.C(Color2, 1f - k)); }
        p.R(-210, -220, 440, 86, 18, p.C(Color1)); p.R(160, -175, 92, 92, 14, p.C(Color1)); p.R(172, -125, 104, 28, 8, p.C(Gfx.Mix(Color1, Hex(0x000000), .2f))); p.R(-410, -220, 70, 165, 12, p.C(Color1)); p.R(-20, -292, 58, 76, 8, p.C(Color1)); p.R(-20, -336, 190, 34, 15, p.C(0xE53935)); p.L(-360, -245, 110, -245, p.C(0xFFFFFF, .65f), 8);
    }
}

public sealed class HairDryerPlugin : QuickShapePlugin { public override string Name => "演出：ドライヤー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HairDryerParameter(s); }
public sealed class HairDryerParameter : Q4Art
{
    public HairDryerParameter() : this(null) { }
    public HairDryerParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFF8FB1); Color2 = Hex(0xFFFFFF); }
    protected override string[] Use => ["Color1=本体の色", "Color2=風の色", "Speed=風の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        for (int i = 0; i < 12; i++) { float k = ArtPen.Fr(t * 1.2f + ArtPen.R01(i)), y = -60 + (i - 5.5f) * (11f + k * 36f), x = 120 + k * 560; var q = new List<Vector2>(); for (int s = 0; s <= 10; s++) { float e = s / 10f; q.Add(new(x + e * 150, y + ArtPen.Sin(e * 6f + t * 8f + i) * 11f)); } p.Line(p.C(Color2, .68f * (1f - k)), 5, q); }
        p.R(-155, 180, 104, 345, 40, p.C(Color1)); p.R(-155, 215, 76, 36, 10, p.C(0xFFFFFF, .75f)); p.P(p.C(0x50555C), 9, -155, 352, -135, 410, -190, 450, -280, 420); p.R(-120, -60, 430, 220, 100, p.C(Color1)); p.F(p.C(Gfx.Mix(Color1, Hex(0x000000), .18f)), 75, -158, 195, -112, 195, -8, 75, 38); p.D(-300, -60, 104, p.C(0x50555C)); p.D(-300, -60, 82, p.C(0x2B2E33)); for (int b = 0; b < 5; b++) { float a = t * 18f + b * MathF.Tau / 5f; p.L(-300, -60, -300 + ArtPen.Cos(a) * 72, -60 + ArtPen.Sin(a) * 72, p.C(0xB9BEC6, .85f), 13); } p.D(-300, -60, 14, p.C(Color1)); p.L(-200, -130, 40, -130, p.C(0xFFFFFF, .55f), 10);
    }
}

public sealed class RotaryPhonePlugin : QuickShapePlugin { public override string Name => "演出：黒電話"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RotaryPhoneParameter(s); }
public sealed class RotaryPhoneParameter : Q4Art
{
    public RotaryPhoneParameter() : this(null) { }
    public RotaryPhoneParameter(SharedDataStore? s) : base(s) { Text = "ジリリリ"; Color1 = Hex(0x17181C); Color2 = Hex(0xE8E4D8); }
    protected override string[] Use => ["Text=ベルの音", "Color1=本体の色", "Color2=ダイヤルの色", "Speed=鳴る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        bool ring = ArtPen.Fr(t * .35f) < .55f; float sh = ring ? ArtPen.Sin(t * 58f) * 7f : 0f;
        p.Move(sh, 0, () =>
        {
            p.F(p.C(Color1), -420, 310, 420, 310, 330, -60, 215, -160, -215, -160, -330, -60); p.L(-400, 300, -310, -55, p.C(0x50545C), 5); p.Turn(0, -240, sh * .007f, () => { p.R(0, -255, 780, 90, 40, p.C(Color1)); foreach (int s in new[] { -1, 1 }) p.R(s * 345, -210, 170, 130, 44, p.C(Color1)); p.L(-300, -282, 300, -282, p.C(0x50545C), 5); }); foreach (int s in new[] { -1, 1 }) p.B(s * 170, -175, 50, 60, p.C(0x2A2C31));
            const float cy = 110, R = 180; p.D(0, cy, R, p.C(0xB9BEC6)); float dial = ArtPen.Sin(t * 1.1f) > .25f ? (ArtPen.Sin(t * 1.1f) - .25f) * 1.5f : 0f; p.D(0, cy, R - 10, p.C(Color2)); for (int i = 0; i < 10; i++) { float a = -2.5f + i * .52f; p.T(((i + 1) % 10).ToString(), 26, ArtPen.Cos(a) * (R - 40), cy + ArtPen.Sin(a) * (R - 40), p.C(0x202024)); }
            p.Turn(0, cy, dial, () => { p.D(0, cy, R - 66, p.C(0x20242B)); for (int i = 0; i < 10; i++) { float a = -2.5f + i * .52f; p.D(ArtPen.Cos(a) * (R - 40), cy + ArtPen.Sin(a) * (R - 40), 19, p.C(0xFFFFFF, .25f)); } }); p.D(0, cy, 48, p.C(0xE5202E)); p.D(0, cy, 22, p.C(0xFFFFFF, .85f)); p.L(R - 12, cy + 70, R + 18, cy + 110, p.C(0xB9BEC6), 8);
        });
        if (ring) { for (int s = -1; s <= 1; s += 2) for (int i = 0; i < 3; i++) { float k = ArtPen.Fr(t * 2.4f + i / 3f); p.Arc(s * 470, -230, 40 + k * 75, s > 0 ? -.75f : MathF.PI - .75f, s > 0 ? .75f : MathF.PI + .75f, p.C(0xFFE45C, 1f - k), 7); } p.T(Str, 76, ArtPen.Sin(t * 37f) * 7f, -405, p.C(0xFFE45C), p.C(0x000000), 5); }
    }
}
