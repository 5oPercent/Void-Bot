using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data.EntityConfiguration;

public sealed class EraRewardEntityConfiguration : IEntityTypeConfiguration<EraRewardEvent>,
    IEntityTypeConfiguration<EraRewardRequest>, IEntityTypeConfiguration<EraRewardGrant>,
    IEntityTypeConfiguration<EraRewardGrantEquipment>
{
    public void Configure(EntityTypeBuilder<EraRewardEvent> builder)
    {
        builder.Property(row => row.SourceDefinitionId).UseCollation("utf8mb4_bin");
        builder.Property(row => row.RewardNodeId).UseCollation("utf8mb4_bin");
        builder.HasOne<EraRewardScope>().WithMany().HasForeignKey(row => row.ScopeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWorld>().WithMany().HasForeignKey(row => row.WorldInstanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasAlternateKey(row => new { row.Id, row.ScopeId }).HasName("ux_era_reward_event_scope");
    }

    public void Configure(EntityTypeBuilder<EraRewardRequest> builder)
    {
        builder.HasOne<EraRewardEvent>().WithMany().HasForeignKey(row => new { row.EventId, row.ScopeId })
            .HasPrincipalKey(row => new { row.Id, row.ScopeId }).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraRewardGrant> builder)
    {
        builder.HasOne<EraRewardEvent>().WithMany().HasForeignKey(row => row.EventId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraCharacter>().WithMany().HasForeignKey(row => new { row.CharacterId, row.PlayerId })
            .HasPrincipalKey(row => new { row.Id, row.PlayerId }).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraRewardGrantEquipment> builder)
    {
        builder.HasOne<EraRewardGrant>().WithMany().HasForeignKey(row => row.GrantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraEquipmentInstance>().WithMany().HasForeignKey(row => row.EquipmentInstanceId).OnDelete(DeleteBehavior.Restrict);
    }
}
