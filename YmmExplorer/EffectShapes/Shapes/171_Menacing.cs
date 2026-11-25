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
//  オーラ文字(ゴゴゴゴ)
//   キャラクターの背後に、圧のある擬音の文字が浮かび、ゆらめくオーラが立ちのぼる
// =====================================================================
public sealed class MenacingPlugin : IShapePlugin
{
    public string Name => "演出：オーラ文字（ゴゴゴゴ）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new MenacingParameter(sharedData);
}

public sealed class MenacingParameter : EffectShapeParameterBase
{
    public MenacingParameter() : this(null) { }
    public MenacingParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "文字", Name = "擬音の文字")]
    [TextEditor(AcceptsReturn = false)]
    public string Sound { get => sound; set => Set(ref sound, value); }
    string sound = "ゴ";

    [Display(GroupName = "文字", Name = "文字の数")]
    [TextBoxSlider("F0", "個", 2, 16)]
    [Range(1, 60)]
    public int Count { get => count; set => Set(ref count, value); }
    int count = 8;

    [Display(GroupName = "文字", Name = "フォント")]
    [FontComboBox]
    public string Font { get => font; set => Set(ref font, value); }
    string font = "";

    [Display(GroupName = "文字", Name = "文字の大きさ")]
    [AnimationSlider("F0", "px", 30, 300)]
    public Animation FontSize { get; } = new Animation(110, 4, 3000);

    [Display(GroupName = "色", Name = "文字の色")]
    [ColorPicker]
    public WpfColor Color { get => color; set => Set(ref color, value); }
    WpfColor color = WpfColor.FromRgb(0x7A, 0x2C, 0xC8);

    [Display(GroupName = "色", Name = "ふちの色")]
    [ColorPicker]
    public WpfColor OutlineColor { get => outlineColor; set => Set(ref outlineColor, value); }
    WpfColor outlineColor = WpfColor.FromRgb(0x12, 0x08, 0x20);

    [Display(GroupName = "色", Name = "オーラを出す")]
    [ToggleSlider]
    public bool Aura { get => aura; set => Set(ref aura, value); }
    bool aura = true;

    [Display(GroupName = "位置と大きさ", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "広がりの幅")]
    [AnimationSlider("F0", "px", 200, 1800)]
    public Animation AreaWidth { get; } = new Animation(900, 10, 8000);

    [Display(GroupName = "位置と大きさ", Name = "広がりの高さ")]
    [AnimationSlider("F0", "px", 200, 1200)]
    public Animation AreaHeight { get; } = new Animation(800, 10, 8000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [FontSize, PosX, PosY, AreaWidth, AreaHeight];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float k = DrawKit.InOut(c, 0.3f, 0.3f);
        if (k <= 0.001f) return;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float aw = c.F(AreaWidth), ah = c.F(AreaHeight), fs = c.F(FontSize);
        using var brush = dc.CreateSolidColorBrush(default);
        if (Aura)
        {
            // 立ちのぼる炎のようなオーラ(縦長の波打つ形を、何層か)
            for (int layer = 0; layer < 3; layer++)
            {
                var pts = new List<Vector2>();
                int n = 60;
                for (int i = 0; i <= n; i++)
                {
                    float a = i / (float)n * MathF.PI * 2f;
                    float flame = 1f + 0.12f * MathF.Sin(a * 7f + t * (4f + layer)) + 0.08f * MathF.Sin(a * 13f - t * 6f);
                    float up = MathF.Sin(a) < 0f ? 1f + 0.35f * (-MathF.Sin(a)) * (0.7f + 0.3f * MathF.Sin(a * 5f + t * 8f)) : 1f;
                    float rx = aw * 0.35f * (1f - layer * 0.18f) * flame, ry = ah * 0.42f * (1f - layer * 0.18f) * flame * up;
                    pts.Add(ctr + new Vector2(MathF.Cos(a) * rx, MathF.Sin(a) * ry));
                }
                brush.Color = Gfx.C4(Color, (0.12f + layer * 0.06f) * k);
                Gfx.FillPoly(dc, brush, pts);
            }
        }
        // 文字: 中心のまわり(左右)に散らばり、それぞれ脈打つように拡大・ゆれる
        string snd = TextKit.Lines(Sound).Trim();
        if (snd.Length == 0) return;
        var s = TextKit.Get(snd, fs, true, Font);
        int cnt = Math.Max(1, Count);
        for (int i = 0; i < cnt; i++)
        {
            // 左右の外側に寄せて置く(まん中のキャラクターにかぶりにくい)
            float side = i % 2 == 0 ? -1f : 1f;
            float hx = side * (0.25f + DrawKit.H(Seed, i, 1) * 0.25f) * aw;
            float hy = (DrawKit.H(Seed, i, 2) - 0.5f) * ah * 0.9f;
            float phase = DrawKit.H(Seed, i, 3) * MathF.PI * 2f;
            float appear = Math.Clamp((t - i * 0.08f) / 0.25f, 0f, 1f);
            if (appear <= 0f) continue;
            float beat = 1f + 0.12f * MathF.Max(0f, MathF.Sin(t * 5f + phase));
            float sz = (0.7f + DrawKit.H(Seed, i, 4) * 0.6f) * beat * DrawKit.EaseOutBack(appear);
            var jitter = new Vector2(DrawKit.H(Seed + (int)(t * 30f), i, 5) - 0.5f, DrawKit.H(Seed + (int)(t * 30f), i, 6) - 0.5f) * fs * 0.04f;
            var p = ctr + new Vector2(hx, hy) + jitter;
            var prev = dc.Transform;
            dc.Transform = Matrix3x2.CreateRotation(Gfx.Rad(side * 12f + MathF.Sin(t * 2f + phase) * 4f), p) * prev;
            TextKit.DrawCentered(dc, brush, s, p, Gfx.C4(Color, k * appear), Gfx.C4(OutlineColor, k * appear), fs * 0.06f, sz,
                Gfx.C4(0f, 0f, 0f, 0.4f * k * appear), new Vector2(fs * 0.04f, fs * 0.05f));
            dc.Transform = prev;
        }
    }
}
