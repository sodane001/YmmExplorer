using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace YmmExplorer;

/// <summary>YMM4に入れているプラグイン1つ分(user\plugin の中の、フォルダー1つ または DLL1つ)</summary>
public sealed class PluginInfo : NotifyBase
{
    public string Name { get; init; } = "";
    public string Path { get; init; } = "";
    public bool IsFolder { get; init; }
    public string Version { get; init; } = "";
    public string Description { get; init; } = "";
    public long Size { get; init; }

    /// <summary>このプラグイン自身(無効化・削除はできない)</summary>
    public bool IsSelf { get; init; }
    public bool CanModify => !IsSelf;

    bool isEnabled;
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (Set(ref isEnabled, value))
            {
                Raise(nameof(StateText));
                Raise(nameof(ToggleText));
            }
        }
    }

    public string StateText => IsSelf ? "このプラグイン" : IsEnabled ? "有効" : "無効(YMM4の再起動後に反映)";
    public string ToggleText => IsEnabled ? "無効にする" : "有効にする";
    public string SizeText => FileItem.FormatSize(Size);
    public string InfoText => string.Join(" / ", new[] { Version, Description }.Where(x => !string.IsNullOrWhiteSpace(x)));
}

/// <summary>
/// プラグインの一覧・無効化・削除。
/// 無効化は、DLLの名前の末尾に「.disabled」を付ける方法(YMM4は .dll だけを読み込むため、再起動後に読み込まれなくなる)。
/// 削除は、フォルダーごと消す。使用中で消せない時は、名前を変えて(次回から読み込まれない)、次の起動時に消す。
/// </summary>
public static class PluginManager
{
    const string DisabledSuffix = ".disabled";
    const string RemovedSuffix = ".removed";

    static readonly string PendingFile = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "pending-delete.json");

    /// <summary>YMM4の user\plugin フォルダー(このプラグインの1つ上のフォルダー)</summary>
    /// <summary>プラグインフォルダーを直接指定する(YMM4 マネージャーなど、YMM4の外から使う時)</summary>
    public static string? RootOverride { get; set; }

    public static string? PluginRoot()
    {
        if (!string.IsNullOrEmpty(RootOverride)) return Directory.Exists(RootOverride) ? RootOverride : null;
        try
        {
            var self = Assembly.GetExecutingAssembly().Location;
            var folder = System.IO.Path.GetDirectoryName(self);
            var parent = folder == null ? null : System.IO.Path.GetDirectoryName(folder);
            if (parent != null && Directory.Exists(parent) && System.IO.Path.GetFileName(parent).Equals("plugin", StringComparison.OrdinalIgnoreCase))
                return parent;
        }
        catch { }
        var guess = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user", "plugin");
        return Directory.Exists(guess) ? guess : null;
    }

    static string SelfFolder()
    {
        if (!string.IsNullOrEmpty(RootOverride)) return "\u0000";      // YMM4の外から使う時は「このプラグイン自身」は無い
        try { return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ""; }
        catch { return ""; }
    }

    public static List<PluginInfo> Scan(out string message)
    {
        var list = new List<PluginInfo>();
        var root = PluginRoot();
        if (root == null)
        {
            message = "YMM4のプラグインフォルダー(user\\plugin)が見つかりませんでした";
            return list;
        }

        var self = SelfFolder();
        try
        {
            foreach (var dir in Directory.GetDirectories(root).OrderBy(d => d, StringComparer.CurrentCultureIgnoreCase))
            {
                var dlls = Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories);
                var disabled = Directory.GetFiles(dir, "*.dll" + DisabledSuffix, SearchOption.AllDirectories);
                if (dlls.Length == 0 && disabled.Length == 0) continue;

                var name = System.IO.Path.GetFileName(dir);
                var main = (dlls.Length > 0 ? dlls : disabled)
                    .OrderByDescending(f => System.IO.Path.GetFileNameWithoutExtension(f.Replace(DisabledSuffix, "")).Equals(name, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(f => f.Length).First();
                var (version, desc) = ReadVersion(main);
                list.Add(new PluginInfo
                {
                    Name = name,
                    Path = dir,
                    IsFolder = true,
                    Version = version,
                    Description = desc,
                    Size = SafeSize(dir),
                    IsSelf = string.Equals(dir.TrimEnd('\\'), self.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase),
                    IsEnabled = dlls.Length > 0,
                });
            }
            foreach (var file in Directory.GetFiles(root, "*.dll*").OrderBy(f => f, StringComparer.CurrentCultureIgnoreCase))
            {
                var enabled = file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
                if (!enabled && !file.EndsWith(".dll" + DisabledSuffix, StringComparison.OrdinalIgnoreCase)) continue;
                var (version, desc) = ReadVersion(file);
                list.Add(new PluginInfo
                {
                    Name = System.IO.Path.GetFileNameWithoutExtension(file.Replace(DisabledSuffix, "")),
                    Path = file,
                    IsFolder = false,
                    Version = version,
                    Description = desc,
                    Size = new FileInfo(file).Length,
                    IsEnabled = enabled,
                });
            }
        }
        catch (Exception ex)
        {
            message = "プラグインの一覧を読み込めませんでした: " + ex.Message;
            return list;
        }
        message = $"{list.Count} 個のプラグインが見つかりました({root})";
        return list;
    }

    static (string Version, string Description) ReadVersion(string file)
    {
        try
        {
            var v = FileVersionInfo.GetVersionInfo(file);
            var version = string.IsNullOrWhiteSpace(v.FileVersion) ? "" : "v" + v.FileVersion;
            var desc = !string.IsNullOrWhiteSpace(v.FileDescription) ? v.FileDescription!
                     : !string.IsNullOrWhiteSpace(v.ProductName) ? v.ProductName! : "";
            return (version, desc);
        }
        catch { return ("", ""); }
    }

    static long SafeSize(string dir)
    {
        try { return new DirectoryInfo(dir).EnumerateFiles("*", SearchOption.AllDirectories).Sum(f => f.Length); }
        catch { return 0; }
    }

    static IEnumerable<string> TargetFiles(PluginInfo p, bool enabled)
    {
        var suffix = enabled ? ".dll" : ".dll" + DisabledSuffix;
        if (!p.IsFolder) return File.Exists(p.Path) ? new[] { p.Path } : Array.Empty<string>();
        return Directory.GetFiles(p.Path, "*" + suffix, SearchOption.AllDirectories)
            .Where(f => f.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>有効・無効を切り替える。戻り値は結果の文章(失敗した場合も含む)</summary>
    public static string Toggle(PluginInfo p)
    {
        if (p.IsSelf) return "このプラグイン自身は、無効にできません";
        try
        {
            if (p.IsFolder)
            {
                var files = Directory.GetFiles(p.Path, "*", SearchOption.AllDirectories)
                    .Where(f => p.IsEnabled ? f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                            : f.EndsWith(".dll" + DisabledSuffix, StringComparison.OrdinalIgnoreCase)).ToList();
                if (files.Count == 0) return "切り替えるDLLが見つかりませんでした";
                foreach (var f in files)
                    File.Move(f, p.IsEnabled ? f + DisabledSuffix : f.Substring(0, f.Length - DisabledSuffix.Length));
            }
            else
            {
                File.Move(p.Path, p.IsEnabled ? p.Path + DisabledSuffix : p.Path.Substring(0, p.Path.Length - DisabledSuffix.Length));
            }
            var now = !p.IsEnabled;
            p.IsEnabled = now;
            return now ? $"「{p.Name}」を有効にしました。YMM4を再起動すると読み込まれます"
                       : $"「{p.Name}」を無効にしました。YMM4を再起動すると読み込まれなくなります";
        }
        catch (Exception ex)
        {
            return $"切り替えできませんでした: {ex.Message}(YMM4を閉じてから、もう一度お試しください)";
        }
    }

    /// <summary>プラグインを削除する。戻り値は結果の文章</summary>
    public static string Delete(PluginInfo p)
    {
        if (p.IsSelf) return "このプラグイン自身は、削除できません";
        try
        {
            if (p.IsFolder) Directory.Delete(p.Path, true);
            else File.Delete(p.Path);
            return $"「{p.Name}」を削除しました";
        }
        catch
        {
            // 使用中で消せない: 読み込まれないよう名前を変えて、次回の起動時に消す
            try
            {
                var files = p.IsFolder
                    ? Directory.GetFiles(p.Path, "*.dll*", SearchOption.AllDirectories).Where(f => !f.EndsWith(RemovedSuffix)).ToList()
                    : new List<string> { p.Path };
                foreach (var f in files) File.Move(f, f + RemovedSuffix);
                AddPending(p.IsFolder ? p.Path : p.Path + RemovedSuffix);
                return $"「{p.Name}」は使用中のため、すぐには消せませんでした。次回から読み込まれないようにして、YMM4の再起動後に削除します";
            }
            catch (Exception ex)
            {
                return $"削除できませんでした: {ex.Message}(YMM4を閉じてから、もう一度お試しください)";
            }
        }
    }

    static void AddPending(string path)
    {
        try
        {
            var list = ReadPending();
            if (!list.Contains(path, StringComparer.OrdinalIgnoreCase)) list.Add(path);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PendingFile)!);
            File.WriteAllText(PendingFile, JsonSerializer.Serialize(list));
        }
        catch { }
    }

    static List<string> ReadPending()
    {
        try { return File.Exists(PendingFile) ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PendingFile)) ?? new() : new(); }
        catch { return new(); }
    }

    /// <summary>前回、消せなかったプラグインを、起動時に削除する</summary>
    public static void ProcessPendingDeletes()
    {
        try
        {
            var list = ReadPending();
            if (list.Count == 0) return;
            var root = PluginRoot();
            var left = new List<string>();
            foreach (var path in list)
            {
                try
                {
                    // プラグインフォルダーの外は、削除しない
                    if (root == null || !path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) continue;
                    if (Directory.Exists(path)) Directory.Delete(path, true);
                    else if (File.Exists(path)) File.Delete(path);
                }
                catch { left.Add(path); }
            }
            File.WriteAllText(PendingFile, JsonSerializer.Serialize(left));
        }
        catch { }
    }
}
