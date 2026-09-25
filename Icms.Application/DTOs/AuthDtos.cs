namespace Icms.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string FatherName,
    string GrandfatherName,
    string Role);

public record UserProfileDto(
    string UserId,
    string Email,
    string DisplayName,
    string Role,
    long? ChurchId,
    long? MemberId,
    string? ChurchName);