
using Microsoft.AspNetCore.Identity;

using Icms.Domain.Enums;

namespace Icms.Infrastructure.Identity;

public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string GrandfatherName { get; set; } = string.Empty;

    /// <summary>The member this login belongs to, when the account is a member profile.</summary>
    public long? MemberId { get; set; }

    /// <summary>Whether the account was locked administratively (clearance out, death) or by the system.</summary>
    public bool IsAccountLocked { get; set; }

    /// <summary>Why the account is locked. None when not locked.</summary>
    public AccountLockReason LockReason { get; set; }

    /// <summary>When the account was locked.</summary>
    public DateTime? LockedAtUtc { get; set; }
}