using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace YmmExplorer;

/// <summary>項目の種類(見せ方)</summary>
public enum ItemRowKind { Number, Anim, Bool, Enum, Font, Text, Multiline, Color }

/// <summary>
/// アイテムタブの1行(=YMM4のアイテムやエフェクトの、1つの項目)。
/// 選択中のアイテムが複数ある時は、同じ項目を持つすべてのアイテムへ、同じ値を書き込む。
/// </summary>
public sealed class ItemPropRow : NotifyBase
{
    static readonly PaletteSwatch[] quickColors = new[]
    {
        "FFFFFF", "000000", "FF3B30", "FF9500", "FFCC00", "34C759", "32ADE6", "007AFF", "AF52DE", "FF2D92",
    }.Select(h => new PaletteSwatch(h)).ToArray();

    static List<string>? fontNames;

    readonly ItemEditorVm host;
    readonly string propName;
    readonly List<(object root, object owner)> targets;
    readonly string format;
    readonly Array? enumValues;
    readonly Array? animTypes;
    /// <summary>文字で持っている選択肢(図形の種類など)。Choices と同じ並びの、実際に書き込む値</summary>
    readonly List<string>? choiceKeys;
    /// <summary>変えると、ほかの項目の並びも変わる項目(変えた後に、一覧を作り直す)</summary>
    readonly bool rebuildAfter;
    /// <summary>アイテム本体から、この項目を持つ入れ物(本体・図形の設定・エフェクトなど)を取り出す方法</summary>
    public Func<object, object?>? Resolver { get; init; }
    /// <summary>選択肢の「いまの値」を独自に読む・書く(図形の種類など、型が string ではない項目用)</summary>
    public Func<object, string?>? ChoiceGet { get; init; }
    public Action<object, string>? ChoiceSet { get; init; }

    ICommand? resetDefaultCommand, setDefaultCommand, toggleQuickCommand;
    /// <summary>「よく使う項目」に入れる・外す</summary>
    public ICommand ToggleQuickCommand => toggleQuickCommand ??= new RelayCommand(_ => host.ToggleQuick(this));
    public string QuickMenuText => host.IsQuick(this) ? "「よく使う項目」から外す" : "「よく使う項目」に追加";
    public ICommand ResetDefaultCommand => resetDefaultCommand ??= new RelayCommand(_ => ResetToDefault());
    public ICommand SetDefaultCommand => setDefaultCommand ??= new RelayCommand(_ => SetAsDefault());

    /// <summary>値を写す。数値や文字はそのまま、移動(Animation)などの入れ物は、JSONを通して中身ごと写す</summary>
    bool CopyValue(object from, object to)
    {
        var pf = Ymm.FindProperty(from.GetType(), propName);
        var pt = Ymm.FindProperty(to.GetType(), propName);
        if (pf == null || pt == null) { host.Report($"「{Label}」は、相手側にありません"); return false; }
        var value = pf.GetValue(from);
        var t = Nullable.GetUnderlyingType(pt.PropertyType) ?? pt.PropertyType;
        if (value != null && !(t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(Color)))
        {
            var json = Ymm.ValueToJson(value);
            if (json == null) { host.Report($"「{Label}」を写せませんでした: {Ymm.LastError}"); return false; }
            value = Ymm.JsonToValue(pt.PropertyType, json);
            if (value == null) { host.Report($"「{Label}」を写せませんでした: {Ymm.LastError}"); return false; }
        }
        if (Ymm.SetValue(to, propName, value, out var error)) return true;
        host.Report($"「{Label}」を写せませんでした: {error}");
        return false;
    }

    /// <summary>YMM4の「デフォルト(既定値)」の値に戻す</summary>
    void ResetToDefault()
    {
        if (targets.Count == 0 || Resolver == null) return;
        var def = Ymm.DefaultItemFor(targets[0].root.GetType());
        if (def == null) { host.Report(Ymm.LastError); return; }
        var from = Resolver(def);
        if (from == null || Ymm.FindProperty(from.GetType(), propName) == null) { host.Report($"「{Label}」には、デフォルトの値がありません(エフェクトの中の項目などは、対象外です)"); return; }
        bool ok = true;
        Commit(owner => ok &= CopyValue(from, owner));
        if (ok) host.Say($"「{Label}」を、デフォルトの値に戻しました");
    }

    /// <summary>いまの値を、YMM4の「デフォルト(既定値)」にする(これから新しく作るアイテムに使われる)</summary>
    void SetAsDefault()
    {
        if (targets.Count == 0 || Resolver == null) return;
        var (root, owner) = targets[0];
        var def = Ymm.DefaultItemFor(root.GetType());
        if (def == null) { host.Report(Ymm.LastError); return; }
        var to = Resolver(def);
        if (to == null || Ymm.FindProperty(to.GetType(), propName) == null) { host.Report($"「{Label}」は、デフォルトに設定できません(エフェクトの中の項目などは、対象外です)"); return; }
        if (!CopyValue(owner, to)) return;
        bool saved = Ymm.SaveItemSettings();
        host.Say($"「{Label}」のいまの値を、デフォルトに設定しました" + (saved ? "" : "(YMM4の終了時に保存されます)"));
    }
    bool isVisible = true;
    bool animOpen;

    public ItemRowKind Kind { get; }
    public string PropName => propName;
    /// <summary>図形の種類の項目(分類つきの専用の選択画面を出す)</summary>
    public bool IsShapeRow { get; init; }
    ShapePickerVm? shapePicker;
    public ShapePickerVm? ShapePicker => IsShapeRow ? (shapePicker ??= new ShapePickerVm(this)) : null;
    /// <summary>選択肢の、書き込む値の一覧(図形の種類の選択画面用)</summary>
    public IReadOnlyList<string> ChoiceKeys => (IReadOnlyList<string>?)choiceKeys ?? Array.Empty<string>();
    /// <summary>いま選ばれている選択肢の表示名</summary>
    public string CurrentChoiceText { get { var i = ChoiceIndex; return i >= 0 && i < Choices.Count ? Choices[i] : "(いまの図形が分かりません)"; } }
    /// <summary>いま選ばれている選択肢の、書き込む値</summary>
    public string? CurrentChoiceKey { get { var i = ChoiceIndex; return choiceKeys != null && i >= 0 && i < choiceKeys.Count ? choiceKeys[i] : null; } }
    /// <summary>いまの図形が、同梱の演出図形か(2D/3Dを切り替えられるか)</summary>
    public bool IsEffectShape => IsShapeRow && CurrentChoiceKey is { } k && k.StartsWith("YMM4EffectShapes.", StringComparison.Ordinal)
        && choiceKeys != null && choiceKeys.Contains(YMM4EffectShapes.Solid3D.Solid3DDefaults.Key3D(YMM4EffectShapes.Solid3D.Solid3DDefaults.Key2D(k)));
    /// <summary>いまの図形が「演出3D」か</summary>
    public bool IsShape3D
    {
        get => CurrentChoiceKey is { } k && YMM4EffectShapes.Solid3D.Solid3DDefaults.Is3DKey(k);
        set { if (value != IsShape3D) Switch3D(value); RaiseShape3DLater(); }
    }
    public bool IsShape2D { get => !IsShape3D; set { if (value) IsShape3D = false; RaiseShape3DLater(); } }
    // ボタンを押して「オフ」になった見た目を、すぐ正しい状態に戻す
    void RaiseShape3DLater() => System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(RaiseShape3D));

    /// <summary>
    /// 同じ図形の2D版と3D版を切り替える。図形の設定(色・数など)は引き継ぎ、3Dの設定は3D版の初期値にする
    /// </summary>
    void Switch3D(bool to3D)
    {
        var cur = CurrentChoiceKey;
        if (cur == null || choiceKeys == null) return;
        var key2D = YMM4EffectShapes.Solid3D.Solid3DDefaults.Key2D(cur);
        var target = to3D ? YMM4EffectShapes.Solid3D.Solid3DDefaults.Key3D(key2D) : key2D;
        if (target == cur || !choiceKeys.Contains(target)) return;
        var olds = targets.Select(t => Ymm.Get(t.owner, "ShapeParameter")).ToList();
        PickChoiceKey(target);
        int copied = 0;
        for (int i = 0; i < targets.Count && i < olds.Count; i++)
        {
            if (olds[i] is YMM4EffectShapes.EffectShapeParameterBase from && Ymm.Get(targets[i].owner, "ShapeParameter") is YMM4EffectShapes.EffectShapeParameterBase to && from.GetType() == to.GetType())
            {
                to.CopySettingsFrom(from, to3D ? null : false);
                copied++;
            }
        }
        RaiseShape3D();
        host.Say(to3D ? "3Dの図形にしました(図形の設定はそのまま。3Dの設定は「3D」の項目で変えられます)" : "2Dの図形にしました(図形の設定はそのまま)");
    }

    void RaiseShape3D() { Raise(nameof(IsShape3D)); Raise(nameof(IsShape2D)); Raise(nameof(IsEffectShape)); }

    /// <summary>書き込む値で選ぶ(図形の種類の選択画面から)</summary>
    public void PickChoiceKey(string key)
    {
        if (choiceKeys == null) return;
        int i = choiceKeys.IndexOf(key);
        if (i >= 0) ChoiceIndex = i;
        Raise(nameof(CurrentChoiceText));
        RaiseShape3D();
    }
    public string Label { get; }
    public string Group { get; }
    public string Tip { get; }
    public string Unit { get; }
    public double SliderMin { get; private set; }
    public double SliderMax { get; private set; }
    public List<string> Choices { get; } = new();
    public PaletteSwatch[] QuickColors => quickColors;
    public ICommand PickColorCommand { get; }
    public ICommand AddPointCommand { get; }
    public ICommand RemovePointCommand { get; }
    public ICommand SwapPointsCommand { get; }
    public ICommand CopyValueCommand { get; }
    public ICommand PasteValueCommand { get; }

    /// <summary>最後に「数値をコピー」した内容(移動の種類と、各点の値も覚えておく)</summary>
    sealed record CopiedValue(string Text, string AnimRaw, List<double> Points);
    static CopiedValue? copied;

    public ICommand SetAnimTypeCommand { get; }
    public ICommand CopyAnimInfoCommand { get; }

    /// <summary>カラーパレット(色相12 × 明るさ6段 + 灰色12段)</summary>
    public PaletteSwatch[] Palette => palette ??= BuildPalette();
    static PaletteSwatch[]? palette;

    static PaletteSwatch[] BuildPalette()
    {
        var list = new List<PaletteSwatch>();
        for (int i = 0; i < 12; i++)
        {
            byte g = (byte)Math.Round(255 * i / 11.0);
            list.Add(new PaletteSwatch(ColorTools.ToHex(g, g, g)));
        }
        var rows = new (double s, double v)[] { (0.25, 1.0), (0.5, 1.0), (0.75, 1.0), (1.0, 1.0), (1.0, 0.75), (1.0, 0.5) };
        foreach (var (sat, val) in rows)
            for (int h = 0; h < 12; h++)
            {
                HsvToRgb(h * 30, sat, val, out var r, out var g, out var b);
                list.Add(new PaletteSwatch(ColorTools.ToHex(r, g, b)));
            }
        return list.ToArray();
    }

    static void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
    {
        double c = v * s, x = c * (1 - Math.Abs(h / 60 % 2 - 1)), m = v - c;
        double r1 = 0, g1 = 0, b1 = 0;
        switch ((int)(h / 60) % 6)
        {
            case 0: r1 = c; g1 = x; break;
            case 1: r1 = x; g1 = c; break;
            case 2: g1 = c; b1 = x; break;
            case 3: g1 = x; b1 = c; break;
            case 4: r1 = x; b1 = c; break;
            default: r1 = c; b1 = x; break;
        }
        r = (byte)Math.Round((r1 + m) * 255); g = (byte)Math.Round((g1 + m) * 255); b = (byte)Math.Round((b1 + m) * 255);
    }

    // ---- フォントを選ぶ画面(検索・最近使ったもの・そのフォントでの見本)
    bool fontOpen;
    string fontSearch = "";
    static List<FontChoice>? allFonts;
    public ObservableCollection<FontChoice> FontList { get; } = new();
    public ObservableCollection<FontChoice> RecentFonts { get; } = new();
    public bool HasRecentFonts => RecentFonts.Count > 0;
    ICommand? pickFontCommand;
    public ICommand PickFontCommand => pickFontCommand ??= new RelayCommand(p => { if (p is FontChoice f) PickFont(f); });

    public bool FontOpen
    {
        get => fontOpen;
        set
        {
            if (!Set(ref fontOpen, value)) return;
            Raise(nameof(FontClosed));
            if (value) { fontSearch = ""; Raise(nameof(FontSearch)); FillFonts(); }
        }
    }
    public bool FontClosed => !fontOpen;
    public string FontSearch { get => fontSearch; set { if (Set(ref fontSearch, value ?? "")) FillFonts(); } }
    public string FontCountText => $"{FontList.Count} 個";
    /// <summary>いまのフォント(ボタンに、そのフォントで名前を出す)</summary>
    public FontFamily CurrentFontFamily
    {
        get
        {
            try { return new FontFamily(TextValue); }
            catch { return new FontFamily("Yu Gothic UI"); }
        }
    }

    static List<FontChoice> AllFonts() => allFonts ??= FontNames().Select(n => new FontChoice(n)).ToList();

    void FillFonts()
    {
        var q = fontSearch.Trim();
        FontList.Clear();
        foreach (var f in AllFonts())
            if (q.Length == 0 || f.Name.Contains(q, StringComparison.CurrentCultureIgnoreCase)) FontList.Add(f);
        RecentFonts.Clear();
        var all = AllFonts();
        foreach (var name in ExplorerSettings.Instance.RecentFonts ?? new List<string>())
        {
            var hit = all.FirstOrDefault(f => f.Name == name);
            if (hit != null) RecentFonts.Add(hit);
        }
        Raise(nameof(FontCountText)); Raise(nameof(HasRecentFonts));
    }

    void PickFont(FontChoice f)
    {
        TextValue = f.Name;
        var recent = ExplorerSettings.Instance.RecentFonts ?? new List<string>();
        recent.Remove(f.Name);
        recent.Insert(0, f.Name);
        if (recent.Count > 10) recent.RemoveRange(10, recent.Count - 10);
        ExplorerSettings.Instance.RecentFonts = recent;
        ExplorerSettings.Instance.Save();
        FontOpen = false;
    }

    bool paletteOpen;
    public bool PaletteOpen { get => paletteOpen; set { if (Set(ref paletteOpen, value)) Raise(nameof(PaletteClosed)); } }
    public bool PaletteClosed => !paletteOpen;

    Color NowColor => Raw() is Color c ? c : Colors.White;
    void SetColor(Color c)
    {
        if (Raw() is Color now && now == c) return;
        Commit(owner => SetProp(owner, c));
    }
    public double ColorR { get => NowColor.R; set { var c = NowColor; c.R = (byte)Math.Clamp(Math.Round(value), 0, 255); SetColor(c); } }
    public double ColorG { get => NowColor.G; set { var c = NowColor; c.G = (byte)Math.Clamp(Math.Round(value), 0, 255); SetColor(c); } }
    public double ColorB { get => NowColor.B; set { var c = NowColor; c.B = (byte)Math.Clamp(Math.Round(value), 0, 255); SetColor(c); } }
    public double ColorA { get => NowColor.A; set { var c = NowColor; c.A = (byte)Math.Clamp(Math.Round(value), 0, 255); SetColor(c); } }

    /// <summary>よく使う動かし方(なし・最初の2種類)を、ボタンで選べるようにする</summary>
    public List<AnimTypeChip> AnimQuickTypes { get; } = new();

    /// <summary>中間点を、YMM4がアイテム全体でまとめて管理している(この画面からは、点の数を変えない)</summary>
    bool keyframeManaged;
    public bool CanEditPoints => IsAnimated;
    public bool PointsLocked => IsAnimated && keyframeManaged;

    // ---- グラフ(AnimGraph)から使う
    int graphVersion;
    /// <summary>値が変わるたびに増える番号(グラフの描き直しの合図)</summary>
    public int GraphVersion => graphVersion;
    double? lockMin, lockMax;

    public List<double> GraphValues() => AnimValues(Raw()).Select(v => ToDouble(Ymm.Get(v, "Value"))).ToList();

    /// <summary>各点の横の位置(0=アイテムの始まり、1=終わり)。中間点は、フレーム位置が分かればそれに合わせる</summary>
    public List<double> GraphPositions(int count)
    {
        var list = new List<double>();
        var frames = KeyFrameList();
        int length = 0;
        try { length = targets.Count > 0 ? Convert.ToInt32(Ymm.Get(targets[0].root, "Length") ?? 0) : 0; } catch { }
        for (int i = 0; i < count; i++)
        {
            double x = count <= 1 ? 0 : i / (double)(count - 1);
            if (i > 0 && i < count - 1 && i - 1 < frames.Count && length > 0) x = Math.Clamp(frames[i - 1] / (double)length, 0.02, 0.98);
            list.Add(x);
        }
        return list;
    }

    /// <summary>グラフの縦の範囲(スライダーの範囲と、いまの値がすべて入る範囲)</summary>
    public void GraphRange(List<double> values, out double min, out double max)
    {
        if (lockMin != null && lockMax != null) { min = lockMin.Value; max = lockMax.Value; return; }
        min = SliderMin; max = SliderMax;
        foreach (var v in values) { if (v < min) min = v; if (v > max) max = v; }
        if (max - min < 1e-9) { min -= 1; max += 1; }
    }

    public void LockGraphRange(double? min, double? max) { lockMin = min; lockMax = max; }
    public Func<double, double> GraphEase() => IsAnimated ? Easing.For(CurrentRaw) : (t => t);
    public string FormatValue(double v) => Fmt(v);

    public void SetPointFromGraph(int index, double v)
    {
        if (TryParse(Fmt(v), out var rounded)) v = rounded;
        if (Math.Abs(v - Number(index)) > 1e-9) ApplyNumber(index, v);
    }

    internal double PointNumber(int index) => Number(index);

    /// <summary>始点・中間点・終点の数(移動していない項目は 1)</summary>
    internal int PointCount => Kind == ItemRowKind.Anim ? Math.Max(1, AnimValues(Raw()).Count) : 1;

    // 端までつまみを動かしている(マウスを離した時に、範囲を広げる)
    bool atMaxEdge, atMinEdge;
    // 広げる前の上限・下限(値を戻した時に、1段ずつ元に戻す)
    readonly Stack<double> maxHistory = new(), minHistory = new();

    /// <summary>
    /// 表示の桁数に丸めた値を返す(スライダー共通)。動かしている最中は範囲を変えず、端で止める。
    /// 端に当てたままマウスを離すと(ReleaseSlider)、上限・下限が今の倍に広がる
    /// </summary>
    double ExtendAndRound(double value)
    {
        if (TryParse(Fmt(value), out var rounded)) value = rounded;
        double eps = (SliderMax - SliderMin) * 0.004;
        atMaxEdge = value >= SliderMax - eps;
        atMinEdge = !atMaxEdge && value <= SliderMin + eps;
        return Math.Clamp(value, SliderMin, SliderMax);
    }

    static double Grow(double edge, double other, bool up)
    {
        double range = Math.Abs(edge - other);
        if (range <= 0) range = 1;
        if (up) return edge > 0 ? edge * 2 : edge + range;
        return edge < 0 ? edge * 2 : edge - range;
    }

    /// <summary>スライダーのつまみを離した時(画面から呼ぶ)。端に当てていたら、その端を今の倍に広げる</summary>
    public void ReleaseSlider()
    {
        if (!IsNumber) return;
        if (atMaxEdge)
        {
            maxHistory.Push(SliderMax);
            SliderMax = Grow(SliderMax, SliderMin, true);
            RaiseRange();
        }
        else if (atMinEdge)
        {
            minHistory.Push(SliderMin);
            SliderMin = Grow(SliderMin, SliderMax, false);
            RaiseRange();
        }
        atMaxEdge = atMinEdge = false;
    }

    /// <summary>スライダーの範囲の表示(下限 〜 上限)</summary>
    public string RangeText => $"下限 {Fmt(SliderMin)}  〜  上限 {Fmt(SliderMax)}"
        + (maxHistory.Count > 0 || minHistory.Count > 0 ? "(広げています。値を戻すと元に戻ります)" : "");

    void RaiseRange()
    {
        Raise(nameof(SliderMin));
        Raise(nameof(SliderMax));
        Raise(nameof(RangeText));
        Raise(nameof(SliderValue));
        foreach (var p in AnimPoints) p.Refresh();
    }

    internal void SetPointNumber(int index, double value)
    {
        value = ExtendAndRound(value);
        if (Math.Abs(value - Number(index)) < 1e-9) return;
        ApplyNumber(index, value);
    }

    // ---- 動かし方を選ぶボタン(基本の動き / 緩急の種類 / 緩急の向き)
    readonly List<string> animRaw = new();
    public ObservableCollection<AnimChoiceChip> AnimBasicTypes { get; } = new();
    public ObservableCollection<AnimChoiceChip> EaseFamilies { get; } = new();
    public ObservableCollection<AnimChoiceChip> EaseDirections { get; } = new();
    public bool HasEasing => EaseFamilies.Count > 0;
    public bool HasEaseInteger { get; private set; }
    string CurrentRaw { get { int i = AnimTypeIndex; return i >= 0 && i < animRaw.Count ? animRaw[i] : ""; } }
    public bool IsEasing => Easing.Parse(CurrentRaw, out _, out _, out _);

    public bool EaseInteger
    {
        get => Easing.Parse(CurrentRaw, out _, out _, out var integer) && integer;
        set { if (Easing.Parse(CurrentRaw, out var f, out var d, out var now) && now != value) SelectEase(f, d, value); }
    }

    public string AnimExplain
    {
        get
        {
            var raw = CurrentRaw;
            if (!IsAnimated) return "動きません(ずっと同じ値)";
            if (Easing.Parse(raw, out var f, out var d, out var integer))
                return (EaseDirection.TryGetValue(d, out var dt) ? dt : d) + "動きます"
                     + (EaseFamily.TryGetValue(f, out var ft) ? $"(曲がり方: {ft})" : "") + (integer ? "。値は整数になります" : "");
            return "始点の値から終点の値へ動きます";
        }
    }

    void SelectEase(string family, string direction, bool integer)
    {
        var want = $"{family}_{direction}{(integer ? "_Int" : "")}";
        int i = animRaw.IndexOf(want);
        if (i < 0) i = animRaw.IndexOf($"{family}_{direction}");
        if (i >= 0) AnimTypeIndex = i;
    }

    void BuildChips()
    {
        var families = new List<string>();
        for (int i = 0; i < animRaw.Count; i++)
        {
            var raw = animRaw[i];
            if (Easing.Parse(raw, out var f, out _, out var integer))
            {
                if (integer) HasEaseInteger = true;
                if (!families.Contains(f)) families.Add(f);
                continue;
            }
            int index = i;
            AnimBasicTypes.Add(new AnimChoiceChip(AnimTypeChoices[i], raw, _ => AnimTypeIndex = index, null));
        }
        foreach (var f in families)
        {
            var family = f;
            var label = EaseFamily.TryGetValue(f, out var jp) ? $"{jp}" : f;
            EaseFamilies.Add(new AnimChoiceChip(label, f, chip =>
            {
                // 向きは、いま選んでいるものを保つ(緩急でなければ「ゆっくり→速く→ゆっくり」)
                Easing.Parse(CurrentRaw, out _, out var d, out var integer);
                SelectEase(family, d.Length > 0 ? d : "InOut", integer);
            }, Curve(f, "InOut")));
        }
        if (families.Count > 0)
            foreach (var d in new[] { "In", "Out", "InOut", "OutIn" })
            {
                var direction = d;
                if (!animRaw.Any(r => Easing.Parse(r, out _, out var rd, out _) && rd == direction)) continue;
                EaseDirections.Add(new AnimChoiceChip(EaseDirection.TryGetValue(d, out var jp) ? jp : d, d, chip =>
                {
                    Easing.Parse(CurrentRaw, out var f, out _, out var integer);
                    SelectEase(f.Length > 0 ? f : families[0], direction, integer);
                }, Curve("Cubic", d)));
            }
    }

    /// <summary>ボタンに描く、小さな曲線(幅40 × 高さ24)</summary>
    static PointCollection Curve(string family, string direction)
    {
        var pts = new PointCollection();
        for (int k = 0; k <= 24; k++)
        {
            double t = k / 24.0;
            pts.Add(new Point(2 + t * 36, 21 - Math.Clamp(Easing.Apply(family, direction, t), -0.1, 1.1) * 18));
        }
        pts.Freeze();
        return pts;
    }

    void RefreshChips()
    {
        var raw = CurrentRaw;
        Easing.Parse(raw, out var f, out var d, out _);
        foreach (var c in AnimBasicTypes) c.SetSelected(c.Key == raw);
        foreach (var c in EaseFamilies) c.SetSelected(c.Key == f);
        foreach (var c in EaseDirections) c.SetSelected(c.Key == d);
        Raise(nameof(IsEasing)); Raise(nameof(EaseInteger)); Raise(nameof(AnimExplain));
    }

    /// <summary>カラーパッド用(不透明度は、いまの値のまま)</summary>
    public Color PadColor
    {
        get { var c = NowColor; c.A = 255; return c; }
        set => SetColor(Color.FromArgb(NowColor.A, value.R, value.G, value.B));
    }

    /// <summary>移動(アニメーション)の種類の一覧(なし・直線移動・加減速移動…)</summary>
    public List<string> AnimTypeChoices { get; } = new();

    /// <summary>移動の各点(始点・中間点・終点)の値</summary>
    public ObservableCollection<ItemAnimPoint> AnimPoints { get; } = new();

    public bool IsAnim => Kind == ItemRowKind.Anim && animTypes != null;
    public bool AnimOpen { get => animOpen; set => Set(ref animOpen, value); }

    public bool IsNumber => Kind == ItemRowKind.Number || Kind == ItemRowKind.Anim;
    public bool IsBool => Kind == ItemRowKind.Bool;
    public bool IsEnum => Kind == ItemRowKind.Enum;
    public bool IsPlainEnum => Kind == ItemRowKind.Enum && !IsShapeRow;
    public bool IsFont => Kind == ItemRowKind.Font;
    public bool IsText => Kind == ItemRowKind.Text;
    public bool IsMultiline => Kind == ItemRowKind.Multiline;
    public bool IsColor => Kind == ItemRowKind.Color;

    public bool IsVisible { get => isVisible; set => Set(ref isVisible, value); }

    public ItemPropRow(ItemEditorVm host, ItemRowKind kind, string propName, string label, string group, string tip,
                       List<(object root, object owner)> targets, string format, string unit, double? min, double? max, Type? enumType,
                       List<(string key, string name)>? keyChoices = null)
    {
        if (keyChoices != null)
        {
            choiceKeys = keyChoices.Select(x => x.key).ToList();
            Choices.AddRange(keyChoices.Select(x => x.name));
            rebuildAfter = true;
        }
        this.host = host;
        this.propName = propName;
        this.targets = targets;
        this.format = format;
        Kind = kind;
        Label = label;
        Group = group;
        Tip = string.IsNullOrWhiteSpace(tip) ? label : tip;
        Unit = unit;
        PickColorCommand = new RelayCommand(p =>
        {
            // 不透明度(A)は、いまの値のままにして、色だけ変える
            if (p is string hex && ColorTools.TryParse(hex, out var r, out var g, out var b)) SetColor(Color.FromArgb(NowColor.A, r, g, b));
        });
        SwapPointsCommand = new RelayCommand(_ => SwapPoints());
        CopyValueCommand = new RelayCommand(_ => CopyValue());
        PasteValueCommand = new RelayCommand(_ => PasteValue());
        SetAnimTypeCommand = new RelayCommand(p => { if (p is int i) AnimTypeIndex = i; });
        CopyAnimInfoCommand = new RelayCommand(_ => CopyAnimInfo());

        if (kind == ItemRowKind.Enum && enumType != null)
        {
            enumValues = Enum.GetValues(enumType);
            foreach (var v in enumValues) Choices.Add(EnumLabel(enumType, v!));
        }
        if (kind == ItemRowKind.Font) Choices.AddRange(FontNames());
        AddPointCommand = new RelayCommand(_ => AddPoint());
        RemovePointCommand = new RelayCommand(_ => RemovePoint(), _ => AnimPoints.Count > 2);
        if (kind == ItemRowKind.Anim)
        {
            try
            {
                var raw = Raw();
                var typeProp = raw == null ? null : Ymm.FindProperty(raw.GetType(), "AnimationType");
                if (typeProp != null && typeProp.PropertyType.IsEnum && Ymm.FindProperty(raw!.GetType(), "Values") != null)
                {
                    animTypes = Enum.GetValues(typeProp.PropertyType);
                    foreach (var v in animTypes)
                    {
                        AnimTypeChoices.Add(EnumLabel(typeProp.PropertyType, v!));
                        animRaw.Add(v!.ToString() ?? "");
                    }
                    BuildChips();
                }
            }
            catch { animTypes = null; }
            for (int i = 0; i < Math.Min(3, AnimTypeChoices.Count); i++) AnimQuickTypes.Add(new AnimTypeChip(AnimTypeChoices[i], i));
            try
            {
                var raw = Raw();
                keyframeManaged = (raw != null && Ymm.FindProperty(raw.GetType(), "KeyFrames") != null)
                                  || (targets.Count > 0 && Ymm.FindProperty(targets[0].root.GetType(), "KeyFrames") != null);
            }
            catch { keyframeManaged = false; }
            SyncPoints();
            animOpen = IsAnimated;
        }

        var now = Number(0);
        SliderMin = min ?? Math.Min(-100, -Math.Abs(now) * 2);
        SliderMax = max ?? Math.Max(100, Math.Abs(now) * 2);
        if (SliderMax <= SliderMin) SliderMax = SliderMin + 1;
        baseMin = SliderMin;
        baseMax = SliderMax;
        maxHistory.Clear();
        minHistory.Clear();
        WidenSlider(now);
    }

    /// <summary>スライダーの、もともとの範囲(端を超えて広げた後、値が戻ってきたら、この範囲に戻す)</summary>
    double baseMin, baseMax;

    /// <summary>
    /// 広げた範囲を、必要な分だけに戻す: いまの値(移動している時は、すべての点)が入る範囲で、もともとの範囲より狭くはしない。
    /// つまみを動かしている最中に範囲が変わると、つまみが飛ぶので、マウスを離している時だけ行う。
    /// </summary>
    void NormalizeSlider()
    {
        if (!IsNumber || Mouse.LeftButton == MouseButtonState.Pressed) return;
        double vLo = double.MaxValue, vHi = double.MinValue;
        if (Kind == ItemRowKind.Anim) foreach (var v in GraphValues()) { if (v < vLo) vLo = v; if (v > vHi) vHi = v; }
        else { var v = Number(0); vLo = vHi = v; }
        if (vLo == double.MaxValue) { vLo = baseMin; vHi = baseMax; }

        // 倍に広げた範囲は、値が1つ前の上限(下限)より内側に戻ったら、1段ずつ元に戻す
        double eps = (SliderMax - SliderMin) * 0.004;
        double hi = SliderMax, lo = SliderMin;
        while (maxHistory.Count > 0 && vHi < maxHistory.Peek() - eps) hi = maxHistory.Pop();
        while (minHistory.Count > 0 && vLo > minHistory.Peek() + eps) lo = minHistory.Pop();
        // 数字の入力で範囲の外にした分は、値に合わせる(戻せば、もともとの範囲へ)
        hi = maxHistory.Count > 0 ? Math.Max(hi, vHi) : Math.Max(baseMax, vHi);
        lo = minHistory.Count > 0 ? Math.Min(lo, vLo) : Math.Min(baseMin, vLo);
        bool changed = false;
        if (Math.Abs(lo - SliderMin) > 1e-9) { SliderMin = lo; changed = true; }
        if (Math.Abs(hi - SliderMax) > 1e-9) { SliderMax = hi; changed = true; }
        if (changed) RaiseRange();
    }

    static readonly Dictionary<string, string> EaseFamily = new()
    {
        ["Quad"] = "2次", ["Cubic"] = "3次", ["Quart"] = "4次", ["Quint"] = "5次", ["Sine"] = "サイン", ["Expo"] = "指数",
        ["Circ"] = "円弧", ["Back"] = "行き過ぎて戻る", ["Elastic"] = "ゴムのように", ["Bounce"] = "弾む",
    };
    static readonly Dictionary<string, string> EaseDirection = new()
    {
        ["In"] = "だんだん速く", ["Out"] = "だんだんゆっくり", ["InOut"] = "ゆっくり→速く→ゆっくり", ["OutIn"] = "速く→ゆっくり→速く",
    };

    /// <summary>「Quad_In」のような緩急(イージング)の名前を、日本語の説明にする</summary>
    static string? EaseLabel(string raw)
    {
        var parts = raw.Split('_');
        if (parts.Length < 2 || !EaseFamily.TryGetValue(parts[0], out var family) || !EaseDirection.TryGetValue(parts[1], out var direction)) return null;
        bool integer = parts.Length > 2 && parts[2] == "Int";
        return $"緩急・{direction}({family}{(integer ? "・整数" : "")})";
    }

    internal static string EnumLabel(Type enumType, object value)
    {
        var raw = value.ToString() ?? "";
        try
        {
            var f = enumType.GetField(raw);
            var d = f?.GetCustomAttribute<DisplayAttribute>();
            var n = d?.GetName();
            if (!string.IsNullOrWhiteSpace(n)) return YmmText.Resolve(n!);
            // 表示名の印が別の種類の時: Name / DisplayName という文字の項目を探す
            if (f != null)
                foreach (var a in f.GetCustomAttributes(false))
                    foreach (var pn in new[] { "Name", "DisplayName", "Description" })
                        if (a.GetType().GetProperty(pn)?.GetValue(a) is string text && !string.IsNullOrWhiteSpace(text)
                            && a.GetType().Namespace?.StartsWith("System.Runtime", StringComparison.Ordinal) != true)
                            return YmmText.Resolve(text);
        }
        catch { }
        return EaseLabel(raw) ?? YmmText.Resolve(raw);
    }

    static List<string> FontNames()
    {
        if (fontNames != null) return fontNames;
        var list = new List<string>();
        try
        {
            var ja = System.Windows.Markup.XmlLanguage.GetLanguage("ja-jp");
            foreach (var f in Fonts.SystemFontFamilies)
            {
                try { list.Add(f.FamilyNames.TryGetValue(ja, out var n) && !string.IsNullOrWhiteSpace(n) ? n : f.Source); }
                catch { }
            }
        }
        catch { }
        fontNames = list.Distinct().OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase).ToList();
        return fontNames;
    }

    // ------------------------------------------------------------------ 読み取り
    /// <summary>
    /// いまの入れ物を取り出し直す。図形の設定などは、YMM4が途中で新しい物に入れ替えることがあるので、
    /// 作った時の物を使い続けると、YMM4に使われていない古い物を書き換えてしまう(プレビューに反映されない)
    /// </summary>
    object Owner(object root, object owner)
    {
        if (Resolver == null) return owner;
        try
        {
            var now = Resolver(root);
            if (now != null && !ReferenceEquals(now, owner) && Ymm.FindProperty(now.GetType(), propName) != null) return now;
        }
        catch { }
        return owner;
    }

    object? Raw()
    {
        if (targets.Count == 0) return null;
        var owner = Owner(targets[0].root, targets[0].owner);
        try { return Ymm.FindProperty(owner.GetType(), propName)?.GetValue(owner); }
        catch { return null; }
    }

    static List<object> AnimValues(object? animation)
    {
        var list = new List<object>();
        if (Ymm.Get(animation, "Values") is IEnumerable e)
            foreach (var x in e) if (x != null) list.Add(x);
        return list;
    }

    static double ToDouble(object? v)
    {
        try { return v == null ? 0 : Convert.ToDouble(v, CultureInfo.InvariantCulture); }
        catch { return 0; }
    }

    double Number(int index)
    {
        var raw = Raw();
        if (Kind == ItemRowKind.Anim)
        {
            var values = AnimValues(raw);
            return index < values.Count ? ToDouble(Ymm.Get(values[index], "Value")) : 0;
        }
        return Kind == ItemRowKind.Number ? ToDouble(raw) : 0;
    }

    string Fmt(double v)
    {
        try { return v.ToString(format, CultureInfo.InvariantCulture); }
        catch { return v.ToString("0.##", CultureInfo.InvariantCulture); }
    }

    void WidenSlider(double v)
    {
        bool changed = false;
        if (v < SliderMin) { SliderMin = v; changed = true; }
        if (v > SliderMax) { SliderMax = v; changed = true; }
        if (changed) RaiseRange();
    }

    /// <summary>移動している項目(移動の種類が「なし」以外)</summary>
    public bool IsAnimated
    {
        get
        {
            if (Kind != ItemRowKind.Anim) return false;
            if (animTypes == null) return AnimValues(Raw()).Count >= 2;
            return AnimTypeIndex > 0;
        }
    }

    public int AnimTypeIndex
    {
        get
        {
            if (animTypes == null) return -1;
            var now = Ymm.Get(Raw(), "AnimationType");
            for (int i = 0; i < animTypes.Length; i++) if (Equals(animTypes.GetValue(i), now)) return i;
            return -1;
        }
        set
        {
            if (animTypes == null || value < 0 || value >= animTypes.Length || value == AnimTypeIndex) return;
            var type = animTypes.GetValue(value);
            bool moving = value > 0;
            Commit(owner =>
            {
                var anim = AnimOf(owner);
                if (anim == null) return;
                // 移動させる時は、始点と終点の2つの値が要る(足りなければ、いまの値を終点として足す)
                if (moving) EnsurePoints(anim, 2);
                if (!Ymm.SetValue(anim, "AnimationType", type, out var error)) host.Report($"「{Label}」の移動を変えられませんでした: {error}");
            });
        }
    }

    public string AnimButtonText => IsAnimated && AnimTypeIndex >= 0 && AnimTypeIndex < AnimTypeChoices.Count
        ? "移動: " + AnimTypeChoices[AnimTypeIndex] : "移動なし";

    object? AnimOf(object owner)
    {
        try { return Ymm.FindProperty(owner.GetType(), propName)?.GetValue(owner); }
        catch { return null; }
    }

    static object? NewPoint(Type elementType, double v)
    {
        try
        {
            var o = Activator.CreateInstance(elementType, true);
            if (o != null && Ymm.SetValue(o, "Value", v, out _)) return o;
        }
        catch { }
        try { return Activator.CreateInstance(elementType, v); }
        catch { return null; }
    }

    /// <summary>移動の点の一覧を、list に入れ替える</summary>
    bool SetPoints(object anim, List<object> list)
    {
        var p = Ymm.FindProperty(anim.GetType(), "Values");
        if (p == null) return false;
        var merged = Ymm.MergeEffects(p.PropertyType, null, list, false);
        if (merged == null) { host.Report(Ymm.LastError); return false; }
        if (Ymm.SetValue(anim, "Values", merged, out var error)) return true;
        host.Report($"「{Label}」の移動の点を変えられませんでした: {error}");
        return false;
    }

    void EnsurePoints(object anim, int min)
    {
        var values = AnimValues(anim);
        if (values.Count == 0 || values.Count >= min) return;
        var elementType = values[0].GetType();
        double last = ToDouble(Ymm.Get(values[^1], "Value"));
        while (values.Count < min)
        {
            var added = NewPoint(elementType, last);
            if (added == null) { host.Report($"「{Label}」の終点を作れませんでした"); return; }
            values.Add(added);
        }
        SetPoints(anim, values);
    }

    // ------------------------------------------------------------------ 中間点(キーフレーム)
    // YMM4は、中間点の位置(フレーム)をアイテム全体で1組だけ持ち(KeyFrames.Frames)、
    // 移動している項目それぞれが「中間点の数 + 2」個の値を持つ、という前提で書いている。
    // 前提と違う形だった時は、何も書き換えずにメッセージを出す。

    static object? KeyFramesOf(object root, object? anim) => Ymm.Get(anim, "KeyFrames") ?? Ymm.Get(root, "KeyFrames");

    static List<int>? ReadFrames(object? keyFrames)
    {
        if (keyFrames == null || Ymm.Get(keyFrames, "Frames") is not IEnumerable e) return null;
        var list = new List<int>();
        try { foreach (var f in e) if (f != null) list.Add(Convert.ToInt32(f, CultureInfo.InvariantCulture)); }
        catch { return null; }
        return list;
    }

    bool WriteFrames(object keyFrames, List<int> frames)
    {
        try
        {
            var p = Ymm.FindProperty(keyFrames.GetType(), "Frames");
            if (p == null) return false;
            var now = p.GetValue(keyFrames);
            // その場で書き換えられる入れ物(List など)なら、中身を入れ替える
            if (now is IList il && !il.IsReadOnly && !il.IsFixedSize)
            {
                il.Clear();
                foreach (var f in frames) il.Add(f);
            }
            else
            {
                object? value = p.PropertyType.IsArray ? frames.ToArray()
                              : p.PropertyType.IsAssignableFrom(typeof(List<int>)) ? new List<int>(frames)
                              : Ymm.MergeEffects(p.PropertyType, null, frames.Cast<object>(), false);
                if (value == null) { host.Report("中間点の位置を作れませんでした: " + Ymm.LastError); return false; }
                if (!Ymm.SetValue(keyFrames, "Frames", value, out var error)) { host.Report("中間点の位置を書き込めませんでした: " + error); return false; }
            }
            var count = Ymm.FindProperty(keyFrames.GetType(), "Count");
            if (count != null && count.PropertyType == typeof(int) && count.GetSetMethod(true) != null) Ymm.SetValue(keyFrames, "Count", frames.Count, out _);
            return true;
        }
        catch (Exception ex) { host.Report("中間点の位置を書き込めませんでした: " + ex.GetBaseException().Message); return false; }
    }

    /// <summary>アイテムの中の、移動できる項目(Animation)をすべて集める(エフェクトや、図形の設定の中も)</summary>
    static void CollectAnimations(object? o, int depth, List<object> result, HashSet<object> seen)
    {
        if (o == null || depth > 4 || !seen.Add(o)) return;
        PropertyInfo[] props;
        try { props = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance); }
        catch { return; }
        foreach (var p in props)
        {
            try
            {
                if (p.GetIndexParameters().Length != 0 || !p.CanRead) continue;
                var pt = p.PropertyType;
                if (pt.IsValueType || pt == typeof(string) || p.Name == "KeyFrames") continue;
                var v = p.GetValue(o);
                if (v == null) continue;
                var vt = v.GetType();
                if (vt.Name == "Animation") { if (seen.Add(v)) result.Add(v); continue; }
                if (v is IEnumerable e)
                {
                    if (p.Name == "VideoEffects" || p.Name == "AudioEffects")
                        foreach (var x in e) CollectAnimations(x, depth + 1, result, seen);
                    continue;
                }
                var ns = vt.Namespace ?? "";
                if (ns.StartsWith("System", StringComparison.Ordinal) || ns.StartsWith("Microsoft", StringComparison.Ordinal)) continue;
                // 設定の入れ物(表示名つきの項目を持つもの)だけ、中まで見る
                if (vt.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(x => x.GetCustomAttribute<DisplayAttribute>(true) != null))
                    CollectAnimations(v, depth + 1, result, seen);
            }
            catch { }
        }
    }

    static int LengthOf(object root)
    {
        try { return Convert.ToInt32(Ymm.Get(root, "Length") ?? 0, CultureInfo.InvariantCulture); }
        catch { return 0; }
    }

    const string ShapeMismatch = "中間点の数と、この項目の値の数が合いません(想定と違う形です)。「そのほか」→「内部データをコピー」で送ってください";

    /// <summary>
    /// 横の位置 fraction(0=始まり、1=終わり)に、値 value の中間点を足す。
    /// YMM4が中間点をアイテム全体で管理している時は、ほかの移動している項目にも、同じ位置に(いまの動きのままになる値で)点が入る。
    /// </summary>
    public void InsertPointAt(double fraction, double value)
    {
        if (!IsAnimated) return;
        if (TryParse(Fmt(value), out var rounded)) value = rounded;
        WidenSlider(value);
        CommitRoot((root, owner) =>
        {
            var anim = AnimOf(owner);
            if (anim == null) return;
            var values = AnimValues(anim);
            if (values.Count < 2) return;
            var elementType = values[0].GetType();
            var keyFrames = KeyFramesOf(root, anim);
            var frames = ReadFrames(keyFrames);

            if (keyFrames == null || frames == null)
            {
                // 中間点の位置を持たない形: この項目の値の並びにだけ、足す
                int at = Math.Clamp((int)Math.Ceiling(fraction * (values.Count - 1)), 1, values.Count - 1);
                var added = NewPoint(elementType, value);
                if (added == null) { host.Report("中間点を作れませんでした"); return; }
                values.Insert(at, added);
                SetPoints(anim, values);
                return;
            }

            int length = LengthOf(root);
            if (length < 2) { host.Report("アイテムが短すぎて、中間点を入れられません"); return; }
            int frame = Math.Clamp((int)Math.Round(fraction * length), 1, length - 1);
            if (values.Count != frames.Count + 2) { host.Report(ShapeMismatch); return; }
            int same = frames.IndexOf(frame);
            if (same >= 0) { Ymm.SetValue(values[same + 1], "Value", value, out _); return; }

            int pos = frames.Count(f => f < frame);          // 新しい中間点が入る場所(0〜)
            int before = pos == 0 ? 0 : frames[pos - 1], after = pos == frames.Count ? length : frames[pos];
            double t = after > before ? (frame - before) / (double)(after - before) : 0.5;

            var all = new List<object>();
            CollectAnimations(root, 0, all, new HashSet<object>(ReferenceEqualityComparer.Instance));
            if (!all.Any(a => ReferenceEquals(a, anim))) all.Add(anim);

            var next = new List<int>(frames);
            next.Insert(pos, frame);
            if (!WriteFrames(keyFrames, next)) return;       // 位置を書けなかった時は、値には触らない

            foreach (var a in all)
            {
                var list = AnimValues(a);
                if (list.Count != frames.Count + 2) continue;    // 移動していない項目(値が1つ)などは、そのまま
                double v;
                if (ReferenceEquals(a, anim)) v = value;
                else
                {
                    // ほかの項目は、見た目の動きが変わらないように、前後の点のあいだの値を入れる
                    double v0 = ToDouble(Ymm.Get(list[pos], "Value")), v1 = ToDouble(Ymm.Get(list[pos + 1], "Value"));
                    v = v0 + (v1 - v0) * t;
                }
                var added = NewPoint(list[0].GetType(), v);
                if (added == null) { host.Report("中間点を作れませんでした"); continue; }
                list.Insert(pos + 1, added);
                SetPoints(a, list);
            }
        });
    }

    /// <summary>中間点 index(1〜)を消す。YMM4がアイテム全体で管理している時は、ほかの項目の同じ中間点も消える</summary>
    public void RemovePointAt(int index)
    {
        CommitRoot((root, owner) =>
        {
            var anim = AnimOf(owner);
            if (anim == null) return;
            var values = AnimValues(anim);
            if (index <= 0 || index >= values.Count - 1) return;
            var keyFrames = KeyFramesOf(root, anim);
            var frames = ReadFrames(keyFrames);
            if (keyFrames == null || frames == null)
            {
                values.RemoveAt(index);
                SetPoints(anim, values);
                return;
            }
            if (values.Count != frames.Count + 2) { host.Report(ShapeMismatch); return; }
            var all = new List<object>();
            CollectAnimations(root, 0, all, new HashSet<object>(ReferenceEqualityComparer.Instance));
            if (!all.Any(a => ReferenceEquals(a, anim))) all.Add(anim);
            var next = new List<int>(frames);
            next.RemoveAt(index - 1);
            if (!WriteFrames(keyFrames, next)) return;
            foreach (var a in all)
            {
                var list = AnimValues(a);
                if (list.Count != frames.Count + 2) continue;
                list.RemoveAt(index);
                SetPoints(a, list);
            }
        });
    }

    /// <summary>中間点 index(1〜)の位置を、横の位置 fraction へ動かす(前後の点は追い越さない)</summary>
    public void MovePointTo(int index, double fraction)
    {
        if (targets.Count == 0) return;
        var (root0, owner0) = targets[0];
        var frames0 = ReadFrames(KeyFramesOf(root0, AnimOf(owner0)));
        int length0 = LengthOf(root0);
        if (frames0 == null || index <= 0 || index - 1 >= frames0.Count || length0 < 2) return;
        if (frames0[index - 1] == Math.Clamp((int)Math.Round(fraction * length0), 1, length0 - 1)) return;   // 変わらないなら、何もしない
        CommitRoot((root, owner) =>
        {
            var keyFrames = KeyFramesOf(root, AnimOf(owner));
            var frames = ReadFrames(keyFrames);
            int length = LengthOf(root);
            if (keyFrames == null || frames == null || index - 1 >= frames.Count || length < 2) return;
            int low = index - 1 == 0 ? 1 : frames[index - 2] + 1;
            int high = index - 1 == frames.Count - 1 ? length - 1 : frames[index] - 1;
            if (high < low) return;
            int frame = Math.Clamp((int)Math.Round(fraction * length), low, high);
            if (frame == frames[index - 1]) return;
            var next = new List<int>(frames) { [index - 1] = frame };
            WriteFrames(keyFrames, next);
        });
    }

    // ------------------------------------------------------------------ グラフの操作モード(すべてのグラフで共通)
    // 0 = ふつう(点を上下に動かす・線の上で中間点を追加)
    // 1 = 手書き(マウスでなぞった形を、そのまま動きにする)
    // 2 = 中間点を横にも動かす(点を上下左右に動かす)
    static int graphMode;
    public static int CurrentGraphMode => graphMode;
    public int GraphMode => graphMode;
    public bool GraphModeNormal { get => graphMode == 0; set { if (value) SetGraphMode(0); } }
    public bool GraphModeDraw { get => graphMode == 1; set => SetGraphMode(value ? 1 : 0); }
    public bool GraphModeSlide { get => graphMode == 2; set => SetGraphMode(value ? 2 : 0); }
    public string GraphModeHint => graphMode switch
    {
        1 => "手書きモード: グラフの上をマウスでなぞると、その形がそのまま動きになります(なぞった所に中間点が入ります)。なめらかにしたい時は「直線移動」がおすすめです",
        2 => "中間点横移動モード: 中間点を上下だけでなく左右にもドラッグできます(左右 = いつ、上下 = 値)。Shift を押しながらで、左右だけ動かせます",
        _ => "丸い点を上下にドラッグで値を変更 / 線の上をドラッグで中間点を追加 / 中間点を右クリックで削除",
    };

    void SetGraphMode(int mode)
    {
        if (graphMode != mode) graphMode = mode;
        RaiseGraphMode();
    }

    void RaiseGraphMode()
    {
        Raise(nameof(GraphMode));
        Raise(nameof(GraphModeNormal));
        Raise(nameof(GraphModeDraw));
        Raise(nameof(GraphModeSlide));
        Raise(nameof(GraphModeHint));
    }

    /// <summary>アイテムの長さ(フレーム)。分からない時は 0</summary>
    public int ItemLength => targets.Count > 0 ? LengthOf(targets[0].root) : 0;

    /// <summary>中間点の横の位置を動かせるか(アイテム全体の中間点の位置を書き換えられる形か)</summary>
    public bool CanMovePoints => targets.Count > 0 && ReadFrames(KeyFramesOf(targets[0].root, AnimOf(targets[0].owner))) != null && ItemLength >= 2;

    /// <summary>
    /// 手書きの線を、動きにする。points は (横の位置 0〜1, 値) を左から順に。
    /// なぞった範囲の中に中間点を入れて、その範囲の点の値を、線の高さにする(範囲の外の点はそのまま)。
    /// 中間点の位置はアイテム全体で共通なので、ほかの移動している項目にも同じ位置に点が入る(動きが変わらない値で)
    /// </summary>
    public void ApplyStroke(List<(double f, double v)> points)
    {
        if (points.Count < 2) return;
        if (!IsAnimated)
        {
            // 移動なしの時は、まず「直線移動」にする
            if (animTypes != null && animTypes.Length > 1) AnimTypeIndex = 1;
            if (!IsAnimated) { host.Report($"「{Label}」は移動させられないので、手書きできません"); return; }
        }
        var pts = points.Select(p =>
        {
            double v = TryParse(Fmt(p.v), out var r) ? r : p.v;
            WidenSlider(v);
            return (f: Math.Clamp(p.f, 0, 1), v);
        }).OrderBy(p => p.f).ToList();
        double f0 = pts[0].f, f1 = pts[^1].f;

        double StrokeAt(double f)
        {
            if (f <= pts[0].f) return pts[0].v;
            for (int i = 1; i < pts.Count; i++)
                if (f <= pts[i].f)
                {
                    double w = pts[i].f - pts[i - 1].f;
                    return w <= 0 ? pts[i].v : pts[i - 1].v + (pts[i].v - pts[i - 1].v) * (f - pts[i - 1].f) / w;
                }
            return pts[^1].v;
        }

        CommitRoot((root, owner) =>
        {
            var anim = AnimOf(owner);
            if (anim == null) return;
            var values = AnimValues(anim);
            if (values.Count < 2) return;
            var keyFrames = KeyFramesOf(root, anim);
            var frames = ReadFrames(keyFrames);

            if (keyFrames == null || frames == null)
            {
                // 中間点の位置を持たない形: 等間隔の点の値だけ、線に合わせる
                for (int i = 0; i < values.Count; i++)
                {
                    double f = i / (double)(values.Count - 1);
                    if (f >= f0 - 0.001 && f <= f1 + 0.001) Ymm.SetValue(values[i], "Value", StrokeAt(f), out _);
                }
                SetPoints(anim, values);
                return;
            }

            int length = LengthOf(root);
            if (length < 2) { host.Report("アイテムが短すぎて、手書きできません"); return; }
            if (values.Count != frames.Count + 2) { host.Report(ShapeMismatch); return; }

            // なぞった所の中間点(フレーム)を足す
            var add = pts.Select(p => (int)Math.Round(p.f * length)).Where(fr => fr >= 1 && fr <= length - 1);
            var next = frames.Union(add).Distinct().OrderBy(x => x).ToList();
            var oldPos = new List<int> { 0 };
            oldPos.AddRange(frames);
            oldPos.Add(length);
            var newPos = new List<int> { 0 };
            newPos.AddRange(next);
            newPos.Add(length);

            var all = new List<object>();
            CollectAnimations(root, 0, all, new HashSet<object>(ReferenceEqualityComparer.Instance));
            if (!all.Any(a => ReferenceEquals(a, anim))) all.Add(anim);

            if (next.Count != frames.Count && !WriteFrames(keyFrames, next)) return;   // 位置を書けなかった時は、値には触らない

            double eps = 0.5 / length;
            foreach (var a in all)
            {
                var list = AnimValues(a);
                if (list.Count != frames.Count + 2) continue;      // 移動していない項目などは、そのまま
                bool mine = ReferenceEquals(a, anim);
                var type = list[0].GetType();
                var rebuilt = new List<object>();
                for (int k = 0; k < newPos.Count; k++)
                {
                    int fr = newPos[k];
                    int at = oldPos.IndexOf(fr);
                    object? el;
                    if (at >= 0) el = list[at];          // 前からある点は、そのまま使う
                    else
                    {
                        // 新しく入った点: いまの動き(前後の点のあいだ)の値を入れる
                        int j = Math.Max(1, oldPos.FindIndex(x => x >= fr));
                        int a0 = oldPos[j - 1], a1 = oldPos[j];
                        double v0 = ToDouble(Ymm.Get(list[j - 1], "Value")), v1 = ToDouble(Ymm.Get(list[j], "Value"));
                        double t = a1 > a0 ? (fr - a0) / (double)(a1 - a0) : 0;
                        el = NewPoint(type, v0 + (v1 - v0) * t);
                        if (el == null) { host.Report("中間点を作れませんでした"); return; }
                    }
                    if (mine)
                    {
                        double f = fr / (double)length;
                        if (f >= f0 - eps && f <= f1 + eps) Ymm.SetValue(el, "Value", StrokeAt(f), out _);
                    }
                    rebuilt.Add(el);
                }
                SetPoints(a, rebuilt);
            }
        });
        graphVersion++;
        Raise(nameof(GraphVersion));
    }

    /// <summary>いちばん間が空いている所のまん中に、中間点を足す(ボタン用)</summary>
    void AddPoint()
    {
        var values = GraphValues();
        if (values.Count < 2) return;
        var pos = GraphPositions(values.Count);
        int widest = 0;
        for (int i = 1; i < pos.Count - 1; i++) if (pos[i + 1] - pos[i] > pos[widest + 1] - pos[widest]) widest = i;
        InsertPointAt((pos[widest] + pos[widest + 1]) / 2, (values[widest] + values[widest + 1]) / 2);
    }

    /// <summary>最後の中間点を消す(始点と終点は残す)</summary>
    void RemovePoint()
    {
        int n = GraphValues().Count;
        if (n > 2) RemovePointAt(n - 2);
    }

    internal string PointText(int index) => Fmt(Number(index));

    internal void CopyPoint(int index, string pointLabel)
    {
        try
        {
            var text = Fmt(Number(index));
            Clipboard.SetText(text);
            host.Say($"「{Label}」の{pointLabel}の値 {text} をコピーしました");
        }
        catch (Exception ex) { host.Report("コピーできませんでした: " + ex.GetBaseException().Message); }
    }

    internal void PastePoint(int index, string pointLabel)
    {
        string clip = "";
        try { clip = Clipboard.ContainsText() ? Clipboard.GetText().Trim() : ""; } catch { }
        if (!TryParse(clip, out var number)) { host.Report("貼り付けられる数値が、クリップボードにありません"); return; }
        WidenSlider(number);
        if (Math.Abs(number - Number(index)) > 1e-9) ApplyNumber(index, number);
        host.Say($"「{Label}」の{pointLabel}へ {Fmt(number)} を貼り付けました");
    }

    /// <summary>グラフを、タブいっぱいに大きく開く</summary>
    public ICommand OpenGraphCommand => openGraphCommand ??= new RelayCommand(_ => host.ShowGraph(this));
    ICommand? openGraphCommand;

    internal void SetPointText(int index, string? text)
    {
        if (TryParse(text, out var v) && Math.Abs(v - Number(index)) > 1e-9) ApplyNumber(index, v);
    }

    /// <summary>中間点のフレーム位置(YMM4が持っていれば)</summary>
    List<int> KeyFrameList()
    {
        var list = new List<int>();
        try
        {
            var kf = Ymm.Get(Raw(), "KeyFrames") ?? (targets.Count > 0 ? Ymm.Get(targets[0].root, "KeyFrames") : null);
            if (Ymm.Get(kf, "Frames") is IEnumerable frames)
                foreach (var f in frames) if (f != null) list.Add(Convert.ToInt32(f, CultureInfo.InvariantCulture));
        }
        catch { }
        return list;
    }

    void SwapPoints()
    {
        Commit(owner =>
        {
            var values = AnimValues(AnimOf(owner));
            if (values.Count < 2) return;
            double first = ToDouble(Ymm.Get(values[0], "Value")), last = ToDouble(Ymm.Get(values[^1], "Value"));
            Ymm.SetValue(values[0], "Value", last, out _);
            Ymm.SetValue(values[^1], "Value", first, out _);
        });
    }

    /// <summary>この項目の数値をコピーする(移動している項目は、動かし方と各点の値も覚える)</summary>
    void CopyValue()
    {
        try
        {
            var text = Fmt(Number(0));
            var points = Kind == ItemRowKind.Anim ? GraphValues() : new List<double> { Number(0) };
            copied = new CopiedValue(text, IsAnimated ? CurrentRaw : "", points);
            Clipboard.SetText(text);
            host.Say(IsAnimated ? $"「{Label}」の値と移動({points.Count} 点)をコピーしました" : $"「{Label}」の値 {text} をコピーしました");
        }
        catch (Exception ex) { host.Report("コピーできませんでした: " + ex.GetBaseException().Message); }
    }

    /// <summary>コピーした数値を、この項目へ貼り付ける(ほかのアプリでコピーした数字も貼り付けられる)</summary>
    void PasteValue()
    {
        string clip = "";
        try { clip = Clipboard.ContainsText() ? Clipboard.GetText().Trim() : ""; } catch { }
        var c = copied;
        // このタブでコピーした移動を、移動できる項目へ貼る時は、動かし方と各点の値もそのまま写す
        if (c != null && c.Text == clip && c.AnimRaw.Length > 0 && c.Points.Count >= 2 && IsAnim)
        {
            int typeIndex = animRaw.IndexOf(c.AnimRaw);
            var type = typeIndex >= 0 ? animTypes!.GetValue(typeIndex) : null;
            Commit(owner =>
            {
                var anim = AnimOf(owner);
                if (anim == null) return;
                EnsurePoints(anim, 2);
                if (type != null) Ymm.SetValue(anim, "AnimationType", type, out _);
                var values = AnimValues(anim);
                // 点の数が違う時は、始点と終点を合わせ、あいだは順番に写す
                for (int i = 0; i < values.Count; i++)
                {
                    double v = i == values.Count - 1 ? c.Points[^1] : c.Points[Math.Min(i, c.Points.Count - 1)];
                    Ymm.SetValue(values[i], "Value", v, out _);
                }
            });
            host.Say($"「{Label}」へ、値と移動を貼り付けました");
            return;
        }
        if (!TryParse(clip, out var number)) { host.Report("貼り付けられる数値が、クリップボードにありません"); return; }
        ApplyNumber(0, number);
        host.Say($"「{Label}」へ {Fmt(number)} を貼り付けました");
    }

    /// <summary>うまく動かない時の調査用: この項目の移動の内部データを、クリップボードへコピーする</summary>
    void CopyAnimInfo()
    {
        try
        {
            var raw = Raw();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"項目: {Label} ({propName}) / 型: {raw?.GetType().FullName}");
            sb.AppendLine("移動のデータ: " + (Ymm.ValueToJson(raw) ?? "(JSONにできません: " + Ymm.LastError + ")"));
            if (raw != null)
                sb.AppendLine("項目の一覧: " + string.Join(", ", raw.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(p => $"{p.PropertyType.Name} {p.Name}")));
            if (targets.Count > 0)
            {
                var kf = Ymm.Get(targets[0].root, "KeyFrames");
                sb.AppendLine("アイテムの中間点: " + (kf == null ? "(なし)" : Ymm.ValueToJson(kf) ?? kf.ToString()));
                sb.AppendLine($"アイテム: {targets[0].root.GetType().FullName} / 長さ {Ymm.Get(targets[0].root, "Length")}");
            }
            Clipboard.SetText(sb.ToString());
            host.Report("この項目の移動の内部データを、クリップボードへコピーしました(貼り付けて送ってください)");
        }
        catch (Exception ex) { host.Report("コピーできませんでした: " + ex.GetBaseException().Message); }
    }

    void SyncPoints()
    {
        if (Kind != ItemRowKind.Anim) return;
        var values = AnimValues(Raw());
        int n = values.Count;
        var frames = KeyFrameList();
        var key = n + ":" + string.Join(",", frames);
        if (pointsKey != key)
        {
            pointsKey = key;
            AnimPoints.Clear();
            for (int i = 0; i < n; i++)
            {
                // 中間点 i (1〜)のフレーム位置は、frames[i-1]
                int? frame = i > 0 && i < n - 1 && i - 1 < frames.Count ? frames[i - 1] : null;
                AnimPoints.Add(new ItemAnimPoint(this, i, n, frame));
            }
            CommandManager.InvalidateRequerySuggested();
        }
        else foreach (var p in AnimPoints) p.Refresh();

        RefreshChips();
        graphVersion++;
        Raise(nameof(GraphVersion));
        Raise(nameof(CanEditPoints));
        Raise(nameof(PointsLocked));
    }
    string pointsKey = "";

    // ------------------------------------------------------------------ 書き込み
    /// <summary>
    /// 値を書き込む。スライダーやグラフを動かしている間は、YMM4へすぐに書き込み(プレビューがなめらかに付いてくる)、
    /// 動かし終わってから、まとめて「元に戻す」の1回分にする(host.LiveEdit)。
    /// </summary>
    void Commit(Action<object> set, int idleMs = 350)
    {
        foreach (var (root, owner) in targets)
        {
            try { var o = Owner(root, owner); host.LiveEdit(root, () => set(o), idleMs); }
            catch (Exception ex) { host.Report($"「{Label}」を変更できませんでした: {ex.InnerException?.Message ?? ex.Message}"); }
        }
        if (rebuildAfter) host.Rebuild(); else Refresh();
    }

    /// <summary>Commit と同じだが、アイテム本体(root)も受け取る(中間点のように、アイテム全体に関わる変更用)</summary>
    void CommitRoot(Action<object, object> set)
    {
        foreach (var (root, owner) in targets)
        {
            try { var o = Owner(root, owner); host.LiveEdit(root, () => set(root, o)); }
            catch (Exception ex) { host.Report($"「{Label}」を変更できませんでした: {ex.InnerException?.Message ?? ex.Message}"); }
        }
        Refresh();
    }

    void SetProp(object owner, object? value)
    {
        if (!Ymm.SetValue(owner, propName, value, out var error)) host.Report($"「{Label}」を変更できませんでした: {error}");
    }

    void ApplyNumber(int index, double v)
    {
        WidenSlider(v);
        if (Kind == ItemRowKind.Anim)
        {
            Commit(owner =>
            {
                object? anim;
                try { anim = Ymm.FindProperty(owner.GetType(), propName)?.GetValue(owner); }
                catch { anim = null; }
                var values = AnimValues(anim);
                if (index >= values.Count) return;
                if (!Ymm.SetValue(values[index], "Value", v, out var error)) host.Report($"「{Label}」を変更できませんでした: {error}");
            });
            return;
        }
        Commit(owner =>
        {
            var p = Ymm.FindProperty(owner.GetType(), propName);
            if (p == null) return;
            var t = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
            SetProp(owner, Convert.ChangeType(v, t, CultureInfo.InvariantCulture));
        });
    }

    static bool TryParse(string? text, out double v)
    {
        text = (text ?? "").Trim().Replace(',', '.');
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }

    public string NumText
    {
        get => Fmt(Number(0));
        set { if (TryParse(value, out var v) && Math.Abs(v - Number(0)) > 1e-9) ApplyNumber(0, v); else Raise(nameof(NumText)); }
    }

    public double SliderValue
    {
        get => Math.Clamp(Number(0), SliderMin, SliderMax);
        set
        {
            // 表示の桁数に合わせて丸める(細かすぎる値にならないように)
            value = ExtendAndRound(value);
            if (Math.Abs(value - Number(0)) < 1e-9) return;
            ApplyNumber(0, value);
        }
    }

    public bool BoolValue
    {
        get => Raw() is bool b && b;
        set { if (value != BoolValue) Commit(owner => SetProp(owner, value)); }
    }

    public int ChoiceIndex
    {
        get
        {
            if (choiceKeys != null && ChoiceGet != null)
            {
                if (targets.Count == 0) return -1;
                string? cur = null;
                try { cur = ChoiceGet(targets[0].owner); } catch { }
                return cur == null ? -1 : choiceKeys.IndexOf(cur);
            }
            var raw = Raw();
            if (choiceKeys != null) return raw is string key ? choiceKeys.IndexOf(key) : -1;
            if (enumValues == null) return -1;
            for (int i = 0; i < enumValues.Length; i++) if (Equals(enumValues.GetValue(i), raw)) return i;
            return -1;
        }
        set
        {
            if (value == ChoiceIndex || value < 0) return;
            if (choiceKeys != null)
            {
                if (value >= choiceKeys.Count) return;
                var key = choiceKeys[value];
                if (ChoiceSet != null)
                {
                    Commit(owner =>
                    {
                        try { ChoiceSet(owner, key); }
                        catch (Exception ex) { host.Report($"「{Label}」を変更できませんでした: {ex.InnerException?.Message ?? ex.Message}"); }
                    });
                    return;
                }
                Commit(owner => SetProp(owner, key));
                return;
            }
            if (enumValues == null || value >= enumValues.Length) return;
            var v = enumValues.GetValue(value);
            Commit(owner => SetProp(owner, v));
        }
    }

    public string TextValue
    {
        get => Raw() as string ?? "";
        set
        {
            value ??= "";
            if (value == TextValue) return;
            // 文字は、1文字打つごとにYMM4へ送る。打つ手が少し止まったら(0.12秒)編集を区切り、プレビューに確実に出す
            Commit(owner => SetProp(owner, value), Kind is ItemRowKind.Multiline or ItemRowKind.Text ? 120 : 350);
        }
    }

    // ---- 日本語入力(IME)の変換中の文字も、確定を待たずにプレビューへ送る
    bool composing;
    public bool IsComposing => composing;

    /// <summary>変換中の文字を含めた全体の文字を、YMM4へ送る(入力欄の表示は変えない)</summary>
    public void PreviewComposing(string fullText)
    {
        composing = true;
        if (fullText == (Raw() as string ?? "")) return;
        foreach (var (root, owner) in targets)
        {
            try { host.LiveEdit(root, () => SetProp(owner, fullText), 120); }
            catch (Exception ex) { host.Report($"「{Label}」を変更できませんでした: {ex.InnerException?.Message ?? ex.Message}"); }
        }
    }

    /// <summary>変換が終わった(確定・取り消し)。入力欄の文字に合わせる</summary>
    public void EndComposing(string text)
    {
        composing = false;
        if (text != (Raw() as string ?? "")) Commit(owner => SetProp(owner, text), 120);
        else Raise(nameof(TextValue));
    }

    public string ColorText
    {
        get => Raw() is Color c ? c.ToString() : "";
        set
        {
            try
            {
                var text = (value ?? "").Trim();
                if (text.Length > 0 && text[0] != '#') text = "#" + text;
                if (ColorConverter.ConvertFromString(text) is not Color c) return;
                if (Raw() is Color now && now == c) return;
                Commit(owner => SetProp(owner, c));
            }
            catch { Raise(nameof(ColorText)); }
        }
    }

    public Brush ColorBrush
    {
        get
        {
            var b = new SolidColorBrush(Raw() is Color c ? c : Colors.Transparent);
            b.Freeze();
            return b;
        }
    }

    /// <summary>YMM4側のいまの値を読み直して、画面に反映する</summary>
    public void Refresh()
    {
        if (IsShapeRow) RaiseShape3D();
        if (Kind == ItemRowKind.Anim) RaiseGraphMode();
        switch (Kind)
        {
            case ItemRowKind.Number:
            case ItemRowKind.Anim:
                NormalizeSlider();
                WidenSlider(Number(0));
                Raise(nameof(NumText)); Raise(nameof(SliderValue));
                Raise(nameof(IsAnimated)); Raise(nameof(AnimTypeIndex)); Raise(nameof(AnimButtonText));
                SyncPoints();
                break;
            case ItemRowKind.Bool: Raise(nameof(BoolValue)); break;
            case ItemRowKind.Enum: Raise(nameof(ChoiceIndex)); Raise(nameof(CurrentChoiceText)); break;
            case ItemRowKind.Color:
                Raise(nameof(ColorText)); Raise(nameof(ColorBrush));
                Raise(nameof(ColorR)); Raise(nameof(ColorG)); Raise(nameof(ColorB)); Raise(nameof(ColorA)); Raise(nameof(PadColor));
                break;
            default:
                // 日本語の変換中に入力欄の文字を書き換えると、変換が途切れるので、その間は読み直さない
                if (!composing) Raise(nameof(TextValue));
                if (Kind == ItemRowKind.Font) Raise(nameof(CurrentFontFamily));
                break;
        }
    }
}

/// <summary>移動の1点(始点・中間点・終点)</summary>
public sealed class ItemAnimPoint : NotifyBase
{
    readonly ItemPropRow row;
    readonly int index;
    public string Label { get; }

    public ItemAnimPoint(ItemPropRow row, int index, int count, int? frame = null)
    {
        this.row = row;
        this.index = index;
        Label = index == 0 ? "始点" : index == count - 1 ? "終点" : $"中間点{index}" + (frame != null ? $"({frame}f)" : "");
    }

    public string Text
    {
        get => row.PointText(index);
        set { row.SetPointText(index, value); Raise(nameof(Text)); }
    }

    ICommand? copyCommand, pasteCommand;
    /// <summary>この点の値をコピーする</summary>
    public ICommand CopyCommand => copyCommand ??= new RelayCommand(_ => row.CopyPoint(index, Label));
    /// <summary>コピーした数値を、この点へ貼り付ける</summary>
    public ICommand PasteCommand => pasteCommand ??= new RelayCommand(_ => row.PastePoint(index, Label));

    public double Min => row.SliderMin;
    public double Max => row.SliderMax;
    public string RangeText => row.RangeText;
    public void ReleaseSlider() => row.ReleaseSlider();
    public double SliderValue
    {
        get => Math.Clamp(row.PointNumber(index), row.SliderMin, row.SliderMax);
        set => row.SetPointNumber(index, value);
    }

    public void Refresh()
    {
        Raise(nameof(Min)); Raise(nameof(Max)); Raise(nameof(RangeText));
        Raise(nameof(Text)); Raise(nameof(SliderValue));
    }
}

/// <summary>動かし方を選ぶボタン1つ分(選ばれているものが光る。Points は、ボタンに描く小さな曲線)</summary>
public sealed class AnimChoiceChip : NotifyBase
{
    readonly Action<AnimChoiceChip> select;
    bool selected;
    public string Label { get; }
    public string Key { get; }
    public PointCollection? Points { get; }
    public bool HasCurve => Points != null;

    public AnimChoiceChip(string label, string key, Action<AnimChoiceChip> select, PointCollection? points)
    {
        Label = label;
        Key = key;
        this.select = select;
        Points = points;
    }

    public bool IsSelected
    {
        get => selected;
        set { if (value && !selected) select(this); else Raise(nameof(IsSelected)); }
    }

    public void SetSelected(bool value) { if (selected != value) { selected = value; Raise(nameof(IsSelected)); } }
}

/// <summary>フォント1つ分(一覧で、そのフォントを使って名前と見本を出す)</summary>
public sealed class FontChoice
{
    FontFamily? family;
    public string Name { get; }
    public FontChoice(string name) => Name = name;
    public FontFamily Family
    {
        get
        {
            if (family != null) return family;
            try { family = new FontFamily(Name); }
            catch { family = new FontFamily("Yu Gothic UI"); }
            return family;
        }
    }
}

/// <summary>よく使う動かし方のボタン1つ分</summary>
public sealed record AnimTypeChip(string Label, int Index);

/// <summary>追加できるエフェクト1つ分(エフェクトを選ぶ画面用)</summary>
public sealed class EffectChoice
{
    public string Name { get; }
    public List<string> Categories { get; }
    public string Category => Categories.Count > 0 ? Categories[0] : "その他";
    public bool IsPlugin { get; }
    public Type Type { get; }
    public string SourceText => IsPlugin ? "プラグイン" : "";
    public string Tip => $"{Name}({string.Join("・", Categories)}{(IsPlugin ? " / プラグイン" : "")})  クリックで追加";

    public EffectChoice(BuiltinEffect fx)
    {
        Name = YmmText.Resolve(fx.Name);
        Type = fx.Type;
        IsPlugin = !fx.IsBuiltIn;
        Categories = CategoriesOf(fx.Type);
        if (Categories.Count == 0) Categories.Add("その他");
    }

    /// <summary>YMM4のエフェクトの印(…EffectAttribute)に書かれている分類を読み、日本語の名前に直す</summary>
    static List<string> CategoriesOf(Type t)
    {
        var result = new List<string>();
        try
        {
            Type? resourceType = null;
            var raw = new List<string>();
            // まず、印そのものから読む(YMM4が日本語に直した名前を持っていれば、それを使う)
            foreach (var a in t.GetCustomAttributes(false))
            {
                var at = a.GetType();
                if (!at.Name.EndsWith("EffectAttribute", StringComparison.Ordinal)) continue;
                try { resourceType = at.GetProperty("ResourceType")?.GetValue(a) as Type; } catch { }
                foreach (var pn in new[] { "Categories", "Category", "Categorys", "CategoryNames" })
                {
                    object? v = null;
                    try { v = at.GetProperty(pn)?.GetValue(a); } catch { }
                    if (v is string one) raw.Add(one);
                    else if (v is IEnumerable many) foreach (var x in many) if (x is string text) raw.Add(text);
                    if (raw.Count > 0) break;
                }
            }
            // 読めなければ、印に書かれた引数(最初の文字の並び)から読む
            if (raw.Count == 0)
                foreach (var a in t.GetCustomAttributesData())
                {
                    if (!a.AttributeType.Name.EndsWith("EffectAttribute", StringComparison.Ordinal)) continue;
                    foreach (var arg in a.ConstructorArguments)
                        if (arg.Value is IReadOnlyCollection<CustomAttributeTypedArgument> list)
                        {
                            raw.AddRange(list.Select(x => x.Value as string ?? "").Where(x => x.Length > 0));
                            break;
                        }
                    foreach (var named in a.NamedArguments)
                        if (named.MemberName == "ResourceType" && named.TypedValue.Value is Type rt) resourceType = rt;
                }
            foreach (var r in raw)
            {
                var text = YmmText.Resolve(r, resourceType);
                if (!string.IsNullOrWhiteSpace(text) && !result.Contains(text)) result.Add(text);
            }
        }
        catch { }
        return result;
    }
}

/// <summary>エフェクトを選んで追加する画面(検索・分類・最近使ったもの)</summary>
public sealed class EffectPickerVm : NotifyBase
{
    const string AllCategories = "すべて";
    const string PluginCategory = "プラグイン";
    static readonly Dictionary<bool, List<EffectChoice>> catalog = new();
    static readonly Dictionary<bool, List<EffectChoice>> recentStore = new() { [true] = new(), [false] = new() };

    readonly ItemEditorVm host;
    readonly bool video;
    List<EffectChoice> all = new();
    bool isOpen;
    string searchText = "", category = AllCategories;

    public ObservableCollection<EffectChoice> Shown { get; } = new();
    public ObservableCollection<EffectChoice> Recent { get; } = new();
    public ObservableCollection<string> Categories { get; } = new();
    public ICommand PickCommand { get; }
    public ICommand CloseCommand { get; }

    public EffectPickerVm(ItemEditorVm host, bool video)
    {
        this.host = host;
        this.video = video;
        PickCommand = new RelayCommand(p => { if (p is EffectChoice c) Pick(c); });
        CloseCommand = new RelayCommand(_ => IsOpen = false);
    }

    public bool IsOpen
    {
        get => isOpen;
        set
        {
            if (!Set(ref isOpen, value) || !value) return;
            Load();
        }
    }

    public string SearchText { get => searchText; set { if (Set(ref searchText, value ?? "")) Filter(); } }
    public string Category { get => category; set { if (value != null && Set(ref category, value)) Filter(); } }
    public bool HasRecent => Recent.Count > 0;
    public string CountText => $"{Shown.Count} / {all.Count} 個";

    void Load()
    {
        try
        {
            if (!catalog.TryGetValue(video, out var list))
            {
                var found = BuiltinCatalog.FindEffects(video, out var info);
                if (found.Count == 0) host.Report(info);
                list = found.Select(f => new EffectChoice(f)).OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
                catalog[video] = list;
            }
            all = list;
            if (Categories.Count == 0)
            {
                Categories.Add(AllCategories);
                if (all.Any(x => x.IsPlugin)) Categories.Add(PluginCategory);
                foreach (var c in all.SelectMany(x => x.Categories).Distinct().OrderBy(x => x, StringComparer.CurrentCultureIgnoreCase)) Categories.Add(c);
            }
            SyncRecent();
            Filter();
        }
        catch (Exception ex) { host.Report("エフェクトの一覧を作れませんでした: " + ex.GetBaseException().Message); }
    }

    void SyncRecent()
    {
        Recent.Clear();
        foreach (var c in recentStore[video]) Recent.Add(c);
        Raise(nameof(HasRecent));
    }

    void Filter()
    {
        var s = searchText.Trim();
        Shown.Clear();
        foreach (var c in all)
        {
            if (category == PluginCategory) { if (!c.IsPlugin) continue; }
            else if (category != AllCategories && !c.Categories.Contains(category)) continue;
            // 名前・分類のほか、内部タグ(読み・英語・言い換え)でもヒットする
            if (s.Length > 0 && !SearchTags.Matches(s, c.Type.FullName ?? c.Type.Name, c.Name, string.Join(" ", c.Categories))) continue;
            Shown.Add(c);
        }
        Raise(nameof(CountText));
    }

    void Pick(EffectChoice c)
    {
        var recent = recentStore[video];
        recent.Remove(c);
        recent.Insert(0, c);
        if (recent.Count > 8) recent.RemoveRange(8, recent.Count - 8);
        SyncRecent();
        host.AddEffect(video, c.Type, c.Name);
    }
}

/// <summary>図形の種類1つ分(図形の選択画面の1つのボタン)</summary>
public sealed class ShapeChoice
{
    public string Key { get; init; } = "";
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string SourceText { get; init; } = "";
    public int Order { get; init; }
    public bool IsCurrent { get; init; }
    public string Tip => $"{Name}\n分類: {Category}" + (SourceText.Length > 0 ? $"\n{SourceText}" : "");
}

/// <summary>図形の種類を選ぶ画面(分類・検索・最近使ったもの)。エフェクトの追加画面と同じ見た目</summary>
public sealed class ShapePickerVm : NotifyBase
{
    const string AllCategories = "すべて";
    static readonly List<string> recentKeys = new();

    readonly ItemPropRow row;
    List<ShapeChoice> all = new();
    bool isOpen;
    string searchText = "", category = AllCategories;

    public ObservableCollection<ShapeChoice> Shown { get; } = new();
    public ObservableCollection<ShapeChoice> Recent { get; } = new();
    public ObservableCollection<string> Categories { get; } = new();
    public ICommand PickCommand { get; }
    public ICommand CloseCommand { get; }

    public ShapePickerVm(ItemPropRow row)
    {
        this.row = row;
        PickCommand = new RelayCommand(p => { if (p is ShapeChoice c) Pick(c); });
        CloseCommand = new RelayCommand(_ => IsOpen = false);
    }

    public bool IsOpen
    {
        get => isOpen;
        set
        {
            if (!Set(ref isOpen, value) || !value) return;
            // 開いた時は、いまの図形に合わせて 2D/3D を選ぶ
            var cur = row.CurrentChoiceKey;
            show3D = cur != null && YMM4EffectShapes.Solid3D.Solid3DDefaults.Is3DKey(cur);
            Raise(nameof(Show3D)); Raise(nameof(Show2D));
            Load();
        }
    }

    public string SearchText { get => searchText; set { if (Set(ref searchText, value ?? "")) Filter(); } }
    public string Category { get => category; set { if (value != null && Set(ref category, value)) Filter(); } }
    public bool HasRecent => Recent.Count > 0;
    public string CountText => $"{Shown.Count} / {all.Count} 種類";

    bool show3D;
    /// <summary>同梱の演出図形を、3D版で出すか(オフ=2D版)。標準・他のプラグインの図形は、どちらでも出る</summary>
    public bool Show3D { get => show3D; set { if (Set(ref show3D, value)) { Raise(nameof(Show2D)); Load(); } RaiseLater(); } }
    public bool Show2D { get => !show3D; set { if (value) Show3D = false; RaiseLater(); } }
    void RaiseLater() => System.Windows.Application.Current?.Dispatcher.BeginInvoke(new Action(() => { Raise(nameof(Show3D)); Raise(nameof(Show2D)); }));

    void Load()
    {
        var keys = row.ChoiceKeys;
        var names = row.Choices;
        var cur = row.CurrentChoiceKey;
        var found = ShapeDiscovery.Found.Count > 0 ? ShapeDiscovery.Found : ShapeDiscovery.Scan();
        var list = new List<ShapeChoice>();
        for (int i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            // 演出図形は、2D/3D の切り替えに合った方だけを出す
            if (key.StartsWith("YMM4EffectShapes.", StringComparison.Ordinal) && YMM4EffectShapes.Solid3D.Solid3DDefaults.Is3DKey(key) != show3D) continue;
            // 設定の「図形の表示」で外した図形は出さない(いま使っている図形だけは出す)
            if (key != cur && !ExplorerSettings.Instance.IsShapeShown(key)) continue;
            var info = found.FirstOrDefault(f => f.Key == key);
            string cat; int order;
            if (ShapeCategories.TryGet(key, out var c, out var o)) { cat = c; order = 1000 + o; }
            else if (info?.IsBuiltIn ?? false) { cat = ShapeCategories.Standard; order = i; }
            else { cat = ShapeCategories.OtherPlugins; order = 100000 + i; }
            var name = (info?.Name ?? (i < names.Count ? names[i] : key));
            if (name.StartsWith("演出：", StringComparison.Ordinal)) name = name.Substring(3);
            else if (name.StartsWith("演出3D：", StringComparison.Ordinal)) name = name.Substring(5);
            list.Add(new ShapeChoice { Key = key, Name = name, Category = cat, Order = order, IsCurrent = key == cur,
                                       SourceText = info == null || info.IsBuiltIn ? "" : info.Source });
        }
        all = list.OrderBy(x => CatIndex(x.Category)).ThenBy(x => x.Order).ToList();
        Categories.Clear();
        Categories.Add(AllCategories);
        foreach (var c2 in ShapeCategories.Order) if (all.Any(x => x.Category == c2)) Categories.Add(c2);
        foreach (var c3 in all.Select(x => x.Category).Distinct()) if (!Categories.Contains(c3)) Categories.Add(c3);
        // いまの図形の分類を最初に開く
        var curChoice = all.FirstOrDefault(x => x.IsCurrent);
        category = curChoice != null && searchText.Length == 0 ? curChoice.Category : category;
        if (!Categories.Contains(category)) category = AllCategories;
        Raise(nameof(Category));
        SyncRecent();
        Filter();
    }

    static int CatIndex(string c)
    {
        int i = Array.IndexOf(ShapeCategories.Order, c);
        return i < 0 ? 999 : i;
    }

    void SyncRecent()
    {
        Recent.Clear();
        foreach (var k in recentKeys) { var c = all.FirstOrDefault(x => x.Key == k); if (c != null) Recent.Add(c); }
        Raise(nameof(HasRecent));
    }

    void Filter()
    {
        var s = searchText.Trim();
        Shown.Clear();
        foreach (var c in all)
        {
            if (s.Length == 0 && category != AllCategories && c.Category != category) continue;
            // 名前・分類のほか、内部タグ(読み・英語・言い換え)でもヒットする
            if (s.Length > 0 && !SearchTags.Matches(s, c.Key, c.Name, c.Category)) continue;
            Shown.Add(c);
        }
        Raise(nameof(CountText));
    }

    void Pick(ShapeChoice c)
    {
        recentKeys.Remove(c.Key);
        recentKeys.Insert(0, c.Key);
        if (recentKeys.Count > 8) recentKeys.RemoveRange(8, recentKeys.Count - 8);
        IsOpen = false;
        row.PickChoiceKey(c.Key);
    }
}

/// <summary>項目のグループ(YMM4の「描画」「テキスト」などのまとまり)</summary>
public sealed class ItemPropGroup : NotifyBase
{
    bool isOpen = true, hasVisible = true, isHidden, layoutEdit;
    ICommand? up, down, hide;
    public string Name { get; }
    public List<ItemPropRow> Rows { get; } = new();
    public ItemEditorVm? Host { get; set; }
    public bool IsOpen { get => isOpen; set => Set(ref isOpen, value); }
    public bool HasVisible { get => hasVisible; set => Set(ref hasVisible, value); }
    /// <summary>非表示にしたカード(レイアウトの編集中だけ、薄く出る)</summary>
    public bool IsHidden { get => isHidden; set { if (Set(ref isHidden, value)) { Raise(nameof(HideText)); Raise(nameof(CardOpacity)); } } }
    public bool LayoutEdit { get => layoutEdit; set => Set(ref layoutEdit, value); }
    public string HideText => isHidden ? "表示する" : "非表示にする";
    public double CardOpacity => isHidden ? 0.45 : 1;
    public ICommand MoveUpCommand => up ??= new RelayCommand(_ => Host?.MoveGroup(this, -1));
    public ICommand MoveDownCommand => down ??= new RelayCommand(_ => Host?.MoveGroup(this, 1));
    public ICommand ToggleHiddenCommand => hide ??= new RelayCommand(_ => Host?.ToggleGroupHidden(this));
    public ItemPropGroup(string name) => Name = name;
}

/// <summary>アイテムに付いているエフェクト1つ分(オン・オフ、並べ替え、削除、中の項目の編集)</summary>
public sealed class ItemEffectCard : NotifyBase
{
    readonly ItemEditorVm host;
    bool isOpen;

    public bool IsVideo { get; }
    public int Index { get; }
    public string Title { get; }
    public List<ItemPropRow> Rows { get; } = new();
    public bool HasRows => Rows.Count > 0;
    public string RowsText => Rows.Count == 0 ? "(変更できる項目はありません)" : $"{Rows.Count} 項目";
    public bool IsOpen { get => isOpen; set => Set(ref isOpen, value); }

    public ICommand MoveUpCommand { get; }
    public ICommand MoveDownCommand { get; }
    public ICommand RemoveCommand { get; }

    public ItemEffectCard(ItemEditorVm host, bool video, int index, string title, bool open)
    {
        this.host = host;
        IsVideo = video;
        Index = index;
        Title = title;
        isOpen = open;
        MoveUpCommand = new RelayCommand(_ => host.MoveEffect(this, -1));
        MoveDownCommand = new RelayCommand(_ => host.MoveEffect(this, 1));
        RemoveCommand = new RelayCommand(_ => host.RemoveEffect(this));
    }

    public bool IsEnabled
    {
        get => host.GetEffectEnabled(this);
        set { if (value != IsEnabled) host.SetEffectEnabled(this, value); }
    }

    public void Refresh()
    {
        Raise(nameof(IsEnabled));
        foreach (var r in Rows) r.Refresh();
    }
}

/// <summary>
/// アイテムタブ: YMM4のタイムラインで選択中のアイテムを、このプラグインの画面から編集する。
/// YMM4が項目に付けている表示名(Display属性)を読み取って、項目の一覧を自動で作るので、
/// テキスト・画像・動画・図形・立ち絵など、どの種類のアイテムにも、プラグインで追加されたエフェクトにも対応する。
/// </summary>
public sealed class ItemEditorVm : NotifyBase
{
    static readonly HashSet<string> QuickNames = new()
        { "Text", "Serif", "CharacterName", "X", "Y", "Zoom", "Opacity", "Rotation", "FontSize", "FontColor", "Volume", "PlaybackRate" };

    static readonly Dictionary<string, string> TypeLabels = new()
    {
        ["TextItem"] = "テキスト", ["VoiceItem"] = "ボイス", ["VideoItem"] = "動画", ["AudioItem"] = "音声",
        ["ImageItem"] = "画像", ["ShapeItem"] = "図形", ["TachieItem"] = "立ち絵", ["TachieFaceItem"] = "表情",
        ["EffectItem"] = "エフェクト", ["TransitionItem"] = "場面切り替え", ["GroupItem"] = "グループ",
        ["FrameBufferItem"] = "画面の複製", ["SceneItem"] = "シーン",
    };

    readonly Func<bool> isActive;

    /// <summary>この画面の入力欄で入力中か(画面側から教えてもらう)。入力中は、値を読み直さない</summary>
    public Func<bool>? IsEditing { get; set; }
    readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(400) };
    readonly Dictionary<string, bool> groupOpen = new();
    readonly HashSet<string> effectOpen = new();
    List<object> roots = new();
    List<object> signature = new();

    public ObservableCollection<ItemPropRow> QuickRows { get; } = new();

    ShapeStageVm? shapeStage;
    /// <summary>配置画面を作るための材料(図形の設定を並べた時に覚えておき、一覧がそろってから作る)</summary>
    (string? type, string title, List<ItemPropRow> rows)? pendingStage;
    /// <summary>図形アイテムの配置画面(位置・向き・長さなどを、画面の上のつまみで決める)。出せない図形では null</summary>
    public ShapeStageVm? ShapeStage
    {
        get => shapeStage;
        private set
        {
            var old = shapeStage;
            if (Set(ref shapeStage, value)) { old?.Dispose(); Raise(nameof(HasShapeStage)); }
        }
    }
    public bool HasShapeStage => shapeStage != null;
    public ObservableCollection<ItemPropGroup> Groups { get; } = new();
    public ObservableCollection<ItemEffectCard> VideoEffects { get; } = new();
    public ObservableCollection<ItemEffectCard> AudioEffects { get; } = new();
    public EffectPickerVm VideoPicker { get; }
    public EffectPickerVm AudioPicker { get; }

    public ICommand ReloadCommand { get; }
    public ICommand CloseGraphCommand { get; }
    public ICommand ResetLayoutCommand { get; }
    public ICommand OpenAllCommand { get; }
    public ICommand CloseAllCommand { get; }

    public ItemEditorVm(Func<bool> isActive)
    {
        this.isActive = isActive;
        ReloadCommand = new RelayCommand(_ => Rebuild());
        CloseGraphCommand = new RelayCommand(_ => CloseGraph());
        ResetLayoutCommand = new RelayCommand(_ => ResetLayout());
        liveTimer.Tick += async (_, _) => await FlushLiveAsync();
        VideoPicker = new EffectPickerVm(this, true);
        AudioPicker = new EffectPickerVm(this, false);
        OpenAllCommand = new RelayCommand(_ => SetAllOpen(true));
        CloseAllCommand = new RelayCommand(_ => SetAllOpen(false));
        timer.Tick += (_, _) => Poll();
        timer.Start();
    }

    public void Dispose() => timer.Stop();

    // ------------------------------------------------------------------ 画面に出す文字
    string title = "", subText = "", message = "", filterText = "";
    bool hasItem;
    public string Title { get => title; private set => Set(ref title, value); }
    public string SubText { get => subText; private set => Set(ref subText, value); }
    public string Message { get => message; private set => Set(ref message, value); }
    public bool HasItem { get => hasItem; private set { if (Set(ref hasItem, value)) Raise(nameof(HasNoItem)); } }
    public bool HasNoItem => !hasItem;
    public bool HasQuick => QuickRows.Count > 0;
    /// <summary>選択中の先頭が、テキストのアイテムか(テキスト分割のボタンを出す)</summary>
    public bool HasText => roots.Count > 0 && (Ymm.FindProperty(roots[0].GetType(), "Text")?.PropertyType == typeof(string)
                                               || Ymm.FindProperty(roots[0].GetType(), "Serif")?.PropertyType == typeof(string));
    public bool HasVideoEffectList { get; private set; }
    public bool HasAudioEffectList { get; private set; }
    public string VideoEffectHeader => $"映像エフェクト({VideoEffects.Count})";
    public string AudioEffectHeader => $"音声エフェクト({AudioEffects.Count})";

    public string FilterText
    {
        get => filterText;
        set { if (Set(ref filterText, value ?? "")) ApplyFilter(); }
    }

    // ------------------------------------------------------------------ なめらかな書き込み(動かしている間は、まとめて1回分の編集にする)
    readonly HashSet<object> liveRoots = new(ReferenceEqualityComparer.Instance);
    readonly DispatcherTimer liveTimer = new() { Interval = TimeSpan.FromMilliseconds(350) };

    /// <summary>
    /// アイテムへ書き込む。最初の書き込みで編集を始め(BeginEdit)、書き込みが 0.35 秒止まったら編集を終える(EndEdit)。
    /// スライダーを動かしている間、毎回編集を終えないので、YMM4のプレビューがなめらかに動き、「元に戻す」も1回で済む。
    /// </summary>
    public void LiveEdit(object root, Action apply, int idleMs = 350)
    {
        if (liveRoots.Add(root)) Ymm.BeginEdit(root);
        apply();
        liveTimer.Stop();
        liveTimer.Interval = TimeSpan.FromMilliseconds(idleMs);
        liveTimer.Start();
    }

    /// <summary>続けて書き込み中の編集を、いま終える</summary>
    public async Task FlushLiveAsync()
    {
        liveTimer.Stop();
        if (liveRoots.Count == 0) return;
        var list = liveRoots.ToList();
        liveRoots.Clear();
        foreach (var r in list) await Ymm.EndEditAsync(r);
    }

    // ------------------------------------------------------------------ レイアウト(カードの並び・表示・よく使う項目・列の幅)
    bool layoutEdit;
    /// <summary>レイアウトの編集中(カードに、並べ替え・非表示のボタンを出す)</summary>
    /// <summary>タイムラインでアイテムをダブルクリックした時に、このタブを開く(YMM4標準のアイテム編集の代わりに)</summary>
    public bool OpenOnDoubleClick
    {
        get => ExplorerSettings.Instance.OpenItemTabOnDoubleClick;
        set { ExplorerSettings.Instance.OpenItemTabOnDoubleClick = value; ExplorerSettings.Instance.Save(); Raise(); }
    }

    public bool LayoutEdit
    {
        get => layoutEdit;
        set { if (Set(ref layoutEdit, value)) Rebuild(); }
    }

    public double ColumnWidth
    {
        get => Math.Clamp(ExplorerSettings.Instance.ItemColumnWidth, 260, 900);
        set
        {
            value = Math.Clamp(Math.Round(value / 10) * 10, 260, 900);
            if (Math.Abs(value - ExplorerSettings.Instance.ItemColumnWidth) < 0.5) return;
            ExplorerSettings.Instance.ItemColumnWidth = value;
            ExplorerSettings.Instance.Save();
            Raise(nameof(ColumnWidth)); Raise(nameof(ColumnWidthText));
        }
    }
    public string ColumnWidthText => $"{ColumnWidth:0} px";

    /// <summary>以前の保存の形(よく使う項目の全部の一覧)を、「足したもの」「外したもの」の形へ直す</summary>
    static void MigrateQuick()
    {
        var st = ExplorerSettings.Instance;
        if (st.ItemQuickProps == null) return;
        st.ItemQuickAdded = st.ItemQuickProps.Where(n => !QuickNames.Contains(n)).ToList();
        st.ItemQuickRemoved = QuickNames.Where(n => !st.ItemQuickProps.Contains(n)).ToList();
        st.ItemQuickProps = null;
        st.Save();
    }

    /// <summary>最初から「よく使う項目」に入れておく項目か(ボイスの、字幕のフォント・字幕を表示するかどうか も入れる)</summary>
    static bool DefaultQuick(ItemPropRow row)
    {
        var n = row.PropName;
        if (QuickNames.Contains(n)) return true;
        bool jimaku = n.Contains("Jimaku", StringComparison.OrdinalIgnoreCase) || row.Label.Contains("字幕") || row.Group.Contains("字幕");
        if (!jimaku) return false;
        if (row.Kind == ItemRowKind.Font) return true;
        if (n.Contains("Visib", StringComparison.OrdinalIgnoreCase) || n.Contains("Show", StringComparison.OrdinalIgnoreCase)
            || n.Contains("Display", StringComparison.OrdinalIgnoreCase) || n.Contains("Hide", StringComparison.OrdinalIgnoreCase)
            || ((row.Kind == ItemRowKind.Bool || row.Kind == ItemRowKind.Enum) && row.Label.Contains("表示")))
            return row.Kind == ItemRowKind.Bool || row.Kind == ItemRowKind.Enum;
        return false;
    }

    public bool IsQuick(ItemPropRow row)
    {
        MigrateQuick();
        var st = ExplorerSettings.Instance;
        if (st.ItemQuickRemoved?.Contains(row.PropName) == true) return false;
        return DefaultQuick(row) || st.ItemQuickAdded?.Contains(row.PropName) == true;
    }

    /// <summary>項目を「よく使う項目」に入れる・外す</summary>
    public void ToggleQuick(ItemPropRow row)
    {
        MigrateQuick();
        var st = ExplorerSettings.Instance;
        var added = st.ItemQuickAdded ?? new List<string>();
        var removed = st.ItemQuickRemoved ?? new List<string>();
        var n = row.PropName;
        if (IsQuick(row))
        {
            added.Remove(n);
            if (DefaultQuick(row) && !removed.Contains(n)) removed.Add(n);
        }
        else
        {
            removed.Remove(n);
            if (!DefaultQuick(row) && !added.Contains(n)) added.Add(n);
        }
        st.ItemQuickAdded = added;
        st.ItemQuickRemoved = removed;
        st.Save();
        Rebuild();
    }

    public void MoveGroup(ItemPropGroup g, int delta)
    {
        var names = Groups.Select(x => x.Name).ToList();
        int i = names.IndexOf(g.Name), to = i + delta;
        if (i < 0 || to < 0 || to >= names.Count) return;
        (names[i], names[to]) = (names[to], names[i]);
        // 保存してある順番に、いま出ていないグループ(ほかの種類のアイテムのもの)も残す
        var order = ExplorerSettings.Instance.ItemGroupOrder ?? new List<string>();
        var rest = order.Where(n => !names.Contains(n)).ToList();
        ExplorerSettings.Instance.ItemGroupOrder = names.Concat(rest).ToList();
        ExplorerSettings.Instance.Save();
        Rebuild();
    }

    public void ToggleGroupHidden(ItemPropGroup g)
    {
        var hidden = ExplorerSettings.Instance.ItemHiddenGroups ?? new List<string>();
        if (!hidden.Remove(g.Name)) hidden.Add(g.Name);
        ExplorerSettings.Instance.ItemHiddenGroups = hidden;
        ExplorerSettings.Instance.Save();
        Rebuild();
    }

    void ResetLayout()
    {
        ExplorerSettings.Instance.ItemGroupOrder = null;
        ExplorerSettings.Instance.ItemHiddenGroups = null;
        ExplorerSettings.Instance.ItemQuickProps = null;
        ExplorerSettings.Instance.ItemQuickAdded = null;
        ExplorerSettings.Instance.ItemQuickRemoved = null;
        ExplorerSettings.Instance.ItemColumnWidth = 430;
        ExplorerSettings.Instance.Save();
        Raise(nameof(ColumnWidth)); Raise(nameof(ColumnWidthText));
        Rebuild();
        Say("レイアウトを、最初の状態に戻しました");
    }

    // ------------------------------------------------------------------ グラフを大きく開く
    ItemPropRow? graphRow;
    public ItemPropRow? GraphRow { get => graphRow; private set { if (Set(ref graphRow, value)) { Raise(nameof(GraphOpen)); Raise(nameof(GraphTitle)); } } }
    public bool GraphOpen => graphRow != null;
    public string GraphTitle => graphRow == null ? "" : $"{graphRow.Group} ― {graphRow.Label} の移動";
    public void ShowGraph(ItemPropRow row) => GraphRow = row;
    public void CloseGraph() => GraphRow = null;

    public void Report(string text) => Message = "⚠ " + text;
    public void Say(string text) => Message = text;

    // ------------------------------------------------------------------ 選択の見張り
    void Poll()
    {
        if (!isActive()) return;
        try
        {
            var now = Ymm.SelectedItems();
            var sig = Signature(now);
            // 入力中・スライダーを動かしている間は、作り直さず・値も読み直さない(入力が消えてしまうため)
            bool busy = IsEditing?.Invoke() == true || Mouse.Captured != null;
            if (!sig.SequenceEqual(signature, ReferenceEqualityComparer.Instance))
            {
                // 選択が変わった時は、すぐ作り直す。図形の設定が入れ替わっただけなら、操作が終わってから
                if (!busy || !now.SequenceEqual(roots, ReferenceEqualityComparer.Instance)) Build(now, sig);
                return;
            }
            if (busy) return;
            RefreshValues();
        }
        catch { }
    }

    /// <summary>「選択が変わったか・エフェクトの並びが変わったか」を見分けるための一覧</summary>
    static List<object> Signature(List<object> items)
    {
        var sig = new List<object>(items);
        if (items.Count > 0)
        {
            foreach (var prop in new[] { "VideoEffects", "AudioEffects" })
                if (Ymm.Get(items[0], prop) is IEnumerable e)
                    foreach (var x in e) if (x != null) sig.Add(x);
            // 図形の設定が入れ替わったら(図形の種類を変えた・元に戻した時など)、一覧を作り直す
            foreach (var item in items)
                try { if (ShapeParamProp(item.GetType())?.GetValue(item) is { } sp) sig.Add(sp); } catch { }
        }
        return sig;
    }

    public void Rebuild()
    {
        try
        {
            var now = Ymm.SelectedItems();
            Build(now, Signature(now));
        }
        catch (Exception ex) { Report(ex.GetBaseException().Message); }
    }

    void RefreshValues()
    {
        foreach (var g in Groups) foreach (var r in g.Rows) r.Refresh();
        foreach (var c in VideoEffects) c.Refresh();
        foreach (var c in AudioEffects) c.Refresh();
        shapeStage?.Bump();
        UpdateHeader();
    }

    // ------------------------------------------------------------------ 一覧を作る
    void Build(List<object> items, List<object> sig)
    {
        var graphKey = graphRow == null ? null : graphRow.Group + "\n" + graphRow.PropName + "\n" + graphRow.Label;
        foreach (var g in Groups) groupOpen[g.Name] = g.IsOpen;
        effectOpen.Clear();
        foreach (var c in VideoEffects.Concat(AudioEffects)) if (c.IsOpen) effectOpen.Add((c.IsVideo ? "v" : "a") + c.Index + c.Title);

        roots = items;
        signature = sig;
        pendingStage = null;
        QuickRows.Clear();
        Groups.Clear();
        VideoEffects.Clear();
        AudioEffects.Clear();
        Message = "";
        HasItem = items.Count > 0;
        HasVideoEffectList = false;
        HasAudioEffectList = false;

        var allRows = new List<ItemPropRow>();
        if (items.Count > 0)
        {
            var rows = allRows;
            BuildRows(items, r => r, "その他", 0, rows);
            foreach (var row in rows)
            {
                var g = Groups.FirstOrDefault(x => x.Name == row.Group);
                if (g == null)
                {
                    g = new ItemPropGroup(row.Group);
                    if (groupOpen.TryGetValue(g.Name, out var open)) g.IsOpen = open;
                    Groups.Add(g);
                }
                g.Rows.Add(row);
                if (IsQuick(row) && !QuickRows.Any(q => q.PropName == row.PropName)) QuickRows.Add(row);
            }
            // 保存してある並び順・非表示を反映する(編集中は、非表示のカードも薄く出す)
            var order = ExplorerSettings.Instance.ItemGroupOrder ?? new List<string>();
            var hidden = ExplorerSettings.Instance.ItemHiddenGroups ?? new List<string>();
            var sorted = Groups.Select((g, i) => (g, i))
                .OrderBy(x => { int k = order.IndexOf(x.g.Name); return k < 0 ? int.MaxValue : k; }).ThenBy(x => x.i).Select(x => x.g).ToList();
            Groups.Clear();
            foreach (var g in sorted)
            {
                g.IsHidden = hidden.Contains(g.Name);
                g.LayoutEdit = layoutEdit;
                g.Host = this;
                if (!g.IsHidden || layoutEdit) Groups.Add(g);
            }
            BuildEffects(items, true);
            BuildEffects(items, false);
        }
        ShapeStageVm? stage = null;
        if (pendingStage is { } ps && items.Count > 0)
        {
            try
            {
                var shapeSet = new HashSet<ItemPropRow>(ps.rows);
                var itemLevel = allRows.Where(r => !shapeSet.Contains(r)).ToList();
                stage = ShapeStageVm.TryCreate(this, ps.type, ps.title, ps.rows, itemLevel, shapeStage);
            }
            catch { stage = null; }
        }
        ShapeStage = stage;
        pendingStage = null;

        if (graphKey != null)
        {
            // 一覧を作り直した時は、大きく開いていたグラフの項目を、新しい一覧から探し直す(無くなっていたら閉じる)
            var again = Groups.SelectMany(g => g.Rows).Concat(VideoEffects.SelectMany(c => c.Rows)).Concat(AudioEffects.SelectMany(c => c.Rows))
                              .FirstOrDefault(r => r.IsAnim && r.Group + "\n" + r.PropName + "\n" + r.Label == graphKey);
            GraphRow = again;
            if (again != null) again.AnimOpen = true;
        }
        UpdateHeader();
        ApplyFilter();
        foreach (var n in new[] { nameof(HasText), nameof(HasQuick), nameof(HasVideoEffectList), nameof(HasAudioEffectList), nameof(VideoEffectHeader), nameof(AudioEffectHeader) })
            Raise(n);
        CommandManager.InvalidateRequerySuggested();
    }

    void UpdateHeader()
    {
        if (roots.Count == 0)
        {
            Title = "";
            SubText = "";
            return;
        }
        var first = roots[0];
        var typeName = first.GetType().Name;
        var label = TypeLabels.TryGetValue(typeName, out var l) ? l : typeName.Replace("Item", "");
        var text = (Ymm.Get(first, "Text") ?? Ymm.Get(first, "Serif")) as string;
        if (!string.IsNullOrWhiteSpace(text))
        {
            var line = text!.Replace("\r", "").Split('\n')[0];
            label += $":「{(line.Length > 24 ? line.Substring(0, 24) + "…" : line)}」";
        }
        Title = label;

        var parts = new List<string>();
        if (roots.Count > 1) parts.Add($"{roots.Count} 個を選択中(同じ項目は、まとめて変わります)");
        try
        {
            int frame = Convert.ToInt32(Ymm.Get(first, "Frame") ?? 0), length = Convert.ToInt32(Ymm.Get(first, "Length") ?? 0);
            int layer = Convert.ToInt32(Ymm.Get(first, "Layer") ?? 0);
            parts.Add($"レイヤー {layer + 1}");
            parts.Add($"{frame} フレームから {length} フレーム分");
        }
        catch { }
        SubText = string.Join(" / ", parts);
    }

    static string? Safe(Func<string?> f)
    {
        try { var s = f(); return string.IsNullOrWhiteSpace(s) ? null : s; }
        catch { return null; }
    }

    static bool IsNumberType(Type t) =>
        t == typeof(double) || t == typeof(float) || t == typeof(int) || t == typeof(long) || t == typeof(decimal);

    /// <summary>スライダーの印(…SliderAttribute)から、表示の形・単位・範囲を読む</summary>
    /// <summary>
    /// 改行を入れられる入力欄にするか。YMM4の TextEditor(AcceptsReturn = true) が付いている項目や、
    /// 名前・説明に「1行に」「改行」とある項目(コメント・選択肢など、1行に1つずつ書く項目)
    /// </summary>
    static bool WantsMultiline(PropertyInfo p, string label, string tip)
    {
        try
        {
            foreach (var a in p.GetCustomAttributesData())
            {
                if (!a.AttributeType.Name.StartsWith("TextEditor", StringComparison.Ordinal)) continue;
                foreach (var na in a.NamedArguments)
                    if (na.MemberName == "AcceptsReturn" && na.TypedValue.Value is bool v) return v;
                foreach (var ca in a.ConstructorArguments)
                    if (ca.Value is bool v2) return v2;
            }
        }
        catch { }
        foreach (var s in new[] { label, tip })
            if (s.Contains("1行に", StringComparison.Ordinal) || s.Contains("１行に", StringComparison.Ordinal) || s.Contains("改行", StringComparison.Ordinal))
                return true;
        return false;
    }

    static void SliderInfo(PropertyInfo p, out string format, out string unit, out double? min, out double? max)
    {
        format = "0.##"; unit = ""; min = null; max = null;
        try
        {
            foreach (var a in p.GetCustomAttributesData())
            {
                if (!a.AttributeType.Name.Contains("Slider", StringComparison.Ordinal)) continue;
                var strings = a.ConstructorArguments.Where(x => x.ArgumentType == typeof(string)).Select(x => x.Value as string ?? "").ToList();
                var numbers = a.ConstructorArguments
                    .Where(x => x.Value != null && (x.ArgumentType == typeof(double) || x.ArgumentType == typeof(float) || x.ArgumentType == typeof(int)))
                    .Select(x => Convert.ToDouble(x.Value, CultureInfo.InvariantCulture)).ToList();
                if (strings.Count > 0 && strings[0].Length > 0)
                {
                    try { _ = 0.0.ToString(strings[0], CultureInfo.InvariantCulture); format = strings[0]; }
                    catch { }
                }
                if (strings.Count > 1) unit = strings[1];
                if (numbers.Count >= 2 && numbers[1] > numbers[0]) { min = numbers[0]; max = numbers[1]; }
                return;
            }
        }
        catch { }
    }

    /// <summary>
    /// owner(アイテム・エフェクト・図形のパラメータなど)が持つ項目のうち、YMM4が表示名を付けているものを行にする。
    /// resolve は「選択中のアイテム1つ」から、その中の owner を取り出す方法。
    /// </summary>
    void BuildRows(IReadOnlyList<object> items, Func<object, object?> resolve, string defaultGroup, int depth, List<ItemPropRow> rows, bool looseLabels = false)
    {
        var primary = resolve(items[0]);
        if (primary == null) return;
        PropertyInfo[] props;
        try { props = primary.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance); }
        catch { return; }

        // 図形アイテム: 図形の種類と、その図形だけの設定(ShapeParameter)を、専用に並べる
        var handled = new HashSet<string>();
        if (depth == 0 && ShapeParamProp(primary.GetType()) is { } spp)
        {
            AddShapeRows(items, resolve, primary, spp, rows);
            handled.Add(spp.Name);
            foreach (var p in props) if (p.Name.Contains("ShapeType", StringComparison.Ordinal)) handled.Add(p.Name);
        }

        foreach (var p in props)
        {
            try
            {
                if (p.GetIndexParameters().Length != 0 || !p.CanRead) continue;
                var name = p.Name;
                if (handled.Contains(name)) continue;
                if (name == "VideoEffects" || name == "AudioEffects") continue;
                // 図形の種類(四角形・円・星など): YMM4では専用の部品で選ぶ項目なので、ここで選択肢を用意する
                if ((name == "ShapeType2" || name == "ShapeType") && p.PropertyType == typeof(string) && p.GetSetMethod(true) != null)
                {
                    var shapes = ShapeChoices();
                    if (shapes.Count > 0)
                    {
                        var shapeTargets = new List<(object root, object owner)>();
                        foreach (var item in items)
                        {
                            var o = resolve(item);
                            if (o != null && Ymm.FindProperty(o.GetType(), name) != null) shapeTargets.Add((item, o));
                        }
                        if (shapeTargets.Count > 0)
                            rows.Add(new ItemPropRow(this, ItemRowKind.Enum, name, "図形の種類", "図形の設定",
                                "図形の形を選びます(変えると、その図形の設定項目に切り替わります)", shapeTargets, "0.##", "", null, null, null, shapes) { Resolver = resolve, IsShapeRow = true });
                    }
                    continue;
                }
                // ボイスのキャラクター: 名前を打ち込むのではなく、YMM4に登録してあるキャラクターから選ぶ
                if (name == "CharacterName" && p.PropertyType == typeof(string) && p.GetSetMethod(true) != null)
                {
                    var characters = CharacterChoices(p.GetValue(primary) as string);
                    if (characters.Count > 0)
                    {
                        var who = new List<(object root, object owner)>();
                        foreach (var item in items)
                        {
                            var o = resolve(item);
                            if (o != null && Ymm.FindProperty(o.GetType(), name) != null) who.Add((item, o));
                        }
                        var shown = p.GetCustomAttribute<DisplayAttribute>(true);
                        var groupName = YmmText.Resolve(Safe(() => shown?.GetGroupName()) ?? defaultGroup);
                        if (who.Count > 0)
                            rows.Add(new ItemPropRow(this, ItemRowKind.Enum, name, "キャラクター", groupName,
                                "このボイスを話すキャラクターを切り替えます", who, "0.##", "", null, null, null, characters) { Resolver = resolve });
                        continue;
                    }
                }
                var display = p.GetCustomAttribute<DisplayAttribute>(true);
                if (display == null && looseLabels && p.CanWrite && p.GetSetMethod() != null)
                {
                    // 図形の設定などで、表示名が付いていない項目も出す(名前は、項目名から)
                    var t0 = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                    if (t0.Name == "Animation" || t0 == typeof(bool) || t0.IsEnum || t0 == typeof(Color) || IsNumberType(t0))
                        display = new DisplayAttribute { Name = YmmText.Resolve(name), GroupName = defaultGroup };
                }
                else if (display == null && looseLabels && (Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType).Name == "Animation")
                    display = new DisplayAttribute { Name = YmmText.Resolve(name), GroupName = defaultGroup };
                if (display == null)
                {
                    // 表示名の無い入れ物(図形の設定 ShapeParameter など): 中に表示名つきの項目があれば、それを並べる
                    if (depth < 4 && IsContainer(p, primary, out _))
                    {
                        var inner = name;
                        BuildRows(items, r =>
                        {
                            var o = resolve(r);
                            if (o == null) return null;
                            try { return Ymm.FindProperty(o.GetType(), inner)?.GetValue(o); }
                            catch { return null; }
                        }, name.Contains("Shape", StringComparison.Ordinal) ? "図形の設定" : "このアイテムだけの設定", depth + 1, rows);
                    }
                    continue;
                }

                var label = YmmText.Resolve(Safe(() => display.GetName()) ?? name);
                var group = YmmText.Resolve(Safe(() => display.GetGroupName()) ?? defaultGroup);
                var tip = Safe(() => display.GetDescription()) ?? "";
                // 図形ごとの案内(同梱の演出図形): その図形で使わない項目は出さず、使う項目は図形に合った名前にする
                var guide = (primary as YMM4EffectShapes.IParamGuide)?.Guide(name);
                if (guide != null)
                {
                    if (guide.Hidden) continue;
                    if (!string.IsNullOrEmpty(guide.Label)) label = guide.Label;
                    if (!string.IsNullOrEmpty(guide.Group)) group = guide.Group;
                    if (!string.IsNullOrEmpty(guide.Tip)) tip = guide.Tip;
                }
                var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                var value = p.GetValue(primary);

                ItemRowKind kind;
                if (type.Name == "Animation") kind = ItemRowKind.Anim;
                else if (type == typeof(bool)) kind = ItemRowKind.Bool;
                else if (type.IsEnum) kind = ItemRowKind.Enum;
                else if (type == typeof(Color)) kind = ItemRowKind.Color;
                else if (IsNumberType(type)) kind = ItemRowKind.Number;
                else if (type == typeof(string))
                    kind = name == "Font" || name.EndsWith("FontFamily", StringComparison.Ordinal) ? ItemRowKind.Font
                         : name == "Text" || name == "Serif" || WantsMultiline(p, label, tip) ? ItemRowKind.Multiline : ItemRowKind.Text;
                else
                {
                    // 入れ物(図形のパラメータなど): 中の項目を、同じグループとして並べる
                    if (depth < 4 && IsContainer(p, primary, out _))
                    {
                        var propName = name;
                        BuildRows(items, r =>
                        {
                            var o = resolve(r);
                            if (o == null) return null;
                            try { return Ymm.FindProperty(o.GetType(), propName)?.GetValue(o); }
                            catch { return null; }
                        }, label, depth + 1, rows);
                    }
                    continue;
                }
                if (kind != ItemRowKind.Anim && p.GetSetMethod(true) == null) continue;
                if (kind == ItemRowKind.Anim && value == null) continue;

                var targets = new List<(object root, object owner)>();
                foreach (var item in items)
                {
                    var o = resolve(item);
                    if (o != null && Ymm.FindProperty(o.GetType(), name) != null) targets.Add((item, o));
                }
                if (targets.Count == 0) continue;

                SliderInfo(p, out var format, out var unit, out var min, out var max);
                if (guide != null)
                {
                    if (guide.Min != null) min = guide.Min;
                    if (guide.Max != null) max = guide.Max;
                    if (guide.Unit != null) unit = guide.Unit;
                }
                rows.Add(new ItemPropRow(this, kind, name, label, group, tip, targets, format, unit, min, max, type.IsEnum ? type : null) { Resolver = resolve });
            }
            catch { /* 読めない項目は飛ばす */ }
        }
    }

    // ---------- 図形アイテム ----------
    /// <summary>図形アイテムの「図形の設定」(IShapeParameter)を入れる項目</summary>
    static PropertyInfo? ShapeParamProp(Type itemType)
    {
        if (!itemType.Name.Contains("Shape", StringComparison.Ordinal)) return null;
        try
        {
            return itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0 && p.CanRead && !p.PropertyType.IsValueType && p.PropertyType != typeof(string))
                .FirstOrDefault(p => p.PropertyType.Name.Contains("ShapeParameter", StringComparison.Ordinal)
                                     || p.PropertyType.GetInterfaces().Any(i => i.Name == "IShapeParameter")
                                     || p.Name == "ShapeParameter");
        }
        catch { return null; }
    }

    // 図形のパラメータの型 → 図形のプラグインの型名(いまの図形の種類を調べる用)
    static Dictionary<Type, string>? shapeParamTypes;

    static Dictionary<Type, string> ShapeParamTypes()
    {
        if (shapeParamTypes != null) return shapeParamTypes;
        var map = new Dictionary<Type, string>();
        foreach (var (key, _) in ShapeChoices())
        {
            try
            {
                var param = CreateShapeParameter(key);
                if (param != null) map.TryAdd(param.GetType(), key);
            }
            catch { }
        }
        return shapeParamTypes = map;
    }

    /// <summary>図形のプラグインから、初期値の図形パラメータを作る</summary>
    static object? CreateShapeParameter(string pluginTypeName)
    {
        var t = Ymm.FindType(pluginTypeName);
        if (t == null) return null;
        var plugin = Activator.CreateInstance(t);
        var create = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(m => m.Name == "CreateShapeParameter");
        if (create == null) return null;
        var args = create.GetParameters()
            .Select(p => p.HasDefaultValue ? p.DefaultValue : (p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null)).ToArray();
        return create.Invoke(plugin, args);
    }

    void AddShapeRows(IReadOnlyList<object> items, Func<object, object?> resolve, object primary, PropertyInfo paramProp, List<ItemPropRow> rows)
    {
        var shapes = ShapeChoices();
        var typeProp = primary.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.Name.Contains("ShapeType", StringComparison.Ordinal) && p.GetIndexParameters().Length == 0);

        string? CurrentKey(object owner)
        {
            try
            {
                if (typeProp != null)
                {
                    var v = typeProp.GetValue(owner);
                    if (v is string s && shapes.Any(x => x.key == s)) return s;
                    if (v is Type tt && shapes.Any(x => x.key == tt.FullName)) return tt.FullName;
                }
                var param = paramProp.GetValue(owner);
                if (param != null && ShapeParamTypes().TryGetValue(param.GetType(), out var k)) return k;
            }
            catch { }
            return null;
        }

        void SetKey(object owner, string key)
        {
            bool done = false;
            // 図形の種類の項目があれば、それを変える(YMM4が図形の設定も切り替える)
            if (typeProp != null && typeProp.GetSetMethod(true) != null)
            {
                if (typeProp.PropertyType == typeof(string)) { typeProp.GetSetMethod(true)!.Invoke(owner, new object[] { key }); done = true; }
                else if (typeProp.PropertyType == typeof(Type) && Ymm.FindType(key) is { } kt) { typeProp.GetSetMethod(true)!.Invoke(owner, new object[] { kt }); done = true; }
            }
            // 図形の設定が、選んだ図形のものになっていなければ、新しく作って入れる
            var param = paramProp.GetValue(owner);
            var wantType = ShapeParamTypes().FirstOrDefault(x => x.Value == key).Key;
            if (param == null || wantType == null || param.GetType() != wantType)
            {
                var fresh = CreateShapeParameter(key);
                if (fresh != null && paramProp.GetSetMethod(true) != null && paramProp.PropertyType.IsInstanceOfType(fresh))
                {
                    paramProp.GetSetMethod(true)!.Invoke(owner, new[] { fresh });
                    done = true;
                }
            }
            if (!done) Report("図形の種類を変えられませんでした(YMM4のバージョンによって、項目の形が違う可能性があります)");
        }

        if (shapes.Count > 0)
        {
            var targets = new List<(object root, object owner)>();
            foreach (var item in items) { var o = resolve(item); if (o != null && ShapeParamProp(o.GetType()) != null) targets.Add((item, o)); }
            if (targets.Count > 0)
                rows.Add(new ItemPropRow(this, ItemRowKind.Enum, typeProp?.Name ?? paramProp.Name, "図形の種類", "図形の設定",
                    "図形の形を選びます(変えると、その図形だけの設定項目に切り替わります)", targets, "0.##", "", null, null, null, shapes)
                { Resolver = resolve, ChoiceGet = CurrentKey, ChoiceSet = SetKey, IsShapeRow = true });
        }

        // その図形だけの設定(サイズ・角の数・丸みなど)
        var paramName = paramProp.Name;
        int first = rows.Count;
        BuildRows(items, r =>
        {
            var o = resolve(r);
            if (o == null) return null;
            try { return Ymm.FindProperty(o.GetType(), paramName)?.GetValue(o); }
            catch { return null; }
        }, "図形の設定", 1, rows, looseLabels: true);

        // 配置画面(演出図形ごとの、位置・向き・長さなどを動かすつまみ)
        try
        {
            object? param = null;
            try { param = paramProp.GetValue(primary); } catch { }
            var key = CurrentKey(primary);
            var title = shapes.FirstOrDefault(x => x.key == key).name ?? param?.GetType().Name ?? "図形";
            pendingStage = (param?.GetType().Name, title, rows.Skip(first).ToList());
        }
        catch { pendingStage = null; }
    }

    /// <summary>YMM4に登録してあるキャラクターの一覧(いまの名前が一覧に無ければ、それも足す)</summary>
    static List<(string key, string name)> CharacterChoices(string? current)
    {
        var list = new List<(string key, string name)>();
        try
        {
            var st = Ymm.FindType("YukkuriMovieMaker.Settings.CharacterSettings");
            var def = st?.GetProperty("Default", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
            if (Ymm.Get(def, "Characters") is IEnumerable e)
                foreach (var c in e)
                {
                    var n = Ymm.Get(c, "Name") as string;
                    if (!string.IsNullOrEmpty(n) && !list.Any(x => x.key == n)) list.Add((n!, n!));
                }
        }
        catch { }
        if (list.Count > 0 && !string.IsNullOrEmpty(current) && !list.Any(x => x.key == current)) list.Insert(0, (current!, current + "(登録されていません)"));
        return list;
    }

    static List<(string key, string name)>? shapeChoices;

    /// <summary>図形の種類の一覧(標準とプラグイン)</summary>
    static List<(string key, string name)> ShapeChoices()
    {
        if (shapeChoices != null && shapeChoices.Count > 0) return shapeChoices;
        var list = new List<(string key, string name)>();
        try
        {
            var found = ShapeDiscovery.Found.Count > 0 ? ShapeDiscovery.Found : ShapeDiscovery.Scan();
            foreach (var s in found)
                if (!string.IsNullOrEmpty(s.Key))
                    list.Add((s.Key, s.IsBuiltIn || ShapeCategories.TryGet(s.Key, out _, out _) ? s.Name : $"{s.Name}(プラグイン)"));
        }
        catch { }
        shapeChoices = list;
        return list;
    }

    /// <summary>項目が「中に設定を持つ入れ物」か(中に、表示名つきの項目が1つ以上ある)</summary>
    static bool IsContainer(PropertyInfo p, object owner, out object? value)
    {
        value = null;
        try
        {
            var pt = p.PropertyType;
            if (pt.IsValueType || pt == typeof(string) || typeof(IEnumerable).IsAssignableFrom(pt)) return false;
            if (p.Name == "KeyFrames" || p.GetIndexParameters().Length != 0) return false;
            value = p.GetValue(owner);
            if (value == null || value is IEnumerable || ReferenceEquals(value, owner)) return false;
            var vt = value.GetType();
            if (vt.Name == "Animation" || vt.IsValueType) return false;
            var ns = vt.Namespace ?? "";
            if (ns.StartsWith("System", StringComparison.Ordinal) || ns.StartsWith("Microsoft", StringComparison.Ordinal)) return false;
            return vt.GetProperties(BindingFlags.Public | BindingFlags.Instance).Any(x => x.GetCustomAttribute<DisplayAttribute>(true) != null);
        }
        catch { return false; }
    }

    static List<object> EffectsOf(object item, bool video)
    {
        var list = new List<object>();
        if (Ymm.Get(item, Ymm.EffectsProperty(video)) is IEnumerable e)
            foreach (var x in e) if (x != null) list.Add(x);
        return list;
    }

    static object? EffectAt(object item, bool video, int index, Type type)
    {
        var list = EffectsOf(item, video);
        return index < list.Count && list[index].GetType() == type ? list[index] : null;
    }

    void BuildEffects(List<object> items, bool video)
    {
        var first = items[0];
        if (Ymm.FindProperty(first.GetType(), Ymm.EffectsProperty(video)) == null) return;
        if (video) HasVideoEffectList = true; else HasAudioEffectList = true;

        var target = video ? VideoEffects : AudioEffects;
        var effects = EffectsOf(first, video);
        for (int i = 0; i < effects.Count; i++)
        {
            var fx = effects[i];
            var type = fx.GetType();
            var label = Ymm.Get(fx, "Label") as string;
            if (string.IsNullOrWhiteSpace(label)) label = type.Name.Replace("Effect", "");
            var key = (video ? "v" : "a") + i + label;
            var card = new ItemEffectCard(this, video, i, label!, effectOpen.Contains(key) || effects.Count == 1);
            int index = i;
            BuildRows(items, r => EffectAt(r, video, index, type), label!, 1, card.Rows);
            target.Add(card);
        }
    }

    // ------------------------------------------------------------------ 絞り込み・開閉
    void ApplyFilter()
    {
        var f = filterText.Trim();
        foreach (var g in Groups)
        {
            bool any = false;
            foreach (var r in g.Rows)
            {
                r.IsVisible = f.Length == 0 || r.Label.Contains(f, StringComparison.CurrentCultureIgnoreCase)
                              || g.Name.Contains(f, StringComparison.CurrentCultureIgnoreCase);
                any |= r.IsVisible;
            }
            g.HasVisible = any;
            if (f.Length > 0 && any) g.IsOpen = true;
        }
    }

    void SetAllOpen(bool open)
    {
        foreach (var g in Groups) g.IsOpen = open;
        foreach (var c in VideoEffects.Concat(AudioEffects)) c.IsOpen = open;
    }

    // ------------------------------------------------------------------ エフェクトの操作
    public bool GetEffectEnabled(ItemEffectCard card)
    {
        if (roots.Count == 0) return false;
        var list = EffectsOf(roots[0], card.IsVideo);
        return card.Index < list.Count && Ymm.Get(list[card.Index], "IsEnabled") is bool b && b;
    }

    public async void SetEffectEnabled(ItemEffectCard card, bool enabled)
    {
        foreach (var item in roots.ToList())
        {
            var list = EffectsOf(item, card.IsVideo);
            if (card.Index >= list.Count) continue;
            var fx = list[card.Index];
            try
            {
                await Ymm.EditAsync(item, () =>
                {
                    if (!Ymm.SetValue(fx, "IsEnabled", enabled, out var error)) Report("エフェクトのオン・オフを変えられませんでした: " + error);
                });
            }
            catch (Exception ex) { Report(ex.GetBaseException().Message); }
        }
        card.Refresh();
    }

    /// <summary>選択中のアイテムのエフェクトの並びを、change で作り直す(null を返したアイテムは、何もしない)</summary>
    async Task ChangeEffectsAsync(bool video, Func<List<object>, List<object>?> change)
    {
        var prop = Ymm.EffectsProperty(video);
        await FlushLiveAsync();
        foreach (var item in roots.ToList())
        {
            try
            {
                var p = Ymm.FindProperty(item.GetType(), prop);
                if (p == null) continue;
                var next = change(EffectsOf(item, video));
                if (next == null) continue;
                var merged = Ymm.MergeEffects(p.PropertyType, null, next, false);
                if (merged == null) { Report(Ymm.LastError); continue; }
                await Ymm.EditAsync(item, () =>
                {
                    if (!Ymm.SetValue(item, prop, merged, out var error)) Report("エフェクトを変更できませんでした: " + error);
                });
            }
            catch (Exception ex) { Report("エフェクトを変更できませんでした: " + ex.GetBaseException().Message); }
        }
        Rebuild();
    }

    public async void RemoveEffect(ItemEffectCard card)
    {
        await ChangeEffectsAsync(card.IsVideo, list =>
        {
            if (card.Index >= list.Count) return null;
            list.RemoveAt(card.Index);
            return list;
        });
    }

    public async void MoveEffect(ItemEffectCard card, int delta)
    {
        await ChangeEffectsAsync(card.IsVideo, list =>
        {
            int to = card.Index + delta;
            if (card.Index >= list.Count || to < 0 || to >= list.Count) return null;
            (list[card.Index], list[to]) = (list[to], list[card.Index]);
            return list;
        });
    }

    /// <summary>選択中のアイテムへ、エフェクトを1つ足す(エフェクトを選ぶ画面から呼ばれる)</summary>
    public async void AddEffect(bool video, Type type, string name)
    {
        if (roots.Count == 0) { Report("YMM4のタイムラインで、アイテムを選んでください"); return; }
        bool failed = false;
        await ChangeEffectsAsync(video, list =>
        {
            // アイテムごとに新しく作る(エフェクトの実体を、複数のアイテムで共有しないため)
            object? fx;
            try { fx = Activator.CreateInstance(type, true); }
            catch { fx = null; }
            if (fx == null) { failed = true; return null; }
            list.Add(fx);
            return list;
        });
        if (failed) Report($"「{name}」を作れませんでした");
        else if (Message.Length == 0) Message = $"「{name}」を追加しました(いちばん下に入ります)";
    }
}
