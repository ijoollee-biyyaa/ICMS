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
[Route("api/churches/{churchId:long}/teams/{teamId:long}/payments")]
[Tags("Teams")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class TeamPaymentsController(
    ITeamPaymentService paymentService,
    IAuthorizationService authorizationService,
    IcmsDbContext db,
    LinkGenerator linkGenerator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TeamPaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Record a member's payment")]
    [EndpointDescription("Records a monthly payment for an active member of the team. One payment per member per month; a duplicate returns 409. Month must be the first day of the month and not in the future. Admin, a church admin of the team's church, or a leader of the team can record payments.")]
    public async Task<IActionResult> RecordPayment(
        long churchId, long teamId, RecordPaymentRequest request, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var result = await paymentService.RecordPaymentAsync(churchId, teamId, request, ct);

        return result.Match<IActionResult>(
            payment => CreatedAtAction(nameof(GetPayments), new { churchId, teamId },
                payment with { Links = BuildLinks(churchId, teamId, payment.Id) }),
            error => error.ToResult(Request));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TeamPaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List team payments")]
    [EndpointDescription("Paged list of payments for the team, filtered by month range and member search.")]
    public async Task<IActionResult> GetPayments(
        long churchId, long teamId,
        [FromQuery] DateOnly? fromMonth, [FromQuery] DateOnly? toMonth,
        [FromQuery] PagedRequest paging, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var result = await paymentService.GetPaymentsAsync(
            churchId, teamId, fromMonth, toMonth, paging, ct);

        return result.Match<IActionResult>(
            page => Ok(page),
            error => error.ToResult(Request));
    }

    [HttpPut("{paymentId:long}", Name = nameof(UpdatePayment))]
    [ProducesResponseType(typeof(TeamPaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Correct a payment amount")]
    [EndpointDescription("Updates the amount of a recorded payment. The member, month and team are fixed at creation.")]
    public async Task<IActionResult> UpdatePayment(
        long churchId, long teamId, long paymentId, UpdatePaymentRequest request, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var result = await paymentService.UpdatePaymentAsync(churchId, teamId, paymentId, request, ct);

        return result.Match<IActionResult>(
            payment => Ok(payment with { Links = BuildLinks(churchId, teamId, payment.Id) }),
            error => error.ToResult(Request));
    }

    [HttpGet("summary", Name = nameof(GetPaymentSummary))]
    [ProducesResponseType(typeof(List<TeamPaymentSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Payment report per member")]
    [EndpointDescription("Per-member totals: payment count, total amount and last paid month. The team leader's financial report. Only admin, church admins, or the team leader can view it.")]
    public async Task<IActionResult> GetPaymentSummary(
        long churchId, long teamId, CancellationToken ct)
    {
        var team = await LoadTeamAsync(churchId, teamId, ct);
        if (team is null)
            return NotFound(new ProblemDetails { Title = $"Team with id {teamId} was not found in this church." });

        var auth = await authorizationService.AuthorizeAsync(User, team, "CanManageTeam");
        if (!auth.Succeeded)
            return Forbid();

        var result = await paymentService.GetSummaryAsync(churchId, teamId, ct);

        return result.Match<IActionResult>(
            summary => Ok(summary),
            error => error.ToResult(Request));
    }

    private Task<Team?> LoadTeamAsync(long churchId, long teamId, CancellationToken ct) =>
        db.Teams.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == teamId && t.ChurchId == churchId, ct);

    private IReadOnlyList<LinkDto> BuildLinks(long churchId, long teamId, long paymentId)
    {
        var path = (string routeName, object values) =>
            linkGenerator.GetPathByName(HttpContext, routeName, values)!;

        return
        [
            new(path(nameof(UpdatePayment), new { churchId, teamId, paymentId }), "update", "PUT")
        ];
    }
}