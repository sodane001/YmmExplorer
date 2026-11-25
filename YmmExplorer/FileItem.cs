using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>読み込み済みサムネイルの保管庫(古いものから捨てて、メモリを使いすぎないようにする)</summary>
internal static class ThumbCache
{
    const int Max = 400;
    static readonly object gate = new();
    static readonly Dictionary<string, ImageSource> map = new();
    static readonly Queue<string> order = new();

    public static bool TryGet(string key, out ImageSource? image)
    {
        lock (gate)
        {
            var ok = map.TryGetValue(key, out var v);
            image = v;
            return ok;
        }
    }

    public static void Put(string key, ImageSource image)
    {
        lock (gate)
        {
            if (!map.ContainsKey(key)) order.Enqueue(key);
            map[key] = image;
            while (map.Count > Max && order.Count > 0) map.Remove(order.Dequeue());
        }
    }
}

public sealed class FileItem : NotifyBase
{
    static readonly HashSet<string> Video = new(StringComparer.OrdinalIgnoreCase)
        { "mp4", "mov", "avi", "mkv", "webm", "wmv", "flv", "m4v", "mpg", "mpeg", "mts", "m2ts" };
    static readonly HashSet<string> Image = new(StringComparer.OrdinalIgnoreCase)
        { "png", "jpg", "jpeg", "gif", "bmp", "webp", "tif", "tiff" };
    static readonly HashSet<string> Audio = new(StringComparer.OrdinalIgnoreCase)
        { "wav", "mp3", "flac", "ogg", "m4a", "aac", "mid", "midi" };
    // Windowsのシェルがサムネイルを作れるその他のファイル
    static readonly HashSet<string> ShellThumbOther = new(StringComparer.OrdinalIgnoreCase)
        { "docx", "doc", "xlsx", "xls", "pptx", "ppt", "psd", "ai", "svg", "heic", "heif", "avif" };

    static Brush B(string hex)
    {
        var b = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
        b.Freeze();
        return b;
    }

    static readonly Brush FolderBrush = B("#FFC83D");
    static readonly Brush VideoBrush = B("#B48CFF");
    static readonly Brush ImageBrush = B("#5CD6A5");
    static readonly Brush AudioBrush = B("#FF9F6B");
    static readonly Brush PdfBrush = B("#FF7B72");
    static readonly Brush FileBrush = B("#8FB4FF");

    public string Name { get; init; } = "";
    public string FullPath { get; init; } = "";
    public bool IsDirectory { get; init; }
    public string Extension { get; init; } = "";
    public DateTime Modified { get; init; }
    public DateTime Created { get; init; }
    public long Size { get; init; }
    public string TypeText { get; init; } = "";
    public string SizeText { get; init; } = "";
    public string ModifiedText { get; init; } = "";
    public string CreatedText { get; init; } = "";
    public string AccessedText { get; init; } = "";
    public string AttributesText { get; init; } = "";
    public string IconGlyph { get; init; } = "\uE7C3";
    public Brush IconBrush { get; init; } = Brushes.Gray;

    /// <summary>クラウドのみ(未ダウンロード)。開く・サムネイルを作る・情報を読むとダウンロードが始まる</summary>
    public bool IsCloudOnly { get; init; }

    /// <summary>一覧に出す名前。クラウドのみのファイルには ☁ を付ける</summary>
    public string DisplayName => IsCloudOnly ? "☁ " + Name : Name;

    public bool IsImage { get; init; }
    public bool IsAudio { get; init; }
    public bool IsVideo { get; init; }
    public bool IsPdf { get; init; }
    public bool CanPreview => IsImage || IsAudio || IsVideo || IsPdf;
    // クラウドのみのファイルは、設定で許可しない限りサムネイルを作らない(作ると勝手にダウンロードされるため)
    bool CanThumb => !IsDirectory
        && (IsImage || IsAudio || IsVideo || IsPdf || ShellThumbOther.Contains(Extension))
        && (!IsCloudOnly || ExplorerSettings.Instance.CloudThumbnails);

    // ---- サムネイル(アイコン表示・ホバー・詳細パネル用)。最初に参照された時に非同期で読み込む ----
    static readonly SemaphoreSlim ImageGate = new(4);
    bool thumbLoading, thumbFailed;

    string CacheKey => FullPath + "|" + Modified.Ticks;

    public ImageSource? Thumbnail
    {
        get
        {
            if (!CanThumb) return null;
            if (ThumbCache.TryGet(CacheKey, out var cached)) return cached;
            if (!thumbLoading && !thumbFailed)
            {
                thumbLoading = true;
                _ = LoadThumbAsync();
            }
            return null;
        }
    }

    async Task LoadThumbAsync()
    {
        ImageSource? result = null;
        try
        {
            // 1. 画像は自前でデコード(速い)
            if (IsImage)
            {
                await ImageGate.WaitAsync();
                try { result = await Task.Run(LoadImageThumb); }
                finally { ImageGate.Release(); }
            }
            // 2. 動画・PDF・Office・PSDなどはWindowsのシェルに作ってもらう(mp4の1コマ目など)
            result ??= await ShellThumbnail.GetAsync(FullPath, 320);
            // 3. PDFでシェルのサムネイルが無い環境では、1ページ目を自分で描画
            if (result == null && IsPdf) result = await PdfDoc.RenderFirstPageAsync(FullPath, 320);
        }
        catch { result = null; }

        if (result != null) ThumbCache.Put(CacheKey, result);
        else thumbFailed = true;
        thumbLoading = false;
        Application.Current?.Dispatcher.BeginInvoke(new Action(() => Raise(nameof(Thumbnail))));
    }

    ImageSource? LoadImageThumb()
    {
        try
        {
            var b = new BitmapImage();
            b.BeginInit();
            b.CacheOption = BitmapCacheOption.OnLoad;
            b.DecodePixelWidth = 320;
            b.UriSource = new Uri(FullPath);
            b.EndInit();
            b.Freeze();
            return b;
        }
        catch { return null; }
    }

    IReadOnlyList<TagInfo> tags = Array.Empty<TagInfo>();
    public IReadOnlyList<TagInfo> Tags => tags;

    public void ReloadTags(TagStore store)
    {
        tags = store.TagsOf(FullPath);
        Raise(nameof(Tags));
    }

    public static string FormatSize(long bytes)
    {
        string[] u = { "B", "KB", "MB", "GB", "TB" };
        double v = bytes;
        int i = 0;
        while (v >= 1024 && i < u.Length - 1) { v /= 1024; i++; }
        return i == 0 ? $"{bytes} B" : $"{v:0.#} {u[i]}";
    }

    static string AttrText(FileAttributes a)
    {
        var l = new List<string>();
        if (a.HasFlag(FileAttributes.ReadOnly)) l.Add("読み取り専用");
        if (a.HasFlag(FileAttributes.Hidden)) l.Add("隠しファイル");
        if (a.HasFlag(FileAttributes.System)) l.Add("システム");
        if (a.HasFlag(FileAttributes.Compressed)) l.Add("圧縮");
        if (a.HasFlag(FileAttributes.Encrypted)) l.Add("暗号化");
        return l.Count == 0 ? "なし" : string.Join("、", l);
    }

    public static FileItem From(FileSystemInfo f, TagStore store)
    {
        var isDir = f is DirectoryInfo;
        var ext = isDir ? "" : f.Extension.TrimStart('.').ToLowerInvariant();
        var size = f is FileInfo fi ? fi.Length : 0;
        var isPdf = !isDir && ext == "pdf";

        string type, glyph;
        Brush brush;
        if (isDir) { type = "フォルダー"; glyph = "\uE8B7"; brush = FolderBrush; }
        else if (Video.Contains(ext)) { type = "動画"; glyph = "\uE714"; brush = VideoBrush; }
        else if (Image.Contains(ext)) { type = "画像"; glyph = "\uE91B"; brush = ImageBrush; }
        else if (Audio.Contains(ext)) { type = "音声"; glyph = "\uE8D6"; brush = AudioBrush; }
        else if (isPdf) { type = "PDF"; glyph = "\uE8A5"; brush = PdfBrush; }
        else
        {
            type = ext.Length > 0 ? ext.ToUpperInvariant() + " ファイル" : "ファイル";
            glyph = "\uE7C3"; brush = FileBrush;
        }

        return new FileItem
        {
            Name = f.Name,
            FullPath = f.FullName,
            IsDirectory = isDir,
            Extension = ext,
            Modified = f.LastWriteTime,
            ModifiedText = f.LastWriteTime.ToString("yyyy/MM/dd HH:mm"),
            Created = f.CreationTime,
            CreatedText = f.CreationTime.ToString("yyyy/MM/dd HH:mm"),
            AccessedText = f.LastAccessTime.ToString("yyyy/MM/dd HH:mm"),
            AttributesText = AttrText(f.Attributes),
            Size = size,
            SizeText = isDir ? "" : FormatSize(size),
            TypeText = type,
            IconGlyph = glyph,
            IconBrush = brush,
            IsImage = !isDir && Image.Contains(ext),
            IsAudio = !isDir && Audio.Contains(ext),
            IsVideo = !isDir && Video.Contains(ext),
            IsPdf = isPdf,
            IsCloudOnly = !isDir && CloudDetector.IsCloudOnly(f.Attributes),
            tags = store.TagsOf(f.FullName),
        };
    }
}
