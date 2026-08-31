using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface ITeamService
{
    Task<Result<TeamResponseDto, TeamError>> CreateTeamAsync(
        long churchId, CreateTeamRequest request, CancellationToken ct);

    Task<Result<TeamResponseDto, TeamError>> GetTeamByIdAsync(
        long churchId, long id, CancellationToken ct);

    Task<Result<TeamDetailDto, TeamError>> GetTeamDetailAsync(
        long churchId, long id, CancellationToken ct);

    Task<Result<TeamResponseDto, TeamError>> UpdateTeamAsync(
        long churchId, long id, UpdateTeamRequest request, CancellationToken ct);

    Task<Result<TeamResponseDto, TeamError>> DeleteTeamAsync(
        long churchId, long id, CancellationToken ct);

    Task<Result<PagedResponse<TeamResponseDto>, TeamError>> GetTeamsAsync(
        long churchId, PagedRequest paging, CancellationToken ct);
}