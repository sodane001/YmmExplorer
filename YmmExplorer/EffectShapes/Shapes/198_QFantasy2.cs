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
//  ポーションの瓶
//   丸い瓶の中の液体がゆれて、泡がぷくぷく上がる。「値」で液体の量
// =====================================================================
public sealed class PotionBottlePlugin : QuickShapePlugin
{
    public override string Name => "演出：ポーションの瓶";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PotionBottleParameter(sharedData);
}

public sealed class PotionBottleParameter : QuickShapeParameter
{
    public PotionBottleParameter() : this(null) { }
    public PotionBottleParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xE040FB);
        Color2 = Hex(0xB3E5FC);
    }
    protected override double DefAmount => 65;

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Label: "液体の量", Group: "「ポーションの瓶」の設定"),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "液体の色"),
        ["Color2"] = new ParamGuide(Label: "瓶の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float R = 180f * u;
        var bc = o + new Vector2(0, 60f * u);
        Box(dc, b, o.X - 50f * u, bc.Y - R - 120f * u, o.X + 50f * u, bc.Y - R + 30f * u, C2(0.5f * a));
        Box(dc, b, o.X - 60f * u, bc.Y - R - 160f * u, o.X + 60f * u, bc.Y - R - 110f * u, Rgb(0.6f, 0.4f, 0.25f, a));
        Dot(dc, b, bc, R, C2(0.3f * a));
        float lvl = bc.Y + R - 2f * R * Amt(c);
        var liq = new List<Vector2>();
        for (int i = 0; i <= 30; i++)
        {
            float f = i / 30f, x = bc.X - R + 2f * R * f;
            liq.Add(new Vector2(x, lvl + MathF.Sin(t * 3f * Spd(c) + f * 6f) * 8f * u));
        }
        dc.PushAxisAlignedClip(new RawRectF(bc.X - R, lvl, bc.X + R, bc.Y + R), AntialiasMode.PerPrimitive);
        Dot(dc, b, bc, R - 6f * u, C1(0.85f * a));
        dc.PopAxisAlignedClip();
        var wave = new List<Vector2>(liq.Where(pq => Vector2.Distance(pq, bc) < R - 8f * u));
        if (wave.Count > 1)
        {
            var under = new List<Vector2>(wave) { new(wave[^1].X, lvl + 14f * u), new(wave[0].X, lvl + 14f * u) };
            Poly(dc, b, under, C1(0.85f * a));
        }
        DrawKit.Glow(dc, bc + new Vector2(0, R * 0.3f), R * 1.3f, C1(0.25f * a));
        for (int i = 0; i < 8; i++)
        {
            float ph = Gfx.Mod(t * 0.6f * Spd(c) + DrawKit.H(Seed, i), 1f);
            var q = new Vector2(bc.X + (DrawKit.H(Seed, i, 1) - 0.5f) * R * 1.2f, bc.Y + R * 0.8f - ph * (bc.Y + R * 0.8f - lvl));
            Ring(dc, b, q, (5f + 8f * DrawKit.H(Seed, i, 2)) * u, Rgb(1, 1, 1, 0.7f * a), 2f * u);
        }
        Ring(dc, b, bc, R, C2(0.9f * a), 6f * u);
        Path(dc, b, Gfx.ArcPts(bc, R * 0.75f, MathF.PI * 1.1f, MathF.PI * 1.4f, 10), Rgb(1, 1, 1, 0.7f * a), 10f * u);
    }
}

// =====================================================================
//  魔法の杖の軌跡
//   杖がくるっと振られて、先から星の粉が軌跡になって残る
// =====================================================================
public sealed class WandTrailPlugin : QuickShapePlugin
{
    public override string Name => "演出：魔法の杖の軌跡";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new WandTrailParameter(sharedData);
}

public sealed class WandTrailParameter : QuickShapeParameter
{
    public WandTrailParameter() : this(null) { }
    public WandTrailParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 40;
        Color1 = Hex(0xFFEB3B);
        Color2 = Hex(0x8D6E63);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "粉の数", Min: 10, Max: 120, Group: "「魔法の杖の軌跡」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "星の粉の色"),
        ["Color2"] = new ParamGuide(Label: "杖の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float sp = 2f * Spd(c);
        Vector2 Tip(float x) => o + new Vector2(MathF.Cos(x) * 260f, MathF.Sin(x * 2f) * 120f - 160f) * u;
        for (int i = Count; i >= 1; i--)
        {
            float tt = t * sp - i * 0.035f;
            var q = Tip(tt) + new Vector2((DrawKit.H(Seed, i) - 0.5f) * 30f, (DrawKit.H(Seed, i, 1) - 0.5f) * 30f + i * 2f) * u;
            float f = 1f - i / (float)Count;
            Star(dc, b, q, (5f + 10f * f) * u, 4, 0.4f, t * 3f + i, C1(f * a));
        }
        var tip = Tip(t * sp);
        var hand = o + new Vector2(0, 120f * u);
        Ln(dc, b, hand, tip, C2(a), 14f * u);
        DrawKit.Glow(dc, tip, 70f * u, C1(0.7f * a));
        Star(dc, b, tip, 30f * u, 5, 0.45f, t * 2f, C1(a));
    }
}

// =====================================================================
//  時間停止
//   画面の色が抜けて(灰色のベール)、大きな時計の針が止まる。「文字」で表示
// =====================================================================
public sealed class TimeStopPlugin : QuickShapePlugin
{
    public override string Name => "演出：時間停止";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TimeStopParameter(sharedData);
}

public sealed class TimeStopParameter : QuickShapeParameter
{
    public TimeStopParameter() : this(null) { }
    public TimeStopParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ザ・ワールド";
        Color1 = Hex(0x7E57C2);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "文字", Group: "「時間停止」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
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
        float p = Intro(c, 0f, 0.8f);
        float s = U0(c);
        float R0 = MathF.Max(w, h) * 1.2f * DrawKit.EaseOut(p);
        var cc = o;
        if (p < 1f) Ring(dc, b, cc, R0, C1(a * (1f - p)), 40f * s);
        b.Color = Rgb(0.5f, 0.5f, 0.55f, 0.55f * a);
        if (R0 > 1f) dc.FillEllipse(new Ellipse(cc, R0, R0), b);
        Box(dc, b, 0, 0, w, h, Rgb(0.3f, 0.2f, 0.45f, 0.15f * a * p));
        float R = 300f * u;
        Ring(dc, b, cc, R, Rgb(1, 1, 1, 0.8f * a), 10f * u);
        for (int i = 0; i < 12; i++) Ln(dc, b, Pol(cc, R * 0.82f, i * MathF.PI / 6f), Pol(cc, R * 0.94f, i * MathF.PI / 6f), Rgb(1, 1, 1, 0.8f * a), 6f * u);
        float spin = (1f - DrawKit.EaseOut(p)) * 20f;
        Ln(dc, b, cc, Pol(cc, R * 0.8f, -MathF.PI / 2f + spin + 1.2f), Rgb(1, 1, 1, a), 8f * u);
        Ln(dc, b, cc, Pol(cc, R * 0.55f, -MathF.PI / 2f + spin * 0.1f + 2.6f), Rgb(1, 1, 1, a), 12f * u);
        Dot(dc, b, cc, 16f * u, Rgb(1, 1, 1, a));
        if (p >= 1f) Say(dc, b, Str, 90f * u, cc + new Vector2(0, R + 90f * u), Rgb(1, 1, 1, a), C1(a), 6f * u);
    }
}

// =====================================================================
//  転送ビーム（光の柱で消える）
//   上から光の柱が降りてきて、光の粒が舞い上がる。転送される場面に
// =====================================================================
public sealed class TeleportBeamPlugin : QuickShapePlugin
{
    public override string Name => "演出：転送ビーム（光の柱で消える）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TeleportBeamParameter(sharedData);
}

public sealed class TeleportBeamParameter : QuickShapeParameter
{
    public TeleportBeamParameter() : this(null) { }
    public TeleportBeamParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x80DEEA);
        Count = 50;
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "光の粒の数", Min: 10, Max: 150, Group: "「転送ビーム（光の柱で消える）」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "光の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float p = Intro(c, 0f, 1.6f / MathF.Max(0.1f, Spd(c)));
        float beam = MathF.Sin(MathF.Min(1f, p * 1.2f) * MathF.PI);
        float bw = 260f * u, top = o.Y - 900f * u;
        for (int k = 0; k < 3; k++)
            Box(dc, b, o.X - bw / 2f * (1f - k * 0.3f), top, o.X + bw / 2f * (1f - k * 0.3f), o.Y, C1(0.18f * beam * a));
        Oval(dc, b, o, bw * 0.7f, 30f * u, C1(0.6f * beam * a));
        for (int i = 0; i < Count; i++)
        {
            float ph = Gfx.Mod(t * 0.8f + DrawKit.H(Seed, i), 1f);
            var q = new Vector2(o.X + (DrawKit.H(Seed, i, 1) - 0.5f) * bw * 0.9f, o.Y - ph * 800f * u);
            Dot(dc, b, q, (3f + 5f * DrawKit.H(Seed, i, 2)) * u, Rgb(1, 1, 1, beam * (1f - ph) * a));
        }
        for (int i = 0; i < 8; i++) { float y = o.Y - Gfx.Mod(t * 300f * u + i * 100f * u, 800f * u); Box(dc, b, o.X - bw / 2f, y, o.X + bw / 2f, y + 4f * u, Rgb(1, 1, 1, 0.3f * beam * a)); }
    }
}

// =====================================================================
//  巨大ロボの目のアップ
//   金属の顔のアップで、目に「ウィーン」と光が入る
// =====================================================================
public sealed class RobotEyeCloseupPlugin : QuickShapePlugin
{
    public override string Name => "演出：巨大ロボの目のアップ";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new RobotEyeCloseupParameter(sharedData);
}

public sealed class RobotEyeCloseupParameter : QuickShapeParameter
{
    public RobotEyeCloseupParameter() : this(null) { }
    public RobotEyeCloseupParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x546E7A);
        Color2 = Hex(0xFF1744);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Label: "金属の顔の背景", Group: "「巨大ロボの目のアップ」の設定"),
        ["Color1"] = new ParamGuide(Label: "金属の色"),
        ["Color2"] = new ParamGuide(Label: "目の光の色"),
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
            Box(dc, b, 0, 0, w, h, C1(a));
            for (int i = 0; i < 6; i++) Ln(dc, b, new Vector2(0, h * (i + 0.5f) / 6f), new Vector2(w, h * (i + 0.5f) / 6f + 20f * s), Rgb(0, 0, 0, 0.25f * a), 4f * s);
            for (int i = 0; i < 30; i++) Dot(dc, b, new Vector2(DrawKit.H(Seed, i) * w, DrawKit.H(Seed, i, 1) * h), 7f * s, Rgb(0.2f, 0.25f, 0.28f, a));
        }
        float on = DrawKit.EaseOut(Intro(c, 0.4f, 0.8f));
        float ew = 900f * u, eh = 260f * u;
        var slot = new List<Vector2> { o + new Vector2(-ew / 2f, -eh * 0.3f), o + new Vector2(ew / 2f, -eh / 2f), o + new Vector2(ew / 2f, eh * 0.2f), o + new Vector2(-ew / 2f, eh / 2f) };
        Poly(dc, b, slot, Rgb(0.05f, 0.05f, 0.06f, a));
        Outline(dc, b, slot, Rgb(0.7f, 0.75f, 0.8f, a), 10f * u);
        if (on > 0f)
        {
            float scan = Gfx.Mod(c.Time * 0.6f, 1f);
            DrawKit.Glow(dc, o, ew * 0.7f * on, C2(0.4f * a));
            var core = new List<Vector2> { o + new Vector2(-ew * 0.42f * on, -eh * 0.2f), o + new Vector2(ew * 0.42f * on, -eh * 0.35f), o + new Vector2(ew * 0.42f * on, eh * 0.12f), o + new Vector2(-ew * 0.42f * on, eh * 0.35f) };
            Poly(dc, b, core, C2(a));
            Dot(dc, b, o + new Vector2((scan - 0.5f) * ew * 0.7f, 0), 40f * u, Rgb(1, 0.9f, 0.9f, a));
        }
    }
}

// =====================================================================
//  宇宙の通信画面
//   ノイズまじりの通信モニター。顔の枠と、声の波形、「通信中」の文字
// =====================================================================
public sealed class SpaceCommPlugin : QuickShapePlugin
{
    public override string Name => "演出：宇宙の通信画面";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SpaceCommParameter(sharedData);
}

public sealed class SpaceCommParameter : QuickShapeParameter
{
    public SpaceCommParameter() : this(null) { }
    public SpaceCommParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "司令部";
        Color1 = Hex(0x00E676);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "相手の名前", Group: "「宇宙の通信画面」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "画面の色"),
        ["Color2"] = new ParamGuide(Hidden: true),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Speed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float mw = 1000f * u, mh = 620f * u;
        float k = DrawKit.EaseOut(Intro(c, 0f, 0.3f));
        RBox(dc, b, o, (mw + 40f * u) * k, (mh + 40f * u), 20f * u, Rgb(0.15f, 0.17f, 0.2f, a));
        RBox(dc, b, o, mw * k, mh * k, 10f * u, Rgb(0.02f, 0.08f, 0.05f, a));
        if (k < 1f) return;
        var face = o + new Vector2(-mw * 0.22f, -20f * u);
        Dot(dc, b, face - new Vector2(0, 60f * u), 90f * u, C1(0.35f * a));
        Oval(dc, b, face + new Vector2(0, 130f * u), 160f * u, 90f * u, C1(0.35f * a));
        Frame(dc, b, face.X - 200f * u, face.Y - 200f * u, face.X + 200f * u, face.Y + 220f * u, C1(0.6f * a), 3f * u);
        var wave = new List<Vector2>();
        for (int i = 0; i <= 60; i++) { float f = i / 60f; wave.Add(new Vector2(o.X + mw * 0.08f + f * mw * 0.36f, o.Y + MathF.Sin(f * 30f + t * 14f) * 40f * u * MathF.Abs(MathF.Sin(t * 3f + f * 3f)))); }
        Path(dc, b, wave, C1(a), 4f * u);
        SayAt(dc, b, Str, 44f * u, new Vector2(o.X + mw * 0.08f, o.Y - mh * 0.4f), C1(a));
        if ((int)(t * 2f) % 2 == 0) SayAt(dc, b, "● 通信中", 30f * u, new Vector2(o.X + mw * 0.08f, o.Y + mh * 0.3f), C1(a), false);
        for (float y = o.Y - mh / 2f; y < o.Y + mh / 2f; y += 6f * u) Box(dc, b, o.X - mw / 2f, y, o.X + mw / 2f, y + 2f * u, Rgb(0, 0, 0, 0.25f * a));
        int fr = (int)(t * 20f);
        for (int i = 0; i < 6; i++)
        {
            float y = o.Y - mh / 2f + DrawKit.H(Seed + fr, i) * mh;
            Box(dc, b, o.X - mw / 2f, y, o.X + mw / 2f, y + (2f + 10f * DrawKit.H(Seed + fr, i, 1)) * u, C1(0.12f * a));
        }
    }
}

// =====================================================================
//  結界（ドーム）
//   六角形の模様のドームが、地面から広がって守る。ときどき模様が光る
// =====================================================================
public sealed class BarrierDomePlugin : QuickShapePlugin
{
    public override string Name => "演出：結界（ドーム）";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BarrierDomeParameter(sharedData);
}

public sealed class BarrierDomeParameter : QuickShapeParameter
{
    public BarrierDomeParameter() : this(null) { }
    public BarrierDomeParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0x40C4FF);
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
        ["Color1"] = new ParamGuide(Label: "結界の色"),
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
        float p = DrawKit.EaseOut(Intro(c, 0f, 0.8f));
        float R = 420f * u * p;
        if (R < 2f) return;
        var dome = new List<Vector2>(Gfx.ArcPts(o, R, MathF.PI, MathF.PI * 2f, 50));
        Poly(dc, b, dome, C1(0.12f * a));
        Oval(dc, b, o, R, R * 0.18f, C1(0.2f * a));
        float hs = 46f * u;
        for (int row = 0; row < 12; row++)
            for (int col = -12; col <= 12; col++)
            {
                var hc = o + new Vector2(col * hs * 1.5f, -row * hs * 1.732f - (col & 1) * hs * 0.866f);
                float d = Vector2.Distance(hc, o);
                if (d > R - hs * 0.5f || hc.Y > o.Y) continue;
                var hex = new List<Vector2>();
                for (int k = 0; k < 6; k++) hex.Add(Pol(hc, hs * 0.92f, k * MathF.PI / 3f));
                float glow = MathF.Max(0f, MathF.Sin(t * 3f - d / (80f * u) + col)) ;
                Outline(dc, b, hex, C1((0.25f + 0.5f * glow * glow) * a), 2.5f * u);
            }
        Path(dc, b, dome, C1(0.9f * a), 6f * u);
    }
}

// =====================================================================
//  剣の打ち合いの火花
//   2本の剣がぶつかって、まん中で火花がバチッと散る。くり返す
// =====================================================================
public sealed class SwordClashPlugin : QuickShapePlugin
{
    public override string Name => "演出：剣の打ち合いの火花";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SwordClashParameter(sharedData);
}

public sealed class SwordClashParameter : QuickShapeParameter
{
    public SwordClashParameter() : this(null) { }
    public SwordClashParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Color1 = Hex(0xFFD54A);
        Color2 = Hex(0xCFD8DC);
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
        ["Color1"] = new ParamGuide(Label: "火花の色"),
        ["Color2"] = new ParamGuide(Label: "剣の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        float per = 0.9f / MathF.Max(0.1f, Spd(c));
        float f = Gfx.Mod(c.Time, per) / per;
        float hit = f < 0.15f ? f / 0.15f : 1f;
        float back = f > 0.6f ? (f - 0.6f) / 0.4f : 0f;
        for (int e = -1; e <= 1; e += 2)
        {
            var hilt = o + new Vector2(e * 360f * u, 260f * u);
            var dir = Vector2.Normalize(o - hilt);
            var rot = MathF.Atan2(dir.Y, dir.X) + (1f - hit + back) * 0.4f * e;
            var tip = hilt + new Vector2(MathF.Cos(rot), MathF.Sin(rot)) * 560f * u;
            var nrm = new Vector2(-MathF.Sin(rot), MathF.Cos(rot));
            Poly(dc, b, new List<Vector2> { hilt + nrm * 14f * u, tip, hilt - nrm * 14f * u }, C2(a));
            Ln(dc, b, hilt + nrm * 50f * u, hilt - nrm * 50f * u, Rgb(0.6f, 0.45f, 0.2f, a), 14f * u);
            Ln(dc, b, hilt, hilt - new Vector2(MathF.Cos(rot), MathF.Sin(rot)) * 90f * u, Rgb(0.35f, 0.2f, 0.1f, a), 18f * u);
        }
        if (f >= 0.15f && f < 0.6f)
        {
            float g = (f - 0.15f) / 0.45f;
            DrawKit.Glow(dc, o, 260f * u * (1f - g), C1(0.8f * a));
            for (int i = 0; i < 16; i++)
            {
                float an = DrawKit.H(Seed + (int)(c.Time / per), i) * MathF.PI * 2f;
                var p0 = Pol(o, 30f * u + 300f * u * g * (0.5f + DrawKit.H(Seed, i, 1)), an) + new Vector2(0, g * g * 200f * u);
                var p1 = Pol(o, 30f * u + 300f * u * MathF.Max(0f, g - 0.08f) * (0.5f + DrawKit.H(Seed, i, 1)), an) + new Vector2(0, g * g * 200f * u);
                Ln(dc, b, p1, p0, C1((1f - g) * a), 5f * u);
            }
        }
    }
}

// =====================================================================
//  影分身
//   まん中の人影から、半透明の残像が左右に分かれていく。「数」で分身の数
// =====================================================================
public sealed class ShadowClonePlugin : QuickShapePlugin
{
    public override string Name => "演出：影分身";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ShadowCloneParameter(sharedData);
}

public sealed class ShadowCloneParameter : QuickShapeParameter
{
    public ShadowCloneParameter() : this(null) { }
    public ShadowCloneParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Count = 2;
        Color1 = Hex(0x311B92);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Hidden: true),
        ["Font"] = new ParamGuide(Hidden: true),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Label: "左右それぞれの分身の数", Min: 1, Max: 5, Group: "「影分身」の設定"),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "影の色"),
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
        float p = DrawKit.EaseOut(Intro(c, 0f, 0.8f));
        void Figure(Vector2 q, float al)
        {
            Dot(dc, b, q - new Vector2(0, 230f * u), 60f * u, C1(al));
            Poly(dc, b, Pts(q, u, u, -70f, -160f, 70f, -160f, 90f, 40f, 50f, 40f, 40f, 200f, 8f, 200f, 0f, 60f, -8f, 200f, -40f, 200f, -50f, 40f, -90f, 40f), C1(al));
        }
        for (int k = Count; k >= 1; k--)
            for (int e = -1; e <= 1; e += 2)
            {
                var q = o + new Vector2(e * k * 220f * u * p + MathF.Sin(t * 10f + k) * 6f * u, 0);
                Figure(q, a * (0.55f - k * 0.08f));
            }
        Figure(o, a);
    }
}

// =====================================================================
//  カードバトルの召喚陣
//   魔法陣の上にカードが浮かび上がって回転し、光る。「文字」がカードの名前
// =====================================================================
public sealed class CardSummonPlugin : QuickShapePlugin
{
    public override string Name => "演出：カードバトルの召喚陣";
    public override IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CardSummonParameter(sharedData);
}

public sealed class CardSummonParameter : QuickShapeParameter
{
    public CardSummonParameter() : this(null) { }
    public CardSummonParameter(SharedDataStore? sharedData) : base(sharedData)
    {
        Text = "ドラゴン";
        Color1 = Hex(0x7C4DFF);
        Color2 = Hex(0xFFD54A);
    }

    static readonly Dictionary<string, ParamGuide> guides = new()
    {
        ["Text"] = new ParamGuide(Label: "カードの名前", Group: "「カードバトルの召喚陣」の設定"),
        ["Amount"] = new ParamGuide(Hidden: true),
        ["Amount2"] = new ParamGuide(Hidden: true),
        ["Count"] = new ParamGuide(Hidden: true),
        ["Variant"] = new ParamGuide(Hidden: true),
        ["Background"] = new ParamGuide(Hidden: true),
        ["Color1"] = new ParamGuide(Label: "魔法陣の色"),
        ["Color2"] = new ParamGuide(Label: "カードの枠の色"),
        ["Color3"] = new ParamGuide(Hidden: true),
        ["Seed"] = new ParamGuide(Hidden: true),
    };
    protected override IReadOnlyDictionary<string, ParamGuide> Guides => guides;

    protected override void Paint(ID2D1DeviceContext dc, in DrawContext c, ID2D1SolidColorBrush b, float a)
    {
        float t = c.Time, w = c.Width, h = c.Height, u = U(c);
        var o = P0(c);
        var ground = o + new Vector2(0, 280f * u);
        float spin = t * 0.8f * Spd(c);
        for (int r = 0; r < 3; r++) { var ring = DrawKit.EllipsePts(ground, (300f - r * 70f) * u, (90f - r * 21f) * u, 60); Outline(dc, b, ring, C1((0.9f - r * 0.2f) * a), 4f * u); }
        var star = new List<Vector2>();
        for (int i = 0; i < 6; i++) { float an = spin + i * MathF.PI * 2f * 2f / 6f; star.Add(ground + new Vector2(MathF.Cos(an) * 230f * u, MathF.Sin(an) * 70f * u)); }
        Outline(dc, b, star.Take(3).ToList(), C1(0.8f * a), 3f * u);
        Outline(dc, b, star.Skip(3).ToList(), C1(0.8f * a), 3f * u);
        float rise = DrawKit.EaseOut(Intro(c, 0.2f, 1f));
        var cc = ground - new Vector2(0, (80f + 300f * rise) * u) + new Vector2(0, MathF.Sin(t * 2f) * 10f * u);
        Poly(dc, b, new List<Vector2> { ground + new Vector2(-200f * u, 0), ground + new Vector2(200f * u, 0), cc + new Vector2(120f * u, 0), cc - new Vector2(120f * u, 0) }, C1(0.15f * a * rise));
        float sx = MathF.Cos(spin * 2f * (1f - DrawKit.EaseOut(Intro(c, 1.2f, 1f))));
        float cw = 260f * u * MathF.Max(0.05f, MathF.Abs(sx)), ch = 370f * u;
        DrawKit.Glow(dc, cc, 320f * u, C2(0.3f * a * rise));
        RBox(dc, b, cc, cw + 14f * u, ch + 14f * u, 18f * u, C2(a * rise));
        RBox(dc, b, cc, cw, ch, 14f * u, sx >= 0 ? Rgb(0.15f, 0.12f, 0.3f, a * rise) : C1(a * rise));
        if (sx >= 0f)
        {
            Dot(dc, b, cc - new Vector2(0, 40f * u), 80f * u * MathF.Abs(sx), C1(0.6f * a * rise));
            Say(dc, b, Str, 36f * u, cc + new Vector2(0, ch * 0.32f), Rgb(1, 1, 1, a * rise), scale: MathF.Abs(sx));
        }
        else Star(dc, b, cc, 60f * u * MathF.Abs(sx) + 0.1f, 6, 0.5f, 0f, C2(a * rise));
    }
}
