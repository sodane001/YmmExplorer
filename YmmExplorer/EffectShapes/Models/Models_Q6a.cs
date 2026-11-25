using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 212_QThings6.cs の図形(解説図・建物・飾り 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)
// =====================================================================
internal static partial class Models3D
{
    static partial void RegisterQ6a(Dictionary<Type, QuickModel> d)
    {
        d[typeof(LightRefractionParameter)] = (p, m, t, v, v2) => Refraction((LightRefractionParameter)p, m, t, v);
        d[typeof(MagnetFieldParameter)] = (p, m, t, v, v2) => Magnet((MagnetFieldParameter)p, m, t);
        d[typeof(WaterCycleParameter)] = (p, m, t, v, v2) => WaterCycle((WaterCycleParameter)p, m, t);
        d[typeof(FoodChainParameter)] = (p, m, t, v, v2) => FoodChain((FoodChainParameter)p, m, t);
        d[typeof(PendulumMotionParameter)] = (p, m, t, v, v2) => Pendulum((PendulumMotionParameter)p, m, t, v);
        d[typeof(UnitCircleTrigParameter)] = (p, m, t, v, v2) => UnitCircle((UnitCircleTrigParameter)p, m, t);
        d[typeof(PythagorasParameter)] = (p, m, t, v, v2) => Pythagoras((PythagorasParameter)p, m, t);
        d[typeof(SupplyDemandParameter)] = (p, m, t, v, v2) => SupplyDemand((SupplyDemandParameter)p, m, t, v, v2);
        d[typeof(PhotosynthesisParameter)] = (p, m, t, v, v2) => Photosynthesis((PhotosynthesisParameter)p, m, t);
        d[typeof(VolcanoSectionParameter)] = (p, m, t, v, v2) => Volcano((VolcanoSectionParameter)p, m, t);
        d[typeof(JapaneseCastleParameter)] = (p, m, t, v, v2) => Castle((JapaneseCastleParameter)p, m, t);
        d[typeof(DesertPyramidParameter)] = (p, m, t, v, v2) => Pyramids((DesertPyramidParameter)p, m, t);
        d[typeof(WindmillHillParameter)] = (p, m, t, v, v2) => Windmills((WindmillHillParameter)p, m, t);
        d[typeof(SuspensionBridgeParameter)] = (p, m, t, v, v2) => Bridge((SuspensionBridgeParameter)p, m, t);
        d[typeof(AncientTempleParameter)] = (p, m, t, v, v2) => Temple((AncientTempleParameter)p, m, t);
        d[typeof(HauntedMansionParameter)] = (p, m, t, v, v2) => Mansion((HauntedMansionParameter)p, m, t);
        d[typeof(SnowCabinParameter)] = (p, m, t, v, v2) => Cabin((SnowCabinParameter)p, m, t);
        d[typeof(FactoryChimneyParameter)] = (p, m, t, v, v2) => Factory((FactoryChimneyParameter)p, m, t);
        d[typeof(DamReleaseParameter)] = (p, m, t, v, v2) => Dam((DamReleaseParameter)p, m, t);
        d[typeof(MoonBaseParameter)] = (p, m, t, v, v2) => MoonBase((MoonBaseParameter)p, m, t);
        d[typeof(FlowerFrameParameter)] = (p, m, t, v, v2) => FlowerFrame((FlowerFrameParameter)p, m, t);
        d[typeof(StainedGlassParameter)] = (p, m, t, v, v2) => StainedGlass((StainedGlassParameter)p, m, t);
        d[typeof(CheckeredFlagParameter)] = (p, m, t, v, v2) => CheckerFlag((CheckeredFlagParameter)p, m, t);
        d[typeof(FloatingBalloonsParameter)] = (p, m, t, v, v2) => FloatBalloons((FloatingBalloonsParameter)p, m, t);
        d[typeof(KusudamaParameter)] = (p, m, t, v, v2) => Kusudama((KusudamaParameter)p, m, t, v);
        d[typeof(GarlandFlagsParameter)] = (p, m, t, v, v2) => Garland((GarlandFlagsParameter)p, m, t);
        d[typeof(HangingBannerParameter)] = (p, m, t, v, v2) => Banner((HangingBannerParameter)p, m, t);
        d[typeof(FlowerBouquetParameter)] = (p, m, t, v, v2) => Bouquet((FlowerBouquetParameter)p, m, t);
        d[typeof(PaperPlaneParameter)] = (p, m, t, v, v2) => PaperPlane((PaperPlaneParameter)p, m, t);
        d[typeof(OrigamiCraneParameter)] = (p, m, t, v, v2) => Crane((OrigamiCraneParameter)p, m, t);
    }

    static Vector4 Hex4(Sculpt m, uint rgb, float a = 1f) => m.C(rgb, a);
    static Vector3 V3(float x, float y, float z = 0f) => new(x, y, z);
    static Vector4 Hsv4(Sculpt m, float h, float s, float v, float a = 1f) => m.C(Gfx.Hsv(h, s, v), a);

    /// <summary>ラベルの文字(白・黒いふち)</summary>
    static void Label(Sculpt m, string s, float size, float x, float y, float z, Vector4 col, Vector4 edge = default, float ew = 0f) =>
        m.Text(s, size, x, y, z, size * .22f, col, 0, edge, ew);

    /// <summary>丘・山の連なり(厚みのある板)。y=ふもとの高さ、z=奥行き、d=厚さ</summary>
    static void Hill(Sculpt m, float y, float amp, float f, float ph, Vector4 col, float z, float d)
    {
        var l = new List<float> { -m.W, m.H, m.W, m.H };
        for (int i = 36; i >= 0; i--) { float x = -m.W + i / 36f * m.W * 2f; l.Add(x); l.Add(y + MathF.Sin(x * f + ph) * amp + MathF.Sin(x * f * 2.3f + ph * 2f) * amp * .3f); }
        m.Slab(z, d, col, [.. l]);
    }

    /// <summary>もこもこの雲(球の集まり)</summary>
    static void Cloud3(Sculpt m, float x, float y, float z, float s, Vector4 c)
    {
        int keep = m.Seg; m.Seg = 16;
        foreach (var (a, b, r) in new[] { (-70f, 10f, 44f), (-22f, -14f, 62f), (40f, -4f, 50f), (88f, 14f, 34f), (8f, 28f, 50f) }) m.Ball(x + a * s, y + b * s, z, r * s, r * s, r * s * .8f, c);
        m.Seg = keep;
    }

    /// <summary>四角すい・四角すい台(城の屋根・ピラミッド)。hwTop・hwBot は上と下の半分の幅</summary>
    static void SquareFrustum(Sculpt m, float x, float z, float yTop, float hwTop, float yBot, float hwBot, Vector4 col)
    {
        int keep = m.Seg; m.Seg = 4;
        m.Yaw(x, 0, z, MathF.PI / 4f, () => m.Lathe(x, z, col, 0, yTop, hwTop * 1.4142f, yTop, hwBot * 1.4142f, yBot, 0, yBot));
        m.Seg = keep;
    }

    // ================================================================ 解説図
    /// <summary>光の屈折: 透明な水のかたまり・水面と法線、入る光・曲がる光・はね返る光、角度と文字</summary>
    static void Refraction(LightRefractionParameter p, Sculpt m, float t, float v)
    {
        m.Box(0, 190, 0, 1240, 380, 500, m.C(p.Color2, .45f), .02f);
        m.Box(0, 0, 0, 1240, 3, 502, m.C(0xFFFFFF, .3f), .02f);
        for (float y = -380; y < 380; y += 34) m.Limb(0, y, 0, 0, y + 18, 0, 2, 2, m.C(0xFFFFFF, .65f));
        float a1 = v * 80f * MathF.PI / 180f, a2 = MathF.Asin(MathF.Sin(a1) / 1.33f), L = 410;
        float ix = -MathF.Sin(a1) * L, iy = -MathF.Cos(a1) * L, ox = MathF.Sin(a2) * L, oy = MathF.Cos(a2) * L, rx = MathF.Sin(a1) * L;
        var c = m.C(p.Color1);
        m.Limb(ix, iy, 0, 0, 0, 0, 6, 6, c); m.Limb(0, 0, 0, ox, oy, 0, 6, 6, c); m.Limb(0, 0, 0, rx, iy, 0, 3.5f, 3.5f, m.C(p.Color1, .28f));
        for (int i = 0; i < 7; i++) { float k = Fr(t * .5f + i / 7f); if (k < .5f) { float e = k * 2f; m.Ball(ix * (1f - e), iy * (1f - e), 0, 11, m.C(0xFFFFFF)); } else { float e = (k - .5f) * 2f; m.Ball(ox * e, oy * e, 0, 11, m.C(0xFFFFFF)); } }
        m.Tube([.. Sculpt.ArcPts(0, 0, 130, -MathF.PI / 2f - a1, -MathF.PI / 2f, 12).Select(q => new Vector3(q, 0))], 2.5f, m.C(0xFFFFFF));
        m.Tube([.. Sculpt.ArcPts(0, 0, 130, MathF.PI / 2f - a2, MathF.PI / 2f, 12).Select(q => new Vector3(q, 0))], 2.5f, m.C(0xFFFFFF));
        var ink = m.C(0x000000, .6f);
        Label(m, "入射角", 34, -150, -180, 20, m.C(0xFFFFFF), ink, 3); Label(m, "屈折角", 34, 150, 200, 260, m.C(0xFFFFFF), ink, 3);
        Label(m, "空気", 42, 500, -70, 20, m.C(0xFFFFFF)); Label(m, "水", 42, 500, 70, 260, m.C(0xFFFFFF));
    }

    /// <summary>磁石と磁力線: N極とS極の棒磁石、まわりをぐるりと回る立体の磁力線と、流れる矢印</summary>
    static void Magnet(MagnetFieldParameter p, Sculpt m, float t)
    {
        int n = Math.Clamp(p.Count, 1, 10);
        int keep = m.Seg; m.Seg = 8;
        var lc = m.C(p.Color3, .75f);
        for (int l = 0; l < n; l++)
            for (int r = 0; r < 6; r++)
            {
                float phi = r * MathF.Tau / 6f + l * .25f;
                float h = 95f + (l + 1f) / n * 300f, w = 300f + (l + 1f) / n * 200f;
                var q = new List<Vector3>();
                for (int i = 0; i <= 32; i++) { float a = MathF.PI * i / 32f, rr = MathF.Sin(a) * h + 30f * (1f - MathF.Sin(a)); q.Add(new(MathF.Cos(a) * w, -MathF.Cos(phi) * rr, MathF.Sin(phi) * rr)); }
                m.Tube(q, 2, lc);
                int idx = (int)(Fr(t * .25f + r / 6f + l * .07f) * 31f);
                m.Limb(q[idx], q[idx + 1] + (q[idx + 1] - q[idx]) * 1.2f, 9, 1, m.C(p.Color3));
            }
        m.Seg = keep;
        m.Box(-155, 0, 0, 310, 150, 150, m.C(p.Color2), .12f);
        m.Box(155, 0, 0, 310, 150, 150, m.C(p.Color1), .12f);
        Label(m, "S", 92, -155, 0, 80, m.C(0xFFFFFF)); Label(m, "N", 92, 155, 0, 80, m.C(0xFFFFFF));
    }

    /// <summary>水の循環: 空・太陽、山と雪・川、海、立ちのぼる水蒸気、雲と雨、3Dの矢印と文字</summary>
    static void WaterCycle(WaterCycleParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x8FD3F4), m.C(0xE8F7FF), -700);
        var sun = m.C(0xFFD54A);
        m.Ball(-700, -390, -300, 80, sun);
        for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12f + t * .15f; m.Limb(-700 + MathF.Cos(a) * 96, -390 + MathF.Sin(a) * 96, -300, -700 + MathF.Cos(a) * 130, -390 + MathF.Sin(a) * 130, -300, 4, 4, sun); }
        m.Slab(-150, 500, m.C(p.Color2), -30, 560, 980, 560, 980, -160, 720, -310, 590, -160, 410, -40, 240, 80, 80, 210);
        m.Slab(-150, 504, m.C(0xFFFFFF), 720, -310, 790, -245, 735, -230, 700, -205, 665, -240);
        var rv = new List<Vector3>(); for (int i = 0; i <= 26; i++) { float e = i / 26f; rv.Add(new(710 - e * 700 + MathF.Sin(e * 7f) * 34, -210 + e * 495, 104 + e * 40)); }
        m.Tube(rv, 10, m.C(p.Color1));
        m.Box(-450, 440, 0, 1150, 260, 700, m.C(p.Color1, .9f), .05f);
        int keep = m.Seg; m.Seg = 8;
        for (int i = 0; i < 18; i++)
        {
            float k = Fr(t * .22f + R01(i)), x = -780 + R01(i + 4) * 620 + MathF.Sin(k * 7f + i) * 22, z = (R01(i + 2) - .5f) * 400f;
            m.Tube([new(x, 240 - k * 440, z), new(x + 8, 222 - k * 440, z), new(x - 6, 205 - k * 440, z)], 2.5f, m.C(0xFFFFFF, .75f * (1f - k)));
        }
        m.Seg = keep;
        float dr = Fr(t * .05f);
        Cloud3(m, -270 + dr * 120, -335, -100, 1.5f, m.C(0xFFFFFF)); Cloud3(m, 330 + dr * 90, -345, -60, 1.8f, m.C(0xD7DEE8));
        for (int i = 0; i < 26; i++) { float k = Fr(t * .9f + R01(i)), x = 170 + R01(i + 7) * 330, z = -60 + (R01(i + 2) - .5f) * 200f; m.Limb(x, -250 + k * 300, z, x - 5, -224 + k * 300, z, 2.5f, 2.5f, m.C(p.Color1, 1f - k * .4f)); }
        m.Arrow(V3(-420, 190, 120), V3(-420, -190, 120), 6, m.C(0xFF7043)); m.Arrow(V3(-50, -390, 40), V3(145, -390, 40), 6, m.C(0x546E7A)); m.Arrow(V3(620, -60, 160), V3(300, 210, 160), 6, m.C(0x1F5FA8));
        Label(m, "蒸発", 46, -520, 40, 140, m.C(0xFFFFFF), m.C(0x8A3A20), 4); Label(m, "雲", 46, -270, -335, 40, m.C(0x37474F));
        Label(m, "雨", 46, 335, -40, 60, m.C(0xFFFFFF), m.C(0x1F5FA8), 4); Label(m, "川", 46, 505, 170, 180, m.C(0xFFFFFF), m.C(0x1F5FA8), 4); Label(m, "海", 56, -600, 420, 360, m.C(0xFFFFFF));
    }

    /// <summary>食物連鎖: 段ごとに飛び出す立体のピラミッド(四角すい台)と、段の名前・「食べられる」の矢印</summary>
    static void FoodChain(FoodChainParameter p, Sculpt m, float t)
    {
        var r = Str(p).Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        int n = Math.Clamp(r.Count, 2, 7); while (r.Count < n) r.Add("");
        const float H = 720; float rh = H / n;
        for (int i = n - 1; i >= 0; i--)
        {
            float k = DrawKit.EaseOutBack(Cl((t - (n - 1 - i) * .3f) * 2.5f)), w0 = 620f * i / n, w1 = 620f * (i + 1f) / n, y = -H / 2f + i * rh;
            if (k <= 0f) continue;
            var col = m.C(Gfx.Mix(p.Color1, p.Color2, i / (n - 1f)));
            float my = y + rh / 2f;
            m.Scale(0, my, 0, k, () =>
            {
                SquareFrustum(m, 0, 0, y + 3, w0 * .5f, y + rh - 3, w1 * .5f, col);
                Label(m, r[i], i == 0 ? 25 : 38, 0, my + (i == 0 ? rh * .22f : 0), (w0 + w1) * .25f + 8, m.C(0xFFFFFF), m.C(0x000000, .45f), 3);
            });
            if (i < n - 1)
            {
                float a = Cl((t - (n - 1 - i) * .3f - .25f) * 3f);
                m.Arrow(V3(760, y + rh * 1.4f, 0), V3(760, y + rh * .6f, 0), 5, m.C(0xFFFFFF, a));
                Label(m, "食べられる", 28, 880, y + rh, 0, m.C(0xFFFFFF, a));
            }
        }
    }

    /// <summary>振り子の運動: 支え・糸・おもり(残像)、振れる道すじ、速さの矢印と文字</summary>
    static void Pendulum(PendulumMotionParameter p, Sculpt m, float t, float v)
    {
        const float L = 600, py = -340; float th0 = .15f + v * .85f, w = 2f, th = th0 * MathF.Cos(t * w), om = -th0 * w * MathF.Sin(t * w);
        m.Box(0, py - 12, 0, 520, 24, 80, m.C(0x70757B), .25f);
        m.Limb(0, py, 0, 0, py + L + 70, 0, 1.5f, 1.5f, m.C(0xFFFFFF, .35f));
        m.Tube([.. Enumerable.Range(0, 41).Select(i => { float a = -th0 + i / 40f * th0 * 2f; return new Vector3(MathF.Sin(a) * L, py + MathF.Cos(a) * L, 0); })], 1.5f, m.C(0xFFFFFF, .45f));
        for (int g = 1; g <= 6; g++) { float o = th0 * MathF.Cos((t - g * .06f) * w); m.Ball(MathF.Sin(o) * L, py + MathF.Cos(o) * L, 0, 50, m.C(p.Color1, .16f * (1f - g / 7f))); }
        float x = MathF.Sin(th) * L, y = py + MathF.Cos(th) * L;
        m.Limb(0, py, 0, x, y, 0, 3, 3, m.C(0xE8E8E8));
        m.Ball(0, py, 0, 13, m.C(0xB9BEC6));
        m.Ball(x, y, 0, 54, m.C(p.Color1));
        float vx = MathF.Cos(th) * om * 130f, vy = -MathF.Sin(th) * om * 130f;
        if (MathF.Abs(om) > .05f) { float sx = x + MathF.Sign(vx) * 58f * MathF.Cos(th), sy = y - MathF.Sign(vx) * 58f * MathF.Sin(th); m.Arrow(V3(sx, sy, 0), V3(sx + vx, sy + vy, 0), 5, m.C(p.Color2)); }
        Label(m, $"速さ {MathF.Abs(om) / (th0 * w) * 100f:0}%", 42, 470, -240, 0, m.C(p.Color2));
        Label(m, $"高さ {(1f - MathF.Cos(th)) / MathF.Max(.001f, 1f - MathF.Cos(th0)) * 100f:0}%", 42, 470, -175, 0, m.C(0xFFD54A));
    }

    /// <summary>単位円と三角関数: 輪・軸・回る点、sin と cos の線、右に伸びる波のグラフと文字</summary>
    static void UnitCircle(UnitCircleTrigParameter p, Sculpt m, float t)
    {
        const float R = 260, cx = -430; float a = t * .9f, c = MathF.Cos(a), s = MathF.Sin(a), px = cx + c * R, py = -s * R;
        var white = m.C(0xFFFFFF, .75f);
        m.Limb(cx - 340, 0, 0, cx + 340, 0, 0, 2, 2, white); m.Limb(cx, -340, 0, cx, 340, 0, 2, 2, white);
        m.Torus(cx, 0, 0, R, 4, m.C(0xFFFFFF));
        m.Slab(0, 6, m.C(p.Color3, .55f), PiePts(cx, 0, 64, -Gfx.Mod(a, MathF.Tau), 0, 16));
        m.Limb(cx, 0, 0, px, py, 0, 3.5f, 3.5f, m.C(0xFFFFFF));
        m.Limb(px, 0, 4, px, py, 4, 5, 5, m.C(p.Color1)); m.Limb(cx, 0, 4, px, 0, 4, 5, 5, m.C(p.Color2));
        const float gx = -60, gw = 880;
        m.Limb(gx, 0, 0, gx + gw, 0, 0, 2, 2, white); m.Limb(gx, -300, 0, gx, 300, 0, 2, 2, white);
        int keep = m.Seg; m.Seg = 10;
        m.Tube([.. Enumerable.Range(0, 61).Select(i => { float e = i / 60f, q = a - e * MathF.Tau * 1.5f; return new Vector3(gx + e * gw, -MathF.Cos(q) * R, 0); })], 3, m.C(p.Color2, .75f));
        m.Tube([.. Enumerable.Range(0, 61).Select(i => { float e = i / 60f, q = a - e * MathF.Tau * 1.5f; return new Vector3(gx + e * gw, -MathF.Sin(q) * R, 4); })], 3.5f, m.C(p.Color1));
        m.Seg = keep;
        m.Limb(px, py, 0, gx, py, 0, 1.5f, 1.5f, m.C(p.Color1, .35f));
        m.Ball(gx, py, 4, 13, m.C(p.Color1)); m.Ball(px, py, 4, 15, m.C(p.Color3));
        Label(m, $"sin = {s:0.00}", 44, 300, -380, 0, m.C(p.Color1)); Label(m, $"cos = {c:0.00}", 44, 650, -380, 0, m.C(p.Color2));
    }

    /// <summary>三平方の定理: 白い直角三角形の板と、3辺から順に立ち上がる正方形の板(a²・b²・c²)と式</summary>
    static void Pythagoras(PythagorasParameter p, Sculpt m, float t)
    {
        const float a = 180, b = 240; var A = new Vector2(-120, 110); var B = A + new Vector2(b, 0); var C = A + new Vector2(0, -a);
        float k1 = Cl((t - .35f) * 2.5f), k2 = Cl((t - .75f) * 2.5f), k3 = Cl((t - 1.15f) * 2.5f);
        void Sq(Vector2 P, Vector2 Q, float k, Vector4 col, string label, float d)
        {
            if (k <= 0f) return;
            var dd = Q - P; var nn = new Vector2(dd.Y, -dd.X) * k; var P2 = P + nn; var Q2 = Q + nn;
            m.Slab(0, d, col, P.X, P.Y, Q.X, Q.Y, Q2.X, Q2.Y, P2.X, P2.Y);
            var mm = (P + Q + P2 + Q2) / 4f;
            if (k >= 1f) Label(m, label, 58, mm.X, mm.Y, d / 2f + 8, m.C(0xFFFFFF), m.C(0x000000, .5f), 4);
        }
        Sq(A, C, k1, m.C(p.Color1, .9f), "a²", 40); Sq(B, A, k2, m.C(p.Color2, .9f), "b²", 50); Sq(C, B, k3, m.C(p.Color3, .9f), "c²", 60);
        m.Slab(0, 70, m.C(0xFFFFFF), A.X, A.Y, B.X, B.Y, C.X, C.Y);
        var ink = m.C(0x263238);
        m.Tube([V3(A.X, A.Y, 36), V3(B.X, B.Y, 36), V3(C.X, C.Y, 36), V3(A.X, A.Y, 36)], 2.5f, ink);
        m.Tube([V3(A.X + 26, A.Y, 36), V3(A.X + 26, A.Y - 26, 36), V3(A.X, A.Y - 26, 36)], 1.5f, ink);
        Label(m, "a", 40, A.X + 24, A.Y - 92, 38, ink); Label(m, "b", 40, A.X + 125, A.Y - 25, 38, ink); Label(m, "c", 40, A.X + 138, A.Y - 112, 38, ink);
        float f = Cl((t - 1.6f) * 3f);
        if (f > 0f) Label(m, "a² + b² = c²", 84, 0, 450, 40, m.C(0xFFFFFF, f), m.C(0x000000, .6f * f), 5);
    }

    /// <summary>需要と供給の曲線: 3Dの軸の矢印、のびる2本の曲線(管)、均衡点の球と輪・点線と文字</summary>
    static void SupplyDemand(SupplyDemandParameter p, Sculpt m, float t, float v, float v2)
    {
        const float x0 = -480, y0 = 330, W = 960, H = 660;
        m.Arrow(V3(x0, y0, 0), V3(x0 + W + 40, y0, 0), 3.5f, m.C(0xFFFFFF)); m.Arrow(V3(x0, y0, 0), V3(x0, y0 - H - 40, 0), 3.5f, m.C(0xFFFFFF));
        Label(m, "数量", 38, x0 + W + 90, y0, 0, m.C(0xFFFFFF)); Label(m, "価格", 38, x0, y0 - H - 72, 0, m.C(0xFFFFFF));
        float d = (v - .5f) * .5f, s = (v2 - .5f) * .5f, k = Cl(t * .9f);
        float Dm(float q) => 1f - (q - d) * .85f; float Sp(float q) => .08f + (q - s) * .85f;
        var a = new List<Vector3>(); var b = new List<Vector3>();
        for (int i = 0; i <= 50 * k; i++) { float q = .04f + i / 50f * .92f; a.Add(new(x0 + q * W, y0 - Math.Clamp(Dm(q), 0f, 1f) * H, 0)); b.Add(new(x0 + q * W, y0 - Math.Clamp(Sp(q), 0f, 1f) * H, 0)); }
        if (a.Count >= 2)
        {
            int keep = m.Seg; m.Seg = 12;
            m.Tube(a, 5, m.C(p.Color1)); m.Tube(b, 5, m.C(p.Color2)); m.Seg = keep;
            Label(m, "需要", 42, a[^1].X + 55, a[^1].Y, 0, m.C(p.Color1)); Label(m, "供給", 42, b[^1].X + 55, b[^1].Y, 0, m.C(p.Color2));
        }
        if (k >= 1f)
        {
            float qe = (1f + d * .85f - .08f + s * .85f) / 1.7f, pe = Dm(qe), ex = x0 + qe * W, ey = y0 - pe * H, f = Cl((m.Now - 1.1f) * 3f);
            for (float yy = ey; yy < y0; yy += 26) m.Limb(ex, yy, 0, ex, MathF.Min(y0, yy + 13), 0, 2, 2, m.C(p.Color3, f));
            for (float xx = x0; xx < ex; xx += 26) m.Limb(xx, ey, 0, MathF.Min(ex, xx + 13), ey, 0, 2, 2, m.C(p.Color3, f));
            m.Torus(ex, ey, 0, 21 + MathF.Sin(t * 5f) * 4f, 3.5f, m.C(p.Color3, f));
            m.Ball(ex, ey, 0, 11, m.C(p.Color3, f));
            Label(m, "均衡点", 40, ex + 105, ey - 42, 0, m.C(p.Color3, f), m.C(0x000000, .6f * f), 3);
        }
    }

    /// <summary>光合成: 葉(葉脈・少し反った板)、太陽と光の粒、水・二酸化炭素・酸素・でんぷんの流れ(矢印と文字)</summary>
    static void Photosynthesis(PhotosynthesisParameter p, Sculpt m, float t)
    {
        var leaf = new List<float>();
        for (int i = 0; i <= 40; i++) { float e = i / 40f; leaf.Add(-350 + e * 700); leaf.Add(40 - MathF.Sin(e * MathF.PI) * 210f - e * 40); }
        for (int i = 40; i >= 0; i--) { float e = i / 40f; leaf.Add(-350 + e * 700); leaf.Add(40 + MathF.Sin(e * MathF.PI) * 210f - e * 40); }
        m.Pitch(0, 20, 0, .25f, () =>
        {
            m.Slab(0, 14, m.C(p.Color1), [.. leaf]);
            var vein = m.C(0xBDE8A8);
            m.Limb(-350, 40, 9, 350, 0, 9, 4, 2, vein);
            for (int i = 1; i < 7; i++) { float x = -350 + i * 100, y = 40 - i * 5.7f, h = MathF.Sin(i / 7f * MathF.PI) * 165f; m.Limb(x, y, 8, x + 75, y - h, 8, 2, 1, vein); m.Limb(x, y, 8, x + 75, y + h, 8, 2, 1, vein); }
        });
        m.Ball(-650, -360, -50, 76, m.C(p.Color2));
        for (int i = 0; i < 5; i++) { float k = Fr(t * .6f + i / 5f), sx = -580 + i * 30, sy = -320 + i * 12; m.Ball(sx + k * 420, sy + k * 270, -40 + k * 60, 9, m.C(p.Color2, 1f - k * .4f)); }
        m.Arrow(V3(-580, -310, -40), V3(-170, -60, 20), 6, m.C(p.Color2));
        void Flow(float x0, float y0, float x1, float y1, Vector4 c, string label, float lx, float ly)
        {
            m.Arrow(V3(x0, y0, 60), V3(x1, y1, 60), 5, c);
            for (int i = 0; i < 4; i++) { float k = Fr(t * .55f + i / 4f); m.Ball(x0 + (x1 - x0) * k, y0 + (y1 - y0) * k, 60, 10, m.C(0xFFFFFF, .85f)); }
            Label(m, label, 42, lx, ly, 60, c, m.C(0x000000, .55f), 3);
        }
        Flow(-650, 300, -210, 130, m.C(0x4FC3F7), "水", -700, 350); Flow(650, -300, 240, -90, m.C(0xB0BEC5), "二酸化炭素", 640, -360);
        Flow(230, 120, 650, 300, m.C(0x9CFFB0), "酸素", 710, 355); Flow(40, 170, 40, 400, m.C(0xFFB74D), "でんぷん(養分)", 40, 445);
        Label(m, "光", 48, -650, -360, 30, m.C(0x7A5A00));
    }

    /// <summary>火山の断面: 切り口の見える地面のかたまり(地層・マグマだまり・火道)、噴煙と飛ぶ溶岩、説明の札</summary>
    static void Volcano(VolcanoSectionParameter p, Sculpt m, float t)
    {
        const float D = 500, fz = D / 2f;
        m.Slab(0, D, m.C(p.Color1), -760, 420, 760, 420, 760, 120, 390, 95, 95, -270, -95, -270, -390, 95, -760, 120);
        for (int l = 0; l < 3; l++) m.Tube([V3(-760, 170 + l * 70, fz + 1), V3(-360, 150 + l * 70, fz + 1), V3(0, 180 + l * 70, fz + 1), V3(360, 150 + l * 70, fz + 1), V3(760, 170 + l * 70, fz + 1)], 2.5f, m.C(0x5A4030));
        float gl = .85f + .15f * MathF.Sin(t * 4f);
        m.Ball(0, 300, fz - 60, 270, 95, 70, m.C(p.Color2, gl));
        m.Ball(0, 305, fz - 20, 165, 52, 40, m.C(p.Color3, .85f));
        m.Slab(fz - 10, 40, m.C(p.Color2, gl), -42, 225, 42, 225, 54, -270, -54, -270);
        m.Slab(fz + 12, 6, m.C(p.Color3, .75f), -15, 230, 15, 230, 20, -270, -20, -270);
        for (int i = 0; i < 9; i++) { float k = Fr(t * .55f + i / 9f); m.Ball((R01(i) - .5f) * 54, 240 - k * 500, fz + 18, 9, m.C(0xFFF2A8, 1f - k * .35f)); }
        int keep = m.Seg; m.Seg = 14;
        for (int i = 0; i < 18; i++) { float k = Fr(t * .42f + R01(i)), x = (R01(i + 5) - .5f) * (40 + k * 280) + MathF.Sin(k * 6f + i) * 22; m.Ball(x, -285 - k * 240, (R01(i + 2) - .5f) * k * 200f, 24 + k * 58, m.C(0x70757B, .55f * (1f - k))); }
        for (int i = 0; i < 16; i++) { float k = Fr(t * .85f + R01(i + 20)), dir = (R01(i + 30) - .5f) * 2f, dz = (R01(i + 33) - .5f) * 2f; m.Ball(dir * k * 260, -280 - MathF.Sin(k * MathF.PI) * 240 + k * 110, dz * k * 200, 7, m.C(p.Color2, 1f - k * .5f)); }
        m.Seg = keep;
        void Tag(string s, float x0, float y0, float x1, float y1)
        {
            m.Limb(x0, y0, fz + 10, x1, y1, fz + 40, 2, 2, m.C(0xFFFFFF)); m.Ball(x0, y0, fz + 10, 7, m.C(0xFFFFFF));
            float lx = x1 + (x1 > 0 ? 100 : -100);
            m.Box(lx, y1, fz + 40, 220, 58, 10, m.C(0x000000, .72f), .3f);
            m.Text(s, 34, lx, y1, fz + 47, 4, m.C(0xFFFFFF), 205);
        }
        Tag("噴煙", 60, -395, 330, -405); Tag("火口", -48, -270, -340, -255); Tag("火道", 25, 40, 390, -20); Tag("マグマだまり", -160, 300, -520, 315);
    }

    // ================================================================ 建物・風景
    /// <summary>日本の城: 石垣(台形の台)、白い壁の3つの層と黒い窓、反った四角すいの屋根、金のしゃちほこ、松の木、雲と丘</summary>
    static void Castle(JapaneseCastleParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x5AAEEA), m.C(0xDDF3FF), -900);
        for (int i = 0; i < 4; i++) Cloud3(m, Fr(t * .02f + i * .27f) * (m.W + 500) - m.W / 2f - 250, -390 + i * 58, -600, 1.1f + i * .2f, m.C(0xFFFFFF, .92f));
        Hill(m, 330, 30, .004f, 1, m.C(0x5B9A58), -400, 500);
        SquareFrustum(m, 0, 0, 180, 390, 400, 520, m.C(0x8C8D88));
        var wall = m.C(p.Color1); var roof = m.C(p.Color2); var win = m.C(0x20282E);
        void Tier(float yc, float hw, float hh, int nw, float wGap)
        {
            m.Box(0, yc, 0, hw * 2, hh, hw * 1.4f, wall, .05f);
            for (int i = 0; i < nw; i++) { float x = (i - (nw - 1) / 2f) * wGap; m.Box(x, yc, hw * .7f + 1, 44, hh * .45f, 4, win, .1f); }
        }
        void Roof(float y, float hw, float h) { SquareFrustum(m, 0, 0, y - h, hw * .62f, y + 18, hw + 44, roof); }
        Tier(110, 280, 150, 5, 105); Roof(35, 330, 85);
        Tier(-80, 200, 118, 3, 112); Roof(-142, 240, 72);
        Tier(-255, 130, 104, 2, 116); Roof(-305, 160, 78);
        SquareFrustum(m, 0, 0, -462, 4, -378, 66, roof);
        foreach (int s in Sides) m.Tube([V3(s * 70, -386, 0), V3(s * 92, -420, 0), V3(s * 70, -440, 0)], 6, m.C(0xE6C65A));
        foreach (int s in Sides)
        {
            float x = s * 720;
            m.Limb(x, 420, 100, x, 150, 100, 18, 8, m.C(0x5A3A22));
            for (int l = 0; l < 4; l++) m.Ball(x + (l % 2 == 0 ? -46 : 42), 215 - l * 58, 100, 118 - l * 15, 40, 80 - l * 10, m.C(l % 2 == 0 ? 0x2F6B3B : 0x3D7D49));
        }
    }

    /// <summary>ピラミッドと砂漠: 本物の四角すいのピラミッド3つ、太陽、砂丘、かげろう、歩くラクダ</summary>
    static void Pyramids(DesertPyramidParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x4FA8E8), m.C(0xFFE9C2), -1200);
        float sb = 1f + MathF.Sin(t * 1.4f) * .05f;
        m.Ball(520, -300, -1000, 145 * sb, m.C(p.Color2, .24f)); m.Ball(520, -300, -990, 94, m.C(p.Color2));
        SquareFrustum(m, -90, -300, 250 - 530, 1, 250, 450, m.C(0xD9A95C));
        SquareFrustum(m, 390, -150, 250 - 310, 1, 250, 265, m.C(0xD9A95C));
        SquareFrustum(m, -650, -500, 260 - 240, 1, 260, 205, m.C(0xD9A95C));
        Hill(m, 255, 18, .006f, 2f, Vector4.Lerp(m.C(p.Color1), m.C(0xFFFFFF), .12f), 100, 300);
        Hill(m, 340, 34, .0035f, .4f, m.C(p.Color1), 300, 400);
        for (int i = 0; i < 12; i++) { float y = 80 + i * 16, x = Fr(t * .12f + R01(i)) * (m.W + 200) - m.W / 2f - 100; m.Tube([V3(x, y, 400), V3(x + 45, y + MathF.Sin(t * 4f + i) * 6f, 400), V3(x + 90, y, 400)], 1.5f, m.C(0xFFFFFF, .22f)); }
        var cam = m.C(0x3B2A1B);
        for (int i = 0; i < 3; i++)
        {
            float x = -420 + i * 150 + Fr(t * .012f) * 90, y = 365, z = 480;
            m.Ball(x, y, z, 54, 20, 24, cam); m.Ball(x - 18, y - 22, z, 17, cam); m.Ball(x + 16, y - 22, z, 17, cam);
            m.Limb(x + 45, y - 7, z, x + 72, y - 48, z, 7, 6, cam); m.Ball(x + 80, y - 50, z, 18, 9, 9, cam);
            foreach (int k in new[] { -30, -12, 14, 34 }) m.Limb(x + k, y + 10, z + (k % 2 == 0 ? 10 : -10), x + k + MathF.Sin(t * 3f + k) * 5f, y + 58, z + (k % 2 == 0 ? 10 : -10), 4, 3, cam);
        }
    }

    /// <summary>風車の丘: 円すい台の塔と円すいの屋根、回る4枚の羽根(格子の帆)、丘と花畑、流れる雲</summary>
    static void Windmills(WindmillHillParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x63B8F0), m.C(0xE6F7FF), -1200);
        for (int i = 0; i < 3; i++) Cloud3(m, Fr(t * .018f + i * .34f) * (m.W + 500) - m.W / 2f - 250, -395 + i * 74, -900, 1.2f, m.C(0xFFFFFF, .92f));
        Hill(m, 145, 44, .003f, 1.2f, m.C(0x7CBF63), -500, 700);
        void Mill(float x, float y, float z, float S)
        {
            int keep = m.Seg; m.Seg = 10;
            m.Lathe(x, z, m.C(p.Color1), 58 * S, y, 92 * S, y + 280 * S, 0, y + 280 * S);
            m.Seg = keep;
            m.Lathe(x, z, m.C(p.Color2), 0, y - 88 * S, 74 * S, y + 4 * S, 0, y + 4 * S);
            m.Box(x, y + 225 * S, z + 80 * S, 50 * S, 110 * S, 30 * S, m.C(0x6B4428), .4f);
            m.Disc(x, y + 88 * S, z + 66 * S, 19 * S, 19 * S, 10 * S, m.C(0x8ED0EE));
            float rot = t * 1.2f, fz = z + 85 * S;
            m.Limb(x, y - 8 * S, z + 40 * S, x, y - 8 * S, fz, 8 * S, 8 * S, m.C(0x4A2F1B));
            for (int b = 0; b < 4; b++)
            {
                float a = rot + b * MathF.PI / 2f;
                m.Turn(x, y - 8 * S, a, () =>
                {
                    m.Limb(x, y - 8 * S, fz, x + 215 * S, y - 8 * S, fz, 4 * S, 3 * S, m.C(0x6B4428));
                    m.Box(x + 135 * S, y + 14 * S, fz, 150 * S, 42 * S, 4 * S, m.C(0xFFFFFF, .9f), .1f);
                    for (int l = 0; l < 5; l++) m.Limb(x + (70 + l * 32) * S, y - 6 * S, fz + 3 * S, x + (70 + l * 32) * S, y + 34 * S, fz + 3 * S, 1.2f * S, 1.2f * S, m.C(0x8A7458));
                });
            }
            m.Ball(x, y - 8 * S, fz + 4 * S, 13 * S, m.C(0x4A2F1B));
        }
        Mill(-470, -70, -300, .78f); Mill(250, -200, -100, 1.2f);
        Hill(m, 300, 26, .005f, 3f, m.C(0x5CA84C), 200, 400);
        int k2 = m.Seg; m.Seg = 8;
        uint[] fc = [0xFF5C8A, 0xFFD54A, 0xE85DFF, 0xFFFFFF];
        for (int i = 0; i < 120; i++)
        {
            float x = (R01(i) - .5f) * m.W, y = 320 + R01(i + 200) * 220, z = 400 + (R01(i + 300) - .5f) * 120, sw = MathF.Sin(t * 1.4f + x * .01f) * 5f;
            m.Limb(x, y + 18, z, x + sw, y, z, 1.2f, 1.2f, m.C(0x2F7D35));
            m.Ball(x + sw, y, z, 6f + R01(i + 9) * 5f, m.C(fc[i % 4]));
        }
        m.Seg = k2;
    }

    /// <summary>つり橋: 谷の両側のがけ、下の川、2本ずつの太いつりロープと手すり、つり糸、板を並べた橋、塔</summary>
    static void Bridge(SuspensionBridgeParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x79C7F0), m.C(0xE8F8FF), -1400);
        Hill(m, -40, 70, .0028f, .7f, m.C(0x8FBFA6), -1100, 200);
        m.Box(0, 460, 0, m.W * 1.6f, 200, 1400, m.C(0x3C97C7), .02f);
        for (int i = 0; i < 16; i++) { float x = Fr(t * .08f + R01(i)) * m.W - m.W / 2f, y = 358, z = (R01(i + 4) - .5f) * 1000; m.Limb(x, y, z, x + 70, y, z, 1.5f, 1.5f, m.C(0xFFFFFF, .5f)); }
        foreach (int s in Sides) m.Slab(0, 900, m.C(p.Color2), s * 1000, 560, s * 1000, 30, s * 660, 50, s * 560, 160, s * 500, 340, s * 430, 560);
        float sw = MathF.Sin(t * 1.5f) * 16f;
        float D(float x) { float e = x / 640f; return 72 + (1f - e * e) * 150f + (1f - e * e) * sw; }
        float Rp(float x) { float e = x / 640f; return -150 + (1f - e * e) * 150f + (1f - e * e) * sw * .7f; }
        var rope = m.C(0xD8C7A8);
        int keep = m.Seg; m.Seg = 10;
        foreach (int zs in Sides)
        {
            float z = zs * 70;
            m.Tube([.. Enumerable.Range(0, 41).Select(i => { float x = -640 + i * 32f; return new Vector3(x, Rp(x), z); })], 4, rope);
            m.Tube([.. Enumerable.Range(0, 41).Select(i => { float x = -640 + i * 32f; return new Vector3(x, D(x) - 58, z); })], 2.5f, rope);
            for (int x = -620; x <= 620; x += 40) m.Limb(x, Rp(x), z, x, D(x), z, 1.2f, 1.2f, rope);
        }
        m.Seg = keep;
        for (int x = -620; x <= 620; x += 40) m.Box(x, D(x) + 4, 0, 34, 12, 150, m.C(R01(x + 900) > .5f ? 0x8A6240u : 0x7A5536u), .2f);
        foreach (int s in Sides) foreach (int zs in Sides) m.Box(s * 640, -20, zs * 70, 28, 300, 28, m.C(p.Color1), .3f);
        foreach (int s in Sides) m.Box(s * 640, -150, 0, 40, 18, 170, m.C(0x5A3A22), .3f);
    }

    /// <summary>古代の神殿: 3段の基壇、前後2列の丸い柱(柱頭と礎石)、梁と飾り、三角の破風、金の飾り</summary>
    static void Temple(AncientTempleParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x3F9BE0), m.C(0xFFF1D2), -1200);
        for (int i = 0; i < 3; i++) Cloud3(m, Fr(t * .018f + i * .33f) * (m.W + 500) - m.W / 2f - 250, -420 + i * 66, -900, 1.15f, m.C(0xFFFFFF, .9f));
        Hill(m, 330, 24, .004f, 1f, m.C(0x9DB56E), -500, 400);
        var stone = m.C(p.Color1); var shade = m.C(p.Color2);
        for (int s = 0; s < 3; s++) m.Box(0, 400 - s * 40, 0, 1320 - s * 72, 42, 700 - s * 60, s % 2 == 0 ? stone : shade, .05f);
        m.Box(0, 80, -120, 900, 460, 300, m.C(0x5A5248), .05f);
        int n = Math.Clamp(p.Count, 4, 12); float gap = 1040f / (n - 1);
        foreach (float z in new[] { 200f, -260f })
            for (int i = 0; i < n; i++)
            {
                float x = -520 + i * gap;
                m.Lathe(x, z, stone, 39, 322, 30, -158);
                m.Box(x, 335, z, 104, 26, 104, stone, .1f); m.Box(x, -170, z, 100, 28, 100, stone, .1f);
            }
        m.Box(0, -215, -30, 1180, 66, 600, stone, .05f);
        for (int i = 0; i < 20; i++) m.Box(-560 + i * 59, -215, 271, 22, 48, 4, shade, .1f);
        m.Box(0, -258, -30, 1250, 22, 640, shade, .05f);
        m.Slab(-30, 600, stone, -650, -268, 650, -268, 0, -470);
        m.Slab(271, 4, shade, -530, -285, 530, -285, 0, -438);
        m.Disc(0, -340, 276, 42, 42, 8, m.C(0xE6C65A));
        m.Slab(281, 4, m.C(0xFFF2A8), StarPts(0, -340, 32, 8, .5f, t * .2f));
    }

    /// <summary>幽霊屋敷: 夜空と月、黒い屋敷(三角屋根・2つの塔・煙突)、ちらつく窓とドア、枯れ木、飛ぶコウモリ</summary>
    static void Mansion(HauntedMansionParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x130E2E), m.C(0x54366F), -1200);
        m.Ball(470, -270, -900, 190, m.C(0xFFF3C8, .12f)); m.Ball(470, -270, -880, 130, m.C(0xFFF3C8));
        m.Ball(430, -300, -760, 24, 24, 6, m.C(0xE4D7A8)); m.Ball(510, -220, -760, 34, 34, 6, m.C(0xE4D7A8));
        for (int i = 0; i < 4; i++) { float x = Fr(t * .025f + i * .27f) * (m.W + 800) - m.W / 2f - 400; m.Ball(x, -340 + i * 72, -600, 240, 18, 60, m.C(0x8D84A8, .25f)); }
        Hill(m, 365, 22, .006f, 1f, m.C(0x100C18), 0, 900);
        var c = m.C(p.Color1);
        m.Box(0, 150, 0, 880, 460, 400, c, .03f);
        m.Slab(0, 400, c, -480, -80, 480, -80, 330, -270, -330, -270);
        m.Box(-400, -25, 40, 180, 690, 200, c, .03f); SquareFrustum(m, -400, 40, -570, 1, -360, 110, c);
        m.Box(390, 10, 40, 150, 620, 180, c, .03f); SquareFrustum(m, 390, 40, -470, 1, -292, 90, c);
        m.Box(150, -320, -60, 54, 120, 54, c, .1f);
        int w = 0;
        void Win(float x, float y, float z, int id) { float fl = MathF.Sin(t * (2f + id) + id * 2f) > -.25f ? 1f : .12f; m.Box(x, y, z, 62, 96, 6, m.C(p.Color2, fl), .4f); m.Box(x, y, z + 4, 4, 96, 6, c, .1f); m.Box(x, y, z + 4, 62, 4, 6, c, .1f); }
        for (int y = 0; y < 2; y++) for (int x = -2; x <= 2; x++) { if (y == 1 && x == 0) continue; Win(x * 130, 30 + y * 170, 200, w++); }
        Win(-400, -190, 140, w++); Win(-400, 30, 140, w++); Win(390, -110, 130, w++);
        m.Box(0, 305, 202, 110, 170, 8, m.C(0x3A2418), .45f); m.Ball(32, 310, 208, 7, m.C(0xE6C65A));
        var tree = m.C(0x0E0A14);
        foreach (int s in Sides)
        {
            float x = s * 720;
            m.Tube([V3(x, 450, 250), V3(x + s * 15, 170, 250), V3(x - s * 28, 20, 250)], 11, tree);
            m.Tube([V3(x + s * 8, 240, 250), V3(x + s * 108, 105, 280), V3(x + s * 150, 120, 300)], 5.5f, tree);
            m.Tube([V3(x - s * 8, 130, 250), V3(x - s * 90, 25, 220), V3(x - s * 125, 40, 200)], 4.5f, tree);
        }
        var bat = m.C(0x07050D);
        for (int i = 0; i < Math.Clamp(p.Count, 0, 24); i++)
        {
            float k = Fr(t * .08f + R01(i)), x = k * (m.W + 200) - m.W / 2f - 100, y = -420 + R01(i + 9) * 340 + MathF.Sin(t * 2f + i) * 30, z = (R01(i + 4) - .5f) * 600, f = MathF.Sin(t * 16f + i) * .9f, S = .7f + R01(i + 3) * .6f;
            m.Ball(x, y + 5, z, 10 * S, 13 * S, 10 * S, bat);
            foreach (int sd in Sides) m.Yaw(x, y, z, sd * f * .6f, () => m.Slab(z, 2, bat, x, y + 5, x + sd * 26 * S, y - 18 * S, x + sd * 58 * S, y + 2, x + sd * 36 * S, y + 2, x + sd * 20 * S, y + 13 * S));
        }
    }

    /// <summary>雪山の山小屋: 夜空と雪の山、雪をかぶったもみの木、丸太の小屋(三角の雪屋根・煙突・光る窓)、煙と降る雪</summary>
    static void Cabin(SnowCabinParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x223A68), m.C(0x9CC4E6), -1400);
        m.Slab(-900, 300, m.C(0x6F8DB0), -1000, 300, -520, -260, -240, 60, 40, -390, 560, 240, 1000, 300);
        m.Slab(-740, 4, m.C(0xF4F8FC), -520, -260, -420, -150, -470, -130, -525, -80, -590, -175);
        m.Slab(-740, 4, m.C(0xF4F8FC), 40, -390, 190, -210, 110, -190, 50, -120, -30, -190, -70, -260);
        m.Super(0, 300, 0, m.W, 40, 700, .5f, .3f, m.C(0xEAF2FA));
        for (int i = 0; i < 9; i++)
        {
            float x = -820 + i * 205 + (i % 2) * 44, y = 300 + (i % 3) * 22, S = .8f + R01(i) * .5f, z = -200 + (i % 3) * 150;
            if (MathF.Abs(x - 120) < 210) continue;
            for (int l = 0; l < 3; l++)
            {
                float w = (90 - l * 22) * S, yy = y - l * 58 * S;
                m.Lathe(x, z, m.C(0x274E3D), 0, yy - 96 * S, w, yy, 0, yy);
                m.Lathe(x, z, m.C(0xFFFFFF), 0, yy - 97 * S, w * .46f, yy - 54 * S, 0, yy - 54 * S);
            }
        }
        const float cx = 120, by = 395;
        var wood = m.C(p.Color1);
        m.Box(cx, by - 110, 100, 360, 220, 300, wood, .05f);
        for (int r = 0; r < 6; r++) m.Limb(cx - 180, by - 200 + r * 38, 251, cx + 180, by - 200 + r * 38, 251, 3, 3, m.C(0x4E2F1B));
        m.Box(cx + 115, by - 345, 60, 48, 150, 48, m.C(0x5D5A58), .1f);
        m.Slab(100, 360, m.C(0xFFFFFF), cx - 235, by - 205, cx + 235, by - 205, cx, by - 395);
        m.Box(cx - 72, by - 62, 252, 86, 124, 6, m.C(0x3E2616), .2f);
        m.Box(cx + 78, by - 115, 252, 92, 84, 6, m.C(p.Color2, .9f + MathF.Sin(t * 5f) * .1f), .2f);
        m.Box(cx + 78, by - 115, 256, 4, 84, 4, m.C(0x3E2616), .1f); m.Box(cx + 78, by - 115, 256, 92, 4, 4, m.C(0x3E2616), .1f);
        int keep = m.Seg; m.Seg = 12;
        for (int i = 0; i < 12; i++) { float k = Fr(t * .2f + i / 12f); m.Ball(cx + 115 + MathF.Sin(k * 6f + i) * (6 + k * 34) + k * 60, by - 420 - k * 260, 60 + k * 40, 14 + k * 38, m.C(0xFFFFFF, .36f * (1f - k))); }
        m.Seg = 6;
        for (int i = 0; i < Math.Clamp(p.Count, 0, 300); i++)
            m.Ball(Fr(R01(i) + MathF.Sin(t * .3f + i) * .01f) * m.W - m.W / 2f, Fr(R01(i + 500) + t * (.03f + R01(i + 7) * .05f)) * m.H - m.H / 2f, (R01(i + 900) - .5f) * 1000f, 1.6f + R01(i + 3) * 3.4f, m.C(0xFFFFFF, .92f));
        m.Seg = keep;
    }

    /// <summary>工場の煙突: 白い帯のある3本の煙突と煙、のこぎり屋根の建物と灯る窓、回る2つの歯車</summary>
    static void Factory(FactoryChimneyParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x8A9BAD), m.C(0xF0D8B8), -1200);
        float[] cxs = [-390, -180, 110]; float[] tops = [-360, -420, -300]; float[] czs = [-200, -320, -150];
        int keep = m.Seg; m.Seg = 14;
        for (int c = 0; c < 3; c++) for (int i = 0; i < 14; i++) { float k = Fr(t * .16f + i / 14f + c * .3f); m.Ball(cxs[c] + k * 330 + MathF.Sin(k * 6f + i) * (8 + k * 36), tops[c] - 18 - k * 180, czs[c] + k * 80, 22 + k * 62, m.C(0xE5E5E5, .48f * (1f - k))); }
        m.Seg = keep;
        for (int c = 0; c < 3; c++)
        {
            float x = cxs[c], y = tops[c], z = czs[c];
            m.Lathe(x, z, m.C(p.Color2), 27, y, 44, 240, 0, 240);
            foreach (float q in new[] { .12f, .24f }) { float yy = y + (240 - y) * q, w = 27 + 17 * q; m.Cyl(x, yy, z, w + 1.5f, 22, m.C(0xF2F2EE), .1f); }
            m.Cyl(x, y, z, 36, 20, m.C(0x3A3E45), .2f);
        }
        m.Slab(0, 360, m.C(p.Color1), -760, 460, -760, 80, -560, -40, -560, 80, -360, -40, -360, 80, -160, -40, -160, 80, 40, -40, 40, 80, 240, -40, 240, 80, 760, 80, 760, 460);
        for (int x = 0; x < 11; x++) for (int y = 0; y < 2; y++) m.Box(-640 + x * 128, 170 + y * 120, 182, 76, 68, 6, m.C(0xFFE08A, R01(x * 7 + y) > .3f ? .9f : .18f), .15f);
        for (int g = 0; g < 2; g++)
        {
            float x = 500 + g * 140, y = -30 - g * 22, R = 96 - g * 26, dir = g == 0 ? 1f : -1.45f;
            var gear = new List<float>();
            for (int i = 0; i < 10; i++) { float a = i * MathF.Tau / 10f; foreach (var (da, rr) in new[] { (-.18f, .86f), (-.1f, 1.16f), (.1f, 1.16f), (.18f, .86f) }) { gear.Add(x + MathF.Cos(a + da) * R * rr); gear.Add(y + MathF.Sin(a + da) * R * rr); } }
            m.Turn(x, y, t * .7f * dir, () =>
            {
                m.Slab(240 + g * 10, 40, m.C(0xB9BEC6), [.. gear]);
                m.Disc(x, y, 262 + g * 10, R * .34f, R * .34f, 10, m.C(p.Color1), .3f);
            });
        }
    }

    /// <summary>ダムの放水: 奥のダム湖と山、斜めの厚いダムの壁(目地)、上の通路と柱、流れ落ちる放水、しぶき、下の川</summary>
    static void Dam(DamReleaseParameter p, Sculpt m, float t)
    {
        if (p.Background) { m.Backdrop(m.C(0x5AAEEA), m.C(0xDFF3FF), -1600); Hill(m, -180, 80, .003f, .8f, m.C(0x4E8B5A), -1300, 400); }
        const float top = -210, bot = 330, zt = -200, zb = 200;
        m.Box(0, top - 22, -800, m.W * 1.6f, 34, 1200, m.C(0x3C97C7), .02f);
        var dam = m.C(p.Color1);
        m.Quad3D(V3(-640, top, zt), V3(640, top, zt), V3(840, bot, zb), V3(-840, bot, zb), dam);
        m.Quad3D(V3(-640, top, zt - 120), V3(-640, top, zt), V3(-840, bot, zb), V3(-840, bot, zt - 120), Sculpt.Dark(dam, .85f));
        m.Quad3D(V3(640, top, zt - 120), V3(640, top, zt), V3(840, bot, zb), V3(840, bot, zt - 120), Sculpt.Dark(dam, .85f));
        for (int y = 1; y < 8; y++) { float e = y / 8f, yy = top + e * (bot - top), w = 640 + e * 200, z = zt + e * (zb - zt) + 1; m.Limb(-w, yy, z, w, yy, z, 1.2f, 1.2f, m.C(0x8E918D, .65f)); }
        m.Box(0, top - 12, zt - 40, 1320, 26, 120, m.C(0xD4D6D2), .1f);
        for (int i = -6; i <= 6; i++) m.Box(i * 104, top - 34, zt - 40, 16, 38, 16, m.C(0x70757B), .2f);
        int n = Math.Clamp(p.Count, 1, 5);
        for (int c = 0; c < n; c++)
        {
            float x = (c - (n - 1) / 2f) * 215;
            m.Box(x, top + 26, zt + 18, 138, 32, 20, m.C(0x31363B), .2f);
            for (int s = 0; s < 6; s++)
            {
                float e0 = s / 6f, e1 = (s + 1) / 6f;
                float y0 = top + 30 + e0 * (bot - top - 8), y1 = top + 30 + e1 * (bot - top - 8), z0 = zt + 30 + e0 * (zb - zt), z1 = zt + 30 + e1 * (zb - zt);
                float w0 = 58 + e0 * 18, w1 = 58 + e1 * 18;
                m.Quad3D(V3(x - w0, y0, z0), V3(x + w0, y0, z0), V3(x + w1, y1, z1), V3(x - w1, y1, z1), m.C(p.Color2, .9f));
            }
            for (int l = 0; l < 13; l++) { float k = Fr(t * (1f + R01(l + c * 9) * .35f) + R01(l * 3 + c)), lx = x + (R01(l * 5 + c) - .5f) * (112 + k * 34), ly = top + 38 + k * (bot - top - 48), lz = zt + 34 + k * (zb - zt); m.Limb(lx, ly, lz, lx, ly + 54, lz + 20, 2, 2, m.C(0xFFFFFF, .75f)); }
            int keep = m.Seg; m.Seg = 12;
            for (int i = 0; i < 10; i++) { float k = Fr(t * .8f + R01(i + c * 13)); m.Ball(x + (R01(i + 40) - .5f) * 290 * (.5f + k), bot + 20 - k * 80, zb + 40 + k * 80, 18 + k * 34, m.C(0xFFFFFF, .5f * (1f - k))); }
            m.Seg = keep;
        }
        m.Box(0, 450, 500, m.W * 1.6f, 240, 600, m.C(p.Color2, .85f), .02f);
        for (int i = 0; i < 24; i++) { float x = Fr(t * .18f + R01(i)) * m.W - m.W / 2f, z = 260 + R01(i + 9) * 500; m.Limb(x, 329, z, x + 62, 329, z, 1.5f, 1.5f, m.C(0xFFFFFF, .58f)); }
    }

    /// <summary>月面基地: 星空と地球、クレーターのある月面、透明なドームの基地(骨組み・窓)、回るアンテナと電波、走る探査車</summary>
    static void MoonBase(MoonBaseParameter p, Sculpt m, float t)
    {
        if (p.Background) m.Backdrop(m.C(0x04060F), m.C(0x04060F), -1500, 2.4f);
        int keep = m.Seg; m.Seg = 6;
        for (int i = 0; i < 130; i++) m.Ball((R01(i) - .5f) * m.W * 1.8f, (R01(i + 300) - .5f) * m.H * 1.2f - m.H * .2f, -1300, 1.8f + R01(i + 600) * 3f, m.C(0xFFFFFF, .35f + .65f * MathF.Abs(MathF.Sin(t * .6f + i))));
        m.Seg = keep;
        m.Ball(-520, -270, -900, 132, m.C(0x3B8EE0));
        for (int i = 0; i < 7; i++) { float a = R01(i) * MathF.Tau + t * .04f, r = R01(i + 5) * 92; var dir = Vector3.Normalize(new Vector3(MathF.Cos(a) * r, MathF.Sin(a) * r, 120)); m.Ball(-520 + dir.X * 128, -270 + dir.Y * 128, -900 + dir.Z * 128, 40 + R01(i + 3) * 28, 22, 10, m.C(0x55B85A)); }
        m.Yaw(-520, -270, -900, -1.1f, () => m.Lathe(-520, -900, [.. Dome(134, 134, -270, 12).Concat(Dome(134, 134, -270, 12).Select(q => new Vector2(q.X, -270 + (-270 - q.Y))).Reverse())], m.C(0x000000, .55f), 0, MathF.PI));
        m.Super(0, 260, 0, m.W, 90, 900, .5f, .4f, m.C(0x8E8E92));
        for (int i = 0; i < 12; i++)
        {
            float x = (R01(i + 80) - .5f) * m.W, z = (R01(i + 90) - .5f) * 1200, rx = 44 + R01(i + 95) * 98;
            m.Pitch(x, 172, z, MathF.PI / 2f, () => m.Torus(x, 172, z, rx, rx * .18f, m.C(0xAAAAAE)));
            m.Super(x, 171, z, rx, 1, rx, 1f, 1f, m.C(0x69696E));
        }
        void Base(float x, float y, float z, float R)
        {
            m.Lathe(x, z, [.. Dome(R, R, y, 14), new Vector2(0, y)], m.C(p.Color2, .32f));
            for (int i = 0; i < 5; i++) m.Yaw(x, y, z, i * MathF.PI / 5f, () => m.Tube([.. Sculpt.ArcPts(x, y, R, MathF.PI, MathF.Tau, 16).Select(q => new Vector3(q, z))], 3, m.C(p.Color1, .8f)));
            m.Pitch(x, y - R * .55f, z, MathF.PI / 2f, () => m.Torus(x, y - R * .55f, z, R * .84f, 3, m.C(p.Color1, .7f)));
            m.Cyl(x, y + 18, z, R + 11, 36, m.C(p.Color1), .2f);
            for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12f; m.Box(x + MathF.Cos(a) * (R + 12), y + 18, z + MathF.Sin(a) * (R + 12), 26, 13, 6, m.C(p.Color2), .3f); }
        }
        Base(70, 240, 0, 190); Base(470, 240, 60, 118);
        m.Limb(250, 250, 30, 360, 250, 50, 17, 17, m.C(p.Color1));
        m.Limb(-300, 280, 0, -300, -40, 0, 6, 6, m.C(p.Color1));
        float dish = MathF.Sin(t * .6f) * .5f;
        m.Turn(-300, -40, dish, () =>
        {
            m.Lathe(-300, 0, m.C(p.Color1), 112, -60, 70, -38, 0, -30);
            m.Limb(-300, -30, 0, -300, -110, 0, 3, 3, m.C(0x70757B)); m.Ball(-300, -112, 0, 9, m.C(0xFF5252));
        });
        for (int r = 0; r < 3; r++) { float k = Fr(t * .7f + r / 3f); m.Tube([.. Sculpt.ArcPts(-300 + MathF.Sin(dish) * 30, -60, 40 + k * 130, -MathF.PI / 2f + dish - .55f, -MathF.PI / 2f + dish + .55f, 10).Select(q => new Vector3(q, 0))], 2.5f, m.C(p.Color2, 1f - k)); }
        float bx = MathF.Sin(t * .22f) * 420 - 120;
        m.Box(bx, 170, 300, 150, 54, 90, m.C(p.Color1), .35f);
        m.Box(bx + 25, 140, 300, 56, 34, 60, m.C(p.Color2, .8f), .3f);
        foreach (int s in Sides) foreach (int zs in Sides) m.Disc(bx + s * 52, 200, 300 + zs * 50, 21, 21, 14, m.C(0x25282D), .4f);
    }

    // ================================================================ 飾り
    /// <summary>花の飾り枠: 立体の緑の枠と、四隅から辺にそって咲いていく花(花びら・中心・葉)</summary>
    static void FlowerFrame(FlowerFrameParameter p, Sculpt m, float t)
    {
        const float W = 1460, H = 800;
        m.Tube([V3(-W / 2, -H / 2), V3(W / 2, -H / 2), V3(W / 2, H / 2), V3(-W / 2, H / 2), V3(-W / 2, -H / 2)], 5, m.C(p.Color2));
        m.Tube([V3(-W / 2 + 13, -H / 2 + 13), V3(W / 2 - 13, -H / 2 + 13), V3(W / 2 - 13, H / 2 - 13), V3(-W / 2 + 13, H / 2 - 13), V3(-W / 2 + 13, -H / 2 + 13)], 2, m.C(0xFFFFFF, .65f));
        void Flower(float x, float y, float S, int id)
        {
            float g = DrawKit.EaseOutBack(Cl((m.Now - .1f - id * .025f) * 3f)); if (g <= 0f) return;
            float sw = MathF.Sin(t * 1.3f + id) * .1f;
            for (int i = 0; i < 2; i++) { float a = id * 1.7f + i * 2.4f; m.Turn(x, y, a, () => m.Ball(x + 54 * S * g, y, -6, 44 * S * g, 18 * S * g, 8 * S * g, m.C(p.Color2))); }
            var pc = m.C(Gfx.Mix(p.Color1, Gfx.Hsv(0, 0, 1), (id % 3) * .2f));
            for (int i = 0; i < 6; i++) { float a = i * MathF.Tau / 6f + sw; m.Pitch(x + MathF.Cos(a) * 25 * S * g, y + MathF.Sin(a) * 25 * S * g, 0, 0, () => m.Ball(x + MathF.Cos(a) * 25 * S * g, y + MathF.Sin(a) * 25 * S * g, 6, 20 * S * g, 20 * S * g, 9 * S * g, pc)); }
            m.Ball(x, y, 14, 14 * S * g, 14 * S * g, 10 * S * g, m.C(p.Color3));
        }
        int k = 0;
        int keep = m.Seg; m.Seg = 14;
        foreach (int sx in Sides) foreach (int sy in Sides)
            {
                float cx = sx * W / 2f, cy = sy * H / 2f; Flower(cx, cy, 1.9f, k++);
                for (int i = 1; i <= 4; i++) { Flower(cx - sx * i * 72, cy + (i % 2 == 0 ? 13 : -13) * sy, 1.15f - i * .13f, k++); Flower(cx + (i % 2 == 0 ? 13 : -13) * sx, cy - sy * i * 66, 1.15f - i * .13f, k++); }
            }
        m.Seg = keep;
    }

    /// <summary>ステンドグラス: 色ガラスの扇形の板(明るさがゆらぐ)と鉛の線、中心の星、石の枠、さしこむ光</summary>
    static void StainedGlass(StainedGlassParameter p, Sculpt m, float t)
    {
        const float R = 430; int n = Math.Clamp(p.Count, 6, 24); float[] rs = [R, R * .73f, R * .47f, R * .22f];
        var lead = m.C(p.Color1);
        int keep = m.Seg; m.Seg = 8;
        for (int r = 0; r < 3; r++)
            for (int i = 0; i < n; i++)
            {
                float a0 = i * MathF.Tau / n + r * MathF.PI / n, a1 = a0 + MathF.Tau / n, L = .5f + .16f * MathF.Sin(t * 1.1f + i * .8f + r);
                var c = Hsv4(m, i * 360f / n + r * 45f, .72f, Math.Clamp(L + .36f, 0f, 1f), .82f);
                var q = new List<Vector2>();
                for (int k = 0; k <= 8; k++) { float a = a0 + (a1 - a0) * k / 8f; q.Add(new(MathF.Cos(a) * rs[r], MathF.Sin(a) * rs[r])); }
                for (int k = 8; k >= 0; k--) { float a = a0 + (a1 - a0) * k / 8f; q.Add(new(MathF.Cos(a) * rs[r + 1], MathF.Sin(a) * rs[r + 1])); }
                m.Slab(q, 0, 10, c);
                m.Tube([.. q.Append(q[0]).Select(u => new Vector3(u, 0))], 3.5f, lead);
            }
        m.Seg = keep;
        m.Disc(0, 0, 0, rs[3], rs[3], 10, m.C(0xFFD54A), .2f);
        m.Slab(8, 6, m.C(0xFFFFFF), StarPts(0, 0, rs[3] * .9f, 8, .45f, t * .12f));
        m.Torus(0, 0, 0, rs[3], 4, lead);
        m.Torus(0, 0, 0, R + 8, 12, lead);
        m.Torus(0, 0, 0, R + 30, 16, m.C(0xB08D57));
        // さしこむ光: 窓の円の内側だけに、ゆっくり動く明るい帯(うすく)
        float sw = MathF.Sin(t * .45f) * 280f;
        var band = new List<float>();
        for (int i = 0; i <= 24; i++) { float y = -R + i * R * 2f / 24f, hw = MathF.Sqrt(MathF.Max(0f, R * R - y * y)), x = Math.Clamp(sw + y * .5f - 100, -hw, hw); band.Add(x); band.Add(y); }
        for (int i = 24; i >= 0; i--) { float y = -R + i * R * 2f / 24f, hw = MathF.Sqrt(MathF.Max(0f, R * R - y * y)), x = Math.Clamp(sw + y * .5f + 100, -hw, hw); band.Add(x); band.Add(y); }
        m.Slab(14, 2, m.C(p.Color2, .16f), [.. band]);
    }

    /// <summary>チェッカーフラッグ: 波打つ布(白黒のマス目に陰)と、旗ざお・金の玉</summary>
    static void CheckerFlag(CheckeredFlagParameter p, Sculpt m, float t)
    {
        const int nx = 16, ny = 10; const float x0 = -380, y0 = -300, cell = 52;
        Vector3 G(int x, int y) { float e = x / (float)nx, w = MathF.Sin(e * 7f - t * 6f + y * .16f) * 46f * e, wx = MathF.Cos(e * 7f - t * 6f) * 10f * e; return new(x0 + x * cell + wx, y0 + y * cell + w * .35f, w * 1.4f); }
        var c1 = m.C(p.Color1); var c2 = m.C(p.Color2);
        for (int y = 0; y < ny; y++) for (int x = 0; x < nx; x++) m.Quad3D(G(x, y), G(x + 1, y), G(x + 1, y + 1), G(x, y + 1), (x + y) % 2 == 0 ? c1 : c2);
        m.Cyl(x0 - 13, 40, 0, 13, 790, m.C(0xB9BEC6), .2f);
        m.Ball(x0 - 13, -360, 0, 23, m.C(0xFFD54A));
    }

    /// <summary>風船が飛ぶ: 奥行きもばらばらに昇る、つやのある風船(結び目)とゆれるひも</summary>
    static void FloatBalloons(FloatingBalloonsParameter p, Sculpt m, float t)
    {
        int n = Math.Clamp(p.Count, 1, 80); float baseH = Gfx.Hue(p.Color1);
        int keep = m.Seg; m.Seg = 20;
        for (int i = 0; i < n; i++)
        {
            float d = .55f + (i + 1f) / n * .75f, R = 62f * d, span = m.H + R * 7f, y = m.H / 2f + R * 3f - Fr(t * .07f * (.7f + d * .5f) + R01(i + 5)) * span, x = (R01(i) - .5f) * m.W + MathF.Sin(t * .8f + i) * 26f, z = (d - 1f) * 600f;
            var c = Hsv4(m, baseH + R01(i + 9) * 300f, .68f, 1f);
            m.Tube([.. Enumerable.Range(0, 9).Select(k => { float e = k / 8f; return new Vector3(x + MathF.Sin(e * 12f + t * 2f + i) * 7f * d, y + R * 1.16f + e * R * 2.3f, z); })], 1, m.C(0xFFFFFF, .55f));
            m.Ball(x, y, z, R, R * 1.18f, R, c);
            m.Lathe(x, z, c, 1, y + R * 1.08f, 10 * d, y + R * 1.25f, 0, y + R * 1.25f);
            m.Ball(x - R * .33f, y - R * .42f, z + R * .8f, R * .15f, R * .26f, R * .06f, m.C(0xFFFFFF, .62f));
        }
        m.Seg = keep;
    }

    /// <summary>くす玉が割れる: つり糸、左右に開く半球の玉(筋)、下りてくる垂れ幕と文字、舞う紙吹雪</summary>
    static void Kusudama(KusudamaParameter p, Sculpt m, float t, float v)
    {
        const float cy = -270, R = 165; float at = v * 5f, k = DrawKit.EaseOut(Cl((m.Now - at) * 2.2f)), ang = k * 1.05f;
        m.Limb(0, -520, 0, 0, cy - R, 0, 2.5f, 2.5f, m.C(0xFFFFFF));
        if (k > 0f)
        {
            float drop = Cl((m.Now - at) * 1.5f), Hh = drop * 500f;
            if (Hh > 4f)
            {
                m.Box(0, cy + Hh / 2f, 0, 170, Hh, 6, m.C(p.Color2), .05f);
                foreach (int s in Sides) m.Box(s * 85, cy + Hh / 2f, 0, 8, Hh, 8, m.C(p.Color3), .2f);
                string s2 = Str(p).Replace(" ", ""); float fs = MathF.Min(100f, 440f / MathF.Max(1, s2.Length));
                for (int i = 0; i < s2.Length; i++) { float y = cy + 45 + fs * .55f + i * fs * 1.03f; if (y < cy + Hh - fs * .3f) m.Text(s2[i].ToString(), fs, 0, y, 6, 8, m.C(p.Color3)); }
            }
            uint[] cs = [0xFF5C8A, 0x4FC3F7, 0xFFE45C, 0x66E08A, 0xFFFFFF];
            for (int i = 0; i < 90; i++)
            {
                float e = Cl((m.Now - at) * (.45f + R01(i + 7) * .3f)); if (e >= 1f) continue;
                float a = R01(i) * MathF.Tau, b = (R01(i + 11) - .5f) * 2f, sp = 120f + R01(i + 3) * 520f, mm = MathF.Min(e * 2.2f, 1f);
                float x = MathF.Cos(a) * sp * mm + MathF.Sin(e * 9f + i) * 18f, y = cy + MathF.Sin(a) * sp * .4f * mm + e * e * 820f, z = b * sp * .6f * mm;
                m.Pitch(x, y, z, e * 8f + i, () => m.Turn(x, y, e * 12f + i, () => m.Box(x, y, z, 20, 10, 2, m.C(cs[i % 5], 1f - e * e), .2f)));
            }
        }
        var gold = m.C(p.Color1); var ink = m.C(0x8A6A10, .5f);
        var prof = new List<Vector2>(); for (int i = 0; i <= 14; i++) { float a = MathF.PI * i / 14f; prof.Add(new(MathF.Sin(a) * R, cy - MathF.Cos(a) * R)); }
        foreach (int s in Sides)
            m.Turn(s * 7, cy - R + 3, s * -ang, () =>
            {
                m.Lathe(s * 7, 0, prof, s > 0 ? gold : Vector4.Lerp(gold, m.C(0xFFFFFF), .16f), s > 0 ? -MathF.PI / 2f : MathF.PI / 2f, s > 0 ? MathF.PI / 2f : MathF.PI * 1.5f);
                for (int i = 1; i < 4; i++) { float a = (s > 0 ? 0 : MathF.PI) + (i - 2) * .5f; m.Yaw(s * 7, cy, 0, -a, () => m.Tube([.. Sculpt.ArcPts(s * 7, cy, R * 1.01f, -MathF.PI / 2f, MathF.PI / 2f, 14).Select(q => new Vector3(s * 7 + (q.Y - cy) * 0f + R * 1.01f * MathF.Cos(MathF.Asin(Math.Clamp((q.Y - cy) / (R * 1.01f), -1f, 1f))), q.Y, 0))], 1.5f, ink)); }
            });
    }

    /// <summary>ガーランド: たるんだひもと、ぶら下がってゆれる三角の旗(奥行き方向にも並ぶ)</summary>
    static void Garland(GarlandFlagsParameter p, Sculpt m, float t)
    {
        float Wd = m.W + 60, top = -m.H / 2f + 40; int n = Math.Clamp(p.Count, 4, 40);
        float Y(float e) => top + MathF.Sin(e * MathF.PI) * 115f + MathF.Sin(t * 1.3f + e * 4f) * 7f;
        float Z(float e) => MathF.Sin(e * MathF.PI) * 160f;
        m.Tube([.. Enumerable.Range(0, 41).Select(i => { float e = i / 40f; return new Vector3(-Wd / 2f + e * Wd, Y(e), Z(e)); })], 2, m.C(0xFFFFFF));
        for (int i = 0; i < n; i++)
        {
            float e = (i + .5f) / n, x = -Wd / 2f + e * Wd, y = Y(e), z = Z(e), fw = MathF.Min(52f, Wd / n * .42f), fh = fw * 2.2f, sw = MathF.Sin(t * 2.4f + i * .9f) * .3f, g = DrawKit.EaseOutBack(Cl((m.Now - i * .035f) * 3.5f));
            var c = m.C((i % 3) switch { 0 => p.Color1, 1 => p.Color2, _ => p.Color3 });
            m.Pitch(x, y, z, sw, () => m.Scale(x, y, z, g, () => { m.Slab(z, 3, c, x - fw, y, x + fw, y, x, y + fh); m.Slab(z + 2, 1, m.C(0xFFFFFF, .24f), x - fw, y, x, y, x, y + fh); }));
        }
    }

    /// <summary>垂れ幕: 金の棒(端の玉)、下りてくる布(ゆれる)とたて書きの立体の文字、下の重しの棒</summary>
    static void Banner(HangingBannerParameter p, Sculpt m, float t)
    {
        string s = Str(p).Replace("\n", ""); int n = Math.Max(1, s.Length); const float W = 250;
        float full = MathF.Min(860f, n * 150f + 70), top = -430, k = DrawKit.EaseOut(Cl(t * .85f)), Hh = full * k, sw = MathF.Sin(m.Now * 2f) * 7f * (1f - Cl((m.Now - 1.2f) * .4f));
        m.Quad3D(V3(-W / 2f, top, 0), V3(W / 2f, top, 0), V3(W / 2f + sw, top + Hh, sw * 2), V3(-W / 2f + sw, top + Hh, sw * 2), m.C(p.Color1));
        foreach (int e in Sides) m.Limb(e * (W / 2f - 12), top, 2, e * (W / 2f - 12) + sw, top + Hh, 2 + sw * 2, 2.5f, 2.5f, m.C(p.Color2));
        float fs = MathF.Min(130f, (full - 70) / n * .94f);
        for (int i = 0; i < s.Length; i++) { float y = top + 34 + fs * .54f + i * (full - 70) / n; if (y + fs * .46f < top + Hh) { float f = (y - top) / MathF.Max(1f, Hh); m.Text(s[i].ToString(), fs, sw * f, y, 6 + sw * 2 * f, 8, m.C(p.Color2)); } }
        m.Limb(-W / 2f - 45, top, 0, W / 2f + 45, top, 0, 13, 13, m.C(p.Color3));
        foreach (int e in Sides) m.Ball(e * (W / 2f + 48), top, 0, 19, m.C(p.Color3));
        m.Limb(-W / 2f - 17 + sw, top + Hh, sw * 2, W / 2f + 17 + sw, top + Hh, sw * 2, 10, 10, m.C(p.Color3));
    }

    /// <summary>花束: 茎と葉、丸く盛った花(バラのような巻き)、円すいの包み紙、リボンの結び目</summary>
    static void Bouquet(FlowerBouquetParameter p, Sculpt m, float t)
    {
        float sw = MathF.Sin(t * 1.2f) * .035f;
        m.Turn(0, 310, sw, () =>
        {
            for (int i = -3; i <= 3; i++) m.Limb(i * 8, 400, 0, i * 46, 110, i * 20, 4, 4, m.C(0x3F8F48));
            int n = Math.Clamp(p.Count, 3, 30);
            m.Ball(0, -60, -40, 275, 215, 150, m.C(0x4FA84F));
            for (int i = 0; i < 9; i++) { float a = MathF.PI + i * MathF.PI / 8f; m.Turn(MathF.Cos(a) * 240, -55 + MathF.Sin(a) * 180, a, () => m.Ball(MathF.Cos(a) * 240, -55 + MathF.Sin(a) * 180, -20, 88, 30, 14, m.C(0x3F9A4A))); }
            float hue = Gfx.Hue(p.Color1);
            for (int i = 0; i < n; i++)
            {
                float a = i * 2.4f, r = MathF.Sqrt((i + .5f) / n) * 205f, x = MathF.Cos(a) * r, y = -75 + MathF.Sin(a) * r * .8f, S = 58f + R01(i) * 16f, g = DrawKit.EaseOutBack(Cl((m.Now - .1f - i * .04f) * 3f));
                if (g <= 0f) continue;
                float z = 110 - r * .3f;
                var col = Hsv4(m, hue + (i % 4) * 16f - 18f, .55f + (i % 3) * .1f, 1f);
                m.Ball(x, y, z, S * g, S * g, S * g * .7f, col);
                var dk = m.C(Gfx.Mix(Gfx.Hsv(hue + (i % 4) * 16f - 18f, .55f + (i % 3) * .1f, 1f), Gfx.Hsv(340, .87f, .48f), .42f));
                for (int l = 0; l < 3; l++) m.Tube(Sculpt.OnFront(x, y, z, S * g, S * g, S * g * .7f, 1f, Sculpt.ArcPts(x, y, S * g * (.78f - l * .24f), i + l * 2f, i + l * 2f + 4.3f, 10)), 2, dk);
            }
            var wrap = m.C(p.Color2);
            m.Lathe(0, 40, wrap, 245, 40, 66, 350, 0, 350);
            var rib = m.C(p.Color3);
            m.Limb(-74, 280, 110, 74, 280, 110, 18, 18, rib);
            foreach (int s in Sides) { m.Ball(s * 80, 255, 120, 55, 30, 18, rib); m.Limb(s * 8, 285, 125, s * 70, 410, 135, 10, 6, rib); }
            m.Ball(0, 279, 130, 25, 25, 18, Vector4.Lerp(rib, m.C(0xFFFFFF), .25f));
        });
    }

    /// <summary>紙飛行機: 折り目のある三角の翼と胴(立体の紙)、宙返りする飛び方と、後ろに残る点線</summary>
    static void PaperPlane(PaperPlaneParameter p, Sculpt m, float t)
    {
        Vector3 At(float k) { float x = -m.W * .6f + k * m.W * 1.2f, y = m.H * .3f - k * m.H * .55f + MathF.Sin(k * 9f) * m.H * .16f, z = MathF.Sin(k * 5f) * 300f; if (k is > .42f and < .62f) { float e = (k - .42f) / .2f * MathF.Tau; x += MathF.Sin(e) * 105f; y += (1f - MathF.Cos(e)) * -105f; } return new(x, y, z); }
        float now = Fr(t * .13f);
        for (int i = 0; i < 90; i += 2) { float k = now - i * .0052f; if (k <= 0f) break; m.Limb(At(k), At(MathF.Max(0f, k - .0052f)), 2, 2, m.C(0xFFFFFF, .8f * (1f - i / 90f))); }
        var q = At(now); var f = At(now + .003f) - q;
        float ang = MathF.Atan2(f.Y, f.X), yaw = MathF.Atan2(-f.Z, MathF.Sqrt(f.X * f.X + f.Y * f.Y)), wob = MathF.Sin(t * 5f) * .06f;
        var paper = m.C(p.Color1); var fold = m.C(p.Color2);
        m.Move(q.X, q.Y, q.Z, () => m.Yaw(0, 0, 0, yaw, () => m.Turn(0, 0, ang + wob, () => m.Pitch(0, 0, 0, MathF.Sin(t * 3f) * .2f, () =>
        {
            m.Tri3D(V3(112, 0, 0), V3(-105, -6, -88), V3(-58, 0, -4), paper);
            m.Tri3D(V3(112, 0, 0), V3(-105, -6, 88), V3(-58, 0, 4), paper);
            m.Tri3D(V3(112, 0, -1), V3(-58, 0, -4), V3(-76, 26, 0), fold);
            m.Tri3D(V3(112, 0, 1), V3(-58, 0, 4), V3(-76, 26, 0), Vector4.Lerp(paper, fold, .45f));
        }))));
    }

    /// <summary>折り鶴: 折り目で明るさの変わる面を組んだ立体の鶴(胴・はばたく左右の翼・首とくちばし・尾)</summary>
    static void Crane(OrigamiCraneParameter p, Sculpt m, float t)
    {
        float y = MathF.Sin(t * 1.3f) * 14f, f = MathF.Sin(t * 1.9f) * .35f, r = MathF.Sin(t * 1.0f) * .035f;
        Vector4 L(float k) => m.C(k >= 0f ? Gfx.Mix(p.Color1, p.Color2, k) : Gfx.Mix(p.Color1, Gfx.Hsv(0, 0, 0), -k));
        m.Super(0, 340, 0, 250, 2, 120, 1f, 1f, m.C(0x000000, .2f));
        m.Move(0, y, () => m.Turn(0, 90, r, () =>
        {
            var top = V3(30, 60, 0); var front = V3(160, 110, 0); var back = V3(-110, 90, 0); var bot = V3(30, 250, 0);
            foreach (int s in Sides)
            {
                var side = V3(30, 130, s * 90);
                m.Tri3D(top, front, side, L(.1f * s)); m.Tri3D(top, back, side, L(-.15f * s)); m.Tri3D(bot, front, side, L(-.25f)); m.Tri3D(bot, back, side, L(-.35f));
                m.Pitch(30, 70, 0, s * (.5f + f * .6f), () =>
                {
                    m.Tri3D(V3(-60, 70, 0), V3(120, 70, 0), V3(-80, -200, 0), L(s > 0 ? .1f : -.2f));
                    m.Tri3D(V3(-80, -200, 0), V3(120, 70, 0), V3(20, -280, 0), L(s > 0 ? -.05f : -.3f));
                });
            }
            m.Tri3D(front, V3(140, 80, 8), V3(360, -130, 0), L(-.28f)); m.Tri3D(front, V3(140, 80, -8), V3(360, -130, 0), L(-.1f));
            m.Tri3D(V3(360, -130, 0), V3(310, -148, 6), V3(455, -82, 0), L(.04f)); m.Tri3D(V3(360, -130, 0), V3(310, -148, -6), V3(455, -82, 0), L(-.1f));
            m.Tri3D(V3(455, -82, 0), V3(500, -18, 0), V3(410, -62, 4), L(-.05f));
            m.Tri3D(back, V3(-100, 60, 8), V3(-445, -105, 0), L(-.42f)); m.Tri3D(back, V3(-100, 60, -8), V3(-445, -105, 0), L(-.2f));
        }));
    }
}
