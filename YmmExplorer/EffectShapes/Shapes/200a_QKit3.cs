using System.Globalization;
using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// =====================================================================
//  411番以降の図形の共通の道具
//   G(...) は、使う項目だけを「項目=名前|説明|最小|最大」で書くと、残りの項目を隠す
// =====================================================================
public abstract class Q3Param : QuickShapeParameter
{
    protected Q3Param(SharedDataStore? sharedData) : base(sharedData) { }

    static readonly string[] allKeys = ["Text", "Amount", "Amount2", "Count", "Variant", "Background", "Color1", "Color2", "Color3", "Speed", "Seed"];

    protected static Dictionary<string, ParamGuide> G(string title, params string[] used)
    {
        var d = allKeys.ToDictionary(k => k, _ => new ParamGuide(Hidden: true));
        string group = $"「{title}」の設定";
        foreach (var spec in used)
        {
            int eq = spec.IndexOf('=');
            string key = eq < 0 ? spec : spec[..eq];
            var f = (eq < 0 ? "" : spec[(eq + 1)..]).Split('|');
            string? At(int i) => i < f.Length && f[i].Length > 0 ? f[i] : null;
            double? N(int i) => double.TryParse(At(i), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;
            d[key] = new ParamGuide(Label: At(0), Tip: At(1), Min: N(2), Max: N(3), Group: key.StartsWith("Color") ? null : group);
        }
        return d;
    }

    protected List<string> Rows => Str.Split('\n').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
    protected static string[] Cells(string row) => row.Split([':', '：', ',', '、']).Select(x => x.Trim()).ToArray();
    protected static float Num(string? s, float def = 0f)
    {
        if (string.IsNullOrWhiteSpace(s)) return def;
        s = s.Trim().Replace('＋', '+').Replace('−', '-').Replace('－', '-').Replace("%", "").Replace("％", "");
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : def;
    }
    protected List<float> Nums => Items.Select(x => Num(x, float.NaN)).Where(x => !float.IsNaN(x)).ToList();
    protected static float Rnd(int n) => Gfx.Hash01(n);
    protected static float Lerp(float x, float y, float k) => x + (y - x) * k;
    protected static float Sub(float t, float start, float dur) => Math.Clamp((t - start) / MathF.Max(0.001f, dur), 0f, 1f);

    protected static void Panel(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, Vector2 ctr, float w, float h, float r, Color4 fill, Color4 edge, float ew)
    {
        RBox(dc, br, ctr, w, h, r, fill);
        if (ew > 0f) RFrame(dc, br, ctr, w, h, r, edge, ew);
    }

    protected static void Arrow(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, Vector2 p, Vector2 q, Color4 col, float w)
    {
        var d = q - p;
        float len = d.Length();
        if (len < 0.01f) return;
        d /= len;
        var n = new Vector2(-d.Y, d.X);
        float head = w * 3.2f;
        Ln(dc, br, p, q - d * head * 0.8f, col, w);
        Poly(dc, br, new List<Vector2> { q, q - d * head * 1.5f + n * head, q - d * head * 1.5f - n * head }, col);
    }

    protected static void Pie(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, Vector2 o, float r, float a0, float a1, Color4 col)
    {
        var pts = Gfx.ArcPts(o, r, a0, a1, Math.Max(6, (int)(MathF.Abs(a1 - a0) * 16f)));
        pts.Insert(0, o);
        Poly(dc, br, pts, col);
    }

    protected static void Arc(ID2D1DeviceContext dc, ID2D1SolidColorBrush br, Vector2 o, float r, float a0, float a1, Color4 col, float w)
        => Path(dc, br, Gfx.ArcPts(o, r, a0, a1, Math.Max(6, (int)(MathF.Abs(a1 - a0) * 16f))), col, w);
}
