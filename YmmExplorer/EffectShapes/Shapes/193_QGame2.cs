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
//  宝の地図
//   古い紙の地図に、点線の道が伸びて、最後に×印が出る
// =====================================================================
public sealed class TreasureMapPlugin : QuickShapePlugin
{
    public override string Name => "演出：宝の地図";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TreasureMapParameter(sharedData);
}

public sealed class TreasureMapParameter : QuickShapeParameter
{
    public TreasureMapParameter() : this(null) { }
    public TreasureMapParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xE8D3A2);
        Color2 = Hex(0xC62828);
        Count = 6;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "道の曲がり角の数", Min: 2, Max: 12, Group: "「宝の地図」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "紙の色"),
        ["Color2"] = new ParamGuide(Label: "×印と道の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float mw = 1300f * u, mh = 780f * u;
        var edge = new List<Vector2>();
        for (int i = 0; i < 40; i++)
        {
            float f = i / 40f;
            Vector2 pp = f < 0.25f ? new(-0.5f + f * 4f, -0.5f) : f < 0.5f ? new(0.5f, -0.5f + (f - 0.25f) * 4f) : f < 0.75f ? new(0.5f - (f - 0.5f) * 4f, 0.5f) : new(-0.5f, 0.5f - (f - 0.75f) * 4f);
            float j = (DrawKit.H(Seed, i) - 0.5f) * 0.025f;
            edge.Add(o + new Vector2(pp.X * mw * (1f + j), pp.Y * mh * (1f + j)));
        }
        float k = DrawKit.EaseOut(Intro(c, 0f, 0.4f));
        Poly(dc, b, DrawKit.Place(edge.Select(x => x - o), o, 0f, k, 1f), C1(a));
        using (var g = Gfx.Radial(dc, o, mw * 0.65f, (0.5f, Rgb(0, 0, 0, 0f)), (1f, Rgb(0.35f, 0.2f, 0.05f, 0.45f * a))))
            Gfx.FillPoly(dc, g, DrawKit.Place(edge.Select(x => x - o), o, 0f, k, 1f));
        if (k < 1f) return;
        int n = Math.Clamp(Count, 2, 12);
        var pts = new List<Vector2>();
        for (int i = 0; i <= n; i++) pts.Add(o + new Vector2(-mw * 0.4f + mw * 0.8f * i / n, (DrawKit.H(Seed, i, 3) - 0.5f) * mh * 0.6f));
        var path = new List<Vector2>();
        for (int i = 0; i < n; i++) path.AddRange(BubbleKit.Quad(pts[i], (pts[i] + pts[i + 1]) / 2f + new Vector2(0, (i % 2 == 0 ? -1 : 1) * 80f * u), pts[i + 1], 14));
        float p = DrawKit.EaseOut(Intro(c, 0.4f, 1.6f));
        var part = BubbleKit.PartByLength(path, p);
        float acc = 0f;
        for (int i = 0; i < part.Count - 1; i++)
        {
            float d = Vector2.Distance(part[i], part[i + 1]);
            if ((int)((acc) / (22f * u)) % 2 == 0) Ln(dc, b, part[i], part[i + 1], C2(0.85f * a), 6f * u);
            acc += d;
        }
        if (p >= 1f)
        {
            var xp = pts[^1];
            float s = 40f * u * DrawKit.EaseOutBack(Intro(c, 2f, 0.3f));
            Ln(dc, b, xp - new Vector2(s, s), xp + new Vector2(s, s), C2(a), 14f * u);
            Ln(dc, b, xp - new Vector2(s, -s), xp + new Vector2(s, -s), C2(a), 14f * u);
        }
        var cp = o + new Vector2(mw * 0.38f, -mh * 0.33f);
        Ring(dc, b, cp, 60f * u, Rgb(0.35f, 0.22f, 0.1f, 0.7f * a), 3f * u);
        Poly(dc, b, Pts(cp, 60f * u, 60f * u, 0f, -1f, 0.18f, 0f, 0f, 1f, -0.18f, 0f), Rgb(0.35f, 0.22f, 0.1f, 0.7f * a));
        Say(dc, b, "N", 26f * u, cp - new Vector2(0, 80f * u), Rgb(0.35f, 0.22f, 0.1f, 0.8f * a));
    }
}

// =====================================================================
//  クエスト受注・達成の帯
//   「QUEST CLEAR」などの帯が、左右から伸びて出る。1行目が大きな文字、2行目がクエスト名
// =====================================================================
public sealed class QuestBannerPlugin : QuickShapePlugin
{
    public override string Name => "演出：クエスト受注・達成の帯";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new QuestBannerParameter(sharedData);
}

public sealed class QuestBannerParameter : QuickShapeParameter
{
    public QuestBannerParameter() : this(null) { }
    public QuestBannerParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "QUEST CLEAR\nスライムを10匹たおせ";
        Color1 = Hex(0x1A237E);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Tip: "1行目が大きな文字、2行目がクエスト名", Group: "「クエスト受注・達成の帯」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "帯の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "文字と線の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var ln = Str.Split('\n');
        string big = ln.Length > 0 ? ln[0] : "", sub = ln.Length > 1 ? ln[1] : "";
        float p = DrawKit.EaseOut(Intro(c, 0f, 0.4f));
        float bw = 1700f * u * p, bh = 230f * u;
        Box(dc, b, o.X - bw / 2f, o.Y - bh / 2f, o.X + bw / 2f, o.Y + bh / 2f, C1(0.9f * a));
        for (int e = -1; e <= 1; e += 2) Box(dc, b, o.X - bw / 2f, o.Y + e * bh / 2f - 4f * u, o.X + bw / 2f, o.Y + e * bh / 2f + 4f * u, C3(a));
        float q = DrawKit.EaseOutBack(Intro(c, 0.3f, 0.35f));
        Say(dc, b, big, 110f * u, o + new Vector2(0, -25f * u), C3(a), Rgb(0.3f, 0.2f, 0f, a), 5f * u, scale: q);
        Say(dc, b, sub, 40f * u, o + new Vector2(0, 70f * u), Rgb(1, 1, 1, a * Intro(c, 0.5f, 0.3f)));
        float sh = Gfx.Mod(c.Time * 0.6f, 1.6f) - 0.3f;
        if (sh > 0f && sh < 1f) Poly(dc, b, new List<Vector2> { new(o.X - bw / 2f + bw * sh, o.Y - bh / 2f), new(o.X - bw / 2f + bw * sh + 60f * u, o.Y - bh / 2f), new(o.X - bw / 2f + bw * sh - 40f * u, o.Y + bh / 2f), new(o.X - bw / 2f + bw * sh - 100f * u, o.Y + bh / 2f) }, Rgb(1, 1, 1, 0.15f * a));
    }
}

// =====================================================================
//  スキルツリー
//   線でつながった丸が、根元から順に解放されて光る。「数」で段の数、「値」で解放する割合
// =====================================================================
public sealed class SkillTreePlugin : QuickShapePlugin
{
    public override string Name => "演出：スキルツリー";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SkillTreeParameter(sharedData);
}

public sealed class SkillTreeParameter : QuickShapeParameter
{
    public SkillTreeParameter() : this(null) { }
    public SkillTreeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 4;
        Color1 = Hex(0x26C6DA);
        Color2 = Hex(0x37474F);
    }
    protected override double DefAmount => 70;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "解放する割合", Group: "「スキルツリー」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "段の数", Min: 2, Max: 7, Group: "「スキルツリー」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "解放した色"),
        ["Color2"] = new ParamGuide(Label: "まだの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int L = Math.Clamp(Count, 2, 7);
        var nodes = new List<(Vector2 p, int parent, int level)>();
        nodes.Add((o + new Vector2(0, 320f * u), -1, 0));
        for (int lv = 1; lv < L; lv++)
        {
            int cnt = Math.Min(2 + lv, 6);
            for (int i = 0; i < cnt; i++)
            {
                var p = o + new Vector2((i - (cnt - 1) / 2f) * 230f * u, 320f * u - lv * 640f * u / (L - 1));
                int best = 0; float bd = float.MaxValue;
                for (int j = 0; j < nodes.Count; j++) if (nodes[j].level == lv - 1) { float d = MathF.Abs(nodes[j].p.X - p.X); if (d < bd) { bd = d; best = j; } }
                nodes.Add((p, best, lv));
            }
        }
        int unlocked = (int)MathF.Round(nodes.Count * Amt(c) * Math.Clamp(c.Time / 2f * Spd(c), 0f, 1f));
        for (int i = 1; i < nodes.Count; i++)
        {
            var par = nodes[nodes[i].parent].p;
            bool on = i < unlocked;
            Ln(dc, b, par, nodes[i].p, on ? C1(a) : C2(a), 8f * u);
        }
        for (int i = 0; i < nodes.Count; i++)
        {
            bool on = i < unlocked;
            bool justNow = on && i == unlocked - 1;
            var q = nodes[i].p;
            if (on) DrawKit.Glow(dc, q, 90f * u * (justNow ? 1.5f : 1f), C1(0.45f * a));
            Dot(dc, b, q, 46f * u, on ? C1(a) : C2(a));
            Ring(dc, b, q, 46f * u, Rgb(1, 1, 1, (on ? 0.9f : 0.3f) * a), 4f * u);
            if (on) Star(dc, b, q, 24f * u, 4, 0.4f, 0f, Rgb(1, 1, 1, a));
            else Say(dc, b, "?", 34f * u, q, Rgb(1, 1, 1, 0.4f * a));
        }
    }
}

// =====================================================================
//  インベントリ（持ち物）画面
//   マス目にアイテムのアイコンが並び、選択カーソルが動く。「文字」にカンマ区切りでアイテム名
// =====================================================================
public sealed class InventoryPlugin : QuickShapePlugin
{
    public override string Name => "演出：インベントリ（持ち物）画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new InventoryParameter(sharedData);
}

public sealed class InventoryParameter : QuickShapeParameter
{
    public InventoryParameter() : this(null) { }
    public InventoryParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "剣,盾,薬,鍵,宝石,地図,パン,杖";
        Count = 24;
        Color1 = Hex(0x263238);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "アイテム名", Tip: "カンマ区切り。名前に合ったアイコン(剣・盾・薬・鍵・宝石・地図・パン・杖)になります", Group: "「インベントリ（持ち物）画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "マスの数", Min: 4, Max: 48, Group: "「インベントリ（持ち物）画面」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "画面の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "カーソルの色"),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        int n = Math.Clamp(Count, 4, 48), cols = 8, rows = (n + cols - 1) / cols;
        float cs = 130f * u, gap = 12f * u;
        float gw = cols * cs + (cols - 1) * gap, gh = rows * cs + (rows - 1) * gap;
        RBox(dc, b, o + new Vector2(0, 40f * u), gw + 60f * u, gh + 160f * u, 20f * u, C1(0.92f * a));
        SayAt(dc, b, "もちもの", 44f * u, new Vector2(o.X - gw / 2f, o.Y - gh / 2f - 50f * u), Rgb(1, 1, 1, a));
        int sel = it.Count == 0 ? 0 : (int)(c.Time * 1.2f * Spd(c)) % Math.Max(1, Math.Min(it.Count, n));
        for (int i = 0; i < n; i++)
        {
            var q = new Vector2(o.X - gw / 2f + cs / 2f + (i % cols) * (cs + gap), o.Y - gh / 2f + cs / 2f + 40f * u + (i / cols) * (cs + gap));
            RBox(dc, b, q, cs, cs, 12f * u, Rgb(1, 1, 1, 0.08f * a));
            RFrame(dc, b, q, cs, cs, 12f * u, Rgb(1, 1, 1, 0.2f * a), 2f * u);
            if (i >= it.Count) continue;
            float p = DrawKit.EaseOutBack(Intro(c, 0.1f + i * 0.05f, 0.3f));
            string nm = it[i];
            float s = cs * 0.32f * p;
            var col = Col(BubbleKit.Palette(0, i), a);
            if (nm.Contains("剣")) { Ln(dc, b, q + new Vector2(-s, s), q + new Vector2(s, -s), Rgb(0.85f, 0.88f, 0.92f, a), 10f * u * p); Ln(dc, b, q + new Vector2(-s * 0.9f, s * 0.3f), q + new Vector2(-s * 0.3f, s * 0.9f), Rgb(0.7f, 0.5f, 0.2f, a), 8f * u * p); }
            else if (nm.Contains("盾")) Poly(dc, b, Pts(q, s, s, -1f, -1f, 1f, -1f, 1f, 0.2f, 0f, 1.1f, -1f, 0.2f), Rgb(0.3f, 0.5f, 0.9f, a));
            else if (nm.Contains("薬")) { Oval(dc, b, q + new Vector2(0, s * 0.3f), s * 0.8f, s * 0.8f, Rgb(0.9f, 0.2f, 0.4f, a)); Box(dc, b, q.X - s * 0.3f, q.Y - s, q.X + s * 0.3f, q.Y - s * 0.3f, Rgb(0.8f, 0.85f, 0.9f, a)); }
            else if (nm.Contains("鍵")) { Ring(dc, b, q + new Vector2(-s * 0.5f, 0), s * 0.4f, Rgb(1f, 0.8f, 0.2f, a), 6f * u * p); Ln(dc, b, q + new Vector2(-s * 0.1f, 0), q + new Vector2(s, 0), Rgb(1f, 0.8f, 0.2f, a), 6f * u * p); }
            else if (nm.Contains("宝") || nm.Contains("石")) Poly(dc, b, Pts(q, s, s, 0f, -1f, 0.8f, -0.3f, 0f, 1f, -0.8f, -0.3f), Rgb(0.3f, 0.9f, 0.9f, a));
            else if (nm.Contains("地図")) Box(dc, b, q.X - s, q.Y - s * 0.7f, q.X + s, q.Y + s * 0.7f, Rgb(0.9f, 0.8f, 0.55f, a));
            else if (nm.Contains("パン")) Oval(dc, b, q, s, s * 0.6f, Rgb(0.85f, 0.6f, 0.3f, a));
            else if (nm.Contains("杖")) { Ln(dc, b, q + new Vector2(-s * 0.6f, s), q + new Vector2(s * 0.4f, -s * 0.6f), Rgb(0.6f, 0.4f, 0.2f, a), 7f * u * p); Dot(dc, b, q + new Vector2(s * 0.5f, -s * 0.7f), s * 0.35f, Rgb(0.6f, 0.4f, 1f, a)); }
            else { Dot(dc, b, q, s, col); Say(dc, b, nm.Length > 0 ? nm[..1] : "", 40f * u, q, Rgb(1, 1, 1, a), scale: p); }
            if (i == sel)
            {
                RFrame(dc, b, q, cs + 10f * u, cs + 10f * u, 14f * u, C3(a * (0.7f + 0.3f * MathF.Sin(t * 8f))), 6f * u);
                Say(dc, b, nm, 40f * u, new Vector2(o.X, o.Y + gh / 2f + 100f * u), Rgb(1, 1, 1, a));
            }
        }
    }
}

// =====================================================================
//  ルーレット盤（大きな円盤）
//   色分けされた円盤が回って、「値」で決めた区切りで止まる。「文字」にカンマ区切りで区切りの名前
// =====================================================================
public sealed class RouletteWheelPlugin : QuickShapePlugin
{
    public override string Name => "演出：ルーレット盤（大きな円盤）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RouletteWheelParameter(sharedData);
}

public sealed class RouletteWheelParameter : QuickShapeParameter
{
    public RouletteWheelParameter() : this(null) { }
    public RouletteWheelParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "1等,ハズレ,2等,ハズレ,3等,もう一回";
        Color3 = Hex(0xFFD54A);
    }
    protected override double DefAmount => 0;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "区切りの名前", Tip: "カンマ区切り", Group: "「ルーレット盤（大きな円盤）」の設定"),
        ["Amount"] = new ParamGuide(Label: "止まる区切り", Tip: "0〜100を区切りの数で等分(0=最初の区切り)", Group: "「ルーレット盤（大きな円盤）」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "針の色"),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        int n = Math.Max(2, it.Count);
        int target = Math.Clamp((int)(Amt(c) * n), 0, n - 1);
        float seg = MathF.PI * 2f / n;
        float spin = 2.5f / MathF.Max(0.1f, Spd(c));
        float p = DrawKit.EaseOut(Intro(c, 0.2f, spin));
        float finalRot = -MathF.PI / 2f - (target + 0.5f) * seg + MathF.PI * 2f * 6f;
        float rot = finalRot * p;
        float R = 380f * u;
        Dot(dc, b, o, R + 20f * u, Rgb(0.2f, 0.15f, 0.12f, a));
        for (int i = 0; i < n; i++)
        {
            float a0 = rot + i * seg;
            var sl = new List<Vector2> { o };
            sl.AddRange(Gfx.ArcPts(o, R, a0, a0 + seg, 16));
            Poly(dc, b, sl, Col(BubbleKit.Palette(0, i), a));
            var lp = Pol(o, R * 0.65f, a0 + seg / 2f);
            var prev = dc.Transform;
            dc.Transform = Matrix3x2.CreateRotation(a0 + seg / 2f, lp) * prev;
            Say(dc, b, i < it.Count ? it[i] : "", 38f * u, lp, Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.4f * a), 3f * u, maxW: R * 0.6f);
            dc.Transform = prev;
        }
        for (int i = 0; i < n * 2; i++) Dot(dc, b, Pol(o, R + 8f * u, rot + i * seg / 2f), 7f * u, Rgb(1, 0.95f, 0.7f, a));
        Dot(dc, b, o, 50f * u, C3(a));
        var tip = o + new Vector2(0, -R + 30f * u);
        Poly(dc, b, new List<Vector2> { tip, tip + new Vector2(-30f, -80f) * u, tip + new Vector2(30f, -80f) * u }, C3(a));
        if (p >= 1f) DrawKit.Glow(dc, Pol(o, R * 0.6f, -MathF.PI / 2f), 200f * u, Rgb(1, 1, 1, 0.35f * a * (0.5f + 0.5f * MathF.Sin(t * 8f))));
    }
}

// =====================================================================
//  パズルの連鎖
//   色の玉が並んだ盤面で、同じ色がそろうと光って消え、「〇連鎖！」と出る
// =====================================================================
public sealed class PuzzleChainPlugin : QuickShapePlugin
{
    public override string Name => "演出：パズルの連鎖";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PuzzleChainParameter(sharedData);
}

public sealed class PuzzleChainParameter : QuickShapeParameter
{
    public PuzzleChainParameter() : this(null) { }
    public PuzzleChainParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 3;
        Color3 = Hex(0xFFEB3B);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "連鎖の数", Min: 1, Max: 9, Group: "「パズルの連鎖」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "連鎖の文字の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int cols = 6, rows = 10;
        float cs = 70f * u;
        var tl = o - new Vector2(cols * cs, rows * cs) / 2f;
        RBox(dc, b, o, cols * cs + 30f * u, rows * cs + 30f * u, 16f * u, Rgb(0.08f, 0.08f, 0.15f, 0.9f * a));
        var cols4 = new[] { Hex(0xEF5350), Hex(0x42A5F5), Hex(0x66BB6A), Hex(0xFFCA28) };
        int chains = Math.Clamp(Count, 1, 9);
        float stepT = 0.9f;
        int cur = Math.Min(chains, (int)MathF.Floor(MathF.Max(0f, c.Time - 0.5f) / stepT));
        float fr = MathF.Max(0f, c.Time - 0.5f) / stepT - cur;
        for (int r = 0; r < rows; r++)
            for (int k = 0; k < cols; k++)
            {
                int idx = r * cols + k;
                int colI = (int)(DrawKit.H(Seed, idx) * 4f) % 4;
                int group = r >= rows - chains * 2 ? (rows - 1 - r) / 2 : -1;
                bool inChain = group >= 0 && k >= (group % 3) && k < (group % 3) + 4 && (r % 2 == 0 || k == group % 3);
                if (inChain) colI = group % 4;
                float shiftDown = 0f;
                if (inChain && group < cur) continue;
                if (inChain && group == cur && cur < chains)
                {
                    float flash = 0.5f + 0.5f * MathF.Sin(fr * 30f);
                    var q0 = tl + new Vector2((k + 0.5f) * cs, (r + 0.5f) * cs);
                    if (fr < 0.6f) { Dot(dc, b, q0, cs * 0.44f, Col(cols4[colI], a)); Dot(dc, b, q0, cs * 0.44f, Rgb(1, 1, 1, 0.6f * flash * a)); }
                    else { float f = (fr - 0.6f) / 0.4f; for (int s = 0; s < 4; s++) Dot(dc, b, Pol(q0, cs * f, s * MathF.PI / 2f + 0.5f), cs * 0.1f * (1f - f), Col(cols4[colI], a)); }
                    continue;
                }
                var q = tl + new Vector2((k + 0.5f) * cs, (r + 0.5f) * cs + shiftDown);
                Dot(dc, b, q, cs * 0.44f, Col(cols4[colI], a));
                Dot(dc, b, q + new Vector2(-cs * 0.13f, -cs * 0.13f), cs * 0.12f, Rgb(1, 1, 1, 0.6f * a));
            }
        if (cur >= 1)
        {
            int shown = Math.Min(cur + (fr > 0.6f ? 1 : 0), chains);
            float k2 = DrawKit.EaseOutBack(Math.Clamp(fr * 4f, 0f, 1f));
            Say(dc, b, $"{shown}連鎖！", 90f * u, o + new Vector2(cols * cs / 2f + 220f * u, -100f * u), C3(a), Rgb(0.6f, 0.1f, 0.1f, a), 6f * u, scale: k2);
        }
    }
}

// =====================================================================
//  ポーズ画面
//   画面が暗くなって「PAUSE」と、選択肢のメニュー。カーソルが点滅する。2行目からがメニュー
// =====================================================================
public sealed class PauseScreenPlugin : QuickShapePlugin
{
    public override string Name => "演出：ポーズ画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PauseScreenParameter(sharedData);
}

public sealed class PauseScreenParameter : QuickShapeParameter
{
    public PauseScreenParameter() : this(null) { }
    public PauseScreenParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "PAUSE\nつづける\nやりなおす\nタイトルへ";
        Color3 = Hex(0xFFD54A);
    }
    protected override double DefAmount => 0;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Tip: "1行目が見出し、2行目からがメニュー", Group: "「ポーズ画面」の設定"),
        ["Amount"] = new ParamGuide(Label: "選んでいる項目", Tip: "0〜100をメニューの数で等分", Group: "「ポーズ画面」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "画面を暗くする", Group: "「ポーズ画面」の設定"),
        ["Color1"] = new ParamGuide(Hidden: true),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "カーソルの色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
        ["PosX"] = new ParamGuide(Hidden: true),
        ["PosY"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var ln = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        if (ln.Count == 0) return;
        float s = U0(c) * c.F(Scale) / 100f;
        var cc = new Vector2(w, h) / 2f;
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(0, 0, 0, 0.6f * a));
        Say(dc, b, ln[0], 150f * s, cc + new Vector2(0, -200f * s), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.5f * a), 5f * s);
        int m = ln.Count - 1;
        int sel = m == 0 ? 0 : Math.Clamp((int)(Amt(c) * m), 0, m - 1);
        for (int i = 0; i < m; i++)
        {
            var q = cc + new Vector2(0, -20f * s + i * 90f * s);
            bool on = i == sel;
            Say(dc, b, ln[i + 1], 54f * s, q, on ? C3(a) : Rgb(0.8f, 0.8f, 0.85f, a));
            if (on && (int)(t * 3f) % 2 == 0)
            {
                var tw = Measure(ln[i + 1], 54f * s).Width;
                Poly(dc, b, Pts(q - new Vector2(tw / 2f + 50f * s, 0), s, s, -16f, -18f, 16f, 0f, -16f, 18f), C3(a));
            }
        }
    }
}

// =====================================================================
//  ターン開始の帯
//   「YOUR TURN」の帯が、画面を左から右へ横切る
// =====================================================================
public sealed class TurnStartPlugin : QuickShapePlugin
{
    public override string Name => "演出：ターン開始の帯";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TurnStartParameter(sharedData);
}

public sealed class TurnStartParameter : QuickShapeParameter
{
    public TurnStartParameter() : this(null) { }
    public TurnStartParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "YOUR TURN";
        Color1 = Hex(0x1565C0);
        Color3 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Group: "「ターン開始の帯」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "帯の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "文字の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
        ["PosX"] = new ParamGuide(Hidden: true),
        ["PosY"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        float cy = h * c.F(PosY) / 100f;
        float tt = c.Time;
        float x = tt < 0.3f ? -w + w * DrawKit.EaseOut(tt / 0.3f) : tt < 1.3f ? (tt - 0.3f) * 80f * s : (tt - 0.3f) * 80f * s + w * DrawKit.EaseOut(MathF.Min(1f, (tt - 1.3f) / 0.3f)) * 1.2f;
        float bh = 200f * s;
        var band = new List<Vector2> { new(x - 100f * s, cy - bh / 2f), new(x + w + 100f * s, cy - bh / 2f), new(x + w, cy + bh / 2f), new(x - 200f * s, cy + bh / 2f) };
        Poly(dc, b, band, C1(0.9f * a));
        for (int i = 0; i < 3; i++) Box(dc, b, x - 100f * s, cy - bh / 2f + 20f * s + i * 6f * s, x + w, cy - bh / 2f + 22f * s + i * 6f * s, C3(0.3f * a));
        Say(dc, b, Str, 130f * s, new Vector2(x + w / 2f, cy), C3(a), Rgb(0, 0, 0, 0.4f * a), 5f * s);
    }
}

// =====================================================================
//  バフ・デバフの矢印
//   キャラの横で、上向き(強化)や下向き(弱体)の矢印が、上下に流れる。「文字」に効果名
// =====================================================================
public sealed class BuffDebuffPlugin : QuickShapePlugin
{
    public override string Name => "演出：バフ・デバフの矢印";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BuffDebuffParameter(sharedData);
}

public sealed class BuffDebuffParameter : QuickShapeParameter
{
    public BuffDebuffParameter() : this(null) { }
    public BuffDebuffParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "攻撃力UP";
        Count = 3;
        Color1 = Hex(0xFF5252);
        Color2 = Hex(0x448AFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "効果の名前", Group: "「バフ・デバフの矢印」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "矢印の数", Min: 1, Max: 8, Group: "「バフ・デバフの矢印」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=上向き(強化)　2=下向き(弱体)", Min: 1, Max: 2, Group: "「バフ・デバフの矢印」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "強化の色"),
        ["Color2"] = new ParamGuide(Label: "弱体の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        bool down = Variant >= 2;
        var col = down ? Color2 : Color1;
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 0.8f * Spd(c) + i / (float)Count, 1f);
            float x = o.X + (i - (Count - 1) / 2f) * 90f * u + 260f * u;
            float y = down ? o.Y - 150f * u + ph * 300f * u : o.Y + 150f * u - ph * 300f * u;
            float al = MathF.Sin(ph * MathF.PI);
            var dir = down ? 1f : -1f;
            Poly(dc, b, Pts(new Vector2(x, y), 40f * u, 40f * u, -0.35f, -dir * 0.6f, 0.35f, -dir * 0.6f, 0.35f, 0f, 0.8f, 0f, 0f, dir * 0.9f, -0.8f, 0f, -0.35f, 0f), Col(col, al * a));
        }
        DrawKit.Glow(dc, o, 260f * u, Col(col, 0.2f * a * (0.6f + 0.4f * MathF.Sin(t * 4f))));
        Say(dc, b, Str, 54f * u, o + new Vector2(260f * u, -220f * u), Col(col, a), Rgb(1, 1, 1, a), 5f * u);
    }
}

// =====================================================================
//  ジャンプ・着地の砂ぼこり
//   足元に、もくもくの砂ぼこりが左右に広がって消える。くり返す
// =====================================================================
public sealed class JumpDustPlugin : QuickShapePlugin
{
    public override string Name => "演出：ジャンプ・着地の砂ぼこり";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new JumpDustParameter(sharedData);
}

public sealed class JumpDustParameter : QuickShapeParameter
{
    public JumpDustParameter() : this(null) { }
    public JumpDustParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xD7CCC8);
        Count = 8;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "もくもくの数", Min: 3, Max: 20, Group: "「ジャンプ・着地の砂ぼこり」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "砂ぼこりの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float per = 1.2f / MathF.Max(0.1f, Spd(c));
        float f = Gfx.Mod(c.Time, per) / per;
        float e = DrawKit.EaseOut(MathF.Min(1f, f * 1.5f));
        for (int side = -1; side <= 1; side += 2)
            for (int i = 0; i < Count; i++)
            {
                float k = (i + 1f) / Count;
                var q = o + new Vector2(side * (40f + 260f * k * e) * u, -(MathF.Sin(k * MathF.PI) * 40f + 20f * e) * u);
                float r = (26f + 30f * (1f - k)) * u * (0.6f + 0.6f * e);
                Dot(dc, b, q, r, C1(0.75f * a * (1f - f)));
            }
        Oval(dc, b, o, 300f * u * e, 20f * u, Rgb(0, 0, 0, 0.15f * a * (1f - f)));
    }
}
