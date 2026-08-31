using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class DeathRecordConfiguration : IEntityTypeConfiguration<DeathRecord>
{
    public void Configure(EntityTypeBuilder<DeathRecord> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => d.MemberId)
            .IsUnique();

        builder.Property(d => d.RecordedBy).HasMaxLength(100);

        builder.HasOne(d => d.Member)
            .WithOne(m => m.DeathRecord)
            .HasForeignKey<DeathRecord>(d => d.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Church)
            .WithMany()
            .HasForeignKey(d => d.ChurchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}