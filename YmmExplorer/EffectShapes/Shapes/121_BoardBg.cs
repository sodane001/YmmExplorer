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

public enum BoardKind
{
    [Display(Name = "黒板")] Chalkboard,
    [Display(Name = "ホワイトボード")] Whiteboard,
    [Display(Name = "方眼ノート")] GridNote,
    [Display(Name = "罫線ノート")] LinedNote,
}

// =====================================================================
//  黒板・ホワイトボード・方眼ノートの背景
//   解説の土台になる背景。文字や図は上に重ねてください
// =====================================================================
public sealed class BoardBgPlugin : IShapePlugin
{
    public string Name => "演出：黒板・ホワイトボード・ノートの背景";
    public bool IsExoShapeSupported => false;
    public bool IsExoMaskSupported => false;
    public IShapeParameter CreateShapeParameter(SharedDataStore? sharedData) => new BoardBgParameter(sharedData);
}

public sealed class BoardBgParameter : EffectShapeParameterBase
{
    public BoardBgParameter() : this(null) { }
    public BoardBgParameter(SharedDataStore? sharedData) : base(sharedData) { }

    [Display(GroupName = "種類", Name = "種類")]
    [EnumComboBox]
    public BoardKind Kind { get => kind; set => Set(ref kind, value); }
    BoardKind kind = BoardKind.Chalkboard;

    [Display(GroupName = "種類", Name = "ふち（枠・トレー・リング）を描く")]
    [ToggleSlider]
    public bool ShowFrame { get => frame; set => Set(ref frame, value); }
    bool frame = true;

    [Display(GroupName = "種類", Name = "よごれ・消し跡を描く")]
    [ToggleSlider]
    public bool Smudge { get => smudge; set => Set(ref smudge, value); }
    bool smudge = true;

    [Display(GroupName = "種類", Name = "マス目・罫線の間隔")]
    [AnimationSlider("F0", "px", 10, 120)]
    public Animation GridSize { get; } = new Animation(40, 4, 1000);

    [Display(GroupName = "種類", Name = "板・紙の色（透明のままなら種類のおすすめの色）")]
    [ColorPicker]
    public WpfColor BaseColor { get => baseColor; set => Set(ref baseColor, value); }
    WpfColor baseColor = WpfColor.FromArgb(0, 0, 0, 0);

    protected override IEnumerable<IAnimatable> GetExtraAnimatables() => [GridSize];

    public override void Draw(ID2D1DeviceContext dc, in DrawContext c)
    {
        float w = c.Width, h = c.Height;
        float S = MathF.Min(w, h) / 1080f;
        float g = MathF.Max(4f, c.F(GridSize));
        bool useBase = BaseColor.A > 0;
        using var brush = dc.CreateSolidColorBrush(default);
        var rng = new Random(Seed);

        switch (Kind)
        {
            case BoardKind.Chalkboard:
            case BoardKind.Whiteboard:
                {
                    bool chalk = Kind == BoardKind.Chalkboard;
                    float fr = ShowFrame ? 34f * S : 0f;
                    float tray = ShowFrame ? 40f * S : 0f;
                    var board = new RawRectF(fr, fr, w - fr, h - fr - tray);
                    var col = useBase ? BaseColor : chalk ? WpfColor.FromRgb(0x23, 0x4A, 0x36) : WpfColor.FromRgb(0xF6, 0xF7, 0xF8);
                    // 枠
                    if (ShowFrame)
                    {
                        var fc = chalk ? WpfColor.FromRgb(0x8A, 0x5A, 0x32) : WpfColor.FromRgb(0xB8, 0xBE, 0xC6);
                        using (var lg = Gfx.Linear(dc, Vector2.Zero, new Vector2(0, h), (0f, Gfx.C4(Gfx.Mix(fc, WpfColor.FromRgb(255, 255, 255), 0.2f), 1f)), (1f, Gfx.C4(Gfx.Mix(fc, WpfColor.FromRgb(0, 0, 0), 0.25f), 1f))))
                            dc.FillRectangle(new RawRectF(0, 0, w, h), lg);
                        brush.Color = Gfx.C4(0f, 0f, 0f, 0.25f);
                        dc.DrawRectangle(new RawRectF(board.Left - 2f, board.Top - 2f, board.Right + 2f, board.Bottom + 2f), brush, 4f);
                    }
                    using (var bg = Gfx.Radial(dc, new Vector2(w * 0.45f, h * 0.4f), MathF.Max(w, h) * 0.8f,
                        (0f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(255, 255, 255), chalk ? 0.06f : 0.02f), 1f)), (1f, Gfx.C4(Gfx.Mix(col, WpfColor.FromRgb(0, 0, 0), chalk ? 0.25f : 0.06f), 1f))))
                        dc.FillRectangle(board, bg);
                    if (Smudge)
                    {
                        // 消し跡（ぼんやりした横長のもや）
                        for (int i = 0; i < 9; i++)
                        {
                            var p = new Vector2(board.Left + rng.NextF() * (board.Right - board.Left), board.Top + rng.NextF() * (board.Bottom - board.Top));
                            float rx = (120f + 260f * rng.NextF()) * S, ry = rx * 0.35f;
                            var prev = dc.Transform;
                            dc.Transform = Matrix3x2.CreateScale(1f, ry / rx, p) * prev;
                            using (var sg = Gfx.Radial(dc, p, rx, (0f, chalk ? Gfx.C4(1f, 1f, 1f, 0.07f) : Gfx.C4(0.3f, 0.35f, 0.45f, 0.05f)), (1f, Gfx.C4(1f, 1f, 1f, 0f))))
                                dc.FillEllipse(new Ellipse(p, rx, rx), sg);
                            dc.Transform = prev;
                        }
                        // 細かい粉・反射
                        if (chalk)
                        {
                            brush.Color = Gfx.C4(1f, 1f, 1f, 0.05f);
                            for (int i = 0; i < 400; i++)
                                dc.FillEllipse(new Ellipse(new Vector2(board.Left + rng.NextF() * (board.Right - board.Left), board.Top + rng.NextF() * (board.Bottom - board.Top)), 1.2f, 1.2f), brush);
                        }
                        else
                        {
                            using var lg = Gfx.Linear(dc, new Vector2(board.Left, board.Top), new Vector2(board.Left + (board.Right - board.Left) * 0.5f, board.Bottom),
                                (0f, Gfx.C4(1f, 1f, 1f, 0f)), (0.45f, Gfx.C4(1f, 1f, 1f, 0.5f)), (0.55f, Gfx.C4(1f, 1f, 1f, 0f)));
                            dc.FillRectangle(board, lg);
                        }
                    }
                    // トレー
                    if (ShowFrame)
                    {
                        var tc = chalk ? WpfColor.FromRgb(0x6E, 0x46, 0x26) : WpfColor.FromRgb(0x9A, 0xA2, 0xAC);
                        brush.Color = Gfx.C4(tc, 1f);
                        dc.FillRectangle(new RawRectF(fr * 0.5f, h - fr - tray, w - fr * 0.5f, h - fr - tray * 0.3f), brush);
                        brush.Color = Gfx.C4(0f, 0f, 0f, 0.3f);
                        dc.FillRectangle(new RawRectF(fr * 0.5f, h - fr - tray * 0.3f, w - fr * 0.5f, h - fr - tray * 0.2f), brush);
                        float ty = h - fr - tray * 0.65f;
                        if (chalk)
                        {
                            var chalkCols = new[] { WpfColor.FromRgb(250, 250, 245), WpfColor.FromRgb(255, 200, 70), WpfColor.FromRgb(255, 120, 130) };
                            for (int i = 0; i < 3; i++)
                            {
                                float cx = w * 0.15f + i * 70f * S;
                                brush.Color = Gfx.C4(chalkCols[i], 1f);
                                dc.DrawLine(new Vector2(cx, ty), new Vector2(cx + 46f * S, ty), brush, 11f * S);
                            }
                            // 黒板消し
                            brush.Color = Gfx.C4(0.75f, 0.68f, 0.55f, 1f);
                            dc.FillRectangle(new RawRectF(w * 0.72f, ty - 14f * S, w * 0.72f + 120f * S, ty + 6f * S), brush);
                            brush.Color = Gfx.C4(0.25f, 0.25f, 0.28f, 1f);
                            dc.FillRectangle(new RawRectF(w * 0.72f, ty + 2f * S, w * 0.72f + 120f * S, ty + 8f * S), brush);
                        }
                        else
                        {
                            var markers = new[] { WpfColor.FromRgb(30, 30, 35), WpfColor.FromRgb(220, 40, 50), WpfColor.FromRgb(30, 90, 220) };
                            for (int i = 0; i < 3; i++)
                            {
                                float cx = w * 0.15f + i * 120f * S;
                                brush.Color = Gfx.C4(0.95f, 0.95f, 0.96f, 1f);
                                dc.DrawLine(new Vector2(cx, ty), new Vector2(cx + 90f * S, ty), brush, 16f * S);
                                brush.Color = Gfx.C4(markers[i], 1f);
                                dc.DrawLine(new Vector2(cx + 62f * S, ty), new Vector2(cx + 96f * S, ty), brush, 17f * S);
                            }
                        }
                    }
                    break;
                }
            default:
                {
                    // ---- ノート ----
                    bool grid = Kind == BoardKind.GridNote;
                    var paper = useBase ? BaseColor : WpfColor.FromRgb(0xFC, 0xFB, 0xF6);
                    brush.Color = Gfx.C4(paper, 1f);
                    dc.FillRectangle(new RawRectF(0, 0, w, h), brush);
                    float left = ShowFrame ? 120f * S : 0f;
                    var lineC = grid ? Gfx.C4(0.55f, 0.75f, 0.85f, 0.45f) : Gfx.C4(0.5f, 0.65f, 0.85f, 0.55f);
                    brush.Color = lineC;
                    if (grid)
                    {
                        int k = 0;
                        for (float x = left + g; x < w; x += g, k++)
                            dc.DrawLine(new Vector2(x, 0), new Vector2(x, h), brush, k % 5 == 4 ? 1.6f : 0.8f);
                        k = 0;
                        for (float y = g; y < h; y += g, k++)
                            dc.DrawLine(new Vector2(left, y), new Vector2(w, y), brush, k % 5 == 4 ? 1.6f : 0.8f);
                    }
                    else
                    {
                        float lg = g * 1.6f;
                        for (float y = lg * 2f; y < h; y += lg)
                            dc.DrawLine(new Vector2(left, y), new Vector2(w, y), brush, 1.2f);
                        brush.Color = Gfx.C4(0.9f, 0.4f, 0.45f, 0.6f);
                        dc.DrawLine(new Vector2(left + 90f * S, 0), new Vector2(left + 90f * S, h), brush, 1.5f);
                    }
                    if (Smudge)
                    {
                        using var sg = Gfx.Radial(dc, new Vector2(w * 0.5f, h * 0.5f), MathF.Max(w, h) * 0.75f, (0f, Gfx.C4(0f, 0f, 0f, 0f)), (0.7f, Gfx.C4(0.35f, 0.3f, 0.2f, 0.03f)), (1f, Gfx.C4(0.35f, 0.3f, 0.2f, 0.12f)));
                        dc.FillRectangle(new RawRectF(0, 0, w, h), sg);
                    }
                    if (ShowFrame)
                    {
                        // 左のリング（スパイラル）
                        brush.Color = Gfx.C4(0.92f, 0.91f, 0.88f, 1f);
                        dc.FillRectangle(new RawRectF(0, 0, left * 0.55f, h), brush);
                        for (float y = 40f * S; y < h - 20f * S; y += 62f * S)
                        {
                            brush.Color = Gfx.C4(0.2f, 0.2f, 0.22f, 1f);
                            dc.FillEllipse(new Ellipse(new Vector2(left * 0.62f, y), 9f * S, 9f * S), brush);
                            brush.Color = Gfx.C4(0.62f, 0.64f, 0.68f, 1f);
                            var ring = DrawKit.EllipsePts(new Vector2(left * 0.38f, y), left * 0.26f, 12f * S, 24, MathF.PI * 0.95f, MathF.PI * 2.05f);
                            Gfx.Polyline(dc, brush, ring, 5f * S + 1f);
                        }
                    }
                    break;
                }
        }
    }
}
