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
//  地球儀
//   台にのった地球儀がくるくる回る。大陸が横に流れて見える
// =====================================================================
public sealed class GlobeSpinPlugin : QuickShapePlugin
{
    public override string Name => "演出：地球儀";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GlobeSpinParameter(sharedData);
}

public sealed class GlobeSpinParameter : QuickShapeParameter
{
    public GlobeSpinParameter() : this(null) { }
    public GlobeSpinParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x1E88E5);
        Color2 = Hex(0x66BB6A);
        Color3 = Hex(0xC9A227);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "海の色"),
        ["Color2"] = new ParamGuide(Label: "陸の色"),
        ["Color3"] = new ParamGuide(Label: "台の色"),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = 260f * u, rot = t * 0.5f * Spd(c);
        Poly(dc, b, Pts(o + new Vector2(0, R + 120f * u), u, u, -160f, 30f, 160f, 30f, 110f, -10f, -110f, -10f), C3(a));
        Box(dc, b, o.X - 14f * u, o.Y + R + 20f * u, o.X + 14f * u, o.Y + R + 110f * u, C3(a));
        Path(dc, b, Gfx.ArcPts(o, R + 30f * u, MathF.PI * 0.35f, MathF.PI * 1.35f, 40), C3(a), 14f * u);
        Dot(dc, b, o, R, C1(a));
        float[] lon = { 0f, 0.9f, 1.7f, 2.6f, 3.6f, 4.6f, 5.4f };
        float[] lat = { -0.4f, 0.2f, -0.1f, 0.5f, -0.6f, 0.1f, 0.35f };
        float[] sz = { 0.5f, 0.35f, 0.6f, 0.3f, 0.45f, 0.4f, 0.25f };
        for (int i = 0; i < lon.Length; i++)
        {
            float lo = lon[i] + rot;
            float z = MathF.Cos(lo);
            if (z <= 0.05f) continue;
            var q = o + new Vector2(MathF.Sin(lo) * R * MathF.Cos(lat[i]), MathF.Sin(lat[i]) * R);
            Poly(dc, b, BubbleKit.CloudPts(q, R * sz[i] * 0.5f * z, R * sz[i] * 0.4f, 6, 0.25f), C2(a));
        }
        for (int i = -2; i <= 2; i++) { float y = i * R * 0.33f; float hw = MathF.Sqrt(R * R - y * y); Oval(dc, b, o + new Vector2(0, y), hw, 0.1f + 2f * u, Rgb(1, 1, 1, 0.25f * a)); }
        using (var g = Gfx.Radial(dc, o - new Vector2(R, R) * 0.35f, R * 1.6f, (0f, Rgb(1, 1, 1, 0.25f * a)), (0.5f, Rgb(1, 1, 1, 0f)), (1f, Rgb(0, 0, 0, 0.35f * a))))
            dc.FillEllipse(new Ellipse(o, R, R), g);
    }
}

// =====================================================================
//  歯車（かみ合って回る）
//   大小の歯車がかみ合って回る。しくみ・仕事の場面に。「数」で歯車の数
// =====================================================================
public sealed class GearSetPlugin : QuickShapePlugin
{
    public override string Name => "演出：歯車（かみ合って回る）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GearSetParameter(sharedData);
}

public sealed class GearSetParameter : QuickShapeParameter
{
    public GearSetParameter() : this(null) { }
    public GearSetParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 3;
        Color1 = Hex(0x90A4AE);
        Color2 = Hex(0xFFB300);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "歯車の数", Min: 1, Max: 6, Group: "「歯車（かみ合って回る）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "大きい歯車の色"),
        ["Color2"] = new ParamGuide(Label: "小さい歯車の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Clamp(Count, 1, 6);
        var q = o - new Vector2((n - 1) * 140f * u, 0);
        float ang = t * Spd(c);
        float prevR = 0f;
        for (int i = 0; i < n; i++)
        {
            float R = (i % 2 == 0 ? 170f : 110f) * u;
            int teeth = (int)(R / u / 14f);
            if (i > 0) q += new Vector2((prevR + R) * 0.92f, (i % 2 == 0 ? -1f : 1f) * 40f * u);
            float rot = (i % 2 == 0 ? 1f : -1f) * ang * (170f * u / R) + (i % 2 == 1 ? MathF.PI / teeth : 0f);
            var g = new List<Vector2>();
            for (int k = 0; k < teeth * 4; k++) g.Add(Pol(q, k % 4 < 2 ? R : R * 0.84f, rot + k * MathF.PI * 2f / (teeth * 4)));
            var col = i % 2 == 0 ? Color1 : Color2;
            Poly(dc, b, g, Col(col, a));
            Dot(dc, b, q, R * 0.55f, Col(Gfx.Mix(col, Hex(0x000000), 0.15f), a));
            Dot(dc, b, q, R * 0.18f, Rgb(0.2f, 0.2f, 0.22f, a));
            for (int k = 0; k < 5; k++) Dot(dc, b, Pol(q, R * 0.37f, rot + k * MathF.PI * 2f / 5f), R * 0.09f, Col(Gfx.Mix(col, Hex(0x000000), 0.3f), a));
            prevR = R;
        }
    }
}

// =====================================================================
//  通知バッジ（数字の赤丸）
//   アイコンの角につく赤い丸と数字。ポンと出て数が増える。「数」が最後の数
// =====================================================================
public sealed class NotifyBadgePlugin : QuickShapePlugin
{
    public override string Name => "演出：通知バッジ（数字の赤丸）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NotifyBadgeParameter(sharedData);
}

public sealed class NotifyBadgeParameter : QuickShapeParameter
{
    public NotifyBadgeParameter() : this(null) { }
    public NotifyBadgeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 3;
        Color1 = Hex(0xFF3B30);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "最後の数", Min: 1, Max: 999, Group: "「通知バッジ（数字の赤丸）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "バッジの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float p = Intro(c, 0.1f, 0.2f + 0.25f * Math.Max(1, Count));
        int n = Math.Max(1, (int)MathF.Ceiling(p * Count));
        float step = Gfx.Mod(p * Count, 1f);
        float k = DrawKit.EaseOutBack(Math.Clamp(c.Time / 0.3f, 0f, 1f)) * (1f + 0.25f * MathF.Max(0f, 1f - step * 4f) * (p < 1f ? 1f : 0f));
        string s = n > 99 ? "99+" : n.ToString();
        var ts = Measure(s, 70f * u);
        float bw = MathF.Max(120f * u, ts.Width + 60f * u);
        RBox(dc, b, o, bw * k, 120f * u * k, 60f * u * k, C1(a));
        RFrame(dc, b, o, bw * k, 120f * u * k, 60f * u * k, Rgb(1, 1, 1, a), 8f * u);
        Say(dc, b, s, 70f * u, o, Rgb(1, 1, 1, a), scale: k);
    }
}

// =====================================================================
//  時限爆弾
//   導火線に火がついた丸い爆弾。「数」秒から数字が減り、0でドカン
// =====================================================================
public sealed class TimeBombPlugin : QuickShapePlugin
{
    public override string Name => "演出：時限爆弾";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TimeBombParameter(sharedData);
}

public sealed class TimeBombParameter : QuickShapeParameter
{
    public TimeBombParameter() : this(null) { }
    public TimeBombParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 5;
        Color1 = Hex(0x212121);
        Color2 = Hex(0xFF6D00);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "爆発までの秒数", Min: 1, Max: 60, Group: "「時限爆弾」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "爆弾の色"),
        ["Color2"] = new ParamGuide(Label: "火と爆発の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float left = Count - c.Time;
        if (left <= 0f)
        {
            float f = Math.Clamp(-left / 0.8f, 0f, 1f);
            DrawKit.Glow(dc, o, (300f + 900f * f) * u, C2((1f - f) * a));
            Poly(dc, b, BubbleKit.SpikyPts(o, (200f + 500f * f) * u, (180f + 450f * f) * u, 14, 0.4f, Seed), Rgb(1f, 0.9f, 0.3f, (1f - f) * a));
            Say(dc, b, "BOOM!", 180f * u, o, Rgb(1, 1, 1, (1f - f) * a), C2((1f - f) * a), 10f * u, scale: 1f + f);
            return;
        }
        float shake = left < 1.5f ? MathF.Sin(t * 60f) * 6f * u : 0f;
        var q = o + new Vector2(shake, 0);
        float R = 200f * u * (1f + (left < 1.5f ? 0.03f * MathF.Sin(t * 30f) : 0f));
        Dot(dc, b, q, R, C1(a));
        Dot(dc, b, q + new Vector2(-R * 0.4f, -R * 0.4f), R * 0.2f, Rgb(1, 1, 1, 0.3f * a));
        Box(dc, b, q.X - 50f * u, q.Y - R - 40f * u, q.X + 50f * u, q.Y - R + 20f * u, Rgb(0.4f, 0.4f, 0.42f, a));
        float burn = Math.Clamp(c.Time / MathF.Max(0.1f, Count), 0f, 1f);
        var fuse = BubbleKit.Quad(q + new Vector2(0, -R - 40f * u), q + new Vector2(60f * u, -R - 200f * u), q + new Vector2(200f * u, -R - 150f * u), 30);
        var rest = BubbleKit.PartByLength(fuse, 1f - burn * 0.9f);
        Path(dc, b, rest, Rgb(0.8f, 0.7f, 0.5f, a), 10f * u);
        var sp = rest[^1];
        DrawKit.Glow(dc, sp, 60f * u, C2(0.8f * a));
        for (int i = 0; i < 6; i++) Ln(dc, b, sp, Pol(sp, (20f + 30f * DrawKit.H(Seed + (int)(t * 20f), i)) * u, i * MathF.PI / 3f + t * 10f), Rgb(1f, 0.9f, 0.3f, a), 3f * u);
        Say(dc, b, MathF.Ceiling(left).ToString("0"), 140f * u, q + new Vector2(0, 10f * u), Rgb(1f, 0.25f, 0.2f, a));
    }
}

// =====================================================================
//  信号機
//   青→黄→赤と光る信号機。「値」で今の色を固定(0=自動、1〜33青、34〜66黄、67〜赤)。バリエーション2は歩行者用
// =====================================================================
public sealed class TrafficSignalPlugin : QuickShapePlugin
{
    public override string Name => "演出：信号機";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TrafficSignalParameter(sharedData);
}

public sealed class TrafficSignalParameter : QuickShapeParameter
{
    public TrafficSignalParameter() : this(null) { }
    public TrafficSignalParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x37474F);
    }
    protected override double DefAmount => 0;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "光る色", Tip: "0=自動で変わる 1〜33=青 34〜66=黄 67〜100=赤", Group: "「信号機」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=車用　2=歩行者用(青か赤)", Min: 1, Max: 2, Group: "「信号機」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "本体の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float v = Amt(c);
        int state = v <= 0.001f ? (int)(Gfx.Mod(t * 0.3f * Spd(c), 1f) * 3f) : v < 0.34f ? 0 : v < 0.67f ? 1 : 2;
        if (Variant >= 2)
        {
            RBox(dc, b, o, 220f * u, 440f * u, 20f * u, C1(a));
            bool go = state == 0;
            var tp = o + new Vector2(0, -105f * u); var bp = o + new Vector2(0, 105f * u);
            Box(dc, b, o.X - 90f * u, tp.Y - 90f * u, o.X + 90f * u, tp.Y + 90f * u, go ? Rgb(0.1f, 0.1f, 0.1f, a) : Rgb(1f, 0.2f, 0.15f, a));
            Box(dc, b, o.X - 90f * u, bp.Y - 90f * u, o.X + 90f * u, bp.Y + 90f * u, go ? Rgb(0.1f, 0.9f, 0.6f, a) : Rgb(0.1f, 0.1f, 0.1f, a));
            var pp = go ? bp : tp;
            Dot(dc, b, pp + new Vector2(0, -50f * u), 16f * u, Rgb(1, 1, 1, 0.9f * a));
            Poly(dc, b, Pts(pp, u, u, -22f, -30f, 22f, -30f, 26f, 20f, go ? 40f : 16f, 60f, go ? 20f : 10f, 60f, 4f, 24f, go ? -20f : -10f, 60f, go ? -40f : -16f, 60f, -26f, 20f), Rgb(1, 1, 1, 0.9f * a));
            return;
        }
        RBox(dc, b, o, 640f * u, 240f * u, 40f * u, C1(a));
        Box(dc, b, o.X - 12f * u, o.Y + 120f * u, o.X + 12f * u, o.Y + 500f * u, C1(a));
        var cols = new[] { Rgb(0.1f, 0.85f, 0.55f, a), Rgb(1f, 0.8f, 0.1f, a), Rgb(1f, 0.2f, 0.15f, a) };
        for (int i = 0; i < 3; i++)
        {
            var q = o + new Vector2((i - 1) * 200f * u, 0);
            Dot(dc, b, q, 80f * u, Rgb(0.08f, 0.08f, 0.1f, a));
            if (i == state) { DrawKit.Glow(dc, q, 190f * u, new Color4(cols[i].R, cols[i].G, cols[i].B, 0.5f * a)); Dot(dc, b, q, 70f * u, cols[i]); }
            else Dot(dc, b, q, 70f * u, new Color4(cols[i].R * 0.25f, cols[i].G * 0.25f, cols[i].B * 0.25f, a));
        }
    }
}

// =====================================================================
//  封筒と手紙
//   封筒のふたが開いて、中から手紙がスーッと出てくる。手紙に「文字」
// =====================================================================
public sealed class EnvelopeLetterPlugin : QuickShapePlugin
{
    public override string Name => "演出：封筒と手紙";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new EnvelopeLetterParameter(sharedData);
}

public sealed class EnvelopeLetterParameter : QuickShapeParameter
{
    public EnvelopeLetterParameter() : this(null) { }
    public EnvelopeLetterParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ありがとう";
        Color1 = Hex(0xFFE0B2);
        Color2 = Hex(0xE53935);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "手紙の文字", Group: "「封筒と手紙」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "封筒の色"),
        ["Color2"] = new ParamGuide(Label: "シールの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float ew = 600f * u, eh = 380f * u;
        float open = DrawKit.EaseOut(Intro(c, 0.3f, 0.5f));
        float rise = DrawKit.EaseOut(Intro(c, 0.8f, 0.7f));
        var tl = o - new Vector2(ew, eh) / 2f;
        var dark = Col(Gfx.Mix(Color1, Hex(0x8D6E63), 0.3f), a);
        Box(dc, b, tl.X, tl.Y, tl.X + ew, tl.Y + eh, dark);
        var flapTip = new Vector2(o.X, tl.Y + eh * 0.55f - open * eh * 1.1f);
        if (open > 0.5f) Poly(dc, b, new List<Vector2> { tl, new(tl.X + ew, tl.Y), flapTip }, dark);
        float ly = tl.Y + 30f * u - rise * 300f * u;
        Box(dc, b, tl.X + 40f * u, ly, tl.X + ew - 40f * u, ly + eh - 40f * u, Rgb(1, 1, 1, a));
        Say(dc, b, Str, 60f * u, new Vector2(o.X, ly + 120f * u), Rgb(0.2f, 0.2f, 0.25f, a), bold: false, maxW: ew - 120f * u);
        Poly(dc, b, new List<Vector2> { new(tl.X, tl.Y + eh * 0.35f), new(o.X, tl.Y + eh * 0.7f), new(tl.X + ew, tl.Y + eh * 0.35f), new(tl.X + ew, tl.Y + eh), new(tl.X, tl.Y + eh) }, C1(a));
        if (open <= 0.5f) Poly(dc, b, new List<Vector2> { tl, new(tl.X + ew, tl.Y), flapTip }, Col(Gfx.Mix(Color1, Hex(0xFFFFFF), 0.2f), a));
        if (open < 0.3f) Heart(dc, b, new Vector2(o.X, tl.Y + eh * 0.55f), 40f * u, 0f, C2(a));
    }
}

// =====================================================================
//  表彰台
//   1・2・3位の表彰台が下からせり上がる。「文字」にカンマ区切りで1位,2位,3位の名前
// =====================================================================
public sealed class PodiumPlugin : QuickShapePlugin
{
    public override string Name => "演出：表彰台";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PodiumParameter(sharedData);
}

public sealed class PodiumParameter : QuickShapeParameter
{
    public PodiumParameter() : this(null) { }
    public PodiumParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "Aさん,Bさん,Cさん";
        Color1 = Hex(0xFFD54A);
        Color2 = Hex(0xCFD8DC);
        Color3 = Hex(0xD7A86E);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "名前", Tip: "カンマ区切りで「1位,2位,3位」", Group: "「表彰台」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "1位の色"),
        ["Color2"] = new ParamGuide(Label: "2位の色"),
        ["Color3"] = new ParamGuide(Label: "3位の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        float bw = 300f * u;
        int[] order = { 1, 0, 2 };
        float[] hs = { 380f, 260f, 190f };
        var cols = new[] { Color1, Color2, Color3 };
        float baseY = o.Y + 300f * u;
        for (int j = 0; j < 3; j++)
        {
            int rank = order[j];
            float p = DrawKit.EaseOutBack(Intro(c, 0.2f + (2 - rank) * 0.3f, 0.5f));
            float x = o.X + (j - 1) * bw;
            float hh = hs[rank] * u * p;
            Box(dc, b, x - bw / 2f, baseY - hh, x + bw / 2f, baseY, Col(cols[rank], a));
            Box(dc, b, x - bw / 2f, baseY - hh, x + bw / 2f, baseY - hh + 20f * u, Rgb(1, 1, 1, 0.3f * a));
            Say(dc, b, (rank + 1).ToString(), 120f * u, new Vector2(x, baseY - hh / 2f), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.3f * a), 4f * u);
            if (p >= 0.95f && rank < it.Count) Say(dc, b, it[rank], 50f * u, new Vector2(x, baseY - hh - 60f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.6f * a), 4f * u);
            if (rank == 0 && p >= 0.95f) Star(dc, b, new Vector2(x, baseY - hh - 160f * u), 50f * u * (1f + 0.1f * MathF.Sin(t * 5f)), 5, 0.45f, 0f, C1(a));
        }
    }
}

// =====================================================================
//  ペンでサイン
//   ペン先が動いて、「文字」が手書きのサインのように書かれていく。下に線
// =====================================================================
public sealed class PenSignaturePlugin : QuickShapePlugin
{
    public override string Name => "演出：ペンでサイン";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PenSignatureParameter(sharedData);
}

public sealed class PenSignatureParameter : QuickShapeParameter
{
    public PenSignatureParameter() : this(null) { }
    public PenSignatureParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "Yukkuri";
        Color1 = Hex(0x1A237E);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "サインの文字", Group: "「ペンでサイン」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "インクの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var ts = Measure(Str, 160f * u, false);
        float p = Intro(c, 0.2f, 0.6f + Str.Length * 0.12f);
        var tl = o - new Vector2(ts.Width, ts.Height) / 2f;
        float cx = tl.X + ts.Width * p;
        var prev = dc.Transform;
        dc.Transform = Matrix3x2.CreateSkew(-0.25f, 0f, o) * prev;
        dc.PushAxisAlignedClip(new RawRectF(tl.X - 40f * u, tl.Y - 100f * u, cx + 2f, tl.Y + ts.Height + 100f * u), AntialiasMode.PerPrimitive);
        TextKit.Draw(dc, b, ts, tl, C1(a));
        dc.PopAxisAlignedClip();
        dc.Transform = prev;
        float q = Intro(c, 0.8f + Str.Length * 0.12f, 0.4f);
        if (q > 0f) Path(dc, b, BubbleKit.PartByLength(BubbleKit.Quad(new Vector2(tl.X, tl.Y + ts.Height + 20f * u), new Vector2(o.X, tl.Y + ts.Height + 50f * u), new Vector2(tl.X + ts.Width + 60f * u, tl.Y + ts.Height), 30), q), C1(a), 6f * u);
        var tip = p < 1f ? new Vector2(cx, o.Y + MathF.Sin(t * 25f) * ts.Height * 0.3f) : (q > 0f ? new Vector2(tl.X + (ts.Width + 60f * u) * q, tl.Y + ts.Height + 30f * u) : new Vector2(cx, o.Y));
        var pen = DrawKit.Place(Pts(Vector2.Zero, u, u, 0f, 0f, 20f, -40f, 260f, -300f, 230f, -330f, -10f, -60f), tip, 0f);
        Poly(dc, b, pen, Rgb(0.15f, 0.15f, 0.18f, a));
        Poly(dc, b, DrawKit.Place(Pts(Vector2.Zero, u, u, 0f, 0f, 20f, -40f, 40f, -60f, -10f, -60f), tip, 0f), Rgb(0.8f, 0.7f, 0.3f, a));
    }
}

// =====================================================================
//  鍵が開く（南京錠）
//   南京錠に鍵がささって回り、カチャッとツルが開く。キラッと光る
// =====================================================================
public sealed class LockOpenPlugin : QuickShapePlugin
{
    public override string Name => "演出：鍵が開く（南京錠）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LockOpenParameter(sharedData);
}

public sealed class LockOpenParameter : QuickShapeParameter
{
    public LockOpenParameter() : this(null) { }
    public LockOpenParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFC107);
        Color2 = Hex(0x9E9E9E);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "錠の色"),
        ["Color2"] = new ParamGuide(Label: "ツルと鍵の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float ins = DrawKit.EaseOut(Intro(c, 0f, 0.4f));
        float turn = DrawKit.EaseOut(Intro(c, 0.5f, 0.3f));
        float open = DrawKit.EaseOutBack(Intro(c, 0.85f, 0.3f));
        var body = o + new Vector2(0, 80f * u);
        var shackleBase = body + new Vector2(0, -150f * u - open * 70f * u);
        var sh = Gfx.ArcPts(shackleBase + new Vector2(0, -60f * u), 110f * u, MathF.PI, MathF.PI * 2f, 30);
        var full = new List<Vector2> { shackleBase + new Vector2(-110f * u, 80f * u) };
        full.AddRange(sh);
        full.Add(shackleBase + new Vector2(110f * u, open > 0.01f ? -40f * u : 80f * u));
        Path(dc, b, DrawKit.Place(full.Select(p => p - shackleBase), shackleBase, -open * 0.3f), C2(a), 40f * u);
        RBox(dc, b, body, 340f * u, 300f * u, 40f * u, C1(a));
        RBox(dc, b, body + new Vector2(0, -110f * u), 340f * u, 40f * u, 20f * u, Rgb(1, 1, 1, 0.25f * a));
        Dot(dc, b, body + new Vector2(0, -10f * u), 30f * u, Rgb(0.3f, 0.2f, 0.05f, a));
        Box(dc, b, body.X - 10f * u, body.Y - 10f * u, body.X + 10f * u, body.Y + 60f * u, Rgb(0.3f, 0.2f, 0.05f, a));
        var kp = body + new Vector2(0, -10f * u) + new Vector2(0, 300f * u * (1f - ins));
        var key = DrawKit.Place(Pts(Vector2.Zero, u, u, -10f, 0f, 10f, 0f, 10f, 160f, 60f, 200f, 0f, 260f, -60f, 200f, -10f, 160f), kp, turn * MathF.PI / 2f * 0f);
        float kw = MathF.Max(0.15f, MathF.Abs(MathF.Cos(turn * MathF.PI / 2f)));
        var key2 = key.Select(p => new Vector2(kp.X + (p.X - kp.X) * kw, p.Y)).ToList();
        Poly(dc, b, key2, C2(a));
        if (open > 0.5f) DrawKit.Twinkle(dc, b, body + new Vector2(150f, -150f) * u, 80f * u * MathF.Sin(Math.Clamp((c.Time - 1f) * 3f, 0f, 1f) * MathF.PI), 6f * u, System.Windows.Media.Colors.White, a);
    }
}

// =====================================================================
//  プレゼント箱
//   リボンのかかった箱がガタガタゆれて、ふたがポンと飛び、中から光と紙吹雪
// =====================================================================
public sealed class GiftBoxPlugin : QuickShapePlugin
{
    public override string Name => "演出：プレゼント箱";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GiftBoxParameter(sharedData);
}

public sealed class GiftBoxParameter : QuickShapeParameter
{
    public GiftBoxParameter() : this(null) { }
    public GiftBoxParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x42A5F5);
        Color2 = Hex(0xFF4081);
        Count = 30;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "紙吹雪の数", Min: 0, Max: 80, Group: "「プレゼント箱」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "箱の色"),
        ["Color2"] = new ParamGuide(Label: "リボンの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float shake = c.Time < 1f ? MathF.Sin(t * 40f) * 8f * u * MathF.Min(1f, c.Time * 2f) : 0f;
        float pop = Intro(c, 1f, 0.5f);
        var bc = o + new Vector2(shake, 120f * u);
        float bw = 400f * u, bh = 320f * u;
        if (pop > 0f) DrawKit.Glow(dc, bc - new Vector2(0, bh / 2f), 500f * u * pop, Rgb(1, 1, 0.8f, 0.6f * a * pop));
        Box(dc, b, bc.X - bw / 2f, bc.Y - bh / 2f, bc.X + bw / 2f, bc.Y + bh / 2f, C1(a));
        Box(dc, b, bc.X - 30f * u, bc.Y - bh / 2f, bc.X + 30f * u, bc.Y + bh / 2f, C2(a));
        var lid = bc + new Vector2(0, -bh / 2f - 30f * u) + new Vector2(pop * 200f * u, -DrawKit.EaseOut(pop) * 400f * u + pop * pop * 200f * u);
        float lr = pop * 1.2f;
        Poly(dc, b, DrawKit.Place(DrawKit.RectPts(-bw / 2f - 20f * u, -30f * u, bw + 40f * u, 60f * u), lid, lr), Col(Gfx.Mix(Color1, Hex(0x000000), 0.1f), a));
        Poly(dc, b, DrawKit.Place(DrawKit.RectPts(-30f * u, -30f * u, 60f * u, 60f * u), lid, lr), C2(a));
        for (int e = -1; e <= 1; e += 2) Poly(dc, b, DrawKit.Place(Pts(Vector2.Zero, u, u, 0f, -30f, e * 90f, -100f, e * 110f, -40f), lid, lr), C2(a));
        if (pop > 0f)
            for (int i = 0; i < Count; i++)
            {
                float f = Math.Clamp((c.Time - 1f) / 1.5f, 0f, 1f);
                var q = bc - new Vector2(0, bh / 2f) + new Vector2((DrawKit.H(Seed, i) - 0.5f) * 1000f * u * f, -DrawKit.H(Seed, i, 1) * 600f * u * DrawKit.EaseOut(f) + f * f * 500f * u);
                Poly(dc, b, DrawKit.Place(DrawKit.RectPts(-10f * u, -5f * u, 20f * u, 10f * u), q, t * 5f + i), Col(BubbleKit.Palette(0, i), a * (1f - f * 0.5f)));
            }
    }
}

// =====================================================================
//  ハンコ（押印）
//   ハンコがドンと押されて、赤い丸い印影が残る。「文字」で名前(縦書き)
// =====================================================================
public sealed class HankoStampPlugin : QuickShapePlugin
{
    public override string Name => "演出：ハンコ（押印）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new HankoStampParameter(sharedData);
}

public sealed class HankoStampParameter : QuickShapeParameter
{
    public HankoStampParameter() : this(null) { }
    public HankoStampParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "山田";
        Color1 = Hex(0xD32F2F);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "名前", Tip: "縦に並びます", Group: "「ハンコ（押印）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "朱肉の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float down = Intro(c, 0f, 0.25f);
        float up = Intro(c, 0.4f, 0.3f);
        float R = 160f * u;
        if (down >= 1f)
        {
            float k = 1f + 0.08f * MathF.Max(0f, 1f - (c.Time - 0.25f) * 6f);
            Ring(dc, b, o, R * k, C1(0.9f * a), 14f * u);
            string s = Str;
            int n = Math.Max(1, s.Length);
            for (int i = 0; i < s.Length; i++) Say(dc, b, s[i].ToString(), R * 1.3f / n, o + new Vector2(0, (i - (n - 1) / 2f) * R * 1.4f / n), C1(0.9f * a), scale: k);
        }
        float lift = down < 1f ? (1f - DrawKit.EaseOut(down)) * 400f * u : DrawKit.EaseOut(up) * 500f * u;
        if (up >= 1f) return;
        var hp = o - new Vector2(0, lift + 20f * u);
        Box(dc, b, hp.X - R, hp.Y - 40f * u, hp.X + R, hp.Y, Rgb(0.2f, 0.2f, 0.22f, a));
        Poly(dc, b, Pts(hp, u, u, -120f, -40f, 120f, -40f, 80f, -300f, -80f, -300f), Rgb(0.95f, 0.88f, 0.75f, a));
        Oval(dc, b, hp - new Vector2(0, 300f * u), 80f * u, 30f * u, Rgb(0.9f, 0.8f, 0.65f, a));
    }
}

// =====================================================================
//  はかり（体重計）の針
//   丸い目盛りの体重計。針がぐるんと回って「値」の位置でゆれて止まる。「文字」で単位
// =====================================================================
public sealed class ScaleNeedlePlugin : QuickShapePlugin
{
    public override string Name => "演出：はかり（体重計）の針";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ScaleNeedleParameter(sharedData);
}

public sealed class ScaleNeedleParameter : QuickShapeParameter
{
    public ScaleNeedleParameter() : this(null) { }
    public ScaleNeedleParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "kg";
        Color1 = Hex(0xF5F5F5);
        Color2 = Hex(0xE53935);
    }
    protected override double DefAmount => 60;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "単位", Group: "「はかり（体重計）の針」の設定"),
        ["Amount"] = new ParamGuide(Label: "はかる値", Tip: "0〜100(表示もこの数)", Group: "「はかり（体重計）の針」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "目盛り板の色"),
        ["Color2"] = new ParamGuide(Label: "針の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = 300f * u;
        RBox(dc, b, o + new Vector2(0, 60f * u), 760f * u, 820f * u, 80f * u, Rgb(0.85f, 0.87f, 0.9f, a));
        Dot(dc, b, o, R + 20f * u, Rgb(0.3f, 0.32f, 0.35f, a));
        Dot(dc, b, o, R, C1(a));
        float a0 = MathF.PI * 0.8f, span = MathF.PI * 1.4f;
        for (int i = 0; i <= 50; i++)
        {
            float an = a0 + span * i / 50f;
            bool big = i % 10 == 0;
            Ln(dc, b, Pol(o, R * (big ? 0.78f : 0.86f), an), Pol(o, R * 0.94f, an), Rgb(0.2f, 0.2f, 0.22f, a), (big ? 5f : 2.5f) * u);
            if (big) Say(dc, b, (i * 2).ToString(), 34f * u, Pol(o, R * 0.64f, an), Rgb(0.2f, 0.2f, 0.22f, a));
        }
        float target = Amt(c);
        float settle = Intro(c, 0f, 1.8f);
        float v = target * DrawKit.EaseOut(MathF.Min(1f, settle * 1.6f)) + MathF.Sin(settle * 18f) * 0.08f * (1f - settle);
        float na = a0 + span * Math.Clamp(v, 0f, 1.05f);
        Ln(dc, b, Pol(o, -40f * u, na), Pol(o, R * 0.9f, na), C2(a), 8f * u);
        Dot(dc, b, o, 22f * u, Rgb(0.2f, 0.2f, 0.22f, a));
        Say(dc, b, $"{target * 100f:0.0} {Str}", 54f * u, o + new Vector2(0, R * 0.55f), Rgb(0.2f, 0.2f, 0.22f, a));
    }
}

// =====================================================================
//  ろうそくの炎
//   ゆらめくろうそくの炎と、まわりの明かり。バリエーション2で燭台つき。「数」で本数
// =====================================================================
public sealed class CandleFlamePlugin : QuickShapePlugin
{
    public override string Name => "演出：ろうそくの炎";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CandleFlameParameter(sharedData);
}

public sealed class CandleFlameParameter : QuickShapeParameter
{
    public CandleFlameParameter() : this(null) { }
    public CandleFlameParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 1;
        Color1 = Hex(0xFFF8E1);
        Color2 = Hex(0xFFB74D);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "ろうそくの数", Min: 1, Max: 9, Group: "「ろうそくの炎」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=ろうそくだけ　2=燭台つき", Min: 1, Max: 2, Group: "「ろうそくの炎」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ろうそくの色"),
        ["Color2"] = new ParamGuide(Label: "炎の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Clamp(Count, 1, 9);
        for (int i = 0; i < n; i++)
        {
            var cb = o + new Vector2((i - (n - 1) / 2f) * 200f * u, (n > 1 ? MathF.Abs(i - (n - 1) / 2f) * 30f : 0f) * u);
            float ch = 360f * u, cw = 70f * u;
            if (Variant >= 2) { Oval(dc, b, cb + new Vector2(0, ch / 2f + 10f * u), 120f * u, 26f * u, Rgb(0.75f, 0.6f, 0.3f, a)); Box(dc, b, cb.X - 20f * u, cb.Y + ch / 2f + 10f * u, cb.X + 20f * u, cb.Y + ch / 2f + 140f * u, Rgb(0.75f, 0.6f, 0.3f, a)); }
            Box(dc, b, cb.X - cw / 2f, cb.Y - ch / 2f, cb.X + cw / 2f, cb.Y + ch / 2f, C1(a));
            Oval(dc, b, cb - new Vector2(0, ch / 2f), cw / 2f, 10f * u, Rgb(1, 1, 1, a));
            Ln(dc, b, cb - new Vector2(0, ch / 2f), cb - new Vector2(0, ch / 2f + 26f * u), Rgb(0.2f, 0.2f, 0.2f, a), 4f * u);
            float fl = MathF.Sin(t * 11f + i * 2f) * 0.5f + MathF.Sin(t * 17f + i) * 0.5f;
            var fp = cb - new Vector2(-fl * 3f * u, ch / 2f + 60f * u);
            DrawKit.Glow(dc, fp, (260f + 20f * fl) * u, C2(0.35f * a));
            float fh = 60f * u * (1f + 0.1f * fl);
            Poly(dc, b, Pts(fp, 24f * u, fh, fl * 0.3f, -1.1f, 0.8f, 0.1f, 0.6f, 0.6f, 0f, 0.85f, -0.6f, 0.6f, -0.8f, 0.1f), C2(0.9f * a));
            Poly(dc, b, Pts(fp + new Vector2(0, 12f * u), 13f * u, fh * 0.6f, fl * 0.2f, -1f, 0.8f, 0.3f, 0f, 0.9f, -0.8f, 0.3f), Rgb(1, 1, 0.9f, a));
        }
    }
}
