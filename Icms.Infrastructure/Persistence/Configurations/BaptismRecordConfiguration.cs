using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class BaptismRecordConfiguration : IEntityTypeConfiguration<BaptismRecord>
{
    public void Configure(EntityTypeBuilder<BaptismRecord> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.ChildName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.PlaceOfBirth)
            .HasMaxLength(255);

        builder.HasOne(b => b.Member)
            .WithMany(m => m.Baptisms)
            .HasForeignKey(b => b.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Father)
            .WithMany()
            .HasForeignKey(b => b.FatherMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Mother)
            .WithMany()
            .HasForeignKey(b => b.MotherMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Minister)
            .WithMany()
            .HasForeignKey(b => b.MinisterEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Church)
            .WithMany()
            .HasForeignKey(b => b.ChurchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}