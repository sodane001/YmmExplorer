using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 451〜470: SNS・配信・画面UI(実在のサービスの名前やロゴは使わない)

public sealed class MessengerChatPlugin : QuickShapePlugin { public override string Name => "演出：メッセージアプリの会話"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new MessengerChatParameter(s); }
public sealed class MessengerChatParameter : Q3Param
{
    public MessengerChatParameter() : this(null) { }
    public MessengerChatParameter(SharedDataStore? s) : base(s) { Text = "左:今どこにいるの？\n右:もうすぐ着くよ\n左:早く来て！\n右:了解！"; Color1 = Hex(0xECEFF1); Color2 = Hex(0x6FD66F); Color3 = Hex(0x8FA6B8); }
    static readonly Dictionary<string, ParamGuide> g = G("メッセージアプリの会話", "Text=会話|1行に「左:文」か「右:文」", "Speed=速さ", "Background=背景を塗る", "Color1=左の吹き出しの色", "Color2=右の吹き出しの色", "Color3=背景の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows.Select(x => x.Split([':', '：'], 2)).Where(x => x.Length == 2).ToList(); if (r.Count == 0) return;
        float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)), W = 760f * u, H = 960f * u, step = 1.15f; var o = P0(c);
        if (Background) Panel(dc, b, o, W, H, 40f * u, C3(a), Rgb(0.08f, 0.08f, 0.1f, a), 10f * u);
        int shown = Math.Clamp((int)(t / step) + 1, 0, r.Count), first = Math.Max(0, shown - 6);
        for (int i = first; i < shown; i++)
        {
            bool right = r[i][0].Trim() == "右"; string s = r[i][1].Trim(); float p = DrawKit.EaseOutBack(Sub(t, i * step + 0.34f, 0.24f)), y = o.Y - H / 2f + 100f * u + (i - first) * 142f * u, bw = MathF.Min(580f * u, Measure(s, 36f * u).Width + 60f * u), x = right ? o.X + W / 2f - 36f * u - bw / 2f : o.X - W / 2f + 36f * u + bw / 2f;
            if (t < i * step + 0.34f) { if (!right) continue; var q0 = new Vector2(o.X + W / 2f - 120f * u, y); RBox(dc, b, q0, 130f * u, 66f * u, 30f * u, C2(0.8f * a)); for (int k = 0; k < 3; k++) Dot(dc, b, q0 + new Vector2((k - 1) * 28f * u, MathF.Sin(t * 9f - k) * 5f * u), 7f * u, Rgb(1, 1, 1, a)); continue; }
            RBox(dc, b, new(x, y), bw * p, 88f * u * p, 28f * u, right ? C2(a) : C1(a)); Say(dc, b, s, 36f * u, new(x, y), Rgb(0.08f, 0.1f, 0.12f, a * p), maxW: 540f * u);
            if (right && i + 1 < shown) Say(dc, b, "既読", 22f * u, new(x - bw / 2f - 34f * u, y + 28f * u), Rgb(1, 1, 1, 0.9f * a));
        }
    }
}

public sealed class HashtagPopPlugin : QuickShapePlugin { public override string Name => "演出：ハッシュタグが並ぶ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HashtagPopParameter(s); }
public sealed class HashtagPopParameter : Q3Param
{
    public HashtagPopParameter() : this(null) { }
    public HashtagPopParameter(SharedDataStore? s) : base(s) { Text = "#ゆっくり実況\n#ゲーム\n#解説\n#おすすめ\n#初見歓迎\n#今日の動画"; Color1 = Hex(0x2F80ED); Color2 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("ハッシュタグ", "Text=タグ|1行に1つ", "Variant=並び方|1=左寄せ　2=中央　3=散らす|1|3", "Color1=タグの色", "Color2=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return; float u = U(c), fs = 44f * u, gap = 18f * u, maxW = 1400f * u, x = 0f, y = 0f; var o = P0(c); var place = new List<(string s, float x, float y, float w)>();
        foreach (var s in r) { float w = Measure(s, fs).Width + 56f * u; if (x + w > maxW && x > 0f) { x = 0f; y += 96f * u; } place.Add((s, x, y, w)); x += w + gap; }
        float totalH = y + 80f * u;
        for (int i = 0; i < place.Count; i++)
        {
            var q = place[i]; float rowW = place.Where(e => e.y == q.y).Sum(e => e.w + gap) - gap, ox = Variant == 2 ? -rowW / 2f : -maxW / 2f; var at = new Vector2(o.X + ox + q.x + q.w / 2f, o.Y + q.y - totalH / 2f + 40f * u);
            if (Variant == 3) at += new Vector2((Rnd(i) - 0.5f) * 90f * u, (Rnd(i + 8) - 0.5f) * 70f * u);
            float p = DrawKit.EaseOutBack(Intro(c, 0.1f + i * 0.13f, 0.32f)); RBox(dc, b, at, q.w * p, 76f * u * p, 38f * u, C1(a)); Say(dc, b, q.s, fs, at, C2(a), scale: MathF.Max(0.01f, p));
        }
    }
}

public sealed class ProfileCardPlugin : QuickShapePlugin { public override string Name => "演出：プロフィールカード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ProfileCardParameter(s); }
public sealed class ProfileCardParameter : Q3Param
{
    public ProfileCardParameter() : this(null) { }
    public ProfileCardParameter(SharedDataStore? s) : base(s) { Text = "ゆっくり太郎\n@yukkuri_taro\nゲームと解説の動画を作っています\n12.4万"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x4FA8FF); Color3 = Hex(0x1F2933); }
    static readonly Dictionary<string, ParamGuide> g = G("プロフィールカード", "Text=内容|1行目=名前　2行目=ID　3行目=紹介文　4行目=フォロワー数", "Color1=カードの色", "Color2=上の帯とアイコンの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0f, 0.42f)), W = 760f * u * p, H = 720f * u * p; var o = P0(c); string At(int i) => i < r.Count ? r[i] : "";
        Panel(dc, b, o, W, H, 32f * u, C1(a), Rgb(0, 0, 0, 0.2f * a), 2f * u); RBox(dc, b, o - new Vector2(0, H * 0.36f), W, H * 0.28f, 30f * u, C2(a)); Dot(dc, b, o - new Vector2(0, H * 0.2f), 105f * u * p, C1(a)); Dot(dc, b, o - new Vector2(0, H * 0.2f), 92f * u * p, C2(a)); Dot(dc, b, o - new Vector2(0, H * 0.235f), 34f * u * p, C1(a)); Oval(dc, b, o - new Vector2(0, H * 0.14f), 58f * u * p, 30f * u * p, C1(a));
        Say(dc, b, At(0), 52f * u, o + new Vector2(0, 40f * u), C3(a), maxW: 680f * u); Say(dc, b, At(1), 30f * u, o + new Vector2(0, 98f * u), Col(Color3, 0.6f * a)); Say(dc, b, At(2), 34f * u, o + new Vector2(0, 165f * u), C3(a), maxW: 680f * u); Say(dc, b, At(3) + " フォロワー", 38f * u, o + new Vector2(0, 268f * u), C2(a));
    }
}

public sealed class StoryProgressPlugin : QuickShapePlugin { public override string Name => "演出：ストーリーズ風の進行バー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new StoryProgressParameter(s); }
public sealed class StoryProgressParameter : Q3Param
{
    public StoryProgressParameter() : this(null) { }
    public StoryProgressParameter(SharedDataStore? s) : base(s) { Text = "ゆっくり太郎"; Count = 5; Color1 = Hex(0xFFFFFF); }
    protected override double DefAmount => 20;
    static readonly Dictionary<string, ParamGuide> g = G("ストーリーズ風の進行バー", "Text=名前", "Count=区切りの数||1|30", "Amount=1区切りの長さ|0〜100 = 0.5〜10.5秒", "Color1=バーの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        int n = Math.Clamp(Count, 1, 30); float u = U(c), W = c.Width - 120f * u, seg = (W - (n - 1) * 10f * u) / n, y = 42f * u, per = 0.5f + Amt(c) * 10f, x0 = (c.Width - W) / 2f, prog = c.Time / per;
        for (int i = 0; i < n; i++) { float x = x0 + i * (seg + 10f * u), k = Math.Clamp(prog - i, 0f, 1f); RBox(dc, b, new(x + seg / 2f, y), seg, 9f * u, 4f * u, C1(0.35f * a)); if (k > 0f) RBox(dc, b, new(x + seg * k / 2f, y), seg * k, 9f * u, 4f * u, C1(a)); }
        Dot(dc, b, new(x0 + 42f * u, y + 72f * u), 38f * u, C1(a)); SayAt(dc, b, Str, 38f * u, new(x0 + 98f * u, y + 48f * u), C1(a));
    }
}

public sealed class ReviewStarsPlugin : QuickShapePlugin { public override string Name => "演出：口コミの星評価"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ReviewStarsParameter(s); }
public sealed class ReviewStarsParameter : Q3Param
{
    public ReviewStarsParameter() : this(null) { }
    public ReviewStarsParameter(SharedDataStore? s) : base(s) { Text = "4.2\n1,284件\nとても分かりやすくて助かりました！"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFFB300); Color3 = Hex(0x263238); }
    static readonly Dictionary<string, ParamGuide> g = G("口コミの星評価", "Text=内容|1行目=点数(0〜5)　2行目=件数　3行目=レビュー文", "Color1=カードの色", "Color2=星の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float score = Math.Clamp(r.Count > 0 ? Num(r[0]) : 0f, 0f, 5f), u = U(c), p = DrawKit.EaseOut(Intro(c, 0.2f, 1f)), cur = score * p; var o = P0(c); Panel(dc, b, o, 900f * u, 430f * u, 28f * u, C1(a), Rgb(0, 0, 0, 0.2f * a), 2f * u);
        for (int i = 0; i < 5; i++)
        {
            var q = o + new Vector2(-300f * u + i * 104f * u, -105f * u); Star(dc, b, q, 50f * u, 5, 0.45f, -MathF.PI / 2f, Rgb(0.82f, 0.84f, 0.86f, a)); float f = Math.Clamp(cur - i, 0f, 1f);
            if (f > 0f) { dc.PushAxisAlignedClip(new Vortice.RawRectF(q.X - 52f * u, q.Y - 52f * u, q.X - 52f * u + 104f * u * f, q.Y + 52f * u), AntialiasMode.PerPrimitive); Star(dc, b, q, 50f * u, 5, 0.45f, -MathF.PI / 2f, C2(a)); dc.PopAxisAlignedClip(); }
        }
        Say(dc, b, cur.ToString("0.0"), 78f * u, o + new Vector2(310f * u, -112f * u), C3(a)); Say(dc, b, r.Count > 1 ? r[1] : "", 32f * u, o + new Vector2(310f * u, -48f * u), Col(Color3, 0.65f * a)); Say(dc, b, r.Count > 2 ? r[2] : "", 40f * u, o + new Vector2(0, 90f * u), C3(a * Sub(c.Time, 0.7f, 0.4f)), maxW: 820f * u);
    }
}

public sealed class ProductCardPlugin : QuickShapePlugin { public override string Name => "演出：ネットショップの商品カード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ProductCardParameter(s); }
public sealed class ProductCardParameter : Q3Param
{
    public ProductCardParameter() : this(null) { }
    public ProductCardParameter(SharedDataStore? s) : base(s) { Text = "ふかふかクッション\n3980\n25"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); Color3 = Hex(0xFFB74D); }
    static readonly Dictionary<string, ParamGuide> g = G("商品カード", "Text=内容|1行目=商品名　2行目=元の値段　3行目=値引き率(%)", "Color1=カードの色", "Color2=値引きの色", "Color3=商品とボタンの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float price = r.Count > 1 ? Num(r[1]) : 0f, off = r.Count > 2 ? Math.Clamp(Num(r[2]), 0f, 100f) : 0f, u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0f, 0.42f)), W = 660f * u * p, H = 840f * u * p; var o = P0(c); Panel(dc, b, o, W, H, 26f * u, C1(a), Rgb(0, 0, 0, 0.2f * a), 2f * u);
        var pic = o - new Vector2(0, 190f * u); RBox(dc, b, pic, 580f * u * p, 390f * u * p, 16f * u, Rgb(0.93f, 0.94f, 0.95f, a)); RBox(dc, b, pic, 280f * u * p, 210f * u * p, 70f * u, C3(a));
        if (off > 0f) { Star(dc, b, o + new Vector2(205f * u, -320f * u), 92f * u * p, 14, 0.82f, c.Time * 0.3f, C2(a)); Say(dc, b, off.ToString("0") + "%OFF", 30f * u, o + new Vector2(205f * u, -320f * u), Rgb(1, 1, 1, a)); }
        var ink = Rgb(0.13f, 0.15f, 0.18f, a); Say(dc, b, r.Count > 0 ? r[0] : "", 44f * u, o + new Vector2(0, 62f * u), ink, maxW: 590f * u); var was = o + new Vector2(-170f * u, 140f * u); string oldS = "¥" + price.ToString("#,0"); Say(dc, b, oldS, 34f * u, was, Rgb(0.5f, 0.5f, 0.52f, a)); float ow = Measure(oldS, 34f * u).Width; Ln(dc, b, was - new Vector2(ow / 2f, 0), was + new Vector2(ow / 2f, 0), C2(a), 3f * u); Say(dc, b, "¥" + (price * (1f - off / 100f)).ToString("#,0"), 62f * u, o + new Vector2(110f * u, 138f * u), C2(a));
        float press = MathF.Max(0f, 1f - MathF.Abs(c.Time - 1.2f) / 0.1f); RBox(dc, b, o + new Vector2(0, 300f * u + press * 4f * u), 560f * u * p, 96f * u, 48f * u, C3(a)); Say(dc, b, c.Time > 1.25f ? "カートに入れました" : "カートに入れる", 40f * u, o + new Vector2(0, 300f * u + press * 4f * u), ink);
    }
}

public sealed class FakeQrScanPlugin : QuickShapePlugin { public override string Name => "演出：QRコード風の読み取り"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FakeQrScanParameter(s); }
public sealed class FakeQrScanParameter : Q3Param
{
    public FakeQrScanParameter() : this(null) { }
    public FakeQrScanParameter(SharedDataStore? s) : base(s) { Text = "読み取り完了"; Color1 = Hex(0x111111); Color2 = Hex(0xFFFFFF); Color3 = Hex(0x33E67A); }
    static readonly Dictionary<string, ParamGuide> g = G("QRコード風", "Text=読み取り後の文字", "Seed=模様のシード", "Color1=模様の色", "Color2=地の色", "Color3=読み取り線の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        const int N = 25; float u = U(c), S = 560f * u, cell = S / N, scan = Sub(c.Time, 0.2f, 1.3f), done = Sub(c.Time, 1.5f, 0.25f); var o = P0(c) - new Vector2(0, 50f * u); float x0 = o.X - S / 2f, y0 = o.Y - S / 2f; Box(dc, b, x0 - 30f * u, y0 - 30f * u, x0 + S + 30f * u, y0 + S + 30f * u, C2(a));
        bool Finder(int x, int y) => (x < 7 && y < 7) || (x >= N - 7 && y < 7) || (x < 7 && y >= N - 7);
        for (int y = 0; y < N; y++) for (int x = 0; x < N; x++) if (!Finder(x, y) && Rnd(x * 31 + y * 57 + Seed * 101) > 0.52f) Box(dc, b, x0 + x * cell, y0 + y * cell, x0 + (x + 1) * cell, y0 + (y + 1) * cell, C1(a));
        foreach (var q in new[] { (0, 0), (N - 7, 0), (0, N - 7) }) { float fx = x0 + q.Item1 * cell, fy = y0 + q.Item2 * cell; Box(dc, b, fx, fy, fx + 7 * cell, fy + 7 * cell, C1(a)); Box(dc, b, fx + cell, fy + cell, fx + 6 * cell, fy + 6 * cell, C2(a)); Box(dc, b, fx + 2 * cell, fy + 2 * cell, fx + 5 * cell, fy + 5 * cell, C1(a)); }
        if (scan < 1f) { float y = y0 + S * MathF.Abs(MathF.Sin(scan * MathF.PI * 1.5f)); Box(dc, b, x0 - 30f * u, y - 50f * u, x0 + S + 30f * u, y, C3(0.18f * a)); Ln(dc, b, new(x0 - 30f * u, y), new(x0 + S + 30f * u, y), C3(a), 7f * u); }
        if (done > 0f) { RFrame(dc, b, o, S + 80f * u, S + 80f * u, 18f * u, C3(a * done), 10f * u); Say(dc, b, "✓ " + Str, 56f * u, o + new Vector2(0, S / 2f + 90f * u), C3(a * done), Rgb(0, 0, 0, 0.7f * a * done), 4f * u); }
    }
}

public sealed class LoginScreenPlugin : QuickShapePlugin { public override string Name => "演出：ログイン画面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new LoginScreenParameter(s); }
public sealed class LoginScreenParameter : Q3Param
{
    public LoginScreenParameter() : this(null) { }
    public LoginScreenParameter(SharedDataStore? s) : base(s) { Text = "yukkuri_user"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x2F80ED); Color3 = Hex(0xE53935); }
    static readonly Dictionary<string, ParamGuide> g = G("ログイン画面", "Text=IDの文字|パスワードは自動で●になります", "Variant=結果|1=成功　2=失敗|1|2", "Speed=打つ速さ", "Color1=窓の色", "Color2=ボタンの色", "Color3=失敗の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        string id = Str.Split('\n')[0]; float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)), per = 0.07f, t1 = 0.2f + id.Length * per, t2 = t1 + 0.2f + 8 * per, tPush = t2 + 0.25f; var o = P0(c); bool fail = Variant == 2; float shake = fail && t > tPush ? MathF.Sin((t - tPush) * 50f) * 12f * u * (1f - Sub(t, tPush, 0.45f)) : 0f; o.X += shake;
        Panel(dc, b, o, 760f * u, 640f * u, 26f * u, C1(a), Rgb(0, 0, 0, 0.2f * a), 2f * u); var ink = Rgb(0.13f, 0.15f, 0.18f, a); Say(dc, b, "ログイン", 56f * u, o - new Vector2(0, 240f * u), ink);
        void Field(string label, string val, float y, bool on) { SayAt(dc, b, label, 28f * u, new(o.X - 310f * u, y - 72f * u), Rgb(0.4f, 0.43f, 0.47f, a)); RBox(dc, b, new(o.X, y), 620f * u, 74f * u, 10f * u, Rgb(0.95f, 0.96f, 0.97f, a)); RFrame(dc, b, new(o.X, y), 620f * u, 74f * u, 10f * u, on ? C2(a) : Rgb(0.75f, 0.77f, 0.8f, a), on ? 4f * u : 2f * u); SayAt(dc, b, val + (on && (int)(t * 3f) % 2 == 0 ? "|" : ""), 34f * u, new(o.X - 292f * u, y - 23f * u), ink); }
        Field("ID", id[..Math.Clamp((int)((t - 0.2f) / per), 0, id.Length)], o.Y - 90f * u, t < t1); Field("パスワード", new string('●', Math.Clamp((int)((t - t1 - 0.2f) / per), 0, 8)), o.Y + 60f * u, t >= t1 && t < t2);
        float press = MathF.Max(0f, 1f - MathF.Abs(t - tPush) / 0.08f); RBox(dc, b, o + new Vector2(0, 205f * u), 620f * u * (1f - press * 0.03f), 84f * u, 12f * u, t > tPush + 0.15f && !fail ? Rgb(0.2f, 0.7f, 0.35f, a) : C2(a)); Say(dc, b, t > tPush + 0.15f && !fail ? "✓ ログインしました" : "ログイン", 38f * u, o + new Vector2(0, 205f * u), Rgb(1, 1, 1, a));
        if (fail && t > tPush) Say(dc, b, "IDかパスワードが違います", 30f * u, o + new Vector2(0, 278f * u), C3(a));
    }
}

public sealed class ErrorScreenPlugin : QuickShapePlugin { public override string Name => "演出：エラー画面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ErrorScreenParameter(s); }
public sealed class ErrorScreenParameter : Q3Param
{
    public ErrorScreenParameter() : this(null) { }
    public ErrorScreenParameter(SharedDataStore? s) : base(s) { Text = "404\nページが見つかりません"; Color1 = Hex(0xF5F6F8); Color2 = Hex(0x1565C0); Color3 = Hex(0x37474F); }
    static readonly Dictionary<string, ParamGuide> g = G("エラー画面", "Text=文字|1行目=大きな文字　2行目=説明", "Variant=種類|1=ページなし　2=青い停止画面風|1|2", "Color1=1の背景", "Color2=2の背景", "Color3=1の文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U0(c), w = c.Width, h = c.Height; string s1 = r.Count > 0 ? r[0] : "", s2 = r.Count > 1 ? r[1] : "";
        if (Variant == 2) { Box(dc, b, 0, 0, w, h, C2(a)); SayAt(dc, b, ":(", 190f * u, new(170f * u, 130f * u), Rgb(1, 1, 1, a)); SayAt(dc, b, s2, 56f * u, new(170f * u, 410f * u), Rgb(1, 1, 1, a), maxW: 1500f * u); SayAt(dc, b, Math.Min(100, (int)(c.Time * 18f)) + "% 完了", 44f * u, new(170f * u, 560f * u), Rgb(1, 1, 1, a)); SayAt(dc, b, "停止コード: " + s1, 34f * u, new(170f * u, 720f * u), Rgb(1, 1, 1, 0.85f * a)); return; }
        Box(dc, b, 0, 0, w, h, C1(a)); var o = new Vector2(w / 2f, h / 2f); Say(dc, b, s1, 260f * u, o - new Vector2(0, 110f * u), C3(a), scale: DrawKit.EaseOutBack(Intro(c, 0f, 0.4f))); Say(dc, b, s2, 58f * u, o + new Vector2(0, 110f * u), C3(a)); RBox(dc, b, o + new Vector2(0, 260f * u), 390f * u, 86f * u, 43f * u, Hex(0x2F80ED).ToC(a)); Say(dc, b, "ホームへ戻る", 36f * u, o + new Vector2(0, 260f * u), Rgb(1, 1, 1, a));
    }
}

internal static class Q3ColorExt { public static Color4 ToC(this System.Windows.Media.Color col, float a) => Gfx.C4(col, a); }

public sealed class FileCopyDialogPlugin : QuickShapePlugin { public override string Name => "演出：ファイルのコピー画面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FileCopyDialogParameter(s); }
public sealed class FileCopyDialogParameter : Q3Param
{
    public FileCopyDialogParameter() : this(null) { }
    public FileCopyDialogParameter(SharedDataStore? s) : base(s) { Text = "動画素材.zip"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x43A047); Color3 = Hex(0xFFCA28); }
    protected override double DefAmount => 100;
    protected override double DefAmount2 => 30;
    static readonly Dictionary<string, ParamGuide> g = G("ファイルのコピー画面", "Text=ファイル名", "Amount=進み具合", "Amount2=かかる時間|0〜100 = 0〜10秒", "Color1=窓の色", "Color2=バーの色", "Color3=フォルダーの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U(c), dur = MathF.Max(0.1f, Amt2(c) * 10f), k = Sub(c.Time, 0f, dur) * Amt(c); var o = P0(c); Panel(dc, b, o, 980f * u, 500f * u, 18f * u, C1(a), Rgb(0, 0, 0, 0.2f * a), 2f * u); var ink = Rgb(0.13f, 0.15f, 0.18f, a); SayAt(dc, b, "コピーしています…", 40f * u, new(o.X - 450f * u, o.Y - 220f * u), ink); var l = o + new Vector2(-300f * u, -50f * u); var rr = o + new Vector2(300f * u, -50f * u);
        foreach (var q in new[] { l, rr }) { RBox(dc, b, q + new Vector2(-34f * u, -48f * u), 80f * u, 32f * u, 6f * u, C3(a)); RBox(dc, b, q, 150f * u, 106f * u, 8f * u, C3(a)); }
        if (k < 1f) for (int i = 0; i < 4; i++) { float f = Gfx.Mod(c.Time * 0.9f + i / 4f, 1f); var q = Vector2.Lerp(l, rr, f) - new Vector2(0, MathF.Sin(f * MathF.PI) * 90f * u); RBox(dc, b, q, 46f * u, 60f * u, 4f * u, Rgb(0.93f, 0.95f, 0.98f, a * MathF.Sin(f * MathF.PI))); RFrame(dc, b, q, 46f * u, 60f * u, 4f * u, Rgb(0.5f, 0.55f, 0.6f, a * MathF.Sin(f * MathF.PI)), 2f * u); }
        Box(dc, b, o.X - 450f * u, o.Y + 80f * u, o.X + 450f * u, o.Y + 118f * u, Rgb(0.88f, 0.89f, 0.9f, a)); Box(dc, b, o.X - 450f * u, o.Y + 80f * u, o.X - 450f * u + 900f * u * k, o.Y + 118f * u, C2(a)); SayAt(dc, b, Str, 32f * u, new(o.X - 450f * u, o.Y + 140f * u), ink, maxW: 620f * u); Say(dc, b, (k * 100f).ToString("0") + "%", 36f * u, new(o.X + 390f * u, o.Y + 160f * u), ink); SayAt(dc, b, k >= Amt(c) ? "完了しました" : $"残り {MathF.Max(0f, dur - c.Time):0} 秒", 30f * u, new(o.X - 450f * u, o.Y + 190f * u), Rgb(0.4f, 0.43f, 0.47f, a));
    }
}

public sealed class TaskbarNoticePlugin : QuickShapePlugin { public override string Name => "演出：タスクバーと通知アイコン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TaskbarNoticeParameter(s); }
public sealed class TaskbarNoticeParameter : Q3Param
{
    public TaskbarNoticeParameter() : this(null) { }
    public TaskbarNoticeParameter(SharedDataStore? s) : base(s) { Text = "21:45\n新しいメッセージが届きました"; Count = 6; Color1 = Hex(0x1C2430); Color2 = Hex(0x4FA8FF); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("タスクバーと通知", "Text=時刻と通知文|1行目=時刻　2行目=通知文", "Count=アイコンの数||1|14", "Color1=バーの色", "Color2=アイコンの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U0(c), w = c.Width, h = c.Height, bh = 76f * u, y = h - bh / 2f; int n = Math.Clamp(Count, 1, 14); Box(dc, b, 0, h - bh, w, h, C1(0.96f * a));
        for (int i = 0; i < n; i++) { var q = new Vector2(w / 2f + (i - (n - 1) / 2f) * 72f * u, y); RBox(dc, b, q, 46f * u, 46f * u, 10f * u, Col(Gfx.Hsv(205f + i * 37f, 0.55f, 0.95f), a)); if (i == 0) Dot(dc, b, q + new Vector2(0, 31f * u), 3.5f * u, C3(a)); }
        Say(dc, b, r.Count > 0 ? r[0] : "", 30f * u, new(w - 110f * u, y), C3(a));
        float k = DrawKit.EaseOut(Sub(c.Time, 0.4f, 0.35f)) * (1f - DrawKit.EaseOut(Sub(c.Time, 4.2f, 0.35f))); if (k <= 0f) return; var p = new Vector2(w - 330f * u + (1f - k) * 680f * u, h - bh - 110f * u); Panel(dc, b, p, 620f * u, 170f * u, 18f * u, C1(0.98f * a), C2(a), 2f * u); Dot(dc, b, p - new Vector2(235f * u, 0), 42f * u, C2(a)); Say(dc, b, "!", 50f * u, p - new Vector2(235f * u, 0), C3(a)); SayAt(dc, b, "お知らせ", 28f * u, p + new Vector2(-170f * u, -56f * u), C2(a)); SayAt(dc, b, r.Count > 1 ? r[1] : "", 34f * u, p + new Vector2(-170f * u, -10f * u), C3(a), maxW: 440f * u);
    }
}

public sealed class ContextMenuGuidePlugin : QuickShapePlugin { public override string Name => "演出：右クリックメニューの解説"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ContextMenuGuideParameter(s); }
public sealed class ContextMenuGuideParameter : Q3Param
{
    public ContextMenuGuideParameter() : this(null) { }
    public ContextMenuGuideParameter(SharedDataStore? s) : base(s) { Text = "開く\nコピー\n貼り付け\n名前の変更\n削除"; Count = 3; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x2F80ED); }
    static readonly Dictionary<string, ParamGuide> g = G("右クリックメニュー", "Text=メニュー項目|1行に1つ", "Count=選ぶ行||1|20", "Color1=メニューの色", "Color2=選んだ行の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return; float u = U(c), t = c.Time, rh = 74f * u, mw = 470f * u, mh = rh * r.Count + 18f * u; var o = P0(c) - new Vector2(mw / 2f, mh / 2f); int pick = Math.Clamp(Count, 1, r.Count) - 1; float open = DrawKit.EaseOut(Sub(t, 0.75f, 0.14f)); var target = o + new Vector2(mw * 0.6f, 9f * u + (pick + 0.5f) * rh);
        if (open > 0f) { Panel(dc, b, o + new Vector2(mw / 2f, mh * open / 2f), mw, mh * open, 10f * u, C1(a), Rgb(0, 0, 0, 0.3f * a), 2f * u); if (open >= 1f) for (int i = 0; i < r.Count; i++) { float y = o.Y + 9f * u + i * rh; if (i == pick && t > 1.65f) Box(dc, b, o.X + 6f * u, y, o.X + mw - 6f * u, y + rh, C2(a)); SayAt(dc, b, r[i], 36f * u, new(o.X + 36f * u, y + 14f * u), i == pick && t > 1.65f ? Rgb(1, 1, 1, a) : Rgb(0.12f, 0.14f, 0.17f, a)); } }
        var cur = t < 0.65f ? Vector2.Lerp(o + new Vector2(-260f * u, -180f * u), o, DrawKit.EaseOut(Sub(t, 0f, 0.6f))) : Vector2.Lerp(o, target, DrawKit.EaseOut(Sub(t, 1f, 0.6f))); var arrow = new List<Vector2> { cur, cur + new Vector2(0, 58f * u), cur + new Vector2(15f * u, 45f * u), cur + new Vector2(27f * u, 70f * u), cur + new Vector2(39f * u, 64f * u), cur + new Vector2(27f * u, 40f * u), cur + new Vector2(45f * u, 40f * u) }; Poly(dc, b, arrow, Rgb(1, 1, 1, a)); Outline(dc, b, arrow, Rgb(0, 0, 0, a), 3f * u);
        float pulse = Sub(t, 0.62f, 0.3f); if (pulse is > 0f and < 1f) Ring(dc, b, o, 70f * u * pulse, C2(a * (1f - pulse)), 5f * u);
    }
}

public sealed class VideoCallGridPlugin : QuickShapePlugin { public override string Name => "演出：ビデオ通話の画面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new VideoCallGridParameter(s); }
public sealed class VideoCallGridParameter : Q3Param
{
    public VideoCallGridParameter() : this(null) { }
    public VideoCallGridParameter(SharedDataStore? s) : base(s) { Text = "霊夢\n魔理沙\n妖夢\n咲夜"; Count = 1; Color1 = Hex(0x26303C); Color2 = Hex(0x5CFF9D); Color3 = Hex(0xFF5252); }
    static readonly Dictionary<string, ParamGuide> g = G("ビデオ通話", "Text=名前|1行に1人(2〜4人)", "Count=話す人の番号||1|4", "Color1=枠の色", "Color2=話している色", "Color3=マイクオフの色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows.Take(4).ToList(); if (r.Count == 0) return; float u = U(c), W = 1560f * u, H = 860f * u; var o = P0(c); int n = r.Count, cols = n <= 2 ? n : 2, rows = n <= 2 ? 1 : 2, talk = Math.Clamp(Count, 1, n) - 1; float cw = W / cols, ch = H / rows;
        for (int i = 0; i < n; i++)
        {
            var q = new Vector2(o.X - W / 2f + (i % cols + 0.5f) * cw, o.Y - H / 2f + (i / cols + 0.5f) * ch); RBox(dc, b, q, cw - 16f * u, ch - 16f * u, 14f * u, C1(a)); float R = MathF.Min(cw, ch) * 0.22f; Dot(dc, b, q - new Vector2(0, R * 0.25f), R, Col(Gfx.Hsv(i * 80f + 15f, 0.45f, 0.85f), a)); Say(dc, b, r[i][..1], R * 0.95f, q - new Vector2(0, R * 0.25f), Rgb(1, 1, 1, a));
            if (i == talk) { float g2 = 0.65f + 0.35f * MathF.Sin(c.Time * 9f); RFrame(dc, b, q, cw - 16f * u, ch - 16f * u, 14f * u, C2(a * g2), 8f * u); }
            var tag = new Vector2(q.X - cw / 2f + 130f * u, q.Y + ch / 2f - 52f * u); RBox(dc, b, tag, 210f * u, 54f * u, 10f * u, Rgb(0, 0, 0, 0.65f * a)); Say(dc, b, r[i], 30f * u, tag + new Vector2(24f * u, 0), Rgb(1, 1, 1, a), maxW: 150f * u); var m = tag - new Vector2(78f * u, 0); RBox(dc, b, m - new Vector2(0, 5f * u), 14f * u, 26f * u, 7f * u, i == talk ? C2(a) : C3(a)); if (i != talk) Ln(dc, b, m + new Vector2(-13f * u, -18f * u), m + new Vector2(13f * u, 14f * u), C3(a), 3f * u);
        }
    }
}

public sealed class RecIndicatorPlugin : QuickShapePlugin { public override string Name => "演出：録画中のRECマーク"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RecIndicatorParameter(s); }
public sealed class RecIndicatorParameter : Q3Param
{
    public RecIndicatorParameter() : this(null) { }
    public RecIndicatorParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFF2D2D); Color2 = Hex(0xFFFFFF); }
    protected override double DefAmount => 0;
    protected override double DefAmount2 => 75;
    static readonly Dictionary<string, ParamGuide> g = G("RECマーク", "Amount=始まりの時間|0〜100 = 0〜100分", "Amount2=電池の残り", "Background=電池を表示", "Color1=赤い点の色", "Color2=文字と線の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U0(c), w = c.Width, h = c.Height, m = 64f * u, l = 110f * u, tw = 5f * u; var ink = C2(a);
        foreach (var (x, y, dx, dy) in new[] { (m, m, 1f, 1f), (w - m, m, -1f, 1f), (m, h - m, 1f, -1f), (w - m, h - m, -1f, -1f) }) { Ln(dc, b, new(x, y), new(x + dx * l, y), ink, tw); Ln(dc, b, new(x, y), new(x, y + dy * l), ink, tw); }
        if ((c.Time % 1f) < 0.62f) Dot(dc, b, new(m + 58f * u, m + 56f * u), 19f * u, C1(a)); SayAt(dc, b, "REC", 46f * u, new(m + 92f * u, m + 26f * u), ink); int sec = (int)(Amt(c) * 6000f + c.Time); Say(dc, b, $"{sec / 3600:00}:{sec / 60 % 60:00}:{sec % 60:00}", 44f * u, new(w / 2f, m + 54f * u), ink);
        if (Background) { float bx = w - m - 190f * u, by = m + 38f * u, lv = Amt2(c); Frame(dc, b, bx, by, bx + 118f * u, by + 44f * u, ink, 4f * u); Box(dc, b, bx + 118f * u, by + 13f * u, bx + 126f * u, by + 31f * u, ink); Box(dc, b, bx + 6f * u, by + 6f * u, bx + 6f * u + 106f * u * lv, by + 38f * u, lv < 0.2f ? C1(a) : ink); }
    }
}

public sealed class CommentPickupPlugin : QuickShapePlugin { public override string Name => "演出：コメントのピックアップ枠"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CommentPickupParameter(s); }
public sealed class CommentPickupParameter : Q3Param
{
    public CommentPickupParameter() : this(null) { }
    public CommentPickupParameter(SharedDataStore? s) : base(s) { Text = "視聴者さん\nこの動画めちゃくちゃ分かりやすいです！"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFF7043); Color3 = Hex(0x263238); }
    static readonly Dictionary<string, ParamGuide> g = G("コメントのピックアップ", "Text=名前とコメント|1行目=名前　2行目=コメント", "Color1=枠の色", "Color2=名前とアイコンの色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float u = U(c), p = DrawKit.EaseOutBack(Intro(c, 0f, 0.38f)); var o = P0(c) + new Vector2((1f - p) * -180f * u, 0); Panel(dc, b, o, 1320f * u, 300f * u, 26f * u, C1(a), C2(a), 6f * u); Dot(dc, b, o - new Vector2(530f * u, 0), 76f * u, C2(a)); Dot(dc, b, o - new Vector2(530f * u, 22f * u), 26f * u, C1(a)); Oval(dc, b, o - new Vector2(530f * u, -30f * u), 44f * u, 23f * u, C1(a));
        SayAt(dc, b, r.Count > 0 ? r[0] : "", 34f * u, o + new Vector2(-420f * u, -105f * u), C2(a), maxW: 950f * u); string s = r.Count > 1 ? r[1] : ""; int n = Math.Clamp((int)((c.Time - 0.3f) / 0.035f), 0, s.Length); SayAt(dc, b, s[..n], 52f * u, o + new Vector2(-420f * u, -36f * u), C3(a), maxW: 950f * u);
    }
}

public sealed class TrendRankingPlugin : QuickShapePlugin { public override string Name => "演出：トレンドランキング"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TrendRankingParameter(s); }
public sealed class TrendRankingParameter : Q3Param
{
    public TrendRankingParameter() : this(null) { }
    public TrendRankingParameter(SharedDataStore? s) : base(s) { Text = "新作ゲーム発表:上\nゆっくり解説:上\n今日の天気:-\n週末のお出かけ:下\n話題の映画:上"; Color1 = Hex(0x15202B); Color2 = Hex(0xFF5252); Color3 = Hex(0x4FC3F7); }
    static readonly Dictionary<string, ParamGuide> g = G("トレンドランキング", "Text=話題|1行に「話題:上」「話題:下」「話題:-」", "Color1=窓の色", "Color2=上昇の色", "Color3=下降の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows.Select(x => x.Split([':', '：'], 2)).ToList(); if (r.Count == 0) return; float u = U(c), rh = 100f * u, H = rh * r.Count + 110f * u; var o = P0(c); Panel(dc, b, o, 980f * u, H, 22f * u, C1(0.96f * a), Rgb(1, 1, 1, 0.2f * a), 2f * u); Say(dc, b, "いまのトレンド", 44f * u, new(o.X, o.Y - H / 2f + 54f * u), Rgb(1, 1, 1, a));
        for (int i = 0; i < r.Count; i++)
        {
            float p = DrawKit.EaseOut(Intro(c, 0.1f + i * 0.1f, 0.4f)), y = o.Y - H / 2f + 110f * u + (i + 0.5f) * rh, x = o.X + (1f - p) * 70f * u; string dir = r[i].Length > 1 ? r[i][1].Trim() : "-"; Say(dc, b, (i + 1).ToString(), 48f * u, new(x - 410f * u, y), i < 3 ? C2(a * p) : Rgb(1, 1, 1, 0.75f * a * p)); SayAt(dc, b, r[i][0], 42f * u, new(x - 340f * u, y - 27f * u), Rgb(1, 1, 1, a * p), maxW: 640f * u);
            if (dir == "上" || dir == "下") { float d = dir == "上" ? -1f : 1f, ax = x + 400f * u, bob = MathF.Sin(c.Time * 5f + i) * 4f * u; var col = dir == "上" ? C2(a * p) : C3(a * p); Poly(dc, b, new List<Vector2> { new(ax, y + bob + d * 26f * u), new(ax + 24f * u, y + bob - d * 18f * u), new(ax - 24f * u, y + bob - d * 18f * u) }, col); }
            else Ln(dc, b, new(x + 382f * u, y), new(x + 418f * u, y), Rgb(0.7f, 0.7f, 0.7f, a * p), 5f * u);
        }
    }
}

public sealed class ScreenshotFlashPlugin : QuickShapePlugin { public override string Name => "演出：スクリーンショット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ScreenshotFlashParameter(s); }
public sealed class ScreenshotFlashParameter : Q3Param
{
    public ScreenshotFlashParameter() : this(null) { }
    public ScreenshotFlashParameter(SharedDataStore? s) : base(s) { Text = "保存しました"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x90A4AE); }
    static readonly Dictionary<string, ParamGuide> g = G("スクリーンショット", "Text=文字", "Variant=吸い込む向き|1=右下　2=左下　3=右上　4=左上|1|4", "Color1=光と枠の色", "Color2=縮む画面の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float u = U0(c), w = c.Width, h = c.Height, t = c.Time, flash = 1f - Sub(t, 0f, 0.25f), k = DrawKit.EaseOut(Sub(t, 0.12f, 0.5f)), leave = Sub(t, 1.8f, 0.35f), sw = Lerp(w, 380f * u, k), sh = Lerp(h, 214f * u, k); float gx = Variant is 2 or 4 ? 230f * u : w - 230f * u, gy = Variant is 3 or 4 ? 150f * u : h - 150f * u; var q = Vector2.Lerp(new(w / 2f, h / 2f), new(gx, gy), k) + new Vector2(Variant is 2 or 4 ? -1f : 1f, 0) * leave * 520f * u;
        if (leave < 1f) { Box(dc, b, q.X - sw / 2f, q.Y - sh / 2f, q.X + sw / 2f, q.Y + sh / 2f, C2(0.45f * a * (1f - leave))); Frame(dc, b, q.X - sw / 2f, q.Y - sh / 2f, q.X + sw / 2f, q.Y + sh / 2f, C1(a * (1f - leave)), 7f * u); if (k > 0.9f) Say(dc, b, Str, 30f * u, q, C1(a * (1f - leave)), Rgb(0, 0, 0, 0.7f * a), 3f * u); }
        if (flash > 0f) Box(dc, b, 0, 0, w, h, C1(0.9f * flash * a));
    }
}

public sealed class CalendarAppPlugin : QuickShapePlugin { public override string Name => "演出：カレンダーアプリの予定"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CalendarAppParameter(s); }
public sealed class CalendarAppParameter : Q3Param
{
    public CalendarAppParameter() : this(null) { }
    public CalendarAppParameter(SharedDataStore? s) : base(s) { Text = "2026/12\n5:動画投稿\n12:収録\n24:クリスマス配信\n31:大晦日"; Color1 = Hex(0xFFFFFF); Color2 = Hex(0x2F80ED); Color3 = Hex(0xFF7043); }
    static readonly Dictionary<string, ParamGuide> g = G("カレンダー", "Text=年月と予定|1行目=年/月。2行目から「日:予定」", "Color1=表の色", "Color2=見出しの色", "Color3=予定の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; if (r.Count == 0) return; var ym = r[0].Split(['/', '年', '月', '-']).Where(x => x.Length > 0).ToArray(); int year = ym.Length > 0 ? Math.Clamp((int)Num(ym[0], 2026), 1, 9999) : 2026, month = ym.Length > 1 ? Math.Clamp((int)Num(ym[1], 1), 1, 12) : 1, first = (int)new DateTime(year, month, 1).DayOfWeek, days = DateTime.DaysInMonth(year, month);
        var ev = new Dictionary<int, string>(); foreach (var x in r.Skip(1)) { var f = x.Split([':', '：'], 2); if (f.Length == 2) ev[(int)Num(f[0])] = f[1].Trim(); }
        float u = U(c), W = 1400f * u, H = 900f * u, cw = W / 7f, top = 170f * u, ch = (H - top) / 6f; var o = P0(c); float x0 = o.X - W / 2f, y0 = o.Y - H / 2f; RBox(dc, b, o, W, H, 22f * u, C1(a)); RBox(dc, b, new(o.X, y0 + 50f * u), W, 100f * u, 20f * u, C2(a)); Say(dc, b, $"{year}年 {month}月", 50f * u, new(o.X, y0 + 50f * u), Rgb(1, 1, 1, a)); string wk = "日月火水木金土";
        for (int i = 0; i < 7; i++) Say(dc, b, wk[i].ToString(), 30f * u, new(x0 + (i + 0.5f) * cw, y0 + 135f * u), i == 0 ? Rgb(0.9f, 0.25f, 0.25f, a) : i == 6 ? C2(a) : Rgb(0.25f, 0.27f, 0.3f, a));
        for (int d = 1; d <= days; d++)
        {
            int slot = first + d - 1, cx = slot % 7, cy = slot / 7; float x = x0 + cx * cw, y = y0 + top + cy * ch; Frame(dc, b, x, y, x + cw, y + ch, Rgb(0.85f, 0.87f, 0.9f, a), 1.5f * u); SayAt(dc, b, d.ToString(), 28f * u, new(x + 10f * u, y + 5f * u), cx == 0 ? Rgb(0.9f, 0.25f, 0.25f, a) : cx == 6 ? C2(a) : Rgb(0.2f, 0.22f, 0.25f, a));
            if (ev.TryGetValue(d, out var txt)) { float p = DrawKit.EaseOut(Intro(c, 0.3f + d * 0.025f, 0.3f)); Box(dc, b, x + 6f * u, y + ch - 50f * u, x + 6f * u + (cw - 12f * u) * p, y + ch - 10f * u, C3(a)); Say(dc, b, txt, 22f * u, new(x + cw / 2f, y + ch - 30f * u), Rgb(1, 1, 1, a * p), maxW: cw - 18f * u); }
        }
    }
}

public sealed class CalculatorScreenPlugin : QuickShapePlugin { public override string Name => "演出：電卓の画面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CalculatorScreenParameter(s); }
public sealed class CalculatorScreenParameter : Q3Param
{
    public CalculatorScreenParameter() : this(null) { }
    public CalculatorScreenParameter(SharedDataStore? s) : base(s) { Text = "1280×12\n15360"; Color1 = Hex(0x20262E); Color2 = Hex(0xFF9800); Color3 = Hex(0xDDF5D5); }
    static readonly Dictionary<string, ParamGuide> g = G("電卓", "Text=式と答え|1行目=式　2行目=答え", "Speed=打つ速さ", "Color1=本体の色", "Color2=押したボタンの色", "Color3=表示窓の色");
    static readonly string[] keys = ["7", "8", "9", "÷", "4", "5", "6", "×", "1", "2", "3", "−", "0", ".", "=", "+"];
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; string ex = r.Count > 0 ? r[0] : "", ans = r.Count > 1 ? r[1] : ""; float u = U(c), t = c.Time * MathF.Max(0.05f, Spd(c)), per = 0.22f; int typed = Math.Clamp((int)(t / per), 0, ex.Length); bool done = t >= ex.Length * per + 0.25f; string press = done && t < ex.Length * per + 0.45f ? "=" : typed > 0 && t - (typed - 1) * per < per * 0.6f && !done ? ex[typed - 1].ToString().Replace("-", "−") : ""; var o = P0(c);
        RBox(dc, b, o, 610f * u, 930f * u, 42f * u, C1(a)); RBox(dc, b, o - new Vector2(0, 325f * u), 530f * u, 180f * u, 16f * u, C3(a)); var ink = Rgb(0.08f, 0.12f, 0.08f, a); string line = done ? ans : ex[..typed]; float fs = MathF.Min(84f * u, 480f * u / MathF.Max(1, line.Length) * 1.7f), tw = Measure(line, fs).Width; SayAt(dc, b, line, fs, o + new Vector2(240f * u - tw, -325f * u - fs * 0.62f), ink); if (done) SayAt(dc, b, ex + " =", 26f * u, o + new Vector2(-245f * u, -405f * u), Rgb(0.2f, 0.3f, 0.2f, a), maxW: 490f * u);
        for (int i = 0; i < keys.Length; i++) { var q = o + new Vector2((i % 4 - 1.5f) * 136f * u, -105f * u + (i / 4) * 136f * u); bool on = keys[i] == press; RBox(dc, b, q + new Vector2(0, on ? 3f * u : 0f), 118f * u, 118f * u, 22f * u, on ? C2(a) : i % 4 == 3 || keys[i] == "=" ? Rgb(0.32f, 0.36f, 0.42f, a) : Rgb(0.2f, 0.24f, 0.29f, a)); Say(dc, b, keys[i], 54f * u, q + new Vector2(0, on ? 3f * u : 0f), Rgb(1, 1, 1, a)); }
    }
}

public sealed class NaviTurnPlugin : QuickShapePlugin { public override string Name => "演出：ナビの曲がる案内"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new NaviTurnParameter(s); }
public sealed class NaviTurnParameter : Q3Param
{
    public NaviTurnParameter() : this(null) { }
    public NaviTurnParameter(SharedDataStore? s) : base(s) { Text = "300\nゆっくり通り"; Color1 = Hex(0x12324F); Color2 = Hex(0x4FE08A); Color3 = Hex(0xFFFFFF); }
    static readonly Dictionary<string, ParamGuide> g = G("ナビの案内", "Text=距離と通り|1行目=距離(m)　2行目=通りの名前", "Variant=向き|1=右　2=左　3=直進　4=Uターン|1|4", "Color1=板の色", "Color2=道順の色", "Color3=文字の色");
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => g;
    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        var r = Rows; float start = r.Count > 0 ? Num(r[0], 300f) : 300f, left = MathF.Max(0f, start * (1f - Sub(c.Time, 0.5f, 4f))), u = U(c); var o = P0(c); Panel(dc, b, o, 1020f * u, 340f * u, 26f * u, C1(0.96f * a), Rgb(1, 1, 1, 0.2f * a), 2f * u); var m = o - new Vector2(330f * u, 0); float dir = Variant == 2 ? -1f : 1f; Ln(dc, b, m + new Vector2(0, 130f * u), m - new Vector2(0, 130f * u), Rgb(1, 1, 1, 0.25f * a), 50f * u); Ln(dc, b, m - new Vector2(130f * u, 20f * u), m + new Vector2(130f * u, -20f * u), Rgb(1, 1, 1, 0.25f * a), 50f * u);
        var route = Variant switch { 3 => new List<Vector2> { m + new Vector2(0, 125f * u), m - new Vector2(0, 92f * u) }, 4 => new List<Vector2> { m + new Vector2(-36f * u, 125f * u), m + new Vector2(-36f * u, -40f * u), m + new Vector2(-18f * u, -75f * u), m + new Vector2(18f * u, -75f * u), m + new Vector2(36f * u, -40f * u), m + new Vector2(36f * u, 40f * u) }, _ => new List<Vector2> { m + new Vector2(0, 125f * u), m + new Vector2(0, -20f * u), m + new Vector2(dir * 90f * u, -20f * u) } };
        var part = BubbleKit.PartByLength(route, DrawKit.EaseOut(Intro(c, 0.1f, 0.5f))); if (part.Count >= 2) { Path(dc, b, part, C2(a), 26f * u); var e = part[^1]; var d = Vector2.Normalize(e - part[^2] + new Vector2(0.0001f, 0)); var n = new Vector2(-d.Y, d.X); Poly(dc, b, new List<Vector2> { e + d * 44f * u, e + n * 38f * u, e - n * 38f * u }, C2(a)); }
        string turn = Variant switch { 2 => "左方向", 3 => "直進", 4 => "Uターン", _ => "右方向" }; SayAt(dc, b, left.ToString("0") + "m 先", 78f * u, o + new Vector2(-130f * u, -125f * u), C3(a)); SayAt(dc, b, turn, 56f * u, o + new Vector2(-130f * u, -25f * u), C2(a)); SayAt(dc, b, r.Count > 1 ? r[1] : "", 36f * u, o + new Vector2(-130f * u, 72f * u), C3(0.85f * a), maxW: 610f * u);
    }
}
