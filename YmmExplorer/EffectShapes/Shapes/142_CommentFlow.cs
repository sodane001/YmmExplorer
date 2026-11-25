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
//  コメントが流れる(弾幕)
//   書いたコメントが、右から左へ流れていく。コメントは1行に1つ
// =====================================================================
public sealed class CommentFlowPlugin : IShapePlugin
{
    public string Name => "演出：コメントが流れる（弾幕）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new CommentFlowParameter(sharedData);
}

public sealed class CommentFlowParameter : EffectShapeParameterBase
{
    public CommentFlowParameter() : this(null) { }
    public CommentFlowParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "コメント", Name = "コメント（1行に1つ）")]
    [TextEditor(AcceptsReturn = true)]
    public string Comments { get => comments; set => Set(ref comments, value); }
    string comments = "www\n草\nかわいい\n8888888888\nなるほど\nきたあああ\nうぽつ\nそれな\n神回\nｗｗｗｗｗｗ";

    [Display(GroupName = "コメント", Name = "順番に流す", Description = "オフなら、ばらばらの順で流します")]
    [ToggleSlider]
    public bool InOrder { get => inOrder; set => Set(ref inOrder, value); }
    bool inOrder = false;

    [Display(GroupName = "コメント", Name = "1秒に流れる数")]
    [AnimationSlider("F1", "個", 0.5, 30)]
    public Animation Rate { get; } = new Animation(5, 0, 200);

    [Display(GroupName = "コメント", Name = "画面を横切る時間")]
    [AnimationSlider("F1", "秒", 1, 10)]
    public Animation CrossTime { get; } = new Animation(4, 0.2, 120);

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "文字", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 16, 120)]
    public Animation FontSize { get; } = new Animation(50, 4, 1000);

    [Display(GroupName = "文字", Name = "文字の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "文字", Name = "ときどき色つきのコメント")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation ColorRatio { get; } = new Animation(15, 0, 100);

    [Display(GroupName = "文字", Name = "ふちの太さ")]
    [AnimationSlider("F1", "px", 0, 10)]
    public Animation OutlineWidth { get; } = new Animation(3, 0, 100);

    [Display(GroupName = "文字", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(90, 0, 100);

    [Display(GroupName = "流れる範囲", Name = "上端")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation AreaTop { get; } = new Animation(4, -100, 200);

    [Display(GroupName = "流れる範囲", Name = "下端")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation AreaBottom { get; } = new Animation(75, -100, 200);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [Rate, CrossTime, FontSize, ColorRatio, OutlineWidth, Opacity, AreaTop, AreaBottom];

    static readonly WpfColor[] Accents =
    {
        WpfColor.FromRgb(0xFF, 0x5C, 0x5C), WpfColor.FromRgb(0xFF, 0xD5, 0x4A), WpfColor.FromRgb(0x5D, 0xDB, 0x7A),
        WpfColor.FromRgb(0x4F, 0xC3, 0xF7), WpfColor.FromRgb(0xFF, 0x8A, 0xD8), WpfColor.FromRgb(0xFF, 0xA1, 0x3D),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var list = TextKit.Lines(Comments).Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
        float rate = c.F(Rate), cross = MathF.Max(0.2f, c.F(CrossTime));
        if (list.Count == 0 || rate <= 0.001f) return;
        float fs = c.F(FontSize), ow = c.F(OutlineWidth), alpha = c.F(Opacity) / 100f, ratio = c.F(ColorRatio) / 100f;
        float top = c.F(AreaTop) / 100f * h, bottom = c.F(AreaBottom) / 100f * h;
        float rowH = fs * 1.25f;
        int rows = Math.Max(1, (int)((bottom - top) / rowH));
        // 1つのコメントが流れきるまでの時間(長いコメントも右端から左端まで)
        int first = Math.Max(0, (int)MathF.Floor((t - cross * 2.5f) * rate));
        int last = (int)MathF.Floor(t * rate);
        using var brush = dc.CreateSolidColorBrush(default);
        var outline = Gfx.C4(0f, 0f, 0f, 0.85f * alpha);
        for (int i = first; i <= last; i++)
        {
            float born = (i + DrawKit.H(Seed, i, 1) * 0.6f) / rate;
            float age = t - born;
            if (age < 0f) continue;
            string text = InOrder ? list[i % list.Count] : list[(int)(DrawKit.H(Seed, i, 2) * list.Count) % list.Count];
            var s = TextKit.Get(text, fs, true, Font);
            // 長いコメントほど少し速く(同じ時間で横切る)
            float dist = w + s.Width;
            float x = w - age / cross * dist;
            if (x + s.Width < 0f) continue;
            // 同じ段に、続けて来ないよう、段をずらす
            int row = (int)(DrawKit.H(Seed, i, 3) * rows);
            if (i > 0 && row == (int)(DrawKit.H(Seed, i - 1, 3) * rows)) row = (row + 1 + i % 3) % rows;
            float y = top + row * rowH;
            var col = DrawKit.H(Seed, i, 4) < ratio ? Accents[(int)(DrawKit.H(Seed, i, 5) * Accents.Length) % Accents.Length] : Color;
            TextKit.Draw(dc, brush, s, new Vector2(x, y), Gfx.C4(col, alpha), outline, ow);
        }
    }
}
