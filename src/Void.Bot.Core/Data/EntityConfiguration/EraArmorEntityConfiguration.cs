using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data.EntityConfiguration;

public sealed class EraArmorEntityConfiguration : IEntityTypeConfiguration<EraArmorConfigurationSlot>,
    IEntityTypeConfiguration<EraArmorModStatModifier>, IEntityTypeConfiguration<EraArmorConfigurationModPool>,
    IEntityTypeConfiguration<EraArmorInstanceModCandidate>, IEntityTypeConfiguration<EraArmorInstanceModSelection>,
    IEntityTypeConfiguration<EraArmorTemplate>, IEntityTypeConfiguration<EraArmorModDefinition>
{
    public void Configure(EntityTypeBuilder<EraArmorTemplate> builder)
    {
        builder.HasOne<EraArmorSlotConfiguration>().WithMany().HasForeignKey(row => row.SlotConfigurationId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraArmorModDefinition> builder)
    {
        builder.HasOne<EraArmorSlotType>().WithMany().HasForeignKey(row => row.SlotTypeId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraArmorConfigurationSlot> builder)
    {
        builder.HasOne<EraArmorSlotConfiguration>().WithMany().HasForeignKey(row => row.ConfigurationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraArmorSlotType>().WithMany().HasForeignKey(row => row.SlotTypeId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraArmorModStatModifier> builder)
    {
        builder.HasKey(row => new { row.ModDefinitionId, row.Sequence });
        builder.HasOne<EraArmorModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraArmorConfigurationModPool> builder)
    {
        builder.HasKey(row => new { row.SlotId, row.ModDefinitionId });
        builder.HasOne<EraArmorConfigurationSlot>().WithMany().HasForeignKey(row => row.SlotId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraArmorModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraArmorInstanceModCandidate> builder)
    {
        builder.HasAlternateKey(row => new { row.EquipmentInstanceId, row.SlotId, row.CandidateId });
        builder.HasIndex(row => new { row.EquipmentInstanceId, row.SlotId, row.ModDefinitionId }).IsUnique();
        builder.HasOne<EraEquipmentInstance>().WithMany().HasForeignKey(row => row.EquipmentInstanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraArmorModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraArmorInstanceModSelection> builder)
    {
        builder.HasKey(row => new { row.EquipmentInstanceId, row.SlotId });
        builder.HasOne<EraArmorSlotType>().WithMany().HasForeignKey(row => row.SlotTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraArmorInstanceModCandidate>().WithMany()
            .HasForeignKey(row => new { row.EquipmentInstanceId, row.SlotId, CandidateId = row.SelectedCandidateId })
            .HasPrincipalKey(row => new { row.EquipmentInstanceId, row.SlotId, row.CandidateId }).OnDelete(DeleteBehavior.Restrict);
    }
}
