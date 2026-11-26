using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YmmExplorer;

/// <summary>エフェクト1件分の保存データ(YMM4でコピーした時のクリップボードの中身をそのまま保存)</summary>
public sealed class EffectEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string Memo { get; set; } = "";
    public DateTime Created { get; set; } = DateTime.Now;
    public string? Thumbnail { get; set; }
    public List<ClipFormat> Formats { get; set; } = new();

    /// <summary>
    /// 選んだアイテムから読み取った「エフェクトのまとまり」(エフェクト一覧のJSON)。
    /// 入っている場合は、適用の時に貼り付け形式の見本へはめ込んで使う
    /// </summary>
    public string? EffectsJson { get; set; }

    /// <summary>
    /// テキストの書式(フォント・大きさ・色・縁取りなどと、エフェクト)のJSON。
    /// 入っている場合は、選択中のテキストアイテムに上書きして適用する
    /// </summary>
    public string? StyleJson { get; set; }

    /// <summary>一覧に出す説明(「書式 12項目 / エフェクト 2個: …」など)</summary>
    public string? Description { get; set; }

    /// <summary>保存した書式に含まれる項目の名前(表示用)</summary>
    public List<string>? StyleKeys { get; set; }

    /// <summary>YMM4の内部機能で直接読み取った書式(項目名 → 値のJSON)。適用はアイテムの値を直接書き換える</summary>
    public bool StyleDirect { get; set; }

    /// <summary>YMM4の内部機能で直接読み取ったエフェクト一覧(YMM4のJSON)</summary>
    public bool EffectsDirect { get; set; }

    /// <summary>標準のエフェクトを自動収録したもの: エフェクトの型の名前(同じものを二重に収録しないための目印)</summary>
    public string? AutoKey { get; set; }

    /// <summary>まとまりに含まれるエフェクトの名前(表示用)</summary>
    public List<string>? EffectNames { get; set; }
}

public sealed class EffectFile
{
    public Dictionary<int, List<EffectEntry>> Slots { get; set; } = new();

    /// <summary>YMM4でエフェクトをコピーした時のデータ(貼り付け形式の見本)。まとまりの適用に使う</summary>
    public List<ClipFormat>? Sample { get; set; }
}

/// <summary>
/// 映像エフェクト・音声エフェクトの一覧(それぞれ、タブごとに独立)。
/// %AppData%\YmmExplorer\effects-video.json / effects-audio.json
/// </summary>
public sealed class EffectStore
{
    static readonly Dictionary<TabKind, EffectStore> instances = new();

    public static EffectStore For(TabKind kind)
    {
        lock (instances)
        {
            if (!instances.TryGetValue(kind, out var s))
                instances[kind] = s = new EffectStore(kind switch
                {
                    TabKind.VideoEffect => "effects-video.json",
                    TabKind.AudioEffect => "effects-audio.json",
                    _ => "texts.json",
                }, kind);
            return s;
        }
    }

    readonly string filePath;
    readonly object gate = new();
    EffectFile data = new();

    public event Action<int>? Changed;

    public TabKind Kind { get; }

    /// <summary>ショートカットキー用の種類名</summary>
    public string ShortcutKind => Kind switch { TabKind.VideoEffect => "video-effect", TabKind.AudioEffect => "audio-effect", _ => "text" };

    EffectStore(string fileName, TabKind kind)
    {
        Kind = kind;
        filePath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", fileName);
        try
        {
            if (File.Exists(filePath))
                data = JsonSerializer.Deserialize<EffectFile>(File.ReadAllText(filePath)) ?? new();
        }
        catch { data = new(); }
    }

    List<EffectEntry> Slot(int slot)
    {
        if (!data.Slots.TryGetValue(slot, out var l)) data.Slots[slot] = l = new();
        return l;
    }

    /// <summary>すべてのスロットの登録(スロットの番号順)</summary>
    public List<EffectEntry> Everything()
    {
        lock (gate) return data.Slots.OrderBy(kv => kv.Key).SelectMany(kv => kv.Value).ToList();
    }

    public IReadOnlyList<EffectEntry> All(int slot)
    {
        lock (gate) return Slot(slot).ToList();
    }

    public List<ClipFormat>? GetSample()
    {
        lock (gate) return data.Sample is { Count: > 0 } ? data.Sample : null;
    }

    public void SetSample(List<ClipFormat>? sample)
    {
        lock (gate) data.Sample = sample;
        Save();
    }

    /// <summary>すべてのタブから、IDで探す(ショートカットキーの実行用)</summary>
    public EffectEntry? Find(string id)
    {
        lock (gate) return data.Slots.Values.SelectMany(l => l).FirstOrDefault(e => e.Id == id);
    }

    /// <summary>まとめて追加する(保存は1回だけ)</summary>
    public void AddMany(int slot, IEnumerable<EffectEntry> entries)
    {
        lock (gate) Slot(slot).AddRange(entries);
        Save();
        Changed?.Invoke(slot);
    }

    /// <summary>自動収録したもので、サムネイルの無いものに、名前のサムネイルを付ける。付けた数</summary>
    public int FillAutoThumbnails(int slot, string kindLabel)
    {
        var targets = All(slot).Where(e => e.AutoKey != null && string.IsNullOrEmpty(e.Thumbnail)).ToList();
        int n = 0;
        foreach (var e in targets)
            if (NameThumb.Encode(e.Name, kindLabel) is { } b64) { e.Thumbnail = b64; n++; }
        if (n > 0) { Save(); Changed?.Invoke(slot); }
        return n;
    }

    public void Add(int slot, EffectEntry entry)
    {
        lock (gate) Slot(slot).Add(entry);
        Save();
        Changed?.Invoke(slot);
    }

    public int Remove(int slot, IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids);
        int removed;
        lock (gate) removed = Slot(slot).RemoveAll(e => set.Contains(e.Id));
        if (removed > 0) { Save(); Changed?.Invoke(slot); }
        return removed;
    }

    public void Move(int slot, IEnumerable<string> ids, string how)
    {
        var set = new HashSet<string>(ids);
        lock (gate) Reorder.Apply(Slot(slot), e => set.Contains(e.Id), how);
        Save();
        Changed?.Invoke(slot);
    }

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
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath)!);
                File.WriteAllText(filePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
        catch { }
    }
}

/// <summary>画面に並べるエフェクト</summary>
public sealed class EffectItem : NotifyBase
{
    public EffectEntry Entry { get; }
    readonly EffectStore store;

    /// <summary>サムネイルが無い時に出す目印(映像: ✦ / 音声: ♪)</summary>
    public string Placeholder { get; }

    /// <summary>このエフェクト(テキスト)に割り当てたショートカットキー</summary>
    public string ShortcutKeys
    {
        get => ShortcutStore.Instance.KeysFor(store.ShortcutKind, Entry.Id);
        set { ShortcutStore.Instance.Set(store.ShortcutKind, Entry.Id, Name, value ?? ""); Raise(nameof(ShortcutKeys)); }
    }

    public EffectItem(EffectEntry e, EffectStore s, string placeholder)
    {
        Entry = e;
        store = s;
        Placeholder = placeholder;
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

    /// <summary>保存してあるデータの指紋。内容が同じなら同じ値になる(別々に登録したのに同じ値なら、同じ内容を保存している)</summary>
    public string DataId => IsTextStyle ? ClipboardRaw.FingerprintText(Entry.StyleJson!)
                          : IsBundle ? ClipboardRaw.FingerprintText(Entry.EffectsJson!)
                          : ClipboardRaw.Fingerprint(Entry.Formats);

    public string CreatedText => Entry.Created.ToString("yyyy/MM/dd HH:mm");

    /// <summary>選んだアイテムから読み取った、エフェクトのまとまり</summary>
    public bool IsBundle => Entry.EffectsJson != null || Entry.StyleJson != null;

    /// <summary>テキストの書式(選択中のテキストアイテムに適用する)</summary>
    public bool IsTextStyle => Entry.StyleJson != null;

    /// <summary>書式に含まれる項目(1行)</summary>
    public string StyleText => Entry.StyleKeys is { Count: > 0 } k ? "含まれる項目: " + string.Join("、", k) : "";

    /// <summary>まとまりに含まれるエフェクトの名前(1行ずつ)</summary>
    public string BundleText => string.Join("\n", Entry.EffectNames ?? new List<string>());

    public string Summary
    {
        get
        {
            if (!string.IsNullOrEmpty(Entry.Description)) return Entry.Description;
            if (IsBundle)
            {
                var names = Entry.EffectNames ?? new List<string>();
                var head = string.Join("、", names.Take(3));
                return $"まとまり {names.Count} 個: {head}{(names.Count > 3 ? "…" : "")}";
            }
            long bytes = Entry.Formats.Sum(f => (long)f.ByteCount);
            return $"{Entry.Formats.Count} 形式 / {FileItem.FormatSize(bytes)}";
        }
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
        store.Save();
        Raise(nameof(Thumb));
        Raise(nameof(HasNoThumb));
        Raise(nameof(HasCustomThumb));
        return true;
    }
}

/// <summary>
/// 映像エフェクト / 音声エフェクト1つ分のタブの中身(一覧・選択・追加・適用・並べ替え・サムネイル)。
/// 画面の DataContext として使います。
/// </summary>
public sealed class EffectLibraryVm : NotifyBase, IDisposable
{
    readonly ExplorerViewModel owner;
    readonly TabKind kind;
    readonly EffectStore store;
    readonly ExplorerSettings settings = ExplorerSettings.Instance;
    readonly DispatcherTimer watchTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };
    List<EffectItem> selected = new();

    public string Title => kind switch
    {
        TabKind.VideoEffect => "映像エフェクト",
        TabKind.AudioEffect => "音声エフェクト",
        _ => "テキスト",
    };
    public string Placeholder => kind switch { TabKind.VideoEffect => "✦", TabKind.AudioEffect => "♪", _ => "Aa" };

    /// <summary>テキストのタブか(エフェクトのまとまりの読み取りは使わず、テキストアイテムをそのまま保存・配置する)</summary>
    public bool IsText => kind == TabKind.Text;

    /// <summary>画面に出す呼び名(エフェクト / テキスト)</summary>
    string Noun => IsText ? "テキスト" : "エフェクト";

    /// <summary>いま開いているタブの番号(このエフェクトのタブを開いていない時は0)</summary>
    int Slot => owner.SelectedKind == kind ? owner.SelectedSlot : 0;

    public ObservableCollection<EffectItem> Items { get; } = new();
    public bool HasNoItems => Items.Count == 0;

    string search = "";
    /// <summary>このタブの検索(名前・メモ・説明・中のエフェクト名・内部タグ)。一覧の表示だけを絞る</summary>
    public string Search
    {
        get => search;
        set
        {
            if (!Set(ref search, value ?? "")) return;
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(Items);
            var q = search.Trim();
            view.Filter = q.Length == 0 ? null : o => o is EffectItem it && SearchTags.Matches(q, it.Entry.AutoKey ?? "", it.Entry.Name, it.Entry.Memo,
                it.Entry.Description, it.Entry.EffectNames == null ? null : string.Join(" ", it.Entry.EffectNames));
        }
    }

    string message = "";
    public string Message { get => message; private set => Set(ref message, value); }

    public string SelectionText => $"{selected.Count} / {Items.Count} 件を選択中";

    EffectItem? selectedItem;
    public EffectItem? SelectedItem
    {
        get => selectedItem;
        private set
        {
            if (!Set(ref selectedItem, value)) return;
            Raise(nameof(HasSelected));
            Raise(nameof(HasNoSelected));
        }
    }
    public bool HasSelected => selectedItem != null;
    public bool HasNoSelected => selectedItem == null;
    public EffectItem? FirstSelected => selected.FirstOrDefault();

    /// <summary>ダブルクリックした時にYMM4へ送るキー操作(設定画面で変更できる)</summary>
    public string ApplyKeys
    {
        get => (kind switch
        {
            TabKind.VideoEffect => settings.VideoEffectApplyKeys,
            TabKind.AudioEffect => settings.AudioEffectApplyKeys,
            _ => settings.TextApplyKeys,
        }) ?? "Ctrl+V";
        set
        {
            value = string.IsNullOrWhiteSpace(value) ? "Ctrl+V" : value.Trim();
            if (value == ApplyKeys) return;
            switch (kind)
            {
                case TabKind.VideoEffect: settings.VideoEffectApplyKeys = value; break;
                case TabKind.AudioEffect: settings.AudioEffectApplyKeys = value; break;
                default: settings.TextApplyKeys = value; break;
            }
            settings.Save();
            Raise(nameof(ApplyKeys));
        }
    }

    /// <summary>「選択中のアイテムから読み取る」時にYMM4へ送るコピーのキー(設定画面で変更できる)</summary>
    public string CopyKeys
    {
        get => string.IsNullOrWhiteSpace(settings.ItemCopyKeys) ? "Ctrl+C" : settings.ItemCopyKeys;
        set
        {
            value = string.IsNullOrWhiteSpace(value) ? "Ctrl+C" : value.Trim();
            if (value == CopyKeys) return;
            settings.ItemCopyKeys = value;
            settings.Save();
            Raise(nameof(CopyKeys));
        }
    }

    /// <summary>テキストの書式として「保存しない・適用しない」項目(カンマ区切り。設定画面で変更できる)</summary>
    public string ExcludeKeys
    {
        get => string.IsNullOrWhiteSpace(settings.TextExcludeKeys) ? "Text,Frame,Layer,Length,Group,Remark,ContentOffset,IsLocked,IsHidden,X,Y,Z" : settings.TextExcludeKeys;
        set
        {
            value = (value ?? "").Trim();
            if (value == settings.TextExcludeKeys) return;
            settings.TextExcludeKeys = value;
            settings.Save();
            Raise(nameof(ExcludeKeys));
        }
    }

    /// <summary>書式・エフェクトを適用する時、選択中の元のアイテムを削除して置き換える(オフなら、適用したコピーを追加する)</summary>
    public bool ReplaceOriginal
    {
        get => settings.ReplaceOriginalItem;
        set
        {
            if (settings.ReplaceOriginalItem == value) return;
            settings.ReplaceOriginalItem = value;
            settings.Save();
            Raise(nameof(ReplaceOriginal));
        }
    }

    /// <summary>書き換えられなかった独自形式を、クリップボードから外して貼り付ける</summary>
    public bool DropUnmodified
    {
        get => settings.DropUnmodifiedFormats;
        set
        {
            if (settings.DropUnmodifiedFormats == value) return;
            settings.DropUnmodifiedFormats = value;
            settings.Save();
            Raise(nameof(DropUnmodified));
        }
    }

    /// <summary>
    /// エフェクトのまとまりを、選択中のアイテムのエフェクトに「追加」する(オフなら、既存のエフェクトをすべて削除してから追加する)。
    /// 映像エフェクトと音声エフェクトで、別々に設定する
    /// </summary>
    public bool AppendEffects
    {
        get => kind == TabKind.AudioEffect ? settings.AudioEffectAppend : settings.VideoEffectAppend;
        set
        {
            if (AppendEffects == value) return;
            if (kind == TabKind.AudioEffect) settings.AudioEffectAppend = value;
            else settings.VideoEffectAppend = value;
            settings.Save();
            Raise(nameof(AppendEffects));
            Raise(nameof(RemoveBeforeApply));
        }
    }

    /// <summary>適用の前に、選択中のアイテムのエフェクトをすべて削除する(AppendEffects の反対)</summary>
    public bool RemoveBeforeApply
    {
        get => !AppendEffects;
        set => AppendEffects = !value;
    }

    /// <summary>YMM4の内部機能(選択中のアイテムの直接取得・書き換え)を使う</summary>
    public bool UseYmmApi
    {
        get => settings.UseYmmApi;
        set
        {
            if (settings.UseYmmApi == value) return;
            settings.UseYmmApi = value;
            settings.Save();
            Raise(nameof(UseYmmApi));
        }
    }

    public bool HasSample => store.GetSample() != null;
    public string SampleText => HasSample
        ? "貼り付け形式の見本: 登録済み"
        : "貼り付け形式の見本: 未登録(まとまりを適用するのに必要です。1回だけ登録します)";

    public System.Windows.Input.ICommand MoveCommand { get; }
    public System.Windows.Input.ICommand RemoveCommand { get; }
    public System.Windows.Input.ICommand PasteThumbnailCommand { get; }
    public System.Windows.Input.ICommand ClearThumbnailCommand { get; }
    public System.Windows.Input.ICommand WatchCommand { get; }
    public System.Windows.Input.ICommand CancelWatchCommand { get; }
    public System.Windows.Input.ICommand PreviewThumbnailCommand { get; }
    public System.Windows.Input.ICommand AutoRegisterCommand { get; }

    public EffectLibraryVm(ExplorerViewModel owner, TabKind kind)
    {
        this.owner = owner;
        this.kind = kind;
        store = EffectStore.For(kind);
        store.Changed += OnStoreChanged;

        MoveCommand = new RelayCommand(p => Move(p as string ?? ""), _ => selected.Count > 0);
        RemoveCommand = new RelayCommand(_ => RemoveSelected(), _ => selected.Count > 0);
        PasteThumbnailCommand = new RelayCommand(_ => SetThumbnailFromClipboard(), _ => selectedItem != null);
        ClearThumbnailCommand = new RelayCommand(_ => SetThumbnail(selectedItem, null, "サムネイルを削除しました"),
                                                 _ => selectedItem != null && selectedItem.HasCustomThumb);
        WatchCommand = new RelayCommand(_ => StartWatch(), _ => !IsWatching);
        CancelWatchCommand = new RelayCommand(_ => StopWatch("コピーの待ち受けを中止しました"), _ => IsWatching);
        watchTimer.Tick += (_, _) => OnWatchTick();
        InitPendingCommands();
        PreviewThumbnailCommand = new RelayCommand(_ => SetThumbnailFromPreview(), _ => selectedItem != null);
        AutoRegisterCommand = new RelayCommand(_ =>
        {
            if (Slot < 1) { Message = "⚠ このタブを開いた状態で押してください(タブが選ばれていません)"; return; }
            try { Message = AutoRegisterBuiltins(Slot); }
            catch (Exception ex) { Message = "⚠ 自動収録に失敗しました: " + ex.GetBaseException().Message; }
        });
    }

    public void Dispose()
    {
        store.Changed -= OnStoreChanged;
        watchTimer.Stop();
    }

    void OnStoreChanged(int slot)
    {
        if (slot == Slot) Reload();
    }

    /// <summary>いま開いているタブの中身を読み込み直す</summary>
    public void Reload()
    {
        selected = new();
        SelectedItem = null;
        Items.Clear();
        Message = "";
        if (Slot >= 1)
            foreach (var e in store.All(Slot)) Items.Add(new EffectItem(e, store, Placeholder));
        Raise(nameof(HasNoItems));
        Raise(nameof(SelectionText));
    }

    public void SetSelection(IEnumerable<EffectItem> sel)
    {
        selected = sel.ToList();
        SelectedItem = selected.Count == 1 ? selected[0] : null;
        Raise(nameof(SelectionText));
    }

    // ---------- 追加(YMM4でコピーした内容をそのまま保存) ----------
    /// <summary>いまクリップボードにある内容を、新しいエフェクトとして追加する</summary>
    public void Capture(IntPtr hwnd)
    {
        if (Slot < 1) return;
        var formats = ClipboardRaw.Capture(hwnd, out var error);
        if (formats == null) { Message = error; return; }
        if (formats.Count == 0)
        {
            Message = $"クリップボードが空です。YMM4で{Noun}をコピー(Ctrl+C)してから押してください";
            return;
        }
        if (!AddEntry(formats)) return;
        Message = ClipboardRaw.LooksLikeData(formats)
            ? $"{Noun}を追加しました(数値などはそのまま保存されています)"
            : "追加しました。ただしYMM4のデータではないかもしれません。コピーできているか確認してください";
    }

    /// <summary>同じ内容がすでにあれば追加しない(YMM4で別のアイテムをコピーできていない可能性を知らせる)</summary>
    bool AddEntry(List<ClipFormat> formats)
    {
        var same = ClipboardRaw.FindSame(formats, store.All(Slot).Select(e => (e.Name, (List<ClipFormat>?)e.Formats)));
        if (same != null)
        {
            Message = $"同じ内容が「{same}」にすでに登録されています(データID {ClipboardRaw.Fingerprint(formats)})。"
                + $"YMM4で別の{Noun}をコピーできているか確認してください";
            return false;
        }
        store.Add(Slot, new EffectEntry { Name = $"{Title} {DateTime.Now:MM/dd HH:mm}", Formats = formats });
        return true;
    }

    // ---------- 選択中のアイテムから、エフェクトのまとまりを読み取る ----------
    /// <summary>
    /// YMM4で選択中のアイテムをコピーして(キー操作を自動で送る)、そのアイテムのエフェクトを1つのまとまりとして保存する。
    /// </summary>
    public async Task ReadSelectedAsync(object? ownView)
    {
        if (Slot < 1) return;
        if (UseYmmApi && TryReadDirect()) return;       // YMM4の選択中アイテムを直接読む(コピー不要)
        if (!await CopySelectedAsync(ownView)) return;
        ReadFromClipboard();
    }

    /// <summary>YMM4で選択中のアイテムをコピーして、クリップボードが落ち着くまで待つ。コピーできたら true</summary>
    async Task<bool> CopySelectedAsync(object? ownView)
    {
        if (IsWatching) StopWatch("");
        Message = $"選択中のアイテムをコピーして、{(IsText ? "テキスト" : "エフェクト")}を読み取っています…";

        var before = ClipboardRaw.SequenceNumber;
        var skipped = await TimelinePaste.SendKeysAsync(ownView, CopyKeys);
        if (skipped != null)
        {
            Message = $"{skipped}YMM4でアイテムを選んで {CopyKeys} を押してから、「コピー済みのアイテムから読み取る」を押してください";
            return false;
        }

        // コピーされて、クリップボードの変化が止まるまで待つ(最大4秒)
        var deadline = DateTime.Now.AddSeconds(4);
        var last = before;
        var lastChange = DateTime.Now;
        while (DateTime.Now < deadline)
        {
            await Task.Delay(150);
            var seq = ClipboardRaw.SequenceNumber;
            if (seq == before) continue;
            if (seq != last) { last = seq; lastChange = DateTime.Now; }
            else if (DateTime.Now - lastChange > TimeSpan.FromMilliseconds(500)) break;
        }
        if (last == before)
        {
            Message = $"コピーされませんでした。アイテムを選んでいるか、コピーのキー({CopyKeys})が合っているか確認してください";
            return false;
        }
        return true;
    }

    /// <summary>いまクリップボードにある、コピー済みのアイテムから、エフェクトのまとまりを読み取る</summary>
    public void ReadFromClipboard()
    {
        if (Slot < 1) return;
        if (UseYmmApi && TryReadDirect()) return;
        if (IsText) { ReadTextStyleFromClipboard(); return; }       // テキスト: アイテムごとではなく、書式とエフェクトだけを保存
        var formats = ClipboardRaw.Capture(IntPtr.Zero, out var error);
        if (formats == null) { Message = error; return; }
        if (formats.Count == 0) { Message = "クリップボードが空です。YMM4でアイテムをコピーしてください"; return; }

        var (result, info) = EffectBundle.Extract(formats, kind);
        if (result == null) { Message = info; return; }
        if (result.Count == 0)
        {
            Message = $"選択中のアイテムに、{Title}はありません";
            return;
        }

        var fp = ClipboardRaw.FingerprintText(result.Json);
        var same = store.All(Slot).FirstOrDefault(e => e.EffectsJson != null && ClipboardRaw.FingerprintText(e.EffectsJson) == fp);
        if (same != null)
        {
            Message = $"同じエフェクトのまとまりが「{same.Name}」にすでにあります(データID {fp})";
            return;
        }

        store.Add(Slot, new EffectEntry
        {
            Name = $"{Title}のまとまり({result.Count}個) {DateTime.Now:MM/dd HH:mm}",
            EffectsJson = result.Json,
            EffectNames = result.Names,
        });
        Message = $"{result.Count} 個のエフェクトを、1つのまとまりとして保存しました";
    }

    // ---------- テキストの書式: 読み取り → 保存する項目を選ぶ → 保存 ----------
    JsonObject? pendingItem;
    bool pendingDirect;     // YMM4から直接読み取った書式(項目名 → 値のJSON)

    /// <summary>読み取った項目の一覧(チェックを入れた項目だけを保存する)</summary>
    public ObservableCollection<StyleKeyOption> PendingKeys { get; } = new();

    public bool HasPending => pendingItem != null;

    /// <summary>YMM4から直接読み取った書式(対象パラメータのスイッチで選ぶ)</summary>
    public bool PendingIsDirect => pendingItem != null && pendingDirect;

    /// <summary>クリップボードから読み取った書式(項目の一覧で選ぶ)</summary>
    public bool PendingIsList => pendingItem != null && !pendingDirect;

    /// <summary>対象パラメータのスイッチの一覧</summary>
    public ObservableCollection<StyleGroupOption> PendingGroups { get; } = new();
    List<Ymm.StyleGroupData>? pendingGroupData;

    void RaisePending()
    {
        Raise(nameof(HasPending));
        Raise(nameof(PendingIsDirect));
        Raise(nameof(PendingIsList));
        RefreshPendingThumb();
    }

    // ---------- 書式から作るサムネイル(背景色と見本の文字を決めると、自動で作る) ----------
    public bool ThumbAuto { get => settings.TextThumbAuto; set { settings.TextThumbAuto = value; settings.Save(); Raise(nameof(ThumbAuto)); } }
    public string ThumbBg
    {
        get => string.IsNullOrWhiteSpace(settings.TextThumbBg) ? "#FF202228" : settings.TextThumbBg!;
        set
        {
            value = (value ?? "").Trim();
            if (value.Length > 0 && !value.StartsWith('#')) value = "#" + value;
            if (TextThumb.ColorOf("\"" + value + "\"") == null) { Raise(nameof(ThumbBg)); return; }
            settings.TextThumbBg = value; settings.Save();
            Raise(nameof(ThumbBg)); Raise(nameof(ThumbBgBrush));
            RefreshPendingThumb();
        }
    }
    public System.Windows.Media.Brush ThumbBgBrush
    {
        get
        {
            var b = new System.Windows.Media.SolidColorBrush(TextThumb.ColorOf("\"" + ThumbBg + "\"") ?? System.Windows.Media.Colors.Black);
            b.Freeze();
            return b;
        }
    }
    public string ThumbSample
    {
        get => string.IsNullOrEmpty(settings.TextThumbSample) ? "あア亜Aa" : settings.TextThumbSample!;
        set { settings.TextThumbSample = value; settings.Save(); Raise(nameof(ThumbSample)); RefreshPendingThumb(); }
    }
    /// <summary>背景色の候補(クリックで選ぶ)</summary>
    public static IReadOnlyList<string> ThumbBgChoices { get; } = new[]
        { "#FF202228", "#FF000000", "#FFFFFFFF", "#FF808080", "#FF00B140", "#FF0047BB", "#FF1E3A5F", "#FFF5E6C8", "#FFFFC0CB", "#FF3A1F4A" };

    System.Windows.Media.Imaging.BitmapSource? pendingThumb;
    public System.Windows.Media.Imaging.BitmapSource? PendingThumb { get => pendingThumb; private set => Set(ref pendingThumb, value); }

    System.Windows.Input.ICommand? setThumbBgCommand, makeThumbFromStyleCommand;
    public System.Windows.Input.ICommand SetThumbBgCommand => setThumbBgCommand ??= new RelayCommand(p => { if (p is string c) ThumbBg = c; });
    /// <summary>選んでいる書式(保存済み)から、サムネイルを作り直す</summary>
    public System.Windows.Input.ICommand MakeThumbFromStyleCommand => makeThumbFromStyleCommand ??= new RelayCommand(_ =>
    {
        if (selectedItem?.Entry.StyleJson is not { } json) { Message = "テキストの書式を選んでください"; return; }
        var bmp = TextThumb.FromStyleJson(json, ThumbSample, ThumbBg);
        SetThumbnail(selectedItem, bmp, "書式から、サムネイルを作りました");
    }, _ => selectedItem?.Entry.StyleJson != null);

    /// <summary>いま読み取っている書式の値(項目名 → JSON)</summary>
    Dictionary<string, string> PendingValues()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (pendingDirect && pendingGroupData != null)
        {
            foreach (var d in pendingGroupData) foreach (var kv in d.Values) values[kv.Key] = kv.Value;
        }
        else if (pendingItem != null)
        {
            foreach (var kv in pendingItem) if (kv.Value != null) values[kv.Key] = kv.Value.ToJsonString();
        }
        return values;
    }

    void RefreshPendingThumb()
    {
        if (!IsText || pendingItem == null) { PendingThumb = null; return; }
        PendingThumb = TextThumb.Render(PendingValues(), ThumbSample, ThumbBg);
    }

    string pendingName = "";
    public string PendingName { get => pendingName; set => Set(ref pendingName, value ?? ""); }

    public System.Windows.Input.ICommand SavePendingCommand { get; private set; } = null!;
    public System.Windows.Input.ICommand CancelPendingCommand { get; private set; } = null!;
    public System.Windows.Input.ICommand CheckAllPendingCommand { get; private set; } = null!;
    public System.Windows.Input.ICommand UncheckAllPendingCommand { get; private set; } = null!;

    void InitPendingCommands()
    {
        SavePendingCommand = new RelayCommand(_ => SavePending(), _ => HasPending);
        CancelPendingCommand = new RelayCommand(_ => ClearPending("保存を取りやめました"), _ => HasPending);
        CheckAllPendingCommand = new RelayCommand(p =>
        {
            foreach (var k in PendingKeys) k.IsChecked = true;
            foreach (var g in PendingGroups) if (g.IsAvailable) g.IsChecked = true;
        }, _ => HasPending);
        UncheckAllPendingCommand = new RelayCommand(p =>
        {
            foreach (var k in PendingKeys) k.IsChecked = false;
            foreach (var g in PendingGroups) g.IsChecked = false;
        }, _ => HasPending);
    }

    /// <summary>コピー済みのテキストアイテムを読み取って、保存する項目を選ぶ状態にする</summary>
    void ReadTextStyleFromClipboard()
    {
        var formats = ClipboardRaw.Capture(IntPtr.Zero, out var error);
        if (formats == null) { Message = error; return; }
        if (formats.Count == 0) { Message = "クリップボードが空です。YMM4でテキストのアイテムをコピーしてください"; return; }
        TryAddTextStyle(formats);
    }

    bool TryAddTextStyle(List<ClipFormat> formats)
    {
        var (item, info) = TextStyle.ExtractAll(formats);
        if (item == null) { Message = info; return false; }

        // 初期状態: 「保存・適用しない項目」(設定)以外にチェック。あとから自由に変えられる
        var exclude = TextStyle.ParseKeys(ExcludeKeys);
        pendingItem = item;
        PendingKeys.Clear();
        PendingGroups.Clear();
        pendingGroupData = null;
        foreach (var kv in item)
        {
            if (kv.Key == "$type") continue;
            PendingKeys.Add(new StyleKeyOption(kv.Key, PreviewOf(kv.Value), !exclude.Contains(kv.Key)));
        }
        pendingDirect = false;
        PendingName = $"テキストの書式 {DateTime.Now:MM/dd HH:mm}";
        RaisePending();
        Message = $"{PendingKeys.Count} 項目を読み取りました。保存する項目を選んで、「この内容で保存」を押してください";
        return true;
    }

    static string PreviewOf(JsonNode? v)
    {
        if (v == null) return "(なし)";
        var text = v is JsonArray a ? $"[{a.Count}個]" : EffectBundle.ToJson(v);
        text = text.Trim('"');
        return text.Length > 70 ? text[..70] + "…" : text;
    }

    void ClearPending(string message)
    {
        pendingItem = null;
        pendingDirect = false;
        PendingKeys.Clear();
        PendingGroups.Clear();
        pendingGroupData = null;
        PendingName = "";
        RaisePending();
        Message = message;
    }

    void SavePending()
    {
        if (pendingItem == null) return;
        if (pendingDirect) { SavePendingDirect(); return; }
        var chosen = PendingKeys.Where(k => k.IsChecked).Select(k => k.Key).ToList();
        if (chosen.Count == 0) { Message = "保存する項目を、1つ以上選んでください"; return; }

        var style = new JsonObject();
        if (!pendingDirect && pendingItem["$type"] != null) style["$type"] = pendingItem["$type"]!.DeepClone();
        foreach (var key in chosen) style[key] = pendingItem[key]?.DeepClone();
        var json = EffectBundle.ToJson(style);

        var fp = ClipboardRaw.FingerprintText(json);
        var same = store.All(Slot).FirstOrDefault(e => e.StyleJson != null && ClipboardRaw.FingerprintText(e.StyleJson) == fp);
        if (same != null)
        {
            Message = $"同じ書式が「{same.Name}」にすでにあります(データID {fp})";
            return;
        }

        var effectNames = new List<string>();
        foreach (var kv in style)
            if (kv.Key.EndsWith("Effects", StringComparison.OrdinalIgnoreCase) && kv.Value is JsonArray arr)
                effectNames.AddRange(EffectBundle.NamesOf(arr));
        var desc = $"書式 {chosen.Count}項目";
        if (effectNames.Count > 0)
            desc += $" / エフェクト {effectNames.Count}個: {string.Join("、", effectNames.Take(3))}{(effectNames.Count > 3 ? "…" : "")}";

        store.Add(Slot, new EffectEntry
        {
            Name = string.IsNullOrWhiteSpace(PendingName) ? $"テキストの書式 {DateTime.Now:MM/dd HH:mm}" : PendingName.Trim(),
            StyleJson = json,
            StyleKeys = chosen,
            EffectNames = effectNames,
            Description = desc,
            StyleDirect = pendingDirect,
            Thumbnail = ThumbAuto && PendingThumb != null ? PresetThumb.Encode(PendingThumb) : null,
        });
        ClearPending($"テキストの書式を保存しました({desc})");
    }

    /// <summary>対象パラメータでオンにした項目だけを、書式として保存する</summary>
    void SavePendingDirect()
    {
        var chosen = PendingGroups.Where(g => g.IsAvailable && g.IsChecked).ToList();
        if (chosen.Count == 0 || pendingGroupData == null)
        {
            Message = "保存する項目(対象パラメータ)を、1つ以上オンにしてください";
            return;
        }

        var style = new JsonObject();
        foreach (var option in chosen)
        {
            var data = pendingGroupData.FirstOrDefault(d => d.Group.Id == option.Id);
            if (data == null) continue;
            foreach (var kv in data.Values)
            {
                JsonNode? node;
                try { node = JsonNode.Parse(kv.Value); }
                catch { node = JsonValue.Create(kv.Value); }
                style[kv.Key] = node;
            }
        }
        var json = EffectBundle.ToJson(style);

        var fp = ClipboardRaw.FingerprintText(json);
        var same = store.All(Slot).FirstOrDefault(e => e.StyleJson != null && ClipboardRaw.FingerprintText(e.StyleJson) == fp);
        if (same != null)
        {
            Message = $"同じ書式が「{same.Name}」にすでにあります(データID {fp})";
            return;
        }

        // 次回も同じスイッチの状態から始められるよう、覚えておく
        settings.TextStyleGroups = PendingGroups.Where(g => g.IsChecked).Select(g => g.Id).ToList();
        settings.Save();

        var effectNames = new List<string>();
        if (style["VideoEffects"] is JsonArray arr) effectNames.AddRange(EffectBundle.NamesOf(arr));
        var labels = chosen.Select(g => g.Label).ToList();
        var desc = $"対象 {labels.Count}項目: {string.Join("、", labels.Take(5))}{(labels.Count > 5 ? "…" : "")}";

        store.Add(Slot, new EffectEntry
        {
            Name = string.IsNullOrWhiteSpace(PendingName) ? $"テキストの書式 {DateTime.Now:MM/dd HH:mm}" : PendingName.Trim(),
            StyleJson = json,
            StyleKeys = labels,
            EffectNames = effectNames,
            Description = desc,
            StyleDirect = true,
            Thumbnail = ThumbAuto && PendingThumb != null ? PresetThumb.Encode(PendingThumb) : null,
        });
        ClearPending($"テキストの書式を保存しました({desc})");
    }

    /// <summary>
    /// いまクリップボードにある内容(YMM4でエフェクトを1つコピーしたもの)を、「貼り付け形式の見本」として登録する。
    /// まとまりを適用する時、この見本の中のエフェクト部分を入れ替えて使う。
    /// </summary>
    public void RegisterSample()
    {
        var formats = ClipboardRaw.Capture(IntPtr.Zero, out var error);
        if (formats == null) { Message = error; return; }
        if (formats.Count == 0) { Message = "クリップボードが空です。YMM4でエフェクトを1つコピーしてから押してください"; return; }
        if (!EffectBundle.IsUsableSample(formats, kind))
        {
            Message = "この内容は見本として使えませんでした(エフェクトのデータをJSONとして読めませんでした)。"
                + "YMM4でエフェクトを1つコピーして、もう一度お試しください。設定の診断で、中身の形式を確認できます";
            return;
        }
        store.SetSample(formats);
        Raise(nameof(HasSample));
        Raise(nameof(SampleText));
        Message = "貼り付け形式の見本を登録しました。これでエフェクトのまとまりを適用できます";
    }

    // コピーを待って追加: YMM4でコピーするだけで追加される(プラグインの画面に戻らなくてよい)
    bool isWatching;
    uint watchSeq, watchPendingSeq;
    DateTime watchDeadline, watchPendingSince;

    public bool IsWatching { get => isWatching; private set => Set(ref isWatching, value); }

    string watchText = "";
    public string WatchText { get => watchText; private set => Set(ref watchText, value); }

    void StartWatch()
    {
        if (Slot < 1) return;
        watchSeq = ClipboardRaw.SequenceNumber;
        watchPendingSeq = 0;
        watchDeadline = DateTime.Now.AddMinutes(3);
        WatchText = $"YMM4で、追加したい{Noun}をコピー(Ctrl+C)してください。コピーすると自動で追加されます。";
        IsWatching = true;
        watchTimer.Start();
    }

    void StopWatch(string message)
    {
        watchTimer.Stop();
        IsWatching = false;
        WatchText = "";
        Message = message;
    }

    void OnWatchTick()
    {
        if (!IsWatching) { watchTimer.Stop(); return; }
        if (DateTime.Now > watchDeadline) { StopWatch("3分たったので、待ち受けを中止しました"); return; }

        var seq = ClipboardRaw.SequenceNumber;
        if (seq == watchSeq) return;
        // YMM4はコピー時にクリップボードを何度か書き換えることがあるので、変化が止まってから(約0.6秒後に)読み取る
        if (seq != watchPendingSeq) { watchPendingSeq = seq; watchPendingSince = DateTime.Now; return; }
        if (DateTime.Now - watchPendingSince < TimeSpan.FromMilliseconds(600)) return;

        var formats = ClipboardRaw.Capture(IntPtr.Zero, out _);
        if (formats == null) return;        // 他のアプリが使用中。次の確認でやり直す
        watchSeq = seq;
        watchPendingSeq = 0;

        if (!ClipboardRaw.LooksLikeData(formats))
        {
            Message = $"コピーされた内容が、YMM4のデータではないようです。{Noun}をもう一度コピーしてください";
            return;
        }
        if (IsText)
        {
            if (TryAddTextStyle(formats)) StopWatch("書式を読み取りました。保存する項目を選んで、「この内容で保存」を押してください");
            return;
        }
        if (AddEntry(formats)) StopWatch($"{Noun}を追加しました");
    }

    // ---------- 適用(ダブルクリック) ----------
    /// <summary>
    /// エフェクトの内容をクリップボードに戻し、YMM4へ ApplyKeys のキー操作を送る。
    /// 適用先は、YMM4のタイムラインで選択中のアイテムです。
    /// </summary>
    public async Task ApplyAsync(EffectItem? item, IntPtr hwnd, object? ownView)
    {
        if (item == null) { Message = $"{Noun}を選んでください"; return; }
        if (IsWatching) StopWatch("");      // 自分でクリップボードを書き換えるので、待ち受けは止める
        // YMM4の内部機能で、選択中のアイテムを直接書き換える(新しいアイテムは増えず、元に戻す履歴にも載る)
        if (UseYmmApi && item.IsTextStyle && item.Entry.StyleDirect) { await ApplyTextStyleDirectAsync(item); return; }
        if (UseYmmApi && !item.IsTextStyle && item.Entry.EffectsJson != null && await ApplyEffectsDirectAsync(item)) return;

        if (item.IsTextStyle) { await ApplyTextStyleAsync(item, ownView); return; }

        if (item.IsBundle) { await ApplyEffectBundleAsync(item, ownView); return; }

        List<ClipFormat> toRestore = item.Entry.Formats;
        if (!ClipboardRaw.RestoreVerified(hwnd, toRestore, out var error, out var dataId)) { Message = error; return; }

        if (!settings.PresetAutoPaste)
        {
            Message = IsText
                ? $"「{item.Name}」をクリップボードにセットしました。タイムラインで {ApplyKeys} で貼り付けてください"
                : $"「{item.Name}」をクリップボードにセットしました。適用したいアイテムを選んで {ApplyKeys} を押してください";
            return;
        }
        var skipped = await TimelinePaste.SendKeysAsync(ownView, ApplyKeys);
        Message = IsText
            ? (skipped == null
                ? $"「{item.Name}」を、シークバーの位置に配置する操作({ApplyKeys})をYMM4へ送りました(データID {dataId})"
                : $"クリップボードにセットしました。{skipped}タイムラインをクリックして {ApplyKeys} で貼り付けてください")
            : (skipped == null
                ? $"「{item.Name}」を、選択中のアイテムに適用する操作({ApplyKeys})をYMM4へ送りました(データID {dataId})"
                : $"クリップボードにセットしました。{skipped}適用したいアイテムを選んで {ApplyKeys} を押してください");
    }

    // ---------- YMM4の内部機能による、読み取りと適用(コピー不要・置き換え不要) ----------
    string EffectLabel => kind == TabKind.VideoEffect ? "映像" : "音声";

    /// <summary>
    /// タイムラインで選択中のアイテムから、直接読み取る。
    /// 戻り値: 処理した(警告を出した場合も含む)なら true。YMM4の内部にアクセスできない時は false(クリップボード方式へ)
    /// </summary>
    bool TryReadDirect()
    {
        if (Ymm.Timeline() == null) return false;
        var items = Ymm.SelectedItems();
        if (items.Count == 0)
        {
            Message = "⚠ YMM4のタイムラインで、アイテムを選んでください(選択中のアイテムがありません)";
            return true;
        }
        return IsText ? ReadTextStyleDirect(items[0]) : ReadEffectsDirect(items[0]);
    }

    bool ReadTextStyleDirect(object first)
    {
        if (!Ymm.IsTextItem(first) && !Ymm.IsVoiceItem(first))
        {
            Message = $"⚠ 選択中のアイテムがテキストではありません(種類: {Ymm.TypeName(first)})。テキストのアイテムを選んでください";
            return true;
        }
        var groups = Ymm.ReadTextStyleGroups(first);
        if (groups.All(g => g.Values.Count == 0))
        {
            Message = "書式を読み取れませんでした。" + Ymm.LastError;
            return true;
        }

        // 前回オンにした項目を引き継ぐ(初めてなら、標準の項目)
        var remembered = settings.TextStyleGroups;
        pendingGroupData = groups;
        PendingKeys.Clear();
        PendingGroups.Clear();
        foreach (var d in groups)
        {
            var available = d.Values.Count > 0;
            var on = available && (remembered == null ? d.Group.DefaultOn : remembered.Contains(d.Group.Id));
            var tip = available
                ? string.Join("\n", d.Values.Select(kv => $"{kv.Key}: {Shorten(kv.Value)}"))
                : "選択中のアイテムには、この項目が見つかりませんでした(YMM4のバージョンによって、項目の名前が違うことがあります)\n探した名前: "
                  + string.Join(", ", d.Group.Props.SelectMany(x => x.Split('|')).Distinct());
            PendingGroups.Add(new StyleGroupOption(d.Group.Id, d.Group.Label, available, on, tip));
        }
        pendingItem = new JsonObject();
        pendingDirect = true;
        PendingName = $"テキストの書式 {DateTime.Now:MM/dd HH:mm}";
        RaisePending();
        Message = $"対象パラメータを読み取りました(アイテムの種類: {Ymm.TypeName(first)})。保存する項目をオンにして、「この内容で保存」を押してください";
        return true;
    }

    static string Shorten(string json)
    {
        var t = json.Replace("\r", "").Replace("\n", " ").Trim('"');
        return t.Length > 80 ? t.Substring(0, 80) + "…" : t;
    }

    bool ReadEffectsDirect(object first)
    {
        var prop = Ymm.EffectsProperty(kind == TabKind.VideoEffect);
        if (Ymm.FindProperty(first.GetType(), prop) == null)
        {
            Message = $"⚠ 選択中のアイテム(種類: {Ymm.TypeName(first)})には、{EffectLabel}エフェクトの項目がありません";
            return true;
        }
        var value = Ymm.Get(first, prop);
        var count = Ymm.Count(value);
        if (count == 0)
        {
            Message = $"選択中のアイテム(種類: {Ymm.TypeName(first)})に、{Title}はありません";
            return true;
        }
        var json = Ymm.ValueToJson(value);
        if (json == null)
        {
            Message = "エフェクトを保存用に変換できませんでした。" + Ymm.LastError;
            return true;
        }

        var names = Ymm.EffectNames(value);
        var fp = ClipboardRaw.FingerprintText(json);
        var same = store.All(Slot).FirstOrDefault(e => e.EffectsJson != null && ClipboardRaw.FingerprintText(e.EffectsJson) == fp);
        if (same != null)
        {
            Message = $"同じエフェクトのまとまりが「{same.Name}」にすでにあります(データID {fp})";
            return true;
        }
        store.Add(Slot, new EffectEntry
        {
            Name = $"{Title}のまとまり({count}個) {DateTime.Now:MM/dd HH:mm}",
            EffectsJson = json,
            EffectNames = names,
            EffectsDirect = true,
        });
        Message = $"{count} 個のエフェクトを、1つのまとまりとして保存しました: {string.Join("、", names.Take(4))}{(names.Count > 4 ? "…" : "")}";
        return true;
    }

    /// <summary>保存した書式を、選択中のテキストアイテムに直接適用する</summary>
    async Task ApplyTextStyleDirectAsync(EffectItem item)
    {
        if (Ymm.Timeline() == null) { Message = "⚠ " + Ymm.LastError; return; }
        var targets = Ymm.SelectedItems();
        if (targets.Count == 0)
        {
            Message = "⚠ YMM4のタイムラインで、書式を適用したいテキストのアイテムを選んでください";
            return;
        }
        var bad = targets.FirstOrDefault(t => !Ymm.IsTextItem(t) && !Ymm.IsVoiceItem(t));
        if (bad != null)
        {
            Message = $"⚠ 選択中のアイテムがテキストではありません(種類: {Ymm.TypeName(bad)})。テキストのアイテムを選んでください";
            return;
        }

        var style = ParseStyle(item.Entry.StyleJson);
        if (style.Count == 0) { Message = "保存した書式を読み取れませんでした"; return; }

        int total = 0;
        var failed = new List<string>();
        try
        {
            foreach (var t in targets)
            {
                var (n, f) = await Ymm.ApplyTextStyleAsync(t, style);
                total += n;
                failed.AddRange(f);
            }
        }
        catch (Exception ex)
        {
            Message = "書式を適用できませんでした: " + (ex.InnerException?.Message ?? ex.Message);
            return;
        }
        Message = $"{targets.Count} 個のテキストに、「{item.Name}」の書式を適用しました({total} 項目)"
            + (failed.Count > 0 ? $"。適用できなかった項目: {string.Join("、", failed.Distinct().Take(5))}" : "");
    }

    static Dictionary<string, string> ParseStyle(string? json)
    {
        var d = new Dictionary<string, string>();
        try
        {
            if (JsonNode.Parse(json ?? "{}") is JsonObject o)
                foreach (var kv in o)
                    if (kv.Key != "$type") d[kv.Key] = kv.Value?.ToJsonString() ?? "null";
        }
        catch { }
        return d;
    }

    /// <summary>
    /// エフェクトのまとまりを、選択中のアイテムに直接適用する。
    /// 戻り値: 処理した(警告を含む)なら true。YMM4で読めない形式の古いデータの時は false(クリップボード方式へ)
    /// </summary>
    async Task<bool> ApplyEffectsDirectAsync(EffectItem item)
    {
        if (Ymm.Timeline() == null) return false;
        var targets = Ymm.SelectedItems();
        if (targets.Count == 0)
        {
            Message = "⚠ YMM4のタイムラインで、エフェクトを適用したいアイテムを選んでください";
            return true;
        }
        var prop = Ymm.EffectsProperty(kind == TabKind.VideoEffect);
        var bad = targets.FirstOrDefault(t => Ymm.FindProperty(t.GetType(), prop) == null);
        if (bad != null)
        {
            Message = $"⚠ 選択中のアイテム(種類: {Ymm.TypeName(bad)})には、{EffectLabel}エフェクトを付けられません";
            return true;
        }

        int ok = 0;
        var errors = new List<string>();
        foreach (var t in targets)
        {
            var p = Ymm.FindProperty(t.GetType(), prop)!;
            // アイテムごとに作り直す(エフェクトの実体を、複数のアイテムで共有しないため)
            var loaded = Ymm.JsonToValue(p.PropertyType, item.Entry.EffectsJson!);
            if (loaded == null) { errors.Add(Ymm.LastError); continue; }
            try
            {
                await Ymm.EditAsync(t, () =>
                {
                    var merged = Ymm.MergeEffects(p.PropertyType, Ymm.Get(t, prop), loaded, AppendEffects);
                    if (merged == null) { errors.Add(Ymm.LastError); return; }
                    if (Ymm.SetValue(t, prop, merged, out var err)) ok++;
                    else errors.Add(err);
                });
            }
            catch (Exception ex) { errors.Add(ex.InnerException?.Message ?? ex.Message); }
        }

        if (ok == 0 && !item.Entry.EffectsDirect) return false;       // 古い形式のデータ: クリップボード方式で試す
        Message = ok > 0
            ? $"{ok} 個のアイテムに、「{item.Name}」のエフェクトを{(AppendEffects ? "追加しました(既存のエフェクトは残しています)" : "追加しました(既存のエフェクトはすべて削除しました)")}"
              + (errors.Count > 0 ? $"(失敗 {errors.Count} 件: {errors[0]})" : "")
            : "エフェクトを適用できませんでした: " + (errors.FirstOrDefault() ?? "原因不明");
        return true;
    }

    /// <summary>保存した書式で、新しいテキストアイテムを作ってシークバーの位置に配置する</summary>
    public async Task PlaceNewTextAsync(EffectItem? item)
    {
        if (item == null) { Message = "書式を選んでください"; return; }
        if (!item.IsTextStyle || !item.Entry.StyleDirect)
        {
            Message = "この書式は、新しいテキストとして配置できません(YMM4から直接読み取った書式で使えます。保存し直してください)";
            return;
        }
        var tl = Ymm.Timeline();
        if (tl == null) { Message = "⚠ " + Ymm.LastError; return; }

        var frame = Ymm.CurrentFrame();
        var selected = Ymm.SelectedItems().FirstOrDefault();
        var layer = selected != null ? Convert.ToInt32(Ymm.Get(selected, "Layer") ?? 0) : 0;
        var textItem = Ymm.CreateDefaultTextItem("テキスト", frame, layer, 150);
        if (textItem == null) { Message = "テキストアイテムを作れませんでした。" + Ymm.LastError; return; }

        await Ymm.ApplyTextStyleAsync(textItem, ParseStyle(item.Entry.StyleJson), useEdit: false);
        var template = Ymm.CreateTemplate("テキスト", new[] { textItem });
        if (template == null) { Message = "配置の準備ができませんでした。" + Ymm.LastError; return; }
        var (ok, msg) = await Ymm.PlaceTemplateAsync(template);
        Message = ok ? $"「{item.Name}」の書式で、新しいテキストを配置しました。{msg}" : "⚠ " + msg;
    }

    // ---------- 標準のエフェクトの自動収録 ----------
    /// <summary>YMM4に最初から入っているエフェクトを、このタブ(スロット)にすべて自動収録する。すでに収録したものは、二重に足さない</summary>
    public string AutoRegisterBuiltins(int slot)
    {
        if (kind != TabKind.VideoEffect && kind != TabKind.AudioEffect) return "このタブでは、自動収録は使えません";
        var video = kind == TabKind.VideoEffect;
        var found = BuiltinCatalog.FindEffects(video, out var info);
        if (found.Count == 0) return info;

        var have = store.All(slot).Where(e => e.AutoKey != null).Select(e => e.AutoKey!).ToHashSet();
        var add = new List<EffectEntry>();
        int failed = 0;
        string firstError = "";
        foreach (var fx in found)
        {
            var key = fx.Type.FullName ?? fx.Type.Name;
            if (have.Contains(key)) continue;
            string? json = null;
            string err = "";
            try { json = BuiltinCatalog.EffectToJson(video, fx.Instance); if (json == null) err = Ymm.LastError; }
            catch (Exception ex) { err = ex.GetBaseException().Message; }
            if (json == null)
            {
                failed++;
                if (firstError.Length == 0) firstError = $"「{fx.Name}」: {err}";
                continue;
            }
            add.Add(new EffectEntry
            {
                Name = fx.Name,
                EffectsJson = json,
                EffectNames = new List<string> { fx.Name },
                EffectsDirect = true,
                AutoKey = key,
                Description = fx.IsBuiltIn ? "標準のエフェクト(自動収録)" : $"プラグインのエフェクト(自動収録: {fx.AssemblyName})",
                // 名前が、そのままサムネイルになる
                Thumbnail = NameThumb.Encode(fx.Name, (video ? "映像エフェクト" : "音声エフェクト") + (fx.IsBuiltIn ? "" : " ・ プラグイン")),
            });
        }
        if (add.Count > 0) store.AddMany(slot, add);
        // 前に自動収録して、サムネイルの無いものにも付ける
        store.FillAutoThumbnails(slot, video ? "映像エフェクト" : "音声エフェクト");
        return $"{info}。{add.Count} 個を自動収録しました"
            + (found.Count - add.Count - failed > 0 ? $"({found.Count - add.Count - failed} 個は収録済み)" : "")
            + (failed > 0 ? $"。保存用に変換できなかったもの {failed} 個(最初の失敗 {firstError})" : "");
    }

    // ---------- サムネイル: YMM4のプレビューを取り込む ----------
    public void SetThumbnailFromPreview()
    {
        var bmp = Ymm.CapturePreview();
        if (bmp == null) { Message = "プレビュー画像を取り込めませんでした。" + Ymm.LastError; return; }
        SetThumbnail(selectedItem, bmp, "YMM4のプレビュー画面をサムネイルにしました");
    }

    // ---------- 選択中のアイテムへの適用(書式・エフェクトのまとまり) ----------
    // YMM4のアイテムを直接書き換える方法が無いため、次の手順にしている:
    //   選択中のアイテムをコピー → そのデータに書式/エフェクトを反映 → (元のアイテムを削除) → 貼り付け
    // 「元のアイテムを置き換える」がオンなら、書式やエフェクトが変わったように置き換わる。

    /// <summary>テキストの書式を、選択中のテキストアイテムに適用する(テキストではない場合は警告して何もしない)</summary>
    async Task ApplyTextStyleAsync(EffectItem item, object? ownView)
    {
        if (!await CopySelectedAsync(ownView)) return;

        var copied = ClipboardRaw.Capture(IntPtr.Zero, out var error);
        if (copied == null) { Message = error; return; }

        var built = TextStyle.ApplyToCopy(copied, item.Entry.StyleJson!, TextStyle.ParseKeys(ExcludeKeys), out var warning);
        if (built == null) { Message = "⚠ " + warning; return; }          // テキストではないアイテムの場合など

        await PasteModifiedAsync(built, item.Name, ownView, "書式を適用したテキスト");
    }

    /// <summary>エフェクトのまとまりを、選択中のアイテムに適用する</summary>
    async Task ApplyEffectBundleAsync(EffectItem item, object? ownView)
    {
        if (!await CopySelectedAsync(ownView)) return;

        var copied = ClipboardRaw.Capture(IntPtr.Zero, out var error);
        if (copied == null) { Message = error; return; }

        var built = EffectBundle.ApplyToCopy(copied, item.Entry.EffectsJson!, kind, AppendEffects, out var warning);
        if (built == null) { Message = "⚠ " + warning; return; }

        await PasteModifiedAsync(built, item.Name, ownView, $"{Title}を{(AppendEffects ? "追加" : "削除してから追加")}したアイテム");
    }

    /// <summary>反映したデータをクリップボードに書き込み、(必要なら元のアイテムを削除して)貼り付ける</summary>
    async Task PasteModifiedAsync(ApplyResult built, string name, object? ownView, string what)
    {
        // 書き換えられなかった独自形式が残っていると、YMM4がそちらを使って元のデータを貼り付けてしまうことがあるので外す
        var toWrite = built.Formats;
        var dropped = new List<ClipFormat>();
        if (settings.DropUnmodifiedFormats)
        {
            dropped = built.Unchanged.Where(f => f.Name != null).ToList();
            toWrite = built.Formats.Where(f => !dropped.Contains(f)).ToList();
        }
        var report = built.Report() + (dropped.Count > 0 ? $"(書き換えられなかった独自形式 {dropped.Count} 個は外しました)" : "");

        if (!ClipboardRaw.RestoreVerified(OwnerHandle(ownView), toWrite, out var error, out var dataId)) { Message = error; return; }

        if (!settings.PresetAutoPaste)
        {
            Message = $"{what}のデータをクリップボードにセットしました。タイムラインで {ApplyKeys} で貼り付けてください。{report}";
            return;
        }

        if (ReplaceOriginal)
        {
            // 選択中の元のアイテムを削除してから、反映したものを貼り付ける
            var del = await TimelinePaste.SendKeysAsync(ownView, "Delete");
            if (del != null) { Message = $"{del}元のアイテムを削除できなかったため、貼り付けも見送りました"; return; }
            await Task.Delay(200);
        }
        var skipped = await TimelinePaste.SendKeysAsync(ownView, ApplyKeys);
        Message = skipped == null
            ? $"「{name}」を反映した{what}を、YMM4へ貼り付けました(データID {dataId})。"
              + (ReplaceOriginal
                  ? "元のアイテムは置き換えています(YMM4で Ctrl+Z で戻せます)。シークバーの位置に貼り付けられます"
                  : "元のアイテムはそのまま残っています")
              + $" [{report}]"
            : $"{what}のデータをクリップボードにセットしました。{skipped}タイムラインをクリックして {ApplyKeys} で貼り付けてください";
    }

    static IntPtr OwnerHandle(object? ownView)
        => ownView is System.Windows.Media.Visual v
           && System.Windows.PresentationSource.FromVisual(v) is System.Windows.Interop.HwndSource h
            ? h.Handle
            : IntPtr.Zero;

    // ---------- 削除・並べ替え ----------
    void RemoveSelected()
    {
        if (Slot < 1 || selected.Count == 0) return;
        var ids = selected.Select(x => x.Entry.Id).ToList();
        store.Remove(Slot, ids);
        Message = $"{ids.Count} 件を削除しました";
    }

    public event Action<IReadOnlyList<EffectItem>>? Reordered;

    void Move(string how)
    {
        if (Slot < 1 || selected.Count == 0) return;
        var ids = selected.Select(x => x.Entry.Id).ToList();
        store.Move(Slot, ids, how);
        AfterReorder(ids);
    }

    string Tag => $"X{(int)kind}-{Slot}";

    public string? ReorderPayload()
        => Slot < 1 || selected.Count == 0 ? null : Tag + "\n" + string.Join("\n", selected.Select(x => x.Entry.Id));

    public bool TryReorder(string payload, int insertIndex)
    {
        var lines = payload.Split('\n');
        if (lines.Length < 2 || lines[0] != Tag) return false;
        var ids = lines.Skip(1).ToList();
        var known = Items.Select(x => x.Entry.Id).ToHashSet();
        if (!ids.All(known.Contains)) return false;
        store.MoveTo(Slot, ids, insertIndex);
        AfterReorder(ids);
        return true;
    }

    void AfterReorder(List<string> ids)
    {
        var set = new HashSet<string>(ids);
        Reordered?.Invoke(Items.Where(x => set.Contains(x.Entry.Id)).ToList());
    }

    // ---------- サムネイル ----------
    void SetThumbnail(EffectItem? item, BitmapSource? image, string okMessage)
    {
        if (item == null) { Message = "エフェクトを選んでください"; return; }
        Message = item.SetThumbnail(image) ? okMessage : "画像を変換できませんでした";
    }

    public void SetThumbnailFromClipboard()
    {
        try
        {
            if (!System.Windows.Clipboard.ContainsImage())
            {
                Message = "クリップボードに画像がありません。画像をコピーしてから押してください";
                return;
            }
            SetThumbnail(selectedItem, System.Windows.Clipboard.GetImage(), "クリップボードの画像をサムネイルにしました");
        }
        catch { Message = "クリップボードの画像を読み取れませんでした"; }
    }

    public void SetThumbnailFromFile(EffectItem? item, string path)
    {
        var img = PresetThumb.LoadFile(path);
        if (img == null) { Message = "画像を読み込めませんでした"; return; }
        SetThumbnail(item, img, "画像をサムネイルにしました");
    }
}

/// <summary>保存する書式の項目1つ分(項目の名前と、値のようす、保存するかどうか)</summary>
public sealed class StyleKeyOption : NotifyBase
{
    public string Key { get; }
    public string Preview { get; }

    bool isChecked;
    public bool IsChecked { get => isChecked; set => Set(ref isChecked, value); }

    /// <summary>画面に出す名前(日本語の項目名など)。無ければ項目名</summary>
    public string Display { get; }

    public StyleKeyOption(string key, string preview, bool isChecked, string? display = null)
    {
        Key = key;
        Preview = preview;
        this.isChecked = isChecked;
        Display = string.IsNullOrEmpty(display) ? key : display;
    }
}

/// <summary>「対象パラメータ」のスイッチ1つ分(描画位置・フォント・文字色など)</summary>
public sealed class StyleGroupOption : NotifyBase
{
    public string Id { get; }
    public string Label { get; }

    /// <summary>選択中のアイテムに、この項目があるか(無ければ、スイッチは押せない)</summary>
    public bool IsAvailable { get; }

    /// <summary>項目の中身(マウスを乗せると表示)</summary>
    public string Tooltip { get; }

    bool isChecked;
    public bool IsChecked { get => isChecked; set => Set(ref isChecked, value); }

    public StyleGroupOption(string id, string label, bool isAvailable, bool isChecked, string tooltip)
    {
        Id = id;
        Label = label;
        IsAvailable = isAvailable;
        this.isChecked = isChecked;
        Tooltip = tooltip;
    }
}
