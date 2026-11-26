using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 223番 2501〜(キーボードのキーの追加分と、ピクトグラム = Q6Art の種類 64)
//   ピクトグラムは 2D と同じ部品の並び(Q6Art.PictoOps)を、看板の板の上に厚みを付けて置く
// =====================================================================
internal static partial class Models3D
{
    static void Picto3D(Sculpt m, float t, Vector4 C, Vector4 D, int a)
    {
        int style = Q6Art.PictoStyle(a);
        var W = m.C(0xFFFFFF); var bg = style == 2 ? W : C;
        m.Turn(0, 0, MathF.Sin(t * 1.2f) * .03f, () =>
        {
            m.Box(0, 0, 0, 460, 460, 50, bg, .1f);
            m.Box(0, 0, -30, 440, 440, 20, Sculpt.Mix(bg, m.C(0x000000), .35f), .1f);
            if (style == 3)
            {
                m.Box(0, -205, 30, 424, 14, 12, D, .3f); m.Box(0, 205, 30, 424, 14, 12, D, .3f);
                m.Box(-205, 0, 30, 14, 424, 12, D, .3f); m.Box(205, 0, 30, 14, 424, 12, D, .3f);
            }
            var ops = Q6Art.PictoOps(a);
            for (int i = 0; i < ops.Count; i++)
            {
                var o = ops[i];
                var col = o.Col == 0 ? D : bg;
                float z = 34f + i * 1.2f, d = 16f;
                switch (o.T)
                {
                    case 'c': m.Disc(o.A, o.B, z, o.C, o.C, d, col, .3f); break;
                    case 'r': m.Box(o.A, o.B, z, o.C, o.D, d, col, Math.Clamp(o.E / MathF.Max(1f, MathF.Min(o.C, o.D)), .05f, .5f)); break;
                    case 'l': m.Limb(o.A, o.B, z, o.C, o.D, z, o.E / 2f, o.E / 2f, col); break;
                    case 'p': if (o.P is { Length: >= 6 } xy) m.Slab(z, d, col, xy); break;
                    case 'o': m.Torus(o.A, o.B, z, o.C, o.D / 2f, col); break;
                    case 't': m.Text(o.S, o.C, o.A, o.B, z, d, col); break;
                }
            }
            if (style == 2) { float zr = 40f + ops.Count * 1.2f; m.Torus(0, 0, zr, 190, 17, C); m.Limb(-134, -134, zr, 134, 134, zr, 17, 17, C); }
        });
    }

    static partial void RegisterL1d(Dictionary<Type, QuickModel> d)
    {
        FamT(d, typeof(X2501Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2502Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2503Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2504Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2505Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2506Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2507Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2508Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2509Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2510Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2511Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2512Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2513Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2514Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2515Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2516Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2517Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2518Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2519Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2520Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2521Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2522Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2523Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2524Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2525Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2526Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2527Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2528Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2529Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2530Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2531Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2532Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2533Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2534Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2535Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2536Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2537Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2538Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2539Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2540Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2541Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2542Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2543Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2544Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2545Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2546Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2547Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2548Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2549Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2550Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2551Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2552Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2553Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2554Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2555Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 1));
        FamT(d, typeof(X2556Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 1));
        FamT(d, typeof(X2557Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2558Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2559Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2560Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2561Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2562Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2563Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2564Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2565Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2566Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2567Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2568Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2569Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2570Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2571Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 1));
        FamT(d, typeof(X2572Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2573Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2574Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2575Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2576Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2577Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2578Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2579Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2580Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2581Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 2));
        FamT(d, typeof(X2582Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2583Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2584Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2585Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 0));
        FamT(d, typeof(X2586Parameter), (m, t, C, D, s) => Art8_3D(m, t, C, D, s, 30, 2));
        FamT(d, typeof(X2587Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 0));
        FamT(d, typeof(X2588Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 1));
        FamT(d, typeof(X2589Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 2));
        FamT(d, typeof(X2590Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 3));
        FamT(d, typeof(X2591Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 4));
        FamT(d, typeof(X2592Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 5));
        FamT(d, typeof(X2593Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 6));
        FamT(d, typeof(X2594Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 7));
        FamT(d, typeof(X2595Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 8));
        FamT(d, typeof(X2596Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 9));
        FamT(d, typeof(X2597Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 10));
        FamT(d, typeof(X2598Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 11));
        FamT(d, typeof(X2599Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 12));
        FamT(d, typeof(X2600Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 13));
        FamT(d, typeof(X2601Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 14));
        FamT(d, typeof(X2602Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 15));
        FamT(d, typeof(X2603Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 16));
        FamT(d, typeof(X2604Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 17));
        FamT(d, typeof(X2605Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 18));
        FamT(d, typeof(X2606Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 19));
        FamT(d, typeof(X2607Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 20));
        FamT(d, typeof(X2608Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 21));
        FamT(d, typeof(X2609Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 22));
        FamT(d, typeof(X2610Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 23));
        FamT(d, typeof(X2611Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 24));
        FamT(d, typeof(X2612Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 25));
        FamT(d, typeof(X2613Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 26));
        FamT(d, typeof(X2614Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 27));
        FamT(d, typeof(X2615Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 28));
        FamT(d, typeof(X2616Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 29));
        FamT(d, typeof(X2617Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 30));
        FamT(d, typeof(X2618Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 31));
        FamT(d, typeof(X2619Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 32));
        FamT(d, typeof(X2620Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 33));
        FamT(d, typeof(X2621Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 34));
        FamT(d, typeof(X2622Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 35));
        FamT(d, typeof(X2623Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 36));
        FamT(d, typeof(X2624Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 37));
        FamT(d, typeof(X2625Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 38));
        FamT(d, typeof(X2626Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 39));
        FamT(d, typeof(X2627Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 40));
        FamT(d, typeof(X2628Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 41));
        FamT(d, typeof(X2629Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 42));
        FamT(d, typeof(X2630Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 43));
        FamT(d, typeof(X2631Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 44));
        FamT(d, typeof(X2632Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 45));
        FamT(d, typeof(X2633Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 46));
        FamT(d, typeof(X2634Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 47));
        FamT(d, typeof(X2635Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 48));
        FamT(d, typeof(X2636Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 49));
        FamT(d, typeof(X2637Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 50));
        FamT(d, typeof(X2638Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 51));
        FamT(d, typeof(X2639Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 52));
        FamT(d, typeof(X2640Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 53));
        FamT(d, typeof(X2641Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 54));
        FamT(d, typeof(X2642Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 55));
        FamT(d, typeof(X2643Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 56));
        FamT(d, typeof(X2644Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 57));
        FamT(d, typeof(X2645Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 58));
        FamT(d, typeof(X2646Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 59));
        FamT(d, typeof(X2647Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 60));
        FamT(d, typeof(X2648Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 61));
        FamT(d, typeof(X2649Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 62));
        FamT(d, typeof(X2650Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 63));
        FamT(d, typeof(X2651Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 64));
        FamT(d, typeof(X2652Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 65));
        FamT(d, typeof(X2653Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 66));
        FamT(d, typeof(X2654Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 67));
        FamT(d, typeof(X2655Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 68));
        FamT(d, typeof(X2656Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 69));
        FamT(d, typeof(X2657Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 70));
        FamT(d, typeof(X2658Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 71));
        FamT(d, typeof(X2659Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 72));
        FamT(d, typeof(X2660Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 73));
        FamT(d, typeof(X2661Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 74));
        FamT(d, typeof(X2662Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 75));
        FamT(d, typeof(X2663Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 76));
        FamT(d, typeof(X2664Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 77));
        FamT(d, typeof(X2665Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 78));
        FamT(d, typeof(X2666Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 79));
        FamT(d, typeof(X2667Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 80));
        FamT(d, typeof(X2668Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 81));
        FamT(d, typeof(X2669Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 82));
        FamT(d, typeof(X2670Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 83));
        FamT(d, typeof(X2671Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 84));
        FamT(d, typeof(X2672Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 85));
        FamT(d, typeof(X2673Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 86));
        FamT(d, typeof(X2674Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 87));
        FamT(d, typeof(X2675Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 88));
        FamT(d, typeof(X2676Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 89));
        FamT(d, typeof(X2677Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 90));
        FamT(d, typeof(X2678Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 91));
        FamT(d, typeof(X2679Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 92));
        FamT(d, typeof(X2680Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 93));
        FamT(d, typeof(X2681Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 94));
        FamT(d, typeof(X2682Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 95));
        FamT(d, typeof(X2683Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 96));
        FamT(d, typeof(X2684Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 97));
        FamT(d, typeof(X2685Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 98));
        FamT(d, typeof(X2686Parameter), (m, t, C, D, s) => Picto3D(m, t, C, D, 99));
    }
}
