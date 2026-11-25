using System.Numerics;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 映像 201〜500(tools で生成)

[VideoEffect("色味 赤み 弱", ["演出：色調整"], ["色味 赤み 弱", "全体を赤みに弱めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V201Effect : FxGrade { public override string Label => "色味 赤み 弱"; private protected override float[] M => Gd.G(1f,1f,0f,1.08f,0.96f,0.952f); }

[VideoEffect("色味 赤み 中", ["演出：色調整"], ["色味 赤み 中", "全体を赤みに中めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V202Effect : FxGrade { public override string Label => "色味 赤み 中"; private protected override float[] M => Gd.G(1f,1f,0f,1.16f,0.92f,0.904f); }

[VideoEffect("色味 赤み 強", ["演出：色調整"], ["色味 赤み 強", "全体を赤みに強めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V203Effect : FxGrade { public override string Label => "色味 赤み 強"; private protected override float[] M => Gd.G(1f,1f,0f,1.28f,0.86f,0.832f); }

[VideoEffect("色味 橙み 弱", ["演出：色調整"], ["色味 橙み 弱", "全体を橙みに弱めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V204Effect : FxGrade { public override string Label => "色味 橙み 弱"; private protected override float[] M => Gd.G(1f,1f,0f,1.08f,1.008f,0.936f); }

[VideoEffect("色味 橙み 中", ["演出：色調整"], ["色味 橙み 中", "全体を橙みに中めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V205Effect : FxGrade { public override string Label => "色味 橙み 中"; private protected override float[] M => Gd.G(1f,1f,0f,1.16f,1.016f,0.872f); }

[VideoEffect("色味 橙み 強", ["演出：色調整"], ["色味 橙み 強", "全体を橙みに強めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V206Effect : FxGrade { public override string Label => "色味 橙み 強"; private protected override float[] M => Gd.G(1f,1f,0f,1.28f,1.028f,0.776f); }

[VideoEffect("色味 黄み 弱", ["演出：色調整"], ["色味 黄み 弱", "全体を黄みに弱めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V207Effect : FxGrade { public override string Label => "色味 黄み 弱"; private protected override float[] M => Gd.G(1f,1f,0f,1.056f,1.048f,0.92f); }

[VideoEffect("色味 黄み 中", ["演出：色調整"], ["色味 黄み 中", "全体を黄みに中めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V208Effect : FxGrade { public override string Label => "色味 黄み 中"; private protected override float[] M => Gd.G(1f,1f,0f,1.112f,1.096f,0.84f); }

[VideoEffect("色味 黄み 強", ["演出：色調整"], ["色味 黄み 強", "全体を黄みに強めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V209Effect : FxGrade { public override string Label => "色味 黄み 強"; private protected override float[] M => Gd.G(1f,1f,0f,1.196f,1.168f,0.72f); }

[VideoEffect("色味 緑み 弱", ["演出：色調整"], ["色味 緑み 弱", "全体を緑みに弱めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V210Effect : FxGrade { public override string Label => "色味 緑み 弱"; private protected override float[] M => Gd.G(1f,1f,0f,0.96f,1.08f,0.96f); }

[VideoEffect("色味 緑み 中", ["演出：色調整"], ["色味 緑み 中", "全体を緑みに中めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V211Effect : FxGrade { public override string Label => "色味 緑み 中"; private protected override float[] M => Gd.G(1f,1f,0f,0.92f,1.16f,0.92f); }

[VideoEffect("色味 緑み 強", ["演出：色調整"], ["色味 緑み 強", "全体を緑みに強めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V212Effect : FxGrade { public override string Label => "色味 緑み 強"; private protected override float[] M => Gd.G(1f,1f,0f,0.86f,1.28f,0.86f); }

[VideoEffect("色味 青緑み 弱", ["演出：色調整"], ["色味 青緑み 弱", "全体を青緑みに弱めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V213Effect : FxGrade { public override string Label => "色味 青緑み 弱"; private protected override float[] M => Gd.G(1f,1f,0f,0.936f,1.04f,1.048f); }

[VideoEffect("色味 青緑み 中", ["演出：色調整"], ["色味 青緑み 中", "全体を青緑みに中めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V214Effect : FxGrade { public override string Label => "色味 青緑み 中"; private protected override float[] M => Gd.G(1f,1f,0f,0.872f,1.08f,1.096f); }

[VideoEffect("色味 青緑み 強", ["演出：色調整"], ["色味 青緑み 強", "全体を青緑みに強めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V215Effect : FxGrade { public override string Label => "色味 青緑み 強"; private protected override float[] M => Gd.G(1f,1f,0f,0.776f,1.14f,1.168f); }

[VideoEffect("色味 青み 弱", ["演出：色調整"], ["色味 青み 弱", "全体を青みに弱めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V216Effect : FxGrade { public override string Label => "色味 青み 弱"; private protected override float[] M => Gd.G(1f,1f,0f,0.944f,0.992f,1.08f); }

[VideoEffect("色味 青み 中", ["演出：色調整"], ["色味 青み 中", "全体を青みに中めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V217Effect : FxGrade { public override string Label => "色味 青み 中"; private protected override float[] M => Gd.G(1f,1f,0f,0.888f,0.984f,1.16f); }

[VideoEffect("色味 青み 強", ["演出：色調整"], ["色味 青み 強", "全体を青みに強めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V218Effect : FxGrade { public override string Label => "色味 青み 強"; private protected override float[] M => Gd.G(1f,1f,0f,0.804f,0.972f,1.28f); }

[VideoEffect("色味 紫み 弱", ["演出：色調整"], ["色味 紫み 弱", "全体を紫みに弱めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V219Effect : FxGrade { public override string Label => "色味 紫み 弱"; private protected override float[] M => Gd.G(1f,1f,0f,1.04f,0.944f,1.08f); }

[VideoEffect("色味 紫み 中", ["演出：色調整"], ["色味 紫み 中", "全体を紫みに中めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V220Effect : FxGrade { public override string Label => "色味 紫み 中"; private protected override float[] M => Gd.G(1f,1f,0f,1.08f,0.888f,1.16f); }

[VideoEffect("色味 紫み 強", ["演出：色調整"], ["色味 紫み 強", "全体を紫みに強めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V221Effect : FxGrade { public override string Label => "色味 紫み 強"; private protected override float[] M => Gd.G(1f,1f,0f,1.14f,0.804f,1.28f); }

[VideoEffect("色味 桃色み 弱", ["演出：色調整"], ["色味 桃色み 弱", "全体を桃色みに弱めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V222Effect : FxGrade { public override string Label => "色味 桃色み 弱"; private protected override float[] M => Gd.G(1f,1f,0f,1.08f,0.96f,1.024f); }

[VideoEffect("色味 桃色み 中", ["演出：色調整"], ["色味 桃色み 中", "全体を桃色みに中めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V223Effect : FxGrade { public override string Label => "色味 桃色み 中"; private protected override float[] M => Gd.G(1f,1f,0f,1.16f,0.92f,1.048f); }

[VideoEffect("色味 桃色み 強", ["演出：色調整"], ["色味 桃色み 強", "全体を桃色みに強めに寄せる", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V224Effect : FxGrade { public override string Label => "色味 桃色み 強"; private protected override float[] M => Gd.G(1f,1f,0f,1.28f,0.86f,1.084f); }

[VideoEffect("二色 紺と桜", ["演出：色調整"], ["二色 紺と桜", "暗い所と明るい所を紺と桜の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V225Effect : FxGrade { public override string Label => "二色 紺と桜"; private protected override float[] M => Gd.D(0x141B3D,0xFFC4D6); }

[VideoEffect("二色 紺と金", ["演出：色調整"], ["二色 紺と金", "暗い所と明るい所を紺と金の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V226Effect : FxGrade { public override string Label => "二色 紺と金"; private protected override float[] M => Gd.D(0x101A3A,0xF2D27A); }

[VideoEffect("二色 紫と水色", ["演出：色調整"], ["二色 紫と水色", "暗い所と明るい所を紫と水色の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V227Effect : FxGrade { public override string Label => "二色 紫と水色"; private protected override float[] M => Gd.D(0x2A1458,0x8FE3FF); }

[VideoEffect("二色 紫と桃", ["演出：色調整"], ["二色 紫と桃", "暗い所と明るい所を紫と桃の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V228Effect : FxGrade { public override string Label => "二色 紫と桃"; private protected override float[] M => Gd.D(0x2B1050,0xFF9EC7); }

[VideoEffect("二色 藍と白", ["演出：色調整"], ["二色 藍と白", "暗い所と明るい所を藍と白の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V229Effect : FxGrade { public override string Label => "二色 藍と白"; private protected override float[] M => Gd.D(0x12264F,0xF4F7FF); }

[VideoEffect("二色 深緑と若草", ["演出：色調整"], ["二色 深緑と若草", "暗い所と明るい所を深緑と若草の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V230Effect : FxGrade { public override string Label => "二色 深緑と若草"; private protected override float[] M => Gd.D(0x0E2E22,0xC9EE8F); }

[VideoEffect("二色 深緑と金", ["演出：色調整"], ["二色 深緑と金", "暗い所と明るい所を深緑と金の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V231Effect : FxGrade { public override string Label => "二色 深緑と金"; private protected override float[] M => Gd.D(0x10291F,0xE8D27A); }

[VideoEffect("二色 茶と生成り", ["演出：色調整"], ["二色 茶と生成り", "暗い所と明るい所を茶と生成りの2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V232Effect : FxGrade { public override string Label => "二色 茶と生成り"; private protected override float[] M => Gd.D(0x3A2416,0xF5E8D2); }

[VideoEffect("二色 焦げ茶と橙", ["演出：色調整"], ["二色 焦げ茶と橙", "暗い所と明るい所を焦げ茶と橙の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V233Effect : FxGrade { public override string Label => "二色 焦げ茶と橙"; private protected override float[] M => Gd.D(0x2C160C,0xFFB45E); }

[VideoEffect("二色 赤と黄", ["演出：色調整"], ["二色 赤と黄", "暗い所と明るい所を赤と黄の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V234Effect : FxGrade { public override string Label => "二色 赤と黄"; private protected override float[] M => Gd.D(0x7A1020,0xFFE27A); }

[VideoEffect("二色 赤と白", ["演出：色調整"], ["二色 赤と白", "暗い所と明るい所を赤と白の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V235Effect : FxGrade { public override string Label => "二色 赤と白"; private protected override float[] M => Gd.D(0x6B0F1A,0xFFF1EC); }

[VideoEffect("二色 黒と赤", ["演出：色調整"], ["二色 黒と赤", "暗い所と明るい所を黒と赤の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V236Effect : FxGrade { public override string Label => "二色 黒と赤"; private protected override float[] M => Gd.D(0x0C0608,0xFF3D4F); }

[VideoEffect("二色 黒と青", ["演出：色調整"], ["二色 黒と青", "暗い所と明るい所を黒と青の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V237Effect : FxGrade { public override string Label => "二色 黒と青"; private protected override float[] M => Gd.D(0x05070F,0x38B6FF); }

[VideoEffect("二色 黒と緑", ["演出：色調整"], ["二色 黒と緑", "暗い所と明るい所を黒と緑の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V238Effect : FxGrade { public override string Label => "二色 黒と緑"; private protected override float[] M => Gd.D(0x040C07,0x39F58C); }

[VideoEffect("二色 黒と黄", ["演出：色調整"], ["二色 黒と黄", "暗い所と明るい所を黒と黄の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V239Effect : FxGrade { public override string Label => "二色 黒と黄"; private protected override float[] M => Gd.D(0x0B0A04,0xFFE03D); }

[VideoEffect("二色 黒と桃", ["演出：色調整"], ["二色 黒と桃", "暗い所と明るい所を黒と桃の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V240Effect : FxGrade { public override string Label => "二色 黒と桃"; private protected override float[] M => Gd.D(0x0E0509,0xFF5CA8); }

[VideoEffect("二色 黒と紫", ["演出：色調整"], ["二色 黒と紫", "暗い所と明るい所を黒と紫の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V241Effect : FxGrade { public override string Label => "二色 黒と紫"; private protected override float[] M => Gd.D(0x08050F,0xB77BFF); }

[VideoEffect("二色 黒と橙", ["演出：色調整"], ["二色 黒と橙", "暗い所と明るい所を黒と橙の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V242Effect : FxGrade { public override string Label => "二色 黒と橙"; private protected override float[] M => Gd.D(0x0D0703,0xFF9A3C); }

[VideoEffect("二色 灰と白", ["演出：色調整"], ["二色 灰と白", "暗い所と明るい所を灰と白の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V243Effect : FxGrade { public override string Label => "二色 灰と白"; private protected override float[] M => Gd.D(0x34363B,0xF5F6F8); }

[VideoEffect("二色 灰と水色", ["演出：色調整"], ["二色 灰と水色", "暗い所と明るい所を灰と水色の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V244Effect : FxGrade { public override string Label => "二色 灰と水色"; private protected override float[] M => Gd.D(0x30363D,0xBDEBFF); }

[VideoEffect("二色 灰と桜", ["演出：色調整"], ["二色 灰と桜", "暗い所と明るい所を灰と桜の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V245Effect : FxGrade { public override string Label => "二色 灰と桜"; private protected override float[] M => Gd.D(0x3A3438,0xFFD6E2); }

[VideoEffect("二色 青と黄", ["演出：色調整"], ["二色 青と黄", "暗い所と明るい所を青と黄の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V246Effect : FxGrade { public override string Label => "二色 青と黄"; private protected override float[] M => Gd.D(0x123A8C,0xFFE872); }

[VideoEffect("二色 青と桃", ["演出：色調整"], ["二色 青と桃", "暗い所と明るい所を青と桃の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V247Effect : FxGrade { public override string Label => "二色 青と桃"; private protected override float[] M => Gd.D(0x1A3FA0,0xFFB0D0); }

[VideoEffect("二色 青緑と橙", ["演出：色調整"], ["二色 青緑と橙", "暗い所と明るい所を青緑と橙の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V248Effect : FxGrade { public override string Label => "二色 青緑と橙"; private protected override float[] M => Gd.D(0x0B3E46,0xFFB061); }

[VideoEffect("二色 青緑と白", ["演出：色調整"], ["二色 青緑と白", "暗い所と明るい所を青緑と白の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V249Effect : FxGrade { public override string Label => "二色 青緑と白"; private protected override float[] M => Gd.D(0x0A3F45,0xE9FFFB); }

[VideoEffect("二色 緑と黄", ["演出：色調整"], ["二色 緑と黄", "暗い所と明るい所を緑と黄の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V250Effect : FxGrade { public override string Label => "二色 緑と黄"; private protected override float[] M => Gd.D(0x12603A,0xF7F27A); }

[VideoEffect("二色 緑と桃", ["演出：色調整"], ["二色 緑と桃", "暗い所と明るい所を緑と桃の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V251Effect : FxGrade { public override string Label => "二色 緑と桃"; private protected override float[] M => Gd.D(0x124F3A,0xFFB7CF); }

[VideoEffect("二色 紅と金", ["演出：色調整"], ["二色 紅と金", "暗い所と明るい所を紅と金の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V252Effect : FxGrade { public override string Label => "二色 紅と金"; private protected override float[] M => Gd.D(0x5C0F24,0xF4CF6B); }

[VideoEffect("二色 紅と水色", ["演出：色調整"], ["二色 紅と水色", "暗い所と明るい所を紅と水色の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V253Effect : FxGrade { public override string Label => "二色 紅と水色"; private protected override float[] M => Gd.D(0x5A1030,0x9FE8FF); }

[VideoEffect("二色 紫と金", ["演出：色調整"], ["二色 紫と金", "暗い所と明るい所を紫と金の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V254Effect : FxGrade { public override string Label => "二色 紫と金"; private protected override float[] M => Gd.D(0x341258,0xF4D27A); }

[VideoEffect("二色 紫と橙", ["演出：色調整"], ["二色 紫と橙", "暗い所と明るい所を紫と橙の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V255Effect : FxGrade { public override string Label => "二色 紫と橙"; private protected override float[] M => Gd.D(0x35155C,0xFFAA5C); }

[VideoEffect("二色 藍と橙", ["演出：色調整"], ["二色 藍と橙", "暗い所と明るい所を藍と橙の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V256Effect : FxGrade { public override string Label => "二色 藍と橙"; private protected override float[] M => Gd.D(0x13245A,0xFFA45C); }

[VideoEffect("二色 藍と桃", ["演出：色調整"], ["二色 藍と桃", "暗い所と明るい所を藍と桃の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V257Effect : FxGrade { public override string Label => "二色 藍と桃"; private protected override float[] M => Gd.D(0x171F5C,0xFF9CC5); }

[VideoEffect("二色 海と砂", ["演出：色調整"], ["二色 海と砂", "暗い所と明るい所を海と砂の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V258Effect : FxGrade { public override string Label => "二色 海と砂"; private protected override float[] M => Gd.D(0x0D4560,0xF5E0B5); }

[VideoEffect("二色 森と霧", ["演出：色調整"], ["二色 森と霧", "暗い所と明るい所を森と霧の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V259Effect : FxGrade { public override string Label => "二色 森と霧"; private protected override float[] M => Gd.D(0x1C3B2C,0xDCE9E2); }

[VideoEffect("二色 夜と月", ["演出：色調整"], ["二色 夜と月", "暗い所と明るい所を夜と月の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V260Effect : FxGrade { public override string Label => "二色 夜と月"; private protected override float[] M => Gd.D(0x0A1230,0xF4EFC9); }

[VideoEffect("二色 炎と灰", ["演出：色調整"], ["二色 炎と灰", "暗い所と明るい所を炎と灰の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V261Effect : FxGrade { public override string Label => "二色 炎と灰"; private protected override float[] M => Gd.D(0x7A2408,0xFFE6B0); }

[VideoEffect("二色 氷と雪", ["演出：色調整"], ["二色 氷と雪", "暗い所と明るい所を氷と雪の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V262Effect : FxGrade { public override string Label => "二色 氷と雪"; private protected override float[] M => Gd.D(0x2B6A8F,0xF2FBFF); }

[VideoEffect("二色 土と草", ["演出：色調整"], ["二色 土と草", "暗い所と明るい所を土と草の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V263Effect : FxGrade { public override string Label => "二色 土と草"; private protected override float[] M => Gd.D(0x4A3620,0xCFE39A); }

[VideoEffect("二色 夕日と雲", ["演出：色調整"], ["二色 夕日と雲", "暗い所と明るい所を夕日と雲の2色にする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V264Effect : FxGrade { public override string Label => "二色 夕日と雲"; private protected override float[] M => Gd.D(0x8A2F3C,0xFFE3C2); }

[VideoEffect("色調 色なし寄り やわらか", ["演出：色調整"], ["色調 色なし寄り やわらか", "彩度を色なし寄り、明暗差をやわらかにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V265Effect : FxGrade { public override string Label => "色調 色なし寄り やわらか"; private protected override float[] M => Gd.G(0.25f,0.72f,0f); }

[VideoEffect("色調 色なし寄り ねむい", ["演出：色調整"], ["色調 色なし寄り ねむい", "彩度を色なし寄り、明暗差をねむいにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V266Effect : FxGrade { public override string Label => "色調 色なし寄り ねむい"; private protected override float[] M => Gd.G(0.25f,0.86f,0f); }

[VideoEffect("色調 色なし寄り ふつう", ["演出：色調整"], ["色調 色なし寄り ふつう", "彩度を色なし寄り、明暗差をふつうにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V267Effect : FxGrade { public override string Label => "色調 色なし寄り ふつう"; private protected override float[] M => Gd.G(0.25f,1f,0f); }

[VideoEffect("色調 色なし寄り くっきり", ["演出：色調整"], ["色調 色なし寄り くっきり", "彩度を色なし寄り、明暗差をくっきりにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V268Effect : FxGrade { public override string Label => "色調 色なし寄り くっきり"; private protected override float[] M => Gd.G(0.25f,1.14f,0f); }

[VideoEffect("色調 色なし寄り 硬い", ["演出：色調整"], ["色調 色なし寄り 硬い", "彩度を色なし寄り、明暗差を硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V269Effect : FxGrade { public override string Label => "色調 色なし寄り 硬い"; private protected override float[] M => Gd.G(0.25f,1.3f,0f); }

[VideoEffect("色調 色なし寄り 超硬い", ["演出：色調整"], ["色調 色なし寄り 超硬い", "彩度を色なし寄り、明暗差を超硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V270Effect : FxGrade { public override string Label => "色調 色なし寄り 超硬い"; private protected override float[] M => Gd.G(0.25f,1.52f,0f); }

[VideoEffect("色調 くすみ やわらか", ["演出：色調整"], ["色調 くすみ やわらか", "彩度をくすみ、明暗差をやわらかにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V271Effect : FxGrade { public override string Label => "色調 くすみ やわらか"; private protected override float[] M => Gd.G(0.55f,0.72f,0f); }

[VideoEffect("色調 くすみ ねむい", ["演出：色調整"], ["色調 くすみ ねむい", "彩度をくすみ、明暗差をねむいにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V272Effect : FxGrade { public override string Label => "色調 くすみ ねむい"; private protected override float[] M => Gd.G(0.55f,0.86f,0f); }

[VideoEffect("色調 くすみ ふつう", ["演出：色調整"], ["色調 くすみ ふつう", "彩度をくすみ、明暗差をふつうにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V273Effect : FxGrade { public override string Label => "色調 くすみ ふつう"; private protected override float[] M => Gd.G(0.55f,1f,0f); }

[VideoEffect("色調 くすみ くっきり", ["演出：色調整"], ["色調 くすみ くっきり", "彩度をくすみ、明暗差をくっきりにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V274Effect : FxGrade { public override string Label => "色調 くすみ くっきり"; private protected override float[] M => Gd.G(0.55f,1.14f,0f); }

[VideoEffect("色調 くすみ 硬い", ["演出：色調整"], ["色調 くすみ 硬い", "彩度をくすみ、明暗差を硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V275Effect : FxGrade { public override string Label => "色調 くすみ 硬い"; private protected override float[] M => Gd.G(0.55f,1.3f,0f); }

[VideoEffect("色調 くすみ 超硬い", ["演出：色調整"], ["色調 くすみ 超硬い", "彩度をくすみ、明暗差を超硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V276Effect : FxGrade { public override string Label => "色調 くすみ 超硬い"; private protected override float[] M => Gd.G(0.55f,1.52f,0f); }

[VideoEffect("色調 ひかえめ やわらか", ["演出：色調整"], ["色調 ひかえめ やわらか", "彩度をひかえめ、明暗差をやわらかにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V277Effect : FxGrade { public override string Label => "色調 ひかえめ やわらか"; private protected override float[] M => Gd.G(0.8f,0.72f,0f); }

[VideoEffect("色調 ひかえめ ねむい", ["演出：色調整"], ["色調 ひかえめ ねむい", "彩度をひかえめ、明暗差をねむいにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V278Effect : FxGrade { public override string Label => "色調 ひかえめ ねむい"; private protected override float[] M => Gd.G(0.8f,0.86f,0f); }

[VideoEffect("色調 ひかえめ ふつう", ["演出：色調整"], ["色調 ひかえめ ふつう", "彩度をひかえめ、明暗差をふつうにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V279Effect : FxGrade { public override string Label => "色調 ひかえめ ふつう"; private protected override float[] M => Gd.G(0.8f,1f,0f); }

[VideoEffect("色調 ひかえめ くっきり", ["演出：色調整"], ["色調 ひかえめ くっきり", "彩度をひかえめ、明暗差をくっきりにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V280Effect : FxGrade { public override string Label => "色調 ひかえめ くっきり"; private protected override float[] M => Gd.G(0.8f,1.14f,0f); }

[VideoEffect("色調 ひかえめ 硬い", ["演出：色調整"], ["色調 ひかえめ 硬い", "彩度をひかえめ、明暗差を硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V281Effect : FxGrade { public override string Label => "色調 ひかえめ 硬い"; private protected override float[] M => Gd.G(0.8f,1.3f,0f); }

[VideoEffect("色調 ひかえめ 超硬い", ["演出：色調整"], ["色調 ひかえめ 超硬い", "彩度をひかえめ、明暗差を超硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V282Effect : FxGrade { public override string Label => "色調 ひかえめ 超硬い"; private protected override float[] M => Gd.G(0.8f,1.52f,0f); }

[VideoEffect("色調 少し鮮やか やわらか", ["演出：色調整"], ["色調 少し鮮やか やわらか", "彩度を少し鮮やか、明暗差をやわらかにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V283Effect : FxGrade { public override string Label => "色調 少し鮮やか やわらか"; private protected override float[] M => Gd.G(1.18f,0.72f,0f); }

[VideoEffect("色調 少し鮮やか ねむい", ["演出：色調整"], ["色調 少し鮮やか ねむい", "彩度を少し鮮やか、明暗差をねむいにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V284Effect : FxGrade { public override string Label => "色調 少し鮮やか ねむい"; private protected override float[] M => Gd.G(1.18f,0.86f,0f); }

[VideoEffect("色調 少し鮮やか ふつう", ["演出：色調整"], ["色調 少し鮮やか ふつう", "彩度を少し鮮やか、明暗差をふつうにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V285Effect : FxGrade { public override string Label => "色調 少し鮮やか ふつう"; private protected override float[] M => Gd.G(1.18f,1f,0f); }

[VideoEffect("色調 少し鮮やか くっきり", ["演出：色調整"], ["色調 少し鮮やか くっきり", "彩度を少し鮮やか、明暗差をくっきりにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V286Effect : FxGrade { public override string Label => "色調 少し鮮やか くっきり"; private protected override float[] M => Gd.G(1.18f,1.14f,0f); }

[VideoEffect("色調 少し鮮やか 硬い", ["演出：色調整"], ["色調 少し鮮やか 硬い", "彩度を少し鮮やか、明暗差を硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V287Effect : FxGrade { public override string Label => "色調 少し鮮やか 硬い"; private protected override float[] M => Gd.G(1.18f,1.3f,0f); }

[VideoEffect("色調 少し鮮やか 超硬い", ["演出：色調整"], ["色調 少し鮮やか 超硬い", "彩度を少し鮮やか、明暗差を超硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V288Effect : FxGrade { public override string Label => "色調 少し鮮やか 超硬い"; private protected override float[] M => Gd.G(1.18f,1.52f,0f); }

[VideoEffect("色調 鮮やか やわらか", ["演出：色調整"], ["色調 鮮やか やわらか", "彩度を鮮やか、明暗差をやわらかにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V289Effect : FxGrade { public override string Label => "色調 鮮やか やわらか"; private protected override float[] M => Gd.G(1.42f,0.72f,0f); }

[VideoEffect("色調 鮮やか ねむい", ["演出：色調整"], ["色調 鮮やか ねむい", "彩度を鮮やか、明暗差をねむいにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V290Effect : FxGrade { public override string Label => "色調 鮮やか ねむい"; private protected override float[] M => Gd.G(1.42f,0.86f,0f); }

[VideoEffect("色調 鮮やか ふつう", ["演出：色調整"], ["色調 鮮やか ふつう", "彩度を鮮やか、明暗差をふつうにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V291Effect : FxGrade { public override string Label => "色調 鮮やか ふつう"; private protected override float[] M => Gd.G(1.42f,1f,0f); }

[VideoEffect("色調 鮮やか くっきり", ["演出：色調整"], ["色調 鮮やか くっきり", "彩度を鮮やか、明暗差をくっきりにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V292Effect : FxGrade { public override string Label => "色調 鮮やか くっきり"; private protected override float[] M => Gd.G(1.42f,1.14f,0f); }

[VideoEffect("色調 鮮やか 硬い", ["演出：色調整"], ["色調 鮮やか 硬い", "彩度を鮮やか、明暗差を硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V293Effect : FxGrade { public override string Label => "色調 鮮やか 硬い"; private protected override float[] M => Gd.G(1.42f,1.3f,0f); }

[VideoEffect("色調 鮮やか 超硬い", ["演出：色調整"], ["色調 鮮やか 超硬い", "彩度を鮮やか、明暗差を超硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V294Effect : FxGrade { public override string Label => "色調 鮮やか 超硬い"; private protected override float[] M => Gd.G(1.42f,1.52f,0f); }

[VideoEffect("色調 ビビッド やわらか", ["演出：色調整"], ["色調 ビビッド やわらか", "彩度をビビッド、明暗差をやわらかにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V295Effect : FxGrade { public override string Label => "色調 ビビッド やわらか"; private protected override float[] M => Gd.G(1.8f,0.72f,0f); }

[VideoEffect("色調 ビビッド ねむい", ["演出：色調整"], ["色調 ビビッド ねむい", "彩度をビビッド、明暗差をねむいにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V296Effect : FxGrade { public override string Label => "色調 ビビッド ねむい"; private protected override float[] M => Gd.G(1.8f,0.86f,0f); }

[VideoEffect("色調 ビビッド ふつう", ["演出：色調整"], ["色調 ビビッド ふつう", "彩度をビビッド、明暗差をふつうにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V297Effect : FxGrade { public override string Label => "色調 ビビッド ふつう"; private protected override float[] M => Gd.G(1.8f,1f,0f); }

[VideoEffect("色調 ビビッド くっきり", ["演出：色調整"], ["色調 ビビッド くっきり", "彩度をビビッド、明暗差をくっきりにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V298Effect : FxGrade { public override string Label => "色調 ビビッド くっきり"; private protected override float[] M => Gd.G(1.8f,1.14f,0f); }

[VideoEffect("色調 ビビッド 硬い", ["演出：色調整"], ["色調 ビビッド 硬い", "彩度をビビッド、明暗差を硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V299Effect : FxGrade { public override string Label => "色調 ビビッド 硬い"; private protected override float[] M => Gd.G(1.8f,1.3f,0f); }

[VideoEffect("色調 ビビッド 超硬い", ["演出：色調整"], ["色調 ビビッド 超硬い", "彩度をビビッド、明暗差を超硬いにする", "色調整", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V300Effect : FxGrade { public override string Label => "色調 ビビッド 超硬い"; private protected override float[] M => Gd.G(1.8f,1.52f,0f); }

[VideoEffect("動き 左右 なめらか", ["演出：動き"], ["動き 左右 なめらか", "左右方向へなめらかリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V301Effect : FxMove { public override string Label => "動き 左右 なめらか"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(S(t*2f))*.07f*a,0); }

[VideoEffect("動き 左右 速い", ["演出：動き"], ["動き 左右 速い", "左右方向へ速いリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V302Effect : FxMove { public override string Label => "動き 左右 速い"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(S(t*6.5f))*.07f*a,0); }

[VideoEffect("動き 左右 ゆっくり", ["演出：動き"], ["動き 左右 ゆっくり", "左右方向へゆっくりリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V303Effect : FxMove { public override string Label => "動き 左右 ゆっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(S(t*.9f))*.07f*a,0); }

[VideoEffect("動き 左右 カクカク", ["演出：動き"], ["動き 左右 カクカク", "左右方向へカクカクリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V304Effect : FxMove { public override string Label => "動き 左右 カクカク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Round(S(t*2.5f)*3f)/3f)*.07f*a,0); }

[VideoEffect("動き 左右 三角波", ["演出：動き"], ["動き 左右 三角波", "左右方向へ三角波リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V305Effect : FxMove { public override string Label => "動き 左右 三角波"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Asin(S(t*3f))*.637f)*.07f*a,0); }

[VideoEffect("動き 左右 パルス", ["演出：動き"], ["動き 左右 パルス", "左右方向へパルスリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V306Effect : FxMove { public override string Label => "動き 左右 パルス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.07f*a,0); }

[VideoEffect("動き 左右 弾む", ["演出：動き"], ["動き 左右 弾む", "左右方向へ弾むリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V307Effect : FxMove { public override string Label => "動き 左右 弾む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Abs(S(t*3f)))*.07f*a,0); }

[VideoEffect("動き 左右 ふらつき", ["演出：動き"], ["動き 左右 ふらつき", "左右方向へふらつきリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V308Effect : FxMove { public override string Label => "動き 左右 ふらつき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(N(t*1.6f,3))*.07f*a,0); }

[VideoEffect("動き 左右 二段", ["演出：動き"], ["動き 左右 二段", "左右方向へ二段リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V309Effect : FxMove { public override string Label => "動き 左右 二段"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,((S(t*2f)+S(t*5f)*.5f)*.67f)*.07f*a,0); }

[VideoEffect("動き 左右 間欠", ["演出：動き"], ["動き 左右 間欠", "左右方向へ間欠リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V310Effect : FxMove { public override string Label => "動き 左右 間欠"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.07f*a,0); }

[VideoEffect("動き 上下 なめらか", ["演出：動き"], ["動き 上下 なめらか", "上下方向へなめらかリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V311Effect : FxMove { public override string Label => "動き 上下 なめらか"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(S(t*2f))*.07f*a); }

[VideoEffect("動き 上下 速い", ["演出：動き"], ["動き 上下 速い", "上下方向へ速いリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V312Effect : FxMove { public override string Label => "動き 上下 速い"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(S(t*6.5f))*.07f*a); }

[VideoEffect("動き 上下 ゆっくり", ["演出：動き"], ["動き 上下 ゆっくり", "上下方向へゆっくりリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V313Effect : FxMove { public override string Label => "動き 上下 ゆっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(S(t*.9f))*.07f*a); }

[VideoEffect("動き 上下 カクカク", ["演出：動き"], ["動き 上下 カクカク", "上下方向へカクカクリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V314Effect : FxMove { public override string Label => "動き 上下 カクカク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(MathF.Round(S(t*2.5f)*3f)/3f)*.07f*a); }

[VideoEffect("動き 上下 三角波", ["演出：動き"], ["動き 上下 三角波", "上下方向へ三角波リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V315Effect : FxMove { public override string Label => "動き 上下 三角波"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(MathF.Asin(S(t*3f))*.637f)*.07f*a); }

[VideoEffect("動き 上下 パルス", ["演出：動き"], ["動き 上下 パルス", "上下方向へパルスリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V316Effect : FxMove { public override string Label => "動き 上下 パルス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.07f*a); }

[VideoEffect("動き 上下 弾む", ["演出：動き"], ["動き 上下 弾む", "上下方向へ弾むリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V317Effect : FxMove { public override string Label => "動き 上下 弾む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(MathF.Abs(S(t*3f)))*.07f*a); }

[VideoEffect("動き 上下 ふらつき", ["演出：動き"], ["動き 上下 ふらつき", "上下方向へふらつきリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V318Effect : FxMove { public override string Label => "動き 上下 ふらつき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(N(t*1.6f,3))*.07f*a); }

[VideoEffect("動き 上下 二段", ["演出：動き"], ["動き 上下 二段", "上下方向へ二段リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V319Effect : FxMove { public override string Label => "動き 上下 二段"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,((S(t*2f)+S(t*5f)*.5f)*.67f)*.07f*a); }

[VideoEffect("動き 上下 間欠", ["演出：動き"], ["動き 上下 間欠", "上下方向へ間欠リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V320Effect : FxMove { public override string Label => "動き 上下 間欠"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,0,(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.07f*a); }

[VideoEffect("動き 斜め なめらか", ["演出：動き"], ["動き 斜め なめらか", "斜め方向へなめらかリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V321Effect : FxMove { public override string Label => "動き 斜め なめらか"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(S(t*2f))*.05f*a,(S(t*2f))*.05f*a); }

[VideoEffect("動き 斜め 速い", ["演出：動き"], ["動き 斜め 速い", "斜め方向へ速いリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V322Effect : FxMove { public override string Label => "動き 斜め 速い"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(S(t*6.5f))*.05f*a,(S(t*6.5f))*.05f*a); }

[VideoEffect("動き 斜め ゆっくり", ["演出：動き"], ["動き 斜め ゆっくり", "斜め方向へゆっくりリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V323Effect : FxMove { public override string Label => "動き 斜め ゆっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(S(t*.9f))*.05f*a,(S(t*.9f))*.05f*a); }

[VideoEffect("動き 斜め カクカク", ["演出：動き"], ["動き 斜め カクカク", "斜め方向へカクカクリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V324Effect : FxMove { public override string Label => "動き 斜め カクカク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Round(S(t*2.5f)*3f)/3f)*.05f*a,(MathF.Round(S(t*2.5f)*3f)/3f)*.05f*a); }

[VideoEffect("動き 斜め 三角波", ["演出：動き"], ["動き 斜め 三角波", "斜め方向へ三角波リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V325Effect : FxMove { public override string Label => "動き 斜め 三角波"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Asin(S(t*3f))*.637f)*.05f*a,(MathF.Asin(S(t*3f))*.637f)*.05f*a); }

[VideoEffect("動き 斜め パルス", ["演出：動き"], ["動き 斜め パルス", "斜め方向へパルスリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V326Effect : FxMove { public override string Label => "動き 斜め パルス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.05f*a,(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.05f*a); }

[VideoEffect("動き 斜め 弾む", ["演出：動き"], ["動き 斜め 弾む", "斜め方向へ弾むリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V327Effect : FxMove { public override string Label => "動き 斜め 弾む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(MathF.Abs(S(t*3f)))*.05f*a,(MathF.Abs(S(t*3f)))*.05f*a); }

[VideoEffect("動き 斜め ふらつき", ["演出：動き"], ["動き 斜め ふらつき", "斜め方向へふらつきリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V328Effect : FxMove { public override string Label => "動き 斜め ふらつき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(N(t*1.6f,3))*.05f*a,(N(t*1.6f,3))*.05f*a); }

[VideoEffect("動き 斜め 二段", ["演出：動き"], ["動き 斜め 二段", "斜め方向へ二段リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V329Effect : FxMove { public override string Label => "動き 斜め 二段"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,((S(t*2f)+S(t*5f)*.5f)*.67f)*.05f*a,((S(t*2f)+S(t*5f)*.5f)*.67f)*.05f*a); }

[VideoEffect("動き 斜め 間欠", ["演出：動き"], ["動き 斜め 間欠", "斜め方向へ間欠リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V330Effect : FxMove { public override string Label => "動き 斜め 間欠"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Mo(f,(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.05f*a,(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.05f*a); }

[VideoEffect("動き 回転 なめらか", ["演出：動き"], ["動き 回転 なめらか", "回転方向へなめらかリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V331Effect : FxMove { public override string Label => "動き 回転 なめらか"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(S(t*2f))*.16f*a); }

[VideoEffect("動き 回転 速い", ["演出：動き"], ["動き 回転 速い", "回転方向へ速いリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V332Effect : FxMove { public override string Label => "動き 回転 速い"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(S(t*6.5f))*.16f*a); }

[VideoEffect("動き 回転 ゆっくり", ["演出：動き"], ["動き 回転 ゆっくり", "回転方向へゆっくりリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V333Effect : FxMove { public override string Label => "動き 回転 ゆっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(S(t*.9f))*.16f*a); }

[VideoEffect("動き 回転 カクカク", ["演出：動き"], ["動き 回転 カクカク", "回転方向へカクカクリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V334Effect : FxMove { public override string Label => "動き 回転 カクカク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(MathF.Round(S(t*2.5f)*3f)/3f)*.16f*a); }

[VideoEffect("動き 回転 三角波", ["演出：動き"], ["動き 回転 三角波", "回転方向へ三角波リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V335Effect : FxMove { public override string Label => "動き 回転 三角波"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(MathF.Asin(S(t*3f))*.637f)*.16f*a); }

[VideoEffect("動き 回転 パルス", ["演出：動き"], ["動き 回転 パルス", "回転方向へパルスリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V336Effect : FxMove { public override string Label => "動き 回転 パルス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.16f*a); }

[VideoEffect("動き 回転 弾む", ["演出：動き"], ["動き 回転 弾む", "回転方向へ弾むリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V337Effect : FxMove { public override string Label => "動き 回転 弾む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(MathF.Abs(S(t*3f)))*.16f*a); }

[VideoEffect("動き 回転 ふらつき", ["演出：動き"], ["動き 回転 ふらつき", "回転方向へふらつきリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V338Effect : FxMove { public override string Label => "動き 回転 ふらつき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(N(t*1.6f,3))*.16f*a); }

[VideoEffect("動き 回転 二段", ["演出：動き"], ["動き 回転 二段", "回転方向へ二段リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V339Effect : FxMove { public override string Label => "動き 回転 二段"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,((S(t*2f)+S(t*5f)*.5f)*.67f)*.16f*a); }

[VideoEffect("動き 回転 間欠", ["演出：動き"], ["動き 回転 間欠", "回転方向へ間欠リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V340Effect : FxMove { public override string Label => "動き 回転 間欠"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ro(f,(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.16f*a); }

[VideoEffect("動き 拡大 なめらか", ["演出：動き"], ["動き 拡大 なめらか", "拡大方向へなめらかリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V341Effect : FxMove { public override string Label => "動き 拡大 なめらか"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(S(t*2f))*.12f*a,1f+(S(t*2f))*.12f*a); }

[VideoEffect("動き 拡大 速い", ["演出：動き"], ["動き 拡大 速い", "拡大方向へ速いリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V342Effect : FxMove { public override string Label => "動き 拡大 速い"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(S(t*6.5f))*.12f*a,1f+(S(t*6.5f))*.12f*a); }

[VideoEffect("動き 拡大 ゆっくり", ["演出：動き"], ["動き 拡大 ゆっくり", "拡大方向へゆっくりリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V343Effect : FxMove { public override string Label => "動き 拡大 ゆっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(S(t*.9f))*.12f*a,1f+(S(t*.9f))*.12f*a); }

[VideoEffect("動き 拡大 カクカク", ["演出：動き"], ["動き 拡大 カクカク", "拡大方向へカクカクリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V344Effect : FxMove { public override string Label => "動き 拡大 カクカク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Round(S(t*2.5f)*3f)/3f)*.12f*a,1f+(MathF.Round(S(t*2.5f)*3f)/3f)*.12f*a); }

[VideoEffect("動き 拡大 三角波", ["演出：動き"], ["動き 拡大 三角波", "拡大方向へ三角波リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V345Effect : FxMove { public override string Label => "動き 拡大 三角波"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Asin(S(t*3f))*.637f)*.12f*a,1f+(MathF.Asin(S(t*3f))*.637f)*.12f*a); }

[VideoEffect("動き 拡大 パルス", ["演出：動き"], ["動き 拡大 パルス", "拡大方向へパルスリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V346Effect : FxMove { public override string Label => "動き 拡大 パルス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.12f*a,1f+(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.12f*a); }

[VideoEffect("動き 拡大 弾む", ["演出：動き"], ["動き 拡大 弾む", "拡大方向へ弾むリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V347Effect : FxMove { public override string Label => "動き 拡大 弾む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Abs(S(t*3f)))*.12f*a,1f+(MathF.Abs(S(t*3f)))*.12f*a); }

[VideoEffect("動き 拡大 ふらつき", ["演出：動き"], ["動き 拡大 ふらつき", "拡大方向へふらつきリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V348Effect : FxMove { public override string Label => "動き 拡大 ふらつき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(N(t*1.6f,3))*.12f*a,1f+(N(t*1.6f,3))*.12f*a); }

[VideoEffect("動き 拡大 二段", ["演出：動き"], ["動き 拡大 二段", "拡大方向へ二段リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V349Effect : FxMove { public override string Label => "動き 拡大 二段"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+((S(t*2f)+S(t*5f)*.5f)*.67f)*.12f*a,1f+((S(t*2f)+S(t*5f)*.5f)*.67f)*.12f*a); }

[VideoEffect("動き 拡大 間欠", ["演出：動き"], ["動き 拡大 間欠", "拡大方向へ間欠リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V350Effect : FxMove { public override string Label => "動き 拡大 間欠"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.12f*a,1f+(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.12f*a); }

[VideoEffect("動き 横伸び なめらか", ["演出：動き"], ["動き 横伸び なめらか", "横伸び方向へなめらかリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V351Effect : FxMove { public override string Label => "動き 横伸び なめらか"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(S(t*2f))*.16f*a,1f-(S(t*2f))*.06f*a); }

[VideoEffect("動き 横伸び 速い", ["演出：動き"], ["動き 横伸び 速い", "横伸び方向へ速いリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V352Effect : FxMove { public override string Label => "動き 横伸び 速い"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(S(t*6.5f))*.16f*a,1f-(S(t*6.5f))*.06f*a); }

[VideoEffect("動き 横伸び ゆっくり", ["演出：動き"], ["動き 横伸び ゆっくり", "横伸び方向へゆっくりリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V353Effect : FxMove { public override string Label => "動き 横伸び ゆっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(S(t*.9f))*.16f*a,1f-(S(t*.9f))*.06f*a); }

[VideoEffect("動き 横伸び カクカク", ["演出：動き"], ["動き 横伸び カクカク", "横伸び方向へカクカクリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V354Effect : FxMove { public override string Label => "動き 横伸び カクカク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Round(S(t*2.5f)*3f)/3f)*.16f*a,1f-(MathF.Round(S(t*2.5f)*3f)/3f)*.06f*a); }

[VideoEffect("動き 横伸び 三角波", ["演出：動き"], ["動き 横伸び 三角波", "横伸び方向へ三角波リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V355Effect : FxMove { public override string Label => "動き 横伸び 三角波"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Asin(S(t*3f))*.637f)*.16f*a,1f-(MathF.Asin(S(t*3f))*.637f)*.06f*a); }

[VideoEffect("動き 横伸び パルス", ["演出：動き"], ["動き 横伸び パルス", "横伸び方向へパルスリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V356Effect : FxMove { public override string Label => "動き 横伸び パルス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.16f*a,1f-(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.06f*a); }

[VideoEffect("動き 横伸び 弾む", ["演出：動き"], ["動き 横伸び 弾む", "横伸び方向へ弾むリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V357Effect : FxMove { public override string Label => "動き 横伸び 弾む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(MathF.Abs(S(t*3f)))*.16f*a,1f-(MathF.Abs(S(t*3f)))*.06f*a); }

[VideoEffect("動き 横伸び ふらつき", ["演出：動き"], ["動き 横伸び ふらつき", "横伸び方向へふらつきリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V358Effect : FxMove { public override string Label => "動き 横伸び ふらつき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(N(t*1.6f,3))*.16f*a,1f-(N(t*1.6f,3))*.06f*a); }

[VideoEffect("動き 横伸び 二段", ["演出：動き"], ["動き 横伸び 二段", "横伸び方向へ二段リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V359Effect : FxMove { public override string Label => "動き 横伸び 二段"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+((S(t*2f)+S(t*5f)*.5f)*.67f)*.16f*a,1f-((S(t*2f)+S(t*5f)*.5f)*.67f)*.06f*a); }

[VideoEffect("動き 横伸び 間欠", ["演出：動き"], ["動き 横伸び 間欠", "横伸び方向へ間欠リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V360Effect : FxMove { public override string Label => "動き 横伸び 間欠"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sc(f,1f+(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.16f*a,1f-(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.06f*a); }

[VideoEffect("動き 縦伸び なめらか", ["演出：動き"], ["動き 縦伸び なめらか", "縦伸び方向へなめらかリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V361Effect : FxMove { public override string Label => "動き 縦伸び なめらか"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(S(t*2f))*.06f*a,1f+(S(t*2f))*.16f*a); }

[VideoEffect("動き 縦伸び 速い", ["演出：動き"], ["動き 縦伸び 速い", "縦伸び方向へ速いリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V362Effect : FxMove { public override string Label => "動き 縦伸び 速い"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(S(t*6.5f))*.06f*a,1f+(S(t*6.5f))*.16f*a); }

[VideoEffect("動き 縦伸び ゆっくり", ["演出：動き"], ["動き 縦伸び ゆっくり", "縦伸び方向へゆっくりリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V363Effect : FxMove { public override string Label => "動き 縦伸び ゆっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(S(t*.9f))*.06f*a,1f+(S(t*.9f))*.16f*a); }

[VideoEffect("動き 縦伸び カクカク", ["演出：動き"], ["動き 縦伸び カクカク", "縦伸び方向へカクカクリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V364Effect : FxMove { public override string Label => "動き 縦伸び カクカク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(MathF.Round(S(t*2.5f)*3f)/3f)*.06f*a,1f+(MathF.Round(S(t*2.5f)*3f)/3f)*.16f*a); }

[VideoEffect("動き 縦伸び 三角波", ["演出：動き"], ["動き 縦伸び 三角波", "縦伸び方向へ三角波リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V365Effect : FxMove { public override string Label => "動き 縦伸び 三角波"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(MathF.Asin(S(t*3f))*.637f)*.06f*a,1f+(MathF.Asin(S(t*3f))*.637f)*.16f*a); }

[VideoEffect("動き 縦伸び パルス", ["演出：動き"], ["動き 縦伸び パルス", "縦伸び方向へパルスリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V366Effect : FxMove { public override string Label => "動き 縦伸び パルス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.06f*a,1f+(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.16f*a); }

[VideoEffect("動き 縦伸び 弾む", ["演出：動き"], ["動き 縦伸び 弾む", "縦伸び方向へ弾むリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V367Effect : FxMove { public override string Label => "動き 縦伸び 弾む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(MathF.Abs(S(t*3f)))*.06f*a,1f+(MathF.Abs(S(t*3f)))*.16f*a); }

[VideoEffect("動き 縦伸び ふらつき", ["演出：動き"], ["動き 縦伸び ふらつき", "縦伸び方向へふらつきリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V368Effect : FxMove { public override string Label => "動き 縦伸び ふらつき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(N(t*1.6f,3))*.06f*a,1f+(N(t*1.6f,3))*.16f*a); }

[VideoEffect("動き 縦伸び 二段", ["演出：動き"], ["動き 縦伸び 二段", "縦伸び方向へ二段リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V369Effect : FxMove { public override string Label => "動き 縦伸び 二段"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-((S(t*2f)+S(t*5f)*.5f)*.67f)*.06f*a,1f+((S(t*2f)+S(t*5f)*.5f)*.67f)*.16f*a); }

[VideoEffect("動き 縦伸び 間欠", ["演出：動き"], ["動き 縦伸び 間欠", "縦伸び方向へ間欠リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V370Effect : FxMove { public override string Label => "動き 縦伸び 間欠"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Ft(f,1f-(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.06f*a,1f+(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.16f*a); }

[VideoEffect("動き なびき なめらか", ["演出：動き"], ["動き なびき なめらか", "なびき方向へなめらかリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V371Effect : FxMove { public override string Label => "動き なびき なめらか"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(S(t*2f))*.16f*a); }

[VideoEffect("動き なびき 速い", ["演出：動き"], ["動き なびき 速い", "なびき方向へ速いリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V372Effect : FxMove { public override string Label => "動き なびき 速い"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(S(t*6.5f))*.16f*a); }

[VideoEffect("動き なびき ゆっくり", ["演出：動き"], ["動き なびき ゆっくり", "なびき方向へゆっくりリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V373Effect : FxMove { public override string Label => "動き なびき ゆっくり"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(S(t*.9f))*.16f*a); }

[VideoEffect("動き なびき カクカク", ["演出：動き"], ["動き なびき カクカク", "なびき方向へカクカクリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V374Effect : FxMove { public override string Label => "動き なびき カクカク"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(MathF.Round(S(t*2.5f)*3f)/3f)*.16f*a); }

[VideoEffect("動き なびき 三角波", ["演出：動き"], ["動き なびき 三角波", "なびき方向へ三角波リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V375Effect : FxMove { public override string Label => "動き なびき 三角波"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(MathF.Asin(S(t*3f))*.637f)*.16f*a); }

[VideoEffect("動き なびき パルス", ["演出：動き"], ["動き なびき パルス", "なびき方向へパルスリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V376Effect : FxMove { public override string Label => "動き なびき パルス"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(MathF.Pow(MathF.Max(0f,S(t*3f)),6f))*.16f*a); }

[VideoEffect("動き なびき 弾む", ["演出：動き"], ["動き なびき 弾む", "なびき方向へ弾むリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V377Effect : FxMove { public override string Label => "動き なびき 弾む"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(MathF.Abs(S(t*3f)))*.16f*a); }

[VideoEffect("動き なびき ふらつき", ["演出：動き"], ["動き なびき ふらつき", "なびき方向へふらつきリズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V378Effect : FxMove { public override string Label => "動き なびき ふらつき"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(N(t*1.6f,3))*.16f*a); }

[VideoEffect("動き なびき 二段", ["演出：動き"], ["動き なびき 二段", "なびき方向へ二段リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V379Effect : FxMove { public override string Label => "動き なびき 二段"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,((S(t*2f)+S(t*5f)*.5f)*.67f)*.16f*a); }

[VideoEffect("動き なびき 間欠", ["演出：動き"], ["動き なびき 間欠", "なびき方向へ間欠リズムで動く", "動き", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V380Effect : FxMove { public override string Label => "動き なびき 間欠"; private protected override Matrix3x2 Mat(in FxFrame f, float t, float a) => Sk(f,(S(t*9f)*MathF.Pow(MathF.Max(0f,S(t*1.2f)),4f))*.16f*a); }

[VideoEffect("登場 スライド 左 近く", ["演出：出入り"], ["登場 スライド 左 近く", "左の近くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V381Effect : FxEnter { public override string Label => "登場 スライド 左 近く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.35f*d*dir,0f*d*dir); }

[VideoEffect("登場 スライド 左 中間", ["演出：出入り"], ["登場 スライド 左 中間", "左の中間から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V382Effect : FxEnter { public override string Label => "登場 スライド 左 中間"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.7f*d*dir,0f*d*dir); }

[VideoEffect("登場 スライド 左 遠く", ["演出：出入り"], ["登場 スライド 左 遠く", "左の遠くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V383Effect : FxEnter { public override string Label => "登場 スライド 左 遠く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-1.2f*d*dir,0f*d*dir); }

[VideoEffect("登場 スライド 右 近く", ["演出：出入り"], ["登場 スライド 右 近く", "右の近くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V384Effect : FxEnter { public override string Label => "登場 スライド 右 近く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.35f*d*dir,0f*d*dir); }

[VideoEffect("登場 スライド 右 中間", ["演出：出入り"], ["登場 スライド 右 中間", "右の中間から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V385Effect : FxEnter { public override string Label => "登場 スライド 右 中間"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.7f*d*dir,0f*d*dir); }

[VideoEffect("登場 スライド 右 遠く", ["演出：出入り"], ["登場 スライド 右 遠く", "右の遠くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V386Effect : FxEnter { public override string Label => "登場 スライド 右 遠く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,1.2f*d*dir,0f*d*dir); }

[VideoEffect("登場 スライド 上 近く", ["演出：出入り"], ["登場 スライド 上 近く", "上の近くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V387Effect : FxEnter { public override string Label => "登場 スライド 上 近く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0f*d*dir,-0.35f*d*dir); }

[VideoEffect("登場 スライド 上 中間", ["演出：出入り"], ["登場 スライド 上 中間", "上の中間から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V388Effect : FxEnter { public override string Label => "登場 スライド 上 中間"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0f*d*dir,-0.7f*d*dir); }

[VideoEffect("登場 スライド 上 遠く", ["演出：出入り"], ["登場 スライド 上 遠く", "上の遠くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V389Effect : FxEnter { public override string Label => "登場 スライド 上 遠く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0f*d*dir,-1.2f*d*dir); }

[VideoEffect("登場 スライド 下 近く", ["演出：出入り"], ["登場 スライド 下 近く", "下の近くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V390Effect : FxEnter { public override string Label => "登場 スライド 下 近く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0f*d*dir,0.35f*d*dir); }

[VideoEffect("登場 スライド 下 中間", ["演出：出入り"], ["登場 スライド 下 中間", "下の中間から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V391Effect : FxEnter { public override string Label => "登場 スライド 下 中間"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0f*d*dir,0.7f*d*dir); }

[VideoEffect("登場 スライド 下 遠く", ["演出：出入り"], ["登場 スライド 下 遠く", "下の遠くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V392Effect : FxEnter { public override string Label => "登場 スライド 下 遠く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0f*d*dir,1.2f*d*dir); }

[VideoEffect("登場 スライド 左上 近く", ["演出：出入り"], ["登場 スライド 左上 近く", "左上の近くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V393Effect : FxEnter { public override string Label => "登場 スライド 左上 近く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.35f*d*dir,-0.35f*d*dir); }

[VideoEffect("登場 スライド 左上 中間", ["演出：出入り"], ["登場 スライド 左上 中間", "左上の中間から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V394Effect : FxEnter { public override string Label => "登場 スライド 左上 中間"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.7f*d*dir,-0.7f*d*dir); }

[VideoEffect("登場 スライド 左上 遠く", ["演出：出入り"], ["登場 スライド 左上 遠く", "左上の遠くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V395Effect : FxEnter { public override string Label => "登場 スライド 左上 遠く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-1.2f*d*dir,-1.2f*d*dir); }

[VideoEffect("登場 スライド 右上 近く", ["演出：出入り"], ["登場 スライド 右上 近く", "右上の近くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V396Effect : FxEnter { public override string Label => "登場 スライド 右上 近く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.35f*d*dir,-0.35f*d*dir); }

[VideoEffect("登場 スライド 右上 中間", ["演出：出入り"], ["登場 スライド 右上 中間", "右上の中間から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V397Effect : FxEnter { public override string Label => "登場 スライド 右上 中間"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.7f*d*dir,-0.7f*d*dir); }

[VideoEffect("登場 スライド 右上 遠く", ["演出：出入り"], ["登場 スライド 右上 遠く", "右上の遠くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V398Effect : FxEnter { public override string Label => "登場 スライド 右上 遠く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,1.2f*d*dir,-1.2f*d*dir); }

[VideoEffect("登場 スライド 左下 近く", ["演出：出入り"], ["登場 スライド 左下 近く", "左下の近くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V399Effect : FxEnter { public override string Label => "登場 スライド 左下 近く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.35f*d*dir,0.35f*d*dir); }

[VideoEffect("登場 スライド 左下 中間", ["演出：出入り"], ["登場 スライド 左下 中間", "左下の中間から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V400Effect : FxEnter { public override string Label => "登場 スライド 左下 中間"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.7f*d*dir,0.7f*d*dir); }

[VideoEffect("登場 スライド 左下 遠く", ["演出：出入り"], ["登場 スライド 左下 遠く", "左下の遠くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V401Effect : FxEnter { public override string Label => "登場 スライド 左下 遠く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-1.2f*d*dir,1.2f*d*dir); }

[VideoEffect("登場 スライド 右下 近く", ["演出：出入り"], ["登場 スライド 右下 近く", "右下の近くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V402Effect : FxEnter { public override string Label => "登場 スライド 右下 近く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.35f*d*dir,0.35f*d*dir); }

[VideoEffect("登場 スライド 右下 中間", ["演出：出入り"], ["登場 スライド 右下 中間", "右下の中間から滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V403Effect : FxEnter { public override string Label => "登場 スライド 右下 中間"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.7f*d*dir,0.7f*d*dir); }

[VideoEffect("登場 スライド 右下 遠く", ["演出：出入り"], ["登場 スライド 右下 遠く", "右下の遠くから滑り込む", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V404Effect : FxEnter { public override string Label => "登場 スライド 右下 遠く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,1.2f*d*dir,1.2f*d*dir); }

[VideoEffect("登場 右回り 45度", ["演出：出入り"], ["登場 右回り 45度", "右回りに45度回りながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V405Effect : FxEnter { public override string Label => "登場 右回り 45度"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,0.7854f*d*dir); }

[VideoEffect("登場 右回り 90度", ["演出：出入り"], ["登場 右回り 90度", "右回りに90度回りながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V406Effect : FxEnter { public override string Label => "登場 右回り 90度"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,1.5708f*d*dir); }

[VideoEffect("登場 右回り 180度", ["演出：出入り"], ["登場 右回り 180度", "右回りに180度回りながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V407Effect : FxEnter { public override string Label => "登場 右回り 180度"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,3.1416f*d*dir); }

[VideoEffect("登場 右回り 360度", ["演出：出入り"], ["登場 右回り 360度", "右回りに360度回りながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V408Effect : FxEnter { public override string Label => "登場 右回り 360度"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,6.2832f*d*dir); }

[VideoEffect("登場 左回り 45度", ["演出：出入り"], ["登場 左回り 45度", "左回りに45度回りながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V409Effect : FxEnter { public override string Label => "登場 左回り 45度"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-0.7854f*d*dir); }

[VideoEffect("登場 左回り 90度", ["演出：出入り"], ["登場 左回り 90度", "左回りに90度回りながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V410Effect : FxEnter { public override string Label => "登場 左回り 90度"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-1.5708f*d*dir); }

[VideoEffect("登場 左回り 180度", ["演出：出入り"], ["登場 左回り 180度", "左回りに180度回りながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V411Effect : FxEnter { public override string Label => "登場 左回り 180度"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-3.1416f*d*dir); }

[VideoEffect("登場 左回り 360度", ["演出：出入り"], ["登場 左回り 360度", "左回りに360度回りながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V412Effect : FxEnter { public override string Label => "登場 左回り 360度"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-6.2832f*d*dir); }

[VideoEffect("登場 大きい所から 0.5倍", ["演出：出入り"], ["登場 大きい所から 0.5倍", "1.5倍の大きさから戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V413Effect : FxEnter { public override string Label => "登場 大きい所から 0.5倍"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f+0.5f*d,1f+0.5f*d); }

[VideoEffect("登場 大きい所から 1倍", ["演出：出入り"], ["登場 大きい所から 1倍", "2倍の大きさから戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V414Effect : FxEnter { public override string Label => "登場 大きい所から 1倍"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f+1f*d,1f+1f*d); }

[VideoEffect("登場 大きい所から 2倍", ["演出：出入り"], ["登場 大きい所から 2倍", "3倍の大きさから戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V415Effect : FxEnter { public override string Label => "登場 大きい所から 2倍"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f+2f*d,1f+2f*d); }

[VideoEffect("登場 大きい所から 3倍", ["演出：出入り"], ["登場 大きい所から 3倍", "4倍の大きさから戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V416Effect : FxEnter { public override string Label => "登場 大きい所から 3倍"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f+3f*d,1f+3f*d); }

[VideoEffect("登場 小さい所から 30%", ["演出：出入り"], ["登場 小さい所から 30%", "30%縮んだ所から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V417Effect : FxEnter { public override string Label => "登場 小さい所から 30%"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f-0.3f*d,1f-0.3f*d); }

[VideoEffect("登場 小さい所から 60%", ["演出：出入り"], ["登場 小さい所から 60%", "60%縮んだ所から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V418Effect : FxEnter { public override string Label => "登場 小さい所から 60%"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f-0.6f*d,1f-0.6f*d); }

[VideoEffect("登場 小さい所から 85%", ["演出：出入り"], ["登場 小さい所から 85%", "85%縮んだ所から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V419Effect : FxEnter { public override string Label => "登場 小さい所から 85%"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f-0.85f*d,1f-0.85f*d); }

[VideoEffect("登場 小さい所から 98%", ["演出：出入り"], ["登場 小さい所から 98%", "98%縮んだ所から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V420Effect : FxEnter { public override string Label => "登場 小さい所から 98%"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f-0.98f*d,1f-0.98f*d); }

[VideoEffect("登場 縦に開く", ["演出：出入り"], ["登場 縦に開く", "上下に開いて出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V421Effect : FxEnter { public override string Label => "登場 縦に開く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f,1f-d); }

[VideoEffect("登場 横に開く", ["演出：出入り"], ["登場 横に開く", "左右に開いて出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V422Effect : FxEnter { public override string Label => "登場 横に開く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f-d,1f); }

[VideoEffect("登場 足元から伸びる", ["演出：出入り"], ["登場 足元から伸びる", "足元から上へ伸びる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V423Effect : FxEnter { public override string Label => "登場 足元から伸びる"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ft(f,1f,1f-d); }

[VideoEffect("登場 つぶれて戻る", ["演出：出入り"], ["登場 つぶれて戻る", "横につぶれた形から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V424Effect : FxEnter { public override string Label => "登場 つぶれて戻る"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sc(f,1f+d,1f-d*.95f); }

[VideoEffect("登場 回りながら 左", ["演出：出入り"], ["登場 回りながら 左", "左から回転しながら入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V425Effect : FxEnter { public override string Label => "登場 回りながら 左"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-0.9f*d*dir)*Mo(f,-0.8f*d*dir,0f*d*dir); }

[VideoEffect("登場 回りながら 右", ["演出：出入り"], ["登場 回りながら 右", "右から回転しながら入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V426Effect : FxEnter { public override string Label => "登場 回りながら 右"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,0.9f*d*dir)*Mo(f,0.8f*d*dir,0f*d*dir); }

[VideoEffect("登場 回りながら 上", ["演出：出入り"], ["登場 回りながら 上", "上から回転しながら入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V427Effect : FxEnter { public override string Label => "登場 回りながら 上"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-0.9f*d*dir)*Mo(f,0f*d*dir,-0.8f*d*dir); }

[VideoEffect("登場 回りながら 下", ["演出：出入り"], ["登場 回りながら 下", "下から回転しながら入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V428Effect : FxEnter { public override string Label => "登場 回りながら 下"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,0.9f*d*dir)*Mo(f,0f*d*dir,0.8f*d*dir); }

[VideoEffect("登場 回りながら 左上", ["演出：出入り"], ["登場 回りながら 左上", "左上から回転しながら入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V429Effect : FxEnter { public override string Label => "登場 回りながら 左上"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-0.9f*d*dir)*Mo(f,-0.8f*d*dir,-0.8f*d*dir); }

[VideoEffect("登場 回りながら 右上", ["演出：出入り"], ["登場 回りながら 右上", "右上から回転しながら入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V430Effect : FxEnter { public override string Label => "登場 回りながら 右上"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,0.9f*d*dir)*Mo(f,0.8f*d*dir,-0.8f*d*dir); }

[VideoEffect("登場 回りながら 左下", ["演出：出入り"], ["登場 回りながら 左下", "左下から回転しながら入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V431Effect : FxEnter { public override string Label => "登場 回りながら 左下"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-0.9f*d*dir)*Mo(f,-0.8f*d*dir,0.8f*d*dir); }

[VideoEffect("登場 回りながら 右下", ["演出：出入り"], ["登場 回りながら 右下", "右下から回転しながら入る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V432Effect : FxEnter { public override string Label => "登場 回りながら 右下"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,0.9f*d*dir)*Mo(f,0.8f*d*dir,0.8f*d*dir); }

[VideoEffect("登場 ふわっと 左", ["演出：出入り"], ["登場 ふわっと 左", "左から少しだけ動いて現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V433Effect : FxEnter { public override string Label => "登場 ふわっと 左"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.1f*d*dir,0f*d*dir); }

[VideoEffect("登場 ふわっと 右", ["演出：出入り"], ["登場 ふわっと 右", "右から少しだけ動いて現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V434Effect : FxEnter { public override string Label => "登場 ふわっと 右"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.1f*d*dir,0f*d*dir); }

[VideoEffect("登場 ふわっと 上", ["演出：出入り"], ["登場 ふわっと 上", "上から少しだけ動いて現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V435Effect : FxEnter { public override string Label => "登場 ふわっと 上"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0f*d*dir,-0.1f*d*dir); }

[VideoEffect("登場 ふわっと 下", ["演出：出入り"], ["登場 ふわっと 下", "下から少しだけ動いて現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V436Effect : FxEnter { public override string Label => "登場 ふわっと 下"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0f*d*dir,0.1f*d*dir); }

[VideoEffect("登場 ふわっと 左上", ["演出：出入り"], ["登場 ふわっと 左上", "左上から少しだけ動いて現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V437Effect : FxEnter { public override string Label => "登場 ふわっと 左上"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.1f*d*dir,-0.1f*d*dir); }

[VideoEffect("登場 ふわっと 右上", ["演出：出入り"], ["登場 ふわっと 右上", "右上から少しだけ動いて現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V438Effect : FxEnter { public override string Label => "登場 ふわっと 右上"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.1f*d*dir,-0.1f*d*dir); }

[VideoEffect("登場 ふわっと 左下", ["演出：出入り"], ["登場 ふわっと 左下", "左下から少しだけ動いて現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V439Effect : FxEnter { public override string Label => "登場 ふわっと 左下"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,-0.1f*d*dir,0.1f*d*dir); }

[VideoEffect("登場 ふわっと 右下", ["演出：出入り"], ["登場 ふわっと 右下", "右下から少しだけ動いて現れる", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V440Effect : FxEnter { public override string Label => "登場 ふわっと 右下"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Mo(f,0.1f*d*dir,0.1f*d*dir); }

[VideoEffect("登場 斜め崩し 右へ弱く", ["演出：出入り"], ["登場 斜め崩し 右へ弱く", "右へ弱く傾いた形から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V441Effect : FxEnter { public override string Label => "登場 斜め崩し 右へ弱く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f,0.4f*d*dir); }

[VideoEffect("登場 斜め崩し 右へ中", ["演出：出入り"], ["登場 斜め崩し 右へ中", "右へ中傾いた形から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V442Effect : FxEnter { public override string Label => "登場 斜め崩し 右へ中"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f,0.8f*d*dir); }

[VideoEffect("登場 斜め崩し 右へ強く", ["演出：出入り"], ["登場 斜め崩し 右へ強く", "右へ強く傾いた形から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V443Effect : FxEnter { public override string Label => "登場 斜め崩し 右へ強く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f,1.4f*d*dir); }

[VideoEffect("登場 斜め崩し 左へ弱く", ["演出：出入り"], ["登場 斜め崩し 左へ弱く", "左へ弱く傾いた形から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V444Effect : FxEnter { public override string Label => "登場 斜め崩し 左へ弱く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f,-0.4f*d*dir); }

[VideoEffect("登場 斜め崩し 左へ中", ["演出：出入り"], ["登場 斜め崩し 左へ中", "左へ中傾いた形から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V445Effect : FxEnter { public override string Label => "登場 斜め崩し 左へ中"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f,-0.8f*d*dir); }

[VideoEffect("登場 斜め崩し 左へ強く", ["演出：出入り"], ["登場 斜め崩し 左へ強く", "左へ強く傾いた形から戻る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V446Effect : FxEnter { public override string Label => "登場 斜め崩し 左へ強く"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Sk(f,-1.4f*d*dir); }

[VideoEffect("登場 右回りで拡大", ["演出：出入り"], ["登場 右回りで拡大", "右回りで拡大しながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V447Effect : FxEnter { public override string Label => "登場 右回りで拡大"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,3.1416f*d*dir)*Sc(f,1f+1.6f*d,1f+1.6f*d); }

[VideoEffect("登場 左回りで拡大", ["演出：出入り"], ["登場 左回りで拡大", "左回りで拡大しながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V448Effect : FxEnter { public override string Label => "登場 左回りで拡大"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-3.1416f*d*dir)*Sc(f,1f+1.6f*d,1f+1.6f*d); }

[VideoEffect("登場 右回りで縮小", ["演出：出入り"], ["登場 右回りで縮小", "右回りで縮小しながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V449Effect : FxEnter { public override string Label => "登場 右回りで縮小"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,3.1416f*d*dir)*Sc(f,1f-.92f*d,1f-.92f*d); }

[VideoEffect("登場 左回りで縮小", ["演出：出入り"], ["登場 左回りで縮小", "左回りで縮小しながら出る", "出入り", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V450Effect : FxEnter { public override string Label => "登場 左回りで縮小"; private protected override Matrix3x2 Mat(in FxFrame f, float d, float dir) => Ro(f,-3.1416f*d*dir)*Sc(f,1f-.92f*d,1f-.92f*d); }

[VideoEffect("重ね模様 水玉 極小", ["演出：質感・画風"], ["重ね模様 水玉 極小", "極小さの水玉を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V451Effect : FxPat { public override string Label => "重ね模様 水玉 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);Each(l,t,r,bo,u,(x,y,i)=>Dot(dc,b,x+o%u,y,u*.22f)); } }

[VideoEffect("重ね模様 水玉 小", ["演出：質感・画風"], ["重ね模様 水玉 小", "小さの水玉を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V452Effect : FxPat { public override string Label => "重ね模様 水玉 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);Each(l,t,r,bo,u,(x,y,i)=>Dot(dc,b,x+o%u,y,u*.22f)); } }

[VideoEffect("重ね模様 水玉 中", ["演出：質感・画風"], ["重ね模様 水玉 中", "中さの水玉を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V453Effect : FxPat { public override string Label => "重ね模様 水玉 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);Each(l,t,r,bo,u,(x,y,i)=>Dot(dc,b,x+o%u,y,u*.22f)); } }

[VideoEffect("重ね模様 水玉 大", ["演出：質感・画風"], ["重ね模様 水玉 大", "大さの水玉を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V454Effect : FxPat { public override string Label => "重ね模様 水玉 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);Each(l,t,r,bo,u,(x,y,i)=>Dot(dc,b,x+o%u,y,u*.22f)); } }

[VideoEffect("重ね模様 水玉 特大", ["演出：質感・画風"], ["重ね模様 水玉 特大", "特大さの水玉を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V455Effect : FxPat { public override string Label => "重ね模様 水玉 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);Each(l,t,r,bo,u,(x,y,i)=>Dot(dc,b,x+o%u,y,u*.22f)); } }

[VideoEffect("重ね模様 輪 極小", ["演出：質感・画風"], ["重ね模様 輪 極小", "極小さの輪を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V456Effect : FxPat { public override string Label => "重ね模様 輪 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);Each(l,t,r,bo,u,(x,y,i)=>Ring(dc,b,x+o%u,y,u*.28f,u*.07f)); } }

[VideoEffect("重ね模様 輪 小", ["演出：質感・画風"], ["重ね模様 輪 小", "小さの輪を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V457Effect : FxPat { public override string Label => "重ね模様 輪 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);Each(l,t,r,bo,u,(x,y,i)=>Ring(dc,b,x+o%u,y,u*.28f,u*.07f)); } }

[VideoEffect("重ね模様 輪 中", ["演出：質感・画風"], ["重ね模様 輪 中", "中さの輪を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V458Effect : FxPat { public override string Label => "重ね模様 輪 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);Each(l,t,r,bo,u,(x,y,i)=>Ring(dc,b,x+o%u,y,u*.28f,u*.07f)); } }

[VideoEffect("重ね模様 輪 大", ["演出：質感・画風"], ["重ね模様 輪 大", "大さの輪を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V459Effect : FxPat { public override string Label => "重ね模様 輪 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);Each(l,t,r,bo,u,(x,y,i)=>Ring(dc,b,x+o%u,y,u*.28f,u*.07f)); } }

[VideoEffect("重ね模様 輪 特大", ["演出：質感・画風"], ["重ね模様 輪 特大", "特大さの輪を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V460Effect : FxPat { public override string Label => "重ね模様 輪 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);Each(l,t,r,bo,u,(x,y,i)=>Ring(dc,b,x+o%u,y,u*.28f,u*.07f)); } }

[VideoEffect("重ね模様 十字 極小", ["演出：質感・画風"], ["重ね模様 十字 極小", "極小さの十字を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V461Effect : FxPat { public override string Label => "重ね模様 十字 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);Each(l,t,r,bo,u,(x,y,i)=>{Line(dc,b,x-u*.24f,y,x+u*.24f,y,u*.08f);Line(dc,b,x,y-u*.24f,x,y+u*.24f,u*.08f);}); } }

[VideoEffect("重ね模様 十字 小", ["演出：質感・画風"], ["重ね模様 十字 小", "小さの十字を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V462Effect : FxPat { public override string Label => "重ね模様 十字 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);Each(l,t,r,bo,u,(x,y,i)=>{Line(dc,b,x-u*.24f,y,x+u*.24f,y,u*.08f);Line(dc,b,x,y-u*.24f,x,y+u*.24f,u*.08f);}); } }

[VideoEffect("重ね模様 十字 中", ["演出：質感・画風"], ["重ね模様 十字 中", "中さの十字を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V463Effect : FxPat { public override string Label => "重ね模様 十字 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);Each(l,t,r,bo,u,(x,y,i)=>{Line(dc,b,x-u*.24f,y,x+u*.24f,y,u*.08f);Line(dc,b,x,y-u*.24f,x,y+u*.24f,u*.08f);}); } }

[VideoEffect("重ね模様 十字 大", ["演出：質感・画風"], ["重ね模様 十字 大", "大さの十字を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V464Effect : FxPat { public override string Label => "重ね模様 十字 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);Each(l,t,r,bo,u,(x,y,i)=>{Line(dc,b,x-u*.24f,y,x+u*.24f,y,u*.08f);Line(dc,b,x,y-u*.24f,x,y+u*.24f,u*.08f);}); } }

[VideoEffect("重ね模様 十字 特大", ["演出：質感・画風"], ["重ね模様 十字 特大", "特大さの十字を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V465Effect : FxPat { public override string Label => "重ね模様 十字 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);Each(l,t,r,bo,u,(x,y,i)=>{Line(dc,b,x-u*.24f,y,x+u*.24f,y,u*.08f);Line(dc,b,x,y-u*.24f,x,y+u*.24f,u*.08f);}); } }

[VideoEffect("重ね模様 斜線 極小", ["演出：質感・画風"], ["重ね模様 斜線 極小", "極小さの斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V466Effect : FxPat { public override string Label => "重ね模様 斜線 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);for(float x=l-(bo-t)-u;x<r+u;x+=u)Line(dc,b,x+o%u,t,x+o%u+(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 斜線 小", ["演出：質感・画風"], ["重ね模様 斜線 小", "小さの斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V467Effect : FxPat { public override string Label => "重ね模様 斜線 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);for(float x=l-(bo-t)-u;x<r+u;x+=u)Line(dc,b,x+o%u,t,x+o%u+(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 斜線 中", ["演出：質感・画風"], ["重ね模様 斜線 中", "中さの斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V468Effect : FxPat { public override string Label => "重ね模様 斜線 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);for(float x=l-(bo-t)-u;x<r+u;x+=u)Line(dc,b,x+o%u,t,x+o%u+(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 斜線 大", ["演出：質感・画風"], ["重ね模様 斜線 大", "大さの斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V469Effect : FxPat { public override string Label => "重ね模様 斜線 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);for(float x=l-(bo-t)-u;x<r+u;x+=u)Line(dc,b,x+o%u,t,x+o%u+(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 斜線 特大", ["演出：質感・画風"], ["重ね模様 斜線 特大", "特大さの斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V470Effect : FxPat { public override string Label => "重ね模様 斜線 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);for(float x=l-(bo-t)-u;x<r+u;x+=u)Line(dc,b,x+o%u,t,x+o%u+(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 逆斜線 極小", ["演出：質感・画風"], ["重ね模様 逆斜線 極小", "極小さの逆斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V471Effect : FxPat { public override string Label => "重ね模様 逆斜線 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);for(float x=l-u;x<r+(bo-t)+u;x+=u)Line(dc,b,x-o%u,t,x-o%u-(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 逆斜線 小", ["演出：質感・画風"], ["重ね模様 逆斜線 小", "小さの逆斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V472Effect : FxPat { public override string Label => "重ね模様 逆斜線 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);for(float x=l-u;x<r+(bo-t)+u;x+=u)Line(dc,b,x-o%u,t,x-o%u-(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 逆斜線 中", ["演出：質感・画風"], ["重ね模様 逆斜線 中", "中さの逆斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V473Effect : FxPat { public override string Label => "重ね模様 逆斜線 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);for(float x=l-u;x<r+(bo-t)+u;x+=u)Line(dc,b,x-o%u,t,x-o%u-(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 逆斜線 大", ["演出：質感・画風"], ["重ね模様 逆斜線 大", "大さの逆斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V474Effect : FxPat { public override string Label => "重ね模様 逆斜線 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);for(float x=l-u;x<r+(bo-t)+u;x+=u)Line(dc,b,x-o%u,t,x-o%u-(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 逆斜線 特大", ["演出：質感・画風"], ["重ね模様 逆斜線 特大", "特大さの逆斜線を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V475Effect : FxPat { public override string Label => "重ね模様 逆斜線 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);for(float x=l-u;x<r+(bo-t)+u;x+=u)Line(dc,b,x-o%u,t,x-o%u-(bo-t),bo,u*.16f); } }

[VideoEffect("重ね模様 ひし形 極小", ["演出：質感・画風"], ["重ね模様 ひし形 極小", "極小さのひし形を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V476Effect : FxPat { public override string Label => "重ね模様 ひし形 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.3f,x+u*.3f,y,x,y+u*.3f,x-u*.3f,y)); } }

[VideoEffect("重ね模様 ひし形 小", ["演出：質感・画風"], ["重ね模様 ひし形 小", "小さのひし形を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V477Effect : FxPat { public override string Label => "重ね模様 ひし形 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.3f,x+u*.3f,y,x,y+u*.3f,x-u*.3f,y)); } }

[VideoEffect("重ね模様 ひし形 中", ["演出：質感・画風"], ["重ね模様 ひし形 中", "中さのひし形を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V478Effect : FxPat { public override string Label => "重ね模様 ひし形 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.3f,x+u*.3f,y,x,y+u*.3f,x-u*.3f,y)); } }

[VideoEffect("重ね模様 ひし形 大", ["演出：質感・画風"], ["重ね模様 ひし形 大", "大さのひし形を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V479Effect : FxPat { public override string Label => "重ね模様 ひし形 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.3f,x+u*.3f,y,x,y+u*.3f,x-u*.3f,y)); } }

[VideoEffect("重ね模様 ひし形 特大", ["演出：質感・画風"], ["重ね模様 ひし形 特大", "特大さのひし形を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V480Effect : FxPat { public override string Label => "重ね模様 ひし形 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.3f,x+u*.3f,y,x,y+u*.3f,x-u*.3f,y)); } }

[VideoEffect("重ね模様 三角 極小", ["演出：質感・画風"], ["重ね模様 三角 極小", "極小さの三角を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V481Effect : FxPat { public override string Label => "重ね模様 三角 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.28f,x+u*.28f,y+u*.24f,x-u*.28f,y+u*.24f)); } }

[VideoEffect("重ね模様 三角 小", ["演出：質感・画風"], ["重ね模様 三角 小", "小さの三角を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V482Effect : FxPat { public override string Label => "重ね模様 三角 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.28f,x+u*.28f,y+u*.24f,x-u*.28f,y+u*.24f)); } }

[VideoEffect("重ね模様 三角 中", ["演出：質感・画風"], ["重ね模様 三角 中", "中さの三角を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V483Effect : FxPat { public override string Label => "重ね模様 三角 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.28f,x+u*.28f,y+u*.24f,x-u*.28f,y+u*.24f)); } }

[VideoEffect("重ね模様 三角 大", ["演出：質感・画風"], ["重ね模様 三角 大", "大さの三角を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V484Effect : FxPat { public override string Label => "重ね模様 三角 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.28f,x+u*.28f,y+u*.24f,x-u*.28f,y+u*.24f)); } }

[VideoEffect("重ね模様 三角 特大", ["演出：質感・画風"], ["重ね模様 三角 特大", "特大さの三角を形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V485Effect : FxPat { public override string Label => "重ね模様 三角 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.28f,x+u*.28f,y+u*.24f,x-u*.28f,y+u*.24f)); } }

[VideoEffect("重ね模様 ジグザグ 極小", ["演出：質感・画風"], ["重ね模様 ジグザグ 極小", "極小さのジグザグを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V486Effect : FxPat { public override string Label => "重ね模様 ジグザグ 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);for(float y=t;y<bo+u;y+=u){int z=0;for(float x=l-u;x<r+u;x+=u*.5f,z++)Line(dc,b,x+o%u,y+(z%2)*u*.3f,x+o%u+u*.5f,y+((z+1)%2)*u*.3f,u*.09f);} } }

[VideoEffect("重ね模様 ジグザグ 小", ["演出：質感・画風"], ["重ね模様 ジグザグ 小", "小さのジグザグを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V487Effect : FxPat { public override string Label => "重ね模様 ジグザグ 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);for(float y=t;y<bo+u;y+=u){int z=0;for(float x=l-u;x<r+u;x+=u*.5f,z++)Line(dc,b,x+o%u,y+(z%2)*u*.3f,x+o%u+u*.5f,y+((z+1)%2)*u*.3f,u*.09f);} } }

[VideoEffect("重ね模様 ジグザグ 中", ["演出：質感・画風"], ["重ね模様 ジグザグ 中", "中さのジグザグを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V488Effect : FxPat { public override string Label => "重ね模様 ジグザグ 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);for(float y=t;y<bo+u;y+=u){int z=0;for(float x=l-u;x<r+u;x+=u*.5f,z++)Line(dc,b,x+o%u,y+(z%2)*u*.3f,x+o%u+u*.5f,y+((z+1)%2)*u*.3f,u*.09f);} } }

[VideoEffect("重ね模様 ジグザグ 大", ["演出：質感・画風"], ["重ね模様 ジグザグ 大", "大さのジグザグを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V489Effect : FxPat { public override string Label => "重ね模様 ジグザグ 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);for(float y=t;y<bo+u;y+=u){int z=0;for(float x=l-u;x<r+u;x+=u*.5f,z++)Line(dc,b,x+o%u,y+(z%2)*u*.3f,x+o%u+u*.5f,y+((z+1)%2)*u*.3f,u*.09f);} } }

[VideoEffect("重ね模様 ジグザグ 特大", ["演出：質感・画風"], ["重ね模様 ジグザグ 特大", "特大さのジグザグを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V490Effect : FxPat { public override string Label => "重ね模様 ジグザグ 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);for(float y=t;y<bo+u;y+=u){int z=0;for(float x=l-u;x<r+u;x+=u*.5f,z++)Line(dc,b,x+o%u,y+(z%2)*u*.3f,x+o%u+u*.5f,y+((z+1)%2)*u*.3f,u*.09f);} } }

[VideoEffect("重ね模様 きらめき 極小", ["演出：質感・画風"], ["重ね模様 きらめき 極小", "極小さのきらめきを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V491Effect : FxPat { public override string Label => "重ね模様 きらめき 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.36f,x+u*.08f,y-u*.08f,x+u*.36f,y,x+u*.08f,y+u*.08f,x,y+u*.36f,x-u*.08f,y+u*.08f,x-u*.36f,y,x-u*.08f,y-u*.08f)); } }

[VideoEffect("重ね模様 きらめき 小", ["演出：質感・画風"], ["重ね模様 きらめき 小", "小さのきらめきを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V492Effect : FxPat { public override string Label => "重ね模様 きらめき 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.36f,x+u*.08f,y-u*.08f,x+u*.36f,y,x+u*.08f,y+u*.08f,x,y+u*.36f,x-u*.08f,y+u*.08f,x-u*.36f,y,x-u*.08f,y-u*.08f)); } }

[VideoEffect("重ね模様 きらめき 中", ["演出：質感・画風"], ["重ね模様 きらめき 中", "中さのきらめきを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V493Effect : FxPat { public override string Label => "重ね模様 きらめき 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.36f,x+u*.08f,y-u*.08f,x+u*.36f,y,x+u*.08f,y+u*.08f,x,y+u*.36f,x-u*.08f,y+u*.08f,x-u*.36f,y,x-u*.08f,y-u*.08f)); } }

[VideoEffect("重ね模様 きらめき 大", ["演出：質感・画風"], ["重ね模様 きらめき 大", "大さのきらめきを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V494Effect : FxPat { public override string Label => "重ね模様 きらめき 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.36f,x+u*.08f,y-u*.08f,x+u*.36f,y,x+u*.08f,y+u*.08f,x,y+u*.36f,x-u*.08f,y+u*.08f,x-u*.36f,y,x-u*.08f,y-u*.08f)); } }

[VideoEffect("重ね模様 きらめき 特大", ["演出：質感・画風"], ["重ね模様 きらめき 特大", "特大さのきらめきを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V495Effect : FxPat { public override string Label => "重ね模様 きらめき 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);Each(l,t,r,bo,u,(x,y,i)=>Poly(dc,b,x,y-u*.36f,x+u*.08f,y-u*.08f,x+u*.36f,y,x+u*.08f,y+u*.08f,x,y+u*.36f,x-u*.08f,y+u*.08f,x-u*.36f,y,x-u*.08f,y-u*.08f)); } }

[VideoEffect("重ね模様 ブロック 極小", ["演出：質感・画風"], ["重ね模様 ブロック 極小", "極小さのブロックを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V496Effect : FxPat { public override string Label => "重ね模様 ブロック 極小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.45f);Each(l,t,r,bo,u,(x,y,i)=>{if(i%3==0)dc.FillRectangle(new Vortice.RawRectF(x,y,x+u*.62f,y+u*.62f),b);}); } }

[VideoEffect("重ね模様 ブロック 小", ["演出：質感・画風"], ["重ね模様 ブロック 小", "小さのブロックを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V497Effect : FxPat { public override string Label => "重ね模様 ブロック 小"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*0.7f);Each(l,t,r,bo,u,(x,y,i)=>{if(i%3==0)dc.FillRectangle(new Vortice.RawRectF(x,y,x+u*.62f,y+u*.62f),b);}); } }

[VideoEffect("重ね模様 ブロック 中", ["演出：質感・画風"], ["重ね模様 ブロック 中", "中さのブロックを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V498Effect : FxPat { public override string Label => "重ね模様 ブロック 中"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1f);Each(l,t,r,bo,u,(x,y,i)=>{if(i%3==0)dc.FillRectangle(new Vortice.RawRectF(x,y,x+u*.62f,y+u*.62f),b);}); } }

[VideoEffect("重ね模様 ブロック 大", ["演出：質感・画風"], ["重ね模様 ブロック 大", "大さのブロックを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V499Effect : FxPat { public override string Label => "重ね模様 ブロック 大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*1.5f);Each(l,t,r,bo,u,(x,y,i)=>{if(i%3==0)dc.FillRectangle(new Vortice.RawRectF(x,y,x+u*.62f,y+u*.62f),b);}); } }

[VideoEffect("重ね模様 ブロック 特大", ["演出：質感・画風"], ["重ね模様 ブロック 特大", "特大さのブロックを形の上に重ねる", "質感・画風", "演出"], IsAviUtlSupported = false)]
public sealed class Fx4V500Effect : FxPat { public override string Label => "重ね模様 ブロック 特大"; private protected override void Pat(ID2D1DeviceContext dc, ID2D1SolidColorBrush b, float l, float t, float r, float bo, float st, float o) { float u=MathF.Max(5f,st*2.2f);Each(l,t,r,bo,u,(x,y,i)=>{if(i%3==0)dc.FillRectangle(new Vortice.RawRectF(x,y,x+u*.62f,y+u*.62f),b);}); } }
