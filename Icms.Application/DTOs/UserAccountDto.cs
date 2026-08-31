using Icms.Domain.Enums;

namespace Icms.Application.DTOs;

public enum AccountKind
{
    Employee,
    Member
}

public record UserAccountDto(
    string UserId,
    string Email,
    string FirstName,
    string FatherName,
    string GrandfatherName,
    string? Position,
    long? ChurchId,
    long? DistrictId,
    IReadOnlyList<string> Roles,
    bool IsAccountLocked,
    AccountLockReason LockReason,
    AccountKind Kind = AccountKind.Employee);

public record RoleChangeRequest(string Role, bool Grant);

public record LockAccountRequest(AccountLockReason Reason);