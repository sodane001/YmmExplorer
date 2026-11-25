using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

// ====================================================================== タイムラインとシーンを、AI に渡せる形にまとめる
public static class ProjectSnapshot
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    // ---- シーンを探す(いま開いているタイムラインを含む一覧を、シーンの一覧とみなす)
    public static List<object> FindScenes(out object? owner)
    {
        owner = null;
        var main = Ymm.MainViewModel();
        var model = Ymm.MainModel();
        var current = Ymm.Timeline();
        List<object> best = new();
        object? bestOwner = null;
        int bestScore = -1;

        void Consider(object holder, IEnumerable e)
        {
            List<object> list;
            try { list = e.Cast<object?>().Take(300).Where(x => x != null).Select(x => Unwrap(x!)).ToList(); }
            catch { return; }
            if (list.Count == 0 || !list.All(LooksLikeScene)) return;
            int score = list.Count;
            if (current != null && list.Any(x => ReferenceEquals(x, current) || ReferenceEquals(TimelineOf(x), current))) score += 1000;
            if (score > bestScore) { bestScore = score; best = list; bestOwner = holder; }
        }

        var roots = new List<object?> { model, Ymm.Get(model, "Project"), main, Ymm.Get(main, "Project") };
        foreach (var r in roots.Where(r => r != null).Distinct())
            foreach (var name in new[] { "Scenes", "SceneList", "Timelines" })
                if (Ymm.Get(r, name) is IEnumerable e and not string) Consider(r!, e);

        if (bestScore < 1000)
        {
            var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var queue = new Queue<(object o, int d)>();
            foreach (var r in roots.Concat(new[] { current }).Where(r => r != null)) queue.Enqueue((r!, 0));
            int visited = 0;
            while (queue.Count > 0 && visited < 3000)
            {
                var (o, d) = queue.Dequeue();
                if (!seen.Add(o)) continue;
                visited++;
                foreach (var p in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (p.GetIndexParameters().Length != 0 || p.PropertyType.IsValueType || p.PropertyType == typeof(string)) continue;
                    var ns = p.PropertyType.Namespace ?? "";
                    if (ns.StartsWith("System.Windows", StringComparison.Ordinal)) continue;
                    object? v;
                    try { v = p.GetValue(o); } catch { continue; }
                    if (v == null) continue;
                    if (v.GetType().Name.Contains("ReactiveProperty", StringComparison.Ordinal))
                        try { v = Ymm.FindProperty(v.GetType(), "Value")?.GetValue(v); } catch { continue; }
                    if (v == null) continue;
                    if (v is IEnumerable e and not string) { Consider(o, e); continue; }
                    if (d < 4 && (v.GetType().Namespace ?? "").StartsWith("YukkuriMovieMaker", StringComparison.Ordinal)) queue.Enqueue((v, d + 1));
                }
            }
        }
        if (best.Count > 0) { owner = bestOwner; return best; }
        if (current != null) { owner = model; return new List<object> { current }; }
        return new List<object>();
    }

    static object Unwrap(object x) => Ymm.Get(x, "Model") is { } m && Ymm.Get(m, "Timeline") != null ? m : x;
    public static object TimelineOf(object scene) => Ymm.Get(scene, "Timeline") ?? scene;

    static bool LooksLikeScene(object x)
    {
        var n = x.GetType().Name;
        if (n.Contains("Item", StringComparison.Ordinal) || n.Contains("Effect", StringComparison.Ordinal)) return false;
        if (n.Contains("Scene", StringComparison.Ordinal)) return true;
        if (n.EndsWith("Timeline", StringComparison.Ordinal)) return Ymm.Get(x, "Items") != null;
        return Ymm.Get(x, "Timeline") is { } tl && tl.GetType().Name.Contains("Timeline", StringComparison.Ordinal);
    }

    public static string SceneName(object scene, int index)
    {
        foreach (var n in new[] { "Name", "Title", "SceneName" })
            if (Ymm.Get(scene, n) is string s && s.Length > 0) return s;
        return $"シーン{index + 1}";
    }

    public static bool IsCurrent(object scene)
    {
        var tl = Ymm.Timeline();
        return tl != null && (ReferenceEquals(scene, tl) || ReferenceEquals(TimelineOf(scene), tl));
    }

    /// <summary>YMM4で、そのシーンを開く</summary>
    public static bool Switch(object scene, object? owner)
    {
        if (IsCurrent(scene)) return true;
        var roots = new[] { owner, Ymm.MainModel(), Ymm.MainViewModel() }.Where(r => r != null).Distinct().ToList();
        foreach (var r in roots)
        {
            var props = r!.GetType().GetProperties(All)
                .Where(p => p.GetIndexParameters().Length == 0 && (p.Name.Contains("Active") || p.Name.Contains("Selected") || p.Name.Contains("Current")))
                .OrderByDescending(p => p.Name.Contains("Scene"));
            foreach (var p in props)
            {
                object? target;
                try { target = p.GetValue(r); } catch { continue; }
                foreach (var cand in new[] { scene, TimelineOf(scene) })
                {
                    try
                    {
                        if (target != null && target.GetType().Name.Contains("ReactiveProperty") &&
                            Ymm.FindProperty(target.GetType(), "Value") is { CanWrite: true } vp && vp.PropertyType.IsInstanceOfType(cand))
                        { vp.SetValue(target, cand); return true; }
                        if (p.GetSetMethod(true) != null && p.PropertyType != typeof(object) && p.PropertyType.IsInstanceOfType(cand))
                        { p.SetValue(r, cand); return true; }
                    }
                    catch { }
                }
            }
        }
        return false;
    }

    public static void SetFrame(object timeline, int frame)
    {
        try
        {
            var p = Ymm.FindProperty(timeline.GetType(), "CurrentFrame");
            if (p == null) return;
            var raw = p.GetValue(timeline);
            if (raw != null && raw.GetType() != typeof(int) && Ymm.FindProperty(raw.GetType(), "Value") is { CanWrite: true } vp) { vp.SetValue(raw, frame); return; }
            p.GetSetMethod(true)?.Invoke(timeline, new object[] { frame });
        }
        catch { }
    }

    // ---- 値の読み取り
    static int ToInt(object? v) { try { return v == null ? 0 : Convert.ToInt32(v, CultureInfo.InvariantCulture); } catch { return 0; } }
    static double ToDouble(object? v) { try { return v == null ? double.NaN : Convert.ToDouble(v, CultureInfo.InvariantCulture); } catch { return double.NaN; } }

    /// <summary>アニメーションする値(X・拡大率など)の、最初と最後の値</summary>
    static string AnimText(object? anim)
    {
        if (anim == null) return "";
        if (anim is double d) return Fmt(d);
        var vals = new List<double>();
        if (Ymm.Get(anim, "Values") is IEnumerable e)
            foreach (var v in e) { var x = ToDouble(Ymm.Get(v, "Value")); if (!double.IsNaN(x)) vals.Add(x); }
        if (vals.Count == 0) { var x = ToDouble(Ymm.Get(anim, "Value")); return double.IsNaN(x) ? "" : Fmt(x); }
        return vals.Count == 1 || vals.All(v => Math.Abs(v - vals[0]) < 1e-9) ? Fmt(vals[0]) : $"{Fmt(vals[0])}→{Fmt(vals[^1])}";
    }

    static string Fmt(double d) => d.ToString("0.##", CultureInfo.InvariantCulture);
    static string Clock(double secs) { var t = TimeSpan.FromSeconds(Math.Max(0, secs)); return t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss\.ff") : t.ToString(@"m\:ss\.ff"); }

    static string KindName(object item)
    {
        var n = item.GetType().Name;
        if (n.EndsWith("Item", StringComparison.Ordinal)) n = n.Substring(0, n.Length - 4);
        return n switch
        {
            "Text" => "テキスト", "Voice" => "ボイス", "Image" => "画像", "Video" => "動画", "Audio" => "音声",
            "Shape" => "図形", "Tachie" => "立ち絵", "TachieFace" => "表情", "Scene" => "シーン", "Group" => "グループ制御",
            "Effect" => "映像エフェクト", "Frame" => "画面の分割", _ => n,
        };
    }

    static string Short(string s, int max)
    {
        s = s.Replace("\r", "").Replace("\n", "⏎");
        return s.Length > max ? s.Substring(0, max) + "…" : s;
    }

    static IEnumerable<string> EffectNames(object item, string prop)
    {
        if (Ymm.Get(item, prop) is not IEnumerable list) yield break;
        foreach (var fx in list)
        {
            if (fx == null) continue;
            var label = Ymm.Get(fx, "Label") as string;
            if (string.IsNullOrEmpty(label)) label = fx.GetType().Name.Replace("Effect", "");
            if (Ymm.Get(fx, "IsEnabled") is false) label += "(無効)";
            yield return label!;
        }
    }

    static readonly HashSet<string> SummaryProps = new() { "X", "Y", "Zoom", "Opacity", "Rotation", "Volume", "FontSize", "Font", "Serif", "Text", "CharacterName", "FilePath", "Remark", "IsHidden" };

    public static List<object> ItemsOf(object timeline)
        => (Ymm.Get(timeline, "Items") as IEnumerable)?.Cast<object?>().Where(i => i != null).Select(i => i!).ToList() ?? new List<object>();

    public static double FpsOf(object timeline)
    {
        double fps = ToDouble(Ymm.Get(Ymm.Get(timeline, "VideoInfo"), "FPS") ?? Ymm.Get(Ymm.Get(timeline, "VideoInfo"), "Fps"));
        return double.IsNaN(fps) || fps <= 0 ? 60 : fps;
    }

    /// <summary>アイテムを短く(「#12 テキスト「こんにちは」」)</summary>
    public static string ItemBrief(object it, int textMax = 30)
    {
        var s = $"{AiItemIds.IdOf(it)} {KindName(it)}";
        var chara = Ymm.Get(it, "CharacterName") as string;
        if (!string.IsNullOrEmpty(chara)) s += $"({chara})";
        var text = (Ymm.Get(it, "Serif") as string) ?? (Ymm.Get(it, "Text") as string);
        if (!string.IsNullOrEmpty(text)) s += $"「{Short(text, textMax)}」";
        else if (Ymm.Get(it, "FilePath") is string f && f.Length > 0) s += " " + Path.GetFileName(f);
        return s;
    }

    /// <summary>そのフレームで画面に出ているアイテム(レイヤー順)</summary>
    public static List<object> VisibleAt(object timeline, int frame)
        => ItemsOf(timeline).Where(it => { int f = ToInt(Ymm.Get(it, "Frame")); return f <= frame && frame < f + ToInt(Ymm.Get(it, "Length")); })
                            .OrderBy(it => ToInt(Ymm.Get(it, "Layer"))).ToList();

    /// <summary>セリフ・テキストが切り替わる所(各アイテムの真ん中)のフレーム</summary>
    public static List<int> TextPoints(object timeline)
        => ItemsOf(timeline).Where(it => Ymm.Get(it, "Serif") is string s1 && s1.Length > 0 || Ymm.Get(it, "Text") is string s2 && s2.Length > 0)
                            .Select(it => ToInt(Ymm.Get(it, "Frame")) + ToInt(Ymm.Get(it, "Length")) / 2).Distinct().OrderBy(x => x).ToList();

    /// <summary>1つのシーンのタイムラインを、文章にまとめる。detailed なら、すべての項目とエフェクトの中身も書く</summary>
    public static string DescribeScene(object scene, int index, int maxItems, bool detailed)
    {
        var tl = TimelineOf(scene);
        var vi = Ymm.Get(tl, "VideoInfo") ?? Ymm.Get(scene, "VideoInfo");
        double fps = ToDouble(Ymm.Get(vi, "FPS") ?? Ymm.Get(vi, "Fps"));
        if (double.IsNaN(fps) || fps <= 0) fps = 60;
        int w = ToInt(Ymm.Get(vi, "Width")), h = ToInt(Ymm.Get(vi, "Height"));
        int hz = ToInt(Ymm.Get(vi, "Hz"));
        var items = ItemsOf(tl);
        int length = ToInt(Ymm.Get(tl, "Length"));
        if (length <= 0) foreach (var it in items) length = Math.Max(length, ToInt(Ymm.Get(it, "Frame")) + ToInt(Ymm.Get(it, "Length")));
        int layers = items.Count == 0 ? 0 : items.Max(i => ToInt(Ymm.Get(i, "Layer"))) + 1;

        var sb = new StringBuilder();
        sb.AppendLine($"## シーン{index + 1}「{SceneName(scene, index)}」" + (IsCurrent(scene) ? "(いま開いているシーン)" : ""));
        sb.AppendLine($"大きさ {w}×{h} / {Fmt(fps)} fps" + (hz > 0 ? $" / 音声 {hz} Hz" : "") + $" / 長さ {Clock(length / fps)}({length} フレーム) / アイテム {items.Count} 個 / 使っているレイヤー {(layers == 0 ? "なし" : $"00〜{layers - 1:00}")}");
        sb.AppendLine("書式: #番号 [レイヤー番号(00から)] 開始〜終了(開始フレーム+長さ) 種類 内容 | 主な値 | エフェクト" + (detailed ? "\n  └ 項目: 日本語名[英語名]=値 …(アニメーションは [キーごとの値](動き方))\n  └ fx番号/afx番号 エフェクト名: 項目…" : ""));
        var sorted = items.OrderBy(i => ToInt(Ymm.Get(i, "Frame"))).ThenBy(i => ToInt(Ymm.Get(i, "Layer"))).ToList();
        foreach (var it in sorted.Take(maxItems))
        {
            int f = ToInt(Ymm.Get(it, "Frame")), len = ToInt(Ymm.Get(it, "Length")), layer = ToInt(Ymm.Get(it, "Layer"));
            var parts = new List<string> { AiItemIds.IdOf(it), $"[L{layer:00}] {Clock(f / fps)}〜{Clock((f + len) / fps)}({f}+{len})", KindName(it) };
            var chara = Ymm.Get(it, "CharacterName") as string;
            if (!string.IsNullOrEmpty(chara)) parts.Add($"キャラ:{chara}");
            var text = (Ymm.Get(it, "Serif") as string) ?? (Ymm.Get(it, "Text") as string);
            if (!string.IsNullOrEmpty(text)) parts.Add($"「{Short(text, detailed ? 200 : 120)}」");
            var file = Ymm.Get(it, "FilePath") as string;
            if (!string.IsNullOrEmpty(file)) parts.Add($"ファイル:{Path.GetFileName(file)}");
            if (Ymm.Get(it, "IsHidden") is true) parts.Add("(非表示)");
            if (Ymm.Get(it, "IsLocked") is true) parts.Add("(ロック)");
            var props = new List<string>();
            foreach (var (name, label) in new[] { ("X", "X"), ("Y", "Y"), ("Zoom", "拡大率"), ("Opacity", "不透明度"), ("Rotation", "回転"), ("Volume", "音量"), ("FontSize", "サイズ") })
            {
                var v = AnimText(Ymm.Get(it, name));
                if (v.Length > 0) props.Add(label == name ? $"{name}={v}" : $"{label}[{name}]={v}");
            }
            var font = Ymm.Get(it, "Font") as string;
            if (!string.IsNullOrEmpty(font)) props.Add($"フォント[Font]={font}");
            if (props.Count > 0) parts.Add("| " + string.Join(" ", props));
            if (!detailed)
            {
                var fx = EffectNames(it, "VideoEffects").Concat(EffectNames(it, "AudioEffects")).ToList();
                if (fx.Count > 0) parts.Add("| エフェクト: " + string.Join("、", fx));
            }
            var remark = Ymm.Get(it, "Remark") as string;
            if (!string.IsNullOrEmpty(remark)) parts.Add($"| メモ: {Short(remark, 60)}");
            sb.AppendLine(string.Join(" ", parts));
            if (detailed)
            {
                try
                {
                    var all = AiInspector.PropsText(it, 60, SummaryProps);
                    if (all.Length > 0) sb.AppendLine("  └ 項目: " + all);
                    var kf = AiKeyframes.ReadFrames(it);
                    if (kf is { Count: > 0 })
                        sb.AppendLine("  └ 中間点: " + string.Join("、", kf.Select((kfr, ki) => $"{ki}番={kfr}フレーム({Clock(kfr / fps)})")) + "(アイテムの始めから。動く項目の値は [始点, 中間点…, 終点] の順)");
                    foreach (var (video, tag) in new[] { (true, "fx"), (false, "afx") })
                    {
                        var list = AiInspector.Effects(it, video);
                        for (int k = 0; k < list.Count; k++)
                        {
                            var fx = list[k];
                            var on = Ymm.Get(fx, "IsEnabled") is false ? "(無効)" : "";
                            sb.AppendLine($"  └ {tag}{k} {AiInspector.EffectLabel(fx)}{on}: {AiInspector.PropsText(fx, 25)}");
                        }
                    }
                }
                catch { }
            }
        }
        if (items.Count > maxItems) sb.AppendLine($"(ほか {items.Count - maxItems} 個は省略)");
        return sb.ToString();
    }

    /// <summary>プレビュー画像を JPEG ファイルにする(長い辺 maxSide px。縦横は偶数)。返すのは保存した大きさ</summary>
    public static (int w, int h)? SaveJpeg(BitmapSource? src, string path, int maxSide)
    {
        if (src == null) return null;
        try
        {
            double scale = Math.Min(1.0, (double)maxSide / Math.Max(src.PixelWidth, src.PixelHeight));
            BitmapSource img = scale < 1 ? new TransformedBitmap(src, new ScaleTransform(scale, scale)) : src;
            if (img.Format != PixelFormats.Bgr24 && img.Format != PixelFormats.Bgr32) img = new FormatConvertedBitmap(img, PixelFormats.Bgr24, null, 0);
            int w = img.PixelWidth & ~1, h = img.PixelHeight & ~1;
            if (w != img.PixelWidth || h != img.PixelHeight) img = new CroppedBitmap(img, new Int32Rect(0, 0, w, h));
            var enc = new JpegBitmapEncoder { QualityLevel = 85 };
            enc.Frames.Add(BitmapFrame.Create(img));
            using var fs = File.Create(path);
            enc.Save(fs);
            return (w, h);
        }
        catch { return null; }
    }

    /// <summary>プレビュー画像を、AI に送る大きさ(長い辺 maxSide px の JPEG)にする</summary>
    public static (string mime, string base64)? Encode(BitmapSource? src, int maxSide = 1024)
    {
        if (src == null) return null;
        try
        {
            double scale = Math.Min(1.0, (double)maxSide / Math.Max(src.PixelWidth, src.PixelHeight));
            BitmapSource img = scale < 1 ? new TransformedBitmap(src, new ScaleTransform(scale, scale)) : src;
            if (img.Format != PixelFormats.Bgr24 && img.Format != PixelFormats.Bgr32) img = new FormatConvertedBitmap(img, PixelFormats.Bgr24, null, 0);
            var enc = new JpegBitmapEncoder { QualityLevel = 82 };
            enc.Frames.Add(BitmapFrame.Create(img));
            using var ms = new MemoryStream();
            enc.Save(ms);
            return ("image/jpeg", Convert.ToBase64String(ms.ToArray()));
        }
        catch { return null; }
    }
}

// ====================================================================== チャット画面
public sealed class AiMessage : NotifyBase
{
    public string Role { get; init; } = "user";
    public bool IsUser => Role == "user";
    public bool IsAssistant => Role == "assistant";
    string text = "";
    public string Text { get => text; set => Set(ref text, value); }
    public string Who => IsUser ? "あなた" : Role == "error" ? "⚠ エラー" : "AI";
    public bool IsError => Role == "error";
    public string Attached { get; init; } = "";
    public bool HasAttached => Attached.Length > 0;
    public DateTime Time { get; init; } = DateTime.Now;
    public string TimeText => Time.ToString("HH:mm");
    /// <summary>AI に送った時の中身(画像つき)。会話の続きで送り直す</summary>
    public AiTurn? Turn { get; set; }
    /// <summary>AI からの質問の選択肢(押すと、その答えを送る)</summary>
    public List<string> Options { get; set; } = new();
    public bool HasOptions => Options.Count > 0;
    bool answered;
    public bool Answered { get => answered; set { if (Set(ref answered, value)) Raise(nameof(ShowOptions)); } }
    public bool ShowOptions => HasOptions && !answered;
}


/// <summary>AI に一緒に送るファイル・フォルダー</summary>
public sealed class AiAttachment
{
    public string Path { get; init; } = "";
    public bool IsFolder { get; init; }
    public string Name => System.IO.Path.GetFileName(Path.TrimEnd('\\', '/')) is { Length: > 0 } n ? n : Path;
    public string Icon => IsFolder ? "📁" : AiFiles.IsImage(Path) ? "🖼" : AiMedia.IsVideo(Path) ? "🎬" : AiMedia.IsAudio(Path) ? "🎵" : AiFiles.IsText(Path) ? "📄" : "📎";
    public string Label => Icon + " " + Name;
}

/// <summary>添付ファイルを、AI に送れる形(文章・画像)にする</summary>
public static class AiFiles
{
    static readonly HashSet<string> ImageExt = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".tif", ".tiff", ".ico" };
    static readonly HashSet<string> TextExt = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".md", ".csv", ".tsv", ".json", ".xml", ".srt", ".ass", ".ssa", ".vtt", ".lrc", ".ini", ".yaml", ".yml", ".log", ".ymmp",
        ".html", ".htm", ".css", ".js", ".ts", ".cs", ".py", ".lua", ".bat", ".ps1", ".toml", ".cfg", ".tex", ".rtf",
    };
    public static bool IsImage(string p) => ImageExt.Contains(System.IO.Path.GetExtension(p));
    public static bool IsText(string p) => TextExt.Contains(System.IO.Path.GetExtension(p));

    const int MaxImages = 20, MaxTextChars = 120_000, MaxFileChars = 40_000, MaxListed = 300, MaxDeferred = 8;

    static string Size(long b) => b >= 1 << 30 ? $"{b / (double)(1 << 30):0.#} GB" : b >= 1 << 20 ? $"{b / (double)(1 << 20):0.#} MB" : b >= 1024 ? $"{b / 1024.0:0.#} KB" : $"{b} B";

    static string ReadText(string path)
    {
        var bytes = File.ReadAllBytes(path);
        try { return new UTF8Encoding(false, true).GetString(bytes).TrimStart('\uFEFF'); }
        catch
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(932).GetString(bytes);     // Shift-JIS
            }
            catch { return Encoding.UTF8.GetString(bytes); }
        }
    }

    static (string mime, string base64)? LoadImage(string path, int maxSide)
    {
        try
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bi.UriSource = new Uri(path);
            bi.EndInit();
            bi.Freeze();
            return ProjectSnapshot.Encode(bi, maxSide);
        }
        catch { return null; }
    }

    /// <summary>添付を、文章と画像にまとめる</summary>
    public static (string text, List<(string, string, string)> images, List<(string path, string name)> deferred, string note) Build(IEnumerable<AiAttachment> list, int maxSide)
    {
        var sb = new StringBuilder();
        var images = new List<(string, string, string)>();
        var deferred = new List<(string path, string name)>();
        int textFiles = 0, skipped = 0;

        void AddFile(string path, string shownName)
        {
            FileInfo fi;
            try { fi = new FileInfo(path); } catch { return; }
            if (!fi.Exists) { sb.AppendLine($"### {shownName}(見つかりません)"); return; }
            if (IsImage(path))
            {
                if (images.Count >= MaxImages) { skipped++; sb.AppendLine($"- 画像 {shownName}({Size(fi.Length)}。枚数が多いため中身は省略)"); return; }
                var enc = LoadImage(path, maxSide);
                if (enc == null) { sb.AppendLine($"- 画像 {shownName}(読み込めませんでした)"); return; }
                images.Add((enc.Value.mime, enc.Value.base64, $"[添付画像] {shownName}"));
                sb.AppendLine($"- 画像 {shownName}(画像として添付)");
                return;
            }
            if (IsText(path))
            {
                if (sb.Length >= MaxTextChars || fi.Length > 20_000_000) { skipped++; sb.AppendLine($"- {shownName}({Size(fi.Length)}。長すぎるため中身は省略)"); return; }
                string text;
                try { text = ReadText(path); } catch (Exception ex) { sb.AppendLine($"- {shownName}(読めませんでした: {ex.Message})"); return; }
                int room = Math.Min(MaxFileChars, MaxTextChars - sb.Length);
                bool cut = text.Length > room;
                if (cut) text = text.Substring(0, Math.Max(0, room));
                textFiles++;
                sb.AppendLine($"### ファイル {shownName}" + (cut ? "(長いため途中まで)" : ""));
                sb.AppendLine("```");
                sb.AppendLine(text);
                sb.AppendLine("```");
                return;
            }
            if (AiMedia.IsMedia(path) || string.Equals(System.IO.Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                if (deferred.Count < MaxDeferred) { deferred.Add((path, shownName)); sb.AppendLine($"- {shownName}(下の「動画・音声・PDF」を見てください)"); }
                else { skipped++; sb.AppendLine($"- {shownName}({Size(fi.Length)}。数が多いため中身は省略)"); }
                return;
            }
            sb.AppendLine($"- {shownName}({Size(fi.Length)}、{fi.LastWriteTime:yyyy/MM/dd HH:mm}。この種類は中身を送れないため、名前と大きさだけ)");
        }

        foreach (var a in list)
        {
            if (a.IsFolder)
            {
                if (!Directory.Exists(a.Path)) { sb.AppendLine($"## フォルダー {a.Name}(見つかりません)"); continue; }
                sb.AppendLine($"## フォルダー {a.Name}");
                List<string> files;
                try
                {
                    files = Directory.EnumerateFiles(a.Path, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, MaxRecursionDepth = 6 })
                                     .Take(MaxListed + 1).OrderBy(f => f, NaturalComparer.Instance).ToList();
                }
                catch (Exception ex) { sb.AppendLine($"(読めませんでした: {ex.Message})"); continue; }
                sb.AppendLine($"中のファイル {Math.Min(files.Count, MaxListed)} 個" + (files.Count > MaxListed ? "以上(多いため途中まで)" : ""));
                foreach (var f in files.Take(MaxListed))
                    AddFile(f, System.IO.Path.GetRelativePath(a.Path, f));
            }
            else AddFile(a.Path, a.Name);
        }
        var note = $"添付 {list.Count()} 件(画像 {images.Count} 枚・文章 {textFiles} 個" + (skipped > 0 ? $"・省略 {skipped} 個" : "") + ")";
        return (sb.ToString(), images, deferred, note);
    }
}

public sealed class AiEditVm : NotifyBase
{
    readonly ExplorerSettings settings = ExplorerSettings.Instance;

    public AiEditVm()
    {
        SendCommand = new RelayCommand(async _ => await SendAsync(), _ => !busy && (input.Trim().Length > 0 || Attachments.Count > 0));
        RemoveAttachmentCommand = new RelayCommand(p => { if (p is AiAttachment a) { Attachments.Remove(a); RaiseAttach(); } });
        ClearAttachmentsCommand = new RelayCommand(_ => { Attachments.Clear(); RaiseAttach(); });
        CancelCommand = new RelayCommand(_ => cts?.Cancel());
        NewChatCommand = new RelayCommand(_ => { Messages.Clear(); chat = null; Status = "新しい会話を始めました"; Raise(nameof(HasNoMessages)); Raise(nameof(CanProposeFromChat)); Raise(nameof(ChatTitle)); });
        ContinueChatCommand = new RelayCommand(p => { if (p is AiChatRecord r) ContinueChat(r); }, _ => !busy);
        DeleteChatCommand = new RelayCommand(p =>
        {
            if (p is not AiChatRecord r) return;
            if (MessageBox.Show($"会話「{r.Title}」の記録を削除しますか?", "YmmExplorer", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            AiChatLog.Delete(r);
            if (chat?.Id == r.Id) chat = null;
            LoadHistory();
        });
        OpenMemoryCommand = new RelayCommand(_ => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(AiMemory.EnsureFile()) { UseShellExecute = true }); } catch { } });
        ClearMemoryCommand = new RelayCommand(_ =>
        {
            if (MessageBox.Show("AI が覚えていることを、すべて忘れさせますか?\n(会話の記録は消えません)", "AI の記憶", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK) return;
            AiMemory.Clear(); Raise(nameof(MemoryInfo)); Status = "AI の記憶を消しました";
        });
        OpenChatFolderCommand = new RelayCommand(_ => { try { Directory.CreateDirectory(AiChatLog.Dir); System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(AiChatLog.Dir) { UseShellExecute = true }); } catch { } });
        CopyCommand = new RelayCommand(p => { if (p is AiMessage m) try { Clipboard.SetText(m.Text); Status = "コピーしました"; } catch { } });
        ToggleSettingsCommand = new RelayCommand(_ => ShowSettings = !ShowSettings);
        OpenKeyPageCommand = new RelayCommand(_ =>
        {
            var url = Provider.KeyUrl;
            if (url.Length > 0) try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
        });
        QuickCommand = new RelayCommand(async p => { if (p is string s) { Input = s; await SendAsync(); } }, _ => !busy);
        ApproveCommand = new RelayCommand(async p => { if (p is AiProposal a) await ApproveAsync(a); });
        ShowSceneCommand = new RelayCommand(async p =>
        {
            if (p is not AiProposal a) return;
            if (!await AiActions.OpenSceneAsync(a)) { Status = "そのシーンに切り替えられませんでした"; return; }
            var tl = Ymm.Timeline();
            if (tl != null && a.JumpFrame >= 0) ProjectSnapshot.SetFrame(tl, a.JumpFrame);
            Status = "YMM4 で " + a.SceneText + " を開きました";
        });
        RejectCommand = new RelayCommand(p => { if (p is AiProposal a && a.CanRun) { a.State = "rejected"; AddLog(LogTitle(a), a.Reason, "却下", null); RaiseCounts(); } });
        ApproveAllCommand = new RelayCommand(async _ => await ApproveAllAsync(), _ => !runningAll && Proposals.Any(x => x.IsPending));
        RejectAllCommand = new RelayCommand(_ => { foreach (var a in Proposals.Where(x => x.IsPending).ToList()) { a.State = "rejected"; AddLog(LogTitle(a), a.Reason, "却下", null); } RaiseCounts(); });
        ClearDoneCommand = new RelayCommand(_ => { foreach (var a in Proposals.Where(x => !x.IsPending).ToList()) Proposals.Remove(a); RaiseCounts(); });
        UndoCommand = new RelayCommand(async p => { if (p is AiLogEntry e) await UndoAsync(e); });
        ClearLogCommand = new RelayCommand(_ => { Log.Clear(); AiLog.Save(Log); Status = "変更ログを消しました"; });
        OpenLogFolderCommand = new RelayCommand(_ => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(AiLog.Folder) { UseShellExecute = true }); } catch { } });
        showSettings = string.IsNullOrEmpty(KeyFor(ProviderId)) && AiClient.NeedsKey(ProviderId);
        AiClient.Retrying += msg => Application.Current?.Dispatcher.BeginInvoke(new Action(() => { if (busy) Status = msg; }));
        AiClient.Progress += msg => Application.Current?.Dispatcher.BeginInvoke(new Action(() => { if (busy) Status = msg; }));
        foreach (var e in AiLog.Load()) Log.Add(e);
        ResetModelChoices();
        _ = AutoFetchModelsAsync();
    }

    public ObservableCollection<AiMessage> Messages { get; } = new();
    public bool HasNoMessages => Messages.Count == 0;
    public IReadOnlyList<AiProviderInfo> Providers => AiClient.Providers;

    /// <summary>よく使う質問(押すとそのまま送る)</summary>
    public IReadOnlyList<string> QuickPrompts { get; } = new[]
    {
        "全体を見て、見やすさ・テンポの改善点を教えて",
        "字幕・テキストの読みやすさ(大きさ・色・位置・表示時間)をチェックして",
        "冒頭の数秒で視聴者を引きつけるには、どこを直せばいい?",
        "間延びしている所・カットできそうな所はある?",
        "BGM・効果音・音量のバランスについてアドバイスして",
        "各シーンの見た目(レイアウト・余白・色使い)の改善案を出して",
        "選択中のアイテムに合うエフェクトを提案して",
    };

    // ---------------------------------------------------------------- 画面の切り替え(会話 / 提案 / ログ)
    int pane;
    /// <summary>0 = 会話 / 1 = 操作の提案 / 2 = 変更ログ</summary>
    public int Pane { get => pane; set { if (Set(ref pane, value)) { Raise(nameof(IsChatPane)); Raise(nameof(IsProposalPane)); Raise(nameof(IsLogPane)); Raise(nameof(IsHistoryPane)); if (value == 3) LoadHistory(); } } }
    public bool IsHistoryPane => pane == 3;

    // ---------------------------------------------------------------- 会話の記録(専用ストレージの「AI会話」。続きから話せる)
    AiChatRecord? chat;
    public string ChatTitle => chat == null ? "" : "続きの会話: " + chat.Title;
    public ObservableCollection<AiChatRecord> History { get; } = new();
    public bool HasNoHistory => History.Count == 0;
    public string ChatFolder => AiChatLog.Dir;
    public ICommand ContinueChatCommand { get; }
    public ICommand DeleteChatCommand { get; }
    public ICommand OpenChatFolderCommand { get; }
    public ICommand OpenMemoryCommand { get; }
    public ICommand ClearMemoryCommand { get; }
    /// <summary>AI が会話をまたいで覚える(ほかのプロジェクトの会話も)。専用ストレージの「AI記憶」に保存</summary>
    public bool UseMemory { get => settings.AiUseMemory; set { settings.AiUseMemory = value; settings.Save(); Raise(nameof(UseMemory)); } }
    public string MemoryInfo => $"覚えていること {AiMemory.Load().Count} 件 ・ 保存先 {AiMemory.FilePath}";

    void LoadHistory()
    {
        History.Clear();
        foreach (var r in AiChatLog.List()) History.Add(r);
        Raise(nameof(HasNoHistory)); Raise(nameof(ChatFolder));
    }

    /// <summary>前の会話を読み込んで、続きから話す(タイムラインと画像は、次の質問の時に今のものを送る)</summary>
    void ContinueChat(AiChatRecord r)
    {
        Messages.Clear();
        foreach (var m in r.Messages)
            Messages.Add(new AiMessage { Role = m.Role, Text = m.Text, Attached = m.Attached, Time = m.Time });
        chat = r;
        Pane = 0;
        Status = $"「{r.Title}」の続きから話せます(これまでのやりとりを AI に送ります)";
        Raise(nameof(HasNoMessages)); Raise(nameof(CanProposeFromChat)); Raise(nameof(ChatTitle));
        MessageAdded?.Invoke();
    }

    /// <summary>いまの会話を保存する</summary>
    void SaveChat()
    {
        if (Messages.Count == 0) return;
        chat ??= new AiChatRecord { Source = "AIエディット(YmmExplorer)" };
        chat.Provider = Provider.Name;
        chat.Model = Model;
        if (chat.Project.Length == 0) { try { chat.Project = Ymm.ProjectName(); } catch { } }
        chat.Messages = Messages.Select(m => new AiChatLine { Role = m.Role, Text = m.Text, Attached = m.Attached, Time = m.Time }).ToList();
        AiChatLog.Save(chat);
        Raise(nameof(ChatTitle));
    }
    public bool IsChatPane => pane == 0;
    public bool IsProposalPane => pane == 1;
    public bool IsLogPane => pane == 2;

    // ---------------------------------------------------------------- 設定
    string ProviderId { get => string.IsNullOrEmpty(settings.AiProvider) ? "claude" : settings.AiProvider!; set => settings.AiProvider = value; }
    public AiProviderInfo Provider => Providers.FirstOrDefault(p => p.Id == ProviderId) ?? Providers[0];
    public int ProviderIndex
    {
        get => Math.Max(0, Providers.ToList().FindIndex(p => p.Id == ProviderId));
        set
        {
            if (value < 0 || value >= Providers.Count) return;
            if (Providers[value].Id == ProviderId) return;
            ProviderId = Providers[value].Id;
            settings.Save();
            foreach (var n in new[] { nameof(ProviderIndex), nameof(Provider), nameof(Model), nameof(ApiKey), nameof(IsCompatible), nameof(KeyHint), nameof(HasKeyPage), nameof(KeyState), nameof(IsLocalStudio), nameof(ModelNote), nameof(HasModelNote), nameof(ModelBadge), nameof(CurrentAi) }) Raise(n);
            ResetModelChoices();
            _ = AutoFetchModelsAsync();
        }
    }
    string KeyFor(string id) => settings.AiKeys != null && settings.AiKeys.TryGetValue(id, out var k) ? SecretBox.Unprotect(k) : "";

    public string ApiKey
    {
        get => KeyFor(ProviderId);
        set
        {
            settings.AiKeys ??= new Dictionary<string, string>();
            settings.AiKeys[ProviderId] = SecretBox.Protect((value ?? "").Trim());
            settings.Save();
            Raise(nameof(ApiKey)); Raise(nameof(KeyState));
            fetched.Remove(ProviderId);
            _ = AutoFetchModelsAsync();
        }
    }
    public string KeyState => ApiKey.Length > 0 ? "✓ APIキーを保存しています(このPCで暗号化して保存)" : !AiClient.NeedsKey(ProviderId) ? "" : "APIキーが未設定です";
    public string KeyHint => Provider.KeyHint;
    public bool HasKeyPage => Provider.KeyUrl.Length > 0;
    public string Model
    {
        get => settings.AiModels != null && settings.AiModels.TryGetValue(ProviderId, out var m) && m.Length > 0 ? m : Provider.DefaultModel;
        set
        {
            var v = (value ?? "").Trim();
            if (v == Model) return;
            settings.AiModels ??= new Dictionary<string, string>();
            settings.AiModels[ProviderId] = v;
            settings.Save();
            Raise(nameof(Model));
            Raise(nameof(ModelNote));
            Raise(nameof(HasModelNote));
            Raise(nameof(ModelBadge)); Raise(nameof(CurrentAi));
            if (v.Length > 0 && !ModelChoices.Contains(v)) ModelChoices.Insert(0, v);
        }
    }
    public bool IsCompatible => ProviderId == "compatible";
    /// <summary>Local AI Studio のモデルの性能・重さ(ほかの AI では空)</summary>
    public string ModelNote => AiClient.ModelNote(ProviderId, Model);
    public bool HasModelNote => ModelNote.Length > 0;
    /// <summary>性能・重さを1行で(画面の上に出す)</summary>
    public string ModelBadge => ModelNote.Split('\n')[0];
    ICommand? openLocalStudioCommand;
    /// <summary>Local AI Studio を開く(モデルのインストールなど)</summary>
    public ICommand OpenLocalStudioCommand => openLocalStudioCommand ??= new RelayCommand(async _ =>
    {
        try { Status = await LocalAiStudio.OpenAppAsync() ? "Local AI Studio を起動しました" : "Local AI Studio は起動中です(タスクバーから開けます)"; }
        catch (Exception ex) { Status = ex.Message; }
    });
    public bool IsLocalStudio => ProviderId == "localstudio";
    /// <summary>いま選んでいる AI とモデル(入力欄の下に出す)</summary>
    public string CurrentAi => $"送り先: {Provider.Name} ・ {(string.IsNullOrWhiteSpace(Model) ? "(モデル未選択)" : Model)}";

    /// <summary>モデルの候補(▼ で選ぶ)。はじめは知っているモデル、APIキーがあればサーバーの一覧に置き換える</summary>
    public ObservableCollection<string> ModelChoices { get; } = new();
    readonly Dictionary<string, List<string>> fetched = new();

    void ResetModelChoices()
    {
        ModelChoices.Clear();
        IEnumerable<string> src = fetched.TryGetValue(ProviderId, out var got) ? got : Provider.KnownModels;
        var cur = Model;
        if (cur.Length > 0 && !src.Contains(cur)) ModelChoices.Add(cur);
        foreach (var m in src) ModelChoices.Add(m);
        Raise(nameof(Model));
        Raise(nameof(ModelNote));
        Raise(nameof(HasModelNote));
        Raise(nameof(ModelBadge)); Raise(nameof(CurrentAi));
        // 一覧を作り直すと、入力できる ComboBox は表示の文字を消してしまうので、少し後でもう一度出す
        System.Windows.Application.Current?.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
            new Action(() => { Raise(nameof(Model)); Raise(nameof(CurrentAi)); }));
    }

    /// <summary>APIキーがあれば、黙ってモデルの一覧を読んでおく(失敗しても何も言わない)</summary>
    async Task AutoFetchModelsAsync()
    {
        var id = ProviderId;
        if (fetched.ContainsKey(id) || (ApiKey.Length == 0 && AiClient.NeedsKey(id))) return;
        try
        {
            using var c = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            var list = await AiClient.ListModelsAsync(id, ApiKey, BaseUrl, c.Token);
            if (list.Count == 0) return;
            fetched[id] = list;
            if (id == ProviderId) ResetModelChoices();
        }
        catch { }
    }

    ICommand? fetchModelsCommand;
    public ICommand FetchModelsCommand => fetchModelsCommand ??= new RelayCommand(async _ =>
    {
        Status = "モデルの一覧を読んでいます…";
        try
        {
            var list = await AiClient.ListModelsAsync(ProviderId, ApiKey, BaseUrl, CancellationToken.None);
            if (list.Count > 0) fetched[ProviderId] = list;
            ResetModelChoices();
            Status = list.Count > 0 ? $"{list.Count} 個のモデルが見つかりました。上の「モデル」の ▼ から選べます" : "使えるモデルが見つかりませんでした";
        }
        catch (Exception ex) { Status = "モデルの一覧を読めませんでした: " + ex.Message; }
    });
    public string BaseUrl { get => settings.AiBaseUrl ?? "http://localhost:11434/v1"; set { settings.AiBaseUrl = value?.Trim(); settings.Save(); Raise(nameof(BaseUrl)); fetched.Remove("compatible"); } }

    public bool SendTimeline { get => settings.AiSendTimeline; set { settings.AiSendTimeline = value; settings.Save(); Raise(nameof(SendTimeline)); } }
    /// <summary>タイムラインの詳しさ: 0 = ふつう / 1 = すべての項目とエフェクトの中身</summary>
    public int Detail { get => Math.Clamp(settings.AiDetail, 0, 1); set { settings.AiDetail = value; settings.Save(); Raise(nameof(Detail)); } }
    public IReadOnlyList<string> DetailModes { get; } = new[] { "タイムライン: ふつう(主な値だけ)", "タイムライン: 詳しく(すべての項目とエフェクトの中身)" };
    /// <summary>プレビューを送る: 0 = 送らない / 1 = いまのシーン / 2 = すべてのシーン</summary>
    public int PreviewMode { get => Math.Clamp(settings.AiPreviewMode, 0, 2); set { settings.AiPreviewMode = value; settings.Save(); Raise(nameof(PreviewMode)); } }
    public IReadOnlyList<string> PreviewModes { get; } = new[] { "プレビューは送らない", "いまのシーンのプレビュー", "すべてのシーンのプレビュー" };
    /// <summary>1つのシーンから撮るコマ数</summary>
    public int FramesPerScene { get => Math.Clamp(settings.AiFramesPerScene, 1, 12); set { settings.AiFramesPerScene = Math.Clamp(value, 1, 12); settings.Save(); Raise(nameof(FramesPerScene)); } }
    public int PreviewSize { get => Math.Clamp(settings.AiPreviewSize, 0, 2); set { settings.AiPreviewSize = value; settings.Save(); Raise(nameof(PreviewSize)); } }
    public IReadOnlyList<string> PreviewSizes { get; } = new[] { "画像: 小(768px・軽い)", "画像: 中(1024px)", "画像: 大(1536px・細かい文字も読める)" };
    int PreviewPixels => PreviewSize switch { 0 => 768, 2 => 1536, _ => 1024 };
    public int PreviewPoints { get => Math.Clamp(settings.AiPreviewPoints, 0, 2); set { settings.AiPreviewPoints = value; settings.Save(); Raise(nameof(PreviewPoints)); Raise(nameof(IsContinuous)); } }
    public IReadOnlyList<string> PreviewPointModes { get; } = new[] { "撮る場所: はじめ〜おわりを均等に", "撮る場所: セリフ・テキストごと", "撮る場所: とぎれなく全部(動画にして送る)" };
    public bool IsContinuous => PreviewPoints == 2;
    /// <summary>とぎれなく撮る時の、1秒あたりのコマ数(0 = 全フレーム)</summary>
    public int ContinuousFps { get => Math.Clamp(settings.AiContinuousFps, 0, 60); set { settings.AiContinuousFps = value; settings.Save(); Raise(nameof(ContinuousFps)); Raise(nameof(ContinuousIndex)); } }
    static readonly int[] ContinuousSteps = { 1, 2, 5, 10, 15, 30, 0 };
    public IReadOnlyList<string> ContinuousChoices { get; } = new[] { "1秒に1コマ", "1秒に2コマ", "1秒に5コマ", "1秒に10コマ", "1秒に15コマ", "1秒に30コマ", "全フレーム(とても時間がかかります)" };
    public int ContinuousIndex { get => Math.Max(0, Array.IndexOf(ContinuousSteps, ContinuousFps)); set { if (value >= 0 && value < ContinuousSteps.Length) ContinuousFps = ContinuousSteps[value]; } }
    public string ExtraInstruction { get => settings.AiExtraInstruction ?? ""; set { settings.AiExtraInstruction = value; settings.Save(); Raise(nameof(ExtraInstruction)); } }
    /// <summary>動画を添付した時に取り出すコマの数(Gemini に動画そのものを送れた時は使わない)</summary>
    public int MediaFrames { get => Math.Clamp(settings.AiMediaFrames, 0, 24); set { settings.AiMediaFrames = Math.Clamp(value, 0, 24); settings.Save(); Raise(nameof(MediaFrames)); } }
    /// <summary>ChatGPT の時、動画・音声を文字起こしして送る(OpenAI の文字起こしは別料金)</summary>
    public bool Transcribe { get => settings.AiTranscribe; set { settings.AiTranscribe = value; settings.Save(); Raise(nameof(Transcribe)); } }

    /// <summary>PDF: Claude・Gemini・ChatGPT にはファイルのまま送る。ほかの AI には、ページを画像にして送る</summary>
    async Task<string> PdfPartAsync(string path, string name, List<(string, string, string)> images, List<AiFilePart> files, int maxSide)
    {
        try
        {
            var len = new FileInfo(path).Length;
            if (ProviderId is "claude" or "gemini" or "openai" && len <= 20_000_000)
            {
                var b64 = Convert.ToBase64String(await File.ReadAllBytesAsync(path));
                files.Add(new AiFilePart { Mime = "application/pdf", Base64 = b64, FileName = System.IO.Path.GetFileName(path), Caption = $"[添付PDF] {name}" });
                return $"### PDF {name}(ファイルのまま添付しています)";
            }
            Status = $"「{name}」のページを画像にしています…";
            var doc = await PdfDoc.OpenAsync(path);
            int pages = Math.Min(doc.PageCount, 10);
            for (int i = 0; i < pages; i++)
            {
                var enc = ProjectSnapshot.Encode(await doc.RenderAsync(i, maxSide), maxSide);
                if (enc != null) images.Add((enc.Value.mime, enc.Value.base64, $"[添付PDF] {name} の {i + 1} ページ目"));
            }
            return $"### PDF {name}(全 {doc.PageCount} ページ。{pages} ページを画像にして添付しています)";
        }
        catch (Exception ex) { return $"### PDF {name}(読めませんでした: {ex.Message})"; }
    }

    /// <summary>提案の時に、素材データベース・テンプレート・プリセット・許可したフォルダーの一覧を AI に見せる(配置の提案に使う)</summary>
    public bool ShareLibrary { get => settings.AiShareLibrary; set { settings.AiShareLibrary = value; settings.Save(); Raise(nameof(ShareLibrary)); } }

    /// <summary>AI に見せてよい素材フォルダー(設定で許可したものだけ)</summary>
    public ObservableCollection<string> AllowedFolders => allowedFolders ??= new ObservableCollection<string>(settings.AiAllowedFolders ?? new List<string>());
    ObservableCollection<string>? allowedFolders;
    public bool HasNoAllowedFolders => AllowedFolders.Count == 0;
    public void AddAllowedFolders(IEnumerable<string> folders)
    {
        foreach (var f in folders)
            if (Directory.Exists(f) && !AllowedFolders.Any(x => string.Equals(x, f, StringComparison.OrdinalIgnoreCase))) AllowedFolders.Add(f);
        SaveAllowed();
    }
    ICommand? removeAllowedCommand;
    public ICommand RemoveAllowedFolderCommand => removeAllowedCommand ??= new RelayCommand(p => { if (p is string f) { AllowedFolders.Remove(f); SaveAllowed(); } });
    void SaveAllowed() { settings.AiAllowedFolders = AllowedFolders.ToList(); settings.Save(); Raise(nameof(HasNoAllowedFolders)); }

    /// <summary>AI がインターネットを検索・閲覧する</summary>
    public bool WebOn
    {
        get => settings.AiWebAccess;
        set
        {
            settings.AiWebAccess = value; settings.Save(); Raise(nameof(WebOn));
            Status = value ? (ProviderId switch
            {
                "claude" => "ネット検索をオンにしました。Claude が必要な時に検索・ページ閲覧をします(Claude の検索は、1回ごとに少し料金がかかります)",
                "gemini" => "ネット検索をオンにしました。Gemini が必要な時に Google 検索をします",
                "localstudio" => "ネット検索をオンにしました。Local AI Studio が検索・ページ閲覧をして、AI に渡します",
                _ => "ネット検索をオンにしました。このソフトが検索・ページ閲覧の道具を用意して、AI に使ってもらいます",
            }) : "ネット検索をオフにしました";
        }
    }

    /// <summary>AI による操作の提案</summary>
    public bool ProposalsOn
    {
        get => settings.AiProposals;
        set
        {
            settings.AiProposals = value; settings.Save(); Raise(nameof(ProposalsOn)); Raise(nameof(CanProposeFromChat));
            // 会話の途中でオンにした時: 次の質問から提案が届く。これまでのアドバイスを提案にするボタンも出す
            if (value && Messages.Any(m => m.IsAssistant)) Status = "操作の提案をオンにしました。次の質問から提案が届きます。「これまでのアドバイスを提案にする」で、ここまでの話を提案にすることもできます";
        }
    }

    /// <summary>会話の途中で提案をオンにした時に、ここまでのアドバイスを提案にしてもらう</summary>
    public bool CanProposeFromChat => ProposalsOn && Messages.Any(m => m.IsAssistant);

    // ---------------------------------------------------------------- 指示モード(「〜して」と頼まれた時は、そのまま実行できる操作を出す)
    /// <summary>この質問が「操作の指示」と判断された(提案がオフでも、この1回は提案を出す)</summary>
    bool commandTurn;
    bool UseProposals => ProposalsOn || commandTurn;
    /// <summary>AI が質問で返した指示(答えを送る時も、指示として扱う)</summary>
    bool pendingCommand;

    static readonly System.Text.RegularExpressions.Regex QuestionBlock = new(@"```[ \t]*ymm-question[ \t]*\r?\n?(.*?)(?:```|\z)", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    /// <summary>返事から、AI の質問(ymm-question)を取り出す</summary>
    static (string? question, List<string> options) ExtractQuestion(ref string text)
    {
        var m = QuestionBlock.Match(text);
        if (!m.Success) return (null, new List<string>());
        text = QuestionBlock.Replace(text, "").TrimEnd();
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(m.Groups[1].Value.Trim(), documentOptions: new System.Text.Json.JsonDocumentOptions { AllowTrailingCommas = true });
            if (node is System.Text.Json.Nodes.JsonArray arr) node = arr.FirstOrDefault();
            var q = node?["question"]?.GetValue<string>() ?? "";
            var opts = (node?["options"] as System.Text.Json.Nodes.JsonArray)?.Select(o => o?.ToString() ?? "").Where(o => o.Length > 0).Take(6).ToList() ?? new List<string>();
            return (q.Length > 0 ? q : "どうしますか?", opts);
        }
        catch { return (m.Groups[1].Value.Trim(), new List<string>()); }
    }

    ICommand? answerCommand;
    /// <summary>AI の質問の選択肢を押した</summary>
    public ICommand AnswerCommand => answerCommand ??= new RelayCommand(async p => { if (p is string s && !busy) { Input = s; await SendAsync(); } }, _ => !busy);

    /// <summary>指示された操作は、OK を押さずにすぐ実行する(変更ログから元に戻せます)</summary>
    public bool AutoRunCommands { get => settings.AiAutoRunCommands; set { settings.AiAutoRunCommands = value; settings.Save(); Raise(nameof(AutoRunCommands)); } }
    /// <summary>YMM4専用ストレージの中身(すべてのファイルの一覧)を AI に見せ、読みたいファイルを読ませる</summary>
    public bool ShareStorage { get => settings.AiShareStorage; set { settings.AiShareStorage = value; settings.Save(); Raise(nameof(ShareStorage)); } }
    /// <summary>「〜して」を指示として扱う</summary>
    public bool DetectCommands { get => settings.AiDetectCommands; set { settings.AiDetectCommands = value; settings.Save(); Raise(nameof(DetectCommands)); } }

    static readonly string[] CommandWords =
    {
        "して", "作って", "つくって", "追加", "加えて", "付けて", "つけて", "動かして", "移動", "配置", "置いて", "変えて", "変更", "消して", "削除",
        "入れて", "揃えて", "そろえて", "伸ばして", "縮めて", "広げて", "合わせて", "ずらして", "並べて", "かけて", "フェード", "アニメーション", "にする", "にして",
        "やって", "お願い", "直して", "複製", "コピーして", "下さい", "ください",
    };
    static readonly string[] QuestionWords = { "教えて", "どう", "どこ", "なぜ", "何が", "何を", "？", "?", "アドバイス", "改善点", "チェック", "意見", "ありますか", "でしょうか", "かな" };

    /// <summary>質問ではなく、操作の指示か(例:「#12 を左から右へ動かして」「テキストにフェードを付けて」)</summary>
    public static bool LooksLikeCommand(string text)
    {
        var t = text.Trim();
        if (t.StartsWith("!") || t.StartsWith("！")) return true;      // 先頭に ! を付けると、必ず指示
        if (QuestionWords.Any(w => t.Contains(w))) return false;
        return CommandWords.Any(w => t.Contains(w));
    }
    ICommand? proposeFromChatCommand;
    public ICommand ProposeFromChatCommand => proposeFromChatCommand ??= new RelayCommand(async _ =>
    {
        if (busy) return;
        Input = "ここまでの会話で出したアドバイスのうち、YMM4の項目を変えれば実行できるものを、いまのタイムライン(#番号)に合わせて、操作の提案(```ymm-actions のブロック)にしてください。文章の説明は短くてかまいません。";
        await SendAsync();
    }, _ => !busy);

    bool showSettings;
    public bool ShowSettings { get => showSettings; set => Set(ref showSettings, value); }

    // ---------------------------------------------------------------- 送信
    string input = "";
    public string Input { get => input; set { if (Set(ref input, value)) CommandManager.InvalidateRequerySuggested(); } }
    bool busy;
    public bool IsBusy { get => busy; private set { Set(ref busy, value); CommandManager.InvalidateRequerySuggested(); } }
    string status = "";
    public string Status { get => status; set => Set(ref status, value); }
    CancellationTokenSource? cts;

    public ICommand SendCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand NewChatCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand ToggleSettingsCommand { get; }
    public ICommand OpenKeyPageCommand { get; }
    public ICommand QuickCommand { get; }
    public ICommand RemoveAttachmentCommand { get; }
    public ICommand ClearAttachmentsCommand { get; }

    // ---------------------------------------------------------------- 添付(ファイル・フォルダー)
    public ObservableCollection<AiAttachment> Attachments { get; } = new();
    public bool HasAttachments => Attachments.Count > 0;
    void RaiseAttach() { Raise(nameof(HasAttachments)); CommandManager.InvalidateRequerySuggested(); }

    /// <summary>ファイル・フォルダーを添付に足す(同じものは足さない)</summary>
    public void AddAttachments(IEnumerable<string> paths)
    {
        int added = 0;
        foreach (var raw in paths)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var path = raw.Trim();
            bool folder = Directory.Exists(path);
            if (!folder && !File.Exists(path)) continue;
            if (Attachments.Any(a => string.Equals(a.Path, path, StringComparison.OrdinalIgnoreCase))) continue;
            Attachments.Add(new AiAttachment { Path = path, IsFolder = folder });
            added++;
        }
        if (added > 0) Status = $"{added} 件を添付しました(送信すると、質問と一緒に AI に送ります)";
        RaiseAttach();
    }

    public ICommand ApproveCommand { get; }
    public ICommand ShowSceneCommand { get; }
    public ICommand RejectCommand { get; }
    public ICommand RejectAllCommand { get; }
    public ICommand ApproveAllCommand { get; }
    bool runningAll;

    /// <summary>確認待ちの提案を、上(新しいもの)から順にすべて実行する</summary>
    async Task ApproveAllAsync()
    {
        var list = Proposals.Where(x => x.IsPending).ToList();
        if (list.Count == 0) return;
        if (MessageBox.Show($"確認待ちの提案 {list.Count} 件を、まとめて実行します。\n(実行したものは「変更ログ」から1つずつ元に戻せます)\n\nよろしいですか?",
                "YmmExplorer", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        runningAll = true;
        CommandManager.InvalidateRequerySuggested();
        int ok = 0, ng = 0;
        try
        {
            for (int i = 0; i < list.Count; i++)
            {
                Status = $"まとめて実行しています…({i + 1}/{list.Count})";
                await ApproveAsync(list[i]);
                if (list[i].State == "done") ok++; else ng++;
            }
            Status = $"まとめて実行しました: 成功 {ok} 件" + (ng > 0 ? $"・できなかった {ng} 件(理由は各提案に出ています)" : "");
        }
        finally { runningAll = false; CommandManager.InvalidateRequerySuggested(); }
    }
    public ICommand ClearDoneCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ClearLogCommand { get; }
    public ICommand OpenLogFolderCommand { get; }

    /// <summary>画面がスクロールを一番下へ送るための合図</summary>
    public event Action? MessageAdded;

    string SystemPrompt()
    {
        var sb = new StringBuilder();
        sb.AppendLine("あなたは「ゆっくりMovieMaker4」(YMM4。ゆっくり実況・解説動画などを作る日本の動画編集ソフト)に詳しい、動画編集のアドバイザーです。");
        sb.AppendLine("ユーザーの質問と一緒に、プロジェクトのタイムライン(シーン・アイテムの一覧)と、プレビュー画像が届きます。");
        sb.AppendLine("それらを根拠に、具体的で実行しやすい編集のアドバイスを日本語で答えてください。");
        sb.AppendLine("・どのシーンの、何秒あたりの、どのアイテム(#番号・レイヤー番号)の話かをはっきり書く");
        sb.AppendLine("・YMM4で実際に行う操作(どの項目をいくつにする、どのエフェクトを足す など)まで書く");
        sb.AppendLine("・大事な順に、箇条書きで。良い所も短く伝える");
        sb.AppendLine("・届いたデータから分からないことは推測と分かるように書き、決めつけない");
        sb.AppendLine();
        sb.AppendLine("## 分からない時は、質問で返す");
        sb.AppendLine("依頼の大事な部分が決まっていない時(どのアイテム・どのシーンか分からない、候補が複数ある、やりたいことが2通り以上に取れる、必要な素材が無い など)は、推測で操作せず、先に短く質問してください。");
        sb.AppendLine("・その時は操作のブロック(ymm-actions)は書かず、返事の最後に次の形で質問を1つだけ書く。選択肢は2〜5個の短い文で、ユーザーがそのまま選べるもの");
        sb.AppendLine("```ymm-question");
        sb.AppendLine("{\"question\":\"どのテキストを動かしますか?\",\"options\":[\"#12「タイトル」\",\"#15「サブタイトル」\",\"両方\"]}");
        sb.AppendLine("```");
        sb.AppendLine("・数値の細かい所(何ピクセル・何秒・何%など)は質問せず、見やすい・自然な値を自分で決める。質問は、決めないと進められないことだけにする");
        sb.AppendLine();
        sb.AppendLine("## YMM4 の基本");
        sb.AppendLine("・タイムラインはレイヤーとフレームでできている。レイヤー番号は YMM4 の画面と同じく 00 から始まる(L00 が一番上の行で一番奥。番号が大きいほど手前に重なる)。1秒 = fps フレーム\n・レイヤーを言う時は、必ずここに書いてある番号(L00・L03 など)をそのまま使い、1 を足したり引いたりしない");
        sb.AppendLine("・アイテムの種類: ボイス(キャラクターのセリフ。字幕と音声と立ち絵の口パク)、テキスト、画像、動画、音声、図形、立ち絵、表情、シーン(別シーンの埋め込み)、グループ制御、映像エフェクト(下のレイヤー全体に効く)、画面の分割");
        sb.AppendLine("・X・Y は画面の中心を 0 とした位置(px)。拡大率・不透明度は %。回転は度。音量は %");
        sb.AppendLine("・値は「アニメーション」にできる(キーごとの値と、直線移動・加減速などの動き方)");
        sb.AppendLine("・映像エフェクト・音声エフェクトはアイテムごとに順番に重ねがけされる(上から順に効く)");
        sb.AppendLine("・プレビュー画像の説明には、その瞬間に画面に出ているアイテムの #番号 が書いてある。画像と #番号 を照らし合わせて答える");
        sb.AppendLine();
        sb.AppendLine(Ymm4Knowledge.Text);
        if (UseProposals)
        {
            string names = "";
            try { names = AiActions.EffectNamesText(); } catch { }
            sb.Append(AiActions.Instructions(names));
        }
        if (AiStorage.Root != null) sb.AppendLine().AppendLine(AiStorage.Instructions);
        if (UseMemory) { try { sb.AppendLine().AppendLine(AiMemory.Prompt(chat?.Id, Ymm.ProjectName())); } catch { } }
        if (!string.IsNullOrWhiteSpace(ExtraInstruction)) sb.AppendLine("\nユーザーからの追加の指示: " + ExtraInstruction.Trim());
        return sb.ToString();
    }

    async Task SendAsync()
    {
        var question = input.Trim();
        if (busy) return;
        if (question.Length == 0)
        {
            if (Attachments.Count == 0) return;
            question = "添付したものを見て、編集に役立つことを教えて";
        }
        var key = ApiKey;
        if (key.Length == 0 && AiClient.NeedsKey(ProviderId)) { Status = $"{Provider.Name} の APIキーを、⚙ 設定に入れてください"; ShowSettings = true; return; }
        if (string.IsNullOrWhiteSpace(Model)) { Status = "モデルを選んでください"; ShowSettings = true; return; }

        IsBusy = true;
        Pane = 0;
        cts = new CancellationTokenSource();
        Input = "";
        commandTurn = (DetectCommands && LooksLikeCommand(question)) || pendingCommand;
        pendingCommand = false;
        foreach (var m in Messages) if (m.HasOptions) m.Answered = true;
        if (question.StartsWith("!") || question.StartsWith("！")) question = question.Substring(1).Trim();
        try
        {
            // 1) タイムラインとプレビューを集める
            Status = "タイムラインとプレビューを集めています…";
            var (context, images, attached, previewFiles) = await CollectAsync(cts.Token);
            var turnText = new StringBuilder();
            var files = new List<AiFilePart>(previewFiles);
            if (context.Length > 0) turnText.AppendLine("# いまのプロジェクト\n" + context + "\n");
            var sentFiles = Attachments.ToList();
            if (sentFiles.Count > 0)
            {
                Status = "添付したファイルを読み込んでいます…";
                var maxSide = Math.Max(1024, PreviewPixels);
                var (ftext, fimages, deferred, fnote) = await Task.Run(() => AiFiles.Build(sentFiles, maxSide), cts.Token);
                turnText.AppendLine("# ユーザーが添付したファイル・フォルダー\n" + ftext + "\n");
                images.AddRange(fimages);
                if (deferred.Count > 0)
                {
                    turnText.AppendLine("## 動画・音声・PDF");
                    foreach (var (path, name) in deferred)
                    {
                        if (AiMedia.IsMedia(path))
                        {
                            var r = await AiMedia.DescribeAsync(path, name, ProviderId, key, Transcribe, MediaFrames, maxSide, s => Status = s, cts.Token);
                            turnText.AppendLine(r.Text.ToString());
                            images.AddRange(r.Images);
                            files.AddRange(r.Files);
                        }
                        else turnText.AppendLine(await PdfPartAsync(path, name, images, files, maxSide));
                    }
                }
                attached = attached.Length > 0 ? attached + "・" + fnote : fnote;
                attached += ":" + string.Join("、", sentFiles.Take(8).Select(a => a.Name)) + (sentFiles.Count > 8 ? " ほか" : "");
                Attachments.Clear();
                RaiseAttach();
            }
            turnText.AppendLine("# 質問\n" + question);
            if (commandTurn)
                turnText.AppendLine("\n(これは操作の指示です。アドバイスではなく、指示どおりに YMM4 を操作する内容を、返事の最後に ```ymm-actions のブロックですべて書いてください。" +
                                    "アニメーションなら動き方・始点・終点(必要なら中間点)まで、数値は妥当なものを自分で決めてください。文章は、何をするかを1〜3行で短く。#番号・項目名は「いまのプロジェクト」のものを使ってください。" +
                                    "ただし、どのアイテムか分からない・候補が複数ある・やりたいことが2通り以上に取れる時は、操作を書かずに ```ymm-question で質問してください)");
            else if (UseProposals)
                turnText.AppendLine("\n(操作の提案がオンです。これまでの返事に提案のブロックが無くても、今回の返事では、実行できる変更を必ず最後に ```ymm-actions のブロックで書いてください。#番号・項目名は、上の「いまのプロジェクト」に書いてあるものを使ってください)");
            var turn = new AiTurn { Role = "user", Text = turnText.ToString(), Images = images, Files = files };
            var userMsg = new AiMessage { Role = "user", Text = question, Attached = attached, Turn = turn };
            Messages.Add(userMsg);
            Raise(nameof(HasNoMessages));
            MessageAdded?.Invoke();

            // 2) 会話を組み立てる(画像とタイムラインは、いちばん新しい質問にだけ付ける。前の質問は文章だけ)
            var turns = new List<AiTurn>();
            foreach (var m in Messages.Where(m => m.Role is "user" or "assistant"))
            {
                if (ReferenceEquals(m, userMsg)) turns.Add(turn);
                else turns.Add(new AiTurn { Role = m.Role, Text = m.Role == "user" ? "# 質問\n" + m.Text : m.Text });
            }

            Status = $"{Provider.Name} に送っています…(画像 {images.Count} 枚" + (files.Count > 0 ? $"・ファイル {files.Count} 個" : "") + ")";
            var reply = await AiClient.SendAsync(ProviderId, Model, key, BaseUrl, SystemPrompt(), turns, cts.Token, web: WebOn && IsLocalStudio);
            var text = string.IsNullOrWhiteSpace(reply) ? "(返事が空でした)" : reply.Trim();

            // AI が専用ストレージのファイルを読みたいと言ったら(ymm-read)、読んで続けて送る(3回まで)
            var readNames = new List<string>();
            var memoryNotes = new List<string>();
            var project = UseMemory ? Ymm.ProjectName() : "";
            for (int round = 0; round < 3; round++)
            {
                var probe = text;
                if (UseMemory) { var mn = AiMemory.ExtractAndApply(ref probe, project); if (mn.Length > 0) memoryNotes.Add(mn); }
                var reads = AiStorage.ExtractReads(ref probe);
                if (reads.Count == 0) break;
                Status = $"AI が専用ストレージのファイルを読んでいます…({reads.Count} 個)";
                var rside = Math.Max(1024, PreviewPixels);
                var atts = reads.Select(r => new AiAttachment { Path = r, IsFolder = Directory.Exists(r) }).ToList();
                var (rtext, rimages, rdeferred, _) = await Task.Run(() => AiFiles.Build(atts, rside), cts.Token);
                var rfiles = new List<AiFilePart>();
                var body = new StringBuilder("# 読んだファイル(ymm-read で頼まれたもの)\n" + rtext);
                foreach (var (path, name) in rdeferred)
                {
                    if (AiMedia.IsMedia(path))
                    {
                        var r = await AiMedia.DescribeAsync(path, name, ProviderId, key, Transcribe, MediaFrames, rside, s => Status = s, cts.Token);
                        body.AppendLine(r.Text.ToString());
                        rimages.AddRange(r.Images);
                        rfiles.AddRange(r.Files);
                    }
                    else body.AppendLine(await PdfPartAsync(path, name, rimages, rfiles, rside));
                }
                readNames.AddRange(reads.Select(r => Path.GetFileName(r) ?? r));
                turns.Add(new AiTurn { Role = "assistant", Text = probe.Length > 0 ? probe : "(ファイルを読みます)" });
                turns.Add(new AiTurn { Role = "user", Text = body + "\n\n上のファイルを読んだうえで、さっきの質問・指示に答えてください。", Images = rimages, Files = rfiles });
                Status = $"{Provider.Name} に送っています…(読んだファイルと一緒に)";
                reply = await AiClient.SendAsync(ProviderId, Model, key, BaseUrl, SystemPrompt(), turns, cts.Token, web: WebOn && IsLocalStudio);
                text = string.IsNullOrWhiteSpace(reply) ? "(返事が空でした)" : reply.Trim();
            }
            AiStorage.ExtractReads(ref text);
            if (UseMemory) { var mn = AiMemory.ExtractAndApply(ref text, project); if (mn.Length > 0) memoryNotes.Add(mn); Raise(nameof(MemoryInfo)); }

            // 3) 操作の提案を取り出す
            var found = AiActions.Extract(text, out var cleaned, out var perr);
            var (question2, options) = ExtractQuestion(ref cleaned);

            // 提案がオンなのに、返事に提案のブロックが無かった時: 提案だけを JSON で出してもらう(画像は送り直さない)
            if (UseProposals && found.Count == 0 && question2 == null)
            {
                try
                {
                    Status = "返事に提案が無かったので、操作の提案だけを作ってもらっています…";
                    var follow = turns.Select(t => new AiTurn { Role = t.Role, Text = t.Text }).ToList();
                    follow.Add(new AiTurn { Role = "assistant", Text = text });
                    follow.Add(new AiTurn { Role = "user", Text = "いまの返事の内容のうち、YMM4で実行できる変更・配置を、操作の提案の JSON 配列だけで出力してください。文章や説明は書かず、[ で始まり ] で終わる JSON だけにしてください。各要素の形は、システムの指示にある ymm-actions の例と同じです(op・item・prop・value・source・frame・layer・reason など)。実行できるものが無ければ [] を返してください。" });
                    var j = (await AiClient.SendAsync(ProviderId, Model, key, BaseUrl, SystemPrompt(), follow, cts.Token, json: true)).Trim();
                    var wrapped = j.Contains("```") ? j : "```ymm-actions\n" + j + "\n```";
                    var more = AiActions.Extract(wrapped, out _, out var perr2);
                    if (more.Count > 0) { found = more; perr = null; }
                    else if (perr2 != null) perr = perr2;
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { perr = "提案を作ってもらえませんでした: " + ex.Message; }
            }

            if (found.Count > 0)
            {
                // 新しい提案を一覧の上に(届いた順のまま)
                for (int k = found.Count - 1; k >= 0; k--) Proposals.Insert(0, found[k]);
                cleaned += $"\n\n🛠 操作の提案が {found.Count} 件あります →「提案」で確認して、OKしたものだけ実行されます";
                RaiseCounts();
            }
            if (perr != null) cleaned += "\n\n⚠ " + perr;
            else if (UseProposals && found.Count == 0 && question2 == null) cleaned += "\n\n(今回の返事には、操作の提案はありませんでした)";
            if (question2 != null)
            {
                cleaned = (cleaned.Length > 0 ? cleaned + "\n\n" : "") + "❓ " + question2 + (options.Count > 0 ? "\n(下のボタンで選ぶか、入力欄に答えを書いてください)" : "");
                pendingCommand = commandTurn;     // 答えも、同じ指示の続きとして送る
            }
            else pendingCommand = false;
            if (readNames.Count > 0) cleaned += "\n\n📂 読んだファイル: " + string.Join("、", readNames.Distinct().Take(20));
            if (memoryNotes.Count > 0) cleaned += "\n\n" + string.Join("\n", memoryNotes);
            Messages.Add(new AiMessage { Role = "assistant", Text = cleaned.Length > 0 ? cleaned : text, Options = options });
            Raise(nameof(CanProposeFromChat));
            Status = found.Count > 0 ? $"操作の提案が {found.Count} 件届きました" : "";
            if (commandTurn && found.Count > 0)
            {
                if (AutoRunCommands)
                {
                    // 指示された操作を、そのまま実行する(届いた順に)
                    int ok = 0;
                    foreach (var a in found)
                    {
                        Status = $"指示された操作を実行しています…({ok + 1}/{found.Count})";
                        await ApproveAsync(a);
                        if (a.State == "done") ok++;
                    }
                    var ng = found.Count - ok;
                    Messages.Add(new AiMessage { Role = "assistant", Text = $"✓ 指示された操作を {ok} 件実行しました" + (ng > 0 ? $"(できなかったもの {ng} 件は「提案」に理由が出ています)" : "") + "。「変更ログ」から元に戻せます。" });
                    Status = $"指示された操作を {ok} 件実行しました";
                }
                else
                {
                    Pane = 1;      // 提案の一覧を開いて、すぐ OK を押せるように
                    Status = $"指示された操作が {found.Count} 件あります。内容を確かめて「OK・実行」(まとめてなら「一括で実行」)を押してください";
                }
            }
        }
        catch (OperationCanceledException) { Status = "取りやめました"; }
        catch (Exception ex)
        {
            var text = ex.Message;
            // 「このモデルは使えません。代わりに ○○ を」と返ってきた時は、モデル名を自動で切り替える
            if (text.Contains("(AI からの返事 404") || text.Contains("no longer available", StringComparison.OrdinalIgnoreCase))
            {
                var suggested = AiClient.SuggestedModel(text);
                if (suggested != null && suggested != Model)
                {
                    Model = suggested;
                    text += $"\n\n→ モデルを「{suggested}」に切り替えました。もう一度送ってください。";
                }
            }
            Messages.Add(new AiMessage { Role = "error", Text = text });
            Status = "";
        }
        finally
        {
            IsBusy = false;
            Raise(nameof(HasNoMessages));
            MessageAdded?.Invoke();
            SaveChat();
        }
    }

    // ---------------------------------------------------------------- 操作の提案
    public ObservableCollection<AiProposal> Proposals { get; } = new();
    public int PendingCount => Proposals.Count(p => p.IsPending);
    public string ProposalTabText => PendingCount > 0 ? $"提案 ({PendingCount})" : "提案";
    public bool HasNoProposals => Proposals.Count == 0;
    void RaiseCounts() { Raise(nameof(PendingCount)); Raise(nameof(ProposalTabText)); Raise(nameof(HasNoProposals)); }

    async Task ApproveAsync(AiProposal a)
    {
        if (!a.CanRun) return;
        try
        {
            var (result, undo) = await AiActions.ExecuteAsync(a);
            a.Message = result;
            a.State = "done";
            AddLog(LogTitle(a), a.Reason, "実行: " + result, undo);
            Status = "実行しました: " + a.Title;
        }
        catch (Exception ex)
        {
            a.Message = ex.InnerException?.Message ?? ex.Message;
            a.State = "failed";
            AddLog(LogTitle(a), a.Reason, "失敗: " + a.Message, null);
            Status = "できませんでした: " + a.Message;
        }
        RaiseCounts();
    }

    // ---------------------------------------------------------------- 変更ログ
    public ObservableCollection<AiLogEntry> Log { get; } = new();

    static string LogTitle(AiProposal a) => (a.HasScene ? "[" + a.SceneText + "] " : "") + a.Title;

    void AddLog(string title, string reason, string result, Func<Task<string?>>? undo)
    {
        Log.Insert(0, new AiLogEntry { Title = title, Reason = reason, Result = result, Ai = $"{Provider.Name} / {Model}", UndoAction = undo });
        while (Log.Count > 500) Log.RemoveAt(Log.Count - 1);
        AiLog.Save(Log);
    }

    async Task UndoAsync(AiLogEntry e)
    {
        if (!e.CanUndo) return;
        try
        {
            var err = await e.UndoAction!();
            if (err != null) { Status = "元に戻せませんでした: " + err; return; }
            e.Undone = "↩ " + DateTime.Now.ToString("HH:mm:ss") + " に元に戻しました";
            AiLog.Save(Log);
            Status = "元に戻しました: " + e.Title;
        }
        catch (Exception ex) { Status = "元に戻せませんでした: " + (ex.InnerException?.Message ?? ex.Message); }
    }

    // ---------------------------------------------------------------- AI へ送るものを集める
    static string Clock(double secs) { var t = TimeSpan.FromSeconds(Math.Max(0, secs)); return t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss\.ff") : t.ToString(@"m\:ss\.ff"); }

    /// <summary>プロジェクト全体のこと(いまの位置・選択中のアイテム・キャラクター)</summary>
    static string Overview(List<object> scenes)
    {
        var sb = new StringBuilder();
        try
        {
            var tl = Ymm.Timeline();
            if (tl != null)
            {
                var idx = scenes.FindIndex(ProjectSnapshot.IsCurrent);
                double fps = ProjectSnapshot.FpsOf(tl);
                int f = Ymm.CurrentFrame();
                sb.AppendLine($"いま開いているシーン: {(idx >= 0 ? $"シーン{idx + 1}「{ProjectSnapshot.SceneName(scenes[idx], idx)}」" : "(不明)")} / 再生位置 {Clock(f / fps)}({f} フレーム目)");
                var sel = Ymm.SelectedItems();
                if (sel.Count > 0) sb.AppendLine("選択中のアイテム: " + string.Join("、", sel.Take(20).Select(i => ProjectSnapshot.ItemBrief(i))));
                var vis = ProjectSnapshot.VisibleAt(tl, f);
                if (vis.Count > 0) sb.AppendLine("再生位置で画面に出ているアイテム: " + string.Join("、", vis.Take(20).Select(i => ProjectSnapshot.ItemBrief(i))));
            }
        }
        catch { }
        try
        {
            var st = Ymm.FindType("YukkuriMovieMaker.Settings.CharacterSettings");
            var def = st?.GetProperty("Default", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
            if (Ymm.Get(def, "Characters") is IEnumerable e)
            {
                var names = new List<string>();
                foreach (var c in e) if (Ymm.Get(c, "Name") is string n && n.Length > 0) names.Add(n);
                if (names.Count > 0) sb.AppendLine("登録されているキャラクター: " + string.Join("、", names.Take(40)));
            }
        }
        catch { }
        return sb.ToString();
    }

    /// <summary>タイムライン(すべてのシーン)の文章と、プレビュー画像を集める。終わったら、元のシーン・位置に戻す</summary>
    async Task<(string text, List<(string, string, string)> images, string attached, List<AiFilePart> files)> CollectAsync(CancellationToken ct)
    {
        var images = new List<(string, string, string)>();
        var files = new List<AiFilePart>();
        // とぎれなく撮ったコマ(ファイル・表示時間・説明・シーン)
        var cont = new List<(string path, double seconds, string caption, int scene, string sceneName, int frame)>();
        string? contDir = null;
        int contW = 0, contH = 0;
        var sb = new StringBuilder();
        var notes = new List<string>();
        List<object> scenes = new();
        object? owner = null;
        try { scenes = ProjectSnapshot.FindScenes(out owner); } catch { }
        bool detailed = Detail == 1 || UseProposals;

        if (SendTimeline)
        {
            var ov = Overview(scenes);
            if (ov.Length > 0) sb.AppendLine(ov);
            if (scenes.Count == 0) notes.Add("タイムラインを読めませんでした");
            else
            {
                int per = Math.Max(80, (detailed ? 800 : 1500) / Math.Max(1, scenes.Count));
                int limit = detailed ? 150000 : 90000;
                // いま開いているシーンを先に(長すぎて省略する時も、いまのシーンは必ず入れる)
                var order = Enumerable.Range(0, scenes.Count).OrderBy(i => ProjectSnapshot.IsCurrent(scenes[i]) ? 0 : 1).ThenBy(i => i).ToList();
                foreach (var i in order)
                {
                    try { sb.AppendLine(ProjectSnapshot.DescribeScene(scenes[i], i, per, detailed)); }
                    catch (Exception ex) { sb.AppendLine($"## シーン{i + 1}: 読めませんでした({ex.Message})"); }
                    if (sb.Length > limit) { sb.AppendLine("(長すぎるため、以降のシーンは省略)"); break; }
                }
                notes.Add($"タイムライン({scenes.Count} シーン{(detailed ? "・詳しく" : "")})");
            }
        }

        if (PreviewMode > 0)
        {
            var original = scenes.FirstOrDefault(ProjectSnapshot.IsCurrent);
            var originalTl = Ymm.Timeline();
            int originalFrame = Ymm.CurrentFrame();
            var targets = PreviewMode == 2 && scenes.Count > 0 ? scenes : (original != null ? new List<object> { original } : new List<object>());
            if (targets.Count == 0 && originalTl != null) targets.Add(originalTl);
            int frames = FramesPerScene;
            int maxImages = PreviewSize == 2 ? 30 : 60;
            try
            {
                for (int i = 0; i < targets.Count && images.Count < maxImages; i++)
                {
                    ct.ThrowIfCancellationRequested();
                    var scene = targets[i];
                    int idx = Math.Max(0, scenes.IndexOf(scene));
                    var name = scenes.Count > 0 ? ProjectSnapshot.SceneName(scene, idx) : "いまのシーン";
                    if (!ProjectSnapshot.IsCurrent(scene))
                    {
                        Status = $"プレビューを撮っています…(シーン「{name}」)";
                        if (!ProjectSnapshot.Switch(scene, owner)) { notes.Add($"「{name}」に切り替えられず、プレビューなし"); continue; }
                        await Task.Delay(500, ct);
                    }
                    var tl = Ymm.Timeline();
                    if (tl == null) continue;
                    int len = ToIntSafe(Ymm.Get(tl, "Length"));
                    if (len <= 0) foreach (var it in ProjectSnapshot.ItemsOf(tl)) len = Math.Max(len, ToIntSafe(Ymm.Get(it, "Frame")) + ToIntSafe(Ymm.Get(it, "Length")));
                    double fpsC = ProjectSnapshot.FpsOf(tl);
                    if (PreviewPoints == 2)
                    {
                        // とぎれなく: 決めた間隔で、はじめからおわりまで全部撮る
                        contDir ??= Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "YmmExplorer_ai_prev_" + Guid.NewGuid().ToString("N"))).FullName;
                        int step = ContinuousFps <= 0 ? 1 : Math.Max(1, (int)Math.Round(fpsC / ContinuousFps));
                        if (len / step > MaxContinuousFrames) step = (int)Math.Ceiling(len / (double)MaxContinuousFrames);
                        var all = Enumerable.Range(0, Math.Max(1, (len + step - 1) / step)).Select(k => k * step).Where(f => f < Math.Max(1, len)).ToList();
                        var began = DateTime.Now;
                        for (int k = 0; k < all.Count && cont.Count < MaxContinuousFrames * 2; k++)
                        {
                            ct.ThrowIfCancellationRequested();
                            int f = all[k];
                            if (k % 5 == 0)
                            {
                                var el = (DateTime.Now - began).TotalSeconds;
                                var left = k > 0 ? el / k * (all.Count - k) : 0;
                                Status = $"プレビューを撮っています…(シーン「{name}」 {k}/{all.Count} コマ" + (k > 10 ? $"・残り約 {Clock(left)})" : ")");
                            }
                            ProjectSnapshot.SetFrame(tl, f);
                            await Task.Delay(100, ct);
                            await Application.Current.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
                            var file = Path.Combine(contDir, $"{cont.Count:D6}.jpg");
                            var size = ProjectSnapshot.SaveJpeg(Ymm.CapturePreview(), file, 1280);
                            if (size == null) { notes.Add($"「{name}」のプレビューを撮れませんでした({Ymm.LastError})"); break; }
                            if (contW == 0) (contW, contH) = size.Value;
                            var vis = ProjectSnapshot.VisibleAt(tl, f);
                            var cap = $"[プレビュー] シーン{idx + 1}「{name}」の {Clock(f / fpsC)}({f} フレーム目)。" +
                                      (vis.Count > 0 ? "出ているアイテム: " + string.Join("、", vis.Take(12).Select(v => $"L{ToIntSafe(Ymm.Get(v, "Layer")):00} " + ProjectSnapshot.ItemBrief(v, 20))) : "アイテムなし");
                            cont.Add((file, Math.Min(step, len - f) / fpsC, cap, idx, name, f));
                        }
                        continue;
                    }
                    List<int> points;
                    if (frames == 1 && ReferenceEquals(tl, originalTl)) points = new List<int> { originalFrame };
                    else if (PreviewPoints == 1 && ProjectSnapshot.TextPoints(tl) is { Count: > 0 } tp)
                        points = tp.Count <= frames ? tp : Enumerable.Range(0, frames).Select(k => tp[(int)((tp.Count - 1) * (double)k / Math.Max(1, frames - 1))]).Distinct().ToList();
                    else if (len <= 1) points = new List<int> { 0 };
                    else points = Enumerable.Range(0, frames).Select(k => (int)Math.Round((len - 1) * (k + 0.5) / frames)).ToList();

                    double fps = ProjectSnapshot.FpsOf(tl);
                    int shot = 0;
                    foreach (var f in points)
                    {
                        if (images.Count >= maxImages) break;
                        shot++;
                        Status = $"プレビューを撮っています…(シーン「{name}」 {shot}/{points.Count})";
                        ProjectSnapshot.SetFrame(tl, f);
                        await Task.Delay(250, ct);
                        await Application.Current.Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.ContextIdle);
                        var enc = ProjectSnapshot.Encode(Ymm.CapturePreview(), PreviewPixels);
                        if (enc == null) { notes.Add($"「{name}」のプレビューを撮れませんでした({Ymm.LastError})"); break; }
                        var at = $"{Clock(f / fps)}({f} フレーム目)";
                        var vis = ProjectSnapshot.VisibleAt(tl, f);
                        var caption = $"[プレビュー] シーン{idx + 1}「{name}」の {at} の画面。";
                        caption += vis.Count > 0
                            ? "この時に出ているアイテム(下のレイヤーから順): " + string.Join("、", vis.Take(15).Select(v => $"L{ToIntSafe(Ymm.Get(v, "Layer")):00} " + ProjectSnapshot.ItemBrief(v, 24)))
                            : "この時に出ているアイテムはありません";
                        images.Add((enc.Value.mime, enc.Value.base64, caption));
                    }
                }
            }
            finally
            {
                // 元のシーン・位置へ戻す
                try
                {
                    if (original != null) ProjectSnapshot.Switch(original, owner);
                    var tl = Ymm.Timeline();
                    if (tl != null) ProjectSnapshot.SetFrame(tl, originalFrame);
                }
                catch { }
            }
            if (cont.Count > 0)
            {
                try { await SendContinuousAsync(cont, contW, contH, sb, images, files, notes, ct); }
                finally { try { if (contDir != null) Directory.Delete(contDir, true); } catch { } }
            }
            else notes.Add($"プレビュー {images.Count} 枚");
        }
        if (ShareStorage && AiStorage.Root != null)
        {
            try
            {
                var idx = await Task.Run(() => AiStorage.Index());
                if (idx.Length > 0) { sb.AppendLine().AppendLine(idx); notes.Add("専用ストレージの中身"); }
            }
            catch { }
        }
        if (commandTurn) notes.Add("操作の指示として送信");
        else if (UseProposals) notes.Add("操作の提案: オン");
        // 配置できる素材: 提案がオンの時は素材データベースなども。専用ストレージの「素材」フォルダーは、いつも見せる
        if (UseProposals || StorageInfo.MaterialsDir != null || StorageInfo.RecordingsDir != null)
        {
            try
            {
                var share = UseProposals && ShareLibrary;
                var lib = AiLibrary.Build(share ? AllowedFolders.ToList() : new List<string>(), share ? 40000 : 15000, share);
                if (lib.Split('\n').Length > 2)
                {
                    sb.AppendLine().AppendLine(lib);
                    notes.Add(share ? "配置できる素材の一覧" : "専用ストレージの素材");
                }
            }
            catch (Exception ex) { notes.Add("素材の一覧を作れませんでした: " + ex.Message); }
        }
        return (sb.ToString(), images, string.Join("・", notes), files);
    }

    const int MaxContinuousFrames = 6000;

    /// <summary>とぎれなく撮ったコマを送る。Gemini には1本の動画にして、ほかの AI には送れる枚数まで画像で</summary>
    async Task SendContinuousAsync(List<(string path, double seconds, string caption, int scene, string sceneName, int frame)> cont, int w, int h,
                                   StringBuilder sb, List<(string, string, string)> images, List<AiFilePart> files, List<string> notes, CancellationToken ct)
    {
        // 動画の何秒目が、どのシーンのどこか
        var map = new StringBuilder("## プレビュー(とぎれなく撮ったもの)の時間とシーンの対応\n");
        double t = 0;
        foreach (var g in cont.GroupBy(c => (c.scene, c.sceneName)))
        {
            var list = g.ToList();
            double len = list.Sum(x => x.seconds);
            map.AppendLine($"動画の {Clock(t)}〜{Clock(t + len)} = シーン{g.Key.scene + 1}「{g.Key.sceneName}」の {list[0].frame}〜{list[^1].frame} フレーム目");
            t += len;
        }

        if (ProviderId == "gemini" && ApiKey.Length > 0)
        {
            var mp4 = Path.Combine(Path.GetTempPath(), "YmmExplorer_ai_preview_" + Guid.NewGuid().ToString("N") + ".mp4");
            try
            {
                if (await AiMedia.FramesToMp4Async(cont.Select(c => (c.path, c.seconds)).ToList(), mp4, w, h, s => Status = s, ct))
                {
                    var uri = await AiMedia.GeminiUploadAsync(ApiKey, mp4, "video/mp4", "YMM4 プレビュー", s => Status = s, ct);
                    files.Add(new AiFilePart { Mime = "video/mp4", Uri = uri, FileName = "preview.mp4", Caption = "[プレビュー動画] YMM4 のプレビューを、とぎれなく撮って動画にしたもの(音声なし)" });
                    sb.AppendLine().Append(map).AppendLine("(プレビュー動画は音声なし。音はタイムラインの情報から判断してください)");
                    notes.Add($"プレビュー動画 {Clock(t)}({cont.Count} コマ)");
                    return;
                }
                notes.Add("プレビューを動画にできなかったため、画像で送ります");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { notes.Add("プレビュー動画を送れなかったため、画像で送ります(" + ex.Message + ")"); }
            finally { try { File.Delete(mp4); } catch { } }
        }

        // 画像で送る: AI ごとの上限まで、均等に選ぶ
        int limit = ProviderId switch { "claude" => 95, "openai" => 80, "gemini" => 300, _ => 30 } - images.Count;
        limit = Math.Max(1, limit);
        var pick = cont.Count <= limit ? Enumerable.Range(0, cont.Count).ToList()
                                       : Enumerable.Range(0, limit).Select(k => (int)Math.Round((cont.Count - 1) * (double)k / Math.Max(1, limit - 1))).Distinct().ToList();
        foreach (var i in pick)
        {
            try { images.Add(("image/jpeg", Convert.ToBase64String(await File.ReadAllBytesAsync(cont[i].path, ct)), cont[i].caption)); }
            catch { }
        }
        notes.Add(cont.Count <= limit ? $"プレビュー {pick.Count} 枚(とぎれなく)" : $"プレビュー {pick.Count} 枚(撮った {cont.Count} コマから、{Provider.Name} に送れる枚数だけ選びました)");
    }

    static int ToIntSafe(object? v) { try { return v == null ? 0 : Convert.ToInt32(v, CultureInfo.InvariantCulture); } catch { return 0; } }
}
