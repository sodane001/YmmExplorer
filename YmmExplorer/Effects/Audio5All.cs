using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 501〜600(gen_v133.py で生成)

/// <summary>いくつかの処理を順につなぐ(場面の音)</summary>
internal sealed class ChainK(params AudioKernel[] ks) : AudioKernel
{
    public override int Latency => ks.Sum(k => k.Latency);
    public override void Reset(int hz) { foreach (var k in ks) k.Reset(hz); }
    public override void Params(long frame, long total, int hz) { foreach (var k in ks) k.Params(frame, total, hz); }
    public override void Process(ref float l, ref float r) { foreach (var k in ks) k.Process(ref l, ref r); }
}

[AudioEffect("反響 山びこ 遠い", ["演出：エコー"], ["反響 山びこ 遠い", "遠くの山から声が返ってくる", "エコー", "反響", "山びこ", "遠い", "echo", "ディレイ", "やまびこ", "ヤッホー", "演出"])]
public sealed class Fx5A501Effect : FxA2 { public override string Label => "反響 山びこ 遠い"; internal override AudioKernel CreateKernel() => new EchoK(this, .6, .65, .45f, 300, 4000, .5f); }

[AudioEffect("反響 山びこ 近い", ["演出：エコー"], ["反響 山びこ 近い", "近くの山から声が返ってくる", "エコー", "反響", "山びこ", "近い", "echo", "ディレイ", "やまびこ", "演出"])]
public sealed class Fx5A502Effect : FxA2 { public override string Label => "反響 山びこ 近い"; internal override AudioKernel CreateKernel() => new EchoK(this, .3, .32, .35f, 250, 6000, .5f); }

[AudioEffect("反響 谷底", ["演出：エコー"], ["反響 谷底", "深い谷に声が何度もひびく", "エコー", "反響", "谷底", "echo", "ディレイ", "やまびこ", "谷", "深い", "演出"])]
public sealed class Fx5A503Effect : FxA2 { public override string Label => "反響 谷底"; internal override AudioKernel CreateKernel() => new EchoK(this, .45, .5, .6f, 200, 3000, .55f); }

[AudioEffect("反響 トンネルの出口", ["演出：エコー"], ["反響 トンネルの出口", "トンネルの中で細かく反響する", "エコー", "反響", "トンネルの出口", "echo", "ディレイ", "やまびこ", "トンネル", "地下", "演出"])]
public sealed class Fx5A504Effect : FxA2 { public override string Label => "反響 トンネルの出口"; internal override AudioKernel CreateKernel() => new EchoK(this, .12, .14, .7f, 200, 3500, .6f); }

[AudioEffect("反響 井戸の底", ["演出：エコー"], ["反響 井戸の底", "井戸の中のような短くこもった反響", "エコー", "反響", "井戸の底", "echo", "ディレイ", "やまびこ", "井戸", "こもる", "演出"])]
public sealed class Fx5A505Effect : FxA2 { public override string Label => "反響 井戸の底"; internal override AudioKernel CreateKernel() => new EchoK(this, .07, .08, .75f, 300, 2500, .6f); }

[AudioEffect("反響 宇宙船の通路", ["演出：エコー"], ["反響 宇宙船の通路", "金属の通路で左右に反響する", "エコー", "反響", "宇宙船の通路", "echo", "ディレイ", "やまびこ", "SF", "金属", "演出"])]
public sealed class Fx5A506Effect : FxA2 { public override string Label => "反響 宇宙船の通路"; internal override AudioKernel CreateKernel() => new EchoK(this, .09, .11, .6f, 500, 6000, .5f, true); }

[AudioEffect("反響 鉄の配管", ["演出：エコー"], ["反響 鉄の配管", "パイプの中のような金属っぽい反響", "エコー", "反響", "鉄の配管", "echo", "ディレイ", "やまびこ", "パイプ", "金属", "演出"])]
public sealed class Fx5A507Effect : FxA2 { public override string Label => "反響 鉄の配管"; internal override AudioKernel CreateKernel() => new EchoK(this, .03, .035, .82f, 600, 5000, .55f); }

[AudioEffect("反響 夜の団地", ["演出：エコー"], ["反響 夜の団地", "建物の壁に声がはね返る", "エコー", "反響", "夜の団地", "echo", "ディレイ", "やまびこ", "団地", "夜", "演出"])]
public sealed class Fx5A508Effect : FxA2 { public override string Label => "反響 夜の団地"; internal override AudioKernel CreateKernel() => new EchoK(this, .25, .3, .4f, 300, 5000, .4f); }

[AudioEffect("反響 ビルの谷間", ["演出：エコー"], ["反響 ビルの谷間", "ビルの間で左右に声がはね返る", "エコー", "反響", "ビルの谷間", "echo", "ディレイ", "やまびこ", "都会", "ビル", "演出"])]
public sealed class Fx5A509Effect : FxA2 { public override string Label => "反響 ビルの谷間"; internal override AudioKernel CreateKernel() => new EchoK(this, .18, .22, .5f, 350, 6000, .45f, true); }

[AudioEffect("反響 スタジアムの客席", ["演出：エコー"], ["反響 スタジアムの客席", "大きな会場の広いエコー", "エコー", "反響", "スタジアムの客席", "echo", "ディレイ", "やまびこ", "スタジアム", "会場", "演出"])]
public sealed class Fx5A510Effect : FxA2 { public override string Label => "反響 スタジアムの客席"; internal override AudioKernel CreateKernel() => new EchoK(this, .3, .36, .5f, 300, 3500, .55f); }

[AudioEffect("反響 洞窟の奥", ["演出：エコー"], ["反響 洞窟の奥", "洞窟の奥で長くひびく", "エコー", "反響", "洞窟の奥", "echo", "ディレイ", "やまびこ", "洞窟", "冒険", "演出"])]
public sealed class Fx5A511Effect : FxA2 { public override string Label => "反響 洞窟の奥"; internal override AudioKernel CreateKernel() => new EchoK(this, .2, .24, .72f, 150, 2800, .6f); }

[AudioEffect("反響 テープエコー", ["演出：エコー"], ["反響 テープエコー", "昔のテープ式エコーのようなこもった返り", "エコー", "反響", "テープエコー", "echo", "ディレイ", "やまびこ", "レトロ", "ダブ", "演出"])]
public sealed class Fx5A512Effect : FxA2 { public override string Label => "反響 テープエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .28, .28, .55f, 400, 3000, .5f); }

[AudioEffect("反響 ピンポン 速い", ["演出：エコー"], ["反響 ピンポン 速い", "左右に速く跳ね返るエコー", "エコー", "反響", "ピンポン", "速い", "echo", "ディレイ", "やまびこ", "ステレオ", "左右", "演出"])]
public sealed class Fx5A513Effect : FxA2 { public override string Label => "反響 ピンポン 速い"; internal override AudioKernel CreateKernel() => new EchoK(this, .15, .3, .5f, 200, 8000, .55f, true); }

[AudioEffect("反響 ピンポン 遅い", ["演出：エコー"], ["反響 ピンポン 遅い", "左右にゆっくり跳ね返るエコー", "エコー", "反響", "ピンポン", "遅い", "echo", "ディレイ", "やまびこ", "ステレオ", "左右", "演出"])]
public sealed class Fx5A514Effect : FxA2 { public override string Label => "反響 ピンポン 遅い"; internal override AudioKernel CreateKernel() => new EchoK(this, .35, .7, .5f, 200, 8000, .55f, true); }

[AudioEffect("反響 3連エコー", ["演出：エコー"], ["反響 3連エコー", "3連のリズムで返るエコー", "エコー", "反響", "3連エコー", "echo", "ディレイ", "やまびこ", "リズム", "音楽", "演出"])]
public sealed class Fx5A515Effect : FxA2 { public override string Label => "反響 3連エコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .167, .167, .5f, 150, 9000, .5f); }

[AudioEffect("反響 付点エコー", ["演出：エコー"], ["反響 付点エコー", "付点のリズムで返るエコー", "エコー", "反響", "付点エコー", "echo", "ディレイ", "やまびこ", "リズム", "音楽", "演出"])]
public sealed class Fx5A516Effect : FxA2 { public override string Label => "反響 付点エコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .375, .375, .45f, 150, 9000, .5f); }

[AudioEffect("反響 ダブリング", ["演出：エコー"], ["反響 ダブリング", "すぐ後ろに声を重ねて厚くする", "エコー", "反響", "ダブリング", "echo", "ディレイ", "やまびこ", "厚み", "二重", "演出"])]
public sealed class Fx5A517Effect : FxA2 { public override string Label => "反響 ダブリング"; internal override AudioKernel CreateKernel() => new EchoK(this, .025, .032, .1f, 120, 12000, .7f); }

[AudioEffect("反響 夢のエコー", ["演出：エコー"], ["反響 夢のエコー", "長く残る夢の中のようなエコー", "エコー", "反響", "夢のエコー", "echo", "ディレイ", "やまびこ", "回想", "ふわふわ", "演出"])]
public sealed class Fx5A518Effect : FxA2 { public override string Label => "反響 夢のエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .5, .55, .75f, 300, 3000, .6f); }

[AudioEffect("反響 水中のエコー", ["演出：エコー"], ["反響 水中のエコー", "こもった水の中のようなエコー", "エコー", "反響", "水中のエコー", "echo", "ディレイ", "やまびこ", "水中", "こもる", "演出"])]
public sealed class Fx5A519Effect : FxA2 { public override string Label => "反響 水中のエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .2, .22, .6f, 100, 900, .6f); }

[AudioEffect("反響 電話のエコー", ["演出：エコー"], ["反響 電話のエコー", "電話の向こうで返るような細いエコー", "エコー", "反響", "電話のエコー", "echo", "ディレイ", "やまびこ", "電話", "細い", "演出"])]
public sealed class Fx5A520Effect : FxA2 { public override string Label => "反響 電話のエコー"; internal override AudioKernel CreateKernel() => new EchoK(this, .22, .22, .4f, 700, 3000, .5f); }

[AudioEffect("響き 浴室", ["演出：場所の声"], ["響き 浴室", "タイルの浴室のような明るい響き", "場所の声", "響き", "浴室", "リバーブ", "reverb", "残響", "場所", "お風呂", "風呂", "演出"])]
public sealed class Fx5A521Effect : FxA2 { public override string Label => "響き 浴室"; internal override AudioKernel CreateKernel() => new RoomK(this, .35f, .25f, 4, .7f, 200, 9000, .65f); }

[AudioEffect("響き トイレの個室", ["演出：場所の声"], ["響き トイレの個室", "狭い個室の短い響き", "場所の声", "響き", "トイレの個室", "リバーブ", "reverb", "残響", "場所", "狭い", "個室", "演出"])]
public sealed class Fx5A522Effect : FxA2 { public override string Label => "響き トイレの個室"; internal override AudioKernel CreateKernel() => new RoomK(this, .18f, .35f, 2, .5f, 220, 8000, .6f); }

[AudioEffect("響き 地下駐車場", ["演出：場所の声"], ["響き 地下駐車場", "コンクリートの広い地下の響き", "場所の声", "響き", "地下駐車場", "リバーブ", "reverb", "残響", "場所", "コンクリート", "地下", "演出"])]
public sealed class Fx5A523Effect : FxA2 { public override string Label => "響き 地下駐車場"; internal override AudioKernel CreateKernel() => new RoomK(this, .75f, .3f, 15, .9f, 150, 6000, .55f); }

[AudioEffect("響き 階段の踊り場", ["演出：場所の声"], ["響き 階段の踊り場", "階段にひびく声", "場所の声", "響き", "階段の踊り場", "リバーブ", "reverb", "残響", "場所", "学校", "階段", "演出"])]
public sealed class Fx5A524Effect : FxA2 { public override string Label => "響き 階段の踊り場"; internal override AudioKernel CreateKernel() => new RoomK(this, .5f, .35f, 8, .8f, 200, 8000, .55f); }

[AudioEffect("響き 講堂", ["演出：場所の声"], ["響き 講堂", "講堂の大きな響き", "場所の声", "響き", "講堂", "リバーブ", "reverb", "残響", "場所", "学校", "広い", "演出"])]
public sealed class Fx5A525Effect : FxA2 { public override string Label => "響き 講堂"; internal override AudioKernel CreateKernel() => new RoomK(this, .8f, .35f, 20, .95f, 150, 7000, .55f); }

[AudioEffect("響き 教会", ["演出：場所の声"], ["響き 教会", "石造りの教会の長い響き", "場所の声", "響き", "教会", "リバーブ", "reverb", "残響", "場所", "聖堂", "神聖", "演出"])]
public sealed class Fx5A526Effect : FxA2 { public override string Label => "響き 教会"; internal override AudioKernel CreateKernel() => new RoomK(this, .95f, .3f, 30, 1f, 120, 6500, .6f); }

[AudioEffect("響き 洞窟", ["演出：場所の声"], ["響き 洞窟", "洞窟の暗く長い響き", "場所の声", "響き", "洞窟", "リバーブ", "reverb", "残響", "場所", "冒険", "演出"])]
public sealed class Fx5A527Effect : FxA2 { public override string Label => "響き 洞窟"; internal override AudioKernel CreateKernel() => new RoomK(this, .9f, .5f, 25, 1f, 120, 4000, .6f); }

[AudioEffect("響き 軽自動車の中", ["演出：場所の声"], ["響き 軽自動車の中", "車の中のせまくこもった響き", "場所の声", "響き", "軽自動車の中", "リバーブ", "reverb", "残響", "場所", "車内", "ドライブ", "演出"])]
public sealed class Fx5A528Effect : FxA2 { public override string Label => "響き 軽自動車の中"; internal override AudioKernel CreateKernel() => new RoomK(this, .12f, .7f, 1, .35f, 150, 6000, .5f); }

[AudioEffect("響き 押し入れ", ["演出：場所の声"], ["響き 押し入れ", "布に囲まれたこもった響き", "場所の声", "響き", "押し入れ", "リバーブ", "reverb", "残響", "場所", "こもる", "隠れる", "演出"])]
public sealed class Fx5A529Effect : FxA2 { public override string Label => "響き 押し入れ"; internal override AudioKernel CreateKernel() => new RoomK(this, .1f, .85f, 1, .3f, 200, 3500, .5f); }

[AudioEffect("響き 図書室", ["演出：場所の声"], ["響き 図書室", "本に吸われた落ち着いた響き", "場所の声", "響き", "図書室", "リバーブ", "reverb", "残響", "場所", "静か", "学校", "演出"])]
public sealed class Fx5A530Effect : FxA2 { public override string Label => "響き 図書室"; internal override AudioKernel CreateKernel() => new RoomK(this, .45f, .7f, 8, .7f, 180, 6000, .4f); }

[AudioEffect("響き 病院の廊下", ["演出：場所の声"], ["響き 病院の廊下", "長い廊下にひびく声", "場所の声", "響き", "病院の廊下", "リバーブ", "reverb", "残響", "場所", "学校", "病院", "演出"])]
public sealed class Fx5A531Effect : FxA2 { public override string Label => "響き 病院の廊下"; internal override AudioKernel CreateKernel() => new RoomK(this, .55f, .4f, 10, .85f, 180, 7500, .5f); }

[AudioEffect("響き 音楽ホール", ["演出：場所の声"], ["響き 音楽ホール", "コンサートホールの豊かな響き", "場所の声", "響き", "音楽ホール", "リバーブ", "reverb", "残響", "場所", "コンサート", "音楽", "演出"])]
public sealed class Fx5A532Effect : FxA2 { public override string Label => "響き 音楽ホール"; internal override AudioKernel CreateKernel() => new RoomK(this, .9f, .35f, 35, 1f, 120, 8000, .5f); }

[AudioEffect("響き 屋根裏", ["演出：場所の声"], ["響き 屋根裏", "木の屋根裏の小さな響き", "場所の声", "響き", "屋根裏", "リバーブ", "reverb", "残響", "場所", "木", "小部屋", "演出"])]
public sealed class Fx5A533Effect : FxA2 { public override string Label => "響き 屋根裏"; internal override AudioKernel CreateKernel() => new RoomK(this, .3f, .6f, 4, .55f, 180, 5000, .5f); }

[AudioEffect("響き 空き倉庫", ["演出：場所の声"], ["響き 空き倉庫", "がらんとした倉庫の響き", "場所の声", "響き", "空き倉庫", "リバーブ", "reverb", "残響", "場所", "広い", "空っぽ", "演出"])]
public sealed class Fx5A534Effect : FxA2 { public override string Label => "響き 空き倉庫"; internal override AudioKernel CreateKernel() => new RoomK(this, .7f, .4f, 18, .9f, 160, 6000, .55f); }

[AudioEffect("響き 地下鉄のホーム", ["演出：場所の声"], ["響き 地下鉄のホーム", "地下のホームに広がる響き", "場所の声", "響き", "地下鉄のホーム", "リバーブ", "reverb", "残響", "場所", "駅", "地下", "演出"])]
public sealed class Fx5A535Effect : FxA2 { public override string Label => "響き 地下鉄のホーム"; internal override AudioKernel CreateKernel() => new RoomK(this, .8f, .4f, 22, .95f, 200, 5000, .55f); }

[AudioEffect("響き 石の部屋", ["演出：場所の声"], ["響き 石の部屋", "石の壁にかたく返る響き", "場所の声", "響き", "石の部屋", "リバーブ", "reverb", "残響", "場所", "城", "ダンジョン", "演出"])]
public sealed class Fx5A536Effect : FxA2 { public override string Label => "響き 石の部屋"; internal override AudioKernel CreateKernel() => new RoomK(this, .6f, .2f, 10, .85f, 160, 9000, .6f); }

[AudioEffect("響き 木の部屋", ["演出：場所の声"], ["響き 木の部屋", "あたたかい木の部屋の響き", "場所の声", "響き", "木の部屋", "リバーブ", "reverb", "残響", "場所", "木造", "和室", "演出"])]
public sealed class Fx5A537Effect : FxA2 { public override string Label => "響き 木の部屋"; internal override AudioKernel CreateKernel() => new RoomK(this, .4f, .55f, 6, .7f, 180, 7000, .5f); }

[AudioEffect("響き 電話ボックス", ["演出：場所の声"], ["響き 電話ボックス", "ガラスに囲まれたとても短い響き", "場所の声", "響き", "電話ボックス", "リバーブ", "reverb", "残響", "場所", "せまい", "ボックス", "演出"])]
public sealed class Fx5A538Effect : FxA2 { public override string Label => "響き 電話ボックス"; internal override AudioKernel CreateKernel() => new RoomK(this, .08f, .4f, 1, .25f, 300, 7000, .55f); }

[AudioEffect("響き テントの中", ["演出：場所の声"], ["響き テントの中", "布のテントの中のほぼ響かない音", "場所の声", "響き", "テントの中", "リバーブ", "reverb", "残響", "場所", "キャンプ", "外", "演出"])]
public sealed class Fx5A539Effect : FxA2 { public override string Label => "響き テントの中"; internal override AudioKernel CreateKernel() => new RoomK(this, .12f, .9f, 1, .3f, 200, 5000, .4f); }

[AudioEffect("響き 森の中", ["演出：場所の声"], ["響き 森の中", "木々にかすかに返る響き", "場所の声", "響き", "森の中", "リバーブ", "reverb", "残響", "場所", "森", "自然", "演出"])]
public sealed class Fx5A540Effect : FxA2 { public override string Label => "響き 森の中"; internal override AudioKernel CreateKernel() => new RoomK(this, .65f, .75f, 20, .9f, 250, 5000, .35f); }

[AudioEffect("声 小人", ["演出：声のキャラ"], ["声 小人", "小人のような高く小さい声", "声のキャラ", "声", "小人", "ボイスチェンジャー", "キャラ", "変声", "高い", "かわいい", "演出"])]
public sealed class Fx5A541Effect : FxA2 { public override string Label => "声 小人"; internal override AudioKernel CreateKernel() => new VoiceK(this, 7, 250, 12000, 0, 0); }

[AudioEffect("声 巨人", ["演出：声のキャラ"], ["声 巨人", "巨人のような低く響く声", "声のキャラ", "声", "巨人", "ボイスチェンジャー", "キャラ", "変声", "低い", "大きい", "演出"])]
public sealed class Fx5A542Effect : FxA2 { public override string Label => "声 巨人"; internal override AudioKernel CreateKernel() => new VoiceK(this, -8, 60, 5000, .1f, .25f, .8); }

[AudioEffect("声 子ども", ["演出：声のキャラ"], ["声 子ども", "子どものような明るい声", "声のキャラ", "声", "子ども", "ボイスチェンジャー", "キャラ", "変声", "若い", "高い", "演出"])]
public sealed class Fx5A543Effect : FxA2 { public override string Label => "声 子ども"; internal override AudioKernel CreateKernel() => new VoiceK(this, 4, 200, 11000, 0, 0, .3, 2800, 3); }

[AudioEffect("声 少年", ["演出：声のキャラ"], ["声 少年", "少年のような少し高い声", "声のキャラ", "声", "少年", "ボイスチェンジャー", "キャラ", "変声", "ボーイ", "演出"])]
public sealed class Fx5A544Effect : FxA2 { public override string Label => "声 少年"; internal override AudioKernel CreateKernel() => new VoiceK(this, 3, 180, 10000, .05f, 0, .3, 2000, 2); }

[AudioEffect("声 お姫さま", ["演出：声のキャラ"], ["声 お姫さま", "お姫さまのような高く澄んだ声", "声のキャラ", "声", "お姫さま", "ボイスチェンジャー", "キャラ", "変声", "プリンセス", "上品", "演出"])]
public sealed class Fx5A545Effect : FxA2 { public override string Label => "声 お姫さま"; internal override AudioKernel CreateKernel() => new VoiceK(this, 5, 250, 13000, 0, .15f, .5, 3500, 3); }

[AudioEffect("声 魔王", ["演出：声のキャラ"], ["声 魔王", "魔王のような低く歪んだ声", "声のキャラ", "声", "魔王", "ボイスチェンジャー", "キャラ", "変声", "ボス", "悪役", "演出"])]
public sealed class Fx5A546Effect : FxA2 { public override string Label => "声 魔王"; internal override AudioKernel CreateKernel() => new VoiceK(this, -7, 50, 6000, .35f, .3f, .9, 400, 4); }

[AudioEffect("声 おじいさん", ["演出：声のキャラ"], ["声 おじいさん", "おじいさんのようなかすれた声", "声のキャラ", "声", "おじいさん", "ボイスチェンジャー", "キャラ", "変声", "老人", "年寄り", "演出"])]
public sealed class Fx5A547Effect : FxA2 { public override string Label => "声 おじいさん"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 250, 5500, .1f, 0, .3, 1500, 4); }

[AudioEffect("声 妖精", ["演出：声のキャラ"], ["声 妖精", "妖精のような高くきらめく声", "声のキャラ", "声", "妖精", "ボイスチェンジャー", "キャラ", "変声", "ファンタジー", "小さい", "演出"])]
public sealed class Fx5A548Effect : FxA2 { public override string Label => "声 妖精"; internal override AudioKernel CreateKernel() => new VoiceK(this, 9, 400, 16000, 0, .35f, .6, 5000, 4); }

[AudioEffect("声 怪獣", ["演出：声のキャラ"], ["声 怪獣", "怪獣のような低くうなる声", "声のキャラ", "声", "怪獣", "ボイスチェンジャー", "キャラ", "変声", "モンスター", "ガオー", "演出"])]
public sealed class Fx5A549Effect : FxA2 { public override string Label => "声 怪獣"; internal override AudioKernel CreateKernel() => new VoiceK(this, -10, 40, 4000, .6f, .25f, .8, 300, 6); }

[AudioEffect("声 宇宙人", ["演出：声のキャラ"], ["声 宇宙人", "宇宙人のような高く金属っぽい声", "声のキャラ", "声", "宇宙人", "ボイスチェンジャー", "キャラ", "変声", "エイリアン", "SF", "演出"])]
public sealed class Fx5A550Effect : FxA2 { public override string Label => "声 宇宙人"; internal override AudioKernel CreateKernel() => new VoiceK(this, 6, 500, 7000, .3f, .2f, .5, 2200, 8); }

[AudioEffect("声 アナウンサー", ["演出：声のキャラ"], ["声 アナウンサー", "はっきり通るアナウンサーの声", "声のキャラ", "声", "アナウンサー", "ボイスチェンジャー", "キャラ", "変声", "ニュース", "明瞭", "演出"])]
public sealed class Fx5A551Effect : FxA2 { public override string Label => "声 アナウンサー"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 120, 12000, .05f, .05f, .3, 3000, 3); }

[AudioEffect("声 店内放送", ["演出：声のキャラ"], ["声 店内放送", "お店の天井スピーカーのような声", "声のキャラ", "声", "店内放送", "ボイスチェンジャー", "キャラ", "変声", "スーパー", "放送", "演出"])]
public sealed class Fx5A552Effect : FxA2 { public override string Label => "声 店内放送"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 400, 6000, .1f, .3f, .7, 1800, 4); }

[AudioEffect("声 ラジオDJ", ["演出：声のキャラ"], ["声 ラジオDJ", "低音の効いたラジオDJの声", "声のキャラ", "声", "ラジオDJ", "ボイスチェンジャー", "キャラ", "変声", "ラジオ", "低音", "演出"])]
public sealed class Fx5A553Effect : FxA2 { public override string Label => "声 ラジオDJ"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 90, 11000, .15f, 0, .3, 150, 4); }

[AudioEffect("声 ナレーター", ["演出：声のキャラ"], ["声 ナレーター", "落ち着いた低めのナレーターの声", "声のキャラ", "声", "ナレーター", "ボイスチェンジャー", "キャラ", "変声", "語り", "解説", "演出"])]
public sealed class Fx5A554Effect : FxA2 { public override string Label => "声 ナレーター"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 70, 10000, .05f, .05f, .4, 200, 3); }

[AudioEffect("声 ぬいぐるみ", ["演出：声のキャラ"], ["声 ぬいぐるみ", "ぬいぐるみが話すようなこもった高い声", "声のキャラ", "声", "ぬいぐるみ", "ボイスチェンジャー", "キャラ", "変声", "かわいい", "マスコット", "演出"])]
public sealed class Fx5A555Effect : FxA2 { public override string Label => "声 ぬいぐるみ"; internal override AudioKernel CreateKernel() => new VoiceK(this, 5, 300, 6000, 0, 0, .3, 1500, 6); }

[AudioEffect("声 幽霊", ["演出：声のキャラ"], ["声 幽霊", "遠くから響く幽霊のような声", "声のキャラ", "声", "幽霊", "ボイスチェンジャー", "キャラ", "変声", "おばけ", "ホラー", "演出"])]
public sealed class Fx5A556Effect : FxA2 { public override string Label => "声 幽霊"; internal override AudioKernel CreateKernel() => new VoiceK(this, 2, 300, 6000, 0, .6f, .95, 2500, 4); }

[AudioEffect("声 機械の兵士", ["演出：声のキャラ"], ["声 機械の兵士", "機械の兵士のような歪んだ声", "声のキャラ", "声", "機械の兵士", "ボイスチェンジャー", "キャラ", "変声", "ロボット", "兵士", "演出"])]
public sealed class Fx5A557Effect : FxA2 { public override string Label => "声 機械の兵士"; internal override AudioKernel CreateKernel() => new VoiceK(this, -3, 300, 5000, .7f, .15f, .4, 1500, 8); }

[AudioEffect("声 ねずみ", ["演出：声のキャラ"], ["声 ねずみ", "ねずみのようなとても高い声", "声のキャラ", "声", "ねずみ", "ボイスチェンジャー", "キャラ", "変声", "動物", "チュー", "演出"])]
public sealed class Fx5A558Effect : FxA2 { public override string Label => "声 ねずみ"; internal override AudioKernel CreateKernel() => new VoiceK(this, 10, 500, 14000, 0, 0, .3, 4000, 4); }

[AudioEffect("声 くま", ["演出：声のキャラ"], ["声 くま", "くまのような低くもこもこした声", "声のキャラ", "声", "くま", "ボイスチェンジャー", "キャラ", "変声", "動物", "のんびり", "演出"])]
public sealed class Fx5A559Effect : FxA2 { public override string Label => "声 くま"; internal override AudioKernel CreateKernel() => new VoiceK(this, -5, 70, 6000, .1f, 0, .4, 400, 5); }

[AudioEffect("声 ひそひそ", ["演出：声のキャラ"], ["声 ひそひそ", "小声で話すような細い声", "声のキャラ", "声", "ひそひそ", "ボイスチェンジャー", "キャラ", "変声", "ささやき", "内緒", "演出"])]
public sealed class Fx5A560Effect : FxA2 { public override string Label => "声 ひそひそ"; internal override AudioKernel CreateKernel() => new VoiceK(this, 1, 600, 9000, .05f, .1f, .3, 5000, 5); }

[AudioEffect("揺れ 扇風機の前", ["演出：変調"], ["揺れ 扇風機の前", "扇風機の前で話すような音量の揺れ", "変調", "揺れ", "扇風機の前", "モジュレーション", "うねり", "扇風機", "夏", "演出"])]
public sealed class Fx5A561Effect : FxA3 { public override string Label => "揺れ 扇風機の前"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 8, 16, 0); }

[AudioEffect("揺れ 回転スピーカー", ["演出：変調"], ["揺れ 回転スピーカー", "回るスピーカーのようなうねり", "変調", "揺れ", "回転スピーカー", "モジュレーション", "うねり", "オルガン", "演出"])]
public sealed class Fx5A562Effect : FxA3 { public override string Label => "揺れ 回転スピーカー"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 6, 3, 3, 7, .1f, .8f); }

[AudioEffect("揺れ 水の中", ["演出：変調"], ["揺れ 水の中", "水の中でゆらぐようなこもった音", "変調", "揺れ", "水の中", "モジュレーション", "うねり", "水中", "ぶくぶく", "演出"])]
public sealed class Fx5A563Effect : FxA3 { public override string Label => "揺れ 水の中"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 1500, 0, 1.5, .1, .5); }

[AudioEffect("揺れ 酔った声", ["演出：変調"], ["揺れ 酔った声", "音程がふらふら揺れる声", "変調", "揺れ", "酔った声", "モジュレーション", "うねり", "ふらふら", "酔う", "演出"])]
public sealed class Fx5A564Effect : FxA3 { public override string Label => "揺れ 酔った声"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 12, 8, .3, 1, 0f, 1f); }

[AudioEffect("揺れ 宇宙のうなり", ["演出：変調"], ["揺れ 宇宙のうなり", "ゆっくりうなる宇宙っぽい音", "変調", "揺れ", "宇宙のうなり", "モジュレーション", "うねり", "SF", "うなり", "演出"])]
public sealed class Fx5A565Effect : FxA3 { public override string Label => "揺れ 宇宙のうなり"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 3, 2.5, .1, .3, .8f, 1f); }

[AudioEffect("揺れ 電波が悪い", ["演出：変調"], ["揺れ 電波が悪い", "電波が途切れるように音が切れる", "変調", "揺れ", "電波が悪い", "モジュレーション", "うねり", "途切れる", "通信", "演出"])]
public sealed class Fx5A566Effect : FxA3 { public override string Label => "揺れ 電波が悪い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 3, 12, 1); }

[AudioEffect("揺れ 鼓動", ["演出：変調"], ["揺れ 鼓動", "鼓動に合わせて音が脈打つ", "変調", "揺れ", "鼓動", "モジュレーション", "うねり", "ドキドキ", "心臓", "演出"])]
public sealed class Fx5A567Effect : FxA3 { public override string Label => "揺れ 鼓動"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 1.2, 2.5, 3); }

[AudioEffect("揺れ ぐるぐる左右", ["演出：変調"], ["揺れ ぐるぐる左右", "音が左右をぐるぐる回る", "変調", "揺れ", "ぐるぐる左右", "モジュレーション", "うねり", "ステレオ", "回る", "演出"])]
public sealed class Fx5A568Effect : FxA3 { public override string Label => "揺れ ぐるぐる左右"; internal override AudioKernel CreateKernel() => new GainLfoK(this, .5, 3, 0, true); }

[AudioEffect("揺れ 波の満ち引き", ["演出：変調"], ["揺れ 波の満ち引き", "波のようにゆっくり大きくなったり小さくなったりする", "変調", "揺れ", "波の満ち引き", "モジュレーション", "うねり", "海", "ゆったり", "演出"])]
public sealed class Fx5A569Effect : FxA3 { public override string Label => "揺れ 波の満ち引き"; internal override AudioKernel CreateKernel() => new GainLfoK(this, .1, .4, 0); }

[AudioEffect("揺れ ワウ 速い", ["演出：変調"], ["揺れ ワウ 速い", "わうわうと速く鳴るフィルター", "変調", "揺れ", "ワウ", "速い", "モジュレーション", "うねり", "ギター", "ファンク", "演出"])]
public sealed class Fx5A570Effect : FxA3 { public override string Label => "揺れ ワウ 速い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 3000, 1, 2, 2, 6); }

[AudioEffect("揺れ ワウ ゆっくり", ["演出：変調"], ["揺れ ワウ ゆっくり", "わうわうとゆっくり鳴るフィルター", "変調", "揺れ", "ワウ", "ゆっくり", "モジュレーション", "うねり", "ギター", "演出"])]
public sealed class Fx5A571Effect : FxA3 { public override string Label => "揺れ ワウ ゆっくり"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 3000, 1, 2, .2, .8); }

[AudioEffect("揺れ ジェット機", ["演出：変調"], ["揺れ ジェット機", "ジェット機が通るようなシュワーという音", "変調", "揺れ", "ジェット機", "モジュレーション", "うねり", "フランジャー", "飛行機", "演出"])]
public sealed class Fx5A572Effect : FxA3 { public override string Label => "揺れ ジェット機"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 1.5, 1.4, .05, .3, .85f, 1f); }

[AudioEffect("揺れ 古いレコード", ["演出：変調"], ["揺れ 古いレコード", "レコードの回転のような小さな音程の揺れ", "変調", "揺れ", "古いレコード", "モジュレーション", "うねり", "レトロ", "昔", "演出"])]
public sealed class Fx5A573Effect : FxA3 { public override string Label => "揺れ 古いレコード"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 8, 1.5, .4, .9, 0f, 1f); }

[AudioEffect("揺れ 合唱", ["演出：変調"], ["揺れ 合唱", "何人かで歌っているように厚くする", "変調", "揺れ", "合唱", "モジュレーション", "うねり", "コーラス", "大勢", "演出"])]
public sealed class Fx5A574Effect : FxA3 { public override string Label => "揺れ 合唱"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 30, 12, .2, 1, .05f, 1f); }

[AudioEffect("揺れ 双子の声", ["演出：変調"], ["揺れ 双子の声", "同じ声が2人で話しているようにする", "変調", "揺れ", "双子の声", "モジュレーション", "うねり", "ダブル", "二人", "演出"])]
public sealed class Fx5A575Effect : FxA3 { public override string Label => "揺れ 双子の声"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 16, 4, .4, 1.2, 0f, 1f); }

[AudioEffect("揺れ 風の音", ["演出：変調"], ["揺れ 風の音", "風が吹くようにゆらぐ帯域", "変調", "揺れ", "風の音", "モジュレーション", "うねり", "風", "ひゅー", "演出"])]
public sealed class Fx5A576Effect : FxA3 { public override string Label => "揺れ 風の音"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 600, 6000, 1, 1.2, .1, .5); }

[AudioEffect("揺れ 点滅音量", ["演出：変調"], ["揺れ 点滅音量", "左右で交互に音が点滅する", "変調", "揺れ", "点滅音量", "モジュレーション", "うねり", "トレモロ", "左右", "演出"])]
public sealed class Fx5A577Effect : FxA3 { public override string Label => "揺れ 点滅音量"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 6, 14, 1, true); }

[AudioEffect("揺れ のこぎり音量", ["演出：変調"], ["揺れ のこぎり音量", "音量がのこぎり形に変わる", "変調", "揺れ", "のこぎり音量", "モジュレーション", "うねり", "リズム", "刻む", "演出"])]
public sealed class Fx5A578Effect : FxA3 { public override string Label => "揺れ のこぎり音量"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 2, 8, 2); }

[AudioEffect("揺れ ヘリの中", ["演出：変調"], ["揺れ ヘリの中", "ヘリコプターの中のような細かい揺れ", "変調", "揺れ", "ヘリの中", "モジュレーション", "うねり", "ヘリ", "振動", "演出"])]
public sealed class Fx5A579Effect : FxA3 { public override string Label => "揺れ ヘリの中"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 14, 24, 3); }

[AudioEffect("揺れ 夢の中", ["演出：変調"], ["揺れ 夢の中", "ゆっくり明るさが変わる夢のような音", "変調", "揺れ", "夢の中", "モジュレーション", "うねり", "回想", "ぼんやり", "演出"])]
public sealed class Fx5A580Effect : FxA3 { public override string Label => "揺れ 夢の中"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 500, 9000, 0, .9, .05, .3); }

[AudioEffect("場面 トンネルの無線", ["演出：場面の音"], ["場面 トンネルの無線", "トンネルの中で無線を聞いているような音", "場面の音", "場面", "トンネルの無線", "シーン", "効果音", "組み合わせ", "無線", "トンネル", "演出"])]
public sealed class Fx5A581Effect : FxA3 { public override string Label => "場面 トンネルの無線"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 500, 3000), new EchoK(this, .12, .14, .6f, 200, 3500, .5f)); }

[AudioEffect("場面 体育館の放送", ["演出：場面の音"], ["場面 体育館の放送", "体育館のスピーカーで流れる放送", "場面の音", "場面", "体育館の放送", "シーン", "効果音", "組み合わせ", "学校", "放送", "演出"])]
public sealed class Fx5A582Effect : FxA3 { public override string Label => "場面 体育館の放送"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 400, 5000), new RoomK(this, .8f, .35f, 20, .95f, 150, 7000, .55f)); }

[AudioEffect("場面 遠くの拡声器", ["演出：場面の音"], ["場面 遠くの拡声器", "遠くの拡声器から聞こえる声", "場面の音", "場面", "遠くの拡声器", "シーン", "効果音", "組み合わせ", "選挙", "拡声器", "演出"])]
public sealed class Fx5A583Effect : FxA3 { public override string Label => "場面 遠くの拡声器"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 600, 3500, 2), new EchoK(this, .3, .35, .4f, 400, 3000, .45f)); }

[AudioEffect("場面 夢の中の声", ["演出：場面の音"], ["場面 夢の中の声", "ぼんやり響く夢の中の声", "場面の音", "場面", "夢の中の声", "シーン", "効果音", "組み合わせ", "回想", "夢", "演出"])]
public sealed class Fx5A584Effect : FxA3 { public override string Label => "場面 夢の中の声"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .7f, .5f, 20, .9f, 200, 6000, .6f), new FilterLfoK(this, 600, 8000, 0, .9, .05, .3)); }

[AudioEffect("場面 電話の向こうの部屋", ["演出：場面の音"], ["場面 電話の向こうの部屋", "電話の向こうの部屋で話している声", "場面の音", "場面", "電話の向こうの部屋", "シーン", "効果音", "組み合わせ", "電話", "通話", "演出"])]
public sealed class Fx5A585Effect : FxA3 { public override string Label => "場面 電話の向こうの部屋"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .4f, .4f, 6, .7f, 200, 8000, .5f), new BandK(this, 400, 3200)); }

[AudioEffect("場面 壊れたロボ", ["演出：場面の音"], ["場面 壊れたロボ", "壊れかけたロボットのような声", "場面の音", "場面", "壊れたロボ", "シーン", "効果音", "組み合わせ", "ロボット", "故障", "演出"])]
public sealed class Fx5A586Effect : FxA3 { public override string Label => "場面 壊れたロボ"; internal override AudioKernel CreateKernel() => new ChainK(new ShapeK(this, x => MathF.Round(x * 5f) / 5f, 3, 14, 6000, 0), new ModDelayK(this, 4, 3, 4, 9, .5f, .8f)); }

[AudioEffect("場面 古いラジオ劇", ["演出：場面の音"], ["場面 古いラジオ劇", "昔のラジオドラマのような音", "場面の音", "場面", "古いラジオ劇", "シーン", "効果音", "組み合わせ", "レトロ", "ラジオ", "演出"])]
public sealed class Fx5A587Effect : FxA3 { public override string Label => "場面 古いラジオ劇"; internal override AudioKernel CreateKernel() => new ChainK(new TapeK(this, true), new BandK(this, 350, 4000)); }

[AudioEffect("場面 水中で話す", ["演出：場面の音"], ["場面 水中で話す", "水の中で話しているような音", "場面の音", "場面", "水中で話す", "シーン", "効果音", "組み合わせ", "水中", "プール", "演出"])]
public sealed class Fx5A588Effect : FxA3 { public override string Label => "場面 水中で話す"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 80, 900), new ModDelayK(this, 10, 6, .3, 1, .2f, .8f)); }

[AudioEffect("場面 宇宙の通信", ["演出：場面の音"], ["場面 宇宙の通信", "途切れながら届く宇宙からの通信", "場面の音", "場面", "宇宙の通信", "シーン", "効果音", "組み合わせ", "SF", "宇宙", "演出"])]
public sealed class Fx5A589Effect : FxA3 { public override string Label => "場面 宇宙の通信"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 500, 4000, 2), new EchoK(this, .09, .11, .4f, 500, 5000, .4f, true), new GainLfoK(this, 4, 10, 1)); }

[AudioEffect("場面 洞窟の奥の声", ["演出：場面の音"], ["場面 洞窟の奥の声", "洞窟の奥から響く低い声", "場面の音", "場面", "洞窟の奥の声", "シーン", "効果音", "組み合わせ", "洞窟", "低い", "演出"])]
public sealed class Fx5A590Effect : FxA3 { public override string Label => "場面 洞窟の奥の声"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, -3, 60, 6000, .1f, 0), new RoomK(this, .9f, .5f, 25, 1f, 120, 4000, .6f)); }

[AudioEffect("場面 ライブ会場", ["演出：場面の音"], ["場面 ライブ会場", "ライブ会場のような迫力のある響き", "場面の音", "場面", "ライブ会場", "シーン", "効果音", "組み合わせ", "コンサート", "ライブ", "演出"])]
public sealed class Fx5A591Effect : FxA3 { public override string Label => "場面 ライブ会場"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (100, .7, 3), (3000, 1, 2)), new RoomK(this, .85f, .4f, 25, 1f, 120, 8000, .45f)); }

[AudioEffect("場面 夜の駅のアナウンス", ["演出：場面の音"], ["場面 夜の駅のアナウンス", "夜の駅に響くアナウンス", "場面の音", "場面", "夜の駅のアナウンス", "シーン", "効果音", "組み合わせ", "駅", "放送", "演出"])]
public sealed class Fx5A592Effect : FxA3 { public override string Label => "場面 夜の駅のアナウンス"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 450, 5000), new EchoK(this, .25, .3, .35f, 400, 4000, .4f), new RoomK(this, .7f, .4f, 15, .9f, 200, 5000, .4f)); }

[AudioEffect("場面 窓ごしの声", ["演出：場面の音"], ["場面 窓ごしの声", "窓ガラスの向こうから聞こえる声", "場面の音", "場面", "窓ごしの声", "シーン", "効果音", "組み合わせ", "外", "こもる", "演出"])]
public sealed class Fx5A593Effect : FxA3 { public override string Label => "場面 窓ごしの声"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 150, 1800), new RoomK(this, .25f, .7f, 3, .45f, 160, 5000, .4f)); }

[AudioEffect("場面 ホラーのささやき", ["演出：場面の音"], ["場面 ホラーのささやき", "不気味にゆらぐささやき声", "場面の音", "場面", "ホラーのささやき", "シーン", "効果音", "組み合わせ", "ホラー", "こわい", "演出"])]
public sealed class Fx5A594Effect : FxA3 { public override string Label => "場面 ホラーのささやき"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, -2, 400, 8000, 0, .3f, .8), new ModDelayK(this, 14, 6, .2, .6, .2f, .9f)); }

[AudioEffect("場面 ゲームのボス", ["演出：場面の音"], ["場面 ゲームのボス", "ゲームのボスのような迫力のある声", "場面の音", "場面", "ゲームのボス", "シーン", "効果音", "組み合わせ", "ボス", "魔王", "演出"])]
public sealed class Fx5A595Effect : FxA3 { public override string Label => "場面 ゲームのボス"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, -5, 50, 7000, .4f, .2f, .8, 300, 5), new RoomK(this, .85f, .35f, 25, 1f, 100, 7000, .5f)); }

[AudioEffect("場面 留守番電話", ["演出：場面の音"], ["場面 留守番電話", "カセットの留守番電話のような声", "場面の音", "場面", "留守番電話", "シーン", "効果音", "組み合わせ", "電話", "録音", "演出"])]
public sealed class Fx5A596Effect : FxA3 { public override string Label => "場面 留守番電話"; internal override AudioKernel CreateKernel() => new ChainK(new TapeK(this, true), new BandK(this, 400, 3000, 2)); }

[AudioEffect("場面 遠くの山びこ", ["演出：場面の音"], ["場面 遠くの山びこ", "遠くの山に響く声", "場面の音", "場面", "遠くの山びこ", "シーン", "効果音", "組み合わせ", "山", "ヤッホー", "演出"])]
public sealed class Fx5A597Effect : FxA3 { public override string Label => "場面 遠くの山びこ"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 300, 4000), new EchoK(this, .6, .7, .55f, 300, 3000, .55f)); }

[AudioEffect("場面 映画館の後ろの席", ["演出：場面の音"], ["場面 映画館の後ろの席", "映画館の後ろの席で聞いているような音", "場面の音", "場面", "映画館の後ろの席", "シーン", "効果音", "組み合わせ", "映画", "劇場", "演出"])]
public sealed class Fx5A598Effect : FxA3 { public override string Label => "場面 映画館の後ろの席"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (80, .7, 4), (8000, .8, -4)), new RoomK(this, .8f, .45f, 20, .95f, 120, 7000, .45f)); }

[AudioEffect("場面 となりの部屋", ["演出：場面の音"], ["場面 となりの部屋", "壁の向こうのとなりの部屋から聞こえる声", "場面の音", "場面", "となりの部屋", "シーン", "効果音", "組み合わせ", "壁ごし", "こもる", "演出"])]
public sealed class Fx5A599Effect : FxA3 { public override string Label => "場面 となりの部屋"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 80, 1200), new RoomK(this, .35f, .6f, 5, .6f, 100, 1500, .5f)); }

[AudioEffect("場面 マイクが遠い", ["演出：場面の音"], ["場面 マイクが遠い", "マイクから離れて話しているような声", "場面の音", "場面", "マイクが遠い", "シーン", "効果音", "組み合わせ", "オフマイク", "遠い", "演出"])]
public sealed class Fx5A600Effect : FxA3 { public override string Label => "場面 マイクが遠い"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (150, .7, -6), (5000, .7, -4)), new RoomK(this, .45f, .45f, 10, .75f, 200, 7000, .6f)); }
