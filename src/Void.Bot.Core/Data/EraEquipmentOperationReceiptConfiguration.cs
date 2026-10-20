using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data;

public sealed class EraEquipmentOperationReceiptConfiguration : IEntityTypeConfiguration<EraEquipmentOperationReceipt>
{
    public void Configure(EntityTypeBuilder<EraEquipmentOperationReceipt> builder)
    {
        builder.HasOne<EraCharacter>().WithMany()
            .HasForeignKey(receipt => new { receipt.CharacterId, receipt.PlayerId })
            .HasPrincipalKey(character => new { character.Id, character.PlayerId })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
