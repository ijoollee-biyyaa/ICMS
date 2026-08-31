using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface ITeamMemberService
{
    Task<Result<TeamMemberDto, TeamMemberError>> JoinTeamAsync(
        long churchId, long teamId, JoinTeamRequest request, CancellationToken ct);

    Task<Result<PagedResponse<TeamMemberDto>, TeamMemberError>> GetTeamMembersAsync(
        long churchId, long teamId, PagedRequest paging, CancellationToken ct);

    Task<Result<TeamMemberDto, TeamMemberError>> SetRoleAsync(
        long churchId, long teamId, long memberId, SetRoleRequest request, CancellationToken ct);

    Task<Result<TeamMemberDto, TeamMemberError>> RemoveMemberAsync(
        long churchId, long teamId, long memberId, CancellationToken ct);
}