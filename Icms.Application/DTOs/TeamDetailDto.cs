using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record TeamDetailDto(
    long Id,
    long ChurchId,
    long? ParentTeamId,
    MembershipRule MembershipRule,
    string Name,
    DateTimeOffset CreatedAt,
    int SubTeamCount,
    int MemberCount)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}