using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Application.Interfaces;

public interface ITeamMeetingRepository
{
    Task<Team?> GetTeamAsync(long churchId, long teamId, CancellationToken ct);
    Task<bool> HasSubTeamsAsync(long churchId, long teamId, CancellationToken ct);
    Task<List<long>> GetActiveMemberIdsAsync(long churchId, long teamId, CancellationToken ct);
    Task<bool> IsLeaderAsync(long teamId, long memberId, CancellationToken ct);
    Task<TeamMeeting?> GetByIdAsync(long teamId, long meetingId, CancellationToken ct);
    Task<TeamMeeting?> GetByIdWithAttendanceAsync(long teamId, long meetingId, CancellationToken ct);
    Task<List<TeamMeeting>> ListMeetingsAsync(long churchId, long teamId, int page, int pageSize, CancellationToken ct);
    Task<int> CountMeetingsAsync(long churchId, long teamId, CancellationToken ct);
    Task<List<TeamAttendance>> GetCountsAsync(long meetingId, CancellationToken ct);
    Task<TeamMeeting> CreateAsync(TeamMeeting meeting, CancellationToken ct);
    Task DeleteAsync(TeamMeeting meeting, CancellationToken ct);
    Task<int> UpsertAttendanceAsync(
        long meetingId, long teamId, DateOnly date,
        IReadOnlyList<(long MemberId, TeamAttendanceStatus Status, string? Reason)> entries,
        CancellationToken ct);
}