using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TitheConfiguration : IEntityTypeConfiguration<Tithe>
{
    public void Configure(EntityTypeBuilder<Tithe> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.HasIndex(t => new { t.ChurchId, t.IncomeDate });
        builder.HasIndex(t => t.MemberId);

        builder.HasOne(t => t.Church)
            .WithMany()
            .HasForeignKey(t => t.ChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Member)
            .WithMany(m => m.Tithes)
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}