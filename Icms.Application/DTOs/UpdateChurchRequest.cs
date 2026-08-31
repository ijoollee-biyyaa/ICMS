namespace Icms.Application.DTOs;

public record UpdateChurchRequest(
    string Name,
    string Code,
    string? City,
    string? Subcity,
    string? Email,
    string? Phone,
    string? Tel,
    string? MapAddress,
    string? WebsiteUrl);