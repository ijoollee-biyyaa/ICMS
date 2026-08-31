using Icms.Domain.Entities;

namespace Icms.Application.Interfaces;

public interface ITeamMemberRepository
{
    Task<Team?> GetTeamAsync(long churchId, long teamId, CancellationToken ct);
    Task<Member?> GetMemberAsync(long memberId, CancellationToken ct);
    Task<TeamMember?> GetMembershipAsync(long churchId, long teamId, long memberId, CancellationToken ct);
    Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct);
    Task<int> CountMembershipsInFamilyAsync(long memberId, long rootTeamId, CancellationToken ct);
    Task<TeamMember> AddAsync(TeamMember membership, CancellationToken ct);
    Task<TeamMember> UpdateAsync(TeamMember membership, CancellationToken ct);
    Task<List<TeamMember>> GetByTeamPagedAsync(long churchId, long teamId, string? search, int page, int pageSize, CancellationToken ct);
    Task<int> CountByTeamAsync(long churchId, long teamId, string? search, CancellationToken ct);
}