using Icms.Domain.Entities;

namespace Icms.Application.Interfaces;

public interface ITeamPaymentRepository
{
    Task<Team?> GetTeamAsync(long churchId, long teamId, CancellationToken ct);
    Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct);
    Task<List<long>> GetActiveMemberIdsAsync(long churchId, long teamId, CancellationToken ct);
    Task<bool> ExistsAsync(long teamId, long memberId, DateOnly month, CancellationToken ct);
    Task<TeamPayment?> GetByIdAsync(long churchId, long teamId, long paymentId, CancellationToken ct);
    Task<TeamPayment> AddAsync(TeamPayment payment, CancellationToken ct);
    Task<TeamPayment> UpdateAsync(TeamPayment payment, CancellationToken ct);
    Task<List<TeamPayment>> GetByTeamAsync(long churchId, long teamId,
        DateOnly? fromMonth, DateOnly? toMonth, string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountByTeamAsync(long churchId, long teamId,
        DateOnly? fromMonth, DateOnly? toMonth, string? search, CancellationToken ct);
    Task<List<TeamPayment>> GetSummaryRowsAsync(long churchId, long teamId, CancellationToken ct);
}