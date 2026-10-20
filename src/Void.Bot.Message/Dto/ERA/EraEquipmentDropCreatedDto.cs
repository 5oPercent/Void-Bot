namespace Void.Bot.Message.Dto.ERA;

/// <summary>本次成功丢出的历史结果，重试不会重建已流转或失效的地面记录。</summary>
public sealed class EraEquipmentDropCreatedDto
{
    public Guid WorldInstanceId { get; set; }
    public Guid DropId { get; set; }
    public Guid EquipmentId { get; set; }
    public long DropVersion { get; set; }
    public long EquipmentVersion { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ShareUntil { get; set; }
    public EraEquipmentDropPlacementDto Placement { get; set; } = new EraEquipmentDropPlacementDto();
}
