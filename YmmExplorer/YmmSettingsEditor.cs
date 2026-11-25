using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;

namespace YmmExplorer;

public enum SettingKind { Bool, Number, Text, Enum }

/// <summary>選択肢1つ分(Value: 内部の名前 / Label: 画面に出す名前)</summary>
public sealed class EnumOption
{
    public string Value { get; }
    public string Label { get; }
    public EnumOption(string value, string label) { Value = value; Label = label; }
}

/// <summary>
/// 項目名・説明を日本語にする。
/// 1) YMM4が持っているリソース(日本語)を、直接読む(YMM4の画面と同じ名前になる)
/// 2) 読めなかった項目は、英語の単語を日本語の辞書で訳す
/// </summary>
public static class JaLabels
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    // 先頭(Is・Has など)や、つなぎの言葉は、訳さずに省く
    static readonly HashSet<string> Skip = new(StringComparer.OrdinalIgnoreCase)
    {
        "Is", "Has", "Should", "Can", "Of", "To", "For", "With", "The", "In", "On", "At", "And", "Or", "Enabled", "Enable",
    };

    static readonly Dictionary<string, string> Words = BuildWords();

    static Dictionary<string, string> BuildWords()
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        void A(string en, string ja) { d[en] = ja; }
        A("FFmpeg", "FFmpeg"); A("Ffmpeg", "FFmpeg"); A("Gpu", "GPU"); A("Cpu", "CPU"); A("Fps", "FPS"); A("Hz", "Hz"); A("Ui", "UI");
        A("Auto", "自動"); A("Save", "保存"); A("Saving", "保存"); A("Load", "読み込み"); A("Interval", "間隔"); A("Backup", "バックアップ");
        A("Timeline", "タイムライン"); A("Preview", "プレビュー"); A("Player", "プレイヤー"); A("Play", "再生"); A("Playback", "再生");
        A("Pause", "一時停止"); A("Stop", "停止"); A("Seek", "シーク"); A("Frame", "フレーム"); A("Frames", "フレーム"); A("Rate", "レート");
        A("Width", "幅"); A("Height", "高さ"); A("Size", "サイズ"); A("Font", "フォント"); A("Color", "色"); A("Colour", "色");
        A("Show", "表示"); A("Display", "表示"); A("Visible", "表示"); A("Visibility", "表示"); A("Hide", "非表示"); A("Hidden", "非表示");
        A("Disable", "無効"); A("Disabled", "無効"); A("Use", "使用"); A("Max", "最大"); A("Maximum", "最大"); A("Min", "最小");
        A("Minimum", "最小"); A("Default", "既定"); A("Item", "アイテム"); A("Items", "アイテム"); A("Layer", "レイヤー");
        A("Layers", "レイヤー"); A("Voice", "ボイス"); A("Audio", "音声"); A("Sound", "サウンド"); A("Video", "映像");
        A("Image", "画像"); A("Picture", "画像"); A("Text", "テキスト"); A("Volume", "音量"); A("Speed", "速度"); A("Time", "時間");
        A("Duration", "長さ"); A("Length", "長さ"); A("Start", "開始"); A("End", "終了"); A("Position", "位置"); A("Zoom", "拡大率");
        A("Scale", "拡大縮小"); A("Opacity", "不透明度"); A("Rotation", "回転"); A("Rotate", "回転"); A("Blend", "合成");
        A("Effect", "エフェクト"); A("Effects", "エフェクト"); A("Thread", "スレッド"); A("Threads", "スレッド"); A("Cache", "キャッシュ");
        A("Memory", "メモリ"); A("Hardware", "ハードウェア"); A("Encode", "エンコード"); A("Encoder", "エンコーダー");
        A("Decode", "デコード"); A("Decoder", "デコーダー"); A("Quality", "品質"); A("Bitrate", "ビットレート"); A("Codec", "コーデック");
        A("Format", "形式"); A("Path", "パス"); A("Folder", "フォルダー"); A("Directory", "ディレクトリ"); A("File", "ファイル");
        A("Files", "ファイル"); A("Name", "名前"); A("Language", "言語"); A("Theme", "テーマ"); A("Window", "ウィンドウ");
        A("Dialog", "ダイアログ"); A("Confirm", "確認"); A("Confirmation", "確認"); A("Warning", "警告"); A("Update", "更新");
        A("Updates", "更新"); A("Check", "チェック"); A("Open", "開く"); A("Close", "閉じる"); A("New", "新規"); A("Last", "前回");
        A("Recent", "最近"); A("History", "履歴"); A("Undo", "元に戻す"); A("Redo", "やり直し"); A("Snap", "スナップ");
        A("Grid", "グリッド"); A("Scroll", "スクロール"); A("Mouse", "マウス"); A("Key", "キー"); A("Keyboard", "キーボード");
        A("Shortcut", "ショートカット"); A("Select", "選択"); A("Selected", "選択中"); A("Selection", "選択"); A("Copy", "コピー");
        A("Paste", "貼り付け"); A("Delete", "削除"); A("Remove", "削除"); A("Add", "追加"); A("Insert", "挿入"); A("Split", "分割");
        A("Merge", "結合"); A("Group", "グループ"); A("Lock", "ロック"); A("Mute", "ミュート"); A("Pan", "パン");
        A("Sample", "サンプル"); A("Channel", "チャンネル"); A("Channels", "チャンネル"); A("Subtitle", "字幕"); A("Jimaku", "字幕");
        A("Character", "キャラクター"); A("Tachie", "立ち絵"); A("Animation", "アニメーション"); A("Scene", "シーン");
        A("Project", "プロジェクト"); A("Export", "出力"); A("Output", "出力"); A("Render", "描画"); A("Rendering", "描画");
        A("Background", "背景"); A("Border", "縁取り"); A("Shadow", "影"); A("Line", "行"); A("Letter", "文字"); A("Space", "間隔");
        A("Spacing", "間隔"); A("Ratio", "比率"); A("Offset", "オフセット"); A("Margin", "余白"); A("Padding", "余白");
        A("Alignment", "揃え"); A("Direction", "方向"); A("Horizontal", "水平"); A("Vertical", "垂直"); A("Left", "左");
        A("Right", "右"); A("Top", "上"); A("Bottom", "下"); A("Center", "中央"); A("Always", "常に"); A("Only", "のみ");
        A("Allow", "許可"); A("Ignore", "無視"); A("Skip", "スキップ"); A("Limit", "上限"); A("Count", "数"); A("Number", "数");
        A("Value", "値"); A("Type", "種類"); A("Mode", "モード"); A("Option", "オプション"); A("Options", "オプション");
        A("Setting", "設定"); A("Settings", "設定"); A("General", "全般"); A("Common", "共通"); A("Advanced", "詳細");
        A("Screen", "画面"); A("Monitor", "モニター"); A("Device", "デバイス"); A("Network", "通信");
        A("Proxy", "プロキシ"); A("Log", "ログ"); A("Debug", "デバッグ"); A("Error", "エラー"); A("Message", "メッセージ");
        A("Notification", "通知"); A("Sort", "並べ替え"); A("Order", "順序"); A("Search", "検索"); A("Filter", "フィルター");
        A("Template", "テンプレート"); A("Preset", "プリセット"); A("Plugin", "プラグイン"); A("Plugins", "プラグイン");
        A("Style", "スタイル"); A("Bold", "太字"); A("Italic", "斜体"); A("Underline", "下線"); A("Shape", "図形");
        A("Fade", "フェード"); A("Transition", "切り替え"); A("Fill", "塗りつぶし");
        A("Stroke", "線"); A("Thumbnail", "サムネイル"); A("Waveform", "波形"); A("Wave", "波形"); A("Level", "レベル");
        A("Meter", "メーター"); A("Normalize", "正規化"); A("Edit", "編集"); A("Editor", "エディター"); A("Edited", "編集");
        A("Current", "現在"); A("First", "最初"); A("Next", "次"); A("Previous", "前"); A("Repeat", "繰り返し"); A("Loop", "ループ");
        A("Realtime", "リアルタイム"); A("Precision", "精度"); A("Smooth", "滑らか"); A("Smoothing", "平滑化");
        A("Antialias", "アンチエイリアス"); A("Resolution", "解像度"); A("Pixel", "ピクセル"); A("Pixels", "ピクセル");
        A("Second", "秒"); A("Seconds", "秒"); A("Millisecond", "ミリ秒"); A("Milliseconds", "ミリ秒"); A("Minute", "分");
        A("Minutes", "分"); A("Percent", "パーセント"); A("Step", "刻み"); A("Unit", "単位"); A("Base", "基準"); A("Point", "点");
        A("Anchor", "基準点"); A("Clip", "クリップ"); A("Clipping", "クリッピング"); A("Mask", "マスク");
        A("Keyframe", "キーフレーム"); A("Keyframes", "キーフレーム"); A("Bezier", "ベジェ"); A("Curve", "曲線");
        A("Tab", "タブ"); A("Tabs", "タブ"); A("Panel", "パネル"); A("Menu", "メニュー"); A("Button", "ボタン"); A("Toolbar", "ツールバー");
        A("Startup", "起動"); A("Launch", "起動"); A("Exit", "終了"); A("Restart", "再起動"); A("Restore", "復元"); A("Reset", "リセット");
        A("Import", "取り込み"); A("Download", "ダウンロード"); A("Upload", "アップロード"); A("Share", "共有"); A("Online", "オンライン");
        A("Version", "バージョン"); A("Info", "情報"); A("Help", "ヘルプ"); A("Tip", "ヒント"); A("Tips", "ヒント");
        return d;
    }

    public static bool HasJapanese(string? s)
        => !string.IsNullOrEmpty(s) && s.Any(c => (c >= '\u3040' && c <= '\u30FF') || (c >= '\u4E00' && c <= '\u9FFF'));

    static readonly Regex WordRx = new(@"[A-Z]+(?![a-z])|[A-Z]?[a-z]+|\d+", RegexOptions.Compiled);

    /// <summary>「AutoSaveInterval」のような名前を、単語ごとに日本語にして並べる(例: 自動保存間隔)</summary>
    public static string FromPascal(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (Words.TryGetValue(name, out var whole)) return whole;

        var sb = new StringBuilder();
        var lastAscii = false;
        foreach (Match m in WordRx.Matches(name))
        {
            var w = m.Value;
            if (Skip.Contains(w)) continue;
            var text = Words.TryGetValue(w, out var ja) ? ja : w;
            var ascii = !HasJapanese(text);
            if (sb.Length > 0 && ascii && lastAscii) sb.Append(' ');
            sb.Append(text);
            lastAscii = ascii;
        }
        return sb.Length == 0 ? name : sb.ToString();
    }

    /// <summary>
    /// リソースの型(Texts など)から、キーに対応する文字列を取る。日本語を優先し、
    /// 型が公開されていなくても、リフレクションで読む
    /// </summary>
    public static string Localize(Type? resourceType, string key)
    {
        if (resourceType == null) return key;
        try
        {
            if (resourceType.GetProperty("ResourceManager", All)?.GetValue(null) is ResourceManager rm)
            {
                foreach (var culture in new[] { new CultureInfo("ja-JP"), new CultureInfo("ja"), CultureInfo.InvariantCulture, CultureInfo.CurrentUICulture })
                {
                    string? v = null;
                    try { v = rm.GetString(key, culture); }
                    catch { }
                    if (!string.IsNullOrEmpty(v)) return v;
                }
            }
            if (resourceType.GetProperty(key, All)?.GetValue(null) is string s && s.Length > 0) return s;
        }
        catch { }
        return key;
    }

    public sealed record DisplayInfo(string? Name, string? Description, string? GroupName);

    /// <summary>[Display(...)] の名前・説明・グループ名を、日本語で取る(リソースを使っていれば、リソースから)</summary>
    public static DisplayInfo ReadDisplay(MemberInfo member)
    {
        try
        {
            foreach (var a in member.GetCustomAttributes(false))
            {
                var at = a.GetType();
                if (at.Name != "DisplayAttribute") continue;
                var rt = at.GetProperty("ResourceType")?.GetValue(a) as Type;
                string? Read(string prop)
                {
                    var raw = at.GetProperty(prop)?.GetValue(a) as string;
                    return string.IsNullOrWhiteSpace(raw) ? null : Localize(rt, raw);
                }
                return new DisplayInfo(Read("Name"), Read("Description"), Read("GroupName"));
            }
        }
        catch { }
        return new DisplayInfo(null, null, null);
    }

    /// <summary>取れた名前を整える。リソースのキー(英数字だけ)のままなら、辞書で訳す</summary>
    public static string Polish(string? text, string fallbackName)
    {
        if (string.IsNullOrWhiteSpace(text)) return FromPascal(fallbackName);
        if (HasJapanese(text)) return text;
        return Regex.IsMatch(text, @"^[A-Za-z0-9_]+$") ? FromPascal(text) : text;
    }
}

/// <summary>YMM4の設定1項目(スイッチ・数・文字・選択肢のどれか)</summary>
public sealed class YmmSettingEntry : NotifyBase
{
    readonly object instance;
    readonly PropertyInfo prop;
    readonly Action<object> markDirty;
    /// <summary>保存(Save)を呼ぶ相手。入れ子の設定(全般の中のプレビューなど)は、外側の設定</summary>
    readonly object saveTarget;

    public string GroupTitle { get; }
    public string SubGroupTitle { get; }
    public string Title { get; }
    public string Description { get; }
    public string PropertyName => prop.Name;
    public SettingKind Kind { get; }
    public EnumOption[] Options { get; } = Array.Empty<EnumOption>();
    public object? Original { get; }

    /// <summary>日本語の名前を取れたか(取れていない項目は、英語の名前を訳したもの)</summary>
    public bool HasJapaneseName { get; }

    public bool IsBool => Kind == SettingKind.Bool;
    public bool IsNotBool => Kind != SettingKind.Bool;
    public bool IsNumberOrText => Kind == SettingKind.Number || Kind == SettingKind.Text;
    public bool IsEnum => Kind == SettingKind.Enum;
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    public ICommand RevertCommand { get; }

    bool isVisible = true;
    public bool IsVisible { get => isVisible; set => Set(ref isVisible, value); }

    string error = "";
    public string Error { get => error; private set { if (Set(ref error, value)) Raise(nameof(HasError)); } }
    public bool HasError => error.Length > 0;

    public YmmSettingEntry(object instance, PropertyInfo prop, string group, string subGroup, string title, string description,
                           bool hasJapaneseName, SettingKind kind, EnumOption[] options, Action<object> markDirty, object? saveTarget = null)
    {
        this.instance = instance;
        this.saveTarget = saveTarget ?? instance;
        this.prop = prop;
        this.markDirty = markDirty;
        GroupTitle = group;
        SubGroupTitle = subGroup;
        Title = title;
        Description = description;
        HasJapaneseName = hasJapaneseName;
        Kind = kind;
        Options = options;
        Original = Read();
        RevertCommand = new RelayCommand(_ => Revert());
    }

    object? Read()
    {
        try { return prop.GetValue(instance); }
        catch { return null; }
    }

    /// <summary>変更されているか(開いた時の値と違うか)</summary>
    public bool IsChanged => !Equals(Original, Read());

    public bool BoolValue
    {
        get => Read() is bool b && b;
        set => Apply(value);
    }

    public string TextValue
    {
        get => Convert.ToString(Read(), CultureInfo.InvariantCulture) ?? "";
        set
        {
            try
            {
                if (Kind == SettingKind.Number)
                {
                    var real = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    Apply(Convert.ChangeType(value.Trim(), real, CultureInfo.InvariantCulture));
                }
                else Apply(value);
            }
            catch
            {
                Error = "数として読み取れませんでした";
                Raise(nameof(TextValue));
            }
        }
    }

    public string? EnumValue
    {
        get => Read()?.ToString();
        set
        {
            if (value == null) return;
            try { Apply(Enum.Parse(Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType, value)); }
            catch { Error = "この値は設定できません"; }
        }
    }

    void Apply(object? value)
    {
        try
        {
            prop.SetValue(instance, value);
            Error = "";
            markDirty(saveTarget);
        }
        catch (Exception ex)
        {
            Error = ex.InnerException?.Message ?? ex.Message;
        }
        RaiseValues();
    }

    void RaiseValues()
    {
        Raise(nameof(BoolValue));
        Raise(nameof(TextValue));
        Raise(nameof(EnumValue));
        Raise(nameof(IsChanged));
    }

    /// <summary>開いた時の値に戻す</summary>
    public void Revert()
    {
        if (!IsChanged) return;
        Apply(Original);
    }
}

/// <summary>グループの中の小見出し(YMM4の「グループ名」ごとのまとまり)</summary>
public sealed class YmmSettingSubGroup : NotifyBase
{
    public string Title { get; }
    public ObservableCollection<YmmSettingEntry> Entries { get; } = new();
    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    public YmmSettingSubGroup(string title) { Title = title; }

    public bool IsVisible => Entries.Any(e => e.IsVisible);
    public void RefreshVisible() => Raise(nameof(IsVisible));
}

/// <summary>設定の大きなグループ(アイテム・タイムライン・再生…)</summary>
public sealed class YmmSettingGroup : NotifyBase
{
    public string Title { get; }
    public ObservableCollection<YmmSettingSubGroup> SubGroups { get; } = new();

    public YmmSettingGroup(string title) { Title = title; }

    public IEnumerable<YmmSettingEntry> Entries => SubGroups.SelectMany(s => s.Entries);
    public int TotalCount => Entries.Count();
    public int VisibleCount => Entries.Count(e => e.IsVisible);
    public string CountText => VisibleCount == TotalCount ? TotalCount.ToString() : $"{VisibleCount}/{TotalCount}";

    bool isVisible = true;
    public bool IsVisible { get => isVisible; set => Set(ref isVisible, value); }

    public void RefreshCounts()
    {
        foreach (var s in SubGroups) s.RefreshVisible();
        Raise(nameof(VisibleCount));
        Raise(nameof(CountText));
    }
}

/// <summary>YMM4本体の設定クラス(YukkuriMovieMaker.Settings.*)を探して、編集できる項目の一覧にする</summary>
public static class YmmSettingsCatalog
{
    static readonly Dictionary<string, string> GroupNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Item"] = "アイテム", ["Timeline"] = "タイムライン", ["Player"] = "再生・プレビュー", ["Preview"] = "プレビュー",
        ["Export"] = "動画の出力", ["Ffmpeg"] = "FFmpeg", ["Font"] = "フォント", ["Voice"] = "音声合成", ["Audio"] = "音声",
        ["Video"] = "映像", ["Project"] = "プロジェクト", ["Backup"] = "バックアップ", ["Window"] = "ウィンドウ",
        ["Shortcut"] = "ショートカット", ["General"] = "全般", ["Update"] = "更新", ["Tachie"] = "立ち絵",
        ["Character"] = "キャラクター", ["Theme"] = "テーマ", ["Hardware"] = "ハードウェア", ["Gpu"] = "GPU",
        ["Render"] = "レンダリング", ["Plugin"] = "プラグイン", ["Language"] = "言語", ["Edit"] = "編集",
        ["Template"] = "テンプレート", ["Keyboard"] = "キーボード", ["Network"] = "通信", ["Cache"] = "キャッシュ",
        ["Common"] = "共通", ["Scene"] = "シーン", ["Subtitle"] = "字幕", ["Text"] = "テキスト", ["Application"] = "アプリ全般",
        ["App"] = "アプリ全般", ["Main"] = "メイン画面", ["Layout"] = "画面レイアウト", ["Sound"] = "サウンド",
    };

    static readonly HashSet<string> SkipNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Version", "Id", "IsLoaded", "FilePath", "Path",
    };

    static IEnumerable<Type> TypesOf(Assembly asm)
    {
        try { return asm.GetTypes(); }
        catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null).Select(t => t!); }
        catch { return Array.Empty<Type>(); }
    }

    static bool IsBrowsable(PropertyInfo p)
    {
        try
        {
            foreach (var a in p.GetCustomAttributes(false))
                if (a.GetType().Name == "BrowsableAttribute" && a.GetType().GetProperty("Browsable")?.GetValue(a) is false) return false;
        }
        catch { }
        return true;
    }

    static EnumOption[] OptionsOf(Type enumType)
    {
        var list = new List<EnumOption>();
        foreach (var name in Enum.GetNames(enumType))
        {
            var field = enumType.GetField(name);
            var info = field == null ? new JaLabels.DisplayInfo(null, null, null) : JaLabels.ReadDisplay(field);
            list.Add(new EnumOption(name, JaLabels.Polish(info.Name, name)));
        }
        return list.ToArray();
    }

    /// <summary>設定クラスを探して、グループごとの一覧にする(YMM4本体の設定だけ。プラグインの設定は含めない)</summary>
    /// <summary>設定クラスの、ただ1つの実体(Default / Instance / Current)</summary>
    static object? SingletonOf(Type t)
    {
        foreach (var n in new[] { "Default", "Instance", "Current" })
        {
            try
            {
                var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                if (p == null || !t.IsAssignableFrom(p.PropertyType) && !p.PropertyType.IsAssignableFrom(t)) continue;
                var v = p.GetValue(null);
                if (v != null && t.IsInstanceOfType(v)) return v;
            }
            catch { }
        }
        return null;
    }

    /// <summary>中に設定の項目を持つ入れ物か(別に Default を持つ設定クラスは、そちらで並べるので除く)</summary>
    static bool IsNestedSettings(PropertyInfo p, object owner, out object? value)
    {
        value = null;
        try
        {
            var pt = p.PropertyType;
            if (pt.IsValueType || pt == typeof(string) || typeof(System.Collections.IEnumerable).IsAssignableFrom(pt)) return false;
            if (pt.IsInterface && !pt.Namespace!.StartsWith("YukkuriMovieMaker", StringComparison.Ordinal)) return false;
            value = p.GetValue(owner);
            if (value == null || ReferenceEquals(value, owner) || value is System.Collections.IEnumerable) return false;
            var vt = value.GetType();
            if (!(vt.Namespace ?? "").StartsWith("YukkuriMovieMaker", StringComparison.Ordinal)) return false;
            if (SingletonOf(vt) != null) return false;
            return vt.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(x =>
                x.GetIndexParameters().Length == 0 && x.GetSetMethod(false) != null &&
                ((Nullable.GetUnderlyingType(x.PropertyType) ?? x.PropertyType) is var rt && (rt == typeof(bool) || rt.IsEnum || rt == typeof(double) || rt == typeof(int))));
        }
        catch { return false; }
    }

    public static List<YmmSettingGroup> Load(Action<object> markDirty, out string message)
    {
        var groups = new List<YmmSettingGroup>();
        var seenTypes = new HashSet<Type>();
        int classes = 0, items = 0, japanese = 0;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            string? name;
            try { name = asm.GetName().Name; }
            catch { continue; }
            if (name == null || !name.StartsWith("YukkuriMovieMaker", StringComparison.Ordinal)) continue;
            string location;
            try { location = asm.Location ?? ""; }
            catch { location = ""; }
            if (location.Contains(@"\user\plugin\", StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var t in TypesOf(asm))
            {
                // 設定のクラス: YukkuriMovieMaker.Settings の中のもの、または名前が「…Settings」で Default(ただ1つの実体)を持つもの
                if (!t.IsClass || t.IsAbstract || t.IsGenericTypeDefinition || t.Namespace == null) continue;
                bool inSettingsNs = t.Namespace.StartsWith("YukkuriMovieMaker.Settings", StringComparison.Ordinal);
                if (!inSettingsNs && !t.Name.EndsWith("Settings", StringComparison.Ordinal)) continue;
                if (!seenTypes.Add(t)) continue;

                object? instance = SingletonOf(t);
                if (instance == null) continue;

                var baseName = t.Name.EndsWith("Settings", StringComparison.Ordinal) ? t.Name.Substring(0, t.Name.Length - "Settings".Length) : t.Name;
                var classInfo = JaLabels.ReadDisplay(t);
                var title = JaLabels.HasJapanese(classInfo.Name) ? classInfo.Name!
                          : GroupNames.TryGetValue(baseName, out var jp) ? jp : JaLabels.FromPascal(baseName);
                var group = new YmmSettingGroup(title);
                var subs = new Dictionary<string, YmmSettingSubGroup>();
                AddProps(instance, instance, title, "", 0);
                if (group.TotalCount == 0) continue;
                groups.Add(group);
                classes++;

                // 設定の項目を並べる。中に設定のまとまり(全般の「プレビュー」など)を持っていれば、その中も見出しをつけて並べる
                void AddProps(object owner, object root, string groupTitle, string prefix, int depth)
                {
                foreach (var p in owner.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.GetIndexParameters().Length != 0 || !p.CanRead) continue;
                    if (SkipNames.Contains(p.Name) || !IsBrowsable(p)) continue;
                    if (p.GetSetMethod(false) == null)
                    {
                        // 読み取り専用の入れ物(設定のまとまり)
                        if (depth < 3 && IsNestedSettings(p, owner, out var nested))
                        {
                            var ni = JaLabels.ReadDisplay(p);
                            var nl = JaLabels.Polish(ni.Name, p.Name);
                            AddProps(nested!, root, groupTitle, prefix.Length > 0 ? prefix + " / " + nl : nl, depth + 1);
                        }
                        continue;
                    }

                    var real = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                    SettingKind kind;
                    var options = Array.Empty<EnumOption>();
                    if (real == typeof(bool)) kind = SettingKind.Bool;
                    else if (real.IsEnum) { kind = SettingKind.Enum; options = OptionsOf(real); }
                    else if (real == typeof(string)) kind = SettingKind.Text;
                    else if (real == typeof(int) || real == typeof(long) || real == typeof(short) || real == typeof(double)
                             || real == typeof(float) || real == typeof(decimal) || real == typeof(byte)) kind = SettingKind.Number;
                    else
                    {
                        // 設定できる入れ物(設定のまとまり)
                        if (depth < 3 && IsNestedSettings(p, owner, out var nested2))
                        {
                            var ni = JaLabels.ReadDisplay(p);
                            var nl = JaLabels.Polish(ni.Name, p.Name);
                            AddProps(nested2!, root, groupTitle, prefix.Length > 0 ? prefix + " / " + nl : nl, depth + 1);
                        }
                        continue;
                    }

                    var info = JaLabels.ReadDisplay(p);
                    var label = JaLabels.Polish(info.Name, p.Name);
                    var desc = info.Description == null ? "" : JaLabels.Polish(info.Description, "");
                    if (desc.Length > 0 && !JaLabels.HasJapanese(desc) && Regex.IsMatch(desc, @"^[A-Za-z0-9_]+$")) desc = "";
                    var sub = string.IsNullOrWhiteSpace(info.GroupName) ? "" : JaLabels.Polish(info.GroupName, "");
                    if (prefix.Length > 0) sub = sub.Length > 0 ? prefix + " / " + sub : prefix;
                    var hasJa = JaLabels.HasJapanese(label);

                    if (!subs.TryGetValue(sub, out var sg))
                    {
                        sg = new YmmSettingSubGroup(sub);
                        subs[sub] = sg;
                        group.SubGroups.Add(sg);
                    }
                    sg.Entries.Add(new YmmSettingEntry(owner, p, groupTitle, sub, label, desc, hasJa, kind, options, markDirty, root));
                    items++;
                    if (hasJa) japanese++;
                }
                }
            }
        }
        groups = groups.OrderBy(g => g.Title, StringComparer.CurrentCultureIgnoreCase).ToList();
        message = classes == 0
            ? "YMM4の設定を見つけられませんでした(YMM4のバージョンによっては、読み取れない場合があります)"
            : $"{classes} 個のグループ、{items} 項目を読み込みました(日本語の名前: {japanese} / {items} 項目)";
        return groups;
    }
}

/// <summary>変更した設定を、少し待ってからまとめて保存する(YMM4自身の Save() を呼ぶ)</summary>
public sealed class YmmSettingsSaver
{
    readonly HashSet<object> dirty = new();
    readonly DispatcherTimer timer;

    public YmmSettingsSaver()
    {
        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
        timer.Tick += (_, _) => Flush();
    }

    public void MarkDirty(object instance)
    {
        dirty.Add(instance);
        timer.Stop();
        timer.Start();
    }

    public void Flush()
    {
        timer.Stop();
        foreach (var inst in dirty.ToList())
        {
            try { inst.GetType().GetMethod("Save", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)?.Invoke(inst, null); }
            catch { }
        }
        dirty.Clear();
    }
}
