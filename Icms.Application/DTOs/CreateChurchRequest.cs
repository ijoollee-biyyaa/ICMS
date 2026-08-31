using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record CreateChurchRequest(
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
    string AdminFirstName,
    string AdminFatherName,
    string AdminGrandfatherName);

public record AddChurchAdminRequest(
    long? MemberId,
    string? FirstName,
    string? FatherName,
    string? GrandfatherName);

public record CreateChurchResponseDto(
    ChurchResponseDto Church,
    string AdminEmail,
    string AdminTempPassword);