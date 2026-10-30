using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data.EntityConfiguration;

public sealed class EraWeaponEntityConfiguration : IEntityTypeConfiguration<EraWeaponConfigurationSlot>,
    IEntityTypeConfiguration<EraWeaponModStatModifier>, IEntityTypeConfiguration<EraWeaponConfigurationModPool>,
    IEntityTypeConfiguration<EraWeaponInstanceModCandidate>, IEntityTypeConfiguration<EraWeaponInstanceModSelection>,
    IEntityTypeConfiguration<EraWeaponTemplateStat>, IEntityTypeConfiguration<EraWeaponFrameworkStat>, IEntityTypeConfiguration<EraWeaponFrameworkCapability>,
    IEntityTypeConfiguration<EraWeaponTemplate>, IEntityTypeConfiguration<EraWeaponModDefinition>
{
    public void Configure(EntityTypeBuilder<EraWeaponTemplateStat> builder)
    {
        builder.HasKey(row => new { row.TemplateId, row.Stat });
        builder.HasOne<EraWeaponTemplate>().WithMany().HasForeignKey(row => row.TemplateId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponFrameworkStat> builder)
    {
        builder.HasKey(row => new { row.FrameworkId, row.Stat });
        builder.HasOne<EraWeaponFramework>().WithMany().HasForeignKey(row => row.FrameworkId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponFrameworkCapability> builder)
    {
        builder.HasKey(row => new { row.FrameworkId, row.CapabilityKind });
        builder.HasOne<EraWeaponFramework>().WithMany().HasForeignKey(row => row.FrameworkId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponTemplate> builder)
    {
        builder.HasOne<EraWeaponFramework>().WithMany().HasForeignKey(row => row.FrameworkId).OnDelete(DeleteBehavior.Restrict);
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

    public void Configure(EntityTypeBuilder<EraWeaponInstanceModCandidate> builder)
    {
        builder.HasAlternateKey(row => new { row.EquipmentInstanceId, row.SlotId, row.CandidateId });
        builder.HasIndex(row => new { row.EquipmentInstanceId, row.SlotId, row.ModDefinitionId }).IsUnique();
        builder.HasOne<EraEquipmentInstance>().WithMany().HasForeignKey(row => row.EquipmentInstanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponInstanceModSelection> builder)
    {
        builder.HasKey(row => new { row.EquipmentInstanceId, row.SlotId });
        builder.HasOne<EraWeaponSlotType>().WithMany().HasForeignKey(row => row.SlotTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponInstanceModCandidate>().WithMany()
            .HasForeignKey(row => new { row.EquipmentInstanceId, row.SlotId, CandidateId = row.SelectedCandidateId })
            .HasPrincipalKey(row => new { row.EquipmentInstanceId, row.SlotId, row.CandidateId }).OnDelete(DeleteBehavior.Restrict);
    }
}
