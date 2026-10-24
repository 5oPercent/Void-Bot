using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data.EntityConfiguration;

public sealed class EraWeaponEntityConfiguration : IEntityTypeConfiguration<EraWeaponTemplateModSlot>,
    IEntityTypeConfiguration<EraWeaponModStatModifier>, IEntityTypeConfiguration<EraWeaponTemplateModPool>,
    IEntityTypeConfiguration<EraWeaponModCandidate>, IEntityTypeConfiguration<EraWeaponModSelection>, IEntityTypeConfiguration<EraWeaponTemplate>
{
    public void Configure(EntityTypeBuilder<EraWeaponTemplate> builder)
    {
        builder.HasOne<EraWeaponStatEffectProfile>().WithMany().HasForeignKey(row => row.StatEffectProfileId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponTemplateModSlot> builder)
    {
        builder.HasKey(row => new { row.TemplateId, row.Slot });
        builder.HasOne<EraWeaponTemplate>().WithMany().HasForeignKey(row => row.TemplateId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponModStatModifier> builder)
    {
        builder.HasKey(row => new { row.ModDefinitionId, row.Sequence });
        builder.HasOne<EraWeaponModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponTemplateModPool> builder)
    {
        builder.HasKey(row => new { row.TemplateId, row.Slot, row.ModDefinitionId });
        builder.HasOne<EraWeaponTemplateModSlot>().WithMany().HasForeignKey(row => new { row.TemplateId, row.Slot }).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponModCandidate> builder)
    {
        builder.HasAlternateKey(row => new { row.EquipmentId, row.Slot, row.CandidateId });
        builder.HasIndex(row => new { row.EquipmentId, row.Slot, row.ModDefinitionId }).IsUnique();
        builder.HasOne<EraEquipmentInstance>().WithMany().HasForeignKey(row => row.EquipmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWeaponModDefinition>().WithMany().HasForeignKey(row => row.ModDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraWeaponModSelection> builder)
    {
        builder.HasKey(row => new { row.EquipmentId, row.Slot });
        builder.HasOne<EraWeaponModCandidate>().WithMany()
            .HasForeignKey(row => new { row.EquipmentId, row.Slot, CandidateId = row.SelectedCandidateId })
            .HasPrincipalKey(row => new { row.EquipmentId, row.Slot, row.CandidateId }).OnDelete(DeleteBehavior.Restrict);
    }
}
