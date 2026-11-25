using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>
/// 自動収録した図形・エフェクトのサムネイル: 名前を大きく書いた画像を作る。
/// 図形のプレビュー画像があれば、その下に名前の帯を重ねる(何の図形か、ひと目で分かるように)
/// </summary>
public static class NameThumb
{
    const int W = 320, H = 180;
    static readonly Brush Back = Frozen(Color.FromRgb(0x1F, 0x23, 0x2B));
    static readonly Brush Band = Frozen(Color.FromArgb(0xD0, 0x14, 0x16, 0x1B));
    static readonly Brush Fore = Frozen(Color.FromRgb(0xE9, 0xEC, 0xF1));
    static readonly Brush Sub = Frozen(Color.FromRgb(0x98, 0xA1, 0xAE));
    static readonly Typeface Face = new(new FontFamily("Yu Gothic UI, Meiryo UI, Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
    static readonly Typeface SubFace = new(new FontFamily("Yu Gothic UI, Meiryo UI, Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

    static Brush Frozen(Color c) { var b = new SolidColorBrush(c); b.Freeze(); return b; }

    /// <summary>種類ごとの色(左の線・下の線)。同じ種類は同じ色になる</summary>
    static Brush AccentFor(string kind)
    {
        uint h = 2166136261;
        foreach (var ch in kind) h = (h ^ ch) * 16777619;
        var hue = h % 360 / 360.0;
        // 彩度・明るさは固定(暗い背景で読みやすい色)
        return Frozen(FromHsv(hue, 0.55, 0.85));
    }

    static Color FromHsv(double h, double s, double v)
    {
        double r = 0, g = 0, b = 0, i = Math.Floor(h * 6), f = h * 6 - i, p = v * (1 - s), q = v * (1 - f * s), t = v * (1 - (1 - f) * s);
        switch ((int)i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }
        return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    static FormattedText Text(string s, Typeface face, double size, Brush brush, double maxWidth, int maxLines)
    {
        var ft = new FormattedText(s, CultureInfo.GetCultureInfo("ja-JP"), FlowDirection.LeftToRight, face, size, brush, 1.0)
        {
            MaxTextWidth = maxWidth,
            MaxLineCount = maxLines,
            Trimming = TextTrimming.CharacterEllipsis,
            TextAlignment = TextAlignment.Center,
        };
        return ft;
    }

    /// <summary>名前のサムネイルを作る。preview があれば、その上に名前の帯を重ねる。kind は「映像エフェクト」などの小さな文字</summary>
    public static BitmapSource Render(string name, string kind, BitmapSource? preview = null)
    {
        name = string.IsNullOrWhiteSpace(name) ? "(名前なし)" : name.Trim();
        var accent = AccentFor(kind);
        var dv = new DrawingVisual();
        using (var dc = dv.RenderOpen())
        {
            dc.DrawRectangle(Back, null, new Rect(0, 0, W, H));
            if (preview != null)
            {
                // プレビュー画像を、縦横比を保って全体に入れる
                double sc = Math.Min(W / (double)preview.PixelWidth, H / (double)preview.PixelHeight);
                double pw = preview.PixelWidth * sc, ph = preview.PixelHeight * sc;
                dc.DrawImage(preview, new Rect((W - pw) / 2, (H - ph) / 2, pw, ph));
                // 下に名前の帯
                var ft = Text(name, Face, 17, Fore, W - 16, 1);
                double bh = ft.Height + 12;
                dc.DrawRectangle(Band, null, new Rect(0, H - bh, W, bh));
                dc.DrawRectangle(accent, null, new Rect(0, H - bh, 4, bh));
                dc.DrawText(ft, new Point(8, H - bh + 6));
            }
            else
            {
                dc.DrawRectangle(accent, null, new Rect(0, 0, 6, H));
                // 長い名前は文字を小さくして、2行まで
                double size = name.Length <= 6 ? 34 : name.Length <= 10 ? 28 : name.Length <= 16 ? 22 : 18;
                var ft = Text(name, Face, size, Fore, W - 36, 2);
                var sub = Text(kind, SubFace, 13, Sub, W - 36, 1);
                double total = ft.Height + 8 + sub.Height;
                double y = (H - total) / 2;
                dc.DrawText(ft, new Point(18, y));
                dc.DrawText(sub, new Point(18, y + ft.Height + 8));
            }
        }
        var bmp = new RenderTargetBitmap(W, H, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(dv);
        bmp.Freeze();
        return bmp;
    }

    /// <summary>図形の絵だけのサムネイル(名前の帯なし)を、保存できる形で作る</summary>
    public static string? EncodePicture(BitmapSource picture)
    {
        try { return PresetThumb.Encode(Picture(picture)); }
        catch { return null; }
    }

    /// <summary>図形の絵だけのサムネイル(名前の帯なし)</summary>
    public static BitmapSource Picture(BitmapSource picture)
    {
        double sc = Math.Min(W / (double)picture.PixelWidth, H / (double)picture.PixelHeight);
        double pw = picture.PixelWidth * sc, ph = picture.PixelHeight * sc;
        var dv = new DrawingVisual();
        RenderOptions.SetBitmapScalingMode(dv, BitmapScalingMode.HighQuality);
        using (var dc = dv.RenderOpen())
        {
            dc.DrawRectangle(Back, null, new Rect(0, 0, W, H));
            dc.DrawImage(picture, new Rect((W - pw) / 2, (H - ph) / 2, pw, ph));
        }
        var bmp = new RenderTargetBitmap(W, H, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(dv);
        bmp.Freeze();
        return bmp;
    }

    /// <summary>そのまま保存できる形(PNG の Base64)で作る</summary>
    public static string? Encode(string name, string kind, BitmapSource? preview = null)
    {
        try { return PresetThumb.Encode(Render(name, kind, preview)); }
        catch { return null; }
    }
}
