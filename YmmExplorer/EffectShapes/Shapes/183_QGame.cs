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
//  ボスのHPバー（段階つき）
//   画面上の大きなHPバー。「値」が残り、「数」が段の数。減った分は少し遅れて赤く消える
// =====================================================================
public sealed class BossHpBarPlugin : QuickShapePlugin
{
    public override string Name => "演出：ボスのHPバー（段階つき）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BossHpBarParameter(sharedData);
}

public sealed class BossHpBarParameter : QuickShapeParameter
{
    public BossHpBarParameter() : this(null) { }
    public BossHpBarParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "魔王ゼルガ";
        Count = 3;
        Color1 = Hex(0xE53935);
        Color2 = Hex(0xFFB300);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "ボスの名前", Group: "「ボスのHPバー（段階つき）」の設定"),
        ["Amount"] = new ParamGuide(Label: "残りのHP", Tip: "全体の何%か", Group: "「ボスのHPバー（段階つき）」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "HPの段の数", Min: 1, Max: 10, Group: "「ボスのHPバー（段階つき）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "少ない段の色"),
        ["Color2"] = new ParamGuide(Label: "多い段の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Max(1, Count);
        float hp = Amt(c) * n;
        int stage = Math.Min(n - 1, (int)MathF.Floor(hp - 1e-4f));
        float cur = hp - stage;
        float lag = Math.Clamp(cur + MathF.Max(0f, 0.15f - (t % 1.5f) * 0.2f), 0f, 1f);
        float bw = 1300f * u, bh = 44f * u, x0 = o.X - bw / 2f, y = o.Y;
        float k = DrawKit.EaseOut(Intro(c, 0f, 0.4f));
        Box(dc, b, x0 - 8f * u, y - bh / 2f - 8f * u, x0 + (bw + 8f * u) * k, y + bh / 2f + 8f * u, Rgb(0, 0, 0, 0.7f * a));
        var under = stage > 0 ? Gfx.Mix(Color1, Color2, (stage - 1) / (float)Math.Max(1, n - 1)) : Hex(0x303030);
        Box(dc, b, x0, y - bh / 2f, x0 + bw * k, y + bh / 2f, Col(under, a));
        Box(dc, b, x0, y - bh / 2f, x0 + bw * lag * k, y + bh / 2f, Rgb(1, 1, 1, 0.7f * a));
        var top = Gfx.Mix(Color1, Color2, stage / (float)Math.Max(1, n - 1));
        Box(dc, b, x0, y - bh / 2f, x0 + bw * cur * k, y + bh / 2f, Col(top, a));
        Box(dc, b, x0, y - bh / 2f, x0 + bw * cur * k, y - bh / 2f + bh * 0.3f, Rgb(1, 1, 1, 0.25f * a));
        SayAt(dc, b, Str, 40f * u, new Vector2(x0, y - bh / 2f - 62f * u), Rgb(1, 1, 1, a));
        Say(dc, b, "×" + (stage + 1).ToString(), 44f * u, new Vector2(x0 + bw - 40f * u, y - bh / 2f - 36f * u), C2(a), Rgb(0, 0, 0, 0.7f * a), 4f * u);
    }
}

// =====================================================================
//  経験値バーとレベルアップ
//   経験値がたまり、満タンで「LEVEL UP!」。「数」回だけレベルが上がる
// =====================================================================
public sealed class ExpBarPlugin : QuickShapePlugin
{
    public override string Name => "演出：経験値バーとレベルアップ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ExpBarParameter(sharedData);
}

public sealed class ExpBarParameter : QuickShapeParameter
{
    public ExpBarParameter() : this(null) { }
    public ExpBarParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 2;
        Color1 = Hex(0x29B6F6);
        Color3 = Hex(0xFFD54A);
        Text = "Lv";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "レベルの文字", Tip: "「Lv」など", Group: "「経験値バーとレベルアップ」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "上がるレベルの数", Min: 1, Max: 10, Group: "「経験値バーとレベルアップ」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "バーの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "LEVEL UPの色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float total = Intro(c, 0.2f, 1.2f * Math.Max(1, Count)) * Math.Max(1, Count);
        int ups = Math.Min(Count, (int)MathF.Floor(total));
        float fill = ups >= Count ? 1f : total - ups;
        float bw = 900f * u, bh = 30f * u, x0 = o.X - bw / 2f;
        RBox(dc, b, o, bw + 12f * u, bh + 12f * u, bh, Rgb(0, 0, 0, 0.7f * a));
        RBox(dc, b, new Vector2(x0 + bw * fill / 2f, o.Y), bw * fill, bh, bh / 2f, C1(a));
        Say(dc, b, "EXP", 30f * u, new Vector2(x0 - 60f * u, o.Y), Rgb(1, 1, 1, a));
        Say(dc, b, Str + " " + (1 + ups).ToString(), 44f * u, new Vector2(x0 + bw + 100f * u, o.Y), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.7f * a), 3f * u);
        float since = total - ups;
        if (ups > 0 && since < 0.8f)
        {
            float k = DrawKit.EaseOutBack(Math.Clamp(since / 0.3f, 0f, 1f));
            DrawKit.Glow(dc, o + new Vector2(0, -90f * u), 300f * u, C3(0.5f * a * (1f - since / 0.8f)));
            Say(dc, b, "LEVEL UP!", 90f * u, o + new Vector2(0, -100f * u - since * 40f * u), C3(a), Rgb(0.4f, 0.2f, 0f, a), 6f * u, scale: k);
        }
    }
}

// =====================================================================
//  コマンド入力の表示
//   「文字」に カンマ区切りで「↓,↘,→,P」など。ボタンが順に光って並ぶ
// =====================================================================
public sealed class CommandInputPlugin : QuickShapePlugin
{
    public override string Name => "演出：コマンド入力の表示";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CommandInputParameter(sharedData);
}

public sealed class CommandInputParameter : QuickShapeParameter
{
    public CommandInputParameter() : this(null) { }
    public CommandInputParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "↓,↘,→,P";
        Color1 = Hex(0xFFFFFF);
        Color2 = Hex(0xE53935);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "入力", Tip: "カンマ区切り(例: ↓,↘,→,P)。英字1文字はボタンになります", Group: "「コマンド入力の表示」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "枠の色"),
        ["Color2"] = new ParamGuide(Label: "ボタンの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        if (it.Count == 0) return;
        float s = 110f * u, gap = 30f * u;
        float x0 = o.X - (it.Count * s + (it.Count - 1) * gap) / 2f + s / 2f;
        for (int i = 0; i < it.Count; i++)
        {
            float p = DrawKit.EaseOutBack(Intro(c, 0.1f + i * 0.12f, 0.25f));
            if (p <= 0f) continue;
            var q = new Vector2(x0 + i * (s + gap), o.Y);
            bool btn = it[i].Length == 1 && char.IsLetter(it[i][0]);
            float flash = MathF.Max(0f, 1f - (c.Time - 0.1f - i * 0.12f) * 3f);
            if (btn) { Dot(dc, b, q, s / 2f * p, C2(a)); Ring(dc, b, q, s / 2f * p, Rgb(1, 1, 1, a), 5f * u); }
            else { RBox(dc, b, q, s * p, s * p, 18f * u, Rgb(0.1f, 0.1f, 0.14f, 0.85f * a)); RFrame(dc, b, q, s * p, s * p, 18f * u, C1(a), 4f * u); }
            Say(dc, b, it[i], 64f * u, q, Rgb(1, 1, 1, a), scale: p);
            if (flash > 0f) DrawKit.Glow(dc, q, s, Rgb(1, 1, 1, 0.6f * flash * a));
            if (i < it.Count - 1 && Intro(c, 0.1f + (i + 1) * 0.12f, 0.1f) > 0f) Say(dc, b, "+", 40f * u, q + new Vector2((s + gap) / 2f, 0), Rgb(1, 1, 1, 0.7f * a));
        }
    }
}

// =====================================================================
//  K.O.表示
//   「K.O.」がドンと出て画面がゆれる。「文字」で表示を変えられる
// =====================================================================
public sealed class KoTextPlugin : QuickShapePlugin
{
    public override string Name => "演出：K.O.表示";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new KoTextParameter(sharedData);
}

public sealed class KoTextParameter : QuickShapeParameter
{
    public KoTextParameter() : this(null) { }
    public KoTextParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "K.O.";
        Color1 = Hex(0xFF3D00);
        Color3 = Hex(0xFFEA00);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "表示する文字", Group: "「K.O.表示」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "画面を白く光らせる", Group: "「K.O.表示」の設定"),
        ["Color1"] = new ParamGuide(Label: "文字の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "ふちと光の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float p = Intro(c, 0f, 0.25f);
        float k = p < 1f ? 3f - 2f * DrawKit.EaseOut(p) : 1f;
        float shake = MathF.Max(0f, 1f - c.Time * 2.5f);
        var q = o + new Vector2(MathF.Sin(t * 70f), MathF.Cos(t * 53f)) * 20f * u * shake;
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(1, 1, 1, 0.6f * shake * a));
        for (int i = 0; i < 16; i++)
        {
            float ang = i * MathF.PI * 2f / 16f + 0.1f;
            float len = (500f + 300f * DrawKit.H(Seed, i)) * u * DrawKit.EaseOut(Math.Clamp(c.Time * 3f, 0f, 1f));
            Poly(dc, b, new List<Vector2> { Pol(q, 100f * u, ang - 0.05f), Pol(q, len, ang), Pol(q, 100f * u, ang + 0.05f) }, C3(0.6f * a));
        }
        Say(dc, b, Str, 260f * u, q + new Vector2(10f, 12f) * u, Rgb(0, 0, 0, 0.6f * a), scale: k);
        Say(dc, b, Str, 260f * u, q, C1(a), C3(a), 12f * u, scale: k);
    }
}

// =====================================================================
//  ステージ選択マップ
//   道でつながったステージの丸。「値」までクリア済み、次のステージで旗がゆれる。「数」でステージ数
// =====================================================================
public sealed class StageMapPlugin : QuickShapePlugin
{
    public override string Name => "演出：ステージ選択マップ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StageMapParameter(sharedData);
}

public sealed class StageMapParameter : QuickShapeParameter
{
    public StageMapParameter() : this(null) { }
    public StageMapParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 6;
        Color1 = Hex(0x66BB6A);
        Color2 = Hex(0x8D6E63);
        Color3 = Hex(0xFFD54A);
    }
    protected override double DefAmount => 50;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "クリア済みの割合", Tip: "ここまでクリア、次のステージに旗", Group: "「ステージ選択マップ」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "ステージの数", Min: 2, Max: 12, Group: "「ステージ選択マップ」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "草原の背景", Group: "「ステージ選択マップ」の設定"),
        ["Color1"] = new ParamGuide(Label: "クリア済みの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "次のステージの色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Max(2, Count);
        if (Background) { Box(dc, b, 0, 0, w, h, Col(Hex(0x7CB342), a)); }
        var pts = new List<Vector2>();
        for (int i = 0; i < n; i++) pts.Add(new Vector2(o.X - 650f * u + 1300f * u * i / (n - 1), o.Y + MathF.Sin(i * 1.7f + 0.5f) * 180f * u));
        var path = new List<Vector2>();
        for (int i = 0; i < n - 1; i++) { var mid = (pts[i] + pts[i + 1]) / 2f + new Vector2(0, (i % 2 == 0 ? -80f : 80f) * u); path.AddRange(BubbleKit.Quad(pts[i], mid, pts[i + 1], 16)); }
        Path(dc, b, path, Col(Hex(0xF5E6C8), a), 30f * u);
        int done = (int)MathF.Round(Amt(c) * (n - 1));
        for (int i = 0; i < n; i++)
        {
            bool cleared = i < done, next = i == done;
            Dot(dc, b, pts[i] + new Vector2(0, 10f * u), 56f * u, Rgb(0, 0, 0, 0.3f * a));
            Dot(dc, b, pts[i], 56f * u, cleared ? C1(a) : next ? C3(a) : Rgb(0.6f, 0.6f, 0.6f, a));
            Ring(dc, b, pts[i], 56f * u, Rgb(1, 1, 1, a), 6f * u);
            Say(dc, b, (i + 1).ToString(), 44f * u, pts[i], Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.5f * a), 3f * u);
            if (cleared) Star(dc, b, pts[i] + new Vector2(40f, -40f) * u, 22f * u, 5, 0.45f, 0f, C3(a));
            if (next)
            {
                var pole = pts[i] + new Vector2(0, -60f * u);
                float bob = MathF.Abs(MathF.Sin(t * 4f)) * 20f * u;
                Ln(dc, b, pole - new Vector2(0, bob), pole - new Vector2(0, 120f * u + bob), Rgb(0.3f, 0.3f, 0.3f, a), 6f * u);
                var fl = new List<Vector2>();
                for (int j = 0; j <= 8; j++) fl.Add(pole + new Vector2(j * 9f * u, -120f * u - bob + MathF.Sin(t * 8f + j) * 4f * u));
                for (int j = 8; j >= 0; j--) fl.Add(pole + new Vector2(j * 9f * u, -70f * u - bob + MathF.Sin(t * 8f + j) * 4f * u));
                Poly(dc, b, fl, Rgb(0.9f, 0.2f, 0.2f, a));
            }
        }
    }
}

// =====================================================================
//  カードめくり
//   裏向きのカードがくるっとめくれて表が見える。「文字」が表の文字、「数」で枚数
// =====================================================================
public sealed class CardFlipPlugin : QuickShapePlugin
{
    public override string Name => "演出：カードめくり";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CardFlipParameter(sharedData);
}

public sealed class CardFlipParameter : QuickShapeParameter
{
    public CardFlipParameter() : this(null) { }
    public CardFlipParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "当たり";
        Count = 1;
        Color1 = Hex(0x3949AB);
        Color3 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "表の文字", Tip: "カンマ区切りで1枚ずつ", Group: "「カードめくり」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "カードの枚数", Min: 1, Max: 7, Group: "「カードめくり」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "裏の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "表の枠の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Clamp(Count, 1, 7);
        var it = Items;
        float cw = 300f * u, ch = 440f * u, gap = 50f * u;
        float x0 = o.X - (n * cw + (n - 1) * gap) / 2f + cw / 2f;
        for (int i = 0; i < n; i++)
        {
            float p = Intro(c, 0.4f + i * 0.3f, 0.5f);
            float sx = MathF.Abs(MathF.Cos(p * MathF.PI));
            bool front = p > 0.5f;
            var q = new Vector2(x0 + i * (cw + gap), o.Y - MathF.Sin(p * MathF.PI) * 60f * u);
            RBox(dc, b, q + new Vector2(10f, 14f) * u, cw * sx, ch, 20f * u, Rgb(0, 0, 0, 0.35f * a));
            if (!front)
            {
                RBox(dc, b, q, cw * sx, ch, 20f * u, C1(a));
                RFrame(dc, b, q, (cw - 30f * u) * sx, ch - 30f * u, 14f * u, Rgb(1, 1, 1, 0.6f * a), 4f * u);
                Star(dc, b, q, 70f * u * sx + 0.1f, 4, 0.4f, 0f, Rgb(1, 1, 1, 0.5f * a));
            }
            else
            {
                RBox(dc, b, q, cw * sx, ch, 20f * u, Rgb(1, 1, 1, a));
                RFrame(dc, b, q, (cw - 30f * u) * sx, ch - 30f * u, 14f * u, C3(a), 5f * u);
                string s = it.Count == 0 ? "" : it[i % it.Count];
                if (sx > 0.02f) Say(dc, b, s, 70f * u, q, Rgb(0.15f, 0.15f, 0.2f, a), maxW: cw * 0.8f, scale: sx);
                if (p >= 1f) DrawKit.Glow(dc, q, cw, C3(0.3f * a * (0.6f + 0.4f * MathF.Sin(t * 4f))));
            }
        }
    }
}

// =====================================================================
//  アイテムドロップ
//   宝石やコインが「数」個ポンと飛び出して、はねて地面に落ちる。光の柱つき
// =====================================================================
public sealed class ItemDropPlugin : QuickShapePlugin
{
    public override string Name => "演出：アイテムドロップ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ItemDropParameter(sharedData);
}

public sealed class ItemDropParameter : QuickShapeParameter
{
    public ItemDropParameter() : this(null) { }
    public ItemDropParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 8;
        Color1 = Hex(0xFFD54A);
        Color2 = Hex(0x26C6DA);
        Color3 = Hex(0xFFF59D);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "アイテムの数", Min: 1, Max: 40, Group: "「アイテムドロップ」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "コインの色"),
        ["Color2"] = new ParamGuide(Label: "宝石の色"),
        ["Color3"] = new ParamGuide(Label: "光の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float pillar = MathF.Max(0f, 1f - c.Time * 0.8f);
        if (pillar > 0f) Box(dc, b, o.X - 60f * u * pillar, 0, o.X + 60f * u * pillar, o.Y, C3(0.4f * pillar * a));
        for (int i = 0; i < Count; i++)
        {
            float tt = c.Time - i * 0.05f;
            if (tt <= 0f) continue;
            float vx = (DrawKit.H(Seed, i) - 0.5f) * 900f * u, vy = -(700f + 500f * DrawKit.H(Seed, i, 1)) * u, g = 2600f * u;
            float tl = -2f * vy / g;
            float x = vx * MathF.Min(tt, tl * 1.4f), y;
            if (tt < tl) y = vy * tt + 0.5f * g * tt * tt;
            else { float t2 = (tt - tl) % (tl * 0.4f + 0.001f); float bounce = tt - tl < tl * 0.4f ? 1f : 0f; y = bounce * (vy * 0.3f * t2 + 0.5f * g * t2 * t2); if (y > 0f) y = 0f; }
            var q = o + new Vector2(x, y);
            bool gem = i % 3 == 0;
            float spin = MathF.Abs(MathF.Cos(t * 6f + i));
            if (gem) Poly(dc, b, Pts(q, 26f * u, 30f * u, 0f, -1f, 0.8f, -0.3f, 0f, 1f, -0.8f, -0.3f), C2(a));
            else { Oval(dc, b, q, 24f * u * spin + 2f, 24f * u, C1(a)); Oval(dc, b, q, 14f * u * spin + 1f, 14f * u, Col(Hex(0xFFB300), a)); }
            DrawKit.Twinkle(dc, b, q + new Vector2(14f, -14f) * u, 22f * u * (0.5f + 0.5f * MathF.Sin(t * 7f + i)), 3f * u, Color3, a);
        }
    }
}

// =====================================================================
//  ミニマップ
//   四角いミニマップ。自分の矢印、「数」個の敵の赤い点、ぐるっと回るレーダー線
// =====================================================================
public sealed class MiniMapPlugin : QuickShapePlugin
{
    public override string Name => "演出：ミニマップ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MiniMapParameter(sharedData);
}

public sealed class MiniMapParameter : QuickShapeParameter
{
    public MiniMapParameter() : this(null) { }
    public MiniMapParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 5;
        Color1 = Hex(0x00E676);
        Color2 = Hex(0xFF1744);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "敵の数", Min: 0, Max: 40, Group: "「ミニマップ」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=四角　2=丸", Min: 1, Max: 2, Group: "「ミニマップ」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "レーダーの色"),
        ["Color2"] = new ParamGuide(Label: "敵の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = 420f * u;
        bool round = Variant >= 2;
        if (round) { Dot(dc, b, o, s / 2f, Rgb(0.05f, 0.12f, 0.1f, 0.85f * a)); }
        else RBox(dc, b, o, s, s, 20f * u, Rgb(0.05f, 0.12f, 0.1f, 0.85f * a));
        for (int i = 1; i < 4; i++)
        {
            Ln(dc, b, new Vector2(o.X - s / 2f + s * i / 4f, o.Y - s / 2f), new Vector2(o.X - s / 2f + s * i / 4f, o.Y + s / 2f), C1(0.12f * a), 2f * u);
            Ln(dc, b, new Vector2(o.X - s / 2f, o.Y - s / 2f + s * i / 4f), new Vector2(o.X + s / 2f, o.Y - s / 2f + s * i / 4f), C1(0.12f * a), 2f * u);
        }
        float sw = t * 2f * Spd(c);
        for (int i = 0; i < 20; i++) Poly(dc, b, new List<Vector2> { o, Pol(o, s * 0.5f, sw - i * 0.04f), Pol(o, s * 0.5f, sw - (i + 1) * 0.04f) }, C1(0.25f * (1f - i / 20f) * a));
        for (int i = 0; i < Count; i++)
        {
            var q = o + new Vector2((DrawKit.H(Seed, i) - 0.5f) * s * 0.8f + MathF.Sin(t * 0.7f + i) * 15f * u, (DrawKit.H(Seed, i, 1) - 0.5f) * s * 0.8f + MathF.Cos(t * 0.6f + i) * 15f * u);
            float ang = MathF.Atan2(q.Y - o.Y, q.X - o.X);
            float since = Gfx.Mod(sw - ang, MathF.PI * 2f);
            Dot(dc, b, q, 9f * u, C2(a * (0.35f + 0.65f * MathF.Max(0f, 1f - since / 3f))));
        }
        Poly(dc, b, Pts(o, 22f * u, 22f * u, 0f, -1f, 0.7f, 0.8f, 0f, 0.4f, -0.7f, 0.8f), Rgb(1, 1, 1, a));
        if (round) Ring(dc, b, o, s / 2f, Rgb(1, 1, 1, 0.8f * a), 6f * u); else RFrame(dc, b, o, s, s, 20f * u, Rgb(1, 1, 1, 0.8f * a), 6f * u);
        Say(dc, b, "N", 34f * u, o + new Vector2(0, -s / 2f + 28f * u), Rgb(1, 1, 1, a));
    }
}

// =====================================================================
//  行動順バー
//   「文字」にカンマ区切りの名前。顔アイコンが横に並び、先頭が光ってくり上がる
// =====================================================================
public sealed class TurnOrderPlugin : QuickShapePlugin
{
    public override string Name => "演出：行動順バー";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TurnOrderParameter(sharedData);
}

public sealed class TurnOrderParameter : QuickShapeParameter
{
    public TurnOrderParameter() : this(null) { }
    public TurnOrderParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "勇者,魔法使い,スライム,僧侶,ゴブリン";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "名前", Tip: "カンマ区切りで行動順", Group: "「行動順バー」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        if (it.Count == 0) return;
        float s = 120f * u, gap = 20f * u;
        float shift = Gfx.Mod(t * 0.5f * Spd(c), it.Count);
        int head = (int)MathF.Floor(shift);
        float fr = DrawKit.EaseOut(Math.Clamp((shift - head) * 4f, 0f, 1f));
        int show = Math.Min(6, it.Count);
        float x0 = o.X - (show * (s + gap)) / 2f + s / 2f;
        Say(dc, b, "NEXT", 34f * u, new Vector2(x0 - s, o.Y), Rgb(1, 1, 1, a));
        for (int j = 0; j < show; j++)
        {
            int idx = (head + j) % it.Count;
            var q = new Vector2(x0 + (j - fr + 1f) * (s + gap) - (s + gap), o.Y);
            if (j == 0) q = new Vector2(x0, o.Y);
            float sc = j == 0 ? 1.2f : 1f;
            var col = BubbleKit.Palette(0, idx);
            RBox(dc, b, q, s * sc, s * sc, 18f * u, Col(col, a));
            Dot(dc, b, q - new Vector2(0, 10f * u), s * 0.22f * sc, Rgb(1, 1, 1, 0.9f * a));
            Oval(dc, b, q + new Vector2(0, s * 0.3f * sc), s * 0.34f * sc, s * 0.18f * sc, Rgb(1, 1, 1, 0.9f * a));
            if (j == 0) { RFrame(dc, b, q, s * sc + 10f * u, s * sc + 10f * u, 22f * u, C3(a * (0.6f + 0.4f * MathF.Sin(t * 6f))), 6f * u); }
            Say(dc, b, it[idx], 26f * u, q + new Vector2(0, s * 0.5f * sc + 26f * u), Rgb(1, 1, 1, a), maxW: s + gap);
        }
    }
}

// =====================================================================
//  ローディング画面
//   黒い画面に、くるくる回る印と「NOW LOADING...」、下にゲームのヒント(文字)
// =====================================================================
public sealed class LoadingScreenPlugin : QuickShapePlugin
{
    public override string Name => "演出：ローディング画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new LoadingScreenParameter(sharedData);
}

public sealed class LoadingScreenParameter : QuickShapeParameter
{
    public LoadingScreenParameter() : this(null) { }
    public LoadingScreenParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ヒント：ボスは炎に弱いぞ！";
        Color1 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "ヒントの文字", Group: "「ローディング画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "黒い背景", Group: "「ローディング画面」の設定"),
        ["Color1"] = new ParamGuide(Label: "文字と印の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(0.02f, 0.02f, 0.04f, a));
        var sp = new Vector2(w - 220f * U0(c), h - 140f * U0(c));
        for (int i = 0; i < 8; i++)
        {
            float k = Gfx.Mod(i / 8f - t * 1.5f * Spd(c), 1f);
            Dot(dc, b, Pol(sp, 40f * U0(c), i * MathF.PI / 4f), 9f * U0(c), C1(a * (0.2f + 0.8f * k)));
        }
        string dots = new string('.', 1 + (int)(t * 3f) % 3);
        SayAt(dc, b, "NOW LOADING" + dots, 44f * U0(c), new Vector2(w - 700f * U0(c), h - 170f * U0(c)), C1(a));
        Say(dc, b, Str, 40f * U0(c), new Vector2(w / 2f, h / 2f), Rgb(0.8f, 0.8f, 0.85f, a), maxW: w * 0.8f);
        float bar = Gfx.Mod(t * 0.3f, 1f);
        Box(dc, b, w * 0.1f, h - 60f * U0(c), w * 0.1f + w * 0.8f * bar, h - 52f * U0(c), C1(0.6f * a));
    }
}

// =====================================================================
//  ゲームオーバー・コンティニュー
//   画面が暗くなり「GAME OVER」。下に CONTINUE? と数が減っていくカウント(「数」から)
// =====================================================================
public sealed class GameOverPlugin : QuickShapePlugin
{
    public override string Name => "演出：ゲームオーバー・コンティニュー";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new GameOverParameter(sharedData);
}

public sealed class GameOverParameter : QuickShapeParameter
{
    public GameOverParameter() : this(null) { }
    public GameOverParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "GAME OVER";
        Count = 9;
        Color1 = Hex(0xD50000);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "表示する文字", Group: "「ゲームオーバー・コンティニュー」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "コンティニューの秒数", Min: 1, Max: 30, Group: "「ゲームオーバー・コンティニュー」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "画面を暗くする", Group: "「ゲームオーバー・コンティニュー」の設定"),
        ["Color1"] = new ParamGuide(Label: "文字の色"),
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
        float dark = Intro(c, 0f, 1f);
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(0, 0, 0, 0.75f * dark * a));
        float p = Intro(c, 0.5f, 1.2f);
        Say(dc, b, Str, 180f * u, o + new Vector2(0, -120f * u + (1f - DrawKit.EaseOut(p)) * -200f * u), C1(a * p), Rgb(0, 0, 0, a * p), 6f * u);
        float q = Intro(c, 1.8f, 0.4f);
        if (q <= 0f) return;
        int left = Math.Max(0, Count - (int)MathF.Floor(MathF.Max(0f, c.Time - 1.8f)));
        Say(dc, b, "CONTINUE?", 70f * u, o + new Vector2(0, 80f * u), Rgb(1, 1, 1, a * q));
        float tick = Gfx.Mod(c.Time - 1.8f, 1f);
        Say(dc, b, left.ToString(), 140f * u, o + new Vector2(0, 220f * u), Rgb(1, 1, 1, a * q), scale: 1.3f - 0.3f * DrawKit.EaseOut(Math.Clamp(tick * 3f, 0f, 1f)));
    }
}

// =====================================================================
//  リザルト画面（星3つ）
//   「CLEAR!」と星3つ。「数」個の星が順にドンと光る。下にスコア(文字)
// =====================================================================
public sealed class ResultStarsPlugin : QuickShapePlugin
{
    public override string Name => "演出：リザルト画面（星3つ）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ResultStarsParameter(sharedData);
}

public sealed class ResultStarsParameter : QuickShapeParameter
{
    public ResultStarsParameter() : this(null) { }
    public ResultStarsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "SCORE 123,450";
        Count = 3;
        Color1 = Hex(0xFFD54A);
        Color2 = Hex(0x455A64);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "スコアの文字", Group: "「リザルト画面（星3つ）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "光る星の数", Min: 0, Max: 3, Group: "「リザルト画面（星3つ）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "画面を暗くする", Group: "「リザルト画面（星3つ）」の設定"),
        ["Color1"] = new ParamGuide(Label: "星の色"),
        ["Color2"] = new ParamGuide(Label: "光っていない星の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(0, 0, 0, 0.5f * a));
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.4f));
        Say(dc, b, "CLEAR!", 150f * u, o + new Vector2(0, -260f * u), C1(a), Rgb(0.5f, 0.3f, 0f, a), 7f * u, scale: k);
        for (int i = 0; i < 3; i++)
        {
            var q = o + new Vector2((i - 1) * 260f * u, i == 1 ? -30f * u : 20f * u);
            float r = (i == 1 ? 120f : 100f) * u;
            Star(dc, b, q, r, 5, 0.45f, 0f, C2(a));
            if (i >= Count) continue;
            float p = Intro(c, 0.6f + i * 0.4f, 0.3f);
            if (p <= 0f) continue;
            float s = p < 1f ? 2.2f - 1.2f * DrawKit.EaseOut(p) : 1f;
            DrawKit.Glow(dc, q, r * 2f, C1(0.4f * a * MathF.Max(0f, 1.5f - (c.Time - 0.6f - i * 0.4f))));
            Star(dc, b, q, r * s, 5, 0.45f, 0f, C1(a * p));
        }
        float q2 = Intro(c, 1.9f, 0.4f);
        Say(dc, b, Str, 64f * u, o + new Vector2(0, 220f * u), Rgb(1, 1, 1, a * q2), Rgb(0, 0, 0, 0.6f * a * q2), 3f * u);
    }
}

// =====================================================================
//  サイコロ
//   サイコロが転がって止まる。「値」で出る目(1〜6 → 0〜100を6等分)。「数」で個数
// =====================================================================
public sealed class DicePlugin : QuickShapePlugin
{
    public override string Name => "演出：サイコロ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DiceParameter(sharedData);
}

public sealed class DiceParameter : QuickShapeParameter
{
    public DiceParameter() : this(null) { }
    public DiceParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 1;
        Color1 = Hex(0xFFFFFF);
        Color2 = Hex(0xD32F2F);
    }
    protected override double DefAmount => 100;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "出る目", Tip: "0〜100を6等分(100=6の目)", Group: "「サイコロ」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "サイコロの数", Min: 1, Max: 5, Group: "「サイコロ」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "サイコロの色"),
        ["Color2"] = new ParamGuide(Label: "1の目の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        int n = Math.Clamp(Count, 1, 5);
        float roll = Intro(c, 0f, 1.2f);
        for (int d = 0; d < n; d++)
        {
            int face = roll < 1f ? 1 + (int)(DrawKit.H(Seed + d, (int)(c.Time * 14f)) * 6f) % 6 : Math.Clamp((int)MathF.Ceiling(Amt(c) * 6f - (d * 0f)), 1, 6);
            if (d > 0 && roll >= 1f) face = 1 + (int)(DrawKit.H(Seed, d, 9) * 6f) % 6;
            float s = 220f * u;
            var q = o + new Vector2((d - (n - 1) / 2f) * s * 1.4f, -MathF.Abs(MathF.Sin(roll * MathF.PI * 3f)) * (1f - roll) * 220f * u);
            float rot = (1f - DrawKit.EaseOut(roll)) * 8f + d;
            var sq = DrawKit.Place(ShapeGeom.RoundRect(0, 0, s, s, s * 0.18f), q, rot);
            Poly(dc, b, DrawKit.Place(ShapeGeom.RoundRect(0, 0, s, s, s * 0.18f), q + new Vector2(10f, 14f) * u, rot), Rgb(0, 0, 0, 0.3f * a));
            Poly(dc, b, sq, C1(a));
            float[][] pips =
            {
                new float[] { 0, 0 }, new float[] { -1, -1, 1, 1 }, new float[] { -1, -1, 0, 0, 1, 1 }, new float[] { -1, -1, 1, -1, -1, 1, 1, 1 },
                new float[] { -1, -1, 1, -1, 0, 0, -1, 1, 1, 1 }, new float[] { -1, -1, 1, -1, -1, 0, 1, 0, -1, 1, 1, 1 },
            };
            var pp = pips[face - 1];
            for (int i = 0; i < pp.Length; i += 2)
                Dot(dc, b, DrawKit.Rot(q + new Vector2(pp[i], pp[i + 1]) * s * 0.27f, q, rot), (face == 1 ? 0.15f : 0.09f) * s, face == 1 ? C2(a) : Rgb(0.1f, 0.1f, 0.12f, a));
        }
    }
}

// =====================================================================
//  トランプを配る
//   山札から「数」枚のカードが手前に飛んで扇形に並ぶ。最後に表になる
// =====================================================================
public sealed class DealCardsPlugin : QuickShapePlugin
{
    public override string Name => "演出：トランプを配る";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DealCardsParameter(sharedData);
}

public sealed class DealCardsParameter : QuickShapeParameter
{
    public DealCardsParameter() : this(null) { }
    public DealCardsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 5;
        Text = "A♠,K♥,Q♦,J♣,10♠";
        Color1 = Hex(0xC62828);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "カード", Tip: "カンマ区切り(例: A♠,K♥)", Group: "「トランプを配る」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "配る枚数", Min: 1, Max: 13, Group: "「トランプを配る」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "裏の色"),
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
        int n = Math.Clamp(Count, 1, 13);
        var it = Items;
        float cw = 170f * u, ch = 250f * u;
        var deck = o + new Vector2(0, -300f * u);
        for (int i = 0; i < 3; i++) RBox(dc, b, deck + new Vector2(i * 3f, -i * 3f) * u, cw, ch, 14f * u, C1(a));
        for (int i = 0; i < n; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.2f + i * 0.15f, 0.4f));
            if (p <= 0f) continue;
            float ang = (i - (n - 1) / 2f) * 0.12f;
            var dst = o + new Vector2((i - (n - 1) / 2f) * 130f * u, 120f * u + MathF.Abs(i - (n - 1) / 2f) * 12f * u);
            var q = Vector2.Lerp(deck, dst, p);
            float rot = ang * p + (1f - p) * 2f;
            float flip = Intro(c, 0.4f + n * 0.15f + i * 0.08f, 0.25f);
            float sx = MathF.Abs(MathF.Cos(flip * MathF.PI));
            var card = DrawKit.Place(ShapeGeom.RoundRect(0, 0, cw * MathF.Max(0.02f, sx), ch, 14f * u), q, rot);
            bool face = flip > 0.5f;
            Poly(dc, b, card, face ? Rgb(1, 1, 1, a) : C1(a));
            Outline(dc, b, card, Rgb(0, 0, 0, 0.3f * a), 2f * u);
            if (face && it.Count > 0)
            {
                string s = it[i % it.Count];
                bool red = s.Contains('♥') || s.Contains('♦');
                Say(dc, b, s, 54f * u, q, red ? Rgb(0.85f, 0.1f, 0.1f, a) : Rgb(0.1f, 0.1f, 0.12f, a), scale: sx);
            }
        }
    }
}

// =====================================================================
//  連打メーター
//   「連打！」の文字とボタンがへこみ、メーターがガタガタ上がる。「値」が最後の量
// =====================================================================
public sealed class MashMeterPlugin : QuickShapePlugin
{
    public override string Name => "演出：連打メーター";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MashMeterParameter(sharedData);
}

public sealed class MashMeterParameter : QuickShapeParameter
{
    public MashMeterParameter() : this(null) { }
    public MashMeterParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "連打！";
        Color1 = Hex(0xFF5722);
        Color3 = Hex(0xFFEB3B);
    }
    protected override double DefAmount => 90;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Group: "「連打メーター」の設定"),
        ["Amount"] = new ParamGuide(Label: "最後のメーターの量", Group: "「連打メーター」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "メーターの上の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Label: "メーターの下と文字の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float lv = Amt(c) * DrawKit.EaseOut(Intro(c, 0f, 2.5f)) + MathF.Sin(t * 30f) * 0.02f;
        lv = Math.Clamp(lv, 0f, 1f);
        float bw = 120f * u, bh = 600f * u;
        var mc = o + new Vector2(-200f * u, 0);
        RBox(dc, b, mc, bw + 16f * u, bh + 16f * u, 20f * u, Rgb(0, 0, 0, 0.7f * a));
        Shade(dc, new Vector2(0, mc.Y + bh / 2f), new Vector2(0, mc.Y - bh / 2f), C3(a), C1(a), mc.X - bw / 2f, mc.Y + bh / 2f - bh * lv, mc.X + bw / 2f, mc.Y + bh / 2f);
        var bc = o + new Vector2(200f * u, 60f * u);
        bool down = (int)(t * 16f) % 2 == 0;
        Oval(dc, b, bc + new Vector2(0, 30f * u), 120f * u, 50f * u, Rgb(0.3f, 0.3f, 0.3f, a));
        Oval(dc, b, bc + new Vector2(0, down ? 18f * u : 0f), 110f * u, 50f * u, Rgb(0.9f, 0.15f, 0.15f, a));
        float sh = MathF.Sin(t * 60f) * 8f * u;
        Say(dc, b, Str, 100f * u, bc + new Vector2(sh, -170f * u), C3(a), Rgb(0.4f, 0.1f, 0f, a), 6f * u);
    }
}

// =====================================================================
//  必殺技のカットイン帯
//   斜めの帯が画面を横切り、技の名前(文字)が流れる。後ろに集中線
// =====================================================================
public sealed class SpecialCutinPlugin : QuickShapePlugin
{
    public override string Name => "演出：必殺技のカットイン帯";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpecialCutinParameter(sharedData);
}

public sealed class SpecialCutinParameter : QuickShapeParameter
{
    public SpecialCutinParameter() : this(null) { }
    public SpecialCutinParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "超必殺 烈風斬";
        Color1 = Hex(0x1A237E);
        Color2 = Hex(0xFF1744);
        Color3 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "技の名前", Group: "「必殺技のカットイン帯」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "帯の色"),
        ["Color2"] = new ParamGuide(Label: "ふちの色"),
        ["Color3"] = new ParamGuide(Label: "文字と線の色"),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["PosX"] = new ParamGuide(Hidden: true),
        ["PosY"] = new ParamGuide(Hidden: true),
        ["Scale"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float p = DrawKit.EaseOut(Intro(c, 0f, 0.3f));
        float slope = -0.18f, bh = 280f * U0(c);
        float cy = h * c.F(PosY) / 100f;
        var band = new List<Vector2> { new(-w * (1f - p), cy - bh / 2f - w * slope / 2f), new(w * p + w * 0.2f, cy - bh / 2f + w * slope / 2f), new(w * p + w * 0.2f, cy + bh / 2f + w * slope / 2f), new(-w * (1f - p), cy + bh / 2f - w * slope / 2f) };
        for (int i = 0; i < 40; i++)
        {
            float y = cy + (DrawKit.H(Seed, i) - 0.5f) * h * 1.2f;
            float x = Gfx.Mod(DrawKit.H(Seed, i, 1) * w * 2f - t * 3000f * U0(c), w * 1.4f) - w * 0.2f;
            Ln(dc, b, new Vector2(x, y), new Vector2(x + 300f * U0(c), y + 300f * U0(c) * slope), C3(0.3f * a), 3f * U0(c));
        }
        Poly(dc, b, band, C1(0.92f * a));
        var edge = new List<Vector2> { band[0] + new Vector2(0, -14f * U0(c)), band[1] + new Vector2(0, -14f * U0(c)), band[1], band[0] };
        Poly(dc, b, edge, C2(a));
        var edge2 = new List<Vector2> { band[3], band[2], band[2] + new Vector2(0, 14f * U0(c)), band[3] + new Vector2(0, 14f * U0(c)) };
        Poly(dc, b, edge2, C2(a));
        float tx = w / 2f + (0.5f - Gfx.Mod(c.Time * 0.15f, 1f)) * 200f * U0(c);
        Say(dc, b, Str, 140f * U0(c), new Vector2(tx, cy), C3(a * p), C2(a * p), 8f * U0(c));
    }
}

// =====================================================================
//  状態異常アイコン
//   毒・まひ・ねむり・やけど・こおり のアイコンが並んでふわふわ。「文字」にカンマ区切りで選ぶ
// =====================================================================
public sealed class StatusIconsPlugin : QuickShapePlugin
{
    public override string Name => "演出：状態異常アイコン";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StatusIconsParameter(sharedData);
}

public sealed class StatusIconsParameter : QuickShapeParameter
{
    public StatusIconsParameter() : this(null) { }
    public StatusIconsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "毒,まひ,ねむり,やけど,こおり";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "状態", Tip: "カンマ区切り(毒・まひ・ねむり・やけど・こおり)", Group: "「状態異常アイコン」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Hidden: true),
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
        var it = Items;
        float s = 110f * u, gap = 30f * u;
        float x0 = o.X - (it.Count * (s + gap) - gap) / 2f + s / 2f;
        for (int i = 0; i < it.Count; i++)
        {
            float p = DrawKit.EaseOutBack(Intro(c, i * 0.1f, 0.3f));
            var q = new Vector2(x0 + i * (s + gap), o.Y + MathF.Sin(t * 3f + i) * 8f * u);
            string k = it[i];
            WpfColor col = k.Contains("毒") ? Hex(0x8E24AA) : k.Contains("まひ") || k.Contains("麻") ? Hex(0xFDD835) : k.Contains("ねむ") || k.Contains("眠") ? Hex(0x5C6BC0) : k.Contains("やけ") || k.Contains("火") ? Hex(0xF4511E) : k.Contains("こお") || k.Contains("氷") ? Hex(0x4FC3F7) : Hex(0x78909C);
            Dot(dc, b, q, s / 2f * p, Col(col, a));
            Ring(dc, b, q, s / 2f * p, Rgb(1, 1, 1, a), 5f * u);
            if (k.Contains("毒")) { for (int j = 0; j < 3; j++) Dot(dc, b, q + new Vector2((j - 1) * 18f * u, -Gfx.Mod(t * 30f + j * 15f, 40f) * u + 15f * u), 8f * u * p, Rgb(0.8f, 1f, 0.5f, a)); }
            else if (k.Contains("まひ") || k.Contains("麻")) Path(dc, b, Pts(q, s * p, s * p, -0.12f, -0.3f, 0.08f, -0.05f, -0.08f, 0.02f, 0.12f, 0.3f), Rgb(1, 1, 1, a), 7f * u);
            else if (k.Contains("ねむ") || k.Contains("眠")) Say(dc, b, "Zz", 48f * u, q + new Vector2(0, MathF.Sin(t * 2f) * 4f * u), Rgb(1, 1, 1, a), scale: p);
            else if (k.Contains("やけ") || k.Contains("火")) Poly(dc, b, Pts(q, s * p, s * p * (1f + 0.08f * MathF.Sin(t * 12f)), 0f, -0.32f, 0.2f, 0.05f, 0.1f, 0.25f, -0.1f, 0.25f, -0.2f, 0.05f), Rgb(1, 0.9f, 0.4f, a));
            else if (k.Contains("こお") || k.Contains("氷")) { for (int j = 0; j < 3; j++) { float an = j * MathF.PI / 3f; Ln(dc, b, Pol(q, s * 0.3f * p, an), Pol(q, s * 0.3f * p, an + MathF.PI), Rgb(1, 1, 1, a), 6f * u); } }
            else Say(dc, b, k.Length > 0 ? k[..1] : "", 50f * u, q, Rgb(1, 1, 1, a), scale: p);
            Say(dc, b, k, 28f * u, q + new Vector2(0, s * 0.5f + 26f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.7f * a), 3f * u);
        }
    }
}
