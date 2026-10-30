using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_armor_instance_mod_candidate")]
public sealed class EraArmorInstanceModCandidate : IEntityBase
{
    [Key] public Guid CandidateId { get; set; }
    public Guid EquipmentInstanceId { get; set; }
    public Guid SlotId { get; set; }
    public Guid ModDefinitionId { get; set; }
    public int DisplayOrder { get; set; }
}
