using System.Numerics;
using Vortice;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 701〜800(gen_v137.py で生成)

[VideoEffect("光 木漏れ日", ["演出：光"], ["光 木漏れ日", "木もれ日のような光の点がゆらめく", "光", "木漏れ日", "ライト", "照明", "グロー", "森", "やさしい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V701Effect : FxLight { public override string Label => "光 木漏れ日"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 40; k++) { float x = l + H(k) * (r - l), y = t + H(k + 99) * (bo - t), a = .5f + .5f * MathF.Sin(o * 1.5f + k); Soft(dc, b, c, x, y, (r - l) * (.02f + .03f * H(k + 7)) * z, a * .6f); } } }

[VideoEffect("光 レンズフレア", ["演出：光"], ["光 レンズフレア", "カメラのレンズに入る光の玉を重ねる", "光", "レンズフレア", "ライト", "照明", "グロー", "カメラ", "逆光", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V702Effect : FxLight { public override string Label => "光 レンズフレア"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { float x0 = r - (r - l) * .15f, y0 = t + (bo - t) * .15f, cx = (l + r) / 2f, cy = (t + bo) / 2f; for (int k = 0; k < 6; k++) { float q = k / 5f * 1.8f; Soft(dc, b, c, x0 + (cx - x0) * q, y0 + (cy - y0) * q, (r - l) * (.02f + .04f * H(k)) * z, .7f); } Soft(dc, b, c, x0, y0, (r - l) * .1f * z, 1f); } }

[VideoEffect("光 光のすじ", ["演出：光"], ["光 光のすじ", "ななめに差しこむ光のすじを重ねる", "光", "光のすじ", "ライト", "照明", "グロー", "天使のはしご", "朝", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V703Effect : FxLight { public override string Label => "光 光のすじ"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { float w = (r - l) * .06f * z; for (int k = 0; k < 5; k++) { float x = l + (k * .22f + (o * .05f) % .22f) * (r - l) * 1.3f - (r - l) * .2f; A(b, c, .25f + .15f * MathF.Sin(o + k)); Poly(dc, b, x, t, x + w, t, x + w - (bo - t) * .5f, bo, x - (bo - t) * .5f, bo); } } }

[VideoEffect("光 光の粒", ["演出：光"], ["光 光の粒", "光の粒がふわふわのぼる", "光", "光の粒", "ライト", "照明", "グロー", "キラキラ", "魔法", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V704Effect : FxLight { public override string Label => "光 光の粒"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 50; k++) { float x = l + H(k) * (r - l), y = bo - ((H(k + 5) + o * .08f * (.5f + H(k + 9))) % 1f) * (bo - t); A(b, c, .8f * (.5f + .5f * MathF.Sin(o * 3f + k))); Dot(dc, b, x, y, (2f + 4f * H(k + 3)) * z); } } }

[VideoEffect("光 虹", ["演出：光"], ["光 虹", "空に虹をかける", "光", "虹", "ライト", "照明", "グロー", "レインボー", "晴れ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V705Effect : FxLight { public override string Label => "光 虹"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { float cx = (l + r) / 2f, cy = bo + (bo - t) * .2f, R0 = (r - l) * .45f * z; uint[] cs = [0xFF0000, 0xFF8000, 0xFFFF00, 0x00C000, 0x0080FF, 0x4000C0, 0x8000C0]; for (int k = 0; k < 7; k++) { Rgb(b, cs[k], .22f * c.A); dc.DrawEllipse(new Ellipse(new(cx, cy), R0 - k * R0 * .05f, R0 - k * R0 * .05f), b, R0 * .05f); } } }

[VideoEffect("光 舞台照明", ["演出：光"], ["光 舞台照明", "色とりどりの舞台照明が動く", "光", "舞台照明", "ライト", "照明", "グロー", "ライブ", "ステージ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V706Effect : FxLight { public override string Label => "光 舞台照明"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { uint[] cs = [0xFF4060, 0x40A0FF, 0xFFE040]; for (int k = 0; k < 3; k++) { float x = l + (r - l) * (.2f + k * .3f) + MathF.Sin(o + k * 2f) * (r - l) * .1f; Rgb(b, cs[k], .3f * c.A); Poly(dc, b, x - 10f, t, x + 10f, t, x + (r - l) * .12f * z, bo, x - (r - l) * .12f * z, bo); } } }

[VideoEffect("光 ストロボ", ["演出：光"], ["光 ストロボ", "ストロボのように一瞬光る", "光", "ストロボ", "ライト", "照明", "グロー", "フラッシュ", "クラブ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V707Effect : FxLight { public override string Label => "光 ストロボ"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { if (o * 4f % 1f < .15f) { A(b, c, .8f); dc.FillRectangle(new RawRectF(l, t, r, bo), b); } } }

[VideoEffect("光 回る光", ["演出：光"], ["光 回る光", "中心から光の帯がぐるぐる回る", "光", "回る光", "ライト", "照明", "グロー", "サンバースト", "集中", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V708Effect : FxLight { public override string Label => "光 回る光"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { float cx = (l + r) / 2f, cy = (t + bo) / 2f, R0 = (r - l) * 1.2f; for (int k = 0; k < 4; k++) { float a = o * .8f + k * MathF.PI / 2f, w = .12f * z; A(b, c, .25f); Poly(dc, b, cx, cy, cx + MathF.Cos(a - w) * R0, cy + MathF.Sin(a - w) * R0, cx + MathF.Cos(a + w) * R0, cy + MathF.Sin(a + w) * R0); } } }

[VideoEffect("光 走る光", ["演出：光"], ["光 走る光", "つやのある光が表面を走る", "光", "走る光", "ライト", "照明", "グロー", "シャイン", "光沢", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V709Effect : FxLight { public override string Label => "光 走る光"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { float w = (r - l) * .08f * z, x = l - w * 4f + ((o * .4f) % 1.5f) * (r - l) * 1.2f; A(b, c, .6f); Poly(dc, b, x, t, x + w, t, x + w - (bo - t) * .3f, bo, x - (bo - t) * .3f, bo); A(b, c, .3f); Poly(dc, b, x + w * 1.6f, t, x + w * 2f, t, x + w * 2f - (bo - t) * .3f, bo, x + w * 1.6f - (bo - t) * .3f, bo); } }

[VideoEffect("光 光の輪", ["演出：光"], ["光 光の輪", "光の輪が広がっていく", "光", "光の輪", "ライト", "照明", "グロー", "波紋", "衝撃", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V710Effect : FxLight { public override string Label => "光 光の輪"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { float cx = (l + r) / 2f, cy = (t + bo) / 2f; for (int k = 0; k < 3; k++) { float q = (o * .5f + k / 3f) % 1f; A(b, c, (1f - q) * .7f); dc.DrawEllipse(new Ellipse(new(cx, cy), q * (r - l) * .6f * z, q * (r - l) * .6f * z), b, 6f * z); } } }

[VideoEffect("光 オーロラ", ["演出：光"], ["光 オーロラ", "空にオーロラが揺れる", "光", "オーロラ", "ライト", "照明", "グロー", "北極", "夜空", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V711Effect : FxLight { public override string Label => "光 オーロラ"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 4; k++) { var q = new List<Vector2>(); for (int j = 0; j <= 20; j++) { float x = l + (r - l) * j / 20f; q.Add(new(x, t + (bo - t) * (.15f + k * .06f) + MathF.Sin(j * .5f + o + k) * (bo - t) * .05f * z)); } for (int j = 20; j >= 0; j--) { float x = l + (r - l) * j / 20f; q.Add(new(x, t + (bo - t) * (.35f + k * .06f) + MathF.Sin(j * .4f + o * .7f + k) * (bo - t) * .06f * z)); } Rgb(b, k % 2 == 0 ? 0x40FFA0u : 0x60A0FFu, .18f * c.A); YMM4EffectShapes.Gfx.FillPoly(dc, b, q); } } }

[VideoEffect("光 逆光", ["演出：光"], ["光 逆光", "左上から強い光が差しこむ", "光", "逆光", "ライト", "照明", "グロー", "まぶしい", "太陽", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V712Effect : FxLight { public override string Label => "光 逆光"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { Soft(dc, b, c, l + (r - l) * .15f, t + (bo - t) * .1f, (r - l) * .5f * z, 1.2f); } }

[VideoEffect("光 窓の光", ["演出：光"], ["光 窓の光", "窓から差しこむ四角い光を重ねる", "光", "窓の光", "ライト", "照明", "グロー", "部屋", "朝", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V713Effect : FxLight { public override string Label => "光 窓の光"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 2; k++) for (int j = 0; j < 2; j++) { float x = l + (r - l) * (.25f + k * .17f), y = t + (bo - t) * (.25f + j * .25f), w = (r - l) * .14f * z, h = (bo - t) * .2f * z; A(b, c, .25f); Poly(dc, b, x, y, x + w, y, x + w + h * .5f, y + h, x + h * .5f, y + h); } } }

[VideoEffect("光 星のまたたき", ["演出：光"], ["光 星のまたたき", "星がきらきらまたたく", "光", "星のまたたき", "ライト", "照明", "グロー", "キラキラ", "夜", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V714Effect : FxLight { public override string Label => "光 星のまたたき"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 30; k++) { float x = l + H(k) * (r - l), y = t + H(k + 40) * (bo - t), s = (3f + 6f * H(k + 2)) * z * (.3f + .7f * MathF.Max(0f, MathF.Sin(o * 3f + k * 1.7f))); A(b, c, .9f); Poly(dc, b, x, y - s * 2f, x + s * .3f, y - s * .3f, x + s * 2f, y, x + s * .3f, y + s * .3f, x, y + s * 2f, x - s * .3f, y + s * .3f, x - s * 2f, y, x - s * .3f, y - s * .3f); } } }

[VideoEffect("光 夕日", ["演出：光"], ["光 夕日", "下から夕日の光が差す", "光", "夕日", "ライト", "照明", "グロー", "夕方", "オレンジ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V715Effect : FxLight { public override string Label => "光 夕日"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 8; k++) { Rgb(b, 0xFF8030, .07f * c.A * z); dc.FillRectangle(new RawRectF(l, bo - (bo - t) * (k + 1) / 8f, r, bo), b); } } }

[VideoEffect("光 朝の光", ["演出：光"], ["光 朝の光", "上から朝のやわらかい光が差す", "光", "朝の光", "ライト", "照明", "グロー", "朝", "さわやか", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V716Effect : FxLight { public override string Label => "光 朝の光"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 8; k++) { Rgb(b, 0xFFF0C0, .07f * c.A * z); dc.FillRectangle(new RawRectF(l, t, r, t + (bo - t) * (k + 1) / 8f), b); } } }

[VideoEffect("光 ホタル", ["演出：光"], ["光 ホタル", "ホタルの光がふわふわ飛ぶ", "光", "ホタル", "ライト", "照明", "グロー", "夏", "夜", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V717Effect : FxLight { public override string Label => "光 ホタル"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 18; k++) { float x = l + ((H(k) + MathF.Sin(o * .3f + k) * .05f) % 1f) * (r - l), y = t + (H(k + 30) + MathF.Cos(o * .4f + k) * .05f) * (bo - t); Rgb(b, 0xE6FF66, 1f); Soft(dc, b, new Color4(.9f, 1f, .4f, c.A), x, y, 12f * z, .5f + .5f * MathF.Sin(o * 2f + k)); } } }

[VideoEffect("光 光のカーテン", ["演出：光"], ["光 光のカーテン", "縦の光のカーテンが揺れる", "光", "光のカーテン", "ライト", "照明", "グロー", "幻想", "舞台", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V718Effect : FxLight { public override string Label => "光 光のカーテン"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { for (int k = 0; k < 12; k++) { float x = l + (r - l) * k / 12f + MathF.Sin(o + k) * 8f; A(b, c, .12f + .1f * MathF.Sin(o * 2f + k * .7f)); dc.FillRectangle(new RawRectF(x, t, x + (r - l) / 24f * z, bo), b); } } }

[VideoEffect("光 雷の光", ["演出：光"], ["光 雷の光", "ときどき雷のように光る", "光", "雷の光", "ライト", "照明", "グロー", "嵐", "ピカッ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V719Effect : FxLight { public override string Label => "光 雷の光"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { float n = FxKit.Noise(o * 3f, 5); if (n > .55f) { A(b, c, (n - .55f) * 2f); dc.FillRectangle(new RawRectF(l, t, r, bo), b); } } }

[VideoEffect("光 ハイライト", ["演出：光"], ["光 ハイライト", "やわらかい光の当たりがゆっくり動く", "光", "ハイライト", "ライト", "照明", "グロー", "つや", "光沢", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V720Effect : FxLight { public override string Label => "光 ハイライト"; private protected override void Lt(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, Vortice.Mathematics.Color4 c, float l, float t, float r, float bo, float z, float o) { Soft(dc, b, c, l + (r - l) * (.3f + .2f * MathF.Sin(o * .5f)), t + (bo - t) * .3f, (r - l) * .25f * z, .8f); } }

[VideoEffect("色の変化 色相 回る ゆっくり", ["演出：色の変化"], ["色の変化 色相 回る ゆっくり", "色相がゆっくり回り続ける", "色の変化", "色相", "回る", "ゆっくり", "色", "カラー", "変化", "アニメーション", "虹色", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V721Effect : FxVA { public override string Label => "色の変化 色相 回る ゆっくり"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Hue(s * .8f), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 色相 回る 速い", ["演出：色の変化"], ["色の変化 色相 回る 速い", "色相が速く回り続ける", "色の変化", "色相", "回る", "速い", "色", "カラー", "変化", "アニメーション", "虹色", "派手", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V722Effect : FxVA { public override string Label => "色の変化 色相 回る 速い"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Hue(s * 4f), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 明るさ 点滅", ["演出：色の変化"], ["色の変化 明るさ 点滅", "明るくなったり戻ったりを点滅する", "色の変化", "明るさ", "点滅", "色", "カラー", "変化", "アニメーション", "フラッシュ", "注目", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V723Effect : FxVA { public override string Label => "色の変化 明るさ 点滅"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(FxKit.Tone(.35f * (s * 2f % 1f < .5f ? 1f : 0f)), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 彩度 呼吸", ["演出：色の変化"], ["色の変化 彩度 呼吸", "鮮やかさが大きくなったり小さくなったりする", "色の変化", "彩度", "呼吸", "色", "カラー", "変化", "アニメーション", "鮮やか", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V724Effect : FxVA { public override string Label => "色の変化 彩度 呼吸"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(FxKit.Tone(0, 1, 1f + MathF.Sin(s * 2f) * .8f), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 白黒と色", ["演出：色の変化"], ["色の変化 白黒と色", "白黒とカラーを行き来する", "色の変化", "白黒と色", "色", "カラー", "変化", "アニメーション", "モノクロ", "回想", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V725Effect : FxVA { public override string Label => "色の変化 白黒と色"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(FxKit.Tone(0, 1, .5f + .5f * MathF.Sin(s * 1.5f)), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 赤く点滅", ["演出：色の変化"], ["色の変化 赤く点滅", "赤く光って点滅する", "色の変化", "赤く点滅", "色", "カラー", "変化", "アニメーション", "警告", "危険", "ピンチ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V726Effect : FxVA { public override string Label => "色の変化 赤く点滅"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(Gd.G(1f, 1f, 0f, 1.6f, .55f, .55f), .5f + .5f * MathF.Sin(s * 6f)), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 青く点滅", ["演出：色の変化"], ["色の変化 青く点滅", "青く光って点滅する", "色の変化", "青く点滅", "色", "カラー", "変化", "アニメーション", "冷たい", "通知", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V727Effect : FxVA { public override string Label => "色の変化 青く点滅"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(Gd.G(1f, 1f, 0f, .55f, .7f, 1.6f), .5f + .5f * MathF.Sin(s * 6f)), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 だんだん夕方", ["演出：色の変化"], ["色の変化 だんだん夕方", "時間とともに夕方の色になる", "色の変化", "だんだん夕方", "色", "カラー", "変化", "アニメーション", "日暮れ", "時間経過", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V728Effect : FxVA { public override string Label => "色の変化 だんだん夕方"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(Gd.G(1.1f, 1f, 0f, 1.25f, .95f, .7f), u), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 だんだん夜", ["演出：色の変化"], ["色の変化 だんだん夜", "時間とともに夜の色になる", "色の変化", "だんだん夜", "色", "カラー", "変化", "アニメーション", "夜", "時間経過", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V729Effect : FxVA { public override string Label => "色の変化 だんだん夜"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(Gd.G(.7f, 1f, -.18f, .8f, .9f, 1.25f), u), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 だんだん色あせ", ["演出：色の変化"], ["色の変化 だんだん色あせ", "時間とともに色があせていく", "色の変化", "だんだん色あせ", "色", "カラー", "変化", "アニメーション", "思い出", "古い", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V730Effect : FxVA { public override string Label => "色の変化 だんだん色あせ"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(FxKit.Tone(.05f, .85f, .2f), u), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 だんだん色が戻る", ["演出：色の変化"], ["色の変化 だんだん色が戻る", "白黒から少しずつ色が戻る", "色の変化", "だんだん色が戻る", "色", "カラー", "変化", "アニメーション", "回想", "復活", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V731Effect : FxVA { public override string Label => "色の変化 だんだん色が戻る"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(FxKit.Tone(0, 1, 0), 1f - u), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 ネガ点滅", ["演出：色の変化"], ["色の変化 ネガ点滅", "ときどき色が反転する", "色の変化", "ネガ点滅", "色", "カラー", "変化", "アニメーション", "反転", "衝撃", "グリッチ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V732Effect : FxVA { public override string Label => "色の変化 ネガ点滅"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(Mx.Out(-1, 0, 0, 1, 0, -1, 0, 1, 0, 0, -1, 1), s * 3f % 1f < .2f ? 1f : 0f), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 ネオン点滅", ["演出：色の変化"], ["色の変化 ネオン点滅", "ギラギラした色に点滅する", "色の変化", "ネオン点滅", "色", "カラー", "変化", "アニメーション", "派手", "夜の街", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V733Effect : FxVA { public override string Label => "色の変化 ネオン点滅"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(Gd.G(1.9f, 1.25f, .02f), s * 2f % 1f < .5f ? 1f : 0f), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 だんだんセピア", ["演出：色の変化"], ["色の変化 だんだんセピア", "時間とともにセピア色になる", "色の変化", "だんだんセピア", "色", "カラー", "変化", "アニメーション", "昔", "回想", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V734Effect : FxVA { public override string Label => "色の変化 だんだんセピア"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(Gd.D(0x3B2418, 0xF5E6C8), u), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 赤と青の交互", ["演出：色の変化"], ["色の変化 赤と青の交互", "赤と青に交互に光る", "色の変化", "赤と青の交互", "色", "カラー", "変化", "アニメーション", "パトカー", "サイレン", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V735Effect : FxVA { public override string Label => "色の変化 赤と青の交互"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(s * 2f % 1f < .5f ? Gd.G(1f, 1f, 0f, 1.5f, .7f, .7f) : Gd.G(1f, 1f, 0f, .7f, .7f, 1.5f), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 コントラスト 呼吸", ["演出：色の変化"], ["色の変化 コントラスト 呼吸", "はっきりしたりぼんやりしたりする", "色の変化", "コントラスト", "呼吸", "色", "カラー", "変化", "アニメーション", "明暗", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V736Effect : FxVA { public override string Label => "色の変化 コントラスト 呼吸"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(FxKit.Tone(0, 1f + MathF.Sin(s * 2f) * .4f), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 暖色と寒色", ["演出：色の変化"], ["色の変化 暖色と寒色", "暖かい色と冷たい色を行き来する", "色の変化", "暖色と寒色", "色", "カラー", "変化", "アニメーション", "温度", "気分", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V737Effect : FxVA { public override string Label => "色の変化 暖色と寒色"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(MathF.Sin(s) > 0 ? Mx.Mix(Gd.G(1f, 1f, 0f, 1.2f, 1f, .8f), MathF.Sin(s)) : Mx.Mix(Gd.G(1f, 1f, 0f, .8f, 1f, 1.2f), -MathF.Sin(s)), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 白く飛ぶ 脈", ["演出：色の変化"], ["色の変化 白く飛ぶ 脈", "ときどき白く飛ぶように光る", "色の変化", "白く飛ぶ", "脈", "色", "カラー", "変化", "アニメーション", "衝撃", "ひらめき", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V738Effect : FxVA { public override string Label => "色の変化 白く飛ぶ 脈"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(FxKit.Tone(.5f * MathF.Pow(MathF.Max(0f, MathF.Sin(s * 2f)), 6f)), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 だんだん暗く", ["演出：色の変化"], ["色の変化 だんだん暗く", "時間とともに暗くなる", "色の変化", "だんだん暗く", "色", "カラー", "変化", "アニメーション", "フェード", "終わり", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V739Effect : FxVA { public override string Label => "色の変化 だんだん暗く"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(FxKit.Tone(-.6f), u), Math.Min(P(f), 1f))); } }

[VideoEffect("色の変化 だんだん明るく", ["演出：色の変化"], ["色の変化 だんだん明るく", "暗い所から少しずつ明るくなる", "色の変化", "だんだん明るく", "色", "カラー", "変化", "アニメーション", "フェード", "始まり", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V740Effect : FxVA { public override string Label => "色の変化 だんだん明るく"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float s = f.T * Sp(f), u = Math.Clamp(f.T / MathF.Max(.01f, f.Dur), 0f, 1f); return k.Matrix("m", i, Mx.Mix(Mx.Mix(FxKit.Tone(-.6f), 1f - u), Math.Min(P(f), 1f))); } }

[VideoEffect("動き4 大きな円", ["演出：動き"], ["動き4 大きな円", "大きな円を描いてゆっくり回る", "動き", "動き4", "大きな円", "モーション", "ループ", "回る", "漂う", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V741Effect : FxMove { public override string Label => "動き4 大きな円"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, C(t) * .1f * a, S(t) * .1f * a); }

[VideoEffect("動き4 逆回りの円", ["演出：動き"], ["動き4 逆回りの円", "左回りに円を描く", "動き", "動き4", "逆回りの円", "モーション", "ループ", "回る", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V742Effect : FxMove { public override string Label => "動き4 逆回りの円"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, C(-t * 2f) * .05f * a, S(-t * 2f) * .05f * a); }

[VideoEffect("動き4 縦の8の字", ["演出：動き"], ["動き4 縦の8の字", "縦長の8の字を描く", "動き", "動き4", "縦の8の字", "モーション", "ループ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V743Effect : FxMove { public override string Label => "動き4 縦の8の字"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * 4f) * .03f * a, S(t * 2f) * .06f * a); }

[VideoEffect("動き4 ゆらゆら 大きく", ["演出：動き"], ["動き4 ゆらゆら 大きく", "大きくゆらゆら揺れる", "動き", "動き4", "ゆらゆら", "大きく", "モーション", "ループ", "揺れ", "船", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V744Effect : FxMove { public override string Label => "動き4 ゆらゆら 大きく"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, S(t * 1.2f) * .2f * a) * Mo(f, S(t * 1.2f) * .04f * a, 0); }

[VideoEffect("動き4 ぶるぶる 縦", ["演出：動き"], ["動き4 ぶるぶる 縦", "縦に細かく震える", "動き", "動き4", "ぶるぶる", "縦", "モーション", "ループ", "震え", "振動", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V745Effect : FxMove { public override string Label => "動き4 ぶるぶる 縦"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, 0, N(t * 25f, 3) * .02f * a); }

[VideoEffect("動き4 ぶるぶる 横", ["演出：動き"], ["動き4 ぶるぶる 横", "横に細かく震える", "動き", "動き4", "ぶるぶる", "横", "モーション", "ループ", "震え", "振動", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V746Effect : FxMove { public override string Label => "動き4 ぶるぶる 横"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, N(t * 25f, 4) * .02f * a, 0); }

[VideoEffect("動き4 ぽよんぽよん", ["演出：動き"], ["動き4 ぽよんぽよん", "足元を支点にぽよんぽよんと伸び縮み", "動き", "動き4", "ぽよんぽよん", "モーション", "ループ", "弾む", "やわらか", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V747Effect : FxMove { public override string Label => "動き4 ぽよんぽよん"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f, 1f + S(t * 6f) * .05f * a, 1f - S(t * 6f) * .07f * a); }

[VideoEffect("動き4 くねくね", ["演出：動き"], ["動き4 くねくね", "くねくねと体を揺らす", "動き", "動き4", "くねくね", "モーション", "ループ", "ダンス", "踊る", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V748Effect : FxMove { public override string Label => "動き4 くねくね"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f, S(t * 3f) * .15f * a) * Mo(f, -S(t * 3f) * .02f * a, 0); }

[VideoEffect("動き4 ゆっくり沈む", ["演出：動き"], ["動き4 ゆっくり沈む", "ゆっくり沈んでは戻る", "動き", "動き4", "ゆっくり沈む", "モーション", "ループ", "沈む", "水", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V749Effect : FxMove { public override string Label => "動き4 ゆっくり沈む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, 0, ((t * .2f) % 1f) * .1f * a); }

[VideoEffect("動き4 ゆっくり浮かぶ", ["演出：動き"], ["動き4 ゆっくり浮かぶ", "ゆっくり浮かんでは戻る", "動き", "動き4", "ゆっくり浮かぶ", "モーション", "ループ", "浮く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V750Effect : FxMove { public override string Label => "動き4 ゆっくり浮かぶ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, 0, -((t * .2f) % 1f) * .1f * a); }

[VideoEffect("動き4 ブランコ", ["演出：動き"], ["動き4 ブランコ", "ブランコのように大きく揺れる", "動き", "動き4", "ブランコ", "モーション", "ループ", "公園", "揺れ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V751Effect : FxMove { public override string Label => "動き4 ブランコ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mv.Rot(new Vector2(f.C.X, f.B.Top - f.H * 1.2f), S(t * 1.6f) * .12f * a); }

[VideoEffect("動き4 なみなみ横移動", ["演出：動き"], ["動き4 なみなみ横移動", "波のように上下しながら左右に動く", "動き", "動き4", "なみなみ横移動", "モーション", "ループ", "泳ぐ", "漂う", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V752Effect : FxMove { public override string Label => "動き4 なみなみ横移動"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * .8f) * .08f * a, S(t * 4f) * .02f * a); }

[VideoEffect("動き4 首かしげ", ["演出：動き"], ["動き4 首かしげ", "ときどき首をかしげる", "動き", "動き4", "首かしげ", "モーション", "ループ", "疑問", "はてな", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V753Effect : FxMove { public override string Label => "動き4 首かしげ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, (S(t * 1.5f) > .3f ? .12f : 0f) * a); }

[VideoEffect("動き4 おじぎ", ["演出：動き"], ["動き4 おじぎ", "前にかがむようにおじぎする", "動き", "動き4", "おじぎ", "モーション", "ループ", "お礼", "あいさつ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V754Effect : FxMove { public override string Label => "動き4 おじぎ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mv.Scale(new Vector2(f.C.X, f.B.Bottom), 1f, 1f - MathF.Max(0f, S(t * 2f)) * .15f * a) * Sk(f, MathF.Max(0f, S(t * 2f)) * .1f * a); }

[VideoEffect("動き4 ひょこひょこ", ["演出：動き"], ["動き4 ひょこひょこ", "小刻みにひょこひょこ歩く", "動き", "動き4", "ひょこひょこ", "モーション", "ループ", "歩く", "かわいい", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V755Effect : FxMove { public override string Label => "動き4 ひょこひょこ"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, 0, -MathF.Abs(S(t * 8f)) * .02f * a) * Ro(f, S(t * 4f) * .04f * a); }

[VideoEffect("動き4 ぐらぐら", ["演出：動き"], ["動き4 ぐらぐら", "足元からぐらぐら傾く", "動き", "動き4", "ぐらぐら", "モーション", "ループ", "不安定", "倒れそう", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V756Effect : FxMove { public override string Label => "動き4 ぐらぐら"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => RoF(f, N(t * 2f, 9) * .2f * a); }

[VideoEffect("動き4 鼓動 2回", ["演出：動き"], ["動き4 鼓動 2回", "ドクンドクンと2回ずつ脈打つ", "動き", "動き4", "鼓動", "2回", "モーション", "ループ", "心臓", "緊張", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V757Effect : FxMove { public override string Label => "動き4 鼓動 2回"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f, 1f + (MathF.Pow(MathF.Max(0f, S(t * 6f)), 8f) + MathF.Pow(MathF.Max(0f, S(t * 6f - 1f)), 8f)) * .06f * a, 1f + (MathF.Pow(MathF.Max(0f, S(t * 6f)), 8f) + MathF.Pow(MathF.Max(0f, S(t * 6f - 1f)), 8f)) * .06f * a); }

[VideoEffect("動き4 ゆっくり回転", ["演出：動き"], ["動き4 ゆっくり回転", "ゆっくりずっと回る", "動き", "動き4", "ゆっくり回転", "モーション", "ループ", "回転", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V758Effect : FxMove { public override string Label => "動き4 ゆっくり回転"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f, t * .5f * a); }

[VideoEffect("動き4 左右に跳ねる", ["演出：動き"], ["動き4 左右に跳ねる", "左右に跳ねながら行き来する", "動き", "動き4", "左右に跳ねる", "モーション", "ループ", "跳ねる", "元気", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V759Effect : FxMove { public override string Label => "動き4 左右に跳ねる"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f, S(t * 2f) * .06f * a, -MathF.Abs(C(t * 2f)) * .05f * a); }

[VideoEffect("動き4 ふわっと拡大縮小", ["演出：動き"], ["動き4 ふわっと拡大縮小", "ゆっくり大きく拡大縮小する", "動き", "動き4", "ふわっと拡大縮小", "モーション", "ループ", "ズーム", "呼吸", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V760Effect : FxMove { public override string Label => "動き4 ふわっと拡大縮小"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f, 1f + S(t * .6f) * .15f * a, 1f + S(t * .6f) * .15f * a); }

[VideoEffect("出入り4 真上から", ["演出：出入り"], ["出入り4 真上から", "真上からまっすぐ降りて出る", "出入り", "出入り4", "真上から", "登場", "退場", "トランジション", "スライド", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V761Effect : FxEnter { public override string Label => "出入り4 真上から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, 0, -.7f * d); }

[VideoEffect("出入り4 真下から", ["演出：出入り"], ["出入り4 真下から", "真下からまっすぐ上がって出る", "出入り", "出入り4", "真下から", "登場", "退場", "トランジション", "スライド", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V762Effect : FxEnter { public override string Label => "出入り4 真下から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, 0, .7f * d); }

[VideoEffect("出入り4 遠くの左から", ["演出：出入り"], ["出入り4 遠くの左から", "画面の外の左から出る", "出入り", "出入り4", "遠くの左から", "登場", "退場", "トランジション", "スライド", "遠く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V763Effect : FxEnter { public override string Label => "出入り4 遠くの左から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, -1.1f * d * dir, 0); }

[VideoEffect("出入り4 遠くの右から", ["演出：出入り"], ["出入り4 遠くの右から", "画面の外の右から出る", "出入り", "出入り4", "遠くの右から", "登場", "退場", "トランジション", "スライド", "遠く", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V764Effect : FxEnter { public override string Label => "出入り4 遠くの右から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, 1.1f * d * dir, 0); }

[VideoEffect("出入り4 回って拡大", ["演出：出入り"], ["出入り4 回って拡大", "小さく回りながら大きくなって出る", "出入り", "出入り4", "回って拡大", "登場", "退場", "トランジション", "回転", "ズーム", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V765Effect : FxEnter { public override string Label => "出入り4 回って拡大"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, d * 3f * dir) * Sc(f, 1f - d * .9f, 1f - d * .9f); }

[VideoEffect("出入り4 ゆれて落ちる", ["演出：出入り"], ["出入り4 ゆれて落ちる", "葉っぱのように揺れながら落ちて出る", "出入り", "出入り4", "ゆれて落ちる", "登場", "退場", "トランジション", "ひらひら", "落ちる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V766Effect : FxEnter { public override string Label => "出入り4 ゆれて落ちる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, MathF.Sin(d * 10f) * d * .4f) * Mo(f, 0, -.5f * d); }

[VideoEffect("出入り4 ぐーんと伸びる", ["演出：出入り"], ["出入り4 ぐーんと伸びる", "縦にぐーんと伸びた形から出る", "出入り", "出入り4", "ぐーんと伸びる", "登場", "退場", "トランジション", "伸びる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V767Effect : FxEnter { public override string Label => "出入り4 ぐーんと伸びる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f, 1f + d * 2f) * Mo(f, 0, -.3f * d); }

[VideoEffect("出入り4 ぺたっと貼る", ["演出：出入り"], ["出入り4 ぺたっと貼る", "ぺたっと貼りつくように出る", "出入り", "出入り4", "ぺたっと貼る", "登場", "退場", "トランジション", "シール", "貼る", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V768Effect : FxEnter { public override string Label => "出入り4 ぺたっと貼る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f + d * .6f, 1f + d * .6f) * Sk(f, d * .3f); }

[VideoEffect("出入り4 ジグザグ", ["演出：出入り"], ["出入り4 ジグザグ", "ジグザグに動きながら出る", "出入り", "出入り4", "ジグザグ", "登場", "退場", "トランジション", "くねくね", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V769Effect : FxEnter { public override string Label => "出入り4 ジグザグ"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, MathF.Sin(d * 12f) * .1f * d, -.4f * d); }

[VideoEffect("出入り4 右回り 大きく", ["演出：出入り"], ["出入り4 右回り 大きく", "大きく右に回って出る", "出入り", "出入り4", "右回り", "大きく", "登場", "退場", "トランジション", "回転", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V770Effect : FxEnter { public override string Label => "出入り4 右回り 大きく"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, -d * 3f * dir); }

[VideoEffect("出入り4 左回り 大きく", ["演出：出入り"], ["出入り4 左回り 大きく", "大きく左に回って出る", "出入り", "出入り4", "左回り", "大きく", "登場", "退場", "トランジション", "回転", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V771Effect : FxEnter { public override string Label => "出入り4 左回り 大きく"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f, d * 3f * dir); }

[VideoEffect("出入り4 ななめ下から", ["演出：出入り"], ["出入り4 ななめ下から", "右ななめ下から出る", "出入り", "出入り4", "ななめ下から", "登場", "退場", "トランジション", "スライド", "ななめ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V772Effect : FxEnter { public override string Label => "出入り4 ななめ下から"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, .35f * d * dir, .5f * d); }

[VideoEffect("出入り4 しぼんで消える", ["演出：出入り"], ["出入り4 しぼんで消える", "横にしぼむように出入りする", "出入り", "出入り4", "しぼんで消える", "登場", "退場", "トランジション", "しぼむ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V773Effect : FxEnter { public override string Label => "出入り4 しぼんで消える"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f - d, 1f - d * .3f); }

[VideoEffect("出入り4 ゆがんで出る", ["演出：出入り"], ["出入り4 ゆがんで出る", "大きくゆがんだ形から出る", "出入り", "出入り4", "ゆがんで出る", "登場", "退場", "トランジション", "ゆがむ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V774Effect : FxEnter { public override string Label => "出入り4 ゆがんで出る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f, d * 1.5f * dir) * Sc(f, 1f - d * .5f, 1f); }

[VideoEffect("出入り4 くるんと着地", ["演出：出入り"], ["出入り4 くるんと着地", "くるんと回って着地する", "出入り", "出入り4", "くるんと着地", "登場", "退場", "トランジション", "着地", "宙返り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V775Effect : FxEnter { public override string Label => "出入り4 くるんと着地"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => RoF(f, -d * MathF.Tau * dir) * Mo(f, 0, -.5f * d); }

[VideoEffect("出入り4 飛びこむ", ["演出：出入り"], ["出入り4 飛びこむ", "弧を描いて飛びこんで出る", "出入り", "出入り4", "飛びこむ", "登場", "退場", "トランジション", "ジャンプ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V776Effect : FxEnter { public override string Label => "出入り4 飛びこむ"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, -.6f * d * dir, -.3f * MathF.Sin(d * MathF.PI)) * Ro(f, d * .6f * dir); }

[VideoEffect("出入り4 弾んで縮む", ["演出：出入り"], ["出入り4 弾んで縮む", "大きさが弾みながら出る", "出入り", "出入り4", "弾んで縮む", "登場", "退場", "トランジション", "ぽよん", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V777Effect : FxEnter { public override string Label => "出入り4 弾んで縮む"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f + MathF.Sin(d * 9f) * d * .5f, 1f + MathF.Sin(d * 9f) * d * .5f); }

[VideoEffect("出入り4 ななめに伸びる", ["演出：出入り"], ["出入り4 ななめに伸びる", "ななめに引き伸ばされた形から出る", "出入り", "出入り4", "ななめに伸びる", "登場", "退場", "トランジション", "伸びる", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V778Effect : FxEnter { public override string Label => "出入り4 ななめに伸びる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f + d, 1f - d * .8f) * Ro(f, d * .5f * dir); }

[VideoEffect("出入り4 すとんと落ちる", ["演出：出入り"], ["出入り4 すとんと落ちる", "すとんと勢いよく落ちて出る", "出入り", "出入り4", "すとんと落ちる", "登場", "退場", "トランジション", "落下", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V779Effect : FxEnter { public override string Label => "出入り4 すとんと落ちる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f, 0, -.8f * d * d); }

[VideoEffect("出入り4 ゆっくり近づく", ["演出：出入り"], ["出入り4 ゆっくり近づく", "少し小さい所からゆっくり近づいて出る", "出入り", "出入り4", "ゆっくり近づく", "登場", "退場", "トランジション", "やさしい", "ズーム", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V780Effect : FxEnter { public override string Label => "出入り4 ゆっくり近づく"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f, 1f - d * .5f, 1f - d * .5f); }

[VideoEffect("重ね模様 さくら", ["演出：質感・画風"], ["重ね模様 さくら", "さくらの花の模様が舞う", "質感・画風", "重ね模様", "さくら", "模様", "パターン", "柄", "テクスチャ", "春", "花見", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V781Effect : FxPat { public override string Label => "重ね模様 さくら"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => { float yy = y + (o + i * 5f) % u; for (int k = 0; k < 5; k++) { float a = k * 1.2566f + i; Dot(dc, b, x + MathF.Cos(a) * u * .08f, yy + MathF.Sin(a) * u * .08f, u * .07f); } }); } }

[VideoEffect("重ね模様 クローバー", ["演出：質感・画風"], ["重ね模様 クローバー", "四つ葉のクローバーの模様", "質感・画風", "重ね模様", "クローバー", "模様", "パターン", "柄", "テクスチャ", "幸運", "緑", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V782Effect : FxPat { public override string Label => "重ね模様 クローバー"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => { for (int k = 0; k < 4; k++) Dot(dc, b, x + MathF.Cos(k * 1.5708f + .785f) * u * .08f, y + MathF.Sin(k * 1.5708f + .785f) * u * .08f, u * .08f); Line(dc, b, x, y, x + u * .05f, y + u * .22f, 2f); }); } }

[VideoEffect("重ね模様 ちょうちょ", ["演出：質感・画風"], ["重ね模様 ちょうちょ", "はばたくちょうちょの模様", "質感・画風", "重ね模様", "ちょうちょ", "模様", "パターン", "柄", "テクスチャ", "春", "ひらひら", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V783Effect : FxPat { public override string Label => "重ね模様 ちょうちょ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { float f2 = MathF.Abs(MathF.Sin(o * .3f + i)); float xx = x + (o * 1.5f) % u; Poly(dc, b, xx, y, xx - u * .2f * f2, y - u * .14f, xx - u * .16f * f2, y + u * .1f); Poly(dc, b, xx, y, xx + u * .2f * f2, y - u * .14f, xx + u * .16f * f2, y + u * .1f); }); } }

[VideoEffect("重ね模様 王冠", ["演出：質感・画風"], ["重ね模様 王冠", "王冠の模様を重ねる", "質感・画風", "重ね模様", "王冠", "模様", "パターン", "柄", "テクスチャ", "王様", "一番", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V784Effect : FxPat { public override string Label => "重ね模様 王冠"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => Poly(dc, b, x - u * .2f, y + u * .1f, x - u * .2f, y - u * .1f, x - u * .1f, y, x, y - u * .14f, x + u * .1f, y, x + u * .2f, y - u * .1f, x + u * .2f, y + u * .1f)); } }

[VideoEffect("重ね模様 スペード", ["演出：質感・画風"], ["重ね模様 スペード", "トランプのスペードの模様", "質感・画風", "重ね模様", "スペード", "模様", "パターン", "柄", "テクスチャ", "トランプ", "カード", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V785Effect : FxPat { public override string Label => "重ね模様 スペード"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => { Poly(dc, b, x, y - u * .2f, x + u * .15f, y, x - u * .15f, y); Dot(dc, b, x - u * .07f, y + u * .02f, u * .08f); Dot(dc, b, x + u * .07f, y + u * .02f, u * .08f); Poly(dc, b, x, y, x + u * .05f, y + u * .16f, x - u * .05f, y + u * .16f); }); } }

[VideoEffect("重ね模様 クラブ", ["演出：質感・画風"], ["重ね模様 クラブ", "トランプのクラブの模様", "質感・画風", "重ね模様", "クラブ", "模様", "パターン", "柄", "テクスチャ", "トランプ", "カード", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V786Effect : FxPat { public override string Label => "重ね模様 クラブ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.5f; Each(l, t, r, bo, u, (x, y, i) => { Dot(dc, b, x, y - u * .08f, u * .07f); Dot(dc, b, x - u * .08f, y + u * .03f, u * .07f); Dot(dc, b, x + u * .08f, y + u * .03f, u * .07f); Poly(dc, b, x, y, x + u * .05f, y + u * .16f, x - u * .05f, y + u * .16f); }); } }

[VideoEffect("重ね模様 リボン", ["演出：質感・画風"], ["重ね模様 リボン", "リボンの模様を重ねる", "質感・画風", "重ね模様", "リボン", "模様", "パターン", "柄", "テクスチャ", "かわいい", "プレゼント", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V787Effect : FxPat { public override string Label => "重ね模様 リボン"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { Poly(dc, b, x, y, x - u * .2f, y - u * .1f, x - u * .2f, y + u * .1f); Poly(dc, b, x, y, x + u * .2f, y - u * .1f, x + u * .2f, y + u * .1f); Dot(dc, b, x, y, u * .05f); }); } }

[VideoEffect("重ね模様 ドーナツ", ["演出：質感・画風"], ["重ね模様 ドーナツ", "太い輪のドーナツの模様", "質感・画風", "重ね模様", "ドーナツ", "模様", "パターン", "柄", "テクスチャ", "お菓子", "輪", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V788Effect : FxPat { public override string Label => "重ね模様 ドーナツ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => Ring(dc, b, x + o % u, y, u * .15f, u * .1f)); } }

[VideoEffect("重ね模様 ネット", ["演出：質感・画風"], ["重ね模様 ネット", "ななめの網目の模様を重ねる", "質感・画風", "重ね模様", "ネット", "模様", "パターン", "柄", "テクスチャ", "網", "フェンス", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V789Effect : FxPat { public override string Label => "重ね模様 ネット"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st; for (float x = l - (bo - t) + o % u; x <= r; x += u) { Line(dc, b, x, bo, x + (bo - t), t, 1.6f); Line(dc, b, x, t, x + (bo - t), bo, 1.6f); } } }

[VideoEffect("重ね模様 風の線", ["演出：質感・画風"], ["重ね模様 風の線", "風が吹く線の模様が流れる", "質感・画風", "重ね模様", "風の線", "模様", "パターン", "柄", "テクスチャ", "風", "そよ風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V790Effect : FxPat { public override string Label => "重ね模様 風の線"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => { float xx = x + (o * 4f + i * 13f) % u; Line(dc, b, xx - u * .3f, y, xx + u * .1f, y, 2f); Line(dc, b, xx + u * .1f, y, xx + u * .18f, y - u * .06f, 2f); }); } }

[VideoEffect("重ね模様 吹き出し", ["演出：質感・画風"], ["重ね模様 吹き出し", "小さな吹き出しの模様", "質感・画風", "重ね模様", "吹き出し", "模様", "パターン", "柄", "テクスチャ", "会話", "おしゃべり", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V791Effect : FxPat { public override string Label => "重ね模様 吹き出し"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { dc.FillEllipse(new Ellipse(new Vector2(x, y), u * .2f, u * .13f), b); Poly(dc, b, x - u * .05f, y + u * .1f, x - u * .15f, y + u * .22f, x + u * .03f, y + u * .1f); }); } }

[VideoEffect("重ね模様 回る四角", ["演出：質感・画風"], ["重ね模様 回る四角", "くるくる回る四角の模様", "質感・画風", "重ね模様", "回る四角", "模様", "パターン", "柄", "テクスチャ", "図形", "ポップ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V792Effect : FxPat { public override string Label => "重ね模様 回る四角"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => { float a = o * .1f + i, s = u * .14f; Poly(dc, b, x + MathF.Cos(a) * s, y + MathF.Sin(a) * s, x + MathF.Cos(a + 1.5708f) * s, y + MathF.Sin(a + 1.5708f) * s, x + MathF.Cos(a + 3.1416f) * s, y + MathF.Sin(a + 3.1416f) * s, x + MathF.Cos(a + 4.7124f) * s, y + MathF.Sin(a + 4.7124f) * s); }); } }

[VideoEffect("重ね模様 回る三角", ["演出：質感・画風"], ["重ね模様 回る三角", "くるくる回る三角の模様", "質感・画風", "重ね模様", "回る三角", "模様", "パターン", "柄", "テクスチャ", "図形", "ポップ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V793Effect : FxPat { public override string Label => "重ね模様 回る三角"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.4f; Each(l, t, r, bo, u, (x, y, i) => { float a = -o * .1f + i, s = u * .16f; Poly(dc, b, x + MathF.Cos(a) * s, y + MathF.Sin(a) * s, x + MathF.Cos(a + 2.094f) * s, y + MathF.Sin(a + 2.094f) * s, x + MathF.Cos(a + 4.189f) * s, y + MathF.Sin(a + 4.189f) * s); }); } }

[VideoEffect("重ね模様 月と星", ["演出：質感・画風"], ["重ね模様 月と星", "月と星が並ぶ模様", "質感・画風", "重ね模様", "月と星", "模様", "パターン", "柄", "テクスチャ", "夜", "おやすみ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V794Effect : FxPat { public override string Label => "重ね模様 月と星"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.8f; Each(l, t, r, bo, u, (x, y, i) => { if (i % 2 == 0) Poly(dc, b, P5.Moon(x, y, u * .15f)); else Poly(dc, b, P5.Star(x, y, u * .1f)); }); } }

[VideoEffect("重ね模様 かさ", ["演出：質感・画風"], ["重ね模様 かさ", "かさの模様を重ねる", "質感・画風", "重ね模様", "かさ", "模様", "パターン", "柄", "テクスチャ", "雨", "梅雨", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V795Effect : FxPat { public override string Label => "重ね模様 かさ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.7f; Each(l, t, r, bo, u, (x, y, i) => { var q = new List<float> { x, y }; for (int k = 0; k <= 8; k++) { float a = 3.1416f + k * .3927f; q.Add(x + MathF.Cos(a) * u * .2f); q.Add(y + MathF.Sin(a) * u * .14f); } Poly(dc, b, q.ToArray()); Line(dc, b, x, y, x, y + u * .18f, 2f); }); } }

[VideoEffect("重ね模様 キャンディ", ["演出：質感・画風"], ["重ね模様 キャンディ", "包み紙のキャンディの模様", "質感・画風", "重ね模様", "キャンディ", "模様", "パターン", "柄", "テクスチャ", "お菓子", "あめ", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V796Effect : FxPat { public override string Label => "重ね模様 キャンディ"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { Dot(dc, b, x, y, u * .08f); Poly(dc, b, x - u * .07f, y, x - u * .18f, y - u * .07f, x - u * .18f, y + u * .07f); Poly(dc, b, x + u * .07f, y, x + u * .18f, y - u * .07f, x + u * .18f, y + u * .07f); }); } }

[VideoEffect("重ね模様 おうち", ["演出：質感・画風"], ["重ね模様 おうち", "小さなおうちの模様", "質感・画風", "重ね模様", "おうち", "模様", "パターン", "柄", "テクスチャ", "家", "街", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V797Effect : FxPat { public override string Label => "重ね模様 おうち"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { Poly(dc, b, x - u * .15f, y, x, y - u * .14f, x + u * .15f, y); dc.FillRectangle(new Vortice.RawRectF(x - u * .11f, y, x + u * .11f, y + u * .14f), b); }); } }

[VideoEffect("重ね模様 もみの木", ["演出：質感・画風"], ["重ね模様 もみの木", "小さなもみの木の模様", "質感・画風", "重ね模様", "もみの木", "模様", "パターン", "柄", "テクスチャ", "クリスマス", "森", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V798Effect : FxPat { public override string Label => "重ね模様 もみの木"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { Poly(dc, b, x, y - u * .2f, x + u * .12f, y + u * .1f, x - u * .12f, y + u * .1f); dc.FillRectangle(new Vortice.RawRectF(x - u * .025f, y + u * .1f, x + u * .025f, y + u * .16f), b); }); } }

[VideoEffect("重ね模様 ロケット", ["演出：質感・画風"], ["重ね模様 ロケット", "飛んでいくロケットの模様", "質感・画風", "重ね模様", "ロケット", "模様", "パターン", "柄", "テクスチャ", "宇宙", "発射", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V799Effect : FxPat { public override string Label => "重ね模様 ロケット"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { float yy = y - (o * 3f + i * 11f) % u; dc.FillEllipse(new Ellipse(new Vector2(x, yy), u * .05f, u * .14f), b); Poly(dc, b, x - u * .05f, yy + u * .08f, x - u * .1f, yy + u * .16f, x - u * .03f, yy + u * .12f); Poly(dc, b, x + u * .05f, yy + u * .08f, x + u * .1f, yy + u * .16f, x + u * .03f, yy + u * .12f); }); } }

[VideoEffect("重ね模様 電球", ["演出：質感・画風"], ["重ね模様 電球", "小さな電球の模様", "質感・画風", "重ね模様", "電球", "模様", "パターン", "柄", "テクスチャ", "ひらめき", "アイデア", "演出"], IsAviUtlSupported = false)]
public sealed class Fx7V800Effect : FxPat { public override string Label => "重ね模様 電球"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u = st * 1.6f; Each(l, t, r, bo, u, (x, y, i) => { Dot(dc, b, x, y, u * .1f); dc.FillRectangle(new Vortice.RawRectF(x - u * .04f, y + u * .08f, x + u * .04f, y + u * .15f), b); }); } }
