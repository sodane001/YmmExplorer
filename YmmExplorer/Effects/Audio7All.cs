using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 701〜800(gen_v137.py で生成)

[AudioEffect("音色 あたたかい", ["演出：音質"], ["音色 あたたかい", "低めを足して高い所をおさえ、あたたかい音にする", "音質", "音色", "あたたかい", "EQ", "イコライザー", "ウォーム", "やさしい", "演出"])]
public sealed class Fx7A701Effect : FxA2 { public override string Label => "音色 あたたかい"; internal override AudioKernel CreateKernel() => new EqK(this, (200, .7, 3), (8000, .7, -4)); }

[AudioEffect("音色 明るい", ["演出：音質"], ["音色 明るい", "高い所を足して明るい音にする", "音質", "音色", "明るい", "EQ", "イコライザー", "ブライト", "演出"])]
public sealed class Fx7A702Effect : FxA2 { public override string Label => "音色 明るい"; internal override AudioKernel CreateKernel() => new EqK(this, (150, .7, -2), (6000, .7, 4)); }

[AudioEffect("音色 薄い", ["演出：音質"], ["音色 薄い", "低い所を大きく削って薄い音にする", "音質", "音色", "薄い", "EQ", "イコライザー", "ペラペラ", "演出"])]
public sealed class Fx7A703Effect : FxA2 { public override string Label => "音色 薄い"; internal override AudioKernel CreateKernel() => new EqK(this, (250, .7, -9), (1200, 1, 2)); }

[AudioEffect("音色 太い", ["演出：音質"], ["音色 太い", "低い所を足して太い音にする", "音質", "音色", "太い", "EQ", "イコライザー", "厚み", "重い", "演出"])]
public sealed class Fx7A704Effect : FxA2 { public override string Label => "音色 太い"; internal override AudioKernel CreateKernel() => new EqK(this, (120, .8, 6), (400, 1, 2)); }

[AudioEffect("音色 鼻声", ["演出：音質"], ["音色 鼻声", "鼻にかかったような声にする", "音質", "音色", "鼻声", "EQ", "イコライザー", "風邪", "鼻づまり", "演出"])]
public sealed class Fx7A705Effect : FxA2 { public override string Label => "音色 鼻声"; internal override AudioKernel CreateKernel() => new EqK(this, (900, 3, 8), (200, .7, -6)); }

[AudioEffect("音色 耳元", ["演出：音質"], ["音色 耳元", "耳元で話すような近い音にする", "音質", "音色", "耳元", "EQ", "イコライザー", "近い", "ASMR", "演出"])]
public sealed class Fx7A706Effect : FxA2 { public override string Label => "音色 耳元"; internal override AudioKernel CreateKernel() => new EqK(this, (150, .7, 5), (5000, .7, 3)); }

[AudioEffect("音色 ドンシャリ", ["演出：音質"], ["音色 ドンシャリ", "低い所と高い所を強くする", "音質", "音色", "ドンシャリ", "EQ", "イコライザー", "迫力", "派手", "演出"])]
public sealed class Fx7A707Effect : FxA2 { public override string Label => "音色 ドンシャリ"; internal override AudioKernel CreateKernel() => new EqK(this, (80, .7, 6), (10000, .7, 6)); }

[AudioEffect("音色 中域カット", ["演出：音質"], ["音色 中域カット", "真ん中の高さを削ってすっきりさせる", "音質", "音色", "中域カット", "EQ", "イコライザー", "すっきり", "演出"])]
public sealed class Fx7A708Effect : FxA2 { public override string Label => "音色 中域カット"; internal override AudioKernel CreateKernel() => new EqK(this, (1000, .8, -8)); }

[AudioEffect("音色 声を前に", ["演出：音質"], ["音色 声を前に", "声がはっきり前に出るようにする", "音質", "音色", "声を前に", "EQ", "イコライザー", "聞き取りやすい", "演出"])]
public sealed class Fx7A709Effect : FxA2 { public override string Label => "音色 声を前に"; internal override AudioKernel CreateKernel() => new EqK(this, (3000, 1, 5), (250, .8, -3)); }

[AudioEffect("音色 声を奥に", ["演出：音質"], ["音色 声を奥に", "声が奥に引っこんだようにする", "音質", "音色", "声を奥に", "EQ", "イコライザー", "遠い", "演出"])]
public sealed class Fx7A710Effect : FxA2 { public override string Label => "音色 声を奥に"; internal override AudioKernel CreateKernel() => new EqK(this, (3000, 1, -6), (8000, .7, -6)); }

[AudioEffect("音色 ガリガリ", ["演出：音質"], ["音色 ガリガリ", "ガリガリと強く歪ませる", "音質", "音色", "ガリガリ", "EQ", "イコライザー", "ファズ", "ノイズ", "演出"])]
public sealed class Fx7A711Effect : FxA2 { public override string Label => "音色 ガリガリ"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Sign(x) * MathF.Min(1f, MathF.Abs(x) * 3f), 14, 30, 4000, 3); }

[AudioEffect("音色 ザラザラ", ["演出：音質"], ["音色 ザラザラ", "音をザラザラに粗くする", "音質", "音色", "ザラザラ", "EQ", "イコライザー", "ローファイ", "8bit", "演出"])]
public sealed class Fx7A712Effect : FxA2 { public override string Label => "音色 ザラザラ"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => MathF.Round(x * 10f) / 10f, 3, 12, 9000, 0); }

[AudioEffect("音色 割れた音", ["演出：音質"], ["音色 割れた音", "マイクが割れたような音にする", "音質", "音色", "割れた音", "EQ", "イコライザー", "クリップ", "叫び", "演出"])]
public sealed class Fx7A713Effect : FxA2 { public override string Label => "音色 割れた音"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => Math.Clamp(x * 2f, -.5f, .5f), 8, 24, 6000, 4); }

[AudioEffect("音色 やわらか歪み", ["演出：音質"], ["音色 やわらか歪み", "ほんのり歪ませて厚くする", "音質", "音色", "やわらか歪み", "EQ", "イコライザー", "サチュレーション", "演出"])]
public sealed class Fx7A714Effect : FxA2 { public override string Label => "音色 やわらか歪み"; internal override AudioKernel CreateKernel() => new ShapeK(this, x => x / (1f + MathF.Abs(x)), 2, 12, 12000, 0); }

[AudioEffect("音色 ブーン", ["演出：音質"], ["音色 ブーン", "こもったブーンという歪み", "音質", "音色", "ブーン", "EQ", "イコライザー", "こもる", "低い", "演出"])]
public sealed class Fx7A715Effect : FxA2 { public override string Label => "音色 ブーン"; internal override AudioKernel CreateKernel() => new ShapeK(this, MathF.Tanh, 10, 26, 1500, 2); }

[AudioEffect("音色 シャキッと", ["演出：音質"], ["音色 シャキッと", "シャキッと歯切れよくする", "音質", "音色", "シャキッと", "EQ", "イコライザー", "クリア", "演出"])]
public sealed class Fx7A716Effect : FxA2 { public override string Label => "音色 シャキッと"; internal override AudioKernel CreateKernel() => new EqK(this, (4500, 1.2, 5), (12000, .7, 3), (300, .8, -3)); }

[AudioEffect("音色 こもり 強く", ["演出：音質"], ["音色 こもり 強く", "布をかぶったように強くこもらせる", "音質", "音色", "こもり", "強く", "EQ", "イコライザー", "布", "こもる", "演出"])]
public sealed class Fx7A717Effect : FxA2 { public override string Label => "音色 こもり 強く"; internal override AudioKernel CreateKernel() => new EqK(this, (2000, .7, -12), (5000, .7, -12)); }

[AudioEffect("音色 低音だけ", ["演出：音質"], ["音色 低音だけ", "低い音だけを残すようにする", "音質", "音色", "低音だけ", "EQ", "イコライザー", "ベース", "振動", "演出"])]
public sealed class Fx7A718Effect : FxA2 { public override string Label => "音色 低音だけ"; internal override AudioKernel CreateKernel() => new EqK(this, (300, .7, -15), (80, .7, 6)); }

[AudioEffect("音色 高音だけ", ["演出：音質"], ["音色 高音だけ", "高い音だけを残すようにする", "音質", "音色", "高音だけ", "EQ", "イコライザー", "シャカシャカ", "演出"])]
public sealed class Fx7A719Effect : FxA2 { public override string Label => "音色 高音だけ"; internal override AudioKernel CreateKernel() => new EqK(this, (1500, .7, -15), (6000, .7, 4)); }

[AudioEffect("音色 へこんだ音", ["演出：音質"], ["音色 へこんだ音", "ところどころへこんだ不思議な音にする", "音質", "音色", "へこんだ音", "EQ", "イコライザー", "不思議", "変", "演出"])]
public sealed class Fx7A720Effect : FxA2 { public override string Label => "音色 へこんだ音"; internal override AudioKernel CreateKernel() => new EqK(this, (500, 2, -10), (2000, 2, -10)); }

[AudioEffect("ハモり 長3度上", ["演出：変調"], ["ハモり 長3度上", "長3度上の声を重ねて明るくハモる", "変調", "ハモり", "長3度上", "ハーモニー", "ピッチ", "重ねる", "明るい", "演出"])]
public sealed class Fx7A721Effect : FxA2 { public override string Label => "ハモり 長3度上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 4 / 12.0), .7f); }

[AudioEffect("ハモり 短3度上", ["演出：変調"], ["ハモり 短3度上", "短3度上の声を重ねて切なくハモる", "変調", "ハモり", "短3度上", "ハーモニー", "ピッチ", "重ねる", "切ない", "演出"])]
public sealed class Fx7A722Effect : FxA2 { public override string Label => "ハモり 短3度上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 3 / 12.0), .7f); }

[AudioEffect("ハモり 4度上", ["演出：変調"], ["ハモり 4度上", "4度上の声を重ねる", "変調", "ハモり", "4度上", "ハーモニー", "ピッチ", "重ねる", "演出"])]
public sealed class Fx7A723Effect : FxA2 { public override string Label => "ハモり 4度上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 5 / 12.0), .65f); }

[AudioEffect("ハモり 5度上", ["演出：変調"], ["ハモり 5度上", "5度上の声を重ねて力強くする", "変調", "ハモり", "5度上", "ハーモニー", "ピッチ", "重ねる", "力強い", "演出"])]
public sealed class Fx7A724Effect : FxA2 { public override string Label => "ハモり 5度上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 7 / 12.0), .65f); }

[AudioEffect("ハモり 6度上", ["演出：変調"], ["ハモり 6度上", "6度上の声を重ねる", "変調", "ハモり", "6度上", "ハーモニー", "ピッチ", "重ねる", "演出"])]
public sealed class Fx7A725Effect : FxA2 { public override string Label => "ハモり 6度上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 9 / 12.0), .6f); }

[AudioEffect("ハモり オクターブと5度", ["演出：変調"], ["ハモり オクターブと5度", "1オクターブと5度上を重ねて輝かせる", "変調", "ハモり", "オクターブと5度", "ハーモニー", "ピッチ", "重ねる", "キラキラ", "演出"])]
public sealed class Fx7A726Effect : FxA2 { public override string Label => "ハモり オクターブと5度"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 19 / 12.0), .5f); }

[AudioEffect("ハモり 3度下", ["演出：変調"], ["ハモり 3度下", "3度下の声を重ねる", "変調", "ハモり", "3度下", "ハーモニー", "ピッチ", "重ねる", "低い", "演出"])]
public sealed class Fx7A727Effect : FxA2 { public override string Label => "ハモり 3度下"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, -4 / 12.0), .7f); }

[AudioEffect("ハモり 4度下", ["演出：変調"], ["ハモり 4度下", "4度下の声を重ねる", "変調", "ハモり", "4度下", "ハーモニー", "ピッチ", "重ねる", "低い", "演出"])]
public sealed class Fx7A728Effect : FxA2 { public override string Label => "ハモり 4度下"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, -5 / 12.0), .65f); }

[AudioEffect("ハモり 5度下", ["演出：変調"], ["ハモり 5度下", "5度下の声を重ねて重くする", "変調", "ハモり", "5度下", "ハーモニー", "ピッチ", "重ねる", "重い", "演出"])]
public sealed class Fx7A729Effect : FxA2 { public override string Label => "ハモり 5度下"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, -7 / 12.0), .65f); }

[AudioEffect("ハモり 2オクターブ上", ["演出：変調"], ["ハモり 2オクターブ上", "2オクターブ上のとても高い声を重ねる", "変調", "ハモり", "2オクターブ上", "ハーモニー", "ピッチ", "重ねる", "高い", "小人", "演出"])]
public sealed class Fx7A730Effect : FxA2 { public override string Label => "ハモり 2オクターブ上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, 4, .45f); }

[AudioEffect("ハモり 2オクターブ下", ["演出：変調"], ["ハモり 2オクターブ下", "2オクターブ下のとても低い声を重ねる", "変調", "ハモり", "2オクターブ下", "ハーモニー", "ピッチ", "重ねる", "低い", "怪物", "演出"])]
public sealed class Fx7A731Effect : FxA2 { public override string Label => "ハモり 2オクターブ下"; internal override AudioKernel CreateKernel() => new PitchAddK(this, .25, .5f); }

[AudioEffect("ハモり 半音上", ["演出：変調"], ["ハモり 半音上", "半音上を重ねて不気味にする", "変調", "ハモり", "半音上", "ハーモニー", "ピッチ", "重ねる", "不気味", "ホラー", "演出"])]
public sealed class Fx7A732Effect : FxA2 { public override string Label => "ハモり 半音上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 1 / 12.0), .6f); }

[AudioEffect("ハモり 半音下", ["演出：変調"], ["ハモり 半音下", "半音下を重ねて不安な感じにする", "変調", "ハモり", "半音下", "ハーモニー", "ピッチ", "重ねる", "不安", "ホラー", "演出"])]
public sealed class Fx7A733Effect : FxA2 { public override string Label => "ハモり 半音下"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, -1 / 12.0), .6f); }

[AudioEffect("ハモり 全音上", ["演出：変調"], ["ハモり 全音上", "全音上を重ねる", "変調", "ハモり", "全音上", "ハーモニー", "ピッチ", "重ねる", "にごり", "演出"])]
public sealed class Fx7A734Effect : FxA2 { public override string Label => "ハモり 全音上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 2 / 12.0), .6f); }

[AudioEffect("ハモり 7度上", ["演出：変調"], ["ハモり 7度上", "7度上を重ねておしゃれにする", "変調", "ハモり", "7度上", "ハーモニー", "ピッチ", "重ねる", "ジャズ", "おしゃれ", "演出"])]
public sealed class Fx7A735Effect : FxA2 { public override string Label => "ハモり 7度上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 10 / 12.0), .55f); }

[AudioEffect("ハモり 増4度", ["演出：変調"], ["ハモり 増4度", "増4度を重ねて落ち着かない響きにする", "変調", "ハモり", "増4度", "ハーモニー", "ピッチ", "重ねる", "不気味", "緊張", "演出"])]
public sealed class Fx7A736Effect : FxA2 { public override string Label => "ハモり 増4度"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 6 / 12.0), .6f); }

[AudioEffect("ハモり 5度上 こもり", ["演出：変調"], ["ハモり 5度上 こもり", "こもった5度上の声を重ねる", "変調", "ハモり", "5度上", "こもり", "ハーモニー", "ピッチ", "重ねる", "やわらか", "演出"])]
public sealed class Fx7A737Effect : FxA2 { public override string Label => "ハモり 5度上 こもり"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 7 / 12.0), .6f, 3000); }

[AudioEffect("ハモり 3度上 こもり", ["演出：変調"], ["ハモり 3度上 こもり", "こもった3度上の声を重ねる", "変調", "ハモり", "3度上", "こもり", "ハーモニー", "ピッチ", "重ねる", "やわらか", "演出"])]
public sealed class Fx7A738Effect : FxA2 { public override string Label => "ハモり 3度上 こもり"; internal override AudioKernel CreateKernel() => new PitchAddK(this, Math.Pow(2, 4 / 12.0), .6f, 3000); }

[AudioEffect("ハモり ほんの少し上", ["演出：変調"], ["ハモり ほんの少し上", "ほんの少しずれた声を重ねて厚くする", "変調", "ハモり", "ほんの少し上", "ハーモニー", "ピッチ", "重ねる", "ダブル", "厚み", "演出"])]
public sealed class Fx7A739Effect : FxA2 { public override string Label => "ハモり ほんの少し上"; internal override AudioKernel CreateKernel() => new PitchAddK(this, 1.012, .8f); }

[AudioEffect("ハモり ほんの少し下", ["演出：変調"], ["ハモり ほんの少し下", "ほんの少し低い声を重ねて厚くする", "変調", "ハモり", "ほんの少し下", "ハーモニー", "ピッチ", "重ねる", "ダブル", "厚み", "演出"])]
public sealed class Fx7A740Effect : FxA2 { public override string Label => "ハモり ほんの少し下"; internal override AudioKernel CreateKernel() => new PitchAddK(this, .988, .8f); }

[AudioEffect("機器 カーナビ", ["演出：音質"], ["機器 カーナビ", "車のカーナビの案内のような声", "音質", "機器", "カーナビ", "デバイス", "電話", "スピーカー", "車", "案内", "演出"])]
public sealed class Fx7A741Effect : FxA3 { public override string Label => "機器 カーナビ"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 400, 7000), new EqK(this, (2500, 1, 4)), new RoomK(this, .12f, .7f, 1, .35f, 150, 6000, .25f)); }

[AudioEffect("機器 子機", ["演出：音質"], ["機器 子機", "家の電話の子機のような声", "音質", "機器", "子機", "デバイス", "電話", "スピーカー", "家", "演出"])]
public sealed class Fx7A742Effect : FxA3 { public override string Label => "機器 子機"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 350, 3400, 2), new ShapeK(this, x => MathF.Tanh(x * 1.3f), 2, 8, 3400, 0)); }

[AudioEffect("機器 タクシー無線", ["演出：音質"], ["機器 タクシー無線", "タクシーの無線のような途切れる声", "音質", "機器", "タクシー無線", "デバイス", "電話", "スピーカー", "無線", "車", "演出"])]
public sealed class Fx7A743Effect : FxA3 { public override string Label => "機器 タクシー無線"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 700, 2600, 4), new ShapeK(this, MathF.Tanh, 8, 20, 2600, 0), new GainLfoK(this, .3, .6, 1)); }

[AudioEffect("機器 ボイスチャット", ["演出：音質"], ["機器 ボイスチャット", "ゲームのボイスチャットのような声", "音質", "機器", "ボイスチャット", "デバイス", "電話", "スピーカー", "ゲーム", "通話", "演出"])]
public sealed class Fx7A744Effect : FxA3 { public override string Label => "機器 ボイスチャット"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 200, 7000, 1), new ShapeK(this, x => MathF.Round(x * 24f) / 24f, 2, 6, 7000, 0)); }

[AudioEffect("機器 ラジカセ", ["演出：音質"], ["機器 ラジカセ", "昔のラジカセのような音", "音質", "機器", "ラジカセ", "デバイス", "電話", "スピーカー", "レトロ", "カセット", "演出"])]
public sealed class Fx7A745Effect : FxA3 { public override string Label => "機器 ラジカセ"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 120, 9000), new TapeK(this, false)); }

[AudioEffect("機器 ボイスレコーダー", ["演出：音質"], ["機器 ボイスレコーダー", "ボイスレコーダーで録ったような音", "音質", "機器", "ボイスレコーダー", "デバイス", "電話", "スピーカー", "録音", "会議", "演出"])]
public sealed class Fx7A746Effect : FxA3 { public override string Label => "機器 ボイスレコーダー"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 250, 6000, 1), new RoomK(this, .35f, .5f, 4, .6f, 200, 6000, .3f)); }

[AudioEffect("機器 テレビ電話", ["演出：音質"], ["機器 テレビ電話", "テレビ電話の向こうの声", "音質", "機器", "テレビ電話", "デバイス", "電話", "スピーカー", "オンライン", "通話", "演出"])]
public sealed class Fx7A747Effect : FxA3 { public override string Label => "機器 テレビ電話"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 200, 8000), new ShapeK(this, x => MathF.Round(x * 40f) / 40f, 1, 4, 8000, 0), new RoomK(this, .3f, .5f, 3, .5f, 200, 8000, .2f)); }

[AudioEffect("機器 ヘッドセット", ["演出：音質"], ["機器 ヘッドセット", "ヘッドセットのマイクの近い声", "音質", "機器", "ヘッドセット", "デバイス", "電話", "スピーカー", "配信", "ゲーム", "演出"])]
public sealed class Fx7A748Effect : FxA3 { public override string Label => "機器 ヘッドセット"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 150, 9000), new EqK(this, (3000, 1, 3))); }

[AudioEffect("機器 携帯ラジオ", ["演出：音質"], ["機器 携帯ラジオ", "小さな携帯ラジオのような音", "音質", "機器", "携帯ラジオ", "デバイス", "電話", "スピーカー", "ラジオ", "小さい", "演出"])]
public sealed class Fx7A749Effect : FxA3 { public override string Label => "機器 携帯ラジオ"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 500, 4500, 3), new ShapeK(this, MathF.Tanh, 4, 12, 4500, 0)); }

[AudioEffect("機器 おもちゃの電話", ["演出：音質"], ["機器 おもちゃの電話", "おもちゃの電話のような細い声", "音質", "機器", "おもちゃの電話", "デバイス", "電話", "スピーカー", "おもちゃ", "子ども", "演出"])]
public sealed class Fx7A750Effect : FxA3 { public override string Label => "機器 おもちゃの電話"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 900, 3500, 5), new ShapeK(this, x => MathF.Sign(x) * MathF.Min(1f, MathF.Abs(x) * 2f), 6, 18, 3500, 0)); }

[AudioEffect("機器 糸電話", ["演出：音質"], ["機器 糸電話", "糸電話で聞くような細く響く声", "音質", "機器", "糸電話", "デバイス", "電話", "スピーカー", "昔", "遊び", "演出"])]
public sealed class Fx7A751Effect : FxA3 { public override string Label => "機器 糸電話"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 800, 2500, 3), new RoomK(this, .08f, .3f, 1, .25f, 600, 3000, .45f)); }

[AudioEffect("機器 ブリキのスピーカー", ["演出：音質"], ["機器 ブリキのスピーカー", "ブリキのスピーカーのような金属っぽい声", "音質", "機器", "ブリキのスピーカー", "デバイス", "電話", "スピーカー", "金属", "レトロ", "演出"])]
public sealed class Fx7A752Effect : FxA3 { public override string Label => "機器 ブリキのスピーカー"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 1000, 4000, 4), new RoomK(this, .05f, .2f, 1, .2f, 800, 5000, .5f)); }

[AudioEffect("機器 防犯カメラのマイク", ["演出：音質"], ["機器 防犯カメラのマイク", "防犯カメラの映像の音声のよう", "音質", "機器", "防犯カメラのマイク", "デバイス", "電話", "スピーカー", "ドキュメント", "記録", "演出"])]
public sealed class Fx7A753Effect : FxA3 { public override string Label => "機器 防犯カメラのマイク"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 300, 4000), new ShapeK(this, x => MathF.Round(x * 16f) / 16f, 3, 10, 4000, 0), new RoomK(this, .5f, .5f, 8, .7f, 200, 4000, .35f)); }

[AudioEffect("機器 ドアホン", ["演出：音質"], ["機器 ドアホン", "玄関のドアホンから聞こえる声", "音質", "機器", "ドアホン", "デバイス", "電話", "スピーカー", "玄関", "訪問", "演出"])]
public sealed class Fx7A754Effect : FxA3 { public override string Label => "機器 ドアホン"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 500, 3800, 3), new RoomK(this, .1f, .4f, 1, .3f, 400, 4000, .35f)); }

[AudioEffect("機器 骨伝導", ["演出：音質"], ["機器 骨伝導", "骨を通して聞くようなこもった音", "音質", "機器", "骨伝導", "デバイス", "電話", "スピーカー", "こもる", "不思議", "演出"])]
public sealed class Fx7A755Effect : FxA3 { public override string Label => "機器 骨伝導"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (300, .7, 6), (3000, .7, -10))); }

[AudioEffect("機器 ぬいぐるみのスピーカー", ["演出：音質"], ["機器 ぬいぐるみのスピーカー", "ぬいぐるみの中から聞こえるような声", "音質", "機器", "ぬいぐるみのスピーカー", "デバイス", "電話", "スピーカー", "かわいい", "こもる", "演出"])]
public sealed class Fx7A756Effect : FxA3 { public override string Label => "機器 ぬいぐるみのスピーカー"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 400, 3000), new RoomK(this, .1f, .9f, 1, .3f, 300, 2500, .4f)); }

[AudioEffect("機器 古いパソコン", ["演出：音質"], ["機器 古いパソコン", "古いパソコンの音声のような粗い声", "音質", "機器", "古いパソコン", "デバイス", "電話", "スピーカー", "レトロ", "合成", "演出"])]
public sealed class Fx7A757Effect : FxA3 { public override string Label => "機器 古いパソコン"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 300, 5000), new ShapeK(this, x => MathF.Round(x * 8f) / 8f, 4, 14, 5000, 0)); }

[AudioEffect("機器 ロボット掃除機の声", ["演出：音質"], ["機器 ロボット掃除機の声", "家電がしゃべるような声", "音質", "機器", "ロボット掃除機の声", "デバイス", "電話", "スピーカー", "家電", "案内", "演出"])]
public sealed class Fx7A758Effect : FxA3 { public override string Label => "機器 ロボット掃除機の声"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 600, 6000, 2), new EqK(this, (3000, 1, 5)), new ShapeK(this, x => MathF.Round(x * 20f) / 20f, 2, 6, 6000, 0)); }

[AudioEffect("機器 ラジオの交通情報", ["演出：音質"], ["機器 ラジオの交通情報", "ラジオの交通情報のような声", "音質", "機器", "ラジオの交通情報", "デバイス", "電話", "スピーカー", "ラジオ", "ニュース", "演出"])]
public sealed class Fx7A759Effect : FxA3 { public override string Label => "機器 ラジオの交通情報"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 300, 5500, 2), new EqK(this, (2500, 1, 3)), new ShapeK(this, MathF.Tanh, 3, 9, 5500, 0)); }

[AudioEffect("機器 屋外スピーカー", ["演出：音質"], ["機器 屋外スピーカー", "屋外のスピーカーで流れる放送のような声", "音質", "機器", "屋外スピーカー", "デバイス", "電話", "スピーカー", "公園", "放送", "演出"])]
public sealed class Fx7A760Effect : FxA3 { public override string Label => "機器 屋外スピーカー"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 400, 5000, 3), new EchoK(this, .35, .4, .4f, 400, 4000, .45f)); }

[AudioEffect("空間 ドーム球場", ["演出：場所の声"], ["空間 ドーム球場", "大きなドーム球場の響き", "場所の声", "空間", "ドーム球場", "リバーブ", "響き", "場所", "スタジアム", "広い", "演出"])]
public sealed class Fx7A761Effect : FxA3 { public override string Label => "空間 ドーム球場"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .95f, .4f, 40, 1f, 120, 6000, .5f), new EchoK(this, .45, .5, .35f, 300, 3000, .35f)); }

[AudioEffect("空間 地下神殿", ["演出：場所の声"], ["空間 地下神殿", "石の地下神殿の深い響き", "場所の声", "空間", "地下神殿", "リバーブ", "響き", "場所", "ダンジョン", "神秘", "演出"])]
public sealed class Fx7A762Effect : FxA3 { public override string Label => "空間 地下神殿"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .95f, .25f, 35, 1f, 100, 5000, .6f), new EchoK(this, .3, .36, .5f, 200, 3000, .4f, true)); }

[AudioEffect("空間 古い洋館", ["演出：場所の声"], ["空間 古い洋館", "古い洋館の部屋の響き", "場所の声", "空間", "古い洋館", "リバーブ", "響き", "場所", "ホラー", "屋敷", "演出"])]
public sealed class Fx7A763Effect : FxA3 { public override string Label => "空間 古い洋館"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .6f, .5f, 10, .8f, 150, 5000, .45f), new EchoK(this, .18, .22, .3f, 200, 4000, .25f)); }

[AudioEffect("空間 竹林", ["演出：場所の声"], ["空間 竹林", "竹林の中のかすかな響き", "場所の声", "空間", "竹林", "リバーブ", "響き", "場所", "和", "静か", "演出"])]
public sealed class Fx7A764Effect : FxA3 { public override string Label => "空間 竹林"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .5f, .7f, 10, .7f, 300, 7000, .3f), new EchoK(this, .12, .16, .3f, 500, 6000, .25f)); }

[AudioEffect("空間 雪原", ["演出：場所の声"], ["空間 雪原", "雪に吸われてほとんど響かない音", "場所の声", "空間", "雪原", "リバーブ", "響き", "場所", "雪", "静か", "演出"])]
public sealed class Fx7A765Effect : FxA3 { public override string Label => "空間 雪原"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .2f, .95f, 2, .4f, 200, 4000, .15f)); }

[AudioEffect("空間 港の倉庫", ["演出：場所の声"], ["空間 港の倉庫", "港の大きな倉庫の響き", "場所の声", "空間", "港の倉庫", "リバーブ", "響き", "場所", "海", "倉庫", "演出"])]
public sealed class Fx7A766Effect : FxA3 { public override string Label => "空間 港の倉庫"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .8f, .3f, 18, .95f, 150, 6000, .5f), new EchoK(this, .25, .3, .3f, 200, 4000, .3f)); }

[AudioEffect("空間 夜の学校", ["演出：場所の声"], ["空間 夜の学校", "誰もいない夜の学校の響き", "場所の声", "空間", "夜の学校", "リバーブ", "響き", "場所", "ホラー", "学校", "演出"])]
public sealed class Fx7A767Effect : FxA3 { public override string Label => "空間 夜の学校"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .6f, .35f, 10, .85f, 180, 6000, .5f), new EchoK(this, .3, .35, .35f, 200, 4000, .3f)); }

[AudioEffect("空間 深い井戸", ["演出：場所の声"], ["空間 深い井戸", "深い井戸の中の響き", "場所の声", "空間", "深い井戸", "リバーブ", "響き", "場所", "井戸", "こもる", "演出"])]
public sealed class Fx7A768Effect : FxA3 { public override string Label => "空間 深い井戸"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .3f, .2f, 5, .6f, 200, 3000, .55f), new EchoK(this, .09, .1, .7f, 200, 2500, .5f)); }

[AudioEffect("空間 鍾乳洞", ["演出：場所の声"], ["空間 鍾乳洞", "しずくの落ちる鍾乳洞の響き", "場所の声", "空間", "鍾乳洞", "リバーブ", "響き", "場所", "洞窟", "神秘", "演出"])]
public sealed class Fx7A769Effect : FxA3 { public override string Label => "空間 鍾乳洞"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .95f, .35f, 30, 1f, 150, 6000, .6f), new EchoK(this, .2, .26, .45f, 300, 5000, .35f, true)); }

[AudioEffect("空間 屋上", ["演出：場所の声"], ["空間 屋上", "ビルの屋上の開けた音", "場所の声", "空間", "屋上", "リバーブ", "響き", "場所", "外", "風", "演出"])]
public sealed class Fx7A770Effect : FxA3 { public override string Label => "空間 屋上"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .15f, .8f, 2, .3f, 200, 9000, .2f), new EchoK(this, .4, .45, .25f, 400, 5000, .25f)); }

[AudioEffect("空間 霧の森", ["演出：場所の声"], ["空間 霧の森", "霧の立ちこめた森のぼんやりした響き", "場所の声", "空間", "霧の森", "リバーブ", "響き", "場所", "幻想", "森", "演出"])]
public sealed class Fx7A771Effect : FxA3 { public override string Label => "空間 霧の森"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .7f, .8f, 20, .9f, 250, 3500, .4f)); }

[AudioEffect("空間 屋内プール", ["演出：場所の声"], ["空間 屋内プール", "屋内プールのよく響く音", "場所の声", "空間", "屋内プール", "リバーブ", "響き", "場所", "プール", "水", "演出"])]
public sealed class Fx7A772Effect : FxA3 { public override string Label => "空間 屋内プール"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .7f, .2f, 12, .9f, 200, 8000, .6f), new EchoK(this, .15, .2, .3f, 300, 6000, .25f)); }

[AudioEffect("空間 水そうの前", ["演出：場所の声"], ["空間 水そうの前", "水族館の大きな水そうの前の響き", "場所の声", "空間", "水そうの前", "リバーブ", "響き", "場所", "水族館", "青", "演出"])]
public sealed class Fx7A773Effect : FxA3 { public override string Label => "空間 水そうの前"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .6f, .6f, 10, .8f, 150, 4500, .4f), new ModDelayK(this, 10, 4, .2, .5, .1f, .6f)); }

[AudioEffect("空間 滝の裏", ["演出：場所の声"], ["空間 滝の裏", "滝の裏の洞くつの響き", "場所の声", "空間", "滝の裏", "リバーブ", "響き", "場所", "滝", "冒険", "演出"])]
public sealed class Fx7A774Effect : FxA3 { public override string Label => "空間 滝の裏"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .5f, .5f, 8, .7f, 300, 4000, .55f), new EqK(this, (6000, .7, -6))); }

[AudioEffect("空間 鉄橋の下", ["演出：場所の声"], ["空間 鉄橋の下", "鉄橋の下の金属っぽい響き", "場所の声", "空間", "鉄橋の下", "リバーブ", "響き", "場所", "橋", "金属", "演出"])]
public sealed class Fx7A775Effect : FxA3 { public override string Label => "空間 鉄橋の下"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .55f, .25f, 8, .8f, 200, 7000, .5f), new EchoK(this, .1, .12, .5f, 300, 6000, .35f)); }

[AudioEffect("空間 茶室", ["演出：場所の声"], ["空間 茶室", "小さな茶室の落ち着いた響き", "場所の声", "空間", "茶室", "リバーブ", "響き", "場所", "和", "茶道", "演出"])]
public sealed class Fx7A776Effect : FxA3 { public override string Label => "空間 茶室"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .15f, .75f, 1, .3f, 180, 7000, .25f)); }

[AudioEffect("空間 カラオケボックス", ["演出：場所の声"], ["空間 カラオケボックス", "カラオケボックスのエコーのかかった声", "場所の声", "空間", "カラオケボックス", "リバーブ", "響き", "場所", "カラオケ", "歌", "演出"])]
public sealed class Fx7A777Effect : FxA3 { public override string Label => "空間 カラオケボックス"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .3f, .55f, 4, .55f, 120, 9000, .4f), new EchoK(this, .18, .22, .35f, 200, 7000, .3f)); }

[AudioEffect("空間 野外ライブ", ["演出：場所の声"], ["空間 野外ライブ", "野外ライブ会場のような開けた響き", "場所の声", "空間", "野外ライブ", "リバーブ", "響き", "場所", "フェス", "外", "演出"])]
public sealed class Fx7A778Effect : FxA3 { public override string Label => "空間 野外ライブ"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .4f, .6f, 10, .6f, 100, 9000, .25f), new EchoK(this, .5, .55, .3f, 300, 4000, .3f)); }

[AudioEffect("空間 宇宙ステーション", ["演出：場所の声"], ["空間 宇宙ステーション", "宇宙ステーションの中の響き", "場所の声", "空間", "宇宙ステーション", "リバーブ", "響き", "場所", "SF", "宇宙", "演出"])]
public sealed class Fx7A779Effect : FxA3 { public override string Label => "空間 宇宙ステーション"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .3f, .3f, 3, .5f, 300, 8000, .45f), new BandK(this, 200, 9000)); }

[AudioEffect("空間 トンネルのまん中", ["演出：場所の声"], ["空間 トンネルのまん中", "長いトンネルのまん中の響き", "場所の声", "空間", "トンネルのまん中", "リバーブ", "響き", "場所", "トンネル", "車", "演出"])]
public sealed class Fx7A780Effect : FxA3 { public override string Label => "空間 トンネルのまん中"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .7f, .25f, 15, .9f, 150, 5000, .6f), new EchoK(this, .22, .26, .45f, 200, 3500, .35f)); }

[AudioEffect("場面 決めゼリフ", ["演出：場面の音"], ["場面 決めゼリフ", "かっこいい決めゼリフのような響き", "場面の音", "場面", "決めゼリフ", "シーン", "効果音", "組み合わせ", "かっこいい", "必殺技", "演出"])]
public sealed class Fx7A781Effect : FxA3 { public override string Label => "場面 決めゼリフ"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (150, .7, 3), (3500, 1, 4)), new RoomK(this, .6f, .4f, 15, .85f, 150, 8000, .35f), new EchoK(this, .3, .35, .3f, 300, 6000, .25f)); }

[AudioEffect("場面 心の声", ["演出：場面の音"], ["場面 心の声", "心の中の声のようにやわらかく響く", "場面の音", "場面", "心の声", "シーン", "効果音", "組み合わせ", "モノローグ", "内心", "演出"])]
public sealed class Fx7A782Effect : FxA3 { public override string Label => "場面 心の声"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (3000, .7, -4), (200, .7, 2)), new RoomK(this, .4f, .7f, 8, .6f, 200, 5000, .45f)); }

[AudioEffect("場面 回想シーン", ["演出：場面の音"], ["場面 回想シーン", "昔を思い出すような遠い響き", "場面の音", "場面", "回想シーン", "シーン", "効果音", "組み合わせ", "思い出", "過去", "演出"])]
public sealed class Fx7A783Effect : FxA3 { public override string Label => "場面 回想シーン"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 200, 6000), new RoomK(this, .7f, .5f, 20, .9f, 200, 5000, .5f), new ModDelayK(this, 15, 5, .2, .5, .1f, .6f)); }

[AudioEffect("場面 ラスボス登場", ["演出：場面の音"], ["場面 ラスボス登場", "ラスボスが登場したような重い声", "場面の音", "場面", "ラスボス登場", "シーン", "効果音", "組み合わせ", "ボス", "迫力", "演出"])]
public sealed class Fx7A784Effect : FxA3 { public override string Label => "場面 ラスボス登場"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, -7, 50, 5000, .4f, .3f, .9, 250, 6), new EchoK(this, .4, .45, .45f, 150, 3000, .4f), new EqK(this, (60, .7, 5))); }

[AudioEffect("場面 秘密の通信", ["演出：場面の音"], ["場面 秘密の通信", "秘密の暗号通信のような声", "場面の音", "場面", "秘密の通信", "シーン", "効果音", "組み合わせ", "スパイ", "暗号", "演出"])]
public sealed class Fx7A785Effect : FxA3 { public override string Label => "場面 秘密の通信"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 800, 3000, 3), new ShapeK(this, x => MathF.Round(x * 12f) / 12f, 3, 10, 3000, 0), new GainLfoK(this, 6, 12, 1)); }

[AudioEffect("場面 テレビの中の人", ["演出：場面の音"], ["場面 テレビの中の人", "テレビの中の人が話しているような声", "場面の音", "場面", "テレビの中の人", "シーン", "効果音", "組み合わせ", "テレビ", "部屋", "演出"])]
public sealed class Fx7A786Effect : FxA3 { public override string Label => "場面 テレビの中の人"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 250, 7000, 1), new RoomK(this, .5f, .5f, 8, .7f, 200, 6000, .3f), new RoomK(this, .35f, .6f, 5, .6f, 150, 5000, .3f)); }

[AudioEffect("場面 迷子のお知らせ", ["演出：場面の音"], ["場面 迷子のお知らせ", "お店の迷子のお知らせのような放送", "場面の音", "場面", "迷子のお知らせ", "シーン", "効果音", "組み合わせ", "放送", "お店", "演出"])]
public sealed class Fx7A787Effect : FxA3 { public override string Label => "場面 迷子のお知らせ"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 400, 5000, 2), new RoomK(this, .85f, .4f, 22, .95f, 200, 5000, .45f), new EchoK(this, .35, .4, .3f, 400, 4000, .3f)); }

[AudioEffect("場面 宇宙からの声", ["演出：場面の音"], ["場面 宇宙からの声", "宇宙のかなたから届くような声", "場面の音", "場面", "宇宙からの声", "シーン", "効果音", "組み合わせ", "SF", "宇宙", "演出"])]
public sealed class Fx7A788Effect : FxA3 { public override string Label => "場面 宇宙からの声"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, 0, 400, 6000, .2f, .3f, .7, 2000, 5), new ModDelayK(this, 3, 2, .5, 1, .6f, .8f), new EchoK(this, .5, .6, .5f, 500, 4000, .4f, true)); }

[AudioEffect("場面 時をこえる声", ["演出：場面の音"], ["場面 時をこえる声", "時をこえて届くような不思議な声", "場面の音", "場面", "時をこえる声", "シーン", "効果音", "組み合わせ", "タイムスリップ", "不思議", "演出"])]
public sealed class Fx7A789Effect : FxA3 { public override string Label => "場面 時をこえる声"; internal override AudioKernel CreateKernel() => new ChainK(new FilterLfoK(this, 600, 7000, 0, .9, .05, .2), new EchoK(this, .4, .5, .55f, 300, 5000, .45f, true), new RoomK(this, .8f, .5f, 25, 1f, 200, 6000, .4f)); }

[AudioEffect("場面 夢の終わり", ["演出：場面の音"], ["場面 夢の終わり", "夢から覚めるようにぼやけていく音", "場面の音", "場面", "夢の終わり", "シーン", "効果音", "組み合わせ", "夢", "目覚め", "演出"])]
public sealed class Fx7A790Effect : FxA3 { public override string Label => "場面 夢の終わり"; internal override AudioKernel CreateKernel() => new ChainK(new RoomK(this, .9f, .5f, 30, 1f, 200, 5000, .6f), new FilterLfoK(this, 300, 5000, 0, .9, .03, .1)); }

[AudioEffect("場面 嵐の中", ["演出：場面の音"], ["場面 嵐の中", "嵐の中で叫んでいるような声", "場面の音", "場面", "嵐の中", "シーン", "効果音", "組み合わせ", "風", "雨", "演出"])]
public sealed class Fx7A791Effect : FxA3 { public override string Label => "場面 嵐の中"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 200, 5000), new GainLfoK(this, 1, 3, 0), new FilterLfoK(this, 800, 5000, 0, 1, .2, .6)); }

[AudioEffect("場面 水の中で叫ぶ", ["演出：場面の音"], ["場面 水の中で叫ぶ", "水の中で叫んでいるようなこもった声", "場面の音", "場面", "水の中で叫ぶ", "シーン", "効果音", "組み合わせ", "水中", "ピンチ", "演出"])]
public sealed class Fx7A792Effect : FxA3 { public override string Label => "場面 水の中で叫ぶ"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 100, 700), new ModDelayK(this, 12, 8, .3, .8, .4f, .9f), new RoomK(this, .5f, .7f, 5, .6f, 100, 800, .5f)); }

[AudioEffect("場面 マイクテスト", ["演出：場面の音"], ["場面 マイクテスト", "体育館でマイクテストをするような声", "場面の音", "場面", "マイクテスト", "シーン", "効果音", "組み合わせ", "学校", "テスト", "演出"])]
public sealed class Fx7A793Effect : FxA3 { public override string Label => "場面 マイクテスト"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 300, 6000, 2), new RoomK(this, .8f, .35f, 20, .95f, 200, 6000, .5f), new EchoK(this, .3, .32, .45f, 300, 5000, .4f)); }

[AudioEffect("場面 学園祭のステージ", ["演出：場面の音"], ["場面 学園祭のステージ", "学園祭のステージで話すような声", "場面の音", "場面", "学園祭のステージ", "シーン", "効果音", "組み合わせ", "学校", "イベント", "演出"])]
public sealed class Fx7A794Effect : FxA3 { public override string Label => "場面 学園祭のステージ"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (3000, 1, 3)), new RoomK(this, .75f, .4f, 18, .9f, 150, 7000, .45f), new EchoK(this, .25, .3, .25f, 200, 6000, .25f)); }

[AudioEffect("場面 怪談の語り", ["演出：場面の音"], ["場面 怪談の語り", "怪談を語るような低く不気味な声", "場面の音", "場面", "怪談の語り", "シーン", "効果音", "組み合わせ", "ホラー", "夏", "演出"])]
public sealed class Fx7A795Effect : FxA3 { public override string Label => "場面 怪談の語り"; internal override AudioKernel CreateKernel() => new ChainK(new VoiceK(this, -1, 150, 6000, 0, .2f, .7, 800, 3), new RoomK(this, .45f, .6f, 6, .65f, 150, 4000, .4f), new ModDelayK(this, 16, 4, .15, .4, .1f, .5f)); }

[AudioEffect("場面 ヒーロー登場", ["演出：場面の音"], ["場面 ヒーロー登場", "ヒーローが現れたような力強い声", "場面の音", "場面", "ヒーロー登場", "シーン", "効果音", "組み合わせ", "ヒーロー", "正義", "演出"])]
public sealed class Fx7A796Effect : FxA3 { public override string Label => "場面 ヒーロー登場"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (120, .7, 4), (4000, 1, 4)), new EchoK(this, .25, .3, .35f, 200, 7000, .35f, true), new RoomK(this, .7f, .4f, 15, .9f, 120, 8000, .35f)); }

[AudioEffect("場面 機械の故障", ["演出：場面の音"], ["場面 機械の故障", "故障した機械のような途切れる声", "場面の音", "場面", "機械の故障", "シーン", "効果音", "組み合わせ", "故障", "エラー", "演出"])]
public sealed class Fx7A797Effect : FxA3 { public override string Label => "場面 機械の故障"; internal override AudioKernel CreateKernel() => new ChainK(new ShapeK(this, x => MathF.Round(x * 6f) / 6f, 4, 14, 5000, 0), new GainLfoK(this, 8, 20, 1), new BandK(this, 400, 5000)); }

[AudioEffect("場面 遠くからの呼びかけ", ["演出：場面の音"], ["場面 遠くからの呼びかけ", "遠くから呼びかけられるような声", "場面の音", "場面", "遠くからの呼びかけ", "シーン", "効果音", "組み合わせ", "遠い", "山", "演出"])]
public sealed class Fx7A798Effect : FxA3 { public override string Label => "場面 遠くからの呼びかけ"; internal override AudioKernel CreateKernel() => new ChainK(new BandK(this, 400, 3000), new RoomK(this, .9f, .7f, 40, 1f, 300, 3000, .7f), new EchoK(this, .5, .6, .4f, 400, 2500, .4f)); }

[AudioEffect("場面 内緒話", ["演出：場面の音"], ["場面 内緒話", "耳元で内緒話をするような声", "場面の音", "場面", "内緒話", "シーン", "効果音", "組み合わせ", "ひそひそ", "秘密", "演出"])]
public sealed class Fx7A799Effect : FxA3 { public override string Label => "場面 内緒話"; internal override AudioKernel CreateKernel() => new ChainK(new EqK(this, (200, .7, -6), (5000, .7, 4)), new RoomK(this, .15f, .8f, 1, .3f, 300, 8000, .2f)); }

[AudioEffect("場面 鏡の中の声", ["演出：場面の音"], ["場面 鏡の中の声", "鏡の向こうから聞こえるような不思議な声", "場面の音", "場面", "鏡の中の声", "シーン", "効果音", "組み合わせ", "不思議", "ファンタジー", "演出"])]
public sealed class Fx7A800Effect : FxA3 { public override string Label => "場面 鏡の中の声"; internal override AudioKernel CreateKernel() => new ChainK(new PitchAddK(this, 1.01, .7f), new RoomK(this, .4f, .2f, 5, .6f, 300, 9000, .5f), new ModDelayK(this, 8, 3, .3, .6, .2f, .6f)); }
