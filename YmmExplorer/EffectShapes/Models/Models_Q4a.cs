using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 210_QThings4.cs の図形(食べ物・動物・乗り物 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)。2Dの絵の位置・大きさ・動きに合わせて立体を組む
// =====================================================================
internal static partial class Models3D
{
    static partial void RegisterQ4a(Dictionary<Type, QuickModel> d)
    {
        d[typeof(OnigiriParameter)] = (p, m, t, v, v2) => Onigiri((OnigiriParameter)p, m, t);
        d[typeof(HamburgerParameter)] = (p, m, t, v, v2) => Hamburger((HamburgerParameter)p, m, t);
        d[typeof(SushiSetParameter)] = (p, m, t, v, v2) => Sushi((SushiSetParameter)p, m, t);
        d[typeof(TakoyakiParameter)] = (p, m, t, v, v2) => Takoyaki((TakoyakiParameter)p, m, t);
        d[typeof(DonutParameter)] = (p, m, t, v, v2) => Donut((DonutParameter)p, m, t);
        d[typeof(SoftCreamParameter)] = (p, m, t, v, v2) => SoftCream((SoftCreamParameter)p, m, t);
        d[typeof(CustardPuddingParameter)] = (p, m, t, v, v2) => Pudding((CustardPuddingParameter)p, m, t);
        d[typeof(CoffeeCupParameter)] = (p, m, t, v, v2) => Coffee((CoffeeCupParameter)p, m, t);
        d[typeof(FriedEggParameter)] = (p, m, t, v, v2) => FriedEgg((FriedEggParameter)p, m, t);
        d[typeof(ShavedIceParameter)] = (p, m, t, v, v2) => ShavedIce((ShavedIceParameter)p, m, t);
        d[typeof(CatFaceParameter)] = (p, m, t, v, v2) => Cat((CatFaceParameter)p, m, t);
        d[typeof(DogFaceParameter)] = (p, m, t, v, v2) => Dog((DogFaceParameter)p, m, t);
        d[typeof(BunnyHopParameter)] = (p, m, t, v, v2) => Bunny((BunnyHopParameter)p, m, t);
        d[typeof(PenguinWalkParameter)] = (p, m, t, v, v2) => Penguin((PenguinWalkParameter)p, m, t);
        d[typeof(PandaBambooParameter)] = (p, m, t, v, v2) => Panda((PandaBambooParameter)p, m, t);
        d[typeof(ChickHopParameter)] = (p, m, t, v, v2) => Chick((ChickHopParameter)p, m, t);
        d[typeof(FrogCroakParameter)] = (p, m, t, v, v2) => Frog((FrogCroakParameter)p, m, t);
        d[typeof(HamsterWheelParameter)] = (p, m, t, v, v2) => Hamster((HamsterWheelParameter)p, m, t);
        d[typeof(CrowFlockParameter)] = (p, m, t, v, v2) => Crows((CrowFlockParameter)p, m, t);
        d[typeof(HoneyBeeParameter)] = (p, m, t, v, v2) => Bees((HoneyBeeParameter)p, m, t);
        d[typeof(DrivingCarParameter)] = (p, m, t, v, v2) => Car((DrivingCarParameter)p, m, t);
        d[typeof(PassingTrainParameter)] = (p, m, t, v, v2) => Train((PassingTrainParameter)p, m, t);
        d[typeof(ContrailPlaneParameter)] = (p, m, t, v, v2) => Plane((ContrailPlaneParameter)p, m, t);
        d[typeof(HelicopterParameter)] = (p, m, t, v, v2) => Heli((HelicopterParameter)p, m, t);
        d[typeof(RocketLaunchParameter)] = (p, m, t, v, v2) => Rocket((RocketLaunchParameter)p, m, t);
        d[typeof(SailingShipParameter)] = (p, m, t, v, v2) => Ship((SailingShipParameter)p, m, t);
        d[typeof(BicycleRideParameter)] = (p, m, t, v, v2) => Bicycle((BicycleRideParameter)p, m, t);
        d[typeof(HotAirBalloonParameter)] = (p, m, t, v, v2) => Balloons((HotAirBalloonParameter)p, m, t);
        d[typeof(RotatingBeaconParameter)] = (p, m, t, v, v2) => Beacon((RotatingBeaconParameter)p, m, t);
        d[typeof(SubmarineParameter)] = (p, m, t, v, v2) => Submarine((SubmarineParameter)p, m, t);
    }

    static readonly int[] Sides = [-1, 1];
    static float R01(int n) => ArtPen.R01(n);
    static float Cl(float x) => Math.Clamp(x, 0f, 1f);
    static float Fr(float x) => x - MathF.Floor(x);

    /// <summary>湯気: ゆらゆら昇る白い管</summary>
    static void Steam(Sculpt m, int n, float x0, float gap, float y0, float len, float z, float t, float r)
    {
        for (int s = 0; s < n; s++)
        {
            var q = new List<Vector3>();
            for (int i = 0; i <= 10; i++) { float k = i / 10f; q.Add(new(x0 + (s - (n - 1) / 2f) * gap + MathF.Sin(k * 7f - t * 2f + s) * 20f, y0 - k * len, z + MathF.Cos(k * 5f - t * 1.5f + s) * 14f)); }
            m.Tube(q, r, m.C(0xFFFFFF, .4f));
        }
    }

    /// <summary>半球のドーム(回転体の断面)。上が y0-h、下が y0</summary>
    static List<Vector2> Dome(float r, float h, float y0, int steps = 12)
    {
        var l = new List<Vector2>();
        for (int i = 0; i <= steps; i++) { float a = MathF.PI / 2f * i / steps; l.Add(new(MathF.Sin(a) * r, y0 - MathF.Cos(a) * h)); }
        return l;
    }

    // ================================================================ 食べ物
    /// <summary>おにぎり: 丸みのある三角のごはんと、前後を包むのり</summary>
    static void Onigiri(OnigiriParameter p, Sculpt m, float t)
    {
        float s = 1f + MathF.Sin(t * 4f) * .025f;
        m.Zoom(0, 200, 1f / s, s, () =>
        {
            var rice = m.C(p.Color1);
            m.Ball(0, 70, 0, 230, 150, 118, rice);
            m.Ball(0, -60, 0, 160, 150, 112, rice);
            m.Ball(0, -168, 0, 84, 84, 92, rice);
            foreach (int sd in Sides) m.Ball(sd * 172, 140, 0, 92, 74, 104, rice);
            m.Box(0, 170, 0, 480, 80, 220, rice, .35f);
            for (int i = 0; i < 26; i++)
            {
                float x = (R01(i) - .5f) * 280f, y = -120f + R01(i + 40) * 170f * (1f - MathF.Abs(x) / 300f);
                m.Ball(x, y, Sculpt.FrontZ(0, -20, 0, 250, 240, 118, x, y) + 1f, 8, 4, 3, m.C(0xE8E0CC));
            }
            m.Box(0, 140, 0, 200, 170, 250, m.C(p.Color2), .12f);
            m.Limb(-82, 98, 126, 82, 98, 126, 2, 2, m.C(0x3A4A40)); m.Limb(-82, 160, 126, 82, 160, 126, 2, 2, m.C(0x3A4A40));
        });
    }

    /// <summary>ハンバーガー: 落ちてきて積み重なる、丸いパン・パティ・チーズ・トマト・レタス・ゴマ付きのパン</summary>
    static void Hamburger(HamburgerParameter p, Sculpt m, float t)
    {
        float Y(int i, float rest) { float q = Cl((t - .1f - i * .16f) / .32f); return rest - (1f - q) * (1f - q) * 620f; }
        float Al(int i) => Cl((t - .1f - i * .16f) * 12f);
        m.Super(0, Y(0, 170), 0, 250, 43, 250, .5f, 1f, m.C(p.Color1, Al(0)));
        m.Super(0, Y(1, 105), 0, 265, 36, 265, .45f, 1f, m.C(0x6B3A1F, Al(1)));
        float cy = Y(2, 66); m.Yaw(0, cy, 0, MathF.PI / 4f, () => m.Box(0, cy, 0, 390, 12, 390, m.C(0xFFCC33, Al(2)), .06f));
        float ty = Y(3, 24); m.Cyl(-110, ty, -40, 112, 30, m.C(0xE5392F, Al(3)), .4f); m.Cyl(110, ty, 40, 112, 30, m.C(0xE5392F, Al(3)), .4f);
        float ly = Y(4, -14); m.Super(0, ly, 0, 292, 13, 292, .6f, 1f, m.C(0x63B446, Al(4)));
        for (int i = 0; i < 14; i++) { float a = i * MathF.Tau / 14f; m.Ball(MathF.Cos(a) * 280f, ly + MathF.Sin(i * 1.7f) * 8f, MathF.Sin(a) * 280f, 34, 12, 34, m.C(0x63B446, Al(4))); }
        float by = Y(5, -40), ba = Al(5);
        var dome = Dome(262f, 196f, by); dome.Add(new(250f, by + 14f)); dome.Add(new(0f, by + 14f));
        m.Lathe(0, 0, dome, m.C(p.Color1, ba));
        for (int i = 0; i < 18; i++)
        {
            float ph = R01(i) * MathF.Tau, th = .25f + R01(i + 20) * 1.0f;
            m.Ball(MathF.Sin(th) * MathF.Cos(ph) * 262f, by - MathF.Cos(th) * 196f, MathF.Sin(th) * MathF.Sin(ph) * 262f, 10, 5, 7, m.C(0xFFF3D0, ba));
        }
    }

    /// <summary>お寿司: 下駄の上に並ぶ3貫(シャリ・ネタ・のり)</summary>
    static void Sushi(SushiSetParameter p, Sculpt m, float t)
    {
        var wood = m.C(p.Color1);
        m.Box(0, 112, 0, 900, 50, 300, wood, .1f);
        foreach (int s in Sides) m.Box(s * 320, 162, 0, 56, 62, 270, Sculpt.Dark(wood, .85f), .1f);
        uint[] net = [0xD72638, 0xFFD34E, 0xFF8A5C];
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 280f, q = DrawKit.EaseOutBack(Cl((t - .1f - i * .18f) / .35f)), y = 40 - (1f - q) * 90f, al = Cl((t - .1f - i * .18f) * 8f);
            m.Box(x, y + 18, 0, 214, 76, 130, m.C(0xFFFDF5, al), .5f);
            m.Super(x, y - 24, 0, 126, 26, 76, .45f, .3f, m.C(net[i], al));
            if (i == 1) m.Box(x, y + 4, 0, 44, 112, 140, m.C(0x1F2A24, al), .15f);
            else for (int k = -3; k <= 3; k++) m.Limb(x + k * 32 - 8, y - 46, 50, x + k * 32 + 8, y - 10, 70, 2.5f, 2.5f, m.C(0xFFFFFF, .45f * al));
        }
    }

    /// <summary>たこ焼き: 舟皿に並ぶ丸いたこ焼き。ソース・マヨネーズ・青のり・湯気</summary>
    static void Takoyaki(TakoyakiParameter p, Sculpt m, float t)
    {
        m.Super(0, 168, 0, 430, 20, 250, .25f, .8f, m.C(0xD9B780));
        m.Super(0, 150, 0, 440, 8, 260, .5f, .8f, m.C(0x8A6A3C));
        int n = Math.Clamp(p.Count, 1, 6);
        for (int i = 0; i < n; i++)
        {
            float x = (i % 3 - 1) * 220f, z = i < 3 ? -95f : 95f, y = 50f;
            m.Ball(x, y, z, 94, m.C(p.Color1));
            m.Super(x, y - 26, z, 96, 66, 96, 1f, 1f, m.C(p.Color2));
            var q = new List<Vector3>();
            for (int k = 0; k <= 6; k++) { float u = k / 6f; float xx = x - 66 + u * 134; q.Add(new(xx, y - 88 + (k % 2) * 10, z + (k % 2 == 0 ? -20 : 20))); }
            m.Tube(q, 4.5f, m.C(0xFFF8DF));
            for (int k = 0; k < 9; k++) m.Ball(x + (R01(i * 9 + k) - .5f) * 110f, y - 90f, z + (R01(i * 7 + k + 3) - .5f) * 110f, 5, 3, 5, m.C(0x3E8E3E));
        }
        Steam(m, 4, 0, 120, -80, 210, 0, t, 5);
    }

    /// <summary>ドーナツ: 輪の生地と、表にかかったチョコ、カラフルなトッピング</summary>
    static void Donut(DonutParameter p, Sculpt m, float t)
    {
        m.Turn(0, 0, t * .6f, () =>
        {
            m.Torus(0, 0, -6, 190, 88, m.C(p.Color2));
            m.Torus(0, 0, 22, 192, 80, m.C(p.Color1));
            uint[] cs = [0xFF6FA8, 0xFFE45C, 0x5CD6FF, 0xFFFFFF];
            for (int i = 0; i < 44; i++)
            {
                float a = R01(i) * MathF.Tau, r = 125f + R01(i + 50) * 135f, rot = R01(i + 90) * 6f;
                float b = MathF.Asin(Math.Clamp((r - 192f) / 80f, -.95f, .95f)), z = 22f + 80f * MathF.Cos(b) + 2f;
                float x = MathF.Cos(a) * r, y = MathF.Sin(a) * r;
                m.Limb(x - MathF.Cos(rot) * 11, y - MathF.Sin(rot) * 11, z, x + MathF.Cos(rot) * 11, y + MathF.Sin(rot) * 11, z, 4, 4, m.C(cs[i % 4]));
            }
        });
    }

    /// <summary>ソフトクリーム: 円すいのコーンと、うず巻きに重なるクリーム</summary>
    static void SoftCream(SoftCreamParameter p, Sculpt m, float t)
    {
        var cone = m.C(p.Color2);
        m.Lathe(0, 0, cone, 0, 22, 138, 22, 135, 34, 0, 410);
        for (int i = 0; i < 5; i++) { float y = 70 + i * 62f, r = 135f * (410f - y) / 380f; m.Pitch(0, y, 0, MathF.PI / 2f, () => m.Torus(0, y, 0, r, 3.5f, Sculpt.Dark(cone, .8f))); }
        float sw = MathF.Sin(t * 2f) * .035f;
        m.Turn(0, 30, sw, () =>
        {
            float[] w = [170, 140, 108, 74];
            var cream = m.C(p.Color1);
            for (int i = 0; i < 4; i++) m.Super(0, 6 - i * 72f, 0, w[i], 50, w[i], .85f, 1f, cream);
            m.Lathe(0, 0, cream, 0, -340, 14, -322, 40, -270, 52, -248, 0, -240);
        });
    }

    /// <summary>プリン: お皿の上の台形のプリンと、上にかかったカラメル。ぷるぷる揺れる</summary>
    static void Pudding(CustardPuddingParameter p, Sculpt m, float t)
    {
        m.Super(0, 202, 0, 330, 10, 330, .3f, 1f, m.C(0xF4F7FA));
        float sx = MathF.Sin(t * 7f) * 22f * MathF.Exp(-Fr(t * .5f) * 2.4f);
        m.Turn(0, 190, sx / 520f, () =>
        {
            m.Lathe(0, 0, m.C(p.Color1), 0, -105, 128, -105, 136, -95, 205, 186, 0, 186);
            var car = m.C(p.Color2);
            m.Lathe(0, 0, car, 0, -112, 126, -112, 135, -104, 142, -66, 0, -66);
            for (int i = 0; i < 7; i++)
            {
                float a = i / 7f * MathF.Tau + .4f, y = -40f + R01(i) * 30f, r0 = 141f, r1 = 132f + (y + 105f) / 291f * 75f;
                m.Limb(MathF.Cos(a) * r0, -72, MathF.Sin(a) * r0, MathF.Cos(a) * r1, y, MathF.Sin(a) * r1, 15, 11, car);
            }
            m.Limb(-150, 140, 165, -110, -10, 112, 6, 5, m.C(0xFFFFFF, .45f));
        });
    }

    /// <summary>コーヒーカップ: 受け皿・カップ(中にコーヒー)・取っ手・湯気</summary>
    static void Coffee(CoffeeCupParameter p, Sculpt m, float t)
    {
        var cup = m.C(p.Color1);
        m.Super(0, 216, 0, 345, 12, 345, .3f, 1f, m.C(0xE9E2D5));
        m.Lathe(0, 0, cup, 213, -30, 225, -26, 216, 40, 185, 165, 115, 212, 0, 212);
        m.Lathe(0, 0, Sculpt.Dark(cup, .9f), 0, 120, 175, 120, 205, -26, 212, -30);
        m.Super(0, -18, 0, 203, 4, 203, .2f, 1f, m.C(p.Color2));
        m.Ball(-55, -23, -40, 62, 3, 20, m.C(0x8A5A36, .8f));
        m.Torus(235, 45, 0, 72, 16, cup);
        m.Limb(-160, 8, 168, -128, 158, 120, 6, 5, m.C(0xFFFFFF, .7f));
        Steam(m, Math.Clamp(p.Count, 0, 8), 0, 58, -70, 260, 0, t, 5.5f);
    }

    /// <summary>目玉焼き: フライパン(縁と柄)、ふちの焼けた白身、ぷっくりした黄身</summary>
    static void FriedEgg(FriedEggParameter p, Sculpt m, float t)
    {
        var pan = m.C(p.Color2);
        m.Disc(0, 0, -34, 330, 330, 30, pan, .3f);
        m.Torus(0, 0, -12, 322, 16, m.C(0x4B5057));
        m.Box(470, 30, -24, 360, 54, 44, m.C(0x20140E), .4f);
        var eg = new List<Vector2>();
        for (int i = 0; i < 48; i++) { float a = i / 48f * MathF.Tau, r = 225f + MathF.Sin(a * 3f + 1f) * 26f + MathF.Sin(a * 5f) * 15f + MathF.Sin(t * 5f + a * 6f) * 2.5f; eg.Add(new(MathF.Cos(a) * r, MathF.Sin(a) * r)); }
        m.Slab(eg, -10, 14, m.C(0xFFFFFF));
        m.Super(0, 0, -3, 228, 228, 10, 1f, 1f, m.C(0xFFFFFF));
        m.Ball(14, -6, 20, 94, 94, 52, m.C(p.Color1));
        m.Ball(-16, -36, 66, 22, 22, 6, m.C(0xFFFFFF, .75f));
        for (int i = 0; i < 16; i++) { float k = Fr(t * 1.8f + R01(i)), a = R01(i + 20) * MathF.Tau, r = 235f + k * 110f; m.Ball(MathF.Cos(a) * r, MathF.Sin(a) * r, k * 80f, 5f * (1f - k) + .5f, m.C(0xFFF3C4, 1f - k)); }
    }

    /// <summary>かき氷: 脚付きのガラスの器、山盛りの氷、上からかかったシロップ、きらめき</summary>
    static void ShavedIce(ShavedIceParameter p, Sculpt m, float t)
    {
        var glass = m.C(p.Color2, .85f);
        m.Lathe(0, 0, glass, 0, 34, 300, 30, 285, 60, 210, 190, 78, 192, 118, 278, 0, 280);
        var ice = Dome(290f, 280f, 30f); ice.Add(new(0, 30));
        m.Lathe(0, 0, ice, m.C(0xFAFDFF));
        var syrup = Dome(296f, 286f, 30f, 12).Take(9).ToList(); syrup.Add(new(0, -120));
        m.Lathe(0, 0, syrup, m.C(p.Color1, .88f));
        for (int i = 0; i < 14; i++)
        {
            float ph = R01(i) * MathF.Tau, th = .2f + R01(i + 8) * 1.1f, tw = MathF.Max(0f, MathF.Sin(t * 3f + i * 1.7f));
            m.Ball(MathF.Sin(th) * MathF.Cos(ph) * 298f, 30 - MathF.Cos(th) * 288f, MathF.Sin(th) * MathF.Sin(ph) * 298f, 14f * tw, m.C(0xFFFFFF));
        }
        m.Limb(-230, 70, 200, -170, 168, 150, 6, 5, m.C(0xFFFFFF, .7f));
    }

    // ================================================================ 動物
    /// <summary>ねこ: 三角の耳・縦長のひとみ・ひげ・しま模様・ゆれるしっぽ</summary>
    static void Cat(CatFaceParameter p, Sculpt m, float t)
    {
        var fur = m.C(p.Color1); var belly = m.C(p.Color2); var ink = m.C(0x20140E);
        float sw = MathF.Sin(t * 3f);
        var tail = new List<Vector3>();
        for (int i = 0; i <= 14; i++) { float k = i / 14f; tail.Add(new(120 + k * 190 + MathF.Sin(k * 3f + t * 3f) * k * 45f, 250 - k * 200 + sw * k * 28f, -150 - k * 40f)); }
        m.Seg = 18; m.Tube(tail, 21, fur); m.Seg = 28;
        m.Ball(0, 175, -70, 188, 165, 150, fur);
        m.Ball(0, 205, Sculpt.FrontZ(0, 175, -70, 188, 165, 150, 0, 205) - 46, 108, 122, 52, belly);
        foreach (int s in Sides)
        {
            m.Ball(s * 125, 255, -95, 76, 78, 100, fur);
            m.Limb(s * 76, 225, 40, s * 80, 312, 60, 34, 30, fur);
            m.Ball(s * 80, 328, 74, 58, 30, 60, belly);
        }
        const float hx = 0, hy = -70, hz = 0, rx = 222, ry = 178, rz = 168;
        m.Ball(hx, hy, hz, rx, ry, rz, fur);
        foreach (int s in Sides)
        {
            m.Slab(-40, 56, fur, s * 205, -110, s * 192, -290, s * 62, -192);
            m.Slab(-6, 8, m.C(0xFF9FB0), s * 178, -138, s * 172, -238, s * 98, -188);
        }
        for (int i = -1; i <= 1; i++) m.Tube(Sculpt.OnFront(hx, hy, hz, rx, ry, rz, 1f, [new(i * 40, -238), new(i * 37, -210), new(i * 34, -180)]), 6, m.C(0xB9702A));
        bool blink = Fr(t * .3f) > .94f;
        foreach (int s in Sides)
        {
            float ex = s * 92, ey = -80, ez = Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, ex, ey);
            if (blink) m.Tube(Sculpt.OnFront(hx, hy, hz, rx, ry, rz, 2f, Sculpt.ArcPts(ex, -82, 32, MathF.PI * 1.1f, MathF.PI * 1.9f)), 3.5f, ink);
            else
            {
                m.Ball(ex, ey, ez - 14, 34, 34, 22, m.C(0xFFFFFF));
                m.Ball(ex, ey, ez + 4, 14, 28, 8, ink);
                m.Ball(ex - 6, ey - 12, ez + 10, 7, 7, 3, m.C(0xFFFFFF));
            }
            for (int k = -1; k <= 1; k++)
            {
                float wx = s * 130, wy = -22 + k * 14;
                m.Limb(wx, wy, Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, wx, wy), s * 262, -38 + k * 36, 70, 1.6f, 1.2f, ink);
            }
        }
        float nz = Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, 0, -26);
        m.Ball(0, -26, nz, 16, 10, 8, m.C(0xFF7F92));
        foreach (int s in Sides) m.Tube(Sculpt.OnFront(hx, hy, hz, rx, ry, rz, 1.5f, Sculpt.ArcPts(s * 18, -10, 18, .2f, MathF.PI - .2f)), 2.5f, ink);
    }

    /// <summary>いぬ: 座った犬。丸い頭・垂れ耳・前に出た口元・胴・前足と後ろ足・振るしっぽ</summary>
    static void Dog(DogFaceParameter p, Sculpt m, float t)
    {
        var fur = m.C(p.Color1); var light = m.C(p.Color2); var ear = m.C(0x8A5A36); var ink = m.C(0x20140E);
        float wag = MathF.Sin(t * 12f), earSwing = MathF.Sin(t * 3f) * .06f;
        m.Ball(0, 190, -70, 180, 165, 150, fur);
        m.Ball(0, 215, Sculpt.FrontZ(0, 190, -70, 180, 165, 150, 0, 215) - 48, 105, 120, 52, light);
        foreach (int s in Sides)
        {
            m.Ball(s * 125, 262, -95, 78, 80, 105, fur);
            m.Limb(s * 82, 230, 40, s * 88, 318, 62, 36, 32, fur);
            m.Ball(s * 90, 335, 78, 56, 30, 62, light);
        }
        m.Yaw(90, 240, -200, wag * .55f, () => m.Limb(90, 240, -200, 210, 120, -250, 26, 20, fur));
        const float hx = 0, hy = -70, hz = 0, hrx = 215, hry = 185, hrz = 175;
        m.Ball(hx, hy, hz, hrx, hry, hrz, fur);
        foreach (int s in Sides)
            m.Turn(s * 185, -175, s * (.28f + earSwing), () =>
                m.Yaw(s * 200, -60, -20, -s * .35f, () => m.Ball(s * 200, -60, -20, 58, 135, 26, ear)));
        foreach (int s in Sides)
        {
            float ex = s * 82, ey = -100, ez = Sculpt.FrontZ(hx, hy, hz, hrx, hry, hrz, ex, ey);
            m.Ball(ex, ey, ez - 8, 27, 29, 16, ink);
            m.Ball(ex - s * 6, ey - 10, ez + 7, 8, 8, 4, m.C(0xFFFFFF));
        }
        const float mx = 0, my = 8, mz = 120, mrx = 112, mry = 84, mrz = 78;
        m.Ball(mx, my, mz, mrx, mry, mrz, light);
        float nz = Sculpt.FrontZ(mx, my, mz, mrx, mry, mrz, 0, -28);
        m.Ball(0, -28, nz - 6, 38, 27, 24, ink);
        m.Ball(-10, -36, nz + 14, 10, 6, 4, m.C(0xFFFFFF, .7f));
        m.Tube(Sculpt.OnFront(mx, my, mz, mrx, mry, mrz, 1f, [new(0, -4), new(0, 28)]), 2.6f, ink);
        m.Tube(Sculpt.OnFront(mx, my, mz, mrx, mry, mrz, 1f, Sculpt.ArcPts(-34, 22, 34, .15f, MathF.PI - .3f)), 2.6f, ink);
        m.Tube(Sculpt.OnFront(mx, my, mz, mrx, mry, mrz, 1f, Sculpt.ArcPts(34, 22, 34, .3f, MathF.PI - .15f)), 2.6f, ink);
        float l = 44f + MathF.Abs(MathF.Sin(t * 8f)) * 28f;
        float tz = Sculpt.FrontZ(mx, my, mz, mrx, mry, mrz, 0, 44) - 14;
        m.Pitch(0, 40, tz, .35f, () =>
        {
            m.Super(0, 44 + l / 2f, tz, 27, l / 2f, 10, .55f, 1f, m.C(0xFF7F92));
            m.Limb(0, 50, tz + 9, 0, 40 + l * .85f, tz + 9, 1.6f, 1.6f, m.C(0xD95A70));
        });
    }

    /// <summary>うさぎ: 跳ねるうさぎ。長い耳(内側の色)・丸いしっぽ・ほっぺ</summary>
    static void Bunny(BunnyHopParameter p, Sculpt m, float t)
    {
        float h = MathF.Abs(MathF.Sin(t * 3.5f)), y = -h * 150f, sq = MathF.Pow(1f - h, 4f) * .14f;
        var body = m.C(p.Color1); var pink = m.C(p.Color2);
        m.Super(0, 338, 0, 150 - h * 55, 4, 120 - h * 40, 1f, 1f, m.C(0x000000, .2f));
        m.Move(0, y, () => m.Zoom(0, 320, 1f + sq, 1f - sq, () =>
        {
            m.Ball(0, 200, -40, 168, 135, 140, body);
            m.Ball(0, 230, -175, 44, body);
            foreach (int s in Sides) { m.Ball(s * 85, 300, 60, 66, 30, 80, body); m.Ball(s * 125, 235, -70, 70, 70, 95, body); }
            foreach (int s in Sides)
                m.Turn(s * 60, -130, s * (.12f + h * .28f), () => m.Pitch(s * 60, -130, -10, -.2f, () =>
                {
                    m.Ball(s * 60, -285, -10, 48, 155, 34, body);
                    m.Ball(s * 60, -275, 16, 25, 120, 10, pink);
                }));
            const float hx = 0, hy = -30, hz = 0, rx = 170, ry = 140, rz = 140;
            m.Ball(hx, hy, hz, rx, ry, rz, body);
            foreach (int s in Sides)
            {
                float ex = s * 70, ey = -50, ez = Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, ex, ey);
                m.Ball(ex, ey, ez - 6, 22, 22, 14, m.C(0xB71C3C));
                m.Ball(ex - 6, ey - 8, ez + 6, 6, 6, 3, m.C(0xFFFFFF));
                m.Ball(s * 112, 14, Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, s * 112, 14) - 3, 26, 15, 6, m.C(p.Color2, .7f));
            }
            float nz = Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, 0, -2);
            m.Ball(0, -2, nz - 2, 14, 10, 8, pink);
            foreach (int s in Sides) m.Tube(Sculpt.OnFront(hx, hy, hz, rx, ry, rz, 1f, Sculpt.ArcPts(s * 14, 12, 14, .2f, MathF.PI - .2f)), 2.4f, m.C(0x444444));
        }));
    }

    /// <summary>ペンギン: よちよち歩くペンギン。白いお腹・羽・くちばし・足</summary>
    static void Penguin(PenguinWalkParameter p, Sculpt m, float t)
    {
        float w = MathF.Sin(t * 5f);
        var back = m.C(p.Color1); var beak = m.C(p.Color2);
        m.Turn(0, 330, w * .13f, () =>
        {
            foreach (int s in Sides) m.Ball(s * 70, 330 - MathF.Max(0f, s * w) * 26, 60, 64, 22, 70, beak);
            m.Ball(0, 70, 0, 205, 270, 175, back);
            m.Ball(0, 115, Sculpt.FrontZ(0, 70, 0, 205, 270, 175, 0, 115) - 60, 150, 210, 70, m.C(0xFFFFFF));
            foreach (int s in Sides)
            {
                m.Turn(s * 190, -40, s * (.25f + s * w * .28f), () => m.Ball(s * 205, 70, -10, 40, 130, 60, back));
                float ex = s * 64, ey = -70, ez = Sculpt.FrontZ(0, 70, 0, 205, 270, 175, ex, ey);
                m.Ball(ex, ey, ez - 10, 34, 34, 16, m.C(0xFFFFFF));
                m.Ball(s * 58, ey, ez + 2, 17, 17, 8, m.C(0x101010));
                m.Ball(s * 52, -76, ez + 7, 5, 5, 3, m.C(0xFFFFFF));
                m.Ball(s * 118, -10, Sculpt.FrontZ(0, 70, 0, 205, 270, 175, s * 118, -10) - 2, 22, 13, 5, m.C(0xFFA9BC, .75f));
            }
            float bz = Sculpt.FrontZ(0, 70, 0, 205, 270, 175, 0, -28);
            m.Limb(0, -28, bz - 20, 0, -18, bz + 45, 30, 3, beak);
        });
    }

    /// <summary>パンダ: 座って笹を食べるパンダ。黒い耳・目のまわり・手足、もぐもぐ口</summary>
    static void Panda(PandaBambooParameter p, Sculpt m, float t)
    {
        float chew = MathF.Abs(MathF.Sin(t * 5f));
        var black = m.C(p.Color1); var white = m.C(0xF8F8F4); var leaf = m.C(p.Color2);
        m.Ball(0, 210, -60, 225, 170, 170, white);
        foreach (int s in Sides) m.Super(s * 140, 318, 40, 64, 38, 80, .5f, 1f, black);
        m.Limb(150, 250, 120, 300, -150, 60, 9, 7, leaf);
        for (int i = 0; i < 5; i++)
        {
            float k = i / 4f, x = 195 + k * 105, y = 130 - k * 280, z = 110 - k * 50;
            m.Slab(z, 6, leaf, x, y, x + 120, y - 66 + MathF.Sin(t * 2 + i) * 8, x + 28, y + 16);
        }
        m.Limb(-185, 110, 60, -170, 260, 120, 58, 50, black);
        m.Limb(180, 120, 60, 200, 200, 130, 56, 48, black);
        const float hx = 0, hy = -55, hz = 0, rx = 230, ry = 190, rz = 180;
        foreach (int s in Sides) m.Ball(s * 168, -205, -40, 66, 66, 40, black);
        m.Ball(hx, hy, hz, rx, ry, rz, m.C(0xFFFFFF));
        foreach (int s in Sides)
        {
            float ex = s * 88, ey = -75, ez = Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, ex, ey);
            m.Turn(ex, ey, s * -.45f, () => m.Ball(ex, ey, ez - 18, 46, 62, 26, black));
            m.Ball(ex, -80, ez + 2, 19, 19, 9, m.C(0xFFFFFF));
            m.Ball(s * 90, -80, ez + 9, 11, 11, 5, m.C(0x101010));
        }
        float nz = Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, 0, -6);
        m.Ball(0, -6, nz - 4, 26, 17, 14, black);
        float mz = Sculpt.FrontZ(hx, hy, hz, rx, ry, rz, 0, 44);
        m.Ball(0, 44 + chew * 6, mz - 8, 34, 9 + chew * 20, 12, m.C(0xB23A48));
    }

    /// <summary>ひよこ: まん丸の体・頭の毛・はばたく羽・三角のくちばし・細い足</summary>
    static void Chick(ChickHopParameter p, Sculpt m, float t)
    {
        float h = MathF.Abs(MathF.Sin(t * 5f));
        var body = m.C(p.Color1); var beak = m.C(p.Color2);
        m.Super(0, 280, 0, 150 - h * 50, 3, 110 - h * 30, 1f, 1f, m.C(0x000000, .2f));
        m.Move(0, -h * 90, () =>
        {
            foreach (int s in Sides)
            {
                m.Limb(s * 60, 220, 0, s * 60, 268, 10, 6, 5, beak);
                m.Limb(s * 60, 270, 10, s * 26, 278, 50, 5, 4, beak); m.Limb(s * 60, 270, 10, s * 96, 278, 50, 5, 4, beak); m.Limb(s * 60, 270, 10, s * 60, 278, 60, 5, 4, beak);
            }
            const float cx = 0, cy = 40, cz = 0, r = 200;
            m.Ball(cx, cy, cz, r, body);
            foreach (int i in new[] { -1, 0, 1 }) m.Pitch(i * 22, -150, 0, .2f, () => m.Ball(i * 22, -170, 0, 12, 34, 12, body));
            foreach (int s in Sides)
            {
                m.Turn(s * 185, 60, s * (-.2f - h * .6f), () => m.Ball(s * 205, 80, -10, 42, 76, 34, m.C(0xF7C92F)));
                float ex = s * 72, ey = -5, ez = Sculpt.FrontZ(cx, cy, cz, r, r, r, ex, ey);
                m.Ball(ex, ey, ez - 4, 22, 22, 12, m.C(0x20140E));
                m.Ball(s * 66, -13, ez + 6, 7, 7, 3, m.C(0xFFFFFF));
                m.Ball(s * 118, 45, Sculpt.FrontZ(cx, cy, cz, r, r, r, s * 118, 45) - 2, 26, 15, 5, m.C(0xFF9F9F, .7f));
            }
            float bz = Sculpt.FrontZ(cx, cy, cz, r, r, r, 0, 36);
            m.Limb(0, 30, bz - 22, 0, 44, bz + 40, 30, 2, beak);
        });
        m.Text(Str(p), 62, 255, -220 + MathF.Sin(t * 5f) * 12f, 40, 14, m.C(0xFFFFFF), 0, m.C(p.Color2), 4);
    }

    /// <summary>カエル: 大きな目玉が上に出た体、ふくらむのど、長い後ろ足と水かき</summary>
    static void Frog(FrogCroakParameter p, Sculpt m, float t)
    {
        float cr = MathF.Max(0f, MathF.Sin(t * 4f)), hop = MathF.Pow(MathF.Max(0f, MathF.Sin(t * 1.2f)), 20f) * 130f;
        var g = m.C(p.Color1);
        m.Move(0, -hop, () =>
        {
            foreach (int s in Sides)
            {
                m.Ball(s * 200, 225, -40, 105, 58, 110, g);
                m.Limb(s * 105, 250, 60, s * 120, 305, 90, 16, 12, g);
                m.Limb(s * 230, 275, 40, s * 290, 300, 90, 9, 7, g); m.Limb(s * 230, 275, 40, s * 270, 318, 110, 9, 7, g);
            }
            const float bx = 0, by = 120, bz = 0, rx = 225, ry = 175, rz = 170;
            m.Ball(bx, by, bz, rx, ry, rz, g);
            m.Ball(0, 175, Sculpt.FrontZ(bx, by, bz, rx, ry, rz, 0, 175) - 50, 145, 105, 60, m.C(p.Color2));
            m.Ball(0, 70 + cr * 42, Sculpt.FrontZ(bx, by, bz, rx, ry, rz, 0, 80) - 36 + cr * 14, 80 + cr * 40, 22 + cr * 50, 26 + cr * 30, m.C(0xFFF7D0, .94f));
            foreach (int s in Sides)
            {
                m.Ball(s * 110, -55, 20, 76, 76, 70, g);
                m.Ball(s * 110, -60, 50, 52, 52, 48, m.C(0xFFFFFF));
                m.Ball(s * 110, -58, 96, 30, 18, 6, m.C(0x101010));
                m.Ball(s * 118, -66, 102, 7, 7, 3, m.C(0xFFFFFF));
            }
            m.Tube(Sculpt.OnFront(bx, by, bz, rx, ry, rz, 1.5f, Sculpt.ArcPts(0, -40, 140, .5f, MathF.PI - .5f)), 4, m.C(0x25621E));
        });
        if (cr > .3f) m.Text(Str(p), 70, 300, -190, 60, 16, m.C(0xFFFFFF, cr), 0, m.C(0x25621E, cr), 4);
    }

    /// <summary>ハムスターの回し車: 奥行きのある回し車(横木が回る)と、中を走るハムスター</summary>
    static void Hamster(HamsterWheelParameter p, Sculpt m, float t)
    {
        var metal = m.C(0x8A97A3); var wheel = m.C(p.Color2);
        foreach (int s in Sides) m.Limb(0, 0, -90, s * 110, 390, -90, 12, 14, metal);
        m.Box(0, 398, -60, 380, 22, 180, m.C(0x6B7680), .3f);
        m.Limb(0, 0, -95, 0, 0, -60, 22, 22, m.C(0x6B7680));
        float rot = t * 3f;
        foreach (float z in new[] { -60f, 60f }) { m.Torus(0, 0, z, 340, 9, wheel); m.Torus(0, 0, z, 300, 5, wheel); }
        int keep = m.Seg; m.Seg = 10;
        for (int i = 0; i < 36; i++) { float a = rot + i * MathF.Tau / 36f, x = MathF.Cos(a) * 322, y = MathF.Sin(a) * 322; m.Limb(x, y, -60, x, y, 60, 4, 4, m.C(0xDDEBFA)); }
        for (int i = 0; i < 6; i++) { float a = rot + i * MathF.Tau / 6f; m.Limb(0, 0, -60, MathF.Cos(a) * 300, MathF.Sin(a) * 300, -60, 5, 5, m.C(p.Color2, .7f)); }
        m.Seg = keep;
        float b = MathF.Abs(MathF.Sin(t * 14f)) * 12f;
        m.Move(0, 230 - b, () =>
        {
            var fur = m.C(p.Color1);
            foreach (int s in Sides) { float leg = MathF.Sin(t * 14f + s) * 34f; m.Ball(s * 58 + leg, 80, s * 40, 24, 13, 18, m.C(0xFFB3B3)); }
            m.Ball(0, 0, 0, 135, 90, 88, fur);
            m.Ball(10, 30, 0, 100, 55, 80, m.C(0xFFF3E2));
            m.Ball(112, -46, 0, 66, 64, 60, fur);
            foreach (int s in Sides)
            {
                m.Ball(96, -102, s * 34, 24, 26, 10, m.C(0xFFB3B3));
                m.Ball(140, -56, s * 46, 11, 11, 8, m.C(0x101010));
                m.Ball(118, -14, s * 44, 20, 13, 10, m.C(0xFFFFFF));
            }
            m.Ball(176, -30, 0, 8, 7, 8, m.C(0xFF7F92));
        });
    }

    /// <summary>カラスが飛ぶ: 体・頭・くちばし・尾羽と、はばたく左右の翼(奥行き方向に広がる)</summary>
    static void Crows(CrowFlockParameter p, Sculpt m, float t)
    {
        int n = Math.Clamp(p.Count, 1, 24); float span = m.W + 520f;
        var c = m.C(p.Color1);
        int keep = m.Seg; m.Seg = 16;
        for (int i = 0; i < n; i++)
        {
            float x = Fr(t * .09f + R01(i) * .25f + i * .035f) * span - span / 2f, y = (R01(i + 20) - .5f) * 390f + MathF.Sin(t * 1.5f + i) * 18f, s = .6f + R01(i + 40) * .75f, z = (R01(i + 60) - .5f) * 300f;
            float flap = MathF.Sin(t * 8f + i * 1.3f) * .9f;
            m.Ball(x, y, z, 56 * s, 22 * s, 24 * s, c);
            m.Ball(x + 52 * s, y - 13 * s, z, 20 * s, c);
            m.Limb(x + 62 * s, y - 13 * s, z, x + 104 * s, y - 8 * s, z, 8 * s, 1 * s, c);
            m.Ball(x + 60 * s, y - 18 * s, z + 15 * s, 3.5f * s, m.C(0xFFFFFF));
            m.Ball(x + 60 * s, y - 18 * s, z - 15 * s, 3.5f * s, m.C(0xFFFFFF));
            if (i == 0) m.Text(Str(p), 48, x + 25, y - 92, z, 10, m.C(0xFFFFFF), 0, c, 4);
            m.Pitch(x - 50 * s, y, z, MathF.PI / 2f, () => m.Slab(y, 4 * s, c, x - 44 * s, z - 8 * s, x - 112 * s, z - 16 * s, x - 100 * s, z + 18 * s, x - 44 * s, z + 8 * s));
            foreach (int sd in Sides)
                m.Pitch(x, y - 8 * s, z, sd * (MathF.PI / 2f) + sd * flap * .9f, () =>
                    m.Slab(z + sd * 0f, 4 * s, c, x - 28 * s, y - 8 * s, x + 26 * s, y - 8 * s, x - 10 * s, y - 70 * s, x - 70 * s, y - 60 * s));
        }
        m.Seg = keep;
    }

    /// <summary>ミツバチ: 花(花びら・茎・葉)のまわりを飛ぶ、しま模様のハチ。羽ははばたく</summary>
    static void Bees(HoneyBeeParameter p, Sculpt m, float t)
    {
        var green = m.C(0x4A9A45);
        m.Limb(0, 110, -20, 0, 390, -20, 8, 8, green);
        m.Yaw(70, 300, -20, .5f, () => m.Ball(70, 300, -20, 72, 26, 18, green));
        for (int i = 0; i < 8; i++) { float a = i * MathF.Tau / 8f; m.Ball(MathF.Cos(a) * 82, 80 + MathF.Sin(a) * 82, -10, 50, 50, 16, m.C(p.Color2)); }
        m.Ball(0, 80, 6, 52, 52, 26, m.C(0xFFC23D));
        int n = Math.Clamp(p.Count, 1, 8);
        int keep = m.Seg; m.Seg = 18;
        for (int i = 0; i < n; i++)
        {
            float ph = t * 1.2f + i * MathF.Tau / n, x = MathF.Sin(ph) * 330, y = 60 + MathF.Sin(ph * 2f) * 130, z = MathF.Cos(ph) * 160f + 80f, dir = MathF.Cos(ph) >= 0f ? 1f : -1f, f = MathF.Abs(MathF.Sin(t * 35f + i));
            for (int k = 1; k <= 8; k++) { float q = ph - k * .09f; m.Ball(MathF.Sin(q) * 330, 60 + MathF.Sin(q * 2f) * 130, MathF.Cos(q) * 160f + 80f, 3.5f, m.C(0xFFFFFF, .6f * (1f - k / 9f))); }
            m.Ball(x, y, z, 66, 44, 44, m.C(p.Color1));
            for (int k = -1; k <= 1; k++) m.Ball(x + k * 26 * dir - dir * 6, y, z, 7, 46f - MathF.Abs(k) * 8f, 46f - MathF.Abs(k) * 8f, m.C(0x241A12));
            m.Limb(x - dir * 60, y, z, x - dir * 90, y, z, 8, 1, m.C(0x241A12));
            m.Ball(x + dir * 50, y - 8, z + 26, 8, m.C(0x101010));
            m.Ball(x + dir * 50, y - 8, z - 26, 8, m.C(0x101010));
            foreach (int sd in Sides)
                m.Pitch(x, y - 30, z, sd * (.5f + f * .7f), () => m.Ball(x - dir * 4, y - 70, z, 40, 46, 4, m.C(0xDFF4FF, .72f)));
        }
        m.Seg = keep;
    }

    // ================================================================ 乗り物
    /// <summary>走る車: 横から見た車体の厚み・窓・4つのタイヤ(回るホイール)・ライト、道路</summary>
    static void Car(DrivingCarParameter p, Sculpt m, float t)
    {
        const float wr = 62f, by = 150f, half = 120f;
        if (p.Background)
        {
            m.Box(0, 216, 0, m.W + 800, 8, 700, m.C(0x4A4E55), .05f);
            float off = -Fr(t * 2f) * 220f;
            for (int i = -7; i <= 8; i++) m.Box(i * 220 + off, 211, 200, 116, 3, 12, m.C(0xFFFFFF), .2f);
        }
        float b = MathF.Sin(t * 19f) * 3f;
        var body = m.C(p.Color1); var glass = m.C(0xBFE7FA);
        m.Move(0, b, () =>
        {
            m.Slab(0, half * 2, body, -335, 120, -335, 40, -220, 26, -125, -72, 85, -72, 180, 22, 320, 48, 348, 90, 348, 138, -335, 138);
            m.Slab(0, half * 2 + 4, glass, -188, 24, -108, -48, -20, -48, -20, 24);
            m.Slab(0, half * 2 + 4, glass, 8, -48, 72, -48, 138, 24, 8, 24);
            m.Slab(0, half * 2 - 30, glass, 86, -70, 98, -70, 178, 18, 160, 18);
            foreach (int s in Sides)
            {
                m.Limb(-6, -70, s * (half + 2), -6, 130, s * (half + 2), 2, 2, m.C(0x7A1C1A));
                m.Box(55, 58, s * (half + 3), 50, 10, 6, m.C(0x7A1C1A), .5f);
                m.Ball(340, 72, s * (half - 30), 12, 18, 16, m.C(0xFFF2A8));
                m.Ball(-333, 74, s * (half - 30), 6, 13, 12, m.C(0xFF4D4D));
            }
        });
        foreach (float x in new[] { -205f, 205f })
            foreach (int s in Sides)
            {
                m.Disc(x, by, s * (half - 6), wr, wr, 46, m.C(0x1D1E22), .45f);
                m.Disc(x, by, s * (half + 18), wr * .55f, wr * .55f, 6, m.C(0xC7CCD3), .3f);
                for (int i = 0; i < 5; i++) { float a = t * 14f + i * MathF.Tau / 5f; m.Limb(x, by, s * (half + 22), x + MathF.Cos(a) * wr * .5f, by + MathF.Sin(a) * wr * .5f, s * (half + 22), 3, 3, m.C(0x5A5F67)); }
            }
        for (int i = 0; i < 8; i++) { float k = Fr(t * 1.4f + R01(i)); m.Ball(-370 - k * 240, 130 - k * 30 + (R01(i + 8) - .5f) * 30, 40 + (R01(i + 3) - .5f) * 60, 10 + k * 26, m.C(0xCFCFCF, .34f * (1f - k))); }
    }

    /// <summary>電車が走る: 箱形の車両(窓・ドア・帯・屋根)、台車と車輪、2本のレールと枕木</summary>
    static void Train(PassingTrainParameter p, Sculpt m, float t)
    {
        int n = Math.Clamp(p.Count, 1, 10); const float carW = 460f, half = 130f;
        float dir = p.Variant == 2 ? -1f : 1f, total = n * carW, span = m.W + total + 200f, x0 = Fr(t * .13f) * span - m.W / 2f - total - 100f;
        foreach (int s in Sides) m.Box(0, 186, s * 75, m.W + 800, 10, 12, m.C(0x70757B), .2f);
        for (int i = -22; i <= 22; i++) m.Box(i * 58f, 200, 0, 34, 14, 230, m.C(0x5B4A3E), .2f);
        for (int c = 0; c < n; c++)
        {
            float x = (x0 + c * carW + carW / 2f) * dir, shake = MathF.Sin(t * 26f + c) * 1.8f;
            m.Box(x, 40 + shake, 0, carW - 12, 240, half * 2, m.C(p.Color2), .14f);
            m.Box(x, 112 + shake, 0, carW - 12, 34, half * 2 + 4, m.C(p.Color1), .06f);
            m.Box(x, 138 + shake, 0, carW - 12, 8, half * 2 + 4, m.C(0x8F9690), .06f);
            for (int w = -2; w <= 2; w++)
            {
                if (w is -2 or 2 or 0) m.Box(x + w * 90, 58 + shake, 0, 54, 120, half * 2 + 6, m.C(0x3E4A52), .1f);
                else m.Box(x + w * 90, 12 + shake, 0, 100, 66, half * 2 + 6, m.C(0xA8DAEF), .1f);
            }
            foreach (int s in Sides)
            {
                m.Box(x + s * 150, 166, 0, 110, 26, half * 2 - 40, m.C(0x2A2C31), .2f);
                foreach (int k in new[] { 180, 120 }) foreach (int zs in Sides) m.Disc(x + s * k, 176, zs * 75, 17, 17, 14, m.C(0x17181B), .3f);
            }
            m.Box(x, -86 + shake, 0, carW - 40, 12, half * 2 - 30, m.C(0x70757B), .3f);
            if (c == n - 1) { float nx = x + dir * (carW / 2f - 4f); foreach (int s in Sides) m.Ball(nx, 80 + shake, s * 70, 10, 15, 15, m.C(0xFFF2A8)); }
        }
    }

    /// <summary>飛行機雲: 空を斜めに昇る飛行機(胴体・左右の主翼・尾翼)と、後ろに伸びる白い雲</summary>
    static void Plane(ContrailPlaneParameter p, Sculpt m, float t)
    {
        if (p.Background)
        {
            m.Box(0, 0, -700, m.W * 1.7f, m.H * 1.7f, 4, m.C(p.Color2), .02f);
            m.Box(0, m.H * .45f, -690, m.W * 1.7f, m.H * .5f, 4, m.C(0xBFE8FF, .55f), .02f);
        }
        float k = Fr(t * .09f), x0 = -m.W / 2f - 150f, x = x0 + k * (m.W + 300f), y0 = 300f, y = y0 - k * 620f;
        int keep = m.Seg; m.Seg = 12;
        for (int i = 0; i < 90; i += 2)
        {
            float q = i / 90f, tx = x - q * q * 1100f - 75f; if (tx < x0) break;
            float ty = y0 + (tx - x0) * (y - y0) / MathF.Max(1f, x - x0) + 4f, r = 7f + q * 24f + MathF.Sin(i * 1.9f) * 3f;
            foreach (int s in Sides) m.Ball(tx, ty, s * 40, r * 1.2f, m.C(p.Color1, .7f * (1f - q)));
        }
        m.Seg = keep;
        m.Turn(x, y, -.5f, () =>
        {
            var white = m.C(0xF3F6FA); var wing = m.C(0x516F8E);
            m.Limb(x - 72, y, 0, x + 66, y, 0, 11, 11, white);
            m.Limb(x + 66, y, 0, x + 80, y + 2, 0, 11, 3, white);
            m.Pitch(x, y, 0, MathF.PI / 2f, () => m.Slab(y + 2, 5, wing, x - 12, -8, x + 34, -8, x - 45, -110, x - 66, -110, x - 66, 110, x - 45, 110, x + 34, 8, x - 12, 8));
            m.Slab(0, 5, wing, x - 72, y - 4, x - 52, y - 4, x - 82, y - 40, x - 92, y - 40);
            m.Pitch(x, y, 0, MathF.PI / 2f, () => m.Slab(y, 4, wing, x - 70, -4, x - 56, -4, x - 80, -36, x - 90, -36, x - 90, 36, x - 80, 36, x - 56, 4, x - 70, 4));
        });
    }

    /// <summary>ヘリコプター: 丸い胴体と窓・細い尾・尾翼と後ろの回転翼・上の大きな回転翼(奥行き方向にも回る)・脚</summary>
    static void Heli(HelicopterParameter p, Sculpt m, float t)
    {
        float hov = MathF.Sin(t * 1.6f) * 18f, tilt = MathF.Sin(t * .9f) * .05f;
        var body = m.C(p.Color1); var dark = m.C(0x25282D);
        m.Move(0, hov, () => m.Turn(0, 0, tilt, () =>
        {
            m.Limb(-120, 20, 0, -470, -22, 0, 50, 16, body);
            m.Slab(0, 14, body, -440, -38, -495, -130, -445, -130, -395, -38);
            float a = t * 28f;
            m.Limb(-470 + MathF.Cos(a) * 70, -20 + MathF.Sin(a) * 70, 22, -470 - MathF.Cos(a) * 70, -20 - MathF.Sin(a) * 70, 22, 5, 5, m.C(0x25282D, .85f));
            m.Limb(-470, -20, 0, -470, -20, 24, 10, 10, dark);
            m.Ball(50, 25, 0, 235, 128, 125, body);
            m.Ball(140, -5, 0, 140, 92, 108, m.C(p.Color2, .92f));
            m.Cyl(0, -123, 0, 16, 46, dark);
            float ra = t * 34f;
            for (int k = 0; k < 2; k++) m.Yaw(0, -152, 0, ra + k * MathF.PI / 2f, () => m.Box(0, -152, 0, 820, 6, 34, m.C(0x7B848F), .3f));
            m.Ball(0, -152, 0, 28, 15, 28, dark);
            foreach (int s in Sides)
            {
                m.Limb(s * 80 + 40, 140, 80, s * 96 + 40, 192, 95, 6, 6, dark);
                m.Limb(s * 80 + 40, 140, -80, s * 96 + 40, 192, -95, 6, 6, dark);
                m.Limb(-140, 196, s * 95, 200, 196, s * 95, 7, 7, dark);
            }
        }));
    }

    /// <summary>ロケット発射: 丸い胴体・赤い先端・4枚の羽・窓・噴射口、炎と煙</summary>
    static void Rocket(RocketLaunchParameter p, Sculpt m, float t)
    {
        float q = MathF.Max(0f, t - .55f), rise = q * q * 110f, shake = t < .55f ? MathF.Sin(t * 60f) * 5f * (t / .55f) : MathF.Sin(t * 45f) * 2f, by = 260f - rise;
        int keep = m.Seg; m.Seg = 14;
        for (int i = 0; i < 46; i++)
        {
            float k = Fr(t * .9f + R01(i)), side = (R01(i + 7) - .5f) * 2f, r = (28f + k * 86f) * Cl(t * 2f), dz = (R01(i + 11) - .5f) * 2f;
            m.Ball(side * (40f + k * 420f), 410f - k * k * 60f + R01(i + 9) * 20f, dz * (40f + k * 300f), r, m.C(0xE3E5E8, .46f * (1f - k)));
        }
        m.Seg = keep;
        if (t > .2f)
        {
            float fl = 1f + MathF.Sin(t * 41f) * .14f, ls = Cl((t - .2f) * 3f);
            m.Lathe(shake, 0, m.C(p.Color3, .9f), 0, by + 92, 58, by + 92, 34, by + 92 + 200 * fl * ls, 0, by + 92 + 310 * fl * ls);
            m.Lathe(shake, 0, m.C(0xFFF2A8), 0, by + 90, 32, by + 90, 18, by + 92 + 110 * fl * ls, 0, by + 92 + 170 * fl * ls);
        }
        m.Move(shake, -rise, () =>
        {
            m.Lathe(0, 0, m.C(p.Color1), 0, -255, 22, -238, 46, -175, 70, -105, 82, -55, 82, 345, 0, 345);
            m.Lathe(0, 0, m.C(p.Color2), 0, -258, 23, -240, 48, -176, 67, -113, 0, -113);
            m.Lathe(0, 0, m.C(0x37474F), 0, 340, 46, 340, 60, 376, 0, 376);
            m.Cyl(0, 250, 0, 84, 14, m.C(p.Color2), .2f);
            for (int k = 0; k < 4; k++) m.Yaw(0, 0, 0, k * MathF.PI / 2f + MathF.PI / 4f, () => m.Slab(0, 12, m.C(p.Color2), 70, 170, 150, 355, 150, 365, 70, 335));
            m.Disc(0, 20, 78, 40, 40, 12, m.C(0x37474F), .3f);
            m.Disc(0, 20, 84, 28, 28, 6, m.C(0x8ED8F8), .3f);
        });
    }

    /// <summary>帆船: 厚みのある船体・2本のマスト・ふくらむ帆・旗。波の上でゆれる</summary>
    static void Ship(SailingShipParameter p, Sculpt m, float t)
    {
        var sea = m.C(p.Color3, .8f);
        float roll = MathF.Sin(t * 1.3f) * .07f, by = MathF.Sin(t * 1.6f) * 12f;
        m.Move(0, by, () => m.Turn(0, 200, roll, () =>
        {
            var hull = m.C(p.Color1);
            m.Slab(0, 170, hull, -320, 110, 340, 80, 245, 245, -230, 245);
            m.Super(10, 100, 0, 330, 14, 90, .3f, .5f, Sculpt.Dark(hull, .8f));
            foreach (int s in Sides) m.Limb(-300, 145, s * 86, 305, 122, s * 86, 3, 3, m.C(0xD9A866));
            foreach (int x in new[] { -130, 110 }) m.Limb(x, 115, 0, x, -310, 0, 9, 7, m.C(0x4E2F16));
            m.Limb(-310, 92, 0, 350, 62, 0, 2.5f, 2.5f, m.C(0x4E2F16));
            foreach (int x in new[] { -130, 110 })
                for (int r = 0; r < 2; r++)
                {
                    float top = -285 + r * 180, h = 155, bw = 118 + r * 22, bulge = 28 + MathF.Sin(t * 1.8f + r + x) * 8f;
                    m.Super(x, top + h / 2f, 18 + bulge * .6f, bw, h / 2f, 6 + bulge * .5f, .35f, .25f, m.C(p.Color2));
                    m.Limb(x - bw, top, 0, x + bw, top, 0, 4, 4, m.C(0x4E2F16));
                }
            m.Slab(0, 4, m.C(0xD7263D), 110, -345, 188, -322 + MathF.Sin(t * 5f) * 7f, 110, -298);
        }));
        m.Super(0, 236, 0, m.W, 16, 420, .2f, .15f, sea);
        for (int i = 0; i < 6; i++) { float x = (i - 2.5f) * 260f + MathF.Sin(t + i) * 30f; m.Ball(x, 222, 200 - (i % 2) * 380, 90, 14, 40, m.C(0xFFFFFF, .35f)); }
    }

    /// <summary>自転車: 2つの車輪(回るスポーク)・フレームの管・サドル・ハンドル・回るペダル</summary>
    static void Bicycle(BicycleRideParameter p, Sculpt m, float t)
    {
        const float wy = 130, R = 165; float ang = t * 4f;
        var frame = m.C(p.Color1); var gray = m.C(0x50555C); var tire = m.C(0x23252A);
        int keep = m.Seg; m.Seg = 14;
        foreach (float x in new[] { -270f, 270f })
        {
            m.Torus(x, wy, 0, R - 2, 9, tire);
            m.Torus(x, wy, 0, R - 14, 3, m.C(0xB9BEC6));
            for (int i = 0; i < 16; i++) { float a = ang + i * MathF.Tau / 16f; m.Limb(x, wy, (i % 2 == 0 ? 10 : -10), x + MathF.Cos(a) * (R - 12), wy + MathF.Sin(a) * (R - 12), 0, 1.2f, 1.2f, m.C(0x9CA3AC)); }
            m.Limb(x, wy, -16, x, wy, 16, 9, 9, gray);
        }
        Vector3 c = new(-25, wy, 0), st = new(-105, -95, 0), hd = new(180, -130, 0);
        m.Torus(c.X, c.Y, 14, 44, 4, gray);
        m.Limb(-270, wy, 10, c.X, c.Y, 10, 3, 3, gray);
        m.Limb(new(-270, wy, 0), st, 6, 6, frame); m.Limb(st, c, 8, 8, frame); m.Limb(c, hd, 8, 8, frame); m.Limb(st, hd, 8, 8, frame); m.Limb(hd, new(270, wy, 0), 6, 6, frame);
        m.Limb(hd.X, hd.Y, 0, 165, -205, 0, 5, 5, gray);
        m.Limb(165, -207, -70, 165, -207, 70, 6, 6, tire);
        foreach (int s in Sides) m.Limb(165, -207, s * 70, 225, -218, s * 80, 8, 8, tire);
        m.Limb(st.X, st.Y, 0, -122, -150, 0, 5, 5, gray);
        m.Super(-135, -166, 0, 75, 14, 34, .5f, .6f, tire);
        foreach (int s in Sides)
        {
            float px = c.X + MathF.Cos(ang) * 64 * s, py = c.Y + MathF.Sin(ang) * 64 * s, pz = s * 30;
            m.Limb(c.X, c.Y, pz * .6f, px, py, pz, 4, 4, m.C(0xB9BEC6));
            m.Box(px, py, pz + s * 22, 62, 12, 40, tire, .3f);
        }
        m.Seg = keep;
    }

    /// <summary>熱気球: 色分けされた丸い気球(縦のしま)・口・ロープ・かご。昇っていく</summary>
    static void Balloons(HotAirBalloonParameter p, Sculpt m, float t)
    {
        if (p.Background)
        {
            m.Box(0, -m.H * .25f, -900, m.W * 2f, m.H * 1.3f, 4, m.C(0x69B9EC), .02f);
            m.Box(0, m.H * .55f, -890, m.W * 2f, m.H * .9f, 4, m.C(0xD4F0FF), .02f);
        }
        int n = Math.Clamp(p.Count, 1, 12);
        for (int b = n - 1; b >= 0; b--)
        {
            float d = .5f + (b + 1f) / n * .7f, S = 160f * d, span = m.H + S * 5f, y = m.H / 2f + S * 2.2f - Fr(t * .035f * d + R01(b + 3)) * span;
            float x = (n == 1 ? 0 : (b + .5f) / n - .5f) * m.W * .9f + MathF.Sin(t * .5f + b) * 22f, z = (R01(b + 30) - .5f) * 400f;
            var c1 = m.C(b % 2 == 0 ? p.Color1 : p.Color2); var c2 = m.C(b % 2 == 0 ? p.Color2 : p.Color1);
            var prof = new List<Vector2>();
            for (int i = 0; i <= 16; i++) { float k = -MathF.PI / 2f + i / 16f * MathF.PI * .82f; prof.Add(new(MathF.Cos(k) * S, y + MathF.Sin(k) * S * 1.08f)); }
            prof.Add(new(S * .42f, y + S * .92f));
            for (int g = 0; g < 12; g++) m.Lathe(x, z, prof, g % 2 == 0 ? c1 : c2, g * MathF.Tau / 12f, (g + 1) * MathF.Tau / 12f);
            m.Lathe(x, z, c1, S * .42f, y + S * .92f, S * .15f, y + S * 1.34f);
            foreach (int s in Sides) foreach (int zs in Sides) m.Limb(x + s * S * .15f, y + S * 1.34f, z + zs * S * .1f, x + s * S * .13f, y + S * 1.62f, z + zs * S * .1f, 1.5f, 1.5f, m.C(0x5A3A22));
            m.Box(x, y + S * 1.74f, z, S * .34f, S * .28f, S * .3f, m.C(0x8A5A32), .25f);
        }
    }

    /// <summary>回転灯(パトランプ): 台・半透明のドーム・中で回る光と、左右に回る光の筋</summary>
    static void Beacon(RotatingBeaconParameter p, Sculpt m, float t)
    {
        float a = t * 6f;
        m.Box(0, 170, 0, 300, 72, 220, m.C(0x25282D), .25f);
        m.Box(0, 115, 0, 250, 44, 190, m.C(0x3A3E45), .25f);
        var light = new Vector3(MathF.Sin(a), 0, MathF.Cos(a));
        m.Ball(light.X * 80, -40, light.Z * 80, 56, 100, 56, m.C(0xFFFFFF, .9f));
        m.Ball(-light.X * 80, -40, -light.Z * 80, 40, 80, 40, m.C(0xFFFFFF, .5f));
        var dome = Dome(145f, 145f, -102f); dome.Add(new(145, 95)); dome.Add(new(0, 95));
        m.Lathe(0, 0, dome, m.C(p.Color1, .8f));
        int keep = m.Seg; m.Seg = 16;
        foreach (int s in Sides)
        {
            var dir = light * s;
            m.Limb(dir.X * 140, -60, dir.Z * 140, dir.X * 700, -60, dir.Z * 700, 24, 130, m.C(p.Color1, .07f + .08f * MathF.Max(0f, dir.Z)));
        }
        m.Seg = keep;
    }

    /// <summary>潜水艦: カプセル形の船体・司令塔・潜望鏡・左右の丸窓・回るスクリュー・尾びれ、泡</summary>
    static void Submarine(SubmarineParameter p, Sculpt m, float t)
    {
        float by = MathF.Sin(t * 1.2f) * 20f;
        int keep = m.Seg; m.Seg = 12;
        for (int i = 0; i < 22; i++)
        {
            float k = Fr(t * .55f + R01(i)), x = -430f - k * 120f + MathF.Sin(k * 7f + i) * 22f, y = by + 30f - k * 330f + (R01(i + 5) - .5f) * 60f, r = 6f + R01(i + 9) * 15f;
            m.Ball(x, y, (R01(i + 13) - .5f) * 160f, r, m.C(0xFFFFFF, .45f * (1f - k)));
        }
        m.Seg = keep;
        m.Move(0, by, () =>
        {
            var hull = m.C(p.Color1); var dark = m.C(0x50555C);
            m.Slab(0, 10, m.C(0xD99A1C), -345, -55, -430, -100, -430, 100, -345, 55);
            m.Pitch(-380, 0, 0, MathF.PI / 2f, () => m.Slab(0, 10, m.C(0xD99A1C), -345, -55, -430, -100, -430, 100, -345, 55));
            float pa = t * 16f;
            for (int k = 0; k < 3; k++) { float a = pa + k * MathF.Tau / 3f; m.Limb(-452, 0, 0, -452, MathF.Cos(a) * 70f, MathF.Sin(a) * 70f, 10, 5, dark); }
            m.Limb(-445, 0, 0, -395, 0, 0, 7, 7, dark);
            m.Limb(-268, 0, 0, 268, 0, 0, 112, 112, hull);
            m.Box(45, -135, 0, 210, 100, 120, hull, .35f);
            m.Tube([new(90, -180, 0), new(90, -245, 0), new(165, -245, 0)], 7, dark);
            m.Box(180, -245, 0, 34, 25, 20, dark, .3f);
            foreach (int s in Sides)
                for (int i = -1; i <= 2; i++)
                {
                    float x = i * 140 - 30;
                    m.Pitch(x, 0, 0, s * .0f, () => { });
                    m.Disc(x, 0, s * 108, 45, 45, 14, m.C(0xB9BEC6), .3f);
                    m.Disc(x, 0, s * 116, 34, 34, 6, m.C(p.Color2), .3f);
                }
            foreach (int s in Sides) m.Limb(-325, 72, s * 60, 320, 72, s * 60, 3, 3, m.C(0xD99A1C));
        });
    }
}
