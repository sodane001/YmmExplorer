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
//  七夕
//   風にゆれる笹と色とりどりの短冊。天の川の夜空つき
// =====================================================================
public sealed class TanabataPlugin : IShapePlugin
{
    public string Name => "演出：七夕（笹と短冊）";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new TanabataParameter(sharedData);
}

public sealed class TanabataParameter : EffectShapeParameterBase
{
    public TanabataParameter() : this(null) { }
    public TanabataParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "笹", Name = "笹のX位置（根元）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(80, -100, 200);

    [Display(GroupName = "笹", Name = "笹の高さ")]
    [AnimationSlider("F0", "%", 30, 120)]
    public Animation StalkHeight { get; } = new Animation(90, 5, 200);

    [Display(GroupName = "笹", Name = "短冊の数")]
    [AnimationSlider("F0", "枚", 0, 30)]
    public Animation Strips { get; } = new Animation(12, 0, 100);

    [Display(GroupName = "笹", Name = "風の強さ")]
    [AnimationSlider("F0", "%", 0, 300)]
    public Animation Wind { get; } = new Animation(100, 0, 2000);

    [Display(GroupName = "背景", Name = "天の川の夜空を描く")]
    [ToggleSlider]
    public bool NightSky { get => nightSky; set => Set(ref nightSky, value); }
    bool nightSky = true;

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [PosX, StalkHeight, Strips, Wind];

    static readonly WpfColor[] StripColors =
    {
        WpfColor.FromRgb(0xE8, 0x3C, 0x4A), WpfColor.FromRgb(0xF2, 0xC2, 0x2E), WpfColor.FromRgb(0x3C, 0xB4, 0x6A),
        WpfColor.FromRgb(0x3C, 0x8A, 0xE8), WpfColor.FromRgb(0xB0, 0x5C, 0xD8), WpfColor.FromRgb(0xF6, 0xF2, 0xE8),
    };

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float wind = c.F(Wind) / 100f;
        var white = WpfColor.FromRgb(255, 255, 255);
        using var brush = dc.CreateSolidColorBrush(default);
        try
        {
            if (NightSky)
            {
                using (var sky = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, h), (0f, Gfx.C4(0.02f, 0.03f, 0.12f, 1f)), (1f, Gfx.C4(0.08f, 0.1f, 0.28f, 1f))))
                    dc.FillRectangle(new RawRectF(0, 0, w, h), sky);
                dc.PrimitiveBlend = PrimitiveBlend.Add;
                // 天の川（斜めの帯）
                var a0 = new Vector2(-w * 0.1f, h * 0.9f);
                var a1 = new Vector2(w * 0.85f, -h * 0.1f);
                for (int i = 0; i < 14; i++)
                {
                    var p = Vector2.Lerp(a0, a1, i / 13f);
                    DrawKit.Glow(dc, p, MathF.Min(w, h) * 0.22f, Gfx.C4(0.55f, 0.6f, 0.95f, 0.12f));
                }
                var rng = new Random(Seed);
                var dir = Vector2.Normalize(a1 - a0);
                var nrm = new Vector2(-dir.Y, dir.X);
                for (int i = 0; i < 700; i++)
                {
                    bool band = i < 450;
                    var p = band ? Vector2.Lerp(a0, a1, rng.NextF()) + nrm * (rng.NextF() + rng.NextF() + rng.NextF() - 1.5f) * h * 0.12f
                                 : new Vector2(rng.NextF() * w, rng.NextF() * h);
                    float tw = 0.6f + 0.4f * MathF.Sin(t * (1f + 2f * rng.NextF()) + i);
                    brush.Color = Gfx.C4(white, (band ? 0.55f : 0.7f) * tw * rng.NextF());
                    float s = band ? 0.8f : 0.6f + rng.NextF();
                    dc.FillEllipse(new Ellipse(p, s, s), brush);
                }
                // 織姫と彦星
                DrawKit.Twinkle(dc, brush, new Vector2(w * 0.2f, h * 0.35f), 18f, 1.5f, white, 0.9f, t * 0.2f);
                DrawKit.Twinkle(dc, brush, new Vector2(w * 0.62f, h * 0.62f), 16f, 1.5f, white, 0.9f, -t * 0.2f);
                dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
            }

            // ---- 笹 ----
            var root = new Vector2(c.F(PosX) / 100f * w, h + 10f);
            float H = h * c.F(StalkHeight) / 100f;
            float sway = MathF.Sin(t * 0.9f) * 0.06f * wind;
            var stalk = new List<Vector2>();
            for (int i = 0; i <= 24; i++)
            {
                float u = i / 24f;
                stalk.Add(root + new Vector2(-u * u * H * 0.18f + MathF.Sin(sway * 10f) * u * u * H * 0.12f, -u * H));
            }
            brush.Color = Gfx.C4(0.35f, 0.55f, 0.22f, 1f);
            for (int i = 0; i < stalk.Count - 1; i++)
                dc.DrawLine(stalk[i], stalk[i + 1], brush, MathF.Max(3f, 16f * (1f - i / 24f) + 3f));
            // 節
            brush.Color = Gfx.C4(0.25f, 0.4f, 0.15f, 1f);
            for (int i = 3; i < stalk.Count; i += 4)
                dc.DrawLine(stalk[i] - new Vector2(10f, 0), stalk[i] + new Vector2(10f, 0), brush, 3f);

            // 枝と葉
            var leafPts = new List<(Vector2 p, float a, float s)>();
            for (int b = 0; b < 9; b++)
            {
                float u = 0.3f + 0.7f * b / 8f;
                var basePt = stalk[(int)(u * 24f)];
                float side = b % 2 == 0 ? -1f : 1f;
                float len = H * (0.28f - 0.15f * u);
                var tip = basePt + new Vector2(side * len, -len * 0.25f + MathF.Sin(t * 1.3f + b) * 10f * wind);
                brush.Color = Gfx.C4(0.3f, 0.5f, 0.2f, 1f);
                dc.DrawLine(basePt, tip, brush, 3f);
                for (int k = 0; k < 5; k++)
                {
                    var lp = Vector2.Lerp(basePt, tip, 0.35f + 0.16f * k);
                    float la = (side > 0 ? 0.6f : MathF.PI - 0.6f) + (k % 2 == 0 ? 0.5f : -0.5f) + MathF.Sin(t * 2f + b + k) * 0.15f * wind;
                    leafPts.Add((lp, la, H * 0.05f));
                }
            }
            foreach (var (p, a, s) in leafPts)
            {
                var leaf = DrawKit.Place(new[] { new Vector2(0, 0), new Vector2(s * 0.6f, -s * 0.22f), new Vector2(s * 1.8f, 0), new Vector2(s * 0.6f, s * 0.22f) }, p, a);
                brush.Color = Gfx.C4(0.25f, 0.62f, 0.3f, 1f);
                Gfx.FillPoly(dc, brush, leaf);
            }

            // ---- 短冊 ----
            int n = (int)c.F(Strips);
            for (int i = 0; i < n; i++)
            {
                var lp = leafPts[(int)(DrawKit.H(Seed, i, 1) * leafPts.Count) % Math.Max(1, leafPts.Count)].p;
                float sw = H * 0.035f, sh = H * 0.13f;
                float swing = MathF.Sin(t * (1.5f + DrawKit.H(Seed, i, 2)) + i) * 0.25f * wind;
                var col = StripColors[i % StripColors.Length];
                var top = lp + new Vector2(0, 6f);
                brush.Color = Gfx.C4(0.95f, 0.95f, 0.9f, 0.8f);
                var hang = top + new Vector2(MathF.Sin(swing) * 14f, MathF.Cos(swing) * 14f);
                dc.DrawLine(lp, hang, brush, 1f);
                var strip = DrawKit.Place(new[] { new Vector2(-sw / 2f, 0), new Vector2(sw / 2f, 0), new Vector2(sw / 2f, sh), new Vector2(-sw / 2f, sh) }, hang, -swing);
                brush.Color = Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(0, 0, 0), 0.2f), 1f);
                Gfx.FillPoly(dc, brush, strip.Select(q => q + new Vector2(2f, 2f)).ToList());
                brush.Color = Gfx.C4(col, 1f);
                Gfx.FillPoly(dc, brush, strip);
                // 願いごとの線（文字の代わり）
                brush.Color = Gfx.C4(0.15f, 0.12f, 0.1f, 0.4f);
                var l0 = DrawKit.Place(new[] { new Vector2(0, sh * 0.15f), new Vector2(0, sh * 0.85f) }, hang, -swing);
                dc.DrawLine(l0[0], l0[1], brush, 1.5f);
            }
        }
        finally
        {
            dc.PrimitiveBlend = PrimitiveBlend.SourceOver;
        }
    }
}
