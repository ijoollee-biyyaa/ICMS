using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/churches/{churchId:long}/teams/{teamId:long}/attendance")]
[Tags("Teams")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class TeamAttendanceController(
    ITeamAttendanceService attendanceService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TeamAttendanceRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List attendance records")]
    [EndpointDescription("Paged list of attendance records for the team, filtered by date range and member name.")]
    public async Task<IActionResult> GetAttendance(
        long churchId, long teamId,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var result = await attendanceService.GetAttendanceAsync(
            churchId, teamId, from, to, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpGet("summary", Name = nameof(GetAttendanceSummary))]
    [ProducesResponseType(typeof(TeamAttendanceReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Attendance report per member")]
    [EndpointDescription("Team-level totals and per-member breakdown (present/late/absent/rate) over the date range. The team leader's monthly report.")]
    public async Task<IActionResult> GetAttendanceSummary(
        long churchId, long teamId,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var result = await attendanceService.GetSummaryAsync(churchId, teamId, from, to, ct);

        return result.Match<IActionResult>(
            report => Ok(report),
            error => error.ToResult(Request));
    }
}