using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Plugin;
using YukkuriMovieMaker.Plugin.Shape;
using WpfColor = System.Windows.Media.Color;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// =====================================================================
//  1001 文字PV背景（立体図形）
//   立体を3Dで計算して、奥から順に面を塗る。1つの図形で並び方・形・描き方・配色を選べる
// =====================================================================

public enum PvScene
{
    [Display(Name = "浮かぶ立体")] Float,
    [Display(Name = "立方体の波")] Wave,
    [Display(Name = "柱の街")] City,
    [Display(Name = "トンネル")] Tunnel,
    [Display(Name = "リングの回廊")] Rings,
    [Display(Name = "らせん")] Spiral,
    [Display(Name = "立体の球")] Sphere,
    [Display(Name = "階段")] Stairs,
    [Display(Name = "ローポリの谷")] Valley,
    [Display(Name = "立方体の壁")] Wall,
    [Display(Name = "床と天井の回廊")] Corridor,
    [Display(Name = "アーチの道")] Arches,
    [Display(Name = "立体の銀河")] Galaxy,
    [Display(Name = "回る円柱")] Drum,
    [Display(Name = "波紋の床")] Ripple,
    [Display(Name = "ピラミッド")] Pyramid,
    [Display(Name = "立体の雨")] Rain,
    [Display(Name = "舞い上がる立体")] Rise,
    [Display(Name = "軌道")] Orbit,
    [Display(Name = "二重らせん")] Dna,
    [Display(Name = "交差する流れ")] CrossFlow,
    [Display(Name = "はじける立体")] Burst,
    [Display(Name = "音のバー")] Bars,
    [Display(Name = "めくれる市松")] Checker,
    [Display(Name = "浮かぶパネル")] Panels,
    [Display(Name = "山脈")] Mountains,
    [Display(Name = "ローポリの海")] Ocean,
    [Display(Name = "洞くつ")] Cave,
    [Display(Name = "割れる破片")] Shards,
    [Display(Name = "入れ子の立方体")] Nest,
    // ここから2D(平面)の背景。3Dの背景はこれより前に足す
    [Display(Name = "流れるグラデーション")] Gradient,
    [Display(Name = "斜めストライプ")] Stripes,
    [Display(Name = "放射線")] Sunburst,
    [Display(Name = "集中線")] SpeedLines,
    [Display(Name = "同心円")] Circles,
    [Display(Name = "波線")] WaveLines,
    [Display(Name = "水玉")] Dots,
    [Display(Name = "市松模様")] Checks,
    [Display(Name = "三角タイル")] Triangles,
    [Display(Name = "六角タイル")] Hexagons,
    [Display(Name = "ひし形タイル")] Diamonds,
    [Display(Name = "方眼")] GridLines,
    [Display(Name = "走査線")] Scan,
    [Display(Name = "光の粒")] Particles,
    [Display(Name = "玉ボケ")] Bokeh,
    [Display(Name = "流れ星")] Meteors,
    [Display(Name = "雨の線")] RainLines,
    [Display(Name = "舞う花びら")] Petals,
    [Display(Name = "流れる雲")] Clouds,
    [Display(Name = "重なる山")] Hills,
    [Display(Name = "街のシルエット")] Skyline,
    [Display(Name = "音の波形")] Waveform,
    [Display(Name = "イコライザー")] Equalizer,
    [Display(Name = "オーロラ")] Aurora,
    [Display(Name = "幾何学コラージュ")] Memphis,
    [Display(Name = "回る円弧")] ArcRings,
    [Display(Name = "グリッチの帯")] Glitch,
    [Display(Name = "紙吹雪")] Confetti,
    [Display(Name = "ハーフトーン")] Halftone,
    [Display(Name = "光のカーテン")] Curtain,
}

/// <summary>背景の種類が2Dかどうか(プラグインの外からも使う)</summary>
public static class PvFlatInfo
{
    public static bool IsFlat(PvScene x) => PvFlat.IsFlat(x);
}

public enum PvSolid
{
    [Display(Name = "混ぜる")] Mix,
    [Display(Name = "立方体")] Cube,
    [Display(Name = "板")] Plate,
    [Display(Name = "三角すい")] Tetra,
    [Display(Name = "八面体")] Octa,
    [Display(Name = "二十面体")] Icosa,
    [Display(Name = "三角柱")] Prism3,
    [Display(Name = "六角柱")] Prism6,
    [Display(Name = "ドーナツ")] Torus,
    [Display(Name = "四角すい")] Pyramid,
    [Display(Name = "星の柱")] StarPrism,
    [Display(Name = "十字")] Cross,
}

public enum PvStyle
{
    [Display(Name = "面")] Fill,
    [Display(Name = "面と線")] FillLine,
    [Display(Name = "線だけ")] Wire,
    [Display(Name = "ガラス")] Glass,
    [Display(Name = "ネオン")] Neon,
    [Display(Name = "点描")] Dots,
    [Display(Name = "太い輪郭")] Bold,
    [Display(Name = "影絵")] Silhouette,
}

public enum PvPalette
{
    [Display(Name = "自分の色")] Custom,
    [Display(Name = "ネオン")] Neon,
    [Display(Name = "パステル")] Pastel,
    [Display(Name = "モノクロ")] Mono,
    [Display(Name = "夕焼け")] Sunset,
    [Display(Name = "深海")] Deep,
    [Display(Name = "和風")] Wafu,
    [Display(Name = "サイバー")] Cyber,
    [Display(Name = "桜")] Sakura,
    [Display(Name = "新緑")] Fresh,
    [Display(Name = "海辺")] Beach,
    [Display(Name = "紅葉")] Autumn,
    [Display(Name = "雪")] Snow,
    [Display(Name = "黄金")] Gold,
    [Display(Name = "白銀")] Silver,
    [Display(Name = "赤と黒")] RedBlack,
    [Display(Name = "青と白")] BlueWhite,
    [Display(Name = "虹")] Rainbow,
    [Display(Name = "レトロ")] Retro,
    [Display(Name = "夢かわ")] Dream,
}

public sealed class PvStage3DPlugin : IShapePlugin
{
    public string Name => "演出：文字PV背景（立体図形）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new PvStage3DParameter(sharedData);
}

public sealed class PvStage3DParameter : EffectShapeParameterBase
{
    public PvStage3DParameter() : this(null) { }
    public PvStage3DParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "組み合わせ", Name = "背景の種類", Description = "3Dの並び方と、2Dの背景から選びます")]
    [EnumComboBox]
    public PvScene Scene { get => scene; set => Set(ref scene, value); }
    PvScene scene = PvScene.Float;

    [Display(GroupName = "組み合わせ", Name = "立体", Description = "3D背景で使う立体の形。「混ぜる」はいろいろな形を組み合わせます(2D背景では使いません)")]
    [EnumComboBox]
    public PvSolid Solid { get => solid; set => Set(ref solid, value); }
    PvSolid solid = PvSolid.Mix;

    [Display(GroupName = "組み合わせ", Name = "描き方", Description = "3D背景の描き方(2D背景では使いません)")]
    [EnumComboBox]
    public PvStyle Style { get => style; set => Set(ref style, value); }
    PvStyle style = PvStyle.FillLine;

    [Display(GroupName = "組み合わせ", Name = "配色", Description = "「自分の色」のときは、下の色の設定を使います")]
    [EnumComboBox]
    public PvPalette Palette { get => palette; set => Set(ref palette, value); }
    PvPalette palette = PvPalette.Custom;

    [Display(GroupName = "色", Name = "背景の色（上）")]
    [ColorPicker]
    public WpfColor Back1 { get => back1; set => Set(ref back1, value); }
    WpfColor back1 = WpfColor.FromRgb(0x0B, 0x10, 0x26);

    [Display(GroupName = "色", Name = "背景の色（下）")]
    [ColorPicker]
    public WpfColor Back2 { get => back2; set => Set(ref back2, value); }
    WpfColor back2 = WpfColor.FromRgb(0x2A, 0x14, 0x58);

    [Display(GroupName = "色", Name = "立体の色1")]
    [ColorPicker]
    public WpfColor Color1 { get => color1; set => Set(ref color1, value); }
    WpfColor color1 = WpfColor.FromRgb(0xFF, 0x3D, 0x81);

    [Display(GroupName = "色", Name = "立体の色2")]
    [ColorPicker]
    public WpfColor Color2 { get => color2; set => Set(ref color2, value); }
    WpfColor color2 = WpfColor.FromRgb(0x38, 0xD6, 0xFF);

    [Display(GroupName = "色", Name = "立体の色3")]
    [ColorPicker]
    public WpfColor Color3 { get => color3; set => Set(ref color3, value); }
    WpfColor color3 = WpfColor.FromRgb(0xFF, 0xD2, 0x4A);

    [Display(GroupName = "色", Name = "背景を塗る", Description = "切ると立体だけを描きます。動画の上に重ねるときに使います")]
    [ToggleSlider]
    public bool BackOn { get => backOn; set => Set(ref backOn, value); }
    bool backOn = true;

    [Display(GroupName = "色", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(100, 0, 100);

    [Display(GroupName = "立体", Name = "数", Description = "多いほど重くなります")]
    [AnimationSlider("F0", "個", 10, 300)]
    public Animation Count { get; } = new Animation(70, 1, 400);

    [Display(GroupName = "立体", Name = "大きさ")]
    [AnimationSlider("F0", "%", 20, 300)]
    public Animation SolidSize { get; } = new Animation(100, 1, 1000);

    [Display(GroupName = "立体", Name = "広がり")]
    [AnimationSlider("F0", "%", 40, 200)]
    public Animation Spread { get; } = new Animation(100, 10, 500);

    [Display(GroupName = "立体", Name = "回る速さ")]
    [AnimationSlider("F0", "%", -300, 300)]
    public Animation Spin { get; } = new Animation(100, -2000, 2000);

    [Display(GroupName = "カメラ", Name = "進む速さ", Description = "マイナスにすると後ろへ下がります")]
    [AnimationSlider("F0", "%", -300, 300)]
    public Animation Move { get; } = new Animation(100, -3000, 3000);

    [Display(GroupName = "カメラ", Name = "左右の向き")]
    [AnimationSlider("F0", "°", -60, 60)]
    public Animation Yaw { get; } = new Animation(0, -180, 180);

    [Display(GroupName = "カメラ", Name = "上下の向き")]
    [AnimationSlider("F0", "°", -45, 45)]
    public Animation Pitch { get; } = new Animation(0, -90, 90);

    [Display(GroupName = "カメラ", Name = "傾き")]
    [AnimationSlider("F0", "°", -45, 45)]
    public Animation Roll { get; } = new Animation(0, -360, 360);

    [Display(GroupName = "カメラ", Name = "ゆれ", Description = "カメラをゆっくり自動でゆらします")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Sway { get; } = new Animation(60, 0, 1000);

    [Display(GroupName = "カメラ", Name = "ズーム")]
    [AnimationSlider("F0", "%", 50, 200)]
    public Animation Zoom { get; } = new Animation(100, 10, 1000);

    [Display(GroupName = "光と線", Name = "陰影の強さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Shade { get; } = new Animation(85, 0, 100);

    [Display(GroupName = "光と線", Name = "奥のかすみ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Fog { get; } = new Animation(70, 0, 100);

    [Display(GroupName = "光と線", Name = "線の太さ")]
    [AnimationSlider("F1", "px", 0, 10)]
    public Animation LineWidth { get; } = new Animation(1.6, 0, 40);

    [Display(GroupName = "文字を見やすく", Name = "中央を空ける", Description = "文字を置く中央から立体をよけます(浮かぶ立体・壁などで効きます)")]
    [AnimationSlider("F0", "%", 0, 150)]
    public Animation CenterClear { get; } = new Animation(55, 0, 300);

    [Display(GroupName = "文字を見やすく", Name = "中央を暗く")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CenterDim { get; } = new Animation(25, 0, 100);

    [Display(GroupName = "文字を見やすく", Name = "周りを暗く")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation EdgeDim { get; } = new Animation(35, 0, 100);

    [Display(GroupName = "音楽に合わせる", Name = "BPM", Description = "曲の速さ。拍の強さが0より大きいときに使います")]
    [AnimationSlider("F0", "", 40, 240)]
    public Animation Bpm { get; } = new Animation(120, 1, 999);

    [Display(GroupName = "音楽に合わせる", Name = "拍の強さ", Description = "拍ごとに立体とカメラがはずみます")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Beat { get; } = new Animation(0, 0, 1000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Opacity, Count, SolidSize, Spread, Spin, Move, Yaw, Pitch, Roll, Sway, Zoom, Shade, Fog, LineWidth, CenterClear, CenterDim, EdgeDim, Bpm, Beat];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        var setup = MakeSetup(c, out var pal, out float op);
        if (op <= 0.002f) return;
        using var brush = dc.CreateSolidColorBrush(default);
        if (BackOn) PaintBack(dc, c.Width, c.Height, pal, op);
        if (PvFlat.IsFlat(Scene)) PvFlat.Paint(dc, brush, setup);
        else
        {
            var stage = new PvStage(setup);
            stage.Build();
            stage.Paint(dc, brush);
        }
        PaintDim(dc, c.Width, c.Height, c.F(CenterDim) / 100f * op, c.F(EdgeDim) / 100f * op);
    }

    /// <summary>
    /// 3Dのとき: 立体を2Dの絵に直さず、本物の3Dの形のまま渡す(3Dプレビューで回しても、ちゃんと奥行きのある立体になる)。
    /// 2Dの背景(グラデーションなど)の種類は、今まで通り2Dの絵から立体にする
    /// </summary>
    internal override bool BuildNative3D(in DrawContext c, Native3D n)
    {
        if (PvFlat.IsFlat(Scene)) return false;
        var setup = MakeSetup(c, out _, out float op);
        if (op <= 0.002f) return true;
        new PvStage(setup).BuildNative(n, BackOn);
        return true;
    }

    PvStage.Setup MakeSetup(in DrawContext c, out PvColors pal, out float op)
    {
        op = Math.Clamp(c.F(Opacity) / 100f, 0f, 1f);
        pal = PvPalettes.Get(Palette, Back1, Back2, Color1, Color2, Color3);
        float t = c.Time, bpm = MathF.Max(1f, c.F(Bpm));
        float ph = t * bpm / 60f;
        float beat = c.F(Beat) / 100f * MathF.Exp(-(ph - MathF.Floor(ph)) * 5f);
        float sway = c.F(Sway) / 100f;
        return new PvStage.Setup
        {
            W = c.Width, H = c.Height, T = t, Seed = Seed, Scene = Scene, Solid = Solid, Style = Style, Pal = pal, Alpha = op,
            N = Math.Clamp((int)c.F(Count), 1, 400),
            Size = c.F(SolidSize) / 100f * (1f + beat * .16f),
            Spread = c.F(Spread) / 100f,
            Spin = c.F(Spin) / 100f,
            Dist = t * c.F(Move) / 100f * 3.2f,
            Yaw = Gfx.Rad(c.F(Yaw)) + MathF.Sin(t * .31f) * .09f * sway,
            Pitch = Gfx.Rad(c.F(Pitch)) + MathF.Sin(t * .23f + 1f) * .05f * sway,
            Roll = Gfx.Rad(c.F(Roll)) + MathF.Sin(t * .19f + 2f) * .04f * sway,
            Zoom = c.F(Zoom) / 100f * (1f + beat * .035f),
            Shade = c.F(Shade) / 100f,
            Fog = c.F(Fog) / 100f,
            Line = c.F(LineWidth) * c.Height / 1080f,
            Clear = c.F(CenterClear) / 100f,
            Flash = beat,
        };
    }

    static Color4 C(Vector3 v, float a) => new(v.X, v.Y, v.Z, Math.Clamp(a, 0f, 1f));

    static void PaintBack(ID2D1DeviceContext dc, float w, float h, in PvColors p, float a)
    {
        GradientStop[] gs = [new() { Position = 0f, Color = C(p.Back1, a) }, new() { Position = 1f, Color = C(p.Back2, a) }];
        using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
        using var b = dc.CreateLinearGradientBrush(new LinearGradientBrushProperties { StartPoint = new(w * .5f, 0f), EndPoint = new(w * .5f, h) }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
        dc.FillRectangle(new RawRectF(0, 0, w, h), b);
    }

    static void PaintDim(ID2D1DeviceContext dc, float w, float h, float center, float edge)
    {
        var mid = new Vector2(w * .5f, h * .5f);
        if (center > .003f)
        {
            GradientStop[] gs = [new() { Position = 0f, Color = new Color4(0, 0, 0, center) }, new() { Position = .55f, Color = new Color4(0, 0, 0, center * .72f) }, new() { Position = 1f, Color = new Color4(0, 0, 0, 0) }];
            using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
            using var b = dc.CreateRadialGradientBrush(new RadialGradientBrushProperties { Center = mid, GradientOriginOffset = Vector2.Zero, RadiusX = w * .42f, RadiusY = h * .4f }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
            dc.FillRectangle(new RawRectF(0, 0, w, h), b);
        }
        if (edge > .003f)
        {
            GradientStop[] gs = [new() { Position = .45f, Color = new Color4(0, 0, 0, 0) }, new() { Position = 1f, Color = new Color4(0, 0, 0, edge) }];
            using var coll = dc.CreateGradientStopCollection(gs, Gamma.StandardRgb, ExtendMode.Clamp);
            using var b = dc.CreateRadialGradientBrush(new RadialGradientBrushProperties { Center = mid, GradientOriginOffset = Vector2.Zero, RadiusX = w * .74f, RadiusY = h * .78f }, new BrushProperties { Opacity = 1f, Transform = Matrix3x2.Identity }, coll);
            dc.FillRectangle(new RawRectF(0, 0, w, h), b);
        }
    }
}

internal readonly record struct PvColors(Vector3 Back1, Vector3 Back2, Vector3 A, Vector3 B, Vector3 C);

internal static class PvPalettes
{
    static Vector3 H(uint x) => new(((x >> 16) & 255) / 255f, ((x >> 8) & 255) / 255f, (x & 255) / 255f);
    static Vector3 W(WpfColor c) => new(c.R / 255f, c.G / 255f, c.B / 255f);
    static PvColors P(uint a, uint b, uint c, uint d, uint e) => new(H(a), H(b), H(c), H(d), H(e));

    public static PvColors Get(PvPalette p, WpfColor b1, WpfColor b2, WpfColor c1, WpfColor c2, WpfColor c3) => p switch
    {
        PvPalette.Neon => P(0x07071A, 0x1A0A3D, 0xFF2BD6, 0x19F0FF, 0xB6FF3C),
        PvPalette.Pastel => P(0xFFE9F3, 0xDDEBFF, 0xFF9EC7, 0x8FD3FF, 0xFFE08A),
        PvPalette.Mono => P(0x0E0E10, 0x2C2C33, 0xF2F2F2, 0x8A8A92, 0x4A4A52),
        PvPalette.Sunset => P(0x2B1240, 0xFF7A59, 0xFF4D6D, 0xFFB347, 0x7B5BE6),
        PvPalette.Deep => P(0x02101F, 0x083A5C, 0x1EC8FF, 0x34E0B8, 0x2F6BFF),
        PvPalette.Wafu => P(0x140A12, 0x4A1226, 0xD7263D, 0xE8C15A, 0xF3E7D3),
        PvPalette.Cyber => P(0x02060A, 0x06262B, 0x00FFA3, 0x00C2FF, 0xF5FF3D),
        PvPalette.Sakura => P(0xFFE6EE, 0xFFC9DA, 0xFF7FA8, 0xFFFFFF, 0x9CCB6B),
        PvPalette.Fresh => P(0x0F2E22, 0x2F7A4B, 0x8EE36A, 0xE4F58A, 0x35C9A0),
        PvPalette.Beach => P(0x8FE3F5, 0xFFF0C9, 0x1E9BD7, 0xFFD27A, 0xFF7F6B),
        PvPalette.Autumn => P(0x2A120C, 0x6B2A14, 0xE8552B, 0xF2B134, 0xA63A24),
        PvPalette.Snow => P(0xDCEBFA, 0xFFFFFF, 0x7FB6E8, 0xB9D8F5, 0x4F86C6),
        PvPalette.Gold => P(0x140E04, 0x3A2708, 0xF4CF57, 0xB9852A, 0xFFF0B0),
        PvPalette.Silver => P(0x12151B, 0x343A46, 0xE6EBF2, 0x9BA6B5, 0x667080),
        PvPalette.RedBlack => P(0x070405, 0x2A060B, 0xE5142E, 0x7A0C1A, 0xF2F2F2),
        PvPalette.BlueWhite => P(0x061537, 0x123B86, 0xFFFFFF, 0x4FA8FF, 0x1F66D6),
        PvPalette.Rainbow => P(0x0C0B1F, 0x221440, 0xFF4D6D, 0x3DDC97, 0x4DA3FF),
        PvPalette.Retro => P(0x2E2A3F, 0xE8A87C, 0xE27D60, 0x41B3A3, 0xF2D388),
        PvPalette.Dream => P(0xCDB4FF, 0xFFC8DD, 0xFF8FC7, 0x8FDDFF, 0xFFF3A8),
        _ => new(W(b1), W(b2), W(c1), W(c2), W(c3)),
    };
}

/// <summary>立体の頂点と面(面は外向きにそろえてある)</summary>
internal sealed class PvMesh
{
    public readonly Vector3[] V;
    public readonly int[][] F;
    PvMesh(Vector3[] v, int[][] f, Func<int[], Vector3>? anchor = null)
    {
        V = v; F = f;
        foreach (var x in F)
        {
            var c = Vector3.Zero; foreach (int k in x) c += v[k]; c /= x.Length;
            var n = Vector3.Zero;
            for (int i = 0; i < x.Length; i++) n += Vector3.Cross(v[x[i]], v[x[(i + 1) % x.Length]]);     // へこんだ面でも向きを正しく求める
            if (Vector3.Dot(n, c - (anchor?.Invoke(x) ?? Vector3.Zero)) < 0f) Array.Reverse(x);
        }
    }

    public static readonly PvMesh Cube = new(
        [new(-1, -1, -1), new(1, -1, -1), new(1, 1, -1), new(-1, 1, -1), new(-1, -1, 1), new(1, -1, 1), new(1, 1, 1), new(-1, 1, 1)],
        [[0, 1, 2, 3], [4, 5, 6, 7], [0, 1, 5, 4], [2, 3, 7, 6], [0, 3, 7, 4], [1, 2, 6, 5]]);

    public static readonly PvMesh Tetra = new(
        [new(1, 1, 1), new(-1, -1, 1), new(-1, 1, -1), new(1, -1, -1)],
        [[0, 1, 2], [0, 1, 3], [0, 2, 3], [1, 2, 3]]);

    public static readonly PvMesh Octa = new(
        [new(1.3f, 0, 0), new(-1.3f, 0, 0), new(0, 1.3f, 0), new(0, -1.3f, 0), new(0, 0, 1.3f), new(0, 0, -1.3f)],
        [[0, 2, 4], [2, 1, 4], [1, 3, 4], [3, 0, 4], [2, 0, 5], [1, 2, 5], [3, 1, 5], [0, 3, 5]]);

    public static readonly PvMesh Icosa = MakeIcosa();
    public static readonly PvMesh Prism3 = MakePrism(3);
    public static readonly PvMesh Prism6 = MakePrism(6);
    public static readonly PvMesh Torus = MakeTorus(14, 6, .92f, .38f);
    public static readonly PvMesh Pyramid = new(
        [new(-1.1f, -.9f, -1.1f), new(1.1f, -.9f, -1.1f), new(1.1f, -.9f, 1.1f), new(-1.1f, -.9f, 1.1f), new(0, 1.3f, 0)],
        [[0, 1, 2, 3], [0, 1, 4], [1, 2, 4], [2, 3, 4], [3, 0, 4]]);
    public static readonly PvMesh StarPrism = MakeOutline(Enumerable.Range(0, 10).Select(i => { float a = i * MathF.PI / 5f + MathF.PI / 2f, r = i % 2 == 0 ? 1.3f : .56f; return new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r); }).ToArray(), .42f);
    public static readonly PvMesh Cross = MakeOutline([new(-.36f, 1.2f), new(.36f, 1.2f), new(.36f, .36f), new(1.2f, .36f), new(1.2f, -.36f), new(.36f, -.36f), new(.36f, -1.2f), new(-.36f, -1.2f), new(-.36f, -.36f), new(-1.2f, -.36f), new(-1.2f, .36f), new(-.36f, .36f)], .42f);
    public static readonly PvMesh[] Mix = [Cube, Octa, Tetra, Icosa, Prism3, Prism6, Torus, Pyramid, StarPrism, Cross];

    /// <summary>平らな輪郭に厚みを付けた立体</summary>
    static PvMesh MakeOutline(Vector2[] o, float depth)
    {
        int c = o.Length; var v = new Vector3[c * 2]; var f = new List<int[]>();
        for (int i = 0; i < c; i++)
        {
            v[i] = new(o[i].X, o[i].Y, -depth); v[i + c] = new(o[i].X, o[i].Y, depth);
            f.Add([i, (i + 1) % c, (i + 1) % c + c, i + c]);
        }
        f.Add(Enumerable.Range(0, c).ToArray()); f.Add(Enumerable.Range(c, c).ToArray());
        return new(v, f.ToArray());
    }

    static PvMesh MakeIcosa()
    {
        float g = (1f + MathF.Sqrt(5f)) / 2f, k = 1.15f / MathF.Sqrt(1f + g * g) ;
        Vector3[] v = [new(-1, g, 0), new(1, g, 0), new(-1, -g, 0), new(1, -g, 0), new(0, -1, g), new(0, 1, g), new(0, -1, -g), new(0, 1, -g), new(g, 0, -1), new(g, 0, 1), new(-g, 0, -1), new(-g, 0, 1)];
        for (int i = 0; i < v.Length; i++) v[i] *= k;
        return new(v, [[0, 11, 5], [0, 5, 1], [0, 1, 7], [0, 7, 10], [0, 10, 11], [1, 5, 9], [5, 11, 4], [11, 10, 2], [10, 7, 6], [7, 1, 8], [3, 9, 4], [3, 4, 2], [3, 2, 6], [3, 6, 8], [3, 8, 9], [4, 9, 5], [2, 4, 11], [6, 2, 10], [8, 6, 7], [9, 8, 1]]);
    }

    static PvMesh MakePrism(int n)
    {
        var v = new Vector3[n * 2]; var f = new List<int[]>();
        for (int i = 0; i < n; i++)
        {
            float a = i * MathF.Tau / n + MathF.PI / 2f;
            v[i] = new(MathF.Cos(a) * 1.15f, MathF.Sin(a) * 1.15f, -1f); v[i + n] = new(v[i].X, v[i].Y, 1f);
            f.Add([i, (i + 1) % n, (i + 1) % n + n, i + n]);
        }
        f.Add(Enumerable.Range(0, n).ToArray()); f.Add(Enumerable.Range(n, n).ToArray());
        return new(v, f.ToArray());
    }

    static PvMesh MakeTorus(int nu, int nv, float R, float r)
    {
        var v = new Vector3[nu * nv]; var f = new List<int[]>();
        for (int u = 0; u < nu; u++)
            for (int w = 0; w < nv; w++)
            {
                float a = u * MathF.Tau / nu, b = w * MathF.Tau / nv, q = R + r * MathF.Cos(b);
                v[u * nv + w] = new(MathF.Cos(a) * q, MathF.Sin(a) * q, r * MathF.Sin(b));
                f.Add([u * nv + w, (u + 1) % nu * nv + w, (u + 1) % nu * nv + (w + 1) % nv, u * nv + (w + 1) % nv]);
            }
        return new(v, f.ToArray(), x =>
        {
            var c = Vector3.Zero; foreach (int k in x) c += v[k]; c /= x.Length;
            float l = MathF.Sqrt(c.X * c.X + c.Y * c.Y);
            return l < 1e-4f ? Vector3.Zero : new Vector3(c.X / l * R, c.Y / l * R, 0f);
        });
    }
}

/// <summary>1フレーム分の立体を組み立てて、奥から順に塗る</summary>
internal sealed class PvStage(PvStage.Setup s)
{
    internal struct Setup
    {
        public float W, H, T, Size, Spread, Spin, Dist, Yaw, Pitch, Roll, Zoom, Shade, Fog, Line, Clear, Flash, Alpha;
        public int Seed, N;
        public PvScene Scene;
        public PvSolid Solid;
        public PvStyle Style;
        public PvColors Pal;
    }

    readonly record struct Face(List<Vector2> P, float Z, Vector3 Col, float A);

    const float Near = .42f, Far = 42f;
    static readonly Vector3 Light = Vector3.Normalize(new(.45f, .82f, -.42f));
    readonly List<Face> faces = new(1024);
    readonly Vector3[] buf = new Vector3[32], clip = new Vector3[40];
    Quaternion view = Quaternion.Identity;
    Vector3 eye;
    float instFade = -1f;
    // 本物の3Dで渡す時の入れ物(null のときは、ふつうに2Dの絵にする)
    Native3D? nat;
    float natK;
    bool natGroup;
    readonly bool lightBack = Vector3.Dot(Vector3.Lerp(s.Pal.Back1, s.Pal.Back2, .5f), new(.3f, .59f, .11f)) > .6f;

    float R(int i, int k) => Gfx.Hash01(unchecked(s.Seed * 131 + i * 17 + k * 1009 + 7));
    static float Mod(float x, float m) => x - MathF.Floor(x / m) * m;

    Vector3 Pick(int i)
    {
        float r = R(i, 8);
        return r < .46f ? s.Pal.A : r < .8f ? s.Pal.B : s.Pal.C;
    }

    PvMesh MeshOf(int i) => s.Solid switch
    {
        PvSolid.Cube or PvSolid.Plate => PvMesh.Cube,
        PvSolid.Tetra => PvMesh.Tetra,
        PvSolid.Octa => PvMesh.Octa,
        PvSolid.Icosa => PvMesh.Icosa,
        PvSolid.Prism3 => PvMesh.Prism3,
        PvSolid.Prism6 => PvMesh.Prism6,
        PvSolid.Torus => PvMesh.Torus,
        PvSolid.Pyramid => PvMesh.Pyramid,
        PvSolid.StarPrism => PvMesh.StarPrism,
        PvSolid.Cross => PvMesh.Cross,
        _ => PvMesh.Mix[(int)(R(i, 9) * PvMesh.Mix.Length) % PvMesh.Mix.Length],
    };

    /// <summary>四角い柱を使う並び方の立体(混ぜるのときは立方体か六角柱)</summary>
    PvMesh BlockOf(int i) => s.Solid switch
    {
        PvSolid.Mix or PvSolid.Plate or PvSolid.Torus or PvSolid.Tetra or PvSolid.Octa or PvSolid.Icosa => PvMesh.Cube,
        PvSolid.Prism3 => PvMesh.Prism3,
        PvSolid.Prism6 => PvMesh.Prism6,
        _ => PvMesh.Cube,
    };

    Vector3 Shape(float x) => s.Solid == PvSolid.Plate ? new(x * 1.45f, x * 1.1f, x * .16f) : new(x, x, x);

    Quaternion Tumble(int i)
    {
        var ax = new Vector3(R(i, 3) - .5f, R(i, 4) - .5f, R(i, 5) - .5f);
        ax = ax.LengthSquared() < 1e-4f ? Vector3.UnitY : Vector3.Normalize(ax);
        return Quaternion.CreateFromAxisAngle(ax, R(i, 6) * MathF.Tau + s.T * s.Spin * (.25f + R(i, 7) * .75f));
    }

    Vector3 Push(Vector3 p)
    {
        if (s.Clear <= 0f || p.Z < .1f) return p;
        float ax = p.X / p.Z / .78f, ay = p.Y / p.Z / .44f, l = ax * ax + ay * ay;
        if (l >= s.Clear * s.Clear) return p;
        if (l < 1e-5f) return new(p.X + s.Clear * .78f * p.Z, p.Y, p.Z);
        float k = s.Clear / MathF.Sqrt(l);
        return new(p.X * k, p.Y * k, p.Z);
    }

    /// <summary>
    /// 立体を2Dに直さず、3Dの形のまま n に入れる。カメラから見た空間を、そのまま3D描画の世界(px)にする:
    /// 奥行き8の所が画面(Z=0)になり、元の絵と同じカメラで見ると、2Dのときと同じ見え方になる
    /// </summary>
    public void BuildNative(Native3D n, bool back)
    {
        nat = n;
        float f = s.H * .98f * s.Zoom;
        natK = f / 8f;
        n.Cam = f; n.Width = s.W; n.Height = s.H;
        n.PivotZ = 12f * natK - f;
        if (back)
        {
            // 背景: いちばん奥に置く大きな板(回しても端が見えにくいよう、上下左右に同じ色で広げる)
            float wz = (Far + 6f) * natK - f, sc = (wz + f) / f;
            float hw = s.W / 2f * sc, hh = s.H / 2f * sc, ew = hw * 1.4f, eh = hh * 1.4f;
            var top = V4(s.Pal.Back1, s.Alpha); var bottom = V4(s.Pal.Back2, s.Alpha);
            n.Begin();
            n.Quad(new(-ew, eh, wz), new(ew, eh, wz), new(ew, hh, wz), new(-ew, hh, wz), top, top);
            n.Quad(new(-ew, hh, wz), new(ew, hh, wz), new(ew, -hh, wz), new(-ew, -hh, wz), top, bottom);
            n.Quad(new(-ew, -hh, wz), new(ew, -hh, wz), new(ew, -eh, wz), new(-ew, -eh, wz), bottom, bottom);
            n.End();
        }
        Build();
        nat = null;
    }

    static Vector4 V4(Vector3 c, float a) => new(Math.Clamp(c.X, 0f, 1f), Math.Clamp(c.Y, 0f, 1f), Math.Clamp(c.Z, 0f, 1f), Math.Clamp(a, 0f, 1f));

    /// <summary>カメラから見た空間(奥が +Z) → 3D描画の世界(px)</summary>
    Vector3 NW(Vector3 v) => new(v.X * natK, v.Y * natK, v.Z * natK - nat!.Cam);

    public void Build()
    {
        view = Quaternion.Conjugate(Quaternion.CreateFromYawPitchRoll(s.Yaw, s.Pitch, s.Roll));
        eye = Vector3.Zero;
        switch (s.Scene)
        {
            case PvScene.Wave: Wave(); break;
            case PvScene.City: City(); break;
            case PvScene.Tunnel: Tunnel(); break;
            case PvScene.Rings: Rings(); break;
            case PvScene.Spiral: Spiral(); break;
            case PvScene.Sphere: Sphere(); break;
            case PvScene.Stairs: Stairs(); break;
            case PvScene.Valley: Valley(); break;
            case PvScene.Wall: Wall(); break;
            case PvScene.Corridor: Corridor(); break;
            case PvScene.Arches: Arches(); break;
            case PvScene.Galaxy: Galaxy(); break;
            case PvScene.Drum: Drum(); break;
            case PvScene.Ripple: Ripple(); break;
            case PvScene.Pyramid: PyramidScene(); break;
            case PvScene.Rain: RainScene(); break;
            case PvScene.Rise: Rise(); break;
            case PvScene.Orbit: Orbit(); break;
            case PvScene.Dna: Dna(); break;
            case PvScene.CrossFlow: CrossFlow(); break;
            case PvScene.Burst: Burst(); break;
            case PvScene.Bars: Bars(); break;
            case PvScene.Checker: Checker(); break;
            case PvScene.Panels: Panels(); break;
            case PvScene.Mountains: Mountains(); break;
            case PvScene.Ocean: Ocean(); break;
            case PvScene.Cave: Cave(); break;
            case PvScene.Shards: Shards(); break;
            case PvScene.Nest: Nest(); break;
            default: Float(); break;
        }
        faces.Sort(static (a, b) => b.Z.CompareTo(a.Z));
    }

    void Tilt(float pitch, float y)
    {
        view = Quaternion.Conjugate(Quaternion.CreateFromYawPitchRoll(s.Yaw, s.Pitch + pitch, s.Roll));
        eye = new(0f, y, 0f);
    }

    void Float()
    {
        const float D = 34f;
        for (int i = 0; i < s.N; i++)
        {
            float z = Mod(R(i, 0) * D - s.Dist * (.7f + R(i, 2) * .3f), D) + 2.4f;
            var p = Push(new Vector3((R(i, 1) - .5f) * 2.1f * z * s.Spread, (R(i, 10) - .5f) * 1.25f * z * s.Spread, z));
            Add(MeshOf(i), p, Tumble(i), Shape((.38f + R(i, 11) * .62f) * s.Size), Pick(i));
        }
    }

    void Wave()
    {
        Tilt(.52f, 7.2f);
        int n = Math.Clamp((int)MathF.Sqrt(s.N * 3.6f), 6, 34);
        float g = 1.5f * MathF.Max(.5f, s.Spread), off = Mod(s.Dist, g);
        int row0 = (int)MathF.Floor(s.Dist / g);
        for (int j = 0; j < n + 6; j++)
            for (int k = -n / 2 - 2; k <= n / 2 + 2; k++)
            {
                float x = k * g, z = 1f + j * g - off;
                float wz = (j + row0) * g, d = MathF.Sqrt(x * x + (wz - s.Dist - 11f) * (wz - s.Dist - 11f));
                float y = MathF.Sin(d * .62f - s.T * 2.1f * s.Spin) * 1.15f + MathF.Sin(x * .34f + wz * .41f + s.T * .8f) * .6f;
                int id = (j + row0) * 97 + k * 5003;
                float q = .5f + .5f * MathF.Sin(d * .36f - s.T * .9f);
                var col = q < .5f ? Vector3.Lerp(s.Pal.A, s.Pal.B, q * 2f) : Vector3.Lerp(s.Pal.B, s.Pal.C, q * 2f - 1f);
                float h = 2.4f;
                Add(BlockOf(id), new(x, y - h, z), Quaternion.Identity, new(g * .47f * MathF.Min(1.4f, s.Size), h, g * .47f * MathF.Min(1.4f, s.Size)), col);
            }
    }

    void City()
    {
        Tilt(.13f, 2.2f);
        const float D = 38f;
        int n = Math.Clamp(s.N, 8, 400);
        for (int i = 0; i < n; i++)
        {
            int side = i % 2 == 0 ? -1 : 1;
            float z = Mod(i * D / n + R(i, 0) * 1.5f - s.Dist, D) + 1f;
            float x = side * (3.4f + R(i, 1) * 9f + MathF.Max(0f, s.Clear - .5f) * 2f) * s.Spread;
            float hh = (1.2f + R(i, 2) * R(i, 3) * 9f) * MathF.Max(.3f, s.Size);
            float bw = (.65f + R(i, 4) * .75f) * MathF.Min(1.5f, s.Size);
            Add(BlockOf(i), new(x, hh - 3f, z), Quaternion.Identity, new(bw, hh, bw), Pick(i));
            if (R(i, 5) > .52f) Add(PvMesh.Cube, new(x, 2f * hh - 2.86f, z), Quaternion.CreateFromAxisAngle(Vector3.UnitY, s.T * s.Spin), new(bw * .55f, .14f, bw * .55f), s.Pal.C);
        }
        for (int i = 0; i < 16; i++)
        {
            float z = Mod(i * D / 16f - s.Dist, D) + 1f;
            Add(PvMesh.Cube, new(0f, -3.02f, z), Quaternion.Identity, new(.09f, .02f, .68f), s.Pal.C);
        }
    }

    void Tunnel()
    {
        const float D = 36f;
        float rad = 5.8f * s.Spread;
        for (int i = 0; i < s.N; i++)
        {
            float a = i * 2.39996f + s.T * s.Spin * .16f;
            float z = Mod(i * D / s.N - s.Dist, D) + 1f;
            float q = rad * (1f + (R(i, 1) - .5f) * .22f);
            var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, a);
            float sz = (.55f + R(i, 2) * .75f) * s.Size;
            var dim = s.Solid == PvSolid.Mix ? new Vector3(sz * 1.25f, sz * .55f, sz * 1.5f) : Shape(sz);
            Add(s.Solid == PvSolid.Mix ? PvMesh.Cube : MeshOf(i), new(MathF.Cos(a + MathF.PI / 2f) * q, MathF.Sin(a + MathF.PI / 2f) * q, z), rot, dim, Pick(i));
        }
    }

    void Rings()
    {
        const float D = 36f;
        int m = Math.Clamp(s.N / 7, 5, 20), seg = 14;
        float gap = D / m, rad = 5.4f * s.Spread;
        int base0 = (int)MathF.Floor(s.Dist / gap);
        for (int j = 0; j < m; j++)
        {
            float z = 1f + j * gap - Mod(s.Dist, gap);
            int id = j + base0;
            float tw = (id % 2 == 0 ? 1f : -1f) * s.T * s.Spin * .3f + id * .31f;
            for (int k = 0; k < seg; k++)
            {
                if ((k + id) % 7 == 0) continue;
                float a = k * MathF.Tau / seg + tw;
                var rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, a);
                float bw = rad * MathF.PI / seg * .9f;
                var col = (k + id) % 3 == 0 ? s.Pal.A : (k + id) % 3 == 1 ? s.Pal.B : s.Pal.C;
                Add(PvMesh.Cube, new(-MathF.Sin(a) * rad, MathF.Cos(a) * rad, z), rot, new(bw, .32f * s.Size, .42f * s.Size), col);
            }
        }
    }

    void Spiral()
    {
        const float D = 34f;
        float rad = 4.6f * s.Spread;
        for (int i = 0; i < s.N; i++)
        {
            int arm = i % 2;
            float u = Mod(i * D / s.N - s.Dist, D);
            float a = u * .55f + arm * MathF.PI + s.T * s.Spin * .45f;
            var p = new Vector3(MathF.Cos(a) * rad, MathF.Sin(a) * rad, u + 1f);
            Add(MeshOf(i), p, Tumble(i), Shape((.46f + R(i, 2) * .3f) * s.Size), arm == 0 ? s.Pal.A : s.Pal.B);
            if (i % 5 == 0)
            {
                var mid = new Vector3(p.X * 1.34f, p.Y * 1.34f, p.Z);
                Add(PvMesh.Octa, mid, Tumble(i + 900), Shape(.22f * s.Size), s.Pal.C);
            }
        }
    }

    void Sphere()
    {
        float rad = 6.3f * s.Spread, cz = 12.5f;
        var spin = Quaternion.CreateFromYawPitchRoll(s.T * s.Spin * .28f + s.Dist * .05f, MathF.Sin(s.T * .3f) * .3f, 0f);
        for (int i = 0; i < s.N; i++)
        {
            float y = 1f - (i + .5f) * 2f / s.N, r = MathF.Sqrt(MathF.Max(0f, 1f - y * y)), a = i * 2.39996f;
            var dir = Vector3.Transform(new Vector3(MathF.Cos(a) * r, y, MathF.Sin(a) * r), spin);
            float pulse = 1f + MathF.Sin(s.T * 2f + i * .37f) * .07f + s.Flash * .08f;
            var p = dir * rad * pulse + new Vector3(0f, 0f, cz);
            var rot = Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, dir) + new Vector3(1e-3f, 0f, 0f)), MathF.Acos(Math.Clamp(dir.Z, -1f, 1f)));
            float sz = (.34f + R(i, 2) * .34f) * s.Size * MathF.Min(1.8f, 14f / MathF.Max(20f, s.N) * 3f);
            Add(s.Solid == PvSolid.Mix ? PvMesh.Cube : MeshOf(i), p, rot, s.Solid == PvSolid.Mix ? new(sz, sz, sz * 1.35f) : Shape(sz), Pick(i));
        }
    }

    void Stairs()
    {
        Tilt(-.1f, -1.2f);
        const float step = 1.25f, rise = .56f;
        int n = Math.Clamp(s.N / 2, 10, 60);
        float m = Mod(s.Dist, step * 2f);
        int base0 = (int)MathF.Floor(s.Dist / (step * 2f)) * 2;
        foreach (int side in new[] { -1, 1 })
        {
            for (int i = 0; i < n; i++)
            {
                int id = i + base0;
                float z = 1f + i * step - m, y = -3.6f + (i - m / step) * rise;
                float x = side * (5.2f + MathF.Sin(id * .22f) * .7f) * s.Spread;
                float w0 = 2.6f * MathF.Max(.5f, s.Size);
                var col = id % 4 == 0 ? s.Pal.C : id % 2 == 0 ? s.Pal.A : s.Pal.B;
                Add(PvMesh.Cube, new(x, y, z), Quaternion.Identity, new(w0, rise * .5f, step * .5f), col);
                if (id % 5 == 0) Add(MeshOf(id), new(x, y + 1f + MathF.Sin(s.T * 1.6f + id) * .28f, z), Tumble(id), Shape(.4f * s.Size), s.Pal.C);
            }
        }
    }

    float Terrain(float x, float wz)
    {
        float a = MathF.Max(0f, MathF.Abs(x) - 1.4f - s.Clear * 1.4f);
        float h = a * .42f * (1f + .26f * MathF.Sin(wz * .41f + x * .3f));
        h += a > 0f ? (MathF.Sin(x * .9f + wz * .63f) + MathF.Sin(x * .37f - wz * .29f + 1.7f)) * MathF.Min(1f, a * .4f) * .62f : 0f;
        return h * s.Size;
    }

    void Valley()
    {
        Tilt(.1f, 2.5f);
        int cols = Math.Clamp((int)MathF.Sqrt(s.N * 6f), 10, 34), rows = Math.Clamp(cols * 3 / 4, 10, 26);
        float g = 1.45f * s.Spread, off = Mod(s.Dist, g);
        int r0 = (int)MathF.Floor(s.Dist / g);
        Span<Vector3> tri = stackalloc Vector3[3];
        for (int j = 0; j < rows; j++)
            for (int k = -cols / 2; k < cols / 2; k++)
            {
                Vector3 V(int a, int b) { float x = a * g, wz = (b + r0) * g; return new(x, Terrain(x, wz) - .4f, 1f + b * g - off); }
                var p00 = V(k, j); var p10 = V(k + 1, j); var p01 = V(k, j + 1); var p11 = V(k + 1, j + 1);
                for (int q = 0; q < 2; q++)
                {
                    tri[0] = q == 0 ? p00 : p10; tri[1] = q == 0 ? p01 : p01; tri[2] = q == 0 ? p10 : p11;
                    if (Vector3.Cross(tri[1] - tri[0], tri[2] - tri[0]).Y < 0f) { var x = tri[1]; tri[1] = tri[2]; tri[2] = x; }
                    float hh = Math.Clamp((tri[0].Y + tri[1].Y + tri[2].Y) / 3f / 3.2f, 0f, 1f);
                    var col = hh < .5f ? Vector3.Lerp(s.Pal.A * .55f, s.Pal.A, hh * 2f) : Vector3.Lerp(s.Pal.A, s.Pal.B, hh * 2f - 1f);
                    Emit(tri, col);
                }
            }
        float sunY = 4.4f;
        Span<Vector3> sun = stackalloc Vector3[20];
        for (int i = 0; i < 20; i++) { float a = i * MathF.Tau / 20f; sun[i] = new(MathF.Cos(a) * 8f, sunY + MathF.Sin(a) * 8f, Far - 3f); }
        if (nat is not null)
        {
            Span<Vector3> sw = stackalloc Vector3[20];
            for (int i = 0; i < 20; i++) sw[i] = NW(sun[i]);
            nat.Poly(sw, new Vector3(0, 0, -1), V4(s.Pal.C, .9f * s.Alpha));
        }
        else faces.Add(new Face(Project(sun), Far + 1f, s.Pal.C, .9f));
    }

    void Wall()
    {
        int cols = Math.Clamp((int)MathF.Sqrt(s.N * 3.2f), 7, 32), rows = Math.Max(4, cols * 9 / 16 + 1);
        float g = 2f * s.Spread, z0 = 13f;
        for (int j = -rows / 2 - 1; j <= rows / 2 + 1; j++)
            for (int k = -cols / 2 - 1; k <= cols / 2 + 1; k++)
            {
                float x = k * g, y = j * g, wv = s.Dist * .34f;
                float pop = MathF.Sin(x * .46f + wv + s.T * s.Spin * .8f) * MathF.Cos(y * .52f - wv * .6f) * 1.5f + MathF.Sin((x + y) * .24f + s.T * .7f) * .75f;
                float cd = MathF.Sqrt(x * x / 42f + y * y / 16f);
                float rec = MathF.Max(0f, s.Clear - cd) * 5f;
                int id = j * 991 + k * 37;
                float q = .5f + .5f * MathF.Sin(x * .23f - y * .31f + s.T * .4f);
                var col = q < .5f ? Vector3.Lerp(s.Pal.A, s.Pal.B, q * 2f) : Vector3.Lerp(s.Pal.B, s.Pal.C, q * 2f - 1f);
                float d = 2.2f;
                Add(BlockOf(id), new(x, y, z0 - pop + rec + d), Quaternion.Identity, new(g * .47f * MathF.Min(1.2f, s.Size), g * .47f * MathF.Min(1.2f, s.Size), d), col);
            }
    }

    Vector3 Ramp(float q)
    {
        q = Math.Clamp(q, 0f, 1f);
        return q < .5f ? Vector3.Lerp(s.Pal.A, s.Pal.B, q * 2f) : Vector3.Lerp(s.Pal.B, s.Pal.C, q * 2f - 1f);
    }

    static Quaternion AxisY(float a) => Quaternion.CreateFromAxisAngle(Vector3.UnitY, a);
    static Quaternion AxisZ(float a) => Quaternion.CreateFromAxisAngle(Vector3.UnitZ, a);
    static Quaternion AxisX(float a) => Quaternion.CreateFromAxisAngle(Vector3.UnitX, a);

    /// <summary>高さの付いた三角形の面(地面・海・天井)</summary>
    void Sheet(float baseY, bool up, float g, Func<float, float, float> height)
    {
        int cols = Math.Clamp((int)MathF.Sqrt(s.N * 6f), 10, 34), rows = Math.Clamp(cols * 3 / 4, 10, 26);
        float off = Mod(s.Dist, g); int r0 = (int)MathF.Floor(s.Dist / g);
        Span<Vector3> tri = stackalloc Vector3[3];
        for (int j = 0; j < rows; j++)
            for (int k = -cols / 2; k < cols / 2; k++)
            {
                Vector3 V(int a, int b) { float x = a * g, h = height(x, (b + r0) * g); return new(x, baseY + (up ? h : -h), 1f + b * g - off); }
                var p00 = V(k, j); var p10 = V(k + 1, j); var p01 = V(k, j + 1); var p11 = V(k + 1, j + 1);
                for (int q = 0; q < 2; q++)
                {
                    tri[0] = q == 0 ? p00 : p10; tri[1] = p01; tri[2] = q == 0 ? p10 : p11;
                    float ny = Vector3.Cross(tri[1] - tri[0], tri[2] - tri[0]).Y;
                    if (up ? ny < 0f : ny > 0f) { var x = tri[1]; tri[1] = tri[2]; tri[2] = x; }
                    Emit(tri, Ramp(MathF.Abs((tri[0].Y + tri[1].Y + tri[2].Y) / 3f - baseY) / 2.6f));
                }
            }
    }

    void Corridor()
    {
        float g = 2f * s.Spread, off = Mod(s.Dist, g); int r0 = (int)MathF.Floor(s.Dist / g), rows = Math.Clamp(s.N / 4, 8, 22);
        float size = g * .47f * MathF.Min(1.3f, s.Size);
        for (int j = 0; j < rows; j++)
        {
            float z = 1f + j * g - off; int id = j + r0;
            for (int k = -3; k <= 3; k++)
            {
                float q = .5f + .5f * MathF.Sin(id * .6f + k * .9f + s.T * 1.6f * s.Spin), h = .15f + q * .3f;
                Add(PvMesh.Cube, new(k * g, -3.8f * s.Spread, z), Quaternion.Identity, new(size, h, size), Ramp(q));
                Add(PvMesh.Cube, new(k * g, 3.8f * s.Spread, z), Quaternion.Identity, new(size, h, size), Ramp(1f - q));
            }
            for (int k = -1; k <= 1; k++)
                foreach (int side in new[] { -1, 1 })
                {
                    float q = .5f + .5f * MathF.Sin(id * .5f + k * 1.4f - s.T * 1.3f * s.Spin + side);
                    Add(PvMesh.Cube, new(side * 7.2f * s.Spread, k * g, z), Quaternion.Identity, new(.15f + q * .35f, size, size), Ramp(q));
                }
        }
    }

    void Arches()
    {
        const float D = 36f; int m = Math.Clamp(s.N / 7, 5, 18), seg = 11;
        float gap = D / m, rad = 6.2f * s.Spread; int r0 = (int)MathF.Floor(s.Dist / gap);
        for (int j = 0; j < m; j++)
        {
            float z = 1f + j * gap - Mod(s.Dist, gap); int id = j + r0;
            for (int k = 0; k < seg; k++)
            {
                float a = k * MathF.PI / (seg - 1);
                Add(PvMesh.Cube, new(MathF.Cos(a) * rad, MathF.Sin(a) * rad - 2.8f, z), AxisZ(a), new(.3f * s.Size, rad * .17f, .42f * s.Size), Ramp((k + id) % 5 / 4f));
            }
            foreach (int side in new[] { -1, 1 }) Add(PvMesh.Cube, new(side * rad, -5.2f, z), Quaternion.Identity, new(.34f * s.Size, 2.4f, .46f * s.Size), s.Pal.C);
            Add(PvMesh.Cube, new(0, -7.5f, z), Quaternion.Identity, new(rad, .1f, gap * .42f), id % 2 == 0 ? s.Pal.A : s.Pal.B);
        }
    }

    void Galaxy()
    {
        const float tilt = .62f; float sn = MathF.Sin(tilt), cs = MathF.Cos(tilt);
        for (int i = 0; i < s.N * 2; i++)
        {
            float u = (i + .5f) / (s.N * 2f), r = (1.2f + u * 9.5f) * s.Spread, a = i % 3 * MathF.Tau / 3f + u * 5.2f + (R(i, 1) - .5f) * .55f + s.T * s.Spin * .22f + s.Dist * .025f;
            float x = MathF.Cos(a) * r, w = MathF.Sin(a) * r, y = (R(i, 2) - .5f) * (1f - u) * 1.3f;
            Add(MeshOf(i), new(x, y * cs + w * sn, 14f - y * sn + w * cs), Tumble(i), Shape((.12f + (1f - u) * .34f) * s.Size), Ramp(u + (R(i, 3) - .5f) * .2f));
        }
        Add(PvMesh.Icosa, new(0, 0, 14f), AxisY(s.T * s.Spin * .4f), new(1.05f * s.Size), s.Pal.C);
    }

    void Drum()
    {
        int rows = Math.Clamp((int)MathF.Sqrt(s.N / 2.6f), 3, 12), cols = rows * 3 + 4; float rad = 6.6f * s.Spread, g = 12f * s.Spread / rows;
        for (int j = 0; j < rows; j++)
            for (int k = 0; k < cols; k++)
            {
                float a = k * MathF.Tau / cols + (j % 2 == 0 ? 1f : -1f) * s.T * s.Spin * .24f + s.Dist * .03f, y = (j - (rows - 1) / 2f) * g;
                Add(BlockOf(k + j * 31), new(MathF.Sin(a) * rad, y, 13f + MathF.Cos(a) * rad), AxisY(a), new(rad * MathF.PI / cols * .86f, g * .43f, .36f * s.Size), Ramp(.5f + .5f * MathF.Sin(k * .7f + j * .5f + s.T)));
            }
    }

    void Ripple()
    {
        Tilt(.56f, 7.4f); int rings = Math.Clamp((int)MathF.Sqrt(s.N * .8f), 5, 18); float g = 1.45f * s.Spread;
        for (int j = 0; j < rings; j++)
        {
            int c = Math.Max(1, j * 7); float r = j * g;
            for (int k = 0; k < c; k++)
            {
                float a = k * MathF.Tau / c + j * .2f, q = MathF.Sin(r * .9f - s.T * 2.8f * s.Spin - s.Dist * .4f), h = 2.1f;
                Add(BlockOf(j * 37 + k), new(MathF.Cos(a) * r, q * .9f - h, 13f + MathF.Sin(a) * r), AxisY(-a), new(g * .44f * MathF.Min(1.3f, s.Size), h, g * .44f * MathF.Min(1.3f, s.Size)), Ramp(.5f + q * .5f));
            }
        }
    }

    void PyramidScene()
    {
        int lv = Math.Clamp((int)MathF.Pow(s.N * .75f, 1f / 3f) + 3, 4, 10); float g = 1.05f * s.Spread; var turn = AxisY(s.T * s.Spin * .3f + s.Dist * .03f);
        for (int l = 0; l < lv; l++)
        {
            int w = lv - l;
            for (int a = -w + 1; a < w; a += 2)
                for (int b = -w + 1; b < w; b += 2)
                {
                    if (Math.Abs(a) != w - 1 && Math.Abs(b) != w - 1) continue;
                    float pop = MathF.Max(0f, MathF.Sin(s.T * 2.1f + a * .5f + b * .7f + l)) * .16f;
                    Add(BlockOf(l * 41 + a * 7 + b), Vector3.Transform(new Vector3(a * g, l * g * 1.25f - 3.6f + pop, b * g), turn) + new Vector3(0, 0, 15f), turn, new(g * .47f * MathF.Min(1.2f, s.Size), g * .6f, g * .47f * MathF.Min(1.2f, s.Size)), Ramp(l / (lv - 1f)));
                }
        }
    }

    void RainScene()
    {
        const float H = 18f;
        for (int i = 0; i < s.N * 2; i++)
        {
            float z = 2.6f + R(i, 0) * 28f, y = H / 2f - Mod(R(i, 1) * H + s.T * s.Spin * (5f + R(i, 2) * 6f) + s.Dist * .5f, H);
            var p = Push(new((R(i, 3) - .5f) * 2.1f * z * s.Spread, y * (.35f + z * .07f), z));
            Add(s.Solid == PvSolid.Mix ? PvMesh.Prism6 : MeshOf(i), p, AxisX(1.5708f), new(.09f * s.Size, .09f * s.Size, (.5f + R(i, 4) * .9f) * s.Size), Pick(i));
        }
    }

    void Rise()
    {
        const float H = 22f;
        for (int i = 0; i < s.N; i++)
        {
            float z = 3f + R(i, 0) * 26f, u = Mod(R(i, 1) * H + s.T * (.8f + R(i, 2) * 1.6f) * MathF.Max(.25f, MathF.Abs(s.Spin)) + s.Dist * .18f, H) - H / 2f;
            var p = Push(new((R(i, 3) - .5f + MathF.Sin(s.T * .7f + i) * .025f) * 2.1f * z * s.Spread, u * (.28f + z * .05f), z));
            Add(MeshOf(i), p, Tumble(i), Shape((.2f + R(i, 11) * .42f) * s.Size), Pick(i));
        }
    }

    void Orbit()
    {
        var c = new Vector3(0, 0, 15f); Add(PvMesh.Icosa, c, AxisY(s.T * s.Spin * .35f), new(1.65f * s.Size), s.Pal.C);
        int rings = 5, per = Math.Max(5, s.N / rings);
        for (int j = 0; j < rings; j++)
        {
            var tilt = Quaternion.CreateFromYawPitchRoll(j * 1.13f + s.Dist * .02f, .55f + j * .34f, j * .62f); float r = (3.3f + j * 1.55f) * s.Spread;
            for (int k = 0; k < per; k++)
            {
                float a = k * MathF.Tau / per + (j % 2 == 0 ? 1f : -1f) * s.T * s.Spin * (.5f - j * .05f);
                Add(MeshOf(j * 53 + k), Vector3.Transform(new Vector3(MathF.Cos(a) * r, 0, MathF.Sin(a) * r), tilt) + c, Tumble(j * 53 + k), Shape(.24f * s.Size), j % 3 == 0 ? s.Pal.A : j % 3 == 1 ? s.Pal.B : s.Pal.C);
            }
        }
    }

    void Dna()
    {
        int n = Math.Clamp(s.N / 2, 14, 80); float span = 26f * s.Spread, rad = 2.6f * s.Spread;
        for (int i = 0; i < n; i++)
        {
            float u = i / (float)n, x = Mod(u * span - s.Dist * .55f, span) - span / 2f, a = x * .62f + s.T * s.Spin * .9f;
            var p0 = new Vector3(x, MathF.Cos(a) * rad, 11f + MathF.Sin(a) * rad); var p1 = new Vector3(x, -p0.Y, 22f - p0.Z);
            Add(s.Solid == PvSolid.Mix ? PvMesh.Icosa : MeshOf(i), p0, Tumble(i), Shape(.34f * s.Size), s.Pal.A);
            Add(s.Solid == PvSolid.Mix ? PvMesh.Icosa : MeshOf(i + 200), p1, Tumble(i + 200), Shape(.34f * s.Size), s.Pal.B);
            if (i % 2 == 0)
            {
                var d = p1 - p0; float l = d.Length();
                if (l > .2f) Add(PvMesh.Cube, (p0 + p1) * .5f, AxisX(MathF.Atan2(d.Z, d.Y)), new(.06f, l * .5f, .06f), s.Pal.C);
            }
        }
    }

    void CrossFlow()
    {
        const float L = 34f;
        for (int i = 0; i < s.N; i++)
        {
            int side = i % 2 == 0 ? 1 : -1; float u = Mod(R(i, 0) * L + s.Dist * (.8f + R(i, 2) * .4f), L) - L / 2f, off = (R(i, 3) - .5f) * 5f * s.Spread, h = (R(i, 4) - .5f) * 8f * s.Spread;
            var p = Push(new(side * u * .78f + off * .62f, h, 15f + u * .62f * side * side - off * .78f * side));
            Add(MeshOf(i), p, Tumble(i), Shape((.25f + R(i, 11) * .46f) * s.Size), side > 0 ? s.Pal.A : s.Pal.B);
        }
    }

    void Burst()
    {
        var c = new Vector3(0, 0, 16f);
        for (int i = 0; i < s.N; i++)
        {
            float y = 1f - (i + .5f) * 2f / s.N, q = MathF.Sqrt(MathF.Max(0f, 1f - y * y)), a = i * 2.39996f, u = Mod(R(i, 0) + s.T * s.Spin * .22f + s.Dist * .012f, 1f);
            var dir = new Vector3(MathF.Cos(a) * q, y, MathF.Sin(a) * q); float d = (1f + u * u * 13f) * s.Spread;
            Add(MeshOf(i), dir * d + c, Tumble(i), Shape((.12f + u * .5f) * s.Size), Ramp(u));
        }
    }

    void Bars()
    {
        Tilt(.16f, 1.8f); int n = Math.Clamp(s.N / 2, 12, 64); float g = 34f * s.Spread / n;
        for (int r = 0; r < 3; r++)
            for (int i = 0; i < n; i++)
            {
                float x = (i - (n - 1) / 2f) * g, q = MathF.Abs(MathF.Sin(i * .43f + s.T * 2.8f * s.Spin + r)) * .62f + MathF.Abs(MathF.Sin(i * 1.1f - s.T * 4.6f * s.Spin)) * .38f, h = (.35f + q * 3.6f) * MathF.Max(.4f, s.Size);
                Add(BlockOf(i + r * 300), new(x, h - 3.4f, 9f + r * 5f), Quaternion.Identity, new(g * .4f, h, g * .4f), Ramp(q));
            }
    }

    void Checker()
    {
        Tilt(.5f, 6.6f); int n = Math.Clamp((int)MathF.Sqrt(s.N * 3.6f), 6, 30); float g = 1.7f * s.Spread, off = Mod(s.Dist, g); int r0 = (int)MathF.Floor(s.Dist / g);
        for (int j = 0; j < n + 4; j++)
            for (int k = -n / 2 - 2; k <= n / 2 + 2; k++)
            {
                int id = j + r0; float wave = MathF.Sin(k * .32f + id * .36f - s.T * 1.7f * s.Spin), flip = MathF.Max(0f, wave) * MathF.PI;
                Add(PvMesh.Cube, new(k * g, -.5f, 1f + j * g - off), AxisX(flip), new(g * .47f, .09f, g * .47f), ((k + id) & 1) == 0 ^ wave > .2f ? s.Pal.A : s.Pal.B);
            }
    }

    void Panels()
    {
        const float D = 28f;
        for (int i = 0; i < s.N; i++)
        {
            float z = Mod(R(i, 0) * D - s.Dist * .75f, D) + 3f;
            var p = Push(new((R(i, 1) - .5f) * 2f * z * s.Spread, (R(i, 2) - .5f) * 1.2f * z * s.Spread, z));
            var q = Quaternion.CreateFromYawPitchRoll(MathF.Sin(s.T * s.Spin * .8f + i) * .5f, MathF.Cos(s.T * s.Spin * .6f + i * 1.3f) * .35f, (R(i, 3) - .5f) * .3f);
            Add(PvMesh.Cube, p, q, new((.55f + R(i, 4) * .85f) * s.Size, (.34f + R(i, 5) * .5f) * s.Size, .035f), Pick(i));
        }
    }

    void Mountains()
    {
        Tilt(.09f, 3.1f); const float D = 40f; int n = Math.Clamp(s.N, 14, 200);
        for (int i = 0; i < n; i++)
        {
            float z = Mod(R(i, 0) * D - s.Dist, D) + 2f, a = (R(i, 1) - .5f) * 2f, x = (a < 0f ? -1f : 1f) * (2.8f + MathF.Abs(a) * 19f + s.Clear) * s.Spread, h = (1.3f + R(i, 2) * 2.7f + MathF.Abs(a) * 1.4f) * s.Size, w = h * (.68f + R(i, 3) * .3f);
            Add(PvMesh.Pyramid, new(x, h * .9f - 3.2f, z), AxisY(R(i, 4) * 3f), new(w, h, w), Ramp(R(i, 5) * .6f + MathF.Abs(a) * .4f));
        }
        Add(PvMesh.Cube, new(0, -3.3f, 21f), Quaternion.Identity, new(44f * s.Spread, .03f, 21f), Vector3.Lerp(s.Pal.A, s.Pal.Back2, .72f));
    }

    void Ocean()
    {
        Tilt(.15f, 3.1f);
        Sheet(-.35f, true, 1.4f * s.Spread, (x, w) => 1.2f + (MathF.Sin(x * .36f + w * .23f + s.T * 1.25f * s.Spin) + MathF.Sin(x * -.21f + w * .47f - s.T * .9f * s.Spin) * .62f + MathF.Sin(x * .83f + w * .75f + s.T * 1.9f) * .22f) * .78f);
    }

    void Cave()
    {
        float F(float x, float w) => MathF.Max(0f, MathF.Abs(x) - 1.6f - s.Clear) * .34f * (1f + .34f * MathF.Sin(w * .57f + x)) + (MathF.Sin(x * 1.13f + w * .77f) + MathF.Sin(x * .41f - w * .39f)) * .34f + .5f;
        Sheet(-2.8f, true, 1.35f * s.Spread, F);
        Sheet(2.8f, false, 1.35f * s.Spread, (x, w) => F(x * 1.12f + 3f, w * .9f + 7f));
    }

    void Shards()
    {
        var c = new Vector3(0, 0, 13f); float cycle = Mod(s.T * MathF.Max(.15f, MathF.Abs(s.Spin)) * .24f + s.Dist * .008f, 1f), burst = cycle < .55f ? 0f : (cycle - .55f) / .45f;
        burst = burst * burst * (3f - 2f * burst);
        for (int i = 0; i < s.N; i++)
        {
            float y = 1f - (i + .5f) * 2f / s.N, q = MathF.Sqrt(MathF.Max(0f, 1f - y * y)), a = i * 2.39996f; var d = new Vector3(MathF.Cos(a) * q, y, MathF.Sin(a) * q);
            var rot = Quaternion.CreateFromAxisAngle(Vector3.Normalize(new(R(i, 3) + .1f, R(i, 4) - .5f, R(i, 5) - .5f)), burst * (2f + R(i, 6) * 5f) + i);
            float sz = MathF.Min(1.1f, 8f / MathF.Sqrt(s.N)) * s.Size;
            Add(s.Solid == PvSolid.Mix ? PvMesh.Tetra : MeshOf(i), d * (4.7f + burst * (2f + R(i, 2) * 9f)) * s.Spread + c, rot, Shape(sz), Pick(i));
        }
    }

    void Nest()
    {
        var c = new Vector3(0, 0, 15f); int lv = Math.Clamp(s.N / 10, 4, 14);
        for (int j = lv - 1; j >= 0; j--)
        {
            float q = (j + 1f) / lv, sz = q * 6.4f * s.Spread * (1f + MathF.Sin(s.T * 1.7f - j * .6f) * .035f);
            var rot = Quaternion.CreateFromYawPitchRoll((j % 2 == 0 ? 1f : -1f) * s.T * s.Spin * (.18f + (1f - q) * .4f) + s.Dist * .02f, .45f + j * .17f, j * .22f);
            float e = MathF.Max(.05f, sz * .055f), h = sz + e;
            foreach (int x in new[] { -1, 1 }) foreach (int y in new[] { -1, 1 })
            {
                var col = Ramp(q);
                Add(PvMesh.Cube, Vector3.Transform(new Vector3(x * sz, y * sz, 0), rot) + c, rot, new(e, e, h), col);
                Add(PvMesh.Cube, Vector3.Transform(new Vector3(x * sz, 0, y * sz), rot) + c, rot, new(e, h, e), col);
                Add(PvMesh.Cube, Vector3.Transform(new Vector3(0, x * sz, y * sz), rot) + c, rot, new(h, e, e), col);
            }
            if (j == 0) Add(MeshOf(0), c, rot, Shape(sz * .68f), s.Pal.C);
        }
    }

    void Add(PvMesh m, Vector3 p, Quaternion q, Vector3 scale, Vector3 col)
    {
        if (p.Z < -3f || p.Z > Far + 4f) return;
        var w = m.V;
        // 手前を通り抜ける並び方は、立体ごとにまとめて薄くする(面ごとだと中が透けて見える)
        bool fly = s.Scene is PvScene.Float or PvScene.Spiral or PvScene.Tunnel or PvScene.Rings or PvScene.Arches or PvScene.Rain or PvScene.Rise or PvScene.CrossFlow or PvScene.Panels or PvScene.Corridor;
        instFade = fly ? Math.Clamp((Vector3.Transform(p - eye, view).Z - 1.7f) / 2.3f, 0f, 1f) : -1f;
        if (instFade == 0f) { instFade = -1f; return; }
        if (nat is not null) { nat.Begin(); natGroup = true; }
        foreach (var f in m.F)
        {
            int n = f.Length;
            for (int i = 0; i < n; i++) buf[i] = Vector3.Transform(w[f[i]] * scale, q) + p;
            Emit(buf.AsSpan(0, n), col);
        }
        if (nat is not null) { nat.End(); natGroup = false; }
        instFade = -1f;
    }

    void Emit(ReadOnlySpan<Vector3> w, Vector3 col)
    {
        if (nat is not null) { EmitNative(w, col); return; }
        var nw = Vector3.Zero;
        for (int i = 0; i < w.Length; i++) nw += Vector3.Cross(w[i], w[(i + 1) % w.Length]);
        float len = nw.Length();
        if (len < 1e-7f) return;
        nw /= len;
        Span<Vector3> v = stackalloc Vector3[w.Length];
        var ctr = Vector3.Zero;
        for (int i = 0; i < w.Length; i++) { v[i] = Vector3.Transform(w[i] - eye, view); ctr += v[i]; }
        ctr /= w.Length;
        if (ctr.Z > Far + 2f) return;
        bool glass = s.Style == PvStyle.Glass;
        var nv = Vector3.Transform(nw, view);
        if (!glass && Vector3.Dot(nv, ctr) >= 0f) return;

        // 手前で切る
        int cn = 0;
        for (int i = 0; i < v.Length; i++)
        {
            var a = v[i]; var b = v[(i + 1) % v.Length];
            bool ia = a.Z >= Near, ib = b.Z >= Near;
            if (ia) clip[cn++] = a;
            if (ia != ib) clip[cn++] = Vector3.Lerp(a, b, (Near - a.Z) / (b.Z - a.Z));
        }
        if (cn < 3) return;
        var pts = Project(clip.AsSpan(0, cn));
        float x0 = float.MaxValue, x1 = float.MinValue, y0 = float.MaxValue, y1 = float.MinValue;
        foreach (var q in pts) { x0 = MathF.Min(x0, q.X); x1 = MathF.Max(x1, q.X); y0 = MathF.Min(y0, q.Y); y1 = MathF.Max(y1, q.Y); }
        if (x1 < -4f || y1 < -4f || x0 > s.W + 4f || y0 > s.H + 4f) return;

        float dif = MathF.Max(0f, Vector3.Dot(nw, Light));
        bool lineArt = s.Style is PvStyle.Wire or PvStyle.Neon or PvStyle.Glass or PvStyle.Dots;
        float lit = lineArt ? 1f - s.Shade * .22f + s.Shade * .22f * dif : 1f - s.Shade + s.Shade * (.3f + .86f * dif);
        float spec = (lineArt ? 0f : MathF.Pow(dif, 7f) * .3f * s.Shade) + s.Flash * .1f;
        var c0 = col * lit + new Vector3(spec);
        float fog = Math.Clamp((ctr.Z - 5f) / (Far - 5f), 0f, 1f) * s.Fog * (lineArt ? .6f : 1f) * (lightBack ? .62f : 1f);
        var far = Vector3.Lerp(s.Pal.Back1, s.Pal.Back2, .55f);
        c0 = Vector3.Lerp(c0, far, fog * (2f - fog) * .92f);
        float na = instFade >= 0f ? instFade : Math.Clamp((ctr.Z - Near) / 1.25f, 0f, 1f), fa = Math.Clamp((Far + 2f - ctr.Z) / 4f, 0f, 1f);
        faces.Add(new Face(pts, ctr.Z, Vector3.Clamp(c0, Vector3.Zero, Vector3.One), na * fa));
    }

    /// <summary>面1つを、2Dに直さず3Dの形のまま入れる(色・かすみ・薄くなり方は2Dのときと同じ)</summary>
    void EmitNative(ReadOnlySpan<Vector3> w, Vector3 col)
    {
        var n = nat!;
        var nw = Vector3.Zero;
        for (int i = 0; i < w.Length; i++) nw += Vector3.Cross(w[i], w[(i + 1) % w.Length]);
        float len = nw.Length();
        if (len < 1e-7f) return;
        nw /= len;
        Span<Vector3> v = stackalloc Vector3[w.Length];
        float cz = 0f;
        for (int i = 0; i < w.Length; i++) { var q = Vector3.Transform(w[i] - eye, view); cz += q.Z; v[i] = NW(q); }
        cz /= w.Length;
        if (cz > Far + 2f) return;

        float dif = MathF.Max(0f, Vector3.Dot(nw, Light));
        bool lineArt = s.Style is PvStyle.Wire or PvStyle.Neon or PvStyle.Glass or PvStyle.Dots;
        float lit = lineArt ? 1f - s.Shade * .22f + s.Shade * .22f * dif : 1f - s.Shade + s.Shade * (.3f + .86f * dif);
        float spec = (lineArt ? 0f : MathF.Pow(dif, 7f) * .3f * s.Shade) + s.Flash * .1f;
        var c0 = col * lit + new Vector3(spec);
        float fog = Math.Clamp((cz - 5f) / (Far - 5f), 0f, 1f) * s.Fog * (lineArt ? .6f : 1f) * (lightBack ? .62f : 1f);
        var far = Vector3.Lerp(s.Pal.Back1, s.Pal.Back2, .55f);
        var c = Vector3.Clamp(Vector3.Lerp(c0, far, fog * (2f - fog) * .92f), Vector3.Zero, Vector3.One);
        float na = instFade >= 0f ? instFade : Math.Clamp((cz - Near) / 1.25f, 0f, 1f), fa = Math.Clamp((Far + 2f - cz) / 4f, 0f, 1f);
        float a = na * fa * s.Alpha;
        if (a <= .004f) return;

        var nv = Vector3.Transform(nw, view);
        // 線の太さ: 2Dでは奥ほど細く描いていた(px)。その太さを、その奥行きでの3Dの太さに直す
        float zz = MathF.Max(cz, .05f), wz = s.Line * Math.Clamp(7f / zz, .45f, 1.6f);
        float Px(float px) => px * zz / 8f;
        var hi = lightBack ? c * .62f : Vector3.Lerp(c, Vector3.One, .5f);
        var neon = lightBack ? c * .72f : Vector3.Lerp(c, Vector3.One, .22f);
        bool own = !natGroup;
        if (own) n.Begin();
        switch (s.Style)
        {
            case PvStyle.Wire:
                Edges(v, neon, a * .95f, Px(MathF.Max(.8f, wz)));
                break;
            case PvStyle.Glass:
                n.Poly(v, nv, V4(c, a * (lightBack ? .42f : .3f)));
                Edges(v, hi, a * .85f, Px(MathF.Max(.6f, wz * .75f)));
                break;
            case PvStyle.Neon:
                n.Poly(v, nv, V4(lightBack ? Vector3.Lerp(c, Vector3.One, .75f) : c * .1f, a * .92f));
                Edges(v, neon, a, Px(MathF.Max(.9f, wz * 1.1f)));
                break;
            case PvStyle.Dots:
                n.Poly(v, nv, V4(c, a * .2f));
                foreach (var q in v) n.Dot(q, Px(MathF.Max(2f, wz * 1.9f)), V4(lightBack ? c * .62f : Vector3.Lerp(c, Vector3.One, .45f), a));
                break;
            case PvStyle.Bold:
                n.Poly(v, nv, V4(c, a));
                Edges(v, lightBack ? c * .3f : c * .12f, a, Px(MathF.Max(1.4f, wz * 2.3f)));
                break;
            case PvStyle.Silhouette:
                n.Poly(v, nv, V4(lightBack ? c * .2f : s.Pal.Back1 * .3f, a));
                Edges(v, lightBack ? c * .55f : Vector3.Lerp(c, Vector3.One, .2f), a * .95f, Px(MathF.Max(1f, wz)));
                break;
            case PvStyle.FillLine:
                n.Poly(v, nv, V4(c, a));
                if (wz > .05f) Edges(v, hi, a * .78f, Px(wz));
                break;
            default:
                n.Poly(v, nv, V4(c, a));
                Edges(v, c, a, Px(.8f));
                break;
        }
        if (own) n.End();
    }

    void Edges(ReadOnlySpan<Vector3> v, Vector3 col, float a, float width)
    {
        var c = V4(col, a);
        for (int i = 0; i < v.Length; i++) nat!.Edge(v[i], v[(i + 1) % v.Length], width, c);
    }

    List<Vector2> Project(ReadOnlySpan<Vector3> p)
    {
        float cx = s.W * .5f, cy = s.H * .5f, f = s.H * .98f * s.Zoom;
        var l = new List<Vector2>(p.Length + 1);
        foreach (var q in p)
        {
            var v = q.Z > Far - 4f && q.Z < Far ? q : q;
            float z = MathF.Max(.05f, v.Z);
            l.Add(new(cx + v.X / z * f, cy - v.Y / z * f));
        }
        return l;
    }

    public void Paint(ID2D1DeviceContext dc, ID2D1SolidColorBrush b)
    {
        var line = new List<Vector2>(10);
        float a0 = s.Alpha, lw = s.Line;
        foreach (var f in faces)
        {
            float a = f.A * a0;
            if (a <= .004f) continue;
            float wz = lw * Math.Clamp(7f / f.Z, .45f, 1.6f);
            line.Clear(); line.AddRange(f.P); line.Add(f.P[0]);
            var c = f.Col;
            var hi = lightBack ? c * .62f : Vector3.Lerp(c, Vector3.One, .5f);
            var neon = lightBack ? c * .72f : Vector3.Lerp(c, Vector3.One, .22f);
            switch (s.Style)
            {
                case PvStyle.Wire:
                    b.Color = C(neon, a * .95f); Gfx.Polyline(dc, b, line, MathF.Max(.8f, wz));
                    break;
                case PvStyle.Glass:
                    b.Color = C(c, a * (lightBack ? .42f : .3f)); Gfx.FillPoly(dc, b, f.P);
                    b.Color = C(hi, a * .85f); Gfx.Polyline(dc, b, line, MathF.Max(.6f, wz * .75f));
                    break;
                case PvStyle.Neon:
                    b.Color = C(lightBack ? Vector3.Lerp(c, Vector3.One, .75f) : c * .1f, a * .92f); Gfx.FillPoly(dc, b, f.P);
                    b.Color = C(c, a * .28f); Gfx.Polyline(dc, b, line, MathF.Max(2.4f, wz * 4.5f));
                    b.Color = C(neon, a); Gfx.Polyline(dc, b, line, MathF.Max(.9f, wz * 1.1f));
                    break;
                case PvStyle.Dots:
                    b.Color = C(c, a * .2f); Gfx.FillPoly(dc, b, f.P);
                    b.Color = C(lightBack ? c * .62f : Vector3.Lerp(c, Vector3.One, .45f), a);
                    foreach (var q in f.P) dc.FillEllipse(new Ellipse(q, MathF.Max(2f, wz * 1.9f), MathF.Max(2f, wz * 1.9f)), b);
                    break;
                case PvStyle.Bold:
                    b.Color = C(c, a); Gfx.FillPoly(dc, b, f.P);
                    b.Color = C(lightBack ? c * .3f : c * .12f, a); Gfx.Polyline(dc, b, line, MathF.Max(1.4f, wz * 2.3f));
                    break;
                case PvStyle.Silhouette:
                    b.Color = C(lightBack ? c * .2f : s.Pal.Back1 * .3f, a); Gfx.FillPoly(dc, b, f.P);
                    b.Color = C(lightBack ? c * .55f : Vector3.Lerp(c, Vector3.One, .2f), a * .95f); Gfx.Polyline(dc, b, line, MathF.Max(1f, wz));
                    break;
                case PvStyle.FillLine:
                    b.Color = C(c, a); Gfx.FillPoly(dc, b, f.P);
                    if (wz > .05f) { b.Color = C(hi, a * .78f); Gfx.Polyline(dc, b, line, wz); }
                    break;
                default:
                    b.Color = C(c, a); Gfx.FillPoly(dc, b, f.P);
                    Gfx.Polyline(dc, b, line, .8f);
                    break;
            }
        }
    }

    static Color4 C(Vector3 v, float a) => new(v.X, v.Y, v.Z, Math.Clamp(a, 0f, 1f));
}
