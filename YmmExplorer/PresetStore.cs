using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>プリセット1件分の保存データ(YMM4のタイムライン素材をコピーした時のクリップボード内容)</summary>
public sealed class PresetEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string Memo { get; set; } = "";
    public DateTime Created { get; set; } = DateTime.Now;

    /// <summary>サムネイル画像(PNGをBase64にしたもの。長辺320px以内)</summary>
    public string? Thumbnail { get; set; }

    /// <summary>プレビュー動画のファイル名(%AppData%\YmmExplorer\previews 内)</summary>
    public string? PreviewFile { get; set; }
    public List<ClipFormat> Formats { get; set; } = new();

    /// <summary>
    /// YMM4のアイテムテンプレート(ItemTemplate)をJSONにしたもの。入っている場合は、これを使って
    /// シークバーの位置へ確実に配置する(クリップボードは使わない)
    /// </summary>
    public string? TemplateJson { get; set; }
}

public sealed class PresetFile
{
    public Dictionary<int, List<PresetEntry>> Slots { get; set; } = new();
}

/// <summary>プリセット一覧(プリセット1〜10は独立して保存)。%AppData%\YmmExplorer\presets.json</summary>
public sealed class PresetStore
{
    static readonly string FilePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "presets.json");

    public static PresetStore Instance { get; } = new();

    readonly object gate = new();
    PresetFile data = new();

    /// <summary>追加・削除で一覧が変わった時(引数はスロット番号。名前やメモの編集では呼ばれません)</summary>
    public event Action<int>? Changed;

    PresetStore()
    {
        try
        {
            if (File.Exists(FilePath))
                data = JsonSerializer.Deserialize<PresetFile>(File.ReadAllText(FilePath)) ?? new();
        }
        catch { data = new(); }
    }

    List<PresetEntry> Slot(int slot)
    {
        if (!data.Slots.TryGetValue(slot, out var l)) data.Slots[slot] = l = new();
        return l;
    }

    public IReadOnlyList<PresetEntry> All(int slot)
    {
        lock (gate) return Slot(slot).ToList();
    }

    /// <summary>すべてのタブから、IDで探す(ショートカットキーの実行用)</summary>
    public PresetEntry? Find(string id)
    {
        lock (gate) return data.Slots.Values.SelectMany(l => l).FirstOrDefault(e => e.Id == id);
    }

    public void Add(int slot, PresetEntry entry)
    {
        lock (gate) Slot(slot).Add(entry);
        Save();
        Changed?.Invoke(slot);
    }

    public void Remove(int slot, IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids);
        List<PresetEntry> gone;
        lock (gate)
        {
            var l = Slot(slot);
            gone = l.Where(e => set.Contains(e.Id)).ToList();
            l.RemoveAll(e => set.Contains(e.Id));
        }
        foreach (var e in gone) PresetPreviewStore.DeleteQuietly(e.PreviewFile);   // 保存してあったプレビュー動画も削除
        if (gone.Count > 0) { Save(); Changed?.Invoke(slot); }
    }

    /// <summary>選んだプリセットを移動する。how = up / down / top / bottom</summary>
    public void Move(int slot, IEnumerable<string> ids, string how)
    {
        var set = new HashSet<string>(ids);
        lock (gate) Reorder.Apply(Slot(slot), e => set.Contains(e.Id), how);
        Save();
        Changed?.Invoke(slot);
    }

    /// <summary>選んだプリセットを、insertIndex の位置へ移動する(ドラッグ&ドロップ用)</summary>
    public void MoveTo(int slot, IEnumerable<string> ids, int insertIndex)
    {
        var set = new HashSet<string>(ids);
        lock (gate) Reorder.MoveTo(Slot(slot), e => set.Contains(e.Id), insertIndex);
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

/// <summary>プリセットのサムネイル画像の変換</summary>
public static class PresetThumb
{
    const int MaxSize = 320;

    /// <summary>画像を長辺320px以内に縮小して、PNGのBase64にする</summary>
    public static string? Encode(BitmapSource src)
    {
        try
        {
            BitmapSource s = src;
            var scale = Math.Min(1.0, MaxSize / (double)Math.Max(src.PixelWidth, src.PixelHeight));
            if (scale < 1) s = new TransformedBitmap(src, new ScaleTransform(scale, scale));
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(s));
            using var ms = new MemoryStream();
            enc.Save(ms);
            return Convert.ToBase64String(ms.ToArray());
        }
        catch { return null; }
    }

    public static BitmapSource? Decode(string base64)
    {
        try
        {
            using var ms = new MemoryStream(Convert.FromBase64String(base64));
            var b = new BitmapImage();
            b.BeginInit();
            b.CacheOption = BitmapCacheOption.OnLoad;
            b.StreamSource = ms;
            b.EndInit();
            b.Freeze();
            return b;
        }
        catch { return null; }
    }

    public static BitmapSource? LoadFile(string path)
    {
        try
        {
            var b = new BitmapImage();
            b.BeginInit();
            b.CacheOption = BitmapCacheOption.OnLoad;
            b.UriSource = new Uri(path);
            b.EndInit();
            b.Freeze();
            return b;
        }
        catch { return null; }
    }

    public static bool IsImageFile(string path)
        => System.IO.Path.GetExtension(path).TrimStart('.').ToLowerInvariant()
            is "png" or "jpg" or "jpeg" or "bmp" or "gif" or "webp" or "tif" or "tiff";
}

/// <summary>画面に並べるプリセット</summary>
public sealed class PresetItem : NotifyBase
{
    public PresetEntry Entry { get; }
    readonly PresetStore store;

    public string Name
    {
        get => Entry.Name;
        set
        {
            if (Entry.Name == value) return;
            Entry.Name = value;
            Raise(nameof(Name));
            store.Save();
        }
    }

    public string Memo
    {
        get => Entry.Memo;
        set
        {
            if (Entry.Memo == value) return;
            Entry.Memo = value;
            Raise(nameof(Memo));
            store.Save();
        }
    }

    // ---- プレビュー動画(含まれる素材からエンコードして保存したもの、または登録した動画) ----
    public string? PreviewPath
    {
        get
        {
            if (string.IsNullOrEmpty(Entry.PreviewFile)) return null;
            var path = PresetPreviewStore.PathOf(Entry.PreviewFile);
            return File.Exists(path) ? path : null;
        }
    }

    public bool HasPreview => PreviewPath != null;
    public bool HasNoPreview => PreviewPath == null;

    /// <summary>プレビュー動画の保存先を変えた/消した時(null で削除)</summary>
    public void SetPreviewFile(string? fileName)
    {
        Entry.PreviewFile = fileName;
        store.Save();
        NotifyPreviewChanged();
    }

    public void NotifyPreviewChanged()
    {
        Raise(nameof(PreviewPath));
        Raise(nameof(HasPreview));
        Raise(nameof(HasNoPreview));
    }

    // ---- サムネイル ----
    BitmapSource? thumb;
    bool thumbLoaded;

    public ImageSource? Thumb
    {
        get
        {
            if (!thumbLoaded)
            {
                thumbLoaded = true;
                thumb = string.IsNullOrEmpty(Entry.Thumbnail) ? null : PresetThumb.Decode(Entry.Thumbnail);
            }
            return thumb;
        }
    }

    public bool HasNoThumb => Thumb == null;

    /// <summary>サムネイルを設定する(nullで削除)。変換に失敗したら false</summary>
    public bool SetThumbnail(BitmapSource? image)
    {
        if (image == null) Entry.Thumbnail = null;
        else
        {
            var b64 = PresetThumb.Encode(image);
            if (b64 == null) return false;
            Entry.Thumbnail = b64;
        }
        thumbLoaded = false;
        Raise(nameof(Thumb));
        Raise(nameof(HasNoThumb));
        store.Save();
        return true;
    }

    /// <summary>保存してあるデータの指紋。内容が同じなら同じ値になる(別々に登録したのに同じ値なら、同じ内容を保存している)</summary>
    public string DataId => Entry.TemplateJson != null ? ClipboardRaw.FingerprintText(Entry.TemplateJson) : ClipboardRaw.Fingerprint(Entry.Formats);

    public string CreatedText => Entry.Created.ToString("yyyy/MM/dd HH:mm");

    /// <summary>「3 形式 / 12.4 KB」のような概要</summary>
    public string Summary
    {
        get
        {
            if (Entry.TemplateJson != null) return $"アイテムテンプレート / {FileItem.FormatSize(Entry.TemplateJson.Length)}";
            long bytes = Entry.Formats.Sum(f => (long)f.ByteCount);
            return $"{Entry.Formats.Count} 形式 / {FileItem.FormatSize(bytes)}";
        }
    }

    /// <summary>保存されている独自形式の名前(YMM4の素材データの目印)</summary>
    public string FormatNames
    {
        get
        {
            var names = Entry.Formats.Where(f => f.Name != null).Select(f => f.Name!).ToList();
            return names.Count == 0 ? "テキストのみ" : string.Join("、", names);
        }
    }

    /// <summary>このプリセットに割り当てたショートカットキー(クリックして、実際にキーを押して記録)</summary>
    public string ShortcutKeys
    {
        get => ShortcutStore.Instance.KeysFor("preset", Entry.Id);
        set { ShortcutStore.Instance.Set("preset", Entry.Id, Name, value ?? ""); Raise(nameof(ShortcutKeys)); }
    }

    public PresetItem(PresetEntry e, PresetStore s)
    {
        Entry = e;
        store = s;
    }
}
