using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_reward_grant_equipment")]
[PrimaryKey(nameof(GrantId), nameof(EquipmentId))]
[Index(nameof(EquipmentId), IsUnique = true, Name = "ux_era_reward_grant_equipment")]
public sealed class EraRewardGrantEquipment : IEntityBase
{
    public Guid GrantId { get; set; }
    public Guid EquipmentId { get; set; }
}
