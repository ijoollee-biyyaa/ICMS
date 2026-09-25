using Icms.Domain.Enums;

namespace Icms.Domain.Entities;

public class Transfer
{
    public long Id { get; set; }
    public long? MemberId { get; set; }

    // Source
    public long? SourceChurchId { get; set; }
    public string? SourceChurchName { get; set; }
    public string? SourceDistrictOrDenomination { get; set; }

    // Destination
    public long? DestinationChurchId { get; set; }
    public string? DestinationChurchName { get; set; }
    public string? DestinationDistrictOrDenomination { get; set; }

    public TransferType Type { get; set; }
    public TransferDirection Direction { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Initiated;

    // Clearance document
    public string ClearanceCode { get; set; } = string.Empty;
    public string? ClearanceDocumentUrl { get; set; }
    public string? RecommendationNotes { get; set; }

    // For external incoming — person info before member record exists
    public string? IncomingFirstName { get; set; }
    public string? IncomingFatherName { get; set; }
    public string? IncomingGrandfatherName { get; set; }
    public string? PreviousEfgbcId { get; set; }

    // Audit
    public string? InitiatedByUserId { get; set; }
    public DateTimeOffset InitiatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CompletedByUserId { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? VoidReason { get; set; }
    public string? VoidedByUserId { get; set; }
    public DateTimeOffset? VoidedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Member? Member { get; set; }
    public Church? SourceChurch { get; set; }
    public Church? DestinationChurch { get; set; }
    public TransferSnapshot? Snapshot { get; set; }
}
