using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.EfgbcId)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(m => m.EfgbcId)
            .IsUnique();

        builder.Property(m => m.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.FatherName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.GrandfatherName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Phone).HasMaxLength(20);
        builder.Property(m => m.Email).HasMaxLength(150);
        builder.Property(m => m.PhotoUrl).HasMaxLength(255);
        builder.Property(m => m.City).HasMaxLength(100);
        builder.Property(m => m.Subcity).HasMaxLength(100);
        builder.Property(m => m.LocalAddress).HasMaxLength(200);
        builder.Property(m => m.BaptismPlace).HasMaxLength(200);
        builder.Property(m => m.SpiritualGift).HasMaxLength(150);

        builder.HasIndex(m => new { m.ChurchId, m.Status });
        builder.HasIndex(m => m.Phone);
        builder.HasIndex(m => m.Email);

        builder.HasOne(m => m.Church)
            .WithMany(c => c.Members)
            .HasForeignKey(m => m.ChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}