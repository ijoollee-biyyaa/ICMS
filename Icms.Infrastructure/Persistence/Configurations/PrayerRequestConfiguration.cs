using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class PrayerRequestConfiguration : IEntityTypeConfiguration<PrayerRequest>
{
    public void Configure(EntityTypeBuilder<PrayerRequest> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Content)
            .IsRequired();

        builder.HasOne(p => p.Member)
            .WithMany(m => m.PrayerRequests)
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}