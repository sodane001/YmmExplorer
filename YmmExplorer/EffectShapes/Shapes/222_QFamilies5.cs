using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Shape;
using SharedDataStore = YukkuriMovieMaker.Project.SharedDataStore;

namespace YMM4EffectShapes;

// No. 2401〜2500(gen_v130.py で生成)

public sealed class X2401Plugin : QuickShapePlugin { public override string Name => "演出：キッチン マグカップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2401Parameter(s); }
public sealed class X2401Parameter : Q6Art { public X2401Parameter() : this(null) { } public X2401Parameter(SharedDataStore? s) : base(s, 54, 0, 0, 0) { Color1 = Hex(0xE8488A); Color2 = Hex(0xB3265F); } }
public sealed class X2402Plugin : QuickShapePlugin { public override string Name => "演出：キッチン ティーカップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2402Parameter(s); }
public sealed class X2402Parameter : Q6Art { public X2402Parameter() : this(null) { } public X2402Parameter(SharedDataStore? s) : base(s, 54, 1, 0, 0) { Color1 = Hex(0xF5F1EA); Color2 = Hex(0x9AA3AD); } }
public sealed class X2403Plugin : QuickShapePlugin { public override string Name => "演出：キッチン 皿"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2403Parameter(s); }
public sealed class X2403Parameter : Q6Art { public X2403Parameter() : this(null) { } public X2403Parameter(SharedDataStore? s) : base(s, 54, 2, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x9AA3AD); } }
public sealed class X2404Plugin : QuickShapePlugin { public override string Name => "演出：キッチン ボウル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2404Parameter(s); }
public sealed class X2404Parameter : Q6Art { public X2404Parameter() : this(null) { } public X2404Parameter(SharedDataStore? s) : base(s, 54, 3, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0xC77B14); } }
public sealed class X2405Plugin : QuickShapePlugin { public override string Name => "演出：キッチン フライパン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2405Parameter(s); }
public sealed class X2405Parameter : Q6Art { public X2405Parameter() : this(null) { } public X2405Parameter(SharedDataStore? s) : base(s, 54, 4, 0, 0) { Color1 = Hex(0xD84315); Color2 = Hex(0x5D4037); } }
public sealed class X2406Plugin : QuickShapePlugin { public override string Name => "演出：キッチン 鍋"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2406Parameter(s); }
public sealed class X2406Parameter : Q6Art { public X2406Parameter() : this(null) { } public X2406Parameter(SharedDataStore? s) : base(s, 54, 5, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x5B6270); } }
public sealed class X2407Plugin : QuickShapePlugin { public override string Name => "演出：キッチン やかん"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2407Parameter(s); }
public sealed class X2407Parameter : Q6Art { public X2407Parameter() : this(null) { } public X2407Parameter(SharedDataStore? s) : base(s, 54, 6, 0, 0) { Color1 = Hex(0xB8C0CC); Color2 = Hex(0x5B6270); } }
public sealed class X2408Plugin : QuickShapePlugin { public override string Name => "演出：キッチン フォーク"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2408Parameter(s); }
public sealed class X2408Parameter : Q6Art { public X2408Parameter() : this(null) { } public X2408Parameter(SharedDataStore? s) : base(s, 54, 7, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x9AA3AD); } }
public sealed class X2409Plugin : QuickShapePlugin { public override string Name => "演出：キッチン スプーン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2409Parameter(s); }
public sealed class X2409Parameter : Q6Art { public X2409Parameter() : this(null) { } public X2409Parameter(SharedDataStore? s) : base(s, 54, 8, 0, 0) { Color1 = Hex(0x35A86B); Color2 = Hex(0x9AA3AD); } }
public sealed class X2410Plugin : QuickShapePlugin { public override string Name => "演出：キッチン ナイフ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2410Parameter(s); }
public sealed class X2410Parameter : Q6Art { public X2410Parameter() : this(null) { } public X2410Parameter(SharedDataStore? s) : base(s, 54, 9, 0, 0) { Color1 = Hex(0x8D5A3B); Color2 = Hex(0x9AA3AD); } }
public sealed class X2411Plugin : QuickShapePlugin { public override string Name => "演出：文房具 鉛筆"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2411Parameter(s); }
public sealed class X2411Parameter : Q6Art { public X2411Parameter() : this(null) { } public X2411Parameter(SharedDataStore? s) : base(s, 55, 0, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xE8488A); } }
public sealed class X2412Plugin : QuickShapePlugin { public override string Name => "演出：文房具 ボールペン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2412Parameter(s); }
public sealed class X2412Parameter : Q6Art { public X2412Parameter() : this(null) { } public X2412Parameter(SharedDataStore? s) : base(s, 55, 1, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x1F5FAF); } }
public sealed class X2413Plugin : QuickShapePlugin { public override string Name => "演出：文房具 消しゴム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2413Parameter(s); }
public sealed class X2413Parameter : Q6Art { public X2413Parameter() : this(null) { } public X2413Parameter(SharedDataStore? s) : base(s, 55, 2, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x1F2329); } }
public sealed class X2414Plugin : QuickShapePlugin { public override string Name => "演出：文房具 定規"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2414Parameter(s); }
public sealed class X2414Parameter : Q6Art { public X2414Parameter() : this(null) { } public X2414Parameter(SharedDataStore? s) : base(s, 55, 3, 0, 0) { Color1 = Hex(0xBFE6FF); Color2 = Hex(0x5B6270); } }
public sealed class X2415Plugin : QuickShapePlugin { public override string Name => "演出：文房具 はさみ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2415Parameter(s); }
public sealed class X2415Parameter : Q6Art { public X2415Parameter() : this(null) { } public X2415Parameter(SharedDataStore? s) : base(s, 55, 4, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x5B6270); } }
public sealed class X2416Plugin : QuickShapePlugin { public override string Name => "演出：文房具 クリップ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2416Parameter(s); }
public sealed class X2416Parameter : Q6Art { public X2416Parameter() : this(null) { } public X2416Parameter(SharedDataStore? s) : base(s, 55, 5, 0, 0) { Color1 = Hex(0xB8C0CC); Color2 = Hex(0x5B6270); } }
public sealed class X2417Plugin : QuickShapePlugin { public override string Name => "演出：文房具 ホッチキス"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2417Parameter(s); }
public sealed class X2417Parameter : Q6Art { public X2417Parameter() : this(null) { } public X2417Parameter(SharedDataStore? s) : base(s, 55, 6, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x3A3F47); } }
public sealed class X2418Plugin : QuickShapePlugin { public override string Name => "演出：文房具 ノート"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2418Parameter(s); }
public sealed class X2418Parameter : Q6Art { public X2418Parameter() : this(null) { } public X2418Parameter(SharedDataStore? s) : base(s, 55, 7, 0, 0) { Color1 = Hex(0x35A86B); Color2 = Hex(0x23784B); } }
public sealed class X2419Plugin : QuickShapePlugin { public override string Name => "演出：文房具 画びょう"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2419Parameter(s); }
public sealed class X2419Parameter : Q6Art { public X2419Parameter() : this(null) { } public X2419Parameter(SharedDataStore? s) : base(s, 55, 8, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x9AA3AD); } }
public sealed class X2420Plugin : QuickShapePlugin { public override string Name => "演出：文房具 三角定規"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2420Parameter(s); }
public sealed class X2420Parameter : Q6Art { public X2420Parameter() : this(null) { } public X2420Parameter(SharedDataStore? s) : base(s, 55, 9, 0, 0) { Color1 = Hex(0x8FD3FF); Color2 = Hex(0x2F86C6); } }
public sealed class X2421Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ キャンディ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2421Parameter(s); }
public sealed class X2421Parameter : Q6Art { public X2421Parameter() : this(null) { } public X2421Parameter(SharedDataStore? s) : base(s, 56, 0, 0, 0) { Color1 = Hex(0xE8488A); Color2 = Hex(0xFF9EC4); } }
public sealed class X2422Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ ぺろぺろキャンディ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2422Parameter(s); }
public sealed class X2422Parameter : Q6Art { public X2422Parameter() : this(null) { } public X2422Parameter(SharedDataStore? s) : base(s, 56, 1, 0, 0) { Color1 = Hex(0xFFFFFF); Color2 = Hex(0xE53935); } }
public sealed class X2423Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ ドーナツ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2423Parameter(s); }
public sealed class X2423Parameter : Q6Art { public X2423Parameter() : this(null) { } public X2423Parameter(SharedDataStore? s) : base(s, 56, 2, 0, 0) { Color1 = Hex(0xF48FB1); Color2 = Hex(0xFFEB3B); } }
public sealed class X2424Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ カップケーキ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2424Parameter(s); }
public sealed class X2424Parameter : Q6Art { public X2424Parameter() : this(null) { } public X2424Parameter(SharedDataStore? s) : base(s, 56, 3, 0, 0) { Color1 = Hex(0xF8BBD0); Color2 = Hex(0x2F86C6); } }
public sealed class X2425Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ ソフトクリーム"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2425Parameter(s); }
public sealed class X2425Parameter : Q6Art { public X2425Parameter() : this(null) { } public X2425Parameter(SharedDataStore? s) : base(s, 56, 4, 0, 0) { Color1 = Hex(0xFFF3E0); Color2 = Hex(0xD9A066); } }
public sealed class X2426Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ マカロン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2426Parameter(s); }
public sealed class X2426Parameter : Q6Art { public X2426Parameter() : this(null) { } public X2426Parameter(SharedDataStore? s) : base(s, 56, 5, 0, 0) { Color1 = Hex(0xA5D6A7); Color2 = Hex(0xFFF3E0); } }
public sealed class X2427Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ クッキー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2427Parameter(s); }
public sealed class X2427Parameter : Q6Art { public X2427Parameter() : this(null) { } public X2427Parameter(SharedDataStore? s) : base(s, 56, 6, 0, 0) { Color1 = Hex(0xD9A066); Color2 = Hex(0x5D4037); } }
public sealed class X2428Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ 板チョコ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2428Parameter(s); }
public sealed class X2428Parameter : Q6Art { public X2428Parameter() : this(null) { } public X2428Parameter(SharedDataStore? s) : base(s, 56, 7, 0, 0) { Color1 = Hex(0x6D4C41); Color2 = Hex(0x3E2723); } }
public sealed class X2429Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ ショートケーキ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2429Parameter(s); }
public sealed class X2429Parameter : Q6Art { public X2429Parameter() : this(null) { } public X2429Parameter(SharedDataStore? s) : base(s, 56, 8, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2430Plugin : QuickShapePlugin { public override string Name => "演出：スイーツ プリン"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2430Parameter(s); }
public sealed class X2430Parameter : Q6Art { public X2430Parameter() : this(null) { } public X2430Parameter(SharedDataStore? s) : base(s, 56, 9, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x8D5A3B); } }
public sealed class X2431Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 電球"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2431Parameter(s); }
public sealed class X2431Parameter : Q6Art { public X2431Parameter() : this(null) { } public X2431Parameter(SharedDataStore? s) : base(s, 57, 0, 0, 0) { Color1 = Hex(0xFFE082); Color2 = Hex(0x5B6270); } }
public sealed class X2432Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 鍵"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2432Parameter(s); }
public sealed class X2432Parameter : Q6Art { public X2432Parameter() : this(null) { } public X2432Parameter(SharedDataStore? s) : base(s, 57, 1, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xB8860B); } }
public sealed class X2433Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 傘"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2433Parameter(s); }
public sealed class X2433Parameter : Q6Art { public X2433Parameter() : this(null) { } public X2433Parameter(SharedDataStore? s) : base(s, 57, 2, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x5D4037); } }
public sealed class X2434Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 本"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2434Parameter(s); }
public sealed class X2434Parameter : Q6Art { public X2434Parameter() : this(null) { } public X2434Parameter(SharedDataStore? s) : base(s, 57, 3, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0xFFFFFF); } }
public sealed class X2435Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 メガネ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2435Parameter(s); }
public sealed class X2435Parameter : Q6Art { public X2435Parameter() : this(null) { } public X2435Parameter(SharedDataStore? s) : base(s, 57, 4, 0, 0) { Color1 = Hex(0x3A3F47); Color2 = Hex(0x9AA3AD); } }
public sealed class X2436Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 ろうそく"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2436Parameter(s); }
public sealed class X2436Parameter : Q6Art { public X2436Parameter() : this(null) { } public X2436Parameter(SharedDataStore? s) : base(s, 57, 5, 0, 0) { Color1 = Hex(0xFFF3E0); Color2 = Hex(0xFFB300); } }
public sealed class X2437Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 ベル"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2437Parameter(s); }
public sealed class X2437Parameter : Q6Art { public X2437Parameter() : this(null) { } public X2437Parameter(SharedDataStore? s) : base(s, 57, 6, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xB8860B); } }
public sealed class X2438Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 虫めがね"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2438Parameter(s); }
public sealed class X2438Parameter : Q6Art { public X2438Parameter() : this(null) { } public X2438Parameter(SharedDataStore? s) : base(s, 57, 7, 0, 0) { Color1 = Hex(0x3A3F47); Color2 = Hex(0x5D4037); } }
public sealed class X2439Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 地球儀"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2439Parameter(s); }
public sealed class X2439Parameter : Q6Art { public X2439Parameter() : this(null) { } public X2439Parameter(SharedDataStore? s) : base(s, 57, 8, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0x8D6E63); } }
public sealed class X2440Plugin : QuickShapePlugin { public override string Name => "演出：家の小物 磁石"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2440Parameter(s); }
public sealed class X2440Parameter : Q6Art { public X2440Parameter() : this(null) { } public X2440Parameter(SharedDataStore? s) : base(s, 57, 9, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x2F86C6); } }
public sealed class X2441Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 0"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2441Parameter(s); }
public sealed class X2441Parameter : Q6Art { public X2441Parameter() : this(null) { } public X2441Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); Text = "0"; } }
public sealed class X2442Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 1"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2442Parameter(s); }
public sealed class X2442Parameter : Q6Art { public X2442Parameter() : this(null) { } public X2442Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0xFFFFFF); Text = "1"; } }
public sealed class X2443Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 2"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2443Parameter(s); }
public sealed class X2443Parameter : Q6Art { public X2443Parameter() : this(null) { } public X2443Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0xFDD835); Color2 = Hex(0xFFFFFF); Text = "2"; } }
public sealed class X2444Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 3"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2444Parameter(s); }
public sealed class X2444Parameter : Q6Art { public X2444Parameter() : this(null) { } public X2444Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xFFFFFF); Text = "3"; } }
public sealed class X2445Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 4"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2445Parameter(s); }
public sealed class X2445Parameter : Q6Art { public X2445Parameter() : this(null) { } public X2445Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xFFFFFF); Text = "4"; } }
public sealed class X2446Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 5"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2446Parameter(s); }
public sealed class X2446Parameter : Q6Art { public X2446Parameter() : this(null) { } public X2446Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0xFFFFFF); Text = "5"; } }
public sealed class X2447Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 6"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2447Parameter(s); }
public sealed class X2447Parameter : Q6Art { public X2447Parameter() : this(null) { } public X2447Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); Text = "6"; } }
public sealed class X2448Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 7"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2448Parameter(s); }
public sealed class X2448Parameter : Q6Art { public X2448Parameter() : this(null) { } public X2448Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0xFFFFFF); Text = "7"; } }
public sealed class X2449Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 8"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2449Parameter(s); }
public sealed class X2449Parameter : Q6Art { public X2449Parameter() : this(null) { } public X2449Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0xFDD835); Color2 = Hex(0xFFFFFF); Text = "8"; } }
public sealed class X2450Plugin : QuickShapePlugin { public override string Name => "演出：数字ブロック 9"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2450Parameter(s); }
public sealed class X2450Parameter : Q6Art { public X2450Parameter() : this(null) { } public X2450Parameter(SharedDataStore? s) : base(s, 58, 0, 0, 0) { Color1 = Hex(0x43A047); Color2 = Hex(0xFFFFFF); Text = "9"; } }
public sealed class X2451Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 雪だるま"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2451Parameter(s); }
public sealed class X2451Parameter : Q6Art { public X2451Parameter() : this(null) { } public X2451Parameter(SharedDataStore? s) : base(s, 59, 0, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2452Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 鯉のぼり"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2452Parameter(s); }
public sealed class X2452Parameter : Q6Art { public X2452Parameter() : this(null) { } public X2452Parameter(SharedDataStore? s) : base(s, 59, 1, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x1E88E5); } }
public sealed class X2453Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 門松"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2453Parameter(s); }
public sealed class X2453Parameter : Q6Art { public X2453Parameter() : this(null) { } public X2453Parameter(SharedDataStore? s) : base(s, 59, 2, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x2E7D32); } }
public sealed class X2454Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 クリスマスツリー"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2454Parameter(s); }
public sealed class X2454Parameter : Q6Art { public X2454Parameter() : this(null) { } public X2454Parameter(SharedDataStore? s) : base(s, 59, 3, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFDD835); } }
public sealed class X2455Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 七夕の笹飾り"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2455Parameter(s); }
public sealed class X2455Parameter : Q6Art { public X2455Parameter() : this(null) { } public X2455Parameter(SharedDataStore? s) : base(s, 59, 4, 0, 0) { Color1 = Hex(0xE8488A); Color2 = Hex(0x1E88E5); } }
public sealed class X2456Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 風鈴"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2456Parameter(s); }
public sealed class X2456Parameter : Q6Art { public X2456Parameter() : this(null) { } public X2456Parameter(SharedDataStore? s) : base(s, 59, 5, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xE53935); } }
public sealed class X2457Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 紅葉の葉"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2457Parameter(s); }
public sealed class X2457Parameter : Q6Art { public X2457Parameter() : this(null) { } public X2457Parameter(SharedDataStore? s) : base(s, 59, 6, 0, 0) { Color1 = Hex(0xE64A19); Color2 = Hex(0x8D3B1A); } }
public sealed class X2458Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 鏡餅"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2458Parameter(s); }
public sealed class X2458Parameter : Q6Art { public X2458Parameter() : this(null) { } public X2458Parameter(SharedDataStore? s) : base(s, 59, 7, 0, 0) { Color1 = Hex(0xFF8F00); Color2 = Hex(0xE53935); } }
public sealed class X2459Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 てるてる坊主"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2459Parameter(s); }
public sealed class X2459Parameter : Q6Art { public X2459Parameter() : this(null) { } public X2459Parameter(SharedDataStore? s) : base(s, 59, 8, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x2F86C6); } }
public sealed class X2460Plugin : QuickShapePlugin { public override string Name => "演出：季節の物 かき氷"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2460Parameter(s); }
public sealed class X2460Parameter : Q6Art { public X2460Parameter() : this(null) { } public X2460Parameter(SharedDataStore? s) : base(s, 59, 9, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0x43A047); } }
public sealed class X2461Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン OK"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2461Parameter(s); }
public sealed class X2461Parameter : Q6Art { public X2461Parameter() : this(null) { } public X2461Parameter(SharedDataStore? s) : base(s, 60, 0, 0, 0) { Color1 = Hex(0x35A86B); Color2 = Hex(0xFFFFFF); } }
public sealed class X2462Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン NG"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2462Parameter(s); }
public sealed class X2462Parameter : Q6Art { public X2462Parameter() : this(null) { } public X2462Parameter(SharedDataStore? s) : base(s, 60, 1, 0, 0) { Color1 = Hex(0xE53935); Color2 = Hex(0xFFFFFF); } }
public sealed class X2463Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン 注意"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2463Parameter(s); }
public sealed class X2463Parameter : Q6Art { public X2463Parameter() : this(null) { } public X2463Parameter(SharedDataStore? s) : base(s, 60, 2, 0, 0) { Color1 = Hex(0xF2A93B); Color2 = Hex(0xFFFFFF); } }
public sealed class X2464Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン 禁止"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2464Parameter(s); }
public sealed class X2464Parameter : Q6Art { public X2464Parameter() : this(null) { } public X2464Parameter(SharedDataStore? s) : base(s, 60, 3, 0, 0) { Color1 = Hex(0xD7263D); Color2 = Hex(0xFFFFFF); } }
public sealed class X2465Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン 情報"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2465Parameter(s); }
public sealed class X2465Parameter : Q6Art { public X2465Parameter() : this(null) { } public X2465Parameter(SharedDataStore? s) : base(s, 60, 4, 0, 0) { Color1 = Hex(0x2F86C6); Color2 = Hex(0xFFFFFF); } }
public sealed class X2466Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン 質問"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2466Parameter(s); }
public sealed class X2466Parameter : Q6Art { public X2466Parameter() : this(null) { } public X2466Parameter(SharedDataStore? s) : base(s, 60, 5, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0xFFFFFF); } }
public sealed class X2467Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン おすすめ"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2467Parameter(s); }
public sealed class X2467Parameter : Q6Art { public X2467Parameter() : this(null) { } public X2467Parameter(SharedDataStore? s) : base(s, 60, 6, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xFFFFFF); } }
public sealed class X2468Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン お気に入り"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2468Parameter(s); }
public sealed class X2468Parameter : Q6Art { public X2468Parameter() : this(null) { } public X2468Parameter(SharedDataStore? s) : base(s, 60, 7, 0, 0) { Color1 = Hex(0xE8488A); Color2 = Hex(0xFFFFFF); } }
public sealed class X2469Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン ひらめき"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2469Parameter(s); }
public sealed class X2469Parameter : Q6Art { public X2469Parameter() : this(null) { } public X2469Parameter(SharedDataStore? s) : base(s, 60, 8, 0, 0) { Color1 = Hex(0xF5B700); Color2 = Hex(0xFFFFFF); } }
public sealed class X2470Plugin : QuickShapePlugin { public override string Name => "演出：状態アイコン 人気"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2470Parameter(s); }
public sealed class X2470Parameter : Q6Art { public X2470Parameter() : this(null) { } public X2470Parameter(SharedDataStore? s) : base(s, 60, 9, 0, 0) { Color1 = Hex(0xFF5722); Color2 = Hex(0xFFFFFF); } }
public sealed class X2471Plugin : QuickShapePlugin { public override string Name => "演出：惑星 太陽"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2471Parameter(s); }
public sealed class X2471Parameter : Q6Art { public X2471Parameter() : this(null) { } public X2471Parameter(SharedDataStore? s) : base(s, 61, 0, 0, 0) { Color1 = Hex(0xFFC107); Color2 = Hex(0xFF6F00); } }
public sealed class X2472Plugin : QuickShapePlugin { public override string Name => "演出：惑星 水星"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2472Parameter(s); }
public sealed class X2472Parameter : Q6Art { public X2472Parameter() : this(null) { } public X2472Parameter(SharedDataStore? s) : base(s, 61, 1, 0, 0) { Color1 = Hex(0x9E9E9E); Color2 = Hex(0x616161); } }
public sealed class X2473Plugin : QuickShapePlugin { public override string Name => "演出：惑星 金星"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2473Parameter(s); }
public sealed class X2473Parameter : Q6Art { public X2473Parameter() : this(null) { } public X2473Parameter(SharedDataStore? s) : base(s, 61, 2, 0, 0) { Color1 = Hex(0xE6C27A); Color2 = Hex(0xB08D57); } }
public sealed class X2474Plugin : QuickShapePlugin { public override string Name => "演出：惑星 地球"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2474Parameter(s); }
public sealed class X2474Parameter : Q6Art { public X2474Parameter() : this(null) { } public X2474Parameter(SharedDataStore? s) : base(s, 61, 3, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0x43A047); } }
public sealed class X2475Plugin : QuickShapePlugin { public override string Name => "演出：惑星 火星"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2475Parameter(s); }
public sealed class X2475Parameter : Q6Art { public X2475Parameter() : this(null) { } public X2475Parameter(SharedDataStore? s) : base(s, 61, 4, 0, 0) { Color1 = Hex(0xD84315); Color2 = Hex(0x8D3B1A); } }
public sealed class X2476Plugin : QuickShapePlugin { public override string Name => "演出：惑星 木星"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2476Parameter(s); }
public sealed class X2476Parameter : Q6Art { public X2476Parameter() : this(null) { } public X2476Parameter(SharedDataStore? s) : base(s, 61, 5, 0, 0) { Color1 = Hex(0xD7B48A); Color2 = Hex(0x8D5A3B); } }
public sealed class X2477Plugin : QuickShapePlugin { public override string Name => "演出：惑星 土星"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2477Parameter(s); }
public sealed class X2477Parameter : Q6Art { public X2477Parameter() : this(null) { } public X2477Parameter(SharedDataStore? s) : base(s, 61, 6, 0, 0) { Color1 = Hex(0xE6CF8B); Color2 = Hex(0xB89A5A); } }
public sealed class X2478Plugin : QuickShapePlugin { public override string Name => "演出：惑星 天王星"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2478Parameter(s); }
public sealed class X2478Parameter : Q6Art { public X2478Parameter() : this(null) { } public X2478Parameter(SharedDataStore? s) : base(s, 61, 7, 0, 0) { Color1 = Hex(0x80DEEA); Color2 = Hex(0x4DB6AC); } }
public sealed class X2479Plugin : QuickShapePlugin { public override string Name => "演出：惑星 海王星"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2479Parameter(s); }
public sealed class X2479Parameter : Q6Art { public X2479Parameter() : this(null) { } public X2479Parameter(SharedDataStore? s) : base(s, 61, 8, 0, 0) { Color1 = Hex(0x3949AB); Color2 = Hex(0x1A237E); } }
public sealed class X2480Plugin : QuickShapePlugin { public override string Name => "演出：惑星 月"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2480Parameter(s); }
public sealed class X2480Parameter : Q6Art { public X2480Parameter() : this(null) { } public X2480Parameter(SharedDataStore? s) : base(s, 61, 9, 0, 0) { Color1 = Hex(0xBDBDBD); Color2 = Hex(0x757575); } }
public sealed class X2481Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 済"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2481Parameter(s); }
public sealed class X2481Parameter : Q6Art { public X2481Parameter() : this(null) { } public X2481Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0xD7263D); Color2 = Hex(0xFFFFFF); Text = "済"; } }
public sealed class X2482Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 確認"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2482Parameter(s); }
public sealed class X2482Parameter : Q6Art { public X2482Parameter() : this(null) { } public X2482Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0xD7263D); Color2 = Hex(0xFFFFFF); Text = "確認"; } }
public sealed class X2483Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 合格"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2483Parameter(s); }
public sealed class X2483Parameter : Q6Art { public X2483Parameter() : this(null) { } public X2483Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0xD7263D); Color2 = Hex(0xFFFFFF); Text = "合格"; } }
public sealed class X2484Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 重要"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2484Parameter(s); }
public sealed class X2484Parameter : Q6Art { public X2484Parameter() : this(null) { } public X2484Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0xD7263D); Color2 = Hex(0xFFFFFF); Text = "重要"; } }
public sealed class X2485Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 秘"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2485Parameter(s); }
public sealed class X2485Parameter : Q6Art { public X2485Parameter() : this(null) { } public X2485Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0xD7263D); Color2 = Hex(0xFFFFFF); Text = "秘"; } }
public sealed class X2486Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 完了"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2486Parameter(s); }
public sealed class X2486Parameter : Q6Art { public X2486Parameter() : this(null) { } public X2486Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xFFFFFF); Text = "完了"; } }
public sealed class X2487Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 未"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2487Parameter(s); }
public sealed class X2487Parameter : Q6Art { public X2487Parameter() : this(null) { } public X2487Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0x1E88E5); Color2 = Hex(0xFFFFFF); Text = "未"; } }
public sealed class X2488Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 検"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2488Parameter(s); }
public sealed class X2488Parameter : Q6Art { public X2488Parameter() : this(null) { } public X2488Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0x35A86B); Color2 = Hex(0xFFFFFF); Text = "検"; } }
public sealed class X2489Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 可"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2489Parameter(s); }
public sealed class X2489Parameter : Q6Art { public X2489Parameter() : this(null) { } public X2489Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0x35A86B); Color2 = Hex(0xFFFFFF); Text = "可"; } }
public sealed class X2490Plugin : QuickShapePlugin { public override string Name => "演出：スタンプ 否"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2490Parameter(s); }
public sealed class X2490Parameter : Q6Art { public X2490Parameter() : this(null) { } public X2490Parameter(SharedDataStore? s) : base(s, 62, 0, 0, 0) { Color1 = Hex(0x7E57C2); Color2 = Hex(0xFFFFFF); Text = "否"; } }
public sealed class X2491Plugin : QuickShapePlugin { public override string Name => "演出：お金 硬貨 1"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2491Parameter(s); }
public sealed class X2491Parameter : Q6Art { public X2491Parameter() : this(null) { } public X2491Parameter(SharedDataStore? s) : base(s, 63, 0, 0, 0) { Color1 = Hex(0xD9DEE6); Color2 = Hex(0x8E99A8); Text = "1"; } }
public sealed class X2492Plugin : QuickShapePlugin { public override string Name => "演出：お金 硬貨 5"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2492Parameter(s); }
public sealed class X2492Parameter : Q6Art { public X2492Parameter() : this(null) { } public X2492Parameter(SharedDataStore? s) : base(s, 63, 1, 0, 0) { Color1 = Hex(0xE6C27A); Color2 = Hex(0xA0763A); Text = "5"; } }
public sealed class X2493Plugin : QuickShapePlugin { public override string Name => "演出：お金 硬貨 10"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2493Parameter(s); }
public sealed class X2493Parameter : Q6Art { public X2493Parameter() : this(null) { } public X2493Parameter(SharedDataStore? s) : base(s, 63, 2, 0, 0) { Color1 = Hex(0xC8804A); Color2 = Hex(0x8B4F22); Text = "10"; } }
public sealed class X2494Plugin : QuickShapePlugin { public override string Name => "演出：お金 硬貨 50"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2494Parameter(s); }
public sealed class X2494Parameter : Q6Art { public X2494Parameter() : this(null) { } public X2494Parameter(SharedDataStore? s) : base(s, 63, 3, 0, 0) { Color1 = Hex(0xD9DEE6); Color2 = Hex(0x8E99A8); Text = "50"; } }
public sealed class X2495Plugin : QuickShapePlugin { public override string Name => "演出：お金 硬貨 100"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2495Parameter(s); }
public sealed class X2495Parameter : Q6Art { public X2495Parameter() : this(null) { } public X2495Parameter(SharedDataStore? s) : base(s, 63, 4, 0, 0) { Color1 = Hex(0xD9DEE6); Color2 = Hex(0x8E99A8); Text = "100"; } }
public sealed class X2496Plugin : QuickShapePlugin { public override string Name => "演出：お金 硬貨 500"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2496Parameter(s); }
public sealed class X2496Parameter : Q6Art { public X2496Parameter() : this(null) { } public X2496Parameter(SharedDataStore? s) : base(s, 63, 5, 0, 0) { Color1 = Hex(0xE6C27A); Color2 = Hex(0xA0763A); Text = "500"; } }
public sealed class X2497Plugin : QuickShapePlugin { public override string Name => "演出：お金 お札"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2497Parameter(s); }
public sealed class X2497Parameter : Q6Art { public X2497Parameter() : this(null) { } public X2497Parameter(SharedDataStore? s) : base(s, 63, 6, 0, 0) { Color1 = Hex(0xBCE3C5); Color2 = Hex(0x2E7D32); Text = "1000"; } }
public sealed class X2498Plugin : QuickShapePlugin { public override string Name => "演出：お金 財布"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2498Parameter(s); }
public sealed class X2498Parameter : Q6Art { public X2498Parameter() : this(null) { } public X2498Parameter(SharedDataStore? s) : base(s, 63, 7, 0, 0) { Color1 = Hex(0x8D5A3B); Color2 = Hex(0xF2C14E); } }
public sealed class X2499Plugin : QuickShapePlugin { public override string Name => "演出：お金 金貨の山"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2499Parameter(s); }
public sealed class X2499Parameter : Q6Art { public X2499Parameter() : this(null) { } public X2499Parameter(SharedDataStore? s) : base(s, 63, 8, 0, 0) { Color1 = Hex(0xF2C14E); Color2 = Hex(0xB8860B); } }
public sealed class X2500Plugin : QuickShapePlugin { public override string Name => "演出：お金 ぶたの貯金箱"; public override IShapeParameter CreateShapeParameter(SharedDataStore? s) => new X2500Parameter(s); }
public sealed class X2500Parameter : Q6Art { public X2500Parameter() : this(null) { } public X2500Parameter(SharedDataStore? s) : base(s, 63, 9, 0, 0) { Color1 = Hex(0xF8A5C2); Color2 = Hex(0xE8488A); } }
