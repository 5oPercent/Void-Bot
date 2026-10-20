namespace Void.Bot.Message.Dto.ERA;

public sealed class EraInventoryRewardGrantReceipt
{
    public Guid GrantId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    public long PreviousEquipmentVersion { get; set; }
    public long EquipmentVersion { get; set; }
    public List<Guid> EquipmentIds { get; set; } = new List<Guid>();
}
