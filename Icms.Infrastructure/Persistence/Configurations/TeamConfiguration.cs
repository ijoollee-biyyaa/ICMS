using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.MembershipRule)
            .IsRequired();

        builder.HasOne(t => t.Church)
            .WithMany(c => c.Teams)
            .HasForeignKey(t => t.ChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ParentTeam)
            .WithMany(t => t.SubTeams)
            .HasForeignKey(t => t.ParentTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}