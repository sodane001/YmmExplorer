using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;

namespace YmmExplorer;

/// <summary>クリップボード1形式分のバイト列(Base64で保存)</summary>
public sealed class ClipFormat
{
    /// <summary>標準形式の番号(独自形式のときは0)</summary>
    public int Id { get; set; }

    /// <summary>独自形式の名前(標準形式のときはnull)</summary>
    public string? Name { get; set; }

    public string Data { get; set; } = "";

    [JsonIgnore]
    public int ByteCount => Data.Length / 4 * 3;
}

/// <summary>
/// クリップボードの中身を、.NETの変換を通さずにバイト列のまま取り出し/書き戻します。
/// YMM4でタイムラインの素材をコピー(Ctrl+C)した内容を、数値などを一切変えずに保存するために使います。
/// </summary>
internal static class ClipboardRaw
{
    const uint CF_TEXT = 1, CF_UNICODETEXT = 13, CF_HDROP = 15;
    const int MaxBytes = 64 * 1024 * 1024;

    // 受け渡しに意味のない(または再現できない)形式
    static readonly HashSet<string> SkipNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "DataObject", "Ole Private Data", "OlePrivateData", "OleClipboardPersistOnFlush",
        "Preferred DropEffect", "Shell Object Offsets",
    };

    /// <summary>現在のクリップボードを保存用に取り出す。失敗時は null と理由を返す</summary>
    public static List<ClipFormat>? Capture(IntPtr hwnd, out string error)
    {
        error = "";
        if (!Open(hwnd)) { error = "クリップボードを開けませんでした(他のアプリが使用中かもしれません)"; return null; }
        try
        {
            var list = new List<ClipFormat>();
            uint fmt = 0;
            while ((fmt = EnumClipboardFormats(fmt)) != 0)
            {
                string? name = null;
                if (fmt >= 0xC000)
                {
                    name = FormatName(fmt);
                    if (string.IsNullOrEmpty(name) || SkipNames.Contains(name)) continue;
                }
                else if (fmt != CF_TEXT && fmt != CF_UNICODETEXT && fmt != CF_HDROP)
                {
                    continue;   // 画像・メタファイルなどのハンドル形式は対象外
                }

                var h = GetClipboardData(fmt);
                if (h == IntPtr.Zero) continue;
                var size = (long)(ulong)GlobalSize(h);
                if (size <= 0 || size > MaxBytes) continue;

                var p = GlobalLock(h);
                if (p == IntPtr.Zero) continue;
                try
                {
                    var buf = new byte[size];
                    Marshal.Copy(p, buf, 0, (int)size);
                    list.Add(new ClipFormat
                    {
                        Id = name == null ? (int)fmt : 0,
                        Name = name,
                        Data = Convert.ToBase64String(buf),
                    });
                }
                finally { GlobalUnlock(h); }
            }
            return list;
        }
        finally { CloseClipboard(); }
    }

    /// <summary>保存しておいた内容をクリップボードに戻す(その後 YMM4 で Ctrl+V)</summary>
    public static bool Restore(IntPtr hwnd, IEnumerable<ClipFormat> formats, out string error)
    {
        error = "";
        if (hwnd == IntPtr.Zero) { error = "ウィンドウを取得できませんでした"; return false; }
        if (!Open(hwnd)) { error = "クリップボードを開けませんでした(他のアプリが使用中かもしれません)"; return false; }
        try
        {
            if (!EmptyClipboard()) { error = "クリップボードを空にできませんでした"; return false; }
            int ok = 0;
            foreach (var f in formats)
            {
                uint id = f.Name != null ? RegisterClipboardFormat(f.Name) : (uint)f.Id;
                if (id == 0) continue;
                byte[] bytes;
                try { bytes = Convert.FromBase64String(f.Data); }
                catch { continue; }
                if (bytes.Length == 0) continue;

                var h = GlobalAlloc(0x0002 /* GMEM_MOVEABLE */, (UIntPtr)(uint)bytes.Length);
                if (h == IntPtr.Zero) continue;
                var p = GlobalLock(h);
                if (p == IntPtr.Zero) { GlobalFree(h); continue; }
                Marshal.Copy(bytes, 0, p, bytes.Length);
                GlobalUnlock(h);
                if (SetClipboardData(id, h) == IntPtr.Zero) GlobalFree(h);   // 成功時はOSが所有
                else ok++;
            }
            if (ok == 0) error = "クリップボードに書き込めませんでした";
            return ok > 0;
        }
        finally { CloseClipboard(); }
    }

    /// <summary>保存したデータの「指紋」(内容が同じなら同じ文字列になる8文字)。別々のはずのデータが同じかどうかの確認用</summary>
    public static string Fingerprint(IReadOnlyList<ClipFormat>? formats)
    {
        if (formats == null || formats.Count == 0) return "なし";
        using var sha = System.Security.Cryptography.SHA256.Create();
        foreach (var f in formats.OrderBy(f => f.Name ?? f.Id.ToString(), StringComparer.Ordinal))
        {
            var head = Encoding.UTF8.GetBytes((f.Name ?? f.Id.ToString()) + "|");
            sha.TransformBlock(head, 0, head.Length, null, 0);
            var data = Encoding.ASCII.GetBytes(f.Data);
            sha.TransformBlock(data, 0, data.Length, null, 0);
        }
        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return Convert.ToHexString(sha.Hash!)[..8].ToLowerInvariant();
    }

    /// <summary>文字列の指紋(8文字)</summary>
    public static string FingerprintText(string text)
    {
        var hash = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash)[..8].ToLowerInvariant();
    }

    /// <summary>同じ内容のデータがすでにあれば、その名前を返す(無ければ null)</summary>
    public static string? FindSame(IReadOnlyList<ClipFormat> formats, IEnumerable<(string Name, List<ClipFormat>? Formats)> existing)
    {
        var fp = Fingerprint(formats);
        foreach (var (name, f) in existing)
            if (f is { Count: > 0 } && Fingerprint(f) == fp) return name;
        return null;
    }

    static readonly Dictionary<uint, string> StandardNames = new()
    {
        [1] = "CF_TEXT", [2] = "CF_BITMAP", [7] = "CF_OEMTEXT", [8] = "CF_DIB", [13] = "CF_UNICODETEXT",
        [14] = "CF_ENHMETAFILE", [15] = "CF_HDROP", [16] = "CF_LOCALE", [17] = "CF_DIBV5",
    };

    /// <summary>
    /// いまのクリップボードの中身を、文章で説明する(YMM4でコピーした内容が、どんな形式で入っているかを調べる用)。
    /// 保存する形式・しない形式の区別と、データの先頭部分も出す。
    /// </summary>
    public static string Describe()
    {
        if (!Open(IntPtr.Zero)) return "クリップボードを開けませんでした(他のアプリが使用中かもしれません)。もう一度お試しください。";
        try
        {
            var formats = new List<uint>();
            uint f = 0;
            while ((f = EnumClipboardFormats(f)) != 0) formats.Add(f);

            var sb = new StringBuilder();
            sb.AppendLine($"クリップボードの形式: {formats.Count} 個 / 変更番号 {SequenceNumber}");
            if (formats.Count == 0) sb.AppendLine("(空です)");

            int i = 0;
            foreach (var fmt in formats)
            {
                i++;
                var custom = fmt >= 0xC000;
                var name = custom ? FormatName(fmt) : (StandardNames.TryGetValue(fmt, out var n) ? n : "標準形式");
                var saved = custom
                    ? !string.IsNullOrEmpty(name) && !SkipNames.Contains(name)
                    : fmt == CF_TEXT || fmt == CF_UNICODETEXT || fmt == CF_HDROP;

                // ビットマップなどのハンドル形式は、サイズを調べられない
                var handleType = !custom && (fmt == 2 || fmt == 3 || fmt == 9 || fmt == 14 || (fmt >= 0x80 && fmt < 0x400));
                string size = "サイズ不明", preview = "";
                if (!handleType)
                {
                    var h = GetClipboardData(fmt);
                    if (h != IntPtr.Zero)
                    {
                        var len = (long)(ulong)GlobalSize(h);
                        size = FormatSize(len);
                        var p = len > 0 ? GlobalLock(h) : IntPtr.Zero;
                        if (p != IntPtr.Zero)
                        {
                            try
                            {
                                var n2 = (int)Math.Min(len, 240);
                                var head = new byte[n2];
                                Marshal.Copy(p, head, 0, n2);
                                preview = Preview(head);
                            }
                            finally { GlobalUnlock(h); }
                        }
                    }
                }

                sb.AppendLine($"[{i}] {(string.IsNullOrEmpty(name) ? "(名前なし)" : name)} (番号 {fmt}) / {size} / {(saved ? "保存する" : "保存しない")}");
                if (preview.Length > 0) sb.AppendLine("      先頭: " + preview);
            }
            return sb.ToString();
        }
        finally { CloseClipboard(); }
    }

    static string FormatSize(long bytes)
        => bytes < 1024 ? $"{bytes} バイト" : $"{bytes / 1024.0:0.#} KB";

    // データの先頭を、読める形(UTF-8 / UTF-16の文字、または16進)で見せる
    static string Preview(byte[] head)
    {
        static string Clean(string t)
        {
            var chars = t.Select(c => char.IsControl(c) ? ' ' : c).ToArray();
            var s = new string(chars).Trim();
            return s.Length > 110 ? s[..110] + "…" : s;
        }
        static bool Readable(string t) => t.Length > 0 && t.Count(c => char.IsControl(c) && c != '\r' && c != '\n' && c != '\t') * 8 <= t.Length;

        var utf8 = Encoding.UTF8.GetString(head);
        if (Readable(utf8) && !utf8.Contains('\uFFFD')) return "\"" + Clean(utf8) + "\"";
        if (head.Length >= 4 && head.Length % 2 == 0)
        {
            var utf16 = Encoding.Unicode.GetString(head);
            if (Readable(utf16)) return "\"" + Clean(utf16) + "\" (UTF-16)";
        }
        return "(バイナリ) " + BitConverter.ToString(head, 0, Math.Min(head.Length, 24));
    }

    /// <summary>クリップボードの中身が変わるたびに増える番号(コピーされたかどうかの検知用)</summary>
    public static uint SequenceNumber => GetClipboardSequenceNumber();

    /// <summary>
    /// 取り出した内容が、YMM4のアイテムのコピーらしいか。
    /// 独自形式のデータがあるか、JSONのような文字列があれば「らしい」とみなします。
    /// </summary>
    public static bool LooksLikeData(IReadOnlyList<ClipFormat> formats)
    {
        if (formats.Any(f => f.Name != null)) return true;
        foreach (var f in formats.Where(f => f.Id == (int)CF_UNICODETEXT))
        {
            try
            {
                var text = Encoding.Unicode.GetString(Convert.FromBase64String(f.Data)).TrimStart('\0', ' ', '\r', '\n', '\t');
                if (text.StartsWith('{') || text.StartsWith('[')) return true;
            }
            catch { }
        }
        return false;
    }

    /// <summary>
    /// 書き戻したあと、クリップボードを読み直して、書いたとおりに入っているか確認する。
    /// id は、書き込んだ内容の指紋(画面に出して、別々のデータが別々に書き込まれているか見分けられるようにする)
    /// </summary>
    public static bool RestoreVerified(IntPtr hwnd, IReadOnlyList<ClipFormat> formats, out string error, out string id)
    {
        id = Fingerprint(formats);
        if (!Restore(hwnd, formats, out error)) return false;

        var back = Capture(IntPtr.Zero, out _);
        if (back == null) return true;      // 読み直せない時は、書き込めたものとして進める
        foreach (var f in formats)
        {
            var found = back.FirstOrDefault(b => b.Id == f.Id && b.Name == f.Name);
            if (found == null || found.Data != f.Data)
            {
                error = "クリップボードに、書き込んだ内容がそのまま入っていません(他のアプリが書き換えた可能性があります)。もう一度お試しください";
                return false;
            }
        }
        return true;
    }

    static bool Open(IntPtr hwnd)
    {
        for (int i = 0; i < 10; i++)
        {
            if (OpenClipboard(hwnd)) return true;
            Thread.Sleep(30);
        }
        return false;
    }

    static string FormatName(uint fmt)
    {
        var sb = new StringBuilder(256);
        return GetClipboardFormatName(fmt, sb, sb.Capacity) > 0 ? sb.ToString() : "";
    }

    [DllImport("user32.dll")] static extern uint GetClipboardSequenceNumber();
    [DllImport("user32.dll", SetLastError = true)] static extern bool OpenClipboard(IntPtr hWndNewOwner);
    [DllImport("user32.dll")] static extern bool CloseClipboard();
    [DllImport("user32.dll")] static extern bool EmptyClipboard();
    [DllImport("user32.dll")] static extern uint EnumClipboardFormats(uint format);
    [DllImport("user32.dll")] static extern IntPtr GetClipboardData(uint uFormat);
    [DllImport("user32.dll")] static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern int GetClipboardFormatName(uint format, StringBuilder lpszFormatName, int cchMaxCount);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    static extern uint RegisterClipboardFormat(string lpszFormat);

    [DllImport("kernel32.dll")] static extern IntPtr GlobalLock(IntPtr hMem);
    [DllImport("kernel32.dll")] static extern bool GlobalUnlock(IntPtr hMem);
    [DllImport("kernel32.dll")] static extern UIntPtr GlobalSize(IntPtr hMem);
    [DllImport("kernel32.dll")] static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
    [DllImport("kernel32.dll")] static extern IntPtr GlobalFree(IntPtr hMem);
}
