using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YmmExplorer;

// =====================================================================================================
//  テキストタブ: 最初から入っている書式のテンプレート(600件)を、演出図形タブと同じ形で並べる。
//  新しいテキストとして配置するか、選択中のテキストに適用する。お気に入りに入れたものは「お気に入り」に集まる
// =====================================================================================================

/// <summary>見た目のエフェクト1つ。Kind: o=縁取り(A=太さ B=ぼかし Flag=角ばる) s=影(A=横 B=縦 C=ぼかし)。数はテンプレートの文字の大きさでの値</summary>
public sealed record TextFx(char Kind, double A, double B, double C, uint Color, bool Flag);

/// <summary>テキストのテンプレート1件(色は AARRGGBB)</summary>
public sealed record TextTpl(string Id, string Name, string Category, string Desc, string Sample, string Font, double Size,
    uint Color, string Style, uint StyleColor, bool Bold, bool Italic, double Spacing, uint Bg, TextFx[] Fx);

/// <summary>テキストタブに並ぶ1件(標準のテンプレート、または前に保存した書式)</summary>
public sealed class TextTplItem : NotifyBase
{
    public TextTpl? Tpl { get; }
    /// <summary>前のテキストタブで保存した書式(読むだけ)</summary>
    public EffectEntry? Legacy { get; }
    public string Id { get; }
    public string Name { get; }
    public string Category { get; }
    public string Description { get; }
    public string Sample => Tpl?.Sample ?? "テキスト";
    public string FontText => Tpl != null ? "フォント: " + Tpl.Font + (TextTplRender.HasFont(Tpl.Font) ? "" : "(このパソコンにありません)") : "前のテキストタブで保存した書式";
    public bool FontMissing => Tpl != null && !TextTplRender.HasFont(Tpl.Font);
    public string NoText => Tpl != null ? "No. " + Id.TrimStart('t').TrimStart('0') : "保存した書式";

    public TextTplItem(TextTpl t)
    {
        Tpl = t; Id = t.Id; Name = t.Name; Category = t.Category; Description = t.Desc;
    }

    public TextTplItem(EffectEntry e, string category)
    {
        Legacy = e; Id = "u:" + e.Id; Name = e.Name; Category = category;
        Description = e.Description ?? (e.StyleKeys is { Count: > 0 } k ? "保存した項目: " + string.Join("、", k) : "前のテキストタブで保存した書式");
    }

    WeakReference<BitmapSource>? cache;
    bool queued;

    /// <summary>画面に出た時に描く(600件あるので、少しずつ描く)</summary>
    public ImageSource? Thumb
    {
        get
        {
            if (cache != null && cache.TryGetTarget(out var bmp)) return bmp;
            if (!queued)
            {
                queued = true;
                Application.Current?.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    queued = false;
                    var img = Render(Sample, TextTplRender.ThumbW, TextTplRender.ThumbH);
                    if (img == null) return;
                    cache = new WeakReference<BitmapSource>(img);
                    Raise(nameof(Thumb));
                }));
            }
            return null;
        }
    }

    /// <summary>この書式で text を描いた絵</summary>
    public BitmapSource? Render(string text, int w, int h)
    {
        if (Tpl != null) return TextTplRender.Render(Tpl, text, w, h);
        var json = Legacy?.StyleJson;
        return json == null ? null : TextThumb.FromStyleJson(json, text, "#2A2D33");
    }

    bool isSelected;
    public bool IsSelected { get => isSelected; set => Set(ref isSelected, value); }

    bool isFavorite;
    public bool IsFavorite { get => isFavorite; set { if (Set(ref isFavorite, value)) Raise(nameof(FavoriteMark)); } }
    public string FavoriteMark => isFavorite ? "★" : "☆";
}

/// <summary>テンプレートの見本の絵を描く(YMM4 の縁取り・影・縁取りエフェクト・影エフェクトを、近い見た目でまねる)</summary>
public static class TextTplRender
{
    public const int ThumbW = 320, ThumbH = 180;
    static Dictionary<string, FontFamily>? families;
    static readonly (string Suffix, FontWeight Weight)[] Weights =
    {
        ("Extra Bold", FontWeights.ExtraBold), ("Ultra Bold", FontWeights.UltraBold), ("Semi Bold", FontWeights.SemiBold), ("Extra Light", FontWeights.ExtraLight),
        ("ExtraBold", FontWeights.ExtraBold), ("Heavy", FontWeights.Heavy), ("Black", FontWeights.Black), ("SemiBold", FontWeights.SemiBold),
        ("DemiBold", FontWeights.DemiBold), ("Bold", FontWeights.Bold), ("Medium", FontWeights.Medium), ("DemiLight", FontWeights.Light),
        ("ExtraLight", FontWeights.ExtraLight), ("Light", FontWeights.Light), ("Thin", FontWeights.Thin), ("Regular", FontWeights.Normal),
    };

    static Dictionary<string, FontFamily> Families()
    {
        if (families != null) return families;
        var d = new Dictionary<string, FontFamily>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var ff in Fonts.SystemFontFamilies)
                foreach (var n in ff.FamilyNames.Values)
                    d.TryAdd(n, ff);
        }
        catch { }
        return families = d;
    }

    /// <summary>YMM4 のフォント名(「Noto Sans JP Black」のように太さが付いた名前も)から、WPF のフォントと太さ</summary>
    static (FontFamily? Family, FontWeight Weight) Resolve(string name)
    {
        var d = Families();
        if (d.TryGetValue(name, out var f)) return (f, FontWeights.Normal);
        foreach (var (suffix, w) in Weights)
            if (name.EndsWith(" " + suffix, StringComparison.OrdinalIgnoreCase) && d.TryGetValue(name[..^(suffix.Length + 1)], out var bf)) return (bf, w);
        // 「YDW バナナスリップplus plus」のように、最後に書体の名前が付いたもの(WPF では「YDW バナナスリップplus」)
        for (int i = name.LastIndexOf(' '); i > 0; i = name.LastIndexOf(' ', i - 1))
            if (d.TryGetValue(name[..i], out var pf)) return (pf, FontWeights.Normal);
        return (null, FontWeights.Normal);
    }

    public static bool HasFont(string name) => Resolve(name).Family != null;

    static Color C(uint argb) => Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
    static Color A(Color c, double k) => Color.FromArgb((byte)Math.Clamp(c.A * k, 0, 255), c.R, c.G, c.B);

    /// <summary>文字の形(大きさ size、文字間隔 spacing。複数行は中央揃え)</summary>
    static Geometry Shape(string text, Typeface tf, double size, double spacing)
    {
        var group = new GeometryGroup { FillRule = FillRule.Nonzero };
        var lines = text.Replace("\\n", "\n").Split('\n');
        double y = 0, lineH = size * 1.25;
        var culture = CultureInfo.GetCultureInfo("ja-JP");
        var widths = new List<double>();
        var parts = new List<List<(FormattedText Ft, double X)>>();
        foreach (var line in lines)
        {
            var list = new List<(FormattedText, double)>();
            double x = 0;
            if (Math.Abs(spacing) < .01)
            {
                var ft = new FormattedText(line, culture, FlowDirection.LeftToRight, tf, size, Brushes.White, 1.0);
                list.Add((ft, 0)); x = ft.WidthIncludingTrailingWhitespace;
            }
            else
            {
                var e = StringInfo.GetTextElementEnumerator(line);
                while (e.MoveNext())
                {
                    var ft = new FormattedText(e.GetTextElement(), culture, FlowDirection.LeftToRight, tf, size, Brushes.White, 1.0);
                    list.Add((ft, x)); x += ft.WidthIncludingTrailingWhitespace + spacing;
                }
                x = Math.Max(0, x - spacing);
            }
            widths.Add(x); parts.Add(list);
        }
        double maxW = widths.Count > 0 ? widths.Max() : 0;
        for (int i = 0; i < parts.Count; i++)
        {
            double off = (maxW - widths[i]) / 2;
            foreach (var (ft, x) in parts[i]) group.Children.Add(ft.BuildGeometry(new Point(off + x, y)));
            y += lineH;
        }
        return group;
    }

    /// <summary>
    /// テンプレート t で text を描く(w×h、背景はテンプレートの色)。
    /// fixedScale を渡すと、テンプレートの 1px をその倍率で描く(YMM4 のプレビューと比べる用)。bg を渡すと背景の色を変える
    /// </summary>
    public static BitmapSource? Render(TextTpl t, string text, int w, int h, double fixedScale = 0, Color? bg = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text)) text = t.Sample;
            var (fam, weight) = Resolve(t.Font);
            fam ??= new FontFamily("Yu Gothic UI");
            if (t.Bold && weight.ToOpenTypeWeight() < FontWeights.Bold.ToOpenTypeWeight()) weight = FontWeights.Bold;
            var tf = new Typeface(fam, t.Italic ? FontStyles.Italic : FontStyles.Normal, weight, FontStretches.Normal);

            const double S0 = 100;                       // 形を作る大きさ
            double u = S0 / Math.Max(1, t.Size);          // テンプレートの1px → 形の単位
            var geo = Shape(text, tf, S0, t.Spacing * u);
            var b = geo.Bounds;
            if (b.IsEmpty || b.Width < 1) return null;

            // 装飾・エフェクトの広がり(形の単位)。YMM4 の縁取りは、文字の大きさのおよそ 4.5%(細いものは 2.5%)
            bool border = t.Style.Contains("Border"), sharp = t.Style.StartsWith("Sharp"), light = t.Style.EndsWith("Light");
            double r0 = border ? S0 * (light ? .025 : .045) : 0;
            double margin = r0, shadowOff = 0;
            if (t.Style.StartsWith("Shadow")) shadowOff = S0 * .05;
            foreach (var fx in t.Fx)
            {
                if (fx.Kind == 'o') margin += (fx.A + fx.B) * u;
                else shadowOff = Math.Max(shadowOff, (Math.Max(Math.Abs(fx.A), Math.Abs(fx.B)) + fx.C) * u);
            }
            margin += shadowOff;
            double scale = fixedScale > 0 ? fixedScale * t.Size / S0 : Math.Min(w * .9 / (b.Width + margin * 2), h * .8 / (b.Height + margin * 2));
            var to = new TransformGroup();
            to.Children.Add(new TranslateTransform(-(b.Left + b.Right) / 2, -(b.Top + b.Bottom) / 2));
            to.Children.Add(new ScaleTransform(scale, scale));
            to.Children.Add(new TranslateTransform(w / 2.0, h / 2.0));

            // 奥から手前へ描く層: (色, 広げる半径, ずらし, ぼかし, 角ばる)
            var layers = new List<(Color Col, double R, Vector Off, double Blur, bool Sharp)> { (C(t.Color), 0, new Vector(), 0, false) };
            if (border) layers.Insert(0, (C(t.StyleColor), r0, new Vector(), 0, sharp));
            if (t.Style.StartsWith("Shadow"))
                layers.Insert(0, (A(C(t.StyleColor), light ? .5 : .9), 0, new Vector(S0 * .045, S0 * .045), light ? S0 * .015 : 0, false));
            double rCur = r0;
            foreach (var fx in t.Fx)
            {
                if (fx.Kind == 'o')
                {
                    rCur += fx.A * u;
                    layers.Insert(0, (C(fx.Color), rCur, new Vector(), fx.B * u, fx.Flag));
                }
                else layers.Insert(0, (C(fx.Color), rCur, new Vector(fx.A * u, fx.B * u), fx.C * u, false));
            }

            // 層ごとに描いて、ぼかす層には本物のぼかし(BlurEffect)をかける
            var root = new ContainerVisual();
            var back = new DrawingVisual();
            using (var dc = back.RenderOpen()) dc.DrawRectangle(new SolidColorBrush(bg ?? C(t.Bg)), null, new Rect(0, 0, w, h));
            root.Children.Add(back);
            foreach (var (col, r, off, blur, sh) in layers)
            {
                var v = new DrawingVisual();
                using (var dc = v.RenderOpen())
                {
                    dc.PushTransform(to);
                    if (off.X != 0 || off.Y != 0) dc.PushTransform(new TranslateTransform(off.X, off.Y));
                    var br = new SolidColorBrush(col);
                    dc.DrawGeometry(br, r > 0 ? new Pen(br, r * 2) { LineJoin = sh ? PenLineJoin.Miter : PenLineJoin.Round, MiterLimit = 3 } : null, geo);
                    if (off.X != 0 || off.Y != 0) dc.Pop();
                    dc.Pop();
                }
                if (blur * scale > .5)
                    v.Effect = new System.Windows.Media.Effects.BlurEffect
                    {
                        Radius = blur * scale,
                        KernelType = System.Windows.Media.Effects.KernelType.Gaussian,
                        RenderingBias = System.Windows.Media.Effects.RenderingBias.Quality,
                    };
                root.Children.Add(v);
            }
            var bmp = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(root);
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }
}

/// <summary>テンプレートを YMM4 のテキストアイテムに当てはめる</summary>
internal static class TextTplApply
{
    /// <summary>テンプレートが付け替える見た目のエフェクト(適用する時に、前のものは外す)</summary>
    static readonly string[] LookEffects = { "OutlineEffect", "ShadowEffect" };

    static string J(string s) => JsonSerializer.Serialize(s);
    static string Col(uint c) => J($"#{c:X8}");

    /// <summary>ボイスアイテムは字幕(Jimaku…)の項目</summary>
    static System.Reflection.PropertyInfo? Prop(object item, string name)
        => (Ymm.IsVoiceItem(item) ? Ymm.FindProperty(item.GetType(), "Jimaku" + name) : null) ?? Ymm.FindProperty(item.GetType(), name);

    /// <summary>いまの値の形(アニメーションなど)をそのまま使って、値だけ value にしたJSON</summary>
    static string? NumberJson(object item, string name, double value)
    {
        var p = Prop(item, name);
        if (p == null) return null;
        var type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
        if (type == typeof(double) || type == typeof(float)) return value.ToString(CultureInfo.InvariantCulture);
        var json = Ymm.ValueToJson(p.GetValue(item));
        if (json == null || JsonNode.Parse(json) is not JsonObject o || o["Values"] is not JsonArray arr || arr.Count == 0) return null;
        var first = arr[0]!.DeepClone();
        if (first is JsonObject fo) fo["Value"] = value;
        arr.Clear(); arr.Add(first);
        return o.ToJsonString();
    }

    /// <summary>文字の大きさ(アニメーションなら最初の値)</summary>
    static double ReadSize(object item)
    {
        var v = Prop(item, "FontSize")?.GetValue(item);
        if (v == null) return 0;
        if (v is double d) return d;
        if (Ymm.Get(v, "Values") is System.Collections.IEnumerable e)
            foreach (var x in e)
            {
                try { return Convert.ToDouble(Ymm.Get(x, "Value"), CultureInfo.InvariantCulture); } catch { }
                break;
            }
        return 0;
    }

    static void SetAnim(JsonObject o, string key, double v)
    {
        if (o[key] is JsonObject a && a["Values"] is JsonArray arr)
            foreach (var e in arr) if (e is JsonObject eo) eo["Value"] = v;
    }

    static void SetColor(JsonObject o, string brushKey, string colorKey, uint c)
    {
        var s = $"#{c:X8}";
        if (o[brushKey] is JsonObject b && b["Parameter"] is JsonObject p) p["Color"] = s;
        if (o.ContainsKey(colorKey)) o[colorKey] = s;
    }

    /// <summary>テンプレートの見た目のエフェクトの一覧(YMM4 のエフェクトを作って、値を書き換える)。scale は太さの倍率</summary>
    static object? BuildEffects(Type listType, TextTpl t, double scale, out string error)
    {
        error = "";
        var insts = new List<object>();
        foreach (var fx in t.Fx)
        {
            var type = Ymm.FindType("YukkuriMovieMaker.Project.Effects." + (fx.Kind == 'o' ? "OutlineEffect" : "ShadowEffect"));
            if (type == null) { error = "YMM4 の縁取り・影のエフェクトが見つかりませんでした"; return null; }
            try { insts.Add(Activator.CreateInstance(type)!); }
            catch (Exception ex) { error = ex.GetBaseException().Message; return null; }
        }
        var proto = Ymm.MergeEffects(listType, null, insts, false);
        if (proto == null) { error = Ymm.LastError; return null; }
        if (insts.Count == 0) return proto;
        var json = Ymm.ValueToJson(proto);
        if (json == null || JsonNode.Parse(json) is not JsonArray arr || arr.Count != t.Fx.Length) { error = "エフェクトを作れませんでした。" + Ymm.LastError; return null; }
        for (int i = 0; i < arr.Count; i++)
        {
            if (arr[i] is not JsonObject o) continue;
            var fx = t.Fx[i];
            if (fx.Kind == 'o')
            {
                SetAnim(o, "StrokeThickness", fx.A * scale);
                SetAnim(o, "Blur", fx.B * scale);
                SetColor(o, "StrokeBrush", "StrokeColor", fx.Color);
                if (o.ContainsKey("IsAngular")) o["IsAngular"] = fx.Flag;
            }
            else
            {
                SetAnim(o, "X", fx.A * scale);
                SetAnim(o, "Y", fx.B * scale);
                SetAnim(o, "Blur", fx.C * scale);
                SetAnim(o, "Opacity", 100);
                SetColor(o, "Brush", "Color", fx.Color);
            }
        }
        var list = Ymm.JsonToValue(listType, arr.ToJsonString());
        if (list == null) error = "エフェクトを作れませんでした。" + Ymm.LastError;
        return list;
    }

    /// <summary>
    /// テンプレートをアイテムに当てはめる。withSize=false の時は文字の大きさは変えず、縁取りの太さなどを今の大きさに合わせる。
    /// 戻り値: (当てはめた項目数, うまくいかなかった項目)
    /// </summary>
    public static async Task<(int Applied, List<string> Failed)> ApplyAsync(object item, TextTpl t, bool withSize, bool useEdit)
    {
        var failed = new List<string>();
        double now = ReadSize(item);
        double scale = withSize || now <= 0 ? 1 : now / t.Size;
        var style = new Dictionary<string, string>
        {
            ["Font"] = J(t.Font), ["FontColor"] = Col(t.Color), ["Style"] = J(t.Style), ["StyleColor"] = Col(t.StyleColor),
            ["Bold"] = t.Bold ? "true" : "false", ["Italic"] = t.Italic ? "true" : "false",
        };
        if (withSize && NumberJson(item, "FontSize", t.Size) is string fs) style["FontSize"] = fs;
        var spName = Prop(item, "LetterSpacing2") != null ? "LetterSpacing2" : "LetterSpacing";
        if (NumberJson(item, spName, t.Spacing * scale) is string sp) style[spName] = sp;

        var vp = Prop(item, "VideoEffects");
        object? looks = null;
        if (vp != null) { looks = BuildEffects(vp.PropertyType, t, scale, out var err); if (looks == null) failed.Add("縁取り・影のエフェクト(" + err + ")"); }

        int applied = 0;
        void Body()
        {
            var r = Ymm.ApplyTextStyleAsync(item, style, useEdit: false).GetAwaiter().GetResult();
            applied += r.Applied;
            failed.AddRange(r.Failed);
            if (vp == null || looks == null) return;
            // 前に付けた縁取り・影は外して、ほかのエフェクト(登場など)は残す
            var keep = new List<object>();
            if (vp.GetValue(item) is System.Collections.IEnumerable cur)
                foreach (var x in cur) if (x != null && !LookEffects.Contains(x.GetType().Name)) keep.Add(x);
            var merged = Ymm.MergeEffects(vp.PropertyType, looks, keep, true);
            if (merged == null) { failed.Add("映像エフェクト(" + Ymm.LastError + ")"); return; }
            if (Ymm.SetValue(item, vp.Name, merged, out var e2)) applied++;
            else failed.Add("映像エフェクト(" + e2 + ")");
        }
        if (useEdit) await Ymm.EditAsync(item, Body); else Body();
        return (applied, failed);
    }

    /// <summary>前のテキストタブで保存した書式(項目名 → JSON)</summary>
    public static Dictionary<string, string> LegacyStyle(string? json)
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
}

public sealed record TextCategory(string Name, int Count)
{
    public string Label => $"{Name}({Count})";
}

public sealed record TextRow(IReadOnlyList<TextTplItem> Tiles);

/// <summary>テキストタブの画面(ウィンドウごと)</summary>
public sealed class TextBrowserVm : NotifyBase, IDisposable
{
    public const string AllName = "すべて", FavName = "★ お気に入り", LegacyName = "前に保存した書式";
    readonly ExplorerSettings settings;
    readonly List<TextTplItem> items = new();
    int columns = 4;
    bool updating;

    public TextBrowserVm(ExplorerSettings settings)
    {
        this.settings = settings;
        PlaceCommand = new RelayCommand(_ => _ = PlaceAsync(), _ => selected != null);
        ApplyCommand = new RelayCommand(_ => _ = ApplyAsync(), _ => selected != null);
        RunCommand = new RelayCommand(_ => _ = RunAsync(), _ => selected != null);
        FavoriteCommand = new RelayCommand(_ => { if (selected != null) ToggleFavorite(selected); }, _ => selected != null);
        Load();
        Refresh();
    }

    public void Dispose() { }

    /// <summary>ID からテンプレートを探す</summary>
    internal TextTplItem? FindItem(string id) => items.FirstOrDefault(i => i.Id == id);

    public RelayCommand PlaceCommand { get; }
    public RelayCommand ApplyCommand { get; }
    public RelayCommand FavoriteCommand { get; }
    /// <summary>選んだ使い方(新しく作成 / 選択中に適用)で実行する。ダブルクリックも同じ</summary>
    public RelayCommand RunCommand { get; }

    // ---------------------------------------------------------------- 使い方: 新しく作成 / 選択中に適用
    /// <summary>新しいテキストを作って、シークバーの位置に配置する</summary>
    public bool ModeNew
    {
        get => settings.TextTplMode == 0;
        set { if (value) SetMode(0); }
    }
    /// <summary>YMM4 で選択中のテキストに適用する</summary>
    public bool ModeApply
    {
        get => settings.TextTplMode == 1;
        set { if (value) SetMode(1); }
    }

    void SetMode(int mode)
    {
        if (settings.TextTplMode == mode) return;
        settings.TextTplMode = mode;
        settings.Save();
        Raise(nameof(ModeNew));
        Raise(nameof(ModeApply));
        Raise(nameof(RunText));
        Raise(nameof(HintText));
    }

    public string RunText => ModeApply ? "選択中のテキストに適用" : "新しいテキストとして配置";
    public string HintText => ModeApply
        ? "テンプレートを選ぶと、ここに表示します。ダブルクリックで、YMM4 で選択中のテキストに適用します。"
        : "テンプレートを選ぶと、ここに表示します。ダブルクリックで、シークバーの位置に新しいテキストとして配置します。";

    Task RunAsync() => ModeApply ? ApplyAsync() : PlaceAsync();

    void Load()
    {
        items.Clear();
        // 番号順だと、あとから足した分(t201〜)が分類の最後にまとまらないので、分類が初めて出てきた順にそろえる(分類の中は番号順)
        var order = new List<string>();
        foreach (var t in TextTplData.All) if (!order.Contains(t.Category)) order.Add(t.Category);
        foreach (var t in TextTplData.All.OrderBy(t => order.IndexOf(t.Category))) items.Add(new TextTplItem(t));
        // 前のテキストタブで保存した書式(YMM4 から直接読んだもの)も、読むだけで並べる
        try
        {
            foreach (var e in EffectStore.For(TabKind.Text).Everything())
                if (e.StyleJson != null && e.StyleDirect) items.Add(new TextTplItem(e, LegacyName));
        }
        catch { }
        var fav = new HashSet<string>(settings.TextFavorites ?? new List<string>());
        foreach (var i in items) i.IsFavorite = fav.Contains(i.Id);
    }

    IReadOnlyList<TextCategory> categories = Array.Empty<TextCategory>();
    public IReadOnlyList<TextCategory> Categories { get => categories; private set => Set(ref categories, value); }

    TextCategory? selectedCategory;
    public TextCategory? SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (value == null && updating) return;
            if (!Set(ref selectedCategory, value)) return;
            if (!updating) Build();
        }
    }

    string search = "";
    public string Search
    {
        get => search;
        set
        {
            if (!Set(ref search, value ?? "")) return;
            Raise(nameof(HasNoSearch));
            Build();
        }
    }
    public bool HasNoSearch => search.Length == 0;

    IReadOnlyList<TextRow> rows = Array.Empty<TextRow>();
    public IReadOnlyList<TextRow> Rows { get => rows; private set => Set(ref rows, value); }

    string countText = "";
    public string CountText { get => countText; private set => Set(ref countText, value); }

    string emptyText = "";
    /// <summary>何も無い時の案内(お気に入りが空など)</summary>
    public string EmptyText { get => emptyText; private set { if (Set(ref emptyText, value)) Raise(nameof(HasEmptyText)); } }
    public bool HasEmptyText => emptyText.Length > 0;

    TextTplItem? selected;
    public TextTplItem? Selected
    {
        get => selected;
        set
        {
            if (ReferenceEquals(selected, value)) return;
            if (selected != null) selected.IsSelected = false;
            selected = value;
            if (selected != null) selected.IsSelected = true;
            Raise(nameof(Selected));
            Raise(nameof(HasSelected));
            Raise(nameof(HasNoSelected));
            Raise(nameof(FavoriteButtonText));
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            RenderPreview();
        }
    }
    public bool HasSelected => selected != null;
    public bool HasNoSelected => selected == null;

    ImageSource? previewImage;
    public ImageSource? PreviewImage { get => previewImage; private set => Set(ref previewImage, value); }

    string previewText = "";
    /// <summary>見本の文字(空ならテンプレートの見本)。配置する時の文字にもなる</summary>
    public string PreviewText
    {
        get => previewText;
        set
        {
            if (!Set(ref previewText, value ?? "")) return;
            Raise(nameof(HasNoPreviewText));
            RenderPreview();
        }
    }
    public bool HasNoPreviewText => previewText.Length == 0;

    void RenderPreview()
    {
        var it = selected;
        PreviewImage = it?.Render(previewText.Length > 0 ? previewText : it.Sample, 640, 360);
    }

    string message = "";
    public string Message { get => message; private set => Set(ref message, value); }

    /// <summary>オン=選択中のテキストに適用する時、文字の大きさもテンプレートに合わせる</summary>
    public bool WithSize
    {
        get => settings.TextTplWithSize;
        set { settings.TextTplWithSize = value; settings.Save(); Raise(nameof(WithSize)); }
    }

    public string FavoriteButtonText => selected?.IsFavorite == true ? "★ お気に入りから外す" : "☆ お気に入りに入れる";

    public void SetColumns(int value)
    {
        value = Math.Max(1, value);
        if (value == columns) return;
        columns = value;
        Build();
    }

    public void Refresh()
    {
        var want = selectedCategory?.Name ?? AllName;
        var list = new List<TextCategory> { new(FavName, items.Count(i => i.IsFavorite)), new(AllName, items.Count(i => i.Tpl != null)) };
        var order = new List<string>();
        foreach (var i in items) if (i.Tpl != null && !order.Contains(i.Category)) order.Add(i.Category);
        list.AddRange(order.Select(c => new TextCategory(c, items.Count(i => i.Category == c))));
        int legacy = items.Count(i => i.Legacy != null);
        if (legacy > 0) list.Add(new TextCategory(LegacyName, legacy));
        updating = true;
        Categories = list;
        SelectedCategory = list.FirstOrDefault(c => c.Name == want) ?? list[1];
        updating = false;
        Build();
    }

    void Build()
    {
        var cat = selectedCategory?.Name ?? AllName;
        var q = search.Trim();
        var hit = items.Where(i => (cat == FavName ? i.IsFavorite : cat == AllName ? i.Tpl != null : i.Category == cat)
            && (q.Length == 0 || SearchTags.Matches(q, i.Id, i.Name, i.Category, i.Description, i.Tpl?.Font, i.Tpl?.Sample))).ToList();
        Rows = hit.Chunk(columns).Select(c => new TextRow(c)).ToList();
        CountText = $"{hit.Count} 件";
        EmptyText = hit.Count > 0 ? "" : cat == FavName && q.Length == 0
            ? "まだお気に入りがありません。テンプレートの ☆ を押すと、ここに集まります。"
            : "見つかりませんでした。";
    }

    public void ToggleFavorite(TextTplItem it)
    {
        it.IsFavorite = !it.IsFavorite;
        var list = settings.TextFavorites ?? new List<string>();
        list.Remove(it.Id);
        if (it.IsFavorite) list.Add(it.Id);
        settings.TextFavorites = list;
        settings.Save();
        Raise(nameof(FavoriteButtonText));
        Message = it.IsFavorite ? $"「{it.Name}」をお気に入りに入れました" : $"「{it.Name}」をお気に入りから外しました";
        // 数を更新(お気に入りを見ている時は、外したものが消える)
        var cat = selectedCategory?.Name;
        var counts = Categories.Select(c => c.Name == FavName ? new TextCategory(FavName, items.Count(i => i.IsFavorite)) : c).ToList();
        updating = true;
        Categories = counts;
        SelectedCategory = counts.FirstOrDefault(c => c.Name == cat) ?? counts[1];
        updating = false;
        if (cat == FavName) Build();
    }

    /// <summary>タイルをクリックした(ダブルクリックで、選んだ使い方で実行)</summary>
    public void Activate(TextTplItem it, bool isDouble)
    {
        Selected = it;
        if (isDouble) _ = RunAsync();
    }

    string TextToPlace(TextTplItem it) => previewText.Length > 0 ? previewText : it.Sample;

    /// <summary>新しいテキストアイテムを作って、シークバーの位置に配置する</summary>
    public async Task PlaceAsync()
    {
        var it = selected;
        if (it == null) return;
        try
        {
            var tl = Ymm.Timeline();
            if (tl == null) { Message = "⚠ " + Ymm.LastError; return; }
            var frame = Ymm.CurrentFrame();
            var sel = Ymm.SelectedItems().FirstOrDefault();
            var layer = sel != null ? Convert.ToInt32(Ymm.Get(sel, "Layer") ?? 0) : 0;
            var item = Ymm.CreateDefaultTextItem(TextToPlace(it).Replace("\\n", "\n"), frame, layer, 150);
            if (item == null) { Message = "⚠ テキストアイテムを作れませんでした。" + Ymm.LastError; return; }
            List<string> failed;
            if (it.Tpl != null) failed = (await TextTplApply.ApplyAsync(item, it.Tpl, withSize: true, useEdit: false)).Failed;
            else failed = (await Ymm.ApplyTextStyleAsync(item, TextTplApply.LegacyStyle(it.Legacy?.StyleJson), useEdit: false)).Failed;
            var template = Ymm.CreateTemplate("テキスト", new[] { item });
            if (template == null) { Message = "⚠ 配置の準備ができませんでした。" + Ymm.LastError; return; }
            var (ok, msg) = await Ymm.PlaceTemplateAsync(template);
            Message = ok ? $"「{it.Name}」で、新しいテキストを配置しました。{msg}" + (failed.Count > 0 ? $"(当てはめられなかった項目: {string.Join("、", failed.Distinct().Take(4))})" : "")
                         : "⚠ " + msg;
        }
        catch (Exception ex) { Message = "⚠ 配置できませんでした: " + ex.GetBaseException().Message; }
    }

    /// <summary>YMM4 のタイムラインで選択中のテキスト(ボイスの字幕も)に適用する</summary>
    public async Task ApplyAsync()
    {
        var it = selected;
        if (it == null) return;
        try
        {
            if (Ymm.Timeline() == null) { Message = "⚠ " + Ymm.LastError; return; }
            var targets = Ymm.SelectedItems();
            if (targets.Count == 0) { Message = "⚠ YMM4のタイムラインで、適用したいテキストのアイテムを選んでください"; return; }
            var bad = targets.FirstOrDefault(t => !Ymm.IsTextItem(t) && !Ymm.IsVoiceItem(t));
            if (bad != null) { Message = $"⚠ 選択中のアイテムがテキストではありません(種類: {Ymm.TypeName(bad)})。テキストのアイテムを選んでください"; return; }
            var failed = new List<string>();
            foreach (var t in targets)
            {
                if (it.Tpl != null) failed.AddRange((await TextTplApply.ApplyAsync(t, it.Tpl, WithSize, useEdit: true)).Failed);
                else failed.AddRange((await Ymm.ApplyTextStyleAsync(t, TextTplApply.LegacyStyle(it.Legacy?.StyleJson))).Failed);
            }
            Message = $"{targets.Count} 個のテキストに「{it.Name}」を適用しました"
                + (failed.Count > 0 ? $"(当てはめられなかった項目: {string.Join("、", failed.Distinct().Take(4))})" : "");
        }
        catch (Exception ex) { Message = "⚠ 適用できませんでした: " + ex.GetBaseException().Message; }
    }
}
