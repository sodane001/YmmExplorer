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

public enum NoteKind
{
    [Display(Name = "紙＋テープ＋画びょう")] All,
    [Display(Name = "紙＋テープ")] PaperTape,
    [Display(Name = "紙＋画びょう")] PaperPin,
    [Display(Name = "紙だけ")] PaperOnly,
    [Display(Name = "マスキングテープだけ")] TapeOnly,
    [Display(Name = "画びょうだけ")] PinOnly,
}

// =====================================================================
//  付箋・マスキングテープ・画びょう
//   貼り紙風の飾り。ちぎった紙、斜めに貼ったテープ、画びょうを重ねる
// =====================================================================
public sealed class StickyNotePlugin : IShapePlugin
{
    public string Name => "演出：付箋・マスキングテープ・画びょう";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new StickyNoteParameter(sharedData);
}

public sealed class StickyNoteParameter : EffectShapeParameterBase
{
    public StickyNoteParameter() : this(null) { }
    public StickyNoteParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "組み合わせ")]
    [EnumComboBox]
    public NoteKind Kind { get => kind; set => Set(ref kind, value); }
    NoteKind kind = NoteKind.All;

    [Display(GroupName = "位置と大きさ", Name = "X位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "Y位置")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(50, -100, 200);

    [Display(GroupName = "位置と大きさ", Name = "紙の幅")]
    [AnimationSlider("F0", "px", 100, 1600)]
    public Animation NoteWidth { get; } = new Animation(560, 10, 8000);

    [Display(GroupName = "位置と大きさ", Name = "紙の高さ")]
    [AnimationSlider("F0", "px", 60, 1000)]
    public Animation NoteHeight { get; } = new Animation(380, 10, 8000);

    [Display(GroupName = "位置と大きさ", Name = "傾き")]
    [AnimationSlider("F0", "°", -30, 30)]
    public Animation Rotation { get; } = new Animation(-3, -360, 360);

    [Display(GroupName = "紙", Name = "紙の色")]
    [ColorPicker]
    public WpfColor PaperColor { get => paperColor; set => Set(ref paperColor, value); }
    WpfColor paperColor = WpfColor.FromRgb(0xFF, 0xF3, 0xA8);

    [Display(GroupName = "紙", Name = "下のふちをちぎった形にする")]
    [ToggleSlider]
    public bool Torn { get => torn; set => Set(ref torn, value); }
    bool torn = true;

    [Display(GroupName = "紙", Name = "罫線を引く")]
    [ToggleSlider]
    public bool Ruled { get => ruled; set => Set(ref ruled, value); }
    bool ruled = true;

    [Display(GroupName = "紙", Name = "影の濃さ")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Shadow { get; } = new Animation(40, 0, 100);

    [Display(GroupName = "テープ", Name = "テープの色")]
    [ColorPicker]
    public WpfColor TapeColor { get => tapeColor; set => Set(ref tapeColor, value); }
    WpfColor tapeColor = WpfColor.FromRgb(0x8F, 0xD3, 0xC8);

    [Display(GroupName = "テープ", Name = "テープのしま模様")]
    [ToggleSlider]
    public bool TapeStripes { get => tapeStripes; set => Set(ref tapeStripes, value); }
    bool tapeStripes = true;

    [Display(GroupName = "画びょう", Name = "画びょうの色")]
    [ColorPicker]
    public WpfColor PinColor { get => pinColor; set => Set(ref pinColor, value); }
    WpfColor pinColor = WpfColor.FromRgb(0xE8, 0x3C, 0x3C);

    [Display(GroupName = "画びょう", Name = "画びょうの大きさ")]
    [AnimationSlider("F0", "px", 8, 60)]
    public Animation PinSize { get; } = new Animation(18, 2, 500);

    [Display(GroupName = "動き", Name = "貼られる動きの時間（最初）")]
    [AnimationSlider("F2", "秒", 0, 2)]
    public Animation IntroTime { get; } = new Animation(0.45, 0, 60);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() =>
        [PosX, PosY, NoteWidth, NoteHeight, Rotation, Shadow, PinSize, IntroTime];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        float NW = c.F(NoteWidth), NH = c.F(NoteHeight), sh = c.F(Shadow) / 100f, ps = c.F(PinSize);
        float intro = c.F(IntroTime);
        float k = intro <= 0.001f ? 1f : Math.Clamp(t / intro, 0f, 1f);
        float ek = DrawKit.EaseOutBack(k);
        float scale = 1.25f - 0.25f * ek;
        float rot = Gfx.Rad(c.F(Rotation) + (1f - ek) * 10f);
        float alpha = Math.Clamp(k * 3f, 0f, 1f);
        bool paper = Kind is NoteKind.All or NoteKind.PaperTape or NoteKind.PaperPin or NoteKind.PaperOnly;
        bool tape = Kind is NoteKind.All or NoteKind.PaperTape or NoteKind.TapeOnly;
        bool pin = Kind is NoteKind.All or NoteKind.PaperPin or NoteKind.PinOnly;

        using var brush = dc.CreateSolidColorBrush(default);
        var prev = dc.Transform;
        try
        {
            dc.Transform = Matrix3x2.CreateScale(scale, ctr) * Matrix3x2.CreateRotation(rot, ctr) * prev;
            float x0 = ctr.X - NW / 2f, y0 = ctr.Y - NH / 2f, x1 = ctr.X + NW / 2f, y1 = ctr.Y + NH / 2f;

            if (paper)
            {
                // 紙の形（下のふちはギザギザ）
                var pts = new List<Vector2> { new(x0, y0), new(x1, y0), new(x1, y1 - 6f) };
                int seg = Math.Max(8, (int)(NW / 9f));
                for (int i = 1; i < seg; i++)
                {
                    float x = x1 - NW * i / seg;
                    float jag = Torn ? (DrawKit.H(Seed, i, 1) - 0.5f) * 14f + MathF.Sin(i * 0.9f) * 4f : 0f;
                    pts.Add(new Vector2(x, y1 + jag));
                }
                pts.Add(new Vector2(x0, y1 - 4f));

                // 影（右下へ少しずらし、ぼかす）
                if (sh > 0f)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        var off = new Vector2(6f + j * 3f, 10f + j * 4f);
                        brush.Color = Gfx.C4(0f, 0f, 0f, sh * 0.16f * alpha);
                        Gfx.FillPoly(dc, brush, pts.Select(p => p + off).ToList());
                    }
                }
                brush.Color = Gfx.C4(PaperColor, alpha);
                Gfx.FillPoly(dc, brush, pts);
                // めくれた影（右下が少し暗い）
                using (var lg = Gfx.Linear(dc, new Vector2(x1, y1), new Vector2(x1 - NW * 0.35f, y1 - NH * 0.35f),
                    (0f, Gfx.C4(0f, 0f, 0f, 0.12f * alpha)), (1f, Gfx.C4(0f, 0f, 0f, 0f))))
                    Gfx.FillPoly(dc, lg, pts);
                // 上の帯（付箋ののり部分）
                brush.Color = Gfx.C4(Gfx.Mix(PaperColor, WpfColor.FromRgb(0, 0, 0), 0.06f), alpha);
                dc.FillRectangle(new RawRectF(x0, y0, x1, y0 + NH * 0.12f), brush);
                if (Ruled)
                {
                    brush.Color = Gfx.C4(0.35f, 0.55f, 0.85f, 0.35f * alpha);
                    for (float y = y0 + NH * 0.26f; y < y1 - 18f; y += NH * 0.13f)
                        dc.DrawLine(new Vector2(x0 + 18f, y), new Vector2(x1 - 18f, y), brush, 1.5f);
                    brush.Color = Gfx.C4(0.9f, 0.35f, 0.35f, 0.35f * alpha);
                    dc.DrawLine(new Vector2(x0 + NW * 0.1f, y0 + NH * 0.12f), new Vector2(x0 + NW * 0.1f, y1 - 10f), brush, 1.5f);
                }
            }

            // ---- マスキングテープ ----
            if (tape)
            {
                float tw = MathF.Min(NW * 0.38f, 260f), th = MathF.Max(30f, tw * 0.22f);
                var tc = paper ? new Vector2(x0 + tw * 0.25f, y0 + th * 0.15f) : ctr;
                float ta = paper ? -0.62f : -0.2f;
                var local = new List<Vector2>();
                // 両端はギザギザ（手でちぎった感じ）
                local.Add(new Vector2(-tw / 2f, -th / 2f));
                local.Add(new Vector2(tw / 2f, -th / 2f));
                for (int i = 1; i < 6; i++) local.Add(new Vector2(tw / 2f + ((i & 1) == 1 ? -5f : 2f), -th / 2f + th * i / 6f));
                local.Add(new Vector2(tw / 2f, th / 2f));
                local.Add(new Vector2(-tw / 2f, th / 2f));
                for (int i = 5; i > 0; i--) local.Add(new Vector2(-tw / 2f + ((i & 1) == 1 ? 5f : -2f), -th / 2f + th * i / 6f));
                var poly = DrawKit.Place(local, tc, ta);
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.08f * alpha);
                Gfx.FillPoly(dc, brush, poly.Select(p => p + new Vector2(2f, 3f)).ToList());
                brush.Color = Gfx.C4(TapeColor, 0.78f * alpha);
                Gfx.FillPoly(dc, brush, poly);
                if (TapeStripes)
                {
                    brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.35f * alpha);
                    for (float sx = -tw / 2f + 10f; sx < tw / 2f - 6f; sx += 22f)
                    {
                        var a = DrawKit.Place(new[] { new Vector2(sx, -th / 2f + 2f), new Vector2(sx + 10f, th / 2f - 2f) }, tc, ta);
                        dc.DrawLine(a[0], a[1], brush, 6f);
                    }
                }
                brush.Color = Gfx.C4(WpfColor.FromRgb(255, 255, 255), 0.18f * alpha);
                var edge = DrawKit.Place(new[] { new Vector2(-tw / 2f, -th / 2f + 3f), new Vector2(tw / 2f, -th / 2f + 3f) }, tc, ta);
                dc.DrawLine(edge[0], edge[1], brush, 2f);
            }

            // ---- 画びょう ----
            if (pin)
            {
                var pc = paper ? new Vector2(tape ? x1 - NW * 0.12f : ctr.X, y0 + NH * 0.08f) : ctr;
                float drop = (1f - ek) * -40f;
                pc += new Vector2(0, drop);
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.28f * alpha);
                dc.FillEllipse(new Ellipse(pc + new Vector2(ps * 0.5f, ps * 0.7f), ps * 1.05f, ps * 0.85f), brush);
                brush.Color = Gfx.C4(0.6f, 0.6f, 0.62f, alpha);
                dc.DrawLine(pc, pc + new Vector2(ps * 0.35f, ps * 0.55f), brush, MathF.Max(1f, ps * 0.12f));
                var dark = Gfx.Mix(PinColor, WpfColor.FromRgb(20, 0, 0), 0.5f);
                using (var g = Gfx.Radial(dc, pc - new Vector2(ps * 0.35f, ps * 0.4f), ps * 1.5f,
                    (0f, Gfx.C4(Gfx.Mix(PinColor, WpfColor.FromRgb(255, 255, 255), 0.65f), alpha)), (0.45f, Gfx.C4(PinColor, alpha)), (1f, Gfx.C4(dark, alpha))))
                    dc.FillEllipse(new Ellipse(pc, ps, ps), g);
                brush.Color = Gfx.C4(dark, 0.6f * alpha);
                dc.DrawEllipse(new Ellipse(pc, ps * 0.62f, ps * 0.62f), brush, MathF.Max(1f, ps * 0.08f));
                brush.Color = Gfx.C4(1f, 1f, 1f, 0.8f * alpha);
                dc.FillEllipse(new Ellipse(pc - new Vector2(ps * 0.35f, ps * 0.38f), ps * 0.2f, ps * 0.14f), brush);
            }
        }
        finally
        {
            dc.Transform = prev;
        }
    }
}
