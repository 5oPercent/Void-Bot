namespace Void.Bot.Message.Dto.ERA;

/// <summary>仅供当前权威主机核验的已装备清单，不包含队友背包、仓库或账号凭证。</summary>
public sealed class EraWorldEquipmentDto
{
    public Guid WorldInstanceId { get; set; }
    public long AuthorityEpoch { get; set; }
    public long ManifestVersion { get; set; }
    public List<EraWorldMemberEquipmentDto> Members { get; set; } = new List<EraWorldMemberEquipmentDto>();
}
