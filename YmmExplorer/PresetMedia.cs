using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace YmmExplorer;

/// <summary>プリセットの中で使われている素材ファイル1つ分</summary>
public sealed class MediaRef
{
    public string Path { get; init; } = "";
    public string Name => System.IO.Path.GetFileName(Path);
    public string Folder => System.IO.Path.GetDirectoryName(Path) ?? "";
}

/// <summary>
/// 保存したプリセットの中身(YMM4のコピーデータ)から、使われている画像・動画ファイルのパスを探します。
/// データの書式が分からなくても使えるよう、文字として読んでパスらしい部分を拾う方法です。
/// </summary>
public static class PresetMediaFinder
{
    static readonly Regex PathRx = new(
        @"[A-Za-z]:(?:\\\\|\\|/)[^""<>|*?\r\n\0]{0,260}?\.(?:png|jpe?g|bmp|gif|webp|mp4|mov|avi|mkv|webm|wmv)(?![A-Za-z0-9])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static List<MediaRef> Find(PresetEntry entry)
    {
        var result = new List<MediaRef>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // クリップボードの形式ごとのデータと、アイテムテンプレートのJSONを、文字として読む
        var sources = new List<string[]>();
        foreach (var f in entry.Formats)
        {
            byte[] bytes;
            try { bytes = Convert.FromBase64String(f.Data); }
            catch { continue; }
            // UTF-8(JSONなど)とUTF-16(Windowsのテキスト・ファイル一覧)の両方で読む
            sources.Add(new[] { Encoding.UTF8.GetString(bytes), Encoding.Unicode.GetString(bytes) });
        }
        if (!string.IsNullOrEmpty(entry.TemplateJson)) sources.Add(new[] { entry.TemplateJson });

        foreach (var texts in sources)
        {
            foreach (var text in texts)
            {
                foreach (Match m in PathRx.Matches(text))
                {
                    var raw = m.Value;
                    var path = raw;
                    // JSONでは \ が \\ になり、日本語は \uXXXX になる
                    if (raw.Contains("\\\\") || raw.Contains("\\u"))
                    {
                        try { path = Regex.Unescape(raw); } catch { path = raw; }
                    }
                    path = path.Replace('/', '\\');
                    try
                    {
                        if (File.Exists(path) && seen.Add(path)) result.Add(new MediaRef { Path = path });
                    }
                    catch { }
                }
            }
        }
        return result;
    }
}
