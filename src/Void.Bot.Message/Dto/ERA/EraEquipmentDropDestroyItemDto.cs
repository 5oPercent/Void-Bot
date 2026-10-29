namespace Void.Bot.Message.Dto.ERA;

public sealed class EraEquipmentDropDestroyItemDto
{
    public Guid DropId { get; set; }
    public long ExpectedDropVersion { get; set; }
    public long ExpectedItemVersion { get; set; }
}
