using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YmmExplorer;

/// <summary>
/// Windows の再生機能(MediaElement)が苦手な WAV(24bit・32bit・浮動小数・6ch など)を、
/// 再生できる形(16bit・ステレオ)に変えた一時ファイルを作る。元のファイルは変えない。
/// AI音声生成(Stable Audio 3)が作る 24bit の WAV が、そのままではプレビューできないため。
/// </summary>
public static class AudioCompat
{
    static readonly string CacheDir = Path.Combine(Path.GetTempPath(), "YmmExplorer", "playable");

    /// <summary>再生に使うファイルの場所。変換が要らない・できない時は、元の場所をそのまま返す</summary>
    public static string PlayablePath(string path)
    {
        try
        {
            if (!string.Equals(Path.GetExtension(path), ".wav", StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return path;
            if (!ReadFormat(path, out var fmt) || fmt.IsPlainPcm16) return path;

            var info = new FileInfo(path);
            var key = Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes($"{info.FullName}|{info.Length}|{info.LastWriteTimeUtc.Ticks}"))).Substring(0, 16);
            Directory.CreateDirectory(CacheDir);
            var dest = Path.Combine(CacheDir, key + ".wav");
            if (File.Exists(dest)) return dest;
            var tmp = dest + ".part";
            Convert16(path, fmt, tmp);
            File.Move(tmp, dest, true);
            return dest;
        }
        catch { return path; }
    }

    sealed record WavFormat(int Format, int Channels, int Rate, int Bits, long DataPos, long DataLen)
    {
        public bool IsPlainPcm16 => Format == 1 && Bits == 16 && Channels <= 2;
    }

    static bool ReadFormat(string path, out WavFormat fmt)
    {
        fmt = new WavFormat(0, 0, 0, 0, -1, 0);
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var br = new BinaryReader(fs);
        if (fs.Length < 12 || new string(br.ReadChars(4)) != "RIFF") return false;
        br.ReadInt32();
        if (new string(br.ReadChars(4)) != "WAVE") return false;
        int format = 0, channels = 0, rate = 0, bits = 0;
        long dataPos = -1, dataLen = 0;
        while (fs.Position + 8 <= fs.Length)
        {
            var id = new string(br.ReadChars(4));
            long len = br.ReadUInt32();
            long next = fs.Position + len + (len & 1);
            if (id == "fmt ")
            {
                format = br.ReadUInt16();
                channels = br.ReadUInt16();
                rate = br.ReadInt32();
                br.ReadInt32(); br.ReadUInt16();
                bits = br.ReadUInt16();
                // WAVE_FORMAT_EXTENSIBLE: 中身の本当の形式(PCM か 浮動小数)を読む
                if (format == 0xFFFE && len >= 26) { br.ReadUInt16(); br.ReadUInt16(); br.ReadUInt32(); format = br.ReadUInt16(); }
            }
            else if (id == "data")
            {
                dataPos = fs.Position;
                dataLen = (len == 0 || dataPos + len > fs.Length) ? fs.Length - dataPos : len;
                break;
            }
            if (next > fs.Length) break;
            fs.Position = next;
        }
        if (dataPos < 0 || channels <= 0 || rate <= 0 || (format != 1 && format != 3)) return false;
        if (format == 1 && bits is not (8 or 16 or 24 or 32)) return false;
        if (format == 3 && bits is not (32 or 64)) return false;
        fmt = new WavFormat(format, channels, rate, bits, dataPos, dataLen);
        return true;
    }

    /// <summary>16bit・ステレオ(元がモノラルならモノラル)の PCM に変える。3ch 以上は、左右へまとめる</summary>
    static void Convert16(string src, WavFormat f, string dest)
    {
        int inBytes = f.Bits / 8, outCh = Math.Min(2, f.Channels);
        int frameIn = inBytes * f.Channels;
        long frames = f.DataLen / frameIn;
        using var input = new FileStream(src, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var output = new FileStream(dest, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(output);
        long dataBytes = frames * outCh * 2;
        bw.Write(Encoding.ASCII.GetBytes("RIFF")); bw.Write((uint)(36 + dataBytes)); bw.Write(Encoding.ASCII.GetBytes("WAVE"));
        bw.Write(Encoding.ASCII.GetBytes("fmt ")); bw.Write(16); bw.Write((short)1); bw.Write((short)outCh); bw.Write(f.Rate);
        bw.Write(f.Rate * outCh * 2); bw.Write((short)(outCh * 2)); bw.Write((short)16);
        bw.Write(Encoding.ASCII.GetBytes("data")); bw.Write((uint)dataBytes);

        input.Position = f.DataPos;
        const int chunkFrames = 16384;
        var buf = new byte[chunkFrames * frameIn];
        var outBuf = new byte[chunkFrames * outCh * 2];
        long done = 0;
        while (done < frames)
        {
            int want = (int)Math.Min(chunkFrames, frames - done);
            int got = 0;
            while (got < want * frameIn)
            {
                int n = input.Read(buf, got, want * frameIn - got);
                if (n <= 0) break;
                got += n;
            }
            int gotFrames = got / frameIn;
            if (gotFrames <= 0) break;
            int o = 0;
            for (int i = 0; i < gotFrames; i++)
            {
                for (int oc = 0; oc < outCh; oc++)
                {
                    // 3ch 以上: 偶数番目の ch を左、奇数番目を右に足して平均する
                    double sum = 0;
                    int count = 0;
                    for (int c = oc; c < f.Channels; c += (f.Channels > 2 ? 2 : f.Channels))
                    {
                        sum += Sample(buf, (i * f.Channels + c) * inBytes, f);
                        count++;
                        if (f.Channels <= 2) break;
                    }
                    double v = Math.Clamp(count == 0 ? 0 : sum / count, -1, 1);
                    short s = (short)Math.Round(v * 32767);
                    outBuf[o++] = (byte)s;
                    outBuf[o++] = (byte)(s >> 8);
                }
            }
            output.Write(outBuf, 0, o);
            done += gotFrames;
        }
    }

    static double Sample(byte[] b, int o, WavFormat f) => f.Format == 3
        ? (f.Bits == 64 ? BitConverter.ToDouble(b, o) : BitConverter.ToSingle(b, o))
        : f.Bits switch
        {
            8 => (b[o] - 128) / 128.0,
            16 => BitConverter.ToInt16(b, o) / 32768.0,
            24 => ((b[o] << 8 | b[o + 1] << 16 | b[o + 2] << 24) >> 8) / 8388608.0,
            _ => BitConverter.ToInt32(b, o) / 2147483648.0,
        };

    /// <summary>作った一時ファイルを消す(YMM4の終了時など)</summary>
    public static void Cleanup()
    {
        try { if (Directory.Exists(CacheDir)) Directory.Delete(CacheDir, true); } catch { }
    }
}

/// <summary>
/// GPUの使用率とメモリ(VRAM)を調べる。NVIDIA のドライバーに入っている nvidia-smi を使う
/// (Audio Studio MCP は Windows では NVIDIA の GPU で動くため)。
/// </summary>
public static class GpuMonitor
{
    public sealed record GpuInfo(string Name, double Usage, double MemUsedMb, double MemTotalMb);

    static string? exe;

    static string? FindExe()
    {
        if (exe != null) return exe.Length == 0 ? null : exe;
        var candidates = new List<string>
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "nvidia-smi.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA Corporation", "NVSMI", "nvidia-smi.exe"),
        };
        foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries))
            candidates.Add(Path.Combine(dir.Trim(), "nvidia-smi.exe"));
        exe = candidates.FirstOrDefault(File.Exists) ?? "";
        return exe.Length == 0 ? null : exe;
    }

    /// <summary>いまの値(取れない時は null)。少し時間がかかるので、裏で呼ぶこと</summary>
    public static GpuInfo? Read()
    {
        var path = FindExe();
        if (path == null) return null;
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo(path, "--query-gpu=name,utilization.gpu,memory.used,memory.total --format=csv,noheader,nounits")
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true,
            };
            using var p = System.Diagnostics.Process.Start(psi);
            if (p == null) return null;
            var line = p.StandardOutput.ReadLine() ?? "";
            if (!p.WaitForExit(3000)) { try { p.Kill(); } catch { } }
            var parts = line.Split(',').Select(x => x.Trim()).ToArray();
            if (parts.Length < 4) return null;
            static double N(string s) => double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
            return new GpuInfo(parts[0], N(parts[1]), N(parts[2]), N(parts[3]));
        }
        catch { return null; }
    }
}

