using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 218番 人物 1002〜1031(歩く・走る・ばんざい・おじぎ…)
//  ピクトグラムの人形を立体にした: 丸い胴・頭、関節に玉のある手足。
//  手前の手足(メインの色)は手前に、奥の手足(サブの色)は奥に置くので、横から見ても体の厚みがある
// =====================================================================
internal static partial class Models3D
{
    static void Person3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        var q = Q6Art.Poses[Math.Clamp(a, 0, Q6Art.Poses.Length - 1)];
        static float Rad(float v) => v * MathF.PI / 180f;
        float bob = MathF.Sin(t * 2.4f) * 6f, ts = MathF.Sin(Rad(q[0])), tc = MathF.Cos(Rad(q[0]));
        var hip = new Vector2(0, 58 + q[9] + bob);
        var sh = hip + new Vector2(ts * 170f, -tc * 170f);
        var head = sh + new Vector2(ts * 84f, -tc * 84f);
        void Limb(Vector2 from, float a1, float a2, float l1, float l2, float r, float z, Vector4 col)
        {
            var mm = from + new Vector2(MathF.Sin(Rad(a1)) * l1, MathF.Cos(Rad(a1)) * l1);
            var e = mm + new Vector2(MathF.Sin(Rad(a2)) * l2, MathF.Cos(Rad(a2)) * l2);
            m.Limb(from.X, from.Y, z, mm.X, mm.Y, z, r, r * .9f, col);
            m.Limb(mm.X, mm.Y, z, e.X, e.Y, z, r * .9f, r * .8f, col);
            m.Ball(mm.X, mm.Y, z, r * .95f, col);
            m.Ball(e.X, e.Y, z, r * .85f, col);
        }
        m.Box(0, 330, 0, 460, 6, 220, D with { W = D.W * .45f }, .3f);
        Limb(sh, q[3], q[4], 92, 88, 20, -32, D);
        Limb(hip, q[7], q[8], 128, 124, 25, -26, D);
        m.Limb(hip.X, hip.Y, 0, sh.X, sh.Y, 0, 45, 45, C);
        m.Ball(hip.X, hip.Y, 0, 45, 45, 40, C);
        m.Ball(sh.X, sh.Y, 0, 45, 45, 40, C);
        Limb(hip, q[5], q[6], 128, 124, 25, 26, C);
        Limb(sh, q[1], q[2], 92, 88, 20, 32, C);
        m.Ball(head.X, head.Y, 0, 56, C);
    }

    static partial void RegisterG1a(Dictionary<Type, QuickModel> d)
    {
        Type[] people =
        [
            typeof(X1002Parameter), typeof(X1003Parameter), typeof(X1004Parameter), typeof(X1005Parameter), typeof(X1006Parameter), typeof(X1007Parameter),
            typeof(X1008Parameter), typeof(X1009Parameter), typeof(X1010Parameter), typeof(X1011Parameter), typeof(X1012Parameter), typeof(X1013Parameter),
            typeof(X1014Parameter), typeof(X1015Parameter), typeof(X1016Parameter), typeof(X1017Parameter), typeof(X1018Parameter), typeof(X1019Parameter),
            typeof(X1020Parameter), typeof(X1021Parameter), typeof(X1022Parameter), typeof(X1023Parameter), typeof(X1024Parameter), typeof(X1025Parameter),
            typeof(X1026Parameter), typeof(X1027Parameter), typeof(X1028Parameter), typeof(X1029Parameter), typeof(X1030Parameter), typeof(X1031Parameter),
        ];
        for (int i = 0; i < people.Length; i++) { int a = i; Fam(d, people[i], (m, t, C, D) => Person3D(m, t, C, D, a)); }
    }
}
