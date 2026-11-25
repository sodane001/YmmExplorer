using System.IO;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace YmmExplorer;

/// <summary>プレビュー動画の保存場所(%AppData%\YmmExplorer\previews)</summary>
public static class PresetPreviewStore
{
    public static string Dir => System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "YmmExplorer", "previews");

    public static string PathOf(string fileName) => System.IO.Path.Combine(Dir, fileName);

    public static void DeleteQuietly(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;
        try { File.Delete(PathOf(fileName)); } catch { }
    }
}

/// <summary>
/// プリセットに含まれる画像・動画をつなげて、短いプレビュー動画(MP4・720p)にエンコードします。
/// Windows標準のエンコーダー(Windows.Media.Editing)を使うので、追加ソフトは不要です。
/// 画像は2秒、動画は先頭の3秒ずつを、最大8個まで使います。
/// </summary>
public static class PresetPreviewEncoder
{
    const int MaxClips = 8;
    static readonly TimeSpan ImageLength = TimeSpan.FromSeconds(2);
    static readonly TimeSpan VideoLength = TimeSpan.FromSeconds(3);

    public static async Task<(bool Ok, string Error)> EncodeAsync(
        IReadOnlyList<MediaRef> media, string fileName, IProgress<double>? progress)
    {
        try
        {
            var comp = new MediaComposition();
            foreach (var m in media.Take(MaxClips))
            {
                try
                {
                    var src = await StorageFile.GetFileFromPathAsync(m.Path);
                    MediaClip clip;
                    if (PresetThumb.IsImageFile(m.Path))
                        clip = await MediaClip.CreateFromImageFileAsync(src, ImageLength);
                    else
                    {
                        clip = await MediaClip.CreateFromFileAsync(src);
                        if (clip.OriginalDuration > VideoLength)
                            clip.TrimTimeFromEnd = clip.OriginalDuration - VideoLength;
                    }
                    comp.Clips.Add(clip);
                }
                catch { /* 読み込めない素材(未対応の形式など)は飛ばす */ }
            }
            if (comp.Clips.Count == 0) return (false, "プレビューに使える素材を読み込めませんでした");

            Directory.CreateDirectory(PresetPreviewStore.Dir);
            var folder = await StorageFolder.GetFolderFromPathAsync(PresetPreviewStore.Dir);
            var dest = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);

            var result = await comp.RenderToFileAsync(dest, MediaTrimmingPreference.Fast, profile).AsTask(progress);
            return result == TranscodeFailureReason.None
                ? (true, "")
                : (false, $"エンコードに失敗しました({result})");
        }
        catch (Exception ex)
        {
            return (false, "プレビューを作成できませんでした: " + ex.Message);
        }
    }
}
