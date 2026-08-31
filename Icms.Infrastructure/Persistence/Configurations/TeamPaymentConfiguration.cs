using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TeamPaymentConfiguration : IEntityTypeConfiguration<TeamPayment>
{
    public void Configure(EntityTypeBuilder<TeamPayment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.HasIndex(p => new { p.TeamId, p.Month });
        builder.HasIndex(p => p.MemberId);
        builder.HasIndex(p => new { p.TeamId, p.MemberId, p.Month })
            .IsUnique();

        builder.HasOne(p => p.Team)
            .WithMany(t => t.Payments)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Member)
            .WithMany()
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}