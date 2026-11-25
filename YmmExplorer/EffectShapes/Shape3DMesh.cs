using System.Numerics;
using SharpGen.Runtime;
using Vortice;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.Mathematics;

namespace YMM4EffectShapes;

/// <summary>3Dにする前の、部品1つ分(塗り・線・文字の1回の描画)</summary>
internal sealed class Prim3D
{
    public Vector2[] Tris = [];                 // 前の面の三角形(画面の座標)
    public List<Vector2[]> Outlines = [];       // 側面を作る輪郭(閉じた折れ線)
    public Color4 Color;                        // 単色の時の色(まっすぐなアルファ)
    public Grad3D? Grad;                        // グラデーションの時の色の求め方
    public bool Add;                            // 加算合成
    public RawRectF Bounds;
    public float Opacity = 1f;
    public Color4 ColorAt(Vector2 p)
    {
        var c = Grad?.At(p) ?? Color;
        return new Color4(c.R, c.G, c.B, c.A * Opacity);
    }
}

/// <summary>グラデーションの色を、画面の座標から求める</summary>
internal sealed class Grad3D
{
    public bool Radial;
    public Vector2 A, B;            // 線形: 始点・終点 / 円形: 中心・半径
    public Matrix3x2 Inv;           // 画面の座標 → ブラシの座標
    public GradientStop[] Stops = [];
    public float Opacity = 1f;

    public Color4 At(Vector2 world)
    {
        var p = Vector2.Transform(world, Inv);
        float t;
        if (Radial)
        {
            var d = p - A;
            float rx = MathF.Max(MathF.Abs(B.X), 1e-4f), ry = MathF.Max(MathF.Abs(B.Y), 1e-4f);
            t = MathF.Sqrt(d.X * d.X / (rx * rx) + d.Y * d.Y / (ry * ry));
        }
        else
        {
            var e = B - A; float l2 = e.LengthSquared();
            t = l2 < 1e-6f ? 0f : Vector2.Dot(p - A, e) / l2;
        }
        t = Math.Clamp(t, 0f, 1f);
        if (Stops.Length == 0) return new Color4(1, 1, 1, Opacity);
        if (t <= Stops[0].Position) return Mul(Stops[0].Color);
        for (int i = 1; i < Stops.Length; i++)
        {
            if (t <= Stops[i].Position)
            {
                var a = Stops[i - 1]; var b = Stops[i];
                float k = (t - a.Position) / MathF.Max(b.Position - a.Position, 1e-6f);
                return Mul(new Color4(a.Color.R + (b.Color.R - a.Color.R) * k, a.Color.G + (b.Color.G - a.Color.G) * k,
                    a.Color.B + (b.Color.B - a.Color.B) * k, a.Color.A + (b.Color.A - a.Color.A) * k));
            }
        }
        return Mul(Stops[^1].Color);
    }
    Color4 Mul(Color4 c) => new(c.R, c.G, c.B, c.A * Opacity);
}

/// <summary>
/// D2D の描画命令を1つずつ受け取り、画面座標の「形」(三角形と輪郭)に直して集める。
/// 塗り=形そのもの、線=太さを付けた形、文字=文字の輪郭の形 にする
/// </summary>
internal sealed class PrimCollector : CallbackBase, ID2D1CommandSink
{
    const float Tol = 0.35f;
    public const int MaxPrims = 40000;
    readonly ID2D1Factory factory;
    readonly float canvasW, canvasH;
    Matrix3x2 transform = Matrix3x2.Identity;
    bool add;
    readonly Stack<ID2D1Geometry?> clips = new();
    readonly Stack<float> opacities = new();
    float opacity = 1f;
    public readonly List<Prim3D> Prims = [];
    public int Skipped;

    public PrimCollector(ID2D1Factory factory, float w, float h) { this.factory = factory; canvasW = w; canvasH = h; }

    public void BeginDraw() { }
    public void EndDraw() { }
    public void SetAntialiasMode(AntialiasMode antialiasMode) { }
    public void SetTags(ulong tag1, ulong tag2) { }
    public void SetTextAntialiasMode(Vortice.Direct2D1.TextAntialiasMode textAntialiasMode) { }
    public void SetTextRenderingParams(IDWriteRenderingParams textRenderingParams) { }
    public void SetTransform(ref Matrix3x2 transform) => this.transform = transform;
    public void SetPrimitiveBlend(PrimitiveBlend primitiveBlend) => add = primitiveBlend == PrimitiveBlend.Add;
    public void SetUnitMode(UnitMode unitMode) { }

    public void Clear(Color4? color)
    {
        if (color is not { } c || c.A <= 0.001f) return;
        using var g = factory.CreateRectangleGeometry(new RawRectF(0, 0, canvasW, canvasH));
        AddPrim(g, Matrix3x2.Identity, null, 0, null, c);
    }

    public void DrawGlyphRun(Vector2 baselineOrigin, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, ID2D1Brush brush, Vortice.DCommon.MeasuringMode measuringMode)
    {
        if (glyphRun?.FontFace is null || glyphRun.Indices is null || glyphRun.Indices.Length == 0) return;
        using var path = factory.CreatePathGeometry();
        using (var sink = path.Open())
        {
            glyphRun.FontFace.GetGlyphRunOutline(glyphRun.FontEmSize, glyphRun.Indices, glyphRun.Advances, glyphRun.Offsets,
                glyphRun.IsSideways, (glyphRun.BidiLevel & 1) != 0, sink);
            sink.Close();
        }
        AddPrim(path, Matrix3x2.CreateTranslation(baselineOrigin) * transform, brush, 0, null, null);
    }

    public void DrawLine(Vector2 point0, Vector2 point1, ID2D1Brush brush, float strokeWidth, ID2D1StrokeStyle strokeStyle)
    {
        using var path = factory.CreatePathGeometry();
        using (var sink = path.Open())
        {
            sink.BeginFigure(point0, FigureBegin.Hollow);
            sink.AddLine(point1);
            sink.EndFigure(FigureEnd.Open);
            sink.Close();
        }
        AddPrim(path, transform, brush, strokeWidth, strokeStyle, null);
    }

    public void DrawGeometry(ID2D1Geometry geometry, ID2D1Brush brush, float strokeWidth, ID2D1StrokeStyle strokeStyle)
        => AddPrim(geometry, transform, brush, strokeWidth, strokeStyle, null);

    public void DrawRectangle(RawRectF rect, ID2D1Brush brush, float strokeWidth, ID2D1StrokeStyle strokeStyle)
    {
        using var g = factory.CreateRectangleGeometry(rect);
        AddPrim(g, transform, brush, strokeWidth, strokeStyle, null);
    }

    public void FillGeometry(ID2D1Geometry geometry, ID2D1Brush brush, ID2D1Brush opacityBrush)
        => AddPrim(geometry, transform, brush, 0, null, null);

    public void FillRectangle(RawRectF rect, ID2D1Brush brush)
    {
        using var g = factory.CreateRectangleGeometry(rect);
        AddPrim(g, transform, brush, 0, null, null);
    }

    // 画像・メッシュ・メタファイルは演出図形では使わないので、何もしない
    public void DrawBitmap(ID2D1Bitmap bitmap, RawRectF? destinationRectangle, float opacity, InterpolationMode interpolationMode, RawRectF? sourceRectangle, Matrix4x4? perspectiveTransform) { Skipped++; }
    public void DrawImage(ID2D1Image image, Vector2? targetOffset, RawRectF? imageRectangle, InterpolationMode interpolationMode, CompositeMode compositeMode) { Skipped++; }
    public void DrawGdiMetafile(ID2D1GdiMetafile gdiMetafile, Vector2? targetOffset) { Skipped++; }
    public void FillMesh(ID2D1Mesh mesh, ID2D1Brush brush) { Skipped++; }
    public void FillOpacityMask(ID2D1Bitmap opacityMask, ID2D1Brush brush, RawRectF? destinationRectangle, RawRectF? sourceRectangle) { Skipped++; }

    public void PushAxisAlignedClip(RawRectF clipRect, AntialiasMode antialiasMode)
    {
        // 切り抜きは画面座標の形にして持っておき、描く形と重なる所だけ残す
        try
        {
            using var r = factory.CreateRectangleGeometry(clipRect);
            var world = WorldPath(r, transform, 0, null);
            var prev = clips.Count > 0 ? clips.Peek() : null;
            if (prev is not null && world is not null)
            {
                var both = Combine(world, prev);
                world.Dispose();
                world = both;
            }
            clips.Push(world);
        }
        catch { clips.Push(clips.Count > 0 ? null : null); }
    }

    public void PopAxisAlignedClip()
    {
        if (clips.Count > 0) clips.Pop()?.Dispose();
    }

    public void PushLayer(ref LayerParameters1 layerParameters1, ID2D1Layer layer)
    {
        opacities.Push(opacity);
        opacity *= Math.Clamp(layerParameters1.Opacity, 0f, 1f);
    }

    public void PopLayer()
    {
        if (opacities.Count > 0) opacity = opacities.Pop();
    }

    ID2D1PathGeometry? WorldPath(ID2D1Geometry g, Matrix3x2 m, float strokeWidth, ID2D1StrokeStyle? style)
    {
        var path = factory.CreatePathGeometry();
        try
        {
            using var sink = path.Open();
            if (strokeWidth > 0)
            {
                // 線: 拡大縮小を含む変換では、線の太さも同じだけ太くなる
                float scale = MathF.Sqrt(MathF.Abs(m.M11 * m.M22 - m.M12 * m.M21));
                sink.SetFillMode(FillMode.Winding);
                g.Widen(strokeWidth * MathF.Max(scale, 1e-3f), style, m, Tol, sink);
            }
            else
            {
                g.Simplify(GeometrySimplificationOption.CubicsAndLines, m, Tol, sink);
            }
            sink.Close();
            return path;
        }
        catch
        {
            path.Dispose();
            return null;
        }
    }

    ID2D1PathGeometry Combine(ID2D1Geometry a, ID2D1Geometry clip)
    {
        var path = factory.CreatePathGeometry();
        using var sink = path.Open();
        a.CombineWithGeometry(clip, CombineMode.Intersect, null, Tol, sink);
        sink.Close();
        return path;
    }

    void AddPrim(ID2D1Geometry g, Matrix3x2 m, ID2D1Brush? brush, float strokeWidth, ID2D1StrokeStyle? style, Color4? fixedColor)
    {
        if (Prims.Count >= MaxPrims) { Skipped++; return; }
        try
        {
            var p = new Prim3D { Add = add, Opacity = opacity };
            if (fixedColor is { } fc) p.Color = fc;
            else if (!ReadBrush(brush, m, p)) return;
            if (p.Grad is null && p.Color.A * p.Opacity <= 0.002f) return;

            var world = WorldPath(g, m, strokeWidth, style);
            if (world is null) return;
            var clip = clips.Count > 0 ? clips.Peek() : null;
            if (clip is not null)
            {
                var cut = Combine(world, clip);
                world.Dispose();
                world = cut;
            }
            using (world)
            {
                var tess = new TriSink();
                world.Tessellate(Tol, tess);
                if (tess.Points.Count < 3) return;
                p.Tris = [.. tess.Points];
                var outl = new LineSink();
                world.Simplify(GeometrySimplificationOption.Lines, Tol, outl);
                p.Outlines = outl.Figures;
                float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
                foreach (var v in p.Tris) { x0 = MathF.Min(x0, v.X); y0 = MathF.Min(y0, v.Y); x1 = MathF.Max(x1, v.X); y1 = MathF.Max(y1, v.Y); }
                p.Bounds = new RawRectF(x0, y0, x1, y1);
                tess.Dispose(); outl.Dispose();
            }
            Prims.Add(p);
        }
        catch
        {
            Skipped++;
        }
    }

    /// <summary>ブラシから色(またはグラデーション)を読む。読めない種類は白にする</summary>
    static bool ReadBrush(ID2D1Brush? brush, Matrix3x2 world, Prim3D p)
    {
        if (brush is null) return false;
        float op = brush.Opacity;
        using (var solid = brush.QueryInterfaceOrNull<ID2D1SolidColorBrush>())
        {
            if (solid is not null)
            {
                var c = solid.Color;
                p.Color = new Color4(c.R, c.G, c.B, c.A * op);
                return true;
            }
        }
        var brushToWorld = brush.Transform * world;
        if (!Matrix3x2.Invert(brushToWorld, out var inv)) inv = Matrix3x2.Identity;
        using (var lin = brush.QueryInterfaceOrNull<ID2D1LinearGradientBrush>())
        {
            if (lin is not null)
            {
                p.Grad = new Grad3D { A = lin.StartPoint, B = lin.EndPoint, Inv = inv, Stops = Stops(lin.GradientStopCollection), Opacity = op };
                p.Color = p.Grad.At(new Vector2(p.Bounds.Left, p.Bounds.Top));
                return true;
            }
        }
        using (var rad = brush.QueryInterfaceOrNull<ID2D1RadialGradientBrush>())
        {
            if (rad is not null)
            {
                p.Grad = new Grad3D { Radial = true, A = rad.Center + rad.GradientOriginOffset * 0.5f, B = new Vector2(rad.RadiusX, rad.RadiusY), Inv = inv, Stops = Stops(rad.GradientStopCollection), Opacity = op };
                return true;
            }
        }
        p.Color = new Color4(1, 1, 1, op);
        return true;
    }

    static GradientStop[] Stops(ID2D1GradientStopCollection? col)
    {
        if (col is null) return [];
        using (col)
        {
            int n = col.GradientStopCount;
            var arr = new GradientStop[n];
            if (n > 0) col.GetGradientStops(arr, n);
            Array.Sort(arr, (a, b) => a.Position.CompareTo(b.Position));
            return arr;
        }
    }
}

/// <summary>塗りの三角形を受け取る</summary>
internal sealed class TriSink : CallbackBase, ID2D1TessellationSink
{
    public readonly List<Vector2> Points = [];
    public void AddTriangles(Triangle[] triangles)
    {
        foreach (var t in triangles) { Points.Add(t.Point1); Points.Add(t.Point2); Points.Add(t.Point3); }
    }
    public void Close() { }
}

/// <summary>輪郭の折れ線を受け取る</summary>
internal sealed class LineSink : CallbackBase, ID2D1SimplifiedGeometrySink
{
    public readonly List<Vector2[]> Figures = [];
    readonly List<Vector2> cur = [];
    public void SetFillMode(FillMode fillMode) { }
    public void SetSegmentFlags(PathSegment vertexFlags) { }
    public void BeginFigure(Vector2 startPoint, FigureBegin figureBegin) { cur.Clear(); cur.Add(startPoint); }
    public void AddLines(Vector2[] ps) { cur.AddRange(ps); }
    public void AddBeziers(BezierSegment[] beziers)
    {
        // Lines を指定しているので来ないはずだが、来たら端点だけつなぐ
        foreach (var b in beziers) cur.Add(b.Point3);
    }
    public void EndFigure(FigureEnd figureEnd)
    {
        if (cur.Count >= 2)
        {
            if (Vector2.DistanceSquared(cur[0], cur[^1]) < 1e-6f) cur.RemoveAt(cur.Count - 1);
            if (cur.Count >= 2) Figures.Add([.. cur]);
        }
        cur.Clear();
    }
    public void Close() { }
}
