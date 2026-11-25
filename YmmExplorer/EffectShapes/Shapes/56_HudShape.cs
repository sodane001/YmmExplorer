using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Shape;
using WpfColor = System.Windows.Media.Color;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

/// <summary>HUD風図形・ネオン図形で共通の「形」の定義と線描画</summary>
internal static class FormKit
{
    // ---- YMM4の図形（IShapePlugin）の名前から、描画する形を判定する ----
    static readonly Dictionary<Type, int> kindCache = new();

    /// <summary>0=円 1=四角 2=三角 3=多角形 4=ひし形 5=星 6=ハート 7=十字 8=ライン（不明な図形は円）</summary>
    public static int KindFromShapeType(Type? type)
    {
        if (type is null) return 0;
        lock (kindCache)
        {
            if (kindCache.TryGetValue(type, out var cached)) return cached;
            string name = "";
            try { if (Activator.CreateInstance(type) is IShapePlugin p) name = p.Name ?? ""; } catch { }
            int k = Classify((name + " " + type.Name).ToLowerInvariant());
            kindCache[type] = k;
            return k;
        }
    }

    static int Classify(string s)
    {
        bool Has(params string[] xs) => xs.Any(x => s.Contains(x));
        if (Has("ハート", "heart")) return 6;
        if (Has("星", "star")) return 5;
        if (Has("十字", "プラス", "cross", "plus")) return 7;
        if (Has("ひし", "菱", "ダイヤ", "diamond", "rhomb")) return 4;
        if (Has("多角", "ポリゴン", "polygon", "hexagon", "六角")) return 3;
        if (Has("三角", "triangle")) return 2;
        if (Has("四角", "長方", "矩形", "rect", "square", "box")) return 1;
        if (Has("線", "ライン", "line")) return 8;
        return 0;
    }

    static Vector2 Rot(Vector2 v, float rot)
    {
        float cs = MathF.Cos(rot), sn = MathF.Sin(rot);
        return new Vector2(v.X * cs - v.Y * sn, v.X * sn + v.Y * cs);
    }

    static List<Vector2> Polygon(Vector2 c, float r, int n, float rot, float startAng)
    {
        var pts = new List<Vector2>();
        for (int i = 0; i <= n; i++)
        {
            float a = startAng + rot + i * MathF.PI * 2f / n;
            pts.Add(c + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r);
        }
        return pts;
    }

    /// <summary>形の輪郭を折れ線のリストで返す</summary>
    public static List<List<Vector2>> FormPaths(int kind, Vector2 c, float r, float rot, int sides, int points)
    {
        var res = new List<List<Vector2>>();
        sides = Math.Clamp(sides, 3, 24);
        points = Math.Clamp(points, 3, 24);
        switch (kind)
        {
            case 0: res.Add(Polygon(c, r, 96, rot, 0f)); break;
            case 1: res.Add(Polygon(c, r * 1.1f, 4, rot, MathF.PI / 4f)); break;
            case 2: res.Add(Polygon(c, r * 1.1f, 3, rot, -MathF.PI / 2f)); break;
            case 3: res.Add(Polygon(c, r, sides, rot, -MathF.PI / 2f)); break;
            case 4: res.Add(Polygon(c, r, 4, rot, 0f)); break;
            case 5:
                {
                    var pts = new List<Vector2>();
                    for (int i = 0; i <= points * 2; i++)
                    {
                        float a = -MathF.PI / 2f + rot + i * MathF.PI / points;
                        float rr = (i % 2 == 0) ? r : r * 0.45f;
                        pts.Add(c + new Vector2(MathF.Cos(a), MathF.Sin(a)) * rr);
                    }
                    res.Add(pts);
                    break;
                }
            case 6:
                {
                    var pts = new List<Vector2>();
                    float s = r / 15f;
                    for (int i = 0; i <= 80; i++)
                    {
                        float tt = i / 80f * MathF.PI * 2f;
                        float x = 16f * MathF.Pow(MathF.Sin(tt), 3f);
                        float y = -(13f * MathF.Cos(tt) - 5f * MathF.Cos(2f * tt) - 2f * MathF.Cos(3f * tt) - MathF.Cos(4f * tt));
                        pts.Add(c + Rot(new Vector2(x, y + 1.5f) * s, rot));
                    }
                    res.Add(pts);
                    break;
                }
            case 7:
                res.Add(new List<Vector2> { c + Rot(new Vector2(-r, 0), rot), c + Rot(new Vector2(r, 0), rot) });
                res.Add(new List<Vector2> { c + Rot(new Vector2(0, -r), rot), c + Rot(new Vector2(0, r), rot) });
                break;
            case 8:
                res.Add(new List<Vector2> { c + Rot(new Vector2(-r * 1.3f, 0), rot), c + Rot(new Vector2(r * 1.3f, 0), rot) });
                break;
            default: break;
        }
        return res;
    }

    /// <summary>折れ線を描く。関節を丸める場合は円で埋める</summary>
    public static void Stroke(ID2D1DeviceContext dc, ID2D1Brush brush, List<Vector2> pts, float width, bool joints)
    {
        if (joints) { Gfx.Polyline(dc, brush, pts, width); return; }
        for (int i = 0; i < pts.Count - 1; i++) dc.DrawLine(pts[i], pts[i + 1], brush, width);
    }

    /// <summary>破線（offset を動かすと流れる）</summary>
    public static void Dash(ID2D1DeviceContext dc, ID2D1Brush brush, List<Vector2> pts, float dash, float gap, float offset, float width)
    {
        float period = MathF.Max(1f, dash + gap);
        float acc = 0f;
        for (int i = 0; i < pts.Count - 1; i++)
        {
            var a = pts[i];
            var d = pts[i + 1] - a;
            float L = d.Length();
            if (L < 0.01f) continue;
            var dirv = d / L;
            float s = 0f;
            while (s < L)
            {
                float m = Gfx.Mod(acc + s + offset, period);
                if (m < dash)
                {
                    float run = MathF.Max(0.05f, MathF.Min(dash - m, L - s));
                    dc.DrawLine(a + dirv * s, a + dirv * (s + run), brush, width);
                    s += run;
                }
                else
                {
                    s += MathF.Max(0.05f, MathF.Min(period - m, L - s));
                }
            }
            acc += L;
        }
    }

    /// <summary>リサジュー軌道で漂う点</summary>
    public static Vector2 Liss(Vector2 c, float ax, float ay, float t, float fx, float fy, float p1, float p2)
        => new(c.X + MathF.Sin(t * fx + p1) * ax, c.Y + MathF.Sin(t * fy + p2) * ay);
}

// =====================================================================
//  HUD風図形
//   緑とグレーで構成されたHUD風の図形。YMM4の図形から形を選べる
// =====================================================================
public sealed class FutureShapePlugin : IShapePlugin
{
    public string Name => "演出：HUD風図形";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new FutureShapeParameter(sharedData);
}

public sealed class FutureShapeParameter : EffectShapeParameterBase
{
    public FutureShapeParameter() : this(null) { }
    public FutureShapeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "形（YMM4の図形）", Description = "YMM4の図形から選ぶ。円・四角形・三角形・多角形・星・ハートなどを認識する")]
    [ShapeTypeComboBox]
    public Type ShapeType { get => shapeType; set => Set(ref shapeType, value); }
    Type shapeType = PluginLoader.GetPrimaryPluginType<IShapePlugin>();

    [Display(GroupName = "形", Name = "パーティクルにする", Description = "オンにすると、選んだ図形の代わりにパーティクル（つながる粒）で表示する")]
    [ToggleSlider]
    public bool ParticleMode { get => particleMode; set => Set(ref particleMode, value); }
    bool particleMode = false;

    [Display(GroupName = "形", Name = "多角形の角数")]
    [TextBoxSlider("F0", "角", 3, 12)]
    [Range(3, 12)]
    public int PolygonSides { get => sides; set => Set(ref sides, value); }
    int sides = 6;

    [Display(GroupName = "形", Name = "星の角数")]
    [TextBoxSlider("F0", "角", 3, 12)]
    [Range(3, 12)]
    public int StarPoints { get => starPoints; set => Set(ref starPoints, value); }
    int starPoints = 5;

    [Display(GroupName = "形", Name = "メインカラー（緑）")]
    [ColorPicker]
    public WpfColor MainColor { get => main; set => Set(ref main, value); }
    WpfColor main = WpfColor.FromRgb(0x39, 0xFF, 0x88);

    [Display(GroupName = "形", Name = "サブカラー（グレー）")]
    [ColorPicker]
    public WpfColor SubColor { get => sub; set => Set(ref sub, value); }
    WpfColor sub = WpfColor.FromRgb(0x8A, 0x9B, 0xA8);

    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "形", Name = "大きさ（半径）")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Size { get; } = new Animation(260, 5, 5000);

    [Display(GroupName = "形", Name = "重ねる数")]
    [AnimationSlider("F0", "層", 1, 10)]
    public Animation Layers { get; } = new Animation(4, 1, 30);

    [Display(GroupName = "形", Name = "回転速度")]
    [AnimationSlider("F0", "°/s", -180, 180)]
    public Animation Speed { get; } = new Animation(25, -3600, 3600);

    [Display(GroupName = "形", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0.5, 8)]
    public Animation Thickness { get; } = new Animation(2, 0.2, 100);

    [Display(GroupName = "形", Name = "光のにじみ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Glow { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "形", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    // ---- 装飾（それぞれオン/オフを選べる）----
    [Display(GroupName = "目盛りリング", Name = "表示する")]
    [ToggleSlider]
    public bool ShowTicks { get => showTicks; set => Set(ref showTicks, value); }
    bool showTicks = true;

    [Display(GroupName = "目盛りリング", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Ticks { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "コーナーの枠", Name = "表示する")]
    [ToggleSlider]
    public bool ShowBrackets { get => showBrackets; set => Set(ref showBrackets, value); }
    bool showBrackets = true;

    [Display(GroupName = "コーナーの枠", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Brackets { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "レーダー", Name = "表示する")]
    [ToggleSlider]
    public bool ShowRadar { get => showRadar; set => Set(ref showRadar, value); }
    bool showRadar = false;

    [Display(GroupName = "レーダー", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Radar { get; } = new Animation(60, 0, 100);

    [Display(GroupName = "周回する点", Name = "表示する")]
    [ToggleSlider]
    public bool ShowNodes { get => showNodes; set => Set(ref showNodes, value); }
    bool showNodes = true;

    [Display(GroupName = "周回する点", Name = "強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Nodes { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "パーティクル", Name = "粒の数")]
    [AnimationSlider("F0", "個", 5, 200)]
    public Animation ParticleCount { get; } = new Animation(60, 1, 1000);

    [Display(GroupName = "パーティクル", Name = "つなぐ距離")]
    [AnimationSlider("F0", "%", 10, 100)]
    public Animation LinkDistance { get; } = new Animation(40, 1, 300);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Size, Layers, Speed, Thickness, Glow, Opacity, Ticks, Brackets, Radar, Nodes, ParticleCount, LinkDistance];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float size = c.F(Size);
        int layers = Math.Clamp((int)c.F(Layers), 1, 10);
        float rotBase = Gfx.Rad(c.F(Speed)) * t;
        float th = c.F(Thickness), glowK = c.F(Glow) / 100f, op = c.F(Opacity) / 100f;
        float tickA = ShowTicks ? c.F(Ticks) / 100f : 0f;
        float brA = ShowBrackets ? c.F(Brackets) / 100f : 0f;
        float radarA = ShowRadar ? c.F(Radar) / 100f : 0f;
        float nodeA = ShowNodes ? c.F(Nodes) / 100f : 0f;
        int kind = ParticleMode ? 9 : FormKit.KindFromShapeType(ShapeType);

        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 中心のぼんやりした光 ----
        using (var g = Gfx.Radial(dc, ctr, size * 0.95f,
            (0f, Gfx.C4(MainColor, 0.10f * glowK * op)), (1f, Gfx.C4(MainColor, 0f))))
            dc.FillEllipse(new Ellipse(ctr, size * 0.95f, size * 0.95f), g);

        // ---- 十字の補助線（グレー）----
        brush.Color = Gfx.C4(SubColor, 0.35f * op);
        dc.DrawLine(ctr - new Vector2(size * 1.35f, 0), ctr + new Vector2(size * 1.35f, 0), brush, 1f);
        dc.DrawLine(ctr - new Vector2(0, size * 1.35f), ctr + new Vector2(0, size * 1.35f), brush, 1f);

        if (kind == 9)
        {
            DrawNetwork(dc, brush, c, ctr, size, op, glowK);
        }
        else
        {
            for (int k = 0; k < layers; k++)
            {
                float rk = size * (1f - 0.55f * k / MathF.Max(1f, layers));
                float sign = (k % 2 == 0) ? 1f : -1f;
                float rot = sign * rotBase * (0.4f + 0.25f * k) + k * 0.35f;
                var paths = FormKit.FormPaths(kind, ctr, rk, rot, PolygonSides, StarPoints);
                bool joints = !(kind == 0 || kind == 6);

                foreach (var path in paths)
                {
                    switch (k % 3)
                    {
                        case 0:
                            brush.Color = Gfx.C4(SubColor, 0.75f * op);
                            FormKit.Stroke(dc, brush, path, th * 0.7f, joints);
                            break;
                        case 1:
                            brush.Color = Gfx.C4(MainColor, 0.14f * glowK * op);
                            FormKit.Dash(dc, brush, path, rk * 0.22f, rk * 0.10f, t * rk * 0.5f * sign, th * 5f);
                            brush.Color = Gfx.C4(MainColor, 0.95f * op);
                            FormKit.Dash(dc, brush, path, rk * 0.22f, rk * 0.10f, t * rk * 0.5f * sign, th * 1.8f);
                            break;
                        default:
                            brush.Color = Gfx.C4(SubColor, 0.8f * op);
                            FormKit.Dash(dc, brush, path, 2.5f, 7f, t * 20f, th * 1.3f);
                            break;
                    }
                }
            }
        }

        // ---- 目盛りリング ----
        if (tickA > 0f)
        {
            float rr = size * 1.12f;
            float tr = t * 0.05f;
            for (int i = 0; i < 72; i++)
            {
                float a = tr + i * MathF.PI * 2f / 72f;
                var dir = new Vector2(MathF.Cos(a), MathF.Sin(a));
                float len = (i % 6 == 0) ? 14f : 6f;
                brush.Color = Gfx.C4(i % 18 == 0 ? MainColor : SubColor, 0.85f * tickA * op);
                dc.DrawLine(ctr + dir * rr, ctr + dir * (rr + len), brush, i % 6 == 0 ? 1.8f : 1f);
            }
        }

        // ---- コーナーの枠 ----
        if (brA > 0f)
        {
            float half = size * 1.3f, cl = size * 0.28f;
            brush.Color = Gfx.C4(MainColor, 0.9f * brA * op);
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sy = -1; sy <= 1; sy += 2)
                {
                    var corner = ctr + new Vector2(sx * half, sy * half);
                    dc.DrawLine(corner, corner - new Vector2(sx * cl, 0), brush, th * 1.6f);
                    dc.DrawLine(corner, corner - new Vector2(0, sy * cl), brush, th * 1.6f);
                }
        }

        // ---- レーダー走査 ----
        if (radarA > 0f)
        {
            float sweep = t * 1.6f;
            for (int j = 0; j < 28; j++)
            {
                float a = sweep - j * 0.035f;
                float al = MathF.Pow(1f - j / 28f, 1.5f) * 0.5f * radarA * op;
                brush.Color = Gfx.C4(MainColor, al);
                dc.DrawLine(ctr, ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * size, brush, MathF.Max(2f, size * 0.04f));
            }
        }

        // ---- 周回する点（データノード）----
        if (nodeA > 0f)
        {
            for (int i = 0; i < 6; i++)
            {
                float a = t * 0.8f + i * MathF.PI / 3f;
                var p = ctr + new Vector2(MathF.Cos(a), MathF.Sin(a)) * size * 1.12f;
                brush.Color = Gfx.C4(MainColor, 0.25f * nodeA * glowK * op);
                dc.FillEllipse(new Ellipse(p, 9f, 9f), brush);
                brush.Color = Gfx.C4(MainColor, 0.95f * nodeA * op);
                Gfx.Rect(dc, brush, p.X - 3f, p.Y - 3f, 6f, 6f);
            }
        }
    }

    // パーティクル：デジタルな粒をつなぐネットワーク
    void DrawNetwork(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, in DrawContext c, Vector2 ctr, float size, float op, float glowK)
    {
        int n = (int)c.F(ParticleCount);
        float link = size * c.F(LinkDistance) / 100f * 1.4f;
        float t = c.Time;
        var rng = new Random(Seed);
        var pos = new Vector2[n];
        var kindArr = new int[n];
        for (int i = 0; i < n; i++)
        {
            float ax = size * 1.6f * (0.3f + 0.7f * rng.NextF()), ay = size * 1.1f * (0.3f + 0.7f * rng.NextF());
            float fx = 0.15f + 0.4f * rng.NextF(), fy = 0.15f + 0.4f * rng.NextF();
            float p1 = rng.NextF() * MathF.PI * 2f, p2 = rng.NextF() * MathF.PI * 2f;
            pos[i] = FormKit.Liss(ctr, ax, ay, t, fx, fy, p1, p2);
            kindArr[i] = rng.NextF() < 0.7f ? 0 : 1;
        }
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            {
                float d = Vector2.Distance(pos[i], pos[j]);
                if (d > link) continue;
                brush.Color = Gfx.C4(kindArr[i] == 0 && kindArr[j] == 0 ? MainColor : SubColor, (1f - d / link) * 0.6f * op);
                dc.DrawLine(pos[i], pos[j], brush, 1f);
            }
        for (int i = 0; i < n; i++)
        {
            if (kindArr[i] == 0)
            {
                brush.Color = Gfx.C4(MainColor, 0.25f * glowK * op);
                dc.FillEllipse(new Ellipse(pos[i], 7f, 7f), brush);
                brush.Color = Gfx.C4(MainColor, 0.95f * op);
                dc.FillEllipse(new Ellipse(pos[i], 2.5f, 2.5f), brush);
            }
            else
            {
                brush.Color = Gfx.C4(SubColor, 0.9f * op);
                Gfx.Rect(dc, brush, pos[i].X - 2.5f, pos[i].Y - 2.5f, 5f, 5f);
            }
        }
    }
}
