using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasKey(t => t.Id);

        builder.HasIndex(t => new { t.TeamId, t.MemberId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasOne(t => t.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(t => t.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Member)
            .WithMany(m => m.TeamMemberships)
            .HasForeignKey(t => t.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}