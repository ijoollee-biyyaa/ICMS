using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(d => d.Church)
            .WithMany()
            .HasForeignKey(d => d.ChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.HeadEmployee)
            .WithMany(e => e.HeadedDepartments)
            .HasForeignKey(d => d.HeadEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}