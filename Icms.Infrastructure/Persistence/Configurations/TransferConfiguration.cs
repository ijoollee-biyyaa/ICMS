using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ClearanceCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(t => t.ClearanceCode)
            .IsUnique();

        builder.HasOne(t => t.Member)
            .WithMany(m => m.Transfers)
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.SourceChurch)
            .WithMany(c => c.OutgoingTransfers)
            .HasForeignKey(t => t.SourceChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.DestinationChurch)
            .WithMany(c => c.IncomingTransfers)
            .HasForeignKey(t => t.DestinationChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.SourceChurchName).HasMaxLength(200);
        builder.Property(t => t.SourceDistrictOrDenomination).HasMaxLength(200);
        
        builder.Property(t => t.DestinationChurchName).HasMaxLength(200);
        builder.Property(t => t.DestinationDistrictOrDenomination).HasMaxLength(200);

        builder.Property(t => t.IncomingFirstName).HasMaxLength(100);
        builder.Property(t => t.IncomingFatherName).HasMaxLength(100);
        builder.Property(t => t.IncomingGrandfatherName).HasMaxLength(100);
        builder.Property(t => t.PreviousEfgbcId).HasMaxLength(30);

        builder.Property(t => t.ClearanceDocumentUrl).HasMaxLength(500);
        builder.Property(t => t.RecommendationNotes).HasMaxLength(2000);
        builder.Property(t => t.VoidReason).HasMaxLength(500);

        builder.Property(t => t.InitiatedByUserId).HasMaxLength(450);
        builder.Property(t => t.CompletedByUserId).HasMaxLength(450);
        builder.Property(t => t.VoidedByUserId).HasMaxLength(450);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}