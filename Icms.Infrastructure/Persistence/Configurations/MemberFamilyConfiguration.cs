using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class MemberFamilyConfiguration : IEntityTypeConfiguration<MemberFamily>
{
    public void Configure(EntityTypeBuilder<MemberFamily> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FamilyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(f => f.Member)
            .WithMany()
            .HasForeignKey(f => f.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.HeadOfHousehold)
            .WithMany()
            .HasForeignKey(f => f.HeadOfHouseholdMemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}