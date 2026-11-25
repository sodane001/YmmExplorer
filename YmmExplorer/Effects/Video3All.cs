using System.Numerics;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 101〜200(tools で生成)

[VideoEffect("冬の朝", ["演出：色調整"], ["冬の朝", "青白く澄んだ冷たい空気の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V101Effect : FxGrade { public override string Label => "冬の朝"; private protected override float[] M => Gd.G(.82f,1.04f,.03f,.9f,.98f,1.14f); }

[VideoEffect("真夏の日差し", ["演出：色調整"], ["真夏の日差し", "まぶしく黄色い強い日差しの色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V102Effect : FxGrade { public override string Label => "真夏の日差し"; private protected override float[] M => Gd.G(1.25f,1.12f,.05f,1.12f,1.05f,.84f); }

[VideoEffect("秋の夕暮れ", ["演出：色調整"], ["秋の夕暮れ", "赤茶色に沈んでいく秋の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V103Effect : FxGrade { public override string Label => "秋の夕暮れ"; private protected override float[] M => Gd.G(.9f,1.06f,-.03f,1.14f,.92f,.72f); }

[VideoEffect("春の陽気", ["演出：色調整"], ["春の陽気", "やわらかいピンクがかった春の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V104Effect : FxGrade { public override string Label => "春の陽気"; private protected override float[] M => Gd.G(1.05f,.92f,.06f,1.08f,1f,1f); }

[VideoEffect("雨の日", ["演出：色調整"], ["雨の日", "色が沈んだ灰青色の雨の日", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V105Effect : FxGrade { public override string Label => "雨の日"; private protected override float[] M => Gd.G(.55f,.92f,-.04f,.92f,.98f,1.08f); }

[VideoEffect("曇り空", ["演出：色調整"], ["曇り空", "白っぽく平坦な曇りの色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V106Effect : FxGrade { public override string Label => "曇り空"; private protected override float[] M => Gd.G(.62f,.8f,.05f,.98f,1f,1.03f); }

[VideoEffect("深夜", ["演出：色調整"], ["深夜", "暗く青い真夜中の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V107Effect : FxGrade { public override string Label => "深夜"; private protected override float[] M => Gd.G(.45f,.95f,-.2f,.62f,.72f,1.05f); }

[VideoEffect("月明かり", ["演出：色調整"], ["月明かり", "青銀色に照らされた月夜の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V108Effect : FxGrade { public override string Label => "月明かり"; private protected override float[] M => Gd.G(.28f,1.1f,-.1f,.78f,.9f,1.18f); }

[VideoEffect("朝焼け", ["演出：色調整"], ["朝焼け", "紫からオレンジへ染まる朝の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V109Effect : FxGrade { public override string Label => "朝焼け"; private protected override float[] M => Gd.G(1.1f,1f,.02f,1.14f,.9f,.96f); }

[VideoEffect("黄昏", ["演出：色調整"], ["黄昏", "金色に暮れていく色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V110Effect : FxGrade { public override string Label => "黄昏"; private protected override float[] M => Gd.G(.85f,1.05f,-.02f,1.18f,.98f,.68f); }

[VideoEffect("ネオン街", ["演出：色調整"], ["ネオン街", "赤紫と青が強い夜の街の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V111Effect : FxGrade { public override string Label => "ネオン街"; private protected override float[] M => Gd.G(1.55f,1.2f,-.06f,1.12f,.72f,1.2f); }

[VideoEffect("サイバーパンク", ["演出：色調整"], ["サイバーパンク", "シアンとマゼンタに振った近未来の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V112Effect : FxGrade { public override string Label => "サイバーパンク"; private protected override float[] M => Mx.Out(1.1f,-.05f,.25f,.02f,-.05f,.82f,.25f,0f,.28f,.1f,1.1f,.05f); }

[VideoEffect("ヴェイパーウェーブ", ["演出：色調整"], ["ヴェイパーウェーブ", "ピンクと水色の懐かしいネット文化の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V113Effect : FxGrade { public override string Label => "ヴェイパーウェーブ"; private protected override float[] M => Gd.D(0x3A1C71,0xFFAFCC); }

[VideoEffect("パステル", ["演出：色調整"], ["パステル", "淡くやさしい色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V114Effect : FxGrade { public override string Label => "パステル"; private protected override float[] M => Gd.G(.62f,.72f,.17f,1.02f,1f,1.03f); }

[VideoEffect("くすみカラー", ["演出：色調整"], ["くすみカラー", "彩度を落とした落ち着いた色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V115Effect : FxGrade { public override string Label => "くすみカラー"; private protected override float[] M => Gd.G(.58f,.9f,.02f,1.02f,1f,.96f); }

[VideoEffect("ハイキー", ["演出：色調整"], ["ハイキー", "全体を明るく軽やかにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V116Effect : FxGrade { public override string Label => "ハイキー"; private protected override float[] M => Gd.G(.92f,.82f,.2f); }

[VideoEffect("ローキー", ["演出：色調整"], ["ローキー", "暗い部分を増やして重厚にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V117Effect : FxGrade { public override string Label => "ローキー"; private protected override float[] M => Gd.G(.9f,1.28f,-.17f); }

[VideoEffect("高コントラスト", ["演出：色調整"], ["高コントラスト", "明暗をはっきり分ける", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V118Effect : FxGrade { public override string Label => "高コントラスト"; private protected override float[] M => Gd.G(1.1f,1.6f,0f); }

[VideoEffect("低コントラスト", ["演出：色調整"], ["低コントラスト", "明暗の差を小さく、ふんわりさせる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V119Effect : FxGrade { public override string Label => "低コントラスト"; private protected override float[] M => Gd.G(.92f,.6f,.02f); }

[VideoEffect("ブリーチバイパス", ["演出：色調整"], ["ブリーチバイパス", "銀残しのような渋く硬い映画の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V120Effect : FxGrade { public override string Label => "ブリーチバイパス"; private protected override float[] M => Gd.G(.42f,1.42f,-.02f); }

[VideoEffect("赤外線写真風", ["演出：色調整"], ["赤外線写真風", "緑が白く、空が暗くなる不思議な写真", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V121Effect : FxGrade { public override string Label => "赤外線写真風"; private protected override float[] M => Mx.Out(-.2f,1.3f,-.1f,0f,-.2f,1.3f,-.1f,0f,.1f,.2f,.5f,.02f); }

[VideoEffect("青写真", ["演出：色調整"], ["青写真", "青地に白線の設計図のような色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V122Effect : FxGrade { public override string Label => "青写真"; private protected override float[] M => Gd.D(0x0B3C8C,0xE8F4FF); }

[VideoEffect("緑のモニター", ["演出：色調整"], ["緑のモニター", "昔のパソコン画面の緑一色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V123Effect : FxGrade { public override string Label => "緑のモニター"; private protected override float[] M => Gd.D(0x001A08,0x66FF88); }

[VideoEffect("琥珀のモニター", ["演出：色調整"], ["琥珀のモニター", "昔の端末のオレンジ一色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V124Effect : FxGrade { public override string Label => "琥珀のモニター"; private protected override float[] M => Gd.D(0x1A0A00,0xFFB84D); }

[VideoEffect("赤い暗視スコープ", ["演出：色調整"], ["赤い暗視スコープ", "暗い赤一色の視界", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V125Effect : FxGrade { public override string Label => "赤い暗視スコープ"; private protected override float[] M => Gd.D(0x120000,0xFF3B30); }

[VideoEffect("血の色", ["演出：色調整"], ["血の色", "赤黒く不穏な色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V126Effect : FxGrade { public override string Label => "血の色"; private protected override float[] M => Gd.D(0x140305,0xE0202F); }

[VideoEffect("毒々しい緑", ["演出：色調整"], ["毒々しい緑", "怪しく濁った緑", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V127Effect : FxGrade { public override string Label => "毒々しい緑"; private protected override float[] M => Gd.D(0x10200A,0xC4FF3D); }

[VideoEffect("金色に染める", ["演出：色調整"], ["金色に染める", "全体を金の像のように", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V128Effect : FxGrade { public override string Label => "金色に染める"; private protected override float[] M => Gd.D(0x3A2500,0xFFE68A); }

[VideoEffect("銀色に染める", ["演出：色調整"], ["銀色に染める", "全体を銀の像のように", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V129Effect : FxGrade { public override string Label => "銀色に染める"; private protected override float[] M => Gd.D(0x1E2228,0xF2F6FA); }

[VideoEffect("銅色に染める", ["演出：色調整"], ["銅色に染める", "全体を赤銅色に", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V130Effect : FxGrade { public override string Label => "銅色に染める"; private protected override float[] M => Gd.D(0x2A1208,0xF3A777); }

[VideoEffect("紫のもや", ["演出：色調整"], ["紫のもや", "紫がかった幻想的な色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V131Effect : FxGrade { public override string Label => "紫のもや"; private protected override float[] M => Gd.G(.7f,.88f,.04f,1.02f,.82f,1.2f); }

[VideoEffect("海の底", ["演出：色調整"], ["海の底", "青緑に沈んだ深海の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V132Effect : FxGrade { public override string Label => "海の底"; private protected override float[] M => Gd.G(.62f,.92f,-.1f,.45f,.92f,1.12f); }

[VideoEffect("森の緑", ["演出：色調整"], ["森の緑", "緑を強めた木陰の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V133Effect : FxGrade { public override string Label => "森の緑"; private protected override float[] M => Gd.G(.95f,1.03f,-.03f,.84f,1.12f,.8f); }

[VideoEffect("砂漠の黄", ["演出：色調整"], ["砂漠の黄", "乾いた黄土色の色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V134Effect : FxGrade { public override string Label => "砂漠の黄"; private protected override float[] M => Gd.G(.7f,1.05f,.03f,1.12f,1f,.68f); }

[VideoEffect("白黒はっきり", ["演出：色調整"], ["白黒はっきり", "灰色の少ない硬い白黒", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V135Effect : FxGrade { public override string Label => "白黒はっきり"; private protected override float[] M => Gd.G(0f,2.2f,0f); }

[VideoEffect("左右に揺れる", ["演出：動き"], ["左右に揺れる", "左右へなめらかに行き来する", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V136Effect : FxMove { public override string Label => "左右に揺れる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,S(t*2.2f)*.07f*a,0); }

[VideoEffect("上下に揺れる", ["演出：動き"], ["上下に揺れる", "上下へなめらかに行き来する", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V137Effect : FxMove { public override string Label => "上下に揺れる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,S(t*2.2f)*.07f*a); }

[VideoEffect("円を描く", ["演出：動き"], ["円を描く", "小さな円を描いて回る", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V138Effect : FxMove { public override string Label => "円を描く"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,C(t*2f)*.06f*a,S(t*2f)*.08f*a); }

[VideoEffect("8の字に動く", ["演出：動き"], ["8の字に動く", "横向きの8の字を描く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V139Effect : FxMove { public override string Label => "8の字に動く"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,S(t*1.6f)*.08f*a,S(t*3.2f)*.05f*a); }

[VideoEffect("ジグザグに動く", ["演出：動き"], ["ジグザグに動く", "折れ線のように左右へ動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V140Effect : FxMove { public override string Label => "ジグザグに動く"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,MathF.Asin(S(t*3f))*.05f*a,MathF.Asin(S(t*6f))*.015f*a); }

[VideoEffect("首をかしげる", ["演出：動き"], ["首をかしげる", "ゆっくり左右に傾く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V141Effect : FxMove { public override string Label => "首をかしげる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => RoF(f,S(t*1.2f)*.14f*a); }

[VideoEffect("おじぎ", ["演出：動き"], ["おじぎ", "前に沈むように縮んで戻る", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V142Effect : FxMove { public override string Label => "おじぎ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f,1f-MathF.Pow(MathF.Max(0f,S(t*1.1f)),3f)*.16f*a); }

[VideoEffect("うなずく", ["演出：動き"], ["うなずく", "小さく縦に2回ずつ動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V143Effect : FxMove { public override string Label => "うなずく"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,MathF.Max(0f,S(t*6f))*MathF.Max(0f,S(t*1.5f))*.05f*a); }

[VideoEffect("首を振る", ["演出：動き"], ["首を振る", "いやいやと小刻みに左右へ", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V144Effect : FxMove { public override string Label => "首を振る"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,S(t*11f)*MathF.Max(0f,S(t*1.1f))*.045f*a,0); }

[VideoEffect("小刻みに震える", ["演出：動き"], ["小刻みに震える", "寒さや恐怖の細かい震え", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V145Effect : FxMove { public override string Label => "小刻みに震える"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,S(t*61f)*.008f*a,C(t*53f)*.006f*a); }

[VideoEffect("ドキドキ拡大", ["演出：動き"], ["ドキドキ拡大", "鼓動のように2回ずつ大きくなる", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V146Effect : FxMove { public override string Label => "ドキドキ拡大"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Pow(MathF.Max(0f,S(t*5f)),12f)+MathF.Pow(MathF.Max(0f,S(t*5f-.8f)),12f)*.6f)*.1f*a,1f+(MathF.Pow(MathF.Max(0f,S(t*5f)),12f)+MathF.Pow(MathF.Max(0f,S(t*5f-.8f)),12f)*.6f)*.1f*a); }

[VideoEffect("波に浮かぶ", ["演出：動き"], ["波に浮かぶ", "傾きながら波に揺られる", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V147Effect : FxMove { public override string Label => "波に浮かぶ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,S(t*1.2f)*.06f*a)*Mo(f,0,S(t*1.7f)*.035f*a); }

[VideoEffect("風になびく", ["演出：動き"], ["風になびく", "足もとを固定して斜めになびく", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V148Effect : FxMove { public override string Label => "風になびく"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(S(t*1.9f)+S(t*4.3f)*.3f)*.12f*a); }

[VideoEffect("横に伸び縮み", ["演出：動き"], ["横に伸び縮み", "横幅だけが伸び縮みする", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V149Effect : FxMove { public override string Label => "横に伸び縮み"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+S(t*3f)*.14f*a,1f); }

[VideoEffect("縦に伸び縮み", ["演出：動き"], ["縦に伸び縮み", "高さだけが伸び縮みする", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V150Effect : FxMove { public override string Label => "縦に伸び縮み"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f,1f+S(t*3f)*.14f*a); }

[VideoEffect("ゆらゆら回転", ["演出：動き"], ["ゆらゆら回転", "左右へゆっくり回って戻る", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V151Effect : FxMove { public override string Label => "ゆらゆら回転"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,S(t*1.4f)*.2f*a); }

[VideoEffect("逆回転", ["演出：動き"], ["逆回転", "左回りに回り続ける", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V152Effect : FxMove { public override string Label => "逆回転"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,-t*MathF.Tau*.4f*a); }

[VideoEffect("揺れて止まる", ["演出：動き"], ["揺れて止まる", "大きく揺れて、だんだん止まる", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V153Effect : FxMove { public override string Label => "揺れて止まる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,S(f.T*18f)*MathF.Exp(-f.T*2.2f)*.1f*a,0); }

[VideoEffect("加速ズーム", ["演出：動き"], ["加速ズーム", "だんだん速く寄っていく", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V154Effect : FxMove { public override string Label => "加速ズーム"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+MathF.Pow(f.Dur>0?Math.Clamp(f.T/f.Dur,0f,1f):0f,3f)*.7f*a,1f+MathF.Pow(f.Dur>0?Math.Clamp(f.T/f.Dur,0f,1f):0f,3f)*.7f*a); }

[VideoEffect("ゆっくり引く", ["演出：動き"], ["ゆっくり引く", "大きい状態から少しずつ引いていく", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V155Effect : FxMove { public override string Label => "ゆっくり引く"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(1f-(f.Dur>0?Math.Clamp(f.T/f.Dur,0f,1f):0f))*.3f*a,1f+(1f-(f.Dur>0?Math.Clamp(f.T/f.Dur,0f,1f):0f))*.3f*a); }

[VideoEffect("上へ流れる", ["演出：動き"], ["上へ流れる", "ゆっくり上へ移動し続ける", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V156Effect : FxMove { public override string Label => "上へ流れる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,-(f.Dur>0?f.T/f.Dur:0f)*.4f*a); }

[VideoEffect("下へ流れる", ["演出：動き"], ["下へ流れる", "ゆっくり下へ移動し続ける", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V157Effect : FxMove { public override string Label => "下へ流れる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(f.Dur>0?f.T/f.Dur:0f)*.4f*a); }

[VideoEffect("左へ流れる", ["演出：動き"], ["左へ流れる", "ゆっくり左へ移動し続ける", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V158Effect : FxMove { public override string Label => "左へ流れる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,-(f.Dur>0?f.T/f.Dur:0f)*.4f*a,0); }

[VideoEffect("右へ流れる", ["演出：動き"], ["右へ流れる", "ゆっくり右へ移動し続ける", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V159Effect : FxMove { public override string Label => "右へ流れる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(f.Dur>0?f.T/f.Dur:0f)*.4f*a,0); }

[VideoEffect("歩く動き", ["演出：動き"], ["歩く動き", "左右に傾きながら上下に弾む", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V160Effect : FxMove { public override string Label => "歩く動き"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => RoF(f,S(t*4f)*.05f*a)*Mo(f,0,-MathF.Abs(S(t*4f))*.035f*a); }

[VideoEffect("走る動き", ["演出：動き"], ["走る動き", "前傾して速く弾む", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V161Effect : FxMove { public override string Label => "走る動き"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => RoF(f,(.08f+S(t*10f)*.03f)*a)*Mo(f,0,-MathF.Abs(S(t*10f))*.05f*a); }

[VideoEffect("スキップ", ["演出：動き"], ["スキップ", "大小の跳ねを交互に繰り返す", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V162Effect : FxMove { public override string Label => "スキップ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,-MathF.Abs(S(t*4f))*(.05f+.05f*MathF.Max(0f,S(t*2f)))*a); }

[VideoEffect("ダンス", ["演出：動き"], ["ダンス", "傾きと伸び縮みでリズムに乗る", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V163Effect : FxMove { public override string Label => "ダンス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => RoF(f,S(t*3.14f)*.1f*a)*Ft(f,1f-MathF.Abs(S(t*6.28f))*.06f*a,1f+MathF.Abs(S(t*6.28f))*.06f*a); }

[VideoEffect("びっくり跳ね", ["演出：動き"], ["びっくり跳ね", "ときどき急に跳び上がる", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V164Effect : FxMove { public override string Label => "びっくり跳ね"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,-MathF.Pow(MathF.Max(0f,S(t*1.5f)),24f)*.22f*a); }

[VideoEffect("ガクガク", ["演出：動き"], ["ガクガク", "カクカクと段階的に揺れる", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V165Effect : FxMove { public override string Label => "ガクガク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Floor(N(t*2f,7)*4f)/4f)*.06f*a,(MathF.Floor(N(t*2f,8)*4f)/4f)*.05f*a); }

[VideoEffect("左上から出る", ["演出：出入り"], ["左上から出る", "左上から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V166Effect : FxEnter { public override string Label => "左上から出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-d*dir,-d*dir); }

[VideoEffect("右上から出る", ["演出：出入り"], ["右上から出る", "右上から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V167Effect : FxEnter { public override string Label => "右上から出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,d*dir,-d*dir); }

[VideoEffect("左下から出る", ["演出：出入り"], ["左下から出る", "左下から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V168Effect : FxEnter { public override string Label => "左下から出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-d*dir,d*dir); }

[VideoEffect("右下から出る", ["演出：出入り"], ["右下から出る", "右下から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V169Effect : FxEnter { public override string Label => "右下から出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,d*dir,d*dir); }

[VideoEffect("遠くから飛んでくる", ["演出：出入り"], ["遠くから飛んでくる", "小さな点から回転せずに近づく", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V170Effect : FxEnter { public override string Label => "遠くから飛んでくる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f-d,1f-d)*Mo(f,d*.25f*dir,-d*.25f); }

[VideoEffect("手前から迫る", ["演出：出入り"], ["手前から迫る", "大きな状態から元の大きさになる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V171Effect : FxEnter { public override string Label => "手前から迫る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f+d*2.2f,1f+d*2.2f); }

[VideoEffect("上から回って出る", ["演出：出入り"], ["上から回って出る", "回りながら上から入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V172Effect : FxEnter { public override string Label => "上から回って出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,d*3f*dir)*Mo(f,0,-d); }

[VideoEffect("下から回って出る", ["演出：出入り"], ["下から回って出る", "回りながら下から入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V173Effect : FxEnter { public override string Label => "下から回って出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-d*3f*dir)*Mo(f,0,d); }

[VideoEffect("左から回って出る", ["演出：出入り"], ["左から回って出る", "転がるように左から入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V174Effect : FxEnter { public override string Label => "左から回って出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-d*5f)*Mo(f,-d*dir,0); }

[VideoEffect("右から回って出る", ["演出：出入り"], ["右から回って出る", "転がるように右から入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V175Effect : FxEnter { public override string Label => "右から回って出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,d*5f)*Mo(f,d*dir,0); }

[VideoEffect("斜めに伸びて出る", ["演出：出入り"], ["斜めに伸びて出る", "斜めにゆがんだ状態から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V176Effect : FxEnter { public override string Label => "斜めに伸びて出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mv.SkewX(f.C.Y,d*1.6f*dir)*Sc(f,1f-d*.5f,1f); }

[VideoEffect("つぶれて出入り", ["演出：出入り"], ["つぶれて出入り", "足もとへつぶれた状態から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V177Effect : FxEnter { public override string Label => "つぶれて出入り"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ft(f,1f+d*.5f,1f-d); }

[VideoEffect("竜巻で出る", ["演出：出入り"], ["竜巻で出る", "高速に回りながら広がる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V178Effect : FxEnter { public override string Label => "竜巻で出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,d*12f*dir)*Sc(f,1f-d,1f-d); }

[VideoEffect("ねじれて出る", ["演出：出入り"], ["ねじれて出る", "回転と斜めのゆがみで現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V179Effect : FxEnter { public override string Label => "ねじれて出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,d*1.2f*dir)*Mv.SkewX(f.C.Y,d*1.1f)*Sc(f,1f-d*.6f,1f-d*.2f); }

[VideoEffect("横に倒れて出る", ["演出：出入り"], ["横に倒れて出る", "足もとを軸に倒れた所から起きる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V180Effect : FxEnter { public override string Label => "横に倒れて出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => RoF(f,d*1.57f*dir); }

[VideoEffect("起き上がる", ["演出：出入り"], ["起き上がる", "後ろへ寝た状態から立ち上がる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V181Effect : FxEnter { public override string Label => "起き上がる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ft(f,1f,1f-d); }

[VideoEffect("振り子で出る", ["演出：出入り"], ["振り子で出る", "上を軸に大きく振れて入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V182Effect : FxEnter { public override string Label => "振り子で出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mv.Rot(new System.Numerics.Vector2(f.C.X,f.B.Top),d*1.9f*dir); }

[VideoEffect("ふわっと上昇", ["演出：出入り"], ["ふわっと上昇", "少し下からやわらかく浮かぶ", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V183Effect : FxEnter { public override string Label => "ふわっと上昇"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0,d*.18f)*Sc(f,1f-d*.08f,1f-d*.08f); }

[VideoEffect("すとんと落ちる", ["演出：出入り"], ["すとんと落ちる", "少し上から一気に降りる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V184Effect : FxEnter { public override string Label => "すとんと落ちる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0,-d*.45f)*Ft(f,1f+d*.2f,1f-d*.2f); }

[VideoEffect("横に流れて伸びる", ["演出：出入り"], ["横に流れて伸びる", "横長に伸びながら流れ込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V185Effect : FxEnter { public override string Label => "横に流れて伸びる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f+d*2f,1f-d*.6f)*Mo(f,-d*.8f*dir,0); }

[VideoEffect("横線の模様", ["演出：質感・画風"], ["横線の模様", "流れる横線を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V186Effect : FxPat { public override string Label => "横線の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float y = t + o % st; y < bo; y += st) Line(dc, b, l, y, r, y, st * .18f); } }

[VideoEffect("縦線の模様", ["演出：質感・画風"], ["縦線の模様", "流れる縦線を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V187Effect : FxPat { public override string Label => "縦線の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float x = l + o % st; x < r; x += st) Line(dc, b, x, t, x, bo, st * .18f); } }

[VideoEffect("チェック柄", ["演出：質感・画風"], ["チェック柄", "格子状のチェックを重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V188Effect : FxPat { public override string Label => "チェック柄"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float y = t; y < bo; y += st * 2) dc.FillRectangle(new Vortice.RawRectF(l, y, r, y + st), b); for (float x = l; x < r; x += st * 2) dc.FillRectangle(new Vortice.RawRectF(x, t, x + st, bo), b); } }

[VideoEffect("市松模様", ["演出：質感・画風"], ["市松模様", "交互に並ぶ四角を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V189Effect : FxPat { public override string Label => "市松模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { int ry = 0; for (float y = t; y < bo; y += st, ry++) { int cx = 0; for (float x = l; x < r; x += st, cx++) if ((ry + cx) % 2 == 0) dc.FillRectangle(new Vortice.RawRectF(x, y, x + st, y + st), b); } } }

[VideoEffect("同心円の模様", ["演出：質感・画風"], ["同心円の模様", "中心から広がる輪を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V190Effect : FxPat { public override string Label => "同心円の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float mx = (l + r) / 2, my = (t + bo) / 2, big = MathF.Max(r - l, bo - t); for (float q = o % st; q < big; q += st) Ring(dc, b, mx, my, q + 1, st * .2f); } }

[VideoEffect("放射線の模様", ["演出：質感・画風"], ["放射線の模様", "中心から伸びる線を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V191Effect : FxPat { public override string Label => "放射線の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float mx = (l + r) / 2, my = (t + bo) / 2, big = MathF.Max(r - l, bo - t), a0 = o * .004f; int n = (int)(big / st) + 8; for (int i = 0; i < n; i++) { float a = a0 + i * MathF.Tau / n; Poly(dc, b, mx, my, mx + MathF.Cos(a - .04f) * big, my + MathF.Sin(a - .04f) * big, mx + MathF.Cos(a + .04f) * big, my + MathF.Sin(a + .04f) * big); } } }

[VideoEffect("三角形の模様", ["演出：質感・画風"], ["三角形の模様", "並んだ三角を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V192Effect : FxPat { public override string Label => "三角形の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { Each(l, t, r, bo, st, (x, y, _) => Poly(dc, b, x, y - st * .34f, x + st * .36f, y + st * .28f, x - st * .36f, y + st * .28f)); } }

[VideoEffect("ひし形の模様", ["演出：質感・画風"], ["ひし形の模様", "並んだひし形を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V193Effect : FxPat { public override string Label => "ひし形の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { Each(l, t, r, bo, st, (x, y, _) => Poly(dc, b, x, y - st * .38f, x + st * .3f, y, x, y + st * .38f, x - st * .3f, y)); } }

[VideoEffect("星の模様", ["演出：質感・画風"], ["星の模様", "並んだ星を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V194Effect : FxPat { public override string Label => "星の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { Each(l, t, r, bo, st, (x, y, _) => YMM4EffectShapes.Gfx.FillPoly(dc, b, YMM4EffectShapes.ShapeGeom.StarPoly(new(x, y), st * .36f, 5, .45f, -MathF.PI / 2f))); } }

[VideoEffect("ハートの模様", ["演出：質感・画風"], ["ハートの模様", "並んだハートを重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V195Effect : FxPat { public override string Label => "ハートの模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { Each(l, t, r, bo, st, (x, y, _) => YMM4EffectShapes.Gfx.FillPoly(dc, b, YMM4EffectShapes.ShapeGeom.HeartPoly(new(x, y), st * .3f, 0f))); } }

[VideoEffect("波線の模様", ["演出：質感・画風"], ["波線の模様", "流れる波線を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V196Effect : FxPat { public override string Label => "波線の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float y = t; y < bo + st; y += st) { var q = new List<System.Numerics.Vector2>(); for (float x = l; x <= r + 8; x += 8) q.Add(new(x, y + MathF.Sin((x + o) / st * 3f) * st * .22f)); YMM4EffectShapes.Gfx.Polyline(dc, b, q, st * .12f); } } }

[VideoEffect("十字の模様", ["演出：質感・画風"], ["十字の模様", "並んだ十字を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V197Effect : FxPat { public override string Label => "十字の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { Each(l, t, r, bo, st, (x, y, _) => { Line(dc, b, x - st * .26f, y, x + st * .26f, y, st * .12f); Line(dc, b, x, y - st * .26f, x, y + st * .26f, st * .12f); }); } }

[VideoEffect("六角形の模様", ["演出：質感・画風"], ["六角形の模様", "ハチの巣のような線を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V198Effect : FxPat { public override string Label => "六角形の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { Each(l, t, r, bo, st, (x, y, _) => { var q = new List<System.Numerics.Vector2>(); for (int i = 0; i <= 6; i++) { float a = i * MathF.PI / 3f + MathF.PI / 6f; q.Add(new(x + MathF.Cos(a) * st * .56f, y + MathF.Sin(a) * st * .56f)); } YMM4EffectShapes.Gfx.Polyline(dc, b, q, 2f); }); } }

[VideoEffect("雨の筋の模様", ["演出：質感・画風"], ["雨の筋の模様", "斜めの細い雨筋を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V199Effect : FxPat { public override string Label => "雨の筋の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { Each(l, t, r, bo, st, (x, y, id) => { float d = (o * (1 + FxKit.Hash(id)) + FxKit.Hash(id + 3) * st) % st; Line(dc, b, x - d * .2f, y + d, x - d * .2f - st * .1f, y + d + st * .55f, 1.6f); }); } }

[VideoEffect("泡の模様", ["演出：質感・画風"], ["泡の模様", "大小の輪を重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx3V200Effect : FxPat { public override string Label => "泡の模様"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { Each(l, t, r, bo, st, (x, y, id) => Ring(dc, b, x + FxKit.Hash(id) * st * .5f, y - (o + FxKit.Hash(id + 2) * st) % st, st * (.08f + FxKit.Hash(id + 5) * .22f), 2f)); } }
