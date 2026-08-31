using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.HasKey(f => f.Id);

        builder.HasIndex(f => new { f.FamilyId, f.MemberId })
            .IsUnique();

        builder.HasOne(f => f.Family)
            .WithMany(f => f.FamilyMembers)
            .HasForeignKey(f => f.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Member)
            .WithMany()
            .HasForeignKey(f => f.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}