using FluentValidation;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Application.Teams;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Services;

public class TeamPaymentService(
    ITeamPaymentRepository paymentRepository,
    RecordPaymentValidator recordValidator,
    UpdatePaymentValidator updateValidator,
    ILogger<TeamPaymentService> logger) : ITeamPaymentService
{
    public async Task<Result<TeamPaymentDto, TeamPaymentError>> RecordPaymentAsync(
        long churchId, long teamId, RecordPaymentRequest request, CancellationToken ct)
    {
        await recordValidator.ValidateAndThrowAsync(request, ct);

        var team = await paymentRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamPaymentDto, TeamPaymentError>.Failure(
                TeamPaymentError.TeamNotFound(teamId));

        if (team.ParentTeamId is null
            && await paymentRepository.HasSubTeamsAsync(churchId, teamId, ct))
            return Result<TeamPaymentDto, TeamPaymentError>.Failure(
                TeamPaymentError.TeamIsCategory(teamId));

        var activeMemberIds = (await paymentRepository.GetActiveMemberIdsAsync(churchId, teamId, ct))
            .ToHashSet();

        var memberId = request.MemberId!.Value;
        var month = request.Month!.Value;

        if (!activeMemberIds.Contains(memberId))
            return Result<TeamPaymentDto, TeamPaymentError>.Failure(
                TeamPaymentError.MemberNotInTeam(memberId, teamId));

        if (await paymentRepository.ExistsAsync(teamId, memberId, month, ct))
            return Result<TeamPaymentDto, TeamPaymentError>.Failure(
                TeamPaymentError.PaymentExists(memberId, month));

        var payment = new TeamPayment
        {
            TeamId = teamId,
            MemberId = memberId,
            Month = month,
            Amount = request.Amount!.Value,
            PaidAt = DateTimeOffset.UtcNow
        };

        try
        {
            var created = await paymentRepository.AddAsync(payment, ct);

            logger.LogInformation("Recorded payment {PaymentId} ({Amount}) for member {MemberId} in {Month}",
                created.Id, created.Amount, created.MemberId, created.Month);

            return Result<TeamPaymentDto, TeamPaymentError>.Success(ToDto(created));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<TeamPaymentDto, TeamPaymentError>.Failure(
                TeamPaymentError.PaymentExists(memberId, month));
        }
    }

    public async Task<Result<TeamPaymentDto, TeamPaymentError>> UpdatePaymentAsync(
        long churchId, long teamId, long paymentId, UpdatePaymentRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var payment = await paymentRepository.GetByIdAsync(churchId, teamId, paymentId, ct);
        if (payment is null)
            return Result<TeamPaymentDto, TeamPaymentError>.Failure(
                TeamPaymentError.PaymentNotFound(paymentId));

        payment.Amount = request.Amount!.Value;

        var updated = await paymentRepository.UpdateAsync(payment, ct);

        logger.LogInformation("Corrected payment {PaymentId} amount to {Amount}",
            updated.Id, updated.Amount);

        return Result<TeamPaymentDto, TeamPaymentError>.Success(ToDto(updated));
    }

    public async Task<Result<PagedResponse<TeamPaymentDto>, TeamPaymentError>> GetPaymentsAsync(
        long churchId, long teamId, DateOnly? fromMonth, DateOnly? toMonth,
        PagedRequest paging, CancellationToken ct)
    {
        var team = await paymentRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<PagedResponse<TeamPaymentDto>, TeamPaymentError>.Failure(
                TeamPaymentError.TeamNotFound(teamId));

        var page = paging.SafePage;

        var payments = await paymentRepository.GetByTeamAsync(
            churchId, teamId, fromMonth, toMonth, paging.Search, page, paging.PageSize, ct);
        var totalCount = await paymentRepository.CountByTeamAsync(
            churchId, teamId, fromMonth, toMonth, paging.Search, ct);

        return Result<PagedResponse<TeamPaymentDto>, TeamPaymentError>.Success(
            new PagedResponse<TeamPaymentDto>
            {
                Items = payments.Select(p => ToDto(p)).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = paging.PageSize
            });
    }

    public async Task<Result<List<TeamPaymentSummaryDto>, TeamPaymentError>> GetSummaryAsync(
        long churchId, long teamId, CancellationToken ct)
    {
        var team = await paymentRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<List<TeamPaymentSummaryDto>, TeamPaymentError>.Failure(
                TeamPaymentError.TeamNotFound(teamId));

        var rows = await paymentRepository.GetSummaryRowsAsync(churchId, teamId, ct);

        var summary = rows
            .GroupBy(p => p.MemberId)
            .Select(g => new TeamPaymentSummaryDto(
                g.Key,
                MemberNames.Full(g.First().Member),
                g.First().Member.EfgbcId,
                g.Count(),
                g.Sum(p => p.Amount),
                g.Max(p => p.Month)))
            .OrderByDescending(s => s.TotalAmount)
            .ThenBy(s => s.MemberName)
            .ToList();

        return Result<List<TeamPaymentSummaryDto>, TeamPaymentError>.Success(summary);
    }

    private static TeamPaymentDto ToDto(TeamPayment p) =>
        new(p.Id, p.MemberId, MemberNames.Full(p.Member), p.Member.EfgbcId, p.Month!.Value, p.Amount, p.PaidAt);
}