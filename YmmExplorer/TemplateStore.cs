using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>テンプレート1件分の保存データ</summary>
public sealed class TemplateEntry
{
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";
    public string Memo { get; set; } = "";

    /// <summary>自分で付けたサムネイル(PNGをBase64にしたもの)。空なら、ファイル自身のサムネイルを使う</summary>
    public string? Thumbnail { get; set; }
}

/// <summary>templates.json の中身。スロット番号(1〜10)ごとに独立した一覧を持つ</summary>
public sealed class TemplateFile
{
    public Dictionary<int, List<TemplateEntry>> Slots { get; set; } = new();
}

/// <summary>
/// テンプレート一覧(テンプレート1〜10は、それぞれ独立して保存)。
/// %AppData%\YmmExplorer\templates.json に保存します。
/// タブの数を減らしても、使っていないスロットの中身は残ります。
/// </summary>
public sealed class TemplateStore
{
    static readonly string FilePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "templates.json");

    public static TemplateStore Instance { get; } = new();

    readonly object gate = new();
    TemplateFile data = new();

    /// <summary>追加・削除で一覧が変わった時(引数はスロット番号。名前やメモの編集では呼ばれません)</summary>
    public event Action<int>? Changed;

    TemplateStore()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var text = File.ReadAllText(FilePath);
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                // 旧形式(タブ1つだけだった頃)はテンプレート1へ引き継ぐ
                var old = JsonSerializer.Deserialize<List<TemplateEntry>>(text) ?? new();
                data.Slots[1] = old;
            }
            else
            {
                data = JsonSerializer.Deserialize<TemplateFile>(text) ?? new();
            }
        }
        catch { data = new(); }
    }

    List<TemplateEntry> Slot(int slot)
    {
        if (!data.Slots.TryGetValue(slot, out var l)) data.Slots[slot] = l = new();
        return l;
    }

    public IReadOnlyList<TemplateEntry> All(int slot)
    {
        lock (gate) return Slot(slot).ToList();
    }

    /// <summary>追加した件数を返します(そのスロットに登録済みのパスは無視)</summary>
    public int Add(int slot, IEnumerable<string> paths)
    {
        int added = 0;
        lock (gate)
        {
            var entries = Slot(slot);
            foreach (var p in paths)
            {
                if (string.IsNullOrWhiteSpace(p)) continue;
                if (entries.Any(e => string.Equals(e.Path, p, StringComparison.OrdinalIgnoreCase))) continue;
                var name = Directory.Exists(p)
                    ? System.IO.Path.GetFileName(p.TrimEnd('\\', '/'))
                    : System.IO.Path.GetFileNameWithoutExtension(p);
                entries.Add(new TemplateEntry { Path = p, Name = string.IsNullOrEmpty(name) ? p : name });
                added++;
            }
        }
        if (added > 0) { Save(); Changed?.Invoke(slot); }
        return added;
    }

    public void Remove(int slot, IEnumerable<string> paths)
    {
        var set = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
        int removed;
        lock (gate) removed = Slot(slot).RemoveAll(e => set.Contains(e.Path));
        if (removed > 0) { Save(); Changed?.Invoke(slot); }
    }

    static bool Same(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    /// <summary>選んだ項目を移動する。how = up / down / top / bottom</summary>
    public void Move(int slot, IEnumerable<string> paths, string how)
    {
        var set = paths.ToList();
        lock (gate) Reorder.Apply(Slot(slot), e => set.Any(p => Same(p, e.Path)), how);
        Save();
        Changed?.Invoke(slot);
    }

    /// <summary>選んだ項目を、insertIndex の位置へ移動する(ドラッグ&ドロップ用)</summary>
    public void MoveTo(int slot, IEnumerable<string> paths, int insertIndex)
    {
        var set = paths.ToList();
        lock (gate) Reorder.MoveTo(Slot(slot), e => set.Any(p => Same(p, e.Path)), insertIndex);
        Save();
        Changed?.Invoke(slot);
    }

    public void Save()
    {
        try
        {
            lock (gate)
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch { }
    }
}

/// <summary>画面に並べるテンプレート。実ファイルの情報(サムネイル等)は Item から取ります</summary>
public sealed class TemplateItem : NotifyBase
{
    readonly TemplateEntry entry;
    readonly TemplateStore store;

    public FileItem? Item { get; }
    public string Path => entry.Path;

    public string Name
    {
        get => entry.Name;
        set
        {
            if (entry.Name == value) return;
            entry.Name = value;
            Raise(nameof(Name));
            store.Save();
        }
    }

    public string Memo
    {
        get => entry.Memo;
        set
        {
            if (entry.Memo == value) return;
            entry.Memo = value;
            Raise(nameof(Memo));
            store.Save();
        }
    }

    // ---- サムネイル(自分で付けたもの > ファイル自身のサムネイル) ----
    BitmapSource? custom;
    bool customLoaded;

    BitmapSource? Custom
    {
        get
        {
            if (!customLoaded)
            {
                customLoaded = true;
                custom = string.IsNullOrEmpty(entry.Thumbnail) ? null : PresetThumb.Decode(entry.Thumbnail);
            }
            return custom;
        }
    }

    /// <summary>画面に出すサムネイル</summary>
    public ImageSource? DisplayThumb => Custom ?? Item?.Thumbnail;

    public bool HasCustomThumb => Custom != null;

    /// <summary>動画なら、再生して好きなコマをサムネイルにできる(クラウドのみの動画は、勝手にダウンロードされるので除く)</summary>
    public bool CanPlayFrames => Item is { IsVideo: true, IsCloudOnly: false };

    /// <summary>サムネイルを設定する(nullで、ファイル自身のサムネイルに戻す)。変換に失敗したら false</summary>
    public bool SetThumbnail(BitmapSource? image)
    {
        if (image == null) entry.Thumbnail = null;
        else
        {
            var b64 = PresetThumb.Encode(image);
            if (b64 == null) return false;
            entry.Thumbnail = b64;
        }
        customLoaded = false;
        store.Save();
        Raise(nameof(DisplayThumb));
        Raise(nameof(HasCustomThumb));
        return true;
    }

    public string StatusText => Item == null
        ? "ファイルが見つかりません"
        : Item.IsDirectory ? "フォルダー" : $"{Item.TypeText} / {Item.SizeText}";

    /// <summary>このファイル・フォルダーに割り当てたショートカットキー(押すと開く)</summary>
    public string ShortcutKeys
    {
        get => ShortcutStore.Instance.KeysFor("file", entry.Path);
        set { ShortcutStore.Instance.Set("file", entry.Path, string.IsNullOrWhiteSpace(entry.Name) ? System.IO.Path.GetFileName(entry.Path) : entry.Name, value ?? ""); Raise(nameof(ShortcutKeys)); }
    }

    public TemplateItem(TemplateEntry e, TemplateStore s)
    {
        entry = e;
        store = s;
        try
        {
            if (Directory.Exists(e.Path)) Item = FileItem.From(new DirectoryInfo(e.Path), TagStore.Instance);
            else if (File.Exists(e.Path)) Item = FileItem.From(new FileInfo(e.Path), TagStore.Instance);
        }
        catch { Item = null; }

        // ファイル自身のサムネイルは後から読み込まれるので、できたら表示を更新する
        if (Item != null) Item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FileItem.Thumbnail)) Raise(nameof(DisplayThumb));
        };
    }
}
