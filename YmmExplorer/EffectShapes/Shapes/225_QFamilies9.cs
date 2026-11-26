using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// No. 3001〜3200(gen_v146.py で生成。部品の並びで描くアイコン)

public sealed class X3001Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 柏餅"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3001Parameter(s); }
public sealed class X3001Parameter : Q6Art { public X3001Parameter() : this(null) { } public X3001Parameter(SharedDataStore? s) : base(s, 97, 0, 0, 0) { Color1 = Hex(0xF5F0E6); Color2 = Hex(0x4E8B3A); } }
public sealed class X3002Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 桜餅"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3002Parameter(s); }
public sealed class X3002Parameter : Q6Art { public X3002Parameter() : this(null) { } public X3002Parameter(SharedDataStore? s) : base(s, 97, 1, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0x6E9E4A); } }
public sealed class X3003Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 どら焼き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3003Parameter(s); }
public sealed class X3003Parameter : Q6Art { public X3003Parameter() : this(null) { } public X3003Parameter(SharedDataStore? s) : base(s, 97, 2, 0, 0) { Color1 = Hex(0xC27A3A); Color2 = Hex(0x5D2E1A); } }
public sealed class X3004Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 ようかん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3004Parameter(s); }
public sealed class X3004Parameter : Q6Art { public X3004Parameter() : this(null) { } public X3004Parameter(SharedDataStore? s) : base(s, 97, 3, 0, 0) { Color1 = Hex(0x6B2E26); Color2 = Hex(0xE0E0E0); } }
public sealed class X3005Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 おはぎ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3005Parameter(s); }
public sealed class X3005Parameter : Q6Art { public X3005Parameter() : this(null) { } public X3005Parameter(SharedDataStore? s) : base(s, 97, 4, 0, 0) { Color1 = Hex(0x5D2E1A); Color2 = Hex(0xFFFFFF); } }
public sealed class X3006Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 最中"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3006Parameter(s); }
public sealed class X3006Parameter : Q6Art { public X3006Parameter() : this(null) { } public X3006Parameter(SharedDataStore? s) : base(s, 97, 5, 0, 0) { Color1 = Hex(0xE8C48A); Color2 = Hex(0x5D2E1A); } }
public sealed class X3007Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 金平糖"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3007Parameter(s); }
public sealed class X3007Parameter : Q6Art { public X3007Parameter() : this(null) { } public X3007Parameter(SharedDataStore? s) : base(s, 97, 6, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0xB3E5FC); } }
public sealed class X3008Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 わらび餅"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3008Parameter(s); }
public sealed class X3008Parameter : Q6Art { public X3008Parameter() : this(null) { } public X3008Parameter(SharedDataStore? s) : base(s, 97, 7, 0, 0) { Color1 = Hex(0xD8C9A8); Color2 = Hex(0x8D6E63); } }
public sealed class X3009Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 草もち"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3009Parameter(s); }
public sealed class X3009Parameter : Q6Art { public X3009Parameter() : this(null) { } public X3009Parameter(SharedDataStore? s) : base(s, 97, 8, 0, 0) { Color1 = Hex(0x7CB342); Color2 = Hex(0x5D2E1A); } }
public sealed class X3010Plugin : QuickShapePlugin { public override string Name => "演出：和菓子 栗きんとん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3010Parameter(s); }
public sealed class X3010Parameter : Q6Art { public X3010Parameter() : this(null) { } public X3010Parameter(SharedDataStore? s) : base(s, 97, 9, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0x8D6E63); } }
public sealed class X3011Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 モンブラン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3011Parameter(s); }
public sealed class X3011Parameter : Q6Art { public X3011Parameter() : this(null) { } public X3011Parameter(SharedDataStore? s) : base(s, 98, 0, 0, 0) { Color1 = Hex(0xC9A06A); Color2 = Hex(0xFFFFFF); } }
public sealed class X3012Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 シュークリーム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3012Parameter(s); }
public sealed class X3012Parameter : Q6Art { public X3012Parameter() : this(null) { } public X3012Parameter(SharedDataStore? s) : base(s, 98, 1, 0, 0) { Color1 = Hex(0xE0A85A); Color2 = Hex(0xFFF8E1); } }
public sealed class X3013Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 エクレア"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3013Parameter(s); }
public sealed class X3013Parameter : Q6Art { public X3013Parameter() : this(null) { } public X3013Parameter(SharedDataStore? s) : base(s, 98, 2, 0, 0) { Color1 = Hex(0xE0A85A); Color2 = Hex(0x4E2A1E); } }
public sealed class X3014Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 いちごタルト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3014Parameter(s); }
public sealed class X3014Parameter : Q6Art { public X3014Parameter() : this(null) { } public X3014Parameter(SharedDataStore? s) : base(s, 98, 3, 0, 0) { Color1 = Hex(0xF0C27A); Color2 = Hex(0xE53935); } }
public sealed class X3015Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 ロールケーキ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3015Parameter(s); }
public sealed class X3015Parameter : Q6Art { public X3015Parameter() : this(null) { } public X3015Parameter(SharedDataStore? s) : base(s, 98, 4, 0, 0) { Color1 = Hex(0xF3D7A6); Color2 = Hex(0xFFFFFF); } }
public sealed class X3016Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 パンケーキ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3016Parameter(s); }
public sealed class X3016Parameter : Q6Art { public X3016Parameter() : this(null) { } public X3016Parameter(SharedDataStore? s) : base(s, 98, 5, 0, 0) { Color1 = Hex(0xE8A954); Color2 = Hex(0xFFF59D); } }
public sealed class X3017Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 チーズケーキ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3017Parameter(s); }
public sealed class X3017Parameter : Q6Art { public X3017Parameter() : this(null) { } public X3017Parameter(SharedDataStore? s) : base(s, 98, 6, 0, 0) { Color1 = Hex(0xFFE082); Color2 = Hex(0xC27A3A); } }
public sealed class X3018Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 ミルフィーユ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3018Parameter(s); }
public sealed class X3018Parameter : Q6Art { public X3018Parameter() : this(null) { } public X3018Parameter(SharedDataStore? s) : base(s, 98, 7, 0, 0) { Color1 = Hex(0xF3D7A6); Color2 = Hex(0xFFF8E1); } }
public sealed class X3019Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 マフィン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3019Parameter(s); }
public sealed class X3019Parameter : Q6Art { public X3019Parameter() : this(null) { } public X3019Parameter(SharedDataStore? s) : base(s, 98, 8, 0, 0) { Color1 = Hex(0xE8C48A); Color2 = Hex(0x5C6BC0); } }
public sealed class X3020Plugin : QuickShapePlugin { public override string Name => "演出：洋菓子 パフェ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3020Parameter(s); }
public sealed class X3020Parameter : Q6Art { public X3020Parameter() : this(null) { } public X3020Parameter(SharedDataStore? s) : base(s, 98, 9, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X3021Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 小籠包"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3021Parameter(s); }
public sealed class X3021Parameter : Q6Art { public X3021Parameter() : this(null) { } public X3021Parameter(SharedDataStore? s) : base(s, 99, 0, 0, 0) { Color1 = Hex(0xF5F0E6); Color2 = Hex(0x6D4C41); } }
public sealed class X3022Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 春巻き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3022Parameter(s); }
public sealed class X3022Parameter : Q6Art { public X3022Parameter() : this(null) { } public X3022Parameter(SharedDataStore? s) : base(s, 99, 1, 0, 0) { Color1 = Hex(0xE0A85A); Color2 = Hex(0x7CB342); } }
public sealed class X3023Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 麻婆豆腐"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3023Parameter(s); }
public sealed class X3023Parameter : Q6Art { public X3023Parameter() : this(null) { } public X3023Parameter(SharedDataStore? s) : base(s, 99, 2, 0, 0) { Color1 = Hex(0xD84315); Color2 = Hex(0xFFF8E1); } }
public sealed class X3024Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 タコス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3024Parameter(s); }
public sealed class X3024Parameter : Q6Art { public X3024Parameter() : this(null) { } public X3024Parameter(SharedDataStore? s) : base(s, 99, 3, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0x7CB342); } }
public sealed class X3025Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 パエリア"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3025Parameter(s); }
public sealed class X3025Parameter : Q6Art { public X3025Parameter() : this(null) { } public X3025Parameter(SharedDataStore? s) : base(s, 99, 4, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0x37474F); } }
public sealed class X3026Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 グラタン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3026Parameter(s); }
public sealed class X3026Parameter : Q6Art { public X3026Parameter() : this(null) { } public X3026Parameter(SharedDataStore? s) : base(s, 99, 5, 0, 0) { Color1 = Hex(0xF5D58A); Color2 = Hex(0xFFFFFF); } }
public sealed class X3027Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 ラザニア"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3027Parameter(s); }
public sealed class X3027Parameter : Q6Art { public X3027Parameter() : this(null) { } public X3027Parameter(SharedDataStore? s) : base(s, 99, 6, 0, 0) { Color1 = Hex(0xE8C48A); Color2 = Hex(0xD84315); } }
public sealed class X3028Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 ステーキ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3028Parameter(s); }
public sealed class X3028Parameter : Q6Art { public X3028Parameter() : this(null) { } public X3028Parameter(SharedDataStore? s) : base(s, 99, 7, 0, 0) { Color1 = Hex(0x8D4A2A); Color2 = Hex(0x37474F); } }
public sealed class X3029Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 ビビンバ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3029Parameter(s); }
public sealed class X3029Parameter : Q6Art { public X3029Parameter() : this(null) { } public X3029Parameter(SharedDataStore? s) : base(s, 99, 8, 0, 0) { Color1 = Hex(0x5D4037); Color2 = Hex(0xFFB300); } }
public sealed class X3030Plugin : QuickShapePlugin { public override string Name => "演出：世界の料理 ボルシチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3030Parameter(s); }
public sealed class X3030Parameter : Q6Art { public X3030Parameter() : this(null) { } public X3030Parameter(SharedDataStore? s) : base(s, 99, 9, 0, 0) { Color1 = Hex(0xC62828); Color2 = Hex(0xFFFFFF); } }
public sealed class X3031Plugin : QuickShapePlugin { public override string Name => "演出：和食 天ぷら"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3031Parameter(s); }
public sealed class X3031Parameter : Q6Art { public X3031Parameter() : this(null) { } public X3031Parameter(SharedDataStore? s) : base(s, 100, 0, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xFF7043); } }
public sealed class X3032Plugin : QuickShapePlugin { public override string Name => "演出：和食 焼き魚"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3032Parameter(s); }
public sealed class X3032Parameter : Q6Art { public X3032Parameter() : this(null) { } public X3032Parameter(SharedDataStore? s) : base(s, 100, 1, 0, 0) { Color1 = Hex(0xC8A27A); Color2 = Hex(0xFFFFFF); } }
public sealed class X3033Plugin : QuickShapePlugin { public override string Name => "演出：和食 茶碗蒸し"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3033Parameter(s); }
public sealed class X3033Parameter : Q6Art { public X3033Parameter() : this(null) { } public X3033Parameter(SharedDataStore? s) : base(s, 100, 2, 0, 0) { Color1 = Hex(0xFFF59D); Color2 = Hex(0x4E8B3A); } }
public sealed class X3034Plugin : QuickShapePlugin { public override string Name => "演出：和食 納豆"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3034Parameter(s); }
public sealed class X3034Parameter : Q6Art { public X3034Parameter() : this(null) { } public X3034Parameter(SharedDataStore? s) : base(s, 100, 3, 0, 0) { Color1 = Hex(0xC8A27A); Color2 = Hex(0xFFFFFF); } }
public sealed class X3035Plugin : QuickShapePlugin { public override string Name => "演出：和食 卵焼き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3035Parameter(s); }
public sealed class X3035Parameter : Q6Art { public X3035Parameter() : this(null) { } public X3035Parameter(SharedDataStore? s) : base(s, 100, 4, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0xF5F5F5); } }
public sealed class X3036Plugin : QuickShapePlugin { public override string Name => "演出：和食 ざるそば"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3036Parameter(s); }
public sealed class X3036Parameter : Q6Art { public X3036Parameter() : this(null) { } public X3036Parameter(SharedDataStore? s) : base(s, 100, 5, 0, 0) { Color1 = Hex(0x8D7B6A); Color2 = Hex(0x6D4C41); } }
public sealed class X3037Plugin : QuickShapePlugin { public override string Name => "演出：和食 牛丼"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3037Parameter(s); }
public sealed class X3037Parameter : Q6Art { public X3037Parameter() : this(null) { } public X3037Parameter(SharedDataStore? s) : base(s, 100, 6, 0, 0) { Color1 = Hex(0x8D4A2A); Color2 = Hex(0xD84315); } }
public sealed class X3038Plugin : QuickShapePlugin { public override string Name => "演出：和食 冷奴"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3038Parameter(s); }
public sealed class X3038Parameter : Q6Art { public X3038Parameter() : this(null) { } public X3038Parameter(SharedDataStore? s) : base(s, 100, 7, 0, 0) { Color1 = Hex(0xFFFDE7); Color2 = Hex(0x5C6BC0); } }
public sealed class X3039Plugin : QuickShapePlugin { public override string Name => "演出：和食 肉じゃが"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3039Parameter(s); }
public sealed class X3039Parameter : Q6Art { public X3039Parameter() : this(null) { } public X3039Parameter(SharedDataStore? s) : base(s, 100, 8, 0, 0) { Color1 = Hex(0xD9A55A); Color2 = Hex(0xE0E0E0); } }
public sealed class X3040Plugin : QuickShapePlugin { public override string Name => "演出：和食 すき焼き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3040Parameter(s); }
public sealed class X3040Parameter : Q6Art { public X3040Parameter() : this(null) { } public X3040Parameter(SharedDataStore? s) : base(s, 100, 9, 0, 0) { Color1 = Hex(0xB5543A); Color2 = Hex(0xFFFFFF); } }
public sealed class X3041Plugin : QuickShapePlugin { public override string Name => "演出：調味料 しょうゆ差し"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3041Parameter(s); }
public sealed class X3041Parameter : Q6Art { public X3041Parameter() : this(null) { } public X3041Parameter(SharedDataStore? s) : base(s, 101, 0, 0, 0) { Color1 = Hex(0x3E2723); Color2 = Hex(0xE53935); } }
public sealed class X3042Plugin : QuickShapePlugin { public override string Name => "演出：調味料 食塩"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3042Parameter(s); }
public sealed class X3042Parameter : Q6Art { public X3042Parameter() : this(null) { } public X3042Parameter(SharedDataStore? s) : base(s, 101, 1, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x90A4AE); } }
public sealed class X3043Plugin : QuickShapePlugin { public override string Name => "演出：調味料 角砂糖"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3043Parameter(s); }
public sealed class X3043Parameter : Q6Art { public X3043Parameter() : this(null) { } public X3043Parameter(SharedDataStore? s) : base(s, 101, 2, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE0E0E0); } }
public sealed class X3044Plugin : QuickShapePlugin { public override string Name => "演出：調味料 ケチャップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3044Parameter(s); }
public sealed class X3044Parameter : Q6Art { public X3044Parameter() : this(null) { } public X3044Parameter(SharedDataStore? s) : base(s, 101, 3, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X3045Plugin : QuickShapePlugin { public override string Name => "演出：調味料 マヨネーズ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3045Parameter(s); }
public sealed class X3045Parameter : Q6Art { public X3045Parameter() : this(null) { } public X3045Parameter(SharedDataStore? s) : base(s, 101, 4, 0, 0) { Color1 = Hex(0xFFF8E1); Color2 = Hex(0xE53935); } }
public sealed class X3046Plugin : QuickShapePlugin { public override string Name => "演出：調味料 みそ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3046Parameter(s); }
public sealed class X3046Parameter : Q6Art { public X3046Parameter() : this(null) { } public X3046Parameter(SharedDataStore? s) : base(s, 101, 5, 0, 0) { Color1 = Hex(0xB5843A); Color2 = Hex(0xFFFFFF); } }
public sealed class X3047Plugin : QuickShapePlugin { public override string Name => "演出：調味料 お酢"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3047Parameter(s); }
public sealed class X3047Parameter : Q6Art { public X3047Parameter() : this(null) { } public X3047Parameter(SharedDataStore? s) : base(s, 101, 6, 0, 0) { Color1 = Hex(0xFFF8E1); Color2 = Hex(0x1565C0); } }
public sealed class X3048Plugin : QuickShapePlugin { public override string Name => "演出：調味料 こしょう"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3048Parameter(s); }
public sealed class X3048Parameter : Q6Art { public X3048Parameter() : this(null) { } public X3048Parameter(SharedDataStore? s) : base(s, 101, 7, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0x37474F); } }
public sealed class X3049Plugin : QuickShapePlugin { public override string Name => "演出：調味料 わさび"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3049Parameter(s); }
public sealed class X3049Parameter : Q6Art { public X3049Parameter() : this(null) { } public X3049Parameter(SharedDataStore? s) : base(s, 101, 8, 0, 0) { Color1 = Hex(0x8BC34A); Color2 = Hex(0x2E7D32); } }
public sealed class X3050Plugin : QuickShapePlugin { public override string Name => "演出：調味料 からし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3050Parameter(s); }
public sealed class X3050Parameter : Q6Art { public X3050Parameter() : this(null) { } public X3050Parameter(SharedDataStore? s) : base(s, 101, 9, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0xF9A825); } }
public sealed class X3051Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 メダカ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3051Parameter(s); }
public sealed class X3051Parameter : Q6Art { public X3051Parameter() : this(null) { } public X3051Parameter(SharedDataStore? s) : base(s, 102, 0, 0, 0) { Color1 = Hex(0xF4D35E); Color2 = Hex(0xFFB74D); } }
public sealed class X3052Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 ザリガニ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3052Parameter(s); }
public sealed class X3052Parameter : Q6Art { public X3052Parameter() : this(null) { } public X3052Parameter(SharedDataStore? s) : base(s, 102, 1, 0, 0) { Color1 = Hex(0xD84315); Color2 = Hex(0xFF8A65); } }
public sealed class X3053Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 オタマジャクシ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3053Parameter(s); }
public sealed class X3053Parameter : Q6Art { public X3053Parameter() : this(null) { } public X3053Parameter(SharedDataStore? s) : base(s, 102, 2, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x78909C); } }
public sealed class X3054Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 ナマズ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3054Parameter(s); }
public sealed class X3054Parameter : Q6Art { public X3054Parameter() : this(null) { } public X3054Parameter(SharedDataStore? s) : base(s, 102, 3, 0, 0) { Color1 = Hex(0x5D4037); Color2 = Hex(0xBCAAA4); } }
public sealed class X3055Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 錦鯉"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3055Parameter(s); }
public sealed class X3055Parameter : Q6Art { public X3055Parameter() : this(null) { } public X3055Parameter(SharedDataStore? s) : base(s, 102, 4, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X3056Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 サワガニ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3056Parameter(s); }
public sealed class X3056Parameter : Q6Art { public X3056Parameter() : this(null) { } public X3056Parameter(SharedDataStore? s) : base(s, 102, 5, 0, 0) { Color1 = Hex(0xD84315); Color2 = Hex(0xFFCC80); } }
public sealed class X3057Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 ドジョウ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3057Parameter(s); }
public sealed class X3057Parameter : Q6Art { public X3057Parameter() : this(null) { } public X3057Parameter(SharedDataStore? s) : base(s, 102, 6, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0x6D4C41); } }
public sealed class X3058Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 アメンボ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3058Parameter(s); }
public sealed class X3058Parameter : Q6Art { public X3058Parameter() : this(null) { } public X3058Parameter(SharedDataStore? s) : base(s, 102, 7, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x90CAF9); } }
public sealed class X3059Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 タガメ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3059Parameter(s); }
public sealed class X3059Parameter : Q6Art { public X3059Parameter() : this(null) { } public X3059Parameter(SharedDataStore? s) : base(s, 102, 8, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0x8D6E63); } }
public sealed class X3060Plugin : QuickShapePlugin { public override string Name => "演出：川の生き物 ウーパールーパー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3060Parameter(s); }
public sealed class X3060Parameter : Q6Art { public X3060Parameter() : this(null) { } public X3060Parameter(SharedDataStore? s) : base(s, 102, 9, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0xE91E63); } }
public sealed class X3061Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 ティラノサウルス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3061Parameter(s); }
public sealed class X3061Parameter : Q6Art { public X3061Parameter() : this(null) { } public X3061Parameter(SharedDataStore? s) : base(s, 103, 0, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xC5E1A5); } }
public sealed class X3062Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 トリケラトプス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3062Parameter(s); }
public sealed class X3062Parameter : Q6Art { public X3062Parameter() : this(null) { } public X3062Parameter(SharedDataStore? s) : base(s, 103, 1, 0, 0) { Color1 = Hex(0xFF8A65); Color2 = Hex(0xFFE0B2); } }
public sealed class X3063Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 ステゴサウルス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3063Parameter(s); }
public sealed class X3063Parameter : Q6Art { public X3063Parameter() : this(null) { } public X3063Parameter(SharedDataStore? s) : base(s, 103, 2, 0, 0) { Color1 = Hex(0x9CCC65); Color2 = Hex(0xFF7043); } }
public sealed class X3064Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 ブラキオサウルス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3064Parameter(s); }
public sealed class X3064Parameter : Q6Art { public X3064Parameter() : this(null) { } public X3064Parameter(SharedDataStore? s) : base(s, 103, 3, 0, 0) { Color1 = Hex(0x4DB6AC); Color2 = Hex(0xB2DFDB); } }
public sealed class X3065Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 プテラノドン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3065Parameter(s); }
public sealed class X3065Parameter : Q6Art { public X3065Parameter() : this(null) { } public X3065Parameter(SharedDataStore? s) : base(s, 103, 4, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0xBCAAA4); } }
public sealed class X3066Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 アンキロサウルス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3066Parameter(s); }
public sealed class X3066Parameter : Q6Art { public X3066Parameter() : this(null) { } public X3066Parameter(SharedDataStore? s) : base(s, 103, 5, 0, 0) { Color1 = Hex(0xA1887F); Color2 = Hex(0x5D4037); } }
public sealed class X3067Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 ヴェロキラプトル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3067Parameter(s); }
public sealed class X3067Parameter : Q6Art { public X3067Parameter() : this(null) { } public X3067Parameter(SharedDataStore? s) : base(s, 103, 6, 0, 0) { Color1 = Hex(0xFFB74D); Color2 = Hex(0x8D6E63); } }
public sealed class X3068Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 スピノサウルス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3068Parameter(s); }
public sealed class X3068Parameter : Q6Art { public X3068Parameter() : this(null) { } public X3068Parameter(SharedDataStore? s) : base(s, 103, 7, 0, 0) { Color1 = Hex(0x5C6BC0); Color2 = Hex(0xFF8A65); } }
public sealed class X3069Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 恐竜のたまご"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3069Parameter(s); }
public sealed class X3069Parameter : Q6Art { public X3069Parameter() : this(null) { } public X3069Parameter(SharedDataStore? s) : base(s, 103, 8, 0, 0) { Color1 = Hex(0xF5F0E6); Color2 = Hex(0x7CB342); } }
public sealed class X3070Plugin : QuickShapePlugin { public override string Name => "演出：恐竜 アンモナイト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3070Parameter(s); }
public sealed class X3070Parameter : Q6Art { public X3070Parameter() : this(null) { } public X3070Parameter(SharedDataStore? s) : base(s, 103, 9, 0, 0) { Color1 = Hex(0xC8A27A); Color2 = Hex(0x8D6E63); } }
public sealed class X3071Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー ユニコーン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3071Parameter(s); }
public sealed class X3071Parameter : Q6Art { public X3071Parameter() : this(null) { } public X3071Parameter(SharedDataStore? s) : base(s, 104, 0, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xF48FB1); } }
public sealed class X3072Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー 妖精"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3072Parameter(s); }
public sealed class X3072Parameter : Q6Art { public X3072Parameter() : this(null) { } public X3072Parameter(SharedDataStore? s) : base(s, 104, 1, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0xB3E5FC); } }
public sealed class X3073Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー 魔女の帽子"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3073Parameter(s); }
public sealed class X3073Parameter : Q6Art { public X3073Parameter() : this(null) { } public X3073Parameter(SharedDataStore? s) : base(s, 104, 2, 0, 0) { Color1 = Hex(0x4A148C); Color2 = Hex(0xFFD54F); } }
public sealed class X3074Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー 魔法のランプ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3074Parameter(s); }
public sealed class X3074Parameter : Q6Art { public X3074Parameter() : this(null) { } public X3074Parameter(SharedDataStore? s) : base(s, 104, 3, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0xE65100); } }
public sealed class X3075Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー ペガサス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3075Parameter(s); }
public sealed class X3075Parameter : Q6Art { public X3075Parameter() : this(null) { } public X3075Parameter(SharedDataStore? s) : base(s, 104, 4, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xB3E5FC); } }
public sealed class X3076Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー 人魚"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3076Parameter(s); }
public sealed class X3076Parameter : Q6Art { public X3076Parameter() : this(null) { } public X3076Parameter(SharedDataStore? s) : base(s, 104, 5, 0, 0) { Color1 = Hex(0x26A69A); Color2 = Hex(0xFFE0B2); } }
public sealed class X3077Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー 魔法の巻物"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3077Parameter(s); }
public sealed class X3077Parameter : Q6Art { public X3077Parameter() : this(null) { } public X3077Parameter(SharedDataStore? s) : base(s, 104, 6, 0, 0) { Color1 = Hex(0xF5E6C8); Color2 = Hex(0xC62828); } }
public sealed class X3078Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー 魔法の鏡"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3078Parameter(s); }
public sealed class X3078Parameter : Q6Art { public X3078Parameter() : this(null) { } public X3078Parameter(SharedDataStore? s) : base(s, 104, 7, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0xB3E5FC); } }
public sealed class X3079Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー ドラゴン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3079Parameter(s); }
public sealed class X3079Parameter : Q6Art { public X3079Parameter() : this(null) { } public X3079Parameter(SharedDataStore? s) : base(s, 104, 8, 0, 0) { Color1 = Hex(0xC62828); Color2 = Hex(0xFFB74D); } }
public sealed class X3080Plugin : QuickShapePlugin { public override string Name => "演出：ファンタジー 魔法使いの杖"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3080Parameter(s); }
public sealed class X3080Parameter : Q6Art { public X3080Parameter() : this(null) { } public X3080Parameter(SharedDataStore? s) : base(s, 104, 9, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0x80DEEA); } }
public sealed class X3081Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー 指輪"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3081Parameter(s); }
public sealed class X3081Parameter : Q6Art { public X3081Parameter() : this(null) { } public X3081Parameter(SharedDataStore? s) : base(s, 105, 0, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0x80DEEA); } }
public sealed class X3082Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー ネックレス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3082Parameter(s); }
public sealed class X3082Parameter : Q6Art { public X3082Parameter() : this(null) { } public X3082Parameter(SharedDataStore? s) : base(s, 105, 1, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0xE91E63); } }
public sealed class X3083Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー イヤリング"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3083Parameter(s); }
public sealed class X3083Parameter : Q6Art { public X3083Parameter() : this(null) { } public X3083Parameter(SharedDataStore? s) : base(s, 105, 2, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0x7E57C2); } }
public sealed class X3084Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー ブレスレット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3084Parameter(s); }
public sealed class X3084Parameter : Q6Art { public X3084Parameter() : this(null) { } public X3084Parameter(SharedDataStore? s) : base(s, 105, 3, 0, 0) { Color1 = Hex(0xCFD8DC); Color2 = Hex(0x26A69A); } }
public sealed class X3085Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー ヘアピン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3085Parameter(s); }
public sealed class X3085Parameter : Q6Art { public X3085Parameter() : this(null) { } public X3085Parameter(SharedDataStore? s) : base(s, 105, 4, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0xF48FB1); } }
public sealed class X3086Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー ブローチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3086Parameter(s); }
public sealed class X3086Parameter : Q6Art { public X3086Parameter() : this(null) { } public X3086Parameter(SharedDataStore? s) : base(s, 105, 5, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0x7E57C2); } }
public sealed class X3087Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー ティアラ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3087Parameter(s); }
public sealed class X3087Parameter : Q6Art { public X3087Parameter() : this(null) { } public X3087Parameter(SharedDataStore? s) : base(s, 105, 6, 0, 0) { Color1 = Hex(0xCFD8DC); Color2 = Hex(0x80DEEA); } }
public sealed class X3088Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー カチューシャ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3088Parameter(s); }
public sealed class X3088Parameter : Q6Art { public X3088Parameter() : this(null) { } public X3088Parameter(SharedDataStore? s) : base(s, 105, 7, 0, 0) { Color1 = Hex(0xE91E63); Color2 = Hex(0xFFFFFF); } }
public sealed class X3089Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー 懐中時計"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3089Parameter(s); }
public sealed class X3089Parameter : Q6Art { public X3089Parameter() : this(null) { } public X3089Parameter(SharedDataStore? s) : base(s, 105, 8, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0xFFF8E1); } }
public sealed class X3090Plugin : QuickShapePlugin { public override string Name => "演出：アクセサリー カフスボタン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3090Parameter(s); }
public sealed class X3090Parameter : Q6Art { public X3090Parameter() : this(null) { } public X3090Parameter(SharedDataStore? s) : base(s, 105, 9, 0, 0) { Color1 = Hex(0xCFD8DC); Color2 = Hex(0x1565C0); } }
public sealed class X3091Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 口紅"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3091Parameter(s); }
public sealed class X3091Parameter : Q6Art { public X3091Parameter() : this(null) { } public X3091Parameter(SharedDataStore? s) : base(s, 106, 0, 0, 0) { Color1 = Hex(0xD81B60); Color2 = Hex(0xFFD54F); } }
public sealed class X3092Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 マニキュア"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3092Parameter(s); }
public sealed class X3092Parameter : Q6Art { public X3092Parameter() : this(null) { } public X3092Parameter(SharedDataStore? s) : base(s, 106, 1, 0, 0) { Color1 = Hex(0xE91E63); Color2 = Hex(0x37474F); } }
public sealed class X3093Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 香水"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3093Parameter(s); }
public sealed class X3093Parameter : Q6Art { public X3093Parameter() : this(null) { } public X3093Parameter(SharedDataStore? s) : base(s, 106, 2, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0xFFD54F); } }
public sealed class X3094Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 手鏡"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3094Parameter(s); }
public sealed class X3094Parameter : Q6Art { public X3094Parameter() : this(null) { } public X3094Parameter(SharedDataStore? s) : base(s, 106, 3, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0xB3E5FC); } }
public sealed class X3095Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 コンパクト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3095Parameter(s); }
public sealed class X3095Parameter : Q6Art { public X3095Parameter() : this(null) { } public X3095Parameter(SharedDataStore? s) : base(s, 106, 4, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0xF8BBD0); } }
public sealed class X3096Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 メイクブラシ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3096Parameter(s); }
public sealed class X3096Parameter : Q6Art { public X3096Parameter() : this(null) { } public X3096Parameter(SharedDataStore? s) : base(s, 106, 5, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xF8BBD0); } }
public sealed class X3097Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 ヘアコーム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3097Parameter(s); }
public sealed class X3097Parameter : Q6Art { public X3097Parameter() : this(null) { } public X3097Parameter(SharedDataStore? s) : base(s, 106, 6, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0x6D4C41); } }
public sealed class X3098Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 ヘアゴム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3098Parameter(s); }
public sealed class X3098Parameter : Q6Art { public X3098Parameter() : this(null) { } public X3098Parameter(SharedDataStore? s) : base(s, 106, 7, 0, 0) { Color1 = Hex(0xE91E63); Color2 = Hex(0xFFD54F); } }
public sealed class X3099Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 化粧水"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3099Parameter(s); }
public sealed class X3099Parameter : Q6Art { public X3099Parameter() : this(null) { } public X3099Parameter(SharedDataStore? s) : base(s, 106, 8, 0, 0) { Color1 = Hex(0xB3E5FC); Color2 = Hex(0xFFFFFF); } }
public sealed class X3100Plugin : QuickShapePlugin { public override string Name => "演出：化粧品 アイシャドウ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3100Parameter(s); }
public sealed class X3100Parameter : Q6Art { public X3100Parameter() : this(null) { } public X3100Parameter(SharedDataStore? s) : base(s, 106, 9, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xF48FB1); } }
public sealed class X3101Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 モップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3101Parameter(s); }
public sealed class X3101Parameter : Q6Art { public X3101Parameter() : this(null) { } public X3101Parameter(SharedDataStore? s) : base(s, 107, 0, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0xE0E0E0); } }
public sealed class X3102Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 ぞうきん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3102Parameter(s); }
public sealed class X3102Parameter : Q6Art { public X3102Parameter() : this(null) { } public X3102Parameter(SharedDataStore? s) : base(s, 107, 1, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x90CAF9); } }
public sealed class X3103Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 はたき"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3103Parameter(s); }
public sealed class X3103Parameter : Q6Art { public X3103Parameter() : this(null) { } public X3103Parameter(SharedDataStore? s) : base(s, 107, 2, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0xFFF59D); } }
public sealed class X3104Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 スポンジ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3104Parameter(s); }
public sealed class X3104Parameter : Q6Art { public X3104Parameter() : this(null) { } public X3104Parameter(SharedDataStore? s) : base(s, 107, 3, 0, 0) { Color1 = Hex(0xFFEB3B); Color2 = Hex(0x43A047); } }
public sealed class X3105Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 ゴミ袋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3105Parameter(s); }
public sealed class X3105Parameter : Q6Art { public X3105Parameter() : this(null) { } public X3105Parameter(SharedDataStore? s) : base(s, 107, 4, 0, 0) { Color1 = Hex(0x78909C); Color2 = Hex(0x37474F); } }
public sealed class X3106Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 ゴミ箱"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3106Parameter(s); }
public sealed class X3106Parameter : Q6Art { public X3106Parameter() : this(null) { } public X3106Parameter(SharedDataStore? s) : base(s, 107, 5, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0x2E7D32); } }
public sealed class X3107Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 粘着ローラー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3107Parameter(s); }
public sealed class X3107Parameter : Q6Art { public X3107Parameter() : this(null) { } public X3107Parameter(SharedDataStore? s) : base(s, 107, 6, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0xFFFFFF); } }
public sealed class X3108Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 ゴム手袋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3108Parameter(s); }
public sealed class X3108Parameter : Q6Art { public X3108Parameter() : this(null) { } public X3108Parameter(SharedDataStore? s) : base(s, 107, 7, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0xF9A825); } }
public sealed class X3109Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 スプレー洗剤"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3109Parameter(s); }
public sealed class X3109Parameter : Q6Art { public X3109Parameter() : this(null) { } public X3109Parameter(SharedDataStore? s) : base(s, 107, 8, 0, 0) { Color1 = Hex(0x4FC3F7); Color2 = Hex(0xFFFFFF); } }
public sealed class X3110Plugin : QuickShapePlugin { public override string Name => "演出：掃除道具 窓ふきワイパー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3110Parameter(s); }
public sealed class X3110Parameter : Q6Art { public X3110Parameter() : this(null) { } public X3110Parameter(SharedDataStore? s) : base(s, 107, 9, 0, 0) { Color1 = Hex(0x1565C0); Color2 = Hex(0x37474F); } }
public sealed class X3111Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 歯みがき粉"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3111Parameter(s); }
public sealed class X3111Parameter : Q6Art { public X3111Parameter() : this(null) { } public X3111Parameter(SharedDataStore? s) : base(s, 108, 0, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x1E88E5); } }
public sealed class X3112Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 洗面器"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3112Parameter(s); }
public sealed class X3112Parameter : Q6Art { public X3112Parameter() : this(null) { } public X3112Parameter(SharedDataStore? s) : base(s, 108, 1, 0, 0) { Color1 = Hex(0xFFD54F); Color2 = Hex(0xFFF59D); } }
public sealed class X3113Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 風呂いす"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3113Parameter(s); }
public sealed class X3113Parameter : Q6Art { public X3113Parameter() : this(null) { } public X3113Parameter(SharedDataStore? s) : base(s, 108, 2, 0, 0) { Color1 = Hex(0x4FC3F7); Color2 = Hex(0x29B6F6); } }
public sealed class X3114Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 アヒルのおもちゃ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3114Parameter(s); }
public sealed class X3114Parameter : Q6Art { public X3114Parameter() : this(null) { } public X3114Parameter(SharedDataStore? s) : base(s, 108, 3, 0, 0) { Color1 = Hex(0xFFEB3B); Color2 = Hex(0xFF9800); } }
public sealed class X3115Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 湯船"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3115Parameter(s); }
public sealed class X3115Parameter : Q6Art { public X3115Parameter() : this(null) { } public X3115Parameter(SharedDataStore? s) : base(s, 108, 4, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x4FC3F7); } }
public sealed class X3116Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 ボディタオル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3116Parameter(s); }
public sealed class X3116Parameter : Q6Art { public X3116Parameter() : this(null) { } public X3116Parameter(SharedDataStore? s) : base(s, 108, 5, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0xFFFFFF); } }
public sealed class X3117Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 バスマット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3117Parameter(s); }
public sealed class X3117Parameter : Q6Art { public X3117Parameter() : this(null) { } public X3117Parameter(SharedDataStore? s) : base(s, 108, 6, 0, 0) { Color1 = Hex(0x80CBC4); Color2 = Hex(0xFFFFFF); } }
public sealed class X3118Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 ヘアブラシ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3118Parameter(s); }
public sealed class X3118Parameter : Q6Art { public X3118Parameter() : this(null) { } public X3118Parameter(SharedDataStore? s) : base(s, 108, 7, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0x37474F); } }
public sealed class X3119Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 シャワーヘッド"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3119Parameter(s); }
public sealed class X3119Parameter : Q6Art { public X3119Parameter() : this(null) { } public X3119Parameter(SharedDataStore? s) : base(s, 108, 8, 0, 0) { Color1 = Hex(0xCFD8DC); Color2 = Hex(0x4FC3F7); } }
public sealed class X3120Plugin : QuickShapePlugin { public override string Name => "演出：お風呂 入浴剤"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3120Parameter(s); }
public sealed class X3120Parameter : Q6Art { public X3120Parameter() : this(null) { } public X3120Parameter(SharedDataStore? s) : base(s, 108, 9, 0, 0) { Color1 = Hex(0xCE93D8); Color2 = Hex(0xFFFFFF); } }
public sealed class X3121Plugin : QuickShapePlugin { public override string Name => "演出：工場 ボルト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3121Parameter(s); }
public sealed class X3121Parameter : Q6Art { public X3121Parameter() : this(null) { } public X3121Parameter(SharedDataStore? s) : base(s, 109, 0, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x607D8B); } }
public sealed class X3122Plugin : QuickShapePlugin { public override string Name => "演出：工場 ナット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3122Parameter(s); }
public sealed class X3122Parameter : Q6Art { public X3122Parameter() : this(null) { } public X3122Parameter(SharedDataStore? s) : base(s, 109, 1, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x607D8B); } }
public sealed class X3123Plugin : QuickShapePlugin { public override string Name => "演出：工場 ネジ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3123Parameter(s); }
public sealed class X3123Parameter : Q6Art { public X3123Parameter() : this(null) { } public X3123Parameter(SharedDataStore? s) : base(s, 109, 2, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x607D8B); } }
public sealed class X3124Plugin : QuickShapePlugin { public override string Name => "演出：工場 ベルトコンベア"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3124Parameter(s); }
public sealed class X3124Parameter : Q6Art { public X3124Parameter() : this(null) { } public X3124Parameter(SharedDataStore? s) : base(s, 109, 3, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xFFB74D); } }
public sealed class X3125Plugin : QuickShapePlugin { public override string Name => "演出：工場 ロボットアーム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3125Parameter(s); }
public sealed class X3125Parameter : Q6Art { public X3125Parameter() : this(null) { } public X3125Parameter(SharedDataStore? s) : base(s, 109, 4, 0, 0) { Color1 = Hex(0xFFB300); Color2 = Hex(0x37474F); } }
public sealed class X3126Plugin : QuickShapePlugin { public override string Name => "演出：工場 フォークリフト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3126Parameter(s); }
public sealed class X3126Parameter : Q6Art { public X3126Parameter() : this(null) { } public X3126Parameter(SharedDataStore? s) : base(s, 109, 5, 0, 0) { Color1 = Hex(0xFFB300); Color2 = Hex(0x37474F); } }
public sealed class X3127Plugin : QuickShapePlugin { public override string Name => "演出：工場 ドラム缶"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3127Parameter(s); }
public sealed class X3127Parameter : Q6Art { public X3127Parameter() : this(null) { } public X3127Parameter(SharedDataStore? s) : base(s, 109, 6, 0, 0) { Color1 = Hex(0x1565C0); Color2 = Hex(0x0D47A1); } }
public sealed class X3128Plugin : QuickShapePlugin { public override string Name => "演出：工場 パイプとバルブ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3128Parameter(s); }
public sealed class X3128Parameter : Q6Art { public X3128Parameter() : this(null) { } public X3128Parameter(SharedDataStore? s) : base(s, 109, 7, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x607D8B); } }
public sealed class X3129Plugin : QuickShapePlugin { public override string Name => "演出：工場 スプリング"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3129Parameter(s); }
public sealed class X3129Parameter : Q6Art { public X3129Parameter() : this(null) { } public X3129Parameter(SharedDataStore? s) : base(s, 109, 8, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x607D8B); } }
public sealed class X3130Plugin : QuickShapePlugin { public override string Name => "演出：工場 溶接マスク"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3130Parameter(s); }
public sealed class X3130Parameter : Q6Art { public X3130Parameter() : this(null) { } public X3130Parameter(SharedDataStore? s) : base(s, 109, 9, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x80DEEA); } }
public sealed class X3131Plugin : QuickShapePlugin { public override string Name => "演出：農業 トラクター"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3131Parameter(s); }
public sealed class X3131Parameter : Q6Art { public X3131Parameter() : this(null) { } public X3131Parameter(SharedDataStore? s) : base(s, 110, 0, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x37474F); } }
public sealed class X3132Plugin : QuickShapePlugin { public override string Name => "演出：農業 農具のくわ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3132Parameter(s); }
public sealed class X3132Parameter : Q6Art { public X3132Parameter() : this(null) { } public X3132Parameter(SharedDataStore? s) : base(s, 110, 1, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x8D6E63); } }
public sealed class X3133Plugin : QuickShapePlugin { public override string Name => "演出：農業 草刈りがま"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3133Parameter(s); }
public sealed class X3133Parameter : Q6Art { public X3133Parameter() : this(null) { } public X3133Parameter(SharedDataStore? s) : base(s, 110, 2, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x8D6E63); } }
public sealed class X3134Plugin : QuickShapePlugin { public override string Name => "演出：農業 かかし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3134Parameter(s); }
public sealed class X3134Parameter : Q6Art { public X3134Parameter() : this(null) { } public X3134Parameter(SharedDataStore? s) : base(s, 110, 3, 0, 0) { Color1 = Hex(0x1565C0); Color2 = Hex(0xFFD54F); } }
public sealed class X3135Plugin : QuickShapePlugin { public override string Name => "演出：農業 わらの束"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3135Parameter(s); }
public sealed class X3135Parameter : Q6Art { public X3135Parameter() : this(null) { } public X3135Parameter(SharedDataStore? s) : base(s, 110, 4, 0, 0) { Color1 = Hex(0xE8C468); Color2 = Hex(0x8D6E63); } }
public sealed class X3136Plugin : QuickShapePlugin { public override string Name => "演出：農業 苗"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3136Parameter(s); }
public sealed class X3136Parameter : Q6Art { public X3136Parameter() : this(null) { } public X3136Parameter(SharedDataStore? s) : base(s, 110, 5, 0, 0) { Color1 = Hex(0x7CB342); Color2 = Hex(0x8D6E63); } }
public sealed class X3137Plugin : QuickShapePlugin { public override string Name => "演出：農業 ビニールハウス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3137Parameter(s); }
public sealed class X3137Parameter : Q6Art { public X3137Parameter() : this(null) { } public X3137Parameter(SharedDataStore? s) : base(s, 110, 6, 0, 0) { Color1 = Hex(0xB3E5FC); Color2 = Hex(0x90A4AE); } }
public sealed class X3138Plugin : QuickShapePlugin { public override string Name => "演出：農業 収穫かご"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3138Parameter(s); }
public sealed class X3138Parameter : Q6Art { public X3138Parameter() : this(null) { } public X3138Parameter(SharedDataStore? s) : base(s, 110, 7, 0, 0) { Color1 = Hex(0xC8A27A); Color2 = Hex(0xE53935); } }
public sealed class X3139Plugin : QuickShapePlugin { public override string Name => "演出：農業 肥料の袋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3139Parameter(s); }
public sealed class X3139Parameter : Q6Art { public X3139Parameter() : this(null) { } public X3139Parameter(SharedDataStore? s) : base(s, 110, 8, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0x43A047); } }
public sealed class X3140Plugin : QuickShapePlugin { public override string Name => "演出：農業 手押し車"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3140Parameter(s); }
public sealed class X3140Parameter : Q6Art { public X3140Parameter() : this(null) { } public X3140Parameter(SharedDataStore? s) : base(s, 110, 9, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0x37474F); } }
public sealed class X3141Plugin : QuickShapePlugin { public override string Name => "演出：釣り 釣りざお"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3141Parameter(s); }
public sealed class X3141Parameter : Q6Art { public X3141Parameter() : this(null) { } public X3141Parameter(SharedDataStore? s) : base(s, 111, 0, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xFFB300); } }
public sealed class X3142Plugin : QuickShapePlugin { public override string Name => "演出：釣り 浮き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3142Parameter(s); }
public sealed class X3142Parameter : Q6Art { public X3142Parameter() : this(null) { } public X3142Parameter(SharedDataStore? s) : base(s, 111, 1, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X3143Plugin : QuickShapePlugin { public override string Name => "演出：釣り ルアー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3143Parameter(s); }
public sealed class X3143Parameter : Q6Art { public X3143Parameter() : this(null) { } public X3143Parameter(SharedDataStore? s) : base(s, 111, 2, 0, 0) { Color1 = Hex(0x26A69A); Color2 = Hex(0xFFEB3B); } }
public sealed class X3144Plugin : QuickShapePlugin { public override string Name => "演出：釣り 釣り針"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3144Parameter(s); }
public sealed class X3144Parameter : Q6Art { public X3144Parameter() : this(null) { } public X3144Parameter(SharedDataStore? s) : base(s, 111, 3, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x37474F); } }
public sealed class X3145Plugin : QuickShapePlugin { public override string Name => "演出：釣り リール"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3145Parameter(s); }
public sealed class X3145Parameter : Q6Art { public X3145Parameter() : this(null) { } public X3145Parameter(SharedDataStore? s) : base(s, 111, 4, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xFFB300); } }
public sealed class X3146Plugin : QuickShapePlugin { public override string Name => "演出：釣り 魚とり網"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3146Parameter(s); }
public sealed class X3146Parameter : Q6Art { public X3146Parameter() : this(null) { } public X3146Parameter(SharedDataStore? s) : base(s, 111, 5, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0x4FC3F7); } }
public sealed class X3147Plugin : QuickShapePlugin { public override string Name => "演出：釣り えさのミミズ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3147Parameter(s); }
public sealed class X3147Parameter : Q6Art { public X3147Parameter() : this(null) { } public X3147Parameter(SharedDataStore? s) : base(s, 111, 6, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0xC2185B); } }
public sealed class X3148Plugin : QuickShapePlugin { public override string Name => "演出：釣り 魚拓"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3148Parameter(s); }
public sealed class X3148Parameter : Q6Art { public X3148Parameter() : this(null) { } public X3148Parameter(SharedDataStore? s) : base(s, 111, 7, 0, 0) { Color1 = Hex(0x263238); Color2 = Hex(0xF5F0E6); } }
public sealed class X3149Plugin : QuickShapePlugin { public override string Name => "演出：釣り 手こぎボート"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3149Parameter(s); }
public sealed class X3149Parameter : Q6Art { public X3149Parameter() : this(null) { } public X3149Parameter(SharedDataStore? s) : base(s, 111, 8, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0xFFFFFF); } }
public sealed class X3150Plugin : QuickShapePlugin { public override string Name => "演出：釣り 釣った魚"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3150Parameter(s); }
public sealed class X3150Parameter : Q6Art { public X3150Parameter() : this(null) { } public X3150Parameter(SharedDataStore? s) : base(s, 111, 9, 0, 0) { Color1 = Hex(0x42A5F5); Color2 = Hex(0xE3F2FD); } }
public sealed class X3151Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び ビリヤード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3151Parameter(s); }
public sealed class X3151Parameter : Q6Art { public X3151Parameter() : this(null) { } public X3151Parameter(SharedDataStore? s) : base(s, 112, 0, 0, 0) { Color1 = Hex(0x2E7D32); Color2 = Hex(0x6D4C41); } }
public sealed class X3152Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び 囲碁"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3152Parameter(s); }
public sealed class X3152Parameter : Q6Art { public X3152Parameter() : this(null) { } public X3152Parameter(SharedDataStore? s) : base(s, 112, 1, 0, 0) { Color1 = Hex(0xE6B86A); Color2 = Hex(0x2A2E35); } }
public sealed class X3153Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び オセロ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3153Parameter(s); }
public sealed class X3153Parameter : Q6Art { public X3153Parameter() : this(null) { } public X3153Parameter(SharedDataStore? s) : base(s, 112, 2, 0, 0) { Color1 = Hex(0x2E7D32); Color2 = Hex(0x2A2E35); } }
public sealed class X3154Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び すごろく"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3154Parameter(s); }
public sealed class X3154Parameter : Q6Art { public X3154Parameter() : this(null) { } public X3154Parameter(SharedDataStore? s) : base(s, 112, 3, 0, 0) { Color1 = Hex(0xFFF3E0); Color2 = Hex(0xE53935); } }
public sealed class X3155Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び 福笑い"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3155Parameter(s); }
public sealed class X3155Parameter : Q6Art { public X3155Parameter() : this(null) { } public X3155Parameter(SharedDataStore? s) : base(s, 112, 4, 0, 0) { Color1 = Hex(0xFFE0B2); Color2 = Hex(0x2A2E35); } }
public sealed class X3156Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び あやとり"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3156Parameter(s); }
public sealed class X3156Parameter : Q6Art { public X3156Parameter() : this(null) { } public X3156Parameter(SharedDataStore? s) : base(s, 112, 5, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFE0B2); } }
public sealed class X3157Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び 積み木タワー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3157Parameter(s); }
public sealed class X3157Parameter : Q6Art { public X3157Parameter() : this(null) { } public X3157Parameter(SharedDataStore? s) : base(s, 112, 6, 0, 0) { Color1 = Hex(0xD7A35B); Color2 = Hex(0xC27A3A); } }
public sealed class X3158Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び ピンボール"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3158Parameter(s); }
public sealed class X3158Parameter : Q6Art { public X3158Parameter() : this(null) { } public X3158Parameter(SharedDataStore? s) : base(s, 112, 7, 0, 0) { Color1 = Hex(0x5C6BC0); Color2 = Hex(0xFFB300); } }
public sealed class X3159Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び クレーンゲーム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3159Parameter(s); }
public sealed class X3159Parameter : Q6Art { public X3159Parameter() : this(null) { } public X3159Parameter(SharedDataStore? s) : base(s, 112, 8, 0, 0) { Color1 = Hex(0xF06292); Color2 = Hex(0xFFD54F); } }
public sealed class X3160Plugin : QuickShapePlugin { public override string Name => "演出：卓上の遊び エアホッケー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3160Parameter(s); }
public sealed class X3160Parameter : Q6Art { public X3160Parameter() : this(null) { } public X3160Parameter(SharedDataStore? s) : base(s, 112, 9, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xE53935); } }
public sealed class X3161Plugin : QuickShapePlugin { public override string Name => "演出：帽子 コック帽"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3161Parameter(s); }
public sealed class X3161Parameter : Q6Art { public X3161Parameter() : this(null) { } public X3161Parameter(SharedDataStore? s) : base(s, 113, 0, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE0E0E0); } }
public sealed class X3162Plugin : QuickShapePlugin { public override string Name => "演出：帽子 警察の帽子"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3162Parameter(s); }
public sealed class X3162Parameter : Q6Art { public X3162Parameter() : this(null) { } public X3162Parameter(SharedDataStore? s) : base(s, 113, 1, 0, 0) { Color1 = Hex(0x1A237E); Color2 = Hex(0xFFD54F); } }
public sealed class X3163Plugin : QuickShapePlugin { public override string Name => "演出：帽子 消防のヘルメット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3163Parameter(s); }
public sealed class X3163Parameter : Q6Art { public X3163Parameter() : this(null) { } public X3163Parameter(SharedDataStore? s) : base(s, 113, 2, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFD54F); } }
public sealed class X3164Plugin : QuickShapePlugin { public override string Name => "演出：帽子 学生帽"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3164Parameter(s); }
public sealed class X3164Parameter : Q6Art { public X3164Parameter() : this(null) { } public X3164Parameter(SharedDataStore? s) : base(s, 113, 3, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xFFD54F); } }
public sealed class X3165Plugin : QuickShapePlugin { public override string Name => "演出：帽子 麦わら帽子"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3165Parameter(s); }
public sealed class X3165Parameter : Q6Art { public X3165Parameter() : this(null) { } public X3165Parameter(SharedDataStore? s) : base(s, 113, 4, 0, 0) { Color1 = Hex(0xF2D088); Color2 = Hex(0xE53935); } }
public sealed class X3166Plugin : QuickShapePlugin { public override string Name => "演出：帽子 野球帽"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3166Parameter(s); }
public sealed class X3166Parameter : Q6Art { public X3166Parameter() : this(null) { } public X3166Parameter(SharedDataStore? s) : base(s, 113, 5, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xFFFFFF); } }
public sealed class X3167Plugin : QuickShapePlugin { public override string Name => "演出：帽子 シルクハット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3167Parameter(s); }
public sealed class X3167Parameter : Q6Art { public X3167Parameter() : this(null) { } public X3167Parameter(SharedDataStore? s) : base(s, 113, 6, 0, 0) { Color1 = Hex(0x455A64); Color2 = Hex(0xC62828); } }
public sealed class X3168Plugin : QuickShapePlugin { public override string Name => "演出：帽子 ベレー帽"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3168Parameter(s); }
public sealed class X3168Parameter : Q6Art { public X3168Parameter() : this(null) { } public X3168Parameter(SharedDataStore? s) : base(s, 113, 7, 0, 0) { Color1 = Hex(0xC62828); Color2 = Hex(0x8E0000); } }
public sealed class X3169Plugin : QuickShapePlugin { public override string Name => "演出：帽子 ニット帽"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3169Parameter(s); }
public sealed class X3169Parameter : Q6Art { public X3169Parameter() : this(null) { } public X3169Parameter(SharedDataStore? s) : base(s, 113, 8, 0, 0) { Color1 = Hex(0x26A69A); Color2 = Hex(0xFFFFFF); } }
public sealed class X3170Plugin : QuickShapePlugin { public override string Name => "演出：帽子 カウボーイハット"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3170Parameter(s); }
public sealed class X3170Parameter : Q6Art { public X3170Parameter() : this(null) { } public X3170Parameter(SharedDataStore? s) : base(s, 113, 9, 0, 0) { Color1 = Hex(0xA1887F); Color2 = Hex(0x5D4037); } }
public sealed class X3171Plugin : QuickShapePlugin { public override string Name => "演出：靴 スニーカー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3171Parameter(s); }
public sealed class X3171Parameter : Q6Art { public X3171Parameter() : this(null) { } public X3171Parameter(SharedDataStore? s) : base(s, 114, 0, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xFFFFFF); } }
public sealed class X3172Plugin : QuickShapePlugin { public override string Name => "演出：靴 ハイヒール"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3172Parameter(s); }
public sealed class X3172Parameter : Q6Art { public X3172Parameter() : this(null) { } public X3172Parameter(SharedDataStore? s) : base(s, 114, 1, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x37474F); } }
public sealed class X3173Plugin : QuickShapePlugin { public override string Name => "演出：靴 登山靴"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3173Parameter(s); }
public sealed class X3173Parameter : Q6Art { public X3173Parameter() : this(null) { } public X3173Parameter(SharedDataStore? s) : base(s, 114, 2, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0xFFB300); } }
public sealed class X3174Plugin : QuickShapePlugin { public override string Name => "演出：靴 革靴"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3174Parameter(s); }
public sealed class X3174Parameter : Q6Art { public X3174Parameter() : this(null) { } public X3174Parameter(SharedDataStore? s) : base(s, 114, 3, 0, 0) { Color1 = Hex(0x5D4037); Color2 = Hex(0x3E2723); } }
public sealed class X3175Plugin : QuickShapePlugin { public override string Name => "演出：靴 バレエシューズ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3175Parameter(s); }
public sealed class X3175Parameter : Q6Art { public X3175Parameter() : this(null) { } public X3175Parameter(SharedDataStore? s) : base(s, 114, 4, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0xE91E63); } }
public sealed class X3176Plugin : QuickShapePlugin { public override string Name => "演出：靴 ルームシューズ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3176Parameter(s); }
public sealed class X3176Parameter : Q6Art { public X3176Parameter() : this(null) { } public X3176Parameter(SharedDataStore? s) : base(s, 114, 5, 0, 0) { Color1 = Hex(0xFFB74D); Color2 = Hex(0xFFF3E0); } }
public sealed class X3177Plugin : QuickShapePlugin { public override string Name => "演出：靴 ぞうり"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3177Parameter(s); }
public sealed class X3177Parameter : Q6Art { public X3177Parameter() : this(null) { } public X3177Parameter(SharedDataStore? s) : base(s, 114, 6, 0, 0) { Color1 = Hex(0xF5E6C8); Color2 = Hex(0xE53935); } }
public sealed class X3178Plugin : QuickShapePlugin { public override string Name => "演出：靴 わらじ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3178Parameter(s); }
public sealed class X3178Parameter : Q6Art { public X3178Parameter() : this(null) { } public X3178Parameter(SharedDataStore? s) : base(s, 114, 7, 0, 0) { Color1 = Hex(0xE8C468); Color2 = Hex(0x8D6E63); } }
public sealed class X3179Plugin : QuickShapePlugin { public override string Name => "演出：靴 ローファー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3179Parameter(s); }
public sealed class X3179Parameter : Q6Art { public X3179Parameter() : this(null) { } public X3179Parameter(SharedDataStore? s) : base(s, 114, 8, 0, 0) { Color1 = Hex(0x8D5A3B); Color2 = Hex(0xC62828); } }
public sealed class X3180Plugin : QuickShapePlugin { public override string Name => "演出：靴 スケート靴"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3180Parameter(s); }
public sealed class X3180Parameter : Q6Art { public X3180Parameter() : this(null) { } public X3180Parameter(SharedDataStore? s) : base(s, 114, 9, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x90A4AE); } }
public sealed class X3181Plugin : QuickShapePlugin { public override string Name => "演出：カバン クラッチバッグ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3181Parameter(s); }
public sealed class X3181Parameter : Q6Art { public X3181Parameter() : this(null) { } public X3181Parameter(SharedDataStore? s) : base(s, 115, 0, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0xFFD54F); } }
public sealed class X3182Plugin : QuickShapePlugin { public override string Name => "演出：カバン ショルダーバッグ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3182Parameter(s); }
public sealed class X3182Parameter : Q6Art { public X3182Parameter() : this(null) { } public X3182Parameter(SharedDataStore? s) : base(s, 115, 1, 0, 0) { Color1 = Hex(0xA1887F); Color2 = Hex(0x5D4037); } }
public sealed class X3183Plugin : QuickShapePlugin { public override string Name => "演出：カバン かごバッグ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3183Parameter(s); }
public sealed class X3183Parameter : Q6Art { public X3183Parameter() : this(null) { } public X3183Parameter(SharedDataStore? s) : base(s, 115, 2, 0, 0) { Color1 = Hex(0xE8C468); Color2 = Hex(0x8D6E63); } }
public sealed class X3184Plugin : QuickShapePlugin { public override string Name => "演出：カバン スーツケース"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3184Parameter(s); }
public sealed class X3184Parameter : Q6Art { public X3184Parameter() : this(null) { } public X3184Parameter(SharedDataStore? s) : base(s, 115, 3, 0, 0) { Color1 = Hex(0x26A69A); Color2 = Hex(0x37474F); } }
public sealed class X3185Plugin : QuickShapePlugin { public override string Name => "演出：カバン トートバッグ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3185Parameter(s); }
public sealed class X3185Parameter : Q6Art { public X3185Parameter() : this(null) { } public X3185Parameter(SharedDataStore? s) : base(s, 115, 4, 0, 0) { Color1 = Hex(0xF5F0E6); Color2 = Hex(0x1565C0); } }
public sealed class X3186Plugin : QuickShapePlugin { public override string Name => "演出：カバン がま口"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3186Parameter(s); }
public sealed class X3186Parameter : Q6Art { public X3186Parameter() : this(null) { } public X3186Parameter(SharedDataStore? s) : base(s, 115, 5, 0, 0) { Color1 = Hex(0xE91E63); Color2 = Hex(0xFFD54F); } }
public sealed class X3187Plugin : QuickShapePlugin { public override string Name => "演出：カバン ポーチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3187Parameter(s); }
public sealed class X3187Parameter : Q6Art { public X3187Parameter() : this(null) { } public X3187Parameter(SharedDataStore? s) : base(s, 115, 6, 0, 0) { Color1 = Hex(0x80CBC4); Color2 = Hex(0x37474F); } }
public sealed class X3188Plugin : QuickShapePlugin { public override string Name => "演出：カバン エコバッグ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3188Parameter(s); }
public sealed class X3188Parameter : Q6Art { public X3188Parameter() : this(null) { } public X3188Parameter(SharedDataStore? s) : base(s, 115, 7, 0, 0) { Color1 = Hex(0xA5D6A7); Color2 = Hex(0x2E7D32); } }
public sealed class X3189Plugin : QuickShapePlugin { public override string Name => "演出：カバン ボストンバッグ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3189Parameter(s); }
public sealed class X3189Parameter : Q6Art { public X3189Parameter() : this(null) { } public X3189Parameter(SharedDataStore? s) : base(s, 115, 8, 0, 0) { Color1 = Hex(0x5D4037); Color2 = Hex(0xFFD54F); } }
public sealed class X3190Plugin : QuickShapePlugin { public override string Name => "演出：カバン ウエストポーチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3190Parameter(s); }
public sealed class X3190Parameter : Q6Art { public X3190Parameter() : this(null) { } public X3190Parameter(SharedDataStore? s) : base(s, 115, 9, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xFFB300); } }
public sealed class X3191Plugin : QuickShapePlugin { public override string Name => "演出：オフィス 名刺"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3191Parameter(s); }
public sealed class X3191Parameter : Q6Art { public X3191Parameter() : this(null) { } public X3191Parameter(SharedDataStore? s) : base(s, 116, 0, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x1565C0); } }
public sealed class X3192Plugin : QuickShapePlugin { public override string Name => "演出：オフィス 書類ファイル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3192Parameter(s); }
public sealed class X3192Parameter : Q6Art { public X3192Parameter() : this(null) { } public X3192Parameter(SharedDataStore? s) : base(s, 116, 1, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xFFFFFF); } }
public sealed class X3193Plugin : QuickShapePlugin { public override string Name => "演出：オフィス シュレッダー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3193Parameter(s); }
public sealed class X3193Parameter : Q6Art { public X3193Parameter() : this(null) { } public X3193Parameter(SharedDataStore? s) : base(s, 116, 2, 0, 0) { Color1 = Hex(0x455A64); Color2 = Hex(0xFFFFFF); } }
public sealed class X3194Plugin : QuickShapePlugin { public override string Name => "演出：オフィス コピー機"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3194Parameter(s); }
public sealed class X3194Parameter : Q6Art { public X3194Parameter() : this(null) { } public X3194Parameter(SharedDataStore? s) : base(s, 116, 3, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x37474F); } }
public sealed class X3195Plugin : QuickShapePlugin { public override string Name => "演出：オフィス プロジェクター"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3195Parameter(s); }
public sealed class X3195Parameter : Q6Art { public X3195Parameter() : this(null) { } public X3195Parameter(SharedDataStore? s) : base(s, 116, 4, 0, 0) { Color1 = Hex(0x546E7A); Color2 = Hex(0xFFF9C4); } }
public sealed class X3196Plugin : QuickShapePlugin { public override string Name => "演出：オフィス 社員証"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3196Parameter(s); }
public sealed class X3196Parameter : Q6Art { public X3196Parameter() : this(null) { } public X3196Parameter(SharedDataStore? s) : base(s, 116, 5, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x1E88E5); } }
public sealed class X3197Plugin : QuickShapePlugin { public override string Name => "演出：オフィス 書類の山"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3197Parameter(s); }
public sealed class X3197Parameter : Q6Art { public X3197Parameter() : this(null) { } public X3197Parameter(SharedDataStore? s) : base(s, 116, 6, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X3198Plugin : QuickShapePlugin { public override string Name => "演出：オフィス タイムカード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3198Parameter(s); }
public sealed class X3198Parameter : Q6Art { public X3198Parameter() : this(null) { } public X3198Parameter(SharedDataStore? s) : base(s, 116, 7, 0, 0) { Color1 = Hex(0xF5F0E6); Color2 = Hex(0xE53935); } }
public sealed class X3199Plugin : QuickShapePlugin { public override string Name => "演出：オフィス 回転いす"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3199Parameter(s); }
public sealed class X3199Parameter : Q6Art { public X3199Parameter() : this(null) { } public X3199Parameter(SharedDataStore? s) : base(s, 116, 8, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x90A4AE); } }
public sealed class X3200Plugin : QuickShapePlugin { public override string Name => "演出：オフィス コーヒーメーカー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3200Parameter(s); }
public sealed class X3200Parameter : Q6Art { public X3200Parameter() : this(null) { } public X3200Parameter(SharedDataStore? s) : base(s, 116, 9, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0x5D4037); } }
