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
//  にわか雨と晴れ間
//   雨が降っていて、だんだん止んで、雲の切れ間から光が差す
// =====================================================================
public sealed class ShowerClearingPlugin : QuickShapePlugin
{
    public override string Name => "演出：にわか雨と晴れ間";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ShowerClearingParameter(sharedData);
}

public sealed class ShowerClearingParameter : QuickShapeParameter
{
    public ShowerClearingParameter() : this(null) { }
    public ShowerClearingParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 120;
        Color1 = Hex(0xB0BEC5);
        Color2 = Hex(0xFFF59D);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "雨の量", Min: 10, Max: 300, Group: "「にわか雨と晴れ間」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "空の背景", Group: "「にわか雨と晴れ間」の設定"),
        ["Color1"] = new ParamGuide(Label: "雨と雲の色"),
        ["Color2"] = new ParamGuide(Label: "光の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        float s = U0(c);
        float total = MathF.Max(1f, (float)c.Length / MathF.Max(1, c.Fps));
        float clear = DrawKit.EaseOut(Math.Clamp(c.Time / total * 1.4f - 0.2f, 0f, 1f));
        if (Background) Sky(dc, w, h, Col(Gfx.Mix(Hex(0x546E7A), Hex(0x64B5F6), clear), a), Col(Gfx.Mix(Hex(0x90A4AE), Hex(0xE3F2FD), clear), a));
        int drops = (int)(Count * (1f - clear));
        for (int i = 0; i < drops; i++)
        {
            float x = DrawKit.H(Seed, i) * w, y = Gfx.Mod(DrawKit.H(Seed, i, 1) * h + t * 1400f * s, h + 60f) - 30f;
            Ln(dc, b, new Vector2(x, y), new Vector2(x - 6f * s, y + 40f * s), C1(0.6f * a), 2f * s);
        }
        for (int i = 0; i < 6; i++)
        {
            float sp = i % 2 == 0 ? -1f : 1f;
            var q = new Vector2(w * (0.1f + i * 0.17f) + sp * clear * w * 0.25f, h * 0.12f);
            Poly(dc, b, BubbleKit.CloudPts(q, 260f * s, 110f * s, 7, 0.25f), Col(Gfx.Mix(Hex(0x607D8B), Hex(0xFFFFFF), clear), a * (1f - clear * 0.4f)));
        }
        if (clear > 0.3f)
        {
            float k = (clear - 0.3f) / 0.7f;
            var src = new Vector2(w * 0.55f, -20f);
            for (int i = 0; i < 7; i++) { float ang = MathF.PI / 2f + (i - 3f) * 0.12f; Poly(dc, b, new List<Vector2> { src, Pol(src, h * 1.3f, ang - 0.03f), Pol(src, h * 1.3f, ang + 0.03f) }, C2(0.12f * k * a)); }
        }
    }
}

// =====================================================================
//  風に飛ぶ洗濯物・旗
//   ひもに干した洗濯物(バリエーション2は小さな旗)が、風でひらひらゆれる
// =====================================================================
public sealed class WindFlagsPlugin : QuickShapePlugin
{
    public override string Name => "演出：風に飛ぶ洗濯物・旗";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WindFlagsParameter(sharedData);
}

public sealed class WindFlagsParameter : QuickShapeParameter
{
    public WindFlagsParameter() : this(null) { }
    public WindFlagsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 6;
        Color1 = Hex(0xFFFFFF);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "数", Min: 2, Max: 14, Group: "「風に飛ぶ洗濯物・旗」の設定"),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=洗濯物　2=三角の旗", Min: 1, Max: 2, Group: "「風に飛ぶ洗濯物・旗」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "ひもの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float span = 1500f * u;
        var rope = new List<Vector2>();
        for (int i = 0; i <= 30; i++) { float f = i / 30f; rope.Add(o + new Vector2(-span / 2f + span * f, MathF.Sin(f * MathF.PI) * 90f * u - 90f * u)); }
        Path(dc, b, rope, C1(a), 4f * u);
        int n = Math.Clamp(Count, 2, 14);
        float gust = 0.6f + 0.4f * MathF.Sin(t * 0.9f * Spd(c));
        for (int i = 0; i < n; i++)
        {
            float f = (i + 0.5f) / n;
            var top = o + new Vector2(-span / 2f + span * f, MathF.Sin(f * MathF.PI) * 90f * u - 90f * u);
            var col = Col(BubbleKit.Palette(0, i), a);
            if (Variant >= 2)
            {
                var flag = new List<Vector2> { top - new Vector2(40f * u, 0), top + new Vector2(40f * u, 0) };
                flag.Add(top + new Vector2(MathF.Sin(t * 6f + i) * 30f * u * gust + 40f * u * gust, 110f * u * (1f - gust * 0.4f)));
                Poly(dc, b, flag, col);
                continue;
            }
            float cw = 110f * u, ch = 140f * u;
            var cloth = new List<Vector2>();
            for (int k = 0; k <= 8; k++) { float g = k / 8f; cloth.Add(top + new Vector2(-cw / 2f + cw * g, 0)); }
            for (int k = 8; k >= 0; k--)
            {
                float g = k / 8f;
                float sway = (MathF.Sin(t * 5f * Spd(c) + i + g * 3f) * 18f + 50f * gust) * u;
                cloth.Add(top + new Vector2(-cw / 2f + cw * g + sway, ch * (1f - gust * 0.3f) + MathF.Sin(t * 7f + g * 5f) * 8f * u));
            }
            Poly(dc, b, cloth, col);
            Box(dc, b, top.X - cw * 0.35f, top.Y - 12f * u, top.X - cw * 0.25f, top.Y + 14f * u, Rgb(0.9f, 0.6f, 0.2f, a));
            Box(dc, b, top.X + cw * 0.25f, top.Y - 12f * u, top.X + cw * 0.35f, top.Y + 14f * u, Rgb(0.9f, 0.6f, 0.2f, a));
        }
    }
}

// =====================================================================
//  雷雲が近づく
//   暗い雲が流れてきて空をおおい、遠くでピカッと光る
// =====================================================================
public sealed class StormApproachPlugin : QuickShapePlugin
{
    public override string Name => "演出：雷雲が近づく";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StormApproachParameter(sharedData);
}

public sealed class StormApproachParameter : QuickShapeParameter
{
    public StormApproachParameter() : this(null) { }
    public StormApproachParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x37474F);
        Color2 = Hex(0xFFFDE7);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "空の背景", Group: "「雷雲が近づく」の設定"),
        ["Color1"] = new ParamGuide(Label: "雲の色"),
        ["Color2"] = new ParamGuide(Label: "稲光の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        float p = DrawKit.EaseOut(Intro(c, 0f, 3f / MathF.Max(0.1f, Spd(c))));
        if (Background) Sky(dc, w, h, Col(Gfx.Mix(Hex(0x81D4FA), Hex(0x263238), p), a), Col(Gfx.Mix(Hex(0xE1F5FE), Hex(0x546E7A), p), a));
        float fl = Gfx.Mod(c.Time, 2.7f);
        bool flash = p > 0.6f && fl < 0.15f;
        if (flash && Background) Box(dc, b, 0, 0, w, h, C2(0.35f * a));
        for (int i = 0; i < 10; i++)
        {
            var q = new Vector2(-w * 0.5f + (w * 1.6f) * p + i * w * 0.14f - w * 0.6f, h * (0.08f + 0.12f * DrawKit.H(Seed, i)));
            Poly(dc, b, BubbleKit.CloudPts(q, 320f * s, 140f * s, 8, 0.25f), Col(Gfx.Mix(Color1, Hex(0x000000), DrawKit.H(Seed, i, 1) * 0.3f), (0.85f + 0.1f * DrawKit.H(Seed, i, 2)) * a));
        }
        if (flash)
        {
            var bolt = new List<Vector2>();
            var bp = new Vector2(w * (0.3f + 0.4f * DrawKit.H(Seed, (int)(c.Time / 2.7f))), h * 0.2f);
            bolt.Add(bp);
            for (int i = 0; i < 6; i++) { bp += new Vector2((DrawKit.H(Seed, i, 7) - 0.5f) * 120f * s, h * 0.1f); bolt.Add(bp); }
            Path(dc, b, bolt, C2(a), 5f * s);
        }
        Box(dc, b, 0, h * 0.85f, w, h, Rgb(0.1f, 0.12f, 0.1f, (Background ? 1f : 0f) * a));
    }
}

// =====================================================================
//  紅葉の山
//   赤や黄色の木々でおおわれた山と、舞う紅葉の葉っぱ
// =====================================================================
public sealed class AutumnMountainPlugin : QuickShapePlugin
{
    public override string Name => "演出：紅葉の山";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new AutumnMountainParameter(sharedData);
}

public sealed class AutumnMountainParameter : QuickShapeParameter
{
    public AutumnMountainParameter() : this(null) { }
    public AutumnMountainParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 30;
        Color1 = Hex(0xE64A19);
        Color2 = Hex(0xFFB300);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "舞う葉っぱの数", Min: 0, Max: 100, Group: "「紅葉の山」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "空の背景", Group: "「紅葉の山」の設定"),
        ["Color1"] = new ParamGuide(Label: "赤い木の色"),
        ["Color2"] = new ParamGuide(Label: "黄色い木の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        if (Background) Sky(dc, w, h, Col(Hex(0x81D4FA), a), Col(Hex(0xFFF3E0), a));
        for (int L = 0; L < 2; L++)
        {
            float baseY = h * (0.55f + L * 0.15f);
            var m = new List<Vector2> { new(-10, h + 10) };
            for (int i = 0; i <= 40; i++) { float x = w * i / 40f; m.Add(new Vector2(x, baseY - (MathF.Sin(x / w * MathF.PI * (1.2f + L)) * 0.5f + 0.5f) * h * 0.3f)); }
            m.Add(new Vector2(w + 10, h + 10));
            Poly(dc, b, m, L == 0 ? Rgb(0.55f, 0.3f, 0.2f, a) : Rgb(0.45f, 0.25f, 0.15f, a));
            for (int i = 0; i < 70; i++)
            {
                float x = DrawKit.H(Seed + L, i) * w;
                float top = baseY - (MathF.Sin(x / w * MathF.PI * (1.2f + L)) * 0.5f + 0.5f) * h * 0.3f;
                float y = top + DrawKit.H(Seed + L, i, 1) * (h - top);
                var col = Gfx.Mix(Color1, Color2, DrawKit.H(Seed + L, i, 2));
                Dot(dc, b, new Vector2(x, y), (26f + 30f * DrawKit.H(Seed + L, i, 3)) * s * (L == 0 ? 0.8f : 1.2f), Col(Gfx.Mix(col, Hex(0x5D4037), L == 0 ? 0.25f : 0f), a));
            }
        }
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 0.12f * Spd(c) + DrawKit.H(Seed, i, 5), 1f);
            var q = new Vector2(DrawKit.H(Seed, i, 6) * w + ph * 300f * s + MathF.Sin(t * 2f + i) * 40f * s - 150f * s, -30f + ph * (h + 60f));
            Star(dc, b, q, 16f * s, 5, 0.5f, t * 2f + i, Col(i % 2 == 0 ? Color1 : Color2, a));
        }
    }
}

// =====================================================================
//  冬の吐く息・窓の結露
//   白い息がふわっと出る。バリエーション2は、窓の結露に指で描いた跡と、しずく
// =====================================================================
public sealed class WinterBreathPlugin : QuickShapePlugin
{
    public override string Name => "演出：冬の吐く息・窓の結露";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WinterBreathParameter(sharedData);
}

public sealed class WinterBreathParameter : QuickShapeParameter
{
    public WinterBreathParameter() : this(null) { }
    public WinterBreathParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFFFFF);
        Text = "♡";
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "結露に描く文字", Group: "「冬の吐く息・窓の結露」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Label: "形の種類", Tip: "1=吐く息　2=窓の結露", Min: 1, Max: 2, Group: "「冬の吐く息・窓の結露」の設定"),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "息・くもりの色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        if (Variant >= 2)
        {
            float s = U0(c);
            Box(dc, b, 0, 0, w, h, C1(0.35f * a));
            for (int i = 0; i < 300; i++) Dot(dc, b, new Vector2(DrawKit.H(Seed, i) * w, DrawKit.H(Seed, i, 1) * h), (1.5f + 3f * DrawKit.H(Seed, i, 2)) * s, C1(0.35f * a));
            float p = Intro(c, 0.3f, 1.2f);
            var ts = Measure(Str, 260f * u, true);
            var tl = o - new Vector2(ts.Width, ts.Height) / 2f;
            dc.PushAxisAlignedClip(new RawRectF(tl.X - 20f, tl.Y - 20f, tl.X + ts.Width * p + 2f, tl.Y + ts.Height + 20f), AntialiasMode.PerPrimitive);
            TextKit.Draw(dc, b, ts, tl, Rgb(0.5f, 0.6f, 0.7f, 0.5f * a), Rgb(0.4f, 0.5f, 0.6f, 0.3f * a), 6f * u);
            dc.PopAxisAlignedClip();
            for (int i = 0; i < 6; i++)
            {
                float ph = Gfx.Mod(t * 0.2f + DrawKit.H(Seed, i, 4), 1f);
                float x = DrawKit.H(Seed, i, 5) * w;
                Ln(dc, b, new Vector2(x, 0), new Vector2(x, ph * h), Rgb(0.6f, 0.7f, 0.8f, 0.3f * a), 6f * s);
                Dot(dc, b, new Vector2(x, ph * h), 7f * s, Rgb(0.8f, 0.9f, 1f, 0.7f * a));
            }
            return;
        }
        float per = 2f / MathF.Max(0.1f, Spd(c));
        float f = Gfx.Mod(c.Time, per) / per;
        for (int i = 0; i < 6; i++)
        {
            float g = Math.Clamp(f * 1.3f - i * 0.06f, 0f, 1f);
            if (g <= 0f) continue;
            var q = o + new Vector2((30f + 200f * DrawKit.EaseOut(g)) * u, -g * 90f * u + MathF.Sin(g * 5f + i) * 15f * u);
            Dot(dc, b, q, (14f + 60f * g) * u, C1(0.55f * a * (1f - g)));
        }
    }
}

// =====================================================================
//  川の流れ（見下ろし）
//   上から見た川。両岸の草地と、流れの筋、きらめきが流れていく
// =====================================================================
public sealed class RiverTopViewPlugin : QuickShapePlugin
{
    public override string Name => "演出：川の流れ（見下ろし）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RiverTopViewParameter(sharedData);
}

public sealed class RiverTopViewParameter : QuickShapeParameter
{
    public RiverTopViewParameter() : this(null) { }
    public RiverTopViewParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x29B6F6);
        Color2 = Hex(0x7CB342);
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
        ["Color1"] = new ParamGuide(Label: "川の色"),
        ["Color2"] = new ParamGuide(Label: "岸の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        Box(dc, b, 0, 0, w, h, C2(a));
        var L = new List<Vector2>(); var R = new List<Vector2>();
        for (int i = 0; i <= 40; i++)
        {
            float y = h * i / 40f;
            float cx = w * 0.5f + MathF.Sin(y / h * 3f) * w * 0.12f;
            float hw = w * 0.16f + MathF.Sin(y / h * 5f + 1f) * w * 0.03f;
            L.Add(new Vector2(cx - hw, y)); R.Add(new Vector2(cx + hw, y));
        }
        var river = new List<Vector2>(L); for (int i = R.Count - 1; i >= 0; i--) river.Add(R[i]);
        Path(dc, b, L, Rgb(0.85f, 0.78f, 0.6f, a), 40f * s);
        Path(dc, b, R, Rgb(0.85f, 0.78f, 0.6f, a), 40f * s);
        Poly(dc, b, river, C1(a));
        for (int i = 0; i < 40; i++)
        {
            float f = DrawKit.H(Seed, i);
            float y = Gfx.Mod(DrawKit.H(Seed, i, 1) * h + t * 220f * s * Spd(c), h + 100f) - 50f;
            float cx = w * 0.5f + MathF.Sin(y / h * 3f) * w * 0.12f;
            float hw = w * 0.16f + MathF.Sin(y / h * 5f + 1f) * w * 0.03f;
            float x = cx - hw * 0.85f + hw * 1.7f * f;
            Ln(dc, b, new Vector2(x, y), new Vector2(x, y + 50f * s), Rgb(1, 1, 1, 0.35f * a), 3f * s);
            if (i % 4 == 0) DrawKit.Twinkle(dc, b, new Vector2(x, y), 16f * s * (0.5f + 0.5f * MathF.Sin(t * 6f + i)), 2f * s, System.Windows.Media.Colors.White, a);
        }
        for (int i = 0; i < 20; i++) { var q = new Vector2(DrawKit.H(Seed, i, 8) < 0.5f ? DrawKit.H(Seed, i, 9) * w * 0.25f : w - DrawKit.H(Seed, i, 9) * w * 0.25f, DrawKit.H(Seed, i, 10) * h); Dot(dc, b, q, (30f + 30f * DrawKit.H(Seed, i, 11)) * s, Rgb(0.25f, 0.5f, 0.2f, a)); }
    }
}

// =====================================================================
//  潮だまり・岩場
//   岩のくぼみの水たまりに、小さな波紋とカニ・ヒトデ
// =====================================================================
public sealed class TidePoolPlugin : QuickShapePlugin
{
    public override string Name => "演出：潮だまり・岩場";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TidePoolParameter(sharedData);
}

public sealed class TidePoolParameter : QuickShapeParameter
{
    public TidePoolParameter() : this(null) { }
    public TidePoolParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x4DD0E1);
        Color2 = Hex(0x8D8378);
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
        ["Color1"] = new ParamGuide(Label: "水の色"),
        ["Color2"] = new ParamGuide(Label: "岩の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        float s = U0(c);
        Box(dc, b, 0, 0, w, h, C2(a));
        for (int i = 0; i < 30; i++) Poly(dc, b, BubbleKit.CloudPts(new Vector2(DrawKit.H(Seed, i) * w, DrawKit.H(Seed, i, 1) * h), (80f + 140f * DrawKit.H(Seed, i, 2)) * s, (60f + 100f * DrawKit.H(Seed, i, 3)) * s, 6, 0.3f), Col(Gfx.Mix(Color2, Hex(0x5D5348), DrawKit.H(Seed, i, 4)), a));
        var pc = new Vector2(w / 2f, h / 2f);
        var pool = BubbleKit.CloudPts(pc, w * 0.3f, h * 0.28f, 9, 0.12f);
        Poly(dc, b, pool, C1(0.85f * a));
        for (int i = 0; i < 4; i++) { float f = Gfx.Mod(t * 0.4f + i * 0.25f, 1f); var q = pc + new Vector2((DrawKit.H(Seed, i, 5) - 0.5f) * w * 0.3f, (DrawKit.H(Seed, i, 6) - 0.5f) * h * 0.25f); Oval(dc, b, q, 0.1f + 80f * s * f, 0.1f + 40f * s * f, Rgb(1, 1, 1, 0.25f * (1f - f) * a)); }
        var st = pc + new Vector2(-w * 0.12f, h * 0.06f);
        Star(dc, b, st, 50f * s, 5, 0.42f, 0.3f, Rgb(1f, 0.55f, 0.3f, a));
        var cb = pc + new Vector2(w * 0.32f + MathF.Sin(t * 1.5f) * 30f * s, -h * 0.05f);
        Oval(dc, b, cb, 44f * s, 30f * s, Rgb(0.9f, 0.3f, 0.2f, a));
        for (int e = -1; e <= 1; e += 2) { Dot(dc, b, cb + new Vector2(e * 50f, -24f) * s, 16f * s, Rgb(0.9f, 0.3f, 0.2f, a)); for (int k = 0; k < 3; k++) Ln(dc, b, cb + new Vector2(e * 30f, 8f + k * 8f) * s, cb + new Vector2(e * 60f, 24f + k * 10f + MathF.Sin(t * 10f + k) * 4f) * s, Rgb(0.8f, 0.25f, 0.15f, a), 4f * s); Dot(dc, b, cb + new Vector2(e * 12f, -30f) * s, 6f * s, Rgb(0, 0, 0, a)); }
    }
}

// =====================================================================
//  砂漠と陽炎
//   砂丘と強い日ざし。遠くの地平線がゆらゆらゆれる
// =====================================================================
public sealed class DesertHeatHazePlugin : QuickShapePlugin
{
    public override string Name => "演出：砂漠と陽炎";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new DesertHeatHazeParameter(sharedData);
}

public sealed class DesertHeatHazeParameter : QuickShapeParameter
{
    public DesertHeatHazeParameter() : this(null) { }
    public DesertHeatHazeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xF4C27A);
        Color2 = Hex(0xFFF3C4);
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
        ["Color1"] = new ParamGuide(Label: "砂の色"),
        ["Color2"] = new ParamGuide(Label: "空の下の色"),
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
        float s = U0(c);
        Sky(dc, w, h * 0.55f, Col(Hex(0x64B5F6), a), C2(a));
        var sun = new Vector2(w * 0.75f, h * 0.15f);
        DrawKit.Glow(dc, sun, 300f * s, Rgb(1, 1, 0.8f, 0.6f * a));
        Dot(dc, b, sun, 70f * s, Rgb(1, 1, 0.9f, a));
        float hz = h * 0.55f;
        for (int i = 0; i < 18; i++)
        {
            float y = hz - 30f * s + i * 4f * s;
            float off = MathF.Sin(t * 4f + i * 0.9f) * 12f * s;
            Box(dc, b, off, y, w + off, y + 3f * s, Rgb(1, 0.95f, 0.85f, 0.18f * a));
        }
        for (int L = 0; L < 3; L++)
        {
            var dune = new List<Vector2> { new(-10, h + 10) };
            float baseY = hz + L * h * 0.13f;
            for (int i = 0; i <= 40; i++) { float x = w * i / 40f; dune.Add(new Vector2(x, baseY - MathF.Abs(MathF.Sin(x / w * MathF.PI * (1.5f + L * 0.7f) + L)) * 90f * s + (L == 0 ? MathF.Sin(t * 5f + x * 0.02f) * 3f * s : 0f))); }
            dune.Add(new Vector2(w + 10, h + 10));
            Poly(dc, b, dune, Col(Gfx.Mix(Color1, Hex(0xC68A4A), L * 0.3f), a));
        }
    }
}

// =====================================================================
//  夜の田んぼとカエルの声
//   夜の田んぼの水面に月が映り、「ケロケロ」の文字が浮かぶ
// =====================================================================
public sealed class NightPaddyFrogPlugin : QuickShapePlugin
{
    public override string Name => "演出：夜の田んぼとカエルの声";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NightPaddyFrogParameter(sharedData);
}

public sealed class NightPaddyFrogParameter : QuickShapeParameter
{
    public NightPaddyFrogParameter() : this(null) { }
    public NightPaddyFrogParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ケロケロ";
        Color1 = Hex(0xFFF59D);
        Color2 = Hex(0x0D1B2A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "鳴き声", Group: "「夜の田んぼとカエルの声」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "月の色"),
        ["Color2"] = new ParamGuide(Label: "夜空の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        float s = U0(c);
        float hz = h * 0.5f;
        Sky(dc, w, hz, C2(a), Col(Hex(0x1B3A4B), a));
        for (int i = 0; i < 60; i++) Dot(dc, b, new Vector2(DrawKit.H(Seed, i) * w, DrawKit.H(Seed, i, 1) * hz * 0.9f), 1.5f * s, Rgb(1, 1, 1, (0.4f + 0.4f * MathF.Sin(t * 2f + i)) * a));
        var moon = new Vector2(w * 0.3f, h * 0.18f);
        DrawKit.Glow(dc, moon, 200f * s, C1(0.4f * a));
        Dot(dc, b, moon, 60f * s, C1(a));
        Box(dc, b, 0, hz, w, h, Rgb(0.05f, 0.1f, 0.15f, a));
        for (int i = 0; i < 12; i++)
        {
            float y = hz + (moon.Y - 0f) * 0.2f + 300f * s + i * 10f * s;
            float wdt = (80f - i * 5f) * s;
            float off = MathF.Sin(t * 2f + i) * 10f * s;
            Box(dc, b, moon.X - wdt + off, y, moon.X + wdt + off, y + 4f * s, C1(0.4f * a * (1f - i / 12f)));
        }
        for (int r = 0; r < 6; r++)
            for (int k = 0; k < 30; k++)
            {
                float y = hz + 40f * s + r * r * 18f * s;
                float x = (k + (r % 2) * 0.5f) * w / 28f;
                Ln(dc, b, new Vector2(x, y), new Vector2(x - 6f * s * (1 + r), y - (14f + r * 8f) * s), Rgb(0.2f, 0.45f, 0.25f, a), (2f + r * 0.6f) * s);
                Ln(dc, b, new Vector2(x, y), new Vector2(x + 6f * s * (1 + r), y - (12f + r * 7f) * s), Rgb(0.2f, 0.45f, 0.25f, a), (2f + r * 0.6f) * s);
            }
        for (int i = 0; i < 3; i++)
        {
            float ph = Gfx.Mod(t * 0.4f + i * 0.33f, 1f);
            var q = new Vector2(w * (0.55f + 0.15f * i), h * 0.7f - ph * 120f * s);
            Say(dc, b, Str, 50f * s, q, Rgb(0.8f, 1f, 0.6f, a * MathF.Sin(ph * MathF.PI)), Rgb(0, 0, 0, 0.4f * a * MathF.Sin(ph * MathF.PI)), 3f * s, bold: true);
        }
    }
}

// =====================================================================
//  星空と街の灯りの境目
//   地平線の街あかりがオレンジに光り、上は満天の星空
// =====================================================================
public sealed class StarsCityLightsPlugin : QuickShapePlugin
{
    public override string Name => "演出：星空と街の灯りの境目";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StarsCityLightsParameter(sharedData);
}

public sealed class StarsCityLightsParameter : QuickShapeParameter
{
    public StarsCityLightsParameter() : this(null) { }
    public StarsCityLightsParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 500;
        Color1 = Hex(0xFFB74D);
        Color2 = Hex(0x050A1E);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "星の数", Min: 50, Max: 1500, Group: "「星空と街の灯りの境目」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "街あかりの色"),
        ["Color2"] = new ParamGuide(Label: "夜空の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
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
        float s = U0(c);
        float hz = h * 0.78f;
        Sky(dc, w, hz, C2(a), Col(Gfx.Mix(Color2, Color1, 0.35f), a));
        for (int i = 0; i < Count; i++)
        {
            float y = DrawKit.H(Seed, i, 1) * hz;
            float fade = 1f - MathF.Pow(y / hz, 3f);
            Dot(dc, b, new Vector2(DrawKit.H(Seed, i) * w, y), (0.8f + 1.8f * DrawKit.H(Seed, i, 2)) * s, Rgb(1, 1, 1, fade * (0.4f + 0.6f * (0.5f + 0.5f * MathF.Sin(t * (1f + 3f * DrawKit.H(Seed, i, 3)) + i))) * a));
        }
        Shade(dc, new Vector2(0, hz - 200f * s), new Vector2(0, hz), Rgb(0, 0, 0, 0f), C1(0.45f * a), 0, hz - 200f * s, w, hz);
        var city = new List<Vector2> { new(-10, h + 10) };
        for (int i = 0; i <= 80; i++) { float x = w * i / 80f; city.Add(new Vector2(x, hz - (10f + 50f * DrawKit.H(Seed, i, 7)) * s)); city.Add(new Vector2(x + w / 80f, hz - (10f + 50f * DrawKit.H(Seed, i, 7)) * s)); }
        city.Add(new Vector2(w + 10, h + 10));
        Poly(dc, b, city, Rgb(0.03f, 0.03f, 0.06f, a));
        for (int i = 0; i < 200; i++)
        {
            var q = new Vector2(DrawKit.H(Seed, i, 8) * w, hz - 40f * s + DrawKit.H(Seed, i, 9) * (h - hz + 40f * s));
            Dot(dc, b, q, (1.5f + 2f * DrawKit.H(Seed, i, 10)) * s, C1((0.5f + 0.5f * MathF.Sin(t * 3f + i)) * a));
        }
    }
}
