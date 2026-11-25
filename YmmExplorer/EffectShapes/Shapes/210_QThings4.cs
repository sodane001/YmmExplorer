using System.Numerics;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// 611〜640: 食べ物・動物・乗り物

public sealed class OnigiriPlugin : QuickShapePlugin { public override string Name => "演出：おにぎり"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new OnigiriParameter(s); }
public sealed class OnigiriParameter : Q4Art
{
    public OnigiriParameter() : this(null) { }
    public OnigiriParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFDF5); Color2 = Hex(0x1F2A24); }
    protected override string[] Use => ["Color1=ごはんの色", "Color2=のりの色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float s = 1f + ArtPen.Sin(t * 4f) * .025f;
        p.Zoom(0, 200, 1f / s, s, () =>
        {
            p.F(p.C(Color1), 0, -250, 60, -225, 245, 110, 262, 170, 215, 212, -215, 212, -262, 170, -245, 110, -60, -225);
            p.D(0, -190, 72, p.C(Color1)); p.D(-175, 150, 62, p.C(Color1)); p.D(175, 150, 62, p.C(Color1));
            for (int i = 0; i < 26; i++) { float x = (ArtPen.R01(i) - .5f) * 280f, y = -120f + ArtPen.R01(i + 40) * 170f * (1f - MathF.Abs(x) / 300f); p.E(x, y, 8, 4, p.C(0xE8E0CC)); }
            p.R(0, 135, 200, 165, 12, p.C(Color2)); p.L(-82, 90, 82, 90, p.C(0x3A4A40), 3); p.L(-82, 150, 82, 150, p.C(0x3A4A40), 3);
        });
    }
}

public sealed class HamburgerPlugin : QuickShapePlugin { public override string Name => "演出：ハンバーガー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HamburgerParameter(s); }
public sealed class HamburgerParameter : Q4Art
{
    public HamburgerParameter() : this(null) { }
    public HamburgerParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xE9A84C); }
    protected override string[] Use => ["Color1=パンの色", "Speed=積む速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float Y(int i, float rest) { float q = ArtPen.Cl((t - .1f - i * .16f) / .32f); return rest - (1f - q) * (1f - q) * 620f; }
        float Al(int i) => ArtPen.Cl((t - .1f - i * .16f) * 12f);
        p.R(0, Y(0, 170), 500, 86, 38, p.C(Color1, Al(0)));
        p.R(0, Y(1, 105), 530, 72, 30, p.C(0x6B3A1F, Al(1)));
        float cy = Y(2, 62), ca = Al(2); p.F(p.C(0xFFCC33, ca), -265, cy - 18, 265, cy - 18, 215, cy + 22, 110, cy + 56, 10, cy + 22, -210, cy + 22);
        float ty = Y(3, 18), ta = Al(3); p.R(-120, ty, 230, 34, 16, p.C(0xE5392F, ta)); p.R(120, ty, 230, 34, 16, p.C(0xE5392F, ta));
        float ly = Y(4, -18), la = Al(4); var leaf = new List<Vector2>(); for (int i = 0; i <= 40; i++) leaf.Add(new(-285 + i * 14.25f, ly + ArtPen.Sin(i * 1.05f) * 15f)); leaf.Add(new(285, ly - 30)); leaf.Add(new(-285, ly - 30)); p.Fill(p.C(0x63B446, la), leaf);
        float by = Y(5, -48), ba = Al(5); p.Pie(0, by, 262, MathF.PI, MathF.Tau, p.C(Color1, ba)); for (int i = 0; i < 18; i++) { float a = MathF.PI * (1.08f + ArtPen.R01(i) * .84f), r = 70f + ArtPen.R01(i + 20) * 165f; p.E(ArtPen.Cos(a) * r, by + ArtPen.Sin(a) * r, 10, 5, p.C(0xFFF3D0, ba)); }
    }
}

public sealed class SushiSetPlugin : QuickShapePlugin { public override string Name => "演出：お寿司"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SushiSetParameter(s); }
public sealed class SushiSetParameter : Q4Art
{
    public SushiSetParameter() : this(null) { }
    public SushiSetParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x8A5A32); }
    protected override string[] Use => ["Color1=下駄の色", "Speed=並ぶ速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.R(0, 112, 900, 50, 8, p.C(Color1)); p.B(-320, 160, 56, 62, p.C(Color1)); p.B(320, 160, 56, 62, p.C(Color1));
        uint[] net = [0xD72638, 0xFFD34E, 0xFF8A5C];
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 280f, q = DrawKit.EaseOutBack(ArtPen.Cl((t - .1f - i * .18f) / .35f)), y = 40 - (1f - q) * 90f, al = ArtPen.Cl((t - .1f - i * .18f) * 8f);
            p.R(x, y + 18, 214, 76, 34, p.C(0xFFFDF5, al)); p.R(x, y - 26, 250, 58, 26, p.C(net[i], al));
            if (i == 1) p.B(x, y + 2, 44, 108, p.C(0x1F2A24, al)); else for (int k = -3; k <= 3; k++) p.L(x + k * 32 - 8, y - 50, x + k * 32 + 8, y - 2, p.C(0xFFFFFF, .4f * al), 3);
        }
    }
}

public sealed class TakoyakiPlugin : QuickShapePlugin { public override string Name => "演出：たこ焼き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new TakoyakiParameter(s); }
public sealed class TakoyakiParameter : Q4Art
{
    public TakoyakiParameter() : this(null) { }
    public TakoyakiParameter(SharedDataStore? s) : base(s) { Count = 6; Color1 = Hex(0xC97A32); Color2 = Hex(0x4A2413); }
    protected override string[] Use => ["Count=個数||1|6", "Color1=生地の色", "Color2=ソースの色", "Speed=湯気の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.E(0, 70, 430, 190, p.C(0xD9B780)); p.EL(0, 70, 430, 190, p.C(0x8A6A3C), 7);
        int n = Math.Clamp(Count, 1, 6);
        for (int i = 0; i < n; i++)
        {
            float x = (i % 3 - 1) * 220f, y = i < 3 ? 0 : 130;
            p.D(x, y, 96, p.C(Color1)); p.Pie(x, y, 92, MathF.PI * 1.02f, MathF.Tau * .99f, p.C(Color2)); p.P(p.C(0xFFF8DF), 6, x - 66, y - 34, x - 38, y - 58, x - 12, y - 28, x + 16, y - 58, x + 42, y - 28, x + 68, y - 42);
            for (int k = 0; k < 9; k++) p.E(x + (ArtPen.R01(i * 9 + k) - .5f) * 120f, y - 22f - ArtPen.R01(i * 7 + k + 3) * 50f, 5, 3, p.C(0x3E8E3E));
        }
        for (int s = 0; s < 4; s++) { var q = new List<Vector2>(); for (int i = 0; i <= 24; i++) { float k = i / 24f; q.Add(new((s - 1.5f) * 120f + ArtPen.Sin(k * 7f - t * 2f + s) * 20f, -110f - k * 210f)); } p.Line(p.C(0xFFFFFF, .4f), 7, q); }
    }
}

public sealed class DonutPlugin : QuickShapePlugin { public override string Name => "演出：ドーナツ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DonutParameter(s); }
public sealed class DonutParameter : Q4Art
{
    public DonutParameter() : this(null) { }
    public DonutParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x5B3220); Color2 = Hex(0xE6B273); }
    protected override string[] Use => ["Color1=チョコの色", "Color2=生地の色", "Speed=回る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.Turn(0, 0, t * .6f, () =>
        {
            p.Ring(0, 0, 190, p.C(Color2), 180);
            var ic = new List<Vector2>(); for (int i = 0; i < 64; i++) { float a = i / 64f * MathF.Tau; ic.Add(new(ArtPen.Cos(a) * (262f + ArtPen.Sin(a * 7f) * 14f), ArtPen.Sin(a) * (262f + ArtPen.Sin(a * 7f) * 14f))); }
            ic.Add(ic[0]); for (int i = 64; i >= 0; i--) { float a = i / 64f * MathF.Tau; ic.Add(new(ArtPen.Cos(a) * 112f, ArtPen.Sin(a) * 112f)); }
            p.Fill(p.C(Color1), ic);
            for (int i = 0; i < 44; i++) { float a = ArtPen.R01(i) * MathF.Tau, r = 120f + ArtPen.R01(i + 50) * 125f, rot = ArtPen.R01(i + 90) * 6f; var c = (i % 4) switch { 0 => p.C(0xFF6FA8), 1 => p.C(0xFFE45C), 2 => p.C(0x5CD6FF), _ => p.C(0xFFFFFF) }; float x = ArtPen.Cos(a) * r, y = ArtPen.Sin(a) * r; p.L(x - ArtPen.Cos(rot) * 11, y - ArtPen.Sin(rot) * 11, x + ArtPen.Cos(rot) * 11, y + ArtPen.Sin(rot) * 11, c, 8); }
        });
    }
}

public sealed class SoftCreamPlugin : QuickShapePlugin { public override string Name => "演出：ソフトクリーム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SoftCreamParameter(s); }
public sealed class SoftCreamParameter : Q4Art
{
    public SoftCreamParameter() : this(null) { }
    public SoftCreamParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFF7E6); Color2 = Hex(0xDFA25B); }
    protected override string[] Use => ["Color1=クリームの色", "Color2=コーンの色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.F(p.C(Color2), -135, 30, 135, 30, 0, 410); for (int i = -4; i <= 4; i++) { p.L(i * 32, 30, i * 32 + 75, 200, p.C(0xB9783A), 3); p.L(i * 32, 30, i * 32 - 75, 200, p.C(0xB9783A), 3); }
        float sw = ArtPen.Sin(t * 2f) * .035f;
        p.Turn(0, 30, sw, () =>
        {
            float[] w = [170, 140, 108, 74]; for (int i = 0; i < 4; i++) { float y = 8 - i * 74f; p.R(0, y, w[i] * 2, 88, 44, p.C(Color1)); p.Arc(0, y - 60, w[i] * 1.02f, .35f, 2.75f, p.C(0xE3D6BC), 5); }
            p.F(p.C(Color1), -42, -245, 42, -245, 8, -330, -2, -336);
        });
    }
}

public sealed class CustardPuddingPlugin : QuickShapePlugin { public override string Name => "演出：プリン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CustardPuddingParameter(s); }
public sealed class CustardPuddingParameter : Q4Art
{
    public CustardPuddingParameter() : this(null) { }
    public CustardPuddingParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFD978); Color2 = Hex(0x6B3414); }
    protected override string[] Use => ["Color1=プリンの色", "Color2=カラメルの色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.E(0, 200, 330, 52, p.C(0xF4F7FA)); p.EL(0, 200, 330, 52, p.C(0xB7C4D1), 4);
        float sx = ArtPen.Sin(t * 7f) * 22f * MathF.Exp(-ArtPen.Fr(t * .5f) * 2.4f);
        p.F(p.C(Color1), -205, 186, 205, 186, 130 + sx, -105, -130 + sx, -105); p.E(0, 186, 205, 26, p.C(Color1)); p.E(sx, -105, 130, 28, p.C(Color2));
        p.F(p.C(Color2), -130 + sx, -105, 130 + sx, -105, 138 + sx * .9f, -62, 92 + sx * .9f, -48, 70 + sx * .9f, -8, 38 + sx * .9f, -46, -22 + sx * .9f, -38, -56 + sx * .9f, 4, -86 + sx * .9f, -46, -138 + sx * .9f, -58);
        p.E(-42 + sx, -112, 44, 8, p.C(0xFFFFFF, .45f)); p.L(-160, 150, -116 + sx * .6f, -16, p.C(0xFFFFFF, .45f), 8);
    }
}

public sealed class CoffeeCupPlugin : QuickShapePlugin { public override string Name => "演出：コーヒーカップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CoffeeCupParameter(s); }
public sealed class CoffeeCupParameter : Q4Art
{
    public CoffeeCupParameter() : this(null) { }
    public CoffeeCupParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFAF6EF); Color2 = Hex(0x4B2A18); Count = 3; }
    protected override string[] Use => ["Color1=カップの色", "Color2=コーヒーの色", "Count=湯気の量||0|8", "Speed=湯気の速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.E(0, 215, 345, 48, p.C(0xE9E2D5)); p.EL(0, 215, 345, 48, p.C(0xB9AE99), 4); p.Ring(235, 45, 76, p.C(Color1), 28);
        p.F(p.C(Color1), -225, -30, 225, -30, 185, 165, 115, 214, -115, 214, -185, 165); p.E(0, -30, 225, 44, p.C(0xE8DFD0)); p.E(0, -28, 200, 34, p.C(Color2)); p.E(-55, -35, 62, 8, p.C(0x8A5A36, .8f));
        p.L(-160, 8, -130, 160, p.C(0xFFFFFF, .75f), 12);
        int n = Math.Clamp(Count, 0, 8); for (int s = 0; s < n; s++) { var q = new List<Vector2>(); for (int i = 0; i <= 24; i++) { float k = i / 24f; q.Add(new((s - (n - 1) / 2f) * 58f + ArtPen.Sin(k * 7f - t * 2f + s) * 22f, -70f - k * 260f)); } p.Line(p.C(0xFFFFFF, .46f), 8, q); }
    }
}

public sealed class FriedEggPlugin : QuickShapePlugin { public override string Name => "演出：目玉焼き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FriedEggParameter(s); }
public sealed class FriedEggParameter : Q4Art
{
    public FriedEggParameter() : this(null) { }
    public FriedEggParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFB300); Color2 = Hex(0x2B2E33); }
    protected override string[] Use => ["Color1=黄身の色", "Color2=フライパンの色", "Speed=焼ける速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.R(455, 30, 380, 54, 26, p.C(0x20140E)); p.D(0, 0, 330, p.C(Color2)); p.Ring(0, 0, 318, p.C(0x4B5057), 10);
        var eg = new List<Vector2>(); for (int i = 0; i < 48; i++) { float a = i / 48f * MathF.Tau, r = 225f + ArtPen.Sin(a * 3f + 1f) * 26f + ArtPen.Sin(a * 5f) * 15f + ArtPen.Sin(t * 5f + a * 6f) * 2.5f; eg.Add(new(ArtPen.Cos(a) * r, ArtPen.Sin(a) * r)); } p.Fill(p.C(0xFFFFFF), eg); p.Line(p.C(0xE2B87E, .8f), 9, eg.Append(eg[0]));
        p.D(14, -6, 94, p.C(Color1)); p.D(-16, -36, 24, p.C(0xFFFFFF, .75f));
        for (int i = 0; i < 16; i++) { float k = ArtPen.Fr(t * 1.8f + ArtPen.R01(i)), a = ArtPen.R01(i + 20) * MathF.Tau, r = 235f + k * 110f; p.D(ArtPen.Cos(a) * r, ArtPen.Sin(a) * r - k * 50f, 5f * (1f - k), p.C(0xFFF3C4, 1f - k)); }
    }
}

public sealed class ShavedIcePlugin : QuickShapePlugin { public override string Name => "演出：かき氷"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ShavedIceParameter(s); }
public sealed class ShavedIceParameter : Q4Art
{
    public ShavedIceParameter() : this(null) { }
    public ShavedIceParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xF03A5F); Color2 = Hex(0xBFE7FF); }
    protected override string[] Use => ["Color1=シロップの色", "Color2=器の色", "Speed=きらめきの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.Pie(0, 30, 290, MathF.PI, MathF.Tau, p.C(0xFAFDFF)); p.F(p.C(Color1, .88f), -205, -175, -110, -258, 0, -292, 112, -256, 206, -172, 150, -120, 118, -60, 72, -112, 26, -44, -34, -106, -80, -48, -130, -112, -170, -92);
        for (int i = 0; i < 14; i++) { float a = MathF.PI * (1.08f + ArtPen.R01(i) * .84f), r = 90f + ArtPen.R01(i + 8) * 190f, tw = MathF.Max(0f, ArtPen.Sin(t * 3f + i * 1.7f)); p.Star(ArtPen.Cos(a) * r, 30 + ArtPen.Sin(a) * r, 18f * tw, 4, .18f, 0, p.C(0xFFFFFF)); }
        p.F(p.C(Color2, .85f), -300, 30, 300, 30, 210, 190, -210, 190); p.E(0, 30, 300, 34, p.C(Color2)); p.F(p.C(Color2, .85f), -78, 190, 78, 190, 118, 280, -118, 280); p.L(-225, 60, -165, 165, p.C(0xFFFFFF, .75f), 12);
    }
}

public sealed class CatFacePlugin : QuickShapePlugin { public override string Name => "演出：ねこ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CatFaceParameter(s); }
public sealed class CatFaceParameter : Q4Art
{
    public CatFaceParameter() : this(null) { }
    public CatFaceParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xF0A24A); Color2 = Hex(0xFFF3E2); }
    protected override string[] Use => ["Color1=毛の色", "Color2=お腹の色", "Speed=動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float sw = ArtPen.Sin(t * 3f); var tail = new List<Vector2>(); for (int i = 0; i <= 18; i++) { float k = i / 18f; tail.Add(new(150 + k * 170 + ArtPen.Sin(k * 3f + t * 3f) * k * 45f, 240 - k * 190 + sw * k * 28f)); } p.Line(p.C(Color1), 42, tail);
        p.E(0, 170, 190, 170, p.C(Color1)); p.E(0, 200, 110, 125, p.C(Color2));
        foreach (int s in new[] { -1, 1 }) { p.E(s * 80, 325, 62, 34, p.C(Color2)); p.F(p.C(Color1), s * 205, -110, s * 195, -285, s * 62, -192); p.F(p.C(0xFF9FB0), s * 180, -135, s * 174, -240, s * 95, -188); }
        p.E(0, -70, 225, 180, p.C(Color1)); for (int i = -1; i <= 1; i++) p.L(i * 40, -240, i * 34, -180, p.C(0xB9702A), 13);
        bool blink = ArtPen.Fr(t * .3f) > .94f; foreach (int s in new[] { -1, 1 }) { if (blink) p.Arc(s * 92, -82, 32, MathF.PI * 1.1f, MathF.PI * 1.9f, p.C(0x20140E), 7); else { p.D(s * 92, -80, 34, p.C(0xFFFFFF)); p.E(s * 92, -80, 15, 29, p.C(0x20140E)); p.D(s * 86, -92, 7, p.C(0xFFFFFF)); } for (int k = -1; k <= 1; k++) p.L(s * 130, -22 + k * 14, s * 262, -38 + k * 36, p.C(0x20140E), 3); }
        p.F(p.C(0xFF7F92), -15, -32, 15, -32, 0, -14); p.Arc(-18, -10, 18, .2f, MathF.PI - .2f, p.C(0x20140E), 5); p.Arc(18, -10, 18, .2f, MathF.PI - .2f, p.C(0x20140E), 5);
    }
}

public sealed class DogFacePlugin : QuickShapePlugin { public override string Name => "演出：いぬ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DogFaceParameter(s); }
public sealed class DogFaceParameter : Q4Art
{
    public DogFaceParameter() : this(null) { }
    public DogFaceParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xD9A066); Color2 = Hex(0xFFF3E2); }
    protected override string[] Use => ["Color1=毛の色", "Color2=口元の色", "Speed=動く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float w = ArtPen.Sin(t * 12f); p.Turn(150, 195, w * .55f - .5f, () => p.R(235, 195, 170, 44, 22, p.C(Color1)));
        p.E(0, 195, 185, 165, p.C(Color1)); p.E(0, 215, 105, 120, p.C(Color2)); foreach (int s in new[] { -1, 1 }) p.E(s * 90, 335, 62, 32, p.C(Color2));
        p.E(0, -70, 220, 190, p.C(Color1)); foreach (int s in new[] { -1, 1 }) { p.Turn(s * 190, -170, s * (.28f + ArtPen.Sin(t * 3f) * .06f), () => p.E(s * 205, -60, 62, 135, p.C(0x8A5A36))); p.D(s * 82, -100, 28, p.C(0x20140E)); p.D(s * 76, -110, 8, p.C(0xFFFFFF)); }
        p.E(0, 5, 112, 84, p.C(Color2)); p.E(0, -28, 38, 27, p.C(0x20140E)); p.L(0, -4, 0, 28, p.C(0x20140E), 5); p.Arc(-34, 22, 34, .15f, MathF.PI - .3f, p.C(0x20140E), 5); p.Arc(34, 22, 34, .3f, MathF.PI - .15f, p.C(0x20140E), 5);
        float l = 44f + MathF.Abs(ArtPen.Sin(t * 8f)) * 28f; p.R(0, 44 + l / 2f, 54, l, 26, p.C(0xFF7F92)); p.L(0, 50, 0, 40 + l, p.C(0xD95A70), 3);
    }
}

public sealed class BunnyHopPlugin : QuickShapePlugin { public override string Name => "演出：うさぎ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BunnyHopParameter(s); }
public sealed class BunnyHopParameter : Q4Art
{
    public BunnyHopParameter() : this(null) { }
    public BunnyHopParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFFA9BC); }
    protected override string[] Use => ["Color1=体の色", "Color2=耳の内側の色", "Speed=跳ねる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float h = MathF.Abs(ArtPen.Sin(t * 3.5f)), y = -h * 150f; p.E(0, 335, 155 - h * 55, 22, p.C(0x000000, .22f));
        float sq = MathF.Pow(1f - h, 4f) * .14f;
        p.Move(0, y, () => p.Zoom(0, 320, 1f + sq, 1f - sq, () =>
        {
            p.D(165, 225, 42, p.C(Color1)); p.E(0, 200, 170, 135, p.C(Color1)); p.E(-85, 310, 70, 28, p.C(Color1)); p.E(85, 310, 70, 28, p.C(Color1));
            foreach (int s in new[] { -1, 1 }) p.Turn(s * 60, -130, s * (.12f + h * .28f), () => { p.E(s * 60, -285, 48, 155, p.C(Color1)); p.E(s * 60, -275, 24, 120, p.C(Color2)); });
            p.E(0, -30, 170, 140, p.C(Color1)); foreach (int s in new[] { -1, 1 }) { p.D(s * 70, -50, 22, p.C(0xB71C3C)); p.D(s * 64, -58, 6, p.C(0xFFFFFF)); p.E(s * 112, 14, 26, 15, p.C(Color2, .7f)); }
            p.F(p.C(Color2), -13, -6, 13, -6, 0, 10); p.Arc(-14, 12, 14, .2f, MathF.PI - .2f, p.C(0x444444), 4); p.Arc(14, 12, 14, .2f, MathF.PI - .2f, p.C(0x444444), 4);
        }));
    }
}

public sealed class PenguinWalkPlugin : QuickShapePlugin { public override string Name => "演出：ペンギン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PenguinWalkParameter(s); }
public sealed class PenguinWalkParameter : Q4Art
{
    public PenguinWalkParameter() : this(null) { }
    public PenguinWalkParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x20293A); Color2 = Hex(0xFFA726); }
    protected override string[] Use => ["Color1=背中の色", "Color2=くちばしの色", "Speed=歩く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float w = ArtPen.Sin(t * 5f); p.Turn(0, 330, w * .13f, () =>
        {
            foreach (int s in new[] { -1, 1 }) p.E(s * 74, 330 - MathF.Max(0f, s * w) * 26, 66, 25, p.C(Color2));
            p.E(0, 70, 205, 270, p.C(Color1)); p.E(0, 115, 145, 205, p.C(0xFFFFFF));
            foreach (int s in new[] { -1, 1 }) { p.Turn(s * 190, -40, s * (.25f + s * w * .28f), () => p.E(s * 205, 70, 40, 130, p.C(Color1))); p.D(s * 64, -70, 34, p.C(0xFFFFFF)); p.D(s * 58, -70, 17, p.C(0x101010)); p.D(s * 52, -76, 5, p.C(0xFFFFFF)); p.E(s * 118, -10, 22, 13, p.C(0xFFA9BC, .75f)); }
            p.F(p.C(Color2), -38, -32, 38, -32, 0, 18);
        });
    }
}

public sealed class PandaBambooPlugin : QuickShapePlugin { public override string Name => "演出：パンダ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PandaBambooParameter(s); }
public sealed class PandaBambooParameter : Q4Art
{
    public PandaBambooParameter() : this(null) { }
    public PandaBambooParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x1B1B20); Color2 = Hex(0x56A84B); }
    protected override string[] Use => ["Color1=黒い毛の色", "Color2=笹の色", "Speed=食べる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float chew = MathF.Abs(ArtPen.Sin(t * 5f)); p.E(0, 220, 225, 170, p.C(0xF8F8F4)); p.R(-140, 325, 120, 72, 34, p.C(Color1)); p.R(140, 325, 120, 72, 34, p.C(Color1));
        p.L(150, 250, 300, -150, p.C(Color2), 16); for (int i = 0; i < 5; i++) { float k = i / 4f, x = 195 + k * 105, y = 130 - k * 280; p.F(p.C(Color2), x, y, x + 120, y - 66 + ArtPen.Sin(t * 2 + i) * 8, x + 28, y + 16); }
        p.E(-185, 190, 62, 112, p.C(Color1)); p.E(180, 190, 64, 92, p.C(Color1)); foreach (int s in new[] { -1, 1 }) p.D(s * 168, -205, 66, p.C(Color1));
        p.E(0, -55, 230, 190, p.C(0xFFFFFF)); foreach (int s in new[] { -1, 1 }) p.Turn(s * 88, -75, s * -.45f, () => { p.E(s * 88, -75, 46, 62, p.C(Color1)); p.D(s * 88, -80, 19, p.C(0xFFFFFF)); p.D(s * 90, -80, 11, p.C(0x101010)); });
        p.E(0, -6, 26, 17, p.C(Color1)); p.E(0, 44 + chew * 6, 34, 9 + chew * 20, p.C(0xB23A48));
    }
}

public sealed class ChickHopPlugin : QuickShapePlugin { public override string Name => "演出：ひよこ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ChickHopParameter(s); }
public sealed class ChickHopParameter : Q4Art
{
    public ChickHopParameter() : this(null) { }
    public ChickHopParameter(SharedDataStore? s) : base(s) { Text = "ぴよ"; Color1 = Hex(0xFFE04F); Color2 = Hex(0xFF8F2B); }
    protected override string[] Use => ["Text=鳴き声", "Color1=体の色", "Color2=くちばしの色", "Speed=跳ねる速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float h = MathF.Abs(ArtPen.Sin(t * 5f)); p.E(0, 275, 150 - h * 50, 20, p.C(0x000000, .2f));
        p.Move(0, -h * 90, () =>
        {
            foreach (int s in new[] { -1, 1 }) { p.L(s * 60, 220, s * 60, 265, p.C(Color2), 11); p.P(p.C(Color2), 9, s * 26, 278, s * 60, 262, s * 96, 278); }
            p.D(0, 40, 200, p.C(Color1)); foreach (int i in new[] { -1, 0, 1 }) p.E(i * 22, -165, 12, 34, p.C(Color1)); foreach (int s in new[] { -1, 1 }) { p.Turn(s * 185, 60, s * (-.2f - h * .6f), () => p.E(s * 215, 80, 42, 76, p.C(0xF7C92F))); p.D(s * 72, -5, 22, p.C(0x20140E)); p.D(s * 66, -13, 7, p.C(0xFFFFFF)); p.E(s * 118, 45, 26, 15, p.C(0xFF9F9F, .7f)); }
            p.F(p.C(Color2), -32, 20, 32, 20, 0, 66);
        });
        p.T(Str, 62, 255, -220 + ArtPen.Sin(t * 5f) * 12f, p.C(0xFFFFFF), p.C(Color2), 4);
    }
}

public sealed class FrogCroakPlugin : QuickShapePlugin { public override string Name => "演出：カエル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new FrogCroakParameter(s); }
public sealed class FrogCroakParameter : Q4Art
{
    public FrogCroakParameter() : this(null) { }
    public FrogCroakParameter(SharedDataStore? s) : base(s) { Text = "ケロ"; Color1 = Hex(0x57B947); Color2 = Hex(0xE9F5C8); }
    protected override string[] Use => ["Text=鳴き声", "Color1=体の色", "Color2=お腹の色", "Speed=鳴く速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float cr = MathF.Max(0f, ArtPen.Sin(t * 4f)), hop = MathF.Pow(MathF.Max(0f, ArtPen.Sin(t * 1.2f)), 20f) * 130f;
        p.Move(0, -hop, () =>
        {
            foreach (int s in new[] { -1, 1 }) { p.E(s * 205, 225, 105, 58, p.C(Color1)); p.P(p.C(Color1), 18, s * 105, 250, s * 130, 300, s * 95, 310); p.P(p.C(Color1), 10, s * 235, 275, s * 290, 300); p.P(p.C(Color1), 10, s * 235, 275, s * 270, 318); }
            p.E(0, 120, 225, 175, p.C(Color1)); p.E(0, 175, 145, 105, p.C(Color2)); p.E(0, 70 + cr * 42, 88 + cr * 62, 24 + cr * 70, p.C(0xFFF7D0, .94f));
            foreach (int s in new[] { -1, 1 }) { p.D(s * 110, -55, 76, p.C(Color1)); p.D(s * 110, -60, 52, p.C(0xFFFFFF)); p.E(s * 110, -58, 30, 18, p.C(0x101010)); p.D(s * 118, -66, 7, p.C(0xFFFFFF)); }
            p.Arc(0, -40, 140, .5f, MathF.PI - .5f, p.C(0x25621E), 7);
        });
        if (cr > .3f) p.T(Str, 70, 300, -190, p.C(0xFFFFFF, cr), p.C(0x25621E, cr), 4);
    }
}

public sealed class HamsterWheelPlugin : QuickShapePlugin { public override string Name => "演出：ハムスターの回し車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HamsterWheelParameter(s); }
public sealed class HamsterWheelParameter : Q4Art
{
    public HamsterWheelParameter() : this(null) { }
    public HamsterWheelParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xE9B872); Color2 = Hex(0x5D9CEC); }
    protected override string[] Use => ["Color1=ハムスターの色", "Color2=回し車の色", "Speed=走る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.F(p.C(0x8A97A3), -20, 0, 20, 0, 130, 390, 90, 390); p.F(p.C(0x8A97A3), -20, 0, 20, 0, -90, 390, -130, 390); p.R(0, 395, 360, 22, 8, p.C(0x6B7680));
        p.Ring(0, 0, 340, p.C(Color2), 16); p.Ring(0, 0, 300, p.C(Color2), 8); float rot = t * 3f; for (int i = 0; i < 36; i++) { float a = rot + i * MathF.Tau / 36f; p.L(ArtPen.Cos(a) * 300, ArtPen.Sin(a) * 300, ArtPen.Cos(a) * 340, ArtPen.Sin(a) * 340, p.C(0xDDEBFA), 5); } for (int i = 0; i < 6; i++) { float a = rot + i * MathF.Tau / 6f; p.L(0, 0, ArtPen.Cos(a) * 300, ArtPen.Sin(a) * 300, p.C(Color2, .55f), 7); } p.D(0, 0, 26, p.C(0x6B7680));
        float b = MathF.Abs(ArtPen.Sin(t * 14f)) * 12f;
        p.Move(0, 185 - b, () =>
        {
            foreach (int s in new[] { -1, 1 }) { float leg = ArtPen.Sin(t * 14f + s) * 34f; p.E(s * 58 + leg, 80, 24, 13, p.C(0xFFB3B3)); }
            p.E(0, 0, 135, 90, p.C(Color1)); p.E(10, 30, 95, 52, p.C(0xFFF3E2)); p.D(112, -46, 66, p.C(Color1)); p.D(82, -105, 25, p.C(0xFFB3B3)); p.D(148, -103, 23, p.C(0xFFB3B3)); p.D(135, -56, 11, p.C(0x101010)); p.D(171, -30, 7, p.C(0xFF7F92)); p.E(112, -14, 20, 13, p.C(0xFFFFFF));
        });
    }
}

public sealed class CrowFlockPlugin : QuickShapePlugin { public override string Name => "演出：カラスが飛ぶ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new CrowFlockParameter(s); }
public sealed class CrowFlockParameter : Q4Art
{
    public CrowFlockParameter() : this(null) { }
    public CrowFlockParameter(SharedDataStore? s) : base(s) { Text = "アホー"; Count = 5; Color1 = Hex(0x15151A); }
    protected override string[] Use => ["Text=鳴き声", "Count=数||1|24", "Color1=カラスの色", "Speed=飛ぶ速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        int n = Math.Clamp(Count, 1, 24); float span = p.W + 520f;
        for (int i = 0; i < n; i++)
        {
            float x = ArtPen.Fr(t * .09f + ArtPen.R01(i) * .25f + i * .035f) * span - span / 2f, y = (ArtPen.R01(i + 20) - .5f) * 390f + ArtPen.Sin(t * 1.5f + i) * 18f, s = .6f + ArtPen.R01(i + 40) * .75f, f = ArtPen.Sin(t * 8f + i * 1.3f) * 62f * s;
            p.E(x, y, 56 * s, 22 * s, p.C(Color1)); p.D(x + 52 * s, y - 13 * s, 20 * s, p.C(Color1)); p.F(p.C(Color1), x + 66 * s, y - 20 * s, x + 104 * s, y - 8 * s, x + 66 * s, y - 2 * s); p.F(p.C(Color1), x - 44 * s, y - 8 * s, x - 112 * s, y - 16 * s, x - 100 * s, y + 18 * s); p.D(x + 58 * s, y - 18 * s, 3.5f * s, p.C(0xFFFFFF));
            p.F(p.C(Color1), x - 28 * s, y - 8 * s, x + 26 * s, y - 8 * s, x - 22 * s, y - 12 * s - f, x - 76 * s, y - 2 * s - f * .75f);
            if (i == 0) p.T(Str, 48, x + 25, y - 92, p.C(0xFFFFFF), p.C(Color1), 4);
        }
    }
}

public sealed class HoneyBeePlugin : QuickShapePlugin { public override string Name => "演出：ミツバチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HoneyBeeParameter(s); }
public sealed class HoneyBeeParameter : Q4Art
{
    public HoneyBeeParameter() : this(null) { }
    public HoneyBeeParameter(SharedDataStore? s) : base(s) { Count = 2; Color1 = Hex(0xFFD033); Color2 = Hex(0xFF7FA8); }
    protected override string[] Use => ["Count=ハチの数||1|8", "Color1=体の色", "Color2=花の色", "Speed=飛ぶ速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        p.L(0, 110, 0, 390, p.C(0x4A9A45), 14); p.E(70, 300, 72, 26, p.C(0x4A9A45)); for (int i = 0; i < 8; i++) { float a = i * MathF.Tau / 8f; p.E(ArtPen.Cos(a) * 82, 80 + ArtPen.Sin(a) * 82, 50, 50, p.C(Color2)); } p.D(0, 80, 52, p.C(0xFFC23D));
        int n = Math.Clamp(Count, 1, 8);
        for (int i = 0; i < n; i++)
        {
            float ph = t * 1.2f + i * MathF.Tau / n, x = ArtPen.Sin(ph) * 330, y = 60 + ArtPen.Sin(ph * 2f) * 130, dir = ArtPen.Cos(ph) >= 0f ? 1f : -1f, f = MathF.Abs(ArtPen.Sin(t * 35f + i));
            for (int k = 1; k <= 8; k++) { float q = ph - k * .09f; p.D(ArtPen.Sin(q) * 330, 60 + ArtPen.Sin(q * 2f) * 130, 3.5f, p.C(0xFFFFFF, .6f * (1f - k / 9f))); }
            p.E(x - dir * 12, y - 48, 46, 22 + f * 34, p.C(0xDFF4FF, .72f)); p.E(x + dir * 22, y - 42, 34, 16 + f * 26, p.C(0xDFF4FF, .72f));
            p.E(x, y, 66, 44, p.C(Color1)); for (int k = -1; k <= 1; k++) p.B(x + k * 26 * dir - dir * 6, y, 13, 76f - MathF.Abs(k) * 18f, p.C(0x241A12)); p.F(p.C(0x241A12), x - dir * 62, y - 9, x - dir * 88, y, x - dir * 62, y + 9); p.D(x + dir * 50, y - 8, 8, p.C(0x101010));
        }
    }
}

public sealed class DrivingCarPlugin : QuickShapePlugin { public override string Name => "演出：走る車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new DrivingCarParameter(s); }
public sealed class DrivingCarParameter : Q4Art
{
    public DrivingCarParameter() : this(null) { }
    public DrivingCarParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xE53935); Background = true; }
    protected override string[] Use => ["Color1=車の色", "Background=道路を描く", "Speed=走る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float wr = 62f, by = 150f; if (Background) { p.B(0, 270, p.W + 800, 160, p.C(0x4A4E55)); float off = -ArtPen.Fr(t * 2f) * 220f; for (int i = -7; i <= 8; i++) p.B(i * 220 + off, 270, 116, 9, p.C(0xFFFFFF)); p.B(0, 188, p.W + 800, 7, p.C(0xD7DBE0)); }
        float b = ArtPen.Sin(t * 19f) * 3f;
        p.Move(0, b, () =>
        {
            p.F(p.C(Color1), -335, 120, -335, 40, -220, 26, -125, -72, 85, -72, 180, 22, 320, 48, 348, 90, 348, 138, -335, 138); p.F(p.C(0xBFE7FA), -188, 24, -108, -48, -20, -48, -20, 24); p.F(p.C(0xBFE7FA), 8, -48, 72, -48, 138, 24, 8, 24); p.L(-6, -70, -6, 130, p.C(0x7A1C1A), 4); p.R(55, 58, 50, 10, 5, p.C(0x7A1C1A)); p.E(340, 72, 12, 18, p.C(0xFFF2A8)); p.B(-333, 74, 8, 26, p.C(0xFF4D4D));
        });
        foreach (float x in new[] { -205f, 205f }) { p.D(x, by, wr, p.C(0x1D1E22)); p.D(x, by, wr * .55f, p.C(0xC7CCD3)); for (int i = 0; i < 5; i++) { float a = t * 14f + i * MathF.Tau / 5f; p.L(x, by, x + ArtPen.Cos(a) * wr * .5f, by + ArtPen.Sin(a) * wr * .5f, p.C(0x5A5F67), 5); } }
        for (int i = 0; i < 8; i++) { float k = ArtPen.Fr(t * 1.4f + ArtPen.R01(i)); p.D(-370 - k * 240, 130 - k * 30 + (ArtPen.R01(i + 8) - .5f) * 30, 10 + k * 26, p.C(0xCFCFCF, .34f * (1f - k))); }
    }
}

public sealed class PassingTrainPlugin : QuickShapePlugin { public override string Name => "演出：電車が走る"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new PassingTrainParameter(s); }
public sealed class PassingTrainParameter : Q4Art
{
    public PassingTrainParameter() : this(null) { }
    public PassingTrainParameter(SharedDataStore? s) : base(s) { Count = 4; Color1 = Hex(0x2E9E6B); Color2 = Hex(0xF4F6F5); }
    protected override string[] Use => ["Count=車両の数||1|10", "Variant=向き|1=右へ　2=左へ|1|2", "Color1=帯の色", "Color2=車体の色", "Speed=走る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        int n = Math.Clamp(Count, 1, 10); const float carW = 460f; float dir = Variant == 2 ? -1f : 1f, total = n * carW, span = p.W + total + 200f, x0 = ArtPen.Fr(t * .13f) * span - p.W / 2f - total - 100f;
        p.B(0, 188, p.W + 800, 10, p.C(0x70757B)); for (int i = -22; i <= 22; i++) p.B(i * 58f, 202, 34, 14, p.C(0x5B4A3E));
        for (int c = 0; c < n; c++)
        {
            float x = (x0 + c * carW + carW / 2f) * dir, shake = ArtPen.Sin(t * 26f + c) * 1.8f; p.R(x, 40 + shake, carW - 12, 240, 18, p.C(Color2)); p.B(x, 112 + shake, carW - 12, 34, p.C(Color1)); p.B(x, 138 + shake, carW - 12, 8, p.C(0x8F9690));
            for (int w = -2; w <= 2; w++) { if (w is -2 or 2 or 0) p.R(x + w * 90, 58 + shake, 54, 120, 4, p.C(0x3E4A52)); else p.R(x + w * 90, 12 + shake, 100, 66, 6, p.C(0xA8DAEF)); }
            foreach (int s in new[] { -1, 1 }) { p.B(x + s * 150, 168, 110, 26, p.C(0x2A2C31)); p.D(x + s * 180, 176, 17, p.C(0x17181B)); p.D(x + s * 120, 176, 17, p.C(0x17181B)); } p.B(x, -86 + shake, carW - 40, 10, p.C(0x70757B));
            if (c == n - 1) { float nx = x + dir * (carW / 2f - 8f); p.E(nx, 80 + shake, 11, 15, p.C(0xFFF2A8)); }
        }
    }
}

public sealed class ContrailPlanePlugin : QuickShapePlugin { public override string Name => "演出：飛行機雲"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new ContrailPlaneParameter(s); }
public sealed class ContrailPlaneParameter : Q4Art
{
    public ContrailPlaneParameter() : this(null) { }
    public ContrailPlaneParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x2F8FE6); Background = true; }
    protected override bool Scene => true;
    protected override string[] Use => ["Background=空を塗る", "Color1=雲の色", "Color2=空の色", "Speed=飛ぶ速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) { p.B(0, 0, p.W + 4, p.H + 4, p.C(Color2)); p.B(0, p.H * .32f, p.W + 4, p.H * .36f, p.C(0xBFE8FF, .55f)); }
        float k = ArtPen.Fr(t * .09f), x0 = -p.W / 2f - 150f, x = x0 + k * (p.W + 300f), y0 = 300f, y = y0 - k * 620f;
        for (int i = 0; i < 90; i++) { float q = i / 90f, tx = x - q * q * 1100f - 75f; if (tx < x0) break; float ty = y0 + (tx - x0) * (y - y0) / MathF.Max(1f, x - x0) + 4f, r = 7f + q * 24f + ArtPen.Sin(i * 1.9f) * 3f; p.D(tx, ty - 6, r, p.C(Color1, .78f * (1f - q))); p.D(tx, ty + 6, r * .86f, p.C(Color1, .55f * (1f - q))); }
        p.Turn(x, y, -.5f, () => { p.R(x, y, 150, 22, 11, p.C(0xF3F6FA)); p.F(p.C(0x516F8E), x - 12, y, x + 34, y, x - 45, y + 48, x - 66, y + 48); p.F(p.C(0x516F8E), x - 72, y - 4, x - 52, y - 4, x - 82, y - 40, x - 92, y - 40); });
    }
}

public sealed class HelicopterPlugin : QuickShapePlugin { public override string Name => "演出：ヘリコプター"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HelicopterParameter(s); }
public sealed class HelicopterParameter : Q4Art
{
    public HelicopterParameter() : this(null) { }
    public HelicopterParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x2F6FBB); Color2 = Hex(0xBFE7FA); }
    protected override string[] Use => ["Color1=機体の色", "Color2=窓の色", "Speed=回る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float hov = ArtPen.Sin(t * 1.6f) * 18f, tilt = ArtPen.Sin(t * .9f) * .05f;
        p.Move(0, hov, () => p.Turn(0, 0, tilt, () =>
        {
            p.F(p.C(Color1), -120, -18, -470, -44, -470, 2, -120, 62); p.F(p.C(Color1), -440, -38, -495, -130, -445, -130, -395, -38);
            float a = t * 28f; p.L(-470 + ArtPen.Cos(a) * 70, -20 + ArtPen.Sin(a) * 70, -470 - ArtPen.Cos(a) * 70, -20 - ArtPen.Sin(a) * 70, p.C(0x25282D, .85f), 9); p.D(-470, -20, 11, p.C(0x25282D));
            p.E(50, 25, 235, 128, p.C(Color1)); p.F(p.C(Color2), 62, -82, 150, -66, 262, 26, 62, 26); p.L(40, -100, 40, 110, p.C(0x1F4F87), 4);
            p.B(0, -123, 30, 44, p.C(0x25282D)); float w = 370f * MathF.Abs(ArtPen.Cos(t * 34f)) + 45f; p.E(0, -152, 410, 13, p.C(0x9AA3AE, .3f)); p.R(0, -152, w * 2, 11, 5, p.C(0x7B848F)); p.E(0, -152, 28, 15, p.C(0x25282D));
            foreach (int s in new[] { -1, 1 }) p.L(s * 80 + 40, 140, s * 96 + 40, 190, p.C(0x25282D), 8); p.R(30, 195, 330, 13, 6, p.C(0x25282D));
        }));
    }
}

public sealed class RocketLaunchPlugin : QuickShapePlugin { public override string Name => "演出：ロケット発射"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RocketLaunchParameter(s); }
public sealed class RocketLaunchParameter : Q4Art
{
    public RocketLaunchParameter() : this(null) { }
    public RocketLaunchParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xF2F4F7); Color2 = Hex(0xE53935); Color3 = Hex(0xFF9D2E); }
    protected override string[] Use => ["Color1=機体の色", "Color2=先端の色", "Color3=炎の色", "Speed=速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float q = MathF.Max(0f, t - .55f), rise = q * q * 110f, shake = t < .55f ? ArtPen.Sin(t * 60f) * 5f * (t / .55f) : ArtPen.Sin(t * 45f) * 2f, by = 260f - rise;
        for (int i = 0; i < 46; i++) { float k = ArtPen.Fr(t * .9f + ArtPen.R01(i)), side = (ArtPen.R01(i + 7) - .5f) * 2f, r = (28f + k * 86f) * ArtPen.Cl(t * 2f); p.D(side * (40f + k * 420f), 410f - k * k * 60f + ArtPen.R01(i + 9) * 20f, r, p.C(0xE3E5E8, .46f * (1f - k))); }
        if (t > .2f) { float fl = 1f + ArtPen.Sin(t * 41f) * .14f, ls = ArtPen.Cl((t - .2f) * 3f); p.F(p.C(Color3), shake - 58, by + 92, shake + 58, by + 92, shake + 22, by + 92 + 245 * fl * ls, shake, by + 92 + 310 * fl * ls, shake - 22, by + 92 + 245 * fl * ls); p.F(p.C(0xFFF2A8), shake - 30, by + 92, shake + 30, by + 92, shake, by + 92 + 170 * fl * ls); }
        p.Move(shake, -rise, () =>
        {
            foreach (int s in new[] { -1, 1 }) p.F(p.C(Color2), s * 62, 170, s * 150, 355, s * 62, 325);
            p.F(p.C(Color1), -82, 345, -82, -55, -46, -175, 0, -255, 46, -175, 82, -55, 82, 345); p.F(p.C(Color2), -46, -175, 0, -255, 46, -175, 66, -115, -66, -115); p.D(0, 20, 40, p.C(0x37474F)); p.D(0, 20, 28, p.C(0x8ED8F8)); p.B(0, 250, 164, 14, p.C(Color2)); p.R(0, 358, 116, 34, 7, p.C(0x37474F));
        });
    }
}

public sealed class SailingShipPlugin : QuickShapePlugin { public override string Name => "演出：帆船"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SailingShipParameter(s); }
public sealed class SailingShipParameter : Q4Art
{
    public SailingShipParameter() : this(null) { }
    public SailingShipParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0x7A4A25); Color2 = Hex(0xFFF9EA); Color3 = Hex(0x2F86C6); }
    protected override string[] Use => ["Color1=船体の色", "Color2=帆の色", "Color3=海の色", "Speed=揺れの速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        void Sea(float y, float amp, float ph, Color4 c) { var q = new List<Vector2> { new(-p.W, 620), new(p.W, 620) }; for (int i = 40; i >= 0; i--) { float x = -p.W + i / 40f * p.W * 2f; q.Add(new(x, y + ArtPen.Sin(x * .012f + t * 1.6f + ph) * amp)); } p.Fill(c, q); }
        Sea(210, 16, 1.6f, p.C(Color3, .7f)); float roll = ArtPen.Sin(t * 1.3f) * .07f, by = ArtPen.Sin(t * 1.6f) * 12f;
        p.Move(0, by, () => p.Turn(0, 200, roll, () =>
        {
            p.F(p.C(Color1), -320, 110, 340, 80, 245, 245, -230, 245); p.L(-300, 145, 305, 122, p.C(0xD9A866), 6); foreach (int x in new[] { -130, 110 }) p.L(x, 115, x, -310, p.C(0x4E2F16), 15); p.L(-310, 92, 350, 62, p.C(0x4E2F16), 4);
            foreach (int x in new[] { -130, 110 }) { for (int r = 0; r < 2; r++) { float top = -285 + r * 180, h = 155, bw = 118 + r * 22, bulge = 28 + ArtPen.Sin(t * 1.8f + r + x) * 8f; var sail = new List<Vector2>(); for (int i = 0; i <= 12; i++) { float k = i / 12f; sail.Add(new(x + bw + ArtPen.Sin(k * MathF.PI) * bulge, top + k * h)); } for (int i = 12; i >= 0; i--) { float k = i / 12f; sail.Add(new(x - bw + ArtPen.Sin(k * MathF.PI) * bulge, top + k * h)); } p.Fill(p.C(Color2), sail); p.L(x - bw, top, x + bw, top, p.C(0x4E2F16), 7); } }
            p.F(p.C(0xD7263D), 110, -345, 188, -322 + ArtPen.Sin(t * 5f) * 7f, 110, -298);
        }));
        Sea(235, 20, 0, p.C(Color3));
    }
}

public sealed class BicycleRidePlugin : QuickShapePlugin { public override string Name => "演出：自転車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new BicycleRideParameter(s); }
public sealed class BicycleRideParameter : Q4Art
{
    public BicycleRideParameter() : this(null) { }
    public BicycleRideParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xE53935); }
    protected override string[] Use => ["Color1=フレームの色", "Speed=こぐ速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        const float wy = 130, R = 165; float ang = t * 4f; foreach (float x in new[] { -270f, 270f }) { p.Ring(x, wy, R, p.C(0x23252A), 15); p.Ring(x, wy, R - 13, p.C(0xB9BEC6), 3); for (int i = 0; i < 16; i++) { float a = ang + i * MathF.Tau / 16f; p.L(x, wy, x + ArtPen.Cos(a) * (R - 10), wy + ArtPen.Sin(a) * (R - 10), p.C(0x9CA3AC), 2); } p.D(x, wy, 13, p.C(0x50555C)); }
        var c = new Vector2(-25, wy); var st = new Vector2(-105, -95); var hd = new Vector2(180, -130); p.Ring(c.X, c.Y, 44, p.C(0x50555C), 8); p.L(-270, wy, c.X, c.Y, p.C(0x50555C), 6); p.L(-270, wy, st.X, st.Y, p.C(Color1), 11); p.L(st.X, st.Y, c.X, c.Y, p.C(Color1), 14); p.L(c.X, c.Y, hd.X, hd.Y, p.C(Color1), 14); p.L(st.X, st.Y, hd.X, hd.Y, p.C(Color1), 14); p.L(hd.X, hd.Y, 270, wy, p.C(Color1), 11); p.L(hd.X, hd.Y, 165, -205, p.C(0x50555C), 10); p.P(p.C(0x23252A), 13, 110, -222, 165, -207, 235, -220); p.L(st.X, st.Y, -122, -150, p.C(0x50555C), 10); p.R(-135, -166, 150, 30, 15, p.C(0x23252A));
        foreach (int s in new[] { -1, 1 }) { float px = c.X + ArtPen.Cos(ang) * 64 * s, py = c.Y + ArtPen.Sin(ang) * 64 * s; p.L(c.X, c.Y, px, py, p.C(0xB9BEC6), 8); p.R(px, py, 62, 14, 5, p.C(0x23252A)); }
    }
}

public sealed class HotAirBalloonPlugin : QuickShapePlugin { public override string Name => "演出：熱気球"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new HotAirBalloonParameter(s); }
public sealed class HotAirBalloonParameter : Q4Art
{
    public HotAirBalloonParameter() : this(null) { }
    public HotAirBalloonParameter(SharedDataStore? s) : base(s) { Count = 3; Color1 = Hex(0xE53935); Color2 = Hex(0xFFD54A); Background = false; }
    protected override bool Scene => true;
    protected override string[] Use => ["Count=数||1|12", "Background=空を塗る", "Color1=色1", "Color2=色2", "Speed=昇る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        if (Background) { p.B(0, -p.H * .2f, p.W + 4, p.H * .62f, p.C(0x69B9EC)); p.B(0, p.H * .3f, p.W + 4, p.H * .42f, p.C(0xD4F0FF)); }
        int n = Math.Clamp(Count, 1, 12); for (int b = n - 1; b >= 0; b--)
        {
            float d = .5f + (b + 1f) / n * .7f, S = 160f * d, span = p.H + S * 5f, y = p.H / 2f + S * 2.2f - ArtPen.Fr(t * .035f * d + ArtPen.R01(b + 3)) * span, x = (n == 1 ? 0 : (b + .5f) / n - .5f) * p.W * .9f + ArtPen.Sin(t * .5f + b) * 22f; var c1 = b % 2 == 0 ? Color1 : Color2; var c2 = b % 2 == 0 ? Color2 : Color1;
            for (int s = -3; s <= 3; s++) { float a0 = (s - .5f) / 7f * MathF.PI, a1 = (s + .5f) / 7f * MathF.PI; var q = new List<Vector2>(); for (int i = 0; i <= 18; i++) { float k = -MathF.PI / 2f + i / 18f * MathF.PI * .82f; q.Add(new(x + MathF.Sin(a0) * ArtPen.Cos(k) * S, y + ArtPen.Sin(k) * S * 1.08f)); } for (int i = 18; i >= 0; i--) { float k = -MathF.PI / 2f + i / 18f * MathF.PI * .82f; q.Add(new(x + MathF.Sin(a1) * ArtPen.Cos(k) * S, y + ArtPen.Sin(k) * S * 1.08f)); } p.Fill(p.C(((s + 3) % 2 == 0) ? c1 : c2), q); }
            p.F(p.C(c1), x - S * .42f, y + S * .92f, x + S * .42f, y + S * .92f, x + S * .15f, y + S * 1.34f, x - S * .15f, y + S * 1.34f); foreach (int s in new[] { -1, 1 }) p.L(x + s * S * .15f, y + S * 1.34f, x + s * S * .13f, y + S * 1.62f, p.C(0x5A3A22), 3); p.R(x, y + S * 1.74f, S * .34f, S * .28f, 3, p.C(0x8A5A32));
        }
    }
}

public sealed class RotatingBeaconPlugin : QuickShapePlugin { public override string Name => "演出：回転灯（パトランプ）"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new RotatingBeaconParameter(s); }
public sealed class RotatingBeaconParameter : Q4Art
{
    public RotatingBeaconParameter() : this(null) { }
    public RotatingBeaconParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFF2020); }
    protected override string[] Use => ["Color1=光の色", "Speed=回る速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float a = t * 6f, c = ArtPen.Cos(a), s = ArtPen.Sin(a); for (int d = -1; d <= 1; d += 2) { float dx = c * d, fade = .1f + .28f * MathF.Abs(dx); p.F(p.C(Color1, fade), 0, -60, dx * 760, -60 - 150 - 90 * (1 - MathF.Abs(dx)), dx * 760, -60 + 150 + 90 * (1 - MathF.Abs(dx))); }
        p.R(0, 170, 300, 72, 12, p.C(0x25282D)); p.R(0, 115, 250, 44, 8, p.C(0x3A3E45)); p.Pie(0, 95, 145, MathF.PI, MathF.Tau, p.C(Color1, .9f)); p.B(0, -5, 290, 200, p.C(Color1, .9f)); p.Pie(0, -102, 145, MathF.PI, MathF.Tau, p.C(Color1, .9f));
        p.E(s * 90, -50, 60 * (.4f + MathF.Abs(c) * .6f), 110, p.C(0xFFFFFF, .85f * MathF.Max(0f, c * .5f + .55f))); p.L(-105, 40, -115, -130, p.C(0xFFFFFF, .4f), 11);
    }
}

public sealed class SubmarinePlugin : QuickShapePlugin { public override string Name => "演出：潜水艦"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new SubmarineParameter(s); }
public sealed class SubmarineParameter : Q4Art
{
    public SubmarineParameter() : this(null) { }
    public SubmarineParameter(SharedDataStore? s) : base(s) { Color1 = Hex(0xFFC62B); Color2 = Hex(0x8EDBF7); }
    protected override string[] Use => ["Color1=船体の色", "Color2=窓の色", "Speed=進む速さ"];
    private protected override void Art(ArtPen p, float t, float v, float v2)
    {
        float by = ArtPen.Sin(t * 1.2f) * 20f; for (int i = 0; i < 22; i++) { float k = ArtPen.Fr(t * .55f + ArtPen.R01(i)), x = -430f - k * 120f + ArtPen.Sin(k * 7f + i) * 22f, y = by + 30f - k * 330f + (ArtPen.R01(i + 5) - .5f) * 60f, r = 6f + ArtPen.R01(i + 9) * 15f; p.Ring(x, y, r, p.C(0xFFFFFF, .75f * (1f - k)), 3); }
        p.Move(0, by, () =>
        {
            p.F(p.C(0xD99A1C), -345, -55, -430, -100, -430, 100, -345, 55); float pr = 72f * MathF.Abs(ArtPen.Sin(t * 16f)) + 8f; p.E(-452, 0, 12, pr, p.C(0x50555C)); p.L(-440, 0, -395, 0, p.C(0x50555C), 8);
            p.R(0, 0, 760, 225, 112, p.C(Color1)); p.R(45, -135, 210, 90, 18, p.C(Color1)); p.P(p.C(0x50555C), 13, 90, -180, 90, -245, 165, -245); p.R(180, -245, 34, 25, 6, p.C(0x50555C));
            for (int i = -1; i <= 2; i++) { float x = i * 140 - 30; p.D(x, 0, 45, p.C(0xB9BEC6)); p.D(x, 0, 34, p.C(Color2)); p.Arc(x, 0, 24, 3.4f, 4.6f, p.C(0xFFFFFF, .85f), 5); } p.L(-325, 72, 320, 72, p.C(0xD99A1C), 7);
        });
    }
}
