using System.Numerics;
using Vortice;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.Mathematics;

// FigureBegin・FigureEnd・FillMode は、Vortice の版によって Vortice.Direct2D1 か Vortice.DCommon のどちらかにある。
// どちらでもコンパイルできるように、Vortice.DCommon の名前空間を必ず存在させておく
namespace Vortice.DCommon
{
    internal static class DCommonNamespaceAnchor { }
}

namespace YMM4EffectShapes
{
    /// <summary>
    /// 描画を軽くするための道具。
    /// 多角形・折れ線・文字の形を、1px の行ごとに長方形で塗る方式(1つの形で数百〜数千回の描画)から、
    /// Direct2D のパス(ジオメトリ)で1回で描く方式に置きかえる。
    /// 光(Glow)のグラデーションのブラシも、毎回作らずに使い回す。
    /// 失敗した時は false を返すので、呼んだ側は前の方式で描く。
    /// </summary>
    internal static class GeoKit
    {
        // 描画先(デバイスコンテキスト)ごとの、Direct2D のファクトリーと道具(スレッドごとに持つ)
        [ThreadStatic] static ID2D1Factory? factory;
        [ThreadStatic] static IntPtr factoryOwner;
        [ThreadStatic] static ID2D1StrokeStyle? roundStroke;
        [ThreadStatic] static bool broken;
        [ThreadStatic] static int failures;

        /// <summary>失敗した時: 道具を作り直す。続けて何度も失敗したら、そのスレッドでは前の方式に戻す</summary>
        static bool Fail()
        {
            try { roundStroke?.Dispose(); } catch { }
            try { factory?.Dispose(); } catch { }
            roundStroke = null; factory = null; factoryOwner = IntPtr.Zero;
            if (++failures >= 3) broken = true;
            return false;
        }

        static bool Ok() { failures = 0; return true; }

        /// <summary>
        /// 描画の道具(デバイス)を作り直した時に上げる番号。作り直すと、前と同じアドレスに新しい道具ができることがあり、
        /// アドレスだけで覚えたパスを使うと、別の道具のパスで描くことになって絵が出なくなる。番号も合わせて覚える
        /// </summary>
        public static int Epoch => epoch;
        static int epoch;

        /// <summary>描画の道具を捨てた時に呼ぶ(このスレッドの道具を捨て、覚えたパスも使わないようにする)</summary>
        public static void Reset()
        {
            Interlocked.Increment(ref epoch);
            try { roundStroke?.Dispose(); } catch { }
            try { factory?.Dispose(); } catch { }
            roundStroke = null; factory = null; factoryOwner = IntPtr.Zero;
            if (glowStops != null) foreach (var gs in glowStops.Values) try { gs.Dispose(); } catch { }
            glowStops = null; glowOwner = IntPtr.Zero; glowBroken = false;
            broken = false; failures = 0;
        }

        [ThreadStatic] static Dictionary<uint, ID2D1GradientStopCollection>? glowStops;
        [ThreadStatic] static IntPtr glowOwner;
        [ThreadStatic] static bool glowBroken;

        /// <summary>パスで描く方式が使えるか(一度失敗したら、そのスレッドでは前の方式に戻す)</summary>
        public static bool Enabled => !broken;

        public static ID2D1Factory? FactoryOf(ID2D1DeviceContext dc)
        {
            if (broken) return null;
            try
            {
                var owner = dc.NativePointer;
                if (factory == null || factoryOwner != owner)
                {
                    try { roundStroke?.Dispose(); } catch { }
                    try { factory?.Dispose(); } catch { }
                    roundStroke = null;
                    factory = dc.Factory;
                    factoryOwner = owner;
                }
                return factory;
            }
            catch { Fail(); return null; }
        }

        static ID2D1StrokeStyle? RoundStroke(ID2D1DeviceContext dc)
        {
            var f = FactoryOf(dc);
            if (f == null) return null;
            if (roundStroke != null) return roundStroke;
            try
            {
                roundStroke = f.CreateStrokeStyle(new StrokeStyleProperties
                {
                    StartCap = CapStyle.Round,
                    EndCap = CapStyle.Round,
                    DashCap = CapStyle.Round,
                    LineJoin = LineJoin.Round,
                    MiterLimit = 10f,
                });
            }
            catch { roundStroke = null; }
            return roundStroke;
        }

        /// <summary>点の並び(閉じた形)をいくつか入れたパスを作る。作れなければ null。使い終わったら Dispose する</summary>
        public static ID2D1PathGeometry? Build(ID2D1DeviceContext dc, IEnumerable<IReadOnlyList<Vector2>> figures, bool winding, bool closed = true)
        {
            var f = FactoryOf(dc);
            if (f == null) return null;
            ID2D1PathGeometry? geo = null;
            try
            {
                geo = f.CreatePathGeometry();
                using (var sink = geo.Open())
                {
                    sink.SetFillMode(winding ? FillMode.Winding : FillMode.Alternate);
                    foreach (var fig in figures)
                    {
                        if (fig.Count < 2) continue;
                        sink.BeginFigure(fig[0], closed ? FigureBegin.Filled : FigureBegin.Hollow);
                        for (int i = 1; i < fig.Count; i++) sink.AddLine(fig[i]);
                        sink.EndFigure(closed ? FigureEnd.Closed : FigureEnd.Open);
                    }
                    sink.Close();
                }
                return geo;
            }
            catch
            {
                try { geo?.Dispose(); } catch { }
                Fail();
                return null;
            }
        }

        /// <summary>多角形を塗る(重なりは「交互」の規則。前の行ごとの方式と同じ)</summary>
        public static bool FillPoly(ID2D1DeviceContext dc, ID2D1Brush brush, IList<Vector2> p)
        {
            if (broken) return false;
            using var geo = Build(dc, new[] { (IReadOnlyList<Vector2>)(p as IReadOnlyList<Vector2> ?? p.ToList()) }, false);
            if (geo == null) return false;
            try { dc.FillGeometry(geo, brush); return Ok(); }
            catch { return Fail(); }
        }

        /// <summary>いくつかの形をまとめて塗る(穴あきなど。交互の規則)</summary>
        public static bool FillFigures(ID2D1DeviceContext dc, ID2D1Brush brush, IEnumerable<IReadOnlyList<Vector2>> figures, bool winding = false)
        {
            if (broken) return false;
            using var geo = Build(dc, figures, winding);
            if (geo == null) return false;
            try { dc.FillGeometry(geo, brush); return Ok(); }
            catch { return Fail(); }
        }

        /// <summary>折れ線を、角と端を丸くして1回で描く</summary>
        public static bool StrokePath(ID2D1DeviceContext dc, ID2D1Brush brush, IReadOnlyList<Vector2> pts, float width)
        {
            if (broken || pts.Count < 2) return false;
            var style = RoundStroke(dc);
            if (style == null) return false;
            using var geo = Build(dc, new[] { pts }, false, closed: false);
            if (geo == null) return false;
            try { dc.DrawGeometry(geo, brush, width, style); return Ok(); }
            catch { return Fail(); }
        }

        /// <summary>作っておいたパスを、倍率と位置を付けて塗る</summary>
        public static bool FillCached(ID2D1DeviceContext dc, ID2D1Brush brush, ID2D1Geometry geo, Vector2 topLeft, float scale)
        {
            var prev = dc.Transform;
            try
            {
                dc.Transform = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(topLeft) * prev;
                dc.FillGeometry(geo, brush);
                return Ok();
            }
            catch { return Fail(); }
            finally { dc.Transform = prev; }
        }

        /// <summary>作っておいたパスの輪郭を、倍率と位置を付けてなぞる(太さは画面上の太さ)</summary>
        public static bool StrokeCached(ID2D1DeviceContext dc, ID2D1Brush brush, ID2D1Geometry geo, Vector2 topLeft, float scale, float width)
        {
            var style = RoundStroke(dc);
            if (style == null || scale <= 0.0001f) return false;
            var prev = dc.Transform;
            try
            {
                dc.Transform = Matrix3x2.CreateScale(scale) * Matrix3x2.CreateTranslation(topLeft) * prev;
                dc.DrawGeometry(geo, brush, width / scale, style);
                return Ok();
            }
            catch { return Fail(); }
            finally { dc.Transform = prev; }
        }

        /// <summary>
        /// ぼんやりした光の円。色ごとのグラデーションの色の並び(作るのが重い部分)を作っておいて使い回す
        /// (毎回作ると、光が多い図形で重くなるため)
        /// </summary>
        public static bool Glow(ID2D1DeviceContext dc, Vector2 c, float r, Color4 col)
        {
            if (glowBroken) return false;
            try
            {
                var owner = dc.NativePointer;
                if (glowStops == null || glowOwner != owner || glowStops.Count > 400)
                {
                    if (glowStops != null) foreach (var gs in glowStops.Values) try { gs.Dispose(); } catch { }
                    glowStops = new();
                    glowOwner = owner;
                }
                uint key = (uint)(Math.Clamp(col.R, 0f, 1f) * 255f) << 24 | (uint)(Math.Clamp(col.G, 0f, 1f) * 255f) << 16
                         | (uint)(Math.Clamp(col.B, 0f, 1f) * 255f) << 8 | (uint)(Math.Clamp(col.A, 0f, 1f) * 255f);
                if (!glowStops.TryGetValue(key, out var stops))
                {
                    stops = dc.CreateGradientStopCollection(new[]
                    {
                        new GradientStop { Position = 0f, Color = col },
                        new GradientStop { Position = 0.35f, Color = new Color4(col.R, col.G, col.B, col.A * 0.45f) },
                        new GradientStop { Position = 1f, Color = new Color4(col.R, col.G, col.B, 0f) },
                    }, Gamma.StandardRgb, ExtendMode.Clamp);
                    glowStops[key] = stops;
                }
                using var brush = dc.CreateRadialGradientBrush(new RadialGradientBrushProperties
                {
                    Center = c,
                    GradientOriginOffset = Vector2.Zero,
                    RadiusX = r,
                    RadiusY = r,
                }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, stops);
                dc.FillEllipse(new Ellipse(c, r, r), brush);
                return true;
            }
            catch { glowBroken = true; return false; }
        }
    }
}
