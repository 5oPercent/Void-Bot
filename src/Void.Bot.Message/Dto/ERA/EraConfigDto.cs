namespace Void.Bot.Message.Dto.ERA;

/// <summary>客户端使用的当前 ERA 配置集合。</summary>
public class EraConfigDto
{
    public long ConfigVersion { get; set; }
    public List<EraConfigurationItemDto> Items { get; set; } = new List<EraConfigurationItemDto>();
}

/// <summary>单个强类型配置项，Value 使用与 ValueType 对应的不变格式字符串。</summary>
public class EraConfigurationItemDto
{
    public string Key { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
