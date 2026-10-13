using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// 技能在指定槽位的释放方案，定义使用次数、冷却恢复、载体和 BUFF 引用。
/// </summary>
public sealed class SkillVariant
{
    /// <summary>
    /// 该方案支持的槽位；主要和次要不固定为区域或自身效果。
    /// </summary>
    public SkillSlotKind Slot { get; set; }

    /// <summary>
    /// 最大储存使用次数；每次释放消耗一次，缺失次数按时间逐次恢复。
    /// </summary>
    public int MaximumCharges { get; set; } = 1;

    /// <summary>
    /// 恢复一次使用次数需要的完整冷却时长，单位为秒。
    /// </summary>
    public double CooldownSeconds { get; set; }

    /// <summary>
    /// 释放成功后或载体结束后开始恢复次数的时机；载体销毁规则独立配置。
    /// </summary>
    public SkillRecoveryStart RecoveryStart { get; set; }

    /// <summary>
    /// 游戏端已注册的载体实现 ID；不包含任意脚本或场景路径。
    /// </summary>
    public string CarrierId { get; set; } = string.Empty;

    /// <summary>
    /// 目标关系过滤；友方包含施法者自身，全体不限制阵营关系。
    /// </summary>
    public SkillTargetRelation TargetRelation { get; set; } = SkillTargetRelation.Friendly;

    /// <summary>
    /// 具体载体显式开放的数值参数；形状、碰撞和视线行为由游戏端实现。
    /// </summary>
    public Dictionary<string, double> Parameters { get; set; } = new Dictionary<string, double>();

    /// <summary>
    /// 载体触发时施加的 BUFF ID 列表，允许一次施加多个 BUFF。
    /// </summary>
    public List<string> BuffIds { get; set; } = new List<string>();
}
