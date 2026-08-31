using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TeamActivityConfiguration : IEntityTypeConfiguration<TeamActivity>
{
    public void Configure(EntityTypeBuilder<TeamActivity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(a => a.Team)
            .WithMany(t => t.Activities)
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}