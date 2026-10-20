using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data;

public sealed class EraWorldEntityConfiguration : IEntityTypeConfiguration<EraWorldMember>
{
    public void Configure(EntityTypeBuilder<EraWorldMember> builder)
    {
        builder.HasOne<EraWorld>().WithMany().HasForeignKey(member => member.WorldId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraCharacter>().WithMany()
            .HasForeignKey(member => new { member.CharacterId, member.PlayerId })
            .HasPrincipalKey(character => new { character.Id, character.PlayerId })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
