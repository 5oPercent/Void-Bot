using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>
/// BUFF 的文字与业务配置；实例状态及实际表现由游戏端管理。
/// </summary>
public sealed class BuffDefinitionDto
{
    /// <summary>
    /// 稳定且唯一的定义 ID，不随数值或客户端表现调整改变。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 基本显示名称；图标、颜色和其他样式由客户端映射。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 基本文字说明，不携带客户端表现资源。
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 当前定义的类型，决定如何解释效果或 BUFF 生命周期。
    /// </summary>
    public BuffKind Kind { get; set; }

    /// <summary>
    /// BUFF 持续时间，单位为秒；持续或周期 BUFF 中 0 表示永久。
    /// </summary>
    public double DurationSeconds { get; set; }

    /// <summary>
    /// 周期 BUFF 两次效果执行之间的间隔，单位为秒。
    /// </summary>
    public double IntervalSeconds { get; set; } = 1;

    /// <summary>
    /// BUFF 叠加分组；空字符串时使用 BUFF ID 作为分组。
    /// </summary>
    public string StackGroup { get; set; } = string.Empty;

    /// <summary>
    /// 叠加协议值：0 独立、1 刷新、2 叠层刷新、3 同组取最强，与游戏端现有规则一致。
    /// </summary>
    public int StackRule { get; set; }

    /// <summary>
    /// 来源隔离协议值：0 全部来源、1 同一角色、2 同一来源实例。
    /// </summary>
    public int SourceScope { get; set; }

    /// <summary>
    /// BUFF 最大层数，叠层时不得超过该上限。
    /// </summary>
    public int MaxStacks { get; set; } = 1;

    /// <summary>
    /// 同组取最强规则的比较优先级，较大的值优先生效。
    /// </summary>
    public int StrengthPriority { get; set; }

    /// <summary>
    /// 目标死亡时是否移除此 BUFF。
    /// </summary>
    public bool RemoveOnDeath { get; set; } = true;

    /// <summary>
    /// BUFF 包含的多个效果；具体执行顺序按列表顺序处理。
    /// </summary>
    public List<BuffEffectDto> Effects { get; set; } = new List<BuffEffectDto>();
}
