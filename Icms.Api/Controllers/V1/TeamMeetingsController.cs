using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Infrastructure.Persistence;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/churches/{churchId:long}/teams/{teamId:long}/meetings")]
[Tags("Teams")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class TeamMeetingsController(
    ITeamMeetingService meetingService,
    IAuthorizationService authorizationService,
    IcmsDbContext db) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TeamMeetingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a team meeting")]
    [EndpointDescription("Creates a meeting record for the team. Admin, a church admin of the team's church, or a leader of the team can create meetings. The meeting date is required and cannot be in the future.")]
    public async Task<IActionResult> CreateMeeting(
        long churchId, long teamId, CreateMeetingRequest request, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var createdById = GetMemberId();
        var result = await meetingService.CreateMeetingAsync(churchId, teamId, request, createdById, ct);

        return result.Match<IActionResult>(
            meeting => CreatedAtAction(nameof(GetMeeting), new { churchId, teamId, meetingId = meeting.Id }, meeting),
            error => error.ToResult(Request));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TeamMeetingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List team meetings")]
    [EndpointDescription("Paged list of meetings for the team, newest first, with per-meeting attendance counts (present/late/absent/marked).")]
    public async Task<IActionResult> GetMeetings(
        long churchId, long teamId, [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var result = await meetingService.GetMeetingsAsync(churchId, teamId, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpGet("{meetingId:long}", Name = nameof(GetMeeting))]
    [ProducesResponseType(typeof(TeamMeetingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a team meeting")]
    [EndpointDescription("Returns the meeting with its recorded attendance per member.")]
    public async Task<IActionResult> GetMeeting(
        long churchId, long teamId, long meetingId, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var result = await meetingService.GetMeetingAsync(churchId, teamId, meetingId, ct);

        return result.Match<IActionResult>(
            meeting => Ok(meeting),
            error => error.ToResult(Request));
    }

    [HttpPost("{meetingId:long}/attendance", Name = nameof(SaveMeetingAttendance))]
    [ProducesResponseType(typeof(TeamMeetingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Record attendance for a meeting")]
    [EndpointDescription("Marks every listed member as present/late/absent for the meeting. Re-saving the same meeting corrects previous marks. Only admin, church admin, or the team leader can record attendance.")]
    public async Task<IActionResult> SaveMeetingAttendance(
        long churchId, long teamId, long meetingId, SaveAttendanceRequest request, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var result = await meetingService.SaveAttendanceAsync(churchId, teamId, meetingId, request, ct);

        return result.Match<IActionResult>(
            meeting => Ok(meeting),
            error => error.ToResult(Request));
    }

    [HttpDelete("{meetingId:long}", Name = nameof(DeleteMeeting))]
    [ProducesResponseType(typeof(TeamMeetingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Delete a team meeting")]
    [EndpointDescription("Deletes the meeting together with ALL of its attendance. This cannot be undone.")]
    public async Task<IActionResult> DeleteMeeting(
        long churchId, long teamId, long meetingId, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var result = await meetingService.DeleteMeetingAsync(churchId, teamId, meetingId, ct);

        return result.Match<IActionResult>(
            meeting => Ok(meeting),
            error => error.ToResult(Request));
    }

    private Task<Team?> LoadTeamAsync(long churchId, long teamId, CancellationToken ct) =>
        db.Teams.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == teamId && t.ChurchId == churchId, ct);

    private long GetMemberId()
    {
        var claim = User.FindFirst("MemberId")?.Value;
        return claim is not null && long.TryParse(claim, out var memberId) ? memberId : 0;
    }
}