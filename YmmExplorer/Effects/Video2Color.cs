using Vortice.Direct2D1;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 37〜52: 色調整

[VideoEffect("セピア", ["演出：色調整"], ["セピア", "sepia", "古い写真", "レトロ", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class SepiaEffect : FxVP { public override string Label => "セピア"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Out(.393f, .769f, .189f, 0, .349f, .686f, .168f, 0, .272f, .534f, .131f, 0), Math.Min(P(f), 1f))); }

[VideoEffect("モノクロ", ["演出：色調整"], ["モノクロ", "白黒", "グレースケール", "monochrome", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class MonochromeEffect : FxVP { public override string Label => "モノクロ"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Out(.2126f, .7152f, .0722f, 0, .2126f, .7152f, .0722f, 0, .2126f, .7152f, .0722f, 0), Math.Min(P(f), 1f))); }

[VideoEffect("ネガ反転", ["演出：色調整"], ["ネガ", "反転", "色反転", "invert", "negative", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class NegativeEffect : FxVP { public override string Label => "ネガ反転"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Out(-1, 0, 0, 1, 0, -1, 0, 1, 0, 0, -1, 1), Math.Min(P(f), 1f))); }

[VideoEffect("色相を変える", ["演出：色調整"], ["色相", "hue", "色を変える", "色味", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class HueShiftEffect : FxVA
{
    public override string Label => "色相を変える";
    public HueShiftEffect() { Speed.Values[0].Value = 0; }
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Hue((Z(f) + Sp(f) * f.T * 0.5f) * MathF.Tau), Math.Min(P(f), 1f)));
}

[VideoEffect("ビビッド（鮮やかに）", ["演出：色調整"], ["ビビッド", "鮮やか", "彩度", "vivid", "saturation", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class VividEffect : FxVP { public override string Label => "ビビッド（鮮やかに）"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, FxKit.Tone(0f, 1f + P(f) * 0.12f, 1f + P(f) * 0.9f)); }

[VideoEffect("夕焼けトーン", ["演出：色調整"], ["夕焼け", "夕方", "オレンジ", "暖色", "sunset", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class SunsetToneEffect : FxVP { public override string Label => "夕焼けトーン"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Out(1.12f, .12f, 0, .04f, .03f, .78f, .03f, .01f, 0, .08f, .55f, -.01f), Math.Min(P(f), 1f))); }

[VideoEffect("昼を夜にする", ["演出：色調整"], ["夜", "デイフォーナイト", "day for night", "暗く", "青い", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class DayForNightEffect : FxVP { public override string Label => "昼を夜にする"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Out(.18f, .12f, .03f, -.01f, .1f, .28f, .07f, 0, .14f, .28f, .42f, .03f), Math.Min(P(f), 1f))); }

[VideoEffect("シネマトーン", ["演出：色調整"], ["シネマ", "映画", "ティールオレンジ", "teal orange", "カラーグレーディング", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class CinemaToneEffect : FxVP { public override string Label => "シネマトーン"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Out(1.22f, -.06f, -.06f, -.05f, -.03f, 1.06f, -.03f, -.01f, -.1f, -.08f, 1.08f, .08f), Math.Min(P(f), 1f))); }

[VideoEffect("昭和の写真", ["演出：色調整"], ["昭和", "退色", "色あせ", "古いカラー写真", "レトロ", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class FadedPhotoEffect : FxVP { public override string Label => "昭和の写真"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Out(.72f, .14f, .03f, .1f, .08f, .68f, .06f, .08f, .05f, .12f, .52f, .09f), Math.Min(P(f), 1f))); }

[VideoEffect("カラーオーバーレイ", ["演出：色調整"], ["カラーオーバーレイ", "色を塗る", "塗りつぶし", "単色", "tint", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class ColorOverlayEffect : FxVC
{
    public override string Label => "カラーオーバーレイ";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color);
        return k.Matrix("m", i, Mx.Mix(Mx.Out(0, 0, 0, c.R, 0, 0, 0, c.G, 0, 0, 0, c.B), Math.Min(P(f), 1f)));
    }
}

[VideoEffect("2色グラデーション", ["演出：色調整"], ["デュオトーン", "duotone", "2色", "グラデーションマップ", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class DuotoneEffect : FxVCC
{
    public override string Label => "2色グラデーション";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var a = FxKit.Rgb(Color); var b = FxKit.Rgb(Color2);
        float dr = b.R - a.R, dg = b.G - a.G, db = b.B - a.B;
        return k.Matrix("m", i, Mx.Mix(Mx.Out(dr * .2126f, dr * .7152f, dr * .0722f, a.R, dg * .2126f, dg * .7152f, dg * .0722f, a.G, db * .2126f, db * .7152f, db * .0722f, a.B), Math.Min(P(f), 1f)));
    }
}

[VideoEffect("ポスタリゼーション", ["演出：色調整"], ["ポスタリゼーション", "posterize", "階調", "ポスター", "色数を減らす", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class PosterizeEffect : FxVP
{
    public override string Label => "ポスタリゼーション";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        int n = Math.Clamp((int)MathF.Round(16f - Math.Min(P(f), 1f) * 13f), 2, 16);
        return k.Builtin("p", "Posterize", i, ("RedValueCount", n), ("GreenValueCount", n), ("BlueValueCount", n));
    }
}

[VideoEffect("露出を上げる", ["演出：色調整"], ["露出", "白飛び", "明るく", "exposure", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class ExposureEffect : FxVP
{
    public override string Label => "露出を上げる";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) { float g = MathF.Pow(2f, P(f)); return k.Matrix("m", i, Mx.Out(g, 0, 0, 0, 0, g, 0, 0, 0, 0, g, 0)); }
}

[VideoEffect("色チャンネル入れ替え", ["演出：色調整"], ["チャンネル", "RGB", "入れ替え", "swap", "不思議な色", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class ChannelSwapEffect : FxVP { public override string Label => "色チャンネル入れ替え"; internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f) => k.Matrix("m", i, Mx.Mix(Mx.Out(0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0), Math.Min(P(f), 1f))); }

[VideoEffect("虹色に変わる", ["演出：色調整"], ["虹", "レインボー", "rainbow", "ゲーミング", "色が変わる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class RainbowCycleEffect : FxVA
{
    public override string Label => "虹色に変わる";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        float p = Math.Min(P(f), 1f), a = (Sp(f) * f.T * 0.8f + Z(f)) * MathF.Tau;
        var m = Mx.Mix(Mx.Hue(a), p);
        return k.Matrix("m", k.Matrix("s", i, FxKit.Tone(0f, 1f, 1f + p * 0.8f)), m);
    }
}

[VideoEffect("カラーフラッシュ", ["演出：色調整"], ["フラッシュ", "点滅", "光る", "flash", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class ColorFlashEffect : FxVAC
{
    public override string Label => "カラーフラッシュ";
    internal override ID2D1Image Build(FxKit k, ID2D1Image i, in FxFrame f)
    {
        var c = FxKit.Rgb(Color);
        float ph = f.T * MathF.Max(0.01f, MathF.Abs(Sp(f))) * 2f; ph -= MathF.Floor(ph);
        float q = MathF.Pow(1f - ph, 1f + Z(f) * 6f) * Math.Min(P(f), 1f);
        return k.Matrix("m", i, Mx.Mix(Mx.Out(0, 0, 0, c.R, 0, 0, 0, c.G, 0, 0, 0, c.B), q));
    }
}
