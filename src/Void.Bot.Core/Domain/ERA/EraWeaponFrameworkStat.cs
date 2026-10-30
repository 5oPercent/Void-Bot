using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_framework_stat")]
public sealed class EraWeaponFrameworkStat : IEntityBase
{
    public Guid FrameworkId { get; set; }
    public EraWeaponStat Stat { get; set; }
    public int DisplayOrder { get; set; }
}
