namespace Icms.Application.DTOs;

/// <summary>Whether a member already has a login account. Mirrors GET /api/members/{id}/account.</summary>
public record MemberAccountInfoDto(long MemberId, bool HasAccount, string? Email);

/// <summary>Result of issuing a member login. TempPassword is only set when the account was just created. Mirrors POST /api/members/{id}/account.</summary>
public record IssueMemberCredentialsDto(long MemberId, string Email, string? TempPassword);

/// <summary>Member counts by status. Mirrors GET /api/members/stats.</summary>
public record MemberStatsDto(long Total, long Active, long Transferring, long Inactive);