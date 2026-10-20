using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment.Models;

/// <summary>单条查询产生的内部快照，不直接暴露持久化实体给 API。</summary>
public sealed class EraEquipmentReadSnapshot
{
    public Guid CharacterId { get; set; }
    public Guid PlayerId { get; set; }
    public bool IsCharacterDisabled { get; set; }
    public long EquipmentVersion { get; set; }
    public EraEquippedOperationRestrictionDto? EquippedOperationRestriction { get; set; }
    public List<EraEquipmentReadEntry> Entries { get; set; } = new List<EraEquipmentReadEntry>();
}
