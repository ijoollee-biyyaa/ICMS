using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record ChurchDetailDto(
    long Id,
    string Name,
    ChurchType Type,
    string Code,
    long? ParentChurchId,
    string? City,
    string? Subcity,
    string? Email,
    string? Phone,
    string? Tel,
    string? MapAddress,
    string? WebsiteUrl,
    int DaughterCount,
    int MemberCount)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}