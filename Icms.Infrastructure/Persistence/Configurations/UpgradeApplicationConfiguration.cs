using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class UpgradeApplicationConfiguration : IEntityTypeConfiguration<UpgradeApplication>
{
    public void Configure(EntityTypeBuilder<UpgradeApplication> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.DaughterChurchId);

        builder.Property(u => u.Feedback);
        builder.Property(u => u.Conditions);

        builder.HasOne(u => u.DaughterChurch)
            .WithMany()
            .HasForeignKey(u => u.DaughterChurchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}