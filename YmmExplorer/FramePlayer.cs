using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YmmExplorer;

/// <summary>
/// 小さな動画プレーヤー(再生・一時停止・シークバー)と、いま表示しているコマの取り出し。
/// プリセットのプレビュー動画と、テンプレートの動画ファイルで共通に使います。
/// </summary>
internal sealed class FramePlayer
{
    const string PlayGlyph = "\uE768", PauseGlyph = "\uE769";

    readonly MediaElement player;
    readonly Slider seek;
    readonly Button playButton;
    readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    bool playing, updatingSeek;
    string? currentPath;
    /// <summary>読み込み中(音を消して再生し、開けたら止める)。その間に再生ボタンが押されたら、そのまま再生を続ける</summary>
    bool priming;
    bool wantPlay;

    // 音声ファイルは、画面の部品(MediaElement)ではなく MediaPlayer で鳴らす。
    // MediaElement は、パネルの表示が切り替わる途中だと開き切らず、再生できないことがあるため
    readonly MediaPlayer audio = new() { Volume = 0.8 };
    bool audioMode;
    static readonly HashSet<string> AudioExt = new(StringComparer.OrdinalIgnoreCase)
        { ".wav", ".mp3", ".m4a", ".aac", ".flac", ".ogg", ".opus", ".wma", ".aif", ".aiff" };

    bool HasMedia => audioMode ? audio.Source != null : player.Source != null;
    Duration Natural => audioMode ? audio.NaturalDuration : player.NaturalDuration;
    TimeSpan Position
    {
        get => audioMode ? audio.Position : player.Position;
        set { if (audioMode) audio.Position = value; else player.Position = value; }
    }
    void MediaPlay() { if (audioMode) audio.Play(); else player.Play(); }
    void MediaPause() { if (audioMode) audio.Pause(); else player.Pause(); }

    /// <summary>読み込んだら、すぐ再生を始める(大きなプレビュー用)</summary>
    public bool AutoPlay { get; set; }

    /// <summary>「0:12 / 3:40」のような時間を出す場所(無くてもよい)</summary>
    public TextBlock? TimeLabel { get; set; }

    static string Fmt(TimeSpan t) => t.TotalHours >= 1 ? $"{(int)t.TotalHours}:{t.Minutes:00}:{t.Seconds:00}" : $"{t.Minutes}:{t.Seconds:00}";

    void UpdateTime()
    {
        if (TimeLabel == null) return;
        var total = Natural.HasTimeSpan ? Natural.TimeSpan : TimeSpan.Zero;
        TimeLabel.Text = !HasMedia ? "" : $"{Fmt(TimeSpan.FromSeconds(seek.Value))} / {Fmt(total)}";
    }

    public void TogglePlay() => Toggle();

    public FramePlayer(MediaElement player, Slider seek, Button playButton)
    {
        this.player = player;
        this.seek = seek;
        this.playButton = playButton;

        timer.Tick += (_, _) =>
        {
            if (!playing || !Natural.HasTimeSpan) return;
            updatingSeek = true;
            seek.Value = Position.TotalSeconds;
            updatingSeek = false;
            UpdateTime();
        };
        player.MediaOpened += (_, _) =>
        {
            if (player.NaturalDuration.HasTimeSpan)
                seek.Maximum = Math.Max(0.1, player.NaturalDuration.TimeSpan.TotalSeconds);
            if (priming)
            {
                // 開けた: 最初のコマ(動画)を表示した状態で止める。読み込み中に再生ボタンが押されていたら、最初から再生する
                priming = false;
                player.IsMuted = false;
                player.Position = TimeSpan.Zero;
                if (!wantPlay) player.Pause();
                else { playing = true; timer.Start(); playButton.Content = PauseGlyph; }
                wantPlay = false;
            }
            playButton.ToolTip = null;
            UpdateTime();
        };
        player.MediaFailed += (_, e) =>
        {
            priming = false;
            wantPlay = false;
            playing = false;
            timer.Stop();
            player.IsMuted = false;
            playButton.Content = PlayGlyph;
            playButton.ToolTip = "このファイルは再生できませんでした: " + (e.ErrorException?.Message ?? "");
            if (TimeLabel != null) TimeLabel.Text = "再生できませんでした";
        };
        player.MediaEnded += (_, _) => OnEnded();
        audio.MediaEnded += (_, _) => OnEnded();
        audio.MediaOpened += (_, _) =>
        {
            if (audio.NaturalDuration.HasTimeSpan) seek.Maximum = Math.Max(0.1, audio.NaturalDuration.TimeSpan.TotalSeconds);
            playButton.ToolTip = null;
            if (wantPlay) { wantPlay = false; StartPlaying(); }
            UpdateTime();
        };
        audio.MediaFailed += (_, e) =>
        {
            wantPlay = false;
            playing = false;
            timer.Stop();
            playButton.Content = PlayGlyph;
            playButton.ToolTip = "このファイルは再生できませんでした: " + (e.ErrorException?.Message ?? "");
            if (TimeLabel != null) TimeLabel.Text = "再生できませんでした";
        };
        // スライダーを動かすと、その位置のコマを表示(一時停止中でも表示される)
        seek.ValueChanged += (_, e) =>
        {
            if (updatingSeek || !HasMedia) return;
            Position = TimeSpan.FromSeconds(e.NewValue);
            UpdateTime();
        };
        playButton.Click += (_, _) => Toggle();
    }

    void OnEnded()
    {
        MediaPause();
        Position = TimeSpan.Zero;
        playing = false;
        timer.Stop();
        playButton.Content = PlayGlyph;
        seek.Value = 0;
    }

    void StartPlaying()
    {
        if (!audioMode) player.IsMuted = false;
        MediaPlay();
        playing = true;
        timer.Start();
        playButton.Content = PauseGlyph;
    }

    /// <summary>動画を読み込んで、最初のコマを表示した状態にする(nullで解放)</summary>
    public void Load(string? path)
    {
        // 同じファイルを読み込み直さない(一覧の更新などで同じ素材が選び直された時に、再生が止まらないように)
        if (path != null && path == currentPath && (HasMedia || priming)) return;
        Release();
        if (path == null) return;
        currentPath = path;
        // 24bit などの WAV は、そのままでは再生できないので、再生できる形の一時ファイルを用意してから読み込む
        _ = Task.Run(() => AudioCompat.PlayablePath(path)).ContinueWith(t =>
        {
            var playable = t.Status == TaskStatus.RanToCompletion ? t.Result : path;
            // 画面の表示切り替えが終わってから読み込む
            player.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => Start(path, playable)));
        }, TaskScheduler.Default);
    }

    void Start(string path, string playable)
    {
        {
            if (currentPath != path) return;
            seek.Value = 0;
            // 音声ファイル(大きなプレビュー以外): MediaPlayer で開く。開けたら再生ボタンで鳴らせる
            if (!AutoPlay && AudioExt.Contains(System.IO.Path.GetExtension(path)))
            {
                audioMode = true;
                wantPlay = false;
                audio.Open(new Uri(playable));
                return;
            }
            audioMode = false;
            player.Source = new Uri(playable);
            if (AutoPlay)
            {
                priming = false;
                player.IsMuted = false;
                player.Play();
                playing = true;
                timer.Start();
                playButton.Content = PauseGlyph;
            }
            else
            {
                // 再生してすぐ止めると、音声ファイルでは開き切らないことがある。音を消して再生し、開けた時点で止める
                priming = true;
                wantPlay = false;
                player.IsMuted = true;
                player.Play();
            }
        }
    }

    /// <summary>再生を止めて、動画ファイルを解放する(上書き・削除の前にも使う)</summary>
    public void Release()
    {
        timer.Stop();
        playing = false;
        priming = false;
        wantPlay = false;
        currentPath = null;
        try { player.Stop(); } catch { }
        player.IsMuted = false;
        player.Source = null;
        try { audio.Stop(); audio.Close(); } catch { }
        audioMode = false;
        playButton.Content = PlayGlyph;
        if (TimeLabel != null) TimeLabel.Text = "";
    }

    public void Pause()
    {
        if (!playing) return;
        MediaPause();
        playing = false;
        timer.Stop();
        playButton.Content = PlayGlyph;
    }

    void Toggle()
    {
        if (!HasMedia) return;
        if (priming) { wantPlay = !wantPlay; playButton.Content = wantPlay ? PauseGlyph : PlayGlyph; return; }   // まだ開いている途中
        if (playing) { Pause(); return; }
        StartPlaying();
    }

    /// <summary>いま表示しているコマを画像にする。取れなかった時は理由を返す</summary>
    public (BitmapSource? Frame, string? Error) CaptureFrame()
    {
        int vw = player.NaturalVideoWidth, vh = player.NaturalVideoHeight;
        double w = player.ActualWidth, h = player.ActualHeight;
        if (vw <= 0 || vh <= 0 || w < 1 || h < 1) return (null, "動画をまだ読み込めていません");

        // 見えている画面を画像にして、動画の映っている部分(黒帯を除く)だけを切り出す
        var rtb = new RenderTargetBitmap((int)Math.Ceiling(w), (int)Math.Ceiling(h), 96, 96, PixelFormats.Pbgra32);
        rtb.Render(player);
        var scale = Math.Min(w / vw, h / vh);
        double dw = vw * scale, dh = vh * scale;
        int x = (int)Math.Max(0, Math.Floor((w - dw) / 2)), y = (int)Math.Max(0, Math.Floor((h - dh) / 2));
        int cw = (int)Math.Min(rtb.PixelWidth - x, Math.Ceiling(dw)), ch = (int)Math.Min(rtb.PixelHeight - y, Math.Ceiling(dh));
        BitmapSource frame = cw > 0 && ch > 0 ? new CroppedBitmap(rtb, new Int32Rect(x, y, cw, ch)) : rtb;

        // 何も描かれていない(完全に透明)場合は、取得に失敗したとみなす
        var stride = frame.PixelWidth * 4;
        var buf = new byte[stride * frame.PixelHeight];
        frame.CopyPixels(buf, stride, 0);
        for (int i = 3; i < buf.Length; i += 4)
            if (buf[i] != 0) return (frame, null);
        return (null, "コマを取得できませんでした。動画を再生・一時停止してからもう一度お試しください");
    }
}
