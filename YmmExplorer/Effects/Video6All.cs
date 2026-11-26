using System.Numerics;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 601〜700(gen_v136.py で生成)

[VideoEffect("フレーム 四角", ["演出：フレーム"], ["フレーム 四角", "まわりに四角い枠を付ける", "フレーム", "四角", "枠", "ふち", "囲み", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V601Effect : FxBorder { public override string Label => "フレーム 四角"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Box(dc, b, l, t, r, bo, w); } }

[VideoEffect("フレーム 角丸", ["演出：フレーム"], ["フレーム 角丸", "角の丸い枠を付ける", "フレーム", "角丸", "枠", "ふち", "囲み", "やわらか", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V602Effect : FxBorder { public override string Label => "フレーム 角丸"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { dc.DrawRoundedRectangle(new RoundedRectangle(new Vortice.RawRectF(l + w / 2f, t + w / 2f, r - w / 2f, bo - w / 2f), w * 3f, w * 3f), b, w); } }

[VideoEffect("フレーム 二重", ["演出：フレーム"], ["フレーム 二重", "二重の線の枠を付ける", "フレーム", "二重", "枠", "ふち", "囲み", "高級", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V603Effect : FxBorder { public override string Label => "フレーム 二重"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Box(dc, b, l, t, r, bo, w * .6f); Box(dc, b, l + w * 1.8f, t + w * 1.8f, r - w * 1.8f, bo - w * 1.8f, w * .4f); } }

[VideoEffect("フレーム 点線", ["演出：フレーム"], ["フレーム 点線", "点線の枠を付ける", "フレーム", "点線", "枠", "ふち", "囲み", "ミシン目", "破線", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V604Effect : FxBorder { public override string Label => "フレーム 点線"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Dash(dc, b, l + w / 2f, t + w / 2f, r - w / 2f, bo - w / 2f, w, 0f); } }

[VideoEffect("フレーム 四隅", ["演出：フレーム"], ["フレーム 四隅", "四隅にカギの形の印を付ける", "フレーム", "四隅", "枠", "ふち", "囲み", "ファインダー", "カメラ", "注目", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V605Effect : FxBorder { public override string Label => "フレーム 四隅"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { float s = MathF.Min(r - l, bo - t) * .18f; foreach (var (x, y, dx, dy) in new[] { (l, t, 1f, 1f), (r, t, -1f, 1f), (l, bo, 1f, -1f), (r, bo, -1f, -1f) }) { float xx = x + dx * w / 2f, yy = y + dy * w / 2f; L(dc, b, xx, yy, xx + dx * s, yy, w); L(dc, b, xx, yy, xx, yy + dy * s, w); } } }

[VideoEffect("フレーム 下線", ["演出：フレーム"], ["フレーム 下線", "下に太い線を引く", "フレーム", "下線", "枠", "ふち", "囲み", "アンダーライン", "強調", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V606Effect : FxBorder { public override string Label => "フレーム 下線"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { dc.FillRectangle(new Vortice.RawRectF(l, bo - w * 1.6f, r, bo), b); } }

[VideoEffect("フレーム 映画の帯", ["演出：フレーム"], ["フレーム 映画の帯", "上下に映画のような帯を付ける", "フレーム", "映画の帯", "枠", "ふち", "囲み", "シネマ", "映画", "レターボックス", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V607Effect : FxBorder { public override string Label => "フレーム 映画の帯"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { float h = (bo - t) * .12f; dc.FillRectangle(new Vortice.RawRectF(l, t, r, t + h), b); dc.FillRectangle(new Vortice.RawRectF(l, bo - h, r, bo), b); } }

[VideoEffect("フレーム フィルムの穴", ["演出：フレーム"], ["フレーム フィルムの穴", "上下にフィルムの穴が流れる帯を付ける", "フレーム", "フィルムの穴", "枠", "ふち", "囲み", "フィルム", "映写", "レトロ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V608Effect : FxBorder { public override string Label => "フレーム フィルムの穴"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { float h = MathF.Max(w * 3f, (bo - t) * .07f); dc.FillRectangle(new Vortice.RawRectF(l, t, r, t + h), b); dc.FillRectangle(new Vortice.RawRectF(l, bo - h, r, bo), b); b.Color = new Vortice.Mathematics.Color4(0, 0, 0, b.Color.A); for (float x = l + h * .5f - (o * 40f) % (h * 1.5f); x < r; x += h * 1.5f) { dc.FillRectangle(new Vortice.RawRectF(x, t + h * .25f, x + h * .6f, t + h * .75f), b); dc.FillRectangle(new Vortice.RawRectF(x, bo - h * .75f, x + h * .6f, bo - h * .25f), b); } } }

[VideoEffect("フレーム 白ふち", ["演出：フレーム"], ["フレーム 白ふち", "写真のような太いふちを付ける(下が広い)", "フレーム", "白ふち", "枠", "ふち", "囲み", "写真", "ポラ", "チェキ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V609Effect : FxBorder { public override string Label => "フレーム 白ふち"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { float s = w * 2.5f; dc.FillRectangle(new Vortice.RawRectF(l, t, r, t + s), b); dc.FillRectangle(new Vortice.RawRectF(l, bo - s * 2.5f, r, bo), b); dc.FillRectangle(new Vortice.RawRectF(l, t, l + s, bo), b); dc.FillRectangle(new Vortice.RawRectF(r - s, t, r, bo), b); } }

[VideoEffect("フレーム 波", ["演出：フレーム"], ["フレーム 波", "波打つ線の枠を付ける", "フレーム", "波", "枠", "ふち", "囲み", "なみなみ", "かわいい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V610Effect : FxBorder { public override string Label => "フレーム 波"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Wave(dc, b, l, t, r, bo, w, o, false); } }

[VideoEffect("フレーム ジグザグ", ["演出：フレーム"], ["フレーム ジグザグ", "ジグザグの線の枠を付ける", "フレーム", "ジグザグ", "枠", "ふち", "囲み", "ギザギザ", "ポップ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V611Effect : FxBorder { public override string Label => "フレーム ジグザグ"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Wave(dc, b, l, t, r, bo, w, o, true); } }

[VideoEffect("フレーム だ円", ["演出：フレーム"], ["フレーム だ円", "だ円の枠を付ける", "フレーム", "だ円", "枠", "ふち", "囲み", "丸", "楕円", "鏡", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V612Effect : FxBorder { public override string Label => "フレーム だ円"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { dc.DrawEllipse(new Ellipse(new Vector2((l + r) / 2f, (t + bo) / 2f), (r - l) / 2f - w, (bo - t) / 2f - w), b, w); } }

[VideoEffect("フレーム 星", ["演出：フレーム"], ["フレーム 星", "まわりに星を並べる", "フレーム", "星", "枠", "ふち", "囲み", "スター", "キラキラ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V613Effect : FxBorder { public override string Label => "フレーム 星"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Along(l, t, r, bo, w * 5f, o * 20f, (x, y, k) => Poly(dc, b, P5.Star(x, y, w * 1.6f))); } }

[VideoEffect("フレーム ハート", ["演出：フレーム"], ["フレーム ハート", "まわりにハートを並べる", "フレーム", "ハート", "枠", "ふち", "囲み", "恋", "かわいい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V614Effect : FxBorder { public override string Label => "フレーム ハート"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Along(l, t, r, bo, w * 5f, o * 20f, (x, y, k) => Poly(dc, b, P5.Heart(x, y, w * 1.5f))); } }

[VideoEffect("フレーム 点滅", ["演出：フレーム"], ["フレーム 点滅", "明るさが点滅する枠を付ける", "フレーム", "点滅", "枠", "ふち", "囲み", "光る", "注目", "警告", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V615Effect : FxBorder { public override string Label => "フレーム 点滅"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { b.Color = new Vortice.Mathematics.Color4(b.Color.R, b.Color.G, b.Color.B, b.Color.A * (.35f + .65f * (.5f + .5f * MathF.Sin(o * 6f)))); Box(dc, b, l, t, r, bo, w * 1.3f); } }

[VideoEffect("フレーム 回る点", ["演出：フレーム"], ["フレーム 回る点", "枠の上を光の点が回る", "フレーム", "回る点", "枠", "ふち", "囲み", "ローディング", "走る", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V616Effect : FxBorder { public override string Label => "フレーム 回る点"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Box(dc, b, l, t, r, bo, w * .4f); float per = 2f * ((r - l) + (bo - t)); var p = OnEdge(l, t, r, bo, (o * 200f) % per); dc.FillEllipse(new Ellipse(p, w * 1.6f, w * 1.6f), b); var q = OnEdge(l, t, r, bo, (o * 200f + per / 2f) % per); dc.FillEllipse(new Ellipse(q, w * 1.6f, w * 1.6f), b); } }

[VideoEffect("フレーム 走る点線", ["演出：フレーム"], ["フレーム 走る点線", "点線が枠に沿って流れる", "フレーム", "走る点線", "枠", "ふち", "囲み", "選択", "動く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V617Effect : FxBorder { public override string Label => "フレーム 走る点線"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Dash(dc, b, l + w / 2f, t + w / 2f, r - w / 2f, bo - w / 2f, w, o * 60f); } }

[VideoEffect("フレーム 四隅の丸", ["演出：フレーム"], ["フレーム 四隅の丸", "四隅に丸の飾りが付いた細い枠", "フレーム", "四隅の丸", "枠", "ふち", "囲み", "飾り", "証書", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V618Effect : FxBorder { public override string Label => "フレーム 四隅の丸"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { float s = w * 3f; foreach (var (x, y) in new[] { (l + s, t + s), (r - s, t + s), (l + s, bo - s), (r - s, bo - s) }) dc.FillEllipse(new Ellipse(new Vector2(x, y), s * .8f, s * .8f), b); Box(dc, b, l + s, t + s, r - s, bo - s, w * .3f); } }

[VideoEffect("フレーム 窓わく", ["演出：フレーム"], ["フレーム 窓わく", "十字の入った窓わくを重ねる", "フレーム", "窓わく", "枠", "ふち", "囲み", "窓", "家", "外", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V619Effect : FxBorder { public override string Label => "フレーム 窓わく"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { Box(dc, b, l, t, r, bo, w); L(dc, b, (l + r) / 2f, t, (l + r) / 2f, bo, w * .7f); L(dc, b, l, (t + bo) / 2f, r, (t + bo) / 2f, w * .7f); } }

[VideoEffect("フレーム ななめの角", ["演出：フレーム"], ["フレーム ななめの角", "左上と右下にななめの三角を付ける", "フレーム", "ななめの角", "枠", "ふち", "囲み", "スタイリッシュ", "ゲーム", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V620Effect : FxBorder { public override string Label => "フレーム ななめの角"; private protected override void Fr(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float w, float o) { float s = MathF.Min(r - l, bo - t) * .2f; Poly(dc, b, l, t, l + s, t, l, t + s); Poly(dc, b, r, bo, r - s, bo, r, bo - s); Box(dc, b, l, t, r, bo, w * .4f); } }

[VideoEffect("動き3 波乗り", ["演出：動き"], ["動き3 波乗り", "波に乗るように上下しながら傾く", "動き", "動き3", "波乗り", "モーション", "ループ", "サーフィン", "波", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V621Effect : FxMove { public override string Label => "動き3 波乗り"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * 1.5f) * .04f * a, S(t * 3f) * .03f * a) * Ro(f, C(t * 1.5f) * .08f * a); }

[VideoEffect("動き3 ぶるん", ["演出：動き"], ["動き3 ぶるん", "ときどきぶるんと縦横に震える", "動き", "動き3", "ぶるん", "モーション", "ループ", "ゼリー", "ぷるん", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V622Effect : FxMove { public override string Label => "動き3 ぶるん"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f, 1f + S(t * 9f) * .04f * a * MathF.Max(0f, S(t * 1.5f)), 1f - S(t * 9f) * .04f * a * MathF.Max(0f, S(t * 1.5f))); }

[VideoEffect("動き3 傾いて戻る", ["演出：動き"], ["動き3 傾いて戻る", "右に傾いては戻るのをくり返す", "動き", "動き3", "傾いて戻る", "モーション", "ループ", "かしげる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V623Effect : FxMove { public override string Label => "動き3 傾いて戻る"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => RoF(f, MathF.Max(0f, S(t * 2f)) * .15f * a); }

[VideoEffect("動き3 前後に揺れる", ["演出：動き"], ["動き3 前後に揺れる", "手前と奥に揺れるように大きさが変わる", "動き", "動き3", "前後に揺れる", "モーション", "ループ", "奥行き", "揺れ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V624Effect : FxMove { public override string Label => "動き3 前後に揺れる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f, 1f + S(t * 2f) * .06f * a, 1f + S(t * 2f) * .06f * a) * Mo(f, 0, S(t * 2f) * .02f * a); }

[VideoEffect("動き3 こっくり", ["演出：動き"], ["動き3 こっくり", "居眠りのようにこっくりする", "動き", "動き3", "こっくり", "モーション", "ループ", "眠い", "居眠り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V625Effect : FxMove { public override string Label => "動き3 こっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => RoF(f, -MathF.Pow(MathF.Max(0f, S(t * 1.2f)), 4f) * .25f * a) * Mo(f, 0, MathF.Pow(MathF.Max(0f, S(t * 1.2f)), 4f) * .02f * a); }

[VideoEffect("動き3 ガッツポーズ", ["演出：動き"], ["動き3 ガッツポーズ", "喜んで跳び上がる", "動き", "動き3", "ガッツポーズ", "モーション", "ループ", "喜ぶ", "成功", "やった", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V626Effect : FxMove { public override string Label => "動き3 ガッツポーズ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, 0, -MathF.Pow(MathF.Max(0f, S(t * 3f)), 3f) * .08f * a) * Sc(f, 1f + MathF.Pow(MathF.Max(0f, S(t * 3f)), 3f) * .05f * a, 1f + MathF.Pow(MathF.Max(0f, S(t * 3f)), 3f) * .05f * a); }

[VideoEffect("動き3 しょんぼり", ["演出：動き"], ["動き3 しょんぼり", "しょんぼり沈むように縮む", "動き", "動き3", "しょんぼり", "モーション", "ループ", "落ち込む", "悲しい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V627Effect : FxMove { public override string Label => "動き3 しょんぼり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f, 1f, 1f - (.5f + .5f * S(t * .8f)) * .06f * a) * Mo(f, 0, (.5f + .5f * S(t * .8f)) * .03f * a); }

[VideoEffect("動き3 ガクガク", ["演出：動き"], ["動き3 ガクガク", "カクカクと小刻みに震える", "動き", "動き3", "ガクガク", "モーション", "ループ", "震え", "怖い", "寒い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V628Effect : FxMove { public override string Label => "動き3 ガクガク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, (MathF.Floor(t * 12f) % 2f - .5f) * .02f * a, (MathF.Floor(t * 9f) % 2f - .5f) * .015f * a); }

[VideoEffect("動き3 ゆっくり往復回転", ["演出：動き"], ["動き3 ゆっくり往復回転", "ゆっくり右と左に回る", "動き", "動き3", "ゆっくり往復回転", "モーション", "ループ", "回転", "振り子", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V629Effect : FxMove { public override string Label => "動き3 ゆっくり往復回転"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, S(t * .7f) * .35f * a); }

[VideoEffect("動き3 三角を描く", ["演出：動き"], ["動き3 三角を描く", "三角形の頂点をめぐるように動く", "動き", "動き3", "三角を描く", "モーション", "ループ", "ワープ", "移動", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V630Effect : FxMove { public override string Label => "動き3 三角を描く"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, (MathF.Floor(t * 1.5f) % 3f == 0 ? 0f : MathF.Floor(t * 1.5f) % 3f == 1 ? .05f : -.05f) * a, (MathF.Floor(t * 1.5f) % 3f == 0 ? -.04f : .03f) * a); }

[VideoEffect("動き3 四角を描く", ["演出：動き"], ["動き3 四角を描く", "四角形の角をめぐるように動く", "動き", "動き3", "四角を描く", "モーション", "ループ", "ワープ", "移動", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V631Effect : FxMove { public override string Label => "動き3 四角を描く"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, ((int)(t * 2f) % 4 is 0 or 3 ? -.04f : .04f) * a, ((int)(t * 2f) % 4 < 2 ? -.04f : .04f) * a); }

[VideoEffect("動き3 風船", ["演出：動き"], ["動き3 風船", "風船のようにふわふわ揺れる", "動き", "動き3", "風船", "モーション", "ループ", "浮く", "ふわふわ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V632Effect : FxMove { public override string Label => "動き3 風船"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * .9f) * .03f * a, -.02f * a + S(t * 1.3f) * .025f * a) * Ro(f, S(t * .9f) * .05f * a); }

[VideoEffect("動き3 ジャンプ連続", ["演出：動き"], ["動き3 ジャンプ連続", "つぶれながら連続でジャンプする", "動き", "動き3", "ジャンプ連続", "モーション", "ループ", "弾む", "元気", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V633Effect : FxMove { public override string Label => "動き3 ジャンプ連続"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, 0, -MathF.Abs(S(t * 5f)) * .05f * a) * Ft(f, 1f + (1f - MathF.Abs(S(t * 5f))) * .04f * a, 1f - (1f - MathF.Abs(S(t * 5f))) * .06f * a); }

[VideoEffect("動き3 寝返り", ["演出：動き"], ["動き3 寝返り", "ゆっくり大きく左右に転がる", "動き", "動き3", "寝返り", "モーション", "ループ", "回転", "ごろごろ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V634Effect : FxMove { public override string Label => "動き3 寝返り"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, S(t * .6f) * 1.4f * a) * Sc(f, .8f, .8f); }

[VideoEffect("動き3 左右に流れる", ["演出：動き"], ["動き3 左右に流れる", "右へ流れては戻るのをくり返す", "動き", "動き3", "左右に流れる", "モーション", "ループ", "スクロール", "流れる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V635Effect : FxMove { public override string Label => "動き3 左右に流れる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, ((t * .15f) % 1f - .5f) * .3f * a, 0); }

[VideoEffect("動き3 ぐるぐる拡大", ["演出：動き"], ["動き3 ぐるぐる拡大", "回りながら大きさが変わる", "動き", "動き3", "ぐるぐる拡大", "モーション", "ループ", "回転", "めまい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V636Effect : FxMove { public override string Label => "動き3 ぐるぐる拡大"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, t * 1.5f) * Sc(f, 1f + S(t * 1.5f) * .1f * a, 1f + S(t * 1.5f) * .1f * a); }

[VideoEffect("動き3 うなずき", ["演出：動き"], ["動き3 うなずき", "小さくうなずくように縮む", "動き", "動き3", "うなずき", "モーション", "ループ", "はい", "同意", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V637Effect : FxMove { public override string Label => "動き3 うなずき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f, 0) * Ft(f, 1f, 1f - MathF.Max(0f, S(t * 6f)) * .05f * a) * Mo(f, 0, MathF.Max(0f, S(t * 6f)) * .01f * a); }

[VideoEffect("動き3 いやいや", ["演出：動き"], ["動き3 いやいや", "ときどき首を振るように左右に震える", "動き", "動き3", "いやいや", "モーション", "ループ", "いいえ", "拒否", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V638Effect : FxMove { public override string Label => "動き3 いやいや"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * 10f) * .015f * a * MathF.Max(0f, S(t * 1.2f)), 0); }

[VideoEffect("動き3 ふり子 大きく", ["演出：動き"], ["動き3 ふり子 大きく", "上の遠くを支点に大きく揺れる", "動き", "動き3", "ふり子", "大きく", "モーション", "ループ", "ぶらんこ", "揺れる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V639Effect : FxMove { public override string Label => "動き3 ふり子 大きく"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mv.Rot(new Vector2(f.C.X, f.B.Top - f.H * .5f), S(t * 2f) * .25f * a); }

[VideoEffect("動き3 ドリフト", ["演出：動き"], ["動き3 ドリフト", "横に滑りながら傾く", "動き", "動き3", "ドリフト", "モーション", "ループ", "車", "スピード", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V640Effect : FxMove { public override string Label => "動き3 ドリフト"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * 1.1f) * .06f * a, 0) * Sk(f, -C(t * 1.1f) * .1f * a); }

[VideoEffect("出入り3 下から跳ねる", ["演出：出入り"], ["出入り3 下から跳ねる", "下から跳ねながら出る", "出入り", "出入り3", "下から跳ねる", "登場", "退場", "トランジション", "弾む", "ジャンプ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V641Effect : FxEnter { public override string Label => "出入り3 下から跳ねる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, 0, .6f * d) * Sc(f, 1f + MathF.Sin(d * 8f) * d * .3f, 1f - MathF.Sin(d * 8f) * d * .3f); }

[VideoEffect("出入り3 右上から", ["演出：出入り"], ["出入り3 右上から", "右上から滑りこんで出る", "出入り", "出入り3", "右上から", "登場", "退場", "トランジション", "スライド", "ななめ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V642Effect : FxEnter { public override string Label => "出入り3 右上から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, .4f * d * dir, -.4f * d); }

[VideoEffect("出入り3 左下から", ["演出：出入り"], ["出入り3 左下から", "左下から滑りこんで出る", "出入り", "出入り3", "左下から", "登場", "退場", "トランジション", "スライド", "ななめ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V643Effect : FxEnter { public override string Label => "出入り3 左下から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, -.4f * d * dir, .4f * d); }

[VideoEffect("出入り3 大回り", ["演出：出入り"], ["出入り3 大回り", "大きな円を描いて出る", "出入り", "出入り3", "大回り", "登場", "退場", "トランジション", "回転", "弧", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V644Effect : FxEnter { public override string Label => "出入り3 大回り"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mv.Rot(new Vector2(f.C.X, f.C.Y + f.H), -d * 1.2f * dir); }

[VideoEffect("出入り3 逆回転ズーム", ["演出：出入り"], ["出入り3 逆回転ズーム", "大きく回りながら縮んで出る", "出入り", "出入り3", "逆回転ズーム", "登場", "退場", "トランジション", "回転", "迫力", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V645Effect : FxEnter { public override string Label => "出入り3 逆回転ズーム"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f + d * 3f, 1f + d * 3f) * Ro(f, -d * 2f * dir); }

[VideoEffect("出入り3 二回転", ["演出：出入り"], ["出入り3 二回転", "2回まわりながら出る", "出入り", "出入り3", "二回転", "登場", "退場", "トランジション", "スピン", "くるくる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V646Effect : FxEnter { public override string Label => "出入り3 二回転"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, d * MathF.Tau * 2f * dir) * Sc(f, 1f - d, 1f - d); }

[VideoEffect("出入り3 縦につぶれて", ["演出：出入り"], ["出入り3 縦につぶれて", "縦につぶれた形から出る", "出入り", "出入り3", "縦につぶれて", "登場", "退場", "トランジション", "ぺちゃんこ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V647Effect : FxEnter { public override string Label => "出入り3 縦につぶれて"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f + d * .5f, 1f - d); }

[VideoEffect("出入り3 横につぶれて", ["演出：出入り"], ["出入り3 横につぶれて", "横につぶれた形から出る", "出入り", "出入り3", "横につぶれて", "登場", "退場", "トランジション", "ぺちゃんこ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V648Effect : FxEnter { public override string Label => "出入り3 横につぶれて"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f - d, 1f + d * .5f); }

[VideoEffect("出入り3 ぽよん左", ["演出：出入り"], ["出入り3 ぽよん左", "左からぽよんと弾んで出る", "出入り", "出入り3", "ぽよん左", "登場", "退場", "トランジション", "ゼリー", "弾む", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V649Effect : FxEnter { public override string Label => "出入り3 ぽよん左"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, -.5f * d * dir, 0) * Sc(f, 1f + MathF.Sin(d * 10f) * d * .2f, 1f - MathF.Sin(d * 10f) * d * .2f); }

[VideoEffect("出入り3 ななめ回転", ["演出：出入り"], ["出入り3 ななめ回転", "回りながら左上から出る", "出入り", "出入り3", "ななめ回転", "登場", "退場", "トランジション", "回転", "スライド", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V650Effect : FxEnter { public override string Label => "出入り3 ななめ回転"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, d * 1.5f * dir) * Mo(f, -.3f * d * dir, -.3f * d); }

[VideoEffect("出入り3 上から伸びる", ["演出：出入り"], ["出入り3 上から伸びる", "上から下へ伸びて出る", "出入り", "出入り3", "上から伸びる", "登場", "退場", "トランジション", "のれん", "伸びる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V651Effect : FxEnter { public override string Label => "出入り3 上から伸びる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mv.Scale(new Vector2(f.C.X, f.B.Top), 1f, MathF.Max(.012f, 1f - d)); }

[VideoEffect("出入り3 ふわっと上へ", ["演出：出入り"], ["出入り3 ふわっと上へ", "下からふわっと浮かんで出る", "出入り", "出入り3", "ふわっと上へ", "登場", "退場", "トランジション", "やさしい", "浮く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V652Effect : FxEnter { public override string Label => "出入り3 ふわっと上へ"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, 0, .15f * d) * Sc(f, 1f - d * .2f, 1f - d * .2f); }

[VideoEffect("出入り3 しゅっと右へ", ["演出：出入り"], ["出入り3 しゅっと右へ", "しゅっと勢いよく右から出る", "出入り", "出入り3", "しゅっと右へ", "登場", "退場", "トランジション", "スピード", "勢い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V653Effect : FxEnter { public override string Label => "出入り3 しゅっと右へ"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, .8f * d * dir, 0) * Sk(f, -d * .6f * dir); }

[VideoEffect("出入り3 ぷるぷる", ["演出：出入り"], ["出入り3 ぷるぷる", "細かく震えながら出る", "出入り", "出入り3", "ぷるぷる", "登場", "退場", "トランジション", "震える", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V654Effect : FxEnter { public override string Label => "出入り3 ぷるぷる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, MathF.Sin(d * 40f) * .03f * d, 0) * Sc(f, 1f - d * .4f, 1f - d * .4f); }

[VideoEffect("出入り3 ジャンプ", ["演出：出入り"], ["出入り3 ジャンプ", "ジャンプして飛びこんで出る", "出入り", "出入り3", "ジャンプ", "登場", "退場", "トランジション", "跳ぶ", "元気", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V655Effect : FxEnter { public override string Label => "出入り3 ジャンプ"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, 0, -MathF.Sin(d * MathF.PI) * .4f - d * .1f); }

[VideoEffect("出入り3 ころん", ["演出：出入り"], ["出入り3 ころん", "ころんと転がるように出る", "出入り", "出入り3", "ころん", "登場", "退場", "トランジション", "転がる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V656Effect : FxEnter { public override string Label => "出入り3 ころん"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => RoF(f, d * 1.6f * dir) * Mo(f, -.2f * d * dir, 0); }

[VideoEffect("出入り3 ひょっこり", ["演出：出入り"], ["出入り3 ひょっこり", "下からひょっこり顔を出す", "出入り", "出入り3", "ひょっこり", "登場", "退場", "トランジション", "のぞく", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V657Effect : FxEnter { public override string Label => "出入り3 ひょっこり"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mv.Move(0, d * f.H * .6f); }

[VideoEffect("出入り3 ぐにゃ回転", ["演出：出入り"], ["出入り3 ぐにゃ回転", "ぐにゃっと傾きながら出る", "出入り", "出入り3", "ぐにゃ回転", "登場", "退場", "トランジション", "ゆがむ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V658Effect : FxEnter { public override string Label => "出入り3 ぐにゃ回転"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, MathF.Sin(d * 6f) * d * .8f) * Sk(f, d * .5f); }

[VideoEffect("出入り3 遠くへ", ["演出：出入り"], ["出入り3 遠くへ", "奥の遠くから近づいて出る", "出入り", "出入り3", "遠くへ", "登場", "退場", "トランジション", "奥行き", "ズーム", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V659Effect : FxEnter { public override string Label => "出入り3 遠くへ"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f - d * .9f, 1f - d * .9f) * Mo(f, 0, -.2f * d); }

[VideoEffect("出入り3 左右から開く", ["演出：出入り"], ["出入り3 左右から開く", "真ん中から左右に開いて出る", "出入り", "出入り3", "左右から開く", "登場", "退場", "トランジション", "開く", "扉", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V660Effect : FxEnter { public override string Label => "出入り3 左右から開く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, MathF.Max(.012f, 1f - d), 1f) * Sk(f, 0); }

[VideoEffect("重ね模様 ハニカム", ["演出：質感・画風"], ["重ね模様 ハニカム", "六角形の蜂の巣の模様を重ねる", "質感・画風", "重ね模様", "ハニカム", "模様", "パターン", "柄", "テクスチャ", "ハチの巣", "六角形", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V661Effect : FxPat { public override string Label => "重ね模様 ハニカム"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => { float s = u * .45f; for (int k = 0; k < 6; k++) { float a0 = k * 1.0472f, a1 = a0 + 1.0472f; Line(dc, b, x + MathF.Cos(a0) * s, y + MathF.Sin(a0) * s, x + MathF.Cos(a1) * s, y + MathF.Sin(a1) * s, 2f); } }); } }

[VideoEffect("重ね模様 青海波", ["演出：質感・画風"], ["重ね模様 青海波", "重なる半円の波の模様を重ねる", "質感・画風", "重ね模様", "青海波", "模様", "パターン", "柄", "テクスチャ", "和柄", "波", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V662Effect : FxPat { public override string Label => "重ね模様 青海波"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.2f; int row = 0; for (float y = t - u; y <= bo + u; y += u * .5f, row++) for (float x = l - u + (row % 2) * u * .5f; x <= r + u; x += u) for (int k = 1; k <= 3; k++) Ring(dc, b, x, y, u * .17f * k, 1.6f); } }

[VideoEffect("重ね模様 うず", ["演出：質感・画風"], ["重ね模様 うず", "うずまきの模様を重ねる", "質感・画風", "重ね模様", "うず", "模様", "パターン", "柄", "テクスチャ", "ぐるぐる", "めまい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V663Effect : FxPat { public override string Label => "重ね模様 うず"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 2f; Each(l, t, r, bo, u, (x, y, i) => { for (int k = 0; k < 24; k++) { float a0 = k * .45f + o * .05f, a1 = a0 + .45f; Line(dc, b, x + MathF.Cos(a0) * k * u * .018f, y + MathF.Sin(a0) * k * u * .018f, x + MathF.Cos(a1) * (k + 1) * u * .018f, y + MathF.Sin(a1) * (k + 1) * u * .018f, 2f); } }); } }

[VideoEffect("重ね模様 チェーン", ["演出：質感・画風"], ["重ね模様 チェーン", "くさりのつながった模様を重ねる", "質感・画風", "重ね模様", "チェーン", "模様", "パターン", "柄", "テクスチャ", "鎖", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V664Effect : FxPat { public override string Label => "重ね模様 チェーン"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.2f; for (float y = t; y <= bo; y += u) for (float x = l - u + o % u; x <= r + u; x += u * .7f) dc.DrawEllipse(new Ellipse(new Vector2(x, y), u * .45f, u * .22f), b, 2.5f); } }

[VideoEffect("重ね模様 いなずま", ["演出：質感・画風"], ["重ね模様 いなずま", "小さな稲妻の模様を重ねる", "質感・画風", "重ね模様", "いなずま", "模様", "パターン", "柄", "テクスチャ", "雷", "電気", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V665Effect : FxPat { public override string Label => "重ね模様 いなずま"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => Poly(dc, b, x + u * .05f, y - u * .35f, x - u * .15f, y + u * .05f, x, y + u * .05f, x - u * .08f, y + u * .35f, x + u * .15f, y - u * .08f, x, y - u * .08f)); } }

[VideoEffect("重ね模様 葉っぱ", ["演出：質感・画風"], ["重ね模様 葉っぱ", "散らばる葉っぱの模様を重ねる", "質感・画風", "重ね模様", "葉っぱ", "模様", "パターン", "柄", "テクスチャ", "木の葉", "秋", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V666Effect : FxPat { public override string Label => "重ね模様 葉っぱ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => { float a = i * .8f, c = MathF.Cos(a), s = MathF.Sin(a); float xx = x + o % u; Poly(dc, b, xx - c * u * .3f, y - s * u * .3f, xx - s * u * .1f, y + c * u * .1f, xx + c * u * .3f, y + s * u * .3f, xx + s * u * .1f, y - c * u * .1f); }); } }

[VideoEffect("重ね模様 さかな", ["演出：質感・画風"], ["重ね模様 さかな", "泳ぐさかなの模様を重ねる", "質感・画風", "重ね模様", "さかな", "模様", "パターン", "柄", "テクスチャ", "魚", "水族館", "海", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V667Effect : FxPat { public override string Label => "重ね模様 さかな"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { float xx = x + (o * 2f) % u; dc.FillEllipse(new Ellipse(new Vector2(xx, y), u * .22f, u * .12f), b); Poly(dc, b, xx - u * .2f, y, xx - u * .35f, y - u * .1f, xx - u * .35f, y + u * .1f); }); } }

[VideoEffect("重ね模様 くも", ["演出：質感・画風"], ["重ね模様 くも", "流れる雲の模様を重ねる", "質感・画風", "重ね模様", "くも", "模様", "パターン", "柄", "テクスチャ", "雲", "空", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V668Effect : FxPat { public override string Label => "重ね模様 くも"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 2f; Each(l, t, r, bo, u, (x, y, i) => { float xx = x + (o * .7f) % u; Dot(dc, b, xx, y, u * .14f); Dot(dc, b, xx - u * .14f, y + u * .05f, u * .1f); Dot(dc, b, xx + u * .15f, y + u * .05f, u * .1f); }); } }

[VideoEffect("重ね模様 太陽", ["演出：質感・画風"], ["重ね模様 太陽", "小さな太陽の模様を重ねる", "質感・画風", "重ね模様", "太陽", "模様", "パターン", "柄", "テクスチャ", "晴れ", "夏", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V669Effect : FxPat { public override string Label => "重ね模様 太陽"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.8f; Each(l, t, r, bo, u, (x, y, i) => { Dot(dc, b, x, y, u * .12f); for (int k = 0; k < 8; k++) { float a = k * .785f + o * .05f; Line(dc, b, x + MathF.Cos(a) * u * .17f, y + MathF.Sin(a) * u * .17f, x + MathF.Cos(a) * u * .27f, y + MathF.Sin(a) * u * .27f, 2f); } }); } }

[VideoEffect("重ね模様 足あと", ["演出：質感・画風"], ["重ね模様 足あと", "動物の足あとの模様を重ねる", "質感・画風", "重ね模様", "足あと", "模様", "パターン", "柄", "テクスチャ", "にくきゅう", "動物", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V670Effect : FxPat { public override string Label => "重ね模様 足あと"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { float yy = y - (o * 2f + i * 7f) % u; dc.FillEllipse(new Ellipse(new Vector2(x, yy), u * .1f, u * .08f), b); for (int k = -1; k <= 2; k++) Dot(dc, b, x - u * .06f + k * u * .045f, yy - u * .12f + MathF.Abs(k - .5f) * u * .02f, u * .03f); }); } }

[VideoEffect("重ね模様 音波", ["演出：質感・画風"], ["重ね模様 音波", "音の波形のような棒を重ねる", "質感・画風", "重ね模様", "音波", "模様", "パターン", "柄", "テクスチャ", "イコライザー", "音楽", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V671Effect : FxPat { public override string Label => "重ね模様 音波"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float x = l; x <= r; x += st * .3f) { float h = (bo - t) * .25f * (.3f + .7f * MathF.Abs(MathF.Sin(x * .05f + o * .2f) * MathF.Sin(x * .013f + o * .07f))); dc.FillRectangle(new Vortice.RawRectF(x, (t + bo) / 2f - h, x + st * .18f, (t + bo) / 2f + h), b); } } }

[VideoEffect("重ね模様 矢印", ["演出：質感・画風"], ["重ね模様 矢印", "右へ進む矢印の模様を重ねる", "質感・画風", "重ね模様", "矢印", "模様", "パターン", "柄", "テクスチャ", "進む", "方向", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V672Effect : FxPat { public override string Label => "重ね模様 矢印"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => { float xx = x + (o * 2f) % u; Poly(dc, b, xx - u * .25f, y - u * .06f, xx + u * .05f, y - u * .06f, xx + u * .05f, y - u * .16f, xx + u * .25f, y, xx + u * .05f, y + u * .16f, xx + u * .05f, y + u * .06f, xx - u * .25f, y + u * .06f); }); } }

[VideoEffect("重ね模様 鍵盤", ["演出：質感・画風"], ["重ね模様 鍵盤", "ピアノの鍵盤の模様を重ねる", "質感・画風", "重ね模様", "鍵盤", "模様", "パターン", "柄", "テクスチャ", "ピアノ", "音楽", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V673Effect : FxPat { public override string Label => "重ね模様 鍵盤"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * .6f; for (float x = l - u * 7f + o % (u * 7f); x <= r; x += u * 7f) foreach (float k in new[] { .7f, 1.7f, 3.7f, 4.7f, 5.7f }) dc.FillRectangle(new Vortice.RawRectF(x + k * u, t, x + k * u + u * .6f, (t + bo) / 2f), b); for (float x = l; x <= r; x += u) Line(dc, b, x, t, x, bo, 1.2f); } }

[VideoEffect("重ね模様 バツ", ["演出：質感・画風"], ["重ね模様 バツ", "小さなバツの模様を重ねる", "質感・画風", "重ね模様", "バツ", "模様", "パターン", "柄", "テクスチャ", "クロス", "×", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V674Effect : FxPat { public override string Label => "重ね模様 バツ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.2f; Each(l, t, r, bo, u, (x, y, i) => { float s = u * .15f; Line(dc, b, x - s, y - s, x + s, y + s, 2.5f); Line(dc, b, x - s, y + s, x + s, y - s, 2.5f); }); } }

[VideoEffect("重ね模様 点描", ["演出：質感・画風"], ["重ね模様 点描", "細かい点を散らした点描の模様を重ねる", "質感・画風", "重ね模様", "点描", "模様", "パターン", "柄", "テクスチャ", "ドット", "絵画", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V675Effect : FxPat { public override string Label => "重ね模様 点描"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * .7f; Each(l, t, r, bo, u, (x, y, i) => Dot(dc, b, x + ((i * 37) % 10 - 5) * u * .06f, y + ((i * 53) % 10 - 5) * u * .06f, u * (.05f + (i % 3) * .03f))); } }

[VideoEffect("重ね模様 木目", ["演出：質感・画風"], ["重ね模様 木目", "木目のような線の模様を重ねる", "質感・画風", "重ね模様", "木目", "模様", "パターン", "柄", "テクスチャ", "木", "ウッド", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V676Effect : FxPat { public override string Label => "重ね模様 木目"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { for (float y = t; y <= bo; y += st * .5f) for (float x = l; x < r; x += 10f) Line(dc, b, x, y + MathF.Sin(x * .01f + y * .03f) * st * .3f, x + 10f, y + MathF.Sin((x + 10f) * .01f + y * .03f) * st * .3f, 1.3f); } }

[VideoEffect("重ね模様 太いななめじま", ["演出：質感・画風"], ["重ね模様 太いななめじま", "太いななめのしまを重ねる", "質感・画風", "重ね模様", "太いななめじま", "模様", "パターン", "柄", "テクスチャ", "ストライプ", "工事", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V677Effect : FxPat { public override string Label => "重ね模様 太いななめじま"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; for (float x = l - (bo - t) - u + o % u; x <= r; x += u) Poly(dc, b, x, bo, x + u * .45f, bo, x + u * .45f + (bo - t), t, x + (bo - t), t); } }

[VideoEffect("重ね模様 星くず", ["演出：質感・画風"], ["重ね模様 星くず", "小さな星と点を散らす", "質感・画風", "重ね模様", "星くず", "模様", "パターン", "柄", "テクスチャ", "夜空", "キラキラ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V678Effect : FxPat { public override string Label => "重ね模様 星くず"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * .9f; Each(l, t, r, bo, u, (x, y, i) => { if (i % 3 == 0) Poly(dc, b, P5.Star(x, y, u * (.08f + .05f * MathF.Abs(MathF.Sin(o * .1f + i))))); else Dot(dc, b, x, y, u * .03f); }); } }

[VideoEffect("重ね模様 宝石", ["演出：質感・画風"], ["重ね模様 宝石", "ダイヤの形の宝石の模様を重ねる", "質感・画風", "重ね模様", "宝石", "模様", "パターン", "柄", "テクスチャ", "ジュエル", "ダイヤ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V679Effect : FxPat { public override string Label => "重ね模様 宝石"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => Poly(dc, b, x - u * .2f, y - u * .05f, x - u * .1f, y - u * .15f, x + u * .1f, y - u * .15f, x + u * .2f, y - u * .05f, x, y + u * .2f)); } }

[VideoEffect("重ね模様 しずく", ["演出：質感・画風"], ["重ね模様 しずく", "しずくが落ちる模様を重ねる", "質感・画風", "重ね模様", "しずく", "模様", "パターン", "柄", "テクスチャ", "雨", "水滴", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V680Effect : FxPat { public override string Label => "重ね模様 しずく"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.3f; Each(l, t, r, bo, u, (x, y, i) => { float yy = y + (o * 3f + i * 9f) % u; Poly(dc, b, x, yy - u * .18f, x + u * .08f, yy, x, yy + u * .08f, x - u * .08f, yy); }); } }

[VideoEffect("ブレ 30度", ["演出：ゆらぎ・ぼかし"], ["ブレ 30度", "30度の向きにブレさせる", "ゆらぎ・ぼかし", "ブレ", "30度", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "スピード", "斜め", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V681Effect : FxVA { public override string Label => "ブレ 30度"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f, 30f); } }

[VideoEffect("ブレ 60度", ["演出：ゆらぎ・ぼかし"], ["ブレ 60度", "60度の向きにブレさせる", "ゆらぎ・ぼかし", "ブレ", "60度", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "スピード", "斜め", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V682Effect : FxVA { public override string Label => "ブレ 60度"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f, 60f); } }

[VideoEffect("ブレ 120度", ["演出：ゆらぎ・ぼかし"], ["ブレ 120度", "120度の向きにブレさせる", "ゆらぎ・ぼかし", "ブレ", "120度", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "スピード", "斜め", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V683Effect : FxVA { public override string Label => "ブレ 120度"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f, 120f); } }

[VideoEffect("ブレ 150度", ["演出：ゆらぎ・ぼかし"], ["ブレ 150度", "150度の向きにブレさせる", "ゆらぎ・ぼかし", "ブレ", "150度", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "スピード", "斜め", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V684Effect : FxVA { public override string Label => "ブレ 150度"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f, 150f); } }

[VideoEffect("ぼかし 呼吸 速い", ["演出：ゆらぎ・ぼかし"], ["ぼかし 呼吸 速い", "速くぼけたり戻ったりする", "ゆらぎ・ぼかし", "ぼかし", "呼吸", "速い", "ゆらぎ", "ゆがみ", "ブラー", "ピンぼけ", "脈", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V685Effect : FxVA { public override string Label => "ぼかし 呼吸 速い"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Blur("b", i, P(f) * Z(f) * 20f * (.5f + .5f * MathF.Sin(f.T * Sp(f) * 10f))); } }

[VideoEffect("ブレ 回る向き", ["演出：ゆらぎ・ぼかし"], ["ブレ 回る向き", "ブレの向きがぐるぐる回る", "ゆらぎ・ぼかし", "ブレ", "回る向き", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "めまい", "混乱", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V686Effect : FxVA { public override string Label => "ブレ 回る向き"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, P(f) * Z(f) * 40f, f.T * Sp(f) * 90f); } }

[VideoEffect("かげろう 強く", ["演出：ゆらぎ・ぼかし"], ["かげろう 強く", "強い熱気で大きくゆらめく", "ゆらぎ・ぼかし", "かげろう", "強く", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "熱", "砂漠", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V687Effect : FxVA { public override string Label => "かげろう 強く"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new Vector2(0, f.T * Sp(f) * 90f), .03f, 2, 21), P(f) * Z(f) * 70f); } }

[VideoEffect("水面ゆらぎ 速い", ["演出：ゆらぎ・ぼかし"], ["水面ゆらぎ 速い", "速くゆらめく水面のようにゆがむ", "ゆらぎ・ぼかし", "水面ゆらぎ", "速い", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "水", "川", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V688Effect : FxVA { public override string Label => "水面ゆらぎ 速い"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new Vector2(f.T * Sp(f) * 90f, 0), .01f, 1, 17), P(f) * Z(f) * 50f); } }

[VideoEffect("ガラス越し 細かい", ["演出：ゆらぎ・ぼかし"], ["ガラス越し 細かい", "細かいでこぼこのガラス越しのようにゆがむ", "ゆらぎ・ぼかし", "ガラス越し", "細かい", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "すりガラス", "窓", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V689Effect : FxVA { public override string Label => "ガラス越し 細かい"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", Vector2.Zero, .12f, 2, 19), P(f) * Z(f) * 30f); } }

[VideoEffect("ふんわり光 強く", ["演出：ゆらぎ・ぼかし"], ["ふんわり光 強く", "強い光でふんわり光らせる", "ゆらぎ・ぼかし", "ふんわり光", "強く", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "グロー", "発光", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V690Effect : FxVA { public override string Label => "ふんわり光 強く"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Comp("c", i, k.Matrix("m", k.Blur("b", i, 10f + Z(f) * 40f), FxKit.Tone(0, 1, 1, Math.Min(P(f), 1f))), CompositeMode.Plus); } }

[VideoEffect("四重写し", ["演出：ゆらぎ・ぼかし"], ["四重写し", "上下左右にずれた絵を重ねる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "分身", "残像", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V691Effect : FxVA { public override string Label => "四重写し"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float d = Z(f) * f.W * .04f; var g = FxKit.Tone(0, 1, 1, Math.Min(P(f), 1f) * .3f); var o = k.Comp("c1", k.Matrix("m1", k.Affine("a1", i, Mv.Move(-d, -d)), g), k.Matrix("m2", k.Affine("a2", i, Mv.Move(d, -d)), g)); o = k.Comp("c2", o, k.Matrix("m3", k.Affine("a3", i, Mv.Move(-d, d)), g)); o = k.Comp("c3", o, k.Matrix("m4", k.Affine("a4", i, Mv.Move(d, d)), g)); return k.Comp("c4", i, o); } }

[VideoEffect("上下二重写し", ["演出：ゆらぎ・ぼかし"], ["上下二重写し", "上下にずれた絵を重ねて二重に見せる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "ダブル", "めまい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V692Effect : FxVA { public override string Label => "上下二重写し"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float d = MathF.Sin(f.T * Sp(f) * 2f) * Z(f) * f.H * .08f; return k.Comp("c", i, k.Matrix("m", k.Affine("a", i, Mv.Move(0, d)), FxKit.Tone(0, 1, 1, Math.Min(P(f), 1f) * .45f))); } }

[VideoEffect("ななめ二重写し", ["演出：ゆらぎ・ぼかし"], ["ななめ二重写し", "ななめにずれた絵を重ねる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "ダブル", "めまい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V693Effect : FxVA { public override string Label => "ななめ二重写し"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float d = MathF.Sin(f.T * Sp(f) * 2f) * Z(f) * f.W * .06f; return k.Comp("c", i, k.Matrix("m", k.Affine("a", i, Mv.Move(d, d)), FxKit.Tone(0, 1, 1, Math.Min(P(f), 1f) * .45f))); } }

[VideoEffect("回転ブレ", ["演出：ゆらぎ・ぼかし"], ["回転ブレ", "回転したようにブレさせる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "回転", "スピン", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V694Effect : FxVA { public override string Label => "回転ブレ"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float a = Z(f) * .12f * Math.Min(P(f), 1f); var g = FxKit.Tone(0, 1, 1, .3f); var o = k.Comp("c1", k.Matrix("m1", k.Affine("a1", i, Mv.Rot(f.C, a)), g), k.Matrix("m2", k.Affine("a2", i, Mv.Rot(f.C, -a)), g)); return k.Comp("c2", i, o); } }

[VideoEffect("縦の波", ["演出：ゆらぎ・ぼかし"], ["縦の波", "縦に大きくうねるようにゆがむ", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "波", "夢", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V695Effect : FxVA { public override string Label => "縦の波"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new Vector2(f.T * Sp(f) * 60f, 0), .003f, 1, 23), P(f) * Z(f) * 70f); } }

[VideoEffect("ゆがみ ゆっくり", ["演出：ゆらぎ・ぼかし"], ["ゆがみ ゆっくり", "ゆっくりと全体がゆがむ", "ゆらぎ・ぼかし", "ゆがみ", "ゆっくり", "ぼかし", "ゆらぎ", "ブラー", "幻想", "回想", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V696Effect : FxVA { public override string Label => "ゆがみ ゆっくり"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new Vector2(f.T * Sp(f) * 10f, f.T * Sp(f) * 10f), .006f, 2, 29), P(f) * Z(f) * 45f); } }

[VideoEffect("ノイズゆがみ 強く", ["演出：ゆらぎ・ぼかし"], ["ノイズゆがみ 強く", "強くざらざらとゆがむ", "ゆらぎ・ぼかし", "ノイズゆがみ", "強く", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "グリッチ", "不調", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V697Effect : FxVA { public override string Label => "ノイズゆがみ 強く"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.Displace("d", i, k.Turb("t", new Vector2(MathF.Floor(f.T * Sp(f) * 15f) * 53f, 0), .05f, 3, 31), P(f) * Z(f) * 70f); } }

[VideoEffect("ぼかして光る", ["演出：ゆらぎ・ぼかし"], ["ぼかして光る", "ぼかした絵に元の絵を足して明るく光らせる", "ゆらぎ・ぼかし", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "夢", "光", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V698Effect : FxVA { public override string Label => "ぼかして光る"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { var g = k.Blur("b", i, 4f + Z(f) * 20f); return k.Comp("c", g, k.Matrix("m", i, FxKit.Tone(.1f * Math.Min(P(f), 1f), 1, 1, .6f)), CompositeMode.Plus); } }

[VideoEffect("横ブレ 点滅", ["演出：ゆらぎ・ぼかし"], ["横ブレ 点滅", "ブレたり戻ったりを点滅のようにくり返す", "ゆらぎ・ぼかし", "横ブレ", "点滅", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "ショック", "衝撃", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V699Effect : FxVA { public override string Label => "横ブレ 点滅"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { return k.DirBlur("b", i, (MathF.Floor(f.T * Sp(f) * 6f) % 2f) * P(f) * Z(f) * 50f, 0f); } }

[VideoEffect("ズームブレ 脈", ["演出：ゆらぎ・ぼかし"], ["ズームブレ 脈", "脈打つように中心から広がるブレ", "ゆらぎ・ぼかし", "ズームブレ", "脈", "ぼかし", "ゆらぎ", "ゆがみ", "ブラー", "鼓動", "衝撃", "演出"], IsAviUtlSupported = false)]
public sealed class Fx6V700Effect : FxVA { public override string Label => "ズームブレ 脈"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = 1f + Z(f) * .15f * Math.Min(P(f), 1f) * (.5f + .5f * MathF.Sin(f.T * Sp(f) * 5f)); return k.Comp("c", i, k.Matrix("m", k.Affine("a", i, Mv.Scale(f.C, s, s)), FxKit.Tone(0, 1, 1, .35f))); } }
