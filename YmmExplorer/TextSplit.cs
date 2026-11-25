using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace YmmExplorer;

/// <summary>テキスト分割の画面の、1文字分(右側に「ここで分ける」印を持つ)</summary>
public sealed class SplitCell : NotifyBase
{
    readonly Action changed;
    bool splitAfter;
    public string Char { get; }
    public bool IsNewline { get; }
    public string Display => IsNewline ? "↵" : Char == " " ? "␣" : Char == "　" ? "□" : Char;
    public bool ShowGap { get; }

    public SplitCell(string ch, bool showGap, Action changed)
    {
        Char = ch;
        IsNewline = ch == "\n";
        ShowGap = showGap;
        this.changed = changed;
    }

    /// <summary>この文字の後ろで分ける</summary>
    public bool SplitAfter
    {
        get => splitAfter;
        set { if (ShowGap && Set(ref splitAfter, value)) changed(); }
    }

    bool shade;
    /// <summary>分けた後のまとまりごとに、交互に色を付ける(どこからどこまでが1つのアイテムになるかが、ひと目で分かる)</summary>
    public bool Shade { get => shade; set => Set(ref shade, value); }

    public void SetSilently(bool value)
    {
        if (!ShowGap || splitAfter == value) return;
        splitAfter = value;
        Raise(nameof(SplitAfter));
    }
}

/// <summary>分けた後にできるアイテム1つ分の見本</summary>
public sealed record SplitPreview(string Number, string Text, string Info);

/// <summary>
/// テキスト分割: 選択中のテキストアイテムを、選んだ位置で複数のアイテムに分ける。
/// ・字幕切替: 時間で分ける(1つめ → 2つめ → … と、順番に表示される)
/// ・レイアウト: 同じ時間のまま、文字の並びを保って別々のアイテムにする(1つずつ動かしたり、色を変えたりできる)
/// </summary>
public sealed class TextSplitVm : NotifyBase
{
    object? target;
    /// <summary>文字が入っている項目の名前(テキストは Text、ボイスは Serif)</summary>
    string textProp = "Text";
    bool isVoice;
    bool isOpen, isLayout, byLength = true, busy;
    string message = "";

    public ObservableCollection<SplitCell> Cells { get; } = new();
    public ICommand OpenCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand SplitAllCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand SplitLinesCommand { get; }
    public ICommand SplitPunctCommand { get; }
    public ICommand ExecuteCommand { get; }

    public TextSplitVm()
    {
        OpenCommand = new RelayCommand(_ => Open());
        CloseCommand = new RelayCommand(_ => IsOpen = false);
        SplitAllCommand = new RelayCommand(_ => SetAll(c => !c.IsNewline));
        ResetCommand = new RelayCommand(_ => SetAll(_ => false));
        SplitLinesCommand = new RelayCommand(_ => SetAll(c => c.IsNewline, keepOthers: true));
        SplitPunctCommand = new RelayCommand(_ => SetAll(c => "、。，．,.!?！？…".Contains(c.Char), keepOthers: true));
        ExecuteCommand = new RelayCommand(async _ => await ExecuteAsync(), _ => !busy && SplitCount > 0);
    }

    public bool IsOpen { get => isOpen; set => Set(ref isOpen, value); }
    public string Message { get => message; private set => Set(ref message, value); }

    public bool IsLayout
    {
        get => isLayout;
        set
        {
            if (value && isVoice) { Raise(nameof(IsLayout)); Raise(nameof(IsSubtitle)); return; }   // ボイスは、時間で分けるだけ
            if (Set(ref isLayout, value)) { Raise(nameof(IsSubtitle)); Raise(nameof(ModeText)); Changed(); }
        }
    }

    /// <summary>ボイスのアイテムを分ける時(音声があるので、レイアウトは選べない)</summary>
    public bool IsVoice => isVoice;
    public bool CanLayout => !isVoice;
    public double LayoutOpacity => isVoice ? 0.4 : 1;
    public string ByLengthText => isVoice
        ? "発音の長さに合わせて、切る位置を決める(オフにすると、同じ長さで切る)"
        : "表示する長さを、文字数に合わせて配分する(オフにすると、同じ長さ)";
    public string VoiceNote => "ボイス: 元の音声を、分けた位置で切って使います(1つめの続きから、2つめが再生されます)。切る位置は、文字の読みの長さから計算した目安です。合わない時は、分けた後に、タイムラインでアイテムの境目を動かしてください";
    public bool IsSubtitle { get => !isLayout; set { if (value) IsLayout = false; } }

    /// <summary>字幕切替: 表示する長さを、文字数に合わせて配分する(オフなら、同じ長さ)</summary>
    public bool ByLength { get => byLength; set { if (Set(ref byLength, value)) Changed(); } }

    public string ModeText => isLayout
        ? "レイアウト: 同じ時間のまま、文字の並びを保って別々のアイテムにします(下のレイヤーへ順に並びます)。位置は、いまの書式(基準点・文字揃え・文字間隔・行の高さ・縦書きなど)を読み取って合わせます"
        : "字幕切替: 時間で分けます。1つめ → 2つめ → … と、順番に表示されます(同じレイヤーに並びます)";

    int SplitCount => Cells.Count(c => c.SplitAfter);
    public string CountText => $"{Cells.Count(c => !c.IsNewline)} 文字";
    public string SplitText => SplitCount == 0 ? "分ける位置を、文字のあいだをクリックして選んでください" : $"{SplitCount} 箇所で分割 → {SplitCount + 1} 個のアイテム";

    /// <summary>分けた後にできるアイテムの見本(番号・文字・表示する長さ)</summary>
    public ObservableCollection<SplitPreview> Preview { get; } = new();
    public bool HasPreview => Preview.Count > 1;

    void Changed()
    {
        // まとまりごとに、交互に色を付ける
        bool on = false;
        foreach (var c in Cells)
        {
            c.Shade = on;
            if (c.SplitAfter || (isLayout && c.IsNewline)) on = !on;
        }
        // できあがりの見本
        Preview.Clear();
        try
        {
            var parts = BuildParts(isLayout);
            int length = target == null ? 0 : Convert.ToInt32(Ymm.Get(target, "Length") ?? 0);
            double total = parts.Sum(Weight);
            for (int i = 0; i < parts.Count; i++)
            {
                string info;
                if (isLayout) info = "同じ時間";
                else
                {
                    double w = Weight(parts[i]);
                    info = length > 0 && total > 0 ? $"約 {Math.Max(1, (int)Math.Round(length * w / total))} フレーム" : "";
                }
                Preview.Add(new SplitPreview((i + 1).ToString(), parts[i].Text.Replace("\n", " ↵ "), info));
            }
        }
        catch { }
        Raise(nameof(HasPreview));
        Raise(nameof(SplitText));
        CommandManager.InvalidateRequerySuggested();
    }

    void SetAll(Func<SplitCell, bool> pick, bool keepOthers = false)
    {
        foreach (var c in Cells)
        {
            bool on = pick(c);
            if (on || !keepOthers) c.SetSilently(on);
        }
        Changed();
    }

    /// <summary>選択中のテキストアイテムを読み込んで、画面を開く</summary>
    public void Open()
    {
        Message = "";
        static string? TextPropOf(object x) =>
            Ymm.FindProperty(x.GetType(), "Text")?.PropertyType == typeof(string) ? "Text"
            : Ymm.FindProperty(x.GetType(), "Serif")?.PropertyType == typeof(string) ? "Serif" : null;
        var item = Ymm.SelectedItems().FirstOrDefault(x => TextPropOf(x) != null);
        if (item == null)
        {
            target = null;
            Cells.Clear();
            Message = "⚠ YMM4のタイムラインで、テキストかボイスのアイテムを選んでから開いてください";
            Raise(nameof(CountText)); Changed();
            IsOpen = true;
            return;
        }
        target = item;
        textProp = TextPropOf(item) ?? "Text";
        isVoice = textProp == "Serif";
        if (isVoice) isLayout = false;
        foreach (var n in new[] { nameof(IsVoice), nameof(CanLayout), nameof(LayoutOpacity), nameof(ByLengthText), nameof(IsLayout), nameof(IsSubtitle), nameof(ModeText) }) Raise(n);
        var text = (Ymm.Get(item, textProp) as string ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
        Cells.Clear();
        // 文字単位(絵文字など、2つで1文字になるものをまとめる)
        var e = StringInfo.GetTextElementEnumerator(text);
        var chars = new List<string>();
        while (e.MoveNext()) chars.Add(e.GetTextElement());
        for (int i = 0; i < chars.Count; i++) Cells.Add(new SplitCell(chars[i], i < chars.Count - 1, Changed));
        try { FormatInfo = "読み取った書式: " + TextFormat.Read(item).Summary; }
        catch { FormatInfo = ""; }
        Raise(nameof(CountText));
        Changed();
        IsOpen = true;
    }

    string formatInfo = "";
    /// <summary>選択中のテキストから読み取った書式(レイアウトの位置の計算に使う)</summary>
    public string FormatInfo { get => formatInfo; private set => Set(ref formatInfo, value); }

    sealed record Part(string Text, int Line, int Start);

    /// <summary>
    /// 読み上げた時の、おおよその長さ(拍の数)。かな1文字 = 1、漢字1文字 = 2、小さい「ゃゅょ」などは前の字とまとめて数え、
    /// 「、」「。」は、息つぎの分を足す。発音に合わせて切る位置を決めるのに使う。
    /// </summary>
    static double SpokenLength(string text)
    {
        double w = 0;
        foreach (var ch in text)
        {
            if (ch == '\n' || char.IsWhiteSpace(ch)) w += 0.3;
            else if ("、，,・".Contains(ch)) w += 1.0;
            else if ("。．.!?！？…".Contains(ch)) w += 1.6;
            else if ("ゃゅょぁぃぅぇぉゎャュョァィゥェォヮ".Contains(ch)) w += 0.1;
            else if (ch >= '\u3040' && ch <= '\u30FF') w += 1.0;               // ひらがな・カタカナ(っ・ー・ん も1拍)
            else if (ch >= '\u4E00' && ch <= '\u9FFF') w += 2.0;               // 漢字は、平均して2拍くらい
            else if (char.IsDigit(ch)) w += 2.0;
            else if (char.IsLetter(ch)) w += ch < 128 ? 0.6 : 1.0;
        }
        return Math.Max(0.5, w);
    }

    /// <summary>時間を配分する時の、そのまとまりの重み</summary>
    double Weight(Part p)
    {
        if (!byLength) return 1;
        return isVoice ? SpokenLength(p.Text) : Math.Max(1, p.Text.Replace("\n", "").Length);
    }

    /// <summary>YMM4のプロジェクトの、1秒あたりのフレーム数</summary>
    static double Fps()
    {
        try
        {
            var tl = Ymm.Timeline();
            foreach (var v in new[] { Ymm.Get(Ymm.Get(tl, "VideoInfo"), "FPS"), Ymm.Get(tl, "FPS"), Ymm.Get(Ymm.Get(tl, "VideoInfo"), "Fps") })
                if (v != null)
                {
                    double f = Convert.ToDouble(v, CultureInfo.InvariantCulture);
                    if (f > 1) return f;
                }
        }
        catch { }
        return 60;
    }

    /// <summary>分けた後の文字のまとまり。レイアウトの時は、改行でも必ず分ける</summary>
    List<Part> BuildParts(bool splitAtNewline)
    {
        var parts = new List<Part>();
        var sb = new System.Text.StringBuilder();
        int line = 0, col = 0, start = 0, startLine = 0;
        void Flush()
        {
            var t = sb.ToString();
            sb.Clear();
            var trimmed = t.Trim('\n');
            if (trimmed.Length > 0) parts.Add(new Part(trimmed, startLine, start));
        }
        foreach (var c in Cells)
        {
            if (c.IsNewline)
            {
                if (splitAtNewline) { Flush(); line++; col = 0; start = 0; startLine = line; continue; }
                sb.Append('\n');
                line++; col = 0;
            }
            else
            {
                if (sb.Length == 0) { start = col; startLine = line; }
                sb.Append(c.Char);
                col += c.Char.Length;
            }
            if (c.SplitAfter) { Flush(); start = col; startLine = line; }
        }
        Flush();
        return parts;
    }

    static double FirstOf(IEnumerable values, double fallback)
    {
        foreach (var v in values) return Convert.ToDouble(Ymm.Get(v, "Value") ?? fallback, CultureInfo.InvariantCulture);
        return fallback;
    }

    static double FirstValue(object item, string prop, double fallback)
    {
        try
        {
            if (Ymm.Get(Ymm.FindProperty(item.GetType(), prop)?.GetValue(item), "Values") is IEnumerable e)
                foreach (var v in e) return Convert.ToDouble(Ymm.Get(v, "Value") ?? fallback, CultureInfo.InvariantCulture);
        }
        catch { }
        return fallback;
    }

    /// <summary>位置(X・Y)を、delta だけずらす(移動している時は、すべての点をずらす)</summary>
    static void Shift(object item, string prop, double delta)
    {
        if (Math.Abs(delta) < 1e-6) return;
        try
        {
            if (Ymm.Get(Ymm.FindProperty(item.GetType(), prop)?.GetValue(item), "Values") is not IEnumerable e) return;
            foreach (var v in e)
            {
                if (v == null) continue;
                double now = Convert.ToDouble(Ymm.Get(v, "Value") ?? 0.0, CultureInfo.InvariantCulture);
                Ymm.SetValue(v, "Value", now + delta, out _);
            }
        }
        catch { }
    }

    static double Width(string text, Typeface face, double size)
    {
        if (text.Length == 0) return 0;
        var ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, face, size, Brushes.Black, 1.0);
        return ft.WidthIncludingTrailingWhitespace;
    }

    async Task ExecuteAsync()
    {
        var item = target;
        if (item == null || busy) return;
        busy = true;
        try
        {
            var parts = BuildParts(isLayout);
            if (parts.Count < 2) { Message = "⚠ 分ける位置を選んでください"; return; }
            int frame = Convert.ToInt32(Ymm.Get(item, "Frame") ?? 0), length = Convert.ToInt32(Ymm.Get(item, "Length") ?? 0);
            int layer = Convert.ToInt32(Ymm.Get(item, "Layer") ?? 0);

            // 2つめ以降のアイテムは、元のアイテムの複製から作る(書式・エフェクトを引き継ぐ)
            var clones = new List<object>();
            for (int i = 1; i < parts.Count; i++)
            {
                var c = Ymm.CloneItem(item);
                if (c == null) { Message = "⚠ アイテムを複製できませんでした: " + Ymm.LastError; return; }
                clones.Add(c);
            }

            string report;
            if (!isLayout)
            {
                // ---- 字幕切替: 時間を配分する
                if (length < parts.Count) { Message = $"⚠ アイテムが短すぎます({length} フレームを {parts.Count} 個には分けられません)"; return; }
                var weights = parts.Select(Weight).ToList();
                double total = weights.Sum();
                var lengths = weights.Select(w => Math.Max(1, (int)Math.Floor(length * w / total))).ToList();
                lengths[^1] = length - lengths.Take(lengths.Count - 1).Sum();
                if (lengths[^1] < 1) { Message = "⚠ アイテムが短すぎて、分けられません"; return; }

                // ボイス: 2つめ以降は、音声の再生を始める位置を、その分だけ後ろへずらす(元の音声の続きから鳴るように)
                double fps = Fps();
                double rate = 1;
                bool cutAudio = false;
                TimeSpan baseOffset = TimeSpan.Zero;
                if (isVoice)
                {
                    try
                    {
                        var rp = Ymm.FindProperty(item.GetType(), "PlaybackRate")?.GetValue(item);
                        double r = rp == null ? 100 : (Ymm.Get(rp, "Values") is IEnumerable re ? FirstOf(re, 100) : Convert.ToDouble(rp, CultureInfo.InvariantCulture));
                        if (r > 0) rate = r / 100.0;
                    }
                    catch { }
                    if (Ymm.FindProperty(item.GetType(), "ContentOffset")?.GetValue(item) is TimeSpan ts) { baseOffset = ts; cutAudio = true; }
                }

                await Ymm.EditAsync(item, () =>
                {
                    Ymm.SetValue(item, textProp, parts[0].Text, out _);
                    Ymm.SetValue(item, "Length", lengths[0], out _);
                });
                int offset = lengths[0], placed = 1;
                for (int i = 1; i < parts.Count; i++)
                {
                    var c = clones[i - 1];
                    Ymm.SetValue(c, textProp, parts[i].Text, out _);
                    if (cutAudio) Ymm.SetValue(c, "ContentOffset", baseOffset + TimeSpan.FromSeconds(offset / fps * rate), out _);
                    Ymm.SetValue(c, "Length", lengths[i], out _);
                    Ymm.SetValue(c, "Frame", 0, out _);
                    Ymm.SetValue(c, "Layer", 0, out _);
                    var template = Ymm.CreateTemplate("テキスト分割", new[] { c });
                    if (template == null) { Message = "⚠ 配置用のデータを作れませんでした: " + Ymm.LastError; return; }
                    var r = await Ymm.PlaceTemplateAtAsync(template, frame + offset, layer);
                    if (!r.Ok) { Message = $"⚠ {placed} 個めまで分けましたが、続きを配置できませんでした: {r.Message}"; return; }
                    offset += lengths[i];
                    placed++;
                }
                report = $"{parts.Count} 個のアイテムに分けました(時間で切り替わります)"
                         + (isVoice ? (cutAudio ? "。音声は、元の音声を切って使っています" : "。⚠ 音声の再生位置を変える項目(ContentOffset)が見つからなかったため、音声は切れていません") : "");
            }
            else
            {
                // ---- レイアウト: いまの書式(基準点・文字揃え・文字間隔・行の高さ・太字・縦書き)を読んで、文字の並びを保つように位置をずらす
                var fmt = TextFormat.Read(item);
                var lines = string.Concat(Cells.Select(c => c.Char)).Split('\n');
                var lineSizes = lines.Select(l => fmt.Advance(l)).ToList();           // 各行(縦書きなら各列)の、進む方向の長さ
                double longest = lineSizes.Count == 0 ? 0 : lineSizes.Max();
                double thick = fmt.LineThickness;                                    // 行の高さ(縦書きなら列の幅)

                (double dx, double dy) Offset(Part p)
                {
                    var lineText = p.Line < lines.Length ? lines[p.Line] : "";
                    double lineLen = p.Line < lineSizes.Count ? lineSizes[p.Line] : 0;
                    double before = fmt.Advance(lineText.Substring(0, Math.Min(p.Start, lineText.Length)));
                    double len = fmt.Advance(p.Text);
                    double x, y;
                    if (!fmt.Vertical)
                    {
                        // 横書き: 全体は 幅 longest × 高さ (行数 × thick)。行は、文字揃えに合わせて左右に寄る
                        double blockW = longest, blockH = lines.Length * thick;
                        double left = fmt.AlignX * (blockW - lineLen) + before, top = p.Line * thick;
                        // 分けた後のアイテムも同じ基準点で描かれるので、「その部分の基準点」と「全体の基準点」の差だけ動かす
                        x = left + fmt.AnchorX * len - fmt.AnchorX * blockW;
                        y = top + fmt.AnchorY * thick - fmt.AnchorY * blockH;
                    }
                    else
                    {
                        // 縦書き: 列は右から左へ並び、文字は上から下へ進む
                        double blockW = lines.Length * thick, blockH = longest;
                        double left = blockW - (p.Line + 1) * thick, top = fmt.AlignY * (blockH - lineLen) + before;
                        x = left + fmt.AnchorX * thick - fmt.AnchorX * blockW;
                        y = top + fmt.AnchorY * len - fmt.AnchorY * blockH;
                    }
                    return (x * fmt.Zoom, y * fmt.Zoom);
                }

                var first = Offset(parts[0]);
                await Ymm.EditAsync(item, () =>
                {
                    Ymm.SetValue(item, textProp, parts[0].Text, out _);
                    Shift(item, "X", first.dx);
                    Shift(item, "Y", first.dy);
                });
                for (int i = 1; i < parts.Count; i++)
                {
                    var c = clones[i - 1];
                    var o = Offset(parts[i]);
                    Ymm.SetValue(c, textProp, parts[i].Text, out _);
                    Shift(c, "X", o.dx);
                    Shift(c, "Y", o.dy);
                    Ymm.SetValue(c, "Frame", 0, out _);
                    Ymm.SetValue(c, "Layer", 0, out _);
                    var template = Ymm.CreateTemplate("テキスト分割", new[] { c });
                    if (template == null) { Message = "⚠ 配置用のデータを作れませんでした: " + Ymm.LastError; return; }
                    var r = await Ymm.PlaceTemplateAtAsync(template, frame, layer + i);
                    if (!r.Ok) { Message = $"⚠ {i} 個めまで分けましたが、続きを配置できませんでした: {r.Message}"; return; }
                }
                report = $"{parts.Count} 個のアイテムに分けました(レイヤー {layer + 1}〜{layer + parts.Count} に並びます)。読み取った書式: {fmt.Summary}";
            }
            Message = report;
            IsOpen = false;
            Done?.Invoke(report);
        }
        catch (Exception ex) { Message = "⚠ 分割できませんでした: " + ex.GetBaseException().Message; }
        finally { busy = false; CommandManager.InvalidateRequerySuggested(); }
    }

    /// <summary>分割が終わった時(下の状態表示に出す)</summary>
    public event Action<string>? Done;
}

/// <summary>
/// テキストアイテムの、いまの書式。レイアウト分割で、分けた後のアイテムの位置を計算するのに使う。
/// YMM4の項目名はバージョンで違うことがあるので、候補の名前を順に探し、見つからないものは標準の値にする。
/// </summary>
public sealed class TextFormat
{
    public Typeface Face { get; private set; } = new("Yu Gothic UI");
    public string FontName { get; private set; } = "";
    public double Size { get; private set; } = 34;
    public double Zoom { get; private set; } = 1;
    /// <summary>文字と文字のあいだに足す長さ(px)</summary>
    public double Spacing { get; private set; }
    /// <summary>行の高さの倍率(1 = 標準)</summary>
    public double LineScale { get; private set; } = 1;
    public bool Bold { get; private set; }
    public bool Italic { get; private set; }
    public bool Vertical { get; private set; }
    public bool Wrapped { get; private set; }
    /// <summary>基準点(0=左・上 / 0.5=中央 / 1=右・下)</summary>
    public double AnchorX { get; private set; } = 0.5;
    public double AnchorY { get; private set; } = 0.5;
    /// <summary>行の中での寄せ方(0=左・上 / 0.5=中央 / 1=右・下)</summary>
    public double AlignX { get; private set; } = 0.5;
    public double AlignY { get; private set; } = 0.5;
    public string AnchorName { get; private set; } = "中央(標準)";

    static object? Raw(object item, params string[] names)
    {
        foreach (var n in names)
        {
            try
            {
                var p = Ymm.FindProperty(item.GetType(), n);
                if (p != null) return p.GetValue(item);
            }
            catch { }
        }
        return null;
    }

    /// <summary>数値を読む(移動できる項目なら、始点の値)</summary>
    static double? Number(object item, params string[] names)
    {
        var raw = Raw(item, names);
        if (raw == null) return null;
        try
        {
            if (Ymm.Get(raw, "Values") is IEnumerable e)
            {
                foreach (var v in e) return Convert.ToDouble(Ymm.Get(v, "Value") ?? 0.0, CultureInfo.InvariantCulture);
                return null;
            }
            return raw is IConvertible ? Convert.ToDouble(raw, CultureInfo.InvariantCulture) : null;
        }
        catch { return null; }
    }

    /// <summary>「LeftTop」「右下」のような名前から、左右(0〜1)と上下(0〜1)を読む。読めない方向は null</summary>
    static void ParseSide(string name, out double? x, out double? y)
    {
        x = null; y = null;
        if (name.Contains("Left", StringComparison.OrdinalIgnoreCase) || name.Contains('左')) x = 0;
        else if (name.Contains("Right", StringComparison.OrdinalIgnoreCase) || name.Contains('右')) x = 1;
        if (name.Contains("Top", StringComparison.OrdinalIgnoreCase) || name.Contains('上')) y = 0;
        else if (name.Contains("Bottom", StringComparison.OrdinalIgnoreCase) || name.Contains('下')) y = 1;
        if (name.Contains("Center", StringComparison.OrdinalIgnoreCase) || name.Contains("Middle", StringComparison.OrdinalIgnoreCase) || name.Contains('中'))
        {
            x ??= 0.5;
            y ??= 0.5;
        }
    }

    public static TextFormat Read(object item)
    {
        var f = new TextFormat();
        f.FontName = Raw(item, "Font", "FontFamily") as string ?? "Yu Gothic UI";
        f.Size = Number(item, "FontSize", "Size") ?? 34;
        f.Zoom = (Number(item, "Zoom") ?? 100) / 100.0;
        f.Spacing = Number(item, "LetterSpacing2", "LetterSpacing") ?? 0;
        // 行の高さ: 100 が標準(%)の形と、1 が標準(倍率)の形の、どちらにも合わせる
        var lh = Number(item, "LineHeight2", "LineHeight");
        if (lh != null && lh.Value > 0) f.LineScale = lh.Value > 10 ? lh.Value / 100.0 : lh.Value;
        f.Bold = Raw(item, "Bold", "IsBold") is bool b && b;
        f.Italic = Raw(item, "Italic", "IsItalic") is bool i && i;
        try { f.Face = new Typeface(new FontFamily(f.FontName), f.Italic ? FontStyles.Italic : FontStyles.Normal, f.Bold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal); }
        catch { f.Face = new Typeface("Yu Gothic UI"); }

        // 基準点(どこを位置の基準にするか)
        var basePoint = Raw(item, "BasePoint", "Anchor", "Origin");
        if (basePoint != null)
        {
            ParseSide(basePoint.ToString() ?? "", out var ax, out var ay);
            f.AnchorX = ax ?? 0.5;
            f.AnchorY = ay ?? 0.5;
            f.AnchorName = (f.AnchorY < 0.25 ? "上" : f.AnchorY > 0.75 ? "下" : "中央") + "・" + (f.AnchorX < 0.25 ? "左" : f.AnchorX > 0.75 ? "右" : "中央");
        }
        // 行の中での寄せ方(文字揃えの項目があればそれを、無ければ基準点と同じ)
        f.AlignX = f.AnchorX;
        f.AlignY = f.AnchorY;
        var align = Raw(item, "TextAlignment", "TextAlign", "Alignment", "HorizontalAlignment", "TextHorizontalAlignment");
        if (align != null)
        {
            ParseSide(align.ToString() ?? "", out var lx, out var ly);
            if (lx != null) f.AlignX = lx.Value;
            if (ly != null) f.AlignY = ly.Value;
        }

        // 縦書き・折り返しは、項目名が分からないので、名前と値から探す
        try
        {
            foreach (var p in item.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (p.GetIndexParameters().Length != 0 || !p.CanRead) continue;
                var n = p.Name;
                object? v;
                try { v = p.GetValue(item); } catch { continue; }
                if (v == null) continue;
                var text = v.ToString() ?? "";
                bool about = n.Contains("Vertical", StringComparison.OrdinalIgnoreCase) || n.Contains("Writing", StringComparison.OrdinalIgnoreCase)
                             || n.Contains("Orientation", StringComparison.OrdinalIgnoreCase) || n.Contains("TextDirection", StringComparison.OrdinalIgnoreCase);
                if (about && ((v is bool vb && vb && n.Contains("Vertical", StringComparison.OrdinalIgnoreCase))
                              || (v.GetType().IsEnum && (text.Contains("Vertical", StringComparison.OrdinalIgnoreCase) || text.Contains('縦')))))
                    f.Vertical = true;
                if (n.Contains("Wrap", StringComparison.OrdinalIgnoreCase)
                    && ((v is bool wb && wb) || (v.GetType().IsEnum && !text.Contains("No", StringComparison.OrdinalIgnoreCase) && !text.Contains("None", StringComparison.OrdinalIgnoreCase)
                                                 && !text.Contains("しない") && !text.Contains("なし"))))
                    f.Wrapped = true;
            }
        }
        catch { }
        return f;
    }

    FormattedText Measure(string text) =>
        new(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Face, Math.Max(1, Size), Brushes.Black, 1.0);

    static int CharCount(string text) => new StringInfo(text).LengthInTextElements;

    /// <summary>文字が進む方向の長さ(横書きなら幅、縦書きなら高さ)。文字間隔を、1文字ごとに足す</summary>
    public double Advance(string text)
    {
        if (text.Length == 0) return 0;
        int n = CharCount(text);
        if (Vertical) return n * (Size + Spacing);
        return Measure(text).WidthIncludingTrailingWhitespace + n * Spacing;
    }

    /// <summary>1行の高さ(縦書きなら、1列の幅)</summary>
    public double LineThickness => (Vertical ? Size : Measure("あ").Height) * LineScale;

    public string Summary =>
        $"基準点 {AnchorName} / {(Vertical ? "縦書き" : "横書き")} / {FontName} {Size:0.#}{(Bold ? " 太字" : "")}{(Italic ? " 斜体" : "")}"
        + (Math.Abs(Spacing) > 0.001 ? $" / 文字間隔 {Spacing:0.#}" : "") + (Math.Abs(LineScale - 1) > 0.001 ? $" / 行の高さ {LineScale * 100:0}%" : "")
        + (Math.Abs(Zoom - 1) > 0.001 ? $" / 拡大率 {Zoom * 100:0}%" : "") + (Wrapped ? " / 折り返しあり(折り返された行は、計算に入りません)" : "");
}
