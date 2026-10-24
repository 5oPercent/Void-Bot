namespace Void.Bot.Message.Dto.ERA;

public sealed class EraWorldMemberEquipmentDto
{
    public Guid CharacterId { get; set; }
    public long EquipmentVersion { get; set; }
    public List<EraEquipmentInstanceDto> Equipment { get; set; } = new List<EraEquipmentInstanceDto>();
}
