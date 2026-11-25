using System.IO;
using System.Text.Json;

namespace YmmExplorer;

/// <summary>パソコン上で見つかったクラウドサービスの同期フォルダー(または見つからなかったサービス)</summary>
public sealed class CloudLocation
{
    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public bool Found => Path.Length > 0;
    public string StatusText => Found ? "見つかりました" : "見つかりません(公式アプリが未インストール、または未ログインです)";
}

/// <summary>
/// クラウドサービスの公式デスクトップアプリ(Google Drive for desktop、Box Drive など)が作る
/// 同期フォルダーを探します。ログインや同期は各アプリが行うので、ここでは「フォルダーとして」扱います。
/// </summary>
public static class CloudDetector
{
    public static List<CloudLocation> Detect()
    {
        var list = new List<CloudLocation>();
        void Add(string name, string? path)
            => list.Add(new CloudLocation { Name = name, Path = !string.IsNullOrEmpty(path) && SafeExists(path) ? path : "" });

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        Add("Google ドライブ", FindGoogleDrive(home));
        Add("Box", First(Join(home, "Box"), Join(home, "Box Sync")));
        Add("OneDrive", First(Env("OneDrive"), Env("OneDriveConsumer"), Join(home, "OneDrive")));
        var work = Env("OneDriveCommercial");
        if (!string.IsNullOrEmpty(work)) Add("OneDrive(組織)", work);
        Add("Dropbox", FindDropbox(home));
        Add("iCloud ドライブ", First(Join(home, "iCloudDrive")));
        Add("pCloud", FindByVolumeLabel("pCloud Drive"));
        Add("MEGA", First(Join(home, "MEGA")));
        return list;
    }

    static string? Env(string name) => Environment.GetEnvironmentVariable(name);
    static string Join(string a, string b) => System.IO.Path.Combine(a, b);
    static bool SafeExists(string p) { try { return Directory.Exists(p); } catch { return false; } }
    static string? First(params string?[] paths) => paths.FirstOrDefault(p => !string.IsNullOrEmpty(p) && SafeExists(p));

    static string? FindGoogleDrive(string home)
    {
        // Google Drive for desktop: ドライブ文字(ボリューム名「Google Drive」)の中の「マイドライブ」/「My Drive」
        var root = FindByVolumeLabel("Google Drive");
        if (root != null)
        {
            var mine = First(Join(root, "マイドライブ"), Join(root, "My Drive"));
            return mine ?? root;
        }
        return First(Join(home, "Google Drive"), Join(home, "Google ドライブ"));
    }

    static string? FindByVolumeLabel(string label)
    {
        try
        {
            foreach (var d in DriveInfo.GetDrives())
            {
                try
                {
                    if (d.IsReady && string.Equals(d.VolumeLabel, label, StringComparison.OrdinalIgnoreCase))
                        return d.RootDirectory.FullName;
                }
                catch { }
            }
        }
        catch { }
        return null;
    }

    static string? FindDropbox(string home)
    {
        foreach (var baseDir in new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        })
        {
            try
            {
                var info = Join(Join(baseDir, "Dropbox"), "info.json");
                if (!File.Exists(info)) continue;
                using var doc = JsonDocument.Parse(File.ReadAllText(info));
                foreach (var kind in new[] { "personal", "business" })
                    if (doc.RootElement.TryGetProperty(kind, out var acc)
                        && acc.TryGetProperty("path", out var p) && p.GetString() is { Length: > 0 } path
                        && SafeExists(path))
                        return path;
            }
            catch { }
        }
        return First(Join(home, "Dropbox"));
    }

    // ---- クラウドのみ(未ダウンロード)ファイルの判定 ----
    const int FILE_ATTRIBUTE_OFFLINE = 0x1000;
    const int FILE_ATTRIBUTE_RECALL_ON_OPEN = 0x40000;
    const int FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS = 0x400000;

    /// <summary>
    /// ファイルの実体がまだパソコンに無く、開くとダウンロードが始まる状態か。
    /// (OneDrive・Google ドライブ・Box などの「オンラインのみ」ファイル)
    /// </summary>
    public static bool IsCloudOnly(FileAttributes attributes)
        => ((int)attributes & (FILE_ATTRIBUTE_OFFLINE | FILE_ATTRIBUTE_RECALL_ON_OPEN | FILE_ATTRIBUTE_RECALL_ON_DATA_ACCESS)) != 0;
}
