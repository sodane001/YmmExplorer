using System.IO;
using System.Windows.Media.Imaging;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;

namespace YmmExplorer;

/// <summary>Windows標準のPDFエンジン(Windows.Data.Pdf)でPDFを画像に描画します。追加ライブラリは不要です</summary>
public sealed class PdfDoc
{
    readonly PdfDocument doc;

    PdfDoc(PdfDocument d) => doc = d;

    public int PageCount => (int)doc.PageCount;

    /// <summary>パスワード付き・破損ファイルは例外になります</summary>
    public static async Task<PdfDoc> OpenAsync(string path)
    {
        var file = await StorageFile.GetFileFromPathAsync(path);
        var d = await PdfDocument.LoadFromFileAsync(file);
        return new PdfDoc(d);
    }

    /// <summary>index ページ目(0始まり)を、幅 width ピクセル程度の画像にします</summary>
    public async Task<BitmapSource> RenderAsync(int index, int width)
    {
        using var page = doc.GetPage((uint)index);
        var size = page.Size;
        double scale = Math.Min(width / Math.Max(size.Width, 1), 9000 / Math.Max(size.Height, 1));
        var opt = new PdfPageRenderOptions
        {
            DestinationWidth = (uint)Math.Max(1, size.Width * scale),
            DestinationHeight = (uint)Math.Max(1, size.Height * scale),
            BackgroundColor = new global::Windows.UI.Color { A = 255, R = 255, G = 255, B = 255 },
        };

        using var ms = new InMemoryRandomAccessStream();
        await page.RenderToStreamAsync(ms, opt);
        ms.Seek(0);

        using var stream = ms.AsStreamForRead();
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = stream;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    /// <summary>サムネイル用: 1ページ目だけを描画(失敗時はnull)</summary>
    public static async Task<BitmapSource?> RenderFirstPageAsync(string path, int width)
    {
        try
        {
            var d = await OpenAsync(path);
            return d.PageCount == 0 ? null : await d.RenderAsync(0, width);
        }
        catch { return null; }
    }
}
