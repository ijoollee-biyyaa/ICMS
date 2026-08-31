using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface ITeamAttendanceService
{
    Task<Result<SaveAttendanceResult, TeamAttendanceError>> SaveAttendanceAsync(
        long churchId, long teamId, SaveAttendanceRequest request, CancellationToken ct);

    Task<Result<PagedResponse<TeamAttendanceRecordDto>, TeamAttendanceError>> GetAttendanceAsync(
        long churchId, long teamId, DateOnly? from, DateOnly? to,
        PagedRequest paging, CancellationToken ct);

    Task<Result<TeamAttendanceReportDto, TeamAttendanceError>> GetSummaryAsync(
        long churchId, long teamId, DateOnly? from, DateOnly? to, CancellationToken ct);
}