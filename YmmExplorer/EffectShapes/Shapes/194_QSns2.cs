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
//  スーパーチャット風の色帯
//   名前と金額とメッセージの帯。金額に合わせて帯の色が変わる。1行目が名前、2行目が金額、3行目からがメッセージ
// =====================================================================
public sealed class SuperChatPlugin : QuickShapePlugin
{
    public override string Name => "演出：スーパーチャット風の色帯";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SuperChatParameter(sharedData);
}

public sealed class SuperChatParameter : QuickShapeParameter
{
    public SuperChatParameter() : this(null) { }
    public SuperChatParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ゆっくりさん\n5000\nいつも楽しく見ています！";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "内容", Tip: "1行目が名前、2行目が金額(数字)、3行目からがメッセージ", Group: "「スーパーチャット風の色帯」の設定"),
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
        var ln = Str.Split('\n');
        string name = ln.Length > 0 ? ln[0] : "", msg = string.Join("\n", ln.Skip(2));
        long.TryParse(new string((ln.Length > 1 ? ln[1] : "").Where(char.IsDigit).ToArray()), out long yen);
        (uint top, uint bot) = yen >= 10000 ? (0xD00000u, 0xE62117u) : yen >= 5000 ? (0xC2185Bu, 0xE91E63u) : yen >= 2000 ? (0xE65100u, 0xF57C00u)
            : yen >= 1000 ? (0xFFB300u, 0xFFCA28u) : yen >= 500 ? (0x00BFA5u, 0x1DE9B6u) : yen >= 200 ? (0x00B8D4u, 0x00E5FFu) : (0x1565C0u, 0x1E88E5u);
        float bw = 900f * u;
        var ms = Measure(msg, 34f * u, false, bw - 60f * u);
        float hh = 110f * u, bh = msg.Length > 0 ? ms.Height + 40f * u : 0f;
        float p = DrawKit.EaseOutBack(Intro(c, 0f, 0.4f));
        var q = o + new Vector2((1f - p) * 300f * u, 0);
        var tl = q - new Vector2(bw / 2f, (hh + bh) / 2f);
        RBox(dc, b, new Vector2(q.X, tl.Y + (hh + bh) / 2f), bw, hh + bh, 14f * u, Col(Hex(bot), a));
        RBox(dc, b, new Vector2(q.X, tl.Y + hh / 2f), bw, hh, 14f * u, Col(Hex(top), a));
        Box(dc, b, tl.X, tl.Y + hh - 14f * u, tl.X + bw, tl.Y + hh, Col(Hex(top), a));
        Dot(dc, b, tl + new Vector2(60f * u, hh / 2f), 34f * u, Rgb(1, 1, 1, 0.9f * a));
        Say(dc, b, name.Length > 0 ? name[..1] : "", 30f * u, tl + new Vector2(60f * u, hh / 2f), Col(Hex(top), a));
        SayAt(dc, b, name, 32f * u, tl + new Vector2(110f * u, 14f * u), Rgb(1, 1, 1, 0.85f * a), false);
        SayAt(dc, b, "¥" + yen.ToString("N0"), 40f * u, tl + new Vector2(110f * u, 52f * u), Rgb(1, 1, 1, a));
        if (bh > 0f) TextKit.Draw(dc, b, ms, tl + new Vector2(30f * u, hh + 20f * u), Rgb(1, 1, 1, a));
    }
}

// =====================================================================
//  チャット欄
//   画面の右側に、コメントが下から積み上がっていく。1行に「名前:コメント」
// =====================================================================
public sealed class ChatColumnPlugin : QuickShapePlugin
{
    public override string Name => "演出：チャット欄";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ChatColumnParameter(sharedData);
}

public sealed class ChatColumnParameter : QuickShapeParameter
{
    public ChatColumnParameter() : this(null) { }
    public ChatColumnParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "たろう:こんにちは！\nはなこ:待ってました\nゆっくり:わこつ\nけん:今日は何するの？\nみさき:888888\nたろう:おもしろい\nはなこ:www";
        Color1 = Hex(0x1F1F23);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "コメント", Tip: "1行に「名前:コメント」", Group: "「チャット欄」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "欄の背景", Group: "「チャット欄」の設定"),
        ["Color1"] = new ParamGuide(Label: "欄の色"),
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
        var rows = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        float s = U0(c) * c.F(Scale) / 100f;
        float cw = 520f * s, x0 = w - cw - 30f * s, y0 = 30f * s, y1 = h - 30f * s;
        if (Background) RBox(dc, b, new Vector2(x0 + cw / 2f, (y0 + y1) / 2f), cw, y1 - y0, 14f * s, C1(0.85f * a));
        SayAt(dc, b, "チャット", 30f * s, new Vector2(x0 + 20f * s, y0 + 14f * s), Rgb(1, 1, 1, a));
        int shown = Math.Min(rows.Count, (int)(c.Time * 1.2f * Spd(c)) + 1);
        float lh = 64f * s;
        float y = y1 - 20f * s;
        dc.PushAxisAlignedClip(new RawRectF(x0, y0 + 60f * s, x0 + cw, y1), AntialiasMode.PerPrimitive);
        for (int i = shown - 1; i >= 0; i--)
        {
            var f = rows[i].Split(new[] { ':', '：' }, 2);
            string nm = f.Length > 1 ? f[0] : "", msg = f.Length > 1 ? f[1] : f[0];
            var ms = Measure(msg, 28f * s, false, cw - 100f * s);
            float hh = MathF.Max(lh, ms.Height + 40f * s);
            float slide = i == shown - 1 ? (1f - DrawKit.EaseOut(Math.Clamp(Gfx.Mod(c.Time * 1.2f * Spd(c), 1f) * 4f, 0f, 1f))) * hh : 0f;
            y -= hh;
            float yy = y + slide;
            Dot(dc, b, new Vector2(x0 + 36f * s, yy + 26f * s), 18f * s, Col(BubbleKit.Palette(0, nm.Sum(ch => (int)ch) % 10), a));
            SayAt(dc, b, nm, 24f * s, new Vector2(x0 + 66f * s, yy + 6f * s), Rgb(0.7f, 0.7f, 0.75f, a), false);
            TextKit.Draw(dc, b, ms, new Vector2(x0 + 66f * s, yy + 34f * s), Rgb(1, 1, 1, a));
        }
        dc.PopAxisAlignedClip();
    }
}

// =====================================================================
//  再生回数・高評価のカウンター
//   再生回数と高評価の数字が、回るように増えていく。1行目が再生回数、2行目が高評価
// =====================================================================
public sealed class ViewCounterPlugin : QuickShapePlugin
{
    public override string Name => "演出：再生回数・高評価のカウンター";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ViewCounterParameter(sharedData);
}

public sealed class ViewCounterParameter : QuickShapeParameter
{
    public ViewCounterParameter() : this(null) { }
    public ViewCounterParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "1234567\n45678";
        Color1 = Hex(0xFF0000);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "数", Tip: "1行目が再生回数、2行目が高評価の数", Group: "「再生回数・高評価のカウンター」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "アイコンの色"),
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
        var ln = Str.Split('\n');
        long v0 = 0, v1 = 0;
        if (ln.Length > 0) long.TryParse(new string(ln[0].Where(char.IsDigit).ToArray()), out v0);
        if (ln.Length > 1) long.TryParse(new string(ln[1].Where(char.IsDigit).ToArray()), out v1);
        float p = DrawKit.EaseOut(Intro(c, 0.2f, 2.5f));
        for (int k = 0; k < 2; k++)
        {
            var q = o + new Vector2(0, (k == 0 ? -1f : 1f) * 90f * u);
            var ic = q - new Vector2(330f * u, 0);
            if (k == 0) { RBox(dc, b, ic, 110f * u, 76f * u, 20f * u, C1(a)); Poly(dc, b, Pts(ic, u, u, -14f, -20f, 22f, 0f, -14f, 20f), Rgb(1, 1, 1, a)); }
            else { Box(dc, b, ic.X - 40f * u, ic.Y - 6f * u, ic.X - 22f * u, ic.Y + 36f * u, Rgb(1, 1, 1, a)); Poly(dc, b, Pts(ic, u, u, -16f, -6f, 0f, -40f, 12f, -36f, 6f, -10f, 40f, -10f, 36f, 36f, -16f, 36f), Rgb(1, 1, 1, a)); }
            long cur = (long)((k == 0 ? v0 : v1) * p);
            string sv = cur.ToString("N0");
            float roll = p < 1f ? MathF.Sin(t * 40f) * 6f * u : 0f;
            SayAt(dc, b, sv, 90f * u, new Vector2(ic.X + 90f * u, q.Y - 60f * u + roll), Rgb(1, 1, 1, a));
            SayAt(dc, b, k == 0 ? "回視聴" : "高評価", 30f * u, new Vector2(ic.X + 100f * u + Measure(sv, 90f * u).Width, q.Y - 10f * u), Rgb(0.8f, 0.8f, 0.85f, a), false);
        }
    }
}

// =====================================================================
//  ショート動画風のUI
//   縦画面の右側に、いいね・コメント・共有ボタン。下にアカウント名と説明。1行目が名前、2行目から説明
// =====================================================================
public sealed class ShortVideoUiPlugin : QuickShapePlugin
{
    public override string Name => "演出：ショート動画風のUI";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ShortVideoUiParameter(sharedData);
}

public sealed class ShortVideoUiParameter : QuickShapeParameter
{
    public ShortVideoUiParameter() : this(null) { }
    public ShortVideoUiParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "@yukkuri_channel\n今日の豆知識 #解説 #ゆっくり";
        Color1 = Hex(0xFF2D55);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "内容", Tip: "1行目がアカウント名、2行目からが説明", Group: "「ショート動画風のUI」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "いいねの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        float s = U0(c) * c.F(Scale) / 100f;
        var ln = Str.Split('\n');
        string acct = ln.Length > 0 ? ln[0] : "", cap = string.Join("\n", ln.Skip(1));
        Shade(dc, new Vector2(0, h * 0.6f), new Vector2(0, h), Rgb(0, 0, 0, 0f), Rgb(0, 0, 0, 0.6f * a), 0, h * 0.6f, w, h);
        float bx = w - 90f * s;
        string[] labels = { "12.3万", "2,345", "共有", "保存" };
        for (int i = 0; i < 4; i++)
        {
            var q = new Vector2(bx, h * 0.45f + i * 140f * s);
            float pop = i == 0 ? 1f + 0.25f * MathF.Max(0f, MathF.Sin(Gfx.Mod(t, 2f) * MathF.PI * 2f)) * (Gfx.Mod(t, 2f) < 0.5f ? 1f : 0f) : 1f;
            if (i == 0) Heart(dc, b, q, 34f * s * pop, 0f, C1(a));
            else if (i == 1) { Oval(dc, b, q, 34f * s, 28f * s, Rgb(1, 1, 1, a)); Poly(dc, b, Pts(q, s, s, -10f, 20f, -24f, 38f, 6f, 24f), Rgb(1, 1, 1, a)); }
            else if (i == 2) Poly(dc, b, Pts(q, s, s, -30f, 20f, -30f, 0f, 0f, -14f, 0f, -30f, 34f, 0f, 0f, 30f, 0f, 14f), Rgb(1, 1, 1, a));
            else Poly(dc, b, Pts(q, s, s, -24f, -30f, 24f, -30f, 24f, 32f, 0f, 14f, -24f, 32f), Rgb(1, 1, 1, a));
            Say(dc, b, labels[i], 24f * s, q + new Vector2(0, 58f * s), Rgb(1, 1, 1, a));
        }
        SayAt(dc, b, acct, 36f * s, new Vector2(40f * s, h - 220f * s), Rgb(1, 1, 1, a));
        SayAt(dc, b, cap, 30f * s, new Vector2(40f * s, h - 160f * s), Rgb(1, 1, 1, 0.9f * a), false, w - 220f * s);
        Box(dc, b, 0, h - 8f * s, w * Gfx.Mod(c.Time / 15f, 1f), h, Rgb(1, 1, 1, 0.8f * a));
    }
}

// =====================================================================
//  ワイプ枠（小窓）
//   画面の角に置く、丸か角丸の枠。話す人の顔などを入れる用(中は透明)。名前の札つき
// =====================================================================
public sealed class WipeFramePlugin : QuickShapePlugin
{
    public override string Name => "演出：ワイプ枠（小窓）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WipeFrameParameter(sharedData);
}

public sealed class WipeFrameParameter : QuickShapeParameter
{
    public WipeFrameParameter() : this(null) { }
    public WipeFrameParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ゲスト";
        Color1 = Hex(0xFFFFFF);
        Color2 = Hex(0xFF7043);
        Background = false;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "名前の札", Group: "「ワイプ枠（小窓）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=丸　2=角丸", Min: 1, Max: 2, Group: "「ワイプ枠（小窓）」の設定"),
        ["Background"] = new ParamGuide(Label: "まわりを少し暗くする", Group: "「ワイプ枠（小窓）」の設定"),
        ["Color1"] = new ParamGuide(Label: "枠の色"),
        ["Color2"] = new ParamGuide(Label: "札の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = 220f * u;
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.35f));
        List<Vector2> hole = Variant >= 2 ? ShapeGeom.RoundRect(o.X, o.Y, R * 2.4f * k, R * 1.8f * k, 40f * u) : DrawKit.EllipsePts(o, R * k, R * k, 80);
        if (Background) { b.Color = Rgb(0, 0, 0, 0.35f * a); DrawKit.FillWithHole(dc, b, DrawKit.RectPts(-2, -2, w + 4, h + 4), hole); }
        Outline(dc, b, hole, Rgb(0, 0, 0, 0.3f * a), 20f * u);
        Outline(dc, b, hole, C1(a), 12f * u);
        if (Str.Length > 0)
        {
            var ts = Measure(Str, 36f * u);
            var tp = new Vector2(o.X, o.Y + (Variant >= 2 ? R * 0.9f : R) * k + 10f * u);
            RBox(dc, b, tp, ts.Width + 50f * u, ts.Height + 20f * u, 30f * u, C2(a));
            Say(dc, b, Str, 36f * u, tp, Rgb(1, 1, 1, a));
        }
    }
}

// =====================================================================
//  字幕の背景帯（自動の長さ）
//   文字の長さに合わせて伸びる、半透明の帯と字幕。「文字」に字幕
// =====================================================================
public sealed class SubtitleBandPlugin : QuickShapePlugin
{
    public override string Name => "演出：字幕の背景帯（自動の長さ）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SubtitleBandParameter(sharedData);
}

public sealed class SubtitleBandParameter : QuickShapeParameter
{
    public SubtitleBandParameter() : this(null) { }
    public SubtitleBandParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ここに字幕が入ります";
        Color1 = Hex(0x000000);
        Color2 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "字幕", Group: "「字幕の背景帯（自動の長さ）」の設定"),
        ["Amount"] = new ParamGuide(Label: "帯の濃さ", Group: "「字幕の背景帯（自動の長さ）」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "帯の色"),
        ["Color2"] = new ParamGuide(Label: "文字の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        if (Str.Length == 0) return;
        var ts = Measure(Str, 56f * u, true, 1600f * u);
        float p = DrawKit.EaseOut(Intro(c, 0f, 0.25f));
        float bw = (ts.Width + 80f * u) * p, bh = ts.Height + 36f * u;
        RBox(dc, b, o, bw, bh, 12f * u, C1((0.2f + 0.8f * Amt(c)) * a));
        if (p > 0.6f) TextKit.Draw(dc, b, ts, o - new Vector2(ts.Width, ts.Height) / 2f, C2(a * (p - 0.6f) / 0.4f));
    }
}

// =====================================================================
//  目次・チャプター一覧
//   章の一覧で、今の項目だけが光る。「値」で今の章。1行に1つ
// =====================================================================
public sealed class ChapterListPlugin : QuickShapePlugin
{
    public override string Name => "演出：目次・チャプター一覧";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ChapterListParameter(sharedData);
}

public sealed class ChapterListParameter : QuickShapeParameter
{
    public ChapterListParameter() : this(null) { }
    public ChapterListParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "はじめに\n準備するもの\n作り方\nまとめ";
        Color1 = Hex(0xFF7043);
    }
    protected override double DefAmount => 30;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "章", Tip: "1行に1つ", Group: "「目次・チャプター一覧」の設定"),
        ["Amount"] = new ParamGuide(Label: "今の章", Tip: "0〜100を章の数で等分", Group: "「目次・チャプター一覧」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "今の章の色"),
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
        var items = Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        int n = items.Count;
        if (n == 0) return;
        int cur = Math.Clamp((int)(Amt(c) * n), 0, n - 1);
        float rh = 100f * u, bw = 900f * u, y0 = o.Y - n * rh / 2f;
        RBox(dc, b, o, bw + 60f * u, n * rh + 60f * u, 20f * u, Rgb(0.1f, 0.1f, 0.13f, 0.85f * a));
        for (int i = 0; i < n; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, i * 0.1f, 0.3f));
            var q = new Vector2(o.X, y0 + i * rh + rh / 2f);
            bool on = i == cur;
            if (on) RBox(dc, b, q, bw, rh - 12f * u, 12f * u, C1((0.8f + 0.2f * MathF.Sin(t * 4f)) * a));
            Say(dc, b, (i + 1).ToString("00"), 38f * u, q - new Vector2(bw / 2f - 60f * u, 0), on ? Rgb(1, 1, 1, a) : Rgb(0.6f, 0.6f, 0.65f, a * p));
            SayAt(dc, b, items[i], 40f * u, new Vector2(q.X - bw / 2f + 130f * u + (1f - p) * 30f * u, q.Y - 28f * u), on ? Rgb(1, 1, 1, a) : Rgb(0.85f, 0.85f, 0.88f, a * p));
            if (i < cur) Path(dc, b, Pts(q + new Vector2(bw / 2f - 50f * u, 0), u, u, -14f, 0f, -4f, 12f, 16f, -12f), Rgb(0.5f, 0.9f, 0.5f, a), 6f * u);
        }
    }
}

// =====================================================================
//  概要欄・リンク誘導
//   「詳しくは概要欄へ↓」の文字と、下を指す矢印がぴょこぴょこ動く
// =====================================================================
public sealed class DescriptionArrowPlugin : QuickShapePlugin
{
    public override string Name => "演出：概要欄・リンク誘導";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DescriptionArrowParameter(sharedData);
}

public sealed class DescriptionArrowParameter : QuickShapeParameter
{
    public DescriptionArrowParameter() : this(null) { }
    public DescriptionArrowParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "詳しくは概要欄へ";
        Color1 = Hex(0xFFEB3B);
        Color2 = Hex(0x000000);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Group: "「概要欄・リンク誘導」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=下向きの矢印　2=右向きの矢印", Min: 1, Max: 2, Group: "「概要欄・リンク誘導」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "文字と矢印の色"),
        ["Color2"] = new ParamGuide(Label: "ふちの色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.35f));
        Say(dc, b, Str, 80f * u, o, C1(a), C2(a), 6f * u, scale: k);
        bool right = Variant >= 2;
        float bob = MathF.Abs(MathF.Sin(t * 5f * Spd(c))) * 30f * u;
        var tw = Measure(Str, 80f * u).Width;
        var ap = right ? o + new Vector2(tw / 2f + 90f * u + bob, 0) : o + new Vector2(0, 120f * u + bob);
        var dir = right ? new Vector2(1, 0) : new Vector2(0, 1);
        var nrm = new Vector2(-dir.Y, dir.X);
        var arrow = new List<Vector2> { ap - dir * 50f * u + nrm * 18f * u, ap + nrm * 18f * u, ap + nrm * 45f * u, ap + dir * 50f * u, ap - nrm * 45f * u, ap - nrm * 18f * u, ap - dir * 50f * u - nrm * 18f * u };
        Outline(dc, b, arrow, C2(a), 10f * u);
        Poly(dc, b, arrow, C1(a));
    }
}

// =====================================================================
//  スマホの着信画面
//   名前と「着信中…」、応答・拒否のボタン。アイコンの輪が広がる。1行目が名前
// =====================================================================
public sealed class IncomingCallPlugin : QuickShapePlugin
{
    public override string Name => "演出：スマホの着信画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new IncomingCallParameter(sharedData);
}

public sealed class IncomingCallParameter : QuickShapeParameter
{
    public IncomingCallParameter() : this(null) { }
    public IncomingCallParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "お母さん\n携帯";
        Color1 = Hex(0x263238);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "名前", Tip: "1行目が名前、2行目が小さな説明", Group: "「スマホの着信画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "画面の色"),
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
        var ln = Str.Split('\n');
        string nm = ln.Length > 0 ? ln[0] : "", sub = ln.Length > 1 ? ln[1] : "";
        float pw = 460f * u, ph = 900f * u;
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.35f));
        var q = o + new Vector2(0, (1f - k) * 300f * u);
        float shake = MathF.Sin(t * 40f) * 4f * u * (Gfx.Mod(t, 1.5f) < 0.6f ? 1f : 0f);
        q.X += shake;
        RBox(dc, b, q, pw + 24f * u, ph + 24f * u, 60f * u, Rgb(0.05f, 0.05f, 0.06f, a));
        RBox(dc, b, q, pw, ph, 46f * u, C1(a));
        var ic = q + new Vector2(0, -180f * u);
        for (int i = 0; i < 3; i++) { float f = Gfx.Mod(t * 0.7f + i / 3f, 1f); Ring(dc, b, ic, (80f + 110f * f) * u, Rgb(1, 1, 1, 0.4f * (1f - f) * a), 3f * u); }
        Dot(dc, b, ic, 80f * u, Rgb(0.6f, 0.65f, 0.7f, a));
        Say(dc, b, nm.Length > 0 ? nm[..1] : "", 70f * u, ic, Rgb(1, 1, 1, a));
        Say(dc, b, nm, 52f * u, q + new Vector2(0, -30f * u), Rgb(1, 1, 1, a), maxW: pw - 40f * u);
        Say(dc, b, sub + "  着信中…", 28f * u, q + new Vector2(0, 30f * u), Rgb(0.75f, 0.78f, 0.82f, a));
        for (int e = -1; e <= 1; e += 2)
        {
            var bp = q + new Vector2(e * 120f * u, ph / 2f - 150f * u);
            Dot(dc, b, bp, 56f * u, e < 0 ? Rgb(0.95f, 0.25f, 0.25f, a) : Rgb(0.2f, 0.8f, 0.35f, a));
            var hs = DrawKit.Place(Pts(Vector2.Zero, u, u, -28f, -6f, -18f, -16f, 18f, -16f, 28f, -6f, 22f, 4f, 10f, -2f, -10f, -2f, -22f, 4f), bp, e < 0 ? MathF.PI * 0.75f : 0f);
            Poly(dc, b, hs, Rgb(1, 1, 1, a));
            Say(dc, b, e < 0 ? "拒否" : "応答", 26f * u, bp + new Vector2(0, 85f * u), Rgb(1, 1, 1, a));
        }
    }
}

// =====================================================================
//  地図アプリの現在地
//   道の地図の上で、青い点と精度の輪、進む向きの扇。点はゆっくり道を進む
// =====================================================================
public sealed class MapCurrentLocationPlugin : QuickShapePlugin
{
    public override string Name => "演出：地図アプリの現在地";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MapCurrentLocationParameter(sharedData);
}

public sealed class MapCurrentLocationParameter : QuickShapeParameter
{
    public MapCurrentLocationParameter() : this(null) { }
    public MapCurrentLocationParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x1E88E5);
        Color2 = Hex(0xEDEBE6);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "地図の背景", Group: "「地図アプリの現在地」の設定"),
        ["Color1"] = new ParamGuide(Label: "現在地の色"),
        ["Color2"] = new ParamGuide(Label: "地図の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float s = U0(c);
        if (Background)
        {
            Box(dc, b, 0, 0, w, h, C2(a));
            for (int i = 0; i < 6; i++) Box(dc, b, DrawKit.H(Seed, i) * w, DrawKit.H(Seed, i, 1) * h, DrawKit.H(Seed, i) * w + (150f + 200f * DrawKit.H(Seed, i, 2)) * s, DrawKit.H(Seed, i, 1) * h + (100f + 150f * DrawKit.H(Seed, i, 3)) * s, Rgb(0.78f, 0.9f, 0.75f, a));
            for (int i = 0; i < 5; i++) { float y = h * (0.15f + i * 0.2f); Ln(dc, b, new Vector2(0, y), new Vector2(w, y + 40f * s), Rgb(1, 1, 1, a), 26f * s); }
            for (int i = 0; i < 6; i++) { float x = w * (0.1f + i * 0.18f); Ln(dc, b, new Vector2(x, 0), new Vector2(x - 60f * s, h), Rgb(1, 1, 1, a), 22f * s); }
            Ln(dc, b, new Vector2(0, h * 0.55f), new Vector2(w, h * 0.45f), Rgb(1f, 0.85f, 0.4f, a), 34f * s);
        }
        var q = o + new Vector2(MathF.Sin(t * 0.2f) * 40f * u, 0);
        float acc = (90f + 20f * MathF.Sin(t * 2f)) * u;
        Dot(dc, b, q, acc, C1(0.15f * a));
        Ring(dc, b, q, acc, C1(0.3f * a), 2f * u);
        float hd = -MathF.PI / 2f + MathF.Sin(t * 0.5f) * 0.4f;
        var fan = new List<Vector2> { q };
        fan.AddRange(Gfx.ArcPts(q, 110f * u, hd - 0.45f, hd + 0.45f, 12));
        using (var g = Gfx.Radial(dc, q, 110f * u, (0f, C1(0.5f * a)), (1f, C1(0f))))
            Gfx.FillPoly(dc, g, fan);
        Dot(dc, b, q, 24f * u, Rgb(1, 1, 1, a));
        Dot(dc, b, q, 17f * u, C1(a));
    }
}

// =====================================================================
//  カレンダーをめくる
//   日めくりカレンダーの日付がパラパラ進んで止まる。「文字」に「始めの日,終わりの日」(例: 1,15)
// =====================================================================
public sealed class CalendarFlipPlugin : QuickShapePlugin
{
    public override string Name => "演出：カレンダーをめくる";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CalendarFlipParameter(sharedData);
}

public sealed class CalendarFlipParameter : QuickShapeParameter
{
    public CalendarFlipParameter() : this(null) { }
    public CalendarFlipParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "1,15";
        Color1 = Hex(0xE53935);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "日付", Tip: "カンマ区切りで「始めの日,終わりの日」", Group: "「カレンダーをめくる」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "上の帯の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var it = Items;
        int d0 = it.Count > 0 && int.TryParse(it[0], out var a0) ? a0 : 1;
        int d1 = it.Count > 1 && int.TryParse(it[1], out var a1) ? a1 : d0 + 10;
        float dur = 0.2f + 0.1f * MathF.Abs(d1 - d0) / MathF.Max(0.1f, Spd(c));
        float p = Math.Clamp(c.Time / dur, 0f, 1f);
        float fv = d0 + (d1 - d0) * DrawKit.EaseOut(p);
        int day = (int)MathF.Floor(fv);
        float flip = fv - day;
        if (p >= 1f) { day = d1; flip = 0f; }
        float cw = 460f * u, ch = 520f * u;
        RBox(dc, b, o + new Vector2(10f, 14f) * u, cw, ch, 20f * u, Rgb(0, 0, 0, 0.3f * a));
        RBox(dc, b, o, cw, ch, 20f * u, Rgb(1, 1, 1, a));
        Box(dc, b, o.X - cw / 2f, o.Y - ch / 2f + 20f * u, o.X + cw / 2f, o.Y - ch / 2f + 110f * u, C1(a));
        RBox(dc, b, o - new Vector2(0, ch / 2f - 20f * u), cw, 40f * u, 20f * u, C1(a));
        for (int e = -1; e <= 1; e += 2) Dot(dc, b, o + new Vector2(e * 120f * u, -ch / 2f + 10f * u), 14f * u, Rgb(0.3f, 0.3f, 0.3f, a));
        Say(dc, b, "DAY", 44f * u, o - new Vector2(0, ch / 2f - 70f * u), Rgb(1, 1, 1, a));
        Say(dc, b, day.ToString(), 260f * u, o + new Vector2(0, 50f * u), Rgb(0.15f, 0.15f, 0.18f, a));
        if (flip > 0.02f && p < 1f)
        {
            float top = o.Y - ch / 2f + 110f * u, bot = o.Y + ch / 2f;
            float y = top + (bot - top) * (1f - flip);
            Poly(dc, b, new List<Vector2> { new(o.X - cw / 2f, top), new(o.X + cw / 2f, top), new(o.X + cw / 2f + 20f * u * flip, y), new(o.X - cw / 2f - 20f * u * flip, y) }, Rgb(0.95f, 0.95f, 0.95f, a));
            Ln(dc, b, new Vector2(o.X - cw / 2f, y), new Vector2(o.X + cw / 2f, y), Rgb(0, 0, 0, 0.2f * a), 3f * u);
        }
    }
}
