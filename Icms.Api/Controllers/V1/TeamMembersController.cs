using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/churches/{churchId:long}/teams/{teamId:long}/members")]
[Tags("Teams")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class TeamMembersController(
    ITeamMemberService teamMemberService,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Add a member to a team")]
    [EndpointDescription("Joins a member to a sub-team or a standalone team. Main teams with sub-teams are categories and cannot hold members. The team and member must belong to the church.")]
    public async Task<IActionResult> JoinTeam(
        long churchId, long teamId, JoinTeamRequest request, CancellationToken ct)
    {
        var result = await teamMemberService.JoinTeamAsync(churchId, teamId, request, ct);

        return result.Match<IActionResult>(
            membership => CreatedAtAction(nameof(GetTeamMembers), new { churchId, teamId },
                membership with { Links = BuildLinks(churchId, teamId, membership.MemberId) }),
            error => error.ToResult(Request));
    }

    [HttpGet(Name = nameof(GetTeamMembers))]
    [ProducesResponseType(typeof(PagedResponse<TeamMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List a team's members")]
    [EndpointDescription("Paged list of team memberships with member name and EFGBC ID. Returns 404 if the team does not exist.")]
    public async Task<IActionResult> GetTeamMembers(
        long churchId, long teamId, [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var result = await teamMemberService.GetTeamMembersAsync(churchId, teamId, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpPut("{memberId:long}/role", Name = nameof(SetMemberRole))]
    [ProducesResponseType(typeof(TeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Change a member's role")]
    [EndpointDescription("Promotes a member to Leader or demotes to Member. A team can have any number of leaders.")]
    public async Task<IActionResult> SetMemberRole(
        long churchId, long teamId, long memberId, SetRoleRequest request, CancellationToken ct)
    {
        var result = await teamMemberService.SetRoleAsync(churchId, teamId, memberId, request, ct);

        return result.Match<IActionResult>(
            membership => Ok(membership with { Links = BuildLinks(churchId, teamId, membership.MemberId) }),
            error => error.ToResult(Request));
    }

    [HttpDelete("{memberId:long}", Name = nameof(RemoveMember))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Remove a member from a team")]
    [EndpointDescription("Soft-removes a member from the team.")]
    public async Task<IActionResult> RemoveMember(
        long churchId, long teamId, long memberId, CancellationToken ct)
    {
        var result = await teamMemberService.RemoveMemberAsync(churchId, teamId, memberId, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            error => error.ToResult(Request));
    }

    private IReadOnlyList<LinkDto> BuildLinks(long churchId, long teamId, long memberId)
    {
        var path = (string routeName, object values) =>
            linkGenerator.GetPathByName(HttpContext, routeName, values)!;

        return
        [
            new(path(nameof(SetMemberRole), new { churchId, teamId, memberId }), "role", "PUT"),
            new(path(nameof(RemoveMember), new { churchId, teamId, memberId }), "remove", "DELETE")
        ];
    }
}