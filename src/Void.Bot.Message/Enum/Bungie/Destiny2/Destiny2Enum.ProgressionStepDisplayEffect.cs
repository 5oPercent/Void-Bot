using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 进度步骤显示效果枚举，定义进度条步骤的显示效果类型
/// </summary>
public enum Destiny2ProgressionStepDisplayEffectEnum
{
    /// <summary>
    /// 无效果，不显示任何特殊效果
    /// </summary>
    [Description("无效果")]
    None = 0,

    /// <summary>
    /// 角色效果，应用于角色级别的进度显示
    /// </summary>
    [Description("角色效果")]
    Character = 1,

    /// <summary>
    /// 物品效果，应用于物品级别的进度显示
    /// </summary>
    [Description("物品效果")]
    Item = 2
}