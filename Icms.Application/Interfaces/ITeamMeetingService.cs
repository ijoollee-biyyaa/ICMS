using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface ITeamMeetingService
{
    Task<Result<TeamMeetingDto, TeamMeetingError>> CreateMeetingAsync(
        long churchId, long teamId, CreateMeetingRequest request, long createdById, CancellationToken ct);

    Task<Result<PagedResponse<TeamMeetingDto>, TeamMeetingError>> GetMeetingsAsync(
        long churchId, long teamId, PagedRequest paging, CancellationToken ct);

    Task<Result<TeamMeetingDetailDto, TeamMeetingError>> GetMeetingAsync(
        long churchId, long teamId, long meetingId, CancellationToken ct);

    Task<Result<TeamMeetingDetailDto, TeamMeetingError>> SaveAttendanceAsync(
        long churchId, long teamId, long meetingId, SaveAttendanceRequest request, CancellationToken ct);

    Task<Result<TeamMeetingDto, TeamMeetingError>> DeleteMeetingAsync(
        long churchId, long teamId, long meetingId, CancellationToken ct);
}