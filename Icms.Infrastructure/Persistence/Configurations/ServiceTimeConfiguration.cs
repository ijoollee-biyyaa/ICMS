using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class ServiceTimeConfiguration : IEntityTypeConfiguration<ServiceTime>
{
    public void Configure(EntityTypeBuilder<ServiceTime> builder)
    {
        builder.HasKey(s => s.Id);

        builder.HasIndex(s => new { s.ChurchId, s.DayOfWeek });

        builder.Property(s => s.ServiceType)
            .HasMaxLength(50);

        builder.HasOne(s => s.Church)
            .WithMany(c => c.ServiceTimes)
            .HasForeignKey(s => s.ChurchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}