using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 224番 2687〜(Q6Art の種類 65〜 のアイコン)
//   2D と同じ部品の並び(Q6Art.IconOps)に厚みを付け、重なる部品は手前に積む(重ならない部品は同じ高さに置く)
// =====================================================================
internal static partial class Models3D
{
    static void Icon3D(Sculpt m, float t, Vector4 C, Vector4 D, int kind, int a)
    {
        var ops = Q6Art.IconOps(kind, a);
        Vector4 Col(uint c) => c == Q6Art.IM ? C : c == Q6Art.IS ? D : m.C(c);
        var placed = new List<(float x0, float y0, float x1, float y1, float front, char T, uint Col, float W)>();
        m.Move(0, MathF.Sin(t * 2f) * 8f, () => m.Turn(0, 0, MathF.Sin(t * 1.3f) * .03f, () =>
        {
            foreach (var o in ops)
            {
                float x0, y0, x1, y1;
                switch (o.T)
                {
                    case 'e': x0 = o.A - o.C; x1 = o.A + o.C; y0 = o.B - o.D; y1 = o.B + o.D; break;
                    case 'r': x0 = o.A - o.C / 2f; x1 = o.A + o.C / 2f; y0 = o.B - o.D / 2f; y1 = o.B + o.D / 2f; break;
                    case 'l': x0 = MathF.Min(o.A, o.C) - o.E / 2f; x1 = MathF.Max(o.A, o.C) + o.E / 2f; y0 = MathF.Min(o.B, o.D) - o.E / 2f; y1 = MathF.Max(o.B, o.D) + o.E / 2f; break;
                    case 'o': x0 = o.A - o.C - o.D; x1 = o.A + o.C + o.D; y0 = o.B - o.C - o.D; y1 = o.B + o.C + o.D; break;
                    default:
                        {
                            var xy = o.P ?? []; x0 = y0 = float.MaxValue; x1 = y1 = float.MinValue;
                            for (int i = 0; i + 1 < xy.Length; i += 2) { x0 = MathF.Min(x0, xy[i]); x1 = MathF.Max(x1, xy[i]); y0 = MathF.Min(y0, xy[i + 1]); y1 = MathF.Max(y1, xy[i + 1]); }
                            break;
                        }
                }
                if (x0 > x1) continue;
                float size = MathF.Min(x1 - x0, y1 - y0);
                float d = Math.Clamp(size * .3f, 8f, 60f);
                // この部品の下にある部品の、いちばん手前
                float under = -d / 2f;
                foreach (var q in placed)
                    if (!(o.T == 'l' && q.T == 'l' && q.Col == o.Col && q.W == o.E) && q.x0 < x1 && x0 < q.x1 && q.y0 < y1 && y0 < q.y1) under = MathF.Max(under, q.front);
                float back = under - MathF.Min(d * .35f, 10f), z = back + d / 2f, front = back + d;
                var col = Col(o.Col);
                switch (o.T)
                {
                    case 'e': m.Ball(o.A, o.B, z, o.C, o.D, d / 2f, col); break;
                    case 'r': m.Box(o.A, o.B, z, o.C, o.D, d, col, Math.Clamp(o.E / MathF.Max(1f, MathF.Min(o.C, o.D)), .08f, .5f)); break;
                    case 'l': m.Limb(o.A, o.B, z, o.C, o.D, z, o.E / 2f, o.E / 2f, col); front = z + o.E / 2f; break;
                    case 'p': if (o.P is { Length: >= 6 } xy) m.Slab(z, d, col, xy); break;
                    case 'o': m.Torus(o.A, o.B, z, o.C, o.D / 2f, col); front = z + o.D / 2f; break;
                }
                placed.Add((x0, y0, x1, y1, front, o.T, o.Col, o.E));
            }
        }));
    }

    static partial void RegisterL1e(Dictionary<Type, QuickModel> d)
    {
        FamT(d, typeof(X2687Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 0));
        FamT(d, typeof(X2688Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 1));
        FamT(d, typeof(X2689Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 2));
        FamT(d, typeof(X2690Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 3));
        FamT(d, typeof(X2691Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 4));
        FamT(d, typeof(X2692Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 5));
        FamT(d, typeof(X2693Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 6));
        FamT(d, typeof(X2694Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 7));
        FamT(d, typeof(X2695Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 8));
        FamT(d, typeof(X2696Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 65, 9));
        FamT(d, typeof(X2697Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 0));
        FamT(d, typeof(X2698Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 1));
        FamT(d, typeof(X2699Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 2));
        FamT(d, typeof(X2700Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 3));
        FamT(d, typeof(X2701Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 4));
        FamT(d, typeof(X2702Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 5));
        FamT(d, typeof(X2703Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 6));
        FamT(d, typeof(X2704Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 7));
        FamT(d, typeof(X2705Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 8));
        FamT(d, typeof(X2706Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 66, 9));
        FamT(d, typeof(X2707Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 0));
        FamT(d, typeof(X2708Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 1));
        FamT(d, typeof(X2709Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 2));
        FamT(d, typeof(X2710Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 3));
        FamT(d, typeof(X2711Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 4));
        FamT(d, typeof(X2712Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 5));
        FamT(d, typeof(X2713Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 6));
        FamT(d, typeof(X2714Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 7));
        FamT(d, typeof(X2715Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 8));
        FamT(d, typeof(X2716Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 67, 9));
        FamT(d, typeof(X2717Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 0));
        FamT(d, typeof(X2718Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 1));
        FamT(d, typeof(X2719Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 2));
        FamT(d, typeof(X2720Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 3));
        FamT(d, typeof(X2721Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 4));
        FamT(d, typeof(X2722Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 5));
        FamT(d, typeof(X2723Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 6));
        FamT(d, typeof(X2724Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 7));
        FamT(d, typeof(X2725Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 8));
        FamT(d, typeof(X2726Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 68, 9));
        FamT(d, typeof(X2727Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 0));
        FamT(d, typeof(X2728Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 1));
        FamT(d, typeof(X2729Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 2));
        FamT(d, typeof(X2730Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 3));
        FamT(d, typeof(X2731Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 4));
        FamT(d, typeof(X2732Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 5));
        FamT(d, typeof(X2733Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 6));
        FamT(d, typeof(X2734Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 7));
        FamT(d, typeof(X2735Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 8));
        FamT(d, typeof(X2736Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 69, 9));
        FamT(d, typeof(X2737Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 0));
        FamT(d, typeof(X2738Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 1));
        FamT(d, typeof(X2739Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 2));
        FamT(d, typeof(X2740Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 3));
        FamT(d, typeof(X2741Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 4));
        FamT(d, typeof(X2742Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 5));
        FamT(d, typeof(X2743Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 6));
        FamT(d, typeof(X2744Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 7));
        FamT(d, typeof(X2745Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 8));
        FamT(d, typeof(X2746Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 70, 9));
        FamT(d, typeof(X2747Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 0));
        FamT(d, typeof(X2748Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 1));
        FamT(d, typeof(X2749Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 2));
        FamT(d, typeof(X2750Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 3));
        FamT(d, typeof(X2751Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 4));
        FamT(d, typeof(X2752Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 5));
        FamT(d, typeof(X2753Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 6));
        FamT(d, typeof(X2754Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 7));
        FamT(d, typeof(X2755Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 8));
        FamT(d, typeof(X2756Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 71, 9));
        FamT(d, typeof(X2757Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 0));
        FamT(d, typeof(X2758Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 1));
        FamT(d, typeof(X2759Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 2));
        FamT(d, typeof(X2760Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 3));
        FamT(d, typeof(X2761Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 4));
        FamT(d, typeof(X2762Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 5));
        FamT(d, typeof(X2763Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 6));
        FamT(d, typeof(X2764Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 7));
        FamT(d, typeof(X2765Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 8));
        FamT(d, typeof(X2766Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 72, 9));
        FamT(d, typeof(X2767Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 0));
        FamT(d, typeof(X2768Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 1));
        FamT(d, typeof(X2769Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 2));
        FamT(d, typeof(X2770Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 3));
        FamT(d, typeof(X2771Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 4));
        FamT(d, typeof(X2772Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 5));
        FamT(d, typeof(X2773Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 6));
        FamT(d, typeof(X2774Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 7));
        FamT(d, typeof(X2775Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 8));
        FamT(d, typeof(X2776Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 73, 9));
        FamT(d, typeof(X2777Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 0));
        FamT(d, typeof(X2778Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 1));
        FamT(d, typeof(X2779Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 2));
        FamT(d, typeof(X2780Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 3));
        FamT(d, typeof(X2781Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 4));
        FamT(d, typeof(X2782Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 5));
        FamT(d, typeof(X2783Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 6));
        FamT(d, typeof(X2784Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 7));
        FamT(d, typeof(X2785Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 8));
        FamT(d, typeof(X2786Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 74, 9));
        FamT(d, typeof(X2787Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 0));
        FamT(d, typeof(X2788Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 1));
        FamT(d, typeof(X2789Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 2));
        FamT(d, typeof(X2790Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 3));
        FamT(d, typeof(X2791Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 4));
        FamT(d, typeof(X2792Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 5));
        FamT(d, typeof(X2793Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 6));
        FamT(d, typeof(X2794Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 7));
        FamT(d, typeof(X2795Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 8));
        FamT(d, typeof(X2796Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 75, 9));
        FamT(d, typeof(X2797Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 0));
        FamT(d, typeof(X2798Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 1));
        FamT(d, typeof(X2799Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 2));
        FamT(d, typeof(X2800Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 3));
        FamT(d, typeof(X2801Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 4));
        FamT(d, typeof(X2802Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 5));
        FamT(d, typeof(X2803Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 6));
        FamT(d, typeof(X2804Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 7));
        FamT(d, typeof(X2805Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 8));
        FamT(d, typeof(X2806Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 76, 9));
        FamT(d, typeof(X2807Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 0));
        FamT(d, typeof(X2808Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 1));
        FamT(d, typeof(X2809Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 2));
        FamT(d, typeof(X2810Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 3));
        FamT(d, typeof(X2811Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 4));
        FamT(d, typeof(X2812Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 5));
        FamT(d, typeof(X2813Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 6));
        FamT(d, typeof(X2814Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 7));
        FamT(d, typeof(X2815Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 8));
        FamT(d, typeof(X2816Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 77, 9));
        FamT(d, typeof(X2817Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 0));
        FamT(d, typeof(X2818Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 1));
        FamT(d, typeof(X2819Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 2));
        FamT(d, typeof(X2820Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 3));
        FamT(d, typeof(X2821Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 4));
        FamT(d, typeof(X2822Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 5));
        FamT(d, typeof(X2823Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 6));
        FamT(d, typeof(X2824Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 7));
        FamT(d, typeof(X2825Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 8));
        FamT(d, typeof(X2826Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 78, 9));
        FamT(d, typeof(X2827Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 0));
        FamT(d, typeof(X2828Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 1));
        FamT(d, typeof(X2829Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 2));
        FamT(d, typeof(X2830Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 3));
        FamT(d, typeof(X2831Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 4));
        FamT(d, typeof(X2832Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 5));
        FamT(d, typeof(X2833Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 6));
        FamT(d, typeof(X2834Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 7));
        FamT(d, typeof(X2835Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 8));
        FamT(d, typeof(X2836Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 79, 9));
        FamT(d, typeof(X2837Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 0));
        FamT(d, typeof(X2838Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 1));
        FamT(d, typeof(X2839Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 2));
        FamT(d, typeof(X2840Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 3));
        FamT(d, typeof(X2841Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 4));
        FamT(d, typeof(X2842Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 5));
        FamT(d, typeof(X2843Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 6));
        FamT(d, typeof(X2844Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 7));
        FamT(d, typeof(X2845Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 8));
        FamT(d, typeof(X2846Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 80, 9));
        FamT(d, typeof(X2847Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 0));
        FamT(d, typeof(X2848Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 1));
        FamT(d, typeof(X2849Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 2));
        FamT(d, typeof(X2850Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 3));
        FamT(d, typeof(X2851Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 4));
        FamT(d, typeof(X2852Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 5));
        FamT(d, typeof(X2853Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 6));
        FamT(d, typeof(X2854Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 7));
        FamT(d, typeof(X2855Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 8));
        FamT(d, typeof(X2856Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 81, 9));
        FamT(d, typeof(X2857Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 0));
        FamT(d, typeof(X2858Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 1));
        FamT(d, typeof(X2859Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 2));
        FamT(d, typeof(X2860Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 3));
        FamT(d, typeof(X2861Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 4));
        FamT(d, typeof(X2862Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 5));
        FamT(d, typeof(X2863Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 6));
        FamT(d, typeof(X2864Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 7));
        FamT(d, typeof(X2865Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 8));
        FamT(d, typeof(X2866Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 82, 9));
        FamT(d, typeof(X2867Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 0));
        FamT(d, typeof(X2868Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 1));
        FamT(d, typeof(X2869Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 2));
        FamT(d, typeof(X2870Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 3));
        FamT(d, typeof(X2871Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 4));
        FamT(d, typeof(X2872Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 5));
        FamT(d, typeof(X2873Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 6));
        FamT(d, typeof(X2874Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 7));
        FamT(d, typeof(X2875Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 8));
        FamT(d, typeof(X2876Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 83, 9));
        FamT(d, typeof(X2877Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 0));
        FamT(d, typeof(X2878Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 1));
        FamT(d, typeof(X2879Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 2));
        FamT(d, typeof(X2880Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 3));
        FamT(d, typeof(X2881Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 4));
        FamT(d, typeof(X2882Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 5));
        FamT(d, typeof(X2883Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 6));
        FamT(d, typeof(X2884Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 7));
        FamT(d, typeof(X2885Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 8));
        FamT(d, typeof(X2886Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 84, 9));
    }

    static partial void RegisterL1f(Dictionary<Type, QuickModel> d)
    {
        FamT(d, typeof(X2887Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 0));
        FamT(d, typeof(X2888Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 1));
        FamT(d, typeof(X2889Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 2));
        FamT(d, typeof(X2890Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 3));
        FamT(d, typeof(X2891Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 4));
        FamT(d, typeof(X2892Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 5));
        FamT(d, typeof(X2893Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 6));
        FamT(d, typeof(X2894Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 7));
        FamT(d, typeof(X2895Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 8));
        FamT(d, typeof(X2896Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 85, 9));
        FamT(d, typeof(X2897Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 0));
        FamT(d, typeof(X2898Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 1));
        FamT(d, typeof(X2899Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 2));
        FamT(d, typeof(X2900Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 3));
        FamT(d, typeof(X2901Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 4));
        FamT(d, typeof(X2902Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 5));
        FamT(d, typeof(X2903Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 6));
        FamT(d, typeof(X2904Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 7));
        FamT(d, typeof(X2905Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 8));
        FamT(d, typeof(X2906Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 86, 9));
        FamT(d, typeof(X2907Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 0));
        FamT(d, typeof(X2908Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 1));
        FamT(d, typeof(X2909Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 2));
        FamT(d, typeof(X2910Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 3));
        FamT(d, typeof(X2911Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 4));
        FamT(d, typeof(X2912Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 5));
        FamT(d, typeof(X2913Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 6));
        FamT(d, typeof(X2914Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 7));
        FamT(d, typeof(X2915Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 8));
        FamT(d, typeof(X2916Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 87, 9));
        FamT(d, typeof(X2917Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 0));
        FamT(d, typeof(X2918Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 1));
        FamT(d, typeof(X2919Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 2));
        FamT(d, typeof(X2920Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 3));
        FamT(d, typeof(X2921Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 4));
        FamT(d, typeof(X2922Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 5));
        FamT(d, typeof(X2923Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 6));
        FamT(d, typeof(X2924Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 7));
        FamT(d, typeof(X2925Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 8));
        FamT(d, typeof(X2926Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 88, 9));
        FamT(d, typeof(X2927Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 0));
        FamT(d, typeof(X2928Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 1));
        FamT(d, typeof(X2929Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 2));
        FamT(d, typeof(X2930Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 3));
        FamT(d, typeof(X2931Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 4));
        FamT(d, typeof(X2932Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 5));
        FamT(d, typeof(X2933Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 6));
        FamT(d, typeof(X2934Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 7));
        FamT(d, typeof(X2935Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 8));
        FamT(d, typeof(X2936Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 89, 9));
        FamT(d, typeof(X2937Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 0));
        FamT(d, typeof(X2938Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 1));
        FamT(d, typeof(X2939Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 2));
        FamT(d, typeof(X2940Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 3));
        FamT(d, typeof(X2941Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 4));
        FamT(d, typeof(X2942Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 5));
        FamT(d, typeof(X2943Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 6));
        FamT(d, typeof(X2944Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 7));
        FamT(d, typeof(X2945Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 8));
        FamT(d, typeof(X2946Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 90, 9));
        FamT(d, typeof(X2947Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 0));
        FamT(d, typeof(X2948Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 1));
        FamT(d, typeof(X2949Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 2));
        FamT(d, typeof(X2950Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 3));
        FamT(d, typeof(X2951Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 4));
        FamT(d, typeof(X2952Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 5));
        FamT(d, typeof(X2953Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 6));
        FamT(d, typeof(X2954Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 7));
        FamT(d, typeof(X2955Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 8));
        FamT(d, typeof(X2956Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 91, 9));
        FamT(d, typeof(X2957Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 0));
        FamT(d, typeof(X2958Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 1));
        FamT(d, typeof(X2959Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 2));
        FamT(d, typeof(X2960Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 3));
        FamT(d, typeof(X2961Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 4));
        FamT(d, typeof(X2962Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 5));
        FamT(d, typeof(X2963Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 6));
        FamT(d, typeof(X2964Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 7));
        FamT(d, typeof(X2965Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 8));
        FamT(d, typeof(X2966Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 92, 9));
        FamT(d, typeof(X2967Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 0));
        FamT(d, typeof(X2968Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 1));
        FamT(d, typeof(X2969Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 2));
        FamT(d, typeof(X2970Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 3));
        FamT(d, typeof(X2971Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 4));
        FamT(d, typeof(X2972Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 5));
        FamT(d, typeof(X2973Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 6));
        FamT(d, typeof(X2974Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 7));
        FamT(d, typeof(X2975Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 8));
        FamT(d, typeof(X2976Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 93, 9));
        FamT(d, typeof(X2977Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 0));
        FamT(d, typeof(X2978Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 1));
        FamT(d, typeof(X2979Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 2));
        FamT(d, typeof(X2980Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 3));
        FamT(d, typeof(X2981Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 4));
        FamT(d, typeof(X2982Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 5));
        FamT(d, typeof(X2983Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 6));
        FamT(d, typeof(X2984Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 7));
        FamT(d, typeof(X2985Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 8));
        FamT(d, typeof(X2986Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 94, 9));
        FamT(d, typeof(X2987Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 0));
        FamT(d, typeof(X2988Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 1));
        FamT(d, typeof(X2989Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 2));
        FamT(d, typeof(X2990Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 3));
        FamT(d, typeof(X2991Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 4));
        FamT(d, typeof(X2992Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 5));
        FamT(d, typeof(X2993Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 6));
        FamT(d, typeof(X2994Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 7));
        FamT(d, typeof(X2995Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 8));
        FamT(d, typeof(X2996Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 95, 9));
        FamT(d, typeof(X2997Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 96, 0));
        FamT(d, typeof(X2998Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 96, 1));
        FamT(d, typeof(X2999Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 96, 2));
        FamT(d, typeof(X3000Parameter), (m, t, C, D, s) => Icon3D(m, t, C, D, 96, 3));
    }
}
