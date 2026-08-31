using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Icms.Application.DTOs;
using Icms.Domain.Entities;
using Icms.Domain.Enums;
using Icms.Infrastructure.Identity;
using Icms.Infrastructure.Persistence;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/districts/{districtId:long}/churches/{churchId:long}/accounts")]
[Tags("Church Accounts")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ChurchAccountsController(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IAuthorizationService authorizationService,
    IcmsDbContext db) : ControllerBase
{
    public const string ChurchAdminRole = "ChurchAdmin";
    public const string ChurchSubAdminRole = "ChurchSubAdmin";

    [HttpGet]
    [ProducesResponseType(typeof(List<UserAccountDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List church accounts")]
    [EndpointDescription(
        "Lists the accounts that serve at this church: employees of the church plus members who were " +
        "issued login credentials. Admin or the church's admin only.")]
    public async Task<IActionResult> ListAccounts(long districtId, long churchId, CancellationToken ct)
    {
        var authResult = await AuthorizeManageChurchAsync(churchId, ct);
        if (authResult is not null)
        {
            return authResult;
        }

        var districtIdOfChurch = await db.Churches.AsNoTracking()
            .Where(c => c.Id == churchId)
            .Select(c => c.DistrictId)
            .FirstAsync(ct);

        var employeeLinks = await db.Employees.AsNoTracking()
            .Where(e => !e.IsDeleted && e.UserId != null && e.ChurchId == churchId)
            .Select(e => new { e.UserId, e.ChurchId, e.DistrictId, e.Position })
            .ToListAsync(ct);

        var employeeUserIds = employeeLinks
            .Select(e => e.UserId!)
            .Distinct()
            .ToList();

        // Members with their own credentials (User.MemberId) who are not employed at this church.
        var memberAccounts = await (
            from u in db.Users.AsNoTracking()
            join m in db.Members on u.MemberId equals m.Id
            where m.ChurchId == churchId && !m.IsDeleted && !employeeUserIds.Contains(u.Id)
            select new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.FatherName,
                u.GrandfatherName,
                u.IsAccountLocked,
                u.LockReason
            })
            .ToListAsync(ct);

        var userIds = employeeUserIds
            .Concat(memberAccounts.Select(m => m.Id))
            .Distinct()
            .ToList();

        var users = await db.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .OrderBy(u => u.FirstName)
            .ToListAsync(ct);

        var userRoles = await (
            from ur in db.UserRoles
            join r in db.Roles on ur.RoleId equals r.Id
            where userIds.Contains(ur.UserId)
            select new { ur.UserId, Role = r.Name! })
            .ToListAsync(ct);

        var accounts = users.Select(u =>
        {
            var link = employeeLinks.FirstOrDefault(e => e.UserId == u.Id);
            if (link is not null)
            {
                return new UserAccountDto(
                    u.Id, u.Email!, u.FirstName, u.FatherName, u.GrandfatherName,
                    link.Position, link.ChurchId, link.DistrictId,
                    userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.Role).ToList(),
                    u.IsAccountLocked, u.LockReason);
            }

            return new UserAccountDto(
                u.Id, u.Email!, u.FirstName, u.FatherName, u.GrandfatherName,
                null, churchId, districtIdOfChurch,
                userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.Role).ToList(),
                u.IsAccountLocked, u.LockReason, AccountKind.Member);
        }).ToList();

        return Ok(accounts);
    }

    [HttpPost("{userId}/roles", Name = nameof(ChangeChurchRole))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Grant or revoke a church role")]
    [EndpointDescription(
        $"Sets church admin or church sub-admin access for an account that serves at this church. " +
        $"Only the '{ChurchAdminRole}' and '{ChurchSubAdminRole}' roles can be granted or revoked here. " +
        "Admin or the church's admin only.")]
    public async Task<IActionResult> ChangeChurchRole(
        long districtId, long churchId, string userId,
        [FromBody] RoleChangeRequest request, CancellationToken ct)
    {
        var authResult = await AuthorizeManageChurchAsync(churchId, ct);
        if (authResult is not null)
        {
            return authResult;
        }

        if (request.Role is not ChurchAdminRole and not ChurchSubAdminRole)
        {
            return BadRequest(new ProblemDetails
            {
                Title = $"Only the '{ChurchAdminRole}' and '{ChurchSubAdminRole}' roles can be managed here."
            });
        }

        var selfId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == selfId)
        {
            return BadRequest(new ProblemDetails { Title = "You cannot change your own account." });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails { Title = "Account not found." });
        }

        var inScope = await db.Employees.AsNoTracking()
            .AnyAsync(e => !e.IsDeleted && e.UserId == user.Id && e.ChurchId == churchId, ct);

        if (!inScope)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "This account does not serve at this church."
            });
        }

        if (request.Grant)
        {
            if (!await roleManager.RoleExistsAsync(ChurchAdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(ChurchAdminRole));
            }

            await userManager.AddToRoleAsync(user, ChurchAdminRole);
        }
        else
        {
            await userManager.RemoveFromRoleAsync(user, ChurchAdminRole);
        }

        return NoContent();
    }

    [HttpPost("{userId}/lock", Name = nameof(LockAccount))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Lock a church account")]
    [EndpointDescription(
        "Locks an account that serves at this church (clearance out, death, or by the system). The account can no " +
        "longer sign in; active refresh tokens are revoked. An account with the 'Admin' role cannot be locked here. " +
        "Admin or the church's admin only.")]
    public async Task<IActionResult> LockAccount(
        long districtId, long churchId, string userId,
        [FromBody] LockAccountRequest request, CancellationToken ct)
    {
        var authResult = await AuthorizeManageChurchAsync(churchId, ct);
        if (authResult is not null)
        {
            return authResult;
        }

        if (request.Reason == AccountLockReason.None)
        {
            return BadRequest(new ProblemDetails { Title = "A lock reason is required (clearance out, death or by the system)." });
        }

        var selfId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == selfId)
        {
            return BadRequest(new ProblemDetails { Title = "You cannot lock your own account." });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails { Title = "Account not found." });
        }

        if (await userManager.IsInRoleAsync(user, "Admin"))
        {
            return BadRequest(new ProblemDetails { Title = "The 'Admin' account is protected and cannot be locked." });
        }

        var inScope = await ServesAtChurchAsync(user.Id, churchId, ct);

        if (!inScope)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "This account does not serve at this church."
            });
        }

        if (user.IsAccountLocked)
        {
            return BadRequest(new ProblemDetails { Title = "This account is already locked." });
        }

        user.IsAccountLocked = true;
        user.LockReason = request.Reason;
        user.LockedAtUtc = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        var tokens = await db.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpPost("{userId}/unlock", Name = nameof(UnlockAccount))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Unlock a church account")]
    [EndpointDescription(
        "Unlocks a previously locked account that serves at this church and clears the lock reason. " +
        "Admin or the church's admin only.")]
    public async Task<IActionResult> UnlockAccount(
        long districtId, long churchId, string userId, CancellationToken ct)
    {
        var authResult = await AuthorizeManageChurchAsync(churchId, ct);
        if (authResult is not null)
        {
            return authResult;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails { Title = "Account not found." });
        }

        var inScope = await ServesAtChurchAsync(user.Id, churchId, ct);

        if (!inScope)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "This account does not serve at this church."
            });
        }

        if (!user.IsAccountLocked)
        {
            return BadRequest(new ProblemDetails { Title = "This account is not locked." });
        }

        user.IsAccountLocked = false;
        user.LockReason = AccountLockReason.None;
        user.LockedAtUtc = null;
        await userManager.UpdateAsync(user);
        await userManager.ResetAccessFailedCountAsync(user);

        return NoContent();
    }

    /// <summary>
    /// An account serves at this church when it is linked to a live employee there, or when its
    /// owner is a live member of that church who was issued their own login credentials.
    /// </summary>
    private async Task<bool> ServesAtChurchAsync(string userId, long churchId, CancellationToken ct)
    {
        if (await db.Employees.AsNoTracking()
            .AnyAsync(e => !e.IsDeleted && e.UserId == userId && e.ChurchId == churchId, ct))
        {
            return true;
        }

        return await db.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.MemberId != null
                && db.Members.Any(m => m.Id == u.MemberId && m.ChurchId == churchId && !m.IsDeleted), ct);
    }

    private async Task<IActionResult?> AuthorizeManageChurchAsync(long churchId, CancellationToken ct)
    {
        var church = await db.Churches.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == churchId && !c.IsDeleted, ct);

        if (church == null)
        {
            return NotFound(new ProblemDetails { Title = "Church not found." });
        }

        var authResult = await authorizationService.AuthorizeAsync(User, church, "CanManageChurch");
        return authResult.Succeeded ? null : Forbid();
    }
}