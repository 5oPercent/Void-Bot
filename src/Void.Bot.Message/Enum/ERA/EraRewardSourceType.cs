namespace Void.Bot.Message.Enum.ERA;

/// <summary>奖励来源类型；声明枚举不表示对应业务入口已经接入。</summary>
public enum EraRewardSourceType
{
    Internal = 1,
    Task = 2,
    Monster = 3,
    Chest = 4,
    /// <summary>世界内事件完成，由当前房主按配置触发奖励。</summary>
    Event = 5
}
