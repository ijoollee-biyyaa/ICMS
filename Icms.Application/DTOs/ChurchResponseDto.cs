using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record ChurchResponseDto(
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
    string? WebsiteUrl);