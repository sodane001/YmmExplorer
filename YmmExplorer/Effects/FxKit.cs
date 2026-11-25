using System.Numerics;
using System.Reflection;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

/// <summary>1フレーム分の情報(演出エフェクト用)</summary>
public readonly struct FxFrame
{
    public long Frame { get; init; }
    public long Length { get; init; }
    public int Fps { get; init; }
    /// <summary>入力画像の範囲(アイテムの座標。まん中が 0,0)</summary>
    public RawRectF B { get; init; }
    /// <summary>アイテムの先頭からの秒</summary>
    public float T => (float)Frame / Math.Max(1, Fps);
    /// <summary>アイテムの長さ(秒)</summary>
    public float Dur => (float)Length / Math.Max(1, Fps);
    public float W => B.Right - B.Left;
    public float H => B.Bottom - B.Top;
    public Vector2 C => new((B.Left + B.Right) / 2f, (B.Top + B.Bottom) / 2f);
    /// <summary>範囲の中の位置(0〜1 の割合)から座標</summary>
    public Vector2 At(float px, float py) => new(B.Left + W * px, B.Top + H * py);
    public float V(Animation a) => (float)a.GetValue(Frame, Length, Fps);
    /// <summary>登場(先頭から inSec 秒で 0→1)と退場(最後の outSec 秒で 1→0)を合わせた進み具合</summary>
    public float InOut(float inSec, float outSec)
    {
        float a = inSec > 0.001f ? Math.Clamp(T / inSec, 0f, 1f) : 1f;
        float b = outSec > 0.001f ? Math.Clamp((Dur - T) / outSec, 0f, 1f) : 1f;
        return MathF.Min(a, b);
    }
}

/// <summary>
/// 同梱の演出エフェクト(映像)の基底。Build で入力画像から出力画像を作る。
/// 失敗した時は、そのまま(エフェクトなし)で表示する
/// </summary>
public abstract class FxVideoEffectBase : VideoEffectBase
{
    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices) => new FxProcessor(devices, this);
    internal abstract ID2D1Image Build(FxKit k, ID2D1Image input, in FxFrame f);
    /// <summary>最後に起きたエラー(調べる用)</summary>
    internal string? LastError;
}

internal sealed class FxProcessor : IVideoEffectProcessor
{
    readonly FxVideoEffectBase item;
    readonly FxKit kit;
    readonly AffineTransform2D pass;
    ID2D1Image? input;

    public ID2D1Image Output { get; }

    public FxProcessor(IGraphicsDevicesAndContext devices, FxVideoEffectBase item)
    {
        this.item = item;
        kit = new FxKit(devices.DeviceContext);
        // 出力は、いつも同じ「通り道」(何もしない変形)にしておき、毎フレーム入力を差し替える
        pass = new AffineTransform2D(devices.DeviceContext);
        Output = pass.Output;
    }

    public void SetInput(ID2D1Image? input)
    {
        this.input = input;
        pass.SetInput(0, input, true);
    }

    public void ClearInput()
    {
        input = null;
        pass.SetInput(0, null, true);
    }

    public DrawDescription Update(EffectDescription d)
    {
        if (input == null) return d.DrawDescription;
        try
        {
            var b = kit.Dc.GetImageLocalBounds(input);
            float w = b.Right - b.Left, h = b.Bottom - b.Top;
            if (!(w > 0 && h > 0) || !float.IsFinite(w) || !float.IsFinite(h) || w > 32768 || h > 32768)
            {
                pass.SetInput(0, input, true);
                kit.EndFrame();
                return d.DrawDescription;
            }
            kit.NoiseArea = b;
            var f = new FxFrame { Frame = d.ItemPosition.Frame, Length = d.ItemDuration.Frame, Fps = d.FPS, B = b };
            var img = item.Build(kit, input, f);
            pass.SetInput(0, img, true);
            kit.EndFrame();
            item.LastError = null;
        }
        catch (Exception ex)
        {
            item.LastError = ex.GetType().Name + ": " + ex.Message;
            try { pass.SetInput(0, input, true); kit.EndFrame(); } catch { }
        }
        return d.DrawDescription;
    }

    public void Dispose()
    {
        pass.SetInput(0, null, true);
        Output.Dispose();
        pass.Dispose();
        kit.Dispose();
    }
}

/// <summary>
/// 演出エフェクトの道具箱。D2D の組み込みエフェクトを名前で作って使い回し(1つの処理につき1つずつ)、
/// 毎フレーム作るもの(コマンドリスト)は、次のフレームで片付ける
/// </summary>
public sealed class FxKit : IDisposable
{
    public ID2D1DeviceContext Dc { get; }
    readonly Dictionary<string, (ID2D1Effect fx, ID2D1Image output)> effects = new();
    List<IDisposable> current = new(), previous = new();

    public FxKit(ID2D1DeviceContext dc) { Dc = dc; }

    internal void EndFrame()
    {
        // 前のフレームで作ったものを片付ける(いまのフレームのものは、まだ使われている)
        foreach (var x in previous) try { x.Dispose(); } catch { }
        previous.Clear();
        (previous, current) = (current, previous);
    }

    public void Dispose()
    {
        foreach (var (fx, output) in effects.Values)
        {
            try { for (int i = 0; i < 2; i++) fx.SetInput(i, null, true); } catch { }
            try { output.Dispose(); } catch { }
            try { fx.Dispose(); } catch { }
        }
        effects.Clear();
        EndFrame(); EndFrame();
    }

    // ------------------------------------------------------------------ 組み込みエフェクト(名前で作る。Vortice の版の違いに強くするため)
    static readonly Assembly VorticeD2D = typeof(GaussianBlur).Assembly;
    static readonly Dictionary<(Type, string), PropertyInfo?> props = new();

    (ID2D1Effect fx, ID2D1Image output) Get(string id, string typeName)
    {
        if (effects.TryGetValue(id, out var e)) return e;
        ID2D1Effect fx;
        if (typeName == nameof(GaussianBlur)) fx = new GaussianBlur(Dc);
        else if (typeName == nameof(AffineTransform2D)) fx = new AffineTransform2D(Dc);
        else if (typeName == nameof(Composite)) fx = new Composite(Dc);
        else
        {
            var t = VorticeD2D.GetType("Vortice.Direct2D1.Effects." + typeName)
                    ?? throw new InvalidOperationException($"{typeName} エフェクトが、この YMM4 では使えません");
            fx = (ID2D1Effect)(Activator.CreateInstance(t, Dc) ?? throw new InvalidOperationException(typeName));
        }
        e = (fx, fx.Output);
        effects[id] = e;
        return e;
    }

    /// <summary>プロパティを名前で設定する(列挙は名前の文字列でも、行列は float の配列でもよい)</summary>
    static void Set(object target, string name, object value)
    {
        var t = target.GetType();
        PropertyInfo? p;
        lock (props)
        {
            if (!props.TryGetValue((t, name), out p))
            {
                p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                props[(t, name)] = p;
            }
        }
        if (p == null || !p.CanWrite) return;
        var pt = p.PropertyType;
        object v = value;
        if (pt.IsEnum && value is string s) v = Enum.Parse(pt, s, true);
        else if (value is float[] arr && pt != typeof(float[]))
        {
            // 行列(Matrix5x4 など): M11, M12 … の順に入れる
            object m = Activator.CreateInstance(pt)!;
            var fields = pt.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => f.Name.Length == 3 && f.Name[0] == 'M').OrderBy(f => f.Name).ToArray();
            if (fields.Length >= arr.Length)
            {
                for (int i = 0; i < arr.Length; i++) fields[i].SetValue(m, arr[i]);
            }
            else
            {
                var ctor = pt.GetConstructor(Enumerable.Repeat(typeof(float), arr.Length).ToArray())
                           ?? throw new InvalidOperationException(pt.Name + " を作れません");
                m = ctor.Invoke(arr.Cast<object>().ToArray());
            }
            v = m;
        }
        else if (pt != value.GetType() && value is IConvertible && pt.IsPrimitive) v = Convert.ChangeType(value, pt);
        p.SetValue(target, v);
    }

    public ID2D1Image Blur(string id, ID2D1Image src, float sigma)
    {
        var (fx, o) = Get(id, nameof(GaussianBlur));
        var g = (GaussianBlur)fx;
        g.SetInput(0, src, true);
        g.StandardDeviation = MathF.Max(0f, sigma);
        return o;
    }

    public ID2D1Image Affine(string id, ID2D1Image src, Matrix3x2 m, bool nearest = false)
    {
        var (fx, o) = Get(id, nameof(AffineTransform2D));
        var a = (AffineTransform2D)fx;
        a.SetInput(0, src, true);
        a.TransformMatrix = m;
        Set(a, "InterpolationMode", nearest ? "NearestNeighbor" : "Linear");
        return o;
    }

    /// <summary>色の行列(5行4列。[r g b a 1] × 行列)</summary>
    public ID2D1Image Matrix(string id, ID2D1Image src, float[] m)
    {
        var (fx, o) = Get(id, "ColorMatrix");
        fx.SetInput(0, src, true);
        Set(fx, "Matrix", m);
        return o;
    }

    /// <summary>重ねる(dst の上に src)。mode: SourceOver / SourceAtop / DestinationIn / DestinationOut / Plus など</summary>
    public ID2D1Image Comp(string id, ID2D1Image dst, ID2D1Image src, CompositeMode mode = CompositeMode.SourceOver)
    {
        var (fx, o) = Get(id, nameof(Composite));
        var c = (Composite)fx;
        c.SetInput(0, dst, true);
        c.SetInput(1, src, true);
        Set(c, "Mode", mode);
        return o;
    }

    /// <summary>雲のようなノイズ(範囲は無限。描く時に切り抜く)</summary>
    public ID2D1Image Turb(string id, Vector2 offset, float freq, int octaves, int seed)
    {
        var (fx, o) = Get(id, "Turbulence");
        // ノイズは「Offset から Size の範囲」にしか出ないので、入力画像の範囲(少し広め)を覆うように作り、
        // offset の分は、できた模様を動かして表す
        var a = NoiseArea;
        const float margin = 64f;
        Set(fx, "Offset", new Vector2(a.Left - margin - offset.X, a.Top - margin - offset.Y));
        Set(fx, "Size", new Vector2(MathF.Ceiling(a.Right - a.Left + margin * 2f), MathF.Ceiling(a.Bottom - a.Top + margin * 2f)));
        Set(fx, "BaseFrequency", new Vector2(freq, freq));
        Set(fx, "NumOctaves", Math.Clamp(octaves, 1, 8));
        Set(fx, "Seed", seed);
        return Affine(id + "_move", o, Matrix3x2.CreateTranslation(offset));
    }

    /// <summary>ノイズを作る範囲(そのフレームの入力画像の範囲)</summary>
    public RawRectF NoiseArea { get; set; } = new(-1024, -1024, 1024, 1024);

    /// <summary>map の赤・緑で、src を ずらす(陽炎・水面など)</summary>
    public ID2D1Image Displace(string id, ID2D1Image src, ID2D1Image map, float scale)
    {
        var (fx, o) = Get(id, "DisplacementMap");
        fx.SetInput(0, src, true);
        fx.SetInput(1, map, true);
        Set(fx, "Scale", scale);
        Set(fx, "XChannelSelect", "R");
        Set(fx, "YChannelSelect", "G");
        return o;
    }

    /// <summary>一方向のぼかし(角度は度)</summary>
    public ID2D1Image DirBlur(string id, ID2D1Image src, float sigma, float angle)
    {
        var (fx, o) = Get(id, "DirectionalBlur");
        fx.SetInput(0, src, true);
        Set(fx, "StandardDeviation", MathF.Max(0f, sigma));
        Set(fx, "Angle", angle);
        return o;
    }

    /// <summary>名前で組み込みエフェクトを使う(Posterize など)。項目は名前と値で渡す</summary>
    public ID2D1Image Builtin(string id, string typeName, ID2D1Image src, params (string name, object value)[] values)
    {
        var (fx, o) = Get(id, typeName);
        fx.SetInput(0, src, true);
        foreach (var (name, value) in values) Set(fx, name, value);
        return o;
    }

    /// <summary>名前で作ったエフェクトの、2つめ以降の入力を入れる</summary>
    public void SetInput(string id, int index, ID2D1Image src)
    {
        if (effects.TryGetValue(id, out var e)) e.fx.SetInput(index, src, true);
    }

    // ------------------------------------------------------------------ 描く(コマンドリスト)
    /// <summary>描いたものを画像にする。描く範囲は bounds に固定する</summary>
    public ID2D1Image Draw(RawRectF bounds, Action<ID2D1DeviceContext> draw, bool clip = true)
    {
        var list = Dc.CreateCommandList();
        current.Add(list);
        var oldTarget = Dc.Target;
        var oldTransform = Dc.Transform;
        try
        {
            Dc.Target = list;
            Dc.BeginDraw();
            Dc.Clear(null);
            Dc.Transform = Matrix3x2.Identity;
            using (var clear = Dc.CreateSolidColorBrush(default))
                Dc.FillRectangle(bounds, clear);          // 範囲を固定する(透明)
            if (clip) Dc.PushAxisAlignedClip(bounds, AntialiasMode.Aliased);   // 範囲の外には描かない(ノイズなど無限の画像も、ここで切る)
            try { draw(Dc); }
            finally
            {
                Dc.Transform = Matrix3x2.Identity;
                if (clip) Dc.PopAxisAlignedClip();
                Dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
                Dc.EndDraw();
            }
        }
        finally
        {
            Dc.Transform = oldTransform;
            Dc.Target = oldTarget;
            oldTarget?.Dispose();
        }
        YMM4EffectShapes.Gfx.CloseCommandList(list);
        return list;
    }

    /// <summary>このフレームの終わりで片付けるものを登録する</summary>
    public T Keep<T>(T x) where T : IDisposable { current.Add(x); return x; }

    /// <summary>画像を描く筆(透明度つき・範囲の外は透明)</summary>
    public ID2D1ImageBrush ImageBrush(ID2D1DeviceContext dc, ID2D1Image img, RawRectF src, float opacity = 1f)
    {
        var b = dc.CreateImageBrush(img, new ImageBrushProperties
        {
            SourceRectangle = src,
            ExtendModeX = ExtendMode.Clamp,
            ExtendModeY = ExtendMode.Clamp,
            InterpolationMode = InterpolationMode.Linear,
        }, new BrushProperties { Opacity = opacity, Transform = Matrix3x2.CreateTranslation(src.Left, src.Top) });
        // 元の範囲の左上が、筆の原点に来る(D2D の決まり)ので、元の位置に戻す
        current.Add(b);
        return b;
    }

    /// <summary>小さな模様(cell の cellRect の範囲)を、area いっぱいに敷き詰めて描く</summary>
    public void Tile(ID2D1DeviceContext dc, ID2D1Image cell, RawRectF cellRect, RawRectF area, Vector2 offset, float opacity = 1f)
    {
        var br = dc.CreateImageBrush(cell, new ImageBrushProperties
        {
            SourceRectangle = cellRect,
            ExtendModeX = ExtendMode.Wrap,
            ExtendModeY = ExtendMode.Wrap,
            InterpolationMode = InterpolationMode.Linear,
        }, new BrushProperties { Opacity = opacity, Transform = Matrix3x2.CreateTranslation(cellRect.Left + offset.X, cellRect.Top + offset.Y) });
        current.Add(br);
        dc.FillRectangle(area, br);
    }

    /// <summary>形(アルファ)のまわりに、太さ width の縁(色 col)を作る。縁の画像は元の形の後ろに重ねて使う</summary>
    public ID2D1Image Outline(string id, ID2D1Image src, float width, Color4 col)
    {
        var sil = Matrix(id + "_sil", src, Fill(col.R, col.G, col.B, col.A));
        var blur = Blur(id + "_blur", sil, MathF.Max(0.5f, width / 2f));
        // ぼかした形の不透明度を強めて、元より太い形にする
        return Matrix(id + "_grow", blur, new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 4f, 0, 0, 0, 0 });
    }

    /// <summary>画像を、透明度を付けて描く</summary>
    public void DrawImage(ID2D1DeviceContext dc, ID2D1Image img, RawRectF b, float opacity)
    {
        if (opacity >= 0.999f) { dc.DrawImage(img, InterpolationMode.Linear, CompositeMode.SourceOver); return; }
        if (opacity <= 0.001f) return;
        dc.FillRectangle(b, ImageBrush(dc, img, b, opacity));
    }

    // ------------------------------------------------------------------ よく使う色の行列
    /// <summary>明るさ・コントラスト・彩度</summary>
    public static float[] Tone(float bright = 0f, float contrast = 1f, float saturation = 1f, float alpha = 1f)
    {
        const float lr = 0.2126f, lg = 0.7152f, lb = 0.0722f;
        float s = saturation, c = contrast;
        float sr = (1 - s) * lr, sg = (1 - s) * lg, sb = (1 - s) * lb;
        float o = (1 - c) * 0.5f + bright;
        return new float[]
        {
            c * (sr + s), c * sr, c * sr, 0,
            c * sg, c * (sg + s), c * sg, 0,
            c * sb, c * sb, c * (sb + s), 0,
            0, 0, 0, alpha,
            o, o, o, 0,
        };
    }

    /// <summary>
    /// 明るい所だけ残す(しきい値 th 0〜1、強さ gain)。光のにじみ用。
    /// 色はそのまま、明るさを不透明度にする(透明な所や暗い所は透明になり、黒いふちが出ない)
    /// </summary>
    public static float[] BrightPass(float th, float gain)
    {
        float k = gain / MathF.Max(0.05f, 1f - th);
        return new float[]
        {
            1, 0, 0, k * 0.2126f,
            0, 1, 0, k * 0.7152f,
            0, 0, 1, k * 0.0722f,
            0, 0, 0, 0,
            0, 0, 0, -th * k,
        };
    }

    /// <summary>色を塗りつぶす(形=アルファはそのまま)。色は 0〜1</summary>
    public static float[] Fill(float r, float g, float b, float alpha = 1f)
        => new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, alpha, r, g, b, 0 };   // 色は一定、不透明度は元×alpha

    /// <summary>1つの色の成分だけ残す(0=赤 1=緑 2=青)</summary>
    public static float[] Channel(int ch)
    {
        var m = new float[20];
        m[ch * 4 + ch] = 1f;
        m[15] = 1f;   // 3つを「足す」で重ねると、色は元どおり、不透明度は上限の1で止まる
        return m;
    }

    /// <summary>色を r,g,b にして、不透明度を「元×k + offset」にする(しきい値で切り抜く時など)</summary>
    public static float[] AlphaBoost(float k, float offset = 0f, float r = 0, float g = 0, float b = 0)
        => new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, k, r, g, b, offset };

    // ------------------------------------------------------------------ 乱数・揺れ
    /// <summary>なめらかな揺れ(-1〜1)。seed ごとに違う動き</summary>
    public static float Noise(float t, int seed)
    {
        float a = Hash(seed * 7 + 1) * 6.28f, b = Hash(seed * 7 + 2) * 6.28f, c = Hash(seed * 7 + 3) * 6.28f;
        return (MathF.Sin(t * 1.0f + a) * 0.5f + MathF.Sin(t * 2.31f + b) * 0.3f + MathF.Sin(t * 4.67f + c) * 0.2f);
    }

    public static float Hash(int n)
    {
        unchecked
        {
            uint x = (uint)n * 747796405u + 2891336453u;
            x = ((x >> (int)((x >> 28) + 4)) ^ x) * 277803737u;
            x = (x >> 22) ^ x;
            return x / (float)uint.MaxValue;
        }
    }

    public static Color4 Rgb(System.Windows.Media.Color c, float a = 1f) => new(c.R / 255f, c.G / 255f, c.B / 255f, Math.Clamp(a, 0f, 1f));
}
