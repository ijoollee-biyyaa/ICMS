using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Icms.Domain.Entities;
using Icms.Infrastructure.Persistence;

namespace Icms.Api.Authorization;

public class ManageChurchHandler : AuthorizationHandler<ManageChurchRequirement, Church>
{
    private readonly IcmsDbContext _db;

    public ManageChurchHandler(IcmsDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageChurchRequirement requirement,
        Church resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = context.User.IsInRole("Admin");

        if (isAdmin)
        {
            context.Succeed(requirement);
            return;
        }

        if (userId == null || !context.User.IsInRole("ChurchAdmin"))
        {
            return;
        }

        var servesAtChurch = await _db.Employees
            .AnyAsync(e =>
                !e.IsDeleted
                && e.UserId == userId
                && e.ChurchId == resource.Id);

        if (servesAtChurch)
        {
            context.Succeed(requirement);
        }
    }
}