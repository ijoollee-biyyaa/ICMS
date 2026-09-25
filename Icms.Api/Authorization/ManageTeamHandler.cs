using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Icms.Domain.Entities;
using Icms.Domain.Enums;
using Icms.Infrastructure.Persistence;

namespace Icms.Api.Authorization;

public class ManageTeamHandler : AuthorizationHandler<ManageTeamRequirement, Team>
{
    private readonly IcmsDbContext _db;

    public ManageTeamHandler(IcmsDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageTeamRequirement requirement,
        Team resource)
    {
        var isAdmin = context.User.IsInRole("Admin");

        if (isAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (context.User.IsInRole("ChurchAdmin"))
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var servesAtChurch = await _db.Employees
                    .AnyAsync(e =>
                        !e.IsDeleted
                        && e.UserId == userId
                        && e.ChurchId == resource.ChurchId);

                if (servesAtChurch)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }

        var memberIdClaim = context.User.FindFirstValue("MemberId");
        if (memberIdClaim != null && long.TryParse(memberIdClaim, out var memberId))
        {
            var isLeader = await _db.TeamMembers
                .AnyAsync(tm =>
                    tm.TeamId == resource.Id
                    && tm.MemberId == memberId
                    && tm.Role == TeamMemberRole.Leader
                    && !tm.IsDeleted);

            if (isLeader)
            {
                context.Succeed(requirement);
            }
        }
    }
}