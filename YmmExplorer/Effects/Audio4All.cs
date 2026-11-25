using YukkuriMovieMaker.Plugin.Effects;

namespace YmmExplorer.Fx;

// 音声 201〜500(tools で生成)

[AudioEffect("帯域 60Hz〜3kHz", ["演出：音質"], ["帯域 60Hz〜3kHz", "60Hzから3kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A201Effect : FxA2 { public override string Label => "帯域 60Hz〜3kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 60, 3000); }

[AudioEffect("帯域 60Hz〜5kHz", ["演出：音質"], ["帯域 60Hz〜5kHz", "60Hzから5kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A202Effect : FxA2 { public override string Label => "帯域 60Hz〜5kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 60, 5000); }

[AudioEffect("帯域 60Hz〜8kHz", ["演出：音質"], ["帯域 60Hz〜8kHz", "60Hzから8kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A203Effect : FxA2 { public override string Label => "帯域 60Hz〜8kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 60, 8000); }

[AudioEffect("帯域 60Hz〜12kHz", ["演出：音質"], ["帯域 60Hz〜12kHz", "60Hzから12kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A204Effect : FxA2 { public override string Label => "帯域 60Hz〜12kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 60, 12000); }

[AudioEffect("帯域 60Hz〜16kHz", ["演出：音質"], ["帯域 60Hz〜16kHz", "60Hzから16kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A205Effect : FxA2 { public override string Label => "帯域 60Hz〜16kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 60, 16000); }

[AudioEffect("帯域 120Hz〜3kHz", ["演出：音質"], ["帯域 120Hz〜3kHz", "120Hzから3kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A206Effect : FxA2 { public override string Label => "帯域 120Hz〜3kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 120, 3000); }

[AudioEffect("帯域 120Hz〜5kHz", ["演出：音質"], ["帯域 120Hz〜5kHz", "120Hzから5kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A207Effect : FxA2 { public override string Label => "帯域 120Hz〜5kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 120, 5000); }

[AudioEffect("帯域 120Hz〜8kHz", ["演出：音質"], ["帯域 120Hz〜8kHz", "120Hzから8kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A208Effect : FxA2 { public override string Label => "帯域 120Hz〜8kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 120, 8000); }

[AudioEffect("帯域 120Hz〜12kHz", ["演出：音質"], ["帯域 120Hz〜12kHz", "120Hzから12kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A209Effect : FxA2 { public override string Label => "帯域 120Hz〜12kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 120, 12000); }

[AudioEffect("帯域 120Hz〜16kHz", ["演出：音質"], ["帯域 120Hz〜16kHz", "120Hzから16kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A210Effect : FxA2 { public override string Label => "帯域 120Hz〜16kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 120, 16000); }

[AudioEffect("帯域 250Hz〜3kHz", ["演出：音質"], ["帯域 250Hz〜3kHz", "250Hzから3kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A211Effect : FxA2 { public override string Label => "帯域 250Hz〜3kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 250, 3000); }

[AudioEffect("帯域 250Hz〜5kHz", ["演出：音質"], ["帯域 250Hz〜5kHz", "250Hzから5kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A212Effect : FxA2 { public override string Label => "帯域 250Hz〜5kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 250, 5000); }

[AudioEffect("帯域 250Hz〜8kHz", ["演出：音質"], ["帯域 250Hz〜8kHz", "250Hzから8kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A213Effect : FxA2 { public override string Label => "帯域 250Hz〜8kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 250, 8000); }

[AudioEffect("帯域 250Hz〜12kHz", ["演出：音質"], ["帯域 250Hz〜12kHz", "250Hzから12kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A214Effect : FxA2 { public override string Label => "帯域 250Hz〜12kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 250, 12000); }

[AudioEffect("帯域 250Hz〜16kHz", ["演出：音質"], ["帯域 250Hz〜16kHz", "250Hzから16kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A215Effect : FxA2 { public override string Label => "帯域 250Hz〜16kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 250, 16000); }

[AudioEffect("帯域 500Hz〜3kHz", ["演出：音質"], ["帯域 500Hz〜3kHz", "500Hzから3kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A216Effect : FxA2 { public override string Label => "帯域 500Hz〜3kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 500, 3000); }

[AudioEffect("帯域 500Hz〜5kHz", ["演出：音質"], ["帯域 500Hz〜5kHz", "500Hzから5kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A217Effect : FxA2 { public override string Label => "帯域 500Hz〜5kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 500, 5000); }

[AudioEffect("帯域 500Hz〜8kHz", ["演出：音質"], ["帯域 500Hz〜8kHz", "500Hzから8kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A218Effect : FxA2 { public override string Label => "帯域 500Hz〜8kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 500, 8000); }

[AudioEffect("帯域 500Hz〜12kHz", ["演出：音質"], ["帯域 500Hz〜12kHz", "500Hzから12kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A219Effect : FxA2 { public override string Label => "帯域 500Hz〜12kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 500, 12000); }

[AudioEffect("帯域 500Hz〜16kHz", ["演出：音質"], ["帯域 500Hz〜16kHz", "500Hzから16kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A220Effect : FxA2 { public override string Label => "帯域 500Hz〜16kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 500, 16000); }

[AudioEffect("帯域 1kHz〜3kHz", ["演出：音質"], ["帯域 1kHz〜3kHz", "1kHzから3kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A221Effect : FxA2 { public override string Label => "帯域 1kHz〜3kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 1000, 3000); }

[AudioEffect("帯域 1kHz〜5kHz", ["演出：音質"], ["帯域 1kHz〜5kHz", "1kHzから5kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A222Effect : FxA2 { public override string Label => "帯域 1kHz〜5kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 1000, 5000); }

[AudioEffect("帯域 1kHz〜8kHz", ["演出：音質"], ["帯域 1kHz〜8kHz", "1kHzから8kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A223Effect : FxA2 { public override string Label => "帯域 1kHz〜8kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 1000, 8000); }

[AudioEffect("帯域 1kHz〜12kHz", ["演出：音質"], ["帯域 1kHz〜12kHz", "1kHzから12kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A224Effect : FxA2 { public override string Label => "帯域 1kHz〜12kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 1000, 12000); }

[AudioEffect("帯域 1kHz〜16kHz", ["演出：音質"], ["帯域 1kHz〜16kHz", "1kHzから16kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A225Effect : FxA2 { public override string Label => "帯域 1kHz〜16kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 1000, 16000); }

[AudioEffect("帯域 2kHz〜3kHz", ["演出：音質"], ["帯域 2kHz〜3kHz", "2kHzから3kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A226Effect : FxA2 { public override string Label => "帯域 2kHz〜3kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 2000, 3000); }

[AudioEffect("帯域 2kHz〜5kHz", ["演出：音質"], ["帯域 2kHz〜5kHz", "2kHzから5kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A227Effect : FxA2 { public override string Label => "帯域 2kHz〜5kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 2000, 5000); }

[AudioEffect("帯域 2kHz〜8kHz", ["演出：音質"], ["帯域 2kHz〜8kHz", "2kHzから8kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A228Effect : FxA2 { public override string Label => "帯域 2kHz〜8kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 2000, 8000); }

[AudioEffect("帯域 2kHz〜12kHz", ["演出：音質"], ["帯域 2kHz〜12kHz", "2kHzから12kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A229Effect : FxA2 { public override string Label => "帯域 2kHz〜12kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 2000, 12000); }

[AudioEffect("帯域 2kHz〜16kHz", ["演出：音質"], ["帯域 2kHz〜16kHz", "2kHzから16kHzの音だけを残す", "音質", "演出"])]
public sealed class Fx4A230Effect : FxA2 { public override string Label => "帯域 2kHz〜16kHz"; internal override AudioKernel CreateKernel() => new BandK(this, 2000, 16000); }

[AudioEffect("EQ 60Hz +3dB", ["演出：音質"], ["EQ 60Hz +3dB", "60Hz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A231Effect : FxA2 { public override string Label => "EQ 60Hz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (60, 1, 3)); }

[AudioEffect("EQ 60Hz +6dB", ["演出：音質"], ["EQ 60Hz +6dB", "60Hz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A232Effect : FxA2 { public override string Label => "EQ 60Hz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (60, 1, 6)); }

[AudioEffect("EQ 60Hz -3dB", ["演出：音質"], ["EQ 60Hz -3dB", "60Hz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A233Effect : FxA2 { public override string Label => "EQ 60Hz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (60, 1, -3)); }

[AudioEffect("EQ 60Hz -6dB", ["演出：音質"], ["EQ 60Hz -6dB", "60Hz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A234Effect : FxA2 { public override string Label => "EQ 60Hz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (60, 1, -6)); }

[AudioEffect("EQ 120Hz +3dB", ["演出：音質"], ["EQ 120Hz +3dB", "120Hz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A235Effect : FxA2 { public override string Label => "EQ 120Hz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (120, 1, 3)); }

[AudioEffect("EQ 120Hz +6dB", ["演出：音質"], ["EQ 120Hz +6dB", "120Hz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A236Effect : FxA2 { public override string Label => "EQ 120Hz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (120, 1, 6)); }

[AudioEffect("EQ 120Hz -3dB", ["演出：音質"], ["EQ 120Hz -3dB", "120Hz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A237Effect : FxA2 { public override string Label => "EQ 120Hz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (120, 1, -3)); }

[AudioEffect("EQ 120Hz -6dB", ["演出：音質"], ["EQ 120Hz -6dB", "120Hz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A238Effect : FxA2 { public override string Label => "EQ 120Hz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (120, 1, -6)); }

[AudioEffect("EQ 250Hz +3dB", ["演出：音質"], ["EQ 250Hz +3dB", "250Hz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A239Effect : FxA2 { public override string Label => "EQ 250Hz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (250, 1, 3)); }

[AudioEffect("EQ 250Hz +6dB", ["演出：音質"], ["EQ 250Hz +6dB", "250Hz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A240Effect : FxA2 { public override string Label => "EQ 250Hz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (250, 1, 6)); }

[AudioEffect("EQ 250Hz -3dB", ["演出：音質"], ["EQ 250Hz -3dB", "250Hz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A241Effect : FxA2 { public override string Label => "EQ 250Hz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (250, 1, -3)); }

[AudioEffect("EQ 250Hz -6dB", ["演出：音質"], ["EQ 250Hz -6dB", "250Hz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A242Effect : FxA2 { public override string Label => "EQ 250Hz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (250, 1, -6)); }

[AudioEffect("EQ 500Hz +3dB", ["演出：音質"], ["EQ 500Hz +3dB", "500Hz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A243Effect : FxA2 { public override string Label => "EQ 500Hz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (500, 1, 3)); }

[AudioEffect("EQ 500Hz +6dB", ["演出：音質"], ["EQ 500Hz +6dB", "500Hz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A244Effect : FxA2 { public override string Label => "EQ 500Hz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (500, 1, 6)); }

[AudioEffect("EQ 500Hz -3dB", ["演出：音質"], ["EQ 500Hz -3dB", "500Hz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A245Effect : FxA2 { public override string Label => "EQ 500Hz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (500, 1, -3)); }

[AudioEffect("EQ 500Hz -6dB", ["演出：音質"], ["EQ 500Hz -6dB", "500Hz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A246Effect : FxA2 { public override string Label => "EQ 500Hz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (500, 1, -6)); }

[AudioEffect("EQ 1kHz +3dB", ["演出：音質"], ["EQ 1kHz +3dB", "1kHz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A247Effect : FxA2 { public override string Label => "EQ 1kHz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (1000, 1, 3)); }

[AudioEffect("EQ 1kHz +6dB", ["演出：音質"], ["EQ 1kHz +6dB", "1kHz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A248Effect : FxA2 { public override string Label => "EQ 1kHz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (1000, 1, 6)); }

[AudioEffect("EQ 1kHz -3dB", ["演出：音質"], ["EQ 1kHz -3dB", "1kHz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A249Effect : FxA2 { public override string Label => "EQ 1kHz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (1000, 1, -3)); }

[AudioEffect("EQ 1kHz -6dB", ["演出：音質"], ["EQ 1kHz -6dB", "1kHz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A250Effect : FxA2 { public override string Label => "EQ 1kHz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (1000, 1, -6)); }

[AudioEffect("EQ 2kHz +3dB", ["演出：音質"], ["EQ 2kHz +3dB", "2kHz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A251Effect : FxA2 { public override string Label => "EQ 2kHz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (2000, 1, 3)); }

[AudioEffect("EQ 2kHz +6dB", ["演出：音質"], ["EQ 2kHz +6dB", "2kHz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A252Effect : FxA2 { public override string Label => "EQ 2kHz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (2000, 1, 6)); }

[AudioEffect("EQ 2kHz -3dB", ["演出：音質"], ["EQ 2kHz -3dB", "2kHz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A253Effect : FxA2 { public override string Label => "EQ 2kHz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (2000, 1, -3)); }

[AudioEffect("EQ 2kHz -6dB", ["演出：音質"], ["EQ 2kHz -6dB", "2kHz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A254Effect : FxA2 { public override string Label => "EQ 2kHz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (2000, 1, -6)); }

[AudioEffect("EQ 4kHz +3dB", ["演出：音質"], ["EQ 4kHz +3dB", "4kHz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A255Effect : FxA2 { public override string Label => "EQ 4kHz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (4000, 1, 3)); }

[AudioEffect("EQ 4kHz +6dB", ["演出：音質"], ["EQ 4kHz +6dB", "4kHz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A256Effect : FxA2 { public override string Label => "EQ 4kHz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (4000, 1, 6)); }

[AudioEffect("EQ 4kHz -3dB", ["演出：音質"], ["EQ 4kHz -3dB", "4kHz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A257Effect : FxA2 { public override string Label => "EQ 4kHz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (4000, 1, -3)); }

[AudioEffect("EQ 4kHz -6dB", ["演出：音質"], ["EQ 4kHz -6dB", "4kHz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A258Effect : FxA2 { public override string Label => "EQ 4kHz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (4000, 1, -6)); }

[AudioEffect("EQ 8kHz +3dB", ["演出：音質"], ["EQ 8kHz +3dB", "8kHz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A259Effect : FxA2 { public override string Label => "EQ 8kHz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (8000, 1, 3)); }

[AudioEffect("EQ 8kHz +6dB", ["演出：音質"], ["EQ 8kHz +6dB", "8kHz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A260Effect : FxA2 { public override string Label => "EQ 8kHz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (8000, 1, 6)); }

[AudioEffect("EQ 8kHz -3dB", ["演出：音質"], ["EQ 8kHz -3dB", "8kHz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A261Effect : FxA2 { public override string Label => "EQ 8kHz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (8000, 1, -3)); }

[AudioEffect("EQ 8kHz -6dB", ["演出：音質"], ["EQ 8kHz -6dB", "8kHz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A262Effect : FxA2 { public override string Label => "EQ 8kHz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (8000, 1, -6)); }

[AudioEffect("EQ 12kHz +3dB", ["演出：音質"], ["EQ 12kHz +3dB", "12kHz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A263Effect : FxA2 { public override string Label => "EQ 12kHz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (12000, 1, 3)); }

[AudioEffect("EQ 12kHz +6dB", ["演出：音質"], ["EQ 12kHz +6dB", "12kHz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A264Effect : FxA2 { public override string Label => "EQ 12kHz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (12000, 1, 6)); }

[AudioEffect("EQ 12kHz -3dB", ["演出：音質"], ["EQ 12kHz -3dB", "12kHz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A265Effect : FxA2 { public override string Label => "EQ 12kHz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (12000, 1, -3)); }

[AudioEffect("EQ 12kHz -6dB", ["演出：音質"], ["EQ 12kHz -6dB", "12kHz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A266Effect : FxA2 { public override string Label => "EQ 12kHz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (12000, 1, -6)); }

[AudioEffect("EQ 16kHz +3dB", ["演出：音質"], ["EQ 16kHz +3dB", "16kHz付近を+3dB調整する", "音質", "演出"])]
public sealed class Fx4A267Effect : FxA2 { public override string Label => "EQ 16kHz +3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (16000, 1, 3)); }

[AudioEffect("EQ 16kHz +6dB", ["演出：音質"], ["EQ 16kHz +6dB", "16kHz付近を+6dB調整する", "音質", "演出"])]
public sealed class Fx4A268Effect : FxA2 { public override string Label => "EQ 16kHz +6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (16000, 1, 6)); }

[AudioEffect("EQ 16kHz -3dB", ["演出：音質"], ["EQ 16kHz -3dB", "16kHz付近を-3dB調整する", "音質", "演出"])]
public sealed class Fx4A269Effect : FxA2 { public override string Label => "EQ 16kHz -3dB"; internal override AudioKernel CreateKernel() => new EqK(this, (16000, 1, -3)); }

[AudioEffect("EQ 16kHz -6dB", ["演出：音質"], ["EQ 16kHz -6dB", "16kHz付近を-6dB調整する", "音質", "演出"])]
public sealed class Fx4A270Effect : FxA2 { public override string Label => "EQ 16kHz -6dB"; internal override AudioKernel CreateKernel() => new EqK(this, (16000, 1, -6)); }

[AudioEffect("用途EQ ボーカル前出し", ["演出：音質"], ["用途EQ ボーカル前出し", "声を前に出す", "音質", "演出"])]
public sealed class Fx4A271Effect : FxA2 { public override string Label => "用途EQ ボーカル前出し"; internal override AudioKernel CreateKernel() => new EqK(this, (200,.9,-2), (3000,.9,4), (11000,.7,2)); }

[AudioEffect("用途EQ ボーカル太く", ["演出：音質"], ["用途EQ ボーカル太く", "声を太くする", "音質", "演出"])]
public sealed class Fx4A272Effect : FxA2 { public override string Label => "用途EQ ボーカル太く"; internal override AudioKernel CreateKernel() => new EqK(this, (140,.8,4), (2500,1,1)); }

[AudioEffect("用途EQ ささやき強調", ["演出：音質"], ["用途EQ ささやき強調", "息の成分を出す", "音質", "演出"])]
public sealed class Fx4A273Effect : FxA2 { public override string Label => "用途EQ ささやき強調"; internal override AudioKernel CreateKernel() => new EqK(this, (6000,.8,4), (12000,.7,5)); }

[AudioEffect("用途EQ アコギ", ["演出：音質"], ["用途EQ アコギ", "アコースティックギターの輪郭", "音質", "演出"])]
public sealed class Fx4A274Effect : FxA2 { public override string Label => "用途EQ アコギ"; internal override AudioKernel CreateKernel() => new EqK(this, (100,.8,-3), (2500,1,3), (9000,.8,3)); }

[AudioEffect("用途EQ エレキ", ["演出：音質"], ["用途EQ エレキ", "エレキギターの押し出し", "音質", "演出"])]
public sealed class Fx4A275Effect : FxA2 { public override string Label => "用途EQ エレキ"; internal override AudioKernel CreateKernel() => new EqK(this, (120,.8,-2), (1800,1,4), (4500,1,2)); }

[AudioEffect("用途EQ ベース", ["演出：音質"], ["用途EQ ベース", "ベースの芯", "音質", "演出"])]
public sealed class Fx4A276Effect : FxA2 { public override string Label => "用途EQ ベース"; internal override AudioKernel CreateKernel() => new EqK(this, (80,.8,4), (800,1,2), (3000,1,-2)); }

[AudioEffect("用途EQ ピアノ", ["演出：音質"], ["用途EQ ピアノ", "ピアノの明るさ", "音質", "演出"])]
public sealed class Fx4A277Effect : FxA2 { public override string Label => "用途EQ ピアノ"; internal override AudioKernel CreateKernel() => new EqK(this, (120,.8,-1), (3200,.8,2), (10000,.7,3)); }

[AudioEffect("用途EQ ドラム", ["演出：音質"], ["用途EQ ドラム", "打楽器の迫力", "音質", "演出"])]
public sealed class Fx4A278Effect : FxA2 { public override string Label => "用途EQ ドラム"; internal override AudioKernel CreateKernel() => new EqK(this, (70,.9,4), (400,1.2,-3), (5000,.8,3)); }

[AudioEffect("用途EQ シンセ", ["演出：音質"], ["用途EQ シンセ", "シンセのきらめき", "音質", "演出"])]
public sealed class Fx4A279Effect : FxA2 { public override string Label => "用途EQ シンセ"; internal override AudioKernel CreateKernel() => new EqK(this, (300,.8,-2), (7000,.8,4)); }

[AudioEffect("用途EQ 弦楽器", ["演出：音質"], ["用途EQ 弦楽器", "弦の温かさ", "音質", "演出"])]
public sealed class Fx4A280Effect : FxA2 { public override string Label => "用途EQ 弦楽器"; internal override AudioKernel CreateKernel() => new EqK(this, (250,.8,2), (3500,1,-2), (9000,.8,2)); }

[AudioEffect("用途EQ 管楽器", ["演出：音質"], ["用途EQ 管楽器", "管楽器の張り", "音質", "演出"])]
public sealed class Fx4A281Effect : FxA2 { public override string Label => "用途EQ 管楽器"; internal override AudioKernel CreateKernel() => new EqK(this, (500,.9,1), (2200,1,3)); }

[AudioEffect("用途EQ 合唱", ["演出：音質"], ["用途EQ 合唱", "合唱の広がり", "音質", "演出"])]
public sealed class Fx4A282Effect : FxA2 { public override string Label => "用途EQ 合唱"; internal override AudioKernel CreateKernel() => new EqK(this, (200,.9,-2), (5000,.7,3)); }

[AudioEffect("用途EQ ゲーム音", ["演出：音質"], ["用途EQ ゲーム音", "電子音を聞きやすく", "音質", "演出"])]
public sealed class Fx4A283Effect : FxA2 { public override string Label => "用途EQ ゲーム音"; internal override AudioKernel CreateKernel() => new EqK(this, (180,.8,-2), (3000,1,3)); }

[AudioEffect("用途EQ 環境音", ["演出：音質"], ["用途EQ 環境音", "自然の音を軽く", "音質", "演出"])]
public sealed class Fx4A284Effect : FxA2 { public override string Label => "用途EQ 環境音"; internal override AudioKernel CreateKernel() => new EqK(this, (100,.8,-4), (8000,.7,3)); }

[AudioEffect("用途EQ 雨音", ["演出：音質"], ["用途EQ 雨音", "雨音の細かさ", "音質", "演出"])]
public sealed class Fx4A285Effect : FxA2 { public override string Label => "用途EQ 雨音"; internal override AudioKernel CreateKernel() => new EqK(this, (300,.8,-3), (6000,.8,4)); }

[AudioEffect("用途EQ 波音", ["演出：音質"], ["用途EQ 波音", "波音の深さ", "音質", "演出"])]
public sealed class Fx4A286Effect : FxA2 { public override string Label => "用途EQ 波音"; internal override AudioKernel CreateKernel() => new EqK(this, (90,.8,3), (1200,.8,-2)); }

[AudioEffect("用途EQ 風音", ["演出：音質"], ["用途EQ 風音", "風音を柔らかく", "音質", "演出"])]
public sealed class Fx4A287Effect : FxA2 { public override string Label => "用途EQ 風音"; internal override AudioKernel CreateKernel() => new EqK(this, (600,.7,2), (6000,.8,-5)); }

[AudioEffect("用途EQ 足音", ["演出：音質"], ["用途EQ 足音", "足音の重さ", "音質", "演出"])]
public sealed class Fx4A288Effect : FxA2 { public override string Label => "用途EQ 足音"; internal override AudioKernel CreateKernel() => new EqK(this, (110,1,5), (3000,1,-2)); }

[AudioEffect("用途EQ 爆発音", ["演出：音質"], ["用途EQ 爆発音", "爆発の低音", "音質", "演出"])]
public sealed class Fx4A289Effect : FxA2 { public override string Label => "用途EQ 爆発音"; internal override AudioKernel CreateKernel() => new EqK(this, (55,.8,6), (250,.9,2)); }

[AudioEffect("用途EQ 金属音", ["演出：音質"], ["用途EQ 金属音", "金属の響き", "音質", "演出"])]
public sealed class Fx4A290Effect : FxA2 { public override string Label => "用途EQ 金属音"; internal override AudioKernel CreateKernel() => new EqK(this, (2800,2,5), (7000,2,4)); }

[AudioEffect("用途EQ ポッドキャスト", ["演出：音質"], ["用途EQ ポッドキャスト", "語りの聞きやすさ", "音質", "演出"])]
public sealed class Fx4A291Effect : FxA2 { public override string Label => "用途EQ ポッドキャスト"; internal override AudioKernel CreateKernel() => new EqK(this, (100,.8,-3), (2800,.9,3)); }

[AudioEffect("用途EQ 朗読", ["演出：音質"], ["用途EQ 朗読", "落ち着いた朗読の声", "音質", "演出"])]
public sealed class Fx4A292Effect : FxA2 { public override string Label => "用途EQ 朗読"; internal override AudioKernel CreateKernel() => new EqK(this, (180,.8,2), (3000,1,1), (9000,.8,-2)); }

[AudioEffect("用途EQ 実況 高音控えめ", ["演出：音質"], ["用途EQ 実況 高音控えめ", "長時間でも疲れにくい", "音質", "演出"])]
public sealed class Fx4A293Effect : FxA2 { public override string Label => "用途EQ 実況 高音控えめ"; internal override AudioKernel CreateKernel() => new EqK(this, (2500,1,2), (7500,.9,-3)); }

[AudioEffect("用途EQ スマホ向け", ["演出：音質"], ["用途EQ スマホ向け", "小さなスピーカーで聞こえやすく", "音質", "演出"])]
public sealed class Fx4A294Effect : FxA2 { public override string Label => "用途EQ スマホ向け"; internal override AudioKernel CreateKernel() => new EqK(this, (180,.8,-5), (1500,.8,4)); }

[AudioEffect("用途EQ イヤホン向け", ["演出：音質"], ["用途EQ イヤホン向け", "イヤホンの刺さりを減らす", "音質", "演出"])]
public sealed class Fx4A295Effect : FxA2 { public override string Label => "用途EQ イヤホン向け"; internal override AudioKernel CreateKernel() => new EqK(this, (70,.8,1), (6000,1.3,-4)); }

[AudioEffect("用途EQ テレビ向け", ["演出：音質"], ["用途EQ テレビ向け", "テレビの音に合わせる", "音質", "演出"])]
public sealed class Fx4A296Effect : FxA2 { public override string Label => "用途EQ テレビ向け"; internal override AudioKernel CreateKernel() => new EqK(this, (120,.8,-2), (3000,.8,3)); }

[AudioEffect("用途EQ ドンシャリ", ["演出：音質"], ["用途EQ ドンシャリ", "低音と高音を強調", "音質", "演出"])]
public sealed class Fx4A297Effect : FxA2 { public override string Label => "用途EQ ドンシャリ"; internal override AudioKernel CreateKernel() => new EqK(this, (80,.8,6), (1000,.7,-4), (10000,.7,6)); }

[AudioEffect("用途EQ かまぼこ", ["演出：音質"], ["用途EQ かまぼこ", "中音を強調", "音質", "演出"])]
public sealed class Fx4A298Effect : FxA2 { public override string Label => "用途EQ かまぼこ"; internal override AudioKernel CreateKernel() => new EqK(this, (100,.8,-4), (1200,.6,5), (9000,.8,-4)); }

[AudioEffect("用途EQ フラット寄り", ["演出：音質"], ["用途EQ フラット寄り", "ごく軽く整える", "音質", "演出"])]
public sealed class Fx4A299Effect : FxA2 { public override string Label => "用途EQ フラット寄り"; internal override AudioKernel CreateKernel() => new EqK(this, (250,.8,-1), (5000,.8,1)); }

[AudioEffect("用途EQ 夜間向け", ["演出：音質"], ["用途EQ 夜間向け", "低音を抑えて静かに", "音質", "演出"])]
public sealed class Fx4A300Effect : FxA2 { public override string Label => "用途EQ 夜間向け"; internal override AudioKernel CreateKernel() => new EqK(this, (90,.8,-6), (5000,.9,-2)); }

[AudioEffect("エコー 60ms 短く", ["演出：エコー"], ["エコー 60ms 短く", "60ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A301Effect : FxA2 { public override string Label => "エコー 60ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.06, 0.06, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 60ms 中くらい", ["演出：エコー"], ["エコー 60ms 中くらい", "60ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A302Effect : FxA2 { public override string Label => "エコー 60ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.06, 0.06, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 60ms 長く", ["演出：エコー"], ["エコー 60ms 長く", "60ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A303Effect : FxA2 { public override string Label => "エコー 60ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.06, 0.06, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 60ms", ["演出：エコー"], ["左右エコー 60ms", "60ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A304Effect : FxA2 { public override string Label => "左右エコー 60ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.06, 0.09, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 90ms 短く", ["演出：エコー"], ["エコー 90ms 短く", "90ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A305Effect : FxA2 { public override string Label => "エコー 90ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.09, 0.09, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 90ms 中くらい", ["演出：エコー"], ["エコー 90ms 中くらい", "90ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A306Effect : FxA2 { public override string Label => "エコー 90ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.09, 0.09, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 90ms 長く", ["演出：エコー"], ["エコー 90ms 長く", "90ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A307Effect : FxA2 { public override string Label => "エコー 90ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.09, 0.09, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 90ms", ["演出：エコー"], ["左右エコー 90ms", "90ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A308Effect : FxA2 { public override string Label => "左右エコー 90ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.09, 0.135, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 120ms 短く", ["演出：エコー"], ["エコー 120ms 短く", "120ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A309Effect : FxA2 { public override string Label => "エコー 120ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.12, 0.12, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 120ms 中くらい", ["演出：エコー"], ["エコー 120ms 中くらい", "120ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A310Effect : FxA2 { public override string Label => "エコー 120ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.12, 0.12, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 120ms 長く", ["演出：エコー"], ["エコー 120ms 長く", "120ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A311Effect : FxA2 { public override string Label => "エコー 120ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.12, 0.12, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 120ms", ["演出：エコー"], ["左右エコー 120ms", "120ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A312Effect : FxA2 { public override string Label => "左右エコー 120ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.12, 0.18, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 180ms 短く", ["演出：エコー"], ["エコー 180ms 短く", "180ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A313Effect : FxA2 { public override string Label => "エコー 180ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.18, 0.18, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 180ms 中くらい", ["演出：エコー"], ["エコー 180ms 中くらい", "180ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A314Effect : FxA2 { public override string Label => "エコー 180ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.18, 0.18, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 180ms 長く", ["演出：エコー"], ["エコー 180ms 長く", "180ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A315Effect : FxA2 { public override string Label => "エコー 180ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.18, 0.18, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 180ms", ["演出：エコー"], ["左右エコー 180ms", "180ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A316Effect : FxA2 { public override string Label => "左右エコー 180ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.18, 0.27, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 250ms 短く", ["演出：エコー"], ["エコー 250ms 短く", "250ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A317Effect : FxA2 { public override string Label => "エコー 250ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.25, 0.25, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 250ms 中くらい", ["演出：エコー"], ["エコー 250ms 中くらい", "250ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A318Effect : FxA2 { public override string Label => "エコー 250ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.25, 0.25, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 250ms 長く", ["演出：エコー"], ["エコー 250ms 長く", "250ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A319Effect : FxA2 { public override string Label => "エコー 250ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.25, 0.25, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 250ms", ["演出：エコー"], ["左右エコー 250ms", "250ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A320Effect : FxA2 { public override string Label => "左右エコー 250ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.25, 0.375, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 333ms 短く", ["演出：エコー"], ["エコー 333ms 短く", "333ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A321Effect : FxA2 { public override string Label => "エコー 333ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.333, 0.333, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 333ms 中くらい", ["演出：エコー"], ["エコー 333ms 中くらい", "333ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A322Effect : FxA2 { public override string Label => "エコー 333ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.333, 0.333, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 333ms 長く", ["演出：エコー"], ["エコー 333ms 長く", "333ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A323Effect : FxA2 { public override string Label => "エコー 333ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.333, 0.333, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 333ms", ["演出：エコー"], ["左右エコー 333ms", "333ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A324Effect : FxA2 { public override string Label => "左右エコー 333ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.333, 0.4995, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 500ms 短く", ["演出：エコー"], ["エコー 500ms 短く", "500ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A325Effect : FxA2 { public override string Label => "エコー 500ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.5, 0.5, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 500ms 中くらい", ["演出：エコー"], ["エコー 500ms 中くらい", "500ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A326Effect : FxA2 { public override string Label => "エコー 500ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.5, 0.5, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 500ms 長く", ["演出：エコー"], ["エコー 500ms 長く", "500ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A327Effect : FxA2 { public override string Label => "エコー 500ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.5, 0.5, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 500ms", ["演出：エコー"], ["左右エコー 500ms", "500ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A328Effect : FxA2 { public override string Label => "左右エコー 500ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.5, 0.75, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 666ms 短く", ["演出：エコー"], ["エコー 666ms 短く", "666ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A329Effect : FxA2 { public override string Label => "エコー 666ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.666, 0.666, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 666ms 中くらい", ["演出：エコー"], ["エコー 666ms 中くらい", "666ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A330Effect : FxA2 { public override string Label => "エコー 666ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.666, 0.666, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 666ms 長く", ["演出：エコー"], ["エコー 666ms 長く", "666ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A331Effect : FxA2 { public override string Label => "エコー 666ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.666, 0.666, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 666ms", ["演出：エコー"], ["左右エコー 666ms", "666ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A332Effect : FxA2 { public override string Label => "左右エコー 666ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.666, 0.999, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 800ms 短く", ["演出：エコー"], ["エコー 800ms 短く", "800ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A333Effect : FxA2 { public override string Label => "エコー 800ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.8, 0.8, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 800ms 中くらい", ["演出：エコー"], ["エコー 800ms 中くらい", "800ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A334Effect : FxA2 { public override string Label => "エコー 800ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.8, 0.8, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 800ms 長く", ["演出：エコー"], ["エコー 800ms 長く", "800ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A335Effect : FxA2 { public override string Label => "エコー 800ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.8, 0.8, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 800ms", ["演出：エコー"], ["左右エコー 800ms", "800ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A336Effect : FxA2 { public override string Label => "左右エコー 800ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 0.8, 1.2, .45f, 150, 7000, .5f, true); }

[AudioEffect("エコー 1000ms 短く", ["演出：エコー"], ["エコー 1000ms 短く", "1000ミリ秒の反響が短く続く", "エコー", "演出"])]
public sealed class Fx4A337Effect : FxA2 { public override string Label => "エコー 1000ms 短く"; internal override AudioKernel CreateKernel() => new EchoK(this, 1, 1, 0.25f, 120, 8000, .5f); }

[AudioEffect("エコー 1000ms 中くらい", ["演出：エコー"], ["エコー 1000ms 中くらい", "1000ミリ秒の反響が中くらい続く", "エコー", "演出"])]
public sealed class Fx4A338Effect : FxA2 { public override string Label => "エコー 1000ms 中くらい"; internal override AudioKernel CreateKernel() => new EchoK(this, 1, 1, 0.45f, 120, 8000, .5f); }

[AudioEffect("エコー 1000ms 長く", ["演出：エコー"], ["エコー 1000ms 長く", "1000ミリ秒の反響が長く続く", "エコー", "演出"])]
public sealed class Fx4A339Effect : FxA2 { public override string Label => "エコー 1000ms 長く"; internal override AudioKernel CreateKernel() => new EchoK(this, 1, 1, 0.65f, 120, 8000, .5f); }

[AudioEffect("左右エコー 1000ms", ["演出：エコー"], ["左右エコー 1000ms", "1000ミリ秒の反響が左右へ跳ねる", "エコー", "演出"])]
public sealed class Fx4A340Effect : FxA2 { public override string Label => "左右エコー 1000ms"; internal override AudioKernel CreateKernel() => new EchoK(this, 1, 1.5, .45f, 150, 7000, .5f, true); }

[AudioEffect("音量ゆれ 波 とても遅い", ["演出：変調"], ["音量ゆれ 波 とても遅い", "とても遅い波形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A341Effect : FxA3 { public override string Label => "音量ゆれ 波 とても遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 1, 3, 0); }

[AudioEffect("音量ゆれ 四角 とても遅い", ["演出：変調"], ["音量ゆれ 四角 とても遅い", "とても遅い四角形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A342Effect : FxA3 { public override string Label => "音量ゆれ 四角 とても遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 1, 3, 1); }

[AudioEffect("音量ゆれ のこぎり とても遅い", ["演出：変調"], ["音量ゆれ のこぎり とても遅い", "とても遅いのこぎり形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A343Effect : FxA3 { public override string Label => "音量ゆれ のこぎり とても遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 1, 3, 2); }

[AudioEffect("音量ゆれ 鼓動 とても遅い", ["演出：変調"], ["音量ゆれ 鼓動 とても遅い", "とても遅い鼓動形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A344Effect : FxA3 { public override string Label => "音量ゆれ 鼓動 とても遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 1, 3, 3); }

[AudioEffect("左右に往復 とても遅い", ["演出：変調"], ["左右に往復 とても遅い", "音がとても遅い速さで左右へ移る", "変調", "演出"])]
public sealed class Fx4A345Effect : FxA3 { public override string Label => "左右に往復 とても遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 1, 3, 0, true); }

[AudioEffect("音量ゆれ 波 遅い", ["演出：変調"], ["音量ゆれ 波 遅い", "遅い波形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A346Effect : FxA3 { public override string Label => "音量ゆれ 波 遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 2, 5, 0); }

[AudioEffect("音量ゆれ 四角 遅い", ["演出：変調"], ["音量ゆれ 四角 遅い", "遅い四角形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A347Effect : FxA3 { public override string Label => "音量ゆれ 四角 遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 2, 5, 1); }

[AudioEffect("音量ゆれ のこぎり 遅い", ["演出：変調"], ["音量ゆれ のこぎり 遅い", "遅いのこぎり形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A348Effect : FxA3 { public override string Label => "音量ゆれ のこぎり 遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 2, 5, 2); }

[AudioEffect("音量ゆれ 鼓動 遅い", ["演出：変調"], ["音量ゆれ 鼓動 遅い", "遅い鼓動形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A349Effect : FxA3 { public override string Label => "音量ゆれ 鼓動 遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 2, 5, 3); }

[AudioEffect("左右に往復 遅い", ["演出：変調"], ["左右に往復 遅い", "音が遅い速さで左右へ移る", "変調", "演出"])]
public sealed class Fx4A350Effect : FxA3 { public override string Label => "左右に往復 遅い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 2, 5, 0, true); }

[AudioEffect("音量ゆれ 波 普通", ["演出：変調"], ["音量ゆれ 波 普通", "普通波形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A351Effect : FxA3 { public override string Label => "音量ゆれ 波 普通"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 4, 8, 0); }

[AudioEffect("音量ゆれ 四角 普通", ["演出：変調"], ["音量ゆれ 四角 普通", "普通四角形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A352Effect : FxA3 { public override string Label => "音量ゆれ 四角 普通"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 4, 8, 1); }

[AudioEffect("音量ゆれ のこぎり 普通", ["演出：変調"], ["音量ゆれ のこぎり 普通", "普通のこぎり形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A353Effect : FxA3 { public override string Label => "音量ゆれ のこぎり 普通"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 4, 8, 2); }

[AudioEffect("音量ゆれ 鼓動 普通", ["演出：変調"], ["音量ゆれ 鼓動 普通", "普通鼓動形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A354Effect : FxA3 { public override string Label => "音量ゆれ 鼓動 普通"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 4, 8, 3); }

[AudioEffect("左右に往復 普通", ["演出：変調"], ["左右に往復 普通", "音が普通速さで左右へ移る", "変調", "演出"])]
public sealed class Fx4A355Effect : FxA3 { public override string Label => "左右に往復 普通"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 4, 8, 0, true); }

[AudioEffect("音量ゆれ 波 速い", ["演出：変調"], ["音量ゆれ 波 速い", "速い波形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A356Effect : FxA3 { public override string Label => "音量ゆれ 波 速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 6, 12, 0); }

[AudioEffect("音量ゆれ 四角 速い", ["演出：変調"], ["音量ゆれ 四角 速い", "速い四角形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A357Effect : FxA3 { public override string Label => "音量ゆれ 四角 速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 6, 12, 1); }

[AudioEffect("音量ゆれ のこぎり 速い", ["演出：変調"], ["音量ゆれ のこぎり 速い", "速いのこぎり形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A358Effect : FxA3 { public override string Label => "音量ゆれ のこぎり 速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 6, 12, 2); }

[AudioEffect("音量ゆれ 鼓動 速い", ["演出：変調"], ["音量ゆれ 鼓動 速い", "速い鼓動形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A359Effect : FxA3 { public override string Label => "音量ゆれ 鼓動 速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 6, 12, 3); }

[AudioEffect("左右に往復 速い", ["演出：変調"], ["左右に往復 速い", "音が速い速さで左右へ移る", "変調", "演出"])]
public sealed class Fx4A360Effect : FxA3 { public override string Label => "左右に往復 速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 6, 12, 0, true); }

[AudioEffect("音量ゆれ 波 とても速い", ["演出：変調"], ["音量ゆれ 波 とても速い", "とても速い波形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A361Effect : FxA3 { public override string Label => "音量ゆれ 波 とても速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 10, 18, 0); }

[AudioEffect("音量ゆれ 四角 とても速い", ["演出：変調"], ["音量ゆれ 四角 とても速い", "とても速い四角形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A362Effect : FxA3 { public override string Label => "音量ゆれ 四角 とても速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 10, 18, 1); }

[AudioEffect("音量ゆれ のこぎり とても速い", ["演出：変調"], ["音量ゆれ のこぎり とても速い", "とても速いのこぎり形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A363Effect : FxA3 { public override string Label => "音量ゆれ のこぎり とても速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 10, 18, 2); }

[AudioEffect("音量ゆれ 鼓動 とても速い", ["演出：変調"], ["音量ゆれ 鼓動 とても速い", "とても速い鼓動形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A364Effect : FxA3 { public override string Label => "音量ゆれ 鼓動 とても速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 10, 18, 3); }

[AudioEffect("左右に往復 とても速い", ["演出：変調"], ["左右に往復 とても速い", "音がとても速い速さで左右へ移る", "変調", "演出"])]
public sealed class Fx4A365Effect : FxA3 { public override string Label => "左右に往復 とても速い"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 10, 18, 0, true); }

[AudioEffect("音量ゆれ 波 超高速", ["演出：変調"], ["音量ゆれ 波 超高速", "超高速波形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A366Effect : FxA3 { public override string Label => "音量ゆれ 波 超高速"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 16, 30, 0); }

[AudioEffect("音量ゆれ 四角 超高速", ["演出：変調"], ["音量ゆれ 四角 超高速", "超高速四角形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A367Effect : FxA3 { public override string Label => "音量ゆれ 四角 超高速"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 16, 30, 1); }

[AudioEffect("音量ゆれ のこぎり 超高速", ["演出：変調"], ["音量ゆれ のこぎり 超高速", "超高速のこぎり形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A368Effect : FxA3 { public override string Label => "音量ゆれ のこぎり 超高速"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 16, 30, 2); }

[AudioEffect("音量ゆれ 鼓動 超高速", ["演出：変調"], ["音量ゆれ 鼓動 超高速", "超高速鼓動形で音量がゆれる", "変調", "演出"])]
public sealed class Fx4A369Effect : FxA3 { public override string Label => "音量ゆれ 鼓動 超高速"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 16, 30, 3); }

[AudioEffect("左右に往復 超高速", ["演出：変調"], ["左右に往復 超高速", "音が超高速速さで左右へ移る", "変調", "演出"])]
public sealed class Fx4A370Effect : FxA3 { public override string Label => "左右に往復 超高速"; internal override AudioKernel CreateKernel() => new GainLfoK(this, 16, 30, 0, true); }

[AudioEffect("揺らぎ コーラス とても遅い", ["演出：変調"], ["揺らぎ コーラス とても遅い", "コーラスの揺れがとても遅い", "変調", "演出"])]
public sealed class Fx4A371Effect : FxA3 { public override string Label => "揺らぎ コーラス とても遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 18, 6, 0.1, 0.4, 0f, 0.8f); }

[AudioEffect("揺らぎ コーラス 遅い", ["演出：変調"], ["揺らぎ コーラス 遅い", "コーラスの揺れが遅い", "変調", "演出"])]
public sealed class Fx4A372Effect : FxA3 { public override string Label => "揺らぎ コーラス 遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 18, 6, 0.2, 0.8, 0f, 0.8f); }

[AudioEffect("揺らぎ コーラス 普通", ["演出：変調"], ["揺らぎ コーラス 普通", "コーラスの揺れが普通", "変調", "演出"])]
public sealed class Fx4A373Effect : FxA3 { public override string Label => "揺らぎ コーラス 普通"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 18, 6, 0.4, 1.5, 0f, 0.8f); }

[AudioEffect("揺らぎ コーラス やや速い", ["演出：変調"], ["揺らぎ コーラス やや速い", "コーラスの揺れがやや速い", "変調", "演出"])]
public sealed class Fx4A374Effect : FxA3 { public override string Label => "揺らぎ コーラス やや速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 18, 6, 0.8, 3, 0f, 0.8f); }

[AudioEffect("揺らぎ コーラス 速い", ["演出：変調"], ["揺らぎ コーラス 速い", "コーラスの揺れが速い", "変調", "演出"])]
public sealed class Fx4A375Effect : FxA3 { public override string Label => "揺らぎ コーラス 速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 18, 6, 1.5, 5, 0f, 0.8f); }

[AudioEffect("揺らぎ コーラス 超高速", ["演出：変調"], ["揺らぎ コーラス 超高速", "コーラスの揺れが超高速", "変調", "演出"])]
public sealed class Fx4A376Effect : FxA3 { public override string Label => "揺らぎ コーラス 超高速"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 18, 6, 3, 8, 0f, 0.8f); }

[AudioEffect("揺らぎ 広いコーラス とても遅い", ["演出：変調"], ["揺らぎ 広いコーラス とても遅い", "広いコーラスの揺れがとても遅い", "変調", "演出"])]
public sealed class Fx4A377Effect : FxA3 { public override string Label => "揺らぎ 広いコーラス とても遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 28, 10, 0.1, 0.4, 0.1f, 0.9f); }

[AudioEffect("揺らぎ 広いコーラス 遅い", ["演出：変調"], ["揺らぎ 広いコーラス 遅い", "広いコーラスの揺れが遅い", "変調", "演出"])]
public sealed class Fx4A378Effect : FxA3 { public override string Label => "揺らぎ 広いコーラス 遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 28, 10, 0.2, 0.8, 0.1f, 0.9f); }

[AudioEffect("揺らぎ 広いコーラス 普通", ["演出：変調"], ["揺らぎ 広いコーラス 普通", "広いコーラスの揺れが普通", "変調", "演出"])]
public sealed class Fx4A379Effect : FxA3 { public override string Label => "揺らぎ 広いコーラス 普通"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 28, 10, 0.4, 1.5, 0.1f, 0.9f); }

[AudioEffect("揺らぎ 広いコーラス やや速い", ["演出：変調"], ["揺らぎ 広いコーラス やや速い", "広いコーラスの揺れがやや速い", "変調", "演出"])]
public sealed class Fx4A380Effect : FxA3 { public override string Label => "揺らぎ 広いコーラス やや速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 28, 10, 0.8, 3, 0.1f, 0.9f); }

[AudioEffect("揺らぎ 広いコーラス 速い", ["演出：変調"], ["揺らぎ 広いコーラス 速い", "広いコーラスの揺れが速い", "変調", "演出"])]
public sealed class Fx4A381Effect : FxA3 { public override string Label => "揺らぎ 広いコーラス 速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 28, 10, 1.5, 5, 0.1f, 0.9f); }

[AudioEffect("揺らぎ 広いコーラス 超高速", ["演出：変調"], ["揺らぎ 広いコーラス 超高速", "広いコーラスの揺れが超高速", "変調", "演出"])]
public sealed class Fx4A382Effect : FxA3 { public override string Label => "揺らぎ 広いコーラス 超高速"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 28, 10, 3, 8, 0.1f, 0.9f); }

[AudioEffect("揺らぎ フランジャー とても遅い", ["演出：変調"], ["揺らぎ フランジャー とても遅い", "フランジャーの揺れがとても遅い", "変調", "演出"])]
public sealed class Fx4A383Effect : FxA3 { public override string Label => "揺らぎ フランジャー とても遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 3, 2.5, 0.1, 0.4, 0.5f, 0.9f); }

[AudioEffect("揺らぎ フランジャー 遅い", ["演出：変調"], ["揺らぎ フランジャー 遅い", "フランジャーの揺れが遅い", "変調", "演出"])]
public sealed class Fx4A384Effect : FxA3 { public override string Label => "揺らぎ フランジャー 遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 3, 2.5, 0.2, 0.8, 0.5f, 0.9f); }

[AudioEffect("揺らぎ フランジャー 普通", ["演出：変調"], ["揺らぎ フランジャー 普通", "フランジャーの揺れが普通", "変調", "演出"])]
public sealed class Fx4A385Effect : FxA3 { public override string Label => "揺らぎ フランジャー 普通"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 3, 2.5, 0.4, 1.5, 0.5f, 0.9f); }

[AudioEffect("揺らぎ フランジャー やや速い", ["演出：変調"], ["揺らぎ フランジャー やや速い", "フランジャーの揺れがやや速い", "変調", "演出"])]
public sealed class Fx4A386Effect : FxA3 { public override string Label => "揺らぎ フランジャー やや速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 3, 2.5, 0.8, 3, 0.5f, 0.9f); }

[AudioEffect("揺らぎ フランジャー 速い", ["演出：変調"], ["揺らぎ フランジャー 速い", "フランジャーの揺れが速い", "変調", "演出"])]
public sealed class Fx4A387Effect : FxA3 { public override string Label => "揺らぎ フランジャー 速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 3, 2.5, 1.5, 5, 0.5f, 0.9f); }

[AudioEffect("揺らぎ フランジャー 超高速", ["演出：変調"], ["揺らぎ フランジャー 超高速", "フランジャーの揺れが超高速", "変調", "演出"])]
public sealed class Fx4A388Effect : FxA3 { public override string Label => "揺らぎ フランジャー 超高速"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 3, 2.5, 3, 8, 0.5f, 0.9f); }

[AudioEffect("揺らぎ ジェットフランジャー とても遅い", ["演出：変調"], ["揺らぎ ジェットフランジャー とても遅い", "ジェットフランジャーの揺れがとても遅い", "変調", "演出"])]
public sealed class Fx4A389Effect : FxA3 { public override string Label => "揺らぎ ジェットフランジャー とても遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 2, 1.8, 0.1, 0.4, 0.75f, 1f); }

[AudioEffect("揺らぎ ジェットフランジャー 遅い", ["演出：変調"], ["揺らぎ ジェットフランジャー 遅い", "ジェットフランジャーの揺れが遅い", "変調", "演出"])]
public sealed class Fx4A390Effect : FxA3 { public override string Label => "揺らぎ ジェットフランジャー 遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 2, 1.8, 0.2, 0.8, 0.75f, 1f); }

[AudioEffect("揺らぎ ジェットフランジャー 普通", ["演出：変調"], ["揺らぎ ジェットフランジャー 普通", "ジェットフランジャーの揺れが普通", "変調", "演出"])]
public sealed class Fx4A391Effect : FxA3 { public override string Label => "揺らぎ ジェットフランジャー 普通"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 2, 1.8, 0.4, 1.5, 0.75f, 1f); }

[AudioEffect("揺らぎ ジェットフランジャー やや速い", ["演出：変調"], ["揺らぎ ジェットフランジャー やや速い", "ジェットフランジャーの揺れがやや速い", "変調", "演出"])]
public sealed class Fx4A392Effect : FxA3 { public override string Label => "揺らぎ ジェットフランジャー やや速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 2, 1.8, 0.8, 3, 0.75f, 1f); }

[AudioEffect("揺らぎ ジェットフランジャー 速い", ["演出：変調"], ["揺らぎ ジェットフランジャー 速い", "ジェットフランジャーの揺れが速い", "変調", "演出"])]
public sealed class Fx4A393Effect : FxA3 { public override string Label => "揺らぎ ジェットフランジャー 速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 2, 1.8, 1.5, 5, 0.75f, 1f); }

[AudioEffect("揺らぎ ジェットフランジャー 超高速", ["演出：変調"], ["揺らぎ ジェットフランジャー 超高速", "ジェットフランジャーの揺れが超高速", "変調", "演出"])]
public sealed class Fx4A394Effect : FxA3 { public override string Label => "揺らぎ ジェットフランジャー 超高速"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 2, 1.8, 3, 8, 0.75f, 1f); }

[AudioEffect("揺らぎ ビブラート とても遅い", ["演出：変調"], ["揺らぎ ビブラート とても遅い", "ビブラートの揺れがとても遅い", "変調", "演出"])]
public sealed class Fx4A395Effect : FxA3 { public override string Label => "揺らぎ ビブラート とても遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 6, 4, 0.1, 0.4, 0f, 6f); }

[AudioEffect("揺らぎ ビブラート 遅い", ["演出：変調"], ["揺らぎ ビブラート 遅い", "ビブラートの揺れが遅い", "変調", "演出"])]
public sealed class Fx4A396Effect : FxA3 { public override string Label => "揺らぎ ビブラート 遅い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 6, 4, 0.2, 0.8, 0f, 6f); }

[AudioEffect("揺らぎ ビブラート 普通", ["演出：変調"], ["揺らぎ ビブラート 普通", "ビブラートの揺れが普通", "変調", "演出"])]
public sealed class Fx4A397Effect : FxA3 { public override string Label => "揺らぎ ビブラート 普通"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 6, 4, 0.4, 1.5, 0f, 6f); }

[AudioEffect("揺らぎ ビブラート やや速い", ["演出：変調"], ["揺らぎ ビブラート やや速い", "ビブラートの揺れがやや速い", "変調", "演出"])]
public sealed class Fx4A398Effect : FxA3 { public override string Label => "揺らぎ ビブラート やや速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 6, 4, 0.8, 3, 0f, 6f); }

[AudioEffect("揺らぎ ビブラート 速い", ["演出：変調"], ["揺らぎ ビブラート 速い", "ビブラートの揺れが速い", "変調", "演出"])]
public sealed class Fx4A399Effect : FxA3 { public override string Label => "揺らぎ ビブラート 速い"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 6, 4, 1.5, 5, 0f, 6f); }

[AudioEffect("揺らぎ ビブラート 超高速", ["演出：変調"], ["揺らぎ ビブラート 超高速", "ビブラートの揺れが超高速", "変調", "演出"])]
public sealed class Fx4A400Effect : FxA3 { public override string Label => "揺らぎ ビブラート 超高速"; internal override AudioKernel CreateKernel() => new ModDelayK(this, 6, 4, 3, 8, 0f, 6f); }

[AudioEffect("開閉フィルター こもり とても遅い なだらか", ["演出：変調"], ["開閉フィルター こもり とても遅い なだらか", "こもりの帯域がとても遅い開閉する", "変調", "演出"])]
public sealed class Fx4A401Effect : FxA3 { public override string Label => "開閉フィルター こもり とても遅い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 0.8, 0.05, 0.3); }

[AudioEffect("開閉フィルター こもり とても遅い 鋭い", ["演出：変調"], ["開閉フィルター こもり とても遅い 鋭い", "こもりの帯域がとても遅い開閉する", "変調", "演出"])]
public sealed class Fx4A402Effect : FxA3 { public override string Label => "開閉フィルター こもり とても遅い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 3, 0.05, 0.3); }

[AudioEffect("開閉フィルター こもり 遅い なだらか", ["演出：変調"], ["開閉フィルター こもり 遅い なだらか", "こもりの帯域が遅い開閉する", "変調", "演出"])]
public sealed class Fx4A403Effect : FxA3 { public override string Label => "開閉フィルター こもり 遅い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 0.8, 0.15, 0.8); }

[AudioEffect("開閉フィルター こもり 遅い 鋭い", ["演出：変調"], ["開閉フィルター こもり 遅い 鋭い", "こもりの帯域が遅い開閉する", "変調", "演出"])]
public sealed class Fx4A404Effect : FxA3 { public override string Label => "開閉フィルター こもり 遅い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 3, 0.15, 0.8); }

[AudioEffect("開閉フィルター こもり 普通 なだらか", ["演出：変調"], ["開閉フィルター こもり 普通 なだらか", "こもりの帯域が普通開閉する", "変調", "演出"])]
public sealed class Fx4A405Effect : FxA3 { public override string Label => "開閉フィルター こもり 普通 なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 0.8, 0.4, 2); }

[AudioEffect("開閉フィルター こもり 普通 鋭い", ["演出：変調"], ["開閉フィルター こもり 普通 鋭い", "こもりの帯域が普通開閉する", "変調", "演出"])]
public sealed class Fx4A406Effect : FxA3 { public override string Label => "開閉フィルター こもり 普通 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 3, 0.4, 2); }

[AudioEffect("開閉フィルター こもり 速い なだらか", ["演出：変調"], ["開閉フィルター こもり 速い なだらか", "こもりの帯域が速い開閉する", "変調", "演出"])]
public sealed class Fx4A407Effect : FxA3 { public override string Label => "開閉フィルター こもり 速い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 0.8, 1, 5); }

[AudioEffect("開閉フィルター こもり 速い 鋭い", ["演出：変調"], ["開閉フィルター こもり 速い 鋭い", "こもりの帯域が速い開閉する", "変調", "演出"])]
public sealed class Fx4A408Effect : FxA3 { public override string Label => "開閉フィルター こもり 速い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 3, 1, 5); }

[AudioEffect("開閉フィルター こもり 超高速 なだらか", ["演出：変調"], ["開閉フィルター こもり 超高速 なだらか", "こもりの帯域が超高速開閉する", "変調", "演出"])]
public sealed class Fx4A409Effect : FxA3 { public override string Label => "開閉フィルター こもり 超高速 なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 0.8, 3, 10); }

[AudioEffect("開閉フィルター こもり 超高速 鋭い", ["演出：変調"], ["開閉フィルター こもり 超高速 鋭い", "こもりの帯域が超高速開閉する", "変調", "演出"])]
public sealed class Fx4A410Effect : FxA3 { public override string Label => "開閉フィルター こもり 超高速 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 300, 8000, 0, 3, 3, 10); }

[AudioEffect("開閉フィルター ワウ とても遅い なだらか", ["演出：変調"], ["開閉フィルター ワウ とても遅い なだらか", "ワウの帯域がとても遅い開閉する", "変調", "演出"])]
public sealed class Fx4A411Effect : FxA3 { public override string Label => "開閉フィルター ワウ とても遅い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 0.8, 0.05, 0.3); }

[AudioEffect("開閉フィルター ワウ とても遅い 鋭い", ["演出：変調"], ["開閉フィルター ワウ とても遅い 鋭い", "ワウの帯域がとても遅い開閉する", "変調", "演出"])]
public sealed class Fx4A412Effect : FxA3 { public override string Label => "開閉フィルター ワウ とても遅い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 3, 0.05, 0.3); }

[AudioEffect("開閉フィルター ワウ 遅い なだらか", ["演出：変調"], ["開閉フィルター ワウ 遅い なだらか", "ワウの帯域が遅い開閉する", "変調", "演出"])]
public sealed class Fx4A413Effect : FxA3 { public override string Label => "開閉フィルター ワウ 遅い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 0.8, 0.15, 0.8); }

[AudioEffect("開閉フィルター ワウ 遅い 鋭い", ["演出：変調"], ["開閉フィルター ワウ 遅い 鋭い", "ワウの帯域が遅い開閉する", "変調", "演出"])]
public sealed class Fx4A414Effect : FxA3 { public override string Label => "開閉フィルター ワウ 遅い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 3, 0.15, 0.8); }

[AudioEffect("開閉フィルター ワウ 普通 なだらか", ["演出：変調"], ["開閉フィルター ワウ 普通 なだらか", "ワウの帯域が普通開閉する", "変調", "演出"])]
public sealed class Fx4A415Effect : FxA3 { public override string Label => "開閉フィルター ワウ 普通 なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 0.8, 0.4, 2); }

[AudioEffect("開閉フィルター ワウ 普通 鋭い", ["演出：変調"], ["開閉フィルター ワウ 普通 鋭い", "ワウの帯域が普通開閉する", "変調", "演出"])]
public sealed class Fx4A416Effect : FxA3 { public override string Label => "開閉フィルター ワウ 普通 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 3, 0.4, 2); }

[AudioEffect("開閉フィルター ワウ 速い なだらか", ["演出：変調"], ["開閉フィルター ワウ 速い なだらか", "ワウの帯域が速い開閉する", "変調", "演出"])]
public sealed class Fx4A417Effect : FxA3 { public override string Label => "開閉フィルター ワウ 速い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 0.8, 1, 5); }

[AudioEffect("開閉フィルター ワウ 速い 鋭い", ["演出：変調"], ["開閉フィルター ワウ 速い 鋭い", "ワウの帯域が速い開閉する", "変調", "演出"])]
public sealed class Fx4A418Effect : FxA3 { public override string Label => "開閉フィルター ワウ 速い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 3, 1, 5); }

[AudioEffect("開閉フィルター ワウ 超高速 なだらか", ["演出：変調"], ["開閉フィルター ワウ 超高速 なだらか", "ワウの帯域が超高速開閉する", "変調", "演出"])]
public sealed class Fx4A419Effect : FxA3 { public override string Label => "開閉フィルター ワウ 超高速 なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 0.8, 3, 10); }

[AudioEffect("開閉フィルター ワウ 超高速 鋭い", ["演出：変調"], ["開閉フィルター ワウ 超高速 鋭い", "ワウの帯域が超高速開閉する", "変調", "演出"])]
public sealed class Fx4A420Effect : FxA3 { public override string Label => "開閉フィルター ワウ 超高速 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 250, 4000, 1, 3, 3, 10); }

[AudioEffect("開閉フィルター シャリ とても遅い なだらか", ["演出：変調"], ["開閉フィルター シャリ とても遅い なだらか", "シャリの帯域がとても遅い開閉する", "変調", "演出"])]
public sealed class Fx4A421Effect : FxA3 { public override string Label => "開閉フィルター シャリ とても遅い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 0.8, 0.05, 0.3); }

[AudioEffect("開閉フィルター シャリ とても遅い 鋭い", ["演出：変調"], ["開閉フィルター シャリ とても遅い 鋭い", "シャリの帯域がとても遅い開閉する", "変調", "演出"])]
public sealed class Fx4A422Effect : FxA3 { public override string Label => "開閉フィルター シャリ とても遅い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 3, 0.05, 0.3); }

[AudioEffect("開閉フィルター シャリ 遅い なだらか", ["演出：変調"], ["開閉フィルター シャリ 遅い なだらか", "シャリの帯域が遅い開閉する", "変調", "演出"])]
public sealed class Fx4A423Effect : FxA3 { public override string Label => "開閉フィルター シャリ 遅い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 0.8, 0.15, 0.8); }

[AudioEffect("開閉フィルター シャリ 遅い 鋭い", ["演出：変調"], ["開閉フィルター シャリ 遅い 鋭い", "シャリの帯域が遅い開閉する", "変調", "演出"])]
public sealed class Fx4A424Effect : FxA3 { public override string Label => "開閉フィルター シャリ 遅い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 3, 0.15, 0.8); }

[AudioEffect("開閉フィルター シャリ 普通 なだらか", ["演出：変調"], ["開閉フィルター シャリ 普通 なだらか", "シャリの帯域が普通開閉する", "変調", "演出"])]
public sealed class Fx4A425Effect : FxA3 { public override string Label => "開閉フィルター シャリ 普通 なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 0.8, 0.4, 2); }

[AudioEffect("開閉フィルター シャリ 普通 鋭い", ["演出：変調"], ["開閉フィルター シャリ 普通 鋭い", "シャリの帯域が普通開閉する", "変調", "演出"])]
public sealed class Fx4A426Effect : FxA3 { public override string Label => "開閉フィルター シャリ 普通 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 3, 0.4, 2); }

[AudioEffect("開閉フィルター シャリ 速い なだらか", ["演出：変調"], ["開閉フィルター シャリ 速い なだらか", "シャリの帯域が速い開閉する", "変調", "演出"])]
public sealed class Fx4A427Effect : FxA3 { public override string Label => "開閉フィルター シャリ 速い なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 0.8, 1, 5); }

[AudioEffect("開閉フィルター シャリ 速い 鋭い", ["演出：変調"], ["開閉フィルター シャリ 速い 鋭い", "シャリの帯域が速い開閉する", "変調", "演出"])]
public sealed class Fx4A428Effect : FxA3 { public override string Label => "開閉フィルター シャリ 速い 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 3, 1, 5); }

[AudioEffect("開閉フィルター シャリ 超高速 なだらか", ["演出：変調"], ["開閉フィルター シャリ 超高速 なだらか", "シャリの帯域が超高速開閉する", "変調", "演出"])]
public sealed class Fx4A429Effect : FxA3 { public override string Label => "開閉フィルター シャリ 超高速 なだらか"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 0.8, 3, 10); }

[AudioEffect("開閉フィルター シャリ 超高速 鋭い", ["演出：変調"], ["開閉フィルター シャリ 超高速 鋭い", "シャリの帯域が超高速開閉する", "変調", "演出"])]
public sealed class Fx4A430Effect : FxA3 { public override string Label => "開閉フィルター シャリ 超高速 鋭い"; internal override AudioKernel CreateKernel() => new FilterLfoK(this, 80, 3000, 2, 3, 3, 10); }

[AudioEffect("声の高さ -12", ["演出：声のキャラ"], ["声の高さ -12", "話す速さのまま声を-12半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A431Effect : FxA2 { public override string Label => "声の高さ -12"; internal override AudioKernel CreateKernel() => new VoiceK(this, -12, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -11", ["演出：声のキャラ"], ["声の高さ -11", "話す速さのまま声を-11半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A432Effect : FxA2 { public override string Label => "声の高さ -11"; internal override AudioKernel CreateKernel() => new VoiceK(this, -11, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -10", ["演出：声のキャラ"], ["声の高さ -10", "話す速さのまま声を-10半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A433Effect : FxA2 { public override string Label => "声の高さ -10"; internal override AudioKernel CreateKernel() => new VoiceK(this, -10, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -9", ["演出：声のキャラ"], ["声の高さ -9", "話す速さのまま声を-9半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A434Effect : FxA2 { public override string Label => "声の高さ -9"; internal override AudioKernel CreateKernel() => new VoiceK(this, -9, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -8", ["演出：声のキャラ"], ["声の高さ -8", "話す速さのまま声を-8半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A435Effect : FxA2 { public override string Label => "声の高さ -8"; internal override AudioKernel CreateKernel() => new VoiceK(this, -8, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -7", ["演出：声のキャラ"], ["声の高さ -7", "話す速さのまま声を-7半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A436Effect : FxA2 { public override string Label => "声の高さ -7"; internal override AudioKernel CreateKernel() => new VoiceK(this, -7, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -6", ["演出：声のキャラ"], ["声の高さ -6", "話す速さのまま声を-6半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A437Effect : FxA2 { public override string Label => "声の高さ -6"; internal override AudioKernel CreateKernel() => new VoiceK(this, -6, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -5", ["演出：声のキャラ"], ["声の高さ -5", "話す速さのまま声を-5半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A438Effect : FxA2 { public override string Label => "声の高さ -5"; internal override AudioKernel CreateKernel() => new VoiceK(this, -5, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -4", ["演出：声のキャラ"], ["声の高さ -4", "話す速さのまま声を-4半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A439Effect : FxA2 { public override string Label => "声の高さ -4"; internal override AudioKernel CreateKernel() => new VoiceK(this, -4, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -3", ["演出：声のキャラ"], ["声の高さ -3", "話す速さのまま声を-3半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A440Effect : FxA2 { public override string Label => "声の高さ -3"; internal override AudioKernel CreateKernel() => new VoiceK(this, -3, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -2", ["演出：声のキャラ"], ["声の高さ -2", "話す速さのまま声を-2半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A441Effect : FxA2 { public override string Label => "声の高さ -2"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 60, 18000, 0, 0); }

[AudioEffect("声の高さ -1", ["演出：声のキャラ"], ["声の高さ -1", "話す速さのまま声を-1半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A442Effect : FxA2 { public override string Label => "声の高さ -1"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +1", ["演出：声のキャラ"], ["声の高さ +1", "話す速さのまま声を+1半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A443Effect : FxA2 { public override string Label => "声の高さ +1"; internal override AudioKernel CreateKernel() => new VoiceK(this, 1, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +2", ["演出：声のキャラ"], ["声の高さ +2", "話す速さのまま声を+2半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A444Effect : FxA2 { public override string Label => "声の高さ +2"; internal override AudioKernel CreateKernel() => new VoiceK(this, 2, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +3", ["演出：声のキャラ"], ["声の高さ +3", "話す速さのまま声を+3半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A445Effect : FxA2 { public override string Label => "声の高さ +3"; internal override AudioKernel CreateKernel() => new VoiceK(this, 3, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +4", ["演出：声のキャラ"], ["声の高さ +4", "話す速さのまま声を+4半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A446Effect : FxA2 { public override string Label => "声の高さ +4"; internal override AudioKernel CreateKernel() => new VoiceK(this, 4, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +5", ["演出：声のキャラ"], ["声の高さ +5", "話す速さのまま声を+5半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A447Effect : FxA2 { public override string Label => "声の高さ +5"; internal override AudioKernel CreateKernel() => new VoiceK(this, 5, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +6", ["演出：声のキャラ"], ["声の高さ +6", "話す速さのまま声を+6半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A448Effect : FxA2 { public override string Label => "声の高さ +6"; internal override AudioKernel CreateKernel() => new VoiceK(this, 6, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +7", ["演出：声のキャラ"], ["声の高さ +7", "話す速さのまま声を+7半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A449Effect : FxA2 { public override string Label => "声の高さ +7"; internal override AudioKernel CreateKernel() => new VoiceK(this, 7, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +8", ["演出：声のキャラ"], ["声の高さ +8", "話す速さのまま声を+8半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A450Effect : FxA2 { public override string Label => "声の高さ +8"; internal override AudioKernel CreateKernel() => new VoiceK(this, 8, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +9", ["演出：声のキャラ"], ["声の高さ +9", "話す速さのまま声を+9半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A451Effect : FxA2 { public override string Label => "声の高さ +9"; internal override AudioKernel CreateKernel() => new VoiceK(this, 9, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +10", ["演出：声のキャラ"], ["声の高さ +10", "話す速さのまま声を+10半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A452Effect : FxA2 { public override string Label => "声の高さ +10"; internal override AudioKernel CreateKernel() => new VoiceK(this, 10, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +11", ["演出：声のキャラ"], ["声の高さ +11", "話す速さのまま声を+11半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A453Effect : FxA2 { public override string Label => "声の高さ +11"; internal override AudioKernel CreateKernel() => new VoiceK(this, 11, 60, 18000, 0, 0); }

[AudioEffect("声の高さ +12", ["演出：声のキャラ"], ["声の高さ +12", "話す速さのまま声を+12半音変える", "声のキャラ", "演出"])]
public sealed class Fx4A454Effect : FxA2 { public override string Label => "声の高さ +12"; internal override AudioKernel CreateKernel() => new VoiceK(this, 12, 60, 18000, 0, 0); }

[AudioEffect("声質 かすれ声", ["演出：声のキャラ"], ["声質 かすれ声", "少しかすれた声", "声のキャラ", "演出"])]
public sealed class Fx4A455Effect : FxA2 { public override string Label => "声質 かすれ声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 220, 7000, .28f, 0); }

[AudioEffect("声質 がらがら声", ["演出：声のキャラ"], ["声質 がらがら声", "荒れた声", "声のキャラ", "演出"])]
public sealed class Fx4A456Effect : FxA2 { public override string Label => "声質 がらがら声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 180, 6000, .5f, 0); }

[AudioEffect("声質 細い声", ["演出：声のキャラ"], ["声質 細い声", "線の細い声", "声のキャラ", "演出"])]
public sealed class Fx4A457Effect : FxA2 { public override string Label => "声質 細い声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 1, 420, 9000, 0, 0); }

[AudioEffect("声質 太い声", ["演出：声のキャラ"], ["声質 太い声", "太く低い声", "声のキャラ", "演出"])]
public sealed class Fx4A458Effect : FxA2 { public override string Label => "声質 太い声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -2, 70, 7000, .12f, 0); }

[AudioEffect("声質 明るい声", ["演出：声のキャラ"], ["声質 明るい声", "通りの良い声", "声のキャラ", "演出"])]
public sealed class Fx4A459Effect : FxA2 { public override string Label => "声質 明るい声"; internal override AudioKernel CreateKernel() => new VoiceK(this, 1, 160, 16000, 0, 0, .6, 3000, 4); }

[AudioEffect("声質 暗い声", ["演出：声のキャラ"], ["声質 暗い声", "こもった沈んだ声", "声のキャラ", "演出"])]
public sealed class Fx4A460Effect : FxA2 { public override string Label => "声質 暗い声"; internal override AudioKernel CreateKernel() => new VoiceK(this, -1, 90, 3600, 0, 0); }

[AudioEffect("響き 六畳間", ["演出：場所の声"], ["響き 六畳間", "六畳間で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A461Effect : FxA2 { public override string Label => "響き 六畳間"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 120, 9000, 0, 0.18f, 0.25); }

[AudioEffect("響き 教室", ["演出：場所の声"], ["響き 教室", "教室で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A462Effect : FxA2 { public override string Label => "響き 教室"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 100, 9000, 0, 0.3f, 0.42); }

[AudioEffect("響き 廊下", ["演出：場所の声"], ["響き 廊下", "廊下で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A463Effect : FxA2 { public override string Label => "響き 廊下"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 150, 8000, 0, 0.38f, 0.5); }

[AudioEffect("響き 階段", ["演出：場所の声"], ["響き 階段", "階段で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A464Effect : FxA2 { public override string Label => "響き 階段"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 140, 7500, 0, 0.42f, 0.55); }

[AudioEffect("響き 体育館", ["演出：場所の声"], ["響き 体育館", "体育館で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A465Effect : FxA2 { public override string Label => "響き 体育館"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 90, 7000, 0, 0.6f, 0.85); }

[AudioEffect("響き 音楽室", ["演出：場所の声"], ["響き 音楽室", "音楽室で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A466Effect : FxA2 { public override string Label => "響き 音楽室"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 90, 11000, 0, 0.32f, 0.45); }

[AudioEffect("響き 図書館", ["演出：場所の声"], ["響き 図書館", "図書館で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A467Effect : FxA2 { public override string Label => "響き 図書館"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 150, 7000, 0, 0.12f, 0.4); }

[AudioEffect("響き 車の中", ["演出：場所の声"], ["響き 車の中", "車の中で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A468Effect : FxA2 { public override string Label => "響き 車の中"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 180, 6000, 0, 0.1f, 0.15); }

[AudioEffect("響き 電車の中", ["演出：場所の声"], ["響き 電車の中", "電車の中で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A469Effect : FxA2 { public override string Label => "響き 電車の中"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 200, 5500, 0, 0.2f, 0.35); }

[AudioEffect("響き エレベーター", ["演出：場所の声"], ["響き エレベーター", "エレベーターで話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A470Effect : FxA2 { public override string Label => "響き エレベーター"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 180, 7000, 0, 0.24f, 0.2); }

[AudioEffect("響き 地下室", ["演出：場所の声"], ["響き 地下室", "地下室で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A471Effect : FxA2 { public override string Label => "響き 地下室"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 80, 4500, 0, 0.46f, 0.5); }

[AudioEffect("響き 倉庫", ["演出：場所の声"], ["響き 倉庫", "倉庫で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A472Effect : FxA2 { public override string Label => "響き 倉庫"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 90, 6000, 0, 0.52f, 0.72); }

[AudioEffect("響き 駅のホーム", ["演出：場所の声"], ["響き 駅のホーム", "駅のホームで話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A473Effect : FxA2 { public override string Label => "響き 駅のホーム"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 140, 7000, 0, 0.55f, 0.8); }

[AudioEffect("響き トンネル内", ["演出：場所の声"], ["響き トンネル内", "トンネル内で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A474Effect : FxA2 { public override string Label => "響き トンネル内"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 100, 5000, 0, 0.68f, 0.78); }

[AudioEffect("響き 聖堂", ["演出：場所の声"], ["響き 聖堂", "聖堂で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A475Effect : FxA2 { public override string Label => "響き 聖堂"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 80, 8000, 0, 0.72f, 0.95); }

[AudioEffect("響き 洞くつの奥", ["演出：場所の声"], ["響き 洞くつの奥", "洞くつの奥で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A476Effect : FxA2 { public override string Label => "響き 洞くつの奥"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 70, 4500, 0, 0.78f, 0.92); }

[AudioEffect("響き 神社の境内", ["演出：場所の声"], ["響き 神社の境内", "神社の境内で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A477Effect : FxA2 { public override string Label => "響き 神社の境内"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 200, 9000, 0, 0.16f, 0.6); }

[AudioEffect("響き 山の中", ["演出：場所の声"], ["響き 山の中", "山の中で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A478Effect : FxA2 { public override string Label => "響き 山の中"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 180, 9000, 0, 0.2f, 0.7); }

[AudioEffect("響き 大ホール", ["演出：場所の声"], ["響き 大ホール", "大ホールで話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A479Effect : FxA2 { public override string Label => "響き 大ホール"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 80, 10000, 0, 0.65f, 0.9); }

[AudioEffect("響き 宇宙船", ["演出：場所の声"], ["響き 宇宙船", "宇宙船で話しているような響き", "場所の声", "演出"])]
public sealed class Fx4A480Effect : FxA2 { public override string Label => "響き 宇宙船"; internal override AudioKernel CreateKernel() => new VoiceK(this, 0, 160, 6500, 0, 0.45f, 0.6); }

[AudioEffect("機器 古いスピーカー", ["演出：場所の声"], ["機器 古いスピーカー", "古いスピーカーを通したような音", "場所の声", "演出"])]
public sealed class Fx4A481Effect : FxA2 { public override string Label => "機器 古いスピーカー"; internal override AudioKernel CreateKernel() => new BandK(this, 400, 3200, 2); }

[AudioEffect("機器 安いイヤホン", ["演出：場所の声"], ["機器 安いイヤホン", "安いイヤホンを通したような音", "場所の声", "演出"])]
public sealed class Fx4A482Effect : FxA2 { public override string Label => "機器 安いイヤホン"; internal override AudioKernel CreateKernel() => new BandK(this, 300, 6000, 1); }

[AudioEffect("機器 館内放送", ["演出：場所の声"], ["機器 館内放送", "館内放送を通したような音", "場所の声", "演出"])]
public sealed class Fx4A483Effect : FxA2 { public override string Label => "機器 館内放送"; internal override AudioKernel CreateKernel() => new BandK(this, 500, 3600, 3); }

[AudioEffect("機器 駅の放送", ["演出：場所の声"], ["機器 駅の放送", "駅の放送を通したような音", "場所の声", "演出"])]
public sealed class Fx4A484Effect : FxA2 { public override string Label => "機器 駅の放送"; internal override AudioKernel CreateKernel() => new BandK(this, 450, 3400, 3); }

[AudioEffect("機器 インターホン", ["演出：場所の声"], ["機器 インターホン", "インターホンを通したような音", "場所の声", "演出"])]
public sealed class Fx4A485Effect : FxA2 { public override string Label => "機器 インターホン"; internal override AudioKernel CreateKernel() => new BandK(this, 650, 2800, 4); }

[AudioEffect("機器 公衆電話", ["演出：場所の声"], ["機器 公衆電話", "公衆電話を通したような音", "場所の声", "演出"])]
public sealed class Fx4A486Effect : FxA2 { public override string Label => "機器 公衆電話"; internal override AudioKernel CreateKernel() => new BandK(this, 500, 2600, 4); }

[AudioEffect("機器 ガラケー", ["演出：場所の声"], ["機器 ガラケー", "ガラケーを通したような音", "場所の声", "演出"])]
public sealed class Fx4A487Effect : FxA2 { public override string Label => "機器 ガラケー"; internal override AudioKernel CreateKernel() => new BandK(this, 380, 3000, 3); }

[AudioEffect("機器 トランシーバー", ["演出：場所の声"], ["機器 トランシーバー", "トランシーバーを通したような音", "場所の声", "演出"])]
public sealed class Fx4A488Effect : FxA2 { public override string Label => "機器 トランシーバー"; internal override AudioKernel CreateKernel() => new BandK(this, 700, 2400, 5); }

[AudioEffect("機器 ドライブスルー", ["演出：場所の声"], ["機器 ドライブスルー", "ドライブスルーを通したような音", "場所の声", "演出"])]
public sealed class Fx4A489Effect : FxA2 { public override string Label => "機器 ドライブスルー"; internal override AudioKernel CreateKernel() => new BandK(this, 600, 2600, 5); }

[AudioEffect("機器 蓄音機", ["演出：場所の声"], ["機器 蓄音機", "蓄音機を通したような音", "場所の声", "演出"])]
public sealed class Fx4A490Effect : FxA2 { public override string Label => "機器 蓄音機"; internal override AudioKernel CreateKernel() => new BandK(this, 600, 2200, 4); }

[AudioEffect("機器 カセット", ["演出：場所の声"], ["機器 カセット", "カセットを通したような音", "場所の声", "演出"])]
public sealed class Fx4A491Effect : FxA2 { public override string Label => "機器 カセット"; internal override AudioKernel CreateKernel() => new BandK(this, 90, 8500, 0); }

[AudioEffect("機器 ビデオテープ", ["演出：場所の声"], ["機器 ビデオテープ", "ビデオテープを通したような音", "場所の声", "演出"])]
public sealed class Fx4A492Effect : FxA2 { public override string Label => "機器 ビデオテープ"; internal override AudioKernel CreateKernel() => new BandK(this, 140, 6500, 0); }

[AudioEffect("機器 AMラジオ", ["演出：場所の声"], ["機器 AMラジオ", "AMラジオを通したような音", "場所の声", "演出"])]
public sealed class Fx4A493Effect : FxA2 { public override string Label => "機器 AMラジオ"; internal override AudioKernel CreateKernel() => new BandK(this, 200, 4500, 1); }

[AudioEffect("機器 FMラジオ", ["演出：場所の声"], ["機器 FMラジオ", "FMラジオを通したような音", "場所の声", "演出"])]
public sealed class Fx4A494Effect : FxA2 { public override string Label => "機器 FMラジオ"; internal override AudioKernel CreateKernel() => new BandK(this, 60, 14000, 0); }

[AudioEffect("機器 昔のテレビ", ["演出：場所の声"], ["機器 昔のテレビ", "昔のテレビを通したような音", "場所の声", "演出"])]
public sealed class Fx4A495Effect : FxA2 { public override string Label => "機器 昔のテレビ"; internal override AudioKernel CreateKernel() => new BandK(this, 220, 5000, 1); }

[AudioEffect("機器 ゲーム機", ["演出：場所の声"], ["機器 ゲーム機", "ゲーム機を通したような音", "場所の声", "演出"])]
public sealed class Fx4A496Effect : FxA2 { public override string Label => "機器 ゲーム機"; internal override AudioKernel CreateKernel() => new BandK(this, 180, 6500, 1); }

[AudioEffect("機器 校内放送", ["演出：場所の声"], ["機器 校内放送", "校内放送を通したような音", "場所の声", "演出"])]
public sealed class Fx4A497Effect : FxA2 { public override string Label => "機器 校内放送"; internal override AudioKernel CreateKernel() => new BandK(this, 420, 4200, 2); }

[AudioEffect("機器 拡声器 小", ["演出：場所の声"], ["機器 拡声器 小", "拡声器 小を通したような音", "場所の声", "演出"])]
public sealed class Fx4A498Effect : FxA2 { public override string Label => "機器 拡声器 小"; internal override AudioKernel CreateKernel() => new BandK(this, 550, 3800, 3); }

[AudioEffect("機器 ヘルメット越し", ["演出：場所の声"], ["機器 ヘルメット越し", "ヘルメット越しを通したような音", "場所の声", "演出"])]
public sealed class Fx4A499Effect : FxA2 { public override string Label => "機器 ヘルメット越し"; internal override AudioKernel CreateKernel() => new BandK(this, 300, 2500, 2); }

[AudioEffect("機器 宇宙服の無線", ["演出：場所の声"], ["機器 宇宙服の無線", "宇宙服の無線を通したような音", "場所の声", "演出"])]
public sealed class Fx4A500Effect : FxA2 { public override string Label => "機器 宇宙服の無線"; internal override AudioKernel CreateKernel() => new BandK(this, 800, 3200, 4); }
