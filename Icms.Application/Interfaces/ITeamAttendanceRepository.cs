using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Application.Interfaces;

public interface ITeamAttendanceRepository
{
    Task<Team?> GetTeamAsync(long churchId, long teamId, CancellationToken ct);
    Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct);
    Task<List<long>> GetActiveMemberIdsAsync(long churchId, long teamId, CancellationToken ct);
    Task<int> UpsertAsync(long teamId, DateOnly date,
        IReadOnlyList<(long MemberId, TeamAttendanceStatus Status, string? Reason)> entries, CancellationToken ct);
    Task<List<TeamAttendance>> GetByTeamAsync(long churchId, long teamId,
        DateOnly? from, DateOnly? to, string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountByTeamAsync(long churchId, long teamId,
        DateOnly? from, DateOnly? to, string? search, CancellationToken ct);
    Task<List<TeamAttendance>> GetSummaryRowsAsync(long churchId, long teamId,
        DateOnly? from, DateOnly? to, CancellationToken ct);
}