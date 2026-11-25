using System.Globalization;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>
/// テキストの書式(フォント・文字色・装飾など)から、見本の文字を描いたサムネイル画像を作る。
/// 値は「項目名 → JSON」で受け取る(YMM4から直接読んだ書式も、コピーから読んだ書式も同じ形)
/// </summary>
public static class TextThumb
{
    public const int Width = 320, Height = 180;

    /// <summary>保存した書式のJSON(オブジェクト)から作る</summary>
    public static BitmapSource? FromStyleJson(string? styleJson, string sample, string background)
    {
        if (string.IsNullOrWhiteSpace(styleJson)) return null;
        try
        {
            if (JsonNode.Parse(styleJson) is not JsonObject obj) return null;
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in obj) if (kv.Value != null) values[kv.Key] = kv.Value.ToJsonString();
            return Render(values, sample, background);
        }
        catch { return null; }
    }

    public static BitmapSource? Render(IReadOnlyDictionary<string, string> values, string sample, string background)
    {
        try
        {
            string? Raw(params string[] keys)
            {
                foreach (var k in keys)
                {
                    if (values.TryGetValue(k, out var v)) return v;
                    if (values.TryGetValue("Jimaku" + k, out v)) return v;      // ボイスアイテムの字幕
                }
                return null;
            }

            var fontName = StringOf(Raw("Font", "FontFamily")) ?? "Yu Gothic UI";
            var color = ColorOf(Raw("FontColor", "Color")) ?? Colors.White;
            var styleColor = ColorOf(Raw("StyleColor", "BorderColor", "OutlineColor")) ?? Colors.Black;
            bool bold = BoolOf(Raw("Bold", "IsBold"));
            bool italic = BoolOf(Raw("Italic", "IsItalic"));
            var deco = DecorationOf(Raw("Style", "Decoration"));
            var bg = ColorOf("\"" + background + "\"") ?? Color.FromRgb(0x20, 0x22, 0x28);

            if (string.IsNullOrWhiteSpace(sample)) sample = "あア亜Aa";
            var typeface = new Typeface(new FontFamily(fontName), italic ? FontStyles.Italic : FontStyles.Normal,
                bold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal);
            double dpi = 1.0;

            // 枠に収まる大きさを探す(装飾の太さの分も空ける)
            FormattedText Make(double size)
            {
                var ft = new FormattedText(sample.Replace("\\n", "\n"), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, size,
                    new SolidColorBrush(color), dpi) { TextAlignment = TextAlignment.Center };
                return ft;
            }
            double size = 100;
            var probe = Make(size);
            double pad = deco == Deco.Border ? 0.18 : deco == Deco.Shadow ? 0.1 : 0.04;
            double maxW = Width * (0.86 - pad), maxH = Height * (0.78 - pad);
            size *= Math.Min(maxW / Math.Max(1, probe.WidthIncludingTrailingWhitespace), maxH / Math.Max(1, probe.Height));
            size = Math.Clamp(size, 6, 160);
            var text = Make(size);

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(new SolidColorBrush(bg), null, new Rect(0, 0, Width, Height));
                // 文字揃えを中央にしているので、x は中央の位置
                var origin = new Point(Width / 2.0, (Height - text.Height) / 2.0);
                var geo = text.BuildGeometry(origin);
                switch (deco)
                {
                    case Deco.Border:
                    {
                        var pen = new Pen(new SolidColorBrush(styleColor), Math.Max(1.5, size * 0.14)) { LineJoin = PenLineJoin.Round };
                        dc.DrawGeometry(null, pen, geo);
                        break;
                    }
                    case Deco.Shadow:
                    {
                        var shadow = geo.Clone();
                        shadow.Transform = new TranslateTransform(size * 0.06, size * 0.06);
                        dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(200, styleColor.R, styleColor.G, styleColor.B)), null, shadow);
                        break;
                    }
                }
                dc.DrawGeometry(new SolidColorBrush(color), null, geo);
            }
            var bmp = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    enum Deco { None, Border, Shadow }

    static Deco DecorationOf(string? json)
    {
        if (json == null) return Deco.None;
        var s = json.Trim('"');
        if (int.TryParse(s, out var n)) return n switch { 0 => Deco.None, 1 or 2 => Deco.Border, _ => Deco.Shadow };
        if (s.Contains("Border", StringComparison.OrdinalIgnoreCase) || s.Contains("Edge", StringComparison.OrdinalIgnoreCase)
            || s.Contains("Outline", StringComparison.OrdinalIgnoreCase) || s.Contains("縁")) return Deco.Border;
        if (s.Contains("Shadow", StringComparison.OrdinalIgnoreCase) || s.Contains("影")) return Deco.Shadow;
        return Deco.None;
    }

    static string? StringOf(string? json)
    {
        if (json == null) return null;
        try
        {
            var node = JsonNode.Parse(json);
            if (node is JsonValue v && v.TryGetValue<string>(out var s)) return s;
            if (node is JsonObject o)
                foreach (var k in new[] { "Name", "FamilyName", "Source", "Family" })
                    if (o[k] is JsonValue fv && fv.TryGetValue<string>(out var fs)) return fs;
        }
        catch { }
        return json.Trim('"');
    }

    static bool BoolOf(string? json) => json != null && json.Trim('"').Equals("true", StringComparison.OrdinalIgnoreCase);

    static double? NumberOf(string? json)
        => json != null && double.TryParse(json.Trim('"'), NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : null;

    /// <summary>"#AARRGGBB" / "#RRGGBB" / {"A":..,"R":..} の色</summary>
    public static Color? ColorOf(string? json)
    {
        if (json == null) return null;
        try
        {
            var node = JsonNode.Parse(json);
            if (node is JsonObject o)
            {
                byte B(string k) => o[k] is JsonValue v && v.TryGetValue<int>(out var i) ? (byte)Math.Clamp(i, 0, 255) : (byte)255;
                if (o["R"] != null) return Color.FromArgb(B("A"), B("R"), B("G"), B("B"));
                if (o["Color"] != null) return ColorOf(o["Color"]!.ToJsonString());
            }
            var s = node is JsonValue sv && sv.TryGetValue<string>(out var str) ? str : json.Trim('"');
            if (ColorConverter.ConvertFromString(s) is Color c) return c;
        }
        catch { }
        return null;
    }
}
