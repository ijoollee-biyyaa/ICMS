using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record TeamResponseDto(
    long Id,
    long ChurchId,
    long? ParentTeamId,
    MembershipRule MembershipRule,
    string Name,
    DateTimeOffset CreatedAt)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}