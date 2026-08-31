using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class Transfer
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public long FromChurchId { get; set; }
    public long? ToChurchId { get; set; }
    public string? DestinationName { get; set; }
    public TransferType Type { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Initiated;
    public TransferInitiator InitiatedBy { get; set; }
    public string? InitiatedByUserId { get; set; }
    public string? VoidReason { get; set; }
    public string? VoidedByUserId { get; set; }
    public DateTimeOffset? VoidedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Member Member { get; set; } = null!;
    public Church FromChurch { get; set; } = null!;
    public Church? ToChurch { get; set; }
    public ClearanceCertificate? Certificate { get; set; }
}
