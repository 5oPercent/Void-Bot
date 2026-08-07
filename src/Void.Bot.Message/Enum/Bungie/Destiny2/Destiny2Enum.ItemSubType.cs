namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运 2 物品子类型。
/// 此枚举主要用于兼容历史数据，并不包含所有可能的物品分类；复杂分类应优先使用物品分类定义。
/// </summary>
public enum Destiny2ItemSubTypeEnum
{
    /// <summary>
    /// 无
    /// </summary>
    None = 0,

    /// <summary>
    /// 熔炉竞技场（已弃用，物品还可能同时属于其他分类）
    /// </summary>
    Crucible = 1,

    /// <summary>
    /// 先锋（已弃用，物品还可能同时属于其他分类）
    /// </summary>
    Vanguard = 2,

    /// <summary>
    /// 异域（已弃用，物品还可能同时属于其他分类）
    /// </summary>
    Exotic = 5,

    /// <summary>
    /// 自动步枪
    /// </summary>
    AutoRifle = 6,

    /// <summary>
    /// 霰弹枪
    /// </summary>
    Shotgun = 7,

    /// <summary>
    /// 机枪
    /// </summary>
    Machinegun = 8,

    /// <summary>
    /// 手炮
    /// </summary>
    HandCannon = 9,

    /// <summary>
    /// 火箭发射器
    /// </summary>
    RocketLauncher = 10,

    /// <summary>
    /// 融合步枪
    /// </summary>
    FusionRifle = 11,

    /// <summary>
    /// 狙击步枪
    /// </summary>
    SniperRifle = 12,

    /// <summary>
    /// 脉冲步枪
    /// </summary>
    PulseRifle = 13,

    /// <summary>
    /// 侦察步枪
    /// </summary>
    ScoutRifle = 14,

    /// <summary>
    /// CRM（已弃用）
    /// </summary>
    Crm = 16,

    /// <summary>
    /// 手枪
    /// </summary>
    Sidearm = 17,

    /// <summary>
    /// 剑
    /// </summary>
    Sword = 18,

    /// <summary>
    /// 面具
    /// </summary>
    Mask = 19,

    /// <summary>
    /// 着色器
    /// </summary>
    Shader = 20,

    /// <summary>
    /// 装饰
    /// </summary>
    Ornament = 21,

    /// <summary>
    /// 线性融合步枪
    /// </summary>
    FusionRifleLine = 22,

    /// <summary>
    /// 榴弹发射器
    /// </summary>
    GrenadeLauncher = 23,

    /// <summary>
    /// 冲锋枪
    /// </summary>
    SubmachineGun = 24,

    /// <summary>
    /// 追踪步枪
    /// </summary>
    TraceRifle = 25,

    /// <summary>
    /// 头盔
    /// </summary>
    HelmetArmor = 26,

    /// <summary>
    /// 臂部护甲
    /// </summary>
    GauntletsArmor = 27,

    /// <summary>
    /// 胸甲护甲
    /// </summary>
    ChestArmor = 28,

    /// <summary>
    /// 腿部护甲
    /// </summary>
    LegArmor = 29,

    /// <summary>
    /// 职业护甲
    /// </summary>
    ClassArmor = 30,

    /// <summary>
    /// 弓
    /// </summary>
    Bow = 31,

    /// <summary>
    /// 占位用可重复悬赏
    /// </summary>
    DummyRepeatableBounty = 32,

    /// <summary>
    /// 偃月
    /// </summary>
    Glaive = 33
}
