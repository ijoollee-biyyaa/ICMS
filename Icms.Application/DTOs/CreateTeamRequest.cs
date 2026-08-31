using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public record CreateTeamRequest(
    string Name,
    MembershipRule? MembershipRule,
    long? ParentTeamId);