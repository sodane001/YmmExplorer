using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YmmExplorer;

// =====================================================================
//  図形の配置画面(アイテムタブ)
//   演出図形それぞれの「位置・向き・長さ・大きさ」を、画面を模した板の上で
//   つまみをドラッグして決める。図形ごとに、どのつまみを出すかを下の表で決めている。
//   (例: ビーム = 発射位置と着弾地点。着弾地点を動かすと、向きと長さが決まる)
// =====================================================================

/// <summary>角度の 0° がどちらを向くか</summary>
public enum StageZero
{
    /// <summary>0°=右、90°=下(ふつう)</summary>
    Right,
    /// <summary>0°=真上、90°=右(光の剣など)</summary>
    Up,
    /// <summary>0°=真下、プラスで右へ傾く(雨など)</summary>
    Down,
}

/// <summary>四角の、位置(X・Y)が指している所</summary>
public enum StageAnchor { Center, LeftCenter, BottomCenter, TopLeft }

public enum StageKind { Point, Ray, Circle, Rect, Span, Segment, Path, HLine, VLine, Dial }

/// <summary>配置画面に出す、つまみ1組の決まり(どの項目を、どう動かすか)</summary>
public sealed class StageHandle
{
    public StageKind Kind { get; init; }
    public string Label { get; init; } = "";
    public string OriginLabel { get; init; } = "";
    public string? X { get; init; }
    public string? Y { get; init; }
    public string? X2 { get; init; }
    public string? Y2 { get; init; }
    public string? Angle { get; init; }
    public string? Len { get; init; }
    public string? Size { get; init; }
    public string? W { get; init; }
    public string? H { get; init; }
    public string? Rot { get; init; }
    public string? Spread { get; init; }
    public string? Count { get; init; }
    public string[]? Xs { get; init; }
    public string[]? Ys { get; init; }
    public StageZero Zero { get; init; }
    public StageAnchor Anchor { get; init; }
    public bool Dashed { get; init; }
    /// <summary>縦線のつまみを置く高さ(描く範囲の高さに対する割合)。図形の足元に合わせる</summary>
    public double GripY { get; init; } = 0.92;

    // ---- 作り方 ----
    public static StageHandle Pt(string label, string x, string y) => new() { Kind = StageKind.Point, Label = label, X = x, Y = y };

    /// <summary>ある点から伸びる向き(と長さ)。先のつまみを動かすと、向きと長さが決まる</summary>
    public static StageHandle Ray(string origin, string end, string x, string y, string angle, string? len = null, StageZero zero = StageZero.Right, string? spread = null)
        => new() { Kind = StageKind.Ray, Label = end, OriginLabel = origin, X = x, Y = y, Angle = angle, Len = len, Zero = zero, Spread = spread };

    /// <summary>中心と半径</summary>
    public static StageHandle Circle(string label, string x, string y, string radius) => new() { Kind = StageKind.Circle, Label = label, X = x, Y = y, Size = radius };

    /// <summary>四角(位置・幅・高さ・回転)</summary>
    public static StageHandle Rect(string label, string x, string y, string w, string h, StageAnchor anchor = StageAnchor.Center, string? rot = null)
        => new() { Kind = StageKind.Rect, Label = label, X = x, Y = y, W = w, H = h, Anchor = anchor, Rot = rot };

    /// <summary>中心から左右に伸びる幅</summary>
    public static StageHandle Span(string label, string x, string y, string len) => new() { Kind = StageKind.Span, Label = label, X = x, Y = y, Len = len };

    /// <summary>2つの点(始点・終点)をつなぐ線</summary>
    public static StageHandle Seg(string a, string b, string x1, string y1, string x2, string y2, bool dashed = false)
        => new() { Kind = StageKind.Segment, OriginLabel = a, Label = b, X = x1, Y = y1, X2 = x2, Y2 = y2, Dashed = dashed };

    /// <summary>X1・Y1 〜 Xn・Yn の点をつなぐ道すじ(使う点の数は count の項目)</summary>
    public static StageHandle Path(string label, string count, int n)
        => new() { Kind = StageKind.Path, Label = label, Count = count, Xs = Enumerable.Range(1, n).Select(i => "X" + i).ToArray(), Ys = Enumerable.Range(1, n).Select(i => "Y" + i).ToArray() };

    /// <summary>横いっぱいの線(高さだけ決める)</summary>
    public static StageHandle HLine(string label, string y) => new() { Kind = StageKind.HLine, Label = label, Y = y };

    /// <summary>縦いっぱいの線(横の位置だけ決める)</summary>
    public static StageHandle VLine(string label, string x, double gripY = 0.92) => new() { Kind = StageKind.VLine, Label = label, X = x, GripY = gripY };

    /// <summary>画面全体にかかる向き(画面の中央に、矢印のダイヤルを出す)</summary>
    public static StageHandle Dial(string label, string angle, StageZero zero = StageZero.Right) => new() { Kind = StageKind.Dial, Label = label, Angle = angle, Zero = zero };
}

/// <summary>演出図形ごとの、配置画面のつまみの表(キーは、図形の設定のクラス名)</summary>
public static class ShapeStageLayouts
{
    static StageHandle Pt(string l, string x, string y) => StageHandle.Pt(l, x, y);
    static StageHandle Circle(string l, string x, string y, string r) => StageHandle.Circle(l, x, y, r);
    static StageHandle Rect(string l, string x, string y, string w, string h, StageAnchor a = StageAnchor.Center, string? rot = null) => StageHandle.Rect(l, x, y, w, h, a, rot);
    static StageHandle HLine(string l, string y) => StageHandle.HLine(l, y);
    static StageHandle VLine(string l, string x, double gy = 0.92) => StageHandle.VLine(l, x, gy);
    static StageHandle Dial(string l, string a, StageZero z = StageZero.Right) => StageHandle.Dial(l, a, z);
    static StageHandle Span(string l, string x, string y, string len) => StageHandle.Span(l, x, y, len);

    static readonly Dictionary<string, StageHandle[]> map = new()
    {
        // ---- 天気・自然 ----
        ["RainParameter"] = [Dial("雨の降る向き", "Angle", StageZero.Down)],
        ["RainRippleParameter"] = [HLine("水面の始まり", "FloorTop")],
        ["CloudParameter"] = [HLine("雲の高さ", "PosY")],
        ["RainbowParameter"] = [Circle("虹のアーチ", "PosX", "PosY", "Radius")],
        ["WindParameter"] = [Dial("風の向き", "Direction")],
        ["SkyParameter"] = [Pt("太陽", "SunX", "SunY"), HLine("雲の高さ", "CloudY")],
        ["SunParameter"] = [Circle("太陽", "PosX", "PosY", "Radius"),
                            StageHandle.Ray("太陽", "光の筋の先", "PosX", "PosY", "ShaftAngle", "ShaftLength", spread: "ShaftSpread")],
        ["NightSkyParameter"] = [Circle("月", "MoonX", "MoonY", "MoonSize"), VLine("地平線の光", "GlowX")],
        ["MeteorShowerParameter"] = [Pt("放射点", "RadiantX", "RadiantY"), Dial("進む向き(放射点を使わないとき)", "Angle")],
        ["SpaceParameter"] = [Pt("銀河", "GalaxyX", "GalaxyY"), Circle("惑星", "PlanetX", "PlanetY", "PlanetSize")],
        ["AuroraParameter"] = [HLine("カーテンの下端", "BaseY")],
        ["SkylineParameter"] = [HLine("街の底辺", "BaseY")],
        ["VolcanoParameter"] = [Rect("山", "PosX", "BaseY", "MountWidth", "MountHeight", StageAnchor.BottomCenter)],
        ["WaterSurfaceUpParameter"] = [Pt("光源", "SunX", "SunY")],
        ["WavesParameter"] = [HLine("水面", "WaterLine")],
        ["SplashParameter"] = [Pt("しぶきの位置", "PosX", "WaterY")],
        ["FireParameter"] = [Pt("炎の根元", "PosX", "PosY")],
        ["SmokeParameter"] = [Span("煙の発生源", "SourceX", "SourceY", "SourceWidth")],
        ["FlyersParameter"] = [HLine("飛ぶ高さ", "Height")],
        ["VortexParameter"] = [Pt("足元・中心", "PosX", "PosY")],
        ["MoonPhaseParameter"] = [Circle("月", "PosX", "PosY", "Radius")],

        // ---- バトル・魔法・ゲーム ----
        ["ExplosionParameter"] = [Circle("爆発", "PosX", "PosY", "Size")],
        ["LaserParameter"] = [StageHandle.Ray("発射位置", "レーザーの先", "OriginX", "OriginY", "Angle", "Length", spread: "Spread")],
        ["BeamParameter"] = [StageHandle.Ray("発射位置", "着弾地点", "OriginX", "OriginY", "Angle", "Length")],
        ["EnergyBallParameter"] = [Circle("弾", "PosX", "PosY", "Radius"),
                                   StageHandle.Ray("溜める場所", "着弾地点", "PosX", "PosY", "Angle", "Distance")],
        ["PlasmaParameter"] = [Circle("プラズマボール", "BallX", "BallY", "BallRadius"),
                               StageHandle.Seg("放電の始点", "放電の終点", "X1", "Y1", "X2", "Y2")],
        ["SpotlightParameter"] = [Circle("照らす場所", "PosX", "PosY", "Radius"),
                                  StageHandle.Seg("光源", "照らす場所", "BeamX", "BeamY", "PosX", "PosY", dashed: true)],
        ["MagicCircleParameter"] = [Circle("魔法陣", "PosX", "PosY", "Radius")],
        ["SlashParameter"] = [StageHandle.Ray("斬撃の中心", "斬る向き・長さ", "PosX", "PosY", "Angle", "Length")],
        ["AuraParameter"] = [Rect("オーラ", "PosX", "PosY", "BodyWidth", "BodyHeight", StageAnchor.BottomCenter)],
        ["SpellParameter"] = [StageHandle.Ray("発射位置", "着弾地点", "StartX", "StartY", "Angle", "Distance")],
        ["SummonParameter"] = [Circle("光の輪", "PosX", "PosY", "Radius")],
        ["CrackerParameter"] = [StageHandle.Ray("発射位置", "飛ばす向き", "OriginX", "OriginY", "Direction", spread: "Spread")],
        ["ShieldParameter"] = [Circle("シールド", "PosX", "PosY", "Radius")],
        ["PortalParameter"] = [Circle("ポータル", "PosX", "PosY", "Size")],
        ["LockOnParameter"] = [Circle("標的", "PosX", "PosY", "Size")],
        ["DamageHitParameter"] = [Pt("ヒット位置", "PosX", "PosY")],
        ["HealBuffParameter"] = [Circle("足元の広がり", "PosX", "PosY", "Radius")],
        ["LightSwordParameter"] = [StageHandle.Ray("柄", "刃の先", "PosX", "PosY", "Angle", "Length", StageZero.Up)],
        ["MuzzleFlashParameter"] = [StageHandle.Ray("銃口", "撃つ向き", "PosX", "PosY", "Direction"),
                                    StageHandle.Seg("銃口", "着弾地点", "PosX", "PosY", "TargetX", "TargetY", dashed: true)],
        ["LevelUpParameter"] = [Pt("足元", "PosX", "PosY")],
        ["TreasureChestParameter"] = [Span("宝箱(底)", "PosX", "PosY", "BoxWidth")],
        ["GachaParameter"] = [Circle("ガチャの玉", "PosX", "PosY", "OrbSize")],
        ["DizzyParameter"] = [Circle("頭の上を回る輪", "PosX", "PosY", "Radius")],
        ["PixelArtParameter"] = [Pt("ドット絵", "PosX", "PosY")],
        ["GameUiParameter"] = [Pt("ライフ", "LifeX", "LifeY"), Pt("スコア", "ScoreX", "ScoreY"), Pt("コンボ", "ComboX", "ComboY")],

        // ---- SF・HUD ----
        ["LensFlareParameter"] = [Pt("光源", "SourceX", "SourceY")],
        ["FuturePanelParameter"] = [Rect("パネル", "PosX", "PosY", "PanelWidth", "PanelHeight")],
        ["FutureShapeParameter"] = [Circle("HUDの図形", "PosX", "PosY", "Size")],
        ["NeonShapeParameter"] = [Circle("ネオン", "PosX", "PosY", "Size")],
        ["HologramParameter"] = [Pt("ホログラム", "PosX", "PosY")],
        ["WarpParameter"] = [Pt("ワープの中心", "CenterX", "CenterY")],
        ["RadarParameter"] = [Circle("レーダー", "PosX", "PosY", "Radius")],
        ["WaveformParameter"] = [Rect("波形", "PosX", "PosY", "WaveWidth", "WaveHeight")],
        ["ScanBeamParameter"] = [Rect("走査する範囲", "PosX", "PosY", "ScanWidth", "ScanHeight")],
        ["BlackHoleParameter"] = [Circle("黒い穴", "PosX", "PosY", "Radius"), Circle("円盤", "PosX", "PosY", "DiskSize")],
        ["CircleVisualizerParameter"] = [Circle("ビジュアライザー", "PosX", "PosY", "Radius")],
        ["CyberGlobeParameter"] = [Circle("地球儀", "PosX", "PosY", "Radius")],
        ["DnaMoleculeParameter"] = [Pt("DNA・分子", "PosX", "PosY")],
        ["DataStreamParameter"] = [Dial("流れる向き", "Direction")],
        ["GeoBgParameter"] = [Dial("流れる向き", "Direction")],
        ["GradientBgParameter"] = [Dial("グラデーションの向き", "Angle")],

        // ---- 動画制作・UI ----
        ["MangaFxParameter"] = [Pt("漫画効果の中心", "PosX", "PosY")],
        ["TelopDecoParameter"] = [Rect("テロップの枠", "PosX", "PosY", "BoxWidth", "BoxHeight")],
        ["MarkerParameter"] = [Rect("マーカー", "PosX", "PosY", "MarkWidth", "MarkHeight", rot: "Rotation")],
        ["GaugeParameter"] = [Rect("ゲージ", "PosX", "PosY", "GaugeWidth", "GaugeHeight")],
        ["ChartParameter"] = [Rect("グラフ", "PosX", "PosY", "ChartWidth", "ChartHeight")],
        ["CountdownParameter"] = [Pt("数字", "PosX", "PosY")],
        ["UiOverlayParameter"] = [Pt("UI", "PosX", "PosY")],
        ["ViewfinderParameter"] = [Pt("ピント枠", "FocusX", "FocusY")],
        ["WindowGlassParameter"] = [Pt("ひびの中心", "CrackX", "CrackY")],
        ["StepProgressParameter"] = [Span("ステップ全体", "PosX", "PosY", "TotalWidth")],
        ["StickyNoteParameter"] = [Rect("付箋", "PosX", "PosY", "NoteWidth", "NoteHeight", rot: "Rotation")],
        ["DividerParameter"] = [Span("飾り罫", "PosX", "PosY", "LineLength")],
        ["ScreenFrameParameter"] = [Pt("窓", "PosX", "PosY")],
        ["WarningParameter"] = [HLine("警告の帯", "CenterY")],
        ["RankingBadgeParameter"] = [Circle("メダル", "PosX", "PosY", "MedalSize")],
        ["ShineParameter"] = [Rect("光らせる範囲", "PosX", "PosY", "AreaWidth", "AreaHeight")],
        ["NamePlateParameter"] = [Rect("名前の帯", "PosX", "PosY", "PlateWidth", "PlateHeight", StageAnchor.LeftCenter)],
        ["TimelineParameter"] = [HLine("年表の線", "PosY")],
        ["MapRouteParameter"] = [StageHandle.Path("ルート", "Count", 5)],
        ["ChecklistParameter"] = [Rect("1行目", "PosX", "PosY", "RowWidth", "RowHeight", StageAnchor.TopLeft)],
        ["ClockHourglassParameter"] = [Circle("時計・砂時計", "PosX", "PosY", "Radius")],

        // ---- 季節・イベント ----
        ["TanabataParameter"] = [VLine("笹の根元", "PosX")],
        ["SummerParameter"] = [Pt("うちわ", "FanX", "FanY")],
        ["ChristmasParameter"] = [VLine("雪だるま", "PosX", 0.9)],
        ["MirrorBallParameter"] = [Circle("ミラーボール", "PosX", "PosY", "BallSize")],
        ["IlluminationParameter"] = [VLine("ツリー", "TreeX")],

        // ---- 128番以降(解説・ゲーム・SNS・漫符・質感) ----
        ["SpeechBubbleParameter"] = [StageHandle.Seg("吹き出し", "しっぽの先", "PosX", "PosY", "TailX", "TailY", dashed: true)],
        ["ArrowParameter"] = [StageHandle.Seg("始点", "終点（矢じり）", "StartX", "StartY", "EndX", "EndY")],
        ["EncircleParameter"] = [Rect("囲む範囲", "PosX", "PosY", "BoxWidth", "BoxHeight", rot: "Tilt")],
        ["AnnotationParameter"] = [StageHandle.Seg("指す点", "注釈", "PointX", "PointY", "LabelX", "LabelY")],
        ["CountUpParameter"] = [Pt("数字", "PosX", "PosY")],
        ["PieChartParameter"] = [Circle("円グラフ", "PosX", "PosY", "Radius")],
        ["RelationshipParameter"] = [Circle("相関図の広がり", "PosX", "PosY", "Spread")],
        ["ChoiceWindowParameter"] = [Pt("選択肢の窓", "PosX", "PosY")],
        ["NovelWindowParameter"] = [HLine("窓の下端", "BottomY")],
        ["BossIntroParameter"] = [HLine("帯", "PosY")],
        ["InputDisplayParameter"] = [Pt("入力表示", "PosX", "PosY")],
        ["HeartsRiseParameter"] = [Pt("出てくる所", "SourceX", "SourceY")],
        ["SubscribeButtonParameter"] = [Pt("ボタン", "PosX", "PosY")],
        ["CommentFlowParameter"] = [HLine("流れる範囲の上端", "AreaTop"), HLine("流れる範囲の下端", "AreaBottom")],
        ["PhoneNotifyParameter"] = [Pt("通知・スマホ", "PosX", "PosY")],
        ["MangaMarkParameter"] = [Pt("漫符", "PosX", "PosY")],
        ["GloomParameter"] = [Pt("頭のてっぺん", "PosX", "PosY")],
        ["GrassParameter"] = [HLine("地面", "BaseY")],
        ["KomorebiParameter"] = [VLine("光源", "SourceX", 0.06), Dial("差し込む向き", "Angle", StageZero.Down)],
        ["DustParameter"] = [VLine("光の帯", "BeamX", 0.06), Dial("光の向き", "BeamAngle", StageZero.Down)],

        // ---- 153番以降 ----
        ["RankingBoardParameter"] = [Rect("ランキング表", "PosX", "PosY", "BoardWidth", "RowHeight")],
        ["CompareTableParameter"] = [Pt("比較表", "PosX", "PosY")],
        ["BarRaceParameter"] = [Rect("グラフ", "PosX", "PosY", "ChartWidth", "ChartHeight")],
        ["FlowchartParameter"] = [Pt("フローチャート", "PosX", "PosY")],
        ["HandwritingParameter"] = [Pt("手書き文字", "PosX", "PosY")],
        ["StatusScreenParameter"] = [Pt("ステータス画面", "PosX", "PosY")],
        ["ItemGetParameter"] = [Pt("アイテム", "PosX", "PosY")],
        ["ImpactTextParameter"] = [Pt("文字", "PosX", "PosY")],
        ["QuizParameter"] = [HLine("クイズの中心", "PosY")],
        ["RouletteParameter"] = [Circle("ルーレット", "PosX", "PosY", "Radius")],
        ["DonationParameter"] = [Pt("カード", "PosX", "PosY")],
        ["SnsPostParameter"] = [Pt("投稿", "PosX", "PosY")],
        ["SearchBarParameter"] = [Pt("検索バー", "PosX", "PosY")],
        ["ReactionParameter"] = [Circle("頭", "PosX", "PosY", "Size")],
        ["TearsParameter"] = [Pt("左目", "LeftX", "LeftY"), Pt("右目", "RightX", "RightY")],
        ["BlushParameter"] = [Pt("左のほお", "LeftX", "LeftY"), Pt("右のほお", "RightX", "RightY")],
        ["MenacingParameter"] = [Rect("広がり", "PosX", "PosY", "AreaWidth", "AreaHeight")],
        ["NightViewParameter"] = [HLine("地平線", "Horizon"), VLine("電波塔", "TowerX", 0.5)],
        ["PuddlesParameter"] = [HLine("地面の始まり", "GroundY")],
        ["SpaceshipWindowParameter"] = [Circle("窓", "PosX", "PosY", "Radius")],
        ["TreeTunnelParameter"] = [HLine("消える点の高さ", "VanishY")],
        ["ImpactFlashParameter"] = [Pt("衝撃の中心", "CenterX", "CenterY")],
        ["KaleidoscopeParameter"] = [Pt("万華鏡の中心", "CenterX", "CenterY")],
    };

    public static IReadOnlyList<StageHandle>? For(string? typeName)
    {
        if (typeName == null) return null;
        if (map.TryGetValue(typeName, out var list)) return list;
        return QuickStageLayouts.Map.TryGetValue(typeName, out var quick) ? quick : null;
    }

    /// <summary>
    /// 表に無い図形(ほかのプラグインの図形など)用: 単位が % の「〜X」「〜Y」の組を、点として出す
    /// </summary>
    public static List<StageHandle> Guess(IEnumerable<ItemPropRow> rows)
    {
        var list = new List<StageHandle>();
        var pct = rows.Where(r => r.IsNumber && r.Unit == "%").ToList();
        foreach (var rx in pct)
        {
            if (!rx.PropName.EndsWith("X", StringComparison.Ordinal)) continue;
            var yName = rx.PropName[..^1] + "Y";
            var ry = pct.FirstOrDefault(r => r.PropName == yName);
            if (ry == null) continue;
            var label = rx.Label.Replace("X", "").Replace("の位置", "").Replace("位置", "").Trim(' ', '　', '（', '）', '(', ')');
            list.Add(StageHandle.Pt(label.Length == 0 ? "位置" : label, rx.PropName, yName));
        }
        return list;
    }
}

/// <summary>「編集する点」のボタン1つ分(始点・中間点・終点)</summary>
public sealed class StageChip : NotifyBase
{
    bool isSelected;
    public string Label { get; }
    public int Index { get; }
    public ICommand Command { get; }
    public bool IsSelected { get => isSelected; set => Set(ref isSelected, value); }
    public StageChip(string label, int index, Action<int> pick) { Label = label; Index = index; Command = new RelayCommand(_ => pick(index)); }
}

/// <summary>配置画面の下に出す、つまみの色の見本</summary>
public sealed class StageLegend
{
    public string Label { get; }
    public Brush Brush { get; }
    public StageLegend(string label, Brush brush) { Label = label; Brush = brush; }
}

/// <summary>板に描くもの(線・円・多角形)。座標は図形の描画範囲のピクセル</summary>
internal sealed class StageDraw
{
    public enum K { Line, Circle, Poly, Dot }
    public K Kind;
    public Point A, B;
    public double R;
    public List<Point>? Pts;
    public int Color;
    public bool Dashed, Arrow, Fill, Faint;
    public string? Text;
}

/// <summary>ドラッグできるつまみ1つ</summary>
internal sealed class StageGrip
{
    public enum S { Dot, Ring, Square, Diamond }
    public Point P;
    public string Label = "";
    public int Color;
    public S Shape;
    public Action<Point, bool>? Drag;
    public Func<string>? Readout;
}

/// <summary>アイテムタブの「配置」カード(選択中の図形1つ分)</summary>
public sealed class ShapeStageVm : NotifyBase
{
    internal static readonly Color[] Palette =
    {
        Color.FromRgb(0x60, 0xCD, 0xFF), Color.FromRgb(0xFF, 0x9F, 0x43), Color.FromRgb(0x7C, 0xE3, 0x8B),
        Color.FromRgb(0xFF, 0x6B, 0x9A), Color.FromRgb(0xC4, 0x9B, 0xFF), Color.FromRgb(0xFF, 0xD5, 0x4A),
    };

    readonly Dictionary<string, ItemPropRow> rows = new();
    /// <summary>アイテム本体の X・Y・拡大率・回転角(図形が画面のどこに出るかを計算するのに使う)</summary>
    readonly Dictionary<string, ItemPropRow> itemRows = new();
    readonly List<StageHandle> handles;
    readonly ItemEditorVm host;
    readonly DispatcherTimer captureTimer = new() { Interval = TimeSpan.FromMilliseconds(350) };
    int version, editIndex, pointCount = 1;
    double canvasW = 1920, canvasH = 1080, screenW = 1920, screenH = 1080;
    int fitMode;            // 0=画面サイズに合わせる 1=拡大縮小 2=指定サイズ
    double fitS = 1, fitOx, fitOy;
    bool snap, autoPreview = true, viewVisible, dragging, disposed;
    long lastCaptureMs = 0, captureCost;
    string lastValues = "";
    BitmapSource? backdrop;

    public string TypeName { get; }
    public string Title { get; }
    public string HelpText { get; }
    public int Version => version;
    public ObservableCollection<StageChip> PointChips { get; } = new();
    public bool HasPointChips => PointChips.Count > 1;
    public List<StageLegend> Legend { get; } = new();
    public ICommand CaptureCommand { get; }
    public ICommand ClearBackdropCommand { get; }
    public string SizeText => $"画面 {screenW:0}×{screenH:0} px" + (fitMode == 0 ? "" : $" / 図形の描く範囲 {canvasW:0}×{canvasH:0} px");

    public bool Snap { get => snap; set => Set(ref snap, value); }
    public BitmapSource? Backdrop
    {
        get => backdrop;
        set { if (Set(ref backdrop, value)) { Raise(nameof(HasBackdrop)); Bump(); } }
    }
    public bool HasBackdrop => backdrop != null;
    public int EditIndex => editIndex;
    internal double CanvasW => canvasW;
    internal double CanvasH => canvasH;
    internal double ScreenW => screenW;
    internal double ScreenH => screenH;

    /// <summary>横に並べる、この図形の項目(つまみで動かす項目と、同じグループの項目)</summary>
    public List<ItemPropRow> Rows { get; } = new();
    public bool HasRows => Rows.Count > 0;

    /// <summary>YMM4のプレビューを、値が変わるたびに自動で背景へ写す</summary>
    public bool AutoPreview
    {
        get => autoPreview;
        set { if (Set(ref autoPreview, value) && value) RequestCapture(0); }
    }

    /// <summary>板が画面に見えているか(見えていない時は、プレビューを写さない)</summary>
    internal bool ViewVisible
    {
        get => viewVisible;
        set { viewVisible = value; if (value) RequestCapture(0); }
    }

    /// <summary>つまみをドラッグ中か(ドラッグ中は写さず、離した後に写す)</summary>
    internal bool Dragging
    {
        get => dragging;
        set { dragging = value; if (!value) RequestCapture(250); }
    }

    /// <summary>少し待ってから、プレビューを写す(続けて呼ばれたら、最後から数える)</summary>
    void RequestCapture(int delayMs)
    {
        if (disposed || !autoPreview) return;
        captureTimer.Stop();
        captureTimer.Interval = TimeSpan.FromMilliseconds(Math.Max(1, delayMs));
        captureTimer.Start();
    }

    void AutoCapture()
    {
        captureTimer.Stop();
        if (disposed || !autoPreview || !viewVisible) return;
        if (dragging) { RequestCapture(250); return; }
        // 写すのが重い環境では、間をあける(YMM4の操作が重くならないように)
        long now = Environment.TickCount64;
        if (captureCost > 120 && now - lastCaptureMs < captureCost * 6) { RequestCapture((int)(captureCost * 6 - (now - lastCaptureMs))); return; }
        var sw = System.Diagnostics.Stopwatch.StartNew();
        BitmapSource? bmp = null;
        try { bmp = Ymm.CapturePreview(); } catch { }
        captureCost = sw.ElapsedMilliseconds;
        lastCaptureMs = Environment.TickCount64;
        if (bmp != null) { backdrop = bmp; Raise(nameof(Backdrop)); Raise(nameof(HasBackdrop)); version++; Raise(nameof(Version)); }
    }

    /// <summary>この配置画面を使い終わった(別のアイテムを選んだ時など)</summary>
    public void Dispose()
    {
        disposed = true;
        captureTimer.Stop();
    }

    ShapeStageVm(ItemEditorVm host, string typeName, string title, IReadOnlyList<ItemPropRow> shapeRows, IEnumerable<ItemPropRow> itemLevelRows, List<StageHandle> handles, bool guessed)
    {
        this.host = host;
        TypeName = typeName;
        Title = title;
        this.handles = handles;
        foreach (var r in shapeRows) rows.TryAdd(r.PropName, r);
        foreach (var r in itemLevelRows)
            if (r.IsNumber && (r.PropName is "X" or "Y" or "Zoom" or "ZoomX" or "ZoomY" or "Rotation")) itemRows.TryAdd(r.PropName, r);
        captureTimer.Tick += (_, _) => AutoCapture();

        // 横に並べる項目: つまみで動かす項目が入っているグループの項目すべて(サイズ・共通のグループは除く)
        var used = new HashSet<string>();
        foreach (var h in handles)
            foreach (var n in new[] { h.X, h.Y, h.X2, h.Y2, h.Angle, h.Len, h.Size, h.W, h.H, h.Rot, h.Spread, h.Count }.Concat(h.Xs ?? Array.Empty<string>()).Concat(h.Ys ?? Array.Empty<string>()))
                if (n != null) used.Add(n);
        var groups = new HashSet<string>(shapeRows.Where(r => used.Contains(r.PropName)).Select(r => r.Group));
        groups.Remove("サイズ"); groups.Remove("共通");
        foreach (var r in shapeRows)
            if (used.Contains(r.PropName) || groups.Contains(r.Group)) Rows.Add(r);

        CaptureCommand = new RelayCommand(_ =>
        {
            var bmp = Ymm.CapturePreview();
            if (bmp == null) { host.Report(Ymm.LastError); return; }
            Backdrop = bmp;
        });
        ClearBackdropCommand = new RelayCommand(_ => Backdrop = null);

        var labels = new List<string>();
        foreach (var h in handles)
        {
            if (h.Kind == StageKind.Ray || h.Kind == StageKind.Segment) { if (!labels.Contains(h.OriginLabel)) labels.Add(h.OriginLabel); }
            if (!labels.Contains(h.Label)) labels.Add(h.Label);
        }
        for (int i = 0; i < labels.Count; i++)
        {
            var b = new SolidColorBrush(Palette[ColorOf(labels[i]) % Palette.Length]);
            b.Freeze();
            Legend.Add(new StageLegend(labels[i], b));
        }
        var parts = new List<string> { "プレビューの上のつまみを、そのままドラッグして動かせます" };
        if (handles.Any(h => h.Kind == StageKind.Ray && h.Len != null)) parts.Add("先のつまみで向きと長さが決まります");
        parts.Add("Shift で、きりのいい値に吸着");
        parts.Add("空いている所をダブルクリックで、最初のつまみをそこへ");
        if (guessed) parts.Add("(この図形には専用の配置がないので、名前から位置の項目を探して出しています)");
        HelpText = string.Join(" / ", parts);
        Bump();
    }

    /// <summary>配置画面を作る(つまみを出せる項目が無い図形なら null)</summary>
    public static ShapeStageVm? TryCreate(ItemEditorVm host, string? typeName, string title, IReadOnlyList<ItemPropRow> shapeRows, IEnumerable<ItemPropRow> itemLevelRows, ShapeStageVm? previous)
    {
        if (typeName == null || shapeRows.Count == 0) return null;
        var names = new HashSet<string>(shapeRows.Select(r => r.PropName));
        var layout = ShapeStageLayouts.For(typeName);
        bool guessed = layout == null;
        var list = (layout ?? ShapeStageLayouts.Guess(shapeRows)).Where(h => Usable(h, names)).ToList();
        if (list.Count == 0) return null;
        var vm = new ShapeStageVm(host, typeName, title, shapeRows, itemLevelRows, list, guessed);
        if (previous != null && previous.TypeName == typeName)
        {
            vm.backdrop = previous.backdrop;
            vm.snap = previous.snap;
            vm.autoPreview = previous.autoPreview;
            vm.viewVisible = previous.viewVisible;
            vm.PickPoint(Math.Min(previous.editIndex, vm.pointCount - 1));
        }
        return vm;
    }

    /// <summary>つまみに要る項目がそろっているか(足りない時は、出せる形に減らすのではなく出さない)</summary>
    static bool Usable(StageHandle h, HashSet<string> n)
    {
        bool Has(string? s) => s == null || n.Contains(s);
        return h.Kind switch
        {
            StageKind.HLine => h.Y != null && n.Contains(h.Y),
            StageKind.VLine => h.X != null && n.Contains(h.X),
            StageKind.Dial => h.Angle != null && n.Contains(h.Angle),
            StageKind.Path => h.Xs != null && h.Ys != null && n.Contains(h.Xs[0]) && n.Contains(h.Ys[0]),
            StageKind.Segment => Has(h.X) && Has(h.Y) && Has(h.X2) && Has(h.Y2) && h.X != null && h.X2 != null,
            StageKind.Ray => h.X != null && h.Y != null && n.Contains(h.X) && n.Contains(h.Y) && h.Angle != null && n.Contains(h.Angle) && Has(h.Len) && Has(h.Spread),
            StageKind.Circle => h.X != null && h.Y != null && n.Contains(h.X) && n.Contains(h.Y) && h.Size != null && n.Contains(h.Size),
            StageKind.Rect => h.X != null && h.Y != null && n.Contains(h.X) && n.Contains(h.Y) && h.W != null && n.Contains(h.W) && h.H != null && n.Contains(h.H) && Has(h.Rot),
            StageKind.Span => h.X != null && h.Y != null && n.Contains(h.X) && n.Contains(h.Y) && h.Len != null && n.Contains(h.Len),
            _ => h.X != null && h.Y != null && n.Contains(h.X) && n.Contains(h.Y),
        };
    }

    /// <summary>同じ名前のつまみは、同じ色にする</summary>
    int ColorOf(string label)
    {
        var labels = new List<string>();
        foreach (var h in handles)
        {
            if (h.Kind == StageKind.Ray || h.Kind == StageKind.Segment) { if (!labels.Contains(h.OriginLabel)) labels.Add(h.OriginLabel); }
            if (!labels.Contains(h.Label)) labels.Add(h.Label);
        }
        int i = labels.IndexOf(label);
        return i < 0 ? 0 : i;
    }

    /// <summary>値が変わった時(YMM4側・ほかの行で変えた時も)。描き直しと、編集する点のボタンの更新</summary>
    public void Bump()
    {
        UpdateCanvasSize();
        int count = 1;
        foreach (var r in rows.Values) if (r.IsAnim && r.IsAnimated) count = Math.Max(count, r.PointCount);
        if (count != pointCount || PointChips.Count == 0)
        {
            pointCount = count;
            PointChips.Clear();
            if (count > 1)
                for (int i = 0; i < count; i++)
                    PointChips.Add(new StageChip(i == 0 ? "始点" : i == count - 1 ? "終点" : $"中間点{i}", i, PickPoint));
            if (editIndex >= count) editIndex = count - 1;
            foreach (var c in PointChips) c.IsSelected = c.Index == editIndex;
            Raise(nameof(HasPointChips));
        }
        // 値(または再生位置)が変わったら、少し待ってからプレビューを写し直す
        var sb = new System.Text.StringBuilder();
        foreach (var r in rows.Values.Concat(itemRows.Values)) if (r.IsNumber) sb.Append(r.PointNumber(0).ToString("R", CultureInfo.InvariantCulture)).Append(',');
        try { sb.Append(Ymm.CurrentFrame()); } catch { }
        var values = sb.ToString();
        if (values != lastValues) { lastValues = values; RequestCapture(dragging ? 400 : 300); }

        version++;
        Raise(nameof(Version));
        Raise(nameof(SizeText));
    }

    void PickPoint(int index)
    {
        editIndex = Math.Clamp(index, 0, Math.Max(0, pointCount - 1));
        foreach (var c in PointChips) c.IsSelected = c.Index == editIndex;
        version++;
        Raise(nameof(Version));
        Raise(nameof(EditIndex));
    }

    void UpdateCanvasSize()
    {
        // 画面(プロジェクト)の大きさ
        double sw = 0, sh = 0;
        try
        {
            var vi = Ymm.Get(Ymm.Timeline(), "VideoInfo");
            sw = Convert.ToDouble(Ymm.Get(vi, "Width") ?? 0, CultureInfo.InvariantCulture);
            sh = Convert.ToDouble(Ymm.Get(vi, "Height") ?? 0, CultureInfo.InvariantCulture);
        }
        catch { sw = sh = 0; }
        double rw = rows.TryGetValue("Width", out var rwr) ? rwr.PointNumber(0) : 1920;
        double rh = rows.TryGetValue("Height", out var rhr) ? rhr.PointNumber(0) : 1080;
        if (rw <= 0) rw = 1920;
        if (rh <= 0) rh = 1080;
        bool hasScreen = sw > 0 && sh > 0;
        fitMode = rows.TryGetValue("Fit", out var fit) ? Math.Max(0, fit.ChoiceIndex) : 0;
        if (!hasScreen) fitMode = 2;
        screenW = hasScreen ? sw : rw;
        screenH = hasScreen ? sh : rh;
        // 描く範囲: 「画面サイズに合わせる」なら画面の大きさ、それ以外は図形の設定の 幅・高さ(演出図形の描き方と同じ)
        canvasW = fitMode == 0 ? screenW : rw;
        canvasH = fitMode == 0 ? screenH : rh;
        fitS = 1; fitOx = fitOy = 0;
        if (fitMode == 1)
        {
            fitS = Math.Max(screenW / rw, screenH / rh);
            fitOx = (screenW - rw * fitS) / 2;
            fitOy = (screenH - rh * fitS) / 2;
        }
    }

    // ------------------------------------------------------------------ 図形の描く範囲 ⇔ 画面(プレビュー)
    // 演出図形は、描いた絵のまん中を、アイテムの位置(X・Y、画面のまん中が 0)に置く。
    // そこに、アイテムの拡大率と回転角がかかる
    double Item(string name, double fallback) => itemRows.TryGetValue(name, out var r) ? r.PointNumber(Idx(r, editIndex)) : fallback;
    double ZoomX => Item("Zoom", 100) / 100 * Item("ZoomX", 100) / 100;
    double ZoomY => Item("Zoom", 100) / 100 * Item("ZoomY", 100) / 100;

    /// <summary>図形の中のピクセル → 画面のピクセル(左上が 0)</summary>
    internal Point ToScreen(Point p)
    {
        Point q = fitMode == 1 ? new Point(p.X * fitS + fitOx - screenW / 2, p.Y * fitS + fitOy - screenH / 2)
                               : new Point(p.X - canvasW / 2, p.Y - canvasH / 2);
        q = Rotate(new Point(q.X * ZoomX, q.Y * ZoomY), Item("Rotation", 0));
        return new Point(screenW / 2 + Item("X", 0) + q.X, screenH / 2 + Item("Y", 0) + q.Y);
    }

    /// <summary>画面のピクセル → 図形の中のピクセル</summary>
    internal Point FromScreen(Point v)
    {
        var q = Rotate(new Point(v.X - screenW / 2 - Item("X", 0), v.Y - screenH / 2 - Item("Y", 0)), -Item("Rotation", 0));
        double zx = ZoomX, zy = ZoomY;
        q = new Point(Math.Abs(zx) < 1e-6 ? 0 : q.X / zx, Math.Abs(zy) < 1e-6 ? 0 : q.Y / zy);
        return fitMode == 1 ? new Point((q.X + screenW / 2 - fitOx) / fitS, (q.Y + screenH / 2 - fitOy) / fitS)
                            : new Point(q.X + canvasW / 2, q.Y + canvasH / 2);
    }

    /// <summary>図形の中の長さ1ピクセルが、画面で何ピクセルになるか(円の半径用)</summary>
    internal double LenScale => (fitMode == 1 ? fitS : 1) * Math.Sqrt(Math.Abs(ZoomX * ZoomY));

    // ------------------------------------------------------------------ 値の読み書き
    ItemPropRow? R(string? name) => name != null && rows.TryGetValue(name, out var r) ? r : null;

    static int Idx(ItemPropRow r, int k) => Math.Clamp(k, 0, r.PointCount - 1);
    double V(string? name, int k, double fallback = 0) { var r = R(name); return r == null ? fallback : r.PointNumber(Idx(r, k)); }
    double V(string? name, double fallback = 0) => V(name, editIndex, fallback);

    void SetV(string? name, double v)
    {
        var r = R(name);
        if (r == null) return;
        r.SetPointFromGraph(Idx(r, editIndex), v);
    }

    Point Pos(string? x, string? y, int k) => new(V(x, k, 50) / 100 * canvasW, V(y, k, 50) / 100 * canvasH);
    Point Pos(string? x, string? y) => Pos(x, y, editIndex);

    bool Snapping(bool shift) => shift ^ snap;
    static double Round(double v, double step) => Math.Round(v / step) * step;

    void SetPos(string? x, string? y, Point p, bool shift)
    {
        double px = p.X / canvasW * 100, py = p.Y / canvasH * 100;
        if (Snapping(shift)) { px = Round(px, 5); py = Round(py, 5); }
        if (x != null) SetV(x, px);
        if (y != null) SetV(y, py);
    }

    void SetLen(string? name, double v, bool shift)
    {
        if (name == null) return;
        v = Math.Max(0, v);
        if (Snapping(shift)) v = Round(v, 10);
        SetV(name, v);
    }

    /// <summary>画面の上での向き(右=0、下=90)と、項目の角度の変換</summary>
    static double ToScreen(double a, StageZero z) => z switch { StageZero.Up => a - 90, StageZero.Down => 90 - a, _ => a };
    static double FromScreen(double s, StageZero z) => z switch { StageZero.Up => s + 90, StageZero.Down => 90 - s, _ => s };

    void SetAngle(string? name, StageZero zero, Point from, Point to, bool shift)
    {
        if (name == null) return;
        double s = Math.Atan2(to.Y - from.Y, to.X - from.X) * 180 / Math.PI;
        double a = FromScreen(s, zero);
        if (Snapping(shift)) a = Round(a, 15);
        // いまの値に近い回り方にする(359° → 1° のように飛ばない。移動で回している時も、急に1回転しない)
        double old = V(name);
        a += 360 * Math.Round((old - a) / 360);
        SetV(name, a);
    }

    static Point Dir(double screenDeg, double len)
    {
        double r = screenDeg * Math.PI / 180;
        return new Point(Math.Cos(r) * len, Math.Sin(r) * len);
    }
    static Point Add(Point a, Point d) => new(a.X + d.X, a.Y + d.Y);
    static double Dist(Point a, Point b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
    static Point Rotate(Point p, double deg) { double r = deg * Math.PI / 180, c = Math.Cos(r), s = Math.Sin(r); return new Point(p.X * c - p.Y * s, p.X * s + p.Y * c); }

    string Pct(string? x, string? y) => $"X {V(x):0.#}%  Y {V(y):0.#}%";
    string Fmt(string? name, string unit) => R(name) is { } r ? $"{r.FormatValue(V(name))}{unit}" : "";

    // ------------------------------------------------------------------ 描くもの・つまみを作る
    internal void Compose(List<StageDraw> draws, List<StageGrip> grips)
    {
        var centers = new HashSet<string>();
        var trails = new HashSet<string>();
        double unit = Math.Min(canvasW, canvasH);

        void Trail(string? x, string? y, int color)
        {
            // 移動している位置は、始点〜終点の道すじも薄く出す
            if (x == null || y == null || !trails.Add(x + "|" + y)) return;
            var rx = R(x); var ry = R(y);
            int n = Math.Max(rx?.PointCount ?? 1, ry?.PointCount ?? 1);
            if (n < 2 || !((rx?.IsAnimated ?? false) || (ry?.IsAnimated ?? false))) return;
            var pts = Enumerable.Range(0, n).Select(k => Pos(x, y, k)).ToList();
            draws.Add(new StageDraw { Kind = StageDraw.K.Poly, Pts = pts, Color = color, Dashed = true, Faint = true });
            for (int k = 0; k < n; k++)
                if (k != editIndex) draws.Add(new StageDraw { Kind = StageDraw.K.Dot, A = pts[k], R = 3.5, Color = color, Faint = true, Text = (k + 1).ToString(CultureInfo.InvariantCulture) });
        }

        void Center(string label, string? x, string? y, int color, StageGrip.S shape = StageGrip.S.Dot)
        {
            if (x == null || y == null || !centers.Add(x + "|" + y)) return;
            Trail(x, y, color);
            grips.Add(new StageGrip
            {
                P = Pos(x, y), Label = label, Color = color, Shape = shape,
                Drag = (p, sh) => SetPos(x, y, p, sh),
                Readout = () => Pct(x, y),
            });
        }

        foreach (var h in handles)
        {
            int col = ColorOf(h.Label) % Palette.Length;
            switch (h.Kind)
            {
                case StageKind.Point:
                    Center(h.Label, h.X, h.Y, col);
                    break;

                case StageKind.Ray:
                {
                    int ocol = ColorOf(h.OriginLabel) % Palette.Length;
                    var o = Pos(h.X, h.Y);
                    double s = ToScreen(V(h.Angle), h.Zero);
                    double len = h.Len != null ? V(h.Len) : unit * 0.22;
                    var end = Add(o, Dir(s, len));
                    if (h.Spread != null)
                    {
                        double sp = V(h.Spread);
                        var fan = new List<Point> { o };
                        for (int i = 0; i <= 16; i++) fan.Add(Add(o, Dir(s - sp / 2 + sp * i / 16, len)));
                        draws.Add(new StageDraw { Kind = StageDraw.K.Poly, Pts = fan, Color = col, Fill = true, Faint = true });
                    }
                    draws.Add(new StageDraw { Kind = StageDraw.K.Line, A = o, B = end, Color = col, Arrow = true, Dashed = h.Len == null });
                    Center(h.OriginLabel, h.X, h.Y, ocol);
                    string angle = h.Angle!, x = h.X!, y = h.Y!;
                    var lenName = h.Len;
                    var zero = h.Zero;
                    grips.Add(new StageGrip
                    {
                        P = end, Label = h.Label, Color = col, Shape = lenName != null ? StageGrip.S.Diamond : StageGrip.S.Ring,
                        Drag = (p, sh) =>
                        {
                            var from = Pos(x, y);
                            SetAngle(angle, zero, from, p, sh);
                            if (lenName != null) SetLen(lenName, Dist(from, p), sh);
                        },
                        Readout = () => lenName != null
                            ? $"向き {Fmt(angle, "°")}  長さ {Fmt(lenName, "px")}  (先: X {Add(Pos(x, y), Dir(ToScreen(V(angle), zero), V(lenName))).X / canvasW * 100:0}%  Y {Add(Pos(x, y), Dir(ToScreen(V(angle), zero), V(lenName))).Y / canvasH * 100:0}%)"
                            : $"向き {Fmt(angle, "°")}",
                    });
                    break;
                }

                case StageKind.Circle:
                {
                    var c = Pos(h.X, h.Y);
                    double r = Math.Max(0, V(h.Size));
                    draws.Add(new StageDraw { Kind = StageDraw.K.Circle, A = c, R = r, Color = col });
                    Center(h.Label, h.X, h.Y, col);
                    string x = h.X!, y = h.Y!, size = h.Size!;
                    grips.Add(new StageGrip
                    {
                        P = Add(c, new Point(r * 0.7071, -r * 0.7071)), Label = "大きさ", Color = col, Shape = StageGrip.S.Ring,
                        Drag = (p, sh) => SetLen(size, Dist(Pos(x, y), p), sh),
                        Readout = () => $"半径・大きさ {Fmt(size, "px")}",
                    });
                    break;
                }

                case StageKind.Rect:
                {
                    var a = Pos(h.X, h.Y);
                    double w = Math.Max(0, V(h.W)), hh = Math.Max(0, V(h.H)), rot = h.Rot != null ? V(h.Rot) : 0;
                    double x0, y0;
                    switch (h.Anchor)
                    {
                        case StageAnchor.LeftCenter: x0 = 0; y0 = -hh / 2; break;
                        case StageAnchor.BottomCenter: x0 = -w / 2; y0 = -hh; break;
                        case StageAnchor.TopLeft: x0 = 0; y0 = 0; break;
                        default: x0 = -w / 2; y0 = -hh / 2; break;
                    }
                    Point L(double lx, double ly) => Add(a, Rotate(new Point(lx, ly), rot));
                    draws.Add(new StageDraw { Kind = StageDraw.K.Poly, Pts = [L(x0, y0), L(x0 + w, y0), L(x0 + w, y0 + hh), L(x0, y0 + hh), L(x0, y0)], Color = col, Fill = true, Faint = true });
                    draws.Add(new StageDraw { Kind = StageDraw.K.Poly, Pts = [L(x0, y0), L(x0 + w, y0), L(x0 + w, y0 + hh), L(x0, y0 + hh), L(x0, y0)], Color = col });
                    Center(h.Label, h.X, h.Y, col);
                    string x = h.X!, y = h.Y!, wn = h.W!, hn = h.H!;
                    var rn = h.Rot;
                    var anchor = h.Anchor;
                    bool top = anchor == StageAnchor.BottomCenter;
                    grips.Add(new StageGrip
                    {
                        P = top ? L(x0 + w, y0) : L(x0 + w, y0 + hh), Label = "幅・高さ", Color = col, Shape = StageGrip.S.Square,
                        Drag = (p, sh) =>
                        {
                            var loc = Rotate(new Point(p.X - Pos(x, y).X, p.Y - Pos(x, y).Y), -(rn != null ? V(rn) : 0));
                            double nw, nh;
                            switch (anchor)
                            {
                                case StageAnchor.LeftCenter: nw = loc.X; nh = 2 * Math.Abs(loc.Y); break;
                                case StageAnchor.BottomCenter: nw = 2 * Math.Abs(loc.X); nh = -loc.Y; break;
                                case StageAnchor.TopLeft: nw = loc.X; nh = loc.Y; break;
                                default: nw = 2 * Math.Abs(loc.X); nh = 2 * Math.Abs(loc.Y); break;
                            }
                            SetLen(wn, Math.Max(1, nw), sh);
                            SetLen(hn, Math.Max(1, nh), sh);
                        },
                        Readout = () => $"幅 {Fmt(wn, "px")}  高さ {Fmt(hn, "px")}",
                    });
                    if (rn != null)
                    {
                        // 回転のつまみ: 四角の上の辺の、少し上
                        double gap = unit * 0.06;
                        grips.Add(new StageGrip
                        {
                            P = L(x0 + w / 2, y0 - gap), Label = "回転", Color = col, Shape = StageGrip.S.Ring,
                            Drag = (p, sh) =>
                            {
                                var m = Add(Pos(x, y), Rotate(new Point(x0 + w / 2, y0 + hh / 2), V(rn)));
                                double s = Math.Atan2(p.Y - m.Y, p.X - m.X) * 180 / Math.PI + 90;
                                if (Snapping(sh)) s = Round(s, 15);
                                double old = V(rn);
                                s += 360 * Math.Round((old - s) / 360);
                                SetV(rn, s);
                            },
                            Readout = () => $"回転 {Fmt(rn, "°")}",
                        });
                        draws.Add(new StageDraw { Kind = StageDraw.K.Line, A = L(x0 + w / 2, y0), B = L(x0 + w / 2, y0 - gap), Color = col, Faint = true });
                    }
                    break;
                }

                case StageKind.Span:
                {
                    var c = Pos(h.X, h.Y);
                    double len = Math.Max(0, V(h.Len));
                    draws.Add(new StageDraw { Kind = StageDraw.K.Line, A = new Point(c.X - len / 2, c.Y), B = new Point(c.X + len / 2, c.Y), Color = col });
                    draws.Add(new StageDraw { Kind = StageDraw.K.Line, A = new Point(c.X - len / 2, c.Y - unit * 0.02), B = new Point(c.X - len / 2, c.Y + unit * 0.02), Color = col });
                    Center(h.Label, h.X, h.Y, col);
                    string x = h.X!, y = h.Y!, ln = h.Len!;
                    grips.Add(new StageGrip
                    {
                        P = new Point(c.X + len / 2, c.Y), Label = "幅", Color = col, Shape = StageGrip.S.Square,
                        Drag = (p, sh) => SetLen(ln, 2 * Math.Abs(p.X - Pos(x, y).X), sh),
                        Readout = () => $"幅 {Fmt(ln, "px")}",
                    });
                    break;
                }

                case StageKind.Segment:
                {
                    int ocol = ColorOf(h.OriginLabel) % Palette.Length;
                    var a = Pos(h.X, h.Y);
                    var b = Pos(h.X2, h.Y2);
                    draws.Add(new StageDraw { Kind = StageDraw.K.Line, A = a, B = b, Color = col, Arrow = true, Dashed = h.Dashed });
                    Center(h.OriginLabel, h.X, h.Y, ocol);
                    Center(h.Label, h.X2, h.Y2, col, StageGrip.S.Diamond);
                    break;
                }

                case StageKind.Path:
                {
                    int n = h.Xs!.Length;
                    if (h.Count != null && R(h.Count) != null) n = (int)Math.Round(V(h.Count, editIndex, n));
                    n = Math.Clamp(n, 1, h.Xs.Length);
                    var pts = Enumerable.Range(0, n).Select(i => Pos(h.Xs[i], h.Ys![i])).ToList();
                    if (pts.Count > 1) draws.Add(new StageDraw { Kind = StageDraw.K.Poly, Pts = pts, Color = col, Dashed = true });
                    for (int i = 0; i < n; i++) Center($"地点{i + 1}", h.Xs[i], h.Ys![i], col, i == 0 ? StageGrip.S.Dot : StageGrip.S.Diamond);
                    break;
                }

                case StageKind.HLine:
                {
                    double y = V(h.Y) / 100 * canvasH;
                    draws.Add(new StageDraw { Kind = StageDraw.K.Line, A = new Point(0, y), B = new Point(canvasW, y), Color = col, Dashed = true });
                    string yn = h.Y!;
                    grips.Add(new StageGrip
                    {
                        P = new Point(canvasW * 0.06, y), Label = h.Label, Color = col, Shape = StageGrip.S.Square,
                        Drag = (p, sh) => SetPos(null, yn, p, sh),
                        Readout = () => $"Y {V(yn):0.#}%",
                    });
                    break;
                }

                case StageKind.VLine:
                {
                    double x = V(h.X) / 100 * canvasW;
                    draws.Add(new StageDraw { Kind = StageDraw.K.Line, A = new Point(x, 0), B = new Point(x, canvasH), Color = col, Dashed = true });
                    string xn = h.X!;
                    grips.Add(new StageGrip
                    {
                        P = new Point(x, canvasH * h.GripY), Label = h.Label, Color = col, Shape = StageGrip.S.Square,
                        Drag = (p, sh) => SetPos(xn, null, p, sh),
                        Readout = () => $"X {V(xn):0.#}%",
                    });
                    break;
                }

                case StageKind.Dial:
                {
                    var c = new Point(canvasW / 2, canvasH / 2);
                    double r = unit * 0.24;
                    double s = ToScreen(V(h.Angle), h.Zero);
                    var end = Add(c, Dir(s, r));
                    draws.Add(new StageDraw { Kind = StageDraw.K.Circle, A = c, R = r, Color = col, Faint = true, Dashed = true });
                    draws.Add(new StageDraw { Kind = StageDraw.K.Line, A = c, B = end, Color = col, Arrow = true });
                    string an = h.Angle!;
                    var zero = h.Zero;
                    grips.Add(new StageGrip
                    {
                        P = end, Label = h.Label, Color = col, Shape = StageGrip.S.Ring,
                        Drag = (p, sh) => SetAngle(an, zero, c, p, sh),
                        Readout = () => $"向き {Fmt(an, "°")}",
                    });
                    break;
                }
            }
        }
    }
}

/// <summary>配置画面の板(画面を模した四角の上に、つまみを描く・ドラッグで動かす)</summary>
public sealed class ShapeStageView : FrameworkElement
{
    public static readonly DependencyProperty StageProperty = DependencyProperty.Register(
        nameof(Stage), typeof(ShapeStageVm), typeof(ShapeStageView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
    public ShapeStageVm? Stage { get => (ShapeStageVm?)GetValue(StageProperty); set => SetValue(StageProperty, value); }

    public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
        nameof(Version), typeof(int), typeof(ShapeStageView), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));
    public int Version { get => (int)GetValue(VersionProperty); set => SetValue(VersionProperty, value); }

    static readonly Typeface Face = new("Yu Gothic UI");
    const double GripHit = 13;

    readonly List<StageDraw> draws = new();
    readonly List<StageGrip> grips = new();
    int dragIndex = -1, hoverIndex = -1;
    Point pendingPoint;
    bool pendingShift, hasPending;
    long lastApply;
    double scale = 1, offX, offY;

    public ShapeStageView()
    {
        Focusable = true;
        ClipToBounds = true;
        Cursor = Cursors.Arrow;
        IsVisibleChanged += (_, _) => { if (Stage != null) Stage.ViewVisible = IsVisible; };
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == StageProperty && e.NewValue is ShapeStageVm st) st.ViewVisible = IsVisible;
    }

    Brush Res(string key, Color fallback)
    {
        if (TryFindResource(key) is Brush b) return b;
        var s = new SolidColorBrush(fallback);
        s.Freeze();
        return s;
    }

    FormattedText Text(string s, Brush brush, double size = 11) =>
        new(s, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Face, size, brush, VisualTreeHelper.GetDpi(this).PixelsPerDip);

    static Brush B(Color c, double opacity = 1)
    {
        var b = new SolidColorBrush(c) { Opacity = opacity };
        b.Freeze();
        return b;
    }

    /// <summary>画面(プレビュー) → この部品の上の位置。画面の外にはみ出した物も少し見えるよう、まわりに余白をとる</summary>
    void Fit(ShapeStageVm st)
    {
        double margin = 0.06;
        double vw = st.ScreenW * (1 + margin * 2), vh = st.ScreenH * (1 + margin * 2);
        scale = Math.Min(Math.Max(10, ActualWidth) / vw, Math.Max(10, ActualHeight) / vh);
        offX = (ActualWidth - st.ScreenW * scale) / 2;
        offY = (ActualHeight - st.ScreenH * scale) / 2;
    }

    Point ScreenToView(Point p) => new(offX + p.X * scale, offY + p.Y * scale);
    /// <summary>図形の中のピクセル → この部品の上の位置</summary>
    Point ToView(Point p) => Stage is { } st ? ScreenToView(st.ToScreen(p)) : ScreenToView(p);
    /// <summary>この部品の上の位置 → 図形の中のピクセル</summary>
    Point ToCanvas(Point v)
    {
        var sp = new Point((v.X - offX) / scale, (v.Y - offY) / scale);
        return Stage is { } st ? st.FromScreen(sp) : sp;
    }

    void Collect()
    {
        draws.Clear();
        grips.Clear();
        Stage?.Compose(draws, grips);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var panel = Res("Bg", Color.FromRgb(0x1B, 0x1C, 0x20));
        var stroke = Res("Stroke", Color.FromRgb(0x3F, 0x42, 0x4B));
        var sub = Res("Sub", Color.FromRgb(0x9C, 0xA3, 0xAF));
        var text = Res("Text", Colors.White);
        dc.DrawRoundedRectangle(panel, null, new Rect(0, 0, ActualWidth, ActualHeight), 8, 8);
        var st = Stage;
        if (st == null) return;
        Fit(st);
        Collect();

        // 画面(プレビューの写し)と、中心の十字
        var screen = new Rect(ScreenToView(new Point(0, 0)), ScreenToView(new Point(st.ScreenW, st.ScreenH)));
        dc.DrawRectangle(B(Color.FromRgb(0x0E, 0x0F, 0x12)), null, screen);
        if (st.Backdrop != null) dc.DrawImage(st.Backdrop, screen);
        else
        {
            var grid = new Pen(B(Color.FromRgb(0xFF, 0xFF, 0xFF), 0.06), 1);
            grid.Freeze();
            for (int i = 1; i < 10; i++)
            {
                double x = screen.Left + screen.Width * i / 10, y = screen.Top + screen.Height * i / 10;
                dc.DrawLine(grid, new Point(x, screen.Top), new Point(x, screen.Bottom));
                dc.DrawLine(grid, new Point(screen.Left, y), new Point(screen.Right, y));
            }
        }
        // 図形の描く範囲(画面とずれている時だけ: アイテムを動かした・拡大した・指定サイズの時など)
        var corners = new[] { new Point(0, 0), new Point(st.CanvasW, 0), new Point(st.CanvasW, st.CanvasH), new Point(0, st.CanvasH) }.Select(ToView).ToList();
        if (corners.Zip(new[] { screen.TopLeft, screen.TopRight, screen.BottomRight, screen.BottomLeft }).Any(t => (t.First - t.Second).Length > 1.5))
        {
            var cg = new StreamGeometry();
            using (var ctx = cg.Open()) { ctx.BeginFigure(corners[0], false, true); ctx.PolyLineTo(corners.Skip(1).ToList(), true, true); }
            cg.Freeze();
            var cpen = new Pen(B(Color.FromRgb(0xFF, 0xFF, 0xFF), 0.35), 1) { DashStyle = new DashStyle(new double[] { 6, 4 }, 0) };
            cpen.Freeze();
            dc.DrawGeometry(null, cpen, cg);
        }
        var cross = new Pen(B(Color.FromRgb(0xFF, 0xFF, 0xFF), 0.18), 1) { DashStyle = new DashStyle(new double[] { 4, 4 }, 0) };
        cross.Freeze();
        dc.DrawLine(cross, new Point(screen.Left + screen.Width / 2, screen.Top), new Point(screen.Left + screen.Width / 2, screen.Bottom));
        dc.DrawLine(cross, new Point(screen.Left, screen.Top + screen.Height / 2), new Point(screen.Right, screen.Top + screen.Height / 2));
        var edge = new Pen(stroke, 1.5);
        edge.Freeze();
        dc.DrawRectangle(null, edge, screen);

        // 図形の形のめやす(プレビューの上でも見えるよう、線の下に黒い縁)
        var shadow = new Pen(B(Colors.Black, 0.55), 4.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        shadow.Freeze();
        foreach (var d in draws)
        {
            var c = ShapeStageVm.Palette[d.Color % ShapeStageVm.Palette.Length];
            var pen = new Pen(B(c, d.Faint ? 0.5 : 0.95), d.Faint ? 1.3 : 2.2) { LineJoin = PenLineJoin.Round, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            if (d.Dashed) pen.DashStyle = new DashStyle(new double[] { 3, 3 }, 0);
            pen.Freeze();
            switch (d.Kind)
            {
                case StageDraw.K.Line:
                {
                    var a = ToView(d.A); var b = ToView(d.B);
                    if (!d.Faint) dc.DrawLine(shadow, a, b);
                    dc.DrawLine(pen, a, b);
                    if (d.Arrow) DrawArrowHead(dc, a, b, B(c, 0.95));
                    break;
                }
                case StageDraw.K.Circle:
                    dc.DrawEllipse(d.Fill ? B(c, 0.12) : null, pen, ToView(d.A), d.R * scale * st.LenScale, d.R * scale * st.LenScale);
                    break;
                case StageDraw.K.Dot:
                {
                    var p = ToView(d.A);
                    dc.DrawEllipse(B(c, 0.55), null, p, d.R, d.R);
                    if (d.Text != null) dc.DrawText(Text(d.Text, B(c, 0.8), 9), new Point(p.X + 5, p.Y - 14));
                    break;
                }
                case StageDraw.K.Poly when d.Pts != null && d.Pts.Count > 1:
                {
                    var g = new StreamGeometry();
                    using (var ctx = g.Open())
                    {
                        ctx.BeginFigure(ToView(d.Pts[0]), d.Fill, d.Fill);
                        ctx.PolyLineTo(d.Pts.Skip(1).Select(ToView).ToList(), true, true);
                    }
                    g.Freeze();
                    if (d.Fill) dc.DrawGeometry(B(c, 0.13), null, g);
                    else dc.DrawGeometry(null, pen, g);
                    break;
                }
            }
        }

        // つまみと名前
        var outline = new Pen(B(Colors.White), 1.6);
        outline.Freeze();
        var labelBg = B(Color.FromRgb(0x10, 0x11, 0x14), 0.78);
        for (int i = 0; i < grips.Count; i++)
        {
            var g = grips[i];
            var c = ShapeStageVm.Palette[g.Color % ShapeStageVm.Palette.Length];
            var p = ToView(g.P);
            bool hot = i == hoverIndex || i == dragIndex;
            double r = hot ? 8 : 6.5;
            var fill = B(c);
            switch (g.Shape)
            {
                case StageGrip.S.Square: dc.DrawRectangle(fill, outline, new Rect(p.X - r, p.Y - r, r * 2, r * 2)); break;
                case StageGrip.S.Diamond:
                {
                    var geo = new StreamGeometry();
                    using (var ctx = geo.Open())
                    {
                        ctx.BeginFigure(new Point(p.X, p.Y - r * 1.25), true, true);
                        ctx.PolyLineTo(new[] { new Point(p.X + r * 1.25, p.Y), new Point(p.X, p.Y + r * 1.25), new Point(p.X - r * 1.25, p.Y) }, true, true);
                    }
                    geo.Freeze();
                    dc.DrawGeometry(fill, outline, geo);
                    break;
                }
                case StageGrip.S.Ring:
                    dc.DrawEllipse(B(Color.FromRgb(0x10, 0x11, 0x14), 0.85), new Pen(fill, 3), p, r, r);
                    break;
                default: dc.DrawEllipse(fill, outline, p, r, r); break;
            }
            var ft = Text(g.Label, text, 11);
            var lp = new Point(p.X + r + 5, p.Y - ft.Height - 2);
            if (lp.X + ft.Width > ActualWidth - 4) lp.X = p.X - r - 5 - ft.Width;
            if (lp.Y < 2) lp.Y = p.Y + r + 2;
            dc.DrawRoundedRectangle(labelBg, null, new Rect(lp.X - 4, lp.Y - 1, ft.Width + 8, ft.Height + 2), 4, 4);
            dc.DrawText(ft, lp);
        }

        // いま触っているつまみの値
        int show = dragIndex >= 0 ? dragIndex : hoverIndex;
        string info = show >= 0 && show < grips.Count ? $"{grips[show].Label}: {grips[show].Readout?.Invoke()}" : st.SizeText;
        var it = Text(info, show >= 0 ? text : sub, 11.5);
        dc.DrawRoundedRectangle(labelBg, null, new Rect(8, 8, it.Width + 14, it.Height + 6), 5, 5);
        dc.DrawText(it, new Point(15, 11));
    }

    static void DrawArrowHead(DrawingContext dc, Point a, Point b, Brush brush)
    {
        double dx = b.X - a.X, dy = b.Y - a.Y, len = Math.Sqrt(dx * dx + dy * dy);
        if (len < 4) return;
        dx /= len; dy /= len;
        double s = 11;
        var p1 = new Point(b.X - dx * s - dy * s * 0.55, b.Y - dy * s + dx * s * 0.55);
        var p2 = new Point(b.X - dx * s + dy * s * 0.55, b.Y - dy * s - dx * s * 0.55);
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(b, true, true);
            ctx.PolyLineTo(new[] { p1, p2 }, true, true);
        }
        g.Freeze();
        dc.DrawGeometry(brush, null, g);
    }

    int HitGrip(Point v)
    {
        Collect();
        int best = -1;
        double bestD = GripHit;
        // 後から描いたもの(上に見えているもの)を優先する
        for (int i = grips.Count - 1; i >= 0; i--)
        {
            var p = ToView(grips[i].P);
            double d = Math.Sqrt((p.X - v.X) * (p.X - v.X) + (p.Y - v.Y) * (p.Y - v.Y));
            if (d < bestD - 0.5) { bestD = d; best = i; }
        }
        return best;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var st = Stage;
        if (st == null) return;
        Focus();
        Fit(st);
        var v = e.GetPosition(this);
        int hit = HitGrip(v);
        if (hit < 0 && e.ClickCount == 2 && grips.Count > 0)
        {
            // 空いている所をダブルクリック: 最初のつまみ(主な位置)を、そこへ動かす
            grips[0].Drag?.Invoke(ToCanvas(v), Keyboard.Modifiers.HasFlag(ModifierKeys.Shift));
            st.Bump();
            e.Handled = true;
            return;
        }
        if (hit < 0) return;
        dragIndex = hit;
        st.Dragging = true;
        CaptureMouse();
        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var st = Stage;
        if (st == null) return;
        Fit(st);
        var v = e.GetPosition(this);
        if (dragIndex >= 0 && IsMouseCaptured)
        {
            pendingPoint = ToCanvas(v);
            pendingShift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
            hasPending = true;
            long now = Environment.TickCount64;
            if (now - lastApply >= 30) ApplyPending();
            return;
        }
        int hover = HitGrip(v);
        if (hover != hoverIndex)
        {
            hoverIndex = hover;
            Cursor = hover >= 0 ? Cursors.Hand : Cursors.Arrow;
            InvalidateVisual();
        }
    }

    void ApplyPending()
    {
        if (!hasPending || dragIndex < 0) return;
        hasPending = false;
        lastApply = Environment.TickCount64;
        Collect();
        if (dragIndex < grips.Count) grips[dragIndex].Drag?.Invoke(pendingPoint, pendingShift);
        Stage?.Bump();
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (dragIndex < 0) return;
        ApplyPending();
        dragIndex = -1;
        if (Stage != null) Stage.Dragging = false;
        if (IsMouseCaptured) ReleaseMouseCapture();
        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        dragIndex = -1;
        if (Stage != null && Stage.Dragging) Stage.Dragging = false;
        InvalidateVisual();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (hoverIndex >= 0 && dragIndex < 0) { hoverIndex = -1; InvalidateVisual(); }
    }
}
