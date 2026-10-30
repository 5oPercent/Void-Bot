using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_weapon_template")]
public sealed class EraWeaponTemplate : IEntity<Guid>
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [StringLength(128)] public string Name { get; set; } = string.Empty;
    [StringLength(256)] public string Icon { get; set; } = string.Empty;
    [StringLength(2048)] public string Note { get; set; } = string.Empty;
    public bool IsGenerationEnabled { get; set; }
    [StringLength(128)] public string PresentationKey { get; set; } = string.Empty;
    /// <summary>可复用的整套改装槽位方案；只影响后续生成。</summary>
    public Guid? SlotConfigurationId { get; set; }
    public Guid FrameworkId { get; set; }
    [Column(TypeName = "text")] public string CompatibleSlotsPayload { get; set; } = string.Empty;
}
