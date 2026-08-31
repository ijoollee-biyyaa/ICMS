using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/members/{memberId:long}/dashboard")]
[Tags("Members")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class MemberDashboardController(IMemberDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(MemberDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Member's own dashboard")]
    [EndpointDescription("A member's personal view: every active team membership with their own attendance summary (meetings, present/late/absent, rate) and payment summary (count, total, last month). Only the member's own records.")]
    public async Task<IActionResult> GetDashboard(long memberId, CancellationToken ct)
    {
        // Members can only see their own record; staff can inspect any member's view.
        if (User.IsInRole("Member"))
        {
            var claim = User.FindFirst("MemberId")?.Value;
            if (claim is null
                || !long.TryParse(claim, out var ownMemberId)
                || ownMemberId != memberId)
            {
                return Forbid();
            }
        }

        var result = await dashboardService.GetDashboardAsync(memberId, ct);

        return result.Match<IActionResult>(
            dashboard => Ok(dashboard),
            error => error.ToResult(Request));
    }
}