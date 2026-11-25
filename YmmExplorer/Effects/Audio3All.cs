using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 101〜200(tools で生成)

internal sealed class ModDelayK2(FxA2 e) : KB(e) { readonly PitchShifter x = new(), y = new(); Lfo p; protected override void Init() { x.Configure(Hz, 45); y.Configure(Hz, 45); x.Reset(); y.Reset(); p.Reset(); } protected override void Run(ref float l, ref float r) { p.Step(6.5, Hz); double q = Math.Pow(2, (2 + p.Sin * (.3 + A)) / 12); l = x.Run(l, q); r = y.Run(r, q); } }
internal sealed class GainLfoK2(FxA2 e) : KB(e) { Lfo p; protected override void Init() => p.Reset(); protected override void Run(ref float l, ref float r) { p.Step(1 + A * 7, Hz); float w = (float)p.Sin * .45f; l *= 1 - w; r *= 1 + w; } }

[AudioEffect("こもらせる", ["演出：音質"], ["こもらせる", "布をかぶせたようにこもる", "音質", "演出"])]
public sealed class Fx3A101Effect : FxA2 { public override string Label => "こもらせる"; internal override AudioKernel CreateKernel() => new BandK(this, 40, 1400); }

[AudioEffect("シャリシャリ", ["演出：音質"], ["シャリシャリ", "低音を切った硬い音", "音質", "演出"])]
public sealed class Fx3A102Effect : FxA2 { public override string Label => "シャリシャリ"; internal override AudioKernel CreateKernel() => new BandK(this, 1800, 16000, 2); }

[AudioEffect("中域だけ", ["演出：音質"], ["中域だけ", "細い中域だけの音", "音質", "演出"])]
public sealed class Fx3A103Effect : FxA2 { public override string Label => "中域だけ"; internal override AudioKernel CreateKernel() => new BandK(this, 500, 2600, 3); }

[AudioEffect("重低音だけ", ["演出：音質"], ["重低音だけ", "低い音だけを残す", "音質", "演出"])]
public sealed class Fx3A104Effect : FxA2 { public override string Label => "重低音だけ"; internal override AudioKernel CreateKernel() => new BandK(this, 25, 220, 2); }

[AudioEffect("ナレーション向け", ["演出：音質"], ["ナレーション向け", "低めで聞き取りやすい語りの音", "音質", "演出"])]
public sealed class Fx3A105Effect : FxA2 { public override string Label => "ナレーション向け"; internal override AudioKernel CreateKernel() => new EqK(this, (110, .8, 2.5), (320, 1, -2), (3400, .9, 4)); }

[AudioEffect("実況向け", ["演出：音質"], ["実況向け", "前に出るはっきりした声", "音質", "演出"])]
public sealed class Fx3A106Effect : FxA2 { public override string Label => "実況向け"; internal override AudioKernel CreateKernel() => new EqK(this, (180, .9, 1.5), (2200, .9, 5), (7000, .8, 2)); }

[AudioEffect("女性ボイス向け", ["演出：音質"], ["女性ボイス向け", "透明感を足す", "音質", "演出"])]
public sealed class Fx3A107Effect : FxA2 { public override string Label => "女性ボイス向け"; internal override AudioKernel CreateKernel() => new EqK(this, (260, 1, -2), (3800, 1, 3), (10000, .7, 4)); }

[AudioEffect("男性ボイス向け", ["演出：音質"], ["男性ボイス向け", "太さと通りを足す", "音質", "演出"])]
public sealed class Fx3A108Effect : FxA2 { public override string Label => "男性ボイス向け"; internal override AudioKernel CreateKernel() => new EqK(this, (130, .9, 3.5), (420, 1.1, -3), (2800, 1, 3)); }

[AudioEffect("合成音声を自然に", ["演出：音質"], ["合成音声を自然に", "機械的な高域をならす", "音質", "演出"])]
public sealed class Fx3A109Effect : FxA2 { public override string Label => "合成音声を自然に"; internal override AudioKernel CreateKernel() => new EqK(this, (220, .9, 1.5), (5600, 1.8, -3), (9500, .8, -3)); }

[AudioEffect("BGM向け", ["演出：音質"], ["BGM向け", "声とぶつかる帯域を空ける", "音質", "演出"])]
public sealed class Fx3A110Effect : FxA2 { public override string Label => "BGM向け"; internal override AudioKernel CreateKernel() => new EqK(this, (1800, .7, -6), (3200, .9, -4)); }

[AudioEffect("効果音向け", ["演出：音質"], ["効果音向け", "輪郭と迫力を足す", "音質", "演出"])]
public sealed class Fx3A111Effect : FxA2 { public override string Label => "効果音向け"; internal override AudioKernel CreateKernel() => new EqK(this, (75, .8, 4), (4300, .9, 4)); }

[AudioEffect("空気感を足す", ["演出：音質"], ["空気感を足す", "一番高い帯域を持ち上げる", "音質", "演出"])]
public sealed class Fx3A112Effect : FxA2 { public override string Label => "空気感を足す"; internal override AudioKernel CreateKernel() => new EqK(this, (14000, .6, 7)); }

[AudioEffect("鼻声を取る", ["演出：音質"], ["鼻声を取る", "鼻にかかった帯域を下げる", "音質", "演出"])]
public sealed class Fx3A113Effect : FxA2 { public override string Label => "鼻声を取る"; internal override AudioKernel CreateKernel() => new EqK(this, (950, 1.5, -7)); }

[AudioEffect("耳に痛い音を下げる", ["演出：音質"], ["耳に痛い音を下げる", "きつい高域をやわらげる", "音質", "演出"])]
public sealed class Fx3A114Effect : FxA2 { public override string Label => "耳に痛い音を下げる"; internal override AudioKernel CreateKernel() => new EqK(this, (3200, 1.6, -5), (6500, 1.6, -4)); }

[AudioEffect("箱鳴りを取る", ["演出：音質"], ["箱鳴りを取る", "狭い部屋のボワつきを下げる", "音質", "演出"])]
public sealed class Fx3A115Effect : FxA2 { public override string Label => "箱鳴りを取る"; internal override AudioKernel CreateKernel() => new EqK(this, (230, 2.2, -6), (480, 2.2, -4)); }

[AudioEffect("ハムノイズを下げる", ["演出：音質"], ["ハムノイズを下げる", "電源の低いブーンを狙って下げる", "音質", "演出"])]
public sealed class Fx3A116Effect : FxA2 { public override string Label => "ハムノイズを下げる"; internal override AudioKernel CreateKernel() => new EqK(this, (50, 7, -9), (60, 7, -9), (100, 7, -6), (120, 7, -6)); }

[AudioEffect("ラウドネス", ["演出：音質"], ["ラウドネス", "小音量でも聞きやすい山谷", "音質", "演出"])]
public sealed class Fx3A117Effect : FxA2 { public override string Label => "ラウドネス"; internal override AudioKernel CreateKernel() => new EqK(this, (70, .8, 5), (1000, .6, -2), (11000, .7, 4)); }

[AudioEffect("ビンテージEQ", ["演出：音質"], ["ビンテージEQ", "中域を丸く盛り上げる昔の音", "音質", "演出"])]
public sealed class Fx3A118Effect : FxA2 { public override string Label => "ビンテージEQ"; internal override AudioKernel CreateKernel() => new EqK(this, (700, .5, 5), (9000, .8, -6)); }

[AudioEffect("クリスタル", ["演出：音質"], ["クリスタル", "高域がきらめく音", "音質", "演出"])]
public sealed class Fx3A119Effect : FxA2 { public override string Label => "クリスタル"; internal override AudioKernel CreateKernel() => new EqK(this, (5000, 1, 3), (12000, .8, 7)); }

[AudioEffect("ウォームベース", ["演出：音質"], ["ウォームベース", "低域を温かく太らせる", "音質", "演出"])]
public sealed class Fx3A120Effect : FxA2 { public override string Label => "ウォームベース"; internal override AudioKernel CreateKernel() => new EqK(this, (100, .7, 5), (260, .8, 3), (7500, .8, -3)); }

[AudioEffect("子どもの声", ["演出：声のキャラ"], ["子どもの声", "高く小さな子の声", "声のキャラ", "演出"])]
public sealed class Fx3A121Effect : FxA2 { public override string Label => "子どもの声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 5, 170, 12000, 0, 0); }

[AudioEffect("おじいさんの声", ["演出：声のキャラ"], ["おじいさんの声", "低く細い声", "声のキャラ", "演出"])]
public sealed class Fx3A122Effect : FxA2 { public override string Label => "おじいさんの声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 240, 5200, .15f, 0, .3, 1500, 3); }

[AudioEffect("おばあさんの声", ["演出：声のキャラ"], ["おばあさんの声", "高く細い声", "声のキャラ", "演出"])]
public sealed class Fx3A123Effect : FxA2 { public override string Label => "おばあさんの声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 2, 320, 5000, .12f, 0, .3, 1700, 4); }

[AudioEffect("小人の声", ["演出：声のキャラ"], ["小人の声", "とても高く軽い声", "声のキャラ", "演出"])]
public sealed class Fx3A124Effect : FxA2 { public override string Label => "小人の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 11, 240, 15000, 0, 0); }

[AudioEffect("大男の声", ["演出：声のキャラ"], ["大男の声", "低く太い声", "声のキャラ", "演出"])]
public sealed class Fx3A125Effect : FxA2 { public override string Label => "大男の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -5, 45, 6500, .1f, 0, .3, 180, 4); }

[AudioEffect("魔王の声", ["演出：声のキャラ"], ["魔王の声", "低く響き渡る声", "声のキャラ", "演出"])]
public sealed class Fx3A126Effect : FxA2 { public override string Label => "魔王の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -8, 45, 4800, .5f, .7f, .85, 200, 4); }

[AudioEffect("天使の声", ["演出：声のキャラ"], ["天使の声", "高く澄んで響く声", "声のキャラ", "演出"])]
public sealed class Fx3A127Effect : FxA2 { public override string Label => "天使の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 4, 260, 14000, 0, .75f, .9, 4200, 3); }

[AudioEffect("妖精の声", ["演出：声のキャラ"], ["妖精の声", "高くきらめく小さな声", "声のキャラ", "演出"])]
public sealed class Fx3A128Effect : FxA2 { public override string Label => "妖精の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 8, 420, 16000, 0, .5f, .7, 6000, 4); }

[AudioEffect("ドラゴンの声", ["演出：声のキャラ"], ["ドラゴンの声", "低く歪んだ唸り", "声のキャラ", "演出"])]
public sealed class Fx3A129Effect : FxA2 { public override string Label => "ドラゴンの声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -10, 50, 3600, 1.2f, .45f, .8, 150, 5); }

[AudioEffect("怪獣の声", ["演出：声のキャラ"], ["怪獣の声", "さらに低く割れた咆哮", "声のキャラ", "演出"])]
public sealed class Fx3A130Effect : FxA2 { public override string Label => "怪獣の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -12, 35, 3000, 1.8f, .4f, .75, 120, 6); }

[AudioEffect("人造人間の声", ["演出：声のキャラ"], ["人造人間の声", "硬く乾いた金属の声", "声のキャラ", "演出"])]
public sealed class Fx3A131Effect : FxA2 { public override string Label => "人造人間の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 420, 5200, .8f, .12f, .25, 1800, 7); }

[AudioEffect("AIアシスタント", ["演出：声のキャラ"], ["AIアシスタント", "整った少し機械的な声", "声のキャラ", "演出"])]
public sealed class Fx3A132Effect : FxA2 { public override string Label => "AIアシスタント"; internal override AudioKernel CreateKernel() => new VoiceK(this, 1, 200, 9000, .15f, .1f, .25, 2500, 4); }

[AudioEffect("ナレーター風", ["演出：声のキャラ"], ["ナレーター風", "低く落ち着いた声", "声のキャラ", "演出"])]
public sealed class Fx3A133Effect : FxA2 { public override string Label => "ナレーター風"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 70, 11000, .05f, .05f, .3, 150, 3); }

[AudioEffect("アニメ声", ["演出：声のキャラ"], ["アニメ声", "少し高く明るい声", "声のキャラ", "演出"])]
public sealed class Fx3A134Effect : FxA2 { public override string Label => "アニメ声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 3, 160, 13000, 0, 0, .3, 3200, 4); }

[AudioEffect("風邪声", ["演出：声のキャラ"], ["風邪声", "鼻が詰まったような声", "声のキャラ", "演出"])]
public sealed class Fx3A135Effect : FxA2 { public override string Label => "風邪声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 170, 2600, .1f, 0, .3, 800, 7); }

[AudioEffect("寝起きの声", ["演出：声のキャラ"], ["寝起きの声", "低くかすれた声", "声のキャラ", "演出"])]
public sealed class Fx3A136Effect : FxA2 { public override string Label => "寝起きの声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -3, 110, 3600, .35f, 0, .3, 420, 3); }

[AudioEffect("酔っぱらいの声", ["演出：声のキャラ"], ["酔っぱらいの声", "低くこもった声", "声のキャラ", "演出"])]
public sealed class Fx3A137Effect : FxA2 { public override string Label => "酔っぱらいの声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 120, 3000, .25f, .12f, .3, 600, 4); }

[AudioEffect("泣き声風", ["演出：声のキャラ"], ["泣き声風", "高く細く震える声", "声のキャラ", "演出"])]
public sealed class Fx3A138Effect : FxA2 { public override string Label => "泣き声風"; internal override AudioKernel CreateKernel() => new ModDelayK2(this); }

[AudioEffect("ロボット犬の声", ["演出：声のキャラ"], ["ロボット犬の声", "高く割れた機械の声", "声のキャラ", "演出"])]
public sealed class Fx3A139Effect : FxA2 { public override string Label => "ロボット犬の声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 7, 650, 6500, 1.1f, .05f, .25, 2300, 6); }

[AudioEffect("地獄の合唱", ["演出：声のキャラ"], ["地獄の合唱", "低い声に長い響き", "声のキャラ", "演出"])]
public sealed class Fx3A140Effect : FxA2 { public override string Label => "地獄の合唱"; internal override AudioKernel CreateKernel() => new VoiceK(this, -9, 60, 3800, .7f, 1f, .95, 300, 3); }

[AudioEffect("トイレの中", ["演出：場所の声"], ["トイレの中", "狭いタイルの響き", "場所の声", "演出"])]
public sealed class Fx3A141Effect : FxA2 { public override string Label => "トイレの中"; internal override AudioKernel CreateKernel() => new RoomK(this, .46f, .12f, 3, .42f, 240, 8500, .95f, .85f); }

[AudioEffect("階段の踊り場", ["演出：場所の声"], ["階段の踊り場", "硬く広がる響き", "場所の声", "演出"])]
public sealed class Fx3A142Effect : FxA2 { public override string Label => "階段の踊り場"; internal override AudioKernel CreateKernel() => new RoomK(this, .7f, .15f, 18, .9f, 260, 7000, .85f, .9f); }

[AudioEffect("地下駐車場", ["演出：場所の声"], ["地下駐車場", "低く長いコンクリートの響き", "場所の声", "演出"])]
public sealed class Fx3A143Effect : FxA2 { public override string Label => "地下駐車場"; internal override AudioKernel CreateKernel() => new RoomK(this, .88f, .22f, 38, 1.5f, 120, 4200, 1f, .78f); }

[AudioEffect("地下鉄の駅", ["演出：場所の声"], ["地下鉄の駅", "広く反射するホーム", "場所の声", "演出"])]
public sealed class Fx3A144Effect : FxA2 { public override string Label => "地下鉄の駅"; internal override AudioKernel CreateKernel() => new RoomK(this, .86f, .2f, 55, 1.7f, 240, 5200, 1f, .72f); }

[AudioEffect("教室", ["演出：場所の声"], ["教室", "少し響く学校の部屋", "場所の声", "演出"])]
public sealed class Fx3A145Effect : FxA2 { public override string Label => "教室"; internal override AudioKernel CreateKernel() => new RoomK(this, .55f, .45f, 12, .85f, 180, 7500, .55f); }

[AudioEffect("音楽室", ["演出：場所の声"], ["音楽室", "吸音された落ち着いた響き", "場所の声", "演出"])]
public sealed class Fx3A146Effect : FxA2 { public override string Label => "音楽室"; internal override AudioKernel CreateKernel() => new RoomK(this, .42f, .85f, 10, .8f, 180, 6500, .42f); }

[AudioEffect("図書室", ["演出：場所の声"], ["図書室", "静かで短い響き", "場所の声", "演出"])]
public sealed class Fx3A147Effect : FxA2 { public override string Label => "図書室"; internal override AudioKernel CreateKernel() => new RoomK(this, .22f, .9f, 4, .5f, 180, 5200, .35f); }

[AudioEffect("ライブハウス", ["演出：場所の声"], ["ライブハウス", "暗く近い音の響き", "場所の声", "演出"])]
public sealed class Fx3A148Effect : FxA2 { public override string Label => "ライブハウス"; internal override AudioKernel CreateKernel() => new RoomK(this, .62f, .55f, 22, 1f, 120, 5500, .75f); }

[AudioEffect("コンサートホール", ["演出：場所の声"], ["コンサートホール", "豊かで長い響き", "場所の声", "演出"])]
public sealed class Fx3A149Effect : FxA2 { public override string Label => "コンサートホール"; internal override AudioKernel CreateKernel() => new RoomK(this, .9f, .38f, 45, 1.8f, 140, 7500, .9f, .82f); }

[AudioEffect("野外スタジアム", ["演出：場所の声"], ["野外スタジアム", "遠くへ抜ける反響", "場所の声", "演出"])]
public sealed class Fx3A150Effect : FxA2 { public override string Label => "野外スタジアム"; internal override AudioKernel CreateKernel() => new EchoK(this, .22, .27, .42f, 350, 3600, .55f); }

[AudioEffect("神社の境内", ["演出：場所の声"], ["神社の境内", "静かに抜ける響き", "場所の声", "演出"])]
public sealed class Fx3A151Effect : FxA2 { public override string Label => "神社の境内"; internal override AudioKernel CreateKernel() => new RoomK(this, .7f, .6f, 60, 1.4f, 240, 5000, .45f); }

[AudioEffect("広い洞窟", ["演出：場所の声"], ["広い洞窟", "長く暗い残響", "場所の声", "演出"])]
public sealed class Fx3A152Effect : FxA2 { public override string Label => "広い洞窟"; internal override AudioKernel CreateKernel() => new RoomK(this, .98f, .35f, 80, 2.4f, 90, 3600, 1.2f, .7f); }

[AudioEffect("井戸の底", ["演出：場所の声"], ["井戸の底", "筒状に鳴る響き", "場所の声", "演出"])]
public sealed class Fx3A153Effect : FxA2 { public override string Label => "井戸の底"; internal override AudioKernel CreateKernel() => new EchoK(this, .045, .052, .78f, 180, 2400, .85f); }

[AudioEffect("土管の中", ["演出：場所の声"], ["土管の中", "金属的に共鳴する音", "場所の声", "演出"])]
public sealed class Fx3A154Effect : FxA2 { public override string Label => "土管の中"; internal override AudioKernel CreateKernel() => new EchoK(this, .012, .015, .85f, 300, 3000, .9f); }

[AudioEffect("屋根裏", ["演出：場所の声"], ["屋根裏", "木の箱のような響き", "場所の声", "演出"])]
public sealed class Fx3A155Effect : FxA2 { public override string Label => "屋根裏"; internal override AudioKernel CreateKernel() => new RoomK(this, .32f, .75f, 4, .55f, 200, 3600, .6f); }

[AudioEffect("プールサイド", ["演出：場所の声"], ["プールサイド", "水面に反射する響き", "場所の声", "演出"])]
public sealed class Fx3A156Effect : FxA2 { public override string Label => "プールサイド"; internal override AudioKernel CreateKernel() => new RoomK(this, .82f, .1f, 32, 1.3f, 300, 9000, .9f, .84f); }

[AudioEffect("雪の中", ["演出：場所の声"], ["雪の中", "音が吸われた静かな響き", "場所の声", "演出"])]
public sealed class Fx3A157Effect : FxA2 { public override string Label => "雪の中"; internal override AudioKernel CreateKernel() => new BandK(this, 60, 3800, -1); }

[AudioEffect("飛行機の機内", ["演出：場所の声"], ["飛行機の機内", "低い音にこもった放送", "場所の声", "演出"])]
public sealed class Fx3A158Effect : FxA2 { public override string Label => "飛行機の機内"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 380, 3400, .45f, .08f, .25, 1400, 5); }

[AudioEffect("潜水艦の無線", ["演出：場所の声"], ["潜水艦の無線", "狭帯域で金属的な声", "場所の声", "演出"])]
public sealed class Fx3A159Effect : FxA2 { public override string Label => "潜水艦の無線"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 650, 2400, .8f, .35f, .4, 1200, 8); }

[AudioEffect("宇宙船の通信", ["演出：場所の声"], ["宇宙船の通信", "細く歪んだ交信", "場所の声", "演出"])]
public sealed class Fx3A160Effect : FxA2 { public override string Label => "宇宙船の通信"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 800, 3000, 1f, .08f, .25, 1800, 7); }

[AudioEffect("短いエコー", ["演出：エコー"], ["短いエコー", "一瞬の反響", "エコー", "演出"])]
public sealed class Fx3A161Effect : FxA2 { public override string Label => "短いエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .09, .11, .25f, 180, 7000, .55f); }

[AudioEffect("長いエコー", ["演出：エコー"], ["長いエコー", "ゆっくり返ってくる反響", "エコー", "演出"])]
public sealed class Fx3A162Effect : FxA2 { public override string Label => "長いエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .48, .52, .55f, 200, 5200, .6f); }

[AudioEffect("無限エコー", ["演出：エコー"], ["無限エコー", "なかなか消えない反響", "エコー", "演出"])]
public sealed class Fx3A163Effect : FxA2 { public override string Label => "無限エコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .32, .36, .88f, 200, 4200, .6f); }

[AudioEffect("暗いエコー", ["演出：エコー"], ["暗いエコー", "繰り返すほどこもる反響", "エコー", "演出"])]
public sealed class Fx3A164Effect : FxA2 { public override string Label => "暗いエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .3, .34, .62f, 90, 1600, .75f); }

[AudioEffect("明るいエコー", ["演出：エコー"], ["明るいエコー", "高い音だけが返る反響", "エコー", "演出"])]
public sealed class Fx3A165Effect : FxA2 { public override string Label => "明るいエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .24, .28, .5f, 1400, 12000, .6f); }

[AudioEffect("ダブディレイ", ["演出：エコー"], ["ダブディレイ", "左右を行き来する太いエコー", "エコー", "演出"])]
public sealed class Fx3A166Effect : FxA2 { public override string Label => "ダブディレイ"; internal override AudioKernel CreateKernel() => new EchoK(this, .36, .54, .7f, 160, 2300, .7f, true); }

[AudioEffect("リズムディレイ", ["演出：エコー"], ["リズムディレイ", "細かく刻まれる左右のエコー", "エコー", "演出"])]
public sealed class Fx3A167Effect : FxA2 { public override string Label => "リズムディレイ"; internal override AudioKernel CreateKernel() => new EchoK(this, .125, .25, .55f, 260, 7000, .6f, true); }

[AudioEffect("ダブリングエコー", ["演出：エコー"], ["ダブリングエコー", "ごく短いズレで厚くする", "エコー", "演出"])]
public sealed class Fx3A168Effect : FxA2 { public override string Label => "ダブリングエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .028, .041, .05f, 120, 12000, .8f); }

[AudioEffect("金属エコー", ["演出：エコー"], ["金属エコー", "短く共鳴する金属音", "エコー", "演出"])]
public sealed class Fx3A169Effect : FxA2 { public override string Label => "金属エコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .007, .011, .86f, 500, 9000, .85f, true); }

[AudioEffect("こだまの谷", ["演出：エコー"], ["こだまの谷", "遠くの山から返る声", "エコー", "演出"])]
public sealed class Fx3A170Effect : FxA2 { public override string Label => "こだまの谷"; internal override AudioKernel CreateKernel() => new EchoK(this, .62, .78, .45f, 420, 2600, .5f, true); }

[AudioEffect("アンサンブル", ["演出：変調"], ["アンサンブル", "大勢で鳴らすような広がり", "変調", "演出"])]
public sealed class Fx3A171Effect : FxA3 { public override string Label => "アンサンブル"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 24, 7, .2, 2, 0f, .9f); }

[AudioEffect("ゆるいコーラス", ["演出：変調"], ["ゆるいコーラス", "ゆったりした厚み", "変調", "演出"])]
public sealed class Fx3A172Effect : FxA3 { public override string Label => "ゆるいコーラス"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 30, 4, .08, .8, 0f, .7f); }

[AudioEffect("ジェットフランジャー", ["演出：変調"], ["ジェットフランジャー", "強いうねり", "変調", "演出"])]
public sealed class Fx3A173Effect : FxA3 { public override string Label => "ジェットフランジャー"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 4, 3, .05, .9, .85f, .95f); }

[AudioEffect("金属フランジャー", ["演出：変調"], ["金属フランジャー", "細かく速いうねり", "変調", "演出"])]
public sealed class Fx3A174Effect : FxA3 { public override string Label => "金属フランジャー"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 2, 1.4, .8, 8, .9f, .9f); }

[AudioEffect("ロータリー", ["演出：変調"], ["ロータリー", "回転するスピーカーの揺れ", "変調", "演出"])]
public sealed class Fx3A175Effect : FxA2 { public override string Label => "ロータリー"; internal override AudioKernel CreateKernel() => new GainLfoK2(this); }

[AudioEffect("ヘリコプター", ["演出：変調"], ["ヘリコプター", "バタバタと刻まれる音", "変調", "演出"])]
public sealed class Fx3A176Effect : FxA3 { public override string Label => "ヘリコプター"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 7, 26, 1); }

[AudioEffect("心臓の鼓動ゲート", ["演出：変調"], ["心臓の鼓動ゲート", "鼓動に合わせて音が出る", "変調", "演出"])]
public sealed class Fx3A177Effect : FxA3 { public override string Label => "心臓の鼓動ゲート"; internal override AudioKernel CreateKernel() => new GainLfoK(this, .7, 2.2, 3); }

[AudioEffect("のこぎりトレモロ", ["演出：変調"], ["のこぎりトレモロ", "ダッダッと減衰を繰り返す", "変調", "演出"])]
public sealed class Fx3A178Effect : FxA3 { public override string Label => "のこぎりトレモロ"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 1, 12, 2); }

[AudioEffect("左右トレモロ", ["演出：変調"], ["左右トレモロ", "左右で交互に音量が揺れる", "変調", "演出"])]
public sealed class Fx3A179Effect : FxA3 { public override string Label => "左右トレモロ"; internal override AudioKernel CreateKernel() => new GainLfoK(this, .5, 9, 0, true); }

[AudioEffect("ゆっくりフィルター", ["演出：変調"], ["ゆっくりフィルター", "こもりと開放がゆっくり続く", "変調", "演出"])]
public sealed class Fx3A180Effect : FxA3 { public override string Label => "ゆっくりフィルター"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 350, 12000, 0, 1.2, .05, 1); }

[AudioEffect("速いワウ", ["演出：変調"], ["速いワウ", "音色が細かく動く", "変調", "演出"])]
public sealed class Fx3A181Effect : FxA3 { public override string Label => "速いワウ"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 420, 3600, 1, 3, 2, 14); }

[AudioEffect("高域スイープ", ["演出：変調"], ["高域スイープ", "低音が抜けたり戻ったりする", "変調", "演出"])]
public sealed class Fx3A182Effect : FxA3 { public override string Label => "高域スイープ"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 60, 3600, 2, 1.1, .05, 1.2); }

[AudioEffect("水中の揺らぎ", ["演出：変調"], ["水中の揺らぎ", "こもった音がゆらゆら動く", "変調", "演出"])]
public sealed class Fx3A183Effect : FxA3 { public override string Label => "水中の揺らぎ"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 260, 1400, 0, 1.6, .2, 2.5); }

[AudioEffect("サイレン風フィルター", ["演出：変調"], ["サイレン風フィルター", "音色が上下に鳴り続ける", "変調", "演出"])]
public sealed class Fx3A184Effect : FxA3 { public override string Label => "サイレン風フィルター"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 500, 2600, 1, 5, .3, 3); }

[AudioEffect("しゃべるフィルター", ["演出：変調"], ["しゃべるフィルター", "母音のように音色が変わる", "変調", "演出"])]
public sealed class Fx3A185Effect : FxA3 { public override string Label => "しゃべるフィルター"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 500, 2400, 1, 6, .8, 5); }

[AudioEffect("船酔い", ["演出：変調"], ["船酔い", "音程がゆっくり大きくゆがむ", "変調", "演出"])]
public sealed class Fx3A186Effect : FxA3 { public override string Label => "船酔い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 26, 18, .08, .7, 0f, 12f); }

[AudioEffect("テープのよれ", ["演出：変調"], ["テープのよれ", "音程が不安定に揺れる", "変調", "演出"])]
public sealed class Fx3A187Effect : FxA3 { public override string Label => "テープのよれ"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 14, 5, .4, 3, 0f, 12f); }

[AudioEffect("宇宙フランジャー", ["演出：変調"], ["宇宙フランジャー", "深く遅いうねり", "変調", "演出"])]
public sealed class Fx3A188Effect : FxA3 { public override string Label => "宇宙フランジャー"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 7, 6, .03, .3, .92f, .95f); }

[AudioEffect("シャワーの音色", ["演出：変調"], ["シャワーの音色", "細かい揺れで水っぽくする", "変調", "演出"])]
public sealed class Fx3A189Effect : FxA3 { public override string Label => "シャワーの音色"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 9, 2, 3, 16, .3f, .8f); }

[AudioEffect("二重コーラス", ["演出：変調"], ["二重コーラス", "広く厚い重なり", "変調", "演出"])]
public sealed class Fx3A190Effect : FxA3 { public override string Label => "二重コーラス"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 40, 9, .15, 1.4, .15f, 1f); }

[AudioEffect("真空管", ["演出：歪み"], ["真空管", "やわらかく太い歪み", "歪み", "演出"])]
public sealed class Fx3A191Effect : FxA2 { public override string Label => "真空管"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => x >= 0 ? MathF.Tanh(x * 1.4f) : MathF.Tanh(x * .9f), 2, 16, 11000, 0); }

[AudioEffect("壊れたスピーカー", ["演出：歪み"], ["壊れたスピーカー", "ビリビリ割れた音", "歪み", "演出"])]
public sealed class Fx3A192Effect : FxA2 { public override string Label => "壊れたスピーカー"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Abs(x) > .25f ? MathF.Sign(x) * (.25f + (MathF.Abs(x) - .25f) * .12f) : x, 8, 28, 3500, 8); }

[AudioEffect("半波整流", ["演出：歪み"], ["半波整流", "片側だけ削った濁った音", "歪み", "演出"])]
public sealed class Fx3A193Effect : FxA2 { public override string Label => "半波整流"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Max(0f, MathF.Tanh(x)) * 1.5f, 2, 18, 6000, 3); }

[AudioEffect("全波整流", ["演出：歪み"], ["全波整流", "1オクターブ上がったような歪み", "歪み", "演出"])]
public sealed class Fx3A194Effect : FxA2 { public override string Label => "全波整流"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Abs(MathF.Tanh(x)) * 1.4f - .4f, 2, 16, 7000, 2); }

[AudioEffect("折り返し歪み", ["演出：歪み"], ["折り返し歪み", "金属的に折れ曲がる音", "歪み", "演出"])]
public sealed class Fx3A195Effect : FxA2 { public override string Label => "折り返し歪み"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Sin(x * 2.4f) * .7f, 2, 20, 9000, 2); }

[AudioEffect("量子化ノイズ", ["演出：歪み"], ["量子化ノイズ", "段々になった粗い音", "歪み", "演出"])]
public sealed class Fx3A196Effect : FxA2 { public override string Label => "量子化ノイズ"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Round(x * 6f) / 6f, 3, 12, 12000, 0); }

[AudioEffect("ハードクリップ", ["演出：歪み"], ["ハードクリップ", "角を切り落とした強い音", "歪み", "演出"])]
public sealed class Fx3A197Effect : FxA2 { public override string Label => "ハードクリップ"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => Math.Clamp(x, -.6f, .6f), 3, 24, 16000, 3); }

[AudioEffect("ソフトクリップ", ["演出：歪み"], ["ソフトクリップ", "耳あたりのよい頭打ち", "歪み", "演出"])]
public sealed class Fx3A198Effect : FxA2 { public override string Label => "ソフトクリップ"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => x / (1f + MathF.Abs(x)) * 1.5f, 0, 14, 18000, 0); }

[AudioEffect("ガレージ", ["演出：歪み"], ["ガレージ", "荒々しく潰れたロックの音", "歪み", "演出"])]
public sealed class Fx3A199Effect : FxA2 { public override string Label => "ガレージ"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Tanh(x * 3f), 8, 28, 4200, 0); }

[AudioEffect("電話線ノイズ", ["演出：歪み"], ["電話線ノイズ", "細く割れた回線の音", "歪み", "演出"])]
public sealed class Fx3A200Effect : FxA2 { public override string Label => "電話線ノイズ"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Round(MathF.Tanh(x * 2f) * 8f) / 8f, 4, 16, 3200, 1); }
