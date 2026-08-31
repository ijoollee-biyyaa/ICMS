using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Icms.Domain.Entities;
using Icms.Infrastructure.Persistence;

namespace Icms.Api.Authorization;

public class ManageDistrictHandler : AuthorizationHandler<ManageDistrictRequirement, District>
{
    private readonly IcmsDbContext _db;

    public ManageDistrictHandler(IcmsDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageDistrictRequirement requirement,
        District resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = context.User.IsInRole("Admin");

        if (isAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (userId == null)
        {
            return;
        }

        var isDistrictPresident = await _db.Employees
            .AnyAsync(e =>
                e.IsDistrictPresident
                && !e.IsDeleted
                && e.UserId == userId
                && e.Church != null
                && e.Church.DistrictId == resource.Id);

        if (isDistrictPresident)
        {
            context.Succeed(requirement);
        }
    }
}