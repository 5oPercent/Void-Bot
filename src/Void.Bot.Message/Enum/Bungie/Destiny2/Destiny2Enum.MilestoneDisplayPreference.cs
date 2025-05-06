using System.ComponentModel;

namespace Void.Bot.Message.Enum.Bungie.Destiny2;

/// <summary>
/// 命运2里程碑显示偏好枚举，定义如何展示里程碑信息
/// </summary>
public enum Destiny2MilestoneDisplayPreferenceEnum
{
    /// <summary>
    /// 显示里程碑定义，表示应显示DestinyMilestoneDefinition.displayProperties的信息
    /// </summary>
    [Description("显示里程碑定义")]
    MilestoneDefinition = 0,

    /// <summary>
    /// 当前任务步骤，表示应显示DestinyMilestone.availableQuests中当前活跃任务步骤的显示属性
    /// </summary>
    [Description("当前任务步骤")]
    CurrentQuestSteps = 1,

    /// <summary>
    /// 当前活动挑战，表示应显示DestinyMilestone.activities中当前活跃活动及其挑战的显示属性
    /// </summary>
    [Description("当前活动挑战")]
    CurrentActivityChallenges = 2
}