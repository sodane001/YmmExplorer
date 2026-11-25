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
//  画面転換
//   画面を色で覆う（または覆いを開く）切り替え演出。31種類（11番目以降は 72b_TransitionMore.cs）
// =====================================================================
public sealed class TransitionPlugin : IShapePlugin
{
    public string Name => "演出：画面転換";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TransitionParameter(sharedData);
}

public sealed partial class TransitionParameter : EffectShapeParameterBase
{
    public TransitionParameter() : this(null) { }
    public TransitionParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "転換", Name = "種類")]
    [EnumComboBox]
    public TransKind Kind { get => kind; set => Set(ref kind, value); }
    TransKind kind = TransKind.WipeH;

    [Display(GroupName = "転換", Name = "色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0, 0, 0);

    [Display(GroupName = "転換", Name = "飾りの色1", Description = "カーテンのふさ・帯・炎・スタンプの文字など")]
    [ColorPicker]
    public WpfColor Color2 { get => color2; set => Set(ref color2, value); }
    WpfColor color2 = WpfColor.FromRgb(0xFF, 0xC8, 0x2E);

    [Display(GroupName = "転換", Name = "飾りの色2", Description = "斜めの帯・グリッチのずれなど")]
    [ColorPicker]
    public WpfColor Color3 { get => color3; set => Set(ref color3, value); }
    WpfColor color3 = WpfColor.FromRgb(0x39, 0xC8, 0xFF);

    [Display(GroupName = "転換", Name = "覆いを開く（画面が現れる）", Description = "オフ=画面を覆っていく、オン=覆いが開いて画面が現れる")]
    [ToggleSlider]
    public bool Reveal { get => reveal; set => Set(ref reveal, value); }
    bool reveal = false;

    [Display(GroupName = "転換", Name = "セルの数（ブラインド・マス目・丸）")]
    [AnimationSlider("F0", "個", 3, 40)]
    public Animation CellCount { get; } = new Animation(12, 2, 200);

    [Display(GroupName = "転換", Name = "ぼかし（ワイプの縁）")]
    [AnimationSlider("F0", "px", 0, 400)]
    public Animation Softness { get; } = new Animation(60, 0, 4000);

    [Display(GroupName = "タイミング", Name = "時間で自動で進める")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = true;

    [Display(GroupName = "タイミング", Name = "開始時刻（自動）")]
    [AnimationSlider("F2", "秒", 0, 20)]
    public Animation StartTime { get; } = new Animation(0.3, 0, 600);

    [Display(GroupName = "タイミング", Name = "かかる時間（自動）")]
    [AnimationSlider("F2", "秒", 0.1, 5)]
    public Animation Duration { get; } = new Animation(1, 0.05, 60);

    [Display(GroupName = "タイミング", Name = "進み（手動）", Description = "「時間で自動で進める」がオフのとき。0→100%で転換する")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Progress { get; } = new Animation(0, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [CellCount, Softness, StartTime, Duration, Progress];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float p;
        if (Auto)
        {
            float x = Math.Clamp((t - c.F(StartTime)) / MathF.Max(0.05f, c.F(Duration)), 0f, 1f);
            p = x < 0.5f ? 2f * x * x : 1f - MathF.Pow(-2f * x + 2f, 2f) / 2f;      // なめらかな加減速
        }
        else p = c.F(Progress) / 100f;
        float cov = Reveal ? 1f - p : p;                       // 0=覆いなし 1=全面を覆う
        if (cov <= 0.001f) return;

        int n = Math.Max(2, (int)c.F(CellCount));
        float soft = c.F(Softness);
        var ctr = new Vector2(w / 2f, h / 2f);
        float maxR = MathF.Sqrt(w * w + h * h) / 2f * 1.05f;

        using var brush = dc.CreateSolidColorBrush(default);
        brush.Color = Gfx.C4(Color);

        switch (Kind)
        {
            case TransKind.WipeH:
                {
                    float edge = cov * (w + soft) - soft;
                    if (edge > 0f) Gfx.Rect(dc, brush, 0, 0, edge, h);
                    if (soft > 0f)
                    {
                        using var gb = Gfx.Linear(dc, new Vector2(edge, 0), new Vector2(edge + soft, 0), (0f, Gfx.C4(Color, 1f)), (1f, Gfx.C4(Color, 0f)));
                        Gfx.Rect(dc, gb, MathF.Max(0f, edge), 0, soft + MathF.Min(0f, edge), h);
                    }
                    break;
                }
            case TransKind.WipeV:
                {
                    float edge = cov * (h + soft) - soft;
                    if (edge > 0f) Gfx.Rect(dc, brush, 0, 0, w, edge);
                    if (soft > 0f)
                    {
                        using var gb = Gfx.Linear(dc, new Vector2(0, edge), new Vector2(0, edge + soft), (0f, Gfx.C4(Color, 1f)), (1f, Gfx.C4(Color, 0f)));
                        Gfx.Rect(dc, gb, 0, MathF.Max(0f, edge), w, soft + MathF.Min(0f, edge));
                    }
                    break;
                }
            case TransKind.WipeDiag:
                {
                    var nrm = new Vector2(0.7071f, 0.7071f);
                    var perp = new Vector2(-nrm.Y, nrm.X);
                    float pmin = 0f, pmax = w * nrm.X + h * nrm.Y;
                    float pb = pmin + cov * (pmax - pmin);
                    float L = MathF.Sqrt(w * w + h * h) * 1.2f;
                    var poly = new List<Vector2>
                    {
                        nrm * pb + perp * L, nrm * pb - perp * L, nrm * (pb - L) - perp * L, nrm * (pb - L) + perp * L,
                    };
                    Gfx.FillPoly(dc, brush, poly, -2f, h + 2f);
                    break;
                }
            case TransKind.CircleOpen:
                dc.FillEllipse(new Ellipse(ctr, maxR * cov, maxR * cov), brush);
                break;
            case TransKind.IrisClose:
                {
                    float inner = (1f - cov) * maxR;
                    float outer = maxR * 1.6f;
                    dc.DrawEllipse(new Ellipse(ctr, (inner + outer) / 2f, (inner + outer) / 2f), brush, outer - inner);
                    break;
                }
            case TransKind.BlindsV:
                {
                    float cw = w / n;
                    for (int i = 0; i < n; i++) Gfx.Rect(dc, brush, i * cw, 0, cw * cov + (cov >= 0.999f ? 1f : 0f), h);
                    break;
                }
            case TransKind.BlindsH:
                {
                    float ch = h / n;
                    for (int i = 0; i < n; i++) Gfx.Rect(dc, brush, 0, i * ch, w, ch * cov + (cov >= 0.999f ? 1f : 0f));
                    break;
                }
            case TransKind.Grid:
                {
                    float cs = w / n;
                    int rows = (int)MathF.Ceiling(h / cs);
                    for (int r = 0; r < rows; r++)
                        for (int cI = 0; cI < n; cI++)
                        {
                            float d = ((cI + 0.5f) / n + (r + 0.5f) / rows) / 2f * 0.6f;
                            float local = Math.Clamp((cov * 1.6f - d) / 0.4f, 0f, 1f);
                            if (local <= 0f) continue;
                            float s = cs * local + (local >= 1f ? 1f : 0f);
                            Gfx.Rect(dc, brush, cI * cs + (cs - s) / 2f, r * cs + (cs - s) / 2f, s, s);
                        }
                    break;
                }
            case TransKind.Clock:
                {
                    var pts = new List<Vector2> { ctr };
                    float a1 = -MathF.PI / 2f + cov * MathF.PI * 2f;
                    pts.AddRange(Gfx.ArcPts(ctr, maxR * 1.6f, -MathF.PI / 2f, a1, Math.Max(4, (int)(80 * cov))));
                    Gfx.FillPoly(dc, brush, pts, -2f, h + 2f);
                    break;
                }
            case TransKind.Dots:
                {
                    float cs = w / n;
                    int rows = (int)MathF.Ceiling(h / cs);
                    for (int r = 0; r < rows; r++)
                        for (int cI = 0; cI < n; cI++)
                        {
                            float d = ((cI + 0.5f) / n + (r + 0.5f) / rows) / 2f * 0.6f;
                            float local = Math.Clamp((cov * 1.6f - d) / 0.4f, 0f, 1f);
                            if (local <= 0f) continue;
                            float rad = cs * 0.78f * local;
                            dc.FillEllipse(new Ellipse(new Vector2((cI + 0.5f) * cs, (r + 0.5f) * cs), rad, rad), brush);
                        }
                    break;
                }
            default:
                DrawMore(dc, brush, w, h, t, cov, n, soft);
                break;
        }
    }
}
