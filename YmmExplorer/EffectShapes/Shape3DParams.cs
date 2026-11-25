using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.ItemEditor.CustomVisibilityAttributes;
using WpfColor = System.Windows.Media.Color;

namespace YMM4EffectShapes;

/// <summary>3Dの厚みの決め方</summary>
public enum Depth3DMode
{
    [Display(Name = "細い物は細く（形に合わせる）")] Fit,
    [Display(Name = "全部同じ厚み")] Same,
    [Display(Name = "厚みなし（板を奥に並べる）")] Flat,
}

/// <summary>
/// 全図形共通の「3D」設定。3Dで描くをオンにすると、図形の描画命令（塗り・線・文字）を
/// 1つずつ厚みのある立体にして、奥行きのある3D空間で描く
/// </summary>
public abstract partial class EffectShapeParameterBase
{
    [Display(GroupName = "3D", Name = "3Dで描く", Description = "オン=図形を立体にして3D空間で描きます。回転すると側面や奥が見えます")]
    [ToggleSlider]
    public bool Is3D { get => is3D; set => Set(ref is3D, value); }
    bool is3D;

    [Display(GroupName = "3D", Name = "厚みの決め方", Description = "形に合わせる=細い線や小さな粒は細いまま、大きな面は「厚み」まで厚くする")]
    [EnumComboBox]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Depth3DMode DepthMode { get => depthMode; set => Set(ref depthMode, value); }
    Depth3DMode depthMode = Depth3DMode.Fit;

    [Display(GroupName = "3D", Name = "厚み", Description = "立体の奥行き(前の面から後ろの面まで)")]
    [AnimationSlider("F0", "px", 0, 400)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation Depth3D { get; } = new Animation(40, 0, 10000);

    [Display(GroupName = "3D", Name = "重なりの間隔", Description = "後から描いた部品ほど手前に出す間隔。大きいほど部品が前後に分かれて見える")]
    [AnimationSlider("F1", "px", 0, 50)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation LayerGap3D { get; } = new Animation(2, 0, 2000);

    [Display(GroupName = "3D", Name = "重なりの奥行き（最大）", Description = "部品がとても多い図形で、前後に広がりすぎないための上限")]
    [AnimationSlider("F0", "px", 0, 2000)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation LayerMax3D { get; } = new Animation(400, 0, 50000);

    [Display(GroupName = "3D", Name = "Z範囲（奥への散らばり）", Description = "部品を1つずつ奥行き方向にばらまく範囲。雨・雪・花びらなどが奥の方にも降るようになる")]
    [AnimationSlider("F0", "px", 0, 5000)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation ZRange3D { get; } = new Animation(0, 0, 100000);

    [Display(GroupName = "3D", Name = "Z方向の流れ", Description = "散らばった部品が奥から手前へ流れる速さ(マイナスで手前から奥へ)。Z範囲が0の時は動きません")]
    [AnimationSlider("F0", "px/秒", -2000, 2000)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation ZFlow3D { get; } = new Animation(0, -100000, 100000);

    [Display(GroupName = "3D", Name = "奥も画面いっぱいに", Description = "オン=奥に散らばった部品も画面の端まで広げる(遠い物は小さく見える)")]
    [ToggleSlider]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public bool ZFill3D { get => zFill3D; set => Set(ref zFill3D, value); }
    bool zFill3D = true;

    [Display(GroupName = "3D", Name = "専用の3Dモデル", Description = "専用の3Dモデルがある図形は、その立体で描く(オフ: 絵に厚みを付ける方式)。専用モデルの無い図形では何も変わらない")]
    [ToggleSlider]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public bool UseModel3D { get => useModel3D; set => Set(ref useModel3D, value); }
    bool useModel3D = true;

    [Display(GroupName = "3D", Name = "部品のひらひら回転", Description = "部品を1つずつ、その場でくるくる回す速さ(花びら・紙吹雪が舞うように)。0=回さない")]
    [AnimationSlider("F0", "°/秒", 0, 720)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation PartSpin3D { get; } = new Animation(0, -36000, 36000);

    [Display(GroupName = "3D", Name = "部品の前後のゆれ", Description = "部品を1つずつ、奥と手前にゆらす幅")]
    [AnimationSlider("F0", "px", 0, 500)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation PartWave3D { get; } = new Animation(0, 0, 100000);

    [Display(GroupName = "3D", Name = "前後のゆれの速さ", Description = "1秒あたりのゆれの回数")]
    [AnimationSlider("F2", "回/秒", 0, 3)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation PartWaveSpeed3D { get; } = new Animation(0.5, 0, 100);

    [Display(GroupName = "3D", Name = "遠くのかすみ", Description = "奥にある物ほど薄くする(奥行きが分かりやすくなる)")]
    [AnimationSlider("F0", "%", 0, 100)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation Fog3D { get; } = new Animation(0, 0, 100);

    [Display(GroupName = "3D", Name = "回転X（上下）", Description = "横の軸で回す。プラスで上側が奥へ倒れる")]
    [AnimationSlider("F1", "°", -180, 180)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation RotX3D { get; } = new Animation(0, -36000, 36000);

    [Display(GroupName = "3D", Name = "回転Y（左右）", Description = "縦の軸で回す。プラスで右側が奥へ回る")]
    [AnimationSlider("F1", "°", -180, 180)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation RotY3D { get; } = new Animation(0, -36000, 36000);

    [Display(GroupName = "3D", Name = "回転Z（画面の中で）", Description = "画面に垂直な軸で回す")]
    [AnimationSlider("F1", "°", -180, 180)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation RotZ3D { get; } = new Animation(0, -36000, 36000);

    [Display(GroupName = "3D", Name = "自動回転X", Description = "1秒あたりに回る角度(上下)")]
    [AnimationSlider("F1", "°/秒", -180, 180)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation SpinX3D { get; } = new Animation(0, -36000, 36000);

    [Display(GroupName = "3D", Name = "自動回転Y", Description = "1秒あたりに回る角度(左右)")]
    [AnimationSlider("F1", "°/秒", -180, 180)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation SpinY3D { get; } = new Animation(0, -36000, 36000);

    [Display(GroupName = "3D", Name = "自動回転Z", Description = "1秒あたりに回る角度(画面の中で)")]
    [AnimationSlider("F1", "°/秒", -180, 180)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation SpinZ3D { get; } = new Animation(0, -36000, 36000);

    [Display(GroupName = "3D", Name = "回転の中心（奥行き）", Description = "回転の中心を奥へずらす量。0=部品の重なりのまん中")]
    [AnimationSlider("F0", "px", -2000, 2000)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation PivotZ3D { get; } = new Animation(0, -100000, 100000);

    [Display(GroupName = "3D", Name = "視野角", Description = "大きいほど遠近感が強くなる(広角)")]
    [AnimationSlider("F0", "°", 5, 120)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation Fov3D { get; } = new Animation(45, 1, 170);

    [Display(GroupName = "3D", Name = "カメラの前後", Description = "プラスでカメラが近づく(大きく写る)")]
    [AnimationSlider("F0", "px", -3000, 3000)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation CamZ3D { get; } = new Animation(0, -100000, 100000);

    [Display(GroupName = "3D", Name = "光の向き（左右）", Description = "光が来る方向。マイナス=左から")]
    [AnimationSlider("F0", "°", -90, 90)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation LightYaw3D { get; } = new Animation(-35, -180, 180);

    [Display(GroupName = "3D", Name = "光の向き（上下）", Description = "光が来る方向。プラス=上から")]
    [AnimationSlider("F0", "°", -90, 90)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation LightPitch3D { get; } = new Animation(35, -180, 180);

    [Display(GroupName = "3D", Name = "光の強さ", Description = "0%=陰影なし(元の色のまま)")]
    [AnimationSlider("F0", "%", 0, 100)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation Light3D { get; } = new Animation(70, 0, 200);

    [Display(GroupName = "3D", Name = "側面の明るさ", Description = "側面(厚みの部分)の明るさ")]
    [AnimationSlider("F0", "%", 0, 150)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation SideBright3D { get; } = new Animation(75, 0, 300);

    [Display(GroupName = "3D", Name = "つや", Description = "光が当たった所の白い照り返し")]
    [AnimationSlider("F0", "%", 0, 100)]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public Animation Gloss3D { get; } = new Animation(20, 0, 300);

    [Display(GroupName = "3D", Name = "側面の色を指定", Description = "オン=側面を下の色にする。オフ=前の面と同じ色")]
    [ToggleSlider]
    [ShowPropertyEditorWhen(nameof(Is3D), true)]
    public bool SideColorOn3D { get => sideColorOn3D; set => Set(ref sideColorOn3D, value); }
    bool sideColorOn3D;

    [Display(GroupName = "3D", Name = "側面の色")]
    [ColorPicker]
    [ShowPropertyEditorWhen(nameof(SideColorOn3D), true)]
    public WpfColor SideColor3D { get => sideColor3D; set => Set(ref sideColor3D, value); }
    WpfColor sideColor3D = WpfColor.FromRgb(0x40, 0x40, 0x48);

    /// <summary>3Dの設定のアニメーション(全図形共通)</summary>
    protected IEnumerable<IAnimatable> Animatables3D() =>
    [
        Depth3D, LayerGap3D, LayerMax3D, ZRange3D, ZFlow3D, PartSpin3D, PartWave3D, PartWaveSpeed3D, Fog3D, RotX3D, RotY3D, RotZ3D, SpinX3D, SpinY3D, SpinZ3D,
        PivotZ3D, Fov3D, CamZ3D, LightYaw3D, LightPitch3D, Light3D, SideBright3D, Gloss3D,
    ];

    /// <summary>
    /// 同じ種類の図形から設定を写す(2D⇔3Dの切り替え用)。copy3D: null=3Dの設定は写さない、false=3Dをオフにする
    /// </summary>
    public void CopySettingsFrom(EffectShapeParameterBase from, bool? copy3D)
    {
        if (from.GetType() != GetType()) return;
        var threeD = new HashSet<string>(typeof(EffectShapeParameterBase).GetProperties()
            .Where(p => p.DeclaringType == typeof(EffectShapeParameterBase) && (p.Name.EndsWith("3D", StringComparison.Ordinal) || p.Name == nameof(DepthMode)))
            .Select(p => p.Name));
        foreach (var p in GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (threeD.Contains(p.Name) || p.GetIndexParameters().Length > 0) continue;
            try
            {
                if (typeof(Animation).IsAssignableFrom(p.PropertyType))
                {
                    if (p.GetValue(this) is Animation dst && p.GetValue(from) is Animation src) dst.CopyFrom(src);
                }
                else if (p.CanWrite && p.SetMethod?.IsPublic == true && p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), true).Length > 0)
                {
                    p.SetValue(this, p.GetValue(from));
                }
            }
            catch { }
        }
        if (copy3D == false) Is3D = false;
    }

    /// <summary>「演出3D：」の図形として作る時の初期値</summary>
    public void Apply3DDefaults(float zRange, float rotX, float rotY, float flow)
    {
        Is3D = true;
        ZRange3D.Values[0].Value = zRange;
        RotX3D.Values[0].Value = rotX;
        RotY3D.Values[0].Value = rotY;
        ZFlow3D.Values[0].Value = flow;
        if (zRange > 0) { LayerGap3D.Values[0].Value = 0.5; Depth3D.Values[0].Value = 12; Fog3D.Values[0].Value = 35; }
    }

    internal Shape3DSettings Get3D(in DrawContext c)
    {
        float t = c.Time;
        return new Shape3DSettings
        {
            Mode = DepthMode,
            Depth = c.F(Depth3D),
            LayerGap = c.F(LayerGap3D),
            LayerMax = c.F(LayerMax3D),
            ZRange = c.F(ZRange3D),
            ZFlow = c.F(ZFlow3D),
            ZFill = ZFill3D,
            PartSpin = c.F(PartSpin3D),
            PartWave = c.F(PartWave3D),
            PartWaveSpeed = c.F(PartWaveSpeed3D),
            Fog = c.F(Fog3D) / 100f,
            RotX = c.F(RotX3D) + c.F(SpinX3D) * t,
            RotY = c.F(RotY3D) + c.F(SpinY3D) * t,
            RotZ = c.F(RotZ3D) + c.F(SpinZ3D) * t,
            SpinX = c.F(SpinX3D) * t,
            SpinY = c.F(SpinY3D) * t,
            SpinZ = c.F(SpinZ3D) * t,
            PivotZ = c.F(PivotZ3D),
            Fov = c.F(Fov3D),
            CamZ = c.F(CamZ3D),
            LightYaw = c.F(LightYaw3D),
            LightPitch = c.F(LightPitch3D),
            Light = c.F(Light3D) / 100f,
            SideBright = c.F(SideBright3D) / 100f,
            Gloss = c.F(Gloss3D) / 100f,
            SideColor = SideColorOn3D ? SideColor3D : null,
            Seed = Seed,
            Time = t,
        };
    }
}

/// <summary>1フレーム分の3D設定</summary>
internal struct Shape3DSettings
{
    public Depth3DMode Mode;
    public float Depth, LayerGap, LayerMax, ZRange, ZFlow;
    public bool ZFill;
    public float PartSpin, PartWave, PartWaveSpeed, Fog;
    public float RotX, RotY, RotZ, PivotZ, Fov, CamZ;
    public float SpinX, SpinY, SpinZ;   // 自動回転の分だけ(3Dプレビューの中では、これだけを使う)
    public float LightYaw, LightPitch, Light, SideBright, Gloss;
    public WpfColor? SideColor;
    public int Seed;
    public float Time;
}
