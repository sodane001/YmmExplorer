using System.Numerics;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 211_QThings5.cs の図形(学校・スポーツ・家電 30種類)
//  座標は2Dの絵と同じ(x=右、y=下、z=手前)
// =====================================================================
internal static partial class Models3D
{
    static partial void RegisterQ5a(Dictionary<Type, QuickModel> d)
    {
        d[typeof(PencilDrawParameter)] = (p, m, t, v, v2) => Pencil((PencilDrawParameter)p, m, t);
        d[typeof(EraserWipeParameter)] = (p, m, t, v, v2) => Eraser((EraserWipeParameter)p, m, t);
        d[typeof(RulerProtractorParameter)] = (p, m, t, v, v2) => Protractor((RulerProtractorParameter)p, m, t, v);
        d[typeof(AbacusParameter)] = (p, m, t, v, v2) => Abacus((AbacusParameter)p, m, t);
        d[typeof(SchoolBagParameter)] = (p, m, t, v, v2) => SchoolBag((SchoolBagParameter)p, m, t, v);
        d[typeof(MicroscopeViewParameter)] = (p, m, t, v, v2) => Microscope((MicroscopeViewParameter)p, m, t);
        d[typeof(FlaskExperimentParameter)] = (p, m, t, v, v2) => Flask((FlaskExperimentParameter)p, m, t);
        d[typeof(TestPaperParameter)] = (p, m, t, v, v2) => TestPaper((TestPaperParameter)p, m, t);
        d[typeof(ClassTimetableParameter)] = (p, m, t, v, v2) => Timetable((ClassTimetableParameter)p, m, t, v);
        d[typeof(DiplomaScrollParameter)] = (p, m, t, v, v2) => Diploma((DiplomaScrollParameter)p, m, t);
        d[typeof(SoccerGoalParameter)] = (p, m, t, v, v2) => SoccerGoal((SoccerGoalParameter)p, m, t);
        d[typeof(BaseballHomerParameter)] = (p, m, t, v, v2) => Baseball((BaseballHomerParameter)p, m, t);
        d[typeof(BasketShotParameter)] = (p, m, t, v, v2) => Basket((BasketShotParameter)p, m, t);
        d[typeof(TennisRallyParameter)] = (p, m, t, v, v2) => Tennis((TennisRallyParameter)p, m, t);
        d[typeof(BowlingStrikeParameter)] = (p, m, t, v, v2) => Bowling((BowlingStrikeParameter)p, m, t);
        d[typeof(PingPongRallyParameter)] = (p, m, t, v, v2) => PingPong((PingPongRallyParameter)p, m, t);
        d[typeof(GolfCupInParameter)] = (p, m, t, v, v2) => Golf((GolfCupInParameter)p, m, t);
        d[typeof(FinishTapeParameter)] = (p, m, t, v, v2) => FinishTape((FinishTapeParameter)p, m, t);
        d[typeof(DartsHitParameter)] = (p, m, t, v, v2) => Darts((DartsHitParameter)p, m, t);
        d[typeof(SumoRingParameter)] = (p, m, t, v, v2) => Sumo((SumoRingParameter)p, m, t);
        d[typeof(ElectricFanParameter)] = (p, m, t, v, v2) => Fan((ElectricFanParameter)p, m, t);
        d[typeof(WashingMachineParameter)] = (p, m, t, v, v2) => Washer((WashingMachineParameter)p, m, t, m.Now);
        d[typeof(MicrowaveChinParameter)] = (p, m, t, v, v2) => Microwave((MicrowaveChinParameter)p, m, t, v);
        d[typeof(RiceCookerParameter)] = (p, m, t, v, v2) => RiceCooker((RiceCookerParameter)p, m, t, v);
        d[typeof(VacuumCleanerParameter)] = (p, m, t, v, v2) => Vacuum((VacuumCleanerParameter)p, m, t);
        d[typeof(BoomboxParameter)] = (p, m, t, v, v2) => Boombox((BoomboxParameter)p, m, t);
        d[typeof(FlashlightBeamParameter)] = (p, m, t, v, v2) => Flashlight((FlashlightBeamParameter)p, m, t);
        d[typeof(FaucetWaterParameter)] = (p, m, t, v, v2) => Faucet((FaucetWaterParameter)p, m, t);
        d[typeof(HairDryerParameter)] = (p, m, t, v, v2) => Dryer((HairDryerParameter)p, m, t);
        d[typeof(RotaryPhoneParameter)] = (p, m, t, v, v2) => Phone((RotaryPhoneParameter)p, m, t);
    }

    static string Str(QuickShapeParameter p) => TextKit.Lines(p.Text);

    /// <summary>扇形(パイ)の点の並び</summary>
    static float[] PiePts(float x, float y, float r, float a0, float a1, int steps = 24)
    {
        var l = new List<float> { x, y };
        for (int i = 0; i <= steps; i++) { float a = a0 + (a1 - a0) * i / steps; l.Add(x + MathF.Cos(a) * r); l.Add(y + MathF.Sin(a) * r); }
        return [.. l];
    }

    /// <summary>星形の点の並び</summary>
    static float[] StarPts(float x, float y, float r, int n, float inner, float rot)
    {
        var l = new List<float>();
        for (int i = 0; i < n * 2; i++) { float a = rot - MathF.PI / 2f + i * MathF.PI / n, rr = i % 2 == 0 ? r : r * inner; l.Add(x + MathF.Cos(a) * rr); l.Add(y + MathF.Sin(a) * rr); }
        return [.. l];
    }

    // ================================================================ 学校
    /// <summary>鉛筆が線を引く: 丸い鉛筆(芯・削った木・軸・金具・消しゴム)と、紙の上にのびる線</summary>
    static void Pencil(PencilDrawParameter p, Sculpt m, float t)
    {
        var q = new List<Vector3>(); float k = Fr(t * .22f) * 1.15f, end = MathF.Min(1f, k);
        for (int i = 0; i <= 120 * end; i++) { float e = i / 120f; q.Add(new(-520 + e * 1040, 120 + MathF.Sin(e * 14f) * 90f * (1f - e * .3f), 0)); }
        if (q.Count >= 2) { int keep = m.Seg; m.Seg = 10; m.Tube(q, 3.5f, m.C(p.Color2)); m.Seg = keep; }
        var h = q.Count > 0 ? q[^1] : new Vector3(-520, 120, 0);
        m.Turn(h.X, h.Y, .72f, () => m.Pitch(h.X, h.Y, 0, -.35f, () =>
        {
            m.Lathe(h.X, 0, m.C(0x30343B), 0, h.Y, 13, h.Y - 38);
            m.Lathe(h.X, 0, m.C(0xF2D2A9), 13, h.Y - 38, 36, h.Y - 98);
            int keep = m.Seg; m.Seg = 6;
            m.Lathe(h.X, 0, m.C(p.Color1), 36, h.Y - 98, 36, h.Y - 422);
            m.Seg = keep;
            m.Cyl(h.X, h.Y - 438, 0, 38, 36, m.C(0xB9BEC6), .2f);
            m.Cyl(h.X, h.Y - 484, 0, 36, 62, m.C(0xFF8FA8), .55f);
        }));
    }

    /// <summary>消しゴムで消す: ノートの紙と罫線、立体の文字、カバー付きの消しゴム、消しかす</summary>
    static void Eraser(EraserWipeParameter p, Sculpt m, float t)
    {
        var paper = m.C(0xFFFDF5);
        m.Box(0, 0, -20, 1100, 460, 16, paper, .05f);
        for (int i = -2; i <= 2; i++) m.Limb(-540, i * 76, -11, 540, i * 76, -11, 1.5f, 1.5f, m.C(0xA8C8E8));
        float k = Cl(t * .35f), x = -470 + k * 940 + MathF.Sin(t * 22f) * 58f * (k < 1f ? 1f : 0f);
        m.Text(Str(p), 150, 0, 0, -6, 8, m.C(p.Color3), 980);
        if (k > 0f) m.Box(-520 + (x + 540) / 2f, 0, -5, MathF.Max(0f, x + 560), 280, 12, paper, .02f);
        for (int i = 0; i < 18; i++) { float e = Fr(t * .9f + R01(i)), dx = (R01(i + 9) - .5f) * 90f; m.Box(x + dx - e * 25f, 120 + e * 105, 10 + e * 40, 22, 6, 8, m.C(0x8C8C92, 1f - e), .4f); }
        m.Turn(x, -25, -.14f, () =>
        {
            m.Box(x, -25, 40, 190, 300, 110, m.C(p.Color1), .25f);
            m.Box(x, 10, 40, 200, 230, 118, m.C(p.Color2), .12f);
            m.Box(x, -60, 40, 202, 22, 120, m.C(0xFFFFFF, .85f), .1f);
        });
    }

    /// <summary>定規と分度器: 半円の透明な分度器(目盛り・数字)と定規、角度の線と文字</summary>
    static void Protractor(RulerProtractorParameter p, Sculpt m, float t, float v)
    {
        const float R = 370, y = 210;
        m.Slab(0, 14, m.C(p.Color1, .62f), PiePts(0, y, R, MathF.PI, MathF.Tau, 40));
        m.Tube([.. Sculpt.ArcPts(0, y, R, MathF.PI, MathF.Tau, 40).Select(q => new Vector3(q, 8))], 3, m.C(0x2F6FBB));
        m.Tube([.. Sculpt.ArcPts(0, y, R * .55f, MathF.PI, MathF.Tau, 30).Select(q => new Vector3(q, 8))], 2, m.C(0x2F6FBB, .6f));
        for (int d = 0; d <= 180; d += 10)
        {
            float a = MathF.PI + d * MathF.PI / 180f, l = d % 30 == 0 ? 38 : 25;
            m.Limb(MathF.Cos(a) * R, y + MathF.Sin(a) * R, 8, MathF.Cos(a) * (R - l), y + MathF.Sin(a) * (R - l), 8, 1.8f, 1.8f, m.C(0x1F4F87));
            if (d % 30 == 0) m.Text(d.ToString(), 26, MathF.Cos(a) * (R - 68), y + MathF.Sin(a) * (R - 68), 8, 3, m.C(0x1F4F87));
        }
        m.Box(0, y + 58, 0, 900, 92, 12, m.C(0xFFF3C4, .86f), .1f);
        for (int i = -21; i <= 21; i++) m.Box(i * 20, y + (i % 5 == 0 ? 33 : 23), 7, 2.5f, i % 5 == 0 ? 42 : 22, 3, m.C(0x4A3A1A), .2f);
        float deg = v * 180f * Cl(t * .7f), an = MathF.PI + (180f - deg) * MathF.PI / 180f;
        var red = m.C(p.Color2);
        m.Limb(-R - 30, y, 16, R + 30, y, 16, 4, 4, red);
        m.Limb(0, y, 16, MathF.Cos(an) * (R + 45), y + MathF.Sin(an) * (R + 45), 16, 4.5f, 4.5f, red);
        m.Tube([.. Sculpt.ArcPts(0, y, 96, an, MathF.Tau, 16).Select(q => new Vector3(q, 16))], 3.5f, red);
        m.Ball(0, y, 16, 10, red);
        m.Text($"{deg:0}°", 66, 0, y - 150, 10, 14, red, 0, m.C(0xFFFFFF), 5);
    }

    /// <summary>そろばん: 木の枠と背板、はり、軸、そろばん玉(そろばん形の回転体)が数に合わせて動く</summary>
    static void Abacus(AbacusParameter p, Sculpt m, float t)
    {
        var dg = new string(Str(p).Where(char.IsDigit).ToArray()); const int n = 9;
        var frame = m.C(p.Color1);
        m.Box(0, -265, 0, 1180, 30, 70, frame, .2f); m.Box(0, 265, 0, 1180, 30, 70, frame, .2f);
        foreach (int s in Sides) m.Box(s * 575, 0, 0, 30, 560, 70, frame, .2f);
        m.Box(0, 0, -28, 1120, 500, 8, m.C(0xE9D7B5), .05f);
        m.Box(0, -112, 0, 1120, 16, 60, frame, .2f);
        for (int c = 0; c < n; c++)
        {
            float x = (c - (n - 1) / 2f) * 120;
            m.Limb(x, -250, 0, x, 250, 0, 3.5f, 3.5f, m.C(0x7A5A35));
            int idx = c - (n - dg.Length), d = idx >= 0 && idx < dg.Length ? dg[idx] - '0' : 0;
            float mm = Cl((t - .15f - MathF.Max(0, idx) * .16f) * 5f); mm = mm * mm * (3f - 2f * mm);
            void Bead(float y) => m.Lathe(x, 0, m.C(p.Color2), 0, y - 26, 12, y - 24, 54, y, 12, y + 24, 0, y + 26);
            Bead(d >= 5 ? -210 + 66 * mm : -210);
            int low = d % 5;
            for (int b = 0; b < 4; b++) Bead(68 + b * 52 - 66 * (b < low ? mm : 0f));
        }
    }

    /// <summary>ランドセル: 箱形の本体、中の教科書、上の持ち手、開け閉めするかぶせ(ふた)と金具</summary>
    static void SchoolBag(SchoolBagParameter p, Sculpt m, float t, float v)
    {
        var body = m.C(p.Color1); var dark = Sculpt.Dark(body, .65f);
        float op = v * (.65f + .35f * MathF.Sin(t * 1.3f));
        m.Box(0, 40, -20, 520, 590, 250, dark, .3f);
        for (int i = 0; i < 4; i++) m.Box(-110 + i * 72, -200, -10, 56, 150, 200, m.C(i % 2 == 0 ? 0x4FA8FF : 0x66BB6A), .1f);
        m.Pitch(0, -255, -20, MathF.PI / 2f, () => m.Torus(0, -255, -20, 96, 13, dark));
        m.Pitch(0, -250, 110, op * 1.4f, () =>
        {
            m.Box(0, 5, 118, 540, 510, 26, body, .35f);
            m.Box(0, -250, 30, 540, 26, 200, body, .35f);
            m.Box(0, 195, 136, 120, 72, 14, m.C(p.Color2), .3f);
            m.Box(0, 195, 144, 52, 22, 6, dark, .4f);
        });
    }

    /// <summary>顕微鏡: 台・アーム・ステージ・鏡筒(接眼・対物レンズ)・つまみと、のぞいた丸い視野の中の微生物</summary>
    static void Microscope(MicroscopeViewParameter p, Sculpt m, float t)
    {
        var o = new Vector2(-340, 60); var body = m.C(p.Color1); var light = m.C(0x90A4AE);
        m.Box(o.X, 300, 0, 360, 44, 240, body, .3f);
        m.Tube([.. Sculpt.ArcPts(o.X - 20, 90, 180, -1.15f, 1.3f, 16).Select(q => new Vector3(q, -40))], 20, body);
        m.Box(o.X - 25, 175, 0, 250, 22, 200, light, .2f);
        m.Turn(o.X, -80, .24f, () =>
        {
            m.Cyl(o.X, -120, 0, 43, 290, body);
            m.Cyl(o.X, -285, 0, 28, 72, m.C(0x263238));
            m.Cyl(o.X, 45, 0, 29, 66, light);
        });
        m.Disc(o.X + 148, 80, -30, 30, 30, 50, light);
        var c = new Vector2(330, -20); const float R = 300;
        m.Torus(c.X, c.Y, 0, R + 14, 16, m.C(0x263238));
        m.Disc(c.X, c.Y, -6, R, R, 10, m.C(0xEAF7D8));
        int keep = m.Seg; m.Seg = 14;
        for (int i = 0; i < Math.Clamp(p.Count, 1, 60); i++)
        {
            float a = R01(i) * MathF.Tau + t * (.08f + R01(i + 3) * .25f), r = MathF.Sqrt(R01(i + 7)) * (R - 48), x = c.X + MathF.Cos(a) * r, y = c.Y + MathF.Sin(a) * r, s = 12f + R01(i + 11) * 20f;
            m.Turn(x, y, a + MathF.Sin(t * 3f + i) * .4f, () =>
            {
                m.Ball(x, y, 6, s * 1.5f, s * .75f, s * .6f, m.C(p.Color2, .78f));
                m.Ball(x + s * .3f, y, 10, s * .3f, m.C(0x2E7D32));
                m.Tube([new(x - s * 1.4f, y, 6), new(x - s * 2f, y + MathF.Sin(t * 9f + i) * 6f, 6), new(x - s * 2.6f, y, 6)], 1.5f, m.C(p.Color2));
            });
        }
        m.Seg = keep;
    }

    /// <summary>フラスコの実験: 三角フラスコ(透明なガラス)・中の液体(色が変わる)・泡・上に出る湯気</summary>
    static void Flask(FlaskExperimentParameter p, Sculpt m, float t)
    {
        float k = Cl((t - .6f) * .55f); var c1 = m.C(p.Color1); var c2 = m.C(p.Color2); var liq = Vector4.Lerp(c1, c2, k);
        m.Lathe(0, 0, new Vector4(liq.X, liq.Y, liq.Z, .9f * m.Al), 0, 28, 106, 28, 245, 292, 220, 326, 0, 326);
        int keep = m.Seg; m.Seg = 12;
        for (int i = 0; i < Math.Clamp(p.Count, 0, 60); i++)
        {
            float e = Fr(t * (.5f + R01(i) * .5f) + R01(i + 5)), hw = 80f + (1f - e) * 110f, x = (R01(i + 8) - .5f) * hw * 1.6f + MathF.Sin(e * 9f + i) * 8f, z = (R01(i + 2) - .5f) * hw * 1.4f;
            m.Ball(x, 310f - e * 278f, z, 5f + R01(i + 3) * 12f, m.C(0xFFFFFF, .55f * (1f - e * .5f)));
        }
        m.Seg = keep;
        m.Lathe(0, 0, m.C(0xE8F4FA, .3f), 70, -280, 70, -40, 252, 290, 228, 336, 0, 336);
        m.Pitch(0, -285, 0, MathF.PI / 2f, () => m.Torus(0, -285, 0, 78, 13, m.C(0xE8F4FA, .9f)));
        m.Limb(-150, 240, 150, -100, 145, 105, 6, 5, m.C(0xFFFFFF, .6f));
        for (int i = 0; i < 8; i++) { float e = Fr(t * .42f + i / 8f); m.Ball(MathF.Sin(e * 7f + i) * (10f + e * 42f), -310f - e * 240f, MathF.Cos(e * 5f + i) * e * 30f, 14f + e * 34f, new Vector4(liq.X, liq.Y, liq.Z, .34f * (1f - e) * m.Al)); }
    }

    /// <summary>テストの答案: 厚みのある紙、立体の文字(問題・点数)、赤ペンの丸とバツ</summary>
    static void TestPaper(TestPaperParameter p, Sculpt m, float t)
    {
        string s = Str(p);
        int n = Math.Clamp(p.Count, 1, 8), score = int.TryParse(s, out var sc) ? sc : 100, ok = (int)MathF.Round(score / 100f * n); const float W = 760, H = 900;
        var red = m.C(p.Color2); var ink = m.C(0x2B2E33);
        m.Box(0, 0, -6, W, H, 10, m.C(p.Color1), .04f);
        m.Limb(-340, -300, 0, 340, -300, 0, 1.6f, 1.6f, m.C(0x444444));
        m.Text("テスト", 50, -200, -372, 1, 3, ink);
        float q = Cl((t - .3f - n * .28f) * 4f);
        m.Tube([.. Sculpt.ArcPts(235, -348, 88, 0, MathF.Tau * q, 32).Select(u => new Vector3(u, 4))], 3.5f, red);
        for (int i = 0; i < n; i++)
        {
            float y = -240 + i * 590f / n, k = Cl((t - .2f - i * .28f) * 5f);
            m.Text($"問{i + 1}", 34, -285, y, 1, 3, ink);
            m.Box(-20, y, 0, 340, 3, 3, m.C(0x9A9AA0), .3f);
            m.Limb(-180, y - 26, 1, 110, y - 26, 1, 4, 4, m.C(0xB9BEC6));
            if (k <= 0f) continue;
            if (i < ok) m.Tube([.. Sculpt.ArcPts(245, y, 40, 0, k * MathF.Tau, 24).Select(u => new Vector3(u, 4))], 3.5f, red);
            else
            {
                m.Limb(205, y - 38, 4, 205 + 80 * k, y - 38 + 76 * k, 4, 3.5f, 3.5f, red);
                if (k >= 1f) m.Limb(285, y - 38, 4, 205, y + 38, 4, 3.5f, 3.5f, red);
            }
        }
        if (q > 0f) m.Text(s, 74, 235, -350, 8, 10, m.C(p.Color2, q));
    }

    /// <summary>時間割: ぽんと飛び出すマス(厚みのある板)と、立体の曜日・科目の文字</summary>
    static void Timetable(ClassTimetableParameter p, Sculpt m, float t, float v)
    {
        var r = Str(p).Split('\n').Select(x => x.Split([',', '、']).Select(y => y.Trim()).ToArray()).Where(x => x.Length > 0 && x[0].Length > 0).Take(8).ToList();
        if (r.Count == 0) return;
        const int d = 5; string wk = "月火水木金"; float cw = 230, rh = MathF.Min(135f, 680f / r.Count), x0 = -cw * d / 2f + 45, y0 = -rh * r.Count / 2f + 45;
        int hit = Math.Clamp((int)MathF.Round(v * 4f), 0, 4);
        for (int c = 0; c < d; c++)
        {
            float x = x0 + (c + .5f) * cw;
            m.Box(x, y0 - 45, 0, cw - 7, 84, 30, m.C(c == hit ? p.Color3 : p.Color1), .2f);
            m.Text(wk[c].ToString(), 42, x, y0 - 45, 18, 6, c == hit ? m.C(0x263238) : m.C(0xFFFFFF));
        }
        float now = m.Now;
        for (int y = 0; y < r.Count; y++)
        {
            float yy = y0 + (y + .5f) * rh;
            m.Text($"{y + 1}", 44, x0 - 52, yy, 6, 10, m.C(0xFFFFFF), 0, m.C(0x000000, .5f), 3);
            for (int c = 0; c < d; c++)
            {
                float k = Cl((now - .1f - (c + y) * .05f) * 4f), q = DrawKit.EaseOutBack(k), x = x0 + (c + .5f) * cw;
                if (q <= .01f) continue;
                var cellCol = c == hit ? Vector4.Lerp(m.C(p.Color2), m.C(p.Color3), .35f) : m.C(p.Color2);
                m.Box(x, yy, -10 + 20 * q, (cw - 7) * q, (rh - 7) * q, 22, cellCol, .2f);
                if (c < r[y].Length) m.Text(r[y][c], 38, x, yy, 12 + 20 * q, 5, m.C(0x263238, k), cw - 22);
            }
        }
    }

    /// <summary>卒業証書: 開いていく紙、左右の筒(金の帯)、たて書きの立体の文字と赤い印</summary>
    static void Diploma(DiplomaScrollParameter p, Sculpt m, float t)
    {
        float k = DrawKit.EaseOut(Cl((t - .2f) * .9f)), W = 110 + k * 820; const float H = 560;
        m.Box(0, 0, 0, W, H, 4, m.C(p.Color2), .02f);
        if (k > .55f)
        {
            float a = Cl((k - .55f) * 3f);
            m.Tube([new(-(W - 66) / 2f, -(H - 54) / 2f, 3), new((W - 66) / 2f, -(H - 54) / 2f, 3), new((W - 66) / 2f, (H - 54) / 2f, 3), new(-(W - 66) / 2f, (H - 54) / 2f, 3), new(-(W - 66) / 2f, -(H - 54) / 2f, 3)], 2.5f, m.C(p.Color3, a));
            string s = Str(p);
            int cnt = Math.Min(s.Length, 6);
            for (int i = 0; i < cnt; i++) m.Text(s[i].ToString(), 72, W * .24f, (i - (cnt - 1) / 2f) * 84, 4, 5, m.C(0x1F2024, a));
            for (int l = 0; l < 4; l++) m.Limb(W * .08f - l * 62, -180, 3, W * .08f - l * 62, 180, 3, 2, 2, m.C(0x55565C, .55f * a));
            m.Box(-W * .28f, 170, 6, 88, 88, 10, m.C(0xD7263D, a), .2f);
            m.Text("印", 48, -W * .28f, 170, 12, 3, m.C(0xFFFFFF, a));
        }
        foreach (int s in Sides)
        {
            float x = s * W / 2f;
            m.Cyl(x, 0, 0, 46, H + 42, m.C(p.Color1), .35f);
            m.Cyl(x, -H / 2f + 22, 0, 47, 12, m.C(p.Color3), .2f);
            m.Cyl(x, H / 2f - 22, 0, 47, 12, m.C(p.Color3), .2f);
        }
    }

    /// <summary>サッカーのゴール: 白い枠(前と奥)・ふくらむネット・芝、白黒のボールがゴールへ</summary>
    static void SoccerGoal(SoccerGoalParameter p, Sculpt m, float t)
    {
        float k = Cl((t - .15f) * 1.5f), hit = Cl((t - .82f) * 2f), bulge = k >= 1f ? MathF.Sin(hit * MathF.PI * 3f) * (1f - hit) * 36f : 0f;
        float bx = k < 1f ? -620 + k * 780 : 160 + bulge * .4f, by = k < 1f ? 300 - k * 370 - MathF.Sin(k * MathF.PI) * 150f : -70 + hit * hit * 330f, bz = k < 1f ? 500 - k * 600 : -100 - bulge;
        var frame = m.C(p.Color1); const float back = -320;
        m.Super(0, 322, -100, 900, 6, 620, .2f, .3f, m.C(0x3F9A46));
        foreach (int s in Sides)
        {
            m.Limb(s * 510, 310, 0, s * 510, -270, 0, 10, 10, frame);
            m.Limb(s * 510, -270, 0, s * 510, -230, back, 6, 6, frame);
            m.Limb(s * 510, -230, back, s * 510, 310, back, 6, 6, frame);
        }
        m.Limb(-510, -270, 0, 510, -270, 0, 10, 10, frame);
        m.Limb(-510, -230, back, 510, -230, back, 6, 6, frame);
        int keep = m.Seg; m.Seg = 8;
        var net = m.C(p.Color1, .42f);
        float Bz(float x, float y) { float dd = new Vector2(x - 160, y + 70).Length(); return MathF.Exp(-dd * dd / 26000f) * bulge * 1.5f; }
        for (int i = -8; i <= 8; i++) m.Tube([.. Enumerable.Range(0, 9).Select(j => { float y = -230 + j * 67.5f, x = i * 63.75f; return new Vector3(x, y, back - Bz(x, y)); })], 1.4f, net);
        for (int j = 0; j <= 8; j++) m.Tube([.. Enumerable.Range(0, 17).Select(i => { float y = -230 + j * 67.5f, x = (i - 8) * 63.75f; return new Vector3(x, y, back - Bz(x, y)); })], 1.4f, net);
        for (int i = 0; i <= 6; i++) { float z = back * i / 6f; foreach (int s in Sides) m.Limb(s * 510, -270 + 40 * i / 6f, z, s * 510, 310, z, 1.4f, 1.4f, net); m.Limb(-510, -270 + 40 * i / 6f, z, 510, -270 + 40 * i / 6f, z, 1.4f, 1.4f, net); }
        m.Seg = keep;
        float R = 64f - k * 18f;
        m.Ball(bx, by, bz, R, m.C(0xFFFFFF));
        m.Turn(bx, by, t * 12f * (1f - hit), () =>
        {
            for (int i = 0; i < 6; i++)
            {
                float a = i * MathF.Tau / 5f, el = i == 5 ? 0f : .9f;
                var dir = i == 5 ? new Vector3(0, 0, 1) : new Vector3(MathF.Cos(a) * MathF.Sin(el), MathF.Sin(a) * MathF.Sin(el), MathF.Cos(el));
                m.Ball(bx + dir.X * R * .93f, by + dir.Y * R * .93f, bz + dir.Z * R * .93f, R * .28f, R * .28f, R * .12f, m.C(0x20242B));
            }
        });
        if (k >= 1f) m.Text(Str(p), 180, 0, -60, 120, 40, m.C(p.Color2, Cl(hit * 4f)), 0, m.C(0xD7263D, Cl(hit * 4f)), 9);
    }

    /// <summary>野球のホームラン: 回るバット(太さの変わる棒)、縫い目のあるボールが遠くへ飛ぶ、光と文字</summary>
    static void Baseball(BaseballHomerParameter p, Sculpt m, float t)
    {
        float inK = Cl(t * 1.8f), sw = Cl((t - .42f) * 7f), ang = 2.15f - sw * sw * 3.1f, h = Cl((t - .55f) * .7f); var hp = new Vector2(-340, 170);
        m.Turn(hp.X, hp.Y, ang, () =>
        {
            m.Limb(hp.X, hp.Y, 0, hp.X + 260, hp.Y, 0, 11, 17, m.C(p.Color1));
            m.Limb(hp.X + 260, hp.Y, 0, hp.X + 500, hp.Y, 0, 17, 34, m.C(p.Color1));
        });
        m.Ball(hp.X, hp.Y, 0, 22, m.C(0x3A3E45));
        float bx, by, bz, r;
        if (t < .55f) { bx = 700 - inK * 740; by = -120 + inK * 145; bz = 200 - inK * 200; r = 25; }
        else
        {
            bx = -35 + h * 880; by = 25 - MathF.Sin(h * MathF.PI * .72f) * 500f; bz = -h * 1400f; r = 34f;
            for (int i = 1; i < 16; i++) { float q = MathF.Max(0f, h - i * .018f); m.Ball(-35 + q * 880, 25 - MathF.Sin(q * MathF.PI * .72f) * 500f, -q * 1400f, r * (1f - i / 17f), m.C(0xFFFFFF, .3f * (1f - i / 16f))); }
        }
        if (h < 1f)
        {
            m.Ball(bx, by, bz, r, m.C(0xFFFFFF));
            foreach (int s in Sides) m.Tube(Sculpt.OnFront(bx, by, bz, r, r, r, .5f, Sculpt.ArcPts(bx + s * r * .9f, by, r * .9f, s > 0 ? MathF.PI - .75f : -.75f, s > 0 ? MathF.PI + .75f : .75f, 10)), MathF.Max(1f, r * .06f), m.C(0xE5202E));
        }
        float f = Cl((t - .55f) * 8f) * (1f - Cl((t - .7f) * 6f));
        if (f > 0f) m.Slab(0, 4, m.C(0xFFF2A8, f), StarPts(-35, 25, 170f * f, 10, .35f, 0));
        float tx = Cl((t - 1.1f) * 3f);
        if (tx > 0f) m.Text(Str(p), 130, 0, 300, 60, 30, m.C(p.Color2, tx), 0, m.C(0xD7263D, tx), 7);
    }

    /// <summary>バスケのシュート: ボード(四角の印)・支柱・水平のリング・下がる網、筋のあるボール</summary>
    static void Basket(BasketShotParameter p, Sculpt m, float t)
    {
        float cyc = Fr(t * .42f), k = Cl(cyc / .55f), fall = Cl((cyc - .55f) / .25f), bx = -620 + k * 930, by = k < 1f ? 310 - k * 520 - MathF.Sin(k * MathF.PI) * 410f : -210 + fall * fall * 560f, bz = k < 1f ? 300 - k * 300 : 0;
        const float rx = 310, ry = -200; var red = m.C(p.Color2);
        m.Box(450, -340, -120, 340, 240, 14, m.C(0xFFFFFF, .9f), .1f);
        m.Tube([new(280, -460, -110), new(620, -460, -110), new(620, -220, -110), new(280, -220, -110), new(280, -460, -110)], 4, red);
        m.Tube([new(338, -347, -110), new(462, -347, -110), new(462, -253, -110), new(338, -253, -110), new(338, -347, -110)], 3, red);
        m.Box(620, -60, -160, 26, 760, 26, m.C(0x70757B), .3f);
        m.Limb(450, -230, -125, rx, ry, -80, 5, 5, m.C(0x70757B));
        m.Pitch(rx, ry, 0, MathF.PI / 2f, () => m.Torus(rx, ry, 0, 100, 6, red));
        float pull = k >= 1f ? MathF.Sin(fall * MathF.PI) * 38f : 0f;
        int keep = m.Seg; m.Seg = 8;
        for (int i = 0; i < 12; i++)
        {
            float a = i * MathF.Tau / 12f;
            m.Limb(rx + MathF.Cos(a) * 100, ry, MathF.Sin(a) * 100, rx + MathF.Cos(a + .5f) * 58, ry + 125 + pull, MathF.Sin(a + .5f) * 58, 1.5f, 1.5f, m.C(0xFFFFFF, .8f));
        }
        m.Seg = keep;
        const float R = 66;
        m.Ball(bx, by, bz, R, m.C(p.Color1));
        m.Turn(bx, by, t * 7f, () =>
        {
            var ink = m.C(0x3A2010);
            m.Pitch(bx, by, bz, MathF.PI / 2f, () => m.Torus(bx, by, bz, R * 1.005f, 2.5f, ink));
            m.Torus(bx, by, bz, R * 1.005f, 2.5f, ink);
            m.Yaw(bx, by, bz, MathF.PI / 2f, () => m.Torus(bx, by, bz, R * 1.005f, 2.5f, ink));
        });
        if (fall > 0f && fall < 1f) m.Text("SWISH!", 72, 280, -420, 0, 14, m.C(0xFFD54A, 1f - fall), 0, m.C(0x000000, .6f * (1f - fall)), 4);
    }

    /// <summary>テニスのラリー: 奥へのびるコート(線)・ネットと支柱、2本のラケット、バウンドするボールと影</summary>
    static void Tennis(TennisRallyParameter p, Sculpt m, float t)
    {
        const float gy = 300, zn = 380, zf = -1100;
        m.Box(0, gy + 6, (zn + zf) / 2f, 1000, 8, zn - zf, m.C(p.Color1), .02f);
        var white = m.C(0xFFFFFF);
        foreach (int s in Sides) m.Limb(s * 460, gy, zn, s * 460, gy, zf, 3, 3, white);
        m.Limb(-460, gy, zn, 460, gy, zn, 3, 3, white); m.Limb(-460, gy, zf, 460, gy, zf, 3, 3, white);
        m.Limb(-380, gy, 60, 380, gy, 60, 2.5f, 2.5f, white); m.Limb(-380, gy, -700, 380, gy, -700, 2.5f, 2.5f, white); m.Limb(0, gy, 60, 0, gy, -700, 2.5f, 2.5f, white);
        const float nz = -320;
        m.Box(0, gy - 50, nz, 940, 100, 3, m.C(0xFFFFFF, .3f), .02f);
        m.Box(0, gy - 102, nz, 950, 10, 8, white, .3f);
        foreach (int s in Sides) m.Limb(s * 475, gy, nz, s * 475, gy - 110, nz, 6, 6, m.C(0x70757B));
        float ph = t * .9f; int round = (int)MathF.Floor(ph); float k = ph - round; bool up = round % 2 == 0;
        float e = up ? k : 1f - k, side = (R01(round) - .5f) * 280f, side2 = (R01(round + 1) - .5f) * 280f, s0 = up ? side : side2, s1 = up ? side2 : side;
        float x = s0 + (s1 - s0) * k, z = zn - 120 - e * (zn - 120 - zf - 200), y = gy - 30 - MathF.Sin(k * MathF.PI) * 260f;
        m.Super(x, gy - 1, z, 24, 1, 24, 1f, 1f, m.C(0x000000, .32f));
        void Racket(float rx, float rz, float swing) => m.Turn(rx, 180, swing, () =>
        {
            m.Limb(rx, 240, rz, rx, 300, rz, 8, 7, m.C(0x3A3E45));
            m.Torus(rx, 180, rz, 54, 6, m.C(0xE53935));
            for (int i = -2; i <= 2; i++) { m.Limb(rx + i * 16, 132, rz, rx + i * 16, 228, rz, 1, 1, m.C(0xFFFFFF, .6f)); m.Limb(rx - 46, 180 + i * 18, rz, rx + 46, 180 + i * 18, rz, 1, 1, m.C(0xFFFFFF, .6f)); }
        });
        float near = up ? 1f - Cl(k * 5f) : Cl((k - .8f) * 5f), far = up ? Cl((k - .8f) * 5f) : 1f - Cl(k * 5f);
        Racket(up ? side + 70 : side2 + 70, zn - 60, -.7f + near * 1.1f);
        Racket((up ? side2 : side) - 40, zf + 120, .6f - far * 1.1f);
        m.Ball(x, y, z, 22, m.C(p.Color2));
        m.Tube(Sculpt.OnFront(x, y, z, 22, 22, 22, .3f, Sculpt.ArcPts(x - 18, y, 15, -.9f, .9f, 8)), 1.4f, white);
    }

    /// <summary>ボウリングのストライク: 奥へのびるレーン、三角に並ぶピン(くびれた回転体と赤い帯)、転がるボール、はじけるピン</summary>
    static void Bowling(BowlingStrikeParameter p, Sculpt m, float t)
    {
        float k = Cl(t * .85f), hit = MathF.Max(0f, t - 1.18f);
        const float gy = 300, pinZ = -900;
        m.Box(0, gy + 8, -400, 520, 14, 1700, m.C(0xC99A5B), .02f);
        for (int i = -4; i <= 4; i++) m.Limb(i * 52, gy, 450, i * 52, gy, -1250, 1, 1, m.C(0x9A7040, .55f));
        foreach (int s in Sides) m.Box(s * 300, gy + 14, -400, 80, 20, 1700, m.C(0x5A4A3A), .3f);
        int[][] rows = [[0], [-1, 1], [-2, 0, 2], [-3, -1, 1, 3]]; int id = 0;
        for (int r = 0; r < 4; r++)
            foreach (int c in rows[r])
            {
                int n = id++;
                float px = c * 44f, pz = pinZ - r * 76f;
                float a = hit * (3f + R01(n) * 7f) * (c >= 0 ? 1f : -1f), dx = hit * (150f + R01(n + 3) * 280f) * (c >= 0 ? 1f : -.8f), dy = -hit * 420f + hit * hit * 620f, dz = -hit * (100 + R01(n + 7) * 300f);
                float op = 1f - Cl((hit - .7f) * 3f); if (op <= 0f) continue;
                float bxp = hit > 0f ? dx : 0, byp = hit > 0f ? dy : 0, bzp = hit > 0f ? dz : 0;
                m.Move(bxp, byp, bzp, () => m.Turn(px, gy - 70, hit > 0f ? a : 0, () =>
                {
                    m.Lathe(px, pz, m.C(0xFFFFFF, op), 0, gy - 150, 12, gy - 146, 18, gy - 132, 12, gy - 110, 12, gy - 100, 24, gy - 70, 30, gy - 40, 26, gy - 12, 18, gy, 0, gy);
                    m.Lathe(px, pz, m.C(0xE5202E, op), 12.6f, gy - 117, 12.6f, gy - 103);
                }));
            }
        if (k < 1f || hit < .6f)
        {
            float bz = k < 1f ? 400 - k * (400 - pinZ) : pinZ - hit * 400, bx = k < 1f ? -120 + k * 120 : hit * 200;
            m.Ball(bx, gy - 76, bz, 76, m.C(p.Color1));
            m.Yaw(bx, gy - 76, bz, -t * 16f, () => { m.Ball(bx + 22, gy - 102, bz + 66, 8, 8, 6, m.C(0x0D1540)); m.Ball(bx + 46, gy - 94, bz + 56, 8, 8, 6, m.C(0x0D1540)); m.Ball(bx + 36, gy - 68, bz + 66, 8, 8, 6, m.C(0x0D1540)); });
        }
        if (hit > 0f && hit < .25f) m.Slab(pinZ + 50, 4, m.C(0xFFF2A8, 1f - hit * 4f), StarPts(0, gy - 120, 240f * (1f - hit * 2f), 12, .4f, 0));
        if (hit > .15f) m.Text(Str(p), 190, 0, -200, 40, 40, m.C(p.Color2), 0, m.C(0xD7263D), 9);
    }

    /// <summary>卓球のラリー: 台(白い線・ネット・脚)、左右のラケット(赤と黒)、はねる球と残像</summary>
    static void PingPong(PingPongRallyParameter p, Sculpt m, float t)
    {
        const float top = 110;
        m.Box(0, top + 16, 0, 1100, 32, 620, m.C(p.Color1), .05f);
        var white = m.C(0xFFFFFF);
        m.Box(0, top - .5f, 0, 1100, 2, 620, m.C(p.Color1), .02f);
        m.Limb(-550, top - 1, 310, 550, top - 1, 310, 3, 3, white); m.Limb(-550, top - 1, -310, 550, top - 1, -310, 3, 3, white);
        foreach (int s in Sides) { m.Limb(s * 550, top - 1, 310, s * 550, top - 1, -310, 3, 3, white); foreach (int zs in Sides) m.Box(s * 450, top + 170, zs * 240, 26, 270, 26, m.C(0x3A3E45), .2f); }
        m.Limb(-550, top - 1, 0, 550, top - 1, 0, 1.5f, 1.5f, white);
        m.Box(0, top - 44, 0, 6, 90, 680, m.C(0xEEEEEE, .55f), .05f);
        m.Box(0, top - 88, 0, 8, 6, 682, white, .3f);
        float ph = t * 1.5f; int rd = (int)MathF.Floor(ph); float k = ph - rd, dir = rd % 2 == 0 ? 1f : -1f, bounce = .68f;
        float Y(float q) { float y = q < bounce ? top - 16 - MathF.Abs(MathF.Sin(q / bounce * MathF.PI)) * 230f : top - 16 - MathF.Sin((q - bounce) / (1f - bounce) * MathF.PI * .5f) * 180f; if (q < bounce * .5f) y -= (bounce * .5f - q) * 260f; return y; }
        float Z(float q) => MathF.Sin(q * MathF.PI) * 120f * (rd % 3 - 1);
        foreach (int s in Sides)
        {
            bool hitNow = (s < 0) == (dir > 0) ? k < .12f : k > .9f; float rx = s * 690, swing = s * (hitNow ? .55f : -.2f);
            m.Turn(rx, -45, swing, () => { m.Box(rx + s * 8, 65, 0, 26, 92, 22, m.C(0xC98A4B), .4f); m.Yaw(rx, -45, 0, s * .5f, () => m.Disc(rx, -45, 0, 84, 84, 14, m.C(s < 0 ? 0xE53935u : 0x1E2228u), .4f)); });
        }
        for (int i = 1; i <= 6; i++) { float q = k - i * .018f; if (q <= 0f) continue; m.Ball(dir * (-640 + q * 1280), Y(q), Z(q), 16f * (1f - i / 7f), m.C(p.Color2, .32f * (1f - i / 7f))); }
        m.Ball(dir * (-640 + k * 1280), Y(k), Z(k), 18, m.C(p.Color2));
    }

    /// <summary>ゴルフのカップイン: 盛り上がったグリーン・カップの穴・旗、転がるボールと軌跡、はじける星</summary>
    static void Golf(GolfCupInParameter p, Sculpt m, float t)
    {
        m.Super(0, 200, 0, 760, 40, 420, .6f, 1f, m.C(p.Color1));
        m.Super(40, 168, 0, 560, 12, 300, .6f, 1f, Vector4.Lerp(m.C(p.Color1), m.C(0xFFFFFF), .16f));
        const float cx = 330, cy = 156; const float cz = 0;
        m.Super(cx, cy + 1, cz, 46, 2, 46, .2f, 1f, m.C(0x20242B));
        m.Limb(cx, cy, cz, cx, -310, cz, 4, 4, m.C(0xF2F2F2));
        m.Slab(cz, 4, m.C(p.Color2), cx, -310, cx + 175, -258 + MathF.Sin(t * 5f) * 11f, cx, -196);
        float k = Cl(t * .55f), e = 1f - (1f - k) * (1f - k);
        float X(float a) => -560 + a * (cx + 560); float Zp(float a) => 260 * (1f - a) + MathF.Sin(a * 8f) * 10f * (1f - a);
        if (k < 1f)
        {
            var q = new List<Vector3>(); for (int i = 0; i <= 30; i++) { float a = e * i / 30f; q.Add(new(X(a), cy - 2, Zp(a))); }
            int keep = m.Seg; m.Seg = 8; m.Tube(q, 2, m.C(0xFFFFFF, .34f)); m.Seg = keep;
            m.Ball(X(e), cy - 22, Zp(e), 22, m.C(0xFFFFFF));
        }
        else
        {
            float f = Cl((t - 1.82f) * 2f);
            for (int i = 0; i < 16; i++) { float a = i / 16f * MathF.Tau, r = 30f + f * 190f; m.Slab(MathF.Sin(a) * r * .6f, 4, m.C(0xFFE45C), StarPts(cx + MathF.Cos(a) * r, cy - 40 - f * 120f, 18f * (1f - f), 4, .25f, 0)); }
            m.Text(Str(p), 140, -80, -130, 40, 30, m.C(0xFFFFFF, Cl(f * 3f)), 0, m.C(0x1F6E2C, Cl(f * 3f)), 7);
        }
    }

    /// <summary>ゴールテープ: 左右の柱、張ったテープ(FINISH)が切れてなびく、舞う紙吹雪と文字</summary>
    static void FinishTape(FinishTapeParameter p, Sculpt m, float t)
    {
        float k = Cl((t - .5f) * 2.4f); var tape = m.C(p.Color1);
        foreach (int s in Sides) m.Box(s * 620, 0, 0, 30, 660, 30, m.C(0xD7DBE0), .4f);
        if (k < .02f) { m.Box(0, -40, 0, 1240, 26, 6, tape, .1f); m.Text("FINISH", 22, 0, -40, 5, 2, m.C(0xFFFFFF)); }
        else
        {
            int keep = m.Seg; m.Seg = 10;
            foreach (int s in Sides)
            {
                var q = new List<Vector3>();
                for (int i = 0; i <= 28; i++) { float e = i / 28f, wave = MathF.Sin(e * 8f + t * 9f) * 22f * e * k, reach = 1f - k * .62f; q.Add(new(s * (620 - e * 620 * reach) + s * k * e * 8f, -40 + wave + k * e * e * 180f, -k * e * 200f + MathF.Cos(e * 6f + t * 7f) * 20f * e)); }
                m.Tube(q, 10, tape);
            }
            m.Seg = keep;
            uint[] cs = [0xFF5C8A, 0x4FC3F7, 0xFFE45C, 0x66E08A];
            for (int i = 0; i < 44; i++)
            {
                float e = Fr(t * .35f + R01(i)), x = (R01(i + 5) - .5f) * 1100f + MathF.Sin(e * 8f + i) * 26f, z = (R01(i + 9) - .5f) * 600f, y = -360 + e * 720;
                m.Pitch(x, y, z, e * 7f + i, () => m.Turn(x, y, e * 9f + i, () => m.Box(x, y, z, 22, 10, 2, m.C(cs[i % 4]), .2f)));
            }
            m.Text(Str(p), 220, 0, 110, 60, 50, m.C(p.Color2, Cl(k * 3f)), 0, m.C(0xD7263D, Cl(k * 3f)), 10);
        }
    }

    /// <summary>ダーツ: 厚みのある的(色分けの扇・ダブルとトリプルの輪・ブル)と、刺さってゆれる立体のダーツ</summary>
    static void Darts(DartsHitParameter p, Sculpt m, float t)
    {
        const float R = 370;
        m.Disc(0, 0, -40, R + 34, R + 34, 50, m.C(0x17181B), .2f);
        for (int i = 0; i < 20; i++)
        {
            float a0 = i * MathF.Tau / 20f - MathF.PI / 20f - MathF.PI / 2f, a1 = a0 + MathF.Tau / 20f;
            m.Slab(-14, 4, i % 2 == 0 ? m.C(0x20242B) : m.C(0xF2E6C8), PiePts(0, 0, R, a0, a1, 6));
            foreach (float rr in new[] { R, R * .62f })
            {
                var ring = new List<float>();
                for (int j = 0; j <= 6; j++) { float a = a0 + (a1 - a0) * j / 6f; ring.Add(MathF.Cos(a) * rr); ring.Add(MathF.Sin(a) * rr); }
                for (int j = 6; j >= 0; j--) { float a = a0 + (a1 - a0) * j / 6f; ring.Add(MathF.Cos(a) * (rr - 24)); ring.Add(MathF.Sin(a) * (rr - 24)); }
                m.Slab(-11, 4, i % 2 == 0 ? m.C(p.Color1) : m.C(p.Color2), [.. ring]);
            }
        }
        m.Disc(0, 0, -11, 58, 58, 4, m.C(p.Color2), .2f);
        m.Disc(0, 0, -8, 26, 26, 4, m.C(p.Color1), .2f);
        int n = Math.Clamp(p.Count, 1, 6);
        for (int i = 0; i < n; i++)
        {
            float k = Cl((t - .2f - i * .5f) * 3.6f);
            if (k <= 0f) continue;
            float hx = i == n - 1 ? 3 : (R01(i + 3) - .5f) * 330, hy = i == n - 1 ? 4 : (R01(i + 9) - .5f) * 330;
            float x = hx + (1f - k) * 860, y = hy - (1f - k) * 240 + MathF.Sin(k * MathF.PI) * -60f, z = -10 + (1f - k) * 600;
            float shake = k >= 1f ? MathF.Sin((t - .2f - i * .5f) * 56f) * .08f * MathF.Exp(-(t - .48f - i * .5f) * 5f) : 0f;
            m.Yaw(x, y, z, -1.25f + shake, () => m.Turn(x, y, -.32f, () =>
            {
                m.Limb(x, y, z, x + 72, y, z, 1, 3, m.C(0xCDD2D8));
                m.Limb(x + 70, y, z, x + 166, y, z, 11, 9, m.C(0x3A3E45));
                m.Limb(x + 165, y, z, x + 225, y, z, 3, 3, m.C(0xE5E7EA));
                m.Slab(z, 2, m.C(0x4FC3F7), x + 205, y, x + 260, y - 42, x + 275, y);
                m.Slab(z, 2, m.C(0x2F86C6), x + 205, y, x + 260, y + 42, x + 275, y);
                m.Pitch(x, y, z, MathF.PI / 2f, () => { m.Slab(y, 2, m.C(0x4FC3F7), x + 205, z, x + 260, z - 42, x + 275, z); m.Slab(y, 2, m.C(0x2F86C6), x + 205, z, x + 260, z + 42, x + 275, z); });
            }));
        }
        float d = Cl((t - .48f - (n - 1) * .5f) * 4f);
        if (d > 0f) m.Text(Str(p), 150, 0, -440, 30, 30, m.C(0xFFE45C, d), 0, m.C(0xD7263D, d), 8);
    }

    /// <summary>相撲の土俵: 盛り土の台・俵の輪・仕切り線、つり屋根と房、行司の軍配、まわりの観客</summary>
    static void Sumo(SumoRingParameter p, Sculpt m, float t)
    {
        m.Box(0, 0, -900, m.W * 1.8f, m.H * 1.8f, 4, m.C(0x1A1820), .02f);
        int keep = m.Seg; m.Seg = 10;
        for (int r = 0; r < 4; r++) for (int i = 0; i < 30; i++)
            {
                float a = MathF.PI * (.05f + .9f * (i + (r % 2) * .5f) / 30f), rad = 1100 + r * 160;
                m.Ball(MathF.Cos(a) * rad * -1f, -40 + r * 50 + MathF.Sin(t * 2f + i) * 2f, -MathF.Sin(a) * rad * .6f - 200, 18, m.C(R01(i * 7 + r) > .5f ? 0x4A4048u : 0x35303Au));
            }
        m.Seg = keep;
        var soil = m.C(p.Color1);
        m.Box(0, 400, 0, 1300, 280, 1100, soil, .12f);
        m.Super(0, 262, 0, 540, 6, 500, .3f, 1f, Vector4.Lerp(soil, m.C(0xFFFFFF), .16f));
        m.Pitch(0, 258, 0, MathF.PI / 2f, () => m.Torus(0, 258, 0, 395, 11, m.C(0xF4E5C4)));
        m.Box(-36, 258, 0, 8, 2, 30, m.C(0xFFFFFF), .3f); m.Box(36, 258, 0, 8, 2, 30, m.C(0xFFFFFF), .3f);
        var roof = m.C(p.Color2);
        m.Box(0, -440, 0, 900, 120, 900, roof, .15f);
        m.Box(0, -345, 0, 1060, 34, 1060, m.C(0x3A2415), .1f);
        for (int i = 0; i < 26; i++) { float x = -510 + i * 40.8f; m.Box(x, -300, 530, 14, 66, 2, m.C(0xF2F2EC), .3f); m.Box(x, -300, -530, 14, 66, 2, m.C(0xF2F2EC), .3f); m.Box(530, -300, x, 2, 66, 14, m.C(0xF2F2EC), .3f); m.Box(-530, -300, x, 2, 66, 14, m.C(0xF2F2EC), .3f); }
        foreach (int s in Sides) foreach (int zs in Sides)
            {
                float sw = MathF.Sin(t * 1.3f + s + zs) * 6f;
                m.Limb(s * 520, -345, zs * 520, s * 520 + sw, -170, zs * 520, 6, 6, m.C(p.Color3));
                m.Ball(s * 520 + sw, -120, zs * 520, 34, 66, 34, m.C(p.Color3));
            }
        m.Turn(0, 170, MathF.Sin(t * 3f) * .13f, () =>
        {
            m.Limb(0, 215, 0, 0, 110, 0, 6, 6, m.C(0x4E2F16));
            m.Disc(0, 62, 0, 66, 68, 10, m.C(0x7A4A25), .3f);
            m.Torus(0, 62, 0, 66, 4, m.C(0xE6C65A));
        });
        m.Text(Str(p), 90, 0, -130, 60, 22, m.C(0xFFFFFF), 900, m.C(p.Color3), 6);
    }

    // ================================================================ 家電
    /// <summary>扇風機: 台と支柱、首をふる頭(モーター・3枚の羽根・前後のガード)、風の線</summary>
    static void Fan(ElectricFanParameter p, Sculpt m, float t)
    {
        var body = m.C(p.Color2);
        m.Super(0, 395, 0, 230, 22, 180, .4f, 1f, body);
        m.Cyl(0, 240, -20, 22, 310, body, .3f);
        m.Box(120, 382, 100, 40, 14, 30, m.C(0x4FA8FF), .4f);
        float yaw = MathF.Sin(t * .7f) * .9f;
        m.Yaw(0, -80, -20, yaw, () => m.Move(0, -80, () =>
        {
            m.Ball(0, 0, -95, 80, 80, 90, body);
            for (int b = 0; b < 3; b++) { float a = t * 16f + b * MathF.Tau / 3f; m.Turn(0, 0, a, () => m.Pitch(138, 0, 0, .35f, () => m.Ball(138, 0, 0, 118, 66, 8, m.C(p.Color1, .78f)))); }
            m.Ball(0, 0, 10, 45, 45, 30, body);
            m.Ball(0, 0, 36, 19, 19, 10, m.C(0x4FA8FF));
            var g = m.C(0xB7C2CC);
            const float R = 255;
            foreach (float z in new[] { 55f, -55f }) m.Torus(0, 0, z, R, 5, g);
            m.Torus(0, 0, 70, R * .66f, 2.5f, m.C(0xB7C2CC, .6f));
            int keep = m.Seg; m.Seg = 8;
            for (int i = 0; i < 24; i++)
            {
                float a = i * MathF.Tau / 24f, cx = MathF.Cos(a), cy = MathF.Sin(a);
                m.Tube([new(cx * 50, cy * 50, 75), new(cx * R * .66f, cy * R * .66f, 70), new(cx * R, cy * R, 55), new(cx * R * 1.02f, cy * R * 1.02f, 0), new(cx * R, cy * R, -55), new(cx * 60, cy * 60, -80)], 1.5f, m.C(0xB7C2CC, .55f));
            }
            m.Seg = keep;
        }));
        for (int i = 0; i < 7; i++) { float k = Fr(t * .75f + i / 7f), y = -280 + i * 62, x = 300 + k * 330; m.Tube([new(x, y, 40), new(x + 45, y - 12, 50), new(x + 90, y + 5, 60)], 2.5f, m.C(0xFFFFFF, .55f * (1f - k))); }
    }

    /// <summary>洗濯機: 箱形の本体・操作パネル(表示とつまみ)・丸い扉(ガラス)、中で回る水と洗濯物と泡</summary>
    static void Washer(WashingMachineParameter p, Sculpt m, float t, float now)
    {
        float sh = MathF.Sin(t * 26f) * 4f;
        m.Move(sh, 0, () =>
        {
            var body = m.C(p.Color1);
            m.Box(0, 0, -60, 680, 800, 560, body, .1f);
            m.Box(0, -275, 222, 680, 4, 4, m.C(0xAEB7C0), .3f);
            m.Box(190, -335, 222, 170, 60, 6, m.C(0x20303A), .2f);
            m.Text($"{Math.Max(0, 45 - (int)now)}分", 34, 190, -335, 227, 2, m.C(0x5CFFB0));
            m.Disc(-230, -335, 228, 42, 42, 20, m.C(0xC4CCD4), .4f);
            m.Limb(-230, -335, 240, -230 + MathF.Cos(t) * 34, -335 + MathF.Sin(t) * 34, 240, 4, 4, m.C(0x3A3E45));
            m.Box(-90, -335, 222, 90, 40, 12, m.C(0xC4CCD4), .4f);
            const float cy = 90, R = 235;
            m.Torus(0, cy, 224, R + 21, 22, m.C(0xCBD3DA));
            m.Disc(0, cy, 214, R, R, 6, m.C(0x233345), .1f);
            float wob = MathF.Sin(t * 2.2f) * .12f;
            m.Slab(219, 4, m.C(p.Color2, .8f), PiePts(0, cy, R - 2, .18f + wob, MathF.PI - .18f + wob, 20));
            uint[] cs = [0xFF7F9F, 0xFFE27A, 0xA3E37A];
            for (int i = 0; i < 7; i++) { float a = t * 3.2f + i * MathF.Tau / 7f, r = 95f + R01(i) * 75f; float x = MathF.Cos(a) * r, y = cy + MathF.Sin(a) * r; m.Turn(x, y, a * 1.4f, () => m.Box(x, y, 222, 112, 62, 18, m.C(cs[i % 3]), .5f)); }
            int keep = m.Seg; m.Seg = 10;
            for (int i = 0; i < Math.Clamp(p.Count, 0, 80); i++) { float a = R01(i) * MathF.Tau + t * (1f + R01(i + 3)), r = MathF.Sqrt(R01(i + 7)) * (R - 16); m.Ball(MathF.Cos(a) * r, cy + MathF.Sin(a) * r, 228, 5f + R01(i + 9) * 9f, m.C(0xFFFFFF, .6f)); }
            m.Seg = keep;
            m.Disc(0, cy, 236, R, R, 8, m.C(0xDDEEFF, .22f), .2f);
            m.Tube([.. Sculpt.ArcPts(0, cy, R - 24, 3.5f, 4.6f, 10).Select(q => new Vector3(q, 242))], 5, m.C(0xFFFFFF, .55f));
        });
    }

    /// <summary>電子レンジ: 中が空いた箱(光る庫内・回る皿と食べ物)・ガラスの扉・表示と押しボタン、終わると「チン！」</summary>
    static void Microwave(MicrowaveChinParameter p, Sculpt m, float t, float v)
    {
        float dur = MathF.Max(.5f, v * 10f), left = dur - m.Now; bool on = left > 0f;
        var body = m.C(p.Color1); const float D = 480, fz = 240;
        m.Box(0, -285, 0, 980, 30, D, body, .2f); m.Box(0, 285, 0, 980, 30, D, body, .2f);
        m.Box(-475, 0, 0, 30, 600, D, body, .2f); m.Box(345, 0, 0, 290, 600, D, body, .15f);
        m.Box(-125, 0, -225, 700, 560, 30, body, .1f);
        var glow = on ? m.C(p.Color2, .9f) : m.C(0x3C4A56);
        m.Box(-125, 0, -205, 640, 540, 6, glow, .05f);
        m.Super(-125, 230, 0, 220, 8, 170, .3f, 1f, m.C(0xFFFFFF, .7f));
        m.Yaw(-125, 0, 0, t * 2.4f, () => { m.Super(-125, 215, 0, 130, 8, 100, .4f, 1f, m.C(0xFFFFFF)); m.Box(-125, 160, 0, 190, 104, 120, m.C(0xE57F4A), .5f); m.Box(-125 + 44, 160, 0, 26, 106, 122, m.C(0xFFE27A), .3f); });
        m.Box(-125, 0, fz, 640, 540, 6, m.C(0x2A333D, .35f), .05f);
        m.Tube([new(-445, -270, fz), new(195, -270, fz), new(195, 270, fz), new(-445, 270, fz), new(-445, -270, fz)], 8, m.C(0x2A333D));
        m.Box(355, -190, fz + 2, 200, 86, 6, m.C(0x20303A), .2f);
        m.Text(on ? $"{(int)left / 60}:{(int)left % 60:00}" : "0:00", 48, 355, -190, fz + 7, 3, m.C(0x5CFFB0));
        for (int y = 0; y < 4; y++) for (int x = 0; x < 3; x++) m.Box(295 + x * 60, -95 + y * 58, fz + 4, 46, 42, 12, m.C(0xCBD3DA), .35f);
        m.Box(355, 200, fz + 4, 170, 58, 14, m.C(0x4FA8FF), .35f);
        if (!on)
        {
            float k = Cl(-left * 2.5f), fade = 1f - Cl((-left - 1.4f) * 2f);
            if (fade > 0f)
            {
                for (int i = 0; i < 10; i++) { float a = i / 10f * MathF.Tau; m.Limb(MathF.Cos(a) * (170 + k * 40), -330 + MathF.Sin(a) * (80 + k * 20), fz + 40, MathF.Cos(a) * (220 + k * 70), -330 + MathF.Sin(a) * (110 + k * 36), fz + 60, 4, 4, m.C(0xFFE45C, fade)); }
                m.Text(Str(p), 110, 0, -330, fz + 50, 24, m.C(0xFFFFFF, fade), 0, m.C(0xE5202E, fade), 7);
            }
        }
    }

    /// <summary>炊飯器: 角の丸い本体・ドームのふた・取っ手・表示パネル(文字・進み具合・ランプ)、湯気</summary>
    static void RiceCooker(RiceCookerParameter p, Sculpt m, float t, float v)
    {
        float dur = MathF.Max(.5f, v * 10f); bool done = m.Now >= dur;
        var body = m.C(p.Color1);
        m.Super(0, 95, 0, 320, 210, 270, .45f, .55f, body);
        m.Super(0, -110, 0, 322, 70, 272, .7f, .55f, m.C(0xE3E7EB));
        m.Super(0, -140, 0, 270, 50, 230, .8f, .55f, body);
        m.Box(0, -205, 0, 140, 40, 60, m.C(0xB7C2CC), .5f);
        const float fz = 266;
        m.Box(0, 150, fz, 380, 160, 10, m.C(0x20303A), .25f);
        m.Text(done ? Str(p) : "炊飯中", 52, 0, 120, fz + 7, 3, m.C(done ? 0xFFE27Au : 0x5CFFB0u), 340);
        float bar = Cl(m.Now / dur);
        m.Box(-140 + 140 * bar, 192, fz + 6, 280 * bar, 12, 4, m.C(0x5CFFB0), .3f);
        m.Ball(255, 150, fz - 6, 19, 19, 14, done || (int)(t * 3f) % 2 == 0 ? m.C(p.Color2) : m.C(0x7A4040));
        Steam(m, 3, 0, 48, -235, done ? 150 : 260, 0, t * 1.2f, 5);
    }

    /// <summary>掃除機: 車輪の付いた本体・曲がるホース・パイプ・吸い込み口、吸い込まれるほこり</summary>
    static void Vacuum(VacuumCleanerParameter p, Sculpt m, float t)
    {
        float nx = 230 + MathF.Sin(t * .9f) * 150f;
        m.Limb(-700, 300, 0, 700, 300, 0, 2, 2, m.C(0xFFFFFF, .35f));
        int keep = m.Seg; m.Seg = 8;
        for (int i = 0; i < Math.Clamp(p.Count, 0, 120); i++)
        {
            float k = Fr(t * .7f + R01(i)), sx = nx + 90 + R01(i + 5) * 340, sy = 290 - R01(i + 8) * 30, sz = (R01(i + 2) - .5f) * 200, x = sx + (nx - sx) * k * k, y = sy + (262 - sy) * k * k, z = sz * (1f - k * k);
            m.Ball(x, y, z, (3f + R01(i + 9) * 6f) * (1f - k * .6f), m.C(p.Color2, 1f - k * .4f));
        }
        m.Seg = keep;
        var dark = m.C(0x3A3E45);
        m.Box(nx, 276, 0, 240, 40, 140, dark, .3f);
        m.Limb(nx - 28, 245, 0, -115, -260, 0, 9, 9, m.C(0xB9BEC6));
        m.Box(-115, -285, 0, 98, 40, 50, dark, .4f);
        var hose = new List<Vector3>();
        for (int i = 0; i <= 26; i++) { float e = i / 26f; hose.Add(new(-120 - e * 220 + MathF.Sin(e * MathF.PI) * -60f, -270 + e * 420 - MathF.Sin(e * MathF.PI) * 170f + MathF.Sin(e * 18f + t * 3f) * 3f, MathF.Sin(e * MathF.PI) * 80f)); }
        m.Tube(hose, 13, m.C(0x50555C));
        for (int i = 1; i < 26; i += 2) { var q = hose[i]; m.Ball(q.X, q.Y, q.Z, 15, 15, 15, m.C(0x70757B)); }
        m.Super(-470, 180, 0, 170, 105, 130, .5f, .6f, m.C(p.Color1));
        m.Super(-470, 130, 60, 90, 30, 80, .5f, .6f, m.C(0xBCD2FF));
        foreach (float x in new[] { -575f, -365f }) foreach (int s in Sides) { m.Disc(x, 268, s * 120, 42, 42, 24, m.C(0x20242B), .4f); m.Disc(x, 268, s * 134, 15, 15, 6, m.C(0xB9BEC6), .3f); }
    }

    /// <summary>ラジカセ: 箱形の本体と持ち手、左右のスピーカー(拍で前に出る)、カセット(回るリール)、光るメーター、飛ぶ音符</summary>
    static void Boombox(BoomboxParameter p, Sculpt m, float t)
    {
        float beat = MathF.Pow(MathF.Abs(MathF.Sin(t * 3.4f)), 5f), s0 = 1f + beat * .025f;
        m.Scale(0, 180, 0, s0, () =>
        {
            const float fz = 130;
            m.Tube([.. Sculpt.ArcPts(0, -175, 330, MathF.PI, MathF.Tau, 20).Select(q => new Vector3(q, 0))], 10, m.C(0x8A8F98));
            m.Box(0, 40, 0, 1240, 500, 260, m.C(p.Color1), .12f);
            foreach (int s in Sides)
            {
                float x = s * 410;
                m.Disc(x, 70, fz, 158, 158, 6, m.C(0x17181C), .2f);
                m.Torus(x, 70, fz + 4, 154, 7, m.C(0xB9BEC6));
                m.Disc(x, 70, fz + 6 + beat * 14, 105 + beat * 18, 105 + beat * 18, 10, m.C(0x2C2F36), .5f);
                m.Ball(x, 70, fz + 14 + beat * 20, 44 + beat * 12, 44 + beat * 12, 22, m.C(0xB9BEC6));
            }
            m.Box(0, 90, fz - 4, 400, 245, 14, m.C(0x17181C), .2f);
            m.Box(0, 90, fz + 2, 370, 212, 6, m.C(0xE7DEC8), .1f);
            m.Box(0, 30, fz + 6, 330, 42, 2, m.C(0xE5533D), .1f);
            foreach (int s in Sides)
            {
                float x = s * 90;
                m.Disc(x, 125, fz + 6, 50, 50, 4, m.C(0x2C2F36), .3f);
                m.Turn(x, 125, t * 4f * s, () => { for (int k = 0; k < 6; k++) { float q = k * MathF.Tau / 6f; m.Limb(x + MathF.Cos(q) * 12, 125 + MathF.Sin(q) * 12, fz + 9, x + MathF.Cos(q) * 36, 125 + MathF.Sin(q) * 36, fz + 9, 3, 3, m.C(0xFFFFFF)); } });
            }
            for (int i = 0; i < 18; i++) { float h = (.2f + MathF.Abs(MathF.Sin(t * 5f + i * .9f)) * .8f) * 50f; m.Box(-170 + i * 20, -130 - h / 2f, fz + 2, 13, h, 8, m.C(0x5CFFB0), .2f); }
        });
        for (int i = 0; i < 8; i++)
        {
            float k = Fr(t * .3f + i / 8f), side = i % 2 == 0 ? -1 : 1, x = side * (250 + k * 340) + MathF.Sin(k * 8f + i) * 26, y = -230 - k * 240, z = 100 + MathF.Cos(k * 6f + i) * 60f;
            var c = m.C(p.Color2, 1f - k);
            m.Ball(x, y, z, 22, 16, 14, c);
            m.Limb(x + 19, y, z, x + 19, y - 62, z, 3, 3, c);
            m.Limb(x + 19, y - 62, z, x + 44, y - 42, z, 3, 3, c);
        }
    }

    /// <summary>懐中電灯: 丸い筒・広がる頭とレンズ・スイッチと滑り止め、先から広がる光の円すい</summary>
    static void Flashlight(FlashlightBeamParameter p, Sculpt m, float t)
    {
        const float bx = -560, by = 240; float a = -.22f + MathF.Sin(t * .9f) * .26f, L = 1250f, sp = .19f;
        m.Turn(bx, by, a, () =>
        {
            // 光: 先が開いた円すい(だんだん薄く)
            m.Turn(bx, by, -MathF.PI / 2f, () =>
            {
                for (int i = 0; i < 4; i++)
                {
                    float a0 = i / 4f, a1 = (i + 1) / 4f;
                    m.Lathe(bx, 0, m.C(p.Color1, .2f * (1f - a0)), 50 + (L * MathF.Tan(sp) - 50) * a0, by + 10 + L * a0, 50 + (L * MathF.Tan(sp) - 50) * a1, by + 10 + L * a1);
                }
            });
            var body = m.C(p.Color2);
            m.Limb(bx - 62, by, 0, bx - 6, by, 0, 48, 64, body);
            m.Limb(bx - 330, by, 0, bx - 60, by, 0, 38, 38, body);
            m.Ball(bx + 1, by, 0, 6, 58, 58, m.C(p.Color1));
            m.Box(bx - 200, by - 40, 0, 74, 14, 34, m.C(0xE53935), .4f);
            for (int i = 0; i < 6; i++) m.Yaw(bx - 300 + i * 18, by, 0, MathF.PI / 2f, () => m.Torus(bx - 300 + i * 18, by, 0, 38, 2.5f, m.C(0x70757B)));
        });
    }

    /// <summary>蛇口の水: 壁から出る管・下向きの口・赤いハンドル、流れ落ちる水の筋・水面の波紋・しぶき</summary>
    static void Faucet(FaucetWaterParameter p, Sculpt m, float t)
    {
        const float wx = 180, top = -130, bot = 300;
        int keep = m.Seg; m.Seg = 12;
        for (int i = 0; i < 3; i++)
        {
            var q = new List<Vector3>();
            for (int k = 0; k <= 20; k++) { float e = k / 20f; q.Add(new(wx + (i - 1) * 13 * (1f - e * .45f) + MathF.Sin(e * 18f - t * 16f + i) * 3.5f, top + e * (bot - top), MathF.Cos(e * 14f - t * 12f + i) * 3f)); }
            m.Tube(q, i == 1 ? 8 : 4.5f, m.C(p.Color2, i == 1 ? .9f : .55f));
        }
        m.Super(wx, bot + 6, 0, 230, 3, 140, 1f, 1f, m.C(p.Color2, .42f));
        for (int r = 0; r < 3; r++) { float k = Fr(t * 1.1f + r / 3f); m.Pitch(wx, bot + 4, 0, MathF.PI / 2f, () => m.Torus(wx, bot + 4, 0, 40 + k * 160, 2.5f, m.C(0xFFFFFF, .7f * (1f - k)))); }
        for (int i = 0; i < Math.Clamp(p.Count, 0, 100); i++)
        {
            float k = Fr(t * (1f + R01(i)) + R01(i + 3)), dir = (R01(i + 5) - .5f) * 2f, dz = (R01(i + 11) - .5f) * 2f, h = 60f + R01(i + 8) * 110f;
            m.Ball(wx + dir * 150f * k, bot - MathF.Sin(k * MathF.PI) * h, dz * 120f * k, (2f + R01(i + 9) * 5f) * (1f - k * .5f), m.C(p.Color2, 1f - k));
        }
        m.Seg = keep;
        var metal = m.C(p.Color1);
        m.Box(-410, -220, -40, 70, 165, 80, metal, .4f);
        m.Limb(-380, -220, 0, 160, -220, 0, 43, 43, metal);
        m.Limb(160, -220, 0, 165, -150, 0, 46, 44, metal);
        m.Cyl(172, -128, 0, 48, 28, Sculpt.Dark(metal, .8f), .3f);
        m.Cyl(-20, -292, 0, 26, 76, metal, .3f);
        m.Box(-20, -336, 0, 190, 34, 60, m.C(0xE53935), .5f);
        m.Limb(-360, -258, 30, 110, -258, 30, 4, 4, m.C(0xFFFFFF, .5f));
    }

    /// <summary>ドライヤー: 丸い筒の本体・広がる吹き出し口・持ち手とコード・後ろの回る羽根、吹き出す風</summary>
    static void Dryer(HairDryerParameter p, Sculpt m, float t)
    {
        for (int i = 0; i < 12; i++)
        {
            float k = Fr(t * 1.2f + R01(i)), y = -60 + (i - 5.5f) * (11f + k * 36f), x = 120 + k * 560, z = (R01(i + 4) - .5f) * 120f * (1f + k);
            var q = new List<Vector3>(); for (int s = 0; s <= 10; s++) { float e = s / 10f; q.Add(new(x + e * 150, y + MathF.Sin(e * 6f + t * 8f + i) * 11f, z)); }
            int keep = m.Seg; m.Seg = 8; m.Tube(q, 2.5f, m.C(p.Color2, .68f * (1f - k))); m.Seg = keep;
        }
        var body = m.C(p.Color1);
        m.Limb(-155, 20, 0, -155, 335, 0, 52, 46, body);
        m.Box(-155, 215, 40, 76, 36, 20, m.C(0xFFFFFF, .75f), .4f);
        m.Tube([new(-155, 352, 0), new(-135, 410, 20), new(-190, 450, 40), new(-280, 420, 20)], 5, m.C(0x50555C));
        m.Limb(-320, -60, 0, 60, -60, 0, 100, 100, body);
        m.Limb(60, -60, 0, 195, -60, 0, 98, 60, Sculpt.Dark(body, .82f));
        m.Ball(-336, -60, 0, 10, 84, 84, m.C(0x2B2E33));
        for (int b = 0; b < 5; b++) { float a = t * 18f + b * MathF.Tau / 5f; m.Limb(-346, -60, 0, -346, -60 + MathF.Cos(a) * 72, MathF.Sin(a) * 72, 6, 4, m.C(0xB9BEC6, .85f)); }
        m.Ball(-350, -60, 0, 8, 14, 14, body);
        m.Limb(-200, -150, 60, 40, -150, 60, 5, 5, m.C(0xFFFFFF, .5f));
    }

    /// <summary>黒電話: 台形の本体・受話器と受け台・回るダイヤル(数字・指穴・指止め)、鳴る時の波とベルの文字</summary>
    static void Phone(RotaryPhoneParameter p, Sculpt m, float t)
    {
        bool ring = Fr(t * .35f) < .55f; float sh = ring ? MathF.Sin(t * 58f) * 7f : 0f;
        var body = m.C(p.Color1);
        m.Move(sh, 0, () =>
        {
            m.Slab(-40, 440, body, -420, 310, 420, 310, 330, -60, 215, -160, -215, -160, -330, -60);
            m.Turn(0, -240, sh * .007f, () =>
            {
                m.Box(0, -255, -40, 780, 90, 110, body, .45f);
                foreach (int s in Sides) m.Super(s * 345, -210, -40, 88, 66, 82, .5f, .7f, body);
            });
            foreach (int s in Sides) m.Box(s * 170, -175, -40, 50, 60, 80, m.C(0x2A2C31), .3f);
            const float cy = 110, R = 180, fz = 182;
            m.Disc(0, cy, fz, R, R, 10, m.C(0xB9BEC6), .3f);
            m.Disc(0, cy, fz + 5, R - 10, R - 10, 4, m.C(p.Color2), .2f);
            for (int i = 0; i < 10; i++) { float a = -2.5f + i * .52f; m.Text(((i + 1) % 10).ToString(), 26, MathF.Cos(a) * (R - 40), cy + MathF.Sin(a) * (R - 40), fz + 8, 2, m.C(0x202024)); }
            float dial = MathF.Sin(t * 1.1f) > .25f ? (MathF.Sin(t * 1.1f) - .25f) * 1.5f : 0f;
            m.Turn(0, cy, dial, () =>
            {
                m.Disc(0, cy, fz + 14, R - 66, R - 66, 4, m.C(0x20242B), .2f);
                for (int i = 0; i < 10; i++) { float a = -2.5f + i * .52f; m.Torus(MathF.Cos(a) * (R - 40), cy + MathF.Sin(a) * (R - 40), fz + 16, 20, 4, m.C(0x20242B, .9f)); }
            });
            m.Disc(0, cy, fz + 18, 48, 48, 6, m.C(0xE5202E), .3f);
            m.Disc(0, cy, fz + 22, 22, 22, 4, m.C(0xFFFFFF, .85f), .3f);
            m.Limb(R - 12, cy + 70, fz + 16, R + 18, cy + 110, fz + 16, 4, 4, m.C(0xB9BEC6));
        });
        if (ring)
        {
            foreach (int s in Sides) for (int i = 0; i < 3; i++)
                {
                    float k = Fr(t * 2.4f + i / 3f);
                    m.Tube([.. Sculpt.ArcPts(s * 470, -230, 40 + k * 75, s > 0 ? -.75f : MathF.PI - .75f, s > 0 ? .75f : MathF.PI + .75f, 10).Select(q => new Vector3(q, 0))], 4, m.C(0xFFE45C, 1f - k));
                }
            m.Text(Str(p), 76, MathF.Sin(t * 37f) * 7f, -405, 20, 18, m.C(0xFFE45C), 0, m.C(0x000000), 5);
        }
    }
}
