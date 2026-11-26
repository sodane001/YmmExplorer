using System.Numerics;
using System.Text.RegularExpressions;

namespace YMM4EffectShapes;

// =====================================================================
//  専用3Dモデル: 121〜150番(背景・季節・解説・配信UI)
// =====================================================================
internal static partial class Models3D
{
    /// <summary>出てから消えるまでの濃さ(出る時間・消える時間)</summary>
    static float InOutK(Sculpt m, DrawContext c, float inT, float outT)
    {
        float a = Math.Clamp(m.Now / MathF.Max(.01f, inT), 0, 1), len = c.Length / (float)MathF.Max(1, c.Fps), b = outT <= 0 ? 1 : Math.Clamp((len - m.Now) / outT, 0, 1);
        return Ease(MathF.Min(a, b));
    }
    static readonly uint[][] ChartPals =
    [
        [0xFF5A5F, 0xFFB400, 0x00A699, 0x4A90E2, 0x9B59B6, 0x2ECC71],
        [0xFFB3BA, 0xFFDFBA, 0xBAFFC9, 0xBAE1FF, 0xE0BBE4, 0xFFFFBA],
        [0x1E88E5, 0x26C6DA, 0x5C6BC0, 0x00897B, 0x7E57C2, 0x90CAF9],
        [0xE53935, 0xFB8C00, 0xFDD835, 0xD81B60, 0xF4511E, 0xFFB300],
        [0xECEFF1, 0xB0BEC5, 0x78909C, 0x546E7A, 0x37474F, 0x90A4AE],
    ];
    static Vector4 ChartCol(Sculpt m, ChartPalette pal, int i) { var a = ChartPals[Math.Clamp((int)pal, 0, 4)]; return m.C(a[i % a.Length]); }
    static void UseFont(Sculpt m, string? f) { if (!string.IsNullOrWhiteSpace(f)) m.Font = f; }

    static partial void RegisterK1e(Dictionary<Type, SceneModel> d)
    {
        var W = new Vector4(1, 1, 1, 1);
        var K = new Vector4(.08f, .09f, .11f, 1);
        S<BoardBgParameter>(d, (p, c, m) => // 黒板・ホワイトボード・ノート: 画面いっぱいの厚い板と木の枠、チョークの粉受け、方眼・罫線
        {
            uint def = p.Kind switch { BoardKind.Whiteboard => 0xF4F6F8u, BoardKind.GridNote or BoardKind.LinedNote => 0xFFFDF4u, _ => 0x24493Au }; var bc = p.BaseColor.A > 0 ? m.C(p.BaseColor) : m.C(def);
            m.Box(0, 0, -40, m.W, m.H, 30, bc, .01f);
            if (p.Smudge && p.Kind != BoardKind.GridNote && p.Kind != BoardKind.LinedNote) for (int i = 0; i < 6; i++) m.Ball((R(p.Seed, i) - .5f) * m.W * .8f, (R(p.Seed, i, 1) - .5f) * m.H * .6f, -24, 260, 90, 4, m.C(0xFFFFFFu, .05f));
            float gs = MathF.Max(15, c.F(p.GridSize)) * 1.5f;
            if (p.Kind == BoardKind.GridNote) { for (float x = -m.W / 2; x < m.W / 2; x += gs) m.Box(x, 0, -24, 2, m.H, 1, m.C(0x90CAF9u, .6f)); for (float y = -m.H / 2; y < m.H / 2; y += gs) m.Box(0, y, -24, m.W, 2, 1, m.C(0x90CAF9u, .6f)); }
            if (p.Kind == BoardKind.LinedNote) { for (float y = -m.H / 2 + gs; y < m.H / 2; y += gs) m.Box(0, y, -24, m.W, 2, 1, m.C(0x90CAF9u, .7f)); m.Box(-m.W * .38f, 0, -24, 3, m.H, 1, m.C(0xEF9A9Au)); }
            if (p.ShowFrame && p.Kind is BoardKind.Chalkboard or BoardKind.Whiteboard)
            {
                var fc = p.Kind == BoardKind.Chalkboard ? m.C(0x8D5A2Bu) : m.C(0xB0BEC5u); RectBars(m, 0, 0, 0, m.W - 50, m.H - 50, 50, fc);
                m.Box(0, m.H / 2 - 40, 40, m.W * .9f, 20, 60, fc, .2f); for (int i = 0; i < 3; i++) m.Turn(-200 + i * 120, m.H / 2 - 58, 0, () => m.Limb(-200 + i * 120, m.H / 2 - 58, 50, -140 + i * 120, m.H / 2 - 58, 50, 8, 8, Pal(m, i * 2)));
            }
        });
        S<ClockHourglassParameter>(d, (p, c, m) => // 時計・砂時計: 文字盤と動く針の厚い掛け時計/上から下へ砂が落ちる、くびれたガラスの砂時計
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius);
            if (p.Kind == TimeKind.Clock)
            {
                float sec = c.F(p.StartHour) * 3600 + c.F(p.StartMinute) * 60 + m.Now * c.F(p.TimeScale);
                m.Disc(o.X, o.Y, -20, r * 1.1f, r * 1.1f, 50, m.C(p.BodyColor), .5f); m.Disc(o.X, o.Y, 6, r, r, 6, m.C(p.FaceColor), .2f);
                for (int i = 0; i < 12; i++) { float a = i * MathF.Tau / 12; if (p.Numbers) m.Text((i == 0 ? 12 : i).ToString(), r * .2f, o.X + MathF.Sin(a) * r * .78f, o.Y - MathF.Cos(a) * r * .78f, 10, 3, m.C(p.HandColor)); else m.Box(o.X + MathF.Sin(a) * r * .85f, o.Y - MathF.Cos(a) * r * .85f, 10, 8, 8, 4, m.C(p.HandColor)); }
                void Hand(float a, float len, float w, float z, Vector4 col) => m.Limb(o.X, o.Y, z, o.X + MathF.Sin(a) * len, o.Y - MathF.Cos(a) * len, z, w, w * .6f, col);
                Hand(sec / 43200 * MathF.Tau, r * .5f, r * .05f, 14, m.C(p.HandColor)); Hand(sec / 3600 * MathF.Tau, r * .75f, r * .035f, 18, m.C(p.HandColor));
                if (p.SecondHand) Hand(sec / 60 * MathF.Tau, r * .85f, r * .012f, 22, m.C(0xE53935u)); m.Ball(o.X, o.Y, 24, r * .05f, m.C(p.HandColor));
                return;
            }
            float st = MathF.Max(.5f, c.F(p.SandTime)), ph = m.Now / st; int cyc = (int)ph; float f = ph - cyc, flip = p.Flip && f > .9f ? (f - .9f) * 10 : 0; float k = p.Flip ? Math.Min(1, f / .9f) : Math.Min(1, ph), h = r * 1.4f;
            m.Turn(o.X, o.Y, flip * MathF.PI, () =>
            {
                foreach (int s in Sides) m.Disc(o.X, o.Y + s * h, 0, r * .75f, r * .75f, r * .12f, m.C(p.BodyColor), .3f);
                foreach (int a in Sides) foreach (int b in Sides) m.Cyl(o.X + a * r * .6f, o.Y, b * r * .3f, r * .05f, h * 2, m.C(p.BodyColor));
                m.Lathe(o.X, 0, m.C(0xDDEEFFu, .25f), r * .55f, o.Y - h, r * .5f, o.Y - h * .4f, r * .08f, o.Y, r * .5f, o.Y + h * .4f, r * .55f, o.Y + h);
                var sand = m.C(0xE8C07Au); float top = (1 - k) * h * .85f, bot = k * h * .7f;
                if (top > 2) m.Lathe(o.X, 0, sand, r * .45f * (1 - k * .5f), o.Y - top, r * .08f, o.Y - 6);
                if (bot > 2) m.Lathe(o.X, 0, sand, r * .5f, o.Y + h - 4, r * .1f, o.Y + h - bot);
                if (k < 1) m.Cyl(o.X, o.Y + h / 2, 0, 3, h, sand);
            });
        });
        S<NewYearParameter>(d, (p, c, m) => // 正月: 竹を束ねた門松、空に揚がる凧、昇る初日の出、梅の枝
        {
            float sc = c.F(p.ItemScale) / 100;
            if (p.Sunrise) { float k = Ease(Math.Clamp(m.Now / MathF.Max(.1f, c.F(p.RiseTime)), 0, 1)); float y = m.H * .6f - k * m.H * .6f; m.Ball(0, y, -1200, 260, m.C(0xFF7043u)); m.Ball(0, y, -1220, 420, m.C(0xFFAB40u, .25f)); m.Box(0, m.H * .55f, -1100, m.W * 2.5f, m.H * .5f, 20, m.C(0x37474Fu), .01f); }
            if (p.Kadomatsu) foreach (int s in Sides) { float x = s * m.W * .38f, y = m.H / 2; m.Scale(x, y, 0, sc, () => { m.Cyl(x, y - 60, 0, 110, 120, m.C(0x8D6E63u), .2f); for (int i = 0; i < 3; i++) { float bx = x + (i - 1) * 55, hh = 300 + (i == 1 ? 100 : 0); m.Cyl(bx, y - 120 - hh / 2, 0, 26, hh, m.C(0x7CB342u)); m.Disc(bx, y - 120 - hh, 0, 26, 26, 4, m.C(0xC5E1A5u)); } for (int i = 0; i < 6; i++) m.Limb(x, y - 140, 0, x + (i - 2.5f) * 50, y - 260, 60, 6, 2, m.C(0x2E7D32u)); m.Torus(x, y - 70, 0, 112, 8, m.C(0xD32F2Fu)); }); }
            if (p.Kite) { float kx = m.W * .2f + MathF.Sin(m.Now * .8f) * 40, ky = -m.H * .3f + MathF.Sin(m.Now * 1.3f) * 30; m.Turn(kx, ky, MathF.Sin(m.Now) * .15f, () => { m.Box(kx, ky, -300, 220 * sc, 280 * sc, 6, m.C(0xFFF8E1u), .02f); m.Disc(kx, ky, -294, 70 * sc, 70 * sc, 4, m.C(0xD32F2Fu)); m.Tube(new List<Vector3> { V3(kx, ky + 140 * sc, -300), V3(kx - 30, ky + 260 * sc, -300), V3(kx + 20, ky + 380 * sc, -300) }, 4, m.C(0xD32F2Fu)); }); m.Limb(kx, ky + 50, -300, -m.W * .2f, m.H, 0, 1.5f, 1.5f, m.C(0xFFFFFFu, .6f)); }
            if (p.Plum) { var br = new List<Vector3> { V3(-m.W / 2, -m.H * .4f, 0), V3(-m.W * .35f, -m.H * .33f, 20), V3(-m.W * .2f, -m.H * .4f, 0) }; m.Tube(br, 10, m.C(0x4E342Eu)); for (int i = 0; i < 6; i++) { var q = Vector3.Lerp(br[0], br[2], i / 5f) + V3(0, (i % 2 - .5f) * 30, 15); for (int k2 = 0; k2 < 5; k2++) { float a = k2 * MathF.Tau / 5; m.Ball(q.X + MathF.Cos(a) * 14 * sc, q.Y + MathF.Sin(a) * 14 * sc, q.Z, 11 * sc, 11 * sc, 5, m.C(0xF06292u)); } m.Ball(q.X, q.Y, q.Z + 4, 5, m.C(0xFFEB3Bu)); } }
        });
        S<TanabataParameter>(d, (p, c, m) => // 七夕: しなる笹竹と葉、風に揺れる色とりどりの短冊、夜空の天の川
        {
            if (p.NightSky) { SkyBands(m, m.C(0x060B2Au), m.C(0x1A2A5Au), -1600, 8); Low(m, () => { for (int i = 0; i < 200; i++) { float u = (R(p.Seed, i) - .5f) * m.W * 2.2f, w = (R(p.Seed, i, 1) + R(p.Seed, i, 2) - 1) * 300; m.Ball(u * .9f + w * .4f, -u * .4f + w, -1500, 3, m.C(0xE3F2FDu, .7f)); } }, 5); }
            float x = (c.F(p.PosX) / 100 - .5f) * m.W, hh = c.F(p.StalkHeight) / 100 * m.H, wind = c.F(p.Wind) / 100, sw = MathF.Sin(m.Now * 1.5f) * .06f * wind;
            m.Turn(x, m.H / 2, sw, () =>
            {
                var stalk = new List<Vector3>(); for (int i = 0; i <= 8; i++) { float f = i / 8f; stalk.Add(V3(x - f * f * 160, m.H / 2 - hh * f, 0)); } m.Tube(stalk, 10, m.C(0x7CB342u));
                for (int i = 0; i < 18; i++) { var q = stalk[2 + i % 7]; float a = (R(p.Seed, i) - .5f) * 2.4f + MathF.PI; m.Turn(q.X, q.Y, a + MathF.Sin(m.Now * 3 + i) * .1f * wind, () => m.Ball(q.X + 70, q.Y, (R(p.Seed, i, 1) - .5f) * 80, 70, 14, 3, m.C(0x43A047u))); }
                for (int i = 0; i < Math.Min((int)c.F(p.Strips), 24); i++) { var q = stalk[2 + i % 6]; float ox = (R(p.Seed, i, 2) - .5f) * 260, oz = (R(p.Seed, i, 3) - .5f) * 200, tx = q.X + ox, ty = q.Y + 40; m.Limb(q.X, q.Y, 0, tx, ty, oz, 1, 1, m.C(0xFFFFFFu, .6f)); m.Turn(tx, ty, MathF.Sin(m.Now * 2.5f + i) * .25f * wind, () => m.Yaw(tx, ty, oz, MathF.Sin(m.Now * 2 + i) * .5f * wind, () => m.Box(tx, ty + 60, oz, 40, 120, 2, Pal(m, i), .02f))); }
            });
        });
        S<SummerParameter>(d, (p, c, m) => // 夏: 軒下に下がって揺れるガラスの風鈴と短冊、あおぐ丸いうちわ
        {
            float sc = c.F(p.ItemScale) / 100, wind = c.F(p.Wind) / 100;
            if (p.Chime) { m.Box(0, -m.H / 2 + 20, 0, m.W * 1.2f, 40, 120, m.C(0x6D4C41u), .1f); int n = Math.Clamp((int)c.F(p.ChimeCount), 1, 6); for (int i = 0; i < n; i++) { float x = ((i + .5f) / n - .5f) * m.W * .6f, top = -m.H / 2 + 40, sw = MathF.Sin(m.Now * 2.2f + i * 1.3f) * .18f * wind; m.Turn(x, top, sw, () => { m.Limb(x, top, 0, x, top + 120 * sc, 0, 1.5f, 1.5f, m.C(0xFFFFFFu, .7f)); m.Lathe(x, 0, m.C(p.ChimeColor, .55f), 0, top + 110 * sc, 50 * sc, top + 140 * sc, 70 * sc, top + 210 * sc, 68 * sc, top + 215 * sc); m.Lathe(x, 0, m.C(0xFFFFFFu, .35f), 66 * sc, top + 212 * sc, 48 * sc, top + 142 * sc); m.Limb(x, top + 200 * sc, 0, x, top + 300 * sc, 0, 1.5f, 1.5f, m.C(0xFFFFFFu, .7f)); m.Yaw(x, top + 300 * sc, 0, MathF.Sin(m.Now * 3 + i) * .6f * wind, () => m.Box(x, top + 370 * sc, 0, 50 * sc, 140 * sc, 2, m.C(0xFFF9C4u), .02f)); }); } }
            if (p.Fan) { var o = PctPos(m, c.F(p.FanX), c.F(p.FanY)); float a = MathF.Sin(m.Now * 5) * .35f; m.Turn(o.X, o.Y + 200 * sc, a, () => m.Yaw(o.X, o.Y, 0, a * .8f, () => { m.Disc(o.X, o.Y - 80 * sc, 0, 170 * sc, 160 * sc, 6, m.C(p.FanColor), .1f); for (int i = 0; i < 9; i++) { float b = -MathF.PI / 2 + (i - 4) * .3f; m.Limb(o.X, o.Y + 60 * sc, 4, o.X + MathF.Cos(b) * 165 * sc, o.Y - 80 * sc + MathF.Sin(b) * 155 * sc, 4, 2, 2, m.C(0xFFF8E1u, .8f)); } m.Box(o.X, o.Y + 150 * sc, 0, 28 * sc, 200 * sc, 14 * sc, m.C(0xD7B98Eu), .3f); })); }
        });
        S<ChristmasParameter>(d, (p, c, m) => // クリスマス: 3段玉の雪だるま(帽子・マフラー・鼻)、雪の地面、下がって揺れるオーナメント、降る雪
        {
            float sc = c.F(p.ItemScale) / 100;
            if (p.Ground) { m.Box(0, m.H / 2 + 10, -300, m.W * 2.4f, 40, 1600, W, .2f); for (int i = 0; i < 8; i++) m.Ball((R(p.Seed, i) - .5f) * m.W * 1.8f, m.H / 2, -900 + R(p.Seed, i, 1) * 900, 200, 60, 150, W); }
            if (p.Snowman) { float x = (c.F(p.PosX) / 100 - .5f) * m.W, y = m.H / 2; m.Scale(x, y, 0, sc, () => { m.Ball(x, y - 140, 0, 160, W); m.Ball(x, y - 370, 0, 115, W); m.Ball(x, y - 540, 0, 85, W); m.Torus(x, y - 470, 0, 80, 22, m.C(p.ScarfColor)); m.Limb(x + 40, y - 460, 70, x + 70, y - 330, 90, 22, 18, m.C(p.ScarfColor)); foreach (int s in Sides) m.Ball(x + s * 30, y - 560, 72, 11, K); m.Pitch(x, y - 540, 80, MathF.PI / 2, () => m.Lathe(x, 80, m.C(0xFF7043u), 14, y - 540, 0, y - 600)); for (int i = 0; i < 3; i++) m.Ball(x, y - 400 + i * 50, 110, 10, K); m.Cyl(x, y - 640, 0, 60, 90, K); m.Disc(x, y - 595, 0, 95, 95, 12, K); foreach (int s in Sides) m.Limb(x + s * 100, y - 380, 0, x + s * 220, y - 470, 0, 8, 4, m.C(0x5D4037u)); }); }
            if (p.Ornaments) for (int i = 0; i < Math.Min((int)c.F(p.OrnamentCount), 14); i++) { float x = ((i + .5f) / c.F(p.OrnamentCount) - .5f) * m.W * .9f, len = 120 + 160 * R(p.Seed, i, 2), sw = MathF.Sin(m.Now * 1.5f + i) * .12f; m.Turn(x, -m.H / 2, sw, () => { m.Limb(x, -m.H / 2, 0, x, -m.H / 2 + len, 0, 1.5f, 1.5f, m.C(0xFFD54Fu)); m.Cyl(x, -m.H / 2 + len + 8, 0, 12, 18, m.C(0xFFD54Fu)); m.Ball(x, -m.H / 2 + len + 60, 0, 50, Pal(m, i)); m.Ball(x - 18, -m.H / 2 + len + 40, 40, 10, m.C(0xFFFFFFu, .7f)); }); }
            if (p.Snow) Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.SnowCount), 200); i++) { float y = -m.H / 2 + Cyc(m.Now * 90 * (.6f + .4f * R(p.Seed, i, 5)) + R(p.Seed, i, 3) * m.H, m.H); m.Ball((R(p.Seed, i, 4) - .5f) * m.W * 1.3f + MathF.Sin(m.Now + i) * 30, y, -900 + R(p.Seed, i, 6) * 1100, 6, W); } }, 6);
        });
        S<MirrorBallParameter>(d, (p, c, m) => // ミラーボール: 鏡のタイル張りの回る玉と、まわりに散る光の点、舞台を照らす色の光の円すい
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float bs = c.F(p.BallSize) * 1.6f, sp = c.F(p.Speed) / 100, br = c.F(p.Brightness) / 100;
            m.Box(0, 0, -1000, m.W * 3, m.H * 3, 10, m.C(0x000000u, c.F(p.Darkness) / 100), .01f);
            if (p.Kind != StageKind.Spotlights)
            {
                m.Limb(o.X, -m.H / 2, 0, o.X, o.Y - bs, 0, 3, 3, m.C(0x9E9E9Eu));
                m.Yaw(o.X, o.Y, 0, m.Now * sp, () => { for (int i = 1; i < 10; i++) { float th = i * MathF.PI / 10, rr = MathF.Sin(th) * bs, y = o.Y - MathF.Cos(th) * bs; int n = Math.Max(4, (int)(rr / 14)); for (int j = 0; j < n; j++) { float a = j * MathF.Tau / n; m.Box(o.X + MathF.Cos(a) * rr, y, MathF.Sin(a) * rr, 22, 22, 6, m.C(Sculpt.Mix(m.C(0xB0BEC5u), m.C(0xFFFFFFu), R(i, j)), 1), .05f); } } });
                m.Ball(o.X, o.Y, 0, bs * .95f, m.C(0x607D8Bu));
                Low(m, () => { for (int i = 0; i < Math.Min((int)c.F(p.Spots), 120); i++) { float a = R(p.Seed, i) * MathF.Tau + m.Now * sp, el = (R(p.Seed, i, 1) - .3f) * 1.4f, dd = 1400; m.Ball(o.X + MathF.Cos(a) * MathF.Cos(el) * dd, o.Y + MathF.Sin(el) * dd * .6f, MathF.Sin(a) * MathF.Cos(el) * dd * .5f - 600, 10, m.C(p.Multicolor ? Pal(m, i) : m.C(p.Color), br)); } }, 5);
            }
            if (p.Kind != StageKind.MirrorBall) for (int i = 0; i < Math.Clamp((int)c.F(p.Lights), 1, 10); i++) { float x = ((i + .5f) / c.F(p.Lights) - .5f) * m.W, y0 = p.FromBottom ? m.H / 2 : -m.H / 2, a = MathF.Sin(m.Now * sp + i * 1.1f) * .5f, L = m.H * 1.2f; var col = p.Multicolor ? Pal(m, i) : m.C(p.Color); m.Limb(x, y0, -200, x + MathF.Sin(a) * L, y0 + (p.FromBottom ? -1 : 1) * MathF.Cos(a) * L, -200, 20, 160, m.C(col, .2f * br)); m.Cyl(x, y0 + (p.FromBottom ? -30 : 30), -200, 40, 70, m.C(0x37474Fu)); }
        });
        S<SpeechBubbleParameter>(d, (p, c, m) => // 吹き出し: 文字の入る厚い吹き出し(丸・楕円・ギザギザ・雲・四角)としっぽ
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float k = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)), fl = MathF.Sin(m.Now * 2) * c.F(p.Float); if (k <= 0) return; UseFont(m, p.Font);
            string s = TextKit.Lines(p.Text); float fs = c.F(p.FontSize); var lines = s.Split('\n'); float w = p.AutoSize ? MathF.Min(c.F(p.WrapWidth), lines.Max(l => l.Length) * fs * 1.02f) + c.F(p.Padding) * 2 : c.F(p.BoxWidth), h = p.AutoSize ? lines.Length * fs * 1.3f + c.F(p.Padding) * 2 : c.F(p.BoxHeight);
            int typed = c.F(p.TypeSpeed) > 0 ? (int)(m.Now * c.F(p.TypeSpeed)) : s.Length; var fc = m.C(p.FillColor, c.F(p.Opacity) / 100); var lc = m.C(p.LineColor);
            m.Scale(o.X, o.Y, 0, k, () => m.Move(0, fl, () =>
            {
                float[] outline = p.Style switch { SpeechBubbleStyle.Box => RoundRectFlat(o.X, o.Y, w, h, 12), SpeechBubbleStyle.Round => RoundRectFlat(o.X, o.Y, w, h, MathF.Min(w, h) * .35f), _ => Enumerable.Range(0, 48).SelectMany(i => { float a = i * MathF.Tau / 48, rr = p.Style == SpeechBubbleStyle.Spiky ? (i % 2 == 0 ? 1.18f : .95f) : p.Style == SpeechBubbleStyle.Cloud ? 1 + .06f * MathF.Abs(MathF.Sin(i * MathF.PI / 4)) : 1; return new[] { o.X + MathF.Cos(a) * w * .58f * rr, o.Y + MathF.Sin(a) * h * .62f * rr }; }).ToArray() };
                Puff(m, -14, 28, fc, outline);
                if (p.TailOn) { var tp = PctPos(m, c.F(p.TailX), c.F(p.TailY)); float tw = c.F(p.TailWidth); var bx = new Vector2(o.X + (tp.X - o.X) * .3f, o.Y + h * .4f * MathF.Sign(tp.Y - o.Y)); m.Slab(-14, 28, fc, bx.X - tw / 2, bx.Y, bx.X + tw / 2, bx.Y, tp.X, tp.Y); m.Tube(new List<Vector3> { V3(bx.X - tw / 2, bx.Y, 15), V3(tp.X, tp.Y, 15), V3(bx.X + tw / 2, bx.Y, 15) }, c.F(p.LineWidth) * .5f, lc); }
                m.Text(s[..Math.Min(typed, s.Length)], fs, o.X, o.Y, 16, 4, m.C(p.TextColor), w - c.F(p.Padding));
            }));
        });
        S<ArrowParameter>(d, (p, c, m) => // 矢印: 始点から終点へ伸びていく、ふち取りのある太い曲がった矢印と矢じり
        {
            var a = PctPos(m, c.F(p.StartX), c.F(p.StartY)); var b = PctPos(m, c.F(p.EndX), c.F(p.EndY)); float k = InOutK(m, c, c.F(p.DrawTime), c.F(p.OutTime)), grow = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.DrawTime)), 0, 1)), lw = c.F(p.LineWidth), hs = c.F(p.HeadSize), op = c.F(p.Opacity) / 100 * MathF.Min(1, k * 3);
            var dir = Vector2.Normalize(b - a); var nrm = new Vector2(-dir.Y, dir.X); float bend = p.Style == ArrowStyle.Straight || p.Style == ArrowStyle.Block ? 0 : c.F(p.Bend) / 100 * Vector2.Distance(a, b) * .5f, poke = MathF.Sin(m.Now * 8) * c.F(p.Poke);
            var pts = new List<Vector3>(); int n = 24; for (int i = 0; i <= n; i++) { float f = i / (float)n * grow; var q = Vector2.Lerp(a, b, f) + nrm * MathF.Sin(f * MathF.PI) * bend + dir * poke * f; pts.Add(V3(q.X, q.Y, 0)); }
            var col = m.C(p.Color, op); var oc = m.C(p.OutlineColor, op);
            if (p.Style == ArrowStyle.Dotted) for (int i = 0; i < pts.Count; i += 2) m.Ball(pts[i].X, pts[i].Y, 0, lw * .7f, col); else { m.Tube(pts, lw * (p.Style == ArrowStyle.Block ? 1.8f : .8f), col); m.Tube(pts.Select(q => q - V3(0, 0, 6)).ToList(), lw * (p.Style == ArrowStyle.Block ? 1.8f : .8f) + c.F(p.OutlineWidth), oc); }
            void Head(Vector3 tip, Vector3 prev) { var dd = Vector2.Normalize(new Vector2(tip.X - prev.X, tip.Y - prev.Y)); var nn = new Vector2(-dd.Y, dd.X); var t2 = new Vector2(tip.X, tip.Y) + dd * hs * .4f; var l = t2 - dd * hs + nn * hs * .6f; var r2 = t2 - dd * hs - nn * hs * .6f; m.Slab(-lw * .6f, lw * 1.2f, col, t2.X, t2.Y, l.X, l.Y, r2.X, r2.Y); }
            if (pts.Count > 1) { Head(pts[^1], pts[^2]); if (p.BothEnds) Head(pts[0], pts[1]); }
        });
        S<EncircleParameter>(d, (p, c, m) => // 丸で囲む: 手書きのように描かれていく、太い丸・四角・下線・波線・バツ・チェック
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float w = c.F(p.BoxWidth), h = c.F(p.BoxHeight), lw = c.F(p.LineWidth) * (p.Highlighter ? 3 : 1), rough = c.F(p.Rough) / 100, k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.DrawTime)), 0, 1)) * InOutK(m, c, .01f, c.F(p.OutTime));
            var col = m.C(p.Color, c.F(p.Opacity) / 100 * (p.Highlighter ? .5f : 1)); var pts = new List<Vector3>(); void Add(float x, float y, int i) => pts.Add(V3(o.X + x + (R(p.Seed, i) - .5f) * 16 * rough, o.Y + y + (R(p.Seed, i, 1) - .5f) * 16 * rough, 10));
            switch (p.Kind)
            {
                case EncircleKind.Circle: for (int i = 0; i <= 40; i++) { float a = -2 + i / 40f * MathF.Tau * 1.08f; Add(MathF.Cos(a) * w / 2, MathF.Sin(a) * h / 2, i); } break;
                case EncircleKind.Rect: { float[] xy = { -1, -1, 1, -1, 1, 1, -1, 1, -1, -1.1f }; for (int i = 0; i < 5; i++) Add(xy[i * 2] * w / 2, xy[i * 2 + 1] * h / 2, i); } break;
                case EncircleKind.Wavy: for (int i = 0; i <= 30; i++) Add(-w / 2 + w * i / 30, h / 2 + MathF.Sin(i * .9f) * 15, i); break;
                case EncircleKind.Cross: Add(-w * .3f, -h / 2, 0); Add(w * .3f, h / 2, 1); break;
                case EncircleKind.Check: Add(-w * .3f, 0, 0); Add(-w * .05f, h / 2, 1); Add(w * .35f, -h / 2, 2); break;
                case EncircleKind.Strike: Add(-w / 2, 0, 0); Add(w / 2, 0, 1); break;
                default: for (int i = 0; i <= 10; i++) Add(-w / 2 + w * i / 10, h / 2, i); break;
            }
            if (k <= 0) return; var part = pts.Take(Math.Max(2, (int)MathF.Ceiling(pts.Count * k))).ToList();
            m.Turn(o.X, o.Y, c.F(p.Tilt) * MathF.PI / 180, () => { m.Tube(part, lw, col); if (p.Kind == EncircleKind.DoubleUnderline) m.Tube(part.Select(q => q + V3(0, lw * 3)).ToList(), lw, col); if (p.Kind == EncircleKind.Cross && k > .5f) m.Tube(new List<Vector3> { V3(o.X + w * .3f, o.Y - h / 2, 10), V3(o.X - w * .3f, o.Y + h / 2, 10) }, lw, col); });
        });
        S<AnnotationParameter>(d, (p, c, m) => // 注釈: 指す点の玉から折れて伸びる引き出し線と、文字の入る札
        {
            var a = PctPos(m, c.F(p.PointX), c.F(p.PointY)); var b = PctPos(m, c.F(p.LabelX), c.F(p.LabelY)); float k = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.DrawTime)), 0, 1)) * InOutK(m, c, .01f, c.F(p.OutTime)); if (k <= 0) return; UseFont(m, p.Font);
            var lc = m.C(p.LineColor); var elbow = new Vector2(b.X - MathF.Sign(b.X - a.X) * 60, b.Y); float lw = c.F(p.LineWidth) * 1.5f;
            m.Ball(a.X, a.Y, 0, c.F(p.DotSize) * 1.5f, lc); m.Torus(a.X, a.Y, 0, c.F(p.DotSize) * 2.5f * (1 + Frac(m.Now)), 2, m.C(lc, 1 - Frac(m.Now)));
            var pts = new List<Vector3> { V3(a.X, a.Y, 0), V3(elbow.X, elbow.Y, 0), V3(b.X, b.Y, 0) }; float L1 = Vector2.Distance(a, elbow), L2 = Vector2.Distance(elbow, b), dd = (L1 + L2) * k;
            if (dd <= L1) m.Limb(pts[0], Vector3.Lerp(pts[0], pts[1], dd / MathF.Max(1, L1)), lw, lw, lc); else { m.Limb(pts[0], pts[1], lw, lw, lc); m.Limb(pts[1], Vector3.Lerp(pts[1], pts[2], (dd - L1) / MathF.Max(1, L2)), lw, lw, lc); }
            if (k >= 1)
            {
                string s = TextKit.Lines(p.Text); float fs = c.F(p.FontSize), w = s.Length * fs + 60, h = fs * 1.6f, cx = b.X + MathF.Sign(b.X - a.X) * w / 2;
                if (p.Style == AnnotationStyle.Underline) m.Box(cx, b.Y + h / 2, 0, w, lw * 2, lw * 2, lc, .3f); else m.Box(cx, b.Y, -10, w, h, 20, m.C(p.BoxColor), p.Style == AnnotationStyle.Pill ? .45f : .12f);
                m.Text(s, fs, cx, b.Y, 6, 4, m.C(p.TextColor), w);
            }
        });
        S<CountUpParameter>(d, (p, c, m) => // カウントアップ: 増えていく厚い数字と、横の小さな単位、ふち取り
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float f = Math.Clamp((m.Now - c.F(p.Delay)) / MathF.Max(.05f, c.F(p.Duration)), 0, 1);
            f = p.Ease switch { CountEase.Linear => f, CountEase.InOut => f * f * (3 - 2 * f), CountEase.Slot => f < 1 ? R(p.Seed, (int)(m.Now * 20)) : 1, _ => 1 - MathF.Pow(1 - f, 3) };
            double v = c.F(p.From) + (c.F(p.To) - c.F(p.From)) * f; string num = p.Comma ? v.ToString("N" + Math.Clamp(p.Decimals, 0, 4)) : v.ToString("F" + Math.Clamp(p.Decimals, 0, 4)); float fs = c.F(p.FontSize), pop = p.Pop && f >= 1 ? 1 + .15f * MathF.Max(0, 1 - (m.Now - c.F(p.Delay) - c.F(p.Duration)) * 4) : 1;
            float af = fs * c.F(p.AffixScale) / 100, nw = num.Length * fs * .58f;
            m.Scale(o.X, o.Y, 0, pop, () =>
            {
                m.Text(num, fs, o.X, o.Y, 0, fs * .2f, m.C(p.Color), 0, m.C(p.OutlineColor), c.F(p.OutlineWidth));
                if (!string.IsNullOrEmpty(p.Prefix)) m.Text(p.Prefix, af, o.X - nw / 2 - af * p.Prefix.Length * .6f, o.Y + fs * .2f, 0, af * .2f, m.C(p.Color), 0, m.C(p.OutlineColor), c.F(p.OutlineWidth) * .6f);
                if (!string.IsNullOrEmpty(p.Suffix)) m.Text(p.Suffix, af, o.X + nw / 2 + af * p.Suffix.Length * .6f, o.Y + fs * .2f, 0, af * .2f, m.C(p.Color), 0, m.C(p.OutlineColor), c.F(p.OutlineWidth) * .6f);
            });
        });
        S<PieChartParameter>(d, (p, c, m) => // 円グラフ: 項目ごとに高さの違う厚い扇形(ドーナツ)と、ラベル、まん中の文字
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float r = c.F(p.Radius), g = Ease(Math.Clamp(m.Now / MathF.Max(.05f, c.F(p.GrowTime)), 0, 1)), hole = p.Donut ? c.F(p.Hole) / 100 : 0, gap = c.F(p.Gap) * MathF.PI / 180;
            var vals = p.Values.Split(',', '、', '\n').Select(s => F(s.Trim())).ToList(); var labs = p.Labels.Split(',', '、', '\n').Select(s => s.Trim()).ToList(); float tot = vals.Where(v => v > 0).Sum(); if (tot <= 0) return;
            float a = -MathF.PI / 2 + c.F(p.StartAngle) * MathF.PI / 180;
            for (int i = 0; i < vals.Count; i++)
            {
                float sp = MathF.Max(0, vals[i]) / tot * MathF.Tau * g; if (sp <= gap) { a += sp; continue; } float mid = a + sp / 2, ex = p.Explode && i == 0 ? r * .08f : 0; var off = new Vector2(MathF.Cos(mid), MathF.Sin(mid)) * ex; var col = ChartCol(m, p.Palette, i);
                if (hole > 0) m.Tube(ArcPts3(o.X + off.X, o.Y + off.Y, r * (1 + hole) / 2, a + gap / 2, a + sp - gap / 2, 0, 30), r * (1 - hole) / 2, col); else m.Slab(-30 - i * 8, 60 + i * 16, col, PiePts(o.X + off.X, o.Y + off.Y, r, a + gap / 2, a + sp - gap / 2));
                if (p.LabelMode != PieLabelMode.None && g >= 1) { string s = p.LabelMode == PieLabelMode.Percent ? $"{vals[i] / tot * 100:0}%" : p.LabelMode == PieLabelMode.Name ? (i < labs.Count ? labs[i] : "") : $"{(i < labs.Count ? labs[i] : "")}\n{vals[i] / tot * 100:0}%"; float lr = hole > 0 ? r * (1 + hole) / 2 : r * .65f; m.Text(s, c.F(p.FontSize), o.X + MathF.Cos(mid) * lr, o.Y + MathF.Sin(mid) * lr, 80, 4, m.C(p.TextColor)); }
                a += sp;
            }
            if (hole > 0 && !string.IsNullOrEmpty(p.CenterText)) m.Text(p.CenterText, c.F(p.FontSize) * 1.2f, o.X, o.Y, 0, 6, m.C(p.TextColor), r * hole * 1.8f);
        });
        S<RelationshipParameter>(d, (p, c, m) => // 相関図: 名前の入った丸い玉の人物と、矢印つきの関係の管、関係の札
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); var ppl = p.People.Split(',', '、', '\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList(); int n = ppl.Count; if (n == 0) return;
            float nr = c.F(p.NodeRadius), spr = c.F(p.Spread), st = c.F(p.Stagger);
            var pos = new List<Vector3>(); for (int i = 0; i < n; i++) { Vector2 q = p.Layout switch { RelationLayout.Row => new(((i + .5f) / n - .5f) * spr * 2.6f, 0), RelationLayout.Hub => i == 0 ? Vector2.Zero : new(MathF.Cos((i - 1) * MathF.Tau / (n - 1) - MathF.PI / 2) * spr, MathF.Sin((i - 1) * MathF.Tau / (n - 1) - MathF.PI / 2) * spr), _ => new(MathF.Cos(i * MathF.Tau / n - MathF.PI / 2) * spr, MathF.Sin(i * MathF.Tau / n - MathF.PI / 2) * spr) }; pos.Add(V3(o.X + q.X, o.Y + q.Y, 0)); }
            foreach (Match lm in Regex.Matches(p.Links, @"(\d+)\s*(<>|>|<|-)\s*(\d+)\s*(?::([^\n]*))?"))
            {
                int a = int.Parse(lm.Groups[1].Value) - 1, b = int.Parse(lm.Groups[3].Value) - 1; if (a < 0 || b < 0 || a >= n || b >= n) continue; float k = Ease(Math.Clamp((m.Now - n * st) / .5f, 0, 1)); if (k <= 0) continue;
                var pa = pos[a]; var pb = pos[b]; var dd = Vector3.Normalize(pb - pa); var s0 = pa + dd * nr; var s1 = pb - dd * nr; m.Limb(s0, Vector3.Lerp(s0, s1, k), c.F(p.LineWidth), c.F(p.LineWidth), m.C(p.LineColor));
                string op = lm.Groups[2].Value; void Tip(Vector3 t, Vector3 dir) { var nn = new Vector2(-dir.Y, dir.X) * 20; m.Slab(-6, 12, m.C(p.LineColor), t.X, t.Y, t.X - dir.X * 40 + nn.X, t.Y - dir.Y * 40 + nn.Y, t.X - dir.X * 40 - nn.X, t.Y - dir.Y * 40 - nn.Y); }
                if (k >= 1) { if (op == ">" || op == "<>") Tip(s1, dd); if (op == "<" || op == "<>") Tip(s0, -dd); string lab = lm.Groups[4].Value.Trim(); if (lab.Length > 0) { var mid = (s0 + s1) / 2; m.Box(mid.X, mid.Y, 10, lab.Length * c.F(p.LabelSize) + 30, c.F(p.LabelSize) * 1.5f, 16, m.C(p.LabelBg), .4f); m.Text(lab, c.F(p.LabelSize), mid.X, mid.Y, 20, 3, m.C(p.TextColor)); } }
            }
            for (int i = 0; i < n; i++) { float k = Ease(Math.Clamp((m.Now - i * st) / .4f, 0, 1)); if (k <= 0) continue; var q = pos[i]; m.Scale(q.X, q.Y, 0, k, () => { m.Ball(q.X, q.Y, 0, nr, nr, nr * .4f, ChartCol(m, p.Palette, i)); m.Text(ppl[i], c.F(p.FontSize), q.X, q.Y, nr * .4f, 4, m.C(p.TextColor), nr * 1.8f); }); }
        });
        S<ChoiceWindowParameter>(d, (p, c, m) => // 選択肢ウィンドウ: 枠のある厚い窓と並んだ選択肢、選んだ行の三角のカーソル
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float k = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)); if (k <= 0) return; UseFont(m, p.Font);
            var ch = p.Choices.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList(); if (ch.Count == 0) return; float fs = c.F(p.FontSize), rh = fs * 1.6f, mw = ch.Max(s => s.Length) * fs + fs * 2.4f;
            float w = p.Horizontal ? mw * ch.Count : mw, h = p.Horizontal ? rh + 40 : rh * ch.Count + 40; int sel = Math.Clamp((int)c.F(p.Selected) - 1, 0, ch.Count - 1); bool dec = c.F(p.DecideTime) > 0 && m.Now > c.F(p.DecideTime);
            var bg = p.Style switch { GameWindowStyle.Rpg => m.C(0x0D1B4Cu, .92f), GameWindowStyle.Retro => m.C(0x000000u), GameWindowStyle.Soft => m.C(0xFFF8F0u, .95f), _ => m.C(0x1C1F26u, .9f) }; var tc = p.Style == GameWindowStyle.Soft ? m.C(0x4E342Eu) : W;
            m.Scale(o.X, o.Y, 0, k, () =>
            {
                m.Box(o.X, o.Y, -10, w, h, 30, bg, p.Style == GameWindowStyle.Soft ? .3f : .08f); RectBars(m, o.X, o.Y, 8, w - 14, h - 14, 6, p.Style == GameWindowStyle.Retro ? W : m.C(p.Accent));
                for (int i = 0; i < ch.Count; i++) { float x = p.Horizontal ? o.X - w / 2 + mw * (i + .5f) : o.X + fs * .5f, y = p.Horizontal ? o.Y : o.Y - h / 2 + 20 + rh * (i + .5f); bool on = i == sel; if (on) { m.Box(x - (p.Horizontal ? 0 : fs * .5f), y, 4, mw - 20, rh * .9f, 10, m.C(p.Accent, dec && MathF.Sin(m.Now * 30) > 0 ? .6f : .25f), .3f); float cx = x - (p.Horizontal ? mw / 2 - fs * .6f : mw / 2 - fs * .2f) + MathF.Sin(m.Now * 8) * 6; m.Slab(10, 14, m.C(p.Accent), cx - fs * .3f, y - fs * .3f, cx + fs * .2f, y, cx - fs * .3f, y + fs * .3f); } m.Text(ch[i], fs, x, y, 18, 3, on ? m.C(p.Accent) : tc); }
            });
        });
        S<NovelWindowParameter>(d, (p, c, m) => // 会話ウィンドウ: 画面下の厚い窓、名前の札、1文字ずつ出るセリフ、跳ねる次へのしるし
        {
            float k = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)); if (k <= 0) return; UseFont(m, p.Font);
            float bh = c.F(p.BoxHeight), mg = c.F(p.Margin), by = (c.F(p.BottomY) / 100 - .5f) * m.H - bh / 2, w = m.W - mg * 2; string s = TextKit.Lines(p.Text); int typed = c.F(p.TypeSpeed) > 0 ? (int)(m.Now * c.F(p.TypeSpeed)) : s.Length;
            var bg = p.Style switch { GameWindowStyle.Rpg => m.C(0x0D1B4Cu, .9f), GameWindowStyle.Retro => m.C(0x000000u), GameWindowStyle.Soft => m.C(0xFFF8F0u, .95f), _ => m.C(0x15171Eu, .85f) }; var tc = p.Style == GameWindowStyle.Soft ? m.C(0x4E342Eu) : W;
            m.Move(0, (1 - k) * 200, () =>
            {
                m.Box(0, by, -10, w, bh, 30, bg, p.Style == GameWindowStyle.Soft ? .25f : .06f); if (p.Style != GameWindowStyle.Modern) RectBars(m, 0, by, 8, w - 14, bh - 14, 5, p.Style == GameWindowStyle.Soft ? m.C(p.Accent) : W); else m.Box(0, by - bh / 2 + 4, 8, w, 8, 10, m.C(p.Accent), .3f);
                if (!string.IsNullOrEmpty(p.Speaker)) { float nw = p.Speaker.Length * c.F(p.NameSize) + 60; m.Box(-w / 2 + nw / 2 + 30, by - bh / 2 - c.F(p.NameSize) * .4f, 10, nw, c.F(p.NameSize) * 1.5f, 30, m.C(p.Accent), .3f); m.Text(p.Speaker, c.F(p.NameSize), -w / 2 + nw / 2 + 30, by - bh / 2 - c.F(p.NameSize) * .4f, 26, 3, W); }
                m.Text(s[..Math.Min(typed, s.Length)], c.F(p.FontSize), 0, by + 10, 10, 3, tc, w - 120);
                if (p.NextMark && typed >= s.Length) { float y = by + bh / 2 - 40 + MathF.Abs(MathF.Sin(m.Now * 4)) * -12; m.Slab(10, 16, m.C(p.Accent), w / 2 - 70, y - 14, w / 2 - 40, y - 14, w / 2 - 55, y + 10); }
            });
        });
        S<BossIntroParameter>(d, (p, c, m) => // ボス登場: 暗くなった画面に斜めの赤い帯が走り込み、厚い大きな名前と肩書き
        {
            float k = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)); if (k <= 0) return; UseFont(m, p.Font); float y = (c.F(p.PosY) / 100 - .5f) * m.H, bh = c.F(p.BandHeight), fs = c.F(p.FontSize);
            m.Box(0, 0, -300, m.W * 2, m.H * 2, 10, m.C(0x000000u, c.F(p.Dim) / 100 * k), .01f);
            if (p.Flash && m.Now < .2f) m.Box(0, 0, 100, m.W * 2, m.H * 2, 4, m.C(0xFFFFFFu, 1 - m.Now / .2f), .01f);
            m.Turn(0, y, c.F(p.Angle) * MathF.PI / 180, () =>
            {
                m.Box(-m.W * (1 - k), y, 0, m.W * 1.4f, bh, 50, m.C(p.BandColor), .05f); foreach (int s in Sides) m.Box(-m.W * (1 - k), y + s * (bh / 2 + 10), 0, m.W * 1.4f, 8, 50, m.C(p.TextColor, .8f), .2f);
                m.Text(p.Title, fs, m.W * (1 - k), y + 10, 30, fs * .2f, m.C(p.TextColor), 0, K, 6);
                m.Text(p.SubTitle, fs * .35f, m.W * (1 - k) - fs * 1.5f, y - bh / 2 + fs * .25f, 30, 6, m.C(p.TextColor));
            });
        });
        S<AchievementParameter>(d, (p, c, m) => // 実績解除: 画面の隅に滑り込む厚いカードと、光る金のトロフィー・星・メダルのアイコン
        {
            float k = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)); if (k <= 0) return; UseFont(m, p.Font); float sc = c.F(p.Scale) / 100, w = 760 * sc, h = 170 * sc, mg = c.F(p.EdgeMargin);
            bool right = p.Corner is ScreenCorner.TopRight or ScreenCorner.BottomRight, left = p.Corner is ScreenCorner.TopLeft or ScreenCorner.BottomLeft, top = p.Corner is ScreenCorner.TopRight or ScreenCorner.TopLeft or ScreenCorner.TopCenter;
            float x = right ? m.W / 2 - mg - w / 2 : left ? -m.W / 2 + mg + w / 2 : 0, y = top ? -m.H / 2 + mg + h / 2 : m.H / 2 - mg - h / 2; float dx = (right ? 1 : left ? -1 : 0) * (1 - k) * (w + mg), dy = (right || left) ? 0 : (top ? -1 : 1) * (1 - k) * (h + mg);
            var ac = m.C(p.Accent);
            m.Move(dx, dy, () =>
            {
                m.Box(x, y, 0, w, h, 30, m.C(p.CardColor), .25f); m.Box(x - w / 2 + 6, y, 10, 12, h - 20, 20, ac, .4f);
                float ix = x - w / 2 + h * .6f, sp = m.Now * 2;
                m.Yaw(ix, y, 20, MathF.Sin(sp) * .5f, () =>
                {
                    switch (p.Icon)
                    {
                        case AchievementIcon.Star: m.Slab(10, 30, ac, StarPts(ix, y, h * .38f, 5, .45f, 0)); break;
                        case AchievementIcon.Medal: m.Disc(ix, y, 20, h * .32f, h * .32f, 16, ac, .5f); break;
                        case AchievementIcon.Check: m.Disc(ix, y, 20, h * .34f, h * .34f, 16, m.C(0x4CAF50u), .5f); m.Tube(new List<Vector3> { V3(ix - h * .15f, y, 32), V3(ix - h * .03f, y + h * .12f, 32), V3(ix + h * .17f, y - h * .13f, 32) }, 6, W); break;
                        case AchievementIcon.Crown: m.Slab(10, 30, ac, ix - h * .3f, y + h * .2f, ix - h * .33f, y - h * .2f, ix - h * .15f, y, ix, y - h * .28f, ix + h * .15f, y, ix + h * .33f, y - h * .2f, ix + h * .3f, y + h * .2f); break;
                        default: m.Lathe(ix, 20, ac, h * .28f, y - h * .3f, h * .22f, y, h * .05f, y + h * .1f, h * .05f, y + h * .2f, h * .18f, y + h * .25f, h * .18f, y + h * .32f); foreach (int s in Sides) m.Torus(ix + s * h * .28f, y - h * .15f, 20, h * .09f, 4, ac); break;
                    }
                });
                m.Text(p.Title, h * .2f, x + h * .4f, y - h * .2f, 20, 3, ac, w - h * 1.4f); m.Text(p.Detail, h * .26f, x + h * .4f, y + h * .15f, 20, 3, W, w - h * 1.4f);
            });
        });
        S<InputDisplayParameter>(d, (p, c, m) => // 入力表示: コントローラー・キー・矢印キー・マウスの立体の部品、押されたボタンは沈んで光る
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size) / 100; var bc = m.C(p.BaseColor, c.F(p.Opacity) / 100); var pc = m.C(p.PressColor);
            var pressed = new HashSet<string>(); if (p.Auto) { string[] seq = { "A", "→", "B", "↑", "←" }; pressed.Add(seq[(int)(m.Now * 2) % seq.Length]); }
            else { string cur = ""; foreach (var line in p.Pressed.Split('\n')) { var kv = line.Split(':'); if (kv.Length == 2 && F(kv[0], 1e9f) <= m.Now) cur = kv[1]; } foreach (var k2 in cur.Split(',')) if (k2.Trim().Length > 0) pressed.Add(k2.Trim()); }
            void Key(string name, float x, float y, float w, float h, bool round = false) { bool on = pressed.Contains(name); float z = on ? -6 : 6; if (round) m.Disc(o.X + x * s, o.Y + y * s, z, w * s / 2, w * s / 2, 20 * s, on ? pc : m.C(0x4A4F5Cu), .5f); else m.Box(o.X + x * s, o.Y + y * s, z, w * s, h * s, 24 * s, on ? pc : m.C(0x4A4F5Cu), .25f); m.Text(name, MathF.Min(w, h) * .5f * s, o.X + x * s, o.Y + y * s, z + 14 * s, 2, W); }
            switch (p.Device)
            {
                case InputDevice.Keys: m.Box(o.X, o.Y, -20, 520 * s, 200 * s, 20 * s, bc, .15f); string[] ks = { "Z", "X", "C", "V" }; for (int i = 0; i < 4; i++) Key(ks[i], -180 + i * 120, 0, 100, 100); break;
                case InputDevice.Arrows: m.Box(o.X, o.Y, -20, 400 * s, 280 * s, 20 * s, bc, .15f); Key("↑", 0, -60, 100, 100); Key("←", -115, 60, 100, 100); Key("↓", 0, 60, 100, 100); Key("→", 115, 60, 100, 100); break;
                case InputDevice.Mouse: m.Ball(o.X, o.Y, 0, 110 * s, 170 * s, 60 * s, bc); foreach (int sd in Sides) { bool on = pressed.Contains(sd < 0 ? "L" : "R"); m.Ball(o.X + sd * 50 * s, o.Y - 90 * s, 40 * s, 48 * s, 60 * s, 14 * s, on ? pc : m.C(0x4A4F5Cu)); } m.Cyl(o.X, o.Y - 100 * s, 55 * s, 10 * s, 40 * s, m.C(0x9E9E9Eu)); break;
                default:
                    foreach (int sd in Sides) m.Ball(o.X + sd * 180 * s, o.Y + 40 * s, 0, 130 * s, 120 * s, 50 * s, bc); m.Box(o.X, o.Y, 0, 420 * s, 180 * s, 70 * s, bc, .45f);
                    Key("↑", -180, -45, 50, 50); Key("↓", -180, 45, 50, 50); Key("←", -225, 0, 50, 50); Key("→", -135, 0, 50, 50);
                    Key("Y", 180, -55, 56, 56, true); Key("A", 180, 55, 56, 56, true); Key("X", 125, 0, 56, 56, true); Key("B", 235, 0, 56, 56, true); break;
            }
        });
        S<HeartsRiseParameter>(d, (p, c, m) => // いいね: 下の口から次々わいて、揺れながら上る厚いハート・グッド・星
        {
            var o = PctPos(m, c.F(p.SourceX), c.F(p.SourceY)); float rate = MathF.Max(.5f, c.F(p.Rate)), life = MathF.Max(.3f, c.F(p.Life)), sz = c.F(p.Size) * 1.4f; int n = (int)(rate * life) + 1;
            for (int j = 0; j < n; j++)
            {
                int idx = (int)(m.Now * rate) - j; if (idx < 0) continue; float age = m.Now - idx / rate, f = age / life; if (f > 1) continue;
                float x = o.X + MathF.Sin(age * 3 + idx) * c.F(p.Sway) + (R(p.Seed, idx) - .5f) * 60, y = o.Y - f * c.F(p.Rise), z = (R(p.Seed, idx, 1) - .5f) * 200, pop = MathF.Min(1, age * 6), al = 1 - MathF.Max(0, f - .7f) / .3f;
                var col = p.ColorMode == ReactionColor.Rainbow ? Pal(m, idx) : p.ColorMode == ReactionColor.Custom ? m.C(p.Color) : m.C(0xFF4D8Du); col = m.C(col, al);
                int kind = p.Kind == ReactionKind.Mixed ? idx % 3 : (int)p.Kind; float s = sz * pop;
                if (kind == 0) Heart3D(m, x, y, z, s, col); else if (kind == 2) m.Slab(z - s * .2f, s * .4f, m.C(0xFFD54Fu, al), StarPts(x, y, s * .6f, 5, .45f, 0)); else { m.Box(x - s * .2f, y + s * .1f, z, s * .3f, s * .5f, s * .3f, m.C(0x42A5F5u, al), .3f); m.Box(x + s * .12f, y + s * .05f, z, s * .45f, s * .55f, s * .35f, m.C(0x42A5F5u, al), .4f); m.Box(x + s * .05f, y - s * .35f, z, s * .2f, s * .4f, s * .3f, m.C(0x42A5F5u, al), .5f); }
                if (p.Outline && kind == 0) Heart3D(m, x, y, z - 6, s * 1.15f, m.C(0xFFFFFFu, al));
            }
        });
        S<SubscribeButtonParameter>(d, (p, c, m) => // チャンネル登録: 厚い赤いボタンと矢印のカーソル、押されると灰色の登録済みに変わり、グッドとベルが光る
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float sc = c.F(p.Scale) / 100, ct = c.F(p.ClickTime), press = MathF.Max(0, 1 - MathF.Abs(m.Now - ct) * 8); bool done = m.Now > ct;
            m.Scale(o.X, o.Y, 0, sc, () =>
            {
                string lab = done ? p.DoneLabel : p.Label; float w = lab.Length * 52 + 120;
                m.Box(o.X, o.Y, -press * 14, w, 100, 40, done ? m.C(p.DoneColor) : m.C(p.ButtonColor), .4f); m.Text(lab, 50, o.X, o.Y, 22 - press * 14, 3, W);
                if (p.ShowLike) { float lx = o.X - w / 2 - 110; m.Disc(lx, o.Y, 0, 50, 50, 30, done ? m.C(0x42A5F5u) : m.C(0x3A3F4Au), .5f); m.Text("👍", 50, lx, o.Y, 18, 2, W); }
                if (p.ShowBell) { float bx = o.X + w / 2 + 110, sw = done ? MathF.Sin(m.Now * 20) * .3f * MathF.Max(0, 1 - (m.Now - ct)) : 0; m.Turn(bx, o.Y - 40, sw, () => { m.Lathe(bx, 0, done ? m.C(0xFFD54Fu) : m.C(0xB0BEC5u), 10, o.Y - 45, 30, o.Y - 20, 40, o.Y + 25, 45, o.Y + 30); m.Ball(bx, o.Y + 38, 0, 10, done ? m.C(0xFFD54Fu) : m.C(0xB0BEC5u)); }); }
                if (p.ShowCursor && m.Now < ct + .6f) { float k = Ease(Math.Clamp(m.Now / MathF.Max(.1f, ct), 0, 1)); float cx = o.X + 300 * (1 - k) + 40, cy = o.Y + 250 * (1 - k) + 20; m.Slab(60, 16, W, cx, cy, cx, cy + 70, cx + 18, cy + 55, cx + 32, cy + 82, cx + 44, cy + 76, cx + 30, cy + 50, cx + 52, cy + 48); }
            });
        });
        S<CommentFlowParameter>(d, (p, c, m) => // コメントが流れる: 画面の奥行きの中を右から左へ流れる、ふち取りのある厚い文字
        {
            var list = p.Comments.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList(); if (list.Count == 0) return; UseFont(m, p.Font);
            float rate = MathF.Max(.2f, c.F(p.Rate)), cross = MathF.Max(.5f, c.F(p.CrossTime)), fs = c.F(p.FontSize), top = (c.F(p.AreaTop) / 100 - .5f) * m.H, bot = (c.F(p.AreaBottom) / 100 - .5f) * m.H; int n = (int)(rate * cross) + 2;
            for (int j = 0; j < n; j++)
            {
                int idx = (int)(m.Now * rate) - j; if (idx < 0) continue; float age = m.Now - idx / rate, f = age / cross; if (f > 1.3f) continue;
                string s = p.InOrder ? list[idx % list.Count] : list[(int)(R(p.Seed, idx) * list.Count) % list.Count]; float w = s.Length * fs, x = m.W / 2 + w / 2 - f * (m.W + w), y = top + fs + R(p.Seed, idx, 1) * MathF.Max(1, bot - top - fs * 2), z = (R(p.Seed, idx, 2) - .5f) * 300;
                var col = R(p.Seed, idx, 3) < c.F(p.ColorRatio) / 100 ? Pal(m, idx) : m.C(p.Color); m.Text(s, fs, x, y, z, fs * .12f, m.C(col, c.F(p.Opacity) / 100), 0, m.C(0x000000u, c.F(p.Opacity) / 100), c.F(p.OutlineWidth));
            }
        });
        S<PhoneNotifyParameter>(d, (p, c, m) => // 通知バナー・スマホ: 上から下りる角丸の通知の板/スマホ本体と、順に出るチャットの吹き出し
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); UseFont(m, p.Font); float sc = c.F(p.Scale) / 100; var bg = p.Dark ? m.C(0x1C1C1Eu, .95f) : m.C(0xF7F7F9u, .95f); var tc = p.Dark ? W : m.C(0x111111u);
            if (p.Kind == PhoneNotifyKind.Banner)
            {
                float k = Ease(Math.Clamp(m.Now / .4f, 0, 1)), w = 900 * sc, h = 190 * sc, y = o.Y + h / 2 - (1 - k) * (h + 200);
                m.Box(o.X, y, 0, w, h, 30, bg, .35f); m.Box(o.X - w / 2 + 70 * sc, y - 30 * sc, 18, 70 * sc, 70 * sc, 20, m.C(p.Accent), .35f);
                m.Text(p.AppName, 30 * sc, o.X - w / 2 + 220 * sc, y - 50 * sc, 18, 2, m.C(tc, .6f)); m.Text(p.Sender, 40 * sc, o.X - w / 2 + 230 * sc, y, 18, 3, tc); m.Text(p.Body, 36 * sc, o.X + 40 * sc, y + 50 * sc, 18, 2, tc, w - 160 * sc);
                return;
            }
            var msgs = p.Chat.Split('\n').Where(s => s.Trim().Length > 0).ToList(); float iv = MathF.Max(.2f, c.F(p.ChatInterval)); int shown = Math.Min(msgs.Count, (int)(m.Now / iv) + 1);
            float pw = 520 * sc, ph = 1000 * sc, cx = p.Kind == PhoneNotifyKind.Phone ? 0 : o.X, cy = 0;
            if (p.Kind == PhoneNotifyKind.Phone) { m.Box(cx, cy, -30, pw + 40, ph + 40, 50, m.C(0x111111u), .2f); m.Box(cx, cy, 0, pw, ph, 8, bg, .1f); m.Box(cx, cy - ph / 2 + 60 * sc, 8, pw, 100 * sc, 10, m.C(p.Accent), .05f); m.Text(p.Sender, 38 * sc, cx, cy - ph / 2 + 60 * sc, 16, 2, W); }
            for (int i = 0; i < shown; i++) { string s = msgs[i].Trim(); bool me = s.StartsWith(">"); if (me) s = s[1..]; float y = cy - ph / 2 + 170 * sc + i * 110 * sc, w = s.Length * 36 * sc + 60 * sc, x = cx + (me ? pw / 2 - w / 2 - 30 * sc : -pw / 2 + w / 2 + 30 * sc), pop = Ease(Math.Clamp((m.Now - i * iv) / .25f, 0, 1)); m.Scale(x, y, 0, pop, () => { m.Box(x, y, 14, w, 80 * sc, 20, me ? m.C(p.Accent) : m.C(0xE5E5EAu), .45f); m.Text(s, 34 * sc, x, y, 26, 2, me ? W : m.C(0x111111u), w); }); }
            if (p.Typing && shown < msgs.Count) { float y = cy - ph / 2 + 170 * sc + shown * 110 * sc; for (int i = 0; i < 3; i++) m.Ball(cx - pw / 2 + 60 * sc + i * 30 * sc, y - MathF.Max(0, MathF.Sin(m.Now * 8 - i)) * 10, 20, 10 * sc, m.C(0x8E8E93u)); }
        });
        S<MangaMarkParameter>(d, (p, c, m) => // 漫符: 汗のしずく・怒りマーク・はてな・びっくり・ハート・ため息・音符・電球など、厚い立体の印
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float s = c.F(p.Size), k = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)), an = p.Animate ? MathF.Sin(m.Now * 6) : 0; if (k <= 0) return;
            uint def = p.Kind switch { MangaMarkKind.Anger => 0xE53935u, MangaMarkKind.Heart or MangaMarkKind.Heartbeat => 0xFF4D8Du, MangaMarkKind.Idea => 0xFFD54Fu, MangaMarkKind.Question or MangaMarkKind.Exclamation => 0xFFB300u, MangaMarkKind.Sigh or MangaMarkKind.Steam => 0xE0E0E0u, _ => 0x4FA8FFu };
            var col = p.CustomColor ? m.C(p.Color) : m.C(def); var oc = m.C(p.OutlineColor);
            m.Scale(o.X, o.Y, 0, k, () => m.Turn(o.X, o.Y, c.F(p.Rotation) * MathF.PI / 180, () => m.Zoom(o.X, o.Y, p.Mirror ? -1 : 1, 1, () =>
            {
                void Drop(float x, float y, float r) => m.Lathe(x, 0, col, 0, y - r * 1.4f, r * .5f, y - r * .4f, r, y + r * .3f, r * .6f, y + r * .9f, 0, y + r);
                switch (p.Kind)
                {
                    case MangaMarkKind.Sweat: Drop(o.X, o.Y + an * 8, s * .35f); break;
                    case MangaMarkKind.SweatMany: for (int i = 0; i < 3; i++) Drop(o.X + (i - 1) * s * .45f, o.Y + (i % 2) * s * .3f + Frac(m.Now + i * .3f) * 20, s * .2f); break;
                    case MangaMarkKind.Anger: { float pu = 1 + .1f * an; for (int i = 0; i < 4; i++) { float a = i * MathF.PI / 2 + MathF.PI / 4; m.Turn(o.X, o.Y, a, () => m.Tube(ArcPts3(o.X + s * .42f * pu, o.Y, s * .25f * pu, MathF.PI * .6f, MathF.PI * 1.4f, 0, 10), s * .07f, col)); } } break;
                    case MangaMarkKind.Question: m.Tube(ArcPts3(o.X, o.Y - s * .2f, s * .25f, MathF.PI * 1.1f, MathF.PI * 2.4f, 0, 14).Append(V3(o.X, o.Y + s * .15f)).ToList(), s * .09f, col); m.Ball(o.X, o.Y + s * .4f, 0, s * .1f, col); break;
                    case MangaMarkKind.Exclamation: m.Lathe(o.X, 0, col, s * .14f, o.Y - s * .5f, s * .06f, o.Y + s * .15f); m.Ball(o.X, o.Y + s * .38f, 0, s * .1f, col); break;
                    case MangaMarkKind.Heart: case MangaMarkKind.Heartbeat: Heart3D(m, o.X, o.Y, 0, s * .5f * (1 + (p.Kind == MangaMarkKind.Heartbeat ? .15f * MathF.Max(0, an) : 0)), col); break;
                    case MangaMarkKind.Sigh: case MangaMarkKind.Steam: for (int i = 0; i < 3; i++) { float f = Frac(m.Now * .6f + i / 3f); m.Ball(o.X + f * s * .5f, o.Y - f * s * .4f, 0, s * (.15f + .15f * f), m.C(col, 1 - f)); } break;
                    case MangaMarkKind.Notes: for (int i = 0; i < 2; i++) { float x = o.X + (i - .5f) * s * .5f, y = o.Y + (i % 2) * s * .15f + MathF.Sin(m.Now * 4 + i) * 10; m.Ball(x, y + s * .25f, 0, s * .12f, s * .09f, s * .08f, col); m.Limb(x + s * .1f, y + s * .25f, 0, x + s * .1f, y - s * .25f, 0, s * .025f, s * .025f, col); m.Limb(x + s * .1f, y - s * .25f, 0, x + s * .25f, y - s * .15f, 0, s * .03f, s * .02f, col); } break;
                    case MangaMarkKind.Sparkle: for (int i = 0; i < 3; i++) m.Slab(-4, 8, m.C(0xFFE082u), StarPts(o.X + (i - 1) * s * .4f, o.Y + (i % 2) * s * .25f, s * .2f * (.6f + .4f * MathF.Abs(MathF.Sin(m.Now * 4 + i))), 4, .25f, 0)); break;
                    case MangaMarkKind.Dots: for (int i = 0; i < 3; i++) m.Ball(o.X + (i - 1) * s * .3f, o.Y, 0, s * .08f * (m.Now * 3 % 3 >= i ? 1 : .3f), oc); break;
                    case MangaMarkKind.Idea: m.Ball(o.X, o.Y - s * .1f, 0, s * .3f, col); m.Cyl(o.X, o.Y + s * .27f, 0, s * .13f, s * .15f, m.C(0x9E9E9Eu)); for (int i = 0; i < 7; i++) { float a = -MathF.PI + i * MathF.PI / 6; m.Limb(o.X + MathF.Cos(a) * s * .42f, o.Y - s * .1f + MathF.Sin(a) * s * .42f, 0, o.X + MathF.Cos(a) * s * .55f, o.Y - s * .1f + MathF.Sin(a) * s * .55f, 0, s * .025f, s * .025f, col); } break;
                    default: m.Text("Z", s * .4f, o.X, o.Y, 0, 6, col); m.Text("z", s * .25f, o.X + s * .3f, o.Y - s * .3f, 0, 4, col); break;
                }
            })));
        });
        S<GloomParameter>(d, (p, c, m) => // ガーン・ずーん: 頭上の暗い雲、上から垂れ下がる縦線のすだれ、しとしと降る雨
        {
            var o = PctPos(m, c.F(p.PosX), c.F(p.PosY)); float k = InOutK(m, c, c.F(p.InTime), c.F(p.OutTime)), aw = c.F(p.AreaWidth), ah = c.F(p.AreaHeight), st = c.F(p.Strength) / 100; if (k <= 0) return; var col = m.C(p.Color, st);
            if (p.Kind != GloomKind.Lines) { for (int i = 0; i < 6; i++) m.Ball(o.X + (i - 2.5f) * aw * .17f, o.Y - ah * .5f + MathF.Sin(i * 2) * 20, 0, aw * .16f, aw * .12f, aw * .12f, col); }
            if (p.Kind != GloomKind.Cloud) for (int i = 0; i < Math.Clamp(p.LineCount, 1, 40); i++) { float x = o.X - aw / 2 + aw * (i + .5f) / p.LineCount, L = ah * k * (.6f + .4f * R(p.Seed, i)); m.Limb(x, o.Y - ah * .45f, 30, x, o.Y - ah * .45f + L, 30, 4, 2, col); }
            if (p.Rain) for (int i = 0; i < 16; i++) { float y = Cyc(m.Now * 500 + R(p.Seed, i, 2) * ah, ah); m.Limb(o.X + (R(p.Seed, i, 3) - .5f) * aw, o.Y - ah * .3f + y, 40, o.X + (R(p.Seed, i, 3) - .5f) * aw, o.Y - ah * .3f + y + 30, 40, 3, 2, m.C(0x90CAF9u, .6f * k)); }
        });
        S<WindowDropsParameter>(d, (p, c, m) => // 水滴が窓を伝う: ガラスの板に付いた丸い水滴と、伝って落ちる大きなしずく、その跡
        {
            var tint = m.C(p.Tint, c.F(p.Opacity) / 100); m.Box(0, 0, -20, m.W, m.H, 6, m.C(p.Tint, .08f), .01f);
            Low(m, () => { for (int i = 0; i < Math.Min(p.DotCount, 300); i++) { float s = c.F(p.DropSize) * (.2f + .4f * R(p.Seed, i, 2)); m.Ball((R(p.Seed, i) - .5f) * m.W, (R(p.Seed, i, 1) - .5f) * m.H, 0, s, s * 1.1f, s * .5f, m.C(tint, .7f)); } }, 6);
            for (int i = 0; i < Math.Min(p.RunCount, 40); i++)
            {
                float x0 = (R(p.Seed, i, 3) - .5f) * m.W, sp = c.F(p.Speed) * (.5f + R(p.Seed, i, 4)), y = -m.H / 2 + Cyc(m.Now * sp * (1 + MathF.Sin(m.Now * 2 + i) * .5f) + R(p.Seed, i, 5) * m.H, m.H * 1.2f), wob = MathF.Sin(y / 60 + i) * 8, s = c.F(p.DropSize) * (.8f + .5f * R(p.Seed, i, 6));
                m.Lathe(x0 + wob, 0, m.C(tint, .85f), 0, y - s * 1.5f, s * .7f, y - s * .2f, s, y + s * .4f, 0, y + s);
                if (p.Trail) { var tr = new List<Vector3>(); for (int k = 0; k <= 8; k++) { float ty = y - k * 30; tr.Add(V3(x0 + MathF.Sin(ty / 60 + i) * 8, ty, -2)); } m.Tube(tr, s * .2f, m.C(tint, .4f)); }
            }
        });
        S<KomorebiParameter>(d, (p, c, m) => // 木漏れ日: 斜めに差し込む光の柱と、揺れる葉の影の床、光の斑点
        {
            float it = c.F(p.Intensity) / 100, ang = c.F(p.Angle) * MathF.PI / 180, sx = (c.F(p.SourceX) / 100 - .5f) * m.W, sw = c.F(p.SwaySpeed) / 100;
            for (int i = 0; i < Math.Clamp(p.RayCount, 1, 16); i++) { float x = sx + (i - p.RayCount / 2f) * 150, w = 40 + 60 * R(p.Seed, i), flick = .7f + .3f * MathF.Sin(m.Now * 1.5f * sw + i); m.Limb(x, -m.H * .7f, -400, x - MathF.Tan(ang) * m.H * 1.5f, m.H * .7f, 100, w * .5f, w * 1.3f, m.C(p.LightColor, .14f * it * flick)); }
            if (p.Leaves) m.Pitch(0, m.H / 2, -300, MathF.PI / 2 - .3f, () => { for (int i = 0; i < Math.Min(p.SpotCount, 60); i++) { float x = (R(p.Seed, i, 1) - .5f) * m.W * 1.5f + MathF.Sin(m.Now * sw + i) * 20, y = m.H / 2 + (R(p.Seed, i, 2) - .5f) * 800, s = 30 + 60 * R(p.Seed, i, 3); m.Disc(x, y, -300, s, s * .7f, 2, R(p.Seed, i, 4) < .5f ? m.C(p.LightColor, .5f * it) : m.C(0x000000u, c.F(p.ShadowStrength) / 100)); } });
            Low(m, () => { for (int i = 0; i < 30; i++) { float y = Cyc(m.Now * 30 + R(p.Seed, i, 5) * m.H, m.H) - m.H / 2; m.Ball(sx + (R(p.Seed, i, 6) - .5f) * 600 - MathF.Tan(ang) * (y + m.H / 2), y, -200, 3, m.C(p.LightColor, .7f)); } }, 5);
        });
        S<GrassParameter>(d, (p, c, m) => // 草原: 奥まで生えそろった細い草の葉が、風の波でいっせいになびく
        {
            float by = (c.F(p.BaseY) / 100 - .5f) * m.H, gh = c.F(p.GrassHeight), wind = c.F(p.Wind) / 100, ws = c.F(p.WaveSpeed) / 100;
            m.Box(0, by + 10, -700, m.W * 3, 20, 2000, m.C(p.Silhouette ? 0x000000u : 0x2E4A1Eu), .01f);
            Low(m, () =>
            {
                for (int i = 0; i < Math.Min(p.Density, 500); i++)
                {
                    float z = -1600 + R(p.Seed, i, 2) * 1700, x = (R(p.Seed, i) - .5f) * m.W * (1.2f + -z / 1200), h = gh * (.6f + .6f * R(p.Seed, i, 1)), bend = (MathF.Sin(x / 300 - m.Now * 2 * ws) * .5f + .5f) * wind * .7f;
                    var tip = V3(x + MathF.Sin(bend) * h, by - MathF.Cos(bend) * h, z); var mid = V3(x + MathF.Sin(bend * .4f) * h * .5f, by - h * .5f, z);
                    var root = p.Silhouette ? m.C(0x000000u) : m.C(p.RootColor); var top = p.Silhouette ? m.C(0x000000u) : m.C(p.TipColor);
                    m.Limb(V3(x, by, z), mid, 5, 3.5f, root); m.Limb(mid, tip, 3.5f, .8f, Sculpt.Mix(root, top, .7f));
                    if (p.Flowers && i % 15 == 0) m.Ball(tip.X, tip.Y, tip.Z, 10, Pal(m, i));
                }
            }, 4);
        });
        S<DustParameter>(d, (p, c, m) => // ほこり: 斜めに差す光の柱の中で、きらきら漂う小さな粒
        {
            float bx = (c.F(p.BeamX) / 100 - .5f) * m.W, ang = c.F(p.BeamAngle) * MathF.PI / 180, bw = c.F(p.BeamWidth), br = c.F(p.Brightness) / 100;
            if (p.Beam) m.Limb(bx, -m.H * .7f, -300, bx - MathF.Tan(ang) * m.H * 1.4f, m.H * .7f, 0, bw * .3f, bw * .7f, m.C(p.Color, .15f * br));
            Low(m, () =>
            {
                for (int i = 0; i < Math.Min(p.Count, 220); i++)
                {
                    bool inB = R(p.Seed, i, 9) > c.F(p.Outside) / 100; float f = R(p.Seed, i), y = -m.H / 2 + f * m.H + MathF.Sin(m.Now * .5f * c.F(p.Drift) / 100 + i) * 30, cx = inB ? bx - MathF.Tan(ang) * (y + m.H * .7f) : 0, x = cx + (R(p.Seed, i, 1) - .5f) * (inB ? bw * .8f : m.W) + MathF.Cos(m.Now * .4f + i) * 20, z = -300 + (R(p.Seed, i, 2) - .5f) * 400;
                    float tw = .5f + .5f * MathF.Sin(m.Now * 3 + i); m.Ball(x, y, z, c.F(p.ParticleSize) * (inB ? 1.2f : .8f), m.C(p.Color, (inB ? 1 : .3f) * tw * br));
                }
            }, 5);
        });
        S<VhsParameter>(d, (p, c, m) => // VHS風: 画面の前の▶PLAYなどの角ばった表示、日付とカウンター、走る乱れの帯と走査線
        {
            float fs = c.F(p.FontSize); var tc = m.C(p.TextColor);
            for (int i = 0; i < 40; i++) m.Box(0, -m.H / 2 + i * m.H / 40, 40, m.W, 3, 2, m.C(0x000000u, c.F(p.Scanlines) / 100 * .5f));
            float ry = Cyc(m.Now * 120, m.H * 1.4f) - m.H * .7f; m.Box(MathF.Sin(m.Now * 30) * c.F(p.Tracking) * .3f, ry, 30, m.W * 1.1f, 60 * c.F(p.RollBand) / 30, 10, m.C(0xFFFFFFu, .08f), .01f);
            Low(m, () => { int fr = (int)(m.Now * 30); for (int i = 0; i < (int)(c.F(p.Speckle) * 1.5f); i++) m.Box((R(fr, i) - .5f) * m.W, (R(fr, i, 1) - .5f) * m.H, 45, 6 + 10 * R(fr, i, 2), 2, 2, m.C(0xFFFFFFu, .6f)); }, 4);
            for (int i = 0; i < 4; i++) { float f = c.F(p.Vignette) / 100 * (.4f - i * .08f), w = 50 + i * 40; foreach (int s in Sides) { m.Box(s * (m.W / 2 - w / 2), 0, 20, w, m.H, 4, m.C(0x000000u, f), .01f); m.Box(0, s * (m.H / 2 - w / 2), 20, m.W, w, 4, m.C(0x000000u, f), .01f); } }
            float lx = -m.W / 2 + 120, ly = -m.H / 2 + 100;
            switch (p.Label)
            {
                case VhsLabel.Play: m.Text("PLAY", fs, lx + 130, ly, 60, 6, tc); m.Slab(55, 12, tc, lx, ly - fs * .4f, lx + fs * .7f, ly, lx, ly + fs * .4f); break;
                case VhsLabel.Rec: m.Ball(lx + 20, ly, 60, fs * .3f, m.C(0xFF2D2Du, MathF.Sin(m.Now * 4) > 0 ? 1 : .2f)); m.Text("REC", fs, lx + 130, ly, 60, 6, tc); break;
                case VhsLabel.Pause: foreach (int s in Sides) m.Box(lx + 20 + s * 14, ly, 60, 16, fs * .8f, 12, tc, .1f); m.Text("PAUSE", fs, lx + 160, ly, 60, 6, tc); break;
                case VhsLabel.Rewind: for (int i = 0; i < 2; i++) m.Slab(55, 12, tc, lx + 60 - i * 40, ly - fs * .4f, lx + 60 - i * 40, ly + fs * .4f, lx + 20 - i * 40, ly); m.Text("REW", fs, lx + 160, ly, 60, 6, tc); break;
            }
            if (!string.IsNullOrEmpty(p.Mode)) m.Text(p.Mode, fs, m.W / 2 - 140, ly, 60, 6, tc);
            if (p.Counter) { float t = m.Now; m.Text($"{(int)t / 3600}:{(int)t / 60 % 60:00}:{(int)t % 60:00}", fs, m.W / 2 - 240, m.H / 2 - 160, 60, 6, tc); }
            m.Text(p.DateText, fs, -m.W / 2 + 300, m.H / 2 - 100, 60, 6, tc);
        });
    }
}
