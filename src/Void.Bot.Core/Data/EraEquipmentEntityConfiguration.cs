using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data;

/// <summary>由现有 DbContext 的程序集扫描加载；历史资产不随关联记录级联删除。</summary>
public sealed class EraEquipmentEntityConfiguration : IEntityTypeConfiguration<EraEquipmentInstance>
{
    public void Configure(EntityTypeBuilder<EraEquipmentInstance> builder)
    {
        builder.Property(equipment => equipment.DefinitionId).UseCollation("utf8mb4_bin");
        builder.HasOne<EraPlayer>().WithMany()
            .HasForeignKey(equipment => equipment.OwnerPlayerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraCharacter>().WithMany()
            .HasForeignKey(equipment => new { equipment.CharacterId, equipment.OwnerPlayerId })
            .HasPrincipalKey(character => new { character.Id, character.PlayerId })
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraArmorInstanceData>().WithOne()
            .HasForeignKey<EraArmorInstanceData>(armor => armor.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
