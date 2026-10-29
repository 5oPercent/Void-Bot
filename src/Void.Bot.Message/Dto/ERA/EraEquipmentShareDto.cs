namespace Void.Bot.Message.Dto.ERA;

/// <summary>装备生成时固定的分享资格；名称仅用于展示，期限不随拾取、丢出刷新。</summary>
public sealed class EraEquipmentShareDto
{
    public DateTimeOffset ShareUntil { get; set; }
    public List<EraEquipmentShareMemberDto> Members { get; set; } = new();
}

public sealed class EraEquipmentShareMemberDto
{
    public Guid PlayerId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
}
