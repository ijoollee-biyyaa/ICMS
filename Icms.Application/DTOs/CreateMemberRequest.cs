using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record CreateMemberRequest(
    long ChurchId,
    string FirstName,
    string FatherName,
    string GrandfatherName,
    DateOnly? DateOfBirth,
    Gender Gender,
    JobStatus JobStatus,
    string? Phone,
    string? Email,
    string? PhotoUrl,
    JoinChannel JoinedVia,
    DateOnly? JoinedAt);