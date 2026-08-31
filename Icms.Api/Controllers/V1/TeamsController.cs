using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;

namespace Icms.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/churches/{churchId:long}/teams")]
[Tags("Teams")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class TeamsController(
    ITeamService teamService,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TeamResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a team")]
    [EndpointDescription("Creates a main team (a card, optionally with sub-teams) or a sub-team under a main team. The membership rule decides whether members can join one or multiple sub-teams of the card. Team names are unique per church, case-insensitively.")]
    public async Task<IActionResult> CreateTeam(
        long churchId, CreateTeamRequest request, CancellationToken ct)
    {
        var result = await teamService.CreateTeamAsync(churchId, request, ct);

        return result.Match<IActionResult>(
            team => CreatedAtAction(nameof(GetTeamById), new { churchId, id = team.Id },
                team with { Links = BuildLinks(churchId, team.Id) }),
            error => error.ToResult(Request));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TeamResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List teams")]
    [EndpointDescription("Paged list of teams of a church. Returns 404 if the church does not exist.")]
    public async Task<IActionResult> GetTeams(
        long churchId, [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var result = await teamService.GetTeamsAsync(churchId, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpGet("{id:long}", Name = nameof(GetTeamById))]
    [ProducesResponseType(typeof(TeamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get team by ID")]
    [EndpointDescription("Returns the team with its sub-team and member counts. The team is looked up within the church.")]
    public async Task<IActionResult> GetTeamById(long churchId, long id, CancellationToken ct)
    {
        var result = await teamService.GetTeamDetailAsync(churchId, id, ct);

        return result.Match<IActionResult>(
            team => Ok(team with { Links = BuildLinks(churchId, team.Id) }),
            error => error.ToResult(Request));
    }

    [HttpPut("{id:long}", Name = nameof(UpdateTeam))]
    [ProducesResponseType(typeof(TeamResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Update a team")]
    [EndpointDescription("Updates the name of a team. Parent membership and the membership rule are fixed at creation.")]
    public async Task<IActionResult> UpdateTeam(
        long churchId, long id, UpdateTeamRequest request, CancellationToken ct)
    {
        var result = await teamService.UpdateTeamAsync(churchId, id, request, ct);

        return result.Match<IActionResult>(
            team => Ok(team with { Links = BuildLinks(churchId, team.Id) }),
            error => error.ToResult(Request));
    }

    [HttpDelete("{id:long}", Name = nameof(DeleteTeam))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Delete a team")]
    [EndpointDescription("Soft-deletes a team. Returns 409 if the team has sub-teams or members.")]
    public async Task<IActionResult> DeleteTeam(long churchId, long id, CancellationToken ct)
    {
        var result = await teamService.DeleteTeamAsync(churchId, id, ct);

        return result.Match<IActionResult>(
            _ => NoContent(),
            error => error.ToResult(Request));
    }

    private IReadOnlyList<LinkDto> BuildLinks(long churchId, long id)
    {
        var path = (string routeName, object values) =>
            linkGenerator.GetPathByName(HttpContext, routeName, values)!;

        return
        [
            new(path(nameof(GetTeamById), new { churchId, id }), "self", "GET"),
            new(path(nameof(UpdateTeam), new { churchId, id }), "update", "PUT"),
            new(path(nameof(DeleteTeam), new { churchId, id }), "delete", "DELETE")
        ];
    }
}