using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.D3DCompiler;
using Vortice.Direct2D1;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using D2DPixelFormat = Vortice.DCommon.PixelFormat;
using D2DAlphaMode = Vortice.DCommon.AlphaMode;
using InputElementDescription = Vortice.Direct3D11.InputElementDescription;
using BlendDescription = Vortice.Direct3D11.BlendDescription;
using Blend = Vortice.Direct3D11.Blend;
using BlendOperation = Vortice.Direct3D11.BlendOperation;
using FillMode = Vortice.Direct3D11.FillMode;

namespace YMM4EffectShapes;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct Vert3D
{
    public Vector3 Pos;
    public Vector3 Normal;
    public Vector4 Color;
    public Vert3D(Vector3 p, Vector3 n, Vector4 c) { Pos = p; Normal = n; Color = c; }
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct Consts3D
{
    public Matrix4x4 Model;      // 部品の空間 → 世界
    public Matrix4x4 ViewProj;   // 世界 → 画面
    public Vector4 Light;        // xyz=光の向き(世界), w=正面の明るさ
    public Vector4 Params;       // x=光の強さ, y=側面の明るさ, z=つや, w=側面の色を使う
    public Vector4 SideColor;
    public Vector4 Camera;       // xyz=カメラの位置(世界)
    public Vector4 Fog;          // x=かすみ始める距離, y=いちばん薄くなる距離, z=強さ
}

/// <summary>
/// 演出図形を本物の3Dで描く。図形の描画命令を部品ごとの立体(前の面・後ろの面・側面)にし、
/// 深度バッファーつきの Direct3D11 で描いて、D2D の画像として返す
/// </summary>
internal sealed class Shape3DRenderer : IDisposable
{
    const string Hlsl = """
cbuffer C : register(b0) { row_major float4x4 Model; row_major float4x4 VP; float4 L; float4 P; float4 SC; float4 Cam; float4 F; };
struct VI { float3 pos : POSITION; float3 nrm : NORMAL; float4 col : COLOR; };
struct PI { float4 pos : SV_POSITION; float3 nrm : NORMAL; float3 mn : TEXCOORD0; float4 col : COLOR; float3 wp : TEXCOORD1; };
PI VS(VI i)
{
    PI o;
    float4 w = mul(float4(i.pos, 1), Model);
    o.pos = mul(w, VP);
    o.nrm = mul(float4(i.nrm, 0), Model).xyz;
    o.mn = i.nrm;
    o.col = i.col;
    o.wp = w.xyz;
    return o;
}
float4 PS(PI i) : SV_Target
{
    float4 c = i.col;
    float3 n = normalize(i.nrm);
    float3 v = normalize(Cam.xyz - i.wp);
    if (dot(n, v) < 0) n = -n;
    float side = 1 - abs(normalize(i.mn).z);
    float3 base = lerp(c.rgb, SC.rgb, side * P.w);
    float d = max(dot(n, L.xyz), 0);
    float lit = lerp(1, saturate(d / max(L.w, 0.05)) * 1.0 + 0.12, P.x);
    lit *= lerp(1, P.y, side);
    float3 h = normalize(L.xyz + v);
    float spec = pow(max(dot(n, h), 0), 40) * P.z * c.a;
    float3 rgb = saturate(base * lit + spec);
    float a = c.a * (1 - saturate((length(Cam.xyz - i.wp) - F.x) / max(F.y - F.x, 1)) * F.z) * F.w;
    return float4(rgb * a, a);
}
""";

    sealed class Pipeline : IDisposable
    {
        public ID3D11VertexShader Vs = null!;
        public ID3D11PixelShader Ps = null!;
        public ID3D11InputLayout Layout = null!;
        public ID3D11Buffer Cb = null!;
        public ID3D11BlendState BlendNormal = null!, BlendAdd = null!;
        public ID3D11DepthStencilState DepthWrite = null!, DepthTest = null!;
        public ID3D11RasterizerState Raster = null!;
        public ID3DDeviceContextState? State;
        public void Dispose()
        {
            Vs?.Dispose(); Ps?.Dispose(); Layout?.Dispose(); Cb?.Dispose(); BlendNormal?.Dispose(); BlendAdd?.Dispose();
            DepthWrite?.Dispose(); DepthTest?.Dispose(); Raster?.Dispose(); State?.Dispose();
        }
    }

    static readonly object gate = new();
    static readonly Dictionary<IntPtr, Pipeline> pipelines = [];
    static string? compileError;

    int width, height, samples;
    ID3D11Texture2D? msaaTex, depthTex, resolveTex;
    ID3D11RenderTargetView? rtv;
    ID3D11DepthStencilView? dsv;
    ID2D1Bitmap1? output;
    ID3D11Buffer? vb;
    int vbCapacity;
    readonly List<Vert3D> verts = [];

    public static string? LastError { get; private set; }

    /// <summary>3Dで描けなかった理由を残す(YmmExplorer の表示用)</summary>
    public static void Report(Exception e)
    {
        LastError = e.GetType().Name + ": " + e.Message + "\n" + e.StackTrace;
        System.Diagnostics.Debug.WriteLine("[EffectShape3D] " + e);
    }

    static Pipeline GetPipeline(ID3D11Device device)
    {
        lock (gate)
        {
            if (pipelines.TryGetValue(device.NativePointer, out var p)) return p;
            if (compileError is not null) throw new InvalidOperationException(compileError);
            p = new Pipeline();
            try
            {
                using var vsb = Compiler.Compile(Hlsl, "VS", "shape3d.hlsl", "vs_4_0", ShaderFlags.OptimizationLevel3, EffectFlags.None);
                using var psb = Compiler.Compile(Hlsl, "PS", "shape3d.hlsl", "ps_4_0", ShaderFlags.OptimizationLevel3, EffectFlags.None);
                var vsBytes = vsb.AsBytes();
                p.Vs = device.CreateVertexShader(vsBytes);
                p.Ps = device.CreatePixelShader(psb.AsBytes());
                p.Layout = device.CreateInputLayout(
                [
                    new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                    new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
                    new InputElementDescription("COLOR", 0, Format.R32G32B32A32_Float, 24, 0),
                ], vsBytes);
            }
            catch (Exception e)
            {
                compileError = "3Dの準備(シェーダー)に失敗: " + e.Message;
                p.Dispose();
                throw;
            }
            p.Cb = device.CreateBuffer(new BufferDescription((Marshal.SizeOf<Consts3D>() + 15) / 16 * 16, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write));
            var bn = new BlendDescription();
            bn.RenderTarget[0] = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = Blend.One, DestinationBlend = Blend.InverseSourceAlpha, BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = Blend.One, DestinationBlendAlpha = Blend.InverseSourceAlpha, BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            p.BlendNormal = device.CreateBlendState(bn);
            var ba = new BlendDescription();
            ba.RenderTarget[0] = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = Blend.One, DestinationBlend = Blend.One, BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = Blend.Zero, DestinationBlendAlpha = Blend.One, BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteEnable.All,
            };
            p.BlendAdd = device.CreateBlendState(ba);
            p.DepthWrite = device.CreateDepthStencilState(new DepthStencilDescription(true, DepthWriteMask.All, ComparisonFunction.LessEqual));
            p.DepthTest = device.CreateDepthStencilState(new DepthStencilDescription(true, DepthWriteMask.Zero, ComparisonFunction.LessEqual));
            p.Raster = device.CreateRasterizerState(new RasterizerDescription(CullMode.None, FillMode.Solid) { MultisampleEnable = true, DepthClipEnable = true, AntialiasedLineEnable = false });
            try
            {
                // YMM4 の描画の状態をこわさないよう、3D用の状態に切り替えて描く
                using var d1 = device.QueryInterfaceOrNull<ID3D11Device1>();
                if (d1 is not null)
                    p.State = d1.CreateDeviceContextState<ID3D11Device>(CreateDeviceContextStateFlags.None, [device.FeatureLevel], out _);
            }
            catch { p.State = null; }
            pipelines[device.NativePointer] = p;
            return p;
        }
    }

    void EnsureTargets(IGraphicsDevicesAndContext devices, int w, int h)
    {
        var device = devices.D3D.Device;
        if (w == width && h == height && output is not null) return;
        ReleaseTargets();
        width = w; height = h;
        samples = 1;
        foreach (var s in new[] { 8, 4, 2 })
        {
            if (device.CheckMultisampleQualityLevels(Format.B8G8R8A8_UNorm, s) > 0 && device.CheckMultisampleQualityLevels(Format.D24_UNorm_S8_UInt, s) > 0) { samples = s; break; }
        }
        msaaTex = device.CreateTexture2D(new Texture2DDescription(Format.B8G8R8A8_UNorm, w, h, 1, 1, BindFlags.RenderTarget, ResourceUsage.Default, CpuAccessFlags.None, samples, 0));
        rtv = device.CreateRenderTargetView(msaaTex);
        depthTex = device.CreateTexture2D(new Texture2DDescription(Format.D24_UNorm_S8_UInt, w, h, 1, 1, BindFlags.DepthStencil, ResourceUsage.Default, CpuAccessFlags.None, samples, 0));
        dsv = device.CreateDepthStencilView(depthTex);
        resolveTex = device.CreateTexture2D(new Texture2DDescription(Format.B8G8R8A8_UNorm, w, h, 1, 1, BindFlags.RenderTarget | BindFlags.ShaderResource));
        using var surface = resolveTex.QueryInterface<IDXGISurface>();
        output = devices.DeviceContext.CreateBitmapFromDxgiSurface(surface, new BitmapProperties1(new D2DPixelFormat(Format.B8G8R8A8_UNorm, D2DAlphaMode.Premultiplied), 96, 96, BitmapOptions.None));
    }

    void ReleaseTargets()
    {
        output?.Dispose(); output = null;
        rtv?.Dispose(); rtv = null;
        dsv?.Dispose(); dsv = null;
        msaaTex?.Dispose(); msaaTex = null;
        depthTex?.Dispose(); depthTex = null;
        resolveTex?.Dispose(); resolveTex = null;
    }

    /// <summary>描画命令の入った commandList を3Dにして描く。戻り値は D2D で貼れる画像</summary>
    public ID2D1Image Render(IGraphicsDevicesAndContext devices, ID2D1CommandList commandList, in DrawContext ctx, in Shape3DSettings s)
    {
        int w = Math.Clamp((int)MathF.Ceiling(ctx.Width), 1, 8192), h = Math.Clamp((int)MathF.Ceiling(ctx.Height), 1, 8192);
        var dc = devices.DeviceContext;

        // 1) 描画命令を部品の形に直す
        using var factory = dc.Factory;
        var col = new PrimCollector(factory, ctx.Width, ctx.Height);
        commandList.Stream(col);
        var prims = col.Prims;
        col.Dispose();

        // 2) 立体の頂点を作る
        float fov = Math.Clamp(s.Fov, 1f, 170f) * MathF.PI / 180f;
        float camDist = ctx.Height / 2f / MathF.Tan(fov / 2f);
        var batches = BuildMesh(prims, ctx, s, camDist, out var model, out var camPos, out var viewProj, out var fog);
        return RenderCore(devices, ctx, batches, model, camPos, viewProj, fog, s, false);
    }

    /// <summary>本物の3Dの形(Native3D)を、ふだんの画面用に描く</summary>
    public ID2D1Image RenderNative(IGraphicsDevicesAndContext devices, Native3D n, in DrawContext ctx, in Shape3DSettings s)
    {
        var batches = BuildNativeMesh(n, ctx, s, out var model, out var camPos, out var viewProj, null, default);
        return RenderCore(devices, ctx, batches, model, camPos, viewProj, new Vector4(1, 2, 0, 1), s, !n.Lit);
    }

    /// <summary>頂点をD3Dで描いて、D2Dで貼れる画像にする。baked=色に光がもう入っている(光の計算をしない)</summary>
    ID2D1Image RenderCore(IGraphicsDevicesAndContext devices, in DrawContext ctx, List<Batch> batches, Matrix4x4 model, Vector3 camPos, Matrix4x4 viewProj, Vector4 fog, in Shape3DSettings s, bool baked)
    {
        int w = Math.Clamp((int)MathF.Ceiling(ctx.Width), 1, 8192), h = Math.Clamp((int)MathF.Ceiling(ctx.Height), 1, 8192);
        var device = devices.D3D.Device;
        var d3d = devices.D3D.DeviceContext;
        var pipe = GetPipeline(device);
        var mt = devices.D3D.Multithread;
        mt?.Enter();
        ID3DDeviceContextState? prev = null;
        ID3D11DeviceContext1? d3d1 = null;
        try
        {
            EnsureTargets(devices, w, h);
            if (pipe.State is not null)
            {
                d3d1 = d3d.QueryInterfaceOrNull<ID3D11DeviceContext1>();
                if (d3d1 is not null) prev = d3d1.SwapDeviceContextState(pipe.State);
            }
            var oldRtv = new ID3D11RenderTargetView[1];
            d3d.OMGetRenderTargets(1, oldRtv, out var oldDsv);
            int vpCount = d3d.RSGetViewports(); var oldVp = new Viewport[Math.Max(vpCount, 0)]; if (vpCount > 0) d3d.RSGetViewports(oldVp);
            try
            {
                d3d.ClearRenderTargetView(rtv!, new Color4(0, 0, 0, 0));
                d3d.ClearDepthStencilView(dsv!, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1f, 0);
                if (verts.Count > 0)
                {
                    UploadVertices(device, d3d);
                    float lyaw = s.LightYaw * MathF.PI / 180f, lpitch = s.LightPitch * MathF.PI / 180f;
                    // 光は、カメラ側(手前 = -Z)から、左右・上下に傾けた向きから来る
                    var light = Vector3.Normalize(new Vector3(MathF.Sin(lyaw) * MathF.Cos(lpitch), MathF.Sin(lpitch), -MathF.Cos(lyaw) * MathF.Cos(lpitch)));
                    var sc = s.SideColor;
                    var cb = new Consts3D
                    {
                        Model = model,
                        ViewProj = viewProj,
                        Light = new Vector4(light, MathF.Max(-light.Z, 0.05f)),
                        Params = baked ? new Vector4(0, 1, 0, 0) : new Vector4(Math.Clamp(s.Light, 0f, 2f), lastLitModel ? 1f : MathF.Max(s.SideBright, 0f), MathF.Max(s.Gloss, 0f), !lastLitModel && sc is not null ? 1f : 0f),
                        SideColor = !baked && !lastLitModel && sc is { } c ? new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, 1f) : Vector4.One,
                        Camera = new Vector4(camPos, 1),
                        Fog = fog,
                    };
                    var map = d3d.Map(pipe.Cb, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
                    Marshal.StructureToPtr(cb, map.DataPointer, false);
                    d3d.Unmap(pipe.Cb, 0);

                    d3d.OMSetRenderTargets(rtv!, dsv);
                    d3d.RSSetViewport(new Viewport(0, 0, w, h, 0, 1));
                    d3d.RSSetState(pipe.Raster);
                    d3d.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
                    d3d.IASetInputLayout(pipe.Layout);
                    d3d.IASetVertexBuffer(0, vb!, Marshal.SizeOf<Vert3D>(), 0);
                    d3d.VSSetShader(pipe.Vs);
                    d3d.VSSetConstantBuffer(0, pipe.Cb);
                    d3d.PSSetShader(pipe.Ps);
                    d3d.PSSetConstantBuffer(0, pipe.Cb);
                    foreach (var b in batches)
                    {
                        d3d.OMSetBlendState(b.Add ? pipe.BlendAdd : pipe.BlendNormal, null, -1);
                        d3d.OMSetDepthStencilState(b.Write ? pipe.DepthWrite : pipe.DepthTest, 0);
                        d3d.Draw(b.Count, b.Start);
                    }
                }
                d3d.ResolveSubresource(resolveTex!, 0, msaaTex!, 0, Format.B8G8R8A8_UNorm);
            }
            finally
            {
                d3d.OMSetRenderTargets(oldRtv[0]!, oldDsv);
                if (oldVp.Length > 0) d3d.RSSetViewports(oldVp);
                oldRtv[0]?.Dispose(); oldDsv?.Dispose();
            }
        }
        finally
        {
            if (d3d1 is not null && prev is not null) { d3d1.SwapDeviceContextState(prev); prev.Dispose(); }
            d3d1?.Dispose();
            mt?.Leave();
        }
        return output!;
    }

    // ------------------------------------------------------------ 3Dプレビュー(YMM43D)の中に描く
    /// <summary>3Dプレビュー(YMM43D)の世界は 1単位 = 100px</summary>
    public const float PxToWorld = 0.01f;

    /// <summary>
    /// 3Dプレビューの中での立体の置き方: 回転の中心(pivotZ)を原点に置き、図形の「3D」の回転(自動回転も含む)で回して、
    /// 右手系(手前が +Z)・100px=1単位に直す。カメラは3Dプレビュー(YMM43D)のものを使う
    /// </summary>
    static Matrix4x4 ExternalModel(in Shape3DSettings s, float pivotZ)
    {
        float rx = s.RotX * MathF.PI / 180f, ry = s.RotY * MathF.PI / 180f, rz = s.RotZ * MathF.PI / 180f;
        return Matrix4x4.CreateTranslation(0, 0, -pivotZ)
            * Matrix4x4.CreateRotationZ(-rz) * Matrix4x4.CreateRotationX(rx) * Matrix4x4.CreateRotationY(-ry)
            * Matrix4x4.CreateScale(PxToWorld, PxToWorld, -PxToWorld);
    }

    List<Prim3D>? lastPrims;
    Native3D? lastNative;
    /// <summary>今描いているのが専用3Dモデル(面の向きで光を当て、側面を暗くしない)か</summary>
    bool lastLitModel;
    DrawContext lastCtx;
    long lastFrame = long.MinValue;
    IntPtr extDevice;
    ID3D11Buffer? extVb;
    int extCapacity;

    /// <summary>描画命令を部品の形に直して、覚えておく(3Dプレビューから描かれる時に使う)</summary>
    public void Collect(IGraphicsDevicesAndContext devices, ID2D1CommandList commandList, in DrawContext ctx)
    {
        using var factory = devices.DeviceContext.Factory;
        var col = new PrimCollector(factory, ctx.Width, ctx.Height);
        commandList.Stream(col);
        lastPrims = col.Prims;
        lastNative = null;
        col.Dispose();
        lastCtx = ctx;
        lastFrame = ctx.Frame;
    }

    /// <summary>本物の3Dの形を、3Dプレビューから描かれる時のために覚えておく</summary>
    public void CollectNative(Native3D n, in DrawContext ctx)
    {
        lastNative = n;
        lastPrims = null;
        lastCtx = ctx;
        lastFrame = ctx.Frame;
    }

    public bool HasFrame(long frame) => (lastPrims is not null || lastNative is not null) && lastFrame == frame;
    public DrawContext LastContext => lastCtx;

    /// <summary>覚えている部品が入る大きさ(3Dプレビューの、見えない物を省く判定に使う)</summary>
    public (Vector3 Min, Vector3 Max) Bounds(in Shape3DSettings s)
    {
        var (lo, hi, pivot) = LocalBox(s);
        var model = ExternalModel(s, pivot);
        if (MathF.Abs(s.SpinX) + MathF.Abs(s.SpinY) + MathF.Abs(s.SpinZ) > 0.001f)
        {
            // 自動で回る時は、どの向きでも入る球の大きさにする
            var c = new Vector3(0, 0, pivot);
            float r = MathF.Max(Vector3.Distance(lo, c), Vector3.Distance(hi, c));
            r = MathF.Max(r, MathF.Max(Vector3.Distance(new Vector3(lo.X, hi.Y, lo.Z), c), Vector3.Distance(new Vector3(hi.X, lo.Y, hi.Z), c))) * PxToWorld;
            return (new Vector3(-r), new Vector3(r));
        }
        var mn = new Vector3(float.MaxValue); var mx = new Vector3(float.MinValue);
        for (int i = 0; i < 8; i++)
        {
            var p = Vector3.Transform(new Vector3((i & 1) == 0 ? lo.X : hi.X, (i & 2) == 0 ? lo.Y : hi.Y, (i & 4) == 0 ? lo.Z : hi.Z), model);
            mn = Vector3.Min(mn, p); mx = Vector3.Max(mx, p);
        }
        return (mn, mx);
    }

    /// <summary>覚えている立体が入る箱(px、回す前)と、回転の中心</summary>
    (Vector3 Lo, Vector3 Hi, float Pivot) LocalBox(in Shape3DSettings s)
    {
        if (lastNative is { } nb)
        {
            if (nb.IsEmpty) return (new Vector3(-1), new Vector3(1), 0f);
            return (nb.Min, nb.Max, 0f);
        }
        float hw = lastCtx.Width / 2f, hh = lastCtx.Height / 2f;
        int n = lastPrims?.Count ?? 0;
        float depth = s.Mode == Depth3DMode.Flat ? 0f : MathF.Max(s.Depth, 0f);
        float layer = MathF.Min(MathF.Max(s.LayerGap, 0f) * Math.Max(n - 1, 0), MathF.Max(s.LayerMax, 0f));
        float zr = MathF.Max(s.ZRange, 0f);
        float fov = Math.Clamp(s.Fov, 1f, 170f) * MathF.PI / 180f;
        float camDist = MathF.Max(lastCtx.Height / 2f / MathF.Tan(fov / 2f), 1f);
        float grow = s.ZFill && zr > 0f ? (camDist + zr / 2f) / camDist : 1f;
        float wave = MathF.Max(s.PartWave, 0f);
        float extra = MathF.Abs(s.PartSpin) > 0.001f ? MathF.Max(hw, hh) * 0.5f : 0f;   // 部品がその場で回る分
        var lo = new Vector3(-hw * grow, -hh * grow, -zr / 2f - wave - extra - 10f);
        var hi = new Vector3(hw * grow, hh * grow, layer + depth + zr / 2f + wave + extra + 10f);
        return (lo, hi, layer / 2f + depth / 2f + s.PivotZ);
    }

    /// <summary>
    /// 3Dプレビュー(YMM43D)が描いている最中の描画先に、立体をそのまま描く。
    /// world=アイテムの置き場所、viewProj=3Dプレビューのカメラ。深度は3Dプレビューのものを使うので、他の3Dの物と正しく前後する
    /// </summary>
    public void DrawExternal(ID3D11Device device, ID3D11DeviceContext d3d, Matrix4x4 world, Matrix4x4 viewProj, Vector3 cam, float opacity, in Shape3DSettings s)
    {
        var ctx = lastCtx;
        List<Batch> batches;
        Matrix4x4 model; Vector3 camPos; Vector4 fog;
        bool baked = false;
        if (lastNative is { } nat)
        {
            batches = BuildNativeMesh(nat, ctx, s, out model, out camPos, out _, world, cam);
            fog = new Vector4(1, 2, 0, 1);
            baked = !nat.Lit;
        }
        else
        {
            if (lastPrims is null || lastPrims.Count == 0) return;
            float fov = Math.Clamp(s.Fov, 1f, 170f) * MathF.PI / 180f;
            float camDist = MathF.Max(ctx.Height / 2f / MathF.Tan(fov / 2f), 1f);
            batches = BuildMesh(lastPrims, ctx, s, camDist, out model, out camPos, out _, out fog, world, cam);
        }
        if (verts.Count == 0) return;
        fog.W = Math.Clamp(opacity, 0f, 1f);
        var pipe = GetPipeline(device);

        // 頂点は、3Dプレビューのデバイス用に別に持つ(YMM4の画面とは、デバイスが違うことがある)
        int stride = Marshal.SizeOf<Vert3D>();
        if (extVb is null || extDevice != device.NativePointer || extCapacity < verts.Count)
        {
            extVb?.Dispose();
            extDevice = device.NativePointer;
            extCapacity = Math.Max(verts.Count * 3 / 2, 4096);
            extVb = device.CreateBuffer(new BufferDescription(extCapacity * stride, BindFlags.VertexBuffer, ResourceUsage.Default, CpuAccessFlags.None));
        }

        // 3Dプレビューの描画先・深度・表示範囲を引き継いで、3D用の状態に切り替える
        var oldRtv = new ID3D11RenderTargetView[1];
        d3d.OMGetRenderTargets(1, oldRtv, out var oldDsv);
        int vpCount = d3d.RSGetViewports(); var oldVp = new Viewport[Math.Max(vpCount, 0)]; if (vpCount > 0) d3d.RSGetViewports(oldVp);
        ID3DDeviceContextState? prev = null;
        using var d3d1 = pipe.State is null ? null : d3d.QueryInterfaceOrNull<ID3D11DeviceContext1>();
        try
        {
            if (d3d1 is not null) prev = d3d1.SwapDeviceContextState(pipe.State!);
            d3d.UpdateSubresource(CollectionsMarshal.AsSpan(verts), extVb, 0, 0, 0, new Vortice.Mathematics.Box(0, 0, 0, verts.Count * stride, 1, 1));
            float lyaw = s.LightYaw * MathF.PI / 180f, lpitch = s.LightPitch * MathF.PI / 180f;
            // 光: 3Dプレビューの空間で、手前(+Z)の左右・上下から来る
            var light = Vector3.Normalize(new Vector3(MathF.Sin(lyaw) * MathF.Cos(lpitch), MathF.Sin(lpitch), MathF.Cos(lyaw) * MathF.Cos(lpitch)));
            var worldModel = model * world;
            var frontN = Vector3.Normalize(Vector3.TransformNormal(new Vector3(0, 0, -1), worldModel));
            var sc = s.SideColor;
            var cb = new Consts3D
            {
                Model = worldModel,
                ViewProj = viewProj,
                Light = new Vector4(light, MathF.Max(MathF.Abs(Vector3.Dot(frontN, light)), 0.2f)),
                Params = baked ? new Vector4(0, 1, 0, 0) : new Vector4(Math.Clamp(s.Light, 0f, 2f), lastLitModel ? 1f : MathF.Max(s.SideBright, 0f), MathF.Max(s.Gloss, 0f), !lastLitModel && sc is not null ? 1f : 0f),
                SideColor = !baked && !lastLitModel && sc is { } c ? new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, 1f) : Vector4.One,
                Camera = new Vector4(camPos, 1),
                Fog = fog,
            };
            var map = d3d.Map(pipe.Cb, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
            Marshal.StructureToPtr(cb, map.DataPointer, false);
            d3d.Unmap(pipe.Cb, 0);
            d3d.OMSetRenderTargets(oldRtv[0]!, oldDsv);
            if (oldVp.Length > 0) d3d.RSSetViewports(oldVp);
            d3d.RSSetState(pipe.Raster);
            d3d.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            d3d.IASetInputLayout(pipe.Layout);
            d3d.IASetVertexBuffer(0, extVb, stride, 0);
            d3d.VSSetShader(pipe.Vs);
            d3d.VSSetConstantBuffer(0, pipe.Cb);
            d3d.PSSetShader(pipe.Ps);
            d3d.PSSetConstantBuffer(0, pipe.Cb);
            foreach (var b in batches)
            {
                d3d.OMSetBlendState(b.Add ? pipe.BlendAdd : pipe.BlendNormal, null, -1);
                d3d.OMSetDepthStencilState(b.Write ? pipe.DepthWrite : pipe.DepthTest, 0);
                d3d.Draw(b.Count, b.Start);
            }
        }
        finally
        {
            if (d3d1 is not null && prev is not null) { d3d1.SwapDeviceContextState(prev); prev.Dispose(); }
            else
            {
                d3d.OMSetRenderTargets(oldRtv[0]!, oldDsv);
                if (oldVp.Length > 0) d3d.RSSetViewports(oldVp);
            }
            oldRtv[0]?.Dispose(); oldDsv?.Dispose();
        }
    }

    void UploadVertices(ID3D11Device device, ID3D11DeviceContext d3d)
    {
        int stride = Marshal.SizeOf<Vert3D>();
        if (vb is null || vbCapacity < verts.Count)
        {
            vb?.Dispose();
            vbCapacity = Math.Max(verts.Count * 3 / 2, 4096);
            vb = device.CreateBuffer(new BufferDescription(vbCapacity * stride, BindFlags.VertexBuffer, ResourceUsage.Default, CpuAccessFlags.None));
        }
        // 使う所だけを書き込む(後ろの残りは描かないので古いままでよい)
        d3d.UpdateSubresource(CollectionsMarshal.AsSpan(verts), vb, 0, 0, 0, new Vortice.Mathematics.Box(0, 0, 0, verts.Count * stride, 1, 1));
    }

    readonly record struct Batch(int Start, int Count, bool Add, bool Write);

    sealed class Placed
    {
        public Prim3D P = null!;
        public float Z0, Thick;
        public Vector2 Shift;
        public float SortDepth;
        public bool Opaque;
        public Matrix4x4? Spin;
    }

    /// <summary>部品を奥行きに並べて、立体の頂点を作る</summary>
    List<Batch> BuildMesh(List<Prim3D> prims, in DrawContext ctx, in Shape3DSettings s, float camDist,
        out Matrix4x4 model, out Vector3 camPos, out Matrix4x4 viewProj, out Vector4 fog,
        Matrix4x4? externalWorld = null, Vector3 externalCam = default)
    {
        verts.Clear();
        lastLitModel = false;
        int n = prims.Count;
        float hw = ctx.Width / 2f, hh = ctx.Height / 2f;
        float gap = MathF.Max(s.LayerGap, 0f);
        if (n > 1 && gap * (n - 1) > s.LayerMax) gap = MathF.Max(s.LayerMax, 0f) / (n - 1);
        float layerTotal = gap * Math.Max(n - 1, 0);
        float zr = MathF.Max(s.ZRange, 0f);
        float depth = s.Mode == Depth3DMode.Flat ? 0f : MathF.Max(s.Depth, 0f);
        bool manyPrims = n > 6000;

        // 回転: 部品の重なりのまん中(+指定のずれ)を中心に回す
        float pivotZ = layerTotal / 2f + depth / 2f + s.PivotZ;
        float rx = s.RotX * MathF.PI / 180f, ry = s.RotY * MathF.PI / 180f, rz = s.RotZ * MathF.PI / 180f;
        model = Matrix4x4.CreateTranslation(0, 0, -pivotZ)
            * Matrix4x4.CreateRotationZ(-rz) * Matrix4x4.CreateRotationX(rx) * Matrix4x4.CreateRotationY(-ry)
            * Matrix4x4.CreateTranslation(0, 0, pivotZ);
        float cam = camDist - s.CamZ;
        camPos = new Vector3(0, 0, -cam);
        float near = MathF.Max(cam * 0.02f, 1f);
        float far = cam + layerTotal + depth + zr + 20000f;
        float ys = 1f / MathF.Tan(MathF.Max(s.Fov, 1f) * MathF.PI / 360f), xs = ys * ctx.Height / MathF.Max(ctx.Width, 1f);
        var view = Matrix4x4.CreateTranslation(0, 0, cam);
        var proj = new Matrix4x4(xs, 0, 0, 0, 0, ys, 0, 0, 0, 0, far / (far - near), 1, 0, 0, -near * far / (far - near), 0);
        viewProj = view * proj;
        // かすみ: 手前の端から奥の端までで、だんだん薄くする
        float spanZ = layerTotal + depth + zr + MathF.Max(s.PartWave, 0f) * 2f;
        float fogNear = MathF.Max(cam - zr / 2f, 1f);
        float fogSpan = MathF.Max(spanZ, 200f);
        if (externalWorld is { } ew)
        {
            // 3Dプレビューの中: カメラは3Dプレビュー側。かすみの距離も3Dプレビューの単位(100px=1)に直す
            model = ExternalModel(s, pivotZ);
            camPos = externalCam;
            var center = Vector3.Transform(new Vector3(0, 0, pivotZ), model * ew);
            float d = Vector3.Distance(center, camPos);
            fogSpan *= PxToWorld;
            fogNear = MathF.Max(d - spanZ * PxToWorld / 2f, 0.01f);
        }
        fog = new Vector4(fogNear, fogNear + fogSpan, Math.Clamp(s.Fog, 0f, 1f), 1f);
        var sortWorld = model * (externalWorld ?? Matrix4x4.Identity);

        var placed = new List<Placed>(n);
        for (int i = 0; i < n; i++)
        {
            var p = prims[i];
            float baseZ = (n - 1 - i) * gap;
            float spread = 0f;
            if (zr > 0f)
            {
                float hsh = Gfx.Hash01(i * 7919 + s.Seed * 104729 + 17);
                spread = Gfx.Mod(hsh * zr - s.Time * s.ZFlow, zr) - zr / 2f;
            }
            float minSide = MathF.Min(p.Bounds.Right - p.Bounds.Left, p.Bounds.Bottom - p.Bounds.Top);
            float thick = s.Mode switch
            {
                Depth3DMode.Fit => MathF.Min(depth, MathF.Max(minSide, 0.5f)),
                Depth3DMode.Same => depth,
                _ => 0f,
            };
            if (manyPrims) thick = 0f;
            var c = new Vector2((p.Bounds.Left + p.Bounds.Right) / 2f - hw, hh - (p.Bounds.Top + p.Bounds.Bottom) / 2f);
            var shift = Vector2.Zero;
            if (zr > 0f && s.ZFill)
            {
                // 奥に置いた部品ほど外側へ広げ、カメラから見た位置を元の画面いっぱいに保つ
                float f = MathF.Max((camDist + spread) / camDist, 0.05f);
                shift = c * (f - 1f);
            }
            if (s.PartWave > 0f)
            {
                float ph = Gfx.Hash01(i * 3571 + s.Seed * 31 + 5) * MathF.PI * 2f;
                spread += MathF.Sin(s.Time * s.PartWaveSpeed * MathF.PI * 2f + ph) * s.PartWave;
            }
            float z0 = baseZ + spread;
            Matrix4x4? spin = null;
            if (MathF.Abs(s.PartSpin) > 0.001f)
            {
                // 部品ごとに、ばらばらの軸と速さで、その場で回す
                float h1 = Gfx.Hash01(i * 1543 + s.Seed * 7 + 11), h2 = Gfx.Hash01(i * 2903 + s.Seed * 13 + 3), h3 = Gfx.Hash01(i * 4271 + 29);
                var axis = Vector3.Normalize(new Vector3(MathF.Cos(h1 * MathF.PI * 2f), MathF.Sin(h1 * MathF.PI * 2f), (h2 - 0.5f) * 1.2f));
                float ang = (s.Time * s.PartSpin * (0.5f + h3) + h2 * 360f) * MathF.PI / 180f;
                var pc = new Vector3(c + shift, z0 + thick / 2f);
                spin = Matrix4x4.CreateTranslation(-pc) * Matrix4x4.CreateFromAxisAngle(axis, ang) * Matrix4x4.CreateTranslation(pc);
            }
            var wc = Vector3.Transform(new Vector3(c + shift, z0 + thick / 2f), sortWorld);
            bool opaque = !p.Add && p.Grad is null && p.Color.A * p.Opacity >= 0.995f;
            if (p.Grad is not null && opaque == false)
            {
                // グラデーションでも、全部の色が不透明なら不透明として扱う
                bool allOpaque = p.Opacity >= 0.995f && p.Grad.Opacity >= 0.995f && p.Grad.Stops.All(st => st.Color.A >= 0.995f);
                opaque = !p.Add && allOpaque;
            }
            placed.Add(new Placed { P = p, Z0 = z0, Thick = thick, Shift = shift, SortDepth = Vector3.Distance(wc, camPos), Opaque = opaque, Spin = spin });
        }

        var batches = new List<Batch>();
        // 不透明な部品を先に(深度を書く)。半透明・加算は、遠い順に重ねる(深度は書かない)
        // かすみがある時は、不透明な部品も薄くなるので、遠い順に重ねる
        if (s.Fog > 0.001f) foreach (var pl in placed) pl.Opaque = false;
        int start = verts.Count;
        foreach (var pl in placed) if (pl.Opaque) Emit(pl, hw, hh);
        if (verts.Count > start) batches.Add(new Batch(start, verts.Count - start, false, true));
        foreach (var pl in placed.Where(x => !x.Opaque).OrderByDescending(x => x.SortDepth))
        {
            int st = verts.Count;
            Emit(pl, hw, hh);
            int cnt = verts.Count - st;
            if (cnt == 0) continue;
            if (batches.Count > 0 && batches[^1] is { Write: false } last && last.Add == pl.P.Add && last.Start + last.Count == st)
                batches[^1] = last with { Count = last.Count + cnt };
            else
                batches.Add(new Batch(st, cnt, pl.P.Add, false));
        }
        return batches;
    }

    /// <summary>
    /// 本物の3Dの形から、描く頂点とまとまりの順番を作る。
    /// ふだんの画面: 図形の「3D」の回転で回し、元の絵と同じカメラで見る。3Dプレビューの中: 画面の面(Z=0)をアイテムの位置に置き、奥へ広がるように置く
    /// </summary>
    List<Batch> BuildNativeMesh(Native3D n, in DrawContext ctx, in Shape3DSettings s,
        out Matrix4x4 model, out Vector3 camPos, out Matrix4x4 viewProj, Matrix4x4? externalWorld, Vector3 externalCam)
    {
        verts.Clear();
        lastLitModel = n.Lit;
        float f = MathF.Max(n.Cam, 1f);
        if (externalWorld is null)
        {
            float rx = s.RotX * MathF.PI / 180f, ry = s.RotY * MathF.PI / 180f, rz = s.RotZ * MathF.PI / 180f;
            var pv = new Vector3(n.PivotXY, n.PivotZ + s.PivotZ);
            model = Matrix4x4.CreateTranslation(-pv)
                * Matrix4x4.CreateRotationZ(-rz) * Matrix4x4.CreateRotationX(rx) * Matrix4x4.CreateRotationY(-ry)
                * Matrix4x4.CreateTranslation(pv);
            float cam = MathF.Max(f - s.CamZ, 1f);
            camPos = new Vector3(0, 0, -cam);
            float near = MathF.Max(cam * 0.01f, 1f);
            float far = cam + MathF.Max(n.Max.Z, 0f) + 40000f;
            // 見え方の広さは、元の絵と同じ(画面の高さと Cam で決まる)
            float ys = 2f * f / MathF.Max(n.Height, 1f), xs = ys * ctx.Height / MathF.Max(ctx.Width, 1f);
            var view = Matrix4x4.CreateTranslation(0, 0, cam);
            var proj = new Matrix4x4(xs, 0, 0, 0, 0, ys, 0, 0, 0, 0, far / (far - near), 1, 0, 0, -near * far / (far - near), 0);
            viewProj = view * proj;
        }
        else
        {
            // 3Dプレビューの中: 画面の面(Z=0)をアイテムの位置に置き、奥へ広がるように置く
            model = ExternalModel(s, 0f);
            camPos = externalCam;
            viewProj = Matrix4x4.Identity;
        }
        var toWorld = model * (externalWorld ?? Matrix4x4.Identity);
        var batches = new List<Batch>();
        int start = verts.Count;
        foreach (var g in n.Groups) if (g.Opaque) verts.AddRange(CollectionsMarshal.AsSpan(n.Verts).Slice(g.Start, g.Count));
        if (verts.Count > start) batches.Add(new Batch(start, verts.Count - start, false, true));
        var cp = camPos;
        var trans = n.Groups.Where(g => !g.Opaque).Select(g => (g, d: Vector3.DistanceSquared(Vector3.Transform(g.Center, toWorld), cp))).OrderByDescending(x => x.d).ToList();
        int ts = verts.Count;
        foreach (var (g, _) in trans) verts.AddRange(CollectionsMarshal.AsSpan(n.Verts).Slice(g.Start, g.Count));
        if (verts.Count > ts) batches.Add(new Batch(ts, verts.Count - ts, false, false));
        return batches;
    }

    /// <summary>部品1つ分の立体(前の面・後ろの面・側面)を頂点に足す</summary>
    void Emit(Placed pl, float hw, float hh)
    {
        var p = pl.P;
        int first = verts.Count;
        float z0 = pl.Z0, z1 = pl.Z0 + pl.Thick;
        var shift = pl.Shift;
        Vector3 To3(Vector2 v, float z) => new(v.X - hw + shift.X, hh - v.Y + shift.Y, z);
        Vector4 Col(Vector2 v) { var c = p.ColorAt(v); return new Vector4(c.R, c.G, c.B, c.A); }

        var front = new Vector3(0, 0, -1);
        var back = new Vector3(0, 0, 1);
        bool grad = p.Grad is not null;
        var tris = grad ? Subdivide(p.Tris, 48f) : p.Tris;
        var flat = new Vector4(p.Color.R, p.Color.G, p.Color.B, p.Color.A * p.Opacity);
        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            var a = tris[i]; var b = tris[i + 1]; var c = tris[i + 2];
            Vector4 ca = grad ? Col(a) : flat, cbb = grad ? Col(b) : flat, cc = grad ? Col(c) : flat;
            verts.Add(new Vert3D(To3(a, z0), front, ca));
            verts.Add(new Vert3D(To3(b, z0), front, cbb));
            verts.Add(new Vert3D(To3(c, z0), front, cc));
            if (pl.Thick > 0.01f)
            {
                verts.Add(new Vert3D(To3(a, z1), back, ca));
                verts.Add(new Vert3D(To3(c, z1), back, cc));
                verts.Add(new Vert3D(To3(b, z1), back, cbb));
            }
        }
        if (pl.Thick <= 0.01f) { SpinVerts(pl, first); return; }

        // 側面: 輪郭の折れ線を前から後ろへ伸ばす。曲線(角度の小さい折れ目)はなめらかな陰にする
        const float smoothCos = 0.766f; // 40°
        foreach (var fig in p.Outlines)
        {
            int m = fig.Length;
            if (m < 2) continue;
            var segN = new Vector3[m];
            for (int i = 0; i < m; i++)
            {
                var a = To3(fig[i], 0); var b = To3(fig[(i + 1) % m], 0);
                var e = b - a;
                var nn = new Vector3(e.Y, -e.X, 0);
                float len = nn.Length();
                segN[i] = len > 1e-6f ? nn / len : Vector3.UnitX;
            }
            for (int i = 0; i < m; i++)
            {
                int j = (i + 1) % m;
                var n0 = segN[i]; var nPrev = segN[(i - 1 + m) % m]; var nNext = segN[j];
                var na = Vector3.Dot(n0, nPrev) > smoothCos ? Vector3.Normalize(n0 + nPrev) : n0;
                var nb = Vector3.Dot(n0, nNext) > smoothCos ? Vector3.Normalize(n0 + nNext) : n0;
                Vector4 ca = grad ? Col(fig[i]) : flat, cbb = grad ? Col(fig[j]) : flat;
                var a0 = To3(fig[i], z0); var b0 = To3(fig[j], z0); var a1 = To3(fig[i], z1); var b1 = To3(fig[j], z1);
                verts.Add(new Vert3D(a0, na, ca)); verts.Add(new Vert3D(b0, nb, cbb)); verts.Add(new Vert3D(b1, nb, cbb));
                verts.Add(new Vert3D(a0, na, ca)); verts.Add(new Vert3D(b1, nb, cbb)); verts.Add(new Vert3D(a1, na, ca));
            }
        }
        SpinVerts(pl, first);
    }

    /// <summary>部品のひらひら回転を、その部品の頂点にかける</summary>
    void SpinVerts(Placed pl, int first)
    {
        if (pl.Spin is not { } m) return;
        var span = CollectionsMarshal.AsSpan(verts);
        for (int i = first; i < span.Length; i++)
        {
            span[i].Pos = Vector3.Transform(span[i].Pos, m);
            span[i].Normal = Vector3.TransformNormal(span[i].Normal, m);
        }
    }

    /// <summary>グラデーションの色をなめらかにするため、大きな三角形を細かく分ける</summary>
    static Vector2[] Subdivide(Vector2[] tris, float maxEdge)
    {
        var outList = new List<Vector2>(tris.Length);
        var stack = new Stack<(Vector2, Vector2, Vector2, int)>();
        for (int i = 0; i + 2 < tris.Length; i += 3)
        {
            stack.Push((tris[i], tris[i + 1], tris[i + 2], 0));
            while (stack.Count > 0)
            {
                var (a, b, c, d) = stack.Pop();
                float ab = Vector2.DistanceSquared(a, b), bc = Vector2.DistanceSquared(b, c), ca = Vector2.DistanceSquared(c, a);
                float mx = MathF.Max(ab, MathF.Max(bc, ca));
                if (mx <= maxEdge * maxEdge || d >= 7 || outList.Count > 200000) { outList.Add(a); outList.Add(b); outList.Add(c); continue; }
                // いちばん長い辺を半分に切る
                if (mx == ab) { var mid = (a + b) / 2; stack.Push((a, mid, c, d + 1)); stack.Push((mid, b, c, d + 1)); }
                else if (mx == bc) { var mid = (b + c) / 2; stack.Push((a, b, mid, d + 1)); stack.Push((a, mid, c, d + 1)); }
                else { var mid = (c + a) / 2; stack.Push((a, b, mid, d + 1)); stack.Push((mid, b, c, d + 1)); }
            }
        }
        return [.. outList];
    }

    public void Dispose()
    {
        ReleaseTargets();
        vb?.Dispose(); vb = null;
        extVb?.Dispose(); extVb = null;
    }
}
