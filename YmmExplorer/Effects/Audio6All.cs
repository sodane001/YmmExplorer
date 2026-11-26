using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 601〜700(gen_v136.py で生成)

[AudioEffect("反響 山小屋", ["演出：エコー"], ["反響 山小屋", "山小屋の外で返る小さな反響", "エコー", "反響", "山小屋", "echo", "ディレイ", "山", "木", "演出"])]
public sealed class Fx6A601Effect : FxA2 { public override string Label => "反響 山小屋"; internal override AudioKernel CreateKernel() => new EchoK(this, .2, .24, .35f, 300, 5000, .4f); }

[AudioEffect("反響 渓谷", ["演出：エコー"], ["反響 渓谷", "深い渓谷にくり返し返る反響", "エコー", "反響", "渓谷", "echo", "ディレイ", "谷", "川", "演出"])]
public sealed class Fx6A602Effect : FxA2 { public override string Label => "反響 渓谷"; internal override AudioKernel CreateKernel() => new EchoK(this, .5, .58, .6f, 250, 3500, .55f); }

[AudioEffect("反響 ダムの上", ["演出：エコー"], ["反響 ダムの上", "大きな壁に遠く返る反響", "エコー", "反響", "ダムの上", "echo", "ディレイ", "ダム", "壁", "演出"])]
public sealed class Fx6A603Effect : FxA2 { public override string Label => "反響 ダムの上"; internal override AudioKernel CreateKernel() => new EchoK(this, .7, .75, .5f, 200, 3000, .5f); }

[AudioEffect("反響 地下通路", ["演出：エコー"], ["反響 地下通路", "地下の通路で細かく返る反響", "エコー", "反響", "地下通路", "echo", "ディレイ", "地下", "通路", "演出"])]
public sealed class Fx6A604Effect : FxA2 { public override string Label => "反響 地下通路"; internal override AudioKernel CreateKernel() => new EchoK(this, .1, .12, .65f, 250, 4000, .55f); }

[AudioEffect("反響 古いホール", ["演出：エコー"], ["反響 古いホール", "古いホールで左右に返る反響", "エコー", "反響", "古いホール", "echo", "ディレイ", "ホール", "古い", "演出"])]
public sealed class Fx6A605Effect : FxA2 { public override string Label => "反響 古いホール"; internal override AudioKernel CreateKernel() => new EchoK(this, .15, .19, .55f, 200, 4500, .5f, true); }

[AudioEffect("反響 金属のタンク", ["演出：エコー"], ["反響 金属のタンク", "タンクの中のような金属の反響", "エコー", "反響", "金属のタンク", "echo", "ディレイ", "金属", "タンク", "演出"])]
public sealed class Fx6A606Effect : FxA2 { public override string Label => "反響 金属のタンク"; internal override AudioKernel CreateKernel() => new EchoK(this, .04, .05, .8f, 500, 6000, .55f); }

[AudioEffect("反響 ガレージ", ["演出：エコー"], ["反響 ガレージ", "車庫の中の短い反響", "エコー", "反響", "ガレージ", "echo", "ディレイ", "車庫", "コンクリート", "演出"])]
public sealed class Fx6A607Effect : FxA2 { public override string Label => "反響 ガレージ"; internal override AudioKernel CreateKernel() => new EchoK(this, .08, .1, .55f, 200, 6000, .5f); }

[AudioEffect("反響 崖の上", ["演出：エコー"], ["反響 崖の上", "崖の下から遠く返る反響", "エコー", "反響", "崖の上", "echo", "ディレイ", "崖", "山", "演出"])]
public sealed class Fx6A608Effect : FxA2 { public override string Label => "反響 崖の上"; internal override AudioKernel CreateKernel() => new EchoK(this, .8, .9, .4f, 300, 3000, .45f); }

[AudioEffect("反響 湖のほとり", ["演出：エコー"], ["反響 湖のほとり", "静かな湖の向こうから返る反響", "エコー", "反響", "湖のほとり", "echo", "ディレイ", "湖", "静か", "演出"])]
public sealed class Fx6A609Effect : FxA2 { public override string Label => "反響 湖のほとり"; internal override AudioKernel CreateKernel() => new EchoK(this, .4, .5, .35f, 250, 5000, .4f); }

[AudioEffect("反響 雪山", ["演出：エコー"], ["反響 雪山", "雪山のこもった遠い反響", "エコー", "反響", "雪山", "echo", "ディレイ", "雪", "冬", "演出"])]
public sealed class Fx6A610Effect : FxA2 { public override string Label => "反響 雪山"; internal override AudioKernel CreateKernel() => new EchoK(this, .45, .5, .45f, 300, 2500, .45f); }

[AudioEffect("反響 ピンポン 中", ["演出：エコー"], ["反響 ピンポン 中", "左右にふつうの速さで跳ね返る", "エコー", "反響", "ピンポン", "中", "echo", "ディレイ", "ステレオ", "左右", "演出"])]
public sealed class Fx6A611Effect : FxA2 { public override string Label => "反響 ピンポン 中"; internal override AudioKernel CreateKernel() => new EchoK(this, .25, .5, .5f, 200, 8000, .55f, true); }

[AudioEffect("反響 16分エコー", ["演出：エコー"], ["反響 16分エコー", "16分音符のリズムで返る", "エコー", "反響", "16分エコー", "echo", "ディレイ", "リズム", "音楽", "演出"])]
public sealed class Fx6A612Effect : FxA2 { public override string Label => "反響 16分エコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .125, .125, .45f, 150, 9000, .5f); }

[AudioEffect("反響 8分エコー", ["演出：エコー"], ["反響 8分エコー", "8分音符のリズムで返る", "エコー", "反響", "8分エコー", "echo", "ディレイ", "リズム", "音楽", "演出"])]
public sealed class Fx6A613Effect : FxA2 { public override string Label => "反響 8分エコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .25, .25, .45f, 150, 9000, .5f); }

[AudioEffect("反響 スラップバック", ["演出：エコー"], ["反響 スラップバック", "昔の歌声のような一回だけのはね返り", "エコー", "反響", "スラップバック", "echo", "ディレイ", "ロカビリー", "一回", "演出"])]
public sealed class Fx6A614Effect : FxA2 { public override string Label => "反響 スラップバック"; internal override AudioKernel CreateKernel() => new EchoK(this, .11, .11, .1f, 150, 8000, .6f); }

[AudioEffect("反響 宇宙エコー", ["演出：エコー"], ["反響 宇宙エコー", "宇宙空間のような長く細いエコー", "エコー", "反響", "宇宙エコー", "echo", "ディレイ", "SF", "宇宙", "演出"])]
public sealed class Fx6A615Effect : FxA2 { public override string Label => "反響 宇宙エコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .6, .7, .7f, 600, 4000, .55f, true); }

[AudioEffect("反響 こもったエコー", ["演出：エコー"], ["反響 こもったエコー", "壁越しのようにこもったエコー", "エコー", "反響", "こもったエコー", "echo", "ディレイ", "こもる", "演出"])]
public sealed class Fx6A616Effect : FxA2 { public override string Label => "反響 こもったエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .3, .35, .55f, 100, 1200, .55f); }

[AudioEffect("反響 明るいエコー", ["演出：エコー"], ["反響 明るいエコー", "シャリっと明るいエコー", "エコー", "反響", "明るいエコー", "echo", "ディレイ", "明るい", "演出"])]
public sealed class Fx6A617Effect : FxA2 { public override string Label => "反響 明るいエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .3, .35, .45f, 800, 14000, .45f); }

[AudioEffect("反響 ゆっくり消える", ["演出：エコー"], ["反響 ゆっくり消える", "長く少しずつ消えていくエコー", "エコー", "反響", "ゆっくり消える", "echo", "ディレイ", "余韻", "長い", "演出"])]
public sealed class Fx6A618Effect : FxA2 { public override string Label => "反響 ゆっくり消える"; internal override AudioKernel CreateKernel() => new EchoK(this, .4, .45, .8f, 200, 6000, .45f); }

[AudioEffect("反響 すぐ消える", ["演出：エコー"], ["反響 すぐ消える", "一度だけ返ってすぐ消えるエコー", "エコー", "反響", "すぐ消える", "echo", "ディレイ", "短い", "演出"])]
public sealed class Fx6A619Effect : FxA2 { public override string Label => "反響 すぐ消える"; internal override AudioKernel CreateKernel() => new EchoK(this, .2, .22, .2f, 200, 7000, .5f); }

[AudioEffect("反響 おばけ屋敷", ["演出：エコー"], ["反響 おばけ屋敷", "暗い屋敷に不気味に返る反響", "エコー", "反響", "おばけ屋敷", "echo", "ディレイ", "ホラー", "屋敷", "演出"])]
public sealed class Fx6A620Effect : FxA2 { public override string Label => "反響 おばけ屋敷"; internal override AudioKernel CreateKernel() => new EchoK(this, .33, .4, .65f, 200, 2500, .55f, true); }

[AudioEffect("響き 美術館", ["演出：場所の声"], ["響き 美術館", "静かな美術館の広い響き", "場所の声", "響き", "美術館", "リバーブ", "reverb", "残響", "場所", "展示", "静か", "演出"])]
public sealed class Fx6A621Effect : FxA2 { public override string Label => "響き 美術館"; internal override AudioKernel CreateKernel() => new RoomK(this, .7f, .5f, 15, .9f, 180, 7000, .45f); }

[AudioEffect("響き 博物館", ["演出：場所の声"], ["響き 博物館", "天井の高い博物館の響き", "場所の声", "響き", "博物館", "リバーブ", "reverb", "残響", "場所", "展示", "広い", "演出"])]
public sealed class Fx6A622Effect : FxA2 { public override string Label => "響き 博物館"; internal override AudioKernel CreateKernel() => new RoomK(this, .75f, .45f, 18, .9f, 160, 6500, .5f); }

[AudioEffect("響き 地下道", ["演出：場所の声"], ["響き 地下道", "コンクリートの地下道の響き", "場所の声", "響き", "地下道", "リバーブ", "reverb", "残響", "場所", "地下", "通路", "演出"])]
public sealed class Fx6A623Effect : FxA2 { public override string Label => "響き 地下道"; internal override AudioKernel CreateKernel() => new RoomK(this, .6f, .3f, 12, .85f, 200, 6000, .55f); }

[AudioEffect("響き 体育倉庫", ["演出：場所の声"], ["響き 体育倉庫", "マットに囲まれた倉庫の響き", "場所の声", "響き", "体育倉庫", "リバーブ", "reverb", "残響", "場所", "学校", "倉庫", "演出"])]
public sealed class Fx6A624Effect : FxA2 { public override string Label => "響き 体育倉庫"; internal override AudioKernel CreateKernel() => new RoomK(this, .35f, .6f, 5, .6f, 180, 5000, .45f); }

[AudioEffect("響き 寺の本堂", ["演出：場所の声"], ["響き 寺の本堂", "木造の本堂の落ち着いた響き", "場所の声", "響き", "寺の本堂", "リバーブ", "reverb", "残響", "場所", "寺", "和", "演出"])]
public sealed class Fx6A625Effect : FxA2 { public override string Label => "響き 寺の本堂"; internal override AudioKernel CreateKernel() => new RoomK(this, .85f, .5f, 25, .95f, 120, 5500, .5f); }

[AudioEffect("響き 城の大広間", ["演出：場所の声"], ["響き 城の大広間", "石の大広間の豪華な響き", "場所の声", "響き", "城の大広間", "リバーブ", "reverb", "残響", "場所", "城", "ファンタジー", "演出"])]
public sealed class Fx6A626Effect : FxA2 { public override string Label => "響き 城の大広間"; internal override AudioKernel CreateKernel() => new RoomK(this, .9f, .3f, 30, 1f, 120, 7000, .55f); }

[AudioEffect("響き 録音スタジオ", ["演出：場所の声"], ["響き 録音スタジオ", "ほとんど響かないスタジオの音", "場所の声", "響き", "録音スタジオ", "リバーブ", "reverb", "残響", "場所", "スタジオ", "デッド", "演出"])]
public sealed class Fx6A627Effect : FxA2 { public override string Label => "響き 録音スタジオ"; internal override AudioKernel CreateKernel() => new RoomK(this, .2f, .8f, 2, .35f, 100, 12000, .25f); }

[AudioEffect("響き 楽屋", ["演出：場所の声"], ["響き 楽屋", "小さな楽屋の短い響き", "場所の声", "響き", "楽屋", "リバーブ", "reverb", "残響", "場所", "控室", "小部屋", "演出"])]
public sealed class Fx6A628Effect : FxA2 { public override string Label => "響き 楽屋"; internal override AudioKernel CreateKernel() => new RoomK(this, .25f, .6f, 3, .45f, 180, 8000, .4f); }

[AudioEffect("響き 操縦室", ["演出：場所の声"], ["響き 操縦室", "宇宙船の操縦室の金属っぽい響き", "場所の声", "響き", "操縦室", "リバーブ", "reverb", "残響", "場所", "SF", "コックピット", "演出"])]
public sealed class Fx6A629Effect : FxA2 { public override string Label => "響き 操縦室"; internal override AudioKernel CreateKernel() => new RoomK(this, .2f, .35f, 2, .4f, 300, 9000, .45f); }

[AudioEffect("響き 観覧車のゴンドラ", ["演出：場所の声"], ["響き 観覧車のゴンドラ", "観覧車のゴンドラの中の狭い響き", "場所の声", "響き", "観覧車のゴンドラ", "リバーブ", "reverb", "残響", "場所", "狭い", "金属", "演出"])]
public sealed class Fx6A630Effect : FxA2 { public override string Label => "響き 観覧車のゴンドラ"; internal override AudioKernel CreateKernel() => new RoomK(this, .12f, .3f, 1, .3f, 250, 8000, .5f); }

[AudioEffect("響き 台所", ["演出：場所の声"], ["響き 台所", "タイルの台所の響き", "場所の声", "響き", "台所", "リバーブ", "reverb", "残響", "場所", "キッチン", "家", "演出"])]
public sealed class Fx6A631Effect : FxA2 { public override string Label => "響き 台所"; internal override AudioKernel CreateKernel() => new RoomK(this, .3f, .3f, 4, .55f, 200, 9000, .45f); }

[AudioEffect("響き リビング", ["演出：場所の声"], ["響き リビング", "家具のあるリビングの響き", "場所の声", "響き", "リビング", "リバーブ", "reverb", "残響", "場所", "家", "部屋", "演出"])]
public sealed class Fx6A632Effect : FxA2 { public override string Label => "響き リビング"; internal override AudioKernel CreateKernel() => new RoomK(this, .35f, .6f, 5, .6f, 150, 8000, .35f); }

[AudioEffect("響き 寝室", ["演出：場所の声"], ["響き 寝室", "布団やカーテンで響きの少ない寝室", "場所の声", "響き", "寝室", "リバーブ", "reverb", "残響", "場所", "家", "静か", "演出"])]
public sealed class Fx6A633Effect : FxA2 { public override string Label => "響き 寝室"; internal override AudioKernel CreateKernel() => new RoomK(this, .25f, .75f, 3, .45f, 150, 6000, .3f); }

[AudioEffect("響き 和室", ["演出：場所の声"], ["響き 和室", "畳とふすまの和室の響き", "場所の声", "響き", "和室", "リバーブ", "reverb", "残響", "場所", "和", "畳", "演出"])]
public sealed class Fx6A634Effect : FxA2 { public override string Label => "響き 和室"; internal override AudioKernel CreateKernel() => new RoomK(this, .3f, .65f, 4, .5f, 160, 6500, .35f); }

[AudioEffect("響き 銀行", ["演出：場所の声"], ["響き 銀行", "大理石の銀行の響き", "場所の声", "響き", "銀行", "リバーブ", "reverb", "残響", "場所", "受付", "ロビー", "演出"])]
public sealed class Fx6A635Effect : FxA2 { public override string Label => "響き 銀行"; internal override AudioKernel CreateKernel() => new RoomK(this, .6f, .4f, 12, .85f, 180, 7000, .45f); }

[AudioEffect("響き 空港ロビー", ["演出：場所の声"], ["響き 空港ロビー", "広い空港ロビーの響き", "場所の声", "響き", "空港ロビー", "リバーブ", "reverb", "残響", "場所", "空港", "アナウンス", "演出"])]
public sealed class Fx6A636Effect : FxA2 { public override string Label => "響き 空港ロビー"; internal override AudioKernel CreateKernel() => new RoomK(this, .9f, .45f, 30, 1f, 150, 6500, .5f); }

[AudioEffect("響き 駅の構内", ["演出：場所の声"], ["響き 駅の構内", "駅の構内の広い響き", "場所の声", "響き", "駅の構内", "リバーブ", "reverb", "残響", "場所", "駅", "人ごみ", "演出"])]
public sealed class Fx6A637Effect : FxA2 { public override string Label => "響き 駅の構内"; internal override AudioKernel CreateKernel() => new RoomK(this, .8f, .4f, 22, .95f, 180, 6000, .5f); }

[AudioEffect("響き 劇場", ["演出：場所の声"], ["響き 劇場", "劇場の豊かな響き", "場所の声", "響き", "劇場", "リバーブ", "reverb", "残響", "場所", "舞台", "演劇", "演出"])]
public sealed class Fx6A638Effect : FxA2 { public override string Label => "響き 劇場"; internal override AudioKernel CreateKernel() => new RoomK(this, .85f, .45f, 25, .95f, 120, 8000, .45f); }

[AudioEffect("響き 洞くつの入口", ["演出：場所の声"], ["響き 洞くつの入口", "洞くつの入口あたりの響き", "場所の声", "響き", "洞くつの入口", "リバーブ", "reverb", "残響", "場所", "洞窟", "冒険", "演出"])]
public sealed class Fx6A639Effect : FxA2 { public override string Label => "響き 洞くつの入口"; internal override AudioKernel CreateKernel() => new RoomK(this, .6f, .45f, 15, .85f, 150, 5000, .5f); }

[AudioEffect("響き タイルの廊下", ["演出：場所の声"], ["響き タイルの廊下", "タイル張りの廊下のかたい響き", "場所の声", "響き", "タイルの廊下", "リバーブ", "reverb", "残響", "場所", "学校", "病院", "演出"])]
public sealed class Fx6A640Effect : FxA2 { public override string Label => "響き タイルの廊下"; internal override AudioKernel CreateKernel() => new RoomK(this, .5f, .2f, 8, .8f, 200, 9000, .55f); }

[AudioEffect("声 王様", ["演出：声のキャラ"], ["声 王様", "大広間に響く王様のような声", "声のキャラ", "声", "王様", "ボイスチェンジャー", "キャラ", "変声", "王", "えらい", "演出"])]
public sealed class Fx6A641Effect : FxA2 { public override string Label => "声 王様"; internal override AudioKernel CreateKernel() => new VoiceK(this, -4, 70, 9000, .1f, .3f, .8, 300, 4); }

[AudioEffect("声 騎士", ["演出：声のキャラ"], ["声 騎士", "鎧の騎士のような力強い声", "声のキャラ", "声", "騎士", "ボイスチェンジャー", "キャラ", "変声", "剣", "ファンタジー", "演出"])]
public sealed class Fx6A642Effect : FxA2 { public override string Label => "声 騎士"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 90, 8000, .15f, .1f, .4, 800, 4); }

[AudioEffect("声 先生", ["演出：声のキャラ"], ["声 先生", "教室で話す先生のような声", "声のキャラ", "声", "先生", "ボイスチェンジャー", "キャラ", "変声", "学校", "授業", "演出"])]
public sealed class Fx6A643Effect : FxA2 { public override string Label => "声 先生"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 100, 10000, .05f, .15f, .5, 2500, 3); }

[AudioEffect("声 博士", ["演出：声のキャラ"], ["声 博士", "少しこもった博士のような声", "声のキャラ", "声", "博士", "ボイスチェンジャー", "キャラ", "変声", "研究", "科学", "演出"])]
public sealed class Fx6A644Effect : FxA2 { public override string Label => "声 博士"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 150, 8000, .1f, .05f, .3, 1500, 4); }

[AudioEffect("声 魔女", ["演出：声のキャラ"], ["声 魔女", "かすれた高い魔女のような声", "声のキャラ", "声", "魔女", "ボイスチェンジャー", "キャラ", "変声", "ハロウィン", "魔法", "演出"])]
public sealed class Fx6A645Effect : FxA2 { public override string Label => "声 魔女"; internal override AudioKernel CreateKernel() => new VoiceK(this, 3, 300, 9000, .3f, .2f, .5, 2800, 6); }

[AudioEffect("声 天使", ["演出：声のキャラ"], ["声 天使", "高く響く天使のような声", "声のキャラ", "声", "天使", "ボイスチェンジャー", "キャラ", "変声", "神聖", "天国", "演出"])]
public sealed class Fx6A646Effect : FxA2 { public override string Label => "声 天使"; internal override AudioKernel CreateKernel() => new VoiceK(this, 6, 250, 15000, 0, .5f, .9, 4000, 3); }

[AudioEffect("声 悪魔", ["演出：声のキャラ"], ["声 悪魔", "低く歪んだ悪魔のような声", "声のキャラ", "声", "悪魔", "ボイスチェンジャー", "キャラ", "変声", "ホラー", "悪役", "演出"])]
public sealed class Fx6A647Effect : FxA2 { public override string Label => "声 悪魔"; internal override AudioKernel CreateKernel() => new VoiceK(this, -9, 50, 5000, .5f, .35f, .9, 250, 5); }

[AudioEffect("声 ゾンビ", ["演出：声のキャラ"], ["声 ゾンビ", "うめくようなゾンビの声", "声のキャラ", "声", "ゾンビ", "ボイスチェンジャー", "キャラ", "変声", "ホラー", "うめき", "演出"])]
public sealed class Fx6A648Effect : FxA2 { public override string Label => "声 ゾンビ"; internal override AudioKernel CreateKernel() => new VoiceK(this, -4, 150, 3500, .6f, .2f, .6, 600, 6); }

[AudioEffect("声 ドラゴン", ["演出：声のキャラ"], ["声 ドラゴン", "ドラゴンのような重く低い声", "声のキャラ", "声", "ドラゴン", "ボイスチェンジャー", "キャラ", "変声", "ファンタジー", "迫力", "演出"])]
public sealed class Fx6A649Effect : FxA2 { public override string Label => "声 ドラゴン"; internal override AudioKernel CreateKernel() => new VoiceK(this, -11, 40, 4500, .7f, .4f, .9, 200, 6); }

[AudioEffect("声 ペンギン", ["演出：声のキャラ"], ["声 ペンギン", "ペンギンのような高い鳴き声風", "声のキャラ", "声", "ペンギン", "ボイスチェンジャー", "キャラ", "変声", "動物", "かわいい", "演出"])]
public sealed class Fx6A650Effect : FxA2 { public override string Label => "声 ペンギン"; internal override AudioKernel CreateKernel() => new VoiceK(this, 6, 400, 9000, .2f, 0, .3, 2000, 6); }

[AudioEffect("声 ねこ", ["演出：声のキャラ"], ["声 ねこ", "ねこのような細く高い声", "声のキャラ", "声", "ねこ", "ボイスチェンジャー", "キャラ", "変声", "動物", "にゃー", "演出"])]
public sealed class Fx6A651Effect : FxA2 { public override string Label => "声 ねこ"; internal override AudioKernel CreateKernel() => new VoiceK(this, 8, 500, 12000, .1f, 0, .3, 3000, 5); }

[AudioEffect("声 いぬ", ["演出：声のキャラ"], ["声 いぬ", "いぬのような元気な声", "声のキャラ", "声", "いぬ", "ボイスチェンジャー", "キャラ", "変声", "動物", "わん", "演出"])]
public sealed class Fx6A652Effect : FxA2 { public override string Label => "声 いぬ"; internal override AudioKernel CreateKernel() => new VoiceK(this, 3, 200, 9000, .15f, 0, .3, 1200, 5); }

[AudioEffect("声 ロボットの子", ["演出：声のキャラ"], ["声 ロボットの子", "子どものロボットのような声", "声のキャラ", "声", "ロボットの子", "ボイスチェンジャー", "キャラ", "変声", "ロボ", "SF", "演出"])]
public sealed class Fx6A653Effect : FxA2 { public override string Label => "声 ロボットの子"; internal override AudioKernel CreateKernel() => new VoiceK(this, 5, 400, 7000, .5f, .1f, .3, 2500, 8); }

[AudioEffect("声 宇宙船のAI", ["演出：声のキャラ"], ["声 宇宙船のAI", "宇宙船に流れるAIのような声", "声のキャラ", "声", "宇宙船のAI", "ボイスチェンジャー", "キャラ", "変声", "SF", "案内", "演出"])]
public sealed class Fx6A654Effect : FxA2 { public override string Label => "声 宇宙船のAI"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 300, 9000, .2f, .35f, .7, 3000, 5); }

[AudioEffect("声 電話オペレーター", ["演出：声のキャラ"], ["声 電話オペレーター", "電話の窓口のような細い声", "声のキャラ", "声", "電話オペレーター", "ボイスチェンジャー", "キャラ", "変声", "電話", "案内", "演出"])]
public sealed class Fx6A655Effect : FxA2 { public override string Label => "声 電話オペレーター"; internal override AudioKernel CreateKernel() => new VoiceK(this, 1, 500, 4000, .05f, 0, .3, 2200, 4); }

[AudioEffect("声 実況", ["演出：声のキャラ"], ["声 実況", "熱い実況のような張りのある声", "声のキャラ", "声", "実況", "ボイスチェンジャー", "キャラ", "変声", "スポーツ", "熱い", "演出"])]
public sealed class Fx6A656Effect : FxA2 { public override string Label => "声 実況"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 150, 11000, .25f, .1f, .5, 2800, 5); }

[AudioEffect("声 審判", ["演出：声のキャラ"], ["声 審判", "会場に響く審判のような声", "声のキャラ", "声", "審判", "ボイスチェンジャー", "キャラ", "変声", "スポーツ", "会場", "演出"])]
public sealed class Fx6A657Effect : FxA2 { public override string Label => "声 審判"; internal override AudioKernel CreateKernel() => new VoiceK(this, 1, 300, 8000, .3f, .3f, .8, 2500, 6); }

[AudioEffect("声 校長先生", ["演出：声のキャラ"], ["声 校長先生", "体育館に響く校長先生のような声", "声のキャラ", "声", "校長先生", "ボイスチェンジャー", "キャラ", "変声", "学校", "朝礼", "演出"])]
public sealed class Fx6A658Effect : FxA2 { public override string Label => "声 校長先生"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 120, 6000, .1f, .45f, .9, 1500, 4); }

[AudioEffect("声 赤ちゃん風", ["演出：声のキャラ"], ["声 赤ちゃん風", "赤ちゃんのように高く小さい声", "声のキャラ", "声", "赤ちゃん風", "ボイスチェンジャー", "キャラ", "変声", "小さい", "かわいい", "演出"])]
public sealed class Fx6A659Effect : FxA2 { public override string Label => "声 赤ちゃん風"; internal override AudioKernel CreateKernel() => new VoiceK(this, 9, 350, 12000, 0, 0, .3, 3500, 4); }

[AudioEffect("声 ささやき天使", ["演出：声のキャラ"], ["声 ささやき天使", "耳元でささやく高く澄んだ声", "声のキャラ", "声", "ささやき天使", "ボイスチェンジャー", "キャラ", "変声", "ASMR", "やさしい", "演出"])]
public sealed class Fx6A660Effect : FxA2 { public override string Label => "声 ささやき天使"; internal override AudioKernel CreateKernel() => new VoiceK(this, 4, 700, 14000, 0, .4f, .8, 6000, 4); }

[AudioEffect("揺れ 脈打つ 遅い", ["演出：変調"], ["揺れ 脈打つ 遅い", "ゆっくりした鼓動のように脈打つ", "変調", "揺れ", "脈打つ", "遅い", "モジュレーション", "うねり", "心臓", "緊張", "演出"])]
public sealed class Fx6A661Effect : FxA3 { public override string Label => "揺れ 脈打つ 遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, .6, 1.2, 3); }

[AudioEffect("揺れ ぱたぱた", ["演出：変調"], ["揺れ ぱたぱた", "ぱたぱたと細かく音が切れる", "変調", "揺れ", "ぱたぱた", "モジュレーション", "うねり", "羽", "点滅", "演出"])]
public sealed class Fx6A662Effect : FxA3 { public override string Label => "揺れ ぱたぱた"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 10, 20, 1); }

[AudioEffect("揺れ 左右に揺れる 遅い", ["演出：変調"], ["揺れ 左右に揺れる 遅い", "ゆっくり左右に揺れる", "変調", "揺れ", "左右に揺れる", "遅い", "モジュレーション", "うねり", "ステレオ", "ゆったり", "演出"])]
public sealed class Fx6A663Effect : FxA3 { public override string Label => "揺れ 左右に揺れる 遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, .2, .8, 0, true); }

[AudioEffect("揺れ 左右に揺れる 速い", ["演出：変調"], ["揺れ 左右に揺れる 速い", "速く左右に揺れる", "変調", "揺れ", "左右に揺れる", "速い", "モジュレーション", "うねり", "ステレオ", "回る", "演出"])]
public sealed class Fx6A664Effect : FxA3 { public override string Label => "揺れ 左右に揺れる 速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 4, 10, 0, true); }

[AudioEffect("揺れ ざわざわ", ["演出：変調"], ["揺れ ざわざわ", "人がざわつくように音を揺らす", "変調", "揺れ", "ざわざわ", "モジュレーション", "うねり", "群衆", "ざわめき", "演出"])]
public sealed class Fx6A665Effect : FxA3 { public override string Label => "揺れ ざわざわ"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 20, 10, 1, 3, .1f, 1f); }

[AudioEffect("揺れ 水中のうねり", ["演出：変調"], ["揺れ 水中のうねり", "水の中のようにうねる", "変調", "揺れ", "水中のうねり", "モジュレーション", "うねり", "水中", "ゆらゆら", "演出"])]
public sealed class Fx6A666Effect : FxA3 { public override string Label => "揺れ 水中のうねり"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 9, 7, .2, .6, .3f, 1f); }

[AudioEffect("揺れ 金属のうなり", ["演出：変調"], ["揺れ 金属のうなり", "金属っぽくうなる音", "変調", "揺れ", "金属のうなり", "モジュレーション", "うねり", "フランジャー", "金属", "演出"])]
public sealed class Fx6A667Effect : FxA3 { public override string Label => "揺れ 金属のうなり"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 1, .9, .3, 1, .9f, 1f); }

[AudioEffect("揺れ ゆっくりコーラス", ["演出：変調"], ["揺れ ゆっくりコーラス", "ゆっくり広がるコーラス", "変調", "揺れ", "ゆっくりコーラス", "モジュレーション", "うねり", "広がり", "やわらか", "演出"])]
public sealed class Fx6A668Effect : FxA3 { public override string Label => "揺れ ゆっくりコーラス"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 25, 8, .1, .3, 0f, .9f); }

[AudioEffect("揺れ 速いビブラート", ["演出：変調"], ["揺れ 速いビブラート", "音程が速く震える", "変調", "揺れ", "速いビブラート", "モジュレーション", "うねり", "震え", "ビブラート", "演出"])]
public sealed class Fx6A669Effect : FxA3 { public override string Label => "揺れ 速いビブラート"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 5, 3, 5, 9, 0f, 1f); }

[AudioEffect("揺れ ゆっくりビブラート", ["演出：変調"], ["揺れ ゆっくりビブラート", "音程がゆっくり震える", "変調", "揺れ", "ゆっくりビブラート", "モジュレーション", "うねり", "歌", "ビブラート", "演出"])]
public sealed class Fx6A670Effect : FxA3 { public override string Label => "揺れ ゆっくりビブラート"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 6, 4, 1.5, 3, 0f, 1f); }

[AudioEffect("揺れ こもる開く", ["演出：変調"], ["揺れ こもる開く", "こもったり開いたりをくり返す", "変調", "揺れ", "こもる開く", "モジュレーション", "うねり", "フィルター", "呼吸", "演出"])]
public sealed class Fx6A671Effect : FxA3 { public override string Label => "揺れ こもる開く"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 200, 8000, 0, 1, .1, .4); }

[AudioEffect("揺れ シャリシャリ", ["演出：変調"], ["揺れ シャリシャリ", "高い音だけがシャリシャリ揺れる", "変調", "揺れ", "シャリシャリ", "モジュレーション", "うねり", "高音", "演出"])]
public sealed class Fx6A672Effect : FxA3 { public override string Label => "揺れ シャリシャリ"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 2000, 9000, 2, 2, .5, 2); }

[AudioEffect("揺れ ワウ 鋭い", ["演出：変調"], ["揺れ ワウ 鋭い", "鋭くクセの強いワウ", "変調", "揺れ", "ワウ", "鋭い", "モジュレーション", "うねり", "ギター", "ファンク", "演出"])]
public sealed class Fx6A673Effect : FxA3 { public override string Label => "揺れ ワウ 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 400, 3500, 1, 4, .5, 2); }

[AudioEffect("揺れ ロボ声の揺れ", ["演出：変調"], ["揺れ ロボ声の揺れ", "ロボットのように音が細かく揺れる", "変調", "揺れ", "ロボ声の揺れ", "モジュレーション", "うねり", "ロボット", "機械", "演出"])]
public sealed class Fx6A674Effect : FxA3 { public override string Label => "揺れ ロボ声の揺れ"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 3, 2, 8, 15, .6f, 1f); }

[AudioEffect("揺れ トンネル通過", ["演出：変調"], ["揺れ トンネル通過", "トンネルを出入りするように音がこもる", "変調", "揺れ", "トンネル通過", "モジュレーション", "うねり", "車", "移動", "演出"])]
public sealed class Fx6A675Effect : FxA3 { public override string Label => "揺れ トンネル通過"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 5000, 0, 1.2, .05, .2); }

[AudioEffect("揺れ ラジオの混線", ["演出：変調"], ["揺れ ラジオの混線", "混線したラジオのように乱れる", "変調", "揺れ", "ラジオの混線", "モジュレーション", "うねり", "ラジオ", "ノイズ", "演出"])]
public sealed class Fx6A676Effect : FxA3 { public override string Label => "揺れ ラジオの混線"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 5, 15, 2, true); }

[AudioEffect("揺れ ふわふわ", ["演出：変調"], ["揺れ ふわふわ", "ふわふわと浮かぶような揺れ", "変調", "揺れ", "ふわふわ", "モジュレーション", "うねり", "夢", "やさしい", "演出"])]
public sealed class Fx6A677Effect : FxA3 { public override string Label => "揺れ ふわふわ"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 14, 6, .3, .8, .2f, 1f); }

[AudioEffect("揺れ ドップラー風", ["演出：変調"], ["揺れ ドップラー風", "近づいて遠ざかるように音程が変わる", "変調", "揺れ", "ドップラー風", "モジュレーション", "うねり", "車", "通過", "演出"])]
public sealed class Fx6A678Effect : FxA3 { public override string Label => "揺れ ドップラー風"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 30, 25, .05, .2, 0f, 1f); }

[AudioEffect("揺れ きらきら", ["演出：変調"], ["揺れ きらきら", "高い音がきらきら揺れる", "変調", "揺れ", "きらきら", "モジュレーション", "うねり", "魔法", "光", "演出"])]
public sealed class Fx6A679Effect : FxA3 { public override string Label => "揺れ きらきら"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 3000, 12000, 1, 3, 1, 4); }

[AudioEffect("揺れ 心のざわめき", ["演出：変調"], ["揺れ 心のざわめき", "不安な気持ちのように音がざわつく", "変調", "揺れ", "心のざわめき", "モジュレーション", "うねり", "不安", "心理", "演出"])]
public sealed class Fx6A680Effect : FxA3 { public override string Label => "揺れ 心のざわめき"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 18, 12, .5, 1.5, .4f, 1f); }

[AudioEffect("場面 雨の日の電話", ["演出：場面の音"], ["場面 雨の日の電話", "雨の日に聞く電話のような声", "場面の音", "場面", "雨の日の電話", "シーン", "効果音", "組み合わせ", "電話", "雨", "演出"])]
public sealed class Fx6A681Effect : FxA3 { public override string Label => "場面 雨の日の電話"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 400, 3000), new FilterLfoK(this, 1500, 4000, 0, .9, .1, .3)); }

[AudioEffect("場面 体育館のマイク", ["演出：場面の音"], ["場面 体育館のマイク", "体育館でマイクを使って話す声", "場面の音", "場面", "体育館のマイク", "シーン", "効果音", "組み合わせ", "学校", "全校集会", "演出"])]
public sealed class Fx6A682Effect : FxA3 { public override string Label => "場面 体育館のマイク"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 300, 6000), new RoomK(this, .8f, .35f, 20, .95f, 150, 7000, .55f), new EchoK(this, .3, .3, .2f, 300, 4000, .3f)); }

[AudioEffect("場面 トランシーバー", ["演出：場面の音"], ["場面 トランシーバー", "トランシーバーのような割れた細い声", "場面の音", "場面", "トランシーバー", "シーン", "効果音", "組み合わせ", "無線", "警察", "演出"])]
public sealed class Fx6A683Effect : FxA3 { public override string Label => "場面 トランシーバー"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 600, 2800, 3), new ShapeK(this, MathF.Tanh, 6, 18, 3000, 0)); }

[AudioEffect("場面 海の中の通信", ["演出：場面の音"], ["場面 海の中の通信", "潜水服の通信のような声", "場面の音", "場面", "海の中の通信", "シーン", "効果音", "組み合わせ", "潜水", "海", "演出"])]
public sealed class Fx6A684Effect : FxA3 { public override string Label => "場面 海の中の通信"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 200, 1500), new ModDelayK(this, 10, 6, .3, .8, .3f, .9f), new EchoK(this, .2, .25, .4f, 200, 1500, .4f)); }

[AudioEffect("場面 遠くの雷のよう", ["演出：場面の音"], ["場面 遠くの雷のよう", "遠くで響く雷のような重い響き", "場面の音", "場面", "遠くの雷のよう", "シーン", "効果音", "組み合わせ", "雷", "迫力", "演出"])]
public sealed class Fx6A685Effect : FxA3 { public override string Label => "場面 遠くの雷のよう"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (80, .7, 6), (6000, .7, -8)), new RoomK(this, .95f, .5f, 40, 1f, 60, 3000, .7f)); }

[AudioEffect("場面 夢の中の呼び声", ["演出：場面の音"], ["場面 夢の中の呼び声", "夢の中で遠くから呼ばれるような声", "場面の音", "場面", "夢の中の呼び声", "シーン", "効果音", "組み合わせ", "夢", "回想", "演出"])]
public sealed class Fx6A686Effect : FxA3 { public override string Label => "場面 夢の中の呼び声"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, 2, 300, 7000, 0, .4f, .9), new FilterLfoK(this, 800, 6000, 0, .9, .05, .2)); }

[AudioEffect("場面 スマホのスピーカー", ["演出：場面の音"], ["場面 スマホのスピーカー", "スマホのスピーカーから流れる音", "場面の音", "場面", "スマホのスピーカー", "シーン", "効果音", "組み合わせ", "スマホ", "動画", "演出"])]
public sealed class Fx6A687Effect : FxA3 { public override string Label => "場面 スマホのスピーカー"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 700, 7000, 1), new ShapeK(this, x => MathF.Tanh(x * 1.2f), 2, 8, 7000, 0)); }

[AudioEffect("場面 古いレコード盤", ["演出：場面の音"], ["場面 古いレコード盤", "古いレコードのような揺れとこもり", "場面の音", "場面", "古いレコード盤", "シーン", "効果音", "組み合わせ", "レトロ", "昭和", "演出"])]
public sealed class Fx6A688Effect : FxA3 { public override string Label => "場面 古いレコード盤"; internal override AudioKernel CreateKernel() => new ChainK(new TapeK(this, true), new ModDelayK(this, 8, 1.5, .5, .9, 0f, 1f), new BandK(this, 200, 6000)); }

[AudioEffect("場面 防災無線", ["演出：場面の音"], ["場面 防災無線", "町に響く防災無線のような声", "場面の音", "場面", "防災無線", "シーン", "効果音", "組み合わせ", "町内", "放送", "演出"])]
public sealed class Fx6A689Effect : FxA3 { public override string Label => "場面 防災無線"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 500, 4000, 3), new EchoK(this, .5, .6, .5f, 400, 3500, .55f), new RoomK(this, .9f, .4f, 20, 1f, 300, 4000, .4f)); }

[AudioEffect("場面 ヘルメット越し", ["演出：場面の音"], ["場面 ヘルメット越し", "ヘルメットの中で話しているような声", "場面の音", "場面", "ヘルメット越し", "シーン", "効果音", "組み合わせ", "宇宙", "こもる", "演出"])]
public sealed class Fx6A690Effect : FxA3 { public override string Label => "場面 ヘルメット越し"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 200, 3000), new RoomK(this, .08f, .3f, 1, .25f, 300, 4000, .6f)); }

[AudioEffect("場面 巨大ロボの声", ["演出：場面の音"], ["場面 巨大ロボの声", "巨大ロボットのような重い機械の声", "場面の音", "場面", "巨大ロボの声", "シーン", "効果音", "組み合わせ", "ロボ", "迫力", "演出"])]
public sealed class Fx6A691Effect : FxA3 { public override string Label => "場面 巨大ロボの声"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, -6, 60, 5000, .5f, .2f, .6, 400, 6), new ModDelayK(this, 2, 1.5, 6, 12, .6f, .8f), new RoomK(this, .8f, .4f, 20, 1f, 100, 5000, .4f)); }

[AudioEffect("場面 妖精のささやき", ["演出：場面の音"], ["場面 妖精のささやき", "妖精がささやくようなきらめく声", "場面の音", "場面", "妖精のささやき", "シーン", "効果音", "組み合わせ", "ファンタジー", "小さい", "演出"])]
public sealed class Fx6A692Effect : FxA3 { public override string Label => "場面 妖精のささやき"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, 8, 500, 15000, 0, .3f, .8), new ModDelayK(this, 12, 5, .5, 1.5, .2f, .9f)); }

[AudioEffect("場面 昔のテレビ", ["演出：場面の音"], ["場面 昔のテレビ", "昔のテレビから流れるような音", "場面の音", "場面", "昔のテレビ", "シーン", "効果音", "組み合わせ", "レトロ", "テレビ", "演出"])]
public sealed class Fx6A693Effect : FxA3 { public override string Label => "場面 昔のテレビ"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 300, 5000), new TapeK(this, false), new RoomK(this, .3f, .6f, 3, .5f, 200, 6000, .3f)); }

[AudioEffect("場面 駅のホームの放送", ["演出：場面の音"], ["場面 駅のホームの放送", "駅のホームに響く放送のような声", "場面の音", "場面", "駅のホームの放送", "シーン", "効果音", "組み合わせ", "駅", "電車", "演出"])]
public sealed class Fx6A694Effect : FxA3 { public override string Label => "場面 駅のホームの放送"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 400, 5000, 2), new EchoK(this, .4, .45, .4f, 400, 4000, .45f), new RoomK(this, .8f, .4f, 22, .95f, 200, 5000, .45f)); }

[AudioEffect("場面 お風呂で歌う", ["演出：場面の音"], ["場面 お風呂で歌う", "お風呂で歌っているような響き", "場面の音", "場面", "お風呂で歌う", "シーン", "効果音", "組み合わせ", "風呂", "歌", "演出"])]
public sealed class Fx6A695Effect : FxA3 { public override string Label => "場面 お風呂で歌う"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .35f, .25f, 4, .7f, 200, 9000, .7f), new EchoK(this, .08, .1, .3f, 300, 8000, .3f)); }

[AudioEffect("場面 ラジオの向こうの会場", ["演出：場面の音"], ["場面 ラジオの向こうの会場", "ラジオ中継で聞く会場の音", "場面の音", "場面", "ラジオの向こうの会場", "シーン", "効果音", "組み合わせ", "中継", "ラジオ", "演出"])]
public sealed class Fx6A696Effect : FxA3 { public override string Label => "場面 ラジオの向こうの会場"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .85f, .4f, 25, 1f, 120, 8000, .5f), new BandK(this, 300, 4500, 2)); }

[AudioEffect("場面 電話の向こうのトイレ", ["演出：場面の音"], ["場面 電話の向こうのトイレ", "電話の向こうで狭い所にいるような声", "場面の音", "場面", "電話の向こうのトイレ", "シーン", "効果音", "組み合わせ", "電話", "狭い", "演出"])]
public sealed class Fx6A697Effect : FxA3 { public override string Label => "場面 電話の向こうのトイレ"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .18f, .35f, 2, .5f, 220, 8000, .6f), new BandK(this, 400, 3200)); }

[AudioEffect("場面 洞くつの魔物", ["演出：場面の音"], ["場面 洞くつの魔物", "洞くつの奥の魔物のような声", "場面の音", "場面", "洞くつの魔物", "シーン", "効果音", "組み合わせ", "ボス", "ダンジョン", "演出"])]
public sealed class Fx6A698Effect : FxA3 { public override string Label => "場面 洞くつの魔物"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, -8, 50, 4500, .4f, 0, .5, 300, 5), new EchoK(this, .25, .3, .6f, 150, 2500, .5f), new RoomK(this, .9f, .5f, 25, 1f, 120, 4000, .5f)); }

[AudioEffect("場面 時計じかけ", ["演出：場面の音"], ["場面 時計じかけ", "時計の刻みのように音が切れる", "場面の音", "場面", "時計じかけ", "シーン", "効果音", "組み合わせ", "時計", "機械", "演出"])]
public sealed class Fx6A699Effect : FxA3 { public override string Label => "場面 時計じかけ"; internal override AudioKernel CreateKernel() => new ChainK(new GainLfoK(this, 4, 4.1, 1), new BandK(this, 800, 6000)); }

[AudioEffect("場面 ゲームのナレーション", ["演出：場面の音"], ["場面 ゲームのナレーション", "ゲームの語りのようなはっきり響く声", "場面の音", "場面", "ゲームのナレーション", "シーン", "効果音", "組み合わせ", "ゲーム", "語り", "演出"])]
public sealed class Fx6A700Effect : FxA3 { public override string Label => "場面 ゲームのナレーション"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (120, .7, 3), (3500, 1, 3)), new RoomK(this, .5f, .5f, 8, .75f, 150, 9000, .3f), new EchoK(this, .15, .2, .2f, 300, 7000, .2f)); }
