using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record MemberResponseDto(
    long Id,
    string EfgbcId,
    long ChurchId,
    string FirstName,
    string FatherName,
    string GrandfatherName,
    DateOnly? DateOfBirth,
    Gender Gender,
    MaritalStatus MaritalStatus,
    JobStatus JobStatus,
    HealthStatus HealthStatus,
    string? Phone,
    string? Email,
    string? City,
    string? Subcity,
    string? LocalAddress,
    string? PhotoUrl,
    MemberStatus Status,
    JoinChannel JoinedVia,
    DateOnly? JoinedAt,
    DateOnly? ConversionDate,
    string? BaptismPlace,
    DateOnly? BaptismDate,
    string? SpiritualGift,
    long? ClearanceId,
    DateTimeOffset CreatedAt)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}