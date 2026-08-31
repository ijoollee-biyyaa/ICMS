using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class TeamAttendanceConfiguration : IEntityTypeConfiguration<TeamAttendance>
{
    public void Configure(EntityTypeBuilder<TeamAttendance> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Reason)
            .HasMaxLength(200);

        builder.HasIndex(a => new { a.TeamId, a.AttendanceDate });

        builder.HasIndex(a => new { a.TeamId, a.MemberId, a.AttendanceDate })
            .IsUnique();

        builder.HasOne(a => a.Team)
            .WithMany(t => t.Attendances)
            .HasForeignKey(a => a.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Member)
            .WithMany()
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}