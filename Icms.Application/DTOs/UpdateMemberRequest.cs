using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record UpdateMemberRequest(
    string FirstName,
    string FatherName,
    string GrandfatherName,
    DateOnly? DateOfBirth,
    Gender Gender,
    JobStatus JobStatus,
    string? Phone,
    string? Email,
    string? PhotoUrl);