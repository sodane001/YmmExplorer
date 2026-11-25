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
//  動画プレーヤーの画面
//   再生ボタン・シークバー・時間の表示。「値」が再生位置、文字がタイトル
// =====================================================================
public sealed class VideoPlayerUiPlugin : QuickShapePlugin
{
    public override string Name => "演出：動画プレーヤーの画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new VideoPlayerUiParameter(sharedData);
}

public sealed class VideoPlayerUiParameter : QuickShapeParameter
{
    public VideoPlayerUiParameter() : this(null) { }
    public VideoPlayerUiParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "【解説】今日のテーマ";
        Color1 = Hex(0xFF0000);
        Background = false;
    }
    protected override double DefAmount => 35;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "タイトル", Group: "「動画プレーヤーの画面」の設定"),
        ["Amount"] = new ParamGuide(Label: "再生位置", Group: "「動画プレーヤーの画面」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=再生ボタン　2=一時停止", Min: 1, Max: 2, Group: "「動画プレーヤーの画面」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "バーの色"),
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
        float s = U0(c);
        float pos = Math.Clamp(Amt(c) + c.Time * 0.01f * Spd(c), 0f, 1f);
        Shade(dc, new Vector2(0, h - 200f * s), new Vector2(0, h), Rgb(0, 0, 0, 0f), Rgb(0, 0, 0, 0.75f * a), 0, h - 200f * s, w, h);
        Shade(dc, new Vector2(0, 0), new Vector2(0, 160f * s), Rgb(0, 0, 0, 0.6f * a), Rgb(0, 0, 0, 0f), 0, 0, w, 160f * s);
        SayAt(dc, b, Str, 40f * s, new Vector2(40f * s, 30f * s), Rgb(1, 1, 1, a));
        float by = h - 110f * s;
        Box(dc, b, 40f * s, by - 3f * s, w - 40f * s, by + 3f * s, Rgb(1, 1, 1, 0.3f * a));
        Box(dc, b, 40f * s, by - 3f * s, 40f * s + (w - 80f * s) * MathF.Min(1f, pos + 0.15f), by + 3f * s, Rgb(1, 1, 1, 0.5f * a));
        Box(dc, b, 40f * s, by - 3f * s, 40f * s + (w - 80f * s) * pos, by + 3f * s, C1(a));
        Dot(dc, b, new Vector2(40f * s + (w - 80f * s) * pos, by), 12f * s, C1(a));
        var pc = new Vector2(80f * s, h - 50f * s);
        if (Variant >= 2) { Box(dc, b, pc.X - 14f * s, pc.Y - 18f * s, pc.X - 4f * s, pc.Y + 18f * s, Rgb(1, 1, 1, a)); Box(dc, b, pc.X + 4f * s, pc.Y - 18f * s, pc.X + 14f * s, pc.Y + 18f * s, Rgb(1, 1, 1, a)); }
        else Poly(dc, b, Pts(pc, s, s, -14f, -20f, 18f, 0f, -14f, 20f), Rgb(1, 1, 1, a));
        Poly(dc, b, Pts(pc + new Vector2(80f * s, 0), s, s, -12f, -18f, 10f, 0f, -12f, 18f), Rgb(1, 1, 1, a));
        Box(dc, b, pc.X + 92f * s, pc.Y - 18f * s, pc.X + 98f * s, pc.Y + 18f * s, Rgb(1, 1, 1, a));
        int total = 600, cur = (int)(total * pos);
        SayAt(dc, b, $"{cur / 60}:{cur % 60:00} / {total / 60}:{total % 60:00}", 30f * s, new Vector2(pc.X + 140f * s, pc.Y - 20f * s), Rgb(1, 1, 1, a), false);
        Frame(dc, b, w - 90f * s, h - 68f * s, w - 50f * s, h - 34f * s, Rgb(1, 1, 1, a), 4f * s);
    }
}

// =====================================================================
//  メールの受信画面
//   受信ボックスに新しいメールがスッと入ってくる。1行目が差出人、2行目が件名
// =====================================================================
public sealed class MailReceivedPlugin : QuickShapePlugin
{
    public override string Name => "演出：メールの受信画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MailReceivedParameter(sharedData);
}

public sealed class MailReceivedParameter : QuickShapeParameter
{
    public MailReceivedParameter() : this(null) { }
    public MailReceivedParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "山田さん\n明日の打ち合わせについて";
        Color1 = Hex(0x1E88E5);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "メール", Tip: "1行目が差出人、2行目が件名", Group: "「メールの受信画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "アプリの色"),
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
        string from = ln.Length > 0 ? ln[0] : "", subj = ln.Length > 1 ? ln[1] : "";
        float bw = 900f * u, rh = 120f * u;
        RBox(dc, b, o, bw + 20f * u, rh * 4.4f, 24f * u, Rgb(0.97f, 0.97f, 0.98f, a));
        Box(dc, b, o.X - bw / 2f - 10f * u, o.Y - rh * 2.2f, o.X + bw / 2f + 10f * u, o.Y - rh * 2.2f + 80f * u, C1(a));
        SayAt(dc, b, "受信トレイ", 38f * u, new Vector2(o.X - bw / 2f + 20f * u, o.Y - rh * 2.2f + 16f * u), Rgb(1, 1, 1, a));
        float p = DrawKit.EaseOutBack(Intro(c, 0.3f, 0.5f));
        for (int i = 0; i < 3; i++)
        {
            float y = o.Y - rh * 2.2f + 80f * u + rh * (i + p) + 10f * u;
            if (y + rh > o.Y + rh * 2.2f) continue;
            Box(dc, b, o.X - bw / 2f + 10f * u, y + rh - 2f * u, o.X + bw / 2f - 10f * u, y + rh, Rgb(0, 0, 0, 0.08f * a));
            Dot(dc, b, new Vector2(o.X - bw / 2f + 60f * u, y + rh / 2f), 34f * u, Rgb(0.75f, 0.77f, 0.8f, a));
            Box(dc, b, o.X - bw / 2f + 120f * u, y + 30f * u, o.X + 100f * u - i * 60f * u, y + 46f * u, Rgb(0.8f, 0.8f, 0.82f, a));
            Box(dc, b, o.X - bw / 2f + 120f * u, y + 66f * u, o.X + 200f * u - i * 40f * u, y + 80f * u, Rgb(0.88f, 0.88f, 0.9f, a));
        }
        float ny = o.Y - rh * 2.2f + 90f * u;
        float sl = (1f - p) * -rh;
        var nb = new Vector2(o.X, ny + rh / 2f + sl);
        Box(dc, b, o.X - bw / 2f + 10f * u, ny + sl, o.X + bw / 2f - 10f * u, ny + rh + sl, Gfx.C4(Gfx.Mix(Color1, System.Windows.Media.Colors.White, 0.85f), a * Math.Clamp(p, 0f, 1f)));
        Dot(dc, b, new Vector2(o.X - bw / 2f + 60f * u, nb.Y), 34f * u, C1(a));
        Say(dc, b, from.Length > 0 ? from[..1] : "", 32f * u, new Vector2(o.X - bw / 2f + 60f * u, nb.Y), Rgb(1, 1, 1, a));
        SayAt(dc, b, from, 34f * u, new Vector2(o.X - bw / 2f + 120f * u, ny + 14f * u + sl), Rgb(0.1f, 0.1f, 0.12f, a));
        SayAt(dc, b, subj, 28f * u, new Vector2(o.X - bw / 2f + 120f * u, ny + 62f * u + sl), Rgb(0.35f, 0.35f, 0.4f, a), false, bw - 200f * u);
        Dot(dc, b, new Vector2(o.X + bw / 2f - 50f * u, nb.Y), 10f * u, C1(a));
    }
}

// =====================================================================
//  ダイアログ（OK・キャンセル）
//   パソコン風の確認ダイアログ。1行目がタイトル、2行目から本文。「値」が50より上でOKが押される
// =====================================================================
public sealed class DialogBoxPlugin : QuickShapePlugin
{
    public override string Name => "演出：ダイアログ（OK・キャンセル）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DialogBoxParameter(sharedData);
}

public sealed class DialogBoxParameter : QuickShapeParameter
{
    public DialogBoxParameter() : this(null) { }
    public DialogBoxParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "確認\n本当に削除しますか？";
        Color1 = Hex(0x1976D2);
    }
    protected override double DefAmount => 0;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "内容", Tip: "1行目がタイトル、2行目から本文", Group: "「ダイアログ（OK・キャンセル）」の設定"),
        ["Amount"] = new ParamGuide(Label: "OKを押す", Tip: "50より大きいとOKが押された見た目", Group: "「ダイアログ（OK・キャンセル）」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "OKボタンの色"),
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
        string title = ln.Length > 0 ? ln[0] : "", body = string.Join("\n", ln.Skip(1));
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.3f));
        float bw = 760f * u * k, bh = 360f * u * k;
        if (bw < 2f) return;
        RBox(dc, b, o + new Vector2(8f, 12f) * u, bw, bh, 14f * u, Rgb(0, 0, 0, 0.3f * a));
        RBox(dc, b, o, bw, bh, 14f * u, Rgb(0.97f, 0.97f, 0.98f, a));
        Box(dc, b, o.X - bw / 2f, o.Y - bh / 2f + 10f * u, o.X + bw / 2f, o.Y - bh / 2f + 64f * u, Rgb(0.9f, 0.91f, 0.93f, a));
        SayAt(dc, b, title, 30f * u, new Vector2(o.X - bw / 2f + 24f * u, o.Y - bh / 2f + 18f * u), Rgb(0.1f, 0.1f, 0.12f, a));
        Say(dc, b, "×", 34f * u, new Vector2(o.X + bw / 2f - 34f * u, o.Y - bh / 2f + 38f * u), Rgb(0.3f, 0.3f, 0.35f, a));
        Dot(dc, b, new Vector2(o.X - bw / 2f + 80f * u, o.Y - 20f * u), 40f * u, Rgb(1f, 0.75f, 0.1f, a));
        Say(dc, b, "!", 50f * u, new Vector2(o.X - bw / 2f + 80f * u, o.Y - 20f * u), Rgb(1, 1, 1, a));
        Say(dc, b, body, 36f * u, new Vector2(o.X + 40f * u, o.Y - 20f * u), Rgb(0.1f, 0.1f, 0.12f, a), maxW: bw - 220f * u);
        bool pressed = Amt(c) > 0.5f;
        float bwid = 170f * u, bht = 60f * u;
        var okp = new Vector2(o.X + bw / 2f - 300f * u, o.Y + bh / 2f - 60f * u);
        var cnp = new Vector2(o.X + bw / 2f - 110f * u, o.Y + bh / 2f - 60f * u);
        RBox(dc, b, okp, bwid * (pressed ? 0.94f : 1f), bht, 8f * u, pressed ? Gfx.C4(Gfx.Mix(Color1, Hex(0x000000), 0.25f), a) : C1(a));
        Say(dc, b, "OK", 30f * u, okp, Rgb(1, 1, 1, a));
        RBox(dc, b, cnp, bwid, bht, 8f * u, Rgb(0.85f, 0.86f, 0.88f, a));
        Say(dc, b, "キャンセル", 28f * u, cnp, Rgb(0.1f, 0.1f, 0.12f, a));
    }
}

// =====================================================================
//  パソコンのウィンドウ
//   タイトルバーと3つのボタンつきのウィンドウ枠。中は透明なので画像を重ねられる
// =====================================================================
public sealed class PcWindowPlugin : QuickShapePlugin
{
    public override string Name => "演出：パソコンのウィンドウ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PcWindowParameter(sharedData);
}

public sealed class PcWindowParameter : QuickShapeParameter
{
    public PcWindowParameter() : this(null) { }
    public PcWindowParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "メモ帳";
        Color1 = Hex(0x2B2B2B);
        Background = false;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "ウィンドウの名前", Group: "「パソコンのウィンドウ」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=Windows風　2=Mac風", Min: 1, Max: 2, Group: "「パソコンのウィンドウ」の設定"),
        ["Background"] = new ParamGuide(Label: "中を白く塗る", Group: "「パソコンのウィンドウ」の設定"),
        ["Color1"] = new ParamGuide(Label: "タイトルバーの色"),
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
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.3f));
        float bw = 1300f * u * k, bh = 760f * u * k, tb = 56f * u;
        if (bw < 2f) return;
        var tl = o - new Vector2(bw, bh) / 2f;
        Box(dc, b, tl.X + 10f * u, tl.Y + 14f * u, tl.X + bw + 10f * u, tl.Y + bh + 14f * u, Rgb(0, 0, 0, 0.25f * a));
        if (Background) Box(dc, b, tl.X, tl.Y, tl.X + bw, tl.Y + bh, Rgb(1, 1, 1, a));
        Box(dc, b, tl.X, tl.Y, tl.X + bw, tl.Y + tb, C1(a));
        Frame(dc, b, tl.X, tl.Y, tl.X + bw, tl.Y + bh, C1(a), 3f * u);
        SayAt(dc, b, Str, 28f * u, new Vector2(tl.X + 20f * u, tl.Y + 10f * u), Rgb(1, 1, 1, a), false);
        if (Variant >= 2)
        {
            for (int i = 0; i < 3; i++) Dot(dc, b, new Vector2(tl.X + bw - 40f * u - i * 36f * u, tl.Y + tb / 2f), 11f * u, i == 0 ? Rgb(1f, 0.37f, 0.34f, a) : i == 1 ? Rgb(1f, 0.74f, 0.18f, a) : Rgb(0.16f, 0.79f, 0.25f, a));
            return;
        }
        float bx = tl.X + bw - 60f * u;
        Ln(dc, b, new Vector2(bx - 12f * u, tl.Y + 16f * u), new Vector2(bx + 12f * u, tl.Y + 40f * u), Rgb(1, 1, 1, a), 3f * u);
        Ln(dc, b, new Vector2(bx - 12f * u, tl.Y + 40f * u), new Vector2(bx + 12f * u, tl.Y + 16f * u), Rgb(1, 1, 1, a), 3f * u);
        Frame(dc, b, bx - 80f * u, tl.Y + 18f * u, bx - 56f * u, tl.Y + 38f * u, Rgb(1, 1, 1, a), 3f * u);
        Ln(dc, b, new Vector2(bx - 150f * u, tl.Y + 28f * u), new Vector2(bx - 126f * u, tl.Y + 28f * u), Rgb(1, 1, 1, a), 3f * u);
    }
}

// =====================================================================
//  クリックの波紋とカーソル
//   矢印カーソルが動いてきてクリック、波紋が広がる。「数」回クリックする
// =====================================================================
public sealed class ClickRipplePlugin : QuickShapePlugin
{
    public override string Name => "演出：クリックの波紋とカーソル";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ClickRippleParameter(sharedData);
}

public sealed class ClickRippleParameter : QuickShapeParameter
{
    public ClickRippleParameter() : this(null) { }
    public ClickRippleParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 1;
        Color1 = Hex(0xFFEB3B);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "クリックの回数", Min: 1, Max: 10, Group: "「クリックの波紋とカーソル」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "波紋の色"),
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
        float move = DrawKit.EaseOut(Intro(c, 0f, 0.6f));
        var start = o + new Vector2(300f, 220f) * u;
        var cur = Vector2.Lerp(start, o, move);
        for (int i = 0; i < Count; i++)
        {
            float tt = c.Time - 0.6f - i * 0.45f;
            if (tt < 0f || tt > 0.8f) continue;
            float f = tt / 0.8f;
            Ring(dc, b, o, (20f + 120f * DrawKit.EaseOut(f)) * u, C1((1f - f) * a), 8f * u * (1f - f) + 1f);
            Dot(dc, b, o, 40f * u * (1f - f), C1(0.4f * (1f - f) * a));
        }
        float press = 0f;
        for (int i = 0; i < Count; i++) { float tt = c.Time - 0.6f - i * 0.45f; if (tt >= 0f && tt < 0.12f) press = 1f; }
        float sc = 1f - press * 0.12f;
        var arrow = Pts(cur, 60f * u * sc, 60f * u * sc, 0f, 0f, 0f, 1.6f, 0.4f, 1.25f, 0.7f, 1.85f, 0.95f, 1.75f, 0.68f, 1.15f, 1.15f, 1.15f);
        Outline(dc, b, arrow, Rgb(0, 0, 0, a), 8f * u);
        Poly(dc, b, arrow, Rgb(1, 1, 1, a));
    }
}

// =====================================================================
//  コードが打ち込まれる画面
//   エディタ風の画面に、文字が1文字ずつ打ち込まれる。行番号つき。「速さ」で打つ速さ
// =====================================================================
public sealed class CodeTypingPlugin : QuickShapePlugin
{
    public override string Name => "演出：コードが打ち込まれる画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CodeTypingParameter(sharedData);
}

public sealed class CodeTypingParameter : QuickShapeParameter
{
    public CodeTypingParameter() : this(null) { }
    public CodeTypingParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "using System;\n\nclass Hello\n{\n    static void Main()\n    {\n        Console.WriteLine(\"Hello!\");\n    }\n}";
        Color1 = Hex(0x1E1E1E);
        Color2 = Hex(0x9CDCFE);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "コード", Group: "「コードが打ち込まれる画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "エディタの背景", Group: "「コードが打ち込まれる画面」の設定"),
        ["Color1"] = new ParamGuide(Label: "背景の色"),
        ["Color2"] = new ParamGuide(Label: "文字の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        string all = Str;
        string shown = TextKit.Typed(all, c.Time, 25f * Spd(c));
        var lines = shown.Split('\n');
        float bw = 1300f * u, bh = 760f * u, fs = 34f * u, lh = fs * 1.4f;
        var tl = o - new Vector2(bw, bh) / 2f;
        if (Background) RBox(dc, b, o, bw, bh, 14f * u, C1(a));
        Box(dc, b, tl.X, tl.Y, tl.X + bw, tl.Y + 50f * u, Rgb(0.2f, 0.2f, 0.22f, a));
        for (int i = 0; i < 3; i++) Dot(dc, b, new Vector2(tl.X + 30f * u + i * 30f * u, tl.Y + 25f * u), 9f * u, i == 0 ? Rgb(1f, 0.37f, 0.34f, a) : i == 1 ? Rgb(1f, 0.74f, 0.18f, a) : Rgb(0.16f, 0.79f, 0.25f, a));
        int first = Math.Max(0, lines.Length - (int)((bh - 90f * u) / lh));
        float y = tl.Y + 70f * u;
        for (int i = first; i < lines.Length; i++)
        {
            SayAt(dc, b, (i + 1).ToString(), fs * 0.85f, new Vector2(tl.X + 20f * u, y), Rgb(0.5f, 0.5f, 0.55f, a), false);
            string l = lines[i];
            var col = l.TrimStart().StartsWith("//") ? Rgb(0.42f, 0.6f, 0.33f, a) : l.Contains('"') ? Rgb(0.81f, 0.57f, 0.47f, a) : C2(a);
            float lw = l.Trim().Length > 0 ? SayAt(dc, b, l.Replace("\t", "    "), fs, new Vector2(tl.X + 90f * u, y), col, false) : 0f;
            if (i == lines.Length - 1 && (int)(t * 2.5f) % 2 == 0)
            {
                float lead = 0f;
                for (int k = 0; k < l.Length && l[k] == ' '; k++) lead += fs * 0.3f;
                float cx = tl.X + 90f * u + (l.Trim().Length > 0 ? lw + lead : lead);
                Box(dc, b, cx + 4f * u, y + 4f * u, cx + 8f * u, y + lh - 6f * u, Rgb(1, 1, 1, a));
            }
            y += lh;
        }
    }
}

// =====================================================================
//  ダウンロードの進み具合
//   ファイル名・進み具合のバー・%・残り時間。「値」で最後の%(初めから伸びる)
// =====================================================================
public sealed class DownloadProgressPlugin : QuickShapePlugin
{
    public override string Name => "演出：ダウンロードの進み具合";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DownloadProgressParameter(sharedData);
}

public sealed class DownloadProgressParameter : QuickShapeParameter
{
    public DownloadProgressParameter() : this(null) { }
    public DownloadProgressParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "動画素材.zip";
        Color1 = Hex(0x43A047);
    }
    protected override double DefAmount => 100;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "ファイル名", Group: "「ダウンロードの進み具合」の設定"),
        ["Amount"] = new ParamGuide(Label: "最後の進み具合", Group: "「ダウンロードの進み具合」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "バーの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float p = Amt(c) * DrawKit.EaseOut(Intro(c, 0.2f, 3f / MathF.Max(0.1f, Spd(c))));
        float bw = 900f * u, bh = 220f * u;
        RBox(dc, b, o, bw, bh, 20f * u, Rgb(0.98f, 0.98f, 0.99f, a));
        var ic = o + new Vector2(-bw / 2f + 90f * u, -10f * u);
        RBox(dc, b, ic, 70f * u, 90f * u, 8f * u, C1(a));
        Poly(dc, b, Pts(ic, u, u, -14f, -10f, 14f, -10f, 14f, 6f, 26f, 6f, 0f, 30f, -26f, 6f, -14f, 6f), Rgb(1, 1, 1, a));
        SayAt(dc, b, Str, 36f * u, new Vector2(o.X - bw / 2f + 160f * u, o.Y - 80f * u), Rgb(0.1f, 0.1f, 0.12f, a));
        float x0 = o.X - bw / 2f + 160f * u, x1 = o.X + bw / 2f - 40f * u;
        RBox(dc, b, new Vector2((x0 + x1) / 2f, o.Y + 10f * u), x1 - x0, 22f * u, 11f * u, Rgb(0.88f, 0.89f, 0.9f, a));
        RBox(dc, b, new Vector2(x0 + (x1 - x0) * p / 2f, o.Y + 10f * u), (x1 - x0) * p, 22f * u, 11f * u, C1(a));
        bool done = p >= 0.999f;
        string info = done ? "完了" : $"{p * 100f:0}%   残り {MathF.Max(1f, (1f - p) * 30f):0} 秒";
        SayAt(dc, b, info, 30f * u, new Vector2(x0, o.Y + 40f * u), Rgb(0.35f, 0.35f, 0.4f, a), false);
        if (done) Path(dc, b, Pts(new Vector2(x1 - 30f * u, o.Y - 60f * u), u, u, -18f, 0f, -4f, 14f, 20f, -14f), C1(a), 8f * u);
    }
}

// =====================================================================
//  投票・アンケートの結果バー
//   1行に「選択肢:票数」。横のバーが伸びて%が出る。一番多いものに印
// =====================================================================
public sealed class PollResultPlugin : QuickShapePlugin
{
    public override string Name => "演出：投票・アンケートの結果バー";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PollResultParameter(sharedData);
}

public sealed class PollResultParameter : QuickShapeParameter
{
    public PollResultParameter() : this(null) { }
    public PollResultParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "賛成:62\n反対:23\nどちらでもない:15";
        Color1 = Hex(0x1DA1F2);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "選択肢", Tip: "1行に「選択肢:票数」", Group: "「投票・アンケートの結果バー」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "バーの色"),
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
        var rows = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        var data = new List<(string n, float v)>();
        foreach (var r0 in rows)
        {
            int ci = r0.LastIndexOfAny(new[] { ':', '：' });
            float v = 0f;
            if (ci > 0) float.TryParse(r0[(ci + 1)..].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v);
            data.Add((ci > 0 ? r0[..ci].Trim() : r0, v));
        }
        if (data.Count == 0) return;
        float sum = MathF.Max(1e-3f, data.Sum(d => d.v)), mx = data.Max(d => d.v);
        float bw = 1000f * u, rh = 100f * u, y0 = o.Y - data.Count * rh / 2f;
        float p = DrawKit.EaseOut(Intro(c, 0.2f, 1f));
        for (int i = 0; i < data.Count; i++)
        {
            float y = y0 + i * rh + rh / 2f, f = data[i].v / sum;
            bool top = data[i].v >= mx;
            RBox(dc, b, new Vector2(o.X, y), bw, rh * 0.75f, 14f * u, Rgb(1, 1, 1, 0.12f * a));
            if (f * p > 0.001f) RBox(dc, b, new Vector2(o.X - bw / 2f + bw * f * p / 2f, y), bw * f * p, rh * 0.75f, 14f * u, top ? C1(a) : C1(0.45f * a));
            SayAt(dc, b, data[i].n + (top && p >= 1f ? "  ✓" : ""), 36f * u, new Vector2(o.X - bw / 2f + 24f * u, y - 24f * u), Rgb(1, 1, 1, a));
            Say(dc, b, $"{f * p * 100f:0}%", 38f * u, new Vector2(o.X + bw / 2f - 70f * u, y), Rgb(1, 1, 1, a));
        }
        SayAt(dc, b, $"{sum:0} 票", 28f * u, new Vector2(o.X - bw / 2f, y0 + data.Count * rh + 10f * u), Rgb(1, 1, 1, 0.7f * a), false);
    }
}

// =====================================================================
//  フォロワー数メーター
//   数字がパラパラ増え、節目(「値2」×1000人ごと)でお祝いのキラキラ。「文字」に最後の数
// =====================================================================
public sealed class FollowerCounterPlugin : QuickShapePlugin
{
    public override string Name => "演出：フォロワー数メーター";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FollowerCounterParameter(sharedData);
}

public sealed class FollowerCounterParameter : QuickShapeParameter
{
    public FollowerCounterParameter() : this(null) { }
    public FollowerCounterParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "10000";
        Color1 = Hex(0xE1306C);
        Color3 = Hex(0xFFD54A);
    }
    protected override double DefAmount2 => 1;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "最後の数", Group: "「フォロワー数メーター」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "アイコンの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        long.TryParse(new string(Str.Where(char.IsDigit).ToArray()), out long goal);
        float p = DrawKit.EaseOut(Intro(c, 0.2f, 3f));
        long cur = (long)(goal * p);
        float k = DrawKit.EaseOutBack(Intro(c, 0f, 0.3f));
        RBox(dc, b, o, 760f * u * k, 260f * u * k, 130f * u * k, Rgb(1, 1, 1, 0.95f * a));
        Dot(dc, b, o + new Vector2(-260f * u, 0) * k, 90f * u * k, C1(a));
        Dot(dc, b, o + new Vector2(-260f * u, -18f * u) * k, 26f * u * k, Rgb(1, 1, 1, a));
        Oval(dc, b, o + new Vector2(-260f * u, 34f * u) * k, 44f * u * k, 22f * u * k, Rgb(1, 1, 1, a));
        Say(dc, b, cur.ToString("N0"), 100f * u, o + new Vector2(70f * u, -10f * u) * k, Rgb(0.1f, 0.1f, 0.12f, a), scale: k);
        Say(dc, b, "フォロワー", 30f * u, o + new Vector2(70f * u, 70f * u) * k, Rgb(0.4f, 0.4f, 0.45f, a), scale: k);
        if (p >= 1f)
        {
            float tt = c.Time - 3.2f;
            for (int i = 0; i < 24; i++)
            {
                float ang = DrawKit.H(Seed, i) * MathF.PI * 2f, f = Math.Clamp(tt * 1.2f, 0f, 1f);
                var q = Pol(o, (300f + 300f * DrawKit.H(Seed, i, 1)) * u * DrawKit.EaseOut(f), ang) + new Vector2(0, f * f * 200f * u);
                Box(dc, b, q.X - 8f * u, q.Y - 4f * u, q.X + 8f * u, q.Y + 4f * u, Col(BubbleKit.Palette(0, i), a * (1f - f)));
            }
        }
    }
}

// =====================================================================
//  配信の待機画面
//   「まもなく配信開始」とカウントダウン(「数」分から)、背景に動く模様
// =====================================================================
public sealed class StreamWaitingPlugin : QuickShapePlugin
{
    public override string Name => "演出：配信の待機画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StreamWaitingParameter(sharedData);
}

public sealed class StreamWaitingParameter : QuickShapeParameter
{
    public StreamWaitingParameter() : this(null) { }
    public StreamWaitingParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "まもなく配信開始";
        Count = 5;
        Color1 = Hex(0x7C4DFF);
        Color2 = Hex(0x00BCD4);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Group: "「配信の待機画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "カウントダウンの分数", Min: 1, Max: 60, Group: "「配信の待機画面」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "動く背景", Group: "「配信の待機画面」の設定"),
        ["Color1"] = new ParamGuide(Label: "背景の色1"),
        ["Color2"] = new ParamGuide(Label: "背景の色2"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        if (Background)
        {
            Shade(dc, new Vector2(0, 0), new Vector2(w, h), C1(a), C2(a), 0, 0, w, h);
            for (int i = 0; i < 12; i++)
            {
                float x = Gfx.Mod(DrawKit.H(Seed, i) * w + t * 40f * (1f + i % 3), w + 400f) - 200f;
                float y = DrawKit.H(Seed, i, 1) * h;
                Dot(dc, b, new Vector2(x, y), (60f + 140f * DrawKit.H(Seed, i, 2)) * U0(c), Rgb(1, 1, 1, 0.08f * a));
            }
        }
        Say(dc, b, Str, 100f * u, o + new Vector2(0, -90f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.3f * a), 4f * u);
        int left = Math.Max(0, Count * 60 - (int)c.Time);
        Say(dc, b, $"{left / 60:00}:{left % 60:00}", 160f * u, o + new Vector2(0, 90f * u), Rgb(1, 1, 1, a), Rgb(0, 0, 0, 0.3f * a), 4f * u);
        string dots = new string('●', 1 + (int)(t * 2f) % 3);
        Say(dc, b, dots, 30f * u, o + new Vector2(0, 220f * u), Rgb(1, 1, 1, 0.8f * a));
    }
}

// =====================================================================
//  エンドカード（終了画面）
//   動画を置く枠2つと、登録ボタンの丸。「ご視聴ありがとうございました」(文字)
// =====================================================================
public sealed class EndCardPlugin : QuickShapePlugin
{
    public override string Name => "演出：エンドカード（終了画面）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new EndCardParameter(sharedData);
}

public sealed class EndCardParameter : QuickShapeParameter
{
    public EndCardParameter() : this(null) { }
    public EndCardParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ご視聴ありがとうございました";
        Color1 = Hex(0xFF0000);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "上の文字", Group: "「エンドカード（終了画面）」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "暗い背景", Group: "「エンドカード（終了画面）」の設定"),
        ["Color1"] = new ParamGuide(Label: "登録ボタンの色"),
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
        if (Background) Box(dc, b, 0, 0, w, h, Rgb(0.08f, 0.08f, 0.1f, a));
        float s = U0(c);
        Say(dc, b, Str, 64f * s, new Vector2(w / 2f, 130f * s), Rgb(1, 1, 1, a));
        for (int i = 0; i < 2; i++)
        {
            float p = DrawKit.EaseOutBack(Intro(c, 0.2f + i * 0.15f, 0.4f));
            var q = new Vector2(w * (i == 0 ? 0.3f : 0.7f), h * 0.5f);
            float fw = 640f * s * p, fh = 360f * s * p;
            RFrame(dc, b, q, fw, fh, 12f * s, Rgb(1, 1, 1, 0.9f * a), 5f * s);
            Say(dc, b, i == 0 ? "おすすめ動画" : "最新の動画", 34f * s, q, Rgb(1, 1, 1, 0.5f * a), scale: p);
        }
        float k = DrawKit.EaseOutBack(Intro(c, 0.6f, 0.4f));
        var sb = new Vector2(w / 2f, h * 0.84f);
        Dot(dc, b, sb, 90f * s * k, C1(a));
        Ring(dc, b, sb, 90f * s * k, Rgb(1, 1, 1, a), 5f * s);
        Say(dc, b, "登録", 40f * s, sb, Rgb(1, 1, 1, a), scale: k);
    }
}

// =====================================================================
//  ニュースの字幕帯
//   画面下の帯。左に局ロゴ風の箱(1行目)、右に見出し(2行目)。帯が左から伸びる
// =====================================================================
public sealed class NewsLowerThirdPlugin : QuickShapePlugin
{
    public override string Name => "演出：ニュースの字幕帯";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NewsLowerThirdParameter(sharedData);
}

public sealed class NewsLowerThirdParameter : QuickShapeParameter
{
    public NewsLowerThirdParameter() : this(null) { }
    public NewsLowerThirdParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "NEWS\n新しい駅が来年オープンへ";
        Color1 = Hex(0x0D47A1);
        Color2 = Hex(0xD32F2F);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "内容", Tip: "1行目が札、2行目が見出し", Group: "「ニュースの字幕帯」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "帯の線の色"),
        ["Color2"] = new ParamGuide(Label: "札の色"),
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
        var ln = Str.Split('\n');
        string tag = ln.Length > 0 ? ln[0] : "", head = ln.Length > 1 ? ln[1] : "";
        float s = U0(c);
        float y = h * c.F(PosY) / 100f;
        if (MathF.Abs(c.F(PosY) - 50f) < 0.01f) y = h - 170f * s;
        float p = DrawKit.EaseOut(Intro(c, 0f, 0.5f));
        float bh = 110f * s, x0 = 80f * s;
        Box(dc, b, x0, y - bh / 2f, x0 + (w - 160f * s) * p, y + bh / 2f, Rgb(1, 1, 1, 0.95f * a));
        Box(dc, b, x0, y + bh / 2f, x0 + (w - 160f * s) * p, y + bh / 2f + 10f * s, C1(a));
        var tw = Measure(tag, 44f * s).Width + 60f * s;
        Box(dc, b, x0, y - bh / 2f - 20f * s, x0 + tw, y + bh / 2f, C2(a));
        Say(dc, b, tag, 44f * s, new Vector2(x0 + tw / 2f, y - 10f * s), Rgb(1, 1, 1, a));
        float q = Intro(c, 0.35f, 0.3f);
        SayAt(dc, b, head, 54f * s, new Vector2(x0 + tw + 30f * s + (1f - q) * 40f * s, y - 36f * s), Rgb(0.05f, 0.05f, 0.1f, a * q));
    }
}

// =====================================================================
//  ニュース速報テロップ
//   画面上に「ニュース速報」の帯が出て、文字が右から左へ流れる。ピロンと光る
// =====================================================================
public sealed class BreakingNewsPlugin : QuickShapePlugin
{
    public override string Name => "演出：ニュース速報テロップ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BreakingNewsParameter(sharedData);
}

public sealed class BreakingNewsParameter : QuickShapeParameter
{
    public BreakingNewsParameter() : this(null) { }
    public BreakingNewsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "【速報】○○で新記録が出ました";
        Color1 = Hex(0xC62828);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "流れる文字", Group: "「ニュース速報テロップ」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "「ニュース速報」の色"),
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
        float s = U0(c);
        float bh = 90f * s;
        float p = DrawKit.EaseOut(Intro(c, 0f, 0.3f));
        float y0 = -bh + bh * p;
        float flash = MathF.Max(0f, 1f - c.Time * 2f);
        Box(dc, b, 0, y0, w, y0 + bh, Rgb(1, 1, 1, 0.97f * a));
        Box(dc, b, 0, y0, 300f * s, y0 + bh, C1(a));
        Box(dc, b, 0, y0, 300f * s, y0 + bh, Rgb(1, 1, 1, 0.6f * flash * a));
        Say(dc, b, "ニュース速報", 40f * s, new Vector2(150f * s, y0 + bh / 2f), Rgb(1, 1, 1, a));
        var ts = Measure(Str, 48f * s);
        float span = w - 300f * s + ts.Width + 100f * s;
        float x = w - Gfx.Mod(c.Time * 260f * s * Spd(c), span);
        dc.PushAxisAlignedClip(new RawRectF(300f * s, y0, w, y0 + bh), AntialiasMode.PerPrimitive);
        TextKit.Draw(dc, b, ts, new Vector2(x, y0 + (bh - ts.Height) / 2f), Rgb(0.05f, 0.05f, 0.1f, a));
        dc.PopAxisAlignedClip();
    }
}

// =====================================================================
//  天気予報の画面
//   1行に「地名:天気:気温」(天気は 晴/曇/雨/雪)。地名ごとにマークと気温が並ぶ
// =====================================================================
public sealed class WeatherForecastPlugin : QuickShapePlugin
{
    public override string Name => "演出：天気予報の画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WeatherForecastParameter(sharedData);
}

public sealed class WeatherForecastParameter : QuickShapeParameter
{
    public WeatherForecastParameter() : this(null) { }
    public WeatherForecastParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "東京:晴:18\n大阪:曇:17\n札幌:雪:2\n福岡:雨:15";
        Color1 = Hex(0x0277BD);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "天気", Tip: "1行に「地名:天気:気温」(天気は 晴/曇/雨/雪)", Group: "「天気予報の画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "背景の板", Group: "「天気予報の画面」の設定"),
        ["Color1"] = new ParamGuide(Label: "背景の色"),
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
        var rows = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        if (rows.Count == 0) return;
        float cw = MathF.Min(330f * u, 1600f * u / rows.Count), ch = 460f * u;
        float x0 = o.X - rows.Count * cw / 2f + cw / 2f;
        if (Background) RBox(dc, b, o, rows.Count * cw + 60f * u, ch + 140f * u, 30f * u, C1(0.9f * a));
        Say(dc, b, "今日の天気", 50f * u, o + new Vector2(0, -ch / 2f - 20f * u), Rgb(1, 1, 1, a));
        for (int i = 0; i < rows.Count; i++)
        {
            var f = rows[i].Split(new[] { ':', '：' });
            string place = f.Length > 0 ? f[0] : "", wx = f.Length > 1 ? f[1] : "", tmp = f.Length > 2 ? f[2] + "℃" : "";
            float p = DrawKit.EaseOutBack(Intro(c, 0.2f + i * 0.12f, 0.35f));
            var q = new Vector2(x0 + i * cw, o.Y + 40f * u);
            RBox(dc, b, q, (cw - 20f * u) * p, ch * p, 20f * u, Rgb(1, 1, 1, 0.95f * a));
            Say(dc, b, place, 40f * u, q + new Vector2(0, -150f * u), Rgb(0.1f, 0.1f, 0.15f, a), scale: p);
            var ic = q + new Vector2(0, -10f * u);
            float r = 60f * u * p;
            if (wx.Contains("晴")) { Dot(dc, b, ic, r, Rgb(1f, 0.6f, 0.1f, a)); for (int k = 0; k < 8; k++) { float an = k * MathF.PI / 4f + t * 0.5f; Ln(dc, b, Pol(ic, r * 1.25f, an), Pol(ic, r * 1.6f, an), Rgb(1f, 0.6f, 0.1f, a), 6f * u); } }
            else
            {
                var cc = Rgb(wx.Contains("曇") ? 0.6f : 0.5f, wx.Contains("曇") ? 0.62f : 0.55f, wx.Contains("曇") ? 0.66f : 0.62f, a);
                Poly(dc, b, BubbleKit.CloudPts(ic, r * 1.3f, r * 0.8f, 6, 0.25f), cc);
                for (int k = 0; k < 3; k++)
                {
                    var dp = ic + new Vector2((k - 1) * r * 0.6f, r + Gfx.Mod(t * 60f + k * 20f, 40f) * u);
                    if (wx.Contains("雨")) Ln(dc, b, dp, dp + new Vector2(-6f, 18f) * u, Rgb(0.2f, 0.5f, 1f, a), 5f * u);
                    else if (wx.Contains("雪")) Dot(dc, b, dp, 7f * u, Rgb(0.6f, 0.8f, 1f, a));
                }
            }
            Say(dc, b, tmp, 54f * u, q + new Vector2(0, 150f * u), tmp.StartsWith("-") || (float.TryParse(f.Length > 2 ? f[2] : "", out var tv) && tv < 10f) ? Rgb(0.1f, 0.4f, 0.9f, a) : Rgb(0.9f, 0.25f, 0.2f, a), scale: p);
        }
    }
}

// =====================================================================
//  レシート・明細
//   1行に「品名:金額」。レシートが下へ伸びて、最後に合計。「文字」の1行目が店名
// =====================================================================
public sealed class ReceiptPlugin : QuickShapePlugin
{
    public override string Name => "演出：レシート・明細";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ReceiptParameter(sharedData);
}

public sealed class ReceiptParameter : QuickShapeParameter
{
    public ReceiptParameter() : this(null) { }
    public ReceiptParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ゆっくり商店\nおにぎり:150\nお茶:120\nからあげ:300\nプリン:180";
        Color1 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "明細", Tip: "1行目が店名、2行目から「品名:金額」", Group: "「レシート・明細」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "紙の色"),
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
        var rows = Str.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        if (rows.Count == 0) return;
        string shop = rows[0];
        var items = new List<(string n, long v)>();
        foreach (var r0 in rows.Skip(1))
        {
            int ci = r0.LastIndexOfAny(new[] { ':', '：' });
            long v = 0;
            if (ci > 0) long.TryParse(new string(r0[(ci + 1)..].Where(char.IsDigit).ToArray()), out v);
            items.Add((ci > 0 ? r0[..ci] : r0, v));
        }
        float rw = 520f * u, lh = 54f * u;
        float full = 200f * u + items.Count * lh + 160f * u;
        float p = DrawKit.EaseOut(Intro(c, 0.1f, 0.4f + items.Count * 0.2f));
        float rh = full * p, top = o.Y - full / 2f;
        var paper = new List<Vector2> { new(o.X - rw / 2f, top), new(o.X + rw / 2f, top) };
        int teeth = 14;
        for (int i = 0; i <= teeth * 2; i++) paper.Add(new Vector2(o.X + rw / 2f - rw * i / (teeth * 2f), top + rh + (i % 2 == 0 ? 0f : -12f * u)));
        Poly(dc, b, paper, C1(a));
        dc.PushAxisAlignedClip(new RawRectF(o.X - rw / 2f, top, o.X + rw / 2f, top + rh - 12f * u), AntialiasMode.PerPrimitive);
        var ink = Rgb(0.15f, 0.15f, 0.18f, a);
        Say(dc, b, shop, 44f * u, new Vector2(o.X, top + 70f * u), ink);
        Ln(dc, b, new Vector2(o.X - rw / 2f + 30f * u, top + 130f * u), new Vector2(o.X + rw / 2f - 30f * u, top + 130f * u), ink, 2f * u);
        float y = top + 160f * u;
        long sum = 0;
        foreach (var (n, v) in items)
        {
            SayAt(dc, b, n, 32f * u, new Vector2(o.X - rw / 2f + 40f * u, y), ink, false);
            string vs = "¥" + v.ToString("N0");
            var ts = Measure(vs, 32f * u, false);
            SayAt(dc, b, vs, 32f * u, new Vector2(o.X + rw / 2f - 40f * u - ts.Width, y), ink, false);
            sum += v; y += lh;
        }
        Ln(dc, b, new Vector2(o.X - rw / 2f + 30f * u, y + 10f * u), new Vector2(o.X + rw / 2f - 30f * u, y + 10f * u), ink, 2f * u);
        SayAt(dc, b, "合計", 40f * u, new Vector2(o.X - rw / 2f + 40f * u, y + 30f * u), ink);
        string tot = "¥" + sum.ToString("N0");
        var tts = Measure(tot, 44f * u);
        SayAt(dc, b, tot, 44f * u, new Vector2(o.X + rw / 2f - 40f * u - tts.Width, y + 26f * u), ink);
        dc.PopAxisAlignedClip();
    }
}
