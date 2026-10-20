namespace Void.Bot.Message.Dto.ERA;

/// <summary>固定的历史成功结果；EquipmentIds 不表示装备当前仍在该角色背包。</summary>
public sealed class EraInventoryRewardReceipt
{
    public Guid EventId { get; set; }
    public DateTimeOffset GrantedAt { get; set; }
    public List<EraInventoryRewardGrantReceipt> Grants { get; set; } = new List<EraInventoryRewardGrantReceipt>();
}
