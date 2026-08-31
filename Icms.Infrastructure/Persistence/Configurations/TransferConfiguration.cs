using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.DestinationName).HasMaxLength(200);
        builder.Property(t => t.VoidReason).HasMaxLength(255);

        builder.HasIndex(t => t.MemberId);
        builder.HasIndex(t => new { t.FromChurchId, t.Status });

        builder.HasOne(t => t.Member)
            .WithMany(m => m.Transfers)
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.FromChurch)
            .WithMany(c => c.OutgoingTransfers)
            .HasForeignKey(t => t.FromChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToChurch)
            .WithMany(c => c.IncomingTransfers)
            .HasForeignKey(t => t.ToChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}