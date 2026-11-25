using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 551〜570: 画面効果・転換
//  図形は下の映像を読み取れないので、「画面を覆う絵」として場面を切り替える。
//  「閉じたあと開く」がオンなら、覆う→開く。オフなら覆ったまま(次の場面の直前に置く)

public abstract class Q3TransParam : Q3Param
{
    protected Q3TransParam(SharedDataStore? s) : base(s) { Background = true; }
    protected override double DefAmount2 => 18;
    protected const string Len = "Amount2=長さ|0〜100 = 0.2〜4秒";
    protected const string Back = "Background=閉じたあと開く|オフにすると覆ったままになります";
    protected float Dur(in DrawContext c) => 0.2f + Amt2(c) * 3.8f;
    protected float Cover(in DrawContext c)
    {
        float d = Dur(c), t = c.Time;
        if (t < d) return Gfx.Smooth(0f, 1f, t / d);
        if (!Background || t < d + 0.12f) return 1f;
        return 1f - Gfx.Smooth(0f, 1f, (t - d - 0.12f) / d);
    }
}

public sealed class DoubleDoorPlugin : QuickShapePlugin { public override string Name => "演出：両開きの扉"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DoubleDoorParameter(s); }
public sealed class DoubleDoorParameter : Q3TransParam
{
    public DoubleDoorParameter() : this(null) { }
    public DoubleDoorParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x7A4A25); Color2 = Hex(0xE6B422); }
    static readonly Dictionary<string, ParamGuide> g = G("両開きの扉", Len, Back, "Variant=扉の種類|1=木　2=鉄　3=ふすま|1|3", "Color1=扉の色", "Color2=取っ手の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), k = Cover(c), dw = w / 2f; if (k <= 0f) return; var col = Variant == 2 ? Hex(0x59616B) : Variant == 3 ? Hex(0xF2E8D2) : Color1;
        foreach (int s in new[] { -1, 1 })
        {
            float x0 = s < 0 ? -dw + dw * k : w - dw * k, x1 = x0 + dw; Box(dc, b, x0, 0, x1, h, Col(col, a));
            if (Variant == 1) { for (int i = 1; i < 6; i++) Ln(dc, b, new(x0 + dw * i / 6f, 0), new(x0 + dw * i / 6f, h), Rgb(0.18f, 0.09f, 0.04f, 0.5f * a), 3f * u); for (int r = 0; r < 2; r++) Frame(dc, b, x0 + 70f * u, 70f * u + r * (h - 80f * u) / 2f, x1 - 70f * u, (r + 1) * (h - 80f * u) / 2f - 10f * u, Rgb(0.2f, 0.1f, 0.05f, 0.75f * a), 7f * u); }
            else if (Variant == 2) { for (int y = 0; y < 8; y++) for (int x = 0; x < 6; x++) Dot(dc, b, new(x0 + (x + 0.5f) * dw / 6f, (y + 0.5f) * h / 8f), 7f * u, Rgb(0.8f, 0.83f, 0.87f, 0.75f * a)); Frame(dc, b, x0 + 14f * u, 14f * u, x1 - 14f * u, h - 14f * u, Rgb(0.2f, 0.23f, 0.27f, a), 12f * u); }
            else { Frame(dc, b, x0 + 8f * u, 8f * u, x1 - 8f * u, h - 8f * u, Rgb(0.2f, 0.12f, 0.07f, a), 16f * u); for (int i = 0; i < 7; i++) { float cx = x0 + (Rnd(i + 4) * 0.8f + 0.1f) * dw, cy = h * (0.15f + Rnd(i + 9) * 0.7f); for (int q = 0; q < 3; q++) Arc(dc, b, new(cx + q * 34f * u, cy), 42f * u, MathF.PI, MathF.Tau, Rgb(0.56f, 0.68f, 0.65f, 0.55f * a), 3f * u); } }
            var hd = new Vector2(s < 0 ? x1 - 54f * u : x0 + 54f * u, h / 2f); if (Variant == 3) { Dot(dc, b, hd, 30f * u, Rgb(0.15f, 0.1f, 0.06f, a)); Ring(dc, b, hd, 30f * u, C2(a), 5f * u); } else { RBox(dc, b, hd, 24f * u, 170f * u, 12f * u, C2(a)); }
        }
        Ln(dc, b, new(dw, 0), new(dw, h), Rgb(0, 0, 0, MathF.Max(0f, k * 2f - 1f) * 0.6f * a), 4f * u);
    }
}

public sealed class RollingShutterPlugin : QuickShapePlugin { public override string Name => "演出：シャッターが閉まる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RollingShutterParameter(s); }
public sealed class RollingShutterParameter : Q3TransParam
{
    public RollingShutterParameter() : this(null) { }
    public RollingShutterParameter(SharedDataStore? s) : base(s) { Text = "本日終了"; Color1 = Hex(0x8C949C); Color2 = Hex(0xFF4D6D); }
    static readonly Dictionary<string, ParamGuide> g = G("シャッター", Len, Back, "Text=落書きの文字", "Color1=シャッターの色", "Color2=落書きの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), k = Cover(c), y1 = h * k, slat = 42f * u; if (k <= 0f) return; Box(dc, b, 0, 0, w, y1, C1(a));
        for (float y = y1 - slat; y > -slat; y -= slat) { Box(dc, b, 0, y, w, y + 7f * u, Rgb(0, 0, 0, 0.26f * a)); Box(dc, b, 0, y + 8f * u, w, y + 12f * u, Rgb(1, 1, 1, 0.22f * a)); }
        Box(dc, b, 0, y1 - 26f * u, w, y1, Rgb(0.26f, 0.28f, 0.31f, a)); RBox(dc, b, new(w / 2f, y1 - 13f * u), 170f * u, 16f * u, 8f * u, Rgb(0.08f, 0.08f, 0.09f, a));
        dc.PushAxisAlignedClip(new Vortice.RawRectF(0, 0, w, MathF.Max(0f, y1 - 26f * u)), AntialiasMode.Aliased); var q = new Vector2(w / 2f, y1 - h * 0.5f); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(-0.08f, q) * prev; Say(dc, b, Str, 210f * u, q, C2(0.92f * a), Rgb(0.1f, 0.1f, 0.12f, 0.75f * a), 8f * u, maxW: w * 0.85f); dc.Transform = prev; dc.PopAxisAlignedClip();
    }
}

public sealed class CubeTurnPlugin : QuickShapePlugin { public override string Name => "演出：キューブ回転"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CubeTurnParameter(s); }
public sealed class CubeTurnParameter : Q3TransParam
{
    public CubeTurnParameter() : this(null) { }
    public CubeTurnParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x284B8F); Color2 = Hex(0xE85D75); }
    static readonly Dictionary<string, ParamGuide> g = G("キューブ回転", Len, Back, "Variant=回る向き|1=左へ　2=右へ　3=上へ　4=下へ|1|4", "Color1=手前の面の色", "Color2=次の面の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, k = Cover(c); if (k <= 0f) return; float th = k * MathF.PI / 2f, vis = MathF.Min(1f, k * 7f); bool horiz = Variant <= 2; float sign = Variant is 2 or 4 ? 1f : -1f, L = horiz ? w : h; Vector2 Pj(float side, float depth, float cross) { float z = 1f + depth * 0.42f, p = 1f / z; float a0 = side * L / 2f * p, c0 = cross * (horiz ? h : w) / 2f * p; return horiz ? new(w / 2f + a0, h / 2f + c0) : new(w / 2f + c0, h / 2f + a0); }
        void Face(float ang, Color4 col) { float cA = MathF.Cos(ang), sA = MathF.Sin(ang), s0 = -cA + sA, d0 = sA + cA - 1f, s1 = cA + sA, d1 = -sA + cA - 1f; if (cA <= 0.001f) return; Poly(dc, b, new List<Vector2> { Pj(s0, d0, -1f), Pj(s1, d1, -1f), Pj(s1, d1, 1f), Pj(s0, d0, 1f) }, new Color4(col.R * (0.45f + cA * 0.55f), col.G * (0.45f + cA * 0.55f), col.B * (0.45f + cA * 0.55f), col.A)); }
        Face(sign * th, C1(a * vis)); Face(sign * (th - MathF.PI / 2f), C2(a * vis));
    }
}

public sealed class PaperBallTossPlugin : QuickShapePlugin { public override string Name => "演出：紙が丸まって飛ぶ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PaperBallTossParameter(s); }
public sealed class PaperBallTossParameter : Q3Param
{
    public PaperBallTossParameter() : this(null) { }
    public PaperBallTossParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xF7F4EA); Color2 = Hex(0x5F6B76); Background = true; }
    protected override double DefAmount2 => 35;
    static readonly Dictionary<string, ParamGuide> g = G("紙が丸まって飛ぶ", "Amount2=長さ|0〜100 = 0.4〜4秒", "Variant=飛ぶ向き|1=右下へ　2=左下へ|1|2", "Background=ごみ箱を表示", "Color1=紙の色", "Color2=ごみ箱の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), d = 0.4f + Amt2(c) * 3.6f, t = c.Time / d, crumple = DrawKit.EaseOut(Sub(t, 0f, 0.45f)), fly = Sub(t, 0.45f, 0.45f), dir = Variant == 2 ? -1f : 1f; var center = new Vector2(w / 2f, h / 2f); var bin = new Vector2(w / 2f + dir * w * 0.36f, h * 0.78f);
        if (Background) { Poly(dc, b, new List<Vector2> { bin + new Vector2(-105f * u, -70f * u), bin + new Vector2(105f * u, -70f * u), bin + new Vector2(78f * u, 165f * u), bin + new Vector2(-78f * u, 165f * u) }, C2(a)); RBox(dc, b, bin - new Vector2(0, 70f * u), 240f * u, 22f * u, 8f * u, Col(Gfx.Mix(Color2, Hex(0xFFFFFF), 0.25f), a)); for (int i = -1; i <= 1; i++) Ln(dc, b, bin + new Vector2(i * 50f * u, -35f * u), bin + new Vector2(i * 38f * u, 135f * u), Rgb(0, 0, 0, 0.3f * a), 7f * u); }
        if (t >= 0.92f) return; float R = Lerp(MathF.Sqrt(w * w + h * h) / 2f, 95f * u, crumple), arc = MathF.Sin(fly * MathF.PI) * h * 0.38f; var q = Vector2.Lerp(center, bin - new Vector2(0, 60f * u), fly) - new Vector2(0, arc); var poly = new List<Vector2>(); int n = 28;
        for (int i = 0; i < n; i++) { float th = i / (float)n * MathF.Tau + fly * 7f * dir, jag = 1f + crumple * (Rnd(i * 5) - 0.5f) * 0.42f, rectR = 1f / MathF.Max(MathF.Abs(MathF.Cos(th)) / (w / 2f), MathF.Abs(MathF.Sin(th)) / (h / 2f)), rr = Lerp(rectR * 1.02f, R, crumple) * jag; poly.Add(q + new Vector2(MathF.Cos(th), MathF.Sin(th)) * rr); }
        Poly(dc, b, poly, C1(a)); float crease = MathF.Min(1f, crumple * 1.5f); for (int i = 0; i < n; i += 2) Ln(dc, b, q + (poly[(i * 7 + 3) % n] - q) * 0.25f, poly[i], Rgb(0.42f, 0.4f, 0.34f, 0.36f * crease * a), 3f * u);
    }
}

public sealed class PuzzleBreakPlugin : QuickShapePlugin { public override string Name => "演出：パズルのピースが外れる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PuzzleBreakParameter(s); }
public sealed class PuzzleBreakParameter : Q3TransParam
{
    public PuzzleBreakParameter() : this(null) { }
    public PuzzleBreakParameter(SharedDataStore? s) : base(s) { Count = 8; Color1 = Hex(0x3C6FB6); Color2 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("パズル", Len, Back, "Count=横のピース数||2|24", "Variant=落ちる順|1=ランダム　2=左から　3=中央から|1|3", "Color1=ピースの色", "Color2=もう一方の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, k = Cover(c); int nx = Math.Clamp(Count, 2, 24), ny = Math.Max(1, (int)MathF.Round(nx * h / w)); float cw = w / nx, ch = h / ny;
        for (int y = 0; y < ny; y++) for (int x = 0; x < nx; x++)
        {
            float order = Variant == 2 ? x / MathF.Max(1f, nx - 1f) : Variant == 3 ? MathF.Min(1f, new Vector2(x - (nx - 1) / 2f, y - (ny - 1) / 2f).Length() / (MathF.Max(nx, ny) / 2f)) : Rnd(x * 37 + y * 91 + Seed), p = Math.Clamp((k - order * 0.62f) / 0.38f, 0f, 1f); if (p <= 0f) continue; float drop = (1f - p) * (1f - p) * h * 1.15f, spin = (1f - p) * (Rnd(x + y * 7) - 0.5f) * 3f; var q = new Vector2((x + 0.5f) * cw + (1f - p) * (Rnd(x * 3 + y) - 0.5f) * 150f, (y + 0.5f) * ch + drop); var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(spin, q) * prev; var col = Col(Gfx.Mix(Color1, (x + y) % 2 == 0 ? Color1 : Color2, 0.34f), a * MathF.Min(1f, p * 4f)); Box(dc, b, q.X - cw / 2f - 0.5f, q.Y - ch / 2f - 0.5f, q.X + cw / 2f + 0.5f, q.Y + ch / 2f + 0.5f, col); float tab = MathF.Min(cw, ch) * 0.18f; if (x + 1 < nx) Dot(dc, b, q + new Vector2(cw / 2f, 0), tab, col); if (y + 1 < ny) Dot(dc, b, q + new Vector2(0, ch / 2f), tab, col); Frame(dc, b, q.X - cw / 2f, q.Y - ch / 2f, q.X + cw / 2f, q.Y + ch / 2f, Rgb(0, 0, 0, 0.16f * a), 1.5f); dc.Transform = prev;
        }
    }
}

public sealed class ScreenMeltPlugin : QuickShapePlugin { public override string Name => "演出：画面が溶ける"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ScreenMeltParameter(s); }
public sealed class ScreenMeltParameter : Q3TransParam
{
    public ScreenMeltParameter() : this(null) { }
    public ScreenMeltParameter(SharedDataStore? s) : base(s) { Count = 28; Color1 = Hex(0xE6396B); }
    static readonly Dictionary<string, ParamGuide> g = G("画面が溶ける", Len, Back, "Count=垂れる本数||4|120", "Seed=シード", "Color1=色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, k = Cover(c); if (k <= 0f) return; int n = Math.Clamp(Count, 4, 120); float cw = w / n; var top = new List<Vector2> { new(0, 0), new(w, 0) };
        for (int i = n; i >= 0; i--) { float x = i * cw, lag = Rnd(i * 13 + Seed) * 0.42f, q = Math.Clamp((k - lag) / (1f - lag), 0f, 1f), y = q * h * 1.08f; top.Add(new(x + cw * 0.45f, y)); top.Add(new(x - cw * 0.45f, y)); }
        Poly(dc, b, top, C1(a)); for (int i = 0; i <= n; i++) { float lag = Rnd(i * 13 + Seed) * 0.42f, q = Math.Clamp((k - lag) / (1f - lag), 0f, 1f); if (q <= 0f) continue; Dot(dc, b, new(i * cw, q * h * 1.08f), cw * 0.48f, C1(a)); Oval(dc, b, new(i * cw - cw * 0.14f, q * h * 1.08f - cw * 0.08f), cw * 0.07f, cw * 0.16f, Rgb(1, 1, 1, 0.45f * a)); }
    }
}

public sealed class MirrorSymmetryPlugin : QuickShapePlugin { public override string Name => "演出：鏡のシンメトリー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MirrorSymmetryParameter(s); }
public sealed class MirrorSymmetryParameter : Q3Param
{
    public MirrorSymmetryParameter() : this(null) { }
    public MirrorSymmetryParameter(SharedDataStore? s) : base(s) { Count = 22; Color1 = Hex(0x7FDBFF); Color2 = Hex(0xFF8FB8); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("鏡のシンメトリー", "Variant=向き|1=左右　2=上下　3=4方向|1|3", "Count=模様の数|映像そのものは反転できないので、左右対称の模様を重ねます|2|120", "Speed=速さ", "Color1=色1", "Color2=色2", "Color3=鏡の線の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c), cx = w / 2f, cy = h / 2f; int n = Math.Clamp(Count, 2, 120);
        for (int i = 0; i < n; i++)
        {
            float x = Gfx.Mod(Rnd(i) * cx + MathF.Sin(t * 0.6f + i) * 44f * u, cx), y = Gfx.Mod(Rnd(i + 9) * cy + t * (10f + Rnd(i + 3) * 26f) * u, cy), S = (24f + Rnd(i + 5) * 72f) * u, rot = t * (Rnd(i + 7) - 0.5f) * 2f; var col = Col(Gfx.Mix(Color1, Color2, Rnd(i + 2)), 0.72f * a);
            for (int m = 0; m < 4; m++) { if (Variant == 1 && m >= 2) break; if (Variant == 2 && m % 2 == 1) continue; bool fx = m % 2 == 1, fy = m >= 2; float px = fx ? w - x : x, py = fy ? h - y : y; if (Variant == 2) py = m == 2 ? h - y : y; else if (Variant == 1) py = y * 2f; float rr = rot * (fx ^ fy ? -1f : 1f); var q = new Vector2(px, py); var pts = new List<Vector2>(); for (int k = 0; k < 3 + i % 4; k++) pts.Add(Pol(q, S, rr + k * MathF.Tau / (3 + i % 4))); Poly(dc, b, pts, col); }
        }
        float glow = 0.55f + 0.45f * MathF.Sin(c.Time * 3f); if (Variant != 2) Ln(dc, b, new(cx, 0), new(cx, h), C3(glow * a), 4f * u); if (Variant != 1) Ln(dc, b, new(0, cy), new(w, cy), C3(glow * a), 4f * u);
    }
}

public sealed class ClockWipePlugin : QuickShapePlugin { public override string Name => "演出：時計の針ワイプ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ClockWipeParameter(s); }
public sealed class ClockWipeParameter : Q3TransParam
{
    public ClockWipeParameter() : this(null) { }
    public ClockWipeParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x14233C); Color2 = Hex(0xFFD54A); }
    protected override double DefAmount => 0;
    static readonly Dictionary<string, ParamGuide> g = G("時計の針ワイプ", Len, Back, "Amount=始まりの角度", "Variant=向き|1=時計回り　2=反時計回り|1|2", "Color1=塗る色", "Color2=針の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), k = Cover(c), R = MathF.Sqrt(w * w + h * h) * 0.75f, start = -MathF.PI / 2f + Amt(c) * MathF.Tau, dir = Variant == 2 ? -1f : 1f; var o = new Vector2(w / 2f, h / 2f); if (k <= 0f) return; if (k >= 0.999f) { Box(dc, b, 0, 0, w, h, C1(a)); return; } float end = start + dir * k * MathF.Tau; var pts = Gfx.ArcPts(o, R, start, end, 90); pts.Insert(0, o); Poly(dc, b, pts, C1(a)); Ln(dc, b, o, Pol(o, R, end), C2(a), 7f * u); Ln(dc, b, o, Pol(o, R, start), C2(0.5f * a), 3f * u); Dot(dc, b, o, 15f * u, C2(a));
    }
}

public sealed class LightningFlashPlugin : QuickShapePlugin { public override string Name => "演出：落雷の白フラッシュ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LightningFlashParameter(s); }
public sealed class LightningFlashParameter : Q3Param
{
    public LightningFlashParameter() : this(null) { }
    public LightningFlashParameter(SharedDataStore? s) : base(s) { Count = 2; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x050507); }
    protected override double DefAmount => 25;
    static readonly Dictionary<string, ParamGuide> g = G("落雷の白フラッシュ", "Count=光る回数||1|8", "Amount=1回の長さ", "Background=人物のシルエット", "Seed=シード", "Color1=光の色", "Color2=影の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U(c), per = 0.08f + Amt(c) * 0.5f, gap = per * 1.4f; int n = Math.Clamp(Count, 1, 8), i = (int)(c.Time / gap); if (i >= n) return; float f = (c.Time - i * gap) / per; if (f >= 1f) return; float al = 1f - f * f; Box(dc, b, 0, 0, w, h, C1(al * a)); var o = P0(c);
        var pts = new List<Vector2>(); float x = w * (0.25f + Rnd(i + Seed) * 0.5f); for (int k = 0; k <= 12; k++) { pts.Add(new(x, h * k / 12f * 0.62f)); x += (Rnd(i * 17 + k + Seed) - 0.5f) * 130f * u; } Path(dc, b, pts, C2(al * a), 9f * u);
        if (Background) { var col = C2(al * a); Dot(dc, b, o + new Vector2(0, -165f * u), 76f * u, col); RBox(dc, b, o + new Vector2(0, 120f * u), 250f * u, 420f * u, 70f * u, col); foreach (int s in new[] { -1, 1 }) { Ln(dc, b, o + new Vector2(s * 105f * u, -35f * u), o + new Vector2(s * 190f * u, 170f * u), col, 58f * u); Ln(dc, b, o + new Vector2(s * 60f * u, 310f * u), o + new Vector2(s * 70f * u, 610f * u), col, 74f * u); } }
    }
}

public sealed class FocusPullPlugin : QuickShapePlugin { public override string Name => "演出：ピント合わせ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FocusPullParameter(s); }
public sealed class FocusPullParameter : Q3Param
{
    public FocusPullParameter() : this(null) { }
    public FocusPullParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x5CFF9D); Color3 = Hex(0x0B0F16); }
    protected override double DefAmount => 30;
    static readonly Dictionary<string, ParamGuide> g = G("ピント合わせ", "Amount=時間|0〜100 = 0.3〜3.3秒", "Color1=枠の色", "Color2=合った時の色", "Color3=ぼかしの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), d = 0.3f + Amt(c) * 3f, k = DrawKit.EaseOut(Sub(c.Time, 0f, d)), wob = MathF.Sin(k * 18f) * (1f - k); var o = P0(c); bool ok = k >= 0.985f; var ink = ok ? C2(a) : C1(a); float R = (190f + (1f - k) * 240f + wob * 26f) * u, blurA = (1f - k) * 0.72f;
        if (blurA > 0.01f) for (int i = 0; i < 12; i++) { float th = i / 12f * MathF.Tau + c.Time * 0.3f, rr = (R + 170f * u) * (0.45f + (i % 3) * 0.28f); var hide = Pol(o, rr, th); using var br = Gfx.Radial(dc, hide, 330f * u, (0f, C3(blurA * 0.5f * a)), (1f, C3(0f))); dc.FillEllipse(new Ellipse(hide, 330f * u, 330f * u), br); }
        Ring(dc, b, o, R, ink, 5f * u); Ring(dc, b, o, R * 0.56f, new Color4(ink.R, ink.G, ink.B, ink.A * 0.45f), 3f * u); for (int i = 0; i < 4; i++) { float th = i * MathF.PI / 2f; Ln(dc, b, Pol(o, R * 0.76f, th), Pol(o, R * 1.16f, th), ink, 5f * u); } Ln(dc, b, o - new Vector2(24f * u, 0), o + new Vector2(24f * u, 0), ink, 3f * u); Ln(dc, b, o - new Vector2(0, 24f * u), o + new Vector2(0, 24f * u), ink, 3f * u); if (ok) { float pop = Sub(c.Time, d, 0.25f); Ring(dc, b, o, R * (1f + pop * 0.3f), C2((1f - pop) * a), 8f * u); }
    }
}

public sealed class ScreenCrackPlugin : QuickShapePlugin { public override string Name => "演出：画面にひびが入る"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ScreenCrackParameter(s); }
public sealed class ScreenCrackParameter : Q3Param
{
    public ScreenCrackParameter() : this(null) { }
    public ScreenCrackParameter(SharedDataStore? s) : base(s) { Count = 16; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x000000); }
    static readonly Dictionary<string, ParamGuide> g = G("画面のひび", "Count=ひびの数||3|48", "Seed=シード", "Color1=ひびの光の色", "Color2=ひびの影の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 3, 48); float u = U(c), p = DrawKit.EaseOut(Sub(c.Time, 0f, 0.22f)), flash = 1f - Sub(c.Time, 0f, 0.2f); var o = P0(c); if (flash > 0f) Box(dc, b, 0, 0, c.Width, c.Height, C1(0.45f * flash * a)); var ends = new List<List<Vector2>>();
        for (int s = 0; s < n; s++) { float th = s / (float)n * MathF.Tau + (Rnd(s + Seed) - 0.5f) * 0.25f, L = (500f + Rnd(s + 5 + Seed) * 1100f) * u; var pts = new List<Vector2> { o }; var q = o; for (int i = 1; i <= 8; i++) { float dth = th + (Rnd(s * 17 + i + Seed) - 0.5f) * 0.46f; q += new Vector2(MathF.Cos(dth), MathF.Sin(dth)) * L / 8f; pts.Add(q); } ends.Add(pts); var part = BubbleKit.PartByLength(pts, p); Path(dc, b, part, C2(0.55f * a), 6f * u); Path(dc, b, part, C1(0.95f * a), 2.5f * u); }
        if (p > 0.45f) foreach (int ringIdx in new[] { 2, 4, 6 }) for (int s = 0; s < n; s++) { if (Rnd(s * 3 + ringIdx + Seed) < 0.3f) continue; var A = ends[s][ringIdx]; var B = ends[(s + 1) % n][ringIdx]; Ln(dc, b, A, Vector2.Lerp(A, B, Sub(p, 0.45f, 0.55f)), C1(0.75f * a), 2f * u); }
        Dot(dc, b, o, 7f * u * p, C1(a));
    }
}

public sealed class CenterSuckPlugin : QuickShapePlugin { public override string Name => "演出：中心へ吸い込む"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CenterSuckParameter(s); }
public sealed class CenterSuckParameter : Q3TransParam
{
    public CenterSuckParameter() : this(null) { }
    public CenterSuckParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x07080F); Color2 = Hex(0xC9D6FF); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("中心へ吸い込む", Len, Back, "Amount=回転の強さ", "Color1=覆う色", "Color2=線の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), k = Cover(c), maxR = MathF.Sqrt(w * w + h * h) * 0.62f, hole = (1f - k) * maxR, sw = Amt(c) * 2.4f; var o = P0(c); if (k <= 0f) return;
        if (hole < 2f) Box(dc, b, 0, 0, w, h, C1(a)); else { var mask = new List<Vector2> { new(-10, -10), new(w + 10, -10), new(w + 10, o.Y) }; for (int i = 0; i <= 64; i++) { float th = i / 64f * MathF.Tau; mask.Add(o + new Vector2(MathF.Cos(th), MathF.Sin(th)) * hole); } mask.Add(new(w + 10, o.Y)); mask.Add(new(w + 10, h + 10)); mask.Add(new(-10, h + 10)); Poly(dc, b, mask, C1(a)); }
        for (int i = 0; i < 70; i++) { float th0 = Rnd(i) * MathF.Tau, rr = hole + (20f + Rnd(i + 3) * 1000f) * u * (0.25f + k), len = (40f + Rnd(i + 5) * 220f) * u; var line = new List<Vector2>(); for (int s = 0; s <= 8; s++) { float e = s / 8f, r = rr + len * e; line.Add(Pol(o, r, th0 + sw * (1f - r / (maxR + 500f * u)) * k * 2f)); } Path(dc, b, line, C2((0.15f + Rnd(i + 7) * 0.48f) * MathF.Min(1f, k * 2f) * a), 2f * u); }
    }
}

public sealed class ColorRipplePlugin : QuickShapePlugin { public override string Name => "演出：色の波が広がる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ColorRippleParameter(s); }
public sealed class ColorRippleParameter : Q3TransParam
{
    public ColorRippleParameter() : this(null) { }
    public ColorRippleParameter(SharedDataStore? s) : base(s) { Count = 3; Color1 = Hex(0xFF5C8A); Color2 = Hex(0xFFD54A); Color3 = Hex(0x4FC3F7); }
    static readonly Dictionary<string, ParamGuide> g = G("色の波", Len, Back, "Count=色の数||1|3", "Color1=色1", "Color2=色2", "Color3=色3");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, k = Cover(c); int n = Math.Clamp(Count, 1, 3); var o = P0(c); float far = 0f; foreach (var e in new[] { new Vector2(0, 0), new Vector2(w, 0), new Vector2(0, h), new Vector2(w, h) }) far = MathF.Max(far, (e - o).Length()); far += 4f; for (int i = 0; i < n; i++) { float lag = i * 0.22f, q = Math.Clamp((k - lag) / MathF.Max(0.05f, 1f - lag), 0f, 1f), R = q * far; Dot(dc, b, o, R, i == 0 ? C1(a) : i == 1 ? C2(a) : C3(a)); }
    }
}

public sealed class PhoneSwipePlugin : QuickShapePlugin { public override string Name => "演出：スマホのスワイプ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PhoneSwipeParameter(s); }
public sealed class PhoneSwipeParameter : Q3TransParam
{
    public PhoneSwipeParameter() : this(null) { }
    public PhoneSwipeParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x2F80ED); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("スマホのスワイプ", Len, Back, "Variant=向き|1=左へ　2=右へ|1|2", "Count=指の表示|0=なし　1=あり|0|1", "Color1=次の画面の色", "Color2=指の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), k = Cover(c), dir = Variant == 2 ? 1f : -1f; if (k <= 0f) return; float x0 = dir < 0f ? w * (1f - k) : -w + w * k; Box(dc, b, x0, 0, x0 + w, h, C1(a)); Box(dc, b, dir < 0f ? x0 - 14f * u : x0 + w, 0, dir < 0f ? x0 : x0 + w + 14f * u, h, Rgb(0, 0, 0, 0.3f * a)); if (Count != 0 && k < 0.985f) { var f = new Vector2(dir < 0f ? Lerp(w * 0.82f, w * 0.14f, k) : Lerp(w * 0.18f, w * 0.86f, k), h * 0.56f); for (int i = 1; i <= 5; i++) Dot(dc, b, f - new Vector2(dir * i * 34f * u, 0), (30f - i * 4f) * u, C2((0.32f - i * 0.05f) * a)); Dot(dc, b, f, 54f * u, C2(0.3f * a)); Dot(dc, b, f, 34f * u, C2(0.95f * a)); }
    }
}

public sealed class BubblePopRevealPlugin : QuickShapePlugin { public override string Name => "演出：ぽよんと弾けて出る"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BubblePopRevealParameter(s); }
public sealed class BubblePopRevealParameter : Q3Param
{
    public BubblePopRevealParameter() : this(null) { }
    public BubblePopRevealParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFF7FB6); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("ぽよんと弾ける", "Variant=形|1=丸　2=ハート　3=星|1|3", "Amount=弾む強さ", "Color1=色", "Color2=光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, grow = Sub(t, 0f, 0.65f), pop = Sub(t, 0.65f, 0.42f), amp = Amt(c) * 0.32f, wob = 1f + MathF.Sin(grow * 15f) * amp * (1f - grow), R = 300f * u * DrawKit.EaseOut(grow) * wob; var o = P0(c);
        if (pop <= 0f) { if (Variant == 2) Heart(dc, b, o, R, 0f, C1(a)); else if (Variant == 3) Star(dc, b, o, R * 1.15f, 5, 0.52f, -MathF.PI / 2f, C1(a)); else Oval(dc, b, o, R * (2f - wob), R * wob, C1(a)); Oval(dc, b, o - new Vector2(R * 0.34f, R * 0.38f), R * 0.2f, R * 0.12f, C2(0.8f * a)); return; }
        if (pop >= 1f) return; Ring(dc, b, o, 300f * u + pop * 360f * u, C1((1f - pop) * a), 22f * u * (1f - pop)); for (int i = 0; i < 34; i++) { float th = Rnd(i) * MathF.Tau, d = (220f + Rnd(i + 5) * 460f) * u * DrawKit.EaseOut(pop); var q = Pol(o, d, th) + new Vector2(0, pop * pop * 150f * u); Dot(dc, b, q, (6f + Rnd(i + 9) * 24f) * u * (1f - pop), i % 3 == 0 ? C2(a) : C1(a)); }
    }
}

public sealed class WordFloodPlugin : QuickShapePlugin { public override string Name => "演出：文字で埋まる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WordFloodParameter(s); }
public sealed class WordFloodParameter : Q3TransParam
{
    public WordFloodParameter() : this(null) { }
    public WordFloodParameter(SharedDataStore? s) : base(s) { Text = "ゆっくり\n見ていってね\n草\nすごい\nなるほど"; Color1 = Hex(0x101218); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xFFD54A); }
    protected override double DefAmount2 => 35;
    static readonly Dictionary<string, ParamGuide> g = G("文字で埋まる", "Text=言葉|1行に1つ", Len, Back, "Seed=シード", "Color1=背景の色", "Color2=文字の色", "Color3=強調の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return; float w = c.Width, h = c.Height, u = U0(c), k = Cover(c); if (k <= 0f) return; const int nx = 11, ny = 15; float cw = w / nx, ch = h / ny; Box(dc, b, 0, 0, w, h, C1(MathF.Max(0f, k * 1.7f - 0.7f) * a));
        for (int y = 0; y < ny; y++) for (int x = 0; x < nx + 1; x++) { int id = x * 41 + y * 73 + Seed; float at = Rnd(id) * 0.82f, p = Math.Clamp((k - at) / 0.18f, 0f, 1f); if (p <= 0f) continue; string s = r[(int)(Rnd(id + 3) * r.Count)]; var q = new Vector2((x + (y % 2) * 0.5f) * cw, (y + 0.5f) * ch); float size = (24f + Rnd(id + 7) * 38f) * u, rot = (Rnd(id + 9) - 0.5f) * 0.32f; var prev = dc.Transform; dc.Transform = Matrix3x2.CreateRotation(rot, q) * prev; Say(dc, b, s, size, q, Rnd(id + 11) > 0.8f ? C3(a) : C2((0.62f + Rnd(id + 13) * 0.38f) * a), maxW: cw * 1.55f, scale: DrawKit.EaseOutBack(p)); dc.Transform = prev; }
    }
}

public sealed class FireSpreadPlugin : QuickShapePlugin { public override string Name => "演出：炎が燃え広がる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FireSpreadParameter(s); }
public sealed class FireSpreadParameter : Q3TransParam
{
    public FireSpreadParameter() : this(null) { }
    public FireSpreadParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x220805); Color2 = Hex(0xFF5A14); Color3 = Hex(0xFFD12E); }
    static readonly Dictionary<string, ParamGuide> g = G("炎が燃え広がる", Len, Back, "Variant=向き|1=下から　2=上から|1|2", "Color1=焼けた跡の色", "Color2=炎の色", "Color3=炎の中心の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, k = Cover(c); if (k <= 0f) return; bool down = Variant == 2; float edge = k * (h + 340f * u) - 170f * u; float Y(float v) => down ? v : h - v; List<Vector2> Band(float off, float amp, float ph) { var p = new List<Vector2> { new(0, Y(-20f * u)), new(w, Y(-20f * u)) }; for (int i = 48; i >= 0; i--) { float x = i / 48f, fl = MathF.Abs(MathF.Sin(x * 17f + t * 6f + ph)) * 0.65f + MathF.Abs(MathF.Sin(x * 41f - t * 8f + ph)) * 0.35f; p.Add(new(x * w, Y(edge + off + fl * amp))); } return p; }
        Poly(dc, b, Band(150f * u, 150f * u, 0f), C2(a)); Poly(dc, b, Band(75f * u, 115f * u, 1.7f), C3(a)); Poly(dc, b, Band(-20f * u, 62f * u, 3.1f), C1(a)); for (int i = 0; i < 60; i++) { float f = Gfx.Mod(t * (0.45f + Rnd(i) * 0.75f) + Rnd(i + 3), 1f), x = Rnd(i + 5) * w + MathF.Sin(f * 8f + i) * 30f * u; Dot(dc, b, new(x, Y(edge + 110f * u + f * 430f * u)), (2f + Rnd(i + 8) * 4f) * u, C3(a * (1f - f))); }
    }
}

public sealed class WaterRiseFillPlugin : QuickShapePlugin { public override string Name => "演出：水が満ちる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new WaterRiseFillParameter(s); }
public sealed class WaterRiseFillParameter : Q3TransParam
{
    public WaterRiseFillParameter() : this(null) { }
    public WaterRiseFillParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x1787D9); Color2 = Hex(0x75D8FF); }
    protected override double DefAmount => 35;
    static readonly Dictionary<string, ParamGuide> g = G("水が満ちる", Len, Back, "Amount=波の高さ", "Color1=水の色", "Color2=波の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time, k = Cover(c); if (k <= 0f) return; float amp = (8f + Amt(c) * 60f) * u, top = h + amp * 2f - k * (h + amp * 4f); List<Vector2> Wave(float dy, float ph, float sp) { var p = new List<Vector2> { new(0, h + 10f), new(w, h + 10f) }; for (int i = 60; i >= 0; i--) { float x = i / 60f; p.Add(new(x * w, top + dy + MathF.Sin(x * 8f + t * sp + ph) * amp + MathF.Sin(x * 19f - t * sp * 0.8f + ph) * amp * 0.35f)); } return p; }
        Poly(dc, b, Wave(-14f * u, 2.1f, 2.1f), C2(0.65f * a)); Poly(dc, b, Wave(0f, 0f, 2.8f), C1(a)); for (int i = 0; i < 46; i++) { float f = Gfx.Mod(t * (0.25f + Rnd(i) * 0.4f) + Rnd(i + 3), 1f), x = Rnd(i + 5) * w + MathF.Sin(f * 9f + i) * 18f * u, y = h - f * (h - top); if (y < top + 18f * u) continue; Ring(dc, b, new(x, y), (4f + Rnd(i + 8) * 13f) * u, Rgb(1, 1, 1, 0.55f * a), 2f * u); }
    }
}

public sealed class PaintSplashPlugin : QuickShapePlugin { public override string Name => "演出：絵の具のスプラッシュ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PaintSplashParameter(s); }
public sealed class PaintSplashParameter : Q3TransParam
{
    public PaintSplashParameter() : this(null) { }
    public PaintSplashParameter(SharedDataStore? s) : base(s) { Count = 3; Color1 = Hex(0xFF3D81); Color2 = Hex(0x29C7F6); Color3 = Hex(0xFFD12E); }
    static readonly Dictionary<string, ParamGuide> g = G("絵の具のスプラッシュ", Len, Back, "Count=色の数||1|3", "Seed=飛び散る位置のシード", "Color1=色1", "Color2=色2", "Color3=色3");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), k = Cover(c), far = MathF.Sqrt(w * w + h * h) * 1.05f; int n = Math.Clamp(Count, 1, 3); if (k <= 0f) return;
        for (int l = 0; l < n; l++) { float lag = l * 0.2f, q = Math.Clamp((k - lag) / MathF.Max(0.1f, 1f - lag), 0f, 1f); if (q <= 0f) continue; var col = l == 0 ? C1(a) : l == 1 ? C2(a) : C3(a); var o = new Vector2(w * (0.2f + Rnd(l + Seed) * 0.6f), h * (0.2f + Rnd(l + 9 + Seed) * 0.6f)); float R = q * q * far; var blob = new List<Vector2>(); for (int i = 0; i < 44; i++) { float th = i / 44f * MathF.Tau, sp = 1f + (Rnd(i * 3 + l + Seed) - 0.3f) * 0.55f + (i % 5 == 0 ? 0.42f : 0f); blob.Add(Pol(o, R * sp, th)); } Poly(dc, b, blob, col); for (int i = 0; i < 26; i++) { float th = Rnd(i + l * 40 + Seed) * MathF.Tau, d = R * (1.1f + Rnd(i + 8 + Seed) * 0.8f) + 30f * u * q; Dot(dc, b, Pol(o, d, th), (5f + Rnd(i + 5) * 34f) * u * MathF.Min(1f, q * 3f), col); } }
    }
}

public sealed class LightTunnelPlugin : QuickShapePlugin { public override string Name => "演出：光のトンネルを抜ける"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LightTunnelParameter(s); }
public sealed class LightTunnelParameter : Q3TransParam
{
    public LightTunnelParameter() : this(null) { }
    public LightTunnelParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x54D6FF); Color2 = Hex(0xB05CFF); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount2 => 45;
    static readonly Dictionary<string, ParamGuide> g = G("光のトンネル", Len, Back, "Variant=輪の形|1=丸　2=四角　3=六角|1|3", "Speed=速さ", "Color1=色1", "Color2=色2", "Color3=抜ける光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float w = c.Width, h = c.Height, u = U0(c), t = c.Time * Spd(c), k = Cover(c), far = MathF.Sqrt(w * w + h * h) * 0.68f; var o = new Vector2(w / 2f, h / 2f); int sides = Variant == 2 ? 4 : Variant == 3 ? 6 : 48; if (k <= 0f) return; Box(dc, b, 0, 0, w, h, Rgb(0.01f, 0.02f, 0.06f, MathF.Min(1f, k * 3f) * a));
        for (int i = 0; i < 18; i++) { float z = Gfx.Mod(i / 18f + t * 0.65f, 1f), R = z * z * z * far * 1.4f + 6f * u, al = MathF.Min(1f, z * 3f) * (1f - z * 0.2f); var col = Col(Gfx.Mix(Color1, Color2, (MathF.Sin(i * 0.7f) + 1f) / 2f), al * MathF.Min(1f, k * 3f) * a); var pts = new List<Vector2>(); for (int s = 0; s < sides; s++) pts.Add(Pol(o, R, s * MathF.Tau / sides + (sides == 4 ? MathF.PI / 4f : 0f) + z * 0.3f)); Outline(dc, b, pts, col, (2f + z * z * 24f) * u); }
        float white = Math.Clamp((k - 0.7f) / 0.3f, 0f, 1f); if (white > 0f) { using var br = Gfx.Radial(dc, o, far * (0.25f + white), (0f, C3(a)), (MathF.Min(0.98f, white), C3(a)), (1f, C3(white * a))); dc.FillRectangle(new Vortice.RawRectF(0, 0, w, h), br); }
    }
}
