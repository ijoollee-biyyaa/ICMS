using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Persistence.Configurations;

public class ClearanceCertificateConfiguration : IEntityTypeConfiguration<ClearanceCertificate>
{
    public void Configure(EntityTypeBuilder<ClearanceCertificate> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CertificateCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(c => c.CertificateCode)
            .IsUnique();

        builder.HasIndex(c => c.TransferId)
            .IsUnique();

        builder.HasOne(c => c.Transfer)
            .WithOne(t => t.Certificate)
            .HasForeignKey<ClearanceCertificate>(c => c.TransferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Member)
            .WithMany()
            .HasForeignKey(c => c.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.FromChurch)
            .WithMany()
            .HasForeignKey(c => c.FromChurchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ToChurch)
            .WithMany()
            .HasForeignKey(c => c.ToChurchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}