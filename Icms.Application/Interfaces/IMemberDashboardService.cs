using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface IMemberDashboardService
{
    Task<Result<MemberDashboardDto, MemberDashboardError>> GetDashboardAsync(
        long memberId, CancellationToken ct);

    Task<Result<MemberHistoryDto, MemberDashboardError>> GetHistoryAsync(
        long memberId, CancellationToken ct);
}