using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Position)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Salary)
            .HasPrecision(18, 2);

        builder.HasIndex(e => new { e.MemberId, e.ChurchId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => new { e.UserId, e.ChurchId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasIndex(e => e.IsDistrictPresident)
            .IsUnique()
            .HasFilter("\"IsDistrictPresident\" = true AND \"IsDeleted\" = false");

        builder.HasIndex(e => e.IsVicePresident)
            .IsUnique()
            .HasFilter("\"IsVicePresident\" = true AND \"IsDeleted\" = false");

        builder.HasOne(e => e.Church)
            .WithMany(c => c.Employees)
            .HasForeignKey(e => e.ChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.District)
            .WithMany()
            .HasForeignKey(e => e.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Member)
            .WithMany(m => m.Employees)
            .HasForeignKey(e => e.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}