using System.Numerics;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 501〜600(gen_v133.py で生成)

internal static class P5
{
    public static float[] Star(float x, float y, float r) { var o = new float[20]; for (int k = 0; k < 10; k++) { float a = -MathF.PI / 2f + k * MathF.PI / 5f, q = k % 2 == 0 ? r : r * .45f; o[k * 2] = x + MathF.Cos(a) * q; o[k * 2 + 1] = y + MathF.Sin(a) * q; } return o; }
    public static float[] Heart(float x, float y, float r) { var o = new float[48]; for (int k = 0; k < 24; k++) { float t = k * MathF.Tau / 24f, hx = 16f * MathF.Pow(MathF.Sin(t), 3f), hy = 13f * MathF.Cos(t) - 5f * MathF.Cos(2f * t) - 2f * MathF.Cos(3f * t) - MathF.Cos(4f * t); o[k * 2] = x + hx * r / 16f; o[k * 2 + 1] = y - hy * r / 16f; } return o; }
    public static float[] Moon(float x, float y, float r) { var o = new List<float>(); for (int k = 0; k <= 12; k++) { float a = MathF.PI * .5f + k * MathF.PI / 12f; o.Add(x + MathF.Cos(a) * r); o.Add(y + MathF.Sin(a) * r); } for (int k = 12; k >= 0; k--) { float a = MathF.PI * .5f + k * MathF.PI / 12f; o.Add(x + r * .35f + MathF.Cos(a) * r * .75f); o.Add(y + MathF.Sin(a) * r * .9f); } return o.ToArray(); }
}

[VideoEffect("2色 紺と金", ["演出：色調整"], ["2色 紺と金", "暗い所を紺、明るい所を金の2色にする", "色調整", "2色", "紺と金", "デュオトーン", "duotone", "ツートン", "高級", "夜", "ゴールド", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V501Effect : FxGrade { public override string Label => "2色 紺と金"; private protected override float[] M => Gd.D(0x1B2A49, 0xF2C14E); }

[VideoEffect("2色 紫とピンク", ["演出：色調整"], ["2色 紫とピンク", "暗い所を紫、明るい所をピンクの2色にする", "色調整", "2色", "紫とピンク", "デュオトーン", "duotone", "ツートン", "かわいい", "ポップ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V502Effect : FxGrade { public override string Label => "2色 紫とピンク"; private protected override float[] M => Gd.D(0x2D1B4E, 0xFF8FB1); }

[VideoEffect("2色 茶とクリーム", ["演出：色調整"], ["2色 茶とクリーム", "暗い所を茶、明るい所をクリームの2色にする", "色調整", "2色", "茶とクリーム", "デュオトーン", "duotone", "ツートン", "レトロ", "セピア", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V503Effect : FxGrade { public override string Label => "2色 茶とクリーム"; private protected override float[] M => Gd.D(0x3B2418, 0xF5E6C8); }

[VideoEffect("2色 緑と黄", ["演出：色調整"], ["2色 緑と黄", "暗い所を緑、明るい所を黄の2色にする", "色調整", "2色", "緑と黄", "デュオトーン", "duotone", "ツートン", "自然", "若葉", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V504Effect : FxGrade { public override string Label => "2色 緑と黄"; private protected override float[] M => Gd.D(0x12372A, 0xE9F27A); }

[VideoEffect("2色 赤と白", ["演出：色調整"], ["2色 赤と白", "暗い所を赤、明るい所を白の2色にする", "色調整", "2色", "赤と白", "デュオトーン", "duotone", "ツートン", "情熱", "ポスター", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V505Effect : FxGrade { public override string Label => "2色 赤と白"; private protected override float[] M => Gd.D(0x5A0E14, 0xFFF1E8); }

[VideoEffect("2色 青と水色", ["演出：色調整"], ["2色 青と水色", "暗い所を青、明るい所を水色の2色にする", "色調整", "2色", "青と水色", "デュオトーン", "duotone", "ツートン", "クール", "海", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V506Effect : FxGrade { public override string Label => "2色 青と水色"; private protected override float[] M => Gd.D(0x0B1F4B, 0x9BE7FF); }

[VideoEffect("2色 黒と橙", ["演出：色調整"], ["2色 黒と橙", "暗い所を黒、明るい所を橙の2色にする", "色調整", "2色", "黒と橙", "デュオトーン", "duotone", "ツートン", "ハロウィン", "夕焼け", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V507Effect : FxGrade { public override string Label => "2色 黒と橙"; private protected override float[] M => Gd.D(0x111111, 0xFF9A3C); }

[VideoEffect("2色 藍と桜", ["演出：色調整"], ["2色 藍と桜", "暗い所を藍、明るい所を桜の2色にする", "色調整", "2色", "藍と桜", "デュオトーン", "duotone", "ツートン", "和風", "春", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V508Effect : FxGrade { public override string Label => "2色 藍と桜"; private protected override float[] M => Gd.D(0x1D2A5A, 0xFFD1DC); }

[VideoEffect("2色 墨と朱", ["演出：色調整"], ["2色 墨と朱", "暗い所を墨、明るい所を朱の2色にする", "色調整", "2色", "墨と朱", "デュオトーン", "duotone", "ツートン", "和風", "版画", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V509Effect : FxGrade { public override string Label => "2色 墨と朱"; private protected override float[] M => Gd.D(0x1A1A1A, 0xE8523A); }

[VideoEffect("2色 深緑と生成り", ["演出：色調整"], ["2色 深緑と生成り", "暗い所を深緑、明るい所を生成りの2色にする", "色調整", "2色", "深緑と生成り", "デュオトーン", "duotone", "ツートン", "森", "ナチュラル", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V510Effect : FxGrade { public override string Label => "2色 深緑と生成り"; private protected override float[] M => Gd.D(0x1F3B2D, 0xF3EBD3); }

[VideoEffect("2色 紫と水色", ["演出：色調整"], ["2色 紫と水色", "暗い所を紫、明るい所を水色の2色にする", "色調整", "2色", "紫と水色", "デュオトーン", "duotone", "ツートン", "サイバー", "幻想", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V511Effect : FxGrade { public override string Label => "2色 紫と水色"; private protected override float[] M => Gd.D(0x3A1C5C, 0x8FF0F0); }

[VideoEffect("2色 茶と橙", ["演出：色調整"], ["2色 茶と橙", "暗い所を茶、明るい所を橙の2色にする", "色調整", "2色", "茶と橙", "デュオトーン", "duotone", "ツートン", "秋", "あたたかい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V512Effect : FxGrade { public override string Label => "2色 茶と橙"; private protected override float[] M => Gd.D(0x2E1A12, 0xFFB25B); }

[VideoEffect("2色 青とピンク", ["演出：色調整"], ["2色 青とピンク", "暗い所を青、明るい所をピンクの2色にする", "色調整", "2色", "青とピンク", "デュオトーン", "duotone", "ツートン", "ポップ", "ネオン", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V513Effect : FxGrade { public override string Label => "2色 青とピンク"; private protected override float[] M => Gd.D(0x1C2C7A, 0xFF9EC4); }

[VideoEffect("2色 灰と黄", ["演出：色調整"], ["2色 灰と黄", "暗い所を灰、明るい所を黄の2色にする", "色調整", "2色", "灰と黄", "デュオトーン", "duotone", "ツートン", "工事", "注意", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V514Effect : FxGrade { public override string Label => "2色 灰と黄"; private protected override float[] M => Gd.D(0x3A3D42, 0xFFE45C); }

[VideoEffect("2色 黒と緑", ["演出：色調整"], ["2色 黒と緑", "暗い所を黒、明るい所を緑の2色にする", "色調整", "2色", "黒と緑", "デュオトーン", "duotone", "ツートン", "ハッカー", "暗視", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V515Effect : FxGrade { public override string Label => "2色 黒と緑"; private protected override float[] M => Gd.D(0x050A05, 0x39FF88); }

[VideoEffect("フィルム 夕方", ["演出：色調整"], ["フィルム 夕方", "夕方の映画のような暖かい色", "色調整", "フィルム", "夕方", "film", "映画", "シネマ", "ルック", "色味", "夕日", "黄昏", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V516Effect : FxGrade { public override string Label => "フィルム 夕方"; private protected override float[] M => Gd.G(1.1f,1.08f,0f,1.12f,1f,.86f); }

[VideoEffect("フィルム 夜", ["演出：色調整"], ["フィルム 夜", "夜の場面のような青く暗めの色", "色調整", "フィルム", "夜", "film", "映画", "シネマ", "ルック", "色味", "夜景", "暗い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V517Effect : FxGrade { public override string Label => "フィルム 夜"; private protected override float[] M => Gd.G(.8f,1.1f,-.05f,.85f,.95f,1.15f); }

[VideoEffect("フィルム 色あせ", ["演出：色調整"], ["フィルム 色あせ", "古いフィルムのような色あせた色", "色調整", "フィルム", "色あせ", "film", "映画", "シネマ", "ルック", "色味", "古い", "昔", "思い出", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V518Effect : FxGrade { public override string Label => "フィルム 色あせ"; private protected override float[] M => Gd.G(.6f,.85f,.06f,1.05f,1f,.92f); }

[VideoEffect("フィルム 硬め", ["演出：色調整"], ["フィルム 硬め", "コントラストの強い硬い映像", "色調整", "フィルム", "硬め", "film", "映画", "シネマ", "ルック", "色味", "くっきり", "劇的", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V519Effect : FxGrade { public override string Label => "フィルム 硬め"; private protected override float[] M => Gd.G(1.05f,1.3f,-.03f); }

[VideoEffect("フィルム 眠い", ["演出：色調整"], ["フィルム 眠い", "コントラストを下げた眠い映像", "色調整", "フィルム", "眠い", "film", "映画", "シネマ", "ルック", "色味", "ふんわり", "淡い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V520Effect : FxGrade { public override string Label => "フィルム 眠い"; private protected override float[] M => Gd.G(.7f,.75f,.08f); }

[VideoEffect("フィルム 冬の朝", ["演出：色調整"], ["フィルム 冬の朝", "冬の朝のような冷たく明るい色", "色調整", "フィルム", "冬の朝", "film", "映画", "シネマ", "ルック", "色味", "寒い", "雪", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V521Effect : FxGrade { public override string Label => "フィルム 冬の朝"; private protected override float[] M => Gd.G(.75f,1.05f,.04f,.92f,1f,1.12f); }

[VideoEffect("フィルム 夏の昼", ["演出：色調整"], ["フィルム 夏の昼", "夏の昼のような鮮やかな色", "色調整", "フィルム", "夏の昼", "film", "映画", "シネマ", "ルック", "色味", "暑い", "鮮やか", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V522Effect : FxGrade { public override string Label => "フィルム 夏の昼"; private protected override float[] M => Gd.G(1.3f,1.1f,.03f,1.05f,1.05f,.95f); }

[VideoEffect("フィルム 雨の日", ["演出：色調整"], ["フィルム 雨の日", "雨の日のような沈んだ色", "色調整", "フィルム", "雨の日", "film", "映画", "シネマ", "ルック", "色味", "曇り", "しっとり", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V523Effect : FxGrade { public override string Label => "フィルム 雨の日"; private protected override float[] M => Gd.G(.55f,.9f,-.02f,.92f,.98f,1.05f); }

[VideoEffect("フィルム ネオン", ["演出：色調整"], ["フィルム ネオン", "ネオン街のような派手な色", "色調整", "フィルム", "ネオン", "film", "映画", "シネマ", "ルック", "色味", "夜の街", "派手", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V524Effect : FxGrade { public override string Label => "フィルム ネオン"; private protected override float[] M => Gd.G(1.6f,1.15f,-.04f,1.05f,.9f,1.15f); }

[VideoEffect("フィルム 秋の午後", ["演出：色調整"], ["フィルム 秋の午後", "秋の午後のような黄色っぽい色", "色調整", "フィルム", "秋の午後", "film", "映画", "シネマ", "ルック", "色味", "紅葉", "秋", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V525Effect : FxGrade { public override string Label => "フィルム 秋の午後"; private protected override float[] M => Gd.G(1.15f,1f,.02f,1.12f,1.02f,.8f); }

[VideoEffect("動き2 8の字", ["演出：動き"], ["動き2 8の字", "8の字を描くように動く", "動き", "動き2", "8の字", "モーション", "ループ", "エイト", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V526Effect : FxMove { public override string Label => "動き2 8の字"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * 2f) * .06f * a, S(t * 4f) * .03f * a); }

[VideoEffect("動き2 円を描く", ["演出：動き"], ["動き2 円を描く", "円を描くように回り続ける", "動き", "動き2", "円を描く", "モーション", "ループ", "ぐるぐる", "回る", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V527Effect : FxMove { public override string Label => "動き2 円を描く"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, C(t * 2f) * .05f * a, S(t * 2f) * .05f * a); }

[VideoEffect("動き2 振り子", ["演出：動き"], ["動き2 振り子", "上を支点に振り子のように揺れる", "動き", "動き2", "振り子", "モーション", "ループ", "ぶら下がる", "揺れる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V528Effect : FxMove { public override string Label => "動き2 振り子"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mv.Rot(new Vector2(f.C.X, f.B.Top), S(t * 2.5f) * .3f * a); }

[VideoEffect("動き2 首ふり", ["演出：動き"], ["動き2 首ふり", "首をふるように左右に傾く", "動き", "動き2", "首ふり", "モーション", "ループ", "いやいや", "傾く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V529Effect : FxMove { public override string Label => "動き2 首ふり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, S(t * 3f) * .12f * a); }

[VideoEffect("動き2 心臓の鼓動", ["演出：動き"], ["動き2 心臓の鼓動", "ドクンと鼓動するように大きくなる", "動き", "動き2", "心臓の鼓動", "モーション", "ループ", "ドキドキ", "脈", "ハート", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V530Effect : FxMove { public override string Label => "動き2 心臓の鼓動"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f, 1f + .08f * a * MathF.Pow(MathF.Max(0f, S(t * 5f)), 6f), 1f + .08f * a * MathF.Pow(MathF.Max(0f, S(t * 5f)), 6f)); }

[VideoEffect("動き2 ぴょんぴょん", ["演出：動き"], ["動き2 ぴょんぴょん", "その場でぴょんぴょん跳ねる", "動き", "動き2", "ぴょんぴょん", "モーション", "ループ", "ジャンプ", "跳ねる", "喜ぶ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V531Effect : FxMove { public override string Label => "動き2 ぴょんぴょん"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, 0, -MathF.Abs(S(t * 4f)) * .08f * a); }

[VideoEffect("動き2 よっぱらい", ["演出：動き"], ["動き2 よっぱらい", "ふらふらと不規則に揺れる", "動き", "動き2", "よっぱらい", "モーション", "ループ", "ふらふら", "酔う", "めまい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V532Effect : FxMove { public override string Label => "動き2 よっぱらい"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, N(t * .8f, 3) * .15f * a) * Mo(f, N(t * .6f, 5) * .05f * a, 0); }

[VideoEffect("動き2 船の上", ["演出：動き"], ["動き2 船の上", "船に乗っているようにゆったり揺れる", "動き", "動き2", "船の上", "モーション", "ループ", "波", "ゆらゆら", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V533Effect : FxMove { public override string Label => "動き2 船の上"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => RoF(f, S(t * 1.5f) * .1f * a) * Mo(f, 0, S(t * 3f) * .02f * a); }

[VideoEffect("動き2 ぷるぷる", ["演出：動き"], ["動き2 ぷるぷる", "小さく小刻みに震える", "動き", "動き2", "ぷるぷる", "モーション", "ループ", "震える", "寒い", "怖い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V534Effect : FxMove { public override string Label => "動き2 ぷるぷる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, N(t * 20f, 7) * .012f * a, N(t * 20f, 9) * .012f * a); }

[VideoEffect("動き2 くるくる", ["演出：動き"], ["動き2 くるくる", "その場でずっと回り続ける", "動き", "動き2", "くるくる", "モーション", "ループ", "回転", "スピン", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V535Effect : FxMove { public override string Label => "動き2 くるくる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, t * 2f * a); }

[VideoEffect("動き2 ゆっくり拡大", ["演出：動き"], ["動き2 ゆっくり拡大", "ゆっくり大きくなったり戻ったりする", "動き", "動き2", "ゆっくり拡大", "モーション", "ループ", "ズーム", "呼吸", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V536Effect : FxMove { public override string Label => "動き2 ゆっくり拡大"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f, 1f + .1f * a * (.5f + .5f * S(t * .8f)), 1f + .1f * a * (.5f + .5f * S(t * .8f))); }

[VideoEffect("動き2 ぺこぺこ", ["演出：動き"], ["動き2 ぺこぺこ", "足元を支点に縦につぶれる", "動き", "動き2", "ぺこぺこ", "モーション", "ループ", "お辞儀", "つぶれる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V537Effect : FxMove { public override string Label => "動き2 ぺこぺこ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f, 1f, 1f - .08f * a * MathF.Abs(S(t * 4f))); }

[VideoEffect("動き2 びよーん", ["演出：動き"], ["動き2 びよーん", "ばねのように縦横に伸び縮みする", "動き", "動き2", "びよーん", "モーション", "ループ", "ばね", "伸びる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V538Effect : FxMove { public override string Label => "動き2 びよーん"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f, 1f - .06f * a * S(t * 5f), 1f + .1f * a * S(t * 5f)); }

[VideoEffect("動き2 スキップ", ["演出：動き"], ["動き2 スキップ", "左右に動きながら小さく跳ねる", "動き", "動き2", "スキップ", "モーション", "ループ", "楽しい", "歩く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V539Effect : FxMove { public override string Label => "動き2 スキップ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * 2f) * .05f * a, -MathF.Abs(S(t * 6f)) * .04f * a); }

[VideoEffect("動き2 風になびく", ["演出：動き"], ["動き2 風になびく", "風になびくように上が左右に傾く", "動き", "動き2", "風になびく", "モーション", "ループ", "旗", "風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V540Effect : FxMove { public override string Label => "動き2 風になびく"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f, S(t * 2f) * .12f * a); }

[VideoEffect("動き2 地震", ["演出：動き"], ["動き2 地震", "地震のように激しく揺れる", "動き", "動き2", "地震", "モーション", "ループ", "ゆれ", "衝撃", "シェイク", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V541Effect : FxMove { public override string Label => "動き2 地震"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, N(t * 30f, 11) * .03f * a, N(t * 30f, 13) * .02f * a); }

[VideoEffect("動き2 浮き沈み", ["演出：動き"], ["動き2 浮き沈み", "水に浮かぶようにゆっくり上下する", "動き", "動き2", "浮き沈み", "モーション", "ループ", "浮かぶ", "ぷかぷか", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V542Effect : FxMove { public override string Label => "動き2 浮き沈み"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, 0, S(t * 1.2f) * .05f * a); }

[VideoEffect("動き2 ジグザグ", ["演出：動き"], ["動き2 ジグザグ", "左右に一定の速さで行き来する", "動き", "動き2", "ジグザグ", "モーション", "ループ", "往復", "パトロール", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V543Effect : FxMove { public override string Label => "動き2 ジグザグ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, (2f * MathF.Abs(2f * (t * .8f - MathF.Floor(t * .8f + .5f))) - 1f) * .06f * a, 0); }

[VideoEffect("動き2 きょろきょろ", ["演出：動き"], ["動き2 きょろきょろ", "きょろきょろと左右に向きを変える", "動き", "動き2", "きょろきょろ", "モーション", "ループ", "探す", "見回す", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V544Effect : FxMove { public override string Label => "動き2 きょろきょろ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, (S(t * 1.5f) > 0 ? 1f : -1f) * .08f * a); }

[VideoEffect("動き2 回転木馬", ["演出：動き"], ["動き2 回転木馬", "奥と手前を回るように動く", "動き", "動き2", "回転木馬", "モーション", "ループ", "メリーゴーランド", "回る", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V545Effect : FxMove { public override string Label => "動き2 回転木馬"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * 2f) * .06f * a, 0) * Sc(f, 1f + C(t * 2f) * .06f * a, 1f + C(t * 2f) * .06f * a); }

[VideoEffect("出入り2 転がる", ["演出：出入り"], ["出入り2 転がる", "転がりながら横から出る", "出入り", "出入り2", "転がる", "登場", "退場", "トランジション", "ころころ", "回転", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V546Effect : FxEnter { public override string Label => "出入り2 転がる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, -d * MathF.Tau * dir) * Mo(f, -.5f * d * dir, 0); }

[VideoEffect("出入り2 縦に伸びる", ["演出：出入り"], ["出入り2 縦に伸びる", "縦に伸びて出る", "出入り", "出入り2", "縦に伸びる", "登場", "退場", "トランジション", "伸びる", "開く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V547Effect : FxEnter { public override string Label => "出入り2 縦に伸びる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f, 1f - d); }

[VideoEffect("出入り2 横に伸びる", ["演出：出入り"], ["出入り2 横に伸びる", "横に伸びて出る", "出入り", "出入り2", "横に伸びる", "登場", "退場", "トランジション", "伸びる", "開く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V548Effect : FxEnter { public override string Label => "出入り2 横に伸びる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f - d, 1f); }

[VideoEffect("出入り2 下から伸びる", ["演出：出入り"], ["出入り2 下から伸びる", "足元から上へ伸びて出る", "出入り", "出入り2", "下から伸びる", "登場", "退場", "トランジション", "生える", "伸びる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V549Effect : FxEnter { public override string Label => "出入り2 下から伸びる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ft(f, 1f, 1f - d); }

[VideoEffect("出入り2 つぶれから", ["演出：出入り"], ["出入り2 つぶれから", "つぶれた形から元に戻って出る", "出入り", "出入り2", "つぶれから", "登場", "退場", "トランジション", "ぺちゃんこ", "弾む", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V550Effect : FxEnter { public override string Label => "出入り2 つぶれから"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f + d * .8f, 1f - d); }

[VideoEffect("出入り2 遠くから回る", ["演出：出入り"], ["出入り2 遠くから回る", "小さく回りながら近づいて出る", "出入り", "出入り2", "遠くから回る", "登場", "退場", "トランジション", "回転", "ズーム", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V551Effect : FxEnter { public override string Label => "出入り2 遠くから回る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f - d, 1f - d) * Ro(f, d * 3f * dir); }

[VideoEffect("出入り2 回って落ちる", ["演出：出入り"], ["出入り2 回って落ちる", "回りながら上から落ちて出る", "出入り", "出入り2", "回って落ちる", "登場", "退場", "トランジション", "落下", "回転", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V552Effect : FxEnter { public override string Label => "出入り2 回って落ちる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, d * 1.2f * dir) * Mo(f, 0, -.6f * d); }

[VideoEffect("出入り2 倒れて起きる", ["演出：出入り"], ["出入り2 倒れて起きる", "倒れた状態から起き上がって出る", "出入り", "出入り2", "倒れて起きる", "登場", "退場", "トランジション", "起きる", "倒れる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V553Effect : FxEnter { public override string Label => "出入り2 倒れて起きる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => RoF(f, -d * 1.4f * dir); }

[VideoEffect("出入り2 傾いて滑る", ["演出：出入り"], ["出入り2 傾いて滑る", "斜めに傾きながら滑って出る", "出入り", "出入り2", "傾いて滑る", "登場", "退場", "トランジション", "スライド", "勢い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V554Effect : FxEnter { public override string Label => "出入り2 傾いて滑る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f, d * 1.2f * dir) * Mo(f, -.3f * d * dir, 0); }

[VideoEffect("出入り2 大きい所から", ["演出：出入り"], ["出入り2 大きい所から", "大きい状態から縮んで出る", "出入り", "出入り2", "大きい所から", "登場", "退場", "トランジション", "ズームアウト", "迫力", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V555Effect : FxEnter { public override string Label => "出入り2 大きい所から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f + d * 2f, 1f + d * 2f); }

[VideoEffect("出入り2 ゆれて止まる", ["演出：出入り"], ["出入り2 ゆれて止まる", "左右に揺れながら止まって出る", "出入り", "出入り2", "ゆれて止まる", "登場", "退場", "トランジション", "振り子", "揺れる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V556Effect : FxEnter { public override string Label => "出入り2 ゆれて止まる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, MathF.Sin(d * 12f) * d * .6f); }

[VideoEffect("出入り2 ぐにゃっと", ["演出：出入り"], ["出入り2 ぐにゃっと", "ぐにゃぐにゃ曲がりながら出る", "出入り", "出入り2", "ぐにゃっと", "登場", "退場", "トランジション", "ゆがむ", "ゼリー", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V557Effect : FxEnter { public override string Label => "出入り2 ぐにゃっと"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f, MathF.Sin(d * 9f) * d * .8f); }

[VideoEffect("出入り2 右下から回る", ["演出：出入り"], ["出入り2 右下から回る", "回りながら右下から出る", "出入り", "出入り2", "右下から回る", "登場", "退場", "トランジション", "回転", "スライド", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V558Effect : FxEnter { public override string Label => "出入り2 右下から回る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, -d * 1.5f * dir) * Mo(f, .4f * d * dir, .4f * d * dir); }

[VideoEffect("出入り2 ばね", ["演出：出入り"], ["出入り2 ばね", "ばねのように弾みながら出る", "出入り", "出入り2", "ばね", "登場", "退場", "トランジション", "弾む", "ボヨン", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V559Effect : FxEnter { public override string Label => "出入り2 ばね"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f - MathF.Sin(d * 10f) * d * .5f - d * .5f, 1f - MathF.Sin(d * 10f) * d * .5f - d * .5f); }

[VideoEffect("出入り2 横回転", ["演出：出入り"], ["出入り2 横回転", "カードがめくれるように横に回って出る", "出入り", "出入り2", "横回転", "登場", "退場", "トランジション", "めくる", "フリップ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V560Effect : FxEnter { public override string Label => "出入り2 横回転"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, MathF.Abs(MathF.Cos(d * 4.7f)), 1f); }

[VideoEffect("出入り2 縦回転", ["演出：出入り"], ["出入り2 縦回転", "縦にくるっと回って出る", "出入り", "出入り2", "縦回転", "登場", "退場", "トランジション", "めくる", "フリップ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V561Effect : FxEnter { public override string Label => "出入り2 縦回転"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f, MathF.Abs(MathF.Cos(d * 4.7f))); }

[VideoEffect("出入り2 吸いこみ", ["演出：出入り"], ["出入り2 吸いこみ", "上から吸いこまれるように出入りする", "出入り", "出入り2", "吸いこみ", "登場", "退場", "トランジション", "吸い込む", "消える", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V562Effect : FxEnter { public override string Label => "出入り2 吸いこみ"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f - d, 1f - d) * Mo(f, 0, -.4f * d); }

[VideoEffect("出入り2 ぶるぶる", ["演出：出入り"], ["出入り2 ぶるぶる", "左右に震えながら出る", "出入り", "出入り2", "ぶるぶる", "登場", "退場", "トランジション", "震える", "振動", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V563Effect : FxEnter { public override string Label => "出入り2 ぶるぶる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, MathF.Sin(d * 14f) * .12f * d, 0); }

[VideoEffect("出入り2 ねじれ", ["演出：出入り"], ["出入り2 ねじれ", "ねじれながら現れる", "出入り", "出入り2", "ねじれ", "登場", "退場", "トランジション", "ひねる", "回転", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V564Effect : FxEnter { public override string Label => "出入り2 ねじれ"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, d * 2f * dir) * Sc(f, 1f - d * .6f, 1f - d); }

[VideoEffect("出入り2 ななめ上から", ["演出：出入り"], ["出入り2 ななめ上から", "左ななめ上から降りて出る", "出入り", "出入り2", "ななめ上から", "登場", "退場", "トランジション", "スライド", "斜め", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V565Effect : FxEnter { public override string Label => "出入り2 ななめ上から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, -.35f * d * dir, -.5f * d); }

[VideoEffect("重ね模様 星", ["演出：質感・画風"], ["重ね模様 星", "星の模様を重ねる", "質感・画風", "重ね模様", "星", "模様", "パターン", "柄", "テクスチャ", "スター", "きらきら", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V566Effect : FxPat { public override string Label => "重ね模様 星"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.3f; Each(l, t, r, bo, u, (x, y, i) => Poly(dc, b, P5.Star(x + o % u, y, u * .3f))); } }

[VideoEffect("重ね模様 ハート", ["演出：質感・画風"], ["重ね模様 ハート", "ハートの模様を重ねる", "質感・画風", "重ね模様", "ハート", "模様", "パターン", "柄", "テクスチャ", "恋", "かわいい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V567Effect : FxPat { public override string Label => "重ね模様 ハート"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.3f; Each(l, t, r, bo, u, (x, y, i) => Poly(dc, b, P5.Heart(x + o % u, y, u * .28f))); } }

[VideoEffect("重ね模様 格子", ["演出：質感・画風"], ["重ね模様 格子", "方眼紙のような格子を重ねる", "質感・画風", "重ね模様", "格子", "模様", "パターン", "柄", "テクスチャ", "グリッド", "方眼", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V568Effect : FxPat { public override string Label => "重ね模様 格子"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float x = l - st + o % st; x <= r + st; x += st) Line(dc, b, x, t, x, bo, 2f); for (float y = t; y <= bo; y += st) Line(dc, b, l, y, r, y, 2f); } }

[VideoEffect("重ね模様 波線", ["演出：質感・画風"], ["重ね模様 波線", "波線の模様を重ねる", "質感・画風", "重ね模様", "波線", "模様", "パターン", "柄", "テクスチャ", "なみなみ", "ウェーブ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V569Effect : FxPat { public override string Label => "重ね模様 波線"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float y = t; y <= bo + st; y += st * .7f) for (float x = l - st; x < r + st; x += 8f) Line(dc, b, x, y + MathF.Sin((x + o) * 6.28f / st) * st * .15f, x + 8f, y + MathF.Sin((x + 8f + o) * 6.28f / st) * st * .15f, 2.5f); } }

[VideoEffect("重ね模様 雪", ["演出：質感・画風"], ["重ね模様 雪", "雪の結晶の模様が降る", "質感・画風", "重ね模様", "雪", "模様", "パターン", "柄", "テクスチャ", "冬", "結晶", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V570Effect : FxPat { public override string Label => "重ね模様 雪"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => { float yy = y + o % u, s = u * .22f; for (int k = 0; k < 3; k++) { float an = k * 1.047f; Line(dc, b, x - MathF.Cos(an) * s, yy - MathF.Sin(an) * s, x + MathF.Cos(an) * s, yy + MathF.Sin(an) * s, 1.6f); } }); } }

[VideoEffect("重ね模様 花", ["演出：質感・画風"], ["重ね模様 花", "小さな花の模様を重ねる", "質感・画風", "重ね模様", "花", "模様", "パターン", "柄", "テクスチャ", "花柄", "春", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V571Effect : FxPat { public override string Label => "重ね模様 花"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => { float xx = x + o % u, rr = u * .1f; for (int k = 0; k < 5; k++) Dot(dc, b, xx + MathF.Cos(k * 1.2566f) * rr * 1.4f, y + MathF.Sin(k * 1.2566f) * rr * 1.4f, rr); }); } }

[VideoEffect("重ね模様 音符", ["演出：質感・画風"], ["重ね模様 音符", "音符の模様がのぼっていく", "質感・画風", "重ね模様", "音符", "模様", "パターン", "柄", "テクスチャ", "音楽", "歌", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V572Effect : FxPat { public override string Label => "重ね模様 音符"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => { float yy = y - o % u; Dot(dc, b, x, yy, u * .1f); Line(dc, b, x + u * .09f, yy, x + u * .09f, yy - u * .35f, 2f); Line(dc, b, x + u * .09f, yy - u * .35f, x + u * .22f, yy - u * .25f, 2f); }); } }

[VideoEffect("重ね模様 市松", ["演出：質感・画風"], ["重ね模様 市松", "市松模様を重ねる", "質感・画風", "重ね模様", "市松", "模様", "パターン", "柄", "テクスチャ", "チェック", "和柄", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V573Effect : FxPat { public override string Label => "重ね模様 市松"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { int row = 0; for (float y = t - st; y <= bo; y += st, row++) for (float x = l - st * 2f + o % (st * 2f) + (row % 2) * st; x <= r; x += st * 2f) dc.FillRectangle(new Vortice.RawRectF(x, y, x + st, y + st), b); } }

[VideoEffect("重ね模様 横じま", ["演出：質感・画風"], ["重ね模様 横じま", "横じまの模様を重ねる", "質感・画風", "重ね模様", "横じま", "模様", "パターン", "柄", "テクスチャ", "ボーダー", "ストライプ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V574Effect : FxPat { public override string Label => "重ね模様 横じま"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float y = t - st + o % st; y <= bo; y += st) dc.FillRectangle(new Vortice.RawRectF(l, y, r, y + st * .4f), b); } }

[VideoEffect("重ね模様 縦じま", ["演出：質感・画風"], ["重ね模様 縦じま", "縦じまの模様を重ねる", "質感・画風", "重ね模様", "縦じま", "模様", "パターン", "柄", "テクスチャ", "ストライプ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V575Effect : FxPat { public override string Label => "重ね模様 縦じま"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float x = l - st + o % st; x <= r; x += st) dc.FillRectangle(new Vortice.RawRectF(x, t, x + st * .4f, bo), b); } }

[VideoEffect("重ね模様 点線", ["演出：質感・画風"], ["重ね模様 点線", "点線の模様を重ねる", "質感・画風", "重ね模様", "点線", "模様", "パターン", "柄", "テクスチャ", "ミシン目", "破線", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V576Effect : FxPat { public override string Label => "重ね模様 点線"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float y = t; y <= bo; y += st) for (float x = l - st + o % st; x <= r; x += st * .5f) Line(dc, b, x, y, x + st * .25f, y, 2.5f); } }

[VideoEffect("重ね模様 うろこ", ["演出：質感・画風"], ["重ね模様 うろこ", "うろこのような丸の重なりを重ねる", "質感・画風", "重ね模様", "うろこ", "模様", "パターン", "柄", "テクスチャ", "和柄", "鱗", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V577Effect : FxPat { public override string Label => "重ね模様 うろこ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.1f; Each(l, t, r, bo, u, (x, y, i) => Ring(dc, b, x + o % u, y, u * .5f, 2f)); } }

[VideoEffect("重ね模様 雨", ["演出：質感・画風"], ["重ね模様 雨", "雨のすじが降る模様を重ねる", "質感・画風", "重ね模様", "雨", "模様", "パターン", "柄", "テクスチャ", "しとしと", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V578Effect : FxPat { public override string Label => "重ね模様 雨"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.2f; Each(l, t, r, bo, u, (x, y, i) => { float yy = y + (o * 3f + i * 13f) % u; Line(dc, b, x, yy, x - u * .08f, yy + u * .35f, 1.6f); }); } }

[VideoEffect("重ね模様 紙吹雪", ["演出：質感・画風"], ["重ね模様 紙吹雪", "紙吹雪が舞う模様を重ねる", "質感・画風", "重ね模様", "紙吹雪", "模様", "パターン", "柄", "テクスチャ", "お祝い", "パーティー", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V579Effect : FxPat { public override string Label => "重ね模様 紙吹雪"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.3f; Each(l, t, r, bo, u, (x, y, i) => { float yy = y + (o * 2f + i * 17f) % u, an = i * .7f + o * .05f; Poly(dc, b, x + MathF.Cos(an) * u * .12f, yy + MathF.Sin(an) * u * .06f, x - MathF.Sin(an) * u * .06f, yy + MathF.Cos(an) * u * .12f, x - MathF.Cos(an) * u * .12f, yy - MathF.Sin(an) * u * .06f, x + MathF.Sin(an) * u * .06f, yy - MathF.Cos(an) * u * .12f); }); } }

[VideoEffect("重ね模様 泡", ["演出：質感・画風"], ["重ね模様 泡", "泡がのぼっていく模様を重ねる", "質感・画風", "重ね模様", "泡", "模様", "パターン", "柄", "テクスチャ", "水中", "ソーダ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V580Effect : FxPat { public override string Label => "重ね模様 泡"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => Ring(dc, b, x + MathF.Sin(i + o * .05f) * u * .1f, y - (o * 2f + i * 11f) % u, u * (.08f + (i % 3) * .05f), 1.8f)); } }

[VideoEffect("重ね模様 ドット大小", ["演出：質感・画風"], ["重ね模様 ドット大小", "大きさのちがう水玉を重ねる", "質感・画風", "重ね模様", "ドット大小", "模様", "パターン", "柄", "テクスチャ", "水玉", "ランダム", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V581Effect : FxPat { public override string Label => "重ね模様 ドット大小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.2f; Each(l, t, r, bo, u, (x, y, i) => Dot(dc, b, x + o % u, y, u * (.08f + (i % 4) * .05f))); } }

[VideoEffect("重ね模様 十字星", ["演出：質感・画風"], ["重ね模様 十字星", "またたく十字の星を重ねる", "質感・画風", "重ね模様", "十字星", "模様", "パターン", "柄", "テクスチャ", "きらめき", "輝き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V582Effect : FxPat { public override string Label => "重ね模様 十字星"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => { float s = u * (.12f + .06f * MathF.Sin(o * .1f + i)); Poly(dc, b, x, y - s * 2f, x + s * .3f, y - s * .3f, x + s * 2f, y, x + s * .3f, y + s * .3f, x, y + s * 2f, x - s * .3f, y + s * .3f, x - s * 2f, y, x - s * .3f, y - s * .3f); }); } }

[VideoEffect("重ね模様 レンガ", ["演出：質感・画風"], ["重ね模様 レンガ", "レンガの目地を重ねる", "質感・画風", "重ね模様", "レンガ", "模様", "パターン", "柄", "テクスチャ", "壁", "れんが", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V583Effect : FxPat { public override string Label => "重ね模様 レンガ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { int row = 0; for (float y = t - st; y <= bo; y += st * .5f, row++) { Line(dc, b, l, y, r, y, 2f); for (float x = l - st + (row % 2) * st * .5f + o % st; x <= r; x += st) Line(dc, b, x, y, x, y + st * .5f, 2f); } } }

[VideoEffect("重ね模様 三日月", ["演出：質感・画風"], ["重ね模様 三日月", "三日月の模様を重ねる", "質感・画風", "重ね模様", "三日月", "模様", "パターン", "柄", "テクスチャ", "月", "夜", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V584Effect : FxPat { public override string Label => "重ね模様 三日月"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => Poly(dc, b, P5.Moon(x + o % u, y, u * .2f))); } }

[VideoEffect("重ね模様 ギンガム", ["演出：質感・画風"], ["重ね模様 ギンガム", "ギンガムチェックを重ねる", "質感・画風", "重ね模様", "ギンガム", "模様", "パターン", "柄", "テクスチャ", "チェック", "布", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V585Effect : FxPat { public override string Label => "重ね模様 ギンガム"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { b.Color = new Vortice.Mathematics.Color4(b.Color.R, b.Color.G, b.Color.B, b.Color.A * .6f); for (float x = l - st + o % st; x <= r; x += st) dc.FillRectangle(new Vortice.RawRectF(x, t, x + st * .5f, bo), b); for (float y = t - st; y <= bo; y += st) dc.FillRectangle(new Vortice.RawRectF(l, y, r, y + st * .5f), b); } }

[VideoEffect("ぼかし 脈打つ", ["演出：ゆらぎ・ぼかし"], ["ぼかし 脈打つ", "ぼけたりはっきりしたりをくり返す", "ゆらぎ・ぼかし", "ぼかし", "脈打つ", "ゆらぎ", "ゆがみ", "ブラー", "ピンぼけ", "脈", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V586Effect : FxVA { public override string Label => "ぼかし 脈打つ"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Blur("b", i, P(f) * Z(f) * 24f * (.5f + .5f * MathF.Sin(f.T * Sp(f) * 4f))); } }

[VideoEffect("ブレ 横", ["演出：ゆらぎ・ぼかし"], ["ブレ 横", "横方向にブレさせる", "ゆらぎ・ぼかし", "ブレ", "横", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "スピード", "流れる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V587Effect : FxVA { public override string Label => "ブレ 横"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f, 0f); } }

[VideoEffect("ブレ 縦", ["演出：ゆらぎ・ぼかし"], ["ブレ 縦", "縦方向にブレさせる", "ゆらぎ・ぼかし", "ブレ", "縦", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "落ちる", "流れる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V588Effect : FxVA { public override string Label => "ブレ 縦"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f, 90f); } }

[VideoEffect("ブレ ななめ", ["演出：ゆらぎ・ぼかし"], ["ブレ ななめ", "ななめ方向にブレさせる", "ゆらぎ・ぼかし", "ブレ", "ななめ", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "スピード", "勢い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V589Effect : FxVA { public override string Label => "ブレ ななめ"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f, 45f); } }

[VideoEffect("ブレ ゆれる", ["演出：ゆらぎ・ぼかし"], ["ブレ ゆれる", "横ブレが強くなったり弱くなったりする", "ゆらぎ・ぼかし", "ブレ", "ゆれる", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "めまい", "揺れ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V590Effect : FxVA { public override string Label => "ブレ ゆれる"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f * MathF.Abs(MathF.Sin(f.T * Sp(f) * 3f)), 0f); } }

[VideoEffect("かげろう", ["演出：ゆらぎ・ぼかし"], ["かげろう", "熱気でゆらめくように絵をゆがめる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "熱", "陽炎", "暑い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V591Effect : FxVA { public override string Label => "かげろう"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new System.Numerics.Vector2(0, f.T * Sp(f) * 60f), .02f, 2, 3), P(f) * Z(f) * 30f); } }

[VideoEffect("水面ゆらぎ", ["演出：ゆらぎ・ぼかし"], ["水面ゆらぎ", "水面に映ったように大きくゆらめく", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "水", "反射", "プール", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V592Effect : FxVA { public override string Label => "水面ゆらぎ"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new System.Numerics.Vector2(f.T * Sp(f) * 30f, 0), .008f, 1, 7), P(f) * Z(f) * 50f); } }

[VideoEffect("ガラス越し", ["演出：ゆらぎ・ぼかし"], ["ガラス越し", "でこぼこのガラス越しに見たようにゆがめる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "すりガラス", "窓", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V593Effect : FxVA { public override string Label => "ガラス越し"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", System.Numerics.Vector2.Zero, .05f, 2, 11), P(f) * Z(f) * 40f); } }

[VideoEffect("ふんわり光", ["演出：ゆらぎ・ぼかし"], ["ふんわり光", "ぼかした光を足してふんわり光らせる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "グロー", "発光", "やわらか", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V594Effect : FxVA { public override string Label => "ふんわり光"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Comp("c", i, k.Matrix("m", k.Blur("b", i, 6f + Z(f) * 30f), FxKit.Tone(0, 1, 1, Math.Min(P(f), 1f) * .7f)), CompositeMode.Plus); } }

[VideoEffect("ソフトフォーカス", ["演出：ゆらぎ・ぼかし"], ["ソフトフォーカス", "ぼかしを薄く重ねてやわらかく見せる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "美肌", "やわらか", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V595Effect : FxVA { public override string Label => "ソフトフォーカス"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Comp("c", i, k.Matrix("m", k.Blur("b", i, 4f + Z(f) * 20f), FxKit.Tone(0, 1, 1, Math.Min(P(f), 1f) * .55f))); } }

[VideoEffect("二重写し", ["演出：ゆらぎ・ぼかし"], ["二重写し", "ずれた絵を薄く重ねて二重に見せる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "ダブル", "めまい", "残像", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V596Effect : FxVA { public override string Label => "二重写し"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float d = MathF.Sin(f.T * Sp(f) * 2f) * Z(f) * f.W * .08f; return k.Comp("c", i, k.Matrix("m", k.Affine("a", i, Mv.Move(d, 0)), FxKit.Tone(0, 1, 1, Math.Min(P(f), 1f) * .45f))); } }

[VideoEffect("三重写し", ["演出：ゆらぎ・ぼかし"], ["三重写し", "左右にずれた絵を重ねて三重に見せる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "分身", "残像", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V597Effect : FxVA { public override string Label => "三重写し"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float d = Z(f) * f.W * .05f * (1f + .3f * MathF.Sin(f.T * Sp(f) * 2f)); var g = FxKit.Tone(0, 1, 1, Math.Min(P(f), 1f) * .35f); var o = k.Comp("c1", k.Matrix("m1", k.Affine("a1", i, Mv.Move(-d, 0)), g), k.Matrix("m2", k.Affine("a2", i, Mv.Move(d, 0)), g)); return k.Comp("c2", i, o); } }

[VideoEffect("ズームブレ", ["演出：ゆらぎ・ぼかし"], ["ズームブレ", "中心から広がるようにブレさせる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "衝撃", "ズーム", "集中", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V598Effect : FxVA { public override string Label => "ズームブレ"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = 1f + Z(f) * .12f * Math.Min(P(f), 1f); var g = FxKit.Tone(0, 1, 1, .3f); var o = k.Comp("c1", k.Matrix("m1", k.Affine("a1", i, Mv.Scale(f.C, s, s)), g), k.Matrix("m2", k.Affine("a2", i, Mv.Scale(f.C, 1f + (s - 1f) * .5f, 1f + (s - 1f) * .5f)), g)); return k.Comp("c2", i, o); } }

[VideoEffect("ゆらゆら波", ["演出：ゆらぎ・ぼかし"], ["ゆらゆら波", "大きな波のようにゆっくりゆがむ", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "波", "夢", "回想", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V599Effect : FxVA { public override string Label => "ゆらゆら波"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new System.Numerics.Vector2(0, f.T * Sp(f) * 120f), .004f, 1, 5), P(f) * Z(f) * 60f); } }

[VideoEffect("ノイズゆがみ", ["演出：ゆらぎ・ぼかし"], ["ノイズゆがみ", "ざらざらとカクカク震えるようにゆがむ", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "不調", "グリッチ", "乱れ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx5V600Effect : FxVA { public override string Label => "ノイズゆがみ"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new System.Numerics.Vector2(MathF.Floor(f.T * Sp(f) * 12f) * 37f, 0), .03f, 3, 13), P(f) * Z(f) * 35f); } }
