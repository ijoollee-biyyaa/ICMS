using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TransferSnapshotConfiguration : IEntityTypeConfiguration<TransferSnapshot>
{
    public void Configure(EntityTypeBuilder<TransferSnapshot> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasOne(s => s.Transfer)
            .WithOne(t => t.Snapshot)
            .HasForeignKey<TransferSnapshot>(s => s.TransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.MemberId);
    }
}
