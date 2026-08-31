using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class MemberDocumentConfiguration : IEntityTypeConfiguration<MemberDocument>
{
    public void Configure(EntityTypeBuilder<MemberDocument> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileUrl)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasOne(d => d.Member)
            .WithMany(m => m.Documents)
            .HasForeignKey(d => d.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}