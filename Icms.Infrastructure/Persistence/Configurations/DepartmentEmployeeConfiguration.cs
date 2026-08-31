using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class DepartmentEmployeeConfiguration : IEntityTypeConfiguration<DepartmentEmployee>
{
    public void Configure(EntityTypeBuilder<DepartmentEmployee> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => new { d.DepartmentId, d.EmployeeId })
            .IsUnique();

        builder.HasOne(d => d.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Employee)
            .WithMany()
            .HasForeignKey(d => d.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}