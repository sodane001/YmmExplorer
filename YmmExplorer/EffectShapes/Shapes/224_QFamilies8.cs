using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// No. 2887〜3000(gen_v135.py で生成。部品の並びで描くアイコン)

public sealed class X2887Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ こけし"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2887Parameter(s); }
public sealed class X2887Parameter : Q6Art { public X2887Parameter() : this(null) { } public X2887Parameter(SharedDataStore? s) : base(s, 85, 0, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x263238); } }
public sealed class X2888Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ けん玉"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2888Parameter(s); }
public sealed class X2888Parameter : Q6Art { public X2888Parameter() : this(null) { } public X2888Parameter(SharedDataStore? s) : base(s, 85, 1, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xD9A066); } }
public sealed class X2889Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ 風呂敷"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2889Parameter(s); }
public sealed class X2889Parameter : Q6Art { public X2889Parameter() : this(null) { } public X2889Parameter(SharedDataStore? s) : base(s, 85, 2, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xFFFFFF); } }
public sealed class X2890Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ 下駄"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2890Parameter(s); }
public sealed class X2890Parameter : Q6Art { public X2890Parameter() : this(null) { } public X2890Parameter(SharedDataStore? s) : base(s, 85, 3, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xE53935); } }
public sealed class X2891Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ こま"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2891Parameter(s); }
public sealed class X2891Parameter : Q6Art { public X2891Parameter() : this(null) { } public X2891Parameter(SharedDataStore? s) : base(s, 85, 4, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x1E5FAF); } }
public sealed class X2892Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ 輪投げ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2892Parameter(s); }
public sealed class X2892Parameter : Q6Art { public X2892Parameter() : this(null) { } public X2892Parameter(SharedDataStore? s) : base(s, 85, 5, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xE53935); } }
public sealed class X2893Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ ビー玉"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2893Parameter(s); }
public sealed class X2893Parameter : Q6Art { public X2893Parameter() : this(null) { } public X2893Parameter(SharedDataStore? s) : base(s, 85, 6, 0, 0) { Color1 = Hex(0x4FC3F7); Color2 = Hex(0x1E88E5); } }
public sealed class X2894Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ おはじき"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2894Parameter(s); }
public sealed class X2894Parameter : Q6Art { public X2894Parameter() : this(null) { } public X2894Parameter(SharedDataStore? s) : base(s, 85, 7, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0x4FC3F7); } }
public sealed class X2895Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ 竹とんぼ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2895Parameter(s); }
public sealed class X2895Parameter : Q6Art { public X2895Parameter() : this(null) { } public X2895Parameter(SharedDataStore? s) : base(s, 85, 8, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0xD9A066); } }
public sealed class X2896Plugin : QuickShapePlugin { public override string Name => "演出：和のおもちゃ 水風船"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2896Parameter(s); }
public sealed class X2896Parameter : Q6Art { public X2896Parameter() : this(null) { } public X2896Parameter(SharedDataStore? s) : base(s, 85, 9, 0, 0) { Color1 = Hex(0xE91E63); Color2 = Hex(0xFFFFFF); } }
public sealed class X2897Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ ヨーヨー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2897Parameter(s); }
public sealed class X2897Parameter : Q6Art { public X2897Parameter() : this(null) { } public X2897Parameter(SharedDataStore? s) : base(s, 86, 0, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFE14D); } }
public sealed class X2898Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ かざぐるま"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2898Parameter(s); }
public sealed class X2898Parameter : Q6Art { public X2898Parameter() : this(null) { } public X2898Parameter(SharedDataStore? s) : base(s, 86, 1, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x8D6E63); } }
public sealed class X2899Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ ぬいぐるみ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2899Parameter(s); }
public sealed class X2899Parameter : Q6Art { public X2899Parameter() : this(null) { } public X2899Parameter(SharedDataStore? s) : base(s, 86, 2, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xE53935); } }
public sealed class X2900Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ ミニカー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2900Parameter(s); }
public sealed class X2900Parameter : Q6Art { public X2900Parameter() : this(null) { } public X2900Parameter(SharedDataStore? s) : base(s, 86, 3, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0xBFE6FF); } }
public sealed class X2901Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ ブーメラン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2901Parameter(s); }
public sealed class X2901Parameter : Q6Art { public X2901Parameter() : this(null) { } public X2901Parameter(SharedDataStore? s) : base(s, 86, 4, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0xE53935); } }
public sealed class X2902Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ フリスビー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2902Parameter(s); }
public sealed class X2902Parameter : Q6Art { public X2902Parameter() : this(null) { } public X2902Parameter(SharedDataStore? s) : base(s, 86, 5, 0, 0) { Color1 = Hex(0x26C6DA); Color2 = Hex(0xFFFFFF); } }
public sealed class X2903Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ 水鉄砲"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2903Parameter(s); }
public sealed class X2903Parameter : Q6Art { public X2903Parameter() : this(null) { } public X2903Parameter(SharedDataStore? s) : base(s, 86, 6, 0, 0) { Color1 = Hex(0x29B6F6); Color2 = Hex(0xFFB300); } }
public sealed class X2904Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ ガラガラ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2904Parameter(s); }
public sealed class X2904Parameter : Q6Art { public X2904Parameter() : this(null) { } public X2904Parameter(SharedDataStore? s) : base(s, 86, 7, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0xFFE14D); } }
public sealed class X2905Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ お面"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2905Parameter(s); }
public sealed class X2905Parameter : Q6Art { public X2905Parameter() : this(null) { } public X2905Parameter(SharedDataStore? s) : base(s, 86, 8, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2906Plugin : QuickShapePlugin { public override string Name => "演出：おもちゃ でんでん太鼓"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2906Parameter(s); }
public sealed class X2906Parameter : Q6Art { public X2906Parameter() : this(null) { } public X2906Parameter(SharedDataStore? s) : base(s, 86, 9, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xD9A066); } }
public sealed class X2907Plugin : QuickShapePlugin { public override string Name => "演出：屋台 わたあめ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2907Parameter(s); }
public sealed class X2907Parameter : Q6Art { public X2907Parameter() : this(null) { } public X2907Parameter(SharedDataStore? s) : base(s, 87, 0, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0xFFFFFF); } }
public sealed class X2908Plugin : QuickShapePlugin { public override string Name => "演出：屋台 りんごあめ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2908Parameter(s); }
public sealed class X2908Parameter : Q6Art { public X2908Parameter() : this(null) { } public X2908Parameter(SharedDataStore? s) : base(s, 87, 1, 0, 0) { Color1 = Hex(0xD50000); Color2 = Hex(0xFFFFFF); } }
public sealed class X2909Plugin : QuickShapePlugin { public override string Name => "演出：屋台 チョコバナナ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2909Parameter(s); }
public sealed class X2909Parameter : Q6Art { public X2909Parameter() : this(null) { } public X2909Parameter(SharedDataStore? s) : base(s, 87, 2, 0, 0) { Color1 = Hex(0x5D4037); Color2 = Hex(0xFFEB3B); } }
public sealed class X2910Plugin : QuickShapePlugin { public override string Name => "演出：屋台 フランクフルト"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2910Parameter(s); }
public sealed class X2910Parameter : Q6Art { public X2910Parameter() : this(null) { } public X2910Parameter(SharedDataStore? s) : base(s, 87, 3, 0, 0) { Color1 = Hex(0xC0392B); Color2 = Hex(0xFFEB3B); } }
public sealed class X2911Plugin : QuickShapePlugin { public override string Name => "演出：屋台 ベビーカステラ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2911Parameter(s); }
public sealed class X2911Parameter : Q6Art { public X2911Parameter() : this(null) { } public X2911Parameter(SharedDataStore? s) : base(s, 87, 4, 0, 0) { Color1 = Hex(0xE6A44A); Color2 = Hex(0xFFFFFF); } }
public sealed class X2912Plugin : QuickShapePlugin { public override string Name => "演出：屋台 いか焼き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2912Parameter(s); }
public sealed class X2912Parameter : Q6Art { public X2912Parameter() : this(null) { } public X2912Parameter(SharedDataStore? s) : base(s, 87, 5, 0, 0) { Color1 = Hex(0xC77B14); Color2 = Hex(0x8D5A3B); } }
public sealed class X2913Plugin : QuickShapePlugin { public override string Name => "演出：屋台 じゃがバター"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2913Parameter(s); }
public sealed class X2913Parameter : Q6Art { public X2913Parameter() : this(null) { } public X2913Parameter(SharedDataStore? s) : base(s, 87, 6, 0, 0) { Color1 = Hex(0xE0B97A); Color2 = Hex(0xFFE082); } }
public sealed class X2914Plugin : QuickShapePlugin { public override string Name => "演出：屋台 クレープ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2914Parameter(s); }
public sealed class X2914Parameter : Q6Art { public X2914Parameter() : this(null) { } public X2914Parameter(SharedDataStore? s) : base(s, 87, 7, 0, 0) { Color1 = Hex(0xF5DEB3); Color2 = Hex(0xE53935); } }
public sealed class X2915Plugin : QuickShapePlugin { public override string Name => "演出：屋台 大判焼き"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2915Parameter(s); }
public sealed class X2915Parameter : Q6Art { public X2915Parameter() : this(null) { } public X2915Parameter(SharedDataStore? s) : base(s, 87, 8, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0x6D4C41); } }
public sealed class X2916Plugin : QuickShapePlugin { public override string Name => "演出：屋台 チュロス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2916Parameter(s); }
public sealed class X2916Parameter : Q6Art { public X2916Parameter() : this(null) { } public X2916Parameter(SharedDataStore? s) : base(s, 87, 9, 0, 0) { Color1 = Hex(0xD9822B); Color2 = Hex(0xF8BBD0); } }
public sealed class X2917Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 寝袋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2917Parameter(s); }
public sealed class X2917Parameter : Q6Art { public X2917Parameter() : this(null) { } public X2917Parameter(SharedDataStore? s) : base(s, 88, 0, 0, 0) { Color1 = Hex(0x26A69A); Color2 = Hex(0xFFE082); } }
public sealed class X2918Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 たき火"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2918Parameter(s); }
public sealed class X2918Parameter : Q6Art { public X2918Parameter() : this(null) { } public X2918Parameter(SharedDataStore? s) : base(s, 88, 1, 0, 0) { Color1 = Hex(0xFF7043); Color2 = Hex(0xFFEB3B); } }
public sealed class X2919Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 飯ごう"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2919Parameter(s); }
public sealed class X2919Parameter : Q6Art { public X2919Parameter() : this(null) { } public X2919Parameter(SharedDataStore? s) : base(s, 88, 2, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x37474F); } }
public sealed class X2920Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 折りたたみいす"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2920Parameter(s); }
public sealed class X2920Parameter : Q6Art { public X2920Parameter() : this(null) { } public X2920Parameter(SharedDataStore? s) : base(s, 88, 3, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x90A4AE); } }
public sealed class X2921Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 クーラーボックス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2921Parameter(s); }
public sealed class X2921Parameter : Q6Art { public X2921Parameter() : this(null) { } public X2921Parameter(SharedDataStore? s) : base(s, 88, 4, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xFFFFFF); } }
public sealed class X2922Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 マッチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2922Parameter(s); }
public sealed class X2922Parameter : Q6Art { public X2922Parameter() : this(null) { } public X2922Parameter(SharedDataStore? s) : base(s, 88, 5, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xF5DEB3); } }
public sealed class X2923Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 ロープ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2923Parameter(s); }
public sealed class X2923Parameter : Q6Art { public X2923Parameter() : this(null) { } public X2923Parameter(SharedDataStore? s) : base(s, 88, 6, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xB5651D); } }
public sealed class X2924Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 バーベキューコンロ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2924Parameter(s); }
public sealed class X2924Parameter : Q6Art { public X2924Parameter() : this(null) { } public X2924Parameter(SharedDataStore? s) : base(s, 88, 7, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xC0392B); } }
public sealed class X2925Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 キャンプのランタン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2925Parameter(s); }
public sealed class X2925Parameter : Q6Art { public X2925Parameter() : this(null) { } public X2925Parameter(SharedDataStore? s) : base(s, 88, 8, 0, 0) { Color1 = Hex(0x2E7D32); Color2 = Hex(0xFFE082); } }
public sealed class X2926Plugin : QuickShapePlugin { public override string Name => "演出：キャンプ用品 虫かご"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2926Parameter(s); }
public sealed class X2926Parameter : Q6Art { public X2926Parameter() : this(null) { } public X2926Parameter(SharedDataStore? s) : base(s, 88, 9, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xFFFFFF); } }
public sealed class X2927Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 レジ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2927Parameter(s); }
public sealed class X2927Parameter : Q6Art { public X2927Parameter() : this(null) { } public X2927Parameter(SharedDataStore? s) : base(s, 89, 0, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x37474F); } }
public sealed class X2928Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 買い物かご"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2928Parameter(s); }
public sealed class X2928Parameter : Q6Art { public X2928Parameter() : this(null) { } public X2928Parameter(SharedDataStore? s) : base(s, 89, 1, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2929Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 紙袋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2929Parameter(s); }
public sealed class X2929Parameter : Q6Art { public X2929Parameter() : this(null) { } public X2929Parameter(SharedDataStore? s) : base(s, 89, 2, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0x8D5A3B); } }
public sealed class X2930Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 のれん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2930Parameter(s); }
public sealed class X2930Parameter : Q6Art { public X2930Parameter() : this(null) { } public X2930Parameter(SharedDataStore? s) : base(s, 89, 3, 0, 0) { Color1 = Hex(0x1E5FAF); Color2 = Hex(0xFFFFFF); } }
public sealed class X2931Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 自動販売機"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2931Parameter(s); }
public sealed class X2931Parameter : Q6Art { public X2931Parameter() : this(null) { } public X2931Parameter(SharedDataStore? s) : base(s, 89, 4, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2932Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 カプセルトイ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2932Parameter(s); }
public sealed class X2932Parameter : Q6Art { public X2932Parameter() : this(null) { } public X2932Parameter(SharedDataStore? s) : base(s, 89, 5, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2933Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 ポイントカード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2933Parameter(s); }
public sealed class X2933Parameter : Q6Art { public X2933Parameter() : this(null) { } public X2933Parameter(SharedDataStore? s) : base(s, 89, 6, 0, 0) { Color1 = Hex(0xFFB300); Color2 = Hex(0x37474F); } }
public sealed class X2934Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 ショーケース"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2934Parameter(s); }
public sealed class X2934Parameter : Q6Art { public X2934Parameter() : this(null) { } public X2934Parameter(SharedDataStore? s) : base(s, 89, 7, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x8FD3FF); } }
public sealed class X2935Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 卓上ベル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2935Parameter(s); }
public sealed class X2935Parameter : Q6Art { public X2935Parameter() : this(null) { } public X2935Parameter(SharedDataStore? s) : base(s, 89, 8, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0x37474F); } }
public sealed class X2936Plugin : QuickShapePlugin { public override string Name => "演出：お店の物 試着室"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2936Parameter(s); }
public sealed class X2936Parameter : Q6Art { public X2936Parameter() : this(null) { } public X2936Parameter(SharedDataStore? s) : base(s, 89, 9, 0, 0) { Color1 = Hex(0xE8488A); Color2 = Hex(0x90A4AE); } }
public sealed class X2937Plugin : QuickShapePlugin { public override string Name => "演出：病院 体温計"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2937Parameter(s); }
public sealed class X2937Parameter : Q6Art { public X2937Parameter() : this(null) { } public X2937Parameter(SharedDataStore? s) : base(s, 90, 0, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x1E88E5); } }
public sealed class X2938Plugin : QuickShapePlugin { public override string Name => "演出：病院 ばんそうこう"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2938Parameter(s); }
public sealed class X2938Parameter : Q6Art { public X2938Parameter() : this(null) { } public X2938Parameter(SharedDataStore? s) : base(s, 90, 1, 0, 0) { Color1 = Hex(0xF5C9A0); Color2 = Hex(0xFFFFFF); } }
public sealed class X2939Plugin : QuickShapePlugin { public override string Name => "演出：病院 マスク"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2939Parameter(s); }
public sealed class X2939Parameter : Q6Art { public X2939Parameter() : this(null) { } public X2939Parameter(SharedDataStore? s) : base(s, 90, 2, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x90A4AE); } }
public sealed class X2940Plugin : QuickShapePlugin { public override string Name => "演出：病院 松葉づえ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2940Parameter(s); }
public sealed class X2940Parameter : Q6Art { public X2940Parameter() : this(null) { } public X2940Parameter(SharedDataStore? s) : base(s, 90, 3, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x37474F); } }
public sealed class X2941Plugin : QuickShapePlugin { public override string Name => "演出：病院 包帯"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2941Parameter(s); }
public sealed class X2941Parameter : Q6Art { public X2941Parameter() : this(null) { } public X2941Parameter(SharedDataStore? s) : base(s, 90, 4, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE0E0E0); } }
public sealed class X2942Plugin : QuickShapePlugin { public override string Name => "演出：病院 目薬"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2942Parameter(s); }
public sealed class X2942Parameter : Q6Art { public X2942Parameter() : this(null) { } public X2942Parameter(SharedDataStore? s) : base(s, 90, 5, 0, 0) { Color1 = Hex(0xE3F2FD); Color2 = Hex(0x1E88E5); } }
public sealed class X2943Plugin : QuickShapePlugin { public override string Name => "演出：病院 血圧計"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2943Parameter(s); }
public sealed class X2943Parameter : Q6Art { public X2943Parameter() : this(null) { } public X2943Parameter(SharedDataStore? s) : base(s, 90, 6, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x37474F); } }
public sealed class X2944Plugin : QuickShapePlugin { public override string Name => "演出：病院 点滴"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2944Parameter(s); }
public sealed class X2944Parameter : Q6Art { public X2944Parameter() : this(null) { } public X2944Parameter(SharedDataStore? s) : base(s, 90, 7, 0, 0) { Color1 = Hex(0xE3F2FD); Color2 = Hex(0x90A4AE); } }
public sealed class X2945Plugin : QuickShapePlugin { public override string Name => "演出：病院 湿布"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2945Parameter(s); }
public sealed class X2945Parameter : Q6Art { public X2945Parameter() : this(null) { } public X2945Parameter(SharedDataStore? s) : base(s, 90, 8, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x90CAF9); } }
public sealed class X2946Plugin : QuickShapePlugin { public override string Name => "演出：病院 氷のう"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2946Parameter(s); }
public sealed class X2946Parameter : Q6Art { public X2946Parameter() : this(null) { } public X2946Parameter(SharedDataStore? s) : base(s, 90, 9, 0, 0) { Color1 = Hex(0x4FC3F7); Color2 = Hex(0x90A4AE); } }
public sealed class X2947Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 おたま"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2947Parameter(s); }
public sealed class X2947Parameter : Q6Art { public X2947Parameter() : this(null) { } public X2947Parameter(SharedDataStore? s) : base(s, 91, 0, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x5D4037); } }
public sealed class X2948Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 計量カップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2948Parameter(s); }
public sealed class X2948Parameter : Q6Art { public X2948Parameter() : this(null) { } public X2948Parameter(SharedDataStore? s) : base(s, 91, 1, 0, 0) { Color1 = Hex(0xE3F2FD); Color2 = Hex(0xE53935); } }
public sealed class X2949Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 菜箸"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2949Parameter(s); }
public sealed class X2949Parameter : Q6Art { public X2949Parameter() : this(null) { } public X2949Parameter(SharedDataStore? s) : base(s, 91, 2, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0x5D4037); } }
public sealed class X2950Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 ざる"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2950Parameter(s); }
public sealed class X2950Parameter : Q6Art { public X2950Parameter() : this(null) { } public X2950Parameter(SharedDataStore? s) : base(s, 91, 3, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xB5651D); } }
public sealed class X2951Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 めん棒"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2951Parameter(s); }
public sealed class X2951Parameter : Q6Art { public X2951Parameter() : this(null) { } public X2951Parameter(SharedDataStore? s) : base(s, 91, 4, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xB5651D); } }
public sealed class X2952Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 土鍋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2952Parameter(s); }
public sealed class X2952Parameter : Q6Art { public X2952Parameter() : this(null) { } public X2952Parameter(SharedDataStore? s) : base(s, 91, 5, 0, 0) { Color1 = Hex(0x5D4037); Color2 = Hex(0xF5E6C8); } }
public sealed class X2953Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 しゃもじ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2953Parameter(s); }
public sealed class X2953Parameter : Q6Art { public X2953Parameter() : this(null) { } public X2953Parameter(SharedDataStore? s) : base(s, 91, 6, 0, 0) { Color1 = Hex(0xF5E6C8); Color2 = Hex(0xD9A066); } }
public sealed class X2954Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 茶せん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2954Parameter(s); }
public sealed class X2954Parameter : Q6Art { public X2954Parameter() : this(null) { } public X2954Parameter(SharedDataStore? s) : base(s, 91, 7, 0, 0) { Color1 = Hex(0xD9C08A); Color2 = Hex(0x8D6E63); } }
public sealed class X2955Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 おろし金"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2955Parameter(s); }
public sealed class X2955Parameter : Q6Art { public X2955Parameter() : this(null) { } public X2955Parameter(SharedDataStore? s) : base(s, 91, 8, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0x78909C); } }
public sealed class X2956Plugin : QuickShapePlugin { public override string Name => "演出：台所の道具 フライパン返し"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2956Parameter(s); }
public sealed class X2956Parameter : Q6Art { public X2956Parameter() : this(null) { } public X2956Parameter(SharedDataStore? s) : base(s, 91, 9, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xD9A066); } }
public sealed class X2957Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 カーブミラー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2957Parameter(s); }
public sealed class X2957Parameter : Q6Art { public X2957Parameter() : this(null) { } public X2957Parameter(SharedDataStore? s) : base(s, 92, 0, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0x90A4AE); } }
public sealed class X2958Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 ガードレール"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2958Parameter(s); }
public sealed class X2958Parameter : Q6Art { public X2958Parameter() : this(null) { } public X2958Parameter(SharedDataStore? s) : base(s, 92, 1, 0, 0) { Color1 = Hex(0xECEFF1); Color2 = Hex(0x90A4AE); } }
public sealed class X2959Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 マンホール"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2959Parameter(s); }
public sealed class X2959Parameter : Q6Art { public X2959Parameter() : this(null) { } public X2959Parameter(SharedDataStore? s) : base(s, 92, 2, 0, 0) { Color1 = Hex(0x78909C); Color2 = Hex(0x546E7A); } }
public sealed class X2960Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 消火栓"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2960Parameter(s); }
public sealed class X2960Parameter : Q6Art { public X2960Parameter() : this(null) { } public X2960Parameter(SharedDataStore? s) : base(s, 92, 3, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFC107); } }
public sealed class X2961Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 街灯"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2961Parameter(s); }
public sealed class X2961Parameter : Q6Art { public X2961Parameter() : this(null) { } public X2961Parameter(SharedDataStore? s) : base(s, 92, 4, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xFFE082); } }
public sealed class X2962Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 電柱"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2962Parameter(s); }
public sealed class X2962Parameter : Q6Art { public X2962Parameter() : this(null) { } public X2962Parameter(SharedDataStore? s) : base(s, 92, 5, 0, 0) { Color1 = Hex(0x9E9E9E); Color2 = Hex(0x37474F); } }
public sealed class X2963Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 ベンチ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2963Parameter(s); }
public sealed class X2963Parameter : Q6Art { public X2963Parameter() : this(null) { } public X2963Parameter(SharedDataStore? s) : base(s, 92, 6, 0, 0) { Color1 = Hex(0x8D5A3B); Color2 = Hex(0x37474F); } }
public sealed class X2964Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 歩道橋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2964Parameter(s); }
public sealed class X2964Parameter : Q6Art { public X2964Parameter() : this(null) { } public X2964Parameter(SharedDataStore? s) : base(s, 92, 7, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x90A4AE); } }
public sealed class X2965Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 バリケード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2965Parameter(s); }
public sealed class X2965Parameter : Q6Art { public X2965Parameter() : this(null) { } public X2965Parameter(SharedDataStore? s) : base(s, 92, 8, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0x37474F); } }
public sealed class X2966Plugin : QuickShapePlugin { public override string Name => "演出：道路の物 車止め"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2966Parameter(s); }
public sealed class X2966Parameter : Q6Art { public X2966Parameter() : this(null) { } public X2966Parameter(SharedDataStore? s) : base(s, 92, 9, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0x37474F); } }
public sealed class X2967Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 イーゼル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2967Parameter(s); }
public sealed class X2967Parameter : Q6Art { public X2967Parameter() : this(null) { } public X2967Parameter(SharedDataStore? s) : base(s, 93, 0, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xFFFFFF); } }
public sealed class X2968Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 ねんど"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2968Parameter(s); }
public sealed class X2968Parameter : Q6Art { public X2968Parameter() : this(null) { } public X2968Parameter(SharedDataStore? s) : base(s, 93, 1, 0, 0) { Color1 = Hex(0x8D6E63); Color2 = Hex(0x4FC3F7); } }
public sealed class X2969Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 クレヨン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2969Parameter(s); }
public sealed class X2969Parameter : Q6Art { public X2969Parameter() : this(null) { } public X2969Parameter(SharedDataStore? s) : base(s, 93, 2, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x2F86C6); } }
public sealed class X2970Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 色鉛筆"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2970Parameter(s); }
public sealed class X2970Parameter : Q6Art { public X2970Parameter() : this(null) { } public X2970Parameter(SharedDataStore? s) : base(s, 93, 3, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xE53935); } }
public sealed class X2971Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 絵の具チューブ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2971Parameter(s); }
public sealed class X2971Parameter : Q6Art { public X2971Parameter() : this(null) { } public X2971Parameter(SharedDataStore? s) : base(s, 93, 4, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xB0BEC5); } }
public sealed class X2972Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 習字の筆"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2972Parameter(s); }
public sealed class X2972Parameter : Q6Art { public X2972Parameter() : this(null) { } public X2972Parameter(SharedDataStore? s) : base(s, 93, 5, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xD9A066); } }
public sealed class X2973Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 彫刻刀"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2973Parameter(s); }
public sealed class X2973Parameter : Q6Art { public X2973Parameter() : this(null) { } public X2973Parameter(SharedDataStore? s) : base(s, 93, 6, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xB0BEC5); } }
public sealed class X2974Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 版画ローラー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2974Parameter(s); }
public sealed class X2974Parameter : Q6Art { public X2974Parameter() : this(null) { } public X2974Parameter(SharedDataStore? s) : base(s, 93, 7, 0, 0) { Color1 = Hex(0x37474F); Color2 = Hex(0xE53935); } }
public sealed class X2975Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 スケッチブック"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2975Parameter(s); }
public sealed class X2975Parameter : Q6Art { public X2975Parameter() : this(null) { } public X2975Parameter(SharedDataStore? s) : base(s, 93, 8, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0x37474F); } }
public sealed class X2976Plugin : QuickShapePlugin { public override string Name => "演出：絵と工作 毛糸と編み棒"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2976Parameter(s); }
public sealed class X2976Parameter : Q6Art { public X2976Parameter() : this(null) { } public X2976Parameter(SharedDataStore? s) : base(s, 93, 9, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xD9A066); } }
public sealed class X2977Plugin : QuickShapePlugin { public override string Name => "演出：パン あんパン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2977Parameter(s); }
public sealed class X2977Parameter : Q6Art { public X2977Parameter() : this(null) { } public X2977Parameter(SharedDataStore? s) : base(s, 94, 0, 0, 0) { Color1 = Hex(0xC77B14); Color2 = Hex(0x5D4037); } }
public sealed class X2978Plugin : QuickShapePlugin { public override string Name => "演出：パン カレーパン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2978Parameter(s); }
public sealed class X2978Parameter : Q6Art { public X2978Parameter() : this(null) { } public X2978Parameter(SharedDataStore? s) : base(s, 94, 1, 0, 0) { Color1 = Hex(0xD9822B); Color2 = Hex(0xB5651D); } }
public sealed class X2979Plugin : QuickShapePlugin { public override string Name => "演出：パン ベーグル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2979Parameter(s); }
public sealed class X2979Parameter : Q6Art { public X2979Parameter() : this(null) { } public X2979Parameter(SharedDataStore? s) : base(s, 94, 2, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xB5651D); } }
public sealed class X2980Plugin : QuickShapePlugin { public override string Name => "演出：パン フランスパン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2980Parameter(s); }
public sealed class X2980Parameter : Q6Art { public X2980Parameter() : this(null) { } public X2980Parameter(SharedDataStore? s) : base(s, 94, 3, 0, 0) { Color1 = Hex(0xC77B14); Color2 = Hex(0xF5DEB3); } }
public sealed class X2981Plugin : QuickShapePlugin { public override string Name => "演出：パン クリームパン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2981Parameter(s); }
public sealed class X2981Parameter : Q6Art { public X2981Parameter() : this(null) { } public X2981Parameter(SharedDataStore? s) : base(s, 94, 4, 0, 0) { Color1 = Hex(0xE6A44A); Color2 = Hex(0xFFF59D); } }
public sealed class X2982Plugin : QuickShapePlugin { public override string Name => "演出：パン チョココロネ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2982Parameter(s); }
public sealed class X2982Parameter : Q6Art { public X2982Parameter() : this(null) { } public X2982Parameter(SharedDataStore? s) : base(s, 94, 5, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0x5D4037); } }
public sealed class X2983Plugin : QuickShapePlugin { public override string Name => "演出：パン プレッツェル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2983Parameter(s); }
public sealed class X2983Parameter : Q6Art { public X2983Parameter() : this(null) { } public X2983Parameter(SharedDataStore? s) : base(s, 94, 6, 0, 0) { Color1 = Hex(0xA0522D); Color2 = Hex(0xFFFFFF); } }
public sealed class X2984Plugin : QuickShapePlugin { public override string Name => "演出：パン ワッフル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2984Parameter(s); }
public sealed class X2984Parameter : Q6Art { public X2984Parameter() : this(null) { } public X2984Parameter(SharedDataStore? s) : base(s, 94, 7, 0, 0) { Color1 = Hex(0xE6A44A); Color2 = Hex(0xC77B14); } }
public sealed class X2985Plugin : QuickShapePlugin { public override string Name => "演出：パン メロンパン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2985Parameter(s); }
public sealed class X2985Parameter : Q6Art { public X2985Parameter() : this(null) { } public X2985Parameter(SharedDataStore? s) : base(s, 94, 8, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xD9A066); } }
public sealed class X2986Plugin : QuickShapePlugin { public override string Name => "演出：パン ロールパン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2986Parameter(s); }
public sealed class X2986Parameter : Q6Art { public X2986Parameter() : this(null) { } public X2986Parameter(SharedDataStore? s) : base(s, 94, 9, 0, 0) { Color1 = Hex(0xC77B14); Color2 = Hex(0xE6A44A); } }
public sealed class X2987Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 空き缶"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2987Parameter(s); }
public sealed class X2987Parameter : Q6Art { public X2987Parameter() : this(null) { } public X2987Parameter(SharedDataStore? s) : base(s, 95, 0, 0, 0) { Color1 = Hex(0xB0BEC5); Color2 = Hex(0xE53935); } }
public sealed class X2988Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 段ボール箱"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2988Parameter(s); }
public sealed class X2988Parameter : Q6Art { public X2988Parameter() : this(null) { } public X2988Parameter(SharedDataStore? s) : base(s, 95, 1, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0xB5651D); } }
public sealed class X2989Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 新聞紙"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2989Parameter(s); }
public sealed class X2989Parameter : Q6Art { public X2989Parameter() : this(null) { } public X2989Parameter(SharedDataStore? s) : base(s, 95, 2, 0, 0) { Color1 = Hex(0xF5F5F5); Color2 = Hex(0x37474F); } }
public sealed class X2990Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 洗濯ばさみ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2990Parameter(s); }
public sealed class X2990Parameter : Q6Art { public X2990Parameter() : this(null) { } public X2990Parameter(SharedDataStore? s) : base(s, 95, 3, 0, 0) { Color1 = Hex(0x4FC3F7); Color2 = Hex(0xFFFFFF); } }
public sealed class X2991Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 物干し"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2991Parameter(s); }
public sealed class X2991Parameter : Q6Art { public X2991Parameter() : this(null) { } public X2991Parameter(SharedDataStore? s) : base(s, 95, 4, 0, 0) { Color1 = Hex(0x90A4AE); Color2 = Hex(0x4FC3F7); } }
public sealed class X2992Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 コンセント"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2992Parameter(s); }
public sealed class X2992Parameter : Q6Art { public X2992Parameter() : this(null) { } public X2992Parameter(SharedDataStore? s) : base(s, 95, 5, 0, 0) { Color1 = Hex(0xF5F5F5); Color2 = Hex(0x37474F); } }
public sealed class X2993Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 延長コード"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2993Parameter(s); }
public sealed class X2993Parameter : Q6Art { public X2993Parameter() : this(null) { } public X2993Parameter(SharedDataStore? s) : base(s, 95, 6, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0x37474F); } }
public sealed class X2994Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 スリッパ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2994Parameter(s); }
public sealed class X2994Parameter : Q6Art { public X2994Parameter() : this(null) { } public X2994Parameter(SharedDataStore? s) : base(s, 95, 7, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0xFFFFFF); } }
public sealed class X2995Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 アイロン台"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2995Parameter(s); }
public sealed class X2995Parameter : Q6Art { public X2995Parameter() : this(null) { } public X2995Parameter(SharedDataStore? s) : base(s, 95, 8, 0, 0) { Color1 = Hex(0x4FC3F7); Color2 = Hex(0x90A4AE); } }
public sealed class X2996Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 輪ゴム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2996Parameter(s); }
public sealed class X2996Parameter : Q6Art { public X2996Parameter() : this(null) { } public X2996Parameter(SharedDataStore? s) : base(s, 95, 9, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0xE53935); } }
public sealed class X2997Plugin : QuickShapePlugin { public override string Name => "演出：学校の物 上ばき"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2997Parameter(s); }
public sealed class X2997Parameter : Q6Art { public X2997Parameter() : this(null) { } public X2997Parameter(SharedDataStore? s) : base(s, 96, 0, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2998Plugin : QuickShapePlugin { public override string Name => "演出：学校の物 名札"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2998Parameter(s); }
public sealed class X2998Parameter : Q6Art { public X2998Parameter() : this(null) { } public X2998Parameter(SharedDataStore? s) : base(s, 96, 1, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2999Plugin : QuickShapePlugin { public override string Name => "演出：学校の物 通知表"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2999Parameter(s); }
public sealed class X2999Parameter : Q6Art { public X2999Parameter() : this(null) { } public X2999Parameter(SharedDataStore? s) : base(s, 96, 2, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0xFFFFFF); } }
public sealed class X3000Plugin : QuickShapePlugin { public override string Name => "演出：学校の物 給食トレー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X3000Parameter(s); }
public sealed class X3000Parameter : Q6Art { public X3000Parameter() : this(null) { } public X3000Parameter(SharedDataStore? s) : base(s, 96, 3, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0xFFFFFF); } }
