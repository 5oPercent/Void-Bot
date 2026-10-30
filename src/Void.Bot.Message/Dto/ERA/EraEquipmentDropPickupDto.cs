namespace Void.Bot.Message.Dto.ERA;

public sealed class EraEquipmentDropPickupDto
{
    public Guid WorldInstanceId { get; set; }
    public Guid DropId { get; set; }
    public Guid EquipmentInstanceId { get; set; }
    public long DropVersion { get; set; }
}
