using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data.EntityConfiguration;

/// <summary>由现有 DbContext 的程序集扫描加载；历史资产不随关联记录级联删除。</summary>
public sealed class EraEquipmentEntityConfiguration : IEntityTypeConfiguration<EraEquipmentInstance>
{
    public void Configure(EntityTypeBuilder<EraEquipmentInstance> builder)
    {
        // 显式数据库名称与 DbUp 脚本保持一致，避免命名约定覆盖 IndexAttribute 的模型名称。
        builder.HasIndex(equipment => new { equipment.CharacterId, equipment.EquippedSlot })
            .IsUnique().HasDatabaseName("ux_era_equipment_equipped_slot");
        builder.HasIndex(equipment => new { equipment.CharacterId, equipment.Location })
            .HasDatabaseName("ix_era_equipment_character_inventory");
        builder.HasIndex(equipment => new { equipment.OwnerPlayerId, equipment.Location })
            .HasDatabaseName("ix_era_equipment_owner_location");
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
