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

public enum InputDevice
{
    [Display(Name = "ゲームパッド")] Pad,
    [Display(Name = "キーボード（WASD）")] Keys,
    [Display(Name = "キーボード（矢印キー）")] Arrows,
    [Display(Name = "マウス")] Mouse,
}

// =====================================================================
//  コントローラー・キーボード入力表示
//   押しているボタンが光る。押すボタンは「A,→」のように書く。
//   移動(時間で変わる)にしたい時は「0.5:A」のように、秒数をつけて並べる
// =====================================================================
public sealed class InputDisplayPlugin : IShapePlugin
{
    public string Name => "演出：コントローラー・キーボード表示";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new InputDisplayParameter(sharedData);
}

public sealed class InputDisplayParameter : EffectShapeParameterBase
{
    public InputDisplayParameter() : this(null) { }
    public InputDisplayParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "入力", Name = "種類")]
    [EnumComboBox]
    public InputDevice Device { get => device; set => Set(ref device, value); }
    InputDevice device = InputDevice.Pad;

    [Display(GroupName = "入力", Name = "押すボタン", Description = "ゲームパッド: A B X Y ↑ ↓ ← → L R / キーボード: W A S D Q E SPACE SHIFT など / マウス: 左 右 ホイール。「1.0:A,B」のように秒数をつけて1行ずつ書くと、時間で切り替わります")]
    [TextEditor(AcceptsReturn = true)]
    public string Pressed { get => pressed; set => Set(ref pressed, value); }
    string pressed = "0:A\n0.4:→\n0.8:B,→\n1.2:";

    [Display(GroupName = "入力", Name = "自動でそれらしく押す（デモ）")]
    [ToggleSlider]
    public bool Auto { get => auto; set => Set(ref auto, value); }
    bool auto = false;

    [Display(GroupName = "色", Name = "本体の色")]
    [ColorPicker]
    public WpfColor BaseColor { get => baseColor; set => Set(ref baseColor, value); }
    WpfColor baseColor = WpfColor.FromRgb(0x2A, 0x2D, 0x36);

    [Display(GroupName = "色", Name = "押した時の色")]
    [ColorPicker]
    public WpfColor PressColor { get => pressColor; set => Set(ref pressColor, value); }
    WpfColor pressColor = WpfColor.FromRgb(0x4F, 0xC3, 0xF7);

    [Display(GroupName = "色", Name = "不透明度")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation Opacity { get; } = new Animation(90, 0, 100);

    [Display(GroupName = "位置", Name = "X位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosX { get; } = new Animation(85, -100, 200);

    [Display(GroupName = "位置", Name = "Y位置（中心）")]
    [AnimationSlider("F0", "%", 0, 100)]
    public Animation PosY { get; } = new Animation(82, -100, 200);

    [Display(GroupName = "位置", Name = "大きさ")]
    [AnimationSlider("F0", "%", 20, 300)]
    public Animation Size { get; } = new Animation(100, 1, 2000);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [Opacity, PosX, PosY, Size];

    /// <summary>いま押しているボタンの一覧(大文字・記号に、そろえる)</summary>
    HashSet<string> Current(float t)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string src = TextKit.Lines(Pressed);
        if (Auto)
        {
            string[] pool = Device switch
            {
                InputDevice.Keys => new[] { "W", "A", "S", "D", "SPACE", "SHIFT", "E" },
                InputDevice.Arrows => new[] { "↑", "↓", "←", "→", "Z", "X" },
                InputDevice.Mouse => new[] { "左", "右", "ホイール" },
                _ => new[] { "A", "B", "X", "Y", "↑", "↓", "←", "→", "L", "R" },
            };
            int slot = (int)MathF.Floor(t / 0.22f);
            for (int k = 0; k < 2; k++)
            {
                float r = DrawKit.H(Seed, slot, k);
                if (r < 0.55f) set.Add(pool[(int)(DrawKit.H(Seed, slot, k + 5) * pool.Length) % pool.Length]);
            }
            return set;
        }
        string active = "";
        bool timed = false;
        foreach (var raw in src.Split('\n'))
        {
            var line = raw.Trim();
            int colon = line.IndexOf(':');
            if (colon > 0 && float.TryParse(line[..colon], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var sec))
            {
                timed = true;
                if (t >= sec) active = line[(colon + 1)..];
            }
        }
        if (!timed) active = src;
        foreach (var part in active.Split(new[] { ',', '、', ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var p = part.Trim().ToUpperInvariant();
            p = p switch { "UP" or "上" => "↑", "DOWN" or "下" => "↓", "LEFT" => "←", "RIGHT" => "→", "SPACE" or "スペース" => "SPACE", "LMB" or "LEFT CLICK" => "左", "RMB" => "右", _ => p };
            set.Add(p);
        }
        return set;
    }

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height, t = c.Time;
        float u = c.F(Size) / 100f, alpha = c.F(Opacity) / 100f;
        if (alpha <= 0.002f) return;
        var ctr = new Vector2(c.F(PosX) / 100f * w, c.F(PosY) / 100f * h);
        var on = Current(t);
        using var brush = dc.CreateSolidColorBrush(default);
        var baseCol = Gfx.C4(BaseColor, alpha);
        var edge = Gfx.C4(Gfx.Mix(BaseColor, WpfColor.FromRgb(255, 255, 255), 0.25f), alpha);
        var label = Gfx.C4(0.92f, 0.94f, 0.97f, alpha);

        void Key(string id, string text, Vector2 p, float kw, float kh)
        {
            bool down = on.Contains(id);
            var rr = ShapeGeom.RoundRect(p.X, p.Y + (down ? 3f * u : 0f), kw, kh, MathF.Min(kw, kh) * 0.18f);
            if (!down)
            {
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.45f * alpha);
                Gfx.FillPoly(dc, brush, rr.Select(q => q + new Vector2(0f, 5f * u)).ToList());
            }
            brush.Color = down ? Gfx.C4(PressColor, alpha) : baseCol;
            Gfx.FillPoly(dc, brush, rr);
            brush.Color = down ? Gfx.C4(1f, 1f, 1f, 0.7f * alpha) : edge;
            Gfx.Polyline(dc, brush, ShapeGeom.Closed(rr), 2f * u);
            if (down) DrawKit.Glow(dc, p, MathF.Max(kw, kh) * 0.8f, Gfx.C4(PressColor, 0.35f * alpha));
            var s = TextKit.Get(text, MathF.Min(kh * 0.42f, kw * 0.36f), true);
            TextKit.DrawCentered(dc, brush, s, p + new Vector2(0f, down ? 3f * u : 0f), down ? Gfx.C4(0.05f, 0.05f, 0.08f, alpha) : label);
        }

        void Round(string id, string text, Vector2 p, float r, WpfColor tint)
        {
            bool down = on.Contains(id);
            brush.Color = Gfx.C4(0f, 0f, 0f, 0.4f * alpha);
            if (!down) dc.FillEllipse(new Ellipse(p + new Vector2(0f, 4f * u), r, r), brush);
            brush.Color = down ? Gfx.C4(PressColor, alpha) : Gfx.C4(Gfx.Mix(BaseColor, tint, 0.35f), alpha);
            dc.FillEllipse(new Ellipse(p + new Vector2(0f, down ? 3f * u : 0f), r, r), brush);
            if (down) DrawKit.Glow(dc, p, r * 1.8f, Gfx.C4(PressColor, 0.4f * alpha));
            var s = TextKit.Get(text, r * 0.95f, true);
            TextKit.DrawCentered(dc, brush, s, p + new Vector2(0f, down ? 3f * u : 0f), down ? Gfx.C4(0.05f, 0.05f, 0.08f, alpha) : Gfx.C4(tint, alpha));
        }

        switch (Device)
        {
            case InputDevice.Keys:
            {
                float k = 74f * u, g = 8f * u;
                Key("Q", "Q", ctr + new Vector2(-(k + g), -(k + g) / 2f), k, k);
                Key("W", "W", ctr + new Vector2(0f, -(k + g) / 2f), k, k);
                Key("E", "E", ctr + new Vector2(k + g, -(k + g) / 2f), k, k);
                Key("A", "A", ctr + new Vector2(-(k + g) * 0.75f, (k + g) / 2f), k, k);
                Key("S", "S", ctr + new Vector2((k + g) * 0.25f, (k + g) / 2f), k, k);
                Key("D", "D", ctr + new Vector2((k + g) * 1.25f, (k + g) / 2f), k, k);
                Key("SHIFT", "SHIFT", ctr + new Vector2(-(k + g) * 2.1f, (k + g) / 2f), k * 1.5f, k);
                Key("SPACE", "SPACE", ctr + new Vector2((k + g) * 0.2f, (k + g) * 1.5f), k * 4.4f, k * 0.8f);
                break;
            }
            case InputDevice.Arrows:
            {
                float k = 74f * u, g = 8f * u;
                Key("↑", "↑", ctr + new Vector2(0f, -(k + g) / 2f), k, k);
                Key("←", "←", ctr + new Vector2(-(k + g), (k + g) / 2f), k, k);
                Key("↓", "↓", ctr + new Vector2(0f, (k + g) / 2f), k, k);
                Key("→", "→", ctr + new Vector2(k + g, (k + g) / 2f), k, k);
                Key("Z", "Z", ctr + new Vector2(-(k + g) * 3.2f, (k + g) / 2f), k, k);
                Key("X", "X", ctr + new Vector2(-(k + g) * 2.2f, (k + g) / 2f), k, k);
                break;
            }
            case InputDevice.Mouse:
            {
                float mw = 150f * u, mh = 230f * u;
                var body = DrawKit.EllipsePts(ctr, mw / 2f, mh / 2f, 64);
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.4f * alpha);
                Gfx.FillPoly(dc, brush, body.Select(q => q + new Vector2(0f, 6f * u)).ToList());
                brush.Color = baseCol;
                Gfx.FillPoly(dc, brush, body);
                float top = ctr.Y - mh / 2f, split = ctr.Y - mh * 0.08f;
                // 左右のボタン(楕円の上半分を、まん中で分ける)
                foreach (var (id, sign) in new[] { ("左", -1f), ("右", 1f) })
                {
                    var half = body.Where(q => q.Y <= split && (sign < 0 ? q.X <= ctr.X : q.X >= ctr.X)).ToList();
                    half.Add(new Vector2(ctr.X, split));
                    half.Add(new Vector2(ctr.X, top));
                    half = half.OrderBy(q => MathF.Atan2(q.Y - (top + split) / 2f, q.X - (ctr.X + sign * mw * 0.2f))).ToList();
                    if (on.Contains(id))
                    {
                        brush.Color = Gfx.C4(PressColor, alpha);
                        Gfx.FillPoly(dc, brush, half);
                        DrawKit.Glow(dc, new Vector2(ctr.X + sign * mw * 0.22f, (top + split) / 2f), mw * 0.5f, Gfx.C4(PressColor, 0.35f * alpha));
                    }
                }
                brush.Color = edge;
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(body), 2.5f * u);
                dc.DrawLine(new Vector2(ctr.X - mw / 2f, split), new Vector2(ctr.X + mw / 2f, split), brush, 2.5f * u);
                dc.DrawLine(new Vector2(ctr.X, top), new Vector2(ctr.X, split), brush, 2.5f * u);
                bool wheel = on.Contains("ホイール") || on.Contains("WHEEL");
                var wr = new RawRectF(ctr.X - 9f * u, top + mh * 0.14f, ctr.X + 9f * u, top + mh * 0.3f);
                brush.Color = wheel ? Gfx.C4(PressColor, alpha) : edge;
                dc.FillRectangle(wr, brush);
                break;
            }
            default:
            {
                // ゲームパッド
                float bw = 420f * u, bh = 230f * u;
                var shell = new List<Vector2>();
                shell.AddRange(Gfx.ArcPts(ctr + new Vector2(-bw * 0.3f, bh * 0.05f), bh * 0.55f, MathF.PI * 0.55f, MathF.PI * 1.5f, 24));
                shell.AddRange(Gfx.ArcPts(ctr + new Vector2(bw * 0.3f, bh * 0.05f), bh * 0.55f, -MathF.PI * 0.5f, MathF.PI * 0.45f, 24));
                shell.Add(ctr + new Vector2(bw * 0.12f, bh * 0.35f));
                shell.Add(ctr + new Vector2(-bw * 0.12f, bh * 0.35f));
                brush.Color = Gfx.C4(0f, 0f, 0f, 0.4f * alpha);
                Gfx.FillPoly(dc, brush, shell.Select(q => q + new Vector2(0f, 7f * u)).ToList());
                brush.Color = baseCol;
                Gfx.FillPoly(dc, brush, shell);
                brush.Color = edge;
                Gfx.Polyline(dc, brush, ShapeGeom.Closed(shell), 2.5f * u);
                // 十字キー
                var dp = ctr + new Vector2(-bw * 0.3f, 0f);
                float d = 36f * u;
                Key("↑", "", dp + new Vector2(0, -d), d, d);
                Key("↓", "", dp + new Vector2(0, d), d, d);
                Key("←", "", dp + new Vector2(-d, 0), d, d);
                Key("→", "", dp + new Vector2(d, 0), d, d);
                brush.Color = baseCol;
                dc.FillRectangle(new RawRectF(dp.X - d / 2f, dp.Y - d / 2f, dp.X + d / 2f, dp.Y + d / 2f), brush);
                // ABXY
                var fb = ctr + new Vector2(bw * 0.3f, 0f);
                float r = 21f * u, o = 40f * u;
                Round("Y", "Y", fb + new Vector2(0, -o), r, WpfColor.FromRgb(0xFF, 0xD5, 0x4A));
                Round("A", "A", fb + new Vector2(0, o), r, WpfColor.FromRgb(0x5D, 0xDB, 0x7A));
                Round("X", "X", fb + new Vector2(-o, 0), r, WpfColor.FromRgb(0x4F, 0xA8, 0xFF));
                Round("B", "B", fb + new Vector2(o, 0), r, WpfColor.FromRgb(0xFF, 0x5C, 0x5C));
                // L・R
                Key("L", "L", ctr + new Vector2(-bw * 0.3f, -bh * 0.58f), 110f * u, 34f * u);
                Key("R", "R", ctr + new Vector2(bw * 0.3f, -bh * 0.58f), 110f * u, 34f * u);
                break;
            }
        }
    }
}
