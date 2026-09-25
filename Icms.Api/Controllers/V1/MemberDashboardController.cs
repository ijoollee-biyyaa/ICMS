using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/members/{memberId:long}")]
[Tags("Members")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class MemberDashboardController(IMemberDashboardService dashboardService) : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(MemberDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Member's own dashboard")]
    [EndpointDescription("A member's personal view: every active team membership with their own attendance summary (meetings, present/late/absent, rate) and payment summary (count, total, last month). Only the member's own records.")]
    public async Task<IActionResult> GetDashboard(long memberId, CancellationToken ct)
    {
        if (!CanView(memberId))
            return Forbid();

        var result = await dashboardService.GetDashboardAsync(memberId, ct);

        return result.Match<IActionResult>(
            dashboard => Ok(dashboard),
            error => error.ToResult(Request));
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(MemberHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Member's own attendance & payment history")]
    [EndpointDescription("Every attendance record and payment the member has across all their teams, newest first. Only the member's own records.")]
    public async Task<IActionResult> GetHistory(long memberId, CancellationToken ct)
    {
        if (!CanView(memberId))
            return Forbid();

        var result = await dashboardService.GetHistoryAsync(memberId, ct);

        return result.Match<IActionResult>(
            history => Ok(history),
            error => error.ToResult(Request));
    }

    // Members can only see their own record; staff can inspect any member's view.
    private bool CanView(long memberId)
    {
        if (!User.IsInRole("Member"))
            return true;

        var claim = User.FindFirst("MemberId")?.Value;
        return claim is not null
            && long.TryParse(claim, out var ownMemberId)
            && ownMemberId == memberId;
    }
}