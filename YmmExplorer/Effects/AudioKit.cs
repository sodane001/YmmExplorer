using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Audio;
using YukkuriMovieMaker.Player.Audio.Effects;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

/// <summary>
/// 同梱の演出音声エフェクトの基底。CreateKernel で、1本の音声ごとの処理(状態を持つ)を作る
/// </summary>
public abstract class FxAudioEffectBase : AudioEffectBase
{
    public override IEnumerable<string> CreateExoAudioFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IAudioEffectProcessor CreateAudioEffect(TimeSpan duration)
        => this is IRateEffect ? new FxRateProcessor(this, duration) : new FxAudioProcessor(this, duration);
    internal abstract AudioKernel CreateKernel();
}

/// <summary>音声1本ぶんの処理(フィルターなどの状態を持つ)</summary>
internal abstract class AudioKernel
{
    /// <summary>この処理が遅れる量(サンプル数)。先読みして打ち消す</summary>
    public virtual int Latency => 0;
    /// <summary>位置が飛んだ時・始まる時に呼ばれる</summary>
    public abstract void Reset(int hz);
    /// <summary>ブロックの始まりで、そのブロックで使う設定値を読む(frame はアイテムの先頭からのサンプル数)</summary>
    public abstract void Params(long frame, long total, int hz);
    /// <summary>1サンプル(左右)を処理する</summary>
    public abstract void Process(ref float l, ref float r);
}

internal sealed class FxAudioProcessor : AudioEffectProcessorBase
{
    readonly FxAudioEffectBase item;
    readonly TimeSpan duration;
    readonly AudioKernel kernel;
    int lastHz = -1;
    bool primed;
    float[] prime = [];

    public override int Hz => Input?.Hz ?? 0;
    public override long Duration => (long)(duration.TotalSeconds * Hz) * 2;

    public FxAudioProcessor(FxAudioEffectBase item, TimeSpan duration)
    {
        this.item = item;
        this.duration = duration;
        kernel = item.CreateKernel();
    }

    protected override void seek(long position)
    {
        Input?.Seek(position);
        kernel.Reset(Hz);
        lastHz = Hz;
        primed = false;
    }

    protected override int read(float[] destBuffer, int offset, int count)
    {
        int hz = Hz;
        if (hz <= 0) { Input?.Read(destBuffer, offset, count); return count; }
        if (hz != lastHz) { kernel.Reset(hz); lastHz = hz; primed = false; }
        // 遅れのある処理は、その分だけ先に読んでおく(出てきた音は捨てる)
        if (!primed)
        {
            primed = true;
            int lat = kernel.Latency;
            if (lat > 0 && Input != null)
            {
                if (prime.Length < lat * 2) prime = new float[lat * 2];
                Input.Read(prime, 0, lat * 2);
                kernel.Params(Position / 2, Duration / 2, hz);
                for (int i = 0; i < lat * 2; i += 2) { float l = prime[i], r = prime[i + 1]; kernel.Process(ref l, ref r); }
            }
        }
        Input?.Read(destBuffer, offset, count);
        long total = Duration / 2;
        const int block = 128;
        for (int i = 0; i < count; i += 2)
        {
            if ((i / 2) % block == 0) kernel.Params((Position + i) / 2, total, hz);
            float l = destBuffer[offset + i], r = destBuffer[offset + i + 1];
            kernel.Process(ref l, ref r);
            destBuffer[offset + i] = float.IsFinite(l) ? l : 0f;
            destBuffer[offset + i + 1] = float.IsFinite(r) ? r : 0f;
        }
        return count;
    }
}

/// <summary>再生の速さを変える演出(逆再生・テープストップなど): 出力の時刻から、読む元の時刻を返す</summary>
internal interface IRateEffect
{
    /// <summary>出力の t 秒目に、元の音の何秒目を鳴らすか(dur はアイテムの長さ)</summary>
    double SourceTime(double t, double dur, long frame, long total, int hz);
}

internal sealed class FxRateProcessor : AudioEffectProcessorBase
{
    readonly FxAudioEffectBase item;
    readonly IRateEffect rate;
    readonly TimeSpan duration;
    float[] buf = [];

    public override int Hz => Input?.Hz ?? 0;
    public override long Duration => (long)(duration.TotalSeconds * Hz) * 2;

    public FxRateProcessor(FxAudioEffectBase item, TimeSpan duration)
    {
        this.item = item;
        rate = (IRateEffect)item;
        this.duration = duration;
    }

    protected override void seek(long position) => Input?.Seek(position);

    protected override int read(float[] destBuffer, int offset, int count)
    {
        int hz = Hz;
        if (hz <= 0 || Input == null) { Input?.Read(destBuffer, offset, count); return count; }
        long total = Duration / 2;
        int frames = count / 2;
        long start = Position / 2;
        double dur = duration.TotalSeconds;
        // このブロックで読む元の範囲
        var src = new double[frames];
        double lo = double.MaxValue, hi = double.MinValue;
        for (int i = 0; i < frames; i++)
        {
            long fr = start + i;
            double s = rate.SourceTime((double)fr / hz, dur, fr, total, hz) * hz;
            src[i] = s;
            if (s < lo) lo = s;
            if (s > hi) hi = s;
        }
        long a = Math.Max(0, (long)Math.Floor(lo)), b = Math.Max(a + 1, (long)Math.Ceiling(hi) + 2);
        b = Math.Min(b, Math.Max(a + 2, total + 2));
        int need = (int)Math.Min(int.MaxValue / 4, (b - a) * 2);
        if (buf.Length < need) buf = new float[need];
        Array.Clear(buf, 0, need);
        Input.Seek(a * 2);
        Input.Read(buf, 0, need);
        int n = need / 2;
        for (int i = 0; i < frames; i++)
        {
            double s = src[i] - a;
            float l = 0, r = 0;
            if (s >= 0 && src[i] < total)
            {
                int i0 = (int)Math.Floor(s);
                float fr = (float)(s - i0);
                int i1 = Math.Min(n - 1, i0 + 1);
                if (i0 < n)
                {
                    l = buf[i0 * 2] * (1 - fr) + buf[i1 * 2] * fr;
                    r = buf[i0 * 2 + 1] * (1 - fr) + buf[i1 * 2 + 1] * fr;
                }
            }
            destBuffer[offset + i * 2] = l;
            destBuffer[offset + i * 2 + 1] = r;
        }
        return count;
    }
}

// =====================================================================================================
//  部品
// =====================================================================================================

/// <summary>2次のフィルター(低い音だけ・高い音だけ・帯域・山・棚)</summary>
internal sealed class Biquad
{
    double b0 = 1, b1, b2, a1, a2, z1, z2;
    public void Reset() { z1 = z2 = 0; }

    public float Run(float x)
    {
        double y = b0 * x + z1;
        z1 = b1 * x - a1 * y + z2;
        z2 = b2 * x - a2 * y;
        return (float)y;
    }

    void Set(double nb0, double nb1, double nb2, double na0, double na1, double na2)
    {
        b0 = nb0 / na0; b1 = nb1 / na0; b2 = nb2 / na0; a1 = na1 / na0; a2 = na2 / na0;
    }

    static (double cos, double alpha) W(double f, int hz, double q)
    {
        double w0 = 2 * Math.PI * Math.Clamp(f, 10, hz * 0.45) / hz;
        return (Math.Cos(w0), Math.Sin(w0) / (2 * Math.Max(0.05, q)));
    }

    public Biquad LowPass(double f, int hz, double q = 0.7071)
    {
        var (c, a) = W(f, hz, q);
        Set((1 - c) / 2, 1 - c, (1 - c) / 2, 1 + a, -2 * c, 1 - a);
        return this;
    }

    public Biquad HighPass(double f, int hz, double q = 0.7071)
    {
        var (c, a) = W(f, hz, q);
        Set((1 + c) / 2, -(1 + c), (1 + c) / 2, 1 + a, -2 * c, 1 - a);
        return this;
    }

    public Biquad BandPass(double f, int hz, double q = 1)
    {
        var (c, a) = W(f, hz, q);
        Set(a, 0, -a, 1 + a, -2 * c, 1 - a);
        return this;
    }

    public Biquad Peak(double f, int hz, double q, double gainDb)
    {
        var (c, a) = W(f, hz, q);
        double A = Math.Pow(10, gainDb / 40);
        Set(1 + a * A, -2 * c, 1 - a * A, 1 + a / A, -2 * c, 1 - a / A);
        return this;
    }

    public Biquad HighShelf(double f, int hz, double gainDb)
    {
        double A = Math.Pow(10, gainDb / 40);
        double w0 = 2 * Math.PI * Math.Clamp(f, 10, hz * 0.45) / hz, c = Math.Cos(w0), s = Math.Sin(w0);
        double alpha = s / 2 * Math.Sqrt(2);
        double sq = 2 * Math.Sqrt(A) * alpha;
        Set(A * ((A + 1) + (A - 1) * c + sq), -2 * A * ((A - 1) + (A + 1) * c), A * ((A + 1) + (A - 1) * c - sq),
            (A + 1) - (A - 1) * c + sq, 2 * ((A - 1) - (A + 1) * c), (A + 1) - (A - 1) * c - sq);
        return this;
    }
}

/// <summary>左右それぞれに同じフィルターをかける</summary>
internal sealed class StereoBiquad
{
    public readonly Biquad L = new(), R = new();
    public void Reset() { L.Reset(); R.Reset(); }
    public void LowPass(double f, int hz, double q = 0.7071) { L.LowPass(f, hz, q); R.LowPass(f, hz, q); }
    public void HighPass(double f, int hz, double q = 0.7071) { L.HighPass(f, hz, q); R.HighPass(f, hz, q); }
    public void BandPass(double f, int hz, double q = 1) { L.BandPass(f, hz, q); R.BandPass(f, hz, q); }
    public void Peak(double f, int hz, double q, double db) { L.Peak(f, hz, q, db); R.Peak(f, hz, q, db); }
    public void HighShelf(double f, int hz, double db) { L.HighShelf(f, hz, db); R.HighShelf(f, hz, db); }
    public void Run(ref float l, ref float r) { l = L.Run(l); r = R.Run(r); }
}

/// <summary>音の大きさを追いかける(立ち上がり・戻りの速さつき)</summary>
internal sealed class Envelope
{
    double env, att, rel;
    public float Value => (float)env;
    public void Reset() { env = 0; }
    public void Set(double attackMs, double releaseMs, int hz)
    {
        att = Math.Exp(-1.0 / Math.Max(1, attackMs * 0.001 * hz));
        rel = Math.Exp(-1.0 / Math.Max(1, releaseMs * 0.001 * hz));
    }
    public float Run(float x)
    {
        double a = Math.Abs(x);
        env = a > env ? att * env + (1 - att) * a : rel * env + (1 - rel) * a;
        return (float)env;
    }
}

/// <summary>遅らせる(エコー・コーラス・音程変更に使う)</summary>
internal sealed class DelayLine
{
    float[] buf = new float[1];
    int pos;
    public void Resize(int samples) { if (buf.Length != samples) buf = new float[Math.Max(1, samples)]; pos = 0; }
    public void Reset() { Array.Clear(buf); pos = 0; }
    public void Write(float x) { buf[pos] = x; pos = (pos + 1) % buf.Length; }
    /// <summary>d サンプル前の音(小数は補間)</summary>
    public float Read(double d)
    {
        d = Math.Clamp(d, 1, buf.Length - 2);
        double p = pos - d;
        while (p < 0) p += buf.Length;
        int i0 = (int)p; float fr = (float)(p - i0);
        if (i0 >= buf.Length) i0 -= buf.Length;   // 計算の誤差で、ちょうど長さになることがある
        int i1 = (i0 + 1) % buf.Length;
        return buf[i0] * (1 - fr) + buf[i1] * fr;
    }
}

/// <summary>残響(Freeverb 型: くし形8つ + 全域通過4つ)</summary>
internal sealed class Reverb
{
    static readonly int[] CombLens = { 1116, 1188, 1277, 1356, 1422, 1491, 1557, 1617 };
    static readonly int[] ApLens = { 556, 441, 341, 225 };
    readonly float[][] combL = new float[8][], combR = new float[8][], apL = new float[4][], apR = new float[4][];
    readonly int[] ciL = new int[8], ciR = new int[8], aiL = new int[4], aiR = new int[4];
    readonly float[] storeL = new float[8], storeR = new float[8];
    int hzSet = -1; float sizeSet = -1;
    float feedback = 0.84f, damp = 0.2f;
    readonly DelayLine preL = new(), preR = new();
    int preDelay;

    public void Configure(int hz, float roomSize, float damping, float preDelayMs, float scale = 1f)
    {
        feedback = 0.7f + 0.28f * Math.Clamp(roomSize, 0f, 1f);
        damp = Math.Clamp(damping, 0f, 1f) * 0.4f;
        if (hz != hzSet || Math.Abs(scale - sizeSet) > 0.001f)
        {
            hzSet = hz; sizeSet = scale;
            double k = hz / 44100.0 * Math.Clamp(scale, 0.2f, 3f);
            for (int i = 0; i < 8; i++) { combL[i] = new float[Math.Max(2, (int)(CombLens[i] * k))]; combR[i] = new float[Math.Max(2, (int)((CombLens[i] + 23) * k))]; ciL[i] = ciR[i] = 0; storeL[i] = storeR[i] = 0; }
            for (int i = 0; i < 4; i++) { apL[i] = new float[Math.Max(2, (int)(ApLens[i] * k))]; apR[i] = new float[Math.Max(2, (int)((ApLens[i] + 23) * k))]; aiL[i] = aiR[i] = 0; }
            preL.Resize(hz / 2 + 2); preR.Resize(hz / 2 + 2);
        }
        preDelay = (int)Math.Clamp(preDelayMs * 0.001 * hz, 1, hz / 2 - 2);
    }

    public void Reset()
    {
        for (int i = 0; i < 8; i++) { if (combL[i] != null) { Array.Clear(combL[i]); Array.Clear(combR[i]); } storeL[i] = storeR[i] = 0; }
        for (int i = 0; i < 4; i++) { if (apL[i] != null) { Array.Clear(apL[i]); Array.Clear(apR[i]); } }
        preL.Reset(); preR.Reset();
    }

    float Run(float x, float[][] comb, int[] ci, float[] store, float[][] ap, int[] ai)
    {
        float o = 0;
        for (int i = 0; i < 8; i++)
        {
            var c = comb[i]; int p = ci[i];
            float y = c[p];
            store[i] = y * (1 - damp) + store[i] * damp;
            c[p] = x + store[i] * feedback;
            ci[i] = (p + 1) % c.Length;
            o += y;
        }
        for (int i = 0; i < 4; i++)
        {
            var a = ap[i]; int p = ai[i];
            float b = a[p];
            a[p] = o + b * 0.5f;
            o = b - o;
            ai[i] = (p + 1) % a.Length;
        }
        return o;
    }

    /// <summary>入力 → 残響だけ(wet)を返す</summary>
    public void Process(float l, float r, out float wl, out float wr)
    {
        if (combL[0] == null) { wl = wr = 0; return; }
        preL.Write(l); preR.Write(r);
        float il = preL.Read(preDelay), ir = preR.Read(preDelay);
        float input = (il + ir) * 0.015f;
        wl = Run(input, combL, ciL, storeL, apL, aiL);
        wr = Run(input, combR, ciR, storeR, apR, aiR);
    }
}

/// <summary>音程を変える(速さは変えない)。遅らせた音を2つの読み取り位置でずらしながら、交互に重ねる</summary>
internal sealed class PitchShifter
{
    readonly DelayLine dl = new();
    double phase;
    int window = 2048;
    public void Configure(int hz, float windowMs = 60f)
    {
        int w = Math.Max(64, (int)(windowMs * 0.001 * hz));
        window = w;
        dl.Resize(w * 2 + 8);
    }
    public void Reset() { dl.Reset(); phase = 0; }
    /// <summary>ratio 2=1オクターブ上、0.5=1オクターブ下</summary>
    public float Run(float x, double ratio)
    {
        dl.Write(x);
        phase += (1 - ratio) / window;
        phase -= Math.Floor(phase);
        double p2 = phase + 0.5; p2 -= Math.Floor(p2);
        float a = dl.Read(2 + phase * window), b = dl.Read(2 + p2 * window);
        // 継ぎ目が聞こえないように、三角の窓で混ぜる
        float wa = (float)(1 - Math.Abs(phase * 2 - 1)), wb = (float)(1 - Math.Abs(p2 * 2 - 1));
        return a * wa + b * wb;
    }
}

/// <summary>白いノイズ(再現できる乱数)</summary>
internal struct WhiteNoise
{
    uint s;
    public WhiteNoise(uint seed) { s = seed == 0 ? 1u : seed; }
    public float Next()
    {
        s ^= s << 13; s ^= s >> 17; s ^= s << 5;
        return s / (float)uint.MaxValue * 2f - 1f;
    }
}

internal static class Db
{
    public static float ToGain(double db) => (float)Math.Pow(10, db / 20);
    public static float ToDb(double g) => (float)(20 * Math.Log10(Math.Max(1e-9, g)));
    /// <summary>やわらかく頭打ちにする(歪ませる時)</summary>
    public static float Soft(float x) => MathF.Tanh(x);
}

/// <summary>
/// ノイズ除去(短い時間ごとに周波数に分けて、ずっと鳴っている小さな音=ノイズを引く)。
/// 1024 サンプルずつ、1/4 ずつずらして処理する
/// </summary>
internal sealed class SpectralDenoiser
{
    public const int N = 1024, Hop = N / 4;
    readonly float[] hist = new float[N], acc = new float[N], ready = new float[Hop];
    readonly float[] re = new float[N], im = new float[N], win = new float[N];
    readonly float[] noise = new float[N / 2 + 1], gainSm = new float[N / 2 + 1];
    int hp, sinceHop, readyPos;
    long frames;
    public float Amount = 1f, Floor = 0.1f;

    public SpectralDenoiser()
    {
        for (int i = 0; i < N; i++) win[i] = 0.5f - 0.5f * MathF.Cos(2 * MathF.PI * i / N);
        Reset();
    }

    public void Reset()
    {
        Array.Clear(hist); Array.Clear(acc); Array.Clear(ready); Array.Fill(gainSm, 1f);
        for (int i = 0; i < noise.Length; i++) noise[i] = 1e9f;
        hp = 0; sinceHop = 0; readyPos = 0; frames = 0;
    }

    /// <summary>1サンプル入れて、ちょうど N サンプル遅れた音を返す</summary>
    public float Run(float x)
    {
        hist[hp] = x;
        hp = (hp + 1) % N;
        if (++sinceHop == Hop)
        {
            sinceHop = 0;
            Frame();
            // 重ね合わせが終わった先頭の Hop 個を、次の Hop サンプルで出す
            Array.Copy(acc, 0, ready, 0, Hop);
            Array.Copy(acc, Hop, acc, 0, N - Hop);
            Array.Clear(acc, N - Hop, Hop);
            readyPos = 0;
        }
        return readyPos < Hop ? ready[readyPos++] : 0f;
    }

    void Frame()
    {
        frames++;
        // hist は輪になっているので、古い順に並べる(hp が一番古い)
        for (int i = 0; i < N; i++) { re[i] = hist[(hp + i) % N] * win[i]; im[i] = 0; }
        Fft.Run(re, im, false);
        int half = N / 2;
        for (int k = 0; k <= half; k++)
        {
            float mag = MathF.Sqrt(re[k] * re[k] + im[k] * im[k]);
            // ノイズの大きさ: 小さい値に素早く合わせ、大きい値にはゆっくり上がる(最小値の追跡)
            if (mag < noise[k]) noise[k] = noise[k] > 1e8f ? mag : noise[k] * 0.9f + mag * 0.1f;
            else noise[k] *= 1.0015f;
            float g = mag > 1e-9f ? 1f - Amount * 2.2f * noise[k] / mag : 0f;
            g = MathF.Max(Floor, g);
            gainSm[k] = gainSm[k] * 0.6f + g * 0.4f;
            float gg = frames < 6 ? 1f : gainSm[k];
            re[k] *= gg; im[k] *= gg;
            if (k > 0 && k < half) { re[N - k] = re[k]; im[N - k] = -im[k]; }
        }
        Fft.Run(re, im, true);
        // 窓(ハン)を 1/4 ずつ重ねて2回かけると 1.5 倍になるので戻す
        for (int i = 0; i < N; i++) acc[i] += re[i] * win[i] / 1.5f;
    }
}

internal static class Fft
{
    /// <summary>その場で高速フーリエ変換(長さは2のべき乗)。inverse で逆変換(1/N も掛ける)</summary>
    public static void Run(float[] re, float[] im, bool inverse)
    {
        int n = re.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) { (re[i], re[j]) = (re[j], re[i]); (im[i], im[j]) = (im[j], im[i]); }
        }
        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = 2 * Math.PI / len * (inverse ? 1 : -1);
            float wr = (float)Math.Cos(ang), wi = (float)Math.Sin(ang);
            for (int i = 0; i < n; i += len)
            {
                float cr = 1, ci = 0;
                for (int j = 0; j < len / 2; j++)
                {
                    int a = i + j, b = i + j + len / 2;
                    float tr = re[b] * cr - im[b] * ci, ti = re[b] * ci + im[b] * cr;
                    re[b] = re[a] - tr; im[b] = im[a] - ti;
                    re[a] += tr; im[a] += ti;
                    float ncr = cr * wr - ci * wi; ci = cr * wi + ci * wr; cr = ncr;
                }
            }
        }
        if (inverse) for (int i = 0; i < n; i++) { re[i] /= n; im[i] /= n; }
    }
}
