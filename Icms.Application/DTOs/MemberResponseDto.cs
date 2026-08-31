using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record MemberResponseDto(
    long Id,
    string EfgbcId,
    long ChurchId,
    string FirstName,
    string FatherName,
    string GrandfatherName,
    DateOnly DateOfBirth,
    Gender Gender,
    JobStatus JobStatus,
    string? Phone,
    string? Email,
    string? PhotoUrl,
    MemberStatus Status,
    JoinChannel JoinedVia,
    DateOnly? JoinedAt,
    DateTimeOffset CreatedAt)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}