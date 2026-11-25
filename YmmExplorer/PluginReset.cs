using System.IO;
using System.Text.Json;

namespace YmmExplorer;

/// <summary>
/// このプラグインの設定・登録内容を、まとめて初期状態に戻す(完全リセット)。
/// 開いている間にファイルを消しても、画面が持っている内容がまた保存されてしまうので、
/// 「予約」だけしておき、次にYMM4を起動した時(どの設定も読み込む前)に実行する。
/// 消すファイルは、念のため YmmExplorer_backup\日時 に移しておく(「バックアップを残さない」にすると完全に消す)
/// </summary>
public static class PluginReset
{
    static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static readonly string DataDir = Path.Combine(AppData, "YmmExplorer");
    public static readonly string BackupRoot = Path.Combine(AppData, "YmmExplorer_backup");
    // 予約の印は、リセットで消えないように別の場所に置く
    static readonly string RequestPath = Path.Combine(AppData, "YmmExplorer_reset.json");

    public sealed class Request
    {
        public bool Settings { get; set; } = true;
        public bool Shortcuts { get; set; } = true;
        public bool Library { get; set; } = true;
        public bool Materials { get; set; }
        public bool KeepBackup { get; set; } = true;
        public DateTime RequestedAt { get; set; } = DateTime.Now;
    }

    /// <summary>種類ごとの、消す対象(DataDir の中の名前)</summary>
    static IEnumerable<string> TargetsOf(Request r)
    {
        if (r.Settings) { yield return "settings.json"; yield return "tags.json"; }
        if (r.Shortcuts) yield return "shortcuts.json";
        if (r.Library)
        {
            yield return "templates.json";
            yield return "presets.json";
            yield return "previews";
            yield return "shapes.json";
            yield return "effects-video.json";
            yield return "effects-audio.json";
            yield return "texts.json";
        }
        if (r.Materials) yield return "MaterialLibrary";
    }

    public static bool IsPending => File.Exists(RequestPath);

    public static Request? Pending()
    {
        try { return File.Exists(RequestPath) ? JsonSerializer.Deserialize<Request>(File.ReadAllText(RequestPath)) : null; }
        catch { return null; }
    }

    public static bool Schedule(Request r, out string error)
    {
        error = "";
        try
        {
            File.WriteAllText(RequestPath, JsonSerializer.Serialize(r));
            return true;
        }
        catch (Exception ex) { error = ex.Message; return false; }
    }

    public static void Cancel()
    {
        try { if (File.Exists(RequestPath)) File.Delete(RequestPath); } catch { }
    }

    static bool applied;
    /// <summary>前回のリセットの結果(画面に知らせる用)。無ければ空</summary>
    public static string LastResult { get; private set; } = "";

    /// <summary>予約されていればリセットを行う。プラグインが読み込まれた直後(設定を読む前)に呼ぶ</summary>
    public static void ApplyPending()
    {
        if (applied) return;
        applied = true;
        var r = Pending();
        if (r == null) { Cancel(); return; }
        int moved = 0;
        var failed = new List<string>();
        var backup = Path.Combine(BackupRoot, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        foreach (var name in TargetsOf(r))
        {
            var src = Path.Combine(DataDir, name);
            try
            {
                bool isDir = Directory.Exists(src);
                if (!isDir && !File.Exists(src)) continue;
                if (r.KeepBackup)
                {
                    Directory.CreateDirectory(backup);
                    var dst = Path.Combine(backup, name);
                    if (isDir) Directory.Move(src, dst); else File.Move(src, dst);
                }
                else if (isDir) Directory.Delete(src, true);
                else File.Delete(src);
                moved++;
            }
            catch (Exception ex) { failed.Add($"{name}({ex.Message})"); }
        }
        Cancel();
        LastResult = $"完全リセットを行いました({moved} 個を{(r.KeepBackup ? "バックアップへ移動" : "削除")})"
                     + (r.KeepBackup && moved > 0 ? $"。元に戻したい時は {backup} の中身を {DataDir} へ戻してください" : "")
                     + (failed.Count > 0 ? "。⚠ 消せなかったもの: " + string.Join("、", failed) : "");
    }
}
