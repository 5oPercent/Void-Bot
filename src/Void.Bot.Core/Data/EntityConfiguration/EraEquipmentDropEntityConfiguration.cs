using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Data.EntityConfiguration;

public sealed class EraEquipmentDropEntityConfiguration : IEntityTypeConfiguration<EraEquipmentDrop>, IEntityTypeConfiguration<EraEquipmentShare>
{
    public void Configure(EntityTypeBuilder<EraEquipmentDrop> builder)
    {
        builder.HasOne<EraEquipmentInstance>().WithMany().HasForeignKey(row => row.EquipmentInstanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraWorld>().WithMany().HasForeignKey(row => row.WorldInstanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraPlayer>().WithMany().HasForeignKey(row => row.PrivateOwnerPlayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EraPlayer>().WithMany().HasForeignKey(row => row.DroppedByPlayerId).OnDelete(DeleteBehavior.Restrict);
    }

    public void Configure(EntityTypeBuilder<EraEquipmentShare> builder)
    {
        builder.HasOne<EraEquipmentInstance>().WithOne().HasForeignKey<EraEquipmentShare>(row => row.EquipmentInstanceId).OnDelete(DeleteBehavior.Restrict);
    }
}
