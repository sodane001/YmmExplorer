using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows.Media.Imaging;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace YmmExplorer;

/// <summary>
/// 動画・音声を AI に見せる。
/// ・どの AI でも: 長さ・大きさ・フレームレート、動画はコマの画像、音の大きさの移り変わり(dB)・無音・音割れ
/// ・Gemini: 動画・音声そのものをアップロードして、AI に直接見て・聞いてもらう
/// ・ChatGPT: 文字起こし(話している内容と時間)を付ける(OpenAI の文字起こし API。別料金)
/// </summary>
public static class AiMedia
{
    static readonly HashSet<string> VideoExt = new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".mov", ".m4v", ".avi", ".wmv", ".webm", ".mkv", ".flv", ".mpg", ".mpeg", ".3gp", ".ts", ".m2ts", ".mts" };
    static readonly HashSet<string> AudioExt = new(StringComparer.OrdinalIgnoreCase) { ".wav", ".mp3", ".m4a", ".aac", ".flac", ".ogg", ".opus", ".wma", ".aif", ".aiff" };
    public static bool IsVideo(string p) => VideoExt.Contains(Path.GetExtension(p));
    public static bool IsAudio(string p) => AudioExt.Contains(Path.GetExtension(p));
    public static bool IsMedia(string p) => IsVideo(p) || IsAudio(p);

    /// <summary>Gemini がそのまま受け取れる形式</summary>
    static string? GeminiMime(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".mp4" or ".m4v" => "video/mp4", ".mov" => "video/mov", ".avi" => "video/avi", ".wmv" => "video/wmv", ".webm" => "video/webm",
        ".flv" => "video/x-flv", ".mpg" or ".mpeg" => "video/mpeg", ".3gp" => "video/3gpp",
        ".wav" => "audio/wav", ".mp3" => "audio/mp3", ".aac" => "audio/aac", ".flac" => "audio/flac", ".ogg" => "audio/ogg", ".aif" or ".aiff" => "audio/aiff",
        _ => null,
    };

    static readonly HttpClient upload = new() { Timeout = Timeout.InfiniteTimeSpan };

    public sealed class Result
    {
        public StringBuilder Text { get; } = new();
        public List<(string, string, string)> Images { get; } = new();
        public List<AiFilePart> Files { get; } = new();
    }

    static string Clock(double s) { var t = TimeSpan.FromSeconds(Math.Max(0, s)); return t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss"); }
    static string Size(long b) => b >= 1 << 30 ? $"{b / (double)(1 << 30):0.#} GB" : b >= 1 << 20 ? $"{b / (double)(1 << 20):0.#} MB" : $"{b / 1024.0:0.#} KB";

    /// <summary>1つの動画・音声を調べて、AI に送れる形にする</summary>
    public static async Task<Result> DescribeAsync(string path, string shownName, string provider, string key, bool transcribe, int frames, int maxSide,
                                                   Action<string> status, CancellationToken ct)
    {
        var r = new Result();
        bool video = IsVideo(path);
        var fi = new FileInfo(path);
        r.Text.AppendLine($"### {(video ? "動画" : "音声")} {shownName}({Size(fi.Length)})");
        string? wav = null;
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path).AsTask(ct);

            // 1) 長さ・大きさ
            double duration = 0;
            try
            {
                if (video)
                {
                    var vp = await file.Properties.GetVideoPropertiesAsync().AsTask(ct);
                    duration = vp.Duration.TotalSeconds;
                    r.Text.AppendLine($"長さ {Clock(duration)} / 画面 {vp.Width}×{vp.Height}" + (vp.Bitrate > 0 ? $" / {vp.Bitrate / 1000} kbps" : ""));
                }
                else
                {
                    var mp = await file.Properties.GetMusicPropertiesAsync().AsTask(ct);
                    duration = mp.Duration.TotalSeconds;
                    var tags = new List<string>();
                    if (!string.IsNullOrEmpty(mp.Title)) tags.Add("曲名: " + mp.Title);
                    if (!string.IsNullOrEmpty(mp.Artist)) tags.Add("アーティスト: " + mp.Artist);
                    r.Text.AppendLine($"長さ {Clock(duration)}" + (mp.Bitrate > 0 ? $" / {mp.Bitrate / 1000} kbps" : "") + (tags.Count > 0 ? " / " + string.Join(" / ", tags) : ""));
                }
            }
            catch { }

            // 2) Gemini: ファイルそのものを送る
            bool native = false;
            if (provider == "gemini" && key.Length > 0)
            {
                var mime = GeminiMime(path);
                if (mime != null && fi.Length <= 1_900_000_000)
                {
                    try
                    {
                        status($"「{shownName}」を Gemini にアップロードしています…({Size(fi.Length)})");
                        var uri = await GeminiUploadAsync(key, path, mime, shownName, status, ct);
                        r.Files.Add(new AiFilePart { Mime = mime, Uri = uri, FileName = shownName, Caption = $"[添付{(video ? "動画" : "音声")}] {shownName}" });
                        r.Text.AppendLine($"({(video ? "動画" : "音声")}そのものを添付しています。直接見て・聞いて判断してください)");
                        native = true;
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex) { r.Text.AppendLine("(アップロードできなかったため、下の解析結果だけ送ります: " + ex.Message + ")"); }
                }
            }

            // 3) 動画のコマ(Gemini に動画そのものを送れた時は省く)
            if (video && !native && frames > 0)
            {
                status($"「{shownName}」のコマを取り出しています…");
                try
                {
                    var clip = await MediaClip.CreateFromFileAsync(file).AsTask(ct);
                    if (duration <= 0) duration = clip.OriginalDuration.TotalSeconds;
                    try
                    {
                        var ep = clip.GetVideoEncodingProperties();
                        if (ep.FrameRate.Denominator > 0) r.Text.AppendLine($"フレームレート {ep.FrameRate.Numerator / (double)ep.FrameRate.Denominator:0.##} fps");
                    }
                    catch { }
                    var comp = new MediaComposition();
                    comp.Clips.Add(clip);
                    var times = Enumerable.Range(0, frames).Select(k => TimeSpan.FromSeconds(Math.Max(0, duration) * (k + 0.5) / frames)).ToList();
                    var w = clip.GetVideoEncodingProperties();
                    double scale = Math.Min(1.0, maxSide / (double)Math.Max(1, Math.Max(w.Width, w.Height)));
                    int tw = Math.Max(16, (int)(w.Width * scale)), th = Math.Max(16, (int)(w.Height * scale));
                    var shots = await comp.GetThumbnailsAsync(times, tw, th, VideoFramePrecision.NearestFrame).AsTask(ct);
                    for (int i = 0; i < shots.Count; i++)
                    {
                        using var s = shots[i].AsStreamForRead();
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = s;
                        bmp.EndInit();
                        bmp.Freeze();
                        var enc = ProjectSnapshot.Encode(bmp, maxSide);
                        if (enc != null) r.Images.Add((enc.Value.mime, enc.Value.base64, $"[添付動画のコマ] {shownName} の {Clock(times[i].TotalSeconds)} の画面"));
                    }
                    r.Text.AppendLine($"(動画から {r.Images.Count} コマを画像にして添付しています)");
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { r.Text.AppendLine("(コマを取り出せませんでした: " + ex.Message + ")"); }
            }

            // 4) 音の大きさ(どの AI にも、数値で送る)
            if (duration <= 0 || duration <= 3 * 3600)
            {
                status($"「{shownName}」の音を調べています…");
                try
                {
                    wav = Path.Combine(Path.GetTempPath(), "YmmExplorer_ai_" + Guid.NewGuid().ToString("N") + ".wav");
                    if (!await TranscodeAsync(file, wav, MediaEncodingProfile.CreateWav(AudioEncodingQuality.Low), ct)) { r.Text.AppendLine("(音声を取り出せませんでした)"); wav = null; }
                    else
                    {
                        var text = await Task.Run(() => Loudness(wav), ct);
                        r.Text.Append(text ?? "(音を調べられませんでした)\n");
                        // Gemini が受け取れない形式の音声(m4a など)は、WAV にしたものを送る
                        if (provider == "gemini" && !native && !video && key.Length > 0 && new FileInfo(wav).Length <= 1_900_000_000)
                        {
                            try
                            {
                                status($"「{shownName}」を Gemini にアップロードしています…");
                                var uri = await GeminiUploadAsync(key, wav, "audio/wav", shownName, status, ct);
                                r.Files.Add(new AiFilePart { Mime = "audio/wav", Uri = uri, FileName = shownName, Caption = $"[添付音声] {shownName}" });
                                r.Text.AppendLine("(音声そのものも添付しています)");
                                native = true;
                            }
                            catch (OperationCanceledException) { throw; }
                            catch { }
                        }
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { r.Text.AppendLine("(音を調べられませんでした: " + ex.Message + ")"); }
            }

            // 5) ChatGPT: 文字起こし
            if (provider == "openai" && transcribe && key.Length > 0)
            {
                status($"「{shownName}」を文字起こししています…");
                var m4a = Path.Combine(Path.GetTempPath(), "YmmExplorer_ai_" + Guid.NewGuid().ToString("N") + ".m4a");
                try
                {
                    if (await TranscodeAsync(file, m4a, MediaEncodingProfile.CreateM4a(AudioEncodingQuality.Low), ct))
                    {
                        if (new FileInfo(m4a).Length > 25_000_000) r.Text.AppendLine("(長すぎるため文字起こしは省きました。25MB まで)");
                        else r.Text.Append(await TranscribeAsync(key, m4a, ct));
                    }
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { r.Text.AppendLine("(文字起こしできませんでした: " + ex.Message + ")"); }
                finally { try { File.Delete(m4a); } catch { } }
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) { r.Text.AppendLine("(このファイルを開けませんでした: " + ex.Message + ")"); }
        finally { if (wav != null) try { File.Delete(wav); } catch { } }
        return r;
    }

    static async Task<bool> TranscodeAsync(StorageFile src, string dstPath, MediaEncodingProfile profile, CancellationToken ct)
    {
        File.WriteAllBytes(dstPath, Array.Empty<byte>());
        var dst = await StorageFile.GetFileFromPathAsync(dstPath).AsTask(ct);
        if (profile.Video != null) profile.Video = null;
        var tr = new MediaTranscoder();
        var prepared = await tr.PrepareFileTranscodeAsync(src, dst, profile).AsTask(ct);
        if (!prepared.CanTranscode) return false;
        await prepared.TranscodeAsync().AsTask(ct);
        return new FileInfo(dstPath).Length > 44;
    }

    // ---------------------------------------------------------------- 音の大きさ
    /// <summary>WAV を読んで、区間ごとの平均(RMS)と最大(ピーク)の dB、無音、音割れをまとめる</summary>
    static string? Loudness(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var br = new BinaryReader(fs);
        if (new string(br.ReadChars(4)) != "RIFF") return null;
        br.ReadInt32();
        if (new string(br.ReadChars(4)) != "WAVE") return null;
        int format = 0, channels = 1, bits = 16, rate = 44100;
        long dataPos = -1, dataLen = 0;
        while (fs.Position + 8 <= fs.Length)
        {
            var id = new string(br.ReadChars(4));
            long len = br.ReadUInt32();
            long next = fs.Position + len + (len & 1);
            if (id == "fmt ")
            {
                format = br.ReadUInt16();
                channels = Math.Max(1, (int)br.ReadUInt16());
                rate = br.ReadInt32(); br.ReadInt32(); br.ReadUInt16();
                bits = br.ReadUInt16();
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
        int bytes = bits / 8;
        if (dataPos < 0 || bytes < 1 || bytes > 4 || (format != 1 && format != 3) || rate <= 0) return null;
        long total = dataLen / (bytes * channels);
        if (total <= 0) return null;
        double seconds = total / (double)rate;
        // 区間の長さ: 2分までは1秒ごと、それより長い時は最大120区間
        double bucketSec = Math.Max(1, Math.Ceiling(seconds / 120));
        long bucketFrames = (long)(bucketSec * rate);
        int buckets = (int)Math.Ceiling(total / (double)bucketFrames);
        var sumSq = new double[buckets];
        var peak = new double[buckets];
        var count = new long[buckets];
        long clipped = 0;

        fs.Position = dataPos;
        const int chunk = 16384;
        var buf = new byte[chunk * bytes * channels];
        long done = 0;
        while (done < total)
        {
            int want = (int)Math.Min(chunk, total - done);
            int got = fs.Read(buf, 0, want * bytes * channels) / (bytes * channels);
            if (got <= 0) break;
            for (int i = 0; i < got; i++)
            {
                int b = (int)Math.Min(buckets - 1, (done + i) / bucketFrames);
                for (int c = 0; c < channels; c++)
                {
                    int o = (i * channels + c) * bytes;
                    double v = bytes switch
                    {
                        1 => (buf[o] - 128) / 128.0,
                        2 => BitConverter.ToInt16(buf, o) / 32768.0,
                        3 => ((buf[o] << 8 | buf[o + 1] << 16 | buf[o + 2] << 24) >> 8) / 8388608.0,
                        _ => format == 3 ? BitConverter.ToSingle(buf, o) : BitConverter.ToInt32(buf, o) / 2147483648.0,
                    };
                    double a = Math.Abs(v);
                    if (a > peak[b]) peak[b] = a;
                    if (a >= 0.999) clipped++;
                    sumSq[b] += v * v;
                    count[b]++;
                }
            }
            done += got;
        }

        static double Db(double x) => x <= 1e-6 ? -120 : 20 * Math.Log10(x);
        var rms = Enumerable.Range(0, buckets).Select(i => count[i] > 0 ? Db(Math.Sqrt(sumSq[i] / count[i])) : -120).ToArray();
        var pk = peak.Select(Db).ToArray();
        double allRms = Db(Math.Sqrt(sumSq.Sum() / Math.Max(1, count.Sum())));
        var loud = rms.Where(x => x > -50).ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"音: 全体の最大 {pk.Max():0.0} dBFS / 全体の平均(RMS) {allRms:0.0} dBFS" + (loud.Count > 0 ? $" / 鳴っている所の平均 {loud.Average():0.0} dBFS" : ""));
        if (clipped > 0) sb.AppendLine($"音割れ(0 dBFS に張り付いたサンプル): {clipped} 個");
        // 無音(-50 dBFS 未満)が続く所
        var silent = new List<string>();
        for (int i = 0; i < buckets;)
        {
            if (rms[i] >= -50) { i++; continue; }
            int j = i;
            while (j < buckets && rms[j] < -50) j++;
            if ((j - i) * bucketSec >= 1) silent.Add($"{Clock(i * bucketSec)}〜{Clock(Math.Min(seconds, j * bucketSec))}");
            i = j;
        }
        if (silent.Count > 0) sb.AppendLine("ほぼ無音の所: " + string.Join("、", silent.Take(40)) + (silent.Count > 40 ? " ほか" : ""));
        sb.AppendLine($"音の大きさの移り変わり({bucketSec:0} 秒ごと。時間 平均/最大 dBFS):");
        var line = new StringBuilder();
        for (int i = 0; i < buckets; i++)
        {
            line.Append($"{Clock(i * bucketSec)} {rms[i]:0}/{pk[i]:0}  ");
            if ((i + 1) % 8 == 0) { sb.AppendLine(line.ToString().TrimEnd()); line.Clear(); }
        }
        if (line.Length > 0) sb.AppendLine(line.ToString().TrimEnd());
        return sb.ToString();
    }

    // ---------------------------------------------------------------- Gemini のアップロード
    /// <summary>Gemini の File API にアップロードして、使えるようになるまで待つ。返すのはファイルの URI</summary>
    public static async Task<string> GeminiUploadAsync(string key, string path, string mime, string displayName, Action<string> status, CancellationToken ct)
    {
        long len = new FileInfo(path).Length;
        var start = new HttpRequestMessage(HttpMethod.Post, "https://generativelanguage.googleapis.com/upload/v1beta/files")
        {
            Content = new StringContent(new JsonObject { ["file"] = new JsonObject { ["display_name"] = displayName } }.ToJsonString(), Encoding.UTF8, "application/json"),
        };
        start.Headers.Add("x-goog-api-key", key);
        start.Headers.Add("X-Goog-Upload-Protocol", "resumable");
        start.Headers.Add("X-Goog-Upload-Command", "start");
        start.Headers.Add("X-Goog-Upload-Header-Content-Length", len.ToString());
        start.Headers.Add("X-Goog-Upload-Header-Content-Type", mime);
        using var r1 = await upload.SendAsync(start, ct);
        if (!r1.IsSuccessStatusCode) throw new InvalidOperationException($"アップロードを始められませんでした({(int)r1.StatusCode}): {await r1.Content.ReadAsStringAsync(ct)}");
        var url = r1.Headers.TryGetValues("X-Goog-Upload-URL", out var v) ? v.FirstOrDefault() : null;
        if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("アップロード先が返ってきませんでした");

        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var up = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StreamContent(fs, 1 << 20) };
        up.Content.Headers.ContentLength = len;
        up.Headers.Add("X-Goog-Upload-Offset", "0");
        up.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
        using var r2 = await upload.SendAsync(up, ct);
        var body = await r2.Content.ReadAsStringAsync(ct);
        if (!r2.IsSuccessStatusCode) throw new InvalidOperationException($"アップロードできませんでした({(int)r2.StatusCode}): {body}");
        var f = JsonNode.Parse(body)?["file"];
        var uri = f?["uri"]?.GetValue<string>() ?? throw new InvalidOperationException("アップロードしたファイルの URI が分かりませんでした");
        var name = f?["name"]?.GetValue<string>();
        var state = f?["state"]?.GetValue<string>() ?? "ACTIVE";

        // 動画は、Gemini 側の準備(PROCESSING)が終わるまで待つ
        var began = DateTime.Now;
        while (state == "PROCESSING" && name != null)
        {
            if ((DateTime.Now - began).TotalMinutes > 10) throw new InvalidOperationException("Gemini 側の準備が終わりませんでした(10分)");
            status($"「{displayName}」を Gemini が読み込んでいます…({(int)(DateTime.Now - began).TotalSeconds} 秒)");
            await Task.Delay(3000, ct);
            var get = new HttpRequestMessage(HttpMethod.Get, "https://generativelanguage.googleapis.com/v1beta/" + name);
            get.Headers.Add("x-goog-api-key", key);
            using var r3 = await upload.SendAsync(get, ct);
            if (!r3.IsSuccessStatusCode) break;
            state = JsonNode.Parse(await r3.Content.ReadAsStringAsync(ct))?["state"]?.GetValue<string>() ?? "ACTIVE";
        }
        if (state == "FAILED") throw new InvalidOperationException("Gemini がこのファイルを読み込めませんでした");
        return uri;
    }

    // ---------------------------------------------------------------- OpenAI の文字起こし
    static async Task<string> TranscribeAsync(string key, string path, CancellationToken ct)
    {
        await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var form = new MultipartFormDataContent();
        var file = new StreamContent(fs);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mp4");
        form.Add(file, "file", "audio.m4a");
        form.Add(new StringContent("whisper-1"), "model");
        form.Add(new StringContent("verbose_json"), "response_format");
        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions") { Content = form };
        req.Headers.Add("Authorization", "Bearer " + key);
        using var res = await upload.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode) throw new InvalidOperationException($"({(int)res.StatusCode}) {body}");
        var j = JsonNode.Parse(body);
        var sb = new StringBuilder("文字起こし(時間 内容):\n");
        if (j?["segments"] is JsonArray segs && segs.Count > 0)
            foreach (var s in segs.Take(800))
            {
                double st = s?["start"]?.GetValue<double>() ?? 0;
                sb.AppendLine($"{Clock(st)} {s?["text"]?.GetValue<string>()?.Trim()}");
            }
        else sb.AppendLine(j?["text"]?.GetValue<string>() ?? "(話し声はありませんでした)");
        return sb.ToString();
    }

    // ---------------------------------------------------------------- コマをつないで動画にする
    /// <summary>JPEG のコマ(それぞれの表示時間つき)を、1本の MP4 にする(音声なし)</summary>
    public static async Task<bool> FramesToMp4Async(IReadOnlyList<(string path, double seconds)> frames, string outPath, int width, int height,
                                                    Action<string> status, CancellationToken ct)
    {
        var comp = new MediaComposition();
        for (int i = 0; i < frames.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            if (i % 50 == 0) status($"プレビューを動画にしています…({i}/{frames.Count})");
            var f = await StorageFile.GetFileFromPathAsync(frames[i].path).AsTask(ct);
            comp.Clips.Add(await MediaClip.CreateFromImageFileAsync(f, TimeSpan.FromSeconds(Math.Max(0.01, frames[i].seconds))).AsTask(ct));
        }
        File.WriteAllBytes(outPath, Array.Empty<byte>());
        var dst = await StorageFile.GetFileFromPathAsync(outPath).AsTask(ct);
        var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);
        profile.Video.Width = (uint)(width & ~1);
        profile.Video.Height = (uint)(height & ~1);
        profile.Audio = null;
        status("プレビューを動画にしています…(書き出し中)");
        var op = comp.RenderToFileAsync(dst, MediaTrimmingPreference.Fast, profile);
        var result = await op.AsTask(ct);
        return result == TranscodeFailureReason.None && new FileInfo(outPath).Length > 1000;
    }
}
