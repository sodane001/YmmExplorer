using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using Wm = System.Windows.Media;
using WPoint = System.Windows.Point;

namespace YMM4EffectShapes;

// YMM4 に文字・フォントの入力欄の属性が無い時の、代わり(ビルドを通すため。YMM4標準の画面には欄が出ない)。
// あるかどうかは、YmmExplorer.csproj の DetectYmmEditorAttributes で調べている
#if !YMM_HAS_TEXTEDITOR
[AttributeUsage(AttributeTargets.Property)]
internal sealed class TextEditorAttribute : Attribute
{
    public bool AcceptsReturn { get; set; }
}
#endif
#if !YMM_HAS_FONTCOMBOBOX
[AttributeUsage(AttributeTargets.Property)]
internal sealed class FontComboBoxAttribute : Attribute { }
#endif

// =====================================================================
//  文字を描く小道具(128番以降の図形で使う)
//   WPF で文字の形(輪郭)を作り、それを Direct2D で塗る。日本語もそのまま描ける。
//   同じ文字・大きさの形は覚えておき、毎フレーム作り直さない
// =====================================================================

/// <summary>文字列の形(輪郭の点の並び)と大きさ</summary>
internal sealed class TextShape
{
    public readonly List<Vector2[]> Contours;
    public readonly float Width, Height;
    /// <summary>辺の一覧(塗る時に使う)。上端の y の小さい順</summary>
    internal readonly (float ax, float ay, float bx, float by, int dir, float y0, float y1)[] Edges;

    public TextShape(List<Vector2[]> contours, float width, float height)
    {
        Contours = contours;
        Width = width;
        Height = height;
        var list = new List<(float, float, float, float, int, float, float)>();
        foreach (var c in contours)
            for (int i = 0; i < c.Length; i++)
            {
                var a = c[i];
                var b = c[(i + 1) % c.Length];
                if (MathF.Abs(a.Y - b.Y) < 1e-6f) continue;
                int dir = b.Y > a.Y ? 1 : -1;
                list.Add((a.X, a.Y, b.X, b.Y, dir, MathF.Min(a.Y, b.Y), MathF.Max(a.Y, b.Y)));
            }
        list.Sort((p, q) => p.Item6.CompareTo(q.Item6));
        Edges = list.ToArray();
    }

    // 形のパス(描画を軽くするため、一度作ったら使い回す。Direct2D のファクトリーごと)
    readonly Dictionary<IntPtr, ID2D1PathGeometry> geos = new();

    internal ID2D1PathGeometry? Geometry(ID2D1DeviceContext dc)
    {
        if (!GeoKit.Enabled || Contours.Count == 0) return null;
        var f = GeoKit.FactoryOf(dc);
        if (f == null) return null;
        var key = f.NativePointer;
        lock (geos)
        {
            if (geos.TryGetValue(key, out var g)) return g;
            if (geos.Count >= 4) geos.Clear();
            g = GeoKit.Build(dc, Contours.Select(c => (IReadOnlyList<Vector2>)c), true);
            if (g != null) geos[key] = g;
            return g;
        }
    }
}

internal static class TextKit
{
    public const string DefaultFont = "Yu Gothic UI";
    static readonly ConcurrentDictionary<string, TextShape> cache = new();
    static readonly CultureInfo Japanese = CultureInfo.GetCultureInfo("ja-JP");

    /// <summary>
    /// 文字列の形を作る。maxWidth を超える行は折り返す(0 なら折り返さない)。
    /// align は 0=左そろえ 1=中央 2=右(複数行の時の、行のそろえ方)
    /// </summary>
    public static TextShape Get(string? text, float size, bool bold = true, string? font = null, float maxWidth = 0f, int align = 0)
    {
        text ??= "";
        size = MathF.Max(1f, MathF.Round(size * 2f) / 2f);
        maxWidth = maxWidth > 0 ? MathF.Round(maxWidth) : 0f;
        string fam = string.IsNullOrWhiteSpace(font) ? DefaultFont : font!.Trim();
        string key = string.Concat(fam, "\u0001", size.ToString(CultureInfo.InvariantCulture), "\u0001", bold ? "B" : "N", "\u0001",
            maxWidth.ToString(CultureInfo.InvariantCulture), "\u0001", align.ToString(CultureInfo.InvariantCulture), "\u0001", text);
        if (cache.TryGetValue(key, out var hit)) return hit;
        if (cache.Count > 600) cache.Clear();
        var made = Build(text, size, bold, fam, maxWidth, align);
        cache[key] = made;
        return made;
    }

    static Vector2 V(WPoint p) => new((float)p.X, (float)p.Y);

    static TextShape Build(string text, float size, bool bold, string fam, float maxWidth, int align)
    {
        var contours = new List<Vector2[]>();
        float w = 0f, h = size * 1.33f;
        if (text.Length == 0) return new TextShape(contours, 0f, h);
        try
        {
            var tf = new Wm.Typeface(new Wm.FontFamily(fam), System.Windows.FontStyles.Normal,
                bold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal, System.Windows.FontStretches.Normal);
            var ft = new Wm.FormattedText(text, Japanese, System.Windows.FlowDirection.LeftToRight, tf, size, Wm.Brushes.Black, 1.0);
            if (maxWidth > 0) ft.MaxTextWidth = maxWidth;
            // 折り返した後の、いちばん長い行の幅に合わせてから、行をそろえる
            double natural = Math.Max(1.0, ft.WidthIncludingTrailingWhitespace);
            if (align != 0)
            {
                ft.MaxTextWidth = Math.Ceiling(natural) + 0.5;
                ft.TextAlignment = align == 1 ? System.Windows.TextAlignment.Center : System.Windows.TextAlignment.Right;
            }
            w = (float)natural;
            h = (float)ft.Height;
            var geo = ft.BuildGeometry(new WPoint(0, 0));
            var flat = geo.GetFlattenedPathGeometry(0.15, Wm.ToleranceType.Absolute);
            foreach (var fig in flat.Figures)
            {
                var pts = new List<Vector2> { V(fig.StartPoint) };
                foreach (var seg in fig.Segments)
                {
                    if (seg is Wm.PolyLineSegment pl) foreach (var p in pl.Points) pts.Add(V(p));
                    else if (seg is Wm.LineSegment ls) pts.Add(V(ls.Point));
                }
                if (pts.Count >= 3) contours.Add(pts.ToArray());
            }
        }
        catch { }
        return new TextShape(contours, w, h);
    }

    /// <summary>形を塗る(左上 topLeft、倍率 scale)。重なった輪郭も正しく塗る(ノンゼロ規則)</summary>
    public static void Fill(ID2D1DeviceContext dc, ID2D1Brush brush, TextShape s, Vector2 topLeft, float scale = 1f)
    {
        var edges = s.Edges;
        if (edges.Length == 0 || scale <= 0.001f) return;
        if (s.Geometry(dc) is { } geo && GeoKit.FillCached(dc, brush, geo, topLeft, scale)) return;
        float minY = edges[0].y0, maxY = float.MinValue;
        foreach (var e in edges) if (e.y1 > maxY) maxY = e.y1;
        // 画面上で 0.5px ごとの行に分けて塗る
        float step = 0.5f;
        float sy0 = MathF.Floor((topLeft.Y + minY * scale) / step) * step;
        float sy1 = topLeft.Y + maxY * scale;
        var xs = new List<(float x, int d)>(64);
        for (float y = sy0; y < sy1; y += step)
        {
            float ly = (y + step * 0.5f - topLeft.Y) / scale;      // 形の中での高さ
            xs.Clear();
            for (int i = 0; i < edges.Length; i++)
            {
                var e = edges[i];
                if (e.y0 > ly) break;
                if (e.y1 <= ly) continue;
                float x = e.ax + (e.bx - e.ax) * (ly - e.ay) / (e.by - e.ay);
                xs.Add((topLeft.X + x * scale, e.dir));
            }
            if (xs.Count < 2) continue;
            xs.Sort((p, q) => p.x.CompareTo(q.x));
            int wind = 0;
            for (int k = 0; k < xs.Count - 1; k++)
            {
                wind += xs[k].d;
                if (wind != 0 && xs[k + 1].x > xs[k].x + 0.01f)
                    dc.FillRectangle(new RawRectF(xs[k].x, y, xs[k + 1].x, y + step), brush);
            }
        }
    }

    /// <summary>輪郭をなぞる(ふち取り用。width は線の太さ)</summary>
    public static void Stroke(ID2D1DeviceContext dc, ID2D1Brush brush, TextShape s, Vector2 topLeft, float width, float scale = 1f)
    {
        if (width <= 0.05f) return;
        if (s.Geometry(dc) is { } geo && GeoKit.StrokeCached(dc, brush, geo, topLeft, scale, width)) return;
        float r = width / 2f;
        foreach (var c in s.Contours)
        {
            for (int i = 0; i < c.Length; i++)
            {
                var a = topLeft + c[i] * scale;
                var b = topLeft + c[(i + 1) % c.Length] * scale;
                dc.DrawLine(a, b, brush, width);
                dc.FillEllipse(new Ellipse(a, r, r), brush);
            }
        }
    }

    /// <summary>ふち取り・影つきで描く(左上を基準)</summary>
    public static void Draw(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, TextShape s, Vector2 topLeft, Color4 fill,
        Color4 outline = default, float outlineWidth = 0f, float scale = 1f, Color4 shadow = default, Vector2 shadowOffset = default)
    {
        if (shadow.A > 0.002f)
        {
            brush.Color = shadow;
            var so = topLeft + shadowOffset;
            if (outlineWidth > 0.05f) Stroke(dc, brush, s, so, outlineWidth * 2f, scale);
            Fill(dc, brush, s, so, scale);
        }
        if (outline.A > 0.002f && outlineWidth > 0.05f)
        {
            brush.Color = outline;
            Stroke(dc, brush, s, topLeft, outlineWidth * 2f, scale);
        }
        if (fill.A > 0.002f)
        {
            brush.Color = fill;
            Fill(dc, brush, s, topLeft, scale);
        }
    }

    /// <summary>中心を基準に描く</summary>
    public static void DrawCentered(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, TextShape s, Vector2 center, Color4 fill,
        Color4 outline = default, float outlineWidth = 0f, float scale = 1f, Color4 shadow = default, Vector2 shadowOffset = default)
        => Draw(dc, brush, s, center - new Vector2(s.Width, s.Height) * (scale / 2f), fill, outline, outlineWidth, scale, shadow, shadowOffset);

    /// <summary>「\n」を改行にする(1行の入力欄で改行を書きたい時用)。null は空文字</summary>
    public static string Lines(string? s) => (s ?? "").Replace("\r\n", "\n").Replace("\\n", "\n");

    /// <summary>カンマ・読点・改行で区切った一覧</summary>
    public static List<string> Split(string? s)
        => Lines(s).Split(new[] { ',', '、', '，', '\n' }, StringSplitOptions.None).Select(x => x.Trim()).ToList();

    /// <summary>1文字ずつ出す時の、いま見えている文字列(秒あたり cps 文字。0 以下なら全部)</summary>
    public static string Typed(string text, float time, float cps)
    {
        if (cps <= 0.001f) return text;
        int n = Math.Clamp((int)MathF.Floor(MathF.Max(0f, time) * cps), 0, text.Length);
        return text[..n];
    }
}
