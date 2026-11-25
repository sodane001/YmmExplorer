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
//  空
//   昼 / 朝焼け / 夕焼け / 夜明け前 / 曇り / カスタム + 太陽 + 流れる雲
// =====================================================================
public enum SkyPreset
{
    [Display(Name = "昼")] Day,
    [Display(Name = "朝焼け")] Morning,
    [Display(Name = "夕焼け")] Sunset,
    [Display(Name = "夜明け前")] Dawn,
    [Display(Name = "曇り")] Overcast,
    [Display(Name = "カスタム")] Custom,
}

public sealed class SkyPlugin : IShapePlugin
{
    public string Name => "演出：空";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new SkyParameter(sharedData);
}

public sealed class SkyParameter : EffectShapeParameterBase
{
    public SkyParameter() : this(null) { }
    public SkyParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "空", Name = "時間帯")]
    [EnumComboBox]
    public SkyPreset Preset { get => preset; set => Set(ref preset, value); }
    SkyPreset preset = SkyPreset.Day;

    // ---- カスタム用の色（時間帯が「カスタム」のときだけ使う）----
    [Display(GroupName = "カスタムの色", Name = "天頂の色")]
    [ColorPicker]
    public WpfColor TopColor { get => top; set => Set(ref top, value); }
    WpfColor top = WpfColor.FromRgb(0x2E, 0x7B, 0xE8);

    [Display(GroupName = "カスタムの色", Name = "中間の色")]
    [ColorPicker]
    public WpfColor MidColor { get => mid; set => Set(ref mid, value); }
    WpfColor mid = WpfColor.FromRgb(0x6E, 0xB4, 0xF5);

    [Display(GroupName = "カスタムの色", Name = "地平線の色")]
    [ColorPicker]
    public WpfColor HorizonColor { get => hor; set => Set(ref hor, value); }
    WpfColor hor = WpfColor.FromRgb(0xCD, 0xEB, 0xFF);

    [Display(GroupName = "カスタムの色", Name = "雲の明るい色")]
    [ColorPicker]
    public WpfColor CloudLit { get => cLit; set => Set(ref cLit, value); }
    WpfColor cLit = WpfColor.FromRgb(255, 255, 255);

    [Display(GroupName = "カスタムの色", Name = "雲の影の色")]
    [ColorPicker]
    public WpfColor CloudShade { get => cShade; set => Set(ref cShade, value); }
    WpfColor cShade = WpfColor.FromRgb(0xB8, 0xCC, 0xE4);

    [Display(GroupName = "カスタムの色", Name = "太陽の色")]
    [ColorPicker]
    public WpfColor SunColor { get => sunCol; set => Set(ref sunCol, value); }
    WpfColor sunCol = WpfColor.FromRgb(0xFF, 0xF6, 0xD0);

    // ---- 太陽 ----
    [Display(GroupName = "太陽", Name = "太陽を表示する")]
    [ToggleSlider]
    public bool ShowSun { get => showSun; set => Set(ref showSun, value); }
    bool showSun = true;

    [Display(GroupName = "太陽", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SunX { get; } = new Animation(70, -100, 200);

    [Display(GroupName = "太陽", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation SunY { get; } = new Animation(28, -100, 200);

    [Display(GroupName = "太陽", Name = "大きさ")]
    [AnimationSlider("F0", "px", 10, 200)]
    public Animation SunSize { get; } = new Animation(46, 2, 2000);

    [Display(GroupName = "太陽", Name = "光の強さ")]
    [AnimationSlider("F0", "%", 0, 200)]
    public Animation SunGlow { get; } = new Animation(100, 0, 500);

    // ---- 雲 ----
    [Display(GroupName = "雲", Name = "数")]
    [AnimationSlider("F0", "個", 0, 120)]
    public Animation CloudCount { get; } = new Animation(36, 0, 500);

    [Display(GroupName = "雲", Name = "大きさ")]
    [AnimationSlider("F0", "px", 50, 500)]
    public Animation CloudSize { get; } = new Animation(170, 10, 3000);

    [Display(GroupName = "雲", Name = "高さ位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CloudY { get; } = new Animation(32, -100, 200);

    [Display(GroupName = "雲", Name = "流れる速度")]
    [AnimationSlider("F0", "px/s", -100, 100)]
    public Animation CloudSpeed { get; } = new Animation(12, -3000, 3000);

    [Display(GroupName = "雲", Name = "濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation CloudOpacity { get; } = new Animation(75, 0, 100);

    [Display(GroupName = "空", Name = "地平線のもや")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Haze { get; } = new Animation(35, 0, 100);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [SunX, SunY, SunSize, SunGlow, CloudCount, CloudSize, CloudY, CloudSpeed, CloudOpacity, Haze];

    // 時間帯ごとの色（天頂, 中間, 地平線, 雲の明, 雲の影, 太陽）
    (WpfColor, WpfColor, WpfColor, WpfColor, WpfColor, WpfColor) Colors()
    {
        static WpfColor R(int rgb) => WpfColor.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        return Preset switch
        {
            SkyPreset.Day => (R(0x2E7BE8), R(0x6EB4F5), R(0xCDEBFF), R(0xFFFFFF), R(0xB8CCE4), R(0xFFF6D0)),
            SkyPreset.Morning => (R(0x4A64B8), R(0xF2A7B8), R(0xFFD9A0), R(0xFFE6C8), R(0xB58AA8), R(0xFFE08A)),
            SkyPreset.Sunset => (R(0x2B3F8F), R(0xE8725A), R(0xFFB35C), R(0xFFC890), R(0x7A4A6A), R(0xFFD27A)),
            SkyPreset.Dawn => (R(0x0B1230), R(0x3A3570), R(0xC07A8A), R(0x8A7AA8), R(0x2C2A4A), R(0xFFB89A)),
            SkyPreset.Overcast => (R(0x7C8794), R(0xA9B2BC), R(0xD6DADF), R(0xE8EBEE), R(0x8C95A0), R(0xFFFFFF)),
            _ => (TopColor, MidColor, HorizonColor, CloudLit, CloudShade, SunColor),
        };
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var (top, mid, hor, lit, shade, sunC) = Colors();
        using var brush = dc.CreateSolidColorBrush(default);

        // ---- 空のグラデーション ----
        using (var g = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, h),
            (0f, Gfx.C4(top)), (0.55f, Gfx.C4(mid)), (1f, Gfx.C4(hor))))
            Gfx.Rect(dc, g, 0, 0, w, h);

        // ---- 太陽の光 ----
        if (ShowSun)
        {
            var sp = new Vector2(c.F(SunX) / 100f * w, c.F(SunY) / 100f * h);
            float sz = c.F(SunSize), glow = c.F(SunGlow) / 100f;
            float gr = MathF.Max(w, h) * 0.55f;
            dc.PrimitiveBlend = PrimitiveBlend.Add;
            try
            {
                using (var gb = Gfx.Radial(dc, sp, gr,
                    (0f, Gfx.C4(sunC, 0.55f * glow)), (0.12f, Gfx.C4(sunC, 0.28f * glow)),
                    (0.4f, Gfx.C4(sunC, 0.08f * glow)), (1f, Gfx.C4(sunC, 0f))))
                    dc.FillEllipse(new Ellipse(sp, gr, gr), gb);
                float dr = sz * 1.5f;
                using (var db = Gfx.Radial(dc, sp, dr,
                    (0f, Gfx.C4(WpfColor.FromRgb(255, 255, 255), 1f)), (0.6f, Gfx.C4(sunC, 1f)), (1f, Gfx.C4(sunC, 0f))))
                    dc.FillEllipse(new Ellipse(sp, dr, dr), db);
            }
            finally
            {
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }
        }

        // ---- 地平線のもや ----
        float haze = c.F(Haze) / 100f;
        if (haze > 0f)
        {
            using var hb = Gfx.Linear(dc, new Vector2(0, h), new Vector2(0, h * 0.55f),
                (0f, Gfx.C4(hor, 0.9f * haze)), (1f, Gfx.C4(hor, 0f)));
            Gfx.Rect(dc, hb, 0, h * 0.55f, w, h * 0.45f);
        }

        // ---- 雲（影→明部の2パス、層ごとに速さが違う）----
        int n = (int)c.F(CloudCount);
        if (n > 0)
        {
            float size = c.F(CloudSize), cy = c.F(CloudY) / 100f * h, spd = c.F(CloudSpeed), op = c.F(CloudOpacity) / 100f;
            float margin = size * 3f, span = w + margin * 2f;
            int clusters = Math.Max(1, n / 6);
            var rng = new Random(Seed);
            var cc = new (float x, float y, float s, float v)[clusters];
            for (int k = 0; k < clusters; k++)
                cc[k] = (rng.NextF() * span, cy + (rng.NextF() - 0.5f) * h * 0.18f, 0.6f + 0.8f * rng.NextF(), 0.5f + rng.NextF());

            var puffs = new List<(float x, float y, float r)>(n);
            for (int i = 0; i < n; i++)
            {
                var cl = cc[i % clusters];
                float ox = (rng.NextF() - 0.5f) * size * 2.2f * cl.s;
                float oy = (rng.NextF() - 0.5f) * size * 0.5f * cl.s;
                float r = size * cl.s * (0.5f + 0.5f * rng.NextF());
                float x = Gfx.Mod(cl.x + t * spd * cl.v, span) - margin + ox;
                puffs.Add((x, cl.y + oy, r));
            }
            foreach (var p in puffs)
            {
                var ctr = new Vector2(p.x, p.y + p.r * 0.25f);
                using var b = Gfx.Radial(dc, ctr, p.r,
                    (0f, Gfx.C4(shade, op)), (0.55f, Gfx.C4(shade, op * 0.6f)), (1f, Gfx.C4(shade, 0f)));
                dc.FillEllipse(new Ellipse(ctr, p.r, p.r), b);
            }
            foreach (var p in puffs)
            {
                var ctr = new Vector2(p.x, p.y - p.r * 0.12f);
                float r = p.r * 0.9f;
                using var b = Gfx.Radial(dc, ctr, r,
                    (0f, Gfx.C4(lit, op)), (0.55f, Gfx.C4(lit, op * 0.6f)), (1f, Gfx.C4(lit, 0f)));
                dc.FillEllipse(new Ellipse(ctr, r, r), b);
            }
        }
    }
}
