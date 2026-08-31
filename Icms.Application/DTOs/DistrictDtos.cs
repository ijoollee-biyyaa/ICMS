namespace Icms.Application.DTOs;

public record CreateDistrictRequest(
    string Name,
    string Code,
    string? Address);

public record UpdateDistrictRequest(
    string Name,
    string Code,
    string? Address);

public record DistrictResponseDto(
    long Id,
    string Name,
    string Code,
    string? Address,
    DateTimeOffset CreatedAt)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}

public record DistrictDetailDto(
    long Id,
    string Name,
    string Code,
    string? Address,
    DateTimeOffset CreatedAt,
    int ChurchCount,
    int MemberCount)
{
    public IReadOnlyList<LinkDto> Links { get; init; } = [];
}