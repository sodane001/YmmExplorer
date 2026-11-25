using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>素材データベースの素材1つ分(Eagleのアイテムに相当)</summary>
public sealed class MaterialEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>表示名(あとから変えられる。ファイル自体の名前は変わらない)</summary>
    public string Name { get; set; } = "";

    /// <summary>ライブラリーフォルダーからの相対パス(files\ID\元のファイル名)</summary>
    public string FileName { get; set; } = "";

    public string Ext { get; set; } = "";

    /// <summary>image / video / audio / other</summary>
    public string Kind { get; set; } = "other";

    public long Size { get; set; }

    /// <summary>動画・音声の長さ(秒)。0なら未取得</summary>
    public double DurationSec { get; set; }

    /// <summary>画像・動画の大きさ(ピクセル)。0なら未取得</summary>
    public int Width { get; set; }
    public int Height { get; set; }

    /// <summary>主な色(多い順。"RRGGBB")。色での検索に使う</summary>
    public List<string> Palette { get; set; } = new();

    /// <summary>ゴミ箱に入っている素材</summary>
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    /// <summary>素材の出どころのURL(Eagleの「ソース」)</summary>
    public string Url { get; set; } = "";
    public List<string> Tags { get; set; } = new();

    /// <summary>入っているフォルダー(空なら「未分類」)</summary>
    public string Folder { get; set; } = "";

    /// <summary>評価(0〜5)</summary>
    public int Rating { get; set; }

    public string Memo { get; set; } = "";
    public DateTime Added { get; set; } = DateTime.Now;

    /// <summary>取り込む前の場所(参考用)</summary>
    public string? SourcePath { get; set; }

    /// <summary>重複を見つけるための指紋</summary>
    public string? Hash { get; set; }
}

public sealed class MaterialDbFile
{
    public List<MaterialEntry> Items { get; set; } = new();
    public List<string> Folders { get; set; } = new();
}

public enum ImportMode { Copy, Move }

public sealed class ImportOptions
{
    public ImportMode Mode { get; set; } = ImportMode.Copy;

    /// <summary>フォルダーごと取り込む時、そのフォルダーの名前をタグにする</summary>
    public bool TagsFromFolders { get; set; } = true;

    /// <summary>取り込み先のフォルダー(空なら未分類)</summary>
    public string Folder { get; set; } = "";

    /// <summary>取り込む素材すべてに付けるタグ</summary>
    public List<string> ExtraTags { get; set; } = new();
}

public sealed record ImportResult(int Added, int Duplicates, int Failed, List<MaterialEntry> NewEntries, int Restored = 0);

/// <summary>
/// 素材データベースの保存先。ライブラリーフォルダーの中に、次の形で保存する(素材は、ここへコピー/移動する)。
///   materials.json      素材の情報(タグ・評価・メモ・フォルダーなど)
///   files\ID\ファイル   取り込んだ素材のファイル
///   thumbs\ID.png       サムネイル
/// </summary>
public sealed class MaterialStore
{

    static readonly HashSet<string> ImageExt = new(StringComparer.OrdinalIgnoreCase)
    {
        "png", "jpg", "jpeg", "jpe", "jfif", "gif", "bmp", "dib", "webp", "tif", "tiff", "heic", "heif", "avif", "svg", "psd", "ico",
        "jxr", "wdp", "hdp", "dds", "tga", "apng", "jxl",
    };
    static readonly HashSet<string> VideoExt = new(StringComparer.OrdinalIgnoreCase)
    {
        "mp4", "mov", "avi", "mkv", "webm", "wmv", "flv", "m4v", "mpg", "mpeg", "mts", "m2ts", "ts",
        "3gp", "3g2", "asf", "ogv", "vob", "m2v", "divx", "f4v", "mp4v", "wm", "mxf",
    };
    static readonly HashSet<string> AudioExt = new(StringComparer.OrdinalIgnoreCase)
    {
        "wav", "mp3", "flac", "ogg", "m4a", "aac", "wma", "mid", "midi", "opus",
        "aif", "aiff", "aifc", "ac3", "amr", "mka", "oga", "mp2", "m4b", "m4r", "weba", "au", "snd", "wv", "ape",
    };
    static readonly HashSet<string> TextExt = new(StringComparer.OrdinalIgnoreCase)
    {
        "txt", "md", "markdown", "json", "csv", "tsv", "xml", "html", "htm", "css", "js", "ts", "cs", "py", "ini", "cfg", "conf",
        "yaml", "yml", "toml", "log", "srt", "ass", "ssa", "vtt", "lrc", "bat", "cmd", "ps1", "sh", "lua", "anm", "obj", "exo",
        "sql", "c", "cpp", "h", "hpp", "java", "rs", "go", "xaml", "csproj", "sln", "ymmp", "ymmt", "url", "reg", "tex", "rtf",
    };

    // 注意: Instance は、上の拡張子の一覧より後に置くこと(static は上から順に作られる。
    // 先に置くと、コンストラクターから拡張子の一覧を使った時に、まだ空(null)でエラーになる)
    public static MaterialStore Instance { get; } = new();

    /// <summary>大きなプレビューでの見せ方: image / video / audio / text / pdf / other</summary>
    public static string ViewModeOf(string ext)
    {
        ext = (ext ?? "").TrimStart('.');
        if (string.Equals(ext, "pdf", StringComparison.OrdinalIgnoreCase)) return "pdf";
        // 「.ts」は動画(MPEG-TS)として扱う
        if (VideoExt.Contains(ext)) return "video";
        if (AudioExt.Contains(ext)) return "audio";
        if (ImageExt.Contains(ext)) return "image";
        if (TextExt.Contains(ext)) return "text";
        return "other";
    }

    readonly object gate = new();
    MaterialDbFile data = new();
    string root = "";

    public event Action? Changed;

    public string Root => root;
    public static string DefaultRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "MaterialLibrary");

    string JsonPath => Path.Combine(root, "materials.json");
    string ThumbsDir => Path.Combine(root, "thumbs");

    MaterialStore() { Open(ExplorerSettings.Instance.MaterialLibraryPath); }

    public static string KindOf(string ext)
        => ImageExt.Contains(ext) ? "image" : VideoExt.Contains(ext) ? "video" : AudioExt.Contains(ext) ? "audio" : "other";

    /// <summary>ライブラリーを開く(フォルダーが空なら、新しく作る。すでにあれば、その中身を読み込む)</summary>
    public void Open(string? folder)
    {
        var target = string.IsNullOrWhiteSpace(folder) ? DefaultRoot : folder!;
        lock (gate)
        {
            root = target;
            data = new();
            try
            {
                Directory.CreateDirectory(root);
                if (File.Exists(JsonPath))
                {
                    MaterialDbFile? loaded = null;
                    for (int attempt = 0; attempt < 4 && loaded == null; attempt++)
                    {
                        try { loaded = JsonSerializer.Deserialize<MaterialDbFile>(File.ReadAllText(JsonPath)); }
                        catch { System.Threading.Thread.Sleep(150); }
                    }
                    data = loaded ?? new();
                }
            }
            catch { data = new(); }
        }
        // 対応する拡張子が増えた時のために、種類(画像・動画・音声)を拡張子から決め直す
        bool fixedKind = false;
        try
        {
            lock (gate)
                foreach (var e in data.Items)
                {
                    var k = KindOf((e.Ext ?? "").TrimStart('.'));
                    if (k != e.Kind) { e.Kind = k; fixedKind = true; }
                }
        }
        catch { }
        if (fixedKind) Save();
        Watch();
        Changed?.Invoke();
    }

    // ---- 別のアプリ(YMM4 マネージャー / YMM4の中のこのプラグイン)が同じライブラリーを変えた時に、読み直す
    FileSystemWatcher? watcher;
    DateTime lastOwnSave;
    System.Threading.Timer? reloadTimer;

    void Watch()
    {
        try
        {
            watcher?.Dispose();
            watcher = new FileSystemWatcher(root, "materials.json")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true,
            };
            watcher.Changed += (_, _) => OnExternalChange();
            watcher.Created += (_, _) => OnExternalChange();
            watcher.Renamed += (_, _) => OnExternalChange();
        }
        catch { watcher = null; }
    }

    void OnExternalChange()
    {
        if ((DateTime.Now - lastOwnSave).TotalSeconds < 2) return;      // 自分で保存した時
        reloadTimer?.Dispose();
        reloadTimer = new System.Threading.Timer(_ =>
        {
            var app = Application.Current;
            if (app == null) ReloadFromDisk();
            else app.Dispatcher.BeginInvoke(new Action(ReloadFromDisk));
        }, null, 500, System.Threading.Timeout.Infinite);
    }

    /// <summary>ファイルから読み直す。書き込みの途中などで読めなかった時は、今の内容のまま(空にはしない)</summary>
    public void ReloadFromDisk()
    {
        for (int attempt = 0; attempt < 4; attempt++)
        {
            try
            {
                if (!File.Exists(JsonPath)) return;
                var fresh = JsonSerializer.Deserialize<MaterialDbFile>(File.ReadAllText(JsonPath));
                if (fresh == null) return;
                lock (gate) data = fresh;
                Changed?.Invoke();
                return;
            }
            catch { System.Threading.Thread.Sleep(150); }
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(root);
            string text;
            lock (gate) text = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            // いったん別の名前で書いてから入れ替える(書き込み途中の中途半端なファイルを、ほかのアプリが読まないように)
            lastOwnSave = DateTime.Now;
            var tmp = JsonPath + ".tmp";
            File.WriteAllText(tmp, text);
            File.Move(tmp, JsonPath, true);
            lastOwnSave = DateTime.Now;
        }
        catch { }
    }

    /// <summary>ゴミ箱に入っていない素材</summary>
    public IReadOnlyList<MaterialEntry> All()
    {
        lock (gate) return data.Items.Where(e => !e.IsDeleted).ToList();
    }

    /// <summary>ゴミ箱の中の素材</summary>
    public IReadOnlyList<MaterialEntry> Trashed()
    {
        lock (gate) return data.Items.Where(e => e.IsDeleted).ToList();
    }

    /// <summary>ゴミ箱へ入れる(ファイルは消さない)。入れた数を返す</summary>
    public int Trash(IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids);
        int n = 0;
        lock (gate)
            foreach (var e in data.Items.Where(x => set.Contains(x.Id) && !x.IsDeleted)) { e.IsDeleted = true; e.DeletedAt = DateTime.Now; n++; }
        if (n > 0) { Save(); Changed?.Invoke(); }
        return n;
    }

    /// <summary>ゴミ箱から元に戻す</summary>
    public int Restore(IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids);
        int n = 0;
        lock (gate)
            foreach (var e in data.Items.Where(x => set.Contains(x.Id) && x.IsDeleted)) { e.IsDeleted = false; e.DeletedAt = null; n++; }
        if (n > 0) { Save(); Changed?.Invoke(); }
        return n;
    }

    /// <summary>ゴミ箱を空にする(ファイルも消える)</summary>
    public int EmptyTrash() => Remove(Trashed().Select(e => e.Id).ToList());

    /// <summary>素材の出どころURLを保存する</summary>
    public void SetUrl(string id, string url)
    {
        lock (gate) { var e = data.Items.FirstOrDefault(x => x.Id == id); if (e != null) e.Url = url ?? ""; }
        Save();
    }

    public IReadOnlyList<string> Folders()
    {
        lock (gate) return data.Folders.ToList();
    }

    public string FullPath(MaterialEntry e) => Path.Combine(root, e.FileName);
    public string ThumbPath(MaterialEntry e) => Path.Combine(ThumbsDir, e.Id + ".png");

    // ------------------------------------------------------------------ 取り込み
    static string SafeName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
        if (name.Length > 120)
        {
            var ext = Path.GetExtension(name);
            name = name.Substring(0, 120 - ext.Length) + ext;
        }
        return name;
    }

    /// <summary>大きなファイルでも速いように、大きさと、先頭・末尾の一部から指紋を作る</summary>
    static string QuickHash(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sha = SHA1.Create();
        var len = fs.Length;
        const int chunk = 256 * 1024;
        var buffer = new byte[(int)Math.Min(len, chunk)];
        var read = fs.Read(buffer, 0, buffer.Length);
        sha.TransformBlock(buffer, 0, read, null, 0);
        if (len > chunk * 2)
        {
            fs.Seek(-chunk, SeekOrigin.End);
            read = fs.Read(buffer, 0, buffer.Length);
            sha.TransformBlock(buffer, 0, read, null, 0);
        }
        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return len + "-" + Convert.ToHexString(sha.Hash!);
    }

    /// <summary>ファイルとフォルダー(中のファイルすべて)を取り込む。進み具合は progress に知らせる</summary>
    public async Task<ImportResult> ImportAsync(IEnumerable<string> paths, ImportOptions options, Action<string>? progress)
    {
        // 取り込むファイルを集める(フォルダーは、中のファイルすべて)
        var files = new List<(string Path, List<string> Tags)>();
        foreach (var p in paths)
        {
            try
            {
                if (File.Exists(p)) files.Add((p, new List<string>()));
                else if (Directory.Exists(p))
                {
                    var top = new DirectoryInfo(p);
                    foreach (var f in top.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        if (f.Attributes.HasFlag(FileAttributes.Hidden) || f.Attributes.HasFlag(FileAttributes.System)) continue;
                        var tags = new List<string>();
                        if (options.TagsFromFolders)
                        {
                            // 取り込んだフォルダーの名前と、その下のフォルダー名をタグにする
                            tags.Add(top.Name);
                            var rel = Path.GetRelativePath(top.FullName, f.DirectoryName ?? top.FullName);
                            if (rel != ".") tags.AddRange(rel.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries));
                        }
                        files.Add((f.FullName, tags));
                    }
                }
            }
            catch { }
        }

        int added = 0, dups = 0, failed = 0, index = 0, restored = 0;
        var newEntries = new List<MaterialEntry>();
        foreach (var (path, folderTags) in files)
        {
            index++;
            progress?.Invoke($"取り込み中 {index} / {files.Count}: {Path.GetFileName(path)}");
            try
            {
                var info = new FileInfo(path);
                var hash = await Task.Run(() => QuickHash(path));
                MaterialEntry? existing;
                lock (gate) existing = data.Items.FirstOrDefault(e => e.Hash == hash);
                if (existing != null)
                {
                    if (!existing.IsDeleted) { dups++; continue; }
                    // ゴミ箱にある素材をもう一度取り込んだ時: タグなどを初期の状態にして、ゴミ箱から戻す
                    lock (gate)
                    {
                        existing.IsDeleted = false;
                        existing.DeletedAt = null;
                        existing.Name = Path.GetFileNameWithoutExtension(info.Name);
                        existing.Tags = folderTags.Concat(options.ExtraTags).Where(t => !string.IsNullOrWhiteSpace(t))
                                                  .Select(t => t.Trim()).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
                        existing.Folder = options.Folder;
                        existing.Rating = 0;
                        existing.Memo = "";
                        existing.Url = "";
                        existing.Added = DateTime.Now;
                        existing.SourcePath = path;
                    }
                    newEntries.Add(existing);
                    restored++;
                    continue;
                }

                var id = Guid.NewGuid().ToString("N");
                var rel = Path.Combine("files", id, SafeName(info.Name));
                var dest = Path.Combine(root, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                await Task.Run(() =>
                {
                    if (options.Mode == ImportMode.Move) File.Move(path, dest);
                    else File.Copy(path, dest);
                });

                var ext = info.Extension.TrimStart('.').ToLowerInvariant();
                var entry = new MaterialEntry
                {
                    Id = id,
                    Name = Path.GetFileNameWithoutExtension(info.Name),
                    FileName = rel,
                    Ext = ext,
                    Kind = KindOf(ext),
                    Size = info.Length,
                    Folder = options.Folder,
                    Tags = folderTags.Concat(options.ExtraTags).Where(t => !string.IsNullOrWhiteSpace(t))
                                     .Select(t => t.Trim()).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList(),
                    Added = DateTime.Now,
                    SourcePath = path,
                    Hash = hash,
                };
                await MakeThumbnailAsync(entry);
                lock (gate) data.Items.Add(entry);
                newEntries.Add(entry);
                added++;
            }
            catch { failed++; }
        }
        if (added > 0 || restored > 0) { Save(); Changed?.Invoke(); }
        return new ImportResult(added, dups, failed, newEntries, restored);
    }

    static BitmapSource? LoadScaled(string path, int width)
    {
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.DecodePixelWidth = width;
            bmp.UriSource = new Uri(path);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    /// <summary>サムネイルを作って、thumbs\ID.png に保存する(音声は波形画像、動画・音声は長さも調べる)</summary>
    async Task MakeThumbnailAsync(MaterialEntry e)
    {
        try
        {
            if (e.Kind == "audio" || e.Kind == "video") await FillMediaInfoAsync(e);
            if (e.Kind == "audio") return;
            var src = FullPath(e);
            BitmapSource? bmp = null;
            if (e.Kind == "image") bmp = await Task.Run(() => LoadScaled(src, 320));
            bmp ??= await ShellThumbnail.GetAsync(src, 320);
            if (bmp == null) return;

            SavePng(bmp, ThumbPath(e));
        }
        catch { }
        try { await AnalyzeAsync(e); } catch { }
    }

    void SavePng(BitmapSource bmp, string path)
    {
        Directory.CreateDirectory(ThumbsDir);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bmp));
        using var fs = File.Create(path);
        encoder.Save(fs);
    }

    // ------------------------------------------------------------------ 長さ・波形
    internal static async Task<double> ReadDurationAsync(string path, bool audio)
    {
        try
        {
            var file = await global::Windows.Storage.StorageFile.GetFileFromPathAsync(path).AsTask();
            if (audio)
            {
                var p = await file.Properties.GetMusicPropertiesAsync().AsTask();
                return p.Duration.TotalSeconds;
            }
            var v = await file.Properties.GetVideoPropertiesAsync().AsTask();
            return v.Duration.TotalSeconds;
        }
        catch { return 0; }
    }

    static async Task<(int w, int h)> ReadVideoSizeAsync(string path)
    {
        try
        {
            var file = await global::Windows.Storage.StorageFile.GetFileFromPathAsync(path).AsTask();
            var v = await file.Properties.GetVideoPropertiesAsync().AsTask();
            return ((int)v.Width, (int)v.Height);
        }
        catch { return (0, 0); }
    }

    /// <summary>大きさ(画像・動画)と主な色(画像・動画)を調べる。すでに調べてあるものは飛ばす</summary>
    async Task AnalyzeAsync(MaterialEntry e)
    {
        if (e.Kind != "image" && e.Kind != "video") return;
        var src = FullPath(e);
        if (e.Width <= 0 || e.Height <= 0)
        {
            if (e.Kind == "image")
            {
                var size = await Task.Run(() =>
                {
                    try
                    {
                        using var fs = File.OpenRead(src);
                        var dec = BitmapDecoder.Create(fs, BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                        var f = dec.Frames[0];
                        return (f.PixelWidth, f.PixelHeight);
                    }
                    catch { return (0, 0); }
                });
                e.Width = size.Item1; e.Height = size.Item2;
            }
            else
            {
                var size = await ReadVideoSizeAsync(src);
                e.Width = size.w; e.Height = size.h;
            }
        }
        if (e.Palette.Count == 0)
        {
            var thumb = ThumbPath(e);
            var palette = await Task.Run(() =>
            {
                var bmp = File.Exists(thumb) ? LoadScaled(thumb, 64) : (e.Kind == "image" ? LoadScaled(src, 64) : null);
                return bmp == null ? new List<string>() : ColorTools.ExtractPalette(bmp, 6);
            });
            e.Palette = palette;
        }
    }

    /// <summary>長さを調べる。音声なら波形のサムネイルも作る</summary>
    async Task FillMediaInfoAsync(MaterialEntry e)
    {
        var src = FullPath(e);
        if (!File.Exists(src)) return;
        bool audio = e.Kind == "audio";
        if (e.DurationSec <= 0) e.DurationSec = await ReadDurationAsync(src, audio);
        if (!audio || File.Exists(ThumbPath(e))) return;
        var peaks = await ComputePeaksAsync(src, 96);
        if (peaks == null) return;
        var bmp = await Task.Run(() => DrawWaveform(peaks, 312, 216));
        SavePng(bmp, ThumbPath(e));
    }

    internal static async Task<float[]?> ComputePeaksAsync(string path, int buckets)
    {
        try
        {
            if (string.Equals(Path.GetExtension(path), ".wav", StringComparison.OrdinalIgnoreCase))
            {
                var direct = await Task.Run(() => PeaksFromWav(path, buckets));
                if (direct != null) return direct;
            }
            // WAV以外(mp3/m4a/flacなど)は、一時的なWAVに変換して読む
            var tmp = Path.Combine(Path.GetTempPath(), "YmmExplorer_wave_" + Guid.NewGuid().ToString("N") + ".wav");
            try
            {
                File.WriteAllBytes(tmp, Array.Empty<byte>());
                var srcFile = await global::Windows.Storage.StorageFile.GetFileFromPathAsync(path).AsTask();
                var dstFile = await global::Windows.Storage.StorageFile.GetFileFromPathAsync(tmp).AsTask();
                var profile = global::Windows.Media.MediaProperties.MediaEncodingProfile.CreateWav(
                    global::Windows.Media.MediaProperties.AudioEncodingQuality.Medium);
                var transcoder = new global::Windows.Media.Transcoding.MediaTranscoder();
                var prepared = await transcoder.PrepareFileTranscodeAsync(srcFile, dstFile, profile).AsTask();
                if (!prepared.CanTranscode) return null;
                await prepared.TranscodeAsync().AsTask();
                return await Task.Run(() => PeaksFromWav(tmp, buckets));
            }
            finally { try { File.Delete(tmp); } catch { } }
        }
        catch { return null; }
    }

    /// <summary>PCM/float の WAV を読んで、buckets 個の区間ごとの最大振幅(0〜1)を返す</summary>
    static float[]? PeaksFromWav(string path, int buckets)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var br = new BinaryReader(fs);
            if (new string(br.ReadChars(4)) != "RIFF") return null;
            br.ReadInt32();
            if (new string(br.ReadChars(4)) != "WAVE") return null;
            int format = 0, channels = 1, bits = 16;
            long dataPos = -1, dataLen = 0;
            while (fs.Position + 8 <= fs.Length)
            {
                var id = new string(br.ReadChars(4));
                long len = br.ReadUInt32();
                long next = fs.Position + len + (len & 1);
                if (id == "fmt ")
                {
                    format = br.ReadUInt16();
                    channels = Math.Max(1, (int)br.ReadUInt16());
                    br.ReadInt32(); br.ReadInt32(); br.ReadUInt16();
                    bits = br.ReadUInt16();
                    if (format == 0xFFFE && len >= 26) { br.ReadUInt16(); br.ReadUInt16(); br.ReadUInt32(); format = br.ReadUInt16(); }
                }
                else if (id == "data")
                {
                    dataPos = fs.Position;
                    dataLen = (len == 0 || dataPos + len > fs.Length) ? fs.Length - dataPos : len;
                    break;
                }
                if (next > fs.Length) break;
                fs.Position = next;
            }
            if (dataPos < 0 || dataLen <= 0) return null;
            int bytes = bits / 8;
            if (bytes < 1 || bytes > 4 || (format != 1 && format != 3)) return null;
            long frames = dataLen / (bytes * channels);
            if (frames <= 0) return null;

            var peaks = new float[buckets];
            fs.Position = dataPos;
            const int chunkFrames = 8192;
            var buf = new byte[chunkFrames * bytes * channels];
            long done = 0;
            while (done < frames)
            {
                int want = (int)Math.Min(chunkFrames, frames - done);
                int got = fs.Read(buf, 0, want * bytes * channels) / (bytes * channels);
                if (got <= 0) break;
                for (int i = 0; i < got; i++)
                {
                    int bucket = (int)((done + i) * buckets / frames);
                    if (bucket >= buckets) bucket = buckets - 1;
                    float m = 0;
                    for (int c = 0; c < channels; c++)
                    {
                        int o = (i * channels + c) * bytes;
                        float v = bytes switch
                        {
                            1 => (buf[o] - 128) / 128f,
                            2 => BitConverter.ToInt16(buf, o) / 32768f,
                            3 => ((buf[o] << 8 | buf[o + 1] << 16 | buf[o + 2] << 24) >> 8) / 8388608f,
                            _ => format == 3 ? BitConverter.ToSingle(buf, o) : BitConverter.ToInt32(buf, o) / 2147483648f,
                        };
                        m = Math.Max(m, Math.Abs(v));
                    }
                    if (m > peaks[bucket]) peaks[bucket] = m;
                }
                done += got;
            }
            float max = peaks.Max();
            if (max <= 0.0001f) return peaks;
            for (int i = 0; i < buckets; i++) peaks[i] = (float)Math.Pow(Math.Min(1f, peaks[i] / max), 0.75);
            return peaks;
        }
        catch { return null; }
    }

    /// <summary>波形をピクセルで直接描いてPNG用の画像にする(UIスレッド不要)</summary>
    static BitmapSource DrawWaveform(float[] peaks, int w, int h)
    {
        var px = new byte[w * h * 4];
        for (int y = 0; y < h; y++)
        {
            double t = (double)y / (h - 1);
            byte r = (byte)(34 + 14 * t), g = (byte)(40 + 14 * t), b = (byte)(54 + 20 * t);
            for (int x = 0; x < w; x++)
            {
                int o = (y * w + x) * 4;
                px[o] = b; px[o + 1] = g; px[o + 2] = r; px[o + 3] = 255;
            }
        }
        int n = peaks.Length;
        double slot = (w - 24.0) / n;
        int barW = Math.Max(2, (int)Math.Floor(slot * 0.62));
        int mid = h / 2;
        for (int i = 0; i < n; i++)
        {
            int half = Math.Max(2, (int)(peaks[i] * (h / 2 - 20)));
            int x0 = 12 + (int)(i * slot);
            for (int y = mid - half; y <= mid + half; y++)
            {
                double k = Math.Abs(y - mid) / (double)Math.Max(1, half);
                byte cr = (byte)(60 + 40 * k), cg = (byte)(210 - 40 * k), cb = 255;
                for (int x = x0; x < x0 + barW && x < w; x++)
                {
                    int o = (y * w + x) * 4;
                    px[o] = cb; px[o + 1] = cg; px[o + 2] = cr; px[o + 3] = 255;
                }
            }
        }
        var bmp = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgra32, null, px, w * 4);
        bmp.Freeze();
        return bmp;
    }

    readonly HashSet<string> backfillTried = new();

    bool NeedsInfo(MaterialEntry e) => e.Kind switch
    {
        "audio" => e.DurationSec <= 0 || !File.Exists(ThumbPath(e)),
        "video" => e.DurationSec <= 0 || e.Width <= 0 || e.Palette.Count == 0,
        "image" => e.Width <= 0 || e.Palette.Count == 0,
        _ => false,
    };

    /// <summary>すでに取り込んである素材に、長さ・波形・大きさ・主な色をあとから付ける(1素材につき1回だけ試す)</summary>
    public async Task BackfillAsync()
    {
        List<MaterialEntry> targets;
        lock (gate) targets = data.Items.Where(e => NeedsInfo(e) && backfillTried.Add(e.Id)).ToList();
        if (targets.Count == 0) return;
        bool any = false;
        int sinceSave = 0;
        foreach (var e in targets)
        {
            try
            {
                var before = (e.DurationSec, e.Width, e.Palette.Count, File.Exists(ThumbPath(e)));
                if (e.Kind == "audio" || e.Kind == "video") await FillMediaInfoAsync(e);
                await AnalyzeAsync(e);
                var after = (e.DurationSec, e.Width, e.Palette.Count, File.Exists(ThumbPath(e)));
                if (before != after) { any = true; sinceSave++; }
                if (sinceSave >= 20) { Save(); sinceSave = 0; }
            }
            catch { }
        }
        if (any) { Save(); Changed?.Invoke(); }
    }

    // ------------------------------------------------------------------ 編集
    public MaterialEntry? Find(string id)
    {
        lock (gate) return data.Items.FirstOrDefault(e => e.Id == id);
    }

    /// <summary>名前・メモなどの編集(一覧の作り直しが要らないもの)。保存だけする</summary>
    public void Touch() => Save();

    /// <summary>タグを追加する(すでにあるタグは、追加しない)</summary>
    public void AddTags(IEnumerable<string> ids, IEnumerable<string> tags)
    {
        var set = new HashSet<string>(ids);
        var add = tags.Select(t => t.Trim()).Where(t => t.Length > 0).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
        lock (gate)
            foreach (var e in data.Items.Where(x => set.Contains(x.Id)))
                foreach (var t in add)
                    if (!e.Tags.Contains(t, StringComparer.CurrentCultureIgnoreCase)) e.Tags.Add(t);
        Save();
        Changed?.Invoke();
    }

    public void RemoveTag(IEnumerable<string> ids, string tag)
    {
        var set = new HashSet<string>(ids);
        lock (gate)
            foreach (var e in data.Items.Where(x => set.Contains(x.Id)))
                e.Tags.RemoveAll(t => string.Equals(t, tag, StringComparison.CurrentCultureIgnoreCase));
        Save();
        Changed?.Invoke();
    }

    /// <summary>タグの名前を、すべての素材で変える(同じ名前のタグがあれば、まとめる)</summary>
    public void RenameTag(string oldName, string newName)
    {
        newName = newName.Trim();
        if (newName.Length == 0) return;
        lock (gate)
            foreach (var e in data.Items)
            {
                var i = e.Tags.FindIndex(t => string.Equals(t, oldName, StringComparison.CurrentCultureIgnoreCase));
                if (i < 0) continue;
                e.Tags.RemoveAt(i);
                if (!e.Tags.Contains(newName, StringComparer.CurrentCultureIgnoreCase)) e.Tags.Insert(i, newName);
            }
        Save();
        Changed?.Invoke();
    }

    public void SetRating(IEnumerable<string> ids, int rating)
    {
        var set = new HashSet<string>(ids);
        lock (gate)
            foreach (var e in data.Items.Where(x => set.Contains(x.Id))) e.Rating = Math.Clamp(rating, 0, 5);
        Save();
        Changed?.Invoke();
    }

    public void SetFolder(IEnumerable<string> ids, string folder)
    {
        var set = new HashSet<string>(ids);
        lock (gate)
            foreach (var e in data.Items.Where(x => set.Contains(x.Id))) e.Folder = folder;
        Save();
        Changed?.Invoke();
    }

    public bool AddFolder(string name)
    {
        name = name.Trim();
        if (name.Length == 0) return false;
        lock (gate)
        {
            if (data.Folders.Contains(name, StringComparer.CurrentCultureIgnoreCase)) return false;
            data.Folders.Add(name);
        }
        Save();
        Changed?.Invoke();
        return true;
    }

    /// <summary>フォルダーを消す(中の素材は、未分類に戻る)</summary>
    public void RemoveFolder(string name)
    {
        lock (gate)
        {
            data.Folders.RemoveAll(f => f == name);
            foreach (var e in data.Items.Where(x => x.Folder == name)) e.Folder = "";
        }
        Save();
        Changed?.Invoke();
    }

    public void RenameFolder(string oldName, string newName)
    {
        newName = newName.Trim();
        if (newName.Length == 0) return;
        lock (gate)
        {
            var i = data.Folders.IndexOf(oldName);
            if (i < 0 || data.Folders.Contains(newName, StringComparer.CurrentCultureIgnoreCase)) return;
            data.Folders[i] = newName;
            foreach (var e in data.Items.Where(x => x.Folder == oldName)) e.Folder = newName;
        }
        Save();
        Changed?.Invoke();
    }

    /// <summary>素材をライブラリーから消す(ファイルも消す)。消した数を返す</summary>
    public int Remove(IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids);
        List<MaterialEntry> gone;
        lock (gate)
        {
            gone = data.Items.Where(e => set.Contains(e.Id)).ToList();
            data.Items.RemoveAll(e => set.Contains(e.Id));
        }
        foreach (var e in gone)
        {
            try { var dir = Path.GetDirectoryName(FullPath(e)); if (dir != null && Directory.Exists(dir)) Directory.Delete(dir, true); }
            catch { }
            try { File.Delete(ThumbPath(e)); } catch { }
        }
        if (gone.Count > 0) { Save(); Changed?.Invoke(); }
        return gone.Count;
    }

    // ------------------------------------------------------------------ 保存先の変更
    static void CopyDirectory(string from, string to)
    {
        Directory.CreateDirectory(to);
        foreach (var f in Directory.GetFiles(from)) File.Copy(f, Path.Combine(to, Path.GetFileName(f)), true);
        foreach (var d in Directory.GetDirectories(from)) CopyDirectory(d, Path.Combine(to, Path.GetFileName(d)));
    }

    /// <summary>ライブラリーを、新しい場所へ引っ越す(中身をコピーして、問題なければ元の場所を消す)</summary>
    public async Task<string> MoveLibraryAsync(string newRoot, Action<string>? progress)
    {
        var from = root;
        try
        {
            var a = Path.GetFullPath(from).TrimEnd('\\');
            var b = Path.GetFullPath(newRoot).TrimEnd('\\');
            if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase)) return "同じ場所です";
            if (b.StartsWith(a + "\\", StringComparison.OrdinalIgnoreCase)) return "ライブラリーの中のフォルダーには、移動できません";
            if (Directory.Exists(newRoot) && Directory.EnumerateFileSystemEntries(newRoot).Any())
                return "移動先のフォルダーが空ではありません。空のフォルダーを選んでください(中に別のライブラリーがある場合は、「別の保存先を開く」で開けます)";

            Save();
            progress?.Invoke("素材を新しい場所へコピーしています…");
            await Task.Run(() => CopyDirectory(from, newRoot));
            if (!File.Exists(Path.Combine(newRoot, "materials.json")) && File.Exists(Path.Combine(from, "materials.json")))
                return "コピーを確認できませんでした。元の場所はそのままです";

            Open(newRoot);
            progress?.Invoke("元の場所を片付けています…");
            try { await Task.Run(() => Directory.Delete(from, true)); }
            catch { return $"新しい場所へ移動しました(元の場所は削除できなかったため、残っています: {from})"; }
            return "新しい場所へ移動しました";
        }
        catch (Exception ex)
        {
            return "移動できませんでした: " + ex.Message;
        }
    }
}
