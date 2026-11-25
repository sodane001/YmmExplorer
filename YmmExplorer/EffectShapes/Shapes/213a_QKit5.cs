using Vortice.Mathematics;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// =====================================================================
//  701番以降のアイコン系の図形の土台。絵は Icon に書き、動きは Mode で選ぶ
//   0=動かない 1=ふわふわ 2=左右に傾く 3=拍動 4=回る 5=小さく震える
// =====================================================================
public abstract class Q5Icon : Q4Art
{
    protected Q5Icon(SharedDataStore? s) : base(s) { }
    protected virtual int Mode => 1;
    /// <summary>動き方(専用3Dモデルでも同じ動きにするため)</summary>
    internal int IconMode => Mode;
    protected override string[] Use => ["Color1=メインの色", "Color2=サブの色", "Speed=動く速さ"];

    private protected sealed override void Art(ArtPen p, float t, float v, float v2)
    {
        var c = p.C(Color1); var d = p.C(Color2);
        void Draw() => Icon(p, t, c, d);
        switch (Mode)
        {
            case 1: p.Move(0, MathF.Sin(t * 2f) * 14f, Draw); break;
            case 2: p.Turn(0, 260, MathF.Sin(t * 2f) * .09f, Draw); break;
            case 3: { float s = 1f + MathF.Sin(t * 3.4f) * .05f; p.Zoom(0, 0, s, s, Draw); break; }
            case 4: p.Turn(0, 0, t * .9f, Draw); break;
            case 5: p.Move(MathF.Sin(t * 37f) * 3f, MathF.Cos(t * 31f) * 2f, Draw); break;
            default: Draw(); break;
        }
    }

    private protected abstract void Icon(ArtPen p, float t, Color4 C, Color4 D);

    /// <summary>絵を書く時の短い道具(乱数・端数・揺れ)</summary>
    private protected static float P(int n) => Gfx.Hash01(n);
    private protected static float Fr(float x) => x - MathF.Floor(x);
    private protected static float Sn(float t, int s) => MathF.Sin(t * 2f + s);

    /// <summary>動物の目(左右ふたつ、光つき)</summary>
    private protected static void Eyes(ArtPen p, float dx, float y, float r)
    {
        foreach (int s in new[] { -1, 1 }) { p.D(s * dx, y, r, p.C(0x20242B)); p.D(s * dx - r * .3f, y - r * .35f, r * .32f, p.C(0xFFFFFF)); }
    }
}
