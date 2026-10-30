using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>可复用的武器改装槽位方案；模板通过方案生成每件实例的固定候选。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponSlotConfigurationDto
{
    /// <summary>槽位方案唯一标识，默认生成新 Guid。</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>槽位方案显示名称，必填且最多 128 字符。</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>方案说明，最多 2048 字符，可为空字符串。</summary>
    public string Note { get; set; } = string.Empty;
    /// <summary>方案中的槽位定义，至少一项；每槽独立配置类型、候选数量及候选池。</summary>
    public List<EraWeaponConfigurationSlotDto> Slots { get; set; } = new();
}
