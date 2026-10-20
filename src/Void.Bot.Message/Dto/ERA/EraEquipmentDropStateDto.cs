namespace Void.Bot.Message.Dto.ERA;

public sealed class EraEquipmentDropStateDto
{
    public Guid WorldInstanceId { get; set; }
    public long WorldVersion { get; set; }
    public Guid CharacterId { get; set; }
    public long EquipmentVersion { get; set; }
    public long? ManifestVersion { get; set; }
    public List<EraEquipmentDropDto> Drops { get; set; } = new List<EraEquipmentDropDto>();
}
