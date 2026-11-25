using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>色の計算(主な色の取り出し・色の近さ)。人の見た目に近い Lab 色空間で比べます</summary>
public static class ColorTools
{
    public static bool TryParse(string? text, out byte r, out byte g, out byte b)
    {
        r = g = b = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var t = text.Trim().TrimStart('#');
        if (t.Length == 3) t = string.Concat(t.Select(c => $"{c}{c}"));
        if (t.Length == 8) t = t.Substring(2);
        if (t.Length != 6) return false;
        try
        {
            r = Convert.ToByte(t.Substring(0, 2), 16);
            g = Convert.ToByte(t.Substring(2, 2), 16);
            b = Convert.ToByte(t.Substring(4, 2), 16);
            return true;
        }
        catch { return false; }
    }

    /// <summary>色あい(0〜360)・鮮やかさ(0〜1)・明るさ(0〜1)から、赤・緑・青を作る</summary>
    public static void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
    {
        h = ((h % 360) + 360) % 360;
        double c = v * s, x = c * (1 - Math.Abs(h / 60 % 2 - 1)), m = v - c;
        double r1 = 0, g1 = 0, b1 = 0;
        switch ((int)(h / 60) % 6)
        {
            case 0: r1 = c; g1 = x; break;
            case 1: r1 = x; g1 = c; break;
            case 2: g1 = c; b1 = x; break;
            case 3: g1 = x; b1 = c; break;
            case 4: r1 = x; b1 = c; break;
            default: r1 = c; b1 = x; break;
        }
        r = (byte)Math.Round((r1 + m) * 255); g = (byte)Math.Round((g1 + m) * 255); b = (byte)Math.Round((b1 + m) * 255);
    }

    public static void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
    {
        double rf = r / 255.0, gf = g / 255.0, bf = b / 255.0;
        double max = Math.Max(rf, Math.Max(gf, bf)), min = Math.Min(rf, Math.Min(gf, bf)), d = max - min;
        v = max;
        s = max <= 0 ? 0 : d / max;
        if (d <= 0) h = 0;
        else if (max == rf) h = 60 * (((gf - bf) / d % 6 + 6) % 6);
        else if (max == gf) h = 60 * ((bf - rf) / d + 2);
        else h = 60 * ((rf - gf) / d + 4);
    }

    public static string ToHex(byte r, byte g, byte b) => $"{r:X2}{g:X2}{b:X2}";

    public static SolidColorBrush Brush(string hex)
    {
        TryParse(hex, out var r, out var g, out var b);
        var br = new SolidColorBrush(Color.FromRgb(r, g, b));
        br.Freeze();
        return br;
    }

    public static (double L, double A, double B) ToLab(byte r, byte g, byte b)
    {
        static double Lin(double c) { c /= 255.0; return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4); }
        double R = Lin(r), G = Lin(g), B = Lin(b);
        double x = (R * 0.4124 + G * 0.3576 + B * 0.1805) / 0.95047;
        double y = (R * 0.2126 + G * 0.7152 + B * 0.0722);
        double z = (R * 0.0193 + G * 0.1192 + B * 0.9505) / 1.08883;
        static double F(double t) => t > 0.008856 ? Math.Cbrt(t) : 7.787 * t + 16.0 / 116.0;
        double fx = F(x), fy = F(y), fz = F(z);
        return (116 * fy - 16, 500 * (fx - fy), 200 * (fy - fz));
    }

    public static double Distance(string hexA, string hexB)
    {
        if (!TryParse(hexA, out var r1, out var g1, out var b1) || !TryParse(hexB, out var r2, out var g2, out var b2)) return 999;
        var a = ToLab(r1, g1, b1);
        var b = ToLab(r2, g2, b2);
        return Math.Sqrt((a.L - b.L) * (a.L - b.L) + (a.A - b.A) * (a.A - b.A) + (a.B - b.B) * (a.B - b.B));
    }

    /// <summary>画像から、主な色を多い順に取り出す("RRGGBB")。似た色は1つにまとめる</summary>
    public static List<string> ExtractPalette(BitmapSource source, int count)
    {
        var result = new List<string>();
        try
        {
            BitmapSource bmp = source;
            if (bmp.Format != PixelFormats.Bgra32)
                bmp = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
            int w = bmp.PixelWidth, h = bmp.PixelHeight;
            if (w <= 0 || h <= 0) return result;
            var px = new byte[w * h * 4];
            bmp.CopyPixels(px, w * 4, 0);

            // 1チャンネル4ビット(16段階)に丸めて数える
            var cnt = new int[4096];
            var sr = new long[4096]; var sg = new long[4096]; var sb = new long[4096];
            long total = 0;
            for (int i = 0; i < px.Length; i += 4)
            {
                if (px[i + 3] < 128) continue;
                int bb = px[i], gg = px[i + 1], rr = px[i + 2];
                int k = (rr >> 4) << 8 | (gg >> 4) << 4 | (bb >> 4);
                cnt[k]++; sr[k] += rr; sg[k] += gg; sb[k] += bb; total++;
            }
            if (total == 0) return result;

            var bins = Enumerable.Range(0, 4096).Where(k => cnt[k] > 0).OrderByDescending(k => cnt[k]).ToList();
            var picked = new List<(byte r, byte g, byte b)>();
            var weights = new List<long>();
            foreach (var k in bins)
            {
                byte r = (byte)(sr[k] / cnt[k]), g = (byte)(sg[k] / cnt[k]), b = (byte)(sb[k] / cnt[k]);
                var lab = ToLab(r, g, b);
                int near = -1;
                for (int i = 0; i < picked.Count; i++)
                {
                    var p = ToLab(picked[i].r, picked[i].g, picked[i].b);
                    if (Math.Sqrt((lab.L - p.L) * (lab.L - p.L) + (lab.A - p.A) * (lab.A - p.A) + (lab.B - p.B) * (lab.B - p.B)) < 16) { near = i; break; }
                }
                if (near >= 0) { weights[near] += cnt[k]; continue; }
                picked.Add((r, g, b));
                weights.Add(cnt[k]);
            }
            // ごく小さい色(全体の1%未満)は、主な色に入れない。ただし最低1色は残す
            var order = Enumerable.Range(0, picked.Count).OrderByDescending(i => weights[i]).ToList();
            foreach (var i in order)
            {
                if (result.Count >= count) break;
                if (result.Count > 0 && weights[i] * 100 < total) continue;
                result.Add(ToHex(picked[i].r, picked[i].g, picked[i].b));
            }
        }
        catch { }
        return result;
    }
}
