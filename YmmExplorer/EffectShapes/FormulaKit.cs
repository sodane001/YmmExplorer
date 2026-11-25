using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;

namespace YMM4EffectShapes;

// =====================================================================
//  関数の式(「座標グラフに関数を描く」で使う)
//   書ける式の例:  x^2/2 - 3　 2x+1　 sin(x)　 3sin(2x)　 √x　 |x|　 1/x　 e^x　 log(x)　 a*x^2
//   x … 横の値、 a … 「値・進み具合」÷50 (50 で 1。アニメーションで動かせる)
// =====================================================================
public static class FormulaKit
{
    /// <summary>式を読んで、x と a から y を計算する関数にする。読めない時は null と、その理由</summary>
    public static Func<double, double, double>? Compile(string? text, out string error)
    {
        error = "";
        var src = Normalize(text);
        if (src.Length == 0) { error = "式が空です"; return null; }
        try
        {
            var p = new Parser(src);
            var f = p.ParseExpression();
            if (!p.End) throw new FormatException($"「{src[p.Pos..]}」の所が読めません");
            return f;
        }
        catch (Exception e)
        {
            error = e.Message;
            return null;
        }
    }

    static readonly Dictionary<string, (Func<double, double, double>? f, string err)> cache = new();

    /// <summary>同じ式は、読むのを1回だけにする</summary>
    public static Func<double, double, double>? Cached(string? text, out string error)
    {
        text ??= "";
        lock (cache)
        {
            if (cache.TryGetValue(text, out var hit)) { error = hit.err; return hit.f; }
            var f = Compile(text, out error);
            if (cache.Count > 200) cache.Clear();
            cache[text] = (f, error);
            return f;
        }
    }

    /// <summary>「y =」「f(x) =」を外し、全角や記号(× ÷ − ² √ π)をそろえる</summary>
    public static string Normalize(string? text)
    {
        var s = (text ?? "").Trim().Replace("²", "^2").Replace("³", "^3");   // 先に置きかえる(NFKC だと ² が 2 になるため)
        int eq = s.LastIndexOf('=');
        if (eq < 0) eq = s.LastIndexOf('＝');
        if (eq >= 0) s = s[(eq + 1)..];
        var sb = new StringBuilder();
        foreach (var ch0 in s.Normalize(NormalizationForm.FormKC))
        {
            char ch = ch0;
            switch (ch)
            {
                case '×': case '・': case '*': sb.Append('*'); break;
                case '÷': case '/': sb.Append('/'); break;
                case '−': case '–': case 'ー': case '-': sb.Append('-'); break;
                case 'π': sb.Append("pi"); break;
                case '√': sb.Append("sqrt"); break;
                case ' ': case '\t': case '\r': case '\n': break;
                case 'X': sb.Append('x'); break;
                default: sb.Append(ch); break;
            }
        }
        return sb.ToString();
    }

    /// <summary>画面に出す時の、読みやすい書き方(x^2 → x²、* → ×)</summary>
    public static string Pretty(string? text)
    {
        var s = Normalize(text);
        s = s.Replace("^2", "²").Replace("^3", "³").Replace("*", "×").Replace("/", "÷").Replace("pi", "π").Replace("sqrt", "√");
        s = s.Replace("+", " + ").Replace("-", " − ").Replace("( − ", "(−");
        if (s.StartsWith(" − ")) s = "−" + s[3..];
        return "y = " + s.Trim();
    }

    // ------------------------------------------------------------------ 式を読む(足し算 → かけ算 → べき乗 → 数・関数 の順)
    sealed class Parser(string s)
    {
        int i;
        public int Pos => i;
        public bool End => i >= s.Length;
        char Peek => i < s.Length ? s[i] : '\0';

        public Func<double, double, double> ParseExpression()
        {
            var left = ParseTerm();
            while (Peek is '+' or '-')
            {
                char op = s[i++];
                var l = left; var r = ParseTerm();
                left = op == '+' ? (x, a) => l(x, a) + r(x, a) : (x, a) => l(x, a) - r(x, a);
            }
            return left;
        }

        Func<double, double, double> ParseTerm()
        {
            var left = ParseUnary();
            while (true)
            {
                if (Peek is '*' or '/')
                {
                    char op = s[i++];
                    var l = left; var r = ParseUnary();
                    left = op == '*' ? (x, a) => l(x, a) * r(x, a) : (x, a) => l(x, a) / r(x, a);
                }
                else if (StartsFactor())
                {
                    // 2x、3(x+1)、x sin(x) のような、かけ算の記号の省略
                    var l = left; var r = ParsePower();
                    left = (x, a) => l(x, a) * r(x, a);
                }
                else return left;
            }
        }

        bool StartsFactor() => Peek == '(' || Peek == '|' && !closingBar || char.IsLetter(Peek) || char.IsDigit(Peek) || Peek == '.';
        bool closingBar;

        Func<double, double, double> ParseUnary()
        {
            if (Peek == '-') { i++; var v = ParseUnary(); return (x, a) => -v(x, a); }
            if (Peek == '+') { i++; return ParseUnary(); }
            return ParsePower();
        }

        Func<double, double, double> ParsePower()
        {
            var b = ParseAtom();
            if (Peek == '^')
            {
                i++;
                var e = ParseUnary();   // 2^-x、x^2^2 は右から
                var bb = b;
                return (x, a) => Math.Pow(bb(x, a), e(x, a));
            }
            return b;
        }

        Func<double, double, double> ParseAtom()
        {
            if (Peek == '(')
            {
                i++;
                bool saved = closingBar; closingBar = false;
                var v = ParseExpression();
                closingBar = saved;
                Expect(')');
                return v;
            }
            if (Peek == '|')
            {
                i++;
                bool saved = closingBar; closingBar = true;
                var v = ParseExpression();
                closingBar = saved;
                Expect('|');
                return (x, a) => Math.Abs(v(x, a));
            }
            if (char.IsDigit(Peek) || Peek == '.')
            {
                int st = i;
                while (char.IsDigit(Peek) || Peek == '.') i++;
                if (!double.TryParse(s[st..i], NumberStyles.Float, CultureInfo.InvariantCulture, out var n))
                    throw new FormatException($"数「{s[st..i]}」が読めません");
                return (_, _) => n;
            }
            if (char.IsLetter(Peek))
            {
                int st = i;
                while (char.IsLetter(Peek)) i++;
                var word = s[st..i].ToLowerInvariant();
                // 関数
                var fn = Function(word, out var used);
                if (fn != null)
                {
                    i = st + used;
                    Func<double, double, double> arg;
                    if (Peek == '(') arg = ParseAtom();
                    else arg = ParsePower();          // sin x、√x のような、かっこの省略
                    var f = fn;
                    return (x, a) => f(arg(x, a));
                }
                // 文字(x・a・pi・e)。xx や ax のようにつながっている時は、1つずつのかけ算
                i = st;
                Func<double, double, double>? acc = null;
                while (char.IsLetter(Peek))
                {
                    Func<double, double, double> one;
                    if (s.AsSpan(i).StartsWith("pi")) { i += 2; one = (_, _) => Math.PI; }
                    else
                    {
                        char c = char.ToLowerInvariant(s[i]);
                        if (Function(s[i..].ToLowerInvariant(), out _) != null) break;
                        i++;
                        one = c switch
                        {
                            'x' => (x, _) => x,
                            'a' => (_, a) => a,
                            'e' => (_, _) => Math.E,
                            _ => throw new FormatException($"「{c}」は使えません(使える文字は x・a・π・e と関数)"),
                        };
                    }
                    var prev = acc;
                    acc = prev == null ? one : (x, a) => prev(x, a) * one(x, a);
                }
                if (acc == null) throw new FormatException("読めない言葉があります");
                return acc;
            }
            throw new FormatException(End ? "式が途中で終わっています" : $"「{Peek}」の所が読めません");
        }

        void Expect(char c)
        {
            if (Peek != c) throw new FormatException($"「{c}」が足りません");
            i++;
        }

        static readonly (string name, Func<double, double> f)[] functions =
        {
            ("sqrt", Math.Sqrt), ("asin", Math.Asin), ("acos", Math.Acos), ("atan", Math.Atan),
            ("sinh", Math.Sinh), ("cosh", Math.Cosh), ("tanh", Math.Tanh),
            ("sin", Math.Sin), ("cos", Math.Cos), ("tan", Math.Tan), ("abs", Math.Abs),
            ("log", Math.Log10), ("ln", Math.Log), ("exp", Math.Exp), ("floor", Math.Floor), ("ceil", Math.Ceiling),
            ("round", Math.Round), ("sign", v => Math.Sign(v)), ("cbrt", Math.Cbrt),
        };

        /// <summary>言葉の頭が関数の名前なら、その関数(used = 使った文字数)</summary>
        static Func<double, double>? Function(string word, out int used)
        {
            foreach (var (name, f) in functions)
                if (word.StartsWith(name, StringComparison.Ordinal)) { used = name.Length; return f; }
            used = 0;
            return null;
        }
    }
}

// =====================================================================
//  関数電卓のようなボタンで式を入れる、設定画面の部品
// =====================================================================
public sealed class FormulaPadAttribute : PropertyEditorAttribute2
{
    public override FrameworkElement Create() => new FormulaPad();

    public override void SetBindings(FrameworkElement control, ItemProperty[] itemProperties)
        => ((FormulaPad)control).SetTargets(itemProperties.Select(p => p.PropertyOwner).OfType<FunctionGraphParameter>().ToArray());

    public override void ClearBindings(FrameworkElement control) => ((FormulaPad)control).SetTargets([]);
}

public sealed class FormulaPad : UserControl, IPropertyEditorControl
{
    public event EventHandler? BeginEdit;
    public event EventHandler? EndEdit;

    FunctionGraphParameter[] targets = [];
    readonly TextBox box;
    readonly TextBlock note;
    bool updating;

    static Brush B(byte r, byte g, byte b) { var x = new SolidColorBrush(Color.FromRgb(r, g, b)); x.Freeze(); return x; }
    static readonly Brush KeyBg = B(0x3A, 0x3F, 0x4A), FnBg = B(0x2E, 0x4A, 0x5A), OpBg = B(0x4A, 0x3F, 0x2E), Fore = B(0xE9, 0xEC, 0xF1), Ok = B(0x6F, 0xD0, 0x9A), Bad = B(0xFF, 0x8A, 0x80);

    // (表示, 入れる文字, 種類) 種類: n=数 f=関数 o=記号 c=操作
    static readonly (string label, string insert, char kind)[][] Keys =
    {
        new[] { ("x", "x", 'n'), ("a", "a", 'n'), ("π", "π", 'n'), ("e", "e", 'n'), ("(", "(", 'o'), (")", ")", 'o'), ("←", "BS", 'c'), ("C", "CLR", 'c') },
        new[] { ("x²", "^2", 'o'), ("x³", "^3", 'o'), ("xʸ", "^", 'o'), ("√", "√(", 'f'), ("|x|", "abs(", 'f'), ("1/x", "1/(", 'f'), ("log", "log(", 'f'), ("ln", "ln(", 'f') },
        new[] { ("sin", "sin(", 'f'), ("cos", "cos(", 'f'), ("tan", "tan(", 'f'), ("eˣ", "e^(", 'f'), ("7", "7", 'n'), ("8", "8", 'n'), ("9", "9", 'n'), ("÷", "÷", 'o') },
        new[] { ("asin", "asin(", 'f'), ("acos", "acos(", 'f'), ("atan", "atan(", 'f'), ("4", "4", 'n'), ("5", "5", 'n'), ("6", "6", 'n'), ("×", "×", 'o'), ("−", "−", 'o') },
        new[] { ("1", "1", 'n'), ("2", "2", 'n'), ("3", "3", 'n'), ("0", "0", 'n'), (".", ".", 'n'), ("+", "+", 'o'), ("◀", "LEFT", 'c'), ("▶", "RIGHT", 'c') },
    };

    public FormulaPad()
    {
        var root = new StackPanel { Margin = new Thickness(0, 2, 0, 4) };
        box = new TextBox { FontSize = 15, Padding = new Thickness(6, 4, 6, 4), FontFamily = new FontFamily("Cambria Math, Segoe UI") };
        box.TextChanged += (_, _) => { if (!updating) Commit(); };
        root.Children.Add(box);
        note = new TextBlock { FontSize = 11, Margin = new Thickness(2, 3, 0, 4), TextWrapping = TextWrapping.Wrap };
        root.Children.Add(note);
        var grid = new UniformGrid { Columns = 8 };
        foreach (var row in Keys)
            foreach (var (label, insert, kind) in row)
            {
                var b = new Button
                {
                    Content = label, Height = 28, Margin = new Thickness(1.5), FontSize = 13, Foreground = Fore, Focusable = false,
                    Background = kind == 'f' ? FnBg : kind == 'o' ? OpBg : KeyBg, BorderThickness = new Thickness(0), Cursor = Cursors.Hand,
                    ToolTip = Tip(label),
                };
                b.Click += (_, _) => Press(insert);
                grid.Children.Add(b);
            }
        root.Children.Add(grid);
        var help = new TextBlock
        {
            FontSize = 11, Opacity = 0.7, Margin = new Thickness(2, 4, 0, 0), TextWrapping = TextWrapping.Wrap,
            Text = "例: 2x+1　x²÷2−3　3sin(2x)　√x　1/x　|x|−2　a×x²(a は「値・進み具合」÷50。アニメーションで形を動かせる)",
        };
        root.Children.Add(help);
        Content = root;
    }

    static string? Tip(string label) => label switch
    {
        "a" => "「値・進み具合」÷50 の数(50 で 1)。アニメーションさせると、グラフの形が動きます",
        "xʸ" => "べき乗(2^x のように使う)",
        "|x|" => "絶対値",
        "log" => "常用対数(10 が底)",
        "ln" => "自然対数(e が底)",
        "←" => "1文字消す",
        "C" => "全部消す",
        _ => null,
    };

    public void SetTargets(FunctionGraphParameter[] props)
    {
        targets = props;
        updating = true;
        box.Text = props.Length > 0 ? props[0].Formula ?? "" : "";
        updating = false;
        Check();
    }

    void Press(string key)
    {
        int at = box.SelectionStart, len = box.SelectionLength;
        var t = box.Text;
        switch (key)
        {
            case "CLR": t = ""; at = 0; break;
            case "BS":
                if (len > 0) { t = t.Remove(at, len); }
                else if (at > 0) { t = t.Remove(at - 1, 1); at--; }
                break;
            case "LEFT": box.SelectionStart = Math.Max(0, at - 1); box.SelectionLength = 0; return;
            case "RIGHT": box.SelectionStart = Math.Min(t.Length, at + len + 1); box.SelectionLength = 0; return;
            default:
                if (len > 0) t = t.Remove(at, len);
                t = t.Insert(at, key);
                at += key.Length;
                // 関数を入れた時は、閉じかっこも付けて、中にカーソルを置く
                if (key.EndsWith('(')) t = t.Insert(at, ")");
                break;
        }
        updating = true;
        box.Text = t;
        updating = false;
        box.SelectionStart = Math.Clamp(at, 0, t.Length);
        box.SelectionLength = 0;
        Commit();
    }

    void Commit()
    {
        BeginEdit?.Invoke(this, EventArgs.Empty);
        foreach (var p in targets) p.Formula = box.Text;
        EndEdit?.Invoke(this, EventArgs.Empty);
        Check();
    }

    void Check()
    {
        var f = FormulaKit.Compile(box.Text, out var err);
        if (string.IsNullOrWhiteSpace(box.Text)) { note.Text = "空の時は「形の種類」の形を描きます"; note.Foreground = Fore; return; }
        note.Text = f != null ? "✓ " + FormulaKit.Pretty(box.Text) : "× " + err;
        note.Foreground = f != null ? Ok : Bad;
    }
}
