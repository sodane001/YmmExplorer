using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>図形1件分の保存データ。YMM4の図形(標準・プラグイン)と、自分で追加したものの両方</summary>
public sealed class ShapeEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>自分で追加した図形か(false = YMM4に入っている図形を自動で見つけたもの)</summary>
    public bool IsCustom { get; set; }

    /// <summary>見つけた図形の型名(自分で追加したものは空)</summary>
    public string Key { get; set; } = "";

    public string Name { get; set; } = "";
    public string Memo { get; set; } = "";
    public string Source { get; set; } = "";

    /// <summary>非表示にした図形</summary>
    public bool Hidden { get; set; }

    /// <summary>サムネイル(PNGをBase64にしたもの)</summary>
    public string? Thumbnail { get; set; }

    /// <summary>サムネイルが、自動収録の時に自動で作った「標準の状態の図形」の画像か(自分で設定したら false)</summary>
    public bool AutoThumbnail { get; set; }

    /// <summary>タイムラインへ配置するためのデータ(YMM4でその図形のアイテムをコピーした時のクリップボードの中身)</summary>
    public List<ClipFormat>? Formats { get; set; }

    /// <summary>YMM4のアイテムテンプレート(JSON)。入っている場合は、これでシークバーの位置へ確実に配置する</summary>
    public string? TemplateJson { get; set; }
}

public sealed class ShapeFile
{
    public Dictionary<int, List<ShapeEntry>> Slots { get; set; } = new();

    /// <summary>タブごとの、削除した自動収録の図形(図形の型名)。もう一度「自動収録」しても入れない</summary>
    public Dictionary<int, List<string>> RemovedKeys { get; set; } = new();
}

/// <summary>図形の一覧(図形タブごとに独立)。%AppData%\YmmExplorer\shapes.json</summary>
public sealed class ShapeStore
{
    static readonly string FilePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "shapes.json");

    public static ShapeStore Instance { get; } = new();

    readonly object gate = new();
    ShapeFile data = new();

    public event Action<int>? Changed;

    ShapeStore()
    {
        try
        {
            if (File.Exists(FilePath))
                data = JsonSerializer.Deserialize<ShapeFile>(File.ReadAllText(FilePath)) ?? new();
        }
        catch { data = new(); }
        // 1つの図形にまとめた「PV背景」の個別図形は、手を加えていなければ一覧から外す
        foreach (var l in data.Slots.Values)
            l.RemoveAll(e => !e.IsCustom && IsMergedPvShape(e.Key) && (e.Thumbnail == null || e.AutoThumbnail) && string.IsNullOrEmpty(e.Memo));
    }

    static bool IsMergedPvShape(string key)
    {
        const string head = "YMM4EffectShapes.S", tail = "Plugin";
        return key.StartsWith(head, StringComparison.Ordinal) && key.EndsWith(tail, StringComparison.Ordinal)
            && int.TryParse(key.AsSpan(head.Length, key.Length - head.Length - tail.Length), out int no) && no is >= 1002 and <= 1081;
    }

    List<ShapeEntry> Slot(int slot)
    {
        if (!data.Slots.TryGetValue(slot, out var l)) data.Slots[slot] = l = new();
        return l;
    }

    public IReadOnlyList<ShapeEntry> All(int slot)
    {
        lock (gate) return Slot(slot).ToList();
    }

    /// <summary>見つかった図形のうち、まだ一覧に無いものを末尾に足す。足した数を返す</summary>
    public int Sync(int slot, IEnumerable<ShapeInfo> found)
    {
        int added = 0;
        lock (gate)
        {
            var list = Slot(slot);
            var removed = RemovedSet(slot);
            foreach (var f in found)
            {
                if (removed.Contains(f.Key)) continue;      // 自分で削除した図形は、入れ直さない
                if (list.Any(e => !e.IsCustom && e.Key == f.Key)) continue;
                list.Add(new ShapeEntry { Key = f.Key, Name = f.Name, Source = f.Source });
                added++;
            }
        }
        if (added > 0) { Save(); Changed?.Invoke(slot); }
        return added;
    }

    /// <summary>
    /// 新しく作ったタブ用: 以前の自動収録で入ったままの図形(配置データもサムネイルもメモも無い、手を加えていないもの)を消す。
    /// 自分で追加した図形・配置データやサムネイルを付けた図形は残す。消した数を返す
    /// </summary>
    public int PurgeAutoLeftovers(int slot)
    {
        int n;
        lock (gate)
        {
            data.RemovedKeys.Remove(slot);          // 新しいタブは、削除した図形の記録も消して最初から
            if (!data.Slots.TryGetValue(slot, out var list)) return 0;
            n = list.RemoveAll(e => !e.IsCustom && e.TemplateJson == null && (e.Formats == null || e.Formats.Count == 0)
                                    && (e.Thumbnail == null || e.AutoThumbnail) && string.IsNullOrEmpty(e.Memo));
        }
        if (n > 0) { Save(); Changed?.Invoke(slot); }
        return n;
    }

    /// <summary>配置データ(アイテムテンプレートのJSON)を、まとめて登録する。登録した数を返す</summary>
    public int AttachTemplates(int slot, IReadOnlyDictionary<string, string> jsonById)
    {
        int n = 0;
        lock (gate)
            foreach (var e in Slot(slot))
                if (jsonById.TryGetValue(e.Id, out var json)) { e.TemplateJson = json; n++; }
        if (n > 0) { Save(); Changed?.Invoke(slot); }
        return n;
    }

    /// <summary>自動で作ったサムネイルを、まとめて登録する(自分で設定したサムネイルがある図形には付けない)。登録した数を返す</summary>
    public int AttachAutoThumbnails(int slot, IReadOnlyDictionary<string, string> b64ById)
    {
        int n = 0;
        lock (gate)
            foreach (var e in Slot(slot))
                if (b64ById.TryGetValue(e.Id, out var b64) && (e.Thumbnail == null || e.AutoThumbnail))
                {
                    e.Thumbnail = b64;
                    e.AutoThumbnail = true;
                    n++;
                }
        if (n > 0) { Save(); Changed?.Invoke(slot); }
        return n;
    }

    public void Add(int slot, ShapeEntry entry)
    {
        lock (gate) Slot(slot).Add(entry);
        Save();
        Changed?.Invoke(slot);
    }

    /// <summary>自分で追加した図形だけを削除する(YMM4の図形は消せないので、非表示にする)。削除した数を返す</summary>
    public int RemoveCustom(int slot, IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids);
        int removed;
        lock (gate) removed = Slot(slot).RemoveAll(e => e.IsCustom && set.Contains(e.Id));
        if (removed > 0) { Save(); Changed?.Invoke(slot); }
        return removed;
    }

    /// <summary>図形を削除する(自動収録した標準の図形も含む)。削除した数を返す</summary>
    public int Remove(int slot, IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids);
        int removed;
        lock (gate)
        {
            var list = Slot(slot);
            // 自動収録した図形は、削除したことを覚えておく(もう一度「自動収録」しても入れない)
            foreach (var e in list.Where(e => set.Contains(e.Id) && !e.IsCustom && !string.IsNullOrEmpty(e.Key)))
            {
                if (!data.RemovedKeys.TryGetValue(slot, out var keys)) data.RemovedKeys[slot] = keys = new();
                if (!keys.Contains(e.Key)) keys.Add(e.Key);
            }
            removed = list.RemoveAll(e => set.Contains(e.Id));
        }
        if (removed > 0) { Save(); Changed?.Invoke(slot); }
        return removed;
    }

    HashSet<string> RemovedSet(int slot)
        => data.RemovedKeys.TryGetValue(slot, out var keys) ? new HashSet<string>(keys) : new HashSet<string>();

    /// <summary>削除した自動収録の図形の数</summary>
    public int RemovedCount(int slot)
    {
        lock (gate) return data.RemovedKeys.TryGetValue(slot, out var keys) ? keys.Count : 0;
    }

    /// <summary>削除した自動収録の図形を、また収録できるように戻す(次の「自動収録」で入る)。戻した数を返す</summary>
    public int ForgetRemoved(int slot)
    {
        int n;
        lock (gate)
        {
            n = data.RemovedKeys.TryGetValue(slot, out var keys) ? keys.Count : 0;
            data.RemovedKeys.Remove(slot);
        }
        if (n > 0) Save();
        return n;
    }

    /// <summary>すべてのタブから、IDで探す(ショートカットキーの実行用)</summary>
    public ShapeEntry? Find(string id)
    {
        lock (gate) return data.Slots.Values.SelectMany(l => l).FirstOrDefault(e => e.Id == id);
    }

    public void SetHidden(int slot, IEnumerable<string> ids, bool hidden)
    {
        var set = new HashSet<string>(ids);
        lock (gate) foreach (var e in Slot(slot).Where(e => set.Contains(e.Id))) e.Hidden = hidden;
        Save();
        Changed?.Invoke(slot);
    }

    // 並べ替え: 画面に出ている図形(非表示を出さない設定なら、非表示は除く)の中での順番として扱う
    void ApplyOnVisible(int slot, bool showHidden, Action<List<ShapeEntry>> op)
    {
        lock (gate)
        {
            var list = Slot(slot);
            var idx = Enumerable.Range(0, list.Count).Where(i => showHidden || !list[i].Hidden).ToList();
            var sub = idx.Select(i => list[i]).ToList();
            op(sub);
            for (int k = 0; k < idx.Count; k++) list[idx[k]] = sub[k];
        }
        Save();
        Changed?.Invoke(slot);
    }

    public void Move(int slot, IEnumerable<string> ids, string how, bool showHidden)
    {
        var set = new HashSet<string>(ids);
        ApplyOnVisible(slot, showHidden, sub => Reorder.Apply(sub, e => set.Contains(e.Id), how));
    }

    public void MoveTo(int slot, IEnumerable<string> ids, int insertIndex, bool showHidden)
    {
        var set = new HashSet<string>(ids);
        ApplyOnVisible(slot, showHidden, sub => Reorder.MoveTo(sub, e => set.Contains(e.Id), insertIndex));
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

/// <summary>画面に並べる図形</summary>
public sealed class ShapeItem : NotifyBase
{
    public ShapeEntry Entry { get; }
    readonly ShapeStore store;

    /// <summary>YMM4の図形として見つからなくなった(プラグインを外した等)</summary>
    public bool IsMissing { get; }

    /// <summary>この図形に割り当てたショートカットキー</summary>
    public string ShortcutKeys
    {
        get => ShortcutStore.Instance.KeysFor("shape", Entry.Id);
        set { ShortcutStore.Instance.Set("shape", Entry.Id, Name, value ?? ""); Raise(nameof(ShortcutKeys)); }
    }

    public ShapeItem(ShapeEntry e, ShapeStore s, bool isMissing)
    {
        Entry = e;
        store = s;
        IsMissing = isMissing;
    }

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

    public bool IsHidden => Entry.Hidden;
    public bool IsCustom => Entry.IsCustom;

    /// <summary>「標準」「プラグイン: ○○」「自分で追加」</summary>
    public string SourceText => Entry.IsCustom ? "自分で追加" : (IsMissing ? Entry.Source + "(見つかりません)" : Entry.Source);

    /// <summary>保存してあるデータの指紋。内容が同じなら同じ値になる(別々に登録したのに同じ値なら、同じ内容を保存している)</summary>
    public string DataId => Entry.TemplateJson != null ? ClipboardRaw.FingerprintText(Entry.TemplateJson) : ClipboardRaw.Fingerprint(Entry.Formats);

    public bool HasSnapshot => Entry.TemplateJson != null || Entry.Formats is { Count: > 0 };
    public string PlaceText => HasSnapshot ? "配置データあり" : "配置データなし(ダブルクリックで登録)";

    public void NotifyChanged()
    {
        Raise(nameof(HasSnapshot));
        Raise(nameof(PlaceText));
        Raise(nameof(IsHidden));
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
    public bool HasCustomThumb => Thumb != null;

    /// <summary>サムネイルを設定する(nullで削除)。変換に失敗したら false</summary>
    public bool SetThumbnail(BitmapSource? image)
    {
        Entry.AutoThumbnail = false;
        if (image == null) Entry.Thumbnail = null;
        else
        {
            var b64 = PresetThumb.Encode(image);
            if (b64 == null) return false;
            Entry.Thumbnail = b64;
        }
        thumbLoaded = false;
        store.Save();
        Raise(nameof(Thumb));
        Raise(nameof(HasNoThumb));
        Raise(nameof(HasCustomThumb));
        return true;
    }
}
