using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record CreateMemberRequest(
    long ChurchId,
    string FirstName,
    string FatherName,
    string GrandfatherName,
    DateOnly? DateOfBirth,
    Gender Gender,
    MaritalStatus MaritalStatus = MaritalStatus.Single,
    JobStatus JobStatus = JobStatus.Other,
    HealthStatus HealthStatus = HealthStatus.Healthy,
    string? Phone = null,
    string? Email = null,
    string? City = null,
    string? Subcity = null,
    string? LocalAddress = null,
    string? PhotoUrl = null,
    JoinChannel JoinedVia = JoinChannel.Salvation,
    DateOnly? JoinedAt = null,
    DateOnly? ConversionDate = null,
    string? BaptismPlace = null,
    DateOnly? BaptismDate = null,
    string? SpiritualGift = null,
    long? ClearanceId = null);