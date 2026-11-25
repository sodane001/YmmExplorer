using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows.Media;

namespace YmmExplorer;

public sealed class TagInfo
{
    public string Name { get; init; } = "";
    public Brush Brush { get; init; } = Brushes.Gray;
    public int Count { get; init; }
}

/// <summary>タグの保存先。%AppData%\YmmExplorer\tags.json にパス→タグ一覧で保存します</summary>
public sealed class TagStore
{
    public static TagStore Instance { get; } = new();

    static readonly Brush[] Palette =
    {
        Make("#FF8A80"), Make("#FFB74D"), Make("#FFE082"), Make("#A5D6A7"),
        Make("#80DEEA"), Make("#90CAF9"), Make("#B39DDB"), Make("#F48FB1"),
    };

    static Brush Make(string hex)
    {
        var b = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
        b.Freeze();
        return b;
    }

    readonly object gate = new();
    readonly string file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "tags.json");
    Dictionary<string, List<string>> map = new(StringComparer.OrdinalIgnoreCase);

    public event Action? Changed;

    TagStore() => Load();

    void Load()
    {
        try
        {
            if (!File.Exists(file)) return;
            var d = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(File.ReadAllText(file));
            if (d != null) map = new(d, StringComparer.OrdinalIgnoreCase);
        }
        catch { /* 壊れていたら空から開始 */ }
    }

    void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            var opt = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            File.WriteAllText(file, JsonSerializer.Serialize(map, opt));
        }
        catch { }
    }

    static TagInfo Info(string name, int count = 0) => new()
    {
        Name = name,
        Count = count,
        Brush = Palette[Math.Abs(name.ToLowerInvariant().Sum(c => (int)c)) % Palette.Length],
    };

    public IReadOnlyList<TagInfo> TagsOf(string path)
    {
        lock (gate)
            return map.TryGetValue(path, out var l) ? l.Select(t => Info(t)).ToList() : Array.Empty<TagInfo>();
    }

    public IReadOnlyList<string> PathsWithTag(string tag)
    {
        lock (gate)
            return map.Where(kv => kv.Value.Contains(tag, StringComparer.OrdinalIgnoreCase))
                      .Select(kv => kv.Key).ToList();
    }

    public IReadOnlyList<TagInfo> AllTags()
    {
        lock (gate)
            return map.Values.SelectMany(v => v)
                      .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                      .OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase)
                      .Select(g => Info(g.Key, g.Count())).ToList();
    }

    public void Add(IEnumerable<string> paths, string tag)
    {
        lock (gate)
        {
            foreach (var p in paths)
            {
                if (!map.TryGetValue(p, out var l)) map[p] = l = new();
                if (!l.Contains(tag, StringComparer.OrdinalIgnoreCase)) l.Add(tag);
            }
            Save();
        }
        Changed?.Invoke();
    }

    public void Remove(IEnumerable<string> paths, string tag)
    {
        lock (gate)
        {
            foreach (var p in paths)
            {
                if (!map.TryGetValue(p, out var l)) continue;
                l.RemoveAll(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
                if (l.Count == 0) map.Remove(p);
            }
            Save();
        }
        Changed?.Invoke();
    }
}
