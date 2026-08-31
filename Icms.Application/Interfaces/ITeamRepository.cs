using Icms.Domain.Entities;

namespace Icms.Application.Interfaces;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(long churchId, long id, CancellationToken ct);
    Task<Church?> GetChurchAsync(long churchId, CancellationToken ct);
    Task<bool> NameExistsAsync(long churchId, string name, CancellationToken ct);
    Task<Team> AddAsync(Team team, CancellationToken ct);
    Task<Team> UpdateAsync(Team team, CancellationToken ct);
    Task<bool> HasMembersAsync(long churchId, long teamId, CancellationToken ct);
    Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct);
    Task<int> CountMembersAsync(long churchId, long teamId, CancellationToken ct);
    Task<int> CountSubTeamsAsync(long churchId, long teamId, CancellationToken ct);
    Task<List<Team>> GetByChurchPagedAsync(long churchId, string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountByChurchAsync(long churchId, string? search, CancellationToken ct);
}