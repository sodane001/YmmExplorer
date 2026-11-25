using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Shape;
using WpfColor = System.Windows.Media.Color;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

/// <summary>1フレーム分の描画情報</summary>
public readonly struct DrawContext
{
    public float Width { get; init; }
    public float Height { get; init; }
    /// <summary>アイテム先頭からの経過秒</summary>
    public float Time { get; init; }
    public long Frame { get; init; }
    public long Length { get; init; }
    public int Fps { get; init; }

    public float F(Animation a) => (float)a.GetValue(Frame, Length, Fps);
}

/// <summary>画面サイズへの合わせ方</summary>
public enum FitMode
{
    [Display(Name = "画面サイズに合わせる")] Canvas,
    [Display(Name = "画面サイズに合わせて拡大縮小")] Scale,
    [Display(Name = "指定サイズ（幅・高さ）")] Manual,
}

/// <summary>
/// 設定項目1つ分の案内(YmmExplorer のアイテムタブで使う)。
/// 図形ごとに、その図形で使わない項目を隠したり、図形に合った名前・説明・範囲にしたりする
/// </summary>
public sealed record ParamGuide(string? Label = null, string? Tip = null, bool Hidden = false, double? Min = null, double? Max = null, string? Group = null, string? Unit = null);

/// <summary>設定項目の案内を持つ図形のパラメーター</summary>
public interface IParamGuide
{
    /// <summary>項目名(プロパティ名)の案内。案内が無ければ null(そのまま表示)</summary>
    ParamGuide? Guide(string propName);
}

/// <summary>全図形共通の基底パラメーター（サイズ・シード）</summary>
public abstract partial class EffectShapeParameterBase : ShapeParameterBase
{
    protected EffectShapeParameterBase(SharedDataStore? sharedData) : base() { }

    [Display(GroupName = "サイズ", Name = "画面サイズへの合わせ方",
        Description = "画面サイズに合わせる=描く範囲を画面と同じ大きさにする。拡大縮小=指定サイズで描いたものを画面いっぱいに拡大縮小する。指定サイズ=下の幅・高さのまま")]
    [EnumComboBox]
    public FitMode Fit { get => fit; set => Set(ref fit, value); }
    FitMode fit = FitMode.Canvas;

    [Display(GroupName = "サイズ", Name = "幅（指定サイズ）", Description = "「指定サイズ」「拡大縮小」のときの基準の幅")]
    [AnimationSlider("F0", "px", 100, 4000)]
    public Animation Width { get; } = new Animation(1920, 1, 16384);

    [Display(GroupName = "サイズ", Name = "高さ（指定サイズ）", Description = "「指定サイズ」「拡大縮小」のときの基準の高さ")]
    [AnimationSlider("F0", "px", 100, 4000)]
    public Animation Height { get; } = new Animation(1080, 1, 16384);

    [Display(GroupName = "共通", Name = "シード", Description = "乱数の種。変えると配置が変わります")]
    [TextBoxSlider("F0", "", 0, 9999)]
    [Range(0, 9999)]
    public int Seed { get => seed; set => Set(ref seed, value); }
    int seed = 0;

    // 図形間で共有するデータは使わないので空実装
    protected override void LoadSharedData(SharedDataStore store) { }
    protected override void SaveSharedData(SharedDataStore store) { }

    /// <summary>描画本体。dc は Target / BeginDraw 済み</summary>
    public abstract void Draw(ID2D1DeviceContext dc, in DrawContext c);

    protected abstract IEnumerable<IAnimatable> GetExtraAnimatables();

    protected sealed override IEnumerable<IAnimatable> GetAnimatables() => [Width, Height, .. GetExtraAnimatables(), .. Animatables3D()];

    /// <summary>本物の3Dの形をそのまま作れる図形は、ここで作って true を返す(2Dの絵を立体にする代わりに使う)</summary>
    internal virtual bool BuildNative3D(in DrawContext c, Native3D n) => false;

    public override IShapeSource CreateShapeSource(IGraphicsDevicesAndContext devices)
    {
        var source = new EffectShapeSource(devices, this);
        // 3Dの図形は、3Dプレビュー(YMM43D)が入っていれば、その3D空間に立体のまま置けるようにする
        return Is3D ? Ymm43DBridge.Wrap(source, this, devices) : source;
    }

    // exo出力は非対応
    public override IEnumerable<string> CreateMaskExoFilter(int keyFrameIndex, ExoOutputDescription desc, ShapeMaskExoOutputDescription shapeMaskParameters) => [];
    public override IEnumerable<string> CreateShapeItemExoFilter(int keyFrameIndex, ExoOutputDescription desc) => [];
}

internal sealed class EffectShapeSource : IShapeSource
{
    readonly IGraphicsDevicesAndContext devices;
    readonly EffectShapeParameterBase parameter;
    readonly DisposeCollector disposer = new();
    ID2D1CommandList? commandList;
    Shape3DRenderer? renderer3D;
    bool failed3D;
    /// <summary>3Dプレビュー(YMM43D)の中に立体として描くか</summary>
    internal bool Bridged { get; set; }
    /// <summary>ふだんの画面の絵も、3Dプレビュー(YMM43D)のカメラで描いてもらう(その時は自分の3D描画を省く)</summary>
    internal bool HostRenders { get; set; }

    public ID2D1Image Output => commandList ?? throw new InvalidOperationException("Update()が呼ばれる前にOutputが参照されました");

    public EffectShapeSource(IGraphicsDevicesAndContext devices, EffectShapeParameterBase parameter)
    {
        this.devices = devices;
        this.parameter = parameter;
    }

    public void Update(TimelineItemSourceDescription desc)
    {
        long frame = desc.ItemPosition.Frame;
        long length = desc.ItemDuration.Frame;
        int fps = desc.FPS;

        float refW = (float)parameter.Width.GetValue(frame, length, fps);
        float refH = (float)parameter.Height.GetValue(frame, length, fps);

        // 画面（プロジェクト）サイズ。取得できないときは指定サイズを使う
        var mode = parameter.Fit;
        bool hasScreen = ScreenInfo.TryGet(desc, out float sw, out float sh);
        if (!hasScreen) mode = FitMode.Manual;

        float cw = mode == FitMode.Canvas ? sw : refW;
        float ch = mode == FitMode.Canvas ? sh : refH;

        var ctx = new DrawContext
        {
            Width = cw,
            Height = ch,
            Time = (float)frame / fps,
            Frame = frame,
            Length = length,
            Fps = fps,
        };

        var dc = devices.DeviceContext;
        disposer.DisposeAndClear();

        // 図形の描画は (0,0)〜(幅,高さ) の左上基準で行い、最後に、まん中が (0,0) に来るようにずらして貼り付ける。
        // YMM4 の図形アイテムは (0,0) がアイテムの位置(X・Y)になるので、こうしないと図形の左上がアイテムの位置に来てしまう
        // 本物の3Dの形を持っている図形(PV背景など)は、その形をそのまま使う(その時は2Dの絵は描かない)
        var native = parameter.Is3D || Bridged ? TryNative(ctx) : null;
        bool skip2D = native is not null && parameter.Is3D && (!failed3D || HostRenders);
        var inner = dc.CreateCommandList();
        disposer.Collect(inner);
        Render(dc, inner, ctx, !skip2D);

        // 3D: 描画命令を立体にして描いた画像を、2Dの代わりに貼る(失敗した時は2Dのまま)
        ID2D1Image image = inner;
        if (Bridged)
        {
            try
            {
                var r = renderer3D ??= new Shape3DRenderer();
                if (native is not null) r.CollectNative(native, ctx); else r.Collect(devices, inner, ctx);
            }
            catch (Exception e) { Shape3DRenderer.Report(e); }
        }
        if (parameter.Is3D && !failed3D && !HostRenders)
        {
            try
            {
                renderer3D ??= new Shape3DRenderer();
                image = native is not null
                    ? renderer3D.RenderNative(devices, native, ctx, parameter.Get3D(ctx))
                    : renderer3D.Render(devices, inner, ctx, parameter.Get3D(ctx));
            }
            catch (Exception e)
            {
                failed3D = true;
                Shape3DRenderer.Report(e);
                image = inner;
                if (skip2D)
                {
                    var flat = dc.CreateCommandList();
                    disposer.Collect(flat);
                    Render(dc, flat, ctx);
                    image = flat;
                }
            }
        }

        var outer = dc.CreateCommandList();
        disposer.Collect(outer);
        float s = 1f, ow = cw, oh = ch, ox = 0f, oy = 0f;
        if (mode == FitMode.Scale)
        {
            // 拡大縮小：指定サイズで描いたものを、画面を覆うように拡大縮小して貼り付ける
            s = MathF.Max(sw / refW, sh / refH);
            ow = sw; oh = sh;
            ox = (sw - refW * s) / 2f; oy = (sh - refH * s) / 2f;
        }

        dc.Target = outer;
        dc.BeginDraw();
        dc.Clear(null);
        try
        {
            using (var ab = dc.CreateSolidColorBrush(default))
                dc.FillRectangle(new RawRectF(-ow / 2f, -oh / 2f, ow / 2f, oh / 2f), ab);       // 描画範囲を、まん中基準の画面サイズに固定
            dc.Transform = Matrix3x2.CreateScale(s, s) * Matrix3x2.CreateTranslation(ox - ow / 2f, oy - oh / 2f);
            dc.DrawImage(image);
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
            dc.EndDraw();
            dc.Target = null;
            Gfx.CloseCommandList(outer);
        }
        commandList = outer;
    }

    Native3D? TryNative(in DrawContext ctx)
    {
        try
        {
            var n = new Native3D();
            return parameter.BuildNative3D(ctx, n) ? n : null;
        }
        catch (Exception e) { Shape3DRenderer.Report(e); return null; }
    }

    void Render(ID2D1DeviceContext dc, ID2D1CommandList cl, in DrawContext ctx, bool drawShape = true)
    {
        dc.Target = cl;
        dc.BeginDraw();
        dc.Clear(null);
        try
        {
            // 透明な矩形で描画範囲をキャンバス全体に固定する（図形の位置ずれ防止）
            using (var ab = dc.CreateSolidColorBrush(default))
                dc.FillRectangle(new RawRectF(0, 0, ctx.Width, ctx.Height), ab);
            if (drawShape) parameter.Draw(dc, ctx);
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            dc.Transform = Matrix3x2.Identity;
            dc.EndDraw();
            dc.Target = null;
            Gfx.CloseCommandList(cl);
        }
    }

    /// <summary>3Dプレビュー(YMM43D)から呼ばれる: その時間の立体を、3Dプレビューの描画先に描く</summary>
    internal void Draw3DExternal(Vortice.Direct3D11.ID3D11Device device, Vortice.Direct3D11.ID3D11DeviceContext d3d, Matrix4x4 world, Matrix4x4 viewProj,
        Vector3 cam, float opacity, int frame, int length, int fps, bool depthOnly)
    {
        try
        {
            var r = renderer3D ??= new Shape3DRenderer();
            if (!r.HasFrame(frame)) Record(r, frame, length, fps);
            var ctx = r.LastContext;
            r.DrawExternal(device, d3d, world, viewProj, cam, opacity, parameter.Get3D(ctx));
        }
        catch (Exception e) { Shape3DRenderer.Report(e); }
    }

    /// <summary>3Dプレビューの見えない物を省く判定用の大きさ</summary>
    internal (Vector3 Min, Vector3 Max) Bounds3D(int frame, int length, int fps)
    {
        try
        {
            var r = renderer3D ??= new Shape3DRenderer();
            if (!r.HasFrame(frame)) Record(r, frame, length, fps);
            return r.Bounds(parameter.Get3D(r.LastContext));
        }
        catch { return (new Vector3(-2000), new Vector3(2000)); }
    }

    /// <summary>Update がまだ来ていない時間の部品を、自分で描いて集める</summary>
    void Record(Shape3DRenderer r, int frame, int length, int fps)
    {
        fps = Math.Max(fps, 1);
        var last = r.LastContext;
        float w = last.Width > 1 ? last.Width : (float)parameter.Width.GetValue(frame, length, fps);
        float h = last.Height > 1 ? last.Height : (float)parameter.Height.GetValue(frame, length, fps);
        var ctx = new DrawContext { Width = w, Height = h, Time = (float)frame / fps, Frame = frame, Length = length, Fps = fps };
        if (TryNative(ctx) is { } native) { r.CollectNative(native, ctx); return; }
        var dc = devices.DeviceContext;
        using var cl = dc.CreateCommandList();
        Render(dc, cl, ctx);
        r.Collect(devices, cl, ctx);
    }

    public void Dispose()
    {
        disposer.Dispose();
        renderer3D?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>プロジェクトの画面サイズをリフレクションで取得する（APIの名前差を吸収）</summary>
internal static class ScreenInfo
{
    static readonly string[] names = { "ScreenSize", "VideoSize", "ProjectSize", "OutputSize", "Resolution" };

    public static bool TryGet(object desc, out float w, out float h)
    {
        w = h = 0f;
        var type = desc.GetType();
        foreach (var n in names)
        {
            object? v = null;
            try { v = type.GetProperty(n)?.GetValue(desc) ?? type.GetField(n)?.GetValue(desc); } catch { }
            if (v is null) continue;
            if (TrySize(v, out w, out h)) return true;
        }
        return false;
    }

    static bool TrySize(object v, out float w, out float h)
    {
        w = h = 0f;
        var t = v.GetType();
        object? wv = null, hv = null;
        try
        {
            wv = t.GetProperty("Width")?.GetValue(v) ?? t.GetField("Width")?.GetValue(v);
            hv = t.GetProperty("Height")?.GetValue(v) ?? t.GetField("Height")?.GetValue(v);
            if (wv is null || hv is null) return false;
            w = Convert.ToSingle(wv);
            h = Convert.ToSingle(hv);
        }
        catch { return false; }
        return w > 1f && h > 1f;
    }
}

/// <summary>Direct2D描画ヘルパー</summary>
internal static class Gfx
{
    public static Color4 C4(WpfColor c, float alpha = 1f)
        => new(c.R / 255f, c.G / 255f, c.B / 255f, Math.Clamp(c.A / 255f * alpha, 0f, 1f));

    public static Color4 C4(float r, float g, float b, float a)
        => new(r, g, b, Math.Clamp(a, 0f, 1f));


    /// <summary>Vortice のバージョン差で Close() の定義が異なるため、リフレクションで呼び出す</summary>
    public static void CloseCommandList(ID2D1CommandList cl)
    {
        const BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var t = cl.GetType();
        foreach (var name in new[] { "Close", "ClosePrivate", "CloseCommandList" })
        {
            var m = t.GetMethod(name, f, null, Type.EmptyTypes, null);
            if (m is not null) { m.Invoke(cl, null); return; }
        }
        foreach (var m in t.GetMethods(f))
        {
            if (m.Name.EndsWith(".Close") && m.GetParameters().Length == 0) { m.Invoke(cl, null); return; }
        }
        // 見つからない場合は自動クローズされる版とみなして何もしない
    }


    public static WpfColor Mix(WpfColor a, WpfColor b, float t)
        => WpfColor.FromArgb(255,
            (byte)Math.Clamp(a.R + (b.R - a.R) * t, 0, 255),
            (byte)Math.Clamp(a.G + (b.G - a.G) * t, 0, 255),
            (byte)Math.Clamp(a.B + (b.B - a.B) * t, 0, 255));

    public static float Smooth(float a, float b, float x)
    {
        float t = Math.Clamp((x - a) / (b - a), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    /// <summary>水底に揺らぐ光の網目（コースティクス）。0〜1</summary>
    public static float Caustic(float x, float y, float t, float scale)
    {
        float px = x * scale, py = y * scale;
        float wx = px + 0.8f * MathF.Sin(py * 1.3f + t * 0.7f);
        float wy = py + 0.8f * MathF.Sin(px * 1.1f - t * 0.55f);
        float a = 1f - MathF.Abs(MathF.Sin(wx * 2.2f + t * 0.4f));
        float b = 1f - MathF.Abs(MathF.Sin(wy * 2.2f - t * 0.35f));
        float c = 1f - MathF.Abs(MathF.Sin((wx + wy) * 1.6f + t * 0.3f));
        float m = MathF.Max(a * b, MathF.Max(b * c, a * c));
        return MathF.Pow(m, 3f);
    }

    /// <summary>大きなうねりによる明暗。-1〜1</summary>
    public static float Shade(float x, float y, float t)
        => (MathF.Sin(x * 0.006f + t * 0.4f) * MathF.Sin(y * 0.007f - t * 0.3f)
            + 0.5f * MathF.Sin((x + y) * 0.010f + t * 0.6f)) / 1.5f;


    /// <summary>色相（0〜360）</summary>
    public static float Hue(WpfColor c)
    {
        float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
        float max = MathF.Max(r, MathF.Max(g, b)), min = MathF.Min(r, MathF.Min(g, b));
        float d = max - min;
        if (d < 1e-5f) return 0f;
        float h;
        if (max == r) h = ((g - b) / d) % 6f;
        else if (max == g) h = (b - r) / d + 2f;
        else h = (r - g) / d + 4f;
        h *= 60f;
        return h < 0 ? h + 360f : h;
    }

    public static WpfColor Hsv(float h, float s, float v)
    {
        h = Mod(h, 360f) / 60f;
        int i = (int)MathF.Floor(h);
        float f = h - i;
        float p = v * (1f - s), q = v * (1f - s * f), t = v * (1f - s * (1f - f));
        (float r, float g, float b) = i switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q),
        };
        return WpfColor.FromRgb(
            (byte)Math.Clamp(r * 255f, 0f, 255f),
            (byte)Math.Clamp(g * 255f, 0f, 255f),
            (byte)Math.Clamp(b * 255f, 0f, 255f));
    }


    /// <summary>多角形の塗りつぶし（ふだんはパスで1回で塗る。使えない時は1pxの行ごとに内側を塗る）</summary>
    public static void FillPoly(ID2D1DeviceContext dc, ID2D1Brush brush, IList<Vector2> p, float yMin = float.NegativeInfinity, float yMax = float.PositiveInfinity)
    {
        if (p.Count < 3) return;
        // 軽い方式: パスで1回で塗る(使えない時だけ、下の1行ずつの方式)
        if (GeoKit.Enabled)
        {
            bool clip = float.IsFinite(yMin) || float.IsFinite(yMax);
            if (clip) dc.PushAxisAlignedClip(new RawRectF(-1e7f, float.IsFinite(yMin) ? yMin : -1e7f, 1e7f, float.IsFinite(yMax) ? yMax : 1e7f), AntialiasMode.Aliased);
            bool ok = GeoKit.FillPoly(dc, brush, p);
            if (clip) dc.PopAxisAlignedClip();
            if (ok) return;
        }
        float minY = p[0].Y, maxY = p[0].Y;
        foreach (var v in p) { if (v.Y < minY) minY = v.Y; if (v.Y > maxY) maxY = v.Y; }
        int y0 = (int)MathF.Floor(MathF.Max(minY, yMin)), y1 = (int)MathF.Ceiling(MathF.Min(maxY, yMax));
        var xs = new List<float>();
        for (int y = y0; y < y1; y++)
        {
            float sy = y + 0.5f;
            xs.Clear();
            for (int i = 0; i < p.Count; i++)
            {
                var a = p[i];
                var b = p[(i + 1) % p.Count];
                if ((a.Y <= sy && b.Y > sy) || (b.Y <= sy && a.Y > sy))
                    xs.Add(a.X + (b.X - a.X) * (sy - a.Y) / (b.Y - a.Y));
            }
            xs.Sort();
            for (int k = 0; k + 1 < xs.Count; k += 2)
                dc.FillRectangle(new RawRectF(xs[k], y, xs[k + 1], y + 1f), brush);
        }
    }

    /// <summary>円弧の点列（角度はラジアン、0=右、時計回りが正）</summary>
    public static List<Vector2> ArcPts(Vector2 c, float r, float a0, float a1, int n = 64)
    {
        var pts = new List<Vector2>();
        for (int i = 0; i <= n; i++)
        {
            float a = a0 + (a1 - a0) * i / n;
            pts.Add(c + new Vector2(MathF.Cos(a), MathF.Sin(a)) * r);
        }
        return pts;
    }

    /// <summary>0〜1の安定した疑似乱数（整数から）</summary>
    public static float Hash01(int n)
    {
        unchecked
        {
            uint x = (uint)n * 747796405u + 2891336453u;
            x = ((x >> (int)((x >> 28) + 4u)) ^ x) * 277803737u;
            x = (x >> 22) ^ x;
            return (x & 0xFFFFFF) / (float)0x1000000;
        }
    }

    public static float Mod(float a, float b) => a - b * MathF.Floor(a / b);
    public static float Rad(float deg) => deg * MathF.PI / 180f;

    /// <summary>0°=真下、正の角度=右向き</summary>
    public static Vector2 Dir(float deg)
    {
        float r = Rad(deg);
        return new Vector2(MathF.Sin(r), MathF.Cos(r));
    }

    public static float NextF(this Random rng) => (float)rng.NextDouble();

    public static void Rect(ID2D1DeviceContext dc, ID2D1Brush brush, float x, float y, float w, float h)
        => dc.FillRectangle(new RawRectF(x, y, x + w, y + h), brush);

    public static ID2D1RadialGradientBrush Radial(ID2D1DeviceContext dc, Vector2 center, float radius,
        params (float pos, Color4 col)[] stops)
    {
        var gs = stops.Select(s => new GradientStop { Position = s.pos, Color = s.col }).ToArray();
        using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
        return dc.CreateRadialGradientBrush(new RadialGradientBrushProperties
        {
            Center = center,
            GradientOriginOffset = Vector2.Zero,
            RadiusX = radius,
            RadiusY = radius,
        }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
    }

    public static ID2D1LinearGradientBrush Linear(ID2D1DeviceContext dc, Vector2 start, Vector2 end,
        params (float pos, Color4 col)[] stops)
    {
        var gs = stops.Select(s => new GradientStop { Position = s.pos, Color = s.col }).ToArray();
        using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
        return dc.CreateLinearGradientBrush(new LinearGradientBrushProperties
        {
            StartPoint = start,
            EndPoint = end,
        }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
    }

    /// <summary>折れ線（関節に円を置いて隙間を埋める）</summary>
    public static void Polyline(ID2D1DeviceContext dc, ID2D1Brush brush, IReadOnlyList<Vector2> pts, float width)
    {
        if (pts.Count >= 3 && GeoKit.StrokePath(dc, brush, pts, width)) return;
        for (int i = 0; i < pts.Count - 1; i++)
            dc.DrawLine(pts[i], pts[i + 1], brush, width);
        float r = width / 2f;
        for (int i = 0; i < pts.Count; i++)
            dc.FillEllipse(new Ellipse(pts[i], r, r), brush);
    }

    /// <summary>始点から終点に向かって薄くなる太線（光芒・光の筋用）</summary>
    public static void FadeLine(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, Vector2 a, Vector2 b,
        float w0, float w1, WpfColor color, float alpha0, int steps = 12, float power = 1.5f)
    {
        for (int s = 0; s < steps; s++)
        {
            float t0 = s / (float)steps, t1 = (s + 1) / (float)steps, mid = (t0 + t1) / 2f;
            var p0 = Vector2.Lerp(a, b, t0);
            var p1 = Vector2.Lerp(a, b, t1);
            float width = w0 + (w1 - w0) * mid;
            brush.Color = C4(color, alpha0 * MathF.Pow(1f - mid, power));
            dc.DrawLine(p0, p1, brush, width);
        }
    }
}
