using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// No. 2687〜2886(gen_v134.py で生成。部品の並びで描くアイコン)

public sealed class X2687Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 かえる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2687Parameter(s); }
public sealed class X2687Parameter : Q6Art { public X2687Parameter() : this(null) { } public X2687Parameter(SharedDataStore? s) : base(s, 65, 0, 0, 0) { Color1 = Hex(0x5DBB63); Color2 = Hex(0xC8E6A0); } }
public sealed class X2688Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 しばいぬ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2688Parameter(s); }
public sealed class X2688Parameter : Q6Art { public X2688Parameter() : this(null) { } public X2688Parameter(SharedDataStore? s) : base(s, 65, 1, 0, 0) { Color1 = Hex(0xE0A05A); Color2 = Hex(0xFFF3E0); } }
public sealed class X2689Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 あらいぐま"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2689Parameter(s); }
public sealed class X2689Parameter : Q6Art { public X2689Parameter() : this(null) { } public X2689Parameter(SharedDataStore? s) : base(s, 65, 2, 0, 0) { Color1 = Hex(0x9E9E9E); Color2 = Hex(0x37474F); } }
public sealed class X2690Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 ラッコ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2690Parameter(s); }
public sealed class X2690Parameter : Q6Art { public X2690Parameter() : this(null) { } public X2690Parameter(SharedDataStore? s) : base(s, 65, 3, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0xD7CCC8); } }
public sealed class X2691Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 アザラシ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2691Parameter(s); }
public sealed class X2691Parameter : Q6Art { public X2691Parameter() : this(null) { } public X2691Parameter(SharedDataStore? s) : base(s, 65, 4, 0, 0) { Color1 = Hex(0xCFD8DC); Color2 = Hex(0x90A4AE); } }
public sealed class X2692Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 カピバラ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2692Parameter(s); }
public sealed class X2692Parameter : Q6Art { public X2692Parameter() : this(null) { } public X2692Parameter(SharedDataStore? s) : base(s, 65, 5, 0, 0) { Color1 = Hex(0xA0703A); Color2 = Hex(0x6D4C3A); } }
public sealed class X2693Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 レッサーパンダ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2693Parameter(s); }
public sealed class X2693Parameter : Q6Art { public X2693Parameter() : this(null) { } public X2693Parameter(SharedDataStore? s) : base(s, 65, 6, 0, 0) { Color1 = Hex(0xD35400); Color2 = Hex(0xFFFFFF); } }
public sealed class X2694Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 アルパカ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2694Parameter(s); }
public sealed class X2694Parameter : Q6Art { public X2694Parameter() : this(null) { } public X2694Parameter(SharedDataStore? s) : base(s, 65, 7, 0, 0) { Color1 = Hex(0xFFF3E0); Color2 = Hex(0xD7B48A); } }
public sealed class X2695Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 ハリネズミ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2695Parameter(s); }
public sealed class X2695Parameter : Q6Art { public X2695Parameter() : this(null) { } public X2695Parameter(SharedDataStore? s) : base(s, 65, 8, 0, 0) { Color1 = Hex(0xF2D3A2); Color2 = Hex(0x6D4C41); } }
public sealed class X2696Plugin : QuickShapePlugin { public override string Name => "演出：動物の顔 ナマケモノ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2696Parameter(s); }
public sealed class X2696Parameter : Q6Art { public X2696Parameter() : this(null) { } public X2696Parameter(SharedDataStore? s) : base(s, 65, 9, 0, 0) { Color1 = Hex(0xA1887F); Color2 = Hex(0xD7CCC8); } }
public sealed class X2697Plugin : QuickShapePlugin { public override string Name => "演出：果物 マンゴー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2697Parameter(s); }
public sealed class X2697Parameter : Q6Art { public X2697Parameter() : this(null) { } public X2697Parameter(SharedDataStore? s) : base(s, 66, 0, 0, 0) { Color1 = Hex(0xFFB300); Color2 = Hex(0xE53935); } }
public sealed class X2698Plugin : QuickShapePlugin { public override string Name => "演出：果物 ドラゴンフルーツ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2698Parameter(s); }
public sealed class X2698Parameter : Q6Art { public X2698Parameter() : this(null) { } public X2698Parameter(SharedDataStore? s) : base(s, 66, 1, 0, 0) { Color1 = Hex(0xE91E63); Color2 = Hex(0x9CCC65); } }
public sealed class X2699Plugin : QuickShapePlugin { public override string Name => "演出：果物 アボカド"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2699Parameter(s); }
public sealed class X2699Parameter : Q6Art { public X2699Parameter() : this(null) { } public X2699Parameter(SharedDataStore? s) : base(s, 66, 2, 0, 0) { Color1 = Hex(0x33691E); Color2 = Hex(0xD4E157); } }
public sealed class X2700Plugin : QuickShapePlugin { public override string Name => "演出：果物 いちじく"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2700Parameter(s); }
public sealed class X2700Parameter : Q6Art { public X2700Parameter() : this(null) { } public X2700Parameter(SharedDataStore? s) : base(s, 66, 3, 0, 0) { Color1 = Hex(0x6A1B9A); Color2 = Hex(0xF48FB1); } }
public sealed class X2701Plugin : QuickShapePlugin { public override string Name => "演出：果物 ライチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2701Parameter(s); }
public sealed class X2701Parameter : Q6Art { public X2701Parameter() : this(null) { } public X2701Parameter(SharedDataStore? s) : base(s, 66, 4, 0, 0) { Color1 = Hex(0xC62828); Color2 = Hex(0xFFF8E1); } }
public sealed class X2702Plugin : QuickShapePlugin { public override string Name => "演出：果物 ざくろ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2702Parameter(s); }
public sealed class X2702Parameter : Q6Art { public X2702Parameter() : this(null) { } public X2702Parameter(SharedDataStore? s) : base(s, 66, 5, 0, 0) { Color1 = Hex(0xC62828); Color2 = Hex(0xF06292); } }
public sealed class X2703Plugin : QuickShapePlugin { public override string Name => "演出：果物 ゆず"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2703Parameter(s); }
public sealed class X2703Parameter : Q6Art { public X2703Parameter() : this(null) { } public X2703Parameter(SharedDataStore? s) : base(s, 66, 6, 0, 0) { Color1 = Hex(0xFDD835); Color2 = Hex(0x43A047); } }
public sealed class X2704Plugin : QuickShapePlugin { public override string Name => "演出：果物 びわ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2704Parameter(s); }
public sealed class X2704Parameter : Q6Art { public X2704Parameter() : this(null) { } public X2704Parameter(SharedDataStore? s) : base(s, 66, 7, 0, 0) { Color1 = Hex(0xF9A825); Color2 = Hex(0x43A047); } }
public sealed class X2705Plugin : QuickShapePlugin { public override string Name => "演出：果物 パパイヤ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2705Parameter(s); }
public sealed class X2705Parameter : Q6Art { public X2705Parameter() : this(null) { } public X2705Parameter(SharedDataStore? s) : base(s, 66, 8, 0, 0) { Color1 = Hex(0xFF8F00); Color2 = Hex(0x2E2E2E); } }
public sealed class X2706Plugin : QuickShapePlugin { public override string Name => "演出：果物 ココナッツ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2706Parameter(s); }
public sealed class X2706Parameter : Q6Art { public X2706Parameter() : this(null) { } public X2706Parameter(SharedDataStore? s) : base(s, 66, 9, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0xFFFFFF); } }
public sealed class X2707Plugin : QuickShapePlugin { public override string Name => "演出：野菜 だいこん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2707Parameter(s); }
public sealed class X2707Parameter : Q6Art { public X2707Parameter() : this(null) { } public X2707Parameter(SharedDataStore? s) : base(s, 67, 0, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x43A047); } }
public sealed class X2708Plugin : QuickShapePlugin { public override string Name => "演出：野菜 さつまいも"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2708Parameter(s); }
public sealed class X2708Parameter : Q6Art { public X2708Parameter() : this(null) { } public X2708Parameter(SharedDataStore? s) : base(s, 67, 1, 0, 0) { Color1 = Hex(0xAD1457); Color2 = Hex(0xFFE082); } }
public sealed class X2709Plugin : QuickShapePlugin { public override string Name => "演出：野菜 レタス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2709Parameter(s); }
public sealed class X2709Parameter : Q6Art { public X2709Parameter() : this(null) { } public X2709Parameter(SharedDataStore? s) : base(s, 67, 2, 0, 0) { Color1 = Hex(0x9CCC65); Color2 = Hex(0x7CB342); } }
public sealed class X2710Plugin : QuickShapePlugin { public override string Name => "演出：野菜 アスパラ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2710Parameter(s); }
public sealed class X2710Parameter : Q6Art { public X2710Parameter() : this(null) { } public X2710Parameter(SharedDataStore? s) : base(s, 67, 3, 0, 0) { Color1 = Hex(0x7CB342); Color2 = Hex(0x558B2F); } }
public sealed class X2711Plugin : QuickShapePlugin { public override string Name => "演出：野菜 パプリカ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2711Parameter(s); }
public sealed class X2711Parameter : Q6Art { public X2711Parameter() : this(null) { } public X2711Parameter(SharedDataStore? s) : base(s, 67, 4, 0, 0) { Color1 = Hex(0xFFB300); Color2 = Hex(0x43A047); } }
public sealed class X2712Plugin : QuickShapePlugin { public override string Name => "演出：野菜 オクラ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2712Parameter(s); }
public sealed class X2712Parameter : Q6Art { public X2712Parameter() : this(null) { } public X2712Parameter(SharedDataStore? s) : base(s, 67, 5, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0x2E7D32); } }
public sealed class X2713Plugin : QuickShapePlugin { public override string Name => "演出：野菜 れんこん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2713Parameter(s); }
public sealed class X2713Parameter : Q6Art { public X2713Parameter() : this(null) { } public X2713Parameter(SharedDataStore? s) : base(s, 67, 6, 0, 0) { Color1 = Hex(0xF5E1C0); Color2 = Hex(0xC8A27A); } }
public sealed class X2714Plugin : QuickShapePlugin { public override string Name => "演出：野菜 ごぼう"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2714Parameter(s); }
public sealed class X2714Parameter : Q6Art { public X2714Parameter() : this(null) { } public X2714Parameter(SharedDataStore? s) : base(s, 67, 7, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0x43A047); } }
public sealed class X2715Plugin : QuickShapePlugin { public override string Name => "演出：野菜 そらまめ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2715Parameter(s); }
public sealed class X2715Parameter : Q6Art { public X2715Parameter() : this(null) { } public X2715Parameter(SharedDataStore? s) : base(s, 67, 8, 0, 0) { Color1 = Hex(0x8BC34A); Color2 = Hex(0xC5E1A5); } }
public sealed class X2716Plugin : QuickShapePlugin { public override string Name => "演出：野菜 しょうが"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2716Parameter(s); }
public sealed class X2716Parameter : Q6Art { public X2716Parameter() : this(null) { } public X2716Parameter(SharedDataStore? s) : base(s, 67, 9, 0, 0) { Color1 = Hex(0xE0B97A); Color2 = Hex(0xC69A5A); } }
public sealed class X2717Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 パトカー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2717Parameter(s); }
public sealed class X2717Parameter : Q6Art { public X2717Parameter() : this(null) { } public X2717Parameter(SharedDataStore? s) : base(s, 68, 0, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x20242B); } }
public sealed class X2718Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 救急車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2718Parameter(s); }
public sealed class X2718Parameter : Q6Art { public X2718Parameter() : this(null) { } public X2718Parameter(SharedDataStore? s) : base(s, 68, 1, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2719Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 消防車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2719Parameter(s); }
public sealed class X2719Parameter : Q6Art { public X2719Parameter() : this(null) { } public X2719Parameter(SharedDataStore? s) : base(s, 68, 2, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2720Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 ショベルカー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2720Parameter(s); }
public sealed class X2720Parameter : Q6Art { public X2720Parameter() : this(null) { } public X2720Parameter(SharedDataStore? s) : base(s, 68, 3, 0, 0) { Color1 = Hex(0xFFB300); Color2 = Hex(0x37474F); } }
public sealed class X2721Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 三輪車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2721Parameter(s); }
public sealed class X2721Parameter : Q6Art { public X2721Parameter() : this(null) { } public X2721Parameter(SharedDataStore? s) : base(s, 68, 4, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x37474F); } }
public sealed class X2722Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 スクーター"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2722Parameter(s); }
public sealed class X2722Parameter : Q6Art { public X2722Parameter() : this(null) { } public X2722Parameter(SharedDataStore? s) : base(s, 68, 5, 0, 0) { Color1 = Hex(0x4DB6AC); Color2 = Hex(0x37474F); } }
public sealed class X2723Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 機関車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2723Parameter(s); }
public sealed class X2723Parameter : Q6Art { public X2723Parameter() : this(null) { } public X2723Parameter(SharedDataStore? s) : base(s, 68, 6, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xE53935); } }
public sealed class X2724Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 新幹線"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2724Parameter(s); }
public sealed class X2724Parameter : Q6Art { public X2724Parameter() : this(null) { } public X2724Parameter(SharedDataStore? s) : base(s, 68, 7, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x1E5FAF); } }
public sealed class X2725Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 一輪車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2725Parameter(s); }
public sealed class X2725Parameter : Q6Art { public X2725Parameter() : this(null) { } public X2725Parameter(SharedDataStore? s) : base(s, 68, 8, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0x37474F); } }
public sealed class X2726Plugin : QuickShapePlugin { public override string Name => "演出：乗り物 ベビーカー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2726Parameter(s); }
public sealed class X2726Parameter : Q6Art { public X2726Parameter() : this(null) { } public X2726Parameter(SharedDataStore? s) : base(s, 68, 9, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0x37474F); } }
public sealed class X2727Plugin : QuickShapePlugin { public override string Name => "演出：建物 お店"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2727Parameter(s); }
public sealed class X2727Parameter : Q6Art { public X2727Parameter() : this(null) { } public X2727Parameter(SharedDataStore? s) : base(s, 69, 0, 0, 0) { Color1 = Hex(0xF5E6C8); Color2 = Hex(0x35A86B); } }
public sealed class X2728Plugin : QuickShapePlugin { public override string Name => "演出：建物 観覧車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2728Parameter(s); }
public sealed class X2728Parameter : Q6Art { public X2728Parameter() : this(null) { } public X2728Parameter(SharedDataStore? s) : base(s, 69, 1, 0, 0) { Color1 = Hex(0xE8488A); Color2 = Hex(0x2F86C6); } }
public sealed class X2729Plugin : QuickShapePlugin { public override string Name => "演出：建物 犬小屋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2729Parameter(s); }
public sealed class X2729Parameter : Q6Art { public X2729Parameter() : this(null) { } public X2729Parameter(SharedDataStore? s) : base(s, 69, 2, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xB5651D); } }
public sealed class X2730Plugin : QuickShapePlugin { public override string Name => "演出：建物 ログハウス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2730Parameter(s); }
public sealed class X2730Parameter : Q6Art { public X2730Parameter() : this(null) { } public X2730Parameter(SharedDataStore? s) : base(s, 69, 3, 0, 0) { Color1 = Hex(0xB5651D); Color2 = Hex(0x6D4C41); } }
public sealed class X2731Plugin : QuickShapePlugin { public override string Name => "演出：建物 教会"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2731Parameter(s); }
public sealed class X2731Parameter : Q6Art { public X2731Parameter() : this(null) { } public X2731Parameter(SharedDataStore? s) : base(s, 69, 4, 0, 0) { Color1 = Hex(0xF5F1EA); Color2 = Hex(0x2F86C6); } }
public sealed class X2732Plugin : QuickShapePlugin { public override string Name => "演出：建物 銭湯"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2732Parameter(s); }
public sealed class X2732Parameter : Q6Art { public X2732Parameter() : this(null) { } public X2732Parameter(SharedDataStore? s) : base(s, 69, 5, 0, 0) { Color1 = Hex(0xF5E6C8); Color2 = Hex(0x1E5FAF); } }
public sealed class X2733Plugin : QuickShapePlugin { public override string Name => "演出：建物 消防署"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2733Parameter(s); }
public sealed class X2733Parameter : Q6Art { public X2733Parameter() : this(null) { } public X2733Parameter(SharedDataStore? s) : base(s, 69, 6, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2734Plugin : QuickShapePlugin { public override string Name => "演出：建物 郵便ポスト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2734Parameter(s); }
public sealed class X2734Parameter : Q6Art { public X2734Parameter() : this(null) { } public X2734Parameter(SharedDataStore? s) : base(s, 69, 7, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x37474F); } }
public sealed class X2735Plugin : QuickShapePlugin { public override string Name => "演出：建物 時計台"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2735Parameter(s); }
public sealed class X2735Parameter : Q6Art { public X2735Parameter() : this(null) { } public X2735Parameter(SharedDataStore? s) : base(s, 69, 8, 0, 0) { Color1 = Hex(0xF5E6C8); Color2 = Hex(0x2F86C6); } }
public sealed class X2736Plugin : QuickShapePlugin { public override string Name => "演出：建物 給水塔"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2736Parameter(s); }
public sealed class X2736Parameter : Q6Art { public X2736Parameter() : this(null) { } public X2736Parameter(SharedDataStore? s) : base(s, 69, 9, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x2F86C6); } }
public sealed class X2737Plugin : QuickShapePlugin { public override string Name => "演出：道具 はしご"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2737Parameter(s); }
public sealed class X2737Parameter : Q6Art { public X2737Parameter() : this(null) { } public X2737Parameter(SharedDataStore? s) : base(s, 70, 0, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0xC77B14); } }
public sealed class X2738Plugin : QuickShapePlugin { public override string Name => "演出：道具 ねじ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2738Parameter(s); }
public sealed class X2738Parameter : Q6Art { public X2738Parameter() : this(null) { } public X2738Parameter(SharedDataStore? s) : base(s, 70, 1, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x78909C); } }
public sealed class X2739Plugin : QuickShapePlugin { public override string Name => "演出：道具 ペンキ缶"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2739Parameter(s); }
public sealed class X2739Parameter : Q6Art { public X2739Parameter() : this(null) { } public X2739Parameter(SharedDataStore? s) : base(s, 70, 2, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0xB0BEC5); } }
public sealed class X2740Plugin : QuickShapePlugin { public override string Name => "演出：道具 くぎ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2740Parameter(s); }
public sealed class X2740Parameter : Q6Art { public X2740Parameter() : this(null) { } public X2740Parameter(SharedDataStore? s) : base(s, 70, 3, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x78909C); } }
public sealed class X2741Plugin : QuickShapePlugin { public override string Name => "演出：道具 メジャー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2741Parameter(s); }
public sealed class X2741Parameter : Q6Art { public X2741Parameter() : this(null) { } public X2741Parameter(SharedDataStore? s) : base(s, 70, 4, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0x37474F); } }
public sealed class X2742Plugin : QuickShapePlugin { public override string Name => "演出：道具 ガムテープ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2742Parameter(s); }
public sealed class X2742Parameter : Q6Art { public X2742Parameter() : this(null) { } public X2742Parameter(SharedDataStore? s) : base(s, 70, 5, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0x8D6E63); } }
public sealed class X2743Plugin : QuickShapePlugin { public override string Name => "演出：道具 工具箱"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2743Parameter(s); }
public sealed class X2743Parameter : Q6Art { public X2743Parameter() : this(null) { } public X2743Parameter(SharedDataStore? s) : base(s, 70, 6, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x37474F); } }
public sealed class X2744Plugin : QuickShapePlugin { public override string Name => "演出：道具 はけ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2744Parameter(s); }
public sealed class X2744Parameter : Q6Art { public X2744Parameter() : this(null) { } public X2744Parameter(SharedDataStore? s) : base(s, 70, 7, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0xFFC107); } }
public sealed class X2745Plugin : QuickShapePlugin { public override string Name => "演出：道具 軍手"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2745Parameter(s); }
public sealed class X2745Parameter : Q6Art { public X2745Parameter() : this(null) { } public X2745Parameter(SharedDataStore? s) : base(s, 70, 8, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2746Plugin : QuickShapePlugin { public override string Name => "演出：道具 工事用ヘルメット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2746Parameter(s); }
public sealed class X2746Parameter : Q6Art { public X2746Parameter() : this(null) { } public X2746Parameter(SharedDataStore? s) : base(s, 70, 9, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0x37474F); } }
public sealed class X2747Plugin : QuickShapePlugin { public override string Name => "演出：楽器 笛"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2747Parameter(s); }
public sealed class X2747Parameter : Q6Art { public X2747Parameter() : this(null) { } public X2747Parameter(SharedDataStore? s) : base(s, 71, 0, 0, 0) { Color1 = Hex(0xF5E6C8); Color2 = Hex(0x5D4037); } }
public sealed class X2748Plugin : QuickShapePlugin { public override string Name => "演出：楽器 シンバル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2748Parameter(s); }
public sealed class X2748Parameter : Q6Art { public X2748Parameter() : this(null) { } public X2748Parameter(SharedDataStore? s) : base(s, 71, 1, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0x5D4037); } }
public sealed class X2749Plugin : QuickShapePlugin { public override string Name => "演出：楽器 アコーディオン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2749Parameter(s); }
public sealed class X2749Parameter : Q6Art { public X2749Parameter() : this(null) { } public X2749Parameter(SharedDataStore? s) : base(s, 71, 2, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x37474F); } }
public sealed class X2750Plugin : QuickShapePlugin { public override string Name => "演出：楽器 カスタネット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2750Parameter(s); }
public sealed class X2750Parameter : Q6Art { public X2750Parameter() : this(null) { } public X2750Parameter(SharedDataStore? s) : base(s, 71, 3, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x1E5FAF); } }
public sealed class X2751Plugin : QuickShapePlugin { public override string Name => "演出：楽器 トライアングル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2751Parameter(s); }
public sealed class X2751Parameter : Q6Art { public X2751Parameter() : this(null) { } public X2751Parameter(SharedDataStore? s) : base(s, 71, 4, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x5D4037); } }
public sealed class X2752Plugin : QuickShapePlugin { public override string Name => "演出：楽器 ウクレレ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2752Parameter(s); }
public sealed class X2752Parameter : Q6Art { public X2752Parameter() : this(null) { } public X2752Parameter(SharedDataStore? s) : base(s, 71, 5, 0, 0) { Color1 = Hex(0xE0A05A); Color2 = Hex(0x6D4C41); } }
public sealed class X2753Plugin : QuickShapePlugin { public override string Name => "演出：楽器 オカリナ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2753Parameter(s); }
public sealed class X2753Parameter : Q6Art { public X2753Parameter() : this(null) { } public X2753Parameter(SharedDataStore? s) : base(s, 71, 6, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0x0D47A1); } }
public sealed class X2754Plugin : QuickShapePlugin { public override string Name => "演出：楽器 ホルン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2754Parameter(s); }
public sealed class X2754Parameter : Q6Art { public X2754Parameter() : this(null) { } public X2754Parameter(SharedDataStore? s) : base(s, 71, 7, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xC69A2E); } }
public sealed class X2755Plugin : QuickShapePlugin { public override string Name => "演出：楽器 鉄琴"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2755Parameter(s); }
public sealed class X2755Parameter : Q6Art { public X2755Parameter() : this(null) { } public X2755Parameter(SharedDataStore? s) : base(s, 71, 8, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x1E5FAF); } }
public sealed class X2756Plugin : QuickShapePlugin { public override string Name => "演出：楽器 ハンドベル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2756Parameter(s); }
public sealed class X2756Parameter : Q6Art { public X2756Parameter() : this(null) { } public X2756Parameter(SharedDataStore? s) : base(s, 71, 9, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xE53935); } }
public sealed class X2757Plugin : QuickShapePlugin { public override string Name => "演出：服 ぼうし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2757Parameter(s); }
public sealed class X2757Parameter : Q6Art { public X2757Parameter() : this(null) { } public X2757Parameter(SharedDataStore? s) : base(s, 72, 0, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xE53935); } }
public sealed class X2758Plugin : QuickShapePlugin { public override string Name => "演出：服 くつした"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2758Parameter(s); }
public sealed class X2758Parameter : Q6Art { public X2758Parameter() : this(null) { } public X2758Parameter(SharedDataStore? s) : base(s, 72, 1, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xFFFFFF); } }
public sealed class X2759Plugin : QuickShapePlugin { public override string Name => "演出：服 手ぶくろ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2759Parameter(s); }
public sealed class X2759Parameter : Q6Art { public X2759Parameter() : this(null) { } public X2759Parameter(SharedDataStore? s) : base(s, 72, 2, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2760Plugin : QuickShapePlugin { public override string Name => "演出：服 エプロン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2760Parameter(s); }
public sealed class X2760Parameter : Q6Art { public X2760Parameter() : this(null) { } public X2760Parameter(SharedDataStore? s) : base(s, 72, 3, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0xFFFFFF); } }
public sealed class X2761Plugin : QuickShapePlugin { public override string Name => "演出：服 ブーツ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2761Parameter(s); }
public sealed class X2761Parameter : Q6Art { public X2761Parameter() : this(null) { } public X2761Parameter(SharedDataStore? s) : base(s, 72, 4, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0x3E2723); } }
public sealed class X2762Plugin : QuickShapePlugin { public override string Name => "演出：服 サンダル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2762Parameter(s); }
public sealed class X2762Parameter : Q6Art { public X2762Parameter() : this(null) { } public X2762Parameter(SharedDataStore? s) : base(s, 72, 5, 0, 0) { Color1 = Hex(0x29B6F6); Color2 = Hex(0xFFB300); } }
public sealed class X2763Plugin : QuickShapePlugin { public override string Name => "演出：服 ベルト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2763Parameter(s); }
public sealed class X2763Parameter : Q6Art { public X2763Parameter() : this(null) { } public X2763Parameter(SharedDataStore? s) : base(s, 72, 6, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0xB0BEC5); } }
public sealed class X2764Plugin : QuickShapePlugin { public override string Name => "演出：服 浴衣"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2764Parameter(s); }
public sealed class X2764Parameter : Q6Art { public X2764Parameter() : this(null) { } public X2764Parameter(SharedDataStore? s) : base(s, 72, 7, 0, 0) { Color1 = Hex(0x1E5FAF); Color2 = Hex(0xE53935); } }
public sealed class X2765Plugin : QuickShapePlugin { public override string Name => "演出：服 はっぴ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2765Parameter(s); }
public sealed class X2765Parameter : Q6Art { public X2765Parameter() : this(null) { } public X2765Parameter(SharedDataStore? s) : base(s, 72, 8, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2766Plugin : QuickShapePlugin { public override string Name => "演出：服 長ぐつ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2766Parameter(s); }
public sealed class X2766Parameter : Q6Art { public X2766Parameter() : this(null) { } public X2766Parameter(SharedDataStore? s) : base(s, 72, 9, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0x37474F); } }
public sealed class X2767Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 おすし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2767Parameter(s); }
public sealed class X2767Parameter : Q6Art { public X2767Parameter() : this(null) { } public X2767Parameter(SharedDataStore? s) : base(s, 73, 0, 0, 0) { Color1 = Hex(0xFF7043); Color2 = Hex(0xFFFFFF); } }
public sealed class X2768Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 おでん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2768Parameter(s); }
public sealed class X2768Parameter : Q6Art { public X2768Parameter() : this(null) { } public X2768Parameter(SharedDataStore? s) : base(s, 73, 1, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xFFFFFF); } }
public sealed class X2769Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 ぎょうざ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2769Parameter(s); }
public sealed class X2769Parameter : Q6Art { public X2769Parameter() : this(null) { } public X2769Parameter(SharedDataStore? s) : base(s, 73, 2, 0, 0) { Color1 = Hex(0xF5DEB3); Color2 = Hex(0xC77B14); } }
public sealed class X2770Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 えびフライ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2770Parameter(s); }
public sealed class X2770Parameter : Q6Art { public X2770Parameter() : this(null) { } public X2770Parameter(SharedDataStore? s) : base(s, 73, 3, 0, 0) { Color1 = Hex(0xD9822B); Color2 = Hex(0xE53935); } }
public sealed class X2771Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 だんご"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2771Parameter(s); }
public sealed class X2771Parameter : Q6Art { public X2771Parameter() : this(null) { } public X2771Parameter(SharedDataStore? s) : base(s, 73, 4, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0x8D6E63); } }
public sealed class X2772Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 焼きそば"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2772Parameter(s); }
public sealed class X2772Parameter : Q6Art { public X2772Parameter() : this(null) { } public X2772Parameter(SharedDataStore? s) : base(s, 73, 5, 0, 0) { Color1 = Hex(0xC77B14); Color2 = Hex(0x8D5A3B); } }
public sealed class X2773Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 お好み焼き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2773Parameter(s); }
public sealed class X2773Parameter : Q6Art { public X2773Parameter() : this(null) { } public X2773Parameter(SharedDataStore? s) : base(s, 73, 6, 0, 0) { Color1 = Hex(0xC77B14); Color2 = Hex(0x5D4037); } }
public sealed class X2774Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 肉まん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2774Parameter(s); }
public sealed class X2774Parameter : Q6Art { public X2774Parameter() : this(null) { } public X2774Parameter(SharedDataStore? s) : base(s, 73, 7, 0, 0) { Color1 = Hex(0xFFF8E1); Color2 = Hex(0xE0C090); } }
public sealed class X2775Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 コロッケ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2775Parameter(s); }
public sealed class X2775Parameter : Q6Art { public X2775Parameter() : this(null) { } public X2775Parameter(SharedDataStore? s) : base(s, 73, 8, 0, 0) { Color1 = Hex(0xD9822B); Color2 = Hex(0x8BC34A); } }
public sealed class X2776Plugin : QuickShapePlugin { public override string Name => "演出：食べ物 からあげ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2776Parameter(s); }
public sealed class X2776Parameter : Q6Art { public X2776Parameter() : this(null) { } public X2776Parameter(SharedDataStore? s) : base(s, 73, 9, 0, 0) { Color1 = Hex(0xC77B14); Color2 = Hex(0xFFEB3B); } }
public sealed class X2777Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 紅茶"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2777Parameter(s); }
public sealed class X2777Parameter : Q6Art { public X2777Parameter() : this(null) { } public X2777Parameter(SharedDataStore? s) : base(s, 74, 0, 0, 0) { Color1 = Hex(0xC0392B); Color2 = Hex(0xFFFFFF); } }
public sealed class X2778Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 湯のみ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2778Parameter(s); }
public sealed class X2778Parameter : Q6Art { public X2778Parameter() : this(null) { } public X2778Parameter(SharedDataStore? s) : base(s, 74, 1, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xFFFFFF); } }
public sealed class X2779Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 水筒"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2779Parameter(s); }
public sealed class X2779Parameter : Q6Art { public X2779Parameter() : this(null) { } public X2779Parameter(SharedDataStore? s) : base(s, 74, 2, 0, 0) { Color1 = Hex(0x26A69A); Color2 = Hex(0xECEFF1); } }
public sealed class X2780Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 ラムネ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2780Parameter(s); }
public sealed class X2780Parameter : Q6Art { public X2780Parameter() : this(null) { } public X2780Parameter(SharedDataStore? s) : base(s, 74, 3, 0, 0) { Color1 = Hex(0x80DEEA); Color2 = Hex(0x1E5FAF); } }
public sealed class X2781Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 ココア"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2781Parameter(s); }
public sealed class X2781Parameter : Q6Art { public X2781Parameter() : this(null) { } public X2781Parameter(SharedDataStore? s) : base(s, 74, 4, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0xF8BBD0); } }
public sealed class X2782Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 スムージー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2782Parameter(s); }
public sealed class X2782Parameter : Q6Art { public X2782Parameter() : this(null) { } public X2782Parameter(SharedDataStore? s) : base(s, 74, 5, 0, 0) { Color1 = Hex(0xF06292); Color2 = Hex(0xFFFFFF); } }
public sealed class X2783Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 甘酒"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2783Parameter(s); }
public sealed class X2783Parameter : Q6Art { public X2783Parameter() : this(null) { } public X2783Parameter(SharedDataStore? s) : base(s, 74, 6, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2784Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 レモネード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2784Parameter(s); }
public sealed class X2784Parameter : Q6Art { public X2784Parameter() : this(null) { } public X2784Parameter(SharedDataStore? s) : base(s, 74, 7, 0, 0) { Color1 = Hex(0xFFF176); Color2 = Hex(0xFFEB3B); } }
public sealed class X2785Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 炭酸水"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2785Parameter(s); }
public sealed class X2785Parameter : Q6Art { public X2785Parameter() : this(null) { } public X2785Parameter(SharedDataStore? s) : base(s, 74, 8, 0, 0) { Color1 = Hex(0xB3E5FC); Color2 = Hex(0x0288D1); } }
public sealed class X2786Plugin : QuickShapePlugin { public override string Name => "演出：飲み物 麦茶"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2786Parameter(s); }
public sealed class X2786Parameter : Q6Art { public X2786Parameter() : this(null) { } public X2786Parameter(SharedDataStore? s) : base(s, 74, 9, 0, 0) { Color1 = Hex(0xC77B14); Color2 = Hex(0xFFFFFF); } }
public sealed class X2787Plugin : QuickShapePlugin { public override string Name => "演出：虫 はち"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2787Parameter(s); }
public sealed class X2787Parameter : Q6Art { public X2787Parameter() : this(null) { } public X2787Parameter(SharedDataStore? s) : base(s, 75, 0, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0x20242B); } }
public sealed class X2788Plugin : QuickShapePlugin { public override string Name => "演出：虫 とんぼ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2788Parameter(s); }
public sealed class X2788Parameter : Q6Art { public X2788Parameter() : this(null) { } public X2788Parameter(SharedDataStore? s) : base(s, 75, 1, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0xBFE6FF); } }
public sealed class X2789Plugin : QuickShapePlugin { public override string Name => "演出：虫 かぶと虫"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2789Parameter(s); }
public sealed class X2789Parameter : Q6Art { public X2789Parameter() : this(null) { } public X2789Parameter(SharedDataStore? s) : base(s, 75, 2, 0, 0) { Color1 = Hex(0x5D4037); Color2 = Hex(0x3E2723); } }
public sealed class X2790Plugin : QuickShapePlugin { public override string Name => "演出：虫 せみ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2790Parameter(s); }
public sealed class X2790Parameter : Q6Art { public X2790Parameter() : this(null) { } public X2790Parameter(SharedDataStore? s) : base(s, 75, 3, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0xB3E5FC); } }
public sealed class X2791Plugin : QuickShapePlugin { public override string Name => "演出：虫 かたつむり"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2791Parameter(s); }
public sealed class X2791Parameter : Q6Art { public X2791Parameter() : this(null) { } public X2791Parameter(SharedDataStore? s) : base(s, 75, 4, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xC8E6A0); } }
public sealed class X2792Plugin : QuickShapePlugin { public override string Name => "演出：虫 ほたる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2792Parameter(s); }
public sealed class X2792Parameter : Q6Art { public X2792Parameter() : this(null) { } public X2792Parameter(SharedDataStore? s) : base(s, 75, 5, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xE6FF66); } }
public sealed class X2793Plugin : QuickShapePlugin { public override string Name => "演出：虫 ばった"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2793Parameter(s); }
public sealed class X2793Parameter : Q6Art { public X2793Parameter() : this(null) { } public X2793Parameter(SharedDataStore? s) : base(s, 75, 6, 0, 0) { Color1 = Hex(0x7CB342); Color2 = Hex(0x558B2F); } }
public sealed class X2794Plugin : QuickShapePlugin { public override string Name => "演出：虫 みのむし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2794Parameter(s); }
public sealed class X2794Parameter : Q6Art { public X2794Parameter() : this(null) { } public X2794Parameter(SharedDataStore? s) : base(s, 75, 7, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0x6D4C41); } }
public sealed class X2795Plugin : QuickShapePlugin { public override string Name => "演出：虫 くわがた"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2795Parameter(s); }
public sealed class X2795Parameter : Q6Art { public X2795Parameter() : this(null) { } public X2795Parameter(SharedDataStore? s) : base(s, 75, 8, 0, 0) { Color1 = Hex(0x3E2723); Color2 = Hex(0x5D4037); } }
public sealed class X2796Plugin : QuickShapePlugin { public override string Name => "演出：虫 かまきり"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2796Parameter(s); }
public sealed class X2796Parameter : Q6Art { public X2796Parameter() : this(null) { } public X2796Parameter(SharedDataStore? s) : base(s, 75, 9, 0, 0) { Color1 = Hex(0x8BC34A); Color2 = Hex(0x558B2F); } }
public sealed class X2797Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 さかな"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2797Parameter(s); }
public sealed class X2797Parameter : Q6Art { public X2797Parameter() : this(null) { } public X2797Parameter(SharedDataStore? s) : base(s, 76, 0, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x8FD3FF); } }
public sealed class X2798Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 たこ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2798Parameter(s); }
public sealed class X2798Parameter : Q6Art { public X2798Parameter() : this(null) { } public X2798Parameter(SharedDataStore? s) : base(s, 76, 1, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFAB91); } }
public sealed class X2799Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 いか"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2799Parameter(s); }
public sealed class X2799Parameter : Q6Art { public X2799Parameter() : this(null) { } public X2799Parameter(SharedDataStore? s) : base(s, 76, 2, 0, 0) { Color1 = Hex(0xF5F1EA); Color2 = Hex(0xFFAB91); } }
public sealed class X2800Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 くじら"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2800Parameter(s); }
public sealed class X2800Parameter : Q6Art { public X2800Parameter() : this(null) { } public X2800Parameter(SharedDataStore? s) : base(s, 76, 3, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0xBFE6FF); } }
public sealed class X2801Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 かに"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2801Parameter(s); }
public sealed class X2801Parameter : Q6Art { public X2801Parameter() : this(null) { } public X2801Parameter(SharedDataStore? s) : base(s, 76, 4, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFAB91); } }
public sealed class X2802Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 えび"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2802Parameter(s); }
public sealed class X2802Parameter : Q6Art { public X2802Parameter() : this(null) { } public X2802Parameter(SharedDataStore? s) : base(s, 76, 5, 0, 0) { Color1 = Hex(0xFF7043); Color2 = Hex(0xFFAB91); } }
public sealed class X2803Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 くらげ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2803Parameter(s); }
public sealed class X2803Parameter : Q6Art { public X2803Parameter() : this(null) { } public X2803Parameter(SharedDataStore? s) : base(s, 76, 6, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0xE8488A); } }
public sealed class X2804Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 ひとで"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2804Parameter(s); }
public sealed class X2804Parameter : Q6Art { public X2804Parameter() : this(null) { } public X2804Parameter(SharedDataStore? s) : base(s, 76, 7, 0, 0) { Color1 = Hex(0xFF8A65); Color2 = Hex(0xFFCCBC); } }
public sealed class X2805Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 いるか"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2805Parameter(s); }
public sealed class X2805Parameter : Q6Art { public X2805Parameter() : this(null) { } public X2805Parameter(SharedDataStore? s) : base(s, 76, 8, 0, 0) { Color1 = Hex(0x5C9BD6); Color2 = Hex(0xDDEBF7); } }
public sealed class X2806Plugin : QuickShapePlugin { public override string Name => "演出：海の生き物 かめ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2806Parameter(s); }
public sealed class X2806Parameter : Q6Art { public X2806Parameter() : this(null) { } public X2806Parameter(SharedDataStore? s) : base(s, 76, 9, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0x8D6E63); } }
public sealed class X2807Plugin : QuickShapePlugin { public override string Name => "演出：鳥 はと"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2807Parameter(s); }
public sealed class X2807Parameter : Q6Art { public X2807Parameter() : this(null) { } public X2807Parameter(SharedDataStore? s) : base(s, 77, 0, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x7E8C99); } }
public sealed class X2808Plugin : QuickShapePlugin { public override string Name => "演出：鳥 からす"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2808Parameter(s); }
public sealed class X2808Parameter : Q6Art { public X2808Parameter() : this(null) { } public X2808Parameter(SharedDataStore? s) : base(s, 77, 1, 0, 0) { Color1 = Hex(0x263238); Color2 = Hex(0x37474F); } }
public sealed class X2809Plugin : QuickShapePlugin { public override string Name => "演出：鳥 あひる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2809Parameter(s); }
public sealed class X2809Parameter : Q6Art { public X2809Parameter() : this(null) { } public X2809Parameter(SharedDataStore? s) : base(s, 77, 2, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xF2A93B); } }
public sealed class X2810Plugin : QuickShapePlugin { public override string Name => "演出：鳥 つる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2810Parameter(s); }
public sealed class X2810Parameter : Q6Art { public X2810Parameter() : this(null) { } public X2810Parameter(SharedDataStore? s) : base(s, 77, 3, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2811Plugin : QuickShapePlugin { public override string Name => "演出：鳥 インコ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2811Parameter(s); }
public sealed class X2811Parameter : Q6Art { public X2811Parameter() : this(null) { } public X2811Parameter(SharedDataStore? s) : base(s, 77, 4, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xFFE14D); } }
public sealed class X2812Plugin : QuickShapePlugin { public override string Name => "演出：鳥 フラミンゴ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2812Parameter(s); }
public sealed class X2812Parameter : Q6Art { public X2812Parameter() : this(null) { } public X2812Parameter(SharedDataStore? s) : base(s, 77, 5, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0xF06292); } }
public sealed class X2813Plugin : QuickShapePlugin { public override string Name => "演出：鳥 はくちょう"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2813Parameter(s); }
public sealed class X2813Parameter : Q6Art { public X2813Parameter() : this(null) { } public X2813Parameter(SharedDataStore? s) : base(s, 77, 6, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xFF8F00); } }
public sealed class X2814Plugin : QuickShapePlugin { public override string Name => "演出：鳥 くじゃく"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2814Parameter(s); }
public sealed class X2814Parameter : Q6Art { public X2814Parameter() : this(null) { } public X2814Parameter(SharedDataStore? s) : base(s, 77, 7, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0x43A047); } }
public sealed class X2815Plugin : QuickShapePlugin { public override string Name => "演出：鳥 カワセミ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2815Parameter(s); }
public sealed class X2815Parameter : Q6Art { public X2815Parameter() : this(null) { } public X2815Parameter(SharedDataStore? s) : base(s, 77, 8, 0, 0) { Color1 = Hex(0x039BE5); Color2 = Hex(0xFF7043); } }
public sealed class X2816Plugin : QuickShapePlugin { public override string Name => "演出：鳥 きつつき"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2816Parameter(s); }
public sealed class X2816Parameter : Q6Art { public X2816Parameter() : this(null) { } public X2816Parameter(SharedDataStore? s) : base(s, 77, 9, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x263238); } }
public sealed class X2817Plugin : QuickShapePlugin { public override string Name => "演出：家電 ストーブ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2817Parameter(s); }
public sealed class X2817Parameter : Q6Art { public X2817Parameter() : this(null) { } public X2817Parameter(SharedDataStore? s) : base(s, 78, 0, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0xFF7043); } }
public sealed class X2818Plugin : QuickShapePlugin { public override string Name => "演出：家電 こたつ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2818Parameter(s); }
public sealed class X2818Parameter : Q6Art { public X2818Parameter() : this(null) { } public X2818Parameter(SharedDataStore? s) : base(s, 78, 1, 0, 0) { Color1 = Hex(0xB5651D); Color2 = Hex(0xE53935); } }
public sealed class X2819Plugin : QuickShapePlugin { public override string Name => "演出：家電 加湿器"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2819Parameter(s); }
public sealed class X2819Parameter : Q6Art { public X2819Parameter() : this(null) { } public X2819Parameter(SharedDataStore? s) : base(s, 78, 2, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x4FC3F7); } }
public sealed class X2820Plugin : QuickShapePlugin { public override string Name => "演出：家電 ホットプレート"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2820Parameter(s); }
public sealed class X2820Parameter : Q6Art { public X2820Parameter() : this(null) { } public X2820Parameter(SharedDataStore? s) : base(s, 78, 3, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xFFAB40); } }
public sealed class X2821Plugin : QuickShapePlugin { public override string Name => "演出：家電 リモコン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2821Parameter(s); }
public sealed class X2821Parameter : Q6Art { public X2821Parameter() : this(null) { } public X2821Parameter(SharedDataStore? s) : base(s, 78, 4, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x90A4AE); } }
public sealed class X2822Plugin : QuickShapePlugin { public override string Name => "演出：家電 電気ケトル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2822Parameter(s); }
public sealed class X2822Parameter : Q6Art { public X2822Parameter() : this(null) { } public X2822Parameter(SharedDataStore? s) : base(s, 78, 5, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x37474F); } }
public sealed class X2823Plugin : QuickShapePlugin { public override string Name => "演出：家電 ミシン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2823Parameter(s); }
public sealed class X2823Parameter : Q6Art { public X2823Parameter() : this(null) { } public X2823Parameter(SharedDataStore? s) : base(s, 78, 6, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0xE53935); } }
public sealed class X2824Plugin : QuickShapePlugin { public override string Name => "演出：家電 空気清浄機"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2824Parameter(s); }
public sealed class X2824Parameter : Q6Art { public X2824Parameter() : this(null) { } public X2824Parameter(SharedDataStore? s) : base(s, 78, 7, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x4DB6AC); } }
public sealed class X2825Plugin : QuickShapePlugin { public override string Name => "演出：家電 ハンディファン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2825Parameter(s); }
public sealed class X2825Parameter : Q6Art { public X2825Parameter() : this(null) { } public X2825Parameter(SharedDataStore? s) : base(s, 78, 8, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0xFFFFFF); } }
public sealed class X2826Plugin : QuickShapePlugin { public override string Name => "演出：家電 ロボット掃除機"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2826Parameter(s); }
public sealed class X2826Parameter : Q6Art { public X2826Parameter() : this(null) { } public X2826Parameter(SharedDataStore? s) : base(s, 78, 9, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x90A4AE); } }
public sealed class X2827Plugin : QuickShapePlugin { public override string Name => "演出：デバイス ノートパソコン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2827Parameter(s); }
public sealed class X2827Parameter : Q6Art { public X2827Parameter() : this(null) { } public X2827Parameter(SharedDataStore? s) : base(s, 79, 0, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x8FD3FF); } }
public sealed class X2828Plugin : QuickShapePlugin { public override string Name => "演出：デバイス デスクトップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2828Parameter(s); }
public sealed class X2828Parameter : Q6Art { public X2828Parameter() : this(null) { } public X2828Parameter(SharedDataStore? s) : base(s, 79, 1, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x8FD3FF); } }
public sealed class X2829Plugin : QuickShapePlugin { public override string Name => "演出：デバイス プリンター"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2829Parameter(s); }
public sealed class X2829Parameter : Q6Art { public X2829Parameter() : this(null) { } public X2829Parameter(SharedDataStore? s) : base(s, 79, 2, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x90A4AE); } }
public sealed class X2830Plugin : QuickShapePlugin { public override string Name => "演出：デバイス USBメモリ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2830Parameter(s); }
public sealed class X2830Parameter : Q6Art { public X2830Parameter() : this(null) { } public X2830Parameter(SharedDataStore? s) : base(s, 79, 3, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0xB0BEC5); } }
public sealed class X2831Plugin : QuickShapePlugin { public override string Name => "演出：デバイス スマートウォッチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2831Parameter(s); }
public sealed class X2831Parameter : Q6Art { public X2831Parameter() : this(null) { } public X2831Parameter(SharedDataStore? s) : base(s, 79, 4, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x4DB6AC); } }
public sealed class X2832Plugin : QuickShapePlugin { public override string Name => "演出：デバイス ルーター"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2832Parameter(s); }
public sealed class X2832Parameter : Q6Art { public X2832Parameter() : this(null) { } public X2832Parameter(SharedDataStore? s) : base(s, 79, 5, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x2F86C6); } }
public sealed class X2833Plugin : QuickShapePlugin { public override string Name => "演出：デバイス VRゴーグル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2833Parameter(s); }
public sealed class X2833Parameter : Q6Art { public X2833Parameter() : this(null) { } public X2833Parameter(SharedDataStore? s) : base(s, 79, 6, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x7E57C2); } }
public sealed class X2834Plugin : QuickShapePlugin { public override string Name => "演出：デバイス イヤホン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2834Parameter(s); }
public sealed class X2834Parameter : Q6Art { public X2834Parameter() : this(null) { } public X2834Parameter(SharedDataStore? s) : base(s, 79, 7, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x90A4AE); } }
public sealed class X2835Plugin : QuickShapePlugin { public override string Name => "演出：デバイス スピーカー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2835Parameter(s); }
public sealed class X2835Parameter : Q6Art { public X2835Parameter() : this(null) { } public X2835Parameter(SharedDataStore? s) : base(s, 79, 8, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x90A4AE); } }
public sealed class X2836Plugin : QuickShapePlugin { public override string Name => "演出：デバイス ドローン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2836Parameter(s); }
public sealed class X2836Parameter : Q6Art { public X2836Parameter() : this(null) { } public X2836Parameter(SharedDataStore? s) : base(s, 79, 9, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x90A4AE); } }
public sealed class X2837Plugin : QuickShapePlugin { public override string Name => "演出：家具 たんす"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2837Parameter(s); }
public sealed class X2837Parameter : Q6Art { public X2837Parameter() : this(null) { } public X2837Parameter(SharedDataStore? s) : base(s, 80, 0, 0, 0) { Color1 = Hex(0xB5651D); Color2 = Hex(0xF2C14E); } }
public sealed class X2838Plugin : QuickShapePlugin { public override string Name => "演出：家具 スタンドライト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2838Parameter(s); }
public sealed class X2838Parameter : Q6Art { public X2838Parameter() : this(null) { } public X2838Parameter(SharedDataStore? s) : base(s, 80, 1, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0x37474F); } }
public sealed class X2839Plugin : QuickShapePlugin { public override string Name => "演出：家具 カーテン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2839Parameter(s); }
public sealed class X2839Parameter : Q6Art { public X2839Parameter() : this(null) { } public X2839Parameter(SharedDataStore? s) : base(s, 80, 2, 0, 0) { Color1 = Hex(0xE8488A); Color2 = Hex(0xF2C14E); } }
public sealed class X2840Plugin : QuickShapePlugin { public override string Name => "演出：家具 ハンモック"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2840Parameter(s); }
public sealed class X2840Parameter : Q6Art { public X2840Parameter() : this(null) { } public X2840Parameter(SharedDataStore? s) : base(s, 80, 3, 0, 0) { Color1 = Hex(0x26A69A); Color2 = Hex(0x8D6E63); } }
public sealed class X2841Plugin : QuickShapePlugin { public override string Name => "演出：家具 座布団"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2841Parameter(s); }
public sealed class X2841Parameter : Q6Art { public X2841Parameter() : this(null) { } public X2841Parameter(SharedDataStore? s) : base(s, 80, 4, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0xF2C14E); } }
public sealed class X2842Plugin : QuickShapePlugin { public override string Name => "演出：家具 傘立て"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2842Parameter(s); }
public sealed class X2842Parameter : Q6Art { public X2842Parameter() : this(null) { } public X2842Parameter(SharedDataStore? s) : base(s, 80, 5, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x2F86C6); } }
public sealed class X2843Plugin : QuickShapePlugin { public override string Name => "演出：家具 ロッキングチェア"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2843Parameter(s); }
public sealed class X2843Parameter : Q6Art { public X2843Parameter() : this(null) { } public X2843Parameter(SharedDataStore? s) : base(s, 80, 6, 0, 0) { Color1 = Hex(0xB5651D); Color2 = Hex(0x8D5A3B); } }
public sealed class X2844Plugin : QuickShapePlugin { public override string Name => "演出：家具 本立て"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2844Parameter(s); }
public sealed class X2844Parameter : Q6Art { public X2844Parameter() : this(null) { } public X2844Parameter(SharedDataStore? s) : base(s, 80, 7, 0, 0) { Color1 = Hex(0x8D5A3B); Color2 = Hex(0xE53935); } }
public sealed class X2845Plugin : QuickShapePlugin { public override string Name => "演出：家具 ちゃぶ台"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2845Parameter(s); }
public sealed class X2845Parameter : Q6Art { public X2845Parameter() : this(null) { } public X2845Parameter(SharedDataStore? s) : base(s, 80, 8, 0, 0) { Color1 = Hex(0xB5651D); Color2 = Hex(0x8D5A3B); } }
public sealed class X2846Plugin : QuickShapePlugin { public override string Name => "演出：家具 二段ベッド"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2846Parameter(s); }
public sealed class X2846Parameter : Q6Art { public X2846Parameter() : this(null) { } public X2846Parameter(SharedDataStore? s) : base(s, 80, 9, 0, 0) { Color1 = Hex(0xB5651D); Color2 = Hex(0x8FD3FF); } }
public sealed class X2847Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン グー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2847Parameter(s); }
public sealed class X2847Parameter : Q6Art { public X2847Parameter() : this(null) { } public X2847Parameter(SharedDataStore? s) : base(s, 81, 0, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0xD9A066); } }
public sealed class X2848Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン チョキ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2848Parameter(s); }
public sealed class X2848Parameter : Q6Art { public X2848Parameter() : this(null) { } public X2848Parameter(SharedDataStore? s) : base(s, 81, 1, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0xD9A066); } }
public sealed class X2849Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン パー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2849Parameter(s); }
public sealed class X2849Parameter : Q6Art { public X2849Parameter() : this(null) { } public X2849Parameter(SharedDataStore? s) : base(s, 81, 2, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0xD9A066); } }
public sealed class X2850Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン いいね"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2850Parameter(s); }
public sealed class X2850Parameter : Q6Art { public X2850Parameter() : this(null) { } public X2850Parameter(SharedDataStore? s) : base(s, 81, 3, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0x2F86C6); } }
public sealed class X2851Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン ゆびさし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2851Parameter(s); }
public sealed class X2851Parameter : Q6Art { public X2851Parameter() : this(null) { } public X2851Parameter(SharedDataStore? s) : base(s, 81, 4, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0xD9A066); } }
public sealed class X2852Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン OK"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2852Parameter(s); }
public sealed class X2852Parameter : Q6Art { public X2852Parameter() : this(null) { } public X2852Parameter(SharedDataStore? s) : base(s, 81, 5, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0xD9A066); } }
public sealed class X2853Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン ピース"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2853Parameter(s); }
public sealed class X2853Parameter : Q6Art { public X2853Parameter() : this(null) { } public X2853Parameter(SharedDataStore? s) : base(s, 81, 6, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0x8FD3FF); } }
public sealed class X2854Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン 手をふる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2854Parameter(s); }
public sealed class X2854Parameter : Q6Art { public X2854Parameter() : this(null) { } public X2854Parameter(SharedDataStore? s) : base(s, 81, 7, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0xD9A066); } }
public sealed class X2855Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン ハート"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2855Parameter(s); }
public sealed class X2855Parameter : Q6Art { public X2855Parameter() : this(null) { } public X2855Parameter(SharedDataStore? s) : base(s, 81, 8, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0xE8488A); } }
public sealed class X2856Plugin : QuickShapePlugin { public override string Name => "演出：手のサイン 拍手"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2856Parameter(s); }
public sealed class X2856Parameter : Q6Art { public X2856Parameter() : this(null) { } public X2856Parameter(SharedDataStore? s) : base(s, 81, 9, 0, 0) { Color1 = Hex(0xF2C9A0); Color2 = Hex(0xF2C14E); } }
public sealed class X2857Plugin : QuickShapePlugin { public override string Name => "演出：植物 もみの木"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2857Parameter(s); }
public sealed class X2857Parameter : Q6Art { public X2857Parameter() : this(null) { } public X2857Parameter(SharedDataStore? s) : base(s, 82, 0, 0, 0) { Color1 = Hex(0x2E7D32); Color2 = Hex(0x8D5A3B); } }
public sealed class X2858Plugin : QuickShapePlugin { public override string Name => "演出：植物 サボテン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2858Parameter(s); }
public sealed class X2858Parameter : Q6Art { public X2858Parameter() : this(null) { } public X2858Parameter(SharedDataStore? s) : base(s, 82, 1, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xE8488A); } }
public sealed class X2859Plugin : QuickShapePlugin { public override string Name => "演出：植物 ヤシの木"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2859Parameter(s); }
public sealed class X2859Parameter : Q6Art { public X2859Parameter() : this(null) { } public X2859Parameter(SharedDataStore? s) : base(s, 82, 2, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0x8D5A3B); } }
public sealed class X2860Plugin : QuickShapePlugin { public override string Name => "演出：植物 鉢植え"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2860Parameter(s); }
public sealed class X2860Parameter : Q6Art { public X2860Parameter() : this(null) { } public X2860Parameter(SharedDataStore? s) : base(s, 82, 3, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xD9822B); } }
public sealed class X2861Plugin : QuickShapePlugin { public override string Name => "演出：植物 つくし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2861Parameter(s); }
public sealed class X2861Parameter : Q6Art { public X2861Parameter() : this(null) { } public X2861Parameter(SharedDataStore? s) : base(s, 82, 4, 0, 0) { Color1 = Hex(0xC8A27A); Color2 = Hex(0x8D6E63); } }
public sealed class X2862Plugin : QuickShapePlugin { public override string Name => "演出：植物 盆栽"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2862Parameter(s); }
public sealed class X2862Parameter : Q6Art { public X2862Parameter() : this(null) { } public X2862Parameter(SharedDataStore? s) : base(s, 82, 5, 0, 0) { Color1 = Hex(0x2E7D32); Color2 = Hex(0x5D4037); } }
public sealed class X2863Plugin : QuickShapePlugin { public override string Name => "演出：植物 松ぼっくり"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2863Parameter(s); }
public sealed class X2863Parameter : Q6Art { public X2863Parameter() : this(null) { } public X2863Parameter(SharedDataStore? s) : base(s, 82, 6, 0, 0) { Color1 = Hex(0x8D5A3B); Color2 = Hex(0x5D4037); } }
public sealed class X2864Plugin : QuickShapePlugin { public override string Name => "演出：植物 多肉植物"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2864Parameter(s); }
public sealed class X2864Parameter : Q6Art { public X2864Parameter() : this(null) { } public X2864Parameter(SharedDataStore? s) : base(s, 82, 7, 0, 0) { Color1 = Hex(0x81C784); Color2 = Hex(0xD9822B); } }
public sealed class X2865Plugin : QuickShapePlugin { public override string Name => "演出：植物 ねこじゃらし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2865Parameter(s); }
public sealed class X2865Parameter : Q6Art { public X2865Parameter() : this(null) { } public X2865Parameter(SharedDataStore? s) : base(s, 82, 8, 0, 0) { Color1 = Hex(0x9CCC65); Color2 = Hex(0x7CB342); } }
public sealed class X2866Plugin : QuickShapePlugin { public override string Name => "演出：植物 しだ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2866Parameter(s); }
public sealed class X2866Parameter : Q6Art { public X2866Parameter() : this(null) { } public X2866Parameter(SharedDataStore? s) : base(s, 82, 9, 0, 0) { Color1 = Hex(0x388E3C); Color2 = Hex(0x2E7D32); } }
public sealed class X2867Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 彗星"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2867Parameter(s); }
public sealed class X2867Parameter : Q6Art { public X2867Parameter() : this(null) { } public X2867Parameter(SharedDataStore? s) : base(s, 83, 0, 0, 0) { Color1 = Hex(0x8FD3FF); Color2 = Hex(0xE3F2FD); } }
public sealed class X2868Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 宇宙飛行士"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2868Parameter(s); }
public sealed class X2868Parameter : Q6Art { public X2868Parameter() : this(null) { } public X2868Parameter(SharedDataStore? s) : base(s, 83, 1, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x37474F); } }
public sealed class X2869Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 望遠鏡"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2869Parameter(s); }
public sealed class X2869Parameter : Q6Art { public X2869Parameter() : this(null) { } public X2869Parameter(SharedDataStore? s) : base(s, 83, 2, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x37474F); } }
public sealed class X2870Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 宇宙人"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2870Parameter(s); }
public sealed class X2870Parameter : Q6Art { public X2870Parameter() : this(null) { } public X2870Parameter(SharedDataStore? s) : base(s, 83, 3, 0, 0) { Color1 = Hex(0x81C784); Color2 = Hex(0x263238); } }
public sealed class X2871Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 天文台"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2871Parameter(s); }
public sealed class X2871Parameter : Q6Art { public X2871Parameter() : this(null) { } public X2871Parameter(SharedDataStore? s) : base(s, 83, 4, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x37474F); } }
public sealed class X2872Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 着陸船"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2872Parameter(s); }
public sealed class X2872Parameter : Q6Art { public X2872Parameter() : this(null) { } public X2872Parameter(SharedDataStore? s) : base(s, 83, 5, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xB0BEC5); } }
public sealed class X2873Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 探査車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2873Parameter(s); }
public sealed class X2873Parameter : Q6Art { public X2873Parameter() : this(null) { } public X2873Parameter(SharedDataStore? s) : base(s, 83, 6, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x37474F); } }
public sealed class X2874Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 宇宙食"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2874Parameter(s); }
public sealed class X2874Parameter : Q6Art { public X2874Parameter() : this(null) { } public X2874Parameter(SharedDataStore? s) : base(s, 83, 7, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0xE53935); } }
public sealed class X2875Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 発射台"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2875Parameter(s); }
public sealed class X2875Parameter : Q6Art { public X2875Parameter() : this(null) { } public X2875Parameter(SharedDataStore? s) : base(s, 83, 8, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0xE53935); } }
public sealed class X2876Plugin : QuickShapePlugin { public override string Name => "演出：宇宙 星座早見盤"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2876Parameter(s); }
public sealed class X2876Parameter : Q6Art { public X2876Parameter() : this(null) { } public X2876Parameter(SharedDataStore? s) : base(s, 83, 9, 0, 0) { Color1 = Hex(0x1A237E); Color2 = Hex(0xFFE14D); } }
public sealed class X2877Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 バット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2877Parameter(s); }
public sealed class X2877Parameter : Q6Art { public X2877Parameter() : this(null) { } public X2877Parameter(SharedDataStore? s) : base(s, 84, 0, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0x37474F); } }
public sealed class X2878Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 グローブ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2878Parameter(s); }
public sealed class X2878Parameter : Q6Art { public X2878Parameter() : this(null) { } public X2878Parameter(SharedDataStore? s) : base(s, 84, 1, 0, 0) { Color1 = Hex(0xB5651D); Color2 = Hex(0x6D4C41); } }
public sealed class X2879Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 卓球ラケット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2879Parameter(s); }
public sealed class X2879Parameter : Q6Art { public X2879Parameter() : this(null) { } public X2879Parameter(SharedDataStore? s) : base(s, 84, 2, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x8D5A3B); } }
public sealed class X2880Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 ゴルフクラブ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2880Parameter(s); }
public sealed class X2880Parameter : Q6Art { public X2880Parameter() : this(null) { } public X2880Parameter(SharedDataStore? s) : base(s, 84, 3, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x37474F); } }
public sealed class X2881Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 なわとび"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2881Parameter(s); }
public sealed class X2881Parameter : Q6Art { public X2881Parameter() : this(null) { } public X2881Parameter(SharedDataStore? s) : base(s, 84, 4, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xF2C14E); } }
public sealed class X2882Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 スケートボード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2882Parameter(s); }
public sealed class X2882Parameter : Q6Art { public X2882Parameter() : this(null) { } public X2882Parameter(SharedDataStore? s) : base(s, 84, 5, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x37474F); } }
public sealed class X2883Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 シャトル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2883Parameter(s); }
public sealed class X2883Parameter : Q6Art { public X2883Parameter() : this(null) { } public X2883Parameter(SharedDataStore? s) : base(s, 84, 6, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2884Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 跳び箱"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2884Parameter(s); }
public sealed class X2884Parameter : Q6Art { public X2884Parameter() : this(null) { } public X2884Parameter(SharedDataStore? s) : base(s, 84, 7, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xFFFFFF); } }
public sealed class X2885Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 スキー板"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2885Parameter(s); }
public sealed class X2885Parameter : Q6Art { public X2885Parameter() : this(null) { } public X2885Parameter(SharedDataStore? s) : base(s, 84, 8, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x2F86C6); } }
public sealed class X2886Plugin : QuickShapePlugin { public override string Name => "演出：スポーツ用品 ボクシンググローブ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2886Parameter(s); }
public sealed class X2886Parameter : Q6Art { public X2886Parameter() : this(null) { } public X2886Parameter(SharedDataStore? s) : base(s, 84, 9, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
