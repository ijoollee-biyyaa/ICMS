using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Enums;
using System.Security.Claims;

namespace Icms.Api.Controllers.V1;

[ApiController]
[Route("api/v1/churches/{churchId}/transfers")]
[Authorize]
public class TransferController(ITransferService transferService) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("outgoing")]
    [Authorize(Policy = "CanManageChurch")]
    public async Task<IActionResult> InitiateOutgoing(
        long churchId,
        [FromBody] InitiateTransferRequest request,
        CancellationToken ct)
    {
        var result = await transferService.InitiateOutgoingAsync(churchId, request, UserId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }

    [HttpPost("{transferId}/accept")]
    [Authorize(Policy = "CanManageChurch")]
    public async Task<IActionResult> AcceptIncoming(
        long churchId,
        long transferId,
        [FromBody] AcceptTransferRequest request,
        CancellationToken ct)
    {
        var result = await transferService.AcceptIncomingAsync(transferId, churchId, request, UserId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }

    [HttpPost("external-incoming")]
    [Authorize(Policy = "CanManageChurch")]
    public async Task<IActionResult> RegisterExternalIncoming(
        long churchId,
        [FromBody] RegisterExternalIncomingRequest request,
        CancellationToken ct)
    {
        if (request.DestinationChurchId != churchId)
            return BadRequest(new { Error = "URL church ID does not match request body" });

        var result = await transferService.RegisterExternalIncomingAsync(request, UserId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }

    [HttpPost("{transferId}/void")]
    [Authorize(Policy = "CanManageChurch")]
    public async Task<IActionResult> VoidTransfer(
        long churchId,
        long transferId,
        [FromBody] string reason,
        CancellationToken ct)
    {
        var result = await transferService.VoidTransferAsync(transferId, churchId, reason, UserId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }

    [HttpGet("{transferId}")]
    public async Task<IActionResult> GetById(
        long churchId,
        long transferId,
        CancellationToken ct)
    {
        var result = await transferService.GetByIdAsync(transferId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        long churchId,
        [FromQuery] TransferDirection? direction,
        [FromQuery] TransferStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        long? sourceChurchId = direction == null || direction == TransferDirection.Outgoing ? churchId : null;
        long? destChurchId = direction == null || direction == TransferDirection.Incoming ? churchId : null;
        
        var paging = new PagedRequest { Page = page, PageSize = pageSize, Search = search };
        
        // Pass null for direction so the repository only filters by the computed source/destination IDs
        var result = await transferService.GetPagedAsync(sourceChurchId, destChurchId, null, status, search, paging, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        long churchId,
        CancellationToken ct)
    {
        var result = await transferService.GetStatsAsync(churchId, null, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }
    [HttpGet("rejoin-candidates")]
    public async Task<IActionResult> SearchRejoinCandidates(
        long churchId,
        [FromQuery] string search,
        CancellationToken ct)
    {
        var result = await transferService.SearchRejoinCandidatesAsync(churchId, search, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }

    [HttpPost("rejoin")]
    public async Task<IActionResult> RejoinMember(
        long churchId,
        [FromBody] RejoinTransferRequest request,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var result = await transferService.RejoinMemberAsync(churchId, request, userId, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error.ToResult(Request);
    }
}
