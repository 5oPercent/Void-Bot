using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data.EntityConfiguration;

public sealed class EraWeaponEntityConfiguration : IEntityTypeConfiguration<EraWeaponConfigurationSlot>,
    IEntityTypeConfiguration<EraWeaponModStatModifier>, IEntityTypeConfiguration<EraWeaponConfigurationModPool>,
    IEntityTypeConfiguration<EraWeaponModCandidate>, IEntityTypeConfiguration<EraWeaponModSelection>,
    IEntityTypeConfiguration<EraWeaponTemplate>, IEntityTypeConfiguration<EraWeaponModDefinition>
{
    public void Configure(EntityTypeBuilder<EraWeaponTemplate> builder)
    {
        builder.HasOne<EraWeaponStatMapping>().WithMany().HasForeignKey(row => row.StatMappingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponSlotConfiguration>().WithMany().HasForeignKey(row => row.SlotConfigurationId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponModDefinition> builder)
    {
        builder.HasOne<EraWeaponSlotType>().WithMany().HasForeignKey(row => row.SlotTypeId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponConfigurationSlot> builder)
    {
        builder.HasOne<EraWeaponSlotConfiguration>().WithMany().HasForeignKey(row => row.ConfigurationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponSlotType>().WithMany().HasForeignKey(row => row.SlotTypeId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponModStatModifier> builder)
    {
        builder.HasKey(row => new { row.ModDefinitionId, row.Sequence });
        builder.HasOne<EraWeaponModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponConfigurationModPool> builder)
    {
        builder.HasKey(row => new { row.SlotId, row.ModDefinitionId });
        builder.HasOne<EraWeaponConfigurationSlot>().WithMany().HasForeignKey(row => row.SlotId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponModCandidate> builder)
    {
        builder.HasAlternateKey(row => new { row.EquipmentId, row.SlotId, row.CandidateId });
        builder.HasIndex(row => new { row.EquipmentId, row.SlotId, row.ModDefinitionId }).IsUnique();
        builder.HasOne<EraEquipmentInstance>().WithMany().HasForeignKey(row => row.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponModSelection> builder)
    {
        builder.HasKey(row => new { row.EquipmentId, row.SlotId });
        builder.HasOne<EraWeaponSlotType>().WithMany().HasForeignKey(row => row.SlotTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponModCandidate>().WithMany()
            .HasForeignKey(row => new { row.EquipmentId, row.SlotId, CandidateId = row.SelectedCandidateId })
            .HasPrincipalKey(row => new { row.EquipmentId, row.SlotId, row.CandidateId }).OnDelete(DeleteBehavior.Restrict);
    }
}
