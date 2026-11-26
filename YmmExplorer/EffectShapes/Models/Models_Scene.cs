using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデルの入口: 01〜180番(EffectShapeParameterBase を直接使う図形)
//  画面のまん中が原点、1920×1080 を基準にした絵の単位で組み立てる
// =====================================================================
internal static partial class Models3D
{
    internal delegate void SceneModel(EffectShapeParameterBase p, DrawContext c, Sculpt m);

    static Dictionary<Type, SceneModel>? scenes;

    static Dictionary<Type, SceneModel> Scenes => scenes ??= BuildScenes();

    public static bool HasScene(Type t) => Scenes.ContainsKey(t);

    static void S<T>(Dictionary<Type, SceneModel> d, Action<T, DrawContext, Sculpt> f) where T : EffectShapeParameterBase =>
        d[typeof(T)] = (p, c, m) => f((T)p, c, m);

    static Dictionary<Type, SceneModel> BuildScenes()
    {
        var d = new Dictionary<Type, SceneModel>();
        RegisterK1a(d);
        RegisterK1b(d);
        RegisterK1c(d);
        RegisterK1d(d);
        RegisterK1e(d);
        RegisterK1f(d);
        return d;
    }
    static partial void RegisterK1a(Dictionary<Type, SceneModel> d);
    static partial void RegisterK1b(Dictionary<Type, SceneModel> d);
    static partial void RegisterK1c(Dictionary<Type, SceneModel> d);
    static partial void RegisterK1d(Dictionary<Type, SceneModel> d);
    static partial void RegisterK1e(Dictionary<Type, SceneModel> d);
    static partial void RegisterK1f(Dictionary<Type, SceneModel> d);

    /// <summary>01〜180番の図形の専用3Dモデルを組み立てる(無ければ false)</summary>
    internal static bool BuildScene(EffectShapeParameterBase p, in DrawContext c, Native3D n)
    {
        if (!p.UseModel3D || !Scenes.TryGetValue(p.GetType(), out var model)) return false;
        float fov = Math.Clamp(c.F(p.Fov3D), 1f, 170f) * MathF.PI / 180f;
        n.Lit = true; n.Width = c.Width; n.Height = c.Height;
        n.Cam = c.Height / 2f / MathF.Tan(fov / 2f);
        n.PivotXY = Vector2.Zero; n.PivotZ = 0f;
        var m = new Sculpt(n, c.Width / 2f, c.Height / 2f, MathF.Min(c.Width / 1920f, c.Height / 1080f), 1f, c.Width, c.Height);
        m.Font = TextKit.DefaultFont;
        m.Now = c.Time;
        model(p, c, m);
        return true;
    }

    // ------------------------------------------------------------ 01〜180番用の小道具
    /// <summary>0〜1 の乱数(種と番号から決まる)</summary>
    static float R(int seed, int i, int k = 0) => DrawKit.H(seed, i, k);
    /// <summary>画面の割合(0〜100%)の位置を、絵の単位に直す</summary>
    static Vector2 PctPos(Sculpt m, float px, float py) => new((px / 100f - .5f) * m.W, (py / 100f - .5f) * m.H);
    /// <summary>画面の px 位置(左上が0)を、絵の単位に直す</summary>
    static Vector2 PxPos(Sculpt m, DrawContext c, float x, float y) { float k = MathF.Min(c.Width / 1920f, c.Height / 1080f); return new(x / k - m.W / 2, y / k - m.H / 2); }
}
