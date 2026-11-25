using System.Numerics;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 671〜700: 解説図・建物・飾り

internal static class Art6
{
    public static void Arrow(ArtPen p, float x0, float y0, float x1, float y1, Color4 c, float w)
    {
        var d = new Vector2(x1 - x0, y1 - y0); float l = d.Length(); if (l < .5f) return; d /= l; var n = new Vector2(-d.Y, d.X); float h = w * 3f;
        p.L(x0, y0, x1 - d.X * h, y1 - d.Y * h, c, w); p.F(c, x1, y1, x1 - d.X * h * 1.5f + n.X * h, y1 - d.Y * h * 1.5f + n.Y * h, x1 - d.X * h * 1.5f - n.X * h, y1 - d.Y * h * 1.5f - n.Y * h);
    }
    public static void Sky(ArtPen p, uint top, uint low) { using var g = Gfx.Linear(p.Dc, p.V(0, -p.H / 2f), p.V(0, p.H / 2f), (0f, p.C(top)), (1f, p.C(low))); var a = p.V(-p.W / 2f - 2, -p.H / 2f - 2); var b = p.V(p.W / 2f + 2, p.H / 2f + 2); p.Dc.FillRectangle(new Vortice.RawRectF(a.X, a.Y, b.X, b.Y), g); }
    public static void Hills(ArtPen p, float y, float amp, float f, float ph, Color4 c) { var q = new List<Vector2> { new(-p.W, p.H), new(p.W, p.H) }; for (int i = 48; i >= 0; i--) { float x = -p.W + i / 48f * p.W * 2f; q.Add(new(x, y + MathF.Sin(x * f + ph) * amp + MathF.Sin(x * f * 2.3f + ph * 2f) * amp * .3f)); } p.Fill(c, q); }
    public static void Cloud(ArtPen p, float x, float y, float s, Color4 c) { foreach (var (a, b, r) in new[] { (-70f, 10f, 44f), (-22f, -14f, 62f), (40f, -4f, 50f), (88f, 14f, 34f) }) p.D(x + a * s, y + b * s, r * s, c); p.R(x + 8 * s, y + 28 * s, 226 * s, 44 * s, 22 * s, c); }
}

public sealed class LightRefractionPlugin : QuickShapePlugin { public override string Name => "演出：光の屈折"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LightRefractionParameter(s); }
public sealed class LightRefractionParameter : Q4Art
{
    public LightRefractionParameter() : this(null) { }
    public LightRefractionParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFE45C); Color2 = Hex(0x4FA8E8); }
    protected override double DefAmount => 45;
    protected override string[] Use => ["Amount=入る角度|0〜100 = 0〜80度", "Color1=光の色", "Color2=水の色", "Speed=光の流れる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.B(0, 190, 1240, 380, p.C(Color2, .62f)); p.L(-620, 0, 620, 0, p.C(0xFFFFFF), 4); for (float y = -380; y < 380; y += 34) p.L(0, y, 0, y + 18, p.C(0xFFFFFF, .65f), 3);
        float a1 = v * 80f * MathF.PI / 180f, a2 = MathF.Asin(MathF.Sin(a1) / 1.33f), L = 410; float ix = -MathF.Sin(a1) * L, iy = -MathF.Cos(a1) * L, ox = MathF.Sin(a2) * L, oy = MathF.Cos(a2) * L, rx = MathF.Sin(a1) * L, ry = iy;
        p.L(ix, iy, 0, 0, p.C(Color1), 11); p.L(0, 0, ox, oy, p.C(Color1), 11); p.L(0, 0, rx, ry, p.C(Color1, .28f), 6);
        for (int i = 0; i < 7; i++) { float k = ArtPen.Fr(t * .5f + i / 7f); if (k < .5f) { float e = k * 2f; p.D(ix * (1f - e), iy * (1f - e), 11, p.C(0xFFFFFF)); } else { float e = (k - .5f) * 2f; p.D(ox * e, oy * e, 11, p.C(0xFFFFFF)); } }
        p.Arc(0, 0, 130, -MathF.PI / 2f - a1, -MathF.PI / 2f, p.C(0xFFFFFF), 4); p.Arc(0, 0, 130, MathF.PI / 2f - a2, MathF.PI / 2f, p.C(0xFFFFFF), 4); p.T("入射角", 34, -150, -180, p.C(0xFFFFFF), p.C(0x000000, .6f), 3); p.T("屈折角", 34, 150, 200, p.C(0xFFFFFF), p.C(0x000000, .6f), 3); p.T("空気", 42, 500, -70, p.C(0xFFFFFF)); p.T("水", 42, 500, 70, p.C(0xFFFFFF));
    }
}

public sealed class MagnetFieldPlugin : QuickShapePlugin { public override string Name => "演出：磁石と磁力線"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MagnetFieldParameter(s); }
public sealed class MagnetFieldParameter : Q4Art
{
    public MagnetFieldParameter() : this(null) { }
    public MagnetFieldParameter(SharedDataStore? s) : base(s) { Count = 5; Color1 = Hex(0xE53935); Color2 = Hex(0x2F6FE0); Color3 = Hex(0xFFE45C); }
    protected override string[] Use => ["Count=線の数(片側)||1|10", "Color1=N極の色", "Color2=S極の色", "Color3=線の色", "Speed=流れる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        int n = Math.Clamp(Count, 1, 10); foreach (int side in new[] { -1, 1 }) for (int l = 0; l < n; l++)
        {
            float h = 95f + (l + 1f) / n * 300f, w = 300f + (l + 1f) / n * 200f; var q = new List<Vector2>(); for (int i = 0; i <= 44; i++) { float e = i / 44f, a = MathF.PI * e; q.Add(new(-MathF.Cos(a) * w * -1f, side * (MathF.Sin(a) * h + 30f * (1f - MathF.Sin(a))))); }
            p.Line(p.C(Color3, .75f), 4, q); for (int m = 0; m < 3; m++) { int idx = (int)(ArtPen.Fr(t * .25f + m / 3f + l * .07f) * 43f); var a = q[idx]; var b = q[idx + 1]; var d = Vector2.Normalize(b - a + new Vector2(.001f, 0)); var nn = new Vector2(-d.Y, d.X); p.F(p.C(Color3), a.X + d.X * 15, a.Y + d.Y * 15, a.X - d.X * 8 + nn.X * 10, a.Y - d.Y * 8 + nn.Y * 10, a.X - d.X * 8 - nn.X * 10, a.Y - d.Y * 8 - nn.Y * 10); }
        }
        p.B(-155, 0, 310, 150, p.C(Color2)); p.B(155, 0, 310, 150, p.C(Color1)); p.T("S", 92, -155, 0, p.C(0xFFFFFF)); p.T("N", 92, 155, 0, p.C(0xFFFFFF));
    }
}

public sealed class WaterCyclePlugin : QuickShapePlugin { public override string Name => "演出：水の循環"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WaterCycleParameter(s); }
public sealed class WaterCycleParameter : Q4Art
{
    public WaterCycleParameter() : this(null) { }
    public WaterCycleParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0x2F86C6); Color2 = Hex(0x5EA84B); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Color1=海の色", "Color2=陸の色", "Speed=めぐる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x8FD3F4, 0xE8F7FF); p.D(-700, -390, 80, p.C(0xFFD54A)); for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12f + t * .15f; p.L(-700 + ArtPen.Cos(a) * 96, -390 + ArtPen.Sin(a) * 96, -700 + ArtPen.Cos(a) * 130, -390 + ArtPen.Sin(a) * 130, p.C(0xFFD54A), 7); }
        p.F(p.C(Color2), -30, 560, 980, 560, 980, -160, 720, -310, 590, -160, 410, -40, 240, 80, 80, 210); p.F(p.C(0xFFFFFF), 720, -310, 790, -245, 735, -230, 700, -205, 665, -240); var rv = new List<Vector2>(); for (int i = 0; i <= 26; i++) { float e = i / 26f; rv.Add(new(710 - e * 700 + ArtPen.Sin(e * 7f) * 34, -210 + e * 495)); } p.Line(p.C(Color1), 18, rv); p.B(-500, 440, 1000, 260, p.C(Color1)); p.F(p.C(Color1), -1000, 250, 110, 250, 40, 570, -1000, 570);
        for (int i = 0; i < 18; i++) { float k = ArtPen.Fr(t * .22f + ArtPen.R01(i)), x = -780 + ArtPen.R01(i + 4) * 620 + ArtPen.Sin(k * 7f + i) * 22; p.P(p.C(0xFFFFFF, .75f * (1f - k)), 4, x, 240 - k * 440, x + 8, 222 - k * 440, x - 6, 205 - k * 440); }
        float dr = ArtPen.Fr(t * .05f); Art6.Cloud(p, -270 + dr * 120, -335, 1.5f, p.C(0xFFFFFF)); Art6.Cloud(p, 330 + dr * 90, -345, 1.8f, p.C(0xD7DEE8)); for (int i = 0; i < 26; i++) { float k = ArtPen.Fr(t * .9f + ArtPen.R01(i)), x = 170 + ArtPen.R01(i + 7) * 330; p.L(x, -250 + k * 300, x - 5, -224 + k * 300, p.C(Color1, 1f - k * .4f), 4); }
        Art6.Arrow(p, -420, 190, -420, -190, p.C(0xFF7043), 11); Art6.Arrow(p, -50, -390, 145, -390, p.C(0x546E7A), 11); Art6.Arrow(p, 620, -60, 300, 210, p.C(0x1F5FA8), 11);
        p.T("蒸発", 46, -520, 40, p.C(0xFFFFFF), p.C(0x8A3A20), 4); p.T("雲", 46, -270, -335, p.C(0x37474F)); p.T("雨", 46, 335, -40, p.C(0xFFFFFF), p.C(0x1F5FA8), 4); p.T("川", 46, 505, 170, p.C(0xFFFFFF), p.C(0x1F5FA8), 4); p.T("海", 56, -600, 420, p.C(0xFFFFFF));
    }
}

public sealed class FoodChainPlugin : QuickShapePlugin { public override string Name => "演出：食物連鎖"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FoodChainParameter(s); }
public sealed class FoodChainParameter : Q4Art
{
    public FoodChainParameter() : this(null) { }
    public FoodChainParameter(SharedDataStore? s) : base(s) { Text = "大型の肉食動物\n小型の肉食動物\n草食動物\n植物"; Color1 = Hex(0xE85D4A); Color2 = Hex(0x5EA84B); }
    protected override string[] Use => ["Text=段の名前|上から1行に1つ", "Color1=上の段の色", "Color2=下の段の色", "Speed=出る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        var r = Rows; int n = Math.Clamp(r.Count, 2, 7); const float H = 720; float rh = H / n;
        for (int i = n - 1; i >= 0; i--)
        {
            float k = DrawKit.EaseOutBack(ArtPen.Cl((t - (n - 1 - i) * .3f) * 2.5f)), w0 = 620f * i / n, w1 = 620f * (i + 1f) / n, y = -H / 2f + i * rh; if (k <= 0f) continue; var col = Gfx.Mix(Color1, Color2, i / (n - 1f)); float my = y + rh / 2f; p.Zoom(0, my, k, k, () => { p.F(p.C(col), -w0, y + 3, w0, y + 3, w1, y + rh - 3, -w1, y + rh - 3); p.T(r[i], i == 0 ? 25 : 38, 0, my + (i == 0 ? rh * .22f : 0), p.C(0xFFFFFF), p.C(0x000000, .45f), 3, MathF.Max(120, w0 + w1 - 30)); });
            if (i < n - 1) { float a = ArtPen.Cl((t - (n - 1 - i) * .3f - .25f) * 3f); Art6.Arrow(p, 760, y + rh * 1.4f, 760, y + rh * .6f, p.C(0xFFFFFF, a), 9); p.T("食べられる", 28, 880, y + rh, p.C(0xFFFFFF, a)); }
        }
    }
}

public sealed class PendulumMotionPlugin : QuickShapePlugin { public override string Name => "演出：振り子の運動"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PendulumMotionParameter(s); }
public sealed class PendulumMotionParameter : Q4Art
{
    public PendulumMotionParameter() : this(null) { }
    public PendulumMotionParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xE53935); Color2 = Hex(0x4FC3F7); }
    protected override double DefAmount => 45;
    protected override string[] Use => ["Amount=振れ幅", "Color1=おもりの色", "Color2=速さの矢印の色", "Speed=振れる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float L = 600, py = -340; float th0 = .15f + v * .85f, w = 2f, th = th0 * ArtPen.Cos(t * w), om = -th0 * w * ArtPen.Sin(t * w); p.B(0, py - 12, 520, 24, p.C(0x70757B)); p.L(0, py, 0, py + L + 70, p.C(0xFFFFFF, .35f), 3); var arc = new List<Vector2>(); for (int i = 0; i <= 40; i++) { float a = -th0 + i / 40f * th0 * 2f; arc.Add(new(MathF.Sin(a) * L, py + MathF.Cos(a) * L)); } p.Line(p.C(0xFFFFFF, .45f), 3, arc);
        for (int g = 1; g <= 6; g++) { float o = th0 * ArtPen.Cos((t - g * .06f) * w); p.D(MathF.Sin(o) * L, py + MathF.Cos(o) * L, 50, p.C(Color1, .16f * (1f - g / 7f))); } float x = MathF.Sin(th) * L, y = py + MathF.Cos(th) * L; p.L(0, py, x, y, p.C(0xE8E8E8), 6); p.D(0, py, 13, p.C(0xB9BEC6)); p.D(x, y, 54, p.C(Color1)); p.D(x - 16, y - 18, 13, p.C(0xFFFFFF, .65f));
        float vx = MathF.Cos(th) * om * 130f, vy = -MathF.Sin(th) * om * 130f; if (MathF.Abs(om) > .05f) { float sx = x + MathF.Sign(vx) * 58f * MathF.Cos(th), sy = y - MathF.Sign(vx) * 58f * MathF.Sin(th); Art6.Arrow(p, sx, sy, sx + vx, sy + vy, p.C(Color2), 9); }
        p.T($"速さ {MathF.Abs(om) / (th0 * w) * 100f:0}%", 42, 470, -240, p.C(Color2)); p.T($"高さ {(1f - MathF.Cos(th)) / MathF.Max(.001f, 1f - MathF.Cos(th0)) * 100f:0}%", 42, 470, -175, p.C(0xFFD54A));
    }
}

public sealed class UnitCircleTrigPlugin : QuickShapePlugin { public override string Name => "演出：単位円と三角関数"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new UnitCircleTrigParameter(s); }
public sealed class UnitCircleTrigParameter : Q4Art
{
    public UnitCircleTrigParameter() : this(null) { }
    public UnitCircleTrigParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFF5C5C); Color2 = Hex(0x4FC3F7); Color3 = Hex(0xFFD54A); }
    protected override string[] Use => ["Color1=sinの色", "Color2=cosの色", "Color3=点の色", "Speed=回る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float R = 260, cx = -430; float a = t * .9f, c = ArtPen.Cos(a), s = ArtPen.Sin(a), px = cx + c * R, py = -s * R; p.L(cx - 340, 0, cx + 340, 0, p.C(0xFFFFFF, .75f), 3); p.L(cx, -340, cx, 340, p.C(0xFFFFFF, .75f), 3); p.Ring(cx, 0, R, p.C(0xFFFFFF), 5); p.Pie(cx, 0, 64, -Gfx.Mod(a, MathF.Tau), 0, p.C(Color3, .55f)); p.L(cx, 0, px, py, p.C(0xFFFFFF), 6); p.L(px, 0, px, py, p.C(Color1), 9); p.L(cx, 0, px, 0, p.C(Color2), 9);
        const float gx = -60, gw = 880; p.L(gx, 0, gx + gw, 0, p.C(0xFFFFFF, .7f), 3); p.L(gx, -300, gx, 300, p.C(0xFFFFFF, .7f), 3); var sn = new List<Vector2>(); var cs = new List<Vector2>(); for (int i = 0; i <= 120; i++) { float e = i / 120f, q = a - e * MathF.Tau * 1.5f; sn.Add(new(gx + e * gw, -ArtPen.Sin(q) * R)); cs.Add(new(gx + e * gw, -ArtPen.Cos(q) * R)); } p.Line(p.C(Color2, .75f), 5, cs); p.Line(p.C(Color1), 6, sn); for (float x = cx; x < gx; x += 24) { float nx = MathF.Min(gx, x + 12); if (x >= px) p.L(x, py, nx, py, p.C(Color1, .7f), 3); } p.L(px, py, gx, py, p.C(Color1, .25f), 2); p.D(gx, py, 13, p.C(Color1)); p.D(px, py, 15, p.C(Color3));
        p.T($"sin = {s:0.00}", 44, 300, -380, p.C(Color1)); p.T($"cos = {c:0.00}", 44, 650, -380, p.C(Color2));
    }
}

public sealed class PythagorasPlugin : QuickShapePlugin { public override string Name => "演出：三平方の定理"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PythagorasParameter(s); }
public sealed class PythagorasParameter : Q4Art
{
    public PythagorasParameter() : this(null) { }
    public PythagorasParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFF7A59); Color2 = Hex(0x4FC3F7); Color3 = Hex(0xFFD54A); }
    protected override string[] Use => ["Color1=aの色", "Color2=bの色", "Color3=cの色", "Speed=出る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float a = 180, b = 240; var A = new Vector2(-120, 110); var B = A + new Vector2(b, 0); var C = A + new Vector2(0, -a); float k1 = ArtPen.Cl((t - .35f) * 2.5f), k2 = ArtPen.Cl((t - .75f) * 2.5f), k3 = ArtPen.Cl((t - 1.15f) * 2.5f);
        void Sq(Vector2 P, Vector2 Q, float k, Color4 col, string label) { var d = Q - P; var n = new Vector2(d.Y, -d.X) * k; var P2 = P + n; var Q2 = Q + n; p.F(col, P.X, P.Y, Q.X, Q.Y, Q2.X, Q2.Y, P2.X, P2.Y); var m = (P + Q + P2 + Q2) / 4f; if (k >= 1f) p.T(label, 58, m.X, m.Y, p.C(0xFFFFFF), p.C(0x000000, .5f), 4); }
        if (k1 > 0f) Sq(A, C, k1, p.C(Color1, .9f), "a²"); if (k2 > 0f) Sq(B, A, k2, p.C(Color2, .9f), "b²"); if (k3 > 0f) Sq(C, B, k3, p.C(Color3, .9f), "c²");
        p.F(p.C(0xFFFFFF), A.X, A.Y, B.X, B.Y, C.X, C.Y); p.P(p.C(0x263238), 5, A.X, A.Y, B.X, B.Y, C.X, C.Y, A.X, A.Y); p.P(p.C(0x263238), 3, A.X + 26, A.Y, A.X + 26, A.Y - 26, A.X, A.Y - 26); p.T("a", 40, A.X + 24, A.Y - 92, p.C(0x263238)); p.T("b", 40, A.X + 125, A.Y - 25, p.C(0x263238)); p.T("c", 40, A.X + 138, A.Y - 112, p.C(0x263238));
        float f = ArtPen.Cl((t - 1.6f) * 3f); if (f > 0f) p.T("a² + b² = c²", 84, 0, 450, p.C(0xFFFFFF, f), p.C(0x000000, .6f * f), 5);
    }
}

public sealed class SupplyDemandPlugin : QuickShapePlugin { public override string Name => "演出：需要と供給の曲線"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SupplyDemandParameter(s); }
public sealed class SupplyDemandParameter : Q4Art
{
    public SupplyDemandParameter() : this(null) { }
    public SupplyDemandParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x4FC3F7); Color2 = Hex(0xFF7A59); Color3 = Hex(0xFFD54A); }
    protected override double DefAmount => 50;
    protected override double DefAmount2 => 50;
    protected override string[] Use => ["Amount=需要の強さ|動かすと線が左右に移る", "Amount2=供給の強さ|動かすと線が左右に移る", "Color1=需要の色", "Color2=供給の色", "Color3=均衡点の色", "Speed=描く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float x0 = -480, y0 = 330, W = 960, H = 660; Art6.Arrow(p, x0, y0, x0 + W + 40, y0, p.C(0xFFFFFF), 5); Art6.Arrow(p, x0, y0, x0, y0 - H - 40, p.C(0xFFFFFF), 5); p.T("数量", 38, x0 + W + 90, y0, p.C(0xFFFFFF)); p.T("価格", 38, x0, y0 - H - 72, p.C(0xFFFFFF));
        float d = (v - .5f) * .5f, s = (v2 - .5f) * .5f, k = ArtPen.Cl(t * .9f); float Dm(float q) => 1f - (q - d) * .85f; float Sp(float q) => .08f + (q - s) * .85f; var a = new List<Vector2>(); var b = new List<Vector2>(); for (int i = 0; i <= 50 * k; i++) { float q = .04f + i / 50f * .92f; float dv = Math.Clamp(Dm(q), 0f, 1f), sv = Math.Clamp(Sp(q), 0f, 1f); a.Add(new(x0 + q * W, y0 - dv * H)); b.Add(new(x0 + q * W, y0 - sv * H)); }
        if (a.Count >= 2) { p.Line(p.C(Color1), 8, a); p.Line(p.C(Color2), 8, b); p.T("需要", 42, a[^1].X + 55, a[^1].Y, p.C(Color1)); p.T("供給", 42, b[^1].X + 55, b[^1].Y, p.C(Color2)); }
        if (k >= 1f) { float qe = (1f + d * .85f - .08f + s * .85f) / 1.7f, pe = Dm(qe), ex = x0 + qe * W, ey = y0 - pe * H, f = ArtPen.Cl((p.Now - 1.1f) * 3f); for (float yy = ey; yy < y0; yy += 26) p.L(ex, yy, ex, MathF.Min(y0, yy + 13), p.C(Color3, f), 3); for (float xx = x0; xx < ex; xx += 26) p.L(xx, ey, MathF.Min(ex, xx + 13), ey, p.C(Color3, f), 3); p.Ring(ex, ey, 21 + ArtPen.Sin(t * 5f) * 4f, p.C(Color3, f), 5); p.D(ex, ey, 11, p.C(Color3, f)); p.T("均衡点", 40, ex + 105, ey - 42, p.C(Color3, f), p.C(0x000000, .6f * f), 3); }
    }
}

public sealed class PhotosynthesisPlugin : QuickShapePlugin { public override string Name => "演出：光合成"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PhotosynthesisParameter(s); }
public sealed class PhotosynthesisParameter : Q4Art
{
    public PhotosynthesisParameter() : this(null) { }
    public PhotosynthesisParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x4FAE47); Color2 = Hex(0xFFD54A); }
    protected override string[] Use => ["Color1=葉の色", "Color2=光の色", "Speed=流れる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        var leaf = new List<Vector2>(); for (int i = 0; i <= 40; i++) { float e = i / 40f; leaf.Add(new(-350 + e * 700, 40 - ArtPen.Sin(e * MathF.PI) * 210f - e * 40)); } for (int i = 40; i >= 0; i--) { float e = i / 40f; leaf.Add(new(-350 + e * 700, 40 + ArtPen.Sin(e * MathF.PI) * 210f - e * 40)); } p.Fill(p.C(Color1), leaf); p.L(-350, 40, 350, 0, p.C(0xBDE8A8), 7); for (int i = 1; i < 7; i++) { float x = -350 + i * 100, y = 40 - i * 5.7f, h = ArtPen.Sin(i / 7f * MathF.PI) * 165f; p.L(x, y, x + 75, y - h, p.C(0xBDE8A8), 3); p.L(x, y, x + 75, y + h, p.C(0xBDE8A8), 3); }
        p.D(-650, -360, 76, p.C(Color2)); for (int i = 0; i < 5; i++) { float k = ArtPen.Fr(t * .6f + i / 5f), sx = -580 + i * 30, sy = -320 + i * 12; p.D(sx + k * 420, sy + k * 270, 9, p.C(Color2, 1f - k * .4f)); } Art6.Arrow(p, -580, -310, -170, -60, p.C(Color2), 10);
        void Flow(float x0, float y0, float x1, float y1, Color4 c, string label, float lx, float ly) { Art6.Arrow(p, x0, y0, x1, y1, c, 9); for (int i = 0; i < 4; i++) { float k = ArtPen.Fr(t * .55f + i / 4f); p.D(x0 + (x1 - x0) * k, y0 + (y1 - y0) * k, 10, p.C(0xFFFFFF, .85f)); } p.T(label, 42, lx, ly, c, p.C(0x000000, .55f), 3); }
        Flow(-650, 300, -210, 130, p.C(0x4FC3F7), "水", -700, 350); Flow(650, -300, 240, -90, p.C(0xB0BEC5), "二酸化炭素", 640, -360); Flow(230, 120, 650, 300, p.C(0x9CFFB0), "酸素", 710, 355); Flow(40, 170, 40, 400, p.C(0xFFB74D), "でんぷん(養分)", 40, 445); p.T("光", 48, -650, -360, p.C(0x7A5A00));
    }
}

public sealed class VolcanoSectionPlugin : QuickShapePlugin { public override string Name => "演出：火山の断面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new VolcanoSectionParameter(s); }
public sealed class VolcanoSectionParameter : Q4Art
{
    public VolcanoSectionParameter() : this(null) { }
    public VolcanoSectionParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x7A5A45); Color2 = Hex(0xFF5A1F); Color3 = Hex(0xFFD54A); }
    protected override string[] Use => ["Color1=地面の色", "Color2=マグマの色", "Color3=明るい部分の色", "Speed=動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.F(p.C(Color1), -760, 420, 760, 420, 760, 120, 390, 95, 95, -270, -95, -270, -390, 95, -760, 120); for (int l = 0; l < 3; l++) p.P(p.C(0x5A4030), 4, -760, 170 + l * 70, -360, 150 + l * 70, 0, 180 + l * 70, 360, 150 + l * 70, 760, 170 + l * 70);
        float gl = .85f + .15f * ArtPen.Sin(t * 4f); p.E(0, 300, 270, 95, p.C(Color2, gl)); p.E(0, 305, 165, 52, p.C(Color3, .8f)); p.F(p.C(Color2, gl), -42, 225, 42, 225, 54, -270, -54, -270); p.F(p.C(Color3, .75f), -15, 230, 15, 230, 20, -270, -20, -270); for (int i = 0; i < 9; i++) { float k = ArtPen.Fr(t * .55f + i / 9f); p.D((ArtPen.R01(i) - .5f) * 54, 240 - k * 500, 9, p.C(0xFFF2A8, 1f - k * .35f)); }
        for (int i = 0; i < 18; i++) { float k = ArtPen.Fr(t * .42f + ArtPen.R01(i)), x = (ArtPen.R01(i + 5) - .5f) * (40 + k * 280) + ArtPen.Sin(k * 6f + i) * 22; p.D(x, -285 - k * 240, 24 + k * 58, p.C(0x70757B, .55f * (1f - k))); } for (int i = 0; i < 16; i++) { float k = ArtPen.Fr(t * .85f + ArtPen.R01(i + 20)), dir = (ArtPen.R01(i + 30) - .5f) * 2f; p.D(dir * k * 260, -280 - ArtPen.Sin(k * MathF.PI) * 240 + k * 110, 7, p.C(Color2, 1f - k * .5f)); }
        void Tag(string s, float x0, float y0, float x1, float y1) { p.L(x0, y0, x1, y1, p.C(0xFFFFFF), 3); p.D(x0, y0, 7, p.C(0xFFFFFF)); p.R(x1 + (x1 > 0 ? 100 : -100), y1, 220, 58, 9, p.C(0x000000, .72f)); p.T(s, 34, x1 + (x1 > 0 ? 100 : -100), y1, p.C(0xFFFFFF), maxW: 205); } Tag("噴煙", 60, -395, 330, -405); Tag("火口", -48, -270, -340, -255); Tag("火道", 25, 40, 390, -20); Tag("マグマだまり", -160, 300, -520, 315);
    }
}

public sealed class JapaneseCastlePlugin : QuickShapePlugin { public override string Name => "演出：日本の城"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new JapaneseCastleParameter(s); }
public sealed class JapaneseCastleParameter : Q4Art
{
    public JapaneseCastleParameter() : this(null) { }
    public JapaneseCastleParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0xF5F2E8); Color2 = Hex(0x3C4A55); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Color1=壁の色", "Color2=屋根の色", "Speed=雲の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x5AAEEA, 0xDDF3FF); for (int i = 0; i < 4; i++) Art6.Cloud(p, ArtPen.Fr(t * .02f + i * .27f) * (p.W + 500) - p.W / 2f - 250, -390 + i * 58, 1.1f + i * .2f, p.C(0xFFFFFF, .92f)); Art6.Hills(p, 330, 30, .004f, 1, p.C(0x5B9A58));
        p.F(p.C(0x8C8D88), -520, 400, 520, 400, 390, 180, -390, 180); for (int r = 0; r < 6; r++) { float y = 190 + r * 37, hw = 390 + r * 23; for (int i = 0; i < 20; i++) { float x = -hw + (i + (r % 2) * .5f) * hw / 10f; p.R(x, y, hw / 10f - 4, 31, 5, p.C(ArtPen.R01(r * 17 + i) > .5f ? 0x9A9B96u : 0x7E7F7Au)); } }
        void Roof(float y, float hw, float h) { p.F(p.C(Color2), -hw - 44, y + 12, -hw, y, -hw * .62f, y - h, hw * .62f, y - h, hw, y, hw + 44, y + 12, hw + 10, y + 28, -hw - 10, y + 28); for (float x = -hw + 20; x < hw; x += 26) p.L(x, y + 26, x * .72f, y - h + 6, p.C(0x25313A), 2); }
        p.B(0, 110, 560, 150, p.C(Color1)); for (int i = -2; i <= 2; i++) p.B(i * 105, 110, 44, 66, p.C(0x20282E)); Roof(35, 330, 85); p.B(0, -80, 400, 118, p.C(Color1)); for (int i = -1; i <= 1; i++) p.B(i * 112, -80, 42, 54, p.C(0x20282E)); Roof(-142, 240, 72); p.B(0, -255, 260, 104, p.C(Color1)); foreach (int i in new[] { -1, 1 }) p.B(i * 58, -255, 40, 48, p.C(0x20282E)); Roof(-305, 160, 78); p.F(p.C(Color2), -66, -378, 66, -378, 0, -462); foreach (int s in new[] { -1, 1 }) { p.P(p.C(0xE6C65A), 8, s * 96, -386, s * 118, -420, s * 96, -440); }
        foreach (int s in new[] { -1, 1 }) { float x = s * 720; p.F(p.C(0x5A3A22), x - 18, 420, x + 18, 420, x + 7, 150, x - 7, 150); for (int l = 0; l < 4; l++) p.E(x + (l % 2 == 0 ? -46 : 42), 215 - l * 58, 118 - l * 15, 40, p.C(l % 2 == 0 ? 0x2F6B3B : 0x3D7D49)); }
    }
}

public sealed class DesertPyramidPlugin : QuickShapePlugin { public override string Name => "演出：ピラミッドと砂漠"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DesertPyramidParameter(s); }
public sealed class DesertPyramidParameter : Q4Art
{
    public DesertPyramidParameter() : this(null) { }
    public DesertPyramidParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0xE6BE74); Color2 = Hex(0xFFD54A); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Color1=砂の色", "Color2=太陽の色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x4FA8E8, 0xFFE9C2); float sb = 1f + ArtPen.Sin(t * 1.4f) * .05f; p.D(520, -300, 145 * sb, p.C(Color2, .24f)); p.D(520, -300, 94, p.C(Color2));
        void Pyr(float x, float base0, float h, float hw) { p.F(p.C(0xD9A95C), x - hw, base0, x + hw * .28f, base0, x, base0 - h); p.F(p.C(0xA9783F), x + hw * .28f, base0, x + hw, base0, x, base0 - h); for (int r = 1; r < 12; r++) { float e = r / 12f, y = base0 - h * e, w = hw * (1f - e); p.L(x - w, y, x + w * .28f, y, p.C(0xB98A48, .7f), 2); p.L(x + w * .28f, y, x + w, y, p.C(0x8A5F30, .7f), 2); } }
        Pyr(-90, 250, 530, 450); Pyr(390, 250, 310, 265); Pyr(-650, 260, 240, 205); Art6.Hills(p, 255, 18, .006f, 2f, p.C(Gfx.Mix(Color1, Hex(0xFFFFFF), .12f))); Art6.Hills(p, 340, 34, .0035f, .4f, p.C(Color1));
        for (int i = 0; i < 12; i++) { float y = 80 + i * 16, x = ArtPen.Fr(t * .12f + ArtPen.R01(i)) * (p.W + 200) - p.W / 2f - 100; p.P(p.C(0xFFFFFF, .22f), 3, x, y, x + 45, y + ArtPen.Sin(t * 4f + i) * 6f, x + 90, y); }
        for (int i = 0; i < 3; i++) { float x = -420 + i * 150 + ArtPen.Fr(t * .012f) * 90, y = 365; p.E(x, y, 54, 20, p.C(0x3B2A1B)); p.D(x - 18, y - 22, 17, p.C(0x3B2A1B)); p.D(x + 16, y - 22, 17, p.C(0x3B2A1B)); p.L(x + 45, y - 7, x + 72, y - 48, p.C(0x3B2A1B), 9); p.E(x + 80, y - 50, 18, 9, p.C(0x3B2A1B)); foreach (int k in new[] { -30, -12, 14, 34 }) p.L(x + k, y + 10, x + k + ArtPen.Sin(t * 3f + k) * 5f, y + 58, p.C(0x3B2A1B), 5); }
    }
}

public sealed class WindmillHillPlugin : QuickShapePlugin { public override string Name => "演出：風車の丘"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WindmillHillParameter(s); }
public sealed class WindmillHillParameter : Q4Art
{
    public WindmillHillParameter() : this(null) { }
    public WindmillHillParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0xF4E8D3); Color2 = Hex(0xB5503C); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Color1=風車の壁の色", "Color2=屋根の色", "Speed=回る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x63B8F0, 0xE6F7FF); for (int i = 0; i < 3; i++) Art6.Cloud(p, ArtPen.Fr(t * .018f + i * .34f) * (p.W + 500) - p.W / 2f - 250, -395 + i * 74, 1.2f, p.C(0xFFFFFF, .92f)); Art6.Hills(p, 145, 44, .003f, 1.2f, p.C(0x7CBF63));
        void Mill(float x, float y, float S) { p.F(p.C(Color1), x - 92 * S, y + 280 * S, x + 92 * S, y + 280 * S, x + 58 * S, y, x - 58 * S, y); p.F(p.C(Color2), x - 72 * S, y + 4 * S, x + 72 * S, y + 4 * S, x, y - 88 * S); p.R(x, y + 225 * S, 50 * S, 110 * S, 22 * S, p.C(0x6B4428)); p.D(x, y + 88 * S, 19 * S, p.C(0x8ED0EE)); float rot = t * 1.2f; for (int b = 0; b < 4; b++) { float a = rot + b * MathF.PI / 2f; p.Turn(x, y - 8 * S, a, () => { p.L(x, y - 8 * S, x + 215 * S, y - 8 * S, p.C(0x6B4428), 7 * S); p.B(x + 135 * S, y + 14 * S, 150 * S, 42 * S, p.C(0xFFFFFF, .9f)); for (int l = 0; l < 5; l++) p.L(x + (70 + l * 32) * S, y - 6 * S, x + (70 + l * 32) * S, y + 34 * S, p.C(0x8A7458), 2 * S); }); } p.D(x, y - 8 * S, 13 * S, p.C(0x4A2F1B)); }
        Mill(-470, -70, .78f); Mill(250, -200, 1.2f); Art6.Hills(p, 300, 26, .005f, 3f, p.C(0x5CA84C)); for (int i = 0; i < 150; i++) { float x = (ArtPen.R01(i) - .5f) * p.W, y = 320 + ArtPen.R01(i + 200) * 220, sw = ArtPen.Sin(t * 1.4f + x * .01f) * 5f; uint c = (i % 4) switch { 0 => 0xFF5C8Au, 1 => 0xFFD54Au, 2 => 0xE85DFFu, _ => 0xFFFFFFu }; p.L(x, y + 18, x + sw, y, p.C(0x2F7D35), 2); p.D(x + sw, y, 6f + ArtPen.R01(i + 9) * 5f, p.C(c)); }
    }
}

public sealed class SuspensionBridgePlugin : QuickShapePlugin { public override string Name => "演出：つり橋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SuspensionBridgeParameter(s); }
public sealed class SuspensionBridgeParameter : Q4Art
{
    public SuspensionBridgeParameter() : this(null) { }
    public SuspensionBridgeParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0x8A6240); Color2 = Hex(0x3F8F58); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Color1=板の色", "Color2=山の色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x79C7F0, 0xE8F8FF); Art6.Hills(p, -40, 70, .0028f, .7f, p.C(0x8FBFA6)); p.B(0, 450, p.W + 4, 220, p.C(0x3C97C7)); for (int i = 0; i < 16; i++) { float x = ArtPen.Fr(t * .08f + ArtPen.R01(i)) * p.W - p.W / 2f, y = 370 + ArtPen.R01(i + 4) * 160; p.L(x, y, x + 70, y, p.C(0xFFFFFF, .5f), 3); }
        p.F(p.C(Color2), -1000, 560, -1000, 30, -660, 50, -560, 160, -500, 340, -430, 560); p.F(p.C(Color2), 1000, 560, 1000, 30, 660, 50, 560, 160, 500, 340, 430, 560); float sw = ArtPen.Sin(t * 1.5f) * 16f; float D(float x) { float e = x / 640f; return 72 + (1f - e * e) * 150f + (1f - e * e) * sw; } float R(float x) { float e = x / 640f; return -150 + (1f - e * e) * 150f + (1f - e * e) * sw * .7f; }
        var rope = new List<Vector2>(); var rope2 = new List<Vector2>(); for (int i = 0; i <= 50; i++) { float x = -640 + i * 25.6f; rope.Add(new(x, R(x))); rope2.Add(new(x, D(x) - 58)); } for (int x = -620; x <= 620; x += 40) { p.L(x, R(x), x, D(x), p.C(0xD8C7A8), 2); p.B(x, D(x) + 4, 34, 16, p.C(ArtPen.R01(x + 900) > .5f ? 0x8A6240u : 0x7A5536u)); } p.Line(p.C(0xD8C7A8), 6, rope); p.Line(p.C(0xD8C7A8), 4, rope2); foreach (int s in new[] { -1, 1 }) { p.B(s * 640, -20, 28, 300, p.C(Color1)); p.B(s * 640, -150, 72, 18, p.C(0x5A3A22)); }
    }
}

public sealed class AncientTemplePlugin : QuickShapePlugin { public override string Name => "演出：古代の神殿"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new AncientTempleParameter(s); }
public sealed class AncientTempleParameter : Q4Art
{
    public AncientTempleParameter() : this(null) { }
    public AncientTempleParameter(SharedDataStore? s) : base(s) { Background = true; Count = 8; Color1 = Hex(0xF2ECDD); Color2 = Hex(0xC9BFA8); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Count=柱の数||4|12", "Color1=石の色", "Color2=影の色", "Speed=雲の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x3F9BE0, 0xFFF1D2); for (int i = 0; i < 3; i++) Art6.Cloud(p, ArtPen.Fr(t * .018f + i * .33f) * (p.W + 500) - p.W / 2f - 250, -420 + i * 66, 1.15f, p.C(0xFFFFFF, .9f)); Art6.Hills(p, 330, 24, .004f, 1f, p.C(0x9DB56E));
        for (int s = 0; s < 3; s++) p.B(0, 400 - s * 40, 1320 - s * 72, 42, p.C(s % 2 == 0 ? Color1 : Color2)); p.B(0, 40, 960, 500, p.C(0x5A5248)); int n = Math.Clamp(Count, 4, 12); float gap = 1040f / (n - 1); for (int i = 0; i < n; i++) { float x = -520 + i * gap; p.F(p.C(Color1), x - 39, 322, x + 39, 322, x + 30, -158, x - 30, -158); p.F(p.C(Color2), x + 8, 322, x + 39, 322, x + 30, -158, x + 8, -158); for (int f = -2; f <= 2; f++) p.L(x + f * 13, 310, x + f * 10, -150, p.C(0xA69B84, .7f), 2); p.R(x, 335, 104, 26, 4, p.C(Color1)); p.R(x, -170, 100, 28, 6, p.C(Color1)); }
        p.B(0, -215, 1180, 66, p.C(Color1)); for (int i = 0; i < 20; i++) p.B(-560 + i * 59, -215, 22, 48, p.C(Color2)); p.B(0, -258, 1250, 22, p.C(Color2)); p.F(p.C(Color1), -650, -268, 650, -268, 0, -470); p.F(p.C(Color2), -530, -285, 530, -285, 0, -438); p.D(0, -340, 42, p.C(0xE6C65A)); p.Star(0, -340, 32, 8, .5f, t * .2f, p.C(0xFFF2A8));
    }
}

public sealed class HauntedMansionPlugin : QuickShapePlugin { public override string Name => "演出：幽霊屋敷"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HauntedMansionParameter(s); }
public sealed class HauntedMansionParameter : Q4Art
{
    public HauntedMansionParameter() : this(null) { }
    public HauntedMansionParameter(SharedDataStore? s) : base(s) { Background = true; Count = 6; Color1 = Hex(0x1B1626); Color2 = Hex(0xFFD86B); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Count=コウモリの数||0|24", "Color1=屋敷の色", "Color2=窓の色", "Speed=動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x130E2E, 0x54366F); p.D(470, -270, 190, p.C(0xFFF3C8, .12f)); p.D(470, -270, 130, p.C(0xFFF3C8)); p.D(430, -300, 24, p.C(0xE4D7A8)); p.D(510, -220, 34, p.C(0xE4D7A8)); for (int i = 0; i < 4; i++) { float x = ArtPen.Fr(t * .025f + i * .27f) * (p.W + 800) - p.W / 2f - 400; p.E(x, -340 + i * 72, 240, 18, p.C(0x8D84A8, .25f)); }
        Art6.Hills(p, 365, 22, .006f, 1f, p.C(0x100C18)); var c = p.C(Color1); p.B(0, 150, 880, 460, c); p.F(c, -480, -80, 480, -80, 330, -270, -330, -270); p.B(-400, -25, 180, 690, c); p.F(c, -510, -360, -290, -360, -400, -570); p.B(390, 10, 150, 620, c); p.F(c, 300, -292, 480, -292, 390, -470); p.B(150, -320, 54, 120, c);
        void Win(float x, float y, int id) { float fl = ArtPen.Sin(t * (2f + id) + id * 2f) > -.25f ? 1f : .12f; p.R(x, y, 62, 96, 26, p.C(Color2, fl)); p.L(x, y - 48, x, y + 48, c, 4); p.L(x - 31, y, x + 31, y, c, 4); } int w = 0; for (int y = 0; y < 2; y++) for (int x = -2; x <= 2; x++) { if (y == 1 && x == 0) continue; Win(x * 130, 30 + y * 170, w++); } Win(-400, -190, w++); Win(-400, 30, w++); Win(390, -110, w++); p.R(0, 305, 110, 170, 54, p.C(0x3A2418)); p.D(32, 310, 7, p.C(0xE6C65A));
        foreach (int s in new[] { -1, 1 }) { float x = s * 720; p.P(p.C(0x0E0A14), 22, x, 450, x + s * 15, 170, x - s * 28, 20); p.P(p.C(0x0E0A14), 11, x + s * 8, 240, x + s * 108, 105, x + s * 150, 120); p.P(p.C(0x0E0A14), 9, x - s * 8, 130, x - s * 90, 25, x - s * 125, 40); }
        for (int i = 0; i < Math.Clamp(Count, 0, 24); i++) { float k = ArtPen.Fr(t * .08f + ArtPen.R01(i)), x = k * (p.W + 200) - p.W / 2f - 100, y = -420 + ArtPen.R01(i + 9) * 340 + ArtPen.Sin(t * 2f + i) * 30, f = ArtPen.Sin(t * 16f + i) * 22f, S = .7f + ArtPen.R01(i + 3) * .6f; p.F(p.C(0x07050D), x, y + 5, x - 26 * S, y - f * S, x - 58 * S, y + 2, x - 36 * S, y + 2, x - 20 * S, y + 13 * S); p.F(p.C(0x07050D), x, y + 5, x + 26 * S, y - f * S, x + 58 * S, y + 2, x + 36 * S, y + 2, x + 20 * S, y + 13 * S); p.E(x, y + 5, 10 * S, 13 * S, p.C(0x07050D)); }
    }
}

public sealed class SnowCabinPlugin : QuickShapePlugin { public override string Name => "演出：雪山の山小屋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SnowCabinParameter(s); }
public sealed class SnowCabinParameter : Q4Art
{
    public SnowCabinParameter() : this(null) { }
    public SnowCabinParameter(SharedDataStore? s) : base(s) { Background = true; Count = 70; Color1 = Hex(0x7A4C2D); Color2 = Hex(0xFFD86B); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Count=雪の量||0|300", "Color1=小屋の色", "Color2=窓の色", "Speed=雪の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x223A68, 0x9CC4E6); p.F(p.C(0x6F8DB0), -1000, 300, -520, -260, -240, 60, 40, -390, 560, 240, 1000, 300); p.F(p.C(0xF4F8FC), -520, -260, -420, -150, -470, -130, -525, -80, -590, -175); p.F(p.C(0xF4F8FC), 40, -390, 190, -210, 110, -190, 50, -120, -30, -190, -70, -260); Art6.Hills(p, 260, 32, .004f, 1.4f, p.C(0xEAF2FA));
        for (int i = 0; i < 9; i++) { float x = -820 + i * 205 + (i % 2) * 44, y = 300 + (i % 3) * 22, S = .8f + ArtPen.R01(i) * .5f; if (MathF.Abs(x - 120) < 210) continue; for (int l = 0; l < 3; l++) { float w = (90 - l * 22) * S, yy = y - l * 58 * S; p.F(p.C(0x274E3D), x - w, yy, x + w, yy, x, yy - 96 * S); p.F(p.C(0xFFFFFF), x - w * .45f, yy - 54 * S, x + w * .45f, yy - 54 * S, x, yy - 96 * S); } }
        const float cx = 120, by = 395; p.B(cx, by - 110, 360, 220, p.C(Color1)); for (int r = 0; r < 6; r++) p.L(cx - 180, by - 200 + r * 38, cx + 180, by - 200 + r * 38, p.C(0x4E2F1B), 4); p.B(cx + 115, by - 345, 48, 150, p.C(0x5D5A58)); p.F(p.C(0xFFFFFF), cx - 235, by - 205, cx + 235, by - 205, cx, by - 395); p.R(cx - 72, by - 62, 86, 124, 8, p.C(0x3E2616)); p.R(cx + 78, by - 115, 92, 84, 6, p.C(Color2, .9f + ArtPen.Sin(t * 5f) * .1f)); p.L(cx + 78, by - 157, cx + 78, by - 73, p.C(0x3E2616), 5); p.L(cx + 32, by - 115, cx + 124, by - 115, p.C(0x3E2616), 5);
        for (int i = 0; i < 12; i++) { float k = ArtPen.Fr(t * .2f + i / 12f); p.D(cx + 115 + ArtPen.Sin(k * 6f + i) * (6 + k * 34) + k * 60, by - 420 - k * 260, 14 + k * 38, p.C(0xFFFFFF, .36f * (1f - k))); } for (int i = 0; i < Math.Clamp(Count, 0, 300); i++) p.D(ArtPen.Fr(ArtPen.R01(i) + ArtPen.Sin(t * .3f + i) * .01f) * p.W - p.W / 2f, ArtPen.Fr(ArtPen.R01(i + 500) + t * (.03f + ArtPen.R01(i + 7) * .05f)) * p.H - p.H / 2f, 1.6f + ArtPen.R01(i + 3) * 3.4f, p.C(0xFFFFFF, .92f));
    }
}

public sealed class FactoryChimneyPlugin : QuickShapePlugin { public override string Name => "演出：工場の煙突"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FactoryChimneyParameter(s); }
public sealed class FactoryChimneyParameter : Q4Art
{
    public FactoryChimneyParameter() : this(null) { }
    public FactoryChimneyParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0x5D6875); Color2 = Hex(0xC9503A); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Color1=建物の色", "Color2=煙突の色", "Speed=煙の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) Art6.Sky(p, 0x8A9BAD, 0xF0D8B8); float[] cxs = [-390, -180, 110]; float[] tops = [-360, -420, -300];
        for (int c = 0; c < 3; c++) for (int i = 0; i < 14; i++) { float k = ArtPen.Fr(t * .16f + i / 14f + c * .3f); p.D(cxs[c] + k * 330 + ArtPen.Sin(k * 6f + i) * (8 + k * 36), tops[c] - 18 - k * 180, 22 + k * 62, p.C(0xE5E5E5, .48f * (1f - k))); }
        for (int c = 0; c < 3; c++) { float x = cxs[c], y = tops[c]; p.F(p.C(Color2), x - 44, 240, x + 44, 240, x + 27, y, x - 27, y); foreach (float q in new[] { .12f, .24f }) { float by = y + (240 - y) * q, w = 27 + 17 * q; p.B(x, by, w * 2, 22, p.C(0xF2F2EE)); } p.B(x, y, 72, 20, p.C(0x3A3E45)); }
        p.F(p.C(Color1), -760, 460, -760, 80, -560, -40, -560, 80, -360, -40, -360, 80, -160, -40, -160, 80, 40, -40, 40, 80, 240, -40, 240, 80, 760, 80, 760, 460); for (int x = 0; x < 11; x++) for (int y = 0; y < 2; y++) p.R(-640 + x * 128, 170 + y * 120, 76, 68, 5, p.C(0xFFE08A, ArtPen.R01(x * 7 + y) > .3f ? .9f : .18f));
        for (int g = 0; g < 2; g++) { float x = 500 + g * 140, y = -30 - g * 22, R = 96 - g * 26, dir = g == 0 ? 1f : -1.45f; p.Turn(x, y, t * .7f * dir, () => { for (int i = 0; i < 10; i++) { float a = i * MathF.Tau / 10f; p.F(p.C(0xB9BEC6), x + ArtPen.Cos(a - .18f) * R * .86f, y + ArtPen.Sin(a - .18f) * R * .86f, x + ArtPen.Cos(a - .1f) * R * 1.16f, y + ArtPen.Sin(a - .1f) * R * 1.16f, x + ArtPen.Cos(a + .1f) * R * 1.16f, y + ArtPen.Sin(a + .1f) * R * 1.16f, x + ArtPen.Cos(a + .18f) * R * .86f, y + ArtPen.Sin(a + .18f) * R * .86f); } p.D(x, y, R * .92f, p.C(0xB9BEC6)); p.D(x, y, R * .34f, p.C(Color1)); }); }
    }
}

public sealed class DamReleasePlugin : QuickShapePlugin { public override string Name => "演出：ダムの放水"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DamReleaseParameter(s); }
public sealed class DamReleaseParameter : Q4Art
{
    public DamReleaseParameter() : this(null) { }
    public DamReleaseParameter(SharedDataStore? s) : base(s) { Background = true; Count = 3; Color1 = Hex(0xB9BCB8); Color2 = Hex(0x6FCBF5); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空と山を塗る", "Count=放水の本数||1|5", "Color1=ダムの色", "Color2=水の色", "Speed=流れる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) { Art6.Sky(p, 0x5AAEEA, 0xDFF3FF); Art6.Hills(p, -180, 80, .003f, .8f, p.C(0x4E8B5A)); } const float top = -210, bot = 330; p.B(0, top - 22, p.W, 34, p.C(0x3C97C7)); p.F(p.C(Color1), -640, top, 640, top, 840, bot, -840, bot); for (int y = 0; y < 8; y++) { float e = y / 8f, yy = top + e * (bot - top), w = 640 + e * 200; p.L(-w, yy, w, yy, p.C(0x8E918D, .65f), 2); } for (int x = -7; x <= 7; x++) p.L(x * 92, top, x * 119, bot, p.C(0x8E918D, .5f), 2); p.B(0, top - 12, 1320, 26, p.C(0xD4D6D2)); for (int i = -6; i <= 6; i++) p.B(i * 104, top - 34, 16, 38, p.C(0x70757B));
        int n = Math.Clamp(Count, 1, 5); for (int c = 0; c < n; c++) { float x = (c - (n - 1) / 2f) * 215; p.R(x, top + 26, 138, 32, 5, p.C(0x31363B)); var body = new List<Vector2>(); for (int i = 0; i <= 20; i++) { float e = i / 20f; body.Add(new(x - 58 - e * 18, top + 30 + e * (bot - top - 8))); } for (int i = 20; i >= 0; i--) { float e = i / 20f; body.Add(new(x + 58 + e * 18, top + 30 + e * (bot - top - 8))); } p.Fill(p.C(Color2, .9f), body); for (int l = 0; l < 13; l++) { float k = ArtPen.Fr(t * (1f + ArtPen.R01(l + c * 9) * .35f) + ArtPen.R01(l * 3 + c)), lx = x + (ArtPen.R01(l * 5 + c) - .5f) * (112 + k * 34), ly = top + 38 + k * (bot - top - 48); p.L(lx, ly, lx, ly + 54, p.C(0xFFFFFF, .75f), 4); } for (int i = 0; i < 10; i++) { float k = ArtPen.Fr(t * .8f + ArtPen.R01(i + c * 13)); p.D(x + (ArtPen.R01(i + 40) - .5f) * 290 * (.5f + k), bot + 20 - k * 80, 18 + k * 34, p.C(0xFFFFFF, .5f * (1f - k))); } }
        p.B(0, 450, p.W + 4, 250, p.C(Color2, .85f)); for (int i = 0; i < 24; i++) { float x = ArtPen.Fr(t * .18f + ArtPen.R01(i)) * p.W - p.W / 2f, y = 350 + ArtPen.R01(i + 9) * 190; p.L(x, y, x + 62, y, p.C(0xFFFFFF, .58f), 3); }
    }
}

public sealed class MoonBasePlugin : QuickShapePlugin { public override string Name => "演出：月面基地"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MoonBaseParameter(s); }
public sealed class MoonBaseParameter : Q4Art
{
    public MoonBaseParameter() : this(null) { }
    public MoonBaseParameter(SharedDataStore? s) : base(s) { Background = true; Color1 = Hex(0xDCE3EA); Color2 = Hex(0x56E0FF); }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=宇宙を塗る", "Color1=基地の色", "Color2=光の色", "Speed=動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) p.B(0, 0, p.W + 4, p.H + 4, p.C(0x04060F)); for (int i = 0; i < 130; i++) p.D((ArtPen.R01(i) - .5f) * p.W, (ArtPen.R01(i + 300) - .5f) * p.H * .72f - p.H * .12f, .9f + ArtPen.R01(i + 600) * 1.8f, p.C(0xFFFFFF, .35f + .65f * MathF.Abs(ArtPen.Sin(t * .6f + i)))); p.D(-520, -270, 132, p.C(0x3B8EE0)); for (int i = 0; i < 7; i++) { float a = ArtPen.R01(i) * MathF.Tau + t * .04f, r = ArtPen.R01(i + 5) * 92; p.E(-520 + ArtPen.Cos(a) * r, -270 + ArtPen.Sin(a) * r, 40 + ArtPen.R01(i + 3) * 28, 22, p.C(0x55B85A)); } p.Pie(-520, -270, 133, -1, MathF.PI - 1, p.C(0x000000, .5f));
        Art6.Hills(p, 165, 28, .004f, 1.3f, p.C(0x8E8E92)); for (int i = 0; i < 12; i++) { float x = (ArtPen.R01(i + 80) - .5f) * p.W, y = 245 + ArtPen.R01(i + 90) * 260, rx = 44 + ArtPen.R01(i + 95) * 98; p.E(x, y, rx * 1.1f, rx * .32f, p.C(0xAAAAAE)); p.E(x, y, rx, rx * .27f, p.C(0x69696E)); }
        void Dome(float x, float y, float R) { p.Pie(x, y, R, MathF.PI, MathF.Tau, p.C(Color2, .32f)); p.Arc(x, y, R, MathF.PI, MathF.Tau, p.C(Color1), 7); for (int i = 1; i < 5; i++) { float a = MathF.PI + i * MathF.PI / 5f; p.L(x, y - R, x + ArtPen.Cos(a) * R, y + ArtPen.Sin(a) * R, p.C(Color1, .6f), 2); } p.Arc(x, y, R * .55f, MathF.PI, MathF.Tau, p.C(Color1, .6f), 2); p.B(x, y + 18, R * 2 + 22, 36, p.C(Color1)); for (float wx = x - R + 18; wx < x + R; wx += 48) p.B(wx, y + 18, 26, 13, p.C(Color2)); }
        Dome(70, 240, 190); Dome(470, 270, 118); p.B(290, 262, 120, 34, p.C(Color1)); p.L(-300, 280, -300, -40, p.C(Color1), 11); float dish = ArtPen.Sin(t * .6f) * .5f; p.Turn(-300, -40, dish, () => { p.Pie(-300, -30, 112, 0, MathF.PI, p.C(Color1)); p.L(-300, -30, -300, 42, p.C(0x70757B), 5); p.D(-300, 45, 9, p.C(0xFF5252)); }); for (int r = 0; r < 3; r++) { float k = ArtPen.Fr(t * .7f + r / 3f); p.Arc(-300 + ArtPen.Sin(dish) * 30, -60, 40 + k * 130, -MathF.PI / 2f + dish - .55f, -MathF.PI / 2f + dish + .55f, p.C(Color2, 1f - k), 4); }
        float bx = ArtPen.Sin(t * .22f) * 420 - 120; p.R(bx, 410, 150, 54, 12, p.C(Color1)); p.R(bx + 25, 380, 56, 34, 7, p.C(Color2, .8f)); foreach (int s in new[] { -1, 1 }) { p.D(bx + s * 52, 442, 21, p.C(0x25282D)); p.D(bx + s * 52, 442, 8, p.C(0xB9BEC6)); }
    }
}

public sealed class FlowerFramePlugin : QuickShapePlugin { public override string Name => "演出：花の飾り枠"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FlowerFrameParameter(s); }
public sealed class FlowerFrameParameter : Q4Art
{
    public FlowerFrameParameter() : this(null) { }
    public FlowerFrameParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFF8FB1); Color2 = Hex(0x6BBF59); Color3 = Hex(0xFFD54A); }
    protected override string[] Use => ["Color1=花の色", "Color2=葉の色", "Color3=花の中心の色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float W = 1460, H = 800; p.RL(0, 0, W, H, 18, p.C(Color2), 7); p.RL(0, 0, W - 26, H - 26, 12, p.C(0xFFFFFF, .65f), 3);
        void Flower(float x, float y, float S, int id) { float g = DrawKit.EaseOutBack(ArtPen.Cl((p.Now - .1f - id * .025f) * 3f)); if (g <= 0f) return; float sw = ArtPen.Sin(t * 1.3f + id) * .1f; for (int i = 0; i < 2; i++) { float a = id * 1.7f + i * 2.4f; p.Turn(x, y, a, () => p.E(x + 54 * S * g, y, 44 * S * g, 18 * S * g, p.C(Color2))); } for (int i = 0; i < 6; i++) { float a = i * MathF.Tau / 6f + sw; p.D(x + ArtPen.Cos(a) * 25 * S * g, y + ArtPen.Sin(a) * 25 * S * g, 20 * S * g, p.C(Gfx.Mix(Color1, Hex(0xFFFFFF), (id % 3) * .2f))); } p.D(x, y, 14 * S * g, p.C(Color3)); }
        int k = 0; foreach (int sx in new[] { -1, 1 }) foreach (int sy in new[] { -1, 1 }) { float cx = sx * W / 2f, cy = sy * H / 2f; Flower(cx, cy, 1.9f, k++); for (int i = 1; i <= 4; i++) { Flower(cx - sx * i * 72, cy + (i % 2 == 0 ? 13 : -13) * sy, 1.15f - i * .13f, k++); Flower(cx + (i % 2 == 0 ? 13 : -13) * sx, cy - sy * i * 66, 1.15f - i * .13f, k++); } }
    }
}

public sealed class StainedGlassPlugin : QuickShapePlugin { public override string Name => "演出：ステンドグラス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new StainedGlassParameter(s); }
public sealed class StainedGlassParameter : Q4Art
{
    public StainedGlassParameter() : this(null) { }
    public StainedGlassParameter(SharedDataStore? s) : base(s) { Count = 12; Color1 = Hex(0x1B1B22); Color2 = Hex(0xFFF2C0); }
    protected override string[] Use => ["Count=分割の数||6|24", "Color1=鉛の線の色", "Color2=光の色", "Speed=光の動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float R = 430; int n = Math.Clamp(Count, 6, 24); float[] rs = [R, R * .73f, R * .47f, R * .22f]; for (int r = 0; r < 3; r++) for (int i = 0; i < n; i++) { float a0 = i * MathF.Tau / n + r * MathF.PI / n, a1 = a0 + MathF.Tau / n, L = .5f + .16f * ArtPen.Sin(t * 1.1f + i * .8f + r); var c = Gfx.Hsv(i * 360f / n + r * 45f, .72f, Math.Clamp(L + .36f, 0f, 1f)); var q = new List<Vector2>(); for (int k = 0; k <= 8; k++) { float a = a0 + (a1 - a0) * k / 8f; q.Add(new(ArtPen.Cos(a) * rs[r], ArtPen.Sin(a) * rs[r])); } for (int k = 8; k >= 0; k--) { float a = a0 + (a1 - a0) * k / 8f; q.Add(new(ArtPen.Cos(a) * rs[r + 1], ArtPen.Sin(a) * rs[r + 1])); } p.Fill(p.C(c), q); p.Line(p.C(Color1), 6, q.Append(q[0])); }
        p.D(0, 0, rs[3], p.C(0xFFD54A)); p.Star(0, 0, rs[3] * .9f, 8, .45f, t * .12f, p.C(0xFFFFFF)); p.Ring(0, 0, rs[3], p.C(Color1), 7); p.Ring(0, 0, R + 8, p.C(Color1), 22); p.Ring(0, 0, R + 24, p.C(0xB08D57), 8);
        float sw = ArtPen.Sin(t * .45f) * 280f; using var gb = Gfx.Linear(p.Dc, p.V(sw - 210, -R), p.V(sw + 210, R), (0f, p.C(Color2, 0f)), (.5f, p.C(Color2, .42f)), (1f, p.C(Color2, 0f))); var cc = p.V(0, 0); var e = p.V(R, 0); float rr = e.X - cc.X; p.Dc.FillEllipse(new Vortice.Direct2D1.Ellipse(cc, rr, rr), gb);
    }
}

public sealed class CheckeredFlagPlugin : QuickShapePlugin { public override string Name => "演出：チェッカーフラッグ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CheckeredFlagParameter(s); }
public sealed class CheckeredFlagParameter : Q4Art
{
    public CheckeredFlagParameter() : this(null) { }
    public CheckeredFlagParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x15161A); }
    protected override string[] Use => ["Color1=色1", "Color2=色2", "Speed=はためく速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const int nx = 16, ny = 10; const float x0 = -380, y0 = -300, cell = 52; Vector2 G(int x, int y) { float e = x / (float)nx, w = ArtPen.Sin(e * 7f - t * 6f + y * .16f) * 46f * e, wx = ArtPen.Cos(e * 7f - t * 6f) * 10f * e; return new(x0 + x * cell + wx, y0 + y * cell + w); }
        for (int y = 0; y < ny; y++) for (int x = 0; x < nx; x++) { var a = G(x, y); var b = G(x + 1, y); var c = G(x + 1, y + 1); var d = G(x, y + 1); float shade = .8f + .2f * ArtPen.Cos((x + .5f) / nx * 7f - t * 6f); var base0 = (x + y) % 2 == 0 ? Color1 : Color2; p.F(p.C(Gfx.Mix(Hex(0x000000), base0, shade)), a.X, a.Y, b.X, b.Y, c.X, c.Y, d.X, d.Y); }
        p.R(x0 - 13, 40, 26, 790, 12, p.C(0xB9BEC6)); p.D(x0 - 13, -360, 23, p.C(0xFFD54A));
    }
}

public sealed class FloatingBalloonsPlugin : QuickShapePlugin { public override string Name => "演出：風船が飛ぶ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FloatingBalloonsParameter(s); }
public sealed class FloatingBalloonsParameter : Q4Art
{
    public FloatingBalloonsParameter() : this(null) { }
    public FloatingBalloonsParameter(SharedDataStore? s) : base(s) { Count = 18; Color1 = Hex(0xFF5C8A); }
    protected override bool Scene => true;
    protected override string[] Use => ["Count=数||1|80", "Color1=基準の色|ここから色相をずらします", "Speed=昇る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        int n = Math.Clamp(Count, 1, 80); float baseH = Gfx.Hue(Color1); for (int i = 0; i < n; i++)
        {
            float d = .55f + (i + 1f) / n * .75f, R = 62f * d, span = p.H + R * 7f, y = p.H / 2f + R * 3f - ArtPen.Fr(t * .07f * (.7f + d * .5f) + ArtPen.R01(i + 5)) * span, x = (ArtPen.R01(i) - .5f) * p.W + ArtPen.Sin(t * .8f + i) * 26f; var c = Gfx.Hsv(baseH + ArtPen.R01(i + 9) * 300f, .68f, 1f); var q = new List<Vector2>(); for (int k = 0; k <= 24; k++) { float e = k / 24f; q.Add(new(x + ArtPen.Sin(e * 12f + t * 2f + i) * 7f * d, y + R * 1.16f + e * R * 2.3f)); } p.Line(p.C(0xFFFFFF, .55f), 2, q);
            p.E(x, y, R, R * 1.18f, p.C(c)); p.F(p.C(c), x - 10 * d, y + R * 1.25f, x + 10 * d, y + R * 1.25f, x, y + R * 1.08f); p.E(x - R * .33f, y - R * .42f, R * .19f, R * .32f, p.C(0xFFFFFF, .62f));
        }
    }
}

public sealed class KusudamaPlugin : QuickShapePlugin { public override string Name => "演出：くす玉が割れる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new KusudamaParameter(s); }
public sealed class KusudamaParameter : Q4Art
{
    public KusudamaParameter() : this(null) { }
    public KusudamaParameter(SharedDataStore? s) : base(s) { Text = "祝 達成"; Color1 = Hex(0xE6B422); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xD7263D); }
    protected override double DefAmount => 15;
    protected override string[] Use => ["Text=垂れ幕の文字", "Amount=割れる時刻|0〜100 = 0〜5秒", "Color1=玉の色", "Color2=垂れ幕の色", "Color3=文字の色"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float cy = -270, R = 165; float at = v * 5f, k = DrawKit.EaseOut(ArtPen.Cl((p.Now - at) * 2.2f)), ang = k * 1.05f; p.L(0, -520, 0, cy - R, p.C(0xFFFFFF), 5);
        if (k > 0f) { float drop = ArtPen.Cl((p.Now - at) * 1.5f), H = drop * 500f; if (H > 4f) { p.B(0, cy + H / 2f, 170, H, p.C(Color2)); p.B(-85, cy + H / 2f, 8, H, p.C(Color3)); p.B(85, cy + H / 2f, 8, H, p.C(Color3)); string s = Str.Replace(" ", ""); float fs = MathF.Min(100f, 440f / MathF.Max(1, s.Length)); for (int i = 0; i < s.Length; i++) { float y = cy + 45 + fs * .55f + i * fs * 1.03f; if (y < cy + H - fs * .3f) p.T(s[i].ToString(), fs, 0, y, p.C(Color3)); } } for (int i = 0; i < 90; i++) { float e = ArtPen.Cl((p.Now - at) * (.45f + ArtPen.R01(i + 7) * .3f)), a = ArtPen.R01(i) * MathF.Tau, sp = 120f + ArtPen.R01(i + 3) * 520f, x = ArtPen.Cos(a) * sp * MathF.Min(e * 2.2f, 1f) + ArtPen.Sin(e * 9f + i) * 18f, y = cy + ArtPen.Sin(a) * sp * .4f * MathF.Min(e * 2.2f, 1f) + e * e * 820f; uint c = (i % 5) switch { 0 => 0xFF5C8Au, 1 => 0x4FC3F7u, 2 => 0xFFE45Cu, 3 => 0x66E08Au, _ => 0xFFFFFFu }; if (e < 1f) p.Turn(x, y, e * 12f + i, () => p.B(x, y, 20, 10, p.C(c, 1f - e * e))); } }
        foreach (int s in new[] { -1, 1 }) p.Turn(s * 7, cy - R + 3, s * -ang, () => { p.Pie(s * 7, cy, R, s > 0 ? -MathF.PI / 2f : MathF.PI / 2f, s > 0 ? MathF.PI / 2f : MathF.PI * 1.5f, p.C(s > 0 ? Color1 : Gfx.Mix(Color1, Hex(0xFFFFFF), .16f))); for (int i = 1; i < 4; i++) p.EL(s * 7, cy, R * i / 4f, R, p.C(0x8A6A10, .32f), 2); p.Arc(s * 7, cy, R, s > 0 ? -MathF.PI / 2f : MathF.PI / 2f, s > 0 ? MathF.PI / 2f : MathF.PI * 1.5f, p.C(0x8A6A10), 5); });
    }
}

public sealed class GarlandFlagsPlugin : QuickShapePlugin { public override string Name => "演出：ガーランド"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GarlandFlagsParameter(s); }
public sealed class GarlandFlagsParameter : Q4Art
{
    public GarlandFlagsParameter() : this(null) { }
    public GarlandFlagsParameter(SharedDataStore? s) : base(s) { Count = 15; Color1 = Hex(0xFF5C8A); Color2 = Hex(0x4FC3F7); Color3 = Hex(0xFFD54A); }
    protected override bool Scene => true;
    protected override string[] Use => ["Count=旗の数||4|40", "Color1=色1", "Color2=色2", "Color3=色3", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float W = p.W + 60, top = -p.H / 2f + 40; int n = Math.Clamp(Count, 4, 40); float Y(float e) => top + ArtPen.Sin(e * MathF.PI) * 115f + ArtPen.Sin(t * 1.3f + e * 4f) * 7f; var q = new List<Vector2>(); for (int i = 0; i <= 60; i++) { float e = i / 60f; q.Add(new(-W / 2f + e * W, Y(e))); } p.Line(p.C(0xFFFFFF), 4, q);
        for (int i = 0; i < n; i++) { float e = (i + .5f) / n, x = -W / 2f + e * W, y = Y(e), fw = MathF.Min(52f, W / n * .42f), fh = fw * 2.2f, sw = ArtPen.Sin(t * 2.4f + i * .9f) * .16f, g = DrawKit.EaseOutBack(ArtPen.Cl((p.Now - i * .035f) * 3.5f)); var c = (i % 3) switch { 0 => Color1, 1 => Color2, _ => Color3 }; p.Turn(x, y, sw, () => p.Zoom(x, y, g, g, () => { p.F(p.C(c), x - fw, y, x + fw, y, x, y + fh); p.F(p.C(0xFFFFFF, .24f), x - fw, y, x, y, x, y + fh); })); }
    }
}

public sealed class HangingBannerPlugin : QuickShapePlugin { public override string Name => "演出：垂れ幕"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HangingBannerParameter(s); }
public sealed class HangingBannerParameter : Q4Art
{
    public HangingBannerParameter() : this(null) { }
    public HangingBannerParameter(SharedDataStore? s) : base(s) { Text = "祝 百万再生"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xD7263D); Color3 = Hex(0xE6B422); }
    protected override string[] Use => ["Text=文字", "Color1=幕の色", "Color2=文字の色", "Color3=棒の色", "Speed=下りる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        string s = Str.Replace("\n", ""); int n = Math.Max(1, s.Length); const float W = 250; float full = MathF.Min(860f, n * 150f + 70), top = -430, k = DrawKit.EaseOut(ArtPen.Cl(t * .85f)), H = full * k, sw = ArtPen.Sin(p.Now * 2f) * 7f * (1f - ArtPen.Cl((p.Now - 1.2f) * .4f));
        p.F(p.C(Color1), -W / 2f, top, W / 2f, top, W / 2f + sw, top + H, -W / 2f + sw, top + H); foreach (int e in new[] { -1, 1 }) p.L(e * (W / 2f - 12), top, e * (W / 2f - 12) + sw, top + H, p.C(Color2), 5); float fs = MathF.Min(130f, (full - 70) / n * .94f); for (int i = 0; i < s.Length; i++) { float y = top + 34 + fs * .54f + i * (full - 70) / n; if (y + fs * .46f < top + H) p.T(s[i].ToString(), fs, sw * (y - top) / MathF.Max(1f, H), y, p.C(Color2)); }
        p.R(0, top, W + 90, 26, 13, p.C(Color3)); foreach (int e in new[] { -1, 1 }) p.D(e * (W / 2f + 48), top, 19, p.C(Color3)); p.R(sw, top + H, W + 34, 20, 10, p.C(Color3));
    }
}

public sealed class FlowerBouquetPlugin : QuickShapePlugin { public override string Name => "演出：花束"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FlowerBouquetParameter(s); }
public sealed class FlowerBouquetParameter : Q4Art
{
    public FlowerBouquetParameter() : this(null) { }
    public FlowerBouquetParameter(SharedDataStore? s) : base(s) { Count = 13; Color1 = Hex(0xFF6F91); Color2 = Hex(0xFFF3F6); Color3 = Hex(0xE5305E); }
    protected override string[] Use => ["Count=花の数||3|30", "Color1=花の色", "Color2=包み紙の色", "Color3=リボンの色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float sw = ArtPen.Sin(t * 1.2f) * .035f; p.Turn(0, 310, sw, () =>
        {
            for (int i = -3; i <= 3; i++) p.L(i * 8, 400, i * 46, 110, p.C(0x3F8F48), 8); int n = Math.Clamp(Count, 3, 30); p.E(0, -60, 275, 215, p.C(0x4FA84F));
            for (int i = 0; i < 9; i++) { float a = MathF.PI + i * MathF.PI / 8f; p.Turn(ArtPen.Cos(a) * 200, -55 + ArtPen.Sin(a) * 160, a, () => p.E(ArtPen.Cos(a) * 240, -55 + ArtPen.Sin(a) * 180, 88, 30, p.C(0x3F9A4A))); }
            for (int i = 0; i < n; i++) { float a = i * 2.4f, r = MathF.Sqrt((i + .5f) / n) * 205f, x = ArtPen.Cos(a) * r, y = -75 + ArtPen.Sin(a) * r * .8f, S = 58f + ArtPen.R01(i) * 16f, g = DrawKit.EaseOutBack(ArtPen.Cl((p.Now - .1f - i * .04f) * 3f)); var col = Gfx.Hsv(Gfx.Hue(Color1) + (i % 4) * 16f - 18f, .55f + (i % 3) * .1f, 1f); p.D(x, y, S * g, p.C(col)); for (int l = 0; l < 3; l++) p.Arc(x, y, S * g * (.78f - l * .24f), i + l * 2f, i + l * 2f + 4.3f, p.C(Gfx.Mix(col, Hex(0x7A1030), .42f)), 4); }
            p.F(p.C(Color2), -245, 40, 245, 40, 66, 350, -66, 350); p.F(p.C(Gfx.Mix(Color2, Hex(0xFF8FB1), .32f)), 0, 40, 245, 40, 66, 350, 8, 350); p.F(p.C(Color2), -245, 40, -130, 95, -70, 42, 0, 104, 70, 42, 130, 95, 245, 40, 66, 200, -66, 200); foreach (int s in new[] { -1, 1 }) { p.F(p.C(Color3), 0, 275, s * 120, 215, s * 132, 305); p.F(p.C(Color3), s * 8, 282, s * 90, 410, s * 34, 410); } p.R(0, 280, 148, 36, 10, p.C(Color3)); p.D(0, 279, 25, p.C(Gfx.Mix(Color3, Hex(0xFFFFFF), .25f)));
        });
    }
}

public sealed class PaperPlanePlugin : QuickShapePlugin { public override string Name => "演出：紙飛行機"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PaperPlaneParameter(s); }
public sealed class PaperPlaneParameter : Q4Art
{
    public PaperPlaneParameter() : this(null) { }
    public PaperPlaneParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xBFD8EE); }
    protected override bool Scene => true;
    protected override string[] Use => ["Color1=紙の色", "Color2=折り目の色", "Speed=飛ぶ速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        static Vector2 At(float k, float W, float H) { float x = -W * .6f + k * W * 1.2f, y = H * .3f - k * H * .55f + MathF.Sin(k * 9f) * H * .16f; if (k is > .42f and < .62f) { float e = (k - .42f) / .2f * MathF.Tau; x += MathF.Sin(e) * 105f; y += (1f - MathF.Cos(e)) * -105f; } return new(x, y); }
        float now = ArtPen.Fr(t * .13f); for (int i = 0; i < 90; i++) { float k = now - i * .0052f; if (k <= 0f) break; if (i % 2 == 1) continue; var a = At(k, p.W, p.H); var b = At(MathF.Max(0f, k - .0052f), p.W, p.H); p.L(a.X, a.Y, b.X, b.Y, p.C(0xFFFFFF, .8f * (1f - i / 90f)), 4); }
        var q = At(now, p.W, p.H); var f = At(now + .003f, p.W, p.H) - q; float ang = MathF.Atan2(f.Y, f.X), wob = ArtPen.Sin(t * 5f) * .06f; p.Turn(q.X, q.Y, ang + wob, () => { float x = q.X, y = q.Y; p.F(p.C(Color1), x + 112, y, x - 105, y - 72, x - 58, y - 4); p.F(p.C(Color2), x + 112, y, x - 58, y - 4, x - 76, y + 24); p.F(p.C(Gfx.Mix(Color1, Color2, .45f)), x + 112, y, x - 76, y + 24, x - 105, y + 64); p.L(x + 112, y, x - 58, y - 4, p.C(0x7E9AB5), 2); });
    }
}

public sealed class OrigamiCranePlugin : QuickShapePlugin { public override string Name => "演出：折り鶴"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new OrigamiCraneParameter(s); }
public sealed class OrigamiCraneParameter : Q4Art
{
    public OrigamiCraneParameter() : this(null) { }
    public OrigamiCraneParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xE5304A); Color2 = Hex(0xFFFFFF); }
    protected override string[] Use => ["Color1=紙の色", "Color2=光の色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float y = ArtPen.Sin(t * 1.3f) * 14f, f = ArtPen.Sin(t * 1.9f) * 34f, r = ArtPen.Sin(t * 1.0f) * .035f; Color4 L(float k) => p.C(k >= 0f ? Gfx.Mix(Color1, Color2, k) : Gfx.Mix(Color1, Hex(0x000000), -k));
        p.E(0, 340, 250, 26, p.C(0x000000, .2f)); p.Move(0, y, () => p.Turn(0, 90, r, () =>
        {
            p.F(L(-.3f), 15, 120, -230, -190 - f, -20, 40); p.F(L(-.1f), 15, 120, 110, -280 - f, 120, 30); p.F(L(-.42f), -20, 100, -360, 20, -255, -48, -40, 40); p.F(L(-.2f), -255, -48, -360, 20, -445, -105, -395, -120);
            p.F(L(-.28f), 30, 92, 190, 150, 360, -130, 310, -148, 115, 30); p.F(L(.04f), 310, -148, 360, -130, 455, -82, 410, -62, 360, -96); p.F(L(-.05f), 455, -82, 500, -18, 410, -62);
            p.F(L(.12f), -130, 72, 190, 100, 80, 250, 0, 280, -55, 215); p.F(L(-.18f), 0, 280, 80, 250, 190, 100, 45, 110); p.L(45, 110, 0, 280, p.C(0x7A1020, .7f), 3); p.F(L(.28f), -130, 72, 45, 110, 15, 120, -80, 60); p.F(L(.2f), 15, 120, 120, 30, 190, 100, 45, 110); p.L(-130, 72, 190, 100, p.C(0x7A1020, .55f), 3);
        }));
    }
}
