namespace Void.Bot.Message.Dto.ERA;

public sealed class EraGroundRewardGrantReceiptDto
{
    public Guid GrantId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid CharacterId { get; set; }
    public List<Guid> EquipmentIds { get; set; } = new List<Guid>();
    public List<Guid> DropIds { get; set; } = new List<Guid>();
}
