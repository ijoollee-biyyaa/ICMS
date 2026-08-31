namespace Icms.Domain.Entities;

public class ClearanceCertificate
{
    public long Id { get; set; }
    public long TransferId { get; set; }
    public required string CertificateCode { get; set; }
    public long MemberId { get; set; }
    public long FromChurchId { get; set; }
    public long? ToChurchId { get; set; }
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? IssuedByUserId { get; set; }

    public Transfer Transfer { get; set; } = null!;
    public Member Member { get; set; } = null!;
    public Church FromChurch { get; set; } = null!;
    public Church? ToChurch { get; set; }
}
