using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class DepartmentActivityConfiguration : IEntityTypeConfiguration<DepartmentActivity>
{
    public void Configure(EntityTypeBuilder<DepartmentActivity> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(a => a.Department)
            .WithMany(d => d.Activities)
            .HasForeignKey(a => a.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}