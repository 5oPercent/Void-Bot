using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.ERA;

/// <summary>武器改装槽位类型；将方案槽位与允许使用的改装定义关联。</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EraWeaponSlotTypeDto
{
    /// <summary>槽位类型唯一标识，默认生成新 Guid。</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>槽位类型显示名称，必填且最多 128 字符。</summary>
    public string Name { get; set; } = string.Empty;
}
