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

public enum LetterboxRatio
{
    [Display(Name = "2.39:1（シネマスコープ）")] Scope,
    [Display(Name = "2:1")] Univisium,
    [Display(Name = "1.85:1（ビスタ）")] Vista,
    [Display(Name = "4:3（左右に帯）")] Classic,
    [Display(Name = "自分で決める")] Custom,
}

// =====================================================================
//  シネマスコープの黒帯
//   上下(または左右)の帯が、すっと入ってきて映画のような画面になる
// =====================================================================
public sealed class CinemascopePlugin : IShapePlugin
{
    public string Name => "演出：シネマスコープの黒帯";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CinemascopeParameter(sharedData);
}

public sealed class CinemascopeParameter : EffectShapeParameterBase
{
    public CinemascopeParameter() : this(null) { }
    public CinemascopeParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "帯", Name = "画面の比率")]
    [EnumComboBox]
    public LetterboxRatio Ratio { get => ratio; set => Set(ref ratio, value); }
    LetterboxRatio ratio = LetterboxRatio.Scope;

    [Display(GroupName = "帯", Name = "帯の太さ（自分で決める時）")]
    [AnimationSlider("F0", "%", 0, 40)]
    public Animation CustomSize { get; } = new Animation(12, 0, 50);

    [Display(GroupName = "帯", Name = "帯の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0, 0, 0);

    [Display(GroupName = "帯", Name = "帯のふちをぼかす")]
    [AnimationSlider("F0", "px", 0, 60)]
    public Animation Feather { get; } = new Animation(0, 0, 500);

    [Display(GroupName = "動き", Name = "入ってくる時間")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation InTime { get; } = new Animation(0.8, 0, 60);

    [Display(GroupName = "動き", Name = "出ていく時間（アイテムの終わり）")]
    [AnimationSlider("F2", "秒", 0, 3)]
    public Animation OutTime { get; } = new Animation(0.8, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [CustomSize, Feather, InTime, OutTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float inT = c.F(InTime), outT = c.F(OutTime);
        float kin = inT <= 0.001f ? 1f : Math.Clamp(c.Time / inT, 0f, 1f);
        float kout = outT <= 0.001f ? 1f : Math.Clamp(DrawKit.TimeLeft(c) / outT, 0f, 1f);
        float k = MathF.Min(kin, kout);
        k = k * k * (3f - 2f * k);
        if (k <= 0.001f) return;
        bool sides = Ratio == LetterboxRatio.Classic;
        float target = Ratio switch
        {
            LetterboxRatio.Scope => 2.39f,
            LetterboxRatio.Univisium => 2f,
            LetterboxRatio.Vista => 1.85f,
            LetterboxRatio.Classic => 4f / 3f,
            _ => 0f,
        };
        float band;
        if (Ratio == LetterboxRatio.Custom) band = h * c.F(CustomSize) / 100f;
        else if (sides) band = MathF.Max(0f, (w - h * target) / 2f);
        else band = MathF.Max(0f, (h - w / target) / 2f);
        band *= k;
        float fe = c.F(Feather);
        using var brush = dc.CreateSolidColorBrush(default);
        brush.Color = Gfx.C4(Color, 1f);
        if (sides)
        {
            dc.FillRectangle(new RawRectF(0, 0, band, h), brush);
            dc.FillRectangle(new RawRectF(w - band, 0, w, h), brush);
            if (fe > 0.5f)
            {
                using (var g = Gfx.Linear(dc, new Vector2(band, 0), new Vector2(band + fe, 0), (0f, Gfx.C4(Color, 1f)), (1f, Gfx.C4(Color, 0f))))
                    dc.FillRectangle(new RawRectF(band, 0, band + fe, h), g);
                using (var g = Gfx.Linear(dc, new Vector2(w - band, 0), new Vector2(w - band - fe, 0), (0f, Gfx.C4(Color, 1f)), (1f, Gfx.C4(Color, 0f))))
                    dc.FillRectangle(new RawRectF(w - band - fe, 0, w - band, h), g);
            }
        }
        else
        {
            dc.FillRectangle(new RawRectF(0, 0, w, band), brush);
            dc.FillRectangle(new RawRectF(0, h - band, w, h), brush);
            if (fe > 0.5f)
            {
                using (var g = Gfx.Linear(dc, new Vector2(0, band), new Vector2(0, band + fe), (0f, Gfx.C4(Color, 1f)), (1f, Gfx.C4(Color, 0f))))
                    dc.FillRectangle(new RawRectF(0, band, w, band + fe), g);
                using (var g = Gfx.Linear(dc, new Vector2(0, h - band), new Vector2(0, h - band - fe), (0f, Gfx.C4(Color, 1f)), (1f, Gfx.C4(Color, 0f))))
                    dc.FillRectangle(new RawRectF(0, h - band - fe, w, h - band), g);
            }
        }
    }
}
