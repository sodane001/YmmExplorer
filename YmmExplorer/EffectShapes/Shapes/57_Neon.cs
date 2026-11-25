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
//  ネオン図形
//   光るネオン管の輪郭。YMM4の図形から形を選べる
// =====================================================================
public sealed class NeonShapePlugin : IShapePlugin
{
    public string Name => "演出：ネオン図形";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new NeonShapeParameter(sharedData);
}

public sealed class NeonShapeParameter : EffectShapeParameterBase
{
    public NeonShapeParameter() : this(null) { }
    public NeonShapeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "形（YMM4の図形）", Description = "YMM4の図形から選ぶ。円・四角形・三角形・多角形・星・ハートなどを認識する")]
    [ShapeTypeComboBox]
    public Type ShapeType { get => shapeType; set => Set(ref shapeType, value); }
    Type shapeType = PluginLoader.GetPrimaryPluginType<IShapePlugin>();

    [Display(GroupName = "形", Name = "パーティクルにする", Description = "オンにすると、選んだ図形の代わりに光の粒で表示する")]
    [ToggleSlider]
    public bool ParticleMode { get => particleMode; set => Set(ref particleMode, value); }
    bool particleMode = false;

    [Display(GroupName = "形", Name = "多角形の角数")]
    [TextBoxSlider("F0", "角", 3, 12)]
    [Range(3, 12)]
    public int PolygonSides { get => sides; set => Set(ref sides, value); }
    int sides = 6;

    [Display(GroupName = "形", Name = "星の角数")]
    [TextBoxSlider("F0", "角", 3, 12)]
    [Range(3, 12)]
    public int StarPoints { get => starPoints; set => Set(ref starPoints, value); }
    int starPoints = 5;

    [Display(GroupName = "色", Name = "色1")]
    [ColorPicker]
    public WpfColor Color1 { get => c1; set => Set(ref c1, value); }
    WpfColor c1 = WpfColor.FromRgb(0xFF, 0x3D, 0xF5);

    [Display(GroupName = "色", Name = "色2", Description = "重ねた図形が色1から色2へ変わっていく")]
    [ColorPicker]
    public WpfColor Color2 { get => c2; set => Set(ref c2, value); }
    WpfColor c2 = WpfColor.FromRgb(0x3D, 0xE8, 0xFF);

    [Display(GroupName = "位置", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "形", Name = "大きさ（半径）")]
    [AnimationSlider("F0", "px", 50, 600)]
    public Animation Size { get; } = new Animation(240, 5, 5000);

    [Display(GroupName = "形", Name = "重ねる数")]
    [AnimationSlider("F0", "個", 1, 8)]
    public Animation Copies { get; } = new Animation(3, 1, 30);

    [Display(GroupName = "形", Name = "重ねたときの縮小率")]
    [AnimationSlider("F0", "%", 40, 95)]
    public Animation Step { get; } = new Animation(78, 10, 100);

    [Display(GroupName = "形", Name = "回転速度")]
    [AnimationSlider("F0", "°/s", -180, 180)]
    public Animation Speed { get; } = new Animation(0, -3600, 3600);

    [Display(GroupName = "光", Name = "管の太さ")]
    [AnimationSlider("F1", "px", 1, 20)]
    public Animation Thickness { get; } = new Animation(4, 0.5, 200);

    [Display(GroupName = "光", Name = "光のにじみ")]
    [AnimationSlider("F0", "px", 0, 150)]
    public Animation Glow { get; } = new Animation(36, 0, 1000);

    [Display(GroupName = "光", Name = "明るさ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation Brightness { get; } = new Animation(100, 0, 500);

    [Display(GroupName = "光", Name = "ちらつき", Description = "ネオン管がときどき消えかける")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Flicker { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "パーティクル", Name = "粒の数")]
    [AnimationSlider("F0", "個", 5, 300)]
    public Animation ParticleCount { get; } = new Animation(70, 1, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, Size, Copies, Step, Speed, Thickness, Glow, Brightness, Flicker, ParticleCount];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float size = c.F(Size);
        int copies = Math.Clamp((int)c.F(Copies), 1, 8);
        float step = c.F(Step) / 100f;
        float rotBase = Gfx.Rad(c.F(Speed)) * t;
        float th = c.F(Thickness), gw = c.F(Glow);
        float bright = c.F(Brightness) / 100f;
        float flick = c.F(Flicker) / 100f;
        int kind = ParticleMode ? 9 : FormKit.KindFromShapeType(ShapeType);

        float fl = 1f;
        if (flick > 0f)
        {
            var rf = new Random(Seed * 31 + (int)MathF.Floor(t * 14f));
            if (rf.NextF() < 0.12f * flick) fl = 0.25f + 0.5f * rf.NextF();
        }
        bright *= fl * (1f - 0.06f * MathF.Sin(t * 2f));
        if (bright <= 0.001f) return;

        var white = WpfColor.FromRgb(255, 255, 255);
        using var brush = dc.CreateSolidColorBrush(default);
        dc.PrimitiveBlend = PrimitiveBlend.Add;
        try
        {
            float gr = size * 1.8f;
            using (var g = Gfx.Radial(dc, ctr, gr,
                (0f, Gfx.C4(Color1, 0.10f * bright)), (1f, Gfx.C4(Color1, 0f))))
                dc.FillEllipse(new Ellipse(ctr, gr, gr), g);

            if (kind == 9)
            {
                DrawSparks(dc, brush, c, ctr, size, bright);
            }
            else
            {
                bool joints = !(kind == 0 || kind == 6);
                for (int k = 0; k < copies; k++)
                {
                    float rk = size * MathF.Pow(step, k);
                    float rot = rotBase * ((k % 2 == 0) ? 1f : -1f) * (1f + 0.2f * k);
                    var col = Gfx.Mix(Color1, Color2, copies == 1 ? 0f : k / (float)(copies - 1));
                    var core = Gfx.Mix(col, white, 0.7f);
                    foreach (var path in FormKit.FormPaths(kind, ctr, rk, rot, PolygonSides, StarPoints))
                    {
                        if (gw > 0f)
                        {
                            brush.Color = Gfx.C4(col, 0.05f * bright);
                            FormKit.Stroke(dc, brush, path, gw * 2.2f, joints);
                            brush.Color = Gfx.C4(col, 0.09f * bright);
                            FormKit.Stroke(dc, brush, path, gw * 1.3f, joints);
                            brush.Color = Gfx.C4(col, 0.20f * bright);
                            FormKit.Stroke(dc, brush, path, gw * 0.7f, joints);
                        }
                        brush.Color = Gfx.C4(col, 0.55f * bright);
                        FormKit.Stroke(dc, brush, path, th * 2.2f, joints);
                        brush.Color = Gfx.C4(core, 0.95f * bright);
                        FormKit.Stroke(dc, brush, path, th, joints);
                    }
                }
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }

    // パーティクル：光の粒が漂い、短い尾を引く
    void DrawSparks(ID2D1DeviceContext dc, ID2D1SolidColorBrush brush, in DrawContext c, Vector2 ctr, float size, float bright)
    {
        int n = (int)c.F(ParticleCount);
        float th = c.F(Thickness);
        float t = c.Time;
        var white = WpfColor.FromRgb(255, 255, 255);
        var rng = new Random(Seed);
        for (int i = 0; i < n; i++)
        {
            float ax = size * 1.6f * (0.3f + 0.7f * rng.NextF()), ay = size * 1.1f * (0.3f + 0.7f * rng.NextF());
            float fx = 0.2f + 0.6f * rng.NextF(), fy = 0.2f + 0.6f * rng.NextF();
            float p1 = rng.NextF() * MathF.PI * 2f, p2 = rng.NextF() * MathF.PI * 2f;
            float cm = rng.NextF();
            float r = th * (0.4f + 0.8f * rng.NextF());
            float tw = 0.6f + 0.4f * MathF.Sin(t * (1f + 2f * rng.NextF()) + p1);

            var col = Gfx.Mix(Color1, Color2, cm);
            var p = FormKit.Liss(ctr, ax, ay, t, fx, fy, p1, p2);
            var pPrev = FormKit.Liss(ctr, ax, ay, t - 0.08f, fx, fy, p1, p2);
            float a = bright * tw;

            brush.Color = Gfx.C4(col, 0.35f * a);
            dc.DrawLine(pPrev, p, brush, r * 1.6f);
            brush.Color = Gfx.C4(col, 0.06f * a);
            dc.FillEllipse(new Ellipse(p, r * 6f, r * 6f), brush);
            brush.Color = Gfx.C4(col, 0.18f * a);
            dc.FillEllipse(new Ellipse(p, r * 3f, r * 3f), brush);
            brush.Color = Gfx.C4(Gfx.Mix(col, white, 0.7f), 0.95f * a);
            dc.FillEllipse(new Ellipse(p, r, r), brush);
        }
    }
}
