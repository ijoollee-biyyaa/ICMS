using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record TeamMemberDto(
    long MemberId,
    string MemberName,
    string MemberEfgbcId,
    TeamMemberRole Role,
    DateTimeOffset JoinedAt)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}