using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Numerics;
using SharpGen.Runtime;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Shape;
using YMM4EffectShapes;

using WColor = System.Windows.Media.Color;
using WRect = System.Windows.Rect;
using WPoint = System.Windows.Point;

namespace YmmExplorer;

/// <summary>
/// 図形タブで自動収録した図形の、標準(初期値)の状態のプレビュー画像を作る。
///  1. 同梱の演出図形 → 自分で描画のしくみを持っているので、そのまま描いて画像にする
///  2. YMM4標準・他のプラグインの図形 → YMM4と同じ描画部品で、初期値のまま描く
///  3. それもできなかった図形 → 名前(円・四角形・星など)から、それらしい形の画像を作る
/// 画像は専用の描画先に作るので、YMM4のプレビューやタイムラインには触れない。
/// UIスレッドから呼んでください。
/// </summary>
public static class ShapeThumbRenderer
{
    const int W = 1920, H = 1080;            // 同梱の演出図形は、この画面サイズで描いて縮小する
    const int GW = 640, GH = 360;            // 標準・他プラグインの図形の描画キャンバス
    const int Fps = 30, AtFrame = 60, LengthFrames = 300;   // 2秒目(出だしのアニメーションが終わったころ)を描く
    static readonly int[] MotionFrames = [6, 9, 12, 15, 18, 24, 30, 45];
    static readonly int[] ShotFrames = [AtFrame, 15, 30, 9, 45, 4, 90, 120, 150, 200, 270];
    static readonly Vortice.Mathematics.Color4 Back = new(0.13f, 0.13f, 0.15f, 1f);

    public static string LastError { get; private set; } = "";

    /// <summary>図形の型名から、標準の状態のプレビュー画像を作る。どの方法でも作れなければ null</summary>
    /// <param name="midMotion">画面転換など、動きの途中がいちばん分かりやすい図形か</param>
    public static BitmapSource? Render(string pluginTypeName, string displayName, bool midMotion = false)
    {
        try { return RenderCore(pluginTypeName, displayName, midMotion); }
        finally { if (sessions == 0) ReleaseDevices(); }
    }

    static BitmapSource? RenderCore(string pluginTypeName, string displayName, bool midMotion)
    {
        LastError = "";
        Type? t = null;
        try { t = FindPluginType(pluginTypeName); } catch { }
        if (t != null)
        {
            try
            {
                var plugin = Activator.CreateInstance(t);
                if (plugin is IShapePlugin sp)
                {
                    var param = sp.CreateShapeParameter(null);
                    var img = param is EffectShapeParameterBase eff ? RenderEffectShape(eff, midMotion) : RenderGeneric(param);
                    if (img != null) return img;
                }
                else if (plugin != null)
                {
                    var create = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(m => m.Name == "CreateShapeParameter");
                    var args = create?.GetParameters().Select(p => p.HasDefaultValue ? p.DefaultValue : null).ToArray();
                    var param = create?.Invoke(plugin, args);
                    if (param != null)
                    {
                        var img = RenderGeneric(param);
                        if (img != null) return img;
                    }
                }
            }
            catch (Exception ex) { LastError = ex.GetBaseException().Message; }
        }
        else LastError = "図形の型が見つかりませんでした";
        // 描けなかった時は、名前から形を推測して作る
        try { return Symbol(displayName); }
        catch (Exception ex) { LastError = ex.GetBaseException().Message; return null; }
    }

    /// <summary>設定を入れた同梱図形を、そのまま画像にする(PV背景の設定ごとのサムネイル用)</summary>
    public static BitmapSource? RenderParameter(EffectShapeParameterBase param)
    {
        LastError = "";
        try { return RenderEffectShape(param, false); }
        catch (Exception ex) { LastError = ex.GetBaseException().Message; return null; }
        finally { if (sessions == 0) ReleaseDevices(); }
    }

    static Type? FindPluginType(string name)
    {
        var t = Type.GetType(name);
        if (t != null) return t;
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (a.IsDynamic) continue;
            try { t = a.GetType(name, false); } catch { t = null; }
            if (t != null) return t;
        }
        foreach (var c in System.Runtime.Loader.AssemblyLoadContext.All)
            foreach (var a in c.Assemblies)
            {
                try { t = a.GetType(name, false); } catch { t = null; }
                if (t != null) return t;
            }
        return null;
    }

    // ------------------------------------------------------------------ 同梱の演出図形
    static BitmapSource? RenderEffectShape(EffectShapeParameterBase param, bool midMotion)
    {
        var dc = Devices().DeviceContext;
        if (midMotion)
        {
            // 画面の半分くらいが動いている時間を選ぶ(全部覆われた絵や、何も無い絵を避ける)
            BitmapSource? best = null;
            double bestScore = -1;
            foreach (int frame in MotionFrames)
            {
                LastError = "";
                var mc = new DrawContext { Width = W, Height = H, Time = frame / (float)Fps, Frame = frame, Length = LengthFrames, Fps = Fps };
                var shot = param.Is3D ? Render3D(param, frame) : Capture(dc, W, H, () => param.Draw(dc, mc));
                if (shot == null) continue;
                double ink = InkRatio(shot), score = Math.Min(ink, 1 - ink);
                if (score > bestScore) { bestScore = score; best = shot; }
            }
            LastError = "";
            if (best != null) return best;      // 動きの途中に絵が無い図形は、下の通常の時間で探す
        }
        // 雷や画面転換のように一瞬だけ出る図形は、2秒目に何も無いので、絵が出る時間を探す
        foreach (int frame in ShotFrames)
        {
            LastError = "";
            var ctx = new DrawContext { Width = W, Height = H, Time = frame / (float)Fps, Frame = frame, Length = LengthFrames, Fps = Fps };
            var img = param.Is3D ? Render3D(param, frame) : Capture(dc, W, H, () => param.Draw(dc, ctx));
            if (img != null) return img;
        }
        return null;
    }

    /// <summary>3Dの図形は、YMM4と同じ「図形の描画元」を通して描く(立体にする処理はそこにある)</summary>
    static BitmapSource? Render3D(EffectShapeParameterBase param, int frame)
    {
        var ctx = Devices();
        var dc = ctx.DeviceContext;
        using var source = param.CreateShapeSource(ctx);
        var timeline = new TimelineSourceDescription(new System.Drawing.Size(W, H), new YukkuriMovieMaker.Player.Video.FrameTime(frame, Fps), new YukkuriMovieMaker.Player.Video.FrameTime(LengthFrames, Fps), Fps, TimelineSourceUsage.Paused, Guid.Empty, []);
        source.Update(new TimelineItemSourceDescription(timeline, frame, LengthFrames, 0));
        var output = source.Output;
        return Capture(dc, W, H, () =>
        {
            dc.Transform = Matrix3x2.CreateTranslation(W / 2f, H / 2f);
            dc.DrawImage(output);
        });
    }

    // ------------------------------------------------------------------ YMM4標準・他のプラグインの図形
    static GraphicsDevices? devices;
    static GraphicsDevicesAndContext? context;
    static int sessions;

    /// <summary>たくさん続けて作る時に、描画部品を使い回す(終わったら Dispose で片付く)</summary>
    public static IDisposable BeginSession() { sessions++; return new SessionEnd(); }

    sealed class SessionEnd : IDisposable
    {
        bool done;
        public void Dispose()
        {
            if (done) return;
            done = true;
            if (--sessions == 0) ReleaseDevices();
        }
    }

    static void ReleaseDevices()
    {
        try { context?.Dispose(); } catch { }
        try { devices?.Dispose(); } catch { }
        context = null;
        devices = null;
        // 同じアドレスに新しい道具ができても、前の道具で作ったパスを使わないようにする
        GeoKit.Reset();
    }

    static BitmapSource? RenderGeneric(object param)
    {
        if (param is not IShapeParameter shape) { LastError = "図形の設定の型が想定と違います"; return null; }
        IShapeSource? source = null;
        try
        {
            var ctx = Devices();
            var dc = ctx.DeviceContext;
            source = shape.CreateShapeSource(ctx);
            var timeline = new TimelineSourceDescription(new System.Drawing.Size(W, H), new YukkuriMovieMaker.Player.Video.FrameTime(AtFrame, Fps), new YukkuriMovieMaker.Player.Video.FrameTime(LengthFrames, Fps), Fps, TimelineSourceUsage.Paused, Guid.Empty, []);
            source.Update(new TimelineItemSourceDescription(timeline, AtFrame, LengthFrames, 0));
            var output = source.Output;

            // 図形は (0,0) がまん中。大きさに合わせて、キャンバスに収まるように拡大縮小する
            float s = 1f, cx = 0f, cy = 0f;
            if (TryBounds(dc, output, out var bd))
            {
                float bw = bd.Right - bd.Left, bh = bd.Bottom - bd.Top;
                if (bw > 0.5f && bh > 0.5f && bw < 1e6f && bh < 1e6f)
                {
                    s = MathF.Min(MathF.Min(GW * 0.8f / bw, GH * 0.8f / bh), 4f);
                    cx = (bd.Left + bd.Right) / 2f; cy = (bd.Top + bd.Bottom) / 2f;
                }
            }

            return Capture(dc, GW, GH, () =>
            {
                dc.Transform = Matrix3x2.CreateTranslation(-cx, -cy) * Matrix3x2.CreateScale(s) * Matrix3x2.CreateTranslation(GW / 2f, GH / 2f);
                dc.DrawImage(output);
            });
        }
        catch (Exception ex)
        {
            LastError = ex.GetBaseException().Message;
            return null;
        }
        finally
        {
            try { source?.Dispose(); } catch { }
        }
    }

    static GraphicsDevicesAndContext Devices()
    {
        if (context == null)
        {
            devices = new GraphicsDevices();
            context = new GraphicsDevicesAndContext(devices);
        }
        return context;
    }

    /// <summary>専用の描画先に描いて、画像として取り出す。何も描かれなかったら null</summary>
    // ------------------------------------------------------------------ 同梱の映像エフェクト(サンプル画像にかけたサムネイル)
    const int FW = 320, FH = 180;

    /// <summary>
    /// サンプル画像に、同梱の映像エフェクトをかけた絵(320×180)。登場・退場のように時間で変わるものもあるので、
    /// いくつかの時間で描いて、かける前といちばん違って見える時間を選ぶ。描けない時は null(理由は LastError)
    /// </summary>
    public static BitmapSource? RenderVideoEffect(Type type, BitmapSource sample)
    {
        LastError = "";
        string stage = "start";
        try
        {
            if (Activator.CreateInstance(type) is not YmmExplorer.Fx.FxVideoEffectBase fx) { LastError = "同梱の映像エフェクトではありません"; return null; }
            stage = "devices"; var dc = Devices().DeviceContext;
            stage = "pixels"; BitmapSource src = sample.Format == PixelFormats.Pbgra32 ? sample : new FormatConvertedBitmap(sample, PixelFormats.Pbgra32, null, 0);
            int sw = src.PixelWidth, sh = src.PixelHeight, stride = sw * 4;
            var px = new byte[stride * sh];
            src.CopyPixels(px, stride, 0);
            var pf = new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied);
            var pin = GCHandle.Alloc(px, GCHandleType.Pinned);
            ID2D1Bitmap1 bmp; stage = "bitmap";
            try { bmp = dc.CreateBitmap(new Vortice.Mathematics.SizeI(sw, sh), IntPtr.Zero, 0, new BitmapProperties1(pf, 96, 96, BitmapOptions.None)); bmp.CopyFromMemory(pin.AddrOfPinnedObject(), stride); }
            finally { pin.Free(); }
            using (bmp)
            {
                stage = "kit"; using var kit = new YmmExplorer.Fx.FxKit(dc);
                float scale = Math.Min(FW / (float)sw, FH / (float)sh);
                var place = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation((FW - sw * scale) / 2f, (FH - sh * scale) / 2f);
                var b = dc.GetImageLocalBounds(bmp);
                kit.NoiseArea = b;
                stage = "plain"; var plain = Capture(dc, FW, FH, () => { dc.Transform = place; dc.DrawImage(bmp); });
                const int len = 180, fps = 60;
                BitmapSource? best = null;
                double bestDiff = -1;
                string err = "";
                stage = "frames";
                foreach (int frame in new[] { 90, 30, 150, 12, 168, 60, 120 })
                {
                    BitmapSource? shot = null;
                    try
                    {
                        var f = new YmmExplorer.Fx.FxFrame { Frame = frame, Length = len, Fps = fps, B = b };
                        // 模様を描くエフェクトは中で BeginDraw するので、描く前(BeginDraw の外)で組み立てる
                        var img = fx.Build(kit, bmp, f);
                        shot = Capture(dc, FW, FH, () =>
                        {
                            dc.Transform = place;
                            dc.DrawImage(img);
                        });
                    }
                    catch (Exception ex) { err = ex.GetBaseException().Message; }
                    finally { try { kit.EndFrame(); } catch { } }
                    if (shot == null) continue;
                    // 違いが大きいほど分かりやすいが、画面全体が別の色に変わった絵(真っ白の光など)は、何のエフェクトか分かりにくいので下げる
                    double d = plain == null ? 1 : Difference(plain, shot);
                    if (d > 150) d = Math.Max(0, 300 - d);   // 反転など全体が大きく変わるものも、絵は使う
                    if (d > bestDiff + 1e-6) { bestDiff = d; best = shot; }
                }
                try { kit.EndFrame(); kit.EndFrame(); } catch { }
                if (best == null) LastError = err.Length > 0 ? err : "絵を描けませんでした";
                return best;
            }
        }
        catch (Exception ex) { LastError = "[" + stage + "] " + ex.GetBaseException().Message; return null; }
        finally { if (sessions == 0) ReleaseDevices(); }
    }

    /// <summary>2つの絵の違い(間引いて比べた、明るさの差の平均)</summary>
    static double Difference(BitmapSource a, BitmapSource b)
    {
        int w = Math.Min(a.PixelWidth, b.PixelWidth), h = Math.Min(a.PixelHeight, b.PixelHeight), stride = w * 4;
        var pa = new byte[stride * h]; var pb = new byte[stride * h];
        a.CopyPixels(new Int32Rect(0, 0, w, h), pa, stride, 0); b.CopyPixels(new Int32Rect(0, 0, w, h), pb, stride, 0);
        long sum = 0; int n = 0;
        for (int y = 0; y < h; y += 5)
            for (int x = 0; x < w; x += 5)
            {
                int i = y * stride + x * 4;
                sum += Math.Abs(pa[i] - pb[i]) + Math.Abs(pa[i + 1] - pb[i + 1]) + Math.Abs(pa[i + 2] - pb[i + 2]);
                n++;
            }
        return n == 0 ? 0 : sum / (double)n;
    }

    static BitmapSource? Capture(ID2D1DeviceContext dc, int w, int h, Action draw)
    {
        var pf = new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied);
        var size = new Vortice.Mathematics.SizeI(w, h);
        using var target = dc.CreateBitmap(size, IntPtr.Zero, 0, new BitmapProperties1(pf, 96, 96, BitmapOptions.Target | BitmapOptions.CannotDraw));
        using var cpu = dc.CreateBitmap(size, IntPtr.Zero, 0, new BitmapProperties1(pf, 96, 96, BitmapOptions.CpuRead | BitmapOptions.CannotDraw));
        dc.Target = target;
        dc.BeginDraw();
        try
        {
            dc.Clear(Back);
            dc.Transform = Matrix3x2.Identity;
            draw();
        }
        finally
        {
            dc.Transform = Matrix3x2.Identity;
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            dc.EndDraw();
            dc.Target = null;
        }
        cpu.CopyFromBitmap(target);
        var map = cpu.Map(MapOptions.Read);
        var px = new byte[w * h * 4];
        try
        {
            for (int y = 0; y < h; y++) Marshal.Copy(map.Bits + y * (int)map.Pitch, px, y * w * 4, w * 4);
        }
        finally { cpu.Unmap(); }

        // ファイルを選ぶ図形など、初期値では何も描かれないものは、名前のサムネイルにする
        if (IsBlank(px)) { LastError = "初期値では何も描かれない図形です"; return null; }
        var bmp = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgr32, null, px, w * 4);
        bmp.Freeze();
        return bmp;
    }

    /// <summary>背景の色ではない部分の割合(間引いて数える)</summary>
    static double InkRatio(BitmapSource b)
    {
        int w = b.PixelWidth, h = b.PixelHeight, stride = w * 4, hit = 0, all = 0;
        var px = new byte[stride * h];
        b.CopyPixels(px, stride, 0);
        int cb = (int)(Back.B * 255f), cg = (int)(Back.G * 255f), cr = (int)(Back.R * 255f);
        for (int y = 0; y < h; y += 12)
            for (int x = 0; x < w; x += 12)
            {
                int i = y * stride + x * 4; all++;
                if (Math.Abs(px[i] - cb) > 10 || Math.Abs(px[i + 1] - cg) > 10 || Math.Abs(px[i + 2] - cr) > 10) hit++;
            }
        return all == 0 ? 0 : hit / (double)all;
    }

    static bool IsBlank(byte[] bgra)
    {
        int b = (int)(Back.B * 255f), g = (int)(Back.G * 255f), r = (int)(Back.R * 255f), seen = 0;
        for (int i = 0; i < bgra.Length; i += 4)
            if (Math.Abs(bgra[i] - b) > 10 || Math.Abs(bgra[i + 1] - g) > 10 || Math.Abs(bgra[i + 2] - r) > 10)
                if (++seen > 8) return false;
        return true;
    }

    static bool TryBounds(ID2D1DeviceContext dc, ID2D1Image img, out RawRectF r)
    {
        r = default;
        try
        {
            foreach (var m in typeof(ID2D1DeviceContext).GetMethods().Where(x => x.Name == "GetImageLocalBounds"))
            {
                var ps = m.GetParameters();
                if (ps.Length == 1 && m.ReturnType == typeof(RawRectF)) { r = (RawRectF)m.Invoke(dc, new object[] { img })!; return true; }
                if (ps.Length == 2 && ps[1].ParameterType.IsByRef)
                {
                    var args = new object?[] { img, null };
                    m.Invoke(dc, args);
                    if (args[1] is RawRectF rr) { r = rr; return true; }
                }
            }
        }
        catch { }
        return false;
    }

    // ------------------------------------------------------------------ 名前から推測した形
    static BitmapSource Symbol(string name)
    {
        const int w = 480, h = 270;
        var dv = new DrawingVisual();
        using (var g = dv.RenderOpen())
        {
            g.DrawRectangle(new SolidColorBrush(WColor.FromRgb(33, 33, 38)), null, new WRect(0, 0, w, h));
            var fill = Brushes.White;
            var c = new WPoint(w / 2.0, h / 2.0);
            double r = 85;
            string n = name ?? "";
            bool Has(params string[] keys) => keys.Any(k => n.Contains(k, StringComparison.OrdinalIgnoreCase));
            int poly = PolygonCount(n);

            if (Has("背景", "Background", "塗りつぶし")) g.DrawRectangle(fill, null, new WRect(20, 20, w - 40, h - 40));
            else if (Has("角丸", "RoundRect", "Rounded")) g.DrawRoundedRectangle(fill, null, new WRect(c.X - r * 1.3, c.Y - r, r * 2.6, r * 2), 30, 30);
            else if (Has("四角", "矩形", "長方形", "正方形", "Rect", "Square", "Box")) g.DrawRectangle(fill, null, new WRect(c.X - r, c.Y - r, r * 2, r * 2));
            else if (Has("三角", "Triangle")) g.DrawGeometry(fill, null, Poly(c, r * 1.15, 3, -Math.PI / 2, 1));
            else if (Has("星", "Star")) g.DrawGeometry(fill, null, Poly(c, r * 1.1, 5, -Math.PI / 2, 0.45));
            else if (poly >= 3) g.DrawGeometry(fill, null, Poly(c, r * 1.1, poly, -Math.PI / 2, 1));
            else if (Has("ハート", "Heart")) g.DrawGeometry(fill, null, Heart(c, r));
            else if (Has("矢印", "Arrow")) g.DrawGeometry(fill, null, Arrow(c, r));
            else if (Has("吹き出し", "Balloon", "Bubble")) g.DrawGeometry(fill, null, Balloon(c, r));
            else if (Has("十字", "Cross", "Plus")) { g.DrawRectangle(fill, null, new WRect(c.X - r, c.Y - r * 0.3, r * 2, r * 0.6)); g.DrawRectangle(fill, null, new WRect(c.X - r * 0.3, c.Y - r, r * 0.6, r * 2)); }
            else if (Has("線", "Line")) g.DrawLine(new Pen(fill, 14) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, new WPoint(c.X - r * 1.5, c.Y + r * 0.6), new WPoint(c.X + r * 1.5, c.Y - r * 0.6));
            else if (Has("円", "楕円", "Circle", "Ellipse", "丸")) g.DrawEllipse(fill, null, c, r, r);
            else
            {
                // 形が分からない時は、名前の頭文字の入った枠
                g.DrawRoundedRectangle(null, new Pen(new SolidColorBrush(WColor.FromRgb(160, 165, 180)), 6), new WRect(c.X - r, c.Y - r, r * 2, r * 2), 26, 26);
                var label = n.Length > 0 ? n[..1] : "?";
                var ft = new FormattedText(label, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Yu Gothic UI"), 90, fill, 1.0);
                g.DrawText(ft, new WPoint(c.X - ft.Width / 2, c.Y - ft.Height / 2));
            }
        }
        var bmp = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
        bmp.Render(dv);
        bmp.Freeze();
        return bmp;
    }

    static int PolygonCount(string n)
    {
        string[] kan = { "", "", "", "三", "四", "五", "六", "七", "八", "九", "十" };
        for (int i = 3; i <= 10; i++)
            if (n.Contains(kan[i] + "角形") || n.Contains(i.ToString(CultureInfo.InvariantCulture) + "角形")) return i;
        if (n.Contains("多角形") || n.Contains("Polygon", StringComparison.OrdinalIgnoreCase)) return 6;
        return 0;
    }

    static Geometry Poly(WPoint c, double r, int n, double rot, double inner)
    {
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            int count = inner < 1 ? n * 2 : n;
            for (int i = 0; i < count; i++)
            {
                double rr = inner < 1 && i % 2 == 1 ? r * inner : r;
                double a = rot + i * Math.PI * 2 / count;
                var p = new WPoint(c.X + Math.Cos(a) * rr, c.Y + Math.Sin(a) * rr);
                if (i == 0) ctx.BeginFigure(p, true, true); else ctx.LineTo(p, true, false);
            }
        }
        g.Freeze();
        return g;
    }

    static Geometry Heart(WPoint c, double r)
    {
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            for (int i = 0; i <= 64; i++)
            {
                double t = i * Math.PI * 2 / 64;
                double x = 16 * Math.Pow(Math.Sin(t), 3), y = -(13 * Math.Cos(t) - 5 * Math.Cos(2 * t) - 2 * Math.Cos(3 * t) - Math.Cos(4 * t));
                var p = new WPoint(c.X + x * r / 16, c.Y + y * r / 16);
                if (i == 0) ctx.BeginFigure(p, true, true); else ctx.LineTo(p, true, false);
            }
        }
        g.Freeze();
        return g;
    }

    static Geometry Arrow(WPoint c, double r)
    {
        var pts = new[] { (-1.4, -0.3), (0.3, -0.3), (0.3, -0.8), (1.4, 0.0), (0.3, 0.8), (0.3, 0.3), (-1.4, 0.3) };
        var g = new StreamGeometry();
        using (var ctx = g.Open())
            for (int i = 0; i < pts.Length; i++)
            {
                var p = new WPoint(c.X + pts[i].Item1 * r, c.Y + pts[i].Item2 * r);
                if (i == 0) ctx.BeginFigure(p, true, true); else ctx.LineTo(p, true, false);
            }
        g.Freeze();
        return g;
    }

    static Geometry Balloon(WPoint c, double r)
    {
        var body = new EllipseGeometry(new WPoint(c.X, c.Y - r * 0.15), r * 1.5, r * 0.85);
        var tail = new StreamGeometry();
        using (var ctx = tail.Open())
        {
            ctx.BeginFigure(new WPoint(c.X - r * 0.5, c.Y + r * 0.4), true, true);
            ctx.LineTo(new WPoint(c.X - r * 0.9, c.Y + r * 1.15), true, false);
            ctx.LineTo(new WPoint(c.X - r * 0.05, c.Y + r * 0.5), true, false);
        }
        var g = new CombinedGeometry(GeometryCombineMode.Union, body, tail);
        g.Freeze();
        return g;
    }
}
