using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Icms.Domain.Entities;
using Icms.Infrastructure.Persistence;

namespace Icms.Api.Authorization;

public class ManageChurchHandler : AuthorizationHandler<ManageChurchRequirement>
{
    private readonly IcmsDbContext _db;

    public ManageChurchHandler(IcmsDbContext db)
    {
        _db = db;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageChurchRequirement requirement)
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

        long churchId = 0;

        if (context.Resource is Church church)
        {
            churchId = church.Id;
        }
        else if (context.Resource is HttpContext httpContext)
        {
            if (httpContext.Request.RouteValues.TryGetValue("churchId", out var routeValue) &&
                long.TryParse(routeValue?.ToString(), out var parsedId))
            {
                churchId = parsedId;
            }
        }
        else if (context.Resource is Microsoft.AspNetCore.Http.Endpoint)
        {
            if (context.Resource is HttpContext hc && hc.Request.RouteValues.TryGetValue("churchId", out var rv) &&
                long.TryParse(rv?.ToString(), out var pid))
            {
                churchId = pid;
            }
            // in .NET 6+ endpoint routing, the resource is HttpContext.
        }

        if (churchId == 0)
        {
            // Try getting from HttpContext accessor if available, or just fail
            if (context.Resource is DefaultHttpContext defaultContext)
            {
                if (defaultContext.Request.RouteValues.TryGetValue("churchId", out var routeValue) &&
                    long.TryParse(routeValue?.ToString(), out var parsedId))
                {
                    churchId = parsedId;
                }
            }
        }

        if (churchId == 0) return;

        var servesAtChurch = await _db.Employees
            .AnyAsync(e =>
                !e.IsDeleted
                && e.UserId == userId
                && e.ChurchId == churchId);

        if (servesAtChurch)
        {
            context.Succeed(requirement);
        }
    }
}