namespace Void.Bot.Message.Enum.ERA;

/// <summary>奖励来源类型；声明枚举不表示对应业务入口已经接入。</summary>
public enum EraRewardSourceType
{
    /// <summary>后端内部业务来源。</summary>
    Internal = 1,
    /// <summary>任务奖励来源。</summary>
    Task = 2,
    /// <summary>怪物奖励来源。</summary>
    Monster = 3,
    /// <summary>箱子奖励来源。</summary>
    Chest = 4,
    /// <summary>世界内事件完成，由当前房主按配置触发奖励。</summary>
    Event = 5
}
