namespace Void.Bot.Message.Enum.ERA;

/// <summary>
/// 载体目标关系过滤，不负责范围形状或命中判定。
/// </summary>
public enum SkillTargetRelation
{
    /// <summary>
    /// 仅施法者自身。
    /// </summary>
    Self = 0,

    /// <summary>
    /// 友方目标，包含施法者自身。
    /// </summary>
    Friendly = 1,

    /// <summary>
    /// 敌方目标。
    /// </summary>
    Hostile = 2,

    /// <summary>
    /// 全部合法目标，不限制阵营关系。
    /// </summary>
    All = 3,
}
