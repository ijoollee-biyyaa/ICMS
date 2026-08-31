using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class LearningRecordConfiguration : IEntityTypeConfiguration<LearningRecord>
{
    public void Configure(EntityTypeBuilder<LearningRecord> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(l => l.Member)
            .WithMany(m => m.Learnings)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}