using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record InitiateTransferRequest
{
    public long MemberId { get; init; }
    
    // For same-district transfer
    public long? DestinationChurchId { get; init; }
    
    // For external transfer (cross-district / cross-denomination)
    public string? DestinationChurchName { get; init; }
    public string? DestinationDistrictOrDenomination { get; init; }
    
    public TransferType Type { get; init; }
    public string? RecommendationNotes { get; init; }
}

public record AcceptTransferRequest
{
    public string? ClearanceDocumentUrl { get; init; }
}

public record RegisterExternalIncomingRequest
{
    public long DestinationChurchId { get; init; }
    
    public string SourceChurchName { get; init; } = string.Empty;
    public string? SourceDistrictOrDenomination { get; init; }
    
    public string? ClearanceDocumentUrl { get; init; }
    
    public string IncomingFirstName { get; init; } = string.Empty;
    public string IncomingFatherName { get; init; } = string.Empty;
    public string IncomingGrandfatherName { get; init; } = string.Empty;
    public string? PreviousEfgbcId { get; init; }
}

public record TransferResponseDto
{
    public long Id { get; init; }
    public long? MemberId { get; init; }
    public string? MemberName { get; init; }
    public string? MemberEfgbcId { get; init; }
    
    public long? SourceChurchId { get; init; }
    public string? SourceChurchName { get; init; }
    public string? SourceDistrictOrDenomination { get; init; }
    
    public long? DestinationChurchId { get; init; }
    public string? DestinationChurchName { get; init; }
    public string? DestinationDistrictOrDenomination { get; init; }
    
    public TransferType Type { get; init; }
    public TransferDirection Direction { get; init; }
    public TransferStatus Status { get; init; }
    
    public string ClearanceCode { get; init; } = string.Empty;
    public string? ClearanceDocumentUrl { get; init; }
    public string? RecommendationNotes { get; init; }
    
    public string? IncomingFirstName { get; init; }
    public string? IncomingFatherName { get; init; }
    public string? IncomingGrandfatherName { get; init; }
    public string? PreviousEfgbcId { get; init; }
    
    public string? InitiatedByUserId { get; init; }
    public DateTimeOffset InitiatedAt { get; init; }
    public string? CompletedByUserId { get; init; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? VoidReason { get; init; }
    public DateTimeOffset? VoidedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record TransferStatsDto
{
    public int Total { get; init; }
    public int Initiated { get; init; }
    public int Completed { get; init; }
    public int Voided { get; init; }
    public int Incoming { get; init; }
    public int Outgoing { get; init; }
    public int Internal { get; init; }
    public int External { get; init; }
}

public record TransferLookupDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public long DistrictId { get; init; }
    public string DistrictName { get; init; } = string.Empty;
}

public record RejoinTransferRequest
{
    public long MemberId { get; init; }
    public string? RecommendationNotes { get; init; }
    public string? ClearanceDocumentUrl { get; init; }
}

public record MemberLookupDto(long Id, string? EfgbcId, string FullName, Gender Gender, string? ChurchName);
