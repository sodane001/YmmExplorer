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

public enum ReactionMark
{
    [Display(Name = "えっ？（振り返りの線）")] Turn,
    [Display(Name = "ハッ（気づきの線）")] Notice,
    [Display(Name = "ドキッ（心臓が跳ねる）")] Doki,
    [Display(Name = "ピキッ（青筋）")] Piki,
    [Display(Name = "ガビーン（稲妻の線）")] Shock,
}

// =====================================================================
//  リアクション
//   キャラクターの頭のまわりに出す、気持ちの線と擬音(えっ？・ハッ・ドキッ・ピキッ など)
// =====================================================================
public sealed class ReactionPlugin : IShapePlugin
{
    public string Name => "演出：リアクション（えっ？・ドキッ・ピキッ）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new ReactionParameter(sharedData);
}

public sealed class ReactionParameter : EffectShapeParameterBase
{
    public ReactionParameter() : this(null) { }
    public ReactionParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "形", Name = "種類")]
    [EnumComboBox]
    public ReactionMark Kind { get => kind; set => Set(ref kind, value); }
    ReactionMark kind = ReactionMark.Turn;

    [Display(GroupName = "形", Name = "擬音の文字（空なら種類に合わせる）")]
    [TextEditor(AcceptsReturn = false)]
    public string Sound { get => sound; set => Set(ref sound, value); }
    string sound = "";

    [Display(GroupName = "形", Name = "線の色")]
    [ColorPicker]
    public WpfColor LineColor { get => lineColor; set => Set(ref lineColor, value); }
    WpfColor lineColor = WpfColor.FromRgb(0x20, 0x20, 0x28);

    [Display(GroupName = "形", Name = "文字の色")]
    [ColorPicker]
    public WpfColor TextColor { get => textColor; set => Set(ref textColor, value); }
    WpfColor textColor = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "形", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "位置と大きさ", Name = "X位置（頭の中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（頭の中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(35, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "大きさ（頭の半径）")]
    [AnimationSlider("F0", "px", 40, 500)]
    public Animation Size { get; } = new Animation(170, 5, 3000);

    [Display(GroupName = "位置と大きさ", Name = "左右反転")]
    [ToggleSlider]
    public bool Mirror { get => mirror; set => Set(ref mirror, value); }
    bool mirror = false;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [PosX, PosY, Size];

    string DefaultSound => Kind switch
    {
        ReactionMark.Turn => "えっ？",
        ReactionMark.Notice => "ハッ",
        ReactionMark.Doki => "ドキッ",
        ReactionMark.Piki => "ピキッ",
        _ => "ガーン",
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0f, 0.2f);
        if (k <= 0.001f) return;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float R = c.F(Size);
        float mir = Mirror ? -1f : 1f;
        float pop = DrawKit.EaseOutBack(Math.Clamp(t / 0.2f, 0f, 1f));
        using var brush = dc.CreateSolidColorBrush(default);
        var line = Gfx.C4(LineColor, k);
        Vector2 P(float x, float y) => ctr + new Vector2(x * mir, y) * R;

        switch (Kind)
        {
            case ReactionMark.Turn:
            {
                // 頭の横に、外へ向かう3本の線(短くピッと出る)
                for (int i = 0; i < 3; i++)
                {
                    float a = -0.6f + i * 0.45f;
                    var d = new Vector2(MathF.Cos(a) * mir, MathF.Sin(a));
                    float grow = DrawKit.EaseOut(Math.Clamp((t - i * 0.03f) / 0.15f, 0f, 1f));
                    var a0 = ctr + d * R * 1.15f;
                    brush.Color = line;
                    dc.DrawLine(a0, a0 + d * R * 0.35f * grow, brush, MathF.Max(3f, R * 0.045f));
                }
                break;
            }
            case ReactionMark.Notice:
            {
                // 頭のまわりに放射状の短い線(ピカッと)
                for (int i = 0; i < 9; i++)
                {
                    float a = -MathF.PI + i / 8f * MathF.PI;
                    var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    float flick = 0.8f + 0.2f * MathF.Sin(t * 30f + i);
                    brush.Color = line;
                    dc.DrawLine(ctr + d * R * 1.15f, ctr + d * R * (1.15f + 0.3f * pop * flick), brush, MathF.Max(3f, R * 0.04f));
                }
                break;
            }
            case ReactionMark.Doki:
            {
                // 胸のあたり(頭の下)で心臓が跳ねる
                var hc = P(0.55f, 1.35f);
                float beat = 1f + 0.35f * MathF.Exp(-DrawKit.Frac(t * 1.2f) * 8f);
                brush.Color = Gfx.C4(0.95f, 0.2f, 0.35f, k);
                Gfx.FillPoly(dc, brush, ShapeGeom.HeartPoly(hc, R * 0.28f * beat * pop, 0f));
                brush.Color = line;
                for (int i = 0; i < 4; i++)
                {
                    float a = -MathF.PI / 2f + (i - 1.5f) * 0.55f;
                    var d = new Vector2(MathF.Cos(a), MathF.Sin(a));
                    dc.DrawLine(hc + d * R * 0.4f * beat, hc + d * R * 0.55f * beat, brush, MathF.Max(2.5f, R * 0.03f));
                }
                break;
            }
            case ReactionMark.Piki:
            {
                // こめかみの青筋(角ばった十字の線)
                var vc = P(0.7f, -0.55f);
                float throb = 1f + 0.12f * MathF.Max(0f, MathF.Sin(t * 12f));
                float s = R * 0.22f * throb * pop;
                brush.Color = Gfx.C4(0.2f, 0.35f, 0.9f, k);
                foreach (var (dx, dy) in new[] { (1f, 0f), (-1f, 0f), (0f, 1f), (0f, -1f) })
                {
                    var d = new Vector2(dx, dy);
                    var n = new Vector2(-dy, dx);
                    var pts = new List<Vector2> { vc + d * s * 0.2f, vc + d * s * 0.6f + n * s * 0.25f, vc + d * s };
                    Gfx.Polyline(dc, brush, pts, MathF.Max(3f, R * 0.05f));
                }
                break;
            }
            default:
            {
                // ガビーン: 頭の後ろに、ギザギザの稲妻の線
                for (int i = 0; i < 4; i++)
                {
                    float x = (i - 1.5f) * 0.4f;
                    var pts = new List<Vector2>();
                    for (int j = 0; j <= 6; j++)
                    {
                        float y = -1.6f + j * 0.18f;
                        pts.Add(P(x + (j % 2 == 0 ? -0.06f : 0.06f), y));
                    }
                    float len = Math.Clamp((t - i * 0.04f) / 0.2f, 0f, 1f);
                    brush.Color = line;
                    Gfx.Polyline(dc, brush, BubbleKit.PartByLength(pts, len), MathF.Max(3f, R * 0.04f));
                }
                break;
            }
        }

        // 擬音の文字
        string snd = string.IsNullOrWhiteSpace(Sound) ? DefaultSound : TextKit.Lines(Sound).Trim();
        var st = TextKit.Get(snd, R * 0.42f, true, Font);
        var sp = Kind switch
        {
            ReactionMark.Doki => P(1.25f, 1.0f),
            ReactionMark.Piki => P(1.15f, -1.05f),
            ReactionMark.Shock => P(0f, -1.95f),
            _ => P(1.35f, -0.9f),
        };
        float shake = Kind is ReactionMark.Piki or ReactionMark.Shock ? MathF.Sin(t * 50f) * R * 0.015f : 0f;
        var prev = dc.Transform;
        dc.Transform = Matrix3x2.CreateRotation(Gfx.Rad(-8f * mir), sp) * prev;
        TextKit.DrawCentered(dc, brush, st, sp + new Vector2(shake, 0f), Gfx.C4(TextColor, k), line, R * 0.025f, pop);
        dc.Transform = prev;
    }
}
