using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YmmExplorer;

/// <summary>シェル(COM)を呼ぶためのSTAスレッド。サムネイルやプロパティ取得を裏で実行します</summary>
internal static class StaWorker
{
    static readonly BlockingCollection<Action> queue = new();

    static StaWorker()
    {
        for (int i = 0; i < 3; i++)
        {
            var t = new Thread(() =>
            {
                foreach (var a in queue.GetConsumingEnumerable())
                {
                    try { a(); } catch { }
                }
            })
            { IsBackground = true, Name = "YmmExplorer.Sta" + i };
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }
    }

    public static Task<T?> Run<T>(Func<T?> func)
    {
        var tcs = new TaskCompletionSource<T?>(TaskCreationOptions.RunContinuationsAsynchronously);
        queue.Add(() =>
        {
            try { tcs.SetResult(func()); }
            catch { tcs.SetResult(default); }
        });
        return tcs.Task;
    }
}

/// <summary>Windowsシェルのサムネイル(動画・PDF・Office・PSDなど)を取得します</summary>
internal static class ShellThumbnail
{
    public static Task<BitmapSource?> GetAsync(string path, int size)
        => StaWorker.Run<BitmapSource>(() => Get(path, size));

    static BitmapSource? Get(string path, int size)
    {
        var iid = typeof(IShellItemImageFactory).GUID;
        if (SHCreateItemFromParsingName(path, IntPtr.Zero, ref iid, out var factory) != 0 || factory == null)
            return null;

        IntPtr hbmp = IntPtr.Zero;
        try
        {
            // サムネイルが無いファイルは失敗を返す(=アイコン表示のまま)
            if (factory.GetImage(new SIZE { cx = size, cy = size }, SIIGBF.ThumbnailOnly, out hbmp) != 0
                || hbmp == IntPtr.Zero)
                return null;
            return ToBitmapSource(hbmp);
        }
        finally
        {
            if (hbmp != IntPtr.Zero) DeleteObject(hbmp);
            Marshal.ReleaseComObject(factory);
        }
    }

    static BitmapSource? ToBitmapSource(IntPtr hbmp)
    {
        if (GetObject(hbmp, Marshal.SizeOf<BITMAP>(), out BITMAP bm) == 0) return null;
        int w = bm.bmWidth, h = Math.Abs(bm.bmHeight);
        if (w <= 0 || h <= 0) return null;

        // 上から下の32bitとして取り出す(向きの違いを吸収)
        var bi = new BITMAPINFOHEADER
        {
            biSize = Marshal.SizeOf<BITMAPINFOHEADER>(),
            biWidth = w,
            biHeight = -h,
            biPlanes = 1,
            biBitCount = 32,
        };
        var buf = new byte[w * h * 4];
        var hdc = GetDC(IntPtr.Zero);
        int lines;
        try { lines = GetDIBits(hdc, hbmp, 0, (uint)h, buf, ref bi, 0); }
        finally { ReleaseDC(IntPtr.Zero, hdc); }
        if (lines == 0) return null;

        // アルファ情報が無いビットマップは不透明にする
        bool hasAlpha = false;
        for (int i = 3; i < buf.Length; i += 4)
            if (buf[i] != 0) { hasAlpha = true; break; }
        if (!hasAlpha)
            for (int i = 3; i < buf.Length; i += 4) buf[i] = 255;

        var bs = BitmapSource.Create(w, h, 96, 96, PixelFormats.Pbgra32, null, buf, w * 4);
        bs.Freeze();
        return bs;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SIZE { public int cx, cy; }

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAP
    {
        public int bmType, bmWidth, bmHeight, bmWidthBytes;
        public ushort bmPlanes, bmBitsPixel;
        public IntPtr bmBits;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFOHEADER
    {
        public int biSize, biWidth, biHeight;
        public ushort biPlanes, biBitCount;
        public uint biCompression, biSizeImage;
        public int biXPelsPerMeter, biYPelsPerMeter;
        public uint biClrUsed, biClrImportant;
    }

    [Flags]
    enum SIIGBF
    {
        ResizeToFit = 0,
        BiggerSizeOk = 1,
        MemoryOnly = 2,
        IconOnly = 4,
        ThumbnailOnly = 8,
        InCacheOnly = 0x10,
    }

    [ComImport, Guid("BCC18B79-BA16-442F-80C4-8A59C30C463B"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellItemImageFactory
    {
        [PreserveSig] int GetImage(SIZE size, SIIGBF flags, out IntPtr phbm);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    static extern int SHCreateItemFromParsingName(
        string pszPath, IntPtr pbc, ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IShellItemImageFactory ppv);

    [DllImport("gdi32.dll")] static extern bool DeleteObject(IntPtr h);
    [DllImport("gdi32.dll", EntryPoint = "GetObjectW")] static extern int GetObject(IntPtr h, int c, out BITMAP pv);
    [DllImport("gdi32.dll")]
    static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint cLines,
                                byte[]? lpvBits, ref BITMAPINFOHEADER lpbmi, uint usage);
    [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
}

/// <summary>動画・音声・画像のメタ情報(詳細パネル用)</summary>
public sealed class FileMeta
{
    public int? Width, Height;
    public TimeSpan? Duration;
    public double? FrameRate;
    public long? Bitrate;
    public int? SampleRate, Channels;
    public int? PdfPages;
}

/// <summary>シェルのプロパティシステムから再生時間・解像度などを読みます</summary>
internal static class ShellProps
{
    static readonly Guid Media = new("64440490-4C8B-11D1-8B70-080036B11A03");
    static readonly Guid Video = new("64440491-4C8B-11D1-8B70-080036B11A03");
    static readonly Guid Image = new("6444048F-4C8B-11D1-8B70-080036B11A03");

    public static Task<FileMeta?> ReadAsync(string path) => StaWorker.Run<FileMeta>(() => Read(path));

    static FileMeta? Read(string path)
    {
        var iid = typeof(IPropertyStore).GUID;
        if (SHGetPropertyStoreFromParsingName(path, IntPtr.Zero, 0, ref iid, out var store) != 0 || store == null)
            return null;
        try
        {
            var m = new FileMeta();
            if (Num(store, Media, 3) is ulong d && d > 0) m.Duration = TimeSpan.FromTicks((long)d);

            if (Num(store, Video, 3) is ulong vw && vw > 0 && Num(store, Video, 4) is ulong vh && vh > 0)
            { m.Width = (int)vw; m.Height = (int)vh; }
            else if (Num(store, Image, 3) is ulong iw && iw > 0 && Num(store, Image, 4) is ulong ih && ih > 0)
            { m.Width = (int)iw; m.Height = (int)ih; }

            if (Num(store, Video, 6) is ulong fr && fr > 0) m.FrameRate = fr / 1000.0;

            var br = Num(store, Video, 8) ?? Num(store, Media, 4);
            if (br is ulong b && b > 0) m.Bitrate = (long)b;

            if (Num(store, Media, 5) is ulong sr && sr > 0) m.SampleRate = (int)sr;
            if (Num(store, Media, 7) is ulong ch && ch > 0) m.Channels = (int)ch;
            return m;
        }
        finally { Marshal.ReleaseComObject(store); }
    }

    static ulong? Num(IPropertyStore s, Guid fmt, uint pid)
    {
        var key = new PROPERTYKEY { fmtid = fmt, pid = pid };
        if (s.GetValue(ref key, out var pv) != 0) return null;
        try
        {
            switch (pv.vt)
            {
                case 3:   // VT_I4
                    return pv.u4 <= int.MaxValue ? (ulong?)pv.u4 : null;
                case 19:  // VT_UI4
                    return pv.u4;
                case 20:  // VT_I8
                case 21:  // VT_UI8
                    return pv.u8;
                default:
                    return null;
            }
        }
        finally { PropVariantClear(ref pv); }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct PROPERTYKEY { public Guid fmtid; public uint pid; }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    struct PROPVARIANT
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(8)] public uint u4;
        [FieldOffset(8)] public ulong u8;
        [FieldOffset(8)] public IntPtr ptr;
    }

    [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        [PreserveSig] int GetCount(out uint cProps);
        [PreserveSig] int GetAt(uint iProp, out PROPERTYKEY pkey);
        [PreserveSig] int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        [PreserveSig] int SetValue(ref PROPERTYKEY key, ref PROPVARIANT pv);
        [PreserveSig] int Commit();
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    static extern int SHGetPropertyStoreFromParsingName(
        string pszPath, IntPtr pbc, int flags, ref Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out IPropertyStore ppv);

    [DllImport("ole32.dll")] static extern int PropVariantClear(ref PROPVARIANT pv);
}

/// <summary>シェル機能(プロパティダイアログ)</summary>
internal static class ShellActions
{
    /// <summary>Windows標準の「プロパティ」ダイアログを開きます</summary>
    public static bool ShowProperties(string path)
    {
        var info = new SHELLEXECUTEINFO
        {
            cbSize = Marshal.SizeOf<SHELLEXECUTEINFO>(),
            fMask = 0x0000000C,   // SEE_MASK_INVOKEIDLIST
            lpVerb = "properties",
            lpFile = path,
            nShow = 1,            // SW_SHOWNORMAL
        };
        return ShellExecuteEx(ref info);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;
        public string? lpVerb;
        public string? lpFile;
        public string? lpParameters;
        public string? lpDirectory;
        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;
        public string? lpClass;
        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIconOrMonitor;
        public IntPtr hProcess;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool ShellExecuteEx(ref SHELLEXECUTEINFO lpExecInfo);
}
