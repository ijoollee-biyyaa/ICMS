using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface ITeamPaymentService
{
    Task<Result<TeamPaymentDto, TeamPaymentError>> RecordPaymentAsync(
        long churchId, long teamId, RecordPaymentRequest request, CancellationToken ct);

    Task<Result<TeamPaymentDto, TeamPaymentError>> UpdatePaymentAsync(
        long churchId, long teamId, long paymentId, UpdatePaymentRequest request, CancellationToken ct);

    Task<Result<PagedResponse<TeamPaymentDto>, TeamPaymentError>> GetPaymentsAsync(
        long churchId, long teamId, DateOnly? fromMonth, DateOnly? toMonth,
        PagedRequest paging, CancellationToken ct);

    Task<Result<List<TeamPaymentSummaryDto>, TeamPaymentError>> GetSummaryAsync(
        long churchId, long teamId, CancellationToken ct);
}