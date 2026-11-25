using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 431〜450: ゲーム・RPG

public sealed class BattleLogWindowPlugin : QuickShapePlugin { public override string Name => "演出：バトルメッセージ窓"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BattleLogWindowParameter(s); }
public sealed class BattleLogWindowParameter : Q3Param
{
    public BattleLogWindowParameter() : this(null) { }
    public BattleLogWindowParameter(SharedDataStore? s) : base(s) { Text = "ゆっくり魔理沙の攻撃！\nスライムに 25 のダメージ！\nスライムをたおした！"; Color1 = Hex(0x101828); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("バトルメッセージ窓", "Text=メッセージ|1行に1つ", "Speed=送る速さ", "Color1=窓の色", "Color2=文字とふちの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return;
        float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)); var o = P0(c) + new Vector2(0, 300f * u);
        Panel(dc, b, o, 1500f * u, 300f * u, 18f * u, C1(0.92f * a), C2(a), 6f * u);
        int shown = 0; float at = 0.15f;
        for (; shown < r.Count; shown++) { if (t < at) break; at += r[shown].Length * 0.045f + 0.55f; }
        int first = Math.Max(0, shown - 3); at = 0.15f;
        for (int i = 0; i < first; i++) at += r[i].Length * 0.045f + 0.55f;
        for (int i = first; i < shown; i++)
        {
            int n = Math.Clamp((int)((t - at) / 0.045f), 0, r[i].Length);
            SayAt(dc, b, r[i][..n], 48f * u, new(o.X - 700f * u, o.Y - 118f * u + (i - first) * 78f * u), C2(a));
            at += r[i].Length * 0.045f + 0.55f;
        }
    }
}

public sealed class CriticalHitPlugin : QuickShapePlugin { public override string Name => "演出：クリティカル表示"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CriticalHitParameter(s); }
public sealed class CriticalHitParameter : Q3Param
{
    public CriticalHitParameter() : this(null) { }
    public CriticalHitParameter(SharedDataStore? s) : base(s) { Text = "9999\nCRITICAL!"; Color1 = Hex(0xFF3D3D); Color2 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("クリティカル表示", "Text=数と文字|1行目=ダメージ　2行目=文字", "Color1=数の色", "Color2=文字と光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), t = c.Time, p = DrawKit.EaseOutBack(Sub(t, 0f, 0.28f)); var o = P0(c);
        for (int i = 0; i < 18; i++) { float th = i / 18f * MathF.Tau + Rnd(i) * 0.2f, l = (220f + Rnd(i + 40) * 220f) * u * DrawKit.EaseOut(Sub(t, 0f, 0.3f)); Ln(dc, b, Pol(o, l * 0.55f, th), Pol(o, l, th), C2(a * (1f - Sub(t, 0.2f, 0.5f))), 8f * u); }
        Star(dc, b, o, 330f * u * p, 12, 0.68f, t * 0.3f, C2(0.25f * a));
        float shake = MathF.Sin(t * 70f) * 10f * u * (1f - Sub(t, 0.1f, 0.4f));
        Say(dc, b, r.Count > 0 ? r[0] : "", 190f * u, o + new Vector2(shake, 20f * u), C1(a), Rgb(1, 1, 1, a), 9f * u, scale: p);
        Say(dc, b, r.Count > 1 ? r[1] : "", 74f * u, o + new Vector2(-shake, -135f * u), C2(a), Rgb(0, 0, 0, a), 6f * u, scale: DrawKit.EaseOutBack(Sub(t, 0.12f, 0.25f)));
    }
}

public sealed class MissDodgePlugin : QuickShapePlugin { public override string Name => "演出：ミス・回避の表示"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MissDodgeParameter(s); }
public sealed class MissDodgeParameter : Q3Param
{
    public MissDodgeParameter() : this(null) { }
    public MissDodgeParameter(SharedDataStore? s) : base(s) { Text = "MISS"; Color1 = Hex(0xB0BEC5); }
    static readonly Dictionary<string, ParamGuide> g = G("ミス・回避", "Text=文字", "Variant=動きの向き|1=上　2=右　3=左　4=下|1|4", "Color1=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), k = Sub(c.Time, 0f, 0.9f); var d = Variant switch { 2 => new Vector2(1, 0), 3 => new Vector2(-1, 0), 4 => new Vector2(0, 1), _ => new Vector2(0, -1) };
        var q = P0(c) + d * DrawKit.EaseOut(k) * 130f * u + new Vector2(MathF.Sin(k * 9f) * 8f * u, 0);
        Say(dc, b, Str, 110f * u, q, C1(a * (1f - Sub(k, 0.65f, 0.35f))), Rgb(0, 0, 0, 0.7f * a * (1f - Sub(k, 0.65f, 0.35f))), 6f * u);
    }
}

public sealed class GemStonePlugin : QuickShapePlugin { public override string Name => "演出：宝石・ジェム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new GemStoneParameter(s); }
public sealed class GemStoneParameter : Q3Param
{
    public GemStoneParameter() : this(null) { }
    public GemStoneParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xE91E63); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("宝石", "Variant=形|1=丸　2=ひし形　3=六角|1|3", "Speed=回る速さ", "Color1=宝石の色", "Color2=光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), R = 220f * u * DrawKit.EaseOutBack(Intro(c, 0f, 0.45f)); var o = P0(c); int n = Variant == 2 ? 4 : Variant == 3 ? 6 : 12; float rot = t * 0.8f;
        var outer = new Vector2[n]; var inner = new Vector2[n];
        for (int i = 0; i < n; i++) { float th = rot + i * MathF.Tau / n; outer[i] = o + new Vector2(MathF.Cos(th) * R, MathF.Sin(th) * R * 0.92f); inner[i] = o + new Vector2(MathF.Cos(th + MathF.PI / n) * R * 0.52f, MathF.Sin(th + MathF.PI / n) * R * 0.48f); }
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n; float lit = 0.5f + 0.5f * MathF.Sin(i * 2.1f + t * 3f);
            Poly(dc, b, new List<Vector2> { outer[i], outer[j], inner[i] }, Col(Gfx.Mix(Color1, Color2, lit * 0.45f), a));
            Poly(dc, b, new List<Vector2> { inner[i], outer[j], inner[j] }, Col(Gfx.Mix(Color1, Hex(0x000000), 0.15f + (1f - lit) * 0.25f), a));
        }
        Poly(dc, b, inner, Col(Gfx.Mix(Color1, Color2, 0.3f), a));
        float tw = MathF.Pow(MathF.Max(0f, MathF.Sin(t * 2.3f)), 8f); Star(dc, b, o + new Vector2(-R * 0.35f, -R * 0.4f), 70f * u * tw, 4, 0.14f, 0f, C2(a));
    }
}

public sealed class CoinGainPlugin : QuickShapePlugin { public override string Name => "演出：コイン獲得（＋の数が飛ぶ）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CoinGainParameter(s); }
public sealed class CoinGainParameter : Q3Param
{
    public CoinGainParameter() : this(null) { }
    public CoinGainParameter(SharedDataStore? s) : base(s) { Text = "500\n1200"; Count = 10; Color1 = Hex(0xFFC107); Color2 = Hex(0x1B2433); }
    static readonly Dictionary<string, ParamGuide> g = G("コイン獲得", "Text=数|1行目=増える数　2行目=もとの所持金", "Count=コインの数||1|40", "Color1=コインの色", "Color2=所持金の欄の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float add = r.Count > 0 ? Num(r[0]) : 0f, baseV = r.Count > 1 ? Num(r[1]) : 0f, u = U(c), t = c.Time; var o = P0(c); int n = Math.Clamp(Count, 1, 40); var bar = o + new Vector2(520f * u, -340f * u);
        Panel(dc, b, bar, 430f * u, 92f * u, 46f * u, C2(0.92f * a), C1(a), 4f * u); Dot(dc, b, bar - new Vector2(165f * u, 0), 30f * u, C1(a));
        Say(dc, b, (baseV + add * DrawKit.EaseOut(Sub(t, 0.7f, 0.9f))).ToString("#,0"), 46f * u, bar + new Vector2(35f * u, 0), Rgb(1, 1, 1, a));
        for (int i = 0; i < n; i++)
        {
            float k = Sub(t, i * 0.05f, 0.95f); if (k <= 0f || k >= 1f) continue;
            var start = o + new Vector2((Rnd(i) - 0.5f) * 180f * u, 0); var mid = start + new Vector2((Rnd(i + 30) - 0.5f) * 460f * u, -(180f + Rnd(i + 60) * 180f) * u); float e = k * k;
            var q = Vector2.Lerp(Vector2.Lerp(start, mid, k), Vector2.Lerp(mid, bar, k), e); float sx = MathF.Max(0.15f, MathF.Abs(MathF.Cos(t * 10f + i)));
            Oval(dc, b, q, 28f * u * sx, 28f * u, C1(a)); Oval(dc, b, q, 17f * u * sx, 17f * u, Rgb(1, 0.92f, 0.55f, a));
        }
        float pop = Sub(t, 0f, 0.8f); Say(dc, b, "+" + add.ToString("#,0"), 88f * u, o - new Vector2(0, 90f * u + pop * 80f * u), C1(a * (1f - Sub(pop, 0.7f, 0.3f))), Rgb(0, 0, 0, 0.7f * a * (1f - Sub(pop, 0.7f, 0.3f))), 5f * u);
    }
}

public sealed class EnemyVanishPlugin : QuickShapePlugin { public override string Name => "演出：敵の消滅（光の粒になる）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new EnemyVanishParameter(s); }
public sealed class EnemyVanishParameter : Q3Param
{
    public EnemyVanishParameter() : this(null) { }
    public EnemyVanishParameter(SharedDataStore? s) : base(s) { Count = 80; Color1 = Hex(0x7E57C2); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xB39DDB); }
    static readonly Dictionary<string, ParamGuide> g = G("敵の消滅", "Count=粒の数||10|300", "Variant=散る向き|1=全方向　2=上　3=右　4=左|1|4", "Color1=敵の色", "Color2=光の色", "Color3=粒の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, white = Sub(t, 0.15f, 0.3f), gone = Sub(t, 0.5f, 0.12f); var o = P0(c);
        if (gone < 1f)
        {
            var col = Col(Gfx.Mix(Color1, Color2, white), a * (1f - gone));
            Oval(dc, b, o + new Vector2(0, 30f * u), 170f * u, 135f * u, col); Dot(dc, b, o + new Vector2(-60f * u, -5f * u), 16f * u, Rgb(0.1f, 0.05f, 0.12f, a * (1f - white))); Dot(dc, b, o + new Vector2(60f * u, -5f * u), 16f * u, Rgb(0.1f, 0.05f, 0.12f, a * (1f - white)));
            Poly(dc, b, new List<Vector2> { o + new Vector2(-120f * u, -60f * u), o + new Vector2(-155f * u, -170f * u), o + new Vector2(-40f * u, -100f * u) }, col); Poly(dc, b, new List<Vector2> { o + new Vector2(120f * u, -60f * u), o + new Vector2(155f * u, -170f * u), o + new Vector2(40f * u, -100f * u) }, col);
        }
        int n = Math.Clamp(Count, 10, 300); var bias = Variant switch { 2 => new Vector2(0, -1), 3 => new Vector2(1, 0), 4 => new Vector2(-1, 0), _ => Vector2.Zero };
        for (int i = 0; i < n; i++)
        {
            float k = Sub(t, 0.5f + Rnd(i) * 0.18f, 0.9f + Rnd(i + 9) * 0.6f); if (k <= 0f || k >= 1f) continue;
            float th = Rnd(i + 20) * MathF.Tau, sp = (120f + Rnd(i + 40) * 420f) * u; var dir = new Vector2(MathF.Cos(th), MathF.Sin(th)) + bias * 1.3f;
            var q = o + new Vector2((Rnd(i + 60) - 0.5f) * 260f * u, (Rnd(i + 70) - 0.5f) * 230f * u) + dir * DrawKit.EaseOut(k) * sp;
            Dot(dc, b, q, (3f + Rnd(i + 80) * 7f) * u * (1f - k * 0.6f), i % 3 == 0 ? C2(a * (1f - k)) : C3(a * (1f - k)));
        }
    }
}

public sealed class ExpOrbsPlugin : QuickShapePlugin { public override string Name => "演出：経験値の玉が吸い込まれる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ExpOrbsParameter(s); }
public sealed class ExpOrbsParameter : Q3Param
{
    public ExpOrbsParameter() : this(null) { }
    public ExpOrbsParameter(SharedDataStore? s) : base(s) { Count = 36; Color1 = Hex(0x76FF7A); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 50;
    protected override double DefAmount2 => 50;
    static readonly Dictionary<string, ParamGuide> g = G("経験値の玉", "Count=玉の数||1|200", "Amount=吸い込む先 X", "Amount2=吸い込む先 Y", "Color1=玉の色", "Color2=中心の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time; var goal = new Vector2(Amt(c) * c.Width, Amt2(c) * c.Height); int n = Math.Clamp(Count, 1, 200); float glow = 0f;
        for (int i = 0; i < n; i++)
        {
            float start = Rnd(i) * 1.1f, k = Sub(t, start, 0.9f); if (k <= 0f) continue; if (k >= 1f) { glow = MathF.Max(glow, 1f - Sub(t, start + 0.9f, 0.3f)); continue; }
            float th = Rnd(i + 11) * MathF.Tau, R0 = (360f + Rnd(i + 22) * 460f) * u, e = k * k * k; var q = Pol(goal, R0 * (1f - e), th + k * 2.2f);
            Dot(dc, b, q, 17f * u, C1(0.25f * a)); Dot(dc, b, q, 8f * u, C1(a)); Dot(dc, b, q, 3.5f * u, C2(a));
        }
        if (glow > 0f) { Dot(dc, b, goal, 110f * u * glow, C1(0.2f * a * glow)); Dot(dc, b, goal, 42f * u * glow, C2(0.6f * a * glow)); }
    }
}

public sealed class ShopWindowPlugin : QuickShapePlugin { public override string Name => "演出：ショップ画面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ShopWindowParameter(s); }
public sealed class ShopWindowParameter : Q3Param
{
    public ShopWindowParameter() : this(null) { }
    public ShopWindowParameter(SharedDataStore? s) : base(s) { Text = "やくそう:10\nどくけし:25\n銅のつるぎ:180\n皮のよろい:260"; Count = 2; Color1 = Hex(0x13233A); Color2 = Hex(0xFFD54A); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount => 12;
    static readonly Dictionary<string, ParamGuide> g = G("ショップ画面", "Text=商品と値段|1行に「商品:値段」", "Count=選ぶ行||1|20", "Amount=所持金|×100 G", "Color1=窓の色", "Color2=カーソルの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows.Select(Cells).Where(x => x.Length >= 2).ToList(); if (r.Count == 0) return;
        float u = U(c), H = 190f * u + r.Count * 92f * u; var o = P0(c); int pick = Math.Clamp(Count, 1, r.Count) - 1; float cur = DrawKit.EaseOut(Sub(c.Time, 0.3f, 0.5f)) * pick;
        Panel(dc, b, o, 980f * u, H, 18f * u, C1(0.95f * a), C3(a), 5f * u); Say(dc, b, "ショップ", 50f * u, new(o.X, o.Y - H / 2f + 52f * u), C2(a));
        float y0 = o.Y - H / 2f + 150f * u; Box(dc, b, o.X - 450f * u, y0 + cur * 92f * u - 40f * u, o.X + 450f * u, y0 + cur * 92f * u + 40f * u, C2(0.2f * a));
        Poly(dc, b, new List<Vector2> { new(o.X - 435f * u, y0 + cur * 92f * u - 18f * u), new(o.X - 405f * u, y0 + cur * 92f * u), new(o.X - 435f * u, y0 + cur * 92f * u + 18f * u) }, C2(a));
        for (int i = 0; i < r.Count; i++) { SayAt(dc, b, r[i][0], 44f * u, new(o.X - 380f * u, y0 + i * 92f * u - 28f * u), C3(a), maxW: 520f * u); Say(dc, b, r[i][1] + " G", 44f * u, new(o.X + 330f * u, y0 + i * 92f * u), C2(a)); }
        Say(dc, b, "所持金 " + (Amt(c) * 10000f).ToString("#,0") + " G", 36f * u, new(o.X + 250f * u, o.Y + H / 2f - 34f * u), C3(a));
    }
}

public sealed class EquipWindowPlugin : QuickShapePlugin { public override string Name => "演出：装備画面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new EquipWindowParameter(s); }
public sealed class EquipWindowParameter : Q3Param
{
    public EquipWindowParameter() : this(null) { }
    public EquipWindowParameter(SharedDataStore? s) : base(s) { Text = "ゆっくり霊夢\n頭:赤いリボン\n体:巫女の服\n武器:お祓い棒\n盾:陰陽玉"; Color1 = Hex(0x15253C); Color2 = Hex(0x4FC3F7); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("装備画面", "Text=名前と装備|1行目=キャラ名。2行目から「枠:装備名」", "Color1=窓の色", "Color2=枠の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return;
        float u = U(c); var o = P0(c); Panel(dc, b, o, 1120f * u, 690f * u, 22f * u, C1(0.95f * a), C2(a), 5f * u); Say(dc, b, r[0], 54f * u, o + new Vector2(-300f * u, -280f * u), C3(a), maxW: 460f * u);
        var body = o + new Vector2(-300f * u, 40f * u); Dot(dc, b, body - new Vector2(0, 150f * u), 64f * u, C2(0.75f * a)); RBox(dc, b, body + new Vector2(0, 40f * u), 190f * u, 250f * u, 50f * u, C2(0.55f * a));
        for (int i = 1; i < r.Count && i <= 6; i++)
        {
            var f = r[i].Split([':', '：'], 2); float p = DrawKit.EaseOut(Intro(c, 0.1f + i * 0.09f, 0.4f)), y = o.Y - 250f * u + (i - 1) * 105f * u, x = o.X + 240f * u + (1f - p) * 60f * u; var q = new Vector2(x, y);
            Panel(dc, b, q, 560f * u, 88f * u, 12f * u, Rgb(1, 1, 1, 0.07f * a * p), C2(a * p), 2f * u); SayAt(dc, b, f[0], 30f * u, q + new Vector2(-260f * u, -21f * u), C2(a * p)); SayAt(dc, b, f.Length > 1 ? f[1].Trim() : "", 38f * u, q + new Vector2(-130f * u, -26f * u), C3(a * p), maxW: 370f * u);
        }
    }
}

public sealed class StatComparePlugin : QuickShapePlugin { public override string Name => "演出：ステータスの比較（↑↓）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new StatCompareParameter(s); }
public sealed class StatCompareParameter : Q3Param
{
    public StatCompareParameter() : this(null) { }
    public StatCompareParameter(SharedDataStore? s) : base(s) { Text = "攻撃力:120:165\n防御力:88:80\n素早さ:54:62\n魔力:30:30"; Color1 = Hex(0x13233A); Color2 = Hex(0x66FF8A); Color3 = Hex(0xFF5C5C); }
    static readonly Dictionary<string, ParamGuide> g = G("ステータスの比較", "Text=項目と前後の数|1行に「項目:前:後」", "Color1=窓の色", "Color2=増えた色", "Color3=減った色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows.Select(Cells).Where(x => x.Length >= 3).ToList(); if (r.Count == 0) return;
        float u = U(c), rh = 96f * u, H = rh * r.Count + 70f * u; var o = P0(c); Panel(dc, b, o, 900f * u, H, 18f * u, C1(0.95f * a), Rgb(1, 1, 1, 0.7f * a), 4f * u);
        for (int i = 0; i < r.Count; i++)
        {
            float y = o.Y - rh * r.Count / 2f + (i + 0.5f) * rh, v0 = Num(r[i][1]), v1 = Num(r[i][2]), p = DrawKit.EaseOut(Intro(c, 0.25f + i * 0.15f, 0.5f)), now = v0 + (v1 - v0) * p; var tone = v1 > v0 ? C2(a) : v1 < v0 ? C3(a) : Rgb(0.8f, 0.8f, 0.8f, a);
            SayAt(dc, b, r[i][0], 42f * u, new(o.X - 400f * u, y - 27f * u), Rgb(1, 1, 1, a), maxW: 300f * u); Say(dc, b, v0.ToString("0"), 42f * u, new(o.X - 20f * u, y), Rgb(0.8f, 0.8f, 0.8f, a));
            Say(dc, b, "→", 38f * u, new(o.X + 70f * u, y), Rgb(1, 1, 1, 0.7f * a)); Say(dc, b, now.ToString("0"), 48f * u, new(o.X + 170f * u, y), tone);
            if (v1 != v0) { float d = v1 > v0 ? -1f : 1f, ax = o.X + 280f * u; Poly(dc, b, new List<Vector2> { new(ax, y + d * 24f * u), new(ax + 22f * u, y - d * 18f * u), new(ax - 22f * u, y - d * 18f * u) }, new Color4(tone.R, tone.G, tone.B, tone.A * p)); Say(dc, b, (v1 - v0).ToString("+0;-0"), 34f * u, new(o.X + 355f * u, y), new Color4(tone.R, tone.G, tone.B, tone.A * p)); }
        }
    }
}

public sealed class DungeonMapPlugin : QuickShapePlugin { public override string Name => "演出：ダンジョンのマップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DungeonMapParameter(s); }
public sealed class DungeonMapParameter : Q3Param
{
    public DungeonMapParameter() : this(null) { }
    public DungeonMapParameter(SharedDataStore? s) : base(s) { Count = 12; Color1 = Hex(0x263238); Color2 = Hex(0x80CBC4); Color3 = Hex(0xFFD54A); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("ダンジョンのマップ", "Count=部屋の数||3|30", "Amount=いまの部屋|0=最初　100=最後", "Seed=シード", "Color1=未踏の部屋の色", "Color2=訪れた部屋の色", "Color3=いまの部屋の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 3, 30), seed = Seed * 97 + 5; float u = U(c), s = 118f * u, now = Amt(c) * (n - 1) * DrawKit.EaseOut(Intro(c, 0.2f, 1.2f)); var o = P0(c);
        var path = new List<(int x, int y)> { (0, 0) }; var used = new HashSet<(int, int)> { (0, 0) }; var links = new List<(int from, int to)>(); int guard = 0, head = 0;
        while (path.Count < n && guard++ < 800)
        {
            var q = path[head]; int d = (int)(Rnd(seed + guard) * 4f); (int x, int y) nx = d switch { 0 => (q.x + 1, q.y), 1 => (q.x - 1, q.y), 2 => (q.x, q.y + 1), _ => (q.x, q.y - 1) };
            if (Math.Abs(nx.x) <= 5 && Math.Abs(nx.y) <= 3 && used.Add(nx)) { path.Add(nx); links.Add((head, path.Count - 1)); head = path.Count - 1; }
            else if (guard % 5 == 0) head = (int)(Rnd(seed + guard + 9) * path.Count);
        }
        float cx = (float)path.Average(q => q.x), cy = (float)path.Average(q => q.y);
        Vector2 Pos((int x, int y) q) => o + new Vector2((q.x - cx) * s * 1.35f, (q.y - cy) * s);
        foreach (var l in links) Ln(dc, b, Pos(path[l.from]), Pos(path[l.to]), Rgb(1, 1, 1, 0.45f * a), 10f * u);
        for (int i = 0; i < path.Count; i++)
        {
            bool seen = i <= now, here = i == (int)MathF.Round(now); float pop = DrawKit.EaseOutBack(Intro(c, i * 0.03f, 0.35f)); var q = Pos(path[i]);
            RBox(dc, b, q, 112f * u * pop, 76f * u * pop, 10f * u, here ? C3(a) : seen ? C2(a) : C1(a)); RFrame(dc, b, q, 112f * u * pop, 76f * u * pop, 10f * u, Rgb(1, 1, 1, 0.55f * a), 3f * u);
            if (here) Dot(dc, b, q, (12f + MathF.Sin(c.Time * 6f) * 3f) * u, Rgb(0.8f, 0.1f, 0.1f, a));
        }
    }
}

public sealed class SavePointPlugin : QuickShapePlugin { public override string Name => "演出：セーブポイント"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SavePointParameter(s); }
public sealed class SavePointParameter : Q3Param
{
    public SavePointParameter() : this(null) { }
    public SavePointParameter(SharedDataStore? s) : base(s) { Text = "セーブしました"; Color1 = Hex(0x4FE0FF); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("セーブポイント", "Text=文字", "Speed=回る速さ", "Color1=結晶の色", "Color2=光の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time * Spd(c), bob = MathF.Sin(t * 2f) * 18f * u, sx = MathF.Max(0.12f, MathF.Abs(MathF.Cos(t))); var o = P0(c) + new Vector2(0, bob - 30f * u);
        Oval(dc, b, P0(c) + new Vector2(0, 230f * u), 170f * u, 34f * u, C1(0.3f * a)); Dot(dc, b, o, 210f * u, C1(0.1f * a));
        var top = o - new Vector2(0, 170f * u); var bot = o + new Vector2(0, 170f * u); var l = o - new Vector2(100f * u * sx, 20f * u); var rr = o + new Vector2(100f * u * sx, -20f * u);
        Poly(dc, b, new List<Vector2> { top, rr, bot, l }, C1(0.8f * a)); Poly(dc, b, new List<Vector2> { top, o - new Vector2(0, 20f * u), bot, l }, Col(Gfx.Mix(Color1, Color2, 0.45f), 0.8f * a));
        for (int i = 0; i < 12; i++) { float k = Gfx.Mod(t * 0.28f + i / 12f, 1f), th = i * 2.4f + t; Dot(dc, b, P0(c) + new Vector2(MathF.Cos(th) * 170f * u, 220f * u - k * 430f * u), 5f * u, C2(a * (1f - k))); }
        Say(dc, b, Str, 54f * u, P0(c) + new Vector2(0, 320f * u), C2(a * Sub(c.Time, 0.5f, 0.35f)), Rgb(0, 0, 0, 0.65f * a * Sub(c.Time, 0.5f, 0.35f)), 4f * u);
    }
}

public sealed class TargetCursorPlugin : QuickShapePlugin { public override string Name => "演出：ターゲット選択の矢印"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TargetCursorParameter(s); }
public sealed class TargetCursorParameter : Q3Param
{
    public TargetCursorParameter() : this(null) { }
    public TargetCursorParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFD54A); Color2 = Hex(0xFF5252); }
    protected override double DefAmount => 30;
    static readonly Dictionary<string, ParamGuide> g = G("ターゲット選択", "Variant=矢印の形|1=三角　2=手の指　3=ひし形　4=照準|1|4", "Amount=揺れの大きさ", "Speed=揺れの速さ", "Color1=矢印の色", "Color2=ふちの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), wave = MathF.Sin(c.Time * Spd(c) * 6f) * Amt(c) * 100f * u; var q = P0(c) + new Vector2(0, wave);
        if (Variant == 4) { float r = 100f * u + wave * 0.25f; Ring(dc, b, q, r, C1(a), 7f * u); for (int i = 0; i < 4; i++) { float th = i * MathF.PI / 2f + c.Time; Ln(dc, b, Pol(q, r * 0.7f, th), Pol(q, r * 1.35f, th), C2(a), 7f * u); } return; }
        if (Variant == 3) { var d = new List<Vector2> { q - new Vector2(0, 62f * u), q + new Vector2(46f * u, 0), q + new Vector2(0, 62f * u), q - new Vector2(46f * u, 0) }; Poly(dc, b, d, C1(a)); Outline(dc, b, d, C2(a), 6f * u); return; }
        if (Variant == 2) { RBox(dc, b, q - new Vector2(0, 45f * u), 34f * u, 110f * u, 16f * u, C1(a)); RBox(dc, b, q + new Vector2(0, 42f * u), 106f * u, 100f * u, 28f * u, C1(a)); RFrame(dc, b, q + new Vector2(0, 42f * u), 106f * u, 100f * u, 28f * u, C2(a), 5f * u); return; }
        var tri = new List<Vector2> { q + new Vector2(0, 62f * u), q + new Vector2(58f * u, -44f * u), q + new Vector2(-58f * u, -44f * u) }; Poly(dc, b, tri, C1(a)); Outline(dc, b, tri, C2(a), 6f * u);
    }
}

public sealed class JustDodgePlugin : QuickShapePlugin { public override string Name => "演出：ジャスト回避（JUST!）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new JustDodgeParameter(s); }
public sealed class JustDodgeParameter : Q3Param
{
    public JustDodgeParameter() : this(null) { }
    public JustDodgeParameter(SharedDataStore? s) : base(s) { Text = "JUST!"; Color1 = Hex(0x1E88FF); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 40;
    static readonly Dictionary<string, ParamGuide> g = G("ジャスト回避", "Text=文字", "Amount=長さ|0〜100 = 0〜3秒", "Color1=画面の色", "Color2=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, life = MathF.Max(0.2f, Amt(c) * 3f), fade = Sub(t, 0f, 0.1f) * (1f - Sub(t, life, 0.3f)); var o = P0(c); if (fade <= 0f) return;
        Box(dc, b, 0, 0, c.Width, c.Height, C1(0.34f * a * fade));
        for (int i = 0; i < 26; i++) { float y = Rnd(i) * c.Height, x = Gfx.Mod(Rnd(i + 30) * c.Width - t * 90f * u, c.Width); Ln(dc, b, new(x, y), new(x + (100f + Rnd(i + 50) * 260f) * u, y), C2(0.35f * a * fade), 3f * u); }
        float ring = DrawKit.EaseOut(Sub(t, 0f, 0.45f)); Ring(dc, b, o, 520f * u * ring, C2(a * fade * (1f - ring)), 14f * u);
        Say(dc, b, Str, 170f * u, o, C2(a * fade), C1(a * fade), 9f * u, scale: DrawKit.EaseOutBack(Sub(t, 0f, 0.25f)));
    }
}

public sealed class RankResultPlugin : QuickShapePlugin { public override string Name => "演出：ランク判定（S・A・B）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RankResultParameter(s); }
public sealed class RankResultParameter : Q3Param
{
    public RankResultParameter() : this(null) { }
    public RankResultParameter(SharedDataStore? s) : base(s) { Text = "S\nタイム:A\nコンボ:S\nノーダメージ:S"; Color1 = Hex(0xFFC107); Color2 = Hex(0x12202F); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("ランク判定", "Text=ランクと評価|1行目=総合ランク。2行目から「項目:評価」", "Color1=ランクの色", "Color2=窓の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return; float u = U(c), t = c.Time; var o = P0(c);
        Panel(dc, b, o + new Vector2(-250f * u, 0), 720f * u, 520f * u, 18f * u, C2(0.92f * a), C3(0.5f * a), 3f * u);
        for (int i = 1; i < r.Count && i <= 5; i++)
        {
            var f = Cells(r[i]); float p = DrawKit.EaseOut(Sub(t, 0.1f + i * 0.28f, 0.3f)), y = o.Y - 195f * u + (i - 1) * 96f * u;
            SayAt(dc, b, f[0], 42f * u, new(o.X - 560f * u + (1f - p) * 40f * u, y - 27f * u), C3(a * p), maxW: 430f * u); Say(dc, b, f.Length > 1 ? f[1] : "", 58f * u, new(o.X + 20f * u, y), C1(a * p), scale: 1.4f - 0.4f * p);
        }
        float at = 0.25f + MathF.Min(5, r.Count - 1) * 0.28f, k = Sub(t, at, 0.2f); if (k <= 0f) return;
        var q = o + new Vector2(430f * u, 0); float sc = 2.2f - 1.2f * DrawKit.EaseOut(k), shake = (1f - Sub(t, at + 0.2f, 0.25f)) * MathF.Sin(t * 80f) * 10f * u; Ring(dc, b, q, 215f * u * sc, C1(a * k), 12f * u);
        Say(dc, b, r[0], 310f * u, q + new Vector2(shake, 0), C1(a * k), Rgb(0, 0, 0, a * k), 10f * u, scale: sc);
    }
}

public sealed class LoadingTipsPlugin : QuickShapePlugin { public override string Name => "演出：ロード中のTips枠"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LoadingTipsParameter(s); }
public sealed class LoadingTipsParameter : Q3Param
{
    public LoadingTipsParameter() : this(null) { }
    public LoadingTipsParameter(SharedDataStore? s) : base(s) { Text = "セーブはこまめにしましょう\n弱点を突くとダメージが増えます\nボスの前には回復を忘れずに"; Color1 = Hex(0x111A26); Color2 = Hex(0x4FC3F7); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount => 30;
    static readonly Dictionary<string, ParamGuide> g = G("ロード中のTips", "Text=文|1行に1つ", "Amount=切り替えの間隔|0〜100 = 1〜11秒", "Color1=枠の色", "Color2=見出しの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return; float u = U(c), iv = 1f + Amt(c) * 10f, ph = c.Time / iv; int i = (int)ph % r.Count; float f = ph - MathF.Floor(ph), al = MathF.Min(Sub(f, 0f, 0.12f), 1f - Sub(f, 0.88f, 0.12f)); var o = P0(c) + new Vector2(0, 360f * u);
        Panel(dc, b, o, 1320f * u, 190f * u, 16f * u, C1(0.9f * a), C2(0.7f * a), 3f * u); RBox(dc, b, o + new Vector2(-540f * u, -48f * u), 150f * u, 52f * u, 10f * u, C2(a)); Say(dc, b, "TIPS", 34f * u, o + new Vector2(-540f * u, -48f * u), Rgb(0.05f, 0.08f, 0.12f, a));
        Say(dc, b, r[i], 44f * u, o + new Vector2(0, 26f * u), C3(a * al), maxW: 1220f * u);
        for (int k = 0; k < 3; k++) Dot(dc, b, o + new Vector2(590f * u + k * 24f * u, -60f * u), 7f * u, C2(a * (0.3f + 0.7f * MathF.Max(0f, MathF.Sin(c.Time * 5f - k)))));
    }
}

public sealed class RhythmNotesPlugin : QuickShapePlugin { public override string Name => "演出：リズムゲームのノーツ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RhythmNotesParameter(s); }
public sealed class RhythmNotesParameter : Q3Param
{
    public RhythmNotesParameter() : this(null) { }
    public RhythmNotesParameter(SharedDataStore? s) : base(s) { Count = 4; Color1 = Hex(0x18202E); Color2 = Hex(0xFF4F9A); Color3 = Hex(0x54E8FF); }
    protected override double DefAmount => 60;
    static readonly Dictionary<string, ParamGuide> g = G("リズムゲーム", "Count=レーン数||2|8", "Amount=テンポ|0〜100 = BPM 60〜240", "Seed=譜面のシード", "Color1=レーンの色", "Color2=ノーツの色", "Color3=判定線の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 2, 8); float u = U(c), beat = c.Time * (60f + Amt(c) * 180f) / 60f, lw = 150f * u, H = 880f * u, view = 4f; var o = P0(c); float x0 = o.X - n * lw / 2f, y0 = o.Y - H / 2f, hit = y0 + H * 0.86f;
        Box(dc, b, x0, y0, x0 + n * lw, y0 + H, C1(0.9f * a)); for (int i = 0; i <= n; i++) Ln(dc, b, new(x0 + i * lw, y0), new(x0 + i * lw, y0 + H), Rgb(1, 1, 1, 0.22f * a), 2f * u);
        Ln(dc, b, new(x0, hit), new(x0 + n * lw, hit), C3(a), 8f * u); float flash = 0f; int bNow = (int)MathF.Floor(beat);
        for (int k = bNow - 1; k <= bNow + (int)view + 1; k++)
        {
            if (k < 0 || Rnd(k * 13 + Seed) < 0.2f) continue; int lane = (int)(Rnd(k * 7 + Seed + 3) * n); float d = k - beat, y = hit - d / view * (hit - y0);
            if (d >= 0f) { if (y > y0 + 19f * u) RBox(dc, b, new(x0 + (lane + 0.5f) * lw, y), lw - 18f * u, 38f * u, 9f * u, C2(a)); }
            else if (d > -0.45f) { float e = -d / 0.45f; flash = MathF.Max(flash, 1f - e); Ring(dc, b, new(x0 + (lane + 0.5f) * lw, hit), (30f + e * 70f) * u, C3(a * (1f - e)), 6f * u); }
        }
        if (flash > 0f) Say(dc, b, "PERFECT", 72f * u, new(o.X, hit - 170f * u), C3(a * flash), Rgb(0, 0, 0, 0.7f * a * flash), 5f * u, scale: 1f + (1f - flash) * 0.2f);
    }
}

public sealed class FighterHpBarsPlugin : QuickShapePlugin { public override string Name => "演出：格闘ゲームの体力バー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FighterHpBarsParameter(s); }
public sealed class FighterHpBarsParameter : Q3Param
{
    public FighterHpBarsParameter() : this(null) { }
    public FighterHpBarsParameter(SharedDataStore? s) : base(s) { Text = "霊夢\n魔理沙\nROUND 1"; Color1 = Hex(0xFFD54A); Color2 = Hex(0xFF5252); Color3 = Hex(0xFFFFFF); }
    protected override double DefAmount => 80;
    protected override double DefAmount2 => 55;
    static readonly Dictionary<string, ParamGuide> g = G("格闘ゲームの体力バー", "Text=名前とラウンド|1行目=左　2行目=右　3行目=ラウンド", "Amount=左の体力", "Amount2=右の体力", "Color1=体力の色", "Color2=減った部分の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), t = c.Time, W = 760f * u, y = 92f * u, cx = P0(c).X, hl = Amt(c), hr = Amt2(c);
        foreach (bool right in new[] { false, true })
        {
            float dir = right ? 1f : -1f, near = cx + dir * 75f * u, far = near + dir * W, hp = right ? hr : hl;
            Box(dc, b, near, y - 28f * u, far, y + 28f * u, Rgb(0.08f, 0.08f, 0.1f, 0.9f * a)); Box(dc, b, far - dir * W * MathF.Min(1f, hp + 0.08f), y - 22f * u, far, y + 22f * u, C2(a)); Box(dc, b, far - dir * W * hp, y - 22f * u, far, y + 22f * u, C1(a)); Frame(dc, b, MathF.Min(near, far), y - 28f * u, MathF.Max(near, far), y + 28f * u, C3(a), 4f * u);
            Say(dc, b, r.Count > (right ? 1 : 0) ? r[right ? 1 : 0] : "", 40f * u, new(far - dir * 130f * u, y + 62f * u), C3(a), Rgb(0, 0, 0, 0.75f * a), 4f * u);
        }
        Dot(dc, b, new(cx, y), 62f * u, Rgb(0.08f, 0.08f, 0.1f, a)); Ring(dc, b, new(cx, y), 62f * u, C1(a), 5f * u); Say(dc, b, Math.Max(0, 99 - (int)t).ToString(), 52f * u, new(cx, y), C3(a));
        float k1 = Sub(t, 0.15f, 0.25f) * (1f - Sub(t, 1.0f, 0.2f)), k2 = Sub(t, 1.2f, 0.18f) * (1f - Sub(t, 1.9f, 0.25f)); var mid = new Vector2(cx, c.Height * 0.48f);
        if (k1 > 0f) Say(dc, b, r.Count > 2 ? r[2] : "ROUND 1", 130f * u, mid, C3(a * k1), Rgb(0, 0, 0, 0.8f * a * k1), 7f * u);
        if (k2 > 0f) Say(dc, b, "FIGHT!", 210f * u, mid, C2(a * k2), C3(a * k2), 8f * u, scale: 1.5f - 0.5f * DrawKit.EaseOut(Sub(t, 1.2f, 0.2f)));
    }
}

public sealed class MashQtePlugin : QuickShapePlugin { public override string Name => "演出：ボタン連打のQTE"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MashQteParameter(s); }
public sealed class MashQteParameter : Q3Param
{
    public MashQteParameter() : this(null) { }
    public MashQteParameter(SharedDataStore? s) : base(s) { Text = "◯"; Color1 = Hex(0xFF5252); Color2 = Hex(0xFFFFFF); Color3 = Hex(0x66FF8A); }
    protected override double DefAmount => 40;
    static readonly Dictionary<string, ParamGuide> g = G("ボタン連打のQTE", "Text=ボタンの文字", "Variant=結果|1=成功　2=失敗|1|2", "Amount=長さ|0〜100 = 0.5〜5.5秒", "Color1=ボタンの色", "Color2=文字の色", "Color3=成功の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), t = c.Time, dur = 0.5f + Amt(c) * 5f, k = Sub(t, 0f, dur); var o = P0(c); bool ok = Variant != 2;
        if (k < 1f)
        {
            float tap = MathF.Abs(MathF.Sin(t * 16f)), rr = 120f * u * (1f - tap * 0.1f); Ring(dc, b, o, Lerp(330f, 130f, k) * u, C2(0.85f * a), 10f * u); Dot(dc, b, o + new Vector2(0, 10f * u), 120f * u, Rgb(0.25f, 0.05f, 0.06f, a)); Dot(dc, b, o + new Vector2(0, tap * 8f * u), rr, C1(a));
            Say(dc, b, Str, 120f * u, o + new Vector2(0, tap * 8f * u), C2(a)); Say(dc, b, Str + " ボタンを連打！", 62f * u, o + new Vector2(0, 390f * u), C2(a), Rgb(0, 0, 0, 0.75f * a), 4f * u); return;
        }
        float p = DrawKit.EaseOutBack(Sub(t, dur, 0.28f)); Say(dc, b, ok ? "SUCCESS!" : "FAILED…", 150f * u, o, ok ? C3(a) : C1(a), Rgb(0, 0, 0, 0.8f * a), 7f * u, scale: p);
    }
}

public sealed class TargetGamePlugin : QuickShapePlugin { public override string Name => "演出：的あてミニゲーム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TargetGameParameter(s); }
public sealed class TargetGameParameter : Q3Param
{
    public TargetGameParameter() : this(null) { }
    public TargetGameParameter(SharedDataStore? s) : base(s) { Text = "100"; Count = 5; Color1 = Hex(0xEF5350); Color2 = Hex(0xFFFFFF); Color3 = Hex(0xFFD54A); }
    static readonly Dictionary<string, ParamGuide> g = G("的あて", "Text=1回の得点", "Count=的の数||1|12", "Speed=速さ", "Seed=シード", "Color1=的の色", "Color2=的の白", "Color3=得点の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 1, 12), hits = 0; float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)), pts = Num(Str, 100f), cycle = 1.5f; var o = P0(c);
        for (int i = 0; i < n; i++)
        {
            int col = i % 4, row = i / 4; var q = o + new Vector2((col - 1.5f) * 340f * u, (row - (n - 1) / 8f) * 300f * u + 80f * u); float off = Rnd(i + Seed) * cycle, ph = (t + off) / cycle; int round = (int)MathF.Floor(ph); float f = ph - round;
            for (int k = 0; k < round; k++) if (Rnd(k * 17 + i + Seed) > 0.35f) hits++;
            bool hit = Rnd(round * 17 + i + Seed) > 0.35f; if (hit && f > 0.55f) hits++;
            float up = f < 0.2f ? DrawKit.EaseOutBack(f / 0.2f) : f < 0.8f ? 1f : 1f - (f - 0.8f) / 0.2f; if (hit && f > 0.55f) up = MathF.Min(up, 1f - Sub(f, 0.55f, 0.12f));
            Ln(dc, b, q + new Vector2(0, 120f * u), q + new Vector2(0, 120f * u - 120f * u * up), Rgb(0.55f, 0.4f, 0.28f, a), 12f * u);
            for (int e = 0; e < 3; e++) Oval(dc, b, q + new Vector2(0, 120f * u - 120f * u * up), (105f - e * 34f) * u * up, (105f - e * 34f) * u * up, e % 2 == 0 ? C1(a) : C2(a));
            if (hit && f is > 0.55f and < 0.95f) { float e = Sub(f, 0.55f, 0.4f); Say(dc, b, "+" + pts.ToString("0"), 52f * u, q - new Vector2(0, 40f * u + e * 90f * u), C3(a * (1f - e)), Rgb(0, 0, 0, 0.7f * a * (1f - e)), 4f * u); }
        }
        var bar = new Vector2(o.X, o.Y - 400f * u); Panel(dc, b, bar, 420f * u, 92f * u, 18f * u, Rgb(0.05f, 0.08f, 0.12f, 0.9f * a), C3(a), 4f * u); Say(dc, b, "SCORE " + (hits * pts).ToString("#,0"), 46f * u, bar, C3(a));
    }
}
