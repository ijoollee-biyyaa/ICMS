using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TeamMeetingConfiguration : IEntityTypeConfiguration<TeamMeeting>
{
    public void Configure(EntityTypeBuilder<TeamMeeting> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .HasMaxLength(200);

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        builder.HasIndex(m => new { m.TeamId, m.MeetingDate });

        builder.HasOne(m => m.Team)
            .WithMany(t => t.Meetings)
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.CreatedBy)
            .WithMany()
            .HasForeignKey(m => m.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}