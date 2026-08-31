using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class ChurchConfiguration : IEntityTypeConfiguration<Church>
{
    public void Configure(EntityTypeBuilder<Church> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.HasIndex(c => c.Code)
            .IsUnique();

        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.Subcity).HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(150);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.Tel).HasMaxLength(20);
        builder.Property(c => c.WebsiteUrl).HasMaxLength(255);

        builder.HasIndex(c => c.DistrictId);
        builder.HasIndex(c => c.ParentChurchId);

        builder.HasOne(c => c.District)
            .WithMany(d => d.Churches)
            .HasForeignKey(c => c.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ParentChurch)
            .WithMany(c => c.ChildChurches)
            .HasForeignKey(c => c.ParentChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}