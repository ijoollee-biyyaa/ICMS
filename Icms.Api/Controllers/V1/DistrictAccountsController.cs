using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Icms.Application.DTOs;
using Icms.Infrastructure.Identity;
using Icms.Infrastructure.Persistence;

namespace Icms.Api.Controllers.V1;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/districts/{districtId:long}/accounts")]
[Tags("District Accounts")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class DistrictAccountsController(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IcmsDbContext db) : ControllerBase
{
    public const string DistrictSubAdminRole = "DistrictSubAdmin";

    [HttpGet]
    [ProducesResponseType(typeof(List<UserAccountDto>), StatusCodes.Status200OK)]
    [EndpointSummary("List district accounts")]
    [EndpointDescription(
        "Lists all accounts that serve in the district: district employees plus employees of the district's churches. " +
        "Super admin only.")]
    public async Task<IActionResult> ListAccounts(long districtId, CancellationToken ct)
    {
        var districtExists = await db.Districts.AsNoTracking()
            .AnyAsync(d => d.Id == districtId && !d.IsDeleted, ct);
        if (!districtExists)
        {
            return NotFound(new ProblemDetails { Title = "District not found." });
        }

        var churchIds = db.Churches.AsNoTracking()
            .Where(c => c.DistrictId == districtId && !c.IsDeleted)
            .Select(c => c.Id);

        var employeeLinks = await db.Employees.AsNoTracking()
            .Where(e => !e.IsDeleted
                && e.UserId != null
                && (e.DistrictId == districtId
                    || (e.ChurchId != null && churchIds.Contains(e.ChurchId.Value))))
            .Select(e => new { e.UserId, e.ChurchId, e.DistrictId, e.Position })
            .ToListAsync(ct);

        var userIds = employeeLinks
            .Select(e => e.UserId!)
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
            var link = employeeLinks.First(e => e.UserId == u.Id);
            return new UserAccountDto(
                u.Id, u.Email!, u.FirstName, u.FatherName, u.GrandfatherName,
                link.Position, link.ChurchId, link.DistrictId,
                userRoles.Where(ur => ur.UserId == u.Id).Select(ur => ur.Role).ToList(),
                u.IsAccountLocked, u.LockReason);
        }).ToList();

        return Ok(accounts);
    }

    [HttpPost("{userId}/roles", Name = nameof(ChangeDistrictRole))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Grant or revoke a district role")]
    [EndpointDescription(
        $"Sets district access for an account that serves in the district. Only the '{DistrictSubAdminRole}' role " +
        $"can be granted or revoked here; the Admin role is protected. Super admin only.")]
    public async Task<IActionResult> ChangeDistrictRole(
        long districtId, string userId, [FromBody] RoleChangeRequest request, CancellationToken ct)
    {
        if (request.Role != DistrictSubAdminRole)
        {
            return BadRequest(new ProblemDetails
            {
                Title = $"Only the '{DistrictSubAdminRole}' role can be managed here."
            });
        }

        if (userId == User.Identity?.Name)
        {
            return BadRequest(new ProblemDetails { Title = "You cannot change your own account." });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails { Title = "Account not found." });
        }

        var churchIds = db.Churches.AsNoTracking()
            .Where(c => c.DistrictId == districtId && !c.IsDeleted)
            .Select(c => c.Id);

        var inScope = await db.Employees.AsNoTracking()
            .AnyAsync(e => !e.IsDeleted
                && e.UserId == user.Id
                && (e.DistrictId == districtId
                    || (e.ChurchId != null && churchIds.Contains(e.ChurchId.Value))), ct);

        if (!inScope)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "This account does not serve in the district."
            });
        }

        if (request.Grant)
        {
            if (!await roleManager.RoleExistsAsync(DistrictSubAdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(DistrictSubAdminRole));
            }

            await userManager.AddToRoleAsync(user, DistrictSubAdminRole);
        }
        else
        {
            await userManager.RemoveFromRoleAsync(user, DistrictSubAdminRole);
        }

        return NoContent();
    }
}